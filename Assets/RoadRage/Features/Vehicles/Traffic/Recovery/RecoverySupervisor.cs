using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Tactical;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Recovery
{
    /// <summary>Cause d'eligibilite a la recuperation (Story 5.39, R1).</summary>
    public enum RecoveryCause
    {
        None = 0,
        /// <summary>But de collision stable mais deplace (AwaitingRecovery, 5.38).</summary>
        Displaced = 1,
        /// <summary>Verrou 2a pose et repli tenu a l'arret (5.52).</summary>
        ToleranceLatched = 2,
        /// <summary>Progression attendue par le plan, non realisee, sans blocker legitime (R2).</summary>
        ProgressDeficit = 3
    }

    /// <summary>Manoeuvre demandee a la tactique (R5).</summary>
    public enum RecoveryManeuver { Realign = 0, Reverse = 1 }

    /// <summary>
    /// Registre de progression attendue contre reelle (R2), pur. E integre une vitesse attendue v_e partie de max(0, v) et
    /// accelere par a, bornee a [0, plafond] ; A est l'avance reelle depuis le debut de l'episode. Une attente fausse remet
    /// tout a zero ; une avance A &gt;= 0,5 m prouve une progression et recommence l'episode ici. Jamais un minuteur : rien ne
    /// s'accumule hors attente.
    /// </summary>
    public sealed class ProgressLedger
    {
        private bool active;
        private float start;
        private float expectedSpeed;

        public float ExpectedMeters { get; private set; }
        public float ActualMeters { get; private set; }

        public void Reset()
        {
            active = false; start = 0f; expectedSpeed = 0f; ExpectedMeters = 0f; ActualMeters = 0f;
        }

        /// <param name="position">Mesure monotone de l'avance (distance sur la reference ou parcours plan).</param>
        /// <returns>Vrai si E &gt;= 2 m alors que A &lt; 0,5 m : la progression attendue n'a pas eu lieu.</returns>
        public bool Observe(bool expecting, float acceleration, float speed, float position, float deltaTimeSeconds, float speedCap)
        {
            if (!expecting || !Finite(acceleration) || !Finite(speed) || !Finite(position) || !Finite(deltaTimeSeconds)
                || !(deltaTimeSeconds > 0f) || !Finite(speedCap))
            {
                Reset();
                return false;
            }
            float cap = Math.Max(0f, speedCap);
            if (!active) Begin(position, speed, cap);
            ActualMeters = position - start;
            if (ActualMeters >= TrafficV2Settings.RecoveryMinimumProgressMeters)
            {
                Begin(position, speed, cap);
                return false;
            }
            expectedSpeed = Mathf.Clamp(expectedSpeed + acceleration * deltaTimeSeconds, 0f, cap);
            ExpectedMeters += expectedSpeed * deltaTimeSeconds;
            return ExpectedMeters >= TrafficV2Settings.RecoveryExpectedProgressMeters;
        }

        private void Begin(float position, float speed, float cap)
        {
            active = true; start = position; expectedSpeed = Mathf.Clamp(speed, 0f, cap); ExpectedMeters = 0f; ActualMeters = 0f;
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    /// <summary>Requete versionnee de recuperation (R3) : le superviseur la soumet, la tactique l'accepte ou la refuse.</summary>
    public sealed class RecoveryRequest
    {
        public ulong Version { get; }
        public ulong SourceFrameId { get; }
        public RoadId TrafficId { get; }
        public RecoveryCause Cause { get; }
        public RecoveryManeuver Maneuver { get; }
        /// <summary>Rang de la tentative dans l'episode, 1 pour la premiere.</summary>
        public int Attempt { get; }

        public RecoveryRequest(ulong version, ulong sourceFrameId, RoadId trafficId, RecoveryCause cause, RecoveryManeuver maneuver,
            int attempt)
        {
            Version = version; SourceFrameId = sourceFrameId; TrafficId = trafficId; Cause = cause; Maneuver = maneuver;
            Attempt = attempt;
        }

        public string ToText()
        {
            return "v" + Version.ToString(CultureInfo.InvariantCulture) + " " + Maneuver + " " + Cause + " tentative "
                + Attempt.ToString(CultureInfo.InvariantCulture) + " source " + SourceFrameId.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Une tentative de l'historique : requete, reponse de la tactique et, si acceptee, sa fin.</summary>
    public sealed class RecoveryAttempt
    {
        public RecoveryRequest Request { get; }
        public TacticalReason Response { get; }
        public ulong ResponseFrame { get; }
        /// <summary>Fin d'une tentative acceptee ; None tant qu'elle dure ou si elle a ete refusee.</summary>
        public TacticalReason Outcome { get; internal set; }
        public ulong OutcomeFrame { get; internal set; }

        public RecoveryAttempt(RecoveryRequest request, TacticalReason response, ulong responseFrame)
        {
            Request = request; Response = response; ResponseFrame = responseFrame;
        }

        public string ToText()
        {
            string text = Request.ToText() + " " + Response + " @" + ResponseFrame.ToString(CultureInfo.InvariantCulture);
            return Outcome == TacticalReason.None ? text
                : text + " -> " + Outcome + " @" + OutcomeFrame.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Faits d'un pas lus par le superviseur, en fin de pas (R1, R2).</summary>
    public readonly struct RecoveryObservation
    {
        public readonly ulong FrameId;
        /// <summary>Un but tactique (collision ou recuperation) est actif en fin de pas.</summary>
        public readonly bool GoalActive;
        public readonly bool CollisionAwaitingRecovery;
        public readonly bool ToleranceLatchedHeld;
        /// <summary>Pas nominal commande : arbitrage present, compose hors repli, aucun but.</summary>
        public readonly bool NominalCommanded;
        public readonly LongitudinalDecision Longitudinal;
        public readonly IReadOnlyList<Blocker> Blockers;
        public readonly float SpeedMetersPerSecond;
        public readonly float DesiredSpeedMetersPerSecond;
        /// <summary>Indice de la reference courante : un changement remet l'episode a zero.</summary>
        public readonly int ReferenceIndex;
        public readonly float ReferenceDistanceMeters;
        public readonly float DeltaTimeSeconds;
        public readonly bool ExitReached;
        /// <summary>Version de la derniere manoeuvre terminee par la tactique, et sa raison.</summary>
        public readonly ulong RecoveryOutcomeVersion;
        public readonly TacticalReason RecoveryOutcome;

        public RecoveryObservation(ulong frameId, bool goalActive, bool collisionAwaitingRecovery, bool toleranceLatchedHeld,
            bool nominalCommanded, LongitudinalDecision longitudinal, IReadOnlyList<Blocker> blockers, float speed, float desiredSpeed,
            int referenceIndex, float referenceDistance, float deltaTimeSeconds, bool exitReached, ulong recoveryOutcomeVersion,
            TacticalReason recoveryOutcome)
        {
            FrameId = frameId; GoalActive = goalActive; CollisionAwaitingRecovery = collisionAwaitingRecovery;
            ToleranceLatchedHeld = toleranceLatchedHeld; NominalCommanded = nominalCommanded; Longitudinal = longitudinal;
            Blockers = blockers; SpeedMetersPerSecond = speed; DesiredSpeedMetersPerSecond = desiredSpeed;
            ReferenceIndex = referenceIndex; ReferenceDistanceMeters = referenceDistance; DeltaTimeSeconds = deltaTimeSeconds;
            ExitReached = exitReached; RecoveryOutcomeVersion = recoveryOutcomeVersion; RecoveryOutcome = recoveryOutcome;
        }
    }

    /// <summary>
    /// Superviseur de recuperation d'un vehicule V2 (Story 5.39, AD-40). Il detecte qu'une progression attendue n'a pas eu
    /// lieu, possede l'identite des requetes et l'historique des tentatives, et escalade jusqu'a Faulted. Il ne pose jamais le
    /// but, ne compose aucun intent, n'accorde aucun grant et ne touche aucun corps : la tactique accepte ou refuse.
    /// </summary>
    public sealed class RecoverySupervisor
    {
        private readonly ProgressLedger ledger = new ProgressLedger();
        private readonly List<RecoveryAttempt> attempts = new List<RecoveryAttempt>();
        private ulong lastVersion;
        private int consecutiveRejections;
        private RecoveryManeuver? lastManeuver;
        private RecoveryAttempt awaiting;
        private int referenceIndex = -1;
        private float progressAnchor = float.NaN;

        public RecoveryCause Cause { get; private set; }
        /// <summary>Tentatives (acceptees ou refusees) de l'episode courant.</summary>
        public int EpisodeAttempts { get; private set; }
        public IReadOnlyList<RecoveryAttempt> Attempts { get { return attempts; } }
        public bool Faulted { get; private set; }
        public ulong FaultedAtFrame { get; private set; }
        public string FaultReason { get; private set; }
        public float ExpectedMeters { get { return ledger.ExpectedMeters; } }
        public float ActualMeters { get { return ledger.ActualMeters; } }

        /// <summary>Lit les faits de fin de pas : fin d'une manoeuvre, cause courante, fin d'episode sur progression.</summary>
        public void Observe(RecoveryObservation o)
        {
            if (Faulted) return;
            if (awaiting != null && o.RecoveryOutcomeVersion == awaiting.Request.Version && o.RecoveryOutcome != TacticalReason.None)
            {
                awaiting.Outcome = o.RecoveryOutcome;
                awaiting.OutcomeFrame = o.FrameId;
                awaiting = null;
                progressAnchor = float.NaN;
            }
            bool referenceChanged = o.ReferenceIndex != referenceIndex;
            referenceIndex = o.ReferenceIndex;

            if (o.ExitReached) { Cause = RecoveryCause.None; ledger.Reset(); return; }
            if (o.CollisionAwaitingRecovery) { Cause = RecoveryCause.Displaced; ledger.Reset(); return; }
            if (o.GoalActive) { Cause = RecoveryCause.None; ledger.Reset(); return; }
            if (o.ToleranceLatchedHeld) { Cause = RecoveryCause.ToleranceLatched; ledger.Reset(); return; }

            float route = 0f;
            bool expecting = o.NominalCommanded && !referenceChanged && ProgressExpected(o.Longitudinal, o.Blockers, out route);
            if (!expecting) route = 0f;
            bool deficit = ledger.Observe(expecting, route, o.SpeedMetersPerSecond, o.ReferenceDistanceMeters, o.DeltaTimeSeconds,
                o.DesiredSpeedMetersPerSecond);
            Cause = deficit ? RecoveryCause.ProgressDeficit : RecoveryCause.None;

            // Fin d'episode : une conduite nominale qui avance de 0,5 m apres la derniere tentative.
            if (!o.NominalCommanded || EpisodeAttempts == 0 || deficit) return;
            if (referenceChanged || float.IsNaN(progressAnchor)) { progressAnchor = o.ReferenceDistanceMeters; return; }
            if (o.ReferenceDistanceMeters - progressAnchor < TrafficV2Settings.RecoveryMinimumProgressMeters) return;
            EpisodeAttempts = 0;
            consecutiveRejections = 0;
            lastManeuver = null;
            progressAnchor = float.NaN;
        }

        /// <summary>
        /// Au plus une requete par pas, jamais pendant une manoeuvre acceptee. La premiere manoeuvre depend de la cause, puis
        /// elles alternent ; tentatives epuisees : Faulted.
        /// </summary>
        public RecoveryRequest TryRequest(ulong frameId, RoadId trafficId)
        {
            if (Faulted || awaiting != null || Cause == RecoveryCause.None || trafficId.IsEmpty) return null;
            if (EpisodeAttempts >= TrafficV2Settings.RecoveryMaxAttempts)
            {
                Fault(frameId, "AttemptsExhausted");
                return null;
            }
            var maneuver = lastManeuver.HasValue
                ? (lastManeuver.Value == RecoveryManeuver.Realign ? RecoveryManeuver.Reverse : RecoveryManeuver.Realign)
                : Cause == RecoveryCause.ProgressDeficit ? RecoveryManeuver.Reverse : RecoveryManeuver.Realign;
            return new RecoveryRequest(++lastVersion, frameId, trafficId, Cause, maneuver, EpisodeAttempts + 1);
        }

        /// <summary>Consigne la reponse de la tactique : un refus met a jour l'historique, jamais une resoumission identique.</summary>
        public void Record(RecoveryRequest request, TacticalResponse response, ulong frameId)
        {
            if (request == null || Faulted) return;
            var attempt = new RecoveryAttempt(request, response.Reason, frameId);
            attempts.Add(attempt);
            EpisodeAttempts++;
            lastManeuver = request.Maneuver;
            if (response.Accepted)
            {
                awaiting = attempt;
                consecutiveRejections = 0;
                return;
            }
            if (++consecutiveRejections >= TrafficV2Settings.RecoveryMaxConsecutiveRejections)
                Fault(frameId, "ConsecutiveRejections " + response.Reason);
        }

        /// <summary>Texte deterministe, invariant de culture.</summary>
        public string ToText()
        {
            var text = new StringBuilder();
            text.Append(Faulted ? "Faulted " + FaultReason + " @" + FaultedAtFrame.ToString(CultureInfo.InvariantCulture) : Cause.ToString());
            text.Append(" tentatives ").Append(EpisodeAttempts.ToString(CultureInfo.InvariantCulture)).Append('/')
                .Append(TrafficV2Settings.RecoveryMaxAttempts.ToString(CultureInfo.InvariantCulture))
                .Append(" E ").Append(ledger.ExpectedMeters.ToString("0.###", CultureInfo.InvariantCulture))
                .Append(" A ").Append(ledger.ActualMeters.ToString("0.###", CultureInfo.InvariantCulture));
            if (attempts.Count > 0) text.Append(" / ").Append(attempts[attempts.Count - 1].ToText());
            return text.ToString();
        }

        private void Fault(ulong frameId, string reason)
        {
            Faulted = true;
            FaultedAtFrame = frameId;
            FaultReason = reason;
        }

        /// <summary>
        /// Progression attendue d'un pas nominal (R2) : aucun blocker legitime, liante de route (Profile, DesiredSpeed) ou
        /// maintien/suivi dont la source est un blocker illegitime, et a_route &gt; 0. a_route : minimum des candidats Profile
        /// et DesiredSpeed non remplaces.
        /// </summary>
        public static bool ProgressExpected(LongitudinalDecision decision, IReadOnlyList<Blocker> blockers, out float routeAcceleration)
        {
            routeAcceleration = float.NaN;
            if (decision == null) return false;
            if (blockers != null)
                for (int i = 0; i < blockers.Count; i++)
                    if (blockers[i].Legitimate) return false;
            float route = float.PositiveInfinity;
            for (int i = 0; i < decision.Candidates.Count; i++)
            {
                var candidate = decision.Candidates[i];
                if (candidate.Superseded) continue;
                if (candidate.Kind == LongitudinalCandidateKind.Profile || candidate.Kind == LongitudinalCandidateKind.DesiredSpeed)
                    route = Math.Min(route, candidate.AccelerationMetersPerSecondSquared);
            }
            if (float.IsNaN(route) || float.IsInfinity(route) || !(route > 0f)) return false;
            routeAcceleration = route;
            var binding = decision.Binding;
            if (binding.Kind == LongitudinalCandidateKind.Profile || binding.Kind == LongitudinalCandidateKind.DesiredSpeed) return true;
            if (binding.Kind != LongitudinalCandidateKind.StopHold && binding.Kind != LongitudinalCandidateKind.LeaderFollowing
                && binding.Kind != LongitudinalCandidateKind.Obstacle) return false;
            string source = binding.SourceId.ToString();
            if (blockers != null)
                for (int i = 0; i < blockers.Count; i++)
                    if (!blockers[i].Legitimate && string.Equals(blockers[i].BlockingActorOrRule, source, StringComparison.Ordinal))
                        return true;
            return false;
        }

        /// <summary>
        /// Balayage arriere d'un recul (D5) : aucun autre acteur de la frame dans le rectangle derriere la caisse, sur la
        /// longueur du recul, elargi du plus grand demi-encombrement de l'acteur. Les obstacles statiques n'y sont pas.
        /// </summary>
        public static bool RearSweepClear(TrafficFrame frame, RoadId self, float travelMeters)
        {
            TrafficActor me;
            if (frame == null || !frame.TryGetActor(self, out me)) return false;
            var footprint = me.Pose.Footprint;
            Vector3 up = me.Pose.Up.sqrMagnitude > 0f ? me.Pose.Up.normalized : Vector3.up;
            Vector3 forward = Vector3.ProjectOnPlane(me.Pose.Forward, up).normalized;
            Vector3 right = Vector3.Cross(up, forward).normalized;
            float halfWidth = Math.Max(footprint.LeftMeters, footprint.RightMeters);
            for (int i = 0; i < frame.Actors.Count; i++)
            {
                var other = frame.Actors[i];
                if (other.TrafficId == self) continue;
                var g = other.Pose.Footprint;
                float reach = Math.Max(Math.Max(g.FrontMeters, g.RearMeters), Math.Max(g.LeftMeters, g.RightMeters));
                Vector3 d = other.Pose.Position - me.Pose.Position;
                float along = Vector3.Dot(d, forward), across = Vector3.Dot(d, right);
                if (along <= 0f && along >= -(footprint.RearMeters + travelMeters + reach) && Math.Abs(across) <= halfWidth + reach)
                    return false;
            }
            return true;
        }
    }
}
