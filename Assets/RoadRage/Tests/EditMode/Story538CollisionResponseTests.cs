using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Collisions;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Features.Vehicles.Traffic.Safety;
using RoadRage.Features.Vehicles.Traffic.Tactical;
using UnityEditor;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.38 -- reponse aux collisions : faits et significativite (C2), poignee de main et propriete du but (C3), tirage
    /// deterministe des reactions (C4), commandes sans propulsion passees au vrai composeur (C5), stabilite (C6) et
    /// branchement du driver (C7), plus l'absence structurelle de toute ecriture du corps.
    /// </summary>
    [Category("Core")]
    [Category("Story538")]
    public sealed class Story538CollisionResponseTests
    {
        private const string VehicleProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string DriverPath = "Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs";
        private const float Dt = 0.02f;
        private const float Mass = 1200f;
        private const float Lock = 30f;
        private const float BMax = 8f;
        private const float BComfort = 2f;
        private const ulong Frame = 10UL;
        private static readonly RouteSeed Seed = new RouteSeed(538UL);
        private static readonly RoadId Self = new RoadId(0x538UL, 1UL);
        private static float Tol { get { return TrafficV2Settings.DeclaredTrackingTolerance.Meters; } }

        private static CollisionFacts Facts(bool contact = true, float relative = 0f, float deltaV = 0f, float contactSeconds = 0f,
            int side = 0, float yaw = 0f, float angular = 0f, int grounded = 4, float displacement = 0f, float mass = Mass)
        {
            return new CollisionFacts(1UL, contact, deltaV * Mass, relative, contactSeconds, side, yaw, angular, grounded, 4,
                displacement, mass);
        }

        private static CollisionResponseRequest Request(CollisionFacts facts, ulong version = 1UL, ulong frame = Frame,
            RoadId trafficId = default(RoadId))
        {
            return new CollisionResponseRequest(version, frame, trafficId.IsEmpty ? Self : trafficId,
                CollisionSignificance.RelativeSpeed, facts);
        }

        private static TacticalDecision Accepted(CollisionReactionWeights weights, int side = 1, float wheelAngle = 7f)
        {
            var tactical = new TacticalDecision();
            var response = tactical.Submit(Request(Facts(relative: 6f, side: side)), Frame, false, Seed, weights, wheelAngle);
            Assert.That(response.Reason, Is.EqualTo(TacticalReason.Accepted));
            return tactical;
        }

        private static CollisionReactionWeights Only(CollisionReaction reaction)
        {
            return new CollisionReactionWeights(reaction == CollisionReaction.Brake ? 1f : 0f,
                reaction == CollisionReaction.Evade ? 1f : 0f, reaction == CollisionReaction.MisReact ? 1f : 0f, 0.8f, 0.6f);
        }

        // ------------------------------------------------------------------ C2 significativite

        [Test]
        public void NoContactGrazeAndUndersideContactsAreNotSignificant()
        {
            var analysis = new CollisionAnalysis();
            Assert.That(analysis.Analyze(Facts(contact: false, relative: 9f, deltaV: 3f), Frame, Self, Tol), Is.Null, "aucun contact");
            Assert.That(analysis.Analyze(Facts(relative: 1f, deltaV: 0.1f, contactSeconds: 0.04f), Frame, Self, Tol), Is.Null, "frottement");
            Assert.That(CollisionPredicates.Significance(Facts(relative: 1f, contactSeconds: 2f), Tol), Is.EqualTo(CollisionSignificance.None),
                "contact long et stable dans l'enveloppe (file au contact)");
            Assert.That(CollisionPredicates.Significance(Facts(relative: float.NaN), Tol), Is.EqualTo(CollisionSignificance.None));

            // Contacts de dessous : exclus avant toute accumulation, par la regle meme de la 5.15.
            string driver = File.ReadAllText(DriverPath);
            string accumulate = Body(driver, "private void AccumulateStep(");
            Assert.That(accumulate.IndexOf("IsSurfaceOnly(collision)", StringComparison.Ordinal),
                Is.LessThan(accumulate.IndexOf("contactsOfStep.Add(", StringComparison.Ordinal)));
            StringAssert.Contains("VehicleSuspensionModel.IsSurfaceContact(", Body(driver, "private bool IsSurfaceOnly("));
            StringAssert.Contains("physicsBody.Profile.SurfaceContactTolerance", Body(driver, "private bool IsSurfaceOnly("));
        }

        [Test]
        public void EachSignificanceCauseFiresAloneAtItsThresholdAndDeterministically()
        {
            Assert.That(CollisionThresholds.SignificantRelativeSpeedMetersPerSecond, Is.EqualTo(NetworkedVehicleDriverController.MinCollisionDamageSpeed));
            Expect(Facts(relative: 4f), CollisionSignificance.RelativeSpeed);
            Expect(Facts(relative: 3.99f), CollisionSignificance.None);
            Expect(Facts(deltaV: 1f), CollisionSignificance.DeltaV);
            Expect(Facts(deltaV: 0.99f), CollisionSignificance.None);
            Expect(Facts(relative: 1f, contactSeconds: 0.25f, yaw: 0.6f), CollisionSignificance.SustainedPush);
            Expect(Facts(relative: 1f, contactSeconds: 0.25f, angular: 0.6f), CollisionSignificance.SustainedPush);
            Expect(Facts(relative: 1f, contactSeconds: 0.25f, grounded: 3), CollisionSignificance.SustainedPush);
            Expect(Facts(relative: 1f, contactSeconds: 0.25f, displacement: Tol + 0.01f), CollisionSignificance.SustainedPush);
            Expect(Facts(relative: 1f, contactSeconds: 0.24f, yaw: 0.6f), CollisionSignificance.None);
            Expect(Facts(relative: 1f, contactSeconds: 0.3f, yaw: 0.6f), CollisionSignificance.SustainedPush);
            Expect(Facts(relative: 6f, side: 1), CollisionSignificance.RelativeSpeed);

            var a = new CollisionAnalysis().Analyze(Facts(relative: 6f), Frame, Self, Tol);
            var b = new CollisionAnalysis().Analyze(Facts(relative: 6f), Frame, Self, Tol);
            Assert.That(a.Significance, Is.EqualTo(b.Significance));
            Assert.That(a.Version, Is.EqualTo(b.Version));
            var analysis = new CollisionAnalysis();
            ulong first = analysis.Analyze(Facts(relative: 6f), Frame, Self, Tol).Version;
            Assert.That(analysis.Analyze(Facts(relative: 6f), Frame + 1UL, Self, Tol).Version, Is.GreaterThan(first), "version croissante");
        }

        private static void Expect(CollisionFacts facts, CollisionSignificance expected)
        {
            Assert.That(CollisionPredicates.Significance(facts, Tol), Is.EqualTo(expected));
            Assert.That(CollisionPredicates.Significance(facts, Tol), Is.EqualTo(expected), "deterministe");
        }

        // ------------------------------------------------------------------ C3 poignee de main

        [Test]
        public void InvalidFactsReachTheTacticalBoundaryThroughAnalysis()
        {
            var analysis = new CollisionAnalysis();
            var tactical = new TacticalDecision();
            foreach (var facts in new[] { Facts(relative: float.NaN), Facts(contact: false, yaw: float.PositiveInfinity),
                Facts(mass: float.NaN) })
            {
                var request = analysis.Analyze(facts, Frame, Self, Tol);
                Assert.That(request, Is.Not.Null, "faits invalides : requete de refus, pas absence de collision");
                Assert.That(tactical.Submit(request, Frame, false, Seed, CollisionReactionWeights.Default, 0f).Reason,
                    Is.EqualTo(TacticalReason.InvalidRequest));
                Assert.That(tactical.Active, Is.False);
            }
            Assert.That(analysis.Analyze(Facts(contact: false), Frame, Self, Tol), Is.Null, "faits valides sans contact");
            var valid = analysis.Analyze(Facts(relative: 6f), Frame, Self, Tol);
            Assert.That(valid.Version, Is.EqualTo(4UL), "les refus aussi portent une version");
            Assert.That(tactical.Submit(valid, Frame, false, Seed, CollisionReactionWeights.Default, 0f).Accepted, Is.True);
        }

        [Test]
        public void UpdateBeforeCommandCreditsOnlyPreviouslyAppliedBraking()
        {
            foreach (var reaction in new[] { CollisionReaction.Evade, CollisionReaction.MisReact })
            {
                var tactical = Accepted(Only(reaction));
                int brakingSteps = 0;
                for (int step = 0; step < 200 && tactical.Active; step++)
                {
                    // Ordre du driver : faits du pas ecoule, Update, puis commande du prochain pas.
                    tactical.Update(Facts(contact: false), Dt, Tol, false);
                    if (!tactical.Active) break;
                    var command = tactical.CommandFor(Frame, 1, BMax, BComfort, Lock);
                    if (tactical.Phase == CollisionGoalPhase.Braking)
                    {
                        Assert.That(command.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BMax));
                        brakingSteps++;
                    }
                }
                Assert.That(tactical.LastReason, Is.EqualTo(TacticalReason.Resumed), reaction.ToString());
                Assert.That(brakingSteps * Dt, Is.GreaterThanOrEqualTo(CollisionThresholds.StableSeconds),
                    "au moins 0,5 s de freinage REEL apres " + reaction);
            }
        }

        [Test]
        public void TacticalAcceptsOrRejectsEachRequestWithItsReason()
        {
            var weights = CollisionReactionWeights.Default;
            Assert.That(new TacticalDecision().Submit(Request(Facts(relative: float.NaN)), Frame, false, Seed, weights, 0f).Reason,
                Is.EqualTo(TacticalReason.InvalidRequest), "faits non finis");
            Assert.That(new TacticalDecision().Submit(new CollisionResponseRequest(1UL, Frame, RoadId.None, CollisionSignificance.RelativeSpeed,
                Facts(relative: 6f)), Frame, false, Seed, weights, 0f).Reason, Is.EqualTo(TacticalReason.InvalidRequest), "identite vide");
            Assert.That(new TacticalDecision().Submit(Request(Facts(relative: 6f), frame: Frame - 1UL), Frame, false, Seed, weights, 0f).Reason,
                Is.EqualTo(TacticalReason.StaleRequest));
            var latched = new TacticalDecision();
            Assert.That(latched.Submit(Request(Facts(relative: 6f)), Frame, true, Seed, weights, 0f).Reason, Is.EqualTo(TacticalReason.FallbackLatched));
            Assert.That(latched.Active, Is.False, "repli verrouille inchange, aucun but");

            var tactical = Accepted(weights);
            Assert.That(tactical.Active, Is.True);
            Assert.That(tactical.Goal, Is.EqualTo(TacticalGoalKind.CollisionResponse));
            var reaction = tactical.Reaction;
            var second = tactical.Submit(Request(Facts(relative: 9f, side: -1), version: 2UL), Frame, false, Seed, weights, 0f);
            Assert.That(second.Reason, Is.EqualTo(TacticalReason.GoalAlreadyActive));
            Assert.That(tactical.GoalVersion, Is.EqualTo(1UL), "but conserve");
            Assert.That(tactical.Reaction, Is.EqualTo(reaction));
            Assert.That(tactical.Submit(Request(Facts(relative: 9f), version: 2UL), Frame, false, Seed, weights, 0f).Reason,
                Is.EqualTo(TacticalReason.InvalidRequest), "version non croissante");
        }

        [Test]
        public void OnlyTacticalOwnsTheGoalAndEndsItByResumptionOrCancellation()
        {
            foreach (var property in typeof(TacticalDecision).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (property.SetMethod != null) Assert.That(property.SetMethod.IsPublic, Is.False, property.Name);
            foreach (var file in Directory.GetFiles("Assets/RoadRage/Features/Vehicles/Traffic/Collision", "*.cs"))
                StringAssert.DoesNotContain("Tactical", File.ReadAllText(file), "l'analyse ne pose jamais le but");

            var tactical = Accepted(Only(CollisionReaction.Brake));
            int steps = 0;
            while (tactical.Active && steps < 100) { tactical.Update(Facts(contact: false), Dt, Tol, false); steps++; }
            Assert.That(tactical.LastReason, Is.EqualTo(TacticalReason.Resumed));
            Assert.That(steps, Is.InRange(25, 26), "0,5 s de stabilite (somme flottante des pas)");

            var cancelled = Accepted(Only(CollisionReaction.Brake));
            cancelled.Update(Facts(relative: 6f), Dt, Tol, true);
            Assert.That(cancelled.Active, Is.False);
            Assert.That(cancelled.LastReason, Is.EqualTo(TacticalReason.ExitPortalReached));
            Assert.That(cancelled.Goal, Is.EqualTo(TacticalGoalKind.Nominal));
        }

        // ------------------------------------------------------------------ C6 stabilite

        [Test]
        public void EachInstabilityBlocksResumptionAndADisplacedVehicleAwaitsRecovery()
        {
            var blockers = new[]
            {
                new { Facts = Facts(relative: 6f), Blocker = StabilityBlocker.SignificantContact },
                new { Facts = Facts(contact: false, yaw: 0.31f), Blocker = StabilityBlocker.YawRate },
                new { Facts = Facts(contact: false, angular: 0.51f), Blocker = StabilityBlocker.AngularSpeed },
                new { Facts = Facts(contact: false, grounded: 3), Blocker = StabilityBlocker.WheelsAirborne },
                new { Facts = Facts(contact: false, yaw: float.NaN), Blocker = StabilityBlocker.NonFiniteFacts },
            };
            foreach (var row in blockers)
            {
                var tactical = Accepted(Only(CollisionReaction.Brake));
                for (int i = 0; i < 200; i++) tactical.Update(row.Facts, Dt, Tol, false);
                Assert.That(tactical.Active, Is.True, row.Blocker.ToString());
                Assert.That(tactical.LastBlocker, Is.EqualTo(row.Blocker));
                Assert.That(tactical.Phase, Is.EqualTo(CollisionGoalPhase.Braking));
            }

            // Une seule instabilite remet le compte de stabilite a zero.
            var interrupted = Accepted(Only(CollisionReaction.Brake));
            for (int i = 0; i < 20; i++) interrupted.Update(Facts(contact: false), Dt, Tol, false);
            interrupted.Update(Facts(contact: false, yaw: 0.5f), Dt, Tol, false);
            for (int i = 0; i < 20; i++) interrupted.Update(Facts(contact: false), Dt, Tol, false);
            Assert.That(interrupted.Active, Is.True);

            var displaced = Accepted(Only(CollisionReaction.Brake));
            for (int i = 0; i < 200; i++) displaced.Update(Facts(contact: false, displacement: 1f), Dt, Tol, false);
            Assert.That(displaced.Active, Is.True, "stable mais deplace : jamais Resumed");
            Assert.That(displaced.Phase, Is.EqualTo(CollisionGoalPhase.AwaitingRecovery));
            var held = displaced.CommandFor(Frame, 1, BMax, BComfort, Lock);
            Assert.That(held.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BMax), "frein maintenu");
            displaced.Update(Facts(contact: false, displacement: Tol), Dt, Tol, false);
            Assert.That(displaced.LastReason, Is.EqualTo(TacticalReason.Resumed), "revenu dans l'enveloppe");
        }

        // ------------------------------------------------------------------ C4 tirage

        [Test]
        public void ComparableCollisionsDrawEveryAuthoredReactionDeterministically()
        {
            var weights = CollisionReactionWeights.Default;
            const int draws = 10000;
            var counts = new int[3];
            var first = new CollisionReaction[draws];
            for (int i = 0; i < draws; i++)
            {
                var tactical = new TacticalDecision();
                tactical.Submit(Request(Facts(relative: 6f, side: 1), trafficId: new RoadId(0x538UL, (ulong)i + 10UL)), Frame, false,
                    Seed, weights, 0f);
                first[i] = tactical.Reaction;
                counts[(int)tactical.Reaction]++;
            }
            Assert.That(counts.All(c => c > 0), Is.True, "trois reactions possibles");
            Assert.That(counts[0] / (double)draws, Is.EqualTo(weights.Brake).Within(0.03), "Brake courant");
            Assert.That(counts[1] / (double)draws, Is.EqualTo(weights.Evade).Within(0.03), "Evade moins frequent");
            Assert.That(counts[2] / (double)draws, Is.EqualTo(weights.MisReact).Within(0.03), "MisReact possible");

            int differ = 0;
            for (int i = 0; i < draws; i++)
            {
                var same = new TacticalDecision();
                same.Submit(Request(Facts(relative: 6f, side: 1), trafficId: new RoadId(0x538UL, (ulong)i + 10UL)), Frame, false, Seed, weights, 0f);
                Assert.That(same.Reaction, Is.EqualTo(first[i]), "meme graine, meme reaction");
                var other = new TacticalDecision();
                other.Submit(Request(Facts(relative: 6f, side: 1), trafficId: new RoadId(0x538UL, (ulong)i + 10UL)), Frame, false,
                    new RouteSeed(539UL), weights, 0f);
                if (other.Reaction != first[i]) differ++;
            }
            Assert.That(differ, Is.GreaterThan(0), "la graine de session decide du tirage");

            var noEvade = new CollisionReactionWeights(0.5f, 0f, 0.5f, 0.8f, 0.6f);
            for (int i = 1; i <= 2000; i++) Assert.That(noEvade.Select(i / 2001d), Is.Not.EqualTo(CollisionReaction.Evade));
            Assert.That(new CollisionReactionWeights(0f, 0f, 0f, 0.8f, 0.6f).Select(0.9d), Is.EqualTo(CollisionReaction.Brake), "invalide : Brake");
            Assert.That(new CollisionReactionWeights(float.NaN, 1f, 1f, 0.8f, 0.6f).Select(0.9d), Is.EqualTo(CollisionReaction.Brake));
            string error;
            Assert.That(new CollisionReactionWeights(-1f, 1f, 1f, 0.8f, 0.6f).TryValidate(out error), Is.False);
            Assert.That(new CollisionReactionWeights(1f, 1f, 1f, -0.1f, 0.6f).TryValidate(out error), Is.False);

            var authored = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath);
            Assert.That(authored.TryValidate(out error), Is.True, error);
            Assert.That(authored.CollisionReaction.Brake, Is.EqualTo(0.75f));
            Assert.That(authored.CollisionReaction.Evade, Is.EqualTo(0.15f));
            Assert.That(authored.CollisionReaction.MisReact, Is.EqualTo(0.10f));
            Assert.That(authored.CollisionReaction.EvadeSeconds, Is.EqualTo(0.8f));
            Assert.That(authored.CollisionReaction.MisReactSeconds, Is.EqualTo(0.6f));
        }

        // ------------------------------------------------------------------ C5 commandes

        [Test]
        public void NoReactionOrPhaseEverCommandsPropulsionThroughTheRealComposer()
        {
            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath).Profile;
            float safeBraking = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile.SafeBrakingLimit;
            var composer = new VehicleDriveIntentComposer(vehicle, safeBraking, Dt);
            float band = composer.ServiceBandMetersPerSecond;
            var limits = SafetyLimits.For(vehicle, Story537Model.DrivabilityProfile, Dt, TrafficV2Settings.PlanValiditySteps);
            float bMax = limits.MaxBrakingDecelerationMetersPerSecondSquared;
            float lockDegrees = Story537Model.DrivabilityProfile.LowSpeedLockDegrees;
            foreach (CollisionReaction reaction in Enum.GetValues(typeof(CollisionReaction)))
                foreach (bool reacting in new[] { true, false })
                {
                    var tactical = Accepted(Only(reaction), side: 1, wheelAngle: 7f);
                    if (!reacting) for (int i = 0; i < 60; i++) tactical.Update(Facts(relative: 6f), Dt, Tol, false);
                    var command = tactical.CommandFor(Frame, 1, bMax, BComfort, lockDegrees);
                    Assert.That(command.TargetAccelerationMetersPerSecondSquared, Is.LessThanOrEqualTo(0f));
                    Assert.That(command.IsValidAt(Frame), Is.True);
                    Assert.That(command.ReferenceCurvaturePerMeter, Is.EqualTo(0f), "aucune route suivie");
                    foreach (float speed in new[] { 0f, 0.04f, 0.2f, band, band + 0.01f, 10f })
                        foreach (float damping in new[] { 0f, 0.3f, 3f })
                        {
                            var composed = composer.Compose(Frame, command, V2FallbackReason.None, speed, damping, propulsion: false);
                            string label = reaction + (reacting ? "/reaction" : "/freinage") + " " + speed + " m/s, damping " + damping;
                            Assert.That(composed.Fallback, Is.False, label);
                            Assert.That(composed.Intent.Throttle, Is.EqualTo(0f), label);
                            Assert.That(VehicleDriveIntentComposer.MinimumWheelDriveTorque(vehicle, composed.Intent, composed.MaxForwardSpeed, speed),
                                Is.GreaterThanOrEqualTo(0f), "aucune marche arriere : " + label);
                            bool braking = command.TargetAccelerationMetersPerSecondSquared < 0f;
                            if (braking && speed > band) Assert.That(composed.Intent.BrakeReverse, Is.GreaterThan(0f), label);
                            if (braking && speed <= band) Assert.That(composed.Intent.Handbrake, Is.EqualTo(1f), label);
                            if (!braking) Assert.That(composed.Intent.BrakeReverse + composed.Intent.Handbrake, Is.EqualTo(0f), "roue libre : " + label);
                        }
                    if (!reacting || reaction == CollisionReaction.Brake)
                    {
                        Assert.That(command.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-bMax));
                        Assert.That(command.TargetWheelAngleDegrees, Is.EqualTo(0f), "roues droites");
                        Assert.That(composer.Compose(Frame, command, V2FallbackReason.None, 10f, 0f, propulsion: false).Intent.BrakeReverse,
                            Is.EqualTo(1f).Within(1e-4f), "frein plein a b_max");
                    }
                    else if (reaction == CollisionReaction.MisReact)
                        Assert.That(command.TargetWheelAngleDegrees, Is.EqualTo(7f), "volant fige au choc");
                    // La commande passe par le SafetyFilter comme toute commande (chemin nul : vehicule hors route admis).
                    var verdict = SafetyFilter.Evaluate(command, Frame, Story537Frame(), Self, 10f, null, null, limits);
                    Assert.That(verdict.Verdict, Is.EqualTo(SafetyVerdict.Pass), reaction + " " + verdict.Reason);
                }

            // Nominal (AC8) : propulsion par defaut, traduction 5.37 inchangee au bit pres.
            var nominal = new MotionCommand(Frame, Frame, Frame, 1.2f, 4f);
            var before = new VehicleDriveIntentComposer(vehicle, safeBraking, Dt).Compose(Frame, nominal, V2FallbackReason.None, 5f, 0.3f);
            var after = new VehicleDriveIntentComposer(vehicle, safeBraking, Dt).Compose(Frame, nominal, V2FallbackReason.None, 5f, 0.3f, false, true);
            Assert.That(after.Intent.Throttle, Is.EqualTo(before.Intent.Throttle));
            Assert.That(after.Intent.Throttle, Is.GreaterThan(0f));
            var coast = new MotionCommand(Frame, Frame, Frame, -3f, 0f);
            Assert.That(composer.Compose(Frame, coast, V2FallbackReason.None, 0.2f, 0f).Intent.Handbrake, Is.EqualTo(0f),
                "la commande normale garde sa roue libre dans la bande");
        }

        [Test]
        public void EvasionSteersAwayFromTheImpactSideThenBrakes()
        {
            var right = Accepted(Only(CollisionReaction.Evade), side: 1);
            Assert.That(right.Phase, Is.EqualTo(CollisionGoalPhase.Reacting));
            var away = right.CommandFor(Frame, 1, BMax, BComfort, Lock);
            Assert.That(away.TargetWheelAngleDegrees, Is.EqualTo(-TacticalDecision.EvadeLockFraction * Lock), "impact a droite : braque a gauche");
            Assert.That(away.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BComfort));
            var left = Accepted(Only(CollisionReaction.Evade), side: -1);
            Assert.That(left.CommandFor(Frame, 1, BMax, BComfort, Lock).TargetWheelAngleDegrees, Is.EqualTo(TacticalDecision.EvadeLockFraction * Lock));

            int steps = 0;
            while (right.Phase == CollisionGoalPhase.Reacting && steps < 100) { right.Update(Facts(relative: 6f), Dt, Tol, false); steps++; }
            Assert.That(steps * Dt, Is.EqualTo(CollisionReactionWeights.Default.EvadeSeconds).Within(Dt + 1e-4f));
            Assert.That(right.CommandFor(Frame, 1, BMax, BComfort, Lock).TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BMax),
                "apres l'esquive, freinage");

            var misReact = Accepted(Only(CollisionReaction.MisReact), wheelAngle: 7f);
            var drift = misReact.CommandFor(Frame, 1, BMax, BComfort, Lock);
            Assert.That(drift.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(0f), "ne freine pas assez");
            for (int i = 0; i < 40; i++) misReact.Update(Facts(relative: 6f), Dt, Tol, false);
            Assert.That(misReact.CommandFor(Frame, 1, BMax, BComfort, Lock).TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BMax));
        }

        // ------------------------------------------------------------------ C1 accumulation et D3

        [Test]
        public void TheStepAccumulatorPublishesSideStreakAndResetsAfterEachStep()
        {
            var accumulator = new StepContactAccumulator();
            accumulator.Add(500f, 2f, 0.9f);
            accumulator.Add(800f, 6f, -0.8f);
            var hit = accumulator.Consume(1UL, Dt, 0f, 0f, 4, 4, 0f, Mass);
            Assert.That(hit.InContact, Is.True);
            Assert.That(hit.ImpulseNewtonSeconds, Is.EqualTo(800f));
            Assert.That(hit.RelativeNormalSpeedMetersPerSecond, Is.EqualTo(6f));
            Assert.That(hit.ImpactSide, Is.EqualTo(-1), "cote de la paire la plus rapide : gauche");
            Assert.That(hit.ContactSeconds, Is.EqualTo(Dt));

            accumulator.Add(10f, 0.5f, 0.6f);
            var held = accumulator.Consume(2UL, Dt, 0f, 0f, 4, 4, 0f, Mass);
            Assert.That(held.ContactSeconds, Is.EqualTo(2f * Dt).Within(1e-6f), "contact continu");
            Assert.That(held.ImpulseNewtonSeconds, Is.EqualTo(10f), "valeurs du pas remises a zero");
            Assert.That(held.ImpactSide, Is.EqualTo(1));

            var free = accumulator.Consume(3UL, Dt, 0f, 0f, 4, 4, 0f, Mass);
            Assert.That(free.InContact, Is.False);
            Assert.That(free.ContactSeconds, Is.EqualTo(0f));
            Assert.That(free.RelativeNormalSpeedMetersPerSecond, Is.EqualTo(0f));
            accumulator.Add(10f, 0.5f, 0.1f);
            var frontal = accumulator.Consume(4UL, Dt, 0f, 0f, 4, 4, 0f, Mass);
            Assert.That(frontal.ContactSeconds, Is.EqualTo(Dt), "nouveau contact apres un pas libre");
            Assert.That(frontal.ImpactSide, Is.EqualTo(0), "choc frontal : zone morte, cote inconnu");

            accumulator.Add(900f, 9f, 1f);
            accumulator.Reset();
            Assert.That(accumulator.Consume(5UL, Dt, 0f, 0f, 4, 4, 0f, Mass).InContact, Is.False, "vehicule inerte : contacts oublies");
        }

        [Test]
        public void TheToleranceLatchWaitsWhileAContactLastsOrAGoalIsActive()
        {
            var slowPush = Facts(relative: 1f, contactSeconds: 0.1f, displacement: Tol + 0.2f);
            Assert.That(CollisionPredicates.Significance(slowPush, Tol), Is.EqualTo(CollisionSignificance.None), "pas encore significative");
            Assert.That(CollisionPredicates.SuspendsToleranceLatch(false, slowPush), Is.True, "le verrou ne devance pas la poussee");
            Assert.That(CollisionPredicates.SuspendsToleranceLatch(true, Facts(contact: false, displacement: 1f)), Is.True, "but actif");
            Assert.That(CollisionPredicates.SuspendsToleranceLatch(false, Facts(contact: false, displacement: 1f)), Is.False,
                "sans contact ni but : verrou 2a inchange");
        }

        [Test]
        public void AReactionIsAlwaysFollowedByBrakingAndAFrontalEvasionBrakesAtOnce()
        {
            foreach (var reaction in new[] { CollisionReaction.Evade, CollisionReaction.MisReact })
            {
                var tactical = Accepted(Only(reaction), side: 1);
                int reactingSteps = 0, brakingSteps = 0;
                while (tactical.Active && reactingSteps + brakingSteps < 200)
                {
                    var command = tactical.CommandFor(Frame, 1, BMax, BComfort, Lock);
                    if (tactical.Phase == CollisionGoalPhase.Reacting) reactingSteps++;
                    else { brakingSteps++; Assert.That(command.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BMax)); }
                    tactical.Update(Facts(contact: false), Dt, Tol, false);
                }
                Assert.That(tactical.LastReason, Is.EqualTo(TacticalReason.Resumed), reaction.ToString());
                Assert.That(reactingSteps, Is.GreaterThan(0), reaction.ToString());
                Assert.That(brakingSteps * Dt, Is.GreaterThanOrEqualTo(CollisionThresholds.StableSeconds - Dt), "0,5 s de freinage apres " + reaction);
            }
            var frontal = Accepted(Only(CollisionReaction.Evade), side: 0);
            Assert.That(frontal.Phase, Is.EqualTo(CollisionGoalPhase.Braking), "esquive sans cote : freinage direct");
            Assert.That(frontal.CommandFor(Frame, 1, BMax, BComfort, Lock).TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BMax));

            var kept = Accepted(Only(CollisionReaction.Brake));
            kept.Submit(Request(Facts(relative: 9f), version: 2UL), Frame, false, Seed, CollisionReactionWeights.Default, 0f);
            Assert.That(kept.LastReason, Is.EqualTo(TacticalReason.Accepted), "un second choc ne masque pas la raison du but");
        }

        // ------------------------------------------------------------------ structure et branchement

        [Test]
        public void CollisionAndTacticalNeverWriteTheBodyNorRecoverNorReadPolicy()
        {
            var forbidden = new Regex(@"(\b(MovePosition|MoveRotation|AddForce|AddTorque|RecoverAtWaypoint|Teleport\w*|ApplyDriveIntent|"
                + @"ApplyMovement|Rigidbody|Despawn|Replan\w*|Rage|Fear|Target(ing)?|JunctionCoordinator|Coroutine|WaitForSeconds|"
                + @"Time\.(time|deltaTime|realtimeSinceStartup)|Random)\b|\.(position|rotation|linearVelocity|velocity|angularVelocity)\s*=)");
            foreach (var folder in new[] { "Collision", "Tactical" })
            {
                var files = Directory.GetFiles("Assets/RoadRage/Features/Vehicles/Traffic/" + folder, "*.cs");
                Assert.That(files, Is.Not.Empty, folder);
                foreach (var file in files)
                {
                    var match = forbidden.Match(File.ReadAllText(file));
                    Assert.That(match.Success, Is.False, file + " : " + match.Value);
                }
            }
            foreach (var file in Directory.GetFiles("Assets/RoadRage/Features/Vehicles/Traffic/Safety", "*.cs"))
                Assert.That(Regex.IsMatch(File.ReadAllText(file), @"\b(Collision\w*|Tactical\w*)\b"), Is.False, "aucune logique de collision dans Safety : " + file);
        }

        [Test]
        [Category("Story539")]
        public void TheDriverRunsAnalysisSubmitUpdateCommandSafetyThenCompose()
        {
            string step = Body(File.ReadAllText(DriverPath), "internal void Step(");
            string[] order = { "collisionAnalysis.Analyze(", "tactical.Submit(", "tactical.Update(", "tactical.CommandFor(",
                "MotionCommand.Track(", "SafetyFilter.Evaluate(", "composer.Compose(" };
            int last = -1;
            foreach (var call in order)
            {
                int index = step.IndexOf(call, StringComparison.Ordinal);
                Assert.That(index, Is.GreaterThan(last), call);
                Assert.That(step.IndexOf(call, index + 1, StringComparison.Ordinal), Is.EqualTo(-1), "un seul appel : " + call);
                last = index;
            }
            // R6 (5.39, accord proprietaire du 2026-10-07) : arbitrage saute et demande de carrefour invalide pour tout but.
            StringAssert.Contains("if (!tacticalGoal) longitudinal = LongitudinalArbitration.Decide(", step);
            StringAssert.Contains("propulsion: !collisionGoal", step);
            StringAssert.Contains("WithFallback(composed.Fallback || tacticalGoal)", step);
            StringAssert.Contains("decision != null ? decision.Path : null", step);
            string prepare = Body(File.ReadAllText(DriverPath), "private bool PrepareStep(");
            StringAssert.Contains("ObserveTrackingTolerance(step, displacement, !collisionHoldsLatch)", prepare);
            StringAssert.Contains("CollisionPredicates.SuspendsToleranceLatch(tactical.Active, preparedFacts)", prepare);
            StringAssert.Contains("contactsOfStep.Consume(", prepare);
            Assert.That(Regex.Matches(prepare, @"contactsOfStep\.Reset\(\)").Count, Is.EqualTo(2), "vehicule inerte : contacts oublies");
            StringAssert.Contains("latchAllowed && toleranceResponse.Observe(", Body(File.ReadAllText(DriverPath),
                "private float ObserveTrackingTolerance(ulong step, float displacement, bool latchAllowed)"));
        }

        [Test]
        public void TheProjectionRendersTheTacticalGoalInEveryCultureAndOnlyWhenPresent()
        {
            var tactical = Accepted(Only(CollisionReaction.Evade));
            var projection = PlanningSpine.Evaluate(new PlanningRequest(Story537Frame(), Self, null, new RoadId(0x538UL, 2UL),
                new RouteSeed(1), 10f, null, null, null, default(DriverProfile), bounds: new LongitudinalBounds(2f, 3f))).Projection;
            var withTactical = projection.WithDrive(new TrafficDriveOutcome(Frame, Frame, Frame, 0f, 0f, 0.25f, 0f, false, null, "None",
                null, null, VehicleCoverage.NotEstablished, null, null, tactical.ToText()));
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                string invariant = withTactical.ToText();
                StringAssert.Contains("Tactical CollisionResponse Accepted Evade/Reacting v1 accepte 10", invariant);
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                Assert.That(withTactical.ToText(), Is.EqualTo(invariant));
            }
            finally { CultureInfo.CurrentCulture = original; }
            StringAssert.DoesNotContain("\nTactical ", projection.WithDrive(new TrafficDriveOutcome(Frame, Frame, Frame, 0f, 0f, 0f, 0f, false,
                null, "None", null, null, VehicleCoverage.NotEstablished, null)).ToText());
        }

        // ------------------------------------------------------------------ outils

        private static CompiledRoadModel model;
        private static RoadId corridorId;

        private static CompiledRoadModel Story537Model
        {
            get
            {
                if (model != null) return model;
                model = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath)));
                corridorId = model.Portals.First(p => p.Role == PortalRole.Entry).CorridorId;
                return model;
            }
        }

        private static TrafficFrame Story537Frame()
        {
            var m = Story537Model;
            EffectiveLaneCorridor corridor;
            m.TryGetCorridor(corridorId, out corridor);
            var point = corridor.Curve.Sample(1f);
            var pose = new VehicleFootprintPose { Position = point.Position, Forward = point.Tangent, Up = point.Up };
            return new TrafficFrame(Frame, m, new[] { new TrafficActorInput(Self, pose, 10f, corridorId) }, null, null, null);
        }

        /// <summary>Corps d'un membre : de sa signature a l'accolade fermante appariee.</summary>
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
