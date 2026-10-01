using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.28 -- authoring semantique des carrefours et Gate A. Tout tourne sur la VRAIE
    /// authoring de MVP_Run (ouverte en additif, jamais sauvegardee) et sur les decisions
    /// committees ; chaque refus mute une copie en memoire, jamais un fichier committe. Aucun test
    /// n'ecrit de sign-off : les sign-offs en memoire ne servent qu'a eprouver la verification.
    /// </summary>
    [Category("Geometry")]
    [Timeout(600000)]
    public sealed class Story528AuthoringAndGateATests
    {
        private const string HistoricalSignedDirectory = "_bmad-output/implementation-artifacts/gate-a-5-52/historical-signed-5-51/";
        // ================================================================== nominal

        [Test]
        public void TheCommittedDecisionsCompileAVersionedModelWithOneUncontrolledControlPerApproach()
        {
            var run = Fresh();
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            var model = run.Compiled;
            Assert.That(model.Version.IsEmpty, Is.False);

            // Instantane lie : 40 approches, 72 mouvements (assertions de test, pas regles du pipeline).
            Assert.That(model.Controls.Count, Is.EqualTo(40));
            Assert.That(model.Movements.Count, Is.EqualTo(72));
            Assert.That(model.Controls.All(c => c.Kind == JunctionControlKind.Uncontrolled && !c.HasStopLine), Is.True);

            var coverage = new Dictionary<RoadId, int>();
            foreach (var control in model.Controls)
            {
                var approaches = new HashSet<RoadId>();
                foreach (var id in control.ControlledMovementIds)
                {
                    CompiledJunctionMovement movement;
                    Assert.That(model.TryGetMovement(id, out movement), Is.True);
                    Assert.That(movement.JunctionId, Is.EqualTo(control.JunctionId));
                    approaches.Add(movement.FromCorridorId);
                    coverage[id] = coverage.TryGetValue(id, out int n) ? n + 1 : 1;
                }

                Assert.That(approaches.Count, Is.EqualTo(1), "Un controle lie une seule approche.");
                int fromApproach = model.Movements.Count(m => m.FromCorridorId == approaches.Single());
                Assert.That(control.ControlledMovementIds.Count, Is.EqualTo(fromApproach), "Tous les mouvements de l'approche.");
            }

            Assert.That(coverage.Count, Is.EqualTo(72));
            Assert.That(coverage.Values.All(n => n == 1), Is.True, "Chaque mouvement couvert exactement une fois.");

            Assert.That(model.SignalPlans.Count, Is.EqualTo(0), "Carrefours non signalises : aucun plan.");
            Assert.That(model.Adjacencies.Count, Is.EqualTo(0), "Aucune adjacence.");
            Assert.That(run.Decisions.Dispositions.Count(d => d.Category == "Signal" && d.Kind == TaskDispositionKind.Unsignalized),
                Is.EqualTo(model.Junctions.Count), "Chaque carrefour est declare non signalise.");

            Assert.That(run.ReportText, Does.Contain("Erreurs dures : **0**"));
            Assert.That(run.ReportText, Does.Contain("Taches 5.27 non disposees : **0**"));
            Assert.That(run.ReportText, Does.Contain("`" + model.Version + "`"));
        }

        [Test]
        public void OnlyAcceptedCandidatesBecomeZonesAndEveryZoneComesFromACandidate()
        {
            var run = Fresh();
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            int accepted = run.Decisions.Conflicts.Count(c => c.Decision == ConflictDecisionKind.Accepted);
            Assert.That(run.Compiled.ConflictZones.Count, Is.EqualTo(accepted));
            Assert.That(run.Candidates.Count, Is.EqualTo(run.Decisions.Conflicts.Count), "Chaque candidat a sa decision.");

            foreach (var zone in run.Compiled.ConflictZones)
            {
                var members = zone.MemberMovementIds;
                Assert.That(run.Candidates.Any(c => c.MovementA == members[0] && c.MovementB == members[1] && c.JunctionId == zone.JunctionId), Is.True);
            }

            // Un rejet fabrique sans certificat ProvenDisjoint est refuse au parse : l'absence de
            // temoin de contact ne vaut jamais preuve de separation.
            var decisions = CommittedDecisions();
            var first = decisions.Conflicts[0];
            first.Decision = ConflictDecisionKind.Rejected;
            first.Id = RoadId.None;
            first.Reason = "Test : rejet motive.";
            decisions.Conflicts[0] = first;
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse(decisions.Serialize()));
        }

        [Test]
        public void CandidatesComeFromTheVersionedProfileAndAreInvariantToResamplingAndPairOrder()
        {
            var run = Fresh();

            // Le balayage 5.50 existe des la compilation 1 et AuthoredRun le conserve meme si le
            // rapprochement des decisions attend encore le proprietaire (HALT de la 5.28) : ce test
            // de lecture le couvre sans exiger que le pipeline soit resolu.
            Assert.That(run.CandidateModel, Is.Not.Null, string.Join("\n", run.Failures.ToArray()));
            var model = run.CandidateModel;
            var profile = model.ValidationProfile;
            float radius = AuthoredRoadModel.SweptRadius(profile);
            Assert.That(radius, Is.EqualTo(profile.MaxVehicleHalfWidthMeters + profile.LateralClearanceMarginMeters));

            foreach (var candidate in run.Candidates)
            {
                Assert.That(Movement(model, candidate.MovementA).FromCorridorId, Is.Not.EqualTo(Movement(model, candidate.MovementB).FromCorridorId),
                    "Une paire de meme approche est du suivi, jamais un candidat.");
                Assert.That(candidate.MovementA.CompareTo(candidate.MovementB), Is.LessThan(0), "Paire ordonnee par RoadId.");
            }

            var pair = run.Candidates[0];
            var a = Movement(model, pair.MovementA);
            var b = Movement(model, pair.MovementB);

            // Critere du pipeline 5.50 (distance exacte des empreintes) : verdict et volume
            // identiques dans les deux sens -- l'ordre des paires ne change rien.
            var forward = ConflictSweep.Evaluate(new List<List<SweepPose>> { ConflictSweep.Poses(a.Samples) }, new List<List<SweepPose>> { ConflictSweep.Poses(b.Samples) }, profile);
            var backward = ConflictSweep.Evaluate(new List<List<SweepPose>> { ConflictSweep.Poses(b.Samples) }, new List<List<SweepPose>> { ConflictSweep.Poses(a.Samples) }, profile);
            Assert.That(forward.IsCandidate, Is.True);
            Assert.That(backward.IsCandidate, Is.True);
            Assert.That(backward.Volume.Center, Is.EqualTo(forward.Volume.Center));
            Assert.That(backward.Volume.Extents, Is.EqualTo(forward.Volume.Extents));

            // Meme courbe, echantillonnage double : meme verdict. La borne conservatrice de la 5.50
            // entre poses ne perd jamais un contact vu par l'echantillonnage direct.
            var resampled = Resample(a.Samples, a.Curve);
            Assert.That(resampled.Count, Is.GreaterThan(a.Samples.Count));
            var dense = ConflictSweep.Evaluate(new List<List<SweepPose>> { ConflictSweep.Poses(resampled) }, new List<List<SweepPose>> { ConflictSweep.Poses(b.Samples) }, profile);
            Assert.That(dense.IsCandidate, Is.True);

            // Et une paire disjointe reste disjointe apres re-echantillonnage.
            var far = FarPair(model);
            RoadBoundsBox none;
            Assert.That(AuthoredRoadModel.SweptOverlap(far.Key.Samples, far.Value.Samples, profile, out none), Is.False);
            Assert.That(AuthoredRoadModel.SweptOverlap(Resample(far.Key.Samples, far.Key.Curve), far.Value.Samples, profile, out none), Is.False);
        }

        [Test]
        public void LocalizationFixturesOnTheRealMapAreGreen()
        {
            var run = Fresh();
            Assert.That(run.Fixtures.Select(f => f.Name).ToArray(),
                Is.EqualTo(new[] { "nominal", "frontiere", "deplace", "contresens", "carrefour ambigu", "hors corridor" }));
            foreach (var fixture in run.Fixtures)
            {
                Assert.That(fixture.Passed, Is.True, fixture.Name + " : " + fixture.Observed);
                Assert.That(run.ReportText, Does.Contain("| " + fixture.Name + " |"));
            }
        }

        [Test]
        public void AFreshProposalParsesAndCompilesWithTheSameCandidateSet()
        {
            string text = null;
            string error = null;
            WithMvpRun(scene => text = AuthoredRoadModel.ProposeText(V1SourceSet.Extract(scene), Committed(MigrationReport.LineagePath), out error));
            Assert.That(text, Is.Not.Null, error);

            var proposed = AuthoringDecisions.Parse(text);
            var committed = CommittedDecisions();
            Assert.That(proposed.Controls.Select(c => c.ApproachKey), Is.EqualTo(committed.Controls.Select(c => c.ApproachKey)));
            Assert.That(proposed.Conflicts.Select(c => c.MovementKeyA + c.MovementKeyB), Is.EqualTo(committed.Conflicts.Select(c => c.MovementKeyA + c.MovementKeyB)));
            Assert.That(RunWith(text).Succeeded, Is.True);
        }

        // ================================================================== matrice : refus

        [Test]
        public void AnApproachWithoutControlOrAnOrphanControlIsAHardFailureNamingTheKey()
        {
            var decisions = CommittedDecisions();
            string removed = decisions.Controls[0].ApproachKey;
            decisions.Controls.RemoveAt(0);
            AssertRefused(RunWith(decisions.Serialize()), "Approche sans controle : '" + removed + "'");

            decisions = CommittedDecisions();
            decisions.Controls.Add(new ControlDecision { Id = RoadId.New(), ApproachKey = "corridor:inconnue", Kind = JunctionControlKind.Uncontrolled });
            AssertRefused(RunWith(decisions.Serialize()), "cle d'approche inconnue 'corridor:inconnue'");
        }

        [Test]
        public void ANonAdmittedControlKindIsAHardFailure()
        {
            var decisions = CommittedDecisions();
            var control = decisions.Controls[0];
            control.Kind = JunctionControlKind.Stop;
            decisions.Controls[0] = control;
            AssertRefused(RunWith(decisions.Serialize()), "5.35");
        }

        [Test]
        public void AnUndisposedCandidateOrADecisionWithoutCandidateIsAHardFailure()
        {
            var decisions = CommittedDecisions();
            decisions.Conflicts.RemoveAt(0);
            AssertRefused(RunWith(decisions.Serialize()), "Candidat de conflit non dispose");

            // Paire de meme approche : jamais un candidat, donc sa decision est orpheline.
            var import = Fresh().Import;
            var sameApproach = import.Movements.GroupBy(m => m.From.Key).First(g => g.Count() >= 2).Select(m => m.Key).OrderBy(k => k, StringComparer.Ordinal).Take(2).ToArray();
            decisions = CommittedDecisions();
            ConflictDecision orphan = decisions.Conflicts[0];
            orphan.Id = RoadId.New();
            orphan.MovementKeyA = sameApproach[0];
            orphan.MovementKeyB = sameApproach[1];
            orphan.Reason = "Test";
            decisions.Conflicts.Add(orphan);
            AssertRefused(RunWith(decisions.Serialize()), "Decision de conflit sans candidat");
        }

        [Test]
        public void AReviewedWidthIsAppliedExactlyAsAuthoredIncludingAnAsymmetricOne()
        {
            // Committees : anneau 4,0 / 4,0 contre une amorce de 2,0 / 2,0 -- appliquee, rapportee importee / appliquee (5.49).
            var run = Fresh();
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            string ring = RingSectionKey(run.Import);
            var reported = run.Widths.Single(w => w.SubjectKey == ring);
            Assert.That(reported.ImportedLeftMax, Is.EqualTo(2f).Within(1e-3f), "Amorce de l'importeur, jamais une autorite.");
            Assert.That(reported.AppliedLeftMin, Is.EqualTo(4f).Within(1e-3f));
            Assert.That(run.ReportText, Does.Contain("| Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |"));

            // AD-45 asymetrique : ecrite exactement telle qu'authoree sur chaque echantillon possede.
            var decisions = CommittedDecisions();
            SetWidth(decisions, ring, 3.5f, 4f, WidthApplication.Uniform);
            var asymmetric = RunWithAutomated(decisions);
            Assert.That(asymmetric.Succeeded, Is.True, string.Join("\n", asymmetric.Failures.ToArray()));
            foreach (var corridor in run.Import.Sections.Single(s => s.Key == ring).Corridors)
            {
                Assert.That(asymmetric.Compiled.TryGetCorridor(asymmetric.Import.IdOf(corridor.Key), out EffectiveLaneCorridor compiled), Is.True);
                Assert.That(compiled.Samples.All(s => s.HalfWidthLeftMeters == 3.5f && s.HalfWidthRightMeters == 4f), Is.True);
            }

            // Carrefour (non giratoire) en Uniform a une largeur differente de ses corridors d'extremite (2,0) :
            // la decision est bien ecrite sur les mouvements, donc le compilateur refuse la marche de largeur aux coutures.
            string crossing = run.Import.Junctions.First(j => j.Module.Kind != V1ModuleKind.Roundabout).Key;
            decisions = CommittedDecisions();
            SetWidth(decisions, crossing, 2.5f, 3f, WidthApplication.Uniform);
            AssertRefused(RunWith(decisions.Serialize()), "MovementSeamBroken");

            decisions = CommittedDecisions();
            decisions.Widths.RemoveAt(0);
            AssertRefused(RunWith(decisions.Serialize()), "Largeur non revue");
        }

        [Test]
        public void AWidthBelowTheGaugeAForbiddenModeABrokenFloorOrAWidenedPortalIsAHardFailure()
        {
            var import = Fresh().Import;
            string ring = RingSectionKey(import);

            // Sous le gabarit (demi-gabarit 1,03 + marge 0,25) : echec nommant sujet et echantillon.
            var decisions = CommittedDecisions();
            SetWidth(decisions, ring, 1.2f, 4f, WidthApplication.Uniform);
            AssertRefused(RunWith(decisions.Serialize()), "Largeur sous le gabarit pour '" + ring + "' : echantillon 0");

            // Idem sur un carrefour en Uniform : la regle vaut pour tout echantillon possede.
            string crossing = import.Junctions.First(j => j.Module.Kind != V1ModuleKind.Roundabout).Key;
            decisions = CommittedDecisions();
            SetWidth(decisions, crossing, 1.2f, 2f, WidthApplication.Uniform);
            AssertRefused(RunWith(decisions.Serialize()), "Largeur sous le gabarit pour '" + crossing + "' : echantillon 0");

            // EndpointInterpolation reserve aux carrefours.
            decisions = CommittedDecisions();
            SetWidth(decisions, ring, 4f, 4f, WidthApplication.EndpointInterpolation);
            AssertRefused(RunWith(decisions.Serialize()), "Mode d'application de largeur interdit pour la section '" + ring + "'");

            // Plancher : l'entree interpole depuis l'approche a 2,0 m, sous un plancher de 3,0 m.
            string roundabout = import.Junctions.First(j => j.Module.Kind == V1ModuleKind.Roundabout).Key;
            decisions = CommittedDecisions();
            SetWidth(decisions, roundabout, 3f, 3f, WidthApplication.EndpointInterpolation);
            AssertRefused(RunWith(decisions.Serialize()), "Plancher de largeur viole pour '" + roundabout + "'");

            // Portail : son enveloppe derive de la largeur importee, un sujet elargi est refuse.
            string portal = import.Portals[0].Corridor.SectionKey;
            decisions = CommittedDecisions();
            SetWidth(decisions, portal, 2.5f, 2.5f, WidthApplication.Uniform);
            AssertRefused(RunWith(decisions.Serialize()), "Portail sur sujet elargi : '" + portal + "'");
        }

        private static string RingSectionKey(V1ImportResult import)
        {
            return import.Sections.First(s => s.Corridors.Any(c => c.IsRing)).Key;
        }

        private static void SetWidth(AuthoringDecisions decisions, string key, float left, float right, WidthApplication application)
        {
            int index = decisions.Widths.FindIndex(w => w.SubjectKey == key);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), key);
            decisions.Widths[index] = new WidthDecision { SubjectKey = key, HalfWidthLeftMeters = left, HalfWidthRightMeters = right, Application = application };
        }

        [Test]
        public void AnUndisposedTaskOrAnOrphanDispositionIsAHardFailure()
        {
            var decisions = CommittedDecisions();
            var removed = decisions.Dispositions[0];
            decisions.Dispositions.RemoveAt(0);
            AssertRefused(RunWith(decisions.Serialize()), "Tache non disposee : " + removed.Category);

            decisions = CommittedDecisions();
            decisions.Dispositions.Add(new TaskDisposition { Category = "Frontiere", SubjectKey = "junction:inconnue", Kind = TaskDispositionKind.Reviewed, Note = "Test" });
            AssertRefused(RunWith(decisions.Serialize()), "Disposition orpheline");

            decisions = CommittedDecisions();
            decisions.DeferredFields.RemoveAt(0);
            AssertRefused(RunWith(decisions.Serialize()), "Champ differe sans reouverture");
        }

        [Test]
        public void AStaleLineageIsRefusedWithTheMigrationToRerun()
        {
            string lineage = Committed(MigrationReport.LineagePath);
            var match = Regex.Match(lineage, "\"Key\": \"movement:");
            Assert.That(match.Success, Is.True);
            string stale = lineage.Substring(0, match.Index) + "\"Key\": \"movementX:" + lineage.Substring(match.Index + match.Length);

            AuthoredRun run = null;
            WithMvpRun(scene => run = AuthoredRoadModel.Run(scene, stale, Committed(AuthoredRoadModel.DecisionsPath)));
            AssertRefused(run, "Lignee perimee");
            Assert.That(run.Failures.Single(), Does.Contain("5.27"));
        }

        [Test]
        public void AStaleFingerprintIsAHistoricalProposalThatBlocksTheGate()
        {
            // Une decision appliquee dont l'empreinte ne correspond plus a la fraiche est une
            // proposition historique : elle n'est ni appliquee, ni convertie en zone.
            var decisions = CommittedDecisions();
            int index = decisions.Conflicts.FindIndex(delegate (ConflictDecision decision)
            {
                return !string.IsNullOrEmpty(decision.GeometryFingerprint);
            });
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "Premisse : chaque decision appliquee porte une empreinte.");
            ConflictDecision stale = decisions.Conflicts[index];
            stale.GeometryFingerprint = new string('a', 64);
            decisions.Conflicts[index] = stale;

            var unconfirmed = RunWith(decisions.Serialize());
            AssertRefused(unconfirmed, "Proposition historique non reconfirmee");

            // Une decision non reconfirmee compte comme ouverte : Gate A fermee, cause nommee.
            var fresh = Fresh();
            string signoff = AuthoredRoadModel.RenderSignoff(fresh, "Testeur", "test@example.invalid", fresh.Overlay.Select(i => i.Key).ToList(), new DateTime(2026, 9, 28, 0, 0, 0, DateTimeKind.Utc));
            var reasons = AuthoredRoadModel.EvaluateGateA(unconfirmed, fresh.ReportText, fresh.ModelText, fresh.OverlayText, signoff);
            Assert.That(reasons.Any(r => r.Contains("Proposition historique non reconfirmee")), Is.True, string.Join("\n", reasons.ToArray()));
        }

        [Test]
        public void ATamperedOrDivergentPersistedModelIsRefusedNeverRepaired()
        {
            string model = Committed(AuthoredRoadModel.ModelPath);
            Assert.That(RoadModelCompiler.Compile(RoadModelDocument.Load(model)).Version, Is.EqualTo(Fresh().Compiled.Version));

            // Un label n'entre pas dans la version, mais c'est un octet modifie : refuse par l'integrite.
            string label = model.Replace("MVP_Run (authoring 5.28)", "MVP_Run (authoring 5.2X)");
            Assert.That(label, Is.Not.EqualTo(model));
            Assert.That(Assert.Throws<FormatException>(() => RoadModelDocument.Load(label)).Message, Does.Contain("integrite"));

            string spaced = model.Replace("{\"Format\":2,", "{\"Format\": 2,");
            Assert.That(Assert.Throws<FormatException>(() => RoadModelDocument.Load(spaced)).Message, Does.Contain("non canonique"));

            // Chaque champ de liaison, un par un : un caractere de chaque hash ou de la version, chaque entier.
            foreach (var field in new[] { "ModelVersion", "SourceHash", "LineageHash", "DecisionsHash", "IntegrityHash" })
            {
                var match = Regex.Match(model, "\"" + field + "\":\"(v4:)?([0-9a-f])");
                Assert.That(match.Success, Is.True, field);
                int at = match.Groups[2].Index;
                string edited = model.Substring(0, at) + (model[at] == '0' ? '1' : '0') + model.Substring(at + 1);
                Assert.Throws<FormatException>(() => RoadModelDocument.Load(edited), field);
            }

            foreach (var field in new[] { "CompilerSchemaVersion", "ImporterVersion", "PipelineVersion" })
            {
                var match = Regex.Match(model, "\"" + field + "\":([0-9]+)");
                Assert.That(match.Success, Is.True, field);
                string bumped = "\"" + field + "\":" + (int.Parse(match.Groups[1].Value) + 1);
                string edited = model.Substring(0, match.Index) + bumped + model.Substring(match.Index + match.Length);
                Assert.Throws<FormatException>(() => RoadModelDocument.Load(edited), field);
            }
        }

        [Test]
        public void ASyntheticModelFillingEveryRecordKindRoundTripsToTheSameBytesAndVersion()
        {
            // Modele 5.25 complet (connexion, adjacence, plan de signal a groupes/phases/etats,
            // ligne d'arret, tombstone, remap), repris par reflexion : les tests 5.25 restent intacts.
            var build = typeof(Story525RoadWorldModelTests).GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(build, Is.Not.Null);
            var source = (RoadModelSource)build.Invoke(null, null);
            source.DrivabilityProfile = V1RoadModelImporter.DrivabilityProfile();

            // Story 5.50 : le demi-tour M3 relie des bandes espacees de 5 m (rayon 2,5 m) : sous
            // R_adm = 4,0344 m il n'est pas admissible. La copie 5.28 le retire -- le type
            // JunctionMovement reste couvert par M1, M2 et M4, admissibles -- et retire ses
            // references du controle, de la zone de conflit et du groupe de signalisation.
            var m2 = source.Movements.Single(m => m.Label == "M2");
            var m3 = source.Movements.Single(m => m.Label == "M3");
            var m4 = source.Movements.Single(m => m.Label == "M4");
            source.Movements = source.Movements.Where(m => m.Id != m3.Id).ToArray();
            for (int i = 0; i < source.Controls.Length; i++)
            {
                source.Controls[i].ControlledMovementIds = source.Controls[i].ControlledMovementIds.Where(id => id != m3.Id).ToArray();
            }

            source.ConflictZones[0].MemberMovementIds = new[] { m2.Id, m4.Id };
            source.SignalPlans[0].Groups[1].MemberMovementIds = new[] { m4.Id };

            source.Corridors[1].HasSpeedLimitOverride = true;
            source.Corridors[1].SpeedLimitOverrideMetersPerSecond = 8.5f;
            source.Corridors[1].HasSurfaceOverride = true;
            source.Corridors[1].SurfaceOverride = RoadSurface.Gravel;
            source.Corridors[1].HasAllowedVehicleClassesOverride = true;
            source.Corridors[1].AllowedVehicleClassesOverride = VehicleClassMask.Car | VehicleClassMask.Bus;

            Assert.That(source.Connections.Length, Is.GreaterThan(0));
            Assert.That(source.Adjacencies.Length, Is.GreaterThan(0));
            Assert.That(source.SignalPlans.Length, Is.GreaterThan(0));
            Assert.That(source.SignalPlans[0].Groups.Length, Is.GreaterThan(0));
            Assert.That(source.SignalPlans[0].Phases.Length, Is.GreaterThan(0));
            Assert.That(source.Controls.Any(c => c.HasStopLine), Is.True);
            Assert.That(source.Manifest.TombstonedIds.Length, Is.GreaterThan(0));
            Assert.That(source.Manifest.Remaps.Length, Is.GreaterThan(0));

            var provenance = new RoadModelProvenance { SourceHash = "a", LineageHash = "b", DecisionsHash = "c", ImporterVersion = 1, PipelineVersion = 1 };
            string text = RoadModelDocument.Serialize(source, provenance);
            var loaded = RoadModelDocument.Load(text, out RoadModelProvenance reread);
            Assert.That(RoadModelDocument.Serialize(loaded, reread), Is.EqualTo(text));
            Assert.That(RoadModelCompiler.Compile(loaded).Version, Is.EqualTo(RoadModelCompiler.Compile(source).Version));

            // Valeurs non declarees : refusees au chargement, jamais aller-retour.
            Assert.That(text, Does.Contain("\"SurfaceOverride\":\"Gravel\""));
            Assert.Throws<FormatException>(() => RoadModelDocument.Load(text.Replace("\"SurfaceOverride\":\"Gravel\"", "\"SurfaceOverride\":\"7\"")));
        }

        [Test]
        public void ThePersistedModelRoundTripsToTheSameBytesAndVersion()
        {
            string text = Committed(AuthoredRoadModel.ModelPath);
            var source = RoadModelDocument.Load(text, out RoadModelProvenance provenance);
            Assert.That(RoadModelDocument.Serialize(source, provenance), Is.EqualTo(text));
            var reloaded = RoadModelDocument.Load(RoadModelDocument.Serialize(source, provenance));
            Assert.That(RoadModelCompiler.Compile(reloaded).Version, Is.EqualTo(RoadModelCompiler.Compile(source).Version));
            Assert.That(RoadModelCompiler.Compile(source).Version.ToString(), Is.EqualTo(Regex.Match(text, "\"ModelVersion\":\"(v4:[0-9a-f]{32})\"").Groups[1].Value));
            Assert.That(provenance.DecisionsHash, Is.EqualTo(V1SourceSet.Sha256Hex(Committed(AuthoredRoadModel.DecisionsPath))));
            Assert.That(provenance.LineageHash, Is.EqualTo(V1SourceSet.Sha256Hex(Committed(MigrationReport.LineagePath))));
        }

        // ================================================================== liaison et Gate A

        [Test]
        public void TheCommittedArtifactsEqualAFreshPipeline()
        {
            var run = Fresh();
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            string decisions = Committed(AuthoredRoadModel.DecisionsPath);
            Assert.That(AuthoringDecisions.Parse(decisions).Serialize(), Is.EqualTo(decisions), "Decisions committees sous leur forme triee.");
            Assert.That(run.ModelText, Is.EqualTo(Committed(AuthoredRoadModel.ModelPath)), "Relancer RoadRage/Traffic V2/Compiler le modele authore.");
            Assert.That(run.OverlayText, Is.EqualTo(Committed(AuthoredRoadModel.OverlayPath)));
            Assert.That(run.ReportText, Is.EqualTo(Committed(AuthoredRoadModel.ReportPath)));
            Assert.That(AuthoredRoadModel.VerifyReport(Committed(AuthoredRoadModel.ReportPath), run), Is.Empty);
            Assert.That(run.Overlay.Count, Is.EqualTo(25), "Une instance d'overlay par module de l'ensemble source.");
            Assert.That(run.Overlay.All(i => i.Primitives.Count > 0), Is.True);
        }

        [Test]
        public void AStaleOrHandEditedReportIsRejected()
        {
            // Pendant le HALT de la 5.50 (decisions en attente), le pipeline frais n'aboutit pas :
            // l'absence de rapport est constatee par la garde, jamais par un acces non garde.
            var run = Fresh();
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            string edited = run.ReportText.Replace("6 vertes sur 6", "6 vertes sur 6 (edite)");
            Assert.That(AuthoredRoadModel.VerifyReport(edited, run).Single(), Does.Contain("body-hash"));

            string stale = run.ReportText.Replace("decisions-hash: " + run.Binding.DecisionsHash, "decisions-hash: " + new string('0', 64));
            Assert.That(AuthoredRoadModel.VerifyReport(stale, run).Single(), Does.Contain("decisions-hash"));

            string detached = run.ReportText.Substring(run.ReportText.IndexOf("-->\n", StringComparison.Ordinal) + 4);
            Assert.That(AuthoredRoadModel.VerifyReport(detached, run).Single(), Does.Contain("detache"));

            // Corps reecrit avec un body-hash recalcule, en-tete intact : seule l'egalite octet pour octet l'attrape.
            int end = run.ReportText.IndexOf("\n-->\n", StringComparison.Ordinal) + "\n-->\n".Length;
            string rewrittenBody = run.ReportText.Substring(end).Replace("6 vertes sur 6", "6 vertes sur 6 (reecrit)");
            string rewritten = run.ReportText.Substring(0, end).Replace("body-hash: " + run.Binding.BodyHash, "body-hash: " + V1SourceSet.Sha256Hex(rewrittenBody)) + rewrittenBody;
            Assert.That(AuthoredRoadModel.VerifyReport(rewritten, run).Single(), Does.Contain("octet pour octet"));

            string extraHeader = run.ReportText.Replace("-->\n", "extra-field: x\n-->\n");
            Assert.That(AuthoredRoadModel.VerifyReport(extraHeader, run).Single(), Does.Contain("octet pour octet"));
        }

        [Test]
        public void ASignoffBindsTheFreshPipelineAndAStaleOneKeepsGateAClosed()
        {
            // Sign-off en memoire, jamais ecrit : il eprouve la verification, il ne signe rien.
            var run = Fresh();
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            var instances = run.Overlay.Select(i => i.Key).ToList();
            string signoff = AuthoredRoadModel.RenderSignoff(run, "Testeur", "test@example.invalid", instances, new DateTime(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc));
            var layout = JsonUtility.FromJson<CommittedSignoff>(signoff);
            Assert.That(layout.Format, Is.EqualTo(AuthoredRoadModel.SignoffFormat));
            Assert.That(layout.TrackingAllowanceMeters, Is.EqualTo(0.34f));
            Assert.That(layout.PoseModel, Is.EqualTo("kinematic-v1"));
            Assert.That(layout.EvidenceParametersHash, Is.EqualTo(V1SourceSet.Sha256Hex(run.EvidenceParameters.CanonicalText)));
            Assert.That(layout.SignedRingSeams, Is.EqualTo(run.SignedRingSeams));
            Assert.That(AuthoredRoadModel.VerifySignoff(signoff, run), Is.Empty);
            Assert.That(instances.Count, Is.EqualTo(25));
            Assert.That(run.ModelText, Does.Not.Contain("Testeur"), "L'identite d'approbation n'entre dans aucun hash de modele.");

            foreach (var field in new[] { "OverlayHash", "SourceHash", "LineageHash", "DecisionsHash", "ModelHash", "PhysicalInputHash", "SemanticInputHash", "ClearanceHash" })
            {
                string stale = Regex.Replace(signoff, "\"" + field + "\": \"[0-9a-f]{64}\"", "\"" + field + "\": \"" + new string('0', 64) + "\"");
                Assert.That(stale, Is.Not.EqualTo(signoff), field);
                Assert.That(AuthoredRoadModel.VerifySignoff(stale, run).Single(), Does.Contain("perime"), field);
            }

            string otherVersion = signoff.Replace(run.Binding.RoadModelVersion, "v4:" + new string('0', 32));
            Assert.That(AuthoredRoadModel.VerifySignoff(otherVersion, run).Single(), Does.Contain("road-model-version"));

            string partial = AuthoredRoadModel.RenderSignoff(run, "Testeur", "test@example.invalid", instances.Skip(1).ToList(), DateTime.UtcNow);
            Assert.That(AuthoredRoadModel.VerifySignoff(partial, run).Single(), Does.Contain("instances revues"));

            string anonymous = AuthoredRoadModel.RenderSignoff(run, " ", "", instances, DateTime.UtcNow);
            Assert.That(AuthoredRoadModel.VerifySignoff(anonymous, run).Single(), Does.Contain("approbateur"));

            Assert.That(AuthoredRoadModel.VerifySignoff(null, run).Single(), Does.Contain("absent"));
        }

        [Test]
        public void GateAComparesTheModelAndOverlayOnDiskWithTheFreshRun()
        {
            var run = Fresh();
            Assert.That(AuthoredRoadModel.VerifyArtifacts(run, run.ModelText, run.OverlayText), Is.Empty);
            Assert.That(AuthoredRoadModel.VerifyArtifacts(run, run.ModelText + " ", run.OverlayText).Single(), Does.Contain(AuthoredRoadModel.ModelPath));
            Assert.That(AuthoredRoadModel.VerifyArtifacts(run, run.ModelText, null).Single(), Does.Contain(AuthoredRoadModel.OverlayPath));
        }

        [Test]
        public void ANonPositiveSweptRadiusIsRefusedInsteadOfLoopingForever()
        {
            var run = Fresh();
            Assert.That(run.CandidateModel, Is.Not.Null, string.Join("\n", run.Failures.ToArray()));
            var curve = run.CandidateModel.Movements[0].Curve;
            RoadBoundsBox volume;
            Assert.Throws<ArgumentOutOfRangeException>(() => AuthoredRoadModel.SweptOverlap(curve, curve, 0f, out volume));
            Assert.Throws<ArgumentOutOfRangeException>(() => AuthoredRoadModel.SweptOverlap(curve, curve, -1f, out volume));
        }

        // ================================================================== decisions : parse strict et amorce

        [Test]
        public void ParseRefusesDuplicatesOutOfOrderListsAndUndeclaredEnumValues()
        {
            string a = new string('a', 32);
            string b = new string('b', 32);
            const string Empty = "\"Conflicts\":[],\"Widths\":[],\"Dispositions\":[],\"DeferredFields\":[]}";

            // Temoin valide.
            Assert.DoesNotThrow(() => AuthoringDecisions.Parse("{\"Format\":2,\"Controls\":[{\"Id\":\"" + a + "\",\"ApproachKey\":\"a\",\"Kind\":\"Uncontrolled\"}]," + Empty));

            // Liste hors d'ordre.
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse("{\"Format\":2,\"Controls\":["
                + "{\"Id\":\"" + a + "\",\"ApproachKey\":\"b\",\"Kind\":\"Uncontrolled\"},"
                + "{\"Id\":\"" + b + "\",\"ApproachKey\":\"a\",\"Kind\":\"Uncontrolled\"}]," + Empty));

            // Meme paire acceptee puis rejetee.
            var pair = new AuthoringDecisions();
            pair.Conflicts.Add(new ConflictDecision { Id = RoadId.New(), MovementKeyA = "m:a", MovementKeyB = "m:b", Decision = ConflictDecisionKind.Accepted, Reason = "x" });
            pair.Conflicts.Add(new ConflictDecision { MovementKeyA = "m:a", MovementKeyB = "m:b", Decision = ConflictDecisionKind.Rejected, Reason = "y" });
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse(pair.Serialize()));

            // Meme sujet de largeur deux fois.
            var widths = new AuthoringDecisions();
            widths.Widths.Add(new WidthDecision { SubjectKey = "section:s", HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f });
            widths.Widths.Add(new WidthDecision { SubjectKey = "section:s", HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f });
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse(widths.Serialize()));

            // Valeurs d'enum numeriques non declarees.
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse("{\"Format\":2,\"Controls\":[{\"Id\":\"" + a + "\",\"ApproachKey\":\"a\",\"Kind\":\"7\"}]," + Empty));
            var undeclared = new AuthoringDecisions();
            undeclared.Conflicts.Add(new ConflictDecision { MovementKeyA = "m:a", MovementKeyB = "m:b", Decision = ConflictDecisionKind.Rejected, Reason = "y" });
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse(undeclared.Serialize().Replace("\"Rejected\"", "\"2\"")));
            var deferred = new AuthoringDecisions();
            deferred.DeferredFields.Add(new DeferredField { Field = DeferredFieldKind.Surface, Reopening = "r" });
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse(deferred.Serialize().Replace("\"Surface\"", "\"9\"")));
        }

        [Test]
        public void TheProposalNeverOverwritesAnExistingDestination()
        {
            string path = Path.Combine(Path.GetTempPath(), "rrs-528-propose-" + Guid.NewGuid().ToString("N") + ".json");
            Assert.That(AuthoredRoadModel.ProposeRefusal(path), Is.Null, "Destination libre : l'amorce peut ecrire.");
            File.WriteAllText(path, "existant");
            try
            {
                Assert.That(AuthoredRoadModel.ProposeRefusal(path), Does.Contain("existe deja"));
                Assert.That(File.ReadAllText(path), Is.EqualTo("existant"));
            }
            finally
            {
                File.Delete(path);
            }
        }

        // ================================================================== liaison physique (correct-course 2026-09-25 / 2026-09-28)

        [Test]
        public void ThePhysicalEvidenceCoversTheNineJunctionsWithStrictlyPositiveResidualsOutsideTheModel()
        {
            var run = Fresh();
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            Assert.That(run.EvidenceFailures, Is.Empty);
            Assert.That(AuthoredRoadModel.VerifyEvidence(run), Is.Empty);

            // 5 carrefours classiques : chaque mouvement, deux gates (5.51).
            var conventional = run.Import.Movements.Where(m => m.Module.Kind == V1ModuleKind.Crossroads || m.Module.Kind == V1ModuleKind.TJunction)
                .Select(m => m.Module.Label + "|" + m.Label).Distinct().OrderBy(s => s, StringComparer.Ordinal).ToArray();
            Assert.That(run.JunctionEvidence.Rows.Select(r => r.Junction + "|" + r.Movement).Distinct().OrderBy(s => s, StringComparer.Ordinal), Is.EqualTo(conventional));
            Assert.That(run.JunctionEvidence.Rows.All(r => r.Physical.Residual > 0f && r.Semantic.Residual > 0f), Is.True);

            // 4 giratoires : chaque mouvement prolonge et chaque corridor d'anneau, gate physique.
            var roundabout = run.Import.Movements.Where(m => m.Module.Kind == V1ModuleKind.Roundabout)
                .Concat(run.Import.Corridors.Where(c => c.IsRing))
                .Select(m => m.Module.Label + "|" + m.Label).OrderBy(s => s, StringComparer.Ordinal).ToArray();
            Assert.That(run.RoundaboutEvidence.Rows.Select(r => r.Junction + "|" + r.Movement).OrderBy(s => s, StringComparer.Ordinal), Is.EqualTo(roundabout));
            Assert.That(run.RoundaboutEvidence.Rows.Select(r => r.Junction).Distinct().Count(), Is.EqualTo(4));
            Assert.That(run.RoundaboutEvidence.Rows.All(r => r.Physical.Residual > 0f && r.Semantic == null), Is.True);
            Assert.That(run.Roundabouts.All(r => r.EnvelopeResidual > 0f && r.PhysicalResidual > 0f), Is.True, "Residus d'anneau a deux gabarits recalcules sur l'anneau final.");

            // Liee au rapport, jamais au modele : RoadModelVersion, hash source et lignee n'en dependent pas.
            foreach (var hash in new[] { run.Binding.PhysicalInputHash, run.Binding.SemanticInputHash, run.Binding.ClearanceHash })
            {
                Assert.That(hash, Does.Match("^[0-9a-f]{64}$"));
                Assert.That(run.ReportText, Does.Contain(hash));
                Assert.That(run.ModelText, Does.Not.Contain(hash));
            }

            Assert.That(run.ReportText, Does.Contain("a_e = 0 m"));
            Assert.That(run.ReportText, Does.Contain("| physique | "), "Colliders mesures publies.");
        }

        [Test]
        public void EachRelevantPhysicalChangeInvalidatesTheFingerprintAndNamesTheInput()
        {
            var run = Fresh();
            Assert.That(run.Succeeded, Is.True, string.Join("\n", run.Failures.ToArray()));
            var baseline = run.RoundaboutEvidence;
            WithMvpRun(scene =>
            {
                var sidewalks = SidewalkDeclarations.Read(scene, new List<string>());
                Func<JunctionClearanceResult> sweep = () => RoundaboutClearance.Sweep(scene, run.Import, run.Compiled, sidewalks);
                var wall = ColliderAt(scene, WitnessPath(run));
                var island = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshCollider>(true))
                    .First(c => c.name == RoundaboutClearance.IslandName);

                bool enabled = wall.enabled;
                try
                {
                    wall.enabled = !enabled;
                    AssertInvalidated(baseline, sweep(), wall);
                }
                finally
                {
                    wall.enabled = enabled;
                }

                bool trigger = wall.isTrigger;
                try
                {
                    wall.isTrigger = !trigger;
                    AssertInvalidated(baseline, sweep(), wall);
                }
                finally
                {
                    wall.isTrigger = trigger;
                }

                var original = island.sharedMesh;
                var edited = UnityEngine.Object.Instantiate(original);
                try
                {
                    var vertices = edited.vertices;
                    vertices[0] += Vector3.up * 0.01f;
                    edited.vertices = vertices;
                    island.sharedMesh = edited;
                    AssertInvalidated(baseline, sweep(), island);
                }
                finally
                {
                    island.sharedMesh = original;
                    UnityEngine.Object.DestroyImmediate(edited);
                }

                int aiLayer = AssetDatabaseLayerOfAiVehicle();
                bool ignored = Physics.GetIgnoreLayerCollision(aiLayer, wall.gameObject.layer);
                try
                {
                    Physics.IgnoreLayerCollision(aiLayer, wall.gameObject.layer, !ignored);
                    AssertInvalidated(baseline, sweep(), wall);
                }
                finally
                {
                    Physics.IgnoreLayerCollision(aiLayer, wall.gameObject.layer, ignored);
                }

                Physics.SyncTransforms();
                var restored = sweep();
                Assert.That(restored.PhysicalFingerprint, Is.EqualTo(baseline.PhysicalFingerprint), "Etat restaure : meme empreinte.");
                Assert.That(scene.isDirty, Is.False, "Le test a laisse MVP_Run modifiee en memoire.");
            });
        }

        [Test]
        public void AMovedColliderClosesGateANamingItAndItsNonPositiveResidualWhileTheModelStaysUnchanged()
        {
            var signed = Fresh();
            Assert.That(signed.Succeeded, Is.True, string.Join("\n", signed.Failures.ToArray()));
            string signoff = SignInMemory(signed);
            Assert.That(AuthoredRoadModel.EvaluateGateA(signed, signed.ReportText, signed.ModelText, signed.OverlayText, signoff), Is.Empty,
                "Temoin : le sign-off en memoire ouvre la Gate A sur l'etat signe.");

            var witness = signed.RoundaboutEvidence.Rows.OrderBy(r => r.Physical.Residual).First();
            AuthoredRun moved = null;
            WithMvpRun(scene =>
            {
                var wall = ColliderAt(scene, WitnessPath(signed));
                var position = wall.transform.position;
                try
                {
                    // Le mur temoin pose sur la trajectoire : residu negatif.
                    wall.transform.position = new Vector3(witness.Physical.Position.x, position.y, witness.Physical.Position.z);
                    Physics.SyncTransforms();
                    moved = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), Committed(AuthoredRoadModel.DecisionsPath));
                }
                finally
                {
                    wall.transform.position = position;
                    Physics.SyncTransforms();
                }

                Assert.That(scene.isDirty, Is.False, "Le test a laisse MVP_Run modifiee en memoire.");
            });

            Assert.That(moved.Succeeded, Is.True, "La preuve physique ferme la Gate A sans invalider le modele.");
            Assert.That(moved.Binding.RoadModelVersion, Is.EqualTo(signed.Binding.RoadModelVersion));
            Assert.That(moved.ModelText, Is.EqualTo(signed.ModelText));

            var reasons = AuthoredRoadModel.EvaluateGateA(moved, signed.ReportText, signed.ModelText, signed.OverlayText, signoff);
            string all = string.Join("\n", reasons.ToArray());
            Assert.That(reasons.Any(r => r.StartsWith("Preuve physique Gate A", StringComparison.Ordinal) && r.Contains("residu physique non positif") && r.Contains("Col_Wall")), Is.True, all);
            Assert.That(reasons.Any(r => r.Contains("Rapport Gate A perime : physical-input-hash") && r.Contains(witness.Physical.Obstacle.Substring(witness.Physical.Obstacle.IndexOf('/') + 1))), Is.True, all);
            Assert.That(reasons.Any(r => r.Contains("Rapport Gate A perime : clearance-hash") && r.Contains(witness.Junction)), Is.True, all);
            Assert.That(reasons.Any(r => r.Contains("Sign-off perime : physical-input-hash")), Is.True, all);
            Assert.That(reasons.Any(r => r.Contains("Sign-off perime : clearance-hash")), Is.True, all);
        }

        [Test]
        public void AMovedCornerObstacleClosesGateANamingTheJunctionAndTheObstacle()
        {
            var signed = Fresh();
            Assert.That(signed.Succeeded, Is.True, string.Join("\n",signed.Failures.ToArray()));
            string signoff = SignInMemory(signed);
            var witness = signed.JunctionEvidence.Rows.Where(r => !string.IsNullOrEmpty(r.Physical.Obstacle)).OrderBy(r => r.Physical.Residual).First();
            string obstacle = witness.Physical.Obstacle.Substring(witness.Physical.Obstacle.IndexOf('/') + 1);
            AuthoredRun moved = null;
            WithMvpRun(scene =>
            {
                var curb = ColliderAt(scene, obstacle);
                var position = curb.transform.position;
                try
                {
                    // Pose sur la trajectoire temoin et releve au-dessus de la borne du relief franchissable
                    // (0,158 m) : sans cela, hors trottoir et sur la chaussee, la bordure de 0,12 m deviendrait
                    // un relief roulable et cesserait d'etre un obstacle.
                    float lift = signed.JunctionEvidence.ReliefClearanceMeters;
                    curb.transform.position = new Vector3(witness.Physical.Position.x, position.y + lift, witness.Physical.Position.z);
                    Physics.SyncTransforms();
                    moved = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), Committed(AuthoredRoadModel.DecisionsPath));
                }
                finally
                {
                    curb.transform.position = position;
                    Physics.SyncTransforms();
                }

                Assert.That(scene.isDirty, Is.False, "Le test a laisse MVP_Run modifiee en memoire.");
            });

            Assert.That(moved.Succeeded, Is.True, "La preuve physique ferme la Gate A sans invalider le modele.");
            Assert.That(moved.Binding.RoadModelVersion, Is.EqualTo(signed.Binding.RoadModelVersion));
            Assert.That(moved.ModelText, Is.EqualTo(signed.ModelText));
            var reasons = AuthoredRoadModel.EvaluateGateA(moved, signed.ReportText, signed.ModelText, signed.OverlayText, signoff);
            string all = string.Join("\n",reasons.ToArray());
            Assert.That(reasons.Any(r => r.StartsWith("Preuve physique Gate A", StringComparison.Ordinal) && r.Contains(witness.Junction) && r.Contains("residu non positif")), Is.True, all);
            Assert.That(moved.JunctionEvidence.Rows.Any(r => r.Junction == witness.Junction && !(r.Physical.Residual > 0f) && r.Physical.Obstacle.EndsWith(obstacle, StringComparison.Ordinal)), Is.True,
                "L'obstacle temoin deplace porte le residu non positif.");
        }

        [Test]
        public void ANonPositiveTwoFootprintRingResidualHaltsGateAWithoutInvalidatingTheModel()
        {
            var signed = Fresh();
            Assert.That(signed.Succeeded, Is.True, string.Join("\n",signed.Failures.ToArray()));
            string signoff = SignInMemory(signed);
            var ring = signed.Roundabouts.First(r => r.NearestObstacle != null);
            AuthoredRun moved = null;
            WithMvpRun(scene =>
            {
                // Levier reel lu par RoundaboutClearance.Measure : l'obstacle le plus proche de l'anneau
                // (mur du portail tunnel) rapproche du centre sous le rayon pave, donc r_out diminue.
                var centre = V1SourceSet.Extract(scene).Modules.Single(m => m.Key == ring.Module.Key).Root.position;
                string[] parts = ring.NearestObstacle.Split('/');
                var wall = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Collider>(true))
                    .Single(c => c.name == parts[1] && c.transform.parent != null && c.transform.parent.parent != null && c.transform.parent.parent.name == parts[0]);
                var position = wall.transform.position;
                try
                {
                    var toward = new Vector3(centre.x - wall.bounds.center.x, 0f, centre.z - wall.bounds.center.z).normalized;
                    wall.transform.position = position + toward * (ring.NearestObstacleRadius - ring.IslandRadius - 4f);
                    Physics.SyncTransforms();
                    moved = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), Committed(AuthoredRoadModel.DecisionsPath));
                }
                finally
                {
                    wall.transform.position = position;
                    Physics.SyncTransforms();
                }

                Assert.That(scene.isDirty, Is.False, "Le test a laisse MVP_Run modifiee en memoire.");
            });

            Assert.That(moved.Succeeded, Is.True, string.Join("\n",moved.Failures.ToArray()));
            Assert.That(moved.Binding.RoadModelVersion, Is.EqualTo(signed.Binding.RoadModelVersion));
            Assert.That(moved.Roundabouts.Single(r => r.Module.Key == ring.Module.Key).PhysicalResidual, Is.LessThanOrEqualTo(0f));
            var reasons = AuthoredRoadModel.EvaluateGateA(moved, signed.ReportText, signed.ModelText, signed.OverlayText, signoff);
            string all = string.Join("\n",reasons.ToArray());
            Assert.That(reasons.Any(r => r.StartsWith("Preuve physique Gate A", StringComparison.Ordinal) && r.Contains(ring.Module.Label)
                && r.Contains("residu d'anneau a deux gabarits non positif") && r.Contains("HALT")), Is.True, all);
        }

        [Test]
        public void AColliderChangeOutOfReachOfTheNineJunctionsKeepsTheSignoffValid()
        {
            var signed = Fresh();
            Assert.That(signed.Succeeded, Is.True, string.Join("\n", signed.Failures.ToArray()));
            string signoff = SignInMemory(signed);
            var measured = new HashSet<string>(signed.JunctionEvidence.PhysicalInputs.Concat(signed.JunctionEvidence.SemanticInputs)
                .Concat(signed.RoundaboutEvidence.PhysicalInputs).Select(i => i.Substring(0, i.LastIndexOf(':'))), StringComparer.Ordinal);

            AuthoredRun far = null;
            WithMvpRun(scene =>
            {
                // Racines relues sur la scene ouverte : celles du pipeline partage peuvent appartenir a une instance fermee.
                var roots = V1SourceSet.Extract(scene).Modules.Where(m => m.Kind == V1ModuleKind.Crossroads || m.Kind == V1ModuleKind.TJunction || m.Kind == V1ModuleKind.Roundabout)
                    .Select(m => m.Root.position).ToArray();
                var collider = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true))
                    .Where(c => !measured.Contains(scene.name + "/" + PathOf(c.transform)) && c.enabled && c.gameObject.activeInHierarchy && !c.isTrigger)
                    .OrderByDescending(c => roots.Min(r => Vector2.Distance(new Vector2(r.x, r.z), new Vector2(c.bounds.center.x, c.bounds.center.z))))
                    .First();
                var position = collider.transform.position;
                try
                {
                    collider.transform.position = position + new Vector3(0.5f, 0f, 0f);
                    Physics.SyncTransforms();
                    far = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), Committed(AuthoredRoadModel.DecisionsPath));
                }
                finally
                {
                    collider.transform.position = position;
                    Physics.SyncTransforms();
                }

                Assert.That(scene.isDirty, Is.False, "Le test a laisse MVP_Run modifiee en memoire.");
            });

            Assert.That(far.Succeeded, Is.True, string.Join("\n", far.Failures.ToArray()));
            Assert.That(far.Binding.PhysicalInputHash, Is.EqualTo(signed.Binding.PhysicalInputHash));
            Assert.That(far.Binding.ClearanceHash, Is.EqualTo(signed.Binding.ClearanceHash));
            var reasons = AuthoredRoadModel.EvaluateGateA(far, signed.ReportText, signed.ModelText, signed.OverlayText, signoff);
            Assert.That(reasons, Is.Empty, string.Join("\n", reasons.ToArray()));
        }

        [Test]
        public void ThePhysicalEvidenceIsBitIdenticalAfterASceneReload()
        {
            // Regle de reproductibilite, volet rechargement de scene : empreintes et residus aller-retour
            // identiques au bit pres, mesure directe ET pipeline complet (bloc canonique des residus :
            // anneaux, obstacles, coutures ; empreintes liees). Le volet nouvelle session d'Editeur reste
            // a prouver au HALT (TheCommittedArtifactsEqualAFreshPipeline dans une nouvelle session).
            string before = null;
            AuthoredRun beforeRun = null;
            WithMvpRun(scene =>
            {
                before = EvidenceText(scene);
                beforeRun = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), Committed(AuthoredRoadModel.DecisionsPath));
            });
            ReloadMvpRun();
            string after = null;
            AuthoredRun afterRun = null;
            WithMvpRun(scene =>
            {
                after = EvidenceText(scene);
                afterRun = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), Committed(AuthoredRoadModel.DecisionsPath));
            });
            Assert.That(after, Is.EqualTo(before));
            Assert.That(beforeRun.Succeeded && afterRun.Succeeded, Is.True);
            Assert.That(afterRun.ClearanceText, Is.EqualTo(beforeRun.ClearanceText));
            Assert.That(afterRun.Binding.ClearanceHash, Is.EqualTo(beforeRun.Binding.ClearanceHash));
            Assert.That(afterRun.Binding.PhysicalInputHash, Is.EqualTo(beforeRun.Binding.PhysicalInputHash));
            Assert.That(afterRun.Binding.SemanticInputHash, Is.EqualTo(beforeRun.Binding.SemanticInputHash));
        }

        [Test]
        public void GateAIsOpenedOnlyByTheOwnersBoundSignoff()
        {
            var run = Fresh();
            var reasons = AuthoredRoadModel.EvaluateGateA(run, Committed(AuthoredRoadModel.ReportPath),
                Committed(AuthoredRoadModel.ModelPath), Committed(AuthoredRoadModel.OverlayPath),
                AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.SignoffPath)));
            var signoff = JsonUtility.FromJson<CommittedSignoff>(Committed(AuthoredRoadModel.SignoffPath));
            if (signoff.Format == AuthoredRoadModel.LegacySignoffFormat)
                Assert.That(reasons, Is.EqualTo(new[] { "Sign-off historique non reconfirme : format 2 face a la preuve kinematic-v1." }),
                    "Gate A reste fermee jusqu'a la re-signature du proprietaire.");
            else
                Assert.That(reasons, Is.Empty, string.Join("\n", reasons.ToArray()));
        }

        [Test]
        [Category("Core")]
        public void TheCommittedSignoffIsPresentAndBoundToTheCommittedArtifacts()
        {
            // Controle LEGER de la porte A, sans pipeline : presence du sign-off et liaison aux seuls
            // artefacts committes (empreintes de fichiers + versions de code). Il porte Core en plus
            // de la categorie de fixture, donc il tourne AUSSI dans le profil Fast : une signature
            // absente, illisible ou detachee des artefacts committes est vue en quelques millisecondes.
            // Ce qu'il ne couvre pas : les empreintes qui exigent la mesure de la scene (source V1,
            // physical-input-hash, semantic-input-hash, clearance-hash) -- elles restent couvertes par
            // GateAIsOpenedOnlyByTheOwnersBoundSignoff dans les profils Geometry et Full, que le profil
            // Auto declenche des qu'une entree geometrique est touchee. Aucun test n'ecrit ni ne signe :
            // la porte A ne s'ouvre que par la fenetre du proprietaire, jamais par le workflow.
            string signoffText = AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.SignoffPath));
            Assert.That(signoffText, Is.Not.Null.And.Not.Empty,
                "Sign-off Gate A absent (" + AuthoredRoadModel.SignoffPath + ") : la porte A reste fermee, et le workflow ne doit jamais le regenerer automatiquement.");

            var layout = JsonUtility.FromJson<CommittedSignoff>(signoffText);
            Assert.That(layout, Is.Not.Null, "Sign-off illisible (JSON).");
            if (layout.Format == AuthoredRoadModel.LegacySignoffFormat)
            {
                Assert.That(signoffText, Is.EqualTo(Committed(HistoricalSignedDirectory + "MVP_Run.road-signoff.json")),
                    "L'ancien sign-off reste verbatim jusqu'a la re-signature.");
                var historical = TrafficV2Lifecycle.Admit(Committed(HistoricalSignedDirectory + "MVP_Run.road-model.json"),
                    signoffText, Committed(HistoricalSignedDirectory + "migration-report-5-28-mvp-run.md"));
                Assert.That(historical.Code, Is.EqualTo(TrafficV2Code.Allowed), "La preuve historique reste liee a ses propres artefacts.");
                var current = TrafficV2Lifecycle.Admit(Committed(AuthoredRoadModel.ModelPath), signoffText,
                    Committed(AuthoredRoadModel.ReportPath));
                Assert.That(current.Code, Is.EqualTo(TrafficV2Code.GateAEvidenceStale), "L'ancien sign-off ne couvre pas le modele actuel.");
                return;
            }
            Assert.That(layout.Format, Is.EqualTo(AuthoredRoadModel.SignoffFormat),
                "Sign-off de format inconnu : a re-signer par le proprietaire depuis la fenetre de revue.");
            Assert.That(string.IsNullOrWhiteSpace(layout.Approver), Is.False, "Sign-off sans approbateur.");
            Assert.That(layout.ReviewedInstances, Is.Not.Null.And.Not.Empty, "Sign-off sans instances revues.");

            string overlay = Committed(AuthoredRoadModel.OverlayPath);
            int overlayInstances = overlay.Split('\n').Count(line => line.StartsWith("instance ", StringComparison.Ordinal));
            Assert.That(layout.ReviewedInstances.Length, Is.EqualTo(overlayInstances),
                "Sign-off perime : la liste des instances revues (" + layout.ReviewedInstances.Length + ") ne correspond pas a l'overlay committe (" + overlayInstances + ").");

            string model = Committed(AuthoredRoadModel.ModelPath);
            string decisions = Committed(AuthoredRoadModel.DecisionsPath);
            string lineage = Committed(MigrationReport.LineagePath);

            Assert.That(layout.OverlayHash, Is.EqualTo(V1SourceSet.Sha256Hex(overlay)), "Sign-off perime : overlay-hash (overlay committe modifie depuis la signature).");
            Assert.That(layout.ModelHash, Is.EqualTo(V1SourceSet.Sha256Hex(model)), "Sign-off perime : model-hash (modele committe modifie depuis la signature).");
            Assert.That(layout.DecisionsHash, Is.EqualTo(V1SourceSet.Sha256Hex(decisions)), "Sign-off perime : decisions-hash (decisions modifiees depuis la signature).");
            Assert.That(layout.LineageHash, Is.EqualTo(V1SourceSet.Sha256Hex(lineage)), "Sign-off perime : lineage-hash (lignee modifiee depuis la signature).");

            string modelVersion = Regex.Match(model, "\"ModelVersion\":\"([^\"]+)\"").Groups[1].Value;
            Assert.That(modelVersion, Is.Not.Empty, "Modele committe sans ModelVersion lisible.");
            Assert.That(layout.RoadModelVersion, Is.EqualTo(modelVersion), "Sign-off perime : road-model-version (modele recompile depuis la signature).");

            Assert.That(layout.ImporterVersion, Is.EqualTo(V1RoadModelImporter.ImporterVersion), "Sign-off perime : importer-version (l'importeur a change de version).");
            Assert.That(layout.CompilerSchemaVersion, Is.EqualTo(RoadModelCompiler.CompilerSchemaVersion), "Sign-off perime : compiler-schema-version (le compilateur a change de version).");
            Assert.That(layout.PipelineVersion, Is.EqualTo(AuthoredRoadModel.PipelineVersion), "Sign-off perime : pipeline-version (le pipeline a change de version).");
        }

        /// <summary>
        /// Miroir de lecture du format de sign-off (SignoffLayout est prive en production). Si le
        /// format evolue et retire un champ, le parseur rend une valeur vide et le test rougit avec
        /// le nom du champ -- jamais silencieusement.
        /// </summary>
        [Serializable]
        private sealed class CommittedSignoff
        {
            public int Format;
            public string Approver;
            public string ApproverEmail;
            public string SignedAtUtc;
            public string[] ReviewedInstances;
            public string OverlayHash;
            public string SourceHash;
            public string LineageHash;
            public string DecisionsHash;
            public int ImporterVersion;
            public int CompilerSchemaVersion;
            public int PipelineVersion;
            public string ModelHash;
            public string RoadModelVersion;
            public string PhysicalInputHash;
            public string SemanticInputHash;
            public string ClearanceHash;
            public float TrackingAllowanceMeters;
            public string PoseModel;
            public string EvidenceParametersHash;
            public string[] SignedRingSeams;
        }

        // ================================================================== outils

        private static string SignInMemory(AuthoredRun run)
        {
            return AuthoredRoadModel.RenderSignoff(run, "Testeur", "test@example.invalid", run.Overlay.Select(i => i.Key).ToList(), new DateTime(2026, 9, 28, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <summary>Obstacle temoin du plus petit residu de giratoire (chemin de scene, sans le nom de scene).</summary>
        private static string WitnessPath(AuthoredRun run)
        {
            string obstacle = run.RoundaboutEvidence.Rows.Where(r => !string.IsNullOrEmpty(r.Physical.Obstacle)).OrderBy(r => r.Physical.Residual).First().Physical.Obstacle;
            return obstacle.Substring(obstacle.IndexOf('/') + 1);
        }

        private static Collider ColliderAt(Scene scene, string path)
        {
            var collider = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Collider>(true)).Single(c => PathOf(c.transform) == path);
            return collider;
        }

        private static string PathOf(Transform transform)
        {
            string path = transform.name;
            for (var t = transform.parent; t != null; t = t.parent)
            {
                path = t.name + "/" + path;
            }

            return path;
        }

        /// <summary>L'empreinte change et la ligne d'entree du collider modifie differe : il est nomme.</summary>
        private static void AssertInvalidated(JunctionClearanceResult baseline, JunctionClearanceResult changed, Collider collider)
        {
            Assert.That(changed.PhysicalFingerprint, Is.Not.EqualTo(baseline.PhysicalFingerprint), collider.name);
            string prefix = "/" + PathOf(collider.transform) + ":";
            var before = baseline.PhysicalInputs.Where(i => i.Contains(prefix)).ToArray();
            var after = changed.PhysicalInputs.Where(i => i.Contains(prefix)).ToArray();
            Assert.That(before, Is.Not.Empty, collider.name + " : entree mesuree.");
            Assert.That(after, Is.Not.EqualTo(before), collider.name + " : ligne d'entree nommee et changee.");
        }

        private static int AssetDatabaseLayerOfAiVehicle()
        {
            var ai = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            Assert.That(ai, Is.Not.Null);
            return ai.layer;
        }

        /// <summary>Preuve des 9 carrefours en texte (empreintes et residus aller-retour), mesuree sur la scene donnee.</summary>
        private static string EvidenceText(Scene scene)
        {
            var migration = MigrationReport.Run(scene, Committed(MigrationReport.LineagePath));
            Assert.That(migration.Succeeded, Is.True, string.Join("\n", migration.Failures.ToArray()));
            var model = RoadModelCompiler.Compile(RoadModelDocument.Load(Committed(AuthoredRoadModel.ModelPath)));
            var failures = new List<string>();
            var sidewalks = SidewalkDeclarations.Read(scene, failures);
            Assert.That(failures, Is.Empty);
            var text = new System.Text.StringBuilder();
            foreach (var evidence in new[] { JunctionClearance.Measure(scene, migration.Import, model, sidewalks), RoundaboutClearance.Sweep(scene, migration.Import, model, sidewalks) })
            {
                Assert.That(evidence.Failures, Is.Empty);
                text.Append(evidence.PhysicalFingerprint).Append('|').Append(evidence.SemanticFingerprint).Append('\n');
                foreach (var row in evidence.Rows)
                {
                    text.Append(row.Junction).Append('|').Append(row.Movement).Append('|').Append(row.Surface).Append('|')
                        .Append(row.Physical.Residual.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
                        .Append(row.Semantic == null ? "-" : row.Semantic.Residual.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
                }
            }

            return text.ToString();
        }

        /// <summary>
        /// Recharge MVP_Run depuis le disque (jamais une scene modifiee). Ouverte dans l'Editeur : une
        /// scene vide additive la remplace le temps du rechargement, puis MVP_Run redevient active.
        /// Le pipeline partage tient des racines de module de l'ancienne instance : il est oublie.
        /// </summary>
        private static void ReloadMvpRun()
        {
            _committedRun = null;
            var open = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
            if (!open.IsValid() || !open.isLoaded)
            {
                return; // chaque WithMvpRun l'ouvre et la ferme deja : deux lectures du disque.
            }

            Assert.That(open.isDirty, Is.False, "MVP_Run modifiee : rechargement refuse.");
            bool active = SceneManager.GetActiveScene() == open;
            var placeholder = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(placeholder);
                Assert.That(EditorSceneManager.CloseScene(open, true), Is.True);
                var reloaded = EditorSceneManager.OpenScene(MigrationReport.ScenePath, OpenSceneMode.Additive);
                Assert.That(reloaded.isLoaded, Is.True);
                if (active)
                {
                    SceneManager.SetActiveScene(reloaded);
                }
            }
            finally
            {
                // Jamais laisser l'Editeur du proprietaire sans MVP_Run, meme si OpenScene a leve.
                var current = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
                if (!current.IsValid() || !current.isLoaded)
                {
                    current = EditorSceneManager.OpenScene(MigrationReport.ScenePath, OpenSceneMode.Additive);
                }

                if (active && current.isLoaded)
                {
                    SceneManager.SetActiveScene(current);
                }

                EditorSceneManager.CloseScene(placeholder, true);
            }
        }

        private static void AssertRefused(AuthoredRun run, string fragment)
        {
            Assert.That(run.Succeeded, Is.False);
            Assert.That(run.Failures.Any(f => f.Contains(fragment)), Is.True,
                "Motif attendu : " + fragment + "\nObtenu :\n" + string.Join("\n", run.Failures.ToArray()));

            // Fail-closed : rien n'est ecrit, meme vers des chemins neufs.
            string root = Path.Combine(Path.GetTempPath(), "rrs-528-" + Guid.NewGuid().ToString("N"));
            string model = Path.Combine(root, "model.json");
            string overlay = Path.Combine(root, "overlay.txt");
            string report = Path.Combine(root, "report.md");
            Assert.That(AuthoredRoadModel.TryWrite(run, model, overlay, report, out string error), Is.False);
            Assert.That(error, Does.Contain("rien n'est ecrit"));
            Assert.That(File.Exists(model) || File.Exists(overlay) || File.Exists(report), Is.False);
        }

        private static CompiledJunctionMovement Movement(CompiledRoadModel model, RoadId id)
        {
            Assert.That(model.TryGetMovement(id, out CompiledJunctionMovement movement), Is.True);
            return movement;
        }

        /// <summary>Insere le point milieu de chaque segment : la polyligne est inchangee.</summary>
        private static List<RoadCurveSample> Resample(IReadOnlyList<RoadCurveSample> samples, RoadCurve curve)
        {
            var dense = new List<RoadCurveSample>();
            for (int i = 0; i < samples.Count; i++)
            {
                if (i > 0)
                {
                    dense.Add(ToSample(curve.Sample(0.5f * (samples[i - 1].SMeters + samples[i].SMeters))));
                }

                dense.Add(samples[i]);
            }

            return dense;
        }

        private static RoadCurveSample ToSample(RoadCurvePoint point)
        {
            var sample = new RoadCurveSample();
            sample.SMeters = point.SMeters;
            sample.Position = point.Position;
            sample.Tangent = point.Tangent;
            sample.Up = point.Up;
            sample.CurvaturePerMeter = point.CurvaturePerMeter;
            sample.HalfWidthLeftMeters = point.HalfWidthLeftMeters;
            sample.HalfWidthRightMeters = point.HalfWidthRightMeters;
            return sample;
        }

        /// <summary>Deux mouvements de carrefours differents : enveloppes forcement disjointes.</summary>
        private static KeyValuePair<CompiledJunctionMovement, CompiledJunctionMovement> FarPair(CompiledRoadModel model)
        {
            var a = model.Movements[0];
            var b = model.Movements.First(m => m.JunctionId != a.JunctionId);
            return new KeyValuePair<CompiledJunctionMovement, CompiledJunctionMovement>(a, b);
        }

        private static AuthoringDecisions CommittedDecisions()
        {
            return AuthoringDecisions.Parse(Committed(AuthoredRoadModel.DecisionsPath));
        }

        private static string Committed(string projectRelative)
        {
            return File.ReadAllText(AuthoredRoadModel.FullPath(projectRelative));
        }

        private static AuthoredRun _committedRun;

        /// <summary>
        /// Le pipeline a decisions committees est une FONCTION PURE de la scene MVP_Run et des
        /// fichiers committes (extraction, import, compilation, balayage, empreintes) : le recalculer
        /// par test ne change pas le resultat, seulement le temps. Le resultat est donc partage au
        /// sein d'UNE execution de fixture, sous trois conditions verifiees : aucun test ne mute le
        /// run retourne (ils lisent et travaillent sur des copies), le resultat est demonte en fin
        /// de fixture ([OneTimeTearDown]), et les tests a decisions modifiees gardent chacun leur
        /// propre run via RunWith (le HALT de la 5.28 n'est jamais contourne : les memes tests
        /// echouent avec le meme message).
        /// </summary>
        private static AuthoredRun Fresh()
        {
            if (_committedRun == null)
            {
                _committedRun = RunWith(Committed(AuthoredRoadModel.DecisionsPath));
            }

            return _committedRun;
        }

        [OneTimeTearDown]
        public void ReleaseTheSharedPipeline()
        {
            // Une execution ulterieure du meme processus (filtre, relance) ne doit jamais relire un
            // resultat calcule avant un changement de fichiers : le partage ne vit que le temps
            // d'une execution de fixture.
            _committedRun = null;
        }

        private static AuthoredRun RunWith(string decisions)
        {
            AuthoredRun run = null;
            WithMvpRun(scene => run = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), decisions));
            return run;
        }

        /// <summary>Une largeur valide change les empreintes : regenerer les revisions avant la compilation 2.</summary>
        private static AuthoredRun RunWithAutomated(AuthoringDecisions decisions)
        {
            AuthoredRun preliminary = RunWith(decisions.Serialize());
            Assert.That(preliminary.CandidateModel, Is.Not.Null, string.Join("\n", preliminary.Failures.ToArray()));
            AutomatedPairDecisionPlan plan = AutomatedPairDecisionPolicy.CreatePlan(preliminary);
            return RunWith(plan.DecisionsText);
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
                    // Presente mais dechargee : seulement redechargee, jamais retiree (garde de production).
                    EditorSceneManager.CloseScene(scene, !inHierarchy);
                }
            }
        }
    }
}
