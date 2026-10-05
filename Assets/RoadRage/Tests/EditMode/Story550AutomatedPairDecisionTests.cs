using System;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>Story 5.50 -- fonction deleguee, manifeste et harnais de progression.</summary>
    [Category("Geometry")]
    public sealed class Story550AutomatedPairDecisionTests
    {
        [Serializable]
        private sealed class ArchivedManifest
        {
            public string SupersededManifestText;
            public string SupersededManifestHash;
        }

        private static AuthoredRun _run;
        private static AutomatedPairDecisionPlan _plan;

        // Politique v3 (5.53) : deux plans raffines frais, ~16 min ; le determinisme est aussi prouve par la campagne
        // Story553Regeneration et par le menu d'application, qui comparent deux plans avant d'ecrire.
        [Test, Explicit, Timeout(3600000)]
        public void TwoPlansReuseAllocatedIdentitiesAndAreByteIdentical()
        {
            AuthoredRun run = Run();
            AutomatedPairDecisionPlan first = AutomatedPairDecisionPolicy.CreatePlan(run);
            AutomatedPairDecisionPlan second = AutomatedPairDecisionPolicy.CreatePlan(run, null, first.AllocatedIds);

            Assert.That(second.DecisionRunId, Is.EqualTo(first.DecisionRunId));
            Assert.That(second.DecisionsText, Is.EqualTo(first.DecisionsText));
            Assert.That(second.ManifestText, Is.EqualTo(first.ManifestText));
        }

        [Test, Timeout(900000)]
        public void TheRealDifferentialIsExhaustiveAndEveryActiveDecisionIsFormatFour()
        {
            AuthoredRun run = Run();
            AutomatedPairDecisionPlan plan = Plan();
            var decisions = AuthoringDecisions.Parse(plan.DecisionsText);

            Assert.That(plan.DecisionsText, Does.StartWith("{\n    \"Format\": 4,"));
            Assert.That(decisions.Conflicts, Has.Count.EqualTo(run.Candidates.Count));
            Assert.That(plan.ManifestText, Does.Contain("\"Classification\": \"Following\""));
            Assert.That(plan.ManifestText, Does.Contain("\"Classification\": \"ProvenDisjoint\""));
            Assert.That(plan.ManifestText, Does.Contain("\"DecisionRevisionId\""));
            Assert.That(plan.ManifestText, Does.Contain("\"EvidenceHash\""));
            Assert.That(plan.ManifestText, Does.Contain("\"DecisionRunId\""));
            foreach (var decision in decisions.Conflicts)
            {
                Assert.That(decision.DecisionRevisionId, Has.Length.EqualTo(64));
                Assert.That(decision.EvidenceHash, Has.Length.EqualTo(64));
                Assert.That(decision.DecisionRunId, Is.EqualTo(plan.DecisionRunId));
                Assert.That(decision.GeometryFingerprint, Has.Length.EqualTo(64));
            }
        }

        [Test]
        public void TamperedEvidenceIsRejectedBeforeAnyWrite()
        {
            AutomatedPairDecisionPlan source = Plan();
            var tampered = new AutomatedPairDecisionPlan
            {
                DecisionRunId = source.DecisionRunId,
                DecisionsText = source.DecisionsText,
                ManifestText = TamperFirstHash(source.ManifestText, "\"EvidenceHash\": \"")
            };

            Assert.Throws<InvalidOperationException>(() =>
                AutomatedPairDecisionPolicy.ValidatePlan(tampered, CommittedPairs()));
        }

        // Le pipeline cinematique complet (Run) coute ~225 s depuis la 5.52.
        [Test, Timeout(900000)]
        public void TheNineJunctionHarnessProvesSafetyMaximalityProgressAndBoundedWait()
        {
            Assert.DoesNotThrow(() => AutomatedPairDecisionPolicy.ValidateTrafficScenarios(Run(), Plan()));
        }

        // Politique v3 (5.53) : un passage du pipeline et deux plans raffines, ~20 min mesurees.
        [Test, Explicit, Timeout(3600000)]
        public void KinematicDecisionPlanIsDeterministicAndKeepsHistoricalDecisionsUnchanged()
        {
            string decisionsText = File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath));
            string manifestText = File.ReadAllText(AuthoredRoadModel.FullPath(AutomatedPairDecisionPolicy.ManifestPath));
            var currentArchive = UnityEngine.JsonUtility.FromJson<ArchivedManifest>(manifestText);
            string historicalManifest = string.IsNullOrEmpty(currentArchive.SupersededManifestText)
                ? manifestText : currentArchive.SupersededManifestText;
            WithMvpRun(scene =>
            {
                var run = AuthoredRoadModel.Run(V1SourceSet.Extract(scene),
                    File.ReadAllText(MigrationReport.LineageFullPath), decisionsText, GateAEvidenceParameters.Declared());
                Assert.That(run.CandidateModel, Is.Not.Null);
                Assert.That(run.PairSweeps, Is.Not.Empty);
                var first = AutomatedPairDecisionPolicy.CreatePlan(run, manifestText);
                var second = AutomatedPairDecisionPolicy.CreatePlan(run, manifestText, first.AllocatedIds);
                Assert.That(second.DecisionRunId, Is.EqualTo(first.DecisionRunId));
                Assert.That(second.DecisionsText, Is.EqualTo(first.DecisionsText));
                Assert.That(second.ManifestText, Is.EqualTo(first.ManifestText));
                Assert.That(first.ManifestText, Does.Contain("\"FingerprintSchemaVersion\": 2"));
                Assert.That(first.ManifestText, Does.Contain("\"ConflictSweepAlgorithmVersion\": 2"));
                Assert.That(first.ManifestText, Does.Contain("\"EvidenceParametersHash\""));
                var archive = UnityEngine.JsonUtility.FromJson<ArchivedManifest>(first.ManifestText);
                Assert.That(archive.SupersededManifestText, Is.EqualTo(historicalManifest), "Le manifeste remplace reste verbatim dans l'audit.");
                Assert.That(archive.SupersededManifestHash, Is.EqualTo(V1SourceSet.Sha256Hex(historicalManifest)));
                // La chaine d'archives (5.53 -> 5.52 -> 5.50) garde le manifeste 5.50 verbatim.
                bool found = false;
                for (var link = archive; link != null && !string.IsNullOrEmpty(link.SupersededManifestText);
                    link = UnityEngine.JsonUtility.FromJson<ArchivedManifest>(link.SupersededManifestText))
                {
                    found |= link.SupersededManifestHash == "32507cb57f33171bfa2a0e9476b2beff53d70fd3fed0866d886aac9d6e56efb8";
                }

                Assert.That(found, Is.True, "Le manifeste 5.50 reste archive dans la chaine.");
                if (!string.IsNullOrEmpty(currentArchive.SupersededManifestText))
                {
                    Assert.That(first.DecisionsText, Is.EqualTo(decisionsText), "Second passage : aucune decision nouvelle.");
                    Assert.That(first.ManifestText, Is.EqualTo(manifestText), "Second passage : aucun changement du manifeste.");
                }
                var unpinned = new AutomatedPairDecisionPlan
                {
                    DecisionRunId = first.DecisionRunId,
                    DecisionsText = first.DecisionsText,
                    ManifestText = first.ManifestText.Replace("\"EngineCommit\": \"", "\"EngineCommit\": \"bad-")
                };
                string error;
                Assert.That(AutomatedPairDecisionPolicy.TryApply(unpinned, run.PairSweeps.Count, out error), Is.False);
                Assert.That(error, Does.Contain("commit du moteur"));
            });
            Assert.That(File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)),
                Is.EqualTo(decisionsText), "Le plan reste en lecture seule avant l'accord sur le commit du moteur.");
        }

        private static string TamperFirstHash(string text, string marker)
        {
            int start = text.IndexOf(marker, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0));
            start += marker.Length;
            char replacement = text[start] == '0' ? '1' : '0';
            return text.Substring(0, start) + replacement + text.Substring(start + 1);
        }

        /// <summary>Plan committe (decisions et manifeste appliques) : relu, jamais recalcule (un plan v3 coute ~8 min).</summary>
        private static AutomatedPairDecisionPlan Plan()
        {
            if (_plan == null)
            {
                string manifest = File.ReadAllText(AuthoredRoadModel.FullPath(AutomatedPairDecisionPolicy.ManifestPath));
                _plan = new AutomatedPairDecisionPlan
                {
                    DecisionRunId = UnityEngine.JsonUtility.FromJson<CommittedRun>(manifest).DecisionRunId,
                    DecisionsText = File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)),
                    ManifestText = manifest
                };
                AutomatedPairDecisionPolicy.ValidatePlan(_plan, CommittedPairs());
            }

            return _plan;
        }

        private static int CommittedPairs()
        {
            return UnityEngine.JsonUtility.FromJson<CommittedRun>(File.ReadAllText(
                AuthoredRoadModel.FullPath(AutomatedPairDecisionPolicy.ManifestPath))).Records.Length;
        }

        [Serializable]
        private sealed class CommittedRun
        {
            public string DecisionRunId;
            public CommittedRecord[] Records;
        }

        [Serializable]
        private sealed class CommittedRecord
        {
            public string PairKey;
        }

        private static AuthoredRun Run()
        {
            if (_run == null)
            {
                WithMvpRun(delegate(Scene scene)
                {
                    _run = AuthoredRoadModel.Run(scene,
                        File.ReadAllText(AuthoredRoadModel.FullPath(MigrationReport.LineagePath)),
                        File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)));
                });
            }

            return _run;
        }

        [OneTimeTearDown]
        public void Release()
        {
            _plan = null;
            _run = null;
        }

        private static void WithMvpRun(Action<Scene> body)
        {
            Scene existing = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
            bool loaded = existing.IsValid() && existing.isLoaded;
            if (loaded)
            {
                Assert.That(existing.isDirty, Is.False);
            }

            Scene scene = loaded ? existing : EditorSceneManager.OpenScene(MigrationReport.ScenePath, OpenSceneMode.Additive);
            try
            {
                body(scene);
            }
            finally
            {
                if (!loaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }
    }
}
