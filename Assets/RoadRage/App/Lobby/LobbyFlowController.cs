using Netcode.Transports.Facepunch;
using RoadRage.App.Services;
using RoadRage.Features.Lobby;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Definitions;
using RoadRage.Shared.Domain;
using RoadRage.Shared.Presentation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.App.Lobby
{
    /// <summary>
    /// Seule couture entre les ecrans UI du flux de lobby et la couche App : relie les intentions de
    /// LobbyShellScreen (pre-room : creation/join/personnage/jeu solo) et LobbyRosterScreen (post-room :
    /// roster, pret, difficulte, Start Game, fermeture) a MatchSettings (feature Lobby), publie les
    /// retours "indisponible" via le canal de notices existant, declenche l'initialisation des services
    /// en ligne Steam a l'ouverture du flux de lobby (Story 2.1), pilote la creation/fermeture de la room
    /// hote (Story 2.2), le join par code d'un lobby existant (Story 2.3), et la synchronisation du
    /// roster/pret/personnage/difficulte (Story 2.4). Bascule LobbyShellScreen/LobbyRosterScreen des
    /// qu'une room devient active ou se ferme.
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

        public const string NetworkStartFailedMessage = "Start Game refuse : impossible de demarrer la session reseau (Steamworks Networking Sockets).";

        [SerializeField]
        private LobbyShellScreen screen;

        [SerializeField]
        private LobbyRosterScreen lobbyRosterScreen;

        [SerializeField]
        private CharacterCatalog catalog;

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

            if (bootstrap != null && bootstrap.Profiles != null)
            {
                bootstrap.Profiles.ProfileChanged += HandleProfileChanged;
            }
            else
            {
                Debug.LogWarning("[App] LobbyFlowController sans depot de profil disponible.");
            }

            if (catalog == null)
            {
                Debug.LogWarning("[App] LobbyFlowController sans reference vers CharacterCatalog : le roster affichera des silhouettes neutres.");
            }

            if (lobbyRosterScreen != null)
            {
                lobbyRosterScreen.ReadyToggleRequested += HandleReadyToggleRequested;
                lobbyRosterScreen.DifficultyChanged += HandleDifficultyChanged;
                lobbyRosterScreen.SoloTestExceptionToggleRequested += HandleSoloTestExceptionToggleRequested;
                lobbyRosterScreen.StartGameRequested += HandleStartGameRequested;
                lobbyRosterScreen.CloseRoomRequested += HandleCloseRoomRequested;

                lobbyRosterScreen.ShowSettingsSummary(Settings.Difficulty);
                lobbyRosterScreen.ShowReadyState(false);
                lobbyRosterScreen.ShowSoloTestException(false);
                lobbyRosterScreen.ShowRoster(null);
                lobbyRosterScreen.Hide();
            }
            else
            {
                Debug.LogWarning("[App] LobbyFlowController sans reference vers LobbyRosterScreen.");
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
                screen.Hide();

                if (lobbyRosterScreen != null)
                {
                    lobbyRosterScreen.ShowRoomCode(lobbyRoom.JoinCode.ToString());
                    lobbyRosterScreen.Show();
                }
            }
            else
            {
                screen.ShowRoomClosed();
                screen.Show();
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

            if (bootstrap != null && bootstrap.Profiles != null)
            {
                bootstrap.Profiles.ProfileChanged -= HandleProfileChanged;
            }

            if (lobbyRosterScreen != null)
            {
                lobbyRosterScreen.ReadyToggleRequested -= HandleReadyToggleRequested;
                lobbyRosterScreen.DifficultyChanged -= HandleDifficultyChanged;
                lobbyRosterScreen.SoloTestExceptionToggleRequested -= HandleSoloTestExceptionToggleRequested;
                lobbyRosterScreen.StartGameRequested -= HandleStartGameRequested;
                lobbyRosterScreen.CloseRoomRequested -= HandleCloseRoomRequested;
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
        /// deja en cours. En pratique ce bouton devient inatteignable des que la room est ouverte (le
        /// panneau qui le porte est masque au profit de LobbyRosterScreen) : la fermeture se fait alors
        /// via HandleCloseRoomRequested, qui appelle le meme LobbyRoomService.CloseRoom().
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
        /// joueur ayant rejoint par code, Start Game reste local au seul profil joueur. Le meme handler
        /// sert le bouton solo de LobbyShellScreen et le bouton in-room de LobbyRosterScreen.
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

                StartNetworkedRun(true);
                return;
            }

            if (lobbyJoin != null && lobbyJoin.Status == LobbyJoinStatus.Joined)
            {
                StartNetworkedRun(false);
                return;
            }

            Debug.Log("[Lobby] Start Game demande : entree locale dans MVP_Run.");
            bootstrap.Router.LoadMvpRun();
        }

        /// <summary>
        /// Demarre reellement la session reseau (Story 2.5) avant d'entrer dans MVP_Run : publie le
        /// profil local (nom + personnage) comme donnees de connexion NGO (lues par
        /// HandleConnectionApproval, hote inclus), cible l'hote du lobby cote invite via son SteamId
        /// deja connu du roster (Story 2.4), puis demarre StartHost()/StartClient(). Le chargement de
        /// MVP_Run passe par NetworkManager.SceneManager cote hote pour rester synchronise avec les
        /// clients deja connectes ; un client ne charge jamais la scene lui-meme, il la recoit de
        /// l'hote une fois connecte.
        /// </summary>
        private void StartNetworkedRun(bool asHost)
        {
            RoadRageBootstrap.EnsureNetworkManager();

            var manager = NetworkManager.Singleton;
            if (manager == null)
            {
                Debug.LogError("[Lobby] Start Game refuse : NetworkManager indisponible.");
                PublishUnavailable(NetworkStartFailedMessage);
                return;
            }

            if (manager.IsListening)
            {
                Debug.LogWarning("[Lobby] Start Game ignore : session reseau deja demarree.");
                return;
            }

            var networkConfig = manager.NetworkConfig;
            if (networkConfig == null)
            {
                Debug.LogError("[Lobby] Start Game refuse : NetworkManager sans NetworkConfig.");
                PublishUnavailable(NetworkStartFailedMessage);
                return;
            }

            var profile = bootstrap.Profiles.Current;
            if (profile == null || profile.CharacterId.IsEmpty)
            {
                Debug.LogWarning("[Lobby] Start Game refuse : profil joueur incomplet.");
                PublishUnavailable(MissingProfileStartGameMessage);
                return;
            }

            manager.ConnectionApprovalCallback = HandleConnectionApproval;
            networkConfig.ConnectionData = NetworkPlayerConnectionPayload.Encode(profile.DisplayName, profile.CharacterId.Value);

            if (asHost)
            {
                Debug.Log("[Lobby] Start Game demande : demarrage hote reseau vers MVP_Run.");
                if (!manager.StartHost())
                {
                    Debug.LogError("[Lobby] Echec StartHost().");
                    PublishUnavailable(NetworkStartFailedMessage);
                    return;
                }

                if (manager.SceneManager == null)
                {
                    Debug.LogError("[Lobby] Start Game refuse : SceneManager reseau indisponible apres StartHost().");
                    manager.Shutdown();
                    PublishUnavailable(NetworkStartFailedMessage);
                    return;
                }

                manager.SceneManager.LoadScene(AppSceneRouter.MvpRunSceneName, LoadSceneMode.Single);
                return;
            }

            var transport = networkConfig.NetworkTransport as FacepunchTransport;
            if (transport == null || lobbyRoster == null || lobbyRoster.Current.OwnerId == 0)
            {
                Debug.LogError("[Lobby] Start Game refuse : impossible de determiner l'hote reseau a rejoindre.");
                PublishUnavailable(NetworkStartFailedMessage);
                return;
            }

            transport.targetSteamId = lobbyRoster.Current.OwnerId;

            Debug.Log("[Lobby] Start Game demande : connexion reseau client vers MVP_Run.");
            if (!manager.StartClient())
            {
                Debug.LogError("[Lobby] Echec StartClient().");
                PublishUnavailable(NetworkStartFailedMessage);
            }
        }

        /// <summary>
        /// Decode le profil (nom + personnage) transporte par NetworkConfig.ConnectionData (Story 2.5)
        /// et l'enregistre pour ce ClientId, hote inclus : NetworkedPlayerSpawnService (MVP_Run) le lit
        /// au spawn reseau. Approuve toujours : le plafond de quatre joueurs et la validite de la room
        /// sont deja appliques en amont par le lobby Steam (Story 2.2/2.3).
        /// </summary>
        private void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string displayName;
            string characterId;
            NetworkPlayerConnectionPayload.TryDecode(request.Payload, out displayName, out characterId);

            if (bootstrap != null && bootstrap.NetworkPlayers != null)
            {
                bootstrap.NetworkPlayers.Register(request.ClientNetworkId, new NetworkPlayerProfile(displayName, characterId));
            }

            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        private void HandleDifficultyChanged(Difficulty difficulty)
        {
            Settings.Difficulty = difficulty;

            if (screen != null)
            {
                screen.ShowSettingsSummary(Settings.Difficulty);
            }

            if (lobbyRosterScreen != null)
            {
                lobbyRosterScreen.ShowSettingsSummary(Settings.Difficulty);
            }

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

            if (lobbyRosterScreen != null)
            {
                lobbyRosterScreen.ShowReadyState(next);
            }
        }

        private void HandleSoloTestExceptionToggleRequested()
        {
            soloTestExceptionEnabled = !soloTestExceptionEnabled;

            if (lobbyRosterScreen != null)
            {
                lobbyRosterScreen.ShowSoloTestException(soloTestExceptionEnabled);
            }
        }

        /// <summary>Fermeture de room depuis l'ecran de lobby. Sans effet cote invite : aucune room a fermer.</summary>
        private void HandleCloseRoomRequested()
        {
            if (lobbyRoom == null || lobbyRoom.Status != LobbyRoomStatus.Open)
            {
                return;
            }

            Debug.Log("[Lobby] Close Room demande depuis l'ecran de lobby.");
            lobbyRoom.CloseRoom();
        }

        /// <summary>Republie le profil local (nom + personnage) des qu'il change, y compris deja en lobby.</summary>
        private void HandleProfileChanged(PlayerProfile profile)
        {
            PublishLocalProfile();
        }

        /// <summary>Sans effet hors lobby actif (voir LobbyRosterService.PublishLocalProfile) : ne fait rien en solo.</summary>
        private void PublishLocalProfile()
        {
            if (lobbyRoster == null)
            {
                return;
            }

            var profile = bootstrap != null && bootstrap.Profiles != null ? bootstrap.Profiles.Current : null;
            var displayName = profile != null ? profile.DisplayName : string.Empty;
            var characterId = profile != null ? profile.CharacterId.Value : string.Empty;
            lobbyRoster.PublishLocalProfile(displayName, characterId);
        }

        /// <summary>
        /// Republie le roster resolu (personnage + nom + pret) pour l'ecran, et applique la difficulte
        /// recue de l'hote au joueur ayant rejoint par code (Story 2.4) : jamais applique cote hote,
        /// source de verite de sa propre difficulte, pour eviter qu'une lecture perimee de ses propres
        /// donnees ne l'ecrase.
        /// </summary>
        private void HandleRosterChanged(LobbyRosterSnapshot snapshot)
        {
            if (lobbyRosterScreen != null)
            {
                lobbyRosterScreen.ShowRoster(BuildRosterEntries(snapshot));
            }

            var isJoinedClient = lobbyJoin != null && lobbyJoin.Status == LobbyJoinStatus.Joined;
            if (isJoinedClient && snapshot.HasLobby && snapshot.Difficulty != Settings.Difficulty)
            {
                Settings.Difficulty = snapshot.Difficulty;

                if (screen != null)
                {
                    screen.ShowSettingsSummary(Settings.Difficulty);
                }

                if (lobbyRosterScreen != null)
                {
                    lobbyRosterScreen.ShowSettingsSummary(Settings.Difficulty);
                }
            }
        }

        /// <summary>
        /// Resout chaque membre du roster en entree d'affichage : teinte du personnage choisi via le
        /// catalogue (silhouette neutre si le personnage est inconnu ou pas encore publie), nom deja
        /// resolu par la plateforme (nom RoadRage publie ou repli sur le nom Steam).
        /// </summary>
        private LobbyRosterEntry[] BuildRosterEntries(LobbyRosterSnapshot snapshot)
        {
            if (!snapshot.HasLobby || snapshot.Members.Length == 0)
            {
                return System.Array.Empty<LobbyRosterEntry>();
            }

            var entries = new LobbyRosterEntry[snapshot.Members.Length];
            for (var i = 0; i < snapshot.Members.Length; i++)
            {
                var member = snapshot.Members[i];
                var tint = Color.gray;

                if (catalog != null && !string.IsNullOrEmpty(member.CharacterId))
                {
                    CharacterDef character;
                    if (catalog.TryGetById(new DefinitionId(member.CharacterId), out character))
                    {
                        tint = character.PreviewTint;
                    }
                }

                entries[i] = new LobbyRosterEntry(member.DisplayName, tint, member.Ready);
            }

            return entries;
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
        /// Traduit chaque changement d'etat de la room hote en retour visible, et bascule l'ecran actif
        /// entre LobbyShellScreen (pre-room) et LobbyRosterScreen (post-room) sur Open/Closed.
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
                        screen.Hide();
                    }

                    if (lobbyRosterScreen != null)
                    {
                        lobbyRosterScreen.ShowRoomCode(lobbyRoom.JoinCode.ToString());
                        lobbyRosterScreen.SetDifficultyEditable(true);
                        lobbyRosterScreen.SetCloseRoomVisible(true);
                        lobbyRosterScreen.ShowReadyState(false);
                        lobbyRosterScreen.Show();
                    }

                    if (lobbyRoster != null)
                    {
                        lobbyRoster.PublishDifficulty(Settings.Difficulty);
                    }

                    PublishLocalProfile();

                    Publish(UserNoticeSeverity.Info, RoomOpenMessage);
                    break;
                case LobbyRoomStatus.Closed:
                    if (screen != null)
                    {
                        screen.ShowRoomClosed();
                        screen.Show();
                    }

                    if (lobbyRosterScreen != null)
                    {
                        lobbyRosterScreen.Hide();
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
        /// Traduit chaque changement d'etat du join par code en retour visible, et bascule vers
        /// LobbyRosterScreen sur Joined (Story 2.4). Idle et Joining ne produisent aucun retour dedie :
        /// Idle est l'etat de repos, Joining n'est ni un succes ni un echec definitif tant que la
        /// tentative Steam n'est pas resolue.
        /// </summary>
        private void HandleJoinStatusChanged(LobbyJoinStatus status)
        {
            switch (status)
            {
                case LobbyJoinStatus.Joined:
                    if (screen != null)
                    {
                        screen.ShowJoinedRoom(lobbyJoin.JoinedLobbyId.ToString());
                        screen.Hide();
                    }

                    if (lobbyRosterScreen != null)
                    {
                        lobbyRosterScreen.ShowRoomCode(lobbyJoin.JoinedLobbyId.ToString());
                        lobbyRosterScreen.SetDifficultyEditable(false);
                        lobbyRosterScreen.SetCloseRoomVisible(false);
                        lobbyRosterScreen.ShowReadyState(false);
                        lobbyRosterScreen.Show();
                    }

                    PublishLocalProfile();

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
