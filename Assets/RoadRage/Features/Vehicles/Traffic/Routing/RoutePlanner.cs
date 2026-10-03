using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;

namespace RoadRage.Features.Vehicles.Traffic.Routing
{
    /// <summary>Planification pure sur les liens diriges du modele compile.</summary>
    public static class RoutePlanner
    {
        private sealed class Edge
        {
            public RoadId Id;
            public RoadId To;
            public RoadElementKind Kind;
            public float MovementLength;
            public float Weight;
            public double Preference;
            public bool ZeroFallback;
            public bool Feasible;
        }

        private sealed class Node
        {
            public RoadId Id;
            public float Length;
            public readonly List<Edge> Edges = new List<Edge>();
            public readonly List<Portal> Portals = new List<Portal>();
            public double Cost = double.PositiveInfinity;
            public Edge Next;
            public Portal? Exit;
        }

        /// <summary>
        /// Planifie vers une sortie du modele par la seule topologie dirigee. Le cout publie combine
        /// distance dirigee restante et cout de preference issu du tirage deterministe (graine de
        /// session, identite trafic, domaine et compteur de decision). Pure : aucun effet de conduite,
        /// de cycle de vie ou de mutation du modele ; `InvalidStart` couvre aussi les fautes d'appel
        /// (modele nul, identite de trafic vide, domaine vide).
        /// </summary>
        public static RouteResult Plan(RouteRequest request)
        {
            // Objectif intermediaire (contrat §4) : garde jusqu'au franchissement. Sur le mouvement vise lui-meme,
            // la traversee est en cours : le reste de la route est une route ordinaire vers la sortie.
            var at = request.Location;
            bool onObjective = at.Localized && at.ElementKind == RoadElementKind.JunctionMovement
                && at.ElementId == request.ViaMovementId;
            if (!request.ViaMovementId.IsEmpty && !onObjective) return PlanThroughMovement(request);
            ObjectivePhase ignored;
            return Core(request, RoadId.None, out ignored);
        }

        /// <summary>Premiere phase d'une route a objectif : occurrences jusqu'au mouvement vise inclus.</summary>
        private sealed class ObjectivePhase
        {
            public List<RouteOccurrence> Occurrences;
            public double Distance;
            public double Preference;
            public RouteDiagnostic Diagnostics;
        }

        /// <summary>
        /// Route a objectif intermediaire, semantique par phases du contrat §4 : phase 1 jusqu'au mouvement vise
        /// (inclus, cout et preference comptes une fois), phase 2 de la fin du mouvement a la sortie, chacune sous
        /// les regles 5.29 inchangees avec son propre objectif. Les phases ne partagent ni etat ni contrainte et le
        /// cout est additif : le minimum de chaque phase donne le minimum global (completude).
        /// </summary>
        private static RouteResult PlanThroughMovement(RouteRequest request)
        {
            var model = request.Model;
            CompiledJunctionMovement objective;
            if (model == null || !model.TryGetMovement(request.ViaMovementId, out objective))
                return new RouteResult(RouteOutcome.InvalidInput, RouteReason.ObjectiveUnknown, null);

            var existing = request.Existing;
            HashSet<RoadId> closed = null;
            if (request.ClosedPortalIds != null && request.ClosedPortalIds.Count > 0) closed = new HashSet<RoadId>(request.ClosedPortalIds);
            bool stale = false;
            if (existing != null)
            {
                RoutePlan reused = null;
                // Une progression au-dela de l'occurrence visee prouve le franchissement : le plan reste valide.
                bool reusable = existing.ViaMovementId == request.ViaMovementId && existing.ViaOccurrenceIndex >= 0
                    && !IsClosed(existing.ExitPortalId, closed)
                    && TryReuse(existing, model, request.Location, request.DestinationExitId, request.TrafficId, out reused);
                if (reusable && !request.Replan) return new RouteResult(RouteOutcome.Planned, RouteReason.Requested, reused);
                stale = !reusable;
            }

            ObjectivePhase phase;
            var first = Core(new RouteRequest(model, request.Location, request.DestinationExitId, request.Seed, request.TrafficId,
                request.DecisionDomain, request.Counter, null, false, request.ClosedPortalIds), request.ViaMovementId, out phase);
            if (first.Outcome == RouteOutcome.InvalidInput) return first;
            if (phase == null) return new RouteResult(RouteOutcome.NoRoute, RouteReason.NoRouteToObjective, null);

            var departure = new RoadLocation { ModelId = model.ModelId, ModelVersion = model.Version, Localized = true,
                ElementKind = RoadElementKind.LaneCorridor, ElementId = objective.ToCorridorId, SMeters = 0f };
            ObjectivePhase ignored;
            var second = Core(new RouteRequest(model, departure, request.DestinationExitId, request.Seed, request.TrafficId,
                request.DecisionDomain, request.Counter, null, false, request.ClosedPortalIds), RoadId.None, out ignored);
            if (second.Plan == null) return new RouteResult(RouteOutcome.NoRoute, RouteReason.NoRouteAfterObjective, null);

            var occurrences = new List<RouteOccurrence>(phase.Occurrences);
            int viaIndex = occurrences.Count - 1;
            occurrences.AddRange(second.Plan.Occurrences);
            var reason = stale ? RouteReason.StalePlan : RouteReason.Requested;
            var plan = new RoutePlan(model.ModelId, model.Version, request.TrafficId, second.Plan.ExitPortalId, reason, occurrences,
                phase.Distance + second.Plan.DistanceMeters, phase.Preference + second.Plan.PreferenceCost,
                phase.Diagnostics | second.Plan.Diagnostics, request.ViaMovementId, viaIndex);
            return new RouteResult(existing != null || request.Replan ? RouteOutcome.Replanned : RouteOutcome.Planned, reason, plan);
        }

