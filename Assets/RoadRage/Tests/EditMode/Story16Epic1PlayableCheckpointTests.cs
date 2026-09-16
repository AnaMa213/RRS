using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Lobby;
using RoadRage.App.MainMenu;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Authoring;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 1.6 : checkpoint de l'Epic 1. Verifie le parcours jouable assemble,
    /// les placeholders visibles et les notes de passation sans ajouter de nouvelle mecanique.
    /// </summary>
    public sealed class Story16Epic1PlayableCheckpointTests
    {
        private const string BootstrapScenePath = "Assets/RoadRage/App/Scenes/Bootstrap.unity";
        private const string MainMenuLobbyScenePath = "Assets/RoadRage/App/Scenes/MainMenuLobby.unity";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string HandoffNotesPath = "docs/setup/story-1-6-epic-1-playable-checkpoint-notes.md";
        private const string CharacterCatalogAssetPath = "Assets/RoadRage/ScriptableObjects/Players/CharacterCatalog.asset";

        [Test]
        public void EnabledBuildScenesKeepTheEpic1PlayableRoute()
        {
            var enabledScenePaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            CollectionAssert.AreEqual(
                new[] { BootstrapScenePath, MainMenuLobbyScenePath, MvpRunScenePath },
                enabledScenePaths);

            Assert.That(AppSceneRouter.BootstrapSceneName, Is.EqualTo("Bootstrap"));
            Assert.That(AppSceneRouter.MainMenuLobbySceneName, Is.EqualTo("MainMenuLobby"));
            Assert.That(AppSceneRouter.MvpRunSceneName, Is.EqualTo("MVP_Run"));
        }

        [Test]
        public void MainMenuLobbySceneStillWiresMenuLobbyProfileAndNoticeSurfaces()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuLobbyScenePath, OpenSceneMode.Additive);
            try
            {
                Assert.That(FindComponentInScene<MainMenuScreen>(scene), Is.Not.Null, "menu principal attendu");
                Assert.That(FindComponentInScene<MainMenuFlowController>(scene), Is.Not.Null, "flux menu attendu");
                Assert.That(FindComponentInScene<LobbyShellScreen>(scene), Is.Not.Null, "coquille lobby attendue");
                Assert.That(FindComponentInScene<LobbyFlowController>(scene), Is.Not.Null, "flux lobby attendu");
                Assert.That(FindComponentInScene<MainMenuProfileFlowController>(scene), Is.Not.Null, "flux profil du menu attendu");

                var menu = FindComponentInScene<MainMenuScreen>(scene);
                AssertSerializedObjectReference(menu, "noticePanel");
                AssertSerializedObjectReference(menu, "noticeText");

                var lobby = FindComponentInScene<LobbyShellScreen>(scene);
                AssertSerializedObjectReference(lobby, "settingsSummaryLabel");
                AssertSerializedObjectReference(lobby, "startGameButton");
                Assert.That(LobbyFlowController.MissingProfileStartGameMessage, Does.Contain("profil joueur"));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void MvpRunSceneContainsGreyboxWorldSpawnAndCheckpointHud()
        {
            var scene = EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
            try
            {
                var runRoot = FindRoot(scene, "RunRoot");
                Assert.That(runRoot, Is.Not.Null, "RunRoot attendu dans MVP_Run");

                var flow = runRoot.GetComponent<RunFlowController>();
                Assert.That(flow, Is.Not.Null, "RunFlowController attendu");
                AssertSerializedObjectReference(flow, "compositionRoot");
                AssertSerializedObjectReference(flow, "characterCatalog");
                AssertSerializedObjectReference(flow, "playerCamera");
                AssertSerializedObjectReference(flow, "playerSpawnPoint");
                AssertSerializedObjectReference(flow, "checkpointHud");

                var composition = runRoot.GetComponent<RunCompositionRoot>();
                Assert.That(composition, Is.Not.Null, "RunCompositionRoot attendu");
                Assert.That(composition.RuntimeRoot, Is.Not.Null);
                Assert.That(composition.SpawnRoot, Is.Not.Null);

                Assert.That(FindChild(runRoot.transform, "GreyboxMap/Greybox_GroundPlane"), Is.Not.Null);
                AssertDistrictCarriesADrivableGreyboxRoad(runRoot.transform);
                Assert.That(FindChild(runRoot.transform, "GreyboxMap/Greybox_PlayerCar_Parked"), Is.Not.Null);
                Assert.That(FindChild(runRoot.transform, "GreyboxMap/Greybox_CityBlock_A_West"), Is.Not.Null);
                Assert.That(FindChild(runRoot.transform, "GreyboxMap/Greybox_CityBlock_A_East"), Is.Not.Null);

                var hud = FindComponentInScene<RunCheckpointHudScreen>(scene);
                Assert.That(hud, Is.Not.Null, "HUD placeholder du checkpoint attendu");
                AssertSerializedTextContains(hud, "lobbyStateLabel", "Lobby");
                AssertSerializedTextContains(hud, "playerStateLabel", "Joueur");
                AssertSerializedTextContains(hud, "futureHudLabel", "HUD futur");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GreyboxCharactersAndWorldPropsRemainTraceableAndLocalOnly()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CharacterCatalog>(CharacterCatalogAssetPath);
            Assert.That(catalog != null, Is.True, "catalogue personnage introuvable");
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "checkpoint Epic 1 attend au moins deux personnages");

            for (var i = 0; i < catalog.Count; i++)
            {
                var character = catalog.GetAt(i);
                Assert.That(character != null, Is.True, "entree catalogue nulle : " + i);
                Assert.That(character.Id.IsEmpty, Is.False, character.RawId);
                Assert.That(character.PreviewPrefab != null, Is.True, character.RawId + " doit pointer vers un prefab greybox");
                Assert.That(character.PreviewPrefab.GetComponent<NetworkObject>(), Is.Null, "Epic 1 reste local-only : " + character.RawId);

                var metadata = character.PreviewPrefab.GetComponent<GreyboxAssetSeedMetadata>();
                Assert.That(metadata != null, Is.True, "metadata greybox manquante : " + character.RawId);
                Assert.That(metadata.StableId, Is.EqualTo(character.RawId));
            }

            AssertLocalGreyboxPrefab("Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab", "vehicle_player_shared", expectNetworkObjectAbsent: false);
            AssertLocalGreyboxPrefab("Assets/RoadRage/Prefabs/Greybox_CityBlock_A.prefab", "building_city_block_a", expectNetworkObjectAbsent: true);
        }

        [Test]
        public void Epic1RuntimeDoesNotStartOrSpawnNetworkSessions()
        {
            // LobbyFlowController.cs demarre desormais reellement le reseau (Story 2.5, Epic 2) :
            // exclu volontairement de cette garde, qui reste pertinente pour le reste du runtime
            // Epic 1, toujours local-only.
            foreach (var file in new[]
                     {
                         "Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs",
                         "Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs",
                         "Assets/RoadRage/App/Run/RunFlowController.cs",
                         "Assets/RoadRage/Features/UI/MainMenuScreen.cs",
                         "Assets/RoadRage/Features/UI/MenuCharacterPreview.cs",
                         "Assets/RoadRage/Features/UI/LobbyShellScreen.cs",
                         "Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs",
                         "Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs"
                     })
            {
                var source = File.ReadAllText(file);
                Assert.That(source, Does.Not.Contain("StartHost("), file);
                Assert.That(source, Does.Not.Contain("StartClient("), file);
                Assert.That(source, Does.Not.Contain(".Spawn("), file);
            }
        }

        [Test]
        public void HandoffNotesNamePlayableStubAndEpic2ReplacementBoundaries()
        {
            Assert.That(File.Exists(HandoffNotesPath), Is.True, HandoffNotesPath);
            var notes = File.ReadAllText(HandoffNotesPath);

            foreach (var required in new[]
                     {
                         "## Jouable",
                         "## Stubs",
                         "## Remplace par Epic 2",
                         "Bootstrap -> MainMenuLobby -> MVP_Run",
                         "VAL-028",
                         "VAL-032",
                         "Story 2.2",
                         "Story 2.5",
                         RunCheckpointHudScreen.FutureHudState
                     })
            {
                Assert.That(notes, Does.Contain(required), required);
            }
        }

        private static void AssertLocalGreyboxPrefab(string prefabPath, string stableId, bool expectNetworkObjectAbsent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab != null, Is.True, prefabPath);

            if (expectNetworkObjectAbsent)
            {
                Assert.That(prefab.GetComponent<NetworkObject>(), Is.Null, prefabPath + " ne doit pas etre networke en Epic 1");
            }
            else
            {
                // Story 3.1 (Epic 3) dote Greybox_PlayerCar d'un NetworkObject (frontiere du
                // module Vehicules) ; le checkpoint Epic 1 reste valide pour les autres seeds.
                Assert.That(prefab.GetComponent<NetworkObject>(), Is.Not.Null,
                    prefabPath + " doit porter un NetworkObject depuis la Story 3.1");
            }

            var metadata = prefab.GetComponent<GreyboxAssetSeedMetadata>();
            Assert.That(metadata != null, Is.True, prefabPath + " doit porter sa metadata");
            Assert.That(metadata.StableId, Is.EqualTo(stableId));
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            return scene.GetRootGameObjects().FirstOrDefault(root => root.name == name);
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
        /// Story 5.10 : la boucle greybox de la Story 3.4 est remplacee par le district route, dont
        /// la topologie et les noms d'instances sont libres d'etre remanies. Ce que cette garde
        /// heritee exige n'a pas change -- une chaussee greybox praticable existe a l'entree du run --
        /// donc elle se verifie sur une PROPRIETE : un module de route est pose sous la racine du
        /// graphe de voies, et ce graphe porte des voies parcourables. Aucun nom d'instance ici : un
        /// troisieme remaniement du district ne doit plus casser ce test.
        /// </summary>
        private static void AssertDistrictCarriesADrivableGreyboxRoad(Transform runRoot)
        {
            const string roadSegmentPrefabPath = "Assets/RoadRage/Prefabs/Greybox_RoadSegment_TwoWay.prefab";

            var laneGraph = runRoot.GetComponentInChildren<LaneGraph>(true);
            Assert.That(laneGraph, Is.Not.Null, "district route greybox attendu sous RunRoot");
            Assert.That(laneGraph.NodeCount, Is.GreaterThan(0), "la chaussee du district doit porter des voies parcourables");

            var roadModules = 0;
            foreach (Transform module in laneGraph.transform)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromSource(module.gameObject);
                if (source != null && AssetDatabase.GetAssetPath(source) == roadSegmentPrefabPath)
                {
                    roadModules++;
                }
            }

            Assert.That(roadModules, Is.GreaterThan(0),
                "au moins un module de route greybox (" + roadSegmentPrefabPath + ") doit etre pose dans le district, quel que soit le nom de l'instance");
        }

        private static Transform FindChild(Transform root, string path)
        {
            return root.Find(path);
        }

        private static void AssertSerializedObjectReference(Component component, string fieldName)
        {
            var value = GetPrivateField(component, fieldName) as UnityEngine.Object;
            Assert.That(value != null, Is.True, component.GetType().Name + "." + fieldName + " doit etre cable");
        }

        private static void AssertSerializedTextContains(Component component, string fieldName, string expectedText)
        {
            var label = GetPrivateField(component, fieldName) as TMP_Text;
            Assert.That(label != null, Is.True, component.GetType().Name + "." + fieldName + " doit referencer un TMP_Text");
            Assert.That(label.text, Does.Contain(expectedText), fieldName);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }
    }
}
