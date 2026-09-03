using Unity.Netcode;

namespace RoadRage.Shared.Networking
{
    /// <summary>
    /// Base commune pour les etats reseau dont la verite appartient au host.
    /// </summary>
    public abstract class HostOwnedNetworkStateBehaviour : NetworkBehaviour, IHostOwnedRuntimeState
    {
        public bool IsHostAuthority
        {
            get { return IsServer; }
        }
    }
}
