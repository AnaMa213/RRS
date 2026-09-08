using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Couvre la Story 2.2 sur le vrai cycle de vie Unity : le clic sur Create Lobby doit faire evoluer
    /// LobbyRoomService vers un etat terminal (jamais rester bloque sur Creating), et un etat Open doit
    /// se refleter sur l'ecran (code affiche, libelle du bouton) et publier une notice visible. Le
    /// resultat reel (Open vs ServicesUnavailable) depend de l'environnement Steam de la machine qui
    /// execute le test, comme pour Story21OnlineServicesPlayModeTests : ce test verifie la resolution
    /// et le cablage, jamais un etat Steam precis.
    /// </summary>
    public sealed class Story22HostCreatedPrivateRoomPlayModeTests
    {
        private const int MaxSettleFrames = 300;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var survivor = RoadRageBootstrap.Instance;
            if (survivor != null)
            {
                var lobbyRoom = survivor.LobbyRoom;
                if (lobbyRoom != null && lobbyRoom.Status == LobbyRoomStatus.Open)
                {
                    lobbyRoom.CloseRoom();
                }

                Object.Destroy(survivor.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateLobbyReachesATerminalRoomStatusAndNeverStaysStuckCreating()
        {
            yield return PressPlayAndEnterLobbyShell();

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            var bootstrap = RoadRageBootstrap.Instance;
            Assert.That(screen, Is.Not.Null);
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.LobbyRoom, Is.Not.Null);

            ClickSerializedButton(screen, "createLobbyButton");

            yield return WaitUntilSettled(bootstrap.LobbyRoom);

            Assert.That(bootstrap.LobbyRoom.Status, Is.Not.EqualTo(LobbyRoomStatus.Creating), "Create Lobby ne doit jamais rester bloque sur Creating");

            if (bootstrap.OnlineServices.Status != OnlineServicesStatus.Online)
            {
                Assert.That(bootstrap.LobbyRoom.Status, Is.EqualTo(LobbyRoomStatus.ServicesUnavailable), "sans services en ligne Online, la creation doit se refuser explicitement");
            }
        }

        [UnityTest]
        public IEnumerator OpenRoomShowsJoinCodeAndCloseButtonLabel()
        {
            yield return PressPlayAndEnterLobbyShell();

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            var bootstrap = RoadRageBootstrap.Instance;

            ClickSerializedButton(screen, "createLobbyButton");
            yield return WaitUntilSettled(bootstrap.LobbyRoom);

            if (bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open)
            {
                Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier l'etat Open.");
                yield break;
            }

            var roomCodeLabel = (TMP_Text)GetPrivateField(screen, "roomCodeLabel");
            var createLabel = (TMP_Text)GetPrivateField(screen, "createLobbyButtonLabel");

            Assert.That(roomCodeLabel.text, Does.Contain(bootstrap.LobbyRoom.JoinCode.ToString()), "le code affiche doit correspondre au JoinCode ouvert");
            Assert.That(createLabel.text, Is.EqualTo("Close Room"));
        }

        [UnityTest]
        public IEnumerator ClosingAnOpenRoomResetsScreenState()
        {
            yield return PressPlayAndEnterLobbyShell();

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            var bootstrap = RoadRageBootstrap.Instance;

            ClickSerializedButton(screen, "createLobbyButton");
            yield return WaitUntilSettled(bootstrap.LobbyRoom);

            if (bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open)
            {
                Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier la fermeture d'une room ouverte.");
                yield break;
            }

            ClickSerializedButton(screen, "createLobbyButton");
            yield return null;

            Assert.That(bootstrap.LobbyRoom.Status, Is.EqualTo(LobbyRoomStatus.Closed));
            Assert.That(bootstrap.LobbyRoom.JoinCode, Is.EqualTo(0UL));

            var roomCodeLabel = (TMP_Text)GetPrivateField(screen, "roomCodeLabel");
            var createLabel = (TMP_Text)GetPrivateField(screen, "createLobbyButtonLabel");
            Assert.That(roomCodeLabel.text, Is.Empty);
            Assert.That(createLabel.text, Is.EqualTo("Create Lobby"));
        }

        private static IEnumerator WaitUntilSettled(LobbyRoomService lobbyRoom)
        {
            var frames = 0;
            while (lobbyRoom.Status == LobbyRoomStatus.Creating && frames < MaxSettleFrames)
            {
                yield return null;
                frames++;
            }
        }

        private static IEnumerator PressPlayAndEnterLobbyShell()
        {
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen introuvable dans MainMenuLobby");

            ClickSerializedButton(menuScreen, "playButton");
            yield return null;
        }

        private static void ClickSerializedButton(Component screen, string buttonFieldName)
        {
            var button = GetPrivateField(screen, buttonFieldName) as Button;
            Assert.That(button != null, Is.True, screen.GetType().Name + "." + buttonFieldName + " doit referencer un Button de la scene");
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
