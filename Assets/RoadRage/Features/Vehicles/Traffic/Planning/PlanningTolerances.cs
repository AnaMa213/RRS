namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    public static class PlanningTolerances
    {
        public const float SeamCurvatureJumpPerMeter = 0.001f;
        public const float MaximumCurvatureSlopePerSquareMeter = 0.30f;
        public const float RoundaboutCurvaturePerMeter = 1f / 6f;
    }
}
