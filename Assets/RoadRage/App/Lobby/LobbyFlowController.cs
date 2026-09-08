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
    /// le canal de notices existant, et declenche l'initialisation des services en ligne Steam a
    /// l'ouverture du flux de lobby (Story 2.1). Aucune vraie creation de lobby, aucun join reseau (Epic 2.2+).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LobbyFlowController : MonoBehaviour
    {
        public const string MissingProfileStartGameMessage = "Start Game refuse : cree un profil joueur avant d'entrer dans le monde.";

        public const string OnlineServicesOnlineMessage = "Services en ligne Steam initialises : connexion active.";

        public const string OnlineServicesInitializationFailedMessage = "Initialisation Steam impossible : lance Steam et reessaie.";

        public const string OnlineServicesSignInFailedMessage = "Connexion Steam introuvable : connecte-toi a Steam avant de continuer.";

        public const string OnlineServicesOfflineMessage = "Services en ligne indisponibles sur cette machine : aucune fonctionnalite reseau.";

        [SerializeField]
        private LobbyShellScreen screen;

        private RoadRageBootstrap bootstrap;

        private OnlineServicesBootstrapService onlineServices;

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
        }

        private void OnDestroy()
        {
            if (onlineServices != null)
            {
                onlineServices.StatusChanged -= HandleOnlineServicesStatusChanged;
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

        private void HandleCreateLobbyRequested()
        {
            Debug.Log("[Lobby] Create Lobby demande : aucune vraie session Steam en Epic 1.");
            PublishUnavailable("Create Lobby indisponible : la creation de room Steam arrive en Epic 2.");
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
