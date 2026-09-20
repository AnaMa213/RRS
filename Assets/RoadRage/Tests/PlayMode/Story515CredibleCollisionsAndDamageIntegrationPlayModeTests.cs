using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.PlayMode
{
    [Category("Story515")]
    public sealed class Story515CredibleCollisionsAndDamageIntegrationPlayModeTests
    {
        private const string PlayerPrefabPath = "Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab";
        private const float FixedStep = 0.02f;
        private const float AuthoredSpeed = 18f;
        private const float MaxVerticalExcursion = 0.4f;
        private const float MaxVerticalSpeed = 4f;

        [Test]
        public void PrefabCollisionsAreFiniteBoundedAndOrderedWithImpact()
        {
            var slow = RunWallImpact(CollisionDetectionMode.Discrete, 6f, 0f);
            var fast = RunWallImpact(CollisionDetectionMode.Discrete, AuthoredSpeed, 0.45f);

            Log("mur lent", slow);
            Log("mur rapide decale", fast);
            Assert.That(slow.IsFinite && fast.IsFinite, Is.True);
            Assert.That(fast.HorizontalDisplacement, Is.GreaterThan(slow.HorizontalDisplacement));
            Assert.That(fast.VerticalExcursion, Is.LessThan(MaxVerticalExcursion));
            Assert.That(fast.MaxVerticalSpeed, Is.LessThan(MaxVerticalSpeed));
            Assert.That(fast.PenetratedWall, Is.False);
        }

        [Test]
        public void OffsetVehicleCollisionIsFiniteBoundedAndSeparatesAfterImpact()
        {
            var slow = RunVehicleImpact(3f);
            var fast = RunVehicleImpact(AuthoredSpeed * 0.5f);

            UnityEngine.Debug.Log("[Story515] voiture-voiture lent : separation " + slow.FinalSeparation.ToString("F3")
                + " m, excursion " + slow.VerticalExcursion.ToString("F3") + " m, pic vertical " + slow.MaxVerticalSpeed.ToString("F3")
                + " m/s; rapide : separation " + fast.FinalSeparation.ToString("F3") + " m, excursion "
                + fast.VerticalExcursion.ToString("F3") + " m, pic vertical " + fast.MaxVerticalSpeed.ToString("F3") + " m/s.");
            Assert.That(slow.IsFinite && fast.IsFinite, Is.True);
            Assert.That(slow.HasFinalPenetration || fast.HasFinalPenetration, Is.False, "Les caisses ne doivent pas rester en penetration apres le choc.");
            Assert.That(fast.VerticalExcursion, Is.LessThan(MaxVerticalExcursion));
            Assert.That(fast.MaxVerticalSpeed, Is.LessThan(MaxVerticalSpeed));
        }

        [Test]
        public void LeastCostCollisionModeWithoutTunnellingIsRetainedForThirtyPrefabBodies()
        {
            var discrete = RunTraffic(CollisionDetectionMode.Discrete);
            var continuous = RunTraffic(CollisionDetectionMode.ContinuousDynamic);

            UnityEngine.Debug.Log("[Story515] 30 corps Discrete : tunnels " + discrete.Tunnels + ", mediane " + discrete.MedianMilliseconds.ToString("F3")
                + " ms, p95 " + discrete.P95Milliseconds.ToString("F3") + " ms; ContinuousDynamic : tunnels " + continuous.Tunnels
                + ", mediane " + continuous.MedianMilliseconds.ToString("F3") + " ms, p95 " + continuous.P95Milliseconds.ToString("F3") + " ms.");
            Assert.That(discrete.Tunnels, Is.EqualTo(0));
            Assert.That(continuous.Tunnels, Is.EqualTo(0));
            Assert.That(discrete.MedianMilliseconds, Is.LessThanOrEqualTo(continuous.MedianMilliseconds + 0.05d));
        }

        private static ImpactMeasurement RunWallImpact(CollisionDetectionMode mode, float speed, float lateralOffset)
        {
            using (var harness = new PhysicsHarness())
            {
                var vehicle = harness.CreateVehicle("Story515_Vehicle", new Vector3(lateralOffset, 0.71f, -7f), mode);
                harness.CreateWall(new Vector3(0f, 1.5f, 0f));
                var body = vehicle.GetComponent<Rigidbody>();
                body.linearVelocity = Vector3.forward * speed;
                var startY = vehicle.transform.position.y;
                var startZ = vehicle.transform.position.z;
                var maxY = startY;
                var maxVerticalSpeed = 0f;
                for (var step = 0; step < 80; step++)
                {
                    harness.Simulate();
                    maxY = Mathf.Max(maxY, vehicle.transform.position.y);
                    maxVerticalSpeed = Mathf.Max(maxVerticalSpeed, Mathf.Abs(body.linearVelocity.y));
                }

                return new ImpactMeasurement
                {
                    HorizontalDisplacement = vehicle.transform.position.z - startZ,
                    VerticalExcursion = maxY - startY,
                    MaxVerticalSpeed = maxVerticalSpeed,
                    PenetratedWall = vehicle.transform.position.z > 1f,
                    IsFinite = IsFinite(body.linearVelocity) && IsFinite(body.angularVelocity) && IsFinite(vehicle.transform.position),
                };
            }
        }

        private static TrafficMeasurement RunTraffic(CollisionDetectionMode mode)
        {
            using (var harness = new PhysicsHarness())
            {
                var bodies = new List<Rigidbody>();
                for (var index = 0; index < 30; index++)
                {
                    var lane = index % 10;
                    var row = index / 10;
                    var vehicle = harness.CreateVehicle("Story515_Traffic_" + index, new Vector3((lane - 4.5f) * 3f, 0.71f, -10f - row * 6f), mode);
                    var body = vehicle.GetComponent<Rigidbody>();
                    body.linearVelocity = Vector3.forward * AuthoredSpeed;
                    bodies.Add(body);
                }

                harness.CreateWall(new Vector3(0f, 1.5f, 0f), new Vector3(40f, 3f, 0.2f));
                var samples = new List<double>();
                for (var step = 0; step < 80; step++)
                {
                    var timer = Stopwatch.StartNew();
                    harness.Simulate();
                    timer.Stop();
                    samples.Add(timer.Elapsed.TotalMilliseconds);
                }

                var tunnels = 0;
                foreach (var body in bodies)
                {
                    if (body.transform.position.z > 1f)
                    {
                        tunnels++;
                    }
                }

                samples.Sort();
                return new TrafficMeasurement
                {
                    Tunnels = tunnels,
                    MedianMilliseconds = samples[samples.Count / 2],
                    P95Milliseconds = samples[(int)Mathf.Ceil(samples.Count * 0.95f) - 1],
                };
            }
        }

        private static VehicleImpactMeasurement RunVehicleImpact(float speed)
        {
            using (var harness = new PhysicsHarness())
            {
                var first = harness.CreateVehicle("Story515_FirstVehicle", new Vector3(0f, 0.71f, -7f), CollisionDetectionMode.Discrete);
                var second = harness.CreateVehicle("Story515_SecondVehicle", new Vector3(0.45f, 0.71f, 7f), CollisionDetectionMode.Discrete);
                var firstBody = first.GetComponent<Rigidbody>();
                var secondBody = second.GetComponent<Rigidbody>();
                firstBody.linearVelocity = Vector3.forward * speed;
                secondBody.linearVelocity = Vector3.back * speed;
                var startY = Mathf.Max(first.transform.position.y, second.transform.position.y);
                var maxY = startY;
                var maxVerticalSpeed = 0f;
                for (var step = 0; step < 120; step++)
                {
                    harness.Simulate();
                    maxY = Mathf.Max(maxY, first.transform.position.y, second.transform.position.y);
                    maxVerticalSpeed = Mathf.Max(maxVerticalSpeed, Mathf.Abs(firstBody.linearVelocity.y), Mathf.Abs(secondBody.linearVelocity.y));
                }

                return new VehicleImpactMeasurement
                {
                    FinalSeparation = Vector3.Distance(first.transform.position, second.transform.position),
                    VerticalExcursion = maxY - startY,
                    MaxVerticalSpeed = maxVerticalSpeed,
                    IsFinite = IsFinite(firstBody.linearVelocity) && IsFinite(firstBody.angularVelocity)
                        && IsFinite(secondBody.linearVelocity) && IsFinite(secondBody.angularVelocity),
                    HasFinalPenetration = Physics.ComputePenetration(
                        first.GetComponent<Collider>(), first.transform.position, first.transform.rotation,
                        second.GetComponent<Collider>(), second.transform.position, second.transform.rotation,
                        out _, out _),
                };
            }
        }

        private static void Log(string scenario, ImpactMeasurement measurement)
        {
            UnityEngine.Debug.Log("[Story515] " + scenario + " : deplacement " + measurement.HorizontalDisplacement.ToString("F3")
                + " m, excursion " + measurement.VerticalExcursion.ToString("F3") + " m, pic vertical "
                + measurement.MaxVerticalSpeed.ToString("F3") + " m/s, penetration " + measurement.PenetratedWall + ".");
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsNaN(value.y) && !float.IsNaN(value.z)
                && !float.IsInfinity(value.x) && !float.IsInfinity(value.y) && !float.IsInfinity(value.z);
        }

        private struct ImpactMeasurement
        {
            public float HorizontalDisplacement;
            public float VerticalExcursion;
            public float MaxVerticalSpeed;
            public bool PenetratedWall;
            public bool IsFinite;
        }

        private struct TrafficMeasurement
        {
            public int Tunnels;
            public double MedianMilliseconds;
            public double P95Milliseconds;
        }

        private struct VehicleImpactMeasurement
        {
            public float FinalSeparation;
            public float VerticalExcursion;
            public float MaxVerticalSpeed;
            public bool IsFinite;
            public bool HasFinalPenetration;
        }

        private sealed class PhysicsHarness : IDisposable
        {
            private readonly Scene scene;
            private readonly PhysicsScene physicsScene;
            private readonly List<GameObject> objects = new List<GameObject>();

            public PhysicsHarness()
            {
                scene = SceneManager.CreateScene("Story515_Physics_" + Guid.NewGuid(), new CreateSceneParameters(LocalPhysicsMode.Physics3D));
                physicsScene = scene.GetPhysicsScene();
            }

            public GameObject CreateVehicle(string name, Vector3 position, CollisionDetectionMode mode)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
                Assert.That(prefab, Is.Not.Null, "Le banc utilise le prefab joueur et son profil authorise, jamais une seconde source de verite.");
                var vehicle = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
                vehicle.name = name;
                SceneManager.MoveGameObjectToScene(vehicle, scene);
                foreach (var behaviour in vehicle.GetComponents<MonoBehaviour>())
                {
                    behaviour.enabled = false;
                }

                var body = vehicle.GetComponent<Rigidbody>();
                body.useGravity = false;
                body.collisionDetectionMode = mode;
                objects.Add(vehicle);
                return vehicle;
            }

            public void CreateWall(Vector3 position, Vector3? size = null)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "Story515_Wall";
                wall.transform.position = position;
                wall.transform.localScale = size ?? new Vector3(8f, 3f, 0.2f);
                SceneManager.MoveGameObjectToScene(wall, scene);
                objects.Add(wall);
            }

            public void Simulate()
            {
                physicsScene.Simulate(FixedStep);
            }

            public void Dispose()
            {
                foreach (var gameObject in objects)
                {
                    UnityEngine.Object.DestroyImmediate(gameObject);
                }

                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }
}
