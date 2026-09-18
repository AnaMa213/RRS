using System;
using System.Collections;
using System.Linq;
using RoadRage.App.Run;
using RoadRage.Features.Players;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.DevTools
{
    /// <summary>
    /// Story 0.8 (VAL-027/VAL-028) auto-start helper for the Dev_LobbySmokeTest NetworkManager
    /// harness. Multiplayer Play Mode "Additional Editor Instance" windows (e.g. "Player 2")
    /// show no menu bar and do not receive the Main Editor's keyboard shortcuts, so this uses
    /// the Play Mode Scenario player tag to decide the role automatically on Play: the
    /// untagged Editor player starts the host, a player tagged "Client" starts the client.
    /// Editor-only behavior. The assembly is runtime-compatible so Unity can resolve this
    /// MonoBehaviour when it is attached to a scene object.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoadRageNetcodeSmokeTestAutoStart : MonoBehaviour
    {
        private const string ClientTag = "Client";
        private const string VehicleSandboxSceneName = "Dev_VehicleSandbox";

#if UNITY_EDITOR
        private IEnumerator Start()
        {
            yield return null;

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[RoadRageNetcodeSmokeTestAutoStart] No NetworkManager.Singleton found.");
                yield break;
            }

            var manager = NetworkManager.Singleton;
            if (manager.IsListening)
            {
                Debug.Log("[RoadRageNetcodeSmokeTestAutoStart] NetworkManager already listening; auto-start skipped.");
                yield break;
            }

            var tags = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags;
            var tagList = tags == null ? string.Empty : string.Join(",", tags);
            var hasClientTag = tags != null && tags.Any(tag => string.Equals(tag, ClientTag, StringComparison.OrdinalIgnoreCase));
            var isVirtualProject = Application.dataPath.Replace('\\', '/').Contains("/Library/VP/", StringComparison.OrdinalIgnoreCase);
            var shouldStartClient = hasClientTag || isVirtualProject;

            if (shouldStartClient)
            {
                yield return new WaitForSecondsRealtime(0.5f);

                var started = manager.StartClient();
                Debug.Log($"[RoadRageNetcodeSmokeTestAutoStart] Tags='{tagList}' VirtualProject={isVirtualProject} -> StartClient() returned {started}.");
            }
            else
            {
                var started = manager.StartHost();
                Debug.Log($"[RoadRageNetcodeSmokeTestAutoStart] Tags='{tagList}' VirtualProject={isVirtualProject} -> StartHost() returned {started}.");

                if (started)
                {
                    yield return null;
                    EnsureVehicleSandboxSeatHarness(manager);
                    EnsureVehicleTelemetryView();
                }
            }
        }

        /// <summary>
        /// Story 5.11 : monte la vue de telemetrie physique sur le vehicule de la sandbox, une seule
        /// fois. La vue se desactive d'elle-meme hors build de developpement, donc ce montage ne peut
        /// rien laisser derriere lui dans un build de livraison. Elle ne detient aucun etat de
        /// gameplay : c'est un instrument de mesure, pas une fonctionnalite.
        /// </summary>
        private static void EnsureVehicleTelemetryView()
        {
            var vehicleState = FindAnyObjectByType<NetworkedVehicleState>();
            if (vehicleState == null)
            {
                return;
            }

            if (vehicleState.GetComponent<VehiclePhysicsTelemetryView>() == null)
            {
                vehicleState.gameObject.AddComponent<VehiclePhysicsTelemetryView>();
            }
        }

        private static void EnsureVehicleSandboxSeatHarness(NetworkManager manager)
        {
            if (manager == null || !manager.IsListening || !manager.IsServer)
            {
                return;
            }

            if (!string.Equals(SceneManager.GetActiveScene().name, VehicleSandboxSceneName, StringComparison.Ordinal))
            {
                return;
            }

            if (FindLocalPlayerState(manager.LocalClientId) != null)
            {
                return;
            }

            var playerRootPrefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            if (playerRootPrefab == null)
            {
                Debug.LogError("[RoadRageNetcodeSmokeTestAutoStart] NetworkedPlayerRoot introuvable : test de siege impossible.");
                return;
            }

            var vehicleState = FindAnyObjectByType<NetworkedVehicleState>();
            var spawnPosition = vehicleState == null ? Vector3.zero : vehicleState.transform.TransformPoint(new Vector3(0f, 0f, -3.2f));
            var spawnRotation = vehicleState == null ? Quaternion.identity : vehicleState.transform.rotation;

            var instance = Instantiate(playerRootPrefab);
            instance.name = "NetworkedPlayer_DevVehicleSandbox_" + manager.LocalClientId;
            instance.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

            var networkObject = instance.GetComponent<NetworkObject>();
            var playerState = instance.GetComponent<NetworkedPlayerState>();
            if (networkObject == null || playerState == null)
            {
                Destroy(instance);
                Debug.LogError("[RoadRageNetcodeSmokeTestAutoStart] NetworkedPlayerRoot incomplet : NetworkObject/NetworkedPlayerState requis.");
                return;
            }

            networkObject.Spawn();

            playerState.Mode.Value = PlayerMode.OnFoot;
            playerState.Lifecycle.Value = PlayerLifecycle.Alive;
            playerState.SeatIndex.Value = NetworkedVehicleState.NoSeatIndex;
            playerState.ClientId.Value = manager.LocalClientId;
            playerState.WorldPosition.Value = spawnPosition;
            playerState.YawDegrees.Value = NormalizeYaw(spawnRotation.eulerAngles.y);

            var seatService = NetworkedVehicleSeatService.Instance;
            if (seatService == null)
            {
                seatService = FindAnyObjectByType<NetworkedVehicleSeatService>();
            }

            if (seatService == null)
            {
                var serviceObject = new GameObject("NetworkedVehicleSeatService");
                seatService = serviceObject.AddComponent<NetworkedVehicleSeatService>();
            }

            seatService.Configure(null, null);
            Debug.Log("[RoadRageNetcodeSmokeTestAutoStart] Harness siege vehicule pret : E entree/sortie, Shift+E passager.");
        }

        private static NetworkedPlayerState FindLocalPlayerState(ulong localClientId)
        {
            var candidates = FindObjectsByType<NetworkedPlayerState>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (candidate != null && candidate.IsSpawned && candidate.ClientId.Value == localClientId)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static float NormalizeYaw(float yawDegrees)
        {
            var normalized = yawDegrees % 360f;
            return normalized < 0f ? normalized + 360f : normalized;
        }
#else
        private void Start()
        {
            enabled = false;
        }
#endif
    }
}
