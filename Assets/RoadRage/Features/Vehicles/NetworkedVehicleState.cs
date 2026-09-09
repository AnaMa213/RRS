using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Identite reseau et frontiere du module Vehicules pour la voiture partagee. Story 3.2 y
    /// ajoute la revendication de conducteur (DriverClientId) ; Story 3.3 y ajoutera l'occupation
    /// formelle des sieges passagers.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkedVehicleState : HostOwnedNetworkStateBehaviour
    {
        /// <summary>
        /// Sentinel signifiant "aucun conducteur" -- meme esprit que SeatIndex = -1 dans
        /// NetworkedPlayerState (Story 2.6), mais propre au module Vehicules : aucun champ
        /// SeatIndex/PlayerMode n'est touche ici (occupation formelle des sieges = Story 3.3).
        /// </summary>
        public const ulong UnclaimedDriverClientId = ulong.MaxValue;

        public NetworkVariable<ulong> DriverClientId = new NetworkVariable<ulong>(
            UnclaimedDriverClientId,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public bool IsDriver(ulong clientId)
        {
            return DriverClientId.Value == clientId;
        }
    }
}
