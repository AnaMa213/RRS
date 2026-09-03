using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Economy
{
    /// <summary>
    /// Squelette host-owned du portefeuille commun.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedCrewEconomyState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<int> CrewWallet = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
