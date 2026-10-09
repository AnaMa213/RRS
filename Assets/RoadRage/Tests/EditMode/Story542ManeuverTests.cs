using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Collisions;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Policy;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Features.Vehicles.Traffic.Safety;
using RoadRage.Features.Vehicles.Traffic.Tactical;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.42 : manoeuvres non structurees. Modeles synthetiques a double sens ecrits a la main (corridor large, etroit,
    /// avec ou sans corridor adjacent de meme sens), profil de drivabilite et conducteur authores de MVP_Run, et mesure sur le
    /// modele MVP_Run committe. Le banc bicyclette suit la loi de guidage reelle (patron Story518, sans pneus) sur la reference
    /// de manoeuvre ; il repond a ce que decide la loi de suivi, pas a la physique du vehicule.
    /// </summary>
    [Category("Core")]
    [Category("Story542")]
    public sealed class Story542ManeuverTests
    {
        private const string TrafficRootPath = "Assets/RoadRage/Features/Vehicles/Traffic";
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const float Dt = 0.02f;
        private const float Epsilon = 0.34f;
        private const float CorridorLength = 140f;
        /// <summary>
        /// Conducteur de fixture a 6 m/s (transitions d'environ 20 m) : le temoin C1 du validateur derive d'environ 2e-4 m par
        /// metre (integration flottante par pas de 1 cm), un corridor synthetique au-dela de ~200 m n'est pas compilable.
        /// </summary>
        private const float DesiredSpeed = 6f;

        private static readonly RoadId Self = new RoadId(0x542UL, 1UL);
        private static readonly RoadId Stalled = new RoadId(0x542UL, 2UL);
        private static readonly RoadId Oncoming = new RoadId(0x542UL, 3UL);
        private static readonly RoadId HazardId = new RoadId(TrafficV2HazardCollector.HazardIdentityDomain, 42UL);

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };
        private static readonly PerceptionLimits Perception = new PerceptionLimits(30f, 3f, 15f, 8);
        private static readonly GaugeBox Gauge = new GaugeBox(1.03f, 4.5f, 0f, 1.5f);

        private static DriverProfileDef Def { get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath); } }

        private static RoadId Id(int index) { return new RoadId(0x5420542000000000UL | (uint)index, 0x0542054200000000UL | (uint)index); }

        private static readonly RoadId Section = Id(1);
        private static readonly RoadId Own = Id(2);
        private static readonly RoadId Opposing = Id(3);
        private static readonly RoadId Adjacent = Id(4);

        // ================================================================== matrice

        [Test]
        public void AWideCorridorSelectsTheInCorridorOffsetWithoutAnyRequest()
        {
            var model = TwoWay(4.5f, false);
            var frame = Frame(model, 10f, hazards: new[] { Box(TrafficHazardKind.Obstacle, model, 50f, 0f, new Vector3(0.5f, 0.75f, 1f)) });
            var result = Evaluate(frame, 10f, HazardId, PerceivedObstacleKind.Obstacle);

            var offset = Candidate(result, ManeuverKind.CorridorOffset);
            Assert.That(offset.Verdict, Is.EqualTo(ManeuverVerdict.Selected), result.ToText());
            Assert.That(result.Ready, Is.True);
            Assert.That(offset.RequiresException, Is.False);
            // 2,37 m (degagement 0,5 + demi-largeur 1,03 + epsilon_t 0,34 depuis le bord de l'obstacle) plus le reste de preuve.
            Assert.That(Math.Abs(offset.Path.TargetOffsetMeters), Is.EqualTo(2.37f + TrafficV2Settings.ManeuverProofStepMeters).Within(0.01f));
            Assert.That(Math.Abs(offset.Path.TargetOffsetMeters) + 1.03f + Epsilon, Is.LessThanOrEqualTo(4.5f), "dans le corridor de 9 m");
            Assert.That(Candidate(result, ManeuverKind.AdjacentCorridor).Verdict, Is.EqualTo(ManeuverVerdict.NotOffered));
            Assert.That(Candidate(result, ManeuverKind.AuthorizedSurface).Verdict, Is.EqualTo(ManeuverVerdict.NotOffered));
        }

        [Test]
        public void ANarrowCorridorRejectsTheOffsetAndTheOpposingCorridorWaitsForTheException()
        {
            var model = TwoWay(2f, false);
            var frame = Frame(model, 10f, stalledAt: 50f);
            var result = Evaluate(frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor);

            var offset = Candidate(result, ManeuverKind.CorridorOffset);
            Assert.That(offset.Verdict, Is.EqualTo(ManeuverVerdict.GeometryInfeasible), result.ToText());
            Assert.That(offset.GeometryCause, Is.EqualTo(ManeuverGeometryCause.NoLateralRoom));
            var opposing = Candidate(result, ManeuverKind.OpposingCorridor);
            Assert.That(opposing.Verdict, Is.EqualTo(ManeuverVerdict.ExceptionPending), result.ToText());
            Assert.That(opposing.RequiresException, Is.True);
            Assert.That(opposing.OtherCorridorId, Is.EqualTo(Opposing));
            Assert.That(result.Selected, Is.SameAs(opposing));
            Assert.That(result.Ready, Is.False, "aucun depart sans exception effective");
            Assert.That(opposing.Path.TargetOffsetMeters, Is.EqualTo(-4f).Within(0.01f));
        }

        [Test]
        public void APolicyThatDoesNotAllowTheOpposingCorridorRefusesItAndProposesNothing()
        {
            var model = TwoWay(2f, false);
            var frame = Frame(model, 10f, stalledAt: 50f);
            var unwilling = new DrivingPolicyProfile(0.3f, 4f, 1f, DrivingSurface.Carriageway, new ManeuverPreference(true, 1f),
                new ManeuverPreference(true, 1f), new ManeuverPreference(true, 1f), new ManeuverPreference(false, 1f));
            var policy = DrivingPolicy.Resolve(Def.Profile.WithDesiredSpeed(DesiredSpeed), unwilling, Self, 1UL);
            var result = Evaluate(frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor, policy: policy);

            var opposing = Candidate(result, ManeuverKind.OpposingCorridor);
            Assert.That(opposing.Verdict, Is.EqualTo(ManeuverVerdict.PolicyRefused), result.ToText());
            Assert.That(opposing.PolicyRefusal, Is.EqualTo(ManeuverPolicyRefusal.Ineligible));
            Assert.That(result.Selected, Is.Null);
            RuleExceptionRequest request;
            Assert.That(DrivingPolicy.TryPropose(policy, TrafficRule.OpposingCorridor, Opposing, RoadId.None, "x",
                new RuleExceptionTermination(RuleExceptionTerminationKind.ScopeExited, 10UL), 1UL, out request), Is.False);
        }

        [Test]
        public void ADeniedExceptionBlocksTheDeparture()
        {
            var model = TwoWay(2f, false);
            var frame = Frame(model, 10f, stalledAt: 50f);
            var result = Evaluate(frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor, denied: true);
            Assert.That(Candidate(result, ManeuverKind.OpposingCorridor).Verdict, Is.EqualTo(ManeuverVerdict.ExceptionDenied), result.ToText());
            Assert.That(result.Selected, Is.Null);
        }

        [Test]
        public void TheExceptionCycleRequestsAtNAndDepartsAtNPlusOneOnly()
        {
            var model = TwoWay(2f, false);
            var frame = Frame(model, 10f, stalledAt: 50f);
            var policy = FixturePolicy();
            var first = Evaluate(frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor, policy: policy, frameId: 5UL);
            var pending = first.Selected;
            RuleExceptionRequest request;
            Assert.That(DrivingPolicy.TryPropose(policy, TrafficRule.OpposingCorridor, pending.OtherCorridorId, RoadId.None,
                "Contournement de " + Stalled, new RuleExceptionTermination(RuleExceptionTerminationKind.ScopeExited,
                    pending.ExceptionExpiryFrame), 5UL, out request), Is.True);
            Assert.That(pending.ExceptionExpiryFrame, Is.GreaterThan(5UL).And.LessThanOrEqualTo(5UL + TrafficV2Settings.MaxRuleExceptionFrames));

            var snapshot = new JunctionCoordinator(model).Resolve(5UL, new[] { On(Self, Own), On(Stalled, Own) }, null, null,
                new[] { request });
            IReadOnlyList<EffectiveRuleException> effective;
            Assert.That(snapshot.TryGetEffectiveExceptions(Self, 5UL, out effective), Is.False, "jamais au lot de la demande");
            Assert.That(snapshot.TryGetEffectiveExceptions(Self, 6UL, out effective), Is.True, snapshot.ToText());

            var departure = Evaluate(frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor, policy: policy, frameId: 6UL,
                exceptions: effective);
            var opposing = Candidate(departure, ManeuverKind.OpposingCorridor);
            Assert.That(opposing.Verdict, Is.EqualTo(ManeuverVerdict.Selected), departure.ToText());
            Assert.That(departure.Ready, Is.True);

            var tactical = new TacticalDecision();
            Assert.That(tactical.SubmitManeuver(opposing, Stalled, 6UL).Accepted, Is.True);
            Assert.That(tactical.ManeuverActive, Is.True);
            Assert.That(tactical.ManeuverExceptionUntilMeters, Is.EqualTo(opposing.Proof.LastOtherDistanceMeters));
        }

        [Test]
        public void OpposingTrafficInsideThePolicyGapOrRiskRefusesTheDeparture()
        {
            var model = TwoWay(2f, false);
            // Vehicule en face sur le corridor oppose, en amont de la region, a 10 m/s.
            var frame = Frame(model, 10f, stalledAt: 50f, oncomingAtOpposingS: 60f, oncomingSpeed: 10f);
            var result = Evaluate(frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor);
            var opposing = Candidate(result, ManeuverKind.OpposingCorridor);
            Assert.That(opposing.Verdict, Is.EqualTo(ManeuverVerdict.TrafficConflict).Or.EqualTo(ManeuverVerdict.PolicyRefused),
                result.ToText());
            Assert.That(opposing.SlackSeconds, Is.LessThan(float.PositiveInfinity), "le trafic oppose est evalue");
            Assert.That(result.Selected, Is.Null);

            // Le meme vehicule tres lent : marge large, risque sous le seuil, candidat retenu en attente d'exception.
            var far = Frame(model, 10f, stalledAt: 50f, oncomingAtOpposingS: 5f, oncomingSpeed: 0.2f);
            var accepted = Evaluate(far, 10f, Stalled, PerceivedObstacleKind.TrafficActor);
            var survivor = Candidate(accepted, ManeuverKind.OpposingCorridor);
            Assert.That(survivor.Verdict, Is.EqualTo(ManeuverVerdict.ExceptionPending), accepted.ToText());
            Assert.That(survivor.SlackSeconds, Is.GreaterThanOrEqualTo(FixturePolicy().AcceptedGapSeconds));
        }

        [Test]
        public void AnArrivalDuringTheDepartureReturnsBehindTheCauseAndAfterThePointOfNoReturnCommits()
        {
            var model = TwoWay(2f, false);
            var frame = Frame(model, 10f, stalledAt: 50f);
            var situation = Situation(model, frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor);
            situation.Exceptions = Granted(model);
            var opposing = Candidate(ManeuverEvaluation.Evaluate(situation, 2UL), ManeuverKind.OpposingCorridor);
            Assert.That(opposing.Verdict, Is.EqualTo(ManeuverVerdict.Selected));
            var path = opposing.Path;
            EffectiveLaneCorridor other;
            Assert.That(model.TryGetCorridor(Opposing, out other), Is.True);

            float early = path.DistanceAtReference(path.StartSMeters + 6f);
            situation.SpeedMetersPerSecond = 2f;
            var back = ManeuverEvaluation.PlanReturn(situation, path, early, path.Track.OffsetRadians(0, early), other,
                ManeuverKind.OpposingCorridor);
            Assert.That(back, Is.Not.Null, "retour prouvable en debut de depart");
            Assert.That(back.Path.ReturnEndSMeters + Gauge.LengthMeters * 0.5f, Is.LessThan(situation.Cause.NearSMeters), "retour derriere la cause");

            float alongside = path.DistanceAtReference(0.5f * (path.DepartEndSMeters + path.ReturnStartSMeters));
            situation.SpeedMetersPerSecond = path.SpeedMetersPerSecond;
            Assert.That(ManeuverEvaluation.PlanReturn(situation, path, alongside, path.Track.OffsetRadians(0, alongside), other,
                ManeuverKind.OpposingCorridor), Is.Null, "a hauteur de la cause : retour non prouvable");

            var tactical = new TacticalDecision();
            Assert.That(tactical.SubmitManeuver(opposing, Stalled, 2UL).Accepted, Is.True);
            Assert.That(tactical.AbortManeuver(back.Path, -1f), Is.True);
            Assert.That(tactical.LastReason, Is.EqualTo(TacticalReason.ManeuverAborted));
            Assert.That(tactical.CommitManeuver(), Is.False, "un seul changement de plan");

            var committed = new TacticalDecision();
            Assert.That(committed.SubmitManeuver(opposing, Stalled, 2UL).Accepted, Is.True);
            Assert.That(committed.CommitManeuver(), Is.True);
            Assert.That(committed.LastReason, Is.EqualTo(TacticalReason.ManeuverCommitted));
            Assert.That(committed.AbortManeuver(back.Path, -1f), Is.False, "apres engagement : plus d'abandon");
            Assert.That(committed.ManeuverActive, Is.True, "engage : la manoeuvre continue");
        }

        [Test]
        public void TheExceptionIsRequiredOnlyWhileTheRemainingReferenceOccupiesTheOpposingCorridor()
        {
            var model = TwoWay(2f, false);
            var opposing = Candidate(Evaluate(Frame(model, 10f, stalledAt: 50f), 10f, Stalled, PerceivedObstacleKind.TrafficActor),
                ManeuverKind.OpposingCorridor);
            var path = opposing.Path;
            float until = opposing.Proof.LastOtherDistanceMeters;
            Assert.That(until, Is.GreaterThan(path.DistanceAtReference(path.ReturnStartSMeters)));
            Assert.That(until, Is.LessThan(path.DistanceAtReference(path.ReturnEndSMeters)),
                "apres la reentree prouvee, la fin ScopeExited ne coupe rien");
            Assert.That(opposing.Proof.OtherSMinMeters, Is.LessThan(opposing.Proof.OtherSMaxMeters));
        }

        [Test]
        public void ACorridorEndingBeforeTheReturnIsTooShort()
        {
            var model = TwoWay(2f, false, 70f);
            var result = Evaluate(Frame(model, 5f, stalledAt: 40f), 5f, Stalled, PerceivedObstacleKind.TrafficActor);
            var opposing = Candidate(result, ManeuverKind.OpposingCorridor);
            Assert.That(opposing.Verdict, Is.EqualTo(ManeuverVerdict.GeometryInfeasible), result.ToText());
            Assert.That(opposing.GeometryCause, Is.EqualTo(ManeuverGeometryCause.CorridorTooShort));
        }

        [Test]
        public void ASameDirectionAdjacentCorridorIsSelectedByTheSameEvaluation()
        {
            var model = TwoWay(2f, true);
            var result = Evaluate(Frame(model, 10f, stalledAt: 50f), 10f, Stalled, PerceivedObstacleKind.TrafficActor);
            var adjacent = Candidate(result, ManeuverKind.AdjacentCorridor);
            Assert.That(adjacent.Verdict, Is.EqualTo(ManeuverVerdict.Selected), result.ToText());
            Assert.That(adjacent.RequiresException, Is.False);
            Assert.That(adjacent.Path.TargetOffsetMeters, Is.EqualTo(4f).Within(0.01f));
            Assert.That(Candidate(result, ManeuverKind.OpposingCorridor).Verdict, Is.EqualTo(ManeuverVerdict.ExceptionPending),
                "le corridor oppose survit aussi : le cout puis l'ordre d'enumeration departagent");
        }

        // ================================================================== D4, decision 4A : plafond de 1000 pas

        [Test]
        public void TheExceptionCeilingIsOneThousandStepsAtTheAuthorityAndInTheManeuverWindow()
        {
            Assert.That(TrafficV2Settings.MaxRuleExceptionFrames, Is.EqualTo(1000));
            var model = TwoWay(2f, false);
            var reports = new[] { On(Self, Own) };
            Func<ulong, RuleExceptionRecord> decide = expiry => new JunctionCoordinator(model).Resolve(5UL, reports, null, null,
                new[] { new RuleExceptionRequest(Self, TrafficRule.OpposingCorridor, Opposing, RoadId.None, "borne",
                    new RuleExceptionTermination(RuleExceptionTerminationKind.ScopeExited, expiry), 5UL) }).RuleExceptions.Single();
            var atCeiling = decide(5UL + 1000UL);
            Assert.That(atCeiling.Status, Is.EqualTo(RuleExceptionStatus.Accepted), atCeiling.ToText());
            var beyond = decide(5UL + 1001UL);
            Assert.That(beyond.Status, Is.EqualTo(RuleExceptionStatus.Denied), beyond.ToText());
            Assert.That(beyond.Reason, Is.EqualTo(RuleExceptionReason.Malformed));

            // Fenetre de manoeuvre : (secondes jusqu'a la sortie de l'enveloppe opposee + 2 s) / dt, pas exact 1/32 s.
            const float step = 0.03125f;
            Assert.That(ManeuverEvaluation.ExceptionWindowFrames(29.25f, step), Is.EqualTo(1000d));
            Assert.That(ManeuverEvaluation.WithinExceptionWindow(ManeuverEvaluation.ExceptionWindowFrames(29.25f, step)), Is.True);
            Assert.That(ManeuverEvaluation.ExceptionWindowFrames(29.27f, step), Is.EqualTo(1001d));
            Assert.That(ManeuverEvaluation.WithinExceptionWindow(ManeuverEvaluation.ExceptionWindowFrames(29.27f, step)), Is.False,
                "fail-closed au-dela de 1000 pas");
            Assert.That(ManeuverEvaluation.WithinExceptionWindow(double.NaN), Is.False);
        }

        [Test]
        public void TheRequestedWindowStopsAtTheLastPointThatNeedsTheOpposingCorridor()
        {
            var model = TwoWay(2f, false);
            var opposing = Candidate(Evaluate(Frame(model, 10f, stalledAt: 50f), 10f, Stalled, PerceivedObstacleKind.TrafficActor),
                ManeuverKind.OpposingCorridor);
            var policy = FixturePolicy();
            var timing = new ManeuverTiming(0f, opposing.Path.SpeedMetersPerSecond, policy.Driver.MaxAcceleration,
                policy.Driver.ComfortableDeceleration);
            double expected = ManeuverEvaluation.ExceptionWindowFrames(timing.SecondsAt(opposing.Proof.LastOtherDistanceMeters), Dt);
            Assert.That(opposing.ExceptionExpiryFrame, Is.EqualTo(1UL + (ulong)expected));
            Assert.That(expected, Is.LessThan(ManeuverEvaluation.ExceptionWindowFrames(opposing.DurationSeconds, Dt)),
                "la fenetre ne couvre pas le retour hors de l'enveloppe opposee");
        }


        // ================================================================== M6 : faits distincts, meme planificateur

        [Test]
        public void FourCauseKindsProduceDistinctFactsAndTheSameEvaluation()
        {
            var model = TwoWay(2f, false);
            var cases = new[]
            {
                Frame(model, 10f, stalledAt: 50f),
                Frame(model, 10f, hazards: new[] { Box(TrafficHazardKind.Vehicle, model, 50f, 0f, CarExtents) }),
                Frame(model, 10f, hazards: new[] { Box(TrafficHazardKind.Obstacle, model, 50f, 0f, CarExtents) }),
                Frame(model, 10f, hazards: new[] { Box(TrafficHazardKind.Pedestrian, model, 50f, 0f, CarExtents) })
            };
            var kinds = new[] { PerceivedObstacleKind.TrafficActor, PerceivedObstacleKind.Vehicle, PerceivedObstacleKind.Obstacle,
                PerceivedObstacleKind.Pedestrian };
            var ids = new[] { Stalled, HazardId, HazardId, HazardId };
            var results = new List<ManeuverEvaluationResult>();
            for (int i = 0; i < cases.Length; i++)
            {
                var result = Evaluate(cases[i], 10f, ids[i], kinds[i]);
                results.Add(result);
                Assert.That(result.Cause.Kind, Is.EqualTo(kinds[i]), "fait distinct");
                var facts = TrafficPerception.ObserveAlong(cases[i], Self, Corridor(model, Own).Curve, Perception, new SpatialQueryBuffer(16));
                Assert.That(facts.Items.Select(f => f.Kind), Does.Contain(kinds[i]), "perception le long du chemin : genre conserve");
            }
            var verdicts = results.Select(r => string.Join(",", r.Candidates.Select(c => c.Kind + ":" + c.Verdict))).Distinct().ToList();
            Assert.That(verdicts, Has.Count.EqualTo(1), string.Join("\n", results.Select(r => r.ToText())));
            Assert.That(results.Select(r => r.Cause.NearSMeters).Max() - results.Select(r => r.Cause.NearSMeters).Min(), Is.LessThan(0.01f));
        }

        // ================================================================== M2 : preuve continue et courbure

        [Test]
        public void TheProvenReferenceIsContinuousCurvatureFeasibleAndInsideTheAuthorizedEnvelope()
        {
            var model = TwoWay(2f, false);
            var opposing = Candidate(Evaluate(Frame(model, 10f, stalledAt: 50f), 10f, Stalled, PerceivedObstacleKind.TrafficActor),
                ManeuverKind.OpposingCorridor);
            var path = opposing.Path;
            var profile = model.DrivabilityProfile;
            float allowed = ManeuverPath.AllowedCurvature(profile, path.SpeedMetersPerSecond,
                TrafficV2Settings.ManeuverLateralAccelerationMetersPerSecondSquared);
            // |kappa'| <= 60 |d| / L_t^3 (q''' maximal de q(u)), sur la transition la plus courte.
            float transition = Math.Min(path.DepartEndSMeters - path.StartSMeters, path.ReturnEndSMeters - path.ReturnStartSMeters);
            float slope = 60f * Math.Abs(path.TargetOffsetMeters) / (transition * transition * transition);
            for (int i = 1; i < path.SampleCount; i++)
            {
                var a = path.Samples[i - 1];
                var b = path.Samples[i];
                Assert.That(Vector3.Distance(a.Position, b.Position), Is.LessThan(0.12f), "positions continues");
                Assert.That(Vector3.Angle(a.Tangent, b.Tangent), Is.LessThan(0.5f), "cap continu");
                Assert.That(Math.Abs(b.CurvaturePerMeter - a.CurvaturePerMeter),
                    Is.LessThanOrEqualTo(slope * (b.SMeters - a.SMeters) * 1.2f + 1e-4f), "courbure continue (C2)");
                Assert.That(Math.Abs(b.CurvaturePerMeter), Is.LessThanOrEqualTo(allowed * 1.02f), "courbure admise a v_m");
            }
            Assert.That(path.SampleOffset(path.SampleCount - 1), Is.EqualTo(0f), "retour sur la reference");

            // Un corridor oppose non contigu (ecart de 1 m) : la meme reference echoue, fail-closed.
            var gap = TwoWay(2f, false, CorridorLength, 1f);
            var refused = Candidate(Evaluate(Frame(gap, 10f, stalledAt: 50f), 10f, Stalled, PerceivedObstacleKind.TrafficActor),
                ManeuverKind.OpposingCorridor);
            Assert.That(refused.Verdict, Is.EqualTo(ManeuverVerdict.GeometryInfeasible), refused.ToText());
            Assert.That(refused.GeometryCause, Is.EqualTo(ManeuverGeometryCause.EnvelopeNotContiguous).Or.EqualTo(ManeuverGeometryCause.EnvelopeExceeded));
        }

        // ================================================================== banc bicyclette : progression et suivi

        [TestCase(0f, TestName = "BypassOfAStalledVehicleKeepsProgressAndTracking")]
        [TestCase(3f, TestName = "OvertakingASlowVehicleKeepsProgressAndTracking")]
        public void TheGuidanceLawFollowsTheManeuverWithoutStallingAndResumes(float causeSpeed)
        {
            // Un depassement etire le passage par v_m / (v_m - v_cause) : corridor de 180 m (sous la limite C1 des fixtures).
            var model = TwoWay(2f, false, causeSpeed > 0f ? 180f : CorridorLength);
            var frame = Frame(model, 10f, stalledAt: 50f, stalledSpeed: causeSpeed);
            // Matrice : leader a 3 m/s, v_m a 8 m/s (vitesse desiree authoree).
            var situation = Situation(model, frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor,
                causeSpeed > 0f ? FixturePolicy(8f) : (EffectivePolicy?)null);
            var policy = situation.Policy;
            situation.Exceptions = Granted(model);
            var result = ManeuverEvaluation.Evaluate(situation, 2UL);
            var opposing = Candidate(result, ManeuverKind.OpposingCorridor);
            Assert.That(opposing.Verdict, Is.EqualTo(ManeuverVerdict.Selected), result.ToText());
            var path = opposing.Path;
            var cause = situation.Cause;

            var tactical = new TacticalDecision();
            Assert.That(tactical.SubmitManeuver(opposing, Stalled, 2UL).Accepted, Is.True);
            var own = Corridor(model, Own);
            var nominal = new ReferenceTrack(new[] { new TrackPiece(RoadElementKind.LaneCorridor, Own, 0f, 0f, own.Curve.Length,
                own.Samples) }, model.DrivabilityProfile.ReferencePointAheadRearAxleMeters, 0f);
            var profile = model.DrivabilityProfile;
            float a = profile.ReferencePointAheadRearAxleMeters, wheelbase = profile.WheelbaseMeters;
            var start = own.Curve.Sample(10f);
            Vector3 forward = start.Tangent, rear = start.Position - forward * a;
            float speed = 0f, progress = 0f, maxDisplacement = 0f, travel;
            for (int step = 0; step < 6000 && tactical.ManeuverActive; step++)
            {
                Vector3 reference = rear + forward * a;
                var command = TacticalDecision.ManeuverCommand((ulong)step, 1, tactical.ManeuverPath, profile, policy.Driver, reference,
                    forward, speed, Dt, null);
                float delta = command.TargetWheelAngleDegrees * Mathf.Deg2Rad;
                speed = Math.Max(0f, speed + command.TargetAccelerationMetersPerSecondSquared * Dt);
                travel = speed * Dt;
                rear += forward * travel;
                forward = Quaternion.AngleAxis(travel * Mathf.Tan(delta) / wheelbase * Mathf.Rad2Deg, Vector3.up) * forward;
                reference = rear + forward * a;
                var body = new BodyState(reference, Quaternion.LookRotation(forward, Vector3.up), reference, forward * speed, Vector3.zero);
                int piece;
                float along = tactical.ManeuverPath.Track.Project(reference, 0, out piece);
                Assert.That(along, Is.GreaterThanOrEqualTo(progress - 1e-3f), "progression monotone au pas " + step);
                progress = along;
                maxDisplacement = Math.Max(maxDisplacement, TrackingMeasurement.StepDisplacement(body, tactical.ManeuverPath.Track, along, Gauge));
                float nominalDisplacement = TrackingMeasurement.StepDisplacement(body, nominal, nominal.Project(reference, 0, out piece), Gauge);
                // Degagement reel vis-a-vis de la cause (predite a sa vitesse) : jamais de recouvrement.
                float causeShift = causeSpeed * (step + 1) * Dt;
                var projected = own.Curve.Project(reference);
                bool overlapsS = projected.SMeters + 2.25f > cause.NearSMeters + causeShift && projected.SMeters - 2.25f < cause.FarSMeters + causeShift;
                bool overlapsLateral = projected.LateralOffsetMeters + 1.03f > cause.LateralMinMeters
                    && projected.LateralOffsetMeters - 1.03f < cause.LateralMaxMeters;
                Assert.That(overlapsS && overlapsLateral, Is.False, "contact avec la cause au pas " + step);
                tactical.Update(new CollisionFacts((ulong)step, false, 0f, 0f, 0f, 0, 0f, 0f, 4, 4, nominalDisplacement, 1500f), Dt, Epsilon,
                    false, new RecoveryMotion(speed, travel, policy.Driver.MaxAcceleration), along);
            }
            Assert.That(tactical.ManeuverActive, Is.False, "manoeuvre terminee");
            Assert.That(tactical.ManeuverOutcome, Is.EqualTo(TacticalReason.Resumed), tactical.ToText());
            Assert.That(maxDisplacement, Is.LessThanOrEqualTo(Epsilon), "suivi dans epsilon_t de la reference de manoeuvre");
        }

        // ================================================================== mesure MVP_Run (decision proprietaire B)

        [Test]
        public void InMvpRunNoCandidateSurvivesBehindAStalledCar()
        {
            var model = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath)));
            var policy = DrivingPolicy.Resolve(Def, Self, 1UL);
            int evaluated = 0;
            var lines = new List<string>();
            foreach (var section in model.Sections)
            {
                var ids = model.GetCorridorsInSection(section.Id);
                if (ids.Count != 2) continue;
                foreach (var id in ids)
                {
                    var own = Corridor(model, id);
                    float selfS = 2.4f, causeS = selfS + 4.44f + Def.Profile.MinimumGap + 0.5f;
                    if (causeS + 2.3f > own.Curve.Length) continue;
                    var frame = new TrafficFrame(1UL, model, new[] { Actor(own, Self, selfS, 0f), Actor(own, Stalled, causeS, 0f) });
                    var situation = Situation(model, frame, selfS, Stalled, PerceivedObstacleKind.TrafficActor, policy, own);
                    var result = ManeuverEvaluation.Evaluate(situation, 1UL);
                    evaluated++;
                    lines.Add(own.CorridorId + " L " + own.Curve.Length.ToString("0.0") + " : " + result.ToText());
                    Assert.That(result.Selected, Is.Null, result.ToText());
                    Assert.That(result.Candidates.All(c => c.Verdict == ManeuverVerdict.NotOffered || c.Verdict == ManeuverVerdict.GeometryInfeasible),
                        Is.True, result.ToText());
                }
            }
            Assert.That(evaluated, Is.GreaterThan(0));
            TestContext.WriteLine("Mesure 5.42 MVP_Run, gagnant observe : aucun (" + evaluated + " corridors)\n" + string.Join("\n", lines));
        }

        // ================================================================== scans structurels

        [Test]
        public void NoBranchChoosesTheOpposingCorridorBecauseTheVehicleIsBlocked()
        {
            string evaluation = CodeOnly(File.ReadAllText(Path.Combine(TrafficRootPath, "Tactical/ManeuverEvaluation.cs")));
            var select = Regex.Match(evaluation, @"public static ManeuverEvaluationResult Evaluate\(ManeuverSituation situation, ulong frameId\)\s*\{(.*?)\n        \}",
                RegexOptions.Singleline);
            Assert.That(select.Success, Is.True);
            Assert.That(select.Groups[1].Value, Does.Not.Contain("ManeuverKind."), "selection : cout puis ordre d'enumeration seulement");
            Assert.That(select.Groups[1].Value, Does.Contain("Cost"));

            string driver = CodeOnly(File.ReadAllText(Path.Combine(TrafficRootPath, "Lifecycle/TrafficV2VehicleDriver.cs")));
            Assert.That(driver, Does.Not.Contain("ManeuverKind."), "le pilote ne choisit aucun candidat");
            Assert.That(driver, Does.Contain("maneuvering ? CollisionPredicates.SuspendsToleranceLatch(false, preparedFacts)"));
            Assert.That(driver, Does.Contain("displacement = TrackingMeasurement.StepDisplacement(state, maneuverTrack, preparedManeuverProgress, gauge)"));
            Assert.That(driver, Does.Contain("ManeuverEvaluation.Supervise(situation, maneuverCandidate, progress"));
            Assert.That(driver, Does.Contain("tactical.ManeuverExceptionUntilMeters, maneuverScopeExited"));
            string runner = CodeOnly(File.ReadAllText(Path.Combine(TrafficRootPath, "Lifecycle/TrafficV2StepRunner.cs")));
            Assert.That(runner, Does.Contain("request.SourceFrame == FrameId"));
            Assert.That(runner, Does.Contain("coordinator.Resolve(FrameId, reports, frame, cycles, ruleRequests)"));

            foreach (var file in Directory.GetFiles(TrafficRootPath, "*.cs", SearchOption.AllDirectories))
                Assert.That(CodeOnly(File.ReadAllText(file)), Does.Not.Contain("TryEvaluateLaneChange"), file);
            string policy = CodeOnly(File.ReadAllText(Path.Combine(TrafficRootPath, "Policy/DrivingPolicy.cs")));
            foreach (var forbidden in new[] { "Maneuver" + "Path", "Maneuver" + "Evaluation", "TrafficFrame", "Perception" })
                Assert.That(policy, Does.Not.Contain(forbidden), "la politique ne planifie rien");
            string safety = CodeOnly(File.ReadAllText(Path.Combine(TrafficRootPath, "Safety/SafetyFilter.cs")));
            Assert.That(safety, Does.Not.Contain("Maneuver").And.Not.Contain("RuleException"));
        }

        // ================================================================== revue : supervision, declencheur, fins, causes

        [Test]
        public void SupervisionAbortsOnAnArrivalBeforeThePointOfNoReturnAndCommitsAfterIt()
        {
            var model = TwoWay(2f, false);
            var opposing = SelectedOpposing(model);
            var path = opposing.Path;
            EffectiveLaneCorridor other = Corridor(model, Opposing);
            // Vehicule en face deja dans la region du corridor oppose.
            var frame = Frame(model, 10f, stalledAt: 50f, oncomingAtOpposingS: 90f, oncomingSpeed: 5f);
            var situation = Situation(model, frame, 10f, Stalled, PerceivedObstacleKind.TrafficActor);
            situation.Exceptions = Granted(model);
            situation.SpeedMetersPerSecond = 2f;
            float until = opposing.Proof.LastOtherDistanceMeters;
            ManeuverCandidate back;
            bool lost;

            float early = path.DistanceAtReference(path.StartSMeters + 6f);
            Assert.That(ManeuverEvaluation.Supervise(situation, opposing, early, path.Track.OffsetRadians(0, early), other, until, false,
                out back, out lost), Is.EqualTo(ManeuverSupervision.Abort));
            Assert.That(lost, Is.False, "conflit, pas une perte d'exception");
            Assert.That(back, Is.Not.Null);

            float alongside = path.DistanceAtReference(0.5f * (path.DepartEndSMeters + path.ReturnStartSMeters));
            situation.SpeedMetersPerSecond = path.SpeedMetersPerSecond;
            Assert.That(ManeuverEvaluation.Supervise(situation, opposing, alongside, path.Track.OffsetRadians(0, alongside), other, until,
                false, out back, out lost), Is.EqualTo(ManeuverSupervision.Commit));
            Assert.That(back, Is.Null);

            // Sans vehicule en face et avec l'exception : rien a faire.
            var quiet = Situation(model, Frame(model, 10f, stalledAt: 50f), 10f, Stalled, PerceivedObstacleKind.TrafficActor);
            quiet.Exceptions = Granted(model);
            Assert.That(ManeuverEvaluation.Supervise(quiet, opposing, early, path.Track.OffsetRadians(0, early), other, until, false,
                out back, out lost), Is.EqualTo(ManeuverSupervision.Continue));
        }

        [Test]
        public void ALostExceptionAbortsButAScopeExitAfterTheNormalReturnIsASuccess()
        {
            var model = TwoWay(2f, false);
            var opposing = SelectedOpposing(model);
            var path = opposing.Path;
            EffectiveLaneCorridor other = Corridor(model, Opposing);
            var situation = Situation(model, Frame(model, 10f, stalledAt: 50f), 10f, Stalled, PerceivedObstacleKind.TrafficActor);
            situation.SpeedMetersPerSecond = 2f;
            float until = opposing.Proof.LastOtherDistanceMeters;
            ManeuverCandidate back;
            bool lost;

            // Exception disparue (Expired, FrameUnavailable) alors que la reference exige encore le corridor oppose.
            float early = path.DistanceAtReference(path.StartSMeters + 6f);
            Assert.That(ManeuverEvaluation.Supervise(situation, opposing, early, path.Track.OffsetRadians(0, early), other, until, false,
                out back, out lost), Is.EqualTo(ManeuverSupervision.Abort));
            Assert.That(lost, Is.True);
            float alongside = path.DistanceAtReference(0.5f * (path.DepartEndSMeters + path.ReturnStartSMeters));
            situation.SpeedMetersPerSecond = path.SpeedMetersPerSecond;
            Assert.That(ManeuverEvaluation.Supervise(situation, opposing, alongside, path.Track.OffsetRadians(0, alongside), other, until,
                false, out back, out lost), Is.EqualTo(ManeuverSupervision.Commit), "retour non prouvable : engagement");
            Assert.That(lost, Is.True);

            // Fin ScopeExited publiee : succes, aucune perte, meme avant LastOtherDistance.
            float returning = path.DistanceAtReference(path.ReturnStartSMeters + 0.5f * (path.ReturnEndSMeters - path.ReturnStartSMeters));
            Assert.That(returning, Is.LessThanOrEqualTo(until));
            Assert.That(ManeuverEvaluation.Supervise(situation, opposing, returning, path.Track.OffsetRadians(0, returning), other, until,
                true, out back, out lost), Is.EqualTo(ManeuverSupervision.Continue));
            Assert.That(lost, Is.False);
            // Apres la sortie de l'enveloppe opposee, plus aucune exception n'est exigee.
            float after = Math.Min(path.LengthMeters, until + 1f);
            Assert.That(ManeuverEvaluation.Supervise(situation, opposing, after, path.Track.OffsetRadians(0, after), other, until, false,
                out back, out lost), Is.EqualTo(ManeuverSupervision.Continue));
        }

        [Test]
        public void AnAlreadyEffectiveExceptionAuthorizesOnlyIfItCoversTheRequiredWindow()
        {
            var granted = Granted(TwoWay(2f, false));
            Assert.That(ManeuverEvaluation.HasEffectiveException(granted, Opposing, 1001UL), Is.True);
            Assert.That(ManeuverEvaluation.HasEffectiveException(granted, Opposing, 1002UL), Is.False, "expiration avant la fin requise");
            Assert.That(ManeuverEvaluation.HasEffectiveException(granted, Own, 1UL), Is.False, "autre portee");
        }

        [Test]
        public void AnObstacleOnTheManeuverReferenceLowersTheCommandedAcceleration()
        {
            var model = TwoWay(2f, false);
            var path = SelectedOpposing(model).Path;
            var at = path.Piece.Curve.Sample(30f);
            var frame = Frame(model, 10f, stalledAt: 50f, hazards: new[] { new TrafficHazardInput(HazardId, TrafficHazardKind.Obstacle,
                new RoadBoundsBox { Center = at.Position + Vector3.up * 0.75f, Extents = new Vector3(1f, 0.75f, 1f) }, Vector3.zero, 1f) });
            ObservationChannel<ObstacleFact> facts;
            var near = ManeuverEvaluation.NearField(frame, Self, path, Perception, new SpatialQueryBuffer(32), false, out facts);
            var swept = near.Obstacles.Where(o => o.Id == HazardId).ToList();
            Assert.That(swept, Has.Count.EqualTo(1), "obstacle dans le couloir balaye du chemin");
            Assert.That(swept[0].NearDistanceMeters, Is.EqualTo(30f - 1f - Car.FrontMeters).Within(0.5f));
            Assert.That(near.HasLeader, Is.False, "aucun leader structure le long du chemin");
            var actor = frame.Actors.Single(a => a.TrafficId == Self);
            var driver = FixturePolicy().Driver;
            var free = TacticalDecision.ManeuverCommand(1UL, 1, path, model.DrivabilityProfile, driver, actor.Pose.Position,
                actor.Pose.Forward, 6f, Dt, null);
            var braking = TacticalDecision.ManeuverCommand(1UL, 1, path, model.DrivabilityProfile, driver, actor.Pose.Position,
                actor.Pose.Forward, 6f, Dt, near);
            Assert.That(braking.TargetAccelerationMetersPerSecondSquared, Is.LessThan(free.TargetAccelerationMetersPerSecondSquared));
        }

        [Test]
        public void TheTriggerOpensAnEvaluationOnlyAfterTwoSecondsOfTheSameSlowLegitimateCause()
        {
            var trigger = new ManeuverTrigger();
            const float desired = 8f;
            var a = new RoadId(0x542UL, 50UL);
            var b = new RoadId(0x542UL, 51UL);
            bool opened = false;
            for (ulong f = 1; f <= 100; f++)
                opened |= trigger.Observe(f, LongitudinalCandidateKind.Obstacle, a, 0f, PerceivedObstacleKind.Obstacle, false, desired, Dt);
            Assert.That(opened, Is.False, "moins de 2 s");
            // 100 x 0,02f = 1,99999995 s : le seuil de 2 s s'ouvre au pas suivant.
            trigger.Observe(101, LongitudinalCandidateKind.Obstacle, a, 0f, PerceivedObstacleKind.Obstacle, false, desired, Dt);
            Assert.That(trigger.Observe(102, LongitudinalCandidateKind.Obstacle, a, 0f, PerceivedObstacleKind.Obstacle, false, desired, Dt),
                Is.True, "2 s de la meme cause");
            Assert.That(trigger.Observe(103, LongitudinalCandidateKind.LeaderFollowing, b, 0f, PerceivedObstacleKind.TrafficActor, false,
                desired, Dt), Is.False, "nouvelle cause : le delai repart");
            Assert.That(trigger.Observe(203, LongitudinalCandidateKind.LeaderFollowing, b, 0.5f * desired + 0.01f,
                PerceivedObstacleKind.TrafficActor, false, desired, Dt), Is.False, "cause plus rapide que 0,5 x vitesse desiree");
            Assert.That(trigger.CauseId.IsEmpty, Is.True, "remise a zero");
            for (ulong f = 300; f <= 401; f++)
                Assert.That(trigger.Observe(f, LongitudinalCandidateKind.LeaderFollowing, b, 0f, PerceivedObstacleKind.TrafficActor, true,
                    desired, Dt), Is.False, "blocker dominant illegitime");
            Assert.That(trigger.Observe(500, LongitudinalCandidateKind.DesiredSpeed, b, 0f, PerceivedObstacleKind.TrafficActor, false,
                desired, Dt), Is.False, "aucune cause liante");
        }

        [Test]
        public void AManeuverEndsOnCollisionExitAndStall()
        {
            var opposing = SelectedOpposing(TwoWay(2f, false));
            var stable = new CollisionFacts(1UL, false, 0f, 0f, 0f, 0, 0f, 0f, 4, 4, 0f, 1500f);

            var collided = new TacticalDecision();
            Assert.That(collided.SubmitManeuver(opposing, Stalled, 2UL).Accepted, Is.True);
            var hit = new CollisionFacts(3UL, true, 9000f, 6f, 0.02f, 1, 0f, 0f, 4, 4, 0f, 1500f);
            Assert.That(collided.Submit(new CollisionResponseRequest(1UL, 3UL, Self, CollisionSignificance.RelativeSpeed, hit), 3UL, false,
                new RouteSeed(1UL), CollisionReactionWeights.Default, 0f).Accepted, Is.True);
            Assert.That(collided.ManeuverOutcome, Is.EqualTo(TacticalReason.CollisionPreempted));
            Assert.That(collided.CollisionActive, Is.True);

            var exiting = new TacticalDecision();
            Assert.That(exiting.SubmitManeuver(opposing, Stalled, 2UL).Accepted, Is.True);
            exiting.Update(stable, Dt, Epsilon, true, new RecoveryMotion(5f, 0.1f, 1.5f), 1f);
            Assert.That(exiting.ManeuverOutcome, Is.EqualTo(TacticalReason.ExitPortalReached));
            Assert.That(exiting.Active, Is.False);

            var stalled = new TacticalDecision();
            Assert.That(stalled.SubmitManeuver(opposing, Stalled, 2UL).Accepted, Is.True);
            for (int i = 0; i < 500 && stalled.ManeuverActive; i++)
                stalled.Update(stable, Dt, Epsilon, false, new RecoveryMotion(0f, 0f, 1.5f), 0f);
            Assert.That(stalled.ManeuverOutcome, Is.EqualTo(TacticalReason.Stalled), "progression commandee absente");
        }

        [Test]
        public void AClosingSpeedUnderTwoMetersPerSecondOrNoDepartureRoomIsInfeasible()
        {
            var model = TwoWay(2f, false);
            var slow = Candidate(Evaluate(Frame(model, 10f, stalledAt: 50f, stalledSpeed: 5f), 10f, Stalled,
                PerceivedObstacleKind.TrafficActor), ManeuverKind.OpposingCorridor);
            Assert.That(slow.GeometryCause, Is.EqualTo(ManeuverGeometryCause.NoClosingSpeed), slow.ToText());
            var close = Candidate(Evaluate(Frame(model, 45f, stalledAt: 50f), 45f, Stalled, PerceivedObstacleKind.TrafficActor),
                ManeuverKind.OpposingCorridor);
            Assert.That(close.GeometryCause, Is.EqualTo(ManeuverGeometryCause.TooClose), close.ToText());
        }

        /// <summary>Corridor oppose retenu et pret (exception effective a la frame 2).</summary>
        private static ManeuverCandidate SelectedOpposing(CompiledRoadModel model)
        {
            var situation = Situation(model, Frame(model, 10f, stalledAt: 50f), 10f, Stalled, PerceivedObstacleKind.TrafficActor);
            situation.Exceptions = Granted(model);
            var opposing = Candidate(ManeuverEvaluation.Evaluate(situation, 2UL), ManeuverKind.OpposingCorridor);
            Assert.That(opposing.Verdict, Is.EqualTo(ManeuverVerdict.Selected), opposing.ToText());
            return opposing;
        }

        // ================================================================== aides

        private static readonly Vector3 CarExtents = new Vector3(1.03f, 0.75f, 2.22f);

        private static EffectivePolicy FixturePolicy(float desiredSpeed = DesiredSpeed)
        {
            return DrivingPolicy.Resolve(Def.Profile.WithDesiredSpeed(desiredSpeed), Def.Policy, Self, 1UL);
        }

        private static ManeuverCandidate Candidate(ManeuverEvaluationResult result, ManeuverKind kind)
        {
            return result.Candidates.Single(c => c.Kind == kind);
        }

        private static ManeuverEvaluationResult Evaluate(TrafficFrame frame, float selfS, RoadId cause, PerceivedObstacleKind kind,
            EffectivePolicy? policy = null, bool denied = false, ulong frameId = 1UL, IReadOnlyList<EffectiveRuleException> exceptions = null)
        {
            var situation = Situation(frame.Model, frame, selfS, cause, kind, policy);
            situation.ExceptionDenied = denied;
            if (exceptions != null) situation.Exceptions = exceptions;
            return ManeuverEvaluation.Evaluate(situation, frameId);
        }

        private static ManeuverSituation Situation(CompiledRoadModel model, TrafficFrame frame, float selfS, RoadId cause,
            PerceivedObstacleKind kind, EffectivePolicy? policy = null, EffectiveLaneCorridor? own = null)
        {
            var corridor = own ?? Corridor(model, Own);
            ManeuverObstacle obstacle;
            Assert.That(ManeuverEvaluation.TryCause(frame, corridor.Curve, cause, kind, out obstacle), Is.True, "cause lue dans la frame");
            var profile = model.DrivabilityProfile;
            return new ManeuverSituation
            {
                Frame = frame, TrafficId = Self, Own = corridor, SelfSMeters = selfS, SelfOffsetRadians = 0f, SpeedMetersPerSecond = 0f,
                Cause = obstacle, Policy = policy ?? FixturePolicy(), Gauge = Gauge, EpsilonMeters = Epsilon,
                DeltaTimeSeconds = Dt,
                Limits = new SafetyLimits(8f, 6f, profile.LowSpeedLockDegrees, 1f / RoadModelCompiler.AdmissionRadiusMeters(profile), Dt),
                PerceptionLimits = Perception, Buffer = new SpatialQueryBuffer(32)
            };
        }

        private static EffectiveLaneCorridor Corridor(CompiledRoadModel model, RoadId id)
        {
            EffectiveLaneCorridor corridor;
            Assert.That(model.TryGetCorridor(id, out corridor), Is.True);
            return corridor;
        }

        private static TrafficFrame Frame(CompiledRoadModel model, float selfS, float stalledAt = -1f, float stalledSpeed = 0f,
            float oncomingAtOpposingS = -1f, float oncomingSpeed = 0f, TrafficHazardInput[] hazards = null)
        {
            var actors = new List<TrafficActorInput> { Actor(Corridor(model, Own), Self, selfS, 0f) };
            if (stalledAt >= 0f) actors.Add(Actor(Corridor(model, Own), Stalled, stalledAt, stalledSpeed));
            if (oncomingAtOpposingS >= 0f) actors.Add(Actor(Corridor(model, Opposing), Oncoming, oncomingAtOpposingS, oncomingSpeed));
            return new TrafficFrame(1UL, model, actors, hazards);
        }

        private static TrafficActorInput Actor(EffectiveLaneCorridor corridor, RoadId id, float s, float speed)
        {
            var point = corridor.Curve.Sample(s);
            return new TrafficActorInput(id, new VehicleFootprintPose { Position = point.Position, Forward = point.Tangent, Up = point.Up,
                Footprint = Car }, speed, corridor.CorridorId);
        }

        /// <summary>Danger de genre donne, centre sur le corridor propre a l'abscisse s, decale lateralement.</summary>
        private static TrafficHazardInput Box(TrafficHazardKind kind, CompiledRoadModel model, float s, float lateral, Vector3 extents)
        {
            var point = Corridor(model, Own).Curve.Sample(s);
            return new TrafficHazardInput(HazardId, kind, new RoadBoundsBox { Center = point.Position + point.Right * lateral + Vector3.up * extents.y,
                Extents = extents }, Vector3.zero, 1f);
        }

        /// <summary>Exception OpposingCorridor demandee au lot 1 et effective a la frame 2 (autorite reelle).</summary>
        private static IReadOnlyList<EffectiveRuleException> Granted(CompiledRoadModel model)
        {
            var snapshot = new JunctionCoordinator(model).Resolve(1UL, new[] { On(Self, Own), On(Stalled, Own) }, null, null,
                new[] { new RuleExceptionRequest(Self, TrafficRule.OpposingCorridor, Opposing, RoadId.None, "banc",
                    new RuleExceptionTermination(RuleExceptionTerminationKind.Expiry, 1UL + 1000UL), 1UL) });
            IReadOnlyList<EffectiveRuleException> effective;
            Assert.That(snapshot.TryGetEffectiveExceptions(Self, 2UL, out effective), Is.True, snapshot.ToText());
            return effective;
        }

        private static JunctionActorReport On(RoadId id, RoadId element)
        {
            var far = new Vector3(1000f, 0f, 1000f);
            return new JunctionActorReport(id, true, element, new[] { far, far, far, far }, null, null, null, null, false, false,
                JunctionRequestRejection.NoTraversal, 6f);
        }

        /// <summary>
        /// Section a double sens le long de +z : corridor propre (x = +w, vers +z), corridor oppose (x = -w - gap, vers -z), et
        /// optionnellement un corridor adjacent de meme sens (x = +3w, vers +z) avec adjacence authoree. Demi-largeurs w.
        /// Profils de drivabilite, validation et localisation authores de MVP_Run.
        /// </summary>
        private static CompiledRoadModel TwoWay(float halfWidth, bool adjacent, float length = CorridorLength, float gap = 0f)
        {
            var authored = RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath));
            var corridors = new List<LaneCorridor>
            {
                Lane(Opposing, new Vector3(-halfWidth - gap, 0f, length), Vector3.back, length, halfWidth, 0, false),
                Lane(Own, new Vector3(halfWidth, 0f, 0f), Vector3.forward, length, halfWidth, 1, true)
            };
            var adjacencies = new List<LaneAdjacency>();
            if (adjacent)
            {
                corridors.Add(Lane(Adjacent, new Vector3(3f * halfWidth, 0f, 0f), Vector3.forward, length, halfWidth, 2, false));
                adjacencies.Add(new LaneAdjacency { Id = Id(10), FromCorridorId = Own, ToCorridorId = Adjacent, Side = LaneSide.Right,
                    FromStartSMeters = 0f, FromEndSMeters = length, ToStartSMeters = 0f, ToEndSMeters = length,
                    Permission = LaneChangePermission.Allowed });
                adjacencies.Add(new LaneAdjacency { Id = Id(11), FromCorridorId = Adjacent, ToCorridorId = Own, Side = LaneSide.Left,
                    FromStartSMeters = 0f, FromEndSMeters = length, ToStartSMeters = 0f, ToEndSMeters = length,
                    Permission = LaneChangePermission.Allowed });
            }
            var source = new RoadModelSource
            {
                ModelId = Id(100), Label = "modele synthetique 5.42", ValidationProfile = authored.ValidationProfile,
                LocalizationProfile = authored.LocalizationProfile, DrivabilityProfile = authored.DrivabilityProfile,
                Sections = new[] { new RoadSection { Id = Section, RoadClass = RoadClass.Local, DefaultSpeedLimitMetersPerSecond = 13.9f,
                    DefaultAllowedVehicleClasses = VehicleClassMask.All } },
                Corridors = corridors.ToArray(), Adjacencies = adjacencies.ToArray(), Junctions = new Junction[0],
                Movements = new JunctionMovement[0], Controls = new JunctionControl[0], ConflictZones = new ConflictZone[0]
            };
            return RoadModelCompiler.Compile(source);
        }

        private static LaneCorridor Lane(RoadId id, Vector3 start, Vector3 tangent, float length, float halfWidth, int order, bool datum)
        {
            int count = Mathf.RoundToInt(length / 1f);
            var samples = Enumerable.Range(0, count + 1).Select(i => new RoadCurveSample { SMeters = length * i / count,
                Position = start + tangent * (length * i / count), Tangent = tangent, Up = Vector3.up,
                HalfWidthLeftMeters = halfWidth, HalfWidthRightMeters = halfWidth }).ToArray();
            return new LaneCorridor { Id = id, SectionId = Section, Samples = samples, LengthMeters = length, LateralOrder = order,
                IsCrossSectionDatum = datum };
        }

        private static string CodeOnly(string source)
        {
            return Regex.Replace(Regex.Replace(source, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\n]*", "");
        }
    }
}
