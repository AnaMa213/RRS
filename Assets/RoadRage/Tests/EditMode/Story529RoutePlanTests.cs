using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    [Category("Core")]
    public sealed class Story529RoutePlanTests
    {
        private static RoadId Id(int n) { return new RoadId(0x529UL, (ulong)n); }

        private static RoadModelSource Source(float[] weights, bool[] exits = null)
        {
            var source = new RoadModelSource();
            source.ModelId = Id(1);
            source.ValidationProfile = new RoadModelValidationProfile {
                MaxVehicleHalfWidthMeters = 1f, MaxVehicleLengthMeters = 4f,
                LateralClearanceMarginMeters = .25f, SeamGapToleranceMeters = .05f,
                SeamTangentToleranceDegrees = 5f, LengthToleranceMeters = .05f,
                EnvelopeOverlapToleranceMeters = .05f, GroundingMaxOffAxisDegrees = 45f };
            source.LocalizationProfile = new RoadLocalizationProfile {
                ScoreBandMeters = .15f, HysteresisMeters = .1f,
                AcceptanceDistanceMeters = 2.5f, WrongWayHeadingDegrees = 90f };
            source.Sections = new RoadSection[4];
            source.Corridors = new LaneCorridor[4];
            for (int i = 0; i < 4; i++)
            {
                source.Sections[i] = new RoadSection {
                    Id = Id(10 + i), RoadClass = RoadClass.Local, Surface = RoadSurface.Asphalt,
                    DefaultSpeedLimitMetersPerSecond = 10f, DefaultAllowedVehicleClasses = VehicleClassMask.Car };
                source.Corridors[i] = new LaneCorridor {
                    Id = Id(20 + i), SectionId = source.Sections[i].Id,
                    LateralOrder = 0, IsCrossSectionDatum = true,
                    Samples = Line(i == 0 ? 0f : 20f, i == 0 ? 10f : 30f), LengthMeters = 10f };
            }
            source.Junctions = new[] { new Junction {
                Id = Id(30), Feature = JunctionFeature.Crossroads,
                Boundary = new RoadBoundsBox { Center = new Vector3(0f, 0f, 15f), Extents = new Vector3(3f, 3f, 6f) } } };
            source.Movements = new JunctionMovement[3];
            for (int i = 0; i < 3; i++)
                source.Movements[i] = new JunctionMovement {
                    Id = Id(40 + i), JunctionId = Id(30), FromCorridorId = Id(20), ToCorridorId = Id(21 + i),
                    Samples = Line(10f, 20f), LengthMeters = 10f, RoutePreferenceWeight = weights[i] };
            source.Controls = new[] { new JunctionControl {
                Id = Id(50), JunctionId = Id(30), Kind = JunctionControlKind.Uncontrolled,
                ControlledMovementIds = new[] { Id(40), Id(41), Id(42) } } };
            if (exits == null) exits = new[] { true, true, true };
            var portals = new System.Collections.Generic.List<Portal>();
            for (int i = 0; i < 3; i++)
                if (exits[i]) portals.Add(new Portal {
                    Id = Id(60 + i), CorridorId = Id(21 + i), Role = PortalRole.Exit,
                    SMeters = 9.5f, EnvelopeLengthMeters = 1f, EnvelopeHalfWidthMeters = 1f });
            source.Portals = portals.ToArray();
            return source;
        }

        private static RoadCurveSample[] Line(float z0, float z1)
        {
            return new[] {
                Sample(0f, z0), Sample(z1 - z0, z1) };
        }

        private static RoadCurveSample Sample(float s, float z)
        {
            return new RoadCurveSample {
                SMeters = s, Position = new Vector3(0f, 0f, z), Tangent = Vector3.forward,
                Up = Vector3.up, HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f };
        }

        private static RoadLocation At(CompiledRoadModel model, RoadId element, float s = 0f,
            RoadElementKind kind = RoadElementKind.LaneCorridor)
        {
            return new RoadLocation {
                ModelId = model.ModelId, ModelVersion = model.Version, Localized = true,
                ElementKind = kind, ElementId = element, SMeters = s };
        }

        private static RouteResult Plan(CompiledRoadModel model, RoadId start, ulong seed = 1)
        {
            return Plan(model, At(model, start), RoadId.None, seed, Id(80), "route", 0);
        }

        private static RouteResult Plan(CompiledRoadModel model, RoadLocation location, RoadId destination,
            ulong seed, RoadId traffic, string domain, ulong counter,
            RoutePlan existing = null, bool replan = false, IReadOnlyCollection<RoadId> closedPortalIds = null)
        {
            return RoutePlanner.Plan(new RouteRequest(model, location, destination, new RouteSeed(seed), traffic,
                domain, new DecisionCounter(counter), existing, replan, closedPortalIds));
        }

        private static void AssertEquivalent(RoutePlan expected, RoutePlan actual)
        {
            Assert.That(actual.ExitPortalId, Is.EqualTo(expected.ExitPortalId));
            Assert.That(actual.Reason, Is.EqualTo(expected.Reason));
            Assert.That(actual.Occurrences.Count, Is.EqualTo(expected.Occurrences.Count));
            for (int i = 0; i < expected.Occurrences.Count; i++)
            {
                Assert.That(actual.Occurrences[i].Kind, Is.EqualTo(expected.Occurrences[i].Kind));
                Assert.That(actual.Occurrences[i].Id, Is.EqualTo(expected.Occurrences[i].Id));
                Assert.That(actual.Occurrences[i].StartSMeters, Is.EqualTo(expected.Occurrences[i].StartSMeters));
                Assert.That(actual.Occurrences[i].EndSMeters, Is.EqualTo(expected.Occurrences[i].EndSMeters));
            }
            Assert.That(actual.DistanceMeters, Is.EqualTo(expected.DistanceMeters));
            Assert.That(actual.PreferenceCost, Is.EqualTo(expected.PreferenceCost));
            Assert.That(actual.Diagnostics, Is.EqualTo(expected.Diagnostics));
        }

        private static RoadModelSource Loop(bool portalOnLoop)
        {
            var source = Source(new[] { 1f, 1f, 1f });
            source.Sections = new[] { source.Sections[0], source.Sections[1] };
            var samples = new RoadCurveSample[17];
            for (int i = 0; i < samples.Length; i++)
            {
                float theta = 2f * Mathf.PI * i / 16f;
                samples[i] = new RoadCurveSample {
                    SMeters = 10f * theta,
                    Position = new Vector3(10f * Mathf.Cos(theta), 0f, 10f * Mathf.Sin(theta)),
                    Tangent = new Vector3(-Mathf.Sin(theta), 0f, Mathf.Cos(theta)),
                    Up = Vector3.up, CurvaturePerMeter = .1f,
                    HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f };
            }
            var ring = source.Corridors[0];
            ring.Samples = samples;
            ring.LengthMeters = samples[16].SMeters;
            var isolated = source.Corridors[1];
            source.Corridors = new[] { ring, isolated };
            source.Connections = new[] { new LaneConnection {
                Id = Id(90), FromCorridorId = ring.Id, ToCorridorId = ring.Id,
                Kind = LaneConnectionKind.Continuation } };
            source.Junctions = new Junction[0];
            source.Movements = new JunctionMovement[0];
            source.Controls = new JunctionControl[0];
            source.Portals = portalOnLoop
                ? new[] { new Portal { Id = Id(70), CorridorId = ring.Id, Role = PortalRole.Exit,
                    SMeters = 5f, EnvelopeLengthMeters = 1f, EnvelopeHalfWidthMeters = 1f } }
                : new[] { new Portal { Id = Id(71), CorridorId = isolated.Id, Role = PortalRole.Exit,
                    SMeters = 5f, EnvelopeLengthMeters = 1f, EnvelopeHalfWidthMeters = 1f } };
            return source;
        }

        private static RoadModelSource PositiveCycleWithZeroExit()
        {
            var source = Source(new[] { 0f, 1f, 1f });
            var ring = Loop(true).Corridors[0];
            var otherRing = ring;
            otherRing.Id = Id(21);
            otherRing.SectionId = Id(11);
            var exitCorridor = source.Corridors[2];
            exitCorridor.Samples = Line(10f, 20f);
            for (int i = 0; i < exitCorridor.Samples.Length; i++)
                exitCorridor.Samples[i].Position.x = 10f;
            source.Sections = new[] { source.Sections[0], source.Sections[1], source.Sections[2] };
            source.Corridors = new[] { ring, otherRing, exitCorridor };
            source.Connections = new[] {
                new LaneConnection { Id = Id(90), FromCorridorId = ring.Id,
                    ToCorridorId = otherRing.Id, Kind = LaneConnectionKind.Continuation },
                new LaneConnection { Id = Id(91), FromCorridorId = otherRing.Id,
                    ToCorridorId = ring.Id, Kind = LaneConnectionKind.Continuation } };
            var movementSamples = Line(0f, 10f);
            for (int i = 0; i < movementSamples.Length; i++)
                movementSamples[i].Position.x = 10f;
            source.Junctions = new[] { new Junction { Id = Id(30), Feature = JunctionFeature.Crossroads,
                Boundary = new RoadBoundsBox { Center = new Vector3(10f, 0f, 5f),
                    Extents = new Vector3(3f, 3f, 6f) } } };
            source.Movements = new[] { new JunctionMovement { Id = Id(40), JunctionId = Id(30),
                FromCorridorId = ring.Id, ToCorridorId = exitCorridor.Id,
                Samples = movementSamples, LengthMeters = 10f, RoutePreferenceWeight = 0f } };
            source.Controls = new[] { new JunctionControl { Id = Id(50), JunctionId = Id(30),
                Kind = JunctionControlKind.Uncontrolled, ControlledMovementIds = new[] { Id(40) } } };
            source.Portals = new[] { new Portal { Id = Id(60), CorridorId = exitCorridor.Id,
                Role = PortalRole.Exit, SMeters = 9.5f, EnvelopeLengthMeters = 1f,
                EnvelopeHalfWidthMeters = 1f } };
            return source;
        }

        [Test]
        public void WeightedChoicesAreReproducibleAndRespectRatios()
        {
            var model = RoadModelCompiler.Compile(Source(new[] { 30f, 50f, 20f }));
            foreach (ulong identity in new[] { 80UL, 81UL })
            {
                var counts = new int[3];
                for (ulong seed = 0; seed < 4000; seed++)
                {
                    var result = Plan(model, At(model, Id(20)), RoadId.None, seed, Id((int)identity), "route", 0);
                    Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Planned));
                    Assert.That(result.Reason, Is.EqualTo(RouteReason.Requested));
                    Assert.That(result.Plan.TotalCost, Is.GreaterThan(0d));
                    Assert.That(double.IsInfinity(result.Plan.TotalCost), Is.False);
                    Assert.That(double.IsNaN(result.Plan.DistanceMeters) || double.IsInfinity(result.Plan.DistanceMeters), Is.False);
                    Assert.That(double.IsNaN(result.Plan.PreferenceCost) || double.IsInfinity(result.Plan.PreferenceCost), Is.False);
                    Assert.That(result.Plan.PreferenceCost, Is.GreaterThan(0d));
                    Assert.That(result.Plan.PreferenceCost, Is.LessThan(40d));
                    Assert.That(result.Plan.Diagnostics, Is.EqualTo(RouteDiagnostic.None));
                    int chosen = (int)result.Plan.ExitPortalId.Low - 60;
                    counts[chosen]++;
                    AssertEquivalent(result.Plan,
                        Plan(model, At(model, Id(20)), RoadId.None, seed, Id((int)identity), "route", 0).Plan);
                }
                Assert.That(counts[0], Is.InRange(1050, 1350));
                Assert.That(counts[1], Is.InRange(1800, 2200));
                Assert.That(counts[2], Is.InRange(650, 950));
            }

            var reordered = Source(new[] { 30f, 50f, 20f });
            Array.Reverse(reordered.Movements);
            Array.Reverse(reordered.Portals);
            var permuted = RoadModelCompiler.Compile(reordered);
            Assert.That(permuted.Version, Is.EqualTo(model.Version));
            for (ulong seed = 0; seed < 100; seed++)
                AssertEquivalent(Plan(model, Id(20), seed).Plan, Plan(permuted, Id(20), seed).Plan);

            var raised = RoadModelCompiler.Compile(Source(new[] { 60f, 50f, 20f }));
            var baseCounts = new int[3];
            var raisedCounts = new int[3];
            for (ulong seed = 0; seed < 4000; seed++)
            {
                var baseline = Plan(model, Id(20), seed).Plan;
                var boosted = Plan(raised, Id(20), seed).Plan;
                baseCounts[(int)baseline.ExitPortalId.Low - 60]++;
                raisedCounts[(int)boosted.ExitPortalId.Low - 60]++;
                if (baseline.ExitPortalId == Id(60))
                {
                    Assert.That(boosted.ExitPortalId, Is.EqualTo(Id(60)));
                    Assert.That(boosted.PreferenceCost, Is.LessThan(baseline.PreferenceCost));
                }
            }
            Assert.That(raisedCounts[0], Is.GreaterThan(baseCounts[0]));

            bool identityChangesDecision = false, domainChangesDecision = false, counterChangesDecision = false;
            for (ulong seed = 0; seed < 100; seed++)
            {
                var baseline = Plan(model, Id(20), seed).Plan.ExitPortalId;
                identityChangesDecision |= Plan(model, At(model, Id(20)), RoadId.None,
                    seed, Id(81), "route", 0).Plan.ExitPortalId != baseline;
                domainChangesDecision |= Plan(model, At(model, Id(20)), RoadId.None,
                    seed, Id(80), "other", 0).Plan.ExitPortalId != baseline;
                counterChangesDecision |= Plan(model, At(model, Id(20)), RoadId.None,
                    seed, Id(80), "route", 1).Plan.ExitPortalId != baseline;
            }
            Assert.That(identityChangesDecision, Is.True);
            Assert.That(domainChangesDecision, Is.True);
            Assert.That(counterChangesDecision, Is.True);
        }

        [Test]
        public void InfeasibleAndZeroWeightChoicesHaveExplicitBehavior()
        {
            var deadEnd = RoadModelCompiler.Compile(Source(new[] { 1f, 1f, 1000f }, new[] { true, true, false }));
            for (ulong seed = 0; seed < 100; seed++)
            {
                var result = Plan(deadEnd, Id(20), seed);
                Assert.That(result.Plan, Is.Not.Null);
                Assert.That(result.Plan.ExitPortalId, Is.EqualTo(Id(60)).Or.EqualTo(Id(61)));
            }

            var mixed = RoadModelCompiler.Compile(Source(new[] { 0f, 1f, 0f }));
            for (ulong seed = 0; seed < 100; seed++)
                Assert.That(Plan(mixed, Id(20), seed).Plan.ExitPortalId, Is.EqualTo(Id(61)));

            var zeros = RoadModelCompiler.Compile(Source(new[] { 0f, 0f, 0f }));
            var first = Plan(zeros, Id(20));
            Assert.That(first.Diagnostics, Is.EqualTo(RouteDiagnostic.ZeroWeightFallback));
            Assert.That(first.Plan.ExitPortalId, Is.EqualTo(Plan(zeros, Id(20)).Plan.ExitPortalId));
            var zeroCounts = new int[3];
            var shuffledZeros = Source(new[] { 0f, 0f, 0f });
            Array.Reverse(shuffledZeros.Movements);
            Array.Reverse(shuffledZeros.Portals);
            var permutedZeros = RoadModelCompiler.Compile(shuffledZeros);
            for (ulong seed = 0; seed < 2400; seed++)
            {
                var result = Plan(zeros, Id(20), seed);
                zeroCounts[(int)result.Plan.ExitPortalId.Low - 60]++;
                Assert.That(result.Diagnostics, Is.EqualTo(RouteDiagnostic.ZeroWeightFallback));
                Assert.That(Plan(permutedZeros, Id(20), seed).Plan.ExitPortalId,
                    Is.EqualTo(result.Plan.ExitPortalId));
            }
            foreach (int count in zeroCounts) Assert.That(count, Is.InRange(650, 950));

            var onlyZerosReach = RoadModelCompiler.Compile(Source(new[] { 0f, 0f, 1000f },
                new[] { true, true, false }));
            for (ulong seed = 0; seed < 100; seed++)
            {
                var result = Plan(onlyZerosReach, Id(20), seed);
                Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Planned));
                Assert.That(result.Diagnostics, Is.EqualTo(RouteDiagnostic.ZeroWeightFallback));
                Assert.That(result.Plan, Is.Not.Null);
                Assert.That(result.Plan.ExitPortalId, Is.EqualTo(Id(60)).Or.EqualTo(Id(61)));
            }
            var invalid = Source(new[] { -1f, 1f, 1f });
            var negative = Assert.Throws<RoadModelCompilationException>(() => RoadModelCompiler.Compile(invalid));
            Assert.That(negative.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True);
            bool negativeOnMovement = false;
            foreach (var issue in negative.Issues)
                negativeOnMovement |= issue.Code == RoadModelValidationCode.NumericValueOutOfRange
                    && issue.SubjectId == Id(40);
            Assert.That(negativeOnMovement, Is.True);

            var notFinite = Source(new[] { float.NaN, 1f, 1f });
            var notFiniteFailure = Assert.Throws<RoadModelCompilationException>(() => RoadModelCompiler.Compile(notFinite));
            Assert.That(notFiniteFailure.HasCode(RoadModelValidationCode.NonFiniteNumericValue), Is.True);
        }

        [Test]
        public void PortalPositionAndStaleInputsAreNamed()
        {
            var source = Source(new[] { 1f, 1f, 1f });
            source.Portals = new[] { new Portal {
                Id = Id(70), CorridorId = Id(20), Role = PortalRole.Exit,
                SMeters = 5f, EnvelopeLengthMeters = 1f, EnvelopeHalfWidthMeters = 1f } };
            var model = RoadModelCompiler.Compile(source);
            var ahead = Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0);
            Assert.That(ahead.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(ahead.Reason, Is.EqualTo(RouteReason.Requested));
            Assert.That(ahead.Plan.Occurrences.Count, Is.EqualTo(1));
            Assert.That(ahead.Plan.DistanceMeters, Is.EqualTo(1d));
            foreach (float s in new[] { 5f, 6f })
            {
                var result = Plan(model, At(model, Id(20), s), Id(70), 1, Id(80), "route", 0);
                Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.NoRoute));
                Assert.That(result.Reason, Is.EqualTo(RouteReason.DestinationUnreachable));
                Assert.That(result.Plan, Is.Null);
            }
            var stale = At(model, Id(20));
            stale.ModelVersion = default(RoadModelVersion);
            var staleResult = Plan(model, stale, Id(70), 1, Id(80), "route", 0);
            Assert.That(staleResult.Outcome, Is.EqualTo(RouteOutcome.InvalidInput));
            Assert.That(staleResult.Reason, Is.EqualTo(RouteReason.StaleLocalization));
            Assert.That(staleResult.Plan, Is.Null);
            var unknown = Plan(model, At(model, Id(99)), Id(70), 1, Id(80), "route", 0);
            Assert.That(unknown.Outcome, Is.EqualTo(RouteOutcome.InvalidInput));
            Assert.That(unknown.Reason, Is.EqualTo(RouteReason.InvalidStart));
            var notLocalized = At(model, Id(20));
            notLocalized.Localized = false;
            Assert.That(Plan(model, notLocalized, Id(70), 1, Id(80), "route", 0).Outcome,
                Is.EqualTo(RouteOutcome.InvalidInput));
            Assert.That(Plan(model, At(model, Id(20), 11f), Id(70), 1, Id(80), "route", 0).Reason,
                Is.EqualTo(RouteReason.InvalidStart));
            Assert.That(Plan(model, At(model, Id(20)), Id(99), 1, Id(80), "route", 0).Reason,
                Is.EqualTo(RouteReason.DestinationUnavailable));
            Assert.That(Plan(model, At(model, Id(20)), Id(70), 1, Id(80), "route", 0,
                closedPortalIds: new[] { Id(70) }).Reason,
                Is.EqualTo(RouteReason.DestinationUnavailable));
            var old = Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0).Plan;
            var replacement = Plan(model, At(model, Id(20), 3f), Id(70), 1, Id(80), "route", 0, old);
            Assert.That(replacement.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(replacement.Reason, Is.EqualTo(RouteReason.StalePlan));
            Assert.That(replacement.Plan.Occurrences[0].StartSMeters, Is.EqualTo(3f));
            var reused = Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0, old).Plan;
            Assert.That(reused, Is.SameAs(old));
            Assert.That(reused.Reason, Is.EqualTo(RouteReason.Requested));
            var explicitReplan = Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0, old, true);
            Assert.That(explicitReplan.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(explicitReplan.Reason, Is.EqualTo(RouteReason.Requested));

            var changedSource = Source(new[] { 2f, 1f, 1f });
            var changedModel = RoadModelCompiler.Compile(changedSource);
            var versionReplan = Plan(changedModel, At(changedModel, Id(20)), RoadId.None, 1, Id(80), "route", 0, old);
            Assert.That(versionReplan.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(versionReplan.Reason, Is.EqualTo(RouteReason.StalePlan));

            var movementModel = RoadModelCompiler.Compile(Source(new[] { 1f, 1f, 1f }));
            var inMovement = Plan(movementModel,
                At(movementModel, Id(40), 4f, RoadElementKind.JunctionMovement), Id(60),
                1, Id(80), "route", 0);
            Assert.That(inMovement.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(inMovement.Plan.Occurrences[0].StartSMeters, Is.EqualTo(4f));
            Assert.That(inMovement.Plan.Occurrences[1].Id, Is.EqualTo(Id(21)));
        }

        [Test]
        public void ReuseRefusesADifferentDestination()
        {
            var model = RoadModelCompiler.Compile(Source(new[] { 1f, 1f, 1f }));
            var original = Plan(model, At(model, Id(20)), Id(60), 1, Id(80), "route", 0).Plan;
            var redirected = Plan(model, At(model, Id(20)), Id(61), 1, Id(80), "route", 0, original);
            Assert.That(redirected.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(redirected.Reason, Is.EqualTo(RouteReason.StalePlan));
            Assert.That(redirected.Plan.ExitPortalId, Is.EqualTo(Id(61)));
        }

        [Test]
        public void PortalBehindRequiresASecondDirectedVisitAndClosedCycleTerminates()
        {
            var model = RoadModelCompiler.Compile(Loop(true));
            foreach (float s in new[] { 5f, 6f })
            {
                var result = Plan(model, At(model, Id(20), s), Id(70),
                    1, Id(80), "route", 0);
                Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Planned));
                Assert.That(result.Plan.Occurrences.Count, Is.EqualTo(2));
                Assert.That(result.Plan.Occurrences[0].Id, Is.EqualTo(Id(20)));
                Assert.That(result.Plan.Occurrences[0].EndSMeters, Is.EqualTo(model.Corridors[0].LengthMeters));
                Assert.That(result.Plan.Occurrences[1].Id, Is.EqualTo(Id(20)));
                Assert.That(result.Plan.Occurrences[1].StartSMeters, Is.Zero);
                Assert.That(result.Plan.Occurrences[1].EndSMeters, Is.EqualTo(5f));
            }
            var closed = RoadModelCompiler.Compile(Loop(false));
            var noRoute = Plan(closed, Id(20));
            Assert.That(noRoute.Outcome, Is.EqualTo(RouteOutcome.NoRoute));
            Assert.That(noRoute.Reason, Is.EqualTo(RouteReason.DestinationUnreachable));
        }

        [Test]
        public void ReuseTracksForwardProgressAndTrafficIdentity()
        {
            var model = RoadModelCompiler.Compile(Source(new[] { 1f, 1f, 1f }));
            var original = Plan(model, At(model, Id(20)), Id(60),
                1, Id(80), "route", 0).Plan;
            var corridorProgress = Plan(model, At(model, Id(20), 5f), Id(60),
                1, Id(80), "route", 0, original);
            Assert.That(corridorProgress.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(corridorProgress.Reason, Is.EqualTo(RouteReason.Requested));
            Assert.That(corridorProgress.Plan.ProgressOccurrenceIndex, Is.Zero);
            Assert.That(corridorProgress.Plan.ProgressSMeters, Is.EqualTo(5f));
            Assert.That(corridorProgress.Plan.Occurrences, Is.SameAs(original.Occurrences));

            var movementProgress = Plan(model,
                At(model, Id(40), 4f, RoadElementKind.JunctionMovement), Id(60),
                1, Id(80), "route", 0, corridorProgress.Plan);
            Assert.That(movementProgress.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(movementProgress.Reason, Is.EqualTo(RouteReason.Requested));
            Assert.That(movementProgress.Plan.ProgressOccurrenceIndex, Is.EqualTo(1));
            Assert.That(movementProgress.Plan.ProgressSMeters, Is.EqualTo(4f));

            var backward = Plan(model, At(model, Id(20), 4f), Id(60),
                1, Id(80), "route", 0, corridorProgress.Plan);
            Assert.That(backward.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(backward.Reason, Is.EqualTo(RouteReason.StalePlan));
            var oldOccurrence = Plan(model, At(model, Id(20), 9f), Id(60),
                1, Id(80), "route", 0, movementProgress.Plan);
            Assert.That(oldOccurrence.Outcome, Is.EqualTo(RouteOutcome.Replanned));

            var otherTraffic = Plan(model, At(model, Id(20)), Id(60),
                1, Id(81), "route", 0, original);
            Assert.That(otherTraffic.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(otherTraffic.Reason, Is.EqualTo(RouteReason.StalePlan));
            Assert.That(otherTraffic.Plan.TrafficId, Is.EqualTo(Id(81)));
        }

        [Test]
        public void ReuseDistinguishesRepeatedVisitsOnALoop()
        {
            var model = RoadModelCompiler.Compile(Loop(true));
            var repeated = Plan(model, At(model, Id(20), 6f), Id(70), 1, Id(80), "route", 0).Plan;
            Assert.That(repeated.Occurrences.Count, Is.EqualTo(2));
            Assert.That(repeated.Occurrences[0].Id, Is.EqualTo(Id(20)));
            Assert.That(repeated.Occurrences[1].Id, Is.EqualTo(Id(20)));

            var firstVisit = Plan(model, At(model, Id(20), 7f), Id(70), 1, Id(80), "route", 0, repeated);
            Assert.That(firstVisit.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(firstVisit.Reason, Is.EqualTo(RouteReason.Requested));
            Assert.That(firstVisit.Plan.ProgressOccurrenceIndex, Is.Zero);
            Assert.That(firstVisit.Plan.ProgressSMeters, Is.EqualTo(7f));
            Assert.That(firstVisit.Plan.Occurrences, Is.SameAs(repeated.Occurrences));

            var secondVisit = Plan(model, At(model, Id(20), 2f), Id(70), 1, Id(80), "route", 0, firstVisit.Plan);
            Assert.That(secondVisit.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(secondVisit.Reason, Is.EqualTo(RouteReason.Requested));
            Assert.That(secondVisit.Plan.ProgressOccurrenceIndex, Is.EqualTo(1));
            Assert.That(secondVisit.Plan.ProgressSMeters, Is.EqualTo(2f));
            Assert.That(secondVisit.Plan.Occurrences, Is.SameAs(repeated.Occurrences));

            var advanced = Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0, secondVisit.Plan);
            Assert.That(advanced.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(advanced.Plan.ProgressOccurrenceIndex, Is.EqualTo(1));
            Assert.That(advanced.Plan.ProgressSMeters, Is.EqualTo(4f));

            var backwardVisit = Plan(model, At(model, Id(20), 3f), Id(70), 1, Id(80), "route", 0, advanced.Plan);
            Assert.That(backwardVisit.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(backwardVisit.Reason, Is.EqualTo(RouteReason.StalePlan));
        }

        [Test]
        public void PositiveCycleDoesNotHideTheOnlyZeroWeightExit()
        {
            var model = RoadModelCompiler.Compile(PositiveCycleWithZeroExit());
            var result = Plan(model, Id(20));
            Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(result.Plan.ExitPortalId, Is.EqualTo(Id(60)));
            Assert.That(result.Diagnostics, Is.EqualTo(RouteDiagnostic.ZeroWeightFallback));
            Assert.That(result.Plan.Occurrences.Count, Is.EqualTo(3));
        }

        [Test]
        public void ToleratedOutOfRangePortalNeverProducesAnOutOfRangeOccurrence()
        {
            var source = Source(new[] { 1f, 1f, 1f });
            var portal = source.Portals[0];
            portal.SMeters = 10.01f;
            source.Portals[0] = portal;
            var model = RoadModelCompiler.Compile(source);
            var unavailable = Plan(model, At(model, Id(20)), Id(60),
                1, Id(80), "route", 0);
            Assert.That(unavailable.Outcome, Is.EqualTo(RouteOutcome.NoRoute));
            Assert.That(unavailable.Reason, Is.EqualTo(RouteReason.DestinationUnavailable));
            Assert.That(unavailable.Plan, Is.Null);
        }

        [Test]
        public void NearestAheadPortalWinsAndDistanceBeatsPreference()
        {
            var source = Source(new[] { 1f, 1f, 1f });
            source.Portals = new[] {
                new Portal { Id = Id(70), CorridorId = Id(20), Role = PortalRole.Exit, SMeters = 8f,
                    EnvelopeLengthMeters = 1f, EnvelopeHalfWidthMeters = 1f },
                new Portal { Id = Id(71), CorridorId = Id(20), Role = PortalRole.Exit, SMeters = 5f,
                    EnvelopeLengthMeters = 1f, EnvelopeHalfWidthMeters = 1f } };
            var corridorModel = RoadModelCompiler.Compile(source);
            var middle = Plan(corridorModel, At(corridorModel, Id(20), 4f), RoadId.None, 1, Id(80), "route", 0);
            Assert.That(middle.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(middle.Plan.ExitPortalId, Is.EqualTo(Id(71)));
            Assert.That(middle.Plan.DistanceMeters, Is.EqualTo(1d));
            var past = Plan(corridorModel, At(corridorModel, Id(20), 6f), RoadId.None, 1, Id(80), "route", 0);
            Assert.That(past.Plan.ExitPortalId, Is.EqualTo(Id(70)));
            Assert.That(past.Plan.DistanceMeters, Is.EqualTo(2d));

            var far = Source(new[] { 1f, 1f, 1f });
            var farCorridor = far.Corridors[3];
            farCorridor.LengthMeters = 100f;
            farCorridor.Samples = Line(20f, 120f);
            far.Corridors[3] = farCorridor;
            var farPortal = far.Portals[2];
            farPortal.SMeters = 99.5f;
            far.Portals[2] = farPortal;
            var farModel = RoadModelCompiler.Compile(far);
            for (ulong seed = 0; seed < 200; seed++)
            {
                var result = Plan(farModel, Id(20), seed);
                Assert.That(result.Plan, Is.Not.Null);
                Assert.That(result.Plan.ExitPortalId, Is.EqualTo(Id(60)).Or.EqualTo(Id(61)));
            }
        }

        [Test]
        public void ClosedExistingExitReplansToAnotherOpenExit()
        {
            var model = RoadModelCompiler.Compile(Source(new[] { 1f, 1f, 1f }));
            var location = At(model, Id(20));
            var old = Plan(model, location, RoadId.None,
                1, Id(80), "route", 0).Plan;
            var result = Plan(model, location, RoadId.None,
                1, Id(80), "route", 0, old, closedPortalIds: new[] { old.ExitPortalId });
            Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(result.Reason, Is.EqualTo(RouteReason.StalePlan));
            Assert.That(result.Plan.ExitPortalId, Is.Not.EqualTo(old.ExitPortalId));
        }

        [Test]
        public void SignedModelConnectsEveryEntryToEveryExitByMovements()
        {
            var source = RoadModelDocument.Load(File.ReadAllText("Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json"));
            var model = RoadModelCompiler.Compile(source);
            Assert.That(model.Connections.Count, Is.Zero);
            int entries = 0, exits = 0, pairs = 0;
            foreach (var portal in model.Portals)
            {
                if (portal.Role == PortalRole.Entry) entries++;
                if (portal.Role == PortalRole.Exit) exits++;
            }
            Assert.That(entries, Is.EqualTo(4));
            Assert.That(exits, Is.EqualTo(4));
            int portalsBefore = model.Portals.Count;
            int corridorsBefore = model.Corridors.Count;
            var versionBefore = model.Version;
            foreach (var entry in model.Portals)
            {
                if (entry.Role != PortalRole.Entry) continue;
                foreach (var exit in model.Portals)
                {
                    if (exit.Role != PortalRole.Exit) continue;
                    pairs++;
                    var result = Plan(model, At(model, entry.CorridorId, entry.SMeters),
                        exit.Id, 7, Id(80), "route", 0);
                    Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Planned), entry.Id + " -> " + exit.Id);
                    Assert.That(result.Plan.ExitPortalId, Is.EqualTo(exit.Id));
                    Assert.That(result.Plan.ModelVersion, Is.EqualTo(model.Version));
                    Assert.That(result.Plan.TotalCost, Is.GreaterThan(0d));
                    Assert.That(double.IsInfinity(result.Plan.TotalCost), Is.False);
                    Assert.That(double.IsNaN(result.Plan.DistanceMeters) || double.IsInfinity(result.Plan.DistanceMeters), Is.False);
                    Assert.That(double.IsNaN(result.Plan.PreferenceCost) || double.IsInfinity(result.Plan.PreferenceCost), Is.False);
                    double sum = 0d;
                    for (int i = 0; i < result.Plan.Occurrences.Count; i++)
                    {
                        var step = result.Plan.Occurrences[i];
                        sum += step.EndSMeters - step.StartSMeters;
                        float length;
                        EffectiveLaneCorridor corridor;
                        CompiledJunctionMovement movement;
                        if (step.Kind == RoadElementKind.LaneCorridor)
                        {
                            Assert.That(model.TryGetCorridor(step.Id, out corridor), Is.True);
                            length = corridor.LengthMeters;
                        }
                        else
                        {
                            Assert.That(model.TryGetMovement(step.Id, out movement), Is.True);
                            length = movement.LengthMeters;
                        }
                        Assert.That(step.StartSMeters, Is.GreaterThanOrEqualTo(0f));
                        Assert.That(step.EndSMeters, Is.InRange(step.StartSMeters, length));
                        if (i + 1 < result.Plan.Occurrences.Count)
                            Assert.That(step.EndSMeters, Is.EqualTo(length));
                    }
                    Assert.That(result.Plan.DistanceMeters, Is.EqualTo(sum).Within(1e-5d));
                    AssertEquivalent(result.Plan, Plan(model, At(model, entry.CorridorId, entry.SMeters),
                        exit.Id, 7, Id(80), "route", 0).Plan);
                }
            }
            Assert.That(pairs, Is.EqualTo(16));
            // Le planner est pur : aucun acces scene/cycle de vie, et le modele compile n'est pas mute.
            Assert.That(model.Portals.Count, Is.EqualTo(portalsBefore));
            Assert.That(model.Corridors.Count, Is.EqualTo(corridorsBefore));
            Assert.That(model.Version, Is.EqualTo(versionBefore));
        }
    }
}
