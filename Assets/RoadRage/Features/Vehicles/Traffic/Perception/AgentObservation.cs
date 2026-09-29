namespace RoadRage.Features.Vehicles.Traffic.Perception
{
    public readonly struct AgentObservation
    {
        public readonly ulong FrameId;
        public readonly RoadId TrafficId;
        public readonly RoadLocation Location;
        public readonly float TangentialSpeedMetersPerSecond;
        public readonly VehicleFootprint Footprint;

        public AgentObservation(ulong frameId, Frame.TrafficActor actor)
        {
            FrameId = frameId;
            TrafficId = actor.TrafficId;
            Location = actor.Location;
            TangentialSpeedMetersPerSecond = actor.TangentialSpeedMetersPerSecond;
            Footprint = actor.Pose.Footprint;
        }
    }
}