        /// <param name="objective">Vide : route vers une sortie. Sinon : phase 1, jusqu'a ce mouvement inclus (sans sortie).</param>
        private static RouteResult Core(RouteRequest request, RoadId objective, out ObjectivePhase phase)
        {
            phase = null;
            bool toObjective = !objective.IsEmpty;
            RoadId objectiveFrom = RoadId.None;
            if (toObjective)
            {
                CompiledJunctionMovement target;
                if (request.Model != null && request.Model.TryGetMovement(objective, out target)) objectiveFrom = target.FromCorridorId;
            }
            var model = request.Model;
            var location = request.Location;
            var destinationExitId = request.DestinationExitId;
            ulong sessionSeed = request.Seed.Value;
            RoadId trafficId = request.TrafficId;
            string decisionDomain = request.DecisionDomain;
            ulong decisionCounter = request.Counter.Value;
            RoutePlan existing = request.Existing;
            bool replan = request.Replan;
            IReadOnlyCollection<RoadId> closedPortalIds = request.ClosedPortalIds;

            HashSet<RoadId> closedPortals = null;
            if (closedPortalIds != null && closedPortalIds.Count > 0)
            {
                closedPortals = new HashSet<RoadId>();
                foreach (var closedId in closedPortalIds) closedPortals.Add(closedId);
            }

            if (model == null || trafficId.IsEmpty || string.IsNullOrEmpty(decisionDomain))
                return new RouteResult(RouteOutcome.InvalidInput, RouteReason.InvalidStart, null);

            if (location.ModelId != model.ModelId || location.ModelVersion != model.Version)
                return new RouteResult(RouteOutcome.InvalidInput, RouteReason.StaleLocalization, null);

            float startLength;
            CompiledJunctionMovement startMovement = default(CompiledJunctionMovement);
            EffectiveLaneCorridor startCorridor;
            if (!location.Localized || location.ElementKind == RoadElementKind.None || location.ElementId.IsEmpty)
                return new RouteResult(RouteOutcome.InvalidInput, RouteReason.InvalidStart, null);
            if (location.ElementKind == RoadElementKind.LaneCorridor && model.TryGetCorridor(location.ElementId, out startCorridor))
                startLength = startCorridor.LengthMeters;
            else if (location.ElementKind == RoadElementKind.JunctionMovement && model.TryGetMovement(location.ElementId, out startMovement))
                startLength = startMovement.LengthMeters;
            else
                return new RouteResult(RouteOutcome.InvalidInput, RouteReason.InvalidStart, null);
            if (float.IsNaN(location.SMeters) || float.IsInfinity(location.SMeters)
                || location.SMeters < 0f || location.SMeters > startLength)
                return new RouteResult(RouteOutcome.InvalidInput, RouteReason.InvalidStart, null);

            var portals = new List<Portal>();
            for (int i = 0; i < model.Portals.Count; i++)
            {
                var portal = model.Portals[i];
                EffectiveLaneCorridor portalCorridor;
                if (portal.Role == PortalRole.Exit && (destinationExitId.IsEmpty || portal.Id == destinationExitId)
                    && !IsClosed(portal.Id, closedPortals)
                    && model.TryGetCorridor(portal.CorridorId, out portalCorridor)
                    && portal.SMeters <= portalCorridor.LengthMeters)
                    portals.Add(portal);
            }
            if (portals.Count == 0 && !toObjective)
                return new RouteResult(RouteOutcome.NoRoute, RouteReason.DestinationUnavailable, null);

            RoutePlan reused = null;
            bool stale = existing != null && (IsClosed(existing.ExitPortalId, closedPortals)
                || !TryReuse(existing, model, location, destinationExitId, trafficId, out reused));
            if (existing != null && !stale && !replan)
                return new RouteResult(RouteOutcome.Planned, RouteReason.Requested, reused);

            TrafficV2WorkCounters.Work.RouteSearches++;
            var nodes = new Dictionary<RoadId, Node>();
            for (int i = 0; i < model.Corridors.Count; i++)
            {
                var corridor = model.Corridors[i];
                nodes.Add(corridor.CorridorId, new Node { Id = corridor.CorridorId, Length = corridor.LengthMeters });
            }
            for (int i = 0; i < model.Connections.Count; i++)
            {
                var connection = model.Connections[i];
                nodes[connection.FromCorridorId].Edges.Add(new Edge {
                    Id = connection.Id, To = connection.ToCorridorId,
                    Kind = RoadElementKind.None, Weight = 1f });
            }
            for (int i = 0; i < model.Movements.Count; i++)
            {
                var movement = model.Movements[i];
                nodes[movement.FromCorridorId].Edges.Add(new Edge {
                    Id = movement.Id, To = movement.ToCorridorId,
                    Kind = RoadElementKind.JunctionMovement, MovementLength = movement.LengthMeters,
                    Weight = movement.RoutePreferenceWeight });
            }
            // Phase 1 d'un objectif : aucune sortie avant le mouvement vise.
            if (!toObjective)
                for (int i = 0; i < portals.Count; i++)
                    nodes[portals[i].CorridorId].Portals.Add(portals[i]);

            var ordered = new List<Node>(nodes.Values);
            ordered.Sort((a, b) => a.Id.CompareTo(b.Id));
            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].Edges.Sort((a, b) => a.Id.CompareTo(b.Id));
                ordered[i].Portals.Sort((a, b) => a.Id.CompareTo(b.Id));
            }

