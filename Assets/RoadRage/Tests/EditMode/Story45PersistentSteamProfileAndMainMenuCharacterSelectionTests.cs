using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.MainMenu;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Definitions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 4.5 : profil Steam persistant et selection de personnage dans le menu principal.
    /// Couvre la matrice de resolution du profil (premier lancement, relance, fichier corrompu,
    /// identifiant inconnu, nom Steam inutilisable, Steam indisponible), la purete de la persistance
    /// et le cablage de scene qui remplace le flux manuel de la Story 1.3.
    /// </summary>
    public sealed class Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
    {
        private const string MainMenuLobbyScenePath = "Assets/RoadRage/App/Scenes/MainMenuLobby.unity";
        private const string CharacterCatalogAssetPath = "Assets/RoadRage/ScriptableObjects/Players/CharacterCatalog.asset";
        private const string ProfileRecordPath = "Assets/RoadRage/Features/Players/PersistentPlayerProfileRecord.cs";
        private const string ProfileFileStorePath = "Assets/RoadRage/Features/Players/PlayerProfileFileStore.cs";
        private const string ProfileBootstrapPath = "Assets/RoadRage/Features/Players/PlayerProfileBootstrapService.cs";
        private const string MenuScreenPath = "Assets/RoadRage/Features/UI/MainMenuScreen.cs";
        private const string MenuPreviewPath = "Assets/RoadRage/Features/UI/MenuCharacterPreview.cs";
        private const string MenuProfileFlowPath = "Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs";
        private const string RemovedSetupScreenPath = "Assets/RoadRage/Features/UI/CharacterSetupScreen.cs";
        private const string RemovedProfileFlowPath = "Assets/RoadRage/App/Players/PlayerProfileFlowController.cs";
        private const string SteamId = "76561198000000001";
        private const string OtherSteamId = "76561198000000002";

        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        private string tempDirectory;

        private PlayerProfileFileStore fileStore;

        private CharacterCatalog catalog;

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in spawned)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            spawned.Clear();

            if (tempDirectory != null && Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }

            tempDirectory = null;
            fileStore = null;
            catalog = null;
        }

        // ---------------------------------------------------------------- Matrice de resolution

        [Test]
        public void FirstLaunchWithSteamCreatesRookieProfileAndPersistsIt()
        {
            var service = NewService();
            var rookie = LoadCatalog().GetAt(0);

            var resolution = service.Resolve(true, "Kenan", SteamId);

            Assert.That(resolution.IsResolved, Is.True, resolution.Error);
            Assert.That(resolution.Profile.DisplayName, Is.EqualTo("Kenan"), "le nom du profil vient de l'identite Steam");
            Assert.That(resolution.Profile.CharacterId.Value, Is.EqualTo(PlayerProfileBootstrapService.DefaultCharacterId));
            Assert.That(resolution.Profile.CharacterId, Is.EqualTo(rookie.Id), "Rookie est le personnage par defaut");
            Assert.That(resolution.ShouldPersist, Is.True, "un profil cree doit etre ecrit");
            Assert.That(resolution.Error, Is.Empty);
            Assert.That(File.Exists(fileStore.FilePath), Is.False, "la resolution ne doit rien ecrire elle-meme");

            Assert.That(service.TryPersist(resolution.Profile, SteamId, resolution.ShouldPersist), Is.True);
            Assert.That(File.Exists(fileStore.FilePath), Is.True, "le profil cree doit survivre a la fermeture du menu");

            string rawName;
            DefinitionId rawId;
            AssertReloadedProfile(out rawName, out rawId);
            Assert.That(rawName, Is.EqualTo("Kenan"));
            Assert.That(rawId, Is.EqualTo(rookie.Id));
        }

        [Test]
        public void RelaunchAndSelectionChangeRestoreWhatWasWritten()
        {
            var service = NewService();
            var veteran = LoadCatalog().GetAt(1);

            // Selection changee dans le menu : le profil remplace est ecrit tel quel.
            var selected = new PlayerProfile("Kenan", veteran.Id);
            Assert.That(service.TryPersist(selected, SteamId, true), Is.True);

            // Relance : un service neuf relit le fichier sans aucune confirmation ni ecran intermediaire.
            var relaunched = new PlayerProfileBootstrapService(LoadCatalog(), new PlayerProfileFileStore(fileStore.FilePath));
            var resolution = relaunched.Resolve(true, "Autre Nom Steam", SteamId);

            Assert.That(resolution.IsResolved, Is.True, resolution.Error);
            Assert.That(resolution.Profile.CharacterId, Is.EqualTo(veteran.Id), "le choix persistant prime sur le defaut");
            Assert.That(resolution.Profile.DisplayName, Is.EqualTo("Kenan"), "le nom persistant prime sur le nom Steam courant");
            Assert.That(resolution.ShouldPersist, Is.False, "un fichier valide ne doit pas etre reecrit");
        }

        /// <summary>
        /// Deux comptes Steam peuvent partager le meme dossier persistant : le fichier doit appartenir
        /// a un compte precis, jamais etre adopte par le suivant.
        /// </summary>
        [Test]
        public void AnotherSteamAccountDoesNotInheritTheStoredProfile()
        {
            var service = NewService();
            var veteran = LoadCatalog().GetAt(1);

            Assert.That(service.TryPersist(new PlayerProfile("Kenan", veteran.Id), OtherSteamId, true), Is.True);

            var resolution = service.Resolve(true, "Second Compte", SteamId);

            Assert.That(resolution.IsResolved, Is.True, resolution.Error);
            Assert.That(resolution.Profile.CharacterId.Value, Is.EqualTo(PlayerProfileBootstrapService.DefaultCharacterId),
                "un autre compte Steam ne doit pas heriter du personnage stocke");
            Assert.That(resolution.Profile.DisplayName, Is.EqualTo("Second Compte"),
                "un autre compte Steam ne doit pas heriter du nom stocke");
            Assert.That(resolution.ShouldPersist, Is.True, "le fichier doit etre repris par le compte courant");

            Assert.That(service.TryPersist(resolution.Profile, SteamId, true), Is.True);

            PlayerProfile reloaded;
            Assert.That(new PlayerProfileFileStore(fileStore.FilePath).TryLoad(SteamId, out reloaded), Is.True,
                "le fichier doit desormais appartenir au compte courant");
            Assert.That(reloaded.CharacterId.Value, Is.EqualTo(PlayerProfileBootstrapService.DefaultCharacterId));
            Assert.That(reloaded.DisplayName, Is.EqualTo("Second Compte"));

            PlayerProfile previousAccount;
            Assert.That(new PlayerProfileFileStore(fileStore.FilePath).TryLoad(OtherSteamId, out previousAccount), Is.False,
                "le compte precedent ne doit plus pouvoir relire ce fichier");
        }

        [Test]
        public void SteamUnavailableKeepsRookieInMemoryAndWritesNothing()
        {
            var service = NewService();

            var resolution = service.Resolve(false, "Kenan", string.Empty);

            Assert.That(resolution.IsResolved, Is.True, resolution.Error);
            Assert.That(resolution.Profile.CharacterId.Value, Is.EqualTo(PlayerProfileBootstrapService.DefaultCharacterId));
            Assert.That(resolution.Profile.DisplayName, Is.EqualTo("Rookie"), "sans Steam, le libelle retombe sur le personnage");
            Assert.That(resolution.ShouldPersist, Is.False, "aucune identite ne doit etre persistee sans Steam");
            Assert.That(resolution.Error, Is.EqualTo(PlayerProfileBootstrapService.SteamUnavailableMessage));

            Assert.That(service.TryPersist(resolution.Profile, string.Empty, resolution.ShouldPersist), Is.False);
            Assert.That(File.Exists(fileStore.FilePath), Is.False, "aucun fichier ne doit naitre sans Steam");
        }

        [Test]
        public void CorruptProfileFileIsIgnoredAndRecreated()
        {
            var service = NewService();
            Directory.CreateDirectory(tempDirectory);
            File.WriteAllText(fileStore.FilePath, "{ ceci n'est pas du json");

            var resolution = service.Resolve(true, "Kenan", SteamId);

            Assert.That(resolution.IsResolved, Is.True, resolution.Error);
            Assert.That(resolution.Profile.CharacterId.Value, Is.EqualTo(PlayerProfileBootstrapService.DefaultCharacterId));
            Assert.That(resolution.ShouldPersist, Is.True, "un fichier illisible doit etre corrige");

            Assert.That(service.TryPersist(resolution.Profile, SteamId, true), Is.True);

            string rawName;
            DefinitionId rawId;
            AssertReloadedProfile(out rawName, out rawId);
            Assert.That(rawName, Is.EqualTo("Kenan"));
            Assert.That(rawId.Value, Is.EqualTo(PlayerProfileBootstrapService.DefaultCharacterId));
        }

        [Test]
        public void UnknownCharacterIdFallsBackToRookieAndRepairsTheFile()
        {
            var service = NewService();
            Assert.That(fileStore.TrySave(new PlayerProfile("Kenan", new DefinitionId("char_ghost")), SteamId), Is.True);

            var resolution = service.Resolve(true, "Kenan", SteamId);

            Assert.That(resolution.IsResolved, Is.True, resolution.Error);
            Assert.That(resolution.Profile.CharacterId.Value, Is.EqualTo(PlayerProfileBootstrapService.DefaultCharacterId),
                "un id absent du catalogue ne doit pas bloquer le menu");
            Assert.That(resolution.ShouldPersist, Is.True, "le fichier doit etre corrige");

            Assert.That(service.TryPersist(resolution.Profile, SteamId, true), Is.True);

            string rawName;
            DefinitionId rawId;
            AssertReloadedProfile(out rawName, out rawId);
            Assert.That(rawId.Value, Is.EqualTo(PlayerProfileBootstrapService.DefaultCharacterId));
        }

        [Test]
        public void UnusableSteamNameFallsBackToTheCharacterDisplayName()
        {
            var service = NewService();
            var rookie = LoadCatalog().GetAt(0);

            foreach (var rawName in new[] { string.Empty, "!!", "nom beaucoup trop long pour la regle des vingt" })
            {
                var resolution = service.Resolve(true, rawName, SteamId);

                Assert.That(resolution.IsResolved, Is.True, resolution.Error);
                Assert.That(resolution.Profile.DisplayName, Is.EqualTo(rookie.DisplayName),
                    "nom Steam refuse : le libelle retombe sur le personnage, jamais sur du brut. Entree : " + rawName);
                Assert.That(resolution.Error, Is.Empty, "un nom Steam refuse n'est pas une erreur joueur visible");
            }
        }

        [Test]
        public void EmptyCatalogIsReportedWithoutThrowing()
        {
            var emptyCatalog = ScriptableObject.CreateInstance<CharacterCatalog>();
            spawned.Add(emptyCatalog);

            var service = new PlayerProfileBootstrapService(emptyCatalog, FileStore());

            var resolution = service.Resolve(true, "Kenan", SteamId);

            Assert.That(resolution.IsResolved, Is.False);
            Assert.That(resolution.Profile, Is.Null);
            Assert.That(resolution.Error, Is.EqualTo(PlayerProfileBootstrapService.EmptyCatalogMessage));
        }

        // ---------------------------------------------------------------- Purete de la persistance

        [Test]
        public void PersistentRecordCarriesOnlyIdentityAndCosmeticChoice()
        {
            var fields = typeof(PersistentPlayerProfileRecord)
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(field => field.Name)
                .ToArray();

            Assert.That(fields, Is.EquivalentTo(new[] { "displayName", "characterId", "steamId" }),
                "le profil persistant ne contient que l'identite du compte Steam et le choix cosmetique : jamais d'etat de session, de run ou d'economie");
        }

        [Test]
        public void ProfilePersistenceTypesArePlainClassesWithoutUnityBaseTypes()
        {
            foreach (var type in new[]
                     {
                         typeof(PersistentPlayerProfileRecord),
                         typeof(PlayerProfileFileStore),
                         typeof(PlayerProfileBootstrapService)
                     })
            {
                Assert.That(typeof(MonoBehaviour).IsAssignableFrom(type), Is.False, type.Name + " must not be a MonoBehaviour");
                Assert.That(typeof(ScriptableObject).IsAssignableFrom(type), Is.False, type.Name + " must not be a ScriptableObject");
                Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(type), Is.False, type.Name + " must not derive from UnityEngine.Object");
            }
        }

        [Test]
        public void ProfileFileStoreRefusesAnEmptyPath()
        {
            Assert.Throws<ArgumentException>(() => new PlayerProfileFileStore(string.Empty));
            Assert.Throws<ArgumentException>(() => new PlayerProfileFileStore("   "));
        }

        /// <summary>
        /// Sans compte proprietaire, rien ne doit etre lu ni ecrit : un fichier orphelin serait
        /// silencieusement adopte par le prochain compte Steam de la machine.
        /// </summary>
        [Test]
        public void ProfileFileStoreNeverReadsOrWritesWithoutAnOwner()
        {
            var store = FileStore();
            var character = LoadCatalog().GetAt(0);
            Assert.That(character, Is.Not.Null, "le catalogue doit contenir un personnage par defaut");
            var profile = new PlayerProfile("Kenan", character.Id);

            Assert.That(store.TrySave(profile, string.Empty), Is.False, "un profil sans proprietaire ne doit jamais etre ecrit");
            Assert.That(store.TrySave(profile, "   "), Is.False);
            Assert.That(File.Exists(store.FilePath), Is.False, "aucun fichier orphelin ne doit naitre");

            Assert.That(store.TrySave(profile, SteamId), Is.True);

            PlayerProfile loaded;
            Assert.That(store.TryLoad(string.Empty, out loaded), Is.False, "sans compte courant, rien n'est restaure");
            Assert.That(loaded, Is.Null);
            Assert.That(store.TryLoad(OtherSteamId, out loaded), Is.False, "un autre compte ne lit pas ce fichier");
            Assert.That(store.TryLoad(SteamId, out loaded), Is.True);
            Assert.That(loaded.CharacterId, Is.EqualTo(character.Id));
        }

        // ---------------------------------------------------------------- Cablage et frontieres

        [Test]
        public void MainMenuSceneWiresTheSelectionSurfaceAndTheProfileFlow()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuLobbyScenePath, OpenSceneMode.Additive);
            try
            {
                var screen = FindComponentInScene<MainMenuScreen>(scene);
                Assert.That(screen, Is.Not.Null, "MainMenuScreen attendu dans la scene menu");

                AssertSerializedReference(screen, "characterPreview");
                AssertSerializedReference(screen, "primaryCharacterButton");
                AssertSerializedReference(screen, "secondaryCharacterButton");
                AssertSerializedReference(screen, "selectedCharacterLabel");

                var preview = FindComponentInScene<MenuCharacterPreview>(scene);
                Assert.That(preview, Is.Not.Null, "l'apercu 3D du menu doit etre cable dans la scene");
                Assert.That(preview.GetComponent<RawImage>(), Is.Not.Null, "l'apercu doit etre affiche par un RawImage");

                var flow = FindComponentInScene<MainMenuProfileFlowController>(scene);
                Assert.That(flow, Is.Not.Null, "MainMenuProfileFlowController attendu dans la scene menu");
                AssertSerializedReference(flow, "screen");
                AssertSerializedReference(flow, "catalog");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ManualProfileSetupFlowIsGone()
        {
            Assert.That(File.Exists(RemovedSetupScreenPath), Is.False, "l'ecran de setup manuel doit avoir disparu : " + RemovedSetupScreenPath);
            Assert.That(File.Exists(RemovedProfileFlowPath), Is.False, "le flux manuel de profil doit avoir disparu : " + RemovedProfileFlowPath);

            var scene = EditorSceneManager.OpenScene(MainMenuLobbyScenePath, OpenSceneMode.Additive);
            try
            {
                foreach (var removed in new[] { "CharacterPanel", "CharacterButton", "PlayerProfileFlow" })
                {
                    Assert.That(FindTransformInScene(scene, removed), Is.Null, removed + " ne doit plus exister dans la scene menu");
                }

                var lobbyFields = typeof(LobbyShellScreen)
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Select(field => field.Name)
                    .ToArray();

                Assert.That(lobbyFields, Does.Not.Contain("characterSetupButton"),
                    "le lobby ne doit plus exposer de surface de selection de personnage");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void MenuSurfacesStayFreeOfGameplayFeaturesAndSceneRouting()
        {
            foreach (var path in new[] { MenuScreenPath, MenuPreviewPath })
            {
                var source = File.ReadAllText(path);

                foreach (var forbidden in new[]
                         {
                             "SceneManagement",
                             "SceneManager",
                             "Application.Quit",
                             "PlayerProfile",
                             "CharacterCatalog",
                             "CharacterDef",
                             "RoadRage.Features.Players",
                             "RoadRage.Features.Online",
                             "NetworkBehaviour",
                             "NetworkVariable"
                         })
                {
                    Assert.That(source, Does.Not.Contain(forbidden),
                        Path.GetFileName(path) + " doit rester une surface de presentation : " + forbidden);
                }
            }
        }

        [Test]
        public void MenuProfileFlowNeverTouchesTheNetworkedChain()
        {
            var source = File.ReadAllText(MenuProfileFlowPath);

            foreach (var forbidden in new[]
                     {
                         "NetworkBehaviour",
                         "NetworkVariable",
                         "NetworkObject",
                         "ConnectionData",
                         "StartHost(",
                         "StartClient(",
                         "SceneManager"
                     })
            {
                Assert.That(source, Does.Not.Contain(forbidden),
                    "MainMenuProfileFlowController ne doit pas toucher la chaine reseau ni le routage : " + forbidden);
            }
        }

        [Test]
        public void ProfileFileStoreKeepsAnInjectedPathAndReusesTheProjectNameValidator()
        {
            Assert.That(File.ReadAllText(ProfileFileStorePath), Does.Contain("FilePath"),
                "le port de persistance doit rester adosse a un chemin injecte, jamais a un chemin code en dur");

            Assert.That(File.ReadAllText(ProfileBootstrapPath), Does.Contain("PlayerNameValidator.TryNormalize"),
                "la normalisation du nom Steam doit passer par l'unique validateur de nom du projet");

            Assert.That(File.ReadAllText(ProfileRecordPath), Does.Contain("characterId"),
                "le profil persistant doit porter l'id stable du personnage, pas une teinte ni un index");
        }

        // ---------------------------------------------------------------- Helpers

        private PlayerProfileBootstrapService NewService()
        {
            return new PlayerProfileBootstrapService(LoadCatalog(), FileStore());
        }

        private PlayerProfileFileStore FileStore()
        {
            if (fileStore == null)
            {
                tempDirectory = Path.Combine(Path.GetTempPath(), "roadrage-story45-" + Guid.NewGuid().ToString("N"));
                fileStore = new PlayerProfileFileStore(Path.Combine(tempDirectory, "player-profile.json"));
            }

            return fileStore;
        }

        private CharacterCatalog LoadCatalog()
        {
            if (catalog == null)
            {
                catalog = AssetDatabase.LoadAssetAtPath<CharacterCatalog>(CharacterCatalogAssetPath);
                Assert.That(catalog, Is.Not.Null, "catalogue introuvable : " + CharacterCatalogAssetPath);
            }

            return catalog;
        }

        private void AssertReloadedProfile(out string displayName, out DefinitionId characterId)
        {
            PlayerProfile reloaded;
            Assert.That(new PlayerProfileFileStore(fileStore.FilePath).TryLoad(SteamId, out reloaded), Is.True,
                "le fichier ecrit doit etre relisible par son proprietaire");
            Assert.That(reloaded, Is.Not.Null);

            displayName = reloaded.DisplayName;
            characterId = reloaded.CharacterId;
        }

        private static Transform FindTransformInScene(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindRecursive(root.transform, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindRecursive(Transform current, string name)
        {
            if (current.name == name)
            {
                return current;
            }

            foreach (Transform child in current)
            {
                var found = FindRecursive(child, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static void AssertSerializedReference(Component component, string fieldName)
        {
            var field = component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, component.GetType().Name + "." + fieldName + " doit exister");
            Assert.That(field.GetValue(component) as UnityEngine.Object, Is.Not.Null,
                component.GetType().Name + "." + fieldName + " doit etre cable dans la scene");
        }
    }
}
