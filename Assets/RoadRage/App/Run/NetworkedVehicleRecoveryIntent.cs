using RoadRage.Features.Players;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Intent local Story 3.4 porte par NetworkedPlayerRoot : le client ne recupere jamais la voiture
    /// lui-meme, il demande seulement au host d'appliquer la recuperation deja implementee cote
    /// NetworkedVehicleDriverController.RecoverAtRecoveryPoint -- meme patron RPC client -> host que
    /// NetworkedVehicleSeatIntent (Story 3.3) : la requete n'est appliquee que si l'emetteur est bien
    /// le conducteur actuel du siege 0 (NetworkedVehicleState.DriverClientId), verifie cote host.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedPlayerState))]
    public sealed class NetworkedVehicleRecoveryIntent : NetworkBehaviour
    {
        [SerializeField]
        private Key recoveryKey = Key.R;

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
            if (!IsSpawned || state == null || !IsRepresentingLocalDriver())
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[recoveryKey].wasPressedThisFrame)
            {
                return;
            }

            RequestRecovery();
        }

        public void RequestRecovery()
        {
            CacheState();

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || state == null || state.ClientId.Value != manager.LocalClientId)
            {
                return;
            }

            if (IsServer)
            {
                ApplyServerRecoveryRequest(manager.LocalClientId);
                return;
            }

            RequestRecoveryRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestRecoveryRpc(RpcParams rpcParams = default)
        {
            CacheState();

            if (state == null || state.ClientId.Value != rpcParams.Receive.SenderClientId)
            {
                return;
            }

            ApplyServerRecoveryRequest(rpcParams.Receive.SenderClientId);
        }

        /// <summary>Requete ignoree si l'emetteur n'est pas le conducteur actuel du siege 0 (matrice I/O Story 3.4).</summary>
        private void ApplyServerRecoveryRequest(ulong clientId)
        {
            if (!IsServer)
            {
                return;
            }

            var vehicleState = FindAnyObjectByType<NetworkedVehicleState>();
            if (vehicleState == null || vehicleState.DriverClientId.Value != clientId)
            {
                Debug.LogWarning("[Vehicles] Recuperation manuelle refusee : emetteur non conducteur du siege 0.");
                return;
            }

            var driverController = vehicleState.GetComponent<NetworkedVehicleDriverController>();
            if (driverController == null)
            {
                return;
            }

            driverController.RecoverAtRecoveryPoint();
        }

        private bool IsRepresentingLocalDriver()
        {
            var manager = NetworkManager.Singleton;
            return manager != null && manager.IsClient && state != null
                && state.ClientId.Value == manager.LocalClientId
                && state.Mode.Value == PlayerMode.Driver;
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
