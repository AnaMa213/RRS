using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Run;
using RoadRage.Features.UI;
using RoadRage.Shared.Input;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.8 : contrat du menu d'echappement de MVP_Run, verifie sans PlayMode. La story ne
    /// reconstruit aucun fondation : elle ajoute un ecran d'intention, un portail statique de blocage
    /// des entrees locales, un drapeau de sortie volontaire et un second declencheur sur le teardown de
    /// session existant. Ce fixture verrouille donc, dans l'ordre ou elles peuvent casser :
    ///
    /// - le portail partage et son respect par les lecteurs d'entree repartis dans quatre assemblies ;
    /// - les gardes de frontiere de Features/UI (aucun routage, aucun reglage de partie, aucune entree) ;
    /// - la lecture d'Echap cote App, derriere une garde de clavier absente ;
    /// - le teardown unique de NetworkedRunSessionMonitor et sa notice desormais parametree ;
    /// - le drapeau de sortie volontaire et la suspension de la re-entree automatique du lobby ;
    /// - le cablage authored de MVP_Run : panneau plein ecran sous le Canvas du HUD, boutons libelles,
    ///   EventSystem et controleur de flux.
    ///
    /// Les gardes de source portent sur du code, jamais sur de la prose (patron <c>CodeOnly</c> de
    /// Story56) : un commentaire qui NOMME une regle interdite ne doit pas faire echouer la garde qui
    /// verifie cette meme regle. Les etats de curseur sont, eux, verifies sur une instance reelle du
    /// controleur ; le Test Runner n'ayant pas de Game view, un environnement qui n'applique pas les
    /// ecritures de <see cref="Cursor"/> rend le test Inconclusif plutot que rouge, sans jamais
    /// escamoter les assertions de panneau et de portail qui le precedent.
    /// </summary>
    public sealed class Story58EscapeMenuTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

        private const string EscapeScreenSourcePath = "Assets/RoadRage/Features/UI/RunEscapeMenuScreen.cs";

        private const string EscapeFlowControllerSourcePath = "Assets/RoadRage/App/Run/RunEscapeMenuFlowController.cs";

        private const string SessionMonitorSourcePath = "Assets/RoadRage/App/Run/NetworkedRunSessionMonitor.cs";

        private const string LobbyFlowSourcePath = "Assets/RoadRage/App/Lobby/LobbyFlowController.cs";

        private const string HudSourcePath = "Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs";

        private const string LocalInputGateSourcePath = "Assets/RoadRage/Shared/Input/LocalInputGate.cs";

        private const string VehicleDriverSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";

        private const string UiAsmdefPath = "Assets/RoadRage/Features/UI/RoadRage.Features.UI.asmdef";

        private const string UiFolderRelativePath = "RoadRage/Features/UI";

        /// <summary>
        /// Les lecteurs d'entree locaux de la story, dans RoadRage.App et dans les quatre assemblies de
        /// feature qui n'ont pas le droit de referencer RoadRage.App : c'est cette dispersion qui
        /// justifie un portail partage plutot qu'autant de plomberies d'activation distinctes qu'il y a
        /// de lecteurs. Cette liste doit rester exhaustive : elle porte la garde qui verifie que CHAQUE
        /// lecteur consulte le portail.
        /// </summary>
        private static readonly string[] LocalInputReaderSourcePaths =
        {
            "Assets/RoadRage/App/Run/RunFlowController.cs",
            "Assets/RoadRage/App/Run/NetworkedPlayerLifecycleIntent.cs",
            "Assets/RoadRage/App/Run/NetworkedPlayerReviveIntent.cs",
            "Assets/RoadRage/App/Run/NetworkedVehicleRecoveryIntent.cs",
            "Assets/RoadRage/App/Run/NetworkedVehicleSeatIntent.cs",
            "Assets/RoadRage/App/Run/LocalVoidRespawnController.cs",
            "Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs",
            "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs",
            "Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs",
            "Assets/RoadRage/Features/PassengerActions/PassengerActionDebugView.cs"
        };

        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            // Le portail est statique et le curseur est un etat global : les deux doivent etre rendus
            // meme si le test a echoue au milieu, sinon la fixture suivante (et l'editeur du
            // developpeur) heritent d'un jeu muet et d'un curseur capture.
            LocalInputGate.Reset();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            foreach (var instance in spawned)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            spawned.Clear();
        }

        // ---------------------------------------------------------------- Portail partage

        [Test]
        public void LocalInputGateBlocksReleasesAndResetsIdempotently()
        {
            LocalInputGate.Reset();
            Assert.That(LocalInputGate.IsBlocked, Is.False, "le portail doit repartir rendu");

            LocalInputGate.Block();
            LocalInputGate.Block();
            Assert.That(LocalInputGate.IsBlocked, Is.True, "Block est idempotent : un seul drapeau partage");

            LocalInputGate.Release();
            LocalInputGate.Release();
            Assert.That(LocalInputGate.IsBlocked, Is.False, "Release est idempotent");

            LocalInputGate.Block();
            LocalInputGate.Reset();
            Assert.That(LocalInputGate.IsBlocked, Is.False,
                "Reset rend les entrees : c'est ce que fait RunEscapeMenuFlowController a la destruction, et une scene dechargee menu ouvert ne doit pas laisser le jeu muet");
        }

        [Test]
        public void EveryLocalInputReaderConsultsTheSharedGate()
        {
            foreach (var path in LocalInputReaderSourcePaths)
            {
                var source = CodeOnly(ReadProjectFile(path));

                Assert.That(source, Does.Contain("LocalInputGate.IsBlocked"),
                    path + " doit consulter le portail : une entree locale qui l'ignore reste active menu ouvert (conduite, deplacement a pied, actions, camera)");
            }
        }

        [Test]
        public void BlockedDrivingSubmitsAnIdleIntentInsteadOfLeavingTheLastOneStanding()
        {
            var source = CodeOnly(ReadProjectFile(VehicleDriverSourcePath));

            Assert.That(source, Does.Contain("SubmitDriveIntent(VehicleDriveIntent.Idle, localClientId)"),
                "menu ouvert, le conducteur doit soumettre une intention a zero : sinon l'hote conserve la derniere intention recue (plein gaz) et la voiture continue d'accelerer alors que le joueur ne conduit plus");
        }

        // ---------------------------------------------------------------- Frontieres de Features.UI

        [Test]
        public void TheEscapeScreenOnlyRaisesIntentionsAndHidesItsOwnPanel()
        {
            var source = CodeOnly(ReadProjectFile(EscapeScreenSourcePath));

            Assert.That(source, Does.Contain("public event Action ResumeRequested"),
                "l'ecran emet une intention de reprise, il ne route rien lui-meme");
            Assert.That(source, Does.Contain("public event Action QuitToMainMenuRequested"),
                "l'ecran emet une intention de retour au menu");
            Assert.That(source, Does.Contain("Hide();"),
                "le panneau authored actif dans MVP_Run se masque une fois ses boutons cables : c'est cet Awake() qui le referme au chargement de la scene");
            Assert.That(source, Does.Contain("public bool IsOpen"),
                "l'etat d'affichage de l'ecran est lisible de l'exterieur, pour la couche App et pour les tests PlayMode");
        }

        [Test]
        public void TheUiFeatureStillCannotRouteScenesQuitTheApplicationOrKnowTheLobby()
        {
            Assert.That(CodeOnly(ReadProjectFile(EscapeScreenSourcePath)), Does.Not.Contain("RoadRage.App"),
                "Features.UI n'a aucune raison de connaitre la couche App : c'est la couche App qui lit Echap et decide du quit");

            var uiSources = Directory.GetFiles(Path.Combine(Application.dataPath, UiFolderRelativePath), "*.cs", SearchOption.AllDirectories);
            Assert.That(uiSources.Length, Is.GreaterThan(0), "sources UI introuvables");

            foreach (var file in uiSources)
            {
                var source = CodeOnly(File.ReadAllText(file));

                Assert.That(source, Does.Not.Contain("SceneManager.LoadScene"), file);
                Assert.That(source, Does.Not.Contain("Application.Quit"), file);
                Assert.That(source, Does.Not.Contain("MatchSettings"), file + " ne connait aucun reglage de partie");
                Assert.That(source, Does.Not.Contain("UnityEngine.InputSystem"), file + " : la touche Echap se lit dans RoadRage.App");
                Assert.That(source, Does.Not.Contain("Keyboard"), file + " : aucune entree clavier n'est lue depuis Features.UI");
            }

            var asmdef = File.ReadAllText(Path.Combine(Application.dataPath, UiFolderRelativePath, "RoadRage.Features.UI.asmdef"));
            Assert.That(asmdef, Does.Not.Contain("RoadRage.App"), "l'asmdef UI ne reference jamais la couche App");
            Assert.That(asmdef, Does.Not.Contain("Unity.InputSystem"), "l'asmdef UI ne reference pas InputSystem");
            Assert.That(asmdef, Does.Not.Contain("RoadRage.Features.Lobby"), "l'asmdef UI ne reference pas le lobby");

            Assert.That(CodeOnly(ReadProjectFile(HudSourcePath)), Does.Not.Contain("Escape"),
                "RunCheckpointHudScreen n'est jamais etendu par le menu : le panneau est un enfant frere sous le meme Canvas");
        }

        // ---------------------------------------------------------------- Lecture d'Echap

        [Test]
        public void EscapeIsReadInTheAppLayerBehindANullKeyboardGuard()
        {
            var source = CodeOnly(ReadProjectFile(EscapeFlowControllerSourcePath));
            var update = MethodBody(source, "private void Update()", "private void OnDestroy()");

            var keyboardGuardIndex = update.IndexOf("if (keyboard == null)", StringComparison.Ordinal);
            var escapeReadIndex = update.IndexOf("keyboard.escapeKey.wasPressedThisFrame", StringComparison.Ordinal);
            var toggleIndex = update.IndexOf("Toggle();", StringComparison.Ordinal);

            Assert.That(keyboardGuardIndex, Is.GreaterThanOrEqualTo(0),
                "Update() doit sortir quand Keyboard.current est absent (machine sans clavier, scene de test)");
            Assert.That(escapeReadIndex, Is.GreaterThan(keyboardGuardIndex),
                "la garde de nullite precede la lecture de la touche : sans clavier, aucun changement et aucune exception");
            Assert.That(toggleIndex, Is.GreaterThan(escapeReadIndex),
                "Echap bascule le menu par le meme Toggle() que les tests PlayMode exercent");
        }

        [Test]
        public void TheEscapeFlowControllerNeverPausesTheSimulationNorMutatesSharedState()
        {
            foreach (var path in new[] { EscapeFlowControllerSourcePath, EscapeScreenSourcePath })
            {
                var source = CodeOnly(ReadProjectFile(path));

                Assert.That(source, Does.Not.Contain("timeScale"),
                    path + " : la simulation hebergee continue de tourner menu ouvert, elle n'est jamais mise en pause");
                Assert.That(source, Does.Not.Contain(".Value"),
                    path + " : le menu ne mute aucune NetworkVariable");
                Assert.That(source, Does.Not.Contain("NetworkVariable"), path);
                Assert.That(source, Does.Not.Contain("Rpc"),
                    path + " : le menu n'emet aucune intention reseau, tout etat partage reste host-authoritative");
            }
        }

        // ---------------------------------------------------------------- Teardown unique du quit

        [Test]
        public void TheEscapeFlowControllerRoutesTheQuitToTheSessionMonitor()
        {
            var source = CodeOnly(ReadProjectFile(EscapeFlowControllerSourcePath));

            Assert.That(source, Does.Not.Contain("Shutdown"),
                "un seul proprietaire du teardown : le menu emprunte NetworkedRunSessionMonitor, il ne reecrit pas Shutdown()");
            Assert.That(source, Does.Not.Contain("SceneManager"), "le menu ne route aucune scene");
            Assert.That(source, Does.Not.Contain("LoadScene"), "le menu ne charge aucune scene");
            Assert.That(source, Does.Contain("monitor.RequestVoluntaryExitToMainMenu()"),
                "le quit volontaire passe par l'entree volontaire du moniteur");
            Assert.That(source, Does.Contain("FindAnyObjectByType<NetworkedRunSessionMonitor>()"),
                "le moniteur est ajoute par RunFlowController.Start() : il se resout au moment du quit, jamais a l'Awake du menu");
            Assert.That(source, Does.Contain("LocalInputGate.Reset()"),
                "le portail est remis a zero par le proprietaire du menu, a l'Awake comme a la destruction");
        }

        [Test]
        public void TheSessionMonitorKeepsOneTeardownWithTwoTriggerFamilies()
        {
            var source = CodeOnly(ReadProjectFile(SessionMonitorSourcePath));

            Assert.That(CountOccurrences(source, ".Shutdown()"), Is.EqualTo(1),
                "un seul Shutdown() dans tout le moniteur : le quit volontaire n'ajoute pas un second chemin");
            Assert.That(CountOccurrences(source, "LoadMainMenu()"), Is.EqualTo(1),
                "un seul LoadMainMenu(), au meme endroit, avec le repli SceneManager deja en place");
            Assert.That(CountOccurrences(source, "StartCoroutine(ReturnClientToLobby("), Is.EqualTo(3),
                "trois declencheurs, une seule routine : perte de session, panne de transport, quit volontaire");
            Assert.That(CountOccurrences(source, "ReturnClientToLobby(manager, HostDisconnectedNotice)"), Is.EqualTo(2),
                "les deux declencheurs subis gardent la notice d'erreur d'hote perdu");
            Assert.That(CountOccurrences(source, "ReturnClientToLobby(manager, null)"), Is.EqualTo(1),
                "le quit volontaire n'est pas un echec : il ne publie aucune notice");
            Assert.That(source, Does.Contain("UserNotice? notice"),
                "la notice est un parametre de la routine existante, pas un second chemin parallele");
            Assert.That(source, Does.Contain("isReturningToLobby = true;"),
                "le drapeau de teardown en cours est arme dans la routine elle-meme");
            Assert.That(source, Does.Contain("bootstrap.SessionExitRequested = true;"),
                "le moniteur marque la sortie volontaire : c'est ce que le lobby consulte pour ne pas re-embarquer le joueur");
        }

        [Test]
        public void AVoluntaryExitIsIgnoredWhileAReturnIsAlreadyInProgress()
        {
            var source = CodeOnly(ReadProjectFile(SessionMonitorSourcePath));
            var request = MethodBody(source, "public bool RequestVoluntaryExitToMainMenu()", "public static bool ShouldReturnClientToLobby");

            var guardIndex = request.IndexOf("isReturningToLobby", StringComparison.Ordinal);
            var startIndex = request.IndexOf("StartCoroutine(", StringComparison.Ordinal);

            Assert.That(guardIndex, Is.GreaterThanOrEqualTo(0), "la garde isReturningToLobby doit etre lue par l'entree volontaire");
            Assert.That(startIndex, Is.GreaterThan(guardIndex),
                "un second quit pendant qu'un retour est deja en cours est ignore avant tout demarrage de teardown : un seul Shutdown() suivi d'un seul LoadMainMenu()");
        }

        // ---------------------------------------------------------------- Drapeau de sortie volontaire

        [Test]
        public void TheLobbyFlowSuspendsTheAutomaticRejoinWhileTheExitFlagIsSet()
        {
            var source = CodeOnly(ReadProjectFile(LobbyFlowSourcePath));
            var update = MethodBody(source, "private void Update()", "private bool IsSessionExitRequested()");

            Assert.That(update, Does.Contain("IsSessionExitRequested()"),
                "un client qui a quitte garde son statut de lobby et un signal de lancement toujours vrai : sans ce filtre il serait relance dans la run des le premier Update() de son retour au menu");
            Assert.That(CountOccurrences(source, "IsSessionExitRequested()"), Is.GreaterThanOrEqualTo(2),
                "la condition est nommee une fois et definie une fois");
            Assert.That(source, Does.Contain("bootstrap.SessionExitRequested = false;"),
                "le drapeau est leve par une entree explicite");
            Assert.That(CountOccurrences(source, "ClearSessionExitRequest();"), Is.EqualTo(4),
                "les quatre chemins d'entree reels (creation de room, join par code, Start Game avec room ouverte ou creee) levent le drapeau ; les chemins refuses sortent avant, pour ne pas rendre au client l'auto-rejoindre qu'il vient de suspendre");
        }

        [Test]
        public void TheVoluntaryExitFlagStartsFalseAndIsLiftedByAnExplicitEntry()
        {
            // Objet de scene inactif : Awake() est differe, donc aucun service Steam n'est construit en
            // EditMode. Le drapeau lui-meme est une propriete portee par l'objet persistant, c'est elle
            // qui est verifiee ici ; le survol du changement de scene est verifie en PlayMode.
            var owner = new GameObject("Story58BootstrapProbe");
            owner.SetActive(false);
            spawned.Add(owner);
            var bootstrap = owner.AddComponent<RoadRageBootstrap>();

            Assert.That(bootstrap.SessionExitRequested, Is.False,
                "le drapeau est leve uniquement par la sortie volontaire : le survoler a l'Awake relancerait une run jamais quittee");

            bootstrap.SessionExitRequested = true;
            Assert.That(bootstrap.SessionExitRequested, Is.True, "le moniteur peut le marquer");

            bootstrap.SessionExitRequested = false;
            Assert.That(bootstrap.SessionExitRequested, Is.False, "une entree explicite rend l'auto-rejoindre de la Story 5.3");
        }

        // ---------------------------------------------------------------- Comportement du proprietaire du menu

        [Test]
        public void OpenAndCloseToggleThePanelAndTheSharedInputGate()
        {
            var screen = CreateEscapeScreen();
            var controller = CreateEscapeFlowController(screen);
            var timeScaleBefore = Time.timeScale;

            Assert.That(LocalInputGate.IsBlocked, Is.False, "etat de depart : aucune entree bloquee");

            controller.Open();
            Assert.That(controller.IsOpen, Is.True);
            Assert.That(screen.IsOpen, Is.True, "le panneau s'affiche");
            Assert.That(LocalInputGate.IsBlocked, Is.True, "les entrees locales sont bloquees");
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBefore), "ouvrir le menu ne met jamais la simulation en pause");

            controller.Open();
            Assert.That(controller.IsOpen, Is.True, "Open est idempotent");
            Assert.That(LocalInputGate.IsBlocked, Is.True);

            controller.Close();
            Assert.That(controller.IsOpen, Is.False);
            Assert.That(screen.IsOpen, Is.False, "le panneau est masque");
            Assert.That(LocalInputGate.IsBlocked, Is.False, "les entrees sont rendues");

            controller.Toggle();
            Assert.That(controller.IsOpen, Is.True, "Echap ouvre quand le menu est ferme");
            Assert.That(LocalInputGate.IsBlocked, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBefore));

            controller.Toggle();
            Assert.That(controller.IsOpen, Is.False, "Echap ferme quand le menu est ouvert");
            Assert.That(screen.IsOpen, Is.False);
            Assert.That(LocalInputGate.IsBlocked, Is.False);

            controller.Close();
            Assert.That(controller.IsOpen, Is.False, "Close est idempotent");
            Assert.That(LocalInputGate.IsBlocked, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(timeScaleBefore), "Time.timeScale reste intouche sur tout le cycle");
        }

        [Test]
        public void OpenAndCloseReleaseAndRestoreTheCursorStateCapturedAtOpen()
        {
            var screen = CreateEscapeScreen();
            var controller = CreateEscapeFlowController(screen);

            var cursorLockBefore = Cursor.lockState;
            var cursorVisibleBefore = Cursor.visible;
            try
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                AssertCursorWritesAreHonouredOrInconclusive();

                controller.Open();
                Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None), "le curseur est libere a l'ouverture");
                Assert.That(Cursor.visible, Is.True, "et rendu visible");

                // Etat intermediaire : une seconde ouverture ne doit pas remplacer l'etat capture. Le
                // curseur reste volontairement a l'etat pose ici -- un Open deja ouvert ne le reprend
                // pas -- et c'est Close() qui doit restaurer l'etat de la PREMIERE ouverture, jamais
                // l'etat intermediaire. L'idempotence se constate donc sur la capture, pas sur le
                // curseur vivant.
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                controller.Open();
                Assert.That(GetPrivateField(controller, "capturedCursorLockMode"), Is.EqualTo(CursorLockMode.Locked),
                    "Open est idempotent : l'etat capture reste celui de la premiere ouverture");
                Assert.That(GetPrivateField(controller, "capturedCursorVisible"), Is.EqualTo(false));

                controller.Close();
                Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked),
                    "le curseur retrouve l'etat capture a l'ouverture, pas l'etat intermediaire");
                Assert.That(Cursor.visible, Is.False);
            }
            finally
            {
                Cursor.lockState = cursorLockBefore;
                Cursor.visible = cursorVisibleBefore;
            }
        }

        [Test]
        public void DestroyingTheOwnerWithTheMenuOpenIsTheTeardownThatReleasesInputAndCursor()
        {
            // OnDestroy ne peut pas etre exerce en EditMode : Unity n'appelle pas les rappels de cycle
            // de vie d'un composant ajoute a la volee hors Play Mode. Le contrat est donc epingle ici
            // sur la source, et son execution reelle est verifiee en PlayMode
            // (Story58EscapeMenuPlayModeTests.TearingDownTheOwnerWithTheMenuOpenReleasesInputAndRestoresTheCursor).
            var source = File.ReadAllText("Assets/RoadRage/App/Run/RunEscapeMenuFlowController.cs");

            var onDestroyIndex = source.IndexOf("private void OnDestroy()", StringComparison.Ordinal);
            Assert.That(onDestroyIndex, Is.GreaterThanOrEqualTo(0),
                "OnDestroy est le seul teardown du menu quand la scene est dechargee");

            var resetIndex = source.IndexOf("LocalInputGate.Reset();", onDestroyIndex, StringComparison.Ordinal);
            Assert.That(resetIndex, Is.GreaterThan(onDestroyIndex),
                "une scene dechargee menu ouvert doit rendre les entrees : sinon le portail statique resterait bloque pour la scene suivante");

            var restoreIndex = source.IndexOf("RestoreCursorState();", onDestroyIndex, StringComparison.Ordinal);
            Assert.That(restoreIndex, Is.GreaterThan(onDestroyIndex),
                "et restaurer le curseur capture, sans quoi la scene suivante heriterait d'un curseur libere");
        }

        // ---------------------------------------------------------------- Cablage authored de MVP_Run

        [Test]
        public void MvpRunAuthoredEscapePanelIsAFullscreenLastChildOfTheHudCanvas()
        {
            var scene = OpenMvpRunScene(out var openedHere);
            try
            {
                var hud = FindComponentInScene<RunCheckpointHudScreen>(scene);
                Assert.That(hud, Is.Not.Null, "RunCheckpointHudScreen attendu dans MVP_Run");

                var screen = hud.GetComponentInChildren<RunEscapeMenuScreen>(true);
                Assert.That(screen, Is.Not.Null, "RunEscapeMenuScreen attendu sous le Canvas du HUD");
                Assert.That(screen.GetComponent<RunCheckpointHudScreen>(), Is.Null,
                    "le menu est un panneau frere du HUD, jamais une extension de l'ecran existant");

                var panel = screen.gameObject;
                Assert.That(panel.transform.parent, Is.SameAs(hud.transform),
                    "le panneau vit sous le meme Canvas que le HUD");

                Assert.That(panel.activeSelf, Is.True,
                    "le panneau est authored ACTIF dans la scene : c'est l'Awake() de RunEscapeMenuScreen qui le masque (verifie sur une instance reelle et en PlayMode), pas la scene");

                var siblingIndex = panel.transform.GetSiblingIndex();
                var deathOverlay = (GameObject)GetPrivateField(hud, "deathOverlayPanel");
                Assert.That(deathOverlay, Is.Not.Null, "l'overlay de mort doit rester present et intact");
                Assert.That(siblingIndex, Is.EqualTo(hud.transform.childCount - 1),
                    "dernier enfant du Canvas : le menu doit rester au-dessus de tout l'affichage de run");
                Assert.That(siblingIndex, Is.GreaterThan(deathOverlay.transform.GetSiblingIndex()),
                    "le menu est au-dessus de l'overlay de mort : un joueur mort doit pouvoir quitter la run");

                var rect = (RectTransform)panel.transform;
                Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero), "panneau plein ecran");
                Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one), "panneau plein ecran");

                var backdrop = panel.GetComponent<UnityEngine.UI.Image>();
                Assert.That(backdrop, Is.Not.Null, "le panneau porte un fond plein ecran qui capte le clic");
                Assert.That(backdrop.color.a, Is.GreaterThan(0f), "fond sombre lisible par-dessus la run");
            }
            finally
            {
                if (openedHere)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void MvpRunEscapeButtonsCarryTheResumeAndQuitLabels()
        {
            var scene = OpenMvpRunScene(out var openedHere);
            try
            {
                var screen = FindComponentInScene<RunEscapeMenuScreen>(scene);
                Assert.That(screen, Is.Not.Null, "RunEscapeMenuScreen attendu dans MVP_Run");

                var resume = (Button)GetPrivateField(screen, "resumeButton");
                var quit = (Button)GetPrivateField(screen, "quitToMainMenuButton");

                Assert.That(resume, Is.Not.Null, "resumeButton doit etre cable dans la scene");
                Assert.That(quit, Is.Not.Null, "quitToMainMenuButton doit etre cable dans la scene");
                Assert.That(resume, Is.Not.SameAs(quit), "les deux intentions ne partagent pas un meme bouton");

                var resumeLabel = resume.GetComponentInChildren<TMP_Text>(true);
                var quitLabel = quit.GetComponentInChildren<TMP_Text>(true);

                Assert.That(resumeLabel, Is.Not.Null, "le bouton Resume porte un libelle lisible");
                Assert.That(quitLabel, Is.Not.Null, "le bouton Quit to Main Menu porte un libelle lisible");
                Assert.That(resumeLabel.text, Is.EqualTo("Resume"));
                Assert.That(quitLabel.text, Is.EqualTo("Quit to Main Menu"));
                Assert.That(resumeLabel.font, Is.Not.Null, "libelle rendu avec une police TMP du projet");
                Assert.That(resumeLabel.font.name, Does.Contain("LiberationSans"),
                    "police TMP du projet : un libelle sans police ne rend rien");
                Assert.That(quitLabel.font, Is.Not.Null);
                Assert.That(quitLabel.font.name, Does.Contain("LiberationSans"));
            }
            finally
            {
                if (openedHere)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void MvpRunCarriesTheEventSystemAndTheWiredEscapeFlowController()
        {
            var scene = OpenMvpRunScene(out var openedHere);
            try
            {
                var eventSystem = FindComponentInScene<EventSystem>(scene);
                Assert.That(eventSystem, Is.Not.Null,
                    "sans EventSystem les boutons du menu ne sont pas cliquables (absent de MVP_Run avant la story)");
                Assert.That(eventSystem.gameObject.activeSelf, Is.True, "un EventSystem inactif ne route aucun clic");
                Assert.That(eventSystem.enabled, Is.True);
                Assert.That(eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(), Is.Not.Null,
                    "le module UI InputSystem est requis : le projet lit les entrees par InputSystem, pas par l'ancien module");

                var hud = FindComponentInScene<RunCheckpointHudScreen>(scene);
                Assert.That(hud, Is.Not.Null, "RunCheckpointHudScreen attendu dans MVP_Run");
                var canvas = hud.GetComponent<Canvas>();
                Assert.That(canvas, Is.Not.Null, "le HUD porte le Canvas unique de MVP_Run");
                Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.That(canvas.GetComponent<GraphicRaycaster>(), Is.Not.Null,
                    "un Canvas sans GraphicRaycaster ne recoit aucun clic");

                var root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == "RunRoot");
                Assert.That(root, Is.Not.Null, "RunRoot attendu dans MVP_Run");

                var flow = root.GetComponent<RunEscapeMenuFlowController>();
                Assert.That(flow, Is.Not.Null,
                    "RunEscapeMenuFlowController est un objet de scene sur RunRoot, comme RunFlowController");
                Assert.That(flow.GetComponent<NetworkedRunSessionMonitor>(), Is.Null,
                    "le moniteur est ajoute a l'execution par RunFlowController.Start() : il ne doit pas etre authored dans la scene");

                var wiredScreen = (RunEscapeMenuScreen)GetPrivateField(flow, "screen");
                Assert.That(wiredScreen, Is.Not.Null, "la reference screen doit etre cablee dans la scene");
                Assert.That(wiredScreen, Is.SameAs(hud.GetComponentInChildren<RunEscapeMenuScreen>(true)),
                    "la reference pointe bien le panneau de MVP_Run, pas un asset orphelin");
            }
            finally
            {
                if (openedHere)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        // ---------------------------------------------------------------- Doubles sans Netcode

        private RunEscapeMenuScreen CreateEscapeScreen()
        {
            var panel = new GameObject("Story58EscapeMenuPanel");
            spawned.Add(panel);
            return panel.AddComponent<RunEscapeMenuScreen>();
        }

        /// <summary>
        /// Le controleur consomme son ecran par reference serialisee. Le champ est pose directement
        /// plutot que par Awake() : en EditMode l'ordre et le declenchement des Awake ne sont pas un
        /// contrat, alors que le comportement d'ouverture et de fermeture l'est.
        /// </summary>
        private RunEscapeMenuFlowController CreateEscapeFlowController(RunEscapeMenuScreen screen)
        {
            var owner = new GameObject("Story58RunEscapeMenuFlowController");
            spawned.Add(owner);
            var controller = owner.AddComponent<RunEscapeMenuFlowController>();
            SetPrivateField(controller, "screen", screen);
            return controller;
        }

        private static void AssertCursorWritesAreHonouredOrInconclusive()
        {
            // La sonde restaure ce qu'elle a trouve : elle ne doit pas devenir la pre-condition du test.
            var lockBefore = Cursor.lockState;
            var visibleBefore = Cursor.visible;

            Cursor.lockState = CursorLockMode.Locked;
            var honoured = Cursor.lockState == CursorLockMode.Locked;

            Cursor.lockState = lockBefore;
            Cursor.visible = visibleBefore;

            if (!honoured)
            {
                Assert.Inconclusive(
                    "Cet environnement Editor n'applique pas les ecritures de Cursor : les assertions d'etat du curseur ne peuvent pas etre evaluees ici. "
                    + "Les assertions de panneau et de portail, evaluees juste avant, restent valables ; le meme controle de curseur est refait en PlayMode (Story58EscapeMenuPlayModeTests).");
            }
        }

        // ---------------------------------------------------------------- Outils

        private static Scene OpenMvpRunScene(out bool openedHere)
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MvpRunScenePath);
            openedHere = !alreadyOpen.IsValid();
            return openedHere ? EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive) : alreadyOpen;
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

        /// <summary>
        /// Lit un fichier du projet depuis <see cref="Application.dataPath"/> plutot que depuis le
        /// repertoire courant : le Test Runner peut etre lance depuis un autre repertoire, et un chemin
        /// relatif produirait alors une FileNotFoundException au lieu d'un echec de test lisible.
        /// </summary>
        private static string ReadProjectFile(string projectRelativePath)
        {
            const string assetsPrefix = "Assets/";

            Assert.That(projectRelativePath.StartsWith(assetsPrefix, StringComparison.Ordinal), Is.True,
                "chemin projet attendu sous Assets/ : " + projectRelativePath);

            var absolutePath = Path.Combine(
                Application.dataPath,
                projectRelativePath.Substring(assetsPrefix.Length).Replace('/', Path.DirectorySeparatorChar));

            Assert.That(File.Exists(absolutePath), Is.True, "source introuvable : " + absolutePath);
            return File.ReadAllText(absolutePath);
        }

        /// <summary>
        /// Source privee de ses commentaires de ligne et de documentation. Les gardes de ce fichier
        /// portent sur du code, pas sur de la prose : un commentaire qui NOMME une regle ("n'ecrit
        /// jamais dans une NetworkVariable") ne doit pas faire echouer la garde qui verifie cette meme
        /// regle (patron <c>CodeOnly</c> de Story56).
        /// </summary>
        private static string CodeOnly(string source)
        {
            return string.Join("\n", source
                .Split('\n')
                .Where(line => !line.TrimStart().StartsWith("//")));
        }

        private static string MethodBody(string source, string signature, string nextSignature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "signature introuvable : " + signature);

            var end = source.IndexOf(nextSignature, start + signature.Length, StringComparison.Ordinal);
            Assert.That(end, Is.GreaterThan(start), "borne introuvable apres " + signature + " : " + nextSignature);

            return source.Substring(start, end - start);
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var index = text.IndexOf(value, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal);
            }

            return count;
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            field.SetValue(target, value);
        }
    }
}
