using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    /// <summary>Story 5.42 : cause stable d'une infaisabilite geometrique d'un candidat de manoeuvre.</summary>
    public enum ManeuverGeometryCause
    {
        None = 0,
        InvalidInput = 1,
        /// <summary>Decalage dans le corridor : aucun cote ne laisse la place au gabarit gonfle.</summary>
        NoLateralRoom = 2,
        /// <summary>L'enveloppe de l'autre corridor ne jouxte pas celle du corridor propre.</summary>
        EnvelopeNotContiguous = 3,
        /// <summary>Un coin gonfle sort de l'union des enveloppes autorisees.</summary>
        EnvelopeExceeded = 4,
        /// <summary>Un coin gonfle depasse un bout de corridor : il toucherait un mouvement de carrefour.</summary>
        CorridorEnd = 5,
        /// <summary>Le retour et l'arret qui suit ne tiennent pas avant la fin du corridor.</summary>
        CorridorTooShort = 6,
        /// <summary>Braquage de la pose nominale ou courbure d'admission depasses a la vitesse de manoeuvre.</summary>
        SteeringInfeasible = 7,
        /// <summary>Le gabarit gonfle coupe l'obstacle contourne, degagements compris.</summary>
        ObstacleClearance = 8,
        /// <summary>Aucune longueur de depart avant l'obstacle.</summary>
        TooClose = 9,
        /// <summary>Depassement : v_m - v_cause sous la vitesse de rapprochement minimale (D3).</summary>
        NoClosingSpeed = 10,
        LateralAccelerationExceeded = 11,
        AdjacencyNotCovered = 12
    }

    /// <summary>
    /// Story 5.42 : emprise d'une cause (vehicule, obstacle, pieton) dans le repere du corridor propre, depuis ses coins monde.
    /// Le meme calcul vaut pour tous les genres : seul le fait de perception les distingue.
    /// </summary>
    public readonly struct ManeuverObstacle
    {
        public readonly RoadId Id;
        public readonly PerceivedObstacleKind Kind;
        public readonly float NearSMeters;
        public readonly float FarSMeters;
        public readonly float LateralMinMeters;
        public readonly float LateralMaxMeters;
        /// <summary>Vitesse le long du corridor propre, positive ou nulle (m/s).</summary>
        public readonly float SpeedMetersPerSecond;
        private readonly Vector3[] worldCorners;
        public readonly Vector3 WorldVelocity;
        public bool HasWorldFootprint { get { return worldCorners != null; } }

        public ManeuverObstacle(RoadId id, PerceivedObstacleKind kind, float nearS, float farS, float lateralMin, float lateralMax,
            float speed)
        {
            Id = id; Kind = kind; NearSMeters = nearS; FarSMeters = farS; LateralMinMeters = lateralMin; LateralMaxMeters = lateralMax;
            SpeedMetersPerSecond = speed;
            worldCorners = null; WorldVelocity = Vector3.zero;
        }

        private ManeuverObstacle(ManeuverObstacle bounds, IReadOnlyList<Vector3> corners, Vector3 velocity) : this(bounds.Id,
            bounds.Kind, bounds.NearSMeters, bounds.FarSMeters, bounds.LateralMinMeters, bounds.LateralMaxMeters, bounds.SpeedMetersPerSecond)
        {
            worldCorners = new Vector3[corners.Count];
            for (int i = 0; i < corners.Count; i++) worldCorners[i] = corners[i];
            WorldVelocity = velocity;
        }

        /// <summary>Emprise monde conservative entre deux instants, translation par le vecteur vitesse complet.</summary>
        public void WorldBounds(float fromSeconds, float toSeconds, out Vector3 min, out Vector3 max)
        {
            min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < worldCorners.Length; i++)
            {
                var a = worldCorners[i] + WorldVelocity * fromSeconds;
                var b = worldCorners[i] + WorldVelocity * toSeconds;
                min = Vector3.Min(min, Vector3.Min(a, b)); max = Vector3.Max(max, Vector3.Max(a, b));
            }
        }

        public bool Valid
        {
            get
            {
                return !Id.IsEmpty && Finite(NearSMeters) && Finite(FarSMeters) && FarSMeters >= NearSMeters
                    && Finite(LateralMinMeters) && Finite(LateralMaxMeters) && LateralMaxMeters >= LateralMinMeters
                    && Finite(SpeedMetersPerSecond) && SpeedMetersPerSecond >= 0f;
            }
        }

        /// <summary>Projection des coins monde sur la courbe du corridor propre (abscisse et lateral).</summary>
        public static ManeuverObstacle FromCorners(RoadId id, PerceivedObstacleKind kind, RoadCurve curve, IReadOnlyList<Vector3> corners,
            float speed)
        {
            // Les acteurs localises gardent leur prediction tangentielle dans le repere courbe du corridor.
            return ProjectCorners(id, kind, curve, corners, speed);
        }

        public static ManeuverObstacle FromCorners(RoadId id, PerceivedObstacleKind kind, RoadCurve curve, IReadOnlyList<Vector3> corners,
            Vector3 velocity)
        {
            var projection = curve.Project(corners[0]);
            return new ManeuverObstacle(ProjectCorners(id, kind, curve, corners, Vector3.Dot(velocity, projection.Point.Tangent)),
                corners, velocity);
        }

        private static ManeuverObstacle ProjectCorners(RoadId id, PerceivedObstacleKind kind, RoadCurve curve,
            IReadOnlyList<Vector3> corners, float speed)
        {
            float near = float.PositiveInfinity, far = float.NegativeInfinity, low = float.PositiveInfinity, high = float.NegativeInfinity;
            for (int i = 0; i < corners.Count; i++)
            {
                var projection = curve.Project(corners[i]);
                float s = projection.SMeters + (projection.SMeters >= curve.Length - 1e-4f ? projection.LongitudinalOverrunMeters
                    : -projection.LongitudinalOverrunMeters);
                near = Math.Min(near, s); far = Math.Max(far, s);
                low = Math.Min(low, projection.LateralOffsetMeters); high = Math.Max(high, projection.LateralOffsetMeters);
            }
            return new ManeuverObstacle(id, kind, near, far, low, high, Finite(speed) ? Math.Max(0f, speed) : 0f);
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    /// <summary>
    /// Story 5.42, D3 : temps de parcours d'une distance depuis v0, a l'acceleration du profil jusqu'a v_m (deceleration de
    /// confort si v0 depasse v_m), puis a v_m.
    /// </summary>
    public readonly struct ManeuverTiming
    {
        public readonly float InitialSpeedMetersPerSecond;
        public readonly float SpeedMetersPerSecond;
        public readonly float AccelerationMetersPerSecondSquared;
        public readonly float DecelerationMetersPerSecondSquared;

        public ManeuverTiming(float initialSpeed, float speed, float acceleration, float deceleration)
        {
            InitialSpeedMetersPerSecond = Math.Max(0f, initialSpeed); SpeedMetersPerSecond = speed;
            AccelerationMetersPerSecondSquared = acceleration; DecelerationMetersPerSecondSquared = deceleration;
        }

        public float SecondsAt(float distanceMeters)
        {
            float v0 = InitialSpeedMetersPerSecond, v = SpeedMetersPerSecond, d = Math.Max(0f, distanceMeters);
            if (!(v > 0f)) return float.PositiveInfinity;
            float rate = v0 < v ? AccelerationMetersPerSecondSquared : DecelerationMetersPerSecondSquared;
            if (!(rate > 0f) || v0 == v) return d / v;
            float ramp = Math.Abs(v * v - v0 * v0) / (2f * rate);
            if (d <= ramp)
            {
                float a = v0 < v ? rate : -rate;
                float speed = (float)Math.Sqrt(Math.Max(0d, v0 * (double)v0 + 2d * a * d));
                return Math.Abs(speed - v0) / rate;
            }
            return Math.Abs(v - v0) / rate + (d - ramp) / v;
        }
    }

    /// <summary>
    /// Story 5.42 : reference de manoeuvre, decalage o(s) C2 sur la courbe du corridor propre (depart, maintien, retour, puis
    /// une queue a o = 0 pour la stabilisation). Echantillons au pas de preuve D2, abscisse = longueur d'arc du chemin. Une
    /// <see cref="TrackPiece"/> et une <see cref="ReferenceTrack"/> la portent : suivi, pose cinematique et mesure epsilon_t
    /// sont ceux de la conduite nominale. Ce n'est pas une reference compilee : elle ne sert que pendant le but Maneuver.
    /// </summary>
    public sealed class ManeuverPath : IPathGeometry
    {
        /// <summary>|q''| maximal de q(u) = 6u^5 - 15u^4 + 10u^3, soit 10 / racine(3) (D2).</summary>
        public const float TransitionCurvatureFactor = 5.7735027f;

        private readonly float[] referenceS;
        private readonly float[] offsets;
        private readonly RoadCurveSample[] samples;
        private readonly PathSpan[] spans;

        public RoadId CorridorId { get; }
        public RoadCurve Reference { get; }
        public float StartSMeters { get; }
        public float DepartEndSMeters { get; }
        public float ReturnStartSMeters { get; }
        /// <summary>Fin du retour (o = 0) ; la queue de stabilisation suit jusqu'a <see cref="TailEndSMeters"/>.</summary>
        public float ReturnEndSMeters { get; }
        public float TailEndSMeters { get; }
        public float StartOffsetMeters { get; }
        public float TargetOffsetMeters { get; }
        /// <summary>v_m (m/s).</summary>
        public float SpeedMetersPerSecond { get; }
        public ReferenceTrack Track { get; }
        public IReadOnlyList<RoadCurveSample> Samples { get { return samples; } }
        public float LengthMeters { get; }
        public IReadOnlyList<PathSpan> Spans { get; }
        public float MaximumAbsoluteCurvaturePerMeter { get; }

        private ManeuverPath(RoadId corridorId, RoadCurve reference, float s0, float s1, float s2, float s3, float s4, float oStart,
            float oTarget, float speed, List<float> refS, List<float> offs, List<RoadCurveSample> points, float a, float e0)
        {
            CorridorId = corridorId; Reference = reference;
            StartSMeters = s0; DepartEndSMeters = s1; ReturnStartSMeters = s2; ReturnEndSMeters = s3; TailEndSMeters = s4;
            StartOffsetMeters = oStart; TargetOffsetMeters = oTarget; SpeedMetersPerSecond = speed;
            referenceS = refS.ToArray(); offsets = offs.ToArray(); samples = points.ToArray();
            LengthMeters = samples[samples.Length - 1].SMeters;
            float kappa = 0f;
            for (int i = 0; i < samples.Length; i++) kappa = Math.Max(kappa, Math.Abs(samples[i].CurvaturePerMeter));
            MaximumAbsoluteCurvaturePerMeter = kappa;
            Track = new ReferenceTrack(new[] { new TrackPiece(RoadElementKind.LaneCorridor, corridorId, 0f, 0f, LengthMeters, samples) },
                a, e0);
            spans = new[] { new PathSpan(RoadElementKind.LaneCorridor, corridorId, 0f, LengthMeters, 0f, LengthMeters) };
            Spans = Array.AsReadOnly(spans);
        }

        public TrackPiece Piece { get { return Track.Pieces[0]; } }
        public int SampleCount { get { return samples.Length; } }
        public float SampleReferenceS(int i) { return referenceS[i]; }
        public float SampleOffset(int i) { return offsets[i]; }

        public int PointCount(int span) { return samples.Length; }
        public Vector3 PointPosition(int span, int point) { return samples[point].Position; }
        public Vector3 PointTangent(int span, int point) { return samples[point].Tangent; }
        public float PointDistance(int span, int point) { return samples[point].SMeters; }

        /// <summary>Distance de chemin de l'echantillon d'abscisse de reference la plus proche par dessous.</summary>
        public float DistanceAtReference(float s)
        {
            int i = IndexAtReference(s);
            if (i >= referenceS.Length - 1) return samples[samples.Length - 1].SMeters;
            float t = (s - referenceS[i]) / Math.Max(1e-6f, referenceS[i + 1] - referenceS[i]);
            return samples[i].SMeters + Mathf.Clamp01(t) * (samples[i + 1].SMeters - samples[i].SMeters);
        }

        /// <summary>Abscisse de reference a une distance de chemin.</summary>
        public float ReferenceAt(float distance)
        {
            int low = 0, high = samples.Length - 1;
            if (distance <= samples[0].SMeters) return referenceS[0];
            if (distance >= samples[high].SMeters) return referenceS[high];
            while (high - low > 1)
            {
                int mid = (low + high) / 2;
                if (samples[mid].SMeters <= distance) low = mid; else high = mid;
            }
            float t = (distance - samples[low].SMeters) / Math.Max(1e-6f, samples[high].SMeters - samples[low].SMeters);
            return referenceS[low] + t * (referenceS[high] - referenceS[low]);
        }

        /// <summary>Decalage o(s) a une distance de chemin (interpole entre echantillons).</summary>
        public float OffsetAtDistance(float distance)
        {
            int low = 0, high = samples.Length - 1;
            if (distance <= samples[0].SMeters) return offsets[0];
            if (distance >= samples[high].SMeters) return offsets[high];
            while (high - low > 1)
            {
                int mid = (low + high) / 2;
                if (samples[mid].SMeters <= distance) low = mid; else high = mid;
            }
            float t = (distance - samples[low].SMeters) / Math.Max(1e-6f, samples[high].SMeters - samples[low].SMeters);
            return offsets[low] + t * (offsets[high] - offsets[low]);
        }

        private int IndexAtReference(float s)
        {
            int low = 0, high = referenceS.Length - 1;
            if (s <= referenceS[0]) return 0;
            if (s >= referenceS[high]) return high;
            while (high - low > 1)
            {
                int mid = (low + high) / 2;
                if (referenceS[mid] <= s) low = mid; else high = mid;
            }
            return low;
        }

        /// <summary>
        /// Construit la reference : depart de <paramref name="startOffset"/> a <paramref name="targetOffset"/> sur
        /// <paramref name="departLength"/>, maintien sur <paramref name="holdLength"/>, retour a 0 sur <paramref name="returnLength"/>,
        /// queue a 0 sur <paramref name="tailLength"/>. Un depart de longueur nulle exige startOffset = targetOffset.
        /// </summary>
        /// <param name="initialOffsetRadians">Ecart cinematique e de la caisse au debut du chemin (pose nominale, contrat §8).</param>
        public static ManeuverPath Build(RoadId corridorId, RoadCurve reference, float startS, float startOffset, float targetOffset,
            float departLength, float holdLength, float returnLength, float tailLength, float speed, float stepMeters,
            float referenceAheadRearAxleMeters, float initialOffsetRadians)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            if (!Finite(startS) || !Finite(startOffset) || !Finite(targetOffset) || !NonNegative(departLength) || !NonNegative(holdLength)
                || !NonNegative(returnLength) || !NonNegative(tailLength) || !(stepMeters > 0f) || !Finite(stepMeters)
                || (departLength == 0f && startOffset != targetOffset) || (returnLength == 0f && targetOffset != 0f)
                || !(speed > 0f) || !Finite(speed))
                throw new ArgumentException("InvalidManeuverPath");
            float s0 = startS, s1 = s0 + departLength, s2 = s1 + holdLength, s3 = s2 + returnLength, s4 = s3 + tailLength;
            if (!(s4 > s0)) throw new ArgumentException("InvalidManeuverPath");
            var refS = new List<float>();
            var offs = new List<float>();
            var points = new List<RoadCurveSample>();
            int count = Math.Max(2, (int)Math.Ceiling((s4 - s0) / stepMeters) + 1);
            double travelled = 0d;
            Vector3 previous = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                float s = i == count - 1 ? s4 : s0 + (s4 - s0) * i / (count - 1);
                float o, d1;
                Offset(s, s0, s1, s2, s3, startOffset, targetOffset, out o, out d1);
                var c = reference.Sample(s);
                Vector3 position = c.Position + c.Right * o;
                double g = 1d - c.CurvaturePerMeter * o;
                Vector3 tangent = ((float)g * c.Tangent + d1 * c.Right).normalized;
                // kappa_chemin = (kappa + phi') / |dp/ds|, phi = atan2(o', 1 - kappa o) : difference centree sur o analytique.
                const float h = 0.01f;
                double phiPlus = Phi(reference, Math.Min(s + h, s4), s0, s1, s2, s3, startOffset, targetOffset);
                double phiMinus = Phi(reference, Math.Max(s - h, s0), s0, s1, s2, s3, startOffset, targetOffset);
                double phiRate = (phiPlus - phiMinus) / (Math.Min(s + h, s4) - Math.Max(s - h, s0));
                double norm = Math.Sqrt(g * g + d1 * (double)d1);
                float kappa = (float)((c.CurvaturePerMeter + phiRate) / Math.Max(1e-6, norm));
                if (i > 0) travelled += Vector3.Distance(previous, position);
                previous = position;
                refS.Add(s); offs.Add(o);
                points.Add(new RoadCurveSample { SMeters = (float)travelled, Position = position, Tangent = tangent, Up = c.Up,
                    CurvaturePerMeter = kappa });
            }
            return new ManeuverPath(corridorId, reference, s0, s1, s2, s3, s4, startOffset, targetOffset, speed, refS, offs, points,
                referenceAheadRearAxleMeters, initialOffsetRadians);
        }

        private static double Phi(RoadCurve reference, float s, float s0, float s1, float s2, float s3, float oStart, float oTarget)
        {
            float o, d1;
            Offset(s, s0, s1, s2, s3, oStart, oTarget, out o, out d1);
            return Math.Atan2(d1, 1d - reference.Sample(s).CurvaturePerMeter * o);
        }

        /// <summary>o(s) et o'(s) par morceaux, q(u) = 6u^5 - 15u^4 + 10u^3 (C2).</summary>
        private static void Offset(float s, float s0, float s1, float s2, float s3, float oStart, float oTarget, out float o, out float d1)
        {
            if (s1 > s0 && s < s1)
            {
                float length = s1 - s0, u = Mathf.Clamp01((s - s0) / length);
                o = oStart + (oTarget - oStart) * Q(u);
                d1 = (oTarget - oStart) * Q1(u) / length;
                return;
            }
            if (s < s2) { o = oTarget; d1 = 0f; return; }
            if (s < s3)
            {
                float length = s3 - s2, u = Mathf.Clamp01((s - s2) / length);
                o = oTarget * (1f - Q(u));
                d1 = -oTarget * Q1(u) / length;
                return;
            }
            o = 0f; d1 = 0f;
        }

        private static float Q(float u) { return u * u * u * (10f + u * (-15f + 6f * u)); }
        private static float Q1(float u) { return 30f * u * u * (1f + u * (-2f + u)); }

        /// <summary>Longueur de transition L_t pour |delta o| sous une courbure admise : |o''| max = 5,774 |delta o| / L_t^2.</summary>
        public static float TransitionLength(float deltaOffsetMeters, float curvaturePerMeter)
        {
            if (!(curvaturePerMeter > 0f)) return float.PositiveInfinity;
            return (float)Math.Sqrt(TransitionCurvatureFactor * Math.Abs(deltaOffsetMeters) / curvaturePerMeter);
        }

        /// <summary>Courbure admise a une vitesse : min(a_lat / v^2, tan(braquage(v)) / L, admission).</summary>
        public static float AllowedCurvature(DrivabilityProfile profile, float speed, float lateralAcceleration)
        {
            float v = Math.Max(0.1f, speed);
            float lateral = lateralAcceleration / (v * v);
            float lockDegrees = profile.FullReductionSpeedMetersPerSecond > 0f
                ? Mathf.Lerp(profile.LowSpeedLockDegrees, profile.HighSpeedLockDegrees, v / profile.FullReductionSpeedMetersPerSecond)
                : profile.LowSpeedLockDegrees;
            float steering = profile.WheelbaseMeters > 0f ? Mathf.Tan(lockDegrees * Mathf.Deg2Rad) / profile.WheelbaseMeters : 0f;
            float admission = 1f / RoadModelCompiler.AdmissionRadiusMeters(profile);
            return Math.Min(lateral, Math.Min(steering, admission));
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static bool NonNegative(float value) { return Finite(value) && value >= 0f; }
    }

    /// <summary>Resultat de la preuve M2 : cause, distance de chemin et region occupee sur l'autre corridor.</summary>
    public readonly struct ManeuverProofResult
    {
        public readonly ManeuverGeometryCause Cause;
        public readonly float AtDistanceMeters;
        /// <summary>Le gabarit gonfle entre dans l'enveloppe de l'autre corridor.</summary>
        public readonly bool EntersOther;
        /// <summary>Region [min, max] en abscisse de l'autre corridor ; NaN sans entree.</summary>
        public readonly float OtherSMinMeters;
        public readonly float OtherSMaxMeters;
        /// <summary>Derniere distance de chemin ou le gabarit gonfle occupe encore l'autre enveloppe ; -1 sans entree.</summary>
        public readonly float LastOtherDistanceMeters;

        public ManeuverProofResult(ManeuverGeometryCause cause, float at, bool entersOther, float otherMin, float otherMax, float lastOther)
        {
            Cause = cause; AtDistanceMeters = at; EntersOther = entersOther; OtherSMinMeters = otherMin; OtherSMaxMeters = otherMax;
            LastOtherDistanceMeters = lastOther;
        }

        public bool Proven { get { return Cause == ManeuverGeometryCause.None; } }
    }

    /// <summary>
    /// Story 5.42, M2 : preuve continue d'une reference de manoeuvre. A chaque echantillon, les quatre coins plans du gabarit
    /// maximal a la pose nominale cinematique, gonfles de epsilon_t et du reste entre echantillons (delta/2)(1 + rho psi'_max),
    /// restent dans l'union contigue de l'enveloppe revue du corridor propre et, si elle est fournie, de celle de l'autre
    /// corridor de la meme section, loin des bouts (aucun mouvement de carrefour) ; la pose nominale reste braquable a v_m ;
    /// le gabarit gonfle evite l'obstacle aux temps predits. Echec : cause nommee, fail-closed.
    /// </summary>
    public static class ManeuverProof
    {
        public static ManeuverProofResult Prove(ManeuverPath path, EffectiveLaneCorridor own, EffectiveLaneCorridor? other,
            DrivabilityProfile profile, GaugeBox gauge, float epsilonMeters, ManeuverObstacle obstacle, ManeuverTiming timing,
            float longitudinalClearance, float lateralClearance, float envelopeTolerance)
        {
            if (path == null || own.Curve == null || !(epsilonMeters >= 0f) || (other.HasValue && other.Value.Curve == null))
                return Fail(ManeuverGeometryCause.InvalidInput, 0f);
            var track = path.Track;
            var ownCurve = own.Curve;
            RoadCurve otherCurve = other.HasValue ? other.Value.Curve : null;
            // Cote de l'autre corridor dans le repere propre : signe du lateral de sa reference.
            int side = 0;
            if (otherCurve != null)
            {
                var probe = ownCurve.Project(otherCurve.Sample(otherCurve.StartS + otherCurve.Length * 0.5f).Position);
                side = probe.LateralOffsetMeters < 0f ? -1 : 1;
            }
            float admission = 1f / RoadModelCompiler.AdmissionRadiusMeters(profile);
            float rho = gauge.Rho;
            float otherMin = float.PositiveInfinity, otherMax = float.NegativeInfinity, lastOther = -1f;
            bool entered = false;
            int n = path.SampleCount;
            var corners = new Vector3[4];
            for (int i = 0; i < n; i++)
            {
                float d = path.Samples[i].SMeters;
                float before = i > 0 ? path.Samples[i - 1].SMeters : d, after = i < n - 1 ? path.Samples[i + 1].SMeters : d;
                float delta = Math.Max(d - before, after - d);
                float remainder = 0.5f * delta * (1f + rho * track.BodyRateMax(0, before, after));
                float r = epsilonMeters + remainder;

                if (Math.Abs(path.Samples[i].CurvaturePerMeter) > admission
                    || PathHorizon.NominalSteeringCeilingMetersPerSecond(profile, track.OffsetRadians(0, d)) < path.SpeedMetersPerSecond)
                    return Fail(ManeuverGeometryCause.SteeringInfeasible, d);
                if (Math.Abs(path.Samples[i].CurvaturePerMeter) * path.SpeedMetersPerSecond * path.SpeedMetersPerSecond
                    > TrafficV2Settings.ManeuverLateralAccelerationMetersPerSecondSquared)
                    return Fail(ManeuverGeometryCause.LateralAccelerationExceeded, d);

                var pose = track.Nominal(0, d);
                Vector3 up = pose.Up.normalized;
                Vector3 forward = Vector3.ProjectOnPlane(pose.Forward, up).normalized;
                Vector3 right = Vector3.Cross(up, forward);
                for (int k = 0; k < 4; k++)
                    corners[k] = pose.Position + right * ((k & 1) == 0 ? -gauge.HalfWidthMeters : gauge.HalfWidthMeters)
                        + forward * ((k & 2) == 0 ? -gauge.LengthMeters * 0.5f : gauge.LengthMeters * 0.5f);

                float boxSMin = float.PositiveInfinity, boxSMax = float.NegativeInfinity;
                float boxLatMin = float.PositiveInfinity, boxLatMax = float.NegativeInfinity;
                for (int k = 0; k < 4; k++)
                {
                    var projection = ownCurve.Project(corners[k]);
                    if (projection.LongitudinalOverrunMeters > 0f || projection.SMeters < ownCurve.StartS + r
                        || projection.SMeters > ownCurve.Length - r)
                        return Fail(ManeuverGeometryCause.CorridorEnd, d);
                    var point = projection.Point;
                    float lateral = projection.LateralOffsetMeters;
                    float lower = -point.HalfWidthLeftMeters, upper = point.HalfWidthRightMeters;
                    bool beyond = side < 0 ? lateral - r < lower : side > 0 && lateral + r > upper;
                    if (beyond)
                    {
                        // Union contigue : l'autre enveloppe jouxte le bord propre a cette abscisse, elle l'etend de sa largeur.
                        Vector3 edge = point.Position + point.Right * (side < 0 ? lower : upper);
                        var shared = otherCurve.Project(edge);
                        if (shared.LongitudinalOverrunMeters > 0f) return Fail(ManeuverGeometryCause.CorridorEnd, d);
                        var otherPoint = shared.Point;
                        float lat = shared.LateralOffsetMeters;
                        bool contiguous = Math.Min(Math.Abs(lat + otherPoint.HalfWidthLeftMeters),
                            Math.Abs(lat - otherPoint.HalfWidthRightMeters)) <= envelopeTolerance;
                        if (!contiguous) return Fail(ManeuverGeometryCause.EnvelopeNotContiguous, d);
                        float width = otherPoint.HalfWidthLeftMeters + otherPoint.HalfWidthRightMeters;
                        if (side < 0) lower -= width; else upper += width;
                        var inOther = otherCurve.Project(corners[k]);
                        if (inOther.LongitudinalOverrunMeters > 0f || inOther.SMeters < otherCurve.StartS + r
                            || inOther.SMeters > otherCurve.Length - r)
                            return Fail(ManeuverGeometryCause.CorridorEnd, d);
                        entered = true;
                        lastOther = d;
                        otherMin = Math.Min(otherMin, inOther.SMeters - r);
                        otherMax = Math.Max(otherMax, inOther.SMeters + r);
                    }
                    if (lateral < lower + r || lateral > upper - r) return Fail(ManeuverGeometryCause.EnvelopeExceeded, d);
                    boxSMin = Math.Min(boxSMin, projection.SMeters); boxSMax = Math.Max(boxSMax, projection.SMeters);
                    boxLatMin = Math.Min(boxLatMin, lateral); boxLatMax = Math.Max(boxLatMax, lateral);
                }

                if (!obstacle.Id.IsEmpty)
                {
                    if (obstacle.HasWorldFootprint)
                    {
                        Vector3 causeMin, causeMax;
                        // Le reste couvre aussi le mouvement de la cause entre les echantillons de preuve.
                        obstacle.WorldBounds(timing.SecondsAt(before), timing.SecondsAt(after), out causeMin, out causeMax);
                        Vector3 bodyMin = corners[0], bodyMax = corners[0];
                        for (int k = 1; k < 4; k++)
                        { bodyMin = Vector3.Min(bodyMin, corners[k]); bodyMax = Vector3.Max(bodyMax, corners[k]); }
                        Vector3 inflation = new Vector3(Math.Abs(forward.x) * longitudinalClearance + Math.Abs(right.x) * lateralClearance + r,
                            Math.Abs(forward.y) * longitudinalClearance + Math.Abs(right.y) * lateralClearance + r,
                            Math.Abs(forward.z) * longitudinalClearance + Math.Abs(right.z) * lateralClearance + r);
                        // Preuve plane, comme l'enveloppe routiere : aucune hauteur ne rend la cause traversable.
                        if (bodyMax.x + inflation.x > causeMin.x && bodyMin.x - inflation.x < causeMax.x
                            && bodyMax.z + inflation.z > causeMin.z && bodyMin.z - inflation.z < causeMax.z)
                            return Fail(ManeuverGeometryCause.ObstacleClearance, d);
                        continue;
                    }
                    float moved = obstacle.SpeedMetersPerSecond * timing.SecondsAt(d);
                    bool overlapsS = boxSMax + r > obstacle.NearSMeters + moved - longitudinalClearance
                        && boxSMin - r < obstacle.FarSMeters + moved + longitudinalClearance;
                    bool overlapsLateral = boxLatMax + r > obstacle.LateralMinMeters - lateralClearance
                        && boxLatMin - r < obstacle.LateralMaxMeters + lateralClearance;
                    if (overlapsS && overlapsLateral) return Fail(ManeuverGeometryCause.ObstacleClearance, d);
                }
            }
            return new ManeuverProofResult(ManeuverGeometryCause.None, 0f, entered, entered ? otherMin : float.NaN,
                entered ? otherMax : float.NaN, lastOther);
        }

        private static ManeuverProofResult Fail(ManeuverGeometryCause cause, float at)
        {
            return new ManeuverProofResult(cause, at, false, float.NaN, float.NaN, -1f);
        }
    }
}
