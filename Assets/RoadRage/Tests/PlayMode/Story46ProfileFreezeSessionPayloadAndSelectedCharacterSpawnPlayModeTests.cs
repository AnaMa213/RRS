using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.MainMenu;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.Lobby;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Definitions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 4.6 : seule preuve bout en bout cote solo. Le personnage choisi dans le menu reel est gele a
    /// l'entree du lobby, refuse toute mutation ensuite, et MVP_Run spawne ce personnage et sa presentation.
    /// Le chemin du profil est redirige vers un fichier temporaire avant tout chargement de scene : le test
    /// ne doit ni lire ni ecraser le profil reel de la machine.
    /// </summary>
    public sealed class Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests
    {
        private string originalProfileFilePath;

        private string tempProfileFilePath;

        /// <summary>
        /// Story 5.3 (AD-26) : Start Game demarre desormais un vrai NetworkManager (StartHost).
        /// NetworkManager gere sa propre survie (DontDestroyOnLoad) independamment de RoadRageBootstrap :
        /// sans arret explicite ici, une session hote laissee active continue de faire tourner
        /// FacepunchTransport a chaque frame et pollue les tests suivants (meme risque que Story15).
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
                var lobbyRoom = survivor.LobbyRoom;
                if (lobbyRoom != null && lobbyRoom.Status == LobbyRoomStatus.Open)
                {
                    lobbyRoom.CloseRoom();
                }

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
        public IEnumerator FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable()
        {
            RedirectProfilePath();
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var bootstrap = RoadRageBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null, "RoadRageBootstrap attendu apres chargement du menu");
            Assert.That(bootstrap.Profiles.IsFrozen, Is.False, "le menu ouvert doit laisser la selection modifiable");

            var menu = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menu, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");

            var catalog = GetCatalog(Object.FindAnyObjectByType<MainMenuProfileFlowController>());
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "deux emplacements cosmetiques attendus");

            // 1. Choisir un personnage non defaut dans le menu, seule surface de selection.
            ClickSerializedButton(menu, "secondaryCharacterButton");
            yield return null;

            var selectedCharacter = catalog.GetAt((int)MainMenuScreen.CharacterOption.Secondary);
            Assert.That(selectedCharacter, Is.Not.Null, "le second emplacement doit pointer un personnage");
            Assert.That(bootstrap.Profiles.Current.CharacterId, Is.EqualTo(selectedCharacter.Id));

            // 2. Entrer dans le lobby (Play) : la selection est gelee sur cet instantane.
            ClickSerializedButton(menu, "playButton");
            yield return null;

            Assert.That(bootstrap.Profiles.IsFrozen, Is.True, "Play doit geler la selection a l'entree du lobby");
            Assert.That(bootstrap.Profiles.SessionSelection, Is.Not.Null, "la session doit porter la selection affichee");
            Assert.That(bootstrap.Profiles.SessionSelection.CharacterId, Is.EqualTo(selectedCharacter.Id));

            // 3. Plus aucune mutation n'est acceptee, meme via l'emplacement masque du menu.
            var profileChangedCount = 0;
            bootstrap.Profiles.ProfileChanged += profile => profileChangedCount++;

            var fileBeforeRefusedClick = File.Exists(tempProfileFilePath) ? File.ReadAllText(tempProfileFilePath) : null;

            ClickSerializedButton(menu, "primaryCharacterButton");
            yield return null;

            Assert.That(bootstrap.Profiles.Current.CharacterId, Is.EqualTo(selectedCharacter.Id),
                "un depot gele ne doit pas muter, meme si le menu est masque");
            Assert.That(bootstrap.Profiles.SessionSelection.CharacterId, Is.EqualTo(selectedCharacter.Id));
            Assert.That(profileChangedCount, Is.Zero, "un refus de gel ne doit publier aucun ProfileChanged");

            // Le refus ne protege pas seulement la memoire : il doit aussi tenir le disque a l'ecart,
            // et rester visible au joueur (Story 4.6, lignes « mutation sous gel » et « ecriture disque »).
            var fileAfterRefusedClick = File.Exists(tempProfileFilePath) ? File.ReadAllText(tempProfileFilePath) : null;
            Assert.That(fileAfterRefusedClick, Is.EqualTo(fileBeforeRefusedClick),
                "un changement refuse sous gel ne doit rien ecrire sur disque");
            Assert.That(bootstrap.Notices.LastNotice.HasValue, Is.True,
                "le refus sous gel doit publier une notice visible, jamais seulement un log");
            Assert.That(bootstrap.Notices.LastNotice.Value.Message, Is.EqualTo(MainMenuProfileFlowController.SelectionFrozenMessage),
                "la notice du refus doit nommer le gel de selection");

            // 4. Start Game solo : MVP_Run doit spawner la selection gelee, jamais le profil courant.
            var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu apres Play");

            // Story 5.3 : Start Game demarre desormais un vrai NetworkManager (StartHost), qui reste
            // actif jusqu'au TearDown (l'etape 6 recharge MainMenuLobby sans l'arreter, pour isoler
            // uniquement le degel du profil). Sur une machine sans Steam P2P pleinement fonctionnel,
            // FacepunchTransport peut logguer une exception a chaque frame de polling reseau tout du
            // long sans que la logique testee ici ne soit en cause -- ignore ces logs jusqu'a la fin
            // du test, meme mitigation que Story15/Story16.
            LogAssert.ignoreFailingMessages = true;
            try
            {
                ClickSerializedButton(lobbyScreen, "startGameButton");

                // Aucune room n'a ete ouverte au prealable : ce clic cree un lobby prive de zero. La
                // creation Steam est asynchrone (callback pompe via Tick()), donc on attend l'etat
                // terminal plutot qu'un nombre de frames fixe, comme Story15EmptyMapEntryPlayModeTests.
                var startGameFrames = 0;
                while (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && startGameFrames < 300)
                {
                    if (bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                    {
                        Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier le demarrage reseau de Start Game.");
                        yield break;
                    }

                    yield return null;
                    startGameFrames++;
                }

                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));

                var runFlow = Object.FindAnyObjectByType<RunFlowController>();
                Assert.That(runFlow, Is.Not.Null, "RunFlowController attendu dans MVP_Run");
                Assert.That(runFlow.ActiveLocalPlayer, Is.Not.Null, "la selection gelee doit produire un joueur local");
                Assert.That(runFlow.ActiveLocalPlayer.name, Does.Contain(selectedCharacter.RawId),
                    "le spawn solo doit consommer la selection gelee");

                var visual = runFlow.ActiveLocalPlayer.transform.Find(selectedCharacter.PreviewPrefab.name + "_Visual");
                Assert.That(visual, Is.Not.Null, "le spawn doit instancier le PreviewPrefab du personnage selectionne");

                // 5. Le gel appartient a la session : une mutation tardive reste refusee.
                Assert.That(bootstrap.Profiles.IsFrozen, Is.True);
                Assert.That(bootstrap.Profiles.Set(new PlayerProfile("Kenan", catalog.GetAt(0).Id)), Is.False,
                    "l'etat de session ne doit jamais laisser remplacer la selection gelee");
                Assert.That(runFlow.ActiveLocalPlayer.name, Does.Contain(selectedCharacter.RawId));

                // 6. Le menu rouvert leve le gel herite et redevient modifiable.
                SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
                yield return null;
                yield return null;

                bootstrap = RoadRageBootstrap.Instance;
                Assert.That(bootstrap.Profiles.IsFrozen, Is.False, "la reouverture du menu doit lever le gel");
                Assert.That(bootstrap.Profiles.HasProfile, Is.True, "le menu doit restaurer un profil utilisable");

                var reopenedMenu = Object.FindAnyObjectByType<MainMenuScreen>();
                Assert.That(reopenedMenu, Is.Not.Null);
                var reopenedCatalog = GetCatalog(Object.FindAnyObjectByType<MainMenuProfileFlowController>());

                ClickSerializedButton(reopenedMenu, "primaryCharacterButton");
                yield return null;

                Assert.That(bootstrap.Profiles.IsFrozen, Is.False);
                Assert.That(bootstrap.Profiles.Current.CharacterId, Is.EqualTo(reopenedCatalog.GetAt(0).Id),
                    "apres la reouverture du menu, la selection doit redevenir modifiable");
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        /// <summary>
        /// Entree du lobby hote : la room ouverte gele la selection, independamment du gel de Play.
        /// Le degel de Play est leve apres coup pour isoler le chemin teste. La branche invite (join
        /// `Joined`) reste couverte par la garde de source EditMode : elle exige une room distante.
        /// </summary>
        [UnityTest]
        public IEnumerator HostRoomOpenFreezesTheSelectionOnItsOwn()
        {
            RedirectProfilePath();
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var bootstrap = RoadRageBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null, "RoadRageBootstrap attendu apres chargement du menu");

            var menu = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menu, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");

            ClickSerializedButton(menu, "playButton");
            yield return null;

            var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu apres Play");

            // Isolation : on repart d'un etat pre-room modifiable pour n'observer que le gel de la room.
            bootstrap.Profiles.Unfreeze();
            Assert.That(bootstrap.Profiles.IsFrozen, Is.False);

            ClickSerializedButton(lobbyScreen, "createLobbyButton");

            var settleFrames = 0;
            while (bootstrap.LobbyRoom.Status == LobbyRoomStatus.Creating && settleFrames < 300)
            {
                yield return null;
                settleFrames++;
            }

            if (bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open)
            {
                Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier l'etat Open.");
                yield break;
            }

            Assert.That(bootstrap.Profiles.IsFrozen, Is.True, "une room ouverte doit geler la selection sans passer par Play");
            Assert.That(bootstrap.Profiles.Set(new PlayerProfile("Kenan", new DefinitionId("char_rookie"))), Is.False,
                "le lobby hote ne doit plus pouvoir modifier la selection gelee");
        }

        /// <summary>
        /// Redirige le fichier de profil avant tout chargement de scene : le bootstrap lit le chemin par
        /// defaut a sa creation, donc l'override doit preceder le chargement.
        /// </summary>
        private void RedirectProfilePath()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story46-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;
        }

        private static CharacterCatalog GetCatalog(MainMenuProfileFlowController flow)
        {
            Assert.That(flow, Is.Not.Null, "MainMenuProfileFlowController attendu dans la scene menu");
            var catalog = GetPrivateField(flow, "catalog") as CharacterCatalog;
            Assert.That(catalog, Is.Not.Null, "MainMenuProfileFlowController.catalog doit etre cable");
            return catalog;
        }

        private static void ClickSerializedButton(Component component, string buttonFieldName)
        {
            var button = GetPrivateField(component, buttonFieldName) as Button;
            Assert.That(button, Is.Not.Null, component.GetType().Name + "." + buttonFieldName + " doit referencer un Button");
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
