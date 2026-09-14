using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Run;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.5 : encodage/decodage du profil transporte par NetworkConfig.ConnectionData
    /// et le depot host-only ClientId -> profil, sans dependance Netcode ni Steam.
    /// </summary>
    public sealed class Story25NetworkedPlayerSpawnTests
    {
        private const string RuntimeManagerName = "RoadRageNetworkManager";

        private const string ExistingTestManagerName = "ExistingNetworkManager";

        [TearDown]
        public void TearDown()
        {
            DestroyNetworkManagers();
        }

        [Test]
        public void ConnectionPayloadRoundTripsDisplayNameAndCharacterId()
        {
            var encoded = NetworkPlayerConnectionPayload.Encode("Hote", "char_rookie");

            var decoded = NetworkPlayerConnectionPayload.TryDecode(encoded, out var displayName, out var characterId);

            Assert.That(decoded, Is.True);
            Assert.That(displayName, Is.EqualTo("Hote"));
            Assert.That(characterId, Is.EqualTo("char_rookie"));
        }

        [Test]
        public void ConnectionPayloadRoundTripsEmptyValues()
        {
            var encoded = NetworkPlayerConnectionPayload.Encode(string.Empty, string.Empty);

            var decoded = NetworkPlayerConnectionPayload.TryDecode(encoded, out var displayName, out var characterId);

            Assert.That(decoded, Is.True);
            Assert.That(displayName, Is.Empty);
            Assert.That(characterId, Is.Empty);
        }

        [Test]
        public void TryDecodeFailsOnNullOrEmptyPayload()
        {
            Assert.That(NetworkPlayerConnectionPayload.TryDecode(null, out _, out _), Is.False);
            Assert.That(NetworkPlayerConnectionPayload.TryDecode(new byte[0], out _, out _), Is.False);
        }

        [Test]
        public void TryDecodeFailsOnCorruptPayloadWithoutThrowing()
        {
            var garbage = new byte[] { 0xFF, 0x01, 0x02 };

            Assert.DoesNotThrow(() => NetworkPlayerConnectionPayload.TryDecode(garbage, out _, out _));
            Assert.That(NetworkPlayerConnectionPayload.TryDecode(garbage, out var displayName, out var characterId), Is.False);
            Assert.That(displayName, Is.Empty);
            Assert.That(characterId, Is.Empty);
        }

        [Test]
        public void RegistryReturnsRegisteredProfileForClientId()
        {
            var registry = new NetworkPlayerRegistry();

            registry.Register(7UL, new NetworkPlayerProfile("Invite", "char_rookie"));

            Assert.That(registry.TryGet(7UL, out var profile), Is.True);
            Assert.That(profile.DisplayName, Is.EqualTo("Invite"));
            Assert.That(profile.CharacterId, Is.EqualTo("char_rookie"));
        }

        [Test]
        public void RegistryTryGetFailsForUnknownClientId()
        {
            var registry = new NetworkPlayerRegistry();

            Assert.That(registry.TryGet(42UL, out _), Is.False);
        }

        [Test]
        public void RegistryUnregisterRemovesProfile()
        {
            var registry = new NetworkPlayerRegistry();
            registry.Register(1UL, new NetworkPlayerProfile("Hote", "char_rookie"));

            registry.Unregister(1UL);

            Assert.That(registry.TryGet(1UL, out _), Is.False);
        }

        [Test]
        public void PlayerModeOnFootIsDistinctFromExistingOnFootValues()
        {
            Assert.That(PlayerMode.OnFoot, Is.Not.EqualTo(PlayerMode.OnFootStop));
            Assert.That(PlayerMode.OnFoot, Is.Not.EqualTo(PlayerMode.OnFootRageRoad));
        }

        [Test]
        public void EnsureNetworkManagerCreatesConfiguredRuntimeManager()
        {
            DestroyNetworkManagers();
            var networkPrefabs = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");

            RoadRageBootstrap.EnsureNetworkManager(networkPrefabs);

            AssertConfiguredNetworkManager(NetworkManager.Singleton, networkPrefabs);
        }

        [Test]
        public void EnsureNetworkManagerRepairsExistingSingletonWithoutNetworkConfig()
        {
            DestroyNetworkManagers();
            var gameObject = new GameObject(ExistingTestManagerName);
            var manager = gameObject.AddComponent<NetworkManager>();
            manager.SetSingleton();
            manager.NetworkConfig = null;
            var networkPrefabs = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");

            RoadRageBootstrap.EnsureNetworkManager(networkPrefabs);

            Assert.That(NetworkManager.Singleton, Is.SameAs(manager));
            AssertConfiguredNetworkManager(manager, networkPrefabs);
        }

        [Test]
        public void NetworkedPlayerRootPrefabIsRuntimeSpawnPrefab()
        {
            var playerRootPrefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            Assert.That(playerRootPrefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable depuis Resources.");

            var networkObject = playerRootPrefab.GetComponent<NetworkObject>();
            Assert.That(networkObject, Is.Not.Null, "NetworkedPlayerRoot doit porter un NetworkObject.");
            Assert.That(networkObject.InScenePlaced, Is.False, "un prefab spawne a runtime ne doit pas etre marque in-scene placed.");
        }

        [Test]
        public void RuntimeVehiclePrefabsAreRegisteredByLobbyEntryPoint()
        {
            var prefabs = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");
            var playerCar = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab");
            var aiVehicle = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");

            Assert.That(prefabs, Is.Not.Null);
            Assert.That(prefabs.PrefabList.Select(entry => entry.Prefab), Does.Contain(playerCar));
            Assert.That(prefabs.PrefabList.Select(entry => entry.Prefab), Does.Contain(aiVehicle));

            var scene = EditorSceneManager.OpenScene("Assets/RoadRage/App/Scenes/MainMenuLobby.unity", OpenSceneMode.Additive);
            try
            {
                var lobbyFlow = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<RoadRage.App.Lobby.LobbyFlowController>(true))
                    .Single();
                var serializedLobbyFlow = new SerializedObject(lobbyFlow);

                Assert.That(serializedLobbyFlow.FindProperty("defaultNetworkPrefabs").objectReferenceValue, Is.SameAs(prefabs));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void NetworkedPlayerRootPrefabHasPresentationWithCatalog()
        {
            var playerRootPrefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            Assert.That(playerRootPrefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable depuis Resources.");

            var presentation = playerRootPrefab.GetComponent<NetworkedPlayerPresentation>();

            Assert.That(presentation, Is.Not.Null, "NetworkedPlayerRoot doit porter la presentation reseau visible.");
            Assert.That(presentation.CharacterCatalog, Is.Not.Null, "la presentation reseau doit connaitre le catalogue de personnages.");
        }

        [Test]
        public void NetworkedPlayerStateCarriesClientIdentityAndPose()
        {
            var source = File.ReadAllText("Assets/RoadRage/Features/Players/NetworkedPlayerState.cs");

            Assert.That(source, Does.Contain("NetworkVariable<ulong> ClientId"));
            Assert.That(source, Does.Contain("NetworkVariable<Vector3> WorldPosition"));
            Assert.That(source, Does.Contain("NetworkVariable<float> YawDegrees"));
            Assert.That(source, Does.Contain("NetworkVariableWritePermission.Server"));
        }

        [Test]
        public void NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId()
        {
            var playerRootPrefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            Assert.That(playerRootPrefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable depuis Resources.");

            var instance = Object.Instantiate(playerRootPrefab);
            try
            {
                var state = instance.GetComponent<NetworkedPlayerState>();
                var presentation = instance.GetComponent<NetworkedPlayerPresentation>();
                Assert.That(state, Is.Not.Null);
                Assert.That(presentation, Is.Not.Null);

                InvokePrivateMethod(presentation, "RefreshVisual", "char_rookie");

                Assert.That(presentation.VisualInstance, Is.Not.Null, "la presentation doit instancier le prefab visible du personnage.");
                Assert.That(presentation.RenderedCharacterId, Is.EqualTo("char_rookie"));
                Assert.That(presentation.VisualInstance.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0));

                var colliders = presentation.VisualInstance.GetComponentsInChildren<Collider>(true);
                for (var i = 0; i < colliders.Length; i++)
                {
                    Assert.That(colliders[i].enabled, Is.False, "les colliders du visuel distant ne doivent pas interagir avec le gameplay local.");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void NetworkedSpawnerDefersInitialSpawnAndDoesNotParentNetworkObjectBeforeSpawn()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs");

            Assert.That(source, Does.Contain("StartCoroutine(SpawnConnectedClientsAfterSceneProcessing(manager))"));
            Assert.That(source, Does.Not.Contain("transform.SetParent"));
            Assert.That(source, Does.Contain("state.ClientId.Value = clientId"));
            Assert.That(source, Does.Contain("state.WorldPosition.Value = spawnPosition"));
        }

        [Test]
        public void FacepunchTransportDoesNotReinitializeOrShutdownBorrowedSteamClient()
        {
            var source = File.ReadAllText("Packages/com.community.netcode.transport.facepunch/Runtime/FacepunchTransport.cs");

            Assert.That(source, Does.Contain("if (SteamClient.IsValid)"));
            Assert.That(source, Does.Contain("m_OwnsSteamClient"));
            Assert.That(source, Does.Contain("if (m_OwnsSteamClient && SteamClient.IsValid)"));
        }

        [Test]
        public void NetworkedPresentationAcceptsPoseOnlyFromRepresentedClient()
        {
            var source = File.ReadAllText("Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs");

            Assert.That(source, Does.Contain("[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]"));
            Assert.That(source, Does.Contain("rpcParams.Receive.SenderClientId"));
            Assert.That(source, Does.Contain("state.ClientId.Value != rpcParams.Receive.SenderClientId"));
            Assert.That(source, Does.Contain("state.WorldPosition.Value = worldPosition"));
            Assert.That(source, Does.Contain("state.YawDegrees.Value = NormalizeYaw(yawDegrees)"));
        }

        [Test]
        public void LocalPlayerPoseReporterTargetsOnlyLocalClientProxy()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/NetworkedLocalPlayerPoseReporter.cs");

            Assert.That(source, Does.Contain("manager.LocalClientId"));
            Assert.That(source, Does.Contain("candidate.RepresentsClient(localClientId)"));
            Assert.That(source, Does.Contain("target.SubmitLocalPose(transform.position, transform.eulerAngles.y)"));
        }

        [Test]
        public void RunSessionMonitorReturnsClientToLobbyWhenHostIsLost()
        {
            Assert.That(NetworkedRunSessionMonitor.ShouldReturnClientToLobby(true, NetworkManager.ServerClientId, 7UL), Is.True);
            Assert.That(NetworkedRunSessionMonitor.ShouldReturnClientToLobby(true, 7UL, 7UL), Is.True);
            Assert.That(NetworkedRunSessionMonitor.ShouldReturnClientToLobby(true, 2UL, 7UL), Is.False);
            Assert.That(NetworkedRunSessionMonitor.ShouldReturnClientToLobby(false, NetworkManager.ServerClientId, 7UL), Is.False);
        }

        [Test]
        public void RunFlowAndMainMenuWireNetworkMonitorReporterAndPersistentNotice()
        {
            var runSource = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");
            var menuSource = File.ReadAllText("Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs");

            Assert.That(runSource, Does.Contain("EnsureNetworkSessionMonitor()"));
            Assert.That(runSource, Does.Contain("localPlayer.AddComponent<NetworkedLocalPlayerPoseReporter>()"));
            Assert.That(menuSource, Does.Contain("bootstrap.Notices.LastNotice.HasValue"));
            Assert.That(menuSource, Does.Contain("screen.ShowNotice(bootstrap.Notices.LastNotice.Value)"));
        }

        [Test]
        public void HudCanShowNetworkedLobbyState()
        {
            var root = new GameObject("HudTest");
            try
            {
                var hud = root.AddComponent<RunCheckpointHudScreen>();
                var lobbyLabel = AddLabel(root, "LobbyLabel");
                var playerLabel = AddLabel(root, "PlayerLabel");
                var futureLabel = AddLabel(root, "FutureLabel");

                SetPrivateField(hud, "lobbyStateLabel", lobbyLabel);
                SetPrivateField(hud, "playerStateLabel", playerLabel);
                SetPrivateField(hud, "futureHudLabel", futureLabel);

                hud.ShowRunState(RunCheckpointHudScreen.NetworkHostLobbyState, "Kenan", "Rookie");

                Assert.That(lobbyLabel.text, Is.EqualTo(RunCheckpointHudScreen.NetworkHostLobbyState));
                Assert.That(playerLabel.text, Is.EqualTo("Joueur : Kenan / Rookie"));
                Assert.That(futureLabel.text, Is.EqualTo(RunCheckpointHudScreen.FutureHudState));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void AssertConfiguredNetworkManager(NetworkManager manager, NetworkPrefabsList networkPrefabs)
        {
            Assert.That(manager, Is.Not.Null);
            Assert.That(manager.NetworkConfig, Is.Not.Null);
            Assert.That(manager.NetworkConfig.NetworkTransport, Is.Not.Null);
            Assert.That(manager.NetworkConfig.NetworkTransport.GetType().FullName, Is.EqualTo("Netcode.Transports.Facepunch.FacepunchTransport"));
            Assert.That(manager.NetworkConfig.ConnectionApproval, Is.True);
            Assert.That(manager.NetworkConfig.EnableSceneManagement, Is.True);
            Assert.That(manager.NetworkConfig.PlayerPrefab, Is.Null);
            Assert.That(manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Count(list => list == networkPrefabs), Is.EqualTo(1));

            var playerRootPrefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            Assert.That(playerRootPrefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable depuis Resources.");
            Assert.That(networkPrefabs.PrefabList.Count(entry => entry.Prefab == playerRootPrefab), Is.EqualTo(1),
                "NetworkedPlayerRoot doit etre enregistre une seule fois dans la liste partagee.");
        }

        private static void DestroyNetworkManagers()
        {
            var managers = Object.FindObjectsByType<NetworkManager>(FindObjectsInactive.Include);
            foreach (var manager in managers)
            {
                if (manager.name == RuntimeManagerName || manager.name == ExistingTestManagerName)
                {
                    Object.DestroyImmediate(manager.gameObject);
                }
            }
        }

        private static TextMeshProUGUI AddLabel(GameObject root, string name)
        {
            var labelObject = new GameObject(name);
            labelObject.transform.SetParent(root.transform);
            return labelObject.AddComponent<TextMeshProUGUI>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, ToTypes(args), null);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, args);
        }

        private static System.Type[] ToTypes(object[] args)
        {
            var types = new System.Type[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                types[i] = args[i].GetType();
            }

            return types;
        }
    }
}
