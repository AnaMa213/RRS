using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
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
    public sealed class Story528AuthoringAndGateATests
    {
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

            // Un rejet motive retire sa zone et rien d'autre ; un rejet sans motif est refuse au parse.
            var decisions = CommittedDecisions();
            var first = decisions.Conflicts[0];
            first.Decision = ConflictDecisionKind.Rejected;
            first.Reason = "Test : rejet motive.";
            decisions.Conflicts[0] = first;
            var rejected = RunWith(decisions.Serialize());
            Assert.That(rejected.Succeeded, Is.True, string.Join("\n", rejected.Failures.ToArray()));
            Assert.That(rejected.Compiled.ConflictZones.Count, Is.EqualTo(accepted - 1));
            Assert.That(rejected.Compiled.Version, Is.Not.EqualTo(run.Compiled.Version), "Une zone en moins change la version.");

            first.Reason = " ";
            decisions.Conflicts[0] = first;
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse(decisions.Serialize()));
        }

        [Test]
        public void CandidatesComeFromTheVersionedProfileAndAreInvariantToResamplingAndPairOrder()
        {
            var run = Fresh();
            var model = run.Compiled;
            float radius = AuthoredRoadModel.SweptRadius(model.ValidationProfile);
            Assert.That(radius, Is.EqualTo(model.ValidationProfile.MaxVehicleHalfWidthMeters + model.ValidationProfile.LateralClearanceMarginMeters));

            foreach (var candidate in run.Candidates)
            {
                Assert.That(Movement(model, candidate.MovementA).FromCorridorId, Is.Not.EqualTo(Movement(model, candidate.MovementB).FromCorridorId),
                    "Une paire de meme approche est du suivi, jamais un candidat.");
                Assert.That(candidate.MovementA.CompareTo(candidate.MovementB), Is.LessThan(0), "Paire ordonnee par RoadId.");
            }

            var pair = run.Candidates[0];
            var a = Movement(model, pair.MovementA).Curve;
            var b = Movement(model, pair.MovementB).Curve;

            Assert.That(AuthoredRoadModel.SweptOverlap(a, b, radius, out RoadBoundsBox forward), Is.True);
            Assert.That(AuthoredRoadModel.SweptOverlap(b, a, radius, out RoadBoundsBox backward), Is.True);
            Assert.That(backward.Center, Is.EqualTo(forward.Center));
            Assert.That(backward.Extents, Is.EqualTo(forward.Extents));
            Assert.That(forward.Center, Is.EqualTo(pair.Volume.Center));

            // Meme courbe, echantillonnage double : meme verdict, meme volume aux arrondis pres.
            var resampled = Resample(Movement(model, pair.MovementA).Samples, a);
            Assert.That(resampled.Count, Is.GreaterThan(Movement(model, pair.MovementA).Samples.Count));
            Assert.That(AuthoredRoadModel.SweptOverlap(new RoadCurve(resampled), b, radius, out RoadBoundsBox dense), Is.True);
            Assert.That((dense.Center - forward.Center).magnitude, Is.LessThan(1e-3f));
            Assert.That((dense.Extents - forward.Extents).magnitude, Is.LessThan(1e-3f));

            // Et une paire disjointe reste disjointe apres re-echantillonnage.
            var far = FarPair(model);
            Assert.That(AuthoredRoadModel.SweptOverlap(far.Key.Curve, far.Value.Curve, radius, out RoadBoundsBox none), Is.False);
            Assert.That(AuthoredRoadModel.SweptOverlap(new RoadCurve(Resample(far.Key.Samples, far.Key.Curve)), far.Value.Curve, radius, out none), Is.False);
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
            decisions.Conflicts.Add(new ConflictDecision
            {
                Id = RoadId.New(),
                MovementKeyA = sameApproach[0],
                MovementKeyB = sameApproach[1],
                Decision = ConflictDecisionKind.Accepted,
                Reason = "Test"
            });
            AssertRefused(RunWith(decisions.Serialize()), "Decision de conflit sans candidat");
        }

        [Test]
        public void ADivergentWidthIsAHardFailureIncludingAnAsymmetricOne()
        {
            var decisions = CommittedDecisions();
            var width = decisions.Widths[0];
            width.HalfWidthLeftMeters = 2.5f;
            width.HalfWidthRightMeters = 2.5f;
            decisions.Widths[0] = width;
            AssertRefused(RunWith(decisions.Serialize()), "Largeur divergente pour '" + width.SubjectKey + "'");

            // AD-45 asymetrique : gauche juste, droite divergente, toujours refuse.
            decisions = CommittedDecisions();
            width = decisions.Widths[0];
            width.HalfWidthRightMeters = width.HalfWidthLeftMeters - 0.5f;
            decisions.Widths[0] = width;
            AssertRefused(RunWith(decisions.Serialize()), "Largeur divergente pour '" + width.SubjectKey + "'");

            decisions = CommittedDecisions();
            decisions.Widths.RemoveAt(0);
            AssertRefused(RunWith(decisions.Serialize()), "Largeur non revue");
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
        public void ATamperedOrDivergentPersistedModelIsRefusedNeverRepaired()
        {
            string model = Committed(AuthoredRoadModel.ModelPath);
            Assert.That(RoadModelCompiler.Compile(RoadModelDocument.Load(model)).Version, Is.EqualTo(Fresh().Compiled.Version));

            // Un label n'entre pas dans la version, mais c'est un octet modifie : refuse par l'integrite.
            string label = model.Replace("MVP_Run (authoring 5.28)", "MVP_Run (authoring 5.2X)");
            Assert.That(label, Is.Not.EqualTo(model));
            Assert.That(Assert.Throws<FormatException>(() => RoadModelDocument.Load(label)).Message, Does.Contain("integrite"));

            string spaced = model.Replace("{\"Format\":1,", "{\"Format\": 1,");
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
            var run = Fresh();
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
            var instances = run.Overlay.Select(i => i.Key).ToList();
            string signoff = AuthoredRoadModel.RenderSignoff(run, "Testeur", "test@example.invalid", instances, new DateTime(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(AuthoredRoadModel.VerifySignoff(signoff, run), Is.Empty);
            Assert.That(instances.Count, Is.EqualTo(25));
            Assert.That(run.ModelText, Does.Not.Contain("Testeur"), "L'identite d'approbation n'entre dans aucun hash de modele.");

            foreach (var field in new[] { "OverlayHash", "SourceHash", "LineageHash", "DecisionsHash", "ModelHash" })
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
            var curve = run.Compiled.Movements[0].Curve;
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
            Assert.DoesNotThrow(() => AuthoringDecisions.Parse("{\"Format\":1,\"Controls\":[{\"Id\":\"" + a + "\",\"ApproachKey\":\"a\",\"Kind\":\"Uncontrolled\"}]," + Empty));

            // Liste hors d'ordre.
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse("{\"Format\":1,\"Controls\":["
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
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse("{\"Format\":1,\"Controls\":[{\"Id\":\"" + a + "\",\"ApproachKey\":\"a\",\"Kind\":\"7\"}]," + Empty));
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

        [Test]
        public void GateAIsOpenedOnlyByTheOwnersBoundSignoff()
        {
            // Rouge tant que le proprietaire n'a pas revu l'overlay et signe dans l'Editeur
            // (RoadRage/Traffic V2/Revue Gate A) : c'est le HALT de la Story 5.28, pas un defaut.
            var run = Fresh();
            var reasons = AuthoredRoadModel.EvaluateGateA(run, Committed(AuthoredRoadModel.ReportPath),
                Committed(AuthoredRoadModel.ModelPath), Committed(AuthoredRoadModel.OverlayPath),
                AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.SignoffPath)));
            Assert.That(reasons, Is.Empty, string.Join("\n", reasons.ToArray()));
        }

        // ================================================================== outils

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

        private static AuthoredRun Fresh()
        {
            return RunWith(Committed(AuthoredRoadModel.DecisionsPath));
        }

        private static AuthoredRun RunWith(string decisions)
        {
            AuthoredRun run = null;
            WithMvpRun(scene => run = AuthoredRoadModel.Run(scene, Committed(MigrationReport.LineagePath), decisions));
            return run;
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
