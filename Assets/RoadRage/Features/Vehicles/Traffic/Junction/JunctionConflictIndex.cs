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
    /// et portails. Story 5.35 : controle et genre de chaque mouvement, frontiere de controle (s_line ou 0), longueur et
    /// courbure maximale, genre et debuts de contact de chaque paire en conflit, et preseance entre controles d'un meme
    /// carrefour (Priority avant Yield/Stop ; entre deux Uncontrolled, l'approche de droite), tout precalcule ici. Deux mouvements distincts sont incompatibles si et seulement s'ils appartiennent a une meme
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
            // Story 5.35 : par paire (meme rang que Incompatible), genre de la zone en cause et debuts de contact de soi et de l'autre.
            public ConflictKind[] Kinds = NoKinds;
            public float[] StartSelf = NoFloats, StartOther = NoFloats;
            public RoadId ControlId;
            public JunctionControlKind ControlKind;
            public float Boundary, Length, MaxCurvature;
        }

        private static readonly ConflictKind[] NoKinds = new ConflictKind[0];
        private static readonly float[] NoFloats = new float[0];

        private readonly Dictionary<RoadId, Entry> movements = new Dictionary<RoadId, Entry>();
        private readonly Dictionary<RoadId, RoadId[]> outgoing = new Dictionary<RoadId, RoadId[]>();
        private readonly Dictionary<RoadId, Portal> portals = new Dictionary<RoadId, Portal>();
        private readonly Dictionary<RoadId, RoadBoundsBox> boundaries = new Dictionary<RoadId, RoadBoundsBox>();
        /// <summary>Paires ordonnees (P, R) de controles d'un meme carrefour ou P a preseance sur R.</summary>
        private readonly HashSet<long> precedence = new HashSet<long>();
        private readonly Dictionary<RoadId, int> controlIndex = new Dictionary<RoadId, int>();
        // Faits statiques calcules au premier acces d'une chaine, puis partages par tous les lots du modele.
        private readonly Dictionary<JunctionTraversal, TraversalFacts> traversals = new Dictionary<JunctionTraversal, TraversalFacts>(new TraversalComparer());
        private readonly Dictionary<(TraversalFacts, TraversalFacts), TraversalPairFacts> traversalPairs = new Dictionary<(TraversalFacts, TraversalFacts), TraversalPairFacts>();

        private sealed class TraversalComparer : IEqualityComparer<JunctionTraversal>
        {
            public int GetHashCode(JunctionTraversal value) { return value.GeometryKeyHash; }
            public bool Equals(JunctionTraversal a, JunctionTraversal b)
            {
                if (ReferenceEquals(a, b)) return true;
                if (a == null || b == null || a.JunctionId != b.JunctionId || a.MovementIds.Count != b.MovementIds.Count) return false;
                for (int i = 0; i < a.MovementIds.Count; i++) if (a.MovementIds[i] != b.MovementIds[i]) return false;
                return true;
            }
        }

        private sealed class TraversalFacts
        {
            public float[] PrefixCurvature, Lengths;
        }

        internal readonly struct TraversalPairFacts
        {
            public readonly int LastRequest, FirstOther;
            public readonly RoadId Zone;
            public TraversalPairFacts(int lastRequest, int firstOther, RoadId zone)
            { LastRequest = lastRequest; FirstOther = firstOther; Zone = zone; }
        }

        private TraversalFacts FactsOf(JunctionTraversal traversal)
        {
            TraversalFacts facts;
            if (traversals.TryGetValue(traversal, out facts)) return facts;
            int count = traversal.MovementIds.Count;
            facts = new TraversalFacts { PrefixCurvature = new float[count], Lengths = new float[count] };
            float curvature = 0f;
            for (int i = 0; i < count; i++)
            {
                curvature = Math.Max(curvature, MaxCurvatureOf(traversal.MovementIds[i]));
                facts.PrefixCurvature[i] = curvature;
                facts.Lengths[i] = LengthOf(traversal.MovementIds[i]);
            }
            traversals.Add(traversal, facts);
            TrafficV2WorkCounters.Work.JunctionTraversalBuilds++;
            return facts;
        }

        internal void ClearanceOf(JunctionTraversal traversal, int last, out float movementLength, out float maxCurvature)
        {
            var facts = FactsOf(traversal);
            movementLength = facts.Lengths[last];
            maxCurvature = facts.PrefixCurvature[last];
        }

        internal TraversalPairFacts PairOf(JunctionTraversal request, JunctionTraversal other)
        {
            var key = (FactsOf(request), FactsOf(other));
            TraversalPairFacts facts;
            if (traversalPairs.TryGetValue(key, out facts)) return facts;
            int last = -1, first = -1;
            RoadId found = RoadId.None;
            for (int i = 0; i < request.MovementIds.Count; i++)
                for (int j = 0; j < other.MovementIds.Count; j++)
                {
                    RoadId zone;
                    if (!TryGetConflict(request.MovementIds[i], other.MovementIds[j], out zone)) continue;
                    if (found.IsEmpty) found = zone;
                    last = Math.Max(last, i);
                    if (first < 0 || j < first) first = j;
                }
            facts = new TraversalPairFacts(last, first, found);
            traversalPairs.Add(key, facts);
            TrafficV2WorkCounters.Work.JunctionTraversalPairBuilds++;
            return facts;
        }

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

            // Zones par id croissant : Crossing domine Merge ; premiere zone du genre retenu pour la cause,
            // et contacts les plus precoces de toutes les zones pour une admission conservative.
            var zones = new List<CompiledConflictZone>(model.ConflictZones);
            zones.Sort((a, b) => a.Id.CompareTo(b.Id));
            var pairs = new Dictionary<RoadId, SortedDictionary<RoadId, PairFact>>();
            foreach (var zone in zones)
                for (int a = 0; a < zone.MemberMovementIds.Count; a++)
                    for (int b = 0; b < zone.MemberMovementIds.Count; b++)
                    {
                        RoadId first = zone.MemberMovementIds[a], second = zone.MemberMovementIds[b];
                        if (first == second) continue;
                        SortedDictionary<RoadId, PairFact> partners;
                        if (!pairs.TryGetValue(first, out partners)) pairs.Add(first, partners = new SortedDictionary<RoadId, PairFact>());
                        PairFact previous;
                        if (partners.TryGetValue(second, out previous))
                        {
                            // P10 : toute Crossing domine ; toutes les fusions doivent etre franchies assez tard.
                            if (previous.Kind == ConflictKind.Merge && zone.Kind != ConflictKind.Merge)
                            { previous.Kind = zone.Kind; previous.Zone = zone.Id; }
                            previous.StartSelf = Math.Min(previous.StartSelf, a < zone.ContactStartSMeters.Count ? zone.ContactStartSMeters[a] : 0f);
                            previous.StartOther = Math.Min(previous.StartOther, b < zone.ContactStartSMeters.Count ? zone.ContactStartSMeters[b] : 0f);
                            partners[second] = previous;
                            continue;
                        }
                        // Debuts absents (schema anterieur) : 0, le plus precoce ; genre lu tel quel, jamais infere.
                        partners.Add(second, new PairFact
                        {
                            Zone = zone.Id, Kind = zone.Kind,
                            StartSelf = a < zone.ContactStartSMeters.Count ? zone.ContactStartSMeters[a] : 0f,
                            StartOther = b < zone.ContactStartSMeters.Count ? zone.ContactStartSMeters[b] : 0f
                        });
                    }
            int count = 0;
            foreach (var pair in pairs)
            {
                Entry entry;
                if (!movements.TryGetValue(pair.Key, out entry)) continue;
                int n = pair.Value.Count;
                entry.Incompatible = new RoadId[n];
                entry.Zones = new RoadId[n];
                entry.Kinds = new ConflictKind[n];
                entry.StartSelf = new float[n];
                entry.StartOther = new float[n];
                int i = 0;
                foreach (var partner in pair.Value)
                {
                    entry.Incompatible[i] = partner.Key; entry.Zones[i] = partner.Value.Zone; entry.Kinds[i] = partner.Value.Kind;
                    entry.StartSelf[i] = partner.Value.StartSelf; entry.StartOther[i] = partner.Value.StartOther;
                    i++;
                }
                count += i;
            }

            // Story 5.35 : controle, frontiere, longueur et courbure maximale de chaque mouvement.
            for (int i = 0; i < model.Movements.Count; i++)
            {
                var movement = model.Movements[i];
                var entry = movements[movement.Id];
                CompiledJunctionControl control;
                if (model.TryGetControlForMovement(movement.Id, out control)) { entry.ControlId = control.Id; entry.ControlKind = control.Kind; }
                float sLine;
                entry.Boundary = model.TryGetStopLine(movement.Id, out sLine) ? sLine : 0f;
                entry.Length = movement.LengthMeters;
                float curvature = 0f;
                for (int k = 0; k < movement.Samples.Count; k++) curvature = Math.Max(curvature, Math.Abs(movement.Samples[k].CurvaturePerMeter));
                entry.MaxCurvature = curvature;
            }

            // Preseance entre controles d'un meme carrefour (P2, P3, P4), calculee une fois.
            var rightOfWay = RightOfWayTable.For(model);
            for (int i = 0; i < model.Controls.Count; i++) controlIndex[model.Controls[i].Id] = i;
            for (int j = 0; j < model.Junctions.Count; j++)
            {
                var ids = model.GetControlsInJunction(model.Junctions[j].Id);
                foreach (var p in ids)
                    foreach (var r in ids)
                    {
                        if (p == r) continue;
                        CompiledJunctionControl cp, cr;
                        model.TryGetControl(p, out cp);
                        model.TryGetControl(r, out cr);
                        RightOfWayRelation relation;
                        bool wins = cp.Kind == JunctionControlKind.Priority
                                && (cr.Kind == JunctionControlKind.Yield || cr.Kind == JunctionControlKind.Stop)
                            || cp.Kind == JunctionControlKind.Uncontrolled && cr.Kind == JunctionControlKind.Uncontrolled
                                && rightOfWay.TryGet(r, p, out relation) && relation == RightOfWayRelation.FromRight;
                        if (wins) precedence.Add(ControlKey(p, r));
                    }
            }
            IncompatiblePairCount = count;
            for (int i = 0; i < model.Portals.Count; i++) portals[model.Portals[i].Id] = model.Portals[i];
            for (int i = 0; i < model.Junctions.Count; i++) boundaries[model.Junctions[i].Id] = model.Junctions[i].Boundary;
        }

        private struct PairFact
        {
            public RoadId Zone;
            public ConflictKind Kind;
            public float StartSelf, StartOther;
        }

        public bool Contains(RoadId movementId) { return movements.ContainsKey(movementId); }

        /// <summary>Genre de controle du mouvement (Uncontrolled s'il est inconnu).</summary>
        public JunctionControlKind ControlKindOf(RoadId movementId)
        {
            Entry entry;
            return movements.TryGetValue(movementId, out entry) ? entry.ControlKind : JunctionControlKind.Uncontrolled;
        }

        /// <summary>Frontiere de controle b du mouvement : s_line compilee, sinon 0 (entree generique).</summary>
        public float BoundaryOf(RoadId movementId)
        {
            Entry entry;
            return movements.TryGetValue(movementId, out entry) ? entry.Boundary : 0f;
        }

        public float LengthOf(RoadId movementId)
        {
            Entry entry;
            return movements.TryGetValue(movementId, out entry) ? entry.Length : 0f;
        }

        /// <summary>|κ| maximale des echantillons du mouvement (1/m).</summary>
        public float MaxCurvatureOf(RoadId movementId)
        {
            Entry entry;
            return movements.TryGetValue(movementId, out entry) ? entry.MaxCurvature : 0f;
        }

        /// <summary>
        /// Vrai si la traversee de premier mouvement <paramref name="firstP"/> a preseance sur celle de premier mouvement
        /// <paramref name="firstR"/> (controles d'approche d'un meme carrefour) ; faux sinon, y compris sans preseance.
        /// </summary>
        public bool HasPrecedence(RoadId firstP, RoadId firstR)
        {
            Entry p, r;
            TrafficV2WorkCounters.Work.JunctionPrecedenceChecks++;
            return movements.TryGetValue(firstP, out p) && movements.TryGetValue(firstR, out r) && !p.ControlId.IsEmpty && !r.ControlId.IsEmpty
                && precedence.Contains(ControlKey(p.ControlId, r.ControlId));
        }

        /// <summary>
        /// Conflit de la paire avec son genre et les debuts de contact (de <paramref name="a"/> puis de <paramref name="b"/>) dans
        /// la zone en cause ; lus dans le modele compile, jamais inferes.
        /// </summary>
        public bool TryGetConflict(RoadId a, RoadId b, out RoadId zoneId, out ConflictKind kind, out float startA, out float startB)
        {
            TrafficV2WorkCounters.Work.JunctionPairChecks++;
            zoneId = RoadId.None; kind = ConflictKind.Crossing; startA = 0f; startB = 0f;
            Entry entry;
            if (a == b || !movements.TryGetValue(a, out entry)) return false;
            int at = Array.BinarySearch(entry.Incompatible, b);
            if (at < 0) return false;
            zoneId = entry.Zones[at]; kind = entry.Kinds[at]; startA = entry.StartSelf[at]; startB = entry.StartOther[at];
            return true;
        }

        private long ControlKey(RoadId p, RoadId r)
        {
            int ip, ir;
            if (!controlIndex.TryGetValue(p, out ip) || !controlIndex.TryGetValue(r, out ir)) return -1L;
            return ((long)ip << 32) | (uint)ir;
        }

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
