using System;
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
            return RoutePlanner.Plan(model, At(model, start), RoadId.None, seed, Id(80), "route", 0);
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
            var counts = new int[3];
            for (ulong seed = 0; seed < 4000; seed++)
            {
                var result = Plan(model, Id(20), seed);
                Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Planned));
                Assert.That(result.Plan.TotalCost, Is.GreaterThan(0d));
                Assert.That(double.IsInfinity(result.Plan.TotalCost), Is.False);
                Assert.That(double.IsNaN(result.Plan.DistanceMeters) || double.IsInfinity(result.Plan.DistanceMeters), Is.False);
                Assert.That(double.IsNaN(result.Plan.PreferenceCost) || double.IsInfinity(result.Plan.PreferenceCost), Is.False);
                int chosen = (int)result.Plan.ExitPortalId.Low - 60;
                counts[chosen]++;
                Assert.That(Plan(model, Id(20), seed).Plan.ExitPortalId, Is.EqualTo(result.Plan.ExitPortalId));
            }
            Assert.That(counts[0], Is.InRange(1050, 1350));
            Assert.That(counts[1], Is.InRange(1800, 2200));
            Assert.That(counts[2], Is.InRange(650, 950));

            var reordered = Source(new[] { 30f, 50f, 20f });
            Array.Reverse(reordered.Movements);
            Array.Reverse(reordered.Portals);
            var permuted = RoadModelCompiler.Compile(reordered);
            Assert.That(permuted.Version, Is.EqualTo(model.Version));
            for (ulong seed = 0; seed < 100; seed++)
                Assert.That(Plan(permuted, Id(20), seed).Plan.ExitPortalId,
                    Is.EqualTo(Plan(model, Id(20), seed).Plan.ExitPortalId));

            var raised = RoadModelCompiler.Compile(Source(new[] { 60f, 50f, 20f }));
            for (ulong seed = 0; seed < 100; seed++)
                if (Plan(model, Id(20), seed).Plan.ExitPortalId == Id(60))
                    Assert.That(Plan(raised, Id(20), seed).Plan.ExitPortalId, Is.EqualTo(Id(60)));

            bool identityChangesDecision = false, domainChangesDecision = false;
            for (ulong seed = 0; seed < 100; seed++)
            {
                var baseline = Plan(model, Id(20), seed).Plan.ExitPortalId;
                identityChangesDecision |= RoutePlanner.Plan(model, At(model, Id(20)), RoadId.None,
                    seed, Id(81), "route", 0).Plan.ExitPortalId != baseline;
                domainChangesDecision |= RoutePlanner.Plan(model, At(model, Id(20)), RoadId.None,
                    seed, Id(80), "other", 1).Plan.ExitPortalId != baseline;
            }
            Assert.That(identityChangesDecision, Is.True);
            Assert.That(domainChangesDecision, Is.True);
        }

        [Test]
        public void InfeasibleAndZeroWeightChoicesHaveExplicitBehavior()
        {
            var deadEnd = RoadModelCompiler.Compile(Source(new[] { 1f, 1f, 1000f }, new[] { true, true, false }));
            for (ulong seed = 0; seed < 100; seed++)
                Assert.That(Plan(deadEnd, Id(20), seed).Plan.ExitPortalId, Is.Not.EqualTo(Id(62)));

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
                Assert.That(result.Plan.ExitPortalId, Is.Not.EqualTo(Id(62)));
            }
            var invalid = Source(new[] { -1f, 1f, 1f });
            Assert.Throws<RoadModelCompilationException>(() => RoadModelCompiler.Compile(invalid));
        }

        [Test]
        public void PortalPositionAndStaleInputsAreNamed()
        {
            var source = Source(new[] { 1f, 1f, 1f });
            source.Portals = new[] { new Portal {
                Id = Id(70), CorridorId = Id(20), Role = PortalRole.Exit,
                SMeters = 5f, EnvelopeLengthMeters = 1f, EnvelopeHalfWidthMeters = 1f } };
            var model = RoadModelCompiler.Compile(source);
            var ahead = RoutePlanner.Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0);
            Assert.That(ahead.Plan.Occurrences.Count, Is.EqualTo(1));
            Assert.That(ahead.Plan.DistanceMeters, Is.EqualTo(1d));
            foreach (float s in new[] { 5f, 6f })
            {
                var result = RoutePlanner.Plan(model, At(model, Id(20), s), Id(70), 1, Id(80), "route", 0);
                Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.NoRoute));
                Assert.That(result.Reason, Is.EqualTo(RouteReason.DestinationUnreachable));
            }
            var stale = At(model, Id(20));
            stale.ModelVersion = default(RoadModelVersion);
            Assert.That(RoutePlanner.Plan(model, stale, Id(70), 1, Id(80), "route", 0).Reason,
                Is.EqualTo(RouteReason.StaleLocalization));
            Assert.That(RoutePlanner.Plan(model, At(model, Id(99)), Id(70), 1, Id(80), "route", 0).Reason,
                Is.EqualTo(RouteReason.InvalidStart));
            Assert.That(RoutePlanner.Plan(model, At(model, Id(20)), Id(99), 1, Id(80), "route", 0).Reason,
                Is.EqualTo(RouteReason.DestinationUnavailable));
            Assert.That(RoutePlanner.Plan(model, At(model, Id(20)), Id(70), 1, Id(80), "route", 0,
                closedPortalIds: new[] { Id(70) }).Reason,
                Is.EqualTo(RouteReason.DestinationUnavailable));
            var old = RoutePlanner.Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0).Plan;
            var replacement = RoutePlanner.Plan(model, At(model, Id(20), 3f), Id(70), 1, Id(80), "route", 0, old);
            Assert.That(replacement.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(replacement.Reason, Is.EqualTo(RouteReason.StalePlan));
            Assert.That(replacement.Plan.Occurrences[0].StartSMeters, Is.EqualTo(3f));
            Assert.That(RoutePlanner.Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0, old).Plan,
                Is.SameAs(old));
            Assert.That(RoutePlanner.Plan(model, At(model, Id(20), 4f), Id(70), 1, Id(80), "route", 0,
                old, true).Outcome, Is.EqualTo(RouteOutcome.Replanned));

            var changedSource = Source(new[] { 2f, 1f, 1f });
            var changedModel = RoadModelCompiler.Compile(changedSource);
            var versionReplan = RoutePlanner.Plan(changedModel, At(changedModel, Id(20)), RoadId.None,
                1, Id(80), "route", 0, old);
            Assert.That(versionReplan.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(versionReplan.Reason, Is.EqualTo(RouteReason.StalePlan));

            var movementModel = RoadModelCompiler.Compile(Source(new[] { 1f, 1f, 1f }));
            var inMovement = RoutePlanner.Plan(movementModel,
                At(movementModel, Id(40), 4f, RoadElementKind.JunctionMovement), Id(60),
                1, Id(80), "route", 0);
            Assert.That(inMovement.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(inMovement.Plan.Occurrences[0].StartSMeters, Is.EqualTo(4f));
            Assert.That(inMovement.Plan.Occurrences[1].Id, Is.EqualTo(Id(21)));
        }

        [Test]
        public void PortalBehindRequiresASecondDirectedVisitAndClosedCycleTerminates()
        {
            var model = RoadModelCompiler.Compile(Loop(true));
            foreach (float s in new[] { 5f, 6f })
            {
                var result = RoutePlanner.Plan(model, At(model, Id(20), s), Id(70),
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
            var original = RoutePlanner.Plan(model, At(model, Id(20)), Id(60),
                1, Id(80), "route", 0).Plan;
            var corridorProgress = RoutePlanner.Plan(model, At(model, Id(20), 5f), Id(60),
                1, Id(80), "route", 0, original);
            Assert.That(corridorProgress.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(corridorProgress.Plan.ProgressOccurrenceIndex, Is.Zero);
            Assert.That(corridorProgress.Plan.ProgressSMeters, Is.EqualTo(5f));
            Assert.That(corridorProgress.Plan.Occurrences, Is.SameAs(original.Occurrences));

            var movementProgress = RoutePlanner.Plan(model,
                At(model, Id(40), 4f, RoadElementKind.JunctionMovement), Id(60),
                1, Id(80), "route", 0, corridorProgress.Plan);
            Assert.That(movementProgress.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(movementProgress.Plan.ProgressOccurrenceIndex, Is.EqualTo(1));
            Assert.That(movementProgress.Plan.ProgressSMeters, Is.EqualTo(4f));

            var backward = RoutePlanner.Plan(model, At(model, Id(20), 4f), Id(60),
                1, Id(80), "route", 0, corridorProgress.Plan);
            Assert.That(backward.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(backward.Reason, Is.EqualTo(RouteReason.StalePlan));
            var oldOccurrence = RoutePlanner.Plan(model, At(model, Id(20), 9f), Id(60),
                1, Id(80), "route", 0, movementProgress.Plan);
            Assert.That(oldOccurrence.Outcome, Is.EqualTo(RouteOutcome.Replanned));

            var otherTraffic = RoutePlanner.Plan(model, At(model, Id(20)), Id(60),
                1, Id(81), "route", 0, original);
            Assert.That(otherTraffic.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(otherTraffic.Reason, Is.EqualTo(RouteReason.StalePlan));
            Assert.That(otherTraffic.Plan.TrafficId, Is.EqualTo(Id(81)));

            var loop = RoadModelCompiler.Compile(Loop(true));
            var repeated = RoutePlanner.Plan(loop, At(loop, Id(20), 6f), Id(70),
                1, Id(80), "route", 0).Plan;
            Assert.That(RoutePlanner.Plan(loop, At(loop, Id(20), 7f), Id(70),
                1, Id(80), "route", 0, repeated).Outcome, Is.EqualTo(RouteOutcome.Replanned));
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
            var unavailable = RoutePlanner.Plan(model, At(model, Id(20)), Id(60),
                1, Id(80), "route", 0);
            Assert.That(unavailable.Outcome, Is.EqualTo(RouteOutcome.NoRoute));
            Assert.That(unavailable.Reason, Is.EqualTo(RouteReason.DestinationUnavailable));
            Assert.That(unavailable.Plan, Is.Null);
        }

        [Test]
        public void ClosedExistingExitReplansToAnotherOpenExit()
        {
            var model = RoadModelCompiler.Compile(Source(new[] { 1f, 1f, 1f }));
            var location = At(model, Id(20));
            var old = RoutePlanner.Plan(model, location, RoadId.None,
                1, Id(80), "route", 0).Plan;
            var result = RoutePlanner.Plan(model, location, RoadId.None,
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
            foreach (var entry in model.Portals)
            {
                if (entry.Role != PortalRole.Entry) continue;
                foreach (var exit in model.Portals)
                {
                    if (exit.Role != PortalRole.Exit) continue;
                    var result = RoutePlanner.Plan(model, At(model, entry.CorridorId, entry.SMeters),
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
                }
            }
        }
    }
}
