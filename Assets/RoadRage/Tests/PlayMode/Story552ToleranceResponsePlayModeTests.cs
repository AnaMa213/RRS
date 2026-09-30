using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using RoadRage.App.Services;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.52, decision 2a : banc physique isole (patron Story 5.31) de la reponse de fonctionnement normal a
    /// TrackingToleranceExceeded, avec la couche VehiclePhysicsBody, le profil vehicule, le collider et le profil
    /// conducteur du prefab V2. Le verrou pris a 8 m/s donne le repli V2 a chaque pas jusqu'a l'arret maintenu ; le
    /// vehicule reste present, non cinematique, sans contrainte, et une poussee externe le deplace encore.
    /// </summary>
    [Category("Story552")]
    public sealed class Story552ToleranceResponsePlayModeTests
    {
        private const string V2PrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab";
        private const int MaxSteps = 1500;
        private const int HoldSteps = 50;
        private const int PushSteps = 100;

        private GameObject ground;
        private GameObject vehicle;
        private Scene benchScene;
        private Scene originalActiveScene;
        private string originalActiveScenePath;
        private readonly List<string> unloadedScenePaths = new List<string>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            originalActiveScene = SceneManager.GetActiveScene();
            originalActiveScenePath = originalActiveScene.path;
            benchScene = SceneManager.CreateScene("Story552_Bench_" + System.Guid.NewGuid());
            SceneManager.SetActiveScene(benchScene);
            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene == benchScene)
                {
                    continue;
                }

                if (scene.name == AppSceneRouter.BootstrapSceneName || scene.name == AppSceneRouter.MainMenuLobbySceneName
                    || scene.name == AppSceneRouter.MvpRunSceneName)
                {
                    if (!string.IsNullOrEmpty(scene.path))
                    {
                        unloadedScenePaths.Add(scene.path);
                    }

                    yield return SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(ground);
            Object.Destroy(vehicle);
            yield return null;
            if (benchScene.IsValid() && benchScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(benchScene);
            }

            foreach (var scenePath in unloadedScenePaths)
            {
                var loaded = SceneManager.GetSceneByPath(scenePath);
                if (!loaded.IsValid() || !loaded.isLoaded)
                {
                    yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                }
            }

            var restore = originalActiveScene;
            if ((!restore.IsValid() || !restore.isLoaded) && !string.IsNullOrEmpty(originalActiveScenePath))
            {
                restore = SceneManager.GetSceneByPath(originalActiveScenePath);
            }

            if (restore.IsValid() && restore.isLoaded)
            {
                SceneManager.SetActiveScene(restore);
            }

            unloadedScenePaths.Clear();
        }

        [UnityTest]
        public IEnumerator AnExceededToleranceHoldsTheVehicleButLeavesItFreeToBePushed()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(V2PrefabPath);
            Assert.That(prefab, Is.Not.Null, V2PrefabPath);
            var profileDef = (VehicleProfileDef)new SerializedObject(prefab.GetComponent<VehiclePhysicsBody>()).FindProperty("vehicleProfile").objectReferenceValue;
            var driverDef = prefab.GetComponent<TrafficV2VehicleDriver>().DriverProfileDefinition;
            var prefabCollider = prefab.GetComponent<BoxCollider>();
            var prefabRigidbody = prefab.GetComponent<Rigidbody>();
            Assert.That(profileDef, Is.Not.Null);
            Assert.That(driverDef, Is.Not.Null);

            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Story552_Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(400f, 1f, 1200f);

            vehicle = new GameObject("Story552_Vehicle");
            vehicle.SetActive(false);
            vehicle.transform.position = new Vector3(0f, 0.5f, -400f);
            var box = vehicle.AddComponent<BoxCollider>();
            box.center = prefabCollider.center;
            box.size = prefabCollider.size;
            var body = vehicle.AddComponent<Rigidbody>();
            body.mass = prefabRigidbody.mass;
            body.linearDamping = prefabRigidbody.linearDamping;
            body.angularDamping = prefabRigidbody.angularDamping;
            var physics = vehicle.AddComponent<VehiclePhysicsBody>();
            physics.BindProfile(profileDef);
            vehicle.SetActive(true);
            Assert.That(physics.HasProfile, Is.True);

            var profile = physics.Profile;
            for (var i = 0; i < 100; i++)
            {
                physics.ApplyDriveIntent(new VehicleDriveIntent(0f, 0f, 0f, 1f), profile.MaxForwardSpeed, profile.SteerRateDegreesPerSecond, profile.BrakeTorque);
                yield return new WaitForFixedUpdate();
            }

            var guard = 0;
            while (Longitudinal(physics) < 8f && guard++ < MaxSteps)
            {
                // Montee en vitesse par les roues, jamais par une ecriture.
                physics.ApplyDriveIntent(new VehicleDriveIntent(1f, 0f, 0f, 0f), profile.MaxForwardSpeed, profile.SteerRateDegreesPerSecond, profile.BrakeTorque);
                yield return new WaitForFixedUpdate();
            }

            Assert.That(Longitudinal(physics), Is.GreaterThanOrEqualTo(8f), "le banc doit atteindre 8 m/s");

            // Depassement hors mesure : le verrou 2a retire toute commande a chaque pas suivant.
            var declared = TrafficV2Settings.DeclaredTrackingTolerance;
            var response = new TrackingToleranceResponse();
            Assert.That(response.Observe(1UL, false, declared, declared.Meters + 0.01f), Is.True);
            var composer = new VehicleDriveIntentComposer(profile, driverDef.Profile.SafeBrakingLimit, Time.fixedDeltaTime);
            var heldStep = -1;
            var step = 1;
            for (; step <= MaxSteps; step++)
            {
                var drive = Compose(composer, response, step, physics, body);
                Assert.That(drive.Fallback && drive.Reason == V2FallbackReason.TrackingToleranceExceeded, Is.True, "repli V2 a chaque pas");
                physics.ApplyDriveIntent(drive.Intent, drive.MaxForwardSpeed, drive.SteerRateDegreesPerSecond, drive.BrakeTorque);
                yield return new WaitForFixedUpdate();
                AssertPresent(body);
                if (heldStep < 0 && drive.Terminal == V2FallbackTerminal.Held) heldStep = step;
                if (heldStep > 0 && step - heldStep >= HoldSteps) break;
            }

            Assert.That(heldStep, Is.GreaterThan(0), "arret maintenu atteint (FallbackHeld)");
            Assert.That(body.isKinematic, Is.False, "corps physiquement libre");
            Assert.That(body.constraints, Is.EqualTo(RigidbodyConstraints.None), "aucune contrainte Rigidbody");

            var before = body.position;
            body.AddForce(vehicle.transform.forward * body.mass * 1.5f, ForceMode.Impulse);
            for (var push = 0; push < PushSteps; push++, step++)
            {
                var drive = Compose(composer, response, step, physics, body);
                Assert.That(drive.Fallback && drive.Reason == V2FallbackReason.TrackingToleranceExceeded, Is.True, "le maintien continue sans snap");
                physics.ApplyDriveIntent(drive.Intent, drive.MaxForwardSpeed, drive.SteerRateDegreesPerSecond, drive.BrakeTorque);
                yield return new WaitForFixedUpdate();
                AssertPresent(body);
            }

            var moved = new Vector2(body.position.x - before.x, body.position.z - before.z).magnitude;
            Debug.Log("[Story552] reponse 2a : arret maintenu au pas " + heldStep + ", deplacement sous poussee "
                + moved.ToString("0.####", CultureInfo.InvariantCulture) + " m.");
            Assert.That(moved, Is.GreaterThan(0.02f), "une poussee externe deplace encore le vehicule maintenu");
        }

        private static ComposedDrive Compose(VehicleDriveIntentComposer composer, TrackingToleranceResponse response, int step,
            VehiclePhysicsBody physics, Rigidbody body)
        {
            Assert.That(response.Latched, Is.True);
            return composer.Compose((ulong)step, null, V2FallbackReason.TrackingToleranceExceeded, Longitudinal(physics), body.linearDamping);
        }

        private void AssertPresent(Rigidbody body)
        {
            Assert.That(vehicle != null && vehicle.activeInHierarchy, Is.True, "le vehicule reste present");
            var position = body.position;
            Assert.That(float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z)
                || float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z), Is.False, "pose finie");
            Assert.That(position.y, Is.GreaterThan(-0.05f), "au-dessus du sol");
        }

        private static float Longitudinal(VehiclePhysicsBody physics)
        {
            return physics.TrySampleTelemetry(out var sample) ? sample.LongitudinalSpeed : 0f;
        }
    }
}
