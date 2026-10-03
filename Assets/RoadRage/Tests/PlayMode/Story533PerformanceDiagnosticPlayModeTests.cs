using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.33, diagnostic de performance (demande proprietaire du 2026-10-02, avant toute optimisation). Le scenario
    /// nominal explore-4 (meme route A, obstacle de file) est rejoue avec 1, 2, 3 puis 4 vehicules, dans les conditions des
    /// runs d'acceptation : pilotage au pas physique, observateur du harnais a chaque pas, trace au pas des vehicules.
    /// Mesures : par pas hote, les sections de l'ordonnanceur (TrafficV2StepCost) et des drivers (V2StageTimings) ; par
    /// frame rendue, des ProfilerRecorder (frame, FixedUpdate executes, physique, GC, sous-sections de frame et de spine).
    /// Publie un resume par population, la timeline des pics et une comparaison 1 -> 4. Ne juge rien : seul un run sans
    /// vehicule conduit echoue.
    /// </summary>
    [Explicit]
    [Category("Story533Perf")]
    public sealed class Story533PerformanceDiagnosticPlayModeTests
    {
        private const string ScenarioLabel = "explore-4";
        private static readonly ProfilerMarker ObserveMarker = new ProfilerMarker("Story533.Harness.Observe");

        /// <summary>Stats suivies par frame rendue : marqueurs du moteur puis marqueurs Traffic V2 et du harnais.</summary>
        private static readonly string[] Stats =
        {
            "Main Thread", "PlayerLoop", "FixedBehaviourUpdate", "FixedUpdate.PhysicsFixedUpdate", "Physics.Simulate",
            "GC.Collect", "GC.Alloc", "GC Allocated In Frame",
            "TrafficV2.Step", "TrafficV2.Prepare", "TrafficV2.Collect", "TrafficV2.Collect.Overlap", "TrafficV2.FrameBuild",
            "TrafficV2.Frame.Localize", "TrafficV2.Frame.Occupancy", "TrafficV2.Frame.Hazards", "TrafficV2.Drive",
            "TrafficV2.Driver.Prepare", "TrafficV2.Driver.Localize", "TrafficV2.Driver.Spine", "TrafficV2.Spine.Route",
            "TrafficV2.Spine.Horizon", "TrafficV2.Spine.Motion", "TrafficV2.Spine.Projection", "TrafficV2.Driver.Perception",
            "TrafficV2.Driver.SpeedPlan", "TrafficV2.Driver.Arbitration", "TrafficV2.Driver.Compose",
            "TrafficV2.Driver.MotionCommand", "TrafficV2.Driver.Instrumentation", "Story533.Harness.Observe"
        };

        private static readonly Dictionary<int, PopulationResult> Results = new Dictionary<int, PopulationResult>();
        private static string campaignStamp;

        private Story533Harness harness;
        private GameObject probeObject;

        [SetUp]
        public void SetUp()
        {
            TrafficV2Session.Reset();
            harness = new Story533Harness();
            if (campaignStamp == null) campaignStamp = Story533Harness.Stamp();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (probeObject != null) Object.Destroy(probeObject);
            yield return harness.Cleanup();
        }

        [UnityTest, Order(1), Timeout(1800000)] public IEnumerator DiagnoseOneVehicle() { yield return Diagnose(ScenarioLabel, 1); }
        [UnityTest, Order(2), Timeout(1800000)] public IEnumerator DiagnoseTwoVehicles() { yield return Diagnose(ScenarioLabel, 2); }
        [UnityTest, Order(3), Timeout(1800000)] public IEnumerator DiagnoseThreeVehicles() { yield return Diagnose(ScenarioLabel, 3); }
        [UnityTest, Order(4), Timeout(1800000)] public IEnumerator DiagnoseFourVehicles() { yield return Diagnose(ScenarioLabel, 4); }

        /// <summary>
        /// N = 8 (cible D13 : moins de 10 ms par pas hote) sur explore-8, quatre entrees et deux vagues. Un verrou pre-5.34
        /// (D12, contact en zone de conflit) arrete la mesure : le cout est celui de la fenetre propre.
        /// </summary>
        [UnityTest, Order(5), Timeout(1800000)]
        public IEnumerator DiagnoseEightVehiclesAndCompare()
        {
            yield return Diagnose("explore-8", 8);
            WriteComparison();
        }

        // ------------------------------------------------------------------ mesures

        private sealed class StepSample
        {
            public ulong HostStep;
            public int Vehicles;
            public TrafficV2StepCost Cost;
            public double DriverLocalize, Spine, Perception, SpeedPlan, Arbitration, Compose, Track, Instrumentation, DriverPrepare;
            public long DriverBytes, InstrumentationBytes;
            public double ObserverMilliseconds;
            public int Collections;
        }

        private sealed class FrameSample
        {
            public int Index;
            public double WallMilliseconds;
            public int FixedSteps;
            public ulong FirstHostStep, LastHostStep;
            public double[] Values;
            public int[] Counts;
        }

        private sealed class PopulationResult
        {
            public int Population;
            public int HostSteps;
            public double WallSeconds;
            public readonly Dictionary<string, double> PerStep = new Dictionary<string, double>();
            public double FramesPerSecond, FixedPerFrame;
            public int Spikes20, Spikes50, Spikes100;
            public long BytesPerStep;
            public int Collections;
            public double CollectionsPerMinute;
        }

        private struct Snapshot
        {
            public double Frame, Perception, Spine, Plan, Compose, Prepare, Arbitration, Track, Instrumentation;
            public long Bytes, InstrumentationBytes;

            public static Snapshot Of(V2StageTimings t)
            {
                return new Snapshot { Frame = t.FrameMilliseconds, Perception = t.PerceptionMilliseconds, Spine = t.SpineMilliseconds,
                    Plan = t.SpeedPlanMilliseconds, Compose = t.ComposeMilliseconds, Prepare = t.PrepareMilliseconds,
                    Arbitration = t.ArbitrationMilliseconds, Track = t.TrackMilliseconds, Instrumentation = t.InstrumentationMilliseconds,
                    Bytes = t.PrepareBytes + t.FrameBytes + t.SpineBytes + t.PerceptionBytes + t.SpeedPlanBytes + t.ComposeBytes
                        + t.InstrumentationBytes,
                    InstrumentationBytes = t.InstrumentationBytes };
            }
        }

        /// <summary>Sonde de frame rendue : FixedUpdate executes et valeurs des recorders de la frame precedente, alignees.</summary>
        private sealed class FrameProbe : MonoBehaviour
        {
            public PortalTrafficSpawner Spawner;
            public ProfilerRecorder[] Recorders;
            public bool Recording;
            public readonly List<FrameSample> Frames = new List<FrameSample>();
            private int fixedInFrame, fixedPrevious;
            private ulong firstStepPrevious, lastStepPrevious, firstStep;
            private double lastUpdate;
            private bool havePrevious;

            private void FixedUpdate()
            {
                if (fixedInFrame == 0 && Spawner != null) firstStep = Spawner.V2Runner.FrameId + 1UL;
                fixedInFrame++;
            }

            private void Update()
            {
                double now = Time.realtimeSinceStartupAsDouble;
                ulong host = Spawner != null ? Spawner.V2Runner.FrameId : 0UL;
                // Les recorders rendent la frame precedente : on la publie avec ses propres FixedUpdate.
                if (Recording && havePrevious && Recorders != null)
                {
                    var sample = new FrameSample { Index = Frames.Count, WallMilliseconds = (now - lastUpdate) * 1000.0,
                        FixedSteps = fixedPrevious, FirstHostStep = firstStepPrevious, LastHostStep = lastStepPrevious,
                        Values = new double[Recorders.Length], Counts = new int[Recorders.Length] };
                    for (int i = 0; i < Recorders.Length; i++)
                    {
                        if (!Recorders[i].Valid || Recorders[i].Count == 0) { sample.Values[i] = double.NaN; continue; }
                        var last = Recorders[i].GetSample(0);
                        sample.Values[i] = last.Value;
                        sample.Counts[i] = (int)last.Count;
                    }
                    Frames.Add(sample);
                }
                fixedPrevious = fixedInFrame;
                firstStepPrevious = fixedInFrame > 0 ? firstStep : 0UL;
                lastStepPrevious = fixedInFrame > 0 ? host : 0UL;
                fixedInFrame = 0;
                lastUpdate = now;
                havePrevious = true;
            }
        }

        private static ProfilerRecorder[] StartRecorders(out string missing)
        {
            var handles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(handles);
            var byName = new Dictionary<string, ProfilerRecorderDescription>();
            foreach (var handle in handles)
            {
                var description = ProfilerRecorderHandle.GetDescription(handle);
                if (!byName.ContainsKey(description.Name)) byName.Add(description.Name, description);
            }
            var recorders = new ProfilerRecorder[Stats.Length];
            var absent = new List<string>();
            for (int i = 0; i < Stats.Length; i++)
            {
                ProfilerRecorderDescription description;
                if (!byName.TryGetValue(Stats[i], out description)) { absent.Add(Stats[i]); continue; }
                recorders[i] = ProfilerRecorder.StartNew(description.Category, description.Name, 1);
            }
            missing = absent.Count == 0 ? "aucune" : string.Join(", ", absent.ToArray());
            return recorders;
        }

        private static int StatIndex(string name) { return Array.IndexOf(Stats, name); }

        private IEnumerator Diagnose(string scenarioLabel, int population)
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile file;
            var source = Story533Harness.Load(admission, scenarioLabel, out file);
            // Meme scenario nominal, tronque aux N premieres insertions : meme route, meme obstacle de file, aucune poussee.
            var record = new Story533Harness.ScenarioRecord { Label = scenarioLabel + "-N" + population, MaxPopulation = population,
                MaxSteps = source.MaxSteps, Insertions = source.Insertions.Take(population).ToArray(), Obstacles = source.Obstacles,
                Pushes = new Story533Harness.PushRecord[0] };
            TrafficV2Session.Request(TrafficComposition.V2Slice, null, Story533Harness.ToScenario(record));
            yield return harness.EnterMvpRun();
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            int parked = 0;
            yield return harness.ParkHostPlayer(file.Parking.Value, n => parked = n);

            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation, admission.Model);
            probeObject = new GameObject("Story533_FrameProbe");
            var probe = probeObject.AddComponent<FrameProbe>();
            probe.Spawner = spawner;
            var obstacles = new GameObject[record.Obstacles.Length];
            var removed = new bool[record.Obstacles.Length];
            var steps = new List<StepSample>();
            var snapshots = new Dictionary<RoadId, Snapshot>();
            ProfilerRecorder[] recorders = null;
            string missing = "non demarre";
            var watch = new Stopwatch();
            int warmup = -1;
            double wallStart = 0d;
            int collectionsStart = 0;

            for (int step = 0; step < record.MaxSteps; step++)
            {
                ulong frame = spawner.V2Runner.FrameId;
                for (int o = 0; o < obstacles.Length; o++)
                {
                    var obstacle = record.Obstacles[o];
                    if (obstacles[o] == null && !removed[o] && frame >= (ulong)obstacle.FromStep) obstacles[o] = harness.CreateObstacle(obstacle);
                    if (obstacles[o] != null && obstacle.UntilStep >= 0 && frame >= (ulong)obstacle.UntilStep)
                    { harness.Destroy(obstacles[o]); obstacles[o] = null; removed[o] = true; }
                }
                int collections = GC.CollectionCount(0);
                yield return new WaitForFixedUpdate();
                var cost = spawner.V2Runner.LastCost;

                // Demarrage des recorders une fois tous les marqueurs crees (premier pas conduit), hors fenetre mesuree.
                if (recorders == null && cost.Vehicles > 0)
                {
                    if (warmup < 0) warmup = step + 5;
                    if (step >= warmup)
                    {
                        recorders = StartRecorders(out missing);
                        probe.Recorders = recorders;
                        probe.Recording = true;
                        wallStart = Time.realtimeSinceStartupAsDouble;
                        collectionsStart = GC.CollectionCount(0);
                    }
                }

                watch.Restart();
                ObserveMarker.Begin();
                observer.Observe();
                ObserveMarker.End();
                double observed = watch.Elapsed.TotalMilliseconds;

                if (recorders != null)
                {
                    var sample = new StepSample { HostStep = spawner.V2Runner.FrameId, Vehicles = cost.Vehicles, Cost = cost,
                        ObserverMilliseconds = observed, Collections = GC.CollectionCount(0) - collections };
                    foreach (var networkObject in spawner.LiveV2Vehicles)
                    {
                        var driver = networkObject.GetComponent<TrafficV2VehicleDriver>();
                        if (driver == null) continue;
                        var now = Snapshot.Of(driver.Timings);
                        Snapshot before;
                        if (snapshots.TryGetValue(driver.TrafficId, out before))
                        {
                            sample.DriverPrepare += now.Prepare - before.Prepare;
                            sample.DriverLocalize += now.Frame - before.Frame;
                            sample.Spine += now.Spine - before.Spine;
                            sample.Perception += now.Perception - before.Perception;
                            sample.Arbitration += now.Arbitration - before.Arbitration;
                            sample.SpeedPlan += (now.Plan - before.Plan) - (now.Arbitration - before.Arbitration);
                            sample.Track += now.Track - before.Track;
                            sample.Compose += (now.Compose - before.Compose) - (now.Track - before.Track);
                            sample.Instrumentation += now.Instrumentation - before.Instrumentation;
                            sample.DriverBytes += now.Bytes - before.Bytes;
                            sample.InstrumentationBytes += now.InstrumentationBytes - before.InstrumentationBytes;
                        }
                        snapshots[driver.TrafficId] = now;
                    }
                    // Le temps « frame » du driver contient sa part de frame partagee : on n'en garde que la localisation propre.
                    sample.DriverLocalize -= cost.CollectorMilliseconds + cost.FrameBuildMilliseconds;
                    steps.Add(sample);
                }
                if (observer.PlayerFacts > 0 || observer.ToleranceLatch != null) break;
                if (spawner.ScenarioInsertions.Count == record.Insertions.Length && spawner.LiveV2Population == 0) break;
            }
            double wallSeconds = Time.realtimeSinceStartupAsDouble - wallStart;
            int totalCollections = GC.CollectionCount(0) - collectionsStart;
            probe.Recording = false;

            // Ecritures disque de fin de run, mesurees a part (aucune pendant le run).
            watch.Restart();
            var runs = Story533Harness.Runs(spawner);
            string traceStamp = Story533Harness.Stamp();
            Story533Harness.WriteTrace("perf-N" + population, traceStamp, runs);
            double traceMilliseconds = watch.Elapsed.TotalMilliseconds;

            var result = Publish(population, record, steps, probe.Frames, wallSeconds, totalCollections, missing, traceMilliseconds,
                observer, traceStamp);
            Results[population] = result;
            if (recorders != null) foreach (var recorder in recorders) recorder.Dispose();
            Assert.That(steps.Count(s => s.Vehicles > 0), Is.GreaterThan(0), "aucun vehicule conduit : diagnostic vide");
        }

        // ------------------------------------------------------------------ publication

        private static string Ms(double value) { return double.IsNaN(value) ? "-" : value.ToString("0.###", CultureInfo.InvariantCulture); }

        private static double Percentile(List<double> values, double p)
        {
            if (values.Count == 0) return double.NaN;
            var sorted = values.OrderBy(v => v).ToList();
            return sorted[Math.Min(sorted.Count - 1, (int)Math.Floor(p * (sorted.Count - 1) + 0.5))];
        }

        private static string Stat(string label, List<double> values)
        {
            return "| " + label + " | " + Ms(values.Count == 0 ? double.NaN : values.Average()) + " | " + Ms(Percentile(values, 0.5))
                + " | " + Ms(Percentile(values, 0.95)) + " | " + Ms(values.Count == 0 ? double.NaN : values.Max()) + " |\n";
        }

        private static double FrameValue(FrameSample frame, string stat)
        {
            int i = StatIndex(stat);
            return i < 0 || double.IsNaN(frame.Values[i]) ? double.NaN : frame.Values[i] / 1e6;
        }

        /// <summary>Section responsable d'une frame : la plus couteuse parmi Traffic V2, physique, autres FixedUpdate, GC, harnais, hors PlayerLoop.</summary>
        private static string Responsible(FrameSample frame)
        {
            double traffic = FrameValue(frame, "TrafficV2.Step");
            double physics = FrameValue(frame, "Physics.Simulate");
            double fixedScripts = FrameValue(frame, "FixedBehaviourUpdate");
            double gc = FrameValue(frame, "GC.Collect");
            double observe = FrameValue(frame, "Story533.Harness.Observe");
            double loop = FrameValue(frame, "PlayerLoop");
            var parts = new List<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>("TrafficV2.Step", traffic),
                new KeyValuePair<string, double>("Physics.Simulate", physics),
                new KeyValuePair<string, double>("autres FixedUpdate (dont VehiclePhysicsBody)", fixedScripts - (double.IsNaN(traffic) ? 0 : traffic)),
                new KeyValuePair<string, double>("GC.Collect", gc),
                new KeyValuePair<string, double>("harnais (Observe)", observe),
                new KeyValuePair<string, double>("hors PlayerLoop (Editeur, rendu)", frame.WallMilliseconds - loop)
            };
            var best = parts.Where(p => !double.IsNaN(p.Value)).OrderByDescending(p => p.Value).FirstOrDefault();
            string text = best.Key == null ? "-" : best.Key + " " + Ms(best.Value) + " ms";
            if (best.Key == "TrafficV2.Step")
            {
                var subs = new[] { "TrafficV2.Prepare", "TrafficV2.Collect", "TrafficV2.Frame.Localize", "TrafficV2.Frame.Occupancy",
                    "TrafficV2.Frame.Hazards", "TrafficV2.Spine.Route", "TrafficV2.Spine.Horizon", "TrafficV2.Spine.Motion",
                    "TrafficV2.Spine.Projection", "TrafficV2.Driver.Perception", "TrafficV2.Driver.SpeedPlan",
                    "TrafficV2.Driver.Compose", "TrafficV2.Driver.Instrumentation" };
                var sub = subs.Select(s => new KeyValuePair<string, double>(s, FrameValue(frame, s))).Where(p => !double.IsNaN(p.Value))
                    .OrderByDescending(p => p.Value).FirstOrDefault();
                if (sub.Key != null) text += " (dont " + sub.Key + " " + Ms(sub.Value) + " ms)";
            }
            return text;
        }

        private static PopulationResult Publish(int population, Story533Harness.ScenarioRecord record, List<StepSample> steps,
            List<FrameSample> frames, double wallSeconds, int collections, string missing, double traceMilliseconds,
            Story533Harness.Observer observer, string traceStamp)
        {
            var driven = steps.Where(s => s.Vehicles > 0).ToList();
            var result = new PopulationResult { Population = population, HostSteps = steps.Count, WallSeconds = wallSeconds,
                Collections = collections, CollectionsPerMinute = wallSeconds > 0 ? collections * 60.0 / wallSeconds : 0 };
            Func<Func<StepSample, double>, List<double>> per = f => driven.Select(f).ToList();
            var sections = new List<KeyValuePair<string, List<double>>>
            {
                new KeyValuePair<string, List<double>>("pas Traffic V2 total", per(s => s.Cost.TotalMilliseconds)),
                new KeyValuePair<string, List<double>>("  preparation (etat, ancres, tolerance)", per(s => s.Cost.PrepareMilliseconds)),
                new KeyValuePair<string, List<double>>("  collecte des dangers", per(s => s.Cost.CollectorMilliseconds)),
                new KeyValuePair<string, List<double>>("    dont OverlapSphereNonAlloc", per(s => s.Cost.OverlapMilliseconds)),
                new KeyValuePair<string, List<double>>("  construction de la TrafficFrame", per(s => s.Cost.FrameBuildMilliseconds)),
                new KeyValuePair<string, List<double>>("  pas des drivers", per(s => s.Cost.DriveMilliseconds)),
                new KeyValuePair<string, List<double>>("    localisation propre (lecture de frame, sortie)", per(s => s.DriverLocalize)),
                new KeyValuePair<string, List<double>>("    spine (route + horizon + mouvement + projection)", per(s => s.Spine)),
                new KeyValuePair<string, List<double>>("    perception (tous vehicules)", per(s => s.Perception)),
                new KeyValuePair<string, List<double>>("    SpeedPlan", per(s => s.SpeedPlan)),
                new KeyValuePair<string, List<double>>("    arbitrage", per(s => s.Arbitration)),
                new KeyValuePair<string, List<double>>("    MotionCommand", per(s => s.Track)),
                new KeyValuePair<string, List<double>>("    composition + application", per(s => s.Compose)),
                new KeyValuePair<string, List<double>>("    instrumentation (blockers, trace, projection)", per(s => s.Instrumentation)),
                new KeyValuePair<string, List<double>>("harnais : observateur par pas", per(s => s.ObserverMilliseconds))
            };
            foreach (var section in sections) result.PerStep[section.Key.Trim()] = section.Value.Count == 0 ? double.NaN : section.Value.Average();
            result.PerStep["perception par vehicule"] = driven.Count == 0 ? double.NaN : driven.Average(s => s.Perception / s.Vehicles);
            result.PerStep["pas Traffic V2 par vehicule"] = driven.Count == 0 ? double.NaN : driven.Average(s => s.Cost.TotalMilliseconds / s.Vehicles);
            result.PerStep["OverlapSphereNonAlloc par pas"] = driven.Count == 0 ? double.NaN : driven.Average(s => (double)s.Cost.Overlaps);
            result.BytesPerStep = driven.Count == 0 ? 0 : (long)driven.Average(s => (double)s.Cost.TotalBytes);

            // Par frame rendue (recorders) : moyennes par pas hote des sous-sections hors ordonnanceur.
            int hostSteps = Math.Max(1, frames.Sum(f => f.FixedSteps));
            foreach (var stat in new[] { "TrafficV2.Frame.Localize", "TrafficV2.Frame.Occupancy", "TrafficV2.Frame.Hazards",
                "TrafficV2.Spine.Route", "TrafficV2.Spine.Horizon", "TrafficV2.Spine.Motion", "TrafficV2.Spine.Projection",
                "Physics.Simulate", "FixedBehaviourUpdate", "FixedUpdate.PhysicsFixedUpdate", "GC.Collect", "TrafficV2.Step" })
            {
                var values = frames.Select(f => FrameValue(f, stat)).Where(v => !double.IsNaN(v)).ToList();
                result.PerStep["[recorder] " + stat] = values.Count == 0 ? double.NaN : values.Sum() / hostSteps;
            }
            result.FramesPerSecond = wallSeconds > 0 ? frames.Count / wallSeconds : 0;
            result.FixedPerFrame = frames.Count == 0 ? 0 : frames.Average(f => (double)f.FixedSteps);
            result.Spikes20 = frames.Count(f => f.WallMilliseconds > 20);
            result.Spikes50 = frames.Count(f => f.WallMilliseconds > 50);
            result.Spikes100 = frames.Count(f => f.WallMilliseconds > 100);

            var text = new StringBuilder();
            text.Append("# Diagnostic de performance Traffic V2, N = ").Append(population).Append(" (").Append(campaignStamp).Append(")\n\n");
            text.Append("- Scenario : ").Append(record.Label).Append(" (scenario nominal, ").Append(record.Insertions.Length)
                .Append(" premieres insertions, obstacle du scenario jusqu'au pas ")
                .Append(record.Obstacles.Length > 0 ? record.Obstacles[0].UntilStep.ToString(CultureInfo.InvariantCulture) : "-")
                .Append(", aucune poussee), pilotage WaitForFixedUpdate, observateur du harnais a chaque pas, trace au pas active\n");
            text.Append("- Fenetre mesuree : ").Append(steps.Count).Append(" pas hote (").Append(driven.Count).Append(" avec vehicule), ")
                .Append(Ms(wallSeconds)).Append(" s reelles, ").Append(frames.Count).Append(" frames rendues (")
                .Append(Ms(result.FramesPerSecond)).Append(" fps), pas simule/reel ")
                .Append(Ms(steps.Count * Time.fixedDeltaTime / Math.Max(1e-6, wallSeconds))).Append('\n');
            text.Append("- Reglages : fixedDeltaTime ").Append(Ms(Time.fixedDeltaTime)).Append(" s, maximumDeltaTime ").Append(Ms(Time.maximumDeltaTime))
                .Append(" s, targetFrameRate ").Append(Application.targetFrameRate).Append(", vSyncCount ").Append(QualitySettings.vSyncCount)
                .Append(", runInBackground ").Append(Application.runInBackground).Append('\n');
            text.Append("- Stats indisponibles : ").Append(missing).Append('\n');
            text.Append("- Fin de run : ").Append(observer.ToleranceLatch != null ? "verrou " + observer.ToleranceLatch : "normale")
                .Append(", invariants ").Append(observer.Violations.Count == 0 ? "verts" : string.Join(" | ", observer.Violations)).Append('\n');
            text.Append("- Ecriture de la trace TSV en fin de run : ").Append(Ms(traceMilliseconds)).Append(" ms (aucune ecriture disque pendant le run)\n\n");

            text.Append("## Cout par pas hote (ms, pas avec au moins un vehicule)\n\n| Section | moyenne | mediane | p95 | max |\n|---|---|---|---|---|\n");
            foreach (var section in sections) text.Append(Stat(section.Key.Replace("  ", "&nbsp;&nbsp;"), section.Value));
            text.Append(Stat("perception par vehicule", per(s => s.Perception / s.Vehicles)));
            text.Append(Stat("pas Traffic V2 par vehicule", per(s => s.Cost.TotalMilliseconds / s.Vehicles)));
            text.Append("\nOverlapSphereNonAlloc : ").Append(Ms(result.PerStep["OverlapSphereNonAlloc par pas"])).Append(" requetes par pas, ")
                .Append(Ms(driven.Count == 0 ? double.NaN : driven.Average(s => s.Cost.Overlaps == 0 ? 0 : s.Cost.OverlapMilliseconds / s.Cost.Overlaps)))
                .Append(" ms par requete.\n\n");

            text.Append("## Sous-sections par pas hote (recorders, ms)\n\n| Stat | ms par pas hote |\n|---|---|\n");
            foreach (var pair in result.PerStep.Where(p => p.Key.StartsWith("[recorder]")))
                text.Append("| ").Append(pair.Key.Substring(11)).Append(" | ").Append(Ms(pair.Value)).Append(" |\n");

            text.Append("\n## Allocations GC\n\n");
            text.Append("- Par pas hote : total ").Append(result.BytesPerStep).Append(" octets ; preparation ")
                .Append(driven.Count == 0 ? 0 : (long)driven.Average(s => (double)s.Cost.PrepareBytes)).Append(", collecteur ")
                .Append(driven.Count == 0 ? 0 : (long)driven.Average(s => (double)s.Cost.CollectorBytes)).Append(", frame ")
                .Append(driven.Count == 0 ? 0 : (long)driven.Average(s => (double)s.Cost.FrameBuildBytes)).Append(", drivers ")
                .Append(driven.Count == 0 ? 0 : (long)driven.Average(s => (double)s.Cost.DriveBytes)).Append(" (dont instrumentation ")
                .Append(driven.Count == 0 ? 0 : (long)driven.Average(s => (double)s.InstrumentationBytes)).Append(")\n");
            var allocated = frames.Select(f => FrameValue(f, "GC Allocated In Frame") * 1e6).Where(v => !double.IsNaN(v)).ToList();
            text.Append("- Par frame rendue (compteur moteur) : moyenne ").Append(allocated.Count == 0 ? "-" : ((long)allocated.Average()).ToString(CultureInfo.InvariantCulture))
                .Append(" octets, max ").Append(allocated.Count == 0 ? "-" : ((long)allocated.Max()).ToString(CultureInfo.InvariantCulture)).Append('\n');
            text.Append("- Collections gen0 : ").Append(collections).Append(" en ").Append(Ms(wallSeconds)).Append(" s (")
                .Append(Ms(result.CollectionsPerMinute)).Append(" par minute) ; pas avec collection : ").Append(steps.Count(s => s.Collections > 0)).Append('\n');
            var gcFrames = frames.Where(f => StatIndex("GC.Collect") >= 0 && f.Counts[StatIndex("GC.Collect")] > 0).ToList();
            text.Append("- Frames avec GC.Collect : ").Append(gcFrames.Count).Append(", pause moyenne ")
                .Append(Ms(gcFrames.Count == 0 ? double.NaN : gcFrames.Average(f => FrameValue(f, "GC.Collect")))).Append(" ms, max ")
                .Append(Ms(gcFrames.Count == 0 ? double.NaN : gcFrames.Max(f => FrameValue(f, "GC.Collect")))).Append(" ms\n\n");

            text.Append("## FixedUpdate par frame rendue\n\n");
            foreach (var group in frames.GroupBy(f => Math.Min(f.FixedSteps, 10)).OrderBy(g => g.Key))
                text.Append("- ").Append(group.Key == 10 ? "10+" : group.Key.ToString(CultureInfo.InvariantCulture)).Append(" : ")
                    .Append(group.Count()).Append(" frames\n");
            text.Append("- Moyenne : ").Append(Ms(result.FixedPerFrame)).Append(" FixedUpdate par frame ; frames avec plus d'un pas : ")
                .Append(frames.Count(f => f.FixedSteps > 1)).Append('\n');

            text.Append("\n## Pics (frames rendues)\n\n- > 20 ms : ").Append(result.Spikes20).Append(" ; > 50 ms : ").Append(result.Spikes50)
                .Append(" ; > 100 ms : ").Append(result.Spikes100).Append('\n');
            var big = frames.Where(f => f.WallMilliseconds > 50).ToList();
            if (big.Count > 1)
            {
                var gaps = new List<double>();
                for (int i = 1; i < big.Count; i++)
                    gaps.Add(frames.Skip(big[i - 1].Index).Take(big[i].Index - big[i - 1].Index).Sum(f => f.WallMilliseconds) / 1000.0);
                text.Append("- Intervalle entre pics > 50 ms : mediane ").Append(Ms(Percentile(gaps, 0.5))).Append(" s, p10 ")
                    .Append(Ms(Percentile(gaps, 0.1))).Append(" s, p90 ").Append(Ms(Percentile(gaps, 0.9))).Append(" s ; pics > 50 ms avec GC.Collect : ")
                    .Append(big.Count(f => StatIndex("GC.Collect") >= 0 && f.Counts[StatIndex("GC.Collect")] > 0)).Append('/').Append(big.Count).Append('\n');
            }
            text.Append("\n### Timeline des pics > 20 ms\n\n| frame | pas hote | ms | FixedUpdate | Traffic V2 | Physics.Simulate | GC.Collect | hors PlayerLoop | responsable |\n|---|---|---|---|---|---|---|---|---|\n");
            foreach (var f in frames.Where(f => f.WallMilliseconds > 20).Take(400))
                text.Append("| ").Append(f.Index).Append(" | ").Append(f.FirstHostStep).Append('-').Append(f.LastHostStep).Append(" | ")
                    .Append(Ms(f.WallMilliseconds)).Append(" | ").Append(f.FixedSteps).Append(" | ").Append(Ms(FrameValue(f, "TrafficV2.Step")))
                    .Append(" | ").Append(Ms(FrameValue(f, "Physics.Simulate"))).Append(" | ").Append(Ms(FrameValue(f, "GC.Collect")))
                    .Append(" | ").Append(Ms(f.WallMilliseconds - FrameValue(f, "PlayerLoop"))).Append(" | ").Append(Responsible(f)).Append(" |\n");

            var stepSpikes = driven.Where(s => s.Cost.TotalMilliseconds > 20).ToList();
            text.Append("\n### Pas hote individuels > 20 ms (ordonnanceur)\n\n").Append(stepSpikes.Count).Append(" pas.\n\n");
            if (stepSpikes.Count > 0)
            {
                text.Append("| pas hote | total | preparation | collecte | frame | drivers | instrumentation | collection GC |\n|---|---|---|---|---|---|---|---|\n");
                foreach (var s in stepSpikes.Take(200))
                    text.Append("| ").Append(s.HostStep).Append(" | ").Append(Ms(s.Cost.TotalMilliseconds)).Append(" | ").Append(Ms(s.Cost.PrepareMilliseconds))
                        .Append(" | ").Append(Ms(s.Cost.CollectorMilliseconds)).Append(" | ").Append(Ms(s.Cost.FrameBuildMilliseconds))
                        .Append(" | ").Append(Ms(s.Cost.DriveMilliseconds)).Append(" | ").Append(Ms(s.Instrumentation)).Append(" | ")
                        .Append(s.Collections).Append(" |\n");
            }

            string folder = Story533Harness.Folder;
            Directory.CreateDirectory(folder);
            File.WriteAllText(folder + "/perf-N" + population + "-" + traceStamp + "-summary.md", text.ToString());
            var frameText = new StringBuilder("frame\twall_ms\tfixed\tfirst_step\tlast_step\t" + string.Join("\t", Stats) + "\n");
            foreach (var f in frames)
            {
                frameText.Append(f.Index).Append('\t').Append(Ms(f.WallMilliseconds)).Append('\t').Append(f.FixedSteps).Append('\t')
                    .Append(f.FirstHostStep).Append('\t').Append(f.LastHostStep);
                for (int i = 0; i < Stats.Length; i++)
                    frameText.Append('\t').Append(Stats[i] == "GC Allocated In Frame" || Stats[i] == "GC.Alloc"
                        ? (double.IsNaN(f.Values[i]) ? "-" : f.Values[i].ToString("0", CultureInfo.InvariantCulture)) : Ms(f.Values[i] / 1e6));
                frameText.Append('\n');
            }
            File.WriteAllText(folder + "/perf-N" + population + "-" + traceStamp + "-frames.tsv", frameText.ToString());
            UnityEngine.Debug.Log("[Story533] diagnostic de performance N = " + population + " publie.");
            return result;
        }

        private static void WriteComparison()
        {
            var populations = Results.Keys.OrderBy(k => k).ToList();
            var text = new StringBuilder();
            text.Append("# Comparaison du cout Traffic V2, N = ").Append(string.Join(", ", populations.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToArray()))
                .Append(" (").Append(campaignStamp).Append(")\n\nMoyennes par pas hote (ms). Marginal : difference avec la population precedente ; par vehicule : valeur / N.\n\n");
            var keys = Results.Values.SelectMany(r => r.PerStep.Keys).Distinct().ToList();
            text.Append("| Section |");
            foreach (var p in populations) text.Append(" N=").Append(p).Append(" |");
            for (int i = 1; i < populations.Count; i++) text.Append(" marginal ").Append(populations[i - 1]).Append("->").Append(populations[i]).Append(" |");
            foreach (var p in populations) text.Append(" /veh N=").Append(p).Append(" |");
            text.Append('\n').Append("|---|").Append(string.Concat(Enumerable.Repeat("---|", populations.Count * 3 - 1))).Append('\n');
            foreach (var key in keys)
            {
                text.Append("| ").Append(key).Append(" |");
                foreach (var p in populations) text.Append(' ').Append(Ms(Value(p, key))).Append(" |");
                for (int i = 1; i < populations.Count; i++) text.Append(' ').Append(Ms(Value(populations[i], key) - Value(populations[i - 1], key))).Append(" |");
                foreach (var p in populations) text.Append(' ').Append(Ms(Value(p, key) / p)).Append(" |");
                text.Append('\n');
            }
            text.Append("\n| N | fps | FixedUpdate par frame | pics > 20 / 50 / 100 ms | octets alloues par pas | collections gen0 par minute |\n|---|---|---|---|---|---|\n");
            foreach (var p in populations)
            {
                var r = Results[p];
                text.Append("| ").Append(p).Append(" | ").Append(Ms(r.FramesPerSecond)).Append(" | ").Append(Ms(r.FixedPerFrame)).Append(" | ")
                    .Append(r.Spikes20).Append(" / ").Append(r.Spikes50).Append(" / ").Append(r.Spikes100).Append(" | ").Append(r.BytesPerStep)
                    .Append(" | ").Append(Ms(r.CollectionsPerMinute)).Append(" |\n");
            }
            File.WriteAllText(Story533Harness.Folder + "/perf-comparison-" + campaignStamp + ".md", text.ToString());
            UnityEngine.Debug.Log("[Story533] comparaison de performance publiee.");
        }

        private static double Value(int population, string key)
        {
            PopulationResult result;
            double value;
            return Results.TryGetValue(population, out result) && result.PerStep.TryGetValue(key, out value) ? value : double.NaN;
        }
    }
}
