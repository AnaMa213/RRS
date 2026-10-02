using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Lifecycle
{
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

        public V2StepRecord(ulong step, int trackIndex, BodyState state, float routeDistance, int piece, RoadId elementId,
            float displacement, float speed, bool unbounded, float ceiling, float lateral, float heading,
            RoadLocationFlags locationFlags, float targetSpeed,
            VehicleDriveIntent intent, bool fallback, V2FallbackReason reason, V2FallbackTerminal terminal,
            V2ComposerDiagnostic diagnostics, float minimumDriveTorque, SpeedConstraint binding,
            float nominalOffsetDegrees, float nominalSteerDegrees, float nominalSteerRate, bool nominalFeasible,
            int groundedWheels, SpeedConstraint limiting = SpeedConstraint.None, float commandedWheelAngle = float.NaN,
            float appliedWheelAngle = float.NaN, bool outsideWidthEnvelope = false)
        {
            OutsideWidthEnvelope = outsideWidthEnvelope;
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

    /// <summary>Temps par etape et par vehicule (observation seulement, jamais un seuil).</summary>
    public sealed class V2StageTimings
    {
        public int Steps;
        public double FrameMilliseconds, SpineMilliseconds, SpeedPlanMilliseconds, ComposeMilliseconds;
        public double MaxFrameMilliseconds, MaxSpineMilliseconds, MaxSpeedPlanMilliseconds, MaxComposeMilliseconds;

        internal void Add(double frame, double spine, double plan, double compose)
        {
            Steps++;
            FrameMilliseconds += frame; SpineMilliseconds += spine; SpeedPlanMilliseconds += plan; ComposeMilliseconds += compose;
            MaxFrameMilliseconds = Math.Max(MaxFrameMilliseconds, frame); MaxSpineMilliseconds = Math.Max(MaxSpineMilliseconds, spine);
            MaxSpeedPlanMilliseconds = Math.Max(MaxSpeedPlanMilliseconds, plan);
            MaxComposeMilliseconds = Math.Max(MaxComposeMilliseconds, compose);
        }

        public override string ToString()
        {
            int n = Math.Max(1, Steps);
            return string.Format(CultureInfo.InvariantCulture,
                "steps {0} / frame+localisation mean {1:0.###} ms max {2:0.###} / route+horizon+mouvement mean {3:0.###} ms max {4:0.###}"
                + " / plan de vitesse mean {5:0.###} ms max {6:0.###} / commande+composition mean {7:0.###} ms max {8:0.###}",
                Steps, FrameMilliseconds / n, MaxFrameMilliseconds, SpineMilliseconds / n, MaxSpineMilliseconds,
                SpeedPlanMilliseconds / n, MaxSpeedPlanMilliseconds, ComposeMilliseconds / n, MaxComposeMilliseconds);
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
    /// Driver V2 hote seul (Story 5.31). A chaque pas physique : TrafficFrame (FrameId = compteur de pas,
    /// acteurs V2 seulement) -> PlanningSpine -> SpeedPlan -> MotionCommand -> composeur -> un seul
    /// ApplyDriveIntent. Il n'ecrit jamais position, rotation ni vitesse : les clients recoivent le
    /// mouvement par le NetworkTransform seul. Retrait signale uniquement sur le corridor du portail de
    /// sortie, a s superieur ou egal au portail ; tout echec en route donne le repli, vehicule present.
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
        private bool recordTrace;
        private bool warnedMissingDriverProfile;
        private bool warnedMissingPhysicsProfile;
        private GaugeBox gauge;
        private TrackingTolerance declared;
        private readonly TrackingToleranceResponse toleranceResponse = new TrackingToleranceResponse();

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

        private void FixedUpdate()
        {
            if (!IsServer || !IsSpawned || insertion == null || body == null || physicsBody == null) return;
            if (driverProfile == null)
            {
                if (!warnedMissingDriverProfile)
                {
                    warnedMissingDriverProfile = true;
                    UnityEngine.Debug.LogWarning("[Traffic V2] " + name + " : aucun DriverProfileDef, vehicule inerte.", this);
                }
                return;
            }
            if (!physicsBody.HasProfile)
            {
                if (!warnedMissingPhysicsProfile)
                {
                    warnedMissingPhysicsProfile = true;
                    UnityEngine.Debug.LogWarning("[Traffic V2] " + name + " : aucun profil physique applique, vehicule inerte.", this);
                }
                return;
            }
            Step();
        }

        private void Step()
        {
            var driver = driverProfile.Profile;
            var model = admission.Model;
            float dt = Time.fixedDeltaTime;
            if (composer == null)
            {
                composer = new VehicleDriveIntentComposer(physicsBody.Profile, driver.SafeBrakingLimit, dt);
                FixedDeltaTimeSeconds = dt;
            }
            ulong frameId = ++stepCounter;

            var state = new BodyState(body.position, body.rotation, body.worldCenterOfMass, body.linearVelocity, body.angularVelocity);
            VehicleSuspensionModel.TelemetrySample telemetry;
            float speed = physicsBody.TrySampleTelemetry(out telemetry) ? telemetry.LongitudinalSpeed : 0f;
            if (float.IsNaN(speed) || float.IsInfinity(speed)) speed = 0f;
            lastSpeed = speed;
            var pose = new VehicleFootprintPose { Position = state.Position, Forward = state.Rotation * Vector3.forward,
                Up = state.Rotation * Vector3.up };
            // Pose nominale courante (contrat §8) : l'ecart e du vehicule sur sa reference fixe le plafond de braquage
            // de l'horizon et l'orientation attendue des candidats de localisation ; un replan le reprend (jamais remis a zero).
            var current = tracks[tracks.Count - 1];
            int currentPiece;
            float currentDistance = current.Project(state.Position, pieceHint, out currentPiece);
            float offset = current.OffsetRadians(currentPiece, currentDistance);
            // Hors mesure, observer avant toute progression/replanification ou detection de sortie.
            float? observedDisplacement = MeasurementLabel == null
                ? ObserveTrackingTolerance(frameId, state, current, currentDistance) : (float?)null;

            stopwatch.Restart();
            var frame = new TrafficFrame(frameId, model, new[] {
                new TrafficActorInput(insertion.TrafficId, pose, speed, previousElement, TrafficV2Lifecycle.ExpectedElements(route),
                    current.KinematicAnchors(currentPiece)) });
            TrafficActor actor;
            frame.TryGetActor(insertion.TrafficId, out actor);
            if (actor.Location.Localized) previousElement = actor.Location.ElementId;
            if (!viaMovement.IsEmpty && actor.Location.Localized)
            {
                // Franchi seulement quand la localisation passe du mouvement vise a l'element suivant.
                if (actor.Location.ElementId == viaMovement) onViaMovement = true;
                else if (onViaMovement) { viaMovement = RoadId.None; onViaMovement = false; }
            }
            if (!toleranceResponse.Latched && !HasReachedExitPortal && TrafficV2Lifecycle.HasReachedExit(actor.Location, exitPortal))
                reachedExitPortal = true;
            double frameMs = stopwatch.Elapsed.TotalMilliseconds;

            stopwatch.Restart();
            PlanningDecision decision = null;
            if (!toleranceResponse.Latched)
            {
                try
                {
                    decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, route, insertion.ExitPortalId,
                        insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared,
                        null, null, admission.Evidence, viaMovement, current.HasKinematicPose ? offset : (float?)null));
                }
                catch (ArgumentException) { decision = null; }
                catch (InvalidOperationException) { decision = null; }
            }
            if (decision != null && decision.Route.Plan != null && decision.Route.Plan != route)
            {
                if (decision.Route.Outcome == RouteOutcome.Replanned)
                {
                    ReplanCount++;
                    Replans.Add(frameId.ToString(CultureInfo.InvariantCulture) + ":" + decision.Route.Reason + ":"
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
            double spineMs = stopwatch.Elapsed.TotalMilliseconds;

            stopwatch.Restart();
            SpeedPlan plan = null;
            MotionCommand? command = null;
            var refusal = V2FallbackReason.NoCommand;
            if (decision == null || decision.Motion == null) refusal = V2FallbackReason.NoRoute;
            else
            {
                plan = SpeedPlan.Build(decision.Motion, model, driver, Math.Max(0f, speed),
                    physicsBody.Profile.LateralFrictionCoefficient * Physics.gravity.magnitude);
                if (!plan.Accepted)
                    refusal = HasReachedExitPortal ? V2FallbackReason.ExitPortalReached
                        : plan.Issue == SpeedPlanIssue.PlanInfeasible
                        ? (plan.PlanIssue == MotionIssue.HorizonNonConforming ? V2FallbackReason.HorizonNonConforming : V2FallbackReason.PlanInfeasible)
                        : V2FallbackReason.ProfileRefused;
            }
            double planMs = stopwatch.Elapsed.TotalMilliseconds;

            stopwatch.Restart();
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
                    command = MotionCommand.Track(frameId, TrafficV2Settings.PlanValiditySteps, plan, driver,
                        model.DrivabilityProfile, curve, first.StartSMeters, pose.Position, pose.Forward, speed, dt,
                        nominalHeading);
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
            double composeMs = stopwatch.Elapsed.TotalMilliseconds;
            Timings.Add(frameMs, spineMs, planMs, composeMs);
            LastComposed = composed;

            Monitor(frameId, state, speed, actor.Location.Flags, actor.Location.OutsideWidthEnvelope, composed, command, plan, observedDisplacement);
            var projection = decision != null ? decision.Projection : LastProjection;
            if (projection != null)
                LastProjection = projection.WithDrive(DriveOutcome(frameId, composed, plan, driver));
            lastIntent = composed.Intent;
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
            ComposedDrive composed, MotionCommand? command, SpeedPlan plan, float? observedDisplacement)
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
                command.HasValue ? command.Value.TargetWheelAngleDegrees : float.NaN, physicsBody.CurrentSteerAngleDegrees, outsideWidth));
        }

        private TrafficDriveOutcome DriveOutcome(ulong step, ComposedDrive composed, SpeedPlan plan, DriverProfile driver)
        {
            var applied = new List<string>();
            var deferred = new List<string>();
            string binding = "None";
            if (plan != null)
            {
                binding = plan.Binding.ToString();
                applied.Add("DesiredSpeed " + F(driver.DesiredSpeed));
                float ceiling = float.PositiveInfinity;
                foreach (var point in plan.Points) if (!point.CeilingUnbounded) ceiling = Math.Min(ceiling, point.CeilingMetersPerSecond);
                applied.Add("SteeringCeiling " + (float.IsPositiveInfinity(ceiling) ? "aucun" : F(ceiling)));
                applied.Add("CurveLimit a_lat " + F(plan.LateralAccelerationMetersPerSecondSquared));
                applied.Add("LongitudinalBounds " + F(driver.MaxAcceleration) + "/" + F(plan.PlanningDecelerationMetersPerSecondSquared)
                    + " verifie " + F(driver.MaxAcceleration) + "/" + F(driver.SafeBrakingLimit));
                foreach (var limit in plan.DeferredLimits)
                    deferred.Add(limit.Kind + " " + limit.ElementId + " " + limit.State
                        + (limit.State == DeferredLimitState.DeferredAuthored ? "(" + F(limit.AuthoredMetersPerSecond) + ")" : ""));
            }
            return new TrafficDriveOutcome(step, step, composed.SourceFrameId, composed.Intent.Throttle, composed.Intent.Steer,
                composed.Intent.BrakeReverse, composed.Intent.Handbrake, composed.Fallback, composed.Reason.ToString(), binding,
                applied, deferred, VehicleCoverageVerdict, MeasurementLabel);
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
