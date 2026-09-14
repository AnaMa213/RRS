using System;
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

        public string JoinedJoinCode { get; private set; } = string.Empty;

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

            if (!TryNormalizeCode(rawCode, out var joinCode))
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
                outcome = await platform.JoinLobbyByCodeAsync(joinCode);
            }
            catch (Exception)
            {
                outcome = LobbyJoinOutcome.Failed;
            }

            if (outcome.Success)
            {
                JoinedLobbyId = outcome.LobbyId;
                JoinedJoinCode = joinCode;
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
        /// Nettoie (trim), normalise en majuscules et valide le code partageable : exactement cinq
        /// caracteres alphanumeriques ASCII. Tout autre format est rejete avant l'appel Steam.
        /// </summary>
        private static bool TryNormalizeCode(string rawCode, out string joinCode)
        {
            joinCode = string.Empty;

            if (string.IsNullOrWhiteSpace(rawCode))
            {
                return false;
            }

            var trimmed = rawCode.Trim().ToUpperInvariant();
            if (trimmed.Length != 5)
            {
                return false;
            }

            for (var i = 0; i < trimmed.Length; i++)
            {
                var character = trimmed[i];
                if ((character < 'A' || character > 'Z') && (character < '0' || character > '9'))
                {
                    return false;
                }
            }

            joinCode = trimmed;
            return true;
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
