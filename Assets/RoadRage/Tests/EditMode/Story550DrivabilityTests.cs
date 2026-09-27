using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic.Migration;

namespace RoadRage.Tests.EditMode
{
    public sealed class Story550DrivabilityTests
    {
        [Test]
        public void HistoricalTableCoversThe76CommittedDecisionsExactly()
        {
            string tableText = File.ReadAllText(AuthoredRoadModel.FullPath(PairGeometryFingerprint.HistoricalPath));
            HistoricalPairFingerprintTable table = HistoricalPairFingerprintTable.Parse(tableText);
            AuthoringDecisions decisions = AuthoringDecisions.Parse(File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)));

            Assert.That(table.Pairs, Has.Length.EqualTo(76));
            var active = new HashSet<string>(decisions.Conflicts.Select(delegate(ConflictDecision decision)
            {
                return AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB);
            }), StringComparer.Ordinal);
            var superseding = new HashSet<string>(decisions.Conflicts
                .Where(delegate(ConflictDecision decision) { return !string.IsNullOrEmpty(decision.SupersedesDecisionRevisionId); })
                .Select(delegate(ConflictDecision decision)
                {
                    return AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB);
                }), StringComparer.Ordinal);
            var actual = new HashSet<string>(table.Pairs.Select(delegate(HistoricalPairFingerprintRecord record)
            {
                return AuthoredRoadModel.PairKey(record.MovementKeyA, record.MovementKeyB);
            }), StringComparer.Ordinal);
            Assert.That(actual.Count, Is.EqualTo(table.Pairs.Length), "Aucune paire historique en double.");
            Assert.That(actual.IsSubsetOf(active), Is.True, "Chaque paire historique garde une decision active qui la remplace.");
            Assert.That(superseding.SetEquals(actual), Is.True,
                "Les seules revisions actives qui remplacent l'historique sont les 76 paires de la table.");
        }

        [Test]
        public void HistoricalTableIsReferencedOnlyByEditorMigrationProductionCode()
        {
            string features = AuthoredRoadModel.FullPath("Assets/RoadRage/Features");
            string marker = nameof(HistoricalPairFingerprintTable);
            string[] offenders = Directory.GetFiles(features, "*.cs", SearchOption.AllDirectories)
                .Where(delegate(string path)
                {
                    string normalized = path.Replace('\\', '/');
                    return !normalized.Contains("/Traffic/Migration/") && File.ReadAllText(path).Contains(marker);
                })
                .ToArray();
            Assert.That(offenders, Is.Empty);
        }
    }
}
