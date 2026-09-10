using RoadRage.Features.Players;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Intent local Story 3.5 porte par NetworkedPlayerRoot : un joueur Alive pres d'un coequipier
    /// Downed demande au host de tenter une resurrection -- meme patron RPC client -> host que
    /// NetworkedVehicleRecoveryIntent (Story 3.4) : l'emetteur ne transmet jamais de cible, le host
    /// resout et valide seul le coequipier Downed le plus proche (NetworkedPlayerLifecycleService.
    /// TryReviveNearestDowned) avant d'appliquer quoi que ce soit.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedPlayerState))]
    public sealed class NetworkedPlayerReviveIntent : NetworkBehaviour
    {
        [SerializeField]
        private Key reviveKey = Key.F;

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
            if (!IsSpawned || state == null || !IsRepresentingLocalAlivePlayer())
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[reviveKey].wasPressedThisFrame)
            {
                return;
            }

            RequestRevive();
        }

        public void RequestRevive()
        {
            CacheState();

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || state == null || state.ClientId.Value != manager.LocalClientId)
            {
                return;
            }

            if (IsServer)
            {
                ApplyServerReviveRequest(manager.LocalClientId);
                return;
            }

            RequestReviveRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestReviveRpc(RpcParams rpcParams = default)
        {
            CacheState();

            if (state == null || state.ClientId.Value != rpcParams.Receive.SenderClientId)
            {
                return;
            }

            ApplyServerReviveRequest(rpcParams.Receive.SenderClientId);
        }

        private void ApplyServerReviveRequest(ulong reviverClientId)
        {
            if (!IsServer || NetworkedPlayerLifecycleService.Instance == null)
            {
                return;
            }

            NetworkedPlayerLifecycleService.Instance.TryReviveNearestDowned(reviverClientId);
        }

        private bool IsRepresentingLocalAlivePlayer()
        {
            var manager = NetworkManager.Singleton;
            return manager != null && manager.IsClient && state != null
                && state.ClientId.Value == manager.LocalClientId
                && state.Lifecycle.Value == PlayerLifecycle.Alive;
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
