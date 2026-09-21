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
    /// composant lit un <see cref="DriverProfileDef"/> authore, le module par la disposition publiee
    /// (<see cref="DriverModel.ResolveEffectiveProfile"/>), detecte un leader devant lui, derive
    /// l'acceleration IDM lissee par le temps de reaction du profil, et cadence l'evaluation de
    /// changement de voie sur l'intervalle du profil avec une phase propre a l'instance. Toutes les
    /// decisions vivent dans <see cref="DriverModel"/> : aucune valeur de conduite n'est litterale
    /// dans ce fichier.
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
    /// budget d'aretes.    ///
    /// Story 5.14 : ce controleur cesse d'ECRIRE le mouvement. Il emet un
    /// <see cref="VehicleDriveIntent"/> (direction, accelerateur, frein) et le soumet a
    /// <see cref="VehiclePhysicsBody.ApplyDriveIntent"/> -- la MEME couche que le vehicule joueur
    /// (AD-35), par un chemin unique et sans jumeau local. Sa vitesse n'est plus une entree entretenue
    /// en boucle ouverte : elle est RELUE du <c>Rigidbody</c> a chaque pas (projection planaire).
    /// Consequences voulues : une IA peut etre poussee (ce que la physique a gagne pendant un contact
    /// n'est plus efface au pas suivant), la gravite redevient effective (plus de decollage sur le
    /// relief de recette), et les aides arcade et la reduction d'autorite de la Story 5.13
    /// s'appliquent au trafic par construction. Le suivi de route, la visee anticipee et la
    /// reorientation des Stories 5.10 a 5.13 restent inchanges : seule change la maniere de traduire
    /// une decision deja prise en mouvement du vehicule.
    ///
    /// <see cref="RecoverAtWaypoint"/> reste le seul chemin qui repositionne le vehicule. Ce n'est pas
    /// de la conduite, et la reaction au choc comme la recuperation physique vers une voie valide
    /// restent hors du perimetre de cette story.    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedAIVehicleState))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed partial class NetworkedAIVehicleDriverController : NetworkBehaviour
    {
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
        [Min(0f)]
        [Tooltip("Duree de visee (s) : la distance du point anticipe vaut vitesse x cette duree. A l'arret la visee retombe sur le noeud lui-meme, et c'est voulu -- un vehicule immobile n'a pas de trajectoire a anticiper.")]
        private float lookAheadSeconds = 1f;

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
        private readonly Collider[] perceptionHits = new Collider[64];
        private readonly TrafficPerceptionCandidate[] perceptionCandidates = new TrafficPerceptionCandidate[64];
        private readonly Collider[] clearanceHits = new Collider[48];

        private NetworkedAIVehicleState state;
        private Rigidbody body;
        private NetworkTransform networkTransform;
        private VehiclePhysicsBody physicsBody;
        private IRageDispositionSource rageSource;
        private float rolloverElapsedSeconds;
        private float stuckElapsedSeconds;
        private float blockedElapsedSeconds;
        private float perceptionElapsedSeconds;
        private bool hasPerceivedLeader;
        private float perceivedLeaderGap = DriverModel.NoLeaderGap;
        private float perceivedLeaderSpeed;
        private TrafficUnblockingAction currentUnblockingAction;
        private bool hornReported;
        private bool replanAttemptedForBlock;

        /// <summary>
        /// Filtre de REACTION du conducteur (Story 5.9) : l'acceleration lissee qui a produit la pedale
        /// du pas -- c'est elle, et non plus une vitesse, qui est conservee d'un pas a l'autre. La
        /// vitesse, elle, n'existe plus comme champ de ce controleur (Story 5.14) : elle est relue du
        /// <c>Rigidbody</c> a chaque pas. Le filtre reste parce que
        /// <see cref="DriverModel.SmoothAcceleration"/> est un lissage a memoire, et que le supprimer
        /// supprimerait le temps de reaction du profil -- c'est-a-dire une decision.
        /// </summary>
        private float appliedAcceleration;

        /// <summary>
        /// Pedale pleine echelle de CE vehicule, derivee de son profil physique (Story 5.14) : couple
        /// moteur total des roues motrices rapporte a la masse et au rayon de roue, soit l'acceleration
        /// que la couche physique produit a plein gaz et a l'arret. C'est l'echelle qui permet de
        /// traduire une acceleration DEMANDEE par le modele de conduite (m/s2) en position de pedale
        /// ([0, 1]) sans multiplier l'intention du conducteur par la capacite du moteur.
        ///
        /// Elle est DERIVEE, jamais authoree : les trois grandeurs viennent du meme profil que celui
        /// que la couche lit, et le facteur de vitesse vient de la fonction pure de la couche
        /// (<see cref="VehicleTireModel.ResolveDriveTorqueFactor"/>). Pour le profil livre, elle
        /// reproduit l'enveloppe MESUREE de la Story 5.12 (16,2 m/s2 a plein gaz, 20,2 m/s2 au frein).
        /// </summary>
        private float driveAccelerationCapacity;
        private float brakeAccelerationCapacity;
        private bool hasDriveCapacity;

        private float laneChangeElapsedSeconds;
        private float instancePhase;
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
        private bool warnedMissingPhysicsProfile;
        private bool warnedLeaderBufferSaturated;
        private bool warnedClearanceBufferSaturated;
        private float frontOffset;
        private float scanRadius;

        /// <summary>
        /// Memoire du point de visee (Story 5.13) : la cible rendue par
        /// <see cref="LaneGraphRouting.ResolveLookAheadPoint"/> part de ce point et avance d'un pas
        /// borne, au lieu de sauter sur la branche du noeud suivant.
        ///
        /// La memoire vit ICI et pas dans la fonction pure : celle-ci reste sans etat, donc sans second
        /// chemin de verite. Elle est remise a zero aux deux seuls moments qui remettent le vehicule a un
        /// point de depart connu -- l'insertion et la recuperation sur place -- exactement comme la
        /// memoire de parcours, et jamais ailleurs : entre ces deux moments, un rappel qui repart de zero
        /// serait un saut de plus, celui-la invisible.
        /// </summary>
        private Vector3 previousAimPoint;
        private bool hasAimPoint;

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

        private void OnDisable()
        {
            appliedAcceleration = 0f;
            // Un appariement de conflit survivrait a la disparition de ce vehicule et laisserait son
            // partenaire prioritaire a vie : le verdict se defait des deux cotes, ici aussi.
            ReleaseTrajectoryConflict();
            if (physicsBody != null && physicsBody.HasProfile)
            {
                SubmitIntentToPhysicsLayer(VehicleDriveIntent.Idle);
            }
        }

        public override void OnNetworkDespawn()
        {
            OnDisable();
            base.OnNetworkDespawn();
        }

        private void FixedUpdate()
        {
            if (!IsServer || body == null || state == null) return;
            var behavior = ResolveBehavior();
            if (state.Behavior.Value != behavior) state.Behavior.Value = behavior;
            if (laneGraph == null || laneGraph.NodeCount <= 0 || driverProfile == null)
            {
                if (driverProfile == null)
                {
                    if (!warnedMissingDriverProfile)
                    {
                        warnedMissingDriverProfile = true;
                        Debug.LogWarning("[Vehicles] " + name + " : aucun DriverProfileDef assigne.", this);
                    }
                }
                SubmitIntentToPhysicsLayer(VehicleDriveIntent.Idle);
                return;
            }
            var dt = Time.fixedDeltaTime;
            var profile = DriverModel.ResolveEffectiveProfile(driverProfile.Profile, behavior);
            var waypointIndex = state.WaypointIndex.Value;
            var waypointPosition = laneGraph.GetNodePosition(waypointIndex);
            if (NetworkedVehicleDriverController.IsBelowVoidHeightThreshold(transform.position.y, voidHeightThreshold))
            {
                RecoverAtWaypoint(waypointPosition);
                return;
            }
            rolloverElapsedSeconds = NetworkedVehicleDriverController.IsRolledOver(transform.up, rolloverUprightDotThreshold)
                ? rolloverElapsedSeconds + dt : 0f;
            if (rolloverElapsedSeconds >= rolloverSustainedSeconds && rolloverElapsedSeconds > 0f)
            {
                RecoverAtWaypoint(waypointPosition);
                return;
            }
            var planarVelocity = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
            var currentSpeed = planarVelocity.magnitude;
            var longitudinalSpeed = Vector3.Dot(planarVelocity, ResolvePlanarForward());
            var hasLeader = TryDetectLeader(profile, dt, out var leaderGap, out var leaderSpeed);
            // Un encastrement GEOMETRIQUE soutenu garde le recovery physique 5.14.
            // La lenteur, meme durable et sans leader, ne suffit jamais a teleporter.
            stuckElapsedSeconds = physicallyEmbedded && !DriverModel.IsDeliberateStop(hasLeader, leaderGap, profile.MinimumGap)
                && IsStuck(currentSpeed, stuckSpeedThreshold)
                ? stuckElapsedSeconds + dt : 0f;
            if (physicallyEmbedded && stuckElapsedSeconds >= stuckSustainedSeconds && !IsAnyPlayerWithinClearanceRadius())
            {
                RecoverAtWaypoint(waypointPosition);
                return;
            }
            // La perception reste active meme pour une disposition immobilisante.
            if (profile.DesiredSpeed <= 0f)
            {
                appliedAcceleration = 0f;
                SubmitIntentToPhysicsLayer(ResolveStopIntent(longitudinalSpeed));
                return;
            }
            if (TickLocalTraffic(profile, dt, currentSpeed, longitudinalSpeed)) return;
            // Story 5.18 : les regles d'intersection s'inserent ICI, au declencheur d'avancee de noeud.
            // Tant que l'arbitrage dit d'attendre, aucun ResolveNextNode n'a lieu -- c'est le seul point
            // de blocage du parcours, et il se place apres la securite de la Story 5.17 (un obstacle
            // percu reste prioritaire sur une priorite de jonction).
            if (TickJunctionRules(profile, dt, currentSpeed, longitudinalSpeed)) return;

            // L'engagement dans une jonction avance le waypoint lui-meme, a la ligne d'arret : la
            // valeur lue plus haut est alors perimee.
            if (waypointIndex != state.WaypointIndex.Value)
            {
                waypointIndex = state.WaypointIndex.Value;
                waypointPosition = laneGraph.GetNodePosition(waypointIndex);
            }
            else if (!JunctionHoldsWaypoint
                && (HasArrivedAtWaypoint(transform.position, waypointPosition, arrivalRadius)
                    || LaneGraphRouting.HasPassedUnreachableWaypoint(
                        transform.position, transform.forward, waypointPosition, currentSpeed, steerDegreesPerSecond)))
            {
                // Le noeud QUITTE, garde pour savoir si le vehicule est engage sur un anneau : y
                // etre engage suppose deux pieds dessus, pas seulement de le viser.
                previousLaneNode = waypointIndex;
                waypointIndex = ResolveNextNode(waypointIndex);
                state.WaypointIndex.Value = waypointIndex;
                waypointPosition = laneGraph.GetNodePosition(waypointIndex);
            }

            // La ligne d'arret d'une jonction se tient comme un LEADER IMMOBILE : le modele
            // longitudinal deja en place amene le vehicule dessus en decelerant, au lieu d'un
            // freinage a fond declenche la ou il se trouvait. C'est ce qui fait qu'il s'arrete AVANT
            // l'intersection et non au milieu.
            // UNE SEULE trajectoire, construite ici, lue ensuite par la visee PUIS par le plafond de
            // courbure. Tant que la conduite visait une tangente extrapolee au-dela du noeud et que la
            // perception lisait des cordes entre noeuds, ni l'une ni l'autre ne decrivait la course
            // reellement parcourue : sur l'anneau de 6,00 m des giratoires la visee sortait 1,68 m,
            // pour un budget d'ecart de 0,97 m avant l'axe median (ANO-5.18-04).
            var lookAheadDistance = Mathf.Max(0f, currentSpeed) * lookAheadSeconds;
            BuildDrivePath(lookAheadDistance);
            lookAheadDistance = ResolveBoundedLookAhead(lookAheadDistance);

            // La visee se REPROJETTE sur la trajectoire au lieu de compter depuis un noeud : c'est ce
            // qui la garde sur la voie quand le vehicule s'en est ecarte -- precisement le moment ou
            // elle compte. Un virage de jonction n'est plus un cas particulier : sa courbe fait partie
            // de la trajectoire comme n'importe quelle autre arete.
            var ideal = HasDrivePath
                ? LaneGraphRouting.ResolvePathLookAheadPoint(DrivePath, DrivePathCount, transform.position, lookAheadDistance)
                : LaneGraphRouting.ResolveLookAheadPoint(transform.position, waypointPosition,
                    laneGraph.GetNodeRotation(waypointIndex) * Vector3.forward, lookAheadDistance, Vector3.zero, 0f);
            // Le rappel de visee LISSAIT les sauts de la cible d'avant, qui changeait de noeud d'un
            // coup. Une visee reprojetee sur la trajectoire ne saute pas : le rappel n'y serait plus
            // que du retard, et du retard sur une courbe se paie en sortie large. Il ne subsiste donc
            // que sur le repli sans trajectoire, ou la cible saute encore.
            var aimPoint = HasDrivePath || !hasAimPoint
                ? ideal
                : Vector3.MoveTowards(previousAimPoint, ideal, Mathf.Max(0f, profile.AimPointRecallSpeed) * dt);
            previousAimPoint = aimPoint;

            // Un plafond fonde sur la courbure COMMANDEE a ete essaye puis RETIRE. Il gagnait 24 %
            // d'ecart sur un virage isole, mais dans un giratoire il prenait l'angle de visee
            // TRANSITOIRE pour une demande de courbure permanente : il tombait a zero et clouait le
            // vehicule a 1,00 m/s sur tout l'anneau, pour une vitesse desiree de 8,0. La courbure du
            // TRACE suffit, et elle ne depend pas de l'erreur instantanee.
            var junctionGap = PlannedStopGap;
            var constrained = hasLeader && leaderGap <= junctionGap;
            var pedal = ResolveLongitudinalPedal(profile, dt, currentSpeed, longitudinalSpeed,
                constrained || float.IsFinite(junctionGap),
                constrained ? leaderGap : junctionGap,
                constrained ? leaderSpeed : 0f);
            TickLaneChangeEvaluation(profile, dt);
            hasAimPoint = true;
            var seekIntent = ComputeSeekIntent(transform.position, transform.forward, aimPoint, steerFullLockDegrees);
            SubmitIntentToPhysicsLayer(new VehicleDriveIntent(pedal.Throttle, seekIntent.Steer, pedal.BrakeReverse, pedal.Handbrake));
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

            if (plannedJunctionNode == currentIndex && plannedJunctionExit >= 0)
            {
                var drawn = plannedJunctionExit;
                plannedJunctionNode = -1;
                plannedJunctionExit = -1;
                MarkNodeTraversed(drawn);
                return drawn;
            }

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

            // La memoire de la CIBLE est effacee ici pour la meme raison que celle du parcours : c'est un
            // point de depart connu, donc tout rappel qui repartirait d'une cible anterieure serait un
            // saut de plus. Ailleurs la memoire doit survivre -- c'est elle qui porte la continuite.
            hasAimPoint = false;
            previousAimPoint = Vector3.zero;

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
        /// Reorientation vers une sortie reellement accessible par les aretes dirigees. L'absence de
        /// route garde le vehicule sur place : un portail proche dans l'espace n'est jamais une cible
        /// atteignable par saut.
        /// </summary>
        private int ResolveRedirectToNearestExit(int currentIndex, IReadOnlyList<int> candidates)
        {
            var next = LaneGraphRouting.FindNextTowardReachableExit(
                currentIndex, laneGraph.NodeCount, laneGraph.GetSuccessors, laneGraph.IsExitPortal);
            if (next >= 0)
            {
                return next;
            }

            if (!warnedNoReachableExit)
            {
                warnedNoReachableExit = true;
                Debug.LogWarning("[Vehicles] " + name
                    + " : aucune route dirigee ne rejoint un portail de sortie; le vehicule reste inerte (jamais retire).", this);
            }

            if (candidates.Count == 0 && !warnedDeadEnd)
            {
                warnedDeadEnd = true;
                Debug.LogWarning("[Vehicles] " + name + " : cul-de-sac sans sortie accessible.", this);
            }

            return currentIndex;
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
        /// Decision longitudinale (Story 5.9, INCHANGEE) traduite en position de pedale (Story 5.14).
        /// L'ordre de composition impose par la spec est conserve -- disposition (deja appliquee par
        /// l'appelant) puis bruit de personnalite sur la seule vitesse desiree, juste avant le calcul
        /// d'acceleration -- et les quatre appels du modele restent les memes.
        ///
        /// Ce qui change est la SORTIE : plus une vitesse entretenue ici, mais un intent. La vitesse est
        /// un ARGUMENT, relu du <c>Rigidbody</c> par l'appelant : c'est elle qui rend le vehicule
        /// poussable, puisque plus rien ne la reimpose au pas suivant.
        /// </summary>
        private VehicleDriveIntent ResolveLongitudinalPedal(
            DriverProfile profile,
            float fixedDeltaTime,
            float speed,
            float longitudinalSpeed,
            bool hasLeader,
            float detectedGap,
            float detectedLeaderSpeed)
        {
            // La vitesse desiree est BORNEE par ce que la courbe a venir permet de tenir. Sans cette
            // borne le vehicule vise une trajectoire que son braquage ne couvre pas : il sort large,
            // et aucune correction de visee ne peut rattraper une consigne physiquement infaisable.
            var noisyDesiredSpeed = Mathf.Min(
                DriverModel.ResolveNoisyDesiredSpeed(profile.DesiredSpeed, profile.Consistency, Time.time, instancePhase),
                ResolveCurveSpeedCeiling(speed));

            var gap = hasLeader ? detectedGap : DriverModel.NoLeaderGap;
            var leaderSpeed = hasLeader ? detectedLeaderSpeed : 0f;

            var targetAcceleration = DriverModel.ComputeAcceleration(
                profile.WithDesiredSpeed(noisyDesiredSpeed), speed, leaderSpeed, gap);

            appliedAcceleration = DriverModel.SmoothAcceleration(
                appliedAcceleration, targetAcceleration, profile.ReactionTime, fixedDeltaTime);

            return ResolvePedalIntent(appliedAcceleration, longitudinalSpeed);
        }

        /// <summary>
        /// Traduction d'une acceleration DEMANDEE par le modele de conduite en position de pedale.
        ///
        /// Elle est necessaire parce que les deux grandeurs ne sont pas dans la meme unite : l'IDM
        /// demande des m/s2, la couche physique attend une position de pedale dans [0, 1]. Sans
        /// echelle, une pedale a 1 transformerait une demande de 1,5 m/s2 en 16,2 m/s2 reels : le
        /// conducteur ne serait plus conduit, il serait multiplie par la capacite du moteur, et sa
        /// vitesse desiree serait atteinte par une poussee que sa personnalite ne decrit pas. La pedale
        /// est donc la FRACTION de la capacite disponible du vehicule -- capacite a plein gaz modulee
        /// par le facteur de vitesse de la couche, braquet constant au frein.
        ///
        /// Frein : sous le seuil de changement de sens du profil, la couche physique ne lit plus le
        /// meme axe -- l'entree de frein y devient une MARCHE ARRIERE
        /// (<see cref="VehicleTireModel.ResolveWheelDriveTorque"/>). Le modele de conduite IA n'a
        /// aucune decision de recul : sous ce seuil, la seule demande possible est de s'arreter, et
        /// l'intent rendu est <see cref="VehicleDriveIntent.Idle"/> (frein moteur authore), jamais une
        /// marche arriere inventee.
        /// </summary>
        private VehicleDriveIntent ResolvePedalIntent(float acceleration, float longitudinalSpeed)
        {
            EnsureDriveCapacity();

            var vehicle = physicsBody != null && physicsBody.HasProfile ? physicsBody.Profile : default;

            // DriverModel requests net acceleration. PhysX damping acts after tire forces,
            // so include that resistance when converting the request to wheel torque.
            if (body != null && longitudinalSpeed > 0f)
            {
                var retained = Mathf.Max(0.01f, 1f - body.linearDamping * Time.fixedDeltaTime);
                acceleration = (acceleration + body.linearDamping * longitudinalSpeed) / retained;
            }

            if (acceleration > 0f)
            {
                var available = driveAccelerationCapacity
                    * VehicleTireModel.ResolveDriveTorqueFactor(longitudinalSpeed, vehicle.MaxForwardSpeed);
                var throttle = available > 0f ? Mathf.Clamp01(acceleration / available) : 0f;

                return new VehicleDriveIntent(throttle, 0f, 0f, 0f);
            }

            if (acceleration >= 0f)
            {
                return VehicleDriveIntent.Idle;
            }

            if (longitudinalSpeed <= vehicle.MinimumDirectionSpeed)
            {
                return VehicleDriveIntent.Idle;
            }

            var brake = brakeAccelerationCapacity > 0f
                ? Mathf.Clamp01(-acceleration / brakeAccelerationCapacity)
                : 0f;

            return new VehicleDriveIntent(0f, 0f, brake, 0f);
        }

        /// <summary>
        /// Derive une seule fois les deux echelles de pedale du vehicule, depuis le profil physique que
        /// la couche lit deja. Appelee paresseusement : l'ordre d'execution des <c>Awake</c> entre ce
        /// controleur et la couche n'est pas defini, donc l'echelle ne peut pas etre calculee a la
        /// construction. Sans couche physique, ou sans profil applique, rien n'est mis en cache -- et la
        /// conversion retombe alors sur un intent neutre, jamais sur un repli numerique invente.
        /// </summary>
        private void EnsureDriveCapacity()
        {
            if (hasDriveCapacity || physicsBody == null || !physicsBody.HasProfile)
            {
                return;
            }

            var vehicle = physicsBody.Profile;
            var drivenWheels = 0;
            var radiusSum = 0f;

            for (var i = 0; i < vehicle.WheelCount; i++)
            {
                var wheel = vehicle.GetWheel(i);
                radiusSum += wheel.Radius;
                if (wheel.IsDriven)
                {
                    drivenWheels++;
                }
            }

            var meanRadius = vehicle.WheelCount > 0 ? radiusSum / vehicle.WheelCount : 0f;
            var massLoad = vehicle.Mass > 0f && meanRadius > 0f ? 1f / (vehicle.Mass * meanRadius) : 0f;

            driveAccelerationCapacity = drivenWheels * Mathf.Max(0f, vehicle.EngineTorque) * massLoad;
            brakeAccelerationCapacity = vehicle.WheelCount * Mathf.Max(0f, vehicle.BrakeTorque) * massLoad;
            hasDriveCapacity = true;
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
            var forward = ResolvePlanarForward();

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
            vehicleBox = box;
            if (box == null)
            {
                return;
            }

            var scale = transform.lossyScale;
            vehicleHalfExtents = Vector3.Scale(box.size * 0.5f, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            vehicleCenterOffset = Vector3.Scale(box.center, scale);
            frontOffset = Mathf.Max(0f, (box.center.z + (box.size.z * 0.5f)) * Mathf.Abs(scale.z));
            scanRadius = Mathf.Max(0f, box.size.x * 0.5f * Mathf.Abs(scale.x) * ScanWidthFactor);
        }

        /// <summary>
        /// Avant du vehicule projete sur le plan et normalise. Meme axe que celui que la couche physique
        /// derive de <c>transform.forward</c> : c'est ce qui rend la vitesse longitudinale lue ici
        /// comparable a celle qu'elle lit pour choisir entre freiner et reculer.
        /// </summary>
        private Vector3 ResolvePlanarForward()
        {
            var forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            return forward.normalized;
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
        /// Predicat pur (Story 5.2, revise en 5.12) : intent de poursuite deterministe vers le POINT DE
        /// VISEE -- direction bornee par steerFullLockDegrees. Le point vient de
        /// <see cref="LaneGraphRouting.ResolveLookAheadPoint"/> et non plus de la position du noeud :
        /// c'est ce qui remplace la poursuite point-a-point.
        ///
        /// Story 5.14 : de ce que cette fonction rend, seule la DIRECTION est consommee par le chemin de
        /// conduite -- la pedale, elle, vient de la decision longitudinale du modele
        /// (<see cref="DriverModel.ComputeAcceleration"/> lissee par le temps de reaction). Le throttle
        /// pose ici reste un repli FINI, pas une consigne de puissance : il n'est plus lu.
        ///
        /// L'arrivee au noeud n'est plus testee ici : elle l'est par <see cref="HasArrivedAtWaypoint"/> a
        /// l'endroit ou la decision se prend, donc la fonction n'a plus a rendre un intent neutre qui
        /// signifiait "je suis arrive" et que l'appelant devait interpreter.
        /// </summary>
        public static VehicleDriveIntent ComputeSeekIntent(Vector3 position, Vector3 forward, Vector3 aimPoint, float steerFullLockDegrees)
        {
            // Derniere barriere avant la couche physique, et elle seule : un point de visee non fini
            // produirait un intent non fini, donc une vitesse non finie dans la couche, donc un vehicule
            // qui disparait. Le repli est « tout droit, plein gaz » -- un cap faux mais FINI, et que le
            // parcours rattrape au noeud suivant.
            if (!float.IsFinite(aimPoint.x) || !float.IsFinite(aimPoint.y) || !float.IsFinite(aimPoint.z)
                || !float.IsFinite(position.x) || !float.IsFinite(position.y) || !float.IsFinite(position.z))
            {
                return new VehicleDriveIntent(1f, 0f, 0f, 0f);
            }

            var toAimPoint = aimPoint - position;
            toAimPoint.y = 0f;

            var flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude <= 0.0001f)
            {
                flatForward = Vector3.forward;
            }

            flatForward.Normalize();

            if (toAimPoint.sqrMagnitude <= 0.0001f)
            {
                return new VehicleDriveIntent(1f, 0f, 0f, 0f);
            }

            var direction = toAimPoint.normalized;
            var signedAngle = Vector3.SignedAngle(flatForward, direction, Vector3.up);
            var steerLock = Mathf.Max(steerFullLockDegrees, 0.0001f);
            var steer = Mathf.Clamp(signedAngle / steerLock, -1f, 1f);

            return new VehicleDriveIntent(1f, steer, 0f, 0f);
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

        /// <summary>
        /// Chemin UNIQUE de l'intent IA vers la couche physique, sur le patron du vehicule joueur
        /// (<c>NetworkedVehicleDriverController.SubmitIntentToPhysicsLayer</c>) : diagnostic d'absence
        /// de profil applique, emis une seule fois, puis soumission.
        ///
        /// Les trois echelles viennent du profil PHYSIQUE, sans reduction de degats : l'IA n'a pas
        /// d'etat de degats, et elle ne recoit pas ici l'autorite du joueur
        /// (<c>NetworkedVehicleDriverController.ResolveDriveAuthority</c>, Story 3.5).
        ///
        /// Aucun second chemin : ce controleur n'ecrit ni vitesse, ni position, ni rotation de caisse.
        /// La seule ecriture de vitesse qui lui reste est la remise a zero de
        /// <see cref="RecoverAtWaypoint"/>, qui n'est pas de la conduite.
        /// </summary>
        private void SubmitIntentToPhysicsLayer(VehicleDriveIntent intent)
        {
            if (physicsBody == null || !physicsBody.HasProfile)
            {
                // Un prefab mal cable ne conduit nulle part : sans ce diagnostic, l'absence de
                // mouvement n'aurait aucune trace, et le symptome se lirait comme un reglage.
                if (!warnedMissingPhysicsProfile)
                {
                    warnedMissingPhysicsProfile = true;
                    Debug.LogWarning("[Vehicles] " + name + " : aucun profil physique applique, la conduite IA est ignoree.", this);
                }

                return;
            }

            var vehicle = physicsBody.Profile;
            physicsBody.ApplyDriveIntent(intent, vehicle.MaxForwardSpeed, vehicle.SteerRateDegreesPerSecond, vehicle.BrakeTorque);
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
            CancelManeuver("physical recovery", driverProfile != null ? driverProfile.Profile : default);
            hasSnapshot = false;
            stuckElapsedSeconds = 0f;
            appliedAcceleration = 0f;

            // Deplacement discontinu : meme purge que cote joueur, sinon le pic d'amortisseur de la
            // reprise de contact s'ajoute a la recuperation.
            var physics = GetComponent<VehiclePhysicsBody>();
            if (physics != null)
            {
                physics.ResetSuspensionState();
            }

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

            // La couche physique unique (AD-35), la meme que celle du vehicule joueur. Absente du
            // prefab, le diagnostic de SubmitIntentToPhysicsLayer parle et le vehicule reste inerte.
            if (physicsBody == null)
            {
                physicsBody = GetComponent<VehiclePhysicsBody>();
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
