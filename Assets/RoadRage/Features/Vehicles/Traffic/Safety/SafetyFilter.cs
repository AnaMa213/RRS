using System;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;

namespace RoadRage.Features.Vehicles.Traffic.Safety
{
    /// <summary>Verdict du filtre (Story 5.37). Valeurs stables.</summary>
    public enum SafetyVerdict { Pass = 0, Clamp = 1, EmergencyStop = 2, Reject = 3 }

    /// <summary>
    /// Raison objective unique d'un verdict (S2). Valeurs stables, ajouts en fin d'enum. L'ordre des valeurs non nulles est
    /// aussi l'ordre de priorite : la premiere cause presente l'emporte.
    /// </summary>
    public enum SafetyReason
    {
        None = 0,
        InvalidActorState = 1,
        NonFiniteOutput = 2,
        StalePlan = 3,
        LocalPlanInvalidated = 4,
        PhysicallyInvalidPath = 5,
        ImminentUnintendedCollision = 6,
        PhysicallyInvalidIntent = 7
    }

    /// <summary>
    /// Contact intentionnel autorise, porte par le plan (S4) ; produit par la 5.44. Il ne fait que retirer sa cible de
    /// l'evaluation de collision imminente, tant que le pas courant ne depasse pas l'expiration et que la vitesse
    /// d'approche reste dans le scope. Scope non fini ou negatif : autorisation sans effet.
    /// </summary>
    public readonly struct AuthorizedContact
    {
        public readonly RoadId TargetId;
        public readonly float MaxClosingSpeedMetersPerSecond;
        public readonly ulong ExpiresAtStep;

        public AuthorizedContact(RoadId targetId, float maxClosingSpeedMetersPerSecond, ulong expiresAtStep)
        {
            TargetId = targetId; MaxClosingSpeedMetersPerSecond = maxClosingSpeedMetersPerSecond; ExpiresAtStep = expiresAtStep;
        }

        public bool Valid
        {
            get
            {
                return !TargetId.IsEmpty && !float.IsNaN(MaxClosingSpeedMetersPerSecond)
                    && !float.IsInfinity(MaxClosingSpeedMetersPerSecond) && MaxClosingSpeedMetersPerSecond >= 0f;
            }
        }

        internal bool Exempts(RoadId hazardId, PerceivedObstacleKind kind, ulong step, float closingSpeed)
        {
            return Valid && hazardId == TargetId && step <= ExpiresAtStep && closingSpeed <= MaxClosingSpeedMetersPerSecond
                && kind != PerceivedObstacleKind.WalkingPlayer && kind != PerceivedObstacleKind.Pedestrian;
        }
    }

    /// <summary>Echantillon de proximite plus recent que la frame source, date de son pas physique (S5).</summary>
    public readonly struct NearFieldSample
    {
        public readonly ulong PhysicsStep;
        public readonly LongitudinalPerception Perception;

        public NearFieldSample(ulong physicsStep, LongitudinalPerception perception)
        {
            PhysicsStep = physicsStep; Perception = perception;
        }
    }

    /// <summary>Bornes physiques du vehicule lues par le filtre ; toutes finies et strictement positives.</summary>
    public readonly struct SafetyLimits
    {
        /// <summary>b_max : capacite de frein physique (m/s2).</summary>
        public readonly float MaxBrakingDecelerationMetersPerSecondSquared;
        /// <summary>a_max : capacite moteur physique (m/s2).</summary>
        public readonly float MaxDriveAccelerationMetersPerSecondSquared;
        public readonly float MaxWheelAngleDegrees;
        /// <summary>1 / rayon d'admission du profil de drivabilite.</summary>
        public readonly float MaxCurvaturePerMeter;
        /// <summary>tau : fenetre de validite de la commande en secondes.</summary>
        public readonly float LatencySeconds;

        public SafetyLimits(float maxBraking, float maxDrive, float maxWheelAngleDegrees, float maxCurvature, float latencySeconds)
        {
            if (!Positive(maxBraking) || !Positive(maxDrive) || !Positive(maxWheelAngleDegrees) || !Positive(maxCurvature)
                || !Positive(latencySeconds))
                throw new ArgumentException("SafetyLimits : bornes finies et strictement positives requises.");
            MaxBrakingDecelerationMetersPerSecondSquared = maxBraking; MaxDriveAccelerationMetersPerSecondSquared = maxDrive;
            MaxWheelAngleDegrees = maxWheelAngleDegrees; MaxCurvaturePerMeter = maxCurvature; LatencySeconds = latencySeconds;
        }

        /// <summary>Bornes d'un vehicule : capacites du composeur, braquage et rayon d'admission declares.</summary>
        public static SafetyLimits For(VehicleProfile vehicle, DrivabilityProfile drivability, float fixedDeltaTime, int validitySteps)
        {
            float drive, brake;
            VehicleDriveIntentComposer.ResolveCapacities(vehicle, out drive, out brake);
            return new SafetyLimits(brake, drive, drivability.LowSpeedLockDegrees,
                1f / RoadModelCompiler.AdmissionRadiusMeters(drivability), validitySteps * fixedDeltaTime);
        }

        private static bool Positive(float value) { return value > 0f && !float.IsInfinity(value); }
    }

