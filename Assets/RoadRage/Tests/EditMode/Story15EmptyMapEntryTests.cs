using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.App.Lobby;
using RoadRage.App.Run;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.Run;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    public sealed class Story15EmptyMapEntryTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

        [Test]
        public void AppSceneRouterExposesMvpRunAsPrimaryBuildScene()
        {
            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            CollectionAssert.Contains(enabledScenes, MvpRunScenePath);
        }

        [Test]
        public void LobbyStartGameRequiresAConfirmedProfileBeforeLoadingRun()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Lobby/LobbyFlowController.cs");

            Assert.That(source, Does.Contain("Profiles.HasProfile"));
            Assert.That(LobbyFlowController.MissingProfileStartGameMessage, Does.Contain("profil joueur"));
        }

        /// <summary>
        /// Story 5.3 (AD-26) : Start Game sans lobby ouvert suit desormais exactement le meme chemin
        /// que Create Lobby (creation du lobby prive puis StartNetworkedRun) -- plus de chargement de
        /// scene local direct via AppSceneRouter.LoadMvpRun.
        /// </summary>
        [Test]
        public void LobbyStartGameWithoutAnOpenRoomCreatesAHostLobbyInsteadOfLoadingTheSceneDirectly()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Lobby/LobbyFlowController.cs");

            Assert.That(source, Does.Not.Contain("bootstrap.Router.LoadMvpRun()"),
                "Start Game ne doit plus contourner la creation de lobby et le demarrage reseau.");
            Assert.That(source, Does.Contain("await lobbyRoom.CreateRoomAsync();"));
            Assert.That(source, Does.Contain("StartNetworkedRun(true);"));
        }

        [Test]
        public void MvpRunSceneContainsWiredRunEntryAndGreyboxMap()
        {
            var scene = EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
            try
            {
                var root = FindRoot(scene, "RunRoot");
                Assert.That(root, Is.Not.Null, "RunRoot attendu dans MVP_Run");

                var composition = root.GetComponent<RunCompositionRoot>();
                Assert.That(composition, Is.Not.Null, "RunCompositionRoot attendu sur RunRoot");
                Assert.That(composition.RuntimeRoot, Is.Not.Null, "RuntimeRoot doit etre cable");
                Assert.That(composition.SpawnRoot, Is.Not.Null, "SpawnRoot doit etre cable");

                var flow = root.GetComponent<RunFlowController>();
                Assert.That(flow, Is.Not.Null, "RunFlowController attendu sur RunRoot");
                AssertSerializedReference(flow, "compositionRoot");
                AssertSerializedReference(flow, "characterCatalog");
                AssertSerializedReference(flow, "playerCamera");
                AssertSerializedReference(flow, "playerSpawnPoint");

                Assert.That(FindChild(root.transform, "GreyboxMap/Greybox_GroundPlane"), Is.Not.Null, "plan de sol greybox attendu");
                AssertDistrictCarriesADrivableGreyboxRoad(root.transform);
                Assert.That(FindChild(root.transform, "GreyboxMap/Greybox_CityBlock_A_West"), Is.Not.Null, "bloc urbain gauche attendu");
                Assert.That(FindChild(root.transform, "GreyboxMap/Greybox_PlayerCar_Parked"), Is.Not.Null, "voiture partagee visible attendue");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
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

        [Test]
        public void RunEntryUsesCharacterCatalogWithoutRegisteringNetworkPrefabs()
        {
            var defaultNetworkPrefabs = File.ReadAllText("Assets/DefaultNetworkPrefabs.asset");
            var runFlowSource = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");

            Assert.That(defaultNetworkPrefabs, Does.Not.Contain("Greybox_Character_Rookie"));
            Assert.That(runFlowSource, Does.Not.Contain(".Spawn("));
            Assert.That(runFlowSource, Does.Not.Contain("StartHost("));
            Assert.That(runFlowSource, Does.Not.Contain("StartClient("));
        }

        [Test]
        public void OnFootIntentClampsMoveAndKeepsSprintFlag()
        {
            var intent = new OnFootMovementIntent(new Vector2(2f, 2f), new Vector2(3f, -1f), true);

            Assert.That(intent.Move.magnitude, Is.LessThanOrEqualTo(1.0001f));
            Assert.That(intent.Look, Is.EqualTo(new Vector2(3f, -1f)));
            Assert.That(intent.SprintRequested, Is.True);
            Assert.That(OnFootMovementIntent.Idle.IsIdle, Is.True);
        }

        [Test]
        public void OnFootFeatureKeepsItsAsmdefBoundary()
        {
            var source = File.ReadAllText("Assets/RoadRage/Features/OnFoot/RoadRage.Features.OnFoot.asmdef");
            var definition = JsonUtility.FromJson<AssemblyDefinition>(source);

            foreach (var reference in definition.references ?? Array.Empty<string>())
            {
                Assert.That(reference.StartsWith("RoadRage.Features.", StringComparison.Ordinal), Is.False, reference);
                Assert.That(reference, Is.Not.EqualTo("RoadRage.App"));
            }
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            return scene.GetRootGameObjects().FirstOrDefault(root => root.name == name);
        }

        private static Transform FindChild(Transform root, string path)
        {
            return root.Find(path);
        }

        private static void AssertSerializedReference(UnityEngine.Object target, string fieldName)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(fieldName);

            Assert.That(property, Is.Not.Null, fieldName);
            Assert.That(property.objectReferenceValue != null, Is.True, fieldName + " doit etre cable");
        }

        [Serializable]
        private sealed class AssemblyDefinition
        {
            public string[] references;
        }
    }
}
