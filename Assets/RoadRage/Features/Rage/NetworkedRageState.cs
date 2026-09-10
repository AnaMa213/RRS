using RoadRage.Shared.Domain;
using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Rage
{
    /// <summary>
    /// Etat host-owned de la rage d'une cible (Story 4.1). Une instance = une cible ; la
    /// multiplicite ("au moins une cible independamment") vient d'un composant par GameObject
    /// cible, pas d'une collection interne.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedRageState : HostOwnedNetworkStateBehaviour
    {
        public NetworkVariable<RageDisposition> Disposition = new NetworkVariable<RageDisposition>(
            RageDisposition.Calm,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<float> RageValue = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        /// <summary>
        /// Applique un delta de rage, clampe RageValue dans [0, tuning.MaxRageValue], puis recalcule
        /// Disposition via tuning.ResolveDisposition (meme convention que NetworkedVehicleState.ApplyDamage :
        /// pas de garde IsServer explicite ici, l'appelant est responsable du contexte hote). No-op
        /// silencieux si tuning est nul (matrice I/O : "Tuning absent").
        /// </summary>
        public void ApplyRageDelta(float delta, RageTuningDef tuning)
        {
            if (tuning == null)
            {
                return;
            }

            var next = Mathf.Clamp(RageValue.Value + delta, 0f, tuning.MaxRageValue);
            RageValue.Value = next;
            Disposition.Value = tuning.ResolveDisposition(next);
        }
    }
}
