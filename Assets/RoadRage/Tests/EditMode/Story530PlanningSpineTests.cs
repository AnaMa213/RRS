using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    [Category("Core")]
    public sealed class Story530PlanningSpineTests
    {
        private const string ModelPath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json";
        private const string SignoffPath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-signoff.json";
        private const string ReportPath = "_bmad-output/implementation-artifacts/migration-report-5-28-mvp-run.md";

        private static CompiledRoadModel Load(out string modelText, out string signoffText, out string reportText)
        {
            modelText = File.ReadAllText(ModelPath);
            signoffText = File.ReadAllText(SignoffPath);
            reportText = File.ReadAllText(ReportPath);
            return RoadModelCompiler.Compile(RoadModelDocument.Load(modelText));
        }

        private static VehicleFootprintPose Pose(RoadCurve curve, float s)
        {
            var point = curve.Sample(s);
            return new VehicleFootprintPose { Position = point.Position,
                Forward = point.Tangent, Up = point.Up };
        }

        private static string Sha256(string value)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                    .Replace("-", "").ToLowerInvariant();
        }

        [Test]
        public void SyntheticEvidenceUsesTheExactResidueBlockAndVersion()
        {
            string realText, signoff, report;
            var model = Load(out realText, out signoff, out report);
            const string modelText = "synthetic model bytes\n";
            const string block = "a_e = 0 m\n| Genre | Valeur |\n|---|---|\n| test | 0 |\n";
            const string syntheticReport = "### Residus\n" + block + "\nOther section\n";
            string syntheticSignoff = "{\"RoadModelVersion\":\"" + model.Version +
                "\",\"ModelHash\":\"" + Sha256(modelText) + "\",\"ClearanceHash\":\"" +
                Sha256(block) + "\"}";
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, syntheticSignoff,
                syntheticReport).Status, Is.EqualTo(GateAEvidenceStatus.Valid));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, syntheticSignoff,
                syntheticReport.Replace("| test | 0 |", "| test | 1 |")).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceStale));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, syntheticSignoff,
                syntheticReport.Replace("a_e = 0 m", "a_e = NaN m")).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceMissing));
        }

        [Test]
        public void FrameIsSortedCopiedAndLocalizedOnce()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var corridor = model.Corridors[0];
            var a = new TrafficActorInput(new RoadId(1, 2), Pose(corridor.Curve, 1f), 2f, corridor.CorridorId);
            var b = new TrafficActorInput(new RoadId(1, 1), Pose(corridor.Curve, 2f), 3f, corridor.CorridorId);
            var inputs = new List<TrafficActorInput> { a, b };
            var frame = new TrafficFrame(17, model, inputs);
            inputs.Clear();
            Assert.That(frame.Actors.Count, Is.EqualTo(2));
            Assert.That(frame.Actors[0].TrafficId, Is.EqualTo(b.TrafficId));
            Assert.That(frame.Actors[1].TrafficId, Is.EqualTo(a.TrafficId));
            Assert.That(frame.Actors[0].Location.ModelVersion, Is.EqualTo(model.Version));
            Assert.Throws<ArgumentException>(() => new TrafficFrame(17, model, new[] { a, a }));
            Assert.Throws<ArgumentException>(() => new TrafficFrame(17, model,
                new[] { new TrafficActorInput(RoadId.None, a.Pose, 0f, RoadId.None) }));
        }

        [Test]
        public void SignedEvidenceBindsAllThreeArtifactsAndRejectsByteChanges()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var valid = GateAEvidenceBinding.Bind(model, modelText, signoff, report);
            Assert.That(valid.Status, Is.EqualTo(GateAEvidenceStatus.Valid));
            Assert.That(valid.TrackingAllowanceMeters, Is.Zero);
            Assert.That(GateAEvidenceBinding.Bind(model, modelText + " ", signoff, report).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceStale));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, signoff.Replace(model.Version.ToString(), "v0:000"), report).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceStale));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, signoff,
                report.Replace("a_e = 0 m", "a_e = 1 m")).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceStale));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, signoff,
                report.Replace("a_e = 0 m", "a_e = x m")).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceMissing));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, signoff,
                report.Replace("a_e = 0 m", "a_e = 0  m")).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceStale));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, signoff,
                report.Replace("\n", "\r\n")).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceStale));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, "", report).Status,
                Is.EqualTo(GateAEvidenceStatus.GateAEvidenceMissing));
        }

        [Test]
        public void DecisionSeparatesRoutePathAndMotionAndKeepsFrameUntouched()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var entry = model.Portals.First(p => p.Role == PortalRole.Entry);
            EffectiveLaneCorridor corridor;
            Assert.That(model.TryGetCorridor(entry.CorridorId, out corridor), Is.True);
            var trafficId = new RoadId(0x530, 1);
            var frame = new TrafficFrame(23, model, new[] {
                new TrafficActorInput(trafficId, Pose(corridor.Curve, entry.SMeters), 0f, entry.CorridorId) });
            var before = frame.Actors[0].Location;
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null, RoadId.None,
                new RouteSeed(1), 200f, modelText, signoff, report, default(RoadRage.Features.Vehicles.DriverProfile),
                bounds: new LongitudinalBounds(2f, 3f)));
            Assert.That(decision.Route.Plan, Is.Not.Null);
            Assert.That(decision.Path.Intervals.Count, Is.GreaterThan(0));
            Assert.That(decision.Motion.ReferenceCoverage, Is.EqualTo(ReferenceCoverage.Covered));
            Assert.That(decision.Motion.VehicleCoverage, Is.EqualTo(VehicleCoverage.NotEstablished));
            Assert.That(decision.Projection.ToText(), Does.Contain("Frame 23 / RoadModelVersion"));
            Assert.That(decision.Projection.ToText(), Does.Contain("vehicle NotEstablished"));
            Assert.That(decision.Projection.NextMovementId, Is.Not.EqualTo(RoadId.None));
            Assert.That(decision.Projection.ToText(),
                Does.Contain("Next movement " + decision.Projection.NextMovementId));
            Assert.That(frame.Actors[0].Location.ElementId, Is.EqualTo(before.ElementId));
            Assert.That(frame.Actors[0].Location.SMeters, Is.EqualTo(before.SMeters));
            var uncovered = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null, RoadId.None,
                new RouteSeed(1), 200f, modelText, signoff, report, default(RoadRage.Features.Vehicles.DriverProfile),
                new TrackingTolerance(.01f), new LongitudinalBounds(2f, 3f)));
            Assert.That(uncovered.Motion.Issue, Is.EqualTo(MotionIssue.NotCoveredByGateA));
            Assert.That(uncovered.Motion.VehicleCoverage, Is.EqualTo(VehicleCoverage.NotEstablished));
            var underReservedMargin = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null,
                RoadId.None, new RouteSeed(1), 200f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile),
                new TrackingTolerance(.1f), new LongitudinalBounds(2f, 3f)));
            Assert.That(underReservedMargin.Motion.Issue, Is.EqualTo(MotionIssue.NotCoveredByGateA));
            var missing = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null,
                RoadId.None, new RouteSeed(1), 200f, modelText, "", report,
                default(RoadRage.Features.Vehicles.DriverProfile), bounds: new LongitudinalBounds(2f, 3f)));
            Assert.That(missing.Path.Intervals.Count, Is.GreaterThan(0));
            Assert.That(missing.Motion.GeometricallyFeasible, Is.False);
            Assert.That(missing.Motion.Issue, Is.EqualTo(MotionIssue.GateAEvidenceMissing));
            var stale = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null,
                RoadId.None, new RouteSeed(1), 200f, modelText + " ", signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile), bounds: new LongitudinalBounds(2f, 3f)));
            Assert.That(stale.Motion.Issue, Is.EqualTo(MotionIssue.GateAEvidenceStale));
            var missingAndInvalidTracking = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId,
                null, RoadId.None, new RouteSeed(1), 200f, modelText, "", report,
                default(RoadRage.Features.Vehicles.DriverProfile), new TrackingTolerance(float.NaN),
                new LongitudinalBounds(2f, 3f)));
            Assert.That(missingAndInvalidTracking.Motion.Issue, Is.EqualTo(MotionIssue.GateAEvidenceMissing));

            var candidate = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null,
                RoadId.None, new RouteSeed(1), 200f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile), bounds: new LongitudinalBounds(2f, 3f),
                candidateSpeedProfile: new[] { new SpeedProfilePoint(0f, -1f),
                    new SpeedProfilePoint(1f, 0f) }));
            Assert.That(candidate.SpeedProfile.HasValue, Is.True);
            Assert.That(candidate.SpeedProfile.Value.Issue, Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
            Assert.That(candidate.Projection.Code, Is.EqualTo("InvalidSpeedProfile"));
        }

        [Test]
        public void CandidateProfileChecksBoundsAndDoesNotGenerateSpeeds()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var entry = model.Portals.First(p => p.Role == PortalRole.Entry);
            EffectiveLaneCorridor corridor;
            model.TryGetCorridor(entry.CorridorId, out corridor);
            var frame = new TrafficFrame(24, model, new[] { new TrafficActorInput(
                new RoadId(0x530, 2), Pose(corridor.Curve, entry.SMeters), 0f, entry.CorridorId) });
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, frame.Actors[0].TrafficId,
                null, RoadId.None, new RouteSeed(1), 200f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile), bounds: new LongitudinalBounds(2f, 3f)));
            var motion = decision.Motion;
            float length = motion.Path.LengthMeters;
            var strictProfile = model.DrivabilityProfile;
            strictProfile.SteeringInactiveBelowMetersPerSecond = 100f;
            var firstInvalidCeiling = motion.Path.Intervals.SelectMany(x => x.Points)
                .First(x => x.SteeringCeilingMetersPerSecond < 100f).DistanceMeters;
            var invalidCeiling = new MotionPlan(motion.Path, strictProfile, motion.Evidence,
                TrackingTolerance.Undeclared);
            Assert.That(invalidCeiling.Issue, Is.EqualTo(MotionIssue.InvalidSteeringCeiling));
            Assert.That(invalidCeiling.IssueDistanceMeters, Is.EqualTo(firstInvalidCeiling));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(length, 0f) }, new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.None));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, -1f),
                new SpeedProfilePoint(length, 0f) }, new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, float.NaN),
                new SpeedProfilePoint(length, 0f) }, new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(length + 1f, 0f) }, new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(1f, 0f),
                new SpeedProfilePoint(1f, 0f) }, new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(1f, 3f) }, new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.AccelerationBoundExceeded));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 2f),
                new SpeedProfilePoint(1f, 0f) }, new LongitudinalBounds(2f, 2f)).Issue,
                Is.EqualTo(SpeedProfileIssue.None));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 2f),
                new SpeedProfilePoint(1f, 0f) }, new LongitudinalBounds(2f, 1f)).Issue,
                Is.EqualTo(SpeedProfileIssue.AccelerationBoundExceeded));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(1f, 0f), new SpeedProfilePoint(2f, .1f) },
                new LongitudinalBounds(2f, 3f)).Issue, Is.EqualTo(SpeedProfileIssue.None));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 100f),
                new SpeedProfilePoint(length, 100f) }, new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.SteeringCeilingExceeded));
            var slow = motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, .1f),
                new SpeedProfilePoint(length, .1f) }, new LongitudinalBounds(2f, 3f));
            Assert.That(slow.Issue, Is.EqualTo(SpeedProfileIssue.None));
            Assert.That(slow.Diagnostics & MotionDiagnostic.SteeringInactiveSpan,
                Is.EqualTo(MotionDiagnostic.SteeringInactiveSpan));
        }

        [Test]
        public void NewPlanningSurfaceContainsNoControlOrTargetIndex()
        {
            foreach (var type in new[] { typeof(PathHorizon), typeof(MotionPlan) })
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
                    Assert.That(member.Name, Does.Not.Match("(?i)(waypoint|target.*index)"));
            string root = "Assets/RoadRage/Features/Vehicles/Traffic";
            foreach (var folder in new[] { "Frame", "Perception", "Planning", "Debug" })
                foreach (var file in Directory.GetFiles(Path.Combine(root, folder), "*.cs"))
                {
                    string source = File.ReadAllText(file);
                    foreach (var forbidden in new[] { "VehicleDriveIntent", "VehiclePhysicsBody", "ApplyDriveIntent",
                        "Rigidbody", "MonoBehaviour", "NetworkVariable", "Rpc", "LateralClearanceMarginMeters" })
                        Assert.That(source, Does.Not.Contain(forbidden), file);
                }
            Assert.That(Directory.GetFiles(root, "*.asmdef", SearchOption.AllDirectories), Is.Empty);
        }

        [Test]
        public void AdmissionAndRouteFailuresHaveStableOutcomes()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var source = RoadModelDocument.Load(modelText);
            source.DrivabilityProfile = default(DrivabilityProfile);
            var undeclared = RoadModelCompiler.Compile(source);
            var entry = model.Portals.First(p => p.Role == PortalRole.Entry);
            EffectiveLaneCorridor corridor;
            model.TryGetCorridor(entry.CorridorId, out corridor);
            var trafficId = new RoadId(0x530, 3);
            var actor = new TrafficActorInput(trafficId, Pose(corridor.Curve, entry.SMeters), 0f, entry.CorridorId);
            var invalidModelFrame = new TrafficFrame(30, undeclared, new[] { actor });
            Assert.That(() => PlanningSpine.Evaluate(new PlanningRequest(invalidModelFrame, trafficId,
                null, RoadId.None, new RouteSeed(1), 10f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile))),
                Throws.InvalidOperationException.With.Message.EqualTo("UndeclaredDrivabilityProfile"));

            var frame = new TrafficFrame(30, model, new[] { actor });
            Assert.That(() => PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null,
                RoadId.None, new RouteSeed(1), float.NaN, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile))),
                Throws.ArgumentException.With.Message.Contains("InvalidLookAhead"));
            var missingExit = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null,
                new RoadId(0x530, 999), new RouteSeed(1), 10f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile)));
            Assert.That(missingExit.Route.Outcome, Is.EqualTo(RouteOutcome.NoRoute));
            Assert.That(missingExit.Route.Reason, Is.EqualTo(RouteReason.DestinationUnavailable));
            Assert.That(missingExit.Path, Is.Null);

            var first = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null,
                RoadId.None, new RouteSeed(1), 10f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile)));
            var otherId = new RoadId(0x530, 4);
            var otherFrame = new TrafficFrame(31, model, new[] {
                new TrafficActorInput(otherId, actor.Pose, 0f, entry.CorridorId) });
            var stale = PlanningSpine.Evaluate(new PlanningRequest(otherFrame, otherId, first.Route.Plan,
                RoadId.None, new RouteSeed(1), 10f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile)));
            Assert.That(stale.Route.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(stale.Route.Reason, Is.EqualTo(RouteReason.StalePlan));
        }

        [Test]
        public void ProjectionDoesNotDependOnActorInputOrder()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var entry = model.Portals.First(p => p.Role == PortalRole.Entry);
            EffectiveLaneCorridor corridor;
            model.TryGetCorridor(entry.CorridorId, out corridor);
            var a = new TrafficActorInput(new RoadId(0x530, 5), Pose(corridor.Curve, entry.SMeters),
                0f, entry.CorridorId);
            var b = new TrafficActorInput(new RoadId(0x530, 6), Pose(corridor.Curve, entry.SMeters),
                0f, entry.CorridorId);
            var first = new TrafficFrame(32, model, new[] { a, b });
            var reversed = new TrafficFrame(32, model, new[] { b, a });
            var left = PlanningSpine.Evaluate(new PlanningRequest(first, a.TrafficId, null, RoadId.None,
                new RouteSeed(1), 10f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile)));
            var right = PlanningSpine.Evaluate(new PlanningRequest(reversed, a.TrafficId, null, RoadId.None,
                new RouteSeed(1), 10f, modelText, signoff, report,
                default(RoadRage.Features.Vehicles.DriverProfile)));
            Assert.That(right.Projection.ToText(), Is.EqualTo(left.Projection.ToText()));
        }
    }
}
