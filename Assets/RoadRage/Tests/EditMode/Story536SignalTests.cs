using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Signals;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.36 -- feux sur une fixture deterministe : la croix reelle de MVP_Run (4 controles Uncontrolled) passee en
    /// Signalized, compilee en memoire depuis le document committe, sans toucher aux fichiers de MVP_Run. Validation du plan,
    /// horloge de phase hote, branchement du runner et regle de feu du coordinateur (matrice de la spec).
    /// </summary>
    [Category("Core")]
    [Category("Story536")]
    public sealed class Story536SignalTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const float Dt = 0.02f;
        private const float CarLength = 4.44f;
        private const float AllRedSeconds = 2f, GreenSeconds = 10f, YellowSeconds = 3f;
        private static readonly Vector3[] FarAway = { new Vector3(1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f),
            new Vector3(1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f) };

        private static readonly RoadId Plan = Id(1);
        private static readonly RoadId AllRed = Id(200);
        private static readonly RoadId CompatibleGreen = Id(300);

        private static CompiledRoadModel model, signalModel, movementGroupModel;
        private static RoadId[] movementPair;

        private static RoadId Id(int n) { return new RoadId(0x536UL, (ulong)n); }
        private static RoadId Green(int group) { return Id(210 + group); }
        private static RoadId Yellow(int group) { return Id(220 + group); }

        private static RoadModelSource Source() { return RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath)); }

        private static CompiledRoadModel Model { get { return model ?? (model = RoadModelCompiler.Compile(Source())); } }

        /// <summary>Croix : seul carrefour a quatre controles, tous Uncontrolled ; controles tries par Id.</summary>
        private static JunctionControl[] CrossControls(RoadModelSource source)
        {
            var crosses = source.Controls.GroupBy(c => c.JunctionId)
                .Where(g => g.Count() == 4 && g.All(c => c.Kind == JunctionControlKind.Uncontrolled)).ToList();
            Assert.That(crosses.Count, Is.EqualTo(1), "Premisse : une seule croix Uncontrolled a quatre approches.");
            return crosses[0].OrderBy(c => c.Id).ToArray();
        }

        private static SignalPhase Phase(RoadId id, float seconds, int groups, Func<int, SignalState> state)
        {
            return new SignalPhase { PhaseId = id, DurationSeconds = seconds,
                GroupStates = Enumerable.Range(0, groups).Select(g => new SignalGroupState { GroupId = Id(100 + g), State = state(g) }).ToArray() };
        }

        /// <summary>Croix signalisee : un groupe par approche ; tout-rouge 2 s, puis vert 10 s / jaune 3 s par approche.</summary>
        private static RoadModelSource SignalSource()
        {
            var source = Source();
            var controls = CrossControls(source);
            var phases = new List<SignalPhase> { Phase(AllRed, AllRedSeconds, 4, g => SignalState.Red) };
            for (int k = 0; k < 4; k++)
            {
                int green = k;
                phases.Add(Phase(Green(k), GreenSeconds, 4, g => g == green ? SignalState.Green : SignalState.Red));
                phases.Add(Phase(Yellow(k), YellowSeconds, 4, g => g == green ? SignalState.Yellow : SignalState.Red));
            }
            Signalize(source, controls);
            source.SignalPlans = new[] { new SignalPlan { Id = Plan, JunctionId = controls[0].JunctionId,
                Groups = controls.Select((c, g) => new SignalGroup { GroupId = Id(100 + g), MemberMovementIds = c.ControlledMovementIds }).ToArray(),
                Phases = phases.ToArray() } };
            return source;
        }

        private static void Signalize(RoadModelSource source, JunctionControl[] controls)
        {
            foreach (var control in controls)
            {
                int index = Array.FindIndex(source.Controls, c => c.Id == control.Id);
                source.Controls[index].Kind = JunctionControlKind.Signalized;
            }
        }

        private static CompiledRoadModel SignalModel
        {
            get { return signalModel ?? (signalModel = RoadModelCompiler.Compile(SignalSource())); }
        }

        /// <summary>Deux mouvements d'approches differentes sans zone commune, de la croix reelle.</summary>
        private static RoadId[] MovementPair
        {
            get
            {
                if (movementPair != null) return movementPair;
                var index = JunctionConflictIndex.For(Model);
                var controls = CrossControls(Source());
                for (int a = 0; a < controls.Length; a++)
                    for (int b = a + 1; b < controls.Length; b++)
                        foreach (var ma in controls[a].ControlledMovementIds)
                            foreach (var mb in controls[b].ControlledMovementIds)
                            {
                                RoadId zone;
                                if (movementPair == null && !index.TryGetConflict(ma, mb, out zone)) movementPair = new[] { ma, mb };
                            }
                Assert.That(movementPair, Is.Not.Null, "Premisse : deux mouvements compatibles d'approches differentes.");
                return movementPair;
            }
        }

        /// <summary>Variante a un groupe par mouvement : tout-rouge, puis la paire compatible verte ensemble.</summary>
        private static CompiledRoadModel MovementGroupModel
        {
            get
            {
                if (movementGroupModel != null) return movementGroupModel;
                var source = Source();
                var controls = CrossControls(source);
                var movements = controls.SelectMany(c => c.ControlledMovementIds).OrderBy(m => m).ToArray();
                Signalize(source, controls);
                var pair = MovementPair;
                source.SignalPlans = new[] { new SignalPlan { Id = Plan, JunctionId = controls[0].JunctionId,
                    Groups = movements.Select((m, g) => new SignalGroup { GroupId = Id(100 + g), MemberMovementIds = new[] { m } }).ToArray(),
                    Phases = new[] {
                        Phase(AllRed, AllRedSeconds, movements.Length, g => SignalState.Red),
                        Phase(CompatibleGreen, GreenSeconds, movements.Length, g => pair.Contains(movements[g]) ? SignalState.Green : SignalState.Red) } } };
                return movementGroupModel = RoadModelCompiler.Compile(source);
            }
        }

        // ============================================================ helpers du coordinateur (patron 5.35)

        private static DriverProfile Driver { get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; } }
        private static float Reservation { get { return CarLength + Driver.MinimumGap; } }
        private static JunctionDistances Distances(float v) { return JunctionDistances.For(Driver, v, Dt, TrafficV2Settings.JunctionStopControlMarginMeters); }

        private static JunctionKinematics Kinematics(float v)
        {
            var driver = Driver;
            return new JunctionKinematics(v, driver.MaxAcceleration, driver.DesiredSpeed, driver.ComfortableDeceleration, CarLength);
        }

        private static JunctionTraversal Traversal(JunctionConflictIndex index, RoadId movement)
        {
            return new JunctionTraversal(index.JunctionOf(movement), new[] { movement }, index.ToCorridorOf(movement));
        }

        private static JunctionActorReport Requesting(CompiledRoadModel m, RoadId id, JunctionTraversal traversal, float d, float v)
        {
            var index = JunctionConflictIndex.For(m);
            var distances = Distances(v);
            float b = index.BoundaryOf(traversal.FirstMovementId);
            var approach = new JunctionApproach(traversal, d, distances, true, RoadId.None, false, false,
                new JunctionExitAssessment(100f, JunctionExitBound.Occupant, RoadId.None, Reservation), new[] { d - b }, b);
            bool valid = d <= distances.RequestThresholdMeters;
            var positions = new[] { new JunctionMovementPosition(traversal.FirstMovementId, JunctionMovementStatus.Ahead) };
            return new JunctionActorReport(id, true, index.FromCorridorOf(traversal.FirstMovementId), FarAway, null, null, positions,
                new[] { approach }, true, valid, valid ? JunctionRequestRejection.None : JunctionRequestRejection.TooFar, Reservation, Kinematics(v));
        }

        /// <summary>Vehicule dans le mouvement de la traversee (occupant), pare-chocs a 1 m dans le mouvement.</summary>
        private static JunctionActorReport Inside(CompiledRoadModel m, RoadId id, JunctionTraversal traversal, float v)
        {
            var movement = traversal.FirstMovementId;
            var approach = new JunctionApproach(traversal, -1f, Distances(v), true, RoadId.None, true, true, default(JunctionExitAssessment), new[] { -1f });
            Junction junction;
            m.TryGetJunction(traversal.JunctionId, out junction);
            var corners = Enumerable.Repeat(junction.Boundary.Center, 4).ToArray();
            return new JunctionActorReport(id, true, movement, corners, new[] { movement }, new[] { traversal },
                new[] { new JunctionMovementPosition(movement, JunctionMovementStatus.Occupied) }, new[] { approach }, false, false,
                JunctionRequestRejection.NoTraversal, Reservation, Kinematics(v));
        }

        private static TrafficFrame Frame(CompiledRoadModel m, ulong frameId, RoadId phase)
        {
            return new TrafficFrame(frameId, m, new TrafficActorInput[0], null, new[] { new SignalPhaseInput(Plan, phase) });
        }

        private static TrafficFrame FrameWithoutSignals(CompiledRoadModel m, ulong frameId)
        {
            return new TrafficFrame(frameId, m, new TrafficActorInput[0]);
        }

        private static JunctionRecord Decision(JunctionSnapshot snapshot, RoadId id, RoadId traversal)
        {
            JunctionRecord record;
            Assert.That(snapshot.TryGetDecision(id, traversal, out record), Is.True, "record de " + id + "\n" + snapshot.ToText());
            return record;
        }

        private static void AssertDecision(JunctionSnapshot snapshot, RoadId id, RoadId traversal, JunctionGrantStatus status, JunctionReason reason,
            RoadId cause = default(RoadId))
        {
            var record = Decision(snapshot, id, traversal);
            Assert.That(record.Status, Is.EqualTo(status), snapshot.ToText());
            Assert.That(record.Reason, Is.EqualTo(reason), snapshot.ToText());
            if (!cause.IsEmpty) Assert.That(record.CauseActorId, Is.EqualTo(cause), snapshot.ToText());
        }

        /// <summary>Un grant revoque au lot : le record Revoked existe, et la demande de la frame est refusee pour la meme raison.</summary>
        private static void AssertRevoked(JunctionSnapshot snapshot, RoadId id, JunctionReason reason)
        {
            Assert.That(snapshot.Records.Any(r => r.TrafficId == id && r.Status == JunctionGrantStatus.Revoked && r.Reason == reason), Is.True,
                snapshot.ToText());
            Assert.That(snapshot.Records.Any(r => r.TrafficId == id && r.IsEffectiveGrant), Is.False, snapshot.ToText());
        }

        /// <summary>Le premier mouvement de l'approche 0 et un mouvement de l'approche 1 qui le croise.</summary>
        private static void ConflictingApproaches(out RoadId a, out RoadId b)
        {
            var index = JunctionConflictIndex.For(SignalModel);
            var controls = CrossControls(Source());
            a = RoadId.None; b = RoadId.None;
            foreach (var ma in controls[0].ControlledMovementIds)
                foreach (var mb in controls[1].ControlledMovementIds)
                {
                    RoadId zone;
                    if (a.IsEmpty && index.TryGetConflict(ma, mb, out zone)) { a = ma; b = mb; }
                }
            Assert.That(a.IsEmpty, Is.False, "Premisse : approches 0 et 1 incompatibles.");
        }

        // ============================================================ validation (D4)

        private static void AssertRejected(Action<RoadModelSource> mutate, RoadModelValidationCode expected)
        {
            var source = SignalSource();
            mutate(source);
            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); });
            Assert.That(exception.HasCode(expected), Is.True, "Code attendu " + expected + ", obtenu : " + exception.Message);
        }

        [Test]
        public void TheSignalizedFixturesCompileAndMvpRunOwnsNoSignal()
        {
            Assert.That(SignalModel.SignalPlans.Count, Is.EqualTo(1));
            Assert.That(SignalModel.SignalPlans[0].Phases.Count, Is.EqualTo(9));
            Assert.That(MovementGroupModel.SignalPlans[0].Groups.Count, Is.EqualTo(12));
            Assert.That(Model.SignalPlans.Count, Is.Zero, "MVP_Run n'a aucun carrefour signalise ni plan factice.");
            Assert.That(Model.Controls.Any(c => c.Kind == JunctionControlKind.Signalized), Is.False);
        }

        [Test]
        public void APlanOutsideASignalizedJunctionOrAnIncompletePhaseFailsValidation()
        {
            // Plan factice sur un carrefour non signalise.
            AssertRejected(source =>
            {
                var other = source.Controls.First(c => c.JunctionId != source.SignalPlans[0].JunctionId).JunctionId;
                var dummy = new SignalPlan { Id = Id(2), JunctionId = other,
                    Groups = new[] { new SignalGroup { GroupId = Id(150), MemberMovementIds = new RoadId[0] } },
                    Phases = new[] { new SignalPhase { PhaseId = Id(250), DurationSeconds = 5f,
                        GroupStates = new[] { new SignalGroupState { GroupId = Id(150), State = SignalState.Green } } } } };
                source.SignalPlans = new[] { source.SignalPlans[0], dummy };
            }, RoadModelValidationCode.SignalPlanScopeInvalid);
            // Membre de groupe hors des mouvements Signalized du carrefour.
            AssertRejected(source =>
            {
                var foreign = source.Controls.First(c => c.Kind != JunctionControlKind.Signalized).ControlledMovementIds[0];
                source.SignalPlans[0].Groups[0].MemberMovementIds = source.SignalPlans[0].Groups[0].MemberMovementIds.Concat(new[] { foreign }).ToArray();
            }, RoadModelValidationCode.SignalPlanScopeInvalid);
            // Phase qui omet un groupe, puis phase qui l'enonce deux fois.
            AssertRejected(source => source.SignalPlans[0].Phases[1].GroupStates = source.SignalPlans[0].Phases[1].GroupStates.Take(3).ToArray(),
                RoadModelValidationCode.SignalPhaseStatesIncomplete);
            AssertRejected(source => source.SignalPlans[0].Phases[1].GroupStates = source.SignalPlans[0].Phases[1].GroupStates
                .Concat(new[] { source.SignalPlans[0].Phases[1].GroupStates[0] }).ToArray(), RoadModelValidationCode.SignalPhaseStatesIncomplete);
            // Membre Signalized d'un autre carrefour signalise : la T la plus basse passee en Signalized avec son propre plan.
            AssertRejected(source =>
            {
                var cross = source.SignalPlans[0].JunctionId;
                var t = source.Controls.Where(c => c.JunctionId != cross).GroupBy(c => c.JunctionId).Where(g => g.Count() == 3)
                    .OrderBy(g => g.Key).First().OrderBy(c => c.Id).ToArray();
                Signalize(source, t);
                var tPlan = new SignalPlan { Id = Id(3), JunctionId = t[0].JunctionId,
                    Groups = t.Select((c, g) => new SignalGroup { GroupId = Id(160 + g), MemberMovementIds = c.ControlledMovementIds }).ToArray(),
                    Phases = new[] { new SignalPhase { PhaseId = Id(260), DurationSeconds = 5f, GroupStates = t.Select((c, g) =>
                        new SignalGroupState { GroupId = Id(160 + g), State = SignalState.Red }).ToArray() } } };
                source.SignalPlans = new[] { source.SignalPlans[0], tPlan };
                source.SignalPlans[0].Groups[0].MemberMovementIds = source.SignalPlans[0].Groups[0].MemberMovementIds
                    .Concat(new[] { t[0].ControlledMovementIds[0] }).ToArray();
            }, RoadModelValidationCode.SignalPlanScopeInvalid);
            // Signalized melange a un autre genre.
            AssertRejected(source =>
            {
                int index = Array.FindIndex(source.Controls, c => c.Kind == JunctionControlKind.Signalized);
                source.Controls[index].Kind = JunctionControlKind.Yield;
            }, RoadModelValidationCode.JunctionControlKindsMixed);
            // Sans plan, phases absentes, deux verts dans une zone.
            AssertRejected(source => source.SignalPlans = new SignalPlan[0], RoadModelValidationCode.SignalizedControlWithoutPlan);
            AssertRejected(source => source.SignalPlans[0].Phases = new SignalPhase[0], RoadModelValidationCode.SignalizedControlWithoutPlan);
            AssertRejected(source =>
            {
                RoadId a, b;
                ConflictingApproaches(out a, out b);
                source.SignalPlans[0].Phases[1].GroupStates[1].State = SignalState.Green;
            }, RoadModelValidationCode.ConflictingMovementsGreenTogether);
        }

        // ============================================================ horloge (D5)

        [Test]
        public void ThePhaseClockAdvancesDeterministicallyThroughTheCycle()
        {
            var clock = new SignalPhaseController(SignalModel);
            Assert.That(clock.Current.Single().PhaseId, Is.EqualTo(AllRed), "Chaque plan demarre a sa phase 0.");
            // Pas binaire exact : transitions a l'abscisse exacte, cycle de 2 + 4 x 13 = 54 s.
            const float step = 0.25f;
            var expected = new List<KeyValuePair<int, RoadId>> { new KeyValuePair<int, RoadId>(8, Green(0)), new KeyValuePair<int, RoadId>(48, Yellow(0)),
                new KeyValuePair<int, RoadId>(60, Green(1)), new KeyValuePair<int, RoadId>(216, AllRed) };
            var index = JunctionConflictIndex.For(SignalModel);
            var seen = new Dictionary<int, RoadId>();
            for (int i = 1; i <= 216; i++)
            {
                var before = clock.Current[0].PhaseId;
                clock.Advance(step);
                if (clock.Current[0].PhaseId != before) seen[i] = clock.Current[0].PhaseId;
                // Jamais deux membres d'une zone verts ensemble, a chaque pas du cycle.
                var frame = new TrafficFrame((ulong)i, SignalModel, new TrafficActorInput[0], null, clock.Current);
                foreach (var a in SignalModel.Movements.Select(mv => mv.Id).Where(id => index.ControlKindOf(id) == JunctionControlKind.Signalized))
                    foreach (var b in index.IncompatibleWith(a))
                    {
                        SignalState sa, sb;
                        Assert.That(frame.TryGetSignalState(a, out sa) && frame.TryGetSignalState(b, out sb)
                            && sa == SignalState.Green && sb == SignalState.Green, Is.False, "verts incompatibles au pas " + i);
                    }
            }
            Assert.That(seen.Count, Is.EqualTo(9), "Neuf phases parcourues sur un cycle.");
            foreach (var pair in expected) Assert.That(seen[pair.Key], Is.EqualTo(pair.Value), "pas " + pair.Key);

            // dt = 0,02 : meme suite, meme resultat ; la fin du tout-rouge tombe au pas 100 ou 101 (0,02f non binaire).
            var first = new SignalPhaseController(SignalModel);
            var second = new SignalPhaseController(SignalModel);
            int change = 0;
            for (int i = 1; i <= 3000; i++)
            {
                first.Advance(Dt);
                second.Advance(Dt);
                Assert.That(first.Current[0].PhaseId, Is.EqualTo(second.Current[0].PhaseId));
                if (change == 0 && first.Current[0].PhaseId != AllRed) change = i;
            }
            Assert.That(change, Is.InRange(100, 101));

            foreach (var invalid in new[] { 0f, -0.02f, float.NaN, float.PositiveInfinity })
                Assert.Throws<ArgumentException>(() => clock.Advance(invalid), invalid.ToString());
            var none = new SignalPhaseController(Model);
            none.Advance(Dt);
            Assert.That(none.Current, Is.Empty, "MVP_Run : aucun plan, aucun travail.");
        }

        [Test]
        public void TheRunnerOwnsTheClockAndAdvancesItOncePerHostStep()
        {
            var runner = new TrafficV2StepRunner();
            var reference = new SignalPhaseController(SignalModel);
            for (int i = 0; i < 150; i++)
            {
                runner.Step(SignalModel, new TrafficV2VehicleDriver[0]);
                reference.Advance(Time.fixedDeltaTime);
                Assert.That(runner.Signals.Current[0].PhaseId, Is.EqualTo(reference.Current[0].PhaseId), "pas " + i);
            }
            Assert.That(runner.Signals.Model, Is.SameAs(SignalModel));
            Assert.That(runner.Signals.Current[0].PhaseId, Is.Not.EqualTo(AllRed), "150 pas fixes depassent le tout-rouge de 2 s.");
            runner.Step(Model, new TrafficV2VehicleDriver[0]);
            Assert.That(runner.Signals.Model, Is.SameAs(Model), "une horloge par modele");
            Assert.That(runner.Signals.Current, Is.Empty);

            // Branchement (aucun vehicule V2 liable en EditMode, patron de Story533SharedFrameTests) : la frame N porte la phase
            // courante, le lot lit cette frame, puis l'horloge avance d'un pas fixe.
            string source = File.ReadAllText("Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs");
            int frame = source.IndexOf("new TrafficFrame(FrameId, model, inputs, hazards, signals.Current)", StringComparison.Ordinal);
            int resolve = source.IndexOf("coordinator.Resolve(FrameId, reports, frame)", StringComparison.Ordinal);
            int advance = source.IndexOf("signals.Advance(", StringComparison.Ordinal);
            Assert.That(frame, Is.GreaterThan(0), "la frame recoit les phases de l'horloge");
            Assert.That(resolve, Is.GreaterThan(0), "le lot recoit la frame");
            Assert.That(advance, Is.GreaterThan(resolve), "l'horloge avance apres le lot");
            Assert.That(source.IndexOf("signals.Advance(", advance + 1, StringComparison.Ordinal), Is.LessThan(0), "un seul pas d'horloge");
        }

        // ============================================================ coordinateur (D1-D3)

        [Test]
        public void OnlyGreenGrantsAndYellowOrRedDenies()
        {
            RoadId a, b;
            ConflictingApproaches(out a, out b);
            var index = JunctionConflictIndex.For(SignalModel);
            var traversal = Traversal(index, a);
            foreach (var phase in new[] { Green(0), Yellow(0), AllRed, Green(1) })
            {
                var snapshot = new JunctionCoordinator(SignalModel).Resolve(1, new[] { Requesting(SignalModel, Id(1000), traversal, 0.3f, 0f) },
                    Frame(SignalModel, 1, phase));
                if (phase == Green(0)) AssertDecision(snapshot, Id(1000), a, JunctionGrantStatus.Granted, JunctionReason.Granted);
                else AssertDecision(snapshot, Id(1000), a, JunctionGrantStatus.Denied, JunctionReason.SignalStop);
            }
        }

        [Test]
        public void AYellowRevokesAnUnengagedGrantAndKeepsAnEngagedOne()
        {
            RoadId a, b;
            ConflictingApproaches(out a, out b);
            var index = JunctionConflictIndex.For(SignalModel);
            var traversal = Traversal(index, a);
            var distances = Distances(8f);
            Assert.That(distances.StopMeters, Is.LessThan(distances.RequestThresholdMeters));
            var far = Requesting(SignalModel, Id(1000), traversal, 0.5f * (distances.StopMeters + distances.RequestThresholdMeters), 8f);
            var near = Requesting(SignalModel, Id(1000), traversal, 0.5f * distances.StopMeters, 8f);

            var unengaged = new JunctionCoordinator(SignalModel);
            AssertDecision(unengaged.Resolve(1, new[] { far }, Frame(SignalModel, 1, Green(0))), Id(1000), a, JunctionGrantStatus.Granted, JunctionReason.Granted);
            AssertRevoked(unengaged.Resolve(2, new[] { far }, Frame(SignalModel, 2, Yellow(0))), Id(1000), JunctionReason.SignalStop);

            foreach (var engaged in new[] { near, Inside(SignalModel, Id(1000), traversal, 8f) })
            {
                var coordinator = new JunctionCoordinator(SignalModel);
                coordinator.Resolve(1, new[] { far }, Frame(SignalModel, 1, Green(0)));
                var snapshot = coordinator.Resolve(2, new[] { engaged }, Frame(SignalModel, 2, Yellow(0)));
                Assert.That(snapshot.Records.Any(r => r.TrafficId == Id(1000) && r.Status == JunctionGrantStatus.Held
                    && r.Reason == JunctionReason.Committed), Is.True, snapshot.ToText());
                // Le vert incompatible suivant reste refuse tant que l'engage tient son grant.
                var next = coordinator.Resolve(3, new[] { engaged, Requesting(SignalModel, Id(1001), Traversal(index, b), 0.3f, 0f) },
                    Frame(SignalModel, 3, Green(1)));
                Assert.That(Decision(next, Id(1001), b).Status, Is.EqualTo(JunctionGrantStatus.Denied), next.ToText());
            }
        }

        [Test]
        public void AMissingSignalStateIsNeverGreen()
        {
            RoadId a, b;
            ConflictingApproaches(out a, out b);
            var traversal = Traversal(JunctionConflictIndex.For(SignalModel), a);
            var request = Requesting(SignalModel, Id(1000), traversal, 0.3f, 0f);
            AssertDecision(new JunctionCoordinator(SignalModel).Resolve(1, new[] { request }, FrameWithoutSignals(SignalModel, 1)),
                Id(1000), a, JunctionGrantStatus.Denied, JunctionReason.SignalUnavailable);
            AssertDecision(new JunctionCoordinator(SignalModel).Resolve(1, new[] { request }),
                Id(1000), a, JunctionGrantStatus.Denied, JunctionReason.SignalUnavailable);
            var held = new JunctionCoordinator(SignalModel);
            var far = Requesting(SignalModel, Id(1000), traversal, 0.5f * (Distances(8f).StopMeters + Distances(8f).RequestThresholdMeters), 8f);
            held.Resolve(1, new[] { far }, Frame(SignalModel, 1, Green(0)));
            AssertRevoked(held.Resolve(2, new[] { far }, FrameWithoutSignals(SignalModel, 2)), Id(1000), JunctionReason.SignalUnavailable);
        }

        [Test]
        public void AGreenNeverWaivesOccupantProtectionAndARedNeverReserves()
        {
            RoadId a, b;
            ConflictingApproaches(out a, out b);
            var index = JunctionConflictIndex.For(SignalModel);
            // Feu brule : occupant sans grant sur l'approche rouge, protege ; la demande verte incompatible attend.
            var runner = new JunctionCoordinator(SignalModel).Resolve(1, new[] { Inside(SignalModel, Id(1000), Traversal(index, a), 3f),
                Requesting(SignalModel, Id(1001), Traversal(index, b), 0.3f, 0f) }, Frame(SignalModel, 1, Green(1)));
            AssertDecision(runner, Id(1001), b, JunctionGrantStatus.Denied, JunctionReason.ConflictOccupied, Id(1000));

            // Rouge ancien, vert plus jeune incompatible : le vert passe.
            var coordinator = new JunctionCoordinator(SignalModel);
            var red = Requesting(SignalModel, Id(1000), Traversal(index, a), 0.3f, 0f);
            AssertDecision(coordinator.Resolve(1, new[] { red }, Frame(SignalModel, 1, Green(1))), Id(1000), a, JunctionGrantStatus.Denied,
                JunctionReason.SignalStop);
            var snapshot = coordinator.Resolve(2, new[] { red, Requesting(SignalModel, Id(1001), Traversal(index, b), 0.3f, 0f) },
                Frame(SignalModel, 2, Green(1)));
            AssertDecision(snapshot, Id(1001), b, JunctionGrantStatus.Granted, JunctionReason.Granted);
            Assert.That(Decision(snapshot, Id(1000), a).RequestSinceFrame, Is.EqualTo(1UL));
        }

        [Test]
        public void AnAllRedPhaseGrantsNothingAndNeverBreaksADeadlock()
        {
            var index = JunctionConflictIndex.For(SignalModel);
            var controls = CrossControls(Source());
            var reports = controls.Select((c, k) => Requesting(SignalModel, Id(1000 + k), Traversal(index, c.ControlledMovementIds[0]), 0.3f, 0f)).ToArray();
            var coordinator = new JunctionCoordinator(SignalModel);
            for (ulong frame = 1; frame <= 20; frame++)
            {
                var snapshot = coordinator.Resolve(frame, reports, Frame(SignalModel, frame, AllRed));
                Assert.That(snapshot.Records.Any(r => r.IsEffectiveGrant), Is.False, snapshot.ToText());
                Assert.That(snapshot.Counters.DeadlockBreaks, Is.Zero);
                Assert.That(snapshot.Records.All(r => r.Reason == JunctionReason.SignalStop), Is.True, snapshot.ToText());
            }
        }

        [Test]
        public void CompatibleGreensAreGrantedTogetherAndTheBatchIsOrderIndependent()
        {
            var index = JunctionConflictIndex.For(MovementGroupModel);
            var pair = MovementPair;
            var both = new[] { Requesting(MovementGroupModel, Id(1000), Traversal(index, pair[0]), 0.3f, 0f),
                Requesting(MovementGroupModel, Id(1001), Traversal(index, pair[1]), 0.3f, 0f) };
            var snapshot = new JunctionCoordinator(MovementGroupModel).Resolve(1, both, Frame(MovementGroupModel, 1, CompatibleGreen));
            AssertDecision(snapshot, Id(1000), pair[0], JunctionGrantStatus.Granted, JunctionReason.Granted);
            AssertDecision(snapshot, Id(1001), pair[1], JunctionGrantStatus.Granted, JunctionReason.Granted);

            var signalIndex = JunctionConflictIndex.For(SignalModel);
            var reports = CrossControls(Source()).Select((c, k) => Requesting(SignalModel, Id(1000 + k),
                Traversal(signalIndex, c.ControlledMovementIds[0]), 0.3f, 0f)).ToArray();
            string reference = null;
            foreach (var order in new[] { new[] { 0, 1, 2, 3 }, new[] { 3, 2, 1, 0 }, new[] { 2, 0, 3, 1 } })
            {
                var coordinator = new JunctionCoordinator(SignalModel);
                string text = "";
                for (ulong frame = 1; frame <= 3; frame++)
                    text += coordinator.Resolve(frame, order.Select(i => reports[i]).ToArray(), Frame(SignalModel, frame, Green(0))).ToText();
                if (reference == null) reference = text;
                Assert.That(text, Is.EqualTo(reference));
            }
        }

        [Test]
        public void AFrameFromAnotherBatchOrModelIsRefusedAndOnlyTheCoordinatorReadsSignals()
        {
            var coordinator = new JunctionCoordinator(SignalModel);
            Assert.Throws<ArgumentException>(() => coordinator.Resolve(2, new JunctionActorReport[0], Frame(SignalModel, 1, AllRed)));
            Assert.Throws<ArgumentException>(() => coordinator.Resolve(1, new JunctionActorReport[0], FrameWithoutSignals(Model, 1)));

            // Le feu est une regle du coordinateur : aucun autre code ne lit l'etat de feu pour une permission (exceptions : 5.41).
            var root = "Assets/RoadRage";
            var readers = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Replace('\\', '/').Contains("/Tests/") && File.ReadAllText(f).Contains("TryGetSignalState("))
                .Select(f => Path.GetFileName(f)).OrderBy(f => f).ToArray();
            Assert.That(readers, Is.EqualTo(new[] { "JunctionCoordinator.cs", "TrafficFrame.cs" }));
        }
    }
}
