using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;

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
    /// 2. les nouvelles demandes, triees par (RequestSinceFrame, TrafficId) : sortie, feu (Story 5.36), StopRequired, occupant protege, grant
    ///    incompatible (Crossing strict ; Merge admis seulement par GrantedMergeGap), YieldToPriority, demande plus ancienne
    ///    refusee pour conflit (sauf si le demandeur a preseance sur elle), sinon Granted ; puis, par carrefour, le briseur
    ///    d'interblocage quand aucune progression n'est possible (Story 5.35) ;
    /// 3. la publication d'un instantane immuable effectif a N+1, qui expire apres N+1.
    /// La preseance (Story 5.35) ne s'evalue qu'entre traversees incompatibles (co-appartenance a une zone compilee) et se lit
    /// sur les controles de leur premier mouvement : deux traversees compatibles ne se refusent jamais.
    /// Le feu (Story 5.36) est une regle, lue dans la frame du lot sur le premier mouvement de la traversee : seul Green autorise un
    /// grant ; Yellow ou Red refusent la demande et revoquent un grant non engage (SignalStop), un etat absent est un refus
    /// (SignalUnavailable). Un refus de feu ne reserve contre personne et n'entre jamais dans le briseur ; un vert ne leve ni la
    /// protection d'occupant, ni l'exclusivite des conflits, ni la sortie.
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
            /// <summary>Story 5.35 : admis par creneau de fusion, publie tant que le grant vit.</summary>
            public bool MergeGap;
            /// <summary>Story 5.35 (trace) : creneau decisif a l'emission (NaN sans creneau evalue).</summary>
            public float Gap = float.NaN, Eta = float.NaN;
            // Etat du lot courant.
            public bool Gone, Covered, Fresh;
            public List<RoadId> Released;
            public JunctionReason ReleaseReason;
        }

        private struct Seniority
        {
            public RoadId TraversalId;
            public ulong Since;
            /// <summary>Story 5.35 : arret marque devant un Stop, vivant aussi longtemps que l'anciennete (jamais de minuterie).</summary>
            public bool StopMarked;
        }

        private struct MergeConflict
        {
            public int RequestIndex;
            public RoadId HeldMovement, Zone;
            public float HolderContactStart;
        }

        // Compteurs du lot courant (Story 5.35).
        private int batchStopRequired, batchYield, batchMergeGaps, batchDeadlockBreaks, batchCrossingRefusals, batchMergeGapRefusals;
        // Trace de la demande examinee (Story 5.35) : creneau a la marge ETA - t_gap la plus faible ; NaN hors examen.
        private float traceGap = float.NaN, traceEta = float.NaN;

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
        // Frame du lot courant (Story 5.36) : seule source de l'etat des feux ; nulle hors lot ou sans frame.
        private TrafficFrame signalFrame;
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
        /// <param name="frame">Frame N du lot, source de l'etat des feux ; nulle : tout mouvement Signalized est SignalUnavailable.</param>
        public JunctionSnapshot Resolve(ulong frameId, IReadOnlyList<JunctionActorReport> reports, TrafficFrame frame = null)
        {
            if (frame != null && (frame.FrameId != frameId || frame.Model != Model)) throw new ArgumentException("FrameMismatch", "frame");
            signalFrame = frame;
            try { return ResolveBatch(frameId, reports); }
            finally { signalFrame = null; }
        }

        private JunctionSnapshot ResolveBatch(ulong frameId, IReadOnlyList<JunctionActorReport> reports)
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
            batchStopRequired = batchYield = batchMergeGaps = batchDeadlockBreaks = batchCrossingRefusals = batchMergeGapRefusals = 0;
            var nextSeniority = new Dictionary<RoadId, Seniority>();
            foreach (var actor in actors)
            {
                if (actor.HasRequest) requests++;
                if (!actor.RequestValid) continue;
                valid++;
                TrafficV2WorkCounters.Work.JunctionRequests++;
                var traversal = actor.Request.Traversal.FirstMovementId;
                Seniority previous;
                var next = seniority.TryGetValue(actor.TrafficId, out previous) && previous.TraversalId == traversal
                    ? previous : new Seniority { TraversalId = traversal, Since = frameId };
                if (Index.ControlKindOf(traversal) == JunctionControlKind.Stop && HaltedAtBoundary(actor.Request)) next.StopMarked = true;
                nextSeniority[actor.TrafficId] = next;
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
                // Story 5.36 : feu non vert, le titulaire peut encore s'arreter (grant non engage).
                var signal = SignalReason(grant.TraversalId);
                if (signal != JunctionReason.None)
                {
                    records.Add(Record(grant, grant.Movements, frameId, effective, JunctionGrantStatus.Revoked, signal));
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

            // Story 5.35 : acteurs prioritaires potentiels par carrefour, seau construit une fois. Derniere approche (valide ou TooFar),
            // hors engagee ; un acteur dont la sortie est insuffisante au lot ne compte pas.
            var threats = new Dictionary<RoadId, List<JunctionActorReport>>();
            foreach (var actor in actors)
            {
                if (!actor.HasRequest || actor.Request.Engaged) continue;
                if (!actor.RequestValid && actor.Rejection != JunctionRequestRejection.TooFar) continue;
                JunctionExitBound ignored;
                if (!ExitSufficient(actor.Request, actor.TrafficId, kept, byId, out ignored)) continue;
                Bucket(threats, actor.Request.Traversal.JunctionId).Add(actor);
            }
            var yielded = new Dictionary<RoadId, List<KeyValuePair<JunctionActorReport, RoadId>>>();

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
                bool mergeGap = false;
                JunctionReason signal;
                traceGap = traceEta = float.NaN;
                if (!Known(traversal))
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.InvalidRequest));
                else if (!ExitSufficient(actor.Request, actor.TrafficId, kept, byId, out bound))
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.ExitBlocked, exitBound: bound));
                else if ((signal = SignalReason(traversal.FirstMovementId)) != JunctionReason.None)
                    // Story 5.36 : regle, hors reservation d'ancien et hors briseur ; aucun grant ne nait sur un feu non vert.
                    records.Add(Denied(actor, traversal, since, frameId, effective, signal));
                else if (Index.ControlKindOf(traversal.FirstMovementId) == JunctionControlKind.Stop && !seniority[actor.TrafficId].StopMarked)
                {
                    batchStopRequired++;
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.StopRequired));
                    Bucket(refused, traversal.JunctionId).Add(new Pending { Actor = actor.TrafficId, Traversal = traversal });
                }
                else if (ConflictsWithOccupants(traversal, actor.TrafficId, occupants, out cause, out zone))
                {
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.ConflictOccupied, cause, zone));
                    Bucket(refused, traversal.JunctionId).Add(new Pending { Actor = actor.TrafficId, Traversal = traversal });
                }
                else if (BlockedByGrants(actor, live, byId, out cause, out zone, out mergeGap))
                {
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.ConflictGranted, cause, zone));
                    Bucket(refused, traversal.JunctionId).Add(new Pending { Actor = actor.TrafficId, Traversal = traversal });
                }
                else if (YieldsToPriority(actor, threats, out cause, out zone))
                {
                    batchYield++;
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.YieldToPriority, cause, zone));
                    // P3(b) : l'anciennete reste acquise, mais une demande qui cede ne reserve pas contre une progression normale.
                    Bucket(yielded, traversal.JunctionId).Add(new KeyValuePair<JunctionActorReport, RoadId>(actor, cause));
                }
                else if (ConflictsWithSeniors(traversal, actor.TrafficId, refused, out cause, out zone))
                {
                    records.Add(Denied(actor, traversal, since, frameId, effective, JunctionReason.SeniorRequestPending, cause, zone));
                    Bucket(refused, traversal.JunctionId).Add(new Pending { Actor = actor.TrafficId, Traversal = traversal });
                }
                else
                {
                    var grant = NewGrant(actor.TrafficId, traversal, since, actor.Request.Exit.RequiredMeters,
                        mergeGap ? JunctionReason.GrantedMergeGap : JunctionReason.Granted);
                    grant.Fresh = true;
                    grant.MergeGap = mergeGap;
                    grant.Gap = traceGap; grant.Eta = traceEta;
                    if (mergeGap) { batchMergeGaps++; TrafficV2WorkCounters.Work.JunctionMergeGapGrants++; }
                    kept.Add(grant);
                    Bucket(live, grant.JunctionId).Add(grant);
                }
            }

            traceGap = traceEta = float.NaN;
            BreakDeadlocks(yielded, threats, live, occupants, kept, records, frameId, effective);

            // 3. Publication : tout grant vivant est republie, sinon il expire.
            kept.Sort(CompareGrants);
            foreach (var grant in kept)
                records.Add(Record(grant, grant.Movements, frameId, effective, IsNewGrant(grant.Reason) ? JunctionGrantStatus.Granted
                    : JunctionGrantStatus.Held, grant.Reason));
            grants = kept;
            Current = Publish(frameId, effective, records, true, actors.Count, occupantCount, requests, valid, entered, incompatible,
                TrafficV2WorkCounters.Work.JunctionPairChecks - pairStart, batchStopRequired, batchYield, batchMergeGaps,
                batchDeadlockBreaks, batchCrossingRefusals, batchMergeGapRefusals);
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

        /// <summary>
        /// Reservation d'ancien (5.34), filtree par la preseance (5.35) : une demande refusee plus ancienne ne bloque jamais un
        /// demandeur plus jeune qui a preseance sur elle.
        /// </summary>
        private bool ConflictsWithSeniors(JunctionTraversal traversal, RoadId self, Dictionary<RoadId, List<Pending>> refused,
            out RoadId cause, out RoadId zone)
        {
            cause = RoadId.None; zone = RoadId.None;
            List<Pending> list;
            if (!refused.TryGetValue(traversal.JunctionId, out list)) return false;
            foreach (var senior in list)
            {
                if (senior.Actor == self || Index.HasPrecedence(traversal.FirstMovementId, senior.Traversal.FirstMovementId)) continue;
                foreach (var other in senior.Traversal.MovementIds)
                    foreach (var movement in traversal.MovementIds)
                        if (Index.TryGetConflict(movement, other, out zone)) { cause = senior.Actor; return true; }
            }
            return false;
        }

        /// <summary>
        /// Grants incompatibles tenus (Story 5.35, P10) : une zone Crossing avec un mouvement non libere d'un titulaire refuse
        /// strictement ; des zones toutes Merge refusent sauf si le creneau de fusion est prouve contre ce titulaire
        /// (<paramref name="mergeGap"/> vrai si au moins un titulaire est ainsi admis).
        /// </summary>
        private bool BlockedByGrants(JunctionActorReport requester, Dictionary<RoadId, List<Grant>> live,
            Dictionary<RoadId, JunctionActorReport> byId, out RoadId cause, out RoadId zone, out bool mergeGap)
        {
            cause = RoadId.None; zone = RoadId.None; mergeGap = false;
            var traversal = requester.Request.Traversal;
            List<Grant> list;
            if (!live.TryGetValue(traversal.JunctionId, out list)) return false;
            foreach (var grant in list)
            {
                if (grant.TrafficId == requester.TrafficId) continue;
                List<MergeConflict> merges = null;
                RoadId first = RoadId.None;
                bool strict = false;
                foreach (var held in grant.Movements)
                {
                    // Un mouvement deja libere a ce lot ne bloque plus rien.
                    if (grant.Released != null && grant.Released.Contains(held)) continue;
                    for (int i = 0; i < traversal.MovementIds.Count; i++)
                    {
                        RoadId z;
                        ConflictKind kind;
                        float startRequest, startHolder;
                        if (!Index.TryGetConflict(traversal.MovementIds[i], held, out z, out kind, out startRequest, out startHolder)) continue;
                        if (first.IsEmpty) first = z;
                        if (kind != ConflictKind.Merge) { strict = true; first = z; break; }
                        if (merges == null) merges = new List<MergeConflict>();
                        merges.Add(new MergeConflict { RequestIndex = i, HeldMovement = held, Zone = z, HolderContactStart = startHolder });
                    }
                    if (strict) break;
                }
                if (first.IsEmpty) continue;
                cause = grant.TrafficId; zone = first;
                if (strict) { batchCrossingRefusals++; return true; }
                if (!MergeGapAdmits(requester, grant, merges, byId)) { batchMergeGapRefusals++; return true; }
                mergeGap = true;
            }
            cause = RoadId.None; zone = RoadId.None;
            return false;
        }

        /// <summary>
        /// GrantedMergeGap (P10) : titulaire present a la frame courante, localise, hors repli, cinematique connue ; pour chaque
        /// fusion, son mouvement de fusion encore devant lui et son ETA jusqu'au debut de contact &gt;= t_gap du demandeur
        /// (degagement de cette zone de fusion). Toute donnee absente ou ambigue refuse.
        /// </summary>
        private bool MergeGapAdmits(JunctionActorReport requester, Grant grant, List<MergeConflict> merges,
            Dictionary<RoadId, JunctionActorReport> byId)
        {
            JunctionActorReport holder;
            if (merges == null || !byId.TryGetValue(grant.TrafficId, out holder) || !holder.Localized || holder.InFallback
                || !holder.Kinematics.Known) return false;
            foreach (var merge in merges)
            {
                TrafficV2WorkCounters.Work.JunctionGapEvaluations++;
                JunctionApproach approach;
                float start;
                if (holder.StatusOf(merge.HeldMovement) != JunctionMovementStatus.Ahead
                    || !holder.TryGetApproachContaining(merge.HeldMovement, out approach)
                    || !approach.TryGetMovementStart(merge.HeldMovement, out start)) return false;
                var k = holder.Kinematics;
                float eta = JunctionPriority.EarliestArrivalSeconds(start + merge.HolderContactStart, k.SpeedMetersPerSecond,
                    k.MaxAccelerationMetersPerSecondSquared, k.DesiredSpeedMetersPerSecond);
                float gap = GapSeconds(requester, merge.RequestIndex);
                Trace(gap, eta);
                if (eta < gap) return false;
            }
            return true;
        }

        /// <summary>
        /// YieldToPriority (P6) : un acteur P dont la traversee approchee est incompatible avec celle du demandeur et a preseance sur
        /// elle arrive au debut de son premier mouvement en conflit avant la fin du creneau du demandeur (ETA_P &lt; t_gap).
        /// </summary>
        private bool YieldsToPriority(JunctionActorReport requester, Dictionary<RoadId, List<JunctionActorReport>> threats,
            out RoadId cause, out RoadId zone, HashSet<RoadId> ignoredCauses = null)
        {
            cause = RoadId.None; zone = RoadId.None;
            var traversal = requester.Request.Traversal;
            List<JunctionActorReport> list;
            if (!threats.TryGetValue(traversal.JunctionId, out list)) return false;
            foreach (var other in list)
            {
                if (other.TrafficId == requester.TrafficId || (ignoredCauses != null && ignoredCauses.Contains(other.TrafficId))) continue;
                var approached = other.Request.Traversal;
                if (!Index.HasPrecedence(approached.FirstMovementId, traversal.FirstMovementId)) continue;
                var pair = Index.PairOf(traversal, approached);
                // Traversees compatibles : aucune preseance, aucun creneau evalue (P9).
                if (pair.LastRequest < 0) continue;
                TrafficV2WorkCounters.Work.JunctionGapEvaluations++;
                float etaP = Eta(other, pair.FirstOther), gap = GapSeconds(requester, pair.LastRequest);
                Trace(gap, etaP);
                if (etaP < gap) { cause = other.TrafficId; zone = pair.Zone; return true; }
            }
            return false;
        }

        /// <summary>
        /// Briseur d'interblocage (P3), par carrefour, a la fin du lot : aucun grant tenu ou emis, aucun occupant protege, et un
        /// ensemble W non vide de demandes refusees seulement YieldToPriority dont chaque cause est dans W. Accorde alors au plus
        /// un grant, au plus ancien membre de W (RequestSinceFrame, puis TrafficId), raison GrantedDeadlockBreak.
        /// </summary>
        private void BreakDeadlocks(Dictionary<RoadId, List<KeyValuePair<JunctionActorReport, RoadId>>> yielded,
            Dictionary<RoadId, List<JunctionActorReport>> threats, Dictionary<RoadId, List<Grant>> live, Dictionary<RoadId, List<Occupation>> occupants, List<Grant> kept,
            List<JunctionRecord> records, ulong frameId, ulong effective)
        {
            var junctions = new List<RoadId>(yielded.Keys);
            junctions.Sort();
            foreach (var junction in junctions)
            {
                List<Grant> holders;
                List<Occupation> occupying;
                if (live.TryGetValue(junction, out holders) && holders.Count > 0) continue;
                if (occupants.TryGetValue(junction, out occupying) && occupying.Count > 0) continue;
                var members = yielded[junction];
                var ids = new HashSet<RoadId>();
                foreach (var member in members) ids.Add(member.Key.TrafficId);
                bool closed = true;
                foreach (var member in members) closed &= ids.Contains(member.Value);
                if (!closed) continue;
                // La raison publiee ne cite que le premier acteur. Une autre cause exterieure interdit le briseur.
                foreach (var member in members)
                {
                    RoadId outside, zone;
                    if (YieldsToPriority(member.Key, threats, out outside, out zone, ids)) { closed = false; break; }
                }
                traceGap = traceEta = float.NaN;
                if (!closed) continue;
                JunctionActorReport chosen = null;
                foreach (var member in members)
                {
                    if (chosen == null) { chosen = member.Key; continue; }
                    int order = seniority[member.Key.TrafficId].Since.CompareTo(seniority[chosen.TrafficId].Since);
                    if (order < 0 || (order == 0 && member.Key.TrafficId.CompareTo(chosen.TrafficId) < 0)) chosen = member.Key;
                }
                var traversal = chosen.Request.Traversal;
                records.RemoveAll(r => r.TrafficId == chosen.TrafficId && r.TraversalId == traversal.FirstMovementId
                    && r.Reason == JunctionReason.YieldToPriority);
                var grant = NewGrant(chosen.TrafficId, traversal, seniority[chosen.TrafficId].Since, chosen.Request.Exit.RequiredMeters,
                    JunctionReason.GrantedDeadlockBreak);
                grant.Fresh = true;
                kept.Add(grant);
                Bucket(live, junction).Add(grant);
                batchDeadlockBreaks++;
                batchYield--;
                TrafficV2WorkCounters.Work.JunctionDeadlockBreaks++;
            }
        }

        /// <summary>t_gap du demandeur pour degager jusqu'a la fin de son mouvement de rang <paramref name="lastIndex"/> ; +inf si inconnu.</summary>
        private float GapSeconds(JunctionActorReport requester, int lastIndex)
        {
            var approach = requester.Request;
            var k = requester.Kinematics;
            if (!k.Known || approach.MovementStartMeters == null || approach.MovementStartMeters.Count <= lastIndex) return float.PositiveInfinity;
            // ponytail: plafond = min des plafonds des mouvements jusqu'au degagement (corridors d'anneau intermediaires non lus),
            // conservatif tant qu'un mouvement d'anneau est au moins aussi courbe que les corridors qui le relient.
            float length, curvature;
            Index.ClearanceOf(approach.Traversal, lastIndex, out length, out curvature);
            float cap = JunctionPriority.MovementSpeedCap(k.DesiredSpeedMetersPerSecond, k.LateralAccelerationMetersPerSecondSquared, curvature);
            float distance = approach.MovementStartMeters[lastIndex] + length + k.LengthMeters;
            float clear = JunctionPriority.TravelSeconds(distance, k.SpeedMetersPerSecond, k.MaxAccelerationMetersPerSecondSquared, cap);
            return JunctionPriority.GapSeconds(clear, approach.Distances.LatencySeconds);
        }

        /// <summary>ETA_P jusqu'au debut de son mouvement de rang <paramref name="index"/> (acceleration a jusqu'a v0) ; 0 si inconnu.</summary>
        private static float Eta(JunctionActorReport actor, int index)
        {
            var approach = actor.Request;
            var k = actor.Kinematics;
            if (!k.Known || approach.MovementStartMeters == null || approach.MovementStartMeters.Count <= index) return 0f;
            return JunctionPriority.EarliestArrivalSeconds(approach.MovementStartMeters[index], k.SpeedMetersPerSecond,
                k.MaxAccelerationMetersPerSecondSquared, k.DesiredSpeedMetersPerSecond);
        }

        /// <summary>Story 5.36 : None si le premier mouvement n'est pas Signalized ou si son feu est Green dans la frame du lot.</summary>
        private JunctionReason SignalReason(RoadId firstMovementId)
        {
            if (Index.ControlKindOf(firstMovementId) != JunctionControlKind.Signalized) return JunctionReason.None;
            SignalState state;
            if (signalFrame == null || !signalFrame.TryGetSignalState(firstMovementId, out state)) return JunctionReason.SignalUnavailable;
            return state == SignalState.Green ? JunctionReason.None : JunctionReason.SignalStop;
        }

        /// <summary>Arret marque (P6) : vitesse tangentielle sous le seuil declare, pare-chocs dans la fenetre d'arret de la frontiere.</summary>
        private static bool HaltedAtBoundary(JunctionApproach approach)
        {
            var distances = approach.Distances;
            return distances.SpeedMetersPerSecond <= JunctionPriority.StopHaltSpeedMetersPerSecond
                && approach.DistanceMeters >= Math.Max(0f, distances.ControlMarginMeters - JunctionPriority.StopHaltIntegrationToleranceMeters)
                && approach.DistanceMeters <= Math.Max(distances.HoldWindowMeters, distances.ControlMarginMeters);
        }

        /// <summary>Retient le creneau a la marge ETA - t_gap la plus faible de la demande examinee (trace seulement).</summary>
        private void Trace(float gap, float eta)
        {
            if (float.IsNaN(traceGap) || eta - gap < traceEta - traceGap) { traceGap = gap; traceEta = eta; }
        }

        private bool StopMarkedOf(RoadId trafficId)
        {
            Seniority entry;
            return seniority.TryGetValue(trafficId, out entry) && entry.StopMarked;
        }

        private static bool IsNewGrant(JunctionReason reason)
        {
            return reason == JunctionReason.Granted || reason == JunctionReason.GrantedMergeGap || reason == JunctionReason.GrantedDeadlockBreak;
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

        private JunctionRecord Record(Grant grant, IReadOnlyList<RoadId> movements, ulong frameId, ulong effective,
            JunctionGrantStatus status, JunctionReason reason, RoadId cause = default(RoadId), RoadId zone = default(RoadId),
            JunctionExitBound exitBound = JunctionExitBound.None)
        {
            return new JunctionRecord(grant.TrafficId, grant.JunctionId, grant.TraversalId, movements, grant.Since, frameId, effective,
                status, reason, cause, zone, exitBound, grant.MergeGap, Index.ControlKindOf(grant.TraversalId), StopMarkedOf(grant.TrafficId),
                grant.Gap, grant.Eta);
        }

        private JunctionRecord Denied(JunctionActorReport actor, JunctionTraversal traversal, ulong since, ulong frameId,
            ulong effective, JunctionReason reason, RoadId cause = default(RoadId), RoadId zone = default(RoadId),
            JunctionExitBound exitBound = JunctionExitBound.None)
        {
            return new JunctionRecord(actor.TrafficId, traversal.JunctionId, traversal.FirstMovementId, traversal.MovementIds, since,
                frameId, effective, JunctionGrantStatus.Denied, reason, cause, zone, exitBound, false,
                Index.ControlKindOf(traversal.FirstMovementId), StopMarkedOf(actor.TrafficId), traceGap, traceEta);
        }

        private static JunctionSnapshot Publish(ulong frameId, ulong effective, List<JunctionRecord> records, bool frameValid,
            int actors, int occupants, int requests, int valid, int entered, int incompatible, long pairs, int stopRequired = 0,
            int yieldToPriority = 0, int mergeGaps = 0, int deadlockBreaks = 0, int crossingRefusals = 0, int mergeGapRefusals = 0)
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
                valid, granted, held, denied, revoked, released, entered, incompatible, pairs, stopRequired, yieldToPriority, mergeGaps,
                deadlockBreaks, crossingRefusals, mergeGapRefusals));
        }
    }
}
