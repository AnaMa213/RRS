using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;

namespace RoadRage.Features.Vehicles.Traffic.Coordination
{
    /// <summary>
    /// Index de coordination d'un modele compile (Story 5.34), construit une seule fois par modele et partage par tous
    /// les lots : carrefour de chaque mouvement, ensemble trie des mouvements incompatibles avec la zone en cause par
    /// paire, successeurs du meme carrefour (chaines de traversee), mouvements sortants d'un corridor (voisinage de sortie)
    /// et portails. Deux mouvements distincts sont incompatibles si et seulement s'ils appartiennent a une meme
    /// <see cref="CompiledConflictZone"/> ; un mouvement est compatible avec lui-meme. Aucune inference geometrique, de nom
    /// ou de cycle.
    /// </summary>
    public sealed class JunctionConflictIndex
    {
        private static readonly ConditionalWeakTable<CompiledRoadModel, JunctionConflictIndex> Cache =
            new ConditionalWeakTable<CompiledRoadModel, JunctionConflictIndex>();
        private static readonly RoadId[] NoIds = new RoadId[0];

        private sealed class Entry
        {
            public RoadId JunctionId, FromCorridorId, ToCorridorId;
            /// <summary>Mouvements incompatibles, tries ; <see cref="Zones"/> porte la zone en cause au meme rang.</summary>
            public RoadId[] Incompatible = NoIds, Zones = NoIds;
            public RoadId[] Successors = NoIds;
            public bool HasPredecessor;
        }

        private readonly Dictionary<RoadId, Entry> movements = new Dictionary<RoadId, Entry>();
        private readonly Dictionary<RoadId, RoadId[]> outgoing = new Dictionary<RoadId, RoadId[]>();
        private readonly Dictionary<RoadId, Portal> portals = new Dictionary<RoadId, Portal>();
        private readonly Dictionary<RoadId, RoadBoundsBox> boundaries = new Dictionary<RoadId, RoadBoundsBox>();

        public CompiledRoadModel Model { get; }
        /// <summary>Paires (ordonnees) de mouvements incompatibles : deux fois le nombre de paires non ordonnees.</summary>
        public int IncompatiblePairCount { get; }

        /// <summary>Index du modele, construit au premier appel puis reutilise (compteur JunctionIndexBuilds).</summary>
        public static JunctionConflictIndex For(CompiledRoadModel model)
        {
            if (model == null) throw new ArgumentNullException("model");
            return Cache.GetValue(model, m => new JunctionConflictIndex(m));
        }

        private JunctionConflictIndex(CompiledRoadModel model)
        {
            Model = model;
            TrafficV2WorkCounters.Work.JunctionIndexBuilds++;
            var byCorridor = new Dictionary<RoadId, List<RoadId>>();
            for (int i = 0; i < model.Movements.Count; i++)
            {
                var movement = model.Movements[i];
                movements[movement.Id] = new Entry { JunctionId = movement.JunctionId, FromCorridorId = movement.FromCorridorId,
                    ToCorridorId = movement.ToCorridorId };
                List<RoadId> bucket;
                if (!byCorridor.TryGetValue(movement.FromCorridorId, out bucket))
                    byCorridor.Add(movement.FromCorridorId, bucket = new List<RoadId>());
                bucket.Add(movement.Id);
            }
            foreach (var pair in byCorridor)
            {
                var values = pair.Value.ToArray();
                Array.Sort(values);
                outgoing.Add(pair.Key, values);
            }
            foreach (var pair in movements)
            {
                var successors = new List<RoadId>();
                foreach (var next in OutgoingMovements(pair.Value.ToCorridorId))
                    if (movements[next].JunctionId == pair.Value.JunctionId) successors.Add(next);
                pair.Value.Successors = successors.ToArray();
                foreach (var next in successors) movements[next].HasPredecessor = true;
            }

            // Zones par id croissant : la premiere zone qui reunit une paire est la plus petite, retenue comme zone en cause.
            var zones = new List<CompiledConflictZone>(model.ConflictZones);
            zones.Sort((a, b) => a.Id.CompareTo(b.Id));
            var pairs = new Dictionary<RoadId, SortedDictionary<RoadId, RoadId>>();
            foreach (var zone in zones)
                for (int a = 0; a < zone.MemberMovementIds.Count; a++)
                    for (int b = 0; b < zone.MemberMovementIds.Count; b++)
                    {
                        RoadId first = zone.MemberMovementIds[a], second = zone.MemberMovementIds[b];
                        if (first == second) continue;
                        SortedDictionary<RoadId, RoadId> partners;
                        if (!pairs.TryGetValue(first, out partners)) pairs.Add(first, partners = new SortedDictionary<RoadId, RoadId>());
                        if (!partners.ContainsKey(second)) partners.Add(second, zone.Id);
                    }
            int count = 0;
            foreach (var pair in pairs)
            {
                Entry entry;
                if (!movements.TryGetValue(pair.Key, out entry)) continue;
                entry.Incompatible = new RoadId[pair.Value.Count];
                entry.Zones = new RoadId[pair.Value.Count];
                int i = 0;
                foreach (var partner in pair.Value) { entry.Incompatible[i] = partner.Key; entry.Zones[i] = partner.Value; i++; }
                count += i;
            }
            IncompatiblePairCount = count;
            for (int i = 0; i < model.Portals.Count; i++) portals[model.Portals[i].Id] = model.Portals[i];
            for (int i = 0; i < model.Junctions.Count; i++) boundaries[model.Junctions[i].Id] = model.Junctions[i].Boundary;
        }

