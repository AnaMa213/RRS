using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    /// <summary>
    /// Chemin complet de la route restante pour la perception (Story 5.33, D14). Memes etendues, meme longueur et, dans le
    /// meme ordre, memes positions, tangentes et distances de points que <see cref="PathHorizon.Build"/> sur la meme portee,
    /// au bit pres : premier point curve.Sample(s0), echantillons interieurs lus dans le cache de la courbe, dernier point
    /// curve.Sample(s1). Ni pose nominale, ni plafond, ni plan : la perception ne lit que la geometrie, et garde ainsi toute la
    /// route alors que la planification est bornee (un leader lointain doit rester percu).
    /// </summary>
    public sealed class RoutePath : IPathGeometry
    {
        private readonly PathSpan[] spans;
        private readonly RoadCurve[] curves;
        private readonly int[] firstSample, innerCount;
        private readonly bool[] hasEnd;
        private readonly RoadCurvePoint[] starts, ends;

        public IReadOnlyList<PathSpan> Spans { get; }
        public float LengthMeters { get; }

        private RoutePath(List<PathSpan> spans, List<RoadCurve> curves, List<int> first, List<int> inner, List<bool> hasEnd,
            List<RoadCurvePoint> starts, List<RoadCurvePoint> ends, float length)
        {
            this.spans = spans.ToArray();
            this.curves = curves.ToArray();
            firstSample = first.ToArray();
            innerCount = inner.ToArray();
            this.hasEnd = hasEnd.ToArray();
            this.starts = starts.ToArray();
            this.ends = ends.ToArray();
            Spans = Array.AsReadOnly(this.spans);
            LengthMeters = length;
        }

        /// <summary>Meme parcours que <see cref="PathHorizon.Build"/> (sans pose nominale), arret au premier element manquant.</summary>
        public static RoutePath Build(CompiledRoadModel model, RoutePlan route, float lookAheadMeters)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (route == null) throw new ArgumentNullException("route");
            if (!(lookAheadMeters > 0f) || float.IsInfinity(lookAheadMeters))
                throw new ArgumentException("InvalidLookAhead", "lookAheadMeters");
            if (route.ModelId != model.ModelId || route.ModelVersion != model.Version)
                throw new ArgumentException("StalePlan", "route");
            int capacity = Math.Max(1, route.Occurrences.Count - route.ProgressOccurrenceIndex);
            var spans = new List<PathSpan>(capacity);
            var curves = new List<RoadCurve>(capacity);
            var first = new List<int>(capacity);
            var inner = new List<int>(capacity);
            var hasEnd = new List<bool>(capacity);
            var starts = new List<RoadCurvePoint>(capacity);
            var ends = new List<RoadCurvePoint>(capacity);
            float travelled = 0f;
            for (int i = route.ProgressOccurrenceIndex; i < route.Occurrences.Count && travelled < lookAheadMeters; i++)
            {
                var occurrence = route.Occurrences[i];
                var curve = PathHorizon.CurveOf(model, occurrence);
                if (curve == null) break;
                float s0 = i == route.ProgressOccurrenceIndex ? route.ProgressSMeters : occurrence.StartSMeters;
                float s1 = Math.Min(occurrence.EndSMeters, s0 + lookAheadMeters - travelled);
                int low = curve.FirstSampleAbove(s0), high = low;
                while (high < curve.SampleCount && curve.SampleS(high) < s1) high++;
                spans.Add(new PathSpan(occurrence.Kind, occurrence.Id, s0, s1, travelled, travelled + s1 - s0));
                curves.Add(curve);
                first.Add(low);
                inner.Add(high - low);
                hasEnd.Add(s1 > s0);
                starts.Add(curve.Sample(s0));
                ends.Add(s1 > s0 ? curve.Sample(s1) : default(RoadCurvePoint));
                travelled += s1 - s0;
            }
            TrafficV2WorkCounters.Work.RoutePathBuilds++;
            TrafficV2WorkCounters.Work.RoutePathSpans += spans.Count;
            return new RoutePath(spans, curves, first, inner, hasEnd, starts, ends, travelled);
        }

        public int PointCount(int span)
        {
            return 1 + innerCount[span] + (hasEnd[span] ? 1 : 0);
        }

        public Vector3 PointPosition(int span, int point)
        {
            return Point(span, point).Position;
        }

        public Vector3 PointTangent(int span, int point)
        {
            return Point(span, point).Tangent;
        }

        public float PointDistance(int span, int point)
        {
            var s = spans[span];
            if (point == 0) return s.StartDistanceMeters;
            if (hasEnd[span] && point == PointCount(span) - 1) return s.StartDistanceMeters + s.EndSMeters - s.StartSMeters;
            return s.StartDistanceMeters + curves[span].SampleS(firstSample[span] + point - 1) - s.StartSMeters;
        }

        private RoadCurvePoint Point(int span, int point)
        {
            if (point == 0) return starts[span];
            if (hasEnd[span] && point == PointCount(span) - 1) return ends[span];
            return curves[span].SampleFrame(firstSample[span] + point - 1);
        }
    }
}
