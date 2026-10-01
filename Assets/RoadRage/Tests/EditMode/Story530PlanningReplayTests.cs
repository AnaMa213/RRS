using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;

namespace RoadRage.Tests.EditMode
{
    [Category("Geometry")]
    public sealed class Story530PlanningReplayTests
    {
        private const string HistoricalDirectory = "_bmad-output/implementation-artifacts/gate-a-5-52/historical-signed-5-51/";
        private static readonly string[] ExpectedRingDiscontinuities = {
            "401b55e11b401435eb1bdd8dde7caa94:entry", "4030253e182e3ed1b7d2aeea7a73feb6:entry",
            "419d893b269e14c02e84e3509d9bf193:entry", "42480748339bbb6fe6fcbc99604c1aa8:exit",
            "4357c472225591a18683e67ea2dd5f92:exit", "43605e569eb08d6ffe62fa7470d59fa0:exit",
            "4439e11d9c47c1d09aad97b8f5dd1cbe:exit", "4469169721b83714f20e63d9fcfff484:exit",
            "452ee31e83feea5ebc05406c271424a9:exit", "453f130c460dc35e052c30714bec6c8e:entry",
            "45607ec286d32b63e09ca677f22031ba:entry", "45d560a7a864362a2f19600802713fac:entry",
            "46077471fe6db9c5bfc3327df0b647af:entry", "464127b42987ee35c9def93cb72dae8c:exit",
            "470e78565e75b89add119d1f7bf3d8b3:exit", "4a5a12c19e62b4f853f928a3d4fb4c96:exit",
            "4a6aa7e11135c1ecb2cb26715ca8cab4:entry", "4a772fed8c8aaeab952d011659612ea7:entry",
            "4ac98ed2e41d83c91f0714135aa67ba7:entry", "4b517add680eba2b77f4e15e9033e180:entry",
            "4cec0461fb9fd74541d5772b2264b88f:exit", "4d6ca77eae0d45e72a542a1478a2aa84:exit",
            "4edce9aa0d470704d0479247d72ff2be:entry", "4f47e1a8140c798681fef66fa633b3b5:exit"
        };

        private static UnityEngine.Vector3 Position(RoadCurve curve, float s) { return curve.Sample(s).Position; }

        private static VehicleFootprintPose Pose(RoadCurve curve, float s)
        {
            var sample = curve.Sample(s);
            return new VehicleFootprintPose { Position = sample.Position, Forward = sample.Tangent, Up = sample.Up };
        }

