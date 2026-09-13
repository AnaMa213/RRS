using System;
using RoadRage.Shared.Definitions;
using RoadRage.Shared.Domain;
using UnityEngine;

namespace RoadRage.Features.Rage
{
    /// <summary>
    /// Donnee auteur statique des seuils de rage (Story 4.1) et des tendances de temperament
    /// (Story 5.1 : borne de peur et sensibilites, multiplicateurs appliques a la magnitude d'un
    /// NpcReactionEffect). Vit comme asset sous Assets/RoadRage/ScriptableObjects/Rage/ et porte un
    /// id globalement unique en minuscules, stable, sur le meme gabarit que CharacterDef
    /// (Features.Players, Story 1.3) : l'id vit ici, la validation de catalogue (unicite, casse,
    /// doublons) vit dans RageTuningCatalog.TryValidate. Jamais mutee a l'execution.
    /// </summary>
    [CreateAssetMenu(fileName = "RageTuningDef", menuName = "RoadRage/Rage/Rage Tuning Def")]
    public sealed class RageTuningDef : ScriptableObject
    {
        /// <summary>Un palier authored : seuil de RageValue a partir duquel Disposition devient cette valeur.</summary>
        [Serializable]
        public struct RageThreshold
        {
            [SerializeField]
            private RageDisposition disposition;

            [SerializeField]
            private float minValue;

            public RageThreshold(RageDisposition disposition, float minValue)
            {
                this.disposition = disposition;
                this.minValue = minValue;
            }

            public RageDisposition Disposition
            {
                get { return disposition; }
            }

            public float MinValue
            {
                get { return minValue; }
            }
        }

        [SerializeField]
        [Tooltip("Id globalement unique en minuscules, ex. rage_default. Stable : ne jamais le renommer une fois publie.")]
        private string id = string.Empty;

        [SerializeField]
        [Tooltip("Valeur maximale de RageValue ; ApplyRageDelta clampe a cette borne.")]
        private float maxRageValue = 100f;

        [SerializeField]
        [Tooltip("Valeur maximale de FearValue ; ApplyFearDelta et ApplyReactionEffect clampent a cette borne.")]
        private float maxFearValue = 100f;

        [SerializeField]
        [Tooltip("Multiplicateur de sensibilite du canal rage (Story 5.1) : 1 = neutre, 0 = canal inerte.")]
        private float rageSensitivity = 1f;

        [SerializeField]
        [Tooltip("Multiplicateur de sensibilite du canal peur (Story 5.1) : 1 = neutre, 0 = canal inerte.")]
        private float fearSensitivity = 1f;

        [SerializeField]
        [Tooltip("Paliers tries par seuil strictement ascendant. Invariante d'authoring validee par RageTuningCatalog.TryValidate, pas recalculee a l'execution (cf. Design Notes).")]
        private RageThreshold[] thresholds = Array.Empty<RageThreshold>();

        /// <summary>Id stable expose sous la forme partagee attendue par les autres couches.</summary>
        public DefinitionId Id
        {
            get { return new DefinitionId(id); }
        }

        /// <summary>Valeur brute de l'id, exposee pour les gardes de validation du catalogue.</summary>
        public string RawId
        {
            get { return id ?? string.Empty; }
        }

        public float MaxRageValue
        {
            get { return maxRageValue; }
        }

        public float MaxFearValue
        {
            get { return maxFearValue; }
        }

        /// <summary>Multiplicateur applique a la magnitude d'un effet visant le canal rage (0 = canal inerte).</summary>
        public float RageSensitivity
        {
            get { return rageSensitivity; }
        }

        /// <summary>Multiplicateur applique a la magnitude d'un effet visant le canal peur (0 = canal inerte).</summary>
        public float FearSensitivity
        {
            get { return fearSensitivity; }
        }

        public RageThreshold[] Thresholds
        {
            get { return thresholds ?? Array.Empty<RageThreshold>(); }
        }

        public bool TryValidate(out string error)
        {
            if (string.IsNullOrWhiteSpace(RawId) || RawId != RawId.Trim() || RawId != RawId.ToLowerInvariant())
            {
                error = "Id de tuning de rage invalide.";
                return false;
            }

            if (!float.IsFinite(maxRageValue) || maxRageValue < 0f)
            {
                error = "MaxRageValue invalide.";
                return false;
            }

            // Une sensibilite de 0 reste valide : elle rend le canal inerte (matrice I/O, Story 5.1).
            // Seules les valeurs negatives ou non finies sont refusees, comme pour les bornes.
            if (!float.IsFinite(maxFearValue) || maxFearValue < 0f)
            {
                error = "MaxFearValue invalide : 'maxFearValue' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!float.IsFinite(rageSensitivity) || rageSensitivity < 0f)
            {
                error = "RageSensitivity invalide : 'rageSensitivity' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!float.IsFinite(fearSensitivity) || fearSensitivity < 0f)
            {
                error = "FearSensitivity invalide : 'fearSensitivity' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            var currentThresholds = Thresholds;
            for (var i = 0; i < currentThresholds.Length; i++)
            {
                if (!float.IsFinite(currentThresholds[i].MinValue) || currentThresholds[i].MinValue > maxRageValue)
                {
                    error = "Palier de rage invalide a l'index " + i + ".";
                    return false;
                }
            }

            if (!HasAscendingThresholds())
            {
                error = "Paliers de rage non ascendants.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Predicat pur (Design Notes) sur des paliers tries par seuil ascendant : dernier palier
        /// dont le seuil est franchi, Calm par defaut. Reste testable en EditMode sans Netcode.
        /// </summary>
        public RageDisposition ResolveDisposition(float rageValue)
        {
            var resolved = RageDisposition.Calm;
            var currentThresholds = Thresholds;

            for (var i = 0; i < currentThresholds.Length; i++)
            {
                if (rageValue >= currentThresholds[i].MinValue)
                {
                    resolved = currentThresholds[i].Disposition;
                }
            }

            return resolved;
        }

        /// <summary>
        /// Vrai si les paliers sont tries par seuil strictement ascendant -- invariante d'authoring
        /// requise par ResolveDisposition, verifiee par RageTuningCatalog.TryValidate.
        /// </summary>
        public bool HasAscendingThresholds()
        {
            var currentThresholds = Thresholds;

            for (var i = 1; i < currentThresholds.Length; i++)
            {
                if (currentThresholds[i].MinValue <= currentThresholds[i - 1].MinValue)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Vrai si le palier le plus haut reste atteignable : son seuil ne depasse pas MaxRageValue.
        /// NetworkedRageState.ApplyRageDelta clampe RageValue a MaxRageValue, donc un palier authore
        /// au-dessus ne resoudrait jamais -- invariante d'authoring requise, verifiee par
        /// RageTuningCatalog.TryValidate.
        /// </summary>
        public bool HasTopThresholdWithinMaxRageValue()
        {
            var currentThresholds = Thresholds;

            if (currentThresholds.Length == 0)
            {
                return true;
            }

            return currentThresholds[currentThresholds.Length - 1].MinValue <= maxRageValue;
        }

        private void OnValidate()
        {
            if (!TryValidate(out var error))
            {
                Debug.LogWarning("[Rage] RageTuningDef invalide : " + error, this);
            }
        }
    }
}
