using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Squelette host-owned de l'etat route/mouvement IA.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedAIVehicleState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<int> RouteIndex = new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
