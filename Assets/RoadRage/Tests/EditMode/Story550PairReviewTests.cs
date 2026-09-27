using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.50 -- revue des paires de conflit : differentiel structure sur la VRAIE authoring de
    /// MVP_Run (ouverte en additif, jamais sauvegardee), actions du proprietaire sur des copies en
    /// memoire des decisions committees (aucun fichier ecrit), isolation de la lecture historique et
    /// fonctions pures de la fenetre.
    /// </summary>
    public sealed class Story550PairReviewTests
    {
        private const string FingerprintA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string FingerprintB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        private const string FingerprintC = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

        // ============================================================ differentiel reel (MVP_Run)

        [Test]
        public void TheReviewCoversEveryHistoricalAndFreshPairExactlyOnce()
        {
            AuthoredRun run;
            PairReviewModel review = Review(out run);
            HistoricalPairFingerprintTable table = HistoricalPairFingerprintTable.Parse(Committed(PairGeometryFingerprint.HistoricalPath));

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in review.Entries)
            {
                Assert.That(keys.Add(entry.PairKey), Is.True, "Paire dupliquee : " + entry.PairKey);
            }

            var expected = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in table.Pairs)
            {
                expected.Add(AuthoredRoadModel.PairKey(record.MovementKeyA, record.MovementKeyB));
            }

            foreach (var candidate in run.Candidates)
            {
                expected.Add(AuthoredRoadModel.PairKey(review.Keys[candidate.MovementA], review.Keys[candidate.MovementB]));
            }

            Assert.That(keys.SetEquals(expected), Is.True);
            int fresh = review.Count(PairReviewStatus.New) + review.Count(PairReviewStatus.Modified) + review.Count(PairReviewStatus.Unchanged);
            int historical = review.Count(PairReviewStatus.Removed) + review.Count(PairReviewStatus.Modified) + review.Count(PairReviewStatus.Unchanged);
            Assert.That(fresh, Is.EqualTo(run.Candidates.Count));
            Assert.That(historical, Is.EqualTo(table.Pairs.Length));
        }

        [Test]
        public void EveryEntryNamesItsJunctionMovementsReasonAndDecisionState()
        {
            AuthoredRun run;
            PairReviewModel review = Review(out run);
            foreach (var entry in review.Entries)
            {
                string subject = entry.PairKey.Replace("\n", " x ");
                Assert.That(entry.JunctionLabel, Is.Not.Empty, subject);
                Assert.That(entry.JunctionLabel, Is.Not.EqualTo(entry.JunctionId.ToString()), "Carrefour nomme : " + subject);
                Assert.That(entry.LabelA, Is.Not.Empty, subject);
                Assert.That(entry.LabelB, Is.Not.Empty, subject);
                Assert.That(entry.Reason, Is.Not.Empty, subject);
                switch (entry.Status)
                {
                    case PairReviewStatus.Modified:
                        Assert.That(entry.GeometryAChanged || entry.GeometryBChanged || entry.VolumeChanged, Is.True, subject);
                        Assert.That(entry.OldSamplesA, Is.Not.Null, subject);
                        Assert.That(entry.NewSamplesA, Is.Not.Null, subject);
                        Assert.That(entry.DecisionState, Is.Not.EqualTo(PairDecisionState.Missing), subject);
                        break;
                    case PairReviewStatus.New:
                        Assert.That(entry.Reason, Does.StartWith(PairReview.HistoricalReason), subject);
                        Assert.That(entry.HasNewVolume, Is.True, subject);
                        Assert.That(entry.HasOldVolume, Is.False, subject);
                        break;
                    case PairReviewStatus.Removed:
                        Assert.That(entry.HasOldVolume, Is.True, subject);
                        Assert.That(entry.HasNewVolume, Is.False, subject);
                        Assert.That(entry.DecisionState, Is.EqualTo(PairDecisionState.Orphan), subject);
                        break;
                }
            }
        }

        [Test]
        public void EveryFreshCandidateIsProvenOrFailsClosedAndNoFollowingPairIsACandidate()
        {
            AuthoredRun run;
            Review(out run);
            var candidates = new HashSet<string>(run.Candidates.Select(c => c.MovementA + "|" + c.MovementB), StringComparer.Ordinal);
            int candidateSweeps = 0;
            foreach (var sweep in run.PairSweeps)
            {
                string pair = sweep.MovementA + "|" + sweep.MovementB;
                CompiledJunctionMovement a;
                CompiledJunctionMovement b;
                Assert.That(run.CandidateModel.TryGetMovement(sweep.MovementA, out a), Is.True);
                Assert.That(run.CandidateModel.TryGetMovement(sweep.MovementB, out b), Is.True);
                Assert.That(a.JunctionId, Is.EqualTo(sweep.JunctionId), "Zone possedee par son carrefour.");
                Assert.That(b.JunctionId, Is.EqualTo(sweep.JunctionId), "Aucune paire entre carrefours.");
                if (sweep.IsCandidate)
                {
                    candidateSweeps++;
                    Assert.That(candidates.Contains(pair), Is.True);
                    Assert.That(sweep.HasVolume, Is.True);
                    Assert.That(sweep.Relation == PairRelation.FailClosed || sweep.ExactProven, Is.True,
                        "Un candidat est prouve par la distance exacte ou retenu par echec ferme.");
                    continue;
                }

                Assert.That(candidates.Contains(pair), Is.False);
                if (sweep.Relation == PairRelation.SameApproach)
                {
                    Assert.That(a.FromCorridorId, Is.EqualTo(b.FromCorridorId));
                }
            }

            Assert.That(candidateSweeps, Is.EqualTo(run.Candidates.Count));
        }

        [Test]
        public void TheDifferentialIsReadOnlyAndNamesJunctionsAndReasons()
        {
            string decisionsBefore = Committed(AuthoredRoadModel.DecisionsPath);
            string text = null;
            string error = null;
            WithMvpRun(scene => text = AuthoredRoadModel.BuildReviewDiff(
                V1SourceSet.Extract(scene),
                Committed(MigrationReport.LineagePath),
                decisionsBefore,
                Committed(PairGeometryFingerprint.HistoricalPath),
                Committed(PairGeometryFingerprint.BaselineHashesPath),
                Committed(PairGeometryFingerprint.BaselineModelPath),
                out error));

            Assert.That(error, Is.Null);
            Assert.That(text, Does.StartWith("# Story 5.50 -- differentiel des paires de conflit"));
            Assert.That(text, Does.Contain("## Paires ecartees par la distance exacte"));
            Assert.That(text, Does.Contain("## Paires de suivi"));
            Assert.That(text, Does.Contain("## Elements plus courts que le gabarit"));
            AuthoredRun run;
            PairReviewModel review = Review(out run);
            foreach (var entry in review.Entries)
            {
                Assert.That(text, Does.Contain("**" + entry.JunctionLabel + "** : " + entry.LabelA + " x " + entry.LabelB));
            }

            Assert.That(Committed(AuthoredRoadModel.DecisionsPath), Is.EqualTo(decisionsBefore), "Le differentiel n'ecrit aucune decision.");
        }

        [Test]
        public void TheHistoricalReaderReadsOnlyTheFormatOneBaseline()
        {
            string baseline = Committed(PairGeometryFingerprint.BaselineModelPath);
            HistoricalMovementReader reader = HistoricalMovementReader.Parse(baseline);
            Assert.That(reader.Movements, Has.Count.EqualTo(72));
            foreach (var movement in reader.Movements.Values)
            {
                Assert.That(movement.Label, Is.Not.Empty);
                Assert.That(movement.Samples.Length, Is.GreaterThan(1));
            }

            Assert.Throws<FormatException>(() => HistoricalMovementReader.Parse(baseline.Replace("{\"Format\":1,", "{\"Format\":2,")));
        }

        [Test]
        public void ATamperedHistoricalTableCannotFeedTheReview()
        {
            string table = Committed(PairGeometryFingerprint.HistoricalPath);
            string hashes = Committed(PairGeometryFingerprint.BaselineHashesPath);
            string baseline = Committed(PairGeometryFingerprint.BaselineModelPath);
            Assert.DoesNotThrow(() => PairGeometryFingerprint.VerifyHistoricalTable(table, hashes, baseline));
            Assert.Throws<FormatException>(() => PairGeometryFingerprint.VerifyHistoricalTable(table + " ", hashes, baseline));
            Assert.Throws<FormatException>(() => PairGeometryFingerprint.VerifyHistoricalTable(table, hashes, baseline + " "));
        }

        // ============================================================ actions du proprietaire (en memoire)

        [Test]
        public void ReconfirmingUnchangedPairsTouchesOnlyPairsIdenticalInTheTable()
        {
            string text = Committed(AuthoredRoadModel.DecisionsPath);
            var committed = AuthoringDecisions.Parse(text).Conflicts;
            var review = new PairReviewModel();
            review.Entries.Add(Entry(committed[0], PairReviewStatus.Unchanged, FingerprintA, FingerprintA));
            review.Entries.Add(Entry(committed[1], PairReviewStatus.Modified, FingerprintB, FingerprintC));
            review.Entries.Add(Entry(committed[2], PairReviewStatus.Unchanged, null, FingerprintA));

            int count;
            string updated = PairReviewActions.ReconfirmUnchanged(text, review, out count);
            var after = AuthoringDecisions.Parse(updated).Conflicts;

            Assert.That(count, Is.EqualTo(1));
            Assert.That(Find(after, committed[0]).GeometryFingerprint, Is.EqualTo(FingerprintA));
            Assert.That(committed[1].GeometryFingerprint, Is.Not.Empty, "Premisse : l'application a emis une empreinte par decision active.");
            Assert.That(Find(after, committed[1]).GeometryFingerprint, Is.EqualTo(committed[1].GeometryFingerprint),
                "Une paire modifiee n'est jamais reconfirmee en lot : son empreinte appliquee reste inchangee.");
            Assert.That(Find(after, committed[2]).GeometryFingerprint, Is.EqualTo(committed[2].GeometryFingerprint),
                "Une paire absente de la table n'est jamais reconfirmee : son empreinte appliquee reste inchangee.");
            Assert.That(after, Has.Count.EqualTo(committed.Count));
            Assert.That(updated, Does.Contain("\"Format\": " + AuthoringDecisions.FormatVersion));
        }

        [Test]
        public void ReconfirmingOnePairKeepsItsDecisionAndWritesTheFreshFingerprint()
        {
            string text = Committed(AuthoredRoadModel.DecisionsPath);
            var committed = AuthoringDecisions.Parse(text).Conflicts;
            var entry = Entry(committed[3], PairReviewStatus.Modified, FingerprintB, FingerprintC);

            var after = AuthoringDecisions.Parse(PairReviewActions.ReconfirmPair(text, entry)).Conflicts;
            ConflictDecision decision = Find(after, committed[3]);
            Assert.That(decision.GeometryFingerprint, Is.EqualTo(FingerprintC));
            Assert.That(decision.Decision, Is.EqualTo(committed[3].Decision));
            Assert.That(decision.Id, Is.EqualTo(committed[3].Id));
            Assert.That(decision.Reason, Is.EqualTo(committed[3].Reason));
            Assert.That(Find(after, committed[4]).GeometryFingerprint, Is.EqualTo(committed[4].GeometryFingerprint),
                "Seule la paire designee est reconfirmee : l'empreinte appliquee des autres reste inchangee.");
        }

        [Test]
        public void DecidingIsRefusedOutsideTheDeterministicPolicy()
        {
            string text = Committed(AuthoredRoadModel.DecisionsPath);
            var committed = AuthoringDecisions.Parse(text).Conflicts;
            var entry = Entry(committed[5], PairReviewStatus.Modified, FingerprintB, FingerprintC);

            Assert.Throws<InvalidOperationException>(() => PairReviewActions.Decide(text, entry, ConflictDecisionKind.Rejected, "Trajectoires separees par le terre-plein."));
            Assert.Throws<InvalidOperationException>(() => PairReviewActions.Decide(text, entry, ConflictDecisionKind.Accepted, "Croisement confirme."));
            Assert.That(PairReviewWindow.AvailableActions(entry), Does.Not.Contain("Rejeter"),
                "Un rejet manuel n'aurait aucun certificat de separation.");
            Assert.That(PairReviewWindow.AvailableActions(entry), Does.Not.Contain("Accepter"),
                "Une acceptation manuelle n'aurait aucune revision liee.");
        }

        [Test]
        public void AcceptingANewPairOutsideThePolicyIsRefused()
        {
            string text = Committed(AuthoredRoadModel.DecisionsPath);
            var decisions = AuthoringDecisions.Parse(text);
            ConflictDecision removed = decisions.Conflicts[6];
            decisions.Conflicts.RemoveAt(6);
            string without = decisions.Serialize();
            var entry = Entry(removed, PairReviewStatus.New, null, FingerprintA);

            Assert.Throws<InvalidOperationException>(() => PairReviewActions.Decide(without, entry, ConflictDecisionKind.Accepted, "Nouvelle convergence."),
                "Une paire nouvelle recoit sa disposition du plan deterministe, jamais d'une action manuelle.");
        }

        [Test]
        public void DisposingRemovesOnlyTheOrphanOfARemovedPair()
        {
            string text = Committed(AuthoredRoadModel.DecisionsPath);
            var committed = AuthoringDecisions.Parse(text).Conflicts;
            var removed = Entry(committed[7], PairReviewStatus.Removed, FingerprintB, null);
            var modified = Entry(committed[7], PairReviewStatus.Modified, FingerprintB, FingerprintC);

            var after = AuthoringDecisions.Parse(PairReviewActions.DisposeRemoved(text, removed)).Conflicts;
            Assert.That(after, Has.Count.EqualTo(committed.Count - 1));
            Assert.That(after.Any(d => d.MovementKeyA == committed[7].MovementKeyA && d.MovementKeyB == committed[7].MovementKeyB), Is.False);
            Assert.Throws<InvalidOperationException>(() => PairReviewActions.DisposeRemoved(text, modified));
        }

        [Test]
        public void NoDecisionCanBeWrittenForAPairThatIsNotAFreshCandidate()
        {
            string text = Committed(AuthoredRoadModel.DecisionsPath);
            var committed = AuthoringDecisions.Parse(text).Conflicts;
            var removed = Entry(committed[8], PairReviewStatus.Removed, FingerprintB, null);

            Assert.Throws<InvalidOperationException>(() => PairReviewActions.ReconfirmPair(text, removed));
            Assert.Throws<InvalidOperationException>(() => PairReviewActions.Decide(text, removed, ConflictDecisionKind.Accepted, "motif"));
        }

        [Test]
        public void ReconfirmingEmitsFormatFourAndFormatThreeReadsAsHistorical()
        {
            string text = Committed(AuthoredRoadModel.DecisionsPath);
            var committed = AuthoringDecisions.Parse(text).Conflicts;
            string updated = PairReviewActions.ReconfirmPair(text, Entry(committed[0], PairReviewStatus.Modified, FingerprintB, FingerprintA));

            Assert.That(updated, Does.Contain("\"Format\": " + AuthoringDecisions.FormatVersion));
            var reread = AuthoringDecisions.Parse(updated).Conflicts;
            Assert.That(Find(reread, committed[0]).GeometryFingerprint, Is.EqualTo(FingerprintA));
            Assert.That(Find(reread, committed[1]).GeometryFingerprint, Is.EqualTo(committed[1].GeometryFingerprint));

            string formatThree = updated.Replace("\"Format\": " + AuthoringDecisions.FormatVersion, "\"Format\": " + AuthoringDecisions.HistoricalFormatVersion);
            var historical = AuthoringDecisions.Parse(formatThree).Conflicts;
            Assert.That(Find(historical, committed[0]).DecisionRevisionId, Is.Null.Or.Empty, "Un format historique se lit comme non confirme.");
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse(updated.Replace(FingerprintA, "pas-une-empreinte")));
        }

        // ============================================================ isolation

        [Test]
        public void OnlyTheReviewWindowCallsTheOwnerActions()
        {
            string[] callers = Sources().Where(path => File.ReadAllText(path).Contains("PairReviewActions.")).ToArray();
            Assert.That(callers.Select(path => Path.GetFileName(path)).ToArray(), Is.EquivalentTo(new[] { "PairReviewWindow.cs" }),
                "Le pipeline, le differentiel et la capture n'appellent jamais une action du proprietaire.");
        }

        [Test]
        public void TheHistoricalReaderIsReferencedOnlyByEditorMigrationCode()
        {
            string[] offenders = Sources()
                .Where(path => !path.Replace('\\', '/').Contains("/Traffic/Migration/") && File.ReadAllText(path).Contains(nameof(HistoricalMovementReader)))
                .ToArray();
            Assert.That(offenders, Is.Empty);
        }

        // ============================================================ fenetre (fonctions pures)

        [Test]
        public void TheWindowFiltersByStatusJunctionAndText()
        {
            var entries = new List<PairReviewEntry>
            {
                Labelled("Roundabout_SouthWest", "entree nord", "anneau", PairReviewStatus.New),
                Labelled("TJunction_East", "tout droit", "gauche", PairReviewStatus.Modified),
                Labelled("TJunction_East", "droite", "gauche", PairReviewStatus.Removed)
            };

            Assert.That(PairReviewWindow.Filter(entries, PairReviewWindow.StatusFilter.Tous, string.Empty, string.Empty), Has.Count.EqualTo(3));
            Assert.That(PairReviewWindow.Filter(entries, PairReviewWindow.StatusFilter.Nouvelles, string.Empty, string.Empty), Has.Count.EqualTo(1));
            Assert.That(PairReviewWindow.Filter(entries, PairReviewWindow.StatusFilter.Tous, "TJunction_East", string.Empty), Has.Count.EqualTo(2));
            Assert.That(PairReviewWindow.Filter(entries, PairReviewWindow.StatusFilter.Tous, string.Empty, "DROITE"), Has.Count.EqualTo(1));
            Assert.That(PairReviewWindow.Title(entries[1]), Does.Contain("TJunction_East").And.Contain("tout droit x gauche").And.Contain("modifiee"));
        }

        [Test]
        public void TheWindowOffersOnlyTheActionsAllowedForThePair()
        {
            var orphan = Labelled("J", "a", "b", PairReviewStatus.Removed);
            orphan.HasDecision = true;
            orphan.DecisionState = PairDecisionState.Orphan;
            Assert.That(PairReviewWindow.AvailableActions(orphan), Is.EqualTo(new[] { "Disposer la decision orpheline" }));

            var stale = Labelled("J", "a", "b", PairReviewStatus.Modified);
            stale.HasDecision = true;
            stale.DecisionState = PairDecisionState.Unconfirmed;
            Assert.That(PairReviewWindow.AvailableActions(stale), Is.EqualTo(new[] { "Reconfirmer cette paire" }));

            var confirmed = Labelled("J", "a", "b", PairReviewStatus.Modified);
            confirmed.HasDecision = true;
            confirmed.DecisionState = PairDecisionState.Confirmed;
            Assert.That(PairReviewWindow.AvailableActions(confirmed), Is.Empty);

            var fresh = Labelled("J", "a", "b", PairReviewStatus.New);
            Assert.That(PairReviewWindow.AvailableActions(fresh), Is.Empty,
                "Une paire nouvelle recoit sa disposition du plan deterministe, jamais d'un bouton manuel.");
        }

        [Test]
        public void SourceNodeKeysSplitAMovementKey()
        {
            Assert.That(PairReviewWindow.SourceNodeKeys("movement:GlobalObjectId_V1-a>GlobalObjectId_V1-b"),
                Is.EqualTo(new[] { "GlobalObjectId_V1-a", "GlobalObjectId_V1-b" }));
            Assert.That(PairReviewWindow.SourceNodeKeys("corridor:x>y"), Is.Empty);
        }

        [Test]
        public void TheReviewWindowIsReachableFromTheMenu()
        {
            MethodInfo open = typeof(PairReviewWindow).GetMethod("Open", BindingFlags.Public | BindingFlags.Static);
            Assert.That(open, Is.Not.Null);
            var menu = (MenuItem)Attribute.GetCustomAttribute(open, typeof(MenuItem));
            Assert.That(menu, Is.Not.Null);
            Assert.That(menu.menuItem, Is.EqualTo(PairReviewWindow.MenuPath));
        }

        // ============================================================ outils

        private static PairReviewEntry Entry(ConflictDecision decision, PairReviewStatus status, string oldFingerprint, string newFingerprint)
        {
            var entry = new PairReviewEntry();
            entry.PairKey = AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB);
            entry.KeyA = decision.MovementKeyA;
            entry.KeyB = decision.MovementKeyB;
            entry.LabelA = decision.MovementKeyA;
            entry.LabelB = decision.MovementKeyB;
            entry.Status = status;
            entry.OldFingerprint = oldFingerprint;
            entry.NewFingerprint = newFingerprint;
            entry.HasDecision = true;
            entry.Decision = decision;
            return entry;
        }

        private static PairReviewEntry Labelled(string junction, string a, string b, PairReviewStatus status)
        {
            var entry = new PairReviewEntry();
            entry.PairKey = "movement:" + a + "\nmovement:" + b + ":" + junction + status;
            entry.JunctionLabel = junction;
            entry.LabelA = a;
            entry.LabelB = b;
            entry.Status = status;
            entry.DecisionState = PairDecisionState.Missing;
            return entry;
        }

        private static ConflictDecision Find(List<ConflictDecision> decisions, ConflictDecision like)
        {
            return decisions.Single(d => d.MovementKeyA == like.MovementKeyA && d.MovementKeyB == like.MovementKeyB);
        }

        private static IEnumerable<string> Sources()
        {
            return Directory.GetFiles(AuthoredRoadModel.FullPath("Assets/RoadRage/Features"), "*.cs", SearchOption.AllDirectories);
        }

        private static AuthoredRun _sharedRun;
        private static PairReviewModel _sharedReview;

        /// <summary>
        /// Run committe, table historique verifiee et revue construite sont des fonctions pures des
        /// fichiers committes et de la scene : les recalculer par test ne change pas un octet du
        /// resultat, seulement le temps. Partage au sein d'UNE execution de fixture : aucun test ne
        /// mute le modele de revue ni le run (lectures seules), le resultat est demonte en fin de
        /// fixture, et le test du differentiel garde son propre appel a BuildReviewDiff pour couvrir
        /// l'API du menu.
        /// </summary>
        private static PairReviewModel Review(out AuthoredRun run)
        {
            if (_sharedReview == null)
            {
                AuthoredRun result = null;
                WithMvpRun(scene => result = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), Committed(AuthoredRoadModel.DecisionsPath)));
                var table = PairGeometryFingerprint.VerifyHistoricalTable(
                    Committed(PairGeometryFingerprint.HistoricalPath),
                    Committed(PairGeometryFingerprint.BaselineHashesPath),
                    Committed(PairGeometryFingerprint.BaselineModelPath));
                _sharedRun = result;
                _sharedReview = PairReview.Build(result, table, Committed(PairGeometryFingerprint.BaselineModelPath));
            }

            run = _sharedRun;
            return _sharedReview;
        }

        [OneTimeTearDown]
        public void ReleaseTheSharedReview()
        {
            // Meme contrat que la fixture 5.28 : le partage ne survit pas a l'execution de fixture.
            _sharedRun = null;
            _sharedReview = null;
        }

        private static string Committed(string projectRelative)
        {
            return File.ReadAllText(AuthoredRoadModel.FullPath(projectRelative));
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
