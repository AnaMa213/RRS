using RoadRage.Shared.Domain;
using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Rage
{
    /// <summary>
    /// Etat host-owned de la rage et de la peur d'une cible (Stories 4.1 et 5.1). Une instance = une
    /// cible ; la multiplicite ("au moins une cible independamment") vient d'un composant par GameObject
    /// cible, pas d'une collection interne.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedRageState : HostOwnedNetworkStateBehaviour, IRageDispositionSource
    {
        public NetworkVariable<RageDisposition> Disposition = new NetworkVariable<RageDisposition>(
            RageDisposition.Calm,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<float> RageValue = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<float> FearValue = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        /// <summary>
        /// Implementation de <see cref="IRageDispositionSource"/> (Story 5.4) : lecture seule de la
        /// disposition synchronisee, pour les consommateurs qui ne peuvent pas dependre de
        /// Features/Rage (Features/Vehicles). Aucun setter : la rage n'est mutee que par cette feature.
        /// </summary>
        public RageDisposition CurrentDisposition
        {
            get { return Disposition.Value; }
        }

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

        /// <summary>
        /// Applique un delta de peur, clampe FearValue dans [0, tuning.MaxFearValue]. Ne touche jamais
        /// RageValue ni Disposition : les deux canaux restent independants. Meme convention que
        /// ApplyRageDelta : pas de garde IsServer ici, no-op silencieux si tuning est nul.
        /// </summary>
        public void ApplyFearDelta(float delta, RageTuningDef tuning)
        {
            if (tuning == null)
            {
                return;
            }

            FearValue.Value = Mathf.Clamp(FearValue.Value + delta, 0f, tuning.MaxFearValue);
        }

        /// <summary>
        /// Applique un <see cref="NpcReactionEffect"/> (Story 5.1) : la magnitude est modulee par les
        /// sensibilites authored du tuning (multiplicateurs, pas des deltas absolus) puis repartie sur
        /// les canaux vises. Disposition n'est recalculee que si RageValue a effectivement bouge, via
        /// ResolveDisposition (unique predicat existant).
        ///
        /// Contrat d'etat (Never de la story) : ne leve jamais. Un tuning absent, un effet vide ou
        /// invalide (canal inconnu, magnitude non finie), un canal None, une magnitude nulle ou une
        /// sensibilite nulle rendent l'appel inerte et silencieux ; une sensibilite non finie (tuning
        /// non valide par TryValidate) rend le canal inerte plutot que d'ecrire un NaN dans une
        /// NetworkVariable synchronisee.
        /// </summary>
        public void ApplyReactionEffect(NpcReactionEffect effect, RageTuningDef tuning)
        {
            if (tuning == null || !effect.TryValidate(out _) || effect.Channel == ReactionChannel.None || effect.Magnitude == 0f)
            {
                return;
            }

            var previousRage = RageValue.Value;
            var rageDelta = effect.AffectsRage ? effect.Magnitude * tuning.RageSensitivity : 0f;
            var fearDelta = effect.AffectsFear ? effect.Magnitude * tuning.FearSensitivity : 0f;

            if (float.IsFinite(rageDelta) && rageDelta != 0f)
            {
                RageValue.Value = Mathf.Clamp(previousRage + rageDelta, 0f, tuning.MaxRageValue);
            }

            if (float.IsFinite(fearDelta) && fearDelta != 0f)
            {
                FearValue.Value = Mathf.Clamp(FearValue.Value + fearDelta, 0f, tuning.MaxFearValue);
            }

            if (RageValue.Value != previousRage)
            {
                Disposition.Value = tuning.ResolveDisposition(RageValue.Value);
            }
        }
    }
}
