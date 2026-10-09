using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Recovery;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.40 -- detection d'interblocage depuis le graphe d'attente (G1-G2), journal (G3), escalade par paliers authores
    /// decidee par le coordinateur (G4-G7), sur un carrefour synthetique en memoire (patron Story 5.35, aucun artefact signe
    /// modifie), et gardes structurelles : aucun palier ne retire ni ne deplace un vehicule.
    /// </summary>
    [Category("Core")]
    [Category("Story540")]
    public sealed class Story540GridlockTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string SupervisorPath = "Assets/RoadRage/Features/Vehicles/Traffic/Recovery/GridlockSupervisor.cs";
        private const string CoordinatorPath = "Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs";
        private const string RunnerPath = "Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs";
        private const float Dt = 0.02f;
        private const float CarLength = 4.44f;
        private static readonly Vector3[] FarAway = { new Vector3(1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f),
            new Vector3(1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f) };

        private static DriverProfile Driver { get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; } }
        private static float Reservation { get { return CarLength + Driver.MinimumGap; } }
        private static RoadId Id(int n) { return new RoadId(0x540UL, (ulong)n); }

        // ============================================================ G1-G2 : graphe d'attente et cycles

        private static Dictionary<string, RoadId> Actors(params RoadId[] ids)
        {
            return ids.ToDictionary(x => x.ToString(), x => x);
        }

        private static Blocker LeaderOn(RoadId leader, ulong since = 1UL)
        {
            return BlockerRules.Leader(leader, 3f, 2f, -1f, since);
        }

        private static Blocker GrantCause(RoadId cause, ulong since = 1UL)
        {
            return BlockerRules.Junction(new JunctionBlockerCause(BlockerKind.JunctionGrant, cause.ToString()), "rule", -1f, since);
        }

        private static Blocker ExitBlocked(RoadId corridor, ulong since = 1UL)
        {
            return BlockerRules.Junction(new JunctionBlockerCause(BlockerKind.BlockedExit, corridor.ToString()), "rule", -1f, since);
        }

        [Test]
        public void AThreeVehicleCycleAcrossTwoJunctionsIsFoundFromLeaderGrantAndBlockedExitArcs()
        {
            var m = RoadModelCompiler.Compile(CrossingSource(0f, -90f, 180f, 90f));
            var index = JunctionConflictIndex.For(m);
            var traversal = Traversal(index, m.Movements.OrderBy(x => x.Id).First().Id);
            RoadId a = Id(1), b = Id(2), c = Id(3);
            var actors = Actors(a, b, c);
            // C attend a l'entree d'un second carrefour : sa sortie est bornee par l'occupant A.
            var exitReport = Requesting(m, c, traversal, 0.3f, 0f, 1f, a);
            var arcs = new List<GridlockArc>();
            GridlockSupervisor.WaitsOf(a, new[] { LeaderOn(b) }, null, actors, arcs);
            GridlockSupervisor.WaitsOf(b, new[] { GrantCause(c) }, null, actors, arcs);
            GridlockSupervisor.WaitsOf(c, new[] { ExitBlocked(traversal.ExitCorridorId) }, exitReport, actors, arcs);
            Assert.That(arcs.Select(x => x.ToText()), Is.EqualTo(new[] { a + " -Leader-> " + b, b + " -JunctionGrant-> " + c,
                c + " -BlockedExit-> " + a }));

            var cycles = GridlockSupervisor.FindCycles(arcs);
            Assert.That(cycles.Count, Is.EqualTo(1));
            Assert.That(cycles[0].Members, Is.EqualTo(new[] { a, b, c }));
            Assert.That(cycles[0].Arcs.Count, Is.EqualTo(3));

            // Determinisme : toute permutation des arcs donne le meme cycle.
            foreach (var order in Permutations(arcs))
                Assert.That(GridlockSupervisor.FindCycles(order).Select(x => x.ToText()), Is.EqualTo(cycles.Select(x => x.ToText())));
        }

        [Test]
        public void AnOpenChainIsNeverAGridlockHoweverLongItWaitsAndRulesOrAbsentActorsGiveNoArc()
        {
            RoadId a = Id(1), b = Id(2), c = Id(3), absent = Id(9);
            var actors = Actors(a, b, c);
            var arcs = new List<GridlockArc>();
            // Attente tres ancienne : le temps seul ne fait jamais un cycle (C roule, il n'a aucun blocker).
            GridlockSupervisor.WaitsOf(a, new[] { LeaderOn(b, 1UL) }, null, actors, arcs);
            GridlockSupervisor.WaitsOf(b, new[] { GrantCause(c, 1UL) }, null, actors, arcs);
            GridlockSupervisor.WaitsOf(c, new Blocker[0], null, actors, arcs);
            Assert.That(GridlockSupervisor.FindCycles(arcs), Is.Empty);

            var none = new List<GridlockArc>();
            GridlockSupervisor.WaitsOf(a, new[] { BlockerRules.PolicyImmobilization(-1f, 1UL), GrantCause(absent),
                ExitBlocked(Id(50)), LeaderOn(a) }, null, actors, none);
            Assert.That(none, Is.Empty, "regle, acteur absent, sortie sans occupant ou soi-meme : aucun arc");
        }

        [Test]
        public void TwoDisjointCyclesAreReportedSeparatelyAndSortedByKey()
        {
            var arcs = new List<GridlockArc>
            {
                new GridlockArc(Id(7), Id(8), "Leader"), new GridlockArc(Id(8), Id(7), "Leader"),
                new GridlockArc(Id(1), Id(2), "Leader"), new GridlockArc(Id(2), Id(3), "JunctionGrant"),
                new GridlockArc(Id(3), Id(1), "BlockedExit"), new GridlockArc(Id(3), Id(7), "Leader")
            };
            var cycles = GridlockSupervisor.FindCycles(arcs);
            Assert.That(cycles.Select(x => x.Members.ToArray()), Is.EqualTo(new[] { new[] { Id(1), Id(2), Id(3) }, new[] { Id(7), Id(8) } }));
            Assert.That(cycles[0].Arcs.Any(x => x.To == Id(7)), Is.False, "un arc sortant du cycle n'en fait pas partie");
        }

        [Test]
        public void ANewCycleIsLoggedOnceInDevelopmentBuildsWithItsVehiclesAndNotOnRepetition()
        {
            var arcs = new[] { new GridlockArc(Id(1), Id(2), "Leader"), new GridlockArc(Id(2), Id(3), "JunctionGrant"),
                new GridlockArc(Id(3), Id(1), "BlockedExit") };
            var lines = new List<string>();
            var supervisor = new GridlockSupervisor(lines.Add);
            supervisor.Detect(1UL, arcs);
            supervisor.Detect(2UL, arcs);
            Assert.That(lines.Count, Is.EqualTo(1), string.Join("\n", lines));
            foreach (var id in new[] { Id(1), Id(2), Id(3) }) Assert.That(lines[0], Does.Contain(id.ToString()));
            supervisor.Detect(3UL, new GridlockArc[0]);
            supervisor.Detect(4UL, arcs);
            Assert.That(lines.Count, Is.EqualTo(2), "un cycle disparu puis revenu est un nouvel interblocage");

            Assert.That(Debug.isDebugBuild, Is.True, "l'Editeur est un build de developpement");
            LogAssert.Expect(LogType.Log, new Regex(Regex.Escape(GridlockSupervisor.LogPrefix) + "frame 5 : interblocage detecte"));
            new GridlockSupervisor().Detect(5UL, arcs);
        }

        // ============================================================ G4-G7 : escalade decidee par le coordinateur

        [Test]
        public void TheAuthoredTiersAreOrderedDistinctEnabledAndNoneRemovesOrMovesAVehicle()
        {
            var tiers = TrafficV2Settings.GridlockEscalationTiers;
            Assert.That(tiers, Is.EqualTo(new[] { GridlockEscalationTier.PrecedenceRelaxation }));
            Assert.That(tiers.Distinct().Count(), Is.EqualTo(tiers.Count));
            foreach (var tier in tiers)
            {
                Assert.That(Enum.IsDefined(typeof(GridlockEscalationTier), tier) && tier != GridlockEscalationTier.None, Is.True);
            }
            var forbidden = new Regex("Remov|Teleport|Reinsert|Despawn|Delete|Destroy|Relocat|Move", RegexOptions.IgnoreCase);
            foreach (var name in Enum.GetNames(typeof(GridlockEscalationTier)))
                Assert.That(forbidden.IsMatch(name), Is.False, name);
        }

        [Test]
        public void ACycleHeldByAnOccupantIsEscalatedOnceToTheOnlyAdmissibleMemberWithoutConflictingGrants()
        {
            var setup = OccupiedCycle();
            var reports = setup.Reports;
            // Sans cycle soumis, le briseur 5.35 est bloque par l'occupant : aucun grant.
            var plain = Batch(setup.Model, new JunctionCoordinator(setup.Model), 1, null, reports);
            Assert.That(plain.Records.Any(Served), Is.False, plain.ToText());
            Assert.That(Decision(plain, Id(6)).Reason, Is.EqualTo(JunctionReason.Restored), "occupant protege (5.34)");
            Assert.That(plain.Counters.DeadlockBreaks, Is.Zero);

            foreach (var order in new[] { reports, reports.Reverse().ToArray(), reports.Skip(2).Concat(reports.Take(2)).ToArray() })
            {
                var snapshot = Batch(setup.Model, new JunctionCoordinator(setup.Model), 1, new[] { setup.Cycle }, order);
                var escalated = snapshot.Records.Where(r => r.Reason == JunctionReason.GrantedGridlockEscalation).ToList();
                Assert.That(escalated.Count, Is.EqualTo(1), snapshot.ToText());
                Assert.That(escalated[0].TrafficId, Is.EqualTo(Id(1)), snapshot.ToText());
                Assert.That(escalated[0].Status, Is.EqualTo(JunctionGrantStatus.Granted));
                Assert.That(snapshot.Records.Count(r => r.TrafficId == Id(1) && r.Status == JunctionGrantStatus.Denied), Is.Zero);
                Assert.That(snapshot.Records.Count(Served), Is.EqualTo(1), "au plus un grant par cycle et par lot");
                Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Escalated));
                Assert.That(snapshot.Gridlocks.Single().Tier, Is.EqualTo(GridlockEscalationTier.PrecedenceRelaxation));
                Assert.That(snapshot.Gridlocks.Single().Served, Is.EqualTo(Id(1)));
                // Grant non cyclique : la sortie du servi recoit tout le vehicule (garder-la-libre tenu).
                Assert.That(reports.Single(r => r.TrafficId == Id(1)).Request.Exit.FreeLengthMeters,
                    Is.GreaterThanOrEqualTo(reports.Single(r => r.TrafficId == Id(1)).Request.Exit.RequiredMeters));
            }

            // Le grant d'escalade vit comme un grant ordinaire au lot suivant.
            var coordinator = new JunctionCoordinator(setup.Model);
            Batch(setup.Model, coordinator, 1, new[] { setup.Cycle }, reports);
            var next = Batch(setup.Model, coordinator, 2, null, reports);
            Assert.That(next.Records.Single(Served).TrafficId, Is.EqualTo(Id(1)), next.ToText());
        }

        [Test]
        public void WithoutAnAdmissibleMemberTheCycleIsExhaustedLoggedOnceAndNobodyIsRemoved()
        {
            var setup = OccupiedCycle(exitFree: 1f);
            var lines = new List<string>();
            var supervisor = new GridlockSupervisor(lines.Add);
            var coordinator = new JunctionCoordinator(setup.Model);
            for (ulong frame = 1; frame <= 3; frame++)
            {
                var snapshot = Batch(setup.Model, coordinator, frame, new[] { setup.Cycle }, setup.Reports);
                Assert.That(snapshot.Records.Any(Served), Is.False, snapshot.ToText());
                Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Exhausted));
                Assert.That(snapshot.Counters.Actors, Is.EqualTo(setup.Reports.Length), "tous les acteurs restent presents");
                supervisor.Report(snapshot);
            }
            Assert.That(lines.Count, Is.EqualTo(1), string.Join("\n", lines));
            Assert.That(lines[0], Does.Contain("paliers epuises"));

            // Un cycle epuise qui disparait puis revient est rejournalise (detection et epuisement).
            var arcs = new[] { new GridlockArc(Id(1), Id(2), "JunctionGrant"), new GridlockArc(Id(2), Id(3), "JunctionGrant"),
                new GridlockArc(Id(3), Id(4), "JunctionGrant"), new GridlockArc(Id(4), Id(6), "JunctionGrant"),
                new GridlockArc(Id(6), Id(1), "Leader") };
            lines.Clear();
            Assert.That(supervisor.Detect(4UL, arcs).Single().Key, Is.EqualTo(setup.Cycle.Key));
            supervisor.Report(Batch(setup.Model, coordinator, 4, new[] { setup.Cycle }, setup.Reports));
            Assert.That(lines.Count(x => x.Contains("paliers epuises")), Is.Zero, "deja journalise tant que le cycle reste detecte");
            supervisor.Detect(5UL, new GridlockArc[0]);
            Assert.That(supervisor.Cycles, Is.Empty);
            supervisor.Detect(6UL, arcs);
            supervisor.Report(Batch(setup.Model, coordinator, 6, new[] { setup.Cycle }, setup.Reports));
            Assert.That(lines.Count(x => x.Contains("interblocage detecte")), Is.EqualTo(2), string.Join("\n", lines));
            Assert.That(lines.Count(x => x.Contains("paliers epuises")), Is.EqualTo(1), string.Join("\n", lines));
        }

        [Test]
        public void AMissingSignalIsNeverRelaxedByTheEscalation()
        {
            var source = CrossingSource(0f, -90f, 180f, 90f);
            for (int i = 0; i < source.Controls.Length; i++) source.Controls[i].Kind = JunctionControlKind.Signalized;
            source.SignalPlans = new[] { new SignalPlan { Id = Id(1600), JunctionId = Id(1001),
                Groups = source.Movements.Select((x, g) => new SignalGroup { GroupId = Id(1610 + g), MemberMovementIds = new[] { x.Id } }).ToArray(),
                Phases = new[] { new SignalPhase { PhaseId = Id(1620), DurationSeconds = 2f,
                    GroupStates = source.Movements.Select((x, g) => new SignalGroupState { GroupId = Id(1610 + g), State = SignalState.Red }).ToArray() } } } };
            var m = RoadModelCompiler.Compile(source);
            var index = JunctionConflictIndex.For(m);
            var moves = m.Movements.OrderBy(x => x.Id).Select(x => Traversal(index, x.Id)).ToArray();
            var reports = Enumerable.Range(0, 4).Select(i => Requesting(m, Id(i + 1), moves[i], 0.3f, 0f)).ToArray();
            var cycle = new GridlockCycle(reports.Select(r => r.TrafficId), null);
            // Sans frame, aucun etat de feu : SignalUnavailable, jamais un vert, et l'escalade ne le releve pas.
            var snapshot = Batch(m, new JunctionCoordinator(m), 1, new[] { cycle }, reports);
            Assert.That(snapshot.Records.Where(r => r.TrafficId == Id(1)).Select(r => r.Reason),
                Is.EqualTo(new[] { JunctionReason.SignalUnavailable }), snapshot.ToText());
            Assert.That(snapshot.Records.Any(r => r.IsEffectiveGrant), Is.False, snapshot.ToText());
            Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Exhausted));
        }

        [TestCase(SignalState.Red)]
        [TestCase(SignalState.Yellow)]
        [TestCase(SignalState.Green)]
        public void AnActualSignalStateIsRespectedWhenACycleIsSubmitted(SignalState state)
        {
            var source = CrossingSource(0f, -90f, 180f, 90f);
            for (int i = 0; i < source.Controls.Length; i++) source.Controls[i].Kind = JunctionControlKind.Signalized;
            source.SignalPlans = new[] { new SignalPlan { Id = Id(1600), JunctionId = Id(1001),
                Groups = source.Movements.Select((x, g) => new SignalGroup { GroupId = Id(1610 + g), MemberMovementIds = new[] { x.Id } }).ToArray(),
                Phases = new[] { new SignalPhase { PhaseId = Id(1620), DurationSeconds = 2f,
                    GroupStates = source.Movements.Select((x, g) => new SignalGroupState { GroupId = Id(1610 + g),
                        State = state == SignalState.Green && g != 0 ? SignalState.Red : state }).ToArray() } } } };
            var m = RoadModelCompiler.Compile(source);
            var index = JunctionConflictIndex.For(m);
            var moves = m.Movements.OrderBy(x => x.Id).Select(x => Traversal(index, x.Id)).ToArray();
            var reports = Enumerable.Range(0, 4).Select(i => Requesting(m, Id(i + 1), moves[i], 0.3f, 0f)).ToArray();
            var cycle = new GridlockCycle(reports.Select(r => r.TrafficId), null);
            var frame = new TrafficFrame(1, m, new TrafficActorInput[0], null, new[] { new SignalPhaseInput(Id(1600), Id(1620)) });
            SignalState actual;
            Assert.That(frame.TryGetSignalState(moves[0].FirstMovementId, out actual), Is.True);
            Assert.That(actual, Is.EqualTo(state), "la phase est presente dans la frame du lot");
            var snapshot = new JunctionCoordinator(m).Resolve(1, reports, frame, new[] { cycle });
            AssertGrantInvariant(m, snapshot);
            Assert.That(snapshot.Records.Any(r => r.Reason == JunctionReason.GrantedGridlockEscalation), Is.False, snapshot.ToText());
            if (state == SignalState.Green)
            {
                Assert.That(snapshot.Records.Count(r => r.IsEffectiveGrant), Is.EqualTo(1), snapshot.ToText());
                Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Progressed));
            }
            else
            {
                Assert.That(snapshot.Records.All(r => r.Status == JunctionGrantStatus.Denied && r.Reason == JunctionReason.SignalStop),
                    Is.True, snapshot.ToText());
                Assert.That(snapshot.Records.Any(r => r.IsEffectiveGrant), Is.False, snapshot.ToText());
                Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Exhausted));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EscalationNeverUsesAMergeGapAgainstALiveGrant(bool holderAdmittedByGap)
        {
            // Geometrie Merge authoree, controles Stop seulement dans ce modele en memoire : aucune preseance sur l'ancien.
            var source = RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath));
            var junction = source.Junctions.Single(x => x.Label == "Roundabout_SouthWest").Id;
            Func<string, RoadId> movement = label =>
            {
                var matches = source.Movements.Where(x => x.JunctionId == junction && x.Label.Contains(label)).ToArray();
                Assert.That(matches.Length, Is.EqualTo(1), "mouvement authore : " + label);
                return matches[0].Id;
            };
            var entryId = movement("Connector_West_In ->");
            var ringId = movement("Ring_Split_West -> Ring_Merge_West");
            for (int i = 0; i < source.Controls.Length; i++)
                if (source.Controls[i].ControlledMovementIds.Contains(entryId) || source.Controls[i].ControlledMovementIds.Contains(ringId))
                    source.Controls[i].Kind = JunctionControlKind.Stop;
            var m = RoadModelCompiler.Compile(source);
            var index = JunctionConflictIndex.For(m);
            var entry = Traversal(index, entryId);
            var ring = Traversal(index, ringId);
            RoadId zone;
            ConflictKind kind;
            float sa, sb;
            Assert.That(index.TryGetConflict(entryId, ringId, out zone, out kind, out sa, out sb), Is.True);
            Assert.That(kind, Is.EqualTo(ConflictKind.Merge));
            Assert.That(index.HasPrecedence(ringId, entryId), Is.False);
            var senior = Requesting(m, Id(1), entry, 0.3f, 3f); // Stop non marque, reserve contre le membre plus jeune.
            var marking = Requesting(m, Id(2), ring, 0.24f, 0f, exitFree: 0f);
            var coordinator = new JunctionCoordinator(m);
            Batch(m, coordinator, 1, null, senior, marking,
                Requesting(m, Id(7), entry, 0.24f, 0f, holderAdmittedByGap ? 0f : 100f));
            var second = new List<JunctionActorReport> { senior, marking, Requesting(m, Id(7), entry, 2f, 8f) };
            if (holderAdmittedByGap)
            {
                var far = Traversal(index, movement("Connector_South_In ->"), movement("Ring_Split_Diagonal -> Ring_Merge_Diagonal"),
                    ringId, movement("Ring_Split_South -> Connector_South_Out"));
                second.Add(Inside(m, Id(8), far, 0, 0f, 0f));
            }
            var seeded = Batch(m, coordinator, 2, null, second.ToArray());
            Assert.That(seeded.Records.Count(r => r.TrafficId == Id(7) && r.IsEffectiveGrant), Is.EqualTo(1), seeded.ToText());
            var holderGrant = seeded.Records.Single(r => r.TrafficId == Id(7) && r.IsEffectiveGrant);
            Assert.That(holderGrant.MergeGap, Is.EqualTo(holderAdmittedByGap), seeded.ToText());
            if (holderAdmittedByGap) Assert.That(holderGrant.Reason, Is.EqualTo(JunctionReason.GrantedMergeGap));

            // Titulaire non engage, assez loin pour un creneau ordinaire. Les deux membres attendent seulement la reservation
            // du Stop non marque : l'escalade doit ignorer ce membre, mais jamais le grant Merge exterieur, meme MergeGap.
            const float holderSpeed = 40f;
            float distance = 0.9f * JunctionDistances.For(Driver, holderSpeed, Dt,
                TrafficV2Settings.JunctionStopControlMarginMeters).RequestThresholdMeters;
            var holder = Requesting(m, Id(7), entry, distance, holderSpeed);
            var requester = Requesting(m, Id(2), ring, 0.3f, 3f);
            var cycle = new GridlockCycle(new[] { Id(1), Id(2) }, null);
            var plain = Batch(m, coordinator, 3, null, senior, requester, holder);
            var denied = Decision(plain, Id(2));
            Assert.That(denied.Reason, Is.EqualTo(JunctionReason.SeniorRequestPending), plain.ToText());
            Assert.That(denied.EtaSeconds, Is.GreaterThanOrEqualTo(denied.GapSeconds), "le creneau ordinaire est prouve");
            Assert.That(plain.Counters.MergeGapRefusals, Is.Zero, plain.ToText());
            var blocked = Batch(m, coordinator, 4, new[] { cycle }, senior, requester, holder);
            Assert.That(blocked.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Exhausted), blocked.ToText());
            Assert.That(blocked.Records.Any(r => r.Reason == JunctionReason.GrantedGridlockEscalation), Is.False, blocked.ToText());
            Assert.That(blocked.Records.Single(r => r.IsEffectiveGrant).TrafficId, Is.EqualTo(Id(7)), blocked.ToText());

            // Controle positif : quand le titulaire disparait, le seul refus restant est relaxable.
            var freed = Batch(m, coordinator, 5, new[] { cycle }, senior, requester);
            Assert.That(freed.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Escalated), freed.ToText());
            Assert.That(freed.Gridlocks.Single().Served, Is.EqualTo(Id(2)));
        }

        [Test]
        public void ALiveNonMemberGrantAloneForbidsTheEscalation()
        {
            // Titulaire sur une voie parallele a celle de A : aucune preseance sur A, seul son grant peut l'exclure.
            var m = RoadModelCompiler.Compile(CrossingSource(0f, -90f, 180f, 90f, 0f));
            var index = JunctionConflictIndex.For(m);
            var moves = m.Movements.OrderBy(x => x.Id).Select(x => Traversal(index, x.Id)).ToArray();
            var holder = Requesting(m, Id(7), moves[4], 0.3f, 0f);
            var coordinator = new JunctionCoordinator(m);
            Assert.That(Batch(m, coordinator, 1, null, holder).Records.Single(r => r.IsEffectiveGrant).TrafficId, Is.EqualTo(Id(7)));
            // Le titulaire non membre garde son grant non engage ; aucun occupant n'existe.
            var members = Enumerable.Range(0, 4).Select(i => Requesting(m, Id(i + 1), moves[i], 0.3f, 0f)).ToArray();
            var cycle = new GridlockCycle(members.Select(r => r.TrafficId), null);
            var snapshot = Batch(m, coordinator, 2, new[] { cycle }, members.Concat(new[] { holder }).ToArray());
            Assert.That(snapshot.Counters.Occupants, Is.Zero);
            Assert.That(snapshot.Records.Where(r => r.IsEffectiveGrant).Select(r => r.TrafficId), Is.EqualTo(new[] { Id(7) }), snapshot.ToText());
            Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Exhausted));
        }

        [Test]
        public void AnOccupantWithoutAnyGrantAloneForbidsTheEscalation()
        {
            var m = RoadModelCompiler.Compile(CrossingSource(0f, -90f, 180f, 90f, -90f));
            var index = JunctionConflictIndex.For(m);
            var moves = m.Movements.OrderBy(x => x.Id).Select(x => Traversal(index, x.Id)).ToArray();
            var reports = Enumerable.Range(0, 4).Select(i => Requesting(m, Id(i + 1), moves[i], 0.3f, 0f)).ToList();
            // Occupant sans traversee connue : protege comme occupant, mais aucun grant reconstruit.
            reports.Add(Inside(m, Id(8), moves[4], 0, 2f, 0f, false));
            var cycle = new GridlockCycle(reports.Take(4).Select(r => r.TrafficId), null);
            var snapshot = Batch(m, new JunctionCoordinator(m), 1, new[] { cycle }, reports.ToArray());
            Assert.That(snapshot.Counters.Occupants, Is.EqualTo(1));
            Assert.That(snapshot.Records.Any(r => r.IsEffectiveGrant), Is.False, snapshot.ToText());
            Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Exhausted));
        }

        [Test]
        public void APriorityThreatOutsideTheCycleIsStillRespected()
        {
            var setup = OccupiedCycle(outsider: true);
            var outsider = setup.Reports.Single(r => r.TrafficId == Id(5));
            Assert.That(outsider.Rejection, Is.EqualTo(JunctionRequestRejection.TooFar));
            var snapshot = Batch(setup.Model, new JunctionCoordinator(setup.Model), 1, new[] { setup.Cycle }, setup.Reports);
            Assert.That(snapshot.Records.Any(Served), Is.False, snapshot.ToText());
            Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Exhausted));
        }

        [Test]
        public void AnUnmarkedStopIsNeverRelaxedByTheEscalation()
        {
            var setup = OccupiedCycle(stop: true);
            var snapshot = Batch(setup.Model, new JunctionCoordinator(setup.Model), 1, new[] { setup.Cycle }, setup.Reports);
            JunctionRecord record;
            Assert.That(snapshot.TryGetDecision(Id(1), setup.Traversals[0].FirstMovementId, out record), Is.True);
            Assert.That(record.Reason, Is.EqualTo(JunctionReason.StopRequired), snapshot.ToText());
            Assert.That(snapshot.Records.Any(Served), Is.False, snapshot.ToText());
            Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Exhausted));
        }

        [Test]
        public void ACycleWhoseMemberProgressesNormallyIsNotEscalatedAndNullOrEmptyCyclesChangeNothing()
        {
            var m = RoadModelCompiler.Compile(CrossingSource(0f, -90f, 180f, 90f));
            var index = JunctionConflictIndex.For(m);
            var moves = m.Movements.OrderBy(x => x.Id).Select(x => Traversal(index, x.Id)).ToArray();
            var reports = Enumerable.Range(0, 4).Select(i => Requesting(m, Id(i + 1), moves[i], 0.3f, 0f)).ToArray();
            var cycle = new GridlockCycle(reports.Select(r => r.TrafficId), null);
            // Le briseur 5.35 sert un membre : le cycle progresse, aucune escalade en plus.
            var snapshot = Batch(m, new JunctionCoordinator(m), 1, new[] { cycle }, reports);
            Assert.That(snapshot.Counters.DeadlockBreaks, Is.EqualTo(1), snapshot.ToText());
            Assert.That(snapshot.Records.Any(r => r.Reason == JunctionReason.GrantedGridlockEscalation), Is.False);
            Assert.That(snapshot.Gridlocks.Single().Outcome, Is.EqualTo(GridlockOutcome.Progressed));

            string without = new JunctionCoordinator(m).Resolve(1, reports).ToText();
            Assert.That(new JunctionCoordinator(m).Resolve(1, reports, null, null).ToText(), Is.EqualTo(without));
            Assert.That(new JunctionCoordinator(m).Resolve(1, reports, null, new GridlockCycle[0]).ToText(), Is.EqualTo(without));
            Assert.That(new JunctionCoordinator(m).Resolve(1, reports).Gridlocks, Is.Empty);
        }

        // ============================================================ structure

        [Test]
        public void NeitherTheSupervisorNorTheEscalationRemovesMovesComposesOrDecidesOutsideTheCoordinator()
        {
            var body = new Regex(@"(\b(MovePosition|MoveRotation|AddForce|AddTorque|Teleport\w*|ApplyDriveIntent|ApplyMovement|Rigidbody|"
                + @"Despawn|Destroy|Reinsert\w*|VehicleDriveIntent\w*|Compose|Time\.(time|deltaTime))\b|\.(position|rotation|velocity|"
                + @"linearVelocity|angularVelocity)\s*=)");
            string supervisor = File.ReadAllText(SupervisorPath);
            Assert.That(body.Match(supervisor).Success, Is.False, body.Match(supervisor).Value);
            Assert.That(new Regex(@"\b(JunctionCoordinator|NewGrant|Resolve|Grant\w*|TrafficRule\w*)\b").IsMatch(supervisor), Is.False,
                "le superviseur soumet, il ne decide pas");

            string coordinator = File.ReadAllText(CoordinatorPath);
            int start = coordinator.IndexOf("private List<GridlockResolution> EscalateGridlocks(", StringComparison.Ordinal);
            int end = coordinator.IndexOf("/// <summary>t_gap du demandeur", start, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThan(0));
            Assert.That(end, Is.GreaterThan(start));
            string escalation = coordinator.Substring(start, end - start);
            Assert.That(body.Match(escalation).Success, Is.False, body.Match(escalation).Value);
            Assert.That(Regex.Matches(escalation, @"\bNewGrant\(").Count, Is.EqualTo(1), "un seul point d'emission");
            Assert.That(escalation, Does.Contain("break;"), "au plus un servi par cycle");
            Assert.That(escalation, Does.Not.Contain("ResolveUnavailableFrame"));
        }

        [Test]
        public void TheRunnerDetectsAfterDrivingThenCoordinatesWithTheCyclesThenReports()
        {
            string source = File.ReadAllText(RunnerPath);
            string[] order = { "prepared[i].Step(", "DetectGridlocks(frame)", "Coordinate(frame, snapshot, ref cost, cycles)",
                "gridlock.Report(coordinator.Current)", "GridlockSupervisor.WaitsOf(", "gridlock.Detect(FrameId, waits)",
                "coordinator.Resolve(FrameId, reports, frame, cycles)" };
            int last = -1;
            foreach (var call in order)
            {
                int at = source.IndexOf(call, last + 1, StringComparison.Ordinal);
                Assert.That(at, Is.GreaterThan(last), call);
                last = at;
            }
            Assert.That(source, Does.Contain("gridlock.Detect(FrameId, null)"), "un pas sans acteur vide la detection");
        }

        // ============================================================ fixtures

        private sealed class Setup
        {
            public CompiledRoadModel Model;
            public JunctionTraversal[] Traversals;
            public JunctionActorReport[] Reports;
            public GridlockCycle Cycle;
        }

        /// <summary>
        /// Croisement a priorite a droite : A (Id 1), B, C, D a l'arret a la ligne, chacun cedant a sa droite ; O (Id 6) occupe le
        /// mouvement de A, incompatible avec B, C, D mais pas avec A. Le cycle soumis est {A, B, C, D, O}. Option : sortie de A
        /// insuffisante, menace exterieure (Id 5, voie parallele a B, en approche), ou carrefour tout Stop avec A non arrete.
        /// </summary>
        private static Setup OccupiedCycle(float exitFree = 100f, bool outsider = false, bool stop = false)
        {
            var source = outsider ? CrossingSource(0f, -90f, 180f, 90f, -90f) : CrossingSource(0f, -90f, 180f, 90f);
            if (stop) for (int i = 0; i < source.Controls.Length; i++) source.Controls[i].Kind = JunctionControlKind.Stop;
            var m = RoadModelCompiler.Compile(source);
            var index = JunctionConflictIndex.For(m);
            var moves = m.Movements.OrderBy(x => x.Id).Select(x => Traversal(index, x.Id)).ToArray();
            var reports = new List<JunctionActorReport>
            {
                Requesting(m, Id(1), moves[0], 0.3f, stop ? 3f : 0f, exitFree),
                Requesting(m, Id(2), moves[1], 0.3f, 0f),
                Requesting(m, Id(3), moves[2], 0.3f, 0f),
                Requesting(m, Id(4), moves[3], 0.3f, 0f),
                Inside(m, Id(6), moves[0], 0, 2f, 0f)
            };
            if (outsider) reports.Add(Requesting(m, Id(5), moves[4], JustTooFarAt(8f), 8f));
            return new Setup { Model = m, Traversals = moves, Reports = reports.ToArray(),
                Cycle = new GridlockCycle(new[] { Id(1), Id(2), Id(3), Id(4), Id(6) }, null) };
        }

        /// <summary>Grant effectif d'un autre vehicule que l'occupant O, dont la protection Restored (5.34) est attendue.</summary>
        private static bool Served(JunctionRecord record)
        {
            return record.IsEffectiveGrant && record.TrafficId != Id(6);
        }

        private static JunctionRecord Decision(JunctionSnapshot snapshot, RoadId id)
        {
            var records = snapshot.Records.Where(r => r.TrafficId == id).ToList();
            Assert.That(records.Count, Is.EqualTo(1), snapshot.ToText());
            return records[0];
        }

        private static float JustTooFarAt(float speed)
        {
            return JunctionDistances.For(Driver, speed, Dt, TrafficV2Settings.JunctionStopControlMarginMeters).RequestThresholdMeters + 0.1f;
        }

        private static JunctionSnapshot Batch(CompiledRoadModel m, JunctionCoordinator coordinator, ulong frame,
            IReadOnlyList<GridlockCycle> cycles, params JunctionActorReport[] reports)
        {
            var snapshot = coordinator.Resolve(frame, reports, null, cycles);
            AssertGrantInvariant(m, snapshot);
            return snapshot;
        }

        /// <summary>Exception MergeGap ordinaire de 5.35 ; un grant d'escalade ne profite jamais de cette exception (G6).</summary>
        private static void AssertGrantInvariant(CompiledRoadModel m, JunctionSnapshot snapshot)
        {
            var index = JunctionConflictIndex.For(m);
            var effective = snapshot.Records.Where(r => r.IsEffectiveGrant).ToList();
            foreach (var a in effective)
                foreach (var b in effective)
                {
                    if (a.TrafficId.CompareTo(b.TrafficId) >= 0) continue;
                    foreach (var ma in a.MovementIds)
                        foreach (var mb in b.MovementIds)
                        {
                            RoadId zone;
                            ConflictKind kind;
                            float sa, sb;
                            if (!index.TryGetConflict(ma, mb, out zone, out kind, out sa, out sb)) continue;
                            Assert.That(kind == ConflictKind.Merge && (a.MergeGap || b.MergeGap)
                                && a.Reason != JunctionReason.GrantedGridlockEscalation && b.Reason != JunctionReason.GrantedGridlockEscalation, Is.True,
                                "grants incompatibles effectifs hors creneau de fusion : " + a.ToText() + " / " + b.ToText());
                        }
                }
        }

        private static IEnumerable<List<GridlockArc>> Permutations(List<GridlockArc> arcs)
        {
            yield return arcs;
            yield return Enumerable.Reverse(arcs).ToList();
            yield return arcs.Skip(1).Concat(arcs.Take(1)).ToList();
        }

        private static JunctionTraversal Traversal(JunctionConflictIndex index, params RoadId[] movements)
        {
            return new JunctionTraversal(index.JunctionOf(movements[0]), movements, index.ToCorridorOf(movements[movements.Length - 1]));
        }

        private static JunctionKinematics Kinematics(float v)
        {
            var driver = Driver;
            return new JunctionKinematics(v, driver.MaxAcceleration, driver.DesiredSpeed, driver.ComfortableDeceleration, CarLength);
        }

        private static float[] Starts(CompiledRoadModel m, JunctionConflictIndex index, JunctionTraversal traversal, float firstStart)
        {
            var starts = new float[traversal.MovementIds.Count];
            starts[0] = firstStart;
            for (int i = 1; i < starts.Length; i++)
            {
                RoadId previous = traversal.MovementIds[i - 1];
                EffectiveLaneCorridor between;
                float length = m.TryGetCorridor(index.ToCorridorOf(previous), out between) ? between.LengthMeters : 0f;
                starts[i] = starts[i - 1] + index.LengthOf(previous) + length;
            }
            return starts;
        }

        /// <summary>Demandeur a d de la frontiere, vitesse v ; sortie bornee par l'occupant <paramref name="exitOccupant"/>.</summary>
        private static JunctionActorReport Requesting(CompiledRoadModel m, RoadId id, JunctionTraversal traversal, float d, float v,
            float exitFree = 100f, RoadId exitOccupant = default(RoadId))
        {
            var index = JunctionConflictIndex.For(m);
            var distances = JunctionDistances.For(Driver, v, Dt, TrafficV2Settings.JunctionStopControlMarginMeters);
            float b = index.BoundaryOf(traversal.FirstMovementId);
            var approach = new JunctionApproach(traversal, d, distances, true, RoadId.None, false, false,
                new JunctionExitAssessment(exitFree, JunctionExitBound.Occupant, exitOccupant, Reservation), Starts(m, index, traversal, d - b), b);
            bool valid = d <= distances.RequestThresholdMeters;
            var positions = traversal.MovementIds.Select(x => new JunctionMovementPosition(x, JunctionMovementStatus.Ahead)).ToArray();
            return new JunctionActorReport(id, true, index.FromCorridorOf(traversal.FirstMovementId), FarAway, null, null, positions,
                new[] { approach }, true, valid, valid ? JunctionRequestRejection.None : JunctionRequestRejection.TooFar, Reservation,
                Kinematics(v));
        }

        /// <summary>Vehicule engage, mouvement de rang <paramref name="occupied"/> occupe, pare-chocs a sInMovement m dedans.</summary>
        private static JunctionActorReport Inside(CompiledRoadModel m, RoadId id, JunctionTraversal traversal, int occupied, float sInMovement,
            float v, bool withTraversal = true)
        {
            var index = JunctionConflictIndex.For(m);
            var movements = traversal.MovementIds;
            var positions = movements.Select((x, i) => new JunctionMovementPosition(x, i < occupied ? JunctionMovementStatus.Behind
                : i == occupied ? JunctionMovementStatus.Occupied : JunctionMovementStatus.Ahead)).ToArray();
            var remaining = new JunctionTraversal(traversal.JunctionId, movements.Skip(occupied).ToArray(), traversal.ExitCorridorId);
            var distances = JunctionDistances.For(Driver, v, Dt, TrafficV2Settings.JunctionStopControlMarginMeters);
            var approach = new JunctionApproach(remaining, -sInMovement, distances, true, RoadId.None, true, true,
                default(JunctionExitAssessment), Starts(m, index, remaining, -sInMovement));
            Junction junction;
            m.TryGetJunction(traversal.JunctionId, out junction);
            var corners = Enumerable.Repeat(junction.Boundary.Center, 4).ToArray();
            return new JunctionActorReport(id, true, movements[occupied], corners, new[] { movements[occupied] },
                withTraversal ? new[] { remaining } : new JunctionTraversal[0], positions, new[] { approach }, false, false,
                JunctionRequestRejection.NoTraversal, Reservation, Kinematics(v));
        }

        // Modele en memoire (patron Story535JunctionRulesTests.CrossingSource) : approches droites independantes, une zone Crossing.
        private static RoadModelSource CrossingSource(params float[] headings)
        {
            var authored = RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath));
            var source = new RoadModelSource { ModelId = Id(1000), ValidationProfile = authored.ValidationProfile,
                LocalizationProfile = authored.LocalizationProfile, DrivabilityProfile = authored.DrivabilityProfile,
                Sections = new RoadSection[headings.Length * 2], Corridors = new LaneCorridor[headings.Length * 2],
                Movements = new JunctionMovement[headings.Length], Controls = new JunctionControl[headings.Length],
                Junctions = new[] { new Junction { Id = Id(1001), Feature = JunctionFeature.Crossroads,
                    Boundary = new RoadBoundsBox { Center = Vector3.zero, Extents = new Vector3(15f, 3f, 15f) } } } };
            for (int i = 0; i < headings.Length; i++)
            {
                Vector3 tangent = Quaternion.AngleAxis(headings[i], Vector3.up) * Vector3.forward;
                Vector3 offset = i == 4 ? Vector3.Cross(Vector3.up, tangent) * 5f : Vector3.zero;
                for (int j = 0; j < 2; j++)
                {
                    int k = i * 2 + j;
                    source.Sections[k] = new RoadSection { Id = Id(1100 + k), DefaultSpeedLimitMetersPerSecond = 10f,
                        DefaultAllowedVehicleClasses = VehicleClassMask.All };
                    source.Corridors[k] = new LaneCorridor { Id = Id(1200 + k), SectionId = source.Sections[k].Id,
                        IsCrossSectionDatum = true, LengthMeters = 40f,
                        Samples = Straight(offset + tangent * (j == 0 ? -45f : 5f), tangent, 40f, source.ValidationProfile) };
                }
                source.Movements[i] = new JunctionMovement { Id = Id(1300 + i), JunctionId = Id(1001),
                    FromCorridorId = source.Corridors[i * 2].Id, ToCorridorId = source.Corridors[i * 2 + 1].Id,
                    LengthMeters = 10f, Samples = Straight(offset - tangent * 5f, tangent, 10f, source.ValidationProfile) };
                source.Controls[i] = new JunctionControl { Id = Id(1400 + i), JunctionId = Id(1001),
                    Kind = JunctionControlKind.Uncontrolled, ControlledMovementIds = new[] { source.Movements[i].Id } };
            }
            source.ConflictZones = new[] { new ConflictZone { Id = Id(1500), JunctionId = Id(1001), Kind = ConflictKind.Crossing,
                Volume = source.Junctions[0].Boundary, MemberMovementIds = source.Movements.Select(x => x.Id).ToArray() } };
            return source;
        }

        private static RoadCurveSample[] Straight(Vector3 start, Vector3 tangent, float length, RoadModelValidationProfile profile)
        {
            int count = Mathf.RoundToInt(length / 0.1f);
            return Enumerable.Range(0, count + 1).Select(i => new RoadCurveSample { SMeters = length * i / count,
                Position = start + tangent * (length * i / count), Tangent = tangent, Up = Vector3.up,
                HalfWidthLeftMeters = profile.MaxVehicleHalfWidthMeters + profile.LateralClearanceMarginMeters,
                HalfWidthRightMeters = profile.MaxVehicleHalfWidthMeters + profile.LateralClearanceMarginMeters }).ToArray();
        }
    }
}
