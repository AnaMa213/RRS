using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    public enum HorizonEnd { ExitPortal, LookAheadLimit }
    public enum PathIssue { None, MissingElement, SeamGap, SeamTangent, SeamCurvature, CurvatureSlope }

    public readonly struct PathPoint
    {
        public readonly float DistanceMeters;
        public readonly float ElementSMeters;
        public readonly RoadCurvePoint Reference;
        public readonly float SteeringCeilingMetersPerSecond;
        /// <summary>Ecart nominal e de la route (radians, contrat §8) ; NaN : plafond de regime etabli du compilateur.</summary>
        public readonly float NominalOffsetRadians;
        public bool Unbounded { get { return float.IsPositiveInfinity(SteeringCeilingMetersPerSecond); } }

        internal PathPoint(float distance, RoadCurvePoint reference, DrivabilityProfile profile)
            : this(distance, reference, RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profile, reference.CurvaturePerMeter),
                float.NaN) { }

        internal PathPoint(float distance, RoadCurvePoint reference, float ceiling, float nominalOffsetRadians)
        {
            DistanceMeters = distance;
            ElementSMeters = reference.SMeters;
            Reference = reference;
            SteeringCeilingMetersPerSecond = ceiling;
            NominalOffsetRadians = nominalOffsetRadians;
        }
    }

    public readonly struct PathInterval
    {
        public readonly RoadElementKind Kind;
        public readonly RoadId Id;
        public readonly float StartSMeters;
        public readonly float EndSMeters;
        public readonly float StartDistanceMeters;
        public readonly float EndDistanceMeters;
        public readonly float MaximumAbsoluteCurvatureSlopePerSquareMeter;
        public readonly IReadOnlyList<PathPoint> Points;

        internal PathInterval(RoadElementKind kind, RoadId id, float s0, float s1, float start,
            float slope, List<PathPoint> points)
        {
            Kind = kind; Id = id; StartSMeters = s0; EndSMeters = s1;
            StartDistanceMeters = start; EndDistanceMeters = start + s1 - s0;
            MaximumAbsoluteCurvatureSlopePerSquareMeter = slope;
            Points = points.AsReadOnly();
        }
    }

    public readonly struct PathSeam
    {
        public readonly float DistanceMeters;
        public readonly float GapMeters;
        public readonly float TangentJumpDegrees;
        public readonly float CurvatureJumpPerMeter;
        public readonly float LeftCeilingMetersPerSecond;
        public readonly float RightCeilingMetersPerSecond;
        public readonly bool RoundaboutDiscontinuity;
        public float MinimumCeilingMetersPerSecond { get { return Math.Min(LeftCeilingMetersPerSecond, RightCeilingMetersPerSecond); } }

        internal PathSeam(PathPoint left, PathPoint right, bool roundabout)
        {
            DistanceMeters = left.DistanceMeters;
            GapMeters = Vector3.Distance(left.Reference.Position, right.Reference.Position);
            TangentJumpDegrees = Vector3.Angle(left.Reference.Tangent, right.Reference.Tangent);
            CurvatureJumpPerMeter = right.Reference.CurvaturePerMeter - left.Reference.CurvaturePerMeter;
            LeftCeilingMetersPerSecond = left.SteeringCeilingMetersPerSecond;
            RightCeilingMetersPerSecond = right.SteeringCeilingMetersPerSecond;
            RoundaboutDiscontinuity = roundabout;
        }
    }

    /// <summary>Geometry read from compiled route occurrences; its lateral offset is identically zero.</summary>
    public sealed class PathHorizon
    {
        // Les 24 raccords tangents de l'anneau couverts par la signature Gate A.
        private static readonly HashSet<string> SignedRingSeams = new HashSet<string>(StringComparer.Ordinal)
        {
            "401b55e11b401435eb1bdd8dde7caa94:entry", "4030253e182e3ed1b7d2aeea7a73feb6:entry",
            "419d893b269e14c02e84e3509d9bf193:entry", "42480748339bbb6fe6fcbc99604c1aa8:exit",
            "4357c472225591a18683e67ea2dd5f92:exit", "43605e569eb08d6ffe62fa7470d59fa0:exit",
            "4439e11d9c47c1d09aad97b8f5dd1cbe:exit", "4469169721b83714f20e63d9fcfff484:exit",
            "452ee31e83feea5ebc05406c271424a9:exit", "453f130c460dc35e052c30714bec6c8e:entry",
            "45607ec286d32b63e09ca677f22031ba:entry", "45d560a7a864362a2f19600802713fac:entry",
            "46077471fe6db9c5bfc3327df0b647af:entry", "464127b42987ee35c9def93cb72dae8c:exit",
            "470e78565e75b89add119d1f7bf3d8b3:exit", "4a5a12c19e62b4f853f928a3d4fb4c96:exit",
            "4a6aa7e11135c1ecb2cb26715ca8cab4:entry", "4a772fed8c8aaeab952d011659612ea7:entry",
            "4ac98ed2e41d83c91f0714135aa67ba7:entry", "4b517add680eba2b77f4e15e9033e180:entry",
            "4cec0461fb9fd74541d5772b2264b88f:exit", "4d6ca77eae0d45e72a542a1478a2aa84:exit",
            "4edce9aa0d470704d0479247d72ff2be:entry", "4f47e1a8140c798681fef66fa633b3b5:exit"
        };

        public IReadOnlyList<PathInterval> Intervals { get; }
        public IReadOnlyList<PathSeam> Seams { get; }
        public HorizonEnd End { get; }
        public float LengthMeters { get; }
        public PathIssue Issue { get; }
        public float IssueDistanceMeters { get; }
        public float MaximumAbsoluteOffsetMeters { get { return 0f; } }

        private PathHorizon(List<PathInterval> intervals, List<PathSeam> seams, HorizonEnd end,
            float length, PathIssue issue, float issueDistance)
        {
            Intervals = intervals.AsReadOnly(); Seams = seams.AsReadOnly();
            End = end; LengthMeters = length; Issue = issue; IssueDistanceMeters = issueDistance;
        }

        /// <summary>
        /// Plafond de braquage de la pose nominale (5.31, decision proprietaire du 2026-09-30) : plus grande vitesse
        /// dont le braquage disponible couvre l'angle de roue nominal tan delta = (L/a) tan e. C'est la condition de
        /// faisabilite du contrat §8 (NominalPoseInfeasible) ; le regime etabli asin(a kappa) du compilateur la
        /// majore sur un pic de courbure plus court que la relaxation de e. +inf sous le braquage haute vitesse.
        /// </summary>
        public static float NominalSteeringCeilingMetersPerSecond(DrivabilityProfile profile, float offsetRadians)
        {
            if (Math.Abs(offsetRadians) >= Math.PI * 0.5) return 0f;
            double delta = Math.Abs(Math.Atan(profile.WheelbaseMeters / profile.ReferencePointAheadRearAxleMeters
                * Math.Tan(offsetRadians))) * 180d / Math.PI;
            if (delta <= profile.HighSpeedLockDegrees) return float.PositiveInfinity;
            if (delta > profile.LowSpeedLockDegrees) return 0f;
            return (float)(profile.FullReductionSpeedMetersPerSecond * (profile.LowSpeedLockDegrees - delta)
                / (profile.LowSpeedLockDegrees - profile.HighSpeedLockDegrees));
        }

        /// <param name="nominalOffsetRadians">
        /// Ecart nominal e de la route au debut de l'horizon (contrat §8). Fourni : chaque point porte e, transporte
        /// par de/ds = kappa - sin(e)/a et saute du saut de tangente signe aux raccords, et le plafond de la pose
        /// nominale. Absent : plafond de regime etabli du compilateur (comportement 5.30).
        /// </param>
        public static PathHorizon Build(CompiledRoadModel model, RoutePlan route, float lookAheadMeters,
            float? nominalOffsetRadians = null)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (route == null) throw new ArgumentNullException("route");
            if (!(lookAheadMeters > 0f) || float.IsInfinity(lookAheadMeters))
                throw new ArgumentException("InvalidLookAhead", "lookAheadMeters");
            if (route.ModelId != model.ModelId || route.ModelVersion != model.Version)
                throw new ArgumentException("StalePlan", "route");

            var profile = model.DrivabilityProfile;
            bool kinematic = nominalOffsetRadians.HasValue && profile.Declared && profile.ReferencePointAheadRearAxleMeters > 0f;
            double e = kinematic ? nominalOffsetRadians.Value : 0d;
            var intervals = new List<PathInterval>();
            var seams = new List<PathSeam>();
            float travelled = 0f;
            PathIssue issue = PathIssue.None;
            float issueAt = 0f;
            HorizonEnd end = HorizonEnd.ExitPortal;
            for (int i = route.ProgressOccurrenceIndex; i < route.Occurrences.Count && travelled < lookAheadMeters; i++)
            {
                var occurrence = route.Occurrences[i];
                RoadCurve curve;
                EffectiveLaneCorridor corridor = default(EffectiveLaneCorridor);
                CompiledJunctionMovement movement = default(CompiledJunctionMovement);
                if (occurrence.Kind == RoadElementKind.LaneCorridor && model.TryGetCorridor(occurrence.Id, out corridor))
                    curve = corridor.Curve;
                else if (occurrence.Kind == RoadElementKind.JunctionMovement && model.TryGetMovement(occurrence.Id, out movement))
                    curve = movement.Curve;
                else
                {
                    issue = PathIssue.MissingElement; issueAt = travelled; break;
                }

                float s0 = i == route.ProgressOccurrenceIndex ? route.ProgressSMeters : occurrence.StartSMeters;
                float s1 = Math.Min(occurrence.EndSMeters, s0 + lookAheadMeters - travelled);
                var points = new List<PathPoint>();
                points.Add(new PathPoint(travelled, curve.Sample(s0), model.DrivabilityProfile));
                var samples = occurrence.Kind == RoadElementKind.LaneCorridor ? corridor.Samples : movement.Samples;
                for (int j = 0; j < samples.Count; j++)
                    if (samples[j].SMeters > s0 && samples[j].SMeters < s1)
                        points.Add(new PathPoint(travelled + samples[j].SMeters - s0,
                            curve.Sample(samples[j].SMeters), model.DrivabilityProfile));
                if (s1 > s0)
                    points.Add(new PathPoint(travelled + s1 - s0, curve.Sample(s1), model.DrivabilityProfile));

                if (kinematic)
                {
                    if (intervals.Count > 0)
                    {
                        var previousPoints = intervals[intervals.Count - 1].Points;
                        e += RoadCurve.SignedTangentJumpRadians(previousPoints[previousPoints.Count - 1].Reference, points[0].Reference);
                    }
                    for (int j = 0; j < points.Count; j++)
                    {
                        if (j > 0)
                            e = curve.AdvanceKinematicOffset(points[j - 1].ElementSMeters, points[j].ElementSMeters, e,
                                profile.ReferencePointAheadRearAxleMeters);
                        points[j] = new PathPoint(points[j].DistanceMeters, points[j].Reference,
                            NominalSteeringCeilingMetersPerSecond(profile, (float)e), (float)e);
                    }
                }

                float maxSlope = 0f;
                for (int j = 1; j < points.Count; j++)
                {
                    var left = points[j - 1]; var right = points[j];
                    float slope = Math.Abs((right.Reference.CurvaturePerMeter - left.Reference.CurvaturePerMeter)
                        / (right.ElementSMeters - left.ElementSMeters));
                    maxSlope = Math.Max(maxSlope, slope);
                    if (issue == PathIssue.None && maxSlope > PlanningTolerances.MaximumCurvatureSlopePerSquareMeter)
                    { issue = PathIssue.CurvatureSlope; issueAt = right.DistanceMeters; }
                }
                if (intervals.Count > 0)
                {
                    var previous = intervals[intervals.Count - 1];
                    var left = previous.Points[previous.Points.Count - 1];
                    var right = points[0];
                    bool ring = IsRoundaboutSeam(model, previous, occurrence)
                        && Math.Abs(Math.Abs(right.Reference.CurvaturePerMeter - left.Reference.CurvaturePerMeter)
                            - PlanningTolerances.RoundaboutCurvaturePerMeter) <= PlanningTolerances.SeamCurvatureJumpPerMeter;
                    var seam = new PathSeam(left, right, ring);
                    seams.Add(seam);
                    if (issue == PathIssue.None)
                    {
                        if (seam.GapMeters > model.ValidationProfile.SeamGapToleranceMeters) issue = PathIssue.SeamGap;
                        else if (seam.TangentJumpDegrees > model.ValidationProfile.SeamTangentToleranceDegrees) issue = PathIssue.SeamTangent;
                        else if (Math.Abs(seam.CurvatureJumpPerMeter) > PlanningTolerances.SeamCurvatureJumpPerMeter && !ring)
                            issue = PathIssue.SeamCurvature;
                        if (issue != PathIssue.None) issueAt = travelled;
                    }
                }
                intervals.Add(new PathInterval(occurrence.Kind, occurrence.Id, s0, s1, travelled, maxSlope, points));
                travelled += s1 - s0;
                if (s1 < occurrence.EndSMeters || (travelled >= lookAheadMeters && i < route.Occurrences.Count - 1))
                    end = HorizonEnd.LookAheadLimit;
            }
            return new PathHorizon(intervals, seams, end, travelled, issue, issueAt);
        }

        private static bool IsRoundaboutSeam(CompiledRoadModel model, PathInterval previous, RouteOccurrence next)
        {
            RoadId movementId = previous.Kind == RoadElementKind.JunctionMovement ? previous.Id
                : next.Kind == RoadElementKind.JunctionMovement ? next.Id : RoadId.None;
            string side = previous.Kind == RoadElementKind.JunctionMovement ? ":exit" : ":entry";
            CompiledJunctionMovement movement;
            Junction junction;
            return !movementId.IsEmpty && SignedRingSeams.Contains(movementId.ToString() + side)
                && model.TryGetMovement(movementId, out movement)
                && model.TryGetJunction(movement.JunctionId, out junction)
                && junction.Feature == JunctionFeature.Roundabout;
        }
    }
}
