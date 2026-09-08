using System.Collections;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Couvre la Story 2.1 sur le vrai cycle de vie Unity : l'ouverture du flux de lobby doit resoudre
    /// le statut des services en ligne vers un etat terminal et publier une notice visible via le
    /// canal existant. Le resultat reel (Online/InitializationFailed/SignInFailed/Offline) depend de
    /// l'environnement Steam de la machine qui execute le test : ce test verifie la resolution et la
    /// notification, jamais un etat precis.
    /// </summary>
    public sealed class Story21OnlineServicesPlayModeTests
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
        public IEnumerator OpeningLobbyFlowResolvesOnlineServicesStatusToATerminalState()
        {
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var bootstrap = RoadRageBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.OnlineServices, Is.Not.Null);
            Assert.That(bootstrap.OnlineServices.Status, Is.Not.EqualTo(OnlineServicesStatus.NotStarted), "l'ouverture du flux de lobby doit avoir declenche TryInitialize");
        }

        /// <summary>
        /// La notice se publie pendant Awake, au chargement de la scene lui-meme : un abonnement pris
        /// apres coup manquerait l'evenement. LastNotice, mis a jour par tout Publish quel que soit
        /// le nombre d'abonnes, evite cette course sans dependre de l'ordre d'execution des Awake.
        /// </summary>
        [UnityTest]
        public IEnumerator OpeningLobbyFlowPublishesAVisibleOnlineServicesNotice()
        {
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var bootstrap = RoadRageBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null);

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null);

            Assert.That(bootstrap.Notices.LastNotice, Is.Not.Null, "un statut de services en ligne resolu doit publier une notice visible");
            Assert.That(bootstrap.Notices.LastNotice.Value.Message, Is.Not.Empty);
        }
    }
}
