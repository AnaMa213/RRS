using System.Collections.Generic;
using RoadRage.Shared.Definitions;
using UnityEngine;

namespace RoadRage.Features.Rage
{
    /// <summary>
    /// Source unique des tunings de rage authores (Story 4.1). Miroir exact de CharacterCatalog
    /// (Features.Players, Story 1.3) : donnee auteur statique, jamais mutee a l'execution.
    /// Catalogue scoped a la feature Rage -- pas le catalogue generique multi-types au bootstrap
    /// decrit par l'AD-25 (Never de la story).
    /// </summary>
    [CreateAssetMenu(fileName = "RageTuningCatalog", menuName = "RoadRage/Rage/Rage Tuning Catalog")]
    public sealed class RageTuningCatalog : ScriptableObject
    {
        [SerializeField]
        private List<RageTuningDef> tunings = new List<RageTuningDef>();

        public int Count
        {
            get { return tunings == null ? 0 : tunings.Count; }
        }

        /// <summary>
        /// Retourne la definition a l'index donne, ou null si l'index sort du catalogue.
        /// Ne leve jamais : un catalogue vide ou mal assigne doit rendre le flux inerte, pas le casser.
        /// </summary>
        public RageTuningDef GetAt(int index)
        {
            if (tunings == null || index < 0 || index >= tunings.Count)
            {
                return null;
            }

            return tunings[index];
        }

        public bool TryGetById(DefinitionId id, out RageTuningDef tuning)
        {
            tuning = null;

            if (tunings == null || id.IsEmpty)
            {
                return false;
            }

            for (var i = 0; i < tunings.Count; i++)
            {
                var candidate = tunings[i];
                if (candidate != null && candidate.Id == id)
                {
                    tuning = candidate;
                    return true;
                }
            }

            return false;
        }

        public int IndexOf(DefinitionId id)
        {
            if (tunings == null || id.IsEmpty)
            {
                return -1;
            }

            for (var i = 0; i < tunings.Count; i++)
            {
                var candidate = tunings[i];
                if (candidate != null && candidate.Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Garde de coherence des donnees auteur : refuse les entrees nulles, les ids vides,
        /// les ids non minuscules, les ids dupliques, et les paliers non ascendants (Design Notes).
        /// Testable sans Editor.
        /// </summary>
        public bool TryValidate(out string error)
        {
            if (tunings == null || tunings.Count == 0)
            {
                error = "Catalogue de tuning de rage vide : aucune definition de rage.";
                return false;
            }

            var seen = new HashSet<string>();

            for (var i = 0; i < tunings.Count; i++)
            {
                var candidate = tunings[i];

                if (candidate == null)
                {
                    error = "Entree de catalogue nulle a l'index " + i + ".";
                    return false;
                }

                var rawId = candidate.RawId;

                if (string.IsNullOrWhiteSpace(rawId))
                {
                    error = "Id de tuning de rage vide a l'index " + i + ".";
                    return false;
                }

                // Un id borde d'espaces passe IsNullOrWhiteSpace et la comparaison de casse, mais ces
                // espaces voyagent ensuite dans le DefinitionId et casseraient la comparaison d'ids
                // cote synchro reseau. Refuse ici, ou l'auteur peut encore corriger la donnee.
                if (rawId != rawId.Trim())
                {
                    error = "Id de tuning de rage borde d'espaces a l'index " + i + " : '" + rawId + "'.";
                    return false;
                }

                if (rawId != rawId.ToLowerInvariant())
                {
                    error = "Id de tuning de rage non minuscule a l'index " + i + " : " + rawId + ".";
                    return false;
                }

                if (!seen.Add(rawId))
                {
                    error = "Id de tuning de rage duplique : " + rawId + ".";
                    return false;
                }

                if (!candidate.HasAscendingThresholds())
                {
                    error = "Paliers non ascendants pour le tuning de rage " + rawId + ".";
                    return false;
                }

                if (!candidate.HasTopThresholdWithinMaxRageValue())
                {
                    error = "Palier au-dessus de MaxRageValue pour le tuning de rage " + rawId + ".";
                    return false;
                }

                if (!candidate.TryValidate(out error))
                {
                    error = "Tuning de rage invalide " + rawId + " : " + error;
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private void OnValidate()
        {
            string error;
            if (!TryValidate(out error))
            {
                Debug.LogWarning("[Rage] RageTuningCatalog invalide : " + error, this);
            }
        }
    }
}
