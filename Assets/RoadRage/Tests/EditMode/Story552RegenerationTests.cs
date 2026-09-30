using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.52 -- regeneration de la preuve Gate A sur la VRAIE MVP_Run (ouverte en additif, jamais sauvegardee),
    /// publiee sous _bmad-output/implementation-artifacts/gate-a-5-52/. A : diagnostic intermediaire (a_e declare,
    /// pose tangente, ne vaut pas preuve). B : regeneration cinematique complete et deterministe, avec differentiel
    /// des candidats. C : verdict -- tout residu non positif, borne non fermee ou pose infaisable le fait echouer
    /// avec le deficit localise (HALT proprietaire). Aucun artefact signe n'est ecrit. Campagne longue : [Explicit],
    /// lancee par sa categorie, jamais dans la suite par defaut.
    /// </summary>
    [Explicit]
    [Category("Geometry")]
    [Category("Story552Regeneration")]
    public sealed class Story552RegenerationTests
    {
        private static GateAEvidenceRegenerationResult _kinematic;

        [Test]
        [Timeout(3600000)]
        public void A_TheTangentDiagnosticWithTheDeclaredAllowanceIsPublished()
        {
            var result = RegenerateTwice(GateAEvidenceParameters.TangentDiagnostic(), "diagnostic-ae-tangente.md", "diagnostic-ae-tangente-diff.md");
            Assert.That(result.Failures, Is.Empty, "Diagnostic incomplet : " + string.Join(" | ", result.Failures.ToArray()));
            StringAssert.Contains("DIAGNOSTIC INTERMEDIAIRE", result.ReportText);
        }

        [Test]
        [Timeout(3600000)]
        public void B_TheKinematicRegenerationIsCompleteAndDeterministic()
        {
            var result = RegenerateTwice(GateAEvidenceParameters.Declared(), "regeneration-cinematique.md", "diff-candidats.md");
            _kinematic = result;
            Assert.That(result.Failures, Is.Empty, "Regeneration incomplete : " + string.Join(" | ", result.Failures.ToArray()));
            Assert.That(result.Run.OffsetBounds != null && result.Run.OffsetBounds.Closed, Is.True, "Bornes d'ecart fermees sur MVP_Run.");
            Assert.That(result.Diff, Is.Not.Empty, "Differentiel publie.");
            foreach (var entry in result.Diff)
            {
                Assert.That(entry.State, Is.Not.EqualTo(CandidateDiffState.Confirmed),
                    "Aucune decision signee (empreinte v1) ne survit a une enveloppe changee : " + entry.PairKey.Replace("\n", " x "));
            }
        }

        [Test]
        [Timeout(3600000)]
        public void C_TheRegeneratedEvidenceCoversTheDeclaredTolerance()
        {
            var result = _kinematic ?? RegenerateTwice(GateAEvidenceParameters.Declared(), "regeneration-cinematique.md", "diff-candidats.md");
            Assert.That(result.Failures, Is.Empty, "Regeneration incomplete.");
            Assert.That(result.Deficits, Is.Empty, "HALT proprietaire (Story 5.52) -- " + result.Deficits.Count + " deficit(s), aucun seuil ni marge touche :\n- "
                + string.Join("\n- ", result.Deficits.ToArray()));
        }

        private static GateAEvidenceRegenerationResult RegenerateTwice(GateAEvidenceParameters parameters, string reportName, string diffName)
        {
            GateAEvidenceRegenerationResult first = null;
            GateAEvidenceRegenerationResult second = null;
            var watch = Stopwatch.StartNew();
            WithMvpRun(scene =>
            {
                string lineage = File.ReadAllText(MigrationReport.LineageFullPath);
                string decisions = File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath));
                first = GateAEvidenceRegeneration.Regenerate(scene, lineage, decisions, parameters);
                double firstSeconds = watch.Elapsed.TotalSeconds;
                second = GateAEvidenceRegeneration.Regenerate(scene, lineage, decisions, parameters);
                Debug.Log("[Story552] " + parameters.PoseModelLabel + " : " + firstSeconds.ToString("0.0") + " s puis "
                    + (watch.Elapsed.TotalSeconds - firstSeconds).ToString("0.0") + " s ; " + first.Deficits.Count + " deficit(s), "
                    + first.Failures.Count + " echec(s), " + first.Diff.Count + " paire(s) au differentiel.");
            });

            Assert.That(second.ReportText, Is.EqualTo(first.ReportText), "Deux executions donnent le meme rapport, octet pour octet.");
            Assert.That(second.DiffText, Is.EqualTo(first.DiffText), "Deux executions donnent le meme differentiel.");
            string error;
            Assert.That(AuthoredRoadModel.TryWriteAll(new[]
            {
                new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(GateAEvidenceRegeneration.OutputDirectory + "/" + reportName), first.ReportText),
                new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(GateAEvidenceRegeneration.OutputDirectory + "/" + diffName), first.DiffText)
            }, out error), Is.True, error);
            return first;
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
