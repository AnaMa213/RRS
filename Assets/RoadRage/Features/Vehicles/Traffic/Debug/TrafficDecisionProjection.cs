using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;

namespace RoadRage.Features.Vehicles.Traffic.Diagnostics
{
    /// <summary>
    /// Partie conduite d'une decision (5.31) : contraintes appliquees et reportees, liante, epoques,
    /// intent final en quatre flottants, repli et raison, couverture vehicule et etiquette de mesure.
    /// Hote seul : elle ne cree aucun chemin de synchronisation client.
    /// </summary>
    public sealed class TrafficDriveOutcome
    {
        public ulong DecisionEpoch { get; }
        public ulong PhysicsEpoch { get; }
        /// <summary>Frame source de la commande appliquee ; 0 pendant un repli.</summary>
        public ulong SourceFrameId { get; }
        public float Throttle { get; }
        public float Steer { get; }
        public float BrakeReverse { get; }
        public float Handbrake { get; }
        public bool Fallback { get; }
        public string FallbackReason { get; }
        public string Binding { get; }
        public IReadOnlyList<string> AppliedConstraints { get; }
        public IReadOnlyList<string> DeferredConstraints { get; }
        public VehicleCoverage VehicleCoverage { get; }
        /// <summary>Null hors run de mesure.</summary>
        public string MeasurementLabel { get; }

        public TrafficDriveOutcome(ulong decisionEpoch, ulong physicsEpoch, ulong sourceFrameId, float throttle,
            float steer, float brakeReverse, float handbrake, bool fallback, string fallbackReason, string binding,
            IReadOnlyList<string> appliedConstraints, IReadOnlyList<string> deferredConstraints,
            VehicleCoverage vehicleCoverage, string measurementLabel)
        {
            DecisionEpoch = decisionEpoch; PhysicsEpoch = physicsEpoch; SourceFrameId = sourceFrameId;
            Throttle = throttle; Steer = steer; BrakeReverse = brakeReverse; Handbrake = handbrake;
            Fallback = fallback; FallbackReason = fallbackReason ?? "None"; Binding = binding ?? "None";
            AppliedConstraints = Array.AsReadOnly(Copy(appliedConstraints));
            DeferredConstraints = Array.AsReadOnly(Copy(deferredConstraints));
            VehicleCoverage = vehicleCoverage; MeasurementLabel = measurementLabel;
        }

        private static string[] Copy(IReadOnlyList<string> source)
        {
            if (source == null) return new string[0];
            var result = new string[source.Count];
            for (int i = 0; i < result.Length; i++) result[i] = source[i];
            return result;
        }
    }

    public sealed class TrafficDecisionProjection
    {
        public ulong FrameId { get; }
        public RoadModelVersion RoadModelVersion { get; }
        public RoadId TrafficId { get; }
        public RoadElementKind ElementKind { get; }
        public RoadId ElementId { get; }
        public float SMeters { get; }
        public RouteOutcome RouteOutcome { get; }
        public RouteReason RouteReason { get; }
        public RoadId ExitPortalId { get; }
        public IReadOnlyList<RouteOccurrence> RouteOccurrences { get; }
        public RoadId NextMovementId { get; }
        public string PathPlanId { get; }
        public string MotionPlanId { get; }
        /// <summary>Null quand aucune liaison n'a ete evaluee (pas de plan de route).</summary>
        public GateAEvidenceStatus? EvidenceStatus { get; }
        /// <summary>Null quand aucune couverture n'a ete evaluee (pas de plan de route).</summary>
        public ReferenceCoverage? ReferenceCoverage { get; }
        public VehicleCoverage VehicleCoverage { get; }
        public string SteeringCeilingText { get; }
        public string Code { get; }
        /// <summary>Partie conduite (5.31) ; nulle pour une decision sans conduite (5.30).</summary>
        public TrafficDriveOutcome Drive { get; private set; }

