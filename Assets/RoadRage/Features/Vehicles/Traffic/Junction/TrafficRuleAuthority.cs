using System;
using System.Collections.Generic;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Policy;

namespace RoadRage.Features.Vehicles.Traffic.Coordination
{
    /// <summary>Story 5.41 : statut d'une decision d'exception. Accepted et Held sont effectifs a EffectiveFrame.</summary>
    public enum RuleExceptionStatus
    {
        Accepted = 0,
        Held = 1,
        Denied = 2,
        Ended = 3
    }

    /// <summary>Story 5.41 (P4) : raison stable d'une decision d'exception.</summary>
    public enum RuleExceptionReason
    {
        Accepted = 0,
        Held = 1,
        Malformed = 2,
        RequesterAbsent = 3,
        TargetAbsent = 4,
        RuleNotViolable = 5,
        ScopeInsideJunction = 6,
        AlreadyActive = 7,
        Expired = 8,
        ScopeExited = 9,
        RequesterGone = 10,
        TargetGone = 11,
        FrameUnavailable = 12
    }

    /// <summary>
    /// Story 5.41 : exception acceptee par l'autorite des regles, portee sur le plan du demandeur. Elle nomme regle, portee ou
    /// cible et expiration, pour qu'aucun consommateur ne lise l'etat vivant de la politique. Construite par l'autorite seule.
    /// </summary>
    public sealed class EffectiveRuleException
    {
        public static readonly IReadOnlyList<EffectiveRuleException> None = Array.AsReadOnly(new EffectiveRuleException[0]);

        public RuleExceptionRequest Request { get; }
        /// <summary>Frame N du lot qui l'a acceptee ; effective a partir de N+1.</summary>
        public ulong AcceptedFrame { get; }

        internal EffectiveRuleException(RuleExceptionRequest request, ulong acceptedFrame)
        {
            Request = request; AcceptedFrame = acceptedFrame;
        }

        public RoadId Requester { get { return Request.Requester; } }
        public TrafficRule Rule { get { return Request.Rule; } }
        public ulong ExpiryFrame { get { return Request.Termination.ExpiryFrame; } }

