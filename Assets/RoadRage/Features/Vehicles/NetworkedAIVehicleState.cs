using RoadRage.Shared.Domain;
using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Etat host-owned de progression et de comportement d'un vehicule IA. L'identite de route n'est
    /// pas repliquee : elle est portee par la reference de scene <c>laneGraph</c> du
    /// <see cref="NetworkedAIVehicleDriverController"/>, identique sur tous les pairs.
    /// Story 5.2 ajoute WaypointIndex : la progression du vehicule le long de sa route,
    /// avancee/reinitialisee par <see cref="NetworkedAIVehicleDriverController"/> (host-only), jamais
    /// par un client. Story 5.10 la reutilise TELLE QUELLE comme index de noeud dans le
    /// <see cref="LaneGraph"/> : aucune NetworkVariable ajoutee malgre le changement de mecanisme.
    /// Story 5.4 ajoute Behavior : le comportement courant derive par l'hote depuis la propre rage du
    /// vehicule (<see cref="IRageDispositionSource"/>), publie ici en lecture pour tous et en ecriture
    /// serveur uniquement. Ce n'est pas une seconde machine de rage : la verite reste
    /// NetworkedRageState, Behavior n'en est que la projection consommee par le mouvement et le label.
    /// Story 5.7 retire RouteIndex : repliquee sans jamais etre ecrite ni lue, elle payait un cout de
    /// replication pour une seconde source de verite de route que la reference de scene porte deja.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedAIVehicleState : HostOwnedNetworkStateBehaviour
    {
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
