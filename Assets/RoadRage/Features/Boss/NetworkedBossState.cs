using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Boss
{
    /// <summary>
    /// Squelette host-owned du point de terminaison boss.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedBossState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<bool> IsDefeated = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
