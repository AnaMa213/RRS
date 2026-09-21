using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    public sealed partial class NetworkedAIVehicleDriverController
    {
        private BoxCollider vehicleBox;
        private Vector3 vehicleHalfExtents;
        private Vector3 vehicleCenterOffset;
        private readonly Collider[] poseHits = new Collider[64];
        private readonly RaycastHit[] groundHits = new RaycastHit[16];
        private readonly Collider[] candidateColliders = new Collider[64];
        private readonly Vector3[] maneuverTargets = new Vector3[3];
        private bool hasSnapshot;
        private bool refreshedThisStep;
        private bool perceptionSaturated;
        private bool physicallyEmbedded;
        private int perceptionCount;
        private float collisionTime = float.PositiveInfinity;
        private Vector3 collisionPosition;
        private Collider blockingCollider;
        private bool maneuverActive;
        private bool reverseAttempted;
        private int failedManeuverMask;
        private bool unblockingEpisodeActive;
        private bool maneuverPathSafe;
        private int maneuverTargetIndex;
        private int maneuverTargetCount;
        private float maneuverElapsed;
        private float reverseTravelSeconds;
        private float noProgressElapsed;
        private float bestTargetDistance;
        private float retryAfter;

        // ----------------------------------------------------------------------------------
        // MODELE DE RECUPERATION UNIFIE (ANO-5.18-10).
        //
        // Quatre horloges distinctes coexistaient -- file bloquee, attente de jonction, cession,
        // enchevetrement -- avec trois echappatoires differentes, et le face-a-face n'en avait
        // AUCUNE. Les quatre cas de recette ne different pourtant que par leur nom : le vehicule
        // n'avance plus. Une seule mesure les couvre donc tous, et un seul escalier y repond.
        // ----------------------------------------------------------------------------------
        private float stalledElapsedSeconds;

        /// <summary>Duree passee a contresens EN CONDUITE NOMINALE (hors manoeuvre). Story 5.19.</summary>
        private float wrongWayElapsedSeconds;
        private bool laneDirectionCorrect = true;
        private float laneAxisDistance;
        private bool reorientAttempted;
        private float routeBestDistance = float.PositiveInfinity;
        private int recoveryStage;
        private bool heldByBoundedWait;
        private bool junctionHeldBySignal;
        private string conflictVerdictReason = string.Empty;
        private NetworkedAIVehicleDriverController conflictDriver;
        private bool conflictYield;
        private string trafficRefusal = string.Empty;

        // ----------------------------------------------------------------------------------
        // Story 5.18 (correctif du retour terrain) : trajectoire propre et conflits de trajectoire.
        //
        // Avant ce correctif, la decision de freiner reposait sur l'extrapolation RECTILIGNE de la
        // vitesse instantanee pendant tout l'horizon de prediction. Un vehicule qui tourne, qui
        // circule sur un anneau de giratoire ou qui derive lateralement de quelques dizaines de
        // centimetres par seconde -- ce que fait en permanence une voiture pilotee par un
        // asservissement de braquage -- voyait donc sa tangente sortir de sa voie et percuter le
        // decor, la voie opposee ou un joueur gare a cote. Ces trois faux positifs etaient un seul
        // et meme defaut de MODELE, pas un probleme de reglage de distance.
        //
        // La trajectoire prevue est desormais la POLYLIGNE de voie que le vehicule va reellement
        // suivre, reconstruite depuis le graphe. C'est la meme donnee des deux cotes d'un conflit,
        // donc deux pairs en tirent le meme verdict.
        // ----------------------------------------------------------------------------------
        /// <summary>
        /// Produit scalaire minimal entre le cap d'un usager et la tangente de notre voie pour qu'il
        /// soit lu comme circulant DANS NOTRE AXE. 0,7 correspond a 45 degres : au-dela, ce n'est plus
        /// une file ni un face-a-face mais un croisement, et l'arbitrage de priorite en repond.
        /// </summary>
        private const float SameLaneAlignmentDot = 0.7f;

        /// <summary>
        /// Pas d'echantillonnage d'une arete courbe. Une arete droite n'en consomme qu'un point, donc
        /// ce pas ne coute que dans les virages et les giratoires, la ou la corde ment.
        /// </summary>
        private const float LanePathSampleStep = 1f;

        // 32 points : une arete d'anneau de giratoire (3,81 m de corde) en consomme 4, et la portee de
        // prediction atteint une vingtaine de metres. A 12 points la trajectoire echantillonnee etait
        // tronquee avant la fin du virage, ce qui rendait la visee a l'extrapolation tangentielle.
        private readonly Vector3[] ownPredictedPath = new Vector3[32];
        private readonly Vector3[] otherPredictedPath = new Vector3[32];

        /// <summary>
        /// Trajectoire de CONDUITE, reconstruite a chaque pas physique. Elle ne partage pas le tampon
        /// de perception parce que celle-ci ne se rafraichit qu'a sa cadence authoree : viser une
        /// trajectoire vieille d'un intervalle de perception ferait tourner le volant en retard.
        /// </summary>
        private readonly Vector3[] drivePath = new Vector3[32];
        private int drivePathCount;

        /// <summary>Marge de trajectoire au-dela de la visee : sans elle la visee tombe du bout de la
        /// polyligne et retombe sur une extrapolation tangentielle, c'est-a-dire sur le defaut corrige.</summary>
        private const float LanePathDriveMargin = 6f;

        private Vector3[] DrivePath => drivePath;
        private int DrivePathCount => drivePathCount;
        private bool HasDrivePath => drivePathCount >= 2;

        private void BuildDrivePath(float lookAheadDistance)
        {
            BuildPathFromLaneGraph(drivePath, Mathf.Max(0f, lookAheadDistance) + LanePathDriveMargin, out drivePathCount);
        }

        /// <summary>
        /// Visee plancher a l'EMPATTEMENT. Sous cette distance une poursuite pure oscille : elle
        /// corrige plus fort que le vehicule ne peut tourner, et le depassement remplace le suivi.
        ///
        /// Une borne HAUTE proportionnelle au rayon a ete essayee puis retiree : la mesure dit
        /// l'inverse de l'intuition. Sur l'anneau de 6,00 m des giratoires, brider la visee a la
        /// moitie du rayon portait l'ecart a 1,27 m, la laisser libre le ramenait a 0,22 m -- a
        /// vitesse pleine et sans a-coup dans les deux cas. Une visee courte ne suit pas mieux une
        /// courbe, elle la poursuit en zigzag.
        /// </summary>
        private float ResolveBoundedLookAhead(float lookAheadDistance)
        {
            return Mathf.Max(lookAheadDistance, Wheelbase());
        }

        /// <summary>
        /// Plafond de vitesse impose par la COURBURE de la trajectoire a venir. Aucune constante
        /// nouvelle : il se deduit de l'empattement et de la loi de braquage authorees sur le vehicule,
        /// donc un carrefour plus large ou un vehicule plus maniable relevent d'eux-memes leur propre
        /// plafond, sans reglage par scene.
        ///
        /// Mesure ANO-5.18-04 : le connecteur de virage serre du district demande 4,25 m quand le
        /// braquage disponible a 8,0 m/s n'en permet que 4,85 m. Le vehicule visait une courbe qu'il ne
        /// pouvait pas tenir et sortait 1,84 m a cote, soit bien au-dela des 0,97 m de marge que lui
        /// laisse sa demi-chaussee.
        /// </summary>
        private float ResolveCurveSpeedCeiling(float speed)
        {
            if (physicsBody == null || !physicsBody.HasProfile || drivePathCount < 3) return float.PositiveInfinity;

            var vehicle = physicsBody.Profile;
            var window = Mathf.Max(LanePathDriveMargin, Mathf.Max(0f, speed) * lookAheadSeconds + LanePathDriveMargin);
            var limit = LaneGraphRouting.ResolvePathCurveSpeedLimit(drivePath, drivePathCount, transform.position,
                window, Wheelbase(), vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees,
                vehicle.SteerFullReductionSpeed);

            if (limit > 0.1f) return limit;

            // Rayon qu'AUCUNE vitesse ne tient : c'est un defaut de trace, pas un reglage de conduite.
            // Le vehicule le prend au pas plutot que d'immobiliser la file, et le defaut est signale.
            if (!warnedInfeasibleCurve)
            {
                warnedInfeasibleCurve = true;
                Debug.LogWarning("[Vehicles] " + name + " : rayon de voie intenable a toute vitesse, passage au pas.", this);
            }

            return 1f;
        }

        private bool warnedInfeasibleCurve;
        /// <summary>
        /// Noeuds de voie que la trajectoire prevue emprunte. C'est l'IDENTITE de voie, et elle
        /// remplace le test geometrique pour reconnaitre un vehicule "de la meme file".
        ///
        /// A un carrefour, deux branches transversales se touchent presque : le nez d'un vehicule
        /// arrete sur la branche d'en face empiete reellement de quelques dizaines de centimetres sur
        /// le couloir de l'autre. Aucun seuil geometrique ne peut donc trancher -- et c'est bien la
        /// regle de priorite qui doit arbitrer ce cas, pas la poursuite en file. Seule l'identite de
        /// voie distingue les deux, et elle ne depend d'aucune distance.
        /// </summary>
        private readonly int[] ownPredictedPathNodes = new int[32];
        private int ownPredictedPathCount;
        private float ownPredictedSpeed;
        private float conflictArrival = float.PositiveInfinity;
        private float conflictOtherArrival = float.PositiveInfinity;
        private NetworkedAIVehicleDriverController conflictPeer;
        private bool conflictIsPlayer;
        private bool hasTrajectoryConflict;
        private bool headOnConflict;
        private bool headOnYield;
        private bool headOnStationary;
        private Vector3 conflictPoint;
        private bool hasConflictPoint;
        private float conflictSpeedLimitGap = float.PositiveInfinity;
        private float yieldElapsedSeconds;
        private bool yieldReleased;
        private bool warnedYieldRelease;
        private TrafficDecisionReason decisionReason;

        // ----------------------------------------------------------------------------------
        // Diagnostic exige par ANO-5.18-01/02 : un motif seul ne se verifie pas. "Obstacle immobile"
        // sans le NOM de l'objet detecte ne permet ni de confirmer ni d'infirmer un faux positif, et
        // c'est precisement ce qui a laisse passer les passes correctives precedentes.
        // ----------------------------------------------------------------------------------
        private float queueElapsedSeconds;
        private bool warnedQueueStalled;
        private string junctionDiagnosticId = string.Empty;
        private bool playerImmediate;
        private float playerImmediateGap;
        private string playerImmediateName = string.Empty;
        private NetworkedAIVehicleDriverController leaderPeer;
        private string leaderName = string.Empty;
        private bool leaderIsStatic;
        private string exitSaturatedBy = string.Empty;
        private string decisionSubject = string.Empty;
        private string decisionDetail = string.Empty;
        private int leaderLaneNode = -1;

        public IReadOnlyList<TrafficPerceptionCandidate> LocalTraffic => perceptionCandidates;
        public int LocalTrafficCount => perceptionCount;
        public bool PerceptionSaturated => perceptionSaturated;
        public float PredictedCollisionTime => collisionTime;
        public TrafficUnblockingAction UnblockingAction => currentUnblockingAction;
        public Vector3 ManeuverTarget => maneuverActive ? maneuverTargets[maneuverTargetIndex] : transform.position;
        public string TrafficRefusal => trafficRefusal;
        public bool IsYielding => conflictDriver != null && conflictYield && !yieldReleased;

        /// <summary>
        /// Story 5.18 (correctif) : POURQUOI ce vehicule roule ou s'arrete, en un mot lisible. Le
        /// libelle debug l'affiche a cote de la disposition de rage : sans lui, "IA : Calm" ne
        /// distingue pas une file, un refus de priorite et un arret sur rien.
        /// </summary>
        public TrafficDecisionReason DecisionReason => decisionReason;

        /// <summary>Delai avant que NOUS atteignions le point de conflit arbitre. Infini sans conflit.</summary>
        public float ConflictArrival => conflictArrival;

        /// <summary>Delai avant que l'autre usager l'atteigne. Infini sans conflit.</summary>
        public float ConflictOtherArrival => conflictOtherArrival;

        /// <summary>Nombre de points de la trajectoire prevue au dernier rafraichissement.</summary>
        public int PredictedPathCount => ownPredictedPathCount;

        /// <summary>Noeud de voie courant de ce vehicule, ou -1. C'est l'identite de voie observable par un pair.</summary>
        public int CurrentLaneNode => state != null ? state.WaypointIndex.Value : -1;

        /// <summary>Objet nomme a l'origine de la decision courante (leader, obstacle, joueur), ou vide.</summary>
        public string DecisionSubject => decisionSubject;

        /// <summary>
        /// Le vehicule dont CET arret depend, ou null. C'est l'arete du graphe d'attente : un
        /// interblocage se lit en la suivant, pas en constatant que trois vehicules sont a l'arret.
        /// Suivre son leader ou ceder a un pair sont les deux seules dependances entre vehicules ;
        /// tout le reste (feu, ligne d'arret, obstacle) ne depend de personne.
        /// </summary>
        public NetworkedAIVehicleDriverController WaitsFor
        {
            get
            {
                switch (decisionReason)
                {
                    case TrafficDecisionReason.FollowingSameLane:
                    case TrafficDecisionReason.TrafficQueue:
                        return leaderPeer;
                    case TrafficDecisionReason.YieldTrajectory:
                    case TrafficDecisionReason.EmergencyBrake:
                    case TrafficDecisionReason.PlayerCollisionRisk:
                        return conflictDriver != null ? conflictDriver : conflictPeer;
                    default:
                        return null;
                }
            }
        }

        /// <summary>Pair en conflit de trajectoire, apparie ou non : le non-apparie est le cas interessant.</summary>
        public NetworkedAIVehicleDriverController ConflictPartner => conflictDriver != null ? conflictDriver : conflictPeer;

        /// <summary>Vrai quand un conflit est vu SANS verdict : le pair arbitrait deja avec un troisieme.</summary>
        public bool ConflictUnarbitrated => hasTrajectoryConflict && conflictDriver == null;

        /// <summary>
        /// Trace lisible de la decision courante : etat, sujet, voies, ecart. C'est la ligne que le
        /// libelle monde affiche et qu'une recette manuelle relit.
        /// </summary>
        public string DecisionDetail => decisionDetail;

        /// <summary>Vrai si <paramref name="node"/> est emprunte par notre trajectoire prevue.</summary>
        public bool IsNodeOnPredictedPath(int node)
        {
            if (node < 0) return false;
            for (var i = 0; i < ownPredictedPathCount && i < ownPredictedPathNodes.Length; i++)
                if (ownPredictedPathNodes[i] == node) return true;
            return false;
        }

        /// <summary>Vitesse planaire mesuree, telle qu'un pair la lit pour predire notre trajectoire.</summary>
        public float PlanarSpeed => body != null
            ? Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude : 0f;

        /// <summary>
        /// Recopie la trajectoire prevue de CE vehicule pour qu'un pair l'arbitre. Comme
        /// <see cref="TryDescribeJunctionClaim"/>, tout ce qui en sort est de la donnee authoree ou du
        /// monde physique partage -- jamais un etat prive -- c'est la condition de la symetrie du
        /// verdict.
        /// </summary>
        public int CopyPredictedPath(Vector3[] destination, float reach)
        {
            if (destination == null || destination.Length < 2) return 0;
            BuildPathFromLaneGraph(destination, reach, out var count);
            return count;
        }

        private bool TryDetectLeader(DriverProfile profile, float dt, out float gap, out float speed)
        {
            perceptionElapsedSeconds += dt;
            refreshedThisStep = !hasSnapshot || perceptionElapsedSeconds >= profile.PerceptionInterval;
            if (refreshedThisStep)
            {
                perceptionElapsedSeconds = 0f;
                RefreshPerception(profile);
            }
            // Le snapshot reste stable; sa distance longitudinale vieillit de facon conservative.
            gap = Mathf.Max(0f, perceivedLeaderGap - Mathf.Max(0f,
                Vector3.Dot(body.linearVelocity, ResolvePlanarForward()) - perceivedLeaderSpeed) * perceptionElapsedSeconds);
            speed = perceivedLeaderSpeed;
            return hasPerceivedLeader;
        }

        private void RefreshPerception(DriverProfile profile)
        {
            hasSnapshot = true;
            perceptionCount = 0;
            collisionTime = float.PositiveInfinity;
            blockingCollider = null;
            physicallyEmbedded = false;
            hasTrajectoryConflict = false;
            conflictArrival = float.PositiveInfinity;
            conflictOtherArrival = float.PositiveInfinity;
            conflictIsPlayer = false;
            headOnConflict = false;
            headOnYield = false;
            headOnStationary = false;
            hasConflictPoint = false;
            conflictPeer = null;
            playerImmediate = false;
            playerImmediateGap = float.PositiveInfinity;
            playerImmediateName = string.Empty;
            var center = transform.position + transform.rotation * vehicleCenterOffset;
            var forward = ResolveScanDirection();
            var right = Vector3.Cross(Vector3.up, forward);

            // Vitesse a laquelle nous PARCOURRIONS notre trajectoire si nous continuions, et non la
            // vitesse instantanee : sinon un vehicule a l'arret ne voit plus aucun conflit, repart,
            // en voit un, et oscille. Les deux termes sont authores (profil), aucun n'est invente ici.
            var measured = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
            ownPredictedSpeed = Mathf.Min(profile.DesiredSpeed,
                measured + Mathf.Max(0f, profile.MaxAcceleration) * profile.PredictionSeconds);
            var reach = ownPredictedSpeed * profile.PredictionSeconds + frontOffset + profile.SafetyMargin;
            BuildPathFromLaneGraph(ownPredictedPath, reach, out ownPredictedPathCount, ownPredictedPathNodes);
            // Emprise propre exprimee dans le repere de la TRAJECTOIRE (x lateral, z longitudinal) :
            // c'est la pose que le vehicule aura en ce point de la voie, pas celle de son nez actuel.
            var ownFootprint = new Vector3(vehicleHalfExtents.x, vehicleHalfExtents.y,
                Mathf.Max(vehicleHalfExtents.z, frontOffset));
            var count = Physics.OverlapSphereNonAlloc(center, profile.PerceptionRadius, perceptionHits, ~0, QueryTriggerInteraction.Ignore);
            perceptionSaturated = count >= perceptionHits.Length;
            if (perceptionSaturated && !warnedLeaderBufferSaturated)
            {
                warnedLeaderBufferSaturated = true;
                Debug.LogWarning("[Vehicles] " + name + " : perception saturee, arret prudent.", this);
            }
            for (var i = 0; i < count; i++)
            {
                var hit = perceptionHits[i];
                if (hit == null || hit.attachedRigidbody == body || IsLowSurface(hit, profile, true)) continue;
                var bounds = hit.bounds;
                if (Mathf.Abs(bounds.center.y - center.y) > bounds.extents.y + vehicleHalfExtents.y) continue;
                if (vehicleBox != null && hit.attachedRigidbody == null && hit.GetComponentInParent<CharacterController>() == null
                    && Physics.ComputePenetration(vehicleBox, transform.position, transform.rotation,
                        hit, hit.transform.position, hit.transform.rotation, out _, out var penetration)
                    && penetration > profile.SafetyMargin)
                    physicallyEmbedded = true;
                ReadColliderGeometry(hit, out var actorCenter, out var actorExtents, out var actorRotation);
                var offset = Vector3.ProjectOnPlane(actorCenter - center, Vector3.up);
                var walker = hit.GetComponentInParent<CharacterController>();
                var otherBody = hit.attachedRigidbody;
                var velocity = otherBody != null ? otherBody.linearVelocity : walker != null ? walker.velocity : Vector3.zero;
                var playerVehicle = otherBody != null
                    && hit.GetComponentInParent<NetworkedAIVehicleDriverController>() == null
                    && hit.GetComponentInParent<NetworkedVehicleDriverController>() != null;

                // ANO-5.18-01 : un pieton sur le TROTTOIR n'est pas un usager de la chaussee. Le faire
                // participer a la prediction de trajectoire suffisait a produire un conflit des qu'il
                // marchait vers la route, alors qu'il ne l'atteindra jamais -- c'est de la circulation
                // pietonne normale. La regle n'est pas une distance, c'est la SURFACE qu'il foule.
                var onFoot = walker != null;
                var offCarriageway = onFoot && !IsOnCarriageway(actorCenter, profile);
                var peer = hit.GetComponentInParent<NetworkedAIVehicleDriverController>();
                if (peer == this) peer = null;

                // Position relative a NOTRE VOIE, pas a la tangente de notre nez : c'est la mesure qui
                // distingue "devant moi dans ma file" de "en face sur l'autre voie" en virage.
                var projected = TrafficPerception.TryProjectOnPath(ownPredictedPath, ownPredictedPathCount,
                    actorCenter, out var pathAlong, out var pathLateral, out var pathTangent);
                if (!projected) { pathTangent = forward; pathAlong = Vector3.Dot(offset, forward); pathLateral = Vector3.Dot(offset, right); }
                var pathNormal = Vector3.Cross(Vector3.up, pathTangent);
                var halfLength = TrafficPerception.ProjectExtent(actorExtents, actorRotation, pathTangent);
                var halfWidth = TrafficPerception.ProjectExtent(actorExtents, actorRotation, pathNormal);
                var behind = Vector3.Dot(offset, forward) < 0f;

                // Degagement REEL entre notre voie et l'emprise de l'acteur : distance a son point le
                // plus proche, pas a son centre. Voir TrafficPerception.PlanarDistanceToBox pour le
                // faux positif que la mesure au centre produisait.
                var corridor = vehicleHalfExtents.x + profile.SafetyMargin;
                var hasClearance = TrafficPerception.TryPathClearance(ownPredictedPath, ownPredictedPathCount,
                    actorCenter, actorExtents, actorRotation, out var laneClearance, out var clearanceAlong, corridor);

                // Un usager qui SE DEPLACE n'appartient a notre file que s'il roule dans le meme axe.
                // A un carrefour, la branche transversale empiete physiquement sur notre couloir sans
                // etre pour autant une file a suivre : c'est la regle de priorite qui l'arbitre. Un
                // objet immobile, lui, n'a pas d'axe -- il obstrue ou il n'obstrue pas.
                var travelDirection = Vector3.ProjectOnPlane(velocity, Vector3.up);
                var mobile = otherBody != null || walker != null;
                var heading = travelDirection.sqrMagnitude > 0.01f ? travelDirection.normalized
                    : mobile ? Vector3.ProjectOnPlane(hit.transform.forward, Vector3.up).normalized : Vector3.zero;

                // LE CAP D'UN PAIR IA EST CELUI DE SA VOIE, jamais celui de son nez.
                //
                // Un vehicule a l'arret garde le nez ou son dernier virage l'a laisse, et sa vitesse
                // ne dit plus rien. Dans un giratoire, ou la voie tourne en permanence, un nez fige
                // pointe donc n'importe ou -- y compris a plus de 135 deg de notre tangente, ce qui
                // suffisait a le declarer FACE-A-FACE. Or un face-a-face ne s'arbitre pas : les deux
                // vehicules s'arretent, definitivement, et la rupture de cycle d'attente ne s'y
                // applique pas puisqu'elle ne couvre que les cessions de priorite. C'est le blocage
                // observe en recette : "EmergencyBrake (face-a-face dans la voie), arrivee=1,11s vs
                // +Infini" -- l'infini disant justement que le pair est immobile.
                //
                // Sa VOIE, elle, dit ou il ira des qu'il repartira, et c'est de la donnee authoree
                // que les deux pairs lisent a l'identique : le verdict reste symetrique. Un vehicule
                // reellement a contresens porte un noeud de voie oppose au notre, et reste donc
                // detecte.
                if (peer != null && laneGraph != null && laneGraph.IsValidIndex(peer.CurrentLaneNode))
                {
                    var laneForward = ResolveNodeForward(peer.CurrentLaneNode);
                    if (laneForward.sqrMagnitude > 0.0001f) heading = laneForward;
                }
                //
                // Le sens compte. Un usager qui nous FAIT FACE occupe bien notre voie -- on ne lui
                // roule pas dessus -- mais ce n'est pas une file : c'est un face-a-face. Les
                // confondre produisait l'interblocage de ANO-5.18-03 : deux vehicules arretes nez a
                // nez se declaraient mutuellement "FollowingSameLane, leader arrete", et comme une
                // file est une attente legitime, aucun des deux ne declenchait le deblocage.
                //
                // ANO-5.18-04 : le cap oppose se juge AUSSI contre notre propre cap, pas seulement
                // contre la tangente de notre trajectoire. La trajectoire prevue relie des NOEUDS en
                // ligne droite, donc un mouvement de jonction y est une corde diagonale en travers du
                // carrefour. Un vehicule PERPENDICULAIRE arrete sur une branche transversale tombe sur
                // cette corde avec un cap oppose a sa tangente, et se declarait face-a-face.
                //
                // Mesure : le mouvement 19->17 a pour corde (7,73;33,96)->(-2;24), de tangente
                // (-0,604;-0,797). Un vehicule arrete en (2,01;25,70) cap plein NORD s'en trouve a
                // 2,17 m, soit 1,14 m d'emprise pour un couloir de 1,33 m -- dans la voie -- et son
                // cap donne -0,797 contre la tangente : face-a-face declare entre deux vehicules a
                // angle droit. Freinage d'urgence, jonction bloquee 75,4 s.
                //
                // Deux vehicules qui se croisent ne se font pas face. Le produit de leurs caps le dit
                // sans ambiguite et sans seuil nouveau : c'est le meme seuil d'alignement de voie.
                var facing = mobile && heading.sqrMagnitude > 0.01f
                    && Vector3.Dot(heading, pathTangent) < -SameLaneAlignmentDot
                    && Vector3.Dot(heading, forward) < -SameLaneAlignmentDot;
                var aligned = !mobile || heading.sqrMagnitude < 0.01f || facing
                    || Vector3.Dot(heading, pathTangent) > SameLaneAlignmentDot;

                // L'identite de voie prime la geometrie : elle repond sans seuil, et elle rattrape le
                // cas ou un leader legitime sort momentanement du couloir (courbe, changement de voie).
                var peerOnOurLane = peer != null && IsNodeOnPredictedPath(peer.CurrentLaneNode);
                var onOwnPath = !behind && (peerOnOurLane
                    || (hasClearance && laneClearance <= corridor && aligned));
                if (peerOnOurLane && hasClearance) { pathAlong = clearanceAlong; }
                else if (hasClearance && onOwnPath) { pathAlong = clearanceAlong; }

                // Trajectoire de l'AUTRE : sa voie s'il en suit une, sinon l'extrapolation de sa
                // vitesse. Un decor immobile se reduit ainsi a un point, ce qui est exactement ce
                // qu'il est -- et non a une demi-droite qui traverse la carte.
                var otherSpeed = Vector3.ProjectOnPlane(velocity, Vector3.up).magnitude;
                var otherPathCount = BuildOtherPath(peer, actorCenter, velocity, otherSpeed,
                    otherSpeed * profile.PredictionSeconds + halfLength + profile.SafetyMargin);
                var conflict = TrafficPerception.FindPathConflict(
                    ownPredictedPath, ownPredictedPathCount, ownPredictedSpeed, ownFootprint,
                    otherPredictedPath, otherPathCount, otherSpeed, actorExtents, actorRotation,
                    profile.SafetyMargin, profile.PredictionSeconds);
                var collisionThreat = conflict.Exists;
                if (collisionThreat && conflict.Time < collisionTime)
                {
                    collisionTime = conflict.Time;
                    collisionPosition = conflict.Point;
                }

                // Trois regimes, et les distinguer EST le correctif :
                //
                // - DANS notre couloir et qui va dans notre sens (ou immobile) : c'est une file ou un
                //   obstacle. L'IDM la gere en douceur par l'ecart au leader ; un arret sec y serait
                //   une reaction disproportionnee ;
                // - DANS notre couloir et qui vient SUR NOUS : c'est un face-a-face. Il ne s'arbitre
                //   pas -- laisser l'un des deux "passer en premier" serait autoriser la collision --
                //   donc les deux s'arretent, toujours ;
                // - HORS de notre couloir : c'est un croisement ou une insertion. Il s'arbitre, et
                //   exactement un des deux cede.
                // Un vehicule qui nous fait face et qui est ARRETE ne "se rapproche" pas, et c'est
                // pourtant le meme face-a-face : le cap suffit a le dire, la vitesse n'est pas requise.
                var closingHeadOn = onOwnPath
                    && (facing || Vector3.Dot(velocity, pathTangent) < -stuckSpeedThreshold);
                // Une jonction AUTHOREE a deja son arbitrage (Story 5.18) : regle de priorite, place
                // de sortie, feu, maintien a l'arret. L'arbitrer une seconde fois ici arreterait les
                // deux vehicules AVANT leur noeud de decision, et l'arbitrage authore ne verrait
                // jamais aucun revendiquant. Un face-a-face reste hors de cette exemption : aucune
                // regle de priorite n'autorise a rouler dans quelqu'un.
                // TRAVERSEE ENGAGEE. Notre mouvement de jonction nous a ete accorde et l'autre n'est
                // pas engage : c'est a LUI de ceder, pas a nous de renoncer au milieu de l'aire de
                // conflit. Le test reste antisymetrique -- l'autre nous lit comme engage et cede.
                var theyYieldToUs = committedJunctionKey > 0 && peer != null
                    && peer.CommittedJunctionKey != committedJunctionKey;
                var contested = collisionThreat && !offCarriageway && (!onOwnPath || closingHeadOn)
                    && (closingHeadOn || (!SharesAuthoredJunctionApproach(peer) && !theyYieldToUs));
                if (contested && conflict.Time <= profile.PredictionSeconds
                    && (!hasTrajectoryConflict || conflict.OwnArrival < conflictArrival))
                {
                    hasTrajectoryConflict = true;
                    headOnConflict = closingHeadOn;
                    headOnYield = closingHeadOn && MustYieldHeadOn(peer);
                    // Un face-a-face contre un vehicule A L'ARRET n'est pas un face-a-face : c'est un
                    // obstacle. Les deux se traitent pareil au FREINAGE -- on ne roule pas dedans --
                    // mais pas a la RECUPERATION. Voir la garde de <see cref="TickLocalTraffic"/>.
                    headOnStationary = closingHeadOn && otherSpeed <= stuckSpeedThreshold;
                    // Le point de RENCONTRE des deux couloirs est la ligne de cession de ce conflit.
                    // Le retenir est ce qui permet de s'y arreter au lieu de s'arreter ici.
                    conflictPoint = conflict.Point;
                    hasConflictPoint = true;
                    conflictArrival = conflict.OwnArrival;
                    conflictOtherArrival = conflict.OtherArrival;
                    // Le pair reste NOMME meme en face-a-face : refuser de l'apparier est une
                    // decision d'arbitrage, l'effacer du diagnostic n'en etait pas une.
                    conflictPeer = peer;
                    conflictIsPlayer = onFoot || playerVehicle;
                }

                // clearanceAlong pointe deja le point le plus proche de l'emprise : sa demi-longueur
                // y est incluse, elle ne se retire donc pas une seconde fois.
                var gap = onOwnPath && hasClearance
                    ? Mathf.Max(0f, clearanceAlong - frontOffset)
                    : Mathf.Max(0f, pathAlong - frontOffset - halfLength);

                // CONFLIT JOUEUR IMMEDIAT : l'unique cas ou le joueur entre nommement dans la decision.
                // Trois conditions cumulatives, et aucune n'est une prediction : il est SUR la chaussee,
                // DANS notre voie, et a portee de contact. Le seuil est l'ecart minimal authore plus la
                // marge de securite (0,75 + 0,30 m) -- la distance en deca de laquelle le modele de
                // suivi considere deja qu'il n'y a plus de place, donc pas une constante nouvelle.
                // L'ecart se mesure d'emprise a emprise, jamais de centre a centre.
                if ((onFoot || playerVehicle) && !offCarriageway && onOwnPath && hasClearance
                    && gap <= profile.MinimumGap + profile.SafetyMargin && gap < playerImmediateGap)
                {
                    playerImmediate = true;
                    playerImmediateGap = gap;
                    playerImmediateName = otherBody != null ? otherBody.gameObject.name : hit.name;
                }
                var candidate = new TrafficPerceptionCandidate(gap,
                    offset.sqrMagnitude > 0.0001f ? Vector3.SignedAngle(forward, offset, Vector3.up) : 0f,
                    Vector3.Dot(velocity, pathTangent), otherBody != null || walker != null, walker != null, true,
                    behind, pathLateral, actorCenter, velocity, actorExtents, halfWidth, actorRotation,
                    walker != null || playerVehicle, collisionThreat, onOwnPath,
                    conflict.OwnArrival, conflict.OtherArrival);
                // Tri borne : les plus proches en premier, sans collection/allocation par tick.
                var insertion = perceptionCount;
                while (insertion > 0 && (perceptionCandidates[insertion - 1].Position - center).sqrMagnitude > offset.sqrMagnitude)
                {
                    perceptionCandidates[insertion] = perceptionCandidates[insertion - 1];
                    candidateColliders[insertion] = candidateColliders[insertion - 1];
                    insertion--;
                }
                perceptionCandidates[insertion] = candidate;
                candidateColliders[insertion] = hit;
                perceptionCount++;
            }
            hasPerceivedLeader = TrafficPerception.TrySelectLeader(perceptionCandidates, perceptionCount,
                profile.PerceptionRadius, profile.PerceptionArcDegrees, out perceivedLeaderGap, out perceivedLeaderSpeed,
                out var leaderIndex, vehicleHalfExtents.x + profile.SafetyMargin);
            leaderPeer = null;
            leaderName = string.Empty;
            leaderLaneNode = -1;
            leaderIsStatic = false;
            if (leaderIndex >= 0)
            {
                var leaderCollider = candidateColliders[leaderIndex];
                if (leaderCollider != null)
                {
                    leaderPeer = leaderCollider.GetComponentInParent<NetworkedAIVehicleDriverController>();
                    if (leaderPeer == this) leaderPeer = null;
                    leaderIsStatic = leaderCollider.attachedRigidbody == null
                        && leaderCollider.GetComponentInParent<CharacterController>() == null;
                    leaderName = leaderCollider.attachedRigidbody != null
                        ? leaderCollider.attachedRigidbody.gameObject.name : leaderCollider.name;
                }

                if (leaderPeer != null) leaderLaneNode = leaderPeer.CurrentLaneNode;
            }
            var nearest = float.PositiveInfinity;
            for (var i = 0; i < perceptionCount; i++)
            {
                var candidate = perceptionCandidates[i];
                if (candidate.IsOnOwnPath && candidate.Distance < nearest)
                {
                    nearest = candidate.Distance;
                    blockingCollider = candidateColliders[i];
                }
            }
            UpdateTrajectoryConflict(profile);
            if (maneuverActive) maneuverPathSafe = ValidatePath(profile, maneuverTargets, maneuverTargetIndex, maneuverTargetCount,
                currentUnblockingAction == TrafficUnblockingAction.Reverse, currentUnblockingAction == TrafficUnblockingAction.SidewalkDetour);
        }

        private static void ReadColliderGeometry(Collider collider, out Vector3 center, out Vector3 extents, out Quaternion rotation)
        {
            if (collider is BoxCollider box)
            {
                center = box.transform.TransformPoint(box.center);
                var scale = box.transform.lossyScale;
                extents = Vector3.Scale(box.size * 0.5f, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                rotation = box.transform.rotation;
                return;
            }
            // ponytail: enveloppe conservative pour les formes non cubiques; ajouter une empreinte
            // specialisee si un futur collider concave provoque un refus de passage mesure.
            center = collider.bounds.center;
            extents = collider.bounds.extents;
            rotation = Quaternion.identity;
        }

        /// <summary>
        /// Trajectoire prevue de ce vehicule : la POLYLIGNE de voie qu'il va suivre, depuis sa position
        /// courante jusqu'a la portee demandee. Sans graphe exploitable, repli sur la tangente -- le
        /// comportement anterieur, jamais une valeur inventee.
        ///
        /// Le successeur retenu est la sortie DEJA TIREE quand l'approche de jonction en a fige une
        /// (<see cref="ResolveJunctionExit"/>), sinon le successeur le mieux aligne sur le cap courant.
        /// Se tromper de branche ne cree pas de faux positif : cela fait manquer un conflit, que
        /// l'arbitrage de jonction de la Story 5.18 couvre deja sur les approches authorees.
        /// </summary>
        private void BuildPathFromLaneGraph(Vector3[] destination, float reach, out int count, int[] nodes = null)
        {
            count = 0;
            if (destination == null || destination.Length < 2) return;
            if (nodes != null) for (var i = 0; i < nodes.Length; i++) nodes[i] = -1;
            destination[count++] = transform.position;
            var heading = ResolvePlanarForward();
            var travelled = 0f;
            var node = laneGraph != null && state != null ? state.WaypointIndex.Value : -1;
            var previous = -1;
            while (count < destination.Length && travelled < reach && laneGraph != null && laneGraph.IsValidIndex(node))
            {
                // UN MOUVEMENT DE JONCTION SE PARCOURT D'UN SEUL TENANT, de la frontiere de la
                // jonction jusqu'a la sortie. Le noeud de decision est un point INTERIEUR au virage,
                // pas une etape ou l'on arrive droit pour repartir droit.
                //
                // Mesure ANO-5.18-04 : partant du noeud de decision, le coin des deux axes de voie
                // n'est plus qu'a 2,00 m, ce qui exige un rayon de 0,76 m. Le vehicule ne descend pas
                // sous 3,69 m, meme au pas (empattement 3,10 m, braquage 40 deg). La trajectoire visee
                // etait donc physiquement impossible sur les 24 virages du district, et le vehicule
                // sortait 2,2 a 3,3 m a cote. Depuis la FRONTIERE, le coin est a 10,00 m et le meme
                // virage demande 4,24 m -- tenable.
                //
                // C'est aussi, exactement, le mouvement que l'arbitrage de jonction revendique
                // (CommitJunctionTraversal) : conduire et arbitrer lisent enfin la meme courbe.
                if (TryEmitJunctionMovement(destination, ref count, nodes, node, heading,
                        ref travelled, out var afterJunction, out var junctionHeading))
                {
                    heading = junctionHeading;
                    previous = afterJunction;
                    node = PredictSuccessor(afterJunction, heading);
                    continue;
                }

                var point = laneGraph.GetNodePosition(node);
                point.y = transform.position.y;
                var from = destination[count - 1];
                var before = count;

                if (previous < 0)
                {
                    // Rejoindre la voie. Le vehicule n'est pas encore dessus, et rien n'autorise a
                    // courber ce raccord : il n'a pas de cap authore, seulement le sien.
                    if (Vector3.ProjectOnPlane(point - from, Vector3.up).sqrMagnitude > 0.0001f)
                    {
                        destination[count++] = point;
                    }
                }
                else
                {
                    count = LaneGraphRouting.SampleLaneEdge(from, ResolveNodeForward(previous),
                        point, ResolveNodeForward(node), LanePathSampleStep, destination, count);
                }

                if (count > before)
                {
                    for (var i = before; i < count; i++)
                    {
                        destination[i].y = transform.position.y;
                        travelled += Vector3.ProjectOnPlane(destination[i] - destination[i - 1], Vector3.up).magnitude;
                    }

                    var tangent = Vector3.ProjectOnPlane(destination[count - 1] - destination[count - 2], Vector3.up);
                    if (tangent.sqrMagnitude > 0.0001f) heading = tangent.normalized;

                    // Le noeud se marque sur le DERNIER echantillon de son arete : c'est lui qui porte
                    // la position du noeud, les precedents sont interieurs a la courbe.
                    if (nodes != null && count - 1 < nodes.Length) nodes[count - 1] = node;
                    previous = node;
                }

                node = PredictSuccessor(node, heading);
            }

            // Un seul point ne decrit aucune trajectoire : prolonger sur la tangente jusqu'a la portee.
            if (count < 2) destination[count++] = transform.position + heading * Mathf.Max(0.01f, reach);
        }

        /// <summary>
        /// Emet le connecteur complet d'un mouvement de jonction -- frontiere -> sortie -- et rend vrai
        /// quand il l'a fait. La frontiere et son cap viennent de l'approche INDEXEE A LA CONSTRUCTION
        /// du graphe, donc ce sont les memes valeurs que celles revendiquees a l'arbitrage.
        ///
        /// Quand le vehicule a deja depasse la frontiere, la courbe est emise telle quelle : ses
        /// premiers points tombent derriere lui, et la visee les ignore puisqu'elle se REPROJETTE sur
        /// le point le plus proche. Recommencer la courbe depuis la position courante la resserrerait
        /// a chaque pas, ce qui est precisement le defaut a ne pas reintroduire.
        /// </summary>
        private bool TryEmitJunctionMovement(Vector3[] destination, ref int count, int[] nodes,
            int node, Vector3 heading, ref float travelled, out int exitNode, out Vector3 exitHeading)
        {
            exitNode = -1;
            exitHeading = heading;
            if (!laneGraph.TryGetJunctionApproach(node, out var approach) || approach.JunctionKey <= 0) return false;

            var exit = PredictSuccessor(node, heading);
            if (!laneGraph.IsValidIndex(exit) || exit == node) return false;

            var entryForward = Vector3.ProjectOnPlane(approach.EntryForward, Vector3.up);
            if (entryForward.sqrMagnitude <= 0.0001f) entryForward = ResolveNodeForward(node);
            if (entryForward.sqrMagnitude <= 0.0001f) return false;
            entryForward.Normalize();

            var entryPoint = approach.EntryNode == node ? laneGraph.GetNodePosition(node) : approach.EntryPoint;
            entryPoint.y = transform.position.y;
            var exitPoint = laneGraph.GetNodePosition(exit);
            exitPoint.y = transform.position.y;
            var exitForward = ResolveNodeForward(exit);
            if (exitForward.sqrMagnitude <= 0.0001f) return false;

            var before = count;
            count = LaneGraphRouting.SampleLaneEdge(entryPoint, entryForward, exitPoint, exitForward,
                LanePathSampleStep, destination, count);
            if (count <= before) return false;

            for (var i = before; i < count; i++)
            {
                destination[i].y = transform.position.y;
                travelled += Vector3.ProjectOnPlane(destination[i] - destination[i - 1], Vector3.up).magnitude;
            }

            var tangent = Vector3.ProjectOnPlane(destination[count - 1] - destination[count - 2], Vector3.up);
            if (tangent.sqrMagnitude > 0.0001f) exitHeading = tangent.normalized;

            // Le noeud de decision est TRAVERSE par ce mouvement : la trajectoire l'emprunte, donc
            // l'identite de voie doit le porter, sinon un suiveur legitime cesse d'etre reconnu.
            if (nodes != null)
            {
                if (before < nodes.Length) nodes[before] = node;
                if (count - 1 < nodes.Length) nodes[count - 1] = exit;
            }

            exitNode = exit;
            return true;
        }

        /// <summary>Cap AUTHORE d'un noeud : l'axe de sa voie, et non la corde qui y mene.</summary>
        private Vector3 ResolveNodeForward(int node)
        {
            if (laneGraph == null || !laneGraph.IsValidIndex(node)) return Vector3.zero;
            var forward = Vector3.ProjectOnPlane(laneGraph.GetNodeRotation(node) * Vector3.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.zero;
        }

        /// <summary>
        /// Vrai quand ce vehicule ET le pair approchent la MEME jonction authoree. C'est la frontiere
        /// de responsabilite entre les deux couches : la trajectoire arbitre la route ouverte, la
        /// regle authoree arbitre la jonction. Les laisser arbitrer toutes les deux ne donnait pas
        /// une double securite, mais un blocage en amont du noeud de decision.
        /// </summary>
        private bool SharesAuthoredJunctionApproach(NetworkedAIVehicleDriverController peer)
        {
            if (peer == null || laneGraph == null || state == null
                || peer.laneGraph == null || peer.state == null) return false;
            if (!laneGraph.TryGetJunctionApproach(state.WaypointIndex.Value, out var own) || own.JunctionKey <= 0) return false;
            return peer.laneGraph.TryGetJunctionApproach(peer.state.WaypointIndex.Value, out var other)
                && other.JunctionKey == own.JunctionKey;
        }

        private int PredictSuccessor(int node, Vector3 heading)
        {
            if (node == plannedJunctionNode && plannedJunctionExit >= 0 && plannedJunctionExit != node)
                return plannedJunctionExit;
            var successors = laneGraph.GetSuccessors(node);
            var from = laneGraph.GetNodePosition(node);
            var best = -1;
            var bestDot = float.NegativeInfinity;
            for (var i = 0; i < successors.Count; i++)
            {
                var direction = Vector3.ProjectOnPlane(laneGraph.GetNodePosition(successors[i]) - from, Vector3.up);
                if (direction.sqrMagnitude <= 0.0001f) continue;
                var alignment = Vector3.Dot(direction.normalized, heading);
                if (alignment > bestDot) { bestDot = alignment; best = successors[i]; }
            }

            return best;
        }

        /// <summary>
        /// Trajectoire prevue d'un AUTRE usager. Un pair IA rend sa polyligne de voie ; tout le reste
        /// -- joueur, pieton, decor -- est extrapole depuis sa vitesse mesuree, ce qui reduit un objet
        /// immobile a un point unique. Un point unique ne peut entrer en conflit qu'avec une
        /// trajectoire qui le traverse reellement : c'est ce qui rend un joueur gare a cote sans objet.
        /// </summary>
        private int BuildOtherPath(NetworkedAIVehicleDriverController peer, Vector3 actorCenter,
            Vector3 velocity, float speed, float reach)
        {
            if (peer != null && peer.IsSpawned)
            {
                var count = peer.CopyPredictedPath(otherPredictedPath, reach);
                if (count >= 2) return count;
            }

            otherPredictedPath[0] = actorCenter;
            if (speed < 0.01f) return 1;
            otherPredictedPath[1] = actorCenter
                + Vector3.ProjectOnPlane(velocity, Vector3.up).normalized * Mathf.Max(0.01f, reach);
            return 2;
        }

        /// <summary>
        /// Story 5.18 (correctif) : appariement et arbitrage d'un conflit de TRAJECTOIRE.
        ///
        /// Ce que cette fonction remplace tenait en une ligne : "un autre vehicule me fait face dans ma
        /// bande avant, donc nous sommes en conflit". Elle n'interrogeait ni les voies, ni les
        /// trajectoires, ni le temps -- elle appariait deux vehicules qui se croisent normalement, et
        /// l'appariement condamnait ensuite l'un des deux a ne plus pouvoir escalader.
        ///
        /// Desormais l'appariement suppose un conflit REELLEMENT predit (occupations qui se recouvrent
        /// sur un point commun), et le verdict sort de <see cref="TrafficPerception.YieldsAtConflict"/> :
        /// celui qui atteint le point de conflit le plus tard cede. Trois proprietes en decoulent :
        ///
        /// 1. l'arbitrage est ANTISYMETRIQUE et pose atomiquement des deux cotes (IA host-only), donc
        ///    "les deux attendent l'autre" est impossible pour une paire ;
        /// 2. un vehicule n'a qu'UN partenaire a la fois, et un partenaire deja apparie n'est pas
        ///    vole : aucun cycle d'attente ne peut donc se former ;
        /// 3. un vehicule deja engage sur un anneau de giratoire atteint le point de conflit avant
        ///    celui qui approche l'entree, donc il passe -- la priorite a l'anneau sans regle dediee.
        ///
        /// ponytail: un seul partenaire arbitre a la fois. Un troisieme usager en conflit plus tardif
        /// est traite au tour de perception suivant, quand il devient le plus proche ; passer a un
        /// arbitrage N-aire ne se justifierait qu'avec une mesure de collisions reelles.
        /// </summary>
        private void UpdateTrajectoryConflict(DriverProfile profile)
        {
            if (conflictDriver != null)
            {
                var stillFacing = conflictDriver.IsSpawned && conflictDriver == conflictPeer && hasTrajectoryConflict;
                if (stillFacing) return;
                ReleaseTrajectoryConflict();
            }

            if (!hasTrajectoryConflict) return;

            // UN CONFLIT A TOUJOURS UN VERDICT. Il se calcule AVANT l'appariement parce qu'il n'en
            // depend pas : le predicat est pur. Sans cela, un vehicule dont le pair arbitrait deja
            // avec un troisieme restait sans verdict, et la decision retombait sur "je cede" --
            // definitivement, le relachement exigeant lui aussi un appariement.
            //
            // Mesure ANO-5.18-04 : Portal_011 a cede 51,1 s "le point de conflit" a Portal_017, qui
            // etait DERRIERE lui a 0,8 m et le suivait ; Portal_002 est reste 27,3 s a l'arret avec
            // la permission de jonction ACCORDEE. Les deux lisaient "verdict=AUCUN".
            //
            // L'appariement garde son role : il pose le verdict des DEUX cotes d'un coup, donc il
            // reste la voie nominale et l'antisymetrie y est garantie. Le calcul local n'est que le
            // repli du cas a trois, ou "les deux passent" reste possible -- c'est moins grave que
            // "les deux cedent pour toujours", et le suivi de file comme le freinage d'urgence
            // restent en dessous.
            conflictYield = ResolveConflictYield();

            if (headOnConflict || conflictPeer == null || !conflictPeer.IsSpawned
                || conflictPeer.conflictDriver != null) return;

            conflictDriver = conflictPeer;
            yieldElapsedSeconds = 0f;
            yieldReleased = false;
            // Decision atomique cote hote : les deux instances gardent le meme verdict jusqu'a separation.
            conflictPeer.conflictDriver = this;
            conflictPeer.conflictYield = !conflictYield;
            conflictPeer.yieldElapsedSeconds = 0f;
            conflictPeer.yieldReleased = false;
        }

        /// <summary>
        /// Cedons-nous CE conflit ? Un usager non arbitrable (joueur, pieton, decor) passe toujours :
        /// ce n'est pas au joueur de deviner. Sinon le predicat pur departage, et il traite deja
        /// l'arrivee infinie d'un vehicule a l'arret comme la plus tardive de toutes.
        /// </summary>
        private bool ResolveConflictYield()
        {
            if (conflictPeer == null || !conflictPeer.IsSpawned)
            {
                conflictVerdictReason = "usager non arbitrable";
                return true;
            }

            // UN PAIR QUI N'ARRIVERA JAMAIS NE PEUT PAS ETRE PRIORITAIRE (ANO-5.18-11).
            //
            // Une arrivee infinie ne dit pas "il passe en premier", elle dit "il est a l'arret".
            // C'est le cas de recette : "cede le point de conflit, arrivee=0,00s vs +Infini" -- le
            // pair etait un vehicule immobilise dans un bouchon, et la priorite a l'anneau, evaluee
            // AVANT toute autre consideration, la lui accordait pour toujours. L'ordre d'arrivee,
            // lui, traitait deja correctement le cas ; c'est la regle d'anneau qui le court-circuitait.
            //
            // Ne plus ceder ne fait pas rouler dedans : le suivi de file, la ligne de cession et le
            // freinage d'urgence restent en dessous. Cela transforme une attente SANS ISSUE en un
            // blocage -- c'est-a-dire en quelque chose dont l'echelle de recuperation sait sortir.
            var weArrive = float.IsFinite(conflictArrival);
            var peerArrives = float.IsFinite(conflictOtherArrival);
            if (weArrive != peerArrives)
            {
                conflictVerdictReason = peerArrives
                    ? "nous sommes a l'arret, le pair avance"
                    : "pair a l'arret : il n'atteindra jamais le point";
                return peerArrives;
            }

            // LA PRIORITE A L'ANNEAU PASSE AVANT L'ORDRE D'ARRIVEE. Un giratoire n'est pas une
            // course : celui qui y circule passe, meme si l'entrant atteindrait le point de conflit
            // en premier -- ce que l'ordre d'arrivee seul lui accordait. La donnee d'anneau est
            // indexee une fois sur le graphe et lue a l'identique par les deux pairs, donc le
            // verdict reste antisymetrique.
            var precedence = JunctionRules.CompareRingPrecedence(RingId, conflictPeer.RingId);
            if (precedence != 0)
            {
                conflictVerdictReason = precedence < 0
                    ? "anneau " + conflictPeer.RingId + " : nous entrons"
                    : "anneau " + RingId + " : nous y circulons";
                return precedence < 0;
            }

            conflictVerdictReason = Mathf.Abs(conflictArrival - conflictOtherArrival) > 0.25f
                ? "ordre d'arrivee" : "arrivees egales : plus petit identifiant";
            return TrafficPerception.YieldsAtConflict(conflictArrival, NetworkObjectId,
                conflictOtherArrival, conflictPeer.NetworkObjectId);
        }

        /// <summary>Motif LISIBLE du dernier verdict de cession. Un verdict sans son motif ne se
        /// verifie pas en recette : c'est ce qui a rendu "cede le point de conflit" inexplicable.</summary>
        public string ConflictVerdictReason => conflictVerdictReason;

        /// <summary>Palier de recuperation courant, 0 quand la conduite est nominale.</summary>
        public int RecoveryStage => recoveryStage;

        /// <summary>Duree (s) passee a contresens en conduite nominale. 0 pendant une manoeuvre.</summary>
        public float WrongWaySeconds => wrongWayElapsedSeconds;

        /// <summary>Le cap suit-il le sens de circulation de l'axe le plus proche ?</summary>
        public bool LaneDirectionCorrect => laneDirectionCorrect;

        /// <summary>Ecart (m) a l'axe routier le plus proche, tel que la semantique LaneGraph le mesure.</summary>
        public float LaneAxisDistance => laneAxisDistance;

        /// <summary>Duree (s) sans progres le long de la route. Remise a zero par tout progres reel.</summary>
        public float StalledSeconds => stalledElapsedSeconds;

        /// <summary>Vrai quand l'attente courante est REGULIERE ET BORNEE (feu rouge, ou file derriere
        /// un vehicule lui-meme retenu par un feu). L'horloge de blocage ne tourne pas dans cet etat.</summary>
        public bool HeldByBoundedWait => heldByBoundedWait;

        private int previousLaneNode = -1;

        /// <summary>
        /// Anneau de circulation ou ce vehicule est ENGAGE, 0 sinon.
        ///
        /// Etre engage suppose DEUX PIEDS sur l'anneau -- le noeud quitte et le noeud vise. Le seul
        /// noeud vise ne suffit pas : un vehicule qui vient d'atteindre le noeud d'entree vise deja
        /// l'anneau alors qu'il est encore sur la ligne de cession. Il se declarerait prioritaire au
        /// moment precis ou il doit ceder, et entrerait dans le flux au lieu de le laisser passer.
        /// </summary>
        public int RingId
        {
            get
            {
                if (laneGraph == null || state == null) return 0;
                var ring = laneGraph.RingIdOf(state.WaypointIndex.Value);
                return ring != 0 && laneGraph.RingIdOf(previousLaneNode) == ring ? ring : 0;
            }
        }

        private void ReleaseTrajectoryConflict()
        {
            var other = conflictDriver;
            conflictDriver = null;
            yieldElapsedSeconds = 0f;
            yieldReleased = false;
            if (other != null && other.conflictDriver == this)
            {
                other.conflictDriver = null;
                other.yieldElapsedSeconds = 0f;
                other.yieldReleased = false;
            }
        }

        private bool TickLocalTraffic(DriverProfile profile, float dt, float speed, float longitudinalSpeed)
        {
            conflictSpeedLimitGap = float.PositiveInfinity;
            if (maneuverActive)
            {
                maneuverElapsed += dt;
                noProgressElapsed += dt;
                var distance = Vector3.ProjectOnPlane(ManeuverTarget - transform.position, Vector3.up).magnitude;
                if (bestTargetDistance - distance >= profile.ProgressDistance)
                {
                    bestTargetDistance = distance;
                    noProgressElapsed = 0f;
                }
                var reverse = currentUnblockingAction == TrafficUnblockingAction.Reverse;
                if (reverse && longitudinalSpeed < -physicsBody.Profile.MinimumDirectionSpeed) reverseTravelSeconds += dt;
                if (distance <= profile.PathSampleDistance || (reverse && reverseTravelSeconds >= profile.ReverseDuration))
                {
                    if (++maneuverTargetIndex >= maneuverTargetCount)
                    {
                        maneuverActive = false;
                        if (reverse)
                        {
                            currentUnblockingAction = TrafficUnblockingAction.Replan;
                        }
                        else
                        {
                            RejoinRouteAfterManeuver();
                            if (HasRecoveredRoute(profile))
                            {
                                ResetUnblockingEpisode(profile);
                            }
                            else
                            {
                                CancelManeuver("retour hors voie ou a contresens", profile);
                            }
                        }
                        SubmitIntentToPhysicsLayer(ResolveStopIntent(longitudinalSpeed));
                        return true;
                    }
                    bestTargetDistance = Vector3.Distance(transform.position, ManeuverTarget);
                    noProgressElapsed = 0f;
                }
                if (maneuverElapsed >= profile.ManeuverTimeout || noProgressElapsed >= profile.ProgressTimeout)
                    CancelManeuver("aucun progres / delai", profile);
                if (!maneuverActive || !maneuverPathSafe || perceptionSaturated)
                {
                    SubmitIntentToPhysicsLayer(ResolveStopIntent(longitudinalSpeed));
                    return true;
                }
                // La trajectoire validee prime le risque de la trajectoire abandonnee (ex. recul separant).
                var targetSpeed = reverse ? profile.ReverseSpeed : profile.ManeuverSpeed;
                VehicleDriveIntent pedal;
                if (reverse)
                {
                    var error = targetSpeed + longitudinalSpeed;
                    EnsureDriveCapacity();
                    var available = driveAccelerationCapacity * physicsBody.Profile.ReverseTorque / Mathf.Max(0.0001f, physicsBody.Profile.EngineTorque);
                    var acceleration = (error * profile.MaxAcceleration + body.linearDamping * Mathf.Max(0f, -longitudinalSpeed))
                        / Mathf.Max(0.01f, 1f - body.linearDamping * Time.fixedDeltaTime);
                    pedal = longitudinalSpeed > physicsBody.Profile.MinimumDirectionSpeed ? ResolveStopIntent(longitudinalSpeed)
                        : acceleration > 0f ? new VehicleDriveIntent(0f, 0f, Mathf.Clamp01(acceleration / Mathf.Max(available, 0.0001f)), 0f)
                        : ResolveStopIntent(longitudinalSpeed);
                }
                else pedal = ResolvePedalIntent((targetSpeed - speed) * profile.MaxAcceleration, longitudinalSpeed);
                var steer = reverse ? 0f : ComputeSeekIntent(transform.position, transform.forward, ManeuverTarget, steerFullLockDegrees).Steer;
                SubmitIntentToPhysicsLayer(new VehicleDriveIntent(pedal.Throttle, steer, pedal.BrakeReverse, pedal.Handbrake));
                return true;
            }
            // ANO-5.18-02 : un leader arrete ne prouve pas que NOUS sommes bloques. S'il attend
            // lui-meme pour une raison reguliere -- feu, priorite, file, sortie occupee -- nous sommes
            // dans une QUEUE, et une queue se resout toute seule des que sa tete avance. La traiter
            // comme un blocage declenchait l'echelle de deblocage au bout du delai de klaxon, puis la
            // manoeuvre ramenait le vehicule derriere le meme leader, indefiniment.
            //
            // Le predicat ne regarde donc pas la vitesse mais la CAUSE, lue chez le leader lui-meme.
            // Un obstacle immobile, un leader hors file ou un leader lui-meme bloque restent des
            // blocages : l'echelle de la Story 5.17 conserve tous ses cas d'usage.
            // Un vehicule qui NOUS FAIT FACE n'est jamais une tete de file, quelle que soit la
            // regularite de SON attente. Une file se resout quand sa tete avance ; celui-la, en
            // avancant, entre dans nous. Le nommer autrement (ANO-5.18-03) ne suffisait pas : le
            // predicat de file, lui, le lisait encore comme une attente legitime -- donc aucun des
            // deux n'entrait dans l'echelle de deblocage, et le blocage tenait jusqu'au garde-fou
            // de file immobile. C'est le cas de recette ANO-5.18-07 : un vehicule arrete a son stop
            // en travers du virage d'un prioritaire, permission de jonction refusee des deux cotes.
            var rawQueueing = hasPerceivedLeader && !headOnConflict && IsLegitimateWait(leaderPeer);
            var queueing = rawQueueing;

            // Une file legitime qui n'avance JAMAIS n'est plus legitime. Le cas existe reellement et il
            // est cree par la correction elle-meme : sur un anneau ferme, A peut suivre B, B suivre C
            // et C suivre A -- chacun voit devant lui une attente reguliere, et plus personne ne
            // declencherait l'echelle de deblocage. Le garde-fou ne remplace pas la decision nominale,
            // il borne le seul cas ou celle-ci ne peut pas conclure. Le delai est celui deja authore
            // pour l'escalade de jonction, pas une valeur nouvelle.
            queueElapsedSeconds = queueing && speed <= stuckSpeedThreshold ? queueElapsedSeconds + dt : 0f;
            if (queueElapsedSeconds >= profile.JunctionEscalationDelay)
            {
                queueing = false;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (!warnedQueueStalled)
                {
                    warnedQueueStalled = true;
                    Debug.LogWarning("[Vehicles] " + name + " : file immobile depuis "
                        + queueElapsedSeconds.ToString("0.0") + " s derriere " + Describe(leaderName)
                        + " -- attente circulaire probable. L'echelle de deblocage reprend la main.", this);
                }
#endif
            }
            // Le joueur en travers a portee de contact ne se traite pas par l'arret : l'IA cherche
            // une echappatoire laterale et contourne. C'est l'echelle de deblocage deja en place
            // (Story 5.17) qui l'execute, appelee sans attendre le delai de klaxon -- attendre deux
            // secondes le nez contre un pieton n'aurait aucun sens. Si aucune sortie n'est validee,
            // le repli reste l'arret : on ne roule pas sur un joueur faute de place.
            if (playerImmediate && refreshedThisStep && !maneuverActive && Time.time >= retryAfter)
            {
                if (TryStartDetour(profile, false) || TryStartDetour(profile, true))
                {
                    decisionReason = TrafficDecisionReason.PlayerImmediateConflict;
                    decisionSubject = playerImmediateName;
                    trafficRefusal = "evitement du joueur";
                    return DriveOnNextStep(longitudinalSpeed);
                }

                retryAfter = Time.time + profile.RetryCooldown;
            }

            // Le face-a-face reste hors de l'echelle de deblocage tant que l'autre ROULE : deux
            // vehicules qui se rapprochent ne se contournent pas, ils s'arretent. Des qu'il est
            // A L'ARRET, ce n'est plus un face-a-face mais un obstacle, et un obstacle se contourne
            // -- c'est la manoeuvre demandee en recette pour le vehicule prioritaire dont la
            // trajectoire de virage est occupee par un vehicule arrete a son stop. Le contournement
            // reste valide par ValidatePath : sans place sure, le repli demeure l'arret.
            // UNE ATTENTE BORNEE N'EST PAS UN BLOCAGE. Un feu rouge finit par passer au vert, et
            // une file derriere un vehicule retenu par un feu avance avec lui. L'etat se propage
            // d'un cran par pas le long de la file, comme IsLegitimateWait : sans cela, un vehicule
            // correctement arrete a un feu partirait en contournement au bout du delai de klaxon.
            heldByBoundedWait = junctionHeldBySignal
                || (rawQueueing && leaderPeer != null && leaderPeer.heldByBoundedWait);

            // PROGRES DE ROUTE : la mesure unique dont depend toute la recuperation. Le vehicule
            // progresse quand il se rapproche de son waypoint ; tout le reste -- file qui n'avance
            // plus, cession sans issue, face-a-face, enchevetrement -- est un arret, et un arret qui
            // dure est un blocage, quel que soit son NOM.
            // CONTRESENS EN CONDUITE NOMINALE. Sans cette mesure, un vehicule a contresens qui
            // ROULE ne declenche aucune horloge : il progresse vers son waypoint, donc l'echelle de
            // recuperation ne le voit jamais. C'est la cause mesuree des 49,8 s et 53,1 s de
            // circulation a l'envers du soak -- le contresens n'etait constate qu'au moment d'un
            // face-a-face ou en fin de manoeuvre, c'est-a-dire trop tard et par accident.
            TickLaneConformance(profile, dt);
            TickRouteProgress(profile, dt);

            var blocked = hasPerceivedLeader && !queueing
                && (!headOnConflict || headOnYield || headOnStationary)
                && perceivedLeaderSpeed <= stuckSpeedThreshold
                && (blockedElapsedSeconds > 0f || perceivedLeaderGap <= profile.MinimumGap + profile.SafetyMargin || speed <= stuckSpeedThreshold);
            if (!blocked)
            {
                blockedElapsedSeconds = 0f;
                if (unblockingEpisodeActive)
                {
                    if (HasRecoveredRoute(profile)) ResetUnblockingEpisode(profile);
                }
                else
                {
                    reverseAttempted = false;
                    hornReported = false;
                    replanAttemptedForBlock = false;
                }
                currentUnblockingAction = TrafficUnblockingAction.FollowRoute;
            }
            else
            {
                blockedElapsedSeconds += dt;
                // Story 5.18 : le palier de deblocage d'intersection est en TETE de l'echelle -- il ne
                // fait qu'assouplir une regle que le vehicule respectait. Les paliers 5.17 (Replan,
                // Reverse, RoadDetour, SidewalkDetour) restent en dessous, et aucun ne retire le
                // vehicule : il n'existe ni despawn, ni teleportation dans cette echelle.
                currentUnblockingAction = junctionBreached
                    ? TrafficUnblockingAction.IntersectionBreach
                    : TrafficUnblockingAction.Replan;
            }

            // L'ECHELLE DE RECUPERATION EST UNIQUE, ET SON DECLENCHEUR AUSSI.
            //
            // Elle n'etait atteignable que par le predicat de FILE. Les trois blocages rapportes en
            // recette -- face-a-face, cession a un pair immobile, enchevetrement apres contact --
            // n'y entraient donc jamais : ils restaient arretes indefiniment, chacun pour une raison
            // nommee differemment mais avec le meme resultat. Le declencheur est desormais l'absence
            // de progres, qui les couvre tous les quatre sans en nommer aucun.
            if (TryEscalateRecovery(profile, longitudinalSpeed)) return true;
            // Story 5.18 (correctif) : l'EXISTENCE d'un conflit ne commande plus l'arret a elle seule.
            // Un conflit arbitre dont le pair nous cede se traverse, sinon les deux attendraient --
            // c'est precisement l'interblocage que le retour terrain decrit dans les giratoires.
            // Un conflit avec un usager NON arbitrable (joueur, pieton, decor) commande toujours
            // l'arret : c'est a l'IA de ceder, jamais au joueur de deviner.
            // Le VERDICT commande, plus l'appariement. Le face-a-face n'est pas un arbitrage --
            // laisser l'un des deux passer autoriserait la collision -- et reste traite a part.
            var mustYield = hasTrajectoryConflict && !headOnConflict && conflictYield && !yieldReleased;
            TickYieldRelease(profile, dt, mustYield);
            mustYield = hasTrajectoryConflict && !headOnConflict && conflictYield && !yieldReleased;

            // UNE CESSION A UNE LIGNE (ANO-5.18-08). Ceder, ce n'est pas s'arreter LA OU L'ON
            // APERCOIT le conflit : c'est ne pas ENTRER dedans. La difference se voit en recette --
            // "les voitures cedent leur point de conflit trop tot au milieu des troncons de route et
            // pas juste devant l'intersection" -- et elle est entiere dans ce bloc : la cession
            // posait un freinage d'urgence a la position courante, jusqu'a la portee de prediction
            // en amont. Les jonctions AUTHOREES ne connaissaient pas ce defaut, elles ont leur ligne
            // d'arret ; les giratoires du district n'en portent aucune, ce qui explique qu'ils
            // concentrent le symptome.
            //
            // Reduire la perception aurait deplace le symptome sans le corriger : il faut voir loin
            // pour freiner doux. Ce qui manquait n'etait pas une portee plus courte, c'etait la
            // cible d'arret. Le ralentissement passe donc par le meme canal que la jonction -- un
            // leader immobile virtuel pose sur la ligne -- donc par l'IDM, donc progressif, et le
            // vehicule garde sa direction pendant l'approche au lieu de braquer a zero.
            yieldingTrajectory = mustYield;
            var conflictStopGap = float.PositiveInfinity;
            var approachingYield = mustYield && TryResolveConflictStopGap(profile, out conflictStopGap);
            if (approachingYield) conflictSpeedLimitGap = conflictStopGap;

            if (blocked || perceptionSaturated || (mustYield && !approachingYield) || headOnConflict
                || (playerImmediate && !maneuverActive))
            {
                appliedAcceleration = 0f;
                if (perceptionSaturated)
                {
                    decisionReason = TrafficDecisionReason.PerceptionSaturated;
                    decisionSubject = string.Empty;
                    trafficRefusal = "perception saturee";
                }
                else if (playerImmediate)
                {
                    // Aucune echappatoire validee : l'arret est le repli, pas la reponse nominale.
                    decisionReason = TrafficDecisionReason.PlayerImmediateConflict;
                    decisionSubject = playerImmediateName;
                    trafficRefusal = "joueur en travers, aucun contournement sur";
                }
                else if (headOnConflict)
                {
                    // Nomme AVANT la file : un vehicule qui nous fait face n'est pas une tete de file,
                    // et l'appeler ainsi rendait l'attente reciproque legitime des deux cotes.
                    decisionReason = TrafficDecisionReason.EmergencyBrake;
                    decisionSubject = leaderName;
                    trafficRefusal = "face-a-face dans la voie";
                }
                else if (blocked)
                {
                    decisionReason = leaderIsStatic
                        ? TrafficDecisionReason.StaticObstacle : TrafficDecisionReason.FollowingSameLane;
                    decisionSubject = leaderName;
                    trafficRefusal = leaderIsStatic ? "obstacle immobile dans la voie" : "file : leader arrete";
                }
                else
                {
                    decisionReason = headOnConflict ? TrafficDecisionReason.EmergencyBrake
                        : conflictIsPlayer ? TrafficDecisionReason.PlayerCollisionRisk
                        : TrafficDecisionReason.YieldTrajectory;
                    decisionSubject = conflictPeer != null ? conflictPeer.name : string.Empty;
                    trafficRefusal = headOnConflict ? "face-a-face dans la voie"
                        : conflictIsPlayer ? "trajectoire du joueur en conflit"
                        : "cede le point de conflit";
                }

                ComposeDecisionDetail();
                SubmitIntentToPhysicsLayer(ResolveStopIntent(longitudinalSpeed));
                return true;
            }

            // Une file LEGITIME est un etat nomme, pas un blocage : c'est ce qui la distingue, dans le
            // diagnostic comme dans l'echelle de deblocage.
            decisionReason = approachingYield ? TrafficDecisionReason.YieldTrajectory
                : yieldReleased ? TrafficDecisionReason.DeadlockRecovery
                : queueing ? TrafficDecisionReason.TrafficQueue
                : hasPerceivedLeader ? TrafficDecisionReason.FollowingSameLane
                : TrafficDecisionReason.Cruise;
            decisionSubject = approachingYield ? (conflictPeer != null ? conflictPeer.name : string.Empty)
                : hasPerceivedLeader ? leaderName : string.Empty;
            if (approachingYield) trafficRefusal = "cede le point de conflit (ralentit vers la ligne)";
            else if (decisionReason != TrafficDecisionReason.DeadlockRecovery) trafficRefusal = string.Empty;
            ComposeDecisionDetail();
            return false;
        }

        /// <summary>
        /// Trace lisible exigee par ANO-5.18 : un motif seul ne se verifie pas. La ligne nomme le
        /// SUJET de la decision (leader, obstacle, joueur, pair en conflit) et les grandeurs qui l'ont
        /// produite, de sorte qu'un arret force soit toujours attribuable a un objet identifie.
        ///
        /// ponytail: une chaine composee a la cadence de perception (5 Hz), pas par frame, et lue par
        /// le libelle debug existant. Aucun cadre de diagnostic permanent n'est introduit.
        /// </summary>
        private void ComposeDecisionDetail()
        {
            switch (decisionReason)
            {
                case TrafficDecisionReason.FollowingSameLane:
                case TrafficDecisionReason.TrafficQueue:
                    decisionDetail = "leader=" + Describe(leaderName) + " voie=" + leaderLaneNode
                        + "/" + CurrentLaneNode + " ecart=" + perceivedLeaderGap.ToString("0.0") + "m"
                        + " vLeader=" + perceivedLeaderSpeed.ToString("0.0")
                        + " attente=" + queueElapsedSeconds.ToString("0.0") + "s";
                    break;
                case TrafficDecisionReason.StaticObstacle:
                    decisionDetail = "objet=" + Describe(leaderName) + " ecart=" + perceivedLeaderGap.ToString("0.0")
                        + "m source=perception/voie=" + CurrentLaneNode;
                    break;
                case TrafficDecisionReason.ExitSaturated:
                    decisionDetail = "jonction=" + junctionDiagnosticId + " sortie=" + plannedJunctionExit
                        + " libre=" + junctionExitFreeLength.ToString("0.00") + "m requis="
                        + junctionExitRequiredLength.ToString("0.00") + "m utile="
                        + junctionExitUsableLength.ToString("0.00") + "m occupants=" + junctionExitOccupants
                        + " premier=" + Describe(exitSaturatedBy);
                    break;
                case TrafficDecisionReason.JunctionYield:
                case TrafficDecisionReason.JunctionBreach:
                    decisionDetail = "jonction=" + junctionDiagnosticId + " mvt=" + CurrentLaneNode + "->"
                        + plannedJunctionExit + " permission=" + Describe(junctionPermission)
                        + " revendications=" + claimantCount + " arret_dans="
                        + junctionStopGap.ToString("0.00") + "m";
                    break;
                case TrafficDecisionReason.PlayerImmediateConflict:
                    decisionDetail = "joueur=" + Describe(playerImmediateName) + " ecart="
                        + playerImmediateGap.ToString("0.00") + "m relation=devant/voie";
                    break;
                case TrafficDecisionReason.YieldTrajectory:
                case TrafficDecisionReason.EmergencyBrake:
                case TrafficDecisionReason.PlayerCollisionRisk:
                    decisionDetail = "pair=" + Describe(decisionSubject) + " arrivee="
                        + conflictArrival.ToString("0.00") + "s vs " + conflictOtherArrival.ToString("0.00") + "s"
                        + " motif=" + Describe(conflictVerdictReason)
                        + (float.IsFinite(conflictSpeedLimitGap)
                            ? " arret_dans=" + conflictSpeedLimitGap.ToString("0.00") + "m"
                            : string.Empty);
                    break;
                default:
                    decisionDetail = string.Empty;
                    break;
            }
        }

        private static string Describe(string value)
        {
            return string.IsNullOrEmpty(value) ? "(inconnu)" : value;
        }

        /// <summary>
        /// Trace COMPLETE d'une decision, avec ses preuves. Un motif seul ne se verifie pas en
        /// recette : c'est ce qui a laisse passer les passes correctives precedentes de la
        /// Story 5.18. Chaque ligne nomme l'objet et la grandeur qui ont produit l'etat.
        ///
        /// ponytail: composee A LA DEMANDE, par le seul vehicule selectionne dans l'Editeur
        /// (<c>OnDrawGizmosSelected</c> n'est appele que pour lui). Aucun cadre de diagnostic
        /// permanent, aucune allocation par frame sur les autres vehicules.
        /// </summary>
        public string DescribeDecision()
        {
            var nl = System.Environment.NewLine;
            var text = name + nl;
            text += "Voie       : " + CurrentLaneNode;
            if (plannedJunctionExit >= 0) text += " -> " + plannedJunctionExit;
            text += nl;

            if (committedJunctionKey > 0)
            {
                text += "Jonction   : cle " + committedJunctionKey + " mvt " + committedApproachNode
                    + "->" + committedExitNode + " permission=engage (traversee en cours)" + nl;
            }
            else if (!string.IsNullOrEmpty(junctionPermission))
            {
                text += "Jonction   : " + Describe(junctionDiagnosticId) + " mvt " + CurrentLaneNode
                    + "->" + plannedJunctionExit + " permission=" + junctionPermission
                    + " revendications=" + claimantCount + " attente=" + junctionWaitSeconds.ToString("0.0") + "s" + nl;
                text += "Arret      : avant du vehicule a " + junctionStopGap.ToString("0.00")
                    + " m de la ligne, maintien " + junctionStopHeldSeconds.ToString("0.00") + " s" + nl;
            }

            text += "Suivi      : " + (perceivedLeaderGap < DriverModel.NoLeaderGap
                ? Describe(leaderName) + " voie=" + leaderLaneNode + " ecart=" + perceivedLeaderGap.ToString("0.0")
                  + "m v=" + perceivedLeaderSpeed.ToString("0.0") + (leaderIsStatic ? " (immobile)" : string.Empty)
                : "aucun") + nl;
            if (hasTrajectoryConflict || conflictDriver != null)
            {
                var partner = ConflictPartner;
                text += "Conflit    : " + (partner != null ? partner.name : "(non IA)")
                    + " verdict=" + (conflictDriver == null ? "AUCUN (pair deja apparie)"
                        : conflictYield ? "je cede" : "je passe")
                    + (yieldReleased ? " (cession relachee)" : string.Empty)
                    + " arrivee=" + conflictArrival.ToString("0.00") + "s vs " + conflictOtherArrival.ToString("0.00") + "s"
                    + " motif=" + Describe(conflictVerdictReason)
                    + (headOnConflict ? " face-a-face" : string.Empty)
                    + (headOnConflict ? (headOnYield ? " (nous sommes le mal place)" : " (l'autre est le mal place)") : string.Empty) + nl;
            }

            var waits = WaitsFor;
            text += "Depend de  : " + (waits != null ? waits.name : "personne") + nl;
            text += "Blocage    : palier " + recoveryStage + " apres " + stalledElapsedSeconds.ToString("0.0")
                + " s sans progres" + (heldByBoundedWait ? " (attente bornee : l'horloge ne tourne pas)" : string.Empty)
                + (unblockingEpisodeActive ? " -- episode de deblocage actif, action " + currentUnblockingAction : string.Empty) + nl;
            text += "Sortie     : libre=" + junctionExitFreeLength.ToString("0.00") + "m requis="
                + junctionExitRequiredLength.ToString("0.00") + "m utile=" + junctionExitUsableLength.ToString("0.00")
                + "m occupants=" + junctionExitOccupants
                + (string.IsNullOrEmpty(exitSaturatedBy) ? string.Empty : " premier=" + exitSaturatedBy) + nl;
            text += "Decision   : " + decisionReason
                + (string.IsNullOrEmpty(trafficRefusal) ? string.Empty : " -- " + trafficRefusal) + nl;
            text += "Preuve     : " + Describe(decisionDetail);
            return text;
        }

        /// <summary>
        /// Une cession ne se relache JAMAIS sur un delai. Un delai ne dit rien de la situation : il
        /// confond la file legitime qui avance lentement avec l'attente qui ne se resoudra pas, et
        /// forcer un vehicule au bout de N secondes fabrique une regle de priorite qui n'existe nulle
        /// part -- deux vehicules peuvent la declencher au meme instant.
        ///
        /// Le seul motif de relachement est SEMANTIQUE : la chaine d'attente revient sur nous. Un
        /// cycle est la preuve qu'aucun de ses membres ne peut avancer par le jeu normal des regles,
        /// puisque chacun attend un membre du cycle. Il se rompt alors par le MEME ordre total qui
        /// arbitre tout le reste -- le plus petit identifiant reseau passe -- ce qui donne exactement
        /// un vehicule liberé, quel que soit le membre qui fait le calcul : tous parcourent le meme
        /// cycle et en tirent le meme minimum.
        ///
        /// Hors cycle, l'attente est LEGITIME : elle a un debouche, meme lent (file, sortie saturee,
        /// prioritaire qui progresse). La laisser courir est la bonne reponse, et c'est ce qui
        /// distingue une saturation reelle d'un interblocage.
        /// </summary>
        private void TickYieldRelease(DriverProfile profile, float dt, bool waiting)
        {
            if (!waiting)
            {
                yieldElapsedSeconds = 0f;
                if (conflictDriver == null && !hasTrajectoryConflict) yieldReleased = false;
                return;
            }

            yieldElapsedSeconds += dt;
            var released = TryBreakWaitCycle();
            if (released == yieldReleased) return;
            yieldReleased = released;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (released && !warnedYieldRelease)
            {
                warnedYieldRelease = true;
                Debug.LogWarning("[Vehicles] " + name + " : cycle d'attente detecte apres "
                    + yieldElapsedSeconds.ToString("0.0") + " s -- plus petit identifiant du cycle,"
                    + " cession relachee. Aucun retrait ni teleportation.", this);
            }
#endif
        }

        /// <summary>Profondeur de parcours de la chaine d'attente. Un cycle plus long qu'elle releve
        /// d'une saturation de la carte, pas d'un interblocage local.</summary>
        private const int WaitCycleDepth = 12;

        /// <summary>
        /// La chaine d'attente revient-elle sur nous, ET sommes-nous le plus petit identifiant du
        /// cycle ? Le parcours ne lit que de l'etat deja partage entre pairs (la decision courante et
        /// le vehicule attendu), donc deux membres du meme cycle en tirent le meme verdict.
        /// </summary>
        private bool TryBreakWaitCycle()
        {
            var members = 0;
            var current = WaitsFor;
            for (var depth = 0; current != null && depth < WaitCycleDepth; depth++)
            {
                if (current == this)
                {
                    return JunctionRules.ReleasesWaitCycle(NetworkObjectId, waitCycleScratch, members);
                }

                if (members >= waitCycleScratch.Length) break;
                waitCycleScratch[members++] = current.NetworkObjectId;
                current = current.WaitsFor;
            }

            // La chaine ne revient pas sur nous : l'attente a un debouche, meme lointain. Elle court.
            return false;
        }

        private readonly ulong[] waitCycleScratch = new ulong[WaitCycleDepth];

        /// <summary>
        /// L'attente de ce leader est-elle REGULIERE ? Vrai quand le leader est un pair IA dont la
        /// decision courante est une attente qui se resoudra d'elle-meme. Un seul saut suffit : chaque
        /// vehicule interroge son propre leader, donc l'etat se propage le long de la file a chaque
        /// pas, sans parcourir la chaine ni risquer une recursion.
        /// </summary>
        private static bool IsLegitimateWait(NetworkedAIVehicleDriverController leader)
        {
            if (leader == null || !leader.IsSpawned) return false;
            switch (leader.DecisionReason)
            {
                case TrafficDecisionReason.FollowingSameLane:
                case TrafficDecisionReason.TrafficQueue:
                case TrafficDecisionReason.JunctionYield:
                case TrafficDecisionReason.ExitSaturated:
                case TrafficDecisionReason.YieldTrajectory:
                case TrafficDecisionReason.Cruise:
                    return true;
                default:
                    // StaticObstacle, EmergencyBrake, PerceptionSaturated, DeadlockRecovery,
                    // UnblockingManeuver, JunctionBreach, PlayerImmediateConflict : la tete de file
                    // ne progresse pas d'elle-meme, l'echelle de deblocage garde tout son sens.
                    return false;
            }
        }

        /// <summary>
        /// Avance-t-on encore vers le waypoint courant ? La distance qui DIMINUE est le seul signe de
        /// progres qui ne se laisse pas tromper : une roue qui patine, un vehicule qui pivote sur
        /// place ou qui recule en boucle ont tous une vitesse non nulle sans avancer d'un metre.
        ///
        /// Un saut de distance vers le haut signale un changement de waypoint, pas une regression :
        /// la reference repart de la nouvelle cible.
        /// </summary>
        /// <summary>
        /// Conformite de voie tenue a CHAQUE pas, et non plus seulement au moment d'un face-a-face
        /// ou d'une fin de manoeuvre. Une manoeuvre de deblocage sort legitimement de la voie : son
        /// temps ne compte pas.
        /// </summary>
        private void TickLaneConformance(DriverProfile profile, float dt)
        {
            ResolveLaneConformance(out laneDirectionCorrect, out laneAxisDistance);

            // LA REFERENCE DE SENS EST LA TANGENTE DE NOTRE PROPRE TRAJECTOIRE, pas l'axe routier
            // le plus proche. Mesure : la premiere version de cette detection lisait l'arete la
            // plus proche, qui au milieu d'un carrefour est souvent transversale ou opposee. Des
            // vehicules parfaitement corrects se declaraient alors a contresens, montaient jusqu'au
            // dernier recours et manoeuvraient les uns dans les autres -- le contresens mesure au
            // banc etait PASSE de 53,1 s a 113,6 s, et le debit de 15 sorties a 9.
            //
            // Une manoeuvre de deblocage sort legitimement de la voie : son temps ne compte pas.
            var tangent = HasDrivePath
                ? LaneGraphRouting.ResolvePathTangent(drivePath, drivePathCount, transform.position)
                : Vector3.zero;

            if (maneuverActive || tangent.sqrMagnitude <= 0.0001f)
            {
                wrongWayElapsedSeconds = 0f;
                return;
            }

            // Franchement oppose, et pas seulement "pas tout a fait aligne" : un vehicule en plein
            // virage serre s'ecarte legitimement de la tangente de son propre echantillonnage.
            if (Vector3.Dot(ResolvePlanarForward(), tangent) > -SameLaneAlignmentDot)
            {
                wrongWayElapsedSeconds = 0f;
                return;
            }

            wrongWayElapsedSeconds += dt;
        }

        private void TickRouteProgress(DriverProfile profile, float dt)
        {
            if (laneGraph == null || state == null) return;

            var target = laneGraph.GetNodePosition(state.WaypointIndex.Value);

            // PROGRES LE LONG DE LA TRAJECTOIRE, pas a vol d'oiseau. En courbe et en giratoire la
            // distance euclidienne au waypoint stagne -- voire augmente -- alors que le vehicule
            // avance normalement sur son arc. Repli sur la distance planaire quand la polyligne ne
            // porte pas la cible (waypoint deja franchi, trajectoire trop courte).
            var arc = HasDrivePath
                ? LaneGraphRouting.ResolvePathRemainingDistance(drivePath, drivePathCount, transform.position, target)
                : -1f;
            var distance = arc > 0f
                ? arc
                : Vector3.ProjectOnPlane(target - transform.position, Vector3.up).magnitude;

            if (!float.IsFinite(routeBestDistance)
                || distance > routeBestDistance + arrivalRadius
                || distance <= routeBestDistance - Mathf.Max(0.01f, profile.ProgressDistance))
            {
                routeBestDistance = distance;
                stalledElapsedSeconds = 0f;
            }
            else
            {
                routeBestDistance = Mathf.Min(routeBestDistance, distance);
                // Une attente bornee (feu rouge, file derriere un feu) n'accumule pas : elle a une fin
                // garantie, et la recuperation n'a rien a y corriger.
                if (!heldByBoundedWait) stalledElapsedSeconds += dt;
            }

            // Deux motifs d'escalade, un seul escalier : ne pas avancer, ou avancer DANS LE MAUVAIS
            // SENS. Le second ne produit aucune immobilite, donc il lui faut son propre compteur --
            // sans quoi il reste invisible a l'echelle.
            var stalledStage = JunctionRules.ResolveRecoveryStage(
                stalledElapsedSeconds, profile.HornDelay, profile.JunctionEscalationDelay);

            // Le contresens ouvre l'echelle, mais il ne donne JAMAIS acces au dernier recours : le
            // palier 4 tolere le contact avec un vehicule immobile, et cela ne se justifie que
            // devant une immobilite reelle. Un vehicule qui roule -- fut-ce a l'envers -- doit se
            // remettre dans le sens, pas forcer le passage.
            var wrongWayStage = Mathf.Min(3, JunctionRules.ResolveRecoveryStage(
                wrongWayElapsedSeconds, profile.HornDelay, profile.JunctionEscalationDelay));

            recoveryStage = Mathf.Max(stalledStage, wrongWayStage);
        }

        /// <summary>
        /// L'ESCALIER DE RECUPERATION, du moins couteux au dernier recours. Chaque palier n'est
        /// essaye que si les precedents ont echoue, et chacun reste valide par
        /// <see cref="ValidatePath"/> : aucun palier n'invente de place qui n'existe pas.
        ///
        /// 1 replanification puis contournement sur la chaussee ;
        /// 2 recul pour se degager et rouvrir un espace de manoeuvre ;
        /// 3 manoeuvre elargie sur l'espace voisin, trottoir compris ;
        /// 4 dernier recours : un vehicule IA A L'ARRET cesse d'etre infranchissable, parce qu'un
        ///   frottement vaut mieux qu'un carrefour fige pour toujours. Le decor, les pietons et les
        ///   joueurs restent infranchissables a TOUS les paliers.
        ///
        /// Dans un face-a-face, seul le vehicule MAL PLACE manoeuvre (conformite de voie lue sur le
        /// graphe : sens de circulation, puis distance a l'axe, puis identifiant). Le vehicule
        /// correctement place ne quitte sa voie qu'au palier 3, quand l'autre a manifestement echoue.
        /// </summary>
        private bool TryEscalateRecovery(DriverProfile profile, float longitudinalSpeed)
        {
            // LA SORTIE D'EPISODE SE TESTE A TOUT PALIER. Elle n'etait atteignable que sous le
            // palier 0, or le palier vient du temps sans progres, qui ne retombe pas tant que le
            // vehicule est coince : un episode ouvert ne pouvait donc plus se refermer, meme une
            // fois la voie reellement reacquise. Le vehicule restait au palier 4 a rejouer les
            // memes manoeuvres (mesure soak : jusqu'a 36 episodes pour un seul vehicule).
            if (unblockingEpisodeActive && HasRecoveredRoute(profile))
            {
                ResetUnblockingEpisode(profile);
                return false;
            }

            if (recoveryStage <= 0) return false;

            if (headOnConflict && !headOnYield && recoveryStage < 3) return false;
            if (!refreshedThisStep || maneuverActive || perceptionSaturated || Time.time < retryAfter) return false;

            var lastResort = recoveryStage >= 4;

            // UNE ATTENTE BORNEE NE SE CONTOURNE PAS, et ici ce n'est pas qu'une question de
            // politesse : une manoeuvre engagee ne repasse plus par TickJunctionRules (l'etage
            // local rend la main avant), donc elle franchirait un feu rouge sans jamais le voir.
            // Tant que le feu EST la cause de l'attente, seul le dernier recours manoeuvre.
            if (junctionHeldBySignal && !lastResort) return false;

            unblockingEpisodeActive = true;
            if (!hornReported)
            {
                hornReported = true;
                Debug.Log("[Vehicles] " + name + " : klaxon de deverrouillage.", this);
            }

            if (!replanAttemptedForBlock)
            {
                TryReplanTowardsExit(profile);
                replanAttemptedForBlock = true;
            }

            // SE REMETTRE DANS LE BON SENS AVANT TOUT LE RESTE. Contourner ou reculer a contresens
            // conserve exactement l'erreur qu'il faut corriger : le contournement se construit sur
            // le cap courant, et le recul le suit a l'envers. Aucun autre palier ne reoriente.
            if (!laneDirectionCorrect && (!reorientAttempted || lastResort)
                && TryStartReorientation(profile, lastResort))
            {
                return DriveOnNextStep(longitudinalSpeed);
            }

            if (TryStartDetour(profile, false, lastResort)) return DriveOnNextStep(longitudinalSpeed);

            if (recoveryStage >= 2 && (!reverseAttempted || lastResort)
                && (lastResort || !HasManeuverFailed(TrafficUnblockingAction.Reverse)))
            {
                reverseAttempted = true;
                maneuverTargets[0] = transform.position
                    - ResolvePlanarForward() * profile.ReverseSpeed * profile.ReverseDuration;
                if (ValidatePath(profile, maneuverTargets, 0, 1, true, false, lastResort))
                {
                    BeginManeuver(TrafficUnblockingAction.Reverse, 1);
                    return DriveOnNextStep(longitudinalSpeed);
                }
            }

            if (recoveryStage >= 3 && TryStartDetour(profile, true, lastResort))
                return DriveOnNextStep(longitudinalSpeed);

            // Prochain essai borne : une manoeuvre avortee ne repart pas immediatement en miroir, et
            // le vehicule attend un changement reel de trafic plutot que de rejouer la meme boucle.
            retryAfter = Time.time + profile.RetryCooldown;
            return false;
        }

        private bool DriveOnNextStep(float speed)
        {
            SubmitIntentToPhysicsLayer(ResolveStopIntent(speed));
            return true;
        }

        private VehicleDriveIntent ResolveStopIntent(float longitudinalSpeed)
        {
            var threshold = physicsBody != null && physicsBody.HasProfile ? physicsBody.Profile.MinimumDirectionSpeed : 0f;
            return longitudinalSpeed > threshold
                ? new VehicleDriveIntent(0f, 0f, 1f, 0f)
                : new VehicleDriveIntent(0f, 0f, 0f, 1f);
        }

        private void TryReplanTowardsExit(DriverProfile profile)
        {
            // Un blocage observe sur notre trajectoire autorise une replanification; la seule immobilite
            // d'un voisin ne suffit pas. Garder le waypoint courant jusqu'a sa vraie arrivee : un
            // successeur n'est pas un raccourci spatial.
            if (blockingCollider == null) return;
            var current = state.WaypointIndex.Value;
            var next = LaneGraphRouting.FindNextTowardReachableExit(
                current, laneGraph.NodeCount, laneGraph.GetSuccessors, laneGraph.IsExitPortal);
            var currentPoint = laneGraph.GetNodePosition(current);
            if (next >= 0 && next != current
                && HasArrivedAtWaypoint(transform.position, currentPoint, profile.PathSampleDistance))
            {
                maneuverTargets[0] = laneGraph.GetNodePosition(next);
                if (ValidatePath(profile, maneuverTargets, 0, 1, false, false))
                {
                    state.WaypointIndex.Value = next;
                    MarkNodeTraversed(next);
                }
            }
        }

        /// <summary>
        /// Remise dans le SENS DE CIRCULATION authore. Le vehicule vise un point de l'axe routier
        /// le plus proche, pris DANS SON SENS LEGAL -- donc derriere lui quand il roule a
        /// contresens -- en amorcant le virage d'un cote puis de l'autre.
        ///
        /// Aucune place n'est inventee : <see cref="ValidatePath"/> simule la pose reelle avec le
        /// braquage reel. Quand le demi-tour ne passe pas d'un seul tenant -- le cas ordinaire sur
        /// une chaussee de 4 m -- la tentative echoue proprement et l'echelle enchaine sur le
        /// recul, qui ouvre l'espace ; le demi-tour redevient alors realisable au cycle suivant.
        /// C'est ainsi que le trois-points emerge des paliers deja en place, sans palier dedie.
        /// </summary>
        private bool TryStartReorientation(DriverProfile profile, bool lastResort)
        {
            if (!lastResort && HasManeuverFailed(TrafficUnblockingAction.Reorient)) return false;
            if (laneGraph == null || vehicleBox == null || physicsBody == null || !physicsBody.HasProfile) return false;
            if (!laneGraph.TryGetRoadPosition(transform.position, out var axisPoint, out var axisDirection, out _)) return false;

            reorientAttempted = true;
            var forward = ResolvePlanarForward();
            var right = Vector3.Cross(Vector3.up, forward);
            var radius = Mathf.Max(0.5f,
                Wheelbase() / Mathf.Tan(Mathf.Max(1f, physicsBody.Profile.MaxSteerAngleDegrees) * Mathf.Deg2Rad));

            for (var side = 1; side >= -1; side -= 2)
            {
                maneuverTargets[0] = transform.position + forward * (radius * 0.5f) + right * (side * radius);
                maneuverTargets[1] = axisPoint + axisDirection * (radius * 2f);
                maneuverTargets[2] = axisPoint + axisDirection * (radius * 2f + Mathf.Max(profile.RoadDetourGap, radius));
                for (var i = 0; i < 3; i++) maneuverTargets[i].y = transform.position.y;
                if (!ValidatePath(profile, maneuverTargets, 0, 3, false, false, lastResort)) continue;
                BeginManeuver(TrafficUnblockingAction.Reorient, 3);
                return true;
            }

            return false;
        }

        private bool TryStartDetour(DriverProfile profile, bool sidewalk, bool lastResort = false)
        {
            var action = sidewalk ? TrafficUnblockingAction.SidewalkDetour : TrafficUnblockingAction.RoadDetour;
            // Un palier deja echoue ne se rejoue pas -- sauf au dernier recours, ou la validation
            // elle-meme a change : ce n'est plus le meme essai.
            if (!lastResort && HasManeuverFailed(action)) return false;
            if (blockingCollider == null || vehicleBox == null || physicsBody == null || !physicsBody.HasProfile) return false;
            var forward = ResolvePlanarForward();
            var right = Vector3.Cross(Vector3.up, forward);
            ReadColliderGeometry(blockingCollider, out var blockerCenter, out var blockerExtents, out var blockerRotation);
            var along = Vector3.Dot(blockerCenter - transform.position, forward);
            var halfLength = TrafficPerception.ProjectExtent(blockerExtents, blockerRotation, forward);
            var lateral = Vector3.Dot(blockerCenter - transform.position, right);
            var offset = TrafficPerception.ProjectExtent(blockerExtents, blockerRotation, right) + vehicleHalfExtents.x
                + profile.SafetyMargin * 2f + profile.PathSampleDistance * 2f;
            if (sidewalk) offset = Mathf.Max(offset, profile.SidewalkClearanceRadius);
            var run = Mathf.Max(profile.RoadDetourGap, Wheelbase() / Mathf.Tan(physicsBody.Profile.MaxSteerAngleDegrees * Mathf.Deg2Rad));
            var pass = Mathf.Max(run * 2f, along + halfLength + frontOffset + profile.MinimumGap);
            for (var side = 1; side >= -1; side -= 2)
            {
                maneuverTargets[0] = transform.position + forward * run + right * (lateral + side * offset);
                maneuverTargets[1] = transform.position + forward * pass + right * (lateral + side * offset);
                var routeProjection = transform.position + forward * (pass + run);
                laneGraph.TryGetRoadPosition(routeProjection, out var returnPoint, out _, out _);
                returnPoint.y = transform.position.y;
                maneuverTargets[2] = returnPoint;
                if (!ValidatePath(profile, maneuverTargets, 0, 3, false, sidewalk, lastResort)) continue;
                BeginManeuver(action, 3);
                return true;
            }
            return false;
        }

        private void BeginManeuver(TrafficUnblockingAction action, int count)
        {
            unblockingEpisodeActive = true;
            currentUnblockingAction = action;
            decisionReason = TrafficDecisionReason.UnblockingManeuver;
            maneuverActive = true; maneuverPathSafe = true;
            maneuverTargetIndex = 0; maneuverTargetCount = count;
            maneuverElapsed = 0f; noProgressElapsed = 0f;
            reverseTravelSeconds = 0f;
            bestTargetDistance = Vector3.Distance(transform.position, maneuverTargets[0]);
            trafficRefusal = string.Empty;
        }

        private void RejoinRouteAfterManeuver()
        {
            var forward = ResolvePlanarForward();
            // Avancer seulement les segments reellement franchis par le contournement.
            // Un portail ne peut etre signale par cette reconciliation.
            for (var step = 0; step < laneGraph.NodeCount; step++)
            {
                var current = state.WaypointIndex.Value;
                var point = laneGraph.GetNodePosition(current);
                if (laneGraph.IsExitPortal(current) || Vector3.Dot(point - transform.position, forward) >= 0f) break;
                var successors = laneGraph.GetSuccessors(current);
                var best = -1;
                var bestDistance = float.PositiveInfinity;
                for (var i = 0; i < successors.Count; i++)
                {
                    var end = laneGraph.GetNodePosition(successors[i]);
                    if (Vector3.Dot(end - point, forward) <= 0f) continue;
                    var projection = TrafficPerception.ClosestPointOnSegment(transform.position, point, end);
                    var distance = Vector3.ProjectOnPlane(projection - transform.position, Vector3.up).sqrMagnitude;
                    if (distance < bestDistance) { best = successors[i]; bestDistance = distance; }
                }
                if (best < 0 || bestDistance > arrivalRadius * arrivalRadius) break;
                state.WaypointIndex.Value = best;
                MarkNodeTraversed(best);
                traversedEdges++;
                hasDepartedSpawnNode = true;
            }
        }

        private void CancelManeuver(string reason, DriverProfile profile)
        {
            failedManeuverMask |= 1 << (int)currentUnblockingAction;
            maneuverActive = false;
            currentUnblockingAction = TrafficUnblockingAction.Replan;
            retryAfter = Time.time + profile.RetryCooldown;
            trafficRefusal = reason;
            appliedAcceleration = 0f;
        }

        private bool HasManeuverFailed(TrafficUnblockingAction action)
        {
            return (failedManeuverMask & (1 << (int)action)) != 0;
        }

        private bool HasRecoveredRoute(DriverProfile profile)
        {
            if (laneGraph == null || !laneGraph.TryGetRoadPosition(
                    transform.position, out _, out var direction, out var distance)) return false;
            return distance <= vehicleHalfExtents.x + profile.SafetyMargin
                && Vector3.Dot(ResolvePlanarForward(), direction) > 0f;
        }

        private void ResetUnblockingEpisode(DriverProfile profile)
        {
            unblockingEpisodeActive = false;
            stalledElapsedSeconds = 0f;
            wrongWayElapsedSeconds = 0f;
            reorientAttempted = false;
            recoveryStage = 0;
            routeBestDistance = float.PositiveInfinity;
            failedManeuverMask = 0;
            reverseAttempted = false;
            hornReported = false;
            replanAttemptedForBlock = false;
            hasAimPoint = false;
            retryAfter = Time.time + profile.RetryCooldown;
        }

        /// <summary>
        /// La ligne de cession du conflit courant est-elle encore DEVANT nous, et a quelle distance ?
        /// Le calcul est pur et vit dans <see cref="TrafficPerception.ResolveConflictStopGap"/>, ou il
        /// est mesurable sans vehicule ; ici ne reste que la lecture de notre propre pose.
        /// </summary>
        private bool TryResolveConflictStopGap(DriverProfile profile, out float gap)
        {
            gap = float.PositiveInfinity;
            if (!hasConflictPoint) return false;

            gap = TrafficPerception.ResolveConflictStopGap(transform.position, ResolvePlanarForward(),
                conflictPoint, frontOffset, vehicleHalfExtents.x, profile.SafetyMargin);
            return gap > 0f;
        }

        private bool MustYieldHeadOn(NetworkedAIVehicleDriverController peer)
        {
            if (peer == null || !peer.IsSpawned) return true;
            ResolveLaneConformance(out var ownCorrect, out var ownDistance);
            peer.ResolveLaneConformance(out var otherCorrect, out var otherDistance);
            return TrafficPerception.MustYield(ownCorrect, ownDistance, NetworkObjectId,
                otherCorrect, otherDistance, peer.NetworkObjectId);
        }

        private void ResolveLaneConformance(out bool correct, out float distance)
        {
            correct = false;
            distance = float.PositiveInfinity;
            if (laneGraph == null || !laneGraph.TryGetRoadPosition(
                    transform.position, out _, out var direction, out distance)) return;
            correct = Vector3.Dot(ResolvePlanarForward(), direction) > SameLaneAlignmentDot;
        }

        private float Wheelbase()
        {
            var min = float.PositiveInfinity; var max = float.NegativeInfinity;
            for (var i = 0; i < physicsBody.Profile.WheelCount; i++)
            {
                var z = physicsBody.Profile.GetWheel(i).LocalPosition.z;
                min = Mathf.Min(min, z); max = Mathf.Max(max, z);
            }
            return Mathf.Max(0.0001f, max - min);
        }

        /// <summary>Prediction cinematique bornee, jamais ecriture physique : gabarit + braquage reel,
        /// echantillons enveloppants et revalidation a la cadence de perception. Un echec ferme le passage.</summary>
        private bool ValidatePath(DriverProfile profile, Vector3[] targets, int first, int count,
            bool reverse, bool sidewalk, bool lastResort = false)
        {
            if (perceptionSaturated || vehicleBox == null || physicsBody == null || !physicsBody.HasProfile) return false;
            var position = transform.position;
            var rotation = Quaternion.LookRotation(ResolvePlanarForward());
            var target = first;
            var step = profile.PathSampleDistance;
            var speed = reverse ? profile.ReverseSpeed : profile.ManeuverSpeed;
            var steerAngle = physicsBody.CurrentSteerAngleDegrees;
            var elapsed = 0f;
            // ponytail: 128 poses maximum par candidat; au-dela, refus plutot qu'un chemin tronque accepte.
            for (var sample = 0; sample < 128 && target < count; sample++)
            {
                if (Vector3.ProjectOnPlane(targets[target] - position, Vector3.up).magnitude <= step)
                {
                    target++;
                    if (target == count) return true;
                }
                var previous = position;
                var forward = rotation * Vector3.forward;
                var input = reverse ? 0f : ComputeSeekIntent(position, forward, targets[target], steerFullLockDegrees).Steer;
                var vehicle = physicsBody.Profile;
                var targetAngle = VehicleSteeringModel.ResolveSteerAngleDegrees(input, speed, vehicle.MinimumDirectionSpeed,
                    vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);
                var sampleTime = step / speed;
                steerAngle = Mathf.MoveTowards(steerAngle, targetAngle, vehicle.SteerRateDegreesPerSecond * sampleTime);
                var yaw = Mathf.Tan(steerAngle * Mathf.Deg2Rad) / Wheelbase() * step * Mathf.Rad2Deg * (reverse ? -1f : 1f);
                position += forward * step * (reverse ? -1f : 1f);
                rotation = Quaternion.AngleAxis(yaw, Vector3.up) * rotation;
                elapsed += sampleTime;
                // La marge couvre l'espace entre deux poses ainsi que le coin balaye en rotation.
                var margin = profile.SafetyMargin + step + vehicleHalfExtents.magnitude * Mathf.Abs(yaw) * Mathf.Deg2Rad;
                if (!IsPoseClear(position, previous, rotation, margin, elapsed, sampleTime, profile, sidewalk, lastResort)) return false;
            }
            trafficRefusal = "trajectoire hors budget / braquage";
            return false;
        }

        private bool IsPoseClear(Vector3 position, Vector3 previous, Quaternion rotation, float margin,
            float time, float duration, DriverProfile profile, bool sidewalk, bool lastResort = false)
        {
            var center = position + rotation * vehicleCenterOffset;
            var extents = vehicleHalfExtents + new Vector3(margin, 0f, margin);
            var found = Physics.OverlapBoxNonAlloc(center, extents, poseHits, rotation, ~0, QueryTriggerInteraction.Ignore);
            if (found >= poseHits.Length) { trafficRefusal = "volume sature"; return false; }
            for (var i = 0; i < found; i++)
            {
                var hit = poseHits[i];
                if (hit == null || hit.attachedRigidbody == body || IsLowSurface(hit, profile, sidewalk)) continue;
                // DERNIER RECOURS UNIQUEMENT : un vehicule IA a l'arret cesse d'etre infranchissable.
                // Un frottement vaut mieux qu'un carrefour fige pour toujours. Le decor, les pietons
                // et les joueurs, eux, le restent a tous les paliers -- c'est la limite de la regle.
                if (lastResort && IsStalledPeerVehicle(hit)) continue;
                // Autoriser seulement un mouvement separant un contact deja present, jamais traverser le bloqueur.
                var startCenter = transform.position + transform.rotation * vehicleCenterOffset;
                var initial = (hit.ClosestPoint(startCenter) - startCenter).sqrMagnitude;
                var beforeCenter = previous + rotation * vehicleCenterOffset;
                var before = (hit.ClosestPoint(beforeCenter) - beforeCenter).sqrMagnitude;
                var after = (hit.ClosestPoint(center) - center).sqrMagnitude;
                var away = Vector3.Dot(position - previous, startCenter - hit.bounds.center) > 0f;
                if (away && after > before && initial < vehicleHalfExtents.sqrMagnitude) continue;
                trafficRefusal = hit.GetComponentInParent<CharacterController>() != null ? "pieton" : "obstacle : " + hit.name;
                return false;
            }
            for (var i = 0; i < perceptionCount; i++)
            {
                var actor = perceptionCandidates[i];
                if (!actor.IsRoadUser || actor.Velocity.sqrMagnitude < 0.0001f) continue;
                var priorCenter = previous + rotation * vehicleCenterOffset;
                var actorStart = actor.Position + actor.Velocity * Mathf.Max(0f, time - duration);
                if (TrafficPerception.PredictOrientedCollision(actorStart - priorCenter,
                    actor.Velocity - (center - priorCenter) / duration, extents, rotation,
                    actor.Extents, actor.Rotation, 0f, duration, out _, out _))
                { trafficRefusal = "trajectoire mobile"; return false; }
            }
            // Les quatre coins doivent porter sur une surface positivement autorisee.
            for (var x = -1; x <= 1; x += 2)
                for (var z = -1; z <= 1; z += 2)
                {
                    var point = position + rotation * new Vector3(x * vehicleHalfExtents.x, 0f, z * vehicleHalfExtents.z);
                    if (!HasSupport(point, profile, sidewalk)) { trafficRefusal = "sol non autorise / trottoir"; return false; }
                }
            return true;
        }

        /// <summary>
        /// Ce collider appartient-il a un vehicule IA A L'ARRET ? Seul cet ensemble est franchissable
        /// au dernier recours : un pieton, un joueur ou un element de decor ne l'est jamais.
        /// </summary>
        private bool IsStalledPeerVehicle(Collider hit)
        {
            if (hit.GetComponentInParent<CharacterController>() != null) return false;
            var peer = hit.GetComponentInParent<NetworkedAIVehicleDriverController>();
            if (peer == null || !peer.IsSpawned) return false;
            var peerBody = hit.attachedRigidbody;
            return peerBody == null
                || Vector3.ProjectOnPlane(peerBody.linearVelocity, Vector3.up).magnitude <= stuckSpeedThreshold;
        }

        // Convention greybox transitoire : noms des colliders projet, jamais noms des visuels Synty.
        // Remplacer cette classification par les metadonnees de surface lorsque l'authoring evoluera.
        private static int SurfaceKind(Collider hit)
        {
            if (hit.attachedRigidbody != null) return 0;
            var label = hit.name;
            if (label == "Col_Roadway" || label.StartsWith("Col_Roadway_", StringComparison.Ordinal)) return 1;
            if (label.StartsWith("Col_Sidewalk_", StringComparison.Ordinal) || label.StartsWith("Col_Curb_", StringComparison.Ordinal)) return 2;
            return 0;
        }

        /// <summary>
        /// L'acteur repose-t-il sur la CHAUSSEE ? Rayon vers le bas depuis son emprise et lecture de la
        /// surface porteuse avec la classification deja en place (<see cref="SurfaceKind"/>). Un
        /// trottoir, une bordure ou une surface inconnue rendent faux : ce n'est pas de la chaussee.
        ///
        /// Rendre faux quand RIEN n'est trouve est volontaire -- un acteur sans sol sous lui n'est pas
        /// etabli sur la voie, et il n'a donc pas a peser sur la circulation.
        /// </summary>
        private bool IsOnCarriageway(Vector3 position, DriverProfile profile)
        {
            var origin = position + Vector3.up * Mathf.Max(0.5f, vehicleHalfExtents.y);
            var reach = Mathf.Max(1f, vehicleHalfExtents.y * 2f + profile.MaxCurbHeight * 4f);
            var count = Physics.RaycastNonAlloc(origin, Vector3.down, groundHits, reach, ~0, QueryTriggerInteraction.Ignore);
            var nearest = float.PositiveInfinity;
            Collider support = null;
            for (var i = 0; i < count; i++)
            {
                var candidate = groundHits[i];
                if (candidate.collider == null || candidate.collider.attachedRigidbody != null
                    || candidate.collider.GetComponentInParent<CharacterController>() != null
                    || candidate.distance >= nearest) continue;
                nearest = candidate.distance;
                support = candidate.collider;
            }

            return support != null && SurfaceKind(support) == 1;
        }

        private bool IsLowSurface(Collider hit, DriverProfile profile, bool sidewalk)
        {
            var kind = SurfaceKind(hit);
            // Le grand sol greybox porte les modules, sans jamais autoriser une manoeuvre sur l'inconnu.
            if (hit.name == "Greybox_GroundPlane" && hit.attachedRigidbody == null) return hit.bounds.max.y < transform.position.y;

            // ANO-5.18-02 : toute surface statique assez BASSE se franchit, quel que soit son NOM.
            //
            // Le critere de franchissement est physique et deja authore -- MaxCurbHeight, 0,15 m --
            // mais il etait jusqu'ici garde derriere une reconnaissance par nom de collider
            // (Col_Roadway / Col_Sidewalk_ / Col_Curb_). Le relief de la Story 5.13 ne porte aucun de
            // ces noms : le dos d'ane de l'avenue (Rampe_Ouest, Rampe_Est) et la marche basse
            // (Relief_MarcheBasse_AvenueCenterToEast) mesurent 0,12 m de haut -- sous le seuil
            // authore -- et etaient pourtant lus comme des murs en travers de la voie. D'ou la boucle
            // rapportee : obstacle immobile -> manoeuvre de contournement -> retour sur la voie ->
            // meme obstacle, indefiniment, alors que le vehicule est CENSE rouler dessus (le banc
            // PlayMode 5.13 lui demande explicitement de franchir ce dos d'ane).
            //
            // La correction retire la garde par nom, elle n'ajoute aucun seuil.
            if (hit.attachedRigidbody == null && hit.GetComponentInParent<CharacterController>() == null
                && hit.bounds.max.y <= transform.position.y + profile.MaxCurbHeight) return true;

            return kind != 0 && (kind == 1 || sidewalk)
                && hit.bounds.max.y <= transform.position.y + profile.MaxCurbHeight;
        }

        private bool HasSupport(Vector3 point, DriverProfile profile, bool sidewalk)
        {
            var origin = point + Vector3.up * vehicleHalfExtents.y;
            var count = Physics.RaycastNonAlloc(origin, Vector3.down, groundHits, vehicleHalfExtents.y + frontOffset,
                ~0, QueryTriggerInteraction.Ignore);
            if (count >= groundHits.Length) return false;
            var nearest = float.PositiveInfinity;
            Collider support = null;
            for (var i = 0; i < count; i++)
            {
                var hit = groundHits[i];
                if (hit.collider.attachedRigidbody == body || hit.distance >= nearest) continue;
                nearest = hit.distance; support = hit.collider;
            }
            if (support == null) return false;
            var kind = SurfaceKind(support);
            return (kind == 1 || (sidewalk && kind == 2)) && support.bounds.max.y <= transform.position.y + profile.MaxCurbHeight;
        }

        // ------------------------------------------------------------------ Story 5.18 : jonctions

        /// <summary>
        /// Revendications concurrentes rassemblees au pas courant. Index 0 = la notre, comme
        /// <see cref="JunctionRules.ResolveWinnerIndex"/> : l'index du gagnant se compare donc a 0.
        /// </summary>
        private readonly JunctionClaim[] claimantScratch = new JunctionClaim[16];
        private readonly int[] claimantWins = new int[16];

        /// <summary>Ordre d'admission, reutilise a chaque pas : <see cref="JunctionRules.IsAdmitted"/> trie sans allouer.</summary>
        private readonly int[] claimantOrder = new int[16];
        private readonly float[] claimantAcceptedGap = new float[16];
        private readonly Vector3[] claimantPositions = new Vector3[16];

        /// <summary>
        /// Tampon de revendications. Separe de <c>poseHits</c> et de <c>perceptionHits</c> : une requete
        /// de place de sortie s'execute pendant qu'on lit cette liste.
        ///
        /// Dimensionne sur une MESURE, pas au juge : au rayon d'approche authore (12 m) autour d'un
        /// noeud de jonction du district, l'overlap remonte 23 a 24 colliders (36 a 24 m). Un tampon de
        /// 16 saturait donc a chaque appel, et la boucle n'inspectait qu'un sous-ensemble arbitraire --
        /// un vehicule concurrent manque, c'est exactement un arbitrage qui rend "les deux entrent".
        /// </summary>
        private readonly Collider[] junctionHits = new Collider[64];

        /// <summary>Meme tampon dedie a la sonde de sortie : 9 a 12 colliders mesures dans ses 6 m, et un vehicule y ajoute les siens.</summary>
        private readonly Collider[] junctionExitHits = new Collider[32];

        private int claimantCount;
        private float nearestClaimantDistance = float.PositiveInfinity;
        private float junctionWaitSeconds;
        private float junctionStopHeldSeconds;
        private bool junctionStopSatisfied;
        private bool junctionClaimantsHeld;
        private bool claimantSetIncomplete;
        private bool junctionBreached;
        private bool warnedJunctionSaturation;
        private bool warnedJunctionDeadlock;
        private int plannedJunctionNode = -1;
        private int plannedJunctionExit = -1;
        private int committedJunctionKey;
        private int committedApproachNode = -1;
        private int committedExitNode = -1;
        private Vector3 committedEntryPoint;
        private Vector3 committedEntryForward;
        private Vector3 committedExitPoint;
        private float junctionStopGap = float.PositiveInfinity;
        private float junctionSpeedLimitGap = float.PositiveInfinity;
        private bool junctionHoldsWaypoint;
        private bool yieldingTrajectory;
        private string junctionPermission = string.Empty;
        private float junctionExitUsableLength;
        private float junctionExitFreeLength = float.PositiveInfinity;
        private float junctionExitRequiredLength;
        private int junctionExitOccupants;

        /// <summary>Nombre de revendications evaluees au dernier pas, la notre comprise. 0 hors approche.</summary>
        public int JunctionClaimants => claimantCount;

        /// <summary>Attente continue (s) a la jonction courante. Remise a zero des que le vehicule franchit ou quitte l'approche.</summary>
        public float JunctionWaitSeconds => junctionWaitSeconds;

        /// <summary>Vrai quand le palier de deblocage d'intersection a ete franchi pour cette approche.</summary>
        public bool JunctionBreached => junctionBreached;

        /// <summary>
        /// Jonction dont ce vehicule OCCUPE l'aire de conflit, ou 0. Lue par les pairs a travers la
        /// revendication qu'il publie, et par le diagnostic.
        /// </summary>
        public int CommittedJunctionKey => committedJunctionKey;

        /// <summary>Ecart (m) de l'AVANT du vehicule a la ligne d'arret de l'approche courante. Infini hors approche.</summary>
        public float JunctionStopGap => junctionStopGap;

        /// <summary>
        /// Contrainte longitudinale du pas : ecart a la PLUS PROCHE des cibles d'arret -- ligne
        /// d'approche d'une jonction authoree, ligne de cession d'un conflit de trajectoire -- ou
        /// infini quand aucune ne s'applique. Une seule grandeur sort d'ici parce qu'une seule pedale
        /// entre dans la couche physique : la plus contraignante commande.
        /// </summary>
        public float PlannedStopGap => Mathf.Min(junctionSpeedLimitGap, conflictSpeedLimitGap);

        /// <summary>Vrai tant que l'arbitrage de jonction interdit d'avancer le waypoint.</summary>
        public bool JunctionHoldsWaypoint => junctionHoldsWaypoint;

        /// <summary>Etat d'autorisation lisible de l'approche courante, pour le diagnostic.</summary>
        public string JunctionPermission => junctionPermission;

        /// <summary>Maintien a l'arret deja accumule sur une approche Stop (s). Preuve du controle de type STOP.</summary>
        public float JunctionStopHeldSeconds => junctionStopHeldSeconds;

        /// <summary>
        /// Mouvement de jonction ENGAGE : frontiere d'entree, sortie, et leurs caps. C'est la courbe
        /// que le vehicule s'est vu accorder, celle a laquelle une recette compare sa trajectoire.
        /// </summary>
        public bool TryGetCommittedMovement(out Vector3 entry, out Vector3 entryForward,
            out Vector3 exit, out Vector3 exitForward)
        {
            entry = committedEntryPoint;
            entryForward = committedEntryForward;
            exit = committedExitPoint;
            exitForward = Vector3.forward;
            if (committedJunctionKey <= 0 || committedExitNode < 0 || laneGraph == null) return false;
            var forward = Vector3.ProjectOnPlane(laneGraph.GetNodeRotation(committedExitNode) * Vector3.forward, Vector3.up);
            if (forward.sqrMagnitude <= 0.0001f) return false;
            exitForward = forward.normalized;
            return true;
        }


        /// <summary>
        /// Story 5.18 : arbitrage d'une approche de jonction. Rend vrai quand le vehicule doit RESTER
        /// sur son noeud de decision -- c'est le SEUL point de blocage du parcours, donc tant que ce
        /// predicat est vrai aucun <c>ResolveNextNode</c> n'a lieu.
        ///
        /// Trois proprietes de forme portent la garantie anti-interblocage, et aucune n'est un detail
        /// d'implementation :
        ///
        /// 1. l'ensemble des revendications est reconstruit par CHAQUE vehicule a partir de la meme
        ///    donnee authoree et du meme monde physique. Aucune valeur privee d'un autre vehicule n'y
        ///    entre : la seule qui le soit -- le minuteur de maintien a l'arret -- reste une garde
        ///    LOCALE. L'y faire entrer rendrait les deux verdicts dissymetriques pendant tout le temps
        ///    du maintien, et deux vehicules attendraient l'un l'autre ;
        /// 2. les revendications sont lues dans une seule passe, avant toute ecriture de pose : tous les
        ///    <c>FixedUpdate</c> d'un pas observent le meme etat de fin de pas precedent ;
        /// 3. le gagnant sort d'un decompte de duels departage par un ordre total strict, donc il en
        ///    existe exactement un -- jamais zero. Un cycle de priorite a droite (quatre approches
        ///    simultanees) ne peut donc pas figer la jonction.
        ///
        /// La place sur la voie de sortie et l'autorisation du feu entrent, elles, dans la
        /// revendication : elles se lisent des deux cotes de facon identique (meme donnee, meme monde,
        /// meme horloge), et elles departagent legitimement deux Approches.
        /// </summary>
        private bool TickJunctionRules(DriverProfile profile, float dt, float speed, float longitudinalSpeed)
        {
            junctionSpeedLimitGap = float.PositiveInfinity;
            junctionHoldsWaypoint = false;

            // TRAVERSEE ENGAGEE. Un mouvement accorde va jusqu'au bout : c'est la contrepartie de
            // l'admission. Tant que le vehicule n'a pas degage, il n'arbitre plus -- il OCCUPE, et
            // c'est son occupation que les suivants lisent.
            if (committedJunctionKey > 0)
            {
                if (!HasClearedCommittedJunction())
                {
                    junctionPermission = "engage";
                    junctionDiagnosticId = committedJunctionKey.ToString();
                    return false;
                }

                ReleaseCommittedJunction();
            }

            if (laneGraph == null || state == null
                || !laneGraph.TryGetJunctionApproach(state.WaypointIndex.Value, out var approach))
            {
                ResetJunctionState();
                return false;
            }

            var node = laneGraph.GetNodePosition(approach.NodeIndex);
            var entryPoint = approach.EntryNode == approach.NodeIndex ? node : approach.EntryPoint;
            var entryForward = approach.EntryForward.sqrMagnitude > 0.0001f ? approach.EntryForward : approach.Forward;

            // LIGNE D'ARRET. Elle se deduit de la frontiere de conflit mesuree sur le graphe, reculee
            // de la demi-largeur du vehicule et de la marge authoree : c'est la distance en deca de
            // laquelle notre emprise commencerait a mordre sur la trajectoire d'une autre approche.
            // Elle vise l'AVANT du vehicule, pas son pivot -- viser le pivot laissait le capot dans
            // l'aire de conflit (ANO-5.18-03, observation 3).
            var conflictEntry = approach.ConflictEntryDistance > 0.01f
                ? approach.ConflictEntryDistance
                : Vector3.Dot(Vector3.ProjectOnPlane(node - entryPoint, Vector3.up), entryForward);
            var stopLineAlong = Mathf.Max(0f, conflictEntry - vehicleHalfExtents.x - profile.SafetyMargin);
            var frontAlong = Vector3.Dot(Vector3.ProjectOnPlane(transform.position - entryPoint, Vector3.up), entryForward)
                + frontOffset;
            junctionStopGap = stopLineAlong - frontAlong;

            // Hors de la zone d'approche, aucune revendication n'existe : la conduite nominale continue.
            if (junctionStopGap > ResolveJunctionApproachRadius(profile))
            {
                ResetJunctionState();
                return false;
            }

            var plannedExit = ResolveJunctionExit(approach);
            var exitPoint = plannedExit >= 0 ? laneGraph.GetNodePosition(plannedExit) : Vector3.zero;
            var distance = Mathf.Max(0f, conflictEntry - frontAlong);

            var ownProvisional = new JunctionClaim(approach.JunctionKey, approach.Rule, approach.Forward,
                distance, approach.NodeIndex, NetworkObjectId, true, true, true, false, entryPoint, exitPoint);
            GatherJunctionClaimants(profile, ownProvisional, node);

            // Le maintien a l'arret se compte sur le vehicule LUI-MEME, et il ne quitte jamais ce
            // fichier : c'est la valeur que personne d'autre ne peut observer.
            //
            // Une fois le maintien ACQUIS, il le reste jusqu'a la traversee. C'est la semantique d'un
            // stop -- on s'y arrete une fois -- et c'est aussi ce qui rend la sortie possible : tant
            // que la ligne d'arret etait un objectif a atteindre, remettre le compteur a zero des que
            // le vehicule redemarrait lui retirait son autorisation au pas suivant. Il refreinait,
            // reattendait 1,2 s, redemarrait -- mesure du banc de flux : immobile 28 s a 0,74 m de sa
            // ligne, maintien bloque a 0,30 s.
            if (approach.Rule != JunctionApproachRule.Stop)
            {
                junctionStopHeldSeconds = 0f;
            }
            else if (!junctionStopSatisfied)
            {
                junctionStopHeldSeconds = speed <= stuckSpeedThreshold && junctionStopGap <= arrivalRadius
                    ? junctionStopHeldSeconds + dt : 0f;
                junctionStopSatisfied = junctionStopHeldSeconds >= profile.JunctionStopHoldSeconds;
            }

            var exitRoom = ResolveApproachExitRoom(approach, profile, plannedExit);
            var signalAllows = ResolveSignalAllows(approach);
            var acceptedGap = JunctionRules.StopGapAccepted(nearestClaimantDistance, profile.JunctionAcceptedGap, claimantCount > 1);
            var heldLongEnough = approach.Rule != JunctionApproachRule.Stop || junctionStopSatisfied;

            claimantScratch[0] = new JunctionClaim(approach.JunctionKey, approach.Rule, approach.Forward,
                distance, approach.NodeIndex, NetworkObjectId, exitRoom, acceptedGap, signalAllows, false, entryPoint, exitPoint);

            var known = !claimantSetIncomplete;

            // ADMISSION, et non "suis-je LE vainqueur". Un vainqueur unique par jonction serialisait
            // des mouvements qui ne se croisent pas, et surtout il se calculait sur un ensemble
            // propre a chaque observateur -- deux vehicules pouvaient donc se croire vainqueurs en
            // meme temps (Story519ArbitrationConsistencyProbe).
            var admitted = JunctionRules.IsAdmitted(claimantScratch, claimantCount, 0, claimantWins, claimantOrder);
            var breachNow = !junctionBreached
                && JunctionRules.IsJunctionDeadlock(junctionWaitSeconds, junctionClaimantsHeld, profile.JunctionEscalationDelay)
                && (!known || admitted);
            if (breachNow)
            {
                junctionBreached = true;
                currentUnblockingAction = TrafficUnblockingAction.IntersectionBreach;
                decisionReason = TrafficDecisionReason.JunctionBreach;
                ReportJunctionDeadlock(approach);
            }

            var granted = junctionBreached
                || (known && admitted && exitRoom && signalAllows && acceptedGap && heldLongEnough);
            // Un feu rouge est la seule attente dont la FIN est garantie par la donnee authoree : le
            // plan la donne. C'est ce qui la distingue d'un blocage et ce qui empeche l'echelle de
            // recuperation de contourner un vehicule correctement arrete au feu.
            junctionHeldBySignal = !granted && !signalAllows;
            junctionPermission = granted ? (junctionBreached ? "forcee (deblocage)" : "accordee") : "refusee";
            junctionDiagnosticId = approach.JunctionId;

            if (granted)
            {
                junctionWaitSeconds = 0f;

                // On ne s'engage qu'a la ligne, jamais douze metres avant : occuper la jonction de
                // loin la fermerait aux autres pendant tout le trajet d'approche.
                // ... et jamais pendant qu'une cession de trajectoire court : la permission de
                // jonction arbitre les approches AUTHOREES, elle ne voit pas le conflit continu que
                // la perception vient de trancher. S'engager malgre lui avancerait le waypoint dans
                // l'aire de conflit alors que le vehicule freine encore vers sa ligne de cession.
                if (junctionStopGap <= 0f && !yieldingTrajectory)
                    CommitJunctionTraversal(approach, plannedExit, entryPoint, entryForward);
                return false;
            }

            junctionWaitSeconds += dt;
            junctionHoldsWaypoint = true;
            // Une sortie occupee n'est PAS un refus de priorite : les deux causes se corrigent
            // differemment, donc elles se nomment differemment.
            decisionReason = !exitRoom ? TrafficDecisionReason.ExitSaturated : TrafficDecisionReason.JunctionYield;
            trafficRefusal = ResolveJunctionRefusal(known, exitRoom, signalAllows, acceptedGap, heldLongEnough);
            ComposeDecisionDetail();

            // Refus AVANT la ligne : le vehicule ralentit dessus comme derriere un leader immobile.
            // C'est le modele longitudinal deja en place qui pose la decelaration, donc l'arret se
            // termine SUR la ligne au lieu de se declencher brutalement la ou le vehicule se trouve.
            if (junctionStopGap > 0f)
            {
                junctionSpeedLimitGap = junctionStopGap;
                return false;
            }

            appliedAcceleration = 0f;
            SubmitIntentToPhysicsLayer(ResolveStopIntent(longitudinalSpeed));
            return true;
        }

        /// <summary>
        /// Engage la traversee : le mouvement est accorde, le waypoint saute au noeud de sortie, et le
        /// connecteur de virage part de la FRONTIERE d'entree -- pas du noeud de decision, qui est pose
        /// au centre du carrefour et donc deja au-dela du coin du virage.
        /// </summary>
        private void CommitJunctionTraversal(in JunctionApproachInfo approach, int plannedExit,
            Vector3 entryPoint, Vector3 entryForward)
        {
            if (plannedExit < 0 || plannedExit == approach.NodeIndex) return;

            committedJunctionKey = approach.JunctionKey;
            committedApproachNode = approach.NodeIndex;
            committedExitNode = plannedExit;
            committedEntryPoint = entryPoint;
            committedEntryForward = entryForward;
            committedExitPoint = laneGraph.GetNodePosition(plannedExit);
            junctionPermission = "engage";
            junctionStopHeldSeconds = 0f;
            junctionStopSatisfied = false;

            // La sortie REELLEMENT prise fait foi : ResolveNextNode retombe sur un tirage ordinaire
            // quand le mouvement n'a pas ete fige, et l'occupation doit alors suivre le trajet
            // parcouru, pas celui qui avait ete envisage.
            state.WaypointIndex.Value = ResolveNextNode(approach.NodeIndex);
            committedExitNode = state.WaypointIndex.Value;
            committedExitPoint = laneGraph.GetNodePosition(committedExitNode);
        }

        /// <summary>
        /// Le vehicule a-t-il DEGAGE la jonction ? Son ARRIERE doit avoir depasse le noeud de sortie le
        /// long du sens de cette voie : tant qu'une partie de l'emprise reste en amont, l'aire de
        /// conflit est encore occupee.
        /// </summary>
        private bool HasClearedCommittedJunction()
        {
            if (committedExitNode < 0 || laneGraph == null) return true;

            var exitForward = Vector3.ProjectOnPlane(laneGraph.GetNodeRotation(committedExitNode) * Vector3.forward, Vector3.up);
            if (exitForward.sqrMagnitude <= 0.0001f) return true;
            exitForward.Normalize();

            var rearOffset = Mathf.Max(vehicleHalfExtents.z, frontOffset);
            var rearAlong = Vector3.Dot(Vector3.ProjectOnPlane(transform.position - committedExitPoint, Vector3.up), exitForward)
                - rearOffset;
            return rearAlong >= 0f;
        }

        private void ReleaseCommittedJunction()
        {
            committedJunctionKey = 0;
            committedApproachNode = -1;
            committedExitNode = -1;
            junctionPermission = string.Empty;
        }

        /// <summary>Fige le tirage de sortie avant l'arbitrage, sans ajouter d'etat reseau : cette IA est host-only.</summary>
        private int ResolveJunctionExit(in JunctionApproachInfo approach)
        {
            if (plannedJunctionNode == approach.NodeIndex && plannedJunctionExit >= 0) return plannedJunctionExit;

            var candidates = laneGraph.GetSuccessors(approach.NodeIndex);
            var nextEdge = traversedEdges + 1;
            if (candidates.Count == 0 || LaneGraphRouting.IsEdgeBudgetExceeded(nextEdge, laneGraph.NodeCount, ResolveEdgeBudgetFactor()))
                return approach.ExitProbeNode;

            var chosen = LaneGraphRouting.SelectWeightedSuccessor(candidates, laneGraph.GetTurnWeights(approach.NodeIndex),
                BuildEligibleSuccessorMask(candidates), NetworkObjectId, nextEdge, out var weightsInvalid);
            if (chosen < 0) return approach.ExitProbeNode;
            if (weightsInvalid) laneGraph.ReportInvalidTurnWeights(approach.NodeIndex);

            plannedJunctionNode = approach.NodeIndex;
            plannedJunctionExit = chosen;
            return chosen;
        }

        /// <summary>
        /// Rassemble les revendications concurrentes du pas. L'index 0 est la notre.
        ///
        /// Le tri se fait sur la JONCTION (meme cle), la VOIE (deux vehicules de la meme voie se
        /// poursuivent, ils ne s'arbitrent pas) et le CAP (deux approches opposees ne se croisent pas).
        /// C'est le meme predicat <see cref="JunctionRules.Conflicts"/> que consomme l'arbitrage : il
        /// n'existe donc pas deux definitions de "concurrent".
        ///
        /// L'ecart accepte de chaque revendication depend d'un COUPLE : il est calcule apres la
        /// collecte, sur les positions rassemblees. Chaque vehicule tire ainsi exactement les memes
        /// nombres du meme ensemble.
        /// </summary>
        private void GatherJunctionClaimants(DriverProfile profile, in JunctionClaim ownProvisional, Vector3 junctionPosition)
        {
            claimantCount = 1;
            claimantPositions[0] = transform.position;
            claimantAcceptedGap[0] = profile.JunctionAcceptedGap;
            junctionClaimantsHeld = false;
            claimantSetIncomplete = false;

            // La sphere est centree sur le NOEUD DE JONCTION, et non sur nous, pour une raison qui est
            // une garantie et pas un confort : tout revendiquant concurrent est a moins d'un rayon
            // d'approche de CE noeud, c'est le test que <see cref="TryDescribeJunctionClaim"/> lui
            // applique. La sphere les contient donc tous par construction.
            //
            // Centrer sur nous avec deux rayons couvrait le meme ensemble, mais en interrogeant une zone
            // qui n'a pas de rapport avec la jonction : mesure du 2026-09-20 dans le district, 36
            // colliders autour d'une approche, contre 24 autour du noeud, pour un tampon de 16 -- donc
            // une saturation a chaque appel.
            var radius = ResolveJunctionApproachRadius(profile);
            var found = Physics.OverlapSphereNonAlloc(junctionPosition, radius, junctionHits, ~0, QueryTriggerInteraction.Ignore);
            if (found >= junctionHits.Length)
            {
                // Sature : l'ensemble est incomplet, et rien ici ne permet de savoir s'il manque un
                // revendiquant. On le DIT a l'arbitrage au lieu de conclure sur ce qu'on a pu lire.
                claimantSetIncomplete = true;
                if (!warnedJunctionSaturation)
                {
                    warnedJunctionSaturation = true;
                    Debug.LogWarning("[Vehicles] " + name + " : tampon de revendications sature (" + junctionHits.Length
                        + "), un revendiquant concurrent a pu etre manque. L'approche attend et l'escalade la debloquera.", this);
                }
            }

            for (var i = 0; i < found; i++)
            {
                var hit = junctionHits[i];
                if (hit == null) continue;
                var other = hit.GetComponentInParent<NetworkedAIVehicleDriverController>();
                if (other == null || other == this || !other.IsSpawned) continue;
                if (claimantCount >= claimantScratch.Length)
                {
                    claimantSetIncomplete = true;
                    break;
                }

                // UN VEHICULE, UNE REVENDICATION. La sphere rend un collider par forme, et un vehicule
                // en porte plusieurs : sans ce filtre le meme pretendant entrait deux ou trois fois
                // dans l'ensemble arbitre, ce qui fausse le decompte de <see cref="JunctionRules.ResolveWinnerIndex"/>
                // et peut lui faire designer un gagnant different de celui que le pair designe --
                // c'est-a-dire casser la symetrie dont tout l'arbitrage depend.
                var duplicate = false;
                for (var k = 1; k < claimantCount; k++)
                {
                    if (claimantScratch[k].NetworkObjectId != other.NetworkObjectId) continue;
                    duplicate = true;
                    break;
                }

                if (duplicate) continue;

                if (!other.TryDescribeJunctionClaim(ResolveJunctionApproachRadius(profile), out var claim, out var otherGap, out var stationary)) continue;

                // ON NE FILTRE PLUS SUR "EN CONFLIT AVEC NOUS". L'admission est un parcours sur
                // l'ensemble entier (JunctionRules.IsAdmitted) : deux observateurs qui n'arbitrent
                // pas le meme ensemble peuvent se croire vainqueurs tous les deux, et c'est le
                // defaut mesure par Story519ArbitrationConsistencyProbe. Les revendications qui ne
                // nous croisent pas ne nous retiennent pas -- elles ne pesent que sur l'ordre.

                claimantScratch[claimantCount] = claim;
                claimantPositions[claimantCount] = other.transform.position;
                claimantAcceptedGap[claimantCount] = otherGap;
                claimantCount++;

                // "Un concurrent attend" ne se lit que sur les revendications qui NOUS DISPUTENT la
                // jonction. Depuis que l'ensemble rassemble la jonction entiere, compter tout
                // vehicule immobile alentour declenchait le palier de deblocage en permanence
                // (mesure : 523 pas en franchissement force pour un seul vehicule).
                if (JunctionRules.Conflicts(ownProvisional, claim)) junctionClaimantsHeld |= stationary;
            }

            // Un ensemble incomplet interdit d'affirmer "personne d'autre n'attend" : on ne peut pas
            // non plus affirmer le contraire. L'attente qui en decoule doit donc pouvoir se rompre,
            // sinon le tampon sature deviendrait un blocage permanent -- ce que la story interdit.
            if (claimantSetIncomplete)
            {
                junctionClaimantsHeld = true;
            }

            ApplySymmetricStopGaps();
        }

        /// <summary>
        /// Rayon d'approche effectif. La DONNEE DE MONDE prime le profil de conduite : l'ensemble
        /// des revendications d'une jonction doit etre le meme pour tous ses observateurs, sinon
        /// l'admission -- qui se calcule sur l'ensemble entier -- rend deux resultats differents.
        /// Le profil reste le repli tant que la scene n'authore rien, donc rien ne casse sans
        /// migration d'asset.
        /// </summary>
        private float ResolveJunctionApproachRadius(DriverProfile profile)
        {
            var settings = laneGraph != null ? laneGraph.TrafficSettings : null;
            var authored = settings != null ? settings.JunctionApproachRadius : 0f;
            return authored > 0f ? authored : profile.JunctionApproachRadius;
        }

        /// <summary>
        /// Ecart accepte de chaque revendication, calcule sur l'ensemble rassemble. La distance est
        /// planaire et symetrique : les deux protagonistes lisent le meme nombre, donc en tirent le meme
        /// verdict. C'est ce qui remplace le minuteur de maintien dans l'arbitrage.
        /// </summary>
        private void ApplySymmetricStopGaps()
        {
            if (claimantCount <= 1)
            {
                claimantScratch[0] = claimantScratch[0].WithStopSatisfied(true);
                nearestClaimantDistance = float.PositiveInfinity;
                return;
            }

            nearestClaimantDistance = float.PositiveInfinity;
            for (var i = 0; i < claimantCount; i++)
            {
                var nearest = float.PositiveInfinity;
                for (var j = 0; j < claimantCount; j++)
                {
                    if (i == j) continue;

                    // Seules les revendications qui SE CROISENT comptent pour l'ecart accepte. Depuis
                    // que l'ensemble rassemble contient toute la jonction, et plus seulement nos
                    // concurrents, un vehicule qui ne coupe pas notre mouvement ferait sinon refuser
                    // notre stop sans jamais nous gener.
                    if (!JunctionRules.Conflicts(claimantScratch[i], claimantScratch[j])) continue;
                    var separation = Vector3.ProjectOnPlane(claimantPositions[i] - claimantPositions[j], Vector3.up).magnitude;
                    if (separation < nearest) nearest = separation;
                }

                claimantScratch[i] = claimantScratch[i].WithStopSatisfied(
                    JunctionRules.StopGapAccepted(nearest, claimantAcceptedGap[i], true));

                if (i == 0) nearestClaimantDistance = nearest;
            }
        }

        /// <summary>
        /// Revendication d'un AUTRE vehicule, telle qu'un pair peut la reconstruire. Tout ce qui y entre
        /// est soit de la donnee authoree, soit du monde physique partage : la position, l'index de noeud
        /// replique, et le graphe. Rien de prive n'y entre -- c'est la condition de la symetrie.
        ///
        /// <paramref name="stationary"/> est le seul proxy observable d'un vehicule "en attente" : ce
        /// controle la, et lui seul, sert a reconnaitre un interblocage plutot qu'un vehicule simplement
        /// retenu derriere un autre.
        /// </summary>
        private bool TryDescribeJunctionClaim(float approachRadius, out JunctionClaim claim, out float acceptedGap, out bool stationary)
        {
            claim = default;
            acceptedGap = 0f;
            stationary = false;

            if (laneGraph == null || state == null || body == null) return false;

            var otherProfile = driverProfile != null ? driverProfile.Profile : default;
            acceptedGap = otherProfile.JunctionAcceptedGap;
            stationary = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude <= stuckSpeedThreshold;

            // OCCUPANT. Un vehicule engage dans la jonction publie toujours sa revendication, meme si
            // son waypoint a deja saute au noeud de sortie. C'est precisement ce qui manquait : sans
            // cela, il devenait invisible a l'arbitrage a l'instant ou il entrait, et le suivant
            // trouvait la jonction libre (ANO-5.18-03, observation 1).
            if (committedJunctionKey > 0 && committedApproachNode >= 0
                && laneGraph.TryGetJunctionApproach(committedApproachNode, out var engaged))
            {
                claim = new JunctionClaim(engaged.JunctionKey, engaged.Rule, engaged.Forward, 0f,
                    engaged.NodeIndex, NetworkObjectId, true, true, true, false,
                    committedEntryPoint, committedExitPoint, true);
                return true;
            }

            if (!laneGraph.TryGetJunctionApproach(state.WaypointIndex.Value, out var approach)) return false;

            var node = laneGraph.GetNodePosition(approach.NodeIndex);
            var entryPoint = approach.EntryNode == approach.NodeIndex ? node : approach.EntryPoint;
            var entryForward = approach.EntryForward.sqrMagnitude > 0.0001f ? approach.EntryForward : approach.Forward;
            var conflictEntry = approach.ConflictEntryDistance > 0.01f
                ? approach.ConflictEntryDistance
                : Vector3.Dot(Vector3.ProjectOnPlane(node - entryPoint, Vector3.up), entryForward);
            var frontAlong = Vector3.Dot(Vector3.ProjectOnPlane(transform.position - entryPoint, Vector3.up), entryForward)
                + frontOffset;
            var distance = Mathf.Max(0f, conflictEntry - frontAlong);
            if (conflictEntry - frontAlong > approachRadius) return false;

            var plannedExit = ResolveJunctionExit(approach);
            var exitPoint = plannedExit >= 0 ? laneGraph.GetNodePosition(plannedExit) : Vector3.zero;
            claim = new JunctionClaim(approach.JunctionKey, approach.Rule, approach.Forward, distance,
                approach.NodeIndex, NetworkObjectId, ResolveApproachExitRoom(approach, otherProfile, plannedExit), true,
                ResolveSignalAllows(approach), false, entryPoint, exitPoint);

            return true;
        }

        /// <summary>
        /// Place disponible sur la voie de sortie sondee. Un vehicule qui occupe la zone de degagement
        /// la rend saturee, et une place INDETERMINEE (approche sans voie de sortie sondee) est saturee
        /// elle aussi : une place qu'on ne peut pas mesurer n'est jamais lue comme libre.
        ///
        /// Seuls les USAGERS DE LA ROUTE comptent -- un vehicule, un pieton -- jamais le decor : un
        /// collider statique pres de la sortie condamnerait la jonction en permanence.
        /// </summary>
        private bool ResolveApproachExitRoom(in JunctionApproachInfo approach, DriverProfile profile, int plannedExit = -1)
        {
            exitSaturatedBy = string.Empty;
            if (laneGraph == null)
            {
                return false;
            }

            Vector3 exitPoint;
            if (plannedExit >= 0)
            {
                exitPoint = laneGraph.GetNodePosition(plannedExit);
            }
            else if (!laneGraph.TryGetJunctionExitProbe(approach.NodeIndex, out exitPoint))
            {
                return false;
            }

            // Axe de la VOIE DE SORTIE : du noeud de decision vers le noeud de sortie. Sans axe, la
            // question "reste-t-il de la place en aval" n'a pas de sens -- et c'est exactement ce qui
            // manquait a la version precedente.
            var entry = laneGraph.GetNodePosition(approach.NodeIndex);
            var lane = Vector3.ProjectOnPlane(exitPoint - entry, Vector3.up);
            if (lane.sqrMagnitude < 0.0001f)
            {
                lane = Vector3.ProjectOnPlane(approach.Forward, Vector3.up);
                if (lane.sqrMagnitude < 0.0001f) return false;
            }

            var direction = lane.normalized;

            // Longueur UTILE de voie en aval, pas un rayon. Le reglage authore est inchange ; c'est la
            // grandeur qu'il designe qui change de nature.
            var usable = profile.JunctionExitClearanceRadius;
            if (!(usable > 0f)) return false;
            var corridor = vehicleHalfExtents.x + profile.SafetyMargin;

            // PLACE REELLEMENT REQUISE pour degager la jonction : notre propre longueur, plus l'ecart
            // minimal authore derriere le vehicule qui nous precede. "Sortie saturee" veut dire que la
            // voie en aval n'offre pas cette longueur -- pas qu'un usager se trouve quelque part dans
            // la zone.
            var required = Mathf.Min(usable, 2f * vehicleHalfExtents.z + profile.MinimumGap);
            junctionExitUsableLength = usable;
            junctionExitRequiredLength = required;
            junctionExitFreeLength = usable;
            junctionExitOccupants = 0;

            var probe = exitPoint + direction * (usable * 0.5f);
            var found = Physics.OverlapSphereNonAlloc(probe, usable, junctionExitHits, ~0, QueryTriggerInteraction.Ignore);
            if (found >= junctionExitHits.Length)
            {
                // Sature : tout ce qui compte dans la zone n'a pas ete vu. Meme regle que la place
                // indeterminee -- lue comme saturee, jamais comme libre.
                exitSaturatedBy = "tampon sature";
                return false;
            }

            for (var i = 0; i < found; i++)
            {
                var hit = junctionExitHits[i];
                if (hit == null) continue;
                var hitBody = hit.attachedRigidbody;
                var walker = hit.GetComponentInParent<CharacterController>();

                // Seuls les USAGERS DE LA ROUTE occupent une voie. Le decor borde la chaussee, il ne
                // la remplit pas -- le compter condamnerait la jonction en permanence.
                if (hitBody == null && walker == null) continue;
                if (hitBody != null && hitBody == body) continue;

                ReadColliderGeometry(hit, out var occupantCenter, out var occupantExtents, out var occupantRotation);
                var occupantVelocity = hitBody != null ? hitBody.linearVelocity
                    : walker != null ? walker.velocity : Vector3.zero;
                if (!TrafficPerception.OccupiesExitLane(exitPoint, direction, usable, corridor,
                    occupantCenter, occupantExtents, occupantRotation, occupantVelocity, stuckSpeedThreshold)) continue;

                junctionExitOccupants++;
                var nearEdge = Vector3.Dot(Vector3.ProjectOnPlane(occupantCenter - exitPoint, Vector3.up), direction)
                    - TrafficPerception.ProjectExtent(occupantExtents, occupantRotation, direction);
                nearEdge = Mathf.Clamp(nearEdge, 0f, usable);
                if (nearEdge >= junctionExitFreeLength) continue;
                junctionExitFreeLength = nearEdge;

                exitSaturatedBy = hit.attachedRigidbody != null && hit.attachedRigidbody.gameObject != null
                    ? hit.attachedRigidbody.gameObject.name : hit.name;
            }

            // Le verdict porte sur une LONGUEUR, pas sur une presence. Un vehicule arrete a 5,50 m en
            // aval laisse la place de degager la jonction ; le compter comme saturation immobilisait
            // l'approche pour rien.
            if (junctionExitFreeLength >= required)
            {
                exitSaturatedBy = string.Empty;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Autorisation de feu de l'approche. Un plan absent ou un id de jonction inconnu laisse le
        /// comportement authore precedent : la fonction rend vrai et aucune regle de feu n'est inventee.
        /// </summary>
        private bool ResolveSignalAllows(in JunctionApproachInfo approach)
        {
            if (approach.Rule != JunctionApproachRule.TrafficLight) return true;
            var settings = laneGraph != null ? laneGraph.TrafficSettings : null;
            if (settings == null || !settings.TryGetSignalPlan(approach.JunctionId, out var plan)) return true;
            return JunctionRules.PhaseAllowsGroup(plan, Time.time, approach.SignalGroup);
        }

        private string ResolveJunctionRefusal(bool known, bool exitRoom, bool signalAllows, bool acceptedGap, bool heldLongEnough)
        {
            if (!known) return "jonction : revendications indeterminees";
            if (!exitRoom) return "jonction : voie de sortie saturee";
            if (!signalAllows) return "jonction : feu";
            if (!heldLongEnough) return "jonction : maintien a l'arret";
            if (!acceptedGap) return "jonction : ecart insuffisant";
            return "jonction : cede la priorite";
        }

        /// <summary>
        /// Journal de developpement de l'interblocage, avec les vehicules concernes. Borne a un
        /// avertissement par vehicule : un interblocage dure, et le journal n'est pas le remede.
        /// </summary>
        private void ReportJunctionDeadlock(in JunctionApproachInfo approach)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (warnedJunctionDeadlock) return;
            warnedJunctionDeadlock = true;

            var involved = NetworkObjectId.ToString();
            for (var i = 1; i < claimantCount; i++)
            {
                involved += ", " + claimantScratch[i].NetworkObjectId;
            }

            Debug.LogWarning("[Vehicles] " + name + " : interblocage a la jonction \"" + approach.JunctionId
                + "\" apres " + junctionWaitSeconds.ToString("0.0") + " s. Vehicules concernes : " + involved
                + ". Palier IntersectionBreach : le vehicule entre dans l'intersection qu'il tenait libre."
                + " Aucun retrait, aucune teleportation.", this);
#endif
        }

        private void ResetJunctionState()
        {
            junctionHeldBySignal = false;
            claimantCount = 0;
            nearestClaimantDistance = float.PositiveInfinity;
            junctionWaitSeconds = 0f;
            junctionStopHeldSeconds = 0f;
            junctionStopSatisfied = false;
            junctionClaimantsHeld = false;
            claimantSetIncomplete = false;
            junctionBreached = false;
            plannedJunctionNode = -1;
            plannedJunctionExit = -1;
            junctionStopGap = float.PositiveInfinity;
            junctionPermission = string.Empty;
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * vehicleHalfExtents.y * 2f,
                DescribeDecision() + System.Environment.NewLine + "Deblocage  : " + currentUnblockingAction
                + " / TTC " + collisionTime.ToString("0.0"));
            Gizmos.color = Color.cyan;
            for (var i = 1; i < ownPredictedPathCount; i++) Gizmos.DrawLine(ownPredictedPath[i - 1], ownPredictedPath[i]);
#endif
            Gizmos.color = maneuverPathSafe ? Color.green : Color.red;
            if (maneuverActive)
            {
                var sampleRadius = driverProfile != null ? driverProfile.Profile.PathSampleDistance : 0f;
                var point = transform.position;
                for (var i = maneuverTargetIndex; i < maneuverTargetCount; i++)
                {
                    Gizmos.DrawLine(point, maneuverTargets[i]);
                    Gizmos.DrawWireSphere(maneuverTargets[i], sampleRadius);
                    point = maneuverTargets[i];
                }
            }
            if (float.IsFinite(collisionTime))
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, collisionPosition);
            }
        }
    }
}
