using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    public enum ReferenceCoverage { Covered, NotCoveredByGateA, GateAEvidenceMissing, GateAEvidenceStale }
    // 5.31 : valeurs ajoutees en fin. Un MotionPlan reste NotEstablished ; le verdict vehicule vient de
    // EvaluateVehicleCoverage, qui exige un epsilon_t declare (la regle 5.30 "compte 0" n'autorise rien).
    // PoseModelMismatch (correct-course 2026-09-29) : la preuve valide n'est pas calculee sur la pose nominale
    // cinematique ; elle ne rend jamais "couvert" (code d'insertion NotCoveredByGateA).
    public enum VehicleCoverage { NotEstablished, TrackingToleranceUndeclared, NotCoveredByGateA, Covered,
        GateAEvidenceMissing, GateAEvidenceStale, PoseModelMismatch }
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
        // Story 5.43 : b_safe = 0 est une borne comportementale valide, aucun freinage autorise dans le profil.
        // Les capacites physiques et le freinage de securite sont des bornes distinctes, toujours positives.
        public bool Valid { get { return MaxAcceleration > 0f && MaxDeceleration >= 0f
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

        /// <summary>
        /// Couverture vehicule (5.31) : couvert si et seulement si la preuve est valide, epsilon_t declare et
        /// max|o| + epsilon_t &lt;= a_e. Ni la marge reservee ni un residu n'entrent dans ce calcul.
        /// </summary>
        public static VehicleCoverage EvaluateVehicleCoverage(GateAEvidenceResult evidence, float maximumAbsoluteOffsetMeters,
            TrackingTolerance declared)
        {
            if (evidence.Status == GateAEvidenceStatus.GateAEvidenceMissing) return VehicleCoverage.GateAEvidenceMissing;
            if (evidence.Status == GateAEvidenceStatus.GateAEvidenceStale) return VehicleCoverage.GateAEvidenceStale;
            if (!declared.Declared) return VehicleCoverage.TrackingToleranceUndeclared;
            if (declared.Meters < 0f || float.IsNaN(declared.Meters) || float.IsInfinity(declared.Meters)
                || float.IsNaN(maximumAbsoluteOffsetMeters) || float.IsInfinity(maximumAbsoluteOffsetMeters))
                return VehicleCoverage.NotCoveredByGateA;
            // Une preuve a pose tangente ne certifie aucune couverture physique (contrat §8).
            if (evidence.PoseModel != NominalPoseModel.Kinematic) return VehicleCoverage.PoseModelMismatch;
            return Math.Abs(maximumAbsoluteOffsetMeters) + declared.Meters <= evidence.TrackingAllowanceMeters
                ? VehicleCoverage.Covered : VehicleCoverage.NotCoveredByGateA;
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
            TrafficV2WorkCounters.Work.VerifyCalls++;
            var knots = new List<float>();
            // Les noeuds croissent aussi d'un intervalle au suivant (intervalles contigus) : un seul curseur dans le profil.
            int candidateCursor = 1;
            foreach (var interval in Path.Intervals)
            {
                knots.Clear();
                float from = Math.Max(interval.StartDistanceMeters, candidate[0].DistanceMeters);
                float to = Math.Min(interval.EndDistanceMeters, candidate[candidate.Count - 1].DistanceMeters);
                if (to < from) continue;
                // Noeuds de l'intervalle : from, puis la fusion de deux suites deja triees (points de l'intervalle et du profil
                // strictement entre from et to), puis to. C'est la meme suite que l'ajout de tous ces points suivi d'un tri
                // (Story 5.33, D14 : meme resultat au bit pres, sans parcourir tout le profil a chaque intervalle).
                knots.Add(from);
                var points = interval.Points;
                int p = 0;
                while (p < points.Count && !(points[p].DistanceMeters > from)) p++;
                int c = FirstAbove(candidate, from);
                while (true)
                {
                    bool hasPoint = p < points.Count && points[p].DistanceMeters < to;
                    bool hasCandidate = c < candidate.Count && candidate[c].DistanceMeters < to;
                    if (!hasPoint && !hasCandidate) break;
                    if (hasPoint && (!hasCandidate || points[p].DistanceMeters <= candidate[c].DistanceMeters))
                        knots.Add(points[p++].DistanceMeters);
                    else knots.Add(candidate[c++].DistanceMeters);
                }
                if (to > from) knots.Add(to);
                TrafficV2WorkCounters.Work.VerifyKnots += knots.Count;
                // Vitesse, plafond et courbure du noeud precedent : memes fonctions pures sur la meme abscisse, reutilisees.
                // Story 5.33, D15 : les noeuds croissent, donc le premier indice i >= 1 tel que s <= d_i ne recule jamais ; un
                // curseur le suit au lieu d'une recherche dichotomique par noeud (meme indice, memes expressions, meme resultat).
                float previousSpeed = 0f, previousCeiling = 0f, previousCurvature = 0f;
                int pointCursor = 1;
                for (int i = 0; i < knots.Count; i++)
                {
                    float s = knots[i];
                    while (candidateCursor <= candidate.Count - 1 && !(s <= candidate[candidateCursor].DistanceMeters)) candidateCursor++;
                    while (pointCursor <= points.Count - 1 && !(s <= points[pointCursor].DistanceMeters)) pointCursor++;
                    float speed = SpeedAt(candidate, candidateCursor <= candidate.Count - 1 ? candidateCursor : -1, s);
                    int at = pointCursor <= points.Count - 1 ? pointCursor : -1;
                    float ceiling = CeilingAt(points, at);
                    float curvature = CurvatureAt(points, at, s);
                    if (speed > ceiling)
                        return new SpeedProfileResult(SpeedProfileIssue.SteeringCeilingExceeded, s, diagnostics);
                    if (speed > 0f && speed < _profile.SteeringInactiveBelowMetersPerSecond && Math.Abs(curvature) > 0f)
                        diagnostics |= MotionDiagnostic.SteeringInactiveSpan;
                    if (i > 0)
                    {
                        if (Math.Max(previousSpeed, speed) > 0f
                            && Math.Min(previousSpeed, speed) < _profile.SteeringInactiveBelowMetersPerSecond
                            && (Math.Abs(previousCurvature) > 0f || Math.Abs(curvature) > 0f))
                            diagnostics |= MotionDiagnostic.SteeringInactiveSpan;
                        float minimumCeiling = Math.Min(previousCeiling, ceiling);
                        if (Math.Max(previousSpeed, speed) > minimumCeiling)
                            return new SpeedProfileResult(SpeedProfileIssue.SteeringCeilingExceeded, knots[i - 1], diagnostics);
                    }
                    previousSpeed = speed; previousCeiling = ceiling; previousCurvature = curvature;
                }
            }
            return new SpeedProfileResult(SpeedProfileIssue.None, 0f, diagnostics);
        }

        /// <summary>Premier indice d'abscisse strictement superieure a <paramref name="s"/> (Count si aucun).</summary>
        private static int FirstAbove(IReadOnlyList<SpeedProfilePoint> points, float s)
        {
            int low = 0, high = points.Count;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (points[mid].DistanceMeters > s) high = mid; else low = mid + 1;
            }
            return low;
        }

        // 5.31 : premier i >= 1 tel que s <= d_i (-1 si aucun), le meme indice que le parcours lineaire d'origine. Story 5.33,
        // D15 : fourni par un curseur monotone de la verification au lieu d'une recherche dichotomique par appel.
        private static float SpeedAt(IReadOnlyList<SpeedProfilePoint> points, int i, float s)
        {
            if (i > 0)
            {
                var a = points[i - 1]; var b = points[i];
                double t = (s - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                double v2 = a.SpeedMetersPerSecond * (double)a.SpeedMetersPerSecond * (1d - t)
                    + b.SpeedMetersPerSecond * (double)b.SpeedMetersPerSecond * t;
                return (float)Math.Sqrt(Math.Max(0d, v2));
            }
            return points[points.Count - 1].SpeedMetersPerSecond;
        }

        private static float CeilingAt(IReadOnlyList<PathPoint> points, int i)
        {
            if (i > 0)
                return Math.Min(points[i - 1].SteeringCeilingMetersPerSecond, points[i].SteeringCeilingMetersPerSecond);
            return points[points.Count - 1].SteeringCeilingMetersPerSecond;
        }

        private static float CurvatureAt(IReadOnlyList<PathPoint> points, int i, float s)
        {
            if (i > 0)
            {
                var a = points[i - 1]; var b = points[i];
                float t = (s - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                return a.Reference.CurvaturePerMeter + t * (b.Reference.CurvaturePerMeter - a.Reference.CurvaturePerMeter);
            }
            return points[points.Count - 1].Reference.CurvaturePerMeter;
        }
    }
}
