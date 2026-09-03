using RoadRage.Shared.Domain;
using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Run
{
    /// <summary>
    /// Squelette host-owned de la verite de run.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedRunState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<RunPhase> Phase = new NetworkVariable<RunPhase>(
            RunPhase.NotStarted,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> SessionSeed = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
