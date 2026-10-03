using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.33, D15 : la projection acceleree de RoadCurve (plage par dichotomie, amorce, elagage par boites) est egale au
    /// bit pres au parcours lineaire qu'elle remplace, recopie ici tel quel, sur chaque element de MVP_Run : plages
    /// completes, partielles, bornees sur un echantillon exact, effondrees ou hors domaine, et amorces quelconques.
    /// </summary>
    [Category("Core")]
    [Category("Story533")]
    public sealed class Story533ProjectionExactnessTests
    {
        private const float BoundEpsilonMeters = 1e-4f;

        private static TrafficV2Admission admission;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                        File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
                Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
                return admission;
            }
        }

        /// <summary>RoadCurve.Project(point, sMin, sMax) d'avant D15, recopie : parcours lineaire de tous les segments.</summary>
        private static RoadProjection Linear(RoadCurve curve, IReadOnlyList<RoadCurveSample> samples, Vector3 point, float sMin, float sMax)
        {
            sMin = Mathf.Clamp(sMin, curve.StartS, curve.Length);
            sMax = Mathf.Clamp(sMax, curve.StartS, curve.Length);
            if (sMax < sMin) sMax = sMin;
            float bestDistanceSquared = float.PositiveInfinity;
            float bestS = sMin;
            for (int i = 0; i < samples.Count - 1; i++)
            {
                float s0 = samples[i].SMeters;
                float s1 = samples[i + 1].SMeters;
                if (s1 < sMin || s0 > sMax) continue;
                float span = s1 - s0;
                float tMin = Mathf.Max(0f, (sMin - s0) / span);
                float tMax = Mathf.Min(1f, (sMax - s0) / span);
                Vector3 a = Vector3.LerpUnclamped(samples[i].Position, samples[i + 1].Position, tMin);
                Vector3 b = Vector3.LerpUnclamped(samples[i].Position, samples[i + 1].Position, tMax);
                Vector3 ab = b - a;
                float lengthSquared = ab.sqrMagnitude;
                float u = lengthSquared > 1e-12f ? Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSquared) : 0f;
                float distanceSquared = (point - (a + ab * u)).sqrMagnitude;
                if (distanceSquared < bestDistanceSquared)
                {
                    bestDistanceSquared = distanceSquared;
                    bestS = s0 + (tMin + (tMax - tMin) * u) * span;
                }
            }
            bestS = Mathf.Clamp(bestS, sMin, sMax);
            var frame = curve.Sample(bestS);
            Vector3 offset = point - frame.Position;
            float along = Vector3.Dot(offset, frame.Tangent);
            float overrun = 0f;
            if (along < 0f && bestS <= sMin + BoundEpsilonMeters) overrun = -along;
            else if (along > 0f && bestS >= sMax - BoundEpsilonMeters) overrun = along;
            return new RoadProjection { SMeters = bestS, LateralOffsetMeters = Vector3.Dot(offset, frame.Right),
                NormalOffsetMeters = Vector3.Dot(offset, frame.Up), DistanceMeters = offset.magnitude,
                LongitudinalOverrunMeters = overrun, Point = frame };
        }

        private static bool Bits(float a, float b) { return BitConverter.SingleToInt32Bits(a) == BitConverter.SingleToInt32Bits(b); }
        private static bool Bits(Vector3 a, Vector3 b) { return Bits(a.x, b.x) && Bits(a.y, b.y) && Bits(a.z, b.z); }

        private static bool Same(RoadProjection a, RoadProjection b)
        {
            return Bits(a.SMeters, b.SMeters) && Bits(a.LateralOffsetMeters, b.LateralOffsetMeters)
                && Bits(a.NormalOffsetMeters, b.NormalOffsetMeters) && Bits(a.DistanceMeters, b.DistanceMeters)
                && Bits(a.LongitudinalOverrunMeters, b.LongitudinalOverrunMeters)
                && Bits(a.Point.Position, b.Point.Position) && Bits(a.Point.Tangent, b.Point.Tangent) && Bits(a.Point.Up, b.Point.Up)
                && Bits(a.Point.Right, b.Point.Right) && Bits(a.Point.SMeters, b.Point.SMeters)
                && Bits(a.Point.CurvaturePerMeter, b.Point.CurvaturePerMeter)
                && Bits(a.Point.HalfWidthLeftMeters, b.Point.HalfWidthLeftMeters)
                && Bits(a.Point.HalfWidthRightMeters, b.Point.HalfWidthRightMeters);
        }

        /// <summary>
        /// D15, localisation : la cellule examinee contient tout element dont la boite de Consider (enveloppe complete
        /// elargie de 2 x voisinage) contient le point, dans l'ordre du balayage complet. Consider etant inchange et le tri des
        /// candidats total (rang, score, id), la localisation est identique a celle du balayage des 116 elements.
        /// </summary>
        [Test]
        public void TheLocalizationCellHoldsEveryElementWhoseBoxContainsThePoint()
        {
            var model = Admission.Model;
            var ids = new List<RoadId>();
            var curves = new List<RoadCurve>();
            foreach (var corridor in model.Corridors) { ids.Add(corridor.CorridorId); curves.Add(corridor.Curve); }
            foreach (var movement in model.Movements) { ids.Add(movement.Id); curves.Add(movement.Curve); }
            float neighbourhood = 2f * model.LocalizationProfile.AcceptanceDistanceMeters;
            var boxes = curves.Select(c => { var b = c.FullBounds; b.Expand(2f * neighbourhood); return b; }).ToList();
            var points = new List<Vector3>();
            var random = new System.Random(533152);
            foreach (var box in boxes) { points.Add(box.min); points.Add(box.max); points.Add(new Vector3(box.min.x, box.center.y, box.max.z)); }
            foreach (var curve in curves)
                for (float s = 0f; s <= curve.Length; s += 0.5f)
                {
                    var at = curve.Sample(s);
                    points.Add(at.Position + at.Right * (float)(random.NextDouble() * 24.0 - 12.0) + at.Up * (float)(random.NextDouble() * 6.0 - 3.0));
                }
            int checkedPoints = 0, contained = 0, missing = 0, disordered = 0;
            string first = null;
            foreach (var point in points)
            {
                var examined = RoadLocalizer.ExaminedElements(model, point);
                var order = new Dictionary<RoadId, int>();
                for (int k = 0; k < examined.Count; k++) order[examined[k]] = k;
                int previous = -1;
                for (int e = 0; e < ids.Count; e++)
                {
                    int at;
                    bool listed = order.TryGetValue(ids[e], out at);
                    if (listed) { if (at < previous) disordered++; previous = at; }
                    if (!boxes[e].Contains(point)) continue;
                    contained++;
                    if (!listed) { missing++; if (first == null) first = ids[e] + " absent de la cellule de " + point.ToString("F4"); }
                }
                checkedPoints++;
            }
            Debug.Log("[Story533] index de localisation : " + checkedPoints + " points, " + contained + " elements contenants, "
                + missing + " absent(s), " + disordered + " desordre(s) ; elements examines par point : "
                + points.Average(p => RoadLocalizer.ExaminedElements(model, p).Count).ToString("0.##") + " sur " + ids.Count + ".");
            Assert.That(missing, Is.EqualTo(0), first);
            Assert.That(disordered, Is.EqualTo(0), "ordre du balayage complet");
        }

        [Test]
        public void TheAcceleratedProjectionEqualsTheLinearProjectionToTheBitOnEveryElementAndRange()
        {
            var model = Admission.Model;
            var elements = new List<KeyValuePair<RoadCurve, IReadOnlyList<RoadCurveSample>>>();
            foreach (var corridor in model.Corridors) elements.Add(new KeyValuePair<RoadCurve, IReadOnlyList<RoadCurveSample>>(corridor.Curve, corridor.Samples));
            foreach (var movement in model.Movements) elements.Add(new KeyValuePair<RoadCurve, IReadOnlyList<RoadCurveSample>>(movement.Curve, movement.Samples));
            var random = new System.Random(533015);
            int ranged = 0, full = 0, nearest = 0, mismatches = 0;
            string first = null;
            foreach (var element in elements)
            {
                var curve = element.Key;
                var samples = element.Value;
                float length = curve.Length;
                for (int k = 0; k < 160; k++)
                {
                    // Points autour de la courbe (laterale, normale, depassement aux bouts), et quelques points lointains.
                    float s = (float)(random.NextDouble() * (length + 4.0) - 2.0);
                    var at = curve.Sample(Mathf.Clamp(s, curve.StartS, length));
                    float lateral = (float)(random.NextDouble() * 16.0 - 8.0), normal = (float)(random.NextDouble() * 4.0 - 2.0);
                    var point = at.Position + at.Right * lateral + at.Up * normal + at.Tangent * (s - Mathf.Clamp(s, curve.StartS, length));
                    if (k % 23 == 0) point += new Vector3((float)(random.NextDouble() * 120.0 - 60.0), 0f, (float)(random.NextDouble() * 120.0 - 60.0));

                    // Plages : sur des echantillons exacts, interieures, effondrees, inversees, hors domaine, minuscules.
                    int i0 = random.Next(samples.Count), i1 = random.Next(samples.Count);
                    float a, b;
                    switch (k % 6)
                    {
                        case 0: a = samples[i0].SMeters; b = samples[i1].SMeters; break;
                        case 1: a = (float)(random.NextDouble() * length); b = a + (float)(random.NextDouble() * 6.0); break;
                        case 2: a = samples[i0].SMeters; b = a; break;
                        case 3: a = (float)(random.NextDouble() * length); b = a - 1f; break;
                        case 4: a = -5f - (float)random.NextDouble(); b = length + 5f; break;
                        default: a = (float)(random.NextDouble() * length); b = a + 1e-4f; break;
                    }
                    var reference = Linear(curve, samples, point, a, b);
                    var actual = curve.Project(point, a, b);
                    ranged++;
                    if (!Same(reference, actual)) { mismatches++; if (first == null) first = "plage [" + a + ", " + b + "] point " + point.ToString("F4"); }

                    var whole = Linear(curve, samples, point, curve.StartS, length);
                    full++;
                    if (!Same(whole, curve.Project(point))) { mismatches++; if (first == null) first = "domaine point " + point.ToString("F4"); }
                    float hint = (float)(random.NextDouble() * (length + 20.0) - 10.0);
                    nearest++;
                    if (!Same(whole, curve.ProjectNearest(point, hint))) { mismatches++; if (first == null) first = "amorce " + hint + " point " + point.ToString("F4"); }
                    var compact = curve.ProjectNearestCompact(point, hint);
                    nearest++;
                    if (!Bits(compact.SMeters, whole.SMeters) || !Bits(compact.DistanceMeters, whole.DistanceMeters)
                        || !Bits(compact.LongitudinalOverrunMeters, whole.LongitudinalOverrunMeters))
                    { mismatches++; if (first == null) first = "compacte, amorce " + hint + " point " + point.ToString("F4"); }
                }
            }
            Debug.Log("[Story533] projection acceleree contre parcours lineaire : " + elements.Count + " elements, " + ranged
                + " plages, " + full + " domaines complets, " + nearest + " amorces ; " + mismatches + " difference(s).");
            Assert.That(mismatches, Is.EqualTo(0), first);
        }
    }
}
