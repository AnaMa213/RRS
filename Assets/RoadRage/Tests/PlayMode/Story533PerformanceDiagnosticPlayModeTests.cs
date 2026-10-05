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
    /// Publie un resume par population, la timeline des pics et une comparaison 1 -> 8, puis juge les cibles D13.
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
            "TrafficV2.Driver.MotionCommand", "TrafficV2.Driver.Instrumentation", "Story533.Harness.Observe",
            "TrafficV2.Coordinate", "TrafficV2.Driver.Junction",
            // Diagnostic D13 : attribuer aussi les pics hors des scripts FixedUpdate, sans exclure de frame.
            "Update.ScriptRunBehaviourUpdate", "PreLateUpdate.ScriptRunBehaviourLateUpdate",
            "CoroutinesDelayedCalls", "Camera.Render", "RenderPipelineManager.DoRenderLoop_Internal",
            "Gfx.WaitForPresentOnGfxThread", "Gfx.PresentFrame", "WaitForTargetFPS", "EditorLoop", "SceneView.Repaint"
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
        /// N = 8 (cible D13 : p95 du pas hote sous 10 ms, lecture proprietaire du 2026-10-03) sur explore-8, quatre entrees et deux
        /// vagues, coordination de carrefour branchee (5.34). Aucune exemption : D12 est retiree et les verdicts O6 s'appliquent.
        /// </summary>
        [UnityTest, Order(5), Timeout(1800000)]
        public IEnumerator DiagnoseEightVehiclesAndCompare()
        {
            yield return Diagnose("explore-8", 8);
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

        internal sealed class FrameSample
        {
            public int Index;
            public double WallMilliseconds;
            public int FixedSteps;
            public ulong FirstHostStep, LastHostStep;
            public double[] Values;
            public int[] Counts;
            public bool Partial;
        }

        internal sealed class PopulationResult
        {
            public int Population;
            public int HostSteps;
            public double WallSeconds;
            public readonly Dictionary<string, double> PerStep = new Dictionary<string, double>();
            public readonly Dictionary<string, double> PerVehicle = new Dictionary<string, double>();
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

        /// <summary>Un intervalle Update : epoques effectivement executees, independamment de l'ordre des FixedUpdate.</summary>
        internal static FrameSample FrameInterval(ulong previousHost, ulong host, int physicalSteps, double milliseconds)
        {
            return new FrameSample { FixedSteps = physicalSteps,
                FirstHostStep = host > previousHost ? previousHost + 1UL : 0UL,
                LastHostStep = host > previousHost ? host : 0UL, WallMilliseconds = milliseconds };
        }

        /// <summary>
        /// Les recorders lus dans Update portent la frame precedente. Son intervalle et ses epoques sont conserves
        /// ensemble jusqu'a cette lecture ; aucune hypothese sur l'ordre du spawner et d'une sonde FixedUpdate.
        /// Les deux bords partiels de la fenetre restent dans le maximum D13.
        /// </summary>
        internal sealed class FrameAssembly
        {
            public readonly List<FrameSample> Frames = new List<FrameSample>();
            private ulong lastHost;
            private double lastUpdate;
            private FrameSample pending;
            private int physicalSteps;

            public void PhysicalStep() { physicalSteps++; }

            public void Begin(ulong host, double now)
            {
                lastHost = host;
                lastUpdate = now;
                physicalSteps = 0;
            }

            public void Update(ulong host, double now, double previousFrameMilliseconds, Action<FrameSample> readRecorders)
            {
                if (pending != null)
                {
                    // Delta non scale du moteur : duree de la frame precedente, sans inclure les FixedUpdate
                    // de la frame courante comme le ferait now - lastUpdate. Un bord partiel garde sa duree propre.
                    if (!pending.Partial)
                    {
                        pending.WallMilliseconds = previousFrameMilliseconds;
                        readRecorders(pending);
                    }
                    Frames.Add(pending);
                }
                pending = Capture(host, now);
            }

            private FrameSample Capture(ulong host, double now)
            {
                var sample = FrameInterval(lastHost, host, physicalSteps, (now - lastUpdate) * 1000.0);
                sample.Index = Frames.Count;
                sample.Partial = Frames.Count == 0 && pending == null;
                sample.Values = Enumerable.Repeat(double.NaN, Stats.Length).ToArray();
                sample.Counts = new int[Stats.Length];
                lastHost = host;
                physicalSteps = 0;
                lastUpdate = now;
                return sample;
            }

            public void End(ulong host, double now)
            {
                // Pas d'Update suivant dans la fenetre : les valeurs moteur sont encore indisponibles, pas inventees.
                if (pending != null)
                {
                    if (!pending.Partial) pending.WallMilliseconds = double.NaN;
                    pending.Partial = true;
                    Frames.Add(pending);
                }
                var final = Capture(host, now);
                final.Partial = true;
                if (final.FixedSteps > 0) Frames.Add(final);
            }
        }

        private sealed class FrameProbe : MonoBehaviour
        {
            public PortalTrafficSpawner Spawner;
            public ProfilerRecorder[] Recorders;
            private bool recording;
            private readonly FrameAssembly assembly = new FrameAssembly();
            public List<FrameSample> Frames { get { return assembly.Frames; } }

            private void FixedUpdate() { if (recording) assembly.PhysicalStep(); }
            public void Begin()
            {
                assembly.Begin(Spawner.V2Runner.FrameId, Time.realtimeSinceStartupAsDouble);
                recording = true;
            }
            private void Update()
            {
                if (recording) assembly.Update(Spawner.V2Runner.FrameId, Time.realtimeSinceStartupAsDouble,
                    Time.unscaledDeltaTime * 1000.0, ReadRecorders);
            }
            private void ReadRecorders(FrameSample sample)
            {
                for (int i = 0; i < Recorders.Length; i++)
                {
                    if (!Recorders[i].Valid || Recorders[i].Count == 0) continue;
                    var last = Recorders[i].GetSample(0);
                    sample.Values[i] = last.Value;
                    sample.Counts[i] = (int)last.Count;
                }
            }
            public void End()
            {
                recording = false;
                assembly.End(Spawner.V2Runner.FrameId, Time.realtimeSinceStartupAsDouble);
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
            int recorderStartFrame = -1;
            bool recording = false;
            double recorderSetupMilliseconds = double.NaN;
            ulong recorderSetupHost = 0UL, recordingStartHost = 0UL;
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

                // Initialiser les recorders APRES creation des marqueurs, AVANT les cinq pas de chauffe.
                // GetAvailable/StartNew peuvent bloquer l'Editeur : leur dette de rattrapage n'est pas du trafic mesure.
                if (recorders == null && cost.Vehicles > 0)
                {
                    watch.Restart();
                    recorders = StartRecorders(out missing);
                    recorderSetupMilliseconds = watch.Elapsed.TotalMilliseconds;
                    recorderSetupHost = spawner.V2Runner.FrameId;
                    probe.Recorders = recorders;
                    recorderStartFrame = Time.frameCount;
                    warmup = step + 5;
                }
                // Un Update doit aussi avoir termine la frame d'initialisation. Une fois Begin appele,
                // tous les pas et les deux bords partiels restent dans le maximum D13, sans exemption.
                if (!recording && recorders != null && step >= warmup && Time.frameCount > recorderStartFrame)
                {
                    probe.Begin();
                    recording = true;
                    recordingStartHost = spawner.V2Runner.FrameId;
                    wallStart = Time.realtimeSinceStartupAsDouble;
                    collectionsStart = GC.CollectionCount(0);
                }

                watch.Restart();
                ObserveMarker.Begin();
                observer.Observe();
                ObserveMarker.End();
                double observed = watch.Elapsed.TotalMilliseconds;

                if (recording)
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
            if (recording) probe.End();

            // Ecritures disque de fin de run, mesurees a part (aucune pendant le run).
            watch.Restart();
            var runs = Story533Harness.Runs(spawner);
            string traceStamp = Story533Harness.Stamp();
            Story533Harness.WriteTrace("perf-N" + population, traceStamp, runs);
            double traceMilliseconds = watch.Elapsed.TotalMilliseconds;

            var result = Publish(population, record, steps, probe.Frames, wallSeconds, totalCollections, missing, traceMilliseconds,
                observer, traceStamp, recorderSetupMilliseconds, recorderSetupHost, recordingStartHost);
            Results[population] = result;
            if (recorders != null) foreach (var recorder in recorders) recorder.Dispose();
            if (population == 8) WriteComparison();
            Assert.That(steps.Count(s => s.Vehicles > 0), Is.GreaterThan(0), "aucun vehicule conduit : diagnostic vide");
            var full = steps.Where(s => s.Vehicles == population).ToList();
            var failures = D13Failures(population, full.Count,
                full.Count == 0 ? double.NaN : full.Average(s => s.Cost.TotalMilliseconds),
                probe.Frames.Count, probe.Frames.Count == 0 ? 0 : probe.Frames.Max(f => f.FixedSteps),
                probe.Frames.All(EpochsAligned), Percentile(full.Select(s => s.Cost.TotalMilliseconds).ToList(), 0.95));
            Assert.That(failures, Is.Empty, string.Join(" | ", failures));
            // Verdicts O6 (Story 5.34) : aucun verrou nominal, aucune exemption, coordination saine, aucun contact V2-V2.
            List<string> junctionFailures, otherVehicleContacts;
            Story533Harness.ClassifyVehicleContacts(runs, admission.Model, out junctionFailures, out otherVehicleContacts);
            Assert.That(observer.ToleranceLatch, Is.Null, "verrou 2a nominal : " + observer.ToleranceLatch);
            Assert.That(observer.IncompatibleGrantSteps, Is.Zero, "echec 5.34 : " + observer.FirstIncompatibleGrants);
            Assert.That(observer.EnteredWithoutGrant, Is.Zero, "echec 5.34 : EnteredWithoutGrant");
            Assert.That(junctionFailures, Is.Empty, "echec 5.34 : " + string.Join(" | ", junctionFailures));
            Assert.That(otherVehicleContacts, Is.Empty, "contact V2-V2 autre : " + string.Join(" | ", otherVehicleContacts));
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
            var engineStats = new[] { "Update.ScriptRunBehaviourUpdate", "PreLateUpdate.ScriptRunBehaviourLateUpdate",
                "CoroutinesDelayedCalls", "Camera.Render", "RenderPipelineManager.DoRenderLoop_Internal",
                "Gfx.WaitForPresentOnGfxThread", "Gfx.PresentFrame", "WaitForTargetFPS", "EditorLoop", "SceneView.Repaint" };
            foreach (string stat in engineStats)
            {
                double milliseconds = FrameValue(frame, stat);
                if (!double.IsNaN(milliseconds) && milliseconds > 5d)
                    text += " ; " + stat + " " + Ms(milliseconds) + " ms";
            }
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

        internal static bool EpochsAligned(FrameSample frame)
        {
            return frame.FixedSteps >= 0 && (ulong)frame.FixedSteps ==
                (frame.FirstHostStep == 0UL ? 0UL : frame.LastHostStep - frame.FirstHostStep + 1UL);
        }

        internal static double RecorderMeanPerStep(IReadOnlyList<FrameSample> frames, int statIndex,
            out int measuredFrames, out int measuredSteps)
        {
            measuredFrames = measuredSteps = 0;
            double total = 0d;
            foreach (var frame in frames)
            {
                if (frame.Partial || statIndex < 0 || double.IsNaN(frame.Values[statIndex])) continue;
                measuredFrames++;
                measuredSteps += frame.FixedSteps;
                total += frame.Values[statIndex] / 1e6;
            }
            return measuredSteps == 0 ? double.NaN : total / measuredSteps;
        }

        /// <summary>
        /// Cibles D13 declarees : moyennes en population pleine, maximum de pas sur toute la fenetre N4 ; a N = 8, p95 du pas hote
        /// en population pleine sous 10 ms (lecture proprietaire du 2026-10-03, Story 5.34), sans objet si le p95 n'est pas fourni.
        /// </summary>
        internal static List<string> D13Failures(int population, int fullSteps, double fullMeanMilliseconds,
            int frames, int maxFixedSteps, bool epochsAligned = true, double fullP95Milliseconds = double.NaN)
        {
            var failures = new List<string>();
            if (fullSteps == 0 || double.IsNaN(fullMeanMilliseconds) || double.IsInfinity(fullMeanMilliseconds))
                failures.Add("D13 : population pleine non mesuree");
            else
            {
                if (fullMeanMilliseconds / population > 1d) failures.Add("D13 : cout moyen en population pleine > 1 ms par vehicule");
                if (population == 8 && fullMeanMilliseconds >= 10d) failures.Add("D13 : N8 doit rester sous 10 ms par pas hote");
                if (population == 8 && !double.IsNaN(fullP95Milliseconds) && fullP95Milliseconds >= 10d)
                    failures.Add("D13 : p95 du pas hote a N8 >= 10 ms");
            }
            if (frames == 0) failures.Add("D13 : fenetre de frames vide");
            if (!epochsAligned) failures.Add("D13 : epoques hote et pas physiques non alignes");
            if (population == 4 && maxFixedSteps > 2) failures.Add("D13 : explore-4/N4 depasse deux FixedUpdate par frame");
            return failures;
        }

        private static PopulationResult Publish(int population, Story533Harness.ScenarioRecord record, List<StepSample> steps,
            List<FrameSample> frames, double wallSeconds, int collections, string missing, double traceMilliseconds,
            Story533Harness.Observer observer, string traceStamp, double recorderSetupMilliseconds,
            ulong recorderSetupHost, ulong recordingStartHost)
        {
            var driven = steps.Where(s => s.Vehicles > 0).ToList();
            var full = driven.Where(s => s.Vehicles == population).ToList();
            double fullMean = full.Count == 0 ? double.NaN : full.Average(s => s.Cost.TotalMilliseconds);
            double fullP95 = Percentile(full.Select(s => s.Cost.TotalMilliseconds).ToList(), 0.95);
            double coordinatorP95 = Percentile(full.Select(s => s.Cost.CoordinatorMilliseconds).ToList(), 0.95);
            int maxFixed = frames.Count == 0 ? 0 : frames.Max(f => f.FixedSteps);
            var d13 = D13Failures(population, full.Count, fullMean, frames.Count, maxFixed, frames.All(EpochsAligned), fullP95);
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
                // Story 5.34 : lot du coordinateur de carrefour, publie a part (inclus dans le pas Traffic V2 total).
                new KeyValuePair<string, List<double>>("  coordinateur de carrefour (lot)", per(s => s.Cost.CoordinatorMilliseconds)),
                new KeyValuePair<string, List<double>>("harnais : observateur par pas", per(s => s.ObserverMilliseconds))
            };
            var vehicleCounts = driven.Select(s => s.Vehicles).ToArray();
            foreach (var section in sections) RecordStepMetric(result, section.Key.Trim(), section.Value, vehicleCounts);
            result.PerStep["perception par vehicule"] = result.PerVehicle["perception par vehicule"] = result.PerVehicle["perception (tous vehicules)"];
            result.PerStep["pas Traffic V2 par vehicule"] = result.PerVehicle["pas Traffic V2 par vehicule"] = result.PerVehicle["pas Traffic V2 total"];
            RecordStepMetric(result, "OverlapSphereNonAlloc par pas", per(s => s.Cost.Overlaps), vehicleCounts);
            result.BytesPerStep = driven.Count == 0 ? 0 : (long)driven.Average(s => (double)s.Cost.TotalBytes);

            // Par frame rendue (recorders) : moyennes par pas hote des sous-sections hors ordonnanceur.
            var recorderCoverage = new Dictionary<string, int[]>();
            foreach (var stat in new[] { "TrafficV2.Frame.Localize", "TrafficV2.Frame.Occupancy", "TrafficV2.Frame.Hazards",
                "TrafficV2.Spine.Route", "TrafficV2.Spine.Horizon", "TrafficV2.Spine.Motion", "TrafficV2.Spine.Projection",
                "Physics.Simulate", "FixedBehaviourUpdate", "FixedUpdate.PhysicsFixedUpdate", "GC.Collect", "TrafficV2.Step" })
            {
                int measuredFrames, measuredSteps;
                result.PerStep["[recorder] " + stat] = RecorderMeanPerStep(frames, StatIndex(stat), out measuredFrames, out measuredSteps);
                recorderCoverage[stat] = new[] { measuredFrames, measuredSteps };
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
            text.Append("- Initialisation des recorders hors mesure : ").Append(Ms(recorderSetupMilliseconds))
                .Append(" ms au pas hote ").Append(recorderSetupHost).Append(" ; debut apres chauffe au pas ")
                .Append(recordingStartHost).Append(" et apres un Update de l'Editeur.\n");
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

            text.Append("## Porte D13\n\n- Population pleine : ").Append(full.Count).Append(" pas ; cout moyen ")
                .Append(Ms(fullMean)).Append(" ms/pas hote, ").Append(Ms(fullMean / population)).Append(" ms/vehicule (cible <= 1) ; p95 ")
                .Append(Ms(fullP95)).Append(" ms/pas hote ; coordinateur de carrefour p95 ").Append(Ms(coordinatorP95)).Append(" ms.\n")
                .Append("- N8 : cible < 10 ms/pas hote en population pleine, en moyenne et au p95 (5.34).\n")
                .Append("- Coordination (verdicts O6) : grants incompatibles simultanes ").Append(observer.IncompatibleGrantSteps)
                .Append(", EnteredWithoutGrant ").Append(observer.EnteredWithoutGrant).Append(", lots ").Append(observer.Batches).Append(".\n")
                .Append("- Maximum sur toute la fenetre : ")
                .Append(maxFixed).Append(" FixedUpdate/frame (cible N4 <= 2, transitions et bords partiels inclus).\n")
                .Append("- Alignement compteur physique / epoques hote : ").Append(frames.All(EpochsAligned) ? "verifie" : "ECHEC")
                .Append(".\n- Verdict : ").Append(d13.Count == 0 ? "vert" : string.Join(" | ", d13)).Append("\n\n")
                .Append("Les pas physiques et epoques sont captures dans Update. A l'Update suivant, les recorders et la duree de la frame precedente (Time.unscaledDeltaTime) leur sont attaches. Les bords partiels gardent leur duree mesuree ; une frame en attente dont la duree moteur n'est plus lisible publie une duree absente et Partial=true. Les recorders absents ne contribuent ni au numerateur ni au denominateur de leur moyenne par pas.\n\n")
                .Append("### Depassements de deux pas ou desalignements (aucune exclusion)\n\n| frame | pas hote | physiques | ms | population des pas | frame precedente ms | Traffic V2 precedent | GC precedent |\n|---|---|---|---|---|---|---|---|\n");
            for (int i = 0; i < frames.Count; i++)
            {
                var f = frames[i];
                if (f.FixedSteps <= 2 && EpochsAligned(f)) continue;
                var previous = i > 0 ? frames[i - 1] : null;
                var populations = steps.Where(s => s.HostStep >= f.FirstHostStep && s.HostStep <= f.LastHostStep)
                    .Select(s => s.HostStep.ToString(CultureInfo.InvariantCulture) + ":N" + s.Vehicles).ToArray();
                text.Append("| ").Append(f.Index).Append(" | ").Append(f.FirstHostStep).Append('-').Append(f.LastHostStep)
                    .Append(" | ").Append(f.FixedSteps).Append(" | ").Append(Ms(f.WallMilliseconds)).Append(" | ")
                    .Append(string.Join(", ", populations)).Append(" | ").Append(Ms(previous == null ? double.NaN : previous.WallMilliseconds))
                    .Append(" | ").Append(Ms(previous == null ? double.NaN : FrameValue(previous, "TrafficV2.Step")))
                    .Append(" | ").Append(Ms(previous == null ? double.NaN : FrameValue(previous, "GC.Collect"))).Append(" |\n");
            }

            text.Append("## Cout par pas hote (ms, pas avec au moins un vehicule)\n\n| Section | moyenne | mediane | p95 | max |\n|---|---|---|---|---|\n");
            foreach (var section in sections) text.Append(Stat(section.Key.Replace("  ", "&nbsp;&nbsp;"), section.Value));
            text.Append(Stat("perception par vehicule", per(s => s.Perception / s.Vehicles)));
            text.Append(Stat("pas Traffic V2 par vehicule", per(s => s.Cost.TotalMilliseconds / s.Vehicles)));
            text.Append("\nOverlapSphereNonAlloc : ").Append(Ms(result.PerStep["OverlapSphereNonAlloc par pas"])).Append(" requetes par pas, ")
                .Append(Ms(driven.Count == 0 ? double.NaN : driven.Average(s => s.Cost.Overlaps == 0 ? 0 : s.Cost.OverlapMilliseconds / s.Cost.Overlaps)))
                .Append(" ms par requete.\n\n");

            text.Append("## Sous-sections par pas hote (recorders, ms)\n\n| Stat | ms par pas hote | frames mesurees | pas physiques mesures |\n|---|---|---|---|\n");
            foreach (var pair in result.PerStep.Where(p => p.Key.StartsWith("[recorder]")))
            {
                var coverage = recorderCoverage[pair.Key.Substring(11)];
                text.Append("| ").Append(pair.Key.Substring(11)).Append(" | ").Append(Ms(pair.Value)).Append(" | ")
                    .Append(coverage[0]).Append(" | ").Append(coverage[1]).Append(" |\n");
            }

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
            var frameText = new StringBuilder("frame\twall_ms\tfixed\tfirst_step\tlast_step\tpartial\t" + string.Join("\t", Stats) + "\n");
            foreach (var f in frames)
            {
                frameText.Append(f.Index).Append('\t').Append(Ms(f.WallMilliseconds)).Append('\t').Append(f.FixedSteps).Append('\t')
                    .Append(f.FirstHostStep).Append('\t').Append(f.LastHostStep).Append('\t').Append(f.Partial ? 1 : 0);
                for (int i = 0; i < Stats.Length; i++)
                    frameText.Append('\t').Append(Stats[i] == "GC Allocated In Frame" || Stats[i] == "GC.Alloc"
                        ? (double.IsNaN(f.Values[i]) ? "-" : f.Values[i].ToString("0", CultureInfo.InvariantCulture)) : Ms(f.Values[i] / 1e6));
                frameText.Append('\n');
            }
            File.WriteAllText(folder + "/perf-N" + population + "-" + traceStamp + "-frames.tsv", frameText.ToString());
            UnityEngine.Debug.Log("[Story533] diagnostic de performance N = " + population + " publie.");
            return result;
        }

        /// <summary>Pas avec vehicule seulement : chaque cout est normalise par sa population reelle avant la moyenne.</summary>
        internal static void RecordStepMetric(PopulationResult result, string key, IReadOnlyList<double> values, IReadOnlyList<int> vehicleCounts)
        {
            result.PerStep[key] = values.Count == 0 ? double.NaN : values.Average();
            result.PerVehicle[key] = values.Count == 0 ? double.NaN : values.Select((v, i) => v / vehicleCounts[i]).Average();
        }

        private static void WriteComparison()
        {
            File.WriteAllText(Story533Harness.Folder + "/perf-comparison-" + campaignStamp + ".md", ComparisonText(Results, campaignStamp));
            UnityEngine.Debug.Log("[Story533] comparaison de performance publiee.");
        }

        internal static string ComparisonText(IReadOnlyDictionary<int, PopulationResult> results, string stamp)
        {
            var populations = results.Keys.OrderBy(k => k).ToList();
            var text = new StringBuilder();
            text.Append("# Comparaison du cout Traffic V2, N = ").Append(string.Join(", ", populations.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToArray()))
                .Append(" (").Append(stamp).Append(")\n\nMoyennes par pas hote (ms). Marginal : difference avec la population precedente ; par vehicule : moyenne des couts divises par le nombre reel de vehicules a chaque pas. Les lignes deja par vehicule gardent leur valeur ; les recorders globaux ne sont pas normalises par vehicule (-).\n\n");
            var keys = results.Values.SelectMany(r => r.PerStep.Keys).Distinct().ToList();
            text.Append("| Section |");
            foreach (var p in populations) text.Append(" N=").Append(p).Append(" |");
            for (int i = 1; i < populations.Count; i++) text.Append(" marginal ").Append(populations[i - 1]).Append("->").Append(populations[i]).Append(" |");
            foreach (var p in populations) text.Append(" /veh N=").Append(p).Append(" |");
            text.Append('\n').Append("|---|").Append(string.Concat(Enumerable.Repeat("---|", populations.Count * 3 - 1))).Append('\n');
            foreach (var key in keys)
            {
                text.Append("| ").Append(key).Append(" |");
                foreach (var p in populations) text.Append(' ').Append(Ms(Value(results, p, key))).Append(" |");
                for (int i = 1; i < populations.Count; i++) text.Append(' ').Append(Ms(Value(results, populations[i], key) - Value(results, populations[i - 1], key))).Append(" |");
                foreach (var p in populations) text.Append(' ').Append(Ms(Value(results, p, key, true))).Append(" |");
                text.Append('\n');
            }
            text.Append("\n| N | fps | FixedUpdate par frame | pics > 20 / 50 / 100 ms | octets alloues par pas | collections gen0 par minute |\n|---|---|---|---|---|---|\n");
            foreach (var p in populations)
            {
                var r = results[p];
                text.Append("| ").Append(p).Append(" | ").Append(Ms(r.FramesPerSecond)).Append(" | ").Append(Ms(r.FixedPerFrame)).Append(" | ")
                    .Append(r.Spikes20).Append(" / ").Append(r.Spikes50).Append(" / ").Append(r.Spikes100).Append(" | ").Append(r.BytesPerStep)
                    .Append(" | ").Append(Ms(r.CollectionsPerMinute)).Append(" |\n");
            }
            return text.ToString();
        }

        private static double Value(IReadOnlyDictionary<int, PopulationResult> results, int population, string key, bool perVehicle = false)
        {
            PopulationResult result;
            double value;
            return results.TryGetValue(population, out result)
                && (perVehicle ? result.PerVehicle : result.PerStep).TryGetValue(key, out value) ? value : double.NaN;
        }
    }
}
