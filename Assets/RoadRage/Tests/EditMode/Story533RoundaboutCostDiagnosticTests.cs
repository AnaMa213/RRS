using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.33, diagnostic rond-point contre ligne droite (demande proprietaire du 2026-10-03, apres D14, avant toute
    /// optimisation). N = 1, 2, 4, 8 vehicules places sur la geometrie reelle de MVP_Run (11 routes 5.31, decor charge pour
    /// la collecte physique), tous dans la meme situation, au plus pres les uns des autres (7 m au moins entre centres).
    /// Chaque configuration rejoue le pas hote du runtime : preparation du driver, collecte de dangers, TrafficFrame, puis
    /// pour chaque vehicule spine (portee bornee D14), perception sur toute la route, SpeedPlan, arbitrage et commande.
    /// Mesures : temps par section (meilleur de plusieurs executions), octets alloues, compteurs de travail du runtime
    /// (TrafficV2WorkCounters) par section. Ne juge rien et ne change aucun comportement. Rapport :
    /// roundabout-vs-straight-diagnostic.md.
    /// </summary>
    [Explicit]
    [Category("Core")]
    [Category("Story533RoundaboutBench")]
    public sealed class Story533RoundaboutCostDiagnosticTests
    {
        private const float Dt = 0.02f;
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string CampaignPath = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/campaign-5-31.json";
        private const string ReportPath = "_bmad-output/implementation-artifacts/traffic-v2-5-33-explorations/roundabout-vs-straight-diagnostic.md";
        private const string V2PrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const float MinimumSpacingMeters = 7f;
        private const float ClusterRadiusMeters = 80f;
        /// <summary>Ligne droite : le tiers le moins courbe des emplacements hors carrefour (+3 m), |kappa| max sur +-6 m, seuil >= 1/150 m.</summary>
        private const float StraightCurvatureFloor = 1f / 150f;
        private const int TimedRuns = 3;
        private static readonly int[] Populations = { 1, 2, 4, 8 };
        private static readonly string[] Situations = { "ligne droite", "rond-point" };
        private static readonly string[] Phases = { "preparation", "collecte", "frame", "spine", "perception", "SpeedPlan", "arbitrage+commande" };
        private static readonly FieldInfo[] WorkFields = typeof(TrafficV2Work).GetFields(BindingFlags.Public | BindingFlags.Instance);

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

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; }
        }

        // ================================================================== routes, emplacements et situations

        [Serializable]
        private sealed class CampaignTripletRecord
        {
            public int Index;
            public string Entry;
            public string Exit;
            public string Via;
            public ulong Seed;
            public ulong InsertionCounter;
        }

        [Serializable]
        private sealed class CampaignFile
        {
            public CampaignTripletRecord[] Triplets;
        }

        /// <summary>Une identite de vehicule sur une route : plan et objectif propres a chaque metre (progression acquise).</summary>
        private sealed class Slot
        {
            public TrafficV2Insertion Insertion;
            public ReferenceTrack Track;
            public RoutePlan[] Routes;
            public RoadId[] Vias;
        }

        private sealed class Lane
        {
            public CampaignTripletRecord Triplet;
            public RoadId[] Ids;
            public readonly List<Slot> Slots = new List<Slot>();
        }

        private struct Spot
        {
            public int Lane;
            public int Distance;
            public Vector3 Position;
        }

        private static float SpeedAt(ReferenceTrack track, int piece, float distance, DriverProfile driver)
        {
            var p = track.Pieces[piece];
            float kappa = Mathf.Abs(p.Curve.Sample(p.ElementS(distance)).CurvaturePerMeter);
            return kappa > 1e-6f ? Mathf.Min(driver.DesiredSpeed, Mathf.Sqrt(driver.ComfortableDeceleration / kappa)) : driver.DesiredSpeed;
        }

        /// <summary>Plan et objectif a chaque metre, comme les acquiert le driver en roulant (le plan est reutilise, pas recherche).</summary>
        private static Slot Walk(Lane lane, int index, DriverProfile driver)
        {
            var model = Admission.Model;
            var t = lane.Triplet;
            var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, RoadId.Parse(t.Entry), RoadId.Parse(t.Exit), t.Seed,
                t.InsertionCounter + 1000UL * (ulong)index, driver, Dt, string.IsNullOrEmpty(t.Via) ? RoadId.None : RoadId.Parse(t.Via));
            if (insertion.Code != TrafficV2Code.Allowed) return null;
            if (lane.Ids != null && !insertion.Route.Occurrences.Select(o => o.Id).SequenceEqual(lane.Ids)) return null;
            var track = ReferenceTrack.FromRoute(model, insertion.Route.Occurrences, 0f);
            int length = (int)Math.Floor(track.LengthMeters);
            var slot = new Slot { Insertion = insertion, Track = track, Routes = new RoutePlan[length], Vias = new RoadId[length] };
            var route = insertion.Route;
            var via = insertion.ViaMovementId;
            for (int d = 0; d < length; d++)
            {
                int piece = track.PieceAt(d);
                var nominal = track.Nominal(piece, d);
                var pose = new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward, Up = nominal.Up, Footprint = Car };
                float speed = SpeedAt(track, piece, d, driver);
                var frame = new TrafficFrame(1, model, new[] { new TrafficActorInput(insertion.TrafficId, pose, speed,
                    track.Pieces[piece].Id, TrafficV2Lifecycle.ExpectedElements(route), track.KinematicAnchors(piece)) });
                if (!via.IsEmpty && route.ViaOccurrenceIndex >= 0 && route.ProgressOccurrenceIndex > route.ViaOccurrenceIndex) via = RoadId.None;
                PlanningDecision decision;
                try { decision = PlanningSpine.Evaluate(Request(frame, insertion, route, via, track.OffsetRadians(piece, d), speed, driver)); }
                catch (ArgumentException) { continue; }
                catch (InvalidOperationException) { continue; }
                if (decision.Route.Plan == null || decision.Path == null || decision.Motion == null || decision.Motion.Issue != MotionIssue.None) continue;
                route = decision.Route.Plan;
                slot.Routes[d] = route;
                slot.Vias[d] = via;
            }
            return slot;
        }

        private static PlanningRequest Request(TrafficFrame frame, TrafficV2Insertion insertion, RoutePlan route, RoadId via,
            float offset, float speed, DriverProfile driver)
        {
            return new PlanningRequest(frame, insertion.TrafficId, route, insertion.ExitPortalId, insertion.Seed,
                TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared, null, null, Admission.Evidence,
                via, offset, PlanningReach.For(driver, speed, Math.Max(Math.Max(0f, speed) * Dt, MotionCommand.PreviewFloorMeters),
                    TrafficV2Settings.PlanningReachMarginMeters));
        }

        private static Slot SlotOf(Lane lane, int index, DriverProfile driver)
        {
            while (lane.Slots.Count <= index) lane.Slots.Add(Walk(lane, lane.Slots.Count, driver));
            return lane.Slots[index];
        }

        private static VehicleFootprint car;

        private static VehicleFootprint Car
        {
            get
            {
                if (car.FrontMeters <= 0f)
                    car = TrafficV2VehicleDriver.FootprintOf(AssetDatabase.LoadAssetAtPath<GameObject>(V2PrefabPath).GetComponent<BoxCollider>());
                return car;
            }
        }

        /// <summary>
        /// 1 rond-point ; sinon, hors de toute frontiere de carrefour (marge 3 m), la plus grande |kappa| sur +-6 m (0 ou
        /// plus, retournee en negatif - 2 pour la distinguer) ; -1 ni l'un ni l'autre. Le seuil de ligne droite est fixe ensuite.
        /// </summary>
        private static float Curvedness(ReferenceTrack track, float distance, Vector3 position)
        {
            var model = Admission.Model;
            foreach (var junction in model.Junctions)
            {
                var c = junction.Boundary.Center; var e = junction.Boundary.Extents;
                bool inside = Mathf.Abs(position.x - c.x) <= e.x && Mathf.Abs(position.z - c.z) <= e.z && Mathf.Abs(position.y - c.y) <= e.y + 2f;
                if (inside && junction.Feature == JunctionFeature.Roundabout) return 1f;
            }
            foreach (var junction in model.Junctions)
            {
                var c = junction.Boundary.Center; var e = junction.Boundary.Extents;
                if (Mathf.Abs(position.x - c.x) <= e.x + 3f && Mathf.Abs(position.z - c.z) <= e.z + 3f) return -1f;
            }
            float max = 0f;
            for (float d = distance - 6f; d <= distance + 6f; d += 1f)
            {
                if (d < 0f || d > track.LengthMeters) return -1f;
                var p = track.Pieces[track.PieceAt(d)];
                max = Mathf.Max(max, Mathf.Abs(p.Curve.Sample(p.ElementS(d)).CurvaturePerMeter));
            }
            return -2f - max;
        }

        private static float straightThreshold;

        // ================================================================== mesure d'une configuration

        private sealed class Vehicle
        {
            public Slot Slot;
            public int Distance;
            public int Piece;
            public float Speed;
            public VehicleFootprintPose Pose;
            public RoutePlan Route;
            public RoadId Via;
            public float Offset;
            // Sorties de la derniere execution instrumentee (sections chronometrees a part).
            public PlanningDecision Decision;
            public SpeedPlan Plan;
            public RoadLocation Location;
        }

        private sealed class Measure
        {
            public int Situation, Population;
            public double Total;
            public readonly double[] Phase = new double[Phases.Length];
            public double Overlap, Localize, Route, Horizon, HorizonWithoutE, RoutePath, Motion, Verify;
            public long Bytes;
            public long[][] Work;
            public TrafficHazardCollectorCounters Collector;
            public double SamplesPerMeter;
        }

        private static long[] Snap()
        {
            object work = TrafficV2WorkCounters.Work;
            var values = new long[WorkFields.Length];
            for (int i = 0; i < WorkFields.Length; i++) values[i] = (long)WorkFields[i].GetValue(work);
            return values;
        }

        private static void Accumulate(long[] into, long[] before, long[] after)
        {
            for (int i = 0; i < into.Length; i++) into[i] += after[i] - before[i];
        }

        /// <summary>
        /// Un pas hote, dans l'ordre du runtime. <paramref name="times"/> recoit le temps de chaque section (ms) ;
        /// <paramref name="work"/> non nul, les compteurs de chaque section (instantanes hors des sections chronometrees).
        /// </summary>
        private static double Step(List<Vehicle> vehicles, TrafficV2HazardCollector collector, SpatialQueryBuffer buffer,
            DriverProfile driver, double[] times, long[][] work, ref double overlap)
        {
            var model = Admission.Model;
            long[] before = null;
            var watch = new Stopwatch();
            var total = Stopwatch.StartNew();

            // Preparation du driver : projection sur la reference de mesure, ancres, elements attendus.
            if (work != null) before = Snap();
            watch.Restart();
            var inputs = new List<TrafficActorInput>(vehicles.Count);
            var queries = new List<HazardQuery>(vehicles.Count);
            var actorIds = new HashSet<RoadId>();
            foreach (var v in vehicles)
            {
                int piece;
                v.Slot.Track.Project(v.Pose.Position, v.Piece, out piece);
                inputs.Add(new TrafficActorInput(v.Slot.Insertion.TrafficId, v.Pose, v.Speed, v.Slot.Track.Pieces[v.Piece].Id,
                    TrafficV2Lifecycle.ExpectedElements(v.Route), v.Slot.Track.KinematicAnchors(piece)));
                queries.Add(new HazardQuery(v.Slot.Insertion.TrafficId, v.Pose.Position, null));
                actorIds.Add(v.Slot.Insertion.TrafficId);
            }
            times[0] = watch.Elapsed.TotalMilliseconds;
            if (work != null) { var after = Snap(); Accumulate(work[0], before, after); before = after; }

            watch.Restart();
            var hazards = collector.Collect(queries, actorIds);
            times[1] = watch.Elapsed.TotalMilliseconds;
            overlap = collector.LastOverlapMilliseconds;
            if (work != null) { var after = Snap(); Accumulate(work[1], before, after); before = after; }

            watch.Restart();
            var frame = new TrafficFrame(7, model, inputs, hazards);
            times[2] = watch.Elapsed.TotalMilliseconds;
            if (work != null) { var after = Snap(); Accumulate(work[2], before, after); before = after; }

            for (int p = 3; p < Phases.Length; p++) times[p] = 0d;
            foreach (var v in vehicles)
            {
                var id = v.Slot.Insertion.TrafficId;
                watch.Restart();
                PlanningDecision decision = null;
                try { decision = PlanningSpine.Evaluate(Request(frame, v.Slot.Insertion, v.Route, v.Via, v.Offset, v.Speed, driver)); }
                catch (ArgumentException) { }
                catch (InvalidOperationException) { }
                times[3] += watch.Elapsed.TotalMilliseconds;
                if (work != null) { var after = Snap(); Accumulate(work[3], before, after); before = after; }

                watch.Restart();
                var observation = default(AgentObservation);
                if (decision != null && decision.Path != null && decision.PerceptionPath != null)
                {
                    try { observation = TrafficPerception.Observe(frame, id, decision.PerceptionPath, TrafficV2Settings.PerceptionLimits, buffer); }
                    catch (ArgumentException) { }
                }
                times[4] += watch.Elapsed.TotalMilliseconds;
                if (work != null) { var after = Snap(); Accumulate(work[4], before, after); before = after; }

                watch.Restart();
                SpeedPlan plan = decision != null && decision.Motion != null ? SpeedPlan.Build(decision.Motion, model, driver, v.Speed) : null;
                times[5] += watch.Elapsed.TotalMilliseconds;
                if (work != null) { var after = Snap(); Accumulate(work[5], before, after); before = after; }

                watch.Restart();
                if (plan != null && plan.Accepted)
                {
                    var perceived = LongitudinalPerception.From(observation, decision.PerceptionPath,
                        LongitudinalPerception.FrontDistanceMeters(frame, id, decision.PerceptionPath), false);
                    var longitudinal = LongitudinalArbitration.Decide(plan, driver, v.Speed, Dt, perceived, LongitudinalMemory.None,
                        TrafficV2Settings.StopHold);
                    int stepPiece;
                    v.Slot.Track.Project(v.Pose.Position, v.Piece, out stepPiece);
                    var first = decision.Path.Intervals[0];
                    RoadElementKind kind;
                    RoadCurve curve;
                    IReadOnlyList<RoadCurveSample> samples;
                    if (TrafficFrame.TryGetElement(model, first.Id, out kind, out curve, out samples))
                        MotionCommand.Track(7, TrafficV2Settings.PlanValiditySteps, plan, driver, model.DrivabilityProfile, curve,
                            first.StartSMeters, v.Pose.Position, v.Pose.Forward, v.Speed, Dt,
                            v.Slot.Track.NominalHeadingErrorDegrees(stepPiece, v.Distance), longitudinal);
                }
                times[6] += watch.Elapsed.TotalMilliseconds;
                if (work != null) { var after = Snap(); Accumulate(work[6], before, after); before = after; }

                TrafficActor actor;
                frame.TryGetActor(id, out actor);
                v.Decision = decision; v.Plan = plan; v.Location = actor.Location;
            }
            return total.Elapsed.TotalMilliseconds;
        }

        private static double Time(Action action)
        {
            action();
            double best = double.MaxValue;
            for (int r = 0; r < TimedRuns; r++)
            {
                var watch = Stopwatch.StartNew();
                action();
                best = Math.Min(best, watch.Elapsed.TotalMilliseconds);
            }
            return best;
        }

        private static long Allocated(Action action)
        {
            for (int attempt = 0; attempt < 6; attempt++)
            {
                int collections = GC.CollectionCount(0);
                long before = GC.GetTotalMemory(false);
                action();
                long after = GC.GetTotalMemory(false);
                if (GC.CollectionCount(0) == collections) return Math.Max(0L, after - before);
            }
            return -1L;
        }

        private static Measure Run(int situation, List<Vehicle> vehicles, TrafficV2HazardCollector collector, DriverProfile driver)
        {
            var model = Admission.Model;
            var buffer = new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity);
            var m = new Measure { Situation = situation, Population = vehicles.Count, Total = double.MaxValue };
            for (int p = 0; p < Phases.Length; p++) m.Phase[p] = double.MaxValue;
            m.Overlap = double.MaxValue;
            var times = new double[Phases.Length];
            double overlap = 0d;
            Step(vehicles, collector, buffer, driver, times, null, ref overlap); // chauffe
            for (int r = 0; r < TimedRuns; r++)
            {
                double total = Step(vehicles, collector, buffer, driver, times, null, ref overlap);
                m.Total = Math.Min(m.Total, total);
                for (int p = 0; p < Phases.Length; p++) m.Phase[p] = Math.Min(m.Phase[p], times[p]);
                m.Overlap = Math.Min(m.Overlap, overlap);
            }
            m.Bytes = Allocated(() => Step(vehicles, collector, buffer, driver, times, null, ref overlap));
            m.Work = Phases.Select(_ => new long[WorkFields.Length]).ToArray();
            Step(vehicles, collector, buffer, driver, times, m.Work, ref overlap);
            m.Collector = collector.Counters;

            // Sous-sections chronometrees a part, sur les memes entrees (les compteurs ne les voient pas).
            var bounds = new LongitudinalBounds(driver.MaxAcceleration, driver.SafeBrakingLimit);
            var evidence = Admission.Evidence;
            var seams = evidence.Valid && evidence.PoseModel == NominalPoseModel.Kinematic ? evidence.SignedRingSeams : null;
            foreach (var v in vehicles)
            {
                int piece;
                v.Slot.Track.Project(v.Pose.Position, v.Piece, out piece);
                var anchors = v.Slot.Track.KinematicAnchors(piece);
                var expected = TrafficV2Lifecycle.ExpectedElements(v.Route);
                m.Localize += Time(() => RoadLocalizer.Localize(model, v.Pose, v.Slot.Track.Pieces[v.Piece].Id, expected, anchors));
                var location = v.Location;
                m.Route += Time(() => RoutePlanner.Plan(new RouteRequest(model, location, v.Slot.Insertion.ExitPortalId, v.Slot.Insertion.Seed,
                    v.Slot.Insertion.TrafficId, "route", new DecisionCounter(7), v.Route, false, null, v.Via)));
                if (v.Decision == null || v.Decision.Route.Plan == null) continue;
                var plan = v.Decision.Route.Plan;
                var reach = PlanningReach.For(driver, v.Speed, Math.Max(v.Speed * Dt, MotionCommand.PreviewFloorMeters),
                    TrafficV2Settings.PlanningReachMarginMeters);
                m.Horizon += Time(() => PathHorizon.Build(model, plan, TrafficV2Settings.LookAheadMeters, v.Offset, seams, reach));
                m.HorizonWithoutE += Time(() => PathHorizon.Build(model, plan, TrafficV2Settings.LookAheadMeters, null, seams, reach));
                m.RoutePath += Time(() => RoutePath.Build(model, plan, TrafficV2Settings.LookAheadMeters));
                if (v.Decision.Path != null)
                {
                    var path = v.Decision.Path;
                    m.Motion += Time(() => new MotionPlan(path, model.DrivabilityProfile, evidence, TrackingTolerance.Undeclared));
                }
                if (v.Plan != null && v.Decision.Motion != null && v.Plan.Points.Count >= 2)
                {
                    var profile = v.Plan.ToProfile();
                    var motion = v.Decision.Motion;
                    m.Verify += Time(() => motion.VerifySpeedProfile(profile, bounds));
                }
                RoadElementKind kind;
                RoadCurve curve;
                IReadOnlyList<RoadCurveSample> samples;
                if (TrafficFrame.TryGetElement(model, v.Slot.Track.Pieces[v.Piece].Id, out kind, out curve, out samples))
                    m.SamplesPerMeter += curve.SampleCount / Math.Max(1e-3f, curve.Length) / vehicles.Count;
            }
            return m;
        }

        // ================================================================== campagne

        [Test]
        public void TheRoundaboutAndTheStraightLineAreMeasuredWithTheSamePopulationAndInstrumentation()
        {
            var model = Admission.Model;
            var driver = Driver;
            var campaign = JsonUtility.FromJson<CampaignFile>(File.ReadAllText(CampaignPath));
            var lanes = new List<Lane>();
            foreach (var triplet in campaign.Triplets)
            {
                var lane = new Lane { Triplet = triplet };
                var first = Walk(lane, 0, driver);
                if (first == null) continue;
                lane.Ids = first.Insertion.Route.Occurrences.Select(o => o.Id).ToArray();
                lane.Slots.Add(first);
                lanes.Add(lane);
            }
            Assert.That(lanes.Count, Is.GreaterThan(0), "aucune route de reference");

            var spots = new List<Spot>[2] { new List<Spot>(), new List<Spot>() };
            var outside = new List<KeyValuePair<float, Spot>>();
            for (int l = 0; l < lanes.Count; l++)
            {
                var slot = lanes[l].Slots[0];
                for (int d = 3; d < slot.Routes.Length - 3; d++)
                {
                    if (slot.Routes[d] == null) continue;
                    int piece = slot.Track.PieceAt(d);
                    var position = slot.Track.Nominal(piece, d).Position;
                    float curvedness = Curvedness(slot.Track, d, position);
                    if (curvedness > 0f) spots[1].Add(new Spot { Lane = l, Distance = d, Position = position });
                    else if (curvedness <= -2f) outside.Add(new KeyValuePair<float, Spot>(-2f - curvedness, new Spot { Lane = l, Distance = d, Position = position }));
                }
            }
            var kappas = outside.Select(p => p.Key).OrderBy(k => k).ToList();
            straightThreshold = kappas.Count == 0 ? StraightCurvatureFloor : Math.Max(StraightCurvatureFloor, kappas[(kappas.Count - 1) / 3]);
            spots[0].AddRange(outside.Where(p => p.Key <= straightThreshold).Select(p => p.Value));

            // Scene deja ouverte dans l'Editeur : reutilisee telle quelle, jamais fermee par le banc.
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(MvpRunScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
            var measures = new List<Measure>();
            var skipped = new Dictionary<string, int>();
            try
            {
                Physics.SyncTransforms();
                var collector = new TrafficV2HazardCollector(TrafficV2Settings.HazardQueryCapacity, TrafficV2Settings.HazardQueryRadiusMeters);
                for (int s = 0; s < 2; s++)
                    foreach (int n in Populations)
                    {
                        int anchors = spots[s].Count == 0 ? 0 : n == 1 ? 24 : 8;
                        for (int a = 0; a < anchors; a++)
                        {
                            var anchor = spots[s][(int)((long)a * spots[s].Count / anchors)];
                            var vehicles = Cluster(lanes, spots[s], anchor, n, driver);
                            if (vehicles == null)
                            {
                                string key = Situations[s] + " N=" + n;
                                int count;
                                skipped.TryGetValue(key, out count);
                                skipped[key] = count + 1;
                                continue;
                            }
                            measures.Add(Run(s, vehicles, collector, driver));
                        }
                    }
            }
            finally
            {
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }

            string report = Report(lanes.Count, spots, measures, skipped);
            File.WriteAllText(ReportPath, report);
            Debug.Log("[Story533] diagnostic rond-point / ligne droite publie : " + ReportPath);
            foreach (int n in Populations)
                for (int s = 0; s < 2; s++)
                    Assert.That(measures.Any(m => m.Population == n && m.Situation == s), Is.True, Situations[s] + " N=" + n + " sans mesure");
        }

        /// <summary>
        /// N emplacements de la situation, l'ancre puis les plus proches d'elle (distance euclidienne), a 7 m au moins de tous
        /// les autres et dans un rayon de 80 m ; un vehicule de plus sur une meme route prend une identite propre.
        /// </summary>
        private static List<Vehicle> Cluster(List<Lane> lanes, List<Spot> spots, Spot anchor, int n, DriverProfile driver)
        {
            var chosen = new List<Spot> { anchor };
            foreach (var spot in spots.OrderBy(x => (x.Position - anchor.Position).sqrMagnitude).ThenBy(x => x.Lane).ThenBy(x => x.Distance))
            {
                if (chosen.Count == n) break;
                if ((spot.Position - anchor.Position).magnitude > ClusterRadiusMeters) break;
                if (chosen.Any(c => (c.Position - spot.Position).magnitude < MinimumSpacingMeters)) continue;
                chosen.Add(spot);
            }
            if (chosen.Count < n) return null;
            var vehicles = new List<Vehicle>();
            var used = new Dictionary<int, int>();
            foreach (var spot in chosen)
            {
                int index;
                used.TryGetValue(spot.Lane, out index);
                used[spot.Lane] = index + 1;
                var slot = SlotOf(lanes[spot.Lane], index, driver);
                if (slot == null || spot.Distance >= slot.Routes.Length || slot.Routes[spot.Distance] == null) return null;
                int piece = slot.Track.PieceAt(spot.Distance);
                var nominal = slot.Track.Nominal(piece, spot.Distance);
                vehicles.Add(new Vehicle { Slot = slot, Distance = spot.Distance, Piece = piece,
                    Speed = SpeedAt(slot.Track, piece, spot.Distance, driver),
                    Pose = new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward, Up = nominal.Up, Footprint = Car },
                    Route = slot.Routes[spot.Distance], Via = slot.Vias[spot.Distance], Offset = slot.Track.OffsetRadians(piece, spot.Distance) });
            }
            return vehicles;
        }

        // ================================================================== rapport

        private static string F(double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value) ? "-" : value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static double Mean(IEnumerable<Measure> measures, Func<Measure, double> f)
        {
            var list = measures.ToList();
            return list.Count == 0 ? double.NaN : list.Average(f);
        }

        private static int Field(string name)
        {
            for (int i = 0; i < WorkFields.Length; i++) if (WorkFields[i].Name == name) return i;
            throw new ArgumentException(name);
        }

        /// <summary>Compteur sur tout le pas (ou une section), par vehicule.</summary>
        private static double PerVehicle(Measure m, string field, int phase = -1)
        {
            int f = Field(field);
            long sum = 0;
            for (int p = 0; p < Phases.Length; p++) if (phase < 0 || p == phase) sum += m.Work[p][f];
            return (double)sum / m.Population;
        }

        private static string Report(int lanes, List<Spot>[] spots, List<Measure> measures, Dictionary<string, int> skipped)
        {
            var text = new StringBuilder();
            text.Append("# Diagnostic rond-point contre ligne droite (Story 5.33, apres D14)\n\n");
            text.Append("Banc `Story533RoundaboutCostDiagnosticTests` (EditMode, geometrie et decor reels de MVP_Run). ").Append(lanes)
                .Append(" routes de reference 5.31 ; emplacements au metre : ").Append(spots[0].Count).Append(" en ligne droite (tiers le moins courbe des emplacements hors carrefour a 3 m pres : |kappa| max sur +-6 m <= ")
                .Append(F(straightThreshold)).Append(" /m, R >= ").Append(F(1.0 / straightThreshold)).Append(" m), ")
                .Append(spots[1].Count).Append(" dans un rond-point (dans la frontiere d'un carrefour Roundabout). ")
                .Append("Chaque configuration place N vehicules dans la meme situation, au plus pres d'une ancre (7 m au moins entre centres), ")
                .Append("pose nominale cinematique, vitesse min(v desiree, sqrt(b_confort / |kappa|)), plan de route acquis comme en roulant. ")
                .Append("Pas hote rejoue dans l'ordre du runtime ; temps = meilleur de ").Append(TimedRuns).Append(" executions apres chauffe ; ")
                .Append("octets = GC.GetTotalMemory sans collection ; compteurs = TrafficV2WorkCounters par section. N = 1 : 24 ancres ; N > 1 : 8 ancres.\n\n");
            text.Append("Carrefours du modele : ").Append(string.Join(", ", Admission.Model.Junctions.GroupBy(j => j.Feature)
                .Select(g => g.Key + " " + g.Count() + " (demi-etendues moyennes " + F(g.Average(j => j.Boundary.Extents.x)) + " x "
                    + F(g.Average(j => j.Boundary.Extents.z)) + " m)").ToArray())).Append("\n\n");
            if (skipped.Count > 0)
                text.Append("Ancres sans grappe complete (ecartees) : ").Append(string.Join(", ", skipped.Select(p => p.Key + " : " + p.Value).ToArray())).Append("\n\n");

            text.Append("## 1. Cout par pas hote (ms, moyenne sur les ancres)\n\n| N | situation | config. | pas total | par vehicule | preparation | collecte | dont Overlap | frame | dont localisation (a part) | occupation (frame - localisation) | spine | route (a part) | horizon (a part) | dont transport de e | RoutePath (a part) | MotionPlan (a part) | perception | SpeedPlan | dont verification (a part) | arbitrage+commande | octets alloues |\n|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|\n");
            foreach (int n in Populations)
                for (int s = 0; s < 2; s++)
                {
                    var g = measures.Where(m => m.Population == n && m.Situation == s).ToList();
                    text.Append("| ").Append(n).Append(" | ").Append(Situations[s]).Append(" | ").Append(g.Count).Append(" | ")
                        .Append(F(Mean(g, m => m.Total))).Append(" | ").Append(F(Mean(g, m => m.Total / n))).Append(" | ")
                        .Append(F(Mean(g, m => m.Phase[0]))).Append(" | ").Append(F(Mean(g, m => m.Phase[1]))).Append(" | ")
                        .Append(F(Mean(g, m => m.Overlap))).Append(" | ").Append(F(Mean(g, m => m.Phase[2]))).Append(" | ")
                        .Append(F(Mean(g, m => m.Localize))).Append(" | ").Append(F(Mean(g, m => m.Phase[2] - m.Localize))).Append(" | ")
                        .Append(F(Mean(g, m => m.Phase[3]))).Append(" | ").Append(F(Mean(g, m => m.Route))).Append(" | ")
                        .Append(F(Mean(g, m => m.Horizon))).Append(" | ").Append(F(Mean(g, m => m.Horizon - m.HorizonWithoutE))).Append(" | ")
                        .Append(F(Mean(g, m => m.RoutePath))).Append(" | ").Append(F(Mean(g, m => m.Motion))).Append(" | ")
                        .Append(F(Mean(g, m => m.Phase[4]))).Append(" | ").Append(F(Mean(g, m => m.Phase[5]))).Append(" | ")
                        .Append(F(Mean(g, m => m.Verify))).Append(" | ").Append(F(Mean(g, m => m.Phase[6]))).Append(" | ")
                        .Append(F(Mean(g, m => m.Bytes))).Append(" |\n");
                }

            var counters = new[]
            {
                new KeyValuePair<string, string>("points construits (PathHorizon)", "HorizonPoints"),
                new KeyValuePair<string, string>("intervalles de l'horizon", "HorizonSpans"),
                new KeyValuePair<string, string>("etendues du chemin de perception (RoutePath)", "RoutePathSpans"),
                new KeyValuePair<string, string>("constructions d'horizon", "HorizonBuilds"),
                new KeyValuePair<string, string>("recherches de route", "RouteSearches"),
                new KeyValuePair<string, string>("RoadCurve.Project : appels", "ProjectCalls"),
                new KeyValuePair<string, string>("RoadCurve.Project : segments parcourus", "ProjectSegmentsIterated"),
                new KeyValuePair<string, string>("RoadCurve.Project : segments evalues", "ProjectSegmentsEvaluated"),
                new KeyValuePair<string, string>("ProjectNearest (occupation) : appels", "NearestCalls"),
                new KeyValuePair<string, string>("ProjectNearest : segments evalues", "NearestSegmentsEvaluated"),
                new KeyValuePair<string, string>("localisation : elements examines", "LocalizeScanned"),
                new KeyValuePair<string, string>("localisation : elements projetes (boite)", "LocalizeProjected"),
                new KeyValuePair<string, string>("localisation : candidats", "LocalizeCandidates"),
                new KeyValuePair<string, string>("transport de e : appels", "KinematicCalls"),
                new KeyValuePair<string, string>("transport de e : pas RK4 (x4 sinus)", "KinematicSteps"),
                new KeyValuePair<string, string>("perception : points lus pour la boite", "ObstacleBoxPoints"),
                new KeyValuePair<string, string>("perception : entrees spatiales examinees", "ObstacleEntries"),
                new KeyValuePair<string, string>("perception : projections d'obstacle", "ObstacleProjections"),
                new KeyValuePair<string, string>("perception : paires d'intention", "IntentPairs"),
                new KeyValuePair<string, string>("perception : ConflictZone examinees", "ConflictZoneTests"),
                new KeyValuePair<string, string>("perception : plages de zone", "ZoneSpans"),
                new KeyValuePair<string, string>("verification : noeuds", "VerifyKnots")
            };
            text.Append("\n## 2. Travail par vehicule et par pas (compteurs du runtime, moyenne sur les ancres)\n\n| Grandeur |");
            foreach (int n in Populations) text.Append(" LD N=").Append(n).Append(" | RP N=").Append(n).Append(" | RP/LD |");
            text.Append('\n').Append("|---|").Append(string.Concat(Enumerable.Repeat("---|---|---|", Populations.Length))).Append('\n');
            Action<string, Func<Measure, double>> row = (label, f) =>
            {
                text.Append("| ").Append(label).Append(" |");
                foreach (int n in Populations)
                {
                    double ld = Mean(measures.Where(m => m.Population == n && m.Situation == 0), f);
                    double rp = Mean(measures.Where(m => m.Population == n && m.Situation == 1), f);
                    text.Append(' ').Append(F(ld)).Append(" | ").Append(F(rp)).Append(" | ").Append(F(rp / ld)).Append(" |");
                }
                text.Append('\n');
            };
            foreach (var c in counters) row(c.Key, m => PerVehicle(m, c.Value));
            row("colliders rendus par requete", m => (double)m.Collector.Hits / m.Population);
            row("dont decor statique ecarte", m => (double)m.Collector.ExcludedStatic / m.Population);
            row("dangers emis (par pas)", m => m.Collector.Emitted);
            row("echantillons par metre de l'element occupe", m => m.SamplesPerMeter);

            text.Append("\n## 3. Attribution par section (par vehicule et par pas)\n\n");
            foreach (var field in new[] { "ProjectCalls", "ProjectSegmentsEvaluated", "KinematicSteps", "NearestSegmentsEvaluated" })
            {
                text.Append("### ").Append(field).Append("\n\n| N | situation |");
                foreach (var phase in Phases) text.Append(' ').Append(phase).Append(" |");
                text.Append('\n').Append("|---|---|").Append(string.Concat(Enumerable.Repeat("---|", Phases.Length))).Append('\n');
                foreach (int n in Populations)
                    for (int s = 0; s < 2; s++)
                    {
                        var g = measures.Where(m => m.Population == n && m.Situation == s).ToList();
                        text.Append("| ").Append(n).Append(" | ").Append(Situations[s]).Append(" |");
                        for (int p = 0; p < Phases.Length; p++) text.Append(' ').Append(F(Mean(g, m => PerVehicle(m, field, p)))).Append(" |");
                        text.Append('\n');
                    }
                text.Append('\n');
            }

            text.Append("## 4. Croissance N = 1 -> 8 (cout par vehicule, ms ; un cout O(N) reste plat, un cout O(N^2) croit avec N)\n\n| Section | situation |");
            foreach (int n in Populations) text.Append(" N=").Append(n).Append(" |");
            text.Append(" N=8 / N=1 |\n|---|---|").Append(string.Concat(Enumerable.Repeat("---|", Populations.Length + 1))).Append('\n');
            var sections = new List<KeyValuePair<string, Func<Measure, double>>>
            {
                new KeyValuePair<string, Func<Measure, double>>("pas total", m => m.Total),
                new KeyValuePair<string, Func<Measure, double>>("collecte", m => m.Phase[1]),
                new KeyValuePair<string, Func<Measure, double>>("frame", m => m.Phase[2]),
                new KeyValuePair<string, Func<Measure, double>>("spine", m => m.Phase[3]),
                new KeyValuePair<string, Func<Measure, double>>("perception", m => m.Phase[4]),
                new KeyValuePair<string, Func<Measure, double>>("SpeedPlan", m => m.Phase[5]),
                new KeyValuePair<string, Func<Measure, double>>("arbitrage+commande", m => m.Phase[6])
            };
            foreach (var section in sections)
                for (int s = 0; s < 2; s++)
                {
                    text.Append("| ").Append(section.Key).Append(" | ").Append(Situations[s]).Append(" |");
                    var values = Populations.Select(n => Mean(measures.Where(m => m.Population == n && m.Situation == s), m => section.Value(m) / n)).ToList();
                    foreach (var value in values) text.Append(' ').Append(F(value)).Append(" |");
                    text.Append(' ').Append(F(values[values.Count - 1] / values[0])).Append(" |\n");
                }
            return text.ToString();
        }
    }
}
