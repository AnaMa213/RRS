using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.App.Services
{
    /// <summary>
    /// Point de verite unique du routage entre les scenes de build activees.
    /// Les noms exposes ici correspondent aux scenes activees dans EditorBuildSettings.
    /// </summary>
    public sealed class AppSceneRouter
    {
        public const string BootstrapSceneName = "Bootstrap";

        public const string MainMenuLobbySceneName = "MainMenuLobby";

        public const string MvpRunSceneName = "MVP_Run";

        public void LoadMainMenu()
        {
            Debug.Log("[App] Routage vers la scene " + MainMenuLobbySceneName);
            SceneManager.LoadScene(MainMenuLobbySceneName);
        }
    }
}
