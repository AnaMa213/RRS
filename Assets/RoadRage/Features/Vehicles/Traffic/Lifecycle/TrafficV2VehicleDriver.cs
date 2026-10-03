using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Lifecycle
{
    /// <summary>
    /// Faits d'interaction d'un pas (Story 5.33) : frame partagee, liante longitudinale, leader, obstacle du couloir
    /// balaye, blockers et requete du collecteur. Valeurs NaN ou vides quand le pas n'a ni perception ni arbitrage.
    /// </summary>
    public readonly struct V2InteractionRecord
    {
        /// <summary>FrameId global (epoque de la frame, de la commande et du composeur).</summary>
        public readonly ulong FrameId;
        public readonly bool Arbitrated;
        public readonly LongitudinalCandidateKind BindingKind;
        public readonly SpeedConstraint BindingConstraint;
        public readonly float TargetAccelerationMetersPerSecondSquared;
        public readonly float AppliedAccelerationMetersPerSecondSquared;
        public readonly bool Smoothed;
        public readonly PerceptionUnavailableReason PerceptionReason;
        public readonly RoadId LeaderId;
        /// <summary>Jeu percu pare-chocs a pare-chocs ; NaN sans leader.</summary>
        public readonly float LeaderGapMeters;
        public readonly float LeaderSpeedMetersPerSecond;
        /// <summary>Obstacle du couloir balaye le plus contraignant ; None sans obstacle.</summary>
        public readonly RoadId ObstacleId;
        public readonly PerceivedObstacleKind ObstacleKind;
        /// <summary>Distance percue a la face proche ; NaN sans obstacle du couloir balaye.</summary>
        public readonly float ObstacleNearMeters;
        /// <summary>Faits obstacles retenus par la perception (couloir balaye ou non).</summary>
        public readonly int ObstacleFacts;
        /// <summary>Faits obstacles retenus de genre WalkingPlayer.</summary>
        public readonly int WalkingPlayerFacts;
        public readonly int BlockerCount;
        /// <summary>Id du blocker dominant ; nul sans blocker.</summary>
        public readonly string DominantBlocker;
        public readonly int HazardQueryHits;
        public readonly bool HazardQuerySaturated;
        /// <summary>Maintien a l'arret D11 du pas ; Phase None sans arbitrage ou hors maintien.</summary>
        public readonly StopHoldState Hold;

        public V2InteractionRecord(ulong frameId, AgentObservation observation, LongitudinalDecision decision,
            IReadOnlyList<Blocker> blockers, HazardQueryReport hazardQuery)
        {
            FrameId = frameId;
            Arbitrated = decision != null;
            BindingKind = decision != null ? decision.Binding.Kind : default(LongitudinalCandidateKind);
            BindingConstraint = decision != null ? decision.Binding.Constraint : SpeedConstraint.None;
            TargetAccelerationMetersPerSecondSquared = decision != null ? decision.TargetAccelerationMetersPerSecondSquared : float.NaN;
            AppliedAccelerationMetersPerSecondSquared = decision != null ? decision.AppliedAccelerationMetersPerSecondSquared : float.NaN;
            Smoothed = decision != null && decision.Smoothed;
            PerceptionReason = decision != null ? decision.PerceptionReason : PerceptionUnavailableReason.None;
            LeaderId = RoadId.None; LeaderGapMeters = float.NaN; LeaderSpeedMetersPerSecond = float.NaN;
            ObstacleId = RoadId.None; ObstacleKind = default(PerceivedObstacleKind); ObstacleNearMeters = float.NaN;
            ObstacleFacts = 0; WalkingPlayerFacts = 0;
            if (observation.Perceived)
            {
                if (observation.Leader.Items.Count > 0)
                {
                    var leader = observation.Leader.Items[0];
                    LeaderId = leader.TrafficId; LeaderGapMeters = leader.GapMeters; LeaderSpeedMetersPerSecond = leader.SpeedMetersPerSecond;
                }
                ObstacleFacts = observation.Obstacles.Items.Count;
                for (int i = 0; i < observation.Obstacles.Items.Count; i++)
                    if (observation.Obstacles.Items[i].Kind == PerceivedObstacleKind.WalkingPlayer) WalkingPlayerFacts++;
            }
            if (decision != null && decision.ObstacleCandidates.Count > 0)
            {
                var obstacle = decision.ObstacleCandidates[0];
                ObstacleId = obstacle.SourceId; ObstacleKind = obstacle.ObstacleKind; ObstacleNearMeters = obstacle.GapMeters;
            }
            BlockerCount = blockers == null ? 0 : blockers.Count;
            Blocker dominant;
            DominantBlocker = BlockerTracker.TryGetDominant(blockers, out dominant) ? dominant.Id : null;
            HazardQueryHits = hazardQuery.Hits;
            HazardQuerySaturated = hazardQuery.Saturated;
            Hold = decision != null ? decision.Hold : default(StopHoldState);
        }
    }

    /// <summary>Un pas physique enregistre par le driver V2 (trace brute pour la borne entre deux pas).</summary>
    public readonly struct V2StepRecord
    {
        public readonly ulong Step;
        public readonly int TrackIndex;
        public readonly BodyState State;
        public readonly float RouteDistanceMeters;
        public readonly int Piece;
        public readonly RoadId ElementId;
        public readonly float StepDisplacementMeters;
        public readonly float LongitudinalSpeed;
        public readonly bool CeilingUnbounded;
        public readonly float CeilingMetersPerSecond;
        public readonly float LateralErrorMeters;
        public readonly float HeadingErrorDegrees;
        public readonly RoadLocationFlags LocationFlags;
        public readonly float TargetSpeedMetersPerSecond;
        public readonly VehicleDriveIntent Intent;
        public readonly bool Fallback;
        public readonly V2FallbackReason Reason;
        public readonly V2FallbackTerminal Terminal;
        public readonly V2ComposerDiagnostic Diagnostics;
        public readonly float MinimumDriveTorque;
        public readonly SpeedConstraint Binding;
        /// <summary>Ecart nominal e de la route (degres, contrat §8) ; l'ecart de cap nominal vaut -e.</summary>
        public readonly float NominalOffsetDegrees;
        /// <summary>Angle de roue implique par la pose nominale : tan delta = (L/a) tan e (degres).</summary>
        public readonly float NominalSteerDegrees;
        /// <summary>Taux de braquage implique par la pose nominale a la vitesse mesuree (degres/s).</summary>
        public readonly float NominalSteerRateDegreesPerSecond;
        /// <summary>Faux : NominalPoseInfeasible (braquage ou taux au-dela du profil declare).</summary>
        public readonly bool NominalFeasible;
        public readonly int GroundedWheels;
        /// <summary>Contrainte qui borne le plan au vehicule (SpeedPlan.LimitingConstraint) ; None sans plan.</summary>
        public readonly SpeedConstraint Limiting;
        /// <summary>Angle de roue vise par la commande (degres) ; NaN sans commande.</summary>
        public readonly float CommandedWheelAngleDegrees;
        /// <summary>Angle de roue applique par le corps physique (VehiclePhysicsBody.CurrentSteerAngleDegrees, degres).</summary>
        public readonly float AppliedWheelAngleDegrees;
        /// <summary>Reference ou empreinte hors de l'enveloppe de largeur (RoadLocation.OutsideWidthEnvelope) : seule cause
        /// de OutsideEnvelope qui compte comme sortie de route dans les criteres de contact (contrat §8, 2026-09-30).</summary>
        public readonly bool OutsideWidthEnvelope;
        /// <summary>Frame partagee, arbitrage, leader, obstacle, blockers et collecteur du pas (Story 5.33).</summary>
        public readonly V2InteractionRecord Interaction;

        public V2StepRecord(ulong step, int trackIndex, BodyState state, float routeDistance, int piece, RoadId elementId,
            float displacement, float speed, bool unbounded, float ceiling, float lateral, float heading,
            RoadLocationFlags locationFlags, float targetSpeed,
            VehicleDriveIntent intent, bool fallback, V2FallbackReason reason, V2FallbackTerminal terminal,
            V2ComposerDiagnostic diagnostics, float minimumDriveTorque, SpeedConstraint binding,
            float nominalOffsetDegrees, float nominalSteerDegrees, float nominalSteerRate, bool nominalFeasible,
            int groundedWheels, SpeedConstraint limiting = SpeedConstraint.None, float commandedWheelAngle = float.NaN,
            float appliedWheelAngle = float.NaN, bool outsideWidthEnvelope = false,
            V2InteractionRecord interaction = default(V2InteractionRecord))
        {
            OutsideWidthEnvelope = outsideWidthEnvelope;
            Interaction = interaction;
            Limiting = limiting; CommandedWheelAngleDegrees = commandedWheelAngle; AppliedWheelAngleDegrees = appliedWheelAngle;
            Step = step; TrackIndex = trackIndex; State = state; RouteDistanceMeters = routeDistance; Piece = piece;
            ElementId = elementId; StepDisplacementMeters = displacement; LongitudinalSpeed = speed;
            CeilingUnbounded = unbounded; CeilingMetersPerSecond = ceiling; LateralErrorMeters = lateral;
            HeadingErrorDegrees = heading; LocationFlags = locationFlags; TargetSpeedMetersPerSecond = targetSpeed;
            Intent = intent; Fallback = fallback; Reason = reason; Terminal = terminal;
            Diagnostics = diagnostics; MinimumDriveTorque = minimumDriveTorque; Binding = binding;
            NominalOffsetDegrees = nominalOffsetDegrees; NominalSteerDegrees = nominalSteerDegrees;
            NominalSteerRateDegreesPerSecond = nominalSteerRate; NominalFeasible = nominalFeasible;
            GroundedWheels = groundedWheels;
        }

        /// <summary>v / v*(s*), NaN quand le plafond n'est pas borne.</summary>
        public float SpeedRatio
        {
            get { return CeilingUnbounded ? float.NaN : CeilingMetersPerSecond > 0f ? LongitudinalSpeed / CeilingMetersPerSecond : float.PositiveInfinity; }
        }
    }

    /// <summary>
    /// Temps par etape et par vehicule (observation seulement, jamais un seuil). Depuis la 5.33, l'etape frame est la part
    /// du vehicule dans la frame partagee (collecteur compris), et le plan de vitesse inclut l'arbitrage longitudinal.
    /// </summary>
    public sealed class V2StageTimings
    {
        public int Steps;
        public double FrameMilliseconds, PerceptionMilliseconds, SpineMilliseconds, SpeedPlanMilliseconds, ComposeMilliseconds;
        public double MaxFrameMilliseconds, MaxPerceptionMilliseconds, MaxSpineMilliseconds, MaxSpeedPlanMilliseconds,
            MaxComposeMilliseconds;

        // Diagnostic de performance 5.33 : etapes hors des cinq historiques (preparation, instrumentation), sous-etapes
        // incluses dans les historiques (arbitrage dans le plan de vitesse, commande de suivi dans la composition) et octets
        // alloues par etape. Les cinq moyennes historiques gardent leur sens et leur somme.
        public double PrepareMilliseconds, ArbitrationMilliseconds, TrackMilliseconds, InstrumentationMilliseconds;
        public long PrepareBytes, FrameBytes, SpineBytes, PerceptionBytes, SpeedPlanBytes, ComposeBytes, InstrumentationBytes;

        internal void AddPrepare(double milliseconds, long bytes)
        {
            PrepareMilliseconds += milliseconds; PrepareBytes += bytes;
        }

        internal void AddDetail(double arbitration, double track, double instrumentation, long frameBytes, long spineBytes,
            long perceptionBytes, long planBytes, long composeBytes, long instrumentationBytes)
        {
            ArbitrationMilliseconds += arbitration; TrackMilliseconds += track; InstrumentationMilliseconds += instrumentation;
            FrameBytes += frameBytes; SpineBytes += spineBytes; PerceptionBytes += perceptionBytes; SpeedPlanBytes += planBytes;
            ComposeBytes += composeBytes; InstrumentationBytes += instrumentationBytes;
        }

        internal void Add(double frame, double perception, double spine, double plan, double compose)
        {
            Steps++;
            FrameMilliseconds += frame; PerceptionMilliseconds += perception; SpineMilliseconds += spine;
            SpeedPlanMilliseconds += plan; ComposeMilliseconds += compose;
            MaxFrameMilliseconds = Math.Max(MaxFrameMilliseconds, frame); MaxSpineMilliseconds = Math.Max(MaxSpineMilliseconds, spine);
            MaxPerceptionMilliseconds = Math.Max(MaxPerceptionMilliseconds, perception);
            MaxSpeedPlanMilliseconds = Math.Max(MaxSpeedPlanMilliseconds, plan);
            MaxComposeMilliseconds = Math.Max(MaxComposeMilliseconds, compose);
        }

        /// <summary>Cout total moyen par pas (ms) : somme des etapes.</summary>
        public double TotalMillisecondsPerStep
        {
            get
            {
                return (FrameMilliseconds + PerceptionMilliseconds + SpineMilliseconds + SpeedPlanMilliseconds + ComposeMilliseconds)
                    / Math.Max(1, Steps);
            }
        }

        public override string ToString()
        {
            int n = Math.Max(1, Steps);
            return string.Format(CultureInfo.InvariantCulture,
                "steps {0} / frame+localisation mean {1:0.###} ms max {2:0.###} / route+horizon+mouvement mean {3:0.###} ms max {4:0.###}"
                + " / plan de vitesse mean {5:0.###} ms max {6:0.###} / commande+composition mean {7:0.###} ms max {8:0.###}"
                + " / perception mean {9:0.###} ms max {10:0.###}",
                Steps, FrameMilliseconds / n, MaxFrameMilliseconds, SpineMilliseconds / n, MaxSpineMilliseconds,
                SpeedPlanMilliseconds / n, MaxSpeedPlanMilliseconds, ComposeMilliseconds / n, MaxComposeMilliseconds,
                PerceptionMilliseconds / n, MaxPerceptionMilliseconds);
        }
    }

    /// <summary>
    /// Episode de contact d'un collider de la caisse (contrat §8, regle de contact des campagnes). Le driver
    /// publie ; la classification (relief reconnu par la preuve 5.51, chaussee, autre) appartient au protocole
    /// de mesure. L'appui des pneus (raycasts de suspension) n'est jamais un contact.
    /// </summary>
    public sealed class V2ContactEpisode
    {
        /// <summary>Chemin de hierarchie "scene/.../objet", meme forme que la preuve 5.51.</summary>
        public string ColliderPath;
        public ulong FirstStep;
        public ulong LastStep;
        public float MaxImpulseNewtonSeconds;
        public float MaxRelativeNormalSpeed;
        public float SpeedAtFirstContact;
        public float MinimumSpeedDuringContact = float.PositiveInfinity;
    }

    /// <summary>Mesures conservees apres le despawn du vehicule, sans reference a un objet Unity detruit.</summary>
    public sealed class V2DriveRecord
    {
        /// <summary>Identite de trafic du vehicule (5.33) : attribue la trace apres le despawn.</summary>
        public RoadId TrafficId;
        public IReadOnlyList<V2StepRecord> Trace;
        public IReadOnlyList<ReferenceTrack> Tracks;
        public IReadOnlyList<V2ContactEpisode> ContactEpisodes;
        public GaugeBox Gauge;
        public V2StageTimings Timings;
        public float FixedDeltaTimeSeconds;
        public float MaxStepDisplacementMeters;
        public float MinimumDirectionSpeed;
        public bool HasReachedExitPortal;
        public int NominalPoseInfeasibleSteps;
        public int ReplanCount;
    }

    /// <summary>
    /// Driver V2 hote seul (Stories 5.31, 5.33). Il ne s'auto-cadence pas : l'ordonnanceur hote
    /// (<see cref="TrafficV2StepRunner"/>) lui demande d'abord son entree d'acteur (pose, empreinte de la caisse,
    /// vitesse), construit une seule TrafficFrame partagee par tous les vehicules V2, puis lui donne exactement un
    /// pas : PlanningSpine -> perception 5.32 -> SpeedPlan -> arbitrage longitudinal -> MotionCommand -> composeur ->
    /// un seul ApplyDriveIntent. Le FrameId global porte la frame, la commande et le composeur ; la trace garde un
    /// compteur de pas propre au vehicule, 1 au premier pas. Il n'ecrit jamais position, rotation ni vitesse : les
    /// clients recoivent le mouvement par le NetworkTransform seul. Retrait signale uniquement sur le corridor du
    /// portail de sortie, a s superieur ou egal au portail ; tout echec en route donne le repli, vehicule present.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(VehiclePhysicsBody))]
    public sealed class TrafficV2VehicleDriver : NetworkBehaviour
    {
        [SerializeField]
        [Tooltip("Profil de conduite authore. Non assigne : vehicule inerte, diagnostic unique, aucun repli code.")]
        private DriverProfileDef driverProfile;

        private Rigidbody body;
        private VehiclePhysicsBody physicsBody;
        private BoxCollider bodyCollider;
        private TrafficV2Admission admission;
        private TrafficV2Insertion insertion;
        private Portal exitPortal;
        private RoutePlan route;
        private RoadId previousElement;
        private VehicleDriveIntentComposer composer;
        private ulong stepCounter;
        private int pieceHint;
        private VehicleDriveIntent lastIntent = VehicleDriveIntent.Idle;
        private readonly List<ReferenceTrack> tracks = new List<ReferenceTrack>();
        private readonly List<V2StepRecord> trace = new List<V2StepRecord>();
        private readonly List<string> contacts = new List<string>();
        private readonly List<V2ContactEpisode> contactEpisodes = new List<V2ContactEpisode>();
        private readonly Dictionary<Collider, V2ContactEpisode> openContacts = new Dictionary<Collider, V2ContactEpisode>();
        private float lastSpeed;
        // Objectif intermediaire (contrat §4) : garde jusqu'a ce que la localisation quitte le mouvement vise.
        private RoadId viaMovement;
        private bool onViaMovement;
        private int stepPiece;
        private float stepDistance;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Stopwatch detailWatch = new Stopwatch();
        private long allocationMark;

        // Marqueurs de profilage (diagnostic de performance 5.33) : aucune influence sur la conduite.
        private static readonly ProfilerMarker PrepareMarker = new ProfilerMarker("TrafficV2.Driver.Prepare");
        private static readonly ProfilerMarker LocalizeMarker = new ProfilerMarker("TrafficV2.Driver.Localize");
        private static readonly ProfilerMarker SpineMarker = new ProfilerMarker("TrafficV2.Driver.Spine");
        private static readonly ProfilerMarker PerceptionMarker = new ProfilerMarker("TrafficV2.Driver.Perception");
        private static readonly ProfilerMarker SpeedPlanMarker = new ProfilerMarker("TrafficV2.Driver.SpeedPlan");
        private static readonly ProfilerMarker ArbitrationMarker = new ProfilerMarker("TrafficV2.Driver.Arbitration");
        private static readonly ProfilerMarker ComposeMarker = new ProfilerMarker("TrafficV2.Driver.Compose");
        private static readonly ProfilerMarker MotionCommandMarker = new ProfilerMarker("TrafficV2.Driver.MotionCommand");
        private static readonly ProfilerMarker InstrumentationMarker = new ProfilerMarker("TrafficV2.Driver.Instrumentation");

        /// <summary>Debut d'une etape mesuree : marqueur, chronometre et compteur d'allocation du thread.</summary>
        private void BeginStage(ProfilerMarker marker)
        {
            marker.Begin();
            stopwatch.Restart();
            allocationMark = GC.GetAllocatedBytesForCurrentThread();
        }

        /// <summary>Fin d'une etape mesuree : rend les octets alloues et le temps (ms).</summary>
        private long EndStage(ProfilerMarker marker, out double milliseconds)
        {
            milliseconds = stopwatch.Elapsed.TotalMilliseconds;
            long bytes = GC.GetAllocatedBytesForCurrentThread() - allocationMark;
            marker.End();
            return bytes;
        }
        private bool recordTrace;
        private bool warnedMissingDriverProfile;
        private bool warnedMissingPhysicsProfile;
        private GaugeBox gauge;
        private TrackingTolerance declared;
        private readonly TrackingToleranceResponse toleranceResponse = new TrackingToleranceResponse();
        // Story 5.33 : empreinte declaree de la caisse, tampon spatial reutilise, seul etat de l'arbitrage, blockers.
        private VehicleFootprint footprint;
        private readonly SpatialQueryBuffer spatialBuffer = new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity);
        private LongitudinalMemory longitudinalMemory;
        private IReadOnlyList<Blocker> blockers = BlockerTracker.Empty;
        // Pas prepare par l'ordonnanceur (phase 1), consomme par Step (phase 2).
        private bool stepPrepared;
        private BodyState preparedState;
        private float preparedSpeed;
        private VehicleFootprintPose preparedPose;
        private ReferenceTrack preparedTrack;
        private float preparedOffset;
        private float? preparedDisplacement;

        /// <summary>Empreinte du BoxCollider de caisse depuis le point de reference (contrat AD-45, H3 5.31).</summary>
        public VehicleFootprint Footprint { get { return footprint; } }
        /// <summary>FrameId global du dernier pas decide.</summary>
        public ulong LastFrameId { get; private set; }
        /// <summary>Observation du dernier pas ; Perceived faux sans perception.</summary>
        public AgentObservation LastObservation { get; private set; }
        /// <summary>Arbitrage du dernier pas ; nul sans plan accepte.</summary>
        public LongitudinalDecision LastLongitudinal { get; private set; }
        /// <summary>Ensemble de blockers du dernier pas.</summary>
        public IReadOnlyList<Blocker> Blockers { get { return blockers; } }
        /// <summary>Requete du collecteur emise pour ce vehicule au dernier pas.</summary>
        public HazardQueryReport LastHazardQuery { get; private set; }
        /// <summary>Vrai entre la phase 1 et la phase 2 d'un pas hote.</summary>
        internal bool StepPrepared { get { return stepPrepared; } }
        /// <summary>Requete de dangers du pas prepare : centree sur le point de reference, corps propre ecarte.</summary>
        internal HazardQuery HazardQuery { get { return new HazardQuery(TrafficId, preparedState.Position, body); } }

        /// <summary>Reponse 2a (Story 5.52) : verrouillee hors mesure au premier depassement d'epsilon_t.</summary>
        public TrackingToleranceResponse ToleranceResponse { get { return toleranceResponse; } }

        public DriverProfileDef DriverProfileDefinition { get { return driverProfile; } }
        public bool IsBound { get { return insertion != null; } }
        public RoadId TrafficId { get { return insertion == null ? RoadId.None : insertion.TrafficId; } }
        public RoadId ExitPortalId { get { return exitPortal.Id; } }
        public string MeasurementLabel { get; private set; }
        public VehicleCoverage VehicleCoverageVerdict { get; private set; }
        private bool reachedExitPortal;
        public bool HasReachedExitPortal { get { return reachedExitPortal && !toleranceResponse.Latched; } }
        public TrafficDecisionProjection LastProjection { get; private set; }
        public ComposedDrive LastComposed { get; private set; }
        public IReadOnlyList<V2StepRecord> Trace { get { return trace; } }
        public IReadOnlyList<ReferenceTrack> Tracks { get { return tracks; } }
        public IReadOnlyList<string> Contacts { get { return contacts; } }
        /// <summary>Episodes de contact de caisse, dans l'ordre d'apparition (chemin, pas, impulsion, vitesses).</summary>
        public IReadOnlyList<V2ContactEpisode> ContactEpisodes { get { return contactEpisodes; } }
        /// <summary>Pas ou la pose nominale exige un braquage ou un taux au-dela du profil (NominalPoseInfeasible).</summary>
        public int NominalPoseInfeasibleSteps { get; private set; }
        /// <summary>Time.fixedDeltaTime reellement en vigueur au premier pas decide (consigne de campagne).</summary>
        public float FixedDeltaTimeSeconds { get; private set; }
        public GaugeBox Gauge { get { return gauge; } }
        public int ReplanCount { get; private set; }
        /// <summary>Replanifications : pas, raison, element et s de la localisation.</summary>
        public List<string> Replans { get; } = new List<string>();
        public int IntentsApplied { get; private set; }
        public int ToleranceExceededCount { get; private set; }
        public int NegativeFallbackTorqueSteps { get; private set; }
        public float MaxStepDisplacementMeters { get; private set; }
        public V2StageTimings Timings { get; } = new V2StageTimings();

        public V2DriveRecord CaptureRecord()
        {
            return new V2DriveRecord {
                TrafficId = TrafficId,
                Trace = trace, Tracks = tracks, ContactEpisodes = contactEpisodes, Gauge = gauge, Timings = Timings,
                FixedDeltaTimeSeconds = FixedDeltaTimeSeconds, MaxStepDisplacementMeters = MaxStepDisplacementMeters,
                MinimumDirectionSpeed = physicsBody.Profile.MinimumDirectionSpeed,
                HasReachedExitPortal = HasReachedExitPortal, NominalPoseInfeasibleSteps = NominalPoseInfeasibleSteps,
                ReplanCount = ReplanCount
            };
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            physicsBody = GetComponent<VehiclePhysicsBody>();
            bodyCollider = GetComponent<BoxCollider>();
            footprint = FootprintOf(bodyCollider);
        }

        /// <summary>
        /// Empreinte declaree du BoxCollider de caisse, depuis l'origine (point de reference, H3 5.31) : sur le prefab
        /// actuel, avant et arriere 2,22 m, gauche et droite 1,03 m. Sans collider : non declaree (extents nulles).
        /// </summary>
        public static VehicleFootprint FootprintOf(BoxCollider box)
        {
            if (box == null) return default(VehicleFootprint);
            Vector3 center = box.center, half = box.size * 0.5f;
            return new VehicleFootprint { ReferenceOriginLocal = Vector3.zero, FrontMeters = center.z + half.z,
                RearMeters = half.z - center.z, RightMeters = center.x + half.x, LeftMeters = half.x - center.x };
        }

        /// <summary>
        /// Contexte pose par le spawner hote AVANT le spawn : modele et preuve admis, insertion preparee
        /// (identite, graine, route de la premiere decision), etiquette de mesure et couverture publiee.
        /// </summary>
        public void Bind(TrafficV2Admission admittedModel, TrafficV2Insertion prepared, TrafficV2Verdict verdict,
            bool recordStepTrace)
        {
            if (admittedModel == null || !admittedModel.Admitted) throw new ArgumentException("AdmissionRefused", "admittedModel");
            if (prepared == null || prepared.Code != TrafficV2Code.Allowed || prepared.Route == null)
                throw new ArgumentException("InsertionNotDrivable", "prepared");
            admission = admittedModel;
            insertion = prepared;
            route = prepared.Route;
            MeasurementLabel = verdict.MeasurementLabel;
            VehicleCoverageVerdict = verdict.VehicleCoverage;
            declared = TrafficV2Settings.DeclaredTrackingTolerance;
            recordTrace = recordStepTrace;
            previousElement = prepared.EntryPortal.CorridorId;
            viaMovement = prepared.ViaMovementId;
            onViaMovement = false;
            for (int i = 0; i < admittedModel.Model.Portals.Count; i++)
                if (admittedModel.Model.Portals[i].Id == route.ExitPortalId) exitPortal = admittedModel.Model.Portals[i];
            if (bodyCollider == null) bodyCollider = GetComponent<BoxCollider>();
            footprint = FootprintOf(bodyCollider);
            float bottom = bodyCollider != null ? bodyCollider.center.y - bodyCollider.size.y * 0.5f : 0f;
            float top = bodyCollider != null ? bodyCollider.center.y + bodyCollider.size.y * 0.5f : 0f;
            var profile = admittedModel.Model.ValidationProfile;
            gauge = new GaugeBox(profile.MaxVehicleHalfWidthMeters, profile.MaxVehicleLengthMeters, bottom, top);
            // Insertion au portail, tangente : e = 0 au s effectif du portail (debut de la premiere occurrence).
            AdoptRoute(route, 0f);
        }

        public override void OnNetworkSpawn()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            if (body != null) body.isKinematic = !IsServer;
        }

        /// <param name="initialOffsetRadians">0 a l'insertion ; lors d'un replan, l'ecart courant (jamais remis a zero).</param>
        private void AdoptRoute(RoutePlan plan, float initialOffsetRadians)
        {
            route = plan;
            tracks.Add(ReferenceTrack.FromRoute(admission.Model, plan.Occurrences, initialOffsetRadians));
            pieceHint = plan.ProgressOccurrenceIndex;
        }

        /// <summary>
        /// Phase 1 d'un pas hote (5.33) : echantillonne la caisse et rend l'entree d'acteur de la frame partagee, avec
        /// l'empreinte declaree. Faux si le vehicule n'est pas lie et spawne cote hote. Un vehicule inerte (profil absent)
        /// reste un acteur de la frame mais ne prepare aucun pas.
        /// </summary>
        internal bool TryPrepareStep(out TrafficActorInput input)
        {
            BeginStage(PrepareMarker);
            bool prepared = PrepareStep(out input);
            double milliseconds;
            long bytes = EndStage(PrepareMarker, out milliseconds);
            Timings.AddPrepare(milliseconds, bytes);
            return prepared;
        }

        private bool PrepareStep(out TrafficActorInput input)
        {
            input = default(TrafficActorInput);
            stepPrepared = false;
            if (!IsServer || !IsSpawned || insertion == null || body == null || physicsBody == null) return false;
            bool drivable = WarnIfInert();
            var state = new BodyState(body.position, body.rotation, body.worldCenterOfMass, body.linearVelocity, body.angularVelocity);
            float speed = 0f;
            VehicleSuspensionModel.TelemetrySample telemetry;
            if (physicsBody.HasProfile && physicsBody.TrySampleTelemetry(out telemetry)) speed = telemetry.LongitudinalSpeed;
            if (float.IsNaN(speed) || float.IsInfinity(speed)) speed = 0f;
            var pose = new VehicleFootprintPose { Position = state.Position, Forward = state.Rotation * Vector3.forward,
                Up = state.Rotation * Vector3.up, Footprint = footprint };
            // Pose nominale courante (contrat §8) : l'ecart e du vehicule sur sa reference fixe le plafond de braquage
            // de l'horizon et l'orientation attendue des candidats de localisation ; un replan le reprend (jamais remis a zero).
            var current = tracks[tracks.Count - 1];
            int currentPiece;
            float currentDistance = current.Project(state.Position, pieceHint, out currentPiece);
            input = new TrafficActorInput(insertion.TrafficId, pose, speed, previousElement, TrafficV2Lifecycle.ExpectedElements(route),
                current.KinematicAnchors(currentPiece));
            if (!drivable) return true;

            if (composer == null)
            {
                float dt = Time.fixedDeltaTime;
                composer = new VehicleDriveIntentComposer(physicsBody.Profile, driverProfile.Profile.SafeBrakingLimit, dt);
                FixedDeltaTimeSeconds = dt;
            }
            ulong step = ++stepCounter;
            lastSpeed = speed;
            preparedState = state; preparedSpeed = speed; preparedPose = pose; preparedTrack = current;
            preparedOffset = current.OffsetRadians(currentPiece, currentDistance);
            // Hors mesure, observer avant toute progression/replanification ou detection de sortie.
            preparedDisplacement = MeasurementLabel == null
                ? ObserveTrackingTolerance(step, state, current, currentDistance) : (float?)null;
            stepPrepared = true;
            return true;
        }

        /// <summary>Vrai si le vehicule peut conduire ; sinon diagnostic unique et vehicule inerte.</summary>
        private bool WarnIfInert()
        {
            if (driverProfile == null)
            {
                if (!warnedMissingDriverProfile)
                {
                    warnedMissingDriverProfile = true;
                    UnityEngine.Debug.LogWarning("[Traffic V2] " + name + " : aucun DriverProfileDef, vehicule inerte.", this);
                }
                return false;
            }
            if (!physicsBody.HasProfile)
            {
                if (!warnedMissingPhysicsProfile)
                {
                    warnedMissingPhysicsProfile = true;
                    UnityEngine.Debug.LogWarning("[Traffic V2] " + name + " : aucun profil physique applique, vehicule inerte.", this);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Phase 2 d'un pas hote (5.33) : decide depuis la frame partagee fournie, et aucune autre, puis applique
        /// exactement un intent. Frame nulle (construction refusee) : repli V2, raison FrameUnavailable.
        /// </summary>
        /// <param name="frameShareMilliseconds">Part du vehicule dans le temps de la frame partagee, collecteur compris.</param>
        internal void Step(ulong frameId, TrafficFrame frame, HazardQueryReport hazardQuery,
            TrafficHazardCollectorCounters collector, double frameShareMilliseconds)
        {
            if (!stepPrepared) return;
            stepPrepared = false;
            var driver = driverProfile.Profile;
            var model = admission.Model;
            float dt = Time.fixedDeltaTime;
            var state = preparedState;
            float speed = preparedSpeed;
            var pose = preparedPose;
            var current = preparedTrack;
            float offset = preparedOffset;
            float? observedDisplacement = preparedDisplacement;
            LastFrameId = frameId;
            LastHazardQuery = hazardQuery;

            BeginStage(LocalizeMarker);
            TrafficActor actor = default(TrafficActor);
            bool located = frame != null && frame.TryGetActor(insertion.TrafficId, out actor);
            if (located && actor.Location.Localized) previousElement = actor.Location.ElementId;
            if (located && !viaMovement.IsEmpty && actor.Location.Localized)
            {
                // Franchi seulement quand la localisation passe du mouvement vise a l'element suivant.
                if (actor.Location.ElementId == viaMovement) onViaMovement = true;
                else if (onViaMovement) { viaMovement = RoadId.None; onViaMovement = false; }
            }
            if (located && !toleranceResponse.Latched && !HasReachedExitPortal && TrafficV2Lifecycle.HasReachedExit(actor.Location, exitPortal))
                reachedExitPortal = true;
            double localizeMs;
            long frameBytes = EndStage(LocalizeMarker, out localizeMs);
            double frameMs = frameShareMilliseconds + localizeMs;

            BeginStage(SpineMarker);
            PlanningDecision decision = null;
            if (located && !toleranceResponse.Latched)
            {
                try
                {
                    decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, route, insertion.ExitPortalId,
                        insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared,
                        null, null, admission.Evidence, viaMovement, current.HasKinematicPose ? offset : (float?)null,
                        PlanningReach.For(driver, speed, Math.Max(Math.Max(0f, speed) * dt, MotionCommand.PreviewFloorMeters),
                            TrafficV2Settings.PlanningReachMarginMeters)));
                }
                catch (ArgumentException) { decision = null; }
                catch (InvalidOperationException) { decision = null; }
            }
            if (decision != null && decision.Route.Plan != null && decision.Route.Plan != route)
            {
                if (decision.Route.Outcome == RouteOutcome.Replanned)
                {
                    ReplanCount++;
                    Replans.Add(stepCounter.ToString(CultureInfo.InvariantCulture) + ":" + decision.Route.Reason + ":"
                        + actor.Location.ElementId + "@" + actor.Location.SMeters.ToString("0.###", CultureInfo.InvariantCulture));
                    // La caisse ne saute pas : la nouvelle reference reprend l'ecart courant de l'ancienne.
                    AdoptRoute(decision.Route.Plan, offset);
                }
                else route = decision.Route.Plan;
                // Une progression du plan ne deplace pas la caisse : la projection de mesure
                // garde son morceau jusqu'a ce que la caisse atteigne effectivement le suivant.
            }
            // Franchi aussi quand la progression acquise depasse l'occurrence visee (localisation au-dela).
            if (!viaMovement.IsEmpty && route != null && route.ViaMovementId == viaMovement
                && route.ViaOccurrenceIndex >= 0 && route.ProgressOccurrenceIndex > route.ViaOccurrenceIndex)
            {
                viaMovement = RoadId.None;
                onViaMovement = false;
            }
            double spineMs;
            long spineBytes = EndStage(SpineMarker, out spineMs);

            // Perception 5.32 sur la frame partagee et l'horizon de la decision : faits, jamais une decision.
            BeginStage(PerceptionMarker);
            var observation = default(AgentObservation);
            if (decision != null && decision.Path != null && decision.PerceptionPath != null)
            {
                try
                {
                    // Perception sur toute la route restante, planification bornee (D14).
                    observation = TrafficPerception.Observe(frame, insertion.TrafficId, decision.PerceptionPath,
                        TrafficV2Settings.PerceptionLimits, spatialBuffer);
                }
                catch (ArgumentException) { observation = default(AgentObservation); }
            }
            double perceptionMs;
            long perceptionBytes = EndStage(PerceptionMarker, out perceptionMs);

            BeginStage(SpeedPlanMarker);
            double arbitrationMs = 0d, trackMs = 0d;
            SpeedPlan plan = null;
            MotionCommand? command = null;
            LongitudinalDecision longitudinal = null;
            var refusal = V2FallbackReason.NoCommand;
            if (frame == null) refusal = V2FallbackReason.FrameUnavailable;
            else if (decision == null || decision.Motion == null) refusal = V2FallbackReason.NoRoute;
            else
            {
                plan = SpeedPlan.Build(decision.Motion, model, driver, Math.Max(0f, speed),
                    physicsBody.Profile.LateralFrictionCoefficient * Physics.gravity.magnitude);
                if (!plan.Accepted)
                    refusal = HasReachedExitPortal ? V2FallbackReason.ExitPortalReached
                        : plan.Issue == SpeedPlanIssue.PlanInfeasible
                        ? (plan.PlanIssue == MotionIssue.HorizonNonConforming ? V2FallbackReason.HorizonNonConforming : V2FallbackReason.PlanInfeasible)
                        : V2FallbackReason.ProfileRefused;
                else
                {
                    // Arbitrage longitudinal (5.33) : le profil 5.31, la route libre et les contraintes d'interaction.
                    ArbitrationMarker.Begin();
                    detailWatch.Restart();
                    var perceived = LongitudinalPerception.From(observation, decision.PerceptionPath,
                        LongitudinalPerception.FrontDistanceMeters(frame, insertion.TrafficId, decision.PerceptionPath), hazardQuery.Saturated);
                    longitudinal = LongitudinalArbitration.Decide(plan, driver, speed, dt, perceived, longitudinalMemory,
                        TrafficV2Settings.StopHold);
                    arbitrationMs = detailWatch.Elapsed.TotalMilliseconds;
                    ArbitrationMarker.End();
                }
            }
            double planMs;
            long planBytes = EndStage(SpeedPlanMarker, out planMs);

            BeginStage(ComposeMarker);
            // Progression sur la reference de mesure : elle porte aussi le cap nominal que vise la commande.
            var track = tracks[tracks.Count - 1];
            stepDistance = track.Project(state.Position, pieceHint, out stepPiece);
            pieceHint = stepPiece;
            float nominalHeading = track.NominalHeadingErrorDegrees(stepPiece, stepDistance);
            if (plan != null && plan.Accepted && decision.Path.Intervals.Count > 0)
            {
                var first = decision.Path.Intervals[0];
                RoadCurve curve = null;
                EffectiveLaneCorridor corridor;
                CompiledJunctionMovement movement;
                if (first.Kind == RoadElementKind.LaneCorridor && model.TryGetCorridor(first.Id, out corridor)) curve = corridor.Curve;
                else if (first.Kind == RoadElementKind.JunctionMovement && model.TryGetMovement(first.Id, out movement)) curve = movement.Curve;
                if (curve != null)
                {
                    MotionCommandMarker.Begin();
                    detailWatch.Restart();
                    command = MotionCommand.Track(frameId, TrafficV2Settings.PlanValiditySteps, plan, driver,
                        model.DrivabilityProfile, curve, first.StartSMeters, pose.Position, pose.Forward, speed, dt,
                        nominalHeading, longitudinal);
                    trackMs = detailWatch.Elapsed.TotalMilliseconds;
                    MotionCommandMarker.End();
                }
            }
            if (toleranceResponse.Latched)
            {
                // Decision 2a (Story 5.52) : hors mesure, la borne depassee retire toute commande ; repli V2 jusqu'a
                // l'arret maintenu, sans recuperation (5.39), vehicule present et physiquement libre.
                command = null;
                refusal = V2FallbackReason.TrackingToleranceExceeded;
            }

            var composed = composer.Compose(frameId, command, refusal, speed, body.linearDamping);
            // Seul point d'application V2 : un intent par pas physique.
            physicsBody.ApplyDriveIntent(composed.Intent, composed.MaxForwardSpeed, composed.SteerRateDegreesPerSecond,
                composed.BrakeTorque);
            IntentsApplied++;
            double composeMs;
            long composeBytes = EndStage(ComposeMarker, out composeMs);
            Timings.Add(frameMs, perceptionMs, spineMs, planMs, composeMs);

            // Instrumentation : etat publie, blockers, trace au pas et projection de diagnostic.
            BeginStage(InstrumentationMarker);
            LastComposed = composed;

            // Seul etat de l'arbitrage (D5) et ensemble de blockers : ceux d'une commande effectivement composee.
            bool commanded = longitudinal != null && command.HasValue && !composed.Fallback;
            longitudinalMemory = commanded ? longitudinal.Memory : LongitudinalMemory.None;
            blockers = BlockerTracker.Update(blockers, commanded ? longitudinal : null, driver, frameId);
            LastObservation = observation;
            LastLongitudinal = commanded ? longitudinal : null;

            Monitor(stepCounter, state, speed, actor.Location.Flags, actor.Location.OutsideWidthEnvelope, composed, command, plan,
                observedDisplacement, new V2InteractionRecord(frameId, observation, LastLongitudinal, blockers, hazardQuery));
            var projection = decision != null ? decision.Projection : LastProjection;
            if (projection != null)
                LastProjection = projection.WithDrive(DriveOutcome(frameId, composed, plan, driver)).WithLongitudinal(decision == null
                    ? null : new TrafficLongitudinalOutcome(frameId, observation, LastLongitudinal, plan == null ? null : plan.RoadLimits,
                        blockers, hazardQuery.Hits, hazardQuery.Saturated, collector));
            lastIntent = composed.Intent;
            double instrumentationMs;
            long instrumentationBytes = EndStage(InstrumentationMarker, out instrumentationMs);
            Timings.AddDetail(arbitrationMs, trackMs, instrumentationMs, frameBytes, spineBytes, perceptionBytes, planBytes,
                composeBytes, instrumentationBytes);
        }

        private float ObserveTrackingTolerance(ulong step, BodyState state, ReferenceTrack track, float distance)
        {
            float displacement = TrackingMeasurement.StepDisplacement(state, track, distance, gauge);
            MaxStepDisplacementMeters = Math.Max(MaxStepDisplacementMeters, displacement);
            if (TrackingToleranceResponse.Exceeds(declared, displacement))
            {
                ToleranceExceededCount++;
                if (ToleranceExceededCount == 1)
                    UnityEngine.Debug.LogWarning("[Traffic V2] TrackingToleranceExceeded : " + name + " d = "
                        + displacement.ToString("0.####", CultureInfo.InvariantCulture) + " m au pas " + step + ".", this);
                if (toleranceResponse.Observe(step, MeasurementLabel != null, declared, displacement))
                    UnityEngine.Debug.LogWarning("[Traffic V2] TrackingToleranceExceeded hors mesure : " + name
                        + " passe en repli V2 jusqu'a l'arret maintenu (Story 5.52, decision 2a).", this);
            }
            return displacement;
        }

        private void Monitor(ulong step, BodyState state, float speed, RoadLocationFlags locationFlags, bool outsideWidth,
            ComposedDrive composed, MotionCommand? command, SpeedPlan plan, float? observedDisplacement,
            V2InteractionRecord interaction)
        {
            var track = tracks[tracks.Count - 1];
            int piece = stepPiece;
            float distance = stepDistance;
            float displacement = observedDisplacement ?? ObserveTrackingTolerance(step, state, track, distance);
            var trackPiece = track.Pieces[piece];
            var drivability = admission.Model.DrivabilityProfile;
            float curvature = trackPiece.Curve.Sample(trackPiece.ElementS(distance)).CurvaturePerMeter;
            // v*(s*) : plafond de la pose nominale (meme fonction que l'horizon), regime etabli sans pose cinematique.
            float ceiling = track.HasKinematicPose
                ? PathHorizon.NominalSteeringCeilingMetersPerSecond(drivability, track.OffsetRadians(piece, distance))
                : RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(drivability, curvature);
            if (piece > 0 && distance <= trackPiece.StartDistanceMeters)
            {
                var left = track.Pieces[piece - 1];
                ceiling = Math.Min(ceiling, track.HasKinematicPose
                    ? PathHorizon.NominalSteeringCeilingMetersPerSecond(drivability, track.OffsetRadians(piece - 1, distance))
                    : RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(drivability,
                        left.Curve.Sample(left.ElementS(distance)).CurvaturePerMeter));
            }
            // Couple moteur au moment de l'application, dans les deux ordres possibles de FixedUpdate.
            var vehicle = physicsBody.Profile;
            float torque = Math.Min(
                VehicleDriveIntentComposer.MinimumWheelDriveTorque(vehicle, composed.Intent, composed.MaxForwardSpeed, speed),
                VehicleDriveIntentComposer.MinimumWheelDriveTorque(vehicle, lastIntent, composed.MaxForwardSpeed, speed));
            if (composed.Fallback && torque < 0f) NegativeFallbackTorqueSteps++;

            // Faisabilite de la pose nominale (contrat §8) : tan delta = (L/a) tan e, dans le braquage disponible
            // a la vitesse mesuree ; taux v |d delta / ds| dans le taux de braquage declare du vehicule.
            float e = track.OffsetRadians(piece, distance);
            float nominalSteer;
            float steerRate;
            bool feasible = NominalPoseFeasibility.FeasibleAtSpeed(drivability, e, curvature, speed,
                vehicle.SteerRateDegreesPerSecond, out nominalSteer, out steerRate) || !track.HasKinematicPose;
            if (!feasible)
            {
                NominalPoseInfeasibleSteps++;
                if (NominalPoseInfeasibleSteps == 1)
                    UnityEngine.Debug.LogWarning("[Traffic V2] NominalPoseInfeasible : " + name + " au pas " + step
                        + " (braquage " + F(nominalSteer) + " deg, taux " + F(steerRate) + " deg/s, v " + F(speed) + " m/s).", this);
            }

            if (!recordTrace) return;
            trace.Add(new V2StepRecord(step, tracks.Count - 1, state, distance, piece, trackPiece.Id, displacement, speed,
                float.IsPositiveInfinity(ceiling), float.IsPositiveInfinity(ceiling) ? 0f : ceiling,
                command.HasValue ? command.Value.LateralErrorMeters : float.NaN,
                command.HasValue ? command.Value.HeadingErrorDegrees : float.NaN, locationFlags,
                plan != null && plan.Accepted ? plan.SpeedAt(Math.Max(speed * Time.fixedDeltaTime,
                    MotionCommand.PreviewFloorMeters)) : 0f, composed.Intent, composed.Fallback,
                composed.Reason, composed.Terminal, composed.Diagnostics, torque,
                plan != null ? plan.Binding : SpeedConstraint.None, e * Mathf.Rad2Deg, nominalSteer, steerRate, feasible,
                physicsBody.GroundedWheelCount, plan != null ? plan.LimitingConstraint : SpeedConstraint.None,
                command.HasValue ? command.Value.TargetWheelAngleDegrees : float.NaN, physicsBody.CurrentSteerAngleDegrees, outsideWidth,
                interaction));
        }

        /// <param name="frameId">Epoque de decision : FrameId global. L'epoque physique est le pas propre au vehicule.</param>
        private TrafficDriveOutcome DriveOutcome(ulong frameId, ComposedDrive composed, SpeedPlan plan, DriverProfile driver)
        {
            var applied = new List<string>();
            string binding = "None";
            if (plan != null)
            {
                binding = plan.Binding.ToString();
                applied.Add("DesiredSpeed " + F(driver.DesiredSpeed));
                // Limite de route (5.33) : seule une valeur authoree est une contrainte appliquee ; Unauthored n'en est pas une.
                float road = float.PositiveInfinity;
                foreach (var limit in plan.RoadLimits) road = Math.Min(road, limit.CapMetersPerSecond);
                if (!float.IsPositiveInfinity(road)) applied.Add("RoadLimit " + F(road));
                float ceiling = float.PositiveInfinity;
                foreach (var point in plan.Points) if (!point.CeilingUnbounded) ceiling = Math.Min(ceiling, point.CeilingMetersPerSecond);
                applied.Add("SteeringCeiling " + (float.IsPositiveInfinity(ceiling) ? "aucun" : F(ceiling)));
                applied.Add("CurveLimit a_lat " + F(plan.LateralAccelerationMetersPerSecondSquared));
                applied.Add("LongitudinalBounds " + F(driver.MaxAcceleration) + "/" + F(plan.PlanningDecelerationMetersPerSecondSquared)
                    + " verifie " + F(driver.MaxAcceleration) + "/" + F(driver.SafeBrakingLimit));
            }
            // Plus aucune limite reportee depuis la 5.33 : la liste reportee reste vide.
            return new TrafficDriveOutcome(frameId, stepCounter, composed.SourceFrameId, composed.Intent.Throttle, composed.Intent.Steer,
                composed.Intent.BrakeReverse, composed.Intent.Handbrake, composed.Fallback, composed.Reason.ToString(), binding,
                applied, null, VehicleCoverageVerdict, MeasurementLabel);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsServer || collision == null || collision.collider == null) return;
            contacts.Add(stepCounter.ToString(CultureInfo.InvariantCulture) + ":" + collision.collider.name);
            var episode = new V2ContactEpisode { ColliderPath = HierarchyPath(collision.collider.transform),
                FirstStep = stepCounter, LastStep = stepCounter, SpeedAtFirstContact = lastSpeed };
            contactEpisodes.Add(episode);
            openContacts[collision.collider] = episode;
            Accumulate(episode, collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!IsServer || collision == null || collision.collider == null) return;
            V2ContactEpisode episode;
            if (openContacts.TryGetValue(collision.collider, out episode)) Accumulate(episode, collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision != null && collision.collider != null) openContacts.Remove(collision.collider);
        }

        private void Accumulate(V2ContactEpisode episode, Collision collision)
        {
            episode.LastStep = stepCounter;
            episode.MaxImpulseNewtonSeconds = Math.Max(episode.MaxImpulseNewtonSeconds, collision.impulse.magnitude);
            episode.MinimumSpeedDuringContact = Math.Min(episode.MinimumSpeedDuringContact, lastSpeed);
            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                episode.MaxRelativeNormalSpeed = Math.Max(episode.MaxRelativeNormalSpeed,
                    Math.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal)));
            }
        }

        /// <summary>Chemin "scene/parent/.../objet", meme forme que les entrees de la preuve 5.51.</summary>
        private static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            for (Transform t = transform.parent; t != null; t = t.parent) path = t.name + "/" + path;
            return transform.gameObject.scene.name + "/" + path;
        }

        private static string F(float value) { return value.ToString("0.###", CultureInfo.InvariantCulture); }
    }
}