        public bool Contains(RoadId movementId) { return movements.ContainsKey(movementId); }

        /// <summary>Carrefour du mouvement ; None s'il est inconnu du modele.</summary>
        public RoadId JunctionOf(RoadId movementId)
        {
            Entry entry;
            return movements.TryGetValue(movementId, out entry) ? entry.JunctionId : RoadId.None;
        }

        /// <summary>Corridor de depart du mouvement ; None s'il est inconnu.</summary>
        public RoadId ToCorridorOf(RoadId movementId)
        {
            Entry entry;
            return movements.TryGetValue(movementId, out entry) ? entry.ToCorridorId : RoadId.None;
        }

        /// <summary>Corridor d'approche du mouvement ; None s'il est inconnu.</summary>
        public RoadId FromCorridorOf(RoadId movementId)
        {
            Entry entry;
            return movements.TryGetValue(movementId, out entry) ? entry.FromCorridorId : RoadId.None;
        }

        /// <summary>Mouvements incompatibles avec le mouvement, tries ; vide s'il est inconnu ou sans zone.</summary>
        public IReadOnlyList<RoadId> IncompatibleWith(RoadId movementId)
        {
            Entry entry;
            return Array.AsReadOnly(movements.TryGetValue(movementId, out entry) ? entry.Incompatible : NoIds);
        }

        /// <summary>
        /// Vrai si les deux mouvements sont distincts et membres d'une meme zone compilee ; la zone rendue est la plus petite
        /// qui les reunit. Recherche dichotomique dans l'ensemble trie du premier : cout independant de la taille du modele.
        /// </summary>
        public bool TryGetConflict(RoadId a, RoadId b, out RoadId zoneId)
        {
            TrafficV2WorkCounters.Work.JunctionPairChecks++;
            zoneId = RoadId.None;
            Entry entry;
            if (a == b || !movements.TryGetValue(a, out entry)) return false;
            int at = Array.BinarySearch(entry.Incompatible, b);
            if (at < 0) return false;
            zoneId = entry.Zones[at];
            return true;
        }

        /// <summary>Mouvements du meme carrefour qui partent du corridor de depart du mouvement (maillons de chaine).</summary>
        public IReadOnlyList<RoadId> SameJunctionSuccessors(RoadId movementId)
        {
            Entry entry;
            return Array.AsReadOnly(movements.TryGetValue(movementId, out entry) ? entry.Successors : NoIds);
        }

        /// <summary>Vrai si le corridor d'approche du mouvement n'est atteint par aucun mouvement du meme carrefour.</summary>
        public bool EntersJunction(RoadId movementId)
        {
            Entry entry;
            return movements.TryGetValue(movementId, out entry) && !entry.HasPredecessor;
        }

        /// <summary>Mouvements qui partent du corridor, tries ; vide si aucun.</summary>
        public IReadOnlyList<RoadId> OutgoingMovements(RoadId corridorId)
        {
            RoadId[] values;
            return Array.AsReadOnly(outgoing.TryGetValue(corridorId, out values) ? values : NoIds);
        }

        public bool TryGetPortal(RoadId portalId, out Portal portal) { return portals.TryGetValue(portalId, out portal); }

        /// <summary>
        /// Vrai si un des points est dans la frontiere du carrefour, en plan (x, z) : critere de liberation d'un titulaire non
        /// localise. La hauteur est ignoree, ce qui garde le grant plus longtemps, jamais moins.
        /// </summary>
        public bool InsideBoundary(RoadId junctionId, IReadOnlyList<UnityEngine.Vector3> points)
        {
            RoadBoundsBox box;
            if (points == null || !boundaries.TryGetValue(junctionId, out box)) return false;
            for (int i = 0; i < points.Count; i++)
                if (Math.Abs(points[i].x - box.Center.x) <= box.Extents.x && Math.Abs(points[i].z - box.Center.z) <= box.Extents.z)
                    return true;
            return false;
        }
    }
}
