using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// Ecran UGUI de setup de personnage (Story 1.3). Il ne connait aucun type du feature joueurs :
    /// il ne recoit que des primitives (texte, teinte) et n'emet que des intentions. La couche App
    /// valide la saisie, ecrit l'etat de session et rafraichit l'affichage.
    /// Rien n'est ecrit ici dans Awake : la couche App est le seul ecrivain de chaque libelle, ce qui
    /// evite la double ecriture d'ordre indefini rencontree en Story 1.2 (Patch 3).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterSetupScreen : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField nameInputField;

        [SerializeField]
        private Button characterCycleButton;

        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TMP_Text characterNameLabel;

        [SerializeField]
        private Image characterPreviewImage;

        [SerializeField]
        private TMP_Text validationLabel;

        /// <summary>Le joueur demande le personnage suivant du catalogue.</summary>
        public event Action NextCharacterRequested;

        /// <summary>Confirmation demandee, avec le texte brut du champ : non normalise, non valide.</summary>
        public event Action<string> ConfirmRequested;

        public event Action CloseRequested;

        private void Awake()
        {
            if (characterCycleButton != null)
            {
                characterCycleButton.onClick.AddListener(RaiseNextCharacterRequested);
            }
            else
            {
                Debug.LogWarning("[UI] CharacterSetupScreen sans reference vers characterCycleButton.");
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(RaiseConfirmRequested);
            }
            else
            {
                Debug.LogWarning("[UI] CharacterSetupScreen sans reference vers confirmButton.");
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(RaiseCloseRequested);
            }
            else
            {
                Debug.LogWarning("[UI] CharacterSetupScreen sans reference vers closeButton.");
            }

            if (nameInputField == null)
            {
                Debug.LogWarning("[UI] CharacterSetupScreen sans reference vers nameInputField.");
            }

            if (characterNameLabel == null)
            {
                Debug.LogWarning("[UI] CharacterSetupScreen sans reference vers characterNameLabel.");
            }

            if (characterPreviewImage == null)
            {
                Debug.LogWarning("[UI] CharacterSetupScreen sans reference vers characterPreviewImage.");
            }

            if (validationLabel == null)
            {
                Debug.LogWarning("[UI] CharacterSetupScreen sans reference vers validationLabel.");
            }
        }

        /// <summary>
        /// Affiche le personnage courant : nom lisible et teinte de la silhouette placeholder.
        /// L'emplacement du vrai modele greybox arrive en Story 1.4.
        /// </summary>
        public void ShowCharacter(string displayName, Color previewTint)
        {
            if (characterNameLabel != null)
            {
                characterNameLabel.text = displayName ?? string.Empty;
            }

            if (characterPreviewImage != null)
            {
                characterPreviewImage.color = previewTint;
            }
        }

        /// <summary>Affiche un retour de validation visible. Jamais d'echec silencieux.</summary>
        public void ShowValidation(string message)
        {
            if (validationLabel == null)
            {
                return;
            }

            validationLabel.text = message ?? string.Empty;
            validationLabel.gameObject.SetActive(true);
        }

        public void ClearValidation()
        {
            if (validationLabel == null)
            {
                return;
            }

            validationLabel.text = string.Empty;
            validationLabel.gameObject.SetActive(false);
        }

        /// <summary>Renseigne le champ de saisie depuis la couche App (reouverture apres confirmation).</summary>
        public void SetName(string displayName)
        {
            if (nameInputField != null)
            {
                nameInputField.text = displayName ?? string.Empty;
            }
        }

        /// <summary>
        /// Texte brut actuellement saisi, non normalise. Prive : la couche App le recoit dans la charge
        /// utile de ConfirmRequested, elle n'a jamais a interroger l'ecran.
        /// </summary>
        private string GetRawName()
        {
            return nameInputField == null ? string.Empty : nameInputField.text;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RaiseNextCharacterRequested()
        {
            NextCharacterRequested?.Invoke();
        }

        private void RaiseConfirmRequested()
        {
            ConfirmRequested?.Invoke(GetRawName());
        }

        private void RaiseCloseRequested()
        {
            CloseRequested?.Invoke();
        }
    }
}
