using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public void CoverageIsExactNotSampled()
        {
            float tolerance = JunctionClearance.ReliefSupportToleranceMeters;
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new[] { Rect(0f, 1f, 0f, 2f), Rect(1f, 2f, 0f, 2f) }, tolerance), Is.True, "Deux moities jointives.");
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new[] { Rect(0f, 1f, 0f, 2f), Rect(1.004f, 2f, 0f, 2f) }, tolerance), Is.False, "Fente de 4 mm.");
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new[] { Rect(0f, 2f, 0f, 1f), Rect(0f, 1f, 1f, 2f) }, tolerance), Is.False, "Coin manquant.");
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new[] { Rect(-1f, 3f, -1f, 3f) }, tolerance), Is.True, "Support englobant.");
            Assert.That(JunctionClearance.Covered(Rect(0f, 2f, 0f, 2f), new Vector2[0][], tolerance), Is.False, "Aucun support.");
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

            // Bordures du carrefour : toujours des obstacles, temoins des virages a droite non encore coupes.
            var rightTurns = result.Rows.Where(r => r.Junction == "Intersection_Center_Crossroads" && r.Movement.Contains("(droite)")).ToArray();
            Assert.That(rightTurns, Is.Not.Empty);
            Assert.That(rightTurns.All(r => r.Physical.Obstacle.Contains("/Col_Curb_") && r.Physical.Residual <= 0f), Is.True);

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

        private static JunctionClearanceResult _measured;

        private static JunctionClearanceResult MeasureMvpRun()
        {
            if (_measured != null) return _measured;
            WithMvpRun(scene =>
            {
                var migration = MigrationReport.Run(scene, File.ReadAllText(MigrationReport.LineageFullPath));
                Assert.That(migration.Import != null && migration.Import.Succeeded, Is.True, string.Join("\n", migration.Failures.ToArray()));
                var model = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(AuthoredRoadModel.FullPath("Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json"))));
                var readFailures = new List<string>();
                var sidewalks = SidewalkDeclarations.Read(scene, readFailures);
                Assert.That(readFailures, Is.Empty);
                _measured = JunctionClearance.Measure(scene, migration.Import, model, sidewalks);
            });
            return _measured;
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
            }
        }
    }
}
