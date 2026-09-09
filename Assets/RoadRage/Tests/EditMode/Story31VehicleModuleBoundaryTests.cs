using System;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 3.1 : verrouille l'identite reseau et la frontiere du module Vehicules pour la
    /// voiture partagee, sans promouvoir de logique de conduite, de sieges ou de degats.
    /// </summary>
    public sealed class Story31VehicleModuleBoundaryTests
    {
        private const string PrefabPath = "Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab";
        private const string AsmdefPath = "Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string DevVehicleSandboxScenePath = "Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity";

        private static readonly Vector3 ExpectedColliderSize = new Vector3(2.0600002f, 1.4200001f, 4.4399996f);

        [Test]
        public void GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt()
        {
            var prefab = LoadPrefab(PrefabPath);

            var networkObject = prefab.GetComponent<NetworkObject>();
            Assert.That(networkObject, Is.Not.Null, "Greybox_PlayerCar doit porter un NetworkObject");

            var vehicleState = prefab.GetComponent<NetworkedVehicleState>();
            Assert.That(vehicleState, Is.Not.Null, "Greybox_PlayerCar doit porter un NetworkedVehicleState");

            var colliders = prefab.GetComponentsInChildren<Collider>(true);
            Assert.That(colliders.Length, Is.EqualTo(1), "Greybox_PlayerCar doit garder exactement un collider");

            var boxCollider = prefab.GetComponent<BoxCollider>();
            Assert.That(boxCollider, Is.Not.Null, "Le collider racine doit rester un BoxCollider");
            Assert.That(boxCollider.size.x, Is.EqualTo(ExpectedColliderSize.x).Within(0.001f));
            Assert.That(boxCollider.size.y, Is.EqualTo(ExpectedColliderSize.y).Within(0.001f));
            Assert.That(boxCollider.size.z, Is.EqualTo(ExpectedColliderSize.z).Within(0.001f));
            Assert.That(boxCollider.size.z, Is.GreaterThan(boxCollider.size.x),
                "L'axe long du collider doit suivre le forward gameplay (+Z), pas la tranche du vehicule.");

            Assert.That(prefab.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0),
                "L'art placeholder (Visual_Greybox_PlayerCar) doit rester present");
        }

        [Test]
        public void GreyboxPlayerCarPrefabIsPlacedInMvpRunAndDevVehicleSandboxScenes()
        {
            var prefabGuid = AssetDatabase.AssetPathToGUID(PrefabPath);
            Assert.That(prefabGuid, Is.Not.Empty, PrefabPath);

            Assert.That(File.Exists(MvpRunScenePath), Is.True, MvpRunScenePath);
            Assert.That(File.Exists(DevVehicleSandboxScenePath), Is.True, DevVehicleSandboxScenePath);

            var mvpRunText = File.ReadAllText(MvpRunScenePath);
            Assert.That(mvpRunText, Does.Contain(prefabGuid), MvpRunScenePath + " doit contenir une instance de Greybox_PlayerCar");

            var devVehicleSandboxText = File.ReadAllText(DevVehicleSandboxScenePath);
            Assert.That(devVehicleSandboxText, Does.Contain(prefabGuid), DevVehicleSandboxScenePath + " doit contenir une instance de Greybox_PlayerCar");
        }

        [Test]
        public void VehiclesAssemblyDoesNotReferenceOtherFeatureSlices()
        {
            Assert.That(File.Exists(AsmdefPath), Is.True, AsmdefPath);

            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(AsmdefPath));
            Assert.That(definition.name, Is.EqualTo("RoadRage.Features.Vehicles"));
            Assert.That(definition.rootNamespace, Is.EqualTo("RoadRage.Features.Vehicles"));

            Assert.That(definition.references, Is.Not.Null);
            Assert.That(definition.references, Does.Not.Contain("RoadRage.Features.Lobby"));
            Assert.That(definition.references, Does.Not.Contain("RoadRage.Features.Economy"));
            Assert.That(definition.references, Does.Not.Contain("RoadRage.Features.Boss"));
            Assert.That(definition.references, Does.Not.Contain("RoadRage.Features.PassengerActions"));

            foreach (var reference in definition.references)
            {
                Assert.That(reference.StartsWith("RoadRage.Features.", StringComparison.Ordinal), Is.False,
                    "RoadRage.Features.Vehicles ne doit referencer aucune autre feature slice : " + reference);
            }
        }

        private static GameObject LoadPrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab != null, Is.True, "prefab introuvable : " + prefabPath);
            return prefab;
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