        internal TrafficDecisionProjection(ulong frameId, RoadModelVersion version, RoadId trafficId,
            RoadLocation location, RouteResult route, MotionPlan motion, string code)
        {
            FrameId = frameId; RoadModelVersion = version; TrafficId = trafficId;
            ElementKind = location.ElementKind; ElementId = location.ElementId; SMeters = location.SMeters;
            RouteOutcome = route.Outcome; RouteReason = route.Reason;
            ExitPortalId = route.Plan == null ? RoadId.None : route.Plan.ExitPortalId;
            var occurrences = route.Plan == null ? new RouteOccurrence[0] : Copy(route.Plan.Occurrences);
            RouteOccurrences = Array.AsReadOnly(occurrences);
            NextMovementId = RoadId.None;
            if (route.Plan != null)
                for (int i = route.Plan.ProgressOccurrenceIndex; i < occurrences.Length; i++)
                    if (occurrences[i].Kind == RoadElementKind.JunctionMovement)
                    { NextMovementId = occurrences[i].Id; break; }
            string key = frameId.ToString(CultureInfo.InvariantCulture) + ":" + trafficId;
            PathPlanId = motion == null ? "aucun" : key + ":path";
            MotionPlanId = motion == null ? "aucun" : key + ":motion";
            if (motion != null)
            {
                EvidenceStatus = motion.Evidence.Status;
                ReferenceCoverage = motion.ReferenceCoverage;
            }
            VehicleCoverage = VehicleCoverage.NotEstablished;
            float minimum = float.PositiveInfinity;
            if (motion != null)
                foreach (var interval in motion.Path.Intervals)
                    foreach (var point in interval.Points)
                        minimum = Math.Min(minimum, point.SteeringCeilingMetersPerSecond);
            SteeringCeilingText = motion == null ? NotEvaluated
                : float.IsPositiveInfinity(minimum) ? "aucun"
                : minimum.ToString("R", CultureInfo.InvariantCulture) + " m/s";
            Code = code;
        }

        private const string NotEvaluated = "non evalue";

        /// <summary>Copie immuable portant la partie conduite ; la decision d'origine est inchangee.</summary>
        public TrafficDecisionProjection WithDrive(TrafficDriveOutcome drive)
        {
            var copy = (TrafficDecisionProjection)MemberwiseClone();
            copy.Drive = drive;
            return copy;
        }

        private static string F(float value) { return value.ToString("R", CultureInfo.InvariantCulture); }

        private static RouteOccurrence[] Copy(IReadOnlyList<RouteOccurrence> source)
        {
            var result = new RouteOccurrence[source.Count];
            for (int i = 0; i < source.Count; i++) result[i] = source[i];
            return result;
        }

        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Frame ").Append(FrameId.ToString(CultureInfo.InvariantCulture))
                .Append(" / RoadModelVersion ").Append(RoadModelVersion).Append('\n');
            text.Append("Vehicle ").Append(TrafficId).Append(" / Element ").Append(ElementKind)
                .Append(' ').Append(ElementId).Append(" / s ")
                .Append(SMeters.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
            text.Append("RoutePlan ").Append(RouteOutcome).Append(" / ").Append(RouteReason)
                .Append(" / exit ").Append(ExitPortalId).Append(" / IDs ");
            for (int i = 0; i < RouteOccurrences.Count; i++)
            { if (i > 0) text.Append(','); text.Append(RouteOccurrences[i].Id); }
            text.Append('\n').Append("Next movement ").Append(NextMovementId).Append('\n');
            text.Append("Path/Motion plan ").Append(PathPlanId).Append(" / ").Append(MotionPlanId).Append('\n');
            text.Append("Steering ceiling ").Append(SteeringCeilingText).Append('\n');
            text.Append("Gate A ").Append(EvidenceStatus.HasValue ? EvidenceStatus.Value.ToString() : NotEvaluated)
                .Append(" / reference ")
                .Append(ReferenceCoverage.HasValue ? ReferenceCoverage.Value.ToString() : NotEvaluated)
                .Append(" / vehicle ").Append(VehicleCoverage).Append('\n');
            text.Append("Code ").Append(Code);
            if (Drive != null)
            {
                text.Append('\n').Append("Speed constraints applied ").Append(string.Join(", ", Drive.AppliedConstraints))
                    .Append(" / binding ").Append(Drive.Binding).Append('\n');
                text.Append("Speed constraints deferred ").Append(string.Join(", ", Drive.DeferredConstraints)).Append('\n');
                text.Append("Epochs decision ").Append(Drive.DecisionEpoch.ToString(CultureInfo.InvariantCulture))
                    .Append(" / physics ").Append(Drive.PhysicsEpoch.ToString(CultureInfo.InvariantCulture))
                    .Append(" / source frame ").Append(Drive.SourceFrameId.ToString(CultureInfo.InvariantCulture)).Append('\n');
                text.Append("Final intent ").Append(F(Drive.Throttle)).Append(' ').Append(F(Drive.Steer)).Append(' ')
                    .Append(F(Drive.BrakeReverse)).Append(' ').Append(F(Drive.Handbrake))
                    .Append(" / fallback ").Append(Drive.Fallback ? "yes " + Drive.FallbackReason : "no").Append('\n');
                text.Append("Vehicle coverage ").Append(Drive.VehicleCoverage)
                    .Append(" / measurement ").Append(Drive.MeasurementLabel ?? "hors mesure");
            }
            return text.ToString();
        }
    }
}
