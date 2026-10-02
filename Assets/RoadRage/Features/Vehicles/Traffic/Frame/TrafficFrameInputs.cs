using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Frame
{
    /// <summary>Genre d'un danger non structure. <c>Vehicle</c> : un vehicule qui n'est pas un acteur de trafic.</summary>
    public enum TrafficHazardKind
    {
        WalkingPlayer = 0,
        Pedestrian = 1,
        Obstacle = 2,
        Vehicle = 3
    }

    /// <summary>Pourquoi un acteur n'a pas d'occupation structuree (Story 5.32).</summary>
    public enum OccupancyExclusion
    {
        None = 0,
        NotLocalized = 1,
        UndeclaredFootprint = 2,
        OccupancyNotBounded = 3
    }

    /// <summary>Portion de route d'un horizon d'intention publie : element et abscisses [s0, s1].</summary>
    public readonly struct IntentInterval
    {
        public readonly RoadElementKind Kind;
        public readonly RoadId ElementId;
        public readonly float StartSMeters;
        public readonly float EndSMeters;

        public IntentInterval(RoadElementKind kind, RoadId elementId, float startSMeters, float endSMeters)
        {
            Kind = kind; ElementId = elementId; StartSMeters = startSMeters; EndSMeters = endSMeters;
        }
    }

    /// <summary>
    /// Horizon d'intention publie par un acteur a une frame anterieure. Intervalles spatiaux seulement :
    /// aucune fenetre temporelle, donc aucune presence future datee.
    /// </summary>
    public sealed class PublishedIntentHorizon
    {
        public ulong SourceFrameId { get; }
        public IReadOnlyList<IntentInterval> Intervals { get; }

        public PublishedIntentHorizon(ulong sourceFrameId, IReadOnlyList<IntentInterval> intervals)
        {
            if (intervals == null) throw new ArgumentNullException("intervals");
            SourceFrameId = sourceFrameId;
            var copy = new IntentInterval[intervals.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = intervals[i];
            Intervals = Array.AsReadOnly(copy);
        }
    }

    /// <summary>Danger non structure deja mesure par l'hote : boite alignee monde, vitesse et confiance dans [0, 1].</summary>
    public readonly struct TrafficHazardInput
    {
        public readonly RoadId Id;
        public readonly TrafficHazardKind Kind;
        public readonly RoadBoundsBox Bounds;
        public readonly Vector3 Velocity;
        public readonly float Confidence;

        public TrafficHazardInput(RoadId id, TrafficHazardKind kind, RoadBoundsBox bounds, Vector3 velocity, float confidence)
        {
            Id = id; Kind = kind; Bounds = bounds; Velocity = velocity; Confidence = confidence;
        }
    }

    /// <summary>Phase courante d'un plan de signal, tenue par le runtime hote.</summary>
    public readonly struct SignalPhaseInput
    {
        public readonly RoadId PlanId;
        public readonly RoadId PhaseId;

        public SignalPhaseInput(RoadId planId, RoadId phaseId)
        {
            PlanId = planId; PhaseId = phaseId;
        }
    }

    /// <summary>Fermeture d'un corridor ou d'un mouvement, avec un code de raison stable.</summary>
    public readonly struct ElementClosureInput
    {
        public readonly RoadId ElementId;
        public readonly string ReasonCode;

        public ElementClosureInput(RoadId elementId, string reasonCode)
        {
            ElementId = elementId; ReasonCode = reasonCode;
        }
    }

    /// <summary>Occupation conservatrice d'un acteur sur son element retenu : [sMin, sMax] couvre tout le rectangle.</summary>
    public readonly struct ElementOccupant
    {
        public readonly RoadId TrafficId;
        public readonly RoadElementKind Kind;
        public readonly RoadId ElementId;
        /// <summary>Abscisse du point de reference.</summary>
        public readonly float SMeters;
        public readonly float SMinMeters;
        public readonly float SMaxMeters;
        /// <summary>Reste de subdivision ajoute de chaque cote.</summary>
        public readonly float RemainderMeters;
        public readonly float SpeedMetersPerSecond;
        public readonly float Confidence;

        internal ElementOccupant(RoadId trafficId, RoadElementKind kind, RoadId elementId, float s, float sMin, float sMax,
            float remainder, float speed, float confidence)
        {
            TrafficId = trafficId; Kind = kind; ElementId = elementId; SMeters = s; SMinMeters = sMin; SMaxMeters = sMax;
            RemainderMeters = remainder; SpeedMetersPerSecond = speed; Confidence = confidence;
        }
    }

    /// <summary>Entree de l'index spatial : un danger, ou un acteur a empreinte declaree.</summary>
    public readonly struct SpatialEntry
    {
        public readonly RoadId Id;
        public readonly bool IsTrafficActor;
        /// <summary>Sans objet pour un acteur.</summary>
        public readonly TrafficHazardKind HazardKind;
        public readonly Bounds Bounds;
        public readonly Vector3 Velocity;
        public readonly float Confidence;

        internal SpatialEntry(RoadId id, bool isTrafficActor, TrafficHazardKind hazardKind, Bounds bounds, Vector3 velocity,
            float confidence)
        {
            Id = id; IsTrafficActor = isTrafficActor; HazardKind = hazardKind; Bounds = bounds; Velocity = velocity;
            Confidence = confidence;
        }
    }

    /// <summary>
    /// Tampon de requete spatiale a capacite fixe, possede par l'appelant. Une requete n'alloue pas ; au-dela
    /// de la capacite, elle compte le total et leve <see cref="Saturated"/> au lieu de tronquer en silence.
    /// </summary>
    public sealed class SpatialQueryBuffer
    {
        private readonly SpatialEntry[] _entries;

        public SpatialQueryBuffer(int capacity)
        {
            if (capacity < 1) throw new ArgumentException("InvalidCapacity", "capacity");
            _entries = new SpatialEntry[capacity];
        }

        public int Capacity { get { return _entries.Length; } }
        public int Count { get; private set; }
        public int Total { get; private set; }
        public bool Saturated { get { return Total > _entries.Length; } }
        public SpatialEntry this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException("index");
                return _entries[index];
            }
        }

        internal void Clear() { Count = 0; Total = 0; }

        internal void Add(SpatialEntry entry, int offset)
        {
            Total++;
            if (Total > offset && Count < _entries.Length) _entries[Count++] = entry;
        }
    }
}
