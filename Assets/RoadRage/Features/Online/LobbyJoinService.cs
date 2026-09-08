using System;
using System.Globalization;
using System.Threading.Tasks;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Service unique de join par code pour un joueur (Story 2.3). Objet C# pur, sans dependance Unity :
    /// normalise et valide le code saisi (trim, non vide, purement numerique) avant tout appel reseau,
    /// verifie que les services en ligne sont prets, puis rejoint le lobby Steam via une abstraction
    /// testable (ISteamLobbyPlatform), en distinguant code invalide, room pleine, session expiree et
    /// echec de service ou de Networking Sockets. Ne mute jamais le moindre etat de gameplay partage.
    /// </summary>
    public sealed class LobbyJoinService
    {
        private readonly ISteamLobbyPlatform platform;

        private readonly OnlineServicesBootstrapService onlineServices;

        public LobbyJoinService(ISteamLobbyPlatform platform, OnlineServicesBootstrapService onlineServices)
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
            Status = LobbyJoinStatus.Idle;
        }

        public LobbyJoinStatus Status { get; private set; }

        public ulong JoinedLobbyId { get; private set; }

        public event Action<LobbyJoinStatus> StatusChanged;

        /// <summary>
        /// Tente de rejoindre le lobby designe par ce code. Refuse silencieusement (republie l'etat
        /// courant) si une tentative est deja en cours ou qu'un lobby est deja rejoint : un double-clic
        /// ne doit jamais declencher un second join concurrent, et un join reussi ne doit jamais etre
        /// remplace par un second sans passer par un leave explicite (hors scope Story 2.3). Un code
        /// invalide est rejete avant tout appel reseau.
        /// </summary>
        public async Task JoinByCodeAsync(string rawCode)
        {
            if (Status == LobbyJoinStatus.Joining || Status == LobbyJoinStatus.Joined)
            {
                RaiseStatusChanged(Status);
                return;
            }

            if (!TryNormalizeCode(rawCode, out var lobbyId))
            {
                SetStatus(LobbyJoinStatus.InvalidCode);
                return;
            }

            if (onlineServices.Status != OnlineServicesStatus.Online)
            {
                SetStatus(LobbyJoinStatus.ServicesUnavailable);
                return;
            }

            SetStatus(LobbyJoinStatus.Joining);

            LobbyJoinOutcome outcome;
            try
            {
                outcome = await platform.JoinLobbyAsync(lobbyId);
            }
            catch (Exception)
            {
                outcome = LobbyJoinOutcome.Failed;
            }

            if (outcome.Success)
            {
                JoinedLobbyId = lobbyId;
                SetStatus(LobbyJoinStatus.Joined);
                return;
            }

            switch (outcome.Reason)
            {
                case LobbyJoinFailureReason.Full:
                    SetStatus(LobbyJoinStatus.RoomFull);
                    break;
                case LobbyJoinFailureReason.Expired:
                    SetStatus(LobbyJoinStatus.SessionExpired);
                    break;
                default:
                    SetStatus(LobbyJoinStatus.JoinFailed);
                    break;
            }
        }

        /// <summary>
        /// Nettoie (trim) et valide le code saisi : doit rester non vide apres trim et purement numerique.
        /// Rejette silencieusement tout caractere non numerique (espaces internes, lettres, ponctuation)
        /// plutot que de tenter un appel Steam voue a l'echec.
        /// </summary>
        private static bool TryNormalizeCode(string rawCode, out ulong lobbyId)
        {
            lobbyId = 0;

            if (string.IsNullOrWhiteSpace(rawCode))
            {
                return false;
            }

            var trimmed = rawCode.Trim();
            return ulong.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out lobbyId) && lobbyId != 0;
        }

        private void SetStatus(LobbyJoinStatus status)
        {
            Status = status;
            RaiseStatusChanged(status);
        }

        private void RaiseStatusChanged(LobbyJoinStatus status)
        {
            var handler = StatusChanged;
            if (handler != null)
            {
                handler(status);
            }
        }
    }
}
