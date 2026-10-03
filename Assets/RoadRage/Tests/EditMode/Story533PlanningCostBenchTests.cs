using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.33, D13 bis (decision proprietaire du 2026-10-02) : banc de cout de l'horizon, du plan de vitesse et de
    /// l'occupation de la frame sur les 11 routes de reference 5.31, avant tout changement runtime. Pour chaque etat le
    /// long des routes (pose nominale, ecart e de la reference, comme le driver) : cout actuel ventile, allocations,
    /// longueur et points de l'horizon ; prototypes exacts (resultats compares au bit pres aux sorties runtime) ; portee
    /// bornee derivee de la distance d'arret, avec controle de l'identite de la commande. Ne change aucun comportement :
    /// les prototypes vivent dans ce banc et n'utilisent que l'API publique. Rapport : planning-cost-bench.md.
    /// </summary>
    [Explicit]
    [Category("Core")]
    [Category("Story533PerfBench")]
    public sealed class Story533PlanningCostBenchTests
    {
        private const float Dt = 0.02f;
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string CampaignPath = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/campaign-5-31.json";
        private const string ReportPath = "_bmad-output/implementation-artifacts/traffic-v2-5-33-explorations/planning-cost-bench.md";
        private const float StateSpacingMeters = 3f;
        private const int Repeats = 3;
        private const float TimingSpeed = 4f;
        /// <summary>Marge de la portee bornee au-dela de la distance d'arret (m) : absorbe l'arrondi de la chaine arriere.</summary>
        private const float BoundedMarginMeters = 1f;
        /// <summary>Tolerance de borne de domaine de RoadCurve.Project (constante privee recopiee).</summary>
        private const float BoundEpsilonMeters = 1e-4f;
        private const int BlockSize = 8;

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };

        private static TrafficV2Admission admission;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                        File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
                Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
                return admission;
            }
        }

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; }
        }

        // ================================================================== routes et etats

        [Serializable]
        private sealed class CampaignTripletRecord
        {
            public int Index;
            public string Entry;
            public string Exit;
            public string Via;
            public ulong Seed;
            public ulong InsertionCounter;
        }

        [Serializable]
        private sealed class CampaignFile
        {
            public string RoadModelVersion;
            public CampaignTripletRecord[] Triplets;
        }

        private sealed class Lane
        {
            public RoutePlan Route;
            public ReferenceTrack Track;
            public RoadId[] Ids;
            public TrafficV2Insertion Insertion;
        }

        private sealed class State
        {
            public Lane Lane;
            public float Distance;
            public int Piece;
            public float Offset;
            public RoutePlan Route;
            public PlanningDecision Decision;
            public TrafficFrame Frame;
            public VehicleFootprintPose Pose;
        }

        private static List<State> States()
        {
            var model = Admission.Model;
            var campaign = JsonUtility.FromJson<CampaignFile>(File.ReadAllText(CampaignPath));
            Assert.That(campaign.Triplets.Length, Is.EqualTo(11));
            var states = new List<State>();
            foreach (var triplet in campaign.Triplets)
            {
                var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, RoadId.Parse(triplet.Entry), RoadId.Parse(triplet.Exit),
                    triplet.Seed, triplet.InsertionCounter, Driver, Dt, string.IsNullOrEmpty(triplet.Via) ? RoadId.None : RoadId.Parse(triplet.Via));
                Assert.That(insertion.Code, Is.EqualTo(TrafficV2Code.Allowed), "triplet " + triplet.Index);
                var lane = new Lane { Route = insertion.Route, Insertion = insertion,
                    Track = ReferenceTrack.FromRoute(model, insertion.Route.Occurrences, 0f),
                    Ids = insertion.Route.Occurrences.Select(o => o.Id).ToArray() };
                var route = lane.Route;
                var via = insertion.ViaMovementId;
                // Poses regulieres et au milieu de chaque morceau : la progression n'avance que d'une occurrence contigue.
                var distances = new SortedSet<float>();
                for (float d = 0f; d < lane.Track.LengthMeters - 0.25f; d += StateSpacingMeters) distances.Add(d);
                foreach (var piece in lane.Track.Pieces) distances.Add(0.5f * (piece.StartDistanceMeters + piece.EndDistanceMeters));
                foreach (float distance in distances)
                {
                    int piece = lane.Track.PieceAt(distance);
                    var nominal = lane.Track.Nominal(piece, distance);
                    var pose = new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward, Up = nominal.Up, Footprint = Car };
                    var frame = new TrafficFrame(1, model, new[] { new TrafficActorInput(insertion.TrafficId, pose, TimingSpeed,
                        lane.Track.Pieces[piece].Id, lane.Ids, lane.Track.KinematicAnchors(piece)) });
                    if (!via.IsEmpty && route.ViaOccurrenceIndex >= 0 && route.ProgressOccurrenceIndex > route.ViaOccurrenceIndex)
                        via = RoadId.None;
                    float offset = lane.Track.OffsetRadians(piece, distance);
                    var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, route, route.ExitPortalId,
                        insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null,
                        null, Admission.Evidence, via, offset));
                    if (decision.Route.Plan != null) route = decision.Route.Plan;
                    if (decision.Path == null || decision.Motion == null || decision.Motion.Issue != MotionIssue.None) continue;
                    states.Add(new State { Lane = lane, Distance = distance, Piece = piece, Offset = offset, Route = route,
                        Decision = decision, Frame = frame, Pose = pose });
                }
            }
            return states;
        }

        // ================================================================== mesure

        private static double Elapsed(Action action)
        {
            var watch = Stopwatch.StartNew();
            action();
            return watch.Elapsed.TotalMilliseconds;
        }

        /// <summary>Meilleur de <see cref="Repeats"/> executions apres une execution de chauffe (ms).</summary>
        private static double Best(Action action)
        {
            action();
            double best = double.MaxValue;
            for (int r = 0; r < Repeats; r++) best = Math.Min(best, Elapsed(action));
            return best;
        }

        /// <summary>Octets alloues par une execution (GC.GetTotalMemory sans collection) ; -1 si chaque essai a collecte.</summary>
        private static long Allocated(Action action)
        {
            for (int attempt = 0; attempt < 4; attempt++)
            {
                int collections = GC.CollectionCount(0);
                long before = GC.GetTotalMemory(false);
                action();
                long after = GC.GetTotalMemory(false);
                if (GC.CollectionCount(0) == collections) return Math.Max(0L, after - before);
            }
            return -1L;
        }

        private sealed class Series
        {
            public readonly List<double> Values = new List<double>();
            public void Add(double value) { if (!double.IsNaN(value) && value >= 0) Values.Add(value); }
            public double Mean { get { return Values.Count == 0 ? double.NaN : Values.Average(); } }
            public double P95 { get { return Percentile(0.95); } }
            public double Max { get { return Values.Count == 0 ? double.NaN : Values.Max(); } }
            private double Percentile(double p)
            {
                if (Values.Count == 0) return double.NaN;
                var sorted = Values.OrderBy(v => v).ToList();
                return sorted[Math.Min(sorted.Count - 1, (int)Math.Floor(p * (sorted.Count - 1) + 0.5))];
            }
        }

        private static string F(double value)
        {
            return double.IsNaN(value) ? "-" : value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        // ================================================================== caches par element (precalcul du modele compile)

        private sealed class ElementCache
        {
            public RoadElementKind Kind;
            public RoadCurve Curve;
            public IReadOnlyList<RoadCurveSample> Samples;
            /// <summary>curve.Sample(samples[i].SMeters), meme appel que l'horizon : identique au bit pres.</summary>
            public RoadCurvePoint[] Frames;
            public float KappaMax;
            /// <summary>Extremites a et b de chaque segment pour une projection sur tout le domaine, et leurs boites.</summary>
            public Vector3[] SegmentA, SegmentB, SegmentMin, SegmentMax, BlockMin, BlockMax;
        }

        private static readonly Dictionary<RoadId, ElementCache> Caches = new Dictionary<RoadId, ElementCache>();

        private static ElementCache CacheOf(RoadId id)
        {
            ElementCache cache;
            if (Caches.TryGetValue(id, out cache)) return cache;
            RoadElementKind kind;
            RoadCurve curve;
            IReadOnlyList<RoadCurveSample> samples;
            Assert.That(TrafficFrame.TryGetElement(Admission.Model, id, out kind, out curve, out samples), Is.True, id.ToString());
            cache = new ElementCache { Kind = kind, Curve = curve, Samples = samples, Frames = new RoadCurvePoint[samples.Count] };
            for (int i = 0; i < samples.Count; i++)
            {
                cache.Frames[i] = curve.Sample(samples[i].SMeters);
                cache.KappaMax = Mathf.Max(cache.KappaMax, Mathf.Abs(samples[i].CurvaturePerMeter));
            }
            int segments = samples.Count - 1;
            cache.SegmentA = new Vector3[segments]; cache.SegmentB = new Vector3[segments];
            cache.SegmentMin = new Vector3[segments]; cache.SegmentMax = new Vector3[segments];
            float sMin = Mathf.Clamp(curve.StartS, curve.StartS, curve.Length), sMax = Mathf.Clamp(curve.Length, curve.StartS, curve.Length);
            for (int i = 0; i < segments; i++)
            {
                // Memes expressions que RoadCurve.Project(point, StartS, Length).
                float s0 = samples[i].SMeters, s1 = samples[i + 1].SMeters, span = s1 - s0;
                float tMin = Mathf.Max(0f, (sMin - s0) / span);
                float tMax = Mathf.Min(1f, (sMax - s0) / span);
                var a = Vector3.LerpUnclamped(samples[i].Position, samples[i + 1].Position, tMin);
                var b = Vector3.LerpUnclamped(samples[i].Position, samples[i + 1].Position, tMax);
                cache.SegmentA[i] = a; cache.SegmentB[i] = b;
                cache.SegmentMin[i] = Vector3.Min(a, b); cache.SegmentMax[i] = Vector3.Max(a, b);
            }
            int blocks = (segments + BlockSize - 1) / BlockSize;
            cache.BlockMin = new Vector3[blocks]; cache.BlockMax = new Vector3[blocks];
            for (int k = 0; k < blocks; k++)
            {
                var min = cache.SegmentMin[k * BlockSize];
                var max = cache.SegmentMax[k * BlockSize];
                for (int i = k * BlockSize + 1; i < Math.Min(segments, (k + 1) * BlockSize); i++)
                { min = Vector3.Min(min, cache.SegmentMin[i]); max = Vector3.Max(max, cache.SegmentMax[i]); }
                cache.BlockMin[k] = min; cache.BlockMax[k] = max;
            }
            Caches.Add(id, cache);
            return cache;
        }

        // ================================================================== horizon : replique ventilee de l'algorithme actuel

        private struct HP
        {
            public float Distance;
            public float ElementS;
            public RoadCurvePoint Reference;
            public float Ceiling;
            public float E;
        }

        /// <summary>Positions d'echantillonnage de l'horizon actuel (s0, echantillons interieurs, s1) par intervalle.</summary>
        private sealed class HorizonLayout
        {
            public readonly List<RouteOccurrence> Occurrences = new List<RouteOccurrence>();
            public readonly List<float> S0 = new List<float>(), S1 = new List<float>(), Start = new List<float>();
            public readonly List<float[]> Positions = new List<float[]>();
            public int Points;
            public float Length;
        }

        private static HorizonLayout Layout(RoutePlan route, float lookAhead)
        {
            var layout = new HorizonLayout();
            float travelled = 0f;
            for (int i = route.ProgressOccurrenceIndex; i < route.Occurrences.Count && travelled < lookAhead; i++)
            {
                var occurrence = route.Occurrences[i];
                var cache = CacheOf(occurrence.Id);
                float s0 = i == route.ProgressOccurrenceIndex ? route.ProgressSMeters : occurrence.StartSMeters;
                float s1 = Math.Min(occurrence.EndSMeters, s0 + lookAhead - travelled);
                var positions = new List<float> { s0 };
                for (int j = 0; j < cache.Samples.Count; j++)
                    if (cache.Samples[j].SMeters > s0 && cache.Samples[j].SMeters < s1) positions.Add(cache.Samples[j].SMeters);
                if (s1 > s0) positions.Add(s1);
                layout.Occurrences.Add(occurrence); layout.S0.Add(s0); layout.S1.Add(s1); layout.Start.Add(travelled);
                layout.Positions.Add(positions.ToArray());
                layout.Points += positions.Count;
                travelled += s1 - s0;
            }
            layout.Length = travelled;
            return layout;
        }

        private static List<RoadCurvePoint[]> SampleAll(HorizonLayout layout)
        {
            var frames = new List<RoadCurvePoint[]>(layout.Positions.Count);
            for (int i = 0; i < layout.Positions.Count; i++)
            {
                var curve = CacheOf(layout.Occurrences[i].Id).Curve;
                var positions = layout.Positions[i];
                var result = new RoadCurvePoint[positions.Length];
                for (int j = 0; j < positions.Length; j++) result[j] = curve.Sample(positions[j]);
                frames.Add(result);
            }
            return frames;
        }

        private static List<List<HP>> BuildPoints(HorizonLayout layout, List<RoadCurvePoint[]> frames)
        {
            var profile = Admission.Model.DrivabilityProfile;
            var intervals = new List<List<HP>>(frames.Count);
            for (int i = 0; i < frames.Count; i++)
            {
                var points = new List<HP>();
                float s0 = layout.S0[i], travelled = layout.Start[i];
                var positions = layout.Positions[i];
                for (int j = 0; j < frames[i].Length; j++)
                {
                    float distance = j == 0 ? travelled : travelled + positions[j] - s0;
                    points.Add(new HP { Distance = distance, ElementS = frames[i][j].SMeters, Reference = frames[i][j],
                        Ceiling = RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profile, frames[i][j].CurvaturePerMeter), E = float.NaN });
                }
                intervals.Add(points);
            }
            return intervals;
        }

        private static void Transport(HorizonLayout layout, List<List<HP>> intervals, double e0)
        {
            var profile = Admission.Model.DrivabilityProfile;
            double e = e0;
            for (int i = 0; i < intervals.Count; i++)
            {
                var points = intervals[i];
                var curve = CacheOf(layout.Occurrences[i].Id).Curve;
                if (i > 0)
                {
                    var previous = intervals[i - 1];
                    e += RoadCurve.SignedTangentJumpRadians(previous[previous.Count - 1].Reference, points[0].Reference);
                }
                for (int j = 0; j < points.Count; j++)
                {
                    if (j > 0) e = curve.AdvanceKinematicOffset(points[j - 1].ElementS, points[j].ElementS, e, profile.ReferencePointAheadRearAxleMeters);
                    var p = points[j];
                    p.Ceiling = PathHorizon.NominalSteeringCeilingMetersPerSecond(profile, (float)e);
                    p.E = (float)e;
                    points[j] = p;
                }
            }
        }

        private static int SlopesAndSeams(HorizonLayout layout, List<List<HP>> intervals)
        {
            int issues = 0;
            var model = Admission.Model;
            for (int i = 0; i < intervals.Count; i++)
            {
                var points = intervals[i];
                float maxSlope = 0f;
                for (int j = 1; j < points.Count; j++)
                {
                    float slope = Math.Abs((points[j].Reference.CurvaturePerMeter - points[j - 1].Reference.CurvaturePerMeter)
                        / (points[j].ElementS - points[j - 1].ElementS));
                    maxSlope = Math.Max(maxSlope, slope);
                }
                if (maxSlope > PlanningTolerances.MaximumCurvatureSlopePerSquareMeter) issues++;
                if (i > 0)
                {
                    var left = intervals[i - 1][intervals[i - 1].Count - 1];
                    var right = points[0];
                    float gap = Vector3.Distance(left.Reference.Position, right.Reference.Position);
                    float angle = Vector3.Angle(left.Reference.Tangent, right.Reference.Tangent);
                    if (gap > model.ValidationProfile.SeamGapToleranceMeters || angle > model.ValidationProfile.SeamTangentToleranceDegrees) issues++;
                }
            }
            return issues;
        }

        // ================================================================== horizon : prototype exact

        /// <summary>
        /// Meme horizon que PathHorizon.Build, au bit pres : reperes des echantillons lus dans le cache (meme appel
        /// curve.Sample), tableaux de taille exacte, transport de e sans recherche dichotomique par point (curseur qui rend
        /// l'indice de Locate), une seule construction de point. Pentes et raccords inchanges.
        /// </summary>
        private static List<HP[]> HorizonExact(RoutePlan route, float lookAhead, double e0)
        {
            var profile = Admission.Model.DrivabilityProfile;
            float a = profile.ReferencePointAheadRearAxleMeters;
            double e = e0;
            var intervals = new List<HP[]>();
            float travelled = 0f;
            for (int i = route.ProgressOccurrenceIndex; i < route.Occurrences.Count && travelled < lookAhead; i++)
            {
                var occurrence = route.Occurrences[i];
                var cache = CacheOf(occurrence.Id);
                var samples = cache.Samples;
                float s0 = i == route.ProgressOccurrenceIndex ? route.ProgressSMeters : occurrence.StartSMeters;
                float s1 = Math.Min(occurrence.EndSMeters, s0 + lookAhead - travelled);
                int first = FirstAbove(samples, s0);
                int last = first;
                while (last < samples.Count && samples[last].SMeters < s1) last++;
                int count = 1 + (last - first) + (s1 > s0 ? 1 : 0);
                var points = new HP[count];
                var start = cache.Curve.Sample(s0);
                points[0] = new HP { Distance = travelled, ElementS = start.SMeters, Reference = start };
                for (int j = first; j < last; j++)
                    points[1 + j - first] = new HP { Distance = travelled + samples[j].SMeters - s0, ElementS = cache.Frames[j].SMeters,
                        Reference = cache.Frames[j] };
                if (s1 > s0)
                {
                    var end = cache.Curve.Sample(s1);
                    points[count - 1] = new HP { Distance = travelled + s1 - s0, ElementS = end.SMeters, Reference = end };
                }
                if (intervals.Count > 0)
                {
                    var previous = intervals[intervals.Count - 1];
                    e += RoadCurve.SignedTangentJumpRadians(previous[previous.Length - 1].Reference, points[0].Reference);
                }
                int cursor = 0;
                for (int j = 0; j < count; j++)
                {
                    if (j > 0) e = AdvanceExact(cache, ref cursor, points[j - 1].ElementS, points[j].ElementS, e, a);
                    points[j].Ceiling = PathHorizon.NominalSteeringCeilingMetersPerSecond(profile, (float)e);
                    points[j].E = (float)e;
                }
                intervals.Add(points);
                travelled += s1 - s0;
            }
            return intervals;
        }

        private static int FirstAbove(IReadOnlyList<RoadCurveSample> samples, float s)
        {
            int low = 0, high = samples.Count;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (samples[mid].SMeters > s) high = mid; else low = mid + 1;
            }
            return low;
        }

        /// <summary>RoadCurve.AdvanceKinematicOffset au bit pres ; l'indice de Locate est suivi par un curseur (s croissant).</summary>
        private static double AdvanceExact(ElementCache cache, ref int cursor, float s0, float s1, double e, float a)
        {
            var samples = cache.Samples;
            var curve = cache.Curve;
            s0 = Mathf.Clamp(s0, curve.StartS, curve.Length);
            s1 = Mathf.Clamp(s1, curve.StartS, curve.Length);
            if (!(a > 0f) || !(s1 > s0)) return e;
            float located = Mathf.Clamp(s0, curve.StartS, curve.Length);
            while (cursor + 1 <= samples.Count - 2 && samples[cursor + 1].SMeters <= located) cursor++;
            for (int i = cursor; i + 1 < samples.Count && samples[i].SMeters < s1; i++)
            {
                double x0 = Math.Max(s0, samples[i].SMeters), x1 = Math.Min(s1, samples[i + 1].SMeters);
                if (!(x1 > x0)) continue;
                double origin = samples[i].SMeters;
                double k0 = samples[i].CurvaturePerMeter;
                double slope = (samples[i + 1].CurvaturePerMeter - k0) / (samples[i + 1].SMeters - origin);
                int steps = Math.Max(1, (int)Math.Ceiling((x1 - x0) / RoadCurve.KinematicOffsetStepMeters));
                double h = (x1 - x0) / steps;
                for (int n = 0; n < steps; n++)
                {
                    double x = x0 + h * n - origin;
                    double r1 = k0 + slope * x - Math.Sin(e) / a;
                    double r2 = k0 + slope * (x + 0.5 * h) - Math.Sin(e + 0.5 * h * r1) / a;
                    double r3 = k0 + slope * (x + 0.5 * h) - Math.Sin(e + 0.5 * h * r2) / a;
                    double r4 = k0 + slope * (x + h) - Math.Sin(e + h * r3) / a;
                    e += h / 6.0 * (r1 + 2.0 * r2 + 2.0 * r3 + r4);
                }
            }
            return e;
        }

        private static int CompareHorizon(PathHorizon runtime, List<HP[]> exact)
        {
            int mismatches = Math.Abs(runtime.Intervals.Count - exact.Count);
            for (int i = 0; i < Math.Min(runtime.Intervals.Count, exact.Count); i++)
            {
                var r = runtime.Intervals[i].Points;
                var x = exact[i];
                if (r.Count != x.Length) { mismatches++; continue; }
                for (int j = 0; j < r.Count; j++)
                {
                    var p = r[j]; var q = x[j];
                    if (p.DistanceMeters != q.Distance || p.ElementSMeters != q.ElementS || p.SteeringCeilingMetersPerSecond != q.Ceiling
                        || !(p.NominalOffsetRadians == q.E || (float.IsNaN(p.NominalOffsetRadians) && float.IsNaN(q.E)))
                        // Vector3 == de Unity tolere ~1e-5 : Equals compare chaque composante exactement.
                        || !p.Reference.Position.Equals(q.Reference.Position) || !p.Reference.Tangent.Equals(q.Reference.Tangent)
                        || !p.Reference.Up.Equals(q.Reference.Up) || !p.Reference.Right.Equals(q.Reference.Right)
                        || p.Reference.CurvaturePerMeter != q.Reference.CurvaturePerMeter
                        || p.Reference.HalfWidthLeftMeters != q.Reference.HalfWidthLeftMeters
                        || p.Reference.HalfWidthRightMeters != q.Reference.HalfWidthRightMeters)
                        mismatches++;
                }
            }
            return mismatches;
        }

        // ================================================================== plan de vitesse : replique et prototype exact

        private sealed class PlanTimes
        {
            public double Flatten, Knots, Passes, Points, Verify;
        }

        private sealed class PlanResult
        {
            public double[] D, Speed;
            public SpeedConstraint[] Binding;
            public SpeedConstraint PlanBinding, Limiting;
            public float Deceleration;
            public SpeedProfileIssue Issue;
            public float IssueDistance;
            public MotionDiagnostic Diagnostics;
        }

        /// <summary>
        /// Algorithme de SpeedPlan.Build (chemin nominal) : <paramref name="exact"/> faux, listes et verification runtime,
        /// ventile par <paramref name="times"/> ; vrai, prototype exact (tableaux de taille exacte, verification par fusion).
        /// </summary>
        private static PlanResult Plan(MotionPlan motion, DriverProfile driver, float currentSpeed, bool exact, PlanTimes times)
        {
            var model = Admission.Model;
            var watch = Stopwatch.StartNew();
            var path = motion.Path;
            float desired = driver.DesiredSpeed;
            double lateral = Math.Min(driver.ComfortableDeceleration, float.PositiveInfinity);
            var limitsByInterval = new float[path.Intervals.Count];
            for (int i = 0; i < limitsByInterval.Length; i++)
                limitsByInterval[i] = SpeedPlan.RoadLimitOf(model, path.Intervals[i].Kind, path.Intervals[i].Id).CapMetersPerSecond;

            IList<float> distances, ceilings, curvatures, limits;
            IList<bool> unbounded;
            int count = 0;
            if (exact)
            {
                int total = 0;
                foreach (var interval in path.Intervals) total += interval.Points.Count;
                distances = new float[total]; ceilings = new float[total]; curvatures = new float[total]; limits = new float[total]; unbounded = new bool[total];
            }
            else
            {
                distances = new List<float>(); ceilings = new List<float>(); curvatures = new List<float>(); limits = new List<float>(); unbounded = new List<bool>();
            }
            for (int interval = 0; interval < path.Intervals.Count; interval++)
            {
                float limit = limitsByInterval[interval];
                foreach (var point in path.Intervals[interval].Points)
                {
                    int last = count - 1;
                    float curvature = Math.Abs(point.Reference.CurvaturePerMeter);
                    if (last >= 0 && point.DistanceMeters <= distances[last])
                    {
                        if (!point.Unbounded && (unbounded[last] || point.SteeringCeilingMetersPerSecond < ceilings[last]))
                        { ceilings[last] = point.SteeringCeilingMetersPerSecond; unbounded[last] = false; }
                        curvatures[last] = Math.Max(curvatures[last], curvature);
                        limits[last] = Math.Min(limits[last], limit);
                        continue;
                    }
                    if (exact)
                    {
                        distances[count] = point.DistanceMeters; ceilings[count] = point.Unbounded ? 0f : point.SteeringCeilingMetersPerSecond;
                        unbounded[count] = point.Unbounded; curvatures[count] = curvature; limits[count] = limit;
                    }
                    else
                    {
                        distances.Add(point.DistanceMeters); ceilings.Add(point.Unbounded ? 0f : point.SteeringCeilingMetersPerSecond);
                        unbounded.Add(point.Unbounded); curvatures.Add(curvature); limits.Add(limit);
                    }
                    count++;
                }
            }
            if (times != null) { times.Flatten += watch.Elapsed.TotalMilliseconds; watch.Restart(); }

            var knots = new List<int>(exact ? count : 4) { 0 };
            for (int i = 1; i < count; i++)
            {
                bool lastPoint = i == count - 1;
                if (distances[i] - distances[knots[knots.Count - 1]] >= SpeedPlan.MinimumKnotSpacingMeters) knots.Add(i);
                else if (lastPoint)
                {
                    if (knots.Count == 1) knots.Add(i);
                    else knots[knots.Count - 1] = i;
                }
            }
            if (knots.Count == 1) knots.Add(0);
            int n = knots.Count;
            var d = new double[n]; var ceiling = new double[n]; var ceilingUnbounded = new bool[n]; var curve = new double[n]; var road = new double[n];
            for (int k = 0; k < n; k++)
            {
                d[k] = distances[knots[k]];
                int from = Math.Max(0, (k == 0 ? knots[0] : knots[k - 1]) - 2);
                int to = Math.Min(count - 1, (k == n - 1 ? knots[n - 1] : knots[k + 1]) + 1);
                ceilingUnbounded[k] = true; ceiling[k] = 0d;
                double curvature = 0d;
                road[k] = double.PositiveInfinity;
                for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
                {
                    if (!unbounded[i] && (ceilingUnbounded[k] || ceilings[i] < ceiling[k])) { ceiling[k] = ceilings[i]; ceilingUnbounded[k] = false; }
                    curvature = Math.Max(curvature, curvatures[i]);
                    road[k] = Math.Min(road[k], limits[i]);
                }
                curve[k] = curvature > 0d ? Math.Sqrt(lateral / curvature) : double.PositiveInfinity;
            }
            if (times != null) { times.Knots += watch.Elapsed.TotalMilliseconds; watch.Restart(); }

            bool terminalStop = path.End == HorizonEnd.LookAheadLimit;
            double v0 = Math.Max(0d, currentSpeed);
            double acceleration = driver.MaxAcceleration * (double)SpeedPlan.PlanningBoundMargin;
            double planningDeceleration = driver.ComfortableDeceleration * (double)SpeedPlan.PlanningBoundMargin;
            bool unreachable = !CeilingReachable(d, ceiling, ceilingUnbounded, terminalStop, v0, planningDeceleration);
            if (unreachable) planningDeceleration = driver.SafeBrakingLimit * (double)SpeedPlan.PlanningBoundMargin;
            var cap = new double[n]; var capBinding = new SpeedConstraint[n];
            for (int k = 0; k < n; k++)
            {
                cap[k] = desired; capBinding[k] = SpeedConstraint.DesiredSpeed;
                if (road[k] < cap[k]) { cap[k] = road[k]; capBinding[k] = SpeedConstraint.RoadLimit; }
                if (!ceilingUnbounded[k] && ceiling[k] < cap[k]) { cap[k] = ceiling[k]; capBinding[k] = SpeedConstraint.SteeringCeiling; }
                if (curve[k] < cap[k]) { cap[k] = curve[k]; capBinding[k] = SpeedConstraint.CurveLimit; }
            }
            if (terminalStop) { cap[n - 1] = 0d; capBinding[n - 1] = SpeedConstraint.HorizonTerminalStop; }
            var backward = new double[n]; var backwardBinding = new SpeedConstraint[n]; var origin = new int[n];
            backward[n - 1] = cap[n - 1]; backwardBinding[n - 1] = capBinding[n - 1]; origin[n - 1] = n - 1;
            for (int k = n - 2; k >= 0; k--)
            {
                double reach = Math.Sqrt(backward[k + 1] * backward[k + 1] + 2d * planningDeceleration * (d[k + 1] - d[k]));
                if (reach < cap[k]) { backward[k] = reach; backwardBinding[k] = SpeedConstraint.AnticipatedDeceleration; origin[k] = origin[k + 1]; }
                else { backward[k] = cap[k]; backwardBinding[k] = capBinding[k]; origin[k] = k; }
            }
            var forward = new double[n]; var lower = new double[n]; var speed = new double[n]; var binding = new SpeedConstraint[n];
            forward[0] = Math.Min(backward[0], v0);
            lower[0] = v0;
            for (int k = 1; k < n; k++)
            {
                double step = d[k] - d[k - 1];
                forward[k] = Math.Min(backward[k], Math.Sqrt(forward[k - 1] * forward[k - 1] + 2d * acceleration * step));
                lower[k] = Math.Sqrt(Math.Max(0d, lower[k - 1] * lower[k - 1] - 2d * planningDeceleration * step));
            }
            for (int k = 0; k < n; k++)
            {
                if (lower[k] > forward[k]) { speed[k] = lower[k]; binding[k] = unreachable ? SpeedConstraint.SteeringCeilingUnreachable : SpeedConstraint.CurrentSpeedDeceleration; }
                else { speed[k] = forward[k]; binding[k] = forward[k] < backward[k] ? SpeedConstraint.MaxAcceleration : backwardBinding[k]; }
            }
            if (n == 2 && d[1] - d[0] < SpeedPlan.MinimumKnotSpacingMeters && speed[1] > speed[0]) speed[1] = speed[0];
            if (times != null) { times.Passes += watch.Elapsed.TotalMilliseconds; watch.Restart(); }

            int kept = n == 2 && (float)d[0] == (float)d[1] ? 1 : n;
            var profile = new SpeedProfilePoint[kept];
            for (int k = 0; k < kept; k++) profile[k] = new SpeedProfilePoint((float)d[k], (float)speed[k]);
            if (times != null) { times.Points += watch.Elapsed.TotalMilliseconds; watch.Restart(); }

            var bounds = new LongitudinalBounds(driver.MaxAcceleration, driver.SafeBrakingLimit);
            var result = new PlanResult { D = d, Speed = speed, Binding = binding, Deceleration = (float)planningDeceleration,
                PlanBinding = unreachable ? SpeedConstraint.SteeringCeilingUnreachable : binding[0], Limiting = capBinding[origin[0]] };
            if (exact)
            {
                float at;
                MotionDiagnostic diagnostics;
                result.Issue = VerifyMerged(motion, profile, bounds, out at, out diagnostics);
                result.IssueDistance = at; result.Diagnostics = diagnostics;
            }
            else
            {
                var verification = motion.VerifySpeedProfile(profile, bounds);
                result.Issue = verification.Issue; result.IssueDistance = verification.DistanceMeters; result.Diagnostics = verification.Diagnostics;
            }
            if (times != null) times.Verify += watch.Elapsed.TotalMilliseconds;
            return result;
        }

        private static bool CeilingReachable(double[] d, double[] ceiling, bool[] unbounded, bool terminalStop, double v0, double deceleration)
        {
            int n = d.Length;
            double next = double.PositiveInfinity;
            var chain = new double[n];
            for (int k = n - 1; k >= 0; k--)
            {
                double cap = unbounded[k] ? double.PositiveInfinity : ceiling[k];
                if (terminalStop && k == n - 1) cap = 0d;
                double reach = k == n - 1 ? double.PositiveInfinity : Math.Sqrt(next * next + 2d * deceleration * (d[k + 1] - d[k]));
                chain[k] = Math.Min(cap, reach);
                next = chain[k];
            }
            double lower = v0;
            for (int k = 0; k < n; k++)
            {
                if (k > 0) lower = Math.Sqrt(Math.Max(0d, lower * lower - 2d * deceleration * (d[k] - d[k - 1])));
                if (lower > chain[k]) return false;
            }
            return true;
        }

        /// <summary>
        /// MotionPlan.VerifySpeedProfile au bit pres (memes premieres defaillances, memes diagnostics) : les noeuds d'un
        /// intervalle sont la fusion de deux suites deja triees (points de l'intervalle, points du profil trouves par
        /// dichotomie) au lieu d'un parcours complet du profil puis d'un tri ; vitesse et plafond du noeud precedent reutilises.
        /// </summary>
        private static SpeedProfileIssue VerifyMerged(MotionPlan motion, SpeedProfilePoint[] candidate, LongitudinalBounds bounds,
            out float at, out MotionDiagnostic diagnostics)
        {
            var path = motion.Path;
            var drivability = Admission.Model.DrivabilityProfile;
            diagnostics = motion.Diagnostics;
            at = 0f;
            if (motion.Issue != MotionIssue.None) { at = motion.IssueDistanceMeters; return SpeedProfileIssue.PlanInfeasible; }
            if (candidate.Length < 2 || !bounds.Valid) return SpeedProfileIssue.InvalidSpeedProfile;
            for (int i = 0; i < candidate.Length; i++)
            {
                var p = candidate[i];
                if (float.IsNaN(p.DistanceMeters) || float.IsInfinity(p.DistanceMeters) || p.DistanceMeters < 0f || p.DistanceMeters > path.LengthMeters
                    || float.IsNaN(p.SpeedMetersPerSecond) || float.IsInfinity(p.SpeedMetersPerSecond) || p.SpeedMetersPerSecond < 0f
                    || (i > 0 && p.DistanceMeters <= candidate[i - 1].DistanceMeters))
                { at = p.DistanceMeters; return SpeedProfileIssue.InvalidSpeedProfile; }
            }
            if (candidate[0].DistanceMeters > PlanningTolerances.ProfileSpanToleranceMeters) return SpeedProfileIssue.InvalidSpeedProfile;
            if (candidate[candidate.Length - 1].DistanceMeters < path.LengthMeters - PlanningTolerances.ProfileSpanToleranceMeters)
            { at = candidate[candidate.Length - 1].DistanceMeters; return SpeedProfileIssue.InvalidSpeedProfile; }
            for (int i = 1; i < candidate.Length; i++)
            {
                var a = candidate[i - 1]; var b = candidate[i];
                double acceleration = ((double)b.SpeedMetersPerSecond * b.SpeedMetersPerSecond - (double)a.SpeedMetersPerSecond * a.SpeedMetersPerSecond)
                    / (2d * (b.DistanceMeters - a.DistanceMeters));
                if (acceleration > bounds.MaxAcceleration + PlanningTolerances.AccelerationBoundToleranceMetersPerSecondSquared
                    || acceleration < -(bounds.MaxDeceleration + PlanningTolerances.AccelerationBoundToleranceMetersPerSecondSquared))
                { at = b.DistanceMeters; return SpeedProfileIssue.AccelerationBoundExceeded; }
            }
            var knots = new List<float>();
            foreach (var interval in path.Intervals)
            {
                knots.Clear();
                float from = Math.Max(interval.StartDistanceMeters, candidate[0].DistanceMeters);
                float to = Math.Min(interval.EndDistanceMeters, candidate[candidate.Length - 1].DistanceMeters);
                if (to < from) continue;
                knots.Add(from);
                var points = interval.Points;
                int ip = 0;
                while (ip < points.Count && !(points[ip].DistanceMeters > from)) ip++;
                int ic = FirstAbove(candidate, from);
                while (true)
                {
                    bool hasP = ip < points.Count && points[ip].DistanceMeters < to;
                    bool hasC = ic < candidate.Length && candidate[ic].DistanceMeters < to;
                    if (!hasP && !hasC) break;
                    if (hasP && (!hasC || points[ip].DistanceMeters <= candidate[ic].DistanceMeters)) knots.Add(points[ip++].DistanceMeters);
                    else knots.Add(candidate[ic++].DistanceMeters);
                }
                if (to > from) knots.Add(to);
                float previousSpeed = 0f, previousCeiling = 0f, previousCurvature = 0f;
                for (int i = 0; i < knots.Count; i++)
                {
                    float s = knots[i];
                    float speed = SpeedAt(candidate, s);
                    float ceiling = CeilingAt(interval, s);
                    float curvature = CurvatureAt(interval, s);
                    if (speed > ceiling) { at = s; return SpeedProfileIssue.SteeringCeilingExceeded; }
                    if (speed > 0f && speed < drivability.SteeringInactiveBelowMetersPerSecond && Math.Abs(curvature) > 0f)
                        diagnostics |= MotionDiagnostic.SteeringInactiveSpan;
                    if (i > 0)
                    {
                        if (Math.Max(previousSpeed, speed) > 0f && Math.Min(previousSpeed, speed) < drivability.SteeringInactiveBelowMetersPerSecond
                            && (Math.Abs(previousCurvature) > 0f || Math.Abs(curvature) > 0f))
                            diagnostics |= MotionDiagnostic.SteeringInactiveSpan;
                        float minimumCeiling = Math.Min(previousCeiling, ceiling);
                        if (Math.Max(previousSpeed, speed) > minimumCeiling) { at = knots[i - 1]; return SpeedProfileIssue.SteeringCeilingExceeded; }
                    }
                    previousSpeed = speed; previousCeiling = ceiling; previousCurvature = curvature;
                }
            }
            return SpeedProfileIssue.None;
        }

        private static int FirstAbove(SpeedProfilePoint[] points, float s)
        {
            int low = 0, high = points.Length;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (points[mid].DistanceMeters > s) high = mid; else low = mid + 1;
            }
            return low;
        }

        private static int FirstAtOrAfter(SpeedProfilePoint[] points, float s)
        {
            int low = 1, high = points.Length - 1, found = -1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (s <= points[mid].DistanceMeters) { found = mid; high = mid - 1; } else low = mid + 1;
            }
            return found;
        }

        private static int FirstAtOrAfter(IReadOnlyList<PathPoint> points, float s)
        {
            int low = 1, high = points.Count - 1, found = -1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (s <= points[mid].DistanceMeters) { found = mid; high = mid - 1; } else low = mid + 1;
            }
            return found;
        }

        private static float SpeedAt(SpeedProfilePoint[] points, float s)
        {
            int i = FirstAtOrAfter(points, s);
            if (i > 0)
            {
                var a = points[i - 1]; var b = points[i];
                double t = (s - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                double v2 = a.SpeedMetersPerSecond * (double)a.SpeedMetersPerSecond * (1d - t) + b.SpeedMetersPerSecond * (double)b.SpeedMetersPerSecond * t;
                return (float)Math.Sqrt(Math.Max(0d, v2));
            }
            return points[points.Length - 1].SpeedMetersPerSecond;
        }

        private static float CeilingAt(PathInterval interval, float s)
        {
            int i = FirstAtOrAfter(interval.Points, s);
            if (i > 0) return Math.Min(interval.Points[i - 1].SteeringCeilingMetersPerSecond, interval.Points[i].SteeringCeilingMetersPerSecond);
            return interval.Points[interval.Points.Count - 1].SteeringCeilingMetersPerSecond;
        }

        private static float CurvatureAt(PathInterval interval, float s)
        {
            int i = FirstAtOrAfter(interval.Points, s);
            if (i > 0)
            {
                var a = interval.Points[i - 1]; var b = interval.Points[i];
                float t = (s - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                return a.Reference.CurvaturePerMeter + t * (b.Reference.CurvaturePerMeter - a.Reference.CurvaturePerMeter);
            }
            return interval.Points[interval.Points.Count - 1].Reference.CurvaturePerMeter;
        }

        private static bool SamePlan(SpeedPlan runtime, PlanResult result)
        {
            if (runtime.Points.Count != result.D.Length && !(runtime.Points.Count == 1 && result.D.Length == 2)) return false;
            for (int k = 0; k < runtime.Points.Count; k++)
                if (runtime.Points[k].DistanceMeters != (float)result.D[k] || runtime.Points[k].SpeedMetersPerSecond != (float)result.Speed[k]
                    || runtime.Points[k].Binding != result.Binding[k]) return false;
            return runtime.Binding == result.PlanBinding && runtime.LimitingConstraint == result.Limiting
                && runtime.PlanningDecelerationMetersPerSecondSquared == result.Deceleration
                && runtime.Verification.Issue == result.Issue && runtime.Verification.DistanceMeters == result.IssueDistance
                && runtime.Verification.Diagnostics == result.Diagnostics;
        }

        // ================================================================== occupation : replique et projection elaguee exacte

        private static Vector3[] Perimeter(VehicleFootprintPose pose)
        {
            var corners = TrafficFrame.Corners(pose);
            var points = new List<Vector3>();
            for (int edge = 0; edge < 4; edge++)
            {
                Vector3 a = corners[edge], b = corners[(edge + 1) % 4];
                int count = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(a, b) / TrafficFrame.OccupancySampleStepMeters));
                for (int k = 0; k < count; k++) points.Add(Vector3.Lerp(a, b, (float)k / count));
            }
            return points.ToArray();
        }

        private static float SqrDistanceToBox(Vector3 p, Vector3 min, Vector3 max)
        {
            float dx = Mathf.Max(0f, Mathf.Max(min.x - p.x, p.x - max.x));
            float dy = Mathf.Max(0f, Mathf.Max(min.y - p.y, p.y - max.y));
            float dz = Mathf.Max(0f, Mathf.Max(min.z - p.z, p.z - max.z));
            return dx * dx + dy * dy + dz * dz;
        }

        private static float SegmentSqr(ElementCache cache, int i, Vector3 point, out float u)
        {
            Vector3 a = cache.SegmentA[i], ab = cache.SegmentB[i] - a;
            float lengthSquared = ab.sqrMagnitude;
            u = lengthSquared > 1e-12f ? Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSquared) : 0f;
            return (point - (a + ab * u)).sqrMagnitude;
        }

        /// <summary>
        /// RoadCurve.Project(point) au bit pres : segments parcourus par abscisse croissante avec la meme comparaison
        /// stricte, mais un bloc ou un segment dont la boite est strictement plus loin (marge relative 1e-5) que la
        /// distance d'un segment amorce (pres de <paramref name="hintS"/>) est saute : il ne peut ni gagner ni egaler.
        /// </summary>
        private static RoadProjection ProjectPruned(ElementCache cache, Vector3 point, float hintS)
        {
            var samples = cache.Samples;
            int segments = samples.Count - 1;
            int seed = Mathf.Clamp(FirstAbove(samples, hintS) - 1, 0, segments - 1);
            float u;
            float threshold = SegmentSqr(cache, seed, point, out u);
            float limit = threshold * (1f + 1e-5f) + 1e-9f;
            float bestDistanceSquared = float.PositiveInfinity;
            float bestS = cache.Curve.StartS;
            for (int block = 0; block * BlockSize < segments; block++)
            {
                if (SqrDistanceToBox(point, cache.BlockMin[block], cache.BlockMax[block]) > limit) continue;
                for (int i = block * BlockSize; i < Math.Min(segments, (block + 1) * BlockSize); i++)
                {
                    if (SqrDistanceToBox(point, cache.SegmentMin[i], cache.SegmentMax[i]) > limit) continue;
                    float distanceSquared = SegmentSqr(cache, i, point, out u);
                    if (distanceSquared < bestDistanceSquared)
                    {
                        float s0 = samples[i].SMeters, span = samples[i + 1].SMeters - s0;
                        float tMin = Mathf.Max(0f, (cache.Curve.StartS - s0) / span);
                        float tMax = Mathf.Min(1f, (cache.Curve.Length - s0) / span);
                        bestDistanceSquared = distanceSquared;
                        bestS = s0 + (tMin + (tMax - tMin) * u) * span;
                    }
                }
            }
            float sMin = cache.Curve.StartS, sMax = cache.Curve.Length;
            bestS = Mathf.Clamp(bestS, sMin, sMax);
            var frame = cache.Curve.Sample(bestS);
            Vector3 offset = point - frame.Position;
            float along = Vector3.Dot(offset, frame.Tangent);
            float overrun = 0f;
            if (along < 0f && bestS <= sMin + BoundEpsilonMeters) overrun = -along;
            else if (along > 0f && bestS >= sMax - BoundEpsilonMeters) overrun = along;
            return new RoadProjection { SMeters = bestS, LateralOffsetMeters = Vector3.Dot(offset, frame.Right),
                NormalOffsetMeters = Vector3.Dot(offset, frame.Up), DistanceMeters = offset.magnitude, LongitudinalOverrunMeters = overrun,
                Point = frame };
        }

        // ================================================================== portee bornee

        /// <summary>
        /// Portee bornee : H = d1 + v_ref^2 / (2 b_plan) + m. d1 : distance du deuxieme noeud du plan (la vitesse visee est
        /// interpolee entre les noeuds 0 et 1) ; v_ref = max(v desiree, v courante) ; b_plan = deceleration de confort x
        /// marge de planification (le plus petit freinage de la passe arriere) ; m = <see cref="BoundedMarginMeters"/>.
        /// Toute contrainte au-dela de H, ou l'arret terminal en H, atteint le noeud 1 a sqrt(v_ref^2 + 2 b_plan m) > v_ref,
        /// donc ne peut lier ni la vitesse visee, ni l'atteignabilite du plafond (la deceleration depuis v courante s'annule
        /// avant H).
        /// </summary>
        private static float BoundedLookAhead(DriverProfile driver, float speed, float secondKnot)
        {
            double reference = Math.Max(driver.DesiredSpeed, speed);
            double deceleration = driver.ComfortableDeceleration * (double)SpeedPlan.PlanningBoundMargin;
            return (float)(secondKnot + reference * reference / (2d * deceleration) + BoundedMarginMeters);
        }

        private static RoadCurve CurveOf(PathInterval interval)
        {
            return CacheOf(interval.Id).Curve;
        }

        // ================================================================== banc

        [Test]
        public void ThePlanningCostIsMeasuredAndTheExactPrototypesMatchTheRuntimeToTheBit()
        {
            var model = Admission.Model;
            var driver = Driver;
            var evidence = Admission.Evidence;
            var seams = evidence.Valid && evidence.PoseModel == NominalPoseModel.Kinematic ? evidence.SignedRingSeams : null;
            var states = States();
            Assert.That(states.Count, Is.GreaterThan(300));
            // Caches precalcules hors mesure (ils appartiendraient au modele compile).
            foreach (var state in states)
                foreach (var occurrence in state.Route.Occurrences) CacheOf(occurrence.Id);

            var horizonRuntime = new Series(); var horizonSampling = new Series(); var horizonPoints = new Series();
            var horizonTransport = new Series(); var horizonSeams = new Series(); var horizonExact = new Series();
            var planRuntime = new Series(); var planExact = new Series();
            var planFlatten = new Series(); var planKnots = new Series(); var planPasses = new Series(); var planPoints = new Series(); var planVerify = new Series();
            var occupancyRuntime = new Series(); var occupancyExact = new Series();
            var boundedHorizon = new Series(); var boundedPlan = new Series(); var boundedExactHorizon = new Series(); var boundedExactPlan = new Series();
            var horizonBytes = new Series(); var planBytes = new Series(); var horizonExactBytes = new Series(); var planExactBytes = new Series();
            var boundedBytes = new Series(); var occupancyBytes = new Series();
            var lengths = new Series(); var pointCounts = new Series(); var knotCounts = new Series(); var boundedLengths = new Series();
            var boundedPointCounts = new Series();
            int horizonMismatches = 0, planMismatches = 0, projectionMismatches = 0, projections = 0;
            int commands = 0, commandMismatches = 0, boundedRefusals = 0;
            var commandDifferences = new List<string>();
            float lookAhead = TrafficV2Settings.LookAheadMeters;

            foreach (var state in states)
            {
                var route = state.Route;
                double e0 = state.Offset;
                var layout = Layout(route, lookAhead);
                lengths.Add(layout.Length);
                pointCounts.Add(layout.Points);

                PathHorizon runtimeHorizon = null;
                horizonRuntime.Add(Best(() => runtimeHorizon = PathHorizon.Build(model, route, lookAhead, state.Offset, seams)));
                horizonBytes.Add(Allocated(() => PathHorizon.Build(model, route, lookAhead, state.Offset, seams)));
                List<RoadCurvePoint[]> frames = null;
                horizonSampling.Add(Best(() => frames = SampleAll(layout)));
                List<List<HP>> points = null;
                horizonPoints.Add(Best(() => points = BuildPoints(layout, frames)));
                horizonTransport.Add(Best(() => Transport(layout, BuildPoints(layout, frames), e0)) - horizonPoints.Values[horizonPoints.Values.Count - 1]);
                Transport(layout, points, e0);
                horizonSeams.Add(Best(() => SlopesAndSeams(layout, points)));
                List<HP[]> exact = null;
                horizonExact.Add(Best(() => exact = HorizonExact(route, lookAhead, e0)));
                horizonExactBytes.Add(Allocated(() => HorizonExact(route, lookAhead, e0)));
                horizonMismatches += CompareHorizon(runtimeHorizon, exact);

                var motion = new MotionPlan(runtimeHorizon, model.DrivabilityProfile, evidence, TrackingTolerance.Undeclared);
                SpeedPlan runtimePlan = null;
                planRuntime.Add(Best(() => runtimePlan = SpeedPlan.Build(motion, model, driver, TimingSpeed)));
                planBytes.Add(Allocated(() => SpeedPlan.Build(motion, model, driver, TimingSpeed)));
                knotCounts.Add(runtimePlan.Points.Count);
                var times = new PlanTimes();
                Plan(motion, driver, TimingSpeed, false, null);
                for (int r = 0; r < Repeats; r++) Plan(motion, driver, TimingSpeed, false, times);
                planFlatten.Add(times.Flatten / Repeats); planKnots.Add(times.Knots / Repeats); planPasses.Add(times.Passes / Repeats);
                planPoints.Add(times.Points / Repeats); planVerify.Add(times.Verify / Repeats);
                PlanResult exactPlan = null;
                planExact.Add(Best(() => exactPlan = Plan(motion, driver, TimingSpeed, true, null)));
                planExactBytes.Add(Allocated(() => Plan(motion, driver, TimingSpeed, true, null)));
                if (!SamePlan(runtimePlan, exactPlan)) planMismatches++;

                // Occupation : ~130 projections du perimetre sur toute la courbe de l'element localise.
                var location = state.Frame.Actors[0].Location;
                if (location.Localized)
                {
                    var cache = CacheOf(location.ElementId);
                    var perimeter = Perimeter(state.Pose);
                    occupancyRuntime.Add(Best(() =>
                    {
                        float kappa = 0f;
                        for (int i = 0; i < cache.Samples.Count; i++) kappa = Mathf.Max(kappa, Mathf.Abs(cache.Samples[i].CurvaturePerMeter));
                        foreach (var p in perimeter) cache.Curve.Project(p);
                    }));
                    occupancyBytes.Add(Allocated(() => { foreach (var p in perimeter) cache.Curve.Project(p); }));
                    occupancyExact.Add(Best(() => { foreach (var p in perimeter) ProjectPruned(cache, p, location.SMeters); }));
                    foreach (var p in perimeter)
                    {
                        var full = cache.Curve.Project(p);
                        var pruned = ProjectPruned(cache, p, location.SMeters);
                        projections++;
                        if (full.SMeters != pruned.SMeters || full.DistanceMeters != pruned.DistanceMeters
                            || full.LongitudinalOverrunMeters != pruned.LongitudinalOverrunMeters || full.LateralOffsetMeters != pruned.LateralOffsetMeters)
                            projectionMismatches++;
                    }
                }

                // Portee bornee : cout et identite de la commande a plusieurs vitesses.
                var firstInterval = runtimeHorizon.Intervals[0];
                var curve = CurveOf(firstInterval);
                float heading = state.Lane.Track.NominalHeadingErrorDegrees(state.Piece, state.Distance);
                foreach (float speed in new[] { 0f, 2f, 4f, 6f, driver.DesiredSpeed, driver.DesiredSpeed * 1.1f })
                {
                    var fullPlan = SpeedPlan.Build(motion, model, driver, speed);
                    float secondKnot = fullPlan.Points.Count > 1 ? fullPlan.Points[1].DistanceMeters : 0f;
                    float bound = BoundedLookAhead(driver, speed, secondKnot);
                    var boundedHorizonValue = PathHorizon.Build(model, route, bound, state.Offset, seams);
                    var boundedMotion = new MotionPlan(boundedHorizonValue, model.DrivabilityProfile, evidence, TrackingTolerance.Undeclared);
                    var boundedPlanValue = SpeedPlan.Build(boundedMotion, model, driver, speed);
                    if (speed == TimingSpeed)
                    {
                        boundedLengths.Add(boundedHorizonValue.LengthMeters);
                        boundedPointCounts.Add(boundedHorizonValue.Intervals.Sum(i => i.Points.Count));
                        boundedHorizon.Add(Best(() => PathHorizon.Build(model, route, bound, state.Offset, seams)));
                        boundedPlan.Add(Best(() => SpeedPlan.Build(boundedMotion, model, driver, speed)));
                        boundedExactHorizon.Add(Best(() => HorizonExact(route, bound, e0)));
                        boundedExactPlan.Add(Best(() => Plan(boundedMotion, driver, speed, true, null)));
                        boundedBytes.Add(Allocated(() =>
                        {
                            var h = PathHorizon.Build(model, route, bound, state.Offset, seams);
                            SpeedPlan.Build(new MotionPlan(h, model.DrivabilityProfile, evidence, TrackingTolerance.Undeclared), model, driver, speed);
                        }));
                    }
                    if (!fullPlan.Accepted) continue;
                    if (!boundedPlanValue.Accepted) { boundedRefusals++; continue; }
                    commands++;
                    var full = MotionCommand.Track(9, 1, fullPlan, driver, model.DrivabilityProfile, curve, firstInterval.StartSMeters,
                        state.Pose.Position, state.Pose.Forward, speed, Dt, heading);
                    var bounded = MotionCommand.Track(9, 1, boundedPlanValue, driver, model.DrivabilityProfile, curve, firstInterval.StartSMeters,
                        state.Pose.Position, state.Pose.Forward, speed, Dt, heading);
                    float preview = Math.Max(speed * Dt, MotionCommand.PreviewFloorMeters);
                    bool same = full.TargetAccelerationMetersPerSecondSquared == bounded.TargetAccelerationMetersPerSecondSquared
                        && full.TargetWheelAngleDegrees == bounded.TargetWheelAngleDegrees
                        && fullPlan.SpeedAt(preview) == boundedPlanValue.SpeedAt(preview)
                        && fullPlan.Binding == boundedPlanValue.Binding && fullPlan.LimitingConstraint == boundedPlanValue.LimitingConstraint
                        && fullPlan.PlanningDecelerationMetersPerSecondSquared == boundedPlanValue.PlanningDecelerationMetersPerSecondSquared;
                    if (!same)
                    {
                        commandMismatches++;
                        if (commandDifferences.Count < 20)
                            commandDifferences.Add(state.Lane.Insertion.TrafficId + " d " + F(state.Distance) + " v " + F(speed) + " : a "
                                + F(full.TargetAccelerationMetersPerSecondSquared) + " / " + F(bounded.TargetAccelerationMetersPerSecondSquared)
                                + ", liante " + fullPlan.Binding + " / " + boundedPlanValue.Binding + ", limitante " + fullPlan.LimitingConstraint
                                + " / " + boundedPlanValue.LimitingConstraint + ", H " + F(bound));
                    }
                }
            }

            // ------------------------------------------------------------------ rapport
            double current = horizonRuntime.Mean + planRuntime.Mean;
            double exactTotal = horizonExact.Mean + planExact.Mean;
            double boundedTotal = boundedExactHorizon.Mean + boundedExactPlan.Mean;
            var text = new StringBuilder();
            text.Append("# Banc de cout du planning Traffic V2 (Story 5.33, D13 bis)\n\n");
            text.Append("Etats : ").Append(states.Count).Append(" poses nominales le long des 11 routes de `campaign-5-31.json` (pas ")
                .Append(F(StateSpacingMeters)).Append(" m et milieu de chaque morceau), e de la reference, v = ").Append(F(TimingSpeed))
                .Append(" m/s pour les temps. Temps : meilleur de ").Append(Repeats).Append(" executions apres chauffe (ms) ; allocations : GC.GetTotalMemory sans collection (octets). Editeur, EditMode.\n\n");
            text.Append("## Horizon actuel (portee ").Append(F(lookAhead)).Append(" m : toute la route restante)\n\n");
            text.Append("| Mesure | moyenne | p95 | max |\n|---|---|---|---|\n");
            Row(text, "longueur de l'horizon (m)", lengths);
            Row(text, "points de l'horizon", pointCounts);
            Row(text, "noeuds du plan de vitesse", knotCounts);
            Row(text, "PathHorizon.Build runtime (ms)", horizonRuntime);
            Row(text, "  echantillonnage curve.Sample (ms)", horizonSampling);
            Row(text, "  construction des points (ms)", horizonPoints);
            Row(text, "  transport de e + pose nominale (ms)", horizonTransport);
            Row(text, "  pentes et raccords (ms)", horizonSeams);
            Row(text, "  allocations (octets)", horizonBytes);
            text.Append("\n## Plan de vitesse actuel\n\n| Mesure | moyenne | p95 | max |\n|---|---|---|---|\n");
            Row(text, "SpeedPlan.Build runtime (ms)", planRuntime);
            Row(text, "  aplatissement des points (ms)", planFlatten);
            Row(text, "  noeuds et voisinages (ms)", planKnots);
            Row(text, "  passes (atteignabilite, plafonds, arriere, avant) (ms)", planPasses);
            Row(text, "  points et profil (ms)", planPoints);
            Row(text, "  verification du profil (ms)", planVerify);
            Row(text, "  allocations (octets)", planBytes);
            text.Append("\n## Occupation de la frame (perimetre ~130 points, projection sur tout l'element)\n\n| Mesure | moyenne | p95 | max |\n|---|---|---|---|\n");
            Row(text, "occupation actuelle (ms)", occupancyRuntime);
            Row(text, "occupation, projection elaguee exacte (ms)", occupancyExact);
            Row(text, "  allocations actuelles (octets)", occupancyBytes);
            text.Append("\nProjections comparees : ").Append(projections).Append(", differences au bit pres : ").Append(projectionMismatches).Append(".\n");
            text.Append("\n## Prototypes exacts (resultats compares au bit pres au runtime)\n\n| Mesure | moyenne | p95 | max |\n|---|---|---|---|\n");
            Row(text, "horizon exact optimise (ms)", horizonExact);
            Row(text, "  allocations (octets)", horizonExactBytes);
            Row(text, "plan de vitesse exact optimise (ms)", planExact);
            Row(text, "  allocations (octets)", planExactBytes);
            text.Append("\nDifferences au bit pres : horizon ").Append(horizonMismatches).Append(" points, plan de vitesse ").Append(planMismatches)
                .Append(" etats sur ").Append(states.Count).Append(".\n");
            text.Append("\n## Portee bornee H = d1 + v_ref^2 / (2 b_plan) + ").Append(F(BoundedMarginMeters)).Append(" m\n\n| Mesure | moyenne | p95 | max |\n|---|---|---|---|\n");
            Row(text, "longueur bornee (m)", boundedLengths);
            Row(text, "points de l'horizon borne", boundedPointCounts);
            Row(text, "horizon runtime borne (ms)", boundedHorizon);
            Row(text, "plan runtime borne (ms)", boundedPlan);
            Row(text, "horizon exact optimise borne (ms)", boundedExactHorizon);
            Row(text, "plan exact optimise borne (ms)", boundedExactPlan);
            Row(text, "allocations horizon + plan runtime bornes (octets)", boundedBytes);
            text.Append("\nCommandes comparees (6 vitesses par etat) : ").Append(commands).Append(", differences : ").Append(commandMismatches)
                .Append(", plans bornes refuses alors que le plan complet est accepte : ").Append(boundedRefusals).Append(".\n");
            foreach (var line in commandDifferences) text.Append("- ").Append(line).Append('\n');
            text.Append("\n## Comparaison horizon + plan de vitesse par vehicule et par pas (ms, moyenne)\n\n");
            text.Append("| Variante | horizon | plan | total | gain |\n|---|---|---|---|---|\n");
            text.Append("| actuel | ").Append(F(horizonRuntime.Mean)).Append(" | ").Append(F(planRuntime.Mean)).Append(" | ").Append(F(current)).Append(" | 1x |\n");
            text.Append("| exact optimise | ").Append(F(horizonExact.Mean)).Append(" | ").Append(F(planExact.Mean)).Append(" | ").Append(F(exactTotal))
                .Append(" | ").Append(F(current / exactTotal)).Append("x |\n");
            text.Append("| exact optimise + portee bornee | ").Append(F(boundedExactHorizon.Mean)).Append(" | ").Append(F(boundedExactPlan.Mean))
                .Append(" | ").Append(F(boundedTotal)).Append(" | ").Append(F(current / boundedTotal)).Append("x |\n");
            text.Append("\nOccupation : ").Append(F(occupancyRuntime.Mean)).Append(" -> ").Append(F(occupancyExact.Mean)).Append(" ms par vehicule (")
                .Append(F(occupancyRuntime.Mean / occupancyExact.Mean)).Append("x).\n");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, text.ToString());
            Debug.Log("[Story533] banc de cout du planning ecrit : " + ReportPath);

            Assert.That(horizonMismatches, Is.Zero, "le prototype d'horizon doit egaler le runtime au bit pres");
            Assert.That(planMismatches, Is.Zero, "le prototype de plan de vitesse doit egaler le runtime au bit pres");
            Assert.That(projectionMismatches, Is.Zero, "la projection elaguee doit egaler RoadCurve.Project au bit pres");
        }

        private static void Row(StringBuilder text, string label, Series series)
        {
            text.Append("| ").Append(label).Append(" | ").Append(F(series.Mean)).Append(" | ").Append(F(series.P95)).Append(" | ").Append(F(series.Max)).Append(" |\n");
        }
    }
}
