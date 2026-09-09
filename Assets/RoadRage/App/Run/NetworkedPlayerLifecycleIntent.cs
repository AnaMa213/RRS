using RoadRage.Features.Players;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Point d'entree joueur du respawn individuel (Story 2.7). Lit la touche de respawn localement
    /// quand ce composant represente le client local et que son NetworkedPlayerState.Lifecycle est
    /// Dead, puis envoie l'intention au hote -- meme pattern que
    /// NetworkedPlayerPresentation.SubmitLocalPose/SubmitPoseRpc (Story 2.5) : chemin direct si
    /// IsServer, sinon Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone) valide
    /// contre rpcParams.Receive.SenderClientId (objets host-owned, RequireOwnership par defaut ne
    /// fonctionnerait pas pour le client concerne).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedPlayerState))]
    public sealed class NetworkedPlayerLifecycleIntent : NetworkBehaviour
    {
        private NetworkedPlayerState state;

        private void Awake()
        {
            CacheState();
        }

        public override void OnNetworkSpawn()
        {
            CacheState();
        }

        private void Update()
        {
            if (!IsSpawned || state == null || !IsRepresentingLocalClient())
            {
                return;
            }

            if (state.Lifecycle.Value != PlayerLifecycle.Dead)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RequestRespawn();
            }
        }

        public void RequestRespawn()
        {
            CacheState();

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || state == null || state.ClientId.Value != manager.LocalClientId)
            {
                return;
            }

            if (IsServer)
            {
                ApplyServerRespawn();
                return;
            }

            RequestRespawnRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestRespawnRpc(RpcParams rpcParams = default)
        {
            CacheState();

            if (state == null || state.ClientId.Value != rpcParams.Receive.SenderClientId)
            {
                return;
            }

            ApplyServerRespawn();
        }

        private void ApplyServerRespawn()
        {
            if (!IsServer || state == null || NetworkedPlayerLifecycleService.Instance == null)
            {
                return;
            }

            NetworkedPlayerLifecycleService.Instance.TryRespawn(state.ClientId.Value);
        }

        private bool IsRepresentingLocalClient()
        {
            var manager = NetworkManager.Singleton;
            return manager != null && manager.IsClient && state != null && state.ClientId.Value == manager.LocalClientId;
        }

        private void CacheState()
        {
            if (state == null)
            {
                state = GetComponent<NetworkedPlayerState>();
            }
        }
    }
}
