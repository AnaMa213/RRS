using System;
using System.Collections.Generic;

namespace RoadRage.Features.Vehicles.Traffic.Routing
{
    public enum RouteOutcome { Planned = 0, Replanned = 1, NoRoute = 2, InvalidInput = 3 }
    public enum RouteReason { Requested = 0, StalePlan = 1, InvalidStart = 2, StaleLocalization = 3,
        DestinationUnavailable = 4, DestinationUnreachable = 5 }

    [Flags]
    public enum RouteDiagnostic { None = 0, ZeroWeightFallback = 1 }

    /// <summary>Une visite dirigee ; une meme identite peut apparaitre plusieurs fois apres une boucle.</summary>
    public readonly struct RouteOccurrence
    {
        public readonly RoadElementKind Kind;
        public readonly RoadId Id;
        public readonly float StartSMeters;
        public readonly float EndSMeters;

        public RouteOccurrence(RoadElementKind kind, RoadId id, float startSMeters, float endSMeters)
        {
            Kind = kind;
            Id = id;
            StartSMeters = startSMeters;
            EndSMeters = endSMeters;
        }
    }

    public sealed class RoutePlan
    {
        public RoadId ModelId { get; }
        public RoadModelVersion ModelVersion { get; }
        public RoadId TrafficId { get; }
        public RoadId ExitPortalId { get; }
        public RouteReason Reason { get; }
        public IReadOnlyList<RouteOccurrence> Occurrences { get; }
        public int ProgressOccurrenceIndex { get; }
        public float ProgressSMeters { get; }
        public double DistanceMeters { get; }
        public double PreferenceCost { get; }
        public double TotalCost { get { return DistanceMeters + PreferenceCost; } }
        public RouteDiagnostic Diagnostics { get; }

        internal RoutePlan(RoadId modelId, RoadModelVersion version, RoadId trafficId, RoadId exitPortalId, RouteReason reason,
            List<RouteOccurrence> occurrences, double distanceMeters, double preferenceCost, RouteDiagnostic diagnostics)
        {
            ModelId = modelId;
            ModelVersion = version;
            TrafficId = trafficId;
            ExitPortalId = exitPortalId;
            Reason = reason;
            Occurrences = occurrences.AsReadOnly();
            ProgressOccurrenceIndex = 0;
            ProgressSMeters = occurrences[0].StartSMeters;
            DistanceMeters = distanceMeters;
            PreferenceCost = preferenceCost;
            Diagnostics = diagnostics;
        }

        private RoutePlan(RoutePlan source, int occurrenceIndex, float sMeters)
        {
            ModelId = source.ModelId;
            ModelVersion = source.ModelVersion;
            TrafficId = source.TrafficId;
            ExitPortalId = source.ExitPortalId;
            Reason = source.Reason;
            Occurrences = source.Occurrences;
            ProgressOccurrenceIndex = occurrenceIndex;
            ProgressSMeters = sMeters;
            DistanceMeters = source.DistanceMeters;
            PreferenceCost = source.PreferenceCost;
            Diagnostics = source.Diagnostics;
        }

        internal RoutePlan Advance(int occurrenceIndex, float sMeters)
        {
            return occurrenceIndex == ProgressOccurrenceIndex && sMeters == ProgressSMeters
                ? this : new RoutePlan(this, occurrenceIndex, sMeters);
        }
    }

    public readonly struct RouteResult
    {
        public readonly RouteOutcome Outcome;
        public readonly RouteReason Reason;
        public readonly RouteDiagnostic Diagnostics;
        public readonly RoutePlan Plan;

        internal RouteResult(RouteOutcome outcome, RouteReason reason, RoutePlan plan)
        {
            Outcome = outcome;
            Reason = reason;
            Plan = plan;
            Diagnostics = plan == null ? RouteDiagnostic.None : plan.Diagnostics;
        }
    }
}
