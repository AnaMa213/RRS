using RoadRage.App.Services;
using RoadRage.Features.Players;
using RoadRage.Shared.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.App
{
    /// <summary>
    /// Singleton persistant proprietaire des services de l'application (routage de scenes, notices).
    /// Survit au changement de scene et route vers MainMenuLobby au demarrage depuis Bootstrap.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoadRageBootstrap : MonoBehaviour
    {
        private static RoadRageBootstrap instance;

        public static RoadRageBootstrap Instance
        {
            get { return instance; }
        }

        public AppSceneRouter Router { get; private set; }

        public UserNoticeChannel Notices { get; private set; }

        /// <summary>
        /// Depot de session du profil joueur (Story 1.3). Porte par cet objet persistant : c'est le
        /// seul point de passage vers l'entree monde de la Story 1.5, qui le lit apres changement de scene.
        /// </summary>
        public PlayerProfileStore Profiles { get; private set; }

        /// <summary>
        /// Garantit l'existence de l'instance persistante. Depuis Bootstrap rien n'est cree ;
        /// en entree directe (ex. MainMenuLobby jouee seule dans l'Editor) l'instance nait a la volee.
        /// </summary>
        public static RoadRageBootstrap EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            var bootstrapObject = new GameObject(nameof(RoadRageBootstrap));
            return bootstrapObject.AddComponent<RoadRageBootstrap>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[App] Seconde instance de RoadRageBootstrap detectee, destruction de la seconde ; la premiere reste la reference.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Router = new AppSceneRouter();
            Notices = new UserNoticeChannel();
            Profiles = new PlayerProfileStore();
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name == AppSceneRouter.BootstrapSceneName)
            {
                Router.LoadMainMenu();
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
