using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Input;
using RoadRage.Shared.Presentation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.8 : preuve du chemin reel du menu d'echappement de MVP_Run. Les gardes EditMode
    /// decrivent le contrat ; ce fixture constate sur un pair reel que le panneau authored actif se
    /// masque a l'ouverture de la scene, qu'Echap (via le meme Toggle que Update appelle) et le bouton
    /// Resume rendent les entrees et restaurent le curseur capture, qu'aucune pause n'est introduite,
    /// et qu'un quit volontaire revient a MainMenuLobby par le teardown de session existant, sans
    /// notice d'erreur, avec un seul teardown meme si le quit est demande deux fois.
    ///
    /// Le quit est demarre par le chemin unique de la Story 5.3 (AD-26) : bootstrap -> menu -> lobby ->
    /// Start Game (StartHost puis chargement synchronise de MVP_Run). Comme Story15/16/57, une machine
    /// sans Steam P2P pleinement fonctionnel rend les tests concernes Inconclusifs plutot que rouges.
    /// Le cas a deux pairs (client qui quitte, hote et autres clients qui continuent) reste, lui,
    /// hors de portee du Test Runner : il est documente comme verification manuelle.
    /// </summary>
    [Category("Story58")]
    public sealed class Story58EscapeMenuPlayModeTests
    {
        private string originalProfileFilePath;

        private string tempProfileFilePath;

        /// <summary>
        /// Story 5.3 (AD-26) : Start Game demarre un vrai NetworkManager (StartHost) et le bootstrap
        /// survit en DontDestroyOnLoad. Sans arret explicite ici, la session hote et le bootstrap
        /// resteraient actifs et pollueraient les fixtures suivantes (meme risque que Story15/16/57).
        ///
        /// Le portail d'entree est statique et le curseur est un etat global : la fixture doit les
        /// rendre elle-meme, sinon un test qui echoue au milieu d'un menu ouvert laisse la suite (et
        /// l'editeur du developpeur) avec un jeu muet et un curseur capture.
        /// </summary>
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            LocalInputGate.Reset();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                if (manager.IsListening)
                {
                    manager.Shutdown();
                }

                UnityEngine.Object.Destroy(manager.gameObject);
            }

            var survivor = RoadRageBootstrap.Instance;
            if (survivor != null)
            {
                UnityEngine.Object.Destroy(survivor.gameObject);
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

        /// <summary>
        /// Chargement direct (comme Story 4.3) : ce chemin ne demande ni Steam ni lobby, donc aucun
        /// pair n'est requis. Le menu est un objet de scene : son etat authored, son affichage, son
        /// portail d'entree, son curseur et l'absence de pause se constatent ici.
        /// </summary>
        [UnityTest]
        public IEnumerator DirectMvpRunLoadHidesTheAuthoredPanelAndResumeRendersInput()
        {
            RedirectProfileFilePath();

            yield return SceneManager.LoadSceneAsync(AppSceneRouter.MvpRunSceneName, LoadSceneMode.Single);
            yield return null;

            var screen = UnityEngine.Object.FindAnyObjectByType<RunEscapeMenuScreen>(FindObjectsInactive.Include);
            Assert.That(screen, Is.Not.Null, "le panneau du menu est un objet de scene de MVP_Run");
            Assert.That(screen.gameObject.activeSelf, Is.False,
                "le panneau est authored ACTIF dans la scene (verifie en EditMode) et se masque dans son Awake() : au chargement de MVP_Run, le menu est ferme");

            var flow = UnityEngine.Object.FindAnyObjectByType<RunEscapeMenuFlowController>();
            Assert.That(flow, Is.Not.Null, "RunEscapeMenuFlowController attendu sur RunRoot");
            Assert.That(flow.IsOpen, Is.False);
            Assert.That(LocalInputGate.IsBlocked, Is.False, "aucune entree locale n'est bloquee au chargement");

            var timeScaleBefore = Time.timeScale;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            flow.Open();
            Assert.That(flow.IsOpen, Is.True);
            Assert.That(screen.gameObject.activeSelf, Is.True, "le panneau s'affiche");
            Assert.That(LocalInputGate.IsBlocked, Is.True,
                "les entrees locales sont bloquees : conduite, deplacement a pied, actions et orbite camera");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None), "le curseur est libere a l'ouverture");
            Assert.That(Cursor.visible, Is.True, "et rendu visible, sans quoi le menu serait incliquable");
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBefore),
                "ouvrir le menu ne met jamais la simulation en pause");

            // Clic reel sur le bouton de la scene : c'est le cablage pose par RunEscapeMenuScreen.Awake
            // (onClick -> ResumeRequested -> Close) qui est exerce ici, pas un appel direct.
            ClickSerializedButton(screen, "resumeButton");
            yield return null;

            Assert.That(flow.IsOpen, Is.False, "Resume ferme le menu");
            Assert.That(screen.gameObject.activeSelf, Is.False, "le panneau est masque");
            Assert.That(LocalInputGate.IsBlocked, Is.False, "les entrees sont rendues");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked),
                "le curseur retrouve exactement l'etat capture a l'ouverture");
            Assert.That(Cursor.visible, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBefore));

            // Echap est lu par Update() derriere une garde de clavier absente (garde de source en
            // EditMode) et bascule le menu par ce meme Toggle() : la bascule est verifiee ici.
            flow.Toggle();
            yield return null;
            Assert.That(flow.IsOpen, Is.True, "Echap ouvre quand le menu est ferme");
            Assert.That(LocalInputGate.IsBlocked, Is.True);

            flow.Toggle();
            yield return null;
            Assert.That(flow.IsOpen, Is.False, "Echap ferme quand le menu est ouvert");
            Assert.That(LocalInputGate.IsBlocked, Is.False);
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked));
            Assert.That(Cursor.visible, Is.False);
        }

        /// <summary>
        /// Quit en hote : le retour au menu passe par le teardown de session existant, sans notice
        /// d'erreur (une sortie volontaire n'est pas un echec), avec un seul teardown meme si le quit
        /// est demande deux fois, et le drapeau de sortie volontaire survit au changement de scene.
        /// </summary>
        [UnityTest]
        public IEnumerator HostQuitReturnsToTheLobbyWithoutAnErrorNoticeAndReleasesTheOpenMenu()
        {
            yield return EnterHostedRun();

            var screen = UnityEngine.Object.FindAnyObjectByType<RunEscapeMenuScreen>(FindObjectsInactive.Include);
            Assert.That(screen, Is.Not.Null, "le panneau du menu est un objet de scene de MVP_Run");
            var flow = UnityEngine.Object.FindAnyObjectByType<RunEscapeMenuFlowController>();
            Assert.That(flow, Is.Not.Null, "RunEscapeMenuFlowController attendu sur RunRoot");

            var monitor = UnityEngine.Object.FindAnyObjectByType<NetworkedRunSessionMonitor>();
            Assert.That(monitor, Is.Not.Null,
                "le moniteur est ajoute par RunFlowController.Start() : c'est lui, et lui seul, qui possede Shutdown() + LoadMainMenu()");

            var bootstrap = RoadRageBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null, "le bootstrap persistant doit exister dans une session hebergee");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            flow.Open();

            var timeScaleWhileOpen = Time.timeScale;
            Assert.That(flow.IsOpen, Is.True, "etat de depart : menu ouvert");
            Assert.That(LocalInputGate.IsBlocked, Is.True);

            var errorNotices = new List<UserNotice>();

            void CollectErrorNotice(UserNotice notice)
            {
                if (notice.Severity == UserNoticeSeverity.Error)
                {
                    errorNotices.Add(notice);
                }
            }

            bootstrap.Notices.NoticePublished += CollectErrorNotice;
            try
            {
                Assert.That(monitor.RequestVoluntaryExitToMainMenu(), Is.True,
                    "premier quit : le teardown de session demarre");
                Assert.That(RoadRageBootstrap.Instance.SessionExitRequested, Is.True,
                    "le moniteur marque la sortie volontaire, sans quoi le lobby relancerait immediatement le joueur dans la run");
                Assert.That(monitor.RequestVoluntaryExitToMainMenu(), Is.False,
                    "second quit : un retour est deja en cours, il est ignore (un seul Shutdown() suivi d'un seul LoadMainMenu())");
                Assert.That(Time.timeScale, Is.EqualTo(timeScaleWhileOpen),
                    "quitter n'introduit aucune pause : la simulation continue jusqu'au changement de scene");

                var frames = 0;
                while (SceneManager.GetActiveScene().name != AppSceneRouter.MainMenuLobbySceneName && frames < 300)
                {
                    yield return null;
                    frames++;
                }
            }
            finally
            {
                bootstrap.Notices.NoticePublished -= CollectErrorNotice;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName),
                "Quit to Main Menu revient au menu principal par le chemin de sortie de session existant");
            Assert.That(RoadRageBootstrap.Instance.SessionExitRequested, Is.True,
                "le drapeau survit au changement de scene (objet persistant) : c'est ce qui empeche la re-entree automatique d'un client revenu au menu");
            Assert.That(errorNotices, Is.Empty,
                "une sortie volontaire n'est pas un echec : aucune notice d'erreur n'est publiee, contrairement a la perte de session qui garde HostDisconnectedMessage");
            Assert.That(LocalInputGate.IsBlocked, Is.False,
                "la scene dechargee alors que le menu etait ouvert a rendu les entrees par OnDestroy");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked),
                "et restaure le curseur capture a l'ouverture");
            Assert.That(Cursor.visible, Is.False);
        }

        /// <summary>
        /// Apres un retour au menu, une entree explicite rend l'auto-rejoindre de la Story 5.3 : le
        /// drapeau de sortie volontaire ne bloque jamais une partie redemandee, seulement la re-entree
        /// automatique. La demande passe par le bouton de creation de lobby, comme un joueur le ferait ;
        /// la room laissee ouverte par la run precedente est fermee par la meme action, ce qui est deja
        /// le comportement du bouton unique de la Story 2.2.
        /// </summary>
        [UnityTest]
        public IEnumerator ExplicitLobbyEntryLiftsTheVoluntaryExitFlag()
        {
            yield return EnterHostedRun();

            var flow = UnityEngine.Object.FindAnyObjectByType<RunEscapeMenuFlowController>();
            Assert.That(flow, Is.Not.Null, "RunEscapeMenuFlowController attendu sur RunRoot");

            flow.Open();
            flow.RequestQuitToMainMenu();

            var frames = 0;
            while (SceneManager.GetActiveScene().name != AppSceneRouter.MainMenuLobbySceneName && frames < 300)
            {
                yield return null;
                frames++;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName),
                "etat de depart : le joueur vient de quitter volontairement la run");
            Assert.That(RoadRageBootstrap.Instance.SessionExitRequested, Is.True,
                "etat de depart : la re-entree automatique est suspendue");

            var menuScreen = UnityEngine.Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null, "le menu principal est de nouveau affiche");
            ClickSerializedButton(menuScreen, "playButton");
            yield return null;

            var lobbyScreen = UnityEngine.Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null, "le joueur peut de nouveau entrer dans le lobby");

            // Le lobby Steam reste ouvert apres le quit (le leave invite est hors scope de la Story 2.3),
            // donc le premier clic ferme la room restee ouverte et le second demande la creation : les
            // deux chemins sont couverts par la meme boucle bornee, et la levee du drapeau est
            // synchrone au clic (avant l'attente reseau de la creation).
            LogAssert.ignoreFailingMessages = true;
            try
            {
                var attempts = 0;
                while (RoadRageBootstrap.Instance.SessionExitRequested && attempts < 3)
                {
                    ClickSerializedButton(lobbyScreen, "createLobbyButton");
                    attempts++;
                    yield return null;
                    yield return null;
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.That(RoadRageBootstrap.Instance.SessionExitRequested, Is.False,
                "une entree explicite redemande la partie : l'auto-rejoindre de la Story 5.3 est rendu au joueur");
        }

        /// <summary>
        /// Scene dechargee menu ouvert (matrice d'E/S) : OnDestroy est le dernier point ou le portail
        /// peut etre rendu et le curseur restaure. Ce chemin ne peut pas etre exerce en EditMode, ou
        /// Unity n'appelle pas les rappels de cycle de vie d'un composant ajoute a la volee ; la
        /// fixture EditMode n'en epingle donc que la source.
        /// </summary>
        [UnityTest]
        public IEnumerator TearingDownTheOwnerWithTheMenuOpenReleasesInputAndRestoresTheCursor()
        {
            RedirectProfileFilePath();

            yield return SceneManager.LoadSceneAsync(AppSceneRouter.MvpRunSceneName, LoadSceneMode.Single);
            yield return null;

            var flow = UnityEngine.Object.FindAnyObjectByType<RunEscapeMenuFlowController>();
            Assert.That(flow, Is.Not.Null, "RunEscapeMenuFlowController attendu sur RunRoot");

            RequireCursorWritesOrInconclusive();

            flow.Open();
            Assert.That(LocalInputGate.IsBlocked, Is.True, "etat de depart du scenario : menu ouvert");
            Assert.That(flow.IsOpen, Is.True);

            UnityEngine.Object.Destroy(flow);
            yield return null;
            yield return null;

            Assert.That(LocalInputGate.IsBlocked, Is.False,
                "une scene dechargee menu ouvert ne doit pas laisser la scene suivante muette");
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked), "le curseur est restaure");
            Assert.That(Cursor.visible, Is.False);
        }

        /// <summary>
        /// Les ecritures de curseur peuvent etre ignorees selon la fenetre de jeu : le test devient
        /// Inconclusif plutot que rouge, meme idiome que la fixture EditMode.
        /// </summary>
        private static void RequireCursorWritesOrInconclusive()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
            {
                Assert.Inconclusive("La fenetre de jeu ignore les ecritures de curseur : l'etat du curseur ne peut pas etre verifie sur cette machine.");
            }
        }

        /// <summary>
        /// Chemin unique de la Story 5.3 (AD-26) : bootstrap -> menu -> Play -> Start Game, jusqu'au
        /// chargement synchronise de MVP_Run. Une machine sans services Steam fonctionnels rend le test
        /// Inconclusif plutot que rouge (meme idiome que Story15/16/57).
        /// </summary>
        private IEnumerator EnterHostedRun()
        {
            RedirectProfileFilePath();

            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName),
                "le bootstrap route vers MainMenuLobby");

            var menuScreen = UnityEngine.Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");
            ClickSerializedButton(menuScreen, "playButton");
            yield return null;

            var lobbyScreen = UnityEngine.Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu apres Play");

            // Fenetre reseau sensible (creation du lobby Steam + StartHost) : FacepunchTransport peut
            // logguer une exception par frame de polling sans que le chemin lobby -> host -> MVP_Run ne
            // soit en cause. La fenetre couvre aussi les quelques frames de stabilisation du spawn, et
            // est refermee avant les assertions, qui restent donc strictes.
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
                        Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier le quit volontaire d'une session hebergee.");
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

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName),
                "Start Game doit entrer dans MVP_Run (AD-26)");

            var manager = NetworkManager.Singleton;
            Assert.That(manager, Is.Not.Null, "Start Game doit avoir demarre un NetworkManager (AD-26)");
            Assert.That(manager.IsServer, Is.True, "le pair local est l'hote du run");
        }

        /// <summary>
        /// Le profil persistant est redirige hors du dossier utilisateur AVANT tout chargement de
        /// scene : le bootstrap lit ce chemin une seule fois dans Awake, donc une redirection plus
        /// tardive laisserait le test lire et ecraser le profil reel de la machine (Story 4.5).
        /// </summary>
        private void RedirectProfileFilePath()
        {
            if (originalProfileFilePath != null)
            {
                return;
            }

            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(
                Path.GetTempPath(),
                "roadrage-story58-" + Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;
        }

        /// <summary>
        /// Clique le bouton reellement cable dans la scene, comme le ferait un joueur : c'est le
        /// cablage authored par l'Inspector qui est exerce, pas un appel direct a la couche App.
        /// </summary>
        private static void ClickSerializedButton(Component screen, string buttonFieldName)
        {
            var field = screen.GetType().GetField(
                buttonFieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, "field not found: " + buttonFieldName);

            var button = field.GetValue(screen) as Button;
            Assert.That(button, Is.Not.Null,
                screen.GetType().Name + "." + buttonFieldName + " doit referencer un Button de la scene");

            button.onClick.Invoke();
        }
    }
}
