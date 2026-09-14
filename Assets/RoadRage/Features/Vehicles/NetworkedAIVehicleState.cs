using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Etat host-owned route/mouvement IA. RouteIndex reste le squelette d'identite de route (non
    /// mute par cette story). Story 5.2 ajoute WaypointIndex : la progression du vehicule le long des
    /// <see cref="RouteWaypoints"/> de sa route, avancee/reinitialisee par
    /// <see cref="NetworkedAIVehicleDriverController"/> (host-only), jamais par un client.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedAIVehicleState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<int> RouteIndex = new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> WaypointIndex = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
