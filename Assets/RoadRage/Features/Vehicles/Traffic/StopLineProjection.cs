using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Projection d'une ligne d'arret ou de cession sur un mouvement controle (Story 5.35) : abscisse s_line ou la polyligne
    /// des echantillons du mouvement coupe le segment de la ligne, dans le plan route de chaque troncon (right = up x tangente,
    /// AD-45). Fonction pure, partagee par le validateur (croisement unique exige) et par le modele compile (s_line).
    /// </summary>
    public static class StopLineProjection
    {
        /// <summary>Deux croisements plus proches que ce pas sont un seul (sommet partage entre deux troncons).</summary>
        public const float DuplicateCrossingMeters = 1e-3f;

        /// <summary>
        /// Croisements du mouvement avec la ligne, abscisses croissantes et dedoublonnees. Un croisement n'est valide que dans
        /// ]0, L[ : la ligne doit couper le mouvement, ni a son debut ni a sa fin.
        /// </summary>
        public static List<float> Crossings(IReadOnlyList<RoadCurveSample> samples, RoadLineSegment line)
        {
            var result = new List<float>();
            if (samples == null || samples.Count < 2) return result;
            for (int i = 0; i + 1 < samples.Count; i++)
            {
                var a = samples[i];
                var b = samples[i + 1];
                Vector3 up = (a.Up + b.Up).normalized;
                if (up.sqrMagnitude < 0.5f) continue;
                Vector3 u = Vector3.ProjectOnPlane(b.Position - a.Position, up);
                if (u.sqrMagnitude < 1e-12f) continue;
                Vector3 e1 = u.normalized;
                Vector3 e2 = Vector3.Cross(up, e1);
                Vector2 p = Vector2.zero;
                Vector2 q = new Vector2(Vector3.Dot(u, e1), 0f);
                Vector2 c = Plan(line.Start - a.Position, e1, e2);
                Vector2 d = Plan(line.End - a.Position, e1, e2);
                float t;
                if (!Intersect(p, q, c, d, out t)) continue;
                float s = a.SMeters + t * (b.SMeters - a.SMeters);
                if (result.Count == 0 || s - result[result.Count - 1] > DuplicateCrossingMeters) result.Add(s);
            }
            return result;
        }

        /// <summary>Abscisse unique de la ligne sur le mouvement, strictement dans ]0, L[ ; faux si absente, multiple ou au bord.</summary>
        public static bool TryProject(IReadOnlyList<RoadCurveSample> samples, float lengthMeters, RoadLineSegment line, out float sLineMeters)
        {
            sLineMeters = 0f;
            var crossings = Crossings(samples, line);
            if (crossings.Count != 1 || !(crossings[0] > 0f) || !(crossings[0] < lengthMeters)) return false;
            sLineMeters = crossings[0];
            return true;
        }

        private static Vector2 Plan(Vector3 v, Vector3 e1, Vector3 e2)
        {
            return new Vector2(Vector3.Dot(v, e1), Vector3.Dot(v, e2));
        }

        /// <summary>Intersection de [p, q] et [c, d] ; t sur [p, q], dans [0, 1].</summary>
        private static bool Intersect(Vector2 p, Vector2 q, Vector2 c, Vector2 d, out float t)
        {
            t = 0f;
            Vector2 r = q - p, s = d - c;
            float denominator = r.x * s.y - r.y * s.x;
            if (Math.Abs(denominator) < 1e-12f) return false;
            Vector2 w = c - p;
            t = (w.x * s.y - w.y * s.x) / denominator;
            float u = (w.x * r.y - w.y * r.x) / denominator;
            return t >= 0f && t <= 1f && u >= 0f && u <= 1f;
        }
    }
}