    /// <summary>Resultat d'une evaluation : verdict, raison unique, commande transmise (nulle sur Reject) et epoques.</summary>
    public sealed class SafetyResult
    {
        public SafetyVerdict Verdict { get; }
        public SafetyReason Reason { get; }
        public MotionCommand? Command { get; }
        /// <summary>Frame source du plan evalue.</summary>
        public ulong SourceFrameId { get; }
        public ulong PhysicsStep { get; }
        /// <summary>Pas de l'echantillon de proximite lu pour la collision : celui de l'echantillon recent, sinon la frame.</summary>
        public ulong NearFieldStep { get; }
        /// <summary>Danger en cause d'un EmergencyStop ; vide sinon.</summary>
        public RoadId HazardId { get; }
        /// <summary>Raison de repli a donner au composeur : SafetyRejected sur Reject, None sinon.</summary>
        public V2FallbackReason Refusal { get { return Verdict == SafetyVerdict.Reject ? V2FallbackReason.SafetyRejected : V2FallbackReason.None; } }

        internal SafetyResult(SafetyVerdict verdict, SafetyReason reason, MotionCommand? command, ulong sourceFrameId,
            ulong physicsStep, ulong nearFieldStep, RoadId hazardId)
        {
            Verdict = verdict; Reason = reason; Command = command; SourceFrameId = sourceFrameId; PhysicsStep = physicsStep;
            NearFieldStep = nearFieldStep; HazardId = hazardId;
        }

        /// <summary>Texte deterministe et invariant de culture.</summary>
        public string ToText()
        {
            string text = Verdict + "/" + Reason + " source " + SourceFrameId.ToString(CultureInfo.InvariantCulture)
                + " pas " + PhysicsStep.ToString(CultureInfo.InvariantCulture)
                + " proximite " + NearFieldStep.ToString(CultureInfo.InvariantCulture);
            return HazardId.IsEmpty ? text : text + " danger " + HazardId;
        }
    }

    /// <summary>
    /// Frontiere de surete V2 (Story 5.37, AD-39) : pure et sans etat, entre la commande de suivi et le composeur. Elle
    /// transmet, borne, arrete en urgence ou rejette une commande pour une seule raison objective, et ne fait rien d'autre :
    /// elle ne produit jamais une nouvelle trajectoire et ne lit que la commande, l'acteur, la frame, les faits de
    /// proximite et le contact autorise porte par le plan.
    /// </summary>
    public static class SafetyFilter
    {
        /// <summary>Tolerance relative sur la courbure de reference (S2).</summary>
        public const float CurvatureTolerance = 1e-3f;

