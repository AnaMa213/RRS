using System;
using System.Threading.Tasks;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Service unique de creation/fermeture de la room hote (Story 2.2). Objet C# pur, sans dependance
    /// Unity ni etat de gameplay : cree un lobby Steam prive de 4 joueurs max via ISteamLobbyPlatform,
    /// expose le code de join (identifiant du lobby) une fois la room ouverte, et ne mute jamais le
    /// moindre etat de gameplay reseau partage.
    /// </summary>
    public sealed class LobbyRoomService
    {
        public const int MaxMembers = 4;

        private readonly ISteamLobbyPlatform platform;

        private readonly OnlineServicesBootstrapService onlineServices;

        public LobbyRoomService(ISteamLobbyPlatform platform, OnlineServicesBootstrapService onlineServices)
        {
            if (platform == null)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            if (onlineServices == null)
            {
                throw new ArgumentNullException(nameof(onlineServices));
            }

            this.platform = platform;
            this.onlineServices = onlineServices;
            Status = LobbyRoomStatus.Closed;
        }

        public LobbyRoomStatus Status { get; private set; }

        public ulong JoinCode { get; private set; }

        public event Action<LobbyRoomStatus> StatusChanged;

        /// <summary>
        /// Cree une room privee hote de 4 joueurs max. Refuse silencieusement (republie l'etat courant)
        /// si une creation est deja en cours ou qu'une room est deja ouverte : un double-clic ne doit
        /// jamais declencher une seconde creation concurrente.
        /// </summary>
        public async Task CreateRoomAsync()
        {
            if (Status == LobbyRoomStatus.Open || Status == LobbyRoomStatus.Creating)
            {
                RaiseStatusChanged(Status);
                return;
            }

            if (onlineServices.Status != OnlineServicesStatus.Online)
            {
                SetStatus(LobbyRoomStatus.ServicesUnavailable);
                return;
            }

            SetStatus(LobbyRoomStatus.Creating);

            LobbyCreateOutcome outcome;
            try
            {
                outcome = await platform.CreateLobbyAsync(MaxMembers);
            }
            catch (Exception)
            {
                outcome = LobbyCreateOutcome.Failed;
            }

            if (!outcome.Success)
            {
                SetStatus(LobbyRoomStatus.CreationFailed);
                return;
            }

            JoinCode = outcome.LobbyId;
            SetStatus(LobbyRoomStatus.Open);
        }

        /// <summary>Ferme la room ouverte par l'hote. Sans effet si aucune room n'est ouverte.</summary>
        public void CloseRoom()
        {
            if (Status != LobbyRoomStatus.Open)
            {
                return;
            }

            platform.LeaveCurrentLobby();
            JoinCode = 0;
            SetStatus(LobbyRoomStatus.Closed);
        }

        private void SetStatus(LobbyRoomStatus status)
        {
            Status = status;
            RaiseStatusChanged(status);
        }

        private void RaiseStatusChanged(LobbyRoomStatus status)
        {
            var handler = StatusChanged;
            if (handler != null)
            {
                handler(status);
            }
        }
    }
}
