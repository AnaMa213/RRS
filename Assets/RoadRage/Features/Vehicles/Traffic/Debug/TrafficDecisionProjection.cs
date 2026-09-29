using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;

namespace RoadRage.Features.Vehicles.Traffic.Diagnostics
{
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
        public GateAEvidenceStatus EvidenceStatus { get; }
        public ReferenceCoverage ReferenceCoverage { get; }
        public VehicleCoverage VehicleCoverage { get; }
        public string SteeringCeilingText { get; }
        public string Code { get; }

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
            EvidenceStatus = motion == null ? GateAEvidenceStatus.GateAEvidenceMissing : motion.Evidence.Status;
            ReferenceCoverage = motion == null ? ReferenceCoverage.GateAEvidenceMissing : motion.ReferenceCoverage;
            VehicleCoverage = VehicleCoverage.NotEstablished;
            float minimum = float.PositiveInfinity;
            if (motion != null)
                foreach (var interval in motion.Path.Intervals)
                    foreach (var point in interval.Points)
                        minimum = Math.Min(minimum, point.SteeringCeilingMetersPerSecond);
            SteeringCeilingText = float.IsPositiveInfinity(minimum) ? "aucun"
                : minimum.ToString("R", CultureInfo.InvariantCulture) + " m/s";
            Code = code;
        }

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
            text.Append("Gate A ").Append(EvidenceStatus).Append(" / reference ").Append(ReferenceCoverage)
                .Append(" / vehicle ").Append(VehicleCoverage).Append('\n');
            text.Append("Code ").Append(Code);
            return text.ToString();
        }
    }
}
