using RoadRage.Shared.Domain;

namespace RoadRage.Features.PassengerActions
{
    public enum PassengerActionVerdictCode : byte
    {
        Accepted = 0,
        SenderDisconnected,
        ActorMismatch,
        ActorNotAlive,
        InvalidPhase,
        ActorNotPassenger,
        SeatMismatch,
        InvalidCatalog,
        InvalidAction,
        InvalidVersion,
        ReplayedSequence,
        CooldownActive,
        InvalidTarget,
        TargetOutOfRange
    }

    public readonly struct PassengerActionValidationContext
    {
        public readonly bool SenderConnected;
        public readonly bool ActorMatchesSender;
        public readonly PlayerLifecycle Lifecycle;
        public readonly PlayerMode Mode;
        public readonly bool PassengerSeat;
        public readonly bool SeatOccupantMatchesActor;
        public readonly RunPhase Phase;
        public readonly bool CatalogValid;
        public readonly bool TargetValid;
        public readonly float TargetDistance;
        public readonly ulong LastSequence;
        public readonly double CooldownEndsAt;
        public readonly double Now;

        public PassengerActionValidationContext(
            bool senderConnected, bool actorMatchesSender, PlayerLifecycle lifecycle, PlayerMode mode,
            bool passengerSeat, bool seatOccupantMatchesActor, RunPhase phase, bool catalogValid,
            bool targetValid, float targetDistance, ulong lastSequence, double cooldownEndsAt, double now)
        {
            SenderConnected = senderConnected;
            ActorMatchesSender = actorMatchesSender;
            Lifecycle = lifecycle;
            Mode = mode;
            PassengerSeat = passengerSeat;
            SeatOccupantMatchesActor = seatOccupantMatchesActor;
            Phase = phase;
            CatalogValid = catalogValid;
            TargetValid = targetValid;
            TargetDistance = targetDistance;
            LastSequence = lastSequence;
            CooldownEndsAt = cooldownEndsAt;
            Now = now;
        }
    }

    public readonly struct PassengerActionVerdict
    {
        public readonly PassengerActionVerdictCode Code;
        public readonly string Message;
        public bool Accepted => Code == PassengerActionVerdictCode.Accepted;

        public PassengerActionVerdict(PassengerActionVerdictCode code, string message)
        {
            Code = code;
            Message = message;
        }
    }

    public static class PassengerActionValidation
    {
        public static PassengerActionVerdict Validate(
            PassengerActionCatalog catalog,
            PassengerActionDef action,
            PassengerActionIntent intent,
            PassengerActionValidationContext context)
        {
            if (!context.SenderConnected) return Reject(PassengerActionVerdictCode.SenderDisconnected, "emetteur deconnecte");
            if (!context.ActorMatchesSender) return Reject(PassengerActionVerdictCode.ActorMismatch, "acteur usurpe");
            if (context.Lifecycle != PlayerLifecycle.Alive) return Reject(PassengerActionVerdictCode.ActorNotAlive, "acteur non jouable");
            if (context.Mode != PlayerMode.Passenger) return Reject(PassengerActionVerdictCode.ActorNotPassenger, "acteur non passager");
            if (!context.PassengerSeat || !context.SeatOccupantMatchesActor) return Reject(PassengerActionVerdictCode.SeatMismatch, "siege passager incoherent");
            if (catalog == null || !context.CatalogValid) return Reject(PassengerActionVerdictCode.InvalidCatalog, "catalogue invalide");
            if (action == null || intent.Slot >= PassengerActionCatalog.SlotCount || action.Slot != intent.Slot || action.RawId != intent.ActionId.ToString()) return Reject(PassengerActionVerdictCode.InvalidAction, "action ou slot inconnu");
            if (intent.CatalogVersion != catalog.Version || intent.ActionVersion != action.Version) return Reject(PassengerActionVerdictCode.InvalidVersion, "version inconnue");
            if (!action.AllowsPhase(context.Phase)) return Reject(PassengerActionVerdictCode.InvalidPhase, "phase non autorisee");
            if (intent.Sequence == 0UL || intent.Sequence <= context.LastSequence) return Reject(PassengerActionVerdictCode.ReplayedSequence, "sequence rejouee");
            if (context.Now < context.CooldownEndsAt) return Reject(PassengerActionVerdictCode.CooldownActive, "cooldown actif");
            if (!context.TargetValid) return Reject(PassengerActionVerdictCode.InvalidTarget, "cible invalide");
            if (!float.IsFinite(context.TargetDistance) || context.TargetDistance > action.MaxRange) return Reject(PassengerActionVerdictCode.TargetOutOfRange, "cible hors portee");
            return new PassengerActionVerdict(PassengerActionVerdictCode.Accepted, "action acceptee");
        }

        private static PassengerActionVerdict Reject(PassengerActionVerdictCode code, string reason)
        {
            return new PassengerActionVerdict(code, "Action refusee : " + reason + ".");
        }
    }
}
