using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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
    /// Story 5.31 : banc physique isole (patron Story 5.12 / 5.13) de la commande de repli V2, avec la
    /// couche VehiclePhysicsBody, le profil vehicule, le collider et le profil conducteur du prefab V2.
    /// Le repli est force a chaque pas depuis 8 ; 0,35 ; 0,3 ; 0,2 et -0,5 m/s, sur sol plat et sur une
    /// pente declaree. Le banc ne fait que soumettre l'intent compose (aucune ecriture de vitesse apres
    /// la mise en condition initiale) et publie sa trace brute. Les seuils de resultat ne sont pas
    /// presumes : seules les regles de la commande (aucune marche arriere commandee, etat terminal
    /// explicite, maintien a l'arret) sont verifiees.
    /// </summary>
    [Category("Story531")]
    public sealed class Story531FallbackLowSpeedPlayModeTests
    {
        private const string V2PrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab";

        /// <summary>Pente declaree du banc (degres), vehicule orienté vers l'aval.</summary>
        private const float DeclaredSlopeDegrees = 5f;

        private const int HoldObservationSteps = 100;
        private const int MaxSteps = 1500;
        private const string RawFolder = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/fallback-bench";

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
            benchScene = SceneManager.CreateScene("Story531_Bench_" + System.Guid.NewGuid());
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
        public IEnumerator FallbackFromEightMetresPerSecondOnFlatGround() { return RunCase(8f, 0f); }

        [UnityTest]
        public IEnumerator FallbackInsideTheServiceBandOnFlatGround() { return RunCase(0.35f, 0f); }

        [UnityTest]
        public IEnumerator FallbackAtPointThreeOnFlatGround() { return RunCase(0.3f, 0f); }

        [UnityTest]
        public IEnumerator FallbackAtPointTwoOnFlatGround() { return RunCase(0.2f, 0f); }

        [UnityTest]
        public IEnumerator FallbackWhileRollingBackwardOnFlatGround() { return RunCase(-0.5f, 0f); }

        [UnityTest]
        public IEnumerator FallbackFromEightMetresPerSecondOnTheDeclaredSlope() { return RunCase(8f, DeclaredSlopeDegrees); }

        [UnityTest]
        public IEnumerator FallbackInsideTheServiceBandOnTheDeclaredSlope() { return RunCase(0.35f, DeclaredSlopeDegrees); }

        [UnityTest]
        public IEnumerator FallbackAtPointThreeOnTheDeclaredSlope() { return RunCase(0.3f, DeclaredSlopeDegrees); }

        [UnityTest]
        public IEnumerator FallbackAtPointTwoOnTheDeclaredSlope() { return RunCase(0.2f, DeclaredSlopeDegrees); }

        [UnityTest]
        public IEnumerator FallbackWhileRollingBackwardOnTheDeclaredSlope() { return RunCase(-0.5f, DeclaredSlopeDegrees); }

        private IEnumerator RunCase(float startSpeed, float slopeDegrees)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(V2PrefabPath);
            Assert.That(prefab, Is.Not.Null, V2PrefabPath);
            var prefabBody = prefab.GetComponent<VehiclePhysicsBody>();
            var profileDef = (VehicleProfileDef)new SerializedObject(prefabBody).FindProperty("vehicleProfile").objectReferenceValue;
            var driverDef = prefab.GetComponent<TrafficV2VehicleDriver>().DriverProfileDefinition;
            var prefabCollider = prefab.GetComponent<BoxCollider>();
            var prefabRigidbody = prefab.GetComponent<Rigidbody>();
            Assert.That(profileDef, Is.Not.Null);
            Assert.That(driverDef, Is.Not.Null);

            var slope = Quaternion.Euler(slopeDegrees, 0f, 0f);
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Story531_Ground";
            ground.transform.rotation = slope;
            ground.transform.position = slope * new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(400f, 1f, 1200f);

            // Le vehicule est pose a l'arriere du plan et regarde l'aval (+z incline vers le bas).
            vehicle = new GameObject("Story531_Vehicle");
            vehicle.SetActive(false);
            vehicle.transform.rotation = slope;
            vehicle.transform.position = slope * new Vector3(0f, 0.5f, -400f);
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

            // Mise en condition : maintien au frein a main pour se poser, puis vitesse de depart.
            var profile = physics.Profile;
            for (var i = 0; i < 100; i++)
            {
                physics.ApplyDriveIntent(new VehicleDriveIntent(0f, 0f, 0f, 1f), profile.MaxForwardSpeed,
                    profile.SteerRateDegreesPerSecond, profile.BrakeTorque);
                yield return new WaitForFixedUpdate();
            }

            if (startSpeed > 1f)
            {
                // Montee en vitesse par les roues, jamais par une ecriture.
                var guard = 0;
                while (Longitudinal(physics) < startSpeed && guard++ < MaxSteps)
                {
                    physics.ApplyDriveIntent(new VehicleDriveIntent(1f, 0f, 0f, 0f), profile.MaxForwardSpeed,
                        profile.SteerRateDegreesPerSecond, profile.BrakeTorque);
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(Longitudinal(physics), Is.GreaterThanOrEqualTo(startSpeed), "le banc doit atteindre la vitesse de depart");
            }
            else
            {
                // Tres basse vitesse : mise en condition initiale du banc (seule ecriture, avant tout repli).
                physics.ApplyDriveIntent(VehicleDriveIntent.Idle, profile.MaxForwardSpeed, profile.SteerRateDegreesPerSecond, profile.BrakeTorque);
                body.linearVelocity = vehicle.transform.forward * startSpeed;
                yield return new WaitForFixedUpdate();
            }

            var composer = new VehicleDriveIntentComposer(profile, driverDef.Profile.SafeBrakingLimit, Time.fixedDeltaTime);
            var trace = new StringBuilder("step\tv\tthrottle\tbrake_reverse\thandbrake\tmin_drive_torque\tterminal\tdiagnostics\ty\n");
            var previous = VehicleDriveIntent.Idle;
            var forwardStart = startSpeed > 0f;
            var minimumSpeed = float.PositiveInfinity;
            var negativeTorqueSteps = 0;
            var brakeReverseInBand = 0;
            var rollingBackwardSeen = false;
            var terminalStep = -1;
            var holdViolations = 0;
            var maxHoldSpeed = 0f;
            V2FallbackTerminal terminal = V2FallbackTerminal.None;
            var groundTop = ground.transform.position.y;
            for (var step = 1; step <= MaxSteps; step++)
            {
                Assert.That(vehicle != null && vehicle.activeInHierarchy, Is.True, "le vehicule reste present");
                var position = body.position;
                Assert.That(float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z)
                    || float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z), Is.False, "pose finie");
                var groundHeight = (Quaternion.Inverse(slope) * position).y;
                Assert.That(groundHeight, Is.GreaterThan(-0.05f), "au-dessus du sol");

                var speed = Longitudinal(physics);
                minimumSpeed = Mathf.Min(minimumSpeed, speed);
                var drive = composer.Compose((ulong)step, null, V2FallbackReason.NoCommand, speed, body.linearDamping);
                var torque = Mathf.Min(
                    VehicleDriveIntentComposer.MinimumWheelDriveTorque(profile, drive.Intent, drive.MaxForwardSpeed, speed),
                    VehicleDriveIntentComposer.MinimumWheelDriveTorque(profile, previous, drive.MaxForwardSpeed, speed));
                if (torque < 0f)
                {
                    negativeTorqueSteps++;
                }

                if (speed <= composer.ServiceBandMetersPerSecond && drive.Intent.BrakeReverse > 0f)
                {
                    brakeReverseInBand++;
                }

                rollingBackwardSeen |= (drive.Diagnostics & V2ComposerDiagnostic.RollingBackward) != 0;
                if (terminalStep < 0 && drive.Terminal != V2FallbackTerminal.None)
                {
                    terminalStep = step;
                    terminal = drive.Terminal;
                }

                if (drive.Terminal == V2FallbackTerminal.Held)
                {
                    maxHoldSpeed = Mathf.Max(maxHoldSpeed, Mathf.Abs(speed));
                    if (Mathf.Abs(speed) > VehicleDriveIntentComposer.FallbackStoppedSpeedMetersPerSecond)
                    {
                        holdViolations++;
                    }
                }

                trace.Append(step).Append('\t').Append(F(speed)).Append('\t').Append(F(drive.Intent.Throttle)).Append('\t')
                    .Append(F(drive.Intent.BrakeReverse)).Append('\t').Append(F(drive.Intent.Handbrake)).Append('\t').Append(F(torque))
                    .Append('\t').Append(drive.Terminal).Append('\t').Append(drive.Diagnostics).Append('\t').Append(F(groundHeight)).Append('\n');

                physics.ApplyDriveIntent(drive.Intent, drive.MaxForwardSpeed, drive.SteerRateDegreesPerSecond, drive.BrakeTorque);
                previous = drive.Intent;
                yield return new WaitForFixedUpdate();

                if (terminalStep > 0 && step - terminalStep >= HoldObservationSteps
                    && (drive.Terminal == V2FallbackTerminal.Held || step - terminalStep >= 2 * HoldObservationSteps))
                {
                    break;
                }
            }

            var label = "v0=" + F(startSpeed) + " pente=" + F(slopeDegrees) + "deg";
            Directory.CreateDirectory(RawFolder);
            File.WriteAllText(RawFolder + "/fallback-" + (startSpeed < 0f ? "m" : string.Empty) + F(Mathf.Abs(startSpeed)).Replace('.', '_')
                + "-slope" + F(slopeDegrees).Replace('.', '_') + ".tsv", trace.ToString());
            var summary = "[Story531] repli " + label + " : terminal " + terminal + " au pas " + terminalStep + ", v min "
                + F(minimumSpeed) + " m/s, v max au maintien " + F(maxHoldSpeed) + " m/s, pas a couple negatif " + negativeTorqueSteps
                + ", BrakeReverse dans la bande " + brakeReverseInBand + ", RollingBackward " + rollingBackwardSeen;
            Debug.Log(summary);

            Assert.That(negativeTorqueSteps, Is.EqualTo(0), "couple moteur recalcule par VehicleTireModel >= 0 a chaque pas : " + summary);
            Assert.That(brakeReverseInBand, Is.EqualTo(0), "aucun BrakeReverse a v <= v_s : " + summary);
            Assert.That(terminal, Is.Not.EqualTo(V2FallbackTerminal.None), "FallbackHeld ou FallbackStopOverrun explicite : " + summary);
            if (forwardStart)
            {
                Assert.That(minimumSpeed, Is.GreaterThanOrEqualTo(-profile.MinimumDirectionSpeed),
                    "jamais de marche arriere apres un depart en marche avant : " + summary);
            }
            else
            {
                Assert.That(rollingBackwardSeen, Is.True, "recul diagnostique RollingBackward : " + summary);
            }

            Assert.That(holdViolations, Is.EqualTo(0), "maintien |v| <= 0,05 m/s pendant l'arret : " + summary);
        }

        private static float Longitudinal(VehiclePhysicsBody physics)
        {
            return physics.TrySampleTelemetry(out var sample) ? sample.LongitudinalSpeed : 0f;
        }

        private static string F(float value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
