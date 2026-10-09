using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Coordination
{
    /// <summary>Statut d'un record de coordination (Story 5.34). Granted et Held sont des grants effectifs.</summary>
    public enum JunctionGrantStatus
    {
        Granted = 0,
        Held = 1,
        Denied = 2,
        Revoked = 3,
        Released = 4
    }

    /// <summary>Raison stable d'un record, assertable par les tests.</summary>
    public enum JunctionReason
    {
        None = 0,
        /// <summary>Nouvelle demande servie a ce lot.</summary>
        Granted = 1,
        /// <summary>Grant engage conserve (titulaire engage ou occupant protege).</summary>
        Committed = 2,
        /// <summary>Grant non engage conserve : demande valide et sortie suffisante.</summary>
        Pending = 3,
        /// <summary>Occupant reel sans grant, compatible avec les autres occupants et grants : protection reconstruite.</summary>
        Restored = 4,
        /// <summary>Lot sur frame invalide : grant engage republie a l'identique.</summary>
        CommittedCarried = 5,
        ExitBlocked = 6,
        ConflictOccupied = 7,
        ConflictGranted = 8,
        SeniorRequestPending = 9,
        InvalidRequest = 10,
        /// <summary>Occupant reel sans grant, incompatible : jamais servi, mais il bloque les mouvements incompatibles.</summary>
        IncompatibleOccupancy = 11,
        ActorGone = 12,
        RequestWithdrawn = 13,
        FrameUnavailable = 14,
        Cleared = 15,
        ClearedUnlocalized = 16,
        MovementCleared = 17,
        /// <summary>Story 5.35 : controle Stop, arret marque pas encore observe (vitesse et fenetre de la frontiere).</summary>
        StopRequired = 18,
        /// <summary>Story 5.35 : un acteur incompatible prioritaire (cause) arrive avant la fin du creneau du demandeur.</summary>
        YieldToPriority = 19,
        /// <summary>Story 5.35 : grant emis malgre un grant incompatible tenu, toutes les zones entre eux etant Merge et le creneau suffisant.</summary>
        GrantedMergeGap = 20,
        /// <summary>Story 5.35 : briseur d'interblocage, aucune progression possible par la regle.</summary>
        GrantedDeadlockBreak = 21,
        /// <summary>Story 5.36 : feu du controle d'approche Yellow ou Red ; un grant non engage est revoque, un engage conserve.</summary>
        SignalStop = 22,
        /// <summary>Story 5.36 : aucun etat de feu dans la frame du lot pour un mouvement Signalized (fail-closed, jamais un vert).</summary>
        SignalUnavailable = 23,
        /// <summary>Story 5.40 : grant d'escalade d'un interblocage detecte, par un palier authore (G5-G6).</summary>
        GrantedGridlockEscalation = 24
    }

    /// <summary>Ce qui a borne la recherche de sortie.</summary>
    public enum JunctionExitBound
    {
        None = 0,
        /// <summary>SMin du premier occupant V2 apres la traversee.</summary>
        Occupant = 1,
        /// <summary>Entree d'un mouvement d'un carrefour futur : la recherche ne le traverse jamais.</summary>
        ExitSearchBound = 2,
        /// <summary>Abscisse du portail de sortie de la route atteinte avant tout occupant : suffisante.</summary>
        ExitPortal = 3,
        /// <summary>Fin de route sans portail.</summary>
        RouteEnd = 4,
        /// <summary>Longueur libre reduite par les grants deja emis vers le meme corridor de sortie.</summary>
        Reservations = 5
    }

    /// <summary>Pourquoi un vehicule n'emet pas de demande valide a cette frame.</summary>
    public enum JunctionRequestRejection
    {
        None = 0,
        NoTraversal = 1,
        NotLocalized = 2,
        NoOccupancy = 3,
        Fallback = 4,
        TooFar = 5,
        NotHeadOfQueue = 6,
        NoDriver = 7
    }

    /// <summary>Position d'un mouvement de la route par rapport a l'occupation structuree du vehicule.</summary>
    public enum JunctionMovementStatus
    {
        /// <summary>Hors de la fenetre de route examinee : la liberation se juge alors sur la frontiere du carrefour.</summary>
        Unknown = 0,
        Ahead = 1,
        Occupied = 2,
        /// <summary>L'arriere du vehicule a depasse la fin du mouvement : libere.</summary>
        Behind = 3
    }

    public readonly struct JunctionMovementPosition
    {
        public readonly RoadId MovementId;
        public readonly JunctionMovementStatus Status;

        public JunctionMovementPosition(RoadId movementId, JunctionMovementStatus status)
        {
            MovementId = movementId; Status = status;
        }
    }

    /// <summary>
    /// Traversee : chaine des mouvements consecutifs d'un meme carrefour sur la route, avec les corridors qui les separent,
    /// jusqu'au premier corridor que la route quitte sans entrer dans un autre mouvement de ce carrefour (corridor de
    /// sortie). Son identite est son premier mouvement. Accordee ou refusee en bloc.
    /// </summary>
    public sealed class JunctionTraversal
    {
        private readonly RoadId[] movements;
        internal readonly int GeometryKeyHash;

        public RoadId JunctionId { get; }
        public IReadOnlyList<RoadId> MovementIds { get; }
        public RoadId FirstMovementId { get { return movements[0]; } }
        public RoadId LastMovementId { get { return movements[movements.Length - 1]; } }
        /// <summary>Corridor de sortie ; None si la route s'arrete apres le dernier mouvement.</summary>
        public RoadId ExitCorridorId { get; }

        public JunctionTraversal(RoadId junctionId, IReadOnlyList<RoadId> movementIds, RoadId exitCorridorId)
        {
            if (movementIds == null || movementIds.Count == 0) throw new ArgumentException("EmptyTraversal", "movementIds");
            movements = new RoadId[movementIds.Count];
            int hash = junctionId.GetHashCode();
            for (int i = 0; i < movements.Length; i++)
            { movements[i] = movementIds[i]; unchecked { hash = hash * 31 + movements[i].GetHashCode(); } }
            GeometryKeyHash = hash;
            JunctionId = junctionId;
            MovementIds = Array.AsReadOnly(movements);
            ExitCorridorId = exitCorridorId;
        }

        public bool Contains(RoadId movementId) { return Array.IndexOf(movements, movementId) >= 0; }

        public string ToText()
        {
            var text = new StringBuilder();
            text.Append(JunctionId).Append(" [");
            for (int i = 0; i < movements.Length; i++) text.Append(i == 0 ? "" : ",").Append(movements[i]);
            return text.Append("] sortie ").Append(ExitCorridorId).ToString();
        }
    }

    /// <summary>Recherche de sortie bornee : longueur libre apres le dernier mouvement et ce qui l'a bornee.</summary>
    public readonly struct JunctionExitAssessment
    {
        /// <summary>Longueur libre cumulee (m) ; +inf quand le portail de sortie de la route est atteint d'abord.</summary>
        public readonly float FreeLengthMeters;
        public readonly JunctionExitBound Bound;
        /// <summary>Occupant ou mouvement en borne ; None sinon.</summary>
        public readonly RoadId BoundId;
        /// <summary>L + s0 du demandeur : longueur exigee, et reservation de son grant pour les suivants.</summary>
        public readonly float RequiredMeters;

        public JunctionExitAssessment(float freeLengthMeters, JunctionExitBound bound, RoadId boundId, float requiredMeters)
        {
            FreeLengthMeters = freeLengthMeters; Bound = bound; BoundId = boundId; RequiredMeters = requiredMeters;
        }

        public string ToText()
        {
            return "sortie libre " + JunctionText.F(FreeLengthMeters) + " / exige " + JunctionText.F(RequiredMeters) + " / borne " + Bound
                + (BoundId.IsEmpty ? "" : " @" + BoundId);
        }
    }

    /// <summary>
    /// Faits d'approche d'un vehicule pour une traversee a une frame : distance d du pare-chocs avant a l'entree, distances
    /// derivees, tete de file, grant effectif lu dans l'instantane de la frame, engagement et sortie.
    /// </summary>
    public readonly struct JunctionApproach
    {
        public readonly JunctionTraversal Traversal;
        /// <summary>d : pare-chocs avant -> entree du premier mouvement (negatif une fois entre).</summary>
        public readonly float DistanceMeters;
        public readonly JunctionDistances Distances;
        public readonly bool HeadOfQueue;
        /// <summary>Premier acteur V2 dont l'occupation coupe la route entre le pare-chocs et l'entree ; None sinon.</summary>
        public readonly RoadId MaskingActorId;
        /// <summary>L'instantane de la frame (EffectiveFrame == frame) porte un grant effectif de cette traversee.</summary>
        public readonly bool GrantEffective;
        /// <summary>Grant effectif, et entree franchie ou d &lt; D_stop.</summary>
        public readonly bool Engaged;
        /// <summary>Evaluee pour la traversee demandee seulement ; Bound None sinon.</summary>
        public readonly JunctionExitAssessment Exit;
        /// <summary>
        /// Story 5.35 : distance (m) du pare-chocs avant au debut de chaque mouvement de la traversee, au meme rang que ses
        /// mouvements (negative une fois le debut depasse) ; vide si inconnue (aucun creneau ne se prouve alors).
        /// </summary>
        public readonly IReadOnlyList<float> MovementStartMeters;
        /// <summary>Story 5.35 : frontiere de controle b sur le premier mouvement (s_line, sinon 0) ; d vise b.</summary>
        public readonly float BoundaryMeters;

        public JunctionApproach(JunctionTraversal traversal, float distanceMeters, JunctionDistances distances, bool headOfQueue,
            RoadId maskingActorId, bool grantEffective, bool engaged, JunctionExitAssessment exit,
            IReadOnlyList<float> movementStartMeters = null, float boundaryMeters = 0f)
        {
            if (traversal == null) throw new ArgumentNullException("traversal");
            Traversal = traversal; DistanceMeters = distanceMeters; Distances = distances; HeadOfQueue = headOfQueue;
            MaskingActorId = maskingActorId; GrantEffective = grantEffective; Engaged = engaged; Exit = exit;
            var starts = new float[movementStartMeters == null || movementStartMeters.Count != traversal.MovementIds.Count ? 0 : movementStartMeters.Count];
            for (int i = 0; i < starts.Length; i++) starts[i] = movementStartMeters[i];
            MovementStartMeters = Array.AsReadOnly(starts);
            BoundaryMeters = boundaryMeters;
        }

        /// <summary>Distance du pare-chocs avant au debut du mouvement de la traversee ; faux s'il en est absent ou si elle est inconnue.</summary>
        public bool TryGetMovementStart(RoadId movementId, out float meters)
        {
            meters = 0f;
            if (Traversal == null || MovementStartMeters == null || MovementStartMeters.Count == 0) return false;
            for (int i = 0; i < Traversal.MovementIds.Count; i++)
                if (Traversal.MovementIds[i] == movementId) { meters = MovementStartMeters[i]; return true; }
            return false;
        }

        /// <summary>Pare-chocs avant au-dela de l'entree du premier mouvement.</summary>
        public bool Crossed { get { return DistanceMeters <= 0f; } }

        public string ToText()
        {
            return "traversee " + Traversal.ToText() + " / d " + JunctionText.F(DistanceMeters) + " / " + Distances.ToText()
                + " / tete de file " + (HeadOfQueue ? "oui" : "non" + (MaskingActorId.IsEmpty ? "" : " @" + MaskingActorId))
                + " / grant effectif " + (GrantEffective ? "oui" : "non") + " / engage " + (Engaged ? "oui" : "non")
                + (BoundaryMeters > 0f ? " / b " + JunctionText.F(BoundaryMeters) : "")
                + (Exit.Bound == JunctionExitBound.None ? "" : " / " + Exit.ToText());
        }
    }

    /// <summary>
    /// Rapport d'un acteur V2 present dans la frame, fonction pure de la frame, de sa route et de l'instantane : occupation
    /// structuree projetee sur la route, positions des mouvements, approches (de la premiere traversee a ou devant le
    /// pare-chocs jusqu'a la premiere sans grant engage, decision O8) et demande (au plus une).
    /// </summary>
    public sealed class JunctionActorReport
    {
        private static readonly RoadId[] NoIds = new RoadId[0];
        private static readonly JunctionMovementPosition[] NoPositions = new JunctionMovementPosition[0];
        private static readonly JunctionApproach[] NoApproaches = new JunctionApproach[0];
        private static readonly JunctionTraversal[] NoTraversals = new JunctionTraversal[0];
        private static readonly Vector3[] NoCorners = new Vector3[0];

        private readonly JunctionApproach[] approaches;
        private readonly JunctionMovementPosition[] positions;

        public RoadId TrafficId { get; }
        public bool Localized { get; }
        /// <summary>Element de localisation (None hors localisation).</summary>
        public RoadId ElementId { get; }
        /// <summary>Coins de l'empreinte (liberation d'un titulaire non localise).</summary>
        public IReadOnlyList<Vector3> Corners { get; }
        /// <summary>Mouvements que l'occupation chevauche, tries.</summary>
        public IReadOnlyList<RoadId> OccupiedMovements { get; }
        /// <summary>Traversees de la route qui contiennent un mouvement occupe, chacune depuis son premier mouvement occupe.</summary>
        public IReadOnlyList<JunctionTraversal> OccupiedTraversals { get; }
        public IReadOnlyList<JunctionMovementPosition> MovementPositions { get; }
        /// <summary>Approches ordonnees : traversees engagees puis, si elle existe, la traversee demandee (la derniere).</summary>
        public IReadOnlyList<JunctionApproach> Approaches { get; }
        /// <summary>La derniere approche est une traversee sans grant engage : la demande de la frame, valide ou non.</summary>
        public bool HasRequest { get; }
        public bool RequestValid { get; }
        public JunctionRequestRejection Rejection { get; }
        /// <summary>L + s0 de l'acteur (longueur de son empreinte seule sans profil de conducteur).</summary>
        public float ReservationMeters { get; }
        /// <summary>Story 5.35 : cinematique declaree pour l'acceptation de creneau ; inconnue sans profil.</summary>
        public JunctionKinematics Kinematics { get; }
        /// <summary>Story 5.35 : le pas du vehicule a abouti au repli (aucun creneau ne se prouve avec lui comme titulaire).</summary>
        public bool InFallback { get; }

        public JunctionActorReport(RoadId trafficId, bool localized, RoadId elementId, IReadOnlyList<Vector3> corners,
            IReadOnlyList<RoadId> occupiedMovements, IReadOnlyList<JunctionTraversal> occupiedTraversals,
            IReadOnlyList<JunctionMovementPosition> movementPositions, IReadOnlyList<JunctionApproach> approaches, bool hasRequest,
            bool requestValid, JunctionRequestRejection rejection, float reservationMeters,
            JunctionKinematics kinematics = default(JunctionKinematics), bool inFallback = false)
        {
            if (trafficId.IsEmpty) throw new ArgumentException("EmptyTrafficId", "trafficId");
            TrafficId = trafficId; Localized = localized; ElementId = elementId;
            Corners = Array.AsReadOnly(Copy(corners, NoCorners));
            var occupied = Copy(occupiedMovements, NoIds);
            Array.Sort(occupied);
            OccupiedMovements = Array.AsReadOnly(occupied);
            OccupiedTraversals = Array.AsReadOnly(Copy(occupiedTraversals, NoTraversals));
            positions = Copy(movementPositions, NoPositions);
            MovementPositions = Array.AsReadOnly(positions);
            this.approaches = Copy(approaches, NoApproaches);
            Approaches = Array.AsReadOnly(this.approaches);
            HasRequest = hasRequest && this.approaches.Length > 0;
            RequestValid = HasRequest && requestValid;
            Rejection = RequestValid ? JunctionRequestRejection.None
                : rejection == JunctionRequestRejection.None ? JunctionRequestRejection.NoTraversal : rejection;
            ReservationMeters = reservationMeters;
            Kinematics = kinematics;
            InFallback = inFallback;
        }

        /// <summary>Approche de la traversee demandee ; sans objet si <see cref="HasRequest"/> est faux.</summary>
        public JunctionApproach Request { get { return HasRequest ? approaches[approaches.Length - 1] : default(JunctionApproach); } }

        /// <summary>Approche de la traversee identifiee par son premier mouvement.</summary>
        public bool TryGetApproach(RoadId firstMovementId, out JunctionApproach approach)
        {
            for (int i = 0; i < approaches.Length; i++)
                if (approaches[i].Traversal.FirstMovementId == firstMovementId) { approach = approaches[i]; return true; }
            approach = default(JunctionApproach);
            return false;
        }

        /// <summary>
        /// Position d'un mouvement dans la fenetre du rapport ; un mouvement present plusieurs fois (boucle legale) est occupe si
        /// une occurrence l'est, devant si une occurrence reste devant, derriere seulement si toutes sont derriere.
        /// </summary>
        public JunctionMovementStatus StatusOf(RoadId movementId)
        {
            var status = JunctionMovementStatus.Unknown;
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i].MovementId != movementId) continue;
                var current = positions[i].Status;
                if (current == JunctionMovementStatus.Occupied) return current;
                if (current == JunctionMovementStatus.Ahead || status == JunctionMovementStatus.Unknown) status = current;
            }
            return status;
        }

        /// <summary>
        /// Meme rapport, demande invalidee si le pas du vehicule a abouti au repli (« hors repli ») ; le repli est publie meme
        /// sans demande (titulaire d'un grant, Story 5.35).
        /// </summary>
        public JunctionActorReport WithFallback(bool fallback)
        {
            if (!fallback) return this;
            return new JunctionActorReport(TrafficId, Localized, ElementId, Corners, OccupiedMovements, OccupiedTraversals,
                MovementPositions, Approaches, HasRequest, false, RequestValid ? JunctionRequestRejection.Fallback : Rejection,
                ReservationMeters, Kinematics, true);
        }

        /// <summary>Approche (engagee ou demandee) dont la traversee contient le mouvement ; faux sinon.</summary>
        public bool TryGetApproachContaining(RoadId movementId, out JunctionApproach approach)
        {
            for (int i = 0; i < approaches.Length; i++)
                if (approaches[i].Traversal.Contains(movementId)) { approach = approaches[i]; return true; }
            approach = default(JunctionApproach);
            return false;
        }

        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Junction report ").Append(TrafficId).Append(Localized ? " localise " + ElementId : " non localise")
                .Append(" / occupe ");
            if (OccupiedMovements.Count == 0) text.Append("aucun mouvement");
            for (int i = 0; i < OccupiedMovements.Count; i++) text.Append(i == 0 ? "" : ",").Append(OccupiedMovements[i]);
            text.Append(" / demande ").Append(HasRequest ? (RequestValid ? "valide" : "invalide " + Rejection) : "aucune " + Rejection);
            for (int i = 0; i < approaches.Length; i++) text.Append("\n  ").Append(approaches[i].ToText());
            return text.ToString();
        }

        private static T[] Copy<T>(IReadOnlyList<T> source, T[] empty)
        {
            if (source == null || source.Count == 0) return empty;
            var copy = new T[source.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = source[i];
            return copy;
        }
    }

    /// <summary>
    /// Record publie par un lot : identite de la traversee (son premier mouvement), mouvements concernes, anciennete,
    /// epoques, statut, raison stable et cause (titulaire, occupant ou demandeur plus ancien, zone, borne de sortie).
    /// </summary>
    public readonly struct JunctionRecord
    {
        public readonly RoadId TrafficId;
        public readonly RoadId JunctionId;
        /// <summary>Premier mouvement de la traversee demandee ou accordee : identite stable du grant.</summary>
        public readonly RoadId TraversalId;
        /// <summary>Mouvements du record : restants pour un grant, demandes pour un refus, liberes pour Released.</summary>
        public readonly IReadOnlyList<RoadId> MovementIds;
        public readonly ulong RequestSinceFrame;
        public readonly ulong SourceFrame;
        public readonly ulong EffectiveFrame;
        public readonly ulong ExpiresAfterFrame;
        public readonly JunctionGrantStatus Status;
        public readonly JunctionReason Reason;
        /// <summary>Titulaire (ConflictGranted), occupant (ConflictOccupied, IncompatibleOccupancy) ou demandeur plus ancien.</summary>
        public readonly RoadId CauseActorId;
        public readonly RoadId ZoneId;
        public readonly JunctionExitBound ExitBound;
        /// <summary>
        /// Story 5.35 : grant admis par creneau de fusion (GrantedMergeGap), conserve tant que le grant vit. Seuls de tels grants
        /// peuvent coexister avec un grant incompatible, et seulement sur des zones Merge (invariant 5.34 amende).
        /// </summary>
        public readonly bool MergeGap;
        /// <summary>Story 5.35 (trace) : genre de controle du premier mouvement de la traversee.</summary>
        public readonly JunctionControlKind ControlKind;
        /// <summary>Story 5.35 (trace) : arret marque devant un Stop, vivant avec l'anciennete.</summary>
        public readonly bool StopMarked;
        /// <summary>
        /// Story 5.35 (trace) : t_gap du demandeur et ETA de l'acteur prioritaire ou du titulaire pour l'evaluation de creneau
        /// decisive (marge ETA - t_gap la plus faible du lot) ; NaN si aucun creneau n'a ete evalue.
        /// </summary>
        public readonly float GapSeconds, EtaSeconds;

        public JunctionRecord(RoadId trafficId, RoadId junctionId, RoadId traversalId, IReadOnlyList<RoadId> movementIds,
            ulong requestSinceFrame, ulong sourceFrame, ulong effectiveFrame, JunctionGrantStatus status, JunctionReason reason,
            RoadId causeActorId = default(RoadId), RoadId zoneId = default(RoadId), JunctionExitBound exitBound = JunctionExitBound.None,
            bool mergeGap = false, JunctionControlKind controlKind = JunctionControlKind.Uncontrolled, bool stopMarked = false,
            float gapSeconds = float.NaN, float etaSeconds = float.NaN)
        {
            TrafficId = trafficId; JunctionId = junctionId; TraversalId = traversalId;
            var copy = new RoadId[movementIds == null ? 0 : movementIds.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = movementIds[i];
            MovementIds = Array.AsReadOnly(copy);
            RequestSinceFrame = requestSinceFrame; SourceFrame = sourceFrame; EffectiveFrame = effectiveFrame;
            ExpiresAfterFrame = effectiveFrame; Status = status; Reason = reason; CauseActorId = causeActorId; ZoneId = zoneId;
            ExitBound = exitBound; MergeGap = mergeGap;
            ControlKind = controlKind; StopMarked = stopMarked; GapSeconds = gapSeconds; EtaSeconds = etaSeconds;
        }

        /// <summary>Granted ou Held : grant effectif a <see cref="EffectiveFrame"/>.</summary>
        public bool IsEffectiveGrant { get { return Status == JunctionGrantStatus.Granted || Status == JunctionGrantStatus.Held; } }

        public bool Contains(RoadId movementId)
        {
            for (int i = 0; i < MovementIds.Count; i++) if (MovementIds[i] == movementId) return true;
            return false;
        }

        /// <summary>Texte deterministe et invariant de culture, ex. « Denied(ConflictGranted) ».</summary>
        public string ToText()
        {
            var text = new StringBuilder();
            text.Append(TrafficId).Append(' ').Append(Status).Append('(').Append(Reason).Append(") ").Append(JunctionId)
                .Append(" traversee ").Append(TraversalId).Append(" [");
            for (int i = 0; i < MovementIds.Count; i++) text.Append(i == 0 ? "" : ",").Append(MovementIds[i]);
            text.Append("] depuis ").Append(RequestSinceFrame.ToString(CultureInfo.InvariantCulture))
                .Append(" source ").Append(SourceFrame.ToString(CultureInfo.InvariantCulture))
                .Append(" effectif ").Append(EffectiveFrame.ToString(CultureInfo.InvariantCulture))
                .Append(" expire apres ").Append(ExpiresAfterFrame.ToString(CultureInfo.InvariantCulture));
            if (!CauseActorId.IsEmpty) text.Append(" cause @").Append(CauseActorId);
            if (!ZoneId.IsEmpty) text.Append(" zone ").Append(ZoneId);
            if (ExitBound != JunctionExitBound.None) text.Append(" borne ").Append(ExitBound);
            if (MergeGap) text.Append(" creneau de fusion");
            // Champs 5.35 en suffixe, absents pour un carrefour Uncontrolled sans creneau (texte 5.34 inchange).
            if (ControlKind != JunctionControlKind.Uncontrolled) text.Append(" genre ").Append(ControlKind);
            if (StopMarked) text.Append(" arret marque");
            if (!float.IsNaN(GapSeconds))
                text.Append(" t_gap ").Append(JunctionText.F(GapSeconds)).Append(" ETA ").Append(JunctionText.F(EtaSeconds));
            return text.ToString();
        }
    }

    /// <summary>Compteurs d'un lot, dont EnteredWithoutGrant et IncompatibleOccupancy.</summary>
    public readonly struct JunctionBatchCounters
    {
        public readonly bool FrameValid;
        public readonly int Actors, Occupants, Requests, ValidRequests;
        public readonly int Granted, Held, Denied, Revoked, Released;
        /// <summary>Acteurs qui occupent un mouvement non couvert par un de leurs grants (avant reconstruction).</summary>
        public readonly int EnteredWithoutGrant;
        public readonly int IncompatibleOccupancy;
        /// <summary>Paires de mouvements testees pendant le lot.</summary>
        public readonly long PairChecks;
        /// <summary>Story 5.35 : refus StopRequired et YieldToPriority, grants par creneau de fusion et par briseur.</summary>
        public readonly int StopRequired, YieldToPriority, MergeGapGrants, DeadlockBreaks;
        /// <summary>Story 5.35 : refus ConflictGranted sur une zone Crossing (exclusivite stricte, P11 compris).</summary>
        public readonly int CrossingRefusals;
        /// <summary>Story 5.35 : creneaux de fusion refuses (titulaire non localise, en repli, rapport absent, ou creneau court).</summary>
        public readonly int MergeGapRefusals;

        public JunctionBatchCounters(bool frameValid, int actors, int occupants, int requests, int validRequests, int granted,
            int held, int denied, int revoked, int released, int enteredWithoutGrant, int incompatibleOccupancy, long pairChecks,
            int stopRequired = 0, int yieldToPriority = 0, int mergeGapGrants = 0, int deadlockBreaks = 0, int crossingRefusals = 0,
            int mergeGapRefusals = 0)
        {
            FrameValid = frameValid; Actors = actors; Occupants = occupants; Requests = requests; ValidRequests = validRequests;
            Granted = granted; Held = held; Denied = denied; Revoked = revoked; Released = released;
            EnteredWithoutGrant = enteredWithoutGrant; IncompatibleOccupancy = incompatibleOccupancy; PairChecks = pairChecks;
            StopRequired = stopRequired; YieldToPriority = yieldToPriority; MergeGapGrants = mergeGapGrants; DeadlockBreaks = deadlockBreaks;
            CrossingRefusals = crossingRefusals; MergeGapRefusals = mergeGapRefusals;
        }

        public string ToText()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "lot {0} / acteurs {1} occupants {2} demandes {3} valides {4} / Granted {5} Held {6} Denied {7} Revoked {8} Released {9}"
                + " / EnteredWithoutGrant {10} IncompatibleOccupancy {11} / paires {12} / StopRequired {13} YieldToPriority {14}"
                + " MergeGap {15} DeadlockBreak {16} CrossingRefusals {17} MergeGapRefusals {18}",
                FrameValid ? "valide" : "frame invalide", Actors, Occupants, Requests, ValidRequests, Granted, Held, Denied, Revoked,
                Released, EnteredWithoutGrant, IncompatibleOccupancy, PairChecks, StopRequired, YieldToPriority, MergeGapGrants,
                DeadlockBreaks, CrossingRefusals, MergeGapRefusals);
        }
    }

    /// <summary>
    /// Instantane immuable publie par un lot : records effectifs a <see cref="EffectiveFrame"/> = N+1. Un instantane lu a une
    /// autre frame que son EffectiveFrame vaut « aucun grant » : aucun vehicule ne lit un grant non publie.
    /// </summary>
    public sealed class JunctionSnapshot
    {
        private readonly JunctionRecord[] records;

        public ulong SourceFrame { get; }
        public ulong EffectiveFrame { get; }
        public IReadOnlyList<JunctionRecord> Records { get; }
        public JunctionBatchCounters Counters { get; }
        /// <summary>Story 5.40 (G7) : une resolution par cycle soumis au lot, triee par cle ; vide sans cycle.</summary>
        public IReadOnlyList<GridlockResolution> Gridlocks { get; }
        /// <summary>Story 5.41 (P5) : decisions d'exception du lot, triees par (demandeur, regle, statut) ; vide sans exception.</summary>
        public IReadOnlyList<RuleExceptionRecord> RuleExceptions { get; }

        public JunctionSnapshot(ulong sourceFrame, ulong effectiveFrame, IReadOnlyList<JunctionRecord> records,
            JunctionBatchCounters counters, IReadOnlyList<GridlockResolution> gridlocks = null,
            IReadOnlyList<RuleExceptionRecord> ruleExceptions = null)
        {
            SourceFrame = sourceFrame; EffectiveFrame = effectiveFrame; Counters = counters;
            var decided = new RuleExceptionRecord[ruleExceptions == null ? 0 : ruleExceptions.Count];
            for (int i = 0; i < decided.Length; i++) decided[i] = ruleExceptions[i];
            Array.Sort(decided, CompareExceptions);
            RuleExceptions = Array.AsReadOnly(decided);
            var resolved = new GridlockResolution[gridlocks == null ? 0 : gridlocks.Count];
            for (int i = 0; i < resolved.Length; i++) resolved[i] = gridlocks[i];
            Gridlocks = Array.AsReadOnly(resolved);
            this.records = new JunctionRecord[records == null ? 0 : records.Count];
            for (int i = 0; i < this.records.Length; i++) this.records[i] = records[i];
            Array.Sort(this.records, Compare);
            Records = Array.AsReadOnly(this.records);
        }

        /// <summary>Instantane vide publie avant tout lot.</summary>
        public static JunctionSnapshot Initial(ulong effectiveFrame)
        {
            return new JunctionSnapshot(0UL, effectiveFrame, null, new JunctionBatchCounters(true, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
        }

        /// <summary>Grant effectif de l'acteur qui contient le mouvement, lu a la frame ; faux si l'instantane est decale.</summary>
        public bool TryGetEffectiveGrant(RoadId trafficId, RoadId movementId, ulong frameId, out JunctionRecord record)
        {
            record = default(JunctionRecord);
            if (EffectiveFrame != frameId) return false;
            for (int i = 0; i < records.Length; i++)
                if (records[i].TrafficId == trafficId && records[i].IsEffectiveGrant && records[i].Contains(movementId))
                { record = records[i]; return true; }
            return false;
        }

        /// <summary>Dernier record du lot sur la traversee (grant, refus, revocation ou liberation), grant d'abord.</summary>
        public bool TryGetDecision(RoadId trafficId, RoadId traversalId, out JunctionRecord record)
        {
            record = default(JunctionRecord);
            bool found = false;
            for (int i = 0; i < records.Length; i++)
            {
                if (records[i].TrafficId != trafficId || records[i].TraversalId != traversalId) continue;
                if (!found || (records[i].IsEffectiveGrant && !record.IsEffectiveGrant)) { record = records[i]; found = true; }
            }
            return found;
        }

        /// <summary>
        /// Story 5.41 (P5) : exceptions effectives du vehicule a la frame lue ; faux et liste vide si l'instantane est decale ou
        /// si aucune n'est effective, sans allocation dans ce cas.
        /// </summary>
        public bool TryGetEffectiveExceptions(RoadId trafficId, ulong frameId, out IReadOnlyList<EffectiveRuleException> exceptions)
        {
            exceptions = EffectiveRuleException.None;
            if (EffectiveFrame != frameId) return false;
            List<EffectiveRuleException> found = null;
            for (int i = 0; i < RuleExceptions.Count; i++)
            {
                var record = RuleExceptions[i];
                if (!record.IsEffective || record.Request.Requester != trafficId) continue;
                if (found == null) found = new List<EffectiveRuleException>();
                found.Add(record.Exception);
            }
            if (found == null) return false;
            exceptions = found.AsReadOnly();
            return true;
        }

        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Junction snapshot source ").Append(SourceFrame.ToString(CultureInfo.InvariantCulture)).Append(" effectif ")
                .Append(EffectiveFrame.ToString(CultureInfo.InvariantCulture)).Append(" / ").Append(Counters.ToText());
            for (int i = 0; i < records.Length; i++) text.Append("\n  ").Append(records[i].ToText());
            for (int i = 0; i < Gridlocks.Count; i++) text.Append("\n  ").Append(Gridlocks[i].ToText());
            for (int i = 0; i < RuleExceptions.Count; i++) text.Append("\n  ").Append(RuleExceptions[i].ToText());
            return text.ToString();
        }

        private static int CompareExceptions(RuleExceptionRecord a, RuleExceptionRecord b)
        {
            int order = a.Request.Requester.CompareTo(b.Request.Requester);
            if (order == 0) order = ((int)a.Request.Rule).CompareTo((int)b.Request.Rule);
            if (order == 0) order = ((int)a.Status).CompareTo((int)b.Status);
            return order != 0 ? order : ((int)a.Reason).CompareTo((int)b.Reason);
        }

        private static int Compare(JunctionRecord a, JunctionRecord b)
        {
            int order = a.TrafficId.CompareTo(b.TrafficId);
            if (order != 0) return order;
            order = a.TraversalId.CompareTo(b.TraversalId);
            if (order != 0) return order;
            order = ((int)a.Status).CompareTo((int)b.Status);
            return order != 0 ? order : ((int)a.Reason).CompareTo((int)b.Reason);
        }
    }

    /// <summary>
    /// Story 5.40 : palier d'escalade d'interblocage, authore dans TrafficV2Settings.GridlockEscalationTiers et evalue par le
    /// coordinateur seul. Aucune valeur ne retire, ne teleporte, ne reinsere ni ne deplace un vehicule.
    /// </summary>
    public enum GridlockEscalationTier
    {
        None = 0,
        /// <summary>G6 : ignore seulement YieldToPriority et SeniorRequestPending dont la cause est un membre du cycle.</summary>
        PrecedenceRelaxation = 1
    }

    /// <summary>Story 5.40 : issue d'un cycle soumis a un lot.</summary>
    public enum GridlockOutcome
    {
        /// <summary>Un membre a obtenu un grant neuf par les regles normales ou le briseur 5.35 : aucune escalade.</summary>
        Progressed = 0,
        /// <summary>Un membre a ete servi par un palier.</summary>
        Escalated = 1,
        /// <summary>Aucun palier n'admet aucun membre : pire cas borne, rien n'est retire.</summary>
        Exhausted = 2
    }

    /// <summary>Story 5.40 : arc d'attente A -> B du graphe, avec le genre du blocker qui le porte.</summary>
    public readonly struct GridlockArc
    {
        public readonly RoadId From, To;
        public readonly string Kind;

        public GridlockArc(RoadId from, RoadId to, string kind) { From = from; To = to; Kind = kind; }

        public string ToText() { return From + " -" + Kind + "-> " + To; }
    }

    /// <summary>Story 5.40 : composante fortement connexe (2 membres ou plus) du graphe d'attente ; membres tries.</summary>
    public sealed class GridlockCycle
    {
        public IReadOnlyList<RoadId> Members { get; }
        /// <summary>Arcs internes au cycle, tries par (From, To, Kind).</summary>
        public IReadOnlyList<GridlockArc> Arcs { get; }
        /// <summary>Identite du cycle : ses membres.</summary>
        public string Key { get; }

        public GridlockCycle(IEnumerable<RoadId> members, IEnumerable<GridlockArc> arcs)
        {
            var m = new List<RoadId>(members);
            m.Sort();
            var a = new List<GridlockArc>(arcs ?? new GridlockArc[0]);
            a.Sort((x, y) =>
            {
                int order = x.From.CompareTo(y.From);
                if (order == 0) order = x.To.CompareTo(y.To);
                return order != 0 ? order : string.CompareOrdinal(x.Kind, y.Kind);
            });
            Members = m.AsReadOnly();
            Arcs = a.AsReadOnly();
            var key = new StringBuilder();
            for (int i = 0; i < m.Count; i++) key.Append(i == 0 ? "" : ",").Append(m[i]);
            Key = key.ToString();
        }

        public bool Contains(RoadId id)
        {
            for (int i = 0; i < Members.Count; i++) if (Members[i] == id) return true;
            return false;
        }

        public string ToText()
        {
            var text = new StringBuilder("{").Append(Key).Append('}');
            for (int i = 0; i < Arcs.Count; i++) text.Append(i == 0 ? " " : ", ").Append(Arcs[i].ToText());
            return text.ToString();
        }
    }

    /// <summary>Story 5.40 (G7) : resolution publiee d'un cycle soumis.</summary>
    public readonly struct GridlockResolution
    {
        public readonly GridlockCycle Cycle;
        public readonly GridlockOutcome Outcome;
        public readonly GridlockEscalationTier Tier;
        /// <summary>Membre servi (Escalated) ou progressant (Progressed) ; None si Exhausted.</summary>
        public readonly RoadId Served;

        public GridlockResolution(GridlockCycle cycle, GridlockOutcome outcome, GridlockEscalationTier tier, RoadId served)
        {
            Cycle = cycle; Outcome = outcome; Tier = tier; Served = served;
        }

        public string ToText()
        {
            return "Gridlock " + Cycle.ToText() + " " + Outcome + (Tier == GridlockEscalationTier.None ? "" : " " + Tier)
                + (Served.IsEmpty ? "" : " @" + Served);
        }
    }

    internal static class JunctionText
    {
        public static string F(float value)
        {
            return float.IsNaN(value) ? "NaN" : float.IsPositiveInfinity(value) ? "inf" : float.IsNegativeInfinity(value) ? "-inf"
                : value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
