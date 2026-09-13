using Netcode.Transports.Facepunch;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Shared.Presentation;
using Unity.Netcode;
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
        /// Fichier de profil persistant (Story 4.5) : identite Steam et choix cosmetique uniquement.
        /// Porte par cet objet persistant pour que le menu ecrive et le run lise le meme chemin.
        /// </summary>
        public PlayerProfileFileStore ProfileFiles { get; private set; }

        /// <summary>
        /// Frontiere d'identite Steam locale (Story 4.5), portee par la meme instance que
        /// OnlineServices : un seul acces au SDK Steam pour le statut et pour l'identite.
        /// </summary>
        public ISteamIdentitySource SteamIdentity { get; private set; }

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
        /// Service unique de join par code (Story 2.3). Partage la meme instance de plateforme Steam que
        /// LobbyRoom : un joueur est soit hote, soit invite, jamais les deux, et LeaveCurrentLobby doit
        /// pouvoir quitter le lobby quel que soit le chemin (creation ou join) par lequel il y est entre.
        /// </summary>
        public LobbyJoinService LobbyJoin { get; private set; }

        /// <summary>
        /// Service unique de synchronisation du roster de lobby (Story 2.4) : membres connectes, etats
        /// prets et difficulte partagee. Partage la meme instance de plateforme Steam que LobbyRoom et
        /// LobbyJoin. Tick() est pompe ici a chaque frame, comme OnlineServices.
        /// </summary>
        public LobbyRosterService LobbyRoster { get; private set; }

        /// <summary>
        /// Depot host-only ClientId -> profil declare, alimente par l'approbation de connexion
        /// reseau et lu par le spawner de MVP_Run (Story 2.5).
        /// </summary>
        public NetworkPlayerRegistry NetworkPlayers { get; private set; }

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
            ProfileFiles = new PlayerProfileFileStore(PlayerProfileFileStore.DefaultFilePath);
            var steamPlatform = new FacepunchSteamPlatform();
            OnlineServices = new OnlineServicesBootstrapService(steamPlatform, SteamTestAppId);
            SteamIdentity = steamPlatform;
            var lobbyPlatform = new FacepunchSteamLobbyPlatform();
            LobbyRoom = new LobbyRoomService(lobbyPlatform, OnlineServices);
            LobbyJoin = new LobbyJoinService(lobbyPlatform, OnlineServices);
            LobbyRoster = new LobbyRosterService(lobbyPlatform, LobbyRoom, LobbyJoin);
            NetworkPlayers = new NetworkPlayerRegistry();
        }

        /// <summary>
        /// Garantit l'existence du NetworkManager avec le transport Steamworks Networking Sockets
        /// (Story 2.5), construit en code comme le reste des services de ce bootstrap : aucune scene ni
        /// prefab a authorer, meme AppID de test que OnlineServices. Volontairement paresseux (appele
        /// par LobbyFlowController juste avant StartHost/StartClient, jamais depuis Awake) : le
        /// transport Facepunch pompe Steamworks a chaque frame des qu'il existe (OnEarlyUpdate), y
        /// compris hors session reseau, ce qui casserait le solo et toute scene/test sans client Steam
        /// si le NetworkManager existait en permanence. NetworkManager gere sa propre survie au
        /// changement de scene une fois cree (DontDestroyOnLoad interne a son Awake).
        /// </summary>
        public static void EnsureNetworkManager()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null)
            {
                var networkManagerObject = new GameObject("RoadRageNetworkManager");
                networkManagerObject.AddComponent<FacepunchTransport>();
                manager = networkManagerObject.AddComponent<NetworkManager>();
                if (NetworkManager.Singleton == null)
                {
                    manager.SetSingleton();
                }
            }

            ConfigureNetworkManager(manager);
        }

        private static void ConfigureNetworkManager(NetworkManager manager)
        {
            if (manager.NetworkConfig == null)
            {
                manager.NetworkConfig = new NetworkConfig();
            }

            var transport = manager.NetworkConfig.NetworkTransport as FacepunchTransport;
            if (transport == null)
            {
                transport = manager.GetComponent<FacepunchTransport>();
                if (transport == null)
                {
                    transport = manager.gameObject.AddComponent<FacepunchTransport>();
                }
            }

            manager.NetworkConfig.NetworkTransport = transport;
            manager.NetworkConfig.ConnectionApproval = true;
            manager.NetworkConfig.EnableSceneManagement = true;
            manager.NetworkConfig.PlayerPrefab = null;
            if (manager.NetworkConfig.Prefabs == null)
            {
                manager.NetworkConfig.Prefabs = new NetworkPrefabs();
            }

            var playerRootPrefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            if (playerRootPrefab != null)
            {
                if (!manager.NetworkConfig.Prefabs.Contains(playerRootPrefab))
                {
                    manager.AddNetworkPrefab(playerRootPrefab);
                }
            }
            else
            {
                Debug.LogError("[App] Prefab NetworkedPlayerRoot introuvable sous Resources : le spawn reseau (Story 2.5) echouera.");
            }
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

            if (LobbyRoster != null)
            {
                LobbyRoster.Tick();
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
