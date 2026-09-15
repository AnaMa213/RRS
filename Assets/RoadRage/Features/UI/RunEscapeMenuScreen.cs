using System;
using UnityEngine;
using UnityEngine.UI;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// Ecran UGUI du menu d'echappement de MVP_Run (Story 5.8). Comme MainMenuScreen, il n'emet que
    /// des intentions : il ne route aucune scene, ne ferme jamais l'application et ne connait ni les
    /// reglages de partie ni l'etat reseau. La couche App decide du quit, du curseur et du blocage
    /// des entrees locales.
    ///
    /// Le composant vit sur le panneau lui-meme : ce panneau est donc son propre support d'affichage,
    /// et Awake() le masque une fois les boutons cables -- meme patron que MainMenuScreen.Awake, qui
    /// affiche explicitement son panneau initial.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunEscapeMenuScreen : MonoBehaviour
    {
        [SerializeField]
        private Button resumeButton;

        [SerializeField]
        private Button quitToMainMenuButton;

        /// <summary>Reprise de la run demandee (bouton Resume).</summary>
        public event Action ResumeRequested;

        /// <summary>Retour au menu principal demande : la couche App emprunte le teardown de session existant.</summary>
        public event Action QuitToMainMenuRequested;

        /// <summary>Vrai quand le panneau est affiche.</summary>
        public bool IsOpen
        {
            get { return gameObject.activeSelf; }
        }

        private void Awake()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(RaiseResumeRequested);
            }
            else
            {
                Debug.LogWarning("[UI] RunEscapeMenuScreen sans reference vers resumeButton.");
            }

            if (quitToMainMenuButton != null)
            {
                quitToMainMenuButton.onClick.AddListener(RaiseQuitToMainMenuRequested);
            }
            else
            {
                Debug.LogWarning("[UI] RunEscapeMenuScreen sans reference vers quitToMainMenuButton.");
            }

            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RaiseResumeRequested()
        {
            ResumeRequested?.Invoke();
        }

        private void RaiseQuitToMainMenuRequested()
        {
            QuitToMainMenuRequested?.Invoke();
        }
    }
}
