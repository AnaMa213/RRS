using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Frame;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Perception
{
    public enum ObservationSource
    {
        StructuredOccupancy = 0,
        SpatialQuery = 1,
        PublishedHorizon = 2
    }

    /// <summary>Etat d'un canal de perception. Seul <c>Evaluated</c> porte des faits.</summary>
    public enum PerceptionStatus
    {
        Evaluated = 0,
        UndeclaredFootprint = 1,
        AgentOccupancyUnavailable = 2,
        NoMovementAhead = 3
    }

    public enum PerceivedObstacleKind
    {
        WalkingPlayer = 0,
        Pedestrian = 1,
        Obstacle = 2,
        Vehicle = 3,
        TrafficActor = 4
    }

    public enum IntentOverlapKind
    {
        SameElement = 0,
        ConflictZone = 1
    }

    /// <summary>Date (frame de mesure, ou frame source d'un horizon publie), source, portee cherchee et confiance [0, 1].</summary>
    public readonly struct ObservationMetadata
    {
        public readonly ulong TimestampFrameId;
        public readonly ObservationSource Source;
        public readonly float RangeMeters;
        public readonly float Confidence;

        public ObservationMetadata(ulong timestampFrameId, ObservationSource source, float rangeMeters, float confidence)
        {
            TimestampFrameId = timestampFrameId; Source = source; RangeMeters = rangeMeters; Confidence = confidence;
        }
    }

    /// <summary>Vehicule devant ou derriere : jeu d'arc pare-chocs a pare-chocs entre occupations conservatrices.</summary>
    public readonly struct VehicleGapFact
    {
        public readonly RoadId TrafficId;
        public readonly RoadId ElementId;
        /// <summary>Negatif en cas de chevauchement.</summary>
        public readonly float GapMeters;
        public readonly float SpeedMetersPerSecond;
        public readonly ObservationMetadata Metadata;

        public VehicleGapFact(RoadId trafficId, RoadId elementId, float gapMeters, float speed, ObservationMetadata metadata)
        {
            TrafficId = trafficId; ElementId = elementId; GapMeters = gapMeters; SpeedMetersPerSecond = speed; Metadata = metadata;
        }
    }

    /// <summary>Occupant d'une voie adjacente ; ecart longitudinal signe (positif devant, 0 a hauteur).</summary>
    public readonly struct AdjacentOccupantFact
    {
        public readonly RoadId TrafficId;
        public readonly RoadId CorridorId;
        public readonly LaneSide Side;
        public readonly float LongitudinalGapMeters;
        public readonly float SpeedMetersPerSecond;
        public readonly ObservationMetadata Metadata;

        public AdjacentOccupantFact(RoadId trafficId, RoadId corridorId, LaneSide side, float gap, float speed,
            ObservationMetadata metadata)
        {
            TrafficId = trafficId; CorridorId = corridorId; Side = side; LongitudinalGapMeters = gap;
            SpeedMetersPerSecond = speed; Metadata = metadata;
        }
    }

    /// <summary>Objet non structure pres du couloir balaye de l'horizon. Ecart lateral ≤ 0 : dans le couloir.</summary>
    public readonly struct ObstacleFact
    {
        public readonly RoadId Id;
        public readonly PerceivedObstacleKind Kind;
        /// <summary>Distance d'arc du pare-chocs avant de l'agent a la face proche.</summary>
        public readonly float NearDistanceMeters;
        public readonly float LateralGapMeters;
        public readonly float VerticalGapMeters;
        public readonly Vector3 Velocity;
        public readonly ObservationMetadata Metadata;
        public bool InSweptPath { get { return LateralGapMeters <= 0f; } }

        public ObstacleFact(RoadId id, PerceivedObstacleKind kind, float nearDistance, float lateralGap, float verticalGap,
            Vector3 velocity, ObservationMetadata metadata)
        {
            Id = id; Kind = kind; NearDistanceMeters = nearDistance; LateralGapMeters = lateralGap;
            VerticalGapMeters = verticalGap; Velocity = velocity; Metadata = metadata;
        }
    }

    /// <summary>
    /// Fait spatial : l'horizon de l'agent et l'horizon publie d'un autre acteur utilisent des portions de route qui
    /// peuvent se recouvrir. Ne predit ni collision ni conflit temporel : aucun horizon ne porte de fenetre temporelle.
    /// </summary>
    public readonly struct IntentPathOverlapFact
    {
        public readonly RoadId OtherTrafficId;
        public readonly IntentOverlapKind Kind;
        /// <summary>Element partage, ou zone de conflit.</summary>
        public readonly RoadId SharedId;
        public readonly float AgentDistanceMeters;
        public readonly float OtherDistanceMeters;
        public readonly ObservationMetadata Metadata;

        public IntentPathOverlapFact(RoadId other, IntentOverlapKind kind, RoadId sharedId, float agentDistance,
            float otherDistance, ObservationMetadata metadata)
        {
            OtherTrafficId = other; Kind = kind; SharedId = sharedId; AgentDistanceMeters = agentDistance;
            OtherDistanceMeters = otherDistance; Metadata = metadata;
        }
    }

    /// <summary>Corridor de sortie du prochain mouvement : longueur libre jusqu'a l'arriere du premier occupant.</summary>
    public readonly struct ExitOccupancyFact
    {
        public readonly RoadId MovementId;
        public readonly RoadId ExitCorridorId;
        /// <summary>Longueur du corridor sans occupant ; negative si le premier occupant deborde en amont.</summary>
        public readonly float FreeLengthMeters;
        public readonly int OccupantCount;
        public readonly RoadId FirstOccupantId;
        public readonly ObservationMetadata Metadata;

        public ExitOccupancyFact(RoadId movementId, RoadId exitCorridorId, float freeLength, int occupantCount,
            RoadId firstOccupantId, ObservationMetadata metadata)
        {
            MovementId = movementId; ExitCorridorId = exitCorridorId; FreeLengthMeters = freeLength;
            OccupantCount = occupantCount; FirstOccupantId = firstOccupantId; Metadata = metadata;
        }
    }

    /// <summary>Acteur localise sur un element cherche mais sans occupation : aucune distance physique.</summary>
    public readonly struct UnmeasuredActorFact
    {
        public readonly RoadId TrafficId;
        public readonly RoadId ElementId;
        public readonly float SMeters;
        public readonly OccupancyExclusion Reason;

        public UnmeasuredActorFact(RoadId trafficId, RoadId elementId, float s, OccupancyExclusion reason)
        {
            TrafficId = trafficId; ElementId = elementId; SMeters = s; Reason = reason;
        }
    }

    /// <summary>Canal borne : faits gardes (les plus proches), total des candidats et saturation explicite.</summary>
    public sealed class ObservationChannel<T>
    {
        public PerceptionStatus Status { get; }
        public IReadOnlyList<T> Items { get; }
        public int Total { get; }
        public bool Saturated { get; }
        public float RangeMeters { get; }

        internal ObservationChannel(PerceptionStatus status, List<T> items, int total, bool saturated, float range)
        {
            Status = status;
            Items = Array.AsReadOnly(items == null ? new T[0] : items.ToArray());
            Total = total; Saturated = saturated; RangeMeters = range;
        }

        internal static ObservationChannel<T> Unavailable(PerceptionStatus status)
        {
            return new ObservationChannel<T>(status, null, 0, false, 0f);
        }
    }

    public readonly struct AgentObservation
    {
        public readonly ulong FrameId;
        public readonly RoadId TrafficId;
        public readonly RoadLocation Location;
        /// <summary>Vitesse signee le long de l'avant de l'empreinte. Aucun consommateur de plan ne la lit comme vitesse de progression.</summary>
        public readonly float TangentialSpeedMetersPerSecond;
        public readonly VehicleFootprint Footprint;

        /// <summary>Faux : observation minimale 5.30, aucun canal evalue (canaux nuls).</summary>
        public readonly bool Perceived;
        public readonly ObservationChannel<VehicleGapFact> Leader;
        public readonly ObservationChannel<VehicleGapFact> Follower;
        public readonly ObservationChannel<AdjacentOccupantFact> Adjacent;
        public readonly ObservationChannel<ObstacleFact> Obstacles;
        public readonly ObservationChannel<IntentPathOverlapFact> IntentOverlaps;
        public readonly ObservationChannel<ExitOccupancyFact> Exit;
        public readonly ObservationChannel<UnmeasuredActorFact> UnmeasuredActors;
        /// <summary>Total et saturation du tampon de requete spatiale.</summary>
        public readonly int SpatialQueryTotal;
        public readonly bool SpatialQuerySaturated;

        public AgentObservation(ulong frameId, TrafficActor actor)
        {
            FrameId = frameId;
            TrafficId = actor.TrafficId;
            Location = actor.Location;
            TangentialSpeedMetersPerSecond = actor.TangentialSpeedMetersPerSecond;
            Footprint = actor.Pose.Footprint;
            Perceived = false;
            Leader = null; Follower = null; Adjacent = null; Obstacles = null; IntentOverlaps = null; Exit = null;
            UnmeasuredActors = null; SpatialQueryTotal = 0; SpatialQuerySaturated = false;
        }

        internal AgentObservation(ulong frameId, TrafficActor actor,
            ObservationChannel<VehicleGapFact> leader, ObservationChannel<VehicleGapFact> follower,
            ObservationChannel<AdjacentOccupantFact> adjacent, ObservationChannel<ObstacleFact> obstacles,
            ObservationChannel<IntentPathOverlapFact> overlaps, ObservationChannel<ExitOccupancyFact> exit,
            ObservationChannel<UnmeasuredActorFact> unmeasured, int spatialTotal, bool spatialSaturated)
            : this(frameId, actor)
        {
            Perceived = true;
            Leader = leader; Follower = follower; Adjacent = adjacent; Obstacles = obstacles; IntentOverlaps = overlaps;
            Exit = exit; UnmeasuredActors = unmeasured; SpatialQueryTotal = spatialTotal;
            SpatialQuerySaturated = spatialSaturated;
        }

        /// <summary>Texte deterministe et invariant de culture, indexe sur la frame.</summary>
        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Frame ").Append(FrameId.ToString(CultureInfo.InvariantCulture))
                .Append(" / Vehicule ").Append(TrafficId).Append('\n');
            if (!Perceived) return text.Append("Perception non evaluee\n").ToString();
            Append(text, "Leader", Leader, f => f.TrafficId + " " + f.ElementId + " jeu " + F(f.GapMeters) + " v " + F(f.SpeedMetersPerSecond) + Meta(f.Metadata));
            Append(text, "Suiveur", Follower, f => f.TrafficId + " " + f.ElementId + " jeu " + F(f.GapMeters) + " v " + F(f.SpeedMetersPerSecond) + Meta(f.Metadata));
            Append(text, "Adjacent", Adjacent, f => f.TrafficId + " " + f.CorridorId + " " + f.Side + " ecart " + F(f.LongitudinalGapMeters) + Meta(f.Metadata));
            Append(text, "Obstacles", Obstacles, f => f.Id + " " + f.Kind + " d " + F(f.NearDistanceMeters) + " lat " + F(f.LateralGapMeters) + " vert " + F(f.VerticalGapMeters) + Meta(f.Metadata));
            Append(text, "Recouvrements d'intention", IntentOverlaps, f => f.OtherTrafficId + " " + f.Kind + " " + f.SharedId + " agent " + F(f.AgentDistanceMeters) + " autre " + F(f.OtherDistanceMeters) + Meta(f.Metadata));
            Append(text, "Sortie", Exit, f => f.MovementId + " -> " + f.ExitCorridorId + " libre " + F(f.FreeLengthMeters) + " occupants " + f.OccupantCount.ToString(CultureInfo.InvariantCulture) + " premier " + f.FirstOccupantId + Meta(f.Metadata));
            Append(text, "Non mesures", UnmeasuredActors, f => f.TrafficId + " " + f.ElementId + " s " + F(f.SMeters) + " " + f.Reason);
            text.Append("Requete spatiale ").Append(SpatialQueryTotal.ToString(CultureInfo.InvariantCulture))
                .Append(SpatialQuerySaturated ? " saturee\n" : "\n");
            return text.ToString();
        }

        private static void Append<T>(StringBuilder text, string label, ObservationChannel<T> channel, Func<T, string> line)
        {
            text.Append(label).Append(' ').Append(channel.Status).Append(" total ")
                .Append(channel.Total.ToString(CultureInfo.InvariantCulture)).Append(" portee ").Append(F(channel.RangeMeters))
                .Append(channel.Saturated ? " sature" : "").Append('\n');
            for (int i = 0; i < channel.Items.Count; i++) text.Append("  ").Append(line(channel.Items[i])).Append('\n');
        }

        private static string Meta(ObservationMetadata m)
        {
            return " [" + m.Source + " @" + m.TimestampFrameId.ToString(CultureInfo.InvariantCulture) + " portee "
                + F(m.RangeMeters) + " conf " + F(m.Confidence) + "]";
        }

        private static string F(float value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture);
        }
    }
}
