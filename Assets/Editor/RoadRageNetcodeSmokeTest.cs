#if UNITY_EDITOR
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Editor
{
    /// <summary>
    /// Story 0.8 (VAL-027/VAL-028) manual smoke-test helper for the Dev_LobbySmokeTest
    /// NetworkManager harness. One-click host/client start so the local Multiplayer Play
    /// Mode two-actor test and the Steam remote test can be run without typing script code
    /// in each Editor instance (Main Editor and Multiplayer Play Mode virtual player).
    /// </summary>
    public static class RoadRageNetcodeSmokeTest
    {
        [MenuItem("RoadRage/Netcode/Start Host (Smoke Test) %#h")]
        public static void StartHost()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[RoadRageNetcodeSmokeTest] No NetworkManager.Singleton found. Open Dev_LobbySmokeTest and enter Play Mode first.");
                return;
            }

            var started = NetworkManager.Singleton.StartHost();
            Debug.Log($"[RoadRageNetcodeSmokeTest] StartHost() returned {started}.");
            LogConnectionState();
        }

        [MenuItem("RoadRage/Netcode/Start Client (Smoke Test) %#j")]
        public static void StartClient()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[RoadRageNetcodeSmokeTest] No NetworkManager.Singleton found. Open Dev_LobbySmokeTest and enter Play Mode first.");
                return;
            }

            var started = NetworkManager.Singleton.StartClient();
            Debug.Log($"[RoadRageNetcodeSmokeTest] StartClient() returned {started}.");
            LogConnectionState();
        }

        [MenuItem("RoadRage/Netcode/Log Connection State (Smoke Test) %#l")]
        public static void LogConnectionState()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null)
            {
                Debug.LogError("[RoadRageNetcodeSmokeTest] No NetworkManager.Singleton found.");
                return;
            }

            Debug.Log($"[RoadRageNetcodeSmokeTest] IsListening={manager.IsListening} IsServer={manager.IsServer} IsHost={manager.IsHost} IsConnectedClient={manager.IsConnectedClient} LocalClientId={manager.LocalClientId} ConnectedClientsCount={manager.ConnectedClientsIds.Count}");
        }

        [MenuItem("RoadRage/Netcode/Shutdown (Smoke Test)")]
        public static void ShutdownNetworkManager()
        {
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            NetworkManager.Singleton.Shutdown();
            Debug.Log("[RoadRageNetcodeSmokeTest] NetworkManager.Shutdown() called.");
        }
    }
}
#endif
