using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.32 : frame partagee indexee et perception pure. Modele synthetique ecrit a la main (route a double
    /// sens, deux voies adjacentes, carrefour avec un tout-droit et un virage r = 10 m) et modele `MVP_Run` pour la
    /// courbe serree. Plusieurs acteurs par frame dans chaque scenario ; deterministe, sans scene ni physique.
    /// </summary>
    [Category("Core")]
    [Category("Story532")]
    public sealed class Story532PerceptionTests
    {
        private const string TrafficRootPath = "Assets/RoadRage/Features/Vehicles/Traffic";
        private const string MvpRunModelPath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json";

        private static RoadId Id(int index)
        {
            return new RoadId(0x5320532000000000UL | (uint)index, 0x0532053200000000UL | (uint)index);
        }

        private static RoadId Vehicle(int index)
        {
            return new RoadId(0x532UL, (ulong)index);
        }

        private static readonly RoadId ModelId = Id(1);
        private static readonly RoadId SectionS1 = Id(2);
        private static readonly RoadId SectionS2 = Id(3);
        private static readonly RoadId SectionS3 = Id(4);
        private static readonly RoadId CorridorSB = Id(5);
        private static readonly RoadId CorridorNB = Id(6);
        private static readonly RoadId CorridorNB2 = Id(7);
        private static readonly RoadId CorridorN2 = Id(8);
        private static readonly RoadId CorridorEB = Id(9);
        private static readonly RoadId JunctionJ = Id(10);
        private static readonly RoadId MovementMS = Id(11);
        private static readonly RoadId MovementMR = Id(12);
        private static readonly RoadId ControlC = Id(13);
        private static readonly RoadId AdjacencyRight = Id(14);
        private static readonly RoadId AdjacencyLeft = Id(15);
        private static readonly RoadId PortalEntry = Id(16);
        private static readonly RoadId PortalExitN2 = Id(17);
        private static readonly RoadId PortalExitEB = Id(18);
        private static readonly RoadId ZoneZ = Id(19);
        private static readonly RoadId PlanP = Id(20);
        private static readonly RoadId GroupStraight = Id(21);
        private static readonly RoadId GroupRight = Id(22);
        private static readonly RoadId PhaseStraightGreen = Id(23);
        private static readonly RoadId PhaseRightGreen = Id(24);

        private const float TurnRadius = 10f;

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };

        private static readonly PerceptionLimits Limits = new PerceptionLimits(30f, 3f, 15f, 8);

        // ================================================================== modele synthetique

        /// <summary>
        /// S1 : SB (x=-2, z 30->0), NB (x=2, z 0->30), NB2 (x=6, z 0->30), adjacences NB -> NB2 a droite et
        /// NB2 -> NB a gauche. Carrefour J a z=30 : MS tout droit NB2 -> N2 (x=6, z 40->70), MR quart de cercle
        /// r=10 a droite NB2 -> EB (z=40, x 16->46). Zone de conflit MS/MR. Sorties sur N2 et EB.
        /// </summary>
        private static RoadModelSource BuildSource(bool signalized = false)
        {
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.Label = "modele synthetique 5.32";
            var validation = new RoadModelValidationProfile();
            validation.MaxVehicleHalfWidthMeters = 1.03f;
            validation.MaxVehicleLengthMeters = 4.5f;
            validation.LateralClearanceMarginMeters = 0.25f;
            validation.SeamGapToleranceMeters = 0.05f;
            validation.SeamTangentToleranceDegrees = 5f;
            validation.LengthToleranceMeters = 0.05f;
            validation.EnvelopeOverlapToleranceMeters = 0.05f;
            validation.GroundingMaxOffAxisDegrees = 45f;
            source.ValidationProfile = validation;
            var localization = new RoadLocalizationProfile();
            localization.ScoreBandMeters = 0.15f;
            localization.HysteresisMeters = 0.1f;
            localization.AcceptanceDistanceMeters = 2.5f;
            localization.WrongWayHeadingDegrees = 90f;
            source.LocalizationProfile = localization;

            source.Sections = new[] { Section(SectionS1), Section(SectionS2), Section(SectionS3) };
            source.Corridors = new[]
            {
                Corridor(CorridorSB, SectionS1, Straight(new Vector3(-2f, 0f, 30f), new Vector3(-2f, 0f, 0f)), 0, false),
                Corridor(CorridorNB, SectionS1, Straight(new Vector3(2f, 0f, 0f), new Vector3(2f, 0f, 30f)), 1, true),
                Corridor(CorridorNB2, SectionS1, Straight(new Vector3(6f, 0f, 0f), new Vector3(6f, 0f, 30f)), 2, false),
                Corridor(CorridorN2, SectionS2, Straight(new Vector3(6f, 0f, 40f), new Vector3(6f, 0f, 70f)), 0, true),
                Corridor(CorridorEB, SectionS3, Straight(new Vector3(16f, 0f, 40f), new Vector3(46f, 0f, 40f)), 0, true)
            };
            source.Adjacencies = new[]
            {
                Adjacency(AdjacencyRight, CorridorNB, CorridorNB2, LaneSide.Right),
                Adjacency(AdjacencyLeft, CorridorNB2, CorridorNB, LaneSide.Left)
            };

            var junction = new Junction();
            junction.Id = JunctionJ;
            junction.Label = "J";
            junction.Feature = JunctionFeature.TJunction;
            junction.Boundary = Box(new Vector3(11f, 0f, 35f), new Vector3(8f, 3f, 6f));
            source.Junctions = new[] { junction };
            source.Movements = new[]
            {
                Movement(MovementMS, CorridorNB2, CorridorN2, Straight(new Vector3(6f, 0f, 30f), new Vector3(6f, 0f, 40f))),
                Movement(MovementMR, CorridorNB2, CorridorEB, RightQuarterTurn(new Vector3(6f, 0f, 30f), TurnRadius))
            };
            var control = new JunctionControl();
            control.Id = ControlC;
            control.JunctionId = JunctionJ;
            control.Kind = signalized ? JunctionControlKind.Signalized : JunctionControlKind.Uncontrolled;
            control.ControlledMovementIds = new[] { MovementMS, MovementMR };
            source.Controls = new[] { control };

            var zone = new ConflictZone();
            zone.Id = ZoneZ;
            zone.JunctionId = JunctionJ;
            zone.Volume = Box(new Vector3(8f, 0f, 34f), new Vector3(3f, 2f, 4f));
            zone.MemberMovementIds = new[] { MovementMS, MovementMR };
            source.ConflictZones = new[] { zone };

            if (signalized)
            {
                var plan = new SignalPlan();
                plan.Id = PlanP;
                plan.JunctionId = JunctionJ;
                plan.Groups = new[] { Group(GroupStraight, MovementMS), Group(GroupRight, MovementMR) };
                plan.Phases = new[]
                {
                    Phase(PhaseStraightGreen, SignalState.Green, SignalState.Red),
                    Phase(PhaseRightGreen, SignalState.Red, SignalState.Green)
                };
                source.SignalPlans = new[] { plan };
            }

            source.Portals = new[]
            {
                MakePortal(PortalEntry, CorridorNB2, PortalRole.Entry, 0f),
                MakePortal(PortalExitN2, CorridorN2, PortalRole.Exit, 30f),
                MakePortal(PortalExitEB, CorridorEB, PortalRole.Exit, 30f)
            };
            return source;
        }

        private static CompiledRoadModel Model(bool signalized = false)
        {
            return RoadModelCompiler.Compile(BuildSource(signalized));
        }

        private static RoadSection Section(RoadId id)
        {
            var section = new RoadSection();
            section.Id = id;
            section.RoadClass = RoadClass.Local;
            section.DefaultSpeedLimitMetersPerSecond = 13.9f;
            section.DefaultAllowedVehicleClasses = VehicleClassMask.Car;
            return section;
        }

        private static LaneCorridor Corridor(RoadId id, RoadId sectionId, RoadCurveSample[] samples, int order, bool datum)
        {
            var corridor = new LaneCorridor();
            corridor.Id = id;
            corridor.SectionId = sectionId;
            corridor.Samples = samples;
            corridor.LengthMeters = samples[samples.Length - 1].SMeters;
            corridor.LateralOrder = order;
            corridor.IsCrossSectionDatum = datum;
            return corridor;
        }

        private static JunctionMovement Movement(RoadId id, RoadId from, RoadId to, RoadCurveSample[] samples)
        {
            var movement = new JunctionMovement();
            movement.Id = id;
            movement.JunctionId = JunctionJ;
            movement.FromCorridorId = from;
            movement.ToCorridorId = to;
            movement.Samples = samples;
            movement.LengthMeters = samples[samples.Length - 1].SMeters;
            movement.RoutePreferenceWeight = 1f;
            return movement;
        }

        private static LaneAdjacency Adjacency(RoadId id, RoadId from, RoadId to, LaneSide side)
        {
            var adjacency = new LaneAdjacency();
            adjacency.Id = id;
            adjacency.FromCorridorId = from;
            adjacency.ToCorridorId = to;
            adjacency.Side = side;
            adjacency.FromStartSMeters = 0f;
            adjacency.FromEndSMeters = 30f;
            adjacency.ToStartSMeters = 0f;
            adjacency.ToEndSMeters = 30f;
            adjacency.Permission = LaneChangePermission.Allowed;
            return adjacency;
        }

        private static SignalGroup Group(RoadId id, RoadId member)
        {
            var group = new SignalGroup();
            group.GroupId = id;
            group.MemberMovementIds = new[] { member };
            return group;
        }

        private static SignalPhase Phase(RoadId id, SignalState straight, SignalState right)
        {
            var phase = new SignalPhase();
            phase.PhaseId = id;
            phase.DurationSeconds = 30f;
            phase.GroupStates = new[]
            {
                new SignalGroupState { GroupId = GroupStraight, State = straight },
                new SignalGroupState { GroupId = GroupRight, State = right }
            };
            return phase;
        }

        private static Portal MakePortal(RoadId id, RoadId corridorId, PortalRole role, float s)
        {
            var portal = new Portal();
            portal.Id = id;
            portal.CorridorId = corridorId;
            portal.Role = role;
            portal.SMeters = s;
            portal.EnvelopeLengthMeters = 6f;
            portal.EnvelopeHalfWidthMeters = 2f;
            return portal;
        }

        private static RoadBoundsBox Box(Vector3 center, Vector3 extents)
        {
            var box = new RoadBoundsBox();
            box.Center = center;
            box.Extents = extents;
            return box;
        }

        private static RoadCurveSample[] Straight(Vector3 start, Vector3 end)
        {
            float length = (end - start).magnitude;
            var tangent = (end - start).normalized;
            return new[]
            {
                Sample(0f, start, tangent, 0f),
                Sample(0.5f * length, Vector3.Lerp(start, end, 0.5f), tangent, 0f),
                Sample(length, end, tangent, 0f)
            };
        }

        private static RoadCurveSample[] RightQuarterTurn(Vector3 start, float radius)
        {
            const int steps = 18;
            var center = start + new Vector3(radius, 0f, 0f);
            var samples = new RoadCurveSample[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float phi = 0.5f * Mathf.PI * i / steps;
                var position = center + new Vector3(-radius * Mathf.Cos(phi), 0f, radius * Mathf.Sin(phi));
                var tangent = new Vector3(Mathf.Sin(phi), 0f, Mathf.Cos(phi));
                samples[i] = Sample(radius * phi, position, tangent, 1f / radius);
            }
            return samples;
        }

        private static RoadCurveSample Sample(float s, Vector3 position, Vector3 tangent, float curvature)
        {
            var sample = new RoadCurveSample();
            sample.SMeters = s;
            sample.Position = position;
            sample.Tangent = tangent;
            sample.Up = Vector3.up;
            sample.CurvaturePerMeter = curvature;
            sample.HalfWidthLeftMeters = 2f;
            sample.HalfWidthRightMeters = 2f;
            return sample;
        }

        // ================================================================== aides

        private static RoadCurve Curve(CompiledRoadModel model, RoadId element)
        {
            RoadElementKind kind;
            RoadCurve curve;
            IReadOnlyList<RoadCurveSample> samples;
            Assert.That(TrafficFrame.TryGetElement(model, element, out kind, out curve, out samples), Is.True);
            return curve;
        }

        private static VehicleFootprintPose Pose(CompiledRoadModel model, RoadId element, float s, VehicleFootprint footprint)
        {
            var point = Curve(model, element).Sample(s);
            return new VehicleFootprintPose { Position = point.Position, Forward = point.Tangent, Up = point.Up, Footprint = footprint };
        }

        private static TrafficActorInput Actor(CompiledRoadModel model, int index, RoadId element, float s,
            VehicleFootprint? footprint = null, float speed = 0f, PublishedIntentHorizon horizon = null)
        {
            return new TrafficActorInput(Vehicle(index), Pose(model, element, s, footprint ?? Car), speed, element,
                null, null, horizon);
        }

        private static PathHorizon Horizon(TrafficFrame frame, int agent, RoadId exit, float lookAhead = 200f)
        {
            TrafficActor actor;
            Assert.That(frame.TryGetActor(Vehicle(agent), out actor), Is.True);
            var route = RoutePlanner.Plan(new RouteRequest(frame.Model, actor.Location, exit, new RouteSeed(1),
                Vehicle(agent), "route", new DecisionCounter(frame.FrameId)));
            Assert.That(route.Plan, Is.Not.Null, route.Reason.ToString());
            return PathHorizon.Build(frame.Model, route.Plan, lookAhead);
        }

        private static AgentObservation Observe(TrafficFrame frame, int agent, PathHorizon horizon, int capacity = 32,
            PerceptionLimits? limits = null)
        {
            return TrafficPerception.Observe(frame, Vehicle(agent), horizon, limits ?? Limits, new SpatialQueryBuffer(capacity));
        }

        private static TrafficHazardInput Hazard(int index, TrafficHazardKind kind, Vector3 center, Vector3 extents,
            float confidence = 1f)
        {
            return new TrafficHazardInput(Vehicle(1000 + index), kind, Box(center, extents), Vector3.zero, confidence);
        }

        private static void AssertCode(string code, TestDelegate action)
        {
            var error = Assert.Throws<ArgumentException>(action);
            StringAssert.StartsWith(code, error.Message);
        }

        // Une occupation conservatrice ajoute au plus h/2 de chaque cote sur une ligne droite.
        private const float StraightSlack = TrafficFrame.OccupancySampleStepMeters + 1e-3f;

        // ================================================================== leader, suiveur, ligne droite

        [Test]
        public void LeaderAndFollowerOnOneCorridorAreMeasuredBumperToBumper()
        {
            var model = Model();
            var frame = new TrafficFrame(7, model, new[]
            {
                Actor(model, 1, CorridorNB2, 12f), Actor(model, 2, CorridorNB2, 22f, speed: 4f),
                Actor(model, 3, CorridorNB2, 2f, speed: 3f)
            });
            var horizon = Horizon(frame, 1, PortalExitN2);
            var observation = Observe(frame, 1, horizon);

            Assert.That(observation.Perceived, Is.True);
            Assert.That(observation.Leader.Status, Is.EqualTo(PerceptionStatus.Evaluated));
            var leader = observation.Leader.Items.Single();
            Assert.That(leader.TrafficId, Is.EqualTo(Vehicle(2)));
            float bumper = (22f - 2.22f) - (12f + 2.22f);
            Assert.That(leader.GapMeters, Is.InRange(bumper - StraightSlack, bumper + 1e-3f));
            Assert.That(Mathf.Abs(leader.GapMeters - 10f), Is.GreaterThan(4f), "jamais centre a centre");
            Assert.That(leader.SpeedMetersPerSecond, Is.EqualTo(4f));
            Assert.That(leader.Metadata.TimestampFrameId, Is.EqualTo(7UL));
            Assert.That(leader.Metadata.Source, Is.EqualTo(ObservationSource.StructuredOccupancy));
            Assert.That(leader.Metadata.RangeMeters, Is.EqualTo(horizon.LengthMeters));
            Assert.That(leader.Metadata.Confidence, Is.InRange(0f, 1f));

            var follower = observation.Follower.Items.Single();
            Assert.That(follower.TrafficId, Is.EqualTo(Vehicle(3)));
            Assert.That(follower.GapMeters, Is.InRange(bumper - StraightSlack, bumper + 1e-3f));
            // NB2 n'a aucun predecesseur : la portee cherchee s'arrete au debut du corridor, a l'arriere de l'agent.
            Assert.That(observation.Follower.RangeMeters, Is.InRange(12f - 2.22f - StraightSlack, 12f - 2.22f + 1e-3f));
        }

        [Test]
        public void NoLeaderReportsTheSearchedHorizonLength()
        {
            var model = Model();
            var frame = new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f), Actor(model, 2, CorridorNB2, 1f) });
            var horizon = Horizon(frame, 1, PortalExitN2);
            var observation = Observe(frame, 1, horizon);
            Assert.That(observation.Leader.Items, Is.Empty);
            Assert.That(observation.Leader.Total, Is.EqualTo(0));
            Assert.That(observation.Leader.RangeMeters, Is.EqualTo(horizon.LengthMeters));
        }

        [Test]
        public void TheNearestOfSeveralVehiclesAheadOnSuccessiveElementsIsTheLeader()
        {
            var model = Model();
            var frame = new TrafficFrame(1, model, new[]
            {
                Actor(model, 1, CorridorNB2, 5f), Actor(model, 2, CorridorNB2, 20f),
                Actor(model, 3, MovementMS, 5f), Actor(model, 4, CorridorN2, 10f)
            });
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            Assert.That(observation.Leader.Items.Single().TrafficId, Is.EqualTo(Vehicle(2)));
            Assert.That(observation.Leader.Total, Is.EqualTo(3), "les vehicules plus loin sont comptes, rien ne masque le plus proche");

            var without = new TrafficFrame(1, model, new[]
            {
                Actor(model, 1, CorridorNB2, 5f), Actor(model, 3, MovementMS, 5f), Actor(model, 4, CorridorN2, 10f)
            });
            var leader = Observe(without, 1, Horizon(without, 1, PortalExitN2)).Leader.Items.Single();
            Assert.That(leader.TrafficId, Is.EqualTo(Vehicle(3)));
            float bumper = 25f + (5f - 2.22f) - 2.22f;
            Assert.That(leader.GapMeters, Is.InRange(bumper - StraightSlack, bumper + 1e-3f));
        }

        // ================================================================== virage

        [Test]
        public void ALeaderJustBeyondTheBendIsFoundAlongTheHorizonWhereAForwardSweepWouldMissIt()
        {
            var model = Model();
            float phi = Mathf.PI / 3f;
            float leaderS = TurnRadius * phi;
            var frame = new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 27f), Actor(model, 2, MovementMR, leaderS) });
            TrafficActor leaderActor;
            frame.TryGetActor(Vehicle(2), out leaderActor);
            Assert.That(leaderActor.Location.ElementId, Is.EqualTo(MovementMR));

            // Axe avant de l'agent : x = 6. Le coin le plus proche du leader en est a plus d'une demi-largeur.
            float nearest = TrafficFrame.Corners(leaderActor.Pose).Min(c => Mathf.Abs(c.x - 6f));
            Assert.That(nearest, Is.GreaterThan(Car.LeftMeters + 1f), "un balayage plat dans l'axe avant le manquerait");

            var leader = Observe(frame, 1, Horizon(frame, 1, PortalExitEB)).Leader.Items.Single();
            Assert.That(leader.TrafficId, Is.EqualTo(Vehicle(2)));
            Assert.That(leader.ElementId, Is.EqualTo(MovementMR));
            // Arriere du rectangle sur l'arc : le coin interieur (rayon 10 - 1,03) recule de r·atan(2,22 / 8,97).
            float rearArc = TurnRadius * Mathf.Atan(Car.RearMeters / (TurnRadius - Car.LeftMeters));
            float bumper = 3f + (leaderS - rearArc) - Car.FrontMeters;
            Assert.That(leader.GapMeters, Is.LessThanOrEqualTo(bumper + 0.01f), "jamais surestime");
            Assert.That(leader.GapMeters, Is.GreaterThan(bumper - 0.3f));
        }

        // ================================================================== occupation conservatrice

        [Test]
        public void ConservativeOccupancyContainsTheWholeRectangleOnTheTightestMvpRunCurve()
        {
            var model = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(MvpRunModelPath)));
            Assert.That(model.DrivabilityProfile.Declared, Is.True);
            float admission = RoadModelCompiler.AdmissionRadiusMeters(model.DrivabilityProfile);
            var tightest = model.Movements.OrderByDescending(m => m.Samples.Max(x => Mathf.Abs(x.CurvaturePerMeter))).First();
            var peak = tightest.Samples.OrderByDescending(x => Mathf.Abs(x.CurvaturePerMeter)).First();
            Assert.That(1f / Mathf.Abs(peak.CurvaturePerMeter), Is.LessThanOrEqualTo(admission * 1.1f), "courbe proche de R_adm");

            var gauge = new VehicleFootprint { FrontMeters = 2.25f, RearMeters = 2.25f, LeftMeters = 1.03f, RightMeters = 1.03f };
            int checkedPoses = 0;
            foreach (float offset in new[] { -1.5f, 0f, 1.5f })
            {
                float s = Mathf.Clamp(peak.SMeters + offset, 0f, tightest.LengthMeters);
                var input = new TrafficActorInput(Vehicle(1), Pose(model, tightest.Id, s, gauge), 0f, tightest.Id,
                    new[] { tightest.Id });
                var frame = new TrafficFrame(1, model, new[] { input });
                ElementOccupant occupant;
                Assert.That(frame.TryGetOccupancy(Vehicle(1), out occupant), Is.True, "s = " + s);
                Assert.That(occupant.RemainderMeters, Is.GreaterThan(0f).And.LessThan(1f));
                var curve = Curve(model, occupant.ElementId);
                var c = TrafficFrame.Corners(input.Pose);
                for (int i = 0; i <= 60; i++)
                    for (int j = 0; j <= 30; j++)
                    {
                        var front = Vector3.Lerp(c[1], c[0], j / 30f);
                        var back = Vector3.Lerp(c[2], c[3], j / 30f);
                        float projected = ExtendedS(curve, Vector3.Lerp(back, front, i / 60f));
                        Assert.That(projected, Is.InRange(occupant.SMinMeters - 1e-4f, occupant.SMaxMeters + 1e-4f),
                            "point interieur hors de [sMin, sMax] a s = " + s);
                    }
                checkedPoses++;
            }
            Assert.That(checkedPoses, Is.EqualTo(3));
        }

        private static float ExtendedS(RoadCurve curve, Vector3 point)
        {
            var projection = curve.Project(point);
            if (!(projection.LongitudinalOverrunMeters > 0f)) return projection.SMeters;
            return projection.SMeters <= curve.StartS + 1e-3f
                ? projection.SMeters - projection.LongitudinalOverrunMeters
                : projection.SMeters + projection.LongitudinalOverrunMeters;
        }

        [Test]
        public void AFootprintBeyondTheCurvatureBoundIsExcludedRatherThanUnderestimated()
        {
            var model = Model();
            var wide = new VehicleFootprint { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 10.5f, RightMeters = 10.5f };
            var frame = new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f), Actor(model, 2, MovementMR, 5f, wide) });
            TrafficActor actor;
            frame.TryGetActor(Vehicle(2), out actor);
            Assert.That(actor.Location.Localized, Is.True);
            Assert.That(actor.OccupancyExclusion, Is.EqualTo(OccupancyExclusion.OccupancyNotBounded));
            ElementOccupant occupant;
            Assert.That(frame.TryGetOccupancy(Vehicle(2), out occupant), Is.False);
            Assert.That(frame.GetOccupants(MovementMR), Is.Empty);
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitEB));
            Assert.That(observation.Leader.Items.Any(x => x.TrafficId == Vehicle(2)), Is.False);
            Assert.That(observation.UnmeasuredActors.Items.Single().Reason, Is.EqualTo(OccupancyExclusion.OccupancyNotBounded));
        }

        // ================================================================== empreintes non declarees

        [Test]
        public void AnActorWithoutFootprintStaysLocalizedButNeverGetsAPhysicalDistance()
        {
            var model = Model();
            var zero = default(VehicleFootprint);
            var frame = new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f), Actor(model, 2, CorridorNB2, 20f, zero) });
            TrafficActor ghost;
            Assert.That(frame.TryGetActor(Vehicle(2), out ghost), Is.True);
            Assert.That(ghost.Location.Localized, Is.True);
            Assert.That(ghost.Location.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(ghost.FootprintDeclared, Is.False);
            Assert.That(ghost.OccupancyExclusion, Is.EqualTo(OccupancyExclusion.UndeclaredFootprint));

            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            Assert.That(observation.Leader.Items, Is.Empty);
            Assert.That(observation.Obstacles.Items, Is.Empty, "ni dans l'index spatial");
            var unmeasured = observation.UnmeasuredActors.Items.Single();
            Assert.That(unmeasured.TrafficId, Is.EqualTo(Vehicle(2)));
            Assert.That(unmeasured.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(unmeasured.SMeters, Is.EqualTo(20f).Within(0.01f));
        }

        [Test]
        public void AnAgentWithoutFootprintGetsExplicitStatusesAndKeepsHorizonOnlyChannels()
        {
            var model = Model();
            var other = new PublishedIntentHorizon(0, new[] { new IntentInterval(RoadElementKind.LaneCorridor, CorridorNB2, 0f, 30f) });
            var frame = new TrafficFrame(1, model, new[]
            {
                Actor(model, 1, CorridorNB2, 5f, default(VehicleFootprint)), Actor(model, 2, CorridorNB2, 20f, horizon: other),
                Actor(model, 3, CorridorN2, 10f)
            });
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            Assert.That(observation.Perceived, Is.True);
            foreach (var status in new[] { observation.Leader.Status, observation.Follower.Status, observation.Adjacent.Status, observation.Obstacles.Status })
                Assert.That(status, Is.EqualTo(PerceptionStatus.UndeclaredFootprint));
            Assert.That(observation.Leader.Items, Is.Empty);
            Assert.That(observation.Exit.Status, Is.EqualTo(PerceptionStatus.Evaluated));
            Assert.That(observation.Exit.Items.Single().FirstOccupantId, Is.EqualTo(Vehicle(3)));
            Assert.That(observation.IntentOverlaps.Status, Is.EqualTo(PerceptionStatus.Evaluated));
            Assert.That(observation.IntentOverlaps.Items.Single().OtherTrafficId, Is.EqualTo(Vehicle(2)));
        }

        [Test]
        public void ADeclaredAgentWithoutOccupancyOrOffItsHorizonGetsAgentOccupancyUnavailable()
        {
            var model = Model();
            var wide = new VehicleFootprint { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 10.5f, RightMeters = 10.5f };
            var other = new PublishedIntentHorizon(0, new[] { new IntentInterval(RoadElementKind.JunctionMovement, MovementMR, 0f, 15f) });
            var frame = new TrafficFrame(1, model, new[]
            {
                Actor(model, 1, MovementMR, 3f, wide), Actor(model, 2, CorridorNB2, 5f, horizon: other), Actor(model, 3, CorridorN2, 12f)
            });
            var unbounded = Observe(frame, 1, Horizon(frame, 1, PortalExitEB));
            AssertBodyChannels(unbounded, PerceptionStatus.AgentOccupancyUnavailable);
            Assert.That(unbounded.Exit.Status, Is.EqualTo(PerceptionStatus.Evaluated));
            Assert.That(unbounded.IntentOverlaps.Status, Is.EqualTo(PerceptionStatus.Evaluated));
            Assert.That(unbounded.IntentOverlaps.Items.Single().OtherTrafficId, Is.EqualTo(Vehicle(2)));

            // Horizon d'un autre element : aucune distance n'est calculee depuis une occupation qui n'y est pas.
            var offHorizon = Observe(frame, 2, Horizon(frame, 3, PortalExitN2));
            AssertBodyChannels(offHorizon, PerceptionStatus.AgentOccupancyUnavailable);
        }

        private static void AssertBodyChannels(AgentObservation observation, PerceptionStatus status)
        {
            Assert.That(observation.Leader.Status, Is.EqualTo(status));
            Assert.That(observation.Follower.Status, Is.EqualTo(status));
            Assert.That(observation.Adjacent.Status, Is.EqualTo(status));
            Assert.That(observation.Obstacles.Status, Is.EqualTo(status));
            Assert.That(observation.Leader.Items.Concat(observation.Follower.Items).Count()
                + observation.Adjacent.Items.Count + observation.Obstacles.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheFollowerIsSearchedOnTheDirectPredecessorAndPublishesTheEffectiveRange()
        {
            var model = Model();
            var frame = new TrafficFrame(1, model, new[] { Actor(model, 1, MovementMS, 4f), Actor(model, 2, CorridorNB2, 27f, speed: 5f) });
            var horizon = Horizon(frame, 1, PortalExitN2);
            var follower = Observe(frame, 1, horizon).Follower;
            float bumper = (4f - 2.22f) + 30f - (27f + 2.22f);
            Assert.That(follower.Items.Single().TrafficId, Is.EqualTo(Vehicle(2)));
            Assert.That(follower.Items.Single().GapMeters, Is.InRange(bumper - StraightSlack, bumper + 1e-3f));
            Assert.That(follower.RangeMeters, Is.EqualTo(Limits.RearRangeMeters), "un element en amont couvre deja la portee");

            var far = Observe(frame, 1, horizon, limits: new PerceptionLimits(80f, 3f, 15f, 8)).Follower;
            Assert.That(far.RangeMeters, Is.InRange(4f - 2.22f + 30f - StraightSlack, 4f - 2.22f + 30f + 1e-3f),
                "portee effectivement cherchee : un seul element en amont");
            Assert.That(far.Items.Single().Metadata.RangeMeters, Is.EqualTo(far.RangeMeters));
        }

        [Test]
        public void AnOverhangBeforeTheElementStartShortensTheGapAndTheExitFreeLength()
        {
            var model = Model();
            var frame = new TrafficFrame(1, model, new[]
            {
                Actor(model, 1, CorridorNB2, 20f), Actor(model, 2, MovementMS, 1f), Actor(model, 3, CorridorN2, 1f)
            });
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            var leader = observation.Leader.Items.Single();
            Assert.That(leader.TrafficId, Is.EqualTo(Vehicle(2)));
            float bumper = 10f + (1f - 2.22f) - 2.22f;
            Assert.That(leader.GapMeters, Is.InRange(bumper - StraightSlack, bumper + 1e-3f), "l'arriere deborde sur NB2");
            Assert.That(observation.Exit.Items.Single().FreeLengthMeters, Is.InRange(1f - 2.22f - StraightSlack, 1f - 2.22f + 1e-3f));
        }

        [Test]
        public void ExclusionFiltersDropWhatIsOutsideTheSearchedRanges()
        {
            var model = Model();
            var disjoint = new PublishedIntentHorizon(0, new[] { new IntentInterval(RoadElementKind.LaneCorridor, CorridorNB2, 0f, 3f) });
            var hazards = new[]
            {
                Hazard(1, TrafficHazardKind.Pedestrian, new Vector3(6f, 6f, 20f), new Vector3(0.3f, 0.9f, 0.3f)),
                Hazard(2, TrafficHazardKind.Pedestrian, new Vector3(6f, 0.9f, 10.5f), new Vector3(0.3f, 0.9f, 0.3f)),
                Hazard(3, TrafficHazardKind.Pedestrian, new Vector3(8.5f, 0.9f, 14f), new Vector3(0.3f, 0.9f, 0.3f))
            };
            var frame = new TrafficFrame(1, model, new[]
            {
                Actor(model, 1, CorridorNB2, 15f), Actor(model, 2, CorridorNB, 29f), Actor(model, 3, CorridorNB, 20f),
                Actor(model, 4, CorridorSB, 28f, horizon: disjoint)
            }, hazards);
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2), limits: new PerceptionLimits(30f, 3f, 5f, 8));
            Assert.That(observation.Adjacent.Items.Select(x => x.TrafficId), Is.EqualTo(new[] { Vehicle(3) }), "hors fenetre : absent");
            var ids = observation.Obstacles.Items.Select(x => x.Id).ToList();
            Assert.That(ids, Has.No.Member(Vehicle(1001)), "autre etage");
            Assert.That(ids, Has.No.Member(Vehicle(1002)), "derriere l'arriere de l'agent");
            Assert.That(ids, Has.Member(Vehicle(1003)), "a cote de l'arriere de l'agent");
            Assert.That(observation.IntentOverlaps.Items, Is.Empty, "intervalle disjoint sur le meme element");
        }

        // ================================================================== dangers et index spatial

        [Test]
        public void WalkingPlayersAreHazardsOnTheSweptPathAndBesideIt()
        {
            var model = Model();
            var hazards = new[]
            {
                Hazard(1, TrafficHazardKind.WalkingPlayer, new Vector3(6.3f, 0.9f, 20f), new Vector3(0.3f, 0.9f, 0.3f)),
                Hazard(2, TrafficHazardKind.WalkingPlayer, new Vector3(9.5f, 0.9f, 20f), new Vector3(0.3f, 0.9f, 0.3f), 0.8f),
                Hazard(3, TrafficHazardKind.Pedestrian, new Vector3(9.5f, 0.9f, 12f), new Vector3(0.3f, 0.9f, 0.3f)),
                Hazard(4, TrafficHazardKind.Obstacle, new Vector3(9.5f, 0.5f, 14f), new Vector3(0.5f, 0.5f, 0.5f)),
                Hazard(5, TrafficHazardKind.Vehicle, new Vector3(10f, 0.7f, 17f), new Vector3(1f, 0.7f, 2f))
            };
            var frame = new TrafficFrame(3, model, new[] { Actor(model, 1, CorridorNB2, 5f), Actor(model, 2, CorridorNB2, 25f) }, hazards);
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            var onPath = observation.Obstacles.Items.Single(x => x.Id == Vehicle(1001));
            var beside = observation.Obstacles.Items.Single(x => x.Id == Vehicle(1002));
            Assert.That(onPath.Kind, Is.EqualTo(PerceivedObstacleKind.WalkingPlayer));
            Assert.That(onPath.InSweptPath, Is.True);
            Assert.That(beside.InSweptPath, Is.False);
            Assert.That(onPath.NearDistanceMeters, Is.InRange(15f - 0.3f - 2.22f - StraightSlack, 15f - 0.3f - 2.22f + 1e-3f));
            Assert.That(beside.Metadata.Confidence, Is.EqualTo(0.8f));
            var kinds = observation.Obstacles.Items.ToDictionary(x => x.Id, x => x.Kind);
            Assert.That(kinds[Vehicle(1003)], Is.EqualTo(PerceivedObstacleKind.Pedestrian));
            Assert.That(kinds[Vehicle(1004)], Is.EqualTo(PerceivedObstacleKind.Obstacle));
            Assert.That(kinds[Vehicle(1005)], Is.EqualTo(PerceivedObstacleKind.Vehicle));
            AssertCode("InvalidHazard", () => new TrafficFrame(1, model, new TrafficActorInput[0], new[]
                { new TrafficHazardInput(Vehicle(1), (TrafficHazardKind)7, Box(Vector3.one, Vector3.one), Vector3.zero, 1f) }));
            Assert.That(onPath.Metadata.Source, Is.EqualTo(ObservationSource.SpatialQuery));
            Assert.That(observation.Obstacles.Items.Any(x => x.Id == Vehicle(2)), Is.False, "le leader structure n'est pas un obstacle");
        }

        [Test]
        public void AnUnlocalizedVehicleIsAnObstacleExactlyOnceAndCannotAlsoBeAHazard()
        {
            var model = Model();
            // Caisse longue poussee hors chaussee, travers a la route : reference a 2,8 m de l'enveloppe, avant sur le chemin.
            var pushed = new TrafficActorInput(Vehicle(9), new VehicleFootprintPose
            {
                Position = new Vector3(10.8f, 0f, 20f), Forward = Vector3.left, Up = Vector3.up,
                Footprint = new VehicleFootprint { FrontMeters = 5f, RearMeters = 1f, LeftMeters = 1.03f, RightMeters = 1.03f }
            }, 1.5f, RoadId.None);
            var hazards = new[] { Hazard(1, TrafficHazardKind.Pedestrian, new Vector3(6f, 0.9f, 15f), new Vector3(0.3f, 0.9f, 0.3f)) };
            var frame = new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f), pushed, Actor(model, 2, CorridorNB2, 26f) }, hazards);
            TrafficActor actor;
            frame.TryGetActor(Vehicle(9), out actor);
            Assert.That(actor.Location.Localized, Is.False);
            Assert.That(actor.OccupancyExclusion, Is.EqualTo(OccupancyExclusion.NotLocalized));

            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            var obstacle = observation.Obstacles.Items.Single(x => x.Id == Vehicle(9));
            Assert.That(obstacle.Kind, Is.EqualTo(PerceivedObstacleKind.TrafficActor));
            Assert.That(obstacle.InSweptPath, Is.True);
            Assert.That(obstacle.Metadata.Confidence, Is.EqualTo(0f), "min des localisations : l'acteur pousse n'est pas localise");
            Assert.That(Vector3.Distance(obstacle.Velocity, Vector3.left * 1.5f), Is.LessThan(1e-4f));
            Assert.That(observation.Leader.Items.Single().TrafficId, Is.EqualTo(Vehicle(2)));
            var ids = observation.Obstacles.Items.Select(x => x.Id).ToList();
            Assert.That(ids, Is.Unique);

            var duplicate = new[] { new TrafficHazardInput(Vehicle(9), TrafficHazardKind.Vehicle,
                Box(new Vector3(10f, 0.7f, 20f), new Vector3(1f, 0.7f, 3f)), Vector3.zero, 1f) };
            AssertCode("DuplicateIdentity", () => new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f), pushed }, duplicate));
        }

        [Test]
        public void SaturationIsReportedForTheQueryBufferAndForTheFactList()
        {
            var model = Model();
            var hazards = Enumerable.Range(1, 12).Select(i =>
                Hazard(i, TrafficHazardKind.Obstacle, new Vector3(6f, 0.5f, 6f + 2f * i), new Vector3(0.2f, 0.5f, 0.2f))).ToArray();
            var frame = new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 2f), Actor(model, 2, CorridorSB, 25f) }, hazards);
            var horizon = Horizon(frame, 1, PortalExitN2);

            var small = Observe(frame, 1, horizon, capacity: 4);
            Assert.That(small.SpatialQuerySaturated, Is.True);
            Assert.That(small.SpatialQueryTotal, Is.GreaterThanOrEqualTo(12));
            Assert.That(small.Obstacles.Saturated, Is.True);

            var listed = Observe(frame, 1, horizon);
            Assert.That(listed.SpatialQuerySaturated, Is.False);
            Assert.That(listed.Obstacles.Total, Is.EqualTo(12));
            Assert.That(listed.Obstacles.Items.Count, Is.EqualTo(Limits.ListCapacity));
            Assert.That(listed.Obstacles.Saturated, Is.True);
            Assert.That(listed.Obstacles.Items.Select(x => x.Id),
                Is.EqualTo(Enumerable.Range(1, 8).Select(i => Vehicle(1000 + i))), "les plus proches sont gardes");
        }

        // ================================================================== voisinage structure

        [Test]
        public void AdjacentLaneOccupantsAreReportedWithSideAndSignedGap()
        {
            var model = Model();
            var frame = new TrafficFrame(1, model, new[]
            {
                Actor(model, 1, CorridorNB2, 10f), Actor(model, 2, CorridorNB, 20f), Actor(model, 3, CorridorNB, 2f),
                Actor(model, 4, CorridorNB, 11f)
            });
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            var facts = observation.Adjacent.Items;
            Assert.That(facts.Select(x => x.TrafficId), Is.EqualTo(new[] { Vehicle(4), Vehicle(3), Vehicle(2) }));
            Assert.That(facts.All(x => x.Side == LaneSide.Left && x.CorridorId == CorridorNB), Is.True);
            Assert.That(facts[0].LongitudinalGapMeters, Is.EqualTo(0f), "a hauteur");
            Assert.That(facts[1].LongitudinalGapMeters, Is.LessThan(0f), "derriere");
            Assert.That(facts[2].LongitudinalGapMeters, Is.InRange(5.56f - StraightSlack, 5.56f + 1e-3f), "devant");
            Assert.That(observation.Leader.Items, Is.Empty, "une voie adjacente ne donne pas de leader");
        }

        [Test]
        public void ExitOccupancyCountsEveryMeasuredOccupantAndListsTheUnmeasuredOne()
        {
            var model = Model();
            var frame = new TrafficFrame(1, model, new[]
            {
                Actor(model, 1, CorridorNB2, 5f), Actor(model, 2, CorridorN2, 20f), Actor(model, 3, CorridorN2, 8f),
                Actor(model, 4, CorridorN2, 14f, default(VehicleFootprint))
            });
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            var exit = observation.Exit.Items.Single();
            Assert.That(exit.MovementId, Is.EqualTo(MovementMS));
            Assert.That(exit.ExitCorridorId, Is.EqualTo(CorridorN2));
            Assert.That(exit.OccupantCount, Is.EqualTo(2));
            Assert.That(exit.FirstOccupantId, Is.EqualTo(Vehicle(3)));
            Assert.That(exit.FreeLengthMeters, Is.InRange(8f - 2.22f - StraightSlack, 8f - 2.22f + 1e-3f));
            Assert.That(observation.UnmeasuredActors.Items.Select(x => x.TrafficId), Is.EqualTo(new[] { Vehicle(4) }));

            var empty = new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f) });
            var free = Observe(empty, 1, Horizon(empty, 1, PortalExitN2)).Exit.Items.Single();
            Assert.That(free.OccupantCount, Is.EqualTo(0));
            Assert.That(free.FreeLengthMeters, Is.EqualTo(30f).Within(1e-3f));
        }

        [Test]
        public void IntentPathOverlapIsSpatialOnSharedElementsAndConflictZones()
        {
            var model = Model();
            var crossing = new PublishedIntentHorizon(4, new[]
            {
                new IntentInterval(RoadElementKind.LaneCorridor, CorridorNB2, 0f, 30f),
                new IntentInterval(RoadElementKind.JunctionMovement, MovementMS, 0f, 10f)
            });
            var elsewhere = new PublishedIntentHorizon(4, new[] { new IntentInterval(RoadElementKind.LaneCorridor, CorridorN2, 0f, 30f) });
            // Horizon publie sur MS, mais tronque apres la zone de conflit : aucun recouvrement de zone.
            var pastZone = new PublishedIntentHorizon(4, new[] { new IntentInterval(RoadElementKind.JunctionMovement, MovementMS, 9.5f, 10f) });
            var frame = new TrafficFrame(5, model, new[]
            {
                Actor(model, 1, CorridorNB2, 5f), Actor(model, 2, CorridorNB, 3f, horizon: crossing),
                Actor(model, 3, CorridorN2, 25f, horizon: elsewhere), Actor(model, 4, CorridorSB, 20f, horizon: pastZone)
            });
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitEB));
            var overlaps = observation.IntentOverlaps.Items;
            Assert.That(overlaps.Count, Is.EqualTo(2));
            Assert.That(overlaps[0].Kind, Is.EqualTo(IntentOverlapKind.SameElement));
            Assert.That(overlaps[0].SharedId, Is.EqualTo(CorridorNB2));
            Assert.That(overlaps[0].AgentDistanceMeters, Is.EqualTo(0f).Within(1e-3f));
            Assert.That(overlaps[0].OtherDistanceMeters, Is.EqualTo(5f).Within(1e-3f));
            Assert.That(overlaps[1].Kind, Is.EqualTo(IntentOverlapKind.ConflictZone));
            Assert.That(overlaps[1].SharedId, Is.EqualTo(ZoneZ));
            Assert.That(overlaps[1].AgentDistanceMeters, Is.EqualTo(25f).Within(1e-3f));
            Assert.That(overlaps[1].OtherDistanceMeters, Is.EqualTo(30f).Within(1e-3f));
            Assert.That(overlaps.All(x => x.OtherTrafficId == Vehicle(2)), Is.True);
            Assert.That(overlaps.All(x => x.Metadata.TimestampFrameId == 4UL && x.Metadata.Source == ObservationSource.PublishedHorizon), Is.True);
        }

        // ================================================================== frame : signaux, fermetures, validation

        [Test]
        public void SignalsAndClosuresAreSnapshotInTheFrame()
        {
            var model = Model(signalized: true);
            var frame = new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f) }, null,
                new[] { new SignalPhaseInput(PlanP, PhaseStraightGreen) },
                new[] { new ElementClosureInput(CorridorEB, "Works") });
            SignalState state;
            Assert.That(frame.TryGetSignalState(MovementMS, out state) && state == SignalState.Green, Is.True);
            Assert.That(frame.TryGetSignalState(MovementMR, out state) && state == SignalState.Red, Is.True);
            Assert.That(frame.IsClosed(CorridorEB), Is.True);
            Assert.That(frame.IsClosed(CorridorN2), Is.False);
            AssertCode("UnknownSignalPhase", () => new TrafficFrame(1, model, new TrafficActorInput[0], null,
                new[] { new SignalPhaseInput(PlanP, Id(98)) }));
            AssertCode("UnknownSignalPlan", () => new TrafficFrame(1, model, new TrafficActorInput[0], null,
                new[] { new SignalPhaseInput(Id(99), PhaseStraightGreen) }));
            AssertCode("DuplicateSignalPlan", () => new TrafficFrame(1, model, new TrafficActorInput[0], null,
                new[] { new SignalPhaseInput(PlanP, PhaseStraightGreen), new SignalPhaseInput(PlanP, PhaseRightGreen) }));
        }

        [Test]
        public void InvalidInputsAreRefusedWithNamedCodes()
        {
            var model = Model();
            var a = Actor(model, 1, CorridorNB2, 5f);
            var empty = new TrafficActorInput[0];
            AssertCode("FutureIntentHorizon", () => new TrafficFrame(3, model, new[] { Actor(model, 1, CorridorNB2, 5f,
                horizon: new PublishedIntentHorizon(3, new IntentInterval[0])) }));
            AssertCode("UnknownElement", () => new TrafficFrame(3, model, new[] { Actor(model, 1, CorridorNB2, 5f,
                horizon: new PublishedIntentHorizon(2, new[] { new IntentInterval(RoadElementKind.LaneCorridor, Id(99), 0f, 1f) })) }));
            AssertCode("InvalidIntentInterval", () => new TrafficFrame(3, model, new[] { Actor(model, 1, CorridorNB2, 5f,
                horizon: new PublishedIntentHorizon(2, new[] { new IntentInterval(RoadElementKind.LaneCorridor, CorridorNB2, 5f, 1f) })) }));
            AssertCode("DuplicateTrafficId", () => new TrafficFrame(1, model, new[] { a, a }));
            AssertCode("InvalidSpeed", () => new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f, speed: float.NaN) }));
            AssertCode("InvalidFootprint", () => new TrafficFrame(1, model, new[] { Actor(model, 1, CorridorNB2, 5f,
                new VehicleFootprint { FrontMeters = -1f, RearMeters = 1f, LeftMeters = 1f, RightMeters = 1f }) }));
            var player = Hazard(1, TrafficHazardKind.WalkingPlayer, Vector3.one, Vector3.one);
            AssertCode("DuplicateHazardId", () => new TrafficFrame(1, model, empty, new[] { player, player }));
            AssertCode("EmptyHazardId", () => new TrafficFrame(1, model, empty,
                new[] { new TrafficHazardInput(RoadId.None, TrafficHazardKind.Obstacle, Box(Vector3.one, Vector3.one), Vector3.zero, 1f) }));
            AssertCode("InvalidHazard", () => new TrafficFrame(1, model, empty,
                new[] { Hazard(1, TrafficHazardKind.Obstacle, new Vector3(float.NaN, 0f, 0f), Vector3.one) }));
            AssertCode("InvalidConfidence", () => new TrafficFrame(1, model, empty,
                new[] { Hazard(1, TrafficHazardKind.Obstacle, Vector3.one, Vector3.one, 2f) }));
            AssertCode("UnknownElement", () => new TrafficFrame(1, model, empty, null, null, new[] { new ElementClosureInput(Id(99), "Works") }));
            AssertCode("EmptyClosureReason", () => new TrafficFrame(1, model, empty, null, null, new[] { new ElementClosureInput(CorridorEB, "") }));
            AssertCode("DuplicateClosure", () => new TrafficFrame(1, model, empty, null, null,
                new[] { new ElementClosureInput(CorridorEB, "Works"), new ElementClosureInput(CorridorEB, "Crash") }));
            AssertCode("InvalidPerceptionLimits", () => new PerceptionLimits(0f, 3f, 15f, 8));
            AssertCode("InvalidPerceptionLimits", () => new PerceptionLimits(30f, 3f, 15f, 0));
            var frame = new TrafficFrame(1, model, new[] { a });
            var horizon = Horizon(frame, 1, PortalExitN2);
            AssertCode("InvalidPerceptionLimits", () => TrafficPerception.Observe(frame, Vehicle(1), horizon,
                default(PerceptionLimits), new SpatialQueryBuffer(4)));
            AssertCode("UnknownTrafficId", () => TrafficPerception.Observe(frame, Vehicle(77), horizon, Limits, new SpatialQueryBuffer(4)));
        }

        [Test]
        public void AFrameWithoutTheNewInputsKeepsThe530Behaviour()
        {
            var model = Model();
            var input = new TrafficActorInput(Vehicle(1), Pose(model, CorridorNB2, 12f, default(VehicleFootprint)), 2f, CorridorNB2);
            var frame = new TrafficFrame(1, model, new[] { input });
            var expected = RoadLocalizer.Localize(model, input.Pose, CorridorNB2, null);
            var actor = frame.Actors.Single();
            Assert.That(actor.Location.ElementId, Is.EqualTo(expected.ElementId));
            Assert.That(actor.Location.SMeters, Is.EqualTo(expected.SMeters));
            Assert.That(actor.Location.Confidence, Is.EqualTo(expected.Confidence));
            Assert.That(actor.TangentialSpeedMetersPerSecond, Is.EqualTo(2f));
            Assert.That(frame.Hazards, Is.Empty);
            Assert.That(frame.SignalPhases, Is.Empty);
            Assert.That(frame.Closures, Is.Empty);
            Assert.That(frame.GetOccupants(CorridorNB2), Is.Empty);
            Assert.That(actor.PublishedHorizon, Is.Null);
            var minimal = new AgentObservation(frame.FrameId, actor);
            Assert.That(minimal.Perceived, Is.False);
            Assert.That(minimal.Leader, Is.Null);
        }

        // ================================================================== vue partagee, determinisme, absence d'etat

        private static TrafficActorInput[] Crowd(CompiledRoadModel model)
        {
            return new[]
            {
                Actor(model, 1, CorridorNB2, 5f, speed: 3f), Actor(model, 2, CorridorNB2, 18f, speed: 2f),
                Actor(model, 3, CorridorNB, 9f, speed: 1f), Actor(model, 4, MovementMS, 4f),
                Actor(model, 5, CorridorN2, 12f), Actor(model, 6, CorridorNB2, 27f, default(VehicleFootprint))
            };
        }

        private static TrafficHazardInput[] CrowdHazards()
        {
            return new[]
            {
                Hazard(1, TrafficHazardKind.WalkingPlayer, new Vector3(6.5f, 0.9f, 14f), new Vector3(0.3f, 0.9f, 0.3f)),
                Hazard(2, TrafficHazardKind.Pedestrian, new Vector3(8.6f, 0.9f, 50f), new Vector3(0.3f, 0.9f, 0.3f))
            };
        }

        [Test]
        public void ShuffledInputsProduceTheSameFrameAndTheSameObservations()
        {
            var model = Model();
            var actors = Crowd(model);
            var hazards = CrowdHazards();
            var ordered = new TrafficFrame(9, model, actors, hazards);
            var shuffled = new TrafficFrame(9, model, actors.Reverse().ToArray(), hazards.Reverse().ToArray());
            Assert.That(shuffled.Actors.Select(x => x.TrafficId), Is.EqualTo(ordered.Actors.Select(x => x.TrafficId)));
            Assert.That(shuffled.Hazards.Select(x => x.Id), Is.EqualTo(ordered.Hazards.Select(x => x.Id)));
            foreach (int agent in new[] { 1, 2, 4, 5 })
                Assert.That(Observe(shuffled, agent, Horizon(shuffled, agent, PortalExitN2)).ToText(),
                    Is.EqualTo(Observe(ordered, agent, Horizon(ordered, agent, PortalExitN2)).ToText()), "agent " + agent);
        }

        [Test]
        public void SuccessiveObservationsFromOneFrameLeakNoStateAndLeaveTheFrameUntouched()
        {
            var model = Model();
            var frame = new TrafficFrame(9, model, Crowd(model), CrowdHazards());
            var agents = new[] { 1, 2, 4, 5 };
            var horizons = agents.ToDictionary(a => a, a => Horizon(frame, a, PortalExitN2));
            var alone = agents.ToDictionary(a => a, a => Observe(frame, a, horizons[a]).ToText());
            string before = Snapshot(frame);

            var shared = new SpatialQueryBuffer(32);
            foreach (var order in new[] { agents, agents.Reverse().ToArray() })
                foreach (int agent in order)
                    Assert.That(TrafficPerception.Observe(frame, Vehicle(agent), horizons[agent], Limits, shared).ToText(),
                        Is.EqualTo(alone[agent]), "agent " + agent);
            Assert.That(Snapshot(frame), Is.EqualTo(before));

            var first = TrafficPerception.Observe(frame, Vehicle(1), horizons[1], Limits, shared);
            Assert.That(first.Leader.Items.Single().TrafficId, Is.EqualTo(Vehicle(2)));
            Assert.That(first.Obstacles.Items.Single(x => x.Id == Vehicle(1001)).InSweptPath, Is.True);
            // Le voisin de la voie adjacente est aussi un fait spatial, hors du couloir balaye.
            Assert.That(first.Obstacles.Items.Single(x => x.Id == Vehicle(3)).InSweptPath, Is.False);
            Assert.That(first.Obstacles.Items.Select(x => x.Id), Is.Unique);
            Assert.That(first.Adjacent.Items.Single().TrafficId, Is.EqualTo(Vehicle(3)));
            Assert.That(first.UnmeasuredActors.Items.Single().TrafficId, Is.EqualTo(Vehicle(6)));
            var second = TrafficPerception.Observe(frame, Vehicle(2), horizons[2], Limits, shared);
            Assert.That(second.Follower.Items.Single().TrafficId, Is.EqualTo(Vehicle(1)));
            Assert.That(second.Leader.Items.Single().TrafficId, Is.EqualTo(Vehicle(4)));
        }

        private static string Snapshot(TrafficFrame frame)
        {
            var text = new System.Text.StringBuilder();
            foreach (var actor in frame.Actors)
            {
                text.Append(actor.TrafficId).Append(' ').Append(actor.Location.ElementId).Append(' ')
                    .Append(actor.Location.SMeters.ToString("R")).Append(' ').Append(actor.OccupancyExclusion).Append('\n');
                ElementOccupant occupant;
                if (frame.TryGetOccupancy(actor.TrafficId, out occupant))
                    text.Append(occupant.SMinMeters.ToString("R")).Append(' ').Append(occupant.SMaxMeters.ToString("R")).Append('\n');
            }
            foreach (var hazard in frame.Hazards) text.Append(hazard.Id).Append(hazard.Bounds.Center).Append('\n');
            return text.ToString();
        }

        [Test]
        public void TheFrameAndTheObservationsExposeNothingWritable()
        {
            var types = new[]
            {
                typeof(TrafficFrame), typeof(TrafficActor), typeof(TrafficActorInput), typeof(ElementOccupant), typeof(SpatialEntry),
                typeof(PublishedIntentHorizon), typeof(IntentInterval), typeof(TrafficHazardInput), typeof(SignalPhaseInput),
                typeof(ElementClosureInput), typeof(AgentObservation), typeof(ObservationChannel<VehicleGapFact>),
                typeof(VehicleGapFact), typeof(AdjacentOccupantFact), typeof(ObstacleFact), typeof(IntentPathOverlapFact),
                typeof(ExitOccupancyFact), typeof(UnmeasuredActorFact), typeof(ObservationMetadata), typeof(PerceptionLimits)
            };
            foreach (var type in types)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    Assert.That(field.IsInitOnly, Is.True, type.Name + "." + field.Name);
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    Assert.That(property.SetMethod == null || !property.SetMethod.IsPublic, Is.True, type.Name + "." + property.Name);
            }

            var model = Model();
            var frame = new TrafficFrame(9, model, Crowd(model), CrowdHazards(), null, new[] { new ElementClosureInput(CorridorEB, "Works") });
            var observation = Observe(frame, 1, Horizon(frame, 1, PortalExitN2));
            foreach (var list in new IEnumerable[] { frame.Actors, frame.Hazards, frame.SignalPhases, frame.Closures,
                frame.GetOccupants(CorridorNB2), observation.Leader.Items, observation.Obstacles.Items, observation.Exit.Items })
                Assert.That(((IList)list).IsReadOnly, Is.True);
        }

        [Test]
        public void ObservationsAreCompactFactsNotCopiesOfTheFrame()
        {
            var forbidden = new[] { typeof(TrafficFrame), typeof(TrafficActor), typeof(ElementOccupant), typeof(SpatialEntry) };
            var perceptionTypes = typeof(AgentObservation).Assembly.GetTypes()
                .Where(t => t.Namespace == typeof(AgentObservation).Namespace && t.IsPublic).ToList();
            foreach (var type in perceptionTypes.Where(t => t != typeof(TrafficPerception)))
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    foreach (var bad in forbidden)
                    {
                        Assert.That(field.FieldType, Is.Not.EqualTo(bad), type.Name + "." + field.Name);
                        Assert.That(typeof(IEnumerable<>).MakeGenericType(bad).IsAssignableFrom(field.FieldType), Is.False,
                            type.Name + "." + field.Name);
                    }
        }

        [Test]
        public void FactsStateNoDecision()
        {
            var words = new[] { "RightOfWay", "Priority", "Grant", "Yield", "Pedal", "Brake", "Throttle", "Steer", "Allowed" };
            var types = typeof(AgentObservation).Assembly.GetTypes().Where(t => t.IsPublic
                && (t.Namespace == typeof(AgentObservation).Namespace || t.Namespace == typeof(TrafficFrame).Namespace)).ToList();
            Assert.That(types.Count, Is.GreaterThan(10));
            foreach (var type in types)
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Select(m => m.Name).Concat(type.IsEnum ? Enum.GetNames(type) : new string[0]).Concat(new[] { type.Name }))
                {
                    foreach (var word in words)
                        Assert.That(member, Does.Not.Contain(word), type.Name);
                    Assert.That(member, Does.Not.Match("(?i)(waypoint|target.*index)"), type.Name);
                }
        }

        [Test]
        public void FrameAndPerceptionSourcesUseNoPhysicsNorControlPath()
        {
            var files = new List<string>();
            foreach (var folder in new[] { "Frame", "Perception" })
                files.AddRange(Directory.GetFiles(Path.Combine(TrafficRootPath, folder), "*.cs"));
            Assert.That(files.Count, Is.GreaterThanOrEqualTo(4));
            var forbidden = new[] { "VehicleDriveIntent", "VehiclePhysicsBody", "ApplyDriveIntent", "Rigid" + "body", "Mono" + "Behaviour",
                "NetworkVariable", "Rp" + "c", "LateralClearanceMarginMeters", "Physics" + ".", "Sphere" + "Cast", "Trans" + "form " };
            foreach (var file in files)
            {
                string source = File.ReadAllText(file);
                foreach (var word in forbidden) Assert.That(source, Does.Not.Contain(word), file);
            }
        }
    }
}
