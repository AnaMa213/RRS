using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Perception
{
    /// <summary>Portees et capacites declarees par l'appelant ; la 5.32 n'en code aucune valeur.</summary>
    public readonly struct PerceptionLimits
    {
        public readonly float RearRangeMeters;
        public readonly float LateralRangeMeters;
        public readonly float AdjacentWindowMeters;
        public readonly int ListCapacity;

        public PerceptionLimits(float rearRangeMeters, float lateralRangeMeters, float adjacentWindowMeters, int listCapacity)
        {
            RearRangeMeters = rearRangeMeters; LateralRangeMeters = lateralRangeMeters;
            AdjacentWindowMeters = adjacentWindowMeters; ListCapacity = listCapacity;
            Validate();
        }

        internal void Validate()
        {
            if (!Positive(RearRangeMeters) || !Positive(LateralRangeMeters) || !Positive(AdjacentWindowMeters) || ListCapacity < 1)
                throw new ArgumentException("InvalidPerceptionLimits");
        }

        private static bool Positive(float value)
        {
            return value > 0f && !float.IsInfinity(value);
        }
    }

    /// <summary>
    /// Perception pure (Story 5.32) : faits objectifs pour un agent, lus dans une frame partagee le long de son
    /// horizon. Aucun etat, aucune ecriture, aucune decision : elle ne dit ni qui passe ni quelle commande appliquer.
    /// </summary>
    public static class TrafficPerception
    {
        public static AgentObservation Observe(TrafficFrame frame, RoadId trafficId, PathHorizon horizon,
            PerceptionLimits limits, SpatialQueryBuffer buffer)
        {
            if (frame == null) throw new ArgumentNullException("frame");
            if (horizon == null) throw new ArgumentNullException("horizon");
            if (buffer == null) throw new ArgumentNullException("buffer");
            limits.Validate();
            TrafficActor agent;
            if (!frame.TryGetActor(trafficId, out agent)) throw new ArgumentException("UnknownTrafficId", "trafficId");

            var context = new Context(frame, agent, horizon, limits);
            var self = default(ElementOccupant);
            PerceptionStatus bodyStatus = !agent.FootprintDeclared ? PerceptionStatus.UndeclaredFootprint
                : !frame.TryGetOccupancy(trafficId, out self) || horizon.Intervals.Count == 0
                    || horizon.Intervals[0].Id != self.ElementId ? PerceptionStatus.AgentOccupancyUnavailable
                : PerceptionStatus.Evaluated;
            context.Self = self;

            var leader = bodyStatus == PerceptionStatus.Evaluated ? Leader(context) : ObservationChannel<VehicleGapFact>.Unavailable(bodyStatus);
            var follower = bodyStatus == PerceptionStatus.Evaluated ? Follower(context) : ObservationChannel<VehicleGapFact>.Unavailable(bodyStatus);
            var adjacent = bodyStatus == PerceptionStatus.Evaluated ? Adjacent(context) : ObservationChannel<AdjacentOccupantFact>.Unavailable(bodyStatus);
            ObservationChannel<ObstacleFact> obstacles;
            buffer.Clear();
            if (bodyStatus == PerceptionStatus.Evaluated) obstacles = Obstacles(context, buffer);
            else obstacles = ObservationChannel<ObstacleFact>.Unavailable(bodyStatus);
            var overlaps = IntentOverlaps(context);
            var exit = Exit(context);
            var unmeasured = Bounded(context, context.Unmeasured, CompareUnmeasured, PerceptionStatus.Evaluated, 0f);
            return new AgentObservation(frame.FrameId, agent, leader, follower, adjacent, obstacles, overlaps, exit,
                unmeasured, buffer.Total, buffer.Saturated);
        }

        private sealed class Context
        {
            public readonly TrafficFrame Frame;
            public readonly TrafficActor Agent;
            public readonly PathHorizon Horizon;
            public readonly PerceptionLimits Limits;
            public ElementOccupant Self;
            public readonly List<UnmeasuredActorFact> Unmeasured = new List<UnmeasuredActorFact>();

            public Context(TrafficFrame frame, TrafficActor agent, PathHorizon horizon, PerceptionLimits limits)
            { Frame = frame; Agent = agent; Horizon = horizon; Limits = limits; }

            /// <summary>Distance d'horizon du pare-chocs avant de l'agent.</summary>
            public float FrontDistance
            {
                get { var first = Horizon.Intervals[0]; return first.StartDistanceMeters + Self.SMaxMeters - first.StartSMeters; }
            }

            public ObservationMetadata Structured(float range, float otherConfidence)
            {
                return new ObservationMetadata(Frame.FrameId, ObservationSource.StructuredOccupancy, range,
                    Mathf.Min(Agent.Location.Confidence, otherConfidence));
            }

            /// <summary>Note les acteurs localises sur l'element mais sans occupation (aucune distance inventee).</summary>
            public void NoteUnmeasured(RoadId elementId)
            {
                for (int i = 0; i < Frame.Actors.Count; i++)
                {
                    var actor = Frame.Actors[i];
                    if (actor.TrafficId == Agent.TrafficId || !actor.Location.Localized || actor.Location.ElementId != elementId
                        || actor.OccupancyExclusion == OccupancyExclusion.None) continue;
                    bool known = false;
                    for (int k = 0; k < Unmeasured.Count && !known; k++) known = Unmeasured[k].TrafficId == actor.TrafficId;
                    if (!known)
                        Unmeasured.Add(new UnmeasuredActorFact(actor.TrafficId, elementId, actor.Location.SMeters, actor.OccupancyExclusion));
                }
            }
        }

        private static ObservationChannel<VehicleGapFact> Leader(Context c)
        {
            var intervals = c.Horizon.Intervals;
            float front = c.FrontDistance, range = c.Horizon.LengthMeters;
            var candidates = new List<VehicleGapFact>();
            var seen = new HashSet<RoadId>();
            for (int i = 0; i < intervals.Count; i++)
            {
                var interval = intervals[i];
                c.NoteUnmeasured(interval.Id);
                var occupants = c.Frame.GetOccupants(interval.Id);
                for (int k = 0; k < occupants.Count; k++)
                {
                    var o = occupants[k];
                    if (o.TrafficId == c.Agent.TrafficId || seen.Contains(o.TrafficId)) continue;
                    // Premier intervalle : devant par la reference (egalite departagee par id), corps dans l'horizon.
                    bool ahead = i == 0 ? Ahead(o, c) && o.SMinMeters <= interval.EndSMeters
                        : o.SMaxMeters >= interval.StartSMeters && o.SMinMeters <= interval.EndSMeters;
                    if (!ahead) continue;
                    seen.Add(o.TrafficId);
                    float gap = interval.StartDistanceMeters + o.SMinMeters - interval.StartSMeters - front;
                    candidates.Add(new VehicleGapFact(o.TrafficId, o.ElementId, gap, o.SpeedMetersPerSecond, c.Structured(range, o.Confidence)));
                }
            }
            candidates.Sort(CompareGap);
            int total = candidates.Count;
            if (total > 1) candidates.RemoveRange(1, total - 1);
            return new ObservationChannel<VehicleGapFact>(PerceptionStatus.Evaluated, candidates, total, false, range);
        }

        /// <summary>Devant sur l'element de l'agent : reference plus loin, ou egale avec un id plus grand.</summary>
        private static bool Ahead(ElementOccupant o, Context c)
        {
            return o.SMeters > c.Self.SMeters || (o.SMeters == c.Self.SMeters && o.TrafficId.CompareTo(c.Agent.TrafficId) > 0);
        }

        private static ObservationChannel<VehicleGapFact> Follower(Context c)
        {
            var model = c.Frame.Model;
            var self = c.Self;
            float range = c.Limits.RearRangeMeters;
            var candidates = new List<VehicleGapFact>();
            var own = c.Frame.GetOccupants(self.ElementId);
            for (int k = 0; k < own.Count; k++)
            {
                var o = own[k];
                if (o.TrafficId == c.Agent.TrafficId || Ahead(o, c)) continue;
                AddWithin(c, candidates, o, self.SMinMeters - o.SMaxMeters, range);
            }
            float searched = Mathf.Max(0f, self.SMinMeters);
            // Predecesseurs directs seulement : un element en amont. Au-dela, la portee reste celle cherchee.
            var predecessors = new List<RoadId>();
            if (self.Kind == RoadElementKind.JunctionMovement)
            {
                CompiledJunctionMovement movement;
                if (model.TryGetMovement(self.ElementId, out movement)) predecessors.Add(movement.FromCorridorId);
            }
            else
            {
                predecessors.AddRange(model.GetPredecessorCorridors(self.ElementId));
                for (int m = 0; m < model.Movements.Count; m++)
                    if (model.Movements[m].ToCorridorId == self.ElementId) predecessors.Add(model.Movements[m].Id);
            }
            for (int p = 0; p < predecessors.Count; p++)
            {
                RoadElementKind kind;
                RoadCurve curve;
                IReadOnlyList<RoadCurveSample> samples;
                if (!TrafficFrame.TryGetElement(model, predecessors[p], out kind, out curve, out samples)) continue;
                c.NoteUnmeasured(predecessors[p]);
                searched = Mathf.Max(searched, self.SMinMeters + curve.Length);
                var occupants = c.Frame.GetOccupants(predecessors[p]);
                for (int k = 0; k < occupants.Count; k++)
                {
                    var o = occupants[k];
                    if (o.TrafficId == c.Agent.TrafficId) continue;
                    AddWithin(c, candidates, o, self.SMinMeters + curve.Length - o.SMaxMeters, range);
                }
            }
            // Portee effectivement cherchee : un seul element en amont, jamais plus que la portee declaree.
            float effective = Mathf.Min(range, searched);
            for (int i = 0; i < candidates.Count; i++)
            {
                var f = candidates[i];
                candidates[i] = new VehicleGapFact(f.TrafficId, f.ElementId, f.GapMeters, f.SpeedMetersPerSecond,
                    new ObservationMetadata(f.Metadata.TimestampFrameId, f.Metadata.Source, effective, f.Metadata.Confidence));
            }
            candidates.Sort(CompareGap);
            int total = candidates.Count;
            if (total > 1) candidates.RemoveRange(1, total - 1);
            return new ObservationChannel<VehicleGapFact>(PerceptionStatus.Evaluated, candidates, total, false, effective);
        }

        private static void AddWithin(Context c, List<VehicleGapFact> candidates, ElementOccupant o, float gap, float range)
        {
            if (gap > range) return;
            for (int i = 0; i < candidates.Count; i++)
                if (candidates[i].TrafficId == o.TrafficId)
                {
                    if (gap < candidates[i].GapMeters)
                        candidates[i] = new VehicleGapFact(o.TrafficId, o.ElementId, gap, o.SpeedMetersPerSecond, c.Structured(range, o.Confidence));
                    return;
                }
            candidates.Add(new VehicleGapFact(o.TrafficId, o.ElementId, gap, o.SpeedMetersPerSecond, c.Structured(range, o.Confidence)));
        }

        private static ObservationChannel<AdjacentOccupantFact> Adjacent(Context c)
        {
            var self = c.Self;
            float window = c.Limits.AdjacentWindowMeters;
            var facts = new List<AdjacentOccupantFact>();
            if (self.Kind == RoadElementKind.LaneCorridor)
            {
                var adjacencies = c.Frame.Model.Adjacencies;
                for (int a = 0; a < adjacencies.Count; a++)
                {
                    var adjacency = adjacencies[a];
                    if (adjacency.FromCorridorId != self.ElementId) continue;
                    float from0 = Mathf.Max(self.SMinMeters - window, adjacency.FromStartSMeters);
                    float from1 = Mathf.Min(self.SMaxMeters + window, adjacency.FromEndSMeters);
                    if (from0 > from1) continue;
                    float span = adjacency.FromEndSMeters - adjacency.FromStartSMeters;
                    float scale = span > 0f ? (adjacency.ToEndSMeters - adjacency.ToStartSMeters) / span : 1f;
                    float to0 = adjacency.ToStartSMeters + (from0 - adjacency.FromStartSMeters) * scale;
                    float to1 = adjacency.ToStartSMeters + (from1 - adjacency.FromStartSMeters) * scale;
                    float body0 = adjacency.ToStartSMeters + (self.SMinMeters - adjacency.FromStartSMeters) * scale;
                    float body1 = adjacency.ToStartSMeters + (self.SMaxMeters - adjacency.FromStartSMeters) * scale;
                    c.NoteUnmeasured(adjacency.ToCorridorId);
                    var occupants = c.Frame.GetOccupants(adjacency.ToCorridorId);
                    for (int k = 0; k < occupants.Count; k++)
                    {
                        var o = occupants[k];
                        if (o.TrafficId == c.Agent.TrafficId || o.SMaxMeters < to0 || o.SMinMeters > to1) continue;
                        float gap = o.SMinMeters > body1 ? o.SMinMeters - body1 : o.SMaxMeters < body0 ? o.SMaxMeters - body0 : 0f;
                        AddAdjacent(facts, new AdjacentOccupantFact(o.TrafficId, adjacency.ToCorridorId, adjacency.Side, gap,
                            o.SpeedMetersPerSecond, c.Structured(window, o.Confidence)));
                    }
                }
            }
            return Bounded(c, facts, CompareAdjacent, PerceptionStatus.Evaluated, window);
        }

        private static void AddAdjacent(List<AdjacentOccupantFact> facts, AdjacentOccupantFact fact)
        {
            for (int i = 0; i < facts.Count; i++)
                if (facts[i].TrafficId == fact.TrafficId && facts[i].CorridorId == fact.CorridorId && facts[i].Side == fact.Side)
                {
                    if (CompareAdjacent(fact, facts[i]) < 0) facts[i] = fact;
                    return;
                }
            facts.Add(fact);
        }

        private static int CompareAdjacent(AdjacentOccupantFact x, AdjacentOccupantFact y)
        {
            int order = Mathf.Abs(x.LongitudinalGapMeters).CompareTo(Mathf.Abs(y.LongitudinalGapMeters));
            if (order == 0) order = x.TrafficId.CompareTo(y.TrafficId);
            if (order == 0) order = x.CorridorId.CompareTo(y.CorridorId);
            if (order == 0) order = x.Side.CompareTo(y.Side);
            return order != 0 ? order : x.LongitudinalGapMeters.CompareTo(y.LongitudinalGapMeters);
        }

        private static ObservationChannel<ObstacleFact> Obstacles(Context c, SpatialQueryBuffer buffer)
        {
            var intervals = c.Horizon.Intervals;
            var frame = c.Frame;
            var footprint = c.Agent.Pose.Footprint;
            float lateralRange = c.Limits.LateralRangeMeters;
            float vertical = frame.Model.LocalizationProfile.AcceptanceDistanceMeters;
            float halfWidth = Mathf.Max(footprint.LeftMeters, footprint.RightMeters);
            float front = c.FrontDistance;
            var first = intervals[0];
            float rear = first.StartDistanceMeters + c.Self.SMinMeters - first.StartSMeters;

            var horizonElements = new HashSet<RoadId>();
            var query = new Bounds();
            bool any = false;
            for (int i = 0; i < intervals.Count; i++)
            {
                horizonElements.Add(intervals[i].Id);
                var points = intervals[i].Points;
                for (int p = 0; p < points.Count; p++)
                {
                    if (!any) { query = new Bounds(points[p].Reference.Position, Vector3.zero); any = true; }
                    else query.Encapsulate(points[p].Reference.Position);
                }
            }
            // L'arriere de l'agent deborde en amont du debut de l'horizon : ses coins entrent dans la boite.
            var corners = TrafficFrame.Corners(c.Agent.Pose);
            for (int k = 0; k < corners.Length; k++) query.Encapsulate(corners[k]);
            float grow = halfWidth + lateralRange;
            // Road-up peut etre incline : la tolerance normale doit etre couverte sur chaque axe monde.
            query.Expand(2f * (grow + vertical));

            var facts = new List<ObstacleFact>();
            int total = 0, offset = 0;
            // ponytail: O(n * pages) avec un petit tampon ; curseur d'index si la 5.46 mesure un cout.
            do
            {
                frame.QuerySpatial(query, buffer, offset);
                for (int e = 0; e < buffer.Count; e++)
                {
                    var entry = buffer[e];
                    if (entry.Id == c.Agent.TrafficId) continue;
                    if (entry.IsTrafficActor)
                    {
                        // Un acteur deja structure sur l'horizon n'est pas aussi un obstacle.
                        ElementOccupant occupant;
                        if (frame.TryGetOccupancy(entry.Id, out occupant) && horizonElements.Contains(occupant.ElementId)) continue;
                    }
                    ObstacleFact fact;
                    if (!TryObstacle(c, entry, front, rear, lateralRange, vertical, out fact)) continue;
                    total++;
                    int at = 0;
                    while (at < facts.Count && CompareObstacle(facts[at], fact) <= 0) at++;
                    if (at >= c.Limits.ListCapacity) continue;
                    facts.Insert(at, fact);
                    if (facts.Count > c.Limits.ListCapacity) facts.RemoveAt(facts.Count - 1);
                }
                offset += buffer.Count;
            }
            while (offset < buffer.Total);
            return new ObservationChannel<ObstacleFact>(PerceptionStatus.Evaluated, facts, total,
                buffer.Saturated || total > c.Limits.ListCapacity, c.Horizon.LengthMeters);
        }

        private static int CompareObstacle(ObstacleFact x, ObstacleFact y)
        {
            int order = x.NearDistanceMeters.CompareTo(y.NearDistanceMeters);
            return order != 0 ? order : x.Id.CompareTo(y.Id);
        }

        private static bool TryObstacle(Context c, SpatialEntry entry, float front, float rear,
            float lateralRange, float vertical, out ObstacleFact fact)
        {
            fact = default(ObstacleFact);
            var intervals = c.Horizon.Intervals;
            var model = c.Frame.Model;
            Vector3 center = entry.Bounds.center, extents = entry.Bounds.extents;
            bool found = false;
            for (int i = 0; i < intervals.Count; i++)
            {
                RoadElementKind kind;
                RoadCurve curve;
                IReadOnlyList<RoadCurveSample> samples;
                if (!TrafficFrame.TryGetElement(model, intervals[i].Id, out kind, out curve, out samples)) continue;
                var projection = curve.Project(center, intervals[i].StartSMeters, intervals[i].EndSMeters);
                var interval = intervals[i];
                float along = interval.StartDistanceMeters + projection.SMeters - interval.StartSMeters;
                if (projection.LongitudinalOverrunMeters > 0f)
                    along += projection.SMeters <= interval.StartSMeters + 1e-3f ? -projection.LongitudinalOverrunMeters : projection.LongitudinalOverrunMeters;
                var point = projection.Point;
                float halfLong = HalfExtent(extents, point.Tangent);
                float halfLat = HalfExtent(extents, point.Right);
                float halfUp = HalfExtent(extents, point.Up);
                float intervalRear = i == 0 ? rear : interval.StartDistanceMeters;
                if (along + halfLong < intervalRear || along - halfLong > interval.EndDistanceMeters) continue;
                var footprint = c.Agent.Pose.Footprint;
                float side = projection.LateralOffsetMeters >= 0f ? footprint.RightMeters : footprint.LeftMeters;
                float lateralGap = Mathf.Abs(projection.LateralOffsetMeters) - halfLat - side;
                float verticalGap = Mathf.Abs(projection.NormalOffsetMeters) - halfUp;
                if (lateralGap > lateralRange || verticalGap > vertical) continue;
                // Acteur : min des deux confiances de localisation ; danger : sa confiance declaree.
                float confidence = entry.IsTrafficActor ? Mathf.Min(c.Agent.Location.Confidence, entry.Confidence) : entry.Confidence;
                var candidate = new ObstacleFact(entry.Id, Kind(entry), along - halfLong - front, lateralGap, verticalGap, entry.Velocity,
                    new ObservationMetadata(c.Frame.FrameId, ObservationSource.SpatialQuery, c.Horizon.LengthMeters, confidence));
                if (!found || CompareObstacle(candidate, fact) < 0) fact = candidate;
                found = true;
            }
            return found;
        }

        private static PerceivedObstacleKind Kind(SpatialEntry entry)
        {
            if (entry.IsTrafficActor) return PerceivedObstacleKind.TrafficActor;
            switch (entry.HazardKind)
            {
                case TrafficHazardKind.WalkingPlayer: return PerceivedObstacleKind.WalkingPlayer;
                case TrafficHazardKind.Pedestrian: return PerceivedObstacleKind.Pedestrian;
                case TrafficHazardKind.Vehicle: return PerceivedObstacleKind.Vehicle;
                default: return PerceivedObstacleKind.Obstacle;
            }
        }

        private static float HalfExtent(Vector3 extents, Vector3 axis)
        {
            return Mathf.Abs(extents.x * axis.x) + Mathf.Abs(extents.y * axis.y) + Mathf.Abs(extents.z * axis.z);
        }

        private static ObservationChannel<IntentPathOverlapFact> IntentOverlaps(Context c)
        {
            var model = c.Frame.Model;
            var mine = c.Horizon.Intervals;
            var facts = new List<IntentPathOverlapFact>();
            for (int a = 0; a < c.Frame.Actors.Count; a++)
            {
                var other = c.Frame.Actors[a];
                if (other.TrafficId == c.Agent.TrafficId || other.PublishedHorizon == null) continue;
                var theirs = other.PublishedHorizon.Intervals;
                var meta = new ObservationMetadata(other.PublishedHorizon.SourceFrameId, ObservationSource.PublishedHorizon,
                    c.Horizon.LengthMeters, Mathf.Min(c.Agent.Location.Confidence, other.Location.Confidence));
                float otherStart = 0f;
                for (int j = 0; j < theirs.Count; j++)
                {
                    var b = theirs[j];
                    for (int i = 0; i < mine.Count; i++)
                    {
                        var m = mine[i];
                        if (m.Id == b.ElementId)
                        {
                            float s0 = Mathf.Max(m.StartSMeters, b.StartSMeters), s1 = Mathf.Min(m.EndSMeters, b.EndSMeters);
                            if (s0 <= s1)
                                AddOverlap(facts, new IntentPathOverlapFact(other.TrafficId, IntentOverlapKind.SameElement, m.Id,
                                    m.StartDistanceMeters + s0 - m.StartSMeters, otherStart + s0 - b.StartSMeters, meta));
                        }
                        else if (m.Kind == RoadElementKind.JunctionMovement && b.Kind == RoadElementKind.JunctionMovement)
                        {
                            var zones = model.ConflictZones;
                            for (int z = 0; z < zones.Count; z++)
                            {
                                if (!Contains(zones[z].MemberMovementIds, m.Id) || !Contains(zones[z].MemberMovementIds, b.ElementId))
                                    continue;
                                // Seulement si la zone tombe dans la portion declaree de chacun des deux horizons.
                                float mine0, mine1, their0, their1;
                                if (!ZoneSpan(model, m.Id, zones[z].Volume, m.StartSMeters, m.EndSMeters, out mine0, out mine1)
                                    || !ZoneSpan(model, b.ElementId, zones[z].Volume, b.StartSMeters, b.EndSMeters, out their0, out their1))
                                    continue;
                                AddOverlap(facts, new IntentPathOverlapFact(other.TrafficId, IntentOverlapKind.ConflictZone,
                                    zones[z].Id, m.StartDistanceMeters + mine0 - m.StartSMeters, otherStart + their0 - b.StartSMeters, meta));
                            }
                        }
                    }
                    otherStart += b.EndSMeters - b.StartSMeters;
                }
            }
            return Bounded(c, facts, (x, y) =>
            {
                int order = x.AgentDistanceMeters.CompareTo(y.AgentDistanceMeters);
                if (order == 0) order = x.OtherTrafficId.CompareTo(y.OtherTrafficId);
                if (order == 0) order = x.Kind.CompareTo(y.Kind);
                return order != 0 ? order : x.SharedId.CompareTo(y.SharedId);
            }, PerceptionStatus.Evaluated, c.Horizon.LengthMeters);
        }

        /// <summary>Une seule occurrence par (autre, genre, element ou zone) : la plus proche sur l'horizon de l'agent.</summary>
        private static void AddOverlap(List<IntentPathOverlapFact> facts, IntentPathOverlapFact fact)
        {
            for (int i = 0; i < facts.Count; i++)
                if (facts[i].OtherTrafficId == fact.OtherTrafficId && facts[i].Kind == fact.Kind && facts[i].SharedId == fact.SharedId)
                {
                    if (fact.AgentDistanceMeters < facts[i].AgentDistanceMeters) facts[i] = fact;
                    return;
                }
            facts.Add(fact);
        }

        /// <summary>Plage conservative d'intersection de l'enveloppe avec la zone, dans [from, to] seulement.</summary>
        // ponytail: bornes AABB conservatives, possibles recouvrements en plus ; intersection polygonale si la 5.34 exige de les lever.
        private static bool ZoneSpan(CompiledRoadModel model, RoadId movementId, RoadBoundsBox volume, float from, float to,
            out float s0, out float s1)
        {
            s0 = float.PositiveInfinity; s1 = float.NegativeInfinity;
            CompiledJunctionMovement movement;
            if (!model.TryGetMovement(movementId, out movement)) return false;
            var box = new Bounds(volume.Center, 2f * volume.Extents);
            for (int k = 0; k + 1 < movement.Samples.Count; k++)
            {
                float start = Mathf.Max(from, movement.Samples[k].SMeters);
                float end = Mathf.Min(to, movement.Samples[k + 1].SMeters);
                if (start > end) continue;
                var a = movement.Curve.Sample(start).Position;
                var b = movement.Curve.Sample(end).Position;
                var centerBounds = new Bounds(a, Vector3.zero);
                centerBounds.Encapsulate(b);
                // Reutilise la borne des bords tournants et des largeurs variables de RoadCurve.
                var envelope = movement.Curve.Bounds(start, end);
                var padding = Vector3.Max(centerBounds.min - envelope.min, envelope.max - centerBounds.max);
                var expanded = box;
                expanded.Expand(2f * padding);
                float u0, u1;
                if (!SegmentBoxSpan(a, b, expanded, out u0, out u1)) continue;
                s0 = Mathf.Min(s0, Mathf.Lerp(start, end, u0));
                s1 = Mathf.Max(s1, Mathf.Lerp(start, end, u1));
            }
            return s0 <= s1;
        }

        private static bool SegmentBoxSpan(Vector3 a, Vector3 b, Bounds box, out float u0, out float u1)
        {
            u0 = 0f; u1 = 1f;
            var delta = b - a;
            var min = box.min;
            var max = box.max;
            for (int axis = 0; axis < 3; axis++)
            {
                if (delta[axis] == 0f)
                {
                    if (a[axis] < min[axis] || a[axis] > max[axis]) return false;
                    continue;
                }
                float lo = (min[axis] - a[axis]) / delta[axis];
                float hi = (max[axis] - a[axis]) / delta[axis];
                u0 = Mathf.Max(u0, Mathf.Min(lo, hi));
                u1 = Mathf.Min(u1, Mathf.Max(lo, hi));
                if (u0 > u1) return false;
            }
            return true;
        }

        private static bool Contains(IReadOnlyList<RoadId> ids, RoadId id)
        {
            for (int i = 0; i < ids.Count; i++) if (ids[i] == id) return true;
            return false;
        }

        private static ObservationChannel<ExitOccupancyFact> Exit(Context c)
        {
            var intervals = c.Horizon.Intervals;
            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].Kind != RoadElementKind.JunctionMovement) continue;
                CompiledJunctionMovement movement;
                EffectiveLaneCorridor corridor;
                if (!c.Frame.Model.TryGetMovement(intervals[i].Id, out movement)
                    || !c.Frame.Model.TryGetCorridor(movement.ToCorridorId, out corridor)) continue;
                c.NoteUnmeasured(corridor.CorridorId);
                var occupants = c.Frame.GetOccupants(corridor.CorridorId);
                int count = 0;
                var first = default(ElementOccupant);
                for (int k = 0; k < occupants.Count; k++)
                {
                    if (occupants[k].TrafficId == c.Agent.TrafficId) continue;
                    if (count == 0) first = occupants[k];
                    count++;
                }
                float range = corridor.Curve.Length;
                var fact = new ExitOccupancyFact(movement.Id, corridor.CorridorId, count == 0 ? range : first.SMinMeters, count,
                    count == 0 ? RoadId.None : first.TrafficId,
                    c.Structured(range, count == 0 ? 1f : first.Confidence));
                return new ObservationChannel<ExitOccupancyFact>(PerceptionStatus.Evaluated,
                    new List<ExitOccupancyFact> { fact }, 1, false, range);
            }
            return ObservationChannel<ExitOccupancyFact>.Unavailable(PerceptionStatus.NoMovementAhead);
        }

        private static ObservationChannel<T> Bounded<T>(Context c, List<T> facts, Comparison<T> compare,
            PerceptionStatus status, float range, bool alreadySaturated = false)
        {
            facts.Sort(compare);
            int total = facts.Count;
            int capacity = c.Limits.ListCapacity;
            if (total > capacity) facts.RemoveRange(capacity, total - capacity);
            return new ObservationChannel<T>(status, facts, total, alreadySaturated || total > capacity, range);
        }

        private static int CompareGap(VehicleGapFact a, VehicleGapFact b)
        {
            int order = a.GapMeters.CompareTo(b.GapMeters);
            return order != 0 ? order : a.TrafficId.CompareTo(b.TrafficId);
        }

        private static int CompareUnmeasured(UnmeasuredActorFact a, UnmeasuredActorFact b)
        {
            return a.TrafficId.CompareTo(b.TrafficId);
        }
    }
}
