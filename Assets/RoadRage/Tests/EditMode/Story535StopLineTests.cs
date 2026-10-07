using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.35, phase 4 : frontiere de controle b sur frames reelles de MVP_Run, branche de TJunction_West (Yield avec
    /// ligne). La distance de demande vise la ligne, mesuree au pare-chocs de reference (continue au basculement corridor ->
    /// mouvement courbe), le vehicule n'occupe pas son mouvement avant b, un refus l'arrete dans la
    /// bande declaree devant la ligne (au-dela de l'entree generique, et non avant elle) avec le pire cas point-masse 5.34
    /// (latence du pipeline a pleine acceleration), puis le grant le fait repartir sans freinage. Le vehicule seul sur les
    /// 11 routes reste couvert par Story534JunctionEntryTests, rejoue sur le modele authore.
    /// </summary>
    [Category("Core")]
    [Category("Story535")]
    public sealed class Story535StopLineTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const float Dt = 0.02f;
        /// <summary>Tolerance d'integration sous m_ctrl (m), celle de la bande 5.34 (O13).</summary>
        private const float StopBandIntegrationTolerance = 0.02f;
        /// <summary>Ecart admis entre d publie et la distance nominale a la ligne (pose nominale relocalisee, m).</summary>
        private const float LocalizationTolerance = 0.05f;

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };
        private static readonly RoadId Agent = new RoadId(0x535EUL, 1UL);
        /// <summary>FromSouth a gauche de TJunction_West (branche Yield avec ligne).</summary>
        private static readonly RoadId SouthLeft = RoadId.Parse("4e437f94525852d3a072a538537c2093");

        private static float Margin { get { return TrafficV2Settings.JunctionStopControlMarginMeters; } }
        private static float HoldEntrySpeed { get { return TrafficV2Settings.StopHold.EntrySpeedMetersPerSecond; } }

        private static TrafficV2Admission admission;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                        File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
                Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
                return admission;
            }
        }

        private static CompiledRoadModel Model { get { return Admission.Model; } }
        private static JunctionConflictIndex Index { get { return JunctionConflictIndex.For(Model); } }

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; }
        }

        private static JunctionDistances DistancesAt(float speed)
        {
            return JunctionDistances.For(Driver, speed, Dt, Margin, HoldEntrySpeed);
        }

        private static float BandLow { get { return Margin - StopBandIntegrationTolerance; } }
        private static float BandHigh { get { return DistancesAt(0f).HoldWindowMeters; } }

        /// <summary>Mouvements de branche de TJunction_West portant une ligne, avec leur b.</summary>
        private static List<KeyValuePair<RoadId, float>> BranchLines()
        {
            var lines = new List<KeyValuePair<RoadId, float>>();
            foreach (var movement in Model.GetMovementsInJunction(Index.JunctionOf(SouthLeft)).OrderBy(m => m))
            {
                float b;
                if (Model.TryGetStopLine(movement, out b)) lines.Add(new KeyValuePair<RoadId, float>(movement, b));
            }
            Assert.That(lines.Count, Is.EqualTo(2), "branche : tourne-a-droite et tourne-a-gauche");
            Assert.That(lines.Any(l => l.Key == SouthLeft), Is.True);
            foreach (var line in lines)
            {
                Assert.That(Index.ControlKindOf(line.Key), Is.EqualTo(JunctionControlKind.Yield));
                Assert.That(Index.BoundaryOf(line.Key), Is.EqualTo(line.Value));
                Assert.That(line.Value, Is.GreaterThan(BandHigh), "la ligne est au-dela de la bande d'arret de l'entree generique");
            }
            return lines;
        }

        private static RoutePlan RouteVia(RoadId movement)
        {
            var start = new RoadLocation { ModelId = Model.ModelId, ModelVersion = Model.Version, Localized = true,
                ElementKind = RoadElementKind.LaneCorridor, ElementId = Index.FromCorridorOf(movement), SMeters = 0f };
            var result = RoutePlanner.Plan(new RouteRequest(Model, start, RoadId.None, new RouteSeed(0), Agent, "route",
                new DecisionCounter(0), null, false, null, movement));
            Assert.That(result.Plan, Is.Not.Null, result.Reason.ToString());
            return result.Plan;
        }

        private static float EntryDistance(RoutePlan route, RoadId movement)
        {
            float entry = 0f;
            for (int k = 0; route.Occurrences[k].Id != movement; k++)
                entry += route.Occurrences[k].EndSMeters - route.Occurrences[k].StartSMeters;
            return entry;
        }

        /// <summary>Rapport de l'acteur seul, centre a <paramref name="center"/> m du debut de sa route, a la vitesse donnee.</summary>
        private static JunctionActorReport ReportAt(RoutePlan route, ReferenceTrack track, float center, float speed, ulong frameId)
        {
            int piece = track.PieceAt(center);
            var nominal = track.Nominal(piece, center);
            var actor = new TrafficActorInput(Agent, new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward,
                Up = nominal.Up, Footprint = Car }, speed, track.Pieces[piece].Id, TrafficV2Lifecycle.ExpectedElements(route),
                track.KinematicAnchors(piece));
            var frame = new TrafficFrame(frameId, Model, new[] { actor });
            return JunctionRequestBuilder.Build(frame, Index, Agent, route, Driver, Dt, Margin, JunctionSnapshot.Initial(frameId), frameId,
                HoldEntrySpeed);
        }

        private static JunctionEntryInput EntryOf(JunctionActorReport report, bool grant)
        {
            return report.HasRequest
                ? new JunctionEntryInput(report.Request.Traversal.FirstMovementId, report.Request.DistanceMeters,
                    report.Request.Distances.EngageThresholdMeters, Margin, grant, report.Request.Distances.StopMeters)
                : default(JunctionEntryInput);
        }

        // ================================================================== banc d'arbitrage (plan a v0, comme 5.34)

        private static SpeedPlan freePlan;

        private static SpeedPlan FreePlan
        {
            get
            {
                if (freePlan != null) return freePlan;
                var straight = Model.Movements.Where(m => m.Samples.All(s => Math.Abs(s.CurvaturePerMeter) < 1e-4f)).OrderBy(m => m.Id).First();
                var route = RouteVia(straight.Id);
                var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
                int piece = track.PieceAt(1f);
                var nominal = track.Nominal(piece, 1f);
                var frame = new TrafficFrame(1, Model, new[] { new TrafficActorInput(Agent, new VehicleFootprintPose { Position = nominal.Position,
                    Forward = nominal.Forward, Up = nominal.Up, Footprint = Car }, Driver.DesiredSpeed, track.Pieces[piece].Id,
                    TrafficV2Lifecycle.ExpectedElements(route), track.KinematicAnchors(piece)) });
                var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, Agent, route, route.ExitPortalId, new RouteSeed(0),
                    TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null, null,
                    Admission.Evidence, RoadId.None, track.OffsetRadians(piece, 1f)));
                Assert.That(decision.Motion, Is.Not.Null, decision.Projection.Code);
                freePlan = SpeedPlan.Build(decision.Motion, Model, Driver, Driver.DesiredSpeed);
                Assert.That(freePlan.Accepted, Is.True);
                return freePlan;
            }
        }

        private static LongitudinalDecision Decide(float speed, LongitudinalMemory memory, JunctionEntryInput entry)
        {
            return LongitudinalArbitration.Decide(FreePlan, Driver, speed, Dt, new LongitudinalPerception(PerceptionUnavailableReason.None, null, null),
                memory, TrafficV2Settings.StopHold, entry);
        }

        // ================================================================== tests

        [Test]
        public void TheRequestDistanceTargetsTheStopLineAndTheVehicleOccupiesItsMovementOnlyBeyondIt()
        {
            ulong frameId = 1;
            foreach (var line in BranchLines())
            {
                var route = RouteVia(line.Key);
                var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
                float boundary = EntryDistance(route, line.Key) + line.Value;
                // d est mesure au pare-chocs de reference : egal a la distance nominale a la ligne et continu au basculement
                // corridor -> mouvement courbe (le SMax de l'occupation y sautait de ~0,4 m sur le tourne-a-droite).
                float previous = float.NaN;
                for (int i = 0; i <= 65; i++)
                {
                    float x = 6f - 0.1f * i;
                    var report = ReportAt(route, track, boundary - x - Car.FrontMeters, 0f, frameId++);
                    string label = line.Key + " pare-chocs a " + x.ToString("0.##", CultureInfo.InvariantCulture) + " m de la ligne";
                    Assert.That(report.HasRequest, Is.True, label + "\n" + report.ToText());
                    Assert.That(report.Request.Traversal.FirstMovementId, Is.EqualTo(line.Key), label);
                    Assert.That(report.Request.BoundaryMeters, Is.EqualTo(line.Value), label);
                    float d = report.Request.DistanceMeters;
                    Assert.That(d, Is.EqualTo(x).Within(LocalizationTolerance), label + " : d mesure jusqu'a b");
                    if (!float.IsNaN(previous)) Assert.That(previous - d, Is.EqualTo(0.1f).Within(LocalizationTolerance), label + " : d continu");
                    previous = d;
                    if (d > 0f) Assert.That(report.OccupiedMovements, Has.No.Member(line.Key), label + " : non occupant avant b");
                }
                var beyond = ReportAt(route, track, boundary + 0.5f + Car.LeftMeters - Car.FrontMeters, 0f, frameId++);
                Assert.That(beyond.OccupiedMovements, Has.Member(line.Key), "pare-chocs 0,5 m au-dela de b : occupant\n" + beyond.ToText());
            }
        }

        [Test]
        public void ReplanningInsideALinedMovementPreservesThePhysicalBoundaryAndMovementStarts()
        {
            ulong frameId = 1;
            foreach (var line in BranchLines())
            {
                var route = RouteVia(line.Key);
                var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
                float entry = EntryDistance(route, line.Key);
                foreach (float distanceToLine in new[] { Margin, -0.5f - Car.LeftMeters })
                {
                    float center = entry + line.Value - distanceToLine - Car.FrontMeters;
                    int piece = track.PieceAt(center);
                    var nominal = track.Nominal(piece, center);
                    var input = new TrafficActorInput(Agent, new VehicleFootprintPose { Position = nominal.Position,
                        Forward = nominal.Forward, Up = nominal.Up, Footprint = Car }, 0f, line.Key,
                        TrafficV2Lifecycle.ExpectedElements(route), track.KinematicAnchors(piece));
                    var frame = new TrafficFrame(frameId, Model, new[] { input });
                    TrafficActor actor;
                    Assert.That(frame.TryGetActor(Agent, out actor), Is.True);
                    Assert.That(actor.Location.Localized, Is.True);
                    Assert.That(actor.Location.ElementId, Is.EqualTo(line.Key), "Premisse : centre deja dans le mouvement de branche.");
                    Assert.That(actor.Location.SMeters, Is.GreaterThan(0f));
                    var replanned = RoutePlanner.Plan(new RouteRequest(Model, actor.Location, route.ExitPortalId,
                        new RouteSeed(0), Agent, "route", new DecisionCounter(0), route, true));
                    Assert.That(replanned.Outcome, Is.EqualTo(RouteOutcome.Replanned));
                    Assert.That(replanned.Plan, Is.Not.Null);
                    Assert.That(replanned.Plan.Occurrences[0].Id, Is.EqualTo(line.Key));
                    Assert.That(replanned.Plan.Occurrences[0].StartSMeters, Is.EqualTo(actor.Location.SMeters));
                    var snapshot = JunctionSnapshot.Initial(frameId);
                    var full = JunctionRequestBuilder.Build(frame, Index, Agent, route, Driver, Dt, Margin, snapshot, frameId, HoldEntrySpeed);
                    var trimmed = JunctionRequestBuilder.Build(frame, Index, Agent, replanned.Plan, Driver, Dt, Margin, snapshot, frameId, HoldEntrySpeed);
                    Assert.That(full.HasRequest && trimmed.HasRequest, Is.True);
                    Assert.That(trimmed.Request.BoundaryMeters, Is.EqualTo(line.Value), "b reste l'abscisse du modele.");
                    Assert.That(trimmed.Request.DistanceMeters, Is.EqualTo(full.Request.DistanceMeters).Within(1e-5f));
                    Assert.That(trimmed.Request.HeadOfQueue, Is.EqualTo(full.Request.HeadOfQueue));
                    Assert.That(trimmed.Request.HeadOfQueue, Is.True, "L'acteur seul reste tete de file.");
                    Assert.That(trimmed.StatusOf(line.Key), Is.EqualTo(full.StatusOf(line.Key)));
                    Assert.That(trimmed.OccupiedMovements.Contains(line.Key), Is.EqualTo(distanceToLine < 0f));
                    float startFull, startTrimmed;
                    Assert.That(full.Request.TryGetMovementStart(line.Key, out startFull), Is.True);
                    Assert.That(trimmed.Request.TryGetMovementStart(line.Key, out startTrimmed), Is.True);
                    Assert.That(startFull, Is.LessThan(0f), "Le debut physique du mouvement est deja derriere le pare-chocs.");
                    Assert.That(startTrimmed, Is.EqualTo(startFull).Within(1e-5f), "ETA et degagement gardent l'origine du mouvement complet.");
                    frameId++;
                }
            }
        }

        [Test]
        public void ALeaderInsideALinedMovementMasksTheFollowerBeforeTheLineEvenAfterReplanning()
        {
            var route = RouteVia(SouthLeft);
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            float entry = EntryDistance(route, SouthLeft);
            // Deux petits gabarits independants permettent d'isoler la file entierement entre l'entree et b.
            var footprint = new VehicleFootprint { FrontMeters = 0.4f, RearMeters = 0.4f, LeftMeters = 0.2f, RightMeters = 0.2f };
            var leaderId = new RoadId(0x535EUL, 2UL);
            Func<RoadId, float, TrafficActorInput> input = (id, center) =>
            {
                int piece = track.PieceAt(center);
                var pose = track.Nominal(piece, center);
                return new TrafficActorInput(id, new VehicleFootprintPose { Position = pose.Position, Forward = pose.Forward,
                    Up = pose.Up, Footprint = footprint }, 0f, SouthLeft, TrafficV2Lifecycle.ExpectedElements(route), track.KinematicAnchors(piece));
            };
            var frame = new TrafficFrame(1, Model, new[] { input(Agent, entry + 1.2f), input(leaderId, entry + 2.4f) });
            TrafficActor actor;
            Assert.That(frame.TryGetActor(Agent, out actor), Is.True);
            Assert.That(actor.Location.ElementId, Is.EqualTo(SouthLeft));
            Assert.That(actor.Location.SMeters, Is.GreaterThan(0f));
            var replanned = RoutePlanner.Plan(new RouteRequest(Model, actor.Location, route.ExitPortalId, new RouteSeed(0),
                Agent, "route", new DecisionCounter(0), route, true));
            Assert.That(replanned.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            foreach (var plan in new[] { route, replanned.Plan })
            {
                var report = JunctionRequestBuilder.Build(frame, Index, Agent, plan, Driver, Dt, Margin, JunctionSnapshot.Initial(1), 1, HoldEntrySpeed);
                Assert.That(report.HasRequest, Is.True, report.ToText());
                Assert.That(report.Request.DistanceMeters, Is.GreaterThan(0f));
                Assert.That(report.Request.MaskingActorId, Is.EqualTo(leaderId));
                Assert.That(report.Request.HeadOfQueue, Is.False);
                Assert.That(report.RequestValid, Is.False);
                Assert.That(report.Rejection, Is.EqualTo(JunctionRequestRejection.NotHeadOfQueue));
            }
            var leader = JunctionRequestBuilder.Build(frame, Index, leaderId, route, Driver, Dt, Margin, JunctionSnapshot.Initial(1), 1, HoldEntrySpeed);
            Assert.That(leader.Request.DistanceMeters, Is.GreaterThan(0f));
            Assert.That(leader.Request.HeadOfQueue, Is.True);
        }

        [Test]
        public void ARefusedBranchVehicleStopsInTheLineBandPastTheGenericEntryAndResumesOnItsGrant()
        {
            var driver = Driver;
            int latencySteps = Mathf.RoundToInt(JunctionDistances.DecisionLatencySeconds(Dt) / Dt);
            var lines = new List<string>();
            ulong frameId = 1;
            foreach (var line in BranchLines())
            {
                var route = RouteVia(line.Key);
                var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
                float entry = EntryDistance(route, line.Key);
                float boundary = entry + line.Value;
                foreach (float v0 in new[] { 0f, 4f, 8f })
                {
                    var at = DistancesAt(v0);
                    foreach (float wanted in new[] { at.StopMeters, at.EngageThresholdMeters, 40f })
                    {
                        // Refus du plus tardif (D_stop) au plus precoce, borne par le corridor d'approche.
                        float dRefusal = Math.Min(wanted, boundary - Car.FrontMeters - Car.RearMeters - 3f);
                        string label = line.Key + " v0 " + v0 + ", refus a " + dRefusal.ToString("0.###", CultureInfo.InvariantCulture) + " m de la ligne";
                        // Corridor plus court que D_stop(v0) : refus en deca de D_stop, hors du domaine du banc 5.34 (un vehicule a
                        // cette vitesse a demande et ete refuse plus tot).
                        if (dRefusal < at.StopMeters) { lines.Add(label + " : hors domaine (D_stop " + at.StopMeters.ToString("0.###", CultureInfo.InvariantCulture) + " m)"); continue; }
                        float front = boundary - dRefusal, v = v0, minimum = float.PositiveInfinity, d = dRefusal;
                        var memory = LongitudinalMemory.None;
                        LongitudinalDecision decision = null;
                        JunctionActorReport report = null;
                        for (int k = 0; k < 6000; k++)
                        {
                            report = ReportAt(route, track, front - Car.FrontMeters, v, frameId++);
                            Assert.That(report.HasRequest, Is.True, label + " pas " + k + "\n" + report.ToText());
                            d = report.Request.DistanceMeters;
                            if (report.Request.Traversal.FirstMovementId == line.Key) minimum = Math.Min(minimum, d);
                            float a = driver.MaxAcceleration;
                            if (k >= latencySteps)
                            {
                                decision = Decide(v, memory, EntryOf(report, false));
                                memory = decision.Memory;
                                a = Mathf.Clamp(decision.AppliedAccelerationMetersPerSecondSquared, -at.PlanningDecelerationMetersPerSecondSquared,
                                    driver.MaxAcceleration);
                            }
                            v = a < 0f && v < 0.41f ? Math.Max(0f, v - 2f * Dt) : Math.Max(0f, v + a * Dt);
                            front += v * Dt;
                            if (v <= 0f && decision != null && decision.Hold.Active) break;
                        }
                        report = ReportAt(route, track, front - Car.FrontMeters, 0f, frameId++);
                        d = report.Request.DistanceMeters;
                        Assert.That(decision.Hold.Active, Is.True, label + " : maintien");
                        Assert.That(decision.Hold.Cause, Is.EqualTo(LongitudinalCandidateKind.JunctionEntry), label);
                        Assert.That(minimum, Is.GreaterThan(0f), label + " : pare-chocs jamais au-dela de la ligne");
                        Assert.That(d, Is.InRange(BandLow, BandHigh), label + " : arret dans la bande de la ligne");
                        Assert.That(front, Is.GreaterThan(entry), label + " : arret au-dela de l'entree generique, pas avant elle");
                        Assert.That(report.RequestValid, Is.True, label + " : demande valide a l'arret (O9)");
                        Assert.That(report.OccupiedMovements, Has.No.Member(line.Key), label + " : non occupant a l'arret");

                        float stopped = front - entry;
                        // Reprise au grant : franchit la ligne sans freinage et devient occupant.
                        int steps = 0;
                        var resumed = new List<LongitudinalDecision>();
                        while (front < boundary + 0.5f && steps < 1000)
                        {
                            var current = ReportAt(route, track, front - Car.FrontMeters, v, frameId++);
                            var step = Decide(v, memory, EntryOf(current, true));
                            resumed.Add(step);
                            memory = step.Memory;
                            v = Math.Max(0f, v + step.AppliedAccelerationMetersPerSecondSquared * Dt);
                            front += v * Dt;
                            steps++;
                        }
                        Assert.That(front, Is.GreaterThanOrEqualTo(boundary + 0.5f), label + " : reprise au grant");
                        Assert.That(resumed[0].Hold.Release, Is.EqualTo(StopHoldRelease.GrantEffective), label);
                        Assert.That(resumed.All(x => x.AppliedAccelerationMetersPerSecondSquared >= 0f), Is.True, label + " : reprise sans freinage");
                        Assert.That(ReportAt(route, track, front - Car.FrontMeters, v, frameId++).OccupiedMovements, Has.Member(line.Key),
                            label + " : occupant au-dela de b");
                        lines.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0} : arret a d = {1:0.###} m de la ligne, {2:0.###} m au-dela de l'entree generique ; ligne franchie {3:0.##} s apres le grant",
                            label, d, stopped, steps * Dt));
                    }
                }
            }
            TestContext.WriteLine("bande [" + BandLow.ToString("0.###", CultureInfo.InvariantCulture) + " ; "
                + BandHigh.ToString("0.###", CultureInfo.InvariantCulture) + "] m\n" + string.Join("\n", lines));
        }
    }
}
