using System;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Collisions;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Recovery;
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

    public enum TacticalGoalKind
    {
        Nominal = 0,
        CollisionResponse = 1,
        /// <summary>Manoeuvre de recuperation acceptee (Story 5.39) : realignement ou recul controle.</summary>
        Recovery = 2
    }

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
        ExitPortalReached = 7,
        /// <summary>Story 5.39 : requete de recuperation refusee, vehicule physiquement instable (C6).</summary>
        Unstable = 8,
        /// <summary>Story 5.39 : realignement refuse, aucune reference a rejoindre.</summary>
        NoReference = 9,
        /// <summary>Story 5.39 : recul refuse, un acteur de la frame est dans le balayage arriere.</summary>
        RearBlocked = 10,
        /// <summary>Story 5.39 : manoeuvre echouee, la progression attendue n'a pas eu lieu (registre R2).</summary>
        Stalled = 11,
        /// <summary>Story 5.39 : realignement echoue, parcours maximal sans retour dans l'enveloppe.</summary>
        NoProgress = 12,
        /// <summary>Story 5.39 : recul controle termine sur sa distance.</summary>
        ManeuverCompleted = 13,
        /// <summary>Story 5.39 : manoeuvre annulee par une collision significative acceptee.</summary>
        CollisionPreempted = 14
    }

    /// <summary>Faits de mouvement d'un pas pour la manoeuvre de recuperation (R5).</summary>
    public readonly struct RecoveryMotion
    {
        public readonly float SpeedMetersPerSecond;
        /// <summary>Distance plane parcourue pendant le pas (m).</summary>
        public readonly float TravelMeters;
        public readonly float MaxAccelerationMetersPerSecondSquared;

        public RecoveryMotion(float speed, float travelMeters, float maxAcceleration)
        {
            SpeedMetersPerSecond = speed; TravelMeters = travelMeters; MaxAccelerationMetersPerSecondSquared = maxAcceleration;
        }
    }

    /// <summary>Entree de la commande d'une manoeuvre : vitesse mesuree, pas, acceleration du profil et angle de realignement.</summary>
    public readonly struct RecoveryCommandInput
    {
        public readonly float SpeedMetersPerSecond;
        public readonly float DeltaTimeSeconds;
        public readonly float MaxAccelerationMetersPerSecondSquared;
        /// <summary>Angle de la loi de suivi vers la pose nominale de la reference projetee (degres).</summary>
        public readonly float RealignWheelAngleDegrees;

        public RecoveryCommandInput(float speed, float deltaTimeSeconds, float maxAcceleration, float realignWheelAngle)
        {
            SpeedMetersPerSecond = speed; DeltaTimeSeconds = deltaTimeSeconds; MaxAccelerationMetersPerSecondSquared = maxAcceleration;
            RealignWheelAngleDegrees = realignWheelAngle;
        }
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
        private ulong lastRecoveryVersion;
        private float reactingSeconds;
        private float stableSeconds;
        private float phaseSeconds;
        private readonly ProgressLedger maneuverLedger = new ProgressLedger();
        private bool recoveryIsLatest;

        public TacticalGoalKind Goal { get; private set; }
        /// <summary>Manoeuvre de recuperation acceptee (but Recovery), ou la derniere.</summary>
        public RecoveryManeuver Maneuver { get; private set; }
        /// <summary>Version de la requete de recuperation qui porte le but Recovery courant (ou le dernier).</summary>
        public ulong RecoveryVersion { get; private set; }
        /// <summary>Parcours plan de la manoeuvre courante (m).</summary>
        public float ManeuverTravelMeters { get; private set; }
        /// <summary>Version et raison de la derniere manoeuvre de recuperation terminee ou annulee ; None sinon.</summary>
        public ulong RecoveryOutcomeVersion { get; private set; }
        public TacticalReason RecoveryOutcome { get; private set; }
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

        /// <summary>Un but (collision ou recuperation) remplace la conduite nominale.</summary>
        public bool Active { get { return Goal != TacticalGoalKind.Nominal; } }
        public bool CollisionActive { get { return Goal == TacticalGoalKind.CollisionResponse; } }
        public bool RecoveryActive { get { return Goal == TacticalGoalKind.Recovery; } }
        /// <summary>Stable mais deplace : la recuperation (5.39) peut prendre le relais.</summary>
        public bool AwaitingRecovery { get { return CollisionActive && Phase == CollisionGoalPhase.AwaitingRecovery; } }
        /// <summary>La commande du but est un recul controle : le composeur doit passer par BrakeReverse.</summary>
        public bool Reversing { get { return RecoveryActive && Maneuver == RecoveryManeuver.Reverse; } }

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
            if (CollisionActive) return new TacticalResponse(request.Version, TacticalReason.GoalAlreadyActive);
            // Un choc significatif annule la manoeuvre de recuperation (5.39) : la physique d'abord.
            if (RecoveryActive) EndRecovery(TacticalReason.CollisionPreempted);

            Goal = TacticalGoalKind.CollisionResponse;
            recoveryIsLatest = false;
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
        /// Requete de recuperation (Story 5.39, R3) : acceptee seulement sans but, ou depuis un but de collision en
        /// AwaitingRecovery, qu'elle remplace ; refusee avec une raison sinon. La tactique possede ensuite la manoeuvre.
        /// </summary>
        /// <param name="stability">Premiere cause d'instabilite C6 des faits du pas.</param>
        /// <param name="hasReference">Une reference de route existe pour le realignement.</param>
        /// <param name="rearClear">Balayage arriere libre pour le recul.</param>
        public TacticalResponse SubmitRecovery(RecoveryRequest request, ulong frameId, StabilityBlocker stability, bool hasReference,
            bool rearClear)
        {
            if (request == null || request.TrafficId.IsEmpty || request.Version <= lastRecoveryVersion
                || request.Cause == RecoveryCause.None)
                return Respond(request == null ? 0UL : request.Version, TacticalReason.InvalidRequest);
            lastRecoveryVersion = request.Version;
            if (request.SourceFrameId != frameId) return Respond(request.Version, TacticalReason.StaleRequest);
            if (RecoveryActive || (CollisionActive && !AwaitingRecovery))
                return new TacticalResponse(request.Version, TacticalReason.GoalAlreadyActive);
            if (stability != StabilityBlocker.None) return Respond(request.Version, TacticalReason.Unstable);
            if (request.Maneuver == RecoveryManeuver.Realign && !hasReference) return Respond(request.Version, TacticalReason.NoReference);
            if (request.Maneuver == RecoveryManeuver.Reverse && !rearClear) return Respond(request.Version, TacticalReason.RearBlocked);

            Goal = TacticalGoalKind.Recovery;
            recoveryIsLatest = true;
            Phase = CollisionGoalPhase.None;
            Maneuver = request.Maneuver;
            RecoveryVersion = request.Version;
            AcceptedAtFrame = frameId;
            ManeuverTravelMeters = 0f;
            stableSeconds = 0f;
            maneuverLedger.Reset();
            return Respond(request.Version, TacticalReason.Accepted);
        }

        /// <summary>
        /// Avance le but d'un pas (C6) : fin de la phase de reaction, compte de stabilite, terminaison Resumed seulement stable
        /// dans l'enveloppe epsilon_t ; stable mais deplace : AwaitingRecovery. Sortie atteinte : annulation.
        /// </summary>
        public void Update(CollisionFacts facts, float deltaTimeSeconds, float toleranceMeters, bool exitReached,
            RecoveryMotion motion = default(RecoveryMotion))
        {
            if (!Active) return;
            if (exitReached)
            {
                if (RecoveryActive) EndRecovery(TacticalReason.ExitPortalReached);
                else End(TacticalReason.ExitPortalReached);
                return;
            }
            float dt = Finite(deltaTimeSeconds) && deltaTimeSeconds > 0f ? deltaTimeSeconds : 0f;
            if (RecoveryActive) { UpdateRecovery(facts, dt, toleranceMeters, motion); return; }
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
        /// <param name="recovery">
        /// Story 5.39, R5 : vitesse, pas, acceleration du profil et angle de realignement, lus seulement pendant une
        /// manoeuvre. Realign : marche avant vers v_rec, angle de la loi de suivi. Reverse : roues droites, consigne -v_rev.
        /// </param>
        public MotionCommand CommandFor(ulong frameId, int validitySteps, float maxBrakingDeceleration, float comfortableDeceleration,
            float lockDegrees, RecoveryCommandInput recovery = default(RecoveryCommandInput))
        {
            if (!Active) throw new InvalidOperationException("NoCollisionGoal");
            ulong validTo = frameId + (ulong)Math.Max(0, validitySteps - 1);
            if (RecoveryActive)
            {
                float speed = Finite(recovery.SpeedMetersPerSecond) ? recovery.SpeedMetersPerSecond : 0f;
                float dt = Finite(recovery.DeltaTimeSeconds) && recovery.DeltaTimeSeconds > 0f ? recovery.DeltaTimeSeconds : 1f;
                float aMax = Finite(recovery.MaxAccelerationMetersPerSecondSquared)
                    ? Math.Max(0f, recovery.MaxAccelerationMetersPerSecondSquared) : 0f;
                if (Maneuver == RecoveryManeuver.Reverse)
                {
                    // Acceleration negative : vers l'arriere. Le composeur la traduit en BrakeReverse, jamais en gaz.
                    float target = -TrafficV2Settings.RecoveryReverseSpeedMetersPerSecond;
                    return new MotionCommand(frameId, frameId, validTo,
                        Mathf.Clamp((target - speed) / dt, -aMax, comfortableDeceleration), 0f);
                }
                float realign = Finite(recovery.RealignWheelAngleDegrees) ? recovery.RealignWheelAngleDegrees : 0f;
                return new MotionCommand(frameId, frameId, validTo,
                    Mathf.Clamp((TrafficV2Settings.RecoveryRealignSpeedMetersPerSecond - speed) / dt, -comfortableDeceleration, aMax),
                    Mathf.Clamp(realign, -lockDegrees, lockDegrees));
            }
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
            return new MotionCommand(frameId, frameId, validTo, acceleration, angle);
        }

        /// <summary>Texte deterministe, invariant de culture.</summary>
        public string ToText()
        {
            string text = Goal + " " + LastReason;
            if (recoveryIsLatest)
                return text + " " + Maneuver + " v" + RecoveryVersion.ToString(CultureInfo.InvariantCulture) + " accepte "
                    + AcceptedAtFrame.ToString(CultureInfo.InvariantCulture) + " parcours "
                    + ManeuverTravelMeters.ToString("0.###", CultureInfo.InvariantCulture) + " stabilite " + LastBlocker;
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

        /// <summary>
        /// Manoeuvre (R5) : Realign termine Resumed apres 0,5 s stable dans l'enveloppe epsilon_t, echoue NoProgress apres son
        /// parcours maximal ; Reverse termine ManeuverCompleted sur sa distance. Les deux echouent Stalled quand le registre R2
        /// constate qu'une progression commandee n'a pas eu lieu.
        /// </summary>
        private void UpdateRecovery(CollisionFacts facts, float dt, float toleranceMeters, RecoveryMotion motion)
        {
            float travel = Finite(motion.TravelMeters) ? Math.Max(0f, motion.TravelMeters) : 0f;
            ManeuverTravelMeters += travel;
            LastBlocker = CollisionPredicates.Stability(facts, toleranceMeters);
            if (Maneuver == RecoveryManeuver.Realign)
            {
                stableSeconds = LastBlocker == StabilityBlocker.None && facts.DisplacementMeters <= toleranceMeters
                    ? stableSeconds + dt : 0f;
                if (stableSeconds >= CollisionThresholds.StableSeconds) { EndRecovery(TacticalReason.Resumed); return; }
                if (ManeuverTravelMeters >= TrafficV2Settings.RecoveryRealignMaxTravelMeters) { EndRecovery(TacticalReason.NoProgress); return; }
            }
            else if (ManeuverTravelMeters >= TrafficV2Settings.RecoveryReverseTravelMeters)
            {
                EndRecovery(TacticalReason.ManeuverCompleted);
                return;
            }
            float cap = Maneuver == RecoveryManeuver.Reverse ? TrafficV2Settings.RecoveryReverseSpeedMetersPerSecond
                : TrafficV2Settings.RecoveryRealignSpeedMetersPerSecond;
            float speed = Finite(motion.SpeedMetersPerSecond) ? Math.Abs(motion.SpeedMetersPerSecond) : 0f;
            if (maneuverLedger.Observe(true, motion.MaxAccelerationMetersPerSecondSquared, speed, ManeuverTravelMeters, dt, cap))
                EndRecovery(TacticalReason.Stalled);
        }

        private void EndRecovery(TacticalReason reason)
        {
            RecoveryOutcomeVersion = RecoveryVersion;
            RecoveryOutcome = reason;
            stableSeconds = 0f;
            End(reason);
        }

        private TacticalResponse Respond(ulong version, TacticalReason reason)
        {
            LastReason = reason;
            return new TacticalResponse(version, reason);
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
