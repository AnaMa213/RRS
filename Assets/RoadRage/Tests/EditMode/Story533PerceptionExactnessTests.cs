using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.33, D15 : le canal obstacles de la perception (rejet prouve par etendue, projection mutualisee dans la frame)
    /// rend exactement les faits, le total et la saturation du canal d'avant D15, recopie ici (chaque entree de la boite de la
    /// route projetee sur chaque etendue). Corpus : les 11 routes 5.31, acteurs de la meme route et des routes croisees,
    /// dangers aleatoires de 0 a 12 m de la route, tampon spatial normal et minuscule (pagination, saturation).
    /// </summary>
    [Category("Core")]
    [Category("Story533")]
    public sealed class Story533PerceptionExactnessTests
    {
        private const float Dt = 0.02f;
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string CampaignPath = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/campaign-5-31.json";
        private const string V2PrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab";

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

        [Serializable]
        private sealed class CampaignTripletRecord
        {
            public string Entry;
            public string Exit;
            public string Via;
            public ulong Seed;
            public ulong InsertionCounter;
        }

        [Serializable]
        private sealed class CampaignFile
        {
            public CampaignTripletRecord[] Triplets;
        }

        // ================================================================== reference : canal obstacles d'avant D15

        private static void ReferenceObstacles(TrafficFrame frame, RoadId agentId, IPathGeometry horizon, PerceptionLimits limits,
            SpatialQueryBuffer buffer, out List<ObstacleFact> facts, out int total, out bool saturated)
        {
            TrafficActor agent;
            frame.TryGetActor(agentId, out agent);
            ElementOccupant self;
            frame.TryGetOccupancy(agentId, out self);
            var intervals = horizon.Spans;
            var footprint = agent.Pose.Footprint;
            float lateralRange = limits.LateralRangeMeters;
            float vertical = frame.Model.LocalizationProfile.AcceptanceDistanceMeters;
            float halfWidth = Mathf.Max(footprint.LeftMeters, footprint.RightMeters);
            var first = intervals[0];
            float front = first.StartDistanceMeters + self.SMaxMeters - first.StartSMeters;
            float rear = first.StartDistanceMeters + self.SMinMeters - first.StartSMeters;
            var horizonElements = new HashSet<RoadId>();
            var query = new Bounds();
            bool any = false;
            for (int i = 0; i < intervals.Count; i++)
            {
                horizonElements.Add(intervals[i].Id);
                for (int p = 0; p < horizon.PointCount(i); p++)
                {
                    if (!any) { query = new Bounds(horizon.PointPosition(i, p), Vector3.zero); any = true; }
                    else query.Encapsulate(horizon.PointPosition(i, p));
                }
            }
            foreach (var corner in TrafficFrame.Corners(agent.Pose)) query.Encapsulate(corner);
            query.Expand(2f * (halfWidth + lateralRange + vertical));
            facts = new List<ObstacleFact>();
            total = 0;
            int offset = 0;
            do
            {
                frame.QuerySpatial(query, buffer, offset);
                for (int e = 0; e < buffer.Count; e++)
                {
                    var entry = buffer[e];
                    if (entry.Id == agentId) continue;
                    if (entry.IsTrafficActor)
                    {
                        ElementOccupant occupant;
                        if (frame.TryGetOccupancy(entry.Id, out occupant) && horizonElements.Contains(occupant.ElementId)) continue;
                    }
                    ObstacleFact fact;
                    if (!ReferenceTryObstacle(frame, agent, horizon, entry, front, rear, lateralRange, vertical, out fact)) continue;
                    total++;
                    int at = 0;
                    while (at < facts.Count && Compare(facts[at], fact) <= 0) at++;
                    if (at >= limits.ListCapacity) continue;
                    facts.Insert(at, fact);
                    if (facts.Count > limits.ListCapacity) facts.RemoveAt(facts.Count - 1);
                }
                offset += buffer.Count;
            }
            while (offset < buffer.Total);
            saturated = buffer.Saturated || total > limits.ListCapacity;
        }

        private static int Compare(ObstacleFact x, ObstacleFact y)
        {
            int order = x.NearDistanceMeters.CompareTo(y.NearDistanceMeters);
            return order != 0 ? order : x.Id.CompareTo(y.Id);
        }

        private static float HalfExtent(Vector3 extents, Vector3 axis)
        {
            return Mathf.Abs(extents.x * axis.x) + Mathf.Abs(extents.y * axis.y) + Mathf.Abs(extents.z * axis.z);
        }

        private static bool ReferenceTryObstacle(TrafficFrame frame, TrafficActor agent, IPathGeometry horizon, SpatialEntry entry,
            float front, float rear, float lateralRange, float vertical, out ObstacleFact fact)
        {
            fact = default(ObstacleFact);
            var intervals = horizon.Spans;
            Vector3 center = entry.Bounds.center, extents = entry.Bounds.extents;
            bool found = false;
            for (int i = 0; i < intervals.Count; i++)
            {
                RoadElementKind kind;
                RoadCurve curve;
                IReadOnlyList<RoadCurveSample> samples;
                if (!TrafficFrame.TryGetElement(frame.Model, intervals[i].Id, out kind, out curve, out samples)) continue;
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
                var footprint = agent.Pose.Footprint;
                float side = projection.LateralOffsetMeters >= 0f ? footprint.RightMeters : footprint.LeftMeters;
                float lateralGap = Mathf.Abs(projection.LateralOffsetMeters) - halfLat - side;
                float verticalGap = Mathf.Abs(projection.NormalOffsetMeters) - halfUp;
                if (lateralGap > lateralRange || verticalGap > vertical) continue;
                float confidence = entry.IsTrafficActor ? Mathf.Min(agent.Location.Confidence, entry.Confidence) : entry.Confidence;
                var kindOf = entry.IsTrafficActor ? PerceivedObstacleKind.TrafficActor
                    : entry.HazardKind == TrafficHazardKind.WalkingPlayer ? PerceivedObstacleKind.WalkingPlayer
                    : entry.HazardKind == TrafficHazardKind.Pedestrian ? PerceivedObstacleKind.Pedestrian
                    : entry.HazardKind == TrafficHazardKind.Vehicle ? PerceivedObstacleKind.Vehicle : PerceivedObstacleKind.Obstacle;
                var candidate = new ObstacleFact(entry.Id, kindOf, along - halfLong - front, lateralGap, verticalGap, entry.Velocity,
                    new ObservationMetadata(frame.FrameId, ObservationSource.SpatialQuery, horizon.LengthMeters, confidence));
                if (!found || Compare(candidate, fact) < 0) fact = candidate;
                found = true;
            }
            return found;
        }

        private static bool Bits(float a, float b) { return BitConverter.SingleToInt32Bits(a) == BitConverter.SingleToInt32Bits(b); }

        private static bool Same(ObstacleFact a, ObstacleFact b)
        {
            return a.Id == b.Id && a.Kind == b.Kind && Bits(a.NearDistanceMeters, b.NearDistanceMeters) && Bits(a.LateralGapMeters, b.LateralGapMeters)
                && Bits(a.VerticalGapMeters, b.VerticalGapMeters) && Bits(a.Velocity.x, b.Velocity.x) && Bits(a.Velocity.y, b.Velocity.y)
                && Bits(a.Velocity.z, b.Velocity.z) && a.Metadata.TimestampFrameId == b.Metadata.TimestampFrameId
                && a.Metadata.Source == b.Metadata.Source && Bits(a.Metadata.RangeMeters, b.Metadata.RangeMeters)
                && Bits(a.Metadata.Confidence, b.Metadata.Confidence);
        }

        // ================================================================== corpus

        [Test]
        public void TheObstacleChannelEqualsThePreD15ChannelToTheBitWithActorsAndHazardsAroundEveryRoute()
        {
            var model = Admission.Model;
            var driver = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile;
            var car = TrafficV2VehicleDriver.FootprintOf(AssetDatabase.LoadAssetAtPath<GameObject>(V2PrefabPath).GetComponent<BoxCollider>());
            var campaign = JsonUtility.FromJson<CampaignFile>(File.ReadAllText(CampaignPath));
            var lanes = new List<KeyValuePair<TrafficV2Insertion, ReferenceTrack>>();
            foreach (var t in campaign.Triplets)
            {
                var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, RoadId.Parse(t.Entry), RoadId.Parse(t.Exit), t.Seed,
                    t.InsertionCounter, driver, Dt, string.IsNullOrEmpty(t.Via) ? RoadId.None : RoadId.Parse(t.Via));
                if (insertion.Code == TrafficV2Code.Allowed)
                    lanes.Add(new KeyValuePair<TrafficV2Insertion, ReferenceTrack>(insertion, ReferenceTrack.FromRoute(model, insertion.Route.Occurrences, 0f)));
            }
            var random = new System.Random(533153);
            var normal = new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity);
            var tiny = new SpatialQueryBuffer(3);
            int observations = 0, facts = 0, saturations = 0, mismatches = 0;
            string firstMismatch = null;
            ulong frameId = 100;
            for (int l = 0; l < lanes.Count; l++)
            {
                var insertion = lanes[l].Key;
                var track = lanes[l].Value;
                var route = insertion.Route;
                for (float d = 2f; d < track.LengthMeters - 2f; d += 7f)
                {
                    int piece = track.PieceAt(d);
                    var inputs = new List<TrafficActorInput> { Actor(insertion.TrafficId, track, d, car, insertion.Route) };
                    // Meme route devant et derriere, puis les vehicules des autres routes les plus proches (trafic croise).
                    ulong next = 1;
                    foreach (float ahead in new[] { 9f, 21f, -8f })
                        if (d + ahead > 0f && d + ahead < track.LengthMeters)
                            inputs.Add(Actor(new RoadId(0x5330E, next++), track, d + ahead, car, null));
                    var here = track.Nominal(piece, d).Position;
                    for (int o = 0; o < lanes.Count; o++)
                    {
                        if (o == l) continue;
                        var other = lanes[o].Value;
                        float best = float.PositiveInfinity, at = -1f;
                        for (float x = 0f; x < other.LengthMeters; x += 2f)
                        {
                            float dist = (other.Nominal(other.PieceAt(x), x).Position - here).sqrMagnitude;
                            if (dist < best) { best = dist; at = x; }
                        }
                        if (best < 45f * 45f && best > 6f * 6f) inputs.Add(Actor(new RoadId(0x5330E, next++), other, at, car, null));
                    }
                    var hazards = new List<TrafficHazardInput>();
                    for (int h = 0; h < 14; h++)
                    {
                        float s = Mathf.Clamp(d + (float)(random.NextDouble() * 70.0 - 6.0), 0f, track.LengthMeters - 0.01f);
                        var nominal = track.Nominal(track.PieceAt(s), s);
                        var right = Vector3.Cross(nominal.Up, nominal.Forward).normalized;
                        var center = nominal.Position + right * (float)(random.NextDouble() * 24.0 - 12.0)
                            + nominal.Up * (float)(random.NextDouble() * 6.0 - 3.0);
                        var extents = new Vector3((float)(0.1 + random.NextDouble() * 2.4), (float)(0.1 + random.NextDouble() * 2.4),
                            (float)(0.1 + random.NextDouble() * 2.4));
                        hazards.Add(new TrafficHazardInput(new RoadId(TrafficV2HazardCollector.HazardIdentityDomain, (ulong)(h + 1)),
                            (TrafficHazardKind)(h % 4), new RoadBoundsBox { Center = center, Extents = extents }, Vector3.right * h, 1f));
                    }
                    TrafficFrame frame;
                    try { frame = new TrafficFrame(++frameId, model, inputs, hazards); }
                    catch (ArgumentException) { continue; }
                    float speed = 4f;
                    PlanningDecision decision;
                    try
                    {
                        decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, route, insertion.ExitPortalId,
                            insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared, null,
                            null, Admission.Evidence, RoadId.None, track.OffsetRadians(piece, d),
                            PlanningReach.For(driver, speed, Math.Max(speed * Dt, MotionCommand.PreviewFloorMeters), TrafficV2Settings.PlanningReachMarginMeters)));
                    }
                    catch (ArgumentException) { continue; }
                    if (decision.Route.Plan != null) route = decision.Route.Plan;
                    if (decision.PerceptionPath == null) continue;
                    foreach (var buffer in new[] { normal, tiny })
                    {
                        var observation = TrafficPerception.Observe(frame, insertion.TrafficId, decision.PerceptionPath,
                            TrafficV2Settings.PerceptionLimits, buffer);
                        if (observation.Obstacles.Status != PerceptionStatus.Evaluated) continue;
                        List<ObstacleFact> expected;
                        int total;
                        bool saturated;
                        var reference = new SpatialQueryBuffer(buffer.Capacity);
                        ReferenceObstacles(frame, insertion.TrafficId, decision.PerceptionPath, TrafficV2Settings.PerceptionLimits,
                            reference, out expected, out total, out saturated);
                        observations++;
                        facts += expected.Count;
                        if (saturated) saturations++;
                        var actual = observation.Obstacles;
                        bool same = actual.Total == total && actual.Saturated == saturated && actual.Items.Count == expected.Count
                            && observation.SpatialQueryTotal == reference.Total && observation.SpatialQuerySaturated == reference.Saturated;
                        for (int k = 0; same && k < expected.Count; k++) same = Same(actual.Items[k], expected[k]);
                        if (!same)
                        {
                            mismatches++;
                            if (firstMismatch == null)
                                firstMismatch = "route " + l + " d " + d + " capacite " + buffer.Capacity + " : total " + actual.Total + "/" + total
                                    + ", faits " + actual.Items.Count + "/" + expected.Count + ", saturation " + actual.Saturated + "/" + saturated;
                        }
                    }
                }
            }
            Debug.Log("[Story533] canal obstacles contre reference d'avant D15 : " + observations + " observations, " + facts
                + " faits, " + saturations + " saturees ; " + mismatches + " difference(s).");
            Assert.That(observations, Is.GreaterThan(200), "corpus trop petit");
            Assert.That(facts, Is.GreaterThan(200), "trop peu de faits pour juger la borne de rejet");
            Assert.That(mismatches, Is.EqualTo(0), firstMismatch);
        }

        private static TrafficActorInput Actor(RoadId id, ReferenceTrack track, float distance, VehicleFootprint car,
            Features.Vehicles.Traffic.Routing.RoutePlan route)
        {
            int piece = track.PieceAt(distance);
            var nominal = track.Nominal(piece, distance);
            return new TrafficActorInput(id, new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward,
                Up = nominal.Up, Footprint = car }, 4f, track.Pieces[piece].Id, route == null ? null : TrafficV2Lifecycle.ExpectedElements(route),
                track.KinematicAnchors(piece));
        }
    }
}
