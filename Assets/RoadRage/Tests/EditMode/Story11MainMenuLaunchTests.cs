using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.MainMenu;
using RoadRage.App.Services;
using RoadRage.Features.UI;
using RoadRage.Shared.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    public sealed class Story11MainMenuLaunchTests
    {
        private const string BootstrapScenePath = "Assets/RoadRage/App/Scenes/Bootstrap.unity";
        private const string MainMenuLobbyScenePath = "Assets/RoadRage/App/Scenes/MainMenuLobby.unity";

        [Test]
        public void UserNoticeSeverityHasExpectedLevels()
        {
            Assert.That(Enum.IsDefined(typeof(UserNoticeSeverity), UserNoticeSeverity.Info), Is.True);
            Assert.That(Enum.IsDefined(typeof(UserNoticeSeverity), UserNoticeSeverity.Warning), Is.True);
            Assert.That(Enum.IsDefined(typeof(UserNoticeSeverity), UserNoticeSeverity.Error), Is.True);
        }

        [Test]
        public void UserNoticeChannelPublishesAndTracksLastNotice()
        {
            var channel = new UserNoticeChannel();
            UserNotice? published = null;

            channel.NoticePublished += notice => published = notice;

            var notice = new UserNotice(UserNoticeSeverity.Warning, "Test notice");
            channel.Publish(notice);

            Assert.That(published, Is.EqualTo(notice));
            Assert.That(channel.LastNotice, Is.EqualTo(notice));
        }

        [Test]
        public void UserNoticeChannelClearResetsLastNotice()
        {
            var channel = new UserNoticeChannel();
            channel.Publish(new UserNotice(UserNoticeSeverity.Error, "Boom"));

            channel.Clear();

            Assert.That(channel.LastNotice, Is.Null);
        }

        [Test]
        public void AppSceneRouterSceneNamesMatchEditorBuildSettingsOrder()
        {
            var enabledSceneNames = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();

            CollectionAssert.AreEqual(
                new[] { AppSceneRouter.BootstrapSceneName, AppSceneRouter.MainMenuLobbySceneName, AppSceneRouter.MvpRunSceneName },
                enabledSceneNames);
        }

        [Test]
        public void BootstrapSceneContainsRoadRageBootstrapComponent()
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Additive);
            try
            {
                var bootstrap = FindComponentInScene<RoadRageBootstrap>(scene);
                Assert.That(bootstrap, Is.Not.Null, "RoadRageBootstrap component expected in Bootstrap scene");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void MainMenuLobbySceneContainsWiredComponents()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuLobbyScenePath, OpenSceneMode.Additive);
            try
            {
                var screen = FindComponentInScene<MainMenuScreen>(scene);
                Assert.That(screen, Is.Not.Null, "MainMenuScreen expected in MainMenuLobby scene");
                AssertSerializedObjectFieldsNonNull(screen);

                var flowController = FindComponentInScene<MainMenuFlowController>(scene);
                Assert.That(flowController, Is.Not.Null, "MainMenuFlowController expected in MainMenuLobby scene");
                AssertSerializedObjectFieldsNonNull(flowController);

                var eventSystem = FindComponentInScene<UnityEngine.EventSystems.EventSystem>(scene);
                Assert.That(eventSystem, Is.Not.Null, "EventSystem expected in MainMenuLobby scene");

                var uiInputModule = FindComponentInScene<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(scene);
                Assert.That(uiInputModule, Is.Not.Null, "InputSystemUIInputModule expected on the EventSystem");

                var canvas = FindComponentInScene<Canvas>(scene);
                Assert.That(canvas, Is.Not.Null, "Canvas expected in MainMenuLobby scene");
                Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));

                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                Assert.That(scaler, Is.Not.Null, "CanvasScaler expected on the Canvas");
                Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920, 1080)));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void UiFeatureSourceNeverCallsSceneManagementOrApplicationQuit()
        {
            var uiSourceFiles = Directory.GetFiles("Assets/RoadRage/Features/UI", "*.cs", SearchOption.AllDirectories);
            Assert.That(uiSourceFiles.Length, Is.GreaterThan(0));

            foreach (var file in uiSourceFiles)
            {
                var source = File.ReadAllText(file);
                Assert.That(source, Does.Not.Contain("SceneManager.LoadScene"), file);
                Assert.That(source, Does.Not.Contain("Application.Quit"), file);
            }
        }

        [Test]
        public void QuitApplicationIsGuardedAndOnlyCallsApplicationQuitOutsideTheEditor()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs");

            Assert.That(source, Does.Contain("#if UNITY_EDITOR"), "build-only Quit must be guarded out of Editor Play Mode");
            Assert.That(source, Does.Contain("Application.Quit();"), "a build must still be able to quit");

            var editorGuardIndex = source.IndexOf("#if UNITY_EDITOR", StringComparison.Ordinal);
            var quitCallIndex = source.IndexOf("Application.Quit();", StringComparison.Ordinal);
            var elseIndex = source.IndexOf("#else", editorGuardIndex, StringComparison.Ordinal);

            Assert.That(elseIndex, Is.GreaterThan(editorGuardIndex));
            Assert.That(quitCallIndex, Is.GreaterThan(elseIndex), "Application.Quit() must sit in the non-Editor branch, not the Editor branch");
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                var component = rootObject.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static void AssertSerializedObjectFieldsNonNull(Component component)
        {
            var fields = component.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                .Where(field => typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType));

            foreach (var field in fields)
            {
                var value = field.GetValue(component) as UnityEngine.Object;
                Assert.That(value, Is.Not.Null, component.GetType().Name + "." + field.Name);
            }
        }
    }
}
