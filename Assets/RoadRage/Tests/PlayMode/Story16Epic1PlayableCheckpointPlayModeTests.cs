using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.MainMenu;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    public sealed class Story16Epic1PlayableCheckpointPlayModeTests
    {
        private string originalProfileFilePath;

        private string tempProfileFilePath;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
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
        public IEnumerator BootstrapToWorldCompletesEpic1PlayableCheckpoint()
        {
            var blockingLogs = new List<string>();
            Application.logMessageReceived += CaptureBlockingLog;

            // Le profil persistant est redirige hors du dossier utilisateur avant le chargement du
            // bootstrap : le checkpoint ne doit ni lire ni ecraser le profil reel de la machine.
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story45-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;

            try
            {
                SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
                yield return null;
                yield return null;

                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));
                Assert.That(RoadRageBootstrap.Instance, Is.Not.Null, "Bootstrap persistant attendu apres routage");

                var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
                Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");

                // Depuis la Story 4.5, le menu principal resout le profil et porte la seule selection de
                // personnage : plus aucun ecran de creation, plus aucune saisie de nom.
                var profileFlow = Object.FindAnyObjectByType<MainMenuProfileFlowController>();
                Assert.That(profileFlow, Is.Not.Null, "MainMenuProfileFlowController attendu dans le menu");

                var catalog = GetCatalog(profileFlow);
                Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "au moins deux personnages selectionnables attendus");

                var bootstrap = RoadRageBootstrap.Instance;
                Assert.That(bootstrap.Profiles.HasProfile, Is.True, "le menu doit resoudre un profil utilisable des l'ouverture");

                ClickSerializedButton(menuScreen, "secondaryCharacterButton");
                yield return null;

                var selectedCharacter = catalog.GetAt((int)MainMenuScreen.CharacterOption.Secondary);
                Assert.That(selectedCharacter, Is.Not.Null, "personnage selectionne attendu");
                Assert.That(bootstrap.Profiles.Current.CharacterId, Is.EqualTo(selectedCharacter.Id),
                    "le clic sur un emplacement du menu doit publier ce personnage");

                var displayName = bootstrap.Profiles.Current.DisplayName;
                Assert.That(displayName, Is.Not.Empty, "le profil resolu doit porter un nom affichable");

                ClickSerializedButton(menuScreen, "playButton");
                yield return null;

                var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
                var lobbyFlow = Object.FindAnyObjectByType<LobbyFlowController>();
                Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu apres Play");
                Assert.That(lobbyFlow, Is.Not.Null, "LobbyFlowController attendu apres Play");
                Assert.That(lobbyFlow.Settings.Difficulty, Is.EqualTo(Difficulty.Normal));

                // Create Lobby n'est plus un stub depuis la Story 2.2 : le checkpoint verifie desormais
                // seulement que le clic fait evoluer LobbyRoomService vers un etat terminal, jamais
                // bloque sur Creating. Couverture complete (code affiche, notice, fermeture) dans
                // Story22HostCreatedPrivateRoomPlayModeTests.
                ClickSerializedButton(lobbyScreen, "createLobbyButton");

                var roomSettleFrames = 0;
                while (bootstrap.LobbyRoom.Status == LobbyRoomStatus.Creating && roomSettleFrames < 300)
                {
                    yield return null;
                    roomSettleFrames++;
                }

                Assert.That(bootstrap.LobbyRoom.Status, Is.Not.EqualTo(LobbyRoomStatus.Creating),
                    "Create Lobby ne doit jamais rester bloque sur Creating");

                if (bootstrap.LobbyRoom.Status == LobbyRoomStatus.Open)
                {
                    bootstrap.LobbyRoom.CloseRoom();
                }

                ClickSerializedButton(lobbyScreen, "difficultyButton");
                yield return null;
                Assert.That(lobbyFlow.Settings.Difficulty, Is.Not.EqualTo(Difficulty.Normal),
                    "le checkpoint doit conserver un reglage lobby local modifiable");

                ClickSerializedButton(lobbyScreen, "startGameButton");
                yield return null;
                yield return null;

                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));

                var runFlow = Object.FindAnyObjectByType<RunFlowController>();
                Assert.That(runFlow, Is.Not.Null, "RunFlowController attendu dans MVP_Run");
                Assert.That(runFlow.ActiveLocalPlayer, Is.Not.Null, "le profil confirme doit produire un joueur local");

                var controller = runFlow.ActiveLocalPlayer.GetComponent<LocalOnFootController>();
                Assert.That(controller, Is.Not.Null, "le joueur local doit porter le controller a pied");
                Assert.That(runFlow.ActiveLocalPlayer.GetComponent<CharacterController>(), Is.Not.Null);
                Assert.That(controller.PlayerCamera, Is.Not.Null, "la camera locale doit etre attachee");

                var start = runFlow.ActiveLocalPlayer.transform.position;
                controller.Step(new OnFootMovementIntent(Vector2.up, Vector2.zero, true), 0.25f);
                Assert.That(runFlow.ActiveLocalPlayer.transform.position.z, Is.GreaterThan(start.z + 0.5f),
                    "le joueur doit pouvoir sprinter dans la carte vide");

                var hud = Object.FindAnyObjectByType<RunCheckpointHudScreen>();
                Assert.That(hud, Is.Not.Null, "HUD placeholder attendu dans MVP_Run");
                AssertSerializedTextContains(hud, "lobbyStateLabel", RunCheckpointHudScreen.LocalLobbyState);
                AssertSerializedTextContains(hud, "playerStateLabel", displayName);
                AssertSerializedTextContains(hud, "playerStateLabel", selectedCharacter.DisplayName);
                AssertSerializedTextContains(hud, "futureHudLabel", RunCheckpointHudScreen.FutureHudState);

                Assert.That(blockingLogs, Is.Empty, string.Join("\n", blockingLogs));
            }
            finally
            {
                Application.logMessageReceived -= CaptureBlockingLog;
            }

            void CaptureBlockingLog(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
                {
                    blockingLogs.Add(type + ": " + condition);
                }
            }
        }

        private static CharacterCatalog GetCatalog(MainMenuProfileFlowController flow)
        {
            var catalog = GetPrivateField(flow, "catalog") as CharacterCatalog;
            Assert.That(catalog != null, Is.True, "MainMenuProfileFlowController.catalog doit etre cable");
            return catalog;
        }

        private static void ClickSerializedButton(Component component, string buttonFieldName)
        {
            var button = GetPrivateField(component, buttonFieldName) as Button;
            Assert.That(button != null, Is.True, component.GetType().Name + "." + buttonFieldName + " doit referencer un Button");
            button.onClick.Invoke();
        }

        private static void AssertSerializedTextContains(Component component, string fieldName, string expectedText)
        {
            var label = GetPrivateField(component, fieldName) as TMP_Text;
            Assert.That(label != null, Is.True, component.GetType().Name + "." + fieldName + " doit referencer un TMP_Text");
            Assert.That(label.text, Does.Contain(expectedText), fieldName);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }
    }
}
