using System;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 3.2 : verrouille la revendication de conducteur host-arbitree, le cablage du prefab
    /// voiture (Rigidbody host-simule, NetworkTransform serveur-autoritaire, controleur, rig camera
    /// local), la camera vehicule inactive par defaut, et la frontiere du module Vehicules apres
    /// l'ajout des references Cinemachine/Input System.
    /// </summary>
    public sealed class Story32DriverControlAndLocalCameraTests
    {
        private const string PrefabPath = "Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab";
        private const string AsmdefPath = "Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef";
        private const string VehicleStateSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string DevVehicleSandboxScenePath = "Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity";
        private const string CinemachineBrainScriptGuid = "72ece51f2901e7445ab60da3685d6b5f";

        [Test]
        public void NetworkedVehicleStateDeclaresDriverClaimSentinelAndHelper()
        {
            var source = File.ReadAllText(VehicleStateSourcePath);

            Assert.That(source, Does.Contain("NetworkVariable<ulong> DriverClientId"));
            Assert.That(source, Does.Contain("UnclaimedDriverClientId = ulong.MaxValue"));
            Assert.That(source, Does.Contain("public bool IsDriver(ulong clientId)"));
        }

        [Test]
        public void GreyboxPlayerCarHasHostSimulatedRigidbodyAndServerAuthoritativeNetworkTransform()
        {
            var prefab = LoadPrefab();

            var rigidbody = prefab.GetComponent<Rigidbody>();
            Assert.That(rigidbody, Is.Not.Null, "Greybox_PlayerCar doit porter un Rigidbody pour la conduite host-simulee.");

            var networkTransform = prefab.GetComponent<NetworkTransform>();
            Assert.That(networkTransform, Is.Not.Null,
                "Greybox_PlayerCar doit porter un NetworkTransform pour repliquer la pose du Rigidbody (autorite serveur par defaut).");
        }

        [Test]
        public void GreyboxPlayerCarHasDriverControllerAlongsideStateAndRigidbody()
        {
            var prefab = LoadPrefab();

            var controller = prefab.GetComponent<NetworkedVehicleDriverController>();
            Assert.That(controller, Is.Not.Null, "Greybox_PlayerCar doit porter le controleur de conduite.");
            Assert.That(prefab.GetComponent<NetworkedVehicleState>(), Is.Not.Null,
                "NetworkedVehicleDriverController requiert NetworkedVehicleState sur le meme objet.");
            Assert.That(prefab.GetComponent<Rigidbody>(), Is.Not.Null,
                "NetworkedVehicleDriverController requiert un Rigidbody sur le meme objet.");
        }

        [Test]
        public void GreyboxPlayerCarGameplayForwardMatchesVisualFrontAndLongitudinalCollider()
        {
            var prefab = LoadPrefab();
            var instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var boxCollider = instance.GetComponent<BoxCollider>();
                Assert.That(boxCollider, Is.Not.Null, "Le collider racine doit rester un BoxCollider.");
                Assert.That(boxCollider.size.z, Is.GreaterThan(boxCollider.size.x),
                    "La longueur gameplay de la voiture doit etre sur Z pour que transform.forward pointe vers l'avant.");

                var frontBumper = FindChild(instance.transform, "Greybox_PlayerCar_FrontBumper");
                Assert.That(frontBumper, Is.Not.Null, "Le bumper avant du visuel doit rester lisible.");

                var localFrontBumper = instance.transform.InverseTransformPoint(frontBumper.position);
                Assert.That(localFrontBumper.z, Is.GreaterThan(0f),
                    "Le nez visuel doit pointer vers +Z, pas vers l'axe lateral +X.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void ArcadeSteeringDirectionInvertsWhenVehicleIsReversing()
        {
            Assert.That(NetworkedVehicleDriverController.ResolveSteerDirectionMultiplier(5f, 0f), Is.EqualTo(1f));
            Assert.That(NetworkedVehicleDriverController.ResolveSteerDirectionMultiplier(0f, 0f), Is.EqualTo(1f));
            Assert.That(NetworkedVehicleDriverController.ResolveSteerDirectionMultiplier(0f, 1f), Is.EqualTo(-1f));
            Assert.That(NetworkedVehicleDriverController.ResolveSteerDirectionMultiplier(-2f, 1f), Is.EqualTo(-1f));
        }

        [Test]
        public void GreyboxPlayerCarHasCameraRigWithInactiveLocalCinemachineCameraTargetingRootAndUnclaimedDriverByDefault()
        {
            var prefab = LoadPrefab();
            var instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var cameraRig = instance.GetComponent<LocalVehicleCameraRig>();
                Assert.That(cameraRig, Is.Not.Null, "Greybox_PlayerCar doit porter le rig camera local.");
                Assert.That(typeof(NetworkBehaviour).IsAssignableFrom(cameraRig.GetType()), Is.False,
                    "LocalVehicleCameraRig ne doit jamais etre un NetworkBehaviour : aucune donnee de camera n'est repliquee.");

                var vehicleCamera = instance.GetComponentInChildren<CinemachineCamera>(true);
                Assert.That(vehicleCamera, Is.Not.Null, "Greybox_PlayerCar doit porter une CinemachineCamera enfant.");
                Assert.That(vehicleCamera.GetComponent<NetworkBehaviour>(), Is.Null,
                    "la CinemachineCamera ne doit porter aucun NetworkBehaviour.");
                Assert.That(vehicleCamera.gameObject.activeSelf, Is.False,
                    "la camera vehicule doit rester inactive par defaut (activee seulement pour le conducteur local).");

                Assert.That(vehicleCamera.Target.TrackingTarget, Is.EqualTo(instance.transform), "Follow doit cibler la racine du vehicule.");
                Assert.That(vehicleCamera.Target.LookAtTarget, Is.EqualTo(instance.transform), "LookAt doit cibler la racine du vehicule.");

                var vehicleState = instance.GetComponent<NetworkedVehicleState>();
                Assert.That(vehicleState.DriverClientId.Value, Is.EqualTo(NetworkedVehicleState.UnclaimedDriverClientId),
                    "DriverClientId doit demarrer non revendique (sentinel).");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void MvpRunAndDevVehicleSandboxMainCamerasHaveCinemachineBrain()
        {
            Assert.That(File.Exists(MvpRunScenePath), Is.True, MvpRunScenePath);
            Assert.That(File.Exists(DevVehicleSandboxScenePath), Is.True, DevVehicleSandboxScenePath);

            var mvpRunText = File.ReadAllText(MvpRunScenePath);
            Assert.That(mvpRunText, Does.Contain(CinemachineBrainScriptGuid),
                MvpRunScenePath + " doit ajouter un CinemachineBrain sur la Main Camera.");

            var devVehicleSandboxText = File.ReadAllText(DevVehicleSandboxScenePath);
            Assert.That(devVehicleSandboxText, Does.Contain(CinemachineBrainScriptGuid),
                DevVehicleSandboxScenePath + " doit ajouter un CinemachineBrain sur la Main Camera.");
        }

        [Test]
        public void DevVehicleSandboxHasAGroundSurfaceForDriveTesting()
        {
            Assert.That(File.Exists(DevVehicleSandboxScenePath), Is.True, DevVehicleSandboxScenePath);

            var devVehicleSandboxText = File.ReadAllText(DevVehicleSandboxScenePath);
            Assert.That(devVehicleSandboxText, Does.Contain("Greybox_GroundPlane"),
                DevVehicleSandboxScenePath + " doit ajouter un sol pour permettre le test de conduite.");
        }

        [Test]
        public void VehiclesAssemblyReferencesInputAndCinemachineButNoOtherFeatureSlice()
        {
            Assert.That(File.Exists(AsmdefPath), Is.True, AsmdefPath);

            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(AsmdefPath));
            Assert.That(definition.references, Does.Contain("Unity.InputSystem"));
            Assert.That(definition.references, Does.Contain("Unity.Cinemachine"));

            foreach (var reference in definition.references)
            {
                Assert.That(reference.StartsWith("RoadRage.Features.", StringComparison.Ordinal), Is.False,
                    "RoadRage.Features.Vehicles ne doit referencer aucune autre feature slice : " + reference);
            }
        }

        private static GameObject LoadPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab != null, Is.True, "prefab introuvable : " + PrefabPath);
            return prefab;
        }

        private static Transform FindChild(Transform root, string childName)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
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
