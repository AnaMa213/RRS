using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 3.6 : checkpoint jouable de l'Epic 3. Compose les invariants deja verrouilles par
    /// 3.1-3.5 (wiring de scene, sieges, degats, HUD, recuperation) en un seul gate d'integration,
    /// sans reintroduire de logique gameplay -- meme role que Story28Epic2OnlinePlayableCheckpointTests
    /// pour l'Epic 2.
    /// </summary>
    public sealed class Story36Epic3DrivingPlayableCheckpointTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string PlayerCarPrefabPath = "Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab";
        private const string PlayerRootPrefabPath = "Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab";
        private const string VehicleAsmdefPath = "Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef";
        private const string HandoffNotesPath = "docs/setup/story-3-6-epic-3-driving-playable-checkpoint-notes.md";

        [Test]
        public void MvpRunSceneWiresTheFullDrivingCheckpointChain()
        {
            var scene = EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
            try
            {
                var root = FindRoot(scene, "RunRoot");
                Assert.That(root, Is.Not.Null, "RunRoot attendu dans MVP_Run");

                var flow = root.GetComponent<RunFlowController>();
                Assert.That(flow, Is.Not.Null, "RunFlowController attendu sur RunRoot");
                AssertSerializedObjectReference(flow, "compositionRoot");
                AssertSerializedObjectReference(flow, "characterCatalog");
                AssertSerializedObjectReference(flow, "playerCamera");
                AssertSerializedObjectReference(flow, "playerSpawnPoint");
                AssertSerializedObjectReference(flow, "checkpointHud");

                var hud = FindComponentInScene<RunCheckpointHudScreen>(scene);
                Assert.That(hud, Is.Not.Null, "HUD de checkpoint attendu dans MVP_Run");

                var prefabGuid = AssetDatabase.AssetPathToGUID(PlayerCarPrefabPath);
                Assert.That(prefabGuid, Is.Not.Empty, PlayerCarPrefabPath);
                var sceneText = File.ReadAllText(MvpRunScenePath);
                Assert.That(sceneText, Does.Contain(prefabGuid),
                    "MVP_Run doit contenir une instance du vehicule partage pour le parcours golden-path.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void PlayerAndVehiclePrefabsCarryTheFullEpic3ComponentSetTogether()
        {
            var playerRoot = LoadPrefab(PlayerRootPrefabPath);
            Assert.That(playerRoot.GetComponent<NetworkedVehicleSeatIntent>(), Is.Not.Null,
                "NetworkedPlayerRoot doit porter l'intent de siege (Story 3.3)");
            Assert.That(playerRoot.GetComponent<NetworkedVehicleRecoveryIntent>(), Is.Not.Null,
                "NetworkedPlayerRoot doit porter l'intent de recuperation (Story 3.4)");
            Assert.That(playerRoot.GetComponent<NetworkedPlayerReviveIntent>(), Is.Not.Null,
                "NetworkedPlayerRoot doit porter l'intent de resurrection (Story 3.5)");

            var vehicle = LoadPrefab(PlayerCarPrefabPath);
            Assert.That(vehicle.GetComponent<NetworkObject>(), Is.Not.Null,
                "Greybox_PlayerCar doit garder son identite reseau (Story 3.1)");
            Assert.That(vehicle.GetComponent<NetworkedVehicleState>(), Is.Not.Null,
                "Greybox_PlayerCar doit garder son etat partage (Story 3.1/3.3/3.5)");
            Assert.That(vehicle.GetComponent<NetworkedVehicleDriverController>(), Is.Not.Null,
                "Greybox_PlayerCar doit garder son controleur de conduite/collision/recuperation (Story 3.2/3.4)");
            Assert.That(vehicle.GetComponentInChildren<LocalVehicleCameraRig>(true), Is.Not.Null,
                "Greybox_PlayerCar doit garder son rig camera local (Story 3.2)");
            Assert.That(vehicle.GetComponentInChildren<NetworkedVehicleDamageVfxController>(true), Is.Not.Null,
                "Greybox_PlayerCar doit garder son retour visuel de degats (Story 3.5)");
        }

        [Test]
        public void VehiclesAssemblyStillOwnsOnlyVehicleConcernsAfterEpic3()
        {
            Assert.That(File.Exists(VehicleAsmdefPath), Is.True, VehicleAsmdefPath);

            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(VehicleAsmdefPath));
            Assert.That(definition.references, Is.Not.Null);

            foreach (var reference in definition.references)
            {
                Assert.That(reference.StartsWith("RoadRage.Features.", StringComparison.Ordinal), Is.False,
                    "RoadRage.Features.Vehicles ne doit toujours referencer aucune autre feature slice apres l'Epic 3 : " + reference);
            }
        }

        [Test]
        public void ChecklistNotesDocumentGoldenPathGreyboxAndEpic4Handoff()
        {
            Assert.That(File.Exists(HandoffNotesPath), Is.True, HandoffNotesPath);
            var notes = File.ReadAllText(HandoffNotesPath);

            foreach (var required in new[]
                     {
                         "## Preuves executees",
                         "## Greybox et placeholders",
                         "## Reste pour l'Epic 4",
                         "MVP_Run",
                     })
            {
                Assert.That(notes, Does.Contain(required), required);
            }
        }

        private static GameObject LoadPrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab != null, Is.True, "prefab introuvable : " + prefabPath);
            return prefab;
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

        private static void AssertSerializedObjectReference(Component component, string fieldName)
        {
            var value = GetPrivateField(component, fieldName) as UnityEngine.Object;
            Assert.That(value != null, Is.True, component.GetType().Name + "." + fieldName + " doit etre cable");
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }

        [Serializable]
        private sealed class AssemblyDefinition
        {
            public string name;
            public string rootNamespace;
            public string[] references;
        }
    }
}
