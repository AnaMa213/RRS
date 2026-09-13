using RoadRage.Features.UI;
using UnityEngine;

namespace RoadRage.App.MainMenu
{
    /// <summary>
    /// Seule couture entre l'ecran UI du menu principal et la couche App : relie les intentions
    /// de l'ecran au routeur de scenes, au canal de notices et au quit (avec garde Editor).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuFlowController : MonoBehaviour
    {
        [SerializeField]
        private MainMenuScreen screen;

        private RoadRageBootstrap bootstrap;

        private void Awake()
        {
            bootstrap = RoadRageBootstrap.EnsureInstance();

            if (screen == null)
            {
                Debug.LogWarning("[App] MainMenuFlowController sans reference vers MainMenuScreen.");
                return;
            }

            screen.PlayRequested += EnterLobbyShell;
            screen.BackRequested += ReturnToMenu;
            screen.QuitRequested += QuitApplication;

            if (bootstrap != null && bootstrap.Notices != null)
            {
                bootstrap.Notices.NoticePublished += screen.ShowNotice;

                if (bootstrap.Notices.LastNotice.HasValue)
                {
                    screen.ShowNotice(bootstrap.Notices.LastNotice.Value);
                }
            }
        }

        private void OnDestroy()
        {
            if (screen == null)
            {
                return;
            }

            screen.PlayRequested -= EnterLobbyShell;
            screen.BackRequested -= ReturnToMenu;
            screen.QuitRequested -= QuitApplication;

            if (bootstrap != null && bootstrap.Notices != null)
            {
                bootstrap.Notices.NoticePublished -= screen.ShowNotice;
            }
        }

        private void QuitApplication()
        {
#if UNITY_EDITOR
            Debug.Log("[App] Quit demande en Play Mode Editor : aucune fermeture d'Editor declenchee.");
#else
            Debug.Log("[App] Quit demande, fermeture de l'application.");
            Application.Quit();
#endif
        }

        private void ReturnToMenu()
        {
            SetProfileSelectionFrozen(false);
            screen.ShowMenu();

            if (bootstrap != null && bootstrap.Notices != null && bootstrap.Notices.LastNotice.HasValue)
            {
                screen.ShowNotice(bootstrap.Notices.LastNotice.Value);
            }
        }

        /// <summary>
        /// Entree de la coquille lobby en solo (Story 1.1) : la selection de personnage y est gelee
        /// (Story 4.6), avant toute publication, et ne se rouvre qu'au retour au menu.
        /// </summary>
        private void EnterLobbyShell()
        {
            SetProfileSelectionFrozen(true);
            screen.ShowSetupPlaceholder();
        }

        /// <summary>
        /// Ouvre et ferme la fenetre de selection de personnage (Story 4.6) : gelee a l'entree du
        /// lobby, leve a la (re)ouverture du menu, seule surface de selection. Sans ce degel, le menu
        /// resterait en lecture seule apres une premiere session.
        /// </summary>
        private void SetProfileSelectionFrozen(bool frozen)
        {
            if (bootstrap == null || bootstrap.Profiles == null)
            {
                Debug.LogWarning("[App] Gel de la selection de personnage impossible : depot de profil indisponible.");
                return;
            }

            if (frozen)
            {
                bootstrap.Profiles.Freeze();
            }
            else
            {
                bootstrap.Profiles.Unfreeze();
            }
        }
    }
}
