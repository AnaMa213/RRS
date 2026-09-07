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

            screen.PlayRequested += screen.ShowSetupPlaceholder;
            screen.BackRequested += screen.ShowMenu;
            screen.QuitRequested += QuitApplication;

            if (bootstrap != null && bootstrap.Notices != null)
            {
                bootstrap.Notices.NoticePublished += screen.ShowNotice;
            }
        }

        private void OnDestroy()
        {
            if (screen == null)
            {
                return;
            }

            screen.PlayRequested -= screen.ShowSetupPlaceholder;
            screen.BackRequested -= screen.ShowMenu;
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
    }
}
