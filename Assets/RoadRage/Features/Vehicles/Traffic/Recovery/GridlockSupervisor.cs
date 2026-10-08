using System;
using System.Collections.Generic;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Recovery
{
    /// <summary>
    /// Superviseur d'interblocage hote (Story 5.40). Il construit a chaque pas le graphe d'attente depuis les blockers et les
    /// rapports du coordinateur (G1), en extrait les cycles (G2), les journalise en build de developpement (G3) et les soumet au
    /// coordinateur, seul a decider une escalade. Il n'accorde aucun grant, ne compose aucun intent, ne lit ni ne modifie aucune
    /// regle et ne retire ni ne deplace aucun vehicule. Sa seule memoire est celle du journal : la detection n'en a aucune.
    /// </summary>
    public sealed class GridlockSupervisor
    {
        public const string LogPrefix = "[Gridlock] ";

        private readonly Action<string> log;
        private readonly HashSet<string> detected = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> exhausted = new HashSet<string>(StringComparer.Ordinal);
        private static readonly IReadOnlyList<GridlockCycle> NoCycles = Array.AsReadOnly(new GridlockCycle[0]);

        /// <param name="log">Puits du journal ; nul : Debug.Log, en build de developpement seulement.</param>
        public GridlockSupervisor(Action<string> log = null)
        {
            this.log = log ?? DevelopmentLog;
        }

        /// <summary>Cycles de la derniere detection, tries par cle.</summary>
        public IReadOnlyList<GridlockCycle> Cycles { get; private set; } = NoCycles;

        /// <summary>
        /// G1 : arcs d'un acteur tenu. Un blocker qui nomme un acteur present (Leader, JunctionGrant cause acteur, Obstacle) donne un
        /// arc vers lui ; BlockedExit donne un arc vers l'occupant de sortie quand la sortie est bornee par un occupant present.
        /// </summary>
        /// <param name="actors">Acteurs de la frame, par RoadId.ToString().</param>
        public static void WaitsOf(RoadId self, IReadOnlyList<Blocker> blockers, JunctionActorReport report,
            IReadOnlyDictionary<string, RoadId> actors, List<GridlockArc> into)
        {
            if (blockers == null || actors == null || into == null) return;
            for (int i = 0; i < blockers.Count; i++)
            {
                var blocker = blockers[i];
                RoadId target;
                if (blocker.Kind == BlockerKind.BlockedExit)
                {
                    if (report == null || !report.HasRequest || report.Request.Exit.Bound != JunctionExitBound.Occupant) continue;
                    target = report.Request.Exit.BoundId;
                    if (!actors.ContainsKey(target.ToString())) continue;
                }
                else if (blocker.BlockingActorOrRule == null || !actors.TryGetValue(blocker.BlockingActorOrRule, out target)) continue;
                if (target != self) into.Add(new GridlockArc(self, target, blocker.Kind.ToString()));
            }
        }

        /// <summary>
        /// G2 : composantes fortement connexes de 2 membres ou plus (Tarjan), independantes de l'ordre des arcs, triees par cle.
        /// Aucun minuteur : seuls les arcs comptent.
        /// </summary>
        public static List<GridlockCycle> FindCycles(IReadOnlyList<GridlockArc> arcs)
        {
            var adjacency = new SortedDictionary<RoadId, List<RoadId>>();
            if (arcs != null)
                foreach (var arc in arcs)
                {
                    List<RoadId> next;
                    if (!adjacency.TryGetValue(arc.From, out next)) adjacency.Add(arc.From, next = new List<RoadId>());
                    if (!adjacency.ContainsKey(arc.To)) adjacency.Add(arc.To, new List<RoadId>());
                    if (!next.Contains(arc.To)) next.Add(arc.To);
                }
            foreach (var list in adjacency.Values) list.Sort();

            var index = new Dictionary<RoadId, int>();
            var low = new Dictionary<RoadId, int>();
            var stack = new Stack<RoadId>();
            var onStack = new HashSet<RoadId>();
            var components = new List<List<RoadId>>();
            int counter = 0;
            foreach (var node in adjacency.Keys)
                if (!index.ContainsKey(node)) Connect(node, adjacency, index, low, stack, onStack, components, ref counter);

            var cycles = new List<GridlockCycle>();
            foreach (var component in components)
            {
                if (component.Count < 2) continue;
                var members = new HashSet<RoadId>(component);
                var inner = new List<GridlockArc>();
                foreach (var arc in arcs)
                    if (members.Contains(arc.From) && members.Contains(arc.To)) inner.Add(arc);
                cycles.Add(new GridlockCycle(component, inner));
            }
            cycles.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return cycles;
        }

        /// <summary>Detection du pas et journal G3 d'un nouveau cycle (une fois tant qu'il reste detecte).</summary>
        public IReadOnlyList<GridlockCycle> Detect(ulong frameId, IReadOnlyList<GridlockArc> arcs)
        {
            // Aucun vehicule tenu (cas courant) : aucun cycle, memoire du journal videe, aucune allocation.
            if (arcs == null || arcs.Count == 0)
            {
                detected.Clear();
                exhausted.Clear();
                return Cycles = NoCycles;
            }
            var cycles = FindCycles(arcs);
            var present = new HashSet<string>(StringComparer.Ordinal);
            foreach (var cycle in cycles)
            {
                present.Add(cycle.Key);
                if (detected.Add(cycle.Key)) log(LogPrefix + Frame(frameId) + "interblocage detecte " + cycle.ToText());
            }
            detected.IntersectWith(present);
            exhausted.IntersectWith(present);
            Cycles = cycles.AsReadOnly();
            return Cycles;
        }

        /// <summary>G3 : journal des resolutions du lot ; chaque escalade, et le premier Exhausted d'un cycle.</summary>
        public void Report(JunctionSnapshot snapshot)
        {
            if (snapshot == null) return;
            for (int i = 0; i < snapshot.Gridlocks.Count; i++)
            {
                var resolution = snapshot.Gridlocks[i];
                if (resolution.Outcome == GridlockOutcome.Escalated)
                    log(LogPrefix + Frame(snapshot.SourceFrame) + "escalade " + resolution.Tier + " @" + resolution.Served + " cycle {"
                        + resolution.Cycle.Key + "}");
                else if (resolution.Outcome == GridlockOutcome.Exhausted && exhausted.Add(resolution.Cycle.Key))
                    log(LogPrefix + Frame(snapshot.SourceFrame) + "paliers epuises, aucun retrait " + resolution.Cycle.ToText());
            }
        }

        private static void Connect(RoadId node, SortedDictionary<RoadId, List<RoadId>> adjacency, Dictionary<RoadId, int> index,
            Dictionary<RoadId, int> low, Stack<RoadId> stack, HashSet<RoadId> onStack, List<List<RoadId>> components, ref int counter)
        {
            index[node] = low[node] = counter++;
            stack.Push(node);
            onStack.Add(node);
            foreach (var next in adjacency[node])
            {
                if (!index.ContainsKey(next))
                {
                    Connect(next, adjacency, index, low, stack, onStack, components, ref counter);
                    low[node] = Math.Min(low[node], low[next]);
                }
                else if (onStack.Contains(next)) low[node] = Math.Min(low[node], index[next]);
            }
            if (low[node] != index[node]) return;
            var component = new List<RoadId>();
            RoadId member;
            do
            {
                member = stack.Pop();
                onStack.Remove(member);
                component.Add(member);
            } while (member != node);
            components.Add(component);
        }

        private static string Frame(ulong frameId)
        {
            return "frame " + frameId.ToString(CultureInfo.InvariantCulture) + " : ";
        }

        private static void DevelopmentLog(string message)
        {
            if (Debug.isDebugBuild) Debug.Log(message);
        }
    }
}
