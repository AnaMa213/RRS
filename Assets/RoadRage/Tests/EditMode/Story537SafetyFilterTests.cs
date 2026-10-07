using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Features.Vehicles.Traffic.Safety;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.37 -- SafetyFilter : une raison par cause (S2), priorite, collision imminente (S3), contact autorise porte par
    /// le plan (S4), epoques et echantillon recent (S5), absence structurelle de logique tactique et branchement du driver.
    /// Frame sur le modele committe de MVP_Run, sans admission.
    /// </summary>
    [Category("Core")]
    [Category("Story537")]
    public sealed class Story537SafetyFilterTests
    {
        private const string VehicleProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const float Dt = 0.02f;
        private const float BMax = 8f;
        private const ulong Frame = 10UL;
        private static readonly SafetyLimits Limits = new SafetyLimits(BMax, 6f, 35f, 0.2f, 0.02f);
        private static readonly RoadId Self = new RoadId(0x537UL, 1UL);
        private static readonly RoadId Other = new RoadId(0x537UL, 2UL);
        private static readonly RoadId Target = new RoadId(0x537UL, 3UL);

        private static CompiledRoadModel model;
        private static RoadId corridorId;

        private static CompiledRoadModel Model
        {
            get
            {
                if (model != null) return model;
                model = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath)));
                corridorId = model.Portals.First(p => p.Role == PortalRole.Entry).CorridorId;
                return model;
            }
        }

        private static TrafficFrame FrameWith(bool closed = false)
        {
            var m = Model;
            EffectiveLaneCorridor corridor;
            m.TryGetCorridor(corridorId, out corridor);
            var point = corridor.Curve.Sample(1f);
            var pose = new VehicleFootprintPose { Position = point.Position, Forward = point.Tangent, Up = point.Up };
            return new TrafficFrame(Frame, m, new[] { new TrafficActorInput(Self, pose, 10f, corridorId) }, null, null,
                closed ? new[] { new ElementClosureInput(corridorId, "Story537") } : null);
        }

        private static MotionCommand Command(float acceleration = 1f, float angle = 5f, float curvature = 0.05f,
            ulong from = Frame, ulong to = Frame, ulong source = Frame)
        {
            return new MotionCommand(source, from, to, acceleration, angle, SpeedConstraint.None, 0.1f, 1f, curvature, 3f);
        }

        private static LongitudinalPerception Leader(RoadId id, float gap, float speed)
        {
            return new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(id, gap, speed), null);
        }

        private static LongitudinalPerception Obstacle(RoadId id, PerceivedObstacleKind kind, float gap, float speed)
        {
            return new LongitudinalPerception(PerceptionUnavailableReason.None, null,
                new[] { new LongitudinalObstacle(id, kind, gap, speed) });
        }

        private static SafetyResult Evaluate(MotionCommand command, LongitudinalPerception near = null, float speed = 10f,
            TrafficFrame frame = null, ulong step = Frame, AuthorizedContact? authorization = null, NearFieldSample? newer = null,
            RoadId? actor = null)
        {
            return SafetyFilter.Evaluate(command, step, frame ?? FrameWith(), actor ?? Self, speed, new LocalPath(corridorId), near,
                Limits, authorization, newer);
        }

        private static void Expect(SafetyResult result, SafetyVerdict verdict, SafetyReason reason)
        {
            Assert.That(result.Verdict, Is.EqualTo(verdict), result.ToText());
            Assert.That(result.Reason, Is.EqualTo(reason), result.ToText());
            Assert.That(result.Command.HasValue, Is.EqualTo(verdict != SafetyVerdict.Reject));
        }

        // ------------------------------------------------------------------ une raison par cause (S2)

        [Test]
        public void FreeRoadPassesTheCommandUnchanged()
        {
            var command = Command();
            var result = Evaluate(command, Leader(Other, 50f, 10f));
            Expect(result, SafetyVerdict.Pass, SafetyReason.None);
            Assert.That(result.Command.Value, Is.EqualTo(command));
        }

        [Test]
        public void InvalidActorStateRejectsForAnAbsentActorANullFrameOrANonFiniteSpeed()
        {
            Expect(Evaluate(Command(), actor: Other), SafetyVerdict.Reject, SafetyReason.InvalidActorState);
            Expect(SafetyFilter.Evaluate(Command(), Frame, null, Self, 10f, null, null, Limits), SafetyVerdict.Reject,
                SafetyReason.InvalidActorState);
            Expect(Evaluate(Command(), speed: float.NaN), SafetyVerdict.Reject, SafetyReason.InvalidActorState);
        }

        [Test]
        public void NonFiniteOutputRejectsANonFiniteAccelerationAngleOrCurvature()
        {
            Expect(Evaluate(Command(acceleration: float.NaN)), SafetyVerdict.Reject, SafetyReason.NonFiniteOutput);
            Expect(Evaluate(Command(angle: float.PositiveInfinity)), SafetyVerdict.Reject, SafetyReason.NonFiniteOutput);
            Expect(Evaluate(Command(curvature: float.NaN)), SafetyVerdict.Reject, SafetyReason.NonFiniteOutput);
        }

        [Test]
        public void StalePlanRejectsAStepOutsideTheWindowOrASourceNewerThanTheFrame()
        {
            Expect(Evaluate(Command(), step: Frame + 1), SafetyVerdict.Reject, SafetyReason.StalePlan);
            Expect(Evaluate(Command(from: Frame + 1, to: Frame + 2, source: Frame + 1), step: Frame + 1),
                SafetyVerdict.Reject, SafetyReason.StalePlan);
            // Plan tenu dans sa fenetre depuis une frame anterieure : pas perime.
            Expect(Evaluate(Command(from: Frame - 1, to: Frame + 1, source: Frame - 1)), SafetyVerdict.Pass, SafetyReason.None);
        }

        [Test]
        public void LocalPlanInvalidatedRejectsAPathThroughAClosedElement()
        {
            Expect(Evaluate(Command(), frame: FrameWith(closed: true)), SafetyVerdict.Reject, SafetyReason.LocalPlanInvalidated);
        }

        [Test]
        public void PhysicallyInvalidPathRejectsACurvatureBeyondAdmission()
        {
            Expect(Evaluate(Command(curvature: 0.2f * 1.0009f)), SafetyVerdict.Pass, SafetyReason.None);
            Expect(Evaluate(Command(curvature: -0.21f)), SafetyVerdict.Reject, SafetyReason.PhysicallyInvalidPath);
        }

        [Test]
        public void PhysicallyInvalidIntentClampsAngleAndAccelerationToThePhysicalBounds()
        {
            var angle = Evaluate(Command(angle: 40f));
            Expect(angle, SafetyVerdict.Clamp, SafetyReason.PhysicallyInvalidIntent);
            Assert.That(angle.Command.Value.TargetWheelAngleDegrees, Is.EqualTo(35f));
            Assert.That(Evaluate(Command(acceleration: 20f)).Command.Value.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(6f));
            var brake = Evaluate(Command(acceleration: -20f));
            Expect(brake, SafetyVerdict.Clamp, SafetyReason.PhysicallyInvalidIntent);
            Assert.That(brake.Command.Value.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BMax));
        }

        [Test]
        public void ImminentUnintendedCollisionStopsAtMaximumBrakingOnlyInsideTheStoppingEnvelope()
        {
            // c = 10, b_max = 8, tau = 0,02 : seuil g = 0,2 + 6,25 = 6,45 m.
            var stop = Evaluate(Command(), Leader(Other, 5f, 0f));
            Expect(stop, SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
            Assert.That(stop.Command.Value.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-BMax));
            Assert.That(stop.Command.Value.TargetWheelAngleDegrees, Is.EqualTo(5f));
            Assert.That(stop.HazardId, Is.EqualTo(Other));
            Expect(Evaluate(Command(), Leader(Other, 6.5f, 0f)), SafetyVerdict.Pass, SafetyReason.None);
            Expect(Evaluate(Command(), Obstacle(Other, PerceivedObstacleKind.Obstacle, 6.4f, 0f)),
                SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
        }

        [Test]
        public void AnEstablishedContactWithoutClosingIsNotImminent()
        {
            Expect(Evaluate(Command(), Leader(Other, -0.5f, 10f)), SafetyVerdict.Pass, SafetyReason.None);
            Expect(Evaluate(Command(), Leader(Other, -0.5f, 12f)), SafetyVerdict.Pass, SafetyReason.None);
        }

        // ------------------------------------------------------------------ priorite (S2)

        [Test]
        public void SeveralCausesYieldTheSingleHighestPriorityReason()
        {
            var imminent = Leader(Other, 1f, 0f);
            Expect(Evaluate(Command(acceleration: float.NaN), imminent), SafetyVerdict.Reject, SafetyReason.NonFiniteOutput);
            Expect(Evaluate(Command(acceleration: float.NaN), imminent, actor: Other), SafetyVerdict.Reject,
                SafetyReason.InvalidActorState);
            Expect(Evaluate(Command(curvature: 0.5f), imminent, step: Frame + 1), SafetyVerdict.Reject, SafetyReason.StalePlan);
            Expect(Evaluate(Command(curvature: 0.5f), imminent, frame: FrameWith(closed: true)), SafetyVerdict.Reject,
                SafetyReason.LocalPlanInvalidated);
            Expect(Evaluate(Command(curvature: 0.5f, angle: 40f), imminent), SafetyVerdict.Reject,
                SafetyReason.PhysicallyInvalidPath);
            Expect(Evaluate(Command(angle: 40f), imminent), SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
        }

        // ------------------------------------------------------------------ contact autorise (S4)

        [Test]
        public void AnAuthorizedContactExemptsOnlyItsTargetWithinExpiryAndScope()
        {
            var imminent = Leader(Target, 5f, 0f);
            Expect(Evaluate(Command(), imminent, authorization: new AuthorizedContact(Target, 15f, Frame)),
                SafetyVerdict.Pass, SafetyReason.None);
            Expect(Evaluate(Command(), imminent, authorization: new AuthorizedContact(Target, 15f, Frame - 1)),
                SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
            Expect(Evaluate(Command(), imminent, authorization: new AuthorizedContact(Target, 5f, Frame)),
                SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
            Expect(Evaluate(Command(), Leader(Other, 5f, 0f), authorization: new AuthorizedContact(Target, 15f, Frame)),
                SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
        }

        [Test]
        public void AnInvalidScopeVoidsTheAuthorization()
        {
            var imminent = Leader(Target, 5f, 0f);
            foreach (var scope in new[] { float.NaN, -1f, float.PositiveInfinity })
            {
                Assert.That(new AuthorizedContact(Target, scope, Frame).Valid, Is.False);
                Expect(Evaluate(Command(), imminent, authorization: new AuthorizedContact(Target, scope, Frame)),
                    SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
            }
        }

        [Test]
        public void WalkingPlayersAndPedestriansAreNeverExempted()
        {
            var authorization = new AuthorizedContact(Target, 15f, Frame);
            foreach (var kind in new[] { PerceivedObstacleKind.WalkingPlayer, PerceivedObstacleKind.Pedestrian })
                Expect(Evaluate(Command(), Obstacle(Target, kind, 2f, 0f), authorization: authorization),
                    SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
            Expect(Evaluate(Command(), Obstacle(Target, PerceivedObstacleKind.Vehicle, 2f, 0f), authorization: authorization),
                SafetyVerdict.Pass, SafetyReason.None);
        }

        [Test]
        public void AnAuthorizationNeverNeutralizesAnotherReason()
        {
            var authorization = new AuthorizedContact(Target, 15f, Frame);
            var imminent = Leader(Target, 5f, 0f);
            Expect(Evaluate(Command(angle: 40f), imminent, authorization: authorization), SafetyVerdict.Clamp,
                SafetyReason.PhysicallyInvalidIntent);
            Expect(Evaluate(Command(acceleration: float.NaN), imminent, authorization: authorization), SafetyVerdict.Reject,
                SafetyReason.NonFiniteOutput);
            Expect(Evaluate(Command(), imminent, frame: FrameWith(closed: true), authorization: authorization),
                SafetyVerdict.Reject, SafetyReason.LocalPlanInvalidated);
        }

        // ------------------------------------------------------------------ epoques et echantillon recent (S5)

        [Test]
        public void SourceFrameAndPhysicsStepEpochsStayDistinct()
        {
            var result = Evaluate(Command(from: Frame, to: Frame + 2), step: Frame + 1);
            Assert.That(result.SourceFrameId, Is.EqualTo(Frame));
            Assert.That(result.PhysicsStep, Is.EqualTo(Frame + 1));
            Assert.That(result.NearFieldStep, Is.EqualTo(Frame));
            Assert.That(result.ToText(), Is.EqualTo("Pass/None source 10 pas 11 proximite 10"));
        }

        [Test]
        public void ANewerSampleActsOnlyThroughTheCollisionVeto()
        {
            var command = Command(from: Frame, to: Frame + 2);
            var vetoed = Evaluate(command, Leader(Other, 50f, 10f), step: Frame + 1,
                newer: new NearFieldSample(Frame + 1, Leader(Other, 5f, 0f)));
            Expect(vetoed, SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
            Assert.That(vetoed.NearFieldStep, Is.EqualTo(Frame + 1));
            Assert.That(vetoed.SourceFrameId, Is.EqualTo(Frame));

            // Danger disparu de l'echantillon : la commande passe telle quelle, jamais recalculee.
            var cleared = Evaluate(command, Leader(Other, 5f, 0f), step: Frame + 1,
                newer: new NearFieldSample(Frame + 1, Leader(Other, 50f, 10f)));
            Expect(cleared, SafetyVerdict.Pass, SafetyReason.None);
            Assert.That(cleared.Command.Value, Is.EqualTo(command));

            // Echantillon sans perception, ou pas posterieur a la frame, ou au-dela du pas courant : ignore, la frame source fait foi.
            foreach (var ignored in new[] { new NearFieldSample(Frame + 1, null), new NearFieldSample(Frame, Leader(Other, 50f, 10f)),
                new NearFieldSample(Frame + 2, Leader(Other, 50f, 10f)) })
            {
                var result = Evaluate(command, Leader(Other, 5f, 0f), step: Frame + 1, newer: ignored);
                Expect(result, SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
                Assert.That(result.NearFieldStep, Is.EqualTo(Frame));
            }

            // Les autres raisons restent evaluees sur la frame, quel que soit l'echantillon.
            Expect(Evaluate(command, null, frame: FrameWith(closed: true), step: Frame + 1,
                newer: new NearFieldSample(Frame + 1, Leader(Other, 50f, 10f))), SafetyVerdict.Reject,
                SafetyReason.LocalPlanInvalidated);
        }

        // ------------------------------------------------------------------ bornes reelles et composeur

        [Test]
        public void TheVehicleLimitsAreTheComposerCapacitiesAndTheDeclaredDrivability()
        {
            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath).Profile;
            var drivability = Model.DrivabilityProfile;
            var limits = SafetyLimits.For(vehicle, drivability, Dt, TrafficV2Settings.PlanValiditySteps);
            Assert.That(limits.MaxWheelAngleDegrees, Is.EqualTo(drivability.LowSpeedLockDegrees));
            Assert.That(limits.MaxCurvaturePerMeter, Is.EqualTo(1f / RoadModelCompiler.AdmissionRadiusMeters(drivability)));
            Assert.That(limits.LatencySeconds, Is.EqualTo(TrafficV2Settings.PlanValiditySteps * Dt));

            // b_max et a_max sont les capacites memes du composeur : -b_max serre le frein a fond, a_max ouvre le gaz a fond.
            var composer = new VehicleDriveIntentComposer(vehicle,
                AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile.SafeBrakingLimit, Dt);
            float b = limits.MaxBrakingDecelerationMetersPerSecondSquared, a = limits.MaxDriveAccelerationMetersPerSecondSquared;
            Assert.That(composer.Compose(Frame, Command(acceleration: -b), V2FallbackReason.None, 10f, 0f).Intent.BrakeReverse,
                Is.EqualTo(1f).Within(1e-5f));
            Assert.That(composer.Compose(Frame, Command(acceleration: -0.5f * b), V2FallbackReason.None, 10f, 0f).Intent.BrakeReverse,
                Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(composer.Compose(Frame, Command(acceleration: 0.5f * a), V2FallbackReason.None, 0f, 0f).Intent.Throttle,
                Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(() => SafetyLimits.For(default(VehicleProfile), drivability, Dt, 1), Throws.ArgumentException);
        }

        [Test]
        public void TheComposerAppliesEachVerdictAsTheDriverPassesIt()
        {
            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath).Profile;
            var limits = SafetyLimits.For(vehicle, Model.DrivabilityProfile, Dt, TrafficV2Settings.PlanValiditySteps);
            float safeBraking = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile.SafeBrakingLimit;
            System.Func<SafetyResult, ComposedDrive> compose = result => new VehicleDriveIntentComposer(vehicle, safeBraking, Dt)
                .Compose(Frame, result.Command, result.Refusal, 10f, 0f, emergencyStop: result.Verdict == SafetyVerdict.EmergencyStop);

            var rejected = SafetyFilter.Evaluate(Command(acceleration: float.NaN), Frame, FrameWith(), Self, 10f, null, null, limits);
            Assert.That(rejected.Refusal, Is.EqualTo(V2FallbackReason.SafetyRejected));
            var fallback = compose(rejected);
            Assert.That(fallback.Fallback, Is.True);
            Assert.That(fallback.Reason, Is.EqualTo(V2FallbackReason.SafetyRejected));

            var stopped = SafetyFilter.Evaluate(Command(acceleration: 2f, curvature: 0f), Frame, FrameWith(), Self, 10f, null, Leader(Other, 1f, 0f), limits);
            Assert.That(stopped.Verdict, Is.EqualTo(SafetyVerdict.EmergencyStop));
            Assert.That(stopped.Refusal, Is.EqualTo(V2FallbackReason.None));
            var brake = compose(stopped);
            Assert.That(brake.Fallback, Is.False);
            Assert.That(brake.Intent.Throttle, Is.EqualTo(0f));
            Assert.That(brake.Intent.BrakeReverse, Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void EmergencyBrakingKeepsFullAuthorityWithDampingAndInsideTheServiceBand()
        {
            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath).Profile;
            var limits = SafetyLimits.For(vehicle, Model.DrivabilityProfile, Dt, TrafficV2Settings.PlanValiditySteps);
            float safeBraking = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile.SafeBrakingLimit;
            var composer = new VehicleDriveIntentComposer(vehicle, safeBraking, Dt);
            foreach (float speed in new[] { 0.04f, 0.2f, composer.ServiceBandMetersPerSecond,
                composer.ServiceBandMetersPerSecond + 0.01f, 10f })
            {
                var stopped = SafetyFilter.Evaluate(Command(curvature: 0f), Frame, FrameWith(), Self, speed,
                    null, Leader(Other, 0f, 0f), limits);
                Expect(stopped, SafetyVerdict.EmergencyStop, SafetyReason.ImminentUnintendedCollision);
                foreach (float damping in new[] { 0f, 0.3f, 3f })
                {
                    var drive = composer.Compose(Frame, stopped.Command, stopped.Refusal, speed, damping,
                        emergencyStop: stopped.Verdict == SafetyVerdict.EmergencyStop);
                    Assert.That(drive.Fallback, Is.False);
                    Assert.That(drive.Intent.Throttle, Is.EqualTo(0f), speed + " m/s, damping " + damping);
                    Assert.That(drive.Intent.BrakeReverse, Is.EqualTo(speed > composer.ServiceBandMetersPerSecond ? 1f : 0f));
                    Assert.That(drive.Intent.Handbrake, Is.EqualTo(speed > composer.ServiceBandMetersPerSecond ? 0f : 1f));
                    Assert.That(VehicleDriveIntentComposer.MinimumWheelDriveTorque(vehicle, drive.Intent, drive.MaxForwardSpeed, speed),
                        Is.GreaterThanOrEqualTo(0f), "aucune marche arriere d'urgence");
                }
            }
            var normal = composer.Compose(Frame, Command(acceleration: -3f), V2FallbackReason.None, 0.2f, 0f);
            Assert.That(normal.Intent.BrakeReverse, Is.EqualTo(0f));
            Assert.That(normal.Intent.Handbrake, Is.EqualTo(0f), "la commande normale conserve sa roue libre");
        }

        [Test]
        public void TheProjectionRendersEverySafetyVerdictReasonAndEpochInEveryCulture()
        {
            var projection = PlanningSpine.Evaluate(new PlanningRequest(FrameWith(), Self, null, Other,
                new RouteSeed(1), 10f, null, null, null, default(DriverProfile),
                bounds: new LongitudinalBounds(2f, 3f))).Projection;
            foreach (var result in new[] { Evaluate(Command()), Evaluate(Command(angle: 40f)),
                Evaluate(Command(), Leader(Other, 0f, 0f)), Evaluate(Command(acceleration: float.NaN)) })
            {
                var withDrive = projection.WithDrive(new TrafficDriveOutcome(Frame, Frame, Frame,
                    0f, 0f, 0f, 0f, result.Verdict == SafetyVerdict.Reject, result.Refusal.ToString(), "None",
                    null, null, VehicleCoverage.NotEstablished, null, result.ToText()));
                var originalCulture = CultureInfo.CurrentCulture;
                try
                {
                    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                    string invariant = withDrive.ToText();
                    StringAssert.Contains("Safety " + result.ToText(), invariant);
                    CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                    Assert.That(withDrive.ToText(), Is.EqualTo(invariant));
                }
                finally { CultureInfo.CurrentCulture = originalCulture; }
            }
            StringAssert.DoesNotContain("\nSafety ", projection.WithDrive(new TrafficDriveOutcome(Frame, Frame, Frame,
                0f, 0f, 0f, 0f, false, null, "None", null, null, VehicleCoverage.NotEstablished, null)).ToText());
        }

        // ------------------------------------------------------------------ structure et branchement

        [Test]
        public void SafetyContainsNoTacticalConcernAndNoState()
        {
            var files = Directory.GetFiles("Assets/RoadRage/Features/Vehicles/Traffic/Safety", "*.cs");
            Assert.That(files, Is.Not.Empty);
            var forbidden = new Regex(@"\b(RoutePlan|RoutePlanner|PlanningSpine|JunctionSnapshot|JunctionCoordinator|"
                + @"JunctionConflictIndex|JunctionRequestBuilder|SignalState|LongitudinalArbitration|BlockerTracker|Blocker|"
                + @"Recovery|Rage|Fear|Target(ing)?Selector|Maneuver|Overtak\w*|Bypass|Replan\w*|VehicleDriveIntent|"
                + @"ApplyDriveIntent|Rigidbody|MonoBehaviour|NetworkVariable|Random)\b");
            foreach (var file in files)
            {
                var match = forbidden.Match(File.ReadAllText(file));
                Assert.That(match.Success, Is.False, file + " : " + match.Value);
            }
            foreach (var field in typeof(SafetyFilter).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                Assert.That(field.IsLiteral, Is.True, field.Name);
            StringAssert.DoesNotContain("RoadRage.Features.Rage",
                File.ReadAllText("Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef"));
        }

        [Test]
        public void TheDriverFiltersBetweenTrackingAndComposition()
        {
            string source = File.ReadAllText("Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs");
            int track = source.IndexOf("MotionCommand.Track(", System.StringComparison.Ordinal);
            int filter = source.IndexOf("SafetyFilter.Evaluate(", System.StringComparison.Ordinal);
            int compose = source.IndexOf("composer.Compose(", System.StringComparison.Ordinal);
            Assert.That(track, Is.GreaterThan(0));
            Assert.That(filter, Is.GreaterThan(track));
            Assert.That(compose, Is.GreaterThan(filter));
            Assert.That(Regex.Matches(source, @"SafetyFilter\.Evaluate\(").Count, Is.EqualTo(1));
            string between = source.Substring(filter, compose - filter);
            StringAssert.Contains("command = LastSafety.Command;", between);
            StringAssert.Contains("refusal = LastSafety.Refusal;", between);
            StringAssert.Contains("emergencyStop: LastSafety != null && LastSafety.Verdict == SafetyVerdict.EmergencyStop", source);
            StringAssert.Contains("if (command.HasValue && !toleranceResponse.Latched)", source.Substring(track, filter - track));
            Assert.That((int)V2FallbackReason.SafetyRejected, Is.EqualTo(12));
        }

        /// <summary>Chemin local minimal : un seul span, sans geometrie.</summary>
        private sealed class LocalPath : IPathGeometry
        {
            private readonly List<PathSpan> spans;
            public LocalPath(RoadId id) { spans = new List<PathSpan> { new PathSpan(RoadElementKind.LaneCorridor, id, 0f, 10f, 0f, 10f) }; }
            public IReadOnlyList<PathSpan> Spans { get { return spans; } }
            public float LengthMeters { get { return 10f; } }
            public int PointCount(int span) { return 0; }
            public Vector3 PointPosition(int span, int point) { return Vector3.zero; }
            public Vector3 PointTangent(int span, int point) { return Vector3.forward; }
            public float PointDistance(int span, int point) { return 0f; }
        }
    }
}
