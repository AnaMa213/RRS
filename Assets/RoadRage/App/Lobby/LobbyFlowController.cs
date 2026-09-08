using RoadRage.Features.Lobby;
using RoadRage.Features.Online;
using RoadRage.Features.UI;
using RoadRage.Shared.Domain;
using RoadRage.Shared.Presentation;
using UnityEngine;

namespace RoadRage.App.Lobby
{
    /// <summary>
    /// Seule couture entre l'ecran UI de la coquille de lobby et la couche App : relie les intentions
    /// de LobbyShellScreen a MatchSettings (feature Lobby), publie les retours "indisponible" via
    /// le canal de notices existant, declenche l'initialisation des services en ligne Steam a
    /// l'ouverture du flux de lobby (Story 2.1), et pilote la creation/fermeture de la room hote
    /// (Story 2.2). Join par code reseau toujours hors scope (Story 2.3+).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LobbyFlowController : MonoBehaviour
    {
        public const string MissingProfileStartGameMessage = "Start Game refuse : cree un profil joueur avant d'entrer dans le monde.";

        public const string OnlineServicesOnlineMessage = "Services en ligne Steam initialises : connexion active.";

        public const string OnlineServicesInitializationFailedMessage = "Initialisation Steam impossible : lance Steam et reessaie.";

        public const string OnlineServicesSignInFailedMessage = "Connexion Steam introuvable : connecte-toi a Steam avant de continuer.";

        public const string OnlineServicesOfflineMessage = "Services en ligne indisponibles sur cette machine : aucune fonctionnalite reseau.";

        public const string RoomOpenMessage = "Room privee creee : partage le code pour inviter jusqu'a 3 joueurs.";

        public const string RoomClosedMessage = "Room fermee.";

        public const string RoomServicesUnavailableMessage = "Creation de room impossible : les services en ligne Steam ne sont pas prets.";

        public const string RoomCreationFailedMessage = "Creation de room impossible : echec du service Steam ou de Networking Sockets.";

        [SerializeField]
        private LobbyShellScreen screen;

        private RoadRageBootstrap bootstrap;

        private OnlineServicesBootstrapService onlineServices;

        private LobbyRoomService lobbyRoom;

        public MatchSettings Settings { get; private set; }

        private void Awake()
        {
            bootstrap = RoadRageBootstrap.EnsureInstance();
            Settings = new MatchSettings();

            if (bootstrap != null && bootstrap.OnlineServices != null)
            {
                onlineServices = bootstrap.OnlineServices;
                onlineServices.StatusChanged += HandleOnlineServicesStatusChanged;
                onlineServices.TryInitialize();
            }
            else
            {
                Debug.LogWarning("[App] LobbyFlowController sans service de services en ligne disponible.");
            }

            if (bootstrap != null && bootstrap.LobbyRoom != null)
            {
                lobbyRoom = bootstrap.LobbyRoom;
                lobbyRoom.StatusChanged += HandleRoomStatusChanged;
            }
            else
            {
                Debug.LogWarning("[App] LobbyFlowController sans service de room disponible.");
            }

            if (screen == null)
            {
                Debug.LogWarning("[App] LobbyFlowController sans reference vers LobbyShellScreen.");
                return;
            }

            screen.CreateLobbyRequested += HandleCreateLobbyRequested;
            screen.JoinByCodeRequested += HandleJoinByCodeRequested;
            screen.StartGameRequested += HandleStartGameRequested;
            screen.DifficultyChanged += HandleDifficultyChanged;

            screen.ShowSettingsSummary(Settings.Difficulty);

            if (lobbyRoom != null && lobbyRoom.Status == LobbyRoomStatus.Open)
            {
                screen.ShowRoomCreated(lobbyRoom.JoinCode.ToString());
            }
            else
            {
                screen.ShowRoomClosed();
            }
        }

        private void OnDestroy()
        {
            if (onlineServices != null)
            {
                onlineServices.StatusChanged -= HandleOnlineServicesStatusChanged;
            }

            if (lobbyRoom != null)
            {
                lobbyRoom.StatusChanged -= HandleRoomStatusChanged;
            }

            if (screen == null)
            {
                return;
            }

            screen.CreateLobbyRequested -= HandleCreateLobbyRequested;
            screen.JoinByCodeRequested -= HandleJoinByCodeRequested;
            screen.StartGameRequested -= HandleStartGameRequested;
            screen.DifficultyChanged -= HandleDifficultyChanged;
        }

        /// <summary>
        /// Meme bouton pour creer et fermer la room (Story 2.2) : le libelle affiche par LobbyShellScreen
        /// distingue les deux etats, jamais un second bouton. Cliquer sur une room deja ouverte la ferme ;
        /// sinon tente une creation, refusee silencieusement par LobbyRoomService si une creation est
        /// deja en cours.
        /// </summary>
        private async void HandleCreateLobbyRequested()
        {
            if (bootstrap == null || lobbyRoom == null)
            {
                Debug.LogWarning("[Lobby] Create Lobby demande sans service de room disponible.");
                PublishUnavailable(RoomCreationFailedMessage);
                return;
            }

            if (lobbyRoom.Status == LobbyRoomStatus.Open)
            {
                lobbyRoom.CloseRoom();
                return;
            }

            await lobbyRoom.CreateRoomAsync();
        }

        private void HandleJoinByCodeRequested()
        {
            Debug.Log("[Lobby] Join By Code demande : placeholder, aucun join reseau en Epic 1.");
            PublishUnavailable("Join By Code indisponible : le join par code de session arrive en Epic 2.");
        }

        private void HandleStartGameRequested()
        {
            if (bootstrap == null || bootstrap.Profiles == null || !bootstrap.Profiles.HasProfile)
            {
                Debug.LogWarning("[Lobby] Start Game refuse : aucun profil joueur confirme.");
                PublishUnavailable(MissingProfileStartGameMessage);
                return;
            }

            Debug.Log("[Lobby] Start Game demande : entree locale dans MVP_Run.");
            bootstrap.Router.LoadMvpRun();
        }

        private void HandleDifficultyChanged(Difficulty difficulty)
        {
            Settings.Difficulty = difficulty;
            screen.ShowSettingsSummary(Settings.Difficulty);
        }

        /// <summary>
        /// Traduit chaque etat resolu des services en ligne Steam en notice visible avec la severite
        /// adaptee. NotStarted n'atteint jamais ce point : TryInitialize resout toujours vers un des
        /// quatre etats terminaux avant de lever cet evenement.
        /// </summary>
        private void HandleOnlineServicesStatusChanged(OnlineServicesStatus status)
        {
            switch (status)
            {
                case OnlineServicesStatus.Online:
                    Publish(UserNoticeSeverity.Info, OnlineServicesOnlineMessage);
                    break;
                case OnlineServicesStatus.SignInFailed:
                    Publish(UserNoticeSeverity.Error, OnlineServicesSignInFailedMessage);
                    break;
                case OnlineServicesStatus.Offline:
                    Publish(UserNoticeSeverity.Warning, OnlineServicesOfflineMessage);
                    break;
                case OnlineServicesStatus.InitializationFailed:
                    Publish(UserNoticeSeverity.Error, OnlineServicesInitializationFailedMessage);
                    break;
            }
        }

        /// <summary>
        /// Traduit chaque changement d'etat de la room hote en retour visible : le libelle du bouton et
        /// le code de join affiches par LobbyShellScreen, plus une notice pour les etats notables.
        /// Creating ne produit aucun retour dedie : la room n'est ni ouverte ni fermee, l'ecran garde
        /// son etat courant jusqu'a la resolution.
        /// </summary>
        private void HandleRoomStatusChanged(LobbyRoomStatus status)
        {
            switch (status)
            {
                case LobbyRoomStatus.Open:
                    if (screen != null)
                    {
                        screen.ShowRoomCreated(lobbyRoom.JoinCode.ToString());
                    }

                    Publish(UserNoticeSeverity.Info, RoomOpenMessage);
                    break;
                case LobbyRoomStatus.Closed:
                    if (screen != null)
                    {
                        screen.ShowRoomClosed();
                    }

                    Publish(UserNoticeSeverity.Info, RoomClosedMessage);
                    break;
                case LobbyRoomStatus.ServicesUnavailable:
                    if (screen != null)
                    {
                        screen.ShowRoomClosed();
                    }

                    Publish(UserNoticeSeverity.Warning, RoomServicesUnavailableMessage);
                    break;
                case LobbyRoomStatus.CreationFailed:
                    if (screen != null)
                    {
                        screen.ShowRoomClosed();
                    }

                    Publish(UserNoticeSeverity.Error, RoomCreationFailedMessage);
                    break;
            }
        }

        /// <summary>
        /// Publie un retour visible dedie a l'action refusee. Un message par action, avec sa cause reelle :
        /// le joueur doit pouvoir distinguer quelle action a ete refusee et pourquoi, jamais un refus generique.
        /// </summary>
        private void PublishUnavailable(string message)
        {
            Publish(UserNoticeSeverity.Warning, message);
        }

        private void Publish(UserNoticeSeverity severity, string message)
        {
            if (bootstrap != null && bootstrap.Notices != null)
            {
                bootstrap.Notices.Publish(new UserNotice(severity, message));
            }
            else
            {
                Debug.LogWarning("[App] LobbyFlowController sans canal de notices disponible.");
            }
        }
    }
}
