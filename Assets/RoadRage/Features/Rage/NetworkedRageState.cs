using RoadRage.Shared.Domain;
using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Rage
{
    /// <summary>
    /// Squelette host-owned de la rage par vehicule ennemi.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedRageState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<RageDisposition> Disposition = new NetworkVariable<RageDisposition>(
            RageDisposition.Calm,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