        [Test]
        public void FourEntriesReplayAcrossCompiledHorizonsToExit()
        {
            string modelText = File.ReadAllText(HistoricalDirectory + "MVP_Run.road-model.json");
            string signoff = File.ReadAllText(HistoricalDirectory + "MVP_Run.road-signoff.json");
            string report = File.ReadAllText(HistoricalDirectory + "migration-report-5-28-mvp-run.md");
            var model = RoadModelCompiler.Compile(RoadModelDocument.Load(modelText));
            int entries = 0;
            var observedRing = new HashSet<string>();
            foreach (var entry in model.Portals.Where(p => p.Role == PortalRole.Entry))
            {
                entries++;
                var trafficId = new RoadId(0x530, (ulong)entries);
                EffectiveLaneCorridor start;
                Assert.That(model.TryGetCorridor(entry.CorridorId, out start), Is.True);
                var firstFrame = new TrafficFrame(1, model, new[] {
                    new TrafficActorInput(trafficId, Pose(start.Curve, entry.SMeters), 0f, entry.CorridorId) });
                var decision = PlanningSpine.Evaluate(new PlanningRequest(firstFrame, trafficId, null, RoadId.None,
                    new RouteSeed(7), 10000f, modelText, signoff, report,
                    default(RoadRage.Features.Vehicles.DriverProfile), bounds: new LongitudinalBounds(2f, 3f)));
                Assert.That(decision.Route.Plan, Is.Not.Null, entry.Id.ToString());
                var initial = decision.Route.Plan;
                for (int i = 0; i < initial.Occurrences.Count; i++)
                {
                    var occurrence = initial.Occurrences[i];
                    RoadCurve curve;
                    EffectiveLaneCorridor corridor;
                    CompiledJunctionMovement movement;
                    if (occurrence.Kind == RoadElementKind.LaneCorridor)
                    { Assert.That(model.TryGetCorridor(occurrence.Id, out corridor), Is.True); curve = corridor.Curve; }
                    else
                    { Assert.That(model.TryGetMovement(occurrence.Id, out movement), Is.True); curve = movement.Curve; }
                    float s = (occurrence.StartSMeters + occurrence.EndSMeters) * .5f;
                    var frame = new TrafficFrame((ulong)(i + 2), model, new[] {
                        new TrafficActorInput(trafficId, Pose(curve, s), 0f, occurrence.Id) });
                    decision = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, decision.Route.Plan,
                        RoadId.None, new RouteSeed(7), 10000f, modelText, signoff, report,
                        default(RoadRage.Features.Vehicles.DriverProfile), bounds: new LongitudinalBounds(2f, 3f)));
                    Assert.That(decision.Route.Plan, Is.Not.Null, occurrence.Id.ToString());
                    // Reutilisation : meme sortie, memes occurrences, progression sur l'occurrence courante.
                    var reused = decision.Route.Plan;
                    Assert.That(decision.Route.Outcome, Is.EqualTo(RouteOutcome.Planned), occurrence.Id.ToString());
                    Assert.That(decision.Route.Reason, Is.EqualTo(RouteReason.Requested), occurrence.Id.ToString());
                    Assert.That(reused.ExitPortalId, Is.EqualTo(initial.ExitPortalId));
                    Assert.That(reused.Occurrences.Count, Is.EqualTo(initial.Occurrences.Count));
                    Assert.That(reused.ProgressOccurrenceIndex, Is.LessThanOrEqualTo(i));
                    Assert.That(reused.Occurrences[reused.ProgressOccurrenceIndex].Id, Is.EqualTo(occurrence.Id));
                    Assert.That(decision.Path.Issue, Is.EqualTo(PathIssue.None), occurrence.Id.ToString());
                    // L'horizon atteint le portail de sortie, par les occurrences restantes et rien d'autre.
                    Assert.That(decision.Path.End, Is.EqualTo(HorizonEnd.ExitPortal), occurrence.Id.ToString());
                    Assert.That(decision.Path.Intervals.Count,
                        Is.EqualTo(reused.Occurrences.Count - reused.ProgressOccurrenceIndex));
                    Assert.That(decision.Path.Intervals[decision.Path.Intervals.Count - 1].Id,
                        Is.EqualTo(reused.Occurrences[reused.Occurrences.Count - 1].Id));
                    float remaining = reused.Occurrences[reused.ProgressOccurrenceIndex].EndSMeters - reused.ProgressSMeters;
                    for (int r = reused.ProgressOccurrenceIndex + 1; r < reused.Occurrences.Count; r++)
                        remaining += reused.Occurrences[r].EndSMeters - reused.Occurrences[r].StartSMeters;
                    Assert.That(decision.Path.LengthMeters, Is.EqualTo(remaining).Within(.01f), occurrence.Id.ToString());
                    Assert.That(decision.Motion.ReferenceCoverage, Is.EqualTo(ReferenceCoverage.Covered));
                    Assert.That(decision.Motion.VehicleCoverage, Is.EqualTo(VehicleCoverage.NotEstablished));
                    foreach (var interval in decision.Path.Intervals)
                    {
                        Assert.That(interval.MaximumAbsoluteCurvatureSlopePerSquareMeter,
                            Is.LessThanOrEqualTo(PlanningTolerances.MaximumCurvatureSlopePerSquareMeter));
                        foreach (var point in interval.Points)
                            Assert.That(point.SteeringCeilingMetersPerSecond, Is.GreaterThanOrEqualTo(
                                model.DrivabilityProfile.SteeringInactiveBelowMetersPerSecond));
                    }
                    for (int j = 0; j < decision.Path.Seams.Count; j++)
                    {
                        var seam = decision.Path.Seams[j];
                        Assert.That(seam.GapMeters, Is.LessThanOrEqualTo(model.ValidationProfile.SeamGapToleranceMeters));
                        Assert.That(seam.TangentJumpDegrees,
                            Is.LessThanOrEqualTo(model.ValidationProfile.SeamTangentToleranceDegrees));
                        bool jump = Math.Abs(seam.CurvatureJumpPerMeter) > PlanningTolerances.SeamCurvatureJumpPerMeter;
                        // Une discontinuite n'est publiee que sur un raccord de l'anneau signe, avec v* = min des deux cotes.
                        Assert.That(seam.RoundaboutDiscontinuity, Is.EqualTo(jump));
                        if (!jump) continue;
                        Assert.That(Math.Abs(seam.CurvatureJumpPerMeter), Is.EqualTo(
                            PlanningTolerances.RoundaboutCurvaturePerMeter).Within(PlanningTolerances.SeamCurvatureJumpPerMeter));
                        Assert.That(seam.MinimumCeilingMetersPerSecond, Is.EqualTo(
                            Math.Min(seam.LeftCeilingMetersPerSecond, seam.RightCeilingMetersPerSecond)));
                        var left = decision.Path.Intervals[j];
                        var right = decision.Path.Intervals[j + 1];
                        observedRing.Add(left.Kind == RoadElementKind.JunctionMovement
                            ? left.Id + ":exit" : right.Id + ":entry");
                    }
                }
                Assert.That(decision.Route.Plan.ExitPortalId, Is.Not.EqualTo(RoadId.None));
            }
            Assert.That(entries, Is.EqualTo(4));
            Assert.That(observedRing, Is.Not.Empty);
            Assert.That(observedRing.IsSubsetOf(ExpectedRingDiscontinuities), Is.True);
        }

        [Test]
        public void SignedRingSeamSetIsPinnedByMovementAndSide()
        {
            var model = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(
                HistoricalDirectory + "MVP_Run.road-model.json")));
            var actual = new List<string>();
            foreach (var movement in model.Movements)
            {
                EffectiveLaneCorridor from, to;
                model.TryGetCorridor(movement.FromCorridorId, out from);
                model.TryGetCorridor(movement.ToCorridorId, out to);
                float entry = movement.Curve.Sample(0f).CurvaturePerMeter
                    - from.Curve.Sample(from.LengthMeters).CurvaturePerMeter;
                float exit = to.Curve.Sample(0f).CurvaturePerMeter
                    - movement.Curve.Sample(movement.LengthMeters).CurvaturePerMeter;
                if (Math.Abs(entry) > PlanningTolerances.SeamCurvatureJumpPerMeter)
                    actual.Add(movement.Id + ":entry");
                if (Math.Abs(exit) > PlanningTolerances.SeamCurvatureJumpPerMeter)
                    actual.Add(movement.Id + ":exit");
            }
            Assert.That(actual.OrderBy(x => x), Is.EqualTo(ExpectedRingDiscontinuities.OrderBy(x => x)));
        }
    }
}
