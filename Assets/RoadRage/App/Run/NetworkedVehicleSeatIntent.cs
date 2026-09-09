using RoadRage.Features.Players;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Intent local Story 3.3 porte par NetworkedPlayerRoot : le client ne choisit jamais lui-meme
    /// son siege, il demande seulement au host de basculer entree/sortie. Shift+E demande un siege
    /// passager quand le joueur veut explicitement eviter le siege conducteur.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedPlayerState))]
    public sealed class NetworkedVehicleSeatIntent : NetworkBehaviour
    {
        [SerializeField]
        private Key enterExitKey = Key.E;

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

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[enterExitKey].wasPressedThisFrame)
            {
                return;
            }

            RequestEnterOrExit(IsPassengerModifierPressed(keyboard));
        }

        public void RequestEnterOrExit(bool preferPassenger)
        {
            CacheState();

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || state == null || state.ClientId.Value != manager.LocalClientId)
            {
                return;
            }

            if (IsServer)
            {
                ApplyServerSeatRequest(manager.LocalClientId, preferPassenger);
                return;
            }

            RequestEnterOrExitRpc(preferPassenger);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestEnterOrExitRpc(bool preferPassenger, RpcParams rpcParams = default)
        {
            CacheState();

            if (state == null || state.ClientId.Value != rpcParams.Receive.SenderClientId)
            {
                return;
            }

            ApplyServerSeatRequest(rpcParams.Receive.SenderClientId, preferPassenger);
        }

        private void ApplyServerSeatRequest(ulong clientId, bool preferPassenger)
        {
            if (!IsServer)
            {
                return;
            }

            var service = NetworkedVehicleSeatService.Instance;
            if (service == null)
            {
                Debug.LogWarning("[Vehicles] Demande de siege ignoree : NetworkedVehicleSeatService absent.");
                return;
            }

            service.RequestEnterOrExit(clientId, preferPassenger);
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

        private static bool IsPassengerModifierPressed(Keyboard keyboard)
        {
            return keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }
    }
}
