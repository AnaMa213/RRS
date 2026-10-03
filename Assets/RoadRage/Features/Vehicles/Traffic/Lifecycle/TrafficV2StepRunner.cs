using System;
using System.Collections.Generic;
using System.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using Unity.Profiling;

namespace RoadRage.Features.Vehicles.Traffic.Lifecycle
{
    /// <summary>
    /// Cout mesure d'un pas hote (diagnostic de performance 5.33) : temps (ms) et octets alloues sur le thread du pas,
    /// par section. Lecture seule pour les tests ; n'influence aucune decision.
    /// </summary>
    public struct TrafficV2StepCost
    {
        public int Vehicles;
        public int Overlaps;
        public double TotalMilliseconds, PrepareMilliseconds, CollectorMilliseconds, OverlapMilliseconds, FrameBuildMilliseconds,
            DriveMilliseconds;
        public long TotalBytes, PrepareBytes, CollectorBytes, FrameBuildBytes, DriveBytes;
    }

    /// <summary>
    /// Ordonnanceur hote V2 (Story 5.33), sans etat de decision. A chaque pas physique : FrameId = compteur de pas
    /// hote global ; chaque vehicule V2 lie fournit son entree d'acteur (pose, empreinte, vitesse) ; le collecteur rend
    /// les dangers ; une seule TrafficFrame est construite, avant tout pas de conduite ; puis chaque vehicule recoit
    /// exactement un pas, donc un intent, par TrafficId croissant, depuis cette frame et aucune autre. Les horizons
    /// d'intention publies ne sont pas alimentes (5.34).
    /// </summary>
    public sealed class TrafficV2StepRunner
    {
        private readonly TrafficV2HazardCollector collector;
        private readonly List<TrafficV2VehicleDriver> ordered = new List<TrafficV2VehicleDriver>();
        private readonly List<TrafficV2VehicleDriver> prepared = new List<TrafficV2VehicleDriver>();
        private readonly List<TrafficActorInput> inputs = new List<TrafficActorInput>();
        private readonly List<HazardQuery> queries = new List<HazardQuery>();
        private readonly HashSet<RoadId> actorIds = new HashSet<RoadId>();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Stopwatch stepWatch = new Stopwatch();
        private readonly Stopwatch sectionWatch = new Stopwatch();

        private static readonly ProfilerMarker StepMarker = new ProfilerMarker("TrafficV2.Step");
        private static readonly ProfilerMarker PrepareMarker = new ProfilerMarker("TrafficV2.Prepare");
        private static readonly ProfilerMarker CollectMarker = new ProfilerMarker("TrafficV2.Collect");
        private static readonly ProfilerMarker FrameBuildMarker = new ProfilerMarker("TrafficV2.FrameBuild");
        private static readonly ProfilerMarker DriveMarker = new ProfilerMarker("TrafficV2.Drive");

        public TrafficV2StepRunner()
            : this(new TrafficV2HazardCollector(TrafficV2Settings.HazardQueryCapacity, TrafficV2Settings.HazardQueryRadiusMeters)) { }

        public TrafficV2StepRunner(TrafficV2HazardCollector collector)
        {
            if (collector == null) throw new ArgumentNullException("collector");
            this.collector = collector;
        }

        /// <summary>Compteur de pas hote global : epoque de la frame, de la commande et du composeur.</summary>
        public ulong FrameId { get; private set; }
        public TrafficV2HazardCollector Collector { get { return collector; } }
        /// <summary>Frame du dernier pas ; nulle sans vehicule, ou si sa construction a ete refusee.</summary>
        public TrafficFrame LastFrame { get; private set; }
        /// <summary>Vehicules qui ont recu un pas au dernier pas hote.</summary>
        public int LastSteppedCount { get; private set; }
        /// <summary>Frames construites depuis le debut de la session : au plus une par pas hote.</summary>
        public int FramesBuilt { get; private set; }
        /// <summary>Constructions de frame refusees (entree invalide) : chaque vehicule recoit alors le repli.</summary>
        public int FrameFailures { get; private set; }
        /// <summary>Temps de la frame partagee du dernier pas, collecteur compris (ms).</summary>
        public double LastFrameMilliseconds { get; private set; }
        /// <summary>Temps du collecteur seul au dernier pas (ms).</summary>
        public double LastCollectorMilliseconds { get; private set; }
        /// <summary>Cout du dernier pas hote par section (diagnostic de performance).</summary>
        public TrafficV2StepCost LastCost { get; private set; }

