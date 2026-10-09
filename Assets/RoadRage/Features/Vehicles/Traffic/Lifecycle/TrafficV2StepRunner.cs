using System;
using System.Collections.Generic;
using System.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Recovery;
using RoadRage.Features.Vehicles.Traffic.Signals;
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
        /// <summary>Lot du coordinateur de carrefour (5.34), rapports d'occupation des vehicules sans pas compris ; publie a part.</summary>
        public double CoordinatorMilliseconds;
        public long CoordinatorBytes;
    }

    /// <summary>
    /// Ordonnanceur hote V2 (Story 5.33) : il n'a d'autre etat de decision que le coordinateur de carrefour qu'il possede
    /// (5.34). A chaque pas physique : FrameId = compteur de pas hote global ; chaque vehicule V2 lie fournit son entree
    /// d'acteur (pose, empreinte, vitesse) ; le collecteur rend les dangers ; une seule TrafficFrame N est construite, avant
    /// tout pas de conduite ; puis chaque vehicule recoit exactement un pas, donc un intent, par TrafficId croissant, depuis
    /// cette frame et l'instantane de coordination effectif a N, et aucun autre ; enfin le coordinateur resout les demandes
    /// de N et publie l'instantane effectif a N+1. Une frame refusee donne un lot fail-closed. Les horizons d'intention
    /// publies ne sont pas alimentes.
    /// Story 5.36 : il possede aussi l'horloge de phase des feux du modele, etat hote : la frame N porte la phase courante, le lot
    /// la lit dans cette frame, puis l'horloge avance d'un pas fixe. Sans plan (MVP_Run), elle ne fait rien.
    /// Story 5.40 : il possede le superviseur d'interblocage ; apres les pas de conduite de N, le graphe d'attente de N donne les
    /// cycles soumis au lot de N, puis les resolutions publiees sont journalisees.
    /// </summary>
    public sealed class TrafficV2StepRunner
    {
        private readonly TrafficV2HazardCollector collector;
        private readonly List<TrafficV2VehicleDriver> ordered = new List<TrafficV2VehicleDriver>();
        private readonly List<TrafficV2VehicleDriver> prepared = new List<TrafficV2VehicleDriver>();
        private readonly List<TrafficV2VehicleDriver> actorsInFrame = new List<TrafficV2VehicleDriver>();
        private readonly List<JunctionActorReport> reports = new List<JunctionActorReport>();
        // Story 5.42 (M4) : demandes d'exception emises par les pilotes au pas N, soumises au lot de N.
        private readonly List<Policy.RuleExceptionRequest> ruleRequests = new List<Policy.RuleExceptionRequest>();
        private JunctionCoordinator coordinator;
        private SignalPhaseController signals;
        private readonly List<TrafficActorInput> inputs = new List<TrafficActorInput>();
        private readonly List<HazardQuery> queries = new List<HazardQuery>();
        private readonly HashSet<RoadId> actorIds = new HashSet<RoadId>();
        private readonly GridlockSupervisor gridlock = new GridlockSupervisor();
        private readonly Dictionary<string, RoadId> actorNames = new Dictionary<string, RoadId>(StringComparer.Ordinal);
        private readonly List<GridlockArc> waits = new List<GridlockArc>();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Stopwatch stepWatch = new Stopwatch();
        private readonly Stopwatch sectionWatch = new Stopwatch();

        private static readonly ProfilerMarker StepMarker = new ProfilerMarker("TrafficV2.Step");
        private static readonly ProfilerMarker PrepareMarker = new ProfilerMarker("TrafficV2.Prepare");
        private static readonly ProfilerMarker CollectMarker = new ProfilerMarker("TrafficV2.Collect");
        private static readonly ProfilerMarker FrameBuildMarker = new ProfilerMarker("TrafficV2.FrameBuild");
        private static readonly ProfilerMarker DriveMarker = new ProfilerMarker("TrafficV2.Drive");
        private static readonly ProfilerMarker CoordinateMarker = new ProfilerMarker("TrafficV2.Coordinate");

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
        /// <summary>Coordinateur de carrefour hote unique (5.34) ; nul avant le premier pas sur un modele.</summary>
        public JunctionCoordinator Coordinator { get { return coordinator; } }
        /// <summary>Horloge de phase des feux du modele (Story 5.36) ; nulle avant le premier pas sur un modele.</summary>
        public SignalPhaseController Signals { get { return signals; } }
        /// <summary>Dernier instantane publie : effectif au pas hote suivant (EffectiveFrame = FrameId + 1).</summary>
        public JunctionSnapshot JunctionSnapshot { get { return coordinator != null ? coordinator.Current : null; } }
        /// <summary>Superviseur d'interblocage (Story 5.40) : cycles de la derniere detection.</summary>
        public GridlockSupervisor Gridlock { get { return gridlock; } }

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
            // Un coordinateur par modele ; son instantane initial, vide, est effectif a ce pas.
            if (coordinator == null || coordinator.Model != model) coordinator = new JunctionCoordinator(model, FrameId);
            if (signals == null || signals.Model != model) signals = new SignalPhaseController(model);
            var snapshot = coordinator.Current;
            ordered.Clear();
            foreach (var driver in drivers)
                if (driver != null && driver.IsBound) ordered.Add(driver);
            ordered.Sort((a, b) => a.TrafficId.CompareTo(b.TrafficId));

            inputs.Clear(); prepared.Clear(); queries.Clear(); actorIds.Clear(); actorsInFrame.Clear(); ruleRequests.Clear();
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
                actorsInFrame.Add(ordered[i]);
                if (!ordered[i].StepPrepared) continue;
                prepared.Add(ordered[i]);
                queries.Add(ordered[i].HazardQuery);
            }
            PrepareMarker.End();
            cost.PrepareMilliseconds = sectionWatch.Elapsed.TotalMilliseconds;
            cost.PrepareBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
            if (inputs.Count == 0)
            {
                // Aucun acteur : lot valide vide, les grants des absents sont revoques (ActorGone) ; aucun cycle (5.40).
                gridlock.Detect(FrameId, null);
                Coordinate(null, snapshot, ref cost);
                return;
            }

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
                frame = new TrafficFrame(FrameId, model, inputs, hazards, signals.Current);
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
                prepared[i].Step(FrameId, frame, report, collector.Counters, share, snapshot, coordinator.Index);
            }
            DriveMarker.End();
            cost.DriveMilliseconds = sectionWatch.Elapsed.TotalMilliseconds;
            cost.DriveBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
            cost.Vehicles = prepared.Count;
            LastSteppedCount = prepared.Count;

            // Story 5.40 : graphe d'attente des pas de N, cycles soumis au lot de N.
            var cycles = DetectGridlocks(frame);

            // Story 5.42 (M4) : demandes d'exception du pas N, seulement celles emises a cette frame.
            for (int i = 0; i < prepared.Count; i++)
            {
                var request = prepared[i].PendingRuleExceptionRequest;
                if (request != null && request.SourceFrame == FrameId) ruleRequests.Add(request);
            }

            // Resolution des demandes de N apres tous les pas : instantane effectif a N+1 seulement.
            Coordinate(frame, snapshot, ref cost, cycles);
            gridlock.Report(coordinator.Current);
        }

        /// <summary>Story 5.40 (G1-G2) : arcs des vehicules ayant recu un pas a N, entre acteurs de la frame N.</summary>
        private IReadOnlyList<GridlockCycle> DetectGridlocks(TrafficFrame frame)
        {
            waits.Clear();
            bool held = false;
            for (int i = 0; frame != null && i < prepared.Count && !held; i++) held = prepared[i].Blockers.Count > 0;
            // Frame refusee ou aucun vehicule tenu : aucun arc, donc aucun cycle, sans allocation.
            if (!held) return gridlock.Detect(FrameId, waits);
            actorNames.Clear();
            foreach (var id in actorIds) actorNames[id.ToString()] = id;
            for (int i = 0; i < prepared.Count; i++)
            {
                var driver = prepared[i];
                var report = driver.LastJunctionReportFrameId == FrameId ? driver.LastJunctionReport : null;
                GridlockSupervisor.WaitsOf(driver.TrafficId, driver.Blockers, report, actorNames, waits);
            }
            return gridlock.Detect(FrameId, waits);
        }

        /// <summary>Lot du coordinateur, rapports d'occupation sans pas compris ; fail-closed si la frame a ete refusee.</summary>
        private void Coordinate(TrafficFrame frame, JunctionSnapshot snapshot, ref TrafficV2StepCost cost,
            IReadOnlyList<GridlockCycle> cycles = null)
        {
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            sectionWatch.Restart();
            CoordinateMarker.Begin();
            reports.Clear();
            if (frame != null)
                for (int i = 0; i < actorsInFrame.Count; i++)
                {
                    var driver = actorsInFrame[i];
                    var report = driver.LastJunctionReportFrameId == FrameId && driver.LastJunctionReport != null
                        ? driver.LastJunctionReport : driver.BuildOccupancyReport(frame, coordinator.Index, snapshot, FrameId);
                    if (report != null) reports.Add(report);
                }
            bool refused = frame == null && inputs.Count > 0;
            if (refused) coordinator.ResolveUnavailableFrame(FrameId);
            else if (ruleRequests.Count == 0) coordinator.Resolve(FrameId, reports, frame, cycles);
            // Story 5.42 (M4) : les demandes d'exception du pas N entrent dans le meme lot.
            else coordinator.Resolve(FrameId, reports, frame, cycles, ruleRequests);
            CoordinateMarker.End();
            cost.CoordinatorMilliseconds = sectionWatch.Elapsed.TotalMilliseconds;
            cost.CoordinatorBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
            // Story 5.36 : apres le lot de N, un pas fixe ; la frame N+1 porte la phase suivante.
            signals.Advance(UnityEngine.Time.fixedDeltaTime);
        }
    }
}
