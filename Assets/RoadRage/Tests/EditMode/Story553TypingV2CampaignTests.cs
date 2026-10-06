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
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.53a -- verification ciblee du typing-v2 (contenance et dedoublonnage exact des racines) sur la VRAIE MVP_Run,
    /// avant toute ecriture : seules les 12 fusions de giratoire sont raffinees (4 Ouest, 4 Sud, 4 Diagonale), aucun plan
    /// global. Sud et Diagonale doivent garder genre et debuts au bit pres ; chaque fusion Ouest doit etre typee Merge sous
    /// le budget, sinon HALT et fallback Crossing (decision proprietaire du 2026-10-06). Campagne longue : [Explicit].
    /// </summary>
    [Explicit]
    [Category("Geometry")]
    [Category("Story553TypingV2Campaign")]
    public sealed class Story553TypingV2CampaignTests
    {
        private const string OutputDirectory = "_bmad-output/implementation-artifacts/traffic-v2-5-53-classification/";

        [Serializable]
        private sealed class Manifest
        {
            public Record[] Records;
        }

        [Serializable]
        private sealed class Record
        {
            public string PairKey;
            public string JunctionId;
            public string MovementAId;
            public string MovementBId;
            public string Classification;
            public string ReasonCode;
            public int RefinementLeaves;
            public bool RefinementComplete;
            public bool CommonExitCorridor;
            public string ConflictKind;
            public float ContactStartSMetersA;
            public float ContactStartSMetersB;
        }

        [Test]
        [Timeout(7200000)]
        public void TypingV2TypesTheWestMergesAndKeepsTheSouthAndDiagonalMerges()
        {
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(AuthoredRoadModel.FullPath(AutomatedPairDecisionPolicy.ManifestPath)));
            AuthoredRun run = null;
            WithMvpRun(scene =>
            {
                var regenerated = GateAEvidenceRegeneration.Regenerate(scene, File.ReadAllText(MigrationReport.LineageFullPath),
                    File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)), GateAEvidenceParameters.Declared());
                Assert.That(regenerated.Covered, Is.True, string.Join(" ; ", regenerated.Failures.ToArray()));
                run = regenerated.Run;
            });

            CompiledRoadModel model = run.CandidateModel;
            SweepGraph graph = SweepGraph.FromModel(model);
            var keys = AuthoringDecisions.KeysById(run.Import);
            var rows = new StringBuilder();
            var changed = new List<string>();
            var westNotMerge = new List<string>();
            int counted = 0;
            foreach (var record in manifest.Records)
            {
                if (!AutomatedPairDecisionPolicy.TypingV2Applies(record.Classification, record.ReasonCode, record.CommonExitCorridor)) continue;
                string junction = JunctionLabel(model, record.JunctionId);
                if (!junction.StartsWith("Roundabout", StringComparison.Ordinal)) continue;

                RoadId idA = RoadId.Parse(record.MovementAId);
                RoadId idB = RoadId.Parse(record.MovementBId);
                CompiledJunctionMovement a;
                CompiledJunctionMovement b;
                Assert.That(model.TryGetMovement(idA, out a), Is.True);
                Assert.That(model.TryGetMovement(idB, out b), Is.True);
                string labels = MovementLabel(a) + " | " + MovementLabel(b);
                bool west = labels.Contains("West");
                string arm = west ? "Ouest" : labels.Contains("South") ? "Sud" : "Diagonale";

                var watch = Stopwatch.StartNew();
                var refinement = ConflictSweep.Refine(graph, idA, idB, Paths(graph, model, idA), Paths(graph, model, idB),
                    model.ValidationProfile, run.EvidenceParameters, run.OffsetBounds, AutomatedPairDecisionPolicy.ProofToleranceMeters,
                    AutomatedPairDecisionPolicy.MaxSubdivisionDepth, AutomatedPairDecisionPolicy.RefinementLeafBudget, true);
                double seconds = watch.Elapsed.TotalSeconds;

                // Meme typage que la politique : ordre (A, B) du balayage, ramene a la cle de paire.
                var typing = ZoneTyping.Of(refinement, a.LengthMeters, b.LengthMeters, a.ToCorridorId == b.ToCorridorId, true);
                if (keys[idA] != record.PairKey.Split('\n')[0]) typing = typing.Swapped();

                string label = arm + " -- " + junction + " : " + labels;
                if (west)
                {
                    if (!refinement.Complete || typing.Kind != ConflictKind.Merge) westNotMerge.Add(label);
                }
                else if (typing.Kind.ToString() != record.ConflictKind || typing.StartA != record.ContactStartSMetersA
                    || typing.StartB != record.ContactStartSMetersB)
                {
                    changed.Add(label);
                }

                rows.Append("| ").Append(arm).Append(" | ").Append(junction).Append(" | ").Append(labels)
                    .Append(" | ").Append(record.ConflictKind).Append(' ').Append(F(record.ContactStartSMetersA)).Append(" / ").Append(F(record.ContactStartSMetersB))
                    .Append(" | ").Append(record.RefinementLeaves).Append(record.RefinementComplete ? string.Empty : " (incomplet)")
                    .Append(" | ").Append(typing.Kind).Append(' ').Append(F(typing.StartA)).Append(" / ").Append(F(typing.StartB))
                    .Append(" | ").Append(refinement.Leaves).Append(refinement.Complete ? string.Empty : " (incomplet)")
                    .Append(" | ").Append(refinement.Roots).Append(" | ").Append(refinement.DuplicateRoots)
                    .Append(" | ").Append(refinement.ProvenLeaves).Append(" | ").Append(refinement.WitnessLeaves)
                    .Append(" | ").Append(refinement.ResolutionLeaves).Append(" | ").Append(refinement.ContainedLeaves)
                    .Append(" | ").Append(seconds.ToString("0.000", CultureInfo.InvariantCulture)).Append(" |\n");
                counted++;
            }

            var report = new StringBuilder();
            report.Append("# Typing-v2 5.53a -- fusions de giratoire sur MVP_Run (verification ciblee, rien n'est ecrit)\n\n");
            report.Append("Chemin 5.52 (regeneration, parametres declares), puis `ConflictSweep.Refine` en typing-v2 (contenance et dedoublonnage ");
            report.Append("exact des racines) sur les seules 12 fusions de giratoire ; aucun plan global. Budget ")
                .Append(AutomatedPairDecisionPolicy.RefinementLeafBudget).Append(" feuilles par paire, tolerances, profondeur, `WitnessSplit` et ordre inchanges. ");
            report.Append("Debuts de contact dans l'ordre de la cle de paire (A / B).\n\n");
            report.Append("- Fusions Ouest non typees `Merge` sous le budget (HALT, fallback Crossing) : ").Append(westNotMerge.Count).Append(".\n");
            report.Append("- Fusions Sud / Diagonale dont genre ou debuts changent (HALT) : ").Append(changed.Count).Append(".\n\n");
            report.Append("| Bras | Giratoire | Mouvements | v1 publie | v1 feuilles | v2 | v2 feuilles | Racines | Doublons ecartes | Prouvees | Temoins | Resolution | Contenance | Duree (s) |\n");
            report.Append("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|\n").Append(rows).Append('\n');
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string error;
            Assert.That(AuthoredRoadModel.TryWriteAll(new[]
            {
                new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(OutputDirectory + "typing-v2-giratoires-" + stamp + ".md"), report.ToString())
            }, out error), Is.True, error);

            Assert.That(counted, Is.EqualTo(12), "12 fusions de giratoire attendues.");
            Assert.That(changed, Is.Empty, "HALT : fusion Sud ou Diagonale modifiee.");
            Assert.That(westNotMerge, Is.Empty, "HALT : fusion Ouest non typee Merge sous le budget ; fallback Crossing.");
        }

        private static List<List<SweepPose>> Paths(SweepGraph graph, CompiledRoadModel model, RoadId movement)
        {
            string failure;
            var paths = ConflictSweep.Paths(graph, movement, ConflictSweep.Reach(model.ValidationProfile), out failure);
            Assert.That(failure, Is.Null, failure);
            return paths;
        }

        private static string JunctionLabel(CompiledRoadModel model, string junctionId)
        {
            RoadId id;
            Junction junction;
            return RoadId.TryParse(junctionId, out id) && model.TryGetJunction(id, out junction) && !string.IsNullOrEmpty(junction.Label)
                ? junction.Label : junctionId;
        }

        private static string MovementLabel(CompiledJunctionMovement movement)
        {
            if (movement.Label == null) return "?";
            int at = movement.Label.IndexOf(": ", StringComparison.Ordinal);
            return at < 0 ? movement.Label : movement.Label.Substring(at + 2);
        }

        private static string F(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
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
