using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Rage;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Policy;
using RoadRage.Features.Vehicles.Traffic.Recovery;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.43 -- rage et peur comme modulation continue de la politique : jauges (E1 decroissance et gel, E2 maintien
    /// verrouille, E3 arbitrage), lecture etroite (E5), modulation continue et bornee (E4), blocker legitime d'une
    /// immobilisation voulue, integration hote de NetworkedRageState (valeurs V1 inchangees) et gardes structurelles.
    /// </summary>
    [Category("Core")]
    [Category("Story543")]
    public sealed class Story543EmotionModulationTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string RageTuningPath = "Assets/RoadRage/ScriptableObjects/Rage/RageTuningDef_Default.asset";
        private const string TrafficRoot = "Assets/RoadRage/Features/Vehicles/Traffic";
        private const float Dt = 0.02f;

        private static readonly RoadId A = new RoadId(0x543UL, 1UL);
        private static readonly RoadId B = new RoadId(0x543UL, 2UL);
        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };

        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        private static DriverProfileDef Def { get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath); } }

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in spawned)
                if (instance != null) UnityEngine.Object.DestroyImmediate(instance);
            spawned.Clear();
        }

        // ============================================================ E1 : decroissance et gel

        [Test]
        public void EachMeterDecaysAtItsAuthoredPercentOfMaxPerSecondDownToZero()
        {
            var tuning = Tuning();
            var state = EmotionMeters.Change(default(EmotionMeterState), 40f, 20f, tuning);
            Assert.That(tuning.RageDecayPercentPerSecond, Is.EqualTo(1f));
            Assert.That(tuning.FearDecayPercentPerSecond, Is.EqualTo(1f));
            Assert.That(tuning.TierFreezeSeconds, Is.EqualTo(10f));

            var after = EmotionMeters.Advance(state, 5f, tuning, false);
            Assert.That(after.Rage, Is.EqualTo(35f).Within(1e-4f), "matrice : rage 40, palier non entre, 5 s");
            Assert.That(after.Fear, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(EmotionMeters.Advance(after, 100f, tuning, false).Rage, Is.EqualTo(0f), "plancher 0");

            var fast = Tuning(rageDecay: 2f, fearDecay: 0f);
            var authored = EmotionMeters.Advance(EmotionMeters.Change(default(EmotionMeterState), 40f, 20f, fast), 5f, fast, false);
            Assert.That(authored.Rage, Is.EqualTo(30f).Within(1e-4f), "taux authore");
            Assert.That(authored.Fear, Is.EqualTo(20f), "taux nul : aucune decroissance");
            Assert.That(EmotionMeters.Advance(state, 0f, tuning, false).Rage, Is.EqualTo(40f));
            Assert.That(EmotionMeters.Advance(state, float.NaN, tuning, false).Rage, Is.EqualTo(40f));
        }

        [Test]
        public void EnteringAHigherTierFreezesRageDecayWhileTheMeterCanStillRise()
        {
            var tuning = Tuning(thresholds: new[] { 20f, 40f });
            var below = EmotionMeters.Change(default(EmotionMeterState), 19f, 0f, tuning);
            Assert.That(below.FreezeSeconds, Is.EqualTo(0f));
            var entered = EmotionMeters.Change(below, 21f, 0f, tuning);
            Assert.That(entered.Tier, Is.EqualTo(1));
            Assert.That(entered.FreezeSeconds, Is.EqualTo(10f));

            var frozen = EmotionMeters.Advance(entered, 5f, tuning, false);
            Assert.That(frozen.Rage, Is.EqualTo(21f), "matrice : gele 5 s");
            var risen = EmotionMeters.Change(frozen, 31f, 0f, tuning);
            Assert.That(risen.Rage, Is.EqualTo(31f), "la jauge monte pendant le gel");
            Assert.That(risen.FreezeSeconds, Is.EqualTo(5f), "meme palier : le gel n'est pas relance");
            var thawed = EmotionMeters.Advance(risen, 5f, tuning, false);
            Assert.That(thawed.Rage, Is.EqualTo(31f), "fin exacte du gel");
            Assert.That(EmotionMeters.Advance(thawed, 1f, tuning, false).Rage, Is.EqualTo(30f).Within(1e-4f), "decroissance reprise");

            var next = EmotionMeters.Change(thawed, 41f, 0f, tuning);
            Assert.That(next.FreezeSeconds, Is.EqualTo(10f), "un nouveau palier relance le gel");
            Assert.That(EmotionMeters.Advance(next, 9f, tuning, false).Rage, Is.EqualTo(41f));
            var partial = EmotionMeters.Advance(EmotionMeters.Advance(next, 9f, tuning, false), 2f, tuning, false);
            Assert.That(partial.Rage, Is.EqualTo(40f).Within(1e-4f), "seule la part hors gel decroit");
        }

        // ============================================================ E2 : maintien verrouille

        [Test]
        public void RageAtMaxHoldsUntilTheAssociatedEventHasBeenActiveAndThenLeaves()
        {
            var tuning = Tuning();
            var held = EmotionMeters.Change(default(EmotionMeterState), 100f, 0f, tuning);
            Assert.That(held.RageHold, Is.True);
            held = EmotionMeters.Advance(held, 60f, tuning, false);
            Assert.That(held.Rage, Is.EqualTo(100f), "matrice : aucun evenement, 60 s");
            held = EmotionMeters.Advance(held, 30f, tuning, true);
            Assert.That(held.Rage, Is.EqualTo(100f), "evenement actif");
            var released = EmotionMeters.Advance(held, 1f, tuning, false);
            Assert.That(released.RageHold, Is.False);
            Assert.That(released.Rage, Is.EqualTo(99f).Within(1e-4f), "matrice : decroit des le pas qui suit la sortie");

            var rearmed = EmotionMeters.Change(released, 100f, 0f, tuning);
            Assert.That(rearmed.RageHold && !rearmed.HoldEventSeen, Is.True, "une nouvelle montee au max rearme le maintien");
            var lowered = EmotionMeters.Change(rearmed, 90f, 0f, tuning);
            Assert.That(lowered.RageHold, Is.False, "un delta negatif explicite sous le max libere le maintien");
            Assert.That(EmotionMeters.Advance(lowered, 1f, tuning, false).Rage, Is.EqualTo(89f).Within(1e-4f));
        }

        // ============================================================ E3 : arbitrage

        [TestCase(100f, 100f, EmotionGovernor.FearSaturated)]
        [TestCase(0f, 100f, EmotionGovernor.FearSaturated)]
        [TestCase(40f, 50f, EmotionGovernor.Escape)]
        [TestCase(50f, 50f, EmotionGovernor.Rage)]
        [TestCase(0f, 49f, EmotionGovernor.Rage)]
        [TestCase(80f, 99f, EmotionGovernor.Escape)]
        [TestCase(100f, 99f, EmotionGovernor.Rage)]
        public void ArbitrationFollowsTheAuthoredThresholdMatrix(float rage, float fear, EmotionGovernor expected)
        {
            var tuning = Tuning();
            var reading = EmotionMeters.Read(EmotionMeters.Change(default(EmotionMeterState), rage, fear, tuning), tuning);
            Assert.That(reading.Governor, Is.EqualTo(expected));
            Assert.That(reading.RageWeight, Is.EqualTo(expected == EmotionGovernor.Rage ? rage / 100f : 0f).Within(1e-6f));
            Assert.That(reading.FearWeight, Is.EqualTo(expected == EmotionGovernor.Rage ? 0f : fear / 100f).Within(1e-6f));
        }

        [Test]
        public void EscapeHoldsWhateverTheRageUntilFearFallsToTheExitThreshold()
        {
            var tuning = Tuning();
            var state = EmotionMeters.Change(default(EmotionMeterState), 40f, 50f, tuning);
            Assert.That(EmotionMeters.Read(state, tuning).Governor, Is.EqualTo(EmotionGovernor.Escape));
            state = EmotionMeters.Change(state, 90f, 31f, tuning);
            Assert.That(EmotionMeters.Read(state, tuning).Governor, Is.EqualTo(EmotionGovernor.Escape), "matrice : tenue a peur 31, rage 90");
            state = EmotionMeters.Change(state, 90f, 30f, tuning);
            Assert.That(EmotionMeters.Read(state, tuning).Governor, Is.EqualTo(EmotionGovernor.Rage), "sortie a peur <= 30");

            // Decroissance jusqu'a la sortie : la fuite tient tant que la peur depasse 30 %.
            var fleeing = EmotionMeters.Change(default(EmotionMeterState), 0f, 50f, tuning);
            fleeing = EmotionMeters.Advance(fleeing, 19f, tuning, false);
            Assert.That(EmotionMeters.Read(fleeing, tuning).Governor, Is.EqualTo(EmotionGovernor.Escape));
            fleeing = EmotionMeters.Advance(fleeing, 1.5f, tuning, false);
            Assert.That(EmotionMeters.Read(fleeing, tuning).Governor, Is.EqualTo(EmotionGovernor.Rage));
        }

        [Test]
        public void EveryArbitrationThresholdIsAuthored()
        {
            var tuning = Tuning(enter: 40f, exit: 20f, overrideAt: 90f);
            var escape = EmotionMeters.Change(default(EmotionMeterState), 10f, 45f, tuning);
            Assert.That(EmotionMeters.Read(escape, tuning).Governor, Is.EqualTo(EmotionGovernor.Escape));
            Assert.That(EmotionMeters.Read(EmotionMeters.Change(escape, 10f, 25f, tuning), tuning).Governor, Is.EqualTo(EmotionGovernor.Escape));
            Assert.That(EmotionMeters.Read(EmotionMeters.Change(escape, 10f, 20f, tuning), tuning).Governor, Is.EqualTo(EmotionGovernor.Rage));
            Assert.That(EmotionMeters.Read(EmotionMeters.Change(default(EmotionMeterState), 100f, 90f, tuning), tuning).Governor,
                Is.EqualTo(EmotionGovernor.FearSaturated));
            Assert.That(EmotionMeters.Read(default(EmotionMeterState), null).Governor, Is.EqualTo(EmotionGovernor.Rage), "sans tuning : calme");
        }

        [TestCase("escapeExitPercent", 50f)]
        [TestCase("escapeEnterPercent", 101f)]
        [TestCase("fearOverridePercent", 40f)]
        [TestCase("rageDecayPercentPerSecond", -1f)]
        [TestCase("fearDecayPercentPerSecond", float.NaN)]
        [TestCase("tierFreezeSeconds", -0.5f)]
        public void AnInvalidAuthoredEvolutionFailsValidation(string field, float value)
        {
            var tuning = Tuning();
            string error;
            Assert.That(tuning.TryValidate(out error), Is.True, error);
            SetPrivate(tuning, field, value);
            Assert.That(tuning.TryValidate(out error), Is.False);
            Assert.That(AssetDatabase.LoadAssetAtPath<RageTuningDef>(RageTuningPath).TryValidate(out error), Is.True, error);
        }

        // ============================================================ E5 : lecture

        [Test]
        public void AReadingWithANonFiniteComponentIsEntirelyCalmAndFiniteComponentsAreBounded()
        {
            foreach (var reading in new[] { new EmotionReading(float.NaN, 0.8f, EmotionGovernor.Escape),
                new EmotionReading(0.5f, float.PositiveInfinity, EmotionGovernor.Rage), new EmotionReading(0.5f, 0.5f, (EmotionGovernor)7) })
            {
                Assert.That(reading.Rage01, Is.EqualTo(0f));
                Assert.That(reading.Fear01, Is.EqualTo(0f));
                Assert.That(reading.Governor, Is.EqualTo(EmotionGovernor.Rage));
                Assert.That(reading.RageWeight + reading.FearWeight, Is.EqualTo(0f));
            }
            var bounded = new EmotionReading(1.7f, -0.3f, EmotionGovernor.Rage);
            Assert.That(bounded.Rage01, Is.EqualTo(1f));
            Assert.That(bounded.Fear01, Is.EqualTo(0f));
            Assert.That(default(EmotionReading).RageWeight + default(EmotionReading).FearWeight, Is.EqualTo(0f), "Calm = default");
        }

        // ============================================================ E4 : modulation continue et bornee

        [Test]
        public void ACalmReadingResolvesExactlyThe541Policy()
        {
            var def = Def;
            var legacy = DrivingPolicy.Resolve(def.Profile, def.Policy, A, 7UL);
            foreach (var calm in new[] { DrivingPolicy.Resolve(def, A, 7UL), DrivingPolicy.Resolve(def, A, 7UL, EmotionReading.Calm),
                DrivingPolicy.Resolve(def, A, 7UL, new EmotionReading(float.NaN, 1f, EmotionGovernor.FearSaturated)) })
            {
                AssertSameBits(calm.Driver, def.Profile);
                AssertSameBits(calm.Driver, legacy.Driver);
                Assert.That(Bits(calm.AcceptedRisk), Is.EqualTo(Bits(def.Policy.AcceptedRisk)));
                Assert.That(Bits(calm.AcceptedGapSeconds), Is.EqualTo(Bits(def.Policy.AcceptedGapSeconds)));
                for (int i = 0; i < DrivingPolicy.ManeuverCount; i++)
                {
                    var kind = (ManeuverKind)i;
                    Assert.That(Bits(calm.Maneuver(kind).Cost), Is.EqualTo(Bits(def.Policy.PreferenceOf(kind).Cost)), kind.ToString());
                    Assert.That(calm.Maneuver(kind).Eligible, Is.EqualTo(legacy.Maneuver(kind).Eligible));
                }
                Assert.That(calm.VariationPhase, Is.EqualTo(legacy.VariationPhase));
            }
        }

        [Test]
        public void EveryLeverIsAContinuousMonotonicFunctionOfTheGoverningWeight()
        {
            var def = Def;
            var gains = EmotionModulation.Default;
            float previousSpeed = -1f, previousHeadway = float.MaxValue;
            for (int step = 0; step <= 20; step++)
            {
                float w = step / 20f;
                var rage = DrivingPolicy.Resolve(def, A, 1UL, new EmotionReading(w, 0f, EmotionGovernor.Rage));
                Assert.That(rage.Driver.DesiredSpeed, Is.EqualTo(def.Profile.DesiredSpeed * (1f + gains.Rage.DesiredSpeed * w)).Within(1e-4f));
                Assert.That(rage.Driver.TimeHeadway, Is.EqualTo(def.Profile.TimeHeadway * (1f + gains.Rage.TimeHeadway * w)).Within(1e-4f));
                Assert.That(rage.Driver.MinimumGap, Is.EqualTo(def.Profile.MinimumGap * (1f + gains.Rage.MinimumGap * w)).Within(1e-4f));
                Assert.That(rage.Driver.MaxAcceleration, Is.EqualTo(def.Profile.MaxAcceleration * (1f + gains.Rage.MaxAcceleration * w)).Within(1e-4f));
                Assert.That(rage.Driver.ComfortableDeceleration,
                    Is.EqualTo(def.Profile.ComfortableDeceleration * (1f + gains.Rage.ComfortableDeceleration * w)).Within(1e-4f));
                Assert.That(rage.Driver.SafeBrakingLimit, Is.EqualTo(def.Profile.SafeBrakingLimit * (1f + gains.Rage.SafeBrakingLimit * w)).Within(1e-4f));
                Assert.That(rage.AcceptedRisk, Is.EqualTo(Mathf.Clamp01(def.Policy.AcceptedRisk * (1f + gains.Rage.AcceptedRisk * w))).Within(1e-5f));
                Assert.That(rage.AcceptedGapSeconds, Is.EqualTo(def.Policy.AcceptedGapSeconds * (1f + gains.Rage.AcceptedGap * w)).Within(1e-4f));
                Assert.That(rage.Maneuver(ManeuverKind.OpposingCorridor).Cost,
                    Is.EqualTo(def.Policy.PreferenceOf(ManeuverKind.OpposingCorridor).Cost * (1f + gains.Rage.ManeuverCost * w)).Within(1e-5f));
                Assert.That(rage.Driver.DesiredSpeed, Is.GreaterThanOrEqualTo(previousSpeed), "monotone");
                Assert.That(rage.Driver.TimeHeadway, Is.LessThanOrEqualTo(previousHeadway), "monotone");
                previousSpeed = rage.Driver.DesiredSpeed;
                previousHeadway = rage.Driver.TimeHeadway;

                // Continuite : un ecart infime du poids ne produit qu'un ecart infime du parametre.
                var near = DrivingPolicy.Resolve(def, A, 1UL, new EmotionReading(Mathf.Min(1f, w + 1e-4f), 0f, EmotionGovernor.Rage));
                Assert.That(Math.Abs(near.Driver.DesiredSpeed - rage.Driver.DesiredSpeed), Is.LessThan(1e-2f));

                var fear = DrivingPolicy.Resolve(def, A, 1UL, new EmotionReading(0f, w, EmotionGovernor.Escape));
                Assert.That(fear.Driver.TimeHeadway, Is.EqualTo(def.Profile.TimeHeadway * (1f + gains.Fear.TimeHeadway * w)).Within(1e-4f));
                Assert.That(fear.AcceptedRisk, Is.EqualTo(def.Policy.AcceptedRisk * (1f + gains.Fear.AcceptedRisk * w)).Within(1e-5f));
            }
        }

        [Test]
        public void OnlyTheGoverningMeterModulatesAndNothingElseOfThePersonalityMoves()
        {
            var def = Def;
            var rageOnly = DrivingPolicy.Resolve(def, A, 1UL, new EmotionReading(0.8f, 0f, EmotionGovernor.Rage));
            var withFear = DrivingPolicy.Resolve(def, A, 1UL, new EmotionReading(0.8f, 0.4f, EmotionGovernor.Rage));
            AssertSameBits(withFear.Driver, rageOnly.Driver);
            var fearOnly = DrivingPolicy.Resolve(def, A, 1UL, new EmotionReading(0f, 0.6f, EmotionGovernor.Escape));
            var withRage = DrivingPolicy.Resolve(def, A, 1UL, new EmotionReading(0.9f, 0.6f, EmotionGovernor.Escape));
            AssertSameBits(withRage.Driver, fearOnly.Driver);

            var raged = DrivingPolicy.Resolve(def, A, 1UL, new EmotionReading(1f, 0f, EmotionGovernor.Rage));
            Assert.That(raged.Driver.Politeness, Is.EqualTo(def.Profile.Politeness));
            Assert.That(raged.Driver.LaneChangeThreshold, Is.EqualTo(def.Profile.LaneChangeThreshold));
            Assert.That(raged.Driver.ReactionTime, Is.EqualTo(def.Profile.ReactionTime));
            Assert.That(raged.Driver.Consistency, Is.EqualTo(def.Profile.Consistency));
            Assert.That(raged.Driver.AimPointRecallSpeed, Is.EqualTo(def.Profile.AimPointRecallSpeed));
            Assert.That(raged.AllowedSurfaces, Is.EqualTo(def.Policy.AllowedSurfaces));
            for (int i = 0; i < DrivingPolicy.ManeuverCount; i++)
                Assert.That(raged.Maneuver((ManeuverKind)i).Eligible, Is.EqualTo(DrivingPolicy.Resolve(def, A, 1UL).Maneuver((ManeuverKind)i).Eligible));
            AssertSameBits(raged.AuthoredDriver, def.Profile);
            Assert.That(raged.Driver.SafeBrakingLimit, Is.GreaterThan(raged.AuthoredDriver.SafeBrakingLimit), "b_safe effectif module");
            AssertSameBits(def.Profile, Def.Profile);
        }

        [Test]
        public void ModulatedLeversStayInTheirDeclaredDomains()
        {
            var floor = new EmotionGains(-1f, -1f, -1f, -0.99f, -0.99f, -1f, -1f, -1f, -1f);
            var ceiling = new EmotionGains(5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f);
            foreach (var modulation in new[] { new EmotionModulation(floor, floor), new EmotionModulation(ceiling, ceiling) })
            {
                string error;
                Assert.That(modulation.TryValidate(out error), Is.True, error);
                foreach (var reading in new[] { new EmotionReading(1f, 0f, EmotionGovernor.Rage), new EmotionReading(0f, 1f, EmotionGovernor.FearSaturated) })
                {
                    var policy = DrivingPolicy.Resolve(Def.Profile, Def.Policy, A, 1UL, modulation, reading);
                    Assert.That(policy.Driver.DesiredSpeed, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(policy.Driver.TimeHeadway, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(policy.Driver.MinimumGap, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(policy.Driver.MaxAcceleration, Is.GreaterThan(0f));
                    Assert.That(policy.Driver.ComfortableDeceleration, Is.GreaterThan(0f));
                    Assert.That(policy.Driver.SafeBrakingLimit, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(policy.AcceptedRisk, Is.InRange(0f, 1f));
                    Assert.That(policy.AcceptedGapSeconds, Is.GreaterThanOrEqualTo(0f));
                    for (int i = 0; i < DrivingPolicy.ManeuverCount; i++)
                        Assert.That(policy.Maneuver((ManeuverKind)i).Cost, Is.GreaterThanOrEqualTo(0f));
                }
            }
            Assert.That(DrivingPolicy.Resolve(Def.Profile, Def.Policy, A, 1UL, new EmotionModulation(ceiling, ceiling),
                new EmotionReading(1f, 0f, EmotionGovernor.Rage)).AcceptedRisk, Is.EqualTo(1f), "risque borne a 1");

            // Modulation non valide : ignoree, jamais propagee en valeur non finie.
            var invalid = new EmotionModulation(new EmotionGains(float.NaN, 0f, 0f, -1f, 0f, 0f, 0f, 0f, 0f), floor);
            string invalidError;
            Assert.That(invalid.TryValidate(out invalidError), Is.False);
            var ignored = DrivingPolicy.Resolve(Def.Profile, Def.Policy, A, 1UL, invalid, new EmotionReading(1f, 0f, EmotionGovernor.Rage));
            AssertSameBits(ignored.Driver, Def.Profile);
        }

        [TestCase(-1.01f, 0f)]
        [TestCase(0f, -1f)]
        [TestCase(float.PositiveInfinity, 0f)]
        public void AnInvalidAuthoredModulationFailsValidation(float speedGain, float accelerationGain)
        {
            var gains = new EmotionGains(speedGain, 0f, 0f, accelerationGain, 0f, 0f, 0f, 0f, 0f);
            string error;
            Assert.That(new EmotionModulation(gains, default(EmotionGains)).TryValidate(out error), Is.False);
            Assert.That(new EmotionModulation(default(EmotionGains), gains).TryValidate(out error), Is.False);
            Assert.That(EmotionModulation.Default.TryValidate(out error), Is.True, error);
            Assert.That(Def.TryValidate(out error), Is.True, error);
        }

        [Test]
        public void OneVehiclesEmotionNeverChangesAnothersPolicy()
        {
            var calmB = DrivingPolicy.Resolve(Def, B, 11UL);
            var enragedA = DrivingPolicy.Resolve(Def, A, 11UL, new EmotionReading(1f, 0f, EmotionGovernor.Rage));
            var againB = DrivingPolicy.Resolve(Def, B, 11UL);
            AssertSameBits(againB.Driver, calmB.Driver);
            Assert.That(Bits(againB.AcceptedRisk), Is.EqualTo(Bits(calmB.AcceptedRisk)));
            Assert.That(enragedA.Driver.DesiredSpeed, Is.Not.EqualTo(calmB.Driver.DesiredSpeed));

            var first = NewRageState("Story543A");
            var second = NewRageState("Story543B");
            var tuning = AssetDatabase.LoadAssetAtPath<RageTuningDef>(RageTuningPath);
            first.ApplyRageDelta(90f, tuning);
            second.ApplyFearDelta(0f, tuning);
            Assert.That(second.CurrentEmotion.Rage01, Is.EqualTo(0f));
            Assert.That(first.CurrentEmotion.Rage01, Is.EqualTo(0.9f).Within(1e-6f));
        }

        // ============================================================ immobilisation voulue

        [Test]
        public void ADeliberateImmobilizationIsALegitimateBlockerAndNeverExpectsProgress()
        {
            var immobilizing = new EmotionModulation(new EmotionGains(-1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f), default(EmotionGains));
            var policy = DrivingPolicy.Resolve(Def.Profile, Def.Policy, A, 1UL, immobilizing, new EmotionReading(1f, 0f, EmotionGovernor.Rage));
            Assert.That(policy.Driver.DesiredSpeed, Is.EqualTo(0f), "matrice : gain v0 = -1, wR = 1");

            var decision = DecideAtRest(policy.Driver);
            var blockers = BlockerTracker.Update(null, decision, policy.Driver, 8);
            var blocker = blockers.Single(b => b.Kind == BlockerKind.PolicyImmobilization);
            Assert.That(blocker.Source, Is.EqualTo(BlockerSource.DrivingPolicy));
            Assert.That(blocker.Legitimate && !blocker.Recoverable, Is.True, "legitime, aucune recuperation");
            float route;
            Assert.That(RecoverySupervisor.ProgressExpected(decision, blockers, out route), Is.False, "aucune progression attendue");
        }

        // ============================================================ integration hote

        [Test]
        public void TheHostStateKeepsTheV1ValuesAndEvolvesOnlyWhenAdvanced()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<RageTuningDef>(RageTuningPath);
            var state = NewRageState("Story543Host");
            Assert.That(state.CurrentEmotion.Rage01 + state.CurrentEmotion.Fear01, Is.EqualTo(0f), "calme sans tuning applique");

            state.ApplyRageDelta(25f, tuning);
            Assert.That(state.RageValue.Value, Is.EqualTo(25f), "valeur V1");
            Assert.That(state.Disposition.Value, Is.EqualTo(tuning.ResolveDisposition(25f)), "disposition V1");
            Assert.That(state.CurrentEmotion.Rage01, Is.EqualTo(0.25f).Within(1e-6f));
            Assert.That(state.CurrentEmotion.Governor, Is.EqualTo(EmotionGovernor.Rage));

            state.Advance(5f, tuning, false);
            Assert.That(state.RageValue.Value, Is.EqualTo(25f), "palier 20 entre : gel");
            state.Advance(10f, tuning, false);
            Assert.That(state.RageValue.Value, Is.EqualTo(20f).Within(1e-4f), "5 s gelees puis 5 s a 1 %/s");
            Assert.That(state.Disposition.Value, Is.EqualTo(tuning.ResolveDisposition(state.RageValue.Value)));

            state.ApplyFearDelta(60f, tuning);
            Assert.That(state.FearValue.Value, Is.EqualTo(60f), "valeur V1");
            Assert.That(state.CurrentEmotion.Governor, Is.EqualTo(EmotionGovernor.Escape));
            state.ApplyReactionEffect(new NpcReactionEffect(ReactionChannel.Rage, 75f), tuning);
            Assert.That(state.RageValue.Value, Is.EqualTo(Mathf.Clamp(20f + 75f * tuning.RageSensitivity, 0f, tuning.MaxRageValue)).Within(1e-4f));
            Assert.That(state.CurrentEmotion.Governor, Is.EqualTo(EmotionGovernor.Escape), "fuite tenue tant que la peur depasse 30 %");

            state.ApplyRageDelta(100f, tuning);
            state.Advance(60f, tuning, false);
            Assert.That(state.RageValue.Value, Is.EqualTo(tuning.MaxRageValue), "maintien verrouille sans evenement");
            state.Advance(1f, null, false);
            Assert.That(state.RageValue.Value, Is.EqualTo(tuning.MaxRageValue), "tuning absent : no-op");
        }

        [Test]
        public void TheV2DriverReadsItsOwnEmotionSourceAndFallsBackToCalm()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<RageTuningDef>(RageTuningPath);
            var root = new GameObject("Story543Driver");
            spawned.Add(root);
            root.AddComponent<NetworkObject>();
            var driver = root.AddComponent<TrafficV2VehicleDriver>();
            CallPrivate(driver, "Awake");
            Assert.That(DriverEmotion(driver).RageWeight + DriverEmotion(driver).FearWeight, Is.EqualTo(0f), "sans source : calme");

            var state = root.AddComponent<NetworkedRageState>();
            CallPrivate(driver, "Awake");
            state.ApplyRageDelta(60f, tuning);
            var reading = DriverEmotion(driver);
            Assert.That(reading.Governor, Is.EqualTo(EmotionGovernor.Rage));
            Assert.That(reading.RageWeight, Is.EqualTo(0.6f).Within(1e-6f), "lecture de la source du meme GameObject");
            Assert.That(DrivingPolicy.Resolve(Def, A, 1UL, reading).Driver.DesiredSpeed, Is.GreaterThan(Def.Profile.DesiredSpeed));

            UnityEngine.Object.DestroyImmediate(state);
            Assert.That(DriverEmotion(driver).RageWeight + DriverEmotion(driver).FearWeight, Is.EqualTo(0f), "source detruite : calme");
        }

        [Test]
        public void AFiniteButHugeGainFallsBackToTheAuthoredPolicyAndTheKernelIgnoresAnInvalidModulation()
        {
            var huge = new EmotionGains(1e38f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 1e38f);
            var modulation = new EmotionModulation(huge, default(EmotionGains));
            string error;
            Assert.That(modulation.TryValidate(out error), Is.True, error);
            var policy = DrivingPolicy.Resolve(Def.Profile, Def.Policy, A, 1UL, modulation, new EmotionReading(1f, 0f, EmotionGovernor.Rage));
            AssertSameBits(policy.Driver, Def.Profile);
            Assert.That(float.IsFinite(policy.Maneuver(ManeuverKind.CorridorOffset).Cost), Is.True);

            var invalid = new EmotionModulation(new EmotionGains(float.NaN, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f), default(EmotionGains));
            AssertSameBits(DriverModel.ResolveEffectiveProfile(Def.Profile, invalid, new EmotionReading(1f, 0f, EmotionGovernor.Rage)), Def.Profile);
        }

        [Test]
        public void AMeterWrittenDirectlyOrADifferentTuningNeverArmsASpuriousFreeze()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<RageTuningDef>(RageTuningPath);
            var state = NewRageState("Story543Seed");
            state.RageValue.Value = 50f;
            state.ApplyRageDelta(-1f, tuning);
            state.Advance(1f, tuning, false);
            Assert.That(state.RageValue.Value, Is.EqualTo(48f).Within(1e-4f), "aucun gel sur un palier deja franchi");

            var other = Tuning(thresholds: new[] { 10f, 20f, 30f, 40f });
            state.ApplyRageDelta(-1f, other);
            state.Advance(1f, other, false);
            Assert.That(state.RageValue.Value, Is.EqualTo(46f).Within(1e-4f), "palier reamorce sur le nouveau tuning");
        }

        // ============================================================ gardes structurelles

        [Test]
        public void NoEmotionalBranchReachesTheDrivingStackAndModulationNeverTouchesPhysics()
        {
            foreach (var file in Directory.GetFiles(TrafficRoot, "*.cs", SearchOption.AllDirectories))
            {
                string code = CodeOnly(File.ReadAllText(file));
                Assert.That(code, Does.Not.Contain("Features.Rage"), file);
                Assert.That(code, Does.Not.Contain("RageDisposition"), file);
                Assert.That(code, Does.Not.Contain("EmotionGovernor."), file);
                Assert.That(Regex.IsMatch(code, @"Governor\s*(==|!=)|\b(if|switch|case|while)\b[^;{]*Governor"), Is.False, file);
            }
            foreach (var file in new[] { TrafficRoot + "/Policy/DrivingPolicy.cs", "Assets/RoadRage/Features/Vehicles/EmotionModulation.cs" })
                Assert.That(CodeOnly(File.ReadAllText(file)), Does.Not.Contain("VehicleProfile").And.Not.Contain("Rigidbody"), file);
            var resolve = typeof(DrivingPolicy).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "Resolve")
                .SelectMany(m => m.GetParameters()).Select(p => p.ParameterType.Name);
            Assert.That(resolve, Has.None.Contains("VehicleProfile"));

            string vehiclesAsmdef = File.ReadAllText("Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef");
            Assert.That(vehiclesAsmdef, Does.Not.Contain("RoadRage.Features.Rage"));
        }

        [Test]
        public void TheV2DriverReadsEmotionThroughTheNarrowSourceAndTheFallbackKeepsTheAuthoredSafeBraking()
        {
            string driver = CodeOnly(File.ReadAllText(TrafficRoot + "/Lifecycle/TrafficV2VehicleDriver.cs"));
            Assert.That(Regex.Matches(driver, @"DrivingPolicy\.Resolve\(").Count, Is.EqualTo(1));
            Assert.That(driver, Does.Contain("DrivingPolicy.Resolve(driverProfile, insertion.TrafficId, insertion.Seed.Value, CurrentEmotion())"));
            Assert.That(driver, Does.Contain("emotionSource = GetComponent<RoadRage.Shared.Domain.IEmotionSource>();"));
            Assert.That(driver, Does.Contain("new VehicleDriveIntentComposer(physicsBody.Profile, policy.AuthoredDriver.SafeBrakingLimit, dt)"));
            Assert.That(driver, Does.Not.Contain("DesiredSpeedAt("), "variation non appliquee au runtime en 5.43");

            string state = CodeOnly(File.ReadAllText("Assets/RoadRage/Features/Rage/NetworkedRageState.cs"));
            Assert.That(Regex.IsMatch(state, @"\bvoid\s+(Update|FixedUpdate|LateUpdate|OnNetworkSpawn)\s*\("), Is.False, "aucun tick automatique");
            Assert.That(typeof(IEmotionSource).IsAssignableFrom(typeof(NetworkedRageState)), Is.True);
        }

        // ============================================================ outils

        private RageTuningDef Tuning(float rageDecay = 1f, float fearDecay = 1f, float enter = 50f, float exit = 30f,
            float overrideAt = 100f, float[] thresholds = null)
        {
            var tuning = ScriptableObject.CreateInstance<RageTuningDef>();
            spawned.Add(tuning);
            SetPrivate(tuning, "id", "rage_story543");
            SetPrivate(tuning, "rageDecayPercentPerSecond", rageDecay);
            SetPrivate(tuning, "fearDecayPercentPerSecond", fearDecay);
            SetPrivate(tuning, "escapeEnterPercent", enter);
            SetPrivate(tuning, "escapeExitPercent", exit);
            SetPrivate(tuning, "fearOverridePercent", overrideAt);
            if (thresholds != null)
                SetPrivate(tuning, "thresholds", thresholds.Select((v, i) => new RageTuningDef.RageThreshold((RageDisposition)(i + 1), v)).ToArray());
            return tuning;
        }

        private NetworkedRageState NewRageState(string name)
        {
            var root = new GameObject(name);
            root.AddComponent<NetworkObject>();
            spawned.Add(root);
            return root.AddComponent<NetworkedRageState>();
        }

        private static void SetPrivate(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, field);
            info.SetValue(target, value);
        }

        private static void CallPrivate(object target, string method)
        {
            var info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, method);
            info.Invoke(target, null);
        }

        private static EmotionReading DriverEmotion(TrafficV2VehicleDriver driver)
        {
            var info = typeof(TrafficV2VehicleDriver).GetMethod("CurrentEmotion", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null);
            return (EmotionReading)info.Invoke(driver, null);
        }

        private static int Bits(float value) { return BitConverter.SingleToInt32Bits(value); }

        private static void AssertSameBits(DriverProfile actual, DriverProfile expected)
        {
            Assert.That(new[] { Bits(actual.DesiredSpeed), Bits(actual.TimeHeadway), Bits(actual.MinimumGap), Bits(actual.MaxAcceleration),
                Bits(actual.ComfortableDeceleration), Bits(actual.Politeness), Bits(actual.LaneChangeThreshold), Bits(actual.SafeBrakingLimit),
                Bits(actual.ReactionTime), Bits(actual.LaneChangeEvaluationInterval), Bits(actual.Consistency), Bits(actual.AimPointRecallSpeed) },
                Is.EqualTo(new[] { Bits(expected.DesiredSpeed), Bits(expected.TimeHeadway), Bits(expected.MinimumGap), Bits(expected.MaxAcceleration),
                Bits(expected.ComfortableDeceleration), Bits(expected.Politeness), Bits(expected.LaneChangeThreshold), Bits(expected.SafeBrakingLimit),
                Bits(expected.ReactionTime), Bits(expected.LaneChangeEvaluationInterval), Bits(expected.Consistency), Bits(expected.AimPointRecallSpeed) }));
        }

        private static bool IsStraight(IReadOnlyList<RoadCurveSample> samples)
        {
            for (int i = 0; i < samples.Count; i++) if (Math.Abs(samples[i].CurvaturePerMeter) > 1e-4f) return false;
            return true;
        }

        private static string CodeOnly(string source)
        {
            return Regex.Replace(Regex.Replace(source, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\n]*", "");
        }

        /// <summary>
        /// Decision longitudinale reelle a l'arret sur la ligne droite de reference de MVP_Run (patron Story 5.33) : spine, plan
        /// et arbitrage construits avec le profil effectif, comme le pilote (TrafficV2VehicleDriver.Step).
        /// </summary>
        private static LongitudinalDecision DecideAtRest(DriverProfile effective)
        {
            var admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            var model = admission.Model;
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
            var result = RoutePlanner.Plan(new RouteRequest(model, start, RoadId.None, new RouteSeed(0), A, "route",
                new DecisionCounter(0), null, false, null, best.Id));
            Assert.That(result.Plan, Is.Not.Null, result.Reason.ToString());
            var track = ReferenceTrack.FromRoute(model, result.Plan.Occurrences, 0f);
            const float distance = 1f;
            int piece = track.PieceAt(distance);
            var nominal = track.Nominal(piece, distance);
            var pose = new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward, Up = nominal.Up, Footprint = Car };
            var actor = new TrafficActorInput(A, pose, 0f, track.Pieces[piece].Id, result.Plan.Occurrences.Select(o => o.Id).ToArray(),
                track.KinematicAnchors(piece));
            var frame = new TrafficFrame(1, model, new[] { actor }, null);
            var spine = PlanningSpine.Evaluate(new PlanningRequest(frame, A, result.Plan, result.Plan.ExitPortalId, new RouteSeed(0),
                TrafficV2Settings.LookAheadMeters, null, null, null, effective, TrackingTolerance.Undeclared, null, null,
                admission.Evidence, RoadId.None, track.OffsetRadians(piece, distance)));
            Assert.That(spine.Motion, Is.Not.Null, spine.Projection.Code);
            var plan = SpeedPlan.Build(spine.Motion, model, effective, 0f);
            Assert.That(plan.Accepted, Is.True, "le profil immobilisant est accepte : aucun repli ProfileRefused");
            return LongitudinalArbitration.Decide(plan, effective, 0f, Dt, new LongitudinalPerception(PerceptionUnavailableReason.None, null, null),
                LongitudinalMemory.None, TrafficV2Settings.StopHold);
        }
    }
}
