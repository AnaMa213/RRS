using System.Collections.Generic;
using RoadRage.Shared.Definitions;
using UnityEngine;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Source unique des personnages selectionnables (Story 1.3). Donnee auteur statique : la liste
    /// est authored dans l'asset, jamais mutee a l'execution.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterCatalog", menuName = "RoadRage/Players/Character Catalog")]
    public sealed class CharacterCatalog : ScriptableObject
    {
        [SerializeField]
        private List<CharacterDef> characters = new List<CharacterDef>();

        public int Count
        {
            get { return characters == null ? 0 : characters.Count; }
        }

        /// <summary>
        /// Retourne la definition a l'index donne, ou null si l'index sort du catalogue.
        /// Ne leve jamais : un catalogue vide ou mal assigne doit rendre le flux inerte, pas le casser.
        /// </summary>
        public CharacterDef GetAt(int index)
        {
            if (characters == null || index < 0 || index >= characters.Count)
            {
                return null;
            }

            return characters[index];
        }

        public bool TryGetById(DefinitionId id, out CharacterDef character)
        {
            character = null;

            if (characters == null || id.IsEmpty)
            {
                return false;
            }

            for (var i = 0; i < characters.Count; i++)
            {
                var candidate = characters[i];
                if (candidate != null && candidate.Id == id)
                {
                    character = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Index de la definition portant cet id, ou -1 si absente. Sert a reafficher le personnage
        /// deja confirme a la reouverture de l'ecran de setup.
        /// </summary>
        public int IndexOf(DefinitionId id)
        {
            if (characters == null || id.IsEmpty)
            {
                return -1;
            }

            for (var i = 0; i < characters.Count; i++)
            {
                var candidate = characters[i];
                if (candidate != null && candidate.Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Garde de coherence des donnees auteur : refuse les entrees nulles, les ids vides,
        /// les ids non minuscules et les ids dupliques. Testable sans Editor.
        /// </summary>
        public bool TryValidate(out string error)
        {
            if (characters == null || characters.Count == 0)
            {
                error = "Catalogue de personnages vide : aucun personnage selectionnable.";
                return false;
            }

            var seen = new HashSet<string>();

            for (var i = 0; i < characters.Count; i++)
            {
                var candidate = characters[i];

                if (candidate == null)
                {
                    error = "Entree de catalogue nulle a l'index " + i + ".";
                    return false;
                }

                var rawId = candidate.RawId;

                if (string.IsNullOrWhiteSpace(rawId))
                {
                    error = "Id de personnage vide a l'index " + i + ".";
                    return false;
                }

                // Un id borde d'espaces passe IsNullOrWhiteSpace et la comparaison de casse, mais ces
                // espaces voyagent ensuite dans le DefinitionId et casseraient la comparaison d'ids
                // cote synchro reseau (Epic 2). Refuse ici, ou l'auteur peut encore corriger la donnee.
                if (rawId != rawId.Trim())
                {
                    error = "Id de personnage borde d'espaces a l'index " + i + " : '" + rawId + "'.";
                    return false;
                }

                if (rawId != rawId.ToLowerInvariant())
                {
                    error = "Id de personnage non minuscule a l'index " + i + " : " + rawId + ".";
                    return false;
                }

                if (!seen.Add(rawId))
                {
                    error = "Id de personnage duplique : " + rawId + ".";
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
                Debug.LogWarning("[Players] CharacterCatalog invalide : " + error, this);
            }
        }
    }
}
