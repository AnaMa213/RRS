using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;

namespace RoadRage.Features.Vehicles.Traffic.Coordination
{
    /// <summary>
    /// Coordinateur de carrefour hote unique (Story 5.34), generique pour tous les carrefours du modele. Il possede seul
    /// l'etat mutable de coordination : grants anterieurs et anciennete des demandes. Il ne choisit pas de route, ne genere
    /// pas de chemin et ne commande aucun vehicule.
    ///
    /// Un lot (frame N) resout ensemble, de facon deterministe et independante de l'ordre des entrees :
    /// 0. la protection des occupants reels, depuis l'occupation structuree et independamment de la memoire ;
    /// 1. les grants anterieurs (ActorGone, liberation, engagement, demande retiree, sortie) ;
    /// 2. les nouvelles demandes, triees par (RequestSinceFrame, TrafficId) : sortie, occupant protege, grant incompatible,
    ///    demande plus ancienne refusee pour conflit, sinon Granted ;
    /// 3. la publication d'un instantane immuable effectif a N+1, qui expire apres N+1.
    /// Une frame invalide ne fait examiner aucune demande : les grants engages sont republies (CommittedCarried), les autres
    /// revoques (FrameUnavailable).
    /// </summary>
    public sealed class JunctionCoordinator
    {
        private sealed class Grant
        {
            public RoadId TrafficId, JunctionId, TraversalId, ExitCorridorId;
            public List<RoadId> Movements;
            public ulong Since;
            public bool Engaged;
            public float Reservation;
            public JunctionReason Reason;
            // Etat du lot courant.
            public bool Gone, Covered, Fresh;
            public List<RoadId> Released;
            public JunctionReason ReleaseReason;
        }

        private struct Seniority
        {
            public RoadId TraversalId;
            public ulong Since;
        }

        private struct Occupation
        {
            public RoadId Actor;
            public RoadId Movement;
        }

        private struct Pending
        {
            public RoadId Actor;
            public JunctionTraversal Traversal;
        }

        private List<Grant> grants = new List<Grant>();
        private Dictionary<RoadId, Seniority> seniority = new Dictionary<RoadId, Seniority>();

        /// <param name="firstEffectiveFrame">Frame a laquelle l'instantane initial, vide, est lu.</param>
        public JunctionCoordinator(CompiledRoadModel model, ulong firstEffectiveFrame = 1UL)
        {
            Index = JunctionConflictIndex.For(model);
            Current = JunctionSnapshot.Initial(firstEffectiveFrame);
        }

        public JunctionConflictIndex Index { get; }
        public CompiledRoadModel Model { get { return Index.Model; } }
        /// <summary>Dernier instantane publie (EffectiveFrame = frame du dernier lot + 1).</summary>
        public JunctionSnapshot Current { get; private set; }
        /// <summary>Lots resolus sur une frame invalide.</summary>
        public int FrameFailures { get; private set; }
        public int Batches { get; private set; }

