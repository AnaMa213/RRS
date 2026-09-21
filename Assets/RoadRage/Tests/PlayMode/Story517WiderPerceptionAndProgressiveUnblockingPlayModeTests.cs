using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.MainMenu;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>Story 5.17 : regression du depart dans MVP_Run, avec prefab reseau et physique reels.
    /// Le test observe le deplacement; il ne force ni vitesse ni position pendant la conduite.</summary>
    [Category("Story517")]
    public sealed class Story517WiderPerceptionAndProgressiveUnblockingPlayModeTests
    {
        private string originalProfileFilePath;
        private string tempProfileFilePath;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (NetworkManager.Singleton != null)
            {
                if (NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            if (RoadRageBootstrap.Instance != null) Object.Destroy(RoadRageBootstrap.Instance.gameObject);
            if (originalProfileFilePath != null) PlayerProfileFileStore.DefaultFilePath = originalProfileFilePath;
            if (!string.IsNullOrEmpty(tempProfileFilePath) && File.Exists(tempProfileFilePath)) File.Delete(tempProfileFilePath);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TrafficLeavesItsEntryTunnelUnderItsOwnPhysics()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story517-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;
            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;

            ClickSerializedButton(Object.FindAnyObjectByType<MainMenuScreen>(), "playButton");
            yield return null;
            var lobby = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobby, Is.Not.Null);
            ClickSerializedButton(lobby, "startGameButton");
            for (var frame = 0; SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frame < 300; frame++)
            {
                var bootstrap = RoadRageBootstrap.Instance;
                if (bootstrap != null && bootstrap.LobbyRoom != null && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                    && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                {
                    Assert.Inconclusive("Services Steam indisponibles : banc 5.17 non executable.");
                    yield break;
                }
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));
            NetworkedAIVehicleDriverController controller = null;
            for (var frame = 0; controller == null && frame < 900; frame++)
            {
                controller = Object.FindAnyObjectByType<NetworkedAIVehicleDriverController>();
                yield return null;
            }
            Assert.That(controller, Is.Not.Null, "MVP_Run doit inserer un vehicule IA par le spawner de portails.");

            var profile = GetPrivateField(controller, "driverProfile") as DriverProfileDef;
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.TryValidate(out var error), Is.True, error);
            var body = controller.GetComponent<Rigidbody>();
            var start = controller.transform.position;
            var forward = controller.transform.forward;
            var previous = start;
            var maxSpeed = 0f;
            var elapsed = 0f;
            var advance = 0f;
            // Les entrees des tunnels MVP_Run sont a 12 m de leur premier successeur routier.
            while (elapsed < 20f && advance < 12f)
            {
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
                Assert.That(controller != null && controller.IsSpawned, Is.True, "Le depart ne doit pas retirer le vehicule.");
                var position = body.position;
                maxSpeed = Mathf.Max(maxSpeed, body.linearVelocity.magnitude);
                Assert.That(Vector3.Distance(position, previous), Is.LessThan(1f), "Aucun saut de recuperation au depart.");
                previous = position;
                advance = Vector3.Dot(position - start, forward);
            }
            Assert.That(maxSpeed, Is.GreaterThan(.5f), "Le moteur doit effectivement mettre la caisse en mouvement.");
            Assert.That(advance, Is.GreaterThanOrEqualTo(12f),
                "La voiture doit sortir du tunnel. Etat=" + controller.UnblockingAction
                + ", refus=" + controller.TrafficRefusal + ", TTC=" + controller.PredictedCollisionTime);

            // Blocage reel, sans piloter l'IA pendant l'observation : le conducteur doit passer par
            // replanification puis recul avant tout detour, sans saut ni retrait. Retirer ensuite
            // l'obstacle prouve que l'episode se ferme et que la route nominale reprend.
            var predictedPath = new Vector3[12];
            var pathCount = controller.CopyPredictedPath(predictedPath, 20f);
            Assert.That(pathCount, Is.GreaterThanOrEqualTo(2));
            var routeForward = Vector3.ProjectOnPlane(predictedPath[1] - predictedPath[0], Vector3.up).normalized;
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Story517_StaticLaneBlocker";
            obstacle.transform.SetPositionAndRotation(body.position + routeForward * 5f + Vector3.up,
                Quaternion.LookRotation(routeForward));
            obstacle.transform.localScale = new Vector3(2f, 2f, 3f);
            Physics.SyncTransforms();

            var sawReplan = false;
            var sawReverse = false;
            previous = body.position;
            for (var step = 0; step < 500 && !sawReverse; step++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(controller != null && controller.IsSpawned, Is.True,
                    "Un bouchon ne doit jamais retirer ou remplacer le vehicule.");
                Assert.That(Vector3.Distance(body.position, previous), Is.LessThan(1f),
                    "Un bouchon ne doit jamais declencher de teleportation.");
                previous = body.position;
                sawReplan |= controller.UnblockingAction == TrafficUnblockingAction.Replan;
                sawReverse |= controller.UnblockingAction == TrafficUnblockingAction.Reverse;
            }

            Assert.That(sawReplan, Is.True, "Le blocage doit declencher la replanification immediate.");
            Assert.That(sawReverse, Is.True,
                "Sans passage routier immediat, le recul utile doit preceder le detour.");

            Object.Destroy(obstacle);
            yield return null;
            var recoveryStart = body.position;
            var resumed = false;
            for (var step = 0; step < 750 && !resumed; step++)
            {
                yield return new WaitForFixedUpdate();
                resumed = controller.UnblockingAction == TrafficUnblockingAction.FollowRoute
                    && Vector3.ProjectOnPlane(body.position - recoveryStart, Vector3.up).magnitude >= 3f;
            }
            Assert.That(resumed, Is.True,
                "Apres disparition du blocage, l'IA doit fermer l'episode et reprendre sa route.");
        }

        private static void ClickSerializedButton(Component screen, string fieldName)
        {
            Assert.That(screen, Is.Not.Null);
            var button = GetPrivateField(screen, fieldName) as Button;
            Assert.That(button, Is.Not.Null);
            button.onClick.Invoke();
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }
    }
}