            // Une continuation ne compte que si son suffixe atteint la sortie sans revenir au
            // meme etat d'entree. Sinon un cycle positif masquerait une sortie a poids nul.
            // Le graphe est fige : la faisabilite est calculee une seule fois par arete, puis
            // partagee par les deux passes.
            for (int i = 0; i < ordered.Count; i++)
            {
                var node = ordered[i];
                for (int e = 0; e < node.Edges.Count; e++)
                    node.Edges[e].Feasible = (toObjective && node.Edges[e].Id == objective)
                        || CanReachWithout(nodes, node.Edges[e].To, node.Id, objectiveFrom);
            }
            for (int i = 0; i < ordered.Count; i++)
            {
                var node = ordered[i];
                bool hasPositive = false;
                int feasible = 0;
                for (int e = 0; e < node.Edges.Count; e++)
                {
                    if (!node.Edges[e].Feasible) continue;
                    feasible++;
                    hasPositive |= node.Edges[e].Weight > 0f;
                }
                for (int e = 0; e < node.Edges.Count; e++)
                {
                    var edge = node.Edges[e];
                    if (!edge.Feasible || (hasPositive && edge.Weight == 0f))
                    {
                        edge.Preference = double.PositiveInfinity;
                        continue;
                    }
                    edge.ZeroFallback = !hasPositive && feasible > 0;
                    edge.Preference = feasible <= 1 ? 0d : -Math.Log(UnitDraw(sessionSeed, trafficId, decisionDomain, decisionCounter, edge.Id))
                        / (hasPositive ? edge.Weight : 1d);
                }
            }

            for (int i = 0; i < ordered.Count; i++)
                for (int p = 0; p < ordered[i].Portals.Count; p++)
                    ConsiderPortal(ordered[i], ordered[i].Portals[p]);

