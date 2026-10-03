using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.33 : arbitrage longitudinal pur (candidats nommes, liante, ordre d'egalite, reprise lissee D5), blockers,
    /// limite de route appliquee et projection etendue. Cas noyau 5.9 rejoues contre l'arbitrage ; bancs point-masse sur
    /// la plus longue ligne droite de MVP_Run, jeu mesure par la perception 5.32 depuis des frames reelles.
    /// </summary>
    [Category("Core")]
    [Category("Story533")]
    public sealed class Story533LongitudinalTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string TrafficRootPath = "Assets/RoadRage/Features/Vehicles/Traffic";
        private const float Dt = 0.02f;
        private const float EditModeTolerance = 0.05f;

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };

        private static readonly RoadId Agent = new RoadId(0x533UL, 1UL);
        private static readonly RoadId Other = new RoadId(0x533UL, 2UL);
        private static readonly RoadId HazardId = new RoadId(0x4841UL, 1UL);

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

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; }
        }

        /// <summary>Profil de reference de la Story 5.9 (aligne sur DriverProfileDef_Default.asset).</summary>
        private static DriverProfile Baseline(float desiredSpeed = 8f, float reactionTime = 0.3f)
        {
            return new DriverProfile(desiredSpeed, 1.5f, 2f, 1.5f, 2f, 0.25f, 0.2f, 4f, reactionTime, 1f, 0.8f);
        }

        // ================================================================== banc de route

        /// <summary>
        /// Route qui traverse la plus longue ligne droite de MVP_Run (corridor, mouvement tout droit, corridor : 48 m),
        /// trouvee par le modele, depart a s = 0 du corridor amont.
        /// </summary>
        private sealed class Bench
        {
            public RoutePlan Route;
            public ReferenceTrack Track;
            public RoadId[] Ids;
            public float StraightLengthMeters;
        }

        private static Bench straightBench;

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
                Assert.That(bestLength, Is.GreaterThan(40f), "ligne droite de reference de MVP_Run");
                var start = new RoadLocation { ModelId = model.ModelId, ModelVersion = model.Version, Localized = true,
                    ElementKind = RoadElementKind.LaneCorridor, ElementId = best.FromCorridorId, SMeters = 0f };
                var result = RoutePlanner.Plan(new RouteRequest(model, start, RoadId.None, new RouteSeed(0), Agent, "route",
                    new DecisionCounter(0), null, false, null, best.Id));
                Assert.That(result.Plan, Is.Not.Null, result.Reason.ToString());
                straightBench = new Bench { Route = result.Plan, StraightLengthMeters = bestLength,
                    Track = ReferenceTrack.FromRoute(model, result.Plan.Occurrences, 0f),
                    Ids = result.Plan.Occurrences.Select(o => o.Id).ToArray() };
                Assert.That(straightBench.Track.Pieces[0].Id, Is.EqualTo(best.FromCorridorId));
                return straightBench;
            }
        }

        private static bool IsStraight(IReadOnlyList<RoadCurveSample> samples)
        {
            for (int i = 0; i < samples.Count; i++) if (Math.Abs(samples[i].CurvaturePerMeter) > 1e-4f) return false;
            return true;
        }

        private static TrafficActorInput ActorAt(RoadId id, Bench bench, float distance, float speed, bool declared = true)
        {
            int piece = bench.Track.PieceAt(distance);
            var nominal = bench.Track.Nominal(piece, distance);
            var pose = new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward, Up = nominal.Up,
                Footprint = declared ? Car : default(VehicleFootprint) };
            return new TrafficActorInput(id, pose, speed, bench.Track.Pieces[piece].Id, bench.Ids,
                bench.Track.KinematicAnchors(piece));
        }

        /// <summary>Danger aligne monde centre sur la reference a la distance de route, a hauteur de caisse.</summary>
        private static TrafficHazardInput BoxAt(RoadId id, Bench bench, float distance, float lateral, Vector3 velocity,
            TrafficHazardKind kind = TrafficHazardKind.Obstacle)
        {
            int piece = bench.Track.PieceAt(distance);
            var nominal = bench.Track.Pieces[piece].Nominal(distance);
            var right = Vector3.Cross(nominal.Up, nominal.Forward).normalized;
            return new TrafficHazardInput(id, kind, new RoadBoundsBox { Center = nominal.Position + right * lateral + nominal.Up * 0.75f,
                Extents = new Vector3(0.5f, 0.75f, 0.5f) }, velocity, 1f);
        }

        private sealed class Step
        {
            public TrafficFrame Frame;
            public PlanningDecision Decision;
            public AgentObservation Observation;
            public SpeedPlan Plan;
            public LongitudinalPerception Perception;
            public LongitudinalDecision Longitudinal;
        }

        private static Step Evaluate(Bench bench, RoutePlan route, ulong frameId, IReadOnlyList<TrafficActorInput> actors,
            IReadOnlyList<TrafficHazardInput> hazards, float agentDistance, float speed, LongitudinalMemory memory,
            DriverProfile driver, bool collectorSaturated = false, int spatialCapacity = TrafficV2Settings.SpatialQueryCapacity)
        {
            var model = Admission.Model;
            var step = new Step { Frame = new TrafficFrame(frameId, model, actors, hazards) };
            int piece = bench.Track.PieceAt(agentDistance);
            step.Decision = PlanningSpine.Evaluate(new PlanningRequest(step.Frame, Agent, route, route.ExitPortalId, new RouteSeed(0),
                TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared, null, null,
                Admission.Evidence, RoadId.None, bench.Track.OffsetRadians(piece, agentDistance)));
            Assert.That(step.Decision.Motion, Is.Not.Null, step.Decision.Projection.Code);
            step.Observation = TrafficPerception.Observe(step.Frame, Agent, step.Decision.Path, TrafficV2Settings.PerceptionLimits,
                new SpatialQueryBuffer(spatialCapacity));
            step.Plan = SpeedPlan.Build(step.Decision.Motion, model, driver, speed);
            Assert.That(step.Plan.Accepted, Is.True, step.Plan.Verification.Issue.ToString());
            step.Perception = LongitudinalPerception.From(step.Observation, step.Decision.Path,
                LongitudinalPerception.FrontDistanceMeters(step.Frame, Agent, step.Decision.Path), collectorSaturated);
            step.Longitudinal = Decide(step.Plan, driver, speed, Dt, step.Perception, memory);
            return step;
        }

        /// <summary>Plan accepte au debut de la ligne droite, a une vitesse donnee (aucune interaction).</summary>
        private static SpeedPlan PlanAt(float distance, float speed, DriverProfile driver)
        {
            var bench = Straight;
            return Evaluate(bench, bench.Route, 1, new[] { ActorAt(Agent, bench, distance, speed) }, null, distance, speed,
                LongitudinalMemory.None, driver).Plan;
        }

        private static LongitudinalPerception Nothing()
        {
            return new LongitudinalPerception(PerceptionUnavailableReason.None, null, null);
        }

        private static LongitudinalPerception LeaderOnly(float gap, float leaderSpeed)
        {
            return new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(Other, gap, leaderSpeed), null);
        }

        /// <summary>Arbitrage avec le maintien a l'arret des reglages runtime (D11), comme le driver.</summary>
        private static LongitudinalDecision Decide(SpeedPlan plan, DriverProfile driver, float speed, float dt,
            LongitudinalPerception perception, LongitudinalMemory memory)
        {
            return LongitudinalArbitration.Decide(plan, driver, speed, dt, perception, memory, TrafficV2Settings.StopHold);
        }

        // ================================================================== noyaux 5.9 rejoues

        [Test]
        public void KernelV1B01FreeRoadReplaysAgainstTheArbitration()
        {
            var driver = Baseline();
            var atRestPlan = PlanAt(1f, 0f, driver);
            var atRest = Decide(atRestPlan, driver, 0f, Dt, Nothing(), LongitudinalMemory.None);
            var nearPlan = PlanAt(1f, driver.DesiredSpeed * 0.95f, driver);
            var near = Decide(nearPlan, driver, driver.DesiredSpeed * 0.95f, Dt, Nothing(), LongitudinalMemory.None);
            // Memes nombres que le noyau : route libre liante, valeur exacte du noyau.
            Assert.That(atRest.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.DesiredSpeed));
            Assert.That(atRest.TargetAccelerationMetersPerSecondSquared,
                Is.EqualTo(DriverModel.ComputeAcceleration(driver, 0f, 0f, DriverModel.NoLeaderGap)));
            Assert.That(near.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.DesiredSpeed));
            Assert.That(near.TargetAccelerationMetersPerSecondSquared,
                Is.EqualTo(DriverModel.ComputeAcceleration(driver, driver.DesiredSpeed * 0.95f, 0f, DriverModel.NoLeaderGap)));
            Assert.That(atRest.TargetAccelerationMetersPerSecondSquared, Is.GreaterThan(0f), "route libre sous v0 : acceleration positive");
            Assert.That(near.TargetAccelerationMetersPerSecondSquared, Is.GreaterThan(0f));
            Assert.That(near.TargetAccelerationMetersPerSecondSquared, Is.LessThan(atRest.TargetAccelerationMetersPerSecondSquared));

            var at = Decide(PlanAt(1f, driver.DesiredSpeed, driver), driver, driver.DesiredSpeed, Dt,
                Nothing(), LongitudinalMemory.None);
            Assert.That(at.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(0f).Within(0.0001f));
            var over = Decide(PlanAt(1f, driver.DesiredSpeed * 1.1f, driver), driver,
                driver.DesiredSpeed * 1.1f, Dt, Nothing(), LongitudinalMemory.None);
            Assert.That(over.TargetAccelerationMetersPerSecondSquared, Is.LessThan(0f), "au-dela de v0, jamais d'acceleration");
            foreach (var decision in new[] { atRest, near, at, over })
                Assert.That(float.IsNaN(decision.AppliedAccelerationMetersPerSecondSquared)
                    || float.IsInfinity(decision.AppliedAccelerationMetersPerSecondSquared), Is.False);
        }

        [Test]
        public void KernelV1B02StoppedLeaderAndCollapsedGapReplayAgainstTheArbitration()
        {
            var driver = Baseline();
            var plan = PlanAt(1f, 8f, driver);
            var firm = Decide(plan, driver, 8f, Dt, LeaderOnly(5f, 0f), LongitudinalMemory.None);
            Assert.That(firm.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.LeaderFollowing), "le suivi domine la vitesse desiree");
            Assert.That(firm.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(DriverModel.ComputeAcceleration(driver, 8f, 0f, 5f)));
            Assert.That(firm.TargetAccelerationMetersPerSecondSquared, Is.LessThan(-driver.ComfortableDeceleration));
            Assert.That(firm.AppliedAccelerationMetersPerSecondSquared, Is.EqualTo(firm.TargetAccelerationMetersPerSecondSquared));

            foreach (var gap in new[] { 0f, -1f, float.NaN })
            {
                var collapsed = Decide(plan, driver, 8f, Dt, LeaderOnly(gap, 0f), LongitudinalMemory.None);
                float a = collapsed.TargetAccelerationMetersPerSecondSquared;
                Assert.That(float.IsNaN(a) || float.IsInfinity(a), Is.False, "jeu " + gap);
                Assert.That(a, Is.LessThanOrEqualTo(0f), "jeu " + gap);
                Assert.That(a, Is.GreaterThan(-1000f), "jeu " + gap);
                Assert.That(collapsed.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.LeaderFollowing), "jeu " + gap);
            }
        }

        [Test]
        public void KernelV1B03TheArbitrationSettlesAtTheAuthoredMinimumGap()
        {
            var driver = Baseline();
            var plan = PlanAt(1f, 0f, driver);
            var atGap = Decide(plan, driver, 0f, Dt, LeaderOnly(driver.MinimumGap, 0f), LongitudinalMemory.None);
            // Le candidat du noyau reste l'equilibre a s0 ; a l'arret dans la fenetre, le maintien D11 le remplace comme liante.
            Assert.That(atGap.Candidates.Single(c => c.Kind == LongitudinalCandidateKind.LeaderFollowing).AccelerationMetersPerSecondSquared,
                Is.EqualTo(0f).Within(0.001f));
            Assert.That(atGap.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.StopHold));
            Assert.That(atGap.Binding.Constraint, Is.EqualTo(SpeedConstraint.LeaderFollowing));
            Assert.That(atGap.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-driver.ComfortableDeceleration));
            var tooClose = Decide(plan, driver, 0f, Dt, LeaderOnly(driver.MinimumGap * 0.5f, 0f), LongitudinalMemory.None);
            Assert.That(tooClose.Candidates.Single(c => c.Kind == LongitudinalCandidateKind.LeaderFollowing).AccelerationMetersPerSecondSquared,
                Is.LessThan(0f));
            Assert.That(tooClose.TargetAccelerationMetersPerSecondSquared, Is.LessThan(0f));
            var farther = Decide(plan, driver, 0f, Dt, LeaderOnly(driver.MinimumGap * 3f, 0f), LongitudinalMemory.None);
            Assert.That(farther.TargetAccelerationMetersPerSecondSquared, Is.GreaterThan(0f));
            Assert.That(farther.TargetAccelerationMetersPerSecondSquared,
                Is.EqualTo(DriverModel.ComputeAcceleration(driver, 0f, 0f, driver.MinimumGap * 3f)));
        }

        [Test]
        public void KernelV1B06RecoveryIsSmoothedWithoutOvershootAndRestrictionIsNeverDelayed()
        {
            var driver = Baseline();
            var plan = PlanAt(1f, 0f, driver);
            float target = DriverModel.ComputeAcceleration(driver, 0f, 0f, DriverModel.NoLeaderGap);
            // Reprise apres un arret derriere un leader : meme suite de nombres que le noyau SmoothAcceleration.
            var memory = new LongitudinalMemory(0f, SpeedConstraint.LeaderFollowing, SpeedConstraint.None);
            float kernel = 0f, previous = float.NegativeInfinity;
            for (int step = 0; step < 100; step++)
            {
                var decision = Decide(plan, driver, 0f, Dt, Nothing(), memory);
                kernel = DriverModel.SmoothAcceleration(kernel, target, driver.ReactionTime, Dt);
                float applied = decision.AppliedAccelerationMetersPerSecondSquared;
                Assert.That(decision.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(target));
                Assert.That(applied, Is.EqualTo(Math.Min(kernel, target)), "pas " + step);
                Assert.That(applied, Is.LessThanOrEqualTo(target + 0.0001f), "jamais de depassement");
                Assert.That(applied, Is.GreaterThan(previous), "convergence monotone");
                Assert.That(decision.Smoothed, Is.EqualTo(applied < target));
                previous = applied;
                memory = decision.Memory;
            }
            Assert.That(previous, Is.EqualTo(target).Within(0.01f));

            // Temps de reaction quasi instantane, lent, nul ou negatif : fini et borne.
            var start = new LongitudinalMemory(0f, SpeedConstraint.Obstacle, SpeedConstraint.None);
            float fast = Decide(plan, Baseline(8f, DriverModel.MinReactionTime), 0f, Dt, Nothing(), start)
                .AppliedAccelerationMetersPerSecondSquared;
            float slow = Decide(plan, Baseline(8f, 1f), 0f, Dt, Nothing(), start)
                .AppliedAccelerationMetersPerSecondSquared;
            Assert.That(fast, Is.GreaterThan(0.8f * target).And.LessThanOrEqualTo(target));
            Assert.That(slow, Is.LessThan(0.1f * target));
            foreach (var reaction in new[] { 0f, -1f, float.NaN })
            {
                float applied = Decide(plan, Baseline(8f, reaction), 0f, Dt, Nothing(), start)
                    .AppliedAccelerationMetersPerSecondSquared;
                Assert.That(float.IsNaN(applied) || float.IsInfinity(applied), Is.False, "reaction " + reaction);
                Assert.That(applied, Is.GreaterThan(0f).And.LessThanOrEqualTo(target), "reaction " + reaction);
            }

            // Aucune restriction retardee : un leader qui freine pendant la reprise s'applique au pas meme.
            var recovering = Decide(plan, driver, 0f, Dt, Nothing(),
                new LongitudinalMemory(0.2f, SpeedConstraint.DesiredSpeed, SpeedConstraint.LeaderFollowing));
            Assert.That(recovering.Smoothed, Is.True);
            var braking = Decide(plan, driver, 0f, Dt, LeaderOnly(driver.MinimumGap * 0.6f, 0f), recovering.Memory);
            Assert.That(braking.TargetAccelerationMetersPerSecondSquared, Is.LessThan(recovering.AppliedAccelerationMetersPerSecondSquared));
            Assert.That(braking.AppliedAccelerationMetersPerSecondSquared, Is.EqualTo(braking.TargetAccelerationMetersPerSecondSquared));
            Assert.That(braking.Smoothed, Is.False);
        }

        // ================================================================== matrice

        [Test]
        public void ASlowerLeaderOnTheHorizonBindsAsANamedConstraintWithItsRejectedAlternatives()
        {
            var bench = Straight;
            var driver = Driver;
            var step = Evaluate(bench, bench.Route, 7, new[] { ActorAt(Agent, bench, 2f, 6f), ActorAt(Other, bench, 22f, 2f) }, null,
                2f, 6f, LongitudinalMemory.None, driver);
            Assert.That(step.Observation.Leader.Items.Count, Is.EqualTo(1));
            Assert.That(step.Perception.HasLeader, Is.True);
            Assert.That(step.Perception.Leader.Id, Is.EqualTo(Other));
            var decision = step.Longitudinal;
            Assert.That(decision.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.LeaderFollowing));
            Assert.That(decision.Binding.Constraint, Is.EqualTo(SpeedConstraint.LeaderFollowing));
            Assert.That(decision.Binding.SourceId, Is.EqualTo(Other));
            Assert.That(decision.Binding.GapMeters, Is.EqualTo(step.Observation.Leader.Items[0].GapMeters));
            // Liante nommee et alternatives conservees avec leur valeur : profil, route libre.
            var kinds = decision.Candidates.Select(c => c.Kind).ToList();
            Assert.That(kinds, Is.EquivalentTo(new[] { LongitudinalCandidateKind.LeaderFollowing, LongitudinalCandidateKind.Profile,
                LongitudinalCandidateKind.DesiredSpeed }));
            Assert.That(decision.Candidates.All(c => c.AccelerationMetersPerSecondSquared >= decision.TargetAccelerationMetersPerSecondSquared), Is.True);
            Assert.That(decision.Candidates.First(c => c.Kind == LongitudinalCandidateKind.DesiredSpeed).AccelerationMetersPerSecondSquared,
                Is.EqualTo(DriverModel.ComputeAcceleration(driver, 6f, 0f, DriverModel.NoLeaderGap)));
            Assert.That(decision.TargetAccelerationMetersPerSecondSquared,
                Is.EqualTo(DriverModel.ComputeAcceleration(driver, 6f, 2f, step.Observation.Leader.Items[0].GapMeters)));
            StringAssert.Contains("Longitudinal binding LeaderFollowing:LeaderFollowing", decision.ToText());
        }

        [Test]
        public void AFarLeaderLeavesTheSmallestCandidateBindingAndRaisesNoBlocker()
        {
            var driver = Baseline();
            var plan = PlanAt(1f, 2f, driver);
            var far = Decide(plan, driver, 2f, Dt, LeaderOnly(30f, 2f), LongitudinalMemory.None);
            var leader = far.Candidates.First(c => c.Kind == LongitudinalCandidateKind.LeaderFollowing);
            Assert.That(leader.AccelerationMetersPerSecondSquared, Is.GreaterThan(0f));
            float minimum = far.Candidates.Min(c => c.AccelerationMetersPerSecondSquared);
            Assert.That(far.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(minimum));
            Assert.That(far.Binding.AccelerationMetersPerSecondSquared, Is.EqualTo(minimum));
            Assert.That(BlockerTracker.Update(null, far, driver, 3), Is.Empty);
        }

        [Test]
        public void AnObstacleInTheSweptPathBindsAndOneBesideTheLaneIsOnlyPublished()
        {
            var bench = Straight;
            var driver = Driver;
            var inPath = Evaluate(bench, bench.Route, 3, new[] { ActorAt(Agent, bench, 2f, 4f) },
                new[] { BoxAt(HazardId, bench, 12f, 0f, Vector3.zero) }, 2f, 4f, LongitudinalMemory.None, driver);
            Assert.That(inPath.Observation.Obstacles.Items.Count, Is.EqualTo(1));
            Assert.That(inPath.Observation.Obstacles.Items[0].InSweptPath, Is.True);
            Assert.That(inPath.Longitudinal.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.Obstacle));
            Assert.That(inPath.Longitudinal.Binding.SourceId, Is.EqualTo(HazardId));
            float near = inPath.Observation.Obstacles.Items[0].NearDistanceMeters;
            Assert.That(inPath.Longitudinal.TargetAccelerationMetersPerSecondSquared,
                Is.EqualTo(DriverModel.ComputeAcceleration(driver, 4f, 0f, near)));
            // En roulant, un candidat <= 0 freine mais n'immobilise pas : aucun blocker (D11).
            Assert.That(inPath.Longitudinal.TargetAccelerationMetersPerSecondSquared, Is.LessThan(0f));
            Assert.That(BlockerTracker.Update(null, inPath.Longitudinal, driver, 3), Is.Empty);
            // Arrete devant l'obstacle : maintien et blocker obstacle fixe.
            var held = Evaluate(bench, bench.Route, 3, new[] { ActorAt(Agent, bench, 2f, 0f) },
                new[] { BoxAt(HazardId, bench, 6f, 0f, Vector3.zero) }, 2f, 0f, LongitudinalMemory.None, driver);
            Assert.That(held.Longitudinal.Hold.Phase, Is.EqualTo(StopHoldPhase.Entered));
            var blockers = BlockerTracker.Update(null, held.Longitudinal, driver, 3);
            Assert.That(blockers.Count, Is.EqualTo(1));
            Assert.That(blockers[0].Kind, Is.EqualTo(BlockerKind.Obstacle));
            Assert.That(blockers[0].Legitimate && !blockers[0].ExpectedToClear && blockers[0].Recoverable, Is.True, "obstacle fixe");

            // Ecart lateral positif, dans la portee : publie par l'observation, aucune contrainte.
            float lateral = Car.RightMeters + 0.5f + 0.5f * TrafficV2Settings.PerceptionLateralRangeMeters;
            var beside = Evaluate(bench, bench.Route, 4, new[] { ActorAt(Agent, bench, 2f, 4f) },
                new[] { BoxAt(HazardId, bench, 12f, lateral, Vector3.zero) }, 2f, 4f, LongitudinalMemory.None, driver);
            Assert.That(beside.Observation.Obstacles.Items.Count, Is.EqualTo(1), "publie");
            Assert.That(beside.Observation.Obstacles.Items[0].InSweptPath, Is.False);
            Assert.That(beside.Perception.Obstacles, Is.Empty);
            Assert.That(beside.Longitudinal.Candidates.Any(c => c.Kind == LongitudinalCandidateKind.Obstacle), Is.False);
            Assert.That(BlockerTracker.Update(null, beside.Longitudinal, driver, 4), Is.Empty);
        }

        [Test]
        public void AnApproachingHazardIsTreatedAtZeroSpeedAndARecedingOneAtItsSpeedAlongThePath()
        {
            var bench = Straight;
            var driver = Driver;
            var tangent = bench.Track.Pieces[0].Nominal(10f).Forward;
            var approaching = Evaluate(bench, bench.Route, 5, new[] { ActorAt(Agent, bench, 2f, 4f) },
                new[] { BoxAt(HazardId, bench, 18f, 0f, -3f * tangent, TrafficHazardKind.WalkingPlayer) }, 2f, 4f,
                LongitudinalMemory.None, driver);
            Assert.That(approaching.Perception.Obstacles.Count, Is.EqualTo(1));
            Assert.That(approaching.Perception.Obstacles[0].SpeedAlongPathMetersPerSecond, Is.EqualTo(0f));
            float near = approaching.Perception.Obstacles[0].NearDistanceMeters;
            Assert.That(approaching.Longitudinal.ObstacleCandidates[0].AccelerationMetersPerSecondSquared,
                Is.EqualTo(DriverModel.ComputeAcceleration(driver, 4f, 0f, near)));
            var receding = Evaluate(bench, bench.Route, 6, new[] { ActorAt(Agent, bench, 2f, 4f) },
                new[] { BoxAt(HazardId, bench, 18f, 0f, 3f * tangent, TrafficHazardKind.WalkingPlayer) }, 2f, 4f,
                LongitudinalMemory.None, driver);
            Assert.That(receding.Perception.Obstacles[0].SpeedAlongPathMetersPerSecond, Is.EqualTo(3f).Within(1e-4f));
            // Un pieton qui s'arrete devant est un obstacle mobile : legitime, liberation attendue.
            var stopped = Evaluate(bench, bench.Route, 7, new[] { ActorAt(Agent, bench, 2f, 0f) },
                new[] { BoxAt(HazardId, bench, 6f, 0f, Vector3.zero, TrafficHazardKind.WalkingPlayer) }, 2f, 0f,
                LongitudinalMemory.None, driver);
            var blockers = BlockerTracker.Update(null, stopped.Longitudinal, driver, 7);
            Assert.That(blockers.Single().Kind, Is.EqualTo(BlockerKind.Obstacle));
            Assert.That(blockers[0].Legitimate && blockers[0].ExpectedToClear && !blockers[0].Recoverable, Is.True, "obstacle mobile");
        }

        [Test]
        public void UnavailableOrTruncatedPerceptionBindsPerceptionUnavailableWithItsReason()
        {
            var bench = Straight;
            var driver = Driver;
            // Agent sans empreinte : canaux a emprise UndeclaredFootprint.
            var undeclared = Evaluate(bench, bench.Route, 1, new[] { ActorAt(Agent, bench, 2f, 0f, false) }, null, 2f, 0f,
                LongitudinalMemory.None, driver);
            Assert.That(undeclared.Observation.Leader.Status, Is.EqualTo(PerceptionStatus.UndeclaredFootprint));
            AssertUnavailable(undeclared.Longitudinal, PerceptionUnavailableReason.ChannelUnavailable);

            // Horizon qui ne part pas de l'element occupe : AgentOccupancyUnavailable.
            var model = Admission.Model;
            var frame = new TrafficFrame(2, model, new[] { ActorAt(Agent, bench, 2f, 0f) });
            var next = bench.Route.Occurrences[1];
            var ahead = RoutePlanner.Plan(new RouteRequest(model, new RoadLocation { ModelId = model.ModelId, ModelVersion = model.Version,
                Localized = true, ElementKind = next.Kind, ElementId = next.Id, SMeters = next.StartSMeters }, RoadId.None,
                new RouteSeed(0), Agent, "route", new DecisionCounter(0)));
            Assert.That(ahead.Plan, Is.Not.Null, ahead.Reason.ToString());
            var shifted = PathHorizon.Build(model, ahead.Plan, TrafficV2Settings.LookAheadMeters);
            var observation = TrafficPerception.Observe(frame, Agent, shifted, TrafficV2Settings.PerceptionLimits,
                new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity));
            Assert.That(observation.Leader.Status, Is.EqualTo(PerceptionStatus.AgentOccupancyUnavailable));
            var perception = LongitudinalPerception.From(observation, shifted, float.NaN, false);
            Assert.That(perception.UnavailableReason, Is.EqualTo(PerceptionUnavailableReason.ChannelUnavailable));
            Assert.That(LongitudinalPerception.From(default(AgentObservation), shifted, float.NaN, false).UnavailableReason,
                Is.EqualTo(PerceptionUnavailableReason.ChannelUnavailable), "observation non percue");

            // Requete spatiale saturee : deux candidats pour une capacite de 1.
            var actors = new[] { ActorAt(Agent, bench, 2f, 0f) };
            var hazards = new[] { BoxAt(HazardId, bench, 20f, 0f, Vector3.zero), BoxAt(new RoadId(0x4841UL, 2UL), bench, 30f, 0f, Vector3.zero) };
            var saturated = Evaluate(bench, bench.Route, 3, actors, hazards, 2f, 0f, LongitudinalMemory.None, driver, false, 1);
            Assert.That(saturated.Observation.SpatialQuerySaturated, Is.True);
            AssertUnavailable(saturated.Longitudinal, PerceptionUnavailableReason.ChannelSaturated);
            Assert.That(saturated.Longitudinal.ObstacleCandidates.Count, Is.EqualTo(2), "les plus proches restent des candidats");

            // Plus d'obstacles que la capacite de liste : plus proches gardes, total publie.
            var many = new List<TrafficHazardInput>();
            for (int i = 0; i < TrafficV2Settings.PerceptionListCapacity + 2; i++)
                many.Add(BoxAt(new RoadId(0x4842UL, (ulong)(i + 1)), bench, 12f + 2.5f * i, 0f, Vector3.zero));
            var crowded = Evaluate(bench, bench.Route, 4, actors, many, 2f, 0f, LongitudinalMemory.None, driver);
            Assert.That(crowded.Observation.Obstacles.Saturated, Is.True);
            Assert.That(crowded.Observation.Obstacles.Total, Is.EqualTo(many.Count));
            Assert.That(crowded.Observation.Obstacles.Items.Count, Is.EqualTo(TrafficV2Settings.PerceptionListCapacity));
            AssertUnavailable(crowded.Longitudinal, PerceptionUnavailableReason.ChannelSaturated);

            // Requete du collecteur saturee pour ce vehicule.
            var collector = Evaluate(bench, bench.Route, 5, actors, null, 2f, 0f, LongitudinalMemory.None, driver, true);
            AssertUnavailable(collector.Longitudinal, PerceptionUnavailableReason.HazardCollectorSaturated);
        }

        private static void AssertUnavailable(LongitudinalDecision decision, PerceptionUnavailableReason reason)
        {
            Assert.That(decision.PerceptionReason, Is.EqualTo(reason));
            var candidate = decision.Candidates.Single(c => c.Kind == LongitudinalCandidateKind.PerceptionUnavailable);
            Assert.That(candidate.AccelerationMetersPerSecondSquared, Is.EqualTo(0f));
            Assert.That(decision.TargetAccelerationMetersPerSecondSquared, Is.LessThanOrEqualTo(0f), "jamais lue comme complete");
            StringAssert.Contains("perception " + reason, decision.ToText());
        }

        [Test]
        public void RestrictionAppliesTheSameStepAndOnlyRecoveryAfterAnInteractionIsSmoothed()
        {
            var driver = Baseline();
            var plan = PlanAt(1f, 4f, driver);
            // Leader qui ralentit puis jeu qui diminue : chaque cible plus basse est appliquee au pas meme.
            var memory = LongitudinalMemory.None;
            float gap = 14f;
            for (int step = 0; step < 20; step++)
            {
                var decision = Decide(plan, driver, 4f, Dt, LeaderOnly(gap, 3f - 0.1f * step), memory);
                Assert.That(decision.AppliedAccelerationMetersPerSecondSquared, Is.EqualTo(decision.TargetAccelerationMetersPerSecondSquared));
                memory = decision.Memory;
                gap -= 0.3f;
            }
            // Obstacle qui apparait pendant une reprise : immediat.
            var recovering = new LongitudinalMemory(-1f, SpeedConstraint.DesiredSpeed, SpeedConstraint.LeaderFollowing);
            var appears = Decide(plan, driver, 4f, Dt, new LongitudinalPerception(PerceptionUnavailableReason.None,
                null, new[] { new LongitudinalObstacle(HazardId, PerceivedObstacleKind.Obstacle, 3f, 0f) }), recovering);
            Assert.That(appears.TargetAccelerationMetersPerSecondSquared, Is.LessThan(-1f));
            Assert.That(appears.AppliedAccelerationMetersPerSecondSquared, Is.EqualTo(appears.TargetAccelerationMetersPerSecondSquared));
            // Hausse sans contrainte d'interaction precedente : aucune reprise lissee (route libre 5.31).
            var free = Decide(plan, driver, 4f, Dt, Nothing(),
                new LongitudinalMemory(-1f, SpeedConstraint.CurveLimit, SpeedConstraint.None));
            Assert.That(free.Smoothed, Is.False);
            Assert.That(free.AppliedAccelerationMetersPerSecondSquared, Is.EqualTo(free.TargetAccelerationMetersPerSecondSquared));
            // Reprise apres PerceptionUnavailable : lissee, plafonnee a la cible.
            var after = Decide(plan, driver, 4f, Dt, Nothing(),
                new LongitudinalMemory(0f, SpeedConstraint.PerceptionUnavailable, SpeedConstraint.None));
            Assert.That(after.Smoothed, Is.True);
            Assert.That(after.SmoothingSource, Is.EqualTo(SpeedConstraint.PerceptionUnavailable));
            Assert.That(after.AppliedAccelerationMetersPerSecondSquared, Is.GreaterThan(0f).And.LessThan(after.TargetAccelerationMetersPerSecondSquared));
        }

        [Test]
        public void WithoutInteractionTheAccelerationIsTheStory531OneToTheBit()
        {
            var driver = Driver;
            var model = Admission.Model;
            int compared = 0, unreachable = 0;
            foreach (int entry in new[] { 0, 1, 2, 3 })
            {
                var portal = model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).ElementAt(entry);
                var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, portal.Id, RoadId.None, 0UL, 1UL, driver, Dt);
                Assert.That(insertion.Code, Is.EqualTo(TrafficV2Code.Allowed));
                var route = insertion.Route;
                var track = ReferenceTrack.FromRoute(model, route.Occurrences, 0f);
                var ids = route.Occurrences.Select(o => o.Id).ToArray();
                for (float distance = 0.5f; distance < track.LengthMeters - 0.5f; distance += 4.3f)
                    foreach (float speed in new[] { 0f, 1f, 4f, 7.9f })
                    {
                        int piece = track.PieceAt(distance);
                        var nominal = track.Nominal(piece, distance);
                        var pose = new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward, Up = nominal.Up, Footprint = Car };
                        var frame = new TrafficFrame(9, model, new[] { new TrafficActorInput(insertion.TrafficId, pose, speed,
                            track.Pieces[piece].Id, ids, track.KinematicAnchors(piece)) });
                        var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, route, route.ExitPortalId,
                            insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared,
                            null, null, Admission.Evidence, RoadId.None, track.OffsetRadians(piece, distance)));
                        if (decision.Motion == null) continue;
                        var plan = SpeedPlan.Build(decision.Motion, model, driver, speed);
                        if (!plan.Accepted) continue;
                        if (plan.Binding == SpeedConstraint.SteeringCeilingUnreachable) unreachable++;
                        var observation = TrafficPerception.Observe(frame, insertion.TrafficId, decision.Path,
                            TrafficV2Settings.PerceptionLimits, new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity));
                        var perception = LongitudinalPerception.From(observation, decision.Path,
                            LongitudinalPerception.FrontDistanceMeters(frame, insertion.TrafficId, decision.Path), false);
                        Assert.That(perception.UnavailableReason, Is.EqualTo(PerceptionUnavailableReason.None),
                            "un vehicule seul en route libre : canaux evalues, d = " + distance);
                        var interval = decision.Path.Intervals[0];
                        var curve = CurveOf(model, interval);
                        foreach (var memory in new[] { LongitudinalMemory.None,
                            new LongitudinalMemory(0.7f, SpeedConstraint.CurveLimit, SpeedConstraint.None) })
                        {
                            var longitudinal = Decide(plan, driver, speed, Dt, perception, memory);
                            var with = MotionCommand.Track(9, 1, plan, driver, model.DrivabilityProfile, curve, interval.StartSMeters,
                                pose.Position, pose.Forward, speed, Dt, track.NominalHeadingErrorDegrees(piece, distance), longitudinal);
                            var without = MotionCommand.Track(9, 1, plan, driver, model.DrivabilityProfile, curve, interval.StartSMeters,
                                pose.Position, pose.Forward, speed, Dt, track.NominalHeadingErrorDegrees(piece, distance));
                            Assert.That(BitConverter.GetBytes(with.TargetAccelerationMetersPerSecondSquared),
                                Is.EqualTo(BitConverter.GetBytes(without.TargetAccelerationMetersPerSecondSquared)),
                                "d = " + distance + ", v = " + speed + ", liante " + longitudinal.Binding.ToText());
                            Assert.That(with.TargetWheelAngleDegrees, Is.EqualTo(without.TargetWheelAngleDegrees));
                            Assert.That(longitudinal.Smoothed, Is.False);
                            compared++;
                        }
                    }
            }
            Assert.That(compared, Is.GreaterThan(200));
            Debug.Log("[Story533] route libre 5.31 au bit pres : " + compared + " comparaisons, dont " + unreachable
                + " plans SteeringCeilingUnreachable.");
        }

        private static RoadCurve CurveOf(CompiledRoadModel model, PathInterval interval)
        {
            EffectiveLaneCorridor corridor;
            CompiledJunctionMovement movement;
            if (interval.Kind == RoadElementKind.LaneCorridor && model.TryGetCorridor(interval.Id, out corridor)) return corridor.Curve;
            Assert.That(model.TryGetMovement(interval.Id, out movement), Is.True);
            return movement.Curve;
        }

        [Test]
        public void UnderAnUnreachableCeilingTheStory531RuleSupersedesTheProfileAndTheFreeRoad()
        {
            var driver = Driver;
            var model = Admission.Model;
            // Patron 5.31 : plafonds de regime etabli (horizon sans pose nominale), 6 m/s a 6 m d'un point v* < 1 m/s.
            var portal = model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).First();
            var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, portal.Id, RoadId.None, 0UL, 1UL, driver, Dt);
            var route = insertion.Route;
            var ids = route.Occurrences.Select(o => o.Id).ToArray();
            var track = ReferenceTrack.FromRoute(model, route.Occurrences, 0f);
            Func<float, float, SpeedPlan> planAt = (distance, v) =>
            {
                int piece = track.PieceAt(distance);
                var point = track.Pieces[piece].Curve.Sample(track.Pieces[piece].ElementS(distance));
                var frame = new TrafficFrame(1, model, new[] { new TrafficActorInput(insertion.TrafficId, new VehicleFootprintPose {
                    Position = point.Position, Forward = point.Tangent, Up = Vector3.up }, v, track.Pieces[piece].Id, ids) });
                var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, route, route.ExitPortalId,
                    insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared, null, null,
                    Admission.Evidence));
                return SpeedPlan.Build(decision.Motion, model, driver, v);
            };
            float tight = planAt(0f, 0f).Points.First(p => !p.CeilingUnbounded && p.CeilingMetersPerSecond < 1f).DistanceMeters;
            float speed = 6f;
            var plan = planAt(tight - 6f, speed);
            Assert.That(plan.Binding, Is.EqualTo(SpeedConstraint.SteeringCeilingUnreachable));
            Assert.That(plan.Accepted, Is.True);
            foreach (float low in new[] { speed, 1f, 0.3f })
            {
                var decision = Decide(plan, driver, low, Dt, Nothing(), LongitudinalMemory.None);
                Assert.That(decision.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-driver.SafeBrakingLimit), "v = " + low);
                Assert.That(decision.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.SteeringCeilingUnreachable));
                Assert.That(decision.Candidates.Where(c => c.Kind == LongitudinalCandidateKind.Profile
                    || c.Kind == LongitudinalCandidateKind.DesiredSpeed).All(c => c.Superseded), Is.True, "publies, hors du minimum");
                // Une interaction plus forte domine encore.
                var wedge = Decide(plan, driver, low, Dt, LeaderOnly(0f, 0f), LongitudinalMemory.None);
                Assert.That(wedge.TargetAccelerationMetersPerSecondSquared, Is.LessThan(-driver.SafeBrakingLimit));
                Assert.That(wedge.Binding.Constraint, Is.EqualTo(SpeedConstraint.LeaderFollowing), "suivi, maintenu ou non");
            }
        }

        [Test]
        public void TheTieOrderIsDeterministicAndTheArbitrationKeepsNoState()
        {
            var driver = Baseline();
            var plan = PlanAt(1f, 0f, driver);
            // Obstacle et leader de meme candidat : Obstacle d'abord.
            var tie = new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(Other, 3f, 0f),
                new[] { new LongitudinalObstacle(HazardId, PerceivedObstacleKind.Obstacle, 3f, 0f) });
            Assert.That(Decide(plan, driver, 0f, Dt, tie, LongitudinalMemory.None).Binding.Kind,
                Is.EqualTo(LongitudinalCandidateKind.Obstacle));
            // Perception indisponible (0) et leader a l'equilibre (0) : PerceptionUnavailable d'abord.
            var zero = new LongitudinalPerception(PerceptionUnavailableReason.ChannelSaturated, new LongitudinalLeader(Other, 2f, 0f), null);
            var zeroDecision = Decide(plan, driver, 0f, Dt, zero, LongitudinalMemory.None);
            Assert.That(zeroDecision.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.PerceptionUnavailable));
            // Profil et route libre a v0 : le profil d'abord.
            var cruise = Decide(PlanAt(1f, 8f, driver), driver, 8f, Dt, Nothing(), LongitudinalMemory.None);
            Assert.That(cruise.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.Profile));
            // Obstacles de meme candidat : plus petit id.
            var twins = new LongitudinalPerception(PerceptionUnavailableReason.None, null, new[]
            {
                new LongitudinalObstacle(new RoadId(9, 9), PerceivedObstacleKind.Obstacle, 4f, 0f),
                new LongitudinalObstacle(new RoadId(1, 1), PerceivedObstacleKind.Obstacle, 4f, 0f)
            });
            Assert.That(Decide(plan, driver, 0f, Dt, twins, LongitudinalMemory.None).Binding.SourceId,
                Is.EqualTo(new RoadId(1, 1)));

            // Determinisme et absence d'etat : A, B, A rendent deux fois le meme texte.
            var a = new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(Other, 6f, 1f),
                new[] { new LongitudinalObstacle(HazardId, PerceivedObstacleKind.Vehicle, 9f, 0.5f) });
            var memory = new LongitudinalMemory(0.3f, SpeedConstraint.LeaderFollowing, SpeedConstraint.None);
            string first = Decide(plan, driver, 0f, Dt, a, memory).ToText();
            Decide(plan, driver, 0f, Dt, zero, LongitudinalMemory.None);
            Assert.That(Decide(plan, driver, 0f, Dt, a, memory).ToText(), Is.EqualTo(first));
            foreach (var type in new[] { typeof(LongitudinalArbitration), typeof(LongitudinalPerception), typeof(BlockerTracker), typeof(BlockerRules) })
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    Assert.That(field.IsLiteral || field.IsInitOnly, Is.True, type.Name + "." + field.Name + " : aucun etat statique");
        }

        // ================================================================== blockers

        [Test]
        public void BlockersCoexistKeepTheirSinceFrameAndResetAfterAnAbsence()
        {
            var driver = Baseline();
            var plan = PlanAt(1f, 0f, driver);
            var both = new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(Other, 2f, 0f),
                new[] { new LongitudinalObstacle(HazardId, PerceivedObstacleKind.Obstacle, 1.5f, 0f) });
            var leaderOnly = LeaderOnly(2f, 0f);
            var decision10 = Decide(plan, driver, 0f, Dt, both, LongitudinalMemory.None);
            var set10 = BlockerTracker.Update(null, decision10, driver, 10);
            Assert.That(set10.Select(b => b.Kind), Is.EqualTo(new[] { BlockerKind.Obstacle, BlockerKind.Leader }), "deux records, jamais un");
            Blocker dominant;
            Assert.That(BlockerTracker.TryGetDominant(set10, out dominant), Is.True);
            Assert.That(dominant.Kind, Is.EqualTo(BlockerKind.Obstacle));
            Assert.That(decision10.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.StopHold), "maintien sur la liante d'interaction");
            Assert.That(decision10.Binding.Constraint, Is.EqualTo(SpeedConstraint.Obstacle), "causes multiples : dominant = cause du maintien");
            var set11 = BlockerTracker.Update(set10, Decide(plan, driver, 0f, Dt, both, decision10.Memory), driver, 11);
            Assert.That(set11.All(b => b.SinceFrame == 10UL), Is.True, "presence continue");
            var set12 = BlockerTracker.Update(set11, Decide(plan, driver, 0f, Dt, leaderOnly, LongitudinalMemory.None), driver, 12);
            Assert.That(set12.Single().Kind, Is.EqualTo(BlockerKind.Leader));
            Assert.That(set12[0].SinceFrame, Is.EqualTo(10UL));
            var set13 = BlockerTracker.Update(set12, Decide(plan, driver, 0f, Dt, both, LongitudinalMemory.None), driver, 13);
            Assert.That(set13.First(b => b.Kind == BlockerKind.Obstacle).SinceFrame, Is.EqualTo(13UL), "absence d'un pas : reinitialise");
            Assert.That(set13.First(b => b.Kind == BlockerKind.Leader).SinceFrame, Is.EqualTo(10UL));
            Assert.That(BlockerTracker.Update(set13, null, driver, 14), Is.Empty, "aucune decision : ensemble vide");
            // Le meme genre pour un autre bloqueur est un autre record.
            var otherLeader = BlockerTracker.Update(set12, Decide(plan, driver, 0f, Dt,
                new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(new RoadId(7, 7), 2f, 0f), null),
                LongitudinalMemory.None), driver, 15);
            Assert.That(otherLeader.Single().SinceFrame, Is.EqualTo(15UL));
        }

        [Test]
        public void AStopBehindALeaderIsALegitimateBlockerAndAWedgeIsNot()
        {
            var driver = Baseline();
            var plan = PlanAt(1f, 0f, driver);
            var deliberate = BlockerTracker.Update(null, Decide(plan, driver, 0f, Dt,
                LeaderOnly(driver.MinimumGap, 0f), LongitudinalMemory.None), driver, 4).Single();
            Assert.That(deliberate.Kind, Is.EqualTo(BlockerKind.Leader));
            Assert.That(deliberate.Source, Is.EqualTo(BlockerSource.LeaderObservation));
            Assert.That(deliberate.BlockingActorOrRule, Is.EqualTo(Other.ToString()));
            Assert.That(deliberate.Legitimate, Is.True);
            Assert.That(deliberate.ExpectedToClear, Is.True);
            Assert.That(deliberate.Recoverable, Is.False);
            Assert.That(deliberate.SinceFrame, Is.EqualTo(4UL));
            var halfGap = BlockerTracker.Update(null, Decide(plan, driver, 0f, Dt,
                LeaderOnly(driver.MinimumGap * 0.5f, 0f), LongitudinalMemory.None), driver, 4).Single();
            Assert.That(halfGap.Legitimate, Is.True, "jeu = s0/2 : encore un arret voulu");
            var wedge = BlockerTracker.Update(null, Decide(plan, driver, 0f, Dt,
                LeaderOnly(driver.MinimumGap * 0.4f, 0f), LongitudinalMemory.None), driver, 4).Single();
            Assert.That(wedge.Legitimate, Is.False, "encastrement");
            Assert.That(wedge.Recoverable, Is.True);
            Assert.That(wedge.ExpectedToClear, Is.True);
        }

        [Test]
        public void PolicyImmobilizationIsALegitimateBlockerAtTheKernelThreshold()
        {
            // Seuil epingle sur le noyau : a 0,001 m/s le profil immobilise, au-dessus il ne le fait plus.
            Assert.That(DriverModel.ComputeAcceleration(Baseline(BlockerRules.PolicyImmobilizationSpeedMetersPerSecond), 1f, 0f,
                DriverModel.NoLeaderGap), Is.EqualTo(-2f), "branche d'immobilisation du noyau");
            Assert.That(DriverModel.ComputeAcceleration(Baseline(BlockerRules.PolicyImmobilizationSpeedMetersPerSecond * 1.1f), 1f, 0f,
                DriverModel.NoLeaderGap), Is.Not.EqualTo(-2f));
            var plan = PlanAt(1f, 0f, Baseline());
            foreach (float desired in new[] { 0f, BlockerRules.PolicyImmobilizationSpeedMetersPerSecond })
            {
                var immobile = Baseline(desired);
                var decision = Decide(plan, immobile, 0f, Dt, Nothing(), LongitudinalMemory.None);
                var blocker = BlockerTracker.Update(null, decision, immobile, 8).Single();
                Assert.That(blocker.Kind, Is.EqualTo(BlockerKind.PolicyImmobilization));
                Assert.That(blocker.Source, Is.EqualTo(BlockerSource.DrivingPolicy));
                Assert.That(blocker.BlockingActorOrRule, Is.EqualTo(BlockerRules.PolicyImmobilizationRule));
                Assert.That(blocker.Legitimate && !blocker.ExpectedToClear && !blocker.Recoverable, Is.True);
                Assert.That(blocker.CandidateAccelerationMetersPerSecondSquared,
                    Is.EqualTo(decision.Candidates.First(c => c.Kind == LongitudinalCandidateKind.DesiredSpeed).AccelerationMetersPerSecondSquared));
            }
            Assert.That(BlockerTracker.Update(null, Decide(plan, Baseline(0.0011f), 0f, Dt, Nothing(),
                LongitudinalMemory.None), Baseline(0.0011f), 8), Is.Empty);
        }

        [Test]
        public void LowSpeedWithoutAnExternalCauseIsNeverABlocker()
        {
            var driver = Driver;
            var bench = Straight;
            // Ralenti par le seul profil (fin de ligne droite avant un virage), vitesse basse : aucun blocker.
            foreach (float speed in new[] { 0f, 0.05f, 0.3f, 2f })
            {
                var step = Evaluate(bench, bench.Route, 2, new[] { ActorAt(Agent, bench, bench.StraightLengthMeters - 0.5f, speed) }, null,
                    bench.StraightLengthMeters - 0.5f, speed, LongitudinalMemory.None, driver);
                Assert.That(BlockerTracker.Update(null, step.Longitudinal, driver, 2), Is.Empty, "v = " + speed);
            }
        }

        // ================================================================== limite de route

        private static string Sha256(string value)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>Preuve documentaire synthetique valide pour un modele mute (patron Story 5.31, a_e = 0).</summary>
        private static GateAEvidenceResult SyntheticEvidence(CompiledRoadModel model)
        {
            const string modelText = "synthetic 5.33 model\n";
            const string block = "a_e = 0 m\n| Genre | Valeur |\n|---|---|\n| test | 0 |\n";
            string signoff = "{\"RoadModelVersion\":\"" + model.Version + "\",\"ModelHash\":\"" + Sha256(modelText)
                + "\",\"ClearanceHash\":\"" + Sha256(block) + "\"}";
            var evidence = GateAEvidenceBinding.Bind(model, modelText, signoff, "### Residus\n" + block + "\nfin\n");
            Assert.That(evidence.Valid, Is.True);
            return evidence;
        }

        private static void Override(RoadModelSource source, RoadId corridorId, float metersPerSecond)
        {
            for (int i = 0; i < source.Corridors.Length; i++)
                if (source.Corridors[i].Id == corridorId)
                {
                    source.Corridors[i].HasSpeedLimitOverride = true;
                    source.Corridors[i].SpeedLimitOverrideMetersPerSecond = metersPerSecond;
                }
        }

        private static CompiledRoadModel Mutated(Action<RoadModelSource> mutate)
        {
            var source = RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath));
            mutate(source);
            return RoadModelCompiler.Compile(source);
        }

        [Test]
        public void AnAuthoredCorridorLimitIsAppliedAndBindsOnAMutatedModel()
        {
            var straight = Straight;
            var limited = straight.Route.Occurrences[0].Id;
            var model = Mutated(source => Override(source, limited, 3f));
            var evidence = SyntheticEvidence(model);
            var driver = Driver;
            var limit = SpeedPlan.RoadLimitOf(model, RoadElementKind.LaneCorridor, limited);
            Assert.That(limit.State, Is.EqualTo(RoadLimitState.Applied));
            Assert.That(limit.MetersPerSecond, Is.EqualTo(3f));
            var track = ReferenceTrack.FromRoute(model, straight.Route.Occurrences, 0f);
            foreach (float speed in new[] { 0f, 3f, 6f })
            {
                var nominal = track.Nominal(0, 2f);
                var frame = new TrafficFrame(1, model, new[] { new TrafficActorInput(Agent, new VehicleFootprintPose { Position = nominal.Position,
                    Forward = nominal.Forward, Up = nominal.Up, Footprint = Car }, speed, limited, straight.Ids, track.KinematicAnchors(0)) });
                var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, Agent, null, RoadId.None, new RouteSeed(0),
                    TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared, null, null, evidence));
                Assert.That(decision.Motion, Is.Not.Null, decision.Projection.Code);
                var plan = SpeedPlan.Build(decision.Motion, model, driver, speed);
                Assert.That(plan.Accepted, Is.True, plan.Verification.Issue.ToString());
                Assert.That(plan.DeferredLimits, Is.Empty, "plus aucune limite reportee");
                Assert.That(plan.RoadLimits[0].State, Is.EqualTo(RoadLimitState.Applied));
                Assert.That(plan.LimitingConstraint, Is.EqualTo(SpeedConstraint.RoadLimit), "v = " + speed);
                float end = decision.Path.Intervals[0].EndDistanceMeters;
                foreach (var point in plan.Points.Where(p => p.DistanceMeters <= end && p.DistanceMeters > 0.5f && speed <= 3f))
                    Assert.That(point.SpeedMetersPerSecond, Is.LessThanOrEqualTo(3f + 1e-4f), "plafond applique a " + point.DistanceMeters);
                Assert.That(plan.Points.Any(p => p.Binding == SpeedConstraint.RoadLimit), Is.True);
                var perception = LongitudinalPerception.From(TrafficPerception.Observe(frame, Agent, decision.Path,
                    TrafficV2Settings.PerceptionLimits, new SpatialQueryBuffer(8)), decision.Path,
                    LongitudinalPerception.FrontDistanceMeters(frame, Agent, decision.Path), false);
                var longitudinal = Decide(plan, driver, speed, Dt, perception, LongitudinalMemory.None);
                if (speed >= 3f)
                {
                    Assert.That(longitudinal.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.Profile), "v = " + speed);
                    Assert.That(longitudinal.Binding.Constraint, Is.EqualTo(SpeedConstraint.RoadLimit), "liante nommee RoadLimit");
                }
                if (speed > 3f)
                {
                    // Limite qui baisse sous la vitesse : la restriction s'applique au pas, meme pendant une reprise.
                    var recovering = Decide(plan, driver, speed, Dt, perception,
                        new LongitudinalMemory(1f, SpeedConstraint.DesiredSpeed, SpeedConstraint.LeaderFollowing));
                    Assert.That(recovering.TargetAccelerationMetersPerSecondSquared, Is.LessThan(0f));
                    Assert.That(recovering.AppliedAccelerationMetersPerSecondSquared, Is.EqualTo(recovering.TargetAccelerationMetersPerSecondSquared));
                }
            }
        }

        [Test]
        public void AMovementTakesTheLowestAuthoredLimitOfItsCorridorsAndZeroIsUnauthored()
        {
            var signed = Admission.Model;
            var movement = signed.Movements.OrderBy(m => m.Id).First();
            var model = Mutated(source =>
            {
                Override(source, movement.FromCorridorId, 6f);
                Override(source, movement.ToCorridorId, 4f);
            });
            var both = SpeedPlan.RoadLimitOf(model, RoadElementKind.JunctionMovement, movement.Id);
            Assert.That(both.State, Is.EqualTo(RoadLimitState.Applied));
            Assert.That(both.MetersPerSecond, Is.EqualTo(4f), "min des valeurs authorees");
            var one = Mutated(source => Override(source, movement.ToCorridorId, 5f));
            var single = SpeedPlan.RoadLimitOf(one, RoadElementKind.JunctionMovement, movement.Id);
            Assert.That(single.MetersPerSecond, Is.EqualTo(5f), "un 0 n'est pas une valeur authoree");
            var none = SpeedPlan.RoadLimitOf(signed, RoadElementKind.JunctionMovement, movement.Id);
            Assert.That(none.State, Is.EqualTo(RoadLimitState.Unauthored));
            Assert.That(none.CapMetersPerSecond, Is.EqualTo(float.PositiveInfinity), "aucun plafond");
            Assert.That(new RoadLimitValue(RoadElementKind.LaneCorridor, Agent, 0f).State, Is.EqualTo(RoadLimitState.Unauthored));
            Assert.That(new RoadLimitValue(RoadElementKind.LaneCorridor, Agent, float.NaN).State, Is.EqualTo(RoadLimitState.Unauthored));
        }

        [Test]
        public void TheSignedModelPublishesUnauthoredLimitsAndNoDeferredLimit()
        {
            var bench = Straight;
            var step = Evaluate(bench, bench.Route, 1, new[] { ActorAt(Agent, bench, 1f, 0f) }, null, 1f, 0f, LongitudinalMemory.None, Driver);
            Assert.That(step.Plan.DeferredLimits, Is.Empty);
            Assert.That(step.Plan.RoadLimits.Count, Is.EqualTo(step.Decision.Path.Intervals.Count));
            Assert.That(step.Plan.RoadLimits.All(l => l.State == RoadLimitState.Unauthored && l.MetersPerSecond == 0f), Is.True);
            Assert.That(step.Plan.Points.All(p => float.IsPositiveInfinity(p.RoadLimitMetersPerSecond)), Is.True);
            Assert.That(step.Plan.Points.Any(p => p.Binding == SpeedConstraint.RoadLimit), Is.False);
        }

        // ================================================================== banc point-masse

        private struct Simulation
        {
            public float MinimumGap;
            public float FinalGap;
            public float FinalSpeed;
            public float SettledAtSeconds;
            public float MinimumSteadyGap;
            public int Steps;
            /// <summary>Distance parcourue sous la vitesse d'entree du maintien, acceleration &gt; 0, liee a une interaction, hors maintien.</summary>
            public float CrawlMeters;
        }

        /// <summary>
        /// Suiveur point-masse sur la ligne droite de reference : chaque pas construit une frame reelle (suiveur et leader
        /// declares, ou obstacle), le spine, la perception, le plan et l'arbitrage ; v += a dt, x += v dt.
        /// </summary>
        private static Simulation Simulate(float targetDistance, float targetSpeed, bool obstacle, float seconds, bool stopWhenSettled)
        {
            var bench = Straight;
            var driver = Driver;
            var route = bench.Route;
            var memory = LongitudinalMemory.None;
            float x = 1f, v = 0f;
            int settled = 0;
            var result = new Simulation { MinimumGap = float.PositiveInfinity, MinimumSteadyGap = float.PositiveInfinity, SettledAtSeconds = float.NaN };
            int steps = Mathf.RoundToInt(seconds / Dt);
            for (int k = 1; k <= steps; k++)
            {
                float target = targetDistance + targetSpeed * Dt * k;
                var actors = new List<TrafficActorInput> { ActorAt(Agent, bench, x, v) };
                var hazards = new List<TrafficHazardInput>();
                if (obstacle) hazards.Add(BoxAt(HazardId, bench, target, 0f, Vector3.zero));
                else actors.Add(ActorAt(Other, bench, target, targetSpeed));
                var step = Evaluate(bench, route, (ulong)k, actors, hazards, x, v, memory, driver);
                route = step.Decision.Route.Plan;
                memory = step.Longitudinal.Memory;
                Assert.That(step.Perception.UnavailableReason, Is.EqualTo(PerceptionUnavailableReason.None), "pas " + k);
                float gap = obstacle ? (step.Perception.Obstacles.Count > 0 ? step.Perception.Obstacles[0].NearDistanceMeters : float.NaN)
                    : (step.Perception.HasLeader ? step.Perception.Leader.GapMeters : float.NaN);
                Assert.That(float.IsNaN(gap), Is.False, "cible percue au pas " + k);
                result.MinimumGap = Math.Min(result.MinimumGap, gap);
                if (k * Dt > seconds - 5f) result.MinimumSteadyGap = Math.Min(result.MinimumSteadyGap, gap);
                result.FinalGap = gap;
                float a = step.Longitudinal.AppliedAccelerationMetersPerSecondSquared;
                Assert.That(float.IsNaN(a) || float.IsInfinity(a), Is.False);
                if (!step.Longitudinal.Hold.Active && a > 0f && v > 0f && v < TrafficV2Settings.StopHold.EntrySpeedMetersPerSecond
                    && LongitudinalArbitration.IsInteraction(step.Longitudinal.Binding.Constraint))
                    result.CrawlMeters += v * Dt;
                v = Math.Max(0f, v + a * Dt);
                x += v * Dt;
                result.FinalSpeed = v;
                result.Steps = k;
                // Arret maintenu (D11) : vitesse nulle sous StopHold, plus d'approche asymptotique de s0.
                settled = step.Longitudinal.Hold.Active && v < 0.01f ? settled + 1 : 0;
                if (stopWhenSettled && settled >= 100) { result.SettledAtSeconds = k * Dt; break; }
            }
            return result;
        }

        [Test]
        public void APointMassFollowerHoldsAStopInsideTheWindowBehindAStoppedLeaderMeasuredByThePerception()
        {
            var driver = Driver;
            var stopped = Simulate(40f, 0f, false, 60f, true);
            Debug.Log("[Story533] banc leader arrete : jeu maintenu " + stopped.FinalGap + " m, min " + stopped.MinimumGap
                + " m, arret maintenu a " + stopped.SettledAtSeconds + " s (" + stopped.Steps + " pas), rampement " + stopped.CrawlMeters + " m.");
            AssertHeldInsideTheWindow(driver, stopped);
        }

        [Test]
        public void APointMassFollowerHoldsAStopInsideTheWindowBeforeTheNearFaceOfAFixedObstacle()
        {
            var driver = Driver;
            var obstacle = Simulate(40f, 0f, true, 60f, true);
            Debug.Log("[Story533] banc obstacle fixe : distance maintenue " + obstacle.FinalGap + " m, min " + obstacle.MinimumGap
                + " m, arret maintenu a " + obstacle.SettledAtSeconds + " s, rampement " + obstacle.CrawlMeters + " m.");
            AssertHeldInsideTheWindow(driver, obstacle);
        }

        /// <summary>Arret maintenu en au plus 60 s, jeu dans [s0 - 0,10 ; s0 + Delta_hold], sans rampement.</summary>
        private static void AssertHeldInsideTheWindow(DriverProfile driver, Simulation run)
        {
            Assert.That(float.IsNaN(run.SettledAtSeconds), Is.False, "arret maintenu en au plus 60 s");
            Assert.That(run.FinalGap, Is.InRange(driver.MinimumGap - 0.10f, driver.MinimumGap + TrafficV2Settings.StopHoldGapMarginMeters));
            Assert.That(run.MinimumGap, Is.GreaterThanOrEqualTo(driver.MinimumGap - 0.10f), "jamais sous s0 - 0,10 m");
            Assert.That(run.CrawlMeters, Is.LessThanOrEqualTo(0.01f), "aucun rampement vers s0");
        }

        [Test]
        public void BehindASlowerMovingLeaderTheSteadyGapNeverGoesBelowTheMinimumGap()
        {
            var driver = Driver;
            var moving = Simulate(12f, 2f, false, 14f, false);
            Debug.Log("[Story533] banc leader a 2 m/s : jeu final " + moving.FinalGap + " m, min en regime " + moving.MinimumSteadyGap
                + " m, v finale " + moving.FinalSpeed + " m/s.");
            Assert.That(moving.MinimumGap, Is.GreaterThanOrEqualTo(driver.MinimumGap - EditModeTolerance));
            Assert.That(moving.MinimumSteadyGap, Is.GreaterThanOrEqualTo(driver.MinimumGap), "jeu jamais sous s0 en regime");
            Assert.That(moving.FinalSpeed, Is.EqualTo(2f).Within(0.3f), "suivi du leader en regime");
        }

        // ================================================================== maintien a l'arret (D11)

        private static LongitudinalPerception ObstacleOnly(float near, float speedAlong, PerceivedObstacleKind kind = PerceivedObstacleKind.Obstacle)
        {
            return new LongitudinalPerception(PerceptionUnavailableReason.None, null,
                new[] { new LongitudinalObstacle(HazardId, kind, near, speedAlong) });
        }

        [Test]
        public void TheStopHoldEntersOnlyQuasiStoppedInsideTheWindowAndNamesItsCause()
        {
            var driver = Driver;
            var stopHold = TrafficV2Settings.StopHold;
            var plan = PlanAt(1f, 0f, driver);
            float s0 = driver.MinimumGap, window = s0 + stopHold.HoldGapMarginMeters;
            float slow = stopHold.EntrySpeedMetersPerSecond * 0.5f;
            var slowPlan = PlanAt(1f, slow, driver);

            var entered = Decide(slowPlan, driver, slow, Dt, LeaderOnly(window - 0.01f, 0f), LongitudinalMemory.None);
            Assert.That(entered.Hold.Phase, Is.EqualTo(StopHoldPhase.Entered));
            Assert.That(entered.Hold.Cause, Is.EqualTo(LongitudinalCandidateKind.LeaderFollowing));
            Assert.That(entered.Hold.SourceId, Is.EqualTo(Other));
            Assert.That(entered.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.StopHold), "cause explicite");
            Assert.That(entered.Binding.Constraint, Is.EqualTo(SpeedConstraint.LeaderFollowing));
            Assert.That(entered.Binding.SourceId, Is.EqualTo(Other));
            Assert.That(entered.TargetAccelerationMetersPerSecondSquared, Is.LessThanOrEqualTo(-driver.ComfortableDeceleration),
                "arret explicite, jamais une approche");
            Assert.That(entered.Memory.Holding, Is.True);
            StringAssert.Contains("StopHold Entered LeaderFollowing", entered.ToText());
            var blocker = BlockerTracker.Update(null, entered, driver, 21).Single();
            Assert.That(blocker.Kind, Is.EqualTo(BlockerKind.Leader));
            Assert.That(blocker.BlockingActorOrRule, Is.EqualTo(Other.ToString()));

            var obstacle = Decide(plan, driver, 0f, Dt, ObstacleOnly(window - 0.2f, 0f), LongitudinalMemory.None);
            Assert.That(obstacle.Hold.Cause, Is.EqualTo(LongitudinalCandidateKind.Obstacle));
            Assert.That(obstacle.Binding.Constraint, Is.EqualTo(SpeedConstraint.Obstacle));
            Assert.That(BlockerTracker.Update(null, obstacle, driver, 22).Single().Kind, Is.EqualTo(BlockerKind.Obstacle));

            // Hors fenetre, trop rapide, ou liante qui n'est pas une interaction : aucun maintien, aucun blocker.
            var far = Decide(plan, driver, 0f, Dt, LeaderOnly(window + 0.01f, 0f), LongitudinalMemory.None);
            var fast = Decide(PlanAt(1f, 2f, driver), driver, stopHold.EntrySpeedMetersPerSecond * 1.5f, Dt, LeaderOnly(window - 0.01f, 0f),
                LongitudinalMemory.None);
            var unavailable = Decide(plan, driver, 0f, Dt, new LongitudinalPerception(PerceptionUnavailableReason.ChannelSaturated,
                new LongitudinalLeader(Other, window - 0.01f, 0f), null), LongitudinalMemory.None);
            foreach (var decision in new[] { far, fast, unavailable })
            {
                Assert.That(decision.Hold.Phase, Is.EqualTo(StopHoldPhase.None), decision.ToText());
                Assert.That(decision.Candidates.Any(c => c.Kind == LongitudinalCandidateKind.StopHold), Is.False);
                Assert.That(BlockerTracker.Update(null, decision, driver, 23), Is.Empty, decision.ToText());
            }
            Assert.That(far.TargetAccelerationMetersPerSecondSquared, Is.GreaterThan(0f), "hors fenetre, l'IDM conduit l'approche");
            // Desactive : l'IDM seul, aucune etape de maintien.
            var disabled = LongitudinalArbitration.Decide(slowPlan, driver, slow, Dt, LeaderOnly(window - 0.01f, 0f), LongitudinalMemory.None,
                StopHoldParameters.Disabled);
            Assert.That(disabled.Hold.Phase, Is.EqualTo(StopHoldPhase.None));
            Assert.Throws<ArgumentException>(() => new StopHoldParameters(0.5f, 1f, 1f, 0.5f), "Delta_release > Delta_hold");
        }

        [Test]
        public void TheStopHoldReleasesOnlyWithItsHysteresisAndTheRecoveryStartsFromRest()
        {
            var driver = Driver;
            var stopHold = TrafficV2Settings.StopHold;
            var plan = PlanAt(1f, 0f, driver);
            float s0 = driver.MinimumGap;
            float holdGap = s0 + stopHold.HoldGapMarginMeters * 0.5f;
            var entered = Decide(plan, driver, 0f, Dt, LeaderOnly(holdGap, 0f), LongitudinalMemory.None);
            var held = entered.Memory;
            Assert.That(held.Holding, Is.True);
            Func<LongitudinalPerception, LongitudinalDecision> next = perception => Decide(plan, driver, 0f, Dt, perception, held);
            var blind = new LongitudinalPerception(PerceptionUnavailableReason.ChannelUnavailable, null, null);

            // Perception indisponible pendant le maintien : la source n'est pas vue, son jeu est inconnu ; le blocker garde
            // ses proprietes (arret voulu derriere un leader, obstacle mobile) et sa continuite, jamais un encastrement.
            var leaderBefore = BlockerTracker.Update(null, entered, driver, 29);
            var leaderBlind = BlockerTracker.Update(leaderBefore, next(blind), driver, 30).Single();
            Assert.That(leaderBefore.Single().Legitimate, Is.True);
            Assert.That(leaderBlind.Legitimate, Is.True, leaderBlind.ToString());
            Assert.That(leaderBlind.ExpectedToClear, Is.True);
            Assert.That(leaderBlind.Recoverable, Is.False);
            Assert.That(leaderBlind.SinceFrame, Is.EqualTo(29UL));
            var obstacleEntered = Decide(plan, driver, 0f, Dt, ObstacleOnly(holdGap, 0f, PerceivedObstacleKind.TrafficActor), LongitudinalMemory.None);
            Assert.That(obstacleEntered.Memory.Holding, Is.True);
            var obstacleBlind = BlockerTracker.Update(BlockerTracker.Update(null, obstacleEntered, driver, 29),
                Decide(plan, driver, 0f, Dt, blind, obstacleEntered.Memory), driver, 30).Single();
            Assert.That(obstacleBlind.Kind, Is.EqualTo(BlockerKind.Obstacle));
            Assert.That(obstacleBlind.ExpectedToClear, Is.True, "obstacle mobile : liberation attendue");
            Assert.That(obstacleBlind.Recoverable, Is.False);
            Assert.That(obstacleBlind.SinceFrame, Is.EqualTo(29UL));

            // Mouvements de la source sans ouverture suffisante : le maintien tient (aucun stop-and-go au premier frisson).
            foreach (var still in new[]
            {
                LeaderOnly(holdGap, 0f),
                LeaderOnly(s0 + stopHold.ReleaseGapMarginMeters - 0.01f, 0f),
                LeaderOnly(s0 + stopHold.HoldGapMarginMeters + 0.3f, stopHold.SourceDepartureSpeedMetersPerSecond * 0.5f),
                LeaderOnly(s0 + stopHold.HoldGapMarginMeters, stopHold.SourceDepartureSpeedMetersPerSecond * 2f),
                blind
            })
            {
                var decision = next(still);
                Assert.That(decision.Hold.Phase, Is.EqualTo(StopHoldPhase.Holding), decision.ToText());
                Assert.That(decision.TargetAccelerationMetersPerSecondSquared, Is.LessThan(0f));
                Assert.That(BlockerTracker.Update(null, decision, driver, 30).Single().Kind, Is.EqualTo(BlockerKind.Leader),
                    "blocker continu pendant le maintien");
            }

            // Liberations : source disparue d'une perception disponible, jeu ouvert, source repartie avec degagement.
            var gone = next(Nothing());
            var opened = next(LeaderOnly(s0 + stopHold.ReleaseGapMarginMeters, 0f));
            var departed = next(LeaderOnly(s0 + stopHold.HoldGapMarginMeters + 0.05f, stopHold.SourceDepartureSpeedMetersPerSecond));
            Assert.That(gone.Hold.Release, Is.EqualTo(StopHoldRelease.SourceGone));
            Assert.That(opened.Hold.Release, Is.EqualTo(StopHoldRelease.GapOpened));
            Assert.That(departed.Hold.Release, Is.EqualTo(StopHoldRelease.SourceDeparted));
            foreach (var released in new[] { gone, opened, departed })
            {
                Assert.That(released.Hold.Phase, Is.EqualTo(StopHoldPhase.Released));
                Assert.That(released.Memory.Holding, Is.False);
                Assert.That(BlockerTracker.Update(null, released, driver, 31), Is.Empty, "cause disparue : plus de blocker");
                // Reprise lissee depuis l'arret (0), jamais depuis la deceleration du maintien.
                Assert.That(released.AppliedAccelerationMetersPerSecondSquared, Is.GreaterThan(0f), released.ToText());
                Assert.That(released.AppliedAccelerationMetersPerSecondSquared, Is.LessThanOrEqualTo(released.TargetAccelerationMetersPerSecondSquared));
                Assert.That(released.Smoothed, Is.True);
            }
            // Un autre leader dans la fenetre quand la source disparait : le maintien reprend sur la nouvelle cause.
            var cutIn = next(new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(new RoadId(7, 7), holdGap, 0f), null));
            Assert.That(cutIn.Hold.Phase, Is.EqualTo(StopHoldPhase.Entered));
            Assert.That(cutIn.Hold.SourceId, Is.EqualTo(new RoadId(7, 7)));
        }

        [TestCase(PerceptionUnavailableReason.ChannelUnavailable)]
        [TestCase(PerceptionUnavailableReason.ChannelSaturated)]
        [TestCase(PerceptionUnavailableReason.HazardCollectorSaturated)]
        public void AnUnavailablePerceptionNeverReleasesAVisibleStopHoldSource(PerceptionUnavailableReason reason)
        {
            var driver = Driver;
            var settings = TrafficV2Settings.StopHold;
            var plan = PlanAt(1f, 0f, driver);
            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(
                "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset").Profile;
            foreach (bool obstacle in new[] { false, true })
                foreach (bool departure in new[] { false, true })
                {
                    float holdGap = driver.MinimumGap + settings.HoldGapMarginMeters * 0.5f;
                    var entered = Decide(plan, driver, 0f, Dt,
                        obstacle ? ObstacleOnly(holdGap, 0f, PerceivedObstacleKind.TrafficActor) : LeaderOnly(holdGap, 0f),
                        LongitudinalMemory.None);
                    Assert.That(entered.Hold.Active, Is.True);
                    var before = BlockerTracker.Update(null, entered, driver, 29);
                    float gap = driver.MinimumGap + (departure ? settings.HoldGapMarginMeters + 0.05f : settings.ReleaseGapMarginMeters);
                    float speed = departure ? settings.SourceDepartureSpeedMetersPerSecond : 0f;
                    var saturated = new LongitudinalPerception(reason,
                        obstacle ? (LongitudinalLeader?)null : new LongitudinalLeader(Other, gap, speed),
                        obstacle ? new[] { new LongitudinalObstacle(HazardId, PerceivedObstacleKind.TrafficActor, gap, speed) } : null);

                    var held = Decide(plan, driver, 0f, Dt, saturated, entered.Memory);
                    Assert.That(held.Hold.Phase, Is.EqualTo(StopHoldPhase.Holding), held.ToText());
                    Assert.That(held.Hold.Release, Is.EqualTo(StopHoldRelease.None));
                    Assert.That(held.Hold.SourceId, Is.EqualTo(entered.Hold.SourceId));
                    Assert.That(held.AppliedAccelerationMetersPerSecondSquared, Is.LessThan(0f));
                    var blocker = BlockerTracker.Update(before, held, driver, 30).Single();
                    Assert.That(blocker.SinceFrame, Is.EqualTo(29UL));
                    Assert.That(blocker.BlockingActorOrRule, Is.EqualTo(before.Single().BlockingActorOrRule));
                    var command = new MotionCommand(30, 30, 30, held.AppliedAccelerationMetersPerSecondSquared, 0f, held.Binding.Constraint);
                    var composed = new VehicleDriveIntentComposer(vehicle, driver.SafeBrakingLimit, Dt)
                        .Compose(30, command, V2FallbackReason.None, 0f, 0f);
                    Assert.That(composed.Fallback, Is.False);
                    Assert.That(composed.Intent.Handbrake, Is.EqualTo(1f), "le maintien physique tient aussi");

                    var available = new LongitudinalPerception(PerceptionUnavailableReason.None,
                        saturated.HasLeader ? (LongitudinalLeader?)saturated.Leader : null, saturated.Obstacles);
                    var released = Decide(plan, driver, 0f, Dt, available, held.Memory);
                    Assert.That(released.Hold.Release, Is.EqualTo(departure ? StopHoldRelease.SourceDeparted : StopHoldRelease.GapOpened));
                    Assert.That(released.Hold.Active, Is.False);
                    Assert.That(BlockerTracker.Update(new[] { blocker }, released, driver, 31), Is.Empty);
                }
        }

        [Test]
        public void ANegativeCandidateWhileMovingIsNeverABlocker()
        {
            var driver = Driver;
            // Cas observe au scenario A (vehicule 3, frame 782 : 3,72 m/s, jeu 10,6 m, candidat -0,007) et plus franc.
            foreach (float speed in new[] { 3.72f, 5f, 6f })
            {
                var decision = Decide(PlanAt(1f, speed, driver), driver, speed, Dt, LeaderOnly(speed < 4f ? 10.6f : 8f, 0f), LongitudinalMemory.None);
                Assert.That(decision.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.LeaderFollowing));
                Assert.That(decision.TargetAccelerationMetersPerSecondSquared, Is.LessThan(0f), "v = " + speed);
                Assert.That(decision.Hold.Phase, Is.EqualTo(StopHoldPhase.None));
                Assert.That(BlockerTracker.Update(null, decision, driver, 40), Is.Empty, "v = " + speed + " : freinage, pas immobilisation");
            }
        }

        /// <summary>Bande de service du composeur v_s = v_dir + 2 b dt (0,41 m/s) : une deceleration commandee n'y est pas freinee.</summary>
        private const float ComposerServiceBandMetersPerSecond = 0.41f;

        /// <summary>
        /// Arret en roue libre dans la bande de service, mesure en PlayMode (acceptance-A 20261002-182834, vehicule 2, frames
        /// 930-936 : 0,249 -> 0,009 m/s en 0,12 s). C'est cet arret anticipe, puis la reprise IDM, qui produisait le rampement.
        /// </summary>
        private const float CoastDecelerationMetersPerSecondSquared = 2f;

        private const string CalibrationReportPath = "_bmad-output/implementation-artifacts/traffic-v2-5-33-explorations/stophold-calibration.md";

        private sealed class HoldRun
        {
            public float EntryGap = float.NaN, HeldGap = float.NaN, MinimumGap = float.PositiveInfinity, CrawlMeters;
            public float ReleaseDelaySeconds = float.NaN;
            public StopHoldRelease FirstRelease;
            public int Entries, Releases, EntriesAfterDeparture;
            public readonly List<StopHoldRelease> Reasons = new List<StopHoldRelease>();
        }

        /// <summary>
        /// Suiveur point-masse derriere un leader synthetique, arbitrage seul : approche a v0 depuis 40 m d'un leader arrete,
        /// puis vitesse du leader donnee a partir de <paramref name="departAt"/>. Physique : l'acceleration commandee, sauf
        /// une deceleration dans la bande de service, ou la caisse s'arrete en roue libre comme en PlayMode. Rampement :
        /// distance parcourue sous la vitesse d'entree avec une acceleration &gt; 0 liee a une interaction, hors maintien,
        /// derriere une source qui ne repart pas.
        /// </summary>
        private static HoldRun RunHold(StopHoldParameters parameters, float approachSpeed, float departAt, float seconds,
            Func<float, float> leaderSpeedAfterDeparture)
        {
            var driver = Driver;
            var plan = PlanAt(1f, driver.DesiredSpeed, driver);
            var band = TrafficV2Settings.StopHold;
            var run = new HoldRun();
            var memory = LongitudinalMemory.None;
            float gap = 40f, v = approachSpeed;
            int steps = Mathf.RoundToInt(seconds / Dt);
            for (int k = 0; k < steps; k++)
            {
                float t = k * Dt;
                float leader = t < departAt ? 0f : leaderSpeedAfterDeparture(t - departAt);
                var decision = LongitudinalArbitration.Decide(plan, driver, v, Dt, new LongitudinalPerception(PerceptionUnavailableReason.None,
                    new LongitudinalLeader(Other, gap, leader), null), memory, parameters);
                memory = decision.Memory;
                if (decision.Hold.Phase == StopHoldPhase.Entered)
                {
                    run.Entries++;
                    if (float.IsNaN(run.EntryGap)) run.EntryGap = gap;
                    if (t >= departAt) run.EntriesAfterDeparture++;
                }
                if (decision.Hold.Phase == StopHoldPhase.Released)
                {
                    run.Releases++;
                    run.Reasons.Add(decision.Hold.Release);
                    if (t >= departAt && float.IsNaN(run.ReleaseDelaySeconds))
                    { run.ReleaseDelaySeconds = t - departAt; run.FirstRelease = decision.Hold.Release; }
                }
                float a = decision.AppliedAccelerationMetersPerSecondSquared;
                if (!decision.Hold.Active && a > 0f && v > 0f && v < band.EntrySpeedMetersPerSecond
                    && leader < band.SourceDepartureSpeedMetersPerSecond && LongitudinalArbitration.IsInteraction(decision.Binding.Constraint))
                    run.CrawlMeters += v * Dt;
                float physical = a < 0f && v <= ComposerServiceBandMetersPerSecond ? -CoastDecelerationMetersPerSecondSquared : a;
                v = Math.Max(0f, v + physical * Dt);
                gap += (leader - v) * Dt;
                run.MinimumGap = Math.Min(run.MinimumGap, gap);
                if (t < departAt) run.HeldGap = gap;
            }
            return run;
        }

        private static string G(float value)
        {
            return float.IsNaN(value) ? "-" : value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        [Test]
        public void TheStopHoldWindowIsCalibratedOnThePointMassBench()
        {
            var driver = Driver;
            float s0 = driver.MinimumGap;
            var settings = TrafficV2Settings.StopHold;
            float entry = settings.EntrySpeedMetersPerSecond, departure = settings.SourceDepartureSpeedMetersPerSecond;
            var approaches = new[] { 1f, 2f, 4f, 6f, 8f };
            const float DepartAt = 30f;
            Func<float, float> departs = t => Math.Min(8f, 1.5f * t);
            Func<float, float> creeps = t => 0.05f;
            Func<float, float> twitches = t => t < 0.7f ? 0.3f : 0f;
            Func<float, float> stopAndGo = t => t < 2f ? 1.5f * t : Math.Max(0f, 3f - 2f * (t - 2f));

            var sets = new List<KeyValuePair<string, StopHoldParameters>>
                { new KeyValuePair<string, StopHoldParameters>("IDM seul (maintien desactive)", StopHoldParameters.Disabled) };
            foreach (float hold in new[] { 0.25f, 0.5f })
                foreach (float release in new[] { 1f, 1.5f, 2f })
                    sets.Add(new KeyValuePair<string, StopHoldParameters>(
                        "Delta_hold " + G(hold) + " / Delta_release " + G(release), new StopHoldParameters(entry, hold, release, departure)));

            var text = new StringBuilder();
            text.Append("# Calibration du maintien a l'arret D11 (Story 5.33)\n\n");
            text.Append("Banc point-masse, arbitrage seul, leader synthetique ; s0 ").Append(G(s0)).Append(" m, vitesse d'entree et de depart ")
                .Append(G(entry)).Append(" m/s (VehicleTireModel.SlipReferenceSpeed). Physique : acceleration commandee, roue libre a ")
                .Append(G(CoastDecelerationMetersPerSecondSquared)).Append(" m/s2 sous la bande de service ").Append(G(ComposerServiceBandMetersPerSecond))
                .Append(" m/s (mesure PlayMode). Approche depuis 40 m d'un leader arrete ; depart du leader a ").Append(G(DepartAt)).Append(" s.\n\n");
            text.Append("Rampement : distance sous la vitesse d'entree, acceleration > 0 liee a une interaction, hors maintien, source arretee.\n\n");
            text.Append("| Parametres | v0 (m/s) | jeu a l'entree | jeu maintenu | jeu min | rampement (m) | liberation au depart | delai (s) | re-entrees apres depart |\n");
            text.Append("|---|---|---|---|---|---|---|---|---|\n");
            foreach (var set in sets)
                foreach (float v0 in approaches)
                {
                    var r = RunHold(set.Value, v0, DepartAt, DepartAt + 10f, departs);
                    text.Append("| ").Append(set.Key).Append(" | ").Append(G(v0)).Append(" | ").Append(G(r.EntryGap)).Append(" | ").Append(G(r.HeldGap))
                        .Append(" | ").Append(G(r.MinimumGap)).Append(" | ").Append(G(r.CrawlMeters)).Append(" | ").Append(r.FirstRelease)
                        .Append(" | ").Append(G(r.ReleaseDelaySeconds)).Append(" | ").Append(r.EntriesAfterDeparture).Append(" |\n");
                }
            text.Append("\n| Parametres | leader rampant 0,05 m/s pendant 60 s : liberations / rampement (m) | frisson 0,21 m : liberations | arret-depart du leader : liberations / re-entrees / rampement (m) |\n");
            text.Append("|---|---|---|---|\n");
            foreach (var set in sets.Skip(1))
            {
                var creep = RunHold(set.Value, 4f, DepartAt, DepartAt + 60f, creeps);
                var twitch = RunHold(set.Value, 4f, DepartAt, DepartAt + 10f, twitches);
                var go = RunHold(set.Value, 4f, DepartAt, DepartAt + 15f, stopAndGo);
                text.Append("| ").Append(set.Key).Append(" | ").Append(creep.Releases).Append(" / ").Append(G(creep.CrawlMeters))
                    .Append(" | ").Append(twitch.Releases).Append(" | ").Append(go.Releases).Append(" / ").Append(go.EntriesAfterDeparture)
                    .Append(" / ").Append(G(go.CrawlMeters)).Append(" |\n");
            }
            text.Append("\nRetenu (TrafficV2Settings.StopHold) : Delta_hold ").Append(G(settings.HoldGapMarginMeters)).Append(" m, Delta_release ")
                .Append(G(settings.ReleaseGapMarginMeters)).Append(" m.\n");
            Directory.CreateDirectory(Path.GetDirectoryName(CalibrationReportPath));
            File.WriteAllText(CalibrationReportPath, text.ToString());
            Debug.Log("[Story533] calibration StopHold ecrite : " + CalibrationReportPath);

            // Le defaut que D11 corrige est reproduit par le banc : l'IDM seul rampe vers s0.
            Assert.That(approaches.Max(v0 => RunHold(StopHoldParameters.Disabled, v0, DepartAt, DepartAt, departs).CrawlMeters), Is.GreaterThan(0.05f),
                "le banc reproduit le rampement de l'IDM seul");
            // Valeurs retenues : arret dans la fenetre d'acceptation sans rampement, liberation unique au depart du leader.
            foreach (float v0 in approaches)
            {
                var r = RunHold(settings, v0, DepartAt, DepartAt + 10f, departs);
                Assert.That(r.Entries, Is.GreaterThanOrEqualTo(1), "v0 " + v0);
                Assert.That(r.HeldGap, Is.InRange(s0 - 0.10f, s0 + 0.50f), "jeu maintenu, v0 " + v0);
                Assert.That(r.MinimumGap, Is.GreaterThanOrEqualTo(s0 - 0.10f), "v0 " + v0);
                Assert.That(r.CrawlMeters, Is.LessThanOrEqualTo(0.01f), "aucun rampement, v0 " + v0);
                Assert.That(r.FirstRelease, Is.EqualTo(StopHoldRelease.SourceDeparted), "v0 " + v0);
                Assert.That(r.ReleaseDelaySeconds, Is.LessThanOrEqualTo(1f), "reprise derriere un leader qui repart, v0 " + v0);
                Assert.That(r.EntriesAfterDeparture, Is.Zero, "aucun stop-and-go au depart, v0 " + v0);
            }
            var twitchRun = RunHold(settings, 4f, DepartAt, DepartAt + 10f, twitches);
            Assert.That(twitchRun.Releases, Is.Zero, "un frisson du leader ne libere pas le maintien");
            var creepRun = RunHold(settings, 4f, DepartAt, DepartAt + 60f, creeps);
            Assert.That(creepRun.Reasons.All(r => r == StopHoldRelease.GapOpened), Is.True, "leader rampant : liberation par ouverture seulement");
            Assert.That(creepRun.Releases, Is.LessThanOrEqualTo(1 + (int)(60f * 0.05f / (settings.ReleaseGapMarginMeters - settings.HoldGapMarginMeters))),
                "cycles bornes par l'hysteresis");
            var goRun = RunHold(settings, 4f, DepartAt, DepartAt + 15f, stopAndGo);
            Assert.That(goRun.Releases, Is.EqualTo(1));
            Assert.That(goRun.EntriesAfterDeparture, Is.EqualTo(1), "re-maintien unique quand le leader s'arrete de nouveau");
            Assert.That(goRun.CrawlMeters, Is.LessThanOrEqualTo(0.01f));
        }

        // ================================================================== projection

        [Test]
        public void TheExtendedProjectionTextIsDeterministicAndCultureInvariant()
        {
            var bench = Straight;
            var driver = Driver;
            var step = Evaluate(bench, bench.Route, 12, new[] { ActorAt(Agent, bench, 2f, 3.25f), ActorAt(Other, bench, 14f, 1.5f) },
                new[] { BoxAt(HazardId, bench, 30f, 0f, Vector3.zero, TrafficHazardKind.WalkingPlayer) }, 2f, 3.25f,
                LongitudinalMemory.None, driver);
            var blockers = BlockerTracker.Update(null, step.Longitudinal, driver, 12);
            var counters = new TrafficHazardCollectorCounters(3, 1, 57, 2, 40, 3, 2, 1, 0);
            Func<string> render = () => step.Decision.Projection
                .WithDrive(new TrafficDriveOutcome(12, 5, 12, 0.25f, -0.5f, 0f, 0f, false, null, "MaxAcceleration",
                    new[] { "DesiredSpeed 8" }, new string[0], VehicleCoverage.Covered, null))
                .WithLongitudinal(new TrafficLongitudinalOutcome(12, step.Observation, step.Longitudinal, step.Plan.RoadLimits,
                    blockers, 57, true, counters)).ToText();
            var culture = Thread.CurrentThread.CurrentCulture;
            string invariant, french;
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                invariant = render();
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                french = render();
            }
            finally { Thread.CurrentThread.CurrentCulture = culture; }
            Assert.That(french, Is.EqualTo(invariant), "invariant de culture");
            Assert.That(render(), Is.EqualTo(invariant), "deterministe");
            foreach (var expected in new[] { "Speed constraints applied", "Speed constraints deferred", "Observation \nFrame 12",
                "Leader Evaluated", "Obstacles Evaluated", "Longitudinal binding", "Longitudinal candidates", "Road limits",
                "Unauthored", "Blockers", "Dominant", "Hazard query 57 saturee", "HazardIdentityCollision 1" })
                StringAssert.Contains(expected, invariant);
            Assert.That(step.Decision.Projection.WithLongitudinal(null).Longitudinal, Is.Null);
            Assert.That(step.Decision.Projection.Longitudinal, Is.Null, "la decision d'origine est inchangee");
        }

        // ================================================================== gardes structurelles

        [Test]
        public void PlanningAndBlockersUseNoPhysicsNorControlPathAndDeclareNoPedalMember()
        {
            var files = new List<string>();
            foreach (var folder in new[] { "Planning", "Blockers" })
                files.AddRange(Directory.GetFiles(Path.Combine(TrafficRootPath, folder), "*.cs"));
            Assert.That(files.Count, Is.GreaterThanOrEqualTo(9));
            foreach (var file in files)
            {
                string source = File.ReadAllText(file);
                foreach (var forbidden in new[] { "VehicleDriveIntent", "VehiclePhysicsBody", "ApplyDriveIntent", "Rigid" + "body",
                    "Mono" + "Behaviour", "NetworkVariable", "Rp" + "c", "LateralClearanceMarginMeters", "Physics" + "." })
                    Assert.That(source, Does.Not.Contain(forbidden), file);
            }
            var namespaces = new[] { typeof(SpeedPlan).Namespace, typeof(Blocker).Namespace };
            var types = typeof(SpeedPlan).Assembly.GetTypes().Where(t => t.IsPublic && namespaces.Contains(t.Namespace)).ToList();
            Assert.That(types.Count, Is.GreaterThan(15));
            foreach (var type in types)
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    foreach (var word in new[] { "Brake", "Throttle", "Pedal" })
                        Assert.That(member.Name, Does.Not.Contain(word), type.Name);
        }
    }
}
