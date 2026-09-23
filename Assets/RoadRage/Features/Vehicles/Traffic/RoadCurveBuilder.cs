using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Polyligne authoree -> echantillons compiles (Story 5.26). Catmull-Rom centripete (alpha 0,5)
    /// a travers les points de controle, extremites prolongees par un point fantome (voir
    /// <see cref="Phantom"/>), subdivision adaptative jusqu'a la tolerance de corde,
    /// abscisse cumulee le long des cordes emises, tangente et courbure signee analytiques.
    ///
    /// Budget de tolerance : chaque corde emise s'ecarte de la spline d'au plus la MOITIE de
    /// <c>chordToleranceMeters</c>. L'autre moitie est reservee a l'ecart entre la spline et la
    /// courbe que l'auteur avait en tete ; elle n'est tenue que si les points de controle sont
    /// assez denses (mesure : un point tous les 10 degres sur un arc de 12 m tient 0,05 m).
    /// </summary>
    public static class RoadCurveBuilder
    {
        private const int MaxSubdivisionDepth = 16;

        /// <exception cref="ArgumentException">
        /// Tableaux absents ou de longueurs differentes, moins de deux points, valeur non finie,
        /// points consecutifs confondus, road-up nul ou parallele a la tangente, demi-largeur non
        /// positive, tolerance non positive, ou tolerance de corde inatteignable.
        /// </exception>
        public static RoadCurveSample[] Build(
            Vector3[] positions,
            Vector3[] ups,
            float[] halfWidthsLeftMeters,
            float[] halfWidthsRightMeters,
            float chordToleranceMeters)
        {
            float unused;
            return Build(positions, ups, halfWidthsLeftMeters, halfWidthsRightMeters, chordToleranceMeters, out unused);
        }

        /// <summary>
        /// Meme construction, qui rend en plus le maximum MESURE du critere de subdivision sur les
        /// cordes emises (Story 5.27) : distance spline-corde aux sondes u = 1/4, 1/2, 3/4 de chaque
        /// corde acceptee. Borne par construction a <c>chordToleranceMeters / 2</c> ; l'ecart entre
        /// la spline et l'intention de l'auteur n'en fait pas partie.
        /// </summary>
        public static RoadCurveSample[] Build(
            Vector3[] positions,
            Vector3[] ups,
            float[] halfWidthsLeftMeters,
            float[] halfWidthsRightMeters,
            float chordToleranceMeters,
            out float maxChordDeviationMeters)
        {
            maxChordDeviationMeters = 0f;
            if (positions == null || ups == null || halfWidthsLeftMeters == null || halfWidthsRightMeters == null)
            {
                throw new ArgumentException("RoadCurveBuilder exige positions, road-up et demi-largeurs.");
            }

            int count = positions.Length;
            if (count < 2 || ups.Length != count || halfWidthsLeftMeters.Length != count || halfWidthsRightMeters.Length != count)
            {
                throw new ArgumentException("RoadCurveBuilder exige au moins deux points et des tableaux de meme longueur.");
            }

            if (!(chordToleranceMeters > 0f) || float.IsInfinity(chordToleranceMeters))
            {
                throw new ArgumentException("La tolerance de corde doit etre finie et strictement positive.", "chordToleranceMeters");
            }

            for (int i = 0; i < count; i++)
            {
                if (!IsFinite(positions[i]) || !IsFinite(ups[i]) || ups[i].sqrMagnitude < 1e-12f
                    || !IsFinite(halfWidthsLeftMeters[i]) || !IsFinite(halfWidthsRightMeters[i]))
                {
                    throw new ArgumentException("Point de controle " + i + " non fini ou road-up nul.");
                }

                if (!(halfWidthsLeftMeters[i] > 0f && halfWidthsRightMeters[i] > 0f))
                {
                    throw new ArgumentException("Point de controle " + i + " : demi-largeur non strictement positive.");
                }

                if (i > 0 && (positions[i] - positions[i - 1]).sqrMagnitude < 1e-12f)
                {
                    throw new ArgumentException("Points de controle " + (i - 1) + " et " + i + " confondus.");
                }
            }

            var builder = new SegmentWalker(halfWidthsLeftMeters, halfWidthsRightMeters, ups, chordToleranceMeters * 0.5f);
            Vector3 phantomStart = count >= 3 ? Phantom(positions[0], positions[1], positions[2]) : 2f * positions[0] - positions[1];
            Vector3 phantomEnd = count >= 3
                ? Phantom(positions[count - 1], positions[count - 2], positions[count - 3])
                : 2f * positions[count - 1] - positions[count - 2];

            for (int i = 0; i < count - 1; i++)
            {
                Vector3 p0 = i == 0 ? phantomStart : positions[i - 1];
                Vector3 p3 = i == count - 2 ? phantomEnd : positions[i + 2];
                builder.Walk(i, p0, positions[i], positions[i + 1], p3);
            }

            maxChordDeviationMeters = builder.MaxAcceptedDeviation;
            return builder.Samples.ToArray();
        }

        /// <summary>
        /// Point fantome avant l'extremite <paramref name="end"/> : extrapolation quadratique par les
        /// trois derniers points, qui prolonge la courbure (une reflexion forcerait la tangente
        /// d'extremite sur la premiere corde et inverserait le signe de la courbure au bout d'un arc).
        /// Si la seconde difference depasse la moitie de la corde -- pas tres inegaux ou angle vif --
        /// l'extrapolation n'est plus fiable (fantome confondu ou en avant de l'extremite) : on
        /// revient a la reflexion.
        /// </summary>
        private static Vector3 Phantom(Vector3 end, Vector3 next, Vector3 afterNext)
        {
            Vector3 reflected = 2f * end - next;
            Vector3 secondDifference = afterNext - 2f * next + end;
            return secondDifference.magnitude <= 0.5f * (next - end).magnitude ? reflected + secondDifference : reflected;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        /// <summary>Un segment de spline sous forme d'Hermite cubique sur u dans [0, 1].</summary>
        private struct HermiteSegment
        {
            public Vector3 P1;
            public Vector3 P2;
            public Vector3 M1;
            public Vector3 M2;

            public Vector3 Position(float u)
            {
                float u2 = u * u;
                float u3 = u2 * u;
                return (2f * u3 - 3f * u2 + 1f) * P1 + (u3 - 2f * u2 + u) * M1 + (-2f * u3 + 3f * u2) * P2 + (u3 - u2) * M2;
            }

            public Vector3 FirstDerivative(float u)
            {
                float u2 = u * u;
                return (6f * u2 - 6f * u) * P1 + (3f * u2 - 4f * u + 1f) * M1 + (-6f * u2 + 6f * u) * P2 + (3f * u2 - 2f * u) * M2;
            }

            public Vector3 SecondDerivative(float u)
            {
                return (12f * u - 6f) * P1 + (6f * u - 4f) * M1 + (-12f * u + 6f) * P2 + (6f * u - 2f) * M2;
            }
        }

        private sealed class SegmentWalker
        {
            public readonly List<RoadCurveSample> Samples = new List<RoadCurveSample>();

            /// <summary>Maximum des distances sonde-corde sur les cordes acceptees.</summary>
            public float MaxAcceptedDeviation;

            private readonly float[] _left;
            private readonly float[] _right;
            private readonly Vector3[] _ups;
            private readonly float _tolerance;

            private HermiteSegment _segment;
            private int _index;

            public SegmentWalker(float[] left, float[] right, Vector3[] ups, float tolerance)
            {
                _left = left;
                _right = right;
                _ups = ups;
                _tolerance = tolerance;
            }

            public void Walk(int index, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
            {
                _index = index;

                // Catmull-Rom centripete : noeuds espaces de la racine des distances.
                float d01 = Mathf.Sqrt((p1 - p0).magnitude);
                float d12 = Mathf.Sqrt((p2 - p1).magnitude);
                float d23 = Mathf.Sqrt((p3 - p2).magnitude);

                Vector3 m1 = ((p1 - p0) / d01 - (p2 - p0) / (d01 + d12) + (p2 - p1) / d12) * d12;
                Vector3 m2 = ((p2 - p1) / d12 - (p3 - p1) / (d12 + d23) + (p3 - p2) / d23) * d12;

                _segment.P1 = p1;
                _segment.P2 = p2;
                _segment.M1 = m1;
                _segment.M2 = m2;

                if (Samples.Count == 0)
                {
                    Emit(0f, 0f);
                }

                Subdivide(0f, 1f, 0);
            }

            private void Subdivide(float u0, float u1, int depth)
            {
                Vector3 a = _segment.Position(u0);
                Vector3 b = _segment.Position(u1);
                bool withinTolerance = true;
                float deviation = 0f;
                for (int k = 1; k <= 3 && withinTolerance; k++)
                {
                    Vector3 probe = _segment.Position(u0 + (u1 - u0) * k * 0.25f);
                    float distance = DistanceToChord(probe, a, b);
                    withinTolerance = distance <= _tolerance;
                    deviation = Mathf.Max(deviation, distance);
                }

                if (!withinTolerance && depth >= MaxSubdivisionDepth)
                {
                    // Jamais de corde hors tolerance emise en silence.
                    throw new ArgumentException("Tolerance de corde inatteignable sur le segment " + _index
                        + " apres " + MaxSubdivisionDepth + " subdivisions.");
                }

                if (withinTolerance)
                {
                    MaxAcceptedDeviation = Mathf.Max(MaxAcceptedDeviation, deviation);
                    var previous = Samples[Samples.Count - 1];
                    Emit(u1, previous.SMeters + (b - previous.Position).magnitude);
                    return;
                }

                float mid = 0.5f * (u0 + u1);
                Subdivide(u0, mid, depth + 1);
                Subdivide(mid, u1, depth + 1);
            }

            private void Emit(float u, float s)
            {
                Vector3 velocity = _segment.FirstDerivative(u);
                Vector3 acceleration = _segment.SecondDerivative(u);
                Vector3 tangent = velocity.normalized;

                Vector3 authoredUp = Vector3.LerpUnclamped(_ups[_index].normalized, _ups[_index + 1].normalized, u);
                Vector3 orthogonalUp = authoredUp - tangent * Vector3.Dot(authoredUp, tangent);
                if (orthogonalUp.sqrMagnitude < 1e-8f)
                {
                    throw new ArgumentException("Road-up parallele a la tangente sur le segment " + _index + " : repere indefini.");
                }

                Vector3 up = orthogonalUp.normalized;
                Vector3 right = Vector3.Cross(up, tangent).normalized;

                // dT/ds = composante normale de l'acceleration divisee par |v|^2 ; signe par road-right.
                float speedSquared = velocity.sqrMagnitude;
                Vector3 curvatureVector = (acceleration - tangent * Vector3.Dot(acceleration, tangent)) / speedSquared;

                var sample = new RoadCurveSample();
                sample.SMeters = s;
                sample.Position = _segment.Position(u);
                sample.Tangent = tangent;
                sample.Up = up;
                sample.CurvaturePerMeter = Vector3.Dot(curvatureVector, right);
                sample.HalfWidthLeftMeters = Mathf.LerpUnclamped(_left[_index], _left[_index + 1], u);
                sample.HalfWidthRightMeters = Mathf.LerpUnclamped(_right[_index], _right[_index + 1], u);
                Samples.Add(sample);
            }

            private static float DistanceToChord(Vector3 point, Vector3 a, Vector3 b)
            {
                Vector3 ab = b - a;
                float lengthSquared = ab.sqrMagnitude;
                float t = lengthSquared > 1e-12f ? Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSquared) : 0f;
                return (point - (a + ab * t)).magnitude;
            }
        }
    }
}
