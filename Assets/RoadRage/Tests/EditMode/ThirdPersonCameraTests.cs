using NUnit.Framework;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Presentation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.Tests.EditMode
{
    public sealed class ThirdPersonCameraTests
    {
        [Test]
        public void MouseActionDrivesNativeOrbitWithoutFrameScaling()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab");
            var vehicle = Object.Instantiate(prefab);
            try
            {
                vehicle.GetComponent<LocalVehicleCameraRig>().SetLocalSoloCameraActive(true);
                var orbit = vehicle.GetComponentInChildren<CinemachineOrbitalFollow>();
                var input = orbit.GetComponent<CinemachineInputAxisController>();
                var action = input.Controllers[0].Input.InputAction.action;
                var hasPointerDeltaBinding = false;
                foreach (var binding in action.bindings)
                {
                    if (binding.effectivePath == "<Pointer>/delta")
                    {
                        hasPointerDeltaBinding = true;
                        break;
                    }
                }

                Assert.That(hasPointerDeltaBinding, Is.True, "L'orbite doit rester liee au delta natif de la souris.");
                input.ReadControlValueOverride = (_, hint, _, _) =>
                    hint == IInputAxisOwner.AxisDescriptor.Hints.Y ? 20f : 100f;
                var yaw = orbit.HorizontalAxis.Value;
                var pitch = orbit.VerticalAxis.Value;
                var deltaTime = Time.deltaTime > 0f ? Time.deltaTime : 1f;
                var horizontal = input.Controllers[0];
                var vertical = input.Controllers[1];
                horizontal.Driver.ProcessInput(ref orbit.HorizontalAxis,
                    horizontal.Input.GetValue(input, IInputAxisOwner.AxisDescriptor.Hints.X), deltaTime);
                vertical.Driver.ProcessInput(ref orbit.VerticalAxis,
                    vertical.Input.GetValue(input, IInputAxisOwner.AxisDescriptor.Hints.Y), deltaTime);
                Assert.That(orbit.HorizontalAxis.Value - yaw, Is.EqualTo(12f).Within(0.001f));
                Assert.That(orbit.VerticalAxis.Value - pitch, Is.EqualTo(-2.4f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(vehicle);
            }
        }

        [Test]
        public void LookingOrbitsWithoutTurningBodyAndMovementFollowsView()
        {
            var player = new GameObject("CameraPlayer");
            var output = new GameObject("CameraOutput");
            try
            {
                player.transform.position = Vector3.up * 1000f;
                var controller = player.AddComponent<LocalOnFootController>();
                var camera = output.AddComponent<Camera>();
                controller.AttachCamera(camera);
                var orbit = player.GetComponentInChildren<CinemachineOrbitalFollow>();
                var yaw = orbit.HorizontalAxis.Value;
                controller.Step(new OnFootMovementIntent(Vector2.zero, new Vector2(100f, 20f), false), 0.1f);
                Assert.That(orbit.HorizontalAxis.Value, Is.EqualTo(yaw + 12f).Within(0.001f));
                Assert.That(Quaternion.Angle(player.transform.rotation, Quaternion.identity), Is.LessThan(0.001f));
                Assert.That(camera.transform.parent, Is.Null);
                camera.transform.rotation = Quaternion.Euler(20f, 90f, 0f);
                var start = player.transform.position;
                controller.Step(new OnFootMovementIntent(Vector2.up, Vector2.zero, false), 0.1f);
                Assert.That(player.transform.position.x - start.x, Is.EqualTo(controller.WalkSpeed * 0.1f).Within(0.01f));
                Assert.That(Vector3.Dot(player.transform.forward, Vector3.right), Is.GreaterThan(0.99f));
                controller.MovementEnabled = false;
                Assert.That(orbit.gameObject.activeSelf, Is.True, "Death freeze does not revoke camera ownership.");
                controller.SetCameraActive(false);
                Assert.That(orbit.gameObject.activeSelf, Is.False);
                controller.AttachCamera(camera);
                Assert.That(orbit.gameObject.activeSelf, Is.True);
                Assert.That(player.GetComponentsInChildren<CinemachineCamera>(true).Length, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(output);
            }
        }

        [Test]
        public void EverySeatHasLocalMouseOrbitAndOnlySelectedCameraIsActive()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab");
            var vehicle = Object.Instantiate(prefab);
            var otherVehicle = Object.Instantiate(prefab);
            var target = new GameObject("RageTarget");
            try
            {
                var rig = vehicle.GetComponent<LocalVehicleCameraRig>();
                var cameras = vehicle.GetComponentsInChildren<CinemachineCamera>(true);
                Assert.That(cameras.Length, Is.EqualTo(4));
                foreach (var camera in cameras)
                {
                    Assert.That(camera.gameObject.activeSelf, Is.False);
                    var orbit = camera.GetComponent<CinemachineOrbitalFollow>();
                    Assert.That(orbit.Radius, Is.EqualTo(7.5f).Within(0.01f));
                    Assert.That(orbit.TargetOffset.y, Is.GreaterThan(1.4f));
                    Assert.That(orbit.VerticalAxis.Value, Is.InRange(12f, 17f));
                    Assert.That(orbit.HorizontalAxis.Recentering.Enabled, Is.True);
                    var input = camera.GetComponent<CinemachineInputAxisController>();
                    Assert.That(input, Is.Not.Null);
                    var enabledAxes = 0;
                    foreach (var axis in input.Controllers)
                    {
                        if (!axis.Enabled) continue;
                        enabledAxes++;
                        Assert.That(axis.Input.InputAction, Is.Not.Null);
                        Assert.That(axis.Input.CancelDeltaTime, Is.True);
                    }
                    Assert.That(enabledAxes, Is.EqualTo(2));
                    var collision = camera.GetComponent<CinemachineDeoccluder>();
                    Assert.That(collision.AvoidObstacles.Enabled, Is.True);
                    Assert.That(collision.IgnoreTag, Is.Empty);
                    Assert.That(collision.AvoidObstacles.UseFollowTarget.Enabled, Is.True);
                }
                for (var seat = 0; seat < 4; seat++)
                {
                    rig.SetLocalSoloCameraActive(true, seat);
                    var active = vehicle.GetComponentsInChildren<CinemachineCamera>();
                    Assert.That(active.Length, Is.EqualTo(1));
                    Assert.That(otherVehicle.GetComponentsInChildren<CinemachineCamera>(), Is.Empty);
                    rig.SetRageTargetLookOverride(target.transform);
                    Assert.That(active[0].LookAt, Is.EqualTo(target.transform));
                    rig.SetRageTargetLookOverride(null);
                    Assert.That(active[0].LookAt, Is.EqualTo(vehicle.transform));
                }
                rig.SetLocalSoloCameraActive(false);
                Assert.That(vehicle.GetComponentsInChildren<CinemachineCamera>(), Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(vehicle);
                Object.DestroyImmediate(otherVehicle);
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void UntaggedObstaclePullsCameraForwardEvenWhenLookAtIsOverridden()
        {
            var target = new GameObject("FollowTarget");
            var lookAt = new GameObject("SeparateLookTarget");
            var cameraObject = new GameObject("CollisionCamera");
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                target.transform.position = new Vector3(1000f, 1000f, 1000f);
                lookAt.transform.position = target.transform.position + Vector3.forward * 10f;
                var camera = cameraObject.AddComponent<CinemachineCamera>();
                ThirdPersonCameraConfiguration.Configure(camera, target.transform, 7.5f, 1.6f, 0f, false);
                camera.LookAt = lookAt.transform;
                wall.transform.position = target.transform.position + new Vector3(0f, 2f, -3f);
                wall.transform.localScale = new Vector3(10f, 10f, 0.5f);
                Physics.SyncTransforms();
                camera.UpdateCameraState(Vector3.up, -1f);
                Assert.That(camera.GetComponent<CinemachineDeoccluder>().GetCameraDisplacementDistance(camera),
                    Is.GreaterThan(1f), "The untagged wall must shorten the follow orbit.");
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(lookAt);
                Object.DestroyImmediate(wall);
            }
        }
    }
}
