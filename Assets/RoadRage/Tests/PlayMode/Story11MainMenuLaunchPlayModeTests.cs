using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Services;
using RoadRage.Features.UI;
using RoadRage.Shared.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Couvre les comportements de la Story 1.1 qui dependent du cycle de vie Unity
    /// (Awake/Start/Destroy) et ne sont donc pas observables par des tests EditMode purs :
    /// routage Bootstrap -> MainMenuLobby, singleton anti-doublon, cablage des intentions UI.
    /// </summary>
    public sealed class Story11MainMenuLaunchPlayModeTests
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
        public IEnumerator RoutingFromBootstrapEntersMainMenuLobby()
        {
            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));
            Assert.That(RoadRageBootstrap.Instance, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator DuplicateBootstrapSelfDestroysAndPreservesFirst()
        {
            var first = RoadRageBootstrap.EnsureInstance();
            yield return null;

            var firstRouter = first.Router;
            var firstNotices = first.Notices;

            LogAssert.Expect(LogType.Warning, "[App] Seconde instance de RoadRageBootstrap detectee, destruction de la seconde ; la premiere reste la reference.");

            var duplicateGameObject = new GameObject("DuplicateRoadRageBootstrap");
            duplicateGameObject.AddComponent<RoadRageBootstrap>();
            yield return null;

            Assert.That(RoadRageBootstrap.Instance, Is.SameAs(first));
            Assert.That(RoadRageBootstrap.Instance.Router, Is.SameAs(firstRouter));
            Assert.That(RoadRageBootstrap.Instance.Notices, Is.SameAs(firstNotices));
            Assert.That(duplicateGameObject == null, Is.True, "the duplicate GameObject must be destroyed");
        }

        [UnityTest]
        public IEnumerator MainMenuFlowControllerWiringRelaysScreenIntentAndNotices()
        {
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var screen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(screen, Is.Not.Null);

            var menuPanel = (GameObject)GetPrivateField(screen, "menuPanel");
            var setupPanel = (GameObject)GetPrivateField(screen, "setupPanel");
            var noticePanel = (GameObject)GetPrivateField(screen, "noticePanel");

            Assert.That(menuPanel.activeSelf, Is.True, "menu panel must be visible right after Awake");
            Assert.That(setupPanel.activeSelf, Is.False);

            InvokePrivateMethod(screen, "RaisePlayRequested");
            Assert.That(setupPanel.activeSelf, Is.True, "MainMenuFlowController must wire PlayRequested to ShowSetupPlaceholder");
            Assert.That(menuPanel.activeSelf, Is.False);

            InvokePrivateMethod(screen, "RaiseBackRequested");
            Assert.That(menuPanel.activeSelf, Is.True, "MainMenuFlowController must wire BackRequested to ShowMenu");

            Assert.That(RoadRageBootstrap.Instance, Is.Not.Null);
            Assert.That(RoadRageBootstrap.Instance.Notices, Is.Not.Null);
            RoadRageBootstrap.Instance.Notices.Publish(new UserNotice(UserNoticeSeverity.Warning, "Test PlayMode"));
            Assert.That(noticePanel.activeSelf, Is.True, "MainMenuFlowController must wire NoticePublished to ShowNotice");

            InvokePrivateMethod(screen, "RaisePlayRequested");
            InvokePrivateMethod(screen, "RaiseBackRequested");
            Assert.That(noticePanel.activeSelf, Is.True, "returning to the menu must restore the latest visible notice");

            LogAssert.Expect(LogType.Log, "[App] Quit demande en Play Mode Editor : aucune fermeture d'Editor declenchee.");
            InvokePrivateMethod(screen, "RaiseQuitRequested");
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "method not found: " + methodName);
            method.Invoke(target, null);
        }
    }
}