        public void Step(CompiledRoadModel model, IEnumerable<TrafficV2VehicleDriver> drivers)
        {
            var cost = new TrafficV2StepCost();
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            stepWatch.Restart();
            StepMarker.Begin();
            try { Run(model, drivers, ref cost); }
            finally
            {
                StepMarker.End();
                cost.TotalMilliseconds = stepWatch.Elapsed.TotalMilliseconds;
                cost.TotalBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
                LastCost = cost;
            }
        }

        private void Run(CompiledRoadModel model, IEnumerable<TrafficV2VehicleDriver> drivers, ref TrafficV2StepCost cost)
        {
            FrameId++;
            LastFrame = null;
            LastSteppedCount = 0;
            LastFrameMilliseconds = 0d;
            LastCollectorMilliseconds = 0d;
            if (model == null || drivers == null) return;
            ordered.Clear();
            foreach (var driver in drivers)
                if (driver != null && driver.IsBound) ordered.Add(driver);
            ordered.Sort((a, b) => a.TrafficId.CompareTo(b.TrafficId));

            inputs.Clear(); prepared.Clear(); queries.Clear(); actorIds.Clear();
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            sectionWatch.Restart();
            PrepareMarker.Begin();
            for (int i = 0; i < ordered.Count; i++)
            {
                // Un vehicule inerte (profil absent) reste un acteur de la frame, sans pas ni requete de dangers.
                TrafficActorInput input;
                if (!ordered[i].TryPrepareStep(out input)) continue;
                inputs.Add(input);
                actorIds.Add(input.TrafficId);
                if (!ordered[i].StepPrepared) continue;
                prepared.Add(ordered[i]);
                queries.Add(ordered[i].HazardQuery);
            }
            PrepareMarker.End();
            cost.PrepareMilliseconds = sectionWatch.Elapsed.TotalMilliseconds;
            cost.PrepareBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
            if (inputs.Count == 0) return;

            stopwatch.Restart();
            allocated = GC.GetAllocatedBytesForCurrentThread();
            CollectMarker.Begin();
            var hazards = collector.Collect(queries, actorIds);
            CollectMarker.End();
            LastCollectorMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            cost.CollectorMilliseconds = LastCollectorMilliseconds;
            cost.CollectorBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
            cost.Overlaps = collector.LastOverlapCount;
            cost.OverlapMilliseconds = collector.LastOverlapMilliseconds;
            allocated = GC.GetAllocatedBytesForCurrentThread();
            TrafficFrame frame;
            FrameBuildMarker.Begin();
            try
            {
                frame = new TrafficFrame(FrameId, model, inputs, hazards);
                FramesBuilt++;
            }
            catch (ArgumentException)
            {
                frame = null;
                FrameFailures++;
            }
            FrameBuildMarker.End();
            LastFrameMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            cost.FrameBuildMilliseconds = LastFrameMilliseconds - LastCollectorMilliseconds;
            cost.FrameBuildBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
            LastFrame = frame;
            double share = LastFrameMilliseconds / inputs.Count;
            allocated = GC.GetAllocatedBytesForCurrentThread();
            sectionWatch.Restart();
            DriveMarker.Begin();
            for (int i = 0; i < prepared.Count; i++)
            {
                HazardQueryReport report;
                collector.TryGetReport(prepared[i].TrafficId, out report);
                prepared[i].Step(FrameId, frame, report, collector.Counters, share);
            }
            DriveMarker.End();
            cost.DriveMilliseconds = sectionWatch.Elapsed.TotalMilliseconds;
            cost.DriveBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
            cost.Vehicles = prepared.Count;
            LastSteppedCount = prepared.Count;
        }
    }
}
