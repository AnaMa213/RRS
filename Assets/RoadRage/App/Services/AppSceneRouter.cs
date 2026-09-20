using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.App.Services
{
#if UNITY_EDITOR
    // App scenes are exclusive at runtime. Preserve the multi-scene editing workspace,
    // but enter Play Mode through one scene, just like the build's scene router.
    [UnityEditor.InitializeOnLoad]
    internal static class AppPlayModeEntry
    {
        private const string OverrideKey = "RoadRage.AppPlayModeEntry";

        static AppPlayModeEntry()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredEditMode
                && UnityEditor.SessionState.GetBool(OverrideKey, false))
            {
                UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = null;
                UnityEditor.SessionState.EraseBool(OverrideKey);
            }

            if (state != UnityEditor.PlayModeStateChange.ExitingEditMode
                || UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene != null
                || SceneManager.sceneCount < 2)
            {
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (scene.name != AppSceneRouter.BootstrapSceneName
                && scene.name != AppSceneRouter.MainMenuLobbySceneName
                && scene.name != AppSceneRouter.MvpRunSceneName)
            {
                return;
            }

            var entry = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scene.path);
            if (entry != null)
            {
                UnityEditor.SessionState.SetBool(OverrideKey, true);
                UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = entry;
            }
        }
    }
#endif

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

        public void LoadMvpRun()
        {
            Debug.Log("[App] Routage vers la scene " + MvpRunSceneName);
            SceneManager.LoadScene(MvpRunSceneName);
        }
    }
}
