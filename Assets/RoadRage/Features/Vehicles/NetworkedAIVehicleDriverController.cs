using System.Collections.Generic;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Controleur de conduite IA basique (Story 5.2) : poursuite deterministe des noeuds du
    /// <see cref="LaneGraph"/> assigne, calculee et appliquee host-only (IsServer), position
    /// repliquee vers les clients par le NetworkTransform existant -- aucune RPC de mouvement.
    /// Reutilise tel quel le predicat retournement/hors-zone de
    /// <see cref="NetworkedVehicleDriverController"/> (IsRolledOver / IsBelowVoidHeightThreshold) et
    /// applique la meme detection "soutenue N secondes" pour le blocage (vitesse quasi nulle). Toute
    /// recuperation (retournement, hors-zone ou blocage) reinitialise le vehicule au noeud courant
    /// -- jamais au WaypointIndex d'un autre vehicule : chaque instance ne porte que son
    /// propre etat (aucune collection statique/partagee), donc independante par construction.
    /// Desactivation/isolation (AC epic 5) : ce comportement passe entierement par FixedUpdate, donc
    /// decocher le composant (ou son GameObject) dans MVP_Run suffit a arreter le vehicule IA sans
    /// toucher au vehicule joueur -- pas de toggle applicatif dedie necessaire.
    ///
    /// Story 5.4 : l'hote derive le comportement du vehicule depuis sa propre rage
    /// (<see cref="IRageDispositionSource"/> sur le meme GameObject, jamais celle d'un autre vehicule
    /// ni une jauge globale) et le publie dans <see cref="NetworkedAIVehicleState.Behavior"/>.
    ///
    /// Story 5.9 : le style de conduite n'est plus un multiplicateur de vitesse cable ici. Ce
    /// composant est devenu un integrateur -- il lit un <see cref="DriverProfileDef"/> authore, le
    /// module par la disposition publiee (<see cref="DriverModel.ResolveEffectiveProfile"/>), detecte
    /// un leader devant lui, integre l'acceleration IDM lissee par le temps de reaction du profil, et
    /// cadence l'evaluation de changement de voie sur l'intervalle du profil avec une phase propre a
    /// l'instance. Toutes les decisions vivent dans <see cref="DriverModel"/> : aucune valeur de
    /// conduite n'est litterale dans ce fichier.
    ///
    /// Story 5.10 : la boucle de waypoints est remplacee par un graphe de voies authore. A chaque
    /// jonction le successeur sort d'un tirage de virage pondere par les ratios authores sur le noeud
    /// (modele jtrrouter), deterministe pour un meme vehicule car graine sur son NetworkObjectId.
    /// L'arrivee a un portail de sortie est SIGNALEE (<see cref="HasReachedExitPortal"/>) mais ne
    /// retire rien : le retrait appartient au seul spawner hote, et uniquement pour ce motif (AD-34).
    /// Toutes les decisions de parcours vivent dans <see cref="LaneGraphRouting"/>.
    ///
    /// Correctif post-livraison du 2026-09-16 : le parcours est une MARCHE AUTO-EVITANTE. Le noeud
    /// d'insertion puis chaque noeud atteint sont marques comme parcourus, et le tirage de virage ne
    /// porte que sur les successeurs non parcourus -- un vehicule IA ne parcourt jamais deux fois le
    /// meme noeud de voie. La consequence de forme est voulue : un tour complet de giratoire devient
    /// impossible (le "continuer" qui ramenerait au noeud d'entree de l'anneau n'est plus eligible a
    /// la derniere branche) et aucun circuit autour d'une jonction carree n'est atteignable. Quand
    /// plus aucun successeur n'est eligible, la reorientation gloutonne vers la sortie la plus proche
    /// reprend la main -- elle peut re-accepter un noeud parcouru, c'est l'echappatoire, bornee par le
    /// budget d'aretes.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedAIVehicleState))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NetworkedAIVehicleDriverController : NetworkBehaviour
    {
        /// <summary>
        /// Marge dimensionnelle (sans unite) appliquee a la portee de detection de leader, deja
        /// derivee du profil (s0 + v.T). Provisoire : la Story 5.17 (ex-5.12) remplace cette detection avant
        /// minimale par une perception elargie.
        /// </summary>
        private const float LeaderDetectionRangeFactor = 2f;

        /// <summary>Fraction de la demi-largeur du vehicule utilisee comme rayon de balayage : sous 1 pour ne pas mordre sur ce qui borde la voie.</summary>
        private const float ScanWidthFactor = 0.8f;

        /// <summary>Part du cap vise dans la direction de balayage (0 = nez seul, 1 = cap seul).</summary>
        private const float ScanSteerBlend = 0.5f;

        [SerializeField]
        [Tooltip("Graphe de voies (LaneGraph) parcouru par ce vehicule. Aucun graphe assigne ou graphe vide : le vehicule reste immobile.")]
        private LaneGraph laneGraph;

        [SerializeField]
        [Tooltip("Profil de conduite authore (Story 5.9). Non assigne : le vehicule reste inerte, comme quand la route est absente -- aucun repli numerique en dur.")]
        private DriverProfileDef driverProfile;

        [SerializeField]
        [Min(0.01f)]
        private float arrivalRadius = 3f;

        [SerializeField]
        [Min(1f)]
        private float steerFullLockDegrees = 45f;

        [SerializeField]
        [Min(0f)]
        private float steerDegreesPerSecond = 90f;

        [SerializeField]
        [Range(-1f, 1f)]
        private float rolloverUprightDotThreshold = 0.35f;

        [SerializeField]
        [Min(0f)]
        private float rolloverSustainedSeconds = 2f;

        [SerializeField]
        private float voidHeightThreshold = -10f;

        [SerializeField]
        [Min(0f)]
        private float stuckSpeedThreshold = 0.5f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Duree d'immobilisation SANS leader devant soi avant le palier de recuperation. Une minute entiere : un arret voulu derriere un leader ne l'alimente jamais (DriverModel.IsDeliberateStop), seul un encastrement reel y arrive.")]
        private float stuckSustainedSeconds = 60f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Rayon en dessous duquel la presence d'un joueur interdit la teleportation de recuperation : elle ne doit jamais se produire sous les yeux de quelqu'un.")]
        private float recoveryPlayerClearanceRadius = 40f;

        // Tampons NonAlloc propres a l'instance, jamais partages entre vehicules. Tailles marginees
        // tres au-dela de la densite actuelle de MVP_Run (9 colliders au total dans la scene) : sans
        // marge, un depassement ne plante pas mais tronque silencieusement les resultats -- soit un
        // leader plus proche non vu (pas de freinage), soit un joueur proche non vu (teleportation
        // visible malgre le garde-fou). Les Stories 5.10 (district greybox) et 5.16 / 5.22 (ex-5.17, ~30
        // vehicules) rapprocheront la densite reelle de ces marges ; un avertissement de saturation
        // rend tout depassement futur bruyant plutot que silencieux.
        private readonly RaycastHit[] leaderHits = new RaycastHit[32];
        private readonly Collider[] clearanceHits = new Collider[48];

        private NetworkedAIVehicleState state;
        private Rigidbody body;
        private NetworkTransform networkTransform;
        private IRageDispositionSource rageSource;
        private float rolloverElapsedSeconds;
        private float stuckElapsedSeconds;
        private float currentSpeed;
        private float appliedAcceleration;
        private float laneChangeElapsedSeconds;
        private float instancePhase;
        private readonly List<Vector3> redirectCandidatePositions = new List<Vector3>();

        /// <summary>
        /// Memoire de parcours (correctif post-livraison du 2026-09-16) : un bit par noeud du graphe,
        /// propre a CE vehicule. C'est elle qui rend la marche auto-evitative -- le tirage ne porte
        /// jamais sur un noeud deja parcouru par ce vehicule.
        ///
        /// ponytail: un tableau de bits dimensionne sur NodeCount suffit -- ni HashSet, ni index
        /// spatial, ni identifiant supplementaire. La memoire est remise a zero aux deux seuls moments
        /// qui remettent le vehicule a un point de depart connu : l'insertion et la recuperation sur
        /// place.
        /// </summary>
        private bool[] traversedNodes;

        /// <summary>Masque d'eligibilite reutilise d'un noeud au suivant : le sous-ensemble du tirage vient de l'appelant (le driver), la ponderation reste celle du noeud.</summary>
        private readonly List<bool> eligibleSuccessors = new List<bool>();

        private int traversedEdges;
        private bool reachedExitPortal;
        private bool hasDepartedSpawnNode;
        private bool warnedDeadEnd;
        private bool warnedNoReachableExit;
        private bool warnedMissingDriverProfile;
        private bool warnedLeaderBufferSaturated;
        private bool warnedClearanceBufferSaturated;
        private float frontOffset;
        private float scanRadius;

        /// <summary>
        /// Vrai des que ce vehicule a atteint un noeud de portail de sortie. C'est le SEUL signal de
        /// retrait du trafic (AD-34) : ni compteur, ni distance au joueur, ni echec de trajet, ni
        /// capot retourne ne le levent. Lu par le spawner hote, qui detient le despawn -- ce
        /// composant, lui, ne detruit jamais rien.
        /// </summary>
        public bool HasReachedExitPortal
        {
            get { return reachedExitPortal; }
        }

        /// <summary>
        /// Cablage du graphe a l'insertion : un prefab ne peut pas porter une reference de scene, donc
        /// le spawner hote la pose juste avant le spawn reseau. Un vehicule pose en scene garde la
        /// reference authoree dans l'Inspector et n'a jamais besoin de cet appel.
        /// </summary>
        public void BindLaneGraph(LaneGraph graph)
        {
            laneGraph = graph;
        }

        private void Awake()
        {
            CacheComponents();
            CacheVehicleExtents();
        }

        public override void OnNetworkSpawn()
        {
            CacheComponents();
            CacheVehicleExtents();

            if (body != null)
            {
                body.isKinematic = !IsServer;
            }

            // Depart host-only sur le noeud le plus proche : un vehicule insere a un portail y
            // demarre sans index a cabler, et un vehicule pose en scene reprend le graphe ou il est.
            if (IsServer && state != null && laneGraph != null && laneGraph.NodeCount > 0)
            {
                var nearest = laneGraph.NearestNodeIndex(transform.position);
                state.WaypointIndex.Value = nearest < 0 ? 0 : nearest;
                traversedEdges = 0;
                reachedExitPortal = false;
                hasDepartedSpawnNode = false;

                // Remise a zero de la memoire de parcours, puis marquage du noeud d'insertion : le
                // vehicule repart d'une marche auto-evitative vierge et ne reviendra jamais sur son
                // propre noeud de naissance.
                ResetRouteMemoryAt(state.WaypointIndex.Value);
            }

            // Phase deterministe de l'instance, dans [0, 1[ : elle desynchronise le minuteur de
            // changement de voie et le bruit de vitesse desiree. Elle vient de l'identifiant reseau
            // de l'objet -- stable, distinct par instance et attribue par l'hote -- jamais d'un
            // tirage aleatoire, donc deux vehicules crees a la meme frame n'evaluent pas sur la
            // meme frame et le comportement reste reproductible.
            instancePhase = (NetworkObjectId % 1000UL) / 1000f;

            if (driverProfile != null)
            {
                laneChangeElapsedSeconds = instancePhase * driverProfile.Profile.LaneChangeEvaluationInterval;
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer || body == null || state == null)
            {
                return;
            }

            // Derivation host-only (Story 5.4) : la rage propre au vehicule devient son comportement
            // publie. Ecriture seulement sur changement, pour ne pas re-emettre a chaque tick.
            var behavior = ResolveBehavior();
            if (state.Behavior.Value != behavior)
            {
                state.Behavior.Value = behavior;
            }

            if (laneGraph == null || laneGraph.NodeCount <= 0)
            {
                return;
            }

            // Profil absent : vehicule inerte et avertissement emis une seule fois. Aucun repli de
            // valeurs code en dur -- ce serait exactement le reglage cable que la story supprime.
            if (driverProfile == null)
            {
                if (!warnedMissingDriverProfile)
                {
                    warnedMissingDriverProfile = true;
                    Debug.LogWarning("[Vehicles] " + name + " : aucun DriverProfileDef assigne, le vehicule IA reste inerte.", this);
                }

                return;
            }

            var fixedDeltaTime = Time.fixedDeltaTime;
            var profile = DriverModel.ResolveEffectiveProfile(driverProfile.Profile, behavior);
            var waypointIndex = state.WaypointIndex.Value;
            var waypointPosition = laneGraph.GetNodePosition(waypointIndex);

            if (NetworkedVehicleDriverController.IsBelowVoidHeightThreshold(transform.position.y, voidHeightThreshold))
            {
                RecoverAtWaypoint(waypointPosition);
                return;
            }

            if (NetworkedVehicleDriverController.IsRolledOver(transform.up, rolloverUprightDotThreshold))
            {
                rolloverElapsedSeconds += fixedDeltaTime;
                if (rolloverElapsedSeconds >= rolloverSustainedSeconds)
                {
                    RecoverAtWaypoint(waypointPosition);
                    return;
                }
            }
            else
            {
                rolloverElapsedSeconds = 0f;
            }

            // Block / ConfrontationCapable : vitesse desiree effective nulle, le vehicule cesse de
            // poursuivre la route. Place apres les recuperations retournement/hors-zone (qui restent
            // des garde-fous) mais avant la detection de blocage : une immobilisation voulue ne doit
            // pas declencher une teleportation "stuck".
            if (profile.DesiredSpeed <= 0f)
            {
                stuckElapsedSeconds = 0f;
                currentSpeed = 0f;
                appliedAcceleration = 0f;
                ApplyMovement(VehicleDriveIntent.Idle, fixedDeltaTime, 0f);
                return;
            }

            // Detection hissee avant le test de blocage : le meme resultat sert a decider si l'arret
            // est voulu ET a nourrir l'IDM plus bas -- un seul rayon par frame, comme avant.
            var hasLeader = TryDetectLeader(profile, out var leaderGap, out var leaderSpeed);

            var planarSpeed = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
            if (DriverModel.IsDeliberateStop(hasLeader, leaderGap, profile.MinimumGap))
            {
                // Arret voulu derriere un leader : ce n'est pas un blocage. Sans cette remise a zero,
                // un vehicule qui freine correctement (donc qui fait son travail) finirait teleporte,
                // ce que la contrainte d'epique interdit explicitement.
                stuckElapsedSeconds = 0f;
            }
            else if (IsStuck(planarSpeed, stuckSpeedThreshold))
            {
                stuckElapsedSeconds += fixedDeltaTime;

                // Palier de derniere chance, et seulement hors de vue : un vehicule reellement
                // encastre dans le decor, sans rien devant lui, pendant une minute entiere. La
                // teleportation reste interdite tant qu'un joueur est assez proche pour la voir --
                // le vehicule patiente alors sans reinitialiser son compteur, et part des que la
                // zone se degage. L'echelle de deblocage propre (klaxon, contournement) arrive avec
                // les Stories 5.18 et 5.17 (ex-5.11 / ex-5.12) et retirera ce palier.
                if (stuckElapsedSeconds >= stuckSustainedSeconds && !IsAnyPlayerWithinClearanceRadius())
                {
                    RecoverAtWaypoint(waypointPosition);
                    return;
                }
            }
            else
            {
                stuckElapsedSeconds = 0f;
            }

            // Deux facons de franchir un noeud, et elles sont complementaires (ANO-5.10-02) :
            // l'arrivee nominale (on entre dans le rayon), et le depassement irrattrapable (le
            // noeud est passe ET tombe dans le cercle de braquage, donc la poursuite pure tournerait
            // autour de lui indefiniment sans jamais entrer dans le rayon). Sans la seconde, un
            // vehicule bouscule pres d'un giratoire reste en orbite pour toujours : son noeud
            // courant ne change plus, donc le parcours ne progresse plus.
            if (HasArrivedAtWaypoint(transform.position, waypointPosition, arrivalRadius)
                || LaneGraphRouting.HasPassedUnreachableWaypoint(
                    transform.position, transform.forward, waypointPosition, currentSpeed, steerDegreesPerSecond))
            {
                waypointIndex = ResolveNextNode(waypointIndex);
                state.WaypointIndex.Value = waypointIndex;
                waypointPosition = laneGraph.GetNodePosition(waypointIndex);
            }

            IntegrateLongitudinalSpeed(profile, fixedDeltaTime, hasLeader, leaderGap, leaderSpeed);
            TickLaneChangeEvaluation(profile, fixedDeltaTime);

            var intent = ComputeSeekIntent(transform.position, transform.forward, waypointPosition, arrivalRadius, steerFullLockDegrees);
            ApplyMovement(intent, fixedDeltaTime, currentSpeed);
        }

        /// <summary>
        /// Point de branchement du parcours (Story 5.10) : a l'arrivee sur un noeud, le successeur
        /// sort d'un tirage de virage pondere par les ratios authores sur CE noeud, jamais d'un
        /// itineraire pre-calcule par vehicule. Trois issues de secours, dont aucune ne retire le
        /// vehicule : portail de sortie atteint (signale, le spawner decide), budget d'aretes depasse
        /// ou cul-de-sac (reorientation gloutonne vers la sortie la plus proche), graphe sans sortie
        /// (le vehicule reste sur place, inerte).
        /// </summary>
        private int ResolveNextNode(int currentIndex)
        {
            // Le vehicule nait exactement SUR son noeud de portail d'entree (le spawner l'y pose a
            // distance 0) : le tout premier appel arrive ici avec currentIndex == ce meme noeud, avant
            // la moindre arete parcourue. Pour un portail marque "sortie reutilisant l'entree",
            // IsExitPortal(currentIndex) serait alors vrai des la naissance -- le vehicule se ferait
            // redespawner sans avoir roule. hasDepartedSpawnNode distingue "je viens de naitre ici"
            // de "j'y reviens apres avoir effectivement circule" : seul le second cas honore la
            // sortie. Un vrai retour ulterieur au meme noeud (boucle du graphe) continue de fonctionner,
            // puisque le drapeau reste vrai une fois pose.
            if (laneGraph.IsExitPortal(currentIndex) && hasDepartedSpawnNode)
            {
                reachedExitPortal = true;
                return currentIndex;
            }

            var candidates = laneGraph.GetSuccessors(currentIndex);
            traversedEdges++;
            hasDepartedSpawnNode = true;

            var budgetExceeded = LaneGraphRouting.IsEdgeBudgetExceeded(
                traversedEdges, laneGraph.NodeCount, ResolveEdgeBudgetFactor());

            if (candidates.Count > 0 && !budgetExceeded)
            {
                var eligible = BuildEligibleSuccessorMask(candidates);

                var drawn = LaneGraphRouting.SelectWeightedSuccessor(
                    candidates,
                    laneGraph.GetTurnWeights(currentIndex),
                    eligible,
                    NetworkObjectId,
                    traversedEdges,
                    out var weightsInvalid);

                if (drawn >= 0)
                {
                    if (weightsInvalid)
                    {
                        laneGraph.ReportInvalidTurnWeights(currentIndex);
                    }

                    MarkNodeTraversed(drawn);
                    return drawn;
                }

                // Tous les successeurs non parcourus ont ete epuises a ce noeud : la marche
                // auto-evitative s'arrete ici. La reorientation gloutonne vers la sortie la plus
                // proche reprend la main -- c'est l'echappatoire voulue, bornee par le budget
                // d'aretes, et le vehicule n'est jamais retire.
            }

            return ResolveRedirectToNearestExit(currentIndex, candidates);
        }

        /// <summary>
        /// Memoire de parcours : remise a zero puis marquage du noeud courant. Appelee aux deux seuls
        /// moments qui remettent le vehicule a un point de depart connu -- l'insertion et la
        /// recuperation sur place -- et jamais ailleurs : ailleurs, la memoire doit survivre au
        /// parcours, c'est tout l'objet de la regle de non-bouclage.
        /// </summary>
        private void ResetRouteMemoryAt(int nodeIndex)
        {
            EnsureRouteMemory();

            if (traversedNodes != null)
            {
                System.Array.Clear(traversedNodes, 0, traversedNodes.Length);
            }

            MarkNodeTraversed(nodeIndex);
        }

        /// <summary>
        /// Dimensionne la memoire sur le graphe. Un trace ne change pas a l'execution -- un Rebuild de
        /// graphe est un cas d'authoring, pas de parcours : aucune reallocation ne vient donc effacer
        /// silencieusement la memoire en cours de route.
        /// </summary>
        private void EnsureRouteMemory()
        {
            var nodeCount = laneGraph != null ? laneGraph.NodeCount : 0;
            if (traversedNodes != null && traversedNodes.Length == nodeCount)
            {
                return;
            }

            traversedNodes = nodeCount > 0 ? new bool[nodeCount] : null;
        }

        private void MarkNodeTraversed(int nodeIndex)
        {
            if (traversedNodes == null || nodeIndex < 0 || nodeIndex >= traversedNodes.Length)
            {
                return;
            }

            traversedNodes[nodeIndex] = true;
        }

        private bool IsNodeTraversed(int nodeIndex)
        {
            return traversedNodes != null && nodeIndex >= 0 && nodeIndex < traversedNodes.Length && traversedNodes[nodeIndex];
        }

        /// <summary>
        /// Sous-ensemble ELIGIBLE du tirage : les successeurs que ce vehicule n'a pas encore
        /// parcourus. C'est le driver qui fournit le sous-ensemble, et <see cref="LaneGraphRouting"/> qui
        /// reste pondere par les ratios authores sur le noeud ; l'absence de candidat eligible rend
        /// -1, ce que l'appelant traite par la reorientation gloutonne.
        /// </summary>
        private IReadOnlyList<bool> BuildEligibleSuccessorMask(IReadOnlyList<int> candidates)
        {
            EnsureRouteMemory();

            eligibleSuccessors.Clear();
            for (var i = 0; i < candidates.Count; i++)
            {
                eligibleSuccessors.Add(!IsNodeTraversed(candidates[i]));
            }

            return eligibleSuccessors;
        }

        /// <summary>
        /// Budget d'aretes authore, ou 0 (= pas de budget) quand aucun reglage de trafic n'est
        /// assigne : un budget invente serait exactement la valeur en dur que la story supprime.
        /// </summary>
        private float ResolveEdgeBudgetFactor()
        {
            var settings = laneGraph.TrafficSettings;
            return settings == null ? 0f : settings.EdgeBudgetFactor;
        }

        /// <summary>
        /// Reorientation vers le portail de sortie le plus proche, un pas a la fois : parmi les
        /// successeurs du noeud courant, celui qui rapproche le plus de ce portail. Sans successeur
        /// (cul-de-sac), la sortie devient directement la cible. Sans aucun portail de sortie dans le
        /// graphe, le vehicule garde son noeud courant et devient inerte -- il n'est jamais retire.
        /// </summary>
        private int ResolveRedirectToNearestExit(int currentIndex, IReadOnlyList<int> candidates)
        {
            var exitIndex = laneGraph.NearestExitNodeIndex(transform.position);
            if (exitIndex < 0)
            {
                if (!warnedNoReachableExit)
                {
                    warnedNoReachableExit = true;
                    Debug.LogWarning("[Vehicles] " + name
                        + " : le graphe de voies ne porte aucun portail de sortie, le vehicule reste inerte (jamais retire).", this);
                }

                return currentIndex;
            }

            if (candidates.Count == 0)
            {
                if (!warnedDeadEnd)
                {
                    warnedDeadEnd = true;
                    Debug.LogWarning("[Vehicles] " + name
                        + " : cul-de-sac dans le graphe de voies, reorientation vers le portail de sortie le plus proche.", this);
                }

                return exitIndex;
            }

            redirectCandidatePositions.Clear();
            for (var i = 0; i < candidates.Count; i++)
            {
                redirectCandidatePositions.Add(laneGraph.GetNodePosition(candidates[i]));
            }

            return LaneGraphRouting.SelectSuccessorTowardTarget(
                candidates, redirectCandidatePositions, laneGraph.GetNodePosition(exitIndex));
        }

        /// <summary>
        /// Comportement courant : la disposition de rage de CE vehicule, Calm par repli quand aucun
        /// <see cref="IRageDispositionSource"/> n'est present (composant absent ou objet non spawne).
        /// </summary>
        private RageDisposition ResolveBehavior()
        {
            return rageSource == null ? RageDisposition.Calm : rageSource.CurrentDisposition;
        }

        /// <summary>
        /// Integration (Story 5.9) : le modele fournit une acceleration, le controleur maintient la
        /// vitesse longitudinale de son instance. Ordre de composition impose par la spec --
        /// disposition (deja appliquee par l'appelant) puis bruit de personnalite sur la seule
        /// vitesse desiree, juste avant le calcul d'acceleration.
        /// </summary>
        private void IntegrateLongitudinalSpeed(
            DriverProfile profile,
            float fixedDeltaTime,
            bool hasLeader,
            float detectedGap,
            float detectedLeaderSpeed)
        {
            var noisyDesiredSpeed = DriverModel.ResolveNoisyDesiredSpeed(
                profile.DesiredSpeed, profile.Consistency, Time.time, instancePhase);

            var gap = hasLeader ? detectedGap : DriverModel.NoLeaderGap;
            var leaderSpeed = hasLeader ? detectedLeaderSpeed : 0f;

            var targetAcceleration = DriverModel.ComputeAcceleration(
                profile.WithDesiredSpeed(noisyDesiredSpeed), currentSpeed, leaderSpeed, gap);

            appliedAcceleration = DriverModel.SmoothAcceleration(
                appliedAcceleration, targetAcceleration, profile.ReactionTime, fixedDeltaTime);

            currentSpeed = Mathf.Max(0f, currentSpeed + (appliedAcceleration * fixedDeltaTime));
        }

        /// <summary>
        /// Detection de leader avant minimale, explicitement provisoire : un seul rayon devant le
        /// vehicule, portee derivee du profil (s0 + v.T). La Story 5.17 (ex-5.12) la remplace par la perception
        /// elargie ; aucun index spatial ni collection partagee n'est introduit ici.
        /// </summary>
        /// <summary>
        /// Direction du balayage : moitie nez, moitie cap vise. En ligne droite les deux coincident ;
        /// en virage, viser uniquement le nez fait manquer ce qui se trouve en sortie de courbe.
        /// </summary>
        private Vector3 ResolveScanDirection()
        {
            var forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();

            if (laneGraph == null || laneGraph.NodeCount <= 0 || state == null)
            {
                return forward;
            }

            var toWaypoint = laneGraph.GetNodePosition(state.WaypointIndex.Value) - transform.position;
            toWaypoint.y = 0f;
            if (toWaypoint.sqrMagnitude <= 0.0001f)
            {
                return forward;
            }

            return Vector3.Slerp(forward, toWaypoint.normalized, ScanSteerBlend).normalized;
        }

        /// <summary>
        /// Demi-longueur avant et demi-largeur du vehicule, lues une fois sur son propre collider.
        /// La longueur sert a mesurer l'ecart depuis le pare-chocs ; la largeur dimensionne le
        /// balayage volumique. Sans collider exploitable, on retombe sur un balayage au centre de
        /// masse avec un rayon nul -- l'ancien comportement, jamais une valeur inventee.
        /// </summary>
        private void CacheVehicleExtents()
        {
            frontOffset = 0f;
            scanRadius = 0f;

            var box = GetComponent<BoxCollider>();
            if (box == null)
            {
                return;
            }

            var scale = transform.lossyScale;
            frontOffset = Mathf.Max(0f, (box.center.z + (box.size.z * 0.5f)) * Mathf.Abs(scale.z));
            scanRadius = Mathf.Max(0f, box.size.x * 0.5f * Mathf.Abs(scale.x) * ScanWidthFactor);
        }

        private bool TryDetectLeader(DriverProfile profile, out float gap, out float leaderSpeed)
        {
            gap = DriverModel.NoLeaderGap;
            leaderSpeed = 0f;

            var range = (profile.MinimumGap + (currentSpeed * profile.TimeHeadway)) * LeaderDetectionRangeFactor;
            if (range <= 0f)
            {
                return false;
            }

            // Direction du BALAYAGE, pas du nez. Sur un virage la trajectoire s'ecarte de
            // transform.forward : viser a mi-chemin entre le nez et le cap vise rend l'obstacle en
            // sortie de courbe visible, alors qu'un rayon strictement droit le manque. La perception
            // reellement geometrique (arc authore) reste la Story 5.17 (ex-5.12).
            var forward = ResolveScanDirection();

            // Depart au PARE-CHOCS, pas au centre de masse : sinon hit.distance inclut la propre
            // demi-longueur du vehicule (2,22 m sur le greybox) et l'equilibre de l'IDM a s0 tombe
            // A L'INTERIEUR du leader -- les vehicules se collent et se poussent au lieu de garder
            // l'ecart authore. L'ecart rendu ici est bien pare-chocs a pare-chocs, comme l'exige
            // la definition de s dans l'IDM.
            //
            // Le centre de la sphere est recule d'un rayon pour que son bord AVANT parte du
            // pare-chocs : hit.distance mesure le trajet du centre, donc sans ce recul l'ecart
            // serait sous-estime d'exactement un rayon. Le centre demarre alors dans le collider du
            // vehicule lui-meme, ce qui est sans effet puisque ses propres hits sont filtres.
            var origin = body.worldCenterOfMass + (forward * (frontOffset - scanRadius));

            // Balayage volumique plutot qu'un rayon d'epaisseur nulle : un rayon central ne voit pas
            // un obstacle decale d'un demi-vehicule. Le rayon de la sphere reste sous la demi-largeur
            // pour ne pas mordre sur ce qui borde la voie.
            var hitCount = Physics.SphereCastNonAlloc(
                origin, scanRadius, forward, leaderHits, range, ~0, QueryTriggerInteraction.Ignore);

            if (hitCount >= leaderHits.Length && !warnedLeaderBufferSaturated)
            {
                warnedLeaderBufferSaturated = true;
                Debug.LogWarning("[Vehicles] " + name + " : tampon de detection de leader sature (" + leaderHits.Length
                    + "), le leader reel a pu etre manque. Agrandir leaderHits.", this);
            }

            var found = false;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = leaderHits[i];

                // Un leader est un obstacle MOBILE devant soi, pas le decor. Deux representations
                // coexistent dans le projet et comptent toutes les deux : le Rigidbody (vehicules
                // IA et voiture joueur) et le CharacterController (joueur a pied, cf.
                // LocalOnFootController). Ne retenir que le Rigidbody rendrait l'IA aveugle aux
                // pietons : elle ne freinerait pas, elle les encastrerait. La geometrie statique du
                // decor ne porte ni l'un ni l'autre et reste exclue. Ses propres colliders aussi.
                if (hit.rigidbody == body)
                {
                    continue;
                }

                float hitSpeed;
                if (hit.rigidbody != null)
                {
                    hitSpeed = Vector3.Dot(hit.rigidbody.linearVelocity, forward);
                }
                else
                {
                    var walker = hit.collider == null ? null : hit.collider.GetComponentInParent<CharacterController>();
                    if (walker == null)
                    {
                        continue;
                    }

                    hitSpeed = Vector3.Dot(walker.velocity, forward);
                }

                if (!found || hit.distance < gap)
                {
                    found = true;
                    gap = hit.distance;
                    leaderSpeed = hitSpeed;
                }
            }

            return found;
        }

        /// <summary>
        /// Vrai si un joueur se trouve assez pres pour voir une teleportation de recuperation. Requete
        /// de proximite ponctuelle -- elle ne tourne qu'au moment de decider la teleportation, donc au
        /// plus une fois par minute et par vehicule, jamais a chaque frame : aucun balayage de scene,
        /// aucun index partage.
        ///
        /// Un joueur, c'est soit un CharacterController (a pied), soit un Rigidbody qui n'est pas un
        /// vehicule IA (voiture joueur). Les autres vehicules IA ne comptent pas : ils ne regardent
        /// personne.
        /// </summary>
        private bool IsAnyPlayerWithinClearanceRadius()
        {
            var found = Physics.OverlapSphereNonAlloc(
                body.position, recoveryPlayerClearanceRadius, clearanceHits, ~0, QueryTriggerInteraction.Ignore);

            if (found >= clearanceHits.Length && !warnedClearanceBufferSaturated)
            {
                warnedClearanceBufferSaturated = true;
                Debug.LogWarning("[Vehicles] " + name + " : tampon de proximite joueur sature (" + clearanceHits.Length
                    + "), un joueur proche a pu etre manque. Agrandir clearanceHits.", this);
            }

            for (var i = 0; i < found; i++)
            {
                var candidate = clearanceHits[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.GetComponentInParent<CharacterController>() != null)
                {
                    return true;
                }

                var hitBody = candidate.attachedRigidbody;
                if (hitBody != null && hitBody != body && hitBody.GetComponent<NetworkedAIVehicleState>() == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Minuteur de changement de voie cadence par l'intervalle authore du profil, avec la phase
        /// initiale propre a l'instance posee au spawn. Aucune voie candidate n'est proposee tant que
        /// une story dediee n'a pas livre les voies paralleles du graphe : le predicat MOBIL
        /// (<see cref="DriverModel.TryEvaluateLaneChange"/>) est en place et se branchera sur la liste
        /// de candidats qu'elle fournira. La Story 5.10 n'authore qu'une voie par sens.
        /// </summary>
        private void TickLaneChangeEvaluation(DriverProfile profile, float fixedDeltaTime)
        {
            laneChangeElapsedSeconds += fixedDeltaTime;

            if (DriverModel.ShouldEvaluateLaneChange(laneChangeElapsedSeconds, profile.LaneChangeEvaluationInterval))
            {
                laneChangeElapsedSeconds = 0f;
            }
        }

        /// <summary>
        /// Predicat pur (Story 5.2) : intent de poursuite deterministe vers le waypoint -- plein gaz
        /// et direction bornee par steerFullLockDegrees tant que le waypoint est hors du rayon
        /// d'arrivee, sinon Idle (l'appelant avance alors WaypointIndex avant de rappeler avec le
        /// waypoint suivant).
        /// </summary>
        public static VehicleDriveIntent ComputeSeekIntent(Vector3 position, Vector3 forward, Vector3 waypointPosition, float arrivalRadius, float steerFullLockDegrees)
        {
            var toWaypoint = waypointPosition - position;
            toWaypoint.y = 0f;

            if (toWaypoint.sqrMagnitude <= arrivalRadius * arrivalRadius)
            {
                return VehicleDriveIntent.Idle;
            }

            var flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude <= 0.0001f)
            {
                flatForward = Vector3.forward;
            }

            flatForward.Normalize();

            var direction = toWaypoint.normalized;
            var signedAngle = Vector3.SignedAngle(flatForward, direction, Vector3.up);
            var steerLock = Mathf.Max(steerFullLockDegrees, 0.0001f);
            var steer = Mathf.Clamp(signedAngle / steerLock, -1f, 1f);

            return new VehicleDriveIntent(1f, steer, 0f);
        }

        /// <summary>Predicat pur (Story 5.2) : arrivee des lors que la distance planaire au waypoint passe sous le rayon d'arrivee.</summary>
        public static bool HasArrivedAtWaypoint(Vector3 position, Vector3 waypointPosition, float arrivalRadius)
        {
            var toWaypoint = waypointPosition - position;
            toWaypoint.y = 0f;
            return toWaypoint.sqrMagnitude <= arrivalRadius * arrivalRadius;
        }

        /// <summary>Predicat pur (Story 5.2) : vitesse planaire quasi nulle -- meme esprit instantane que IsRolledOver, accumule en FixedUpdate.</summary>
        public static bool IsStuck(float planarSpeed, float stuckSpeedThreshold)
        {
            return planarSpeed <= stuckSpeedThreshold;
        }

        private void ApplyMovement(VehicleDriveIntent intent, float fixedDeltaTime, float longitudinalSpeed)
        {
            // La composante verticale est preservee comme dans NetworkedVehicleDriverController : la
            // conduite IA ne pilote que le plan horizontal. L'ecraser annulerait la gravite -- le
            // vehicule levite et ne peut plus jamais franchir le seuil de vide, ce qui rendrait la
            // recuperation hors-zone inatteignable.
            var verticalVelocity = Vector3.up * body.linearVelocity.y;

            if (intent.IsIdle)
            {
                body.linearVelocity = verticalVelocity;
                return;
            }

            var yawDegrees = intent.Steer * steerDegreesPerSecond * fixedDeltaTime;
            var rotation = Quaternion.AngleAxis(yawDegrees, Vector3.up) * body.rotation;
            body.MoveRotation(rotation);

            var forward = rotation * Vector3.forward;
            body.linearVelocity = (forward * longitudinalSpeed * intent.Throttle) + verticalVelocity;
        }

        /// <summary>
        /// Recuperation (Story 5.2) : reinitialise position/rotation/vitesse au waypoint courant --
        /// jamais un repere de scene fixe (il n'y en a pas pour l'IA) -- et remet le vehicule a
        /// l'endroit (roll corrige), sans avancer WaypointIndex ni emettre de RPC.
        /// </summary>
        private void RecoverAtWaypoint(Vector3 waypointPosition)
        {
            var rotation = ResolveUprightRecoveryRotation();

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = waypointPosition;
            body.rotation = rotation;
            transform.SetPositionAndRotation(waypointPosition, rotation);
            rolloverElapsedSeconds = 0f;
            stuckElapsedSeconds = 0f;
            currentSpeed = 0f;
            appliedAcceleration = 0f;

            // Memoire de parcours remise a zero (correctif post-livraison du 2026-09-16) : la
            // recuperation sur place repose le vehicule sur son noeud courant, donc elle rouvre la
            // marche auto-evitative exactement comme une insertion. C'est le second et dernier moment
            // ou la memoire est effacee -- ailleurs elle doit survivre au parcours. La recuperation
            // reste un repositionnement, jamais un retrait (AD-34).
            ResetRouteMemoryAt(state != null ? state.WaypointIndex.Value : -1);

            if (networkTransform != null)
            {
                networkTransform.Teleport(waypointPosition, rotation, transform.localScale);
            }
        }

        private Quaternion ResolveUprightRecoveryRotation()
        {
            var flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (flatForward.sqrMagnitude <= 0.0001f)
            {
                flatForward = Vector3.forward;
            }

            return Quaternion.LookRotation(flatForward.normalized, Vector3.up);
        }

        private void CacheComponents()
        {
            if (state == null)
            {
                state = GetComponent<NetworkedAIVehicleState>();
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }

            if (networkTransform == null)
            {
                networkTransform = GetComponent<NetworkTransform>();
            }

            // Sa propre rage uniquement : GetComponent sur ce GameObject, jamais une recherche de
            // scene. Absente (composant non ajoute a ce vehicule) => repli Calm, sans exception.
            if (rageSource == null)
            {
                rageSource = GetComponent<IRageDispositionSource>();
            }

        }
    }
}
