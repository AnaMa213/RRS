using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace RoadRage.Shared.Presentation
{
    public static class ThirdPersonCameraConfiguration
    {
        public static void Configure(CinemachineCamera camera, Transform target, float distance,
            float targetHeight, float shoulderOffset, bool recenter)
        {
            camera.Follow = target;
            camera.LookAt = target;
            var orbit = camera.GetComponent<CinemachineOrbitalFollow>();
            if (orbit == null) orbit = camera.gameObject.AddComponent<CinemachineOrbitalFollow>();
            orbit.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbit.Radius = distance;
            orbit.TargetOffset = Vector3.up * targetHeight;
            orbit.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            orbit.TrackerSettings.PositionDamping = new Vector3(0.15f, 0.2f, 0.15f);
            orbit.HorizontalAxis.Value = target.eulerAngles.y;
            orbit.HorizontalAxis.Recentering = new InputAxis.RecenteringSettings
                { Enabled = recenter, Wait = 1.5f, Time = 2f };
            orbit.RecenteringTarget = CinemachineOrbitalFollow.ReferenceFrames.TrackingTarget;
            orbit.VerticalAxis.Range = new Vector2(-15f, 65f);
            orbit.VerticalAxis.Value = orbit.VerticalAxis.Center = 15f;
            orbit.VerticalAxis.Recentering = orbit.HorizontalAxis.Recentering;
            orbit.RadialAxis.Value = 1f;

            var aim = camera.GetComponent<CinemachineRotationComposer>();
            if (aim == null) aim = camera.gameObject.AddComponent<CinemachineRotationComposer>();
            aim.TargetOffset = Vector3.up * targetHeight;
            aim.Damping = new Vector2(0.1f, 0.1f);
            aim.Composition.ScreenPosition = new Vector2(shoulderOffset, 0f);

            var collision = camera.GetComponent<CinemachineDeoccluder>();
            if (collision == null) collision = camera.gameObject.AddComponent<CinemachineDeoccluder>();
            collision.CollideAgainst = Physics.DefaultRaycastLayers;
            collision.IgnoreTag = string.Empty;
            collision.AvoidObstacles.Enabled = true;
            collision.AvoidObstacles.Strategy = CinemachineDeoccluder.ObstacleAvoidance.ResolutionStrategy.PullCameraForward;
            collision.AvoidObstacles.UseFollowTarget.Enabled = true;
            collision.AvoidObstacles.UseFollowTarget.YOffset = targetHeight;
            collision.AvoidObstacles.CameraRadius = 0.2f;
            collision.AvoidObstacles.Damping = 0.3f;
            collision.AvoidObstacles.DampingWhenOccluded = 0f;
        }
    }
}
