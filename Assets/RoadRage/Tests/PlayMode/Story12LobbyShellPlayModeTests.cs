using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.Services;
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
    /// <summary>
    /// Couvre les comportements de la Story 1.2 qui dependent du cycle de vie Unity (Awake, clics reels,
    /// evenements) : Join By Code / Start Game publient une notice "indisponible" via UserNoticeChannel,
    /// et le changement de difficulte met a jour les reglages de partie ainsi que le libelle affiche.
    /// Create Lobby n'est plus un placeholder depuis la Story 2.2 (creation reelle de room hote) : sa
    /// couverture vit desormais dans Story22HostCreatedPrivateRoomPlayModeTests. LobbyShellScreen vit sur
    /// SetupPanel, inactif tant que Play n'a pas ete presse : chaque test entre donc reellement dans la
    /// coquille de lobby via MainMenuScreen avant d'agir.
    /// </summary>
    public sealed class Story12LobbyShellPlayModeTests
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
        public IEnumerator JoinByCodeRequestedPublishesUnavailableNotice()
        {
            yield return PressPlayAndEnterLobbyShell();

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(screen, Is.Not.Null);

            UserNotice? published = null;
            RoadRageBootstrap.Instance.Notices.NoticePublished += notice => published = notice;

            ClickSerializedButton(screen, "joinByCodeButton");

            Assert.That(published, Is.Not.Null, "Join By Code doit publier une notice via UserNoticeChannel");
        }

        // Le test « Start Game refuse et ne charge aucune scene » a ete retire avec la Story 4.5 : le
        // menu principal publie desormais toujours un profil (identite Steam, ou repli en memoire non
        // persiste), donc le refus MissingProfileStartGameMessage n'est plus atteignable depuis
        // l'interface. La garde reste en place dans LobbyFlowController comme defense en profondeur et
        // son existence est verrouillee en EditMode (Story15EmptyMapEntryTests). Le parcours positif
        // (Start Game avec profil -> MVP_Run) est couvert par Story15EmptyMapEntryPlayModeTests.

        [UnityTest]
        public IEnumerator DifficultyChangeUpdatesMatchSettingsAndDisplayedLabel()
        {
            yield return PressPlayAndEnterLobbyShell();

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            var flowController = Object.FindAnyObjectByType<LobbyFlowController>();
            Assert.That(screen, Is.Not.Null);
            Assert.That(flowController, Is.Not.Null);

            var initialDifficulty = flowController.Settings.Difficulty;
            var expectedNext = NextInCycleOrder(initialDifficulty);

            ClickSerializedButton(screen, "difficultyButton");
            yield return null;

            Assert.That(flowController.Settings.Difficulty, Is.EqualTo(expectedNext), "LobbyFlowController doit mettre a jour la difficulte des reglages de partie");

            var label = (TMP_Text)GetPrivateField(screen, "settingsSummaryLabel");
            Assert.That(label.text, Does.Contain(expectedNext.ToString()), "le libelle affiche doit se synchroniser avec la nouvelle difficulte");
        }

        [UnityTest]
        public IEnumerator LobbyShellOpensWithDefaultDifficultyVisible()
        {
            yield return PressPlayAndEnterLobbyShell();

            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            var flowController = Object.FindAnyObjectByType<LobbyFlowController>();
            Assert.That(screen, Is.Not.Null);
            Assert.That(flowController, Is.Not.Null);

            Assert.That(flowController.Settings.Difficulty, Is.EqualTo(Difficulty.Normal), "la difficulte par defaut doit etre Normal a l'ouverture de la coquille");

            var label = (TMP_Text)GetPrivateField(screen, "settingsSummaryLabel");
            Assert.That(label, Is.Not.Null);
            Assert.That(label.gameObject.activeInHierarchy, Is.True, "le libelle de reglages doit etre visible a l'ouverture de la coquille");
            Assert.That(label.text, Does.Contain(Difficulty.Normal.ToString()), "la difficulte par defaut doit etre affichee des l'ouverture, sans interaction prealable");
        }

        /// <summary>
        /// Couvre la ligne "Back" de la matrice I/O. Le BackButton de la scene est possede par
        /// MainMenuScreen (Story 1.1) : le retour passe par MainMenuScreen -> MainMenuFlowController
        /// -> ShowMenu. Ce test clique le vrai bouton serialise et verifie que les reglages de partie
        /// survivent au retour au menu.
        /// </summary>
        [UnityTest]
        public IEnumerator BackReturnsToMenuAndKeepsMatchSettingsState()
        {
            yield return PressPlayAndEnterLobbyShell();

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            var screen = Object.FindAnyObjectByType<LobbyShellScreen>();
            var flowController = Object.FindAnyObjectByType<LobbyFlowController>();
            Assert.That(menuScreen, Is.Not.Null);
            Assert.That(screen, Is.Not.Null);
            Assert.That(flowController, Is.Not.Null);

            ClickSerializedButton(screen, "difficultyButton");
            yield return null;

            var difficultyBeforeBack = flowController.Settings.Difficulty;
            Assert.That(difficultyBeforeBack, Is.Not.EqualTo(Difficulty.Normal), "la difficulte doit avoir quitte sa valeur par defaut avant de tester Back");

            var menuPanel = (GameObject)GetPrivateField(menuScreen, "menuPanel");
            var setupPanel = (GameObject)GetPrivateField(menuScreen, "setupPanel");
            Assert.That(setupPanel.activeSelf, Is.True, "la coquille de lobby doit etre affichee avant le clic Back");

            ClickSerializedButton(menuScreen, "backButton");
            yield return null;

            Assert.That(setupPanel.activeSelf, Is.False, "Back doit masquer la coquille de lobby");
            Assert.That(menuPanel.activeSelf, Is.True, "Back doit reafficher le menu principal");
            Assert.That(flowController.Settings.Difficulty, Is.EqualTo(difficultyBeforeBack), "les reglages de partie doivent conserver leur etat courant apres Back");
        }

        private static IEnumerator PressPlayAndEnterLobbyShell()
        {
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen introuvable dans MainMenuLobby");

            ClickSerializedButton(menuScreen, "playButton");
            yield return null;
        }

        /// <summary>
        /// Ordre de cycle attendu du controle de difficulte, derive de l'ordre reel du cycle et non
        /// d'une arithmetique sur les valeurs de l'enum (qui casserait au moindre reordonnancement).
        /// </summary>
        private static readonly Difficulty[] ExpectedCycleOrder = { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard };

        private static Difficulty NextInCycleOrder(Difficulty current)
        {
            var index = System.Array.IndexOf(ExpectedCycleOrder, current);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "difficulte hors de l'ordre de cycle attendu : " + current);
            return ExpectedCycleOrder[(index + 1) % ExpectedCycleOrder.Length];
        }

        /// <summary>
        /// Clique un bouton reellement reference par le champ serialise de l'ecran. Exerce donc a la fois
        /// la liaison serialisee de la scene et le listener enregistre dans Awake : un bouton non cable,
        /// ou cable sur le mauvais objet, fait echouer le test au lieu de passer au vert.
        /// </summary>
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
