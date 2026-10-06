using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
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
    /// Story 5.34, phase 2 : demande et non-demande sur frames reelles de MVP_Run, engagement, distances derivees, pire cas
    /// point-masse avec la latence du pipeline, contrainte JunctionEntry (equilibre, finitude, rang, maintien et liberation au
    /// grant, reprise lissee), carrefour libre sur les 11 routes de la campagne 5.31, instantane decale, blockers, ToText,
    /// garde structurelle de Junction/ et constructeur des scenarios PlayMode C et D.
    /// </summary>
    [Category("Core")]
    [Category("Story534")]
    public sealed class Story534JunctionEntryTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string CampaignPath = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/campaign-5-31.json";
        private const string ScenarioFolder = "_bmad-output/implementation-artifacts/traffic-v2-5-34-explorations";
        private const string ScenarioPath = ScenarioFolder + "/scenarios-5-34.json";
        private const string TrafficRootPath = "Assets/RoadRage/Features/Vehicles/Traffic";
        private const float Dt = 0.02f;

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };

        private static readonly RoadId Agent = new RoadId(0x534EUL, 1UL);
        private static readonly RoadId Other = new RoadId(0x534EUL, 2UL);
        private static readonly RoadId Holder = new RoadId(0x534EUL, 9UL);
        private static readonly RoadId EastStraight = RoadId.Parse("40ca7f10a97f50a918e8c3a2a1e58493");
        private static readonly RoadId SouthLeft = RoadId.Parse("4e437f94525852d3a072a538537c2093");
        private static readonly RoadId Zone23 = RoadId.Parse("4258af5419bba1365a3f0ad6ed3d44aa");

        private static float Margin { get { return TrafficV2Settings.JunctionStopControlMarginMeters; } }
        /// <summary>Vitesse d'entree du maintien D11 : la fenetre de maintien d'une entree est D_stop a cette vitesse (O14).</summary>
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

        private static JunctionDistances DistancesAt(float speed, float dt = Dt)
        {
            return JunctionDistances.For(Driver, speed, dt, Margin, HoldEntrySpeed);
        }

        // ================================================================== routes et poses

        private static RoutePlan RouteVia(RoadId movement, RoadId trafficId)
        {
            var start = new RoadLocation { ModelId = Model.ModelId, ModelVersion = Model.Version, Localized = true,
                ElementKind = RoadElementKind.LaneCorridor, ElementId = Index.FromCorridorOf(movement), SMeters = 0f };
            var result = RoutePlanner.Plan(new RouteRequest(Model, start, RoadId.None, new RouteSeed(0), trafficId, "route",
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

        /// <summary>
        /// Acteur a la pose nominale de sa route, a une distance de route donnee depuis son debut ; bonus de localisation sur les
        /// elements attendus, comme le driver (ExpectedElements : une route a boucle ne favorise pas un passage ulterieur).
        /// </summary>
        private static TrafficActorInput ActorOn(RoadId id, RoutePlan route, ReferenceTrack track, float distance, float speed)
        {
            int piece = track.PieceAt(distance);
            var nominal = track.Nominal(piece, distance);
            return new TrafficActorInput(id, new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward,
                Up = nominal.Up, Footprint = Car }, speed, track.Pieces[piece].Id, TrafficV2Lifecycle.ExpectedElements(route),
                track.KinematicAnchors(piece));
        }

        private static JunctionActorReport Report(TrafficFrame frame, RoadId id, RoutePlan route, JunctionSnapshot snapshot = null)
        {
            return JunctionRequestBuilder.Build(frame, Index, id, route, Driver, Dt, Margin, snapshot ?? JunctionSnapshot.Initial(frame.FrameId),
                frame.FrameId, HoldEntrySpeed);
        }

        // ================================================================== banc d'arbitrage (plan a v0)

        private static SpeedPlan freePlan;

        /// <summary>Plan accepte au debut de la plus longue ligne droite de MVP_Run, a v0 (aucune interaction), comme le banc 5.33.</summary>
        private static SpeedPlan FreePlan
        {
            get
            {
                if (freePlan != null) return freePlan;
                var straight = Model.Movements.Where(m => m.Samples.All(s => Math.Abs(s.CurvaturePerMeter) < 1e-4f))
                    .OrderBy(m => m.Id).First();
                var route = RouteVia(straight.Id, Agent);
                var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
                var frame = new TrafficFrame(1, Model, new[] { ActorOn(Agent, route, track, 1f, Driver.DesiredSpeed) });
                var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, Agent, route, route.ExitPortalId, new RouteSeed(0),
                    TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null, null,
                    Admission.Evidence, RoadId.None, track.OffsetRadians(track.PieceAt(1f), 1f)));
                Assert.That(decision.Motion, Is.Not.Null, decision.Projection.Code);
                freePlan = SpeedPlan.Build(decision.Motion, Model, Driver, Driver.DesiredSpeed);
                Assert.That(freePlan.Accepted, Is.True);
                return freePlan;
            }
        }

        private static LongitudinalPerception Nothing(PerceptionUnavailableReason reason = PerceptionUnavailableReason.None)
        {
            return new LongitudinalPerception(reason, null, null);
        }

        private static JunctionEntryInput Entry(float d, float speed, bool grant = false)
        {
            var at = DistancesAt(speed);
            return new JunctionEntryInput(EastStraight, d, at.EngageThresholdMeters, Margin, grant, at.StopMeters);
        }

        private static LongitudinalDecision Decide(float speed, LongitudinalMemory memory, JunctionEntryInput entry,
            LongitudinalPerception perception = null)
        {
            return LongitudinalArbitration.Decide(FreePlan, Driver, speed, Dt, perception ?? Nothing(), memory, TrafficV2Settings.StopHold, entry);
        }

        // ================================================================== distances

        [Test]
        public void TheDerivedDistancesArePublishedMonotoneAndRecomputedForAnotherStep()
        {
            var driver = Driver;
            foreach (float v in new[] { 0f, 4f, 8f })
                TestContext.WriteLine("profil par defaut, " + v.ToString(CultureInfo.InvariantCulture) + " m/s : " + DistancesAt(v).ToText());
            var at8 = DistancesAt(8f);
            float tLat = 2f * Dt, vRef = 8f + driver.MaxAcceleration * tLat, bPlan = 0.99f * driver.ComfortableDeceleration;
            Assert.That(at8.LatencySeconds, Is.EqualTo(tLat).Within(1e-6f));
            Assert.That(at8.ReferenceSpeedMetersPerSecond, Is.EqualTo(vRef).Within(1e-5f));
            Assert.That(at8.PlanningDecelerationMetersPerSecondSquared, Is.EqualTo(bPlan).Within(1e-5f));
            float stop = vRef * tLat + vRef * vRef / (2f * bPlan) + Margin;
            float star = driver.MinimumGap + vRef * driver.TimeHeadway
                + vRef * vRef / (2f * Mathf.Sqrt(driver.MaxAcceleration * driver.ComfortableDeceleration));
            Assert.That(at8.StopMeters, Is.EqualTo(stop).Within(1e-3f));
            Assert.That(at8.EngageMeters, Is.EqualTo(Math.Max(stop, star) + vRef * tLat).Within(1e-3f));
            Assert.That(at8.RequestMeters, Is.EqualTo(DistancesAt(vRef).EngageMeters + vRef * tLat).Within(1e-3f),
                "D_request = D_engage(v_ref) + v_ref t_lat");
            Assert.That(at8.StopMeters, Is.EqualTo(16.95f).Within(0.1f), "exemple calcule de la spec, m_ctrl 0,25 m (O14)");
            Assert.That(at8.EngageMeters, Is.EqualTo(33.2f).Within(0.1f));
            Assert.That(at8.RequestMeters, Is.EqualTo(33.86f).Within(0.1f));

            var previous = DistancesAt(0f);
            for (float v = 0.1f; v <= driver.DesiredSpeed + 1e-3f; v += 0.1f)
            {
                var current = DistancesAt(v);
                Assert.That(current.StopMeters, Is.GreaterThanOrEqualTo(previous.StopMeters), "D_stop monotone, v = " + v);
                Assert.That(current.EngageMeters, Is.GreaterThanOrEqualTo(previous.EngageMeters), "D_engage monotone, v = " + v);
                Assert.That(current.RequestMeters, Is.GreaterThanOrEqualTo(previous.RequestMeters), "D_request monotone, v = " + v);
                Assert.That(current.RequestMeters, Is.GreaterThan(current.EngageMeters));
                Assert.That(current.EngageMeters, Is.GreaterThan(current.StopMeters));
                previous = current;
            }

            var fine = DistancesAt(8f, 0.01f);
            Assert.That(fine.LatencySeconds, Is.EqualTo(0.02f).Within(1e-6f), "t_lat = 2 dt");
            Assert.That(fine.StopMeters, Is.LessThan(at8.StopMeters), "un pas plus court reduit la latence et D_stop");
            float vFine = 8f + driver.MaxAcceleration * 0.02f;
            Assert.That(fine.StopMeters, Is.EqualTo(vFine * 0.02f + vFine * vFine / (2f * bPlan) + Margin).Within(1e-3f));

            // Decisions O9 et O14 : plancher = fenetre de maintien, D_stop a la vitesse d'entree du maintien ; D_engage la couvre a l'arret.
            var rest = DistancesAt(0f);
            Assert.That(rest.HoldWindowMeters, Is.EqualTo(DistancesAt(HoldEntrySpeed).StopMeters).Within(1e-6f), "fenetre de maintien devant l'entree");
            Assert.That(rest.EngageMeters, Is.GreaterThan(rest.HoldWindowMeters), "a l'arret, D_engage couvre la fenetre de maintien");
            Assert.That(rest.RequestThresholdMeters, Is.EqualTo(rest.RequestMeters));
            Assert.That(rest.EngageThresholdMeters, Is.EqualTo(rest.EngageMeters));
            Assert.That(DistancesAt(1f).RequestThresholdMeters, Is.EqualTo(DistancesAt(1f).RequestMeters), "formule au-dela de la fenetre");
        }

        // ================================================================== pire cas : arret avant l'entree

        /// <summary>
        /// Point-masse avec le modele de latence : evenement (refus, revocation ou absence de grant) mesure a la frame N, publie
        /// a N+1, actuation a N+1 ; pendant t_lat le vehicule accelere a a ; ensuite la commande est la decision de l'arbitrage
        /// sur l'entree, deceleration realisee bornee a b_plan. Rend la distance minimale du pare-chocs avant a l'entree.
        /// </summary>
        private static float WorstCase(float d0, float v0, out int steps)
        {
            var driver = Driver;
            float d = d0, v = v0, minimum = d0;
            int latencySteps = Mathf.RoundToInt(JunctionDistances.DecisionLatencySeconds(Dt) / Dt);
            float bPlan = DistancesAt(v0).PlanningDecelerationMetersPerSecondSquared;
            var memory = LongitudinalMemory.None;
            for (steps = 0; steps < 3000; steps++)
            {
                float acceleration;
                if (steps < latencySteps) acceleration = driver.MaxAcceleration;
                else
                {
                    var decision = Decide(v, memory, Entry(d, v));
                    memory = decision.Memory;
                    acceleration = Mathf.Clamp(decision.AppliedAccelerationMetersPerSecondSquared, -bPlan, driver.MaxAcceleration);
                }
                v = Math.Max(0f, v + acceleration * Dt);
                d -= v * Dt;
                minimum = Math.Min(minimum, d);
                if (v <= 0f && steps > latencySteps) break;
            }
            return minimum;
        }

        [Test]
        public void ARefusalAtTheLastPermittedDistanceOrAtTheEngageDistanceNeverLetsTheFrontBumperPassTheEntry()
        {
            var lines = new List<string>();
            float worst = float.PositiveInfinity;
            for (float v = 0f; v <= Driver.DesiredSpeed + 1e-3f; v += 0.5f)
            {
                var distances = DistancesAt(v);
                int stepsStop, stepsEngage;
                float atStop = WorstCase(distances.StopMeters, v, out stepsStop);
                float atEngage = WorstCase(distances.EngageThresholdMeters, v, out stepsEngage);
                Assert.That(atStop, Is.GreaterThanOrEqualTo(0f), "refus a d = D_stop, v = " + v);
                Assert.That(atEngage, Is.GreaterThanOrEqualTo(0f), "refus a d = D_engage, v = " + v);
                worst = Math.Min(worst, Math.Min(atStop, atEngage));
                lines.Add(string.Format(CultureInfo.InvariantCulture,
                    "v {0:0.0} m/s : D_stop {1:0.###} -> d min {2:0.###} m ({3} pas) ; D_engage {4:0.###} -> d min {5:0.###} m ({6} pas)",
                    v, distances.StopMeters, atStop, stepsStop, distances.EngageThresholdMeters, atEngage, stepsEngage));
            }
            Publish("o13-worst-case.txt", "pire cas, distance minimale du pare-chocs avant a l'entree :\n" + string.Join("\n", lines)
                + "\nminimum global " + worst.ToString("0.####", CultureInfo.InvariantCulture) + " m");
        }

        // ================================================================== contrainte JunctionEntry

        [Test]
        public void TheEntryCandidateIsAtEquilibriumAtTheControlMarginFiniteAtTheEntryAndInactiveOnceEntered()
        {
            var driver = Driver;
            // Decisions O13 et O14 : l'entree est une ligne ; a l'arret a m_ctrl (en deca de D_stop(0)), a_kin annule le candidat.
            float stop0 = DistancesAt(0f).StopMeters;
            Assert.That(Margin, Is.LessThanOrEqualTo(stop0));
            Assert.That(LongitudinalArbitration.JunctionEntryAcceleration(driver, 0f, Margin, Margin, stop0), Is.EqualTo(0f).Within(1e-6f),
                "arret a m_ctrl : candidat nul");
            Assert.That(LongitudinalArbitration.JunctionEntryAcceleration(driver, 0f, driver.MinimumGap, Margin, stop0), Is.GreaterThan(0f),
                "a s0 de l'entree, un vehicule arrete avance encore : a_kin (nul a l'arret) ne le fige pas au-dela de D_stop");
            // a_kin n'entre qu'en deca de D_stop ; au-dela, le candidat est l'IDM seul (O13).
            var at3 = DistancesAt(3f);
            Assert.That(LongitudinalArbitration.JunctionEntryAcceleration(driver, 3f, at3.StopMeters + 1f, Margin, at3.StopMeters),
                Is.EqualTo(DriverModel.ComputeAcceleration(driver, 3f, 0f, at3.StopMeters + 1f + driver.MinimumGap)));
            foreach (float d in new[] { 0.6f, 0.5f, 0.1f, 0f, -0.5f })
                foreach (float v in new[] { 0f, 0.5f, 3f, 8f })
                {
                    float a = LongitudinalArbitration.JunctionEntryAcceleration(driver, v, d, Margin, DistancesAt(v).StopMeters);
                    Assert.That(float.IsNaN(a) || float.IsInfinity(a), Is.False, "fini a d = " + d + ", v = " + v);
                }
            Assert.That(Entry(0f, 3f).Active, Is.False, "pare-chocs a l'entree : entre");
            Assert.That(Entry(-0.2f, 3f).Active, Is.False);
            Assert.That(Entry(5f, 3f, true).Active, Is.False, "grant effectif");
            Assert.That(Entry(DistancesAt(3f).EngageThresholdMeters + 0.01f, 3f).Active, Is.False, "au-dela de D_engage");
            var entered = Decide(3f, LongitudinalMemory.None, Entry(-0.2f, 3f));
            Assert.That(entered.Candidates.Any(c => c.Kind == LongitudinalCandidateKind.JunctionEntry), Is.False);
            // Refus tardif a 8 m/s et 9 m : le candidat est le minimum exact de l'IDM et de a_kin.
            var late = Decide(8f, LongitudinalMemory.None, Entry(9f, 8f));
            Assert.That(late.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.JunctionEntry));
            Assert.That(late.TargetAccelerationMetersPerSecondSquared,
                Is.EqualTo(Math.Min(DriverModel.ComputeAcceleration(driver, 8f, 0f, 9f + driver.MinimumGap),
                    LongitudinalArbitration.KinematicStopAcceleration(8f, 9f, Margin))));
        }

        [Test]
        public void TheEntryRanksBetweenStopHoldAndObstacleAndLeavesThe533OrderUnchanged()
        {
            Assert.That(LongitudinalArbitration.TieRank(LongitudinalCandidateKind.StopHold),
                Is.LessThan(LongitudinalArbitration.TieRank(LongitudinalCandidateKind.JunctionEntry)));
            Assert.That(LongitudinalArbitration.TieRank(LongitudinalCandidateKind.JunctionEntry),
                Is.LessThan(LongitudinalArbitration.TieRank(LongitudinalCandidateKind.Obstacle)));
            var old = new[] { LongitudinalCandidateKind.SteeringCeilingUnreachable, LongitudinalCandidateKind.PerceptionUnavailable,
                LongitudinalCandidateKind.StopHold, LongitudinalCandidateKind.Obstacle, LongitudinalCandidateKind.LeaderFollowing,
                LongitudinalCandidateKind.Profile, LongitudinalCandidateKind.DesiredSpeed };
            for (int i = 0; i < old.Length; i++) Assert.That((int)old[i], Is.EqualTo(i), "aucune valeur 5.33 renumerotee");
            Assert.That(old.Select(LongitudinalArbitration.TieRank), Is.Ordered, "ordre relatif 5.33 inchange");

            // Egalite exacte avec un obstacle fixe a la place de l'obstacle virtuel (d + s0), au-dela de D_stop (IDM seul) : JunctionEntry lie.
            float v = 1f, d = 1.5f, gap = d + Driver.MinimumGap;
            Assert.That(d, Is.GreaterThan(DistancesAt(v).StopMeters));
            var obstacle = new LongitudinalPerception(PerceptionUnavailableReason.None, null,
                new[] { new LongitudinalObstacle(Other, PerceivedObstacleKind.Obstacle, gap, 0f) });
            var tie = Decide(v, LongitudinalMemory.None, Entry(d, v), obstacle);
            Assert.That(tie.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.JunctionEntry), tie.ToText());
            var kinds = tie.Candidates.Select(c => c.Kind).ToList();
            Assert.That(kinds.IndexOf(LongitudinalCandidateKind.JunctionEntry), Is.LessThan(kinds.IndexOf(LongitudinalCandidateKind.Obstacle)));
        }

        [Test]
        public void WithoutAnActiveEntryTheArbitrationIsThe533OneBitForBit()
        {
            var leader = new LongitudinalPerception(PerceptionUnavailableReason.None, new LongitudinalLeader(Other, 12f, 2f), null);
            foreach (float v in new[] { 0f, 0.3f, 2f, 5f, 8f })
                foreach (var perception in new[] { Nothing(), leader, Nothing(PerceptionUnavailableReason.ChannelSaturated) })
                {
                    var reference = LongitudinalArbitration.Decide(FreePlan, Driver, v, Dt, perception, LongitudinalMemory.None,
                        TrafficV2Settings.StopHold);
                    foreach (var inactive in new[] { default(JunctionEntryInput), Entry(5f, v, true), Entry(-1f, v), Entry(500f, v) })
                    {
                        var decided = LongitudinalArbitration.Decide(FreePlan, Driver, v, Dt, perception, LongitudinalMemory.None,
                            TrafficV2Settings.StopHold, inactive);
                        Assert.That(decided.ToText(), Is.EqualTo(reference.ToText()));
                        Assert.That(decided.AppliedAccelerationMetersPerSecondSquared, Is.EqualTo(reference.AppliedAccelerationMetersPerSecondSquared));
                    }
                }
        }

        /// <summary>
        /// Approche point-masse d'une entree refusee, physique du banc 5.33 (roue libre a 2 m/s2 dans la bande de service
        /// 0,41 m/s), arbitrage reel avec le maintien D11 ; rend l'etat apres l'arret.
        /// </summary>
        private static LongitudinalDecision ApproachARefusedEntry(float v0, out float d, out LongitudinalMemory memory, out float minimum)
        {
            d = 40f;
            float v = v0;
            minimum = d;
            memory = LongitudinalMemory.None;
            LongitudinalDecision decision = null;
            for (int k = 0; k < 4000; k++)
            {
                decision = Decide(v, memory, Entry(d, v));
                memory = decision.Memory;
                float a = decision.AppliedAccelerationMetersPerSecondSquared;
                v = a < 0f && v < 0.41f ? Math.Max(0f, v - 2f * Dt) : Math.Max(0f, v + a * Dt);
                d -= v * Dt;
                minimum = Math.Min(minimum, d);
                if (v <= 0f && decision.Hold.Active) break;
            }
            return decision;
        }

        [Test]
        public void AVehicleRefusedAtTheEntryHoldsKeepsRequestingAndIsReleasedOnlyByItsGrantWithASmoothedResume()
        {
            foreach (float v0 in new[] { 4f, 6f, 8f })
            {
                float d, minimum;
                LongitudinalMemory memory;
                var held = ApproachARefusedEntry(v0, out d, out memory, out minimum);
                Assert.That(held.Hold.Active, Is.True, "maintien a l'entree, v0 " + v0);
                Assert.That(held.Hold.Cause, Is.EqualTo(LongitudinalCandidateKind.JunctionEntry));
                Assert.That(held.Hold.SourceId, Is.EqualTo(EastStraight));
                Assert.That(minimum, Is.GreaterThan(0f), "pare-chocs avant jamais au-dela de l'entree");
                var rest = DistancesAt(0f);
                Assert.That(d, Is.LessThanOrEqualTo(rest.RequestThresholdMeters), "decision O9 : demande toujours valide a l'arret");
                Assert.That(Entry(d, 0f).Active, Is.True, "decision O9 : contrainte toujours active a l'arret");
                TestContext.WriteLine("v0 " + v0 + " : arret a d = " + d.ToString("0.###", CultureInfo.InvariantCulture) + " m, seuils a l'arret "
                    + rest.ToText());

                // Rien ne libere le maintien sans grant, meme apres longtemps ; une perception indisponible ne libere jamais.
                for (int k = 0; k < 500; k++)
                {
                    var still = Decide(0f, memory, Entry(d, 0f));
                    Assert.That(still.Hold.Active, Is.True);
                    Assert.That(still.AppliedAccelerationMetersPerSecondSquared, Is.LessThan(0f), "arret maintenu au frein a main");
                    memory = still.Memory;
                }
                var blind = Decide(0f, memory, Entry(d, 0f, true), Nothing(PerceptionUnavailableReason.ChannelUnavailable));
                Assert.That(blind.Hold.Phase, Is.EqualTo(StopHoldPhase.Holding), "grant effectif mais perception indisponible");
                memory = blind.Memory;
                var released = Decide(0f, memory, Entry(d, 0f, true));
                Assert.That(released.Hold.Phase, Is.EqualTo(StopHoldPhase.Released));
                Assert.That(released.Hold.Release, Is.EqualTo(StopHoldRelease.GrantEffective));
                Assert.That(released.Candidates.Any(c => c.Kind == LongitudinalCandidateKind.JunctionEntry), Is.False);
                Assert.That(released.Smoothed, Is.True, "reprise lissee D5 depuis 0");
                Assert.That(released.SmoothingSource, Is.EqualTo(SpeedConstraint.JunctionEntry));
                Assert.That(released.AppliedAccelerationMetersPerSecondSquared, Is.LessThan(released.TargetAccelerationMetersPerSecondSquared));
                Assert.That(released.AppliedAccelerationMetersPerSecondSquared, Is.GreaterThanOrEqualTo(0f));
            }
        }

        // ================================================================== point d'arret et reprise (decision O13)

        /// <summary>Mesure publiee a cote des scenarios 5.34, et dans la sortie du test.</summary>
        private static void Publish(string name, string text)
        {
            TestContext.WriteLine(text);
            Directory.CreateDirectory(ScenarioFolder);
            File.WriteAllText(ScenarioFolder + "/" + name, text + "\n");
        }

        /// <summary>Tolerance d'integration sous m_ctrl (m) : un pas d'Euler a l'arret, v Δt avec v &lt;= 0,5 m/s, arrondi.</summary>
        private const float StopBandIntegrationTolerance = 0.02f;

        /// <summary>Bande declaree de la distance finale du pare-chocs avant a l'entree : [m_ctrl − tolerance, fenetre de maintien] (O14).</summary>
        private static float BandLow { get { return Margin - StopBandIntegrationTolerance; } }
        private static float BandHigh { get { return DistancesAt(0f).HoldWindowMeters; } }

        /// <summary>
        /// Point-masse du banc 5.33 (roue libre a 2 m/s2 sous 0,41 m/s) : vehicule a v0, refus a la distance dRefusal ; pendant
        /// t_lat il accelere a a (pire cas du pipeline), ensuite l'arbitrage reel decide, deceleration realisee bornee a b_plan.
        /// Rend la decision a l'arret ; d est la distance finale du pare-chocs avant a l'entree, minimum la plus petite atteinte.
        /// </summary>
        private static LongitudinalDecision StopAfterRefusal(float v0, float dRefusal, out float d, out float minimum,
            out LongitudinalMemory memory)
        {
            var driver = Driver;
            int latencySteps = Mathf.RoundToInt(JunctionDistances.DecisionLatencySeconds(Dt) / Dt);
            float bPlan = DistancesAt(v0).PlanningDecelerationMetersPerSecondSquared;
            float v = v0;
            d = minimum = dRefusal;
            memory = LongitudinalMemory.None;
            LongitudinalDecision decision = null;
            for (int k = 0; k < 6000; k++)
            {
                float a = driver.MaxAcceleration;
                if (k >= latencySteps)
                {
                    decision = Decide(v, memory, Entry(d, v));
                    memory = decision.Memory;
                    a = Mathf.Clamp(decision.AppliedAccelerationMetersPerSecondSquared, -bPlan, driver.MaxAcceleration);
                }
                v = a < 0f && v < 0.41f ? Math.Max(0f, v - 2f * Dt) : Math.Max(0f, v + a * Dt);
                d -= v * Dt;
                minimum = Math.Min(minimum, d);
                if (v <= 0f && decision != null && decision.Hold.Active) break;
            }
            return decision;
        }

        [Test]
        public void ARefusedVehicleStopsInTheDeclaredBandBeforeTheEntryWhateverItsSpeedAndTheRefusalDistance()
        {
            var lines = new List<string>();
            // v0 = 0 : vehicule arrete hors de la fenetre de maintien (laisse par un leader), qui doit encore avancer jusqu'a la bande.
            foreach (float v0 in new[] { 0f, 2f, 4f, 6f, 8f })
            {
                var at = DistancesAt(v0);
                // Du refus le plus tardif (D_stop : en deca, un grant tenu est engage) au plus precoce (hors de toute fenetre).
                var refusals = new[] { at.StopMeters, 0.5f * (at.StopMeters + at.EngageThresholdMeters), at.EngageThresholdMeters,
                    at.RequestThresholdMeters, 40f };
                var finals = new List<float>();
                foreach (float dRefusal in refusals)
                {
                    float d, minimum;
                    LongitudinalMemory memory;
                    var held = StopAfterRefusal(v0, dRefusal, out d, out minimum, out memory);
                    string label = "v0 " + v0 + ", refus a d = " + dRefusal.ToString("0.###", CultureInfo.InvariantCulture);
                    Assert.That(held.Hold.Active, Is.True, label + " : maintien a l'entree");
                    Assert.That(held.Hold.Cause, Is.EqualTo(LongitudinalCandidateKind.JunctionEntry), label);
                    Assert.That(minimum, Is.GreaterThan(0f), label + " : pare-chocs avant jamais au-dela de l'entree");
                    Assert.That(d, Is.InRange(BandLow, BandHigh), label + " : distance finale dans la bande declaree");
                    Assert.That(Entry(d, 0f).Active, Is.True, label + " : contrainte toujours active a l'arret (O9)");
                    Assert.That(d, Is.LessThanOrEqualTo(DistancesAt(0f).RequestThresholdMeters), label + " : demande toujours valide (O9)");
                    finals.Add(d);
                    lines.Add(string.Format(CultureInfo.InvariantCulture, "{0} m -> arret a d = {1:0.###} m (minimum {2:0.###} m)",
                        label, d, minimum));
                }
                lines.Add(string.Format(CultureInfo.InvariantCulture, "v0 {0} : ecart des arrets selon la distance de refus {1:0.###} m",
                    v0, finals.Max() - finals.Min()));
            }
            Publish("o13-stop-band.txt", "bande declaree [" + BandLow.ToString("0.###", CultureInfo.InvariantCulture) + " ; "
                + BandHigh.ToString("0.###", CultureInfo.InvariantCulture) + "] m\n" + string.Join("\n", lines));
        }

        /// <summary>Pas jusqu'au franchissement de l'entree (d &lt;= 0) depuis l'arret a d, grant effectif ; -1 s'il n'entre pas.</summary>
        private static int LeaveAfterGrant(float d, LongitudinalMemory memory, out float speedAtEntry, List<LongitudinalDecision> decisions)
        {
            float v = 0f;
            speedAtEntry = 0f;
            for (int k = 0; k < 1000; k++)
            {
                var decision = Decide(v, memory, d > 0f ? Entry(d, v, true) : default(JunctionEntryInput));
                if (decisions != null) decisions.Add(decision);
                memory = decision.Memory;
                v = Math.Max(0f, v + decision.AppliedAccelerationMetersPerSecondSquared * Dt);
                d -= v * Dt;
                if (d <= 0f) { speedAtEntry = v; return k + 1; }
            }
            return -1;
        }

        [Test]
        public void AVehicleHeldBeforeTheEntryLeavesNormallyOnceItsGrantIsEffective()
        {
            var lines = new List<string>();
            foreach (float v0 in new[] { 2f, 8f })
            {
                float d, minimum;
                LongitudinalMemory memory;
                StopAfterRefusal(v0, DistancesAt(v0).EngageThresholdMeters, out d, out minimum, out memory);
                for (int k = 0; k < 100; k++) memory = Decide(0f, memory, Entry(d, 0f)).Memory;

                var decisions = new List<LongitudinalDecision>();
                float speed;
                int steps = LeaveAfterGrant(d, memory, out speed, decisions);
                Assert.That(decisions[0].Hold.Phase, Is.EqualTo(StopHoldPhase.Released));
                Assert.That(decisions[0].Hold.Release, Is.EqualTo(StopHoldRelease.GrantEffective));
                Assert.That(steps, Is.GreaterThan(0), "entre dans son mouvement apres le grant");
                for (int k = 1; k < decisions.Count; k++)
                {
                    Assert.That(decisions[k].Hold.Active, Is.False, "aucun nouveau maintien apres le grant, pas " + k);
                    Assert.That(decisions[k].Candidates.Any(c => c.Kind == LongitudinalCandidateKind.JunctionEntry), Is.False);
                }
                Assert.That(decisions.All(x => x.AppliedAccelerationMetersPerSecondSquared >= 0f), Is.True, "reprise sans freinage");

                // Reference : meme depart arrete sans aucun carrefour ; seul le lissage D5 de la reprise (temps de reaction) s'y ajoute.
                float freeSpeed;
                int free = LeaveAfterGrant(d, LongitudinalMemory.None, out freeSpeed, null);
                int lag = Mathf.CeilToInt(2f * Driver.ReactionTime / Dt);
                Assert.That(steps, Is.LessThanOrEqualTo(free + lag), "franchissement au plus 2 temps de reaction apres un depart libre");
                lines.Add(string.Format(CultureInfo.InvariantCulture,
                    "v0 {0} : arret a d = {1:0.###} m ; entree franchie {2:0.##} s apres le grant a {3:0.##} m/s (depart libre {4:0.##} s)",
                    v0, d, steps * Dt, speed, free * Dt));
            }
            Publish("o13-resume.txt", string.Join("\n", lines));
        }

        [Test]
        public void TheFootprintHeldAtTheControlMarginStaysClearOfEveryConflictingMovementOfMvpRun()
        {
            // Pire cas de la bande : pare-chocs avant a m_ctrl de l'entree, sur la pose nominale de son corridor d'approche.
            var lines = new List<string>();
            float worst = float.PositiveInfinity;
            int skipped = 0;
            int entries = 0;
            foreach (var movement in Model.Movements)
            {
                // Corridor interne (anneau) : jamais l'entree d'une traversee, chainee depuis le mouvement d'entree du carrefour.
                if (Model.Movements.Any(o => o.JunctionId == movement.JunctionId && o.ToCorridorId == movement.FromCorridorId)) continue;
                entries++;
                var route = RouteVia(movement.Id, Agent);
                var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
                float center = EntryDistance(route, movement.Id) - Margin - Car.FrontMeters;
                if (center - Car.RearMeters < 0f) { skipped++; continue; }
                var pose = track.Nominal(track.PieceAt(center), center);
                var forward = new Vector2(pose.Forward.x, pose.Forward.z).normalized;
                float clearance = float.PositiveInfinity;
                string against = null;
                foreach (var other in Model.Movements)
                {
                    RoadId zone;
                    if (other.Id == movement.Id || other.FromCorridorId == movement.FromCorridorId
                        || !Index.TryGetConflict(movement.Id, other.Id, out zone)) continue;
                    // ponytail: l'autre caisse est approchee par des disques de demi-largeur le long de son axe (coins des virages non
                    // balayes) ; la marge mesuree couvre cet ecart, une empreinte balayee exacte si elle devient serree.
                    foreach (var sample in other.Samples)
                    {
                        var offset = new Vector2(sample.Position.x - pose.Position.x, sample.Position.z - pose.Position.z);
                        float along = Mathf.Max(Mathf.Abs(Vector2.Dot(offset, forward)) - Car.FrontMeters, 0f);
                        float across = Mathf.Max(Mathf.Abs(forward.x * offset.y - forward.y * offset.x) - Car.LeftMeters, 0f);
                        float c = new Vector2(along, across).magnitude - Car.LeftMeters;
                        if (c < clearance) { clearance = c; against = other.Label; }
                    }
                }
                if (against == null) continue;
                Assert.That(clearance, Is.GreaterThan(0f), movement.Label + " : empreinte a l'arret dans " + against);
                if (clearance < worst) worst = clearance;
                lines.Add(clearance.ToString("0.##", CultureInfo.InvariantCulture) + " m  " + movement.Label + "  /  " + against);
            }
            Assert.That(skipped, Is.Zero, "corridor d'approche plus court que l'empreinte a l'arret");
            Assert.That(entries, Is.GreaterThan(0));
            Publish("o13-clearance.txt", entries + " entrees de traversee, degagement minimal " + worst.ToString("0.##", CultureInfo.InvariantCulture)
                + " m ; cinq plus serres :\n"
                + string.Join("\n", lines.OrderBy(l => float.Parse(l.Split(' ')[0], CultureInfo.InvariantCulture)).Take(5).ToArray()));
        }

        // ================================================================== blockers

        [Test]
        public void AnEntryHoldIsAJunctionGrantOrBlockedExitBlockerAndAMovingRefusalIsNone()
        {
            float d, minimum;
            LongitudinalMemory memory;
            var held = ApproachARefusedEntry(6f, out d, out memory, out minimum);
            var grant = BlockerTracker.Update(null, held, Driver, 40, new JunctionBlockerCause(BlockerKind.JunctionGrant, Holder.ToString())).Single();
            Assert.That(grant.Kind, Is.EqualTo(BlockerKind.JunctionGrant));
            Assert.That(grant.Source, Is.EqualTo(BlockerSource.JunctionCoordination));
            Assert.That(grant.BlockingActorOrRule, Is.EqualTo(Holder.ToString()), "titulaire en cause");
            Assert.That(grant.Legitimate, Is.True);
            Assert.That(grant.ExpectedToClear, Is.True);
            Assert.That(grant.Recoverable, Is.False);
            var exit = BlockerTracker.Update(new[] { grant }, held, Driver, 41,
                new JunctionBlockerCause(BlockerKind.BlockedExit, "exit-corridor")).Single();
            Assert.That(exit.Kind, Is.EqualTo(BlockerKind.BlockedExit));
            Assert.That(exit.SinceFrame, Is.EqualTo(41UL), "nouveau (genre, bloqueur)");
            var sinceKept = BlockerTracker.Update(new[] { grant }, held, Driver, 42,
                new JunctionBlockerCause(BlockerKind.JunctionGrant, Holder.ToString())).Single();
            Assert.That(sinceKept.SinceFrame, Is.EqualTo(40UL), "presence continue");
            var unnamed = BlockerTracker.Update(null, held, Driver, 43).Single();
            Assert.That(unnamed.Kind, Is.EqualTo(BlockerKind.JunctionGrant), "sans record : attente de grant sur la traversee");
            Assert.That(unnamed.BlockingActorOrRule, Is.EqualTo(EastStraight.ToString()));

            var moving = Decide(6f, LongitudinalMemory.None, Entry(15f, 6f));
            Assert.That(moving.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.JunctionEntry));
            Assert.That(moving.TargetAccelerationMetersPerSecondSquared, Is.LessThan(0f));
            Assert.That(BlockerTracker.Update(null, moving, Driver, 44), Is.Empty, "un freinage en roulant n'est pas une immobilisation");
        }

        // ================================================================== demande, tete de file, engagement (frames reelles)

        [Test]
        public void AValidRequestATooFarOneAndALeaderMaskingTheEntryWithAnotherNextMovement()
        {
            var route = RouteVia(EastStraight, Agent);
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            float entry = EntryDistance(route, EastStraight);
            float speed = 4f;
            float near = entry - Car.FrontMeters - 6f;
            var alone = new TrafficFrame(1, Model, new[] { ActorOn(Agent, route, track, near, speed) });
            var valid = Report(alone, Agent, route);
            Assert.That(valid.RequestValid, Is.True, valid.ToText());
            Assert.That(valid.Request.Traversal.FirstMovementId, Is.EqualTo(EastStraight));
            Assert.That(valid.Request.DistanceMeters, Is.EqualTo(6f).Within(0.2f));
            Assert.That(valid.Request.HeadOfQueue, Is.True);

            float slow = 1f;
            float far = entry - Car.FrontMeters - (DistancesAt(slow).RequestThresholdMeters + 1f);
            Assert.That(far, Is.GreaterThan(0f));
            var tooFar = Report(new TrafficFrame(2, Model, new[] { ActorOn(Agent, route, track, far, slow) }), Agent, route);
            Assert.That(tooFar.HasRequest, Is.True);
            Assert.That(tooFar.RequestValid, Is.False);
            Assert.That(tooFar.Rejection, Is.EqualTo(JunctionRequestRejection.TooFar));

            // Leader sur la meme approche, qui va ailleurs (mouvement voisin), entre le demandeur et l'entree.
            var sibling = Index.OutgoingMovements(Index.FromCorridorOf(EastStraight)).First(m => m != EastStraight);
            var leaderRoute = RouteVia(sibling, Other);
            var leaderTrack = ReferenceTrack.FromRoute(Model, leaderRoute.Occurrences, 0f);
            float leaderEntry = EntryDistance(leaderRoute, sibling);
            var masked = new TrafficFrame(3, Model, new[] { ActorOn(Agent, route, track, near, speed),
                ActorOn(Other, leaderRoute, leaderTrack, leaderEntry - Car.FrontMeters - 1f, speed) });
            var behind = Report(masked, Agent, route);
            Assert.That(behind.HasRequest, Is.True);
            Assert.That(behind.Request.HeadOfQueue, Is.False);
            Assert.That(behind.Request.MaskingActorId, Is.EqualTo(Other));
            Assert.That(behind.Rejection, Is.EqualTo(JunctionRequestRejection.NotHeadOfQueue));

            // Leader deja localise sur son mouvement, l'arriere encore sur l'approche : il masque toujours l'entree.
            var engagedLeader = new TrafficFrame(4, Model, new[] { ActorOn(Agent, route, track, near, speed),
                ActorOn(Other, leaderRoute, leaderTrack, leaderEntry + 1f, speed) });
            TrafficActor other;
            Assert.That(engagedLeader.TryGetActor(Other, out other) && other.Location.ElementId == sibling, Is.True);
            var stillMasked = Report(engagedLeader, Agent, route);
            Assert.That(stillMasked.RequestValid, Is.False, stillMasked.ToText());
            Assert.That(stillMasked.Request.MaskingActorId, Is.EqualTo(Other));
            Assert.That(Report(engagedLeader, Other, leaderRoute).OccupiedMovements, Does.Contain(sibling));
        }

        [Test]
        public void AnEngagedTraversalIsHeldWithoutRequestAndTheRequestMovesToTheNextOne()
        {
            var route = RouteVia(EastStraight, Agent);
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            float entry = EntryDistance(route, EastStraight);
            float speed = 4f;
            float d = DistancesAt(speed).StopMeters - 1f;
            var frame = new TrafficFrame(5, Model, new[] { ActorOn(Agent, route, track, entry - Car.FrontMeters - d, speed) });
            var coordinator = new JunctionCoordinator(Model, 4);
            var granted = coordinator.Resolve(4, new[] { Report(new TrafficFrame(4, Model, new[] {
                ActorOn(Agent, route, track, entry - Car.FrontMeters - d - 0.08f, speed) }), Agent, route) });
            JunctionRecord record;
            Assert.That(granted.TryGetEffectiveGrant(Agent, EastStraight, 5, out record), Is.True, granted.ToText());

            var engaged = Report(frame, Agent, route, granted);
            Assert.That(engaged.Approaches[0].Traversal.FirstMovementId, Is.EqualTo(EastStraight));
            Assert.That(engaged.Approaches[0].Engaged, Is.True, "grant effectif et d < D_stop");
            if (engaged.HasRequest)
                Assert.That(engaged.Request.Traversal.FirstMovementId, Is.Not.EqualTo(EastStraight), "decision O8 : demande suivante");
            var held = coordinator.Resolve(5, new[] { engaged });
            Assert.That(held.TryGetEffectiveGrant(Agent, EastStraight, 6, out record), Is.True);
            Assert.That(record.Reason, Is.EqualTo(JunctionReason.Committed));

            // Instantane decale : aucun grant, la traversee redevient la demande et la contrainte s'appliquerait.
            var stale = Report(new TrafficFrame(7, Model, new[] { ActorOn(Agent, route, track, entry - Car.FrontMeters - d, speed) }), Agent, route, granted);
            Assert.That(stale.Approaches[0].GrantEffective, Is.False, "un instantane decale vaut aucun grant");
            Assert.That(stale.Request.Traversal.FirstMovementId, Is.EqualTo(EastStraight));
            Assert.That(new TrafficJunctionOutcome(7, stale, granted, true).SnapshotStale, Is.True);
        }

        [TestCase(JunctionReason.Committed)]
        [TestCase(JunctionReason.CommittedCarried)]
        [TestCase(JunctionReason.Restored)]
        public void ACommittedRoundaboutContinuationKeepsEngagementAndRequestsTheNextTraversal(JunctionReason reason)
        {
            RoutePlan route = null;
            RoadId enteredMovement = RoadId.None;
            int ring = 0, last = -1, next = -1;
            foreach (var movement in Model.Movements.OrderBy(m => m.Id))
            {
                EffectiveLaneCorridor corridor;
                var predecessor = Model.Movements.FirstOrDefault(m => m.ToCorridorId == movement.FromCorridorId
                    && m.JunctionId == movement.JunctionId);
                if (predecessor.Id.IsEmpty
                    || !Model.TryGetCorridor(movement.FromCorridorId, out corridor)
                    || corridor.LengthMeters <= Car.FrontMeters + Car.RearMeters + 2f * DistancesAt(0f).StopMeters) continue;
                var candidate = RouteVia(movement.Id, Agent);
                int first = candidate.Occurrences.ToList().FindIndex(o => o.Id == movement.Id);
                int end = first, following = -1;
                for (int k = first + 1; k < candidate.Occurrences.Count; k++)
                {
                    if (candidate.Occurrences[k].Kind != RoadElementKind.JunctionMovement) continue;
                    if (Index.JunctionOf(candidate.Occurrences[k].Id) != movement.JunctionId) { following = k; break; }
                    end = k;
                }
                if (following < 0) continue;
                route = candidate; enteredMovement = predecessor.Id; last = end; next = following;
                break;
            }
            Assert.That(route, Is.Not.Null, "route MVP : anneau assez long puis carrefour suivant");
            float ringStart = 0f;
            for (int k = 0; k < ring; k++) ringStart += route.Occurrences[k].EndSMeters - route.Occurrences[k].StartSMeters;
            // Garder de la marge pour l'occupation structuree conservative dans ce corridor courbe.
            float distance = ringStart + (route.Occurrences[ring].EndSMeters - route.Occurrences[ring].StartSMeters) * 0.4f;
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            var frame = new TrafficFrame(7, Model, new[] { ActorOn(Agent, route, track, distance, 0f) });
            TrafficActor actor;
            Assert.That(frame.TryGetActor(Agent, out actor) && actor.Location.ElementId == route.Occurrences[ring].Id, Is.True,
                "vehicule reellement localise sur le corridor interne");
            // Route re-emise depuis l'anneau : le grant original conserve aussi le mouvement deja franchi.
            var movements = new[] { enteredMovement }.Concat(route.Occurrences.Take(last + 1)
                .Where(o => o.Kind == RoadElementKind.JunctionMovement).Select(o => o.Id)).ToArray();
            var snapshot = new JunctionSnapshot(6, 7, new[] { new JunctionRecord(Agent, Index.JunctionOf(movements[0]), movements[0],
                movements, 1, 6, 7, JunctionGrantStatus.Held, reason) }, default(JunctionBatchCounters));
            var report = Report(frame, Agent, route, snapshot);
            Assert.That(report.Approaches[0].DistanceMeters, Is.GreaterThanOrEqualTo(report.Approaches[0].Distances.StopMeters),
                "la continuation est devant, hors de sa propre distance d'engagement");
            Assert.That(report.Approaches[0].GrantEffective, Is.True);
            Assert.That(report.Approaches[0].Engaged, Is.True, "O8 : l'entree originale a deja engage toute la traversee");
            Assert.That(report.HasRequest, Is.True, report.ToText());
            Assert.That(report.Request.Traversal.FirstMovementId, Is.EqualTo(route.Occurrences[next].Id), report.ToText());

            var stale = new JunctionSnapshot(5, 6, snapshot.Records, snapshot.Counters);
            var ungranted = Report(frame, Agent, route, stale);
            Assert.That(ungranted.Approaches[0].GrantEffective, Is.False, "un engagement ancien ne contourne pas EffectiveFrame");
            Assert.That(ungranted.Request.Traversal.FirstMovementId, Is.EqualTo(route.Occurrences[ring + 1].Id));
        }

        // ================================================================== carrefour libre : vehicule seul sur les 11 routes

        [Serializable]
        private sealed class CampaignTriplet
        {
            public int Index;
            public string Entry;
            public string Exit;
            public string Via;
            public ulong Seed;
            public ulong InsertionCounter;
        }

        [Serializable]
        private sealed class Campaign
        {
            public CampaignTriplet[] Triplets;
        }

        /// <summary>
        /// Vehicule seul, point-masse, pipeline reel du driver a chaque pas : frame, spine (route a jour, horizon borne D14),
        /// perception, plan de vitesse, rapport de coordination, arbitrage avec l'entree, lot du coordinateur. Depart a l'arret
        /// au portail d'entree. Rend les pas ou la contrainte JunctionEntry etait active.
        /// </summary>
        private static List<string> DriveAlone(CampaignTriplet triplet, out int grants, out int steps,
            System.Text.StringBuilder diagnostics = null)
        {
            var driver = Driver;
            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>("Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset").Profile;
            float grip = vehicle.LateralFrictionCoefficient * 9.81f;
            var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, RoadId.Parse(triplet.Entry), RoadId.Parse(triplet.Exit),
                triplet.Seed, triplet.InsertionCounter, driver, Dt, RoadId.Parse(triplet.Via));
            Assert.That(insertion.Code, Is.EqualTo(TrafficV2Code.Allowed));
            var route = insertion.Route;
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            float length = 0f;
            foreach (var o in route.Occurrences) length += o.EndSMeters - o.StartSMeters;
            var coordinator = new JunctionCoordinator(Model, 1);
            var buffer = new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity);
            var memory = LongitudinalMemory.None;
            var via = route.ViaMovementId;
            var active = new List<string>();
            float s = 0f, v = 0f;
            grants = 0;
            ulong frameId = 1;
            for (; frameId < 40000 && s < length - Car.FrontMeters - 1f; frameId++)
            {
                var frame = new TrafficFrame(frameId, Model, new[] { ActorOn(insertion.TrafficId, route, track, s, v) });
                int piece = track.PieceAt(s);
                var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, route, insertion.ExitPortalId,
                    insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared, null, null,
                    Admission.Evidence, via, track.OffsetRadians(piece, s),
                    PlanningReach.For(driver, v, Math.Max(v * Dt, MotionCommand.PreviewFloorMeters), TrafficV2Settings.PlanningReachMarginMeters)));
                Assert.That(decision.Motion, Is.Not.Null, "route " + triplet.Index + " frame " + frameId + " : " + decision.Projection.Code);
                route = decision.Route.Plan;
                if (!via.IsEmpty && route.ViaOccurrenceIndex >= 0 && route.ProgressOccurrenceIndex > route.ViaOccurrenceIndex) via = RoadId.None;
                var observation = TrafficPerception.Observe(frame, insertion.TrafficId, decision.PerceptionPath, TrafficV2Settings.PerceptionLimits, buffer);
                var report = JunctionRequestBuilder.Build(frame, Index, insertion.TrafficId, route, driver, Dt, Margin, coordinator.Current,
                    frameId, HoldEntrySpeed);
                var entry = report.HasRequest ? new JunctionEntryInput(report.Request.Traversal.FirstMovementId, report.Request.DistanceMeters,
                    report.Request.Distances.EngageThresholdMeters, Margin, report.Request.GrantEffective, report.Request.Distances.StopMeters)
                    : default(JunctionEntryInput);
                var plan = SpeedPlan.Build(decision.Motion, Model, driver, v, grip);
                Assert.That(plan.Accepted, Is.True);
                var perceived = LongitudinalPerception.From(observation, decision.PerceptionPath,
                    LongitudinalPerception.FrontDistanceMeters(frame, insertion.TrafficId, decision.PerceptionPath), false);
                var longitudinal = LongitudinalArbitration.Decide(plan, driver, v, Dt, perceived, memory, TrafficV2Settings.StopHold, entry);
                memory = longitudinal.Memory;
                if (entry.Active)
                {
                    active.Add(string.Format(CultureInfo.InvariantCulture, "frame {0} : d {1:0.###} m, v {2:0.###} m/s, D_engage {3:0.###}, D_request {4:0.###}, liante {5} {6:0.###}",
                        frameId, entry.DistanceMeters, v, report.Request.Distances.EngageThresholdMeters, report.Request.Distances.RequestThresholdMeters,
                        longitudinal.Binding.Kind, longitudinal.TargetAccelerationMetersPerSecondSquared));
                    if (diagnostics != null && active.Count <= 6)
                        diagnostics.Append("route ").Append(triplet.Index).Append(' ').Append(active[active.Count - 1]).Append("\n  ")
                            .Append(report.ToText().Replace("\n", "\n  ")).Append("\n  route ")
                            .Append(string.Join(",", route.Occurrences.Skip(route.ProgressOccurrenceIndex).Take(9).Select(o => o.Id.ToString().Substring(0, 8)).ToArray()))
                            .Append(" progression ").Append(route.ProgressOccurrenceIndex).Append("\n  ")
                            .Append(coordinator.Current.ToText().Replace("\n", "\n  ")).Append('\n');
                }
                var snapshot = coordinator.Resolve(frameId, new[] { report });
                Assert.That(snapshot.Records.Any(x => x.Status == JunctionGrantStatus.Denied), Is.False, snapshot.ToText());
                grants += snapshot.Records.Count(x => x.Status == JunctionGrantStatus.Granted);
                v = Math.Max(0f, v + longitudinal.AppliedAccelerationMetersPerSecondSquared * Dt);
                s += v * Dt;
            }
            steps = (int)frameId;
            return active;
        }

        [Test]
        public void AVehicleAloneNeverMeetsAnActiveEntryConstraintOnTheElevenCampaignRoutes()
        {
            var campaign = JsonUtility.FromJson<Campaign>(File.ReadAllText(CampaignPath));
            Assert.That(campaign.Triplets.Length, Is.EqualTo(11));
            var lines = new List<string>();
            var failures = new List<string>();
            var diagnostics = new System.Text.StringBuilder();
            foreach (var triplet in campaign.Triplets)
            {
                int grants, steps;
                var active = DriveAlone(triplet, out grants, out steps, diagnostics);
                lines.Add("route " + triplet.Index + " : " + grants + " traversees accordees, " + steps + " pas, " + active.Count + " pas avec JunctionEntry active");
                foreach (var line in active.Take(4)) failures.Add("route " + triplet.Index + ", " + line);
                Assert.That(grants, Is.GreaterThan(0));
            }
            Directory.CreateDirectory(ScenarioFolder);
            File.WriteAllText(ScenarioFolder + "/free-junction-diagnostic.txt", string.Join("\n", lines) + "\n\n" + diagnostics);
            TestContext.WriteLine("carrefour libre, vehicule seul :\n" + string.Join("\n", lines));
            Assert.That(failures, Is.Empty, "JunctionEntry active sur carrefour libre (HALT) :\n" + string.Join("\n", lines) + "\n"
                + string.Join("\n", failures));
        }

        // ================================================================== projection et garde

        [Test]
        public void TheJunctionProjectionTextIsDeterministicAndCultureInvariant()
        {
            var route = RouteVia(EastStraight, Agent);
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            float entry = EntryDistance(route, EastStraight);
            var frame = new TrafficFrame(9, Model, new[] { ActorOn(Agent, route, track, entry - Car.FrontMeters - 5.5f, 3.25f) });
            var report = Report(frame, Agent, route);
            var snapshot = new JunctionCoordinator(Model, 9).Resolve(9, new[] { report });
            Func<string> render = () => new TrafficJunctionOutcome(10, report, snapshot, false).ToText() + "\n" + snapshot.ToText();
            var culture = Thread.CurrentThread.CurrentCulture;
            string invariant, french;
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                invariant = render();
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                french = render();
            }
            finally { Thread.CurrentThread.CurrentCulture = culture; }
            Assert.That(french, Is.EqualTo(invariant));
            Assert.That(invariant, Does.Contain("Granted(Granted)"));
            Assert.That(invariant, Does.Contain("D_request"));
            Assert.That(invariant, Does.Contain("EnteredWithoutGrant 0"));
            Assert.That(invariant, Does.Contain("IncompatibleOccupancy 0"));
            TestContext.WriteLine(invariant);
        }

        [Test]
        [Category("Story535")]
        public void TheJunctionFolderUsesNoPhysicsNorControlPathAndDeclaresNoPedalMember()
        {
            var files = Directory.GetFiles(Path.Combine(TrafficRootPath, "Junction"), "*.cs");
            // Responsabilites attendues ; leur repartition en fichiers peut evoluer sans changer le contrat.
            var expected = new[] { typeof(JunctionCoordinator), typeof(JunctionConflictIndex), typeof(JunctionRequestBuilder),
                typeof(JunctionDistances), typeof(JunctionTraversal), typeof(JunctionActorReport), typeof(JunctionRecord),
                typeof(JunctionSnapshot), typeof(RightOfWay), typeof(RightOfWayTable), typeof(JunctionPriority), typeof(JunctionKinematics) };
            var types = typeof(JunctionCoordinator).Assembly.GetTypes().Where(t => t.IsPublic && t.Namespace == typeof(JunctionCoordinator).Namespace).ToList();
            Assert.That(types, Is.SupersetOf(expected), "Coordination, index, demandes, distances, records et regles pures.");
            foreach (var file in files)
            {
                string source = File.ReadAllText(file);
                foreach (var forbidden in new[] { "VehicleDriveIntent", "VehiclePhysicsBody", "ApplyDriveIntent", "Rigid" + "body",
                    "Mono" + "Behaviour", "NetworkVariable", "Rp" + "c", "LateralClearanceMarginMeters", "Physics" + ".", "UnityEngine" + ".Object" })
                    Assert.That(source, Does.Not.Contain(forbidden), file);
            }
            foreach (var type in types)
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    foreach (var word in new[] { "Brake", "Throttle", "Pedal" })
                        Assert.That(member.Name, Does.Not.Contain(word), type.Name);
            // Le coordinateur ne choisit pas de route et ne commande aucun vehicule.
            foreach (var member in typeof(JunctionCoordinator).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                Assert.That(member.Name, Does.Not.Match("(?i)(route|intent|steer|command)"), member.Name);
        }

        // ================================================================== scenarios PlayMode C et D

        [Serializable]
        private sealed class VectorRecord
        {
            public float x, y, z;
            public static VectorRecord Of(Vector3 v) { return new VectorRecord { x = v.x, y = v.y, z = v.z }; }
        }

        [Serializable]
        private sealed class InsertionRecord
        {
            public string Entry;
            public string Exit;
            public ulong Seed;
            public ulong EarliestStep;
            public string[] Elements;
        }

        [Serializable]
        private sealed class ObstacleRecord
        {
            public string Label;
            public string ElementId;
            public float RouteDistanceMeters;
            public VectorRecord Center;
            public VectorRecord Forward;
            public VectorRecord Size;
            public int FromStep;
            /// <summary>-1 : retire par le test sur condition.</summary>
            public int UntilStep;
        }

        [Serializable]
        private sealed class PushRecord
        {
            public int Insertion;
            public int AtStep;
            public float LateralImpulsePerKilogram;
        }

        [Serializable]
        private sealed class ScenarioRecord
        {
            public string Label;
            public int MaxPopulation;
            public int MaxSteps;
            public InsertionRecord[] Insertions;
            public ObstacleRecord[] Obstacles;
            public PushRecord[] Pushes;
        }

        [Serializable]
        private sealed class ScenarioFile
        {
            public int Format;
            public string RoadModelVersion;
            public float VehicleLengthMeters;
            public float MinimumGapMeters;
            public float MinimumObstacleDistanceMeters;
            public VectorRecord Parking;
            public ScenarioRecord[] Scenarios;
        }

        private const float ObstacleLengthMeters = 1f;
        private const float ObstacleWidthMeters = 2f;
        private const float ObstacleHeightMeters = 1.5f;

        /// <summary>Premiere insertion (entree, sortie, graine) dont la route, au compteur donne, passe par le mouvement.</summary>
        private static TrafficV2Insertion InsertionVia(RoadId movement, ulong counter, out InsertionRecord record)
        {
            var entries = Model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).ToList();
            var exits = Model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id).ToList();
            for (ulong seed = 0; seed < 16; seed++)
                foreach (var entry in entries)
                    foreach (var exit in exits)
                    {
                        var prepared = TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, exit.Id, seed, counter, Driver, Dt);
                        if (prepared.Code != TrafficV2Code.Allowed || !prepared.Route.Occurrences.Any(o => o.Id == movement)) continue;
                        record = new InsertionRecord { Entry = entry.Id.ToString(), Exit = exit.Id.ToString(), Seed = seed, EarliestStep = 0UL,
                            Elements = prepared.Route.Occurrences.Select(o => o.Id.ToString()).ToArray() };
                        return prepared;
                    }
            Assert.Fail("aucune insertion ne passe par " + movement);
            record = null;
            return null;
        }

        /// <summary>Obstacle de test sur l'element, face proche a l'abscisse de route donnee (element droit exige).</summary>
        private static ObstacleRecord ObstacleAt(RoutePlan route, RoadId elementId, float nearDistance, string label)
        {
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            var piece = track.Pieces.First(p => p.Id == elementId);
            EffectiveLaneCorridor corridor;
            Assert.That(Model.TryGetCorridor(elementId, out corridor), Is.True, "obstacle sur un corridor");
            Assert.That(corridor.Samples.All(s => Math.Abs(s.CurvaturePerMeter) < 1e-4f), Is.True, "corridor droit");
            Assert.That(nearDistance, Is.GreaterThanOrEqualTo(piece.StartDistanceMeters + 0.1f));
            Assert.That(nearDistance + ObstacleLengthMeters, Is.LessThanOrEqualTo(piece.EndDistanceMeters - 0.1f));
            var nominal = piece.Nominal(nearDistance + ObstacleLengthMeters * 0.5f);
            return new ObstacleRecord { Label = label, ElementId = elementId.ToString(), RouteDistanceMeters = nearDistance,
                Center = VectorRecord.Of(nominal.Position + nominal.Up * (ObstacleHeightMeters * 0.5f)), Forward = VectorRecord.Of(nominal.Forward),
                Size = VectorRecord.Of(new Vector3(ObstacleWidthMeters, ObstacleHeightMeters, ObstacleLengthMeters)), FromStep = 0, UntilStep = -1 };
        }

        /// <summary>Obstacle sur le corridor d'approche du mouvement, face proche a <paramref name="beforeEntry"/> de son entree.</summary>
        private static ObstacleRecord BeforeEntry(RoutePlan route, RoadId movement, float beforeEntry, string label)
        {
            return ObstacleAt(route, Index.FromCorridorOf(movement), EntryDistance(route, movement) - beforeEntry, label);
        }

        private static string BuildScenarioText()
        {
            var car = Car;
            float length = car.FrontMeters + car.RearMeters;
            InsertionRecord first, second;
            var a = InsertionVia(EastStraight, 1UL, out first);
            var b = InsertionVia(SouthLeft, 2UL, out second);
            Assert.That(Index.ToCorridorOf(EastStraight), Is.EqualTo(Index.ToCorridorOf(SouthLeft)), "corridor de depart commun 40e937a9");

            // C : le sud porte le plus petit TrafficId et gagne l'egalite du lot commun ; sa traversee plus longue permet
            // l'arret de l'est sous JunctionGrant avant l'occupation de leur sortie commune (C.3).
            // Les obstacles restent a 8 m ; le test retire celui de l'est puis celui du sud un pas plus tard.
            InsertionRecord southFirst, eastSecond;
            var southC = InsertionVia(SouthLeft, 1UL, out southFirst);
            var eastC = InsertionVia(EastStraight, 2UL, out eastSecond);
            var c = new ScenarioRecord { Label = "C", MaxPopulation = 2, MaxSteps = 9000, Insertions = new[] { southFirst, eastSecond },
                Obstacles = new[] { BeforeEntry(eastC.Route, EastStraight, 8f, "attente-est"), BeforeEntry(southC.Route, SouthLeft, 8f, "attente-sud") },
                Pushes = new PushRecord[0] };

            // D : obstacle sur le corridor de depart commun, entierement hors du mouvement, qui retient le premier vehicule en
            // laissant derriere lui une longueur libre < L + s0 ; le second attend derriere un obstacle sur son approche, retire
            // quand le premier est arrete.
            var exitCorridor = Index.ToCorridorOf(EastStraight);
            float exitStart = EntryDistance(a.Route, EastStraight) + Model.Movements.First(m => m.Id == EastStraight).LengthMeters;
            float stopGap = Driver.MinimumGap + 0.4f;
            float near = exitStart + length + stopGap + (length + Driver.MinimumGap) * 0.5f;
            var d = new ScenarioRecord { Label = "D", MaxPopulation = 2, MaxSteps = 9000, Insertions = new[] { first, second },
                Obstacles = new[] { ObstacleAt(a.Route, exitCorridor, near, "sortie-tenue"), BeforeEntry(b.Route, SouthLeft, 8f, "attente-sud") },
                Pushes = new PushRecord[0] };

            float maxX = float.NegativeInfinity, minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
            foreach (var corridor in Model.Corridors)
                foreach (var sample in corridor.Samples)
                { maxX = Math.Max(maxX, sample.Position.x); minZ = Math.Min(minZ, sample.Position.z); maxZ = Math.Max(maxZ, sample.Position.z); }
            var file = new ScenarioFile { Format = 1, RoadModelVersion = Model.Version.ToString(), VehicleLengthMeters = length,
                MinimumGapMeters = Driver.MinimumGap, MinimumObstacleDistanceMeters = 8f,
                Parking = VectorRecord.Of(new Vector3(maxX + 150f, 0f, 0.5f * (minZ + maxZ))), Scenarios = new[] { c, d } };
            return JsonUtility.ToJson(file, true);
        }

        [Test]
        [Category("Story535")]
        public void TheScenarioBuilderIsDeterministicAndWritesScenariosCAndD()
        {
            string firstText = BuildScenarioText();
            Assert.That(BuildScenarioText(), Is.EqualTo(firstText), "memes entrees, memes scenarios");
            var file = JsonUtility.FromJson<ScenarioFile>(firstText);
            Assert.That(file.Scenarios.Select(s => s.Label), Is.EqualTo(new[] { "C", "D" }));
            var c = file.Scenarios[0];
            Assert.That(c.Insertions[0].Elements, Does.Contain(SouthLeft.ToString()));
            Assert.That(c.Insertions[1].Elements, Does.Contain(EastStraight.ToString()));
            Assert.That(c.Obstacles.Length, Is.EqualTo(2));
            var d = file.Scenarios[1];
            Assert.That(d.Obstacles[0].ElementId, Is.EqualTo(Index.ToCorridorOf(EastStraight).ToString()), "obstacle sur 40e937a9");
            EffectiveLaneCorridor exit;
            Assert.That(Model.TryGetCorridor(Index.ToCorridorOf(EastStraight), out exit), Is.True);
            TestContext.WriteLine("corridor de sortie commun " + exit.CorridorId + " : " + exit.LengthMeters.ToString("0.##", CultureInfo.InvariantCulture)
                + " m ; obstacle D a " + d.Obstacles[0].RouteDistanceMeters.ToString("0.##", CultureInfo.InvariantCulture) + " m de route");
            Directory.CreateDirectory(ScenarioFolder);
            File.WriteAllText(ScenarioPath, firstText);
        }
    }
}
