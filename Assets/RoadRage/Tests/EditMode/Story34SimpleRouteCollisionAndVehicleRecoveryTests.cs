using System;
using System.IO;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 3.4 : verification ciblee de la detection retournement/hors-zone, de la recuperation
    /// vehicule (auto et manuelle), du relais collision/recuperation vers le HUD, de la frontiere
    /// asmdef du module Vehicules, et des marqueurs de scene (boucle/limites/props/recuperation)
    /// -- seul MVP_Run recoit le contenu de carte de cette story, Dev_VehicleSandbox reste inchange.
    /// </summary>
    public sealed class Story34SimpleRouteCollisionAndVehicleRecoveryTests
    {
        private const string VehicleAsmdefPath = "Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef";
        private const string DriverControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string RecoveryIntentSourcePath = "Assets/RoadRage/App/Run/NetworkedVehicleRecoveryIntent.cs";
        private const string RunFlowSourcePath = "Assets/RoadRage/App/Run/RunFlowController.cs";
        private const string HudSourcePath = "Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs";
        private const string PlayerRootPrefabPath = "Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string DevVehicleSandboxScenePath = "Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity";
        private const string CityBlockPrefabPath = "Assets/RoadRage/Prefabs/Greybox_CityBlock_A.prefab";
        private const string BarrelPrefabPath = "Assets/RoadRage/Prefabs/Prop_Barrel.prefab";

        [Test]
        public void VoidHeightThresholdPredicateMatchesLifecycleAndLocalRespawnPattern()
        {
            Assert.That(NetworkedVehicleDriverController.IsBelowVoidHeightThreshold(-10.1f, -10f), Is.True);
            Assert.That(NetworkedVehicleDriverController.IsBelowVoidHeightThreshold(-10f, -10f), Is.False);
            Assert.That(NetworkedVehicleDriverController.IsBelowVoidHeightThreshold(0f, -10f), Is.False);
        }

        [Test]
        public void RolloverPredicateFlagsOnlyWhenUpAxisStronglyLeavesVertical()
        {
            Assert.That(NetworkedVehicleDriverController.IsRolledOver(Vector3.up, 0.35f), Is.False,
                "Vehicule parfaitement droit ne doit jamais etre considere retourne.");
            Assert.That(NetworkedVehicleDriverController.IsRolledOver(Vector3.down, 0.35f), Is.True,
                "Vehicule totalement retourne doit etre detecte.");
            Assert.That(NetworkedVehicleDriverController.IsRolledOver(Vector3.right, 0.35f), Is.True,
                "Vehicule sur le flanc (axe haut horizontal) doit etre detecte comme retourne.");
        }

        [Test]
        public void DriverControllerExposesHostOnlyRecoveryAndCollisionContract()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            // Story 3.5 : VehicleCollided porte desormais la vitesse d'impact (pont de degats vehicule/joueur).
            Assert.That(source, Does.Contain("public event Action<float> VehicleCollided;"));
            Assert.That(source, Does.Contain("public event Action VehicleRecovered;"));
            Assert.That(source, Does.Contain("public void RecoverVehicle(Vector3 position, Quaternion rotation)"));
            Assert.That(source, Does.Contain("public void RecoverAtRecoveryPoint()"));
            Assert.That(source, Does.Contain("private void OnCollisionEnter(Collision collision)"));
            Assert.That(source, Does.Contain("private void UpdateRecoveryDetection(float fixedDeltaTime)"));

            // Story 5.3 : le solo est toujours host-authoritative, la garde d'autorite se reduit a IsServer.
            Assert.That(Occurrences(source, "if (!IsServer || body == null"), Is.GreaterThanOrEqualTo(2),
                "RecoverVehicle et FixedUpdate doivent partager la meme garde d'autorite host.");
            Assert.That(source, Does.Contain("if (!IsServer)"),
                "OnCollisionEnter doit aussi refuser le chemin client reseau.");
            Assert.That(source, Does.Not.Contain("localSoloDriverActive"),
                "Story 5.3 : plus de bascule locale sans session Netcode, le solo est toujours host.");

            // Repli sur la position initiale (Dev_VehicleSandbox sans repere explicite) plutot qu'un no-op silencieux.
            Assert.That(source, Does.Contain("CaptureFallbackRecoveryPose"));
            Assert.That(source, Does.Contain("fallbackRecoveryCaptured"));

            // Revue Story 3.4 : snap NetworkTransform (pas d'interpolation cote client lors d'une recuperation).
            Assert.That(source, Does.Contain("networkTransform.Teleport(position, rotation, transform.localScale);"));

            // Revue Story 3.4 : le retour HUD collision/recuperation doit atteindre chaque client, pas seulement le host.
            Assert.That(source, Does.Contain("[Rpc(SendTo.NotServer)]"));
            Assert.That(Occurrences(source, "[Rpc(SendTo.NotServer)]"), Is.EqualTo(2),
                "Collision et recuperation doivent chacune notifier les clients.");
            Assert.That(source, Does.Contain("NotifyClientsIfNetworked(NotifyVehicleCollidedRpc, impactSpeed);"));
            Assert.That(source, Does.Contain("NotifyClientsIfNetworked(NotifyVehicleRecoveredRpc);"));
        }

        [Test]
        public void RecoveryIntentMirrorsSeatIntentClientToHostRpcPattern()
        {
            var source = File.ReadAllText(RecoveryIntentSourcePath);

            Assert.That(source, Does.Contain("[RequireComponent(typeof(NetworkedPlayerState))]"));
            Assert.That(source, Does.Contain("[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]"));
            Assert.That(source, Does.Contain("state.ClientId.Value != rpcParams.Receive.SenderClientId"),
                "La RPC doit rester validee contre l'emetteur, comme NetworkedVehicleSeatIntent.");
            Assert.That(source, Does.Contain("vehicleState.DriverClientId.Value != clientId"),
                "Seul le conducteur actuel du siege 0 peut declencher la recuperation manuelle.");
            Assert.That(source, Does.Contain("driverController.RecoverAtRecoveryPoint()"));
        }

        /// <summary>
        /// Story 5.3 (AD-26) : la touche R de recuperation manuelle n'est plus geree par RunFlowController
        /// (HandleLocalSoloVehicleRecoveryInteraction, supprimee) mais entierement par
        /// NetworkedVehicleRecoveryIntent (client -> host RPC, deja verifie par
        /// RecoveryIntentMirrorsSeatIntentClientToHostRpcPattern), exercee identiquement en solo (host)
        /// et en ligne. RunFlowController ne fait plus que relayer collision/recuperation vers le HUD.
        /// </summary>
        [Test]
        public void RunFlowBridgesNetworkVehicleEventsToHudWithoutAnyLocalSoloRecoveryPath()
        {
            var source = File.ReadAllText(RunFlowSourcePath);

            Assert.That(source, Does.Not.Contain("HandleLocalSoloVehicleRecoveryInteraction"),
                "Story 5.3 : la recuperation manuelle passe entierement par NetworkedVehicleRecoveryIntent.");
            Assert.That(source, Does.Not.Contain("LocalSolo"));

            Assert.That(source, Does.Contain("EnsureVehicleEventBridge"));
            Assert.That(source, Does.Contain("driverController.VehicleCollided += HandleVehicleCollided;"));
            Assert.That(source, Does.Contain("driverController.VehicleRecovered += HandleVehicleRecovered;"));
            Assert.That(source, Does.Contain("UnsubscribeFromVehicleEvents"));
            Assert.That(source, Does.Contain("checkpointHud.ShowVehicleCollisionMessage();"));
            Assert.That(source, Does.Contain("checkpointHud.ShowVehicleRecoveredMessage();"));
        }

        [Test]
        public void HudExposesMinimalCollisionAndRecoveryFeedbackWithoutNewDurableUi()
        {
            var source = File.ReadAllText(HudSourcePath);

            Assert.That(source, Does.Contain("public void ShowVehicleCollisionMessage()"));
            Assert.That(source, Does.Contain("public void ShowVehicleRecoveredMessage()"));
            Assert.That(source, Does.Contain("ShowVehicleSeatMessage(VehicleCollisionMessage)"),
                "Le retour collision doit reutiliser le label existant (pas de nouvelle UI durable).");
            Assert.That(source, Does.Contain("ShowVehicleSeatMessage(VehicleRecoveredMessage)"));
        }

        [Test]
        public void VehiclesAssemblyStillReferencesNoOtherFeatureSlice()
        {
            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(VehicleAsmdefPath));
            Assert.That(definition.name, Is.EqualTo("RoadRage.Features.Vehicles"));
            Assert.That(definition.references, Is.Not.Null);

            foreach (var reference in definition.references)
            {
                Assert.That(reference.StartsWith("RoadRage.Features.", StringComparison.Ordinal), Is.False,
                    "RoadRage.Features.Vehicles ne doit referencer aucune autre feature slice : " + reference);
            }
        }

        [Test]
        public void NetworkedPlayerRootCarriesRecoveryIntentNetworkBehaviour()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerRootPrefabPath);
            Assert.That(prefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable.");

            var intent = prefab.GetComponent<NetworkedVehicleRecoveryIntent>();
            Assert.That(intent, Is.Not.Null, "NetworkedPlayerRoot doit porter l'intent de recuperation vehicule.");
        }

        [Test]
        public void MvpRunSceneContainsEnlargedLoopBoundariesDecorAndRecoveryMarker()
        {
            Assert.That(File.Exists(MvpRunScenePath), Is.True, MvpRunScenePath);
            var sceneText = File.ReadAllText(MvpRunScenePath);

            Assert.That(sceneText, Does.Contain("m_LocalScale: {x: 120, y: 0.1, z: 120}"),
                "Le sol greybox doit etre nettement agrandi (~120x120).");

            Assert.That(sceneText, Does.Contain("Greybox_RoadLoop_North"));
            Assert.That(sceneText, Does.Contain("Greybox_RoadLoop_South"));
            Assert.That(sceneText, Does.Contain("Greybox_RoadLoop_East"));
            Assert.That(sceneText, Does.Contain("Greybox_RoadLoop_West"));

            Assert.That(sceneText, Does.Contain("Greybox_MapBoundary_North"));
            Assert.That(sceneText, Does.Contain("Greybox_MapBoundary_South"));
            Assert.That(sceneText, Does.Contain("Greybox_MapBoundary_East"));
            Assert.That(sceneText, Does.Contain("Greybox_MapBoundary_West"));

            Assert.That(sceneText, Does.Contain("VehicleRecoveryPoint"));
            Assert.That(sceneText, Does.Contain("recoveryPoint"),
                "Le vehicule partage doit avoir son champ recoveryPoint assigne via override de scene.");

            var cityBlockGuid = AssetDatabase.AssetPathToGUID(CityBlockPrefabPath);
            Assert.That(cityBlockGuid, Is.Not.Empty, CityBlockPrefabPath);
            Assert.That(sceneText, Does.Contain(cityBlockGuid), "Des batiments Greybox_CityBlock_A doivent decorer la boucle.");

            var barrelGuid = AssetDatabase.AssetPathToGUID(BarrelPrefabPath);
            Assert.That(barrelGuid, Is.Not.Empty, BarrelPrefabPath);
            Assert.That(sceneText, Does.Contain(barrelGuid), "Des props Prop_Barrel doivent decorer la boucle.");
        }

        [Test]
        public void DevVehicleSandboxSceneStaysUntouchedByMapContentOfThisStory()
        {
            Assert.That(File.Exists(DevVehicleSandboxScenePath), Is.True, DevVehicleSandboxScenePath);
            var sceneText = File.ReadAllText(DevVehicleSandboxScenePath);

            Assert.That(sceneText, Does.Not.Contain("VehicleRecoveryPoint"),
                "Dev_VehicleSandbox ne doit recevoir aucun repere de recuperation explicite (repli sur la position initiale).");
            Assert.That(sceneText, Does.Not.Contain("Greybox_RoadLoop_"),
                "Dev_VehicleSandbox ne doit pas recevoir la boucle routiere de cette story.");
            Assert.That(sceneText, Does.Not.Contain("Greybox_MapBoundary_"),
                "Dev_VehicleSandbox ne doit pas recevoir les limites de zone de cette story.");
        }

        private static int Occurrences(string source, string token)
        {
            var count = 0;
            var index = 0;
            while ((index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }

        [Serializable]
        private sealed class AssemblyDefinition
        {
            public string name;
            public string[] references;
        }
    }
}
