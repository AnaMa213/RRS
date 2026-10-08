using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Collisions;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Recovery;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Features.Vehicles.Traffic.Safety;
using RoadRage.Features.Vehicles.Traffic.Tactical;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.39 -- superviseur de recuperation : progression attendue contre reelle et suppression par blocker legitime
    /// (R1, R2), poignee de main et refus (R3), escalade jusqu'a Faulted (R4), manoeuvres et commandes au vrai composeur (R5),
    /// branchement du driver (R6, R7) et absence structurelle de toute ecriture du corps, teleportation ou retrait.
    /// </summary>
    [Category("Core")]
    [Category("Story539")]
    public sealed class Story539RecoveryTests
    {
        private const string VehicleProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string DriverPath = "Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs";
        private const string RecoveryFolder = "Assets/RoadRage/Features/Vehicles/Traffic/Recovery";
        private const float Dt = 0.02f;
        private const float Mass = 1200f;
        private const float Lock = 30f;
        private const float BMax = 8f;
        private const float BComfort = 2f;
        private const ulong Frame = 10UL;
        private static readonly RoadId Agent = new RoadId(0x539UL, 1UL);
        private static readonly RoadId Other = new RoadId(0x539UL, 2UL);
        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };
        private static float Tol { get { return TrafficV2Settings.DeclaredTrackingTolerance.Meters; } }

        // ------------------------------------------------------------------ R2 progression attendue

        [Test]
        public void AFreeRoadVehicleThatDoesNotMoveBecomesEligibleOnlyOnceTheExpectedProgressIsReached()
        {
            var free = FreeDecision();
            float route;
            Assert.That(RecoverySupervisor.ProgressExpected(free, BlockerTracker.Empty, out route), Is.True, free.ToText());
            Assert.That(route, Is.GreaterThan(0f));

            var supervisor = new RecoverySupervisor();
            int eligibleAt = -1;
            for (int i = 0; i < 1000 && eligibleAt < 0; i++)
            {
                supervisor.Observe(Nominal(free, BlockerTracker.Empty, speed: 0f, distance: 10f));
                if (supervisor.Cause == RecoveryCause.ProgressDeficit) eligibleAt = i;
                else Assert.That(supervisor.ExpectedMeters, Is.LessThan(TrafficV2Settings.RecoveryExpectedProgressMeters), "pas avant E = 2 m");
            }
            Assert.That(eligibleAt, Is.GreaterThan(0), "cale sans blocker : eligible");
            Assert.That(supervisor.ExpectedMeters, Is.GreaterThanOrEqualTo(TrafficV2Settings.RecoveryExpectedProgressMeters));
            Assert.That(supervisor.ActualMeters, Is.LessThan(TrafficV2Settings.RecoveryMinimumProgressMeters));
            // E integre a_route depuis l'arret : 1/2 a t^2 = 2 m, jamais une duree fixe.
            float expected = Mathf.Sqrt(2f * TrafficV2Settings.RecoveryExpectedProgressMeters / route) / Dt;
            Assert.That(eligibleAt, Is.EqualTo(expected).Within(3f));
        }

        [Test]
        public void AMovingVehicleASlowCurveAndAnApproachToALeaderAreNeverEligible()
        {
            var free = FreeDecision();
            var moving = new RecoverySupervisor();
            for (int i = 0; i < 5000; i++)
            {
                moving.Observe(Nominal(free, BlockerTracker.Empty, speed: 0.5f, distance: 10f + 0.5f * Dt * i));
                Assert.That(moving.Cause, Is.EqualTo(RecoveryCause.None), "roule a 0,5 m/s, pas " + i);
            }

            var approach = Decide(Plan(5f), 5f, LeaderOnly(10f, 0f), LongitudinalMemory.None);
            Assert.That(approach.Binding.Kind, Is.Not.EqualTo(LongitudinalCandidateKind.Profile), approach.ToText());
            float route;
            Assert.That(RecoverySupervisor.ProgressExpected(approach, BlockerTracker.Empty, out route), Is.False,
                "liante d'interaction sans blocker : progression non attendue");

            var stopped = new RecoverySupervisor();
            for (int i = 0; i < 5000; i++) stopped.Observe(Nominal(approach, BlockerTracker.Empty, speed: 0f, distance: 10f));
            Assert.That(stopped.Cause, Is.EqualTo(RecoveryCause.None));
            Assert.That(RecoverySupervisor.ProgressExpected(null, BlockerTracker.Empty, out route), Is.False, "sans arbitrage");
        }

        [Test]
        public void EveryLegitimateBlockerSuppressesRecoveryAndTheWholeSetIsRead()
        {
            var free = FreeDecision();
            var legitimate = new[]
            {
                BlockerRules.Leader(Other, 5f, Driver.MinimumGap, -1f, 1UL),
                BlockerRules.Obstacle(Other, PerceivedObstacleKind.WalkingPlayer, -1f, 1UL),
                BlockerRules.Obstacle(Other, PerceivedObstacleKind.TrafficActor, -1f, 1UL),
                BlockerRules.Obstacle(Other, PerceivedObstacleKind.Obstacle, -1f, 1UL),
                BlockerRules.PolicyImmobilization(-1f, 1UL),
                BlockerRules.Junction(new JunctionBlockerCause(BlockerKind.JunctionGrant, "titulaire"), "traversee", -1f, 1UL),
                BlockerRules.Junction(new JunctionBlockerCause(BlockerKind.BlockedExit, "sortie"), "traversee", -1f, 1UL),
            };
            foreach (var blocker in legitimate)
            {
                Assert.That(blocker.Legitimate, Is.True, blocker.ToText());
                var supervisor = new RecoverySupervisor();
                // 100 s d'attente legitime : jamais eligible, rien ne s'accumule.
                for (int i = 0; i < 5000; i++) supervisor.Observe(Nominal(free, new[] { blocker }, speed: 0f, distance: 10f));
                Assert.That(supervisor.Cause, Is.EqualTo(RecoveryCause.None), blocker.ToText());
                Assert.That(supervisor.ExpectedMeters, Is.EqualTo(0f), blocker.ToText());
            }

            // Plusieurs causes coexistent : un encastrement dominant n'efface pas une attente de grant legitime.
            var embedded = BlockerRules.Leader(Other, -1f, Driver.MinimumGap, -9f, 1UL);
            var grant = BlockerRules.Junction(new JunctionBlockerCause(BlockerKind.JunctionGrant, "titulaire"), "traversee", -1f, 1UL);
            var set = new List<Blocker> { embedded, grant };
            Blocker dominant;
            Assert.That(BlockerTracker.TryGetDominant(set, out dominant), Is.True);
            Assert.That(dominant.Legitimate, Is.False, "le dominant est l'encastrement");
            float route;
            Assert.That(RecoverySupervisor.ProgressExpected(free, set, out route), Is.False, "l'ensemble entier est lu");
            Assert.That(set.Count, Is.EqualTo(2), "l'ensemble n'est pas reduit");
        }

        [Test]
        public void AnEmbeddedStopHoldOnAnIllegitimateLeaderIsAProgressDeficitThatStartsWithReverse()
        {
            var plan = Plan(0f);
            var memory = LongitudinalMemory.None;
            LongitudinalDecision decision = null;
            IReadOnlyList<Blocker> blockers = BlockerTracker.Empty;
            for (int i = 0; i < 20; i++)
            {
                decision = Decide(plan, 0f, LeaderOnly(-1f, 0f), memory);
                memory = decision.Memory;
                blockers = BlockerTracker.Update(blockers, decision, Driver, (ulong)i + 1UL);
            }
            Assert.That(decision.Hold.Active, Is.True, decision.ToText());
            Assert.That(blockers.Count, Is.EqualTo(1));
            Assert.That(blockers[0].Legitimate, Is.False, "jeu negatif : encastrement");
            Assert.That(blockers[0].Recoverable, Is.True);
            float route;
            Assert.That(RecoverySupervisor.ProgressExpected(decision, blockers, out route), Is.True, decision.ToText());

            var supervisor = new RecoverySupervisor();
            for (int i = 0; i < 1000 && supervisor.Cause == RecoveryCause.None; i++)
                supervisor.Observe(Nominal(decision, blockers, speed: 0f, distance: 10f));
            Assert.That(supervisor.Cause, Is.EqualTo(RecoveryCause.ProgressDeficit));
            var request = supervisor.TryRequest(Frame, Agent);
            Assert.That(request.Maneuver, Is.EqualTo(RecoveryManeuver.Reverse));
            Assert.That(request.Attempt, Is.EqualTo(1));

            // Le meme maintien derriere un leader a jeu legitime n'est jamais eligible.
            memory = LongitudinalMemory.None;
            blockers = BlockerTracker.Empty;
            for (int i = 0; i < 20; i++)
            {
                decision = Decide(plan, 0f, LeaderOnly(Driver.MinimumGap, 0f), memory);
                memory = decision.Memory;
                blockers = BlockerTracker.Update(blockers, decision, Driver, (ulong)i + 1UL);
            }
            Assert.That(blockers.All(b => b.Legitimate), Is.True);
            Assert.That(RecoverySupervisor.ProgressExpected(decision, blockers, out route), Is.False);
        }

        [Test]
        public void DisplacedAndLatchedVehiclesStartWithRealignAndAGoalOrTheExitSuppressesEligibility()
        {
            var displaced = new RecoverySupervisor();
            displaced.Observe(Observation(goal: true, awaiting: true));
            Assert.That(displaced.Cause, Is.EqualTo(RecoveryCause.Displaced));
            Assert.That(displaced.TryRequest(Frame, Agent).Maneuver, Is.EqualTo(RecoveryManeuver.Realign));

            var latched = new RecoverySupervisor();
            latched.Observe(Observation(latchedHeld: true));
            Assert.That(latched.Cause, Is.EqualTo(RecoveryCause.ToleranceLatched));
            Assert.That(latched.TryRequest(Frame, Agent).Maneuver, Is.EqualTo(RecoveryManeuver.Realign));

            var busy = new RecoverySupervisor();
            busy.Observe(Observation(goal: true));
            Assert.That(busy.Cause, Is.EqualTo(RecoveryCause.None), "but de collision en cours : la physique d'abord");
            Assert.That(busy.TryRequest(Frame, Agent), Is.Null);
            var exit = new RecoverySupervisor();
            exit.Observe(Observation(goal: true, awaiting: true, exit: true));
            Assert.That(exit.Cause, Is.EqualTo(RecoveryCause.None));
            Assert.That(displaced.TryRequest(Frame, RoadId.None), Is.Null, "identite vide");
        }

        // ------------------------------------------------------------------ R3 / R4 historique et escalade

        [Test]
        public void ARejectionUpdatesHistoryAlternatesTheManeuverAndTwoInARowFault()
        {
            var supervisor = Displaced();
            var first = supervisor.TryRequest(Frame, Agent);
            supervisor.Record(first, new TacticalResponse(first.Version, TacticalReason.RearBlocked), Frame);
            Assert.That(supervisor.Attempts.Count, Is.EqualTo(1));
            Assert.That(supervisor.Attempts[0].Response, Is.EqualTo(TacticalReason.RearBlocked));
            Assert.That(supervisor.Faulted, Is.False);

            supervisor.Observe(Observation(goal: true, awaiting: true));
            var second = supervisor.TryRequest(Frame + 1UL, Agent);
            Assert.That(second.Version, Is.GreaterThan(first.Version), "jamais la meme requete");
            Assert.That(second.Maneuver, Is.Not.EqualTo(first.Maneuver), "alternance");
            Assert.That(second.Attempt, Is.EqualTo(2));
            supervisor.Record(second, new TacticalResponse(second.Version, TacticalReason.Unstable), Frame + 1UL);
            Assert.That(supervisor.Faulted, Is.True, "deux refus consecutifs");
            Assert.That(supervisor.FaultReason, Does.Contain("ConsecutiveRejections"));
            Assert.That(supervisor.FaultedAtFrame, Is.EqualTo(Frame + 1UL));
            supervisor.Observe(Observation(goal: true, awaiting: true));
            Assert.That(supervisor.TryRequest(Frame + 2UL, Agent), Is.Null, "Faulted : plus aucune requete");
            StringAssert.Contains("Faulted ConsecutiveRejections", supervisor.ToText());
        }

        [Test]
        public void FailedAttemptsAlternateUntilTheBudgetFaultsWhileAnAcceptedManeuverBlocksNewRequests()
        {
            var supervisor = Displaced();
            var maneuvers = new List<RecoveryManeuver>();
            for (int attempt = 1; attempt <= TrafficV2Settings.RecoveryMaxAttempts; attempt++)
            {
                var request = supervisor.TryRequest(Frame + (ulong)attempt, Agent);
                Assert.That(request, Is.Not.Null, "tentative " + attempt);
                maneuvers.Add(request.Maneuver);
                supervisor.Record(request, new TacticalResponse(request.Version, TacticalReason.Accepted), Frame);
                supervisor.Observe(Observation(goal: true));
                Assert.That(supervisor.TryRequest(Frame, Agent), Is.Null, "manoeuvre en cours : aucune requete");
                supervisor.Observe(Observation(goal: true, awaiting: true, outcomeVersion: request.Version, outcome: TacticalReason.Stalled));
                Assert.That(supervisor.Attempts.Last().Outcome, Is.EqualTo(TacticalReason.Stalled));
            }
            CollectionAssert.AreEqual(new[] { RecoveryManeuver.Realign, RecoveryManeuver.Reverse, RecoveryManeuver.Realign,
                RecoveryManeuver.Reverse }, maneuvers);
            Assert.That(supervisor.TryRequest(Frame + 9UL, Agent), Is.Null);
            Assert.That(supervisor.Faulted, Is.True);
            Assert.That(supervisor.FaultReason, Is.EqualTo("AttemptsExhausted"));
        }

        [Test]
        public void NominalProgressAfterAnAttemptEndsTheEpisode()
        {
            var supervisor = Displaced();
            var request = supervisor.TryRequest(Frame, Agent);
            supervisor.Record(request, new TacticalResponse(request.Version, TacticalReason.Accepted), Frame);
            supervisor.Observe(Observation(outcomeVersion: request.Version, outcome: TacticalReason.Resumed));
            Assert.That(supervisor.EpisodeAttempts, Is.EqualTo(1));
            var free = FreeDecision();
            for (int i = 0; i < 40; i++) supervisor.Observe(Nominal(free, BlockerTracker.Empty, speed: 1f, distance: 10f + i * 0.02f));
            Assert.That(supervisor.EpisodeAttempts, Is.EqualTo(0), "0,5 m de conduite nominale : episode clos");
            Assert.That(supervisor.Attempts.Count, Is.EqualTo(1), "l'historique reste");
        }

        [TestCase(RecoveryCause.Displaced, TacticalReason.Stalled)]
        [TestCase(RecoveryCause.ProgressDeficit, TacticalReason.NoProgress)]
        [TestCase(RecoveryCause.ProgressDeficit, TacticalReason.Resumed)]
        public void TheLastFailedAttemptFaultsBeforeALegitimateWaitCanResetTheBudget(RecoveryCause cause, TacticalReason outcome)
        {
            var free = FreeDecision();
            var supervisor = cause == RecoveryCause.Displaced ? Displaced() : new RecoverySupervisor();
            if (cause == RecoveryCause.ProgressDeficit)
                for (int i = 0; i < 200; i++) supervisor.Observe(Nominal(free, BlockerTracker.Empty, 0f, 10f));
            Assert.That(supervisor.Cause, Is.EqualTo(cause));
            RecoveryRequest last = null;
            for (int i = 1; i <= TrafficV2Settings.RecoveryMaxAttempts; i++)
            {
                last = supervisor.TryRequest(Frame + (ulong)i, Agent);
                Assert.That(last, Is.Not.Null);
                supervisor.Record(last, new TacticalResponse(last.Version, TacticalReason.Accepted), Frame);
                Assert.That(supervisor.Faulted, Is.False, "la derniere tentative acceptee peut encore reussir");
                if (i < TrafficV2Settings.RecoveryMaxAttempts)
                    supervisor.Observe(Observation(goal: true, awaiting: true, outcomeVersion: last.Version, outcome: TacticalReason.Stalled));
            }
            var blockers = new[] { BlockerRules.Leader(Other, 5f, Driver.MinimumGap, -1f, Frame) };
            supervisor.Observe(new RecoveryObservation(Frame + 5UL, false, false, false, true, free, blockers, 0f,
                Driver.DesiredSpeed, 0, 10f, Dt, false, last.Version, outcome));
            bool failed = outcome != TacticalReason.Resumed;
            Assert.That(supervisor.TryRequest(Frame + 6UL, Agent), Is.Null, "aucune requete pendant l'attente legitime");
            Assert.That(supervisor.Faulted, Is.EqualTo(failed), "un blocker legitime ne masque pas le dernier echec");
            if (failed)
            {
                Assert.That(supervisor.FaultReason, Is.EqualTo("AttemptsExhausted"));
                Assert.That(supervisor.FaultedAtFrame, Is.EqualTo(Frame + 6UL));
            }
            for (int i = 0; i < 100; i++) supervisor.Observe(Nominal(free, BlockerTracker.Empty, 1f, 10f + i * Dt));
            Assert.That(supervisor.Faulted, Is.EqualTo(failed));
            Assert.That(supervisor.EpisodeAttempts, Is.EqualTo(failed ? TrafficV2Settings.RecoveryMaxAttempts : 0),
                "la progression ne ressuscite pas un vehicule Faulted ; une derniere reussite ferme l'episode");
        }

        [Test]
        public void ALastRejectionExhaustsTheBudgetEvenWithoutTwoConsecutiveRejections()
        {
            var supervisor = Displaced();
            for (int i = 1; i <= TrafficV2Settings.RecoveryMaxAttempts; i++)
            {
                var request = supervisor.TryRequest(Frame + (ulong)i, Agent);
                Assert.That(request, Is.Not.Null);
                bool accepted = i % 2 == 1;
                supervisor.Record(request, new TacticalResponse(request.Version,
                    accepted ? TacticalReason.Accepted : TacticalReason.RearBlocked), Frame);
                if (accepted) supervisor.Observe(Observation(goal: true, awaiting: true, outcomeVersion: request.Version,
                    outcome: TacticalReason.Stalled));
            }
            supervisor.Observe(Observation());
            Assert.That(supervisor.TryRequest(Frame + 5UL, Agent), Is.Null);
            Assert.That(supervisor.Faulted, Is.True, "la quatrieme tentative refusee epuise le budget");
            Assert.That(supervisor.FaultReason, Is.EqualTo("AttemptsExhausted"));
            Assert.That(supervisor.TryRequest(Frame + 5UL, Agent), Is.Null);
        }

        // ------------------------------------------------------------------ R3 tactique

        [Test]
        public void TacticalAcceptsOrRejectsEachRecoveryRequestWithItsReason()
        {
            Assert.That(new TacticalDecision().SubmitRecovery(null, Frame, StabilityBlocker.None, true, true).Reason,
                Is.EqualTo(TacticalReason.InvalidRequest));
            Assert.That(new TacticalDecision().SubmitRecovery(Request(RecoveryManeuver.Realign, cause: RecoveryCause.None), Frame,
                StabilityBlocker.None, true, true).Reason, Is.EqualTo(TacticalReason.InvalidRequest));
            Assert.That(new TacticalDecision().SubmitRecovery(Request(RecoveryManeuver.Realign, frame: Frame - 1UL), Frame,
                StabilityBlocker.None, true, true).Reason, Is.EqualTo(TacticalReason.StaleRequest));
            Assert.That(new TacticalDecision().SubmitRecovery(Request(RecoveryManeuver.Realign), Frame, StabilityBlocker.YawRate, true,
                true).Reason, Is.EqualTo(TacticalReason.Unstable));
            Assert.That(new TacticalDecision().SubmitRecovery(Request(RecoveryManeuver.Realign), Frame, StabilityBlocker.None, false,
                true).Reason, Is.EqualTo(TacticalReason.NoReference));
            Assert.That(new TacticalDecision().SubmitRecovery(Request(RecoveryManeuver.Reverse), Frame, StabilityBlocker.None, true,
                false).Reason, Is.EqualTo(TacticalReason.RearBlocked));

            var braking = CollisionAccepted();
            Assert.That(braking.SubmitRecovery(Request(RecoveryManeuver.Realign), Frame, StabilityBlocker.None, true, true).Reason,
                Is.EqualTo(TacticalReason.GoalAlreadyActive), "collision encore en freinage");
            Assert.That(braking.LastReason, Is.EqualTo(TacticalReason.Accepted), "le refus ne masque pas la raison du but");

            var active = Realigning();
            Assert.That(active.SubmitRecovery(Request(RecoveryManeuver.Reverse, version: 2UL), Frame, StabilityBlocker.None, true,
                true).Reason, Is.EqualTo(TacticalReason.GoalAlreadyActive));
            Assert.That(active.SubmitRecovery(Request(RecoveryManeuver.Reverse, version: 1UL), Frame, StabilityBlocker.None, true,
                true).Reason, Is.EqualTo(TacticalReason.InvalidRequest), "version non croissante");
            Assert.That(active.Maneuver, Is.EqualTo(RecoveryManeuver.Realign));
        }

        [Test]
        public void AcceptingFromAwaitingRecoveryTransfersTheGoalAndARealignResumesOnlyStableInsideEpsilon()
        {
            var tactical = CollisionAccepted();
            for (int i = 0; i < 100; i++) tactical.Update(Facts(displacement: 2f), Dt, Tol, false);
            Assert.That(tactical.AwaitingRecovery, Is.True);
            var response = tactical.SubmitRecovery(Request(RecoveryManeuver.Realign), Frame, StabilityBlocker.None, true, true);
            Assert.That(response.Accepted, Is.True);
            Assert.That(tactical.Goal, Is.EqualTo(TacticalGoalKind.Recovery));
            Assert.That(tactical.CollisionActive, Is.False);
            Assert.That(tactical.Active, Is.True);

            // Hors enveloppe ou instable : jamais Resumed.
            foreach (var facts in new[] { Facts(displacement: 1f), Facts(displacement: 0f, yaw: 0.31f), Facts(displacement: 0f, angular: 0.51f),
                Facts(displacement: 0f, grounded: 3), Facts(contact: true, relative: 6f) })
            {
                var realign = Realigning();
                for (int i = 0; i < 200; i++) realign.Update(facts, Dt, Tol, false, Moving(2f));
                Assert.That(realign.RecoveryActive, Is.True);
            }
            int steps = 0;
            while (tactical.RecoveryActive && steps < 200) { tactical.Update(Facts(displacement: Tol), Dt, Tol, false, Moving(2f)); steps++; }
            Assert.That(tactical.LastReason, Is.EqualTo(TacticalReason.Resumed));
            Assert.That(steps, Is.InRange(25, 26), "0,5 s stable dans l'enveloppe");
            Assert.That(tactical.RecoveryOutcomeVersion, Is.EqualTo(1UL));
            Assert.That(tactical.RecoveryOutcome, Is.EqualTo(TacticalReason.Resumed));
            Assert.That(tactical.Goal, Is.EqualTo(TacticalGoalKind.Nominal));
        }

        [Test]
        public void ManeuversEndByNoProgressStalledCompletionCollisionOrExit()
        {
            var lost = Realigning();
            float travel = 0f;
            while (lost.RecoveryActive && travel < 100f) { lost.Update(Facts(displacement: 3f), Dt, Tol, false, Moving(2f)); travel += 2f * Dt; }
            Assert.That(lost.RecoveryOutcome, Is.EqualTo(TacticalReason.NoProgress));
            Assert.That(travel, Is.EqualTo(TrafficV2Settings.RecoveryRealignMaxTravelMeters).Within(2f * Dt + 1e-3f));

            foreach (var maneuver in new[] { RecoveryManeuver.Realign, RecoveryManeuver.Reverse })
            {
                var stuck = maneuver == RecoveryManeuver.Realign ? Realigning() : Reversing();
                int steps = 0;
                while (stuck.RecoveryActive && steps < 1000) { stuck.Update(Facts(displacement: 3f), Dt, Tol, false, new RecoveryMotion(0f, 0f, 2f)); steps++; }
                Assert.That(stuck.RecoveryOutcome, Is.EqualTo(TacticalReason.Stalled), maneuver.ToString());
                Assert.That(steps, Is.GreaterThan(50), "E doit atteindre 2 m : " + maneuver);
            }

            var reverse = Reversing();
            float back = 0f;
            while (reverse.RecoveryActive && back < 10f) { reverse.Update(Facts(displacement: 3f), Dt, Tol, false, Moving(-1f)); back += Dt; }
            Assert.That(reverse.RecoveryOutcome, Is.EqualTo(TacticalReason.ManeuverCompleted));
            Assert.That(back, Is.EqualTo(TrafficV2Settings.RecoveryReverseTravelMeters).Within(Dt + 1e-3f));

            var preempted = Realigning();
            var collision = preempted.Submit(new CollisionResponseRequest(1UL, Frame, Agent, CollisionSignificance.RelativeSpeed,
                Facts(contact: true, relative: 6f)), Frame, false, new RouteSeed(539UL), CollisionReactionWeights.Default, 0f);
            Assert.That(collision.Accepted, Is.True, "la physique d'abord");
            Assert.That(preempted.CollisionActive, Is.True);
            Assert.That(preempted.RecoveryOutcome, Is.EqualTo(TacticalReason.CollisionPreempted));

            var exiting = Reversing();
            exiting.Update(Facts(), Dt, Tol, true, Moving(-1f));
            Assert.That(exiting.RecoveryOutcome, Is.EqualTo(TacticalReason.ExitPortalReached));
            Assert.That(exiting.Active, Is.False);
        }

        // ------------------------------------------------------------------ R5 commandes au vrai composeur

        [Test]
        public void RealignPropelsTowardTheCreepSpeedAndReverseNeverUsesThrottleThroughTheRealComposer()
        {
            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath).Profile;
            var composer = new VehicleDriveIntentComposer(vehicle, Driver.SafeBrakingLimit, Dt);
            float vDir = vehicle.MinimumDirectionSpeed;

            var realign = Realigning();
            var go = realign.CommandFor(Frame, 1, BMax, BComfort, Lock, new RecoveryCommandInput(0f, Dt, 2f, 45f));
            Assert.That(go.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(2f), "borne a a_max");
            Assert.That(go.TargetWheelAngleDegrees, Is.EqualTo(Lock), "borne au verrou");
            Assert.That(composer.Compose(Frame, go, V2FallbackReason.None, 0f, 0.3f).Intent.Throttle, Is.GreaterThan(0f));
            var cruise = realign.CommandFor(Frame, 1, BMax, BComfort, Lock, new RecoveryCommandInput(3f, Dt, 2f, -5f));
            Assert.That(cruise.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BComfort), "au-dessus de v_rec : freine");
            Assert.That(cruise.TargetWheelAngleDegrees, Is.EqualTo(-5f));

            var reverse = Reversing();
            foreach (float speed in new[] { 0f, vDir * 0.5f, -0.5f, -1f, -1.5f, vDir + 0.05f, 1f, 5f })
                foreach (float damping in new[] { 0f, 0.3f })
                {
                    var command = reverse.CommandFor(Frame, 1, BMax, BComfort, Lock, new RecoveryCommandInput(speed, Dt, 2f, 30f));
                    Assert.That(command.TargetWheelAngleDegrees, Is.EqualTo(0f), "roues droites");
                    var composed = composer.Compose(Frame, command, V2FallbackReason.None, speed, damping, reverse: true);
                    string label = speed + " m/s, damping " + damping;
                    Assert.That(composed.Fallback, Is.False, label);
                    Assert.That(composed.Intent.Throttle, Is.EqualTo(0f), "jamais de gaz : " + label);
                    Assert.That(composed.Intent.Handbrake, Is.EqualTo(0f), label);
                    float torque = VehicleDriveIntentComposer.MinimumWheelDriveTorque(vehicle, composed.Intent, composed.MaxForwardSpeed, speed);
                    if (speed > vDir)
                    {
                        Assert.That(torque, Is.EqualTo(0f), "roule en avant : aucun couple moteur : " + label);
                        Assert.That(composed.Intent.BrakeReverse, Is.GreaterThan(0f), "frein de service demande : " + label);
                        float brakeTorque = VehicleTireModel.ResolveWheelBrakeTorque(composed.Intent.Throttle,
                            composed.Intent.BrakeReverse, speed, vDir, vehicle.BrakeTorque, 0f);
                        Assert.That(brakeTorque, Is.GreaterThan(0f), "freinage effectif, sans compter le frein moteur : " + label);
                    }
                    if (speed <= vDir && speed > -TrafficV2Settings.RecoveryReverseSpeedMetersPerSecond)
                        Assert.That(torque, Is.LessThan(0f), "marche arriere engagee : " + label);
                    if (speed <= -TrafficV2Settings.RecoveryReverseSpeedMetersPerSecond)
                        Assert.That(composed.Intent.BrakeReverse, Is.EqualTo(0f), "consigne atteinte : roue libre : " + label);
                }

            // Defaut inchange au bit pres (5.37/5.38).
            foreach (var command in new[] { new MotionCommand(Frame, Frame, Frame, 1.2f, 4f), new MotionCommand(Frame, Frame, Frame, -3f, 0f) })
                foreach (float speed in new[] { 0f, 0.2f, 5f })
                {
                    var before = new VehicleDriveIntentComposer(vehicle, Driver.SafeBrakingLimit, Dt).Compose(Frame, command,
                        V2FallbackReason.None, speed, 0.3f);
                    var after = new VehicleDriveIntentComposer(vehicle, Driver.SafeBrakingLimit, Dt).Compose(Frame, command,
                        V2FallbackReason.None, speed, 0.3f, false, true, false);
                    Assert.That(after.Intent.Throttle, Is.EqualTo(before.Intent.Throttle));
                    Assert.That(after.Intent.BrakeReverse, Is.EqualTo(before.Intent.BrakeReverse));
                    Assert.That(after.Intent.Handbrake, Is.EqualTo(before.Intent.Handbrake));
                    Assert.That(after.Intent.Steer, Is.EqualTo(before.Intent.Steer));
                }
            var faulted = composer.Compose(Frame, null, V2FallbackReason.Faulted, 0f, 0f);
            Assert.That(faulted.Fallback, Is.True);
            Assert.That(faulted.Reason, Is.EqualTo(V2FallbackReason.Faulted));
            Assert.That(faulted.Intent.Handbrake, Is.EqualTo(1f), "repli tenu");
        }

        [Test]
        public void ManeuverCommandsCrossTheSafetyFilterAndTheRealignLawIsTheTrackingLaw()
        {
            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath).Profile;
            var model = Admission.Model;
            var limits = SafetyLimits.For(vehicle, model.DrivabilityProfile, Dt, TrafficV2Settings.PlanValiditySteps);
            var bench = Straight;
            var frame = new TrafficFrame(Frame, model, new[] { ActorAt(Agent, bench, 20f, 0f) }, null);
            foreach (var tactical in new[] { Realigning(), Reversing() })
            {
                var command = tactical.CommandFor(Frame, 1, limits.MaxBrakingDecelerationMetersPerSecondSquared, BComfort,
                    model.DrivabilityProfile.LowSpeedLockDegrees, new RecoveryCommandInput(0f, Dt, Driver.MaxAcceleration, 10f));
                var verdict = SafetyFilter.Evaluate(command, Frame, frame, Agent, 0f, null, null, limits);
                Assert.That(verdict.Verdict, Is.Not.EqualTo(SafetyVerdict.Reject), tactical.Maneuver + " " + verdict.Reason);
            }

            // Meme loi d'angle que le suivi nominal : Track est inchange.
            var piece = bench.Track.Pieces[0];
            var nominal = bench.Track.Nominal(0, 5f);
            Vector3 right = Vector3.Cross(nominal.Up, nominal.Forward).normalized;
            Vector3 position = nominal.Position + right * 1.5f;
            Vector3 forward = Quaternion.AngleAxis(25f, nominal.Up) * nominal.Forward;
            var plan = Plan(2f);
            var track = MotionCommand.Track(Frame, 1, plan, Driver, model.DrivabilityProfile, piece.Curve, piece.ElementS(5f), position,
                forward, 2f, Dt, -3f);
            float lateral, heading, curvature, s;
            float angle = MotionCommand.TrackingWheelAngleDegrees(model.DrivabilityProfile, piece.Curve, piece.ElementS(5f), position,
                forward, 2f, -3f, out lateral, out heading, out curvature, out s);
            Assert.That(angle, Is.EqualTo(track.TargetWheelAngleDegrees));
            Assert.That(lateral, Is.EqualTo(track.LateralErrorMeters));
            Assert.That(s, Is.EqualTo(track.ReferenceSMeters));
            Assert.That(Math.Sign(angle), Is.EqualTo(-1), "deplace a droite, cap a droite : braque a gauche, vers la reference");
        }

        [Test]
        public void TheRearSweepSeesFrameActorsBehindOnly()
        {
            var bench = Straight;
            var model = Admission.Model;
            Func<float, TrafficFrame> with = otherDistance => new TrafficFrame(Frame, model, new[] { ActorAt(Agent, bench, 20f, 0f),
                ActorAt(Other, bench, otherDistance, 0f) }, null);
            float reach = 2.22f + 2.22f + TrafficV2Settings.RecoveryReverseTravelMeters;
            Assert.That(RecoverySupervisor.RearSweepClear(with(20f - reach + 0.3f), Agent, TrafficV2Settings.RecoveryReverseTravelMeters),
                Is.False, "acteur dans le balayage");
            Assert.That(RecoverySupervisor.RearSweepClear(with(20f - reach - 0.3f), Agent, TrafficV2Settings.RecoveryReverseTravelMeters),
                Is.True, "acteur au-dela du recul");
            Assert.That(RecoverySupervisor.RearSweepClear(with(30f), Agent, TrafficV2Settings.RecoveryReverseTravelMeters), Is.True,
                "acteur devant");
            Assert.That(RecoverySupervisor.RearSweepClear(null, Agent, 3f), Is.False, "sans frame : refus");
        }

        [Test]
        public void TheRearSweepIncludesRotatedCornersAndFootprintsWhoseReferenceIsAhead()
        {
            var bench = Straight;
            var self = ActorAt(Agent, bench, 20f, 0f);
            var pose = self.Pose;
            Vector3 right = Vector3.Cross(pose.Up, pose.Forward).normalized;
            Func<float, float, float, VehicleFootprint, bool> clear = (along, across, yaw, footprint) =>
            {
                var otherPose = new VehicleFootprintPose
                {
                    Position = pose.Position + pose.Forward * along + right * across,
                    Forward = Quaternion.AngleAxis(yaw, pose.Up) * pose.Forward, Up = pose.Up, Footprint = footprint
                };
                var frame = new TrafficFrame(Frame, Admission.Model, new[] { self,
                    new TrafficActorInput(Other, otherPose, 0f, bench.Ids[0]) });
                return RecoverySupervisor.RearSweepClear(frame, Agent, TrafficV2Settings.RecoveryReverseTravelMeters);
            };
            Assert.That(clear(-7.60f, 0f, 25f, Car), Is.False, "coin tourne dans le bout du balayage");
            Assert.That(clear(-7.85f, 0f, 25f, Car), Is.True, "coin tourne au-dela du balayage");
            Assert.That(clear(-3.8f, 3.3f, 65f, Car), Is.False, "coin tourne dans le cote du balayage");
            Assert.That(clear(-3.8f, 3.6f, 65f, Car), Is.True, "empreinte lateralement separee");
            var longRear = Car;
            longRear.RearMeters = 5f;
            Assert.That(clear(0.2f, 0f, 0f, longRear), Is.False, "reference devant, empreinte dans le recul");
            Assert.That(clear(0.2f, 0f, 0f, Car), Is.True, "acteur devant sans empreinte dans le recul");
        }

        // ------------------------------------------------------------------ structure et branchement

        [Test]
        public void RecoveryNeverWritesTheBodyComposesIntentGrantsOrRemovesAndNeverSetsTheGoal()
        {
            var forbidden = new Regex(@"(\b(MovePosition|MoveRotation|AddForce|AddTorque|RecoverAtWaypoint|Teleport\w*|ApplyDriveIntent|"
                + @"ApplyMovement|Rigidbody|Despawn|Destroy|Reinsert\w*|VehicleDriveIntent\w*|Compose|JunctionCoordinator|Grant\w*|"
                + @"TrafficRule\w*|SafetyFilter|Waypoint\w*|Coroutine|WaitForSeconds|Time\.(time|deltaTime|realtimeSinceStartup)|Random|"
                + @"Submit\w*|Rage|Fear)\b|\.(position|rotation|linearVelocity|velocity|angularVelocity)\s*=)");
            var files = Directory.GetFiles(RecoveryFolder, "*.cs");
            Assert.That(files, Is.Not.Empty);
            foreach (var file in files)
            {
                var match = forbidden.Match(File.ReadAllText(file));
                Assert.That(match.Success, Is.False, file + " : " + match.Value);
            }
            foreach (var property in typeof(RecoverySupervisor).GetProperties())
                if (property.SetMethod != null) Assert.That(property.SetMethod.IsPublic, Is.False, property.Name);
            foreach (var property in typeof(TacticalDecision).GetProperties())
                if (property.SetMethod != null) Assert.That(property.SetMethod.IsPublic, Is.False, property.Name);
        }

        [Test]
        public void TheDriverSubmitsCollisionThenRecoveryAndObservesAfterComposition()
        {
            string source = File.ReadAllText(DriverPath);
            string step = Body(source, "internal void Step(");
            string[] order = { "collisionAnalysis.Analyze(", "tactical.Submit(", "recovery.TryRequest(", "tactical.SubmitRecovery(",
                "recovery.Record(", "tactical.Update(", "PlanningSpine.Evaluate(", "tactical.CommandFor(", "SafetyFilter.Evaluate(",
                "composer.Compose(", "BlockerTracker.Update(", "recovery.Observe(" };
            int last = -1;
            foreach (var call in order)
            {
                int index = step.IndexOf(call, StringComparison.Ordinal);
                Assert.That(index, Is.GreaterThan(last), call);
                Assert.That(step.IndexOf(call, index + 1, StringComparison.Ordinal), Is.EqualTo(-1), "un seul appel : " + call);
                last = index;
            }
            StringAssert.Contains("if (located && !toleranceResponse.Latched && !faulted)", step);
            StringAssert.Contains("refusal = V2FallbackReason.Faulted;", step);
            StringAssert.Contains("reverse: tactical.Reversing", step);
            StringAssert.Contains("if (!tacticalGoal) longitudinal = LongitudinalArbitration.Decide(", step);
            StringAssert.Contains("WithFallback(composed.Fallback || tacticalGoal)", step);
            StringAssert.Contains("toleranceResponse.Release()", step);
            Assert.That(Regex.IsMatch(step, @"\.(position|rotation|linearVelocity|velocity|angularVelocity)\s*="), Is.False);
            Assert.That(Regex.IsMatch(source, @"\b(Despawn|Destroy|MovePosition|MoveRotation)\b"), Is.False, "le driver ne retire ni ne deplace");
            StringAssert.Contains("CollisionPredicates.SuspendsToleranceLatch(tactical.Active, preparedFacts)", Body(source, "private bool PrepareStep("));
            // Le spawner ne retire un V2 qu'au portail de sortie : un vehicule Faulted reste present.
            string spawner = File.ReadAllText("Assets/RoadRage/App/Run/PortalTrafficSpawner.cs");
            StringAssert.Contains("(v2Driver == null || !v2Driver.HasReachedExitPortal)", spawner);
            Assert.That(Regex.IsMatch(spawner, @"\b(Faulted|RecoverySupervisor|TrafficV2LifecycleState)\b"), Is.False, "aucun retrait sur Faulted");
        }

        // ------------------------------------------------------------------ outils

        private static TrafficV2Admission admission;
        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                        File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
                Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
                return admission;
            }
        }

        private static DriverProfile Driver { get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; } }

        private sealed class Bench
        {
            public RoutePlan Route;
            public ReferenceTrack Track;
            public RoadId[] Ids;
        }

        private static Bench straightBench;

        /// <summary>Plus longue ligne droite de MVP_Run (corridor, mouvement, corridor), comme le banc 5.33.</summary>
        private static Bench Straight
        {
            get
            {
                if (straightBench != null) return straightBench;
                var model = Admission.Model;
                CompiledJunctionMovement best = default(CompiledJunctionMovement);
                float bestLength = 0f;
                foreach (var movement in model.Movements.OrderBy(m => m.Id))
                {
                    EffectiveLaneCorridor from, to;
                    if (!IsStraight(movement.Samples) || !model.TryGetCorridor(movement.FromCorridorId, out from)
                        || !model.TryGetCorridor(movement.ToCorridorId, out to) || !IsStraight(from.Samples) || !IsStraight(to.Samples))
                        continue;
                    float length = from.LengthMeters + movement.LengthMeters + to.LengthMeters;
                    if (length > bestLength + 1e-3f) { best = movement; bestLength = length; }
                }
                Assert.That(bestLength, Is.GreaterThan(40f));
                var start = new RoadLocation { ModelId = model.ModelId, ModelVersion = model.Version, Localized = true,
                    ElementKind = RoadElementKind.LaneCorridor, ElementId = best.FromCorridorId, SMeters = 0f };
                var result = RoutePlanner.Plan(new RouteRequest(model, start, RoadId.None, new RouteSeed(0), Agent, "route",
                    new DecisionCounter(0), null, false, null, best.Id));
                Assert.That(result.Plan, Is.Not.Null, result.Reason.ToString());
                straightBench = new Bench { Route = result.Plan, Track = ReferenceTrack.FromRoute(model, result.Plan.Occurrences, 0f),
                    Ids = result.Plan.Occurrences.Select(o => o.Id).ToArray() };
                return straightBench;
            }
        }

        private static bool IsStraight(IReadOnlyList<RoadCurveSample> samples)
        {
            for (int i = 0; i < samples.Count; i++) if (Math.Abs(samples[i].CurvaturePerMeter) > 1e-4f) return false;
            return true;
        }

        private static TrafficActorInput ActorAt(RoadId id, Bench bench, float distance, float speed)
        {
            int piece = bench.Track.PieceAt(distance);
            var nominal = bench.Track.Nominal(piece, distance);
            var pose = new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward, Up = nominal.Up, Footprint = Car };
            return new TrafficActorInput(id, pose, speed, bench.Track.Pieces[piece].Id, bench.Ids, bench.Track.KinematicAnchors(piece));
        }

        /// <summary>Plan accepte a 5 m du debut de la ligne droite, a la vitesse donnee, sans interaction.</summary>
        private static SpeedPlan Plan(float speed)
        {
            var bench = Straight;
            var model = Admission.Model;
            var frame = new TrafficFrame(1, model, new[] { ActorAt(Agent, bench, 5f, speed) }, null);
            int piece = bench.Track.PieceAt(5f);
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, Agent, bench.Route, bench.Route.ExitPortalId, new RouteSeed(0),
                TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null, null,
                Admission.Evidence, RoadId.None, bench.Track.OffsetRadians(piece, 5f)));
            Assert.That(decision.Motion, Is.Not.Null, decision.Projection.Code);
            var plan = SpeedPlan.Build(decision.Motion, model, Driver, speed);
            Assert.That(plan.Accepted, Is.True);
            return plan;
        }

        private static LongitudinalDecision Decide(SpeedPlan plan, float speed, LongitudinalPerception perception, LongitudinalMemory memory)
        {
            return LongitudinalArbitration.Decide(plan, Driver, speed, Dt, perception, memory, TrafficV2Settings.StopHold);
        }

        private static LongitudinalDecision FreeDecision()
        {
            var decision = Decide(Plan(0f), 0f, new LongitudinalPerception(PerceptionUnavailableReason.None, null, null), LongitudinalMemory.None);
            Assert.That(decision.Binding.Kind == LongitudinalCandidateKind.Profile || decision.Binding.Kind == LongitudinalCandidateKind.DesiredSpeed,
                Is.True, decision.ToText());
            return decision;
        }

        private static LongitudinalPerception LeaderOnly(float gap, float leaderSpeed)
        {
            return new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(Other, gap, leaderSpeed), null);
        }

        private static RecoveryObservation Nominal(LongitudinalDecision decision, IReadOnlyList<Blocker> blockers, float speed, float distance)
        {
            return new RecoveryObservation(Frame, false, false, false, true, decision, blockers, speed, Driver.DesiredSpeed, 0, distance, Dt,
                false, 0UL, TacticalReason.None);
        }

        private static RecoveryObservation Observation(bool goal = false, bool awaiting = false, bool latchedHeld = false, bool exit = false,
            ulong outcomeVersion = 0UL, TacticalReason outcome = TacticalReason.None)
        {
            return new RecoveryObservation(Frame, goal, awaiting, latchedHeld, false, null, BlockerTracker.Empty, 0f, Driver.DesiredSpeed, 0,
                10f, Dt, exit, outcomeVersion, outcome);
        }

        private static RecoverySupervisor Displaced()
        {
            var supervisor = new RecoverySupervisor();
            supervisor.Observe(Observation(goal: true, awaiting: true));
            Assert.That(supervisor.Cause, Is.EqualTo(RecoveryCause.Displaced));
            return supervisor;
        }

        private static CollisionFacts Facts(bool contact = false, float relative = 0f, float yaw = 0f, float angular = 0f, int grounded = 4,
            float displacement = 0f)
        {
            return new CollisionFacts(1UL, contact, 0f, relative, contact ? Dt : 0f, 0, yaw, angular, grounded, 4, displacement, Mass);
        }

        private static RecoveryRequest Request(RecoveryManeuver maneuver, ulong version = 1UL, ulong frame = Frame,
            RecoveryCause cause = RecoveryCause.Displaced)
        {
            return new RecoveryRequest(version, frame, Agent, cause, maneuver, 1);
        }

        private static TacticalDecision CollisionAccepted()
        {
            var tactical = new TacticalDecision();
            var weights = new CollisionReactionWeights(1f, 0f, 0f, 0.8f, 0.6f);
            Assert.That(tactical.Submit(new CollisionResponseRequest(1UL, Frame, Agent, CollisionSignificance.RelativeSpeed,
                Facts(contact: true, relative: 6f)), Frame, false, new RouteSeed(539UL), weights, 0f).Accepted, Is.True);
            return tactical;
        }

        private static TacticalDecision Realigning() { return Maneuvering(RecoveryManeuver.Realign); }
        private static TacticalDecision Reversing() { return Maneuvering(RecoveryManeuver.Reverse); }

        private static TacticalDecision Maneuvering(RecoveryManeuver maneuver)
        {
            var tactical = new TacticalDecision();
            Assert.That(tactical.SubmitRecovery(Request(maneuver), Frame, StabilityBlocker.None, true, true).Accepted, Is.True);
            Assert.That(tactical.Maneuver, Is.EqualTo(maneuver));
            return tactical;
        }

        /// <summary>Mouvement d'un pas a vitesse constante : parcours |v| dt.</summary>
        private static RecoveryMotion Moving(float speed) { return new RecoveryMotion(speed, Math.Abs(speed) * Dt, 2f); }

        private static string Body(string source, string signature)
        {
            int start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), signature);
            int open = source.IndexOf('{', start), depth = 0;
            for (int i = open; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}' && --depth == 0) return source.Substring(start, i - start + 1);
            }
            return source.Substring(start);
        }
    }
}
