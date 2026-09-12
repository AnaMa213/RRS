using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Definitions;
using RoadRage.Shared.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    public sealed class Story15EmptyMapEntryPlayModeTests
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
        public IEnumerator StartGameWithoutProfilePublishesVisibleWarningAndStaysInLobby()
        {
            yield return EnterLobbyShell();

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(screen, Is.Not.Null);

            UserNotice? published = null;
            RoadRageBootstrap.Instance.Notices.NoticePublished += notice => published = notice;

            ClickSerializedButton(screen, "startGameButton");
            yield return null;

            Assert.That(published, Is.Not.Null, "un refus Start Game sans profil doit etre visible");
            Assert.That(published.Value.Message, Is.EqualTo(LobbyFlowController.MissingProfileStartGameMessage));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));
        }

        [UnityTest]
        public IEnumerator StartGameWithProfileLoadsMvpRunAndSpawnsLocalOnFootPlayer()
        {
            yield return EnterLobbyShell();

            RoadRageBootstrap.Instance.Profiles.Set(new PlayerProfile("Kenan", new DefinitionId("char_rookie")));

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(screen, Is.Not.Null);

            ClickSerializedButton(screen, "startGameButton");
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));

            var runFlow = Object.FindAnyObjectByType<RunFlowController>();
            Assert.That(runFlow, Is.Not.Null, "RunFlowController doit exister dans MVP_Run");
            Assert.That(runFlow.ActiveLocalPlayer, Is.Not.Null, "le profil confirme doit produire un joueur local");
            Assert.That(runFlow.ActiveLocalPlayer.name, Does.Contain("char_rookie"));

            var controller = runFlow.ActiveLocalPlayer.GetComponent<LocalOnFootController>();
            Assert.That(controller, Is.Not.Null, "le joueur local doit pouvoir se deplacer a pied");
            Assert.That(runFlow.ActiveLocalPlayer.GetComponent<CharacterController>(), Is.Not.Null);
            Assert.That(controller.PlayerCamera, Is.Not.Null, "la camera locale doit etre attachee au controller");
            Assert.That(controller.PlayerCamera.transform.parent, Is.Null, "Main Camera stays independent during seat blends.");
        }

        [UnityTest]
        public IEnumerator LocalOnFootControllerWalksSprintsAndStopsFromIntent()
        {
            SceneManager.LoadScene(AppSceneRouter.MvpRunSceneName);
            yield return null;

            var player = new GameObject("OnFootMovementHarness");
            var characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 0.9f, 0f);

            var controller = player.AddComponent<LocalOnFootController>();
            yield return null;

            var start = player.transform.position;
            controller.Step(new OnFootMovementIntent(Vector2.up, Vector2.zero, false), 0.25f);
            var walked = player.transform.position;

            player.transform.position = start;
            controller.Step(new OnFootMovementIntent(Vector2.up, Vector2.zero, true), 0.25f);
            var sprinted = player.transform.position;

            var beforeIdle = player.transform.position;
            controller.Step(OnFootMovementIntent.Idle, 0.25f);
            var afterIdle = player.transform.position;

            Assert.That(walked.z, Is.GreaterThan(start.z + 0.5f), "marcher vers l'avant doit deplacer le joueur");
            Assert.That(sprinted.z, Is.GreaterThan(walked.z), "sprint doit aller plus loin que marche sur la meme duree");
            Assert.That(Mathf.Abs(afterIdle.z - beforeIdle.z), Is.LessThan(0.01f), "sans intention horizontale le joueur doit s'arreter");
        }

        private static IEnumerator EnterLobbyShell()
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
