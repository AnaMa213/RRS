using RoadRage.Shared.Domain;
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
    /// Story 5.4 ajoute Behavior : le comportement courant derive par l'hote depuis la propre rage du
    /// vehicule (<see cref="IRageDispositionSource"/>), publie ici en lecture pour tous et en ecriture
    /// serveur uniquement. Ce n'est pas une seconde machine de rage : la verite reste
    /// NetworkedRageState, Behavior n'en est que la projection consommee par le mouvement et le label.
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

        public NetworkVariable<RageDisposition> Behavior = new NetworkVariable<RageDisposition>(
            RageDisposition.Calm,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
