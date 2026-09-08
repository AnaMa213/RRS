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

        public event Action PlayRequested;

        public event Action QuitRequested;

        public event Action BackRequested;

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

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
    }
}
