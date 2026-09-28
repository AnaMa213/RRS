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
    [Category("Core")]
    public sealed class ThirdPersonCameraTests
    {
        [Test]
        public void MouseActionDrivesNativeOrbitWithoutFrameScaling()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab");
            var vehicle = Object.Instantiate(prefab);
            try
            {
                vehicle.GetComponent<LocalVehicleCameraRig>().SetManualCameraActive(true);
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
                    rig.SetManualCameraActive(true, seat);
                    var active = vehicle.GetComponentsInChildren<CinemachineCamera>();
                    Assert.That(active.Length, Is.EqualTo(1));
                    Assert.That(otherVehicle.GetComponentsInChildren<CinemachineCamera>(), Is.Empty);
                    target.transform.position = vehicle.transform.position + (vehicle.transform.forward * 20f);
                    rig.SetRageTargetLookOverride(target.transform);
                    // Is.SameAs et non Is.EqualTo : Transform implemente IEnumerable, donc NUnit
                    // comparerait les enfants et jugerait egaux deux transforms sans enfant.
                    Assert.That(active[0].LookAt, Is.Not.SameAs(target.transform),
                        "Story 5.5 : la camera vise un relais borne au cone de conduite, jamais la cible en dur.");
                    Assert.That(active[0].LookAt.IsChildOf(vehicle.transform), Is.True);
                    Assert.That(active[0].LookAt.position, Is.EqualTo(target.transform.position).Using(Vector3Comparer),
                        "Cible dans le cone : suivi exact.");
                    Assert.That(active[0].Follow, Is.SameAs(vehicle.transform),
                        "Le lock change uniquement le regard : la position reste ancree au vehicule du joueur.");
                    rig.SetRageTargetLookOverride(null);
                    Assert.That(active[0].LookAt, Is.SameAs(vehicle.transform));
                    Assert.That(active[0].Follow, Is.EqualTo(vehicle.transform));
                }
                rig.SetManualCameraActive(false);
                Assert.That(vehicle.GetComponentsInChildren<CinemachineCamera>(), Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(vehicle);
                Object.DestroyImmediate(otherVehicle);
                Object.DestroyImmediate(target);
            }
        }

        /// <summary>
        /// Story 5.5 (correctif feel, 2026-09-15) : le cone de visee est ce qui empeche le lock de
        /// rendre la conduite illisible quand la cible passe sur le cote ou derriere. Teste sur la
        /// fonction pure, sans camera ni session reseau.
        /// </summary>
        [Test]
        public void LockedTargetLookPointStaysInsideTheDrivingCone()
        {
            const float maxAngle = 40f;
            var side = 1f;
            var anchor = new Vector3(3f, 0f, 7f);
            var rotation = Quaternion.Euler(0f, 90f, 0f);

            var ahead = anchor + (rotation * new Vector3(0f, 0f, 25f));
            Assert.That(LocalVehicleCameraRig.ResolveRageTargetLookPoint(anchor, rotation, ahead, maxAngle, ref side),
                Is.EqualTo(ahead).Using(Vector3Comparer), "Cible droit devant : suivi exact.");

            var slightlyOff = anchor + (rotation * (Quaternion.Euler(0f, 30f, 0f) * new Vector3(0f, 0f, 25f)));
            Assert.That(LocalVehicleCameraRig.ResolveRageTargetLookPoint(anchor, rotation, slightlyOff, maxAngle, ref side),
                Is.EqualTo(slightlyOff).Using(Vector3Comparer), "Cible dans le cone : suivi exact, pas de bornage premature.");

            foreach (var targetYaw in new[] { 75f, 120f, 179f, -75f, -120f, -179f })
            {
                var target = anchor + (rotation * (Quaternion.Euler(0f, targetYaw, 0f) * new Vector3(0f, 0f, 25f)));
                var point = LocalVehicleCameraRig.ResolveRageTargetLookPoint(anchor, rotation, target, maxAngle, ref side);
                var local = Quaternion.Inverse(rotation) * (point - anchor);
                var resolvedYaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
                Assert.That(Mathf.Abs(resolvedYaw), Is.EqualTo(maxAngle).Within(0.01f),
                    "Cible hors cone (" + targetYaw + " deg) : la vue reste au bord du cone, jamais braquee hors route.");
                Assert.That(local.magnitude, Is.EqualTo(25f).Within(0.01f), "Distance de visee conservee.");
                if (Mathf.Abs(targetYaw) < 150f)
                {
                    Assert.That(Mathf.Sign(resolvedYaw), Is.EqualTo(Mathf.Sign(targetYaw)),
                        "La vue penche du cote de la cible : elle indique ou elle est.");
                }
            }
        }

        /// <summary>Cible pile derriere : le cote du cone est fige, sinon la vue claque de gauche a droite a chaque embardee.</summary>
        [Test]
        public void TargetDirectlyBehindKeepsTheSameConeSide()
        {
            const float maxAngle = 40f;
            var side = 1f;
            var rotation = Quaternion.identity;

            var rightSide = new Vector3(20f, 0f, 5f);
            LocalVehicleCameraRig.ResolveRageTargetLookPoint(Vector3.zero, rotation, rightSide, maxAngle, ref side);
            Assert.That(side, Is.EqualTo(1f));

            var justBehindLeft = new Vector3(-0.2f, 0f, -25f);
            var justBehindRight = new Vector3(0.2f, 0f, -25f);
            var first = LocalVehicleCameraRig.ResolveRageTargetLookPoint(Vector3.zero, rotation, justBehindLeft, maxAngle, ref side);
            var second = LocalVehicleCameraRig.ResolveRageTargetLookPoint(Vector3.zero, rotation, justBehindRight, maxAngle, ref side);
            Assert.That(Mathf.Sign(first.x), Is.EqualTo(1f), "Le cote est conserve malgre le passage de la cible a gauche.");
            Assert.That(first, Is.EqualTo(second).Using(Vector3Comparer), "Aucun battement quand la cible traverse l'axe arriere.");
        }

        private static readonly System.Collections.Generic.IComparer<Vector3> Vector3Comparer =
            System.Collections.Generic.Comparer<Vector3>.Create((left, right) => (left - right).sqrMagnitude <= 0.0001f ? 0 : 1);

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
