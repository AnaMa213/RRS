using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Repere de route a une abscisse (AD-45) : <c>forward = Tangent</c>, <c>up = Up</c> (road-up,
    /// jamais le haut monde), <c>right = normalize(cross(up, forward))</c>. Lateral positif a droite,
    /// normal positif selon road-up ; demi-largeur gauche pour les lateraux negatifs, droite pour
    /// les positifs.
    /// </summary>
    public struct RoadCurvePoint
    {
        public float SMeters;
        public Vector3 Position;
        public Vector3 Tangent;
        public Vector3 Up;
        public Vector3 Right;

        /// <summary>Positive quand dTangent/ds pointe vers la droite, nulle en ligne droite.</summary>
        public float CurvaturePerMeter;

        public float HalfWidthLeftMeters;
        public float HalfWidthRightMeters;

        /// <summary>
        /// Cap signe, en degres dans [-180, 180] : angle autour de road-up de la tangente vers
        /// <paramref name="forward"/>, positif vers la droite.
        /// </summary>
        public float SignedHeadingDegrees(Vector3 forward)
        {
            Vector3 planar = forward - Up * Vector3.Dot(forward, Up);
            return Mathf.Atan2(Vector3.Dot(planar, Right), Vector3.Dot(planar, Tangent)) * Mathf.Rad2Deg;
        }
    }

    /// <summary>Resultat de <see cref="RoadCurve.Project(Vector3)"/>.</summary>
    public struct RoadProjection
    {
        /// <summary>Abscisse bornee au domaine interroge, jamais extrapolee.</summary>
        public float SMeters;

        public float LateralOffsetMeters;
        public float NormalOffsetMeters;

        /// <summary>Distance 3D du point au point de courbe a <see cref="SMeters"/>.</summary>
        public float DistanceMeters;

        /// <summary>
        /// Part du point situee au-dela d'une borne du domaine interroge, le long de la tangente
        /// (0 a l'interieur). C'est ce qu'une abscisse bornee ne dit pas.
        /// </summary>
        public float LongitudinalOverrunMeters;

        /// <summary>Repere complet de la courbe a <see cref="SMeters"/>.</summary>
        public RoadCurvePoint Point;
    }

    /// <summary>
    /// Courbe dirigee immuable sur echantillons compiles (AD-45), commune aux corridors et aux
    /// mouvements. Entre deux echantillons : position, abscisse, courbure et largeurs interpolees
    /// lineairement, tangente et road-up interpoles puis reorthonormalises. Fonction pure, sans
    /// dependance de package : aucune reference aux splines Unity.
    /// </summary>
    public sealed class RoadCurve
    {
        /// <summary>Tolerance numerique de detection d'une borne de domaine, en metres.</summary>
        private const float BoundEpsilonMeters = 1e-4f;

        private readonly RoadCurveSample[] _samples;
        private readonly UnityEngine.Bounds _fullBounds;

        /// <exception cref="ArgumentException">Moins de deux echantillons, ou abscisses non strictement croissantes.</exception>
        public RoadCurve(IReadOnlyList<RoadCurveSample> samples)
        {
            if (samples == null || samples.Count < 2)
            {
                throw new ArgumentException("RoadCurve exige au moins deux echantillons.", "samples");
            }

            _samples = new RoadCurveSample[samples.Count];
            for (int i = 0; i < _samples.Length; i++)
            {
                _samples[i] = samples[i];
                if (i > 0 && !(_samples[i].SMeters > _samples[i - 1].SMeters))
                {
                    throw new ArgumentException("RoadCurve exige des abscisses strictement croissantes (index " + i + ").", "samples");
                }
            }

            _fullBounds = Bounds(StartS, Length);
        }

        /// <summary>Abscisse du premier echantillon (0 pour un modele valide).</summary>
        public float StartS
        {
            get { return _samples[0].SMeters; }
        }

        /// <summary>Longueur en metres : abscisse du dernier echantillon.</summary>
        public float Length
        {
            get { return _samples[_samples.Length - 1].SMeters; }
        }

        /// <summary>Bornes 3D de toute l'enveloppe, calculees une fois.</summary>
        public UnityEngine.Bounds FullBounds
        {
            get { return _fullBounds; }
        }

        /// <summary>Repere complet a <paramref name="s"/>, borne au domaine.</summary>
        public RoadCurvePoint Sample(float s)
        {
            int index;
            float t;
            Locate(s, out index, out t);
            return Interpolate(index, t);
        }

        /// <summary>Projection 3D sur tout le domaine.</summary>
        public RoadProjection Project(Vector3 point)
        {
            return Project(point, StartS, Length);
        }

        /// <summary>
        /// Projection 3D au plus proche point, restreinte a <c>[sMin, sMax]</c> (borne au domaine).
        /// Jamais d'extrapolation ; egalite de distance departagee par la plus petite abscisse.
        /// </summary>
        public RoadProjection Project(Vector3 point, float sMin, float sMax)
        {
            sMin = Mathf.Clamp(sMin, StartS, Length);
            sMax = Mathf.Clamp(sMax, StartS, Length);
            if (sMax < sMin)
            {
                sMax = sMin;
            }

            float bestDistanceSquared = float.PositiveInfinity;
            float bestS = sMin;
            for (int i = 0; i < _samples.Length - 1; i++)
            {
                float s0 = _samples[i].SMeters;
                float s1 = _samples[i + 1].SMeters;
                if (s1 < sMin || s0 > sMax)
                {
                    continue;
                }

                float span = s1 - s0;
                float tMin = Mathf.Max(0f, (sMin - s0) / span);
                float tMax = Mathf.Min(1f, (sMax - s0) / span);
                Vector3 a = Vector3.LerpUnclamped(_samples[i].Position, _samples[i + 1].Position, tMin);
                Vector3 b = Vector3.LerpUnclamped(_samples[i].Position, _samples[i + 1].Position, tMax);
                Vector3 ab = b - a;
                float lengthSquared = ab.sqrMagnitude;
                float u = lengthSquared > 1e-12f ? Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSquared) : 0f;

                float distanceSquared = (point - (a + ab * u)).sqrMagnitude;

                // Strictement inferieur : parcours par abscisse croissante, donc la premiere egalite
                // rencontree -- la plus petite abscisse -- est conservee.
                if (distanceSquared < bestDistanceSquared)
                {
                    bestDistanceSquared = distanceSquared;
                    bestS = s0 + (tMin + (tMax - tMin) * u) * span;
                }
            }

            bestS = Mathf.Clamp(bestS, sMin, sMax);
            var frame = Sample(bestS);
            Vector3 offset = point - frame.Position;
            float along = Vector3.Dot(offset, frame.Tangent);

            float overrun = 0f;
            if (along < 0f && bestS <= sMin + BoundEpsilonMeters)
            {
                overrun = -along;
            }
            else if (along > 0f && bestS >= sMax - BoundEpsilonMeters)
            {
                overrun = along;
            }

            var projection = new RoadProjection();
            projection.SMeters = bestS;
            projection.LateralOffsetMeters = Vector3.Dot(offset, frame.Right);
            projection.NormalOffsetMeters = Vector3.Dot(offset, frame.Up);
            projection.DistanceMeters = offset.magnitude;
            projection.LongitudinalOverrunMeters = overrun;
            projection.Point = frame;
            return projection;
        }

        /// <summary>
        /// Bornes 3D de l'intervalle <c>[s0, s1]</c> (borne au domaine, ordre indifferent) contenant
        /// toute l'enveloppe de largeur, pas seulement la ligne centrale.
        /// </summary>
        public UnityEngine.Bounds Bounds(float s0, float s1)
        {
            if (s1 < s0)
            {
                float swap = s0;
                s0 = s1;
                s1 = swap;
            }

            s0 = Mathf.Clamp(s0, StartS, Length);
            s1 = Mathf.Clamp(s1, StartS, Length);

            var points = new List<RoadCurvePoint>();
            points.Add(Sample(s0));
            for (int i = 0; i < _samples.Length; i++)
            {
                if (_samples[i].SMeters > s0 && _samples[i].SMeters < s1)
                {
                    points.Add(Interpolate(Math.Min(i, _samples.Length - 2), i == _samples.Length - 1 ? 1f : 0f));
                }
            }

            points.Add(Sample(s1));

            var bounds = new UnityEngine.Bounds(points[0].Position, Vector3.zero);
            float pad = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                bounds.Encapsulate(p.Position);
                bounds.Encapsulate(p.Position - p.Right * p.HalfWidthLeftMeters);
                bounds.Encapsulate(p.Position + p.Right * p.HalfWidthRightMeters);

                if (i > 0)
                {
                    // Entre deux points, le bord d'enveloppe suit une droite tournante, pas la corde
                    // de ses extremites : on borne l'ecart par la rotation du vecteur droite et la
                    // variation de largeur, de facon conservatrice.
                    var q = points[i - 1];
                    float maxWidth = Mathf.Max(Mathf.Max(p.HalfWidthLeftMeters, p.HalfWidthRightMeters), Mathf.Max(q.HalfWidthLeftMeters, q.HalfWidthRightMeters));
                    float widthDelta = Mathf.Max(
                        Mathf.Abs(p.HalfWidthLeftMeters - q.HalfWidthLeftMeters),
                        Mathf.Abs(p.HalfWidthRightMeters - q.HalfWidthRightMeters));
                    float rightTurn = (p.Right - q.Right).magnitude;
                    float segmentPad = maxWidth * (1f - Vector3.Dot(p.Right, q.Right)) + 0.25f * widthDelta * rightTurn;
                    pad = Mathf.Max(pad, segmentPad);
                }
            }

            bounds.Expand(2f * pad);
            return bounds;
        }

        /// <summary>Pas RK4 maximal du transport de l'ecart nominal, en metres.</summary>
        public const float KinematicOffsetStepMeters = 0.1f;

        /// <summary>
        /// Ecart nominal cinematique e (contrat §8, radians) transporte de <paramref name="s0"/> a
        /// <paramref name="s1"/> (bornes au domaine ; e inchange si s1 &lt;= s0 ou sans point de reference) :
        /// de/ds = kappa(s) - sin(e)/a, kappa lineaire entre echantillons comme <see cref="Sample"/>, RK4 par
        /// segment d'echantillons a pas &lt;= <see cref="KinematicOffsetStepMeters"/>. Le raccord a un autre
        /// element (saut de tangente) appartient a l'appelant : <see cref="SignedTangentJumpRadians"/>.
        /// </summary>
        public double AdvanceKinematicOffset(float s0, float s1, double e, float a)
        {
            return AdvanceKinematicOffset(s0, s1, e, a, null);
        }

        /// <summary>
        /// Meme transport, qui publie chaque noeud d'integration (s, e) apres son pas : les bornes de preuve
        /// Gate A (Story 5.52) encadrent ainsi la solution entre noeuds sans refaire une autre integration.
        /// </summary>
        public double AdvanceKinematicOffset(float s0, float s1, double e, float a, Action<double, double> node)
        {
            s0 = Mathf.Clamp(s0, StartS, Length);
            s1 = Mathf.Clamp(s1, StartS, Length);
            if (!(a > 0f) || !(s1 > s0)) return e;
            // Segments couverts seulement : un mouvement porte ~230 echantillons et l'horizon l'appelle point par point.
            int first;
            float unused;
            Locate(s0, out first, out unused);
            for (int i = first; i + 1 < _samples.Length && _samples[i].SMeters < s1; i++)
            {
                double x0 = Math.Max(s0, _samples[i].SMeters), x1 = Math.Min(s1, _samples[i + 1].SMeters);
                if (!(x1 > x0)) continue;
                double origin = _samples[i].SMeters;
                double k0 = _samples[i].CurvaturePerMeter;
                double slope = (_samples[i + 1].CurvaturePerMeter - k0) / (_samples[i + 1].SMeters - origin);
                int steps = Math.Max(1, (int)Math.Ceiling((x1 - x0) / KinematicOffsetStepMeters));
                double h = (x1 - x0) / steps;
                for (int n = 0; n < steps; n++)
                {
                    double x = x0 + h * n - origin;
                    double r1 = k0 + slope * x - Math.Sin(e) / a;
                    double r2 = k0 + slope * (x + 0.5 * h) - Math.Sin(e + 0.5 * h * r1) / a;
                    double r3 = k0 + slope * (x + 0.5 * h) - Math.Sin(e + 0.5 * h * r2) / a;
                    double r4 = k0 + slope * (x + h) - Math.Sin(e + h * r3) / a;
                    e += h / 6.0 * (r1 + 2.0 * r2 + 2.0 * r3 + r4);
                    if (node != null) node(x0 + h * (n + 1), e);
                }
            }
            return e;
        }

        /// <summary>
        /// Saut de tangente signe (radians, positif vers road-right) de <paramref name="left"/> vers
        /// <paramref name="right"/> autour du road-up de droite : e en saute a un raccord, le cap de caisse
        /// nominal restant continu (contrat §8).
        /// </summary>
        public static double SignedTangentJumpRadians(RoadCurvePoint left, RoadCurvePoint right)
        {
            Vector3 up = right.Up.normalized;
            Vector3 from = Vector3.ProjectOnPlane(left.Tangent, up);
            Vector3 to = Vector3.ProjectOnPlane(right.Tangent, up);
            if (from.sqrMagnitude <= 0f || to.sqrMagnitude <= 0f) return 0d;
            return Vector3.SignedAngle(from, to, up) * Mathf.Deg2Rad;
        }

        // ------------------------------------------------------------------ interne

        private void Locate(float s, out int index, out float t)
        {
            s = Mathf.Clamp(s, StartS, Length);

            int low = 0;
            int high = _samples.Length - 2;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (_samples[mid].SMeters <= s)
                {
                    low = mid;
                }
                else
                {
                    high = mid - 1;
                }
            }

            index = low;
            float span = _samples[index + 1].SMeters - _samples[index].SMeters;
            t = Mathf.Clamp01((s - _samples[index].SMeters) / span);
        }

        private RoadCurvePoint Interpolate(int index, float t)
        {
            var a = _samples[index];
            var b = _samples[index + 1];

            Vector3 tangent = a.Tangent * (1f - t) + b.Tangent * t;
            if (tangent.sqrMagnitude < 1e-12f)
            {
                // Tangentes opposees : la corde reste la seule direction definie.
                tangent = b.Position - a.Position;
            }

            tangent.Normalize();

            Vector3 up = a.Up * (1f - t) + b.Up * t;
            up = (up - tangent * Vector3.Dot(up, tangent)).normalized;

            var point = new RoadCurvePoint();
            point.SMeters = Mathf.LerpUnclamped(a.SMeters, b.SMeters, t);
            point.Position = Vector3.LerpUnclamped(a.Position, b.Position, t);
            point.Tangent = tangent;
            point.Up = up;
            point.Right = Vector3.Cross(up, tangent).normalized;
            point.CurvaturePerMeter = Mathf.LerpUnclamped(a.CurvaturePerMeter, b.CurvaturePerMeter, t);
            point.HalfWidthLeftMeters = Mathf.LerpUnclamped(a.HalfWidthLeftMeters, b.HalfWidthLeftMeters, t);
            point.HalfWidthRightMeters = Mathf.LerpUnclamped(a.HalfWidthRightMeters, b.HalfWidthRightMeters, t);
            return point;
        }
    }
}
