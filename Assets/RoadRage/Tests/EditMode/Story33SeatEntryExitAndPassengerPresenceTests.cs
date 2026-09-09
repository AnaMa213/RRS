using System;
using System.IO;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Players;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 3.3 : verification ciblee du contrat de sieges, du prefab joueur porteur de l'intent
    /// reseau, et des gardes qui empechent le controle a pied/passager de contredire l'etat assis.
    /// </summary>
    public sealed class Story33SeatEntryExitAndPassengerPresenceTests
    {
        private const string PlayerRootPrefabPath = "Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab";
        private const string VehicleAsmdefPath = "Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef";
        private const string DriverControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string CameraRigSourcePath = "Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs";
        private const string SeatServiceSourcePath = "Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs";
        private const string PoseReporterSourcePath = "Assets/RoadRage/App/Run/NetworkedLocalPlayerPoseReporter.cs";
        private const string RunFlowSourcePath = "Assets/RoadRage/App/Run/RunFlowController.cs";
        private const string PlayerPresentationSourcePath = "Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs";
        private const string HudSourcePath = "Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs";
        private const string DevToolsAsmdefPath = "Assets/RoadRage/DevTools/RoadRage.DevTools.asmdef";
        private const string DevHarnessSourcePath = "Assets/RoadRage/DevTools/RoadRageNetcodeSmokeTestAutoStart.cs";

        [Test]
        public void VehicleStateAssignsDriverThenPassengersAndKeepsSeatIndicesStable()
        {
            var root = new GameObject("VehicleStateHarness");
            try
            {
                root.AddComponent<NetworkObject>();
                var state = root.AddComponent<NetworkedVehicleState>();

                Assert.That(NetworkedVehicleState.NoSeatIndex, Is.EqualTo(-1));
                Assert.That(NetworkedVehicleState.DriverSeatIndex, Is.EqualTo(0));
                Assert.That(NetworkedVehicleState.FirstPassengerSeatIndex, Is.EqualTo(1));
                Assert.That(NetworkedVehicleState.SeatCount, Is.EqualTo(4));

                Assert.That(state.TryFindAvailableSeat(false, out var firstSeat), Is.True);
                Assert.That(firstSeat, Is.EqualTo(NetworkedVehicleState.DriverSeatIndex));
                Assert.That(state.TryAssignSeat(firstSeat, 11UL), Is.True);
                Assert.That(state.DriverClientId.Value, Is.EqualTo(11UL));

                Assert.That(state.TryFindAvailableSeat(false, out var secondSeat), Is.True);
                Assert.That(secondSeat, Is.EqualTo(NetworkedVehicleState.FirstPassengerSeatIndex));
                Assert.That(state.TryAssignSeat(secondSeat, 22UL), Is.True);
                Assert.That(state.FindSeatIndex(22UL), Is.EqualTo(NetworkedVehicleState.FirstPassengerSeatIndex));
                Assert.That(state.IsDriver(22UL), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void VehicleStateRejectsDuplicateOccupantsAndReleasesSeats()
        {
            var root = new GameObject("VehicleStateReleaseHarness");
            try
            {
                root.AddComponent<NetworkObject>();
                var state = root.AddComponent<NetworkedVehicleState>();

                Assert.That(state.TryAssignSeat(NetworkedVehicleState.FirstPassengerSeatIndex, 22UL), Is.True);
                Assert.That(state.TryAssignSeat(NetworkedVehicleState.FirstPassengerSeatIndex + 1, 22UL), Is.False,
                    "Un client ne doit pas occuper deux sieges.");
                Assert.That(state.ReleaseClient(22UL), Is.True);
                Assert.That(state.FindSeatIndex(22UL), Is.EqualTo(NetworkedVehicleState.NoSeatIndex));
                Assert.That(state.PassengerSeat1ClientId.Value, Is.EqualTo(NetworkedVehicleState.UnoccupiedSeatClientId));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SeatServicePureGuardsCoverEntryRefusalExitAndCleanupRules()
        {
            Assert.That(NetworkedVehicleSeatService.CanEnterSeat(PlayerMode.OnFoot, PlayerLifecycle.Alive, -1, 1f, 5f), Is.True);
            Assert.That(NetworkedVehicleSeatService.CanEnterSeat(PlayerMode.OnFoot, PlayerLifecycle.Dead, -1, 1f, 5f), Is.False);
            Assert.That(NetworkedVehicleSeatService.CanEnterSeat(PlayerMode.Driver, PlayerLifecycle.Alive, 0, 1f, 5f), Is.False);
            Assert.That(NetworkedVehicleSeatService.CanEnterSeat(PlayerMode.OnFoot, PlayerLifecycle.Alive, -1, 6f, 5f), Is.False);

            Assert.That(NetworkedVehicleSeatService.IsSeatedMode(PlayerMode.Driver), Is.True);
            Assert.That(NetworkedVehicleSeatService.IsSeatedMode(PlayerMode.Passenger), Is.True);
            Assert.That(NetworkedVehicleSeatService.ShouldReleaseOccupant(PlayerLifecycle.Dead), Is.True);
            Assert.That(NetworkedVehicleSeatService.ShouldReleaseOccupant(PlayerLifecycle.Disconnected), Is.True);
            Assert.That(NetworkedVehicleSeatService.ShouldReleaseOccupant(PlayerLifecycle.Alive), Is.False);
        }

        [Test]
        public void NetworkedPlayerRootCarriesSeatIntentNetworkBehaviour()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerRootPrefabPath);
            Assert.That(prefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable.");

            var intent = prefab.GetComponent<NetworkedVehicleSeatIntent>();
            Assert.That(intent, Is.Not.Null, "NetworkedPlayerRoot doit porter l'intent d'entree/sortie vehicule.");
            Assert.That(typeof(NetworkBehaviour).IsAssignableFrom(intent.GetType()), Is.True,
                "L'intent doit etre un NetworkBehaviour pour porter la Rpc client -> host.");
        }

        [Test]
        public void PassengerPresenceCannotBypassDriverGateOrOnFootPoseReporter()
        {
            var driverSource = File.ReadAllText(DriverControllerSourcePath);
            Assert.That(driverSource, Does.Contain("state.DriverClientId.Value != localClientId"));
            Assert.That(driverSource, Does.Contain("ClearServerDriverIfClient"));
            Assert.That(driverSource, Does.Contain("SetLocalSoloDriverActive"),
                "Le mode solo doit pouvoir activer localement le meme controleur voiture sans session Netcode.");

            var serviceSource = File.ReadAllText(SeatServiceSourcePath);
            Assert.That(serviceSource, Does.Contain("PlayerMode.Driver : PlayerMode.Passenger"));
            Assert.That(serviceSource, Does.Contain("ShouldReleaseOccupant"));

            var poseReporterSource = File.ReadAllText(PoseReporterSourcePath);
            Assert.That(poseReporterSource, Does.Contain("IsVehicleSeatMode(targetState)"),
                "La pose a pied ne doit pas ecraser la pose assise publiee par le host.");

            var presentationSource = File.ReadAllText(PlayerPresentationSourcePath);
            Assert.That(presentationSource, Does.Contain("IsVehicleSeatMode()"));
            Assert.That(presentationSource, Does.Contain("ClearVisual()"),
                "Les personnages reseau assis doivent disparaitre plutot qu'etre rendus debout dans la voiture.");
        }

        [Test]
        public void RunFlowFreezesAndRestoresLocalOnFootControllerAroundVehicleSeats()
        {
            var source = File.ReadAllText(RunFlowSourcePath);

            Assert.That(source, Does.Contain("state.Mode.OnValueChanged += HandleModeChanged"));
            Assert.That(source, Does.Contain("state.SeatIndex.OnValueChanged += HandleSeatIndexChanged"));
            Assert.That(source, Does.Contain("localOnFootController.MovementEnabled = false"));
            Assert.That(source, Does.Contain("localOnFootController.Teleport(position, rotation)"));
            Assert.That(source, Does.Contain("SetLocalPlayerBodyActive(false)"));
            Assert.That(source, Does.Contain("activeLocalPlayerVisual.SetActive(active)"));
        }

        [Test]
        public void SoloRunFlowUsesSameVehicleModuleAndRestoresOnFootCameraOnExit()
        {
            var runFlowSource = File.ReadAllText(RunFlowSourcePath);
            Assert.That(runFlowSource, Does.Contain("HandleLocalSoloVehicleInteraction"));
            Assert.That(runFlowSource, Does.Contain("TryEnterLocalSoloVehicle"));
            Assert.That(runFlowSource, Does.Contain("ExitLocalSoloVehicle"));
            Assert.That(runFlowSource, Does.Contain("localSoloVehicleDriver.SetLocalSoloDriverActive(true)"));
            Assert.That(runFlowSource, Does.Contain("localSoloVehicleCameraRig.SetLocalSoloCameraActive(true)"));
            Assert.That(runFlowSource, Does.Contain("RestoreOnFootCamera(localOnFootController)"),
                "La sortie vehicule doit rattacher explicitement la camera au rig a pied.");

            var cameraRigSource = File.ReadAllText(CameraRigSourcePath);
            Assert.That(cameraRigSource, Does.Contain("SetLocalSoloCameraActive"));
            Assert.That(cameraRigSource, Does.Contain("localSoloCameraActive"));
        }

        [Test]
        public void HudAndVehicleAssemblyKeepMinimalFeedbackAndFeatureBoundary()
        {
            var hudSource = File.ReadAllText(HudSourcePath);
            Assert.That(hudSource, Does.Contain("ShowVehicleSeatMessage"));

            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(VehicleAsmdefPath));
            foreach (var reference in definition.references)
            {
                Assert.That(reference.StartsWith("RoadRage.Features.", StringComparison.Ordinal), Is.False,
                    "RoadRage.Features.Vehicles ne doit referencer aucune autre feature slice : " + reference);
            }
        }

        [Test]
        public void DevVehicleSandboxAutoStartCreatesMinimalSeatHarness()
        {
            var source = File.ReadAllText(DevHarnessSourcePath);
            Assert.That(source, Does.Contain("VehicleSandboxSceneName"));
            Assert.That(source, Does.Contain("NetworkedPlayerSpawnService.PlayerRootResourceName"));
            Assert.That(source, Does.Contain("NetworkedVehicleSeatService"));

            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(DevToolsAsmdefPath));
            Assert.That(definition.references, Does.Contain("RoadRage.App"));
            Assert.That(definition.references, Does.Contain("RoadRage.Features.Players"));
            Assert.That(definition.references, Does.Contain("RoadRage.Features.Vehicles"));
        }

        [Serializable]
        private sealed class AssemblyDefinition
        {
            public string[] references;
        }
    }
}
