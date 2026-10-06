using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Coordination
{
    /// <summary>
    /// Constructeur pur du rapport de coordination d'un acteur (Story 5.34), fonction de la frame, de la route du vehicule
    /// et de l'instantane effectif a la frame. Les distances le long de la route ont pour origine le debut de l'occurrence 0
    /// de la route ; l'occupation structuree [SMin, SMax] y est projetee, debordements aux bornes compris.
    /// <list type="bullet">
    /// <item>Positions : un mouvement est occupe si l'intervalle de l'empreinte le chevauche, derriere si l'arriere a depasse
    /// sa fin (libere), devant sinon.</item>
    /// <item>Approches (decision O8) : depuis la premiere traversee a ou devant le pare-chocs avant, les traversees dont le
    /// vehicule tient un grant engage, puis la premiere sans grant engage, qui porte la demande de la frame.</item>
    /// <item>Tete de file : aucune occupation d'un autre acteur V2 ne coupe la route entre le pare-chocs avant et l'entree,
    /// quel que soit le prochain mouvement de cet acteur (debordement arriere des mouvements sortants compris).</item>
    /// <item>Sortie : depuis la fin du dernier mouvement, corridor par corridor, jusqu'au premier occupant V2, a l'entree d'un
    /// mouvement (jamais au-dela d'un carrefour futur), au portail de sortie de la route ou a la fin de la route.</item>
    /// </list>
    /// Ni blocker, ni signe d'une acceleration, ni vitesse n'entrent dans la tete de file ; les dangers non V2 ne comptent
    /// pas dans la sortie.
    /// Story 5.35 : la frontiere de controle b (s_line du premier mouvement de la traversee, sinon 0) remplace l'entree du
    /// mouvement pour d, la tete de file et l'occupation : le troncon avant la ligne n'est ni occupation ni engagement. Le
    /// rapport publie la cinematique declaree de l'acteur et, par approche, la distance au debut de chaque mouvement.
    /// </summary>
    public static class JunctionRequestBuilder
    {
        /// <summary>Occurrences de route examinees de part et d'autre de l'element localise pour la position des mouvements.</summary>
        public const int PositionWindowOccurrences = 12;

        /// <summary>Traversees examinees au plus devant le pare-chocs : les engagees, puis la demandee.</summary>
        public const int MaxApproaches = 3;

        private static readonly ConditionalWeakTable<IReadOnlyList<RouteOccurrence>, double[]> StartCache =
            new ConditionalWeakTable<IReadOnlyList<RouteOccurrence>, double[]>();

        /// <param name="driver">Profil du conducteur ; nul : rapport d'occupation seul, sans demande (vehicule inerte).</param>
        /// <param name="snapshot">Instantane lu a la frame ; un instantane decale ne porte aucun grant effectif.</param>
        /// <param name="holdEntrySpeedMetersPerSecond">Vitesse d'entree du maintien D11 : plancher du seuil de demande (O9, O14).</param>
        /// <param name="lateralGripMetersPerSecondSquared">Adherence laterale du vehicule (Story 5.35) : a_lat = min(confort, adherence).</param>
        public static JunctionActorReport Build(TrafficFrame frame, JunctionConflictIndex index, RoadId trafficId, RoutePlan route,
            DriverProfile? driver, float deltaTimeSeconds, float controlMarginMeters, JunctionSnapshot snapshot, ulong frameId,
            float holdEntrySpeedMetersPerSecond = 0f, float lateralGripMetersPerSecondSquared = float.PositiveInfinity)
        {
            if (frame == null) throw new ArgumentNullException("frame");
            if (index == null) throw new ArgumentNullException("index");
            TrafficActor actor;
            if (!frame.TryGetActor(trafficId, out actor)) throw new ArgumentException("UnknownTrafficId", "trafficId");
            TrafficV2WorkCounters.Work.JunctionReports++;
            var corners = TrafficFrame.Corners(actor.Pose);
            float length = actor.Pose.Footprint.FrontMeters + actor.Pose.Footprint.RearMeters;
            float reservation = length + (driver.HasValue && Finite(driver.Value.MinimumGap) ? driver.Value.MinimumGap : 0f);
            var kinematics = driver.HasValue
                ? new JunctionKinematics(Math.Max(0f, actor.TangentialSpeedMetersPerSecond), driver.Value.MaxAcceleration, driver.Value.DesiredSpeed,
                    Math.Min(driver.Value.ComfortableDeceleration, lateralGripMetersPerSecondSquared), length)
                : default(JunctionKinematics);
            if (!actor.Location.Localized)
                return new JunctionActorReport(trafficId, false, RoadId.None, corners, null, null, null, null, false, false,
                    JunctionRequestRejection.NotLocalized, reservation, kinematics);

            RoadId element = actor.Location.ElementId;
            ElementOccupant occupancy;
            bool occupied = frame.TryGetOccupancy(trafficId, out occupancy);
            int q = route == null || route.ModelId != index.Model.ModelId || route.ModelVersion != index.Model.Version ? -1
                : Locate(route, occupied ? occupancy.ElementId : element);
            if (q < 0) return Minimal(index, trafficId, element, corners, reservation, kinematics);

            var occurrences = route.Occurrences;
            double[] start = Starts(occurrences);
            double front, rear;
            if (occupied)
            {
                front = start[q] + occupancy.SMaxMeters - occurrences[q].StartSMeters;
                rear = start[q] + occupancy.SMinMeters - occurrences[q].StartSMeters;
            }
            else front = rear = start[q] + actor.Location.SMeters - occurrences[q].StartSMeters;
            // Story 5.35 : pare-chocs de reference (abscisse de route du point localise + porte-a-faux avant), continu d'un element
            // a l'autre. Il mesure d et l'occupation au-dela d'une ligne (b > 0) : le SMax de l'occupation, projete sur le seul
            // element localise et elargi de sa marge r = (h/2)/(1 - kappa.d_max), saute de plusieurs decimetres au basculement
            // corridor -> mouvement courbe, en pleine zone de freinage. C'est aussi la pose de la preuve de separation P5.
            double referenceFront = start[q] + actor.Location.SMeters - occurrences[q].StartSMeters + actor.Pose.Footprint.FrontMeters;

            var approaches = new List<JunctionApproach>();
            bool hasRequest = false;
            int approachEnd = -1;
            var rejection = JunctionRequestRejection.None;
            if (!driver.HasValue) rejection = JunctionRequestRejection.NoDriver;
            else if (!occupied) rejection = JunctionRequestRejection.NoOccupancy;
            else
            {
                int k0 = q;
                while (k0 < occurrences.Count && (occurrences[k0].Kind != RoadElementKind.JunctionMovement || start[k0 + 1] <= front)) k0++;
                for (int count = 0; k0 < occurrences.Count && count < MaxApproaches; count++)
                {
                    int last;
                    List<int> at;
                    var traversal = Chain(index, occurrences, k0, out last, out at);
                    approachEnd = last;
                    float boundary = index.BoundaryOf(occurrences[k0].Id);
                    double lineFront = boundary > 0f ? referenceFront : front;
                    // Une replanification dans le mouvement tronque sa premiere occurrence : b reste une abscisse du
                    // modele, donc son origine de route precede start[k0] de StartSMeters.
                    float d = (float)(start[k0] + boundary - occurrences[k0].StartSMeters - lineFront);
                    var startsAhead = new float[at.Count];
                    for (int i = 0; i < at.Count; i++) startsAhead[i] = (float)(start[at[i]] - occurrences[at[i]].StartSMeters - front);
                    var distances = JunctionDistances.For(driver.Value, actor.TangentialSpeedMetersPerSecond, deltaTimeSeconds,
                        controlMarginMeters, holdEntrySpeedMetersPerSecond);
                    JunctionRecord grant;
                    bool effective = Covers(snapshot, trafficId, traversal, frameId, out grant);
                    bool committed = effective && (grant.Reason == JunctionReason.Committed
                        || grant.Reason == JunctionReason.CommittedCarried || grant.Reason == JunctionReason.Restored);
                    if (effective && (committed || d <= 0f || d < distances.StopMeters))
                    {
                        // O8 : une continuation appartient toujours a la traversee engagee, meme loin de sa propre entree.
                        approaches.Add(new JunctionApproach(traversal, d, distances, true, RoadId.None, true, true, default(JunctionExitAssessment),
                            startsAhead, boundary));
                        k0 = last + 1;
                        while (k0 < occurrences.Count && occurrences[k0].Kind != RoadElementKind.JunctionMovement) k0++;
                        continue;
                    }
                    RoadId masking;
                    bool head = HeadOfQueue(frame, index, occurrences, start, q, k0, boundary, lineFront, trafficId, out masking);
                    var exit = ExitSearch(frame, index, route, start, last, trafficId, reservation);
                    approaches.Add(new JunctionApproach(traversal, d, distances, head, masking, effective, false, exit, startsAhead, boundary));
                    hasRequest = true;
                    rejection = d > distances.RequestThresholdMeters ? JunctionRequestRejection.TooFar
                        : !head ? JunctionRequestRejection.NotHeadOfQueue : JunctionRequestRejection.None;
                    break;
                }
                if (!hasRequest) rejection = JunctionRequestRejection.NoTraversal;
            }

            // Positions des mouvements, de la traversee la plus recente a ou derriere l'arriere du vehicule jusqu'a la fin de la
            // traversee demandee : une route qui repasse par un carrefour (objectif intermediaire, boucle legale) ne confond pas
            // ses passages. Sans occupation, seul l'element localise est connu.
            int rearOccurrence = q;
            while (rearOccurrence > 0 && start[rearOccurrence] > rear) rearOccurrence--;
            int low = rearOccurrence;
            for (int k = rearOccurrence; k >= 0 && k >= rearOccurrence - PositionWindowOccurrences; k--)
                if (occurrences[k].Kind == RoadElementKind.JunctionMovement) { low = ChainStart(index, occurrences, k); break; }
            int high = approachEnd >= 0 ? Math.Max(approachEnd, q) : Math.Min(occurrences.Count - 1, q + PositionWindowOccurrences);
            var positions = new List<JunctionMovementPosition>();
            var occupiedIds = new List<RoadId>();
            var occupiedTraversals = new List<JunctionTraversal>();
            int coveredUntil = -1;
            for (int k = low; k <= high; k++)
            {
                if (occurrences[k].Kind != RoadElementKind.JunctionMovement) continue;
                // Le premier mouvement d'une traversee n'est occupe qu'au-dela de la frontiere de controle, mesuree au pare-chocs
                // de reference (Story 5.35).
                float line = ChainStart(index, occurrences, k) == k ? index.BoundaryOf(occurrences[k].Id) : 0f;
                double a = start[k] + line - occurrences[k].StartSMeters, b = start[k + 1], reach = line > 0f ? referenceFront : front;
                var status = !occupied ? (k == q ? JunctionMovementStatus.Occupied : JunctionMovementStatus.Unknown)
                    : a < reach && b > rear ? JunctionMovementStatus.Occupied
                    : b <= rear ? JunctionMovementStatus.Behind : JunctionMovementStatus.Ahead;
                positions.Add(new JunctionMovementPosition(occurrences[k].Id, status));
                if (status != JunctionMovementStatus.Occupied) continue;
                if (!occupiedIds.Contains(occurrences[k].Id)) occupiedIds.Add(occurrences[k].Id);
                if (k <= coveredUntil) continue;
                int last;
                List<int> ignored;
                occupiedTraversals.Add(Chain(index, occurrences, k, out last, out ignored));
                coveredUntil = last;
            }
            return new JunctionActorReport(trafficId, true, element, corners, occupiedIds, occupiedTraversals, positions, approaches,
                hasRequest, hasRequest && rejection == JunctionRequestRejection.None, rejection, reservation, kinematics);
        }

        /// <summary>Premier mouvement de la traversee qui contient l'occurrence de mouvement <paramref name="k"/>.</summary>
        private static int ChainStart(JunctionConflictIndex index, IReadOnlyList<RouteOccurrence> occurrences, int k)
        {
            var junction = index.JunctionOf(occurrences[k].Id);
            int first = k;
            for (int j = k - 1; j >= 0 && !junction.IsEmpty; j--)
            {
                if (occurrences[j].Kind != RoadElementKind.JunctionMovement) continue;
                if (index.JunctionOf(occurrences[j].Id) != junction) break;
                first = j;
            }
            return first;
        }

        /// <summary>Rapport sans route exploitable : seul l'element localise, s'il est un mouvement, est occupe.</summary>
        private static JunctionActorReport Minimal(JunctionConflictIndex index, RoadId trafficId, RoadId element,
            IReadOnlyList<Vector3> corners, float reservation, JunctionKinematics kinematics)
        {
            bool movement = index.Contains(element);
            return new JunctionActorReport(trafficId, true, element, corners, movement ? new[] { element } : null,
                movement ? new[] { new JunctionTraversal(index.JunctionOf(element), new[] { element }, index.ToCorridorOf(element)) } : null,
                movement ? new[] { new JunctionMovementPosition(element, JunctionMovementStatus.Occupied) } : null, null, false, false,
                JunctionRequestRejection.NoTraversal, reservation, kinematics);
        }

        /// <summary>Occurrence de l'element pres de la progression : d'abord en avant, puis en arriere ; -1 si absente.</summary>
        private static int Locate(RoutePlan route, RoadId elementId)
        {
            int p = route.ProgressOccurrenceIndex, n = route.Occurrences.Count;
            for (int k = Math.Max(0, p); k < n && k <= p + PositionWindowOccurrences; k++)
                if (route.Occurrences[k].Id == elementId) return k;
            for (int k = Math.Min(n - 1, p - 1); k >= 0 && k >= p - PositionWindowOccurrences; k--)
                if (route.Occurrences[k].Id == elementId) return k;
            return -1;
        }

        /// <summary>Debut cumule de chaque occurrence (m), plus la longueur totale en fin ; calcule une fois par route.</summary>
        private static double[] Starts(IReadOnlyList<RouteOccurrence> occurrences)
        {
            return StartCache.GetValue(occurrences, list =>
            {
                var values = new double[list.Count + 1];
                for (int k = 0; k < list.Count; k++) values[k + 1] = values[k] + (list[k].EndSMeters - list[k].StartSMeters);
                return values;
            });
        }

        /// <summary>
        /// Traversee depuis le mouvement de l'occurrence <paramref name="k0"/> : mouvements consecutifs du meme carrefour, avec les
        /// corridors qui les separent ; arret au premier mouvement d'un autre carrefour ou a la fin de route.
        /// </summary>
        private static JunctionTraversal Chain(JunctionConflictIndex index, IReadOnlyList<RouteOccurrence> occurrences, int k0,
            out int last, out List<int> at)
        {
            var junction = index.JunctionOf(occurrences[k0].Id);
            var movements = new List<RoadId> { occurrences[k0].Id };
            at = new List<int> { k0 };
            last = k0;
            for (int k = k0 + 1; k < occurrences.Count && !junction.IsEmpty; k++)
            {
                if (occurrences[k].Kind != RoadElementKind.JunctionMovement) continue;
                if (index.JunctionOf(occurrences[k].Id) != junction) break;
                movements.Add(occurrences[k].Id);
                at.Add(k);
                last = k;
            }
            var exit = last + 1 < occurrences.Count && occurrences[last + 1].Kind == RoadElementKind.LaneCorridor
                ? occurrences[last + 1].Id : RoadId.None;
            return new JunctionTraversal(junction, movements, exit);
        }

        /// <summary>Grant effectif de l'instantane, lu a la frame, qui contient tous les mouvements de la traversee.</summary>
        private static bool Covers(JunctionSnapshot snapshot, RoadId trafficId, JunctionTraversal traversal, ulong frameId,
            out JunctionRecord record)
        {
            record = default(JunctionRecord);
            if (snapshot == null || !snapshot.TryGetEffectiveGrant(trafficId, traversal.FirstMovementId, frameId, out record)) return false;
            for (int i = 0; i < traversal.MovementIds.Count; i++)
                if (!record.Contains(traversal.MovementIds[i])) return false;
            return true;
        }

        private static bool HeadOfQueue(TrafficFrame frame, JunctionConflictIndex index, IReadOnlyList<RouteOccurrence> occurrences,
            double[] start, int q, int k0, float boundary, double front, RoadId self, out RoadId masking)
        {
            masking = RoadId.None;
            double entry = start[k0] + boundary - occurrences[k0].StartSMeters;
            if (entry <= front) return true;
            // Story 5.35 : un acteur arrete a la ligne occupe deja le debut du mouvement, avant la frontiere.
            if (boundary > 0f && Cuts(frame.GetOccupants(occurrences[k0].Id), start[k0] - occurrences[k0].StartSMeters, front, entry, self,
                false, out masking)) return false;
            for (int j = q; j < k0; j++)
            {
                if (Cuts(frame.GetOccupants(occurrences[j].Id), start[j] - occurrences[j].StartSMeters, front, entry, self, false, out masking))
                    return false;
                if (occurrences[j].Kind != RoadElementKind.LaneCorridor) continue;
                // Mouvements sortants du corridor : un acteur engage sur un autre mouvement que le notre, l'arriere encore ici.
                var outgoing = index.OutgoingMovements(occurrences[j].Id);
                for (int m = 0; m < outgoing.Count; m++)
                    if (Cuts(frame.GetOccupants(outgoing[m]), start[j + 1], front, entry, self, true, out masking)) return false;
            }
            return true;
        }

        /// <summary>Un occupant (autre que soi) dont l'intervalle projete coupe ]from, to[ ; seulement l'arriere debordant si demande.</summary>
        private static bool Cuts(IReadOnlyList<ElementOccupant> occupants, double origin, double from, double to, RoadId self,
            bool rearOverflowOnly, out RoadId actor)
        {
            actor = RoadId.None;
            for (int i = 0; i < occupants.Count; i++)
            {
                var o = occupants[i];
                if (o.TrafficId == self || (rearOverflowOnly && !(o.SMinMeters < 0f))) continue;
                if (origin + o.SMinMeters < to && origin + o.SMaxMeters > from) { actor = o.TrafficId; return true; }
            }
            return false;
        }

        private static JunctionExitAssessment ExitSearch(TrafficFrame frame, JunctionConflictIndex index, RoutePlan route,
            double[] start, int last, RoadId self, float required)
        {
            var occurrences = route.Occurrences;
            double exitStart = start[last + 1];
            Portal portal;
            bool hasPortal = index.TryGetPortal(route.ExitPortalId, out portal);
            for (int j = last + 1; j < occurrences.Count; j++)
            {
                var occurrence = occurrences[j];
                if (occurrence.Kind == RoadElementKind.JunctionMovement)
                    return new JunctionExitAssessment((float)(start[j] - exitStart), JunctionExitBound.ExitSearchBound, occurrence.Id, required);
                double nearest = double.PositiveInfinity;
                RoadId nearestId = RoadId.None;
                var occupants = frame.GetOccupants(occurrence.Id);
                for (int i = 0; i < occupants.Count; i++)
                {
                    if (occupants[i].TrafficId == self) continue;
                    double at = start[j] + occupants[i].SMinMeters - occurrence.StartSMeters;
                    if (at < nearest) { nearest = at; nearestId = occupants[i].TrafficId; }
                }
                var outgoing = index.OutgoingMovements(occurrence.Id);
                for (int m = 0; m < outgoing.Count; m++)
                {
                    var onMovement = frame.GetOccupants(outgoing[m]);
                    for (int i = 0; i < onMovement.Count; i++)
                    {
                        if (onMovement[i].TrafficId == self || !(onMovement[i].SMinMeters < 0f)) continue;
                        double at = start[j + 1] + onMovement[i].SMinMeters;
                        if (at < nearest) { nearest = at; nearestId = onMovement[i].TrafficId; }
                    }
                }
                double portalAt = hasPortal && portal.CorridorId == occurrence.Id && portal.SMeters >= occurrence.StartSMeters
                    && portal.SMeters <= occurrence.EndSMeters ? start[j] + portal.SMeters - occurrence.StartSMeters : double.PositiveInfinity;
                if (!double.IsPositiveInfinity(nearest) && nearest <= portalAt)
                    return new JunctionExitAssessment((float)Math.Max(0d, nearest - exitStart), JunctionExitBound.Occupant, nearestId, required);
                if (!double.IsPositiveInfinity(portalAt))
                    return new JunctionExitAssessment(float.PositiveInfinity, JunctionExitBound.ExitPortal, RoadId.None, required);
            }
            return new JunctionExitAssessment((float)(start[occurrences.Count] - exitStart), JunctionExitBound.RouteEnd, RoadId.None, required);
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
