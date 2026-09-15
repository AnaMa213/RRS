using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
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
    /// <summary>
    /// Story 5.9 : preuve d'integration dans MVP_Run (exigence AGENTS.md). Les gardes EditMode
    /// decrivent le contrat du modele ; ce fixture constate sur un pair reel que les trois vehicules
    /// de AITraffic portent bien le profil authore, qu'ils avancent le long de leur route sous le
    /// nouveau modele (acceleration integree au lieu d'une vitesse de croisiere cablee), et
    /// qu'aucune vitesse NaN/Inf ne contamine leur Rigidbody.
    ///
    /// Meme demarrage que Story 5.7 : bootstrap -> menu -> lobby -> Start Game. Une machine sans
    /// Steam P2P fonctionnel rend le test Inconclusif plutot que rouge.
    /// </summary>
    [Category("Story59")]
    public sealed class Story59ParameterizedDriverModelPlayModeTests
    {
        private string originalProfileFilePath;

        private string tempProfileFilePath;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                if (manager.IsListening)
                {
                    manager.Shutdown();
                }

                Object.Destroy(manager.gameObject);
            }

            var survivor = RoadRageBootstrap.Instance;
            if (survivor != null)
            {
                Object.Destroy(survivor.gameObject);
            }

            if (originalProfileFilePath != null)
            {
                PlayerProfileFileStore.DefaultFilePath = originalProfileFilePath;
                originalProfileFilePath = null;
            }

            if (!string.IsNullOrEmpty(tempProfileFilePath) && File.Exists(tempProfileFilePath))
            {
                File.Delete(tempProfileFilePath);
            }

            tempProfileFilePath = null;

            yield return null;
        }

        [UnityTest]
        public IEnumerator AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story59-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;

            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");
            ClickSerializedButton(menuScreen, "playButton");
            yield return null;

            var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu apres Play");

            // Meme fenetre de tolerance que Story 5.7 : la creation du lobby Steam peut logguer par
            // frame de polling sans que le chemin lobby -> host -> MVP_Run soit en cause.
            LogAssert.ignoreFailingMessages = true;
            try
            {
                ClickSerializedButton(lobbyScreen, "startGameButton");

                var frames = 0;
                while (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frames < 300)
                {
                    var bootstrap = RoadRageBootstrap.Instance;
                    if (bootstrap != null && bootstrap.LobbyRoom != null
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                    {
                        Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier la conduite IA hote.");
                        yield break;
                    }

                    yield return null;
                    frames++;
                }

                for (var settleFrame = 0; settleFrame < 8; settleFrame++)
                {
                    yield return null;
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));

            var controllers = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsInactive.Exclude);
            Assert.That(controllers.Length, Is.EqualTo(3), "Les trois vehicules de AITraffic doivent etre spawes.");

            var startPositions = new Vector3[controllers.Length];
            for (var i = 0; i < controllers.Length; i++)
            {
                var profile = GetPrivateField(controllers[i], "driverProfile") as DriverProfileDef;
                Assert.That(profile != null, Is.True,
                    controllers[i].name + " : le profil de conduite vient du prefab, aucune instance de scene ne doit le perdre.");
                Assert.That(profile.TryValidate(out var error), Is.True, controllers[i].name + " : " + error);

                startPositions[i] = controllers[i].transform.position;
            }

            // Le modele integre une acceleration a partir de 0 : laisser quelques secondes de
            // simulation avant de constater le deplacement.
            var elapsed = 0f;
            while (elapsed < 4f)
            {
                elapsed += Time.deltaTime;

                foreach (var controller in controllers)
                {
                    var velocity = controller.GetComponent<Rigidbody>().linearVelocity;
                    Assert.That(float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z), Is.False,
                        controller.name + " : une vitesse NaN contaminerait le NetworkTransform.");
                    Assert.That(float.IsInfinity(velocity.x) || float.IsInfinity(velocity.y) || float.IsInfinity(velocity.z), Is.False,
                        controller.name + " : une vitesse infinie contaminerait le NetworkTransform.");
                }

                yield return null;
            }

            for (var i = 0; i < controllers.Length; i++)
            {
                var travelled = Vector3.ProjectOnPlane(controllers[i].transform.position - startPositions[i], Vector3.up).magnitude;
                Assert.That(travelled, Is.GreaterThan(1f),
                    controllers[i].name + " : le vehicule doit avancer le long de sa route sous le modele parametre.");
            }
        }

        private static void ClickSerializedButton(Component screen, string buttonFieldName)
        {
            var button = GetPrivateField(screen, buttonFieldName) as Button;
            Assert.That(button, Is.Not.Null, screen.GetType().Name + "." + buttonFieldName + " doit referencer un Button de la scene.");
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
