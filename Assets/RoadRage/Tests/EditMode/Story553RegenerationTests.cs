using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.53 -- execution de la politique v3 sur la VRAIE MVP_Run par le chemin 5.52 (scene ouverte en additif, jamais
    /// sauvegardee), en diagnostic : deux plans (determinisme), diff paire par paire avant / apres, compteurs, cout du
    /// raffinement (feuilles, duree, allocation) et traversees compatibles, publies sous traffic-v2-5-53-classification/.
    /// Rien n'est ecrit dans les decisions, le manifeste ni le modele : l'application reste le menu transactionnel, apres revue.
    /// Les verdicts rouges sont des HALT proprietaire. Campagne longue : [Explicit], lancee par sa categorie.
    /// </summary>
    [Explicit]
    [Category("Geometry")]
    [Category("Story553Regeneration")]
    public sealed class Story553RegenerationTests
    {
        private const string OutputDirectory = "_bmad-output/implementation-artifacts/traffic-v2-5-53-classification/";

        /// <summary>Zones de l'audit du 2026-10-05 : groupe A (16) et voies opposees en ligne droite du groupe B (6).</summary>
        private static readonly string[] AuditedZones =
        {
            "458eb40d533eff27d37e8ed61c6eaabb", "4cbd976887b4f64454c372023ad131a2", "47b2bdca8b4d502a90968e846cd49abb", "4b53383c5c1fcf5fddb5b7ee011ed78a",
            "45221b336e33b670c8623e386ad6bf8d", "45c9ea1a463d131a176bf4c5516872a3", "470ef9a2fd6c8d61f93092fe1646ba97", "4f1c595ed26310a51f9f60a0f3ead586",
            "433fa627c90e7aef2af3fd986d2faa8d", "45168e2b5ecebf3c89b80036bbddd0af", "4269dde366f786c6fd4e279786aefc94", "433af752017a55720032fc063252c999",
            "4b38f8659ad8dac11a7d0a0b53fc47b5", "4fedf986cee6f96bf250c5b1f2439e82", "45f7adaf82bc052cf599183a4c402484", "4dbd9c73370da2e3405d8328dacd9d99",
            "4a8d1f62800182ac0fade27255a39190", "415e54e20484285ef32cb5ede00b61ae", "436005183adb61a537157827df3372be", "40bd2f68ba2bc3ff9535b711aea9998f",
            "4b09a0f12076cb983208a9d922a545b7", "465d5eec7c97132ad85c3ac363cca48e"
        };

        private static Result _result;

        private sealed class Result
        {
            public AuthoredRun Run;
            public string BeforeManifest;
            public string AfterManifest;
            public AutomatedPairDecisionPolicy.ClassificationSummary Summary;
        }

        [Test]
        [Timeout(7200000)]
        public void A_TheRefinedPolicyIsDeterministicAndItsDiffIsPublished()
        {
            var result = Execute();
            Assert.That(result.Summary.Pairs, Is.GreaterThan(0));
        }

        [Test]
        [Timeout(7200000)]
        public void B_NoClassificationOutsideTheConservativeConflictsChanges()
        {
            var summary = Execute().Summary;
            Assert.That(summary.ChangedOutsideConservative, Is.Empty, "HALT proprietaire : classification non conservative changee :\n- "
                + string.Join("\n- ", summary.ChangedOutsideConservative.ToArray()));
            Assert.That(summary.ProvenUnchanged, Is.EqualTo(summary.ProvenBefore), "Chaque ConflictProven garde classification, raison et preuve.");
        }

        [Test]
        [Timeout(7200000)]
        public void C_OpposedStraightLanesAndAuditGroupAAreResolved()
        {
            var result = Execute();
            var remaining = new List<string>();
            foreach (var zone in AuditedZones)
            {
                string pair = AutomatedPairDecisionPolicy.PairKeyOfZone(result.BeforeManifest, zone);
                Assert.That(pair, Is.Not.Null, "zone de l'audit absente du manifeste v2 : " + zone);
                if (result.Summary.RemainingConservative.Contains(pair)) remaining.Add(zone);
            }

            Assert.That(remaining, Is.Empty, "HALT avant la 5.35 (spec 5.53, Ask First) : zones auditees restees ConservativeConflict, motifs au diff :\n- "
                + string.Join("\n- ", remaining.ToArray()));
        }

        private static Result Execute()
        {
            if (_result != null) return _result;
            var result = new Result();
            var timing = new StringBuilder();
            AutomatedPairDecisionPlan first = null;
            AutomatedPairDecisionPlan second = null;
            GateAEvidenceRegenerationResult regenerated = null;
            WithMvpRun(scene =>
            {
                string lineage = File.ReadAllText(MigrationReport.LineageFullPath);
                string decisions = File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath));
                result.BeforeManifest = File.ReadAllText(AuthoredRoadModel.FullPath(AutomatedPairDecisionPolicy.ManifestPath));
                var watch = Stopwatch.StartNew();
                regenerated = GateAEvidenceRegeneration.Regenerate(scene, lineage, decisions, GateAEvidenceParameters.Declared());
                double regenerationSeconds = watch.Elapsed.TotalSeconds;
                Assert.That(regenerated.Covered, Is.True, "Regeneration 5.52 non couverte : "
                    + string.Join(" ; ", regenerated.Failures.ToArray()) + " ; " + string.Join(" ; ", regenerated.Deficits.ToArray()));

                long memoryBefore = GC.GetTotalMemory(false);
                long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                watch.Restart();
                first = AutomatedPairDecisionPolicy.CreatePlan(regenerated.Run, result.BeforeManifest);
                double firstSeconds = watch.Elapsed.TotalSeconds;
                long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
                long memoryAfter = GC.GetTotalMemory(false);
                watch.Restart();
                second = AutomatedPairDecisionPolicy.CreatePlan(regenerated.Run, result.BeforeManifest, first.AllocatedIds);
                double secondSeconds = watch.Elapsed.TotalSeconds;

                timing.Append("| Mesure | Valeur |\n|---|---|\n");
                timing.Append("| Regeneration 5.52 (pipeline, quatre preuves) | ").Append(S(regenerationSeconds)).Append(" s |\n");
                timing.Append("| Plan v3, premier passage (balayage deja fait, raffinement et typage) | ").Append(S(firstSeconds)).Append(" s |\n");
                timing.Append("| Plan v3, second passage (determinisme) | ").Append(S(secondSeconds)).Append(" s |\n");
                timing.Append("| Allocation du premier plan (thread courant) | ").Append(Mb(allocated)).Append(" Mo |\n");
                timing.Append("| Tas geree avant / apres le premier plan | ").Append(Mb(memoryBefore)).Append(" / ").Append(Mb(memoryAfter)).Append(" Mo |\n");
                Debug.Log("[Story553] regeneration " + S(regenerationSeconds) + " s, plan " + S(firstSeconds) + " s puis " + S(secondSeconds)
                    + " s, allocation " + Mb(allocated) + " Mo.");
            });

            Assert.That(second.DecisionRunId, Is.EqualTo(first.DecisionRunId));
            Assert.That(second.DecisionsText, Is.EqualTo(first.DecisionsText), "Deux plans identiques, octet pour octet.");
            Assert.That(second.ManifestText, Is.EqualTo(first.ManifestText));
            AutomatedPairDecisionPolicy.ValidatePlan(first, regenerated.Run.PairSweeps.Count);

            result.Run = regenerated.Run;
            result.AfterManifest = first.ManifestText;
            result.Summary = AutomatedPairDecisionPolicy.Summarize(regenerated.Run.CandidateModel, result.BeforeManifest, result.AfterManifest);
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string diff = AutomatedPairDecisionPolicy.RenderClassificationDiff(regenerated.Run, result.BeforeManifest, result.AfterManifest);
            var report = new StringBuilder();
            report.Append("# Regeneration 5.53 -- politique v3 sur MVP_Run (diagnostic, rien n'est ecrit)\n\n");
            report.Append("Chemin 5.52 : `GateAEvidenceRegeneration.Regenerate` (parametres declares), puis deux `CreatePlan` v3 sur le meme passage. ");
            report.Append("Diff paire par paire : `diff-paires-").Append(stamp).Append(".md`. Geometrie, gonflement, tolerance et profil inchanges.\n\n");
            report.Append("## Cout\n\n").Append(timing).Append('\n');
            report.Append("## Verdicts\n\n");
            report.Append("- Determinisme : deux plans identiques (run `").Append(first.DecisionRunId).Append("`).\n");
            report.Append("- Classifications non conservatives changees : ").Append(result.Summary.ChangedOutsideConservative.Count).Append(".\n");
            report.Append("- `ConflictProven` inchangees : ").Append(result.Summary.ProvenUnchanged).Append(" / ").Append(result.Summary.ProvenBefore).Append(".\n");
            report.Append("- `ConservativeConflict` : ").Append(result.Summary.ConservativeBefore).Append(" -> ").Append(result.Summary.ConservativeToDisjoint)
                .Append(" `ProvenDisjoint`, ").Append(result.Summary.ConservativeToProven).Append(" `ConflictProven`, ")
                .Append(result.Summary.ConservativeRemaining).Append(" restees.\n\n");
            AppendRefinedPairs(report, result.Summary, first.RefinementSeconds);
            report.Append("## Preuves Gate A regenerees (chemin 5.52)\n\n").Append(regenerated.ReportText);

            string error;
            Assert.That(AuthoredRoadModel.TryWriteAll(new[]
            {
                new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(OutputDirectory + "diff-paires-" + stamp + ".md"), diff),
                new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(OutputDirectory + "regeneration-" + stamp + ".md"), report.ToString())
            }, out error), Is.True, error);
            _result = result;
            return result;
        }

        /// <summary>Cout par paire raffinee (feuilles, arrets a la resolution, duree) et paires au plafond du budget.</summary>
        private static void AppendRefinedPairs(StringBuilder report, AutomatedPairDecisionPolicy.ClassificationSummary summary,
            Dictionary<string, double> seconds)
        {
            var durations = new List<double>();
            double total = 0d;
            foreach (var pair in summary.Refined)
            {
                double value;
                seconds.TryGetValue(pair.PairKey, out value);
                durations.Add(value);
                total += value;
            }

            durations.Sort();
            var leaves = new List<int>(summary.RefinementLeaves);
            leaves.Sort();
            report.Append("## Cout par paire raffinee\n\n");
            report.Append("| Mesure | Mediane | p95 | Maximum | Total |\n|---|---|---|---|---|\n");
            report.Append("| Feuilles | ").Append(Pick(leaves, 0.5)).Append(" | ").Append(Pick(leaves, 0.95)).Append(" | ")
                .Append(leaves.Count == 0 ? 0 : leaves[leaves.Count - 1]).Append(" | ").Append(Sum(leaves)).Append(" |\n");
            report.Append("| Duree (s) | ").Append(S3(Pick(durations, 0.5))).Append(" | ").Append(S3(Pick(durations, 0.95))).Append(" | ")
                .Append(S3(durations.Count == 0 ? 0d : durations[durations.Count - 1])).Append(" | ").Append(S3(total)).Append(" |\n\n");

            report.Append("### Paires ayant atteint le plafond de ").Append(AutomatedPairDecisionPolicy.RefinementLeafBudget).Append(" feuilles\n\n");
            int capped = 0;
            foreach (var pair in summary.Refined)
            {
                if (pair.Leaves < AutomatedPairDecisionPolicy.RefinementLeafBudget) continue;
                capped++;
                report.Append("- ").Append(pair.Junction).Append(" : ").Append(pair.Movements).Append(" -- ").Append(pair.Classification)
                    .Append(", ").Append(pair.Outcome).Append('\n');
            }

            report.Append(capped == 0 ? "Aucune.\n\n" : "\n");
            report.Append("### Detail par paire\n\n| Carrefour | Mouvements | Classification | Issue | Complet | Feuilles | Arrets resolution | Duree (s) | Genre |\n");
            report.Append("|---|---|---|---|---|---|---|---|---|\n");
            foreach (var pair in summary.Refined)
            {
                double value;
                seconds.TryGetValue(pair.PairKey, out value);
                report.Append("| ").Append(pair.Junction).Append(" | ").Append(pair.Movements).Append(" | ").Append(pair.Classification)
                    .Append(" | ").Append(pair.Outcome).Append(" | ").Append(pair.Complete ? "oui" : "non").Append(" | ").Append(pair.Leaves)
                    .Append(" | ").Append(pair.ResolutionLeaves).Append(" | ").Append(S3(value)).Append(" | ").Append(pair.Kind).Append(" |\n");
            }

            report.Append('\n');
        }

        private static T Pick<T>(List<T> sorted, double q)
        {
            if (sorted.Count == 0) return default(T);
            int index = (int)Math.Ceiling(q * sorted.Count) - 1;
            return sorted[Math.Max(0, Math.Min(sorted.Count - 1, index))];
        }

        private static long Sum(List<int> values)
        {
            long total = 0;
            foreach (int value in values) total += value;
            return total;
        }

        private static string S3(double seconds)
        {
            return seconds.ToString("0.000", CultureInfo.InvariantCulture);
        }

        private static string S(double seconds)
        {
            return seconds.ToString("0.0", CultureInfo.InvariantCulture);
        }

        private static string Mb(long bytes)
        {
            return (bytes / (1024d * 1024d)).ToString("0.0", CultureInfo.InvariantCulture);
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
