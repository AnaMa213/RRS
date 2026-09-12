using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.PassengerActions
{
    /// <summary>Marqueur unique, replique et host-owned des incidents crees par les actions passager.</summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedPassengerActionIncidentState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<int> ActivationCount = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public bool IsActive => ActivationCount.Value > 0;

        public void ActivateOrRefresh()
        {
            if (IsSpawned && !IsServer)
            {
                return;
            }

            ActivationCount.Value++;
        }
    }
}
