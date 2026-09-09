using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Identite reseau et frontiere du module Vehicules pour la voiture partagee.
    /// Volontairement vide (aucun NetworkVariable) : ancrage pour les stories 3.2 (conduite)
    /// et 3.3 (sieges), qui y ajouteront leurs propres champs.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkedVehicleState : HostOwnedNetworkStateBehaviour
    {
    }
}