        public string ToText()
        {
            return Rule + (Request.Scope.IsEmpty ? "" : " scope " + Request.Scope) + (Request.Target.IsEmpty ? "" : " target " + Request.Target)
                + " '" + Request.StartReason + "' " + Request.Termination.ToText() + " depuis "
                + AcceptedFrame.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Story 5.41 (P5) : decision publiee d'un lot sur une demande ou une exception active.</summary>
    public readonly struct RuleExceptionRecord
    {
        public readonly RuleExceptionRequest Request;
        public readonly RuleExceptionStatus Status;
        public readonly RuleExceptionReason Reason;
        public readonly ulong SourceFrame;
        public readonly ulong EffectiveFrame;
        /// <summary>Exception effective (Accepted, Held) ; nulle sinon.</summary>
        public readonly EffectiveRuleException Exception;

        internal RuleExceptionRecord(RuleExceptionRequest request, RuleExceptionStatus status, RuleExceptionReason reason,
            ulong sourceFrame, ulong effectiveFrame, EffectiveRuleException exception)
        {
            Request = request; Status = status; Reason = reason; SourceFrame = sourceFrame; EffectiveFrame = effectiveFrame;
            Exception = exception;
        }

        public bool IsEffective { get { return Status == RuleExceptionStatus.Accepted || Status == RuleExceptionStatus.Held; } }

        public string ToText()
        {
            return "RuleException " + Status + " " + Reason + " " + (Request == null ? "?" : Request.ToText()) + " source "
                + SourceFrame.ToString(CultureInfo.InvariantCulture) + " effectif " + EffectiveFrame.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Story 5.41 (AD-39) : autorite des TrafficRules, possedee par le coordinateur et evaluee dans son lot. Seule a accepter ou
    /// refuser une demande ; une exception ne touche ni grant, ni refus, ni occupant. Fail-closed sur frame invalide.
    /// </summary>
    public sealed class TrafficRuleAuthority
    {
        private sealed class Active
        {
            public EffectiveRuleException Exception;
            public bool Entered;
        }

        private List<Active> active = new List<Active>();

        /// <summary>Exceptions actives apres le dernier lot, triees par (demandeur, regle).</summary>
        public int ActiveCount { get { return active.Count; } }

        /// <summary>
        /// Lot de la frame <paramref name="frameId"/> : termine les exceptions actives, puis decide les demandes dans un ordre
        /// stable. <paramref name="reports"/> : un rapport par acteur present (presence et localisation).
        /// </summary>
        internal List<RuleExceptionRecord> Resolve(ulong frameId, IReadOnlyDictionary<RoadId, JunctionActorReport> reports,
            IReadOnlyList<RuleExceptionRequest> requests, CompiledRoadModel model, IReadOnlyList<TrafficRule> violable,
            int maxFrames)
        {
            ulong effective = frameId + 1UL;
            var records = new List<RuleExceptionRecord>();
            var kept = new List<Active>();
            foreach (var current in active)
            {
                var request = current.Exception.Request;
                JunctionActorReport requester;
                var reason = RuleExceptionReason.Held;
                if (!reports.TryGetValue(request.Requester, out requester)) reason = RuleExceptionReason.RequesterGone;
                else if (!request.Target.IsEmpty && !reports.ContainsKey(request.Target)) reason = RuleExceptionReason.TargetGone;
                else if (effective > current.Exception.ExpiryFrame) reason = RuleExceptionReason.Expired;
                else if (request.Termination.Kind == RuleExceptionTerminationKind.ScopeExited && requester.Localized)
                {
                    if (requester.ElementId == request.Scope) current.Entered = true;
                    else if (current.Entered) reason = RuleExceptionReason.ScopeExited;
                }
                if (reason == RuleExceptionReason.Held)
                {
                    kept.Add(current);
                    records.Add(new RuleExceptionRecord(request, RuleExceptionStatus.Held, reason, frameId, effective, current.Exception));
                }
                else records.Add(new RuleExceptionRecord(request, RuleExceptionStatus.Ended, reason, frameId, effective, null));
            }

            var ordered = new List<RuleExceptionRequest>();
            if (requests != null)
                for (int i = 0; i < requests.Count; i++)
                    if (requests[i] != null) ordered.Add(requests[i]);
            ordered.Sort(CompareRequests);
            foreach (var request in ordered)
            {
                var reason = Decide(request, frameId, reports, model, violable, maxFrames, kept);
                if (reason != RuleExceptionReason.Accepted)
                {
                    records.Add(new RuleExceptionRecord(request, RuleExceptionStatus.Denied, reason, frameId, effective, null));
                    continue;
                }
                var exception = new EffectiveRuleException(request, frameId);
                var entered = request.Termination.Kind == RuleExceptionTerminationKind.ScopeExited
                    && reports[request.Requester].Localized && reports[request.Requester].ElementId == request.Scope;
                kept.Add(new Active { Exception = exception, Entered = entered });
                records.Add(new RuleExceptionRecord(request, RuleExceptionStatus.Accepted, reason, frameId, effective, exception));
            }
            kept.Sort((a, b) => CompareRequests(a.Exception.Request, b.Exception.Request));
            active = kept;
            return records;
        }

        /// <summary>Lot sur frame invalide : aucune demande examinee, toute exception active prend fin (FrameUnavailable).</summary>
        internal List<RuleExceptionRecord> FailClosed(ulong frameId)
        {
            var records = new List<RuleExceptionRecord>(active.Count);
            foreach (var current in active)
                records.Add(new RuleExceptionRecord(current.Exception.Request, RuleExceptionStatus.Ended,
                    RuleExceptionReason.FrameUnavailable, frameId, frameId + 1UL, null));
            active = new List<Active>();
            return records;
        }

        private static RuleExceptionReason Decide(RuleExceptionRequest request, ulong frameId,
            IReadOnlyDictionary<RoadId, JunctionActorReport> reports, CompiledRoadModel model, IReadOnlyList<TrafficRule> violable,
            int maxFrames, List<Active> kept)
        {
            ulong expiry = request.Termination.ExpiryFrame;
            bool knownRule = Enum.IsDefined(typeof(TrafficRule), request.Rule) && request.Rule != TrafficRule.None;
            bool knownTermination = Enum.IsDefined(typeof(RuleExceptionTerminationKind), request.Termination.Kind);
            if (!knownRule || !knownTermination || request.Requester.IsEmpty || (request.Scope.IsEmpty && request.Target.IsEmpty)
                || request.Target == request.Requester
                || string.IsNullOrWhiteSpace(request.StartReason) || request.SourceFrame != frameId
                || expiry <= frameId || maxFrames <= 0 || expiry > frameId + (ulong)maxFrames
                || (request.Termination.Kind == RuleExceptionTerminationKind.ScopeExited && request.Scope.IsEmpty))
                return RuleExceptionReason.Malformed;
            if (!reports.ContainsKey(request.Requester)) return RuleExceptionReason.RequesterAbsent;
            if (!request.Target.IsEmpty && !reports.ContainsKey(request.Target)) return RuleExceptionReason.TargetAbsent;
            bool violableRule = false;
            if (violable != null)
                for (int i = 0; i < violable.Count; i++) if (violable[i] == request.Rule) violableRule = true;
            if (!violableRule) return RuleExceptionReason.RuleNotViolable;
            EffectiveLaneCorridor corridor;
            if (!request.Scope.IsEmpty && !model.TryGetCorridor(request.Scope, out corridor)) return RuleExceptionReason.ScopeInsideJunction;
            foreach (var current in kept)
                if (current.Exception.Requester == request.Requester && current.Exception.Rule == request.Rule)
                    return RuleExceptionReason.AlreadyActive;
            return RuleExceptionReason.Accepted;
        }

        private static int CompareRequests(RuleExceptionRequest a, RuleExceptionRequest b)
        {
            int order = a.Requester.CompareTo(b.Requester);
            if (order == 0) order = ((int)a.Rule).CompareTo((int)b.Rule);
            if (order == 0) order = a.Scope.CompareTo(b.Scope);
            if (order == 0) order = a.Target.CompareTo(b.Target);
            if (order == 0) order = a.Termination.ExpiryFrame.CompareTo(b.Termination.ExpiryFrame);
            if (order == 0) order = ((int)a.Termination.Kind).CompareTo((int)b.Termination.Kind);
            return order != 0 ? order : string.CompareOrdinal(a.StartReason, b.StartReason);
        }
    }
}