            // Bellman-Ford sur couts non negatifs ; les cycles ne peuvent jamais ameliorer un chemin.
            for (int pass = 0; pass < ordered.Count; pass++)
            {
                bool changed = false;
                for (int i = 0; i < ordered.Count; i++)
                {
                    var node = ordered[i];
                    for (int e = 0; e < node.Edges.Count; e++)
                    {
                        var edge = node.Edges[e];
                        double cost = node.Length + edge.MovementLength + edge.Preference + Tail(nodes, edge, objective);
                        if (cost < node.Cost)
                        {
                            node.Cost = cost;
                            node.Next = edge;
                            node.Exit = null;
                            changed = true;
                        }
                    }
                }
                if (!changed) break;
            }

            var occurrences = new List<RouteOccurrence>();
            double distance = 0d;
            double preference = 0d;
            RouteDiagnostic diagnostics = RouteDiagnostic.None;
            Node current;
            float s;
            if (location.ElementKind == RoadElementKind.JunctionMovement)
            {
                current = nodes[startMovement.ToCorridorId];
                if (double.IsInfinity(current.Cost))
                    return new RouteResult(RouteOutcome.NoRoute, RouteReason.DestinationUnreachable, null);
                occurrences.Add(new RouteOccurrence(RoadElementKind.JunctionMovement, location.ElementId,
                    location.SMeters, startMovement.LengthMeters));
                distance += startMovement.LengthMeters - location.SMeters;
                s = 0f;
            }
            else
            {
                current = nodes[location.ElementId];
                s = location.SMeters;
            }

            // Premiere visite partielle : le portail doit etre strictement devant s.
            Portal? direct = null;
            double best = double.PositiveInfinity;
            for (int p = 0; p < current.Portals.Count; p++)
            {
                var portal = current.Portals[p];
                if (portal.SMeters > s && portal.SMeters - s < best)
                {
                    direct = portal;
                    best = portal.SMeters - s;
                }
            }
            Edge firstEdge = null;
            double firstPreference = 0d;
            bool firstZeroFallback = false;
            bool partialStart = s > 0f;
            bool firstHasPositive = false;
            int firstFeasible = 0;
            for (int e = 0; e < current.Edges.Count; e++)
            {
                var edge = current.Edges[e];
                if (partialStart ? !double.IsInfinity(Tail(nodes, edge, objective)) : !double.IsInfinity(edge.Preference))
                {
                    firstFeasible++;
                    firstHasPositive |= edge.Weight > 0f;
                }
            }
            for (int e = 0; e < current.Edges.Count; e++)
            {
                var edge = current.Edges[e];
                bool feasibleEdge = partialStart ? !double.IsInfinity(Tail(nodes, edge, objective))
                    : !double.IsInfinity(edge.Preference);
                if (!feasibleEdge || (firstHasPositive && edge.Weight == 0f)) continue;
                double edgePreference = partialStart
                    ? (firstFeasible <= 1 ? 0d : -Math.Log(UnitDraw(sessionSeed, trafficId,
                        decisionDomain, decisionCounter, edge.Id)) / (firstHasPositive ? edge.Weight : 1d))
                    : edge.Preference;
                double cost = current.Length - s + edge.MovementLength + edgePreference + Tail(nodes, edge, objective);
                if (cost < best)
                {
                    best = cost;
                    firstEdge = edge;
                    firstPreference = edgePreference;
                    firstZeroFallback = !firstHasPositive && firstFeasible > 0;
                    direct = null;
                }
            }
            if (double.IsInfinity(best))
                return new RouteResult(RouteOutcome.NoRoute, RouteReason.DestinationUnreachable, null);

