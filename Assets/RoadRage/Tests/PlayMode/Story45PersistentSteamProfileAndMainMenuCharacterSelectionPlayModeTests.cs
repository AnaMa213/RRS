using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.MainMenu;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 4.5 : parcours reel du menu principal. Verifie que l'ouverture du menu resout un profil
    /// utilisable, que l'apercu 3D est monte, que la selection d'emplacement publie et persiste selon
    /// la disponibilite de Steam, qu'aucune saisie de nom n'existe et que le lobby reste en lecture seule.
    /// Le chemin du profil est redirige vers un fichier temporaire : le test ne doit ni lire ni ecraser
    /// le profil reel de la machine.
    /// </summary>
    public sealed class Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests
    {
        private string originalProfileFilePath;

        private string tempProfileFilePath;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var survivor = RoadRageBootstrap.Instance;
            if (survivor != null)
            {
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
        public IEnumerator MenuResolvesProfileAndPublishesTheChosenCharacter()
        {
            RedirectProfilePath();
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var bootstrap = RoadRageBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null, "RoadRageBootstrap attendu apres chargement du menu");

            var screen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(screen, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");

            var flow = Object.FindAnyObjectByType<MainMenuProfileFlowController>();
            Assert.That(flow, Is.Not.Null, "MainMenuProfileFlowController attendu dans la scene menu");

            var catalog = GetCatalog(flow);
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "deux emplacements cosmetiques attendus");

            Assert.That(bootstrap.Profiles.HasProfile, Is.True, "le menu doit resoudre un profil des l'ouverture");
            var resolved = bootstrap.Profiles.Current;
            Assert.That(resolved.DisplayName, Is.Not.Empty, "le profil resolu doit porter un libelle");

            var menuPanel = GetPrivateField(screen, "menuPanel") as GameObject;
            Assert.That(menuPanel, Is.Not.Null, "MainMenuScreen.menuPanel doit etre cable");
            Assert.That(menuPanel.GetComponentInChildren<TMP_InputField>(true), Is.Null,
                "aucun champ de saisie de nom ne doit subsister dans le menu principal");

            var preview = Object.FindAnyObjectByType<MenuCharacterPreview>();
            Assert.That(preview, Is.Not.Null, "l'apercu 3D doit etre monte dans le menu");
            var previewImage = preview.GetComponent<RawImage>();
            Assert.That(previewImage.texture, Is.Not.Null, "un RenderTexture doit alimenter l'apercu");
            Assert.That(GameObject.Find("MenuCharacterPreviewRig"), Is.Not.Null, "le banc de rendu de l'apercu doit exister");
            AssertPreviewRendersAndRotates(preview, previewImage);

            var steamOnline = bootstrap.OnlineServices != null && bootstrap.OnlineServices.Status == OnlineServicesStatus.Online;

            ClickSerializedButton(screen, "secondaryCharacterButton");
            yield return null;

            var veteran = catalog.GetAt((int)MainMenuScreen.CharacterOption.Secondary);
            Assert.That(bootstrap.Profiles.Current.CharacterId, Is.EqualTo(veteran.Id),
                "le second emplacement doit publier le second personnage du catalogue");
            AssertScreenShows(screen, veteran.DisplayName);

            if (steamOnline)
            {
                AssertPersistedCharacterIs(veteran.Id,
                    "avec Steam, un changement de selection doit etre ecrit sur disque, pas seulement le profil initial");
            }
            else
            {
                Assert.That(File.Exists(tempProfileFilePath), Is.False,
                    "sans Steam, aucun profil ne doit etre ecrit");
            }

            ClickSerializedButton(screen, "primaryCharacterButton");
            yield return null;

            var rookie = catalog.GetAt((int)MainMenuScreen.CharacterOption.Primary);
            Assert.That(bootstrap.Profiles.Current.CharacterId, Is.EqualTo(rookie.Id),
                "le premier emplacement doit republier le premier personnage du catalogue");
            AssertScreenShows(screen, rookie.DisplayName);

            if (steamOnline)
            {
                AssertPersistedCharacterIs(rookie.Id, "revenir au premier emplacement doit aussi etre ecrit sur disque");
            }
        }

        [UnityTest]
        public IEnumerator LobbyKeepsNoCharacterSelectionControl()
        {
            RedirectProfilePath();
            SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            yield return null;
            yield return null;

            var bootstrap = RoadRageBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null, "RoadRageBootstrap attendu apres chargement du menu");

            var screen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(screen, Is.Not.Null);
            Assert.That(bootstrap.Profiles.HasProfile, Is.True, "le menu doit avoir resolu un profil avant le lobby");

            var published = bootstrap.Profiles.Current;

            ClickSerializedButton(screen, "playButton");
            yield return null;

            var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu apres Play");

            var lobbySelectionFields = typeof(LobbyShellScreen)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(field => field.Name)
                .Where(name => name.ToLowerInvariant().Contains("character"))
                .ToArray();

            Assert.That(lobbySelectionFields, Is.Empty,
                "le lobby ne doit exposer aucun controle de personnage : la selection est gelee a l'entree");

            Assert.That(bootstrap.Profiles.Current, Is.SameAs(published),
                "entrer dans le lobby ne doit pas republier ni remplacer la selection");
        }

        /// <summary>
        /// Redirige le fichier de profil avant tout chargement de scene : le bootstrap lit le chemin
        /// par defaut a sa creation, donc l'override doit preceder le chargement.
        /// </summary>
        private void RedirectProfilePath()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story45-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;
        }

        /// <summary>
        /// Verifie le contenu relu du fichier, jamais sa seule existence : un fichier cree au premier
        /// lancement reste present meme si aucun changement de selection n'est jamais ecrit.
        /// La lecture est faite avec l'id du compte local : le fichier appartient a un compte precis.
        /// </summary>
        private void AssertPersistedCharacterIs(DefinitionId expectedCharacterId, string because)
        {
            PlayerProfile persisted;
            Assert.That(new PlayerProfileFileStore(tempProfileFilePath).TryLoad(LocalSteamId(), out persisted), Is.True, because);
            Assert.That(persisted.CharacterId, Is.EqualTo(expectedCharacterId), because);
        }

        /// <summary>Id du compte Steam local, sous la meme forme que celle ecrite par la couche App.</summary>
        private static string LocalSteamId()
        {
            var identity = RoadRageBootstrap.Instance.SteamIdentity;
            Assert.That(identity, Is.Not.Null, "RoadRageBootstrap.SteamIdentity doit etre cable");

            string playerName;
            ulong steamId;
            Assert.That(identity.TryGetLocalIdentity(out playerName, out steamId), Is.True,
                "la persistance n'est assertee que lorsque Steam est disponible");

            return steamId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private static void AssertScreenShows(MainMenuScreen screen, string expectedLabel)
        {
            var label = GetPrivateField(screen, "selectedCharacterLabel") as TMP_Text;
            Assert.That(label, Is.Not.Null, "MainMenuScreen.selectedCharacterLabel doit etre cable");
            Assert.That(label.text, Is.EqualTo(expectedLabel), "le choix courant doit rester lisible en texte");
        }

        private static void AssertPreviewRendersAndRotates(MenuCharacterPreview preview, RawImage previewImage)
        {
            var renderTexture = previewImage.texture as RenderTexture;
            Assert.That(renderTexture, Is.Not.Null, "l'apercu doit etre rendu dans une RenderTexture");

            var previous = RenderTexture.active;
            var snapshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = renderTexture;
                snapshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                snapshot.Apply();
                Assert.That(snapshot.GetPixels32().Any(pixel => pixel.r != 0 || pixel.g != 0 || pixel.b != 0), Is.True,
                    "l'apercu doit contenir le personnage, pas seulement un RenderTexture vide");
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(snapshot);
            }

            var anchor = GetPrivateField(preview, "anchor") as Transform;
            Assert.That(anchor, Is.Not.Null, "l'apercu doit exposer son ancrage de rotation");
            var rotation = anchor.localRotation;
            preview.OnDrag(new PointerEventData(EventSystem.current) { delta = new Vector2(8f, 0f) });
            Assert.That(Quaternion.Angle(rotation, anchor.localRotation), Is.GreaterThan(0f), "le glisser doit faire tourner l'apercu");
        }

        private static CharacterCatalog GetCatalog(MainMenuProfileFlowController flow)
        {
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
