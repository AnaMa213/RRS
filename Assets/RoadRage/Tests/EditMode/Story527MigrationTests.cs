using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
    /// Story 5.27 -- importeur V1, validateur et rapport de migration mesure. Tout tourne sur la
    /// VRAIE authoring de MVP_Run (ouverte en additif, jamais sauvegardee) ; seuls les echecs durs
    /// d'ensemble source utilisent des racines temporaires hors scene. Les effectifs asserts ici sont ceux de
    /// l'instantane MVP_Run lie au rapport committe : l'importeur n'en code aucun.
    /// </summary>
    public sealed class Story527MigrationTests
    {
        private const string MvpRunScenePath = MigrationReport.ScenePath;

        // ================================================================== instantane nominal

        [Test]
        public void TheBoundMvpRunSnapshotImportsWithEverySourceItemDisposed()
        {
            var run = RunOnMvpRun(CommittedLineage());
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));

            var set = run.SourceSet;
            var import = run.Import;
            var source = import.Source;

            // Instantane lie (audit statique du 2026-09-22) : assertions de test, pas regles d'importeur.
            Assert.That(set.Modules.Count, Is.EqualTo(25));
            Assert.That(set.Nodes.Count, Is.EqualTo(204));
            Assert.That(source.Sections.Length, Is.EqualTo(28));
            Assert.That(source.Corridors.Length, Is.EqualTo(44));
            Assert.That(source.Junctions.Length, Is.EqualTo(9));
            Assert.That(source.Movements.Length, Is.EqualTo(72));
            Assert.That(source.Portals.Length, Is.EqualTo(8));
            Assert.That(source.Portals.Count(p => p.Role == PortalRole.Entry), Is.EqualTo(4));
            Assert.That(source.Portals.Count(p => p.Role == PortalRole.Exit), Is.EqualTo(4));

            // Condition de reouverture 5.25 (deferred-work) : aucune boucle sur un meme corridor.
            Assert.That(source.Movements.All(m => m.FromCorridorId != m.ToCorridorId), Is.True);
            Assert.That(source.Connections.All(c => c.FromCorridorId != c.ToCorridorId), Is.True);

            // Chaque element source a exactement une disposition typee.
            Assert.That(Dispositions(import, SourceItemKind.Node), Is.EqualTo(set.Nodes.Count));
            Assert.That(Dispositions(import, SourceItemKind.Edge), Is.EqualTo(set.Edges.Count));
            Assert.That(Dispositions(import, SourceItemKind.TurnWeight), Is.EqualTo(set.Edges.Count - set.Joins.Count));
            Assert.That(Dispositions(import, SourceItemKind.ConnectorMatch), Is.EqualTo(set.Joins.Count));
            Assert.That(Dispositions(import, SourceItemKind.PortalRole), Is.EqualTo(8));
            Assert.That(import.Dispositions.Select(d => d.Item + "|" + d.SourceKey).Distinct().Count(), Is.EqualTo(import.Dispositions.Count),
                "Aucun element dispose deux fois.");
            Assert.That(import.Dispositions.All(d => import.IdOf(d.TargetKey) != RoadId.None), Is.True,
                "Chaque disposition vise un enregistrement vivant du modele.");
        }

        [Test]
        public void JunctionsCarryTheExplicitMovementsOfTheSnapshotWithoutCycleInference()
        {
            var import = RunOnMvpRun(CommittedLineage()).Import;

            var crossroads = import.Junctions.Single(j => j.Feature == JunctionFeature.Crossroads);
            Assert.That(crossroads.Movements.Count, Is.EqualTo(12));

            var tees = import.Junctions.Where(j => j.Feature == JunctionFeature.TJunction).ToList();
            Assert.That(tees.Count, Is.EqualTo(4));
            Assert.That(tees.All(j => j.Movements.Count == 6), Is.True);

            var roundabouts = import.Junctions.Where(j => j.Feature == JunctionFeature.Roundabout).ToList();
            Assert.That(roundabouts.Count, Is.EqualTo(4));
            foreach (var roundabout in roundabouts)
            {
                Assert.That(roundabout.Movements.Count(m => m.Role == MovementRole.RoundaboutEntry), Is.EqualTo(3), roundabout.Module.Label);
                Assert.That(roundabout.Movements.Count(m => m.Role == MovementRole.RoundaboutExit), Is.EqualTo(3), roundabout.Module.Label);
                Assert.That(roundabout.Movements.Count(m => m.Role == MovementRole.RoundaboutContinuation), Is.EqualTo(3), roundabout.Module.Label);
            }

            // Chaque poids preserve resout vers un mouvement nomme, polarite explicite.
            foreach (var movement in import.Movements)
            {
                Assert.That(movement.Label, Is.Not.Empty);
                Assert.That(movement.KeyEdge.Weight, Is.GreaterThan(0f));
            }

            Assert.That(import.Dispositions.Where(d => d.Kind == DispositionKind.RoutePreference).All(d => d.Detail.Contains("plus grand = prefere")), Is.True);
        }

        [Test]
        public void EveryEntryPortalReachesAtLeastOneExitOnTheV2Topology()
        {
            var run = RunOnMvpRun(CommittedLineage());

            Assert.That(run.Reachability.Count, Is.EqualTo(4));
            Assert.That(run.EveryEntryReachesAnExit, Is.True, "Contrat AD-47 et garde V1 Story510 : chaque entree atteint au moins une sortie.");
            Assert.That(run.ReportText, Does.Contain("Resultat : **respecte**"));
        }

        [Test]
        public void MissingSemanticsBecomeAuthoringTasksAndTheModelSaysItDoesNotCompile()
        {
            var run = RunOnMvpRun(CommittedLineage());
            var source = run.Import.Source;

            Assert.That(source.Controls, Is.Empty, "Aucun controle invente.");
            Assert.That(source.ConflictZones, Is.Empty);
            Assert.That(source.SignalPlans, Is.Empty);
            Assert.That(source.Adjacencies, Is.Empty, "Voies opposees : jamais d'adjacence.");

            Assert.That(run.Compiled, Is.Null, "Aucune version pour un modele qui ne passe pas Compile.");
            Assert.That(run.StructuralIssues.Count, Is.EqualTo(72));
            Assert.That(run.StructuralIssues.All(i => i.Code == RoadModelValidationCode.MovementControlCoverageInvalid), Is.True);
            Assert.That(run.GeometricIssues, Is.Empty, "Geometrie, AD-48 et coutures valides sur la carte reelle.");
            Assert.That(run.ReportText, Does.Contain("absente -- Compile refuse : 72 erreurs"));
            Assert.That(run.ReportText, Does.Contain("Le modele ne passe pas encore la validation"));

            foreach (var junction in run.Import.Junctions)
            {
                foreach (var category in new[] { "Controle", "Ligne", "Conflit", "Signal" })
                {
                    Assert.That(run.Import.Tasks.Any(t => t.Category == category && t.SubjectKey == junction.Key), Is.True,
                        category + " attendu pour " + junction.Module.Label);
                }
            }

            Assert.That(run.Import.Tasks.Count(t => t.Category == "Largeur"), Is.EqualTo(source.Sections.Length + source.Junctions.Length));
        }

        [Test]
        public void MeasurementsPublishDistributionsAndOnlyTheApprovedExceptionCategory()
        {
            var run = RunOnMvpRun(CommittedLineage());

            string[] expected =
            {
                "Ecart de connecteur", "Angle de connecteur", "Couture : ecart de position", "Couture : ecart de tangente",
                "Couture de mouvement : ecart de demi-largeur", "Corde compilee", "Degagement lateral", "Derive de noeud", "Derive de portail"
            };
            foreach (var title in expected)
            {
                Assert.That(run.Metrics.Any(m => m.Title.StartsWith(title, StringComparison.Ordinal) && m.Values.Count > 0), Is.True, title);
            }

            // Les exceptions ne portent que sur les graines lissees par un virage.
            foreach (var metric in run.Metrics)
            {
                foreach (var value in metric.Values.Where(v => v.Class == MeasureClass.JustifiedException))
                {
                    Assert.That(metric.Title, Does.StartWith("Derive de noeud"));
                    Assert.That(value.Subject, Does.Contain("(graine)"));
                    Assert.That(value.Subject, Does.Contain("(droite)").Or.Contain("(gauche)"));
                }
            }

            var chord = run.Metrics.Single(m => m.Title.StartsWith("Corde", StringComparison.Ordinal));
            Assert.That(chord.Values.All(v => v.Value <= 0.5f * V1RoadModelImporter.ChordToleranceMeters + 1e-6f), Is.True,
                "Critere 5.26 : borne par construction a la moitie de la tolerance.");
            Assert.That(run.ReportText, Does.Contain("| p50 | p95 | max |"));
        }

        [Test]
        public void AMeasurementAboveItsThresholdIsListedAsADeviationWithTheThresholdUnchanged()
        {
            RunOnMvpRunWith(delegate(Scene scene)
            {
                var set = V1SourceSet.Extract(scene);
                var merged = set.Nodes.First(n => n.Module.IsJunction && n.IsIncomingConnector);
                merged.Position += merged.Forward * 0.3f;

                var run = MigrationReport.Run(set, CommittedLineage());
                Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));

                var drift = run.Metrics.Single(m => m.Title.StartsWith("Derive de noeud", StringComparison.Ordinal));
                var deviation = drift.Values.Single(v => v.Class == MeasureClass.DeviationToCorrect);
                Assert.That(deviation.Value, Is.EqualTo(0.3f).Within(1e-3f));
                Assert.That(deviation.Subject, Does.Contain(merged.Label));
                Assert.That(drift.Threshold, Is.EqualTo("<= 0.1000"), "Aucun seuil relache.");
                Assert.That(run.ReportText, Does.Contain("### Deviations a corriger (authoring)"));
                Assert.That(run.ReportText, Does.Contain("| 0.3000 | <= 0.1000 |"));
            });
        }

        // ================================================================== lignee

        [Test]
        public void ReimportingAnUnchangedSourceMintsNoIdentityAndIsByteIdentical()
        {
            var first = RunOnMvpRun(null);
            Assert.That(first.Succeeded, Is.True, string.Join("\n", first.Failures.ToArray()));
            Assert.That(first.Import.Lineage.ModelIdMinted, Is.True);

            var second = RunOnMvpRun(first.LineageJson);
            var third = RunOnMvpRun(first.LineageJson);

            Assert.That(second.Import.Lineage.Minted, Is.Empty);
            Assert.That(second.Import.Lineage.Retired, Is.Empty);
            Assert.That(second.Import.Lineage.Preserved.Count, Is.EqualTo(first.Import.Lineage.Minted.Count));
            Assert.That(second.LineageJson, Is.EqualTo(first.LineageJson));
            Assert.That(third.ReportText, Is.EqualTo(second.ReportText), "Deux imports de la meme source : memes octets.");
            Assert.That(second.Import.Source.ModelId, Is.EqualTo(first.Import.Source.ModelId));
        }

        [Test]
        public void AnEntityAbsentFromTheSourceIsTombstonedAndNeverRecycled()
        {
            const string ghostKey = "corridor:fantome";
            const string ghostId = "0123456789abcdef0123456789abcdef";
            string lineage = CommittedLineage().Replace("\"Records\": [",
                "\"Records\": [\n        {\"Key\": \"" + ghostKey + "\", \"Kind\": \"Corridor\", \"Id\": \"" + ghostId + "\"},");

            var run = RunOnMvpRun(lineage);
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));

            var retired = run.Import.Lineage.Retired.Single();
            Assert.That(retired.Key, Is.EqualTo(ghostKey));
            Assert.That(run.Import.Source.Manifest.TombstonedIds, Has.Member(RoadId.Parse(ghostId)));
            Assert.That(run.LineageJson, Does.Contain(ghostId), "Le tombstone est persiste.");
            Assert.That(run.ReportText, Does.Contain("`" + ghostKey + "` | Corridor | `" + ghostId + "`"));

            // Une identite tombstonee n'est jamais reattribuee.
            Assert.That(run.Import.IdByKey.Values, Has.No.Member(RoadId.Parse(ghostId)));
        }

        [Test]
        public void AnEntityMissingFromTheLineageReceivesANewIdentityAndIsReportedNew()
        {
            string committed = CommittedLineage();
            var match = Regex.Match(committed, "\\{\\s*\"Key\": \"(movement:[^\"]+)\",\\s*\"Kind\": \"Movement\",\\s*\"Id\": \"([0-9a-f]{32})\"\\s*\\},?\\s*");
            Assert.That(match.Success, Is.True);
            string lineage = committed.Remove(match.Index, match.Length);
            if (!committed.Substring(match.Index, match.Length).TrimEnd().EndsWith(",", StringComparison.Ordinal))
            {
                lineage = Regex.Replace(lineage, ",(\\s*\\])", "$1");
            }

            var run = RunOnMvpRun(lineage);
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));

            string key = match.Groups[1].Value;
            Assert.That(run.Import.Lineage.Minted, Is.EqualTo(new[] { key }));
            Assert.That(run.Import.IdOf(key), Is.Not.EqualTo(RoadId.Parse(match.Groups[2].Value)));
            Assert.That(run.ReportText, Does.Contain("| `" + key + "` | `" + run.Import.IdOf(key) + "` |"));
        }

        [Test]
        public void TwoEntitiesProducingTheSameLineageKeyAreAHardFailure()
        {
            var registry = new LineageKeyRegistry();
            var failures = new List<string>();

            Assert.That(registry.TryRegister("corridor:a>b", RoadRecordKind.Corridor, "voie A", failures), Is.True);
            Assert.That(registry.TryRegister("corridor:a>b", RoadRecordKind.Corridor, "voie B", failures), Is.False);
            Assert.That(failures.Single(), Does.Contain("voie A").And.Contain("voie B"));
        }

        [Test]
        public void LineageKeysNeverUseNamesHierarchyOrGeneratedIdentities()
        {
            var import = RunOnMvpRun(CommittedLineage()).Import;
            var labels = new HashSet<string>(import.SourceSet.Nodes.Select(n => n.Label).Concat(import.SourceSet.Modules.Select(m => m.Label)));

            foreach (var key in import.IdByKey.Keys)
            {
                string body = key.Substring(key.IndexOf(':') + 1);
                foreach (var part in body.Split('>'))
                {
                    string identity = part.EndsWith(":Entry", StringComparison.Ordinal) || part.EndsWith(":Exit", StringComparison.Ordinal)
                        ? part.Substring(0, part.LastIndexOf(':'))
                        : part;
                    Assert.That(identity, Does.StartWith("GlobalObjectId_V1-"), key);
                    Assert.That(labels.Contains(identity), Is.False);
                }
            }
        }

        // ================================================================== ensemble source

        [Test]
        public void AnUnrecognisedModulePrefabIsAHardFailureAndNothingIsProduced()
        {
            RunOnMvpRunWith(delegate(Scene scene)
            {
                var withoutTunnel = V1SourceSet.RecognisedPrefabs
                    .Where(p => p.Value != V1ModuleKind.TunnelPortal)
                    .ToDictionary(p => p.Key, p => p.Value);

                var set = V1SourceSet.Extract(scene, withoutTunnel);
                var run = MigrationReport.Run(set, CommittedLineage());

                Assert.That(run.Succeeded, Is.False);
                Assert.That(run.Failures.Any(f => f.Contains("Greybox_TunnelPortal.prefab") && f.Contains("non reconnu")), Is.True);
                Assert.That(run.ReportText, Is.Null);
                Assert.That(run.LineageJson, Is.Null);
            });
        }

        [Test]
        public void ALooseLaneNodeOutsideAnyModuleIsAHardFailure()
        {
            // Racines explicites, objets HideAndDontSave : aucune scene creee, rien de serialise ni
            // de sali dans la scene active. L'ensemble source est exactement ce qui vit sous root.
            var root = EditorUtility.CreateGameObjectWithHideFlags("LaneGraphRoot", HideFlags.HideAndDontSave, typeof(LaneGraph));
            try
            {
                var loose = EditorUtility.CreateGameObjectWithHideFlags("LooseNode", HideFlags.HideAndDontSave, typeof(LaneNode));
                loose.transform.SetParent(root.transform);

                var set = V1SourceSet.Extract(new[] { root }, string.Empty, V1SourceSet.RecognisedPrefabs);
                Assert.That(set.IsValid, Is.False);
                Assert.That(set.Failures.Any(f => f.Contains("hors de tout module") && f.Contains("LooseNode")), Is.True, string.Join("\n", set.Failures.ToArray()));

                var run = MigrationReport.Run(set, null);
                Assert.That(run.ReportText, Is.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TheSourceHashChangesWhenTheSourceChanges()
        {
            RunOnMvpRunWith(delegate(Scene scene)
            {
                var set = V1SourceSet.Extract(scene);
                var again = V1SourceSet.Extract(scene);
                Assert.That(again.SourceHash, Is.EqualTo(set.SourceHash), "Deux extractions de la meme scene : memes octets canoniques.");
                Assert.That(V1SourceSet.ComputeSourceHash(set), Is.EqualTo(set.SourceHash), "Le hash publie par l'extraction est celui de son texte canonique.");

                // Mutations en memoire de l'ensemble EXTRAIT, jamais de la scene : la pose d'un noeud
                // et les poids authores font partie de la source, le decor deplace non.
                set.Nodes[0].Position += new Vector3(1f, 0f, 0f);
                string moved = V1SourceSet.ComputeSourceHash(set);
                Assert.That(moved, Is.Not.EqualTo(set.SourceHash), "Un noeud deplace d'un metre change le hash de source.");

                var edge = set.Edges.First(e => !e.IsConnectorJoin);
                edge.Weight += 0.5f;
                Assert.That(V1SourceSet.ComputeSourceHash(set), Is.Not.EqualTo(moved), "Un poids de virage change le hash de source.");

                // Le rapport publie ce hash dans sa liaison : une source qui a change, meme d'un seul
                // noeud deplace, fait refuser le rapport precedent par Verify sur `source-hash` au
                // prochain import frais. C'est pourquoi le rapport committe doit etre regenere.
            });
        }

        // ================================================================== liaison du rapport

        [Test]
        public void TheCommittedReportAndLineageMatchAFreshImport()
        {
            // Chemins absolus resolus depuis la racine du projet (LineageFullPath / ReportFullPath) :
            // la comparaison ne depend pas du repertoire courant du runner.
            string lineage = File.ReadAllText(MigrationReport.LineageFullPath);
            string report = File.ReadAllText(MigrationReport.ReportFullPath);
            var run = RunOnMvpRun(lineage);

            Assert.That(MigrationReport.Verify(report, lineage, run.Binding), Is.Empty,
                "Rapport perime ou retouche : relancer RoadRage/Traffic V2/Migrer MVP_Run.");
            Assert.That(run.LineageJson, Is.EqualTo(lineage));
            Assert.That(run.ReportText, Is.EqualTo(report));
        }

        [Test]
        public void AStaleOrHandEditedReportIsRejectedNeverRepaired()
        {
            var run = RunOnMvpRun(CommittedLineage());
            Assert.That(MigrationReport.Verify(run.ReportText, run.LineageJson, run.Binding), Is.Empty);

            string edited = run.ReportText.Replace("Resultat : **respecte**", "Resultat : **respecte (edite)**");
            Assert.That(MigrationReport.Verify(edited, run.LineageJson, run.Binding).Single(), Does.Contain("body-hash"));

            string otherLineage = run.LineageJson + " ";
            Assert.That(MigrationReport.Verify(run.ReportText, otherLineage, run.Binding).Single(), Does.Contain("lineage-hash"));

            var moved = new MigrationBinding
            {
                SourceHash = new string('0', 64),
                ImporterVersion = run.Binding.ImporterVersion,
                CompilerSchemaVersion = run.Binding.CompilerSchemaVersion,
                ModelId = run.Binding.ModelId,
                LineageHash = run.Binding.LineageHash
            };
            Assert.That(MigrationReport.Verify(run.ReportText, run.LineageJson, moved).Single(), Does.Contain("source-hash"));

            string detached = run.ReportText.Substring(run.ReportText.IndexOf("-->\n", StringComparison.Ordinal) + 4);
            Assert.That(MigrationReport.Verify(detached, run.LineageJson, run.Binding).Single(), Does.Contain("detache"));
        }

        // ================================================================== surcharge 5.26 et gardes

        [Test]
        public void TheMeasuringBuildOverloadIsBehaviourallyIdenticalToBuild()
        {
            var points = new Vector3[10];
            var ups = new Vector3[10];
            var widths = new float[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = 0.5f * Mathf.PI * i / 9f;
                points[i] = new Vector3(12f, 0f, 0f) + new Vector3(-12f * Mathf.Cos(angle), 0f, 12f * Mathf.Sin(angle));
                ups[i] = Vector3.up;
                widths[i] = 2f;
            }

            var plain = RoadCurveBuilder.Build(points, ups, widths, widths, 0.05f);
            float deviation;
            var measured = RoadCurveBuilder.Build(points, ups, widths, widths, 0.05f, out deviation);

            Assert.That(measured.Length, Is.EqualTo(plain.Length));
            for (int i = 0; i < plain.Length; i++)
            {
                Assert.That(measured[i].SMeters, Is.EqualTo(plain[i].SMeters));
                Assert.That(measured[i].Position, Is.EqualTo(plain[i].Position));
                Assert.That(measured[i].Tangent, Is.EqualTo(plain[i].Tangent));
                Assert.That(measured[i].Up, Is.EqualTo(plain[i].Up));
                Assert.That(measured[i].CurvaturePerMeter, Is.EqualTo(plain[i].CurvaturePerMeter));
                Assert.That(measured[i].HalfWidthLeftMeters, Is.EqualTo(plain[i].HalfWidthLeftMeters));
                Assert.That(measured[i].HalfWidthRightMeters, Is.EqualTo(plain[i].HalfWidthRightMeters));
            }

            Assert.That(deviation, Is.GreaterThan(0f), "Un arc subdivise a un ecart de corde non nul.");
            Assert.That(deviation, Is.LessThanOrEqualTo(0.025f), "Critere 5.26 : au plus la moitie de la tolerance.");
        }

        [Test]
        public void TheMigrationToolingIsEditorOnly()
        {
            foreach (var path in Directory.GetFiles("Assets/RoadRage/Features/Vehicles/Traffic/Migration", "*.cs"))
            {
                Assert.That(File.ReadAllText(path), Does.StartWith("#if UNITY_EDITOR"), path);
            }
        }

        // ================================================================== correctifs de revue

        [Test]
        public void EveryBindingFieldChangeIsRejectedOnItsOwn()
        {
            var run = RunOnMvpRun(CommittedLineage());
            var fields = new[] { "importer-version", "compiler-schema-version", "model-id" };
            for (int i = 0; i < fields.Length; i++)
            {
                var moved = new MigrationBinding
                {
                    SourceHash = run.Binding.SourceHash,
                    ImporterVersion = run.Binding.ImporterVersion + (i == 0 ? 1 : 0),
                    CompilerSchemaVersion = run.Binding.CompilerSchemaVersion + (i == 1 ? 1 : 0),
                    ModelId = i == 2 ? new string('0', 32) : run.Binding.ModelId,
                    LineageHash = run.Binding.LineageHash
                };
                Assert.That(MigrationReport.Verify(run.ReportText, run.LineageJson, moved).Single(), Does.Contain(fields[i]));
            }

            Assert.That(MigrationReport.Verify(run.ReportText, run.LineageJson, null).Single(), Does.Contain("Liaison attendue absente"));
        }

        [Test]
        public void ATombstonedKeyThatComesBackGetsANewIdentityAndTheTombstoneStays()
        {
            string key;
            string oldId;
            string lineage = RemoveFirstMovementRecord(CommittedLineage(), out key, out oldId);
            lineage = lineage.Replace("\"Tombstones\": []",
                "\"Tombstones\": [{\"Key\": \"" + key + "\", \"Kind\": \"Movement\", \"Id\": \"" + oldId + "\"}]");

            var run = RunOnMvpRun(lineage);
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));

            Assert.That(run.Import.Lineage.Minted, Is.EqualTo(new[] { key }));
            Assert.That(run.Import.IdOf(key), Is.Not.EqualTo(RoadId.Parse(oldId)), "Une identite retiree n'est jamais recyclee.");
            Assert.That(run.Import.Source.Manifest.TombstonedIds, Has.Member(RoadId.Parse(oldId)));
            Assert.That(run.Import.Lineage.Next.Tombstones.Select(t => t.Id), Has.Member(RoadId.Parse(oldId)));
        }

        [Test]
        public void ReachabilityFollowsTheTopologyAndReportsAViolation()
        {
            var source = RunOnMvpRun(CommittedLineage()).Import.Source;
            Assert.That(MigrationReport.ComputeReachability(source).Values.All(exits => exits.Count == 4), Is.True);

            // Partiel : plus aucun mouvement ne mene au corridor d'une sortie.
            var exit = source.Portals.First(p => p.Role == PortalRole.Exit);
            string exitName = exit.Label + " " + exit.Id;
            source.Movements = source.Movements.Where(m => m.ToCorridorId != exit.CorridorId).ToArray();
            var partial = MigrationReport.ComputeReachability(source);
            Assert.That(partial.Values.All(exits => exits.Count == 3 && !exits.Contains(exitName)), Is.True);

            // Negatif : aucune entree n'atteint de sortie, le contrat est viole.
            source.Movements = new JunctionMovement[0];
            var run = new MigrationRun();
            foreach (var pair in MigrationReport.ComputeReachability(source))
            {
                Assert.That(pair.Value, Is.Empty, pair.Key);
                run.Reachability.Add(pair.Key, pair.Value);
            }

            Assert.That(run.EveryEntryReachesAnExit, Is.False);
        }

        [Test]
        public void AFailedRunWritesNothingAndASucceededRunWritesBothFiles()
        {
            string directory = Path.GetFullPath(Path.Combine("Temp", "Story527WriteTest"));
            Directory.CreateDirectory(directory);
            string lineageFile = Path.Combine(directory, "lineage.json");
            string reportFile = Path.Combine(directory, "report.md");
            try
            {
                File.WriteAllText(lineageFile, "ancienne lignee");
                File.WriteAllText(reportFile, "ancien rapport");

                var failed = new MigrationRun();
                failed.Failures.Add("echec simule");
                string error;
                Assert.That(MigrationReport.TryWrite(failed, lineageFile, reportFile, out error), Is.False);
                Assert.That(error, Does.Contain("echec simule"));
                Assert.That(File.ReadAllText(lineageFile), Is.EqualTo("ancienne lignee"));
                Assert.That(File.ReadAllText(reportFile), Is.EqualTo("ancien rapport"));

                var run = RunOnMvpRun(CommittedLineage());
                Assert.That(MigrationReport.TryWrite(run, lineageFile, reportFile, out error), Is.True, error);
                Assert.That(File.ReadAllText(lineageFile), Is.EqualTo(run.LineageJson));
                Assert.That(File.ReadAllText(reportFile), Is.EqualTo(run.ReportText));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void ABlankOrCorruptedLineageIsAHardFailureAndNothingIsProduced()
        {
            var blank = RunOnMvpRun("   ");
            Assert.That(blank.Succeeded, Is.False);
            Assert.That(blank.Failures.Single(), Does.Contain("Lignee vide"));
            Assert.That(blank.ReportText, Is.Null);

            string committed = CommittedLineage();
            string liveId = Regex.Match(committed, "\"Id\": \"([0-9a-f]{32})\"").Groups[1].Value;
            string duplicated = committed.Replace("\"Tombstones\": []",
                "\"Tombstones\": [{\"Key\": \"corridor:double\", \"Kind\": \"Corridor\", \"Id\": \"" + liveId + "\"}]");
            var run = RunOnMvpRun(duplicated);
            Assert.That(run.Succeeded, Is.False);
            Assert.That(run.Failures.Single(), Does.Contain("porte deux fois"));
            Assert.That(run.ReportText, Is.Null);
        }

        [TestCase("FormatInconnu", "format absent ou inconnu")]
        [TestCase("GenreInconnu", "genre inconnu")]
        [TestCase("IdIllisible", "identifiant illisible")]
        [TestCase("CleEnDouble", "cle en double")]
        public void EveryLineageRefusalGateIsAHardFailure(string corruption, string reason)
        {
            var run = RunOnMvpRun(CorruptLineage(corruption));

            Assert.That(run.Succeeded, Is.False);
            Assert.That(run.Failures.Single(), Does.Contain(reason));
            Assert.That(run.LineageJson, Is.Null, "Une lignee refusee n'est jamais reserialisee.");
            Assert.That(run.ReportText, Is.Null, "Rien n'est produit : le refus est dur, jamais repare.");
        }

        [Test]
        public void ASeedDisplacedOffItsApproachAxisIsADeviationNotAnException()
        {
            RunOnMvpRunWith(delegate(Scene scene)
            {
                var set = V1SourceSet.Extract(scene);
                var seed = set.Nodes.First(n => n.Module.Kind == V1ModuleKind.TJunction && n.Role == LaneNodeRole.Normal && n.AuthoredSuccessorCount >= 2);
                seed.Position += Vector3.Cross(Vector3.up, seed.Forward).normalized * 1f;

                var run = MigrationReport.Run(set, CommittedLineage());
                Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));

                string subject = seed.Module.Label + " / " + seed.Label + " (graine)";
                var drift = run.Metrics.Single(m => m.Title.StartsWith("Derive de noeud", StringComparison.Ordinal));
                var seedValues = drift.Values.Where(v => v.Subject.StartsWith(subject, StringComparison.Ordinal) && v.Value > 0.1f).ToList();
                Assert.That(seedValues, Is.Not.Empty);
                Assert.That(seedValues.All(v => v.Class == MeasureClass.DeviationToCorrect), Is.True,
                    "Le lissage n'excuse pas une graine sortie de l'axe de son approche.");
            });
        }

        [Test]
        public void ValidationNeverCountsAGeometricFailureTwice()
        {
            var source = RunOnMvpRun(CommittedLineage()).Import.Source;

            // Controles explicites (Uncontrolled, choix du test) : la source devient structurellement propre.
            source.Controls = source.Junctions.Select(j => new JunctionControl
            {
                Id = RoadId.New(),
                JunctionId = j.Id,
                Kind = JunctionControlKind.Uncontrolled,
                ControlledMovementIds = source.Movements.Where(m => m.JunctionId == j.Id).Select(m => m.Id).ToArray()
            }).ToArray();

            var structural = new List<RoadModelValidationIssue>();
            var geometric = new List<RoadModelValidationIssue>();
            Assert.That(MigrationReport.ValidateSource(source, structural, geometric), Is.Not.Null,
                "Controles authores : la geometrie migree compile, seul Compile emet la version.");

            source.Corridors[0].Samples[1].HalfWidthLeftMeters = 0f;
            structural.Clear();
            geometric.Clear();
            Assert.That(MigrationReport.ValidateSource(source, structural, geometric), Is.Null);
            Assert.That(structural, Is.Empty);
            Assert.That(geometric, Is.Not.Empty);
            Assert.That(geometric.Select(i => i.Code + "|" + i.SubjectId + "|" + i.Message).Distinct().Count(), Is.EqualTo(geometric.Count));
        }

        [Test]
        public void TheValidationSplitNamesTheGeometricCodesOnly()
        {
            // Source minimale ecrite a la main : une section, un corridor droit de 30 m a deux
            // echantillons, aucun carrefour. Un seul defaut : la couture passe a 0,06 m, au-dessus
            // du plafond approuve de 0,05 m.
            var section = new RoadSection();
            section.Id = RoadId.New();
            section.RoadClass = RoadClass.Local;
            section.DefaultSpeedLimitMetersPerSecond = 13.9f;
            section.DefaultAllowedVehicleClasses = VehicleClassMask.Car;

            var start = new RoadCurveSample();
            start.SMeters = 0f;
            start.Position = Vector3.zero;
            start.Tangent = Vector3.forward;
            start.Up = Vector3.up;
            start.CurvaturePerMeter = 0f;
            start.HalfWidthLeftMeters = 2f;
            start.HalfWidthRightMeters = 2f;
            var end = start;
            end.SMeters = 30f;
            end.Position = new Vector3(0f, 0f, 30f);

            var corridor = new LaneCorridor();
            corridor.Id = RoadId.New();
            corridor.SectionId = section.Id;
            corridor.Samples = new[] { start, end };
            corridor.LengthMeters = 30f;
            corridor.LateralOrder = 0;
            corridor.IsCrossSectionDatum = true;

            var source = new RoadModelSource();
            source.ModelId = RoadId.New();
            source.Label = "source minimale 5.27";
            source.Sections = new[] { section };
            source.Corridors = new[] { corridor };

            // Les valeurs de l'importeur de migration, a une exception pres : SeamGapToleranceMeters
            // relache le plafond approuve.
            source.ValidationProfile = new RoadModelValidationProfile
            {
                MaxVehicleHalfWidthMeters = 1.03f,
                MaxVehicleLengthMeters = 4.5f,
                LateralClearanceMarginMeters = 0.25f,
                SeamGapToleranceMeters = 0.06f,
                SeamTangentToleranceDegrees = 5f,
                LengthToleranceMeters = 0.05f,
                EnvelopeOverlapToleranceMeters = 0.05f,
                GroundingMaxOffAxisDegrees = 45f
            };
            source.LocalizationProfile = new RoadLocalizationProfile
            {
                ScoreBandMeters = 0.15f,
                HysteresisMeters = 0.1f,
                AcceptanceDistanceMeters = 2.5f,
                WrongWayHeadingDegrees = 90f
            };

            var structural = new List<RoadModelValidationIssue>();
            var geometric = new List<RoadModelValidationIssue>();
            Assert.That(MigrationReport.ValidateSource(source, structural, geometric), Is.Null, "Un profil hors plafond n'emet aucune version.");

            // `ProfileToleranceAboveApprovedCeiling` (31) vient du validateur SEMANTIQUE, pas du
            // validateur geometrique : une regle naive "code >= 19 = geometrique" le classerait en
            // geometrie. Le split doit le nommer structurel, et la geometrie saine reste vide.
            Assert.That(structural.Count, Is.EqualTo(1));
            Assert.That(structural[0].Code, Is.EqualTo(RoadModelValidationCode.ProfileToleranceAboveApprovedCeiling));
            Assert.That(geometric, Is.Empty, "La geometrie du corridor minimal est saine : le seul defaut est le profil.");
        }

        [Test]
        public void TheFormattingHelperIsDeterministic()
        {
            // Precision fixe : metres et poids a 4 decimales, degres a 3 ; les zeros finaux sont des
            // octets du rapport, jamais tronques.
            Assert.That(MigrationFormat.Meters(3f), Is.EqualTo("3.0000"));
            Assert.That(MigrationFormat.Meters(0.05f), Is.EqualTo("0.0500"));
            Assert.That(MigrationFormat.Meters(1.23456f), Is.EqualTo("1.2346"));
            Assert.That(MigrationFormat.Degrees(30f), Is.EqualTo("30.000"));
            Assert.That(MigrationFormat.Degrees(1.23456f), Is.EqualTo("1.235"));
            Assert.That(MigrationFormat.Weight(1f), Is.EqualTo("1.0000"));
            Assert.That(MigrationFormat.Weight(0.4f), Is.EqualTo("0.4000"));

            // -0 se normalise : un signe sans valeur ne doit pas changer les octets du rapport. Un
            // negatif qui arrondit a zero perd son signe ; un negatif reel le garde.
            Assert.That(MigrationFormat.Meters(-0f), Is.EqualTo("0.0000"));
            Assert.That(MigrationFormat.Degrees(-0f), Is.EqualTo("0.000"));
            Assert.That(MigrationFormat.Weight(-0f), Is.EqualTo("0.0000"));
            Assert.That(MigrationFormat.Meters(-0.00001f), Is.EqualTo("0.0000"));
            Assert.That(MigrationFormat.Meters(-0.5f), Is.EqualTo("-0.5000"));

            // Culture invariante : le separateur decimal reste le point sous une culture a virgule.
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                Assert.That(MigrationFormat.Meters(1.5f), Is.EqualTo("1.5000"));
                Assert.That(MigrationFormat.Degrees(1.5f), Is.EqualTo("1.500"));
                Assert.That(MigrationFormat.Weight(0.25f), Is.EqualTo("0.2500"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        // ================================================================== helpers

        private static string RemoveFirstMovementRecord(string committed, out string key, out string id)
        {
            var match = Regex.Match(committed, "\\{\\s*\"Key\": \"(movement:[^\"]+)\",\\s*\"Kind\": \"Movement\",\\s*\"Id\": \"([0-9a-f]{32})\"\\s*\\},?\\s*");
            Assert.That(match.Success, Is.True);
            key = match.Groups[1].Value;
            id = match.Groups[2].Value;
            string lineage = committed.Remove(match.Index, match.Length);
            return match.Value.TrimEnd().EndsWith(",", StringComparison.Ordinal) ? lineage : Regex.Replace(lineage, ",(\\s*\\])", "$1");
        }

        private static string CommittedLineage()
        {
            // Chemin absolu resolu depuis la racine du projet : aucune lecture d'artefact committe
            // ne depend du repertoire courant du runner.
            return File.ReadAllText(MigrationReport.LineageFullPath);
        }

        /// <summary>
        /// Fabrique une lignee refusee en editant le texte committe (jamais en l'ecrivant a la main,
        /// ce qui la ferait diverger du format reel) : un seul defaut a la fois.
        /// </summary>
        private static string CorruptLineage(string corruption)
        {
            string committed = CommittedLineage();
            switch (corruption)
            {
                case "FormatInconnu":
                    return Regex.Replace(committed, "\"Format\": 1", "\"Format\": 2");
                case "GenreInconnu":
                    return Regex.Replace(committed, "\"Kind\": \"Corridor\"", "\"Kind\": \"Boulevard\"");
                case "IdIllisible":
                    return Regex.Replace(committed, "\"Id\": \"[0-9a-f]{32}\"", "\"Id\": \"zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz\"");
                case "CleEnDouble":
                    var match = Regex.Match(committed, "\\{\\s*\"Key\": \"([^\"]+)\",\\s*\"Kind\": \"([^\"]+)\",\\s*\"Id\": \"([0-9a-f]{32})\"\\s*\\}");
                    Assert.That(match.Success, Is.True);
                    // Meme cle, identite distincte : le refus doit venir de la CLE, pas du doublon d'identite.
                    string otherId = (match.Groups[3].Value[0] == '0' ? "1" : "0") + match.Groups[3].Value.Substring(1);
                    string duplicate = "{\"Key\": \"" + match.Groups[1].Value + "\", \"Kind\": \"" + match.Groups[2].Value + "\", \"Id\": \"" + otherId + "\"}";
                    return committed.Insert(match.Index, duplicate + ",\n        ");
                default:
                    Assert.Fail("Corruption sans fabrique : " + corruption);
                    return null;
            }
        }

        private static int Dispositions(V1ImportResult import, SourceItemKind item)
        {
            return import.Dispositions.Count(d => d.Item == item);
        }

        private static MigrationRun RunOnMvpRun(string priorLineage)
        {
            MigrationRun run = null;
            RunOnMvpRunWith(delegate(Scene scene) { run = MigrationReport.Run(scene, priorLineage); });
            return run;
        }

        private static void RunOnMvpRunWith(Action<Scene> body)
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MvpRunScenePath);
            bool wasOpen = alreadyOpen.IsValid() && alreadyOpen.isLoaded;
            if (wasOpen)
            {
                // Une scene deja ouverte mais modifiee decrirait un etat non sauve, pas la scene du
                // disque : toute mesure doit refuser de porter sur autre chose que l'authoring committe.
                Assert.That(alreadyOpen.isDirty, Is.False,
                    "MVP_Run est ouvert avec des modifications non sauvegardees : la mesure decrirait un etat non sauve, pas la scene du disque.");
            }

            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
            try
            {
                body(scene);
            }
            finally
            {
                if (!wasOpen)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void ANegativeAuthoredChoiceWeightIsARefusedForm()
        {
            // L'importeur ne peut refuser une forme que si l'extraction a reussi : on mute donc la
            // source REELLE plutot que de fabriquer un faux module, et le meme ensemble sert de
            // temoin avant mutation — sans quoi le test ne prouverait pas que le refus vient du poids.
            RunOnMvpRunWith(delegate(Scene scene)
            {
                var set = V1SourceSet.Extract(scene);
                Assert.That(set.IsValid, Is.True, string.Join("\n", set.Failures.ToArray()));
                Assert.That(MigrationReport.Run(set, CommittedLineage()).Succeeded, Is.True,
                    "Le temoin non mute doit passer : sinon le refus ne viendrait pas de la mutation.");

                V1Edge choice = null;
                foreach (var module in set.Modules)
                {
                    foreach (var node in module.Nodes)
                    {
                        if (node.Outgoing.Count < 2)
                        {
                            continue;
                        }

                        foreach (var edge in node.Outgoing)
                        {
                            if (!edge.IsConnectorJoin)
                            {
                                choice = edge;
                                break;
                            }
                        }

                        if (choice != null)
                        {
                            break;
                        }
                    }

                    if (choice != null)
                    {
                        break;
                    }
                }

                Assert.That(choice, Is.Not.Null, "La carte reelle porte des choix authores (noeuds de decision).");
                choice.Weight = -1f;

                var refused = MigrationReport.Run(set, CommittedLineage());
                Assert.That(refused.Succeeded, Is.False, "Un poids de choix negatif est une forme non disposable.");
                Assert.That(refused.Failures.Count, Is.EqualTo(1), string.Join("\n", refused.Failures.ToArray()));
                Assert.That(refused.ReportText, Is.Null, "Rien n'est produit pour une forme refusee.");
                Assert.That(refused.LineageJson, Is.Null);
            });
        }

        [Test]
        public void TheSharedCoreReadsThePriorLineageAndWritesBothFilesThroughInjectedPaths()
        {
            // Le noyau que le menu appelle : chemins injectes, donc les artefacts committes ne sont
            // jamais touches. Trois passages : premier import, relecture de la lignee ecrite, puis
            // lignee vide qui doit refuser sans rien ecrire.
            RunOnMvpRunWith(delegate(Scene scene)
            {
                string directory = Path.GetFullPath(Path.Combine("Temp", "Story527CoreTest"));
                Directory.CreateDirectory(directory);
                string lineageFile = Path.Combine(directory, "core-lineage.json");
                string reportFile = Path.Combine(directory, "core-report.md");
                try
                {
                    string error;
                    Assert.That(MigrationReport.Migrate(scene, lineageFile, reportFile, out error), Is.True, error);
                    string firstLineage = File.ReadAllText(lineageFile);
                    Assert.That(firstLineage, Is.Not.Empty);
                    Assert.That(File.ReadAllText(reportFile), Does.Contain("## Coupes transversales (AD-48)"));

                    Assert.That(MigrationReport.Migrate(scene, lineageFile, reportFile, out error), Is.True, error);
                    Assert.That(File.ReadAllText(lineageFile), Is.EqualTo(firstLineage),
                        "La lignee ecrite doit etre RELUE par le passage suivant, pas refrappee.");

                    File.WriteAllText(lineageFile, string.Empty);
                    Assert.That(MigrationReport.Migrate(scene, lineageFile, reportFile, out error), Is.False);
                    Assert.That(error, Does.Contain("Lignee vide"));
                    Assert.That(File.ReadAllText(lineageFile), Is.Empty, "Un passage refuse n'ecrit rien.");
                }
                finally
                {
                    Directory.Delete(directory, true);
                }
            });
        }

        [Test]
        public void AConnectorWithoutAnyJoinCandidateIsAHardFailure()
        {
            // Un module isole : ses connecteurs n'ont aucun candidat a moins du seuil de jointure, donc
            // la decouverte doit refuser la forme au lieu de fabriquer une topologie.
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_RoadSegment_TwoWay.prefab");
            var settings = AssetDatabase.LoadAssetAtPath<TrafficSettingsDef>("Assets/RoadRage/ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(settings, Is.Not.Null);

            var root = EditorUtility.CreateGameObjectWithHideFlags("LaneGraphRoot", HideFlags.HideAndDontSave, typeof(LaneGraph));
            var serialized = new SerializedObject(root.GetComponent<LaneGraph>());
            serialized.FindProperty("trafficSettings").objectReferenceValue = settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Scene d'apercu : l'instance de prefab a besoin d'une scene, et une scene d'apercu ne
            // touche ni la hierarchie ouverte ni l'etat "sale" d'une scene reelle.
            var bench = EditorSceneManager.NewPreviewScene();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, bench);
            // Le graph porte ses modules comme enfants (comme RunRoot/LaneGraph dans MVP_Run) : sans
            // ce lien, l'extraction refuse des noeuds « hors du LaneGraph ».
            instance.transform.SetParent(root.transform);
            try
            {
                // Racine unique : le graph, dont le module est desormais un enfant (une racine
                // supplementaire ferait visiter chaque noeud deux fois).
                var set = V1SourceSet.Extract(new[] { root }, string.Empty, V1SourceSet.RecognisedPrefabs);
                Assert.That(set.IsValid, Is.False, "Un connecteur sans candidat de jointure est orphelin.");
                Assert.That(set.Failures.Any(f => f.Contains("orphelin")), Is.True, string.Join("\n", set.Failures.ToArray()));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(bench);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ThePreFlightRefusalsRefuseBeforeAnythingIsWritten()
        {
            // Les deux seules decisions que le menu prend avant d'ouvrir la scene : le noyau est
            // eprouve ici parce que le menu lui-meme ecrit les vrais fichiers.
            Assert.That(MigrationReport.RefusalReason(true, false, false), Does.Contain("Play Mode"));
            Assert.That(MigrationReport.RefusalReason(false, true, true), Does.Contain("non sauvegardees"));
            Assert.That(MigrationReport.RefusalReason(false, true, false), Is.Null,
                "Scene ouverte et propre : le passage est permis.");
            Assert.That(MigrationReport.RefusalReason(false, false, true), Is.Null,
                "Une scene non ouverte est ouverte par le menu ; son etat de salissure ne le concerne pas.");
        }
    }
}