            Edge selected = firstEdge;
            double selectedPreference = firstPreference;
            bool selectedZeroFallback = firstZeroFallback;
            for (int visits = 0; visits <= ordered.Count; visits++)
            {
                if (selected == null)
                {
                    Portal portal = direct ?? current.Exit.Value;
                    occurrences.Add(new RouteOccurrence(RoadElementKind.LaneCorridor, current.Id, s, portal.SMeters));
                    distance += portal.SMeters - s;
                    var reason = stale ? RouteReason.StalePlan : RouteReason.Requested;
                    var plan = new RoutePlan(model.ModelId, model.Version, trafficId, portal.Id, reason, occurrences,
                        distance, preference, diagnostics);
                    return new RouteResult(existing != null || replan ? RouteOutcome.Replanned : RouteOutcome.Planned,
                        reason, plan);
                }
                occurrences.Add(new RouteOccurrence(RoadElementKind.LaneCorridor, current.Id, s, current.Length));
                distance += current.Length - s;
                preference += selectedPreference;
                if (selectedZeroFallback) diagnostics |= RouteDiagnostic.ZeroWeightFallback;
                if (selected.Kind == RoadElementKind.JunctionMovement)
                {
                    occurrences.Add(new RouteOccurrence(RoadElementKind.JunctionMovement, selected.Id, 0f, selected.MovementLength));
                    distance += selected.MovementLength;
                }
                if (toObjective && selected.Id == objective)
                {
                    // Fin de la phase 1 : le mouvement vise est traverse sur toute sa longueur.
                    phase = new ObjectivePhase { Occurrences = occurrences, Distance = distance, Preference = preference,
                        Diagnostics = diagnostics };
                    return new RouteResult(RouteOutcome.Planned, RouteReason.Requested, null);
                }
                current = nodes[selected.To];
                s = 0f;
                selected = current.Next;
                if (selected != null)
                {
                    selectedPreference = selected.Preference;
                    selectedZeroFallback = selected.ZeroFallback;
                }
                direct = null;
            }
            return new RouteResult(RouteOutcome.NoRoute, RouteReason.DestinationUnreachable, null);
        }

        private static void ConsiderPortal(Node node, Portal portal)
        {
            if (portal.SMeters > 0f && portal.SMeters < node.Cost)
            {
                node.Cost = portal.SMeters;
                node.Exit = portal;
                node.Next = null;
            }
        }

        /// <summary>Cout restant apres une arete ; nul apres le mouvement vise, qui termine la phase 1.</summary>
        private static double Tail(Dictionary<RoadId, Node> nodes, Edge edge, RoadId objective)
        {
            return !objective.IsEmpty && edge.Id == objective ? 0d : nodes[edge.To].Cost;
        }

        /// <param name="objectiveFrom">Vide : atteindre une sortie ; sinon atteindre ce corridor (d'ou part le mouvement vise).</param>
        private static bool CanReachWithout(Dictionary<RoadId, Node> nodes, RoadId start, RoadId forbidden, RoadId objectiveFrom)
        {
            var seen = new HashSet<RoadId> { forbidden };
            var stack = new Stack<RoadId>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                RoadId id = stack.Pop();
                if (!seen.Add(id)) continue;
                if (!objectiveFrom.IsEmpty && id == objectiveFrom) return true;
                var node = nodes[id];
                for (int p = 0; p < node.Portals.Count; p++)
                    if (node.Portals[p].SMeters > 0f) return true;
                for (int e = 0; e < node.Edges.Count; e++)
                    stack.Push(node.Edges[e].To);
            }
            return false;
        }

        private static bool IsClosed(RoadId portalId, HashSet<RoadId> closedPortals)
        {
            return closedPortals != null && closedPortals.Contains(portalId);
        }

        private static bool TryReuse(RoutePlan plan, CompiledRoadModel model, RoadLocation location,
            RoadId destination, RoadId trafficId, out RoutePlan reused)
        {
            reused = null;
            if (plan.ModelId != model.ModelId || plan.ModelVersion != model.Version || plan.Occurrences.Count == 0
                || plan.TrafficId != trafficId || (!destination.IsEmpty && plan.ExitPortalId != destination)) return false;
            if (plan.ProgressOccurrenceIndex < 0 || plan.ProgressOccurrenceIndex >= plan.Occurrences.Count)
                return false;
            // Une identite repetee est une boucle legale, pas une raison de refuser le plan :
            // l'occurrence courante est la premiere qui contient `s`, en partant de la progression
            // acquise. A progression egale, `s` ne recule jamais ; une occurrence ulterieure du meme
            // element est une nouvelle visite (retour de giratoire), jamais un saut arriere.
            // La progression n'avance que d'une occurrence contigue (2026-09-30) : une visite ulterieure
            // non parcourue n'est jamais atteinte par saut, la localisation qui la designe rend le plan perime.
            // `s` et les bornes sont des donnees stockees re-emises telles quelles : l'egalite
            // exacte est le contrat, une valeur quantifiee en amont doit produire un nouveau plan.
            int match = -1;
            int reachable = Math.Min(plan.Occurrences.Count - 1, plan.ProgressOccurrenceIndex + 1);
            for (int i = plan.ProgressOccurrenceIndex; i <= reachable; i++)
            {
                var occurrence = plan.Occurrences[i];
                if (occurrence.Kind != location.ElementKind || occurrence.Id != location.ElementId) continue;
                if (location.SMeters < occurrence.StartSMeters || location.SMeters > occurrence.EndSMeters) continue;
                if (i == plan.ProgressOccurrenceIndex && location.SMeters < plan.ProgressSMeters) continue;
                match = i;
                break;
            }
            if (match < 0) return false;
            Portal exit = default(Portal);
            bool found = false;
            for (int i = 0; i < model.Portals.Count; i++)
                if (model.Portals[i].Id == plan.ExitPortalId && model.Portals[i].Role == PortalRole.Exit)
                {
                    exit = model.Portals[i];
                    found = true;
                    break;
                }
            if (!found) return false;
            for (int i = 0; i < plan.Occurrences.Count; i++)
            {
                var occurrence = plan.Occurrences[i];
                float length;
                EffectiveLaneCorridor corridor;
                CompiledJunctionMovement movement = default(CompiledJunctionMovement);
                if (occurrence.Kind == RoadElementKind.LaneCorridor && model.TryGetCorridor(occurrence.Id, out corridor))
                    length = corridor.LengthMeters;
                else if (occurrence.Kind == RoadElementKind.JunctionMovement && model.TryGetMovement(occurrence.Id, out movement))
                    length = movement.LengthMeters;
                else return false;
                if (occurrence.StartSMeters < 0f || occurrence.EndSMeters > length
                    || occurrence.EndSMeters < occurrence.StartSMeters) return false;
                if (i + 1 < plan.Occurrences.Count && occurrence.EndSMeters != length) return false;
                if (i + 1 < plan.Occurrences.Count)
                {
                    var next = plan.Occurrences[i + 1];
                    if (next.StartSMeters != 0f) return false;
                    if (occurrence.Kind == RoadElementKind.JunctionMovement)
                    {
                        if (next.Kind != RoadElementKind.LaneCorridor || next.Id != movement.ToCorridorId) return false;
                    }
                    else if (next.Kind == RoadElementKind.JunctionMovement)
                    {
                        if (!model.TryGetMovement(next.Id, out movement) || movement.FromCorridorId != occurrence.Id)
                            return false;
                    }
                    else if (next.Kind == RoadElementKind.LaneCorridor)
                    {
                        bool connected = false;
                        for (int c = 0; c < model.Connections.Count; c++)
                            connected |= model.Connections[c].FromCorridorId == occurrence.Id
                                && model.Connections[c].ToCorridorId == next.Id;
                        if (!connected) return false;
                    }
                    else return false;
                }
            }
            var last = plan.Occurrences[plan.Occurrences.Count - 1];
            if (last.Kind != RoadElementKind.LaneCorridor || last.Id != exit.CorridorId
                || last.EndSMeters != exit.SMeters || last.EndSMeters <= last.StartSMeters) return false;
            reused = plan.Advance(match, location.SMeters);
            return true;
        }

        private static double UnitDraw(ulong seed, RoadId trafficId, string domain, ulong counter, RoadId edgeId)
        {
            ulong hash = 14695981039346656037UL;
            Mix(ref hash, seed);
            Mix(ref hash, trafficId.High);
            Mix(ref hash, trafficId.Low);
            Mix(ref hash, counter);
            for (int i = 0; i < domain.Length; i++) Mix(ref hash, domain[i]);
            Mix(ref hash, edgeId.High);
            Mix(ref hash, edgeId.Low);
            unchecked
            {
                hash ^= hash >> 30;
                hash *= 0xbf58476d1ce4e5b9UL;
                hash ^= hash >> 27;
                hash *= 0x94d049bb133111ebUL;
                hash ^= hash >> 31;
            }
            return ((hash >> 11) + 1d) / 9007199254740993d;
        }

        private static void Mix(ref ulong hash, ulong value)
        {
            unchecked
            {
                for (int i = 0; i < 8; i++)
                {
                    hash ^= (byte)(value >> (8 * i));
                    hash *= 1099511628211UL;
                }
            }
        }
    }
}
