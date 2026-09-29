using System;
using System.Collections.Generic;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    public enum ReferenceCoverage { Covered, NotCoveredByGateA, GateAEvidenceMissing, GateAEvidenceStale }
    public enum VehicleCoverage { NotEstablished }
    public enum MotionIssue { None, HorizonNonConforming, InvalidSteeringCeiling, GateAEvidenceMissing,
        GateAEvidenceStale, NotCoveredByGateA }
    // PlanInfeasible est ajoute en fin : le plan ne permet aucun verdict (la cause exacte est PlanIssue).
    public enum SpeedProfileIssue { None, InvalidSpeedProfile, AccelerationBoundExceeded, SteeringCeilingExceeded,
        PlanInfeasible }
    [Flags]
    public enum MotionDiagnostic { None = 0, SteeringInactiveSpan = 1, HorizonTruncated = 2 }

    public readonly struct TrackingTolerance
    {
        public readonly bool Declared;
        public readonly float Meters;
        public static TrackingTolerance Undeclared { get { return default(TrackingTolerance); } }
        public TrackingTolerance(float meters) { Declared = true; Meters = meters; }
    }

    public readonly struct LongitudinalBounds
    {
        public readonly float MaxAcceleration;
        public readonly float MaxDeceleration;
        public LongitudinalBounds(float acceleration, float deceleration)
        {
            MaxAcceleration = acceleration; MaxDeceleration = deceleration;
        }
        public bool Valid { get { return MaxAcceleration > 0f && MaxDeceleration > 0f
            && !float.IsNaN(MaxAcceleration) && !float.IsInfinity(MaxAcceleration)
            && !float.IsNaN(MaxDeceleration) && !float.IsInfinity(MaxDeceleration); } }
    }

    public readonly struct SpeedProfilePoint
    {
        public readonly float DistanceMeters;
        public readonly float SpeedMetersPerSecond;
        public SpeedProfilePoint(float distanceMeters, float speedMetersPerSecond)
        { DistanceMeters = distanceMeters; SpeedMetersPerSecond = speedMetersPerSecond; }
    }

    public readonly struct SpeedProfileResult
    {
        public readonly SpeedProfileIssue Issue;
        public readonly float DistanceMeters;
        public readonly MotionDiagnostic Diagnostics;
        /// <summary>Cause exacte de l'infaisabilite quand <see cref="Issue"/> vaut PlanInfeasible, sinon None.</summary>
        public readonly MotionIssue PlanIssue;
        internal SpeedProfileResult(SpeedProfileIssue issue, float distance, MotionDiagnostic diagnostics,
            MotionIssue planIssue = MotionIssue.None)
        { Issue = issue; DistanceMeters = distance; Diagnostics = diagnostics; PlanIssue = planIssue; }
    }

    /// <summary>Geometric constraints and a candidate checker; no speed profile is generated.</summary>
    public sealed class MotionPlan
    {
        public PathHorizon Path { get; }
        public GateAEvidenceResult Evidence { get; }
        public TrackingTolerance Tracking { get; }
        public ReferenceCoverage ReferenceCoverage { get; }
        public VehicleCoverage VehicleCoverage { get { return VehicleCoverage.NotEstablished; } }
        public MotionIssue Issue { get; }
        public float IssueDistanceMeters { get; }
        public MotionDiagnostic Diagnostics { get; }
        public bool GeometricallyFeasible { get { return Issue == MotionIssue.None; } }
        private readonly DrivabilityProfile _profile;

        public MotionPlan(PathHorizon path, DrivabilityProfile profile, GateAEvidenceResult evidence,
            TrackingTolerance tracking)
        {
            if (path == null) throw new ArgumentNullException("path");
            Path = path; Evidence = evidence; Tracking = tracking; _profile = profile;
            Diagnostics = path.End == HorizonEnd.LookAheadLimit ? MotionDiagnostic.HorizonTruncated : MotionDiagnostic.None;
            if (evidence.Status == GateAEvidenceStatus.GateAEvidenceMissing)
                ReferenceCoverage = ReferenceCoverage.GateAEvidenceMissing;
            else if (evidence.Status == GateAEvidenceStatus.GateAEvidenceStale)
                ReferenceCoverage = ReferenceCoverage.GateAEvidenceStale;
            else if (tracking.Declared && (tracking.Meters < 0f || float.IsNaN(tracking.Meters)
                || float.IsInfinity(tracking.Meters)))
                ReferenceCoverage = ReferenceCoverage.NotCoveredByGateA;
            else ReferenceCoverage = path.MaximumAbsoluteOffsetMeters
                + (tracking.Declared ? tracking.Meters : 0f) <= evidence.TrackingAllowanceMeters
                    ? ReferenceCoverage.Covered : ReferenceCoverage.NotCoveredByGateA;

            Issue = MotionIssue.None;
            IssueDistanceMeters = 0f;
            if (path.Issue != PathIssue.None)
            { Issue = MotionIssue.HorizonNonConforming; IssueDistanceMeters = path.IssueDistanceMeters; }
            else
            {
                foreach (var interval in path.Intervals)
                    foreach (var point in interval.Points)
                        if (Issue == MotionIssue.None
                            && (float.IsNaN(point.SteeringCeilingMetersPerSecond)
                                || point.SteeringCeilingMetersPerSecond < profile.SteeringInactiveBelowMetersPerSecond))
                        { Issue = MotionIssue.InvalidSteeringCeiling; IssueDistanceMeters = point.DistanceMeters; break; }
            }
            if (Issue == MotionIssue.None && ReferenceCoverage != ReferenceCoverage.Covered)
                Issue = ReferenceCoverage == ReferenceCoverage.GateAEvidenceMissing ? MotionIssue.GateAEvidenceMissing
                    : ReferenceCoverage == ReferenceCoverage.GateAEvidenceStale ? MotionIssue.GateAEvidenceStale
                    : MotionIssue.NotCoveredByGateA;
        }

        public SpeedProfileResult VerifySpeedProfile(IReadOnlyList<SpeedProfilePoint> candidate,
            LongitudinalBounds bounds)
        {
            // Un plan infaisable ne juge aucun candidat : jamais None, et la cause reste nommee.
            if (Issue != MotionIssue.None)
                return new SpeedProfileResult(SpeedProfileIssue.PlanInfeasible, IssueDistanceMeters, Diagnostics, Issue);
            if (candidate == null || candidate.Count < 2 || !bounds.Valid)
                return new SpeedProfileResult(SpeedProfileIssue.InvalidSpeedProfile, 0f, Diagnostics);
            for (int i = 0; i < candidate.Count; i++)
            {
                var p = candidate[i];
                if (float.IsNaN(p.DistanceMeters) || float.IsInfinity(p.DistanceMeters)
                    || p.DistanceMeters < 0f || p.DistanceMeters > Path.LengthMeters
                    || float.IsNaN(p.SpeedMetersPerSecond) || float.IsInfinity(p.SpeedMetersPerSecond)
                    || p.SpeedMetersPerSecond < 0f
                    || (i > 0 && p.DistanceMeters <= candidate[i - 1].DistanceMeters))
                    return new SpeedProfileResult(SpeedProfileIssue.InvalidSpeedProfile, p.DistanceMeters, Diagnostics);
            }

            // Un candidat partiel laisserait des portions de l'horizon non jugees.
            if (candidate[0].DistanceMeters > PlanningTolerances.ProfileSpanToleranceMeters)
                return new SpeedProfileResult(SpeedProfileIssue.InvalidSpeedProfile, 0f, Diagnostics);
            if (candidate[candidate.Count - 1].DistanceMeters < Path.LengthMeters - PlanningTolerances.ProfileSpanToleranceMeters)
                return new SpeedProfileResult(SpeedProfileIssue.InvalidSpeedProfile,
                    candidate[candidate.Count - 1].DistanceMeters, Diagnostics);

            for (int i = 1; i < candidate.Count; i++)
            {
                var a = candidate[i - 1]; var b = candidate[i];
                double acceleration = ((double)b.SpeedMetersPerSecond * b.SpeedMetersPerSecond
                    - (double)a.SpeedMetersPerSecond * a.SpeedMetersPerSecond)
                    / (2d * (b.DistanceMeters - a.DistanceMeters));
                if (acceleration > bounds.MaxAcceleration + PlanningTolerances.AccelerationBoundToleranceMetersPerSecondSquared
                    || acceleration < -(bounds.MaxDeceleration + PlanningTolerances.AccelerationBoundToleranceMetersPerSecondSquared))
                    return new SpeedProfileResult(SpeedProfileIssue.AccelerationBoundExceeded, b.DistanceMeters, Diagnostics);
            }

            MotionDiagnostic diagnostics = Diagnostics;
            foreach (var interval in Path.Intervals)
            {
                var knots = new List<float>();
                float from = Math.Max(interval.StartDistanceMeters, candidate[0].DistanceMeters);
                float to = Math.Min(interval.EndDistanceMeters, candidate[candidate.Count - 1].DistanceMeters);
                if (to < from) continue;
                knots.Add(from);
                foreach (var point in interval.Points)
                    if (point.DistanceMeters > from && point.DistanceMeters < to) knots.Add(point.DistanceMeters);
                foreach (var point in candidate)
                    if (point.DistanceMeters > from && point.DistanceMeters < to) knots.Add(point.DistanceMeters);
                if (to > from) knots.Add(to);
                knots.Sort();
                for (int i = 0; i < knots.Count; i++)
                {
                    float s = knots[i];
                    float speed = SpeedAt(candidate, s);
                    float ceiling = CeilingAt(interval, s);
                    if (speed > ceiling)
                        return new SpeedProfileResult(SpeedProfileIssue.SteeringCeilingExceeded, s, diagnostics);
                    if (speed > 0f && speed < _profile.SteeringInactiveBelowMetersPerSecond
                        && Math.Abs(CurvatureAt(interval, s)) > 0f)
                        diagnostics |= MotionDiagnostic.SteeringInactiveSpan;
                    if (i > 0)
                    {
                        float previous = knots[i - 1];
                        float previousSpeed = SpeedAt(candidate, previous);
                        if (Math.Max(previousSpeed, speed) > 0f
                            && Math.Min(previousSpeed, speed) < _profile.SteeringInactiveBelowMetersPerSecond
                            && (Math.Abs(CurvatureAt(interval, previous)) > 0f
                                || Math.Abs(CurvatureAt(interval, s)) > 0f))
                            diagnostics |= MotionDiagnostic.SteeringInactiveSpan;
                        float minimumCeiling = Math.Min(CeilingAt(interval, previous), ceiling);
                        if (Math.Max(previousSpeed, speed) > minimumCeiling)
                            return new SpeedProfileResult(SpeedProfileIssue.SteeringCeilingExceeded, previous, diagnostics);
                    }
                }
            }
            return new SpeedProfileResult(SpeedProfileIssue.None, 0f, diagnostics);
        }

        private static float SpeedAt(IReadOnlyList<SpeedProfilePoint> points, float s)
        {
            for (int i = 1; i < points.Count; i++)
                if (s <= points[i].DistanceMeters)
                {
                    var a = points[i - 1]; var b = points[i];
                    double t = (s - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                    double v2 = a.SpeedMetersPerSecond * (double)a.SpeedMetersPerSecond * (1d - t)
                        + b.SpeedMetersPerSecond * (double)b.SpeedMetersPerSecond * t;
                    return (float)Math.Sqrt(Math.Max(0d, v2));
                }
            return points[points.Count - 1].SpeedMetersPerSecond;
        }

        private static float CeilingAt(PathInterval interval, float s)
        {
            for (int i = 1; i < interval.Points.Count; i++)
                if (s <= interval.Points[i].DistanceMeters)
                    return Math.Min(interval.Points[i - 1].SteeringCeilingMetersPerSecond,
                        interval.Points[i].SteeringCeilingMetersPerSecond);
            return interval.Points[interval.Points.Count - 1].SteeringCeilingMetersPerSecond;
        }

        private static float CurvatureAt(PathInterval interval, float s)
        {
            for (int i = 1; i < interval.Points.Count; i++)
                if (s <= interval.Points[i].DistanceMeters)
                {
                    var a = interval.Points[i - 1]; var b = interval.Points[i];
                    float t = (s - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                    return a.Reference.CurvaturePerMeter + t * (b.Reference.CurvaturePerMeter - a.Reference.CurvaturePerMeter);
                }
            return interval.Points[interval.Points.Count - 1].Reference.CurvaturePerMeter;
        }
    }
}
