using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Definitions;
using RoadRage.Shared.Presentation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    public sealed class Story15EmptyMapEntryPlayModeTests
    {
        /// <summary>
        /// Story 5.3 (AD-26) : Start Game demarre desormais un vrai NetworkManager (StartHost), la ou
        /// avant cette story le solo ne touchait jamais au reseau. NetworkManager gere sa propre
        /// survie (DontDestroyOnLoad) independamment de RoadRageBootstrap : sans arret explicite ici,
        /// une session hote laissee active continue de faire tourner FacepunchTransport a chaque frame
        /// et pollue les tests suivants (StartGameWithProfileLoadsMvpRunAndSpawnsLocalOnFootPlayer).
        /// </summary>
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

            yield return null;
        }

        /// <summary>
        /// Depuis la Story 4.5, le menu principal publie toujours un profil (identite Steam, ou repli
        /// en memoire non persiste quand Steam est indisponible). Le cas « Start Game sans profil »
        /// n'est donc plus atteignable depuis l'interface : le refus de LobbyFlowController reste en
        /// place comme defense en profondeur et sa garde de source est verifiee en EditMode
        /// (Story15EmptyMapEntryTests). Ce test verrouille desormais la garantie qui remplace ce cas.
        /// </summary>
        [UnityTest]
        public IEnumerator MenuAlwaysPublishesAProfileBeforeLobbyEntry()
        {
            yield return EnterLobbyShell();

            Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.True,
                "le menu doit publier un profil avant toute entree dans le lobby");

            Assert.That(RoadRageBootstrap.Instance.Profiles.Current.CharacterId.IsEmpty, Is.False,
                "le profil publie doit porter un personnage exploitable par l'entree monde");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));
        }

        /// <summary>
        /// Depuis la Story 5.3 (AD-26), Start Game suit le meme chemin que Create Lobby : creation
        /// d'un lobby prive Steam puis StartHost() avant le chargement reseau de MVP_Run. Le resultat
        /// reel (succes vs ServicesUnavailable) depend de l'environnement Steam de la machine qui
        /// execute le test, comme Story21OnlineServicesPlayModeTests/Story22HostCreatedPrivateRoomPlayModeTests :
        /// ce test verifie la resolution et le cablage, jamais un etat Steam precis.
        /// </summary>
        [UnityTest]
        public IEnumerator StartGameWithProfileLoadsMvpRunAndSpawnsLocalOnFootPlayer()
        {
            yield return EnterLobbyShell(new PlayerProfile("Kenan", new DefinitionId("char_rookie")));

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(screen, Is.Not.Null);

            // Story 5.3 : Start Game demarre desormais un vrai NetworkManager (StartHost). Sur une
            // machine sans Steam P2P pleinement fonctionnel, FacepunchTransport peut logguer une
            // exception a chaque frame de polling reseau sans que la resolution logique du chemin
            // (lobby -> host -> MVP_Run) ne soit en cause -- ignore transitoirement les logs d'erreur
            // pendant cette fenetre reseau sensible, toujours restaure avant les assertions finales.
            LogAssert.ignoreFailingMessages = true;
            try
            {
                ClickSerializedButton(screen, "startGameButton");

                var bootstrap = RoadRageBootstrap.Instance;
                var frames = 0;
                while (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frames < 300)
                {
                    if (bootstrap != null && bootstrap.LobbyRoom != null
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                    {
                        Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier le demarrage reseau de Start Game.");
                        yield break;
                    }

                    yield return null;
                    frames++;
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

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

        /// <summary>
        /// Depuis la Story 4.6, le clic Play gele la selection de personnage : un profil a imposer pour
        /// le run doit donc etre publie avant ce clic, jamais apres (une mutation sous gel est refusee).
        /// </summary>
        private static IEnumerator EnterLobbyShell(PlayerProfile profileBeforePlay = null)
        {
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen introuvable dans MainMenuLobby");

            if (profileBeforePlay != null)
            {
                Assert.That(RoadRageBootstrap.Instance.Profiles.Set(profileBeforePlay), Is.True,
                    "le profil doit rester publiable tant que le lobby n'est pas entre");
            }

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