        /// <param name="physicsStep">Pas physique d'application de la commande.</param>
        /// <param name="frame">Frame evaluee ; nulle : acteur absent.</param>
        /// <param name="path">Chemin local du plan ; nul : aucun element a verifier.</param>
        /// <param name="nearField">Faits de proximite de la frame source ; nuls : aucun danger connu.</param>
        /// <param name="authorization">Contact autorise porte par le plan.</param>
        /// <param name="newerSample">
        /// Echantillon plus recent, lu pour la collision imminente seulement. Ignore s'il n'a pas de perception ou s'il n'est
        /// pas posterieur a la frame et au plus au pas courant.
        /// </param>
        public static SafetyResult Evaluate(MotionCommand command, ulong physicsStep, TrafficFrame frame, RoadId trafficId,
            float speedMetersPerSecond, IPathGeometry path, LongitudinalPerception nearField, SafetyLimits limits,
            AuthorizedContact? authorization = null, NearFieldSample? newerSample = null)
        {
            ulong source = command.SourceFrameId;
            bool newer = frame != null && newerSample.HasValue && newerSample.Value.Perception != null
                && newerSample.Value.PhysicsStep > frame.FrameId && newerSample.Value.PhysicsStep <= physicsStep;
            ulong nearStep = newer ? newerSample.Value.PhysicsStep : frame != null ? frame.FrameId : 0UL;

            TrafficActor actor;
            if (frame == null || !frame.TryGetActor(trafficId, out actor) || !Finite(speedMetersPerSecond)
                || !Finite(actor.Pose.Position.x) || !Finite(actor.Pose.Position.y) || !Finite(actor.Pose.Position.z)
                || !Finite(actor.Pose.Forward.x) || !Finite(actor.Pose.Forward.y) || !Finite(actor.Pose.Forward.z))
                return Rejected(SafetyReason.InvalidActorState, source, physicsStep, nearStep);
            if (!command.IsFinite || !Finite(command.ReferenceCurvaturePerMeter))
                return Rejected(SafetyReason.NonFiniteOutput, source, physicsStep, nearStep);
            if (!command.IsValidAt(physicsStep) || source > frame.FrameId)
                return Rejected(SafetyReason.StalePlan, source, physicsStep, nearStep);
            if (path != null)
                for (int i = 0; i < path.Spans.Count; i++)
                    if (frame.IsClosed(path.Spans[i].Id)) return Rejected(SafetyReason.LocalPlanInvalidated, source, physicsStep, nearStep);
            if (Math.Abs(command.ReferenceCurvaturePerMeter) > limits.MaxCurvaturePerMeter * (1f + CurvatureTolerance))
                return Rejected(SafetyReason.PhysicallyInvalidPath, source, physicsStep, nearStep);

            var hazards = newer ? newerSample.Value.Perception : nearField;
            RoadId hazard = ImminentHazard(hazards, speedMetersPerSecond, limits, physicsStep, authorization);
            if (!hazard.IsEmpty)
                return new SafetyResult(SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision,
                    With(command, -limits.MaxBrakingDecelerationMetersPerSecondSquared, command.TargetWheelAngleDegrees),
                    source, physicsStep, nearStep, hazard);

            float acceleration = Math.Max(-limits.MaxBrakingDecelerationMetersPerSecondSquared,
                Math.Min(limits.MaxDriveAccelerationMetersPerSecondSquared, command.TargetAccelerationMetersPerSecondSquared));
            float angle = Math.Max(-limits.MaxWheelAngleDegrees, Math.Min(limits.MaxWheelAngleDegrees, command.TargetWheelAngleDegrees));
            if (acceleration != command.TargetAccelerationMetersPerSecondSquared || angle != command.TargetWheelAngleDegrees)
                return new SafetyResult(SafetyVerdict.Clamp, SafetyReason.PhysicallyInvalidIntent, With(command, acceleration, angle),
                    source, physicsStep, nearStep, RoadId.None);
            return new SafetyResult(SafetyVerdict.Pass, SafetyReason.None, command, source, physicsStep, nearStep, RoadId.None);
        }

        /// <summary>
        /// Premier danger imminent (S3), leader puis obstacles du couloir balaye dans leur ordre : approche c &gt; 0 et
        /// g &lt;= c tau + c^2 / (2 b_max). Une cible couverte par le contact autorise est retiree de cette seule evaluation.
        /// </summary>
        private static RoadId ImminentHazard(LongitudinalPerception perception, float speed, SafetyLimits limits, ulong step,
            AuthorizedContact? authorization)
        {
            if (perception == null) return RoadId.None;
            if (perception.HasLeader)
            {
                var leader = perception.Leader;
                if (Imminent(leader.GapMeters, speed - leader.SpeedMetersPerSecond, limits, leader.Id,
                        PerceivedObstacleKind.TrafficActor, step, authorization))
                    return leader.Id;
            }
            for (int i = 0; i < perception.Obstacles.Count; i++)
            {
                var obstacle = perception.Obstacles[i];
                if (Imminent(obstacle.NearDistanceMeters, speed - obstacle.SpeedAlongPathMetersPerSecond, limits, obstacle.Id,
                        obstacle.Kind, step, authorization))
                    return obstacle.Id;
            }
            return RoadId.None;
        }

        private static bool Imminent(float gap, float closing, SafetyLimits limits, RoadId id, PerceivedObstacleKind kind,
            ulong step, AuthorizedContact? authorization)
        {
            if (!Finite(gap) || !Finite(closing) || !(closing > 0f)) return false;
            if (gap > closing * limits.LatencySeconds + closing * closing / (2f * limits.MaxBrakingDecelerationMetersPerSecondSquared))
                return false;
            return !(authorization.HasValue && authorization.Value.Exempts(id, kind, step, closing));
        }

        private static MotionCommand With(MotionCommand command, float acceleration, float angle)
        {
            return new MotionCommand(command.SourceFrameId, command.ValidFromStep, command.ValidToStep, acceleration, angle,
                command.Binding, command.LateralErrorMeters, command.HeadingErrorDegrees, command.ReferenceCurvaturePerMeter,
                command.ReferenceSMeters);
        }

        private static SafetyResult Rejected(SafetyReason reason, ulong source, ulong step, ulong nearStep)
        {
            return new SafetyResult(SafetyVerdict.Reject, reason, null, source, step, nearStep, RoadId.None);
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
