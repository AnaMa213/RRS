using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.53 -- faits de la classification v3 appliquee a MVP_Run, lus dans les artefacts committes (manifeste v3 et
    /// manifeste v2 qu'il archive, decisions, modele compile, diff publie) : aucun recalcul de paire.
    /// </summary>
    [Category("Geometry")]
    [Category("Story553")]
    public sealed class Story553MvpRunClassificationTests
    {
        private const string ClassificationDirectory = "_bmad-output/implementation-artifacts/traffic-v2-5-53-classification/";

        private static readonly string[] AuditedZones =
        {
            "458eb40d533eff27d37e8ed61c6eaabb", "4cbd976887b4f64454c372023ad131a2", "47b2bdca8b4d502a90968e846cd49abb", "4b53383c5c1fcf5fddb5b7ee011ed78a",
            "45221b336e33b670c8623e386ad6bf8d", "45c9ea1a463d131a176bf4c5516872a3", "470ef9a2fd6c8d61f93092fe1646ba97", "4f1c595ed26310a51f9f60a0f3ead586",
            "433fa627c90e7aef2af3fd986d2faa8d", "45168e2b5ecebf3c89b80036bbddd0af", "4269dde366f786c6fd4e279786aefc94", "433af752017a55720032fc063252c999",
            "4b38f8659ad8dac11a7d0a0b53fc47b5", "4fedf986cee6f96bf250c5b1f2439e82", "45f7adaf82bc052cf599183a4c402484", "4dbd9c73370da2e3405d8328dacd9d99",
            "4a8d1f62800182ac0fade27255a39190", "415e54e20484285ef32cb5ede00b61ae", "436005183adb61a537157827df3372be", "40bd2f68ba2bc3ff9535b711aea9998f",
            "4b09a0f12076cb983208a9d922a545b7", "465d5eec7c97132ad85c3ac363cca48e"
        };

        private sealed class Archive
        {
            public int DecisionPolicyVersion;
            public string DecisionRunId;
            public string SupersededManifestText;
            public Record[] Records;
        }

        [System.Serializable]
        private sealed class Record
        {
            public string PairKey;
            public string RoadId;
            public string MovementAId;
            public string Classification;
            public string ReasonCode;
            public string Decision;
            public bool Active;
            public bool RefinementComplete;
            public string TypingCanonical;
        }

        [Test]
        public void TheCommittedPlanRevalidatesAndEveryRefinedRejectionIsADecision()
        {
            string decisionsText = File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath));
            var archive = UnityEngine.JsonUtility.FromJson<Archive>(After);
            var plan = new AutomatedPairDecisionPlan { DecisionRunId = archive.DecisionRunId, DecisionsText = decisionsText, ManifestText = After };
            Assert.DoesNotThrow(() => AutomatedPairDecisionPolicy.ValidatePlan(plan, archive.Records.Length),
                "Revision, preuve, typage et decisions lies, relus depuis les artefacts committes.");

            var decisions = AuthoringDecisions.Parse(decisionsText);
            var decided = decisions.Conflicts.ToDictionary(c => AuthoredRoadModel.PairKey(c.MovementKeyA, c.MovementKeyB), c => c);
            int refined = 0;
            foreach (var record in archive.Records.Where(r => r.ReasonCode == "refined-disjoint"))
            {
                refined++;
                Assert.That(record.Active, Is.True, "Un rejet raffine reste candidat : sa decision est ecrite.");
                Assert.That(decided.ContainsKey(record.PairKey), Is.True);
                Assert.That(decided[record.PairKey].Decision, Is.EqualTo(ConflictDecisionKind.Rejected));
            }

            Assert.That(refined, Is.EqualTo(22));
        }

        [Test]
        public void EachCompiledContactStartBelongsToItsOwnMemberInTheProof()
        {
            var archive = UnityEngine.JsonUtility.FromJson<Archive>(After);
            var byZone = archive.Records.Where(r => !string.IsNullOrEmpty(r.RoadId)).ToDictionary(r => r.RoadId, r => r);
            var pattern = new System.Text.RegularExpressions.Regex(@"\|A((?:\[[^\]]*\])*)\|B((?:\[[^\]]*\])*)$");
            int checkedZones = 0;
            foreach (var zone in Model.ConflictZones)
            {
                var record = byZone[zone.Id.ToString()];
                var match = pattern.Match(record.TypingCanonical);
                Assert.That(match.Success, Is.True, record.TypingCanonical);
                for (int m = 0; m < zone.MemberMovementIds.Count; m++)
                {
                    CompiledJunctionMovement movement;
                    Model.TryGetMovement(zone.MemberMovementIds[m], out movement);
                    string intervals = zone.MemberMovementIds[m].ToString() == record.MovementAId ? match.Groups[1].Value : match.Groups[2].Value;
                    float expected = !record.RefinementComplete || intervals.Length == 0 ? 0f
                        : UnityEngine.Mathf.Clamp(float.Parse(intervals.Substring(1, intervals.IndexOf(',') - 1),
                            System.Globalization.CultureInfo.InvariantCulture), 0f, movement.LengthMeters);
                    Assert.That(zone.ContactStartSMeters[m], Is.EqualTo(expected).Within(1e-5f),
                        "Debut de contact du membre " + zone.MemberMovementIds[m] + " dans la zone " + zone.Id);
                }

                checkedZones++;
            }

            Assert.That(checkedZones, Is.EqualTo(Model.ConflictZones.Count));
        }

        private static string _after;
        private static string _before;
        private static CompiledRoadModel _model;

        private static string After
        {
            get { return _after ?? (_after = File.ReadAllText(AuthoredRoadModel.FullPath(AutomatedPairDecisionPolicy.ManifestPath))); }
        }

        private static string Before
        {
            get { return _before ?? (_before = UnityEngine.JsonUtility.FromJson<Archive>(After).SupersededManifestText); }
        }

        private static CompiledRoadModel Model
        {
            get
            {
                return _model ?? (_model = RoadModelCompiler.Compile(RoadModelDocument.Load(
                    File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.ModelPath)))));
            }
        }

        [Test]
        public void TheAppliedPolicyIsV3AndKeepsEveryNonConservativeClassification()
        {
            Assert.That(UnityEngine.JsonUtility.FromJson<Archive>(After).DecisionPolicyVersion, Is.EqualTo(AutomatedPairDecisionPolicy.RefinedDecisionPolicyVersion));
            Assert.That(UnityEngine.JsonUtility.FromJson<Archive>(Before).DecisionPolicyVersion, Is.EqualTo(AutomatedPairDecisionPolicy.KinematicDecisionPolicyVersion),
                "Le manifeste remplace est la v2 signee de la 5.52.");
            var summary = AutomatedPairDecisionPolicy.Summarize(Model, Before, After);
            Assert.That(summary.ChangedOutsideConservative, Is.Empty);
            Assert.That(summary.ProvenBefore, Is.EqualTo(42));
            Assert.That(summary.ProvenUnchanged, Is.EqualTo(42), "Les 42 ConflictProven gardent classification, raison et preuve.");
            Assert.That(summary.ConservativeBefore, Is.EqualTo(94));
            Assert.That(summary.ConservativeRemaining, Is.Zero, string.Join("\n", summary.RemainingConservative.ToArray()));
            Assert.That(summary.ConservativeToDisjoint + summary.ConservativeToProven, Is.EqualTo(94));
        }

        [Test]
        public void TheAuditedOpposedStraightLanesAndGroupAPairsAreResolved()
        {
            var summary = AutomatedPairDecisionPolicy.Summarize(Model, Before, After);
            foreach (var zone in AuditedZones)
            {
                string pair = AutomatedPairDecisionPolicy.PairKeyOfZone(Before, zone);
                Assert.That(pair, Is.Not.Null, zone);
                Assert.That(summary.RemainingConservative, Does.Not.Contain(pair), zone);
            }
        }

        [Test]
        public void EveryCompiledZoneIsTypedAndEveryMergeConvergesIntoACommonExitCorridor()
        {
            var summary = AutomatedPairDecisionPolicy.Summarize(Model, Before, After);
            Assert.That(Model.Version.SchemaVersion, Is.EqualTo(RoadModelCompiler.CompilerSchemaVersion));
            Assert.That(Model.ConflictZones.Count, Is.EqualTo(summary.Crossing + summary.Merge));
            Assert.That(Model.ConflictZones.Count(z => z.Kind == ConflictKind.Merge), Is.EqualTo(summary.Merge));
            Assert.That(summary.Merge, Is.GreaterThan(0));
            foreach (var zone in Model.ConflictZones)
            {
                Assert.That(zone.ContactStartSMeters.Count, Is.EqualTo(zone.MemberMovementIds.Count), zone.Id.ToString());
                for (int m = 0; m < zone.MemberMovementIds.Count; m++)
                {
                    CompiledJunctionMovement movement;
                    Assert.That(Model.TryGetMovement(zone.MemberMovementIds[m], out movement), Is.True);
                    Assert.That(zone.ContactStartSMeters[m], Is.InRange(0f, movement.LengthMeters), zone.Id.ToString());
                }

                if (zone.Kind != ConflictKind.Merge) continue;
                CompiledJunctionMovement a;
                CompiledJunctionMovement b;
                Model.TryGetMovement(zone.MemberMovementIds[0], out a);
                Model.TryGetMovement(zone.MemberMovementIds[1], out b);
                Assert.That(a.ToCorridorId, Is.EqualTo(b.ToCorridorId), "Merge a corridor aval commun : " + zone.Id);
            }
        }

        [Test]
        public void TheCompatibleTraversalTableEqualsThePublishedDiff()
        {
            string published = Directory.GetFiles(AuthoredRoadModel.FullPath(ClassificationDirectory), "diff-paires-*.md")
                .OrderBy(path => path, System.StringComparer.Ordinal).Last();
            string text = File.ReadAllText(published).Replace("\r\n", "\n");
            var summary = AutomatedPairDecisionPolicy.Summarize(Model, Before, After);
            foreach (var entry in summary.CompatibleAfter)
            {
                string row = "| " + entry.Key + " | " + summary.InterApproachPairs[entry.Key] + " | " + summary.CompatibleBefore[entry.Key]
                    + " | " + entry.Value.Count + " |";
                StringAssert.Contains(row, text, "Table publiee : " + Path.GetFileName(published));
                foreach (var pair in entry.Value)
                {
                    StringAssert.Contains("- " + pair + "\n", text);
                }
            }
        }
    }
}
