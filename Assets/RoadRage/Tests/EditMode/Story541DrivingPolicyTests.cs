using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Policy;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Features.Vehicles.Traffic.Safety;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.41 -- frontiere DrivingPolicy (P1-P3 : parametres effectifs, variation, proposition) et protocole d'exception aux
    /// TrafficRules decide par l'autorite du coordinateur (P4-P5), porte sur la commande et la projection (P6). Carrefour
    /// synthetique en memoire (patron Story 5.35/5.40, aucun artefact signe modifie) et gardes structurelles.
    /// </summary>
    [Category("Core")]
    [Category("Story541")]
    public sealed class Story541DrivingPolicyTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string TrafficRoot = "Assets/RoadRage/Features/Vehicles/Traffic";
        private const string PolicyPath = TrafficRoot + "/Policy/DrivingPolicy.cs";
        private const float Dt = 0.02f;
        private const float CarLength = 4.44f;
        private static readonly Vector3[] FarAway = { new Vector3(1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f),
            new Vector3(1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f) };

        private static DriverProfileDef Def { get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath); } }
        private static DriverProfile Driver { get { return Def.Profile; } }
        private static float Reservation { get { return CarLength + Driver.MinimumGap; } }
        private static RoadId Id(int n) { return new RoadId(0x541UL, (ulong)n); }
        private static readonly RoadId A = Id(1), B = Id(2);
        // Corridors du carrefour synthetique : approche et sortie de la premiere branche, mouvement de la premiere branche.
        private static readonly RoadId C0 = Id(1200), C1 = Id(1201), M0 = Id(1300);

        private static CompiledRoadModel crossing;
        private static CompiledRoadModel Crossing { get { return crossing ?? (crossing = RoadModelCompiler.Compile(CrossingSource(0f, -90f))); } }

        // ============================================================ P1-P3 : politique

        [Test]
        public void ResolutionCarriesTheAuthoredParametersWithoutMutatingTheDefinition()
        {
            var def = Def;
            string before = EditorJsonUtility.ToJson(def);
            var policy = DrivingPolicy.Resolve(def, A, 7UL);
            Assert.That(EditorJsonUtility.ToJson(def), Is.EqualTo(before), "la definition authoree n'est pas mutee");
            Assert.That(def.TryValidate(out var error), Is.True, error);

            Assert.That(policy.TrafficId, Is.EqualTo(A));
            Assert.That(policy.Driver, Is.EqualTo(def.Profile), "vitesse desiree, T et s0 : le profil authore");
            Assert.That(policy.AcceptedRisk, Is.EqualTo(0.3f));
            Assert.That(policy.AcceptedGapSeconds, Is.EqualTo(4f));
            Assert.That(policy.RoutePreferenceWeight, Is.EqualTo(1f));
            Assert.That(policy.AllowedSurfaces, Is.EqualTo(DrivingSurface.Carriageway | DrivingSurface.OpposingCorridor));
            Assert.That(policy.Maneuver(ManeuverKind.CorridorOffset).Eligible, Is.True);
            Assert.That(policy.Maneuver(ManeuverKind.AdjacentCorridor).Eligible, Is.True);
            Assert.That(policy.Maneuver(ManeuverKind.OpposingCorridor).Eligible, Is.True);
            Assert.That(policy.Maneuver(ManeuverKind.AuthorizedSurface).Eligible, Is.False);
            Assert.That(policy.Maneuver(ManeuverKind.OpposingCorridor).Cost, Is.EqualTo(1f));
            Assert.That(def.Policy, Is.EqualTo(DrivingPolicyProfile.Default), "l'asset authore les valeurs D3 explicitement");
        }

        [Test]
        public void AManeuverIsEligibleOnlyWhenWillingAndItsSurfaceIsAllowed()
        {
            var willing = new ManeuverPreference(true, 2f);
            var refused = new ManeuverPreference(false, 2f);
            var carriagewayOnly = new DrivingPolicyProfile(0.5f, 3f, 1f, DrivingSurface.Carriageway, willing, willing, willing, willing);
            var policy = DrivingPolicy.Resolve(Driver, carriagewayOnly, A, 1UL);
            Assert.That(policy.Maneuver(ManeuverKind.AdjacentCorridor).Eligible, Is.True);
            Assert.That(policy.Maneuver(ManeuverKind.OpposingCorridor).Eligible, Is.False, "surface opposee non permise");
            Assert.That(policy.Maneuver(ManeuverKind.AuthorizedSurface).Eligible, Is.False, "trottoir non permis");

            var unwilling = new DrivingPolicyProfile(0.5f, 3f, 1f,
                DrivingSurface.Carriageway | DrivingSurface.OpposingCorridor | DrivingSurface.Sidewalk, refused, willing, refused, willing);
            policy = DrivingPolicy.Resolve(Driver, unwilling, A, 1UL);
            Assert.That(policy.Maneuver(ManeuverKind.CorridorOffset).Eligible, Is.False);
            Assert.That(policy.Maneuver(ManeuverKind.OpposingCorridor).Eligible, Is.False, "surface permise mais non volontaire");
            Assert.That(policy.Maneuver(ManeuverKind.AuthorizedSurface).Eligible, Is.True);
            Assert.That(policy.Maneuver(ManeuverKind.AuthorizedSurface).Cost, Is.EqualTo(2f));
        }

        [TestCase(float.NaN, 4f, 1f, DrivingSurface.Carriageway, 1f)]
        [TestCase(1.5f, 4f, 1f, DrivingSurface.Carriageway, 1f)]
        [TestCase(-0.1f, 4f, 1f, DrivingSurface.Carriageway, 1f)]
        [TestCase(0.3f, -1f, 1f, DrivingSurface.Carriageway, 1f)]
        [TestCase(0.3f, float.PositiveInfinity, 1f, DrivingSurface.Carriageway, 1f)]
        [TestCase(0.3f, 4f, -1f, DrivingSurface.Carriageway, 1f)]
        [TestCase(0.3f, 4f, 1f, (DrivingSurface)8, 1f)]
        [TestCase(0.3f, 4f, 1f, DrivingSurface.Carriageway, -1f)]
        [TestCase(0.3f, 4f, 1f, DrivingSurface.Carriageway, float.NaN)]
        public void AnInvalidAuthoredPolicyFailsValidationAndSoDoesItsDefinition(float risk, float gap, float weight,
            DrivingSurface surfaces, float cost)
        {
            var invalid = new DrivingPolicyProfile(risk, gap, weight, surfaces, new ManeuverPreference(true, 1f),
                new ManeuverPreference(true, 1f), new ManeuverPreference(true, cost), new ManeuverPreference(false, 1f));
            string error;
            Assert.That(invalid.TryValidate(out error), Is.False);
            Assert.That(error, Does.StartWith("Policy invalide"));
            var def = UnityEngine.Object.Instantiate(Def);
            try
            {
                typeof(DriverProfileDef).GetField("policy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(def, invalid);
                Assert.That(def.TryValidate(out error), Is.False, "la definition refuse une politique invalide");
            }
            finally { UnityEngine.Object.DestroyImmediate(def); }
        }

        [Test]
        public void OneVehiclesPolicyNeverChangesAnothersAndTheSameSeedGivesTheSamePolicy()
        {
            var first = DrivingPolicy.Resolve(Def, A, 11UL);
            string firstText = first.ToText();
            float firstPhase = first.VariationPhase;
            var aggressive = new DrivingPolicyProfile(1f, 0.5f, 0f, DrivingSurface.Carriageway, default(ManeuverPreference),
                default(ManeuverPreference), default(ManeuverPreference), default(ManeuverPreference));
            var other = DrivingPolicy.Resolve(Driver.WithDesiredSpeed(30f), aggressive, B, 11UL);
            Assert.That(first.ToText(), Is.EqualTo(firstText));
            Assert.That(first.Driver.DesiredSpeed, Is.EqualTo(Driver.DesiredSpeed));
            Assert.That(first.VariationPhase, Is.EqualTo(firstPhase));
            Assert.That(other.AcceptedRisk, Is.EqualTo(1f));

            var again = DrivingPolicy.Resolve(Def, A, 11UL);
            Assert.That(again.VariationPhase, Is.EqualTo(firstPhase), "meme graine, meme vehicule : meme variation");
            Assert.That(again.ToText(), Is.EqualTo(firstText));
            Assert.That(DrivingPolicy.Resolve(Def, B, 11UL).VariationPhase, Is.Not.EqualTo(firstPhase), "la phase est propre au vehicule");
            Assert.That(firstPhase, Is.GreaterThan(0f).And.LessThan(1f));
        }

        [Test]
        public void DesiredSpeedVariationIsDeterministicAndBoundedByTheConsistencyEnvelope()
        {
            var driver = Driver;
            var policy = DrivingPolicy.Resolve(Def, A, 3UL);
            var replay = DrivingPolicy.Resolve(Def, A, 3UL);
            // Enveloppe de DriverModel.ResolveNoisyDesiredSpeed (Story 5.9) : 6 % x (1 - consistency).
            float bound = driver.DesiredSpeed * 0.06f * (1f - driver.Consistency) + 1e-4f;
            bool varied = false;
            for (int i = 0; i < 400; i++)
            {
                float t = i * 0.37f;
                float v = policy.DesiredSpeedAt(t);
                Assert.That(replay.DesiredSpeedAt(t), Is.EqualTo(v));
                Assert.That(Math.Abs(v - driver.DesiredSpeed), Is.LessThanOrEqualTo(bound), "t = " + t);
                varied |= v != driver.DesiredSpeed;
            }
            Assert.That(varied, Is.True, "consistency 0,8 : la vitesse desiree varie");

            var regular = new DriverProfile(8f, 1.5f, 2f, 1.5f, 2f, 0.25f, 0.2f, 4f, 0.3f, 1f, 1f);
            var steady = DrivingPolicy.Resolve(regular, DrivingPolicyProfile.Default, A, 3UL);
            for (int i = 0; i < 50; i++) Assert.That(steady.DesiredSpeedAt(i * 0.7f), Is.EqualTo(8f), "consistency 1 : aucun bruit");
        }

        [Test]
        public void PolicyProposesOnlyForItsOwnVehicleAndAnAllowedSurface()
        {
            var policy = DrivingPolicy.Resolve(Def, A, 1UL);
            var termination = new RuleExceptionTermination(RuleExceptionTerminationKind.ScopeExited, 20UL);
            RuleExceptionRequest request;
            Assert.That(DrivingPolicy.TryPropose(policy, TrafficRule.OpposingCorridor, C0, RoadId.None, "Story541", termination, 9UL,
                out request), Is.True);
            Assert.That(request.Requester, Is.EqualTo(A));
            Assert.That(request.Rule, Is.EqualTo(TrafficRule.OpposingCorridor));
            Assert.That(request.Scope, Is.EqualTo(C0));
            Assert.That(request.StartReason, Is.EqualTo("Story541"));
            Assert.That(request.Termination.ExpiryFrame, Is.EqualTo(20UL));
            Assert.That(request.SourceFrame, Is.EqualTo(9UL));

            Assert.That(DrivingPolicy.TryPropose(policy, TrafficRule.KeepClear, C0, RoadId.None, "x", termination, 9UL, out request), Is.False);
            Assert.That(DrivingPolicy.TryPropose(policy, TrafficRule.Sidewalk, C0, RoadId.None, "x", termination, 9UL, out request), Is.False);
            var cautious = DrivingPolicy.Resolve(Driver, new DrivingPolicyProfile(0f, 6f, 1f, DrivingSurface.Carriageway,
                default(ManeuverPreference), default(ManeuverPreference), default(ManeuverPreference), default(ManeuverPreference)), A, 1UL);
            Assert.That(DrivingPolicy.TryPropose(cautious, TrafficRule.OpposingCorridor, C0, RoadId.None, "x", termination, 9UL,
                out request), Is.False);
            Assert.That(request, Is.Null);
        }

        // ============================================================ P4-P5 : autorite et publication (matrice)

        [Test]
        public void AnAcceptedExceptionIsPublishedEffectiveAtTheNextFrameOnly()
        {
            var coordinator = new JunctionCoordinator(Crossing);
            var snapshot = coordinator.Resolve(5UL, new[] { On(A, C0), On(B, C0) }, null, null, new[] { Ask(A, 5UL) });
            var record = Only(snapshot);
            Assert.That(record.Status, Is.EqualTo(RuleExceptionStatus.Accepted), record.ToText());
            Assert.That(record.SourceFrame, Is.EqualTo(5UL));
            Assert.That(record.EffectiveFrame, Is.EqualTo(6UL));
            Assert.That(snapshot.ToText(), Does.Contain("RuleException Accepted"));

            IReadOnlyList<EffectiveRuleException> effective;
            Assert.That(snapshot.TryGetEffectiveExceptions(A, 6UL, out effective), Is.True);
            Assert.That(effective.Single().Rule, Is.EqualTo(TrafficRule.OpposingCorridor));
            Assert.That(effective.Single().AcceptedFrame, Is.EqualTo(5UL));
            Assert.That(snapshot.TryGetEffectiveExceptions(A, 5UL, out effective), Is.False, "jamais au lot de la demande");
            Assert.That(snapshot.TryGetEffectiveExceptions(A, 7UL, out effective), Is.False, "instantane decale : aucune");
            Assert.That(effective, Is.Not.Null.And.Empty);
            Assert.That(snapshot.TryGetEffectiveExceptions(B, 6UL, out effective), Is.False, "jamais pour un autre vehicule");

            var held = coordinator.Resolve(6UL, new[] { On(A, C0), On(B, C0) });
            Assert.That(Only(held).Status, Is.EqualTo(RuleExceptionStatus.Held));
            Assert.That(held.TryGetEffectiveExceptions(A, 7UL, out effective), Is.True);
            Assert.That(coordinator.ActiveRuleExceptions, Is.EqualTo(1));
        }

        [TestCase(TrafficRule.KeepClear)]
        [TestCase(TrafficRule.JunctionControl)]
        [TestCase(TrafficRule.SignalCompliance)]
        [TestCase(TrafficRule.SpeedLimit)]
        [TestCase(TrafficRule.Sidewalk)]
        public void ARuleOutsideTheAuthoredViolableSetIsDenied(TrafficRule rule)
        {
            Assert.That(TrafficV2Settings.ViolableTrafficRules, Is.EqualTo(new[] { TrafficRule.OpposingCorridor }));
            ExpectDenied(Ask(A, 5UL, rule), RuleExceptionReason.RuleNotViolable);
        }

        [Test]
        public void AScopeThatIsNotACorridorOfTheModelIsDenied()
        {
            ExpectDenied(Ask(A, 5UL, scope: M0), RuleExceptionReason.ScopeInsideJunction);
            ExpectDenied(Ask(A, 5UL, scope: Id(9999)), RuleExceptionReason.ScopeInsideJunction);
        }

        [Test]
        public void AnImplicitOrUnboundedRequestIsMalformed()
        {
            var cases = new Dictionary<string, RuleExceptionRequest>
            {
                { "ni portee ni cible", Ask(A, 5UL, scope: RoadId.None) },
                { "raison vide", Ask(A, 5UL, reason: "") },
                { "raison blanche", Ask(A, 5UL, reason: "  ") },
                { "raison nulle", Ask(A, 5UL, reason: null) },
                { "expiration au lot", Ask(A, 5UL, expiry: 5UL) },
                { "expiration au-dela du plafond", Ask(A, 5UL, expiry: 5UL + TrafficV2Settings.MaxRuleExceptionFrames + 1UL) },
                { "regle None", Ask(A, 5UL, TrafficRule.None) },
                { "regle inconnue", Ask(A, 5UL, (TrafficRule)99) },
                { "sortie de portee sans portee", Ask(A, 5UL, scope: RoadId.None, target: B, kind: RuleExceptionTerminationKind.ScopeExited) },
                { "terminaison inconnue", Ask(A, 5UL, kind: (RuleExceptionTerminationKind)7) },
                { "frame source differente", Ask(A, 4UL, frame: 5UL) },
                { "demandeur vide", Ask(RoadId.None, 5UL) },
                { "cible = demandeur", Ask(A, 5UL, target: A) }
            };
            foreach (var entry in cases)
                ExpectDenied(entry.Value, RuleExceptionReason.Malformed, entry.Key);
            var coordinator = new JunctionCoordinator(Crossing);
            var bound = coordinator.Resolve(5UL, new[] { On(A, C0) }, null, null,
                new[] { Ask(A, 5UL, expiry: 5UL + TrafficV2Settings.MaxRuleExceptionFrames) });
            Assert.That(Only(bound).Status, Is.EqualTo(RuleExceptionStatus.Accepted), "le plafond lui-meme est admis");
        }

        [Test]
        public void AnAbsentRequesterOrTargetIsDenied()
        {
            var coordinator = new JunctionCoordinator(Crossing);
            var snapshot = coordinator.Resolve(5UL, new[] { On(B, C0) }, null, null, new[] { Ask(A, 5UL) });
            Assert.That(Only(snapshot).Reason, Is.EqualTo(RuleExceptionReason.RequesterAbsent));
            snapshot = coordinator.Resolve(6UL, new[] { On(A, C0) }, null, null, new[] { Ask(A, 6UL, target: B) });
            Assert.That(Only(snapshot).Reason, Is.EqualTo(RuleExceptionReason.TargetAbsent));
            snapshot = coordinator.Resolve(7UL, new[] { On(A, C0), On(B, C1) }, null, null,
                new[] { Ask(A, 7UL, scope: RoadId.None, target: B) });
            Assert.That(Only(snapshot).Status, Is.EqualTo(RuleExceptionStatus.Accepted), "cible seule, presente");
        }

        [Test]
        public void ASecondExceptionForTheSameRequesterAndRuleIsDenied()
        {
            var coordinator = new JunctionCoordinator(Crossing);
            var snapshot = coordinator.Resolve(5UL, new[] { On(A, C0) }, null, null,
                new[] { Ask(A, 5UL, reason: "second"), Ask(A, 5UL, reason: "first") });
            var records = snapshot.RuleExceptions;
            Assert.That(records.Count, Is.EqualTo(2), snapshot.ToText());
            Assert.That(records.Count(r => r.Status == RuleExceptionStatus.Accepted && r.Request.StartReason == "first"), Is.EqualTo(1),
                "ordre stable, independant de l'ordre de soumission");
            Assert.That(records.Count(r => r.Reason == RuleExceptionReason.AlreadyActive), Is.EqualTo(1));
            snapshot = coordinator.Resolve(6UL, new[] { On(A, C0) }, null, null, new[] { Ask(A, 6UL) });
            Assert.That(snapshot.RuleExceptions.Single(r => r.Status == RuleExceptionStatus.Denied).Reason,
                Is.EqualTo(RuleExceptionReason.AlreadyActive));
            Assert.That(snapshot.RuleExceptions.Count(r => r.Status == RuleExceptionStatus.Held), Is.EqualTo(1));
        }

        [Test]
        public void AnExceptionEndsAtExpiryWithAPublishedReason()
        {
            var coordinator = new JunctionCoordinator(Crossing);
            coordinator.Resolve(5UL, new[] { On(A, C0) }, null, null, new[] { Ask(A, 5UL, expiry: 7UL) });
            var held = coordinator.Resolve(6UL, new[] { On(A, C0) });
            Assert.That(Only(held).Status, Is.EqualTo(RuleExceptionStatus.Held));
            var ended = coordinator.Resolve(7UL, new[] { On(A, C0) });
            Assert.That(Only(ended).Status, Is.EqualTo(RuleExceptionStatus.Ended));
            Assert.That(Only(ended).Reason, Is.EqualTo(RuleExceptionReason.Expired));
            IReadOnlyList<EffectiveRuleException> effective;
            Assert.That(ended.TryGetEffectiveExceptions(A, 8UL, out effective), Is.False);
            Assert.That(coordinator.ActiveRuleExceptions, Is.Zero);
            Assert.That(coordinator.Resolve(8UL, new[] { On(A, C0) }).RuleExceptions, Is.Empty, "une fin n'est publiee qu'une fois");
        }

        [Test]
        public void AScopeExitEndsTheExceptionOnlyAfterTheRequesterEnteredTheScope()
        {
            var coordinator = new JunctionCoordinator(Crossing);
            coordinator.Resolve(5UL, new[] { On(A, C1) }, null, null,
                new[] { Ask(A, 5UL, kind: RuleExceptionTerminationKind.ScopeExited, expiry: 50UL) });
            Assert.That(Only(coordinator.Resolve(6UL, new[] { On(A, C1) })).Status, Is.EqualTo(RuleExceptionStatus.Held), "pas encore entre");
            Assert.That(Only(coordinator.Resolve(7UL, new[] { On(A, C0) })).Status, Is.EqualTo(RuleExceptionStatus.Held), "dans la portee");
            Assert.That(Only(coordinator.Resolve(8UL, new[] { On(A, C0, localized: false) })).Status,
                Is.EqualTo(RuleExceptionStatus.Held), "localisation inconnue : rien n'est conclu, l'expiration borne");
            var exited = Only(coordinator.Resolve(9UL, new[] { On(A, C1) }));
            Assert.That(exited.Status, Is.EqualTo(RuleExceptionStatus.Ended));
            Assert.That(exited.Reason, Is.EqualTo(RuleExceptionReason.ScopeExited));
        }

        [Test]
        public void AnExceptionEndsWhenItsRequesterOrTargetLeavesTheFrame()
        {
            var coordinator = new JunctionCoordinator(Crossing);
            coordinator.Resolve(5UL, new[] { On(A, C0), On(B, C1) }, null, null, new[] { Ask(A, 5UL, target: B) });
            var gone = Only(coordinator.Resolve(6UL, new[] { On(A, C0) }));
            Assert.That(gone.Reason, Is.EqualTo(RuleExceptionReason.TargetGone));

            coordinator.Resolve(7UL, new[] { On(A, C0) }, null, null, new[] { Ask(A, 7UL) });
            gone = Only(coordinator.Resolve(8UL, new[] { On(B, C0) }));
            Assert.That(gone.Reason, Is.EqualTo(RuleExceptionReason.RequesterGone));
        }

        [Test]
        public void AnInvalidFrameEndsEveryExceptionFailClosed()
        {
            var coordinator = new JunctionCoordinator(Crossing);
            coordinator.Resolve(5UL, new[] { On(A, C0), On(B, C0) }, null, null, new[] { Ask(A, 5UL), Ask(B, 5UL) });
            var failed = coordinator.ResolveUnavailableFrame(6UL);
            Assert.That(failed.RuleExceptions.Count, Is.EqualTo(2), failed.ToText());
            Assert.That(failed.RuleExceptions.All(r => r.Status == RuleExceptionStatus.Ended
                && r.Reason == RuleExceptionReason.FrameUnavailable), Is.True, failed.ToText());
            Assert.That(coordinator.ActiveRuleExceptions, Is.Zero);
            IReadOnlyList<EffectiveRuleException> effective;
            Assert.That(failed.TryGetEffectiveExceptions(A, 7UL, out effective), Is.False);
            Assert.That(coordinator.ResolveUnavailableFrame(7UL).RuleExceptions, Is.Empty);
        }

        // ============================================================ aucune exception ne leve un grant ni le SafetyFilter

        [Test]
        public void ExceptionRequestsNeverChangeAJunctionDecision()
        {
            var m = Crossing;
            var index = JunctionConflictIndex.For(m);
            var moves = m.Movements.OrderBy(x => x.Id).Select(x => Traversal(index, x.Id)).ToArray();
            var reports = new[] { Requesting(m, A, moves[0], 0.3f, 0f), Requesting(m, B, moves[1], 0.3f, 0f) };
            var plain = new JunctionCoordinator(m);
            var asked = new JunctionCoordinator(m);
            bool granted = false, denied = false;
            for (ulong frame = 1UL; frame <= 4UL; frame++)
            {
                var requests = new[] { Ask(A, frame, expiry: frame + 20UL), Ask(B, frame, TrafficRule.JunctionControl, scope: M0),
                    Ask(B, frame, TrafficRule.KeepClear), Ask(A, frame, scope: M0) };
                var expected = plain.Resolve(frame, reports);
                var actual = asked.Resolve(frame, reports, null, null, requests);
                Assert.That(actual.Records.Select(r => r.ToText()), Is.EqualTo(expected.Records.Select(r => r.ToText())), "lot " + frame);
                // « paires » compte un cout de calcul, pas une decision : l'index partage par modele met en cache les faits de
                // paire de traversees (JunctionConflictIndex.PairOf), donc le second coordinateur ne le paie plus.
                Assert.That(WithoutPairCost(actual.Counters.ToText()), Is.EqualTo(WithoutPairCost(expected.Counters.ToText())), "lot " + frame);
                Assert.That(actual.RuleExceptions, Is.Not.Empty);
                granted |= actual.Records.Any(r => r.IsEffectiveGrant);
                denied |= actual.Records.Any(r => r.Status == JunctionGrantStatus.Denied);
            }
            Assert.That(granted && denied, Is.True, "le lot compare contient un grant et un refus");
            Assert.That(asked.ActiveRuleExceptions, Is.EqualTo(1));
        }

        [Test]
        public void TheSafetyFilterVerdictIgnoresTheExceptionsCarriedOnTheCommand()
        {
            var exceptions = AcceptedExceptions();
            var command = new MotionCommand(23UL, 23UL, 23UL, 1f, 5f, SpeedConstraint.None, 0.1f, 1f, 0.05f, 3f);
            Assert.That(command.RuleExceptions, Is.Not.Null.And.Empty, "jamais nul");
            var carried = command.WithRuleExceptions(exceptions);
            Assert.That(carried.RuleExceptions, Is.SameAs(exceptions));
            Assert.That(carried.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(command.TargetAccelerationMetersPerSecondSquared));
            Assert.That(carried.TargetWheelAngleDegrees, Is.EqualTo(command.TargetWheelAngleDegrees));
            Assert.That(carried.ReferenceSMeters, Is.EqualTo(command.ReferenceSMeters));
            Assert.That(carried.WithRuleExceptions(null).RuleExceptions, Is.Empty);

            var decision = MvpDecision();
            var limits = new SafetyLimits(8f, 6f, 35f, 0.2f, 0.02f);
            foreach (float acceleration in new[] { 1f, -20f, float.NaN })
            {
                var plain = new MotionCommand(23UL, 23UL, 23UL, acceleration, 5f, SpeedConstraint.None, 0.1f, 1f, 0.05f, 3f);
                var expected = SafetyFilter.Evaluate(plain, 23UL, decision.Frame, decision.TrafficId, 5f, decision.Decision.Path, null, limits);
                var actual = SafetyFilter.Evaluate(plain.WithRuleExceptions(exceptions), 23UL, decision.Frame, decision.TrafficId, 5f,
                    decision.Decision.Path, null, limits);
                Assert.That(actual.Verdict, Is.EqualTo(expected.Verdict), "a = " + acceleration);
                Assert.That(actual.Reason, Is.EqualTo(expected.Reason));
                Assert.That(actual.Command.HasValue, Is.EqualTo(expected.Command.HasValue));
                if (expected.Command.HasValue)
                {
                    Assert.That(actual.Command.Value.TargetAccelerationMetersPerSecondSquared,
                        Is.EqualTo(expected.Command.Value.TargetAccelerationMetersPerSecondSquared));
                    Assert.That(actual.Command.Value.TargetWheelAngleDegrees, Is.EqualTo(expected.Command.Value.TargetWheelAngleDegrees));
                }
            }
        }

        // ============================================================ P6 : debug

        [Test]
        public void TheProjectionShowsThePolicyAndTheActiveExceptions()
        {
            var decision = MvpDecision();
            var projection = decision.Decision.Projection;
            Assert.That(projection.ToText(), Does.Not.Contain("\nPolicy "), "sans politique resolue, aucune ligne");
            var policy = DrivingPolicy.Resolve(Def, decision.TrafficId, 1UL);
            var none = projection.WithPolicy(policy, null);
            Assert.That(none.RuleExceptions, Is.Empty);
            Assert.That(none.ToText(), Does.Contain("\nPolicy risk 0.3 gap 4s").And.Contain("/ exceptions aucune"));
            var exceptions = AcceptedExceptions();
            var text = projection.WithPolicy(policy, exceptions).ToText();
            Assert.That(text, Does.Contain("/ exceptions OpposingCorridor scope " + C0).And.Contain("'Story541'").And.Contain("depuis 5"));
            Assert.That(projection.ToText(), Does.Not.Contain("\nPolicy "), "la decision d'origine est inchangee");
        }

        // ============================================================ gardes structurelles

        [Test]
        public void ThePolicyPlansNoManeuverReadsNoTrafficStateAndGrantsNothing()
        {
            string code = CodeOnly(File.ReadAllText(PolicyPath));
            var forbidden = new Regex(@"\b(TrafficFrame|TrafficActor|AgentObservation|TrafficPerception|LongitudinalPerception|PathHorizon|"
                + @"IPathGeometry|MotionPlan|MotionCommand|SpeedPlan|RoadCurve|CompiledRoadModel|RoutePlan|TrafficRuleAuthority|"
                + @"Junction(Coordinator|Snapshot|Record|ActorReport|ConflictIndex|Traversal|Request\w*|Distances|Approach)|"
                + @"EffectiveRuleException|RuleExceptionRecord|SafetyFilter|VehicleDriveIntent|Rigidbody|Physics|Gap\w*Selection|Overtak\w*|"
                + @"Bypass\w*|Opposing\w*Traffic)\b");
            Assert.That(forbidden.IsMatch(code), Is.False, forbidden.Match(code).Value);
            Assert.That(Regex.IsMatch(code, @"using RoadRage\.Features\.Vehicles\.Traffic\.(Frame|Perception|Planning|Coordination|Safety|Intent|Tactical)"),
                Is.False);
        }

        [Test]
        public void OnlyTheAuthorityCreatesAnEffectiveExceptionAndTheSafetyFilterReadsNone()
        {
            var creators = Directory.GetFiles(TrafficRoot, "*.cs", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/'))
                .Where(p => Regex.IsMatch(CodeOnly(File.ReadAllText(p)), @"new\s+EffectiveRuleException\s*\(")).ToList();
            Assert.That(creators, Is.EqualTo(new[] { TrafficRoot + "/Junction/TrafficRuleAuthority.cs" }));
            Assert.That(typeof(EffectiveRuleException).GetConstructors(), Is.Empty, "aucun constructeur public");
            string safety = CodeOnly(File.ReadAllText(TrafficRoot + "/Safety/SafetyFilter.cs"));
            Assert.That(Regex.IsMatch(safety, @"RuleException|TrafficRule|DrivingPolicy|EffectivePolicy"), Is.False);
        }

        [Test]
        public void TheV2DriverReadsItsParametersThroughThePolicyAndTheAuthorityRunsAfterEveryGrantDecision()
        {
            string driver = CodeOnly(File.ReadAllText(TrafficRoot + "/Lifecycle/TrafficV2VehicleDriver.cs"));
            Assert.That(driver, Does.Not.Contain("driverProfile.Profile"));
            Assert.That(Regex.Matches(driver, @"DrivingPolicy\.Resolve\(").Count, Is.EqualTo(1));
            // Option 1 (2026-10-09) : resolue a chaque pas prepare, comme l'ancienne lecture du profil (fixture 5.35 ScenarioGGap).
            int prepare = driver.IndexOf("private bool PrepareStep(", StringComparison.Ordinal);
            Assert.That(driver.IndexOf("DrivingPolicy.Resolve(", StringComparison.Ordinal),
                Is.GreaterThan(prepare).And.LessThan(driver.IndexOf("private bool WarnIfInert(", StringComparison.Ordinal)));
            Assert.That(driver, Does.Contain("var driver = policy.Driver;"));
            Assert.That(driver, Does.Contain("junctions.TryGetEffectiveExceptions(insertion.TrafficId, frameId, out ruleExceptions)"));
            Assert.That(driver, Does.Contain("command = command.Value.WithRuleExceptions(ruleExceptions)"));
            Assert.That(driver, Does.Contain(".WithPolicy(policy, ruleExceptions)"));
            Assert.That(driver.IndexOf("SafetyFilter.Evaluate(", StringComparison.Ordinal),
                Is.LessThan(driver.IndexOf("WithRuleExceptions(", StringComparison.Ordinal)), "exceptions portees apres le SafetyFilter");
            Assert.That(driver, Does.Not.Contain("DesiredSpeedAt("), "D1 : aucune variation appliquee au runtime en 5.41");

            string coordinator = CodeOnly(File.ReadAllText(TrafficRoot + "/Junction/JunctionCoordinator.cs"));
            int escalation = coordinator.IndexOf("var gridlocks = EscalateGridlocks(", StringComparison.Ordinal);
            int authority = coordinator.IndexOf("ruleAuthority.Resolve(", StringComparison.Ordinal);
            int publication = coordinator.IndexOf("kept.Sort(CompareGrants);", StringComparison.Ordinal);
            Assert.That(escalation, Is.GreaterThan(0));
            Assert.That(authority, Is.GreaterThan(escalation).And.LessThan(publication));
            Assert.That(Regex.Matches(coordinator, @"ruleAuthority\.").Count, Is.EqualTo(5), "Resolve, FailClosed et ActiveCount seulement");
        }

        // ============================================================ outils

        private static RuleExceptionRequest Ask(RoadId who, ulong sourceFrame, TrafficRule rule = TrafficRule.OpposingCorridor,
            RoadId? scope = null, RoadId target = default(RoadId), string reason = "Story541",
            RuleExceptionTerminationKind kind = RuleExceptionTerminationKind.Expiry, ulong? expiry = null, ulong? frame = null)
        {
            ulong batch = frame ?? sourceFrame;
            return new RuleExceptionRequest(who, rule, scope ?? C0, target, reason,
                new RuleExceptionTermination(kind, expiry ?? batch + 10UL), sourceFrame);
        }

        private static void ExpectDenied(RuleExceptionRequest request, RuleExceptionReason reason, string label = null)
        {
            var coordinator = new JunctionCoordinator(Crossing);
            var snapshot = coordinator.Resolve(5UL, new[] { On(A, C0), On(B, C1) }, null, null, new[] { request });
            var record = Only(snapshot);
            Assert.That(record.Status, Is.EqualTo(RuleExceptionStatus.Denied), label + " : " + record.ToText());
            Assert.That(record.Reason, Is.EqualTo(reason), label + " : " + record.ToText());
            Assert.That(record.Exception, Is.Null);
            IReadOnlyList<EffectiveRuleException> effective;
            Assert.That(snapshot.TryGetEffectiveExceptions(A, 6UL, out effective), Is.False, label);
            Assert.That(coordinator.ActiveRuleExceptions, Is.Zero, label);
        }

        private static RuleExceptionRecord Only(JunctionSnapshot snapshot)
        {
            Assert.That(snapshot.RuleExceptions.Count, Is.EqualTo(1), snapshot.ToText());
            return snapshot.RuleExceptions[0];
        }

        private static IReadOnlyList<EffectiveRuleException> AcceptedExceptions()
        {
            var snapshot = new JunctionCoordinator(Crossing).Resolve(5UL, new[] { On(A, C0) }, null, null, new[] { Ask(A, 5UL) });
            IReadOnlyList<EffectiveRuleException> exceptions;
            Assert.That(snapshot.TryGetEffectiveExceptions(A, 6UL, out exceptions), Is.True);
            return exceptions;
        }

        /// <summary>Acteur present hors carrefour, localise sur un element (rapport d'occupation seul).</summary>
        private static JunctionActorReport On(RoadId id, RoadId element, bool localized = true)
        {
            return new JunctionActorReport(id, localized, localized ? element : RoadId.None, FarAway, null, null, null, null, false, false,
                JunctionRequestRejection.NoTraversal, Reservation);
        }

        private sealed class MvpCase
        {
            public TrafficFrame Frame;
            public RoadId TrafficId;
            public PlanningDecision Decision;
        }

        private static MvpCase MvpDecision()
        {
            string modelText = File.ReadAllText(TrafficV2Settings.ModelPath);
            var model = RoadModelCompiler.Compile(RoadModelDocument.Load(modelText));
            var entry = model.Portals.First(p => p.Role == PortalRole.Entry);
            EffectiveLaneCorridor corridor;
            Assert.That(model.TryGetCorridor(entry.CorridorId, out corridor), Is.True);
            var point = corridor.Curve.Sample(entry.SMeters);
            var trafficId = new RoadId(0x541UL, 77UL);
            var frame = new TrafficFrame(23UL, model, new[] { new TrafficActorInput(trafficId,
                new VehicleFootprintPose { Position = point.Position, Forward = point.Tangent, Up = point.Up }, 0f, entry.CorridorId) });
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null, RoadId.None, new RouteSeed(1), 200f,
                modelText, File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath), Driver,
                bounds: new LongitudinalBounds(2f, 3f)));
            Assert.That(decision.Projection, Is.Not.Null);
            Assert.That(decision.Path, Is.Not.Null);
            return new MvpCase { Frame = frame, TrafficId = trafficId, Decision = decision };
        }

        private static string WithoutPairCost(string counters)
        {
            return Regex.Replace(counters, @" / paires \d+", "");
        }

        private static string CodeOnly(string source)
        {
            return Regex.Replace(Regex.Replace(source, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\n]*", "");
        }

        // Carrefour en memoire (patron Story540GridlockTests / Story535JunctionRulesTests.CrossingSource).
        private static JunctionTraversal Traversal(JunctionConflictIndex index, params RoadId[] movements)
        {
            return new JunctionTraversal(index.JunctionOf(movements[0]), movements, index.ToCorridorOf(movements[movements.Length - 1]));
        }

        private static JunctionActorReport Requesting(CompiledRoadModel m, RoadId id, JunctionTraversal traversal, float d, float v)
        {
            var index = JunctionConflictIndex.For(m);
            var driver = Driver;
            var distances = JunctionDistances.For(driver, v, Dt, TrafficV2Settings.JunctionStopControlMarginMeters);
            float b = index.BoundaryOf(traversal.FirstMovementId);
            var approach = new JunctionApproach(traversal, d, distances, true, RoadId.None, false, false,
                new JunctionExitAssessment(100f, JunctionExitBound.Occupant, RoadId.None, Reservation), new[] { d - b }, b);
            bool valid = d <= distances.RequestThresholdMeters;
            var positions = traversal.MovementIds.Select(x => new JunctionMovementPosition(x, JunctionMovementStatus.Ahead)).ToArray();
            return new JunctionActorReport(id, true, index.FromCorridorOf(traversal.FirstMovementId), FarAway, null, null, positions,
                new[] { approach }, true, valid, valid ? JunctionRequestRejection.None : JunctionRequestRejection.TooFar, Reservation,
                new JunctionKinematics(v, driver.MaxAcceleration, driver.DesiredSpeed, driver.ComfortableDeceleration, CarLength));
        }

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
                for (int j = 0; j < 2; j++)
                {
                    int k = i * 2 + j;
                    source.Sections[k] = new RoadSection { Id = Id(1100 + k), DefaultSpeedLimitMetersPerSecond = 10f,
                        DefaultAllowedVehicleClasses = VehicleClassMask.All };
                    source.Corridors[k] = new LaneCorridor { Id = Id(1200 + k), SectionId = source.Sections[k].Id,
                        IsCrossSectionDatum = true, LengthMeters = 40f,
                        Samples = Straight(tangent * (j == 0 ? -45f : 5f), tangent, 40f, source.ValidationProfile) };
                }
                source.Movements[i] = new JunctionMovement { Id = Id(1300 + i), JunctionId = Id(1001),
                    FromCorridorId = source.Corridors[i * 2].Id, ToCorridorId = source.Corridors[i * 2 + 1].Id,
                    LengthMeters = 10f, Samples = Straight(-tangent * 5f, tangent, 10f, source.ValidationProfile) };
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
