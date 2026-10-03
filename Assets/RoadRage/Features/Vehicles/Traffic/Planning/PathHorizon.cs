using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;

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

    /// <summary>Etendue d'un element sur un chemin, telle que la perception la lit.</summary>
    public readonly struct PathSpan
    {
        public readonly RoadElementKind Kind;
        public readonly RoadId Id;
        public readonly float StartSMeters;
        public readonly float EndSMeters;
        public readonly float StartDistanceMeters;
        public readonly float EndDistanceMeters;

        public PathSpan(RoadElementKind kind, RoadId id, float startS, float endS, float startDistance, float endDistance)
        {
            Kind = kind; Id = id; StartSMeters = startS; EndSMeters = endS; StartDistanceMeters = startDistance;
            EndDistanceMeters = endDistance;
        }
    }

    /// <summary>
    /// Geometrie d'un chemin lue par la perception (Story 5.33, D14) : etendues, longueur et, dans l'ordre, positions,
    /// tangentes et distances des points. Implementee par l'horizon de planification et par le chemin complet de la route
    /// (<see cref="RoutePath"/>), qui rend les memes valeurs sans construire les points.
    /// </summary>
    public interface IPathGeometry
    {
        IReadOnlyList<PathSpan> Spans { get; }
        float LengthMeters { get; }
        int PointCount(int span);
        Vector3 PointPosition(int span, int point);
        Vector3 PointTangent(int span, int point);
        float PointDistance(int span, int point);
    }

    /// <summary>
    /// Portee bornee de la planification (Story 5.33, D14) : H = d1 + v_ref^2 / (2 b_plan) + m, ou d1 est le premier noeud du
    /// plan de vitesse au-dela de la previsualisation de la commande, v_ref = max(v desiree, v courante >= 0), b_plan la
    /// deceleration de confort x <see cref="SpeedPlan.PlanningBoundMargin"/> et m une marge numerique. Toute contrainte au-dela
    /// de H, comme l'arret terminal en H, atteint d1 a au moins sqrt(v_ref^2 + 2 b_plan m) > v_ref : elle ne lie ni la vitesse
    /// visee, ni la contrainte limitante, ni l'atteignabilite du plafond (la deceleration depuis v courante s'annule avant H).
    /// </summary>
    public readonly struct PlanningReach
    {
        public readonly float ReferenceSpeedMetersPerSecond;
        public readonly float DecelerationMetersPerSecondSquared;
        public readonly float PreviewMeters;
        public readonly float MarginMeters;

        public PlanningReach(float referenceSpeed, float deceleration, float preview, float margin)
        {
            if (!(referenceSpeed >= 0f) || float.IsInfinity(referenceSpeed) || !(deceleration > 0f) || float.IsInfinity(deceleration)
                || !(preview >= 0f) || float.IsInfinity(preview) || !(margin > 0f) || float.IsInfinity(margin))
                throw new ArgumentException("InvalidPlanningReach");
            ReferenceSpeedMetersPerSecond = referenceSpeed; DecelerationMetersPerSecondSquared = deceleration;
            PreviewMeters = preview; MarginMeters = margin;
        }

        /// <summary>Portee du conducteur a une vitesse et une previsualisation donnees.</summary>
        public static PlanningReach For(DriverProfile driver, float speedMetersPerSecond, float previewMeters, float marginMeters)
        {
            float speed = float.IsNaN(speedMetersPerSecond) || float.IsInfinity(speedMetersPerSecond) ? 0f : Math.Max(0f, speedMetersPerSecond);
            return new PlanningReach(Math.Max(driver.DesiredSpeed, speed), driver.ComfortableDeceleration * SpeedPlan.PlanningBoundMargin,
                previewMeters, marginMeters);
        }

        /// <summary>H pour un premier noeud au-dela de la previsualisation a <paramref name="nodeMeters"/>.</summary>
        public float LengthMeters(float nodeMeters)
        {
            double v = ReferenceSpeedMetersPerSecond;
            return (float)(nodeMeters + v * v / (2d * DecelerationMetersPerSecondSquared) + MarginMeters);
        }
    }

    /// <summary>Geometry read from compiled route occurrences; its lateral offset is identically zero.</summary>
    public sealed class PathHorizon : IPathGeometry
    {
        public IReadOnlyList<PathInterval> Intervals { get; }
        public IReadOnlyList<PathSeam> Seams { get; }
        public HorizonEnd End { get; }
        public float LengthMeters { get; }
        public PathIssue Issue { get; }
        public float IssueDistanceMeters { get; }
        public float MaximumAbsoluteOffsetMeters { get { return 0f; } }
        public IReadOnlyList<PathSpan> Spans { get; }

        private PathHorizon(List<PathInterval> intervals, List<PathSeam> seams, HorizonEnd end,
            float length, PathIssue issue, float issueDistance)
        {
            Intervals = intervals.AsReadOnly(); Seams = seams.AsReadOnly();
            End = end; LengthMeters = length; Issue = issue; IssueDistanceMeters = issueDistance;
            var spans = new PathSpan[intervals.Count];
            for (int i = 0; i < spans.Length; i++)
            {
                var interval = intervals[i];
                spans[i] = new PathSpan(interval.Kind, interval.Id, interval.StartSMeters, interval.EndSMeters,
                    interval.StartDistanceMeters, interval.EndDistanceMeters);
            }
            Spans = Array.AsReadOnly(spans);
        }

        public int PointCount(int span) { return Intervals[span].Points.Count; }
        public Vector3 PointPosition(int span, int point) { return Intervals[span].Points[point].Reference.Position; }
        public Vector3 PointTangent(int span, int point) { return Intervals[span].Points[point].Reference.Tangent; }
        public float PointDistance(int span, int point) { return Intervals[span].Points[point].DistanceMeters; }

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
        /// <param name="reach">
        /// Portee bornee de la planification (Story 5.33, D14) : nulle, l'horizon couvre <paramref name="lookAheadMeters"/>.
        /// </param>
        public static PathHorizon Build(CompiledRoadModel model, RoutePlan route, float lookAheadMeters,
            float? nominalOffsetRadians = null, IReadOnlyList<string> signedRingSeams = null, PlanningReach? reach = null)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (route == null) throw new ArgumentNullException("route");
            if (!(lookAheadMeters > 0f) || float.IsInfinity(lookAheadMeters))
                throw new ArgumentException("InvalidLookAhead", "lookAheadMeters");
            if (route.ModelId != model.ModelId || route.ModelVersion != model.Version)
                throw new ArgumentException("StalePlan", "route");
            if (reach.HasValue)
            {
                float node = FirstPlanNodeBeyond(model, route, reach.Value.PreviewMeters);
                if (!float.IsNaN(node)) lookAheadMeters = Math.Min(lookAheadMeters, reach.Value.LengthMeters(node));
            }

            var profile = model.DrivabilityProfile;
            bool kinematic = nominalOffsetRadians.HasValue && profile.Declared && profile.ReferencePointAheadRearAxleMeters > 0f;
            double e = kinematic ? nominalOffsetRadians.Value : 0d;
            var intervals = new List<PathInterval>(Math.Max(1, route.Occurrences.Count - route.ProgressOccurrenceIndex));
            var seams = new List<PathSeam>(intervals.Capacity);
            float travelled = 0f;
            PathIssue issue = PathIssue.None;
            float issueAt = 0f;
            HorizonEnd end = HorizonEnd.ExitPortal;
            for (int i = route.ProgressOccurrenceIndex; i < route.Occurrences.Count && travelled < lookAheadMeters; i++)
            {
                var occurrence = route.Occurrences[i];
                RoadCurve curve = CurveOf(model, occurrence);
                if (curve == null)
                {
                    issue = PathIssue.MissingElement; issueAt = travelled; break;
                }

                float s0 = i == route.ProgressOccurrenceIndex ? route.ProgressSMeters : occurrence.StartSMeters;
                float s1 = Math.Min(occurrence.EndSMeters, s0 + lookAheadMeters - travelled);
                // Echantillons strictement entre s0 et s1, dans l'ordre : une plage contigue des echantillons tries. Repere de
                // chacun lu dans le cache de la courbe (exactement curve.Sample(s_j)) ; chaque point construit une seule fois,
                // e transporte au fil des points avec un curseur de segment (Story 5.33, D14 : meme horizon au bit pres).
                int first = curve.FirstSampleAbove(s0), last = first;
                while (last < curve.SampleCount && curve.SampleS(last) < s1) last++;
                int count = 1 + (last - first) + (s1 > s0 ? 1 : 0);
                var points = new List<PathPoint>(count);
                int cursor = -1;
                float previousS = 0f;
                for (int j = 0; j < count; j++)
                {
                    RoadCurvePoint reference;
                    float distance;
                    if (j == 0) { reference = curve.Sample(s0); distance = travelled; }
                    else if (j == count - 1 && s1 > s0) { reference = curve.Sample(s1); distance = travelled + s1 - s0; }
                    else
                    {
                        int index = first + j - 1;
                        reference = curve.SampleFrame(index);
                        distance = travelled + curve.SampleS(index) - s0;
                    }
                    if (kinematic)
                    {
                        if (j == 0 && intervals.Count > 0)
                        {
                            var previousPoints = intervals[intervals.Count - 1].Points;
                            e += RoadCurve.SignedTangentJumpRadians(previousPoints[previousPoints.Count - 1].Reference, reference);
                        }
                        if (j > 0)
                            e = curve.AdvanceKinematicOffset(previousS, reference.SMeters, e, profile.ReferencePointAheadRearAxleMeters,
                                ref cursor);
                        points.Add(new PathPoint(distance, reference, NominalSteeringCeilingMetersPerSecond(profile, (float)e), (float)e));
                    }
                    else points.Add(new PathPoint(distance, reference, model.DrivabilityProfile));
                    previousS = reference.SMeters;
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
                    bool ring = IsRoundaboutSeam(model, previous, occurrence, signedRingSeams)
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
                TrafficV2WorkCounters.Work.HorizonPoints += points.Count;
                intervals.Add(new PathInterval(occurrence.Kind, occurrence.Id, s0, s1, travelled, maxSlope, points));
                travelled += s1 - s0;
                if (s1 < occurrence.EndSMeters || (travelled >= lookAheadMeters && i < route.Occurrences.Count - 1))
                    end = HorizonEnd.LookAheadLimit;
            }
            TrafficV2WorkCounters.Work.HorizonBuilds++;
            TrafficV2WorkCounters.Work.HorizonSpans += intervals.Count;
            return new PathHorizon(intervals, seams, end, travelled, issue, issueAt);
        }

        internal static RoadCurve CurveOf(CompiledRoadModel model, RouteOccurrence occurrence)
        {
            EffectiveLaneCorridor corridor;
            CompiledJunctionMovement movement;
            if (occurrence.Kind == RoadElementKind.LaneCorridor && model.TryGetCorridor(occurrence.Id, out corridor)) return corridor.Curve;
            if (occurrence.Kind == RoadElementKind.JunctionMovement && model.TryGetMovement(occurrence.Id, out movement)) return movement.Curve;
            return null;
        }

        /// <summary>
        /// Distance du premier noeud du plan de vitesse d'indice >= 1 situe a au moins <paramref name="preview"/> : memes
        /// distances de points que l'horizon complet (un raccord n'ajoute aucune distance), memes regles de noeuds que
        /// <see cref="SpeedPlan"/> (espacement minimal depuis le noeud precedent). NaN si la route s'acheve avant : aucune borne.
        /// </summary>
        private static float FirstPlanNodeBeyond(CompiledRoadModel model, RoutePlan route, float preview)
        {
            float travelled = 0f, lastDistance = 0f, lastNode = 0f;
            bool any = false;
            for (int i = route.ProgressOccurrenceIndex; i < route.Occurrences.Count; i++)
            {
                var occurrence = route.Occurrences[i];
                var curve = CurveOf(model, occurrence);
                if (curve == null) return float.NaN;
                float s0 = i == route.ProgressOccurrenceIndex ? route.ProgressSMeters : occurrence.StartSMeters;
                float s1 = occurrence.EndSMeters;
                int first = curve.FirstSampleAbove(s0), last = first;
                while (last < curve.SampleCount && curve.SampleS(last) < s1) last++;
                int count = 1 + (last - first) + (s1 > s0 ? 1 : 0);
                for (int j = 0; j < count; j++)
                {
                    float distance = j == 0 ? travelled
                        : j == count - 1 && s1 > s0 ? travelled + s1 - s0
                        : travelled + curve.SampleS(first + j - 1) - s0;
                    if (!any) { any = true; lastDistance = distance; lastNode = distance; continue; }
                    // Raccord : le plan de vitesse fusionne un point qui n'avance pas avec le precedent.
                    if (distance <= lastDistance) continue;
                    lastDistance = distance;
                    if (distance - lastNode >= SpeedPlan.MinimumKnotSpacingMeters)
                    {
                        lastNode = distance;
                        if (distance >= preview) return distance;
                    }
                }
                travelled += s1 - s0;
            }
            return float.NaN;
        }

        private static bool IsRoundaboutSeam(CompiledRoadModel model, PathInterval previous, RouteOccurrence next,
            IReadOnlyList<string> signedRingSeams)
        {
            RoadId movementId = previous.Kind == RoadElementKind.JunctionMovement ? previous.Id
                : next.Kind == RoadElementKind.JunctionMovement ? next.Id : RoadId.None;
            string side = previous.Kind == RoadElementKind.JunctionMovement ? ":exit" : ":entry";
            CompiledJunctionMovement movement;
            Junction junction;
            if (movementId.IsEmpty || !model.TryGetMovement(movementId, out movement)
                || !model.TryGetJunction(movement.JunctionId, out junction)
                || junction.Feature != JunctionFeature.Roundabout) return false;
            if (signedRingSeams == null) return true; // Legacy : la geometrie signee porte deja la couture.
            string key = movementId + side;
            for (int i = 0; i < signedRingSeams.Count; i++)
                if (signedRingSeams[i] == key) return true;
            return false;
        }
    }
}
