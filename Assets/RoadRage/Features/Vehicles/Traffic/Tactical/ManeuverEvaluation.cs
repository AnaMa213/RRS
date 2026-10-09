using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Collisions;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Policy;
using RoadRage.Features.Vehicles.Traffic.Safety;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Tactical
{
    /// <summary>Story 5.42 (M1) : verdict stable d'un candidat de manoeuvre ; valeurs ajoutees en fin d'enum.</summary>
    public enum ManeuverVerdict
    {
        NotOffered = 0,
        GeometryInfeasible = 1,
        TrafficConflict = 2,
        PolicyRefused = 3,
        /// <summary>Candidat survivant qui attend l'exception de l'autorite des regles.</summary>
        ExceptionPending = 4,
        ExceptionDenied = 5,
        SafetyRejected = 6,
        /// <summary>Survivant complet, non retenu (plus couteux, ou plus loin dans l'enumeration).</summary>
        Feasible = 7,
        Selected = 8
    }

    /// <summary>Story 5.42 (M5) : decision de supervision d'une manoeuvre non engagee.</summary>
    public enum ManeuverSupervision { Continue = 0, Abort = 1, Commit = 2 }

    /// <summary>Story 5.42 (M3) : motif d'un refus de politique.</summary>
    public enum ManeuverPolicyRefusal { None = 0, Ineligible = 1, Gap = 2, Risk = 3, ExceptionWindow = 4 }

    /// <summary>Story 5.42 : un candidat evalue, avec son verdict, sa cause et, s'il a passe la geometrie, son chemin prouve.</summary>
    public sealed class ManeuverCandidate
    {
        public ManeuverKind Kind { get; internal set; }
        public ManeuverVerdict Verdict { get; internal set; }
        public ManeuverGeometryCause GeometryCause { get; internal set; }
        public ManeuverPolicyRefusal PolicyRefusal { get; internal set; }
        public SafetyReason SafetyReason { get; internal set; }
        /// <summary>Corridor adjacent ou oppose du candidat ; None pour le decalage dans le corridor.</summary>
        public RoadId OtherCorridorId { get; internal set; }
        public ManeuverPath Path { get; internal set; }
        public ManeuverProofResult Proof { get; internal set; }
        /// <summary>Duree jusqu'a la fin du retour (s).</summary>
        public float DurationSeconds { get; internal set; }
        /// <summary>Marge en temps des conflits (s) ; +inf sans acteur predit.</summary>
        public float SlackSeconds { get; internal set; }
        public float Risk { get; internal set; }
        public float Cost { get; internal set; }
        /// <summary>Le chemin entre dans une surface que la politique doit faire autoriser (corridor oppose).</summary>
        public bool RequiresException { get; internal set; }
        public ulong ExceptionExpiryFrame { get; internal set; }

        public string ToText()
        {
            var text = Kind + ":" + Verdict;
            if (Verdict == ManeuverVerdict.GeometryInfeasible) text += "(" + GeometryCause + ")";
            else if (Verdict == ManeuverVerdict.PolicyRefused) text += "(" + PolicyRefusal + ")";
            else if (Verdict == ManeuverVerdict.SafetyRejected) text += "(" + SafetyReason + ")";
            if (Path != null)
                text += " o " + F(Path.TargetOffsetMeters) + " v " + F(Path.SpeedMetersPerSecond) + " T " + F(DurationSeconds)
                    + " marge " + (float.IsPositiveInfinity(SlackSeconds) ? "inf" : F(SlackSeconds)) + " risque " + F(Risk);
            if (!OtherCorridorId.IsEmpty) text += " via " + OtherCorridorId;
            return text;
        }

        private static string F(float value) { return value.ToString("0.###", CultureInfo.InvariantCulture); }
    }

    /// <summary>Story 5.42 : resultat d'une evaluation, candidats dans l'ordre d'enumeration et selection eventuelle.</summary>
    public sealed class ManeuverEvaluationResult
    {
        public ulong FrameId { get; }
        public ManeuverObstacle Cause { get; }
        public IReadOnlyList<ManeuverCandidate> Candidates { get; }
        /// <summary>Candidat retenu (Selected, ou ExceptionPending s'il attend l'autorite) ; nul sinon.</summary>
        public ManeuverCandidate Selected { get; }
        /// <summary>Le candidat retenu peut partir a cette frame.</summary>
        public bool Ready { get { return Selected != null && Selected.Verdict == ManeuverVerdict.Selected; } }

        internal ManeuverEvaluationResult(ulong frameId, ManeuverObstacle cause, List<ManeuverCandidate> candidates,
            ManeuverCandidate selected)
        {
            FrameId = frameId; Cause = cause; Candidates = candidates.AsReadOnly(); Selected = selected;
        }

        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Maneuver frame ").Append(FrameId.ToString(CultureInfo.InvariantCulture)).Append(" cause ")
                .Append(Cause.Id).Append(' ').Append(Cause.Kind);
            for (int i = 0; i < Candidates.Count; i++) text.Append(" | ").Append(Candidates[i].ToText());
            text.Append(" | selection ").Append(Selected == null ? "aucune" : Selected.Kind + (Ready ? " pret" : " attend l'exception"));
            return text.ToString();
        }
    }

    /// <summary>Story 5.42 : situation d'une evaluation, lue dans la frame N et la politique effective ; aucune ecriture.</summary>
    public sealed class ManeuverSituation
    {
        public TrafficFrame Frame;
        public RoadId TrafficId;
        public EffectiveLaneCorridor Own;
        /// <summary>Abscisse du point de reference sur le corridor propre.</summary>
        public float SelfSMeters;
        /// <summary>Ecart cinematique e de la caisse (radians, contrat §8).</summary>
        public float SelfOffsetRadians;
        public float SpeedMetersPerSecond;
        public ManeuverObstacle Cause;
        public EffectivePolicy Policy;
        public GaugeBox Gauge;
        public float EpsilonMeters;
        public float DeltaTimeSeconds;
        /// <summary>Exceptions effectives a cette frame (instantane du coordinateur).</summary>
        public IReadOnlyList<EffectiveRuleException> Exceptions = EffectiveRuleException.None;
        /// <summary>Un refus de l'autorite est encore dans son delai D4.</summary>
        public bool ExceptionDenied;
        public SafetyLimits Limits;
        public PerceptionLimits PerceptionLimits;
        public SpatialQueryBuffer Buffer;
        public bool HazardCollectorSaturated;
    }

    /// <summary>
    /// Story 5.42 (M1-M3) : evaluation pure des candidats lateraux. Enumeration depuis la geometrie authoree, puis pour chaque
    /// candidat, dans cet ordre : faisabilite geometrique (chemin + preuve M2), conflits dynamiques (marge en temps), politique
    /// (eligibilite, creneau, risque, fenetre d'exception), exception effective, SafetyFilter. Selection : cout de politique le
    /// plus bas, ordre d'enumeration ensuite. Aucun candidat n'est prefere par sa nature.
    /// </summary>
    public static class ManeuverEvaluation
    {
        public static readonly ManeuverKind[] Enumeration =
        {
            ManeuverKind.CorridorOffset, ManeuverKind.AdjacentCorridor, ManeuverKind.OpposingCorridor, ManeuverKind.AuthorizedSurface
        };

        public static ManeuverEvaluationResult Evaluate(ManeuverSituation situation, ulong frameId)
        {
            if (situation == null || situation.Frame == null) throw new ArgumentNullException("situation");
            var candidates = new List<ManeuverCandidate>(Enumeration.Length);
            ManeuverCandidate selected = null;
            foreach (var kind in Enumeration)
            {
                var candidate = Evaluate(situation, frameId, kind);
                candidates.Add(candidate);
                bool survivor = candidate.Verdict == ManeuverVerdict.Feasible || candidate.Verdict == ManeuverVerdict.ExceptionPending;
                if (survivor && (selected == null || candidate.Cost < selected.Cost)) selected = candidate;
            }
            if (selected != null && selected.Verdict == ManeuverVerdict.Feasible) selected.Verdict = ManeuverVerdict.Selected;
            return new ManeuverEvaluationResult(frameId, situation.Cause, candidates, selected);
        }

        private static ManeuverCandidate Evaluate(ManeuverSituation s, ulong frameId, ManeuverKind kind)
        {
            var candidate = new ManeuverCandidate { Kind = kind, SlackSeconds = float.PositiveInfinity };
            var model = s.Frame.Model;
            EffectiveLaneCorridor? other = null;
            if (kind == ManeuverKind.AdjacentCorridor || kind == ManeuverKind.OpposingCorridor)
            {
                EffectiveLaneCorridor found;
                if (!(kind == ManeuverKind.AdjacentCorridor ? TryAdjacent(model, s, out found) : TryOpposing(model, s, out found)))
                    return Verdict(candidate, ManeuverVerdict.NotOffered);
                other = found;
                candidate.OtherCorridorId = found.CorridorId;
            }
            else if (kind == ManeuverKind.AuthorizedSurface) return Verdict(candidate, ManeuverVerdict.NotOffered);

            // 1. Faisabilite geometrique.
            var cause = Geometry(s, kind, other, candidate);
            if (cause != ManeuverGeometryCause.None)
            {
                candidate.GeometryCause = cause;
                return Verdict(candidate, ManeuverVerdict.GeometryInfeasible);
            }
            candidate.RequiresException = kind == ManeuverKind.OpposingCorridor && candidate.Proof.EntersOther;

            // 2. Conflits dynamiques (tactique).
            candidate.SlackSeconds = Slack(s, candidate, other, 0f);
            if (candidate.SlackSeconds < 0f) return Verdict(candidate, ManeuverVerdict.TrafficConflict);
            candidate.Risk = float.IsPositiveInfinity(candidate.SlackSeconds) ? 0f
                : candidate.DurationSeconds / (candidate.DurationSeconds + candidate.SlackSeconds);

            // 3. Politique : eligibilite, creneau et risque acceptes, fenetre de l'exception.
            var policy = s.Policy.Maneuver(kind);
            candidate.Cost = policy.Cost;
            if (!policy.Eligible) return Refuse(candidate, ManeuverPolicyRefusal.Ineligible);
            if (candidate.SlackSeconds < s.Policy.AcceptedGapSeconds) return Refuse(candidate, ManeuverPolicyRefusal.Gap);
            if (candidate.Risk > s.Policy.AcceptedRisk) return Refuse(candidate, ManeuverPolicyRefusal.Risk);
            if (candidate.RequiresException)
            {
                // D4, decision 4A : fenetre jusqu'au dernier point qui occupe l'enveloppe opposee, plus la marge ; jamais raccourcie.
                var timing = new ManeuverTiming(s.SpeedMetersPerSecond, candidate.Path.SpeedMetersPerSecond, s.Policy.Driver.MaxAcceleration,
                    s.Policy.Driver.ComfortableDeceleration);
                float leaving = timing.SecondsAt(candidate.Proof.LastOtherDistanceMeters);
                double frames = ExceptionWindowFrames(leaving, s.DeltaTimeSeconds);
                if (!WithinExceptionWindow(frames)) return Refuse(candidate, ManeuverPolicyRefusal.ExceptionWindow);
                candidate.ExceptionExpiryFrame = frameId + (ulong)Math.Max(1d, frames);
                // 4. Exception de l'autorite des regles : effective et couvrant au moins la sortie de l'enveloppe opposee (la
                // marge D4 de la demande absorbe le pas entre la demande N et le depart N+1).
                ulong needed = frameId + (ulong)Math.Max(1d, Math.Ceiling(leaving / Math.Max(1e-4f, s.DeltaTimeSeconds)));
                if (!HasEffectiveException(s.Exceptions, candidate.OtherCorridorId, needed))
                    return Verdict(candidate, s.ExceptionDenied ? ManeuverVerdict.ExceptionDenied : ManeuverVerdict.ExceptionPending);
            }

            // 5. SafetyFilter sur la premiere commande du chemin.
            var safety = ValidateSafety(s, candidate.Path, s.Frame.FrameId);
            if (safety.Verdict == SafetyVerdict.Reject || safety.Verdict == SafetyVerdict.EmergencyStop)
            {
                candidate.SafetyReason = safety.Reason;
                return Verdict(candidate, ManeuverVerdict.SafetyRejected);
            }
            return Verdict(candidate, ManeuverVerdict.Feasible);
        }

        /// <summary>
        /// D4, decision 4A : pas hote demandes pour l'exception, jusqu'a la sortie de l'enveloppe opposee plus la marge D4.
        /// </summary>
        public static double ExceptionWindowFrames(float secondsUntilLeavingOpposing, float deltaTimeSeconds)
        {
            return Math.Ceiling((secondsUntilLeavingOpposing + TrafficV2Settings.ManeuverExceptionMarginSeconds)
                / Math.Max(1e-4f, deltaTimeSeconds));
        }

        /// <summary>Fail-closed : au-dela de MaxRuleExceptionFrames (ou non fini), ExceptionWindow.</summary>
        public static bool WithinExceptionWindow(double frames)
        {
            return frames <= TrafficV2Settings.MaxRuleExceptionFrames;
        }

        /// <summary>
        /// Exception OpposingCorridor effective dont la portee est le corridor du candidat et qui couvre la fenetre requise
        /// (expiration au moins <paramref name="requiredExpiryFrame"/>) : une exception plus courte n'autorise aucun depart.
        /// </summary>
        public static bool HasEffectiveException(IReadOnlyList<EffectiveRuleException> exceptions, RoadId corridor,
            ulong requiredExpiryFrame = 0UL)
        {
            if (exceptions == null) return false;
            for (int i = 0; i < exceptions.Count; i++)
                if (exceptions[i] != null && exceptions[i].Rule == TrafficRule.OpposingCorridor && exceptions[i].Request.Scope == corridor
                    && exceptions[i].ExpiryFrame >= requiredExpiryFrame)
                    return true;
            return false;
        }

        /// <summary>
        /// M2 et D2-D3 : decalage vise, vitesse de manoeuvre, longueurs de depart, de maintien et de retour, puis preuve continue.
        /// </summary>
        private static ManeuverGeometryCause Geometry(ManeuverSituation s, ManeuverKind kind, EffectiveLaneCorridor? other,
            ManeuverCandidate candidate)
        {
            var model = s.Frame.Model;
            var profile = model.DrivabilityProfile;
            var gauge = s.Gauge;
            var cause = s.Cause;
            var curve = s.Own.Curve;
            if (curve == null || !cause.Valid || !(s.EpsilonMeters >= 0f) || !(s.DeltaTimeSeconds > 0f))
                return ManeuverGeometryCause.InvalidInput;
            float half = gauge.HalfWidthMeters, length = gauge.LengthMeters, eps = s.EpsilonMeters;
            float clearLong = TrafficV2Settings.ManeuverLongitudinalClearanceMeters;
            float clearLat = TrafficV2Settings.ManeuverLateralClearanceMeters;
            float aLat = TrafficV2Settings.ManeuverLateralAccelerationMetersPerSecondSquared;
            var at = curve.Sample(Mathf.Clamp(cause.NearSMeters, curve.StartS, curve.Length));

            float target;
            if (kind == ManeuverKind.CorridorOffset)
            {
                // Gonflement de la preuve : epsilon_t et le reste entre echantillons, majore par un pas de preuve.
                float inflation = eps + TrafficV2Settings.ManeuverProofStepMeters;
                float left = cause.LateralMinMeters - clearLat - half - inflation;
                float right = cause.LateralMaxMeters + clearLat + half + inflation;
                bool fitsLeft = left - half - inflation >= -at.HalfWidthLeftMeters;
                bool fitsRight = right + half + inflation <= at.HalfWidthRightMeters;
                if (!fitsLeft && !fitsRight) return ManeuverGeometryCause.NoLateralRoom;
                target = fitsLeft && (!fitsRight || Math.Abs(left) <= Math.Abs(right)) ? left : right;
            }
            else
            {
                if (other.Value.Curve == null) return ManeuverGeometryCause.InvalidInput;
                var projection = other.Value.Curve.Project(at.Position);
                target = Vector3.Dot(projection.Point.Position - at.Position, at.Right);
            }
            float delta = Math.Abs(target);
            if (!(delta > 0f)) return ManeuverGeometryCause.InvalidInput;

            var driver = s.Policy.Driver;
            float desired = driver.DesiredSpeed;
            float departEnd = cause.NearSMeters - clearLong - length * 0.5f;
            float available = departEnd - s.SelfSMeters;
            if (!(available > 0f)) return ManeuverGeometryCause.TooClose;
            float transition = ManeuverPath.TransitionLength(delta, ManeuverPath.AllowedCurvature(profile, desired, aLat));
            float speed = desired;
            if (!(transition <= available))
            {
                // D3 : la longueur disponible plafonne la courbure, donc la vitesse ; le braquage est verifie par la preuve.
                transition = available;
                speed = Math.Min(desired, (float)Math.Sqrt(aLat * transition * transition / (ManeuverPath.TransitionCurvatureFactor * delta)));
            }
            if (!(speed > 0f)) return ManeuverGeometryCause.SteeringInfeasible;
            float causeSpeed = cause.SpeedMetersPerSecond;
            if (causeSpeed > 0f && speed - causeSpeed < TrafficV2Settings.ManeuverMinimumClosingSpeedMetersPerSecond)
                return ManeuverGeometryCause.NoClosingSpeed;
            var timing = new ManeuverTiming(s.SpeedMetersPerSecond, speed, driver.MaxAcceleration, driver.ComfortableDeceleration);

            // Maintien : la caisse depasse la cause (degagement compris) ; etire par v_m / (v_m - v_cause) pour une cause mobile.
            float startS = s.SelfSMeters, departEndS = startS + transition;
            float farAtDepartEnd = cause.FarSMeters + causeSpeed * timing.SecondsAt(transition);
            float required = farAtDepartEnd + clearLong + length * 0.5f - departEndS;
            float hold = required > 0f ? required * speed / (speed - causeSpeed) : 0f;
            float returnLength = ManeuverPath.TransitionLength(delta, ManeuverPath.AllowedCurvature(profile, speed, aLat));
            float tail = speed * CollisionThresholds.StableSeconds + 1f;
            float stop = driver.ComfortableDeceleration > 0f ? speed * speed / (2f * driver.ComfortableDeceleration) : float.PositiveInfinity;
            if (!(departEndS + hold + returnLength + tail + stop <= curve.Length)) return ManeuverGeometryCause.CorridorTooShort;

            ManeuverPath path;
            try
            {
                path = ManeuverPath.Build(s.Own.CorridorId, curve, startS, 0f, target, transition, hold, returnLength, tail, speed,
                    TrafficV2Settings.ManeuverProofStepMeters, profile.Declared ? profile.ReferencePointAheadRearAxleMeters : 0f,
                    s.SelfOffsetRadians);
            }
            catch (ArgumentException) { return ManeuverGeometryCause.InvalidInput; }
            var proof = ManeuverProof.Prove(path, s.Own, other, profile, gauge, eps, cause, timing, clearLong, clearLat,
                model.ValidationProfile.EnvelopeOverlapToleranceMeters);
            candidate.Path = path;
            candidate.Proof = proof;
            candidate.DurationSeconds = timing.SecondsAt(path.DistanceAtReference(path.ReturnEndSMeters));
            if (!proof.Proven) candidate.Path = null;
            return proof.Cause;
        }

        /// <summary>
        /// M3 : marge en temps = arrivee la plus precoce d'un acteur (vitesse constante) ou d'un danger dans la region balayee,
        /// moins la duree restante. Region sur l'autre corridor (et ses elements amont), et devant l'agent sur le corridor
        /// propre ; la cause contournee et l'agent sont exclus. Un occupant deja dans la region : marge negative.
        /// </summary>
        /// <param name="progressMeters">Distance de chemin deja parcourue (0 a l'evaluation).</param>
        public static float Slack(ManeuverSituation s, ManeuverCandidate candidate, EffectiveLaneCorridor? other, float progressMeters)
        {
            var frame = s.Frame;
            var path = candidate.Path;
            var timing = new ManeuverTiming(s.SpeedMetersPerSecond, path.SpeedMetersPerSecond, s.Policy.Driver.MaxAcceleration,
                s.Policy.Driver.ComfortableDeceleration);
            float remaining = timing.SecondsAt(Math.Max(0f, path.DistanceAtReference(path.ReturnEndSMeters) - progressMeters));
            float arrival = float.PositiveInfinity;
            if (other.HasValue && candidate.Proof.EntersOther && progressMeters <= candidate.Proof.LastOtherDistanceMeters)
            {
                float r0 = candidate.Proof.OtherSMinMeters, r1 = candidate.Proof.OtherSMaxMeters;
                TrafficActor agent;
                if (progressMeters > 0f && other.Value.Curve != null && frame.TryGetActor(s.TrafficId, out agent))
                {
                    // Pendant la manoeuvre, seule la region encore devant la caisse compte : un acteur deja croise n'y est plus.
                    var mine = other.Value.Curve.Project(agent.Pose.Position);
                    float margin = s.Gauge.LengthMeters * 0.5f + s.EpsilonMeters;
                    bool antiparallel = Vector3.Dot(mine.Point.Tangent, agent.Pose.Forward) < 0f;
                    if (antiparallel) r1 = Math.Min(r1, mine.SMeters + margin);
                    else r0 = Math.Max(r0, mine.SMeters - margin);
                }
                if (r1 >= r0)
                    arrival = Math.Min(arrival, Arrival(frame, other.Value.CorridorId, r0, r1, s.TrafficId, s.Cause.Id,
                        float.NegativeInfinity, true));
            }
            float ownEnd = path.ReturnEndSMeters + s.Gauge.LengthMeters * 0.5f;
            ElementOccupant self;
            float selfFront = frame.TryGetOccupancy(s.TrafficId, out self) && self.ElementId == s.Own.CorridorId ? self.SMaxMeters
                : s.SelfSMeters;
            arrival = Math.Min(arrival, Arrival(frame, s.Own.CorridorId, selfFront, ownEnd, s.TrafficId, s.Cause.Id, selfFront, false));
            arrival = Math.Min(arrival, HazardArrival(frame, s, path, selfFront, ownEnd));
            return float.IsPositiveInfinity(arrival) ? float.PositiveInfinity : arrival - remaining;
        }

        private static float Arrival(TrafficFrame frame, RoadId corridor, float r0, float r1, RoadId self, RoadId cause,
            float behindLimit, bool upstream)
        {
            float arrival = float.PositiveInfinity;
            var occupants = frame.GetOccupants(corridor);
            for (int i = 0; i < occupants.Count; i++)
            {
                var o = occupants[i];
                if (o.TrafficId == self || o.TrafficId == cause || o.SMaxMeters <= behindLimit) continue;
                float speed = Math.Max(0f, o.SpeedMetersPerSecond);
                if (o.SMaxMeters >= r0 && o.SMinMeters <= r1) return float.NegativeInfinity;
                if (o.SMaxMeters < r0 && speed > 0f) arrival = Math.Min(arrival, (r0 - o.SMaxMeters) / speed);
            }
            if (!upstream) return arrival;
            var model = frame.Model;
            for (int m = 0; m < model.Movements.Count; m++)
            {
                var movement = model.Movements[m];
                if (movement.ToCorridorId != corridor) continue;
                var onMovement = frame.GetOccupants(movement.Id);
                for (int i = 0; i < onMovement.Count; i++)
                {
                    var o = onMovement[i];
                    float speed = Math.Max(0f, o.SpeedMetersPerSecond);
                    if (o.TrafficId == self || o.TrafficId == cause || !(speed > 0f)) continue;
                    arrival = Math.Min(arrival, (movement.LengthMeters - o.SMaxMeters + r0) / speed);
                }
            }
            return arrival;
        }

        /// <summary>Dangers (hors acteurs) dans la bande balayee du corridor propre : marge negative, quel que soit leur genre.</summary>
        private static float HazardArrival(TrafficFrame frame, ManeuverSituation s, ManeuverPath path, float r0, float r1)
        {
            var curve = s.Own.Curve;
            float reach = s.Gauge.HalfWidthMeters + s.EpsilonMeters;
            float low = Math.Min(0f, path.TargetOffsetMeters) - reach, high = Math.Max(0f, path.TargetOffsetMeters) + reach;
            for (int i = 0; i < frame.Hazards.Count; i++)
            {
                var hazard = frame.Hazards[i];
                if (hazard.Id == s.Cause.Id) continue;
                var projection = curve.Project(hazard.Bounds.Center);
                float half = Math.Max(hazard.Bounds.Extents.x, hazard.Bounds.Extents.z);
                if (projection.LongitudinalOverrunMeters > half || projection.SMeters + half < r0 || projection.SMeters - half > r1) continue;
                if (projection.LateralOffsetMeters + half >= low && projection.LateralOffsetMeters - half <= high)
                    return float.NegativeInfinity;
            }
            return float.PositiveInfinity;
        }

        /// <summary>M1 : corridor de meme sens d'une adjacence authoree qui couvre l'etendue de la manoeuvre.</summary>
        private static bool TryAdjacent(CompiledRoadModel model, ManeuverSituation s, out EffectiveLaneCorridor corridor)
        {
            corridor = default(EffectiveLaneCorridor);
            RoadId best = RoadId.None;
            for (int i = 0; i < model.Adjacencies.Count; i++)
            {
                var adjacency = model.Adjacencies[i];
                if (adjacency.FromCorridorId != s.Own.CorridorId || adjacency.FromStartSMeters > s.SelfSMeters
                    || adjacency.FromEndSMeters < s.Cause.FarSMeters) continue;
                if (best.IsEmpty || adjacency.ToCorridorId.CompareTo(best) < 0) best = adjacency.ToCorridorId;
            }
            return !best.IsEmpty && model.TryGetCorridor(best, out corridor) && corridor.Curve != null;
        }

        /// <summary>
        /// M1 : corridor antiparallele de la meme section le plus proche lateralement (TrafficId departage) ; la contiguite est
        /// prouvee par M2.
        /// </summary>
        private static bool TryOpposing(CompiledRoadModel model, ManeuverSituation s, out EffectiveLaneCorridor corridor)
        {
            corridor = default(EffectiveLaneCorridor);
            var at = s.Own.Curve.Sample(Mathf.Clamp(s.Cause.NearSMeters, s.Own.Curve.StartS, s.Own.Curve.Length));
            RoadId best = RoadId.None;
            float bestDistance = float.PositiveInfinity;
            var ids = model.GetCorridorsInSection(s.Own.SectionId);
            for (int i = 0; i < ids.Count; i++)
            {
                EffectiveLaneCorridor candidate;
                if (ids[i] == s.Own.CorridorId || !model.TryGetCorridor(ids[i], out candidate) || candidate.Curve == null) continue;
                var projection = candidate.Curve.Project(at.Position);
                if (Vector3.Dot(projection.Point.Tangent, at.Tangent) >= 0f) continue;
                float distance = projection.DistanceMeters;
                if (distance < bestDistance || (distance == bestDistance && ids[i].CompareTo(best) < 0))
                {
                    best = ids[i];
                    bestDistance = distance;
                }
            }
            return !best.IsEmpty && model.TryGetCorridor(best, out corridor);
        }

        /// <summary>
        /// M1, etape 5 : SafetyFilter sur la premiere commande de la reference, datee de la frame evaluee, proximite mesuree le
        /// long du chemin.
        /// </summary>
        private static SafetyResult ValidateSafety(ManeuverSituation s, ManeuverPath path, ulong frameId)
        {
            TrafficActor actor;
            if (!s.Frame.TryGetActor(s.TrafficId, out actor) || s.Buffer == null)
                return SafetyFilter.Evaluate(default(MotionCommand), frameId, null, s.TrafficId, s.SpeedMetersPerSecond, path, null, s.Limits);
            ObservationChannel<ObstacleFact> facts;
            var near = NearField(s.Frame, s.TrafficId, path, s.PerceptionLimits, s.Buffer, s.HazardCollectorSaturated, out facts);
            var command = TacticalDecision.ManeuverCommand(frameId, TrafficV2Settings.PlanValiditySteps, path,
                s.Frame.Model.DrivabilityProfile, s.Policy.Driver, actor.Pose.Position, actor.Pose.Forward, s.SpeedMetersPerSecond,
                s.DeltaTimeSeconds, near);
            return SafetyFilter.Evaluate(command, frameId, s.Frame, s.TrafficId, s.SpeedMetersPerSecond, path, near, s.Limits);
        }

        /// <summary>
        /// M5 : proximite le long de la reference de manoeuvre (aucun leader structure) ; obstacles du couloir balaye avec leur
        /// vitesse le long du chemin. Une perception saturee ou indisponible porte sa raison.
        /// </summary>
        public static LongitudinalPerception NearField(TrafficFrame frame, RoadId trafficId, ManeuverPath path, PerceptionLimits limits,
            SpatialQueryBuffer buffer, bool collectorSaturated, out ObservationChannel<ObstacleFact> facts)
        {
            TrafficActor actor;
            facts = null;
            try { facts = TrafficPerception.ObserveAlong(frame, trafficId, path.Piece.Curve, limits, buffer); }
            catch (ArgumentException) { return new LongitudinalPerception(PerceptionUnavailableReason.ChannelUnavailable, null, null); }
            if (!frame.TryGetActor(trafficId, out actor))
                return new LongitudinalPerception(PerceptionUnavailableReason.ChannelUnavailable, null, null);
            float front = path.Piece.Curve.Project(actor.Pose.Position).SMeters + actor.Pose.Footprint.FrontMeters;
            return NearField(facts, path, front, collectorSaturated);
        }

        private static LongitudinalPerception NearField(ObservationChannel<ObstacleFact> channel, ManeuverPath path, float front,
            bool collectorSaturated)
        {
            var reason = channel.Status != PerceptionStatus.Evaluated ? PerceptionUnavailableReason.ChannelUnavailable
                : channel.Saturated ? PerceptionUnavailableReason.ChannelSaturated
                : collectorSaturated ? PerceptionUnavailableReason.HazardCollectorSaturated : PerceptionUnavailableReason.None;
            var obstacles = new List<LongitudinalObstacle>();
            for (int i = 0; i < channel.Items.Count; i++)
            {
                var fact = channel.Items[i];
                if (!fact.InSweptPath) continue;
                float at = Mathf.Clamp(front + fact.NearDistanceMeters, 0f, path.LengthMeters);
                float along = Vector3.Dot(fact.Velocity, path.Piece.Curve.Sample(at).Tangent);
                obstacles.Add(new LongitudinalObstacle(fact.Id, fact.Kind, fact.NearDistanceMeters,
                    along > 0f && !float.IsNaN(along) && !float.IsInfinity(along) ? along : 0f));
            }
            return new LongitudinalPerception(reason, null, obstacles);
        }

        /// <summary>
        /// M4-M5 : decision de supervision d'une manoeuvre non engagee, a une progression donnee. Abandon si la marge restante
        /// est negative, ou si l'exception a disparu alors que la reference restante occupe encore le corridor oppose. Une fin
        /// ScopeExited (retour normal prouve) n'est jamais une perte. Retour prouvable derriere la cause : Abort ; sinon Commit.
        /// </summary>
        /// <param name="exceptionUntilMeters">La reference occupe l'autre corridor jusqu'a cette distance (-1 : jamais).</param>
        /// <param name="endedByScopeExit">L'autorite a publie la fin ScopeExited de l'exception de ce vehicule.</param>
        public static ManeuverSupervision Supervise(ManeuverSituation s, ManeuverCandidate current, float progressMeters,
            float offsetRadians, EffectiveLaneCorridor? other, float exceptionUntilMeters, bool endedByScopeExit,
            out ManeuverCandidate back, out bool exceptionLost)
        {
            back = null;
            exceptionLost = false;
            if (s == null || current == null || current.Path == null) return ManeuverSupervision.Commit;
            exceptionLost = progressMeters <= exceptionUntilMeters && !endedByScopeExit
                && !HasEffectiveException(s.Exceptions, current.OtherCorridorId);
            bool conflict = Slack(s, current, other, progressMeters) < 0f;
            if (!exceptionLost && !conflict) return ManeuverSupervision.Continue;
            back = PlanReturn(s, current.Path, progressMeters, offsetRadians, other, current.Kind);
            return back != null ? ManeuverSupervision.Abort : ManeuverSupervision.Commit;
        }


        /// <summary>
        /// M6 : emprise de la cause lue dans la frame, par le meme calcul pour tous les genres. Acteur : empreinte declaree et
        /// vitesse tangentielle ; danger : boite de la frame et vitesse le long du corridor. Faux si la cause a disparu.
        /// </summary>
        public static bool TryCause(TrafficFrame frame, RoadCurve curve, RoadId id, PerceivedObstacleKind kind, out ManeuverObstacle cause)
        {
            cause = default(ManeuverObstacle);
            if (frame == null || curve == null || id.IsEmpty) return false;
            TrafficActor actor;
            if (frame.TryGetActor(id, out actor))
            {
                if (!actor.FootprintDeclared) return false;
                cause = ManeuverObstacle.FromCorners(id, PerceivedObstacleKind.TrafficActor, curve, TrafficFrame.Corners(actor.Pose),
                    actor.TangentialSpeedMetersPerSecond);
                return cause.Valid;
            }
            for (int i = 0; i < frame.Hazards.Count; i++)
            {
                var hazard = frame.Hazards[i];
                if (hazard.Id != id) continue;
                Vector3 c = hazard.Bounds.Center, e = hazard.Bounds.Extents;
                var corners = new Vector3[8];
                for (int k = 0; k < 8; k++)
                    corners[k] = c + new Vector3((k & 1) == 0 ? -e.x : e.x, (k & 2) == 0 ? -e.y : e.y, (k & 4) == 0 ? -e.z : e.z);
                float along = Vector3.Dot(hazard.Velocity, curve.Project(c).Point.Tangent);
                cause = ManeuverObstacle.FromCorners(id, kind, curve, corners, along);
                return cause.Valid;
            }
            return false;
        }

        /// <summary>
        /// M5 : reference de retour depuis la progression courante, decalage courant ramene a 0 a la vitesse courante, prouvee
        /// (M2) et finie derriere la cause (degagement compris, cause predite). Nulle sinon : la manoeuvre est alors engagee.
        /// </summary>
        /// <param name="offsetRadians">Ecart cinematique e de la caisse sur la reference courante a cette progression.</param>
        public static ManeuverCandidate PlanReturn(ManeuverSituation s, ManeuverPath current, float progressMeters, float offsetRadians,
            EffectiveLaneCorridor? other, ManeuverKind kind)
        {
            if (s == null || current == null || s.Own.Curve == null || !s.Cause.Valid) return null;
            var profile = s.Frame.Model.DrivabilityProfile;
            float aLat = TrafficV2Settings.ManeuverLateralAccelerationMetersPerSecondSquared;
            float startS = current.ReferenceAt(progressMeters);
            float offset = current.OffsetAtDistance(progressMeters);
            if (!(Math.Abs(offset) > 1e-3f)) return null;
            float speed = Math.Max(1f, Math.Min(current.SpeedMetersPerSecond, s.SpeedMetersPerSecond));
            float length = ManeuverPath.TransitionLength(Math.Abs(offset), ManeuverPath.AllowedCurvature(profile, speed, aLat));
            float tail = speed * CollisionThresholds.StableSeconds + 1f;
            var timing = new ManeuverTiming(s.SpeedMetersPerSecond, speed, s.Policy.Driver.MaxAcceleration,
                s.Policy.Driver.ComfortableDeceleration);
            float behind = startS + length + tail + s.Gauge.LengthMeters * 0.5f + TrafficV2Settings.ManeuverLongitudinalClearanceMeters;
            if (!(length > 0f) || float.IsInfinity(length) || behind > s.Cause.NearSMeters) return null;
            // e de la caisse rapportee a la tangente du retour (o' = 0 au depart) : saut de tangente signe entre les deux chemins.
            var from = current.Piece.Curve.Sample(progressMeters);
            var to = s.Own.Curve.Sample(startS);
            to.Position = from.Position;
            float e0 = offsetRadians + (float)RoadCurve.SignedTangentJumpRadians(from, to);
            ManeuverPath path;
            try
            {
                path = ManeuverPath.Build(s.Own.CorridorId, s.Own.Curve, startS, offset, offset, 0f, 0f, length, tail, speed,
                    TrafficV2Settings.ManeuverProofStepMeters, profile.Declared ? profile.ReferencePointAheadRearAxleMeters : 0f, e0);
            }
            catch (ArgumentException) { return null; }
            var proof = ManeuverProof.Prove(path, s.Own, other, profile, s.Gauge, s.EpsilonMeters, s.Cause, timing,
                TrafficV2Settings.ManeuverLongitudinalClearanceMeters, TrafficV2Settings.ManeuverLateralClearanceMeters,
                s.Frame.Model.ValidationProfile.EnvelopeOverlapToleranceMeters);
            if (!proof.Proven) return null;
            return new ManeuverCandidate { Kind = kind, Verdict = ManeuverVerdict.Selected, Path = path, Proof = proof,
                OtherCorridorId = other.HasValue ? other.Value.CorridorId : RoadId.None, SlackSeconds = float.PositiveInfinity,
                DurationSeconds = timing.SecondsAt(path.DistanceAtReference(path.ReturnEndSMeters)),
                RequiresException = kind == ManeuverKind.OpposingCorridor && proof.EntersOther };
        }

        private static ManeuverCandidate Verdict(ManeuverCandidate candidate, ManeuverVerdict verdict)
        {
            candidate.Verdict = verdict;
            return candidate;
        }

        private static ManeuverCandidate Refuse(ManeuverCandidate candidate, ManeuverPolicyRefusal refusal)
        {
            candidate.PolicyRefusal = refusal;
            return Verdict(candidate, ManeuverVerdict.PolicyRefused);
        }
    }

    /// <summary>
    /// Story 5.42 (D1) : declenchement d'une evaluation. La meme cause liante (leader suivi, obstacle, ou leur maintien a
    /// l'arret), legitime et lente (au plus 0,5 x vitesse desiree), tenue 2 s. Il ouvre une evaluation, jamais un candidat.
    /// </summary>
    public sealed class ManeuverTrigger
    {
        private RoadId cause;
        private ulong since;

        public RoadId CauseId { get { return cause; } }
        public LongitudinalCandidateKind CauseKind { get; private set; }
        public PerceivedObstacleKind ObstacleKind { get; private set; }
        public float CauseSpeedMetersPerSecond { get; private set; }

        public void Reset() { cause = RoadId.None; since = 0UL; }

        public bool Observe(ulong frameId, LongitudinalDecision decision, IReadOnlyList<Blocker> blockers, float desiredSpeed,
            float deltaTimeSeconds)
        {
            if (decision == null) { Reset(); return false; }
            var binding = decision.Binding;
            bool hold = binding.Kind == LongitudinalCandidateKind.StopHold;
            Blocker dominant;
            bool illegitimate = blockers != null && BlockerTracker.TryGetDominant(blockers, out dominant)
                && (dominant.Kind == BlockerKind.Leader || dominant.Kind == BlockerKind.Obstacle) && !dominant.Legitimate;
            return Observe(frameId, hold ? decision.Hold.Cause : binding.Kind, hold ? decision.Hold.SourceId : binding.SourceId,
                hold ? decision.Hold.SourceSpeedMetersPerSecond : binding.SourceSpeedMetersPerSecond, binding.ObstacleKind, illegitimate,
                desiredSpeed, deltaTimeSeconds);
        }

        /// <summary>D1 sur des faits primitifs : genre et source de la cause liante, sa vitesse, et un blocker dominant illegitime.</summary>
        public bool Observe(ulong frameId, LongitudinalCandidateKind kind, RoadId source, float speed, PerceivedObstacleKind obstacleKind,
            bool illegitimateBlocker, float desiredSpeed, float deltaTimeSeconds)
        {
            if ((kind != LongitudinalCandidateKind.LeaderFollowing && kind != LongitudinalCandidateKind.Obstacle) || source.IsEmpty
                || float.IsNaN(speed) || speed > TrafficV2Settings.ManeuverSlowSpeedFraction * desiredSpeed || illegitimateBlocker)
            { Reset(); return false; }
            if (source != cause) { cause = source; since = frameId; }
            CauseKind = kind;
            ObstacleKind = kind == LongitudinalCandidateKind.Obstacle ? obstacleKind : PerceivedObstacleKind.TrafficActor;
            CauseSpeedMetersPerSecond = Math.Max(0f, speed);
            return (frameId - since) * (double)deltaTimeSeconds >= TrafficV2Settings.ManeuverConsiderSeconds;
        }
    }
}
