using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.49 -- anneaux de giratoire elargis et largeurs revues appliquees. Mesure sur la VRAIE
    /// MVP_Run (ouverte en additif, jamais sauvegardee) : geometrie cible contraignante, residu a
    /// deux gabarits, et donnees V1 inchangees octet pour octet (exception physique a AD-36).
    /// </summary>
    [Category("Geometry")]
    public sealed class Story549RoundaboutWideningTests
    {
        /// <summary>Empreintes pre-changement (v1-regression-5-49/baseline-hashes.txt), jamais recalculees.</summary>
        private const string PreChangeSourceHash = "b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc";

        private const string PreChangeLineageSha256 = "f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4";

        [Test]
        public void TheTwoFootprintResidualMatchesTheGoldenExamples()
        {
            // Profil versionne (1,03 / 4,5 / 0,25) : aucune constante nouvelle.
            var profile = V1RoadModelImporter.ValidationProfile();
            Assert.That(RoundaboutClearance.Residual(4f, 8f, profile), Is.EqualTo(-1.78f).Within(0.01f), "Anneau V2 avant la 5.49.");
            Assert.That(RoundaboutClearance.Residual(3f, 8f, profile), Is.EqualTo(-0.88f).Within(0.01f), "Anneau physique avant la 5.49.");
            Assert.That(RoundaboutClearance.Residual(2f, 10f, profile), Is.EqualTo(1.99f).Within(0.01f), "Cible.");
        }

        [Test]
        public void TheV1SourceAndTheLineageStayByteIdenticalAndNoIdentityIsMintedOrRetired()
        {
            string lineagePath = MigrationReport.LineageFullPath;
            using (var sha = SHA256.Create())
            {
                string hex = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(lineagePath))).Replace("-", string.Empty).ToLowerInvariant();
                Assert.That(hex, Is.EqualTo(PreChangeLineageSha256), "Octets de la lignee committee.");
            }

            string lineage = File.ReadAllText(lineagePath);
            WithMvpRun(scene =>
            {
                var set = V1SourceSet.Extract(scene);
                Assert.That(set.SourceHash, Is.EqualTo(PreChangeSourceHash), "Hash source V1 fraichement extrait.");

                var migration = MigrationReport.Run(set, lineage);
                Assert.That(migration.Succeeded, Is.True, string.Join("\n", migration.Failures.ToArray()));
                Assert.That(migration.LineageJson, Is.EqualTo(lineage), "Lignee re-serialisee par la migration 5.27.");
                var resolution = migration.Import.Lineage;
                Assert.That(resolution.ModelIdMinted, Is.False);
                Assert.That(resolution.Minted, Is.Empty);
                Assert.That(resolution.Retired, Is.Empty);
            });
        }

        [Test]
        public void TrafficV2SourcesNeverReferenceTheNavMesh()
        {
            string root = Path.Combine(Application.dataPath, "RoadRage", "Features", "Vehicles", "Traffic");
            var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
            Assert.That(files, Is.Not.Empty);
            foreach (var file in files)
            {
                string text = File.ReadAllText(file);
                Assert.That(text, Does.Not.Contain("UnityEngine.AI").And.Not.Contain("NavMesh"), file + " : le NavMesh est une donnee V1 authoree (AD-33), jamais une source V2.");
            }
        }

        [Test]
        public void EachRoundaboutMeetsTheBindingTargetGeometryWithAPositiveTwoFootprintResidual()
        {
            // Mesure et assertions scene ouverte : les racines de module vivent dans MVP_Run.
            WithMvpRun(scene => AssertTargetGeometry(AuthoredRoadModel.Run(scene, File.ReadAllText(MigrationReport.LineageFullPath),
                File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)))));
        }

        private static void AssertTargetGeometry(AuthoredRun run)
        {
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            Assert.That(run.Roundabouts.Count, Is.EqualTo(4));

            var import = run.Import;
            var model = run.Compiled;
            foreach (var roundabout in run.Roundabouts)
            {
                var module = roundabout.Module;
                string name = module.Label;

                // Noeuds V1 d'anneau au rayon 6,0 m, jamais deplaces.
                var centre = module.Root.position;
                var ringNodes = module.Nodes.Where(n => n.Label.StartsWith("Ring_", StringComparison.Ordinal)).ToArray();
                Assert.That(ringNodes, Is.Not.Empty, name);
                foreach (var node in ringNodes)
                {
                    Assert.That(Flat(node.Position - centre), Is.EqualTo(6f).Within(0.01f), name + " / " + node.Label);
                }

                // Anneau et continuations : 4,0 / 4,0 sur tout echantillon, axe egal a l'import.
                int curves = 0;
                foreach (var corridor in import.Corridors.Where(c => c.IsRing && c.Module == module))
                {
                    Assert.That(model.TryGetCorridor(import.IdOf(corridor.Key), out EffectiveLaneCorridor compiled), Is.True);
                    AssertRingCurve(name, corridor, compiled.Samples);
                    curves++;
                }

                foreach (var movement in import.Movements.Where(m => m.Role == MovementRole.RoundaboutContinuation && m.Module == module))
                {
                    Assert.That(model.TryGetMovement(import.IdOf(movement.Key), out CompiledJunctionMovement compiled), Is.True);
                    AssertRingCurve(name, movement, compiled.Samples);
                    curves++;
                }

                Assert.That(curves, Is.EqualTo(6), name + " : trois arcs et trois continuations.");

                // Entrees et sorties : interpolees en s/Length entre les largeurs appliquees des extremites (2,0 <-> 4,0).
                int joins = 0;
                foreach (var movement in import.Movements.Where(m => m.Module == module
                    && (m.Role == MovementRole.RoundaboutEntry || m.Role == MovementRole.RoundaboutExit)))
                {
                    float from = AppliedWidth(run, movement.From.SectionKey);
                    float to = AppliedWidth(run, movement.To.SectionKey);
                    Assert.That(from, Is.Not.EqualTo(to), name + " : une entree ou une sortie joint deux largeurs differentes.");
                    Assert.That(model.TryGetMovement(import.IdOf(movement.Key), out CompiledJunctionMovement compiled), Is.True);
                    foreach (var sample in compiled.Samples)
                    {
                        float expected = Mathf.Lerp(from, to, Mathf.Clamp01(sample.SMeters / movement.Curve.Length));
                        Assert.That(sample.HalfWidthLeftMeters, Is.EqualTo(expected).Within(1e-3f), name + " / " + movement.Label);
                        Assert.That(sample.HalfWidthRightMeters, Is.EqualTo(expected).Within(1e-3f), name + " / " + movement.Label);
                    }

                    joins++;
                }

                Assert.That(joins, Is.EqualTo(6), name + " : trois entrees et trois sorties.");

                // Physique, mesuree sur les colliders.
                Assert.That(roundabout.IslandRadius, Is.LessThanOrEqualTo(1.75f), name + " : ilot");
                Assert.That(roundabout.PavedRadius, Is.GreaterThanOrEqualTo(10.25f), name + " : pave");
                Assert.That(roundabout.NearestObstacle == null || roundabout.NearestObstacleRadius >= 10.25f, Is.True,
                    name + " : aucun collider non-chaussee en deca de 10,25 m (" + roundabout.NearestObstacle + " a " + roundabout.NearestObstacleRadius + " m).");

                // Preuve supplementaire, publiee par instance.
                Assert.That(roundabout.EnvelopeResidual, Is.GreaterThan(0f), name + " : residu V2");
                Assert.That(roundabout.PhysicalResidual, Is.GreaterThan(0f), name + " : residu physique");
                Assert.That(run.ReportText, Does.Contain("| " + name + " `" + module.Key + "` | "));
            }

            Assert.That(run.ReportText, Does.Contain("| Importee g / d (m) | Appliquee g / d (m) |"));

            // Frontiere de carrefour : elle enveloppe les largeurs APPLIQUEES (position +/- max(g, d),
            // regle de l'importeur) ; un carrefour non elargi garde sa frontiere importee a l'identique.
            foreach (var junction in import.Junctions)
            {
                var id = import.IdOf(junction.Key);
                var record = run.Source.Junctions.Single(j => j.Id == id);
                var box = new Bounds(record.Boundary.Center, 2f * record.Boundary.Extents);
                box.Expand(1e-3f);
                foreach (var movement in run.Source.Movements.Where(m => m.JunctionId == id))
                {
                    foreach (var sample in movement.Samples)
                    {
                        float reach = Mathf.Max(sample.HalfWidthLeftMeters, sample.HalfWidthRightMeters);
                        Assert.That(box.Contains(sample.Position - Vector3.one * reach) && box.Contains(sample.Position + Vector3.one * reach), Is.True,
                            junction.Module.Label + " : un echantillon applique deborde la frontiere du carrefour.");
                    }
                }

                if (junction.Module.Kind != V1ModuleKind.Roundabout)
                {
                    var imported = import.Source.Junctions.Single(j => j.Id == id).Boundary;
                    Assert.That(record.Boundary.Center, Is.EqualTo(imported.Center), junction.Module.Label);
                    Assert.That(record.Boundary.Extents, Is.EqualTo(imported.Extents), junction.Module.Label);
                }
            }
        }

        private static void AssertRingCurve(string name, ImportedCurve imported, System.Collections.Generic.IReadOnlyList<RoadCurveSample> compiled)
        {
            Assert.That(compiled.Count, Is.EqualTo(imported.Samples.Length), name);
            for (int i = 0; i < compiled.Count; i++)
            {
                Assert.That(compiled[i].HalfWidthLeftMeters, Is.EqualTo(4f), name + " : gauche, echantillon " + i);
                Assert.That(compiled[i].HalfWidthRightMeters, Is.EqualTo(4f), name + " : droite, echantillon " + i);
                Assert.That(compiled[i].Position, Is.EqualTo(imported.Samples[i].Position), name + " : axe, echantillon " + i);
                Assert.That(compiled[i].Tangent, Is.EqualTo(imported.Samples[i].Tangent), name + " : tangente, echantillon " + i);
            }
        }

        [Test]
        public void AnUnsupportedColliderWithinReachOfThePavedDiscIsAHardFailure()
        {
            // Scene untitled du runner seulement : jamais d'objet ajoute a une scene enregistree (MVP_Run).
            Assume.That(SceneManager.GetActiveScene().path, Is.Empty, "La scene active du runner doit etre untitled.");
            var root = new GameObject("Giratoire_Synthetique");
            try
            {
                var roadway = new GameObject(RoundaboutClearance.RoadwayPrefix + "_Test").AddComponent<BoxCollider>();
                roadway.transform.SetParent(root.transform, false);
                roadway.size = new Vector3(30f, 0.2f, 30f);
                roadway.center = new Vector3(0f, -0.1f, 0f);
                var island = new GameObject(RoundaboutClearance.IslandName).AddComponent<MeshCollider>();
                island.transform.SetParent(root.transform, false);
                island.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                island.convex = true;
                island.transform.localScale = new Vector3(3f, 0.1f, 3f);
                island.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                var bollard = new GameObject("Borne").AddComponent<SphereCollider>();
                bollard.transform.SetParent(root.transform, false);
                bollard.transform.localPosition = new Vector3(5f, 0.5f, 0f);

                var module = new V1Module { Label = root.name, Kind = V1ModuleKind.Roundabout, Root = root.transform };
                var failures = new System.Collections.Generic.List<string>();
                Assert.That(RoundaboutClearance.MeasurePhysical(module, new RoundaboutMeasurement(), failures), Is.False);
                Assert.That(failures.Single(), Does.Contain("Collider non supporte").And.Contain("Borne"));

                // Hors de portee du pave (15 m), le meme collider ne peut pas abaisser r_out : ignore.
                bollard.transform.localPosition = new Vector3(40f, 0.5f, 0f);
                failures.Clear();
                var measurement = new RoundaboutMeasurement();
                Assert.That(RoundaboutClearance.MeasurePhysical(module, measurement, failures), Is.True, string.Join("\n", failures.ToArray()));
                Assert.That(measurement.IslandRadius, Is.EqualTo(island.sharedMesh.bounds.extents.x * 3f).Within(0.01f));
                Assert.That(measurement.PavedRadius, Is.EqualTo(15f).Within(0.02f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static float AppliedWidth(AuthoredRun run, string sectionKey)
        {
            var decision = run.Decisions.Widths.Single(w => w.SubjectKey == sectionKey);
            Assert.That(decision.HalfWidthLeftMeters, Is.EqualTo(decision.HalfWidthRightMeters), sectionKey);
            return decision.HalfWidthLeftMeters;
        }

        private static float Flat(Vector3 v)
        {
            return new Vector2(v.x, v.z).magnitude;
        }

        /// <summary>Harnais 5.27 : garde dirty, ouverture additive, fermeture sans sauvegarde.</summary>
        private static void WithMvpRun(Action<Scene> body)
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
            bool inHierarchy = alreadyOpen.IsValid();
            bool wasOpen = inHierarchy && alreadyOpen.isLoaded;
            if (wasOpen)
            {
                Assert.That(alreadyOpen.isDirty, Is.False,
                    "MVP_Run est ouvert avec des modifications non sauvegardees : la mesure decrirait un etat non sauve, pas la scene du disque.");
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
