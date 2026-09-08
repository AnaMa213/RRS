using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Players;
using RoadRage.App.Services;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 1.3 : chaque ligne de la matrice I/O exercee par un vrai clic sur un bouton serialise de
    /// la scene (jamais d'invocation de methode privee : un bouton non cable doit faire echouer le test,
    /// pas passer au vert -- Patch 1 de la Story 1.2). Le panneau de personnage est inactif au
    /// chargement, donc chaque test entre reellement dans la coquille de lobby puis ouvre le setup.
    /// </summary>
    public sealed class Story13CharacterSetupPlayModeTests
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

        // ------------------------------------------------ Ouverture du setup de personnage

        [UnityTest]
        public IEnumerator OpeningCharacterSetupShowsNameFieldAndFirstCatalogCharacter()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            Assert.That(flow != null, Is.True, "PlayerProfileFlowController introuvable dans la scene");

            var screen = GetScreen(flow);
            Assert.That(screen.gameObject.activeSelf, Is.False, "le panneau de personnage doit etre ferme avant le clic");

            yield return OpenCharacterSetup();

            Assert.That(screen.gameObject.activeInHierarchy, Is.True, "le clic sur le bouton Personnage doit ouvrir le panneau");

            var input = (TMP_InputField)GetPrivateField(screen, "nameInputField");
            Assert.That(input != null, Is.True, "le champ de nom doit etre cable");
            Assert.That(input.gameObject.activeInHierarchy, Is.True, "le champ de nom doit etre visible");

            var catalog = GetCatalog(flow);
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(1), "au moins un personnage placeholder selectionnable");

            var nameLabel = (TMP_Text)GetPrivateField(screen, "characterNameLabel");
            Assert.That(nameLabel.text, Is.EqualTo(catalog.GetAt(0).DisplayName),
                "le premier personnage du catalogue doit etre selectionne par defaut");

            var preview = (Image)GetPrivateField(screen, "characterPreviewImage");
            Assert.That(preview.color, Is.EqualTo(catalog.GetAt(0).PreviewTint), "la silhouette doit porter la teinte du personnage courant");

            AssertSerializedButtonExists(screen, "confirmButton");
            AssertSerializedButtonExists(screen, "closeButton");
            AssertSerializedButtonExists(screen, "characterCycleButton");
        }

        // ------------------------------------------------ Nom valide confirme

        [UnityTest]
        public IEnumerator ConfirmingValidNameWritesProfilePublishesNoticeAndClosesScreen()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);
            var catalog = GetCatalog(flow);

            yield return OpenCharacterSetup();

            var sceneCountBefore = SceneManager.sceneCount;
            var activeSceneBefore = SceneManager.GetActiveScene().name;

            UserNotice? published = null;
            RoadRageBootstrap.Instance.Notices.NoticePublished += notice => published = notice;

            SetInputText(screen, "Kenan");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            var store = RoadRageBootstrap.Instance.Profiles;
            Assert.That(store, Is.Not.Null, "le bootstrap doit porter le depot de profil");
            Assert.That(store.HasProfile, Is.True, "un nom valide doit ecrire le profil de session");
            Assert.That(store.Current.DisplayName, Is.EqualTo("Kenan"));
            Assert.That(store.Current.CharacterId, Is.EqualTo(catalog.GetAt(0).Id));
            Assert.That(store.Current.CharacterId.IsEmpty, Is.False);
            Assert.That(store.Current.CharacterId.Value, Is.EqualTo(store.Current.CharacterId.Value.ToLowerInvariant()));

            Assert.That(published, Is.Not.Null, "la confirmation doit publier une notice visible");
            Assert.That(published.Value.Severity, Is.EqualTo(UserNoticeSeverity.Info));
            Assert.That(published.Value.Message, Does.Contain("Kenan"));

            Assert.That(screen.gameObject.activeSelf, Is.False, "l'ecran doit se fermer apres confirmation");
            Assert.That(SceneManager.sceneCount, Is.EqualTo(sceneCountBefore), "le setup de personnage ne charge aucune scene");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(activeSceneBefore));
        }

        [UnityTest]
        public IEnumerator ConfirmedProfileSurvivesSceneChange()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);
            var catalog = GetCatalog(flow);

            yield return OpenCharacterSetup();

            SetInputText(screen, "Kenan");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            var expectedId = catalog.GetAt(0).Id;

            // Recharge la meme scene : seul l'objet persistant du bootstrap survit.
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var store = RoadRageBootstrap.Instance.Profiles;
            Assert.That(store.HasProfile, Is.True, "le profil doit survivre au changement de scene (entree monde Story 1.5)");
            Assert.That(store.Current.DisplayName, Is.EqualTo("Kenan"));
            Assert.That(store.Current.CharacterId, Is.EqualTo(expectedId));
        }

        // ------------------------------------------------ Noms refuses

        [UnityTest]
        public IEnumerator ConfirmingEmptyNameShowsValidationAndLeavesProfileUnwritten()
        {
            yield return AssertRejectedName(string.Empty);
        }

        [UnityTest]
        public IEnumerator ConfirmingWhitespaceOnlyNameShowsValidationAndLeavesProfileUnwritten()
        {
            yield return AssertRejectedName("   ");
        }

        [UnityTest]
        public IEnumerator ConfirmingTooLongNameShowsValidationAndLeavesProfileUnwritten()
        {
            yield return AssertRejectedName(new string('a', 40));
        }

        [UnityTest]
        public IEnumerator ConfirmingForbiddenCharactersShowsValidationAndLeavesProfileUnwritten()
        {
            yield return AssertRejectedName("a<b>");
        }

        [UnityTest]
        public IEnumerator ConfirmingNameWithBorderWhitespaceNormalizesBeforeWriting()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);

            yield return OpenCharacterSetup();

            SetInputText(screen, "  Kenan  ");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            var store = RoadRageBootstrap.Instance.Profiles;
            Assert.That(store.HasProfile, Is.True);
            Assert.That(store.Current.DisplayName, Is.EqualTo("Kenan"), "le nom doit etre normalise avant ecriture du profil");
        }

        // ------------------------------------------------ Changement de personnage

        [UnityTest]
        public IEnumerator CyclingCharacterShowsNextCatalogEntryWithoutWritingProfile()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);
            var catalog = GetCatalog(flow);
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "le cycle n'est observable qu'avec au moins deux personnages");

            yield return OpenCharacterSetup();

            var nameLabel = (TMP_Text)GetPrivateField(screen, "characterNameLabel");
            var preview = (Image)GetPrivateField(screen, "characterPreviewImage");
            Assert.That(nameLabel.text, Is.EqualTo(catalog.GetAt(0).DisplayName));

            ClickSerializedButton(screen, "characterCycleButton");
            yield return null;

            Assert.That(flow.CurrentIndex, Is.EqualTo(1));
            Assert.That(nameLabel.text, Is.EqualTo(catalog.GetAt(1).DisplayName), "le cycle doit afficher le personnage suivant");
            Assert.That(preview.color, Is.EqualTo(catalog.GetAt(1).PreviewTint), "la teinte doit suivre le personnage affiche");
            Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.False,
                "cycler un personnage ne doit rien ecrire tant que Confirmer n'est pas presse");
        }

        [UnityTest]
        public IEnumerator CyclingWrapsAroundToFirstCatalogEntry()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);
            var catalog = GetCatalog(flow);

            // Sans cette garde le test serait vide de sens sur un catalogue a une seule entree :
            // l'index retomberait sur 0 sans qu'aucun enroulement n'ait eu lieu.
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "l'enroulement n'est observable qu'avec au moins deux personnages");

            yield return OpenCharacterSetup();

            for (var i = 0; i < catalog.Count; i++)
            {
                ClickSerializedButton(screen, "characterCycleButton");
                yield return null;
            }

            Assert.That(flow.CurrentIndex, Is.EqualTo(0), "le cycle doit revenir au premier personnage");
        }

        // ------------------------------------------------ Catalogue vide ou non assigne

        [UnityTest]
        public IEnumerator MissingCatalogLeavesCycleAndConfirmInertWithVisibleValidation()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);

            SetPrivateField(flow, "catalog", null);

            yield return OpenCharacterSetup();

            var validationLabel = (TMP_Text)GetPrivateField(screen, "validationLabel");
            Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True, "un catalogue absent doit afficher un retour visible");
            Assert.That(validationLabel.text, Is.EqualTo(PlayerProfileFlowController.EmptyCatalogValidation));

            UserNotice? published = null;
            RoadRageBootstrap.Instance.Notices.NoticePublished += notice => published = notice;

            ClickSerializedButton(screen, "characterCycleButton");
            yield return null;
            Assert.That(flow.CurrentIndex, Is.EqualTo(0), "le cycle doit rester inerte sans catalogue");

            SetInputText(screen, "Kenan");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.False, "aucun profil ne doit etre ecrit sans catalogue");
            Assert.That(published, Is.Null, "aucune notice de confirmation ne doit etre publiee sans catalogue");
            Assert.That(screen.gameObject.activeInHierarchy, Is.True, "l'ecran doit rester ouvert");
            Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True);
        }

        /// <summary>
        /// Troisieme forme de catalogue inexploitable : assigne, non vide, mais dont l'entree courante
        /// est nulle. Count compte les emplacements, donc la garde Count > 0 passe : seule la garde
        /// d'entree nulle empeche une NullReferenceException sur l'id du personnage. La matrice exige
        /// "Aucune exception".
        /// </summary>
        [UnityTest]
        public IEnumerator CatalogWithNullEntryLeavesCycleAndConfirmInertWithVisibleValidation()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);

            var holeyCatalog = ScriptableObject.CreateInstance<CharacterCatalog>();
            SetPrivateField(holeyCatalog, "characters", new List<CharacterDef> { null });
            Assert.That(holeyCatalog.Count, Is.EqualTo(1), "le catalogue de test doit compter une entree, nulle mais presente");
            Assert.That(holeyCatalog.GetAt(0), Is.Null, "l'entree doit bien etre nulle");
            SetPrivateField(flow, "catalog", holeyCatalog);

            UserNotice? published = null;

            try
            {
                yield return OpenCharacterSetup();

                var validationLabel = (TMP_Text)GetPrivateField(screen, "validationLabel");
                Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True, "une entree de catalogue nulle doit afficher un retour visible");
                Assert.That(validationLabel.text, Is.EqualTo(PlayerProfileFlowController.MissingCharacterValidation));

                RoadRageBootstrap.Instance.Notices.NoticePublished += notice => published = notice;

                ClickSerializedButton(screen, "characterCycleButton");
                yield return null;
                Assert.That(flow.CurrentIndex, Is.EqualTo(0), "cycler sur une entree unique doit rester sur place, sans exception");

                SetInputText(screen, "Kenan");
                ClickSerializedButton(screen, "confirmButton");
                yield return null;

                Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.False, "aucun profil ne doit etre ecrit depuis une entree nulle");
                Assert.That(published, Is.Null, "aucune notice de confirmation ne doit etre publiee");
                Assert.That(screen.gameObject.activeInHierarchy, Is.True, "l'ecran doit rester ouvert");
                Assert.That(validationLabel.text, Is.EqualTo(PlayerProfileFlowController.MissingCharacterValidation));
            }
            finally
            {
                Object.DestroyImmediate(holeyCatalog);
            }
        }

        /// <summary>
        /// Un CharacterDef a l'id vide ne doit jamais atteindre le depot : le critere d'acceptation
        /// exige un DefinitionId minuscule non vide, exploitable tel quel par la synchro de l'Epic 2.
        /// </summary>
        [UnityTest]
        public IEnumerator CatalogWithEmptyCharacterIdRefusesConfirmationWithVisibleValidation()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);

            var blankIdDef = ScriptableObject.CreateInstance<CharacterDef>();
            SetPrivateField(blankIdDef, "id", string.Empty);
            SetPrivateField(blankIdDef, "displayName", "Sans id");
            var blankIdCatalog = ScriptableObject.CreateInstance<CharacterCatalog>();
            SetPrivateField(blankIdCatalog, "characters", new List<CharacterDef> { blankIdDef });
            SetPrivateField(flow, "catalog", blankIdCatalog);

            try
            {
                yield return OpenCharacterSetup();

                var validationLabel = (TMP_Text)GetPrivateField(screen, "validationLabel");
                Assert.That(validationLabel.text, Is.EqualTo(PlayerProfileFlowController.EmptyCharacterIdValidation));

                SetInputText(screen, "Kenan");
                ClickSerializedButton(screen, "confirmButton");
                yield return null;

                Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.False,
                    "un id de personnage vide ne doit jamais produire de profil");
                Assert.That(screen.gameObject.activeInHierarchy, Is.True, "l'ecran doit rester ouvert");
                Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True);
                Assert.That(validationLabel.text, Is.EqualTo(PlayerProfileFlowController.EmptyCharacterIdValidation));
            }
            finally
            {
                Object.DestroyImmediate(blankIdCatalog);
                Object.DestroyImmediate(blankIdDef);
            }
        }

        /// <summary>
        /// Seconde moitie de la ligne "Catalogue vide ou non assigne" de la matrice : un catalogue bien
        /// assigne mais sans aucune entree. Distinct du catalogue absent : il emprunte la garde
        /// Count > 0 et non la garde de reference nulle.
        /// </summary>
        [UnityTest]
        public IEnumerator EmptyCatalogLeavesCycleAndConfirmInertWithVisibleValidation()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);

            var emptyCatalog = ScriptableObject.CreateInstance<CharacterCatalog>();
            Assert.That(emptyCatalog.Count, Is.EqualTo(0), "le catalogue de test doit bien etre vide, pas absent");
            SetPrivateField(flow, "catalog", emptyCatalog);

            UserNotice? published = null;

            try
            {
                yield return OpenCharacterSetup();

                var validationLabel = (TMP_Text)GetPrivateField(screen, "validationLabel");
                Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True, "un catalogue vide doit afficher un retour visible");
                Assert.That(validationLabel.text, Is.EqualTo(PlayerProfileFlowController.EmptyCatalogValidation));

                RoadRageBootstrap.Instance.Notices.NoticePublished += notice => published = notice;

                ClickSerializedButton(screen, "characterCycleButton");
                yield return null;
                Assert.That(flow.CurrentIndex, Is.EqualTo(0), "le cycle doit rester inerte sur un catalogue vide");

                SetInputText(screen, "Kenan");
                ClickSerializedButton(screen, "confirmButton");
                yield return null;

                Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.False, "aucun profil ne doit etre ecrit sur un catalogue vide");
                Assert.That(published, Is.Null, "aucune notice de confirmation ne doit etre publiee");
                Assert.That(screen.gameObject.activeInHierarchy, Is.True, "l'ecran doit rester ouvert");
                Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(emptyCatalog);
            }
        }

        // ------------------------------------------------ Reouverture apres confirmation

        [UnityTest]
        public IEnumerator ReopeningAfterConfirmationRestoresConfirmedNameAndCharacter()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);
            var catalog = GetCatalog(flow);

            yield return OpenCharacterSetup();

            ClickSerializedButton(screen, "characterCycleButton");
            yield return null;

            var confirmedCharacter = catalog.GetAt(flow.CurrentIndex);
            SetInputText(screen, "Kenan");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            Assert.That(screen.gameObject.activeSelf, Is.False);

            // Repart d'un index different pour prouver que la reouverture restaure vraiment le profil.
            SetPrivateField(flow, "currentIndex", 0);

            yield return OpenCharacterSetup();

            var input = (TMP_InputField)GetPrivateField(screen, "nameInputField");
            var nameLabel = (TMP_Text)GetPrivateField(screen, "characterNameLabel");

            Assert.That(input.text, Is.EqualTo("Kenan"), "le nom confirme doit etre reaffiche");
            Assert.That(nameLabel.text, Is.EqualTo(confirmedCharacter.DisplayName), "le personnage confirme doit etre reaffiche");
            Assert.That(flow.CurrentIndex, Is.EqualTo(catalog.IndexOf(confirmedCharacter.Id)));
        }

        /// <summary>
        /// Sans profil confirme, la reouverture doit repartir d'un champ vide. La couche App reecrit
        /// chaque libelle a l'ouverture : autrement une saisie refusee puis abandonnee reapparaitrait
        /// sans son message d'erreur, ce qui contredit le principe d'ecrivain unique.
        /// </summary>
        [UnityTest]
        public IEnumerator ReopeningAfterAbandonedRejectionClearsTheNameField()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);

            yield return OpenCharacterSetup();

            var input = (TMP_InputField)GetPrivateField(screen, "nameInputField");
            var validationLabel = (TMP_Text)GetPrivateField(screen, "validationLabel");

            SetInputText(screen, "Kenan!");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True);
            Assert.That(input.text, Is.EqualTo("Kenan!"), "le texte refuse reste visible tant que l'ecran est ouvert");

            ClickSerializedButton(screen, "closeButton");
            yield return null;
            Assert.That(screen.gameObject.activeSelf, Is.False);

            yield return OpenCharacterSetup();

            Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.False, "aucun profil n'a ete confirme");
            Assert.That(input.text, Is.Empty, "sans profil confirme, la reouverture doit repartir d'un champ vide");
            Assert.That(validationLabel.gameObject.activeInHierarchy, Is.False, "le message de refus abandonne ne doit pas survivre a la reouverture");
        }

        // ------------------------------------------------ Profil existant : inchange ou remplace

        /// <summary>
        /// "Profil du depot inchange" se prouve sur un depot deja ecrit, pas sur un depot vide :
        /// un refus doit laisser le profil confirme identique, champ par champ et instance comprise.
        /// </summary>
        [UnityTest]
        public IEnumerator RejectedNameLeavesAnAlreadyConfirmedProfileStrictlyUnchanged()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);
            var catalog = GetCatalog(flow);

            yield return OpenCharacterSetup();

            SetInputText(screen, "Kenan");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            var store = RoadRageBootstrap.Instance.Profiles;
            Assert.That(store.HasProfile, Is.True, "le profil de reference doit avoir ete ecrit");

            var confirmedProfile = store.Current;
            var confirmedName = store.Current.DisplayName;
            var confirmedId = store.Current.CharacterId;

            yield return OpenCharacterSetup();

            UserNotice? published = null;
            RoadRageBootstrap.Instance.Notices.NoticePublished += notice => published = notice;

            SetInputText(screen, "a<b>");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            var validationLabel = (TMP_Text)GetPrivateField(screen, "validationLabel");
            Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True, "le refus doit rester visible");
            Assert.That(screen.gameObject.activeInHierarchy, Is.True, "l'ecran doit rester ouvert apres un refus");
            Assert.That(published, Is.Null, "un refus ne publie aucune notice de confirmation");

            Assert.That(store.Current, Is.SameAs(confirmedProfile), "le refus ne doit pas remplacer l'instance de profil");
            Assert.That(store.Current.DisplayName, Is.EqualTo(confirmedName), "le nom confirme doit rester intact");
            Assert.That(store.Current.CharacterId, Is.EqualTo(confirmedId), "l'id de personnage confirme doit rester intact");
            Assert.That(store.Current.CharacterId, Is.EqualTo(catalog.GetAt(0).Id));
        }

        /// <summary>
        /// Le symetrique : une seconde confirmation valide doit bien remplacer les deux champs,
        /// sinon "profil inchange" serait vrai pour la mauvaise raison.
        /// </summary>
        [UnityTest]
        public IEnumerator ConfirmingAgainOverwritesBothProfileFields()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);
            var catalog = GetCatalog(flow);
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "le remplacement de personnage exige deux entrees");

            yield return OpenCharacterSetup();

            SetInputText(screen, "Kenan");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            var store = RoadRageBootstrap.Instance.Profiles;
            Assert.That(store.Current.DisplayName, Is.EqualTo("Kenan"));
            Assert.That(store.Current.CharacterId, Is.EqualTo(catalog.GetAt(0).Id));

            yield return OpenCharacterSetup();

            ClickSerializedButton(screen, "characterCycleButton");
            yield return null;

            var replacementCharacter = catalog.GetAt(flow.CurrentIndex);
            Assert.That(replacementCharacter.Id, Is.Not.EqualTo(catalog.GetAt(0).Id), "le cycle doit avoir change de personnage");

            SetInputText(screen, "Rider");
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            Assert.That(store.Current.DisplayName, Is.EqualTo("Rider"), "le nom doit avoir ete remplace");
            Assert.That(store.Current.CharacterId, Is.EqualTo(replacementCharacter.Id), "l'id de personnage doit avoir ete remplace");
            Assert.That(store.Current.CharacterId.IsEmpty, Is.False);
            Assert.That(screen.gameObject.activeSelf, Is.False, "l'ecran doit se refermer sur la seconde confirmation");
        }

        // ------------------------------------------------ Accessibilite reelle des controles

        /// <summary>
        /// Tous les autres tests cliquent via onClick.Invoke(), ce qui court-circuite le layout : un
        /// controle pousse hors du canvas resterait vert alors que le joueur ne peut plus l'atteindre.
        /// C'est exactement la regression rencontree sur le bouton Personnage. Portee volontairement
        /// limitee aux controles ajoutes par la Story 1.3 : les elements des Stories 1.1 et 1.2
        /// debordent deja pour une cause anterieure, laissee a une passe de layout dediee.
        /// </summary>
        [UnityTest]
        public IEnumerator Story13ControlsStayInsideTheCanvasSoAPlayerCanReachThem()
        {
            yield return PressPlayAndEnterLobbyShell();

            var lobbyShell = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyShell != null, Is.True);

            var canvas = lobbyShell.GetComponentInParent<Canvas>();
            Assert.That(canvas != null, Is.True, "la coquille de lobby doit vivre sous un Canvas");
            var canvasRect = (RectTransform)canvas.transform;

            Canvas.ForceUpdateCanvases();
            AssertInsideCanvas(canvasRect, lobbyShell, "characterSetupButton");

            yield return OpenCharacterSetup();
            yield return null;

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);
            Assert.That(screen.gameObject.activeInHierarchy, Is.True);

            Canvas.ForceUpdateCanvases();
            AssertInsideCanvas(canvasRect, screen, "characterCycleButton");
            AssertInsideCanvas(canvasRect, screen, "confirmButton");
            AssertInsideCanvas(canvasRect, screen, "closeButton");
            AssertInsideCanvas(canvasRect, screen, "nameInputField");
            AssertInsideCanvas(canvasRect, screen, "characterNameLabel");
            AssertInsideCanvas(canvasRect, screen, "validationLabel");
        }

        // ------------------------------------------------ Fermeture sans confirmation

        [UnityTest]
        public IEnumerator ClosingWithoutConfirmingHidesScreenAndLeavesProfileUnchanged()
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);

            yield return OpenCharacterSetup();

            SetInputText(screen, "Kenan");
            ClickSerializedButton(screen, "closeButton");
            yield return null;

            Assert.That(screen.gameObject.activeSelf, Is.False, "Fermer doit masquer l'ecran");
            Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.False, "Fermer ne doit rien ecrire");
        }

        // ------------------------------------------------ Helpers

        /// <summary>
        /// Verifie une ligne "nom refuse" de la matrice : retour visible, depot strictement inchange,
        /// ecran toujours ouvert.
        /// </summary>
        private static IEnumerator AssertRejectedName(string raw)
        {
            yield return PressPlayAndEnterLobbyShell();

            var flow = Object.FindAnyObjectByType<PlayerProfileFlowController>();
            var screen = GetScreen(flow);

            yield return OpenCharacterSetup();

            var validationLabel = (TMP_Text)GetPrivateField(screen, "validationLabel");
            Assert.That(validationLabel.gameObject.activeInHierarchy, Is.False, "aucune validation visible avant la tentative");

            UserNotice? published = null;
            RoadRageBootstrap.Instance.Notices.NoticePublished += notice => published = notice;

            SetInputText(screen, raw);
            ClickSerializedButton(screen, "confirmButton");
            yield return null;

            Assert.That(validationLabel.gameObject.activeInHierarchy, Is.True, "un nom refuse doit afficher un retour visible");
            Assert.That(validationLabel.text, Is.Not.Empty, "le libelle de validation doit nommer la regle violee");
            Assert.That(RoadRageBootstrap.Instance.Profiles.HasProfile, Is.False, "un nom refuse ne doit jamais ecrire le profil");
            Assert.That(screen.gameObject.activeInHierarchy, Is.True, "l'ecran doit rester ouvert apres un refus");
            Assert.That(published, Is.Null, "un refus ne publie pas de notice de confirmation");
        }

        private static IEnumerator PressPlayAndEnterLobbyShell()
        {
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen != null, Is.True, "MainMenuScreen introuvable dans MainMenuLobby");

            ClickSerializedButton(menuScreen, "playButton");
            yield return null;
        }

        /// <summary>
        /// Ouvre le setup en cliquant le vrai bouton Personnage de la coquille de lobby : la liaison
        /// serialisee et le listener d'Awake sont donc tous deux exerces.
        /// </summary>
        private static IEnumerator OpenCharacterSetup()
        {
            var lobbyShell = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyShell != null, Is.True, "LobbyShellScreen introuvable : la coquille de lobby n'est pas ouverte");

            ClickSerializedButton(lobbyShell, "characterSetupButton");
            yield return null;
        }

        private static CharacterSetupScreen GetScreen(PlayerProfileFlowController flow)
        {
            Assert.That(flow != null, Is.True, "PlayerProfileFlowController introuvable dans la scene");
            var screen = GetPrivateField(flow, "characterSetupScreen") as CharacterSetupScreen;
            Assert.That(screen != null, Is.True, "PlayerProfileFlowController.characterSetupScreen doit etre cable dans la scene");
            return screen;
        }

        private static CharacterCatalog GetCatalog(PlayerProfileFlowController flow)
        {
            var catalog = GetPrivateField(flow, "catalog") as CharacterCatalog;
            Assert.That(catalog != null, Is.True, "PlayerProfileFlowController.catalog doit etre cable dans la scene");
            return catalog;
        }

        private static void SetInputText(CharacterSetupScreen screen, string value)
        {
            var input = (TMP_InputField)GetPrivateField(screen, "nameInputField");
            Assert.That(input != null, Is.True, "CharacterSetupScreen.nameInputField doit referencer le champ de la scene");
            input.text = value;
        }

        /// <summary>
        /// Verifie que le controle reference par un champ serialise tient entierement dans le
        /// rectangle du canvas : les quatre coins monde du controle doivent etre a l'interieur.
        /// Un controle deplace hors de l'ecran echoue ici, la ou onClick.Invoke() ne verrait rien.
        /// </summary>
        private static void AssertInsideCanvas(RectTransform canvasRect, Component owner, string fieldName)
        {
            var target = GetPrivateField(owner, fieldName) as Component;
            Assert.That(target != null, Is.True, owner.GetType().Name + "." + fieldName + " doit referencer un objet de la scene");

            var rect = target.transform as RectTransform;
            Assert.That(rect != null, Is.True, fieldName + " doit porter un RectTransform");

            var canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);
            var minX = canvasCorners[0].x;
            var minY = canvasCorners[0].y;
            var maxX = canvasCorners[2].x;
            var maxY = canvasCorners[2].y;

            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            for (var i = 0; i < corners.Length; i++)
            {
                Assert.That(corners[i].x, Is.InRange(minX, maxX),
                    fieldName + " : coin " + i + " hors du canvas en x (canvas [" + minX + ".." + maxX + "])");
                Assert.That(corners[i].y, Is.InRange(minY, maxY),
                    fieldName + " : coin " + i + " hors du canvas en y (canvas [" + minY + ".." + maxY + "])");
            }
        }

        private static void AssertSerializedButtonExists(Component component, string fieldName)
        {
            var button = GetPrivateField(component, fieldName) as Button;
            Assert.That(button != null, Is.True, component.GetType().Name + "." + fieldName + " doit referencer un Button de la scene");
            Assert.That(button.gameObject.activeInHierarchy, Is.True, fieldName + " doit etre visible");
        }

        /// <summary>
        /// Clique un bouton reellement reference par un champ serialise : un bouton non cable, ou cable
        /// sur le mauvais objet, fait echouer le test au lieu de passer au vert.
        /// </summary>
        private static void ClickSerializedButton(Component component, string buttonFieldName)
        {
            var button = GetPrivateField(component, buttonFieldName) as Button;
            Assert.That(button != null, Is.True, component.GetType().Name + "." + buttonFieldName + " doit referencer un Button de la scene");
            button.onClick.Invoke();
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
