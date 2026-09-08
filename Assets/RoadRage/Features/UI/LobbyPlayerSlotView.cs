using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// Une entree de roster dans l'ecran de lobby (Story 2.4) : silhouette teintee du personnage
    /// choisi, nom, et etat pret ; ou un etat vide si aucun joueur n'occupe ce slot. Pur relais
    /// d'affichage sans intention propre, mute uniquement sa propre presentation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LobbyPlayerSlotView : MonoBehaviour
    {
        private static readonly Color EmptySlotTint = new Color(1f, 1f, 1f, 0.2f);

        [SerializeField]
        private Image portraitImage;

        [SerializeField]
        private TMP_Text nameLabel;

        private void Awake()
        {
            if (portraitImage == null)
            {
                Debug.LogWarning("[UI] LobbyPlayerSlotView sans reference vers portraitImage.");
            }

            if (nameLabel == null)
            {
                Debug.LogWarning("[UI] LobbyPlayerSlotView sans reference vers nameLabel.");
            }
        }

        /// <summary>Affiche ce slot comme inoccupe : aucun joueur connecte a cette place du roster.</summary>
        public void ShowEmpty()
        {
            if (portraitImage != null)
            {
                portraitImage.color = EmptySlotTint;
            }

            if (nameLabel != null)
            {
                nameLabel.text = "-- emplacement libre --";
            }
        }

        /// <summary>Affiche un joueur connecte : teinte de son personnage, son nom, et son etat pret.</summary>
        public void ShowPlayer(string displayName, Color portraitTint, bool ready)
        {
            if (portraitImage != null)
            {
                portraitImage.color = portraitTint;
            }

            if (nameLabel != null)
            {
                var name = string.IsNullOrEmpty(displayName) ? "?" : displayName;
                nameLabel.text = name + " - " + (ready ? "Pret" : "En attente");
            }
        }
    }
}
