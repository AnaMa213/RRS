using System;
using System.Collections.Generic;

namespace RoadRage.Features.Vehicles.Traffic.Frame
{
    public readonly struct TrafficActorInput
    {
        public readonly RoadId TrafficId;
        public readonly VehicleFootprintPose Pose;
        public readonly float TangentialSpeedMetersPerSecond;
        public readonly RoadId PreviousElementId;

        public TrafficActorInput(RoadId trafficId, VehicleFootprintPose pose, float tangentialSpeedMetersPerSecond,
            RoadId previousElementId)
        {
            TrafficId = trafficId;
            Pose = pose;
            TangentialSpeedMetersPerSecond = tangentialSpeedMetersPerSecond;
            PreviousElementId = previousElementId;
        }
    }

    public readonly struct TrafficActor
    {
        public readonly RoadId TrafficId;
        public readonly VehicleFootprintPose Pose;
        public readonly float TangentialSpeedMetersPerSecond;
        public readonly RoadId PreviousElementId;
        public readonly RoadLocation Location;

        internal TrafficActor(TrafficActorInput input, RoadLocation location)
        {
            TrafficId = input.TrafficId;
            Pose = input.Pose;
            TangentialSpeedMetersPerSecond = input.TangentialSpeedMetersPerSecond;
            PreviousElementId = input.PreviousElementId;
            Location = location;
        }
    }

    /// <summary>One host snapshot. Actor order and localization are fixed at construction.</summary>
    public sealed class TrafficFrame
    {
        public ulong FrameId { get; }
        public RoadId ModelId { get { return Model.ModelId; } }
        public RoadModelVersion Version { get { return Model.Version; } }
        public CompiledRoadModel Model { get; }
        public IReadOnlyList<TrafficActor> Actors { get; }

        public TrafficFrame(ulong frameId, CompiledRoadModel model, IReadOnlyList<TrafficActorInput> inputs)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (inputs == null) throw new ArgumentNullException("inputs");
            FrameId = frameId;
            Model = model;
            var ordered = new List<TrafficActorInput>(inputs.Count);
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].TrafficId.IsEmpty) throw new ArgumentException("EmptyTrafficId", "inputs");
                if (float.IsNaN(inputs[i].TangentialSpeedMetersPerSecond)
                    || float.IsInfinity(inputs[i].TangentialSpeedMetersPerSecond)
                    || inputs[i].TangentialSpeedMetersPerSecond < 0f)
                    throw new ArgumentException("InvalidSpeed", "inputs");
                ordered.Add(inputs[i]);
            }
            ordered.Sort((a, b) => a.TrafficId.CompareTo(b.TrafficId));
            var actors = new TrafficActor[ordered.Count];
            for (int i = 0; i < ordered.Count; i++)
            {
                if (i > 0 && ordered[i].TrafficId == ordered[i - 1].TrafficId)
                    throw new ArgumentException("DuplicateTrafficId", "inputs");
                actors[i] = new TrafficActor(ordered[i],
                    RoadLocalizer.Localize(model, ordered[i].Pose, ordered[i].PreviousElementId, null));
            }
            Actors = Array.AsReadOnly(actors);
        }

        public bool TryGetActor(RoadId id, out TrafficActor actor)
        {
            int low = 0, high = Actors.Count - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                int order = Actors[mid].TrafficId.CompareTo(id);
                if (order == 0) { actor = Actors[mid]; return true; }
                if (order < 0) low = mid + 1; else high = mid - 1;
            }
            actor = default(TrafficActor);
            return false;
        }
    }
}
