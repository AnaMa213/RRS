using System;
using RoadRage.Shared.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// Ecran UGUI du menu principal. N'appelle jamais de chargement de scene ni de fermeture d'application :
    /// il n'emet que des intentions, la couche App decide du routage et du quit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuScreen : MonoBehaviour
    {
        [SerializeField]
        private GameObject menuPanel;

        [SerializeField]
        private GameObject setupPanel;

        [SerializeField]
        private GameObject noticePanel;

        [SerializeField]
        private TMP_Text titleLabel;

        [SerializeField]
        private TMP_Text noticeText;

        [SerializeField]
        private Button playButton;

        [SerializeField]
        private Button quitButton;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private MenuCharacterPreview characterPreview;

        [SerializeField]
        private Button primaryCharacterButton;

        [SerializeField]
        private Button secondaryCharacterButton;

        [SerializeField]
        private TMP_Text selectedCharacterLabel;

        public event Action PlayRequested;

        public event Action QuitRequested;

        public event Action BackRequested;

        /// <summary>Choix cosmetique demande depuis le menu. La couche App le traduit vers le catalogue.</summary>
        public event Action<CharacterOption> CharacterOptionRequested;

        /// <summary>
        /// Emplacement cosmetique propose par le menu (Story 4.5). Le contenu reel des emplacements
        /// reste authored dans la scene et traduit par la couche App : cet ecran ne connait ni
        /// catalogue ni identifiant de personnage.
        /// </summary>
        public enum CharacterOption
        {
            Primary = 0,
            Secondary = 1,
        }

        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(RaisePlayRequested);
            }
            else
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers playButton.");
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(RaiseQuitRequested);
            }
            else
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers quitButton.");
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(RaiseBackRequested);
            }
            else
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers backButton.");
            }

            if (primaryCharacterButton != null)
            {
                primaryCharacterButton.onClick.AddListener(RaisePrimaryCharacterRequested);
            }
            else
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers primaryCharacterButton.");
            }

            if (secondaryCharacterButton != null)
            {
                secondaryCharacterButton.onClick.AddListener(RaiseSecondaryCharacterRequested);
            }
            else
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers secondaryCharacterButton.");
            }

            if (characterPreview == null)
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers characterPreview.");
            }

            if (selectedCharacterLabel == null)
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers selectedCharacterLabel.");
            }

            if (menuPanel == null)
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers menuPanel.");
            }

            if (setupPanel == null)
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers setupPanel.");
            }

            if (noticePanel == null)
            {
                Debug.LogWarning("[UI] MainMenuScreen sans reference vers noticePanel.");
            }

            ShowMenu();
        }

        public void ShowMenu()
        {
            SetPanelActive(menuPanel, true);
            SetPanelActive(setupPanel, false);
            ClearNotice();
        }

        public void ShowSetupPlaceholder()
        {
            SetPanelActive(menuPanel, false);
            SetPanelActive(setupPanel, true);
            ClearNotice();
        }

        /// <summary>
        /// Affiche le personnage selectionne : libelle lisible, modele d'apercu et teinte de son
        /// emplacement. La couche App est le seul ecrivain de ces elements ; l'ecran les presente, et
        /// le choix courant reste lisible en texte, jamais par la seule couleur.
        /// </summary>
        public void ShowCharacter(string displayName, GameObject previewPrefab, Color previewTint, CharacterOption selectedOption)
        {
            if (selectedCharacterLabel != null)
            {
                selectedCharacterLabel.text = displayName ?? string.Empty;
            }

            if (primaryCharacterButton != null)
            {
                primaryCharacterButton.interactable = selectedOption != CharacterOption.Primary;
            }

            if (secondaryCharacterButton != null)
            {
                secondaryCharacterButton.interactable = selectedOption != CharacterOption.Secondary;
            }

            if (characterPreview != null)
            {
                characterPreview.Show(previewPrefab, previewTint);
            }
        }

        public void ShowNotice(UserNotice notice)
        {
            SetPanelActive(noticePanel, true);

            if (noticeText != null)
            {
                noticeText.text = "[" + notice.Severity + "] " + notice.Message;
            }
        }

        public void ClearNotice()
        {
            SetPanelActive(noticePanel, false);

            if (noticeText != null)
            {
                noticeText.text = string.Empty;
            }
        }

        private void RaisePlayRequested()
        {
            PlayRequested?.Invoke();
        }

        private void RaiseQuitRequested()
        {
            QuitRequested?.Invoke();
        }

        private void RaiseBackRequested()
        {
            BackRequested?.Invoke();
        }

        private void RaisePrimaryCharacterRequested()
        {
            CharacterOptionRequested?.Invoke(CharacterOption.Primary);
        }

        private void RaiseSecondaryCharacterRequested()
        {
            CharacterOptionRequested?.Invoke(CharacterOption.Secondary);
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
    }
}
