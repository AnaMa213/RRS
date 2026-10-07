using System;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Collisions;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Tactical
{
    /// <summary>Reaction tiree a l'acceptation d'une collision (Story 5.38, C4).</summary>
    public enum CollisionReaction { Brake = 0, Evade = 1, MisReact = 2 }

    /// <summary>
    /// Distribution authoree des reactions (C4, decision D2) et durees de la premiere phase. Portee par
    /// <see cref="DriverProfileDef"/>, hors de la struct de conduite V1.
    /// </summary>
    [Serializable]
    public struct CollisionReactionWeights
    {
        [SerializeField, Min(0f), Tooltip("Poids du freinage d'urgence (reaction courante).")]
        private float brake;
        [SerializeField, Min(0f), Tooltip("Poids de l'esquive (reaction moins frequente).")]
        private float evade;
        [SerializeField, Min(0f), Tooltip("Poids de la mauvaise reaction : roue libre, volant fige, puis freinage.")]
        private float misReact;
        [SerializeField, Min(0f), Tooltip("Duree du coup de volant d'esquive avant le freinage (s).")]
        private float evadeSeconds;
        [SerializeField, Min(0f), Tooltip("Duree de la mauvaise reaction avant le freinage (s).")]
        private float misReactSeconds;

        public CollisionReactionWeights(float brake, float evade, float misReact, float evadeSeconds, float misReactSeconds)
        {
            this.brake = brake; this.evade = evade; this.misReact = misReact;
            this.evadeSeconds = evadeSeconds; this.misReactSeconds = misReactSeconds;
        }

        public static CollisionReactionWeights Default { get { return new CollisionReactionWeights(0.75f, 0.15f, 0.10f, 0.8f, 0.6f); } }

        public float Brake { get { return brake; } }
        public float Evade { get { return evade; } }
        public float MisReact { get { return misReact; } }
        public float EvadeSeconds { get { return evadeSeconds; } }
        public float MisReactSeconds { get { return misReactSeconds; } }

        public bool TryValidate(out string error)
        {
            if (!NonNegative(brake) || !NonNegative(evade) || !NonNegative(misReact) || !(brake + evade + misReact > 0f)
                || float.IsInfinity(brake + evade + misReact))
            {
                error = "CollisionReaction invalide : poids finis, positifs ou nuls, de somme strictement positive.";
                return false;
            }
            if (!NonNegative(evadeSeconds) || !NonNegative(misReactSeconds))
            {
                error = "CollisionReaction invalide : durees finies et positives ou nulles.";
                return false;
            }
            error = string.Empty;
            return true;
        }

        /// <summary>Reaction pour un tirage u dans ]0, 1[ ; distribution invalide : Brake.</summary>
        public CollisionReaction Select(double u)
        {
            string error;
            if (!TryValidate(out error) || double.IsNaN(u)) return CollisionReaction.Brake;
            double x = u * (brake + evade + misReact);
            if (x < brake) return CollisionReaction.Brake;
            if (x < brake + evade) return CollisionReaction.Evade;
            return misReact > 0f ? CollisionReaction.MisReact : evade > 0f ? CollisionReaction.Evade : CollisionReaction.Brake;
        }

        public float PhaseSeconds(CollisionReaction reaction)
        {
            return reaction == CollisionReaction.Evade ? evadeSeconds : reaction == CollisionReaction.MisReact ? misReactSeconds : 0f;
        }

        private static bool NonNegative(float value) { return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f; }
    }

    public enum TacticalGoalKind { Nominal = 0, CollisionResponse = 1 }

    /// <summary>Phase du but de reponse a collision : reaction tiree, freinage, puis attente de la recuperation (5.39).</summary>
    public enum CollisionGoalPhase { None = 0, Reacting = 1, Braking = 2, AwaitingRecovery = 3 }

    /// <summary>Raisons stables de la tactique ; valeurs ajoutees en fin d'enum.</summary>
    public enum TacticalReason
    {
        None = 0,
        Accepted = 1,
        InvalidRequest = 2,
        StaleRequest = 3,
        GoalAlreadyActive = 4,
        FallbackLatched = 5,
        /// <summary>But termine : vehicule stable dans l'enveloppe epsilon_t, conduite nominale reprise.</summary>
        Resumed = 6,
        /// <summary>But annule : portail de sortie atteint.</summary>
        ExitPortalReached = 7
    }

    /// <summary>Reponse de la tactique a une requete (C3).</summary>
    public readonly struct TacticalResponse
    {
        public readonly ulong RequestVersion;
        public readonly TacticalReason Reason;
        public bool Accepted { get { return Reason == TacticalReason.Accepted; } }

        public TacticalResponse(ulong requestVersion, TacticalReason reason) { RequestVersion = requestVersion; Reason = reason; }
    }

    /// <summary>
    /// Decision tactique d'un vehicule V2 (Story 5.38, AD-40) : seule proprietaire du but. Elle accepte ou refuse une requete
    /// de collision avec une raison, tire la reaction, possede le but jusqu'a sa terminaison (Resumed) ou son annulation, et
    /// produit sa commande sans propulsion (C5). Elle ne touche ni corps, ni route, ni carrefour.
    /// </summary>
    public sealed class TacticalDecision
    {
        /// <summary>Domaine du tirage deterministe des reactions.</summary>
        public const string ReactionDrawDomain = "collision";
        /// <summary>Fraction du braquage declare d'une esquive.</summary>
        public const float EvadeLockFraction = 0.5f;

        private ulong lastRequestVersion;
        private float reactingSeconds;
        private float stableSeconds;
        private float phaseSeconds;

        public TacticalGoalKind Goal { get; private set; }
        public CollisionGoalPhase Phase { get; private set; }
        public CollisionReaction Reaction { get; private set; }
        /// <summary>Version de la requete acceptee qui porte le but courant (ou le dernier).</summary>
        public ulong GoalVersion { get; private set; }
        public ulong AcceptedAtFrame { get; private set; }
        public int ImpactSide { get; private set; }
        /// <summary>Angle de roue applique au choc (degres), garde par la mauvaise reaction.</summary>
        public float ImpactWheelAngleDegrees { get; private set; }
        /// <summary>Derniere raison publiee : reponse a une requete, terminaison ou annulation.</summary>
        public TacticalReason LastReason { get; private set; }
        public StabilityBlocker LastBlocker { get; private set; }

        public bool Active { get { return Goal == TacticalGoalKind.CollisionResponse; } }

        public TacticalResponse Submit(CollisionResponseRequest request, ulong frameId, bool fallbackLatched, RouteSeed seed,
            CollisionReactionWeights weights, float appliedWheelAngleDegrees)
        {
            if (request == null || request.TrafficId.IsEmpty || !request.Facts.IsFinite || request.Version <= lastRequestVersion
                || request.Significance == CollisionSignificance.None)
                return Respond(request == null ? 0UL : request.Version, TacticalReason.InvalidRequest);
            lastRequestVersion = request.Version;
            if (request.SourceFrameId != frameId) return Respond(request.Version, TacticalReason.StaleRequest);
            if (fallbackLatched) return Respond(request.Version, TacticalReason.FallbackLatched);
            // Le but garde sa raison publiee : un second choc pendant le but est refuse sans la masquer.
            if (Active) return new TacticalResponse(request.Version, TacticalReason.GoalAlreadyActive);

            Goal = TacticalGoalKind.CollisionResponse;
            GoalVersion = request.Version;
            AcceptedAtFrame = frameId;
            Reaction = weights.Select(RoutePlanner.UnitDraw(seed.Value, request.TrafficId, ReactionDrawDomain, request.Version, RoadId.None));
            phaseSeconds = weights.PhaseSeconds(Reaction);
            Phase = Reaction == CollisionReaction.Brake || !(phaseSeconds > 0f) ? CollisionGoalPhase.Braking : CollisionGoalPhase.Reacting;
            ImpactSide = request.Facts.ImpactSide;
            // Esquive sans cote connu (choc frontal ou arriere) : aucun sens ou braquer, freinage direct.
            if (Reaction == CollisionReaction.Evade && ImpactSide == 0) Phase = CollisionGoalPhase.Braking;
            ImpactWheelAngleDegrees = Finite(appliedWheelAngleDegrees) ? appliedWheelAngleDegrees : 0f;
            reactingSeconds = 0f;
            stableSeconds = 0f;
            return Respond(request.Version, TacticalReason.Accepted);
        }

        /// <summary>
        /// Avance le but d'un pas (C6) : fin de la phase de reaction, compte de stabilite, terminaison Resumed seulement stable
        /// dans l'enveloppe epsilon_t ; stable mais deplace : AwaitingRecovery. Sortie atteinte : annulation.
        /// </summary>
        public void Update(CollisionFacts facts, float deltaTimeSeconds, float toleranceMeters, bool exitReached)
        {
            if (!Active) return;
            if (exitReached) { End(TacticalReason.ExitPortalReached); return; }
            float dt = Finite(deltaTimeSeconds) && deltaTimeSeconds > 0f ? deltaTimeSeconds : 0f;
            // Les faits de ce pas decrivent la commande precedente, pas celle produite apres Update.
            bool braking = Phase != CollisionGoalPhase.Reacting;
            if (Phase == CollisionGoalPhase.Reacting)
            {
                reactingSeconds += dt;
                if (reactingSeconds >= phaseSeconds) Phase = CollisionGoalPhase.Braking;
            }
            // La stabilite se compte en freinage seulement : la reaction est toujours suivie de 0,5 s de freinage au moins.
            LastBlocker = CollisionPredicates.Stability(facts, toleranceMeters);
            stableSeconds = LastBlocker == StabilityBlocker.None && braking ? stableSeconds + dt : 0f;
            if (!braking || stableSeconds < CollisionThresholds.StableSeconds) return;
            if (facts.DisplacementMeters <= toleranceMeters) End(TacticalReason.Resumed);
            else Phase = CollisionGoalPhase.AwaitingRecovery;
        }

        /// <summary>
        /// Commande du but (C5), jamais de propulsion : Brake -b_max roues droites ; Evade, pendant sa phase, braquage oppose
        /// au cote d'impact et -b confort ; MisReact, pendant sa phase, acceleration nulle et angle du choc. Sans courbure de
        /// reference : la commande ne suit aucune route.
        /// </summary>
        public MotionCommand CommandFor(ulong frameId, int validitySteps, float maxBrakingDeceleration, float comfortableDeceleration,
            float lockDegrees)
        {
            if (!Active) throw new InvalidOperationException("NoCollisionGoal");
            float acceleration = -maxBrakingDeceleration, angle = 0f;
            if (Phase == CollisionGoalPhase.Reacting && Reaction == CollisionReaction.Evade)
            {
                acceleration = -comfortableDeceleration;
                angle = -ImpactSide * EvadeLockFraction * lockDegrees;
            }
            else if (Phase == CollisionGoalPhase.Reacting && Reaction == CollisionReaction.MisReact)
            {
                acceleration = 0f;
                angle = Mathf.Clamp(ImpactWheelAngleDegrees, -lockDegrees, lockDegrees);
            }
            return new MotionCommand(frameId, frameId, frameId + (ulong)Math.Max(0, validitySteps - 1), acceleration, angle);
        }

        /// <summary>Texte deterministe, invariant de culture.</summary>
        public string ToText()
        {
            string text = Goal + " " + LastReason;
            if (GoalVersion == 0UL) return text;
            return text + " " + Reaction + "/" + Phase + " v" + GoalVersion.ToString(CultureInfo.InvariantCulture)
                + " accepte " + AcceptedAtFrame.ToString(CultureInfo.InvariantCulture) + " stabilite " + LastBlocker;
        }

        private void End(TacticalReason reason)
        {
            Goal = TacticalGoalKind.Nominal;
            Phase = CollisionGoalPhase.None;
            LastReason = reason;
        }

        private TacticalResponse Respond(ulong version, TacticalReason reason)
        {
            LastReason = reason;
            return new TacticalResponse(version, reason);
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