        /// <summary>Lot de la frame <paramref name="frameId"/>, construite : un rapport par acteur present.</summary>
        public JunctionSnapshot Resolve(ulong frameId, IReadOnlyList<JunctionActorReport> reports)
        {
            Batches++;
            TrafficV2WorkCounters.Work.JunctionBatches++;
            long pairStart = TrafficV2WorkCounters.Work.JunctionPairChecks;
            var actors = new List<JunctionActorReport>();
            if (reports != null)
                for (int i = 0; i < reports.Count; i++)
                    if (reports[i] != null) actors.Add(reports[i]);
            actors.Sort((a, b) => a.TrafficId.CompareTo(b.TrafficId));
            var byId = new Dictionary<RoadId, JunctionActorReport>(actors.Count);
            for (int i = 0; i < actors.Count; i++)
            {
                if (byId.ContainsKey(actors[i].TrafficId)) throw new ArgumentException("DuplicateTrafficId", "reports");
                byId.Add(actors[i].TrafficId, actors[i]);
            }
            var records = new List<JunctionRecord>();
            ulong effective = frameId + 1UL;
            int requests = 0, valid = 0;

            // Anciennete : premiere frame d'une suite ininterrompue de demandes valides pour la meme traversee.
            var nextSeniority = new Dictionary<RoadId, Seniority>();
            foreach (var actor in actors)
            {
                if (actor.HasRequest) requests++;
                if (!actor.RequestValid) continue;
                valid++;
                TrafficV2WorkCounters.Work.JunctionRequests++;
                var traversal = actor.Request.Traversal.FirstMovementId;
                Seniority previous;
                nextSeniority[actor.TrafficId] = seniority.TryGetValue(actor.TrafficId, out previous) && previous.TraversalId == traversal
                    ? previous : new Seniority { TraversalId = traversal, Since = frameId };
            }
            seniority = nextSeniority;

            // A. Grants anterieurs : absents, et mouvements liberes des grants engages.
            foreach (var grant in grants)
            {
                grant.Gone = false; grant.Covered = false; grant.Fresh = false; grant.Released = null;
                JunctionActorReport holder;
                if (!byId.TryGetValue(grant.TrafficId, out holder)) { grant.Gone = true; continue; }
                if (grant.Engaged) MarkReleased(grant, holder);
            }

            // 0. Protection des occupants, depuis l'occupation de la frame seule.
            var occupants = new Dictionary<RoadId, List<Occupation>>();
            int occupantCount = 0, entered = 0, incompatible = 0;
            foreach (var actor in actors)
            {
                if (actor.OccupiedMovements.Count == 0) continue;
                occupantCount++;
                foreach (var movement in actor.OccupiedMovements)
                {
                    var junction = Index.JunctionOf(movement);
                    if (!junction.IsEmpty) Bucket(occupants, junction).Add(new Occupation { Actor = actor.TrafficId, Movement = movement });
                }
            }
            var live = new Dictionary<RoadId, List<Grant>>();
            foreach (var grant in grants) if (!grant.Gone) Bucket(live, grant.JunctionId).Add(grant);
            var unserved = new HashSet<RoadId>();
            var restored = new List<Grant>();
            foreach (var actor in actors)
            {
                if (actor.OccupiedMovements.Count == 0) continue;
                var uncovered = new List<RoadId>();
                foreach (var movement in actor.OccupiedMovements)
                {
                    bool covered = false;
                    foreach (var grant in grants)
                    {
                        if (grant.Gone || grant.TrafficId != actor.TrafficId || !LiveContains(grant, movement)) continue;
                        grant.Covered = true;
                        grant.Engaged = true;
                        covered = true;
                    }
                    if (!covered) uncovered.Add(movement);
                }
                if (uncovered.Count == 0) continue;
                entered++;
                foreach (var traversal in actor.OccupiedTraversals)
                {
                    if (!Intersects(traversal, uncovered) || traversal.JunctionId.IsEmpty) continue;
                    RoadId cause, zone;
                    if (ConflictsWithOccupants(traversal, actor.TrafficId, occupants, out cause, out zone)
                        || ConflictsWithGrants(traversal, actor.TrafficId, live, out cause, out zone))
                    {
                        incompatible++;
                        unserved.Add(actor.TrafficId);
                        records.Add(new JunctionRecord(actor.TrafficId, traversal.JunctionId, traversal.FirstMovementId, traversal.MovementIds,
                            SinceOf(actor.TrafficId, traversal.FirstMovementId, frameId), frameId, effective, JunctionGrantStatus.Denied,
                            JunctionReason.IncompatibleOccupancy, cause, zone));
                        continue;
                    }
                    var grant = NewGrant(actor.TrafficId, traversal, SinceOf(actor.TrafficId, traversal.FirstMovementId, frameId),
                        actor.ReservationMeters, JunctionReason.Restored);
                    grant.Engaged = true; grant.Covered = true; grant.Fresh = true;
                    restored.Add(grant);
                    Bucket(live, grant.JunctionId).Add(grant);
                }
            }

            // 1. Grants anterieurs non couverts par l'etape 0 (engages et occupants d'abord, puis demandes en attente).
            var kept = new List<Grant>();
            var waiting = new List<Grant>();
            foreach (var grant in grants)
            {
                if (grant.Gone)
                {
                    records.Add(Record(grant, grant.Movements, frameId, effective, JunctionGrantStatus.Revoked, JunctionReason.ActorGone));
                    continue;
                }
                var holder = byId[grant.TrafficId];
                if (grant.Released != null)
                {
                    bool all = grant.Released.Count == grant.Movements.Count;
                    records.Add(Record(grant, grant.Released, frameId, effective, JunctionGrantStatus.Released,
                        all ? grant.ReleaseReason : JunctionReason.MovementCleared));
                    if (all) continue;
                    foreach (var movement in grant.Released) grant.Movements.Remove(movement);
                }
                if (grant.Covered || grant.Engaged || Engaged(grant, holder))
                {
                    grant.Engaged = true;
                    grant.Reason = JunctionReason.Committed;
                    kept.Add(grant);
                }
                else waiting.Add(grant);
            }
            kept.AddRange(restored);
            Rebuild(live, kept);
            foreach (var grant in waiting)
            {
                var holder = byId[grant.TrafficId];
                if (!holder.RequestValid || !MatchesTraversal(grant, holder.Request.Traversal))
                {
                    records.Add(Record(grant, grant.Movements, frameId, effective, JunctionGrantStatus.Revoked, JunctionReason.RequestWithdrawn));
                    continue;
                }
                JunctionExitBound bound;
                if (!ExitSufficient(holder.Request, grant.TrafficId, kept, byId, out bound))
                {
                    records.Add(Record(grant, grant.Movements, frameId, effective, JunctionGrantStatus.Revoked, JunctionReason.ExitBlocked,
                        exitBound: bound));
                    continue;
                }
                // Un occupant incompatible (entre sans grant, etape 0) bloque les mouvements incompatibles : un grant non engage
                // ne survit pas a lui, son titulaire pouvant encore s'arreter.
                RoadId occupant, zone;
                if (ConflictsWithOccupants(holder.Request.Traversal, grant.TrafficId, occupants, out occupant, out zone))
                {
                    records.Add(Record(grant, grant.Movements, frameId, effective, JunctionGrantStatus.Revoked, JunctionReason.ConflictOccupied,
                        occupant, zone));
                    continue;
                }
                grant.Reason = JunctionReason.Pending;
                grant.Since = SinceOf(grant.TrafficId, grant.TraversalId, frameId);
                kept.Add(grant);
                Bucket(live, grant.JunctionId).Add(grant);
            }

            // 2. Nouvelles demandes, par (RequestSinceFrame, TrafficId).
            var candidates = new List<JunctionActorReport>();
            foreach (var actor in actors)
                if (actor.RequestValid && !unserved.Contains(actor.TrafficId) && !CoveredByOwnGrant(actor, kept)) candidates.Add(actor);
            candidates.Sort((a, b) =>
            {
                int order = seniority[a.TrafficId].Since.CompareTo(seniority[b.TrafficId].Since);
                return order != 0 ? order : a.TrafficId.CompareTo(b.TrafficId);
            });
            var refused = new Dictionary<RoadId, List<Pending>>();
            foreach (var actor in candidates)
            {
                var traversal = actor.Request.Traversal;
                ulong since = seniority[actor.TrafficId].Since;
                RoadId cause, zone;
                JunctionExitBound bound;
                if (!Known(traversal))
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.InvalidRequest));
                else if (!ExitSufficient(actor.Request, actor.TrafficId, kept, byId, out bound))
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.ExitBlocked, exitBound: bound));
                else if (ConflictsWithOccupants(traversal, actor.TrafficId, occupants, out cause, out zone))
                {
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.ConflictOccupied, cause, zone));
                    Bucket(refused, traversal.JunctionId).Add(new Pending { Actor = actor.TrafficId, Traversal = traversal });
                }
                else if (ConflictsWithGrants(traversal, actor.TrafficId, live, out cause, out zone))
                {
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.ConflictGranted, cause, zone));
                    Bucket(refused, traversal.JunctionId).Add(new Pending { Actor = actor.TrafficId, Traversal = traversal });
                }
                else if (ConflictsWithSeniors(traversal, actor.TrafficId, refused, out cause, out zone))
                {
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.SeniorRequestPending, cause, zone));
                    Bucket(refused, traversal.JunctionId).Add(new Pending { Actor = actor.TrafficId, Traversal = traversal });
                }
                else
                {
                    var grant = NewGrant(actor.TrafficId, traversal, since, actor.Request.Exit.RequiredMeters, JunctionReason.Granted);
                    grant.Fresh = true;
                    kept.Add(grant);
                    Bucket(live, grant.JunctionId).Add(grant);
                }
            }

            // 3. Publication : tout grant vivant est republie, sinon il expire.
            kept.Sort(CompareGrants);
            foreach (var grant in kept)
                records.Add(Record(grant, grant.Movements, frameId, effective,
                    grant.Reason == JunctionReason.Granted ? JunctionGrantStatus.Granted : JunctionGrantStatus.Held, grant.Reason));
            grants = kept;
            Current = Publish(frameId, effective, records, true, actors.Count, occupantCount, requests, valid, entered, incompatible,
                TrafficV2WorkCounters.Work.JunctionPairChecks - pairStart);
            return Current;
        }

        /// <summary>
        /// Lot sur frame invalide (fail-closed) : aucune demande examinee, aucun grant emis. Les grants engages connus au dernier
        /// lot valide sont republies a l'identique (CommittedCarried) ; les autres sont revoques (FrameUnavailable). L'anciennete
        /// n'est pas interrompue.
        /// </summary>
        public JunctionSnapshot ResolveUnavailableFrame(ulong frameId)
        {
            Batches++;
            FrameFailures++;
            TrafficV2WorkCounters.Work.JunctionBatches++;
            ulong effective = frameId + 1UL;
            var records = new List<JunctionRecord>();
            var kept = new List<Grant>();
            foreach (var grant in grants)
            {
                if (grant.Engaged)
                {
                    grant.Reason = JunctionReason.CommittedCarried;
                    kept.Add(grant);
                    records.Add(Record(grant, grant.Movements, frameId, effective, JunctionGrantStatus.Held, JunctionReason.CommittedCarried));
                }
                else records.Add(Record(grant, grant.Movements, frameId, effective, JunctionGrantStatus.Revoked, JunctionReason.FrameUnavailable));
            }
            grants = kept;
            Current = Publish(frameId, effective, records, false, 0, 0, 0, 0, 0, 0, 0L);
            return Current;
        }

        // ------------------------------------------------------------------ regles

        /// <summary>Un mouvement est libere quand l'arriere du titulaire l'a depasse ; hors fenetre de route ou sans localisation,
        /// quand plus aucun coin de l'empreinte n'est dans la frontiere du carrefour.</summary>
        private void MarkReleased(Grant grant, JunctionActorReport holder)
        {
            bool outside = !Index.InsideBoundary(grant.JunctionId, holder.Corners);
            foreach (var movement in grant.Movements)
            {
                var status = holder.Localized ? holder.StatusOf(movement) : JunctionMovementStatus.Unknown;
                bool released = status == JunctionMovementStatus.Behind || (status == JunctionMovementStatus.Unknown && outside);
                if (!released) continue;
                if (grant.Released == null) { grant.Released = new List<RoadId>(); grant.ReleaseReason = JunctionReason.Cleared; }
                grant.Released.Add(movement);
                if (status == JunctionMovementStatus.Unknown) grant.ReleaseReason = JunctionReason.ClearedUnlocalized;
            }
        }

        /// <summary>Engagement d'un grant tenu : entree franchie, ou d &lt; D_stop sur sa traversee.</summary>
        private static bool Engaged(Grant grant, JunctionActorReport holder)
        {
            var status = holder.Localized ? holder.StatusOf(grant.TraversalId) : JunctionMovementStatus.Unknown;
            if (status == JunctionMovementStatus.Occupied || status == JunctionMovementStatus.Behind) return true;
            JunctionApproach approach;
            return holder.TryGetApproach(grant.TraversalId, out approach)
                && (approach.Crossed || approach.DistanceMeters < approach.Distances.StopMeters);
        }

        /// <summary>
        /// Sortie : longueur libre de la recherche bornee, moins L + s0 par grant deja emis vers le meme corridor de sortie dont
        /// le titulaire n'y est pas encore localise ; suffisante si le reste couvre L + s0 du demandeur. Le portail de sortie
        /// atteint d'abord est toujours suffisant.
        /// </summary>
        private static bool ExitSufficient(JunctionApproach request, RoadId self, List<Grant> grants,
            Dictionary<RoadId, JunctionActorReport> byId, out JunctionExitBound bound)
        {
            var exit = request.Exit;
            bound = JunctionExitBound.None;
            if (float.IsPositiveInfinity(exit.FreeLengthMeters)) return true;
            double reserved = 0d;
            var corridor = request.Traversal.ExitCorridorId;
            foreach (var grant in grants)
            {
                if (grant.TrafficId == self || grant.ExitCorridorId != corridor || corridor.IsEmpty) continue;
                JunctionActorReport holder;
                if (byId.TryGetValue(grant.TrafficId, out holder) && holder.Localized && holder.ElementId == corridor) continue;
                reserved += grant.Reservation;
            }
            if (exit.FreeLengthMeters - reserved >= exit.RequiredMeters) return true;
            bound = reserved > 0d && exit.FreeLengthMeters >= exit.RequiredMeters ? JunctionExitBound.Reservations : exit.Bound;
            return false;
        }

        private bool ConflictsWithOccupants(JunctionTraversal traversal, RoadId self, Dictionary<RoadId, List<Occupation>> occupants,
            out RoadId cause, out RoadId zone)
        {
            cause = RoadId.None; zone = RoadId.None;
            List<Occupation> list;
            if (!occupants.TryGetValue(traversal.JunctionId, out list)) return false;
            foreach (var occupation in list)
            {
                if (occupation.Actor == self) continue;
                foreach (var movement in traversal.MovementIds)
                    if (Index.TryGetConflict(movement, occupation.Movement, out zone)) { cause = occupation.Actor; return true; }
            }
            return false;
        }

        private bool ConflictsWithGrants(JunctionTraversal traversal, RoadId self, Dictionary<RoadId, List<Grant>> live,
            out RoadId cause, out RoadId zone)
        {
            cause = RoadId.None; zone = RoadId.None;
            List<Grant> list;
            if (!live.TryGetValue(traversal.JunctionId, out list)) return false;
            foreach (var grant in list)
            {
                if (grant.TrafficId == self) continue;
                foreach (var held in grant.Movements)
                {
                    // Un mouvement deja libere a ce lot ne bloque plus rien.
                    if (grant.Released != null && grant.Released.Contains(held)) continue;
                    foreach (var movement in traversal.MovementIds)
                        if (Index.TryGetConflict(movement, held, out zone)) { cause = grant.TrafficId; return true; }
                }
            }
            return false;
        }

        private bool ConflictsWithSeniors(JunctionTraversal traversal, RoadId self, Dictionary<RoadId, List<Pending>> refused,
            out RoadId cause, out RoadId zone)
        {
            cause = RoadId.None; zone = RoadId.None;
            List<Pending> list;
            if (!refused.TryGetValue(traversal.JunctionId, out list)) return false;
            foreach (var senior in list)
            {
                if (senior.Actor == self) continue;
                foreach (var other in senior.Traversal.MovementIds)
                    foreach (var movement in traversal.MovementIds)
                        if (Index.TryGetConflict(movement, other, out zone)) { cause = senior.Actor; return true; }
            }
            return false;
        }

        private bool Known(JunctionTraversal traversal)
        {
            if (traversal.JunctionId.IsEmpty) return false;
            foreach (var movement in traversal.MovementIds)
                if (!Index.Contains(movement) || Index.JunctionOf(movement) != traversal.JunctionId) return false;
            return true;
        }

        private static bool CoveredByOwnGrant(JunctionActorReport actor, List<Grant> kept)
        {
            var traversal = actor.Request.Traversal;
            foreach (var grant in kept)
            {
                if (grant.TrafficId != actor.TrafficId || grant.JunctionId != traversal.JunctionId
                    || grant.ExitCorridorId != traversal.ExitCorridorId) continue;
                if (!grant.Engaged) { if (MatchesTraversal(grant, traversal)) return true; else continue; }
                bool all = true;
                foreach (var movement in traversal.MovementIds) all &= grant.Movements.Contains(movement);
                if (all) return true;
            }
            return false;
        }

        /// <summary>Un grant non engage ne couvre que la meme chaine complete et la meme sortie, pas son seul premier mouvement.</summary>
        private static bool MatchesTraversal(Grant grant, JunctionTraversal traversal)
        {
            if (grant.TraversalId != traversal.FirstMovementId || grant.JunctionId != traversal.JunctionId
                || grant.ExitCorridorId != traversal.ExitCorridorId || grant.Movements.Count != traversal.MovementIds.Count) return false;
            for (int i = 0; i < grant.Movements.Count; i++)
                if (grant.Movements[i] != traversal.MovementIds[i]) return false;
            return true;
        }

        private static bool LiveContains(Grant grant, RoadId movement)
        {
            return grant.Movements.Contains(movement) && (grant.Released == null || !grant.Released.Contains(movement));
        }

        private static bool Intersects(JunctionTraversal traversal, List<RoadId> movements)
        {
            foreach (var movement in movements) if (traversal.Contains(movement)) return true;
            return false;
        }

        private ulong SinceOf(RoadId trafficId, RoadId traversalId, ulong frameId)
        {
            Seniority value;
            return seniority.TryGetValue(trafficId, out value) && value.TraversalId == traversalId ? value.Since : frameId;
        }

        private static Grant NewGrant(RoadId trafficId, JunctionTraversal traversal, ulong since, float reservation, JunctionReason reason)
        {
            return new Grant { TrafficId = trafficId, JunctionId = traversal.JunctionId, TraversalId = traversal.FirstMovementId,
                ExitCorridorId = traversal.ExitCorridorId, Movements = new List<RoadId>(traversal.MovementIds), Since = since,
                Reservation = reservation, Reason = reason };
        }

        private static void Rebuild(Dictionary<RoadId, List<Grant>> live, List<Grant> kept)
        {
            live.Clear();
            foreach (var grant in kept) Bucket(live, grant.JunctionId).Add(grant);
        }

        private static List<T> Bucket<T>(Dictionary<RoadId, List<T>> map, RoadId key)
        {
            List<T> list;
            if (!map.TryGetValue(key, out list)) map.Add(key, list = new List<T>());
            return list;
        }

        private static int CompareGrants(Grant a, Grant b)
        {
            int order = a.TrafficId.CompareTo(b.TrafficId);
            return order != 0 ? order : a.TraversalId.CompareTo(b.TraversalId);
        }

        private static JunctionRecord Record(Grant grant, IReadOnlyList<RoadId> movements, ulong frameId, ulong effective,
            JunctionGrantStatus status, JunctionReason reason, RoadId cause = default(RoadId), RoadId zone = default(RoadId),
            JunctionExitBound exitBound = JunctionExitBound.None)
        {
            return new JunctionRecord(grant.TrafficId, grant.JunctionId, grant.TraversalId, movements, grant.Since, frameId, effective,
                status, reason, cause, zone, exitBound);
        }

        private static JunctionRecord Denied(JunctionActorReport actor, JunctionTraversal traversal, ulong since, ulong frameId,
            ulong effective, JunctionReason reason, RoadId cause = default(RoadId), RoadId zone = default(RoadId),
            JunctionExitBound exitBound = JunctionExitBound.None)
        {
            return new JunctionRecord(actor.TrafficId, traversal.JunctionId, traversal.FirstMovementId, traversal.MovementIds, since,
                frameId, effective, JunctionGrantStatus.Denied, reason, cause, zone, exitBound);
        }

        private static JunctionSnapshot Publish(ulong frameId, ulong effective, List<JunctionRecord> records, bool frameValid,
            int actors, int occupants, int requests, int valid, int entered, int incompatible, long pairs)
        {
            int granted = 0, held = 0, denied = 0, revoked = 0, released = 0;
            foreach (var record in records)
            {
                if (record.Status == JunctionGrantStatus.Granted) granted++;
                else if (record.Status == JunctionGrantStatus.Held) held++;
                else if (record.Status == JunctionGrantStatus.Denied) denied++;
                else if (record.Status == JunctionGrantStatus.Revoked) revoked++;
                else released++;
            }
            return new JunctionSnapshot(frameId, effective, records, new JunctionBatchCounters(frameValid, actors, occupants, requests,
                valid, granted, held, denied, revoked, released, entered, incompatible, pairs));
        }
    }
}
