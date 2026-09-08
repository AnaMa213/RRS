using RoadRage.App.Services;
using RoadRage.Features.Online;
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
        /// <summary>AppID de test Steamworks (480, Spacewar) : meme AppID que le smoke test de l'Epic 0, en attendant l'AppID reel du jeu.</summary>
        private const uint SteamTestAppId = 480;

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
        /// Service unique d'initialisation des services en ligne Steam (Story 2.1). Construit ici,
        /// declenche par LobbyFlowController a l'ouverture du flux de lobby.
        /// </summary>
        public OnlineServicesBootstrapService OnlineServices { get; private set; }

        /// <summary>
        /// Service unique de creation/fermeture de la room hote (Story 2.2). Construit ici pour
        /// survivre au changement de scene ; declenche par LobbyFlowController sur demande du joueur.
        /// </summary>
        public LobbyRoomService LobbyRoom { get; private set; }

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
            OnlineServices = new OnlineServicesBootstrapService(new FacepunchSteamPlatform(), SteamTestAppId);
            LobbyRoom = new LobbyRoomService(new FacepunchSteamLobbyPlatform(), OnlineServices);
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name == AppSceneRouter.BootstrapSceneName)
            {
                Router.LoadMainMenu();
            }
        }

        private void Update()
        {
            if (OnlineServices != null)
            {
                OnlineServices.Tick();
            }
        }

        private void OnDestroy()
        {
            if (LobbyRoom != null)
            {
                LobbyRoom.CloseRoom();
            }

            if (OnlineServices != null)
            {
                OnlineServices.Shutdown();
            }

            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
