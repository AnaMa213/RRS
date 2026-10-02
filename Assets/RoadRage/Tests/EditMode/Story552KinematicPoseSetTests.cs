using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Migration;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.52 -- ensemble de poses nominales cinematiques (contrat Road World Model §8). Sur des droites, arcs,
    /// S, sauts de raccord et cycles synthetiques : les intervalles fermes contiennent l'ecart de toute route d'une
    /// enumeration exhaustive bornee, un cas non fermable echoue explicitement, l'union couvre l'echantillonnage
    /// dense, le residu d'anneau tourne egale sa forme fermee. Sur MVP_Run : fermeture, contenance des solutions
    /// des routes planifiees, et integrite de la preuve historique sur une geometrie actuelle distincte.
    /// </summary>
    [Category("Geometry")]
    [Category("Story552")]
    public sealed class Story552KinematicPoseSetTests
    {
        private const float A = 1.55f;
        private const string HistoricalSignedDirectory = "_bmad-output/implementation-artifacts/gate-a-5-52/historical-signed-5-51/";

        private static readonly RoadId E = new RoadId(10UL, 1UL);
        private static readonly RoadId M1 = new RoadId(10UL, 2UL);
        private static readonly RoadId R1 = new RoadId(10UL, 3UL);
        private static readonly RoadId R2 = new RoadId(10UL, 4UL);
        private static readonly RoadId R3 = new RoadId(10UL, 5UL);
        private static readonly RoadId R4 = new RoadId(10UL, 6UL);
        private static readonly RoadId M2 = new RoadId(10UL, 7UL);
        private static readonly RoadId X = new RoadId(10UL, 8UL);
        private static readonly RoadId S = new RoadId(10UL, 9UL);
        private static readonly RoadId U = new RoadId(10UL, 10UL);
        private static readonly RoadId C = new RoadId(10UL, 11UL);

        private static RoadModelValidationProfile Profile()
        {
            var profile = new RoadModelValidationProfile();
            profile.MaxVehicleHalfWidthMeters = 1.03f;
            profile.MaxVehicleLengthMeters = 4.5f;
            profile.LateralClearanceMarginMeters = 0.25f;
            return profile;
        }

        private static DrivabilityProfile Drivability()
        {
            return new DrivabilityProfile
            {
                Declared = true,
                WheelbaseMeters = 3.1f,
                ReferencePointAheadRearAxleMeters = A,
                LowSpeedLockDegrees = 40f,
                HighSpeedLockDegrees = 16f,
                FullReductionSpeedMetersPerSecond = 26f,
                SteeringInactiveBelowMetersPerSecond = 0.25f
            };
        }

        private static GateAEvidenceParameters Parameters()
        {
            return GateAEvidenceParameters.Create(NominalPoseModel.Kinematic, 0.34f, 0.008f, 0.002f, 256,
                new NominalPoseFeasibilityInputs(300f, 9.81f, 8f));
        }

        /// <summary>Arc (ou droite) de courbure constante, cap mesure de +z vers +x (courbure positive a droite).</summary>
        private static List<RoadCurveSample> Arc(ref Vector3 position, ref float heading, float curvature, float length, float step = 0.1f)
        {
            var samples = new List<RoadCurveSample>();
            int count = Mathf.Max(1, Mathf.CeilToInt(length / step));
            Vector3 start = position;
            float h0 = heading;
            for (int i = 0; i <= count; i++)
            {
                float s = length * i / count;
                float h = h0 + curvature * s;
                Vector3 p = Mathf.Abs(curvature) < 1e-9f
                    ? start + new Vector3(Mathf.Sin(h0), 0f, Mathf.Cos(h0)) * s
                    : start + new Vector3((Mathf.Cos(h0) - Mathf.Cos(h)) / curvature, 0f, (Mathf.Sin(h) - Mathf.Sin(h0)) / curvature);
                samples.Add(new RoadCurveSample
                {
                    SMeters = s,
                    Position = p,
                    Tangent = new Vector3(Mathf.Sin(h), 0f, Mathf.Cos(h)),
                    Up = Vector3.up,
                    CurvaturePerMeter = curvature,
                    HalfWidthLeftMeters = 1.75f,
                    HalfWidthRightMeters = 1.75f
                });
            }

            position = samples[samples.Count - 1].Position;
            heading = h0 + curvature * length;
            return samples;
        }

        /// <summary>Carrefour synthetique : entree, mouvement, anneau a quatre arcs (sauts de 1 deg, cycle), sortie, S, isole, croisement.</summary>
        private static SweepGraph Graph(out List<KeyValuePair<RoadId, float>> seeds)
        {
            var graph = new SweepGraph();
            var position = Vector3.zero;
            float heading = 0f;
            graph.Add(E, false, Arc(ref position, ref heading, 0f, 20f));
            graph.Add(M1, true, Arc(ref position, ref heading, 0.2f, 5f));
            var exitPosition = Vector3.zero;
            float exitHeading = 0f;
            foreach (var ring in new[] { R1, R2, R3, R4 })
            {
                heading += Mathf.Deg2Rad;
                graph.Add(ring, false, Arc(ref position, ref heading, 1f / 6f, Mathf.PI * 3f));
                if (ring == R2)
                {
                    exitPosition = position;
                    exitHeading = heading;
                }
            }

            // La sortie part de la fin de R2 (tangente continue) : seuls les raccords d'anneau portent un saut.
            position = exitPosition;
            heading = exitHeading;
            graph.Add(M2, true, Arc(ref position, ref heading, 0.2f, 5f));
            graph.Add(X, false, Arc(ref position, ref heading, 0f, 15f));
            var sCurve = Arc(ref position, ref heading, 0.15f, 5f);
            var second = Arc(ref position, ref heading, -0.15f, 5f);
            for (int i = 1; i < second.Count; i++)
            {
                var sample = second[i];
                sample.SMeters += 5f;
                sCurve.Add(sample);
            }

            graph.Add(S, false, sCurve);
            var isolated = new Vector3(100f, 0f, 0f);
            float isolatedHeading = 0f;
            graph.Add(U, false, Arc(ref isolated, ref isolatedHeading, 0f, 10f));
            var crossing = new Vector3(6.2f, 0f, 16f);
            float crossingHeading = 0f;
            graph.Add(C, false, Arc(ref crossing, ref crossingHeading, 0f, 14f));

            graph.Link(E, M1);
            graph.Link(M1, R1);
            graph.Link(R1, R2);
            graph.Link(R2, R3);
            graph.Link(R3, R4);
            graph.Link(R4, R1);
            graph.Link(R2, M2);
            graph.Link(M2, X);
            graph.Link(X, S);
            seeds = new List<KeyValuePair<RoadId, float>> { new KeyValuePair<RoadId, float>(E, 2f), new KeyValuePair<RoadId, float>(C, 0f) };
            return graph;
        }

        private static KinematicOffsetBounds Bounds(SweepGraph graph, List<KeyValuePair<RoadId, float>> seeds)
        {
            var bounds = KinematicOffsetBounds.Compute(graph, seeds, Drivability(), Parameters());
            Assert.That(bounds.Failures, Is.Empty);
            return bounds;
        }

        private static List<SweepPose> Poses(SweepGraph graph, RoadId id)
        {
            var poses = new List<SweepPose>();
            foreach (var sample in graph.Elements[id].Samples)
            {
                var pose = SweepPose.From(sample.Position, sample.Tangent);
                pose.ElementId = id;
                pose.SMeters = sample.SMeters;
                pose.Up = sample.Up;
                poses.Add(pose);
            }

            return poses;
        }

        private static SweepPose PoseAt(SweepGraph graph, RoadId id, float s, double offset)
        {
            RoadCurvePoint point = graph.Elements[id].Curve.Sample(s);
            var pose = SweepPose.From(point.Position, point.Tangent);
            pose.ElementId = id;
            pose.SMeters = s;
            pose.Up = point.Up;
            return KinematicPoseSet.WithHeading(pose, KinematicPoseSet.BodyHeading(pose, offset));
        }

        [Test]
        public void AStraightFromAPortalStaysAtZeroOffsetAndIsUnreachableBeforeIt()
        {
            List<KeyValuePair<RoadId, float>> seeds;
            var graph = Graph(out seeds);
            var bounds = Bounds(graph, seeds);
            double lo;
            double hi;
            Assert.That(bounds.TryHull(E, 10d, 10d, out lo, out hi), Is.True);
            Assert.That(Math.Max(Math.Abs(lo), Math.Abs(hi)), Is.LessThan(0.003d), "e = 0 au portail, jamais d'ecart en ligne droite.");
            Assert.That(bounds.TryHull(E, 0d, 1.5d, out lo, out hi), Is.False, "Avant le portail, aucune solution.");
            Assert.That(bounds.Unreachable, Does.Contain(U), "Ni portail ni predecesseur : inatteignable, sans intervalle.");
            Assert.That(bounds.TryHull(U, 0d, 10d, out lo, out hi), Is.False);

            PoseGrid[] grids;
            float[] rotations;
            Assert.That(KinematicPoseSet.Build(Poses(graph, U), bounds, 0.008f, out grids, out rotations), Is.Null);
            Assert.That(grids[3].Unreachable && grids[3].OffsetLo == 0d && grids[3].OffsetHi == 0d, Is.True,
                "Portion inatteignable balayee a e = 0 (sur-ensemble).");
        }

        [Test]
        public void TheClosedIntervalsContainEveryRouteOfABoundedEnumeration()
        {
            List<KeyValuePair<RoadId, float>> seeds;
            var graph = Graph(out seeds);
            var bounds = Bounds(graph, seeds);
            Assert.That(bounds.Converged, Is.True, "Iterations : " + bounds.Iterations);
            int checks = 0;
            Walk(graph, bounds, E, 2d, 0d, 12, ref checks);
            Walk(graph, bounds, C, 0d, 0d, 12, ref checks);
            Assert.That(checks, Is.GreaterThan(500), "Volume de l'enumeration (anneau parcouru plusieurs fois, deux portails).");

            // L'anneau converge vers le regime etabli asin(a kappa) sans jamais y etre remis.
            double lo;
            double hi;
            Assert.That(bounds.TryHull(R3, 8d, 8d, out lo, out hi), Is.True);
            double steady = Math.Asin(A / 6d);
            Assert.That(lo <= steady + 0.02d && hi >= steady - 0.02d, Is.True, "e sur l'anneau [" + lo + ", " + hi + "] vs " + steady);
        }

        private static void Walk(SweepGraph graph, KinematicOffsetBounds bounds, RoadId id, double start, double e0, int depth, ref int checks)
        {
            var element = graph.Elements[id];
            for (double s = start; s <= element.EndS; s += 0.25d)
            {
                double e = element.Curve.AdvanceKinematicOffset((float)start, (float)s, e0, A);
                double lo;
                double hi;
                Assert.That(bounds.TryHull(id, s, s, out lo, out hi), Is.True, id + " @ " + s);
                Assert.That(e >= lo && e <= hi, Is.True, "Route hors de l'enveloppe : " + id + " @ " + s + " e = " + e + " hors [" + lo + ", " + hi + "]");
                checks++;
            }

            if (depth == 0) return;
            double end = element.Curve.AdvanceKinematicOffset((float)start, element.EndS, e0, A);
            foreach (var next in element.Next)
            {
                double entry = end + RoadCurve.SignedTangentJumpRadians(element.Curve.Sample(element.EndS), next.Curve.Sample(next.StartS));
                var offsets = bounds.Elements[next.Id];
                Assert.That(offsets.HasEntry && entry >= offsets.EntryLo && entry <= offsets.EntryHi, Is.True,
                    "Entree de " + next.Id + " : " + entry + " hors I_X [" + offsets.EntryLo + ", " + offsets.EntryHi + "]");
                Walk(graph, bounds, next.Id, next.StartS, entry, depth - 1, ref checks);
            }
        }

        [Test]
        public void ANonClosableOffsetFailsExplicitly()
        {
            var graph = new SweepGraph();
            var position = Vector3.zero;
            float heading = 0f;
            graph.Add(E, false, Arc(ref position, ref heading, 0f, 10f));
            graph.Add(M1, true, Arc(ref position, ref heading, 1f, 6f));
            graph.Add(X, false, Arc(ref position, ref heading, 0f, 5f));
            graph.Link(E, M1);
            graph.Link(M1, X);
            var bounds = KinematicOffsetBounds.Compute(graph, new List<KeyValuePair<RoadId, float>> { new KeyValuePair<RoadId, float>(E, 1f) },
                Drivability(), Parameters());
            Assert.That(bounds.Closed, Is.False);
            Assert.That(bounds.Failures[0], Does.StartWith(KinematicOffsetBounds.NotClosedCode), "Rayon 1 m < a : |e| franchit 90 deg.");
        }

        [Test]
        public void TheKinematicConflictSweepCoversTheDenseUnionOverOffsetsAndProgress()
        {
            List<KeyValuePair<RoadId, float>> seeds;
            var graph = Graph(out seeds);
            var bounds = Bounds(graph, seeds);
            var profile = Profile();
            var parameters = Parameters();
            var sweep = ConflictSweep.EvaluateKinematic(new List<List<SweepPose>> { Poses(graph, M1) },
                new List<List<SweepPose>> { Poses(graph, C) }, profile, parameters, bounds);
            Assert.That(sweep.Relation, Is.Not.EqualTo(PairRelation.FailClosed), sweep.FailClosedReason);
            Assert.That(float.IsInfinity(sweep.ExactSlackMeters), Is.False, "Le cas doit passer le filtre englobant.");

            float halfLength = ConflictSweep.HalfLength(profile);
            float halfWidth = profile.MaxVehicleHalfWidthMeters;
            float dense = float.PositiveInfinity;
            foreach (var poseA in DensePoses(graph, bounds, M1, 0.02f, 8))
            {
                foreach (var poseB in DensePoses(graph, bounds, C, 0.05f, 3))
                {
                    dense = Mathf.Min(dense, ConflictSweep.RectangleDistance(poseA, poseB, halfLength, halfWidth) - 2f * sweep.BaseInflationMeters);
                }
            }

            Assert.That(sweep.ExactSlackMeters, Is.LessThanOrEqualTo(dense + 1e-4f), "La preuve couvre toute pose atteignable.");
            Assert.That(sweep.ExactSlackMeters, Is.GreaterThan(dense - 1f), "Borne non vide de sens.");
        }

        [Test]
        public void TheKinematicClearanceCoversTheDenseUnionAndLocalizesItsWitness()
        {
            List<KeyValuePair<RoadId, float>> seeds;
            var graph = Graph(out seeds);
            var bounds = Bounds(graph, seeds);
            var profile = Profile();
            var parameters = Parameters();
            float halfLength = JunctionClearance.HalfLength(profile, parameters.TrackingAllowanceMeters);
            float halfWidth = JunctionClearance.HalfWidth(profile, parameters.TrackingAllowanceMeters);
            var poses = Poses(graph, M1);
            PoseGrid[] grids;
            float[] rotations;
            Assert.That(KinematicPoseSet.Build(poses, bounds, parameters.OffsetGridStepRadians, out grids, out rotations), Is.Null);

            // Un obstacle separe (residu positif, comparaison discriminante) et un obstacle recouvrant (deficit localise).
            foreach (var obstacle in new[] { Rect(6.5f, 7.5f, 22f, 24f), Rect(1.5f, 2.5f, 22f, 23f) })
            {
                var witness = JunctionClearance.MeasurePathKinematic(poses, grids, rotations, new[] { obstacle }, halfLength, halfWidth,
                    parameters.OffsetGridStepRadians, "obstacle");
                float dense = float.PositiveInfinity;
                foreach (var pose in DensePoses(graph, bounds, M1, 0.01f, 10))
                {
                    dense = Mathf.Min(dense, JunctionClearance.SignedDistance(ConflictSweep.Corners(pose, halfLength, halfWidth), obstacle));
                }

                Assert.That(witness.Residual, Is.LessThanOrEqualTo(dense + 1e-4f), "La preuve couvre toute pose atteignable.");
                Assert.That(witness.Residual, Is.GreaterThan(dense - 0.5f), "Borne non vide de sens.");
                Assert.That(witness.ElementId, Is.EqualTo(M1), "Deficit localise : element du temoin.");
                Assert.That(witness.SMeters >= 0f && witness.SMeters <= 5f && witness.OffsetLoRadians <= witness.OffsetHiRadians, Is.True);
                Assert.That(witness.GridRemainder, Is.EqualTo(Mathf.Sqrt(halfLength * halfLength + halfWidth * halfWidth) * 0.004f).Within(1e-6f));
            }
        }

        private static IEnumerable<SweepPose> DensePoses(SweepGraph graph, KinematicOffsetBounds bounds, RoadId id, float step, int offsets)
        {
            var element = graph.Elements[id];
            for (float s = element.StartS; s <= element.EndS + 1e-4f; s += step)
            {
                double lo;
                double hi;
                if (!bounds.TryHull(id, s, s, out lo, out hi)) continue;
                for (int k = 0; k <= offsets; k++)
                {
                    yield return PoseAt(graph, id, s, lo + (hi - lo) * k / offsets);
                }
            }
        }

        private static Vector2[] Rect(float xMin, float xMax, float zMin, float zMax)
        {
            return new[] { new Vector2(xMin, zMin), new Vector2(xMax, zMin), new Vector2(xMax, zMax), new Vector2(xMin, zMax) };
        }

        [Test]
        public void TheRotatedRingResidualMatchesItsClosedForm()
        {
            const double halfLength = 2.25d;
            const double halfWidth = 1.03d;
            foreach (double radius in new[] { 5d, 7.5d })
            {
                foreach (double degrees in new[] { 0d, 10d, 25d, 40d })
                {
                    double e = degrees * Math.PI / 180d;
                    double min;
                    double max;
                    BruteForce(radius, e, halfLength, halfWidth, out min, out max);
                    Assert.That(RoundaboutClearance.OutermostCorner(radius, e, halfLength, halfWidth), Is.EqualTo(max).Within(1e-4d));
                }
            }

            foreach (double clear in new[] { 4d, 6d })
            {
                foreach (double degrees in new[] { 0d, 15d, 30d, 45d })
                {
                    double e = degrees * Math.PI / 180d;
                    double centre = RoundaboutClearance.CentreForInnermost(clear, e, halfLength, halfWidth);
                    double min;
                    double max;
                    BruteForce(centre, e, halfLength, halfWidth, out min, out max);
                    Assert.That(min, Is.EqualTo(clear).Within(1e-3d), "Bord (ou coin) interieur a la distance voulue, e = " + degrees + " deg.");
                }
            }

            var profile = Profile();
            float previous = float.PositiveInfinity;
            for (int degrees = 0; degrees <= 40; degrees += 5)
            {
                float e = degrees * Mathf.Deg2Rad;
                float residual = RoundaboutClearance.Residual(4f, 13f, profile, 0.34f, e);
                Assert.That(residual, Is.LessThanOrEqualTo(previous), "Le residu decroit avec |e|.");
                previous = residual;
                Assert.That(residual, Is.EqualTo(BruteForceResidual(4d, 13d, profile, 0.34d, e)).Within(2e-3d), "e = " + degrees + " deg.");
            }
        }

        [Test]
        public void TheRingEnvelopeIncludesTheInteriorMaximumForAWideFootprint()
        {
            var square = Profile();
            square.MaxVehicleLengthMeters = 2f;
            square.MaxVehicleHalfWidthMeters = 1f;
            square.LateralClearanceMarginMeters = 0f;
            float maximum = 60f * Mathf.Deg2Rad;
            Assert.That(RoundaboutClearance.Residual(100f, 105.5f, square, 0f, maximum), Is.GreaterThan(0f),
                "L'extreme seul manquerait le deficit a 45 deg.");
            float worst = RoundaboutClearance.WorstResidual(100f, 105.5f, square, 0f, maximum);
            Assert.That(worst, Is.EqualTo(105.5d - (100d + 4d * Math.Sqrt(2d))).Within(1e-5d));
            Assert.That(worst, Is.LessThan(0f));
            foreach (var profile in new[] { square, Profile() })
                foreach (var maximumDegrees in new[] { 15f, 40f, 60f, 89f })
                {
                    float bound = RoundaboutClearance.WorstResidual(100f, 110f, profile, 0.34f, maximumDegrees * Mathf.Deg2Rad);
                    float dense = float.PositiveInfinity;
                    for (int i = 0; i <= 900; i++)
                        dense = Mathf.Min(dense, RoundaboutClearance.Residual(100f, 110f, profile, 0.34f,
                            maximumDegrees * Mathf.Deg2Rad * i / 900f));
                    Assert.That(bound, Is.LessThanOrEqualTo(dense + 1e-5f), "Toute pose intermediaire est couverte.");
                    Assert.That(bound, Is.EqualTo(dense).Within(1e-4f), "Le maximum interieur est inclus sans marge arbitraire.");
                }
        }

        private static void BruteForce(double radius, double e, double halfLength, double halfWidth, out double min, out double max)
        {
            // Centre (R, 0), tangente +y tournee de e : avant (sin e, cos e), lateral (cos e, -sin e).
            min = double.PositiveInfinity;
            max = 0d;
            const int steps = 2000;
            for (int side = 0; side < 4; side++)
            {
                for (int k = 0; k <= steps; k++)
                {
                    double t = -1d + 2d * k / steps;
                    double u = side < 2 ? (side == 0 ? 1d : -1d) * halfLength : t * halfLength;
                    double v = side < 2 ? t * halfWidth : (side == 2 ? 1d : -1d) * halfWidth;
                    double x = radius + u * Math.Sin(e) + v * Math.Cos(e);
                    double y = u * Math.Cos(e) - v * Math.Sin(e);
                    double distance = Math.Sqrt(x * x + y * y);
                    min = Math.Min(min, distance);
                    max = Math.Max(max, distance);
                }
            }
        }

        private static double BruteForceResidual(double inner, double outer, RoadModelValidationProfile profile, double allowance, double e)
        {
            double halfLength = 0.5d * profile.MaxVehicleLengthMeters;
            double halfWidth = profile.MaxVehicleHalfWidthMeters;
            double margin = profile.LateralClearanceMarginMeters + allowance;
            double innerCentre = Bisect(inner + margin, e, halfLength, halfWidth);
            double min;
            double innerCorner;
            BruteForce(innerCentre, e, halfLength, halfWidth, out min, out innerCorner);
            double outerCentre = Bisect(innerCorner + 2d * margin, e, halfLength, halfWidth);
            double outerCorner;
            BruteForce(outerCentre, e, halfLength, halfWidth, out min, out outerCorner);
            return outer - margin - outerCorner;
        }

        private static double Bisect(double clear, double e, double halfLength, double halfWidth)
        {
            double low = clear;
            double high = clear + 10d;
            for (int i = 0; i < 60; i++)
            {
                double mid = 0.5d * (low + high);
                double min;
                double max;
                BruteForce(mid, e, halfLength, halfWidth, out min, out max);
                if (min < clear) low = mid; else high = mid;
            }

            return high;
        }

        [TestCase(0f)]
        [TestCase(0.25f)]
        public void TheFeasibilityCheckFlagsAPoseBeyondTheLowSpeedLock(float inactiveThreshold)
        {
            var drivability = Drivability();
            drivability.SteeringInactiveBelowMetersPerSecond = inactiveThreshold;
            foreach (var tight in new[] { true, false })
            {
                var graph = new SweepGraph();
                var position = Vector3.zero;
                float heading = 0f;
                graph.Add(E, false, Arc(ref position, ref heading, 0f, 5f));
                graph.Add(M1, true, Arc(ref position, ref heading, tight ? 0.3f : 0.05f, 12f));
                graph.Link(E, M1);
                var bounds = KinematicOffsetBounds.Compute(graph, new List<KeyValuePair<RoadId, float>> { new KeyValuePair<RoadId, float>(E, 0f) },
                    drivability, Parameters());
                Assert.That(bounds.Failures, Is.Empty);
                bounds.CheckFeasibility(drivability, Parameters());
                if (tight)
                {
                    Assert.That(bounds.Infeasible.Count, Is.EqualTo(1));
                    StringAssert.StartsWith(KinematicOffsetBounds.InfeasibleCode, bounds.Infeasible[0]);
                    StringAssert.Contains(M1.ToString(), bounds.Infeasible[0], "e* = asin(a kappa) = 27,7 deg : delta_N 46 deg > 40 deg.");
                }
                else
                {
                    Assert.That(bounds.Infeasible, Is.Empty, "Arc doux : braquage et taux dans le profil.");
                    Assert.That(bounds.MinimumSteerRateMarginDegreesPerSecond, Is.GreaterThan(0d));
                }
            }
        }

        // ============================================================ MVP_Run

        [Test]
        public void TheMvpRunBoundsCloseAndContainThePlannedRouteSolutions()
        {
            string modelText = File.ReadAllText(TrafficV2Settings.ModelPath);
            var admission = TrafficV2Lifecycle.Admit(modelText, File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
            Assert.That(admission.Code, Is.EqualTo(TrafficV2Code.Allowed), "La nouvelle signature doit lier la preuve cinematique courante.");
            Assert.That(admission.Evidence.PoseModel, Is.EqualTo(NominalPoseModel.Kinematic));
            Assert.That(admission.Evidence.TrackingAllowanceMeters, Is.EqualTo(0.34f));
            Assert.That(admission.Evidence.SignedRingSeams.Count, Is.EqualTo(24));
            var model = admission.Model;
            Assert.That(model, Is.Not.Null);
            var bounds = KinematicOffsetBounds.Compute(model, SweepGraph.FromModel(model), GateAEvidenceParameters.Declared());
            Assert.That(bounds.Failures, Is.Empty, "Fermeture des intervalles sur MVP_Run.");
            Assert.That(bounds.Unreachable, Is.Empty, "Chaque element est atteignable depuis un portail d'entree.");

            int routes = 0;
            int checks = 0;
            ulong counter = 1UL;
            foreach (var entry in model.Portals)
            {
                if (entry.Role != PortalRole.Entry) continue;
                Portal portal; Vector3 position; Quaternion rotation;
                Assert.That(TrafficV2Lifecycle.TryPortalPose(model, entry.Id, out portal, out position, out rotation), Is.True);
                var location = RoadLocalizer.Localize(model, new VehicleFootprintPose { Position = position,
                    Forward = rotation * Vector3.forward, Up = rotation * Vector3.up }, entry.CorridorId, null);
                foreach (var exit in model.Portals)
                {
                    if (exit.Role != PortalRole.Exit) continue;
                    var route = RoutePlanner.Plan(new RouteRequest(model, location, exit.Id, new RouteSeed(0UL),
                        TrafficV2Lifecycle.TrafficIdentity(0UL, counter++), "route", new DecisionCounter(0UL))).Plan;
                    if (route == null) continue;
                    routes++;
                    var track = ReferenceTrack.FromRoute(model, route.Occurrences, 0f);
                    for (int p = 0; p < track.Pieces.Count; p++)
                    {
                        var piece = track.Pieces[p];
                        for (float d = piece.StartDistanceMeters; d <= piece.EndDistanceMeters; d += 0.1f)
                        {
                            double e = track.OffsetRadians(p, d);
                            double lo;
                            double hi;
                            float s = piece.ElementS(d);
                            Assert.That(bounds.TryHull(piece.Id, s, s, out lo, out hi), Is.True, piece.Id + " @ " + s);
                            Assert.That(e >= lo && e <= hi, Is.True, "Solution de route " + entry.Label + " -> " + exit.Label + " hors de l'enveloppe : "
                                + piece.Id + " @ " + s + " m, e = " + e + " hors [" + lo + ", " + hi + "].");
                            checks++;
                        }
                    }
                }
            }

            Assert.That(routes, Is.GreaterThan(10), "Routes planifiees entree -> sortie.");
            Debug.Log("[Story552] MVP_Run : " + bounds.Iterations + " iteration(s), " + routes + " route(s), " + checks + " controle(s) de contenance.");
        }

        [Serializable]
        private sealed class SignedHashes
        {
            public string DecisionsHash;
        }

        [Test]
        public void HistoricalEvidenceRemainsIntactAndLegacyRejectsChangedGeometry()
        {
            // SHA-256 des octets de la baseline signee 5845dc54f70818097b302935e1f4669df7b38414.
            Assert.That(HistoricalSha256(HistoricalSignedDirectory + "MVP_Run.road-model.json"), Is.EqualTo("6525a2366641aeb6c28595259a2762abb4e881ff1c32b2e2f8de16e5e88c1ebb"));
            Assert.That(HistoricalSha256(HistoricalSignedDirectory + "overlay-5-28-mvp-run.txt"), Is.EqualTo("20cd1d1f262f812d6fd035119a465a20dcbd4b39c18a58410f481f71d00a0abe"));
            Assert.That(HistoricalSha256(HistoricalSignedDirectory + "migration-report-5-28-mvp-run.md"), Is.EqualTo("7b97335902b6e6cfcb59c201bef73e92ecc0c5900b3b4dcd30f498eb909fdbad"));
            Assert.That(HistoricalSha256(HistoricalSignedDirectory + "MVP_Run.road-signoff.json"), Is.EqualTo("e2d77b994d6e7e28aefb86ca0fff3d3d35d2eccf7cc15dff2e3097d3f5a2973e"));

            var signed = JsonUtility.FromJson<SignedHashes>(File.ReadAllText(AuthoredRoadModel.FullPath(HistoricalSignedDirectory + "MVP_Run.road-signoff.json")));
            string decisionsText = File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath));
            Assert.That(V1SourceSet.Sha256Hex(decisionsText), Is.Not.EqualTo(signed.DecisionsHash),
                "La largeur des anneaux a change depuis la signature historique.");

            // Legacy restaure les parametres et le comportement historique de la preuve, pas l'etat
            // historique de la scene, des prefabs ou de l'authoring.
            WithMvpRun(scene =>
            {
                var run = AuthoredRoadModel.Run(V1SourceSet.Extract(scene), File.ReadAllText(MigrationReport.LineageFullPath),
                    decisionsText, GateAEvidenceParameters.Legacy);
                Assert.That(run.EvidenceParameters.IsLegacy, Is.True);
                Assert.That(run.CandidateModel, Is.Not.Null, "Les candidats restent reconstructibles.");
                Assert.That(run.Candidates, Is.Not.Empty);
                Assert.That(run.Failures, Is.Not.Empty, "Les anciennes empreintes ne doivent pas etre reconfirmees.");
                bool fingerprintChanged = false;
                foreach (string failure in run.Failures)
                {
                    Assert.That(PairReview.IsReconciliationFailure(failure), Is.True, failure);
                    if (failure.StartsWith("Proposition historique non reconfirmee : ", StringComparison.Ordinal))
                        fingerprintChanged = true;
                }
                Assert.That(fingerprintChanged, Is.True, "Le fingerprint historique doit etre explicitement perime.");
                Assert.That(run.Compiled, Is.Null, "Aucun modele avec decisions historiques reutilisees.");
                Assert.That(run.Binding, Is.Null, "Aucun nouveau binding signe.");
            });
        }

        private static string HistoricalSha256(string path)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(AuthoredRoadModel.FullPath(path)))).Replace("-", "").ToLowerInvariant();
        }

        private static void WithMvpRun(Action<Scene> body)
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
            bool inHierarchy = alreadyOpen.IsValid();
            bool wasOpen = inHierarchy && alreadyOpen.isLoaded;
            if (wasOpen)
            {
                Assert.That(alreadyOpen.isDirty, Is.False, "MVP_Run est ouvert avec des modifications non sauvegardees.");
            }

            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(MigrationReport.ScenePath, OpenSceneMode.Additive);
            try
            {
                body(scene);
            }
            finally
            {
                if (!wasOpen) EditorSceneManager.CloseScene(scene, !inHierarchy);
            }
        }
    }
}
