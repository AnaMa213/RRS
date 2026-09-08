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
    /// l'ouverture du flux de lobby (Story 2.1), pilote la creation/fermeture de la room hote
    /// (Story 2.2), et le join par code d'un lobby existant (Story 2.3).
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

        public const string JoinSucceededMessage = "Room rejointe : tu es maintenant dans le lobby.";

        public const string JoinInvalidCodeMessage = "Code de join invalide : verifie le code et reessaie.";

        public const string JoinServicesUnavailableMessage = "Join impossible : les services en ligne Steam ne sont pas prets.";

        public const string JoinRoomFullMessage = "Join impossible : cette room a deja atteint son plafond de quatre joueurs.";

        public const string JoinSessionExpiredMessage = "Join impossible : cette room n'existe plus (fermee ou expiree).";

        public const string JoinFailedMessage = "Join impossible : echec du service Steam ou de Networking Sockets.";

        public const string JoinRefusedHostingMessage = "Join impossible : ferme d'abord ta room hote avant de rejoindre une autre partie.";

        public const string CreateRefusedAlreadyJoinedMessage = "Creation impossible : tu as deja rejoint une room, quitte-la avant d'en creer une nouvelle.";

        public const string StartRefusedServicesMessage = "Start Game refuse : les services en ligne Steam ne sont pas prets.";

        public const string StartRefusedRosterNotSyncedMessage = "Start Game refuse : le roster de la room n'est pas encore synchronise.";

        public const string StartRefusedNotAllReadyMessage = "Start Game refuse : tous les joueurs connectes doivent etre prets (ou active le test solo).";

        [SerializeField]
        private LobbyShellScreen screen;

        private RoadRageBootstrap bootstrap;

        private OnlineServicesBootstrapService onlineServices;

        private LobbyRoomService lobbyRoom;

        private LobbyJoinService lobbyJoin;

        private LobbyRosterService lobbyRoster;

        private bool soloTestExceptionEnabled;

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

            if (bootstrap != null && bootstrap.LobbyJoin != null)
            {
                lobbyJoin = bootstrap.LobbyJoin;
                lobbyJoin.StatusChanged += HandleJoinStatusChanged;
            }
            else
            {
                Debug.LogWarning("[App] LobbyFlowController sans service de join disponible.");
            }

            if (bootstrap != null && bootstrap.LobbyRoster != null)
            {
                lobbyRoster = bootstrap.LobbyRoster;
                lobbyRoster.RosterChanged += HandleRosterChanged;
            }
            else
            {
                Debug.LogWarning("[App] LobbyFlowController sans service de roster disponible.");
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
            screen.ReadyToggleRequested += HandleReadyToggleRequested;
            screen.SoloTestExceptionToggleRequested += HandleSoloTestExceptionToggleRequested;

            screen.ShowSettingsSummary(Settings.Difficulty);
            screen.ShowReadyState(false);
            screen.ShowSoloTestException(false);
            screen.ShowRoster(BuildRosterText(LobbyRosterSnapshot.Empty));

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

            if (lobbyJoin != null)
            {
                lobbyJoin.StatusChanged -= HandleJoinStatusChanged;
            }

            if (lobbyRoster != null)
            {
                lobbyRoster.RosterChanged -= HandleRosterChanged;
            }

            if (screen == null)
            {
                return;
            }

            screen.CreateLobbyRequested -= HandleCreateLobbyRequested;
            screen.JoinByCodeRequested -= HandleJoinByCodeRequested;
            screen.StartGameRequested -= HandleStartGameRequested;
            screen.DifficultyChanged -= HandleDifficultyChanged;
            screen.ReadyToggleRequested -= HandleReadyToggleRequested;
            screen.SoloTestExceptionToggleRequested -= HandleSoloTestExceptionToggleRequested;
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

            if (lobbyJoin != null && lobbyJoin.Status == LobbyJoinStatus.Joined)
            {
                Debug.LogWarning("[Lobby] Create Lobby refuse : un lobby rejoint par code est deja actif.");
                PublishUnavailable(CreateRefusedAlreadyJoinedMessage);
                return;
            }

            if (lobbyRoom.Status == LobbyRoomStatus.Open)
            {
                Debug.Log("[Lobby] Close Room demande.");
                lobbyRoom.CloseRoom();
                return;
            }

            Debug.Log("[Lobby] Create Lobby demande : creation d'une room Steam privee.");
            await lobbyRoom.CreateRoomAsync();
        }

        /// <summary>
        /// Rejoint le lobby designe par le code saisi (Story 2.3). LobbyJoinService normalise, valide et
        /// refuse silencieusement un double-clic pendant qu'une tentative est deja en cours ; ce
        /// controller ne fait que relayer la demande et traduire le resultat en retour visible.
        /// </summary>
        private async void HandleJoinByCodeRequested(string rawCode)
        {
            if (bootstrap == null || lobbyJoin == null)
            {
                Debug.LogWarning("[Lobby] Join By Code demande sans service de join disponible.");
                PublishUnavailable(JoinFailedMessage);
                return;
            }

            if (lobbyRoom != null && lobbyRoom.Status == LobbyRoomStatus.Open)
            {
                Debug.LogWarning("[Lobby] Join By Code refuse : une room hote est deja ouverte.");
                PublishUnavailable(JoinRefusedHostingMessage);
                return;
            }

            Debug.Log("[Lobby] Join By Code demande.");
            await lobbyJoin.JoinByCodeAsync(rawCode);
        }

        /// <summary>
        /// Le gate roster/pret/settings (Story 2.4) ne s'applique que quand une room hote est ouverte :
        /// hors lobby (jeu solo local, comportement inchange depuis la Story 1.2/1.5) ou en tant que
        /// joueur ayant rejoint par code, Start Game reste local au seul profil joueur.
        /// </summary>
        private void HandleStartGameRequested()
        {
            if (bootstrap == null || bootstrap.Profiles == null || !bootstrap.Profiles.HasProfile)
            {
                Debug.LogWarning("[Lobby] Start Game refuse : aucun profil joueur confirme.");
                PublishUnavailable(MissingProfileStartGameMessage);
                return;
            }

            if (lobbyRoom != null && lobbyRoom.Status == LobbyRoomStatus.Open)
            {
                if (onlineServices == null || onlineServices.Status != OnlineServicesStatus.Online)
                {
                    Debug.LogWarning("[Lobby] Start Game refuse : services en ligne indisponibles.");
                    PublishUnavailable(StartRefusedServicesMessage);
                    return;
                }

                if (lobbyRoster == null || !lobbyRoster.IsSynchronized)
                {
                    Debug.LogWarning("[Lobby] Start Game refuse : roster non synchronise.");
                    PublishUnavailable(StartRefusedRosterNotSyncedMessage);
                    return;
                }

                if (!lobbyRoster.AllMembersReady && !soloTestExceptionEnabled)
                {
                    Debug.LogWarning("[Lobby] Start Game refuse : tous les joueurs ne sont pas prets.");
                    PublishUnavailable(StartRefusedNotAllReadyMessage);
                    return;
                }
            }

            Debug.Log("[Lobby] Start Game demande : entree locale dans MVP_Run.");
            bootstrap.Router.LoadMvpRun();
        }

        private void HandleDifficultyChanged(Difficulty difficulty)
        {
            Settings.Difficulty = difficulty;
            screen.ShowSettingsSummary(Settings.Difficulty);

            if (lobbyRoom != null && lobbyRoom.Status == LobbyRoomStatus.Open && lobbyRoster != null)
            {
                lobbyRoster.PublishDifficulty(difficulty);
            }
        }

        private void HandleReadyToggleRequested()
        {
            if (lobbyRoster == null)
            {
                return;
            }

            var next = !lobbyRoster.LocalReady;
            lobbyRoster.SetLocalReady(next);
            screen.ShowReadyState(next);
        }

        private void HandleSoloTestExceptionToggleRequested()
        {
            soloTestExceptionEnabled = !soloTestExceptionEnabled;
            screen.ShowSoloTestException(soloTestExceptionEnabled);
        }

        /// <summary>
        /// Republie le roster forme en texte pour l'ecran, et applique la difficulte recue de l'hote au
        /// joueur ayant rejoint par code (Story 2.4) : jamais applique cote hote, source de verite de sa
        /// propre difficulte, pour eviter qu'une lecture perimee de ses propres donnees ne l'ecrase.
        /// </summary>
        private void HandleRosterChanged(LobbyRosterSnapshot snapshot)
        {
            if (screen != null)
            {
                screen.ShowRoster(BuildRosterText(snapshot));
            }

            var isJoinedClient = lobbyJoin != null && lobbyJoin.Status == LobbyJoinStatus.Joined;
            if (isJoinedClient && snapshot.HasLobby && snapshot.Difficulty != Settings.Difficulty)
            {
                Settings.Difficulty = snapshot.Difficulty;
                if (screen != null)
                {
                    screen.ShowSettingsSummary(Settings.Difficulty);
                }
            }
        }

        private static string BuildRosterText(LobbyRosterSnapshot snapshot)
        {
            if (!snapshot.HasLobby || snapshot.Members.Length == 0)
            {
                return "Joueurs : solo";
            }

            var builder = new System.Text.StringBuilder();
            builder.Append("Joueurs (").Append(snapshot.Members.Length).Append("/4)");

            foreach (var member in snapshot.Members)
            {
                builder.Append('\n').Append(member.DisplayName).Append(" - ").Append(member.Ready ? "Pret" : "En attente");
            }

            return builder.ToString();
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
                        screen.SetDifficultyEditable(true);
                        screen.ShowReadyState(false);
                    }

                    if (lobbyRoster != null)
                    {
                        lobbyRoster.PublishDifficulty(Settings.Difficulty);
                    }

                    Publish(UserNoticeSeverity.Info, RoomOpenMessage);
                    break;
                case LobbyRoomStatus.Closed:
                    if (screen != null)
                    {
                        screen.ShowRoomClosed();
                        screen.SetDifficultyEditable(true);
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
        /// Traduit chaque changement d'etat du join par code en retour visible. Idle et Joining ne
        /// produisent aucun retour dedie : Idle est l'etat de repos, Joining n'est ni un succes ni un
        /// echec definitif tant que la tentative Steam n'est pas resolue.
        /// </summary>
        private void HandleJoinStatusChanged(LobbyJoinStatus status)
        {
            switch (status)
            {
                case LobbyJoinStatus.Joined:
                    if (screen != null)
                    {
                        screen.ShowJoinedRoom(lobbyJoin.JoinedLobbyId.ToString());
                        screen.SetDifficultyEditable(false);
                        screen.ShowReadyState(false);
                    }

                    Publish(UserNoticeSeverity.Info, JoinSucceededMessage);
                    break;
                case LobbyJoinStatus.InvalidCode:
                    Publish(UserNoticeSeverity.Warning, JoinInvalidCodeMessage);
                    break;
                case LobbyJoinStatus.ServicesUnavailable:
                    Publish(UserNoticeSeverity.Warning, JoinServicesUnavailableMessage);
                    break;
                case LobbyJoinStatus.RoomFull:
                    Publish(UserNoticeSeverity.Warning, JoinRoomFullMessage);
                    break;
                case LobbyJoinStatus.SessionExpired:
                    Publish(UserNoticeSeverity.Warning, JoinSessionExpiredMessage);
                    break;
                case LobbyJoinStatus.JoinFailed:
                    Publish(UserNoticeSeverity.Error, JoinFailedMessage);
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
