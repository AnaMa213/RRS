using System;
using System.Collections.Generic;
using UnityEngine;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;

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

        // Caches exacts (Story 5.33, D14) calcules une fois a la construction, par les memes expressions que les appels
        // qu'ils remplacent : repere de chaque echantillon (Sample(s_i)), courbure absolue maximale, extremites et boites
        // des segments d'une projection sur tout le domaine, par blocs et super-blocs.
        private const int ProjectionBlockSegments = 8;
        private const int ProjectionSuperBlockSegments = 64;
        /// <summary>Marge d'elagage (m) : tres au-dessus de l'arrondi des distances, tres en dessous de l'espacement des segments.</summary>
        private const float ProjectionPruneMarginMeters = 1e-3f;
        private readonly RoadCurvePoint[] _sampleFrames;
        private readonly float _maximumAbsoluteCurvature;
        private readonly float _maximumChordTangentAngle;
        private readonly Vector3[] _segmentA, _segmentB, _segmentMin, _segmentMax, _blockMin, _blockMax, _superMin, _superMax;
        private readonly float[] _segmentTMin, _segmentTMax;

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

            _sampleFrames = new RoadCurvePoint[_samples.Length];
            for (int i = 0; i < _samples.Length; i++)
            {
                _sampleFrames[i] = Sample(_samples[i].SMeters);
                _maximumAbsoluteCurvature = Mathf.Max(_maximumAbsoluteCurvature, Mathf.Abs(_samples[i].CurvaturePerMeter));
            }
            for (int i = 0; i + 1 < _samples.Length; i++)
            {
                Vector3 chord = _samples[i + 1].Position - _samples[i].Position;
                float length = chord.magnitude;
                if (!(length > 1e-6f)) { _maximumChordTangentAngle = Mathf.PI; break; }
                chord /= length;
                float a0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(chord, _sampleFrames[i].Tangent), -1f, 1f));
                float a1 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(chord, _sampleFrames[i + 1].Tangent), -1f, 1f));
                _maximumChordTangentAngle = Mathf.Max(_maximumChordTangentAngle, Mathf.Max(a0, a1));
            }

            int segments = _samples.Length - 1;
            _segmentA = new Vector3[segments]; _segmentB = new Vector3[segments];
            _segmentMin = new Vector3[segments]; _segmentMax = new Vector3[segments];
            _segmentTMin = new float[segments]; _segmentTMax = new float[segments];
            float sMin = Mathf.Clamp(StartS, StartS, Length), sMax = Mathf.Clamp(Length, StartS, Length);
            for (int i = 0; i < segments; i++)
            {
                // Memes expressions que Project(point, StartS, Length) pour le segment i.
                float s0 = _samples[i].SMeters, span = _samples[i + 1].SMeters - s0;
                float tMin = Mathf.Max(0f, (sMin - s0) / span);
                float tMax = Mathf.Min(1f, (sMax - s0) / span);
                _segmentTMin[i] = tMin; _segmentTMax[i] = tMax;
                _segmentA[i] = Vector3.LerpUnclamped(_samples[i].Position, _samples[i + 1].Position, tMin);
                _segmentB[i] = Vector3.LerpUnclamped(_samples[i].Position, _samples[i + 1].Position, tMax);
                _segmentMin[i] = Vector3.Min(_segmentA[i], _segmentB[i]);
                _segmentMax[i] = Vector3.Max(_segmentA[i], _segmentB[i]);
            }
            Group(_segmentMin, _segmentMax, ProjectionBlockSegments, out _blockMin, out _blockMax);
            Group(_segmentMin, _segmentMax, ProjectionSuperBlockSegments, out _superMin, out _superMax);
        }

        private static void Group(Vector3[] min, Vector3[] max, int size, out Vector3[] groupMin, out Vector3[] groupMax)
        {
            int groups = (min.Length + size - 1) / size;
            groupMin = new Vector3[groups];
            groupMax = new Vector3[groups];
            for (int g = 0; g < groups; g++)
            {
                groupMin[g] = min[g * size];
                groupMax[g] = max[g * size];
                for (int i = g * size + 1; i < Math.Min(min.Length, (g + 1) * size); i++)
                {
                    groupMin[g] = Vector3.Min(groupMin[g], min[i]);
                    groupMax[g] = Vector3.Max(groupMax[g], max[i]);
                }
            }
        }

        /// <summary>Nombre d'echantillons compiles.</summary>
        public int SampleCount
        {
            get { return _samples.Length; }
        }

        /// <summary>Abscisse de l'echantillon <paramref name="index"/>.</summary>
        public float SampleS(int index)
        {
            return _samples[index].SMeters;
        }

        /// <summary>Repere a l'echantillon <paramref name="index"/> : exactement <c>Sample(SampleS(index))</c>, precalcule.</summary>
        public RoadCurvePoint SampleFrame(int index)
        {
            return _sampleFrames[index];
        }

        /// <summary>Indice du premier echantillon d'abscisse strictement superieure a <paramref name="s"/> (SampleCount si aucun).</summary>
        public int FirstSampleAbove(float s)
        {
            int low = 0, high = _samples.Length;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (_samples[mid].SMeters > s) high = mid; else low = mid + 1;
            }
            return low;
        }

        /// <summary>Plus grande courbure absolue des echantillons (meme maximum que leur parcours dans l'ordre), precalculee.</summary>
        public float MaximumAbsoluteCurvaturePerMeter
        {
            get { return _maximumAbsoluteCurvature; }
        }

        /// <summary>
        /// Plus grand angle (radians) entre la corde d'un segment et la tangente de l'un de ses deux echantillons ; pi si un
        /// segment est degenere. Toute tangente interpolee du segment reste dans ce cone autour de la corde (normalisation
        /// d'une interpolation de deux vecteurs du cone, cone convexe sous 90 degres). Story 5.33, D15 : borne de rejet de la
        /// perception d'obstacles.
        /// </summary>
        public float MaximumChordTangentAngleRadians
        {
            get { return _maximumChordTangentAngle; }
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
        /// <remarks>
        /// Definition : parmi les segments qui rejoignent la plage (s1 &gt;= sMin et s0 &lt;= sMax), bornes aux extremites de la
        /// plage, le premier par abscisse croissante dont la distance au carre est strictement la plus petite. Calcul accelere
        /// (Story 5.33, D15), au bit pres : plage de segments par dichotomie, extremites des segments interieurs lues dans le
        /// cache (tMin = 0 et tMax = 1 exactement, memes expressions), segments de bord calcules comme avant, et tout ce qui
        /// ne peut ni gagner ni egaler saute (voir <see cref="ProjectCore"/>).
        /// </remarks>
        public RoadProjection Project(Vector3 point, float sMin, float sMax)
        {
            TrafficV2WorkCounters.Work.ProjectCalls++;
            return ProjectCore(point, sMin, sMax, false, 0f, false, false);
        }

        /// <summary>
        /// Exactement <see cref="Project(Vector3)"/>, au bit pres, amorcee sous <paramref name="hintS"/> (Story 5.33, D14) :
        /// <paramref name="hintS"/> ne change jamais le resultat, seulement le cout. Une amorce proche du point le plus proche
        /// (le s du point voisin deja projete) reduit la recherche a quelques segments.
        /// </summary>
        public RoadProjection ProjectNearest(Vector3 point, float hintS)
        {
            TrafficV2WorkCounters.Work.NearestCalls++;
            return ProjectCore(point, StartS, Length, true, hintS, true, false);
        }

        /// <summary>
        /// <see cref="ProjectNearest"/> reduite a SMeters, DistanceMeters et LongitudinalOverrunMeters, identiques au bit pres
        /// (meme Locate, meme interpolation de position ; la tangente n'est calculee qu'aux bornes du domaine, la ou le
        /// depassement en depend). Lateral, normal et repere ne sont pas calcules. Story 5.33, D15 : occupation de la frame.
        /// </summary>
        public RoadProjection ProjectNearestCompact(Vector3 point, float hintS)
        {
            TrafficV2WorkCounters.Work.NearestCalls++;
            return ProjectCore(point, StartS, Length, true, hintS, true, true);
        }

        /// <summary>
        /// Coeur exact des projections (Story 5.33, D14 puis D15). Les segments de la plage sont parcourus dans l'ordre
        /// d'abscisse croissante avec les memes expressions et la meme comparaison stricte que le parcours lineaire ; un
        /// super-bloc, un bloc ou un segment n'est saute que si sa boite (celle du segment entier, qui contient le segment
        /// borne) est plus loin que d_amorce + <see cref="ProjectionPruneMarginMeters"/>. d_amorce est la distance exacte d'un
        /// segment de la plage (sous l'indice, sinon sous la boite la plus proche, descendue par super-blocs puis blocs) :
        /// d_amorce &gt;= d_min, donc un segment saute est strictement plus loin que le minimum, a une marge tres superieure a
        /// l'arrondi, et le premier segment qui atteint le minimum est le meme.
        /// </summary>
        private RoadProjection ProjectCore(Vector3 point, float sMin, float sMax, bool hinted, float hintS, bool nearest, bool compact)
        {
            sMin = Mathf.Clamp(sMin, StartS, Length);
            sMax = Mathf.Clamp(sMax, StartS, Length);
            if (sMax < sMin)
            {
                sMax = sMin;
            }

            int segments = _samples.Length - 1;
            // Domaine entier : premier et dernier segment sans recherche (les memes indices que la dichotomie).
            int first = sMin == StartS ? 0 : FirstSegmentReaching(sMin);
            int last = sMax == Length ? segments - 1 : LastSegmentStartingBy(sMax);
            if (nearest) TrafficV2WorkCounters.Work.NearestSegmentsIterated += last - first + 1;
            else TrafficV2WorkCounters.Work.ProjectSegmentsIterated += last - first + 1;

            int seed;
            if (hinted)
            {
                float unused;
                Locate(hintS, out seed, out unused);
                seed = Math.Min(Math.Max(seed, first), last);
            }
            else seed = NearestBoxSegment(point, first, last);
            float u, tMin, tMax;
            float reach = Mathf.Sqrt(RangeSegmentDistanceSquared(seed, point, sMin, sMax, out u, out tMin, out tMax))
                + ProjectionPruneMarginMeters;
            float limit = reach * reach;

            float bestDistanceSquared = float.PositiveInfinity;
            float bestS = sMin;
            const int blocksPerSuper = ProjectionSuperBlockSegments / ProjectionBlockSegments;
            for (int super = first / ProjectionSuperBlockSegments; super <= last / ProjectionSuperBlockSegments; super++)
            {
                if (BoxDistanceSquared(point, _superMin[super], _superMax[super]) > limit) continue;
                int blockFrom = Math.Max(super * blocksPerSuper, first / ProjectionBlockSegments);
                int blockTo = Math.Min(Math.Min(_blockMin.Length, (super + 1) * blocksPerSuper) - 1, last / ProjectionBlockSegments);
                for (int block = blockFrom; block <= blockTo; block++)
                {
                    if (BoxDistanceSquared(point, _blockMin[block], _blockMax[block]) > limit) continue;
                    int from = Math.Max(block * ProjectionBlockSegments, first);
                    int to = Math.Min(Math.Min(segments, (block + 1) * ProjectionBlockSegments) - 1, last);
                    for (int i = from; i <= to; i++)
                    {
                        if (BoxDistanceSquared(point, _segmentMin[i], _segmentMax[i]) > limit) continue;
                        if (nearest) TrafficV2WorkCounters.Work.NearestSegmentsEvaluated++;
                        else TrafficV2WorkCounters.Work.ProjectSegmentsEvaluated++;
                        float distanceSquared = RangeSegmentDistanceSquared(i, point, sMin, sMax, out u, out tMin, out tMax);
                        // Strictement inferieur : parcours par abscisse croissante, donc la premiere egalite
                        // rencontree -- la plus petite abscisse -- est conservee.
                        if (distanceSquared < bestDistanceSquared)
                        {
                            float s0 = _samples[i].SMeters, span = _samples[i + 1].SMeters - s0;
                            bestDistanceSquared = distanceSquared;
                            bestS = s0 + (tMin + (tMax - tMin) * u) * span;
                        }
                    }
                }
            }

            bestS = Mathf.Clamp(bestS, sMin, sMax);
            if (compact)
            {
                // Sample(bestS) se reduit a sa position (memes Locate et interpolation) ; tangente seulement si une borne est touchee.
                int index;
                float t;
                Locate(bestS, out index, out t);
                Vector3 near = point - Vector3.LerpUnclamped(_samples[index].Position, _samples[index + 1].Position, t);
                float exceed = 0f;
                bool atMin = bestS <= sMin + BoundEpsilonMeters, atMax = bestS >= sMax - BoundEpsilonMeters;
                if (atMin || atMax)
                {
                    float onTangent = Vector3.Dot(near, Interpolate(index, t).Tangent);
                    if (onTangent < 0f && atMin) exceed = -onTangent;
                    else if (onTangent > 0f && atMax) exceed = onTangent;
                }
                var reduced = new RoadProjection();
                reduced.SMeters = bestS;
                reduced.DistanceMeters = near.magnitude;
                reduced.LongitudinalOverrunMeters = exceed;
                return reduced;
            }
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

        /// <summary>Premier segment i tel que s(i+1) &gt;= s : le premier que le parcours lineaire ne saute pas.</summary>
        private int FirstSegmentReaching(float s)
        {
            int low = 0, high = _samples.Length - 2;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (_samples[mid + 1].SMeters >= s) high = mid; else low = mid + 1;
            }
            return low;
        }

        /// <summary>Dernier segment i tel que s(i) &lt;= s : le dernier que le parcours lineaire ne saute pas.</summary>
        private int LastSegmentStartingBy(float s)
        {
            int low = 0, high = _samples.Length - 2;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (_samples[mid].SMeters <= s) low = mid; else high = mid - 1;
            }
            return low;
        }

        /// <summary>Segment de [first, last] sous la boite la plus proche, par super-bloc, bloc puis segment : une amorce, pas un resultat.</summary>
        private int NearestBoxSegment(Vector3 point, int first, int last)
        {
            const int blocksPerSuper = ProjectionSuperBlockSegments / ProjectionBlockSegments;
            int super = first / ProjectionSuperBlockSegments;
            float best = float.PositiveInfinity;
            for (int k = first / ProjectionSuperBlockSegments; k <= last / ProjectionSuperBlockSegments; k++)
            {
                float d = BoxDistanceSquared(point, _superMin[k], _superMax[k]);
                if (d < best) { best = d; super = k; }
            }
            int blockFrom = Math.Max(super * blocksPerSuper, first / ProjectionBlockSegments);
            int blockTo = Math.Min(Math.Min(_blockMin.Length, (super + 1) * blocksPerSuper) - 1, last / ProjectionBlockSegments);
            int block = blockFrom;
            best = float.PositiveInfinity;
            for (int k = blockFrom; k <= blockTo; k++)
            {
                float d = BoxDistanceSquared(point, _blockMin[k], _blockMax[k]);
                if (d < best) { best = d; block = k; }
            }
            int from = Math.Max(block * ProjectionBlockSegments, first);
            int to = Math.Min(Math.Min(_samples.Length - 1, (block + 1) * ProjectionBlockSegments) - 1, last);
            int segment = from;
            best = float.PositiveInfinity;
            for (int i = from; i <= to; i++)
            {
                float d = BoxDistanceSquared(point, _segmentMin[i], _segmentMax[i]);
                if (d < best) { best = d; segment = i; }
            }
            return segment;
        }

        /// <summary>
        /// Distance au carre du point au segment i borne a [sMin, sMax], memes expressions que le parcours lineaire. Un segment
        /// interieur a la plage a tMin = 0 et tMax = 1 exactement : ses extremites sont celles du cache, au bit pres.
        /// </summary>
        private float RangeSegmentDistanceSquared(int i, Vector3 point, float sMin, float sMax, out float u, out float tMin, out float tMax)
        {
            float s0 = _samples[i].SMeters, s1 = _samples[i + 1].SMeters;
            Vector3 a, b;
            if (s0 >= sMin && s1 <= sMax)
            {
                tMin = _segmentTMin[i]; tMax = _segmentTMax[i];
                a = _segmentA[i]; b = _segmentB[i];
            }
            else
            {
                float span = s1 - s0;
                tMin = Mathf.Max(0f, (sMin - s0) / span);
                tMax = Mathf.Min(1f, (sMax - s0) / span);
                a = Vector3.LerpUnclamped(_samples[i].Position, _samples[i + 1].Position, tMin);
                b = Vector3.LerpUnclamped(_samples[i].Position, _samples[i + 1].Position, tMax);
            }
            Vector3 ab = b - a;
            float lengthSquared = ab.sqrMagnitude;
            u = lengthSquared > 1e-12f ? Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSquared) : 0f;
            return (point - (a + ab * u)).sqrMagnitude;
        }

        private static float BoxDistanceSquared(Vector3 p, Vector3 min, Vector3 max)
        {
            float dx = Mathf.Max(0f, Mathf.Max(min.x - p.x, p.x - max.x));
            float dy = Mathf.Max(0f, Mathf.Max(min.y - p.y, p.y - max.y));
            float dz = Mathf.Max(0f, Mathf.Max(min.z - p.z, p.z - max.z));
            return dx * dx + dy * dy + dz * dz;
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
            return Advance(first, s0, s1, e, a, node);
        }

        /// <summary>
        /// Meme transport au bit pres, pour une suite d'appels a <paramref name="s0"/> croissant (l'horizon, point par point) :
        /// <paramref name="segmentCursor"/> suit l'indice que rendrait la recherche dichotomique, sans la refaire (Story 5.33,
        /// D14). Un curseur negatif, hors domaine ou en avant de s0 est recale par la recherche.
        /// </summary>
        public double AdvanceKinematicOffset(float s0, float s1, double e, float a, ref int segmentCursor)
        {
            s0 = Mathf.Clamp(s0, StartS, Length);
            s1 = Mathf.Clamp(s1, StartS, Length);
            if (!(a > 0f) || !(s1 > s0)) return e;
            if (segmentCursor < 0 || segmentCursor > _samples.Length - 2 || _samples[segmentCursor].SMeters > s0)
            {
                float unused;
                Locate(s0, out segmentCursor, out unused);
            }
            else
                while (segmentCursor + 1 <= _samples.Length - 2 && _samples[segmentCursor + 1].SMeters <= s0) segmentCursor++;
            return Advance(segmentCursor, s0, s1, e, a, null);
        }

        private double Advance(int first, float s0, float s1, double e, float a, Action<double, double> node)
        {
            TrafficV2WorkCounters.Work.KinematicCalls++;
            for (int i = first; i + 1 < _samples.Length && _samples[i].SMeters < s1; i++)
            {
                double x0 = Math.Max(s0, _samples[i].SMeters), x1 = Math.Min(s1, _samples[i + 1].SMeters);
                if (!(x1 > x0)) continue;
                double origin = _samples[i].SMeters;
                double k0 = _samples[i].CurvaturePerMeter;
                double slope = (_samples[i + 1].CurvaturePerMeter - k0) / (_samples[i + 1].SMeters - origin);
                int steps = Math.Max(1, (int)Math.Ceiling((x1 - x0) / KinematicOffsetStepMeters));
                TrafficV2WorkCounters.Work.KinematicSteps += steps;
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
