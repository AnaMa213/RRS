using System;
using System.Collections.Generic;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.50, P1 -- balayage conservateur des candidats de conflit, sur trajectoires
    /// synthetiques. Chaque cas adversarial est construit pour que le contact n'existe qu'entre deux
    /// poses evaluees ; un controle aux seules poses le manque (preuve que le test discrimine) et le
    /// balayage doit le trouver. La reference est une evaluation dense de l'interpolation canonique
    /// (position lineaire, cap par interpolation lineaire normalisee).
    /// </summary>
    [Category("Geometry")]
    public sealed class Story550ConflictSweepTests
    {
        private static RoadModelValidationProfile Profile()
        {
            var profile = new RoadModelValidationProfile();
            profile.MaxVehicleHalfWidthMeters = 1.03f;
            profile.MaxVehicleLengthMeters = 4.5f;
            profile.LateralClearanceMarginMeters = 0.25f;
            return profile;
        }

        private static float HalfLength
        {
            get { return ConflictSweep.HalfLength(Profile()); }
        }

        private static float HalfWidth
        {
            get { return Profile().MaxVehicleHalfWidthMeters; }
        }

        private static float ContactDistance
        {
            get { return 2f * ConflictSweep.Inflation(Profile()); }
        }

        private static SweepPose Pose(float x, float z, float headingDegrees)
        {
            float radians = headingDegrees * Mathf.Deg2Rad;
            return SweepPose.From(new Vector3(x, 0f, z), new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)));
        }

        private static List<SweepPose> Line(float x0, float z0, float x1, float z1, float step)
        {
            var poses = new List<SweepPose>();
            float length = new Vector2(x1 - x0, z1 - z0).magnitude;
            int count = Mathf.Max(1, Mathf.CeilToInt(length / step));
            float heading = Mathf.Atan2(z1 - z0, x1 - x0) * Mathf.Rad2Deg;
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                poses.Add(Pose(Mathf.Lerp(x0, x1, t), Mathf.Lerp(z0, z1, t), heading));
            }

            return poses;
        }

        private static List<List<SweepPose>> One(List<SweepPose> path)
        {
            return new List<List<SweepPose>> { path };
        }

        private static PairSweep Sweep(List<SweepPose> a, List<SweepPose> b)
        {
            return ConflictSweep.Evaluate(One(a), One(b), Profile());
        }

        /// <summary>Controle naif : contact seulement si deux poses evaluees se touchent.</summary>
        private static bool PoseOnlyContact(List<SweepPose> a, List<SweepPose> b)
        {
            foreach (var pa in a)
            {
                foreach (var pb in b)
                {
                    if (ConflictSweep.RectangleDistance(pa, pb, HalfLength, HalfWidth) <= ContactDistance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Poses intermediaires de l'interpolation canonique, a pas fin.</summary>
        private static List<SweepPose> Dense(List<SweepPose> path, int steps)
        {
            if (path.Count == 1)
            {
                return new List<SweepPose>(path);
            }

            var dense = new List<SweepPose>();
            for (int i = 1; i < path.Count; i++)
            {
                SweepPose a = path[i - 1];
                SweepPose b = path[i];
                for (int k = 0; k <= steps; k++)
                {
                    float t = (float)k / steps;
                    Vector2 heading = Vector2.Lerp(a.Heading, b.Heading, t);
                    Vector3 position = Vector3.Lerp(a.Position, b.Position, t);
                    dense.Add(SweepPose.From(position, new Vector3(heading.x, 0f, heading.y)));
                }
            }

            return dense;
        }

        private static float DenseMinimum(List<SweepPose> a, List<SweepPose> b)
        {
            var da = Dense(a, 64);
            var db = Dense(b, 64);
            float best = float.PositiveInfinity;
            foreach (var pa in da)
            {
                foreach (var pb in db)
                {
                    best = Math.Min(best, ConflictSweep.RectangleDistance(pa, pb, HalfLength, HalfWidth));
                }
            }

            return best;
        }

        // ============================================================ cas adversariaux P1

        [Test]
        public void ATranslationCrossingBetweenTwoPosesTenMetresApartIsFound()
        {
            var a = new List<SweepPose> { Pose(0f, 0f, 0f), Pose(10f, 0f, 0f) };
            var b = new List<SweepPose> { Pose(5f, -5f, 90f), Pose(5f, 5f, 90f) };

            Assert.That(PoseOnlyContact(a, b), Is.False, "Le cas doit echapper a un controle aux seules poses.");
            Assert.That(DenseMinimum(a, b), Is.LessThanOrEqualTo(ContactDistance), "Les trajectoires reelles se croisent.");
            PairSweep sweep = Sweep(a, b);
            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.Candidate));
            Assert.That(sweep.IsCandidate, Is.True);
            Assert.That(sweep.ExactProven, Is.True);
            Assert.That(sweep.HasVolume, Is.True);
            Assert.That(Contains(sweep.Volume, new Vector3(5f, 0f, 0f)), Is.True, "Le volume couvre le point de croisement.");
        }

        [Test]
        public void ACornerTouchingOnlyDuringAnIntervalRotationIsFound()
        {
            // Pivot sur place de 0 a 80 deg ; l'obstacle mobile (une pose) est place par recherche
            // pour ne toucher que pendant la rotation.
            var a = new List<SweepPose> { Pose(0f, 0f, 0f), Pose(0f, 0f, 80f) };
            List<SweepPose> b = null;
            for (float radius = 2.5f; radius < 8f && b == null; radius += 0.01f)
            {
                float angle = 80f * Mathf.Deg2Rad;
                var candidate = new List<SweepPose> { Pose(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 170f) };
                if (!PoseOnlyContact(a, candidate) && DenseMinimum(a, candidate) <= ContactDistance)
                {
                    b = candidate;
                }
            }

            Assert.That(b, Is.Not.Null, "Un placement ne touchant qu'en cours de rotation doit exister.");
            PairSweep sweep = Sweep(a, b);
            Assert.That(sweep.IsCandidate, Is.True);
            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.Candidate));
        }

        [Test]
        public void AContactOnlyWithinASeamIntervalIsFound()
        {
            // Mouvement M (x -10 -> 0) puis corridor K apres une couture (x 10 -> 20). La couture est
            // ici volontairement large pour isoler le mecanisme : elle forme un intervalle balaye.
            var graph = new SweepGraph();
            RoadId m = RoadId.New();
            RoadId k = RoadId.New();
            graph.Add(m, true, Samples(-10f, 0f, 1f));
            graph.Add(k, false, Samples(10f, 20f, 1f));
            graph.Link(m, k);
            string failure;
            var paths = ConflictSweep.Paths(graph, m, 12f, out failure);
            Assert.That(failure, Is.Null);
            Assert.That(paths, Has.Count.EqualTo(1));

            var b = Line(5f, -6f, 5f, 6f, 12f);
            Assert.That(PoseOnlyContact(paths[0], b), Is.False);
            Assert.That(DenseMinimum(paths[0], b), Is.LessThanOrEqualTo(ContactDistance));
            PairSweep sweep = ConflictSweep.Evaluate(paths, One(b), Profile());
            Assert.That(sweep.IsCandidate, Is.True, "L'intervalle de couture est balaye.");
        }

        [Test]
        public void AShortElementIsTraversedAndItsSuccessorReached()
        {
            // M1 (x -10 -> 0), corridor court K (1 m), corridor N (x 1 -> 20). La trajectoire de B
            // coupe N a x = 4,5 : hors de portee de la seule empreinte de fin de M1, borne d'intervalle comprise.
            var graph = new SweepGraph();
            RoadId m1 = RoadId.New();
            RoadId k = RoadId.New();
            RoadId n = RoadId.New();
            graph.Add(m1, true, Samples(-10f, 0f, 0.5f));
            graph.Add(k, false, Samples(0f, 1f, 0.5f));
            graph.Add(n, false, Samples(1f, 20f, 0.5f));
            graph.Link(m1, k);
            graph.Link(k, n);
            var b = Line(4.5f, -10f, 4.5f, 10f, 0.5f);

            var own = One(ConflictSweep.Poses(graph.Elements[m1].Samples));
            Assert.That(ConflictSweep.Evaluate(own, One(b), Profile()).IsCandidate, Is.False,
                "Sans prolongement, la fin de M1 reste hors de portee de B.");

            string failure;
            var paths = ConflictSweep.Paths(graph, m1, ConflictSweep.Reach(Profile()), out failure);
            Assert.That(failure, Is.Null);
            float farthest = float.NegativeInfinity;
            foreach (var pose in paths[0])
            {
                farthest = Math.Max(farthest, pose.Position.x);
            }

            Assert.That(farthest, Is.EqualTo(ConflictSweep.Reach(Profile())).Within(1e-3f),
                "La portee traverse K (1 m) et s'arrete a un demi-gabarit dans N.");
            Assert.That(ConflictSweep.Evaluate(paths, One(b), Profile()).IsCandidate, Is.True);
        }

        [Test]
        public void EveryEndPoseCombinationIsExamined()
        {
            var a = new List<SweepPose> { Pose(0f, 0f, 0f), Pose(1f, 0f, 0f) };
            var b = new List<SweepPose> { Pose(10f, 0f, 0f), Pose(5.5f, 0f, 0f) };

            PairSweep sweep = Sweep(a, b);
            Assert.That(sweep.IsCandidate, Is.True);
            Assert.That(sweep.HasWitness, Is.True);
            Assert.That(sweep.WitnessA.Position.x, Is.EqualTo(1f).Within(1e-5f), "Temoin : seconde pose de A.");
            Assert.That(sweep.WitnessB.Position.x, Is.EqualTo(5.5f).Within(1e-5f), "Temoin : seconde pose de B.");
        }

        // ============================================================ englobant contre preuve exacte

        [Test]
        public void APairRetainedOnlyByBoundingBoxesIsPublishedButNotACandidate()
        {
            Vector2 normal = new Vector2(Mathf.Cos(-45f * Mathf.Deg2Rad), Mathf.Sin(-45f * Mathf.Deg2Rad));
            float gap = ContactDistance + 0.1f;
            float offset = 2f * HalfWidth + gap;
            var a = new List<SweepPose> { Pose(0f, 0f, 45f) };
            var b = new List<SweepPose> { Pose(normal.x * offset, normal.y * offset, 45f) };

            PairSweep sweep = Sweep(a, b);
            Assert.That(sweep.EnvelopeSelected, Is.True, "Les AABB de rectangles a 45 deg se recoupent.");
            Assert.That(sweep.ExactProven, Is.False);
            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.EnvelopeOnly));
            Assert.That(sweep.IsCandidate, Is.False);
            Assert.That(sweep.ExactSlackMeters, Is.EqualTo(0.1f).Within(1e-3f));
        }

        [Test]
        public void TheExactRectangleDistanceMatchesADenseBoundarySearch()
        {
            var random = new System.Random(5050);
            for (int trial = 0; trial < 60; trial++)
            {
                SweepPose a = Pose((float)random.NextDouble() * 12f - 6f, (float)random.NextDouble() * 12f - 6f, (float)random.NextDouble() * 360f);
                SweepPose b = Pose((float)random.NextDouble() * 12f - 6f, (float)random.NextDouble() * 12f - 6f, (float)random.NextDouble() * 360f);
                float exact = ConflictSweep.RectangleDistance(a, b, HalfLength, HalfWidth);
                float envelope = ConflictSweep.AabbDistance(a, b, HalfLength, HalfWidth);
                Assert.That(envelope, Is.LessThanOrEqualTo(exact + 1e-4f), "La distance englobante minore la distance exacte.");
                if (exact == 0f)
                {
                    continue;
                }

                float brute = BoundaryDistance(ConflictSweep.Corners(a, HalfLength, HalfWidth), ConflictSweep.Corners(b, HalfLength, HalfWidth));
                Assert.That(exact, Is.LessThanOrEqualTo(brute + 1e-4f), "La distance exacte ne surestime jamais.");
                Assert.That(exact, Is.EqualTo(brute).Within(0.01f));
            }
        }

        // ============================================================ echecs fermes

        [Test]
        public void ARotationOfNinetyDegreesOrMoreFailsClosed()
        {
            var a = new List<SweepPose> { Pose(0f, 0f, 0f), Pose(0.1f, 0f, 95f) };
            var b = Line(100f, 100f, 110f, 100f, 1f);

            PairSweep sweep = Sweep(a, b);
            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.FailClosed));
            Assert.That(sweep.IsCandidate, Is.True, "Echec ferme : candidat par prudence, jamais ecarte.");
            Assert.That(sweep.FailClosedReason, Does.Contain("90"));
            Assert.That(sweep.HasVolume, Is.True, "Un echec ferme publie toujours un volume de couverture.");
            Assert.That(sweep.Volume.Extents.x, Is.GreaterThan(50f), "Le volume couvre les deux trajectoires, pas une pose isolee.");
        }

        [Test]
        public void ATangentWithoutHorizontalPartFailsClosed()
        {
            var a = new List<SweepPose> { SweepPose.From(Vector3.zero, Vector3.up), Pose(1f, 0f, 0f) };
            var b = Line(100f, 100f, 110f, 100f, 1f);

            PairSweep sweep = Sweep(a, b);
            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.FailClosed));
            Assert.That(sweep.FailClosedReason, Does.Contain("horizontale"));
        }

        [Test]
        public void AnOverBranchedReachFailsClosed()
        {
            var graph = new SweepGraph();
            RoadId m = RoadId.New();
            RoadId hub = RoadId.New();
            graph.Add(m, true, Samples(-10f, 0f, 1f));
            graph.Add(hub, false, Samples(0f, 0.5f, 0.25f));
            graph.Link(m, hub);
            for (int i = 0; i < ConflictSweep.MaxPathsPerMovement + 1; i++)
            {
                RoadId branch = RoadId.New();
                graph.Add(branch, false, Samples(0.5f, 10f, 1f));
                graph.Link(hub, branch);
            }

            string failure;
            var paths = ConflictSweep.Paths(graph, m, ConflictSweep.Reach(Profile()), out failure);
            Assert.That(paths, Is.Null);
            Assert.That(failure, Does.Contain("ramifiee"));
        }

        // ============================================================ bords de l'API

        [Test]
        public void ASinglePoseTrajectoryKeepsTheExactEnvelopeComparison()
        {
            // Deux poses isolees, tres eloignees : le filtre englobant doit rester actif et rendre
            // NoContact (la ligne voisine non remplie faisait passer toute paire pour EnvelopeOnly).
            var a = new List<SweepPose> { Pose(0f, 0f, 0f) };
            var b = new List<SweepPose> { Pose(100f, 0f, 0f) };
            PairSweep sweep = Sweep(a, b);

            Assert.That(sweep.EnvelopeSelected, Is.False);
            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.NoContact));
            Assert.That(sweep.EnvelopeSlackMeters, Is.GreaterThan(0f), "La marge englobante publiee est reelle, jamais nulle par accident.");
        }

        [Test]
        public void APathWithoutPosesFailsClosed()
        {
            var empty = new List<SweepPose>();
            PairSweep sweep = ConflictSweep.Evaluate(One(empty), One(Line(100f, 100f, 110f, 100f, 1f)), Profile());

            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.FailClosed));
            Assert.That(sweep.FailClosedReason, Does.Contain("aucune pose"));
        }

        // ============================================================ suivi et portee

        [Test]
        public void FollowingIsDetectedAlongCorridorsWithinOneVehicleLength()
        {
            var graph = new SweepGraph();
            RoadId k1 = RoadId.New();
            RoadId k2 = RoadId.New();
            RoadId k3 = RoadId.New();
            graph.Add(k1, false, Samples(0f, 2f, 0.5f));
            graph.Add(k2, false, Samples(2f, 4f, 0.5f));
            graph.Add(k3, false, Samples(4f, 10f, 0.5f));
            graph.Link(k1, k2);
            graph.Link(k2, k3);

            Assert.That(ConflictSweep.Follows(graph, k1, k1, 4.5f), Is.True);
            Assert.That(ConflictSweep.Follows(graph, k1, k3, 4.5f), Is.True, "4 m de corridors avant k3.");
            Assert.That(ConflictSweep.Follows(graph, k1, k3, 3.5f), Is.False, "Au-dela de la distance de suivi.");
            Assert.That(ConflictSweep.Follows(graph, k3, k1, 100f), Is.False, "Le suivi est dirige.");
        }

        [Test]
        public void FollowingNeverCrossesAMovement()
        {
            var graph = new SweepGraph();
            RoadId k1 = RoadId.New();
            RoadId m = RoadId.New();
            RoadId k2 = RoadId.New();
            graph.Add(k1, false, Samples(0f, 1f, 0.5f));
            graph.Add(m, true, Samples(1f, 2f, 0.5f));
            graph.Add(k2, false, Samples(2f, 3f, 0.5f));
            graph.Link(k1, m);
            graph.Link(m, k2);

            Assert.That(ConflictSweep.Follows(graph, k1, k2, 100f), Is.False);
        }

        [Test]
        public void HeadAndTailCutsAreCanonicalEvaluations()
        {
            var graph = new SweepGraph();
            RoadId id = RoadId.New();
            SweepElement element = graph.Add(id, false, Samples(0f, 10f, 1f));

            var head = ConflictSweep.Head(element, 2.3f);
            var tail = ConflictSweep.Tail(element, 2.3f);
            Assert.That(head[head.Count - 1].Position.x, Is.EqualTo(element.Curve.Sample(element.StartS + 2.3f).Position.x).Within(1e-5f));
            Assert.That(tail[0].Position.x, Is.EqualTo(element.Curve.Sample(element.EndS - 2.3f).Position.x).Within(1e-5f));
            Assert.That(ConflictSweep.Head(element, 50f), Has.Count.EqualTo(element.Samples.Count), "Portee plus longue : element entier.");
        }

        // ============================================================ propriete de surete

        [Test]
        public void TheSweepNeverMissesADenseContact()
        {
            var random = new System.Random(20260927);
            int contacts = 0;
            for (int trial = 0; trial < 250; trial++)
            {
                var a = RandomPath(random);
                var b = RandomPath(random);
                bool dense = DenseMinimum(a, b) <= ContactDistance;
                PairSweep sweep = Sweep(a, b);
                if (dense)
                {
                    contacts++;
                    Assert.That(sweep.IsCandidate, Is.True, "Contact reel manque au tirage " + trial + ".");
                }

                if (sweep.Relation == PairRelation.NoContact || sweep.Relation == PairRelation.EnvelopeOnly)
                {
                    Assert.That(dense, Is.False, "Paire ecartee malgre un contact au tirage " + trial + ".");
                }
            }

            Assert.That(contacts, Is.GreaterThan(20), "Le tirage doit contenir assez de contacts pour etre probant.");
        }

        // ============================================================ outils

        private static List<SweepPose> RandomPath(System.Random random)
        {
            var path = new List<SweepPose>();
            float x = (float)random.NextDouble() * 16f - 8f;
            float z = (float)random.NextDouble() * 16f - 8f;
            float heading = (float)random.NextDouble() * 360f;
            int count = 2 + random.Next(3);
            for (int i = 0; i < count; i++)
            {
                path.Add(Pose(x, z, heading));
                float step = (float)random.NextDouble() * 6f;
                x += step * Mathf.Cos(heading * Mathf.Deg2Rad);
                z += step * Mathf.Sin(heading * Mathf.Deg2Rad);
                heading += (float)random.NextDouble() * 120f - 60f;
            }

            return path;
        }

        private static RoadCurveSample[] Samples(float x0, float x1, float step)
        {
            int count = Mathf.Max(1, Mathf.CeilToInt((x1 - x0) / step));
            var samples = new RoadCurveSample[count + 1];
            for (int i = 0; i <= count; i++)
            {
                float x = Mathf.Lerp(x0, x1, (float)i / count);
                samples[i] = new RoadCurveSample
                {
                    SMeters = x - x0,
                    Position = new Vector3(x, 0f, 0f),
                    Tangent = Vector3.right,
                    Up = Vector3.up,
                    CurvaturePerMeter = 0f,
                    HalfWidthLeftMeters = 2f,
                    HalfWidthRightMeters = 2f
                };
            }

            return samples;
        }

        private static bool Contains(RoadBoundsBox box, Vector3 point)
        {
            Vector3 d = point - box.Center;
            return Mathf.Abs(d.x) <= box.Extents.x && Mathf.Abs(d.y) <= box.Extents.y && Mathf.Abs(d.z) <= box.Extents.z;
        }

        private static float BoundaryDistance(Vector2[] a, Vector2[] b)
        {
            var pa = Boundary(a, 150);
            var pb = Boundary(b, 150);
            float best = float.PositiveInfinity;
            foreach (var p in pa)
            {
                foreach (var q in pb)
                {
                    best = Math.Min(best, (p - q).magnitude);
                }
            }

            return best;
        }

        private static List<Vector2> Boundary(Vector2[] corners, int perEdge)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                for (int k = 0; k < perEdge; k++)
                {
                    points.Add(Vector2.Lerp(corners[i], corners[(i + 1) % 4], (float)k / perEdge));
                }
            }

            return points;
        }
    }
}
