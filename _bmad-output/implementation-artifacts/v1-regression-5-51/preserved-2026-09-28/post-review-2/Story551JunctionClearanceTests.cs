using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.51 -- degagement des angles. Regle du relief routier franchissable (decision
    /// proprietaire du 2026-09-28) et premier angle TJunction_South/SE, mesures sur la VRAIE MVP_Run
    /// (ouverte en additif, jamais sauvegardee).
    /// </summary>
    public sealed class Story551JunctionClearanceTests
    {
        /// <summary>Hauteur de bordure authoree (contrat du kit, Story 5.11) : un relief de recette n'y depasse jamais.</summary>
        private const float AuthoredCurbHeight = 0.12f;

        private const float Clearance = 0.158f;

        private static Vector2[] Rect(float xMin, float xMax, float zMin, float zMax)
        {
            return new[] { new Vector2(xMin, zMin), new Vector2(xMax, zMin), new Vector2(xMax, zMax), new Vector2(xMin, zMax) };
        }

        // Chaussee z in [-4, 4], trottoirs au-dela : la geometrie de l'avenue Est de MVP_Run.
        private static readonly List<Vector2[]> Sidewalks = new List<Vector2[]> { Rect(8f, 24f, 4f, 8f), Rect(8f, 24f, -8f, -4f) };

        [Test]
        public void AReliefSpanningTheCarriagewayAndOnlyTouchingTheSidewalksIsDrivable()
        {
            Assert.That(JunctionClearance.IsDrivableRelief(Rect(9f, 11.4f, -4f, 4f), true, 0.12f, Sidewalks, Clearance), Is.True);
        }

        [Test]
        public void AReliefTallerThanTheStaticBodyClearanceStaysAnObstacle()
        {
            Assert.That(JunctionClearance.IsDrivableRelief(Rect(9f, 11.4f, -4f, 4f), true, Clearance, Sidewalks, Clearance), Is.True, "Borne incluse.");
            Assert.That(JunctionClearance.IsDrivableRelief(Rect(9f, 11.4f, -4f, 4f), true, Clearance + 0.001f, Sidewalks, Clearance), Is.False);
            Assert.That(JunctionClearance.IsDrivableRelief(Rect(9f, 11.4f, -4f, 4f), true, 0.8f, Sidewalks, Clearance), Is.False, "Muret ou barriere.");
            Assert.That(JunctionClearance.IsDrivableRelief(Rect(9f, 11.4f, -4f, 4f), true, float.NaN, Sidewalks, Clearance), Is.False, "Hauteur illisible.");
        }

        [Test]
        public void ACurbOverlappingASidewalkStaysAnObstacleEvenWhenLow()
        {
            // Col_Curb_* du carrefour : 0,30 m de large, poses DANS le polygone du trottoir d'angle.
            Assert.That(JunctionClearance.IsDrivableRelief(Rect(9f, 13f, 3.98f, 4.3f), true, 0.12f, Sidewalks, Clearance), Is.False);
            Assert.That(JunctionClearance.IsDrivableRelief(Rect(9f, 13f, 3.7f, 4.02f), true, 0.12f, Sidewalks, Clearance), Is.False, "2 cm de recouvrement.");
        }

        [Test]
        public void AReliefThatDoesNotRestOnTheCarriagewayStaysAnObstacle()
        {
            Assert.That(JunctionClearance.IsDrivableRelief(Rect(9f, 11.4f, -2f, 2f), false, 0.05f, Sidewalks, Clearance), Is.False);
            Assert.That(JunctionClearance.IsDrivableRelief(null, true, 0.05f, Sidewalks, Clearance), Is.False);
        }

        [Test]
        public void ContactWithinOneCentimetreIsNotAnOverlap()
        {
            Assert.That(JunctionClearance.Overlaps(Rect(0f, 1f, 0f, 1f), Rect(1f, 2f, 0f, 1f), JunctionClearance.ReliefOverlapToleranceMeters), Is.False, "Arete commune.");
            Assert.That(JunctionClearance.Overlaps(Rect(0f, 1.005f, 0f, 1f), Rect(1f, 2f, 0f, 1f), JunctionClearance.ReliefOverlapToleranceMeters), Is.False, "5 mm : contact.");
            Assert.That(JunctionClearance.Overlaps(Rect(0f, 1.02f, 0f, 1f), Rect(1f, 2f, 0f, 1f), JunctionClearance.ReliefOverlapToleranceMeters), Is.True, "2 cm : recouvrement.");
        }

        [Test]
        public void ATiltedBoxIsProjectedWithItsWholeVolumeSoAnOverhangingTopIsNotHidden()
        {
            WithScratchScene(() =>
            {
                // Boite inclinee de 60 deg : sa face haute deborde tres loin de sa base.
                var go = new GameObject("TiltedWall");
                go.transform.SetPositionAndRotation(new Vector3(500f, 1f, 0f), Quaternion.Euler(60f, 0f, 0f));
                var box = go.AddComponent<BoxCollider>();
                box.size = new Vector3(2f, 3f, 0.2f);
                Physics.SyncTransforms();

                Vector2[] hull = JunctionClearance.Footprint(box);
                Assert.That(hull, Is.Not.Null);
                for (int i = 0; i < 8; i++)
                {
                    var local = Vector3.Scale(box.size * 0.5f, new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                    Vector3 corner = go.transform.TransformPoint(local);
                    var point = new Vector2(corner.x, corner.z);
                    float inside = DistanceToHull(hull, point);
                    Assert.That(inside, Is.LessThanOrEqualTo(1e-4f), "Sommet " + i + " hors de l'enveloppe.");
                }

                Assert.That(hull.Max(p => p.y) - hull.Min(p => p.y), Is.GreaterThan(2.5f), "Le porte-a-faux en hauteur fait partie de l'empreinte.");
            });
        }

        [Test]
        public void ANonConvexMeshColliderIsRefusedNotIgnored()
        {
            WithScratchScene(() =>
            {
                var go = new GameObject("NonConvex");
                go.transform.position = new Vector3(500f, 0f, 0f);
                var mesh = go.AddComponent<MeshCollider>();
                mesh.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                mesh.convex = false;
                Assert.That(JunctionClearance.Footprint(mesh), Is.Null);
            });
        }

        [Test]
        public void RoadSupportIsDemonstratedOverTheWholeFootprint()
        {
            WithScratchScene(() =>
            {
                // Chaussee en deux dalles separees par un trou de 0,5 m ; trottoir affleurant ; plan de sol plus bas.
                Box("RoadA", new Vector3(495f, -0.1f, 0f), new Vector3(10f, 0.2f, 8f));
                Box("RoadB", new Vector3(505.5f, -0.1f, 0f), new Vector3(10f, 0.2f, 8f));
                var sidewalk = Box("Sidewalk", new Vector3(500f, -0.1f, 6f), new Vector3(20f, 0.2f, 4f));
                Box("Ground", new Vector3(500f, -0.15f, 20f), new Vector3(40f, 0.1f, 20f));
                var sidewalks = new HashSet<Collider> { sidewalk };
                Physics.SyncTransforms();

                Assert.That(Supported(Box("OnRoad", new Vector3(495f, 0.05f, 0f), new Vector3(2f, 0.1f, 8f)), sidewalks), Is.True, "Relief entierement sur la chaussee.");
                Assert.That(Supported(Box("OverGap", new Vector3(500.25f, 0.05f, 0f), new Vector3(2f, 0.1f, 6f)), sidewalks), Is.False, "Le milieu surplombe le trou : les sommets seuls ne suffisent pas.");
                Assert.That(Supported(Box("OverSidewalk", new Vector3(495f, 0.05f, 4.5f), new Vector3(2f, 0.1f, 2f)), sidewalks), Is.False, "Une partie repose sur le trottoir.");
                Assert.That(Supported(Box("OnGround", new Vector3(500f, 0.05f, 20f), new Vector3(2f, 0.1f, 2f)), sidewalks), Is.False, "Le plan de sol n'est pas la chaussee.");

                // Fente de 6 cm entre deux points de l'ancienne grille de 10 cm (500,05 et 500,15) : vue par l'inclusion exacte.
                Box("RoadC", new Vector3(495.035f, -0.1f, -25f), new Vector3(10.07f, 0.2f, 8f));
                Box("RoadD", new Vector3(505.065f, -0.1f, -25f), new Vector3(9.87f, 0.2f, 8f));
                Assert.That(Supported(Box("OverSlit", new Vector3(500f, 0.05f, -25f), new Vector3(2f, 0.1f, 2f)), sidewalks), Is.False, "Fente entre deux points d'echantillonnage.");

                // Dalles jointives : le joint n'est pas un trou.
                Box("RoadE", new Vector3(495f, -0.1f, -45f), new Vector3(10f, 0.2f, 8f));
                Box("RoadF", new Vector3(505f, -0.1f, -45f), new Vector3(10f, 0.2f, 8f));
                Assert.That(Supported(Box("OverJoint", new Vector3(500f, 0.05f, -45f), new Vector3(2f, 0.1f, 8f)), sidewalks), Is.True, "Joint de dalles jointives.");
            });
        }

        [Test]
        public void RoadSupportCannotComeFromAnotherLoadedScene()
        {
            WithScratchScene(() =>
            {
                Scene local = SceneManager.GetActiveScene();
                var relief = Box("Relief", new Vector3(500f, 0.05f, 90f), new Vector3(2f, 0.1f, 2f));
                WithBootstrapScene(foreign =>
                {
                    SceneManager.SetActiveScene(local);
                    var road = Box("ForeignRoad", new Vector3(500f, -0.1f, 90f), new Vector3(4f, 0.2f, 4f));
                    SceneManager.MoveGameObjectToScene(road.gameObject, foreign);
                    Assert.That(Supported(relief, new HashSet<Collider>()), Is.False, "Un support superpose mais etranger ne porte pas le relief.");
                    SceneManager.MoveGameObjectToScene(road.gameObject, local);
                    Assert.That(Supported(relief, new HashSet<Collider>()), Is.True, "Le meme support dans la scene du relief le porte.");
                });
            });
        }

        [Test]
        public void RoadTopCannotComeFromAnotherLoadedScene()
        {
            WithScratchScene(() =>
            {
                Scene local = SceneManager.GetActiveScene();
                WithBootstrapScene(foreign =>
                {
                    SceneManager.SetActiveScene(local);
                    var method = typeof(JunctionClearance).GetMethod("RoadTop", BindingFlags.NonPublic | BindingFlags.Static);
                    Assert.That(method, Is.Not.Null);
                    var position = new Vector3(500f, 0.05f, 140f);
                    Func<float> top = () => (float)method.Invoke(null, new object[] { position, local });
                    Assert.That(float.IsNegativeInfinity(top()), Is.True, "Aucune surface roulable locale sous la pose.");

                    var foreignRoad = Box("ForeignRoadTop", new Vector3(500f, -0.1f, 140f), new Vector3(4f, 0.2f, 4f));
                    SceneManager.MoveGameObjectToScene(foreignRoad.gameObject, foreign);
                    Physics.SyncTransforms();
                    Assert.That(float.IsNegativeInfinity(top()), Is.True, "Une chaussee etrangere ne fournit jamais la surface roulable.");

                    SceneManager.MoveGameObjectToScene(foreignRoad.gameObject, local);
                    Physics.SyncTransforms();
                    Assert.That(top(), Is.EqualTo(0f).Within(1e-4f), "La meme chaussee dans la scene locale est vue.");
                });
            });
        }

        [Test]
        public void CoverageIsExactNotSampled()
        {
            float tolerance = JunctionClearance.ReliefSupportToleranceMeters;
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new[] { Rect(0f, 1f, 0f, 2f), Rect(1f, 2f, 0f, 2f) }, tolerance), Is.True, "Deux moities jointives.");
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new[] { Rect(0f, 1f, 0f, 2f), Rect(1.004f, 2f, 0f, 2f) }, tolerance), Is.False, "Fente de 4 mm.");
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new[] { Rect(0f, 2f, 0f, 1f), Rect(0f, 1f, 1f, 2f) }, tolerance), Is.False, "Coin manquant.");
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new[] { Rect(-1f, 3f, -1f, 3f) }, tolerance), Is.True, "Support englobant.");
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new Vector2[0][], tolerance), Is.False, "Aucun support.");
        }

        private static RoadModelValidationProfile SweepProfile()
        {
            return new RoadModelValidationProfile
            {
                MaxVehicleLengthMeters = 4.5f,
                MaxVehicleHalfWidthMeters = 1.03f,
                LateralClearanceMarginMeters = 0.25f
            };
        }

        private static SweepPose Pose(float x, float y, float z, float headingDegrees, RoadId element)
        {
            float angle = headingDegrees * Mathf.Deg2Rad;
            var pose = SweepPose.From(new Vector3(x, y, z), new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
            pose.ElementId = element;
            return pose;
        }

        [Test]
        public void PlanarClearanceFindsContactBetweenPosesAndAcrossASeamAtAnyHeight()
        {
            var profile = SweepProfile();
            var obstacle = Rect(-0.1f, 0.1f, -0.1f, 0.1f);
            RoadId first = RoadId.New(), second = RoadId.New();
            var a = Pose(-5f, 0f, 0f, 0f, first);
            var b = Pose(5f, 0f, 0f, 0f, first);
            Assert.That(JunctionClearance.Distance(ConflictSweep.Corners(a, JunctionClearance.HalfLength(profile), JunctionClearance.HalfWidth(profile)), obstacle), Is.GreaterThan(0f));
            Assert.That(JunctionClearance.Distance(ConflictSweep.Corners(b, JunctionClearance.HalfLength(profile), JunctionClearance.HalfWidth(profile)), obstacle), Is.GreaterThan(0f));

            var interval = JunctionClearance.MeasurePath(new[] { a, b }, new[] { obstacle }, profile);
            Assert.That(interval.Residual, Is.LessThan(0f), "Le contact est entre les poses, absentes de l'obstacle aux extremites.");
            Assert.That(interval.Seam, Is.False);

            b.ElementId = second;
            b.Position.y = 100f; // La porte Sidewalk reste planaire, sans condition de hauteur.
            var seam = JunctionClearance.MeasurePath(new[] { a, b }, new[] { obstacle }, profile);
            Assert.That(seam.Residual, Is.EqualTo(interval.Residual).Within(1e-4f));
            Assert.That(seam.Seam, Is.True);
            Assert.That(seam.IntervalDelta, Is.EqualTo(10f).Within(1e-4f));
        }

        [Test]
        public void PlanarClearanceFindsAContactOnlyDuringRotation()
        {
            var profile = SweepProfile();
            var obstacle = Rect(2.4f, 2.5f, -0.05f, 0.05f);
            RoadId element = RoadId.New();
            var a = Pose(0f, 0f, 0f, -45f, element);
            var b = Pose(0f, 0f, 0f, 45f, element);
            Assert.That(JunctionClearance.Distance(ConflictSweep.Corners(a, JunctionClearance.HalfLength(profile), JunctionClearance.HalfWidth(profile)), obstacle), Is.GreaterThan(0f));
            Assert.That(JunctionClearance.Distance(ConflictSweep.Corners(b, JunctionClearance.HalfLength(profile), JunctionClearance.HalfWidth(profile)), obstacle), Is.GreaterThan(0f));
            Assert.That(JunctionClearance.Distance(ConflictSweep.Corners(Pose(0f, 0f, 0f, 0f, element), JunctionClearance.HalfLength(profile), JunctionClearance.HalfWidth(profile)), obstacle), Is.Zero);
            var witness = JunctionClearance.MeasurePath(new[] { a, b }, new[] { obstacle }, profile);
            Assert.That(witness.Residual, Is.LessThan(0f));
            Assert.That(witness.IntervalDelta, Is.GreaterThan(0f));
        }

        [Test]
        public void ARightTurnSquareCornerFailsBothGatesAndARealPlanChamferClearsThem()
        {
            var profile = SweepProfile();
            var pose = Pose(0f, 0f, 0f, 0f, RoadId.New());
            var square = Rect(2.5f, 5f, 1f, 5f);
            var cut = new[]
            {
                new Vector2(3.5f, 1f), new Vector2(5f, 1f), new Vector2(5f, 5f),
                new Vector2(2.5f, 5f), new Vector2(2.5f, 2f)
            };
            Assert.That(JunctionClearance.MeasurePath(new[] { pose }, new[] { square }, profile).Residual, Is.Zero, "Angle carre : empreinte dans trottoir et bordure.");
            Assert.That(JunctionClearance.MeasurePath(new[] { pose }, new[] { cut }, profile).Residual, Is.GreaterThan(0f), "Le chanfrein en plan degage les deux portes.");
        }

        [Test]
        public void SubdivisionUsesStoredCurveKnotsAndRejectsDegenerateTangents()
        {
            var graph = new SweepGraph();
            RoadId id = RoadId.New();
            var samples = new[]
            {
                new RoadCurveSample { SMeters = 0f, Position = Vector3.zero, Tangent = Vector3.right, Up = Vector3.up, CurvaturePerMeter = 25f, HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f },
                new RoadCurveSample { SMeters = 1f, Position = new Vector3(1f, 0f, 1f), Tangent = new Vector3(1f, 0f, 1f).normalized, Up = Vector3.up, CurvaturePerMeter = -25f, HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f },
                new RoadCurveSample { SMeters = 2f, Position = new Vector3(1f, 0f, 2f), Tangent = Vector3.forward, Up = Vector3.up, CurvaturePerMeter = 10f, HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f }
            };
            var element = graph.Add(id, true, samples);
            var knots = ConflictSweep.Poses(element.Samples);
            for (int i = 0; i < knots.Count; i++)
            {
                var knot = knots[i];
                knot.ElementId = id;
                knot.SMeters = samples[i].SMeters;
                knots[i] = knot;
            }
            var dense = JunctionClearance.Subdivide(knots, graph, 0.25f);
            Assert.That(dense.Count, Is.EqualTo(9));
            foreach (var knot in knots)
                Assert.That(dense.Any(p => p.SMeters == knot.SMeters && p.Position == knot.Position && p.Heading == knot.Heading), Is.True, "Noeud compile perdu : " + knot.SMeters);
            foreach (var pose in dense)
            {
                RoadCurvePoint canonical = element.Curve.Sample(pose.SMeters);
                Assert.That(pose.Position, Is.EqualTo(canonical.Position));
                Assert.That(pose.Heading, Is.EqualTo(new Vector2(canonical.Tangent.x, canonical.Tangent.z).normalized));
            }

            var degenerate = SweepPose.From(Vector3.zero, Vector3.up);
            Assert.Throws<ArgumentException>(() => JunctionClearance.Subdivide(new[] { degenerate }, graph, 0.25f));
            Assert.Throws<ArgumentException>(() => JunctionClearance.Subdivide(new[] { Pose(0f, 0f, 0f, 0f, id), Pose(1f, 0f, 0f, 180f, id) }, graph, 0.25f));
        }

        [Test]
        public void PhysicalFilterRejectsTriggersInactiveCollidersAndDynamicBodies()
        {
            WithScratchScene(() =>
            {
                var ai = Box("Ai", new Vector3(510f, 0f, 0f), Vector3.one);
                var box = Box("Candidate", new Vector3(500f, 0f, 0f), Vector3.one);
                var method = typeof(JunctionClearance).GetMethod("Participates", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.That(method, Is.Not.Null);
                Func<bool> participates = () => (bool)method.Invoke(null, new object[] { box, ai });
                Assert.That(participates(), Is.True);
                box.isTrigger = true;
                Assert.That(participates(), Is.False);
                box.isTrigger = false;
                box.enabled = false;
                Assert.That(participates(), Is.False);
                box.enabled = true;
                var body = box.gameObject.AddComponent<Rigidbody>();
                Assert.That(participates(), Is.False);
                body.isKinematic = true;
                Assert.That(participates(), Is.True);
                box.excludeLayers = 1 << ai.gameObject.layer;
                ai.excludeLayers = 1 << box.gameObject.layer;
                Assert.That(participates(), Is.False);
                ai.excludeLayers = 0;
            });
        }

        private static BoxCollider Box(string name, Vector3 position, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            return box;
        }

        private static bool Supported(BoxCollider relief, HashSet<Collider> sidewalks)
        {
            Physics.SyncTransforms();
            return JunctionClearance.RoadSupported(relief, JunctionClearance.Footprint(relief), 0f, 0f, sidewalks);
        }

        private static float DistanceToHull(Vector2[] hull, Vector2 point)
        {
            // 0 dedans ; sinon distance a l'arete la plus proche.
            bool positive = false, negative = false;
            float best = float.PositiveInfinity;
            for (int i = 0; i < hull.Length; i++)
            {
                Vector2 a = hull[i], b = hull[(i + 1) % hull.Length];
                float cross = (b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x);
                if (cross > 1e-6f) positive = true;
                if (cross < -1e-6f) negative = true;
                Vector2 e = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(point - a, e) / e.sqrMagnitude);
                best = Mathf.Min(best, (point - a - t * e).magnitude);
            }

            return positive && negative ? best : 0f;
        }

        /// <summary>
        /// Objets temporaires poses dans la scene de test active (celle du lanceur, jamais MVP_Run), loin de
        /// MVP_Run (x = 500 m), detruits a la fin ; rien n'est sauvegarde.
        /// </summary>
        private static void WithScratchScene(Action body)
        {
            var active = SceneManager.GetActiveScene();
            Assert.That(active.path, Is.Not.EqualTo(MigrationReport.ScenePath), "Les objets temporaires ne vont jamais dans MVP_Run.");
            var before = new HashSet<GameObject>(active.GetRootGameObjects());
            try
            {
                body();
            }
            finally
            {
                foreach (GameObject root in active.GetRootGameObjects())
                {
                    if (!before.Contains(root)) UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        [Test]
        public void TheReliefBoundIsTheAiVehicleStaticBodyClearanceAndClearsTheAuthoredCurbHeight()
        {
            var ai = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            var body = ai.GetComponent<BoxCollider>();
            var def = new SerializedObject(ai.GetComponent<VehiclePhysicsBody>()).FindProperty("vehicleProfile").objectReferenceValue as VehicleProfileDef;
            Assert.That(def, Is.Not.Null);
            Assert.That(def.TryValidate(out string error), Is.True, error);

            float clearance = JunctionClearance.StaticBodyClearance(def.Profile, body);
            // 0,45 - 1200 x 9,81 / (4 x 32000) - 0,22 + (0,73 - 0,71) = 0,158 m
            Assert.That(clearance, Is.EqualTo(0.158f).Within(0.002f));
            Assert.That(clearance, Is.GreaterThan(AuthoredCurbHeight));
        }

        [Test]
        public void MvpRunClassifiesTheRoadReliefsAsDrivableAndKeepsTheCurbsAsObstacles()
        {
            var result = MeasureMvpRun();
            Assert.That(result.ReliefClearanceMeters, Is.EqualTo(0.158f).Within(0.002f));

            var reliefs = result.DrivableReliefs.Select(r => r.Collider.Substring(r.Collider.LastIndexOf('/') + 1)).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            Assert.That(reliefs, Is.EqualTo(new[] { "Rampe_Est", "Rampe_Ouest", "Relief_MarcheBasse_AvenueCenterToEast" }));
            Assert.That(result.DrivableReliefs.All(r => r.HeightMeters <= r.ClearanceMeters), Is.True);

            foreach (var row in result.Rows)
            {
                string obstacle = row.Physical.Obstacle ?? string.Empty;
                Assert.That(obstacle.Contains("Rampe_") || obstacle.Contains("Relief_"), Is.False, row.Junction + " / " + row.Movement + " : " + obstacle);
            }

            // Bordures du carrefour : toujours des obstacles, jamais des reliefs ; chaque virage a droite a pour
            // temoin physique la bordure reculee de son angle.
            Assert.That(result.DrivableReliefs.Any(r => r.Collider.Contains("/Col_Curb_")), Is.False);
            var rightTurns = result.Rows.Where(r => r.Junction == "Intersection_Center_Crossroads" && r.Movement.Contains("(droite)")).ToArray();
            Assert.That(rightTurns, Is.Not.Empty);
            Assert.That(rightTurns.All(r => r.Physical.Obstacle.Contains("/Col_Curb_") && r.Physical.Residual > 0f), Is.True);

            // Le dos d'ane ne penalise plus les mouvements qui le franchissent.
            var straight = result.Rows.Where(r => r.Junction == "Intersection_Center_Crossroads" && r.Movement.Contains("Junction_FromEast -> Connector_West_Out")).ToArray();
            Assert.That(straight, Is.Not.Empty);
            Assert.That(straight.All(r => r.Physical.Residual > 0f), Is.True);
        }

        [Test]
        public void TheFirstCornerPassesBothGatesWithConcordantVisuals()
        {
            var result = MeasureMvpRun();
            var rows = result.Rows.Where(r => r.Junction == "TJunction_South" && r.Surface.EndsWith("TJunction_South/Collision/Col_Sidewalk_Corner_SE", StringComparison.Ordinal)).ToArray();
            Assert.That(rows.Length, Is.EqualTo(4), "Mouvements a portee de l'angle SE.");
            foreach (var row in rows)
            {
                Assert.That(row.Physical.Residual, Is.GreaterThan(0f), row.Movement);
                Assert.That(row.Semantic.Residual, Is.GreaterThan(0f), row.Movement);
            }

            Assert.That(rows.Min(r => r.Semantic.Residual), Is.EqualTo(0.1127f).Within(0.001f), "Virage a droite FromSouth -> East, coupe de 1,00 m.");
            Assert.That(result.Failures.Where(f => f.Contains("TJunction_South/Collision/Col_Sidewalk_Corner_SE") || f.StartsWith("Visuel Sidewalk discordant TJunction_South", StringComparison.Ordinal)), Is.Empty);
        }

        [Test]
        public void EveryTJunctionMovementPassesBothGatesAfterTheFirstWave()
        {
            var result = MeasureMvpRun();
            foreach (string junction in new[] { "TJunction_South", "TJunction_North", "TJunction_East", "TJunction_West" })
            {
                var rows = result.Rows.Where(r => r.Junction == junction).ToArray();
                Assert.That(rows.Length, Is.GreaterThan(0), junction);
                foreach (var row in rows)
                {
                    Assert.That(row.Physical.Residual, Is.GreaterThan(0f), junction + " / " + row.Movement + " / " + row.Surface);
                    Assert.That(row.Semantic.Residual, Is.GreaterThan(0f), junction + " / " + row.Movement + " / " + row.Surface);
                }

                Assert.That(result.Failures.Where(f => f.Contains(junction)), Is.Empty, junction);
            }
        }

        [TestCase("SE", "Col_Curb_South_East", "Junction_FromSouth -> Connector_East_Out")]
        [TestCase("NE", "Col_Curb_North_East", "Junction_FromEast -> Connector_North_Out")]
        [TestCase("NW", "Col_Curb_North_West", "Junction_FromNorth -> Connector_West_Out")]
        [TestCase("SW", "Col_Curb_South_West", "Junction_FromWest -> Connector_South_Out")]
        public void EachCrossroadsCornerPassesBothGatesAndKeepsItsCurbAsAnObstacle(string corner, string curbName, string rightTurn)
        {
            var result = MeasureMvpRun();
            string surface = "Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_" + corner;
            var rows = result.Rows.Where(r => r.Junction == "Intersection_Center_Crossroads" && r.Surface.EndsWith(surface, StringComparison.Ordinal)).ToArray();
            Assert.That(rows, Is.Not.Empty);
            foreach (var row in rows)
            {
                Assert.That(row.Physical.Residual, Is.GreaterThan(0f), row.Movement);
                Assert.That(row.Semantic.Residual, Is.GreaterThan(0f), row.Movement);
            }

            // Le virage a droite longe la bordure reculee : elle reste le temoin physique, a la meme marge que le trottoir.
            var turn = rows.Single(r => r.Movement.Contains(rightTurn));
            Assert.That(turn.Physical.Obstacle, Does.EndWith("/" + curbName));
            Assert.That(turn.Physical.Residual, Is.EqualTo(0.1127f).Within(0.001f));
            Assert.That(turn.Semantic.Residual, Is.EqualTo(0.1127f).Within(0.001f));
            Assert.That(result.Failures.Where(f => f.Contains(surface) || f.StartsWith("Visuel Sidewalk discordant Intersection_Center_Crossroads", StringComparison.Ordinal)), Is.Empty);

            // Role physique de la bordure conserve : meme nom, active, pleine, 0,12 m, reculee de 1,00 m jusqu'au sommet du chanfrein.
            WithMvpRun(scene =>
            {
                var curb = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true))
                    .Single(c => c.name == curbName && c.transform.parent.parent.name == "Intersection_Center_Crossroads");
                Assert.That(curb.enabled && curb.gameObject.activeInHierarchy && !curb.isTrigger, Is.True);
                Bounds b = curb.bounds;
                Assert.That(b.size.y, Is.EqualTo(0.12f).Within(1e-4f));
                Assert.That(b.size.z, Is.EqualTo(0.3f).Within(1e-4f));
                Assert.That(b.size.x, Is.EqualTo(3f).Within(1e-3f));
                Assert.That(Mathf.Min(Mathf.Abs(b.min.x), Mathf.Abs(b.max.x)), Is.EqualTo(5f).Within(1e-3f), "Extremite au sommet du chanfrein (4 + 1,00 m).");
            });
        }

        [Test]
        public void TheFiveJunctionsPassBothGatesEverywhere()
        {
            var result = MeasureMvpRun();
            Assert.That(result.Failures, Is.Empty);
            Assert.That(result.Passed, Is.True);
            Assert.That(result.Rows.Select(r => r.Junction).Distinct().Count(), Is.EqualTo(5));
        }

        [Test]
        public void EveryImportedJunctionMovementHasRowsAndRightTurnsHavePhysicalObstacles()
        {
            var result = MeasureMvpRun();
            WithMvpRun(scene =>
            {
                var migration = MigrationReport.Run(scene, File.ReadAllText(MigrationReport.LineageFullPath));
                Assert.That(migration.Import != null && migration.Import.Succeeded, Is.True, string.Join("\n", migration.Failures.ToArray()));
                string[] expected = migration.Import.Movements
                    .Where(m => m.Module.Kind == V1ModuleKind.Crossroads || m.Module.Kind == V1ModuleKind.TJunction)
                    .Select(m => m.Module.Label + "|" + m.Label).OrderBy(s => s, StringComparer.Ordinal).ToArray();
                string[] actual = result.Rows.Select(r => r.Junction + "|" + r.Movement).Distinct()
                    .OrderBy(s => s, StringComparer.Ordinal).ToArray();
                Assert.That(actual, Is.EqualTo(expected), "Le jeu des mouvements mesures doit egaler exactement celui de l'import V1.");
            });

            var empty = result.Rows.Where(r => float.IsPositiveInfinity(r.Physical.Residual)).ToArray();
            Assert.That(empty.All(r => r.PhysicalSetEmpty && string.IsNullOrEmpty(r.Physical.Obstacle)), Is.True,
                "Chaque residu physique +infini (aucun obstacle participant) est publie explicitement, jamais silencieux.");
            var rightTurns = result.Rows.Where(r => r.Movement.Contains("(droite)"))
                .GroupBy(r => r.Junction + "|" + r.Movement).Select(g => g.First()).ToArray();
            Assert.That(rightTurns.Length, Is.EqualTo(12));
            Assert.That(rightTurns.All(r => !r.PhysicalSetEmpty && !float.IsInfinity(r.Physical.Residual)
                && r.Physical.Residual > 0f && !string.IsNullOrEmpty(r.Physical.Obstacle)), Is.True,
                "Chacun des 12 virages a droite doit garder un obstacle physique attendu, distinct du cas vide.");
        }

        [Test]
        public void NewAuthoredSidewalkLooksAndTwoAdjacentFiveCentimetreCellsAreDetected()
        {
            var role = typeof(JunctionClearance).GetMethod("IsAuthoredSidewalkVisual", BindingFlags.NonPublic | BindingFlags.Static);
            var adjacent = typeof(JunctionClearance).GetMethod("HasAdjacentMismatch", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(role, Is.Not.Null);
            Assert.That(adjacent, Is.Not.Null);
            WithScratchScene(() =>
            {
                var visual = new GameObject("Sidewalk_NewMeshAndMaterial").AddComponent<MeshRenderer>();
                var road = new GameObject("Road_NewMeshAndMaterial").AddComponent<MeshRenderer>();
                Assert.That((bool)role.Invoke(null, new object[] { visual }), Is.True);
                Assert.That((bool)role.Invoke(null, new object[] { road }), Is.False, "Le decor ordinaire reste hors gate.");
                visual.name = "Chamfer_Visual_0";
                Assert.That((bool)role.Invoke(null, new object[] { visual }), Is.True);
            });

            var cells = new byte[3 * 3];
            Func<int, int, bool> hasPair = (x, z) => (bool)adjacent.Invoke(null, new object[] { cells, 3, 3, x, z, (byte)2 });
            cells[1 * 3 + 1] = 2;
            Assert.That(hasPair(1, 1), Is.False, "Une cellule de 5 cm seule reste sous la tolerance.");
            cells[1 * 3 + 2] = 2;
            Assert.That(hasPair(1, 1) && hasPair(1, 2), Is.True, "Deux cellules voisines de 5 cm echouent.");
            cells[1 * 3 + 2] = 0;
            cells[2 * 3 + 2] = 2;
            Assert.That(hasPair(1, 1), Is.False, "Une diagonale ne forme pas une bande contigue de deux cellules.");
        }

        [Test]
        public void ANameOnlySidewalkVisualOutsideDeclaredRegionsFailsAndStaysQuietWithoutTheName()
        {
            WithMvpRun(scene =>
            {
                var module = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                    .First(t => t.name == "TJunction_South" && t.parent != null && t.parent.name == "LaneGraph");
                var corner = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true))
                    .First(c => c.name == "Col_Sidewalk_Corner_SE" && HasAncestor(c.transform, "TJunction_South"));
                float outward = Mathf.Sign(corner.bounds.center.z - module.position.z);
                var visual = new GameObject("Sidewalk_ScratchUnknownLook");
                var mesh = new Mesh();
                try
                {
                    mesh.vertices = new[]
                    {
                        new Vector3(-0.2f, 0f, -0.2f), new Vector3(0.2f, 0f, -0.2f),
                        new Vector3(0.2f, 0f, 0.2f), new Vector3(-0.2f, 0f, 0.2f)
                    };
                    mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
                    mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
                    visual.AddComponent<MeshFilter>().sharedMesh = mesh;
                    visual.AddComponent<MeshRenderer>();
                    visual.transform.SetParent(module, false);
                    visual.transform.position = new Vector3(
                        corner.bounds.center.x,
                        0.5f,
                        corner.bounds.center.z + outward * (corner.bounds.extents.z + 0.5f));
                    Physics.SyncTransforms();

                    var named = MeasureFresh(scene);
                    Assert.That(named.Failures.Any(f => f.StartsWith("Visuel Sidewalk discordant TJunction_South", StringComparison.Ordinal)
                        && f.Contains("trottoir visible non declare")), Is.True,
                        "Un visuel nomme trottoir hors zone declaree est signale par la porte visuelle.");

                    visual.name = "Road_ScratchUnknownLook";
                    var neutral = MeasureFresh(scene);
                    Assert.That(neutral.Failures.Any(f => f.StartsWith("Visuel Sidewalk discordant TJunction_South", StringComparison.Ordinal)), Is.False,
                        "Le meme visuel sans nom authoring trottoir reste hors de la porte.");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(visual);
                    UnityEngine.Object.DestroyImmediate(mesh);
                    Physics.SyncTransforms();
                }
            });
        }

        [Test]
        public void DisabledNorthCornersRemainSemanticInputsAndOrdinaryRoadDoesNot()
        {
            WithMvpRun(scene =>
            {
                var failures = new List<string>();
                var sidewalks = SidewalkDeclarations.Read(scene, failures);
                Assert.That(failures, Is.Empty);
                foreach (string corner in new[] { "SE", "SW" })
                {
                    var surface = sidewalks.Single(s => s.Name.EndsWith("TJunction_North/Collision/Col_Sidewalk_Corner_" + corner, StringComparison.Ordinal));
                    Assert.That(surface.Colliders.Any(c => c.name == "Col_Sidewalk_Corner_" + corner && !c.enabled), Is.True, corner);
                    Assert.That(MeasureMvpRun().Rows.Any(r => r.Surface == surface.Name && r.Semantic.Residual > 0f), Is.True, corner);
                }

                Assert.That(sidewalks.Any(s => s.Name.Contains("Road_n2_s2")), Is.False, "La chaussee n'est pas une declaration Sidewalk.");
            });
        }

        [Test]
        public void ADeclarationWithoutItsVisibleSidewalkAndAVisibleSidewalkWithoutItsDeclarationBothFail()
        {
            var baseline = MeasureMvpRun();
            WithMvpRun(scene =>
            {
                var readFailures = new List<string>();
                var sidewalks = SidewalkDeclarations.Read(scene, readFailures);
                Assert.That(readFailures, Is.Empty);
                var omitted = sidewalks.Where(s => !s.Name.EndsWith("TJunction_South/Collision/Col_Sidewalk_Corner_SE", StringComparison.Ordinal)).ToArray();
                Assert.That(omitted.Length, Is.EqualTo(sidewalks.Count - 1));
                var withoutDeclaration = MeasureFresh(scene, omitted);
                Assert.That(withoutDeclaration.Failures.Any(f => f.StartsWith("Visuel Sidewalk discordant TJunction_South", StringComparison.Ordinal)), Is.True);
                Assert.That(withoutDeclaration.PhysicalFingerprint, Is.EqualTo(baseline.PhysicalFingerprint));
                Assert.That(withoutDeclaration.SemanticFingerprint, Is.Not.EqualTo(baseline.SemanticFingerprint));

                var visuals = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true))
                    .Where(r => HasAncestor(r.transform, "TJunction_South")).ToArray();
                Assert.That(visuals, Is.Not.Empty);
                bool[] enabled = visuals.Select(r => r.enabled).ToArray();
                try
                {
                    foreach (var visual in visuals) visual.enabled = false;
                    Physics.SyncTransforms();
                    var withoutVisual = MeasureFresh(scene);
                    Assert.That(withoutVisual.Failures.Any(f => f.StartsWith("Visuel Sidewalk discordant TJunction_South", StringComparison.Ordinal)), Is.True);
                    Assert.That(withoutVisual.PhysicalFingerprint, Is.EqualTo(baseline.PhysicalFingerprint));
                    Assert.That(withoutVisual.SemanticFingerprint, Is.Not.EqualTo(baseline.SemanticFingerprint));
                }
                finally
                {
                    for (int i = 0; i < visuals.Length; i++) visuals[i].enabled = enabled[i];
                    Physics.SyncTransforms();
                    Assert.That(visuals.Select(r => r.enabled), Is.EqualTo(enabled));
                }
            });
        }

        [Test]
        public void PhysicalAndSemanticFingerprintsInvalidateIndependently()
        {
            var baseline = MeasureMvpRun();
            WithMvpRun(scene =>
            {
                var curb = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true))
                    .Single(c => c.name == "Col_Curb_South_East" && HasAncestor(c.transform, "Intersection_Center_Crossroads"));
                bool trigger = curb.isTrigger;
                try
                {
                    curb.isTrigger = !trigger;
                    Physics.SyncTransforms();
                    var changed = MeasureFresh(scene);
                    Assert.That(changed.PhysicalFingerprint, Is.Not.EqualTo(baseline.PhysicalFingerprint));
                    Assert.That(changed.SemanticFingerprint, Is.EqualTo(baseline.SemanticFingerprint));
                }
                finally
                {
                    curb.isTrigger = trigger;
                    Physics.SyncTransforms();
                    Assert.That(curb.isTrigger, Is.EqualTo(trigger));
                }

                var readFailures = new List<string>();
                var surface = SidewalkDeclarations.Read(scene, readFailures).Single(s => s.Name.EndsWith("TJunction_North/Collision/Col_Sidewalk_Corner_SE", StringComparison.Ordinal));
                Assert.That(readFailures, Is.Empty);
                var declaration = (Behaviour)surface.Declaration;
                bool enabled = declaration.enabled;
                try
                {
                    declaration.enabled = !enabled;
                    Physics.SyncTransforms();
                    var changed = MeasureFresh(scene);
                    Assert.That(changed.PhysicalFingerprint, Is.EqualTo(baseline.PhysicalFingerprint));
                    Assert.That(changed.SemanticFingerprint, Is.Not.EqualTo(baseline.SemanticFingerprint));
                }
                finally
                {
                    declaration.enabled = enabled;
                    Physics.SyncTransforms();
                    Assert.That(declaration.enabled, Is.EqualTo(enabled));
                }
            });
        }

        [Test]
        public void SemanticFingerprintChangesWhenOnlyMeshUvsChange()
        {
            WithScratchScene(() =>
            {
                var visual = new GameObject("Visual");
                var mesh = new Mesh();
                try
                {
                    mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.forward };
                    mesh.triangles = new[] { 0, 1, 2 };
                    mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up };
                    visual.AddComponent<MeshFilter>().sharedMesh = mesh;
                    var renderer = visual.AddComponent<MeshRenderer>();
                    var method = typeof(JunctionClearance).GetMethod("Fingerprint", BindingFlags.NonPublic | BindingFlags.Static);
                    Assert.That(method, Is.Not.Null);
                    Func<string> fingerprint = () => (string)method.Invoke(null, new object[] { new Component[] { renderer }, "", 0 });
                    string baseline = fingerprint();

                    mesh.uv = new[] { Vector2.zero, new Vector2(0.5f, 0f), Vector2.up };
                    string changedUv0 = fingerprint();
                    Assert.That(changedUv0, Is.Not.EqualTo(baseline), "Le canal UV principal modifie l'empreinte semantique.");

                    mesh.uv2 = new[] { Vector2.zero, Vector2.right, Vector2.up };
                    Assert.That(fingerprint(), Is.Not.EqualTo(changedUv0), "Un canal UV secondaire la modifie aussi.");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
            });
        }

        [Test]
        public void CollisionMeshUvsDoNotInvalidateThePhysicalFingerprint()
        {
            WithScratchScene(() =>
            {
                var mesh = UnityEngine.Object.Instantiate(Resources.GetBuiltinResource<Mesh>("Cube.fbx"));
                try
                {
                    var collision = new GameObject("Collision").AddComponent<MeshCollider>();
                    collision.sharedMesh = mesh;
                    var method = typeof(JunctionClearance).GetMethod("Fingerprint", BindingFlags.NonPublic | BindingFlags.Static);
                    Assert.That(method, Is.Not.Null);
                    Func<string> fingerprint = () => (string)method.Invoke(null, new object[] { new Component[] { collision }, "", 0 });
                    string baseline = fingerprint();

                    var uv = mesh.uv;
                    uv[0] = new Vector2(uv[0].x + 0.25f, uv[0].y);
                    mesh.uv = uv;
                    Assert.That(fingerprint(), Is.EqualTo(baseline), "Les UV d'un mesh de collision restent hors de l'empreinte physique.");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
            });
        }

        private static JunctionClearanceResult _measured;

        private static bool HasAncestor(Transform transform, string name)
        {
            for (Transform t = transform.parent; t != null; t = t.parent)
                if (t.name == name) return true;
            return false;
        }

        private static JunctionClearanceResult MeasureMvpRun()
        {
            if (_measured != null) return _measured;
            WithMvpRun(scene =>
            {
                _measured = MeasureFresh(scene);
            });
            return _measured;
        }

        private static JunctionClearanceResult MeasureFresh(Scene scene, IReadOnlyList<JunctionClearanceSurface> sidewalks = null)
        {
            var migration = MigrationReport.Run(scene, File.ReadAllText(MigrationReport.LineageFullPath));
            Assert.That(migration.Import != null && migration.Import.Succeeded, Is.True, string.Join("\n", migration.Failures.ToArray()));
            var model = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(AuthoredRoadModel.FullPath("Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json"))));
            if (sidewalks == null)
            {
                var readFailures = new List<string>();
                sidewalks = SidewalkDeclarations.Read(scene, readFailures);
                Assert.That(readFailures, Is.Empty);
            }
            return JunctionClearance.Measure(scene, migration.Import, model, sidewalks);
        }

        /// <summary>
        /// Ouvre Bootstrap en additif pour un test de scene etrangere ; ferme uniquement ce que ce test a
        /// ouvert, refusant une instance deja ouverte et modifiee (motif de WithMvpRun).
        /// </summary>
        private static void WithBootstrapScene(Action<Scene> body)
        {
            const string bootstrap = "Assets/RoadRage/App/Scenes/Bootstrap.unity";
            var alreadyOpen = SceneManager.GetSceneByPath(bootstrap);
            bool wasOpen = alreadyOpen.IsValid() && alreadyOpen.isLoaded;
            if (wasOpen)
            {
                Assert.That(alreadyOpen.isDirty, Is.False, "Bootstrap est ouvert avec des modifications non sauvegardees.");
            }

            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(bootstrap, OpenSceneMode.Additive);
            try
            {
                body(scene);
            }
            finally
            {
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
            }
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
                if (!wasOpen)
                {
                    EditorSceneManager.CloseScene(scene, !inHierarchy);
                }
                else Assert.That(scene.isDirty, Is.False, "Le test a laisse MVP_Run modifiee en memoire.");
            }
        }
    }
}
