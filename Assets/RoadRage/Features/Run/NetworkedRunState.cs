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

        /// <summary>
        /// Story 5.6 (AD-17/AD-22) : etat de l'UNIQUE evenement Rage Road du run (AD-16). Vit ici et
        /// nulle part ailleurs -- ni champ local, ni seconde source de verite. Server-write : seul
        /// l'hote declenche et avance l'evenement, les clients lisent (y compris les rejoignants tardifs).
        /// </summary>
        public NetworkVariable<RageRoadEventState> RageRoadEvent = new NetworkVariable<RageRoadEventState>(
            RageRoadEventState.Idle,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        /// <summary>
        /// Cible de l'evenement Rage Road, publiee au meme moment que <see cref="RageRoadEvent"/>
        /// (AD-20 : une <see cref="NetworkObjectReference"/>, jamais un id authored ni un index de
        /// voie). Reference explicitement nulle tant qu'aucun evenement n'existe ; si elle cesse de se
        /// resoudre (cible detruite ou despawnee), l'etat reste celui publie par l'hote : aucun
        /// retargeting silencieux et aucun reset automatique.
        /// </summary>
        public NetworkVariable<NetworkObjectReference> RageRoadEventTarget = new NetworkVariable<NetworkObjectReference>(
            new NetworkObjectReference((NetworkObject)null),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
