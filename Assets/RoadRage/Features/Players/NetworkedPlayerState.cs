using RoadRage.Shared.Domain;
using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Squelette host-owned du mode, du siege et de la vie joueur.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedPlayerState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<PlayerMode> Mode = new NetworkVariable<PlayerMode>(
            PlayerMode.Spectating,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<PlayerLifecycle> Lifecycle = new NetworkVariable<PlayerLifecycle>(
            PlayerLifecycle.Alive,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> SeatIndex = new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
