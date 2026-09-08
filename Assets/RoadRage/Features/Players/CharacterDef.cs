using RoadRage.Shared.Definitions;
using UnityEngine;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Donnee auteur statique d'un personnage selectionnable (Story 1.3). Vit comme asset sous
    /// Assets/RoadRage/ScriptableObjects/Players/ et porte un id globalement unique en minuscules,
    /// stable et directement exploitable par la synchro reseau de l'Epic 2.
    /// Aucune valeur de session runtime ne vit ici : le profil joueur est un objet C# pur porte par
    /// le depot de session (epic-1-context).
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDef", menuName = "RoadRage/Players/Character Def")]
    public sealed class CharacterDef : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Id globalement unique en minuscules, ex. char_rookie. Stable : ne jamais le renommer une fois publie.")]
        private string id = string.Empty;

        [SerializeField]
        private string displayName = string.Empty;

        [SerializeField]
        [Tooltip("Teinte de la silhouette placeholder affichee par l'ecran de setup.")]
        private Color previewTint = Color.white;

        [SerializeField]
        [Tooltip("Emplacement reserve au greybox de la Story 1.4 (intake Blender). Volontairement vide en Story 1.3.")]
        private GameObject previewPrefab;

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

        public string DisplayName
        {
            get { return string.IsNullOrEmpty(displayName) ? RawId : displayName; }
        }

        public Color PreviewTint
        {
            get { return previewTint; }
        }

        /// <summary>
        /// Emplacement du modele placeholder. Reste nul en Story 1.3 : le greybox arrive en Story 1.4,
        /// apres le gate d'intake Blender.
        /// </summary>
        public GameObject PreviewPrefab
        {
            get { return previewPrefab; }
        }
    }
}
