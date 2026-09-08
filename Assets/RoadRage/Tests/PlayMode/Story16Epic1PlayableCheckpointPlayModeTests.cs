using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.Players;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Domain;
using RoadRage.Shared.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    public sealed class Story16Epic1PlayableCheckpointPlayModeTests
    {
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var survivor = RoadRageBootstrap.Instance;
            if (survivor != null)
            {
                Object.Destroy(survivor.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator BootstrapToWorldCompletesEpic1PlayableCheckpoint()
        {
            var blockingLogs = new List<string>();
            Application.logMessageReceived += CaptureBlockingLog;

            try
            {
                SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
                yield return null;
                yield return null;

                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));
                Assert.That(RoadRageBootstrap.Instance, Is.Not.Null, "Bootstrap persistant attendu apres routage");

                var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
                Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");

                ClickSerializedButton(menuScreen, "playButton");
                yield return null;

                var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
                var lobbyFlow = Object.FindAnyObjectByType<LobbyFlowController>();
                Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu apres Play");
                Assert.That(lobbyFlow, Is.Not.Null, "LobbyFlowController attendu apres Play");
                Assert.That(lobbyFlow.Settings.Difficulty, Is.EqualTo(Difficulty.Normal));

                UserNotice? unavailableNotice = null;
                RoadRageBootstrap.Instance.Notices.NoticePublished += notice => unavailableNotice = notice;

                ClickSerializedButton(lobbyScreen, "createLobbyButton");
                yield return null;

                Assert.That(unavailableNotice, Is.Not.Null, "Create Lobby doit rester un stub visible");
                Assert.That(unavailableNotice.Value.Severity, Is.EqualTo(UserNoticeSeverity.Warning));
                Assert.That(unavailableNotice.Value.Message, Does.Contain("Epic 2"));

                ClickSerializedButton(lobbyScreen, "difficultyButton");
                yield return null;
                Assert.That(lobbyFlow.Settings.Difficulty, Is.Not.EqualTo(Difficulty.Normal),
                    "le checkpoint doit conserver un reglage lobby local modifiable");

                var profileFlow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
                Assert.That(profileFlow, Is.Not.Null, "PlayerProfileFlowController attendu dans le flux Epic 1");

                ClickSerializedButton(lobbyScreen, "characterSetupButton");
                yield return null;

                var setupScreen = GetScreen(profileFlow);
                var catalog = GetCatalog(profileFlow);
                Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "au moins deux personnages selectionnables attendus");

                ClickSerializedButton(setupScreen, "characterCycleButton");
                yield return null;

                var selectedCharacter = catalog.GetAt(profileFlow.CurrentIndex);
                Assert.That(selectedCharacter, Is.Not.Null, "personnage selectionne attendu");

                SetInputText(setupScreen, "Checkpoint");
                ClickSerializedButton(setupScreen, "confirmButton");
                yield return null;

                var profileStore = RoadRageBootstrap.Instance.Profiles;
                Assert.That(profileStore.HasProfile, Is.True, "un profil valide doit etre garde pour l'entree monde");
                Assert.That(profileStore.Current.DisplayName, Is.EqualTo("Checkpoint"));
                Assert.That(profileStore.Current.CharacterId, Is.EqualTo(selectedCharacter.Id));

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
                AssertSerializedTextContains(hud, "playerStateLabel", "Checkpoint");
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

        private static CharacterSetupScreen GetScreen(PlayerProfileFlowController flow)
        {
            var screen = GetPrivateField(flow, "characterSetupScreen") as CharacterSetupScreen;
            Assert.That(screen != null, Is.True, "PlayerProfileFlowController.characterSetupScreen doit etre cable");
            return screen;
        }

        private static CharacterCatalog GetCatalog(PlayerProfileFlowController flow)
        {
            var catalog = GetPrivateField(flow, "catalog") as CharacterCatalog;
            Assert.That(catalog != null, Is.True, "PlayerProfileFlowController.catalog doit etre cable");
            return catalog;
        }

        private static void SetInputText(CharacterSetupScreen screen, string value)
        {
            var input = GetPrivateField(screen, "nameInputField") as TMP_InputField;
            Assert.That(input != null, Is.True, "CharacterSetupScreen.nameInputField doit etre cable");
            input.text = value;
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
