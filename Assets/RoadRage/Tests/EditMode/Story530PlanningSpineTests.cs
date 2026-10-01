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
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    [Category("Core")]
    public sealed class Story530PlanningSpineTests
    {
        private const string HistoricalDirectory = "_bmad-output/implementation-artifacts/gate-a-5-52/historical-signed-5-51/";
        private const string ModelPath = HistoricalDirectory + "MVP_Run.road-model.json";
        private const string SignoffPath = HistoricalDirectory + "MVP_Run.road-signoff.json";
        private const string ReportPath = HistoricalDirectory + "migration-report-5-28-mvp-run.md";

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
                new SpeedProfilePoint(1f, 3f), new SpeedProfilePoint(length, 3f) },
                new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.AccelerationBoundExceeded));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 2f),
                new SpeedProfilePoint(1f, 0f), new SpeedProfilePoint(length, 0f) },
                new LongitudinalBounds(2f, 2f)).Issue, Is.EqualTo(SpeedProfileIssue.None));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 2f),
                new SpeedProfilePoint(1f, 0f), new SpeedProfilePoint(length, 0f) },
                new LongitudinalBounds(2f, 1f)).Issue,
                Is.EqualTo(SpeedProfileIssue.AccelerationBoundExceeded));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(1f, 0f), new SpeedProfilePoint(2f, .1f),
                new SpeedProfilePoint(length, .1f) },
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
            foreach (var type in new[] { typeof(PathHorizon), typeof(MotionPlan), typeof(PlanningRequest),
                typeof(PlanningDecision), typeof(TrafficDecisionProjection), typeof(TrafficFrame),
                typeof(TrafficActor), typeof(AgentObservation) })
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
                    Assert.That(member.Name, Does.Not.Match("(?i)(waypoint|target.*index)"));
            string root = "Assets/RoadRage/Features/Vehicles/Traffic";
            var files = new List<string> { Path.Combine(root, "PlanningSpine.cs") };
            foreach (var folder in new[] { "Frame", "Perception", "Planning", "Debug" })
                files.AddRange(Directory.GetFiles(Path.Combine(root, folder), "*.cs"));
            Assert.That(files.Count, Is.GreaterThanOrEqualTo(8));
            foreach (var file in files)
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
            var a = new TrafficActorInput(new RoadId(0x530, 5), Pose(corridor.Curve, entry.SMeters + 1f),
                0f, entry.CorridorId);
            var b = new TrafficActorInput(new RoadId(0x530, 6), Pose(corridor.Curve, entry.SMeters + 2f),
                0f, entry.CorridorId);
            var c = new TrafficActorInput(new RoadId(0x530, 7), Pose(corridor.Curve, entry.SMeters + 3f),
                0f, entry.CorridorId);
            var orders = new[] { new[] { a, b, c }, new[] { a, c, b }, new[] { b, a, c },
                new[] { b, c, a }, new[] { c, a, b }, new[] { c, b, a } };
            foreach (var target in new[] { a, b, c })
            {
                string expected = null;
                foreach (var order in orders)
                {
                    var frame = new TrafficFrame(32, model, order);
                    string text = Decide(frame, target.TrafficId, 10f, modelText, signoff, report)
                        .Projection.ToText();
                    if (expected == null) expected = text;
                    Assert.That(text, Is.EqualTo(expected), target.TrafficId.ToString());
                }
            }
        }

        [Test]
        public void SignedTangentialSpeedIsKeptAndNeverReadAsPlanProgress()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var id = new RoadId(0x530, 40);
            var reverse = EntryFrame(model, 0, 60, id, -3f);
            var forward = EntryFrame(model, 0, 60, id, 3f);
            Assert.That(reverse.Actors[0].TangentialSpeedMetersPerSecond, Is.EqualTo(-3f));
            Assert.That(new AgentObservation(60, reverse.Actors[0]).TangentialSpeedMetersPerSecond, Is.EqualTo(-3f));
            var left = Decide(reverse, id, 200f, modelText, signoff, report);
            var right = Decide(forward, id, 200f, modelText, signoff, report);
            // Route, horizon et projection ne lisent pas la vitesse : le signe ne change rien au plan.
            Assert.That(left.Observation.TangentialSpeedMetersPerSecond, Is.EqualTo(-3f));
            Assert.That(right.Observation.TangentialSpeedMetersPerSecond, Is.EqualTo(3f));
            Assert.That(left.Projection.ToText(), Is.EqualTo(right.Projection.ToText()));
            Assert.That(left.Path.LengthMeters, Is.EqualTo(right.Path.LengthMeters));
            Assert.That(left.Route.Plan.Occurrences.Count, Is.EqualTo(right.Route.Plan.Occurrences.Count));
            var pose = reverse.Actors[0].Pose;
            foreach (var invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assert.That(() => new TrafficFrame(60, model, new[] {
                    new TrafficActorInput(id, pose, invalid, RoadId.None) }),
                    Throws.ArgumentException.With.Message.Contains("InvalidSpeed"));
        }

        [Test]
        public void ActorLookupAndInputRefusalsAreStable()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var entry = model.Portals.First(p => p.Role == PortalRole.Entry);
            EffectiveLaneCorridor corridor;
            model.TryGetCorridor(entry.CorridorId, out corridor);
            var inputs = new List<TrafficActorInput>();
            foreach (ulong low in new ulong[] { 3, 1, 2 })
                inputs.Add(new TrafficActorInput(new RoadId(1, low), Pose(corridor.Curve, entry.SMeters + low),
                    0f, entry.CorridorId));
            var frame = new TrafficFrame(41, model, inputs);
            for (ulong low = 1; low <= 3; low++)
            {
                TrafficActor found;
                Assert.That(frame.TryGetActor(new RoadId(1, low), out found), Is.True, low.ToString());
                Assert.That(found.TrafficId, Is.EqualTo(new RoadId(1, low)));
            }
            TrafficActor absent;
            Assert.That(frame.TryGetActor(new RoadId(1, 0), out absent), Is.False);
            Assert.That(frame.TryGetActor(new RoadId(1, 4), out absent), Is.False);
            Assert.That(frame.TryGetActor(RoadId.None, out absent), Is.False);

            var last = new RoadId(1, 3);
            Assert.That(Decide(frame, last, 10f, modelText, signoff, report).Observation.TrafficId, Is.EqualTo(last));
            Assert.That(() => Decide(frame, new RoadId(1, 4), 10f, modelText, signoff, report),
                Throws.ArgumentException.With.Message.Contains("UnknownTrafficId"));
            Assert.That(() => Decide(frame, RoadId.None, 10f, modelText, signoff, report),
                Throws.ArgumentException.With.Message.Contains("EmptyTrafficId"));
            Assert.That(() => Decide(null, last, 10f, modelText, signoff, report), Throws.ArgumentNullException);
            foreach (var look in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                Assert.That(() => Decide(frame, last, look, modelText, signoff, report),
                    Throws.ArgumentException.With.Message.Contains("InvalidLookAhead"), look.ToString());
                var plan = Decide(frame, last, 10f, modelText, signoff, report).Route.Plan;
                Assert.That(() => PathHorizon.Build(model, plan, look),
                    Throws.ArgumentException.With.Message.Contains("InvalidLookAhead"), look.ToString());
            }
        }

        [Test]
        public void PlanFromAnotherModelVersionIsStaleAtRouteAndHorizon()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var other = Mutate(modelText, UnsignRoundabouts);
            Assert.That(other.Version, Is.Not.EqualTo(model.Version));
            var id = new RoadId(0x530, 41);
            var first = Decide(EntryFrame(model, 0, 1, id, 0f), id, 50f, modelText, signoff, report);
            var replanned = Decide(EntryFrame(other, 0, 2, id, 0f), id, 50f, modelText, signoff, report,
                existing: first.Route.Plan);
            Assert.That(replanned.Route.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(replanned.Route.Reason, Is.EqualTo(RouteReason.StalePlan));
            Assert.That(() => PathHorizon.Build(other, first.Route.Plan, 50f),
                Throws.ArgumentException.With.Message.Contains("StalePlan"));
        }

        [Test]
        public void DefaultEvidenceFailsClosed()
        {
            Assert.That(default(GateAEvidenceResult).Status, Is.EqualTo(GateAEvidenceStatus.GateAEvidenceMissing));
            Assert.That(default(GateAEvidenceResult).Valid, Is.False);
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var id = new RoadId(0x530, 42);
            var path = Decide(EntryFrame(model, 0, 1, id, 0f), id, 20f, modelText, signoff, report).Path;
            var motion = new MotionPlan(path, model.DrivabilityProfile, default(GateAEvidenceResult),
                TrackingTolerance.Undeclared);
            Assert.That(motion.GeometricallyFeasible, Is.False);
            Assert.That(motion.Issue, Is.EqualTo(MotionIssue.GateAEvidenceMissing));
            Assert.That(motion.ReferenceCoverage, Is.EqualTo(ReferenceCoverage.GateAEvidenceMissing));
        }

        [Test]
        public void ProjectionSaysNotEvaluatedWhenNoRouteExistsAndReportsEvaluatedStages()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var id = new RoadId(0x530, 43);
            var frame = EntryFrame(model, 0, 50, id, 0f);
            var noRoute = PlanningSpine.Evaluate(new PlanningRequest(frame, id, null, new RoadId(0x530, 999),
                new RouteSeed(1), 10f, modelText, signoff, report, NoDriver)).Projection;
            Assert.That(noRoute.EvidenceStatus, Is.Null);
            Assert.That(noRoute.ReferenceCoverage, Is.Null);
            Assert.That(noRoute.PathPlanId, Is.EqualTo("aucun"));
            Assert.That(noRoute.MotionPlanId, Is.EqualTo("aucun"));
            Assert.That(noRoute.ExitPortalId, Is.EqualTo(RoadId.None));
            Assert.That(noRoute.NextMovementId, Is.EqualTo(RoadId.None));
            Assert.That(noRoute.RouteOccurrences, Is.Empty);
            Assert.That(noRoute.Code, Is.EqualTo("DestinationUnavailable"));
            string text = noRoute.ToText();
            Assert.That(text, Does.Contain("Gate A non evalue / reference non evalue / vehicle NotEstablished"));
            Assert.That(text, Does.Contain("Steering ceiling non evalue"));
            Assert.That(text, Does.Not.Contain("GateAEvidenceMissing"));

            var decision = Decide(frame, id, 200f, modelText, signoff, report);
            var projection = decision.Projection;
            Assert.That(projection.EvidenceStatus, Is.EqualTo(GateAEvidenceStatus.Valid));
            Assert.That(projection.ReferenceCoverage, Is.EqualTo(ReferenceCoverage.Covered));
            Assert.That(projection.PathPlanId, Is.EqualTo("50:" + id + ":path"));
            Assert.That(projection.MotionPlanId, Is.EqualTo("50:" + id + ":motion"));
            float minimum = decision.Path.Intervals.SelectMany(x => x.Points).Min(x => x.SteeringCeilingMetersPerSecond);
            Assert.That(projection.SteeringCeilingText, Is.EqualTo(float.IsPositiveInfinity(minimum) ? "aucun"
                : minimum.ToString("R", CultureInfo.InvariantCulture) + " m/s"));
            Assert.That(projection.ToText(), Does.Contain("IDs " + string.Join(",",
                decision.Route.Plan.Occurrences.Select(x => x.Id.ToString()).ToArray())));
            Assert.That(projection.ToText(), Does.Contain("exit " + decision.Route.Plan.ExitPortalId));
        }

        [Test]
        public void NextMovementStartsFromTheReusedPlanProgress()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var id = new RoadId(0x530, 44);
            var first = Decide(EntryFrame(model, 0, 1, id, 0f), id, 10000f, modelText, signoff, report);
            var plan = first.Route.Plan;
            int firstMovement = -1;
            for (int i = 0; i < plan.Occurrences.Count && firstMovement < 0; i++)
                if (plan.Occurrences[i].Kind == RoadElementKind.JunctionMovement) firstMovement = i;
            Assert.That(firstMovement, Is.GreaterThanOrEqualTo(0));
            Assert.That(first.Projection.NextMovementId, Is.EqualTo(plan.Occurrences[firstMovement].Id));
            Assert.That(firstMovement + 1, Is.LessThan(plan.Occurrences.Count));

            // Le contrat 5.29 avance d'une occurrence contigue par decision, sans saut direct.
            var reused = plan;
            PlanningDecision second = null;
            for (int i = plan.ProgressOccurrenceIndex + 1; i <= firstMovement + 1; i++)
            {
                var next = plan.Occurrences[i];
                RoadCurve curve;
                EffectiveLaneCorridor corridor;
                CompiledJunctionMovement movement;
                if (next.Kind == RoadElementKind.LaneCorridor)
                { Assert.That(model.TryGetCorridor(next.Id, out corridor), Is.True); curve = corridor.Curve; }
                else
                { Assert.That(model.TryGetMovement(next.Id, out movement), Is.True); curve = movement.Curve; }
                var frame = new TrafficFrame((ulong)(i + 1), model, new[] { new TrafficActorInput(id,
                    Pose(curve, (next.StartSMeters + next.EndSMeters) * .5f), 0f, next.Id) });
                second = Decide(frame, id, 10000f, modelText, signoff, report, existing: reused);
                reused = second.Route.Plan;
                Assert.That(reused.ProgressOccurrenceIndex, Is.EqualTo(i), "Progression contigue de la route.");
            }
            Assert.That(reused.ProgressOccurrenceIndex, Is.GreaterThan(firstMovement));
            RoadId expected = RoadId.None;
            for (int i = reused.ProgressOccurrenceIndex; i < reused.Occurrences.Count && expected.IsEmpty; i++)
                if (reused.Occurrences[i].Kind == RoadElementKind.JunctionMovement) expected = reused.Occurrences[i].Id;
            Assert.That(second.Projection.NextMovementId, Is.EqualTo(expected));
            Assert.That(second.Projection.NextMovementId, Is.Not.EqualTo(plan.Occurrences[firstMovement].Id));
            Assert.That(second.Projection.ToText(), Does.Contain("Next movement " + expected));
        }

        [Test]
        public void DriverProfileSuppliesTheDefaultLongitudinalBounds()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            int entry = StraightEntry(model, modelText, signoff, report);
            var id = new RoadId(0x530, 45);
            var frame = EntryFrame(model, entry, 1, id, 0f);
            var driver = new RoadRage.Features.Vehicles.DriverProfile(10f, 1.5f, 2f, 1.7f, 2f, 0f, .1f, 2.9f, .5f, 1f, 1f);
            var request = new PlanningRequest(frame, id, null, RoadId.None, new RouteSeed(1), 5f,
                modelText, signoff, report, driver);
            Assert.That(request.Bounds.MaxAcceleration, Is.EqualTo(1.7f));
            Assert.That(request.Bounds.MaxDeceleration, Is.EqualTo(2.9f));
            float length = Decide(frame, id, 5f, modelText, signoff, report).Path.LengthMeters;
            var accelerate = new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(length, (float)Math.Sqrt(2d * 2.0 * length)) };
            var brake = new[] { new SpeedProfilePoint(0f, (float)Math.Sqrt(2d * 2.5 * length)),
                new SpeedProfilePoint(length, 0f) };
            Assert.That(Decide(frame, id, 5f, modelText, signoff, report, candidate: accelerate,
                driver: driver).SpeedProfile.Value.Issue, Is.EqualTo(SpeedProfileIssue.AccelerationBoundExceeded));
            Assert.That(Decide(frame, id, 5f, modelText, signoff, report, candidate: brake,
                driver: driver).SpeedProfile.Value.Issue, Is.EqualTo(SpeedProfileIssue.None));
            // Sans profil de conduite ni bornes, les bornes par defaut sont invalides : le candidat est refuse.
            Assert.That(Decide(frame, id, 5f, modelText, signoff, report, candidate: brake)
                .SpeedProfile.Value.Issue, Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
        }

        [Test]
        public void InvalidTrackingToleranceIsNotCoveredEvenWithValidEvidence()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var id = new RoadId(0x530, 46);
            var frame = EntryFrame(model, 0, 1, id, 0f);
            foreach (var invalid in new[] { float.NaN, -1f, float.PositiveInfinity })
            {
                var decision = Decide(frame, id, 10f, modelText, signoff, report,
                    tracking: new TrackingTolerance(invalid));
                Assert.That(decision.Motion.Evidence.Status, Is.EqualTo(GateAEvidenceStatus.Valid));
                Assert.That(decision.Motion.ReferenceCoverage, Is.EqualTo(ReferenceCoverage.NotCoveredByGateA));
                Assert.That(decision.Motion.Issue, Is.EqualTo(MotionIssue.NotCoveredByGateA), invalid.ToString());
            }
        }

        [Test]
        public void TruncatedAndCompleteHorizonsAreQualified()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var id = new RoadId(0x530, 47);
            var frame = EntryFrame(model, 0, 1, id, 0f);
            var truncated = Decide(frame, id, 10f, modelText, signoff, report);
            Assert.That(truncated.Path.End, Is.EqualTo(HorizonEnd.LookAheadLimit));
            Assert.That(truncated.Path.LengthMeters, Is.EqualTo(10f).Within(1e-3f));
            Assert.That(truncated.Motion.Diagnostics & MotionDiagnostic.HorizonTruncated,
                Is.EqualTo(MotionDiagnostic.HorizonTruncated));
            var complete = Decide(frame, id, 10000f, modelText, signoff, report);
            Assert.That(complete.Path.End, Is.EqualTo(HorizonEnd.ExitPortal));
            Assert.That(complete.Path.Intervals[complete.Path.Intervals.Count - 1].Id,
                Is.EqualTo(complete.Route.Plan.Occurrences[complete.Route.Plan.Occurrences.Count - 1].Id));
            Assert.That(complete.Motion.Diagnostics & MotionDiagnostic.HorizonTruncated, Is.EqualTo(MotionDiagnostic.None));
        }

        [Test]
        public void HorizonDefectsAreNamedAndReachTheDecision()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            int entryIndex;
            ulong seed;
            Assert.That(FindRingRoute(model, modelText, signoff, report, out entryIndex, out seed), Is.True);
            var id = new RoadId(0x530, 48);
            var ok = Decide(EntryFrame(model, entryIndex, 1, id, 0f), id, 10000f, modelText, signoff, report, seed);
            Assert.That(ok.Path.Issue, Is.EqualTo(PathIssue.None));

            // Un raccord d'anneau hors du jeu signe (carrefour qui n'est plus un giratoire) est refuse.
            var unsigned = Mutate(modelText, UnsignRoundabouts);
            AssertNonConforming(Decide(EntryFrame(unsigned, entryIndex, 2, id, 0f), id, 10000f,
                modelText, signoff, report, seed), PathIssue.SeamCurvature);
        }

        [Test]
        public void GeometryDefectsAreRefusedByTheValidatorBeforeAnyHorizonExists()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var id = new RoadId(0x530, 49);
            var firstId = Decide(EntryFrame(model, 0, 1, id, 0f), id, 10000f, modelText, signoff, report)
                .Path.Intervals[0].Id;
            // Le validateur applique ses propres regles (C1 reference, C5 tangente/courbure, tolerances de
            // raccord) : un ecart de position, un saut de courbure ou une pente de courbure incoherents avec
            // la geometrie n'aboutissent jamais a un modele compile, donc jamais a PathHorizon. Les branches
            // SeamGap, SeamTangent, CurvatureSlope et MissingElement de PathHorizon sont une seconde ligne de
            // defense, inatteignable avec un modele compile valide (MissingElement : le plan porte la version
            // du modele et RoutePlan n'a pas de constructeur public).
            Assert.That(() => Mutate(modelText, source => EditSamples(source, firstId,
                samples => samples[samples.Length - 1].Position += samples[samples.Length - 1].Tangent * .3f)),
                Throws.TypeOf<RoadModelCompilationException>());
            Assert.That(() => Mutate(modelText, source => EditSamples(source, firstId,
                samples => samples[samples.Length - 1].CurvaturePerMeter += .005f)),
                Throws.TypeOf<RoadModelCompilationException>());
            Assert.That(() => Mutate(modelText, source => EditSamples(source, firstId,
                samples => samples[samples.Length / 2].CurvaturePerMeter += 10f)),
                Throws.TypeOf<RoadModelCompilationException>());
        }

        [Test]
        public void InfeasiblePlanNeverAdmitsACandidateAndKeepsItsCause()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var id = new RoadId(0x530, 50);
            var frame = EntryFrame(model, 0, 1, id, 0f);
            var valid = Decide(frame, id, 200f, modelText, signoff, report);
            float length = valid.Path.LengthMeters;
            var candidate = new[] { new SpeedProfilePoint(0f, 0f), new SpeedProfilePoint(length, 0f) };
            var bounds = new LongitudinalBounds(2f, 3f);
            Assert.That(Decide(frame, id, 200f, modelText, signoff, report, bounds: bounds, candidate: candidate)
                .SpeedProfile.Value.Issue, Is.EqualTo(SpeedProfileIssue.None));

            var cases = new[] {
                new { Decision = Decide(frame, id, 200f, modelText, "", report, bounds: bounds, candidate: candidate),
                    Expected = MotionIssue.GateAEvidenceMissing },
                new { Decision = Decide(frame, id, 200f, modelText + " ", signoff, report, bounds: bounds, candidate: candidate),
                    Expected = MotionIssue.GateAEvidenceStale },
                new { Decision = Decide(frame, id, 200f, modelText, signoff, report,
                    tracking: new TrackingTolerance(.01f), bounds: bounds, candidate: candidate),
                    Expected = MotionIssue.NotCoveredByGateA }
            };
            foreach (var item in cases)
            {
                var result = item.Decision.SpeedProfile.Value;
                Assert.That(result.Issue, Is.EqualTo(SpeedProfileIssue.PlanInfeasible), item.Expected.ToString());
                Assert.That(result.PlanIssue, Is.EqualTo(item.Expected));
                Assert.That(result.DistanceMeters, Is.EqualTo(item.Decision.Motion.IssueDistanceMeters));
                Assert.That(item.Decision.Projection.Code, Is.EqualTo(item.Expected.ToString()));
            }

            var strict = model.DrivabilityProfile;
            strict.SteeringInactiveBelowMetersPerSecond = 100f;
            var invalidCeiling = new MotionPlan(valid.Path, strict, valid.Motion.Evidence, TrackingTolerance.Undeclared);
            var refused = invalidCeiling.VerifySpeedProfile(candidate, bounds);
            Assert.That(refused.Issue, Is.EqualTo(SpeedProfileIssue.PlanInfeasible));
            Assert.That(refused.PlanIssue, Is.EqualTo(MotionIssue.InvalidSteeringCeiling));
            Assert.That(refused.DistanceMeters, Is.EqualTo(invalidCeiling.IssueDistanceMeters));

            int entryIndex;
            ulong seed;
            Assert.That(FindRingRoute(model, modelText, signoff, report, out entryIndex, out seed), Is.True);
            var unsigned = Mutate(modelText, UnsignRoundabouts);
            var defect = Decide(EntryFrame(unsigned, entryIndex, 2, id, 0f), id, 10000f, modelText, signoff, report,
                seed, bounds: bounds, candidate: new[] { new SpeedProfilePoint(0f, 0f), new SpeedProfilePoint(1f, 0f) });
            Assert.That(defect.SpeedProfile.Value.Issue, Is.EqualTo(SpeedProfileIssue.PlanInfeasible));
            Assert.That(defect.SpeedProfile.Value.PlanIssue, Is.EqualTo(MotionIssue.HorizonNonConforming));
        }

        [Test]
        public void CandidateMustCoverTheWholeHorizon()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            int entry = StraightEntry(model, modelText, signoff, report);
            var id = new RoadId(0x530, 51);
            var decision = Decide(EntryFrame(model, entry, 1, id, 0f), id, 5f, modelText, signoff, report);
            var motion = decision.Motion;
            float length = motion.Path.LengthMeters;
            var bounds = new LongitudinalBounds(2f, 3f);
            Assert.That(motion.Path.End, Is.EqualTo(HorizonEnd.LookAheadLimit));

            var full = motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 3f),
                new SpeedProfilePoint(length, 3f) }, bounds);
            Assert.That(full.Issue, Is.EqualTo(SpeedProfileIssue.None));
            Assert.That(full.Diagnostics & MotionDiagnostic.HorizonTruncated, Is.EqualTo(MotionDiagnostic.HorizonTruncated));
            var late = motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(1f, 3f),
                new SpeedProfilePoint(length, 3f) }, bounds);
            Assert.That(late.Issue, Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
            Assert.That(late.DistanceMeters, Is.Zero);
            var early = motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 3f),
                new SpeedProfilePoint(length - 1f, 3f) }, bounds);
            Assert.That(early.Issue, Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
            Assert.That(early.DistanceMeters, Is.EqualTo(length - 1f));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 3f),
                new SpeedProfilePoint(length - PlanningTolerances.ProfileSpanToleranceMeters * .5f, 3f) },
                bounds).Issue, Is.EqualTo(SpeedProfileIssue.None));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 3f),
                new SpeedProfilePoint(length + PlanningTolerances.ProfileSpanToleranceMeters * 2f, 3f) },
                bounds).Issue, Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));

            // Sur une ligne droite a vitesse elevee, le plafond est « aucun » et le profil est admis.
            Assert.That(decision.Path.Intervals.SelectMany(x => x.Points).All(x => x.Unbounded), Is.True);
            Assert.That(decision.Projection.SteeringCeilingText, Is.EqualTo("aucun"));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 50f),
                new SpeedProfilePoint(length, 50f) }, new LongitudinalBounds(2f, 3f)).Issue,
                Is.EqualTo(SpeedProfileIssue.None));

            var complete = Decide(EntryFrame(model, 0, 2, id, 0f), id, 10000f, modelText, signoff, report);
            Assert.That(complete.Path.End, Is.EqualTo(HorizonEnd.ExitPortal));
            float total = complete.Path.LengthMeters;
            Assert.That(complete.Motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(total - 1f, 0f) }, bounds).Issue, Is.EqualTo(SpeedProfileIssue.InvalidSpeedProfile));
            Assert.That(complete.Motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(total, 0f) }, bounds).Issue, Is.EqualTo(SpeedProfileIssue.None));
        }

        [Test]
        public void AccelerationToleranceAdmitsRoundingAndRefusesRealOvershoot()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            int entry = StraightEntry(model, modelText, signoff, report);
            var id = new RoadId(0x530, 52);
            var motion = Decide(EntryFrame(model, entry, 1, id, 0f), id, 5f, modelText, signoff, report).Motion;
            float length = motion.Path.LengthMeters;
            const float bound = 2f;
            double tolerance = PlanningTolerances.AccelerationBoundToleranceMetersPerSecondSquared;
            var bounds = new LongitudinalBounds(bound, bound);

            // Borne exacte : v^2 = 4, ds = 1 -> a = -2 sans arrondi.
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 2f),
                new SpeedProfilePoint(1f, 0f), new SpeedProfilePoint(length, 0f) }, bounds).Issue,
                Is.EqualTo(SpeedProfileIssue.None));

            float exact = (float)Math.Sqrt(2d * bound * length);
            float rounded = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(exact) + 2);
            double excess = (double)rounded * rounded / (2d * length) - bound;
            Assert.That(excess, Is.GreaterThan(0d).And.LessThan(tolerance), "l'erreur d'arrondi simulee est reelle et minuscule");
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                new SpeedProfilePoint(length, rounded) }, bounds).Issue, Is.EqualTo(SpeedProfileIssue.None));
            Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, rounded),
                new SpeedProfilePoint(length, 0f) }, bounds).Issue, Is.EqualTo(SpeedProfileIssue.None));

            foreach (double overshoot in new[] { 2d * tolerance, 1e-2, 1d })
            {
                float fast = (float)Math.Sqrt(2d * (bound + overshoot) * length);
                var accelerating = motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, 0f),
                    new SpeedProfilePoint(length, fast) }, bounds);
                Assert.That(accelerating.Issue, Is.EqualTo(SpeedProfileIssue.AccelerationBoundExceeded), overshoot.ToString());
                Assert.That(accelerating.DistanceMeters, Is.EqualTo(length));
                Assert.That(motion.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, fast),
                    new SpeedProfilePoint(length, 0f) }, bounds).Issue,
                    Is.EqualTo(SpeedProfileIssue.AccelerationBoundExceeded), overshoot.ToString());
            }
        }

        [Test]
        public void SteeringCeilingIsCheckedBetweenCoarsePointsAndAtRingSeams()
        {
            string modelText, signoff, report;
            var model = Load(out modelText, out signoff, out report);
            var big = new LongitudinalBounds(1e6f, 1e6f);

            MotionPlan bend;
            float floor;
            Assert.That(FindBend(model, modelText, signoff, report, out bend, out floor), Is.True, "virage");
            float length = bend.Path.LengthMeters;
            var below = bend.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, floor * .95f),
                new SpeedProfilePoint(length, floor * .95f) }, big);
            Assert.That(below.Issue, Is.EqualTo(SpeedProfileIssue.None));
            // Aux deux extremites du profil le plafond depasse la vitesse : le depassement est entre eux.
            var above = bend.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, floor * 1.05f),
                new SpeedProfilePoint(length, floor * 1.05f) }, big);
            Assert.That(above.Issue, Is.EqualTo(SpeedProfileIssue.SteeringCeilingExceeded));
            Assert.That(above.DistanceMeters, Is.GreaterThan(0f).And.LessThan(length));

            MotionPlan ring;
            int seamIndex;
            Assert.That(FindRingEntrySeam(model, modelText, signoff, report, out ring, out seamIndex), Is.True, "raccord d'anneau");
            var points = ring.Path.Intervals[seamIndex + 1].Points;
            float admit = points.Min(x => x.SteeringCeilingMetersPerSecond);
            float seam = ring.Path.Seams[seamIndex].DistanceMeters;
            float horizon = ring.Path.LengthMeters;
            Assert.That(ring.Path.Seams[seamIndex].MinimumCeilingMetersPerSecond, Is.EqualTo(
                Math.Min(ring.Path.Seams[seamIndex].LeftCeilingMetersPerSecond,
                    ring.Path.Seams[seamIndex].RightCeilingMetersPerSecond)));
            // Egal au plafond a l'echantillon du raccord : admis. Au-dessus du v* du cote fini : refuse au raccord.
            Assert.That(ring.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, .2f),
                new SpeedProfilePoint(seam - RingApproachMeters, .2f), new SpeedProfilePoint(seam, admit),
                new SpeedProfilePoint(horizon, .2f) }, big).Issue, Is.EqualTo(SpeedProfileIssue.None));
            var refused = ring.VerifySpeedProfile(new[] { new SpeedProfilePoint(0f, .2f),
                new SpeedProfilePoint(seam - RingApproachMeters, .2f), new SpeedProfilePoint(seam, admit * 1.05f),
                new SpeedProfilePoint(horizon, .2f) }, big);
            Assert.That(refused.Issue, Is.EqualTo(SpeedProfileIssue.SteeringCeilingExceeded));
            Assert.That(refused.DistanceMeters, Is.EqualTo(seam).Within(1e-3f));
        }

        // ------------------------------------------------------------------ aides

        // Distance avant le raccord d'anneau sur laquelle le plafond est non borne : le profil de test y monte a v.
        private const float RingApproachMeters = .3f;

        private static readonly RoadRage.Features.Vehicles.DriverProfile NoDriver =
            default(RoadRage.Features.Vehicles.DriverProfile);

        private static TrafficFrame EntryFrame(CompiledRoadModel model, int entryIndex, ulong frameId,
            RoadId trafficId, float speed)
        {
            var entry = model.Portals.Where(p => p.Role == PortalRole.Entry).ElementAt(entryIndex);
            EffectiveLaneCorridor corridor;
            Assert.That(model.TryGetCorridor(entry.CorridorId, out corridor), Is.True);
            return new TrafficFrame(frameId, model, new[] { new TrafficActorInput(trafficId,
                Pose(corridor.Curve, entry.SMeters), speed, entry.CorridorId) });
        }

        private static PlanningDecision Decide(TrafficFrame frame, RoadId trafficId, float lookAhead,
            string modelText, string signoff, string report, ulong seed = 1, RoutePlan existing = null,
            TrackingTolerance tracking = default(TrackingTolerance), LongitudinalBounds? bounds = null,
            IReadOnlyList<SpeedProfilePoint> candidate = null,
            RoadRage.Features.Vehicles.DriverProfile driver = default(RoadRage.Features.Vehicles.DriverProfile))
        {
            return PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, existing, RoadId.None,
                new RouteSeed(seed), lookAhead, modelText, signoff, report, driver, tracking, bounds, candidate));
        }

        private static CompiledRoadModel Mutate(string modelText, Action<RoadModelSource> mutation)
        {
            var source = RoadModelDocument.Load(modelText);
            mutation(source);
            return RoadModelCompiler.Compile(source);
        }

        private static void UnsignRoundabouts(RoadModelSource source)
        {
            for (int i = 0; i < source.Junctions.Length; i++)
                if (source.Junctions[i].Feature == JunctionFeature.Roundabout)
                {
                    var junction = source.Junctions[i];
                    junction.Feature = JunctionFeature.Unspecified;
                    source.Junctions[i] = junction;
                }
        }

        private static void EditSamples(RoadModelSource source, RoadId corridorId, Action<RoadCurveSample[]> edit)
        {
            for (int i = 0; i < source.Corridors.Length; i++)
                if (source.Corridors[i].Id == corridorId)
                {
                    var corridor = source.Corridors[i];
                    var samples = (RoadCurveSample[])corridor.Samples.Clone();
                    edit(samples);
                    corridor.Samples = samples;
                    source.Corridors[i] = corridor;
                    return;
                }
            Assert.Fail("corridor introuvable : " + corridorId);
        }

        private static void AssertNonConforming(PlanningDecision decision, PathIssue issue)
        {
            Assert.That(decision.Path.Issue, Is.EqualTo(issue));
            Assert.That(decision.Path.IssueDistanceMeters, Is.InRange(0f, decision.Path.LengthMeters));
            Assert.That(decision.Motion.Issue, Is.EqualTo(MotionIssue.HorizonNonConforming));
            Assert.That(decision.Motion.GeometricallyFeasible, Is.False);
            Assert.That(decision.Motion.IssueDistanceMeters, Is.EqualTo(decision.Path.IssueDistanceMeters));
            Assert.That(decision.Projection.Code, Is.EqualTo(issue.ToString()));
        }

        private static bool FindRingRoute(CompiledRoadModel model, string modelText, string signoff, string report,
            out int entryIndex, out ulong seed)
        {
            var id = new RoadId(0x530, 100);
            for (int e = 0; e < 4; e++)
                for (ulong s = 1; s <= 12; s++)
                {
                    var decision = Decide(EntryFrame(model, e, 1, id, 0f), id, 10000f, modelText, signoff, report, s);
                    if (decision.Path != null && decision.Path.Seams.Any(x => x.RoundaboutDiscontinuity))
                    { entryIndex = e; seed = s; return true; }
                }
            entryIndex = 0; seed = 1;
            return false;
        }

        private static int StraightEntry(CompiledRoadModel model, string modelText, string signoff, string report)
        {
            var id = new RoadId(0x530, 101);
            for (int e = 0; e < 4; e++)
            {
                var path = Decide(EntryFrame(model, e, 1, id, 0f), id, 5f, modelText, signoff, report).Path;
                if (path.Intervals.SelectMany(x => x.Points).All(x => x.Unbounded)) return e;
            }
            Assert.Fail("aucune entree ne commence par 5 m de ligne droite");
            return -1;
        }

        private static bool FindBend(CompiledRoadModel model, string modelText, string signoff, string report,
            out MotionPlan bend, out float floor)
        {
            var id = new RoadId(0x530, 102);
            var evidence = GateAEvidenceBinding.Bind(model, modelText, signoff, report);
            for (int e = 0; e < 4; e++)
                for (ulong s = 1; s <= 8; s++)
                {
                    var plan = Decide(EntryFrame(model, e, 1, id, 0f), id, 10000f, modelText, signoff, report, s).Route.Plan;
                    var whole = PathHorizon.Build(model, plan, 10000f);
                    for (int k = 1; k + 1 < whole.Intervals.Count; k++)
                    {
                        if (whole.Intervals[k].Kind != RoadElementKind.JunctionMovement
                            || whole.Seams[k - 1].RoundaboutDiscontinuity || whole.Seams[k].RoundaboutDiscontinuity)
                            continue;
                        var path = PathHorizon.Build(model, plan, whole.Intervals[k].EndDistanceMeters + 2f);
                        var motion = new MotionPlan(path, model.DrivabilityProfile, evidence, TrackingTolerance.Undeclared);
                        var points = path.Intervals.SelectMany(x => x.Points).ToList();
                        float lowest = points.Min(x => x.SteeringCeilingMetersPerSecond);
                        if (motion.Issue == MotionIssue.None && !float.IsPositiveInfinity(lowest)
                            && points[0].SteeringCeilingMetersPerSecond > lowest * 1.06f
                            && points[points.Count - 1].SteeringCeilingMetersPerSecond > lowest * 1.06f)
                        { bend = motion; floor = lowest; return true; }
                    }
                }
            bend = null; floor = 0f;
            return false;
        }

        private static bool FindRingEntrySeam(CompiledRoadModel model, string modelText, string signoff, string report,
            out MotionPlan ring, out int seamIndex)
        {
            var id = new RoadId(0x530, 103);
            var evidence = GateAEvidenceBinding.Bind(model, modelText, signoff, report);
            for (int e = 0; e < 4; e++)
                for (ulong s = 1; s <= 12; s++)
                {
                    var plan = Decide(EntryFrame(model, e, 1, id, 0f), id, 10000f, modelText, signoff, report, s).Route.Plan;
                    var whole = PathHorizon.Build(model, plan, 10000f);
                    for (int j = 0; j < whole.Seams.Count; j++)
                    {
                        var seam = whole.Seams[j];
                        if (!seam.RoundaboutDiscontinuity || !float.IsPositiveInfinity(seam.LeftCeilingMetersPerSecond)
                            || float.IsPositiveInfinity(seam.RightCeilingMetersPerSecond)) continue;
                        var path = PathHorizon.Build(model, plan, seam.DistanceMeters + .05f);
                        var motion = new MotionPlan(path, model.DrivabilityProfile, evidence, TrackingTolerance.Undeclared);
                        var left = path.Intervals[j].Points;
                        if (motion.Issue == MotionIssue.None && path.Intervals.Count == j + 2
                            && path.Intervals[j + 1].Points.Count >= 2
                            && seam.DistanceMeters > RingApproachMeters
                            && left.Where(x => x.DistanceMeters >= seam.DistanceMeters - RingApproachMeters).All(x => x.Unbounded)
                            && path.Intervals[j + 1].Points[1].SteeringCeilingMetersPerSecond >= .25f)
                        { ring = motion; seamIndex = j; return true; }
                    }
                }
            ring = null; seamIndex = -1;
            return false;
        }
    }
}
