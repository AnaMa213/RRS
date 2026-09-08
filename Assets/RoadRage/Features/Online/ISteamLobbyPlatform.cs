using System.Threading.Tasks;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Abstraction fine de la creation de lobby Steam (Story 2.2). Seul FacepunchSteamLobbyPlatform en
    /// connait l'implementation reelle : LobbyRoomService ne depend jamais directement du SDK, ce qui
    /// le rend testable sans client Steam installe.
    /// </summary>
    public interface ISteamLobbyPlatform
    {
        Task<LobbyCreateOutcome> CreateLobbyAsync(int maxMembers);

        /// <summary>Tente de rejoindre un lobby Steam existant par son identifiant (Story 2.3).</summary>
        Task<LobbyJoinOutcome> JoinLobbyAsync(ulong lobbyId);

        void LeaveCurrentLobby();
    }

    /// <summary>Resultat d'une tentative de creation de lobby, sans exposer de type Steamworks.</summary>
    public readonly struct LobbyCreateOutcome
    {
        public static readonly LobbyCreateOutcome Failed = new LobbyCreateOutcome(false, 0);

        public LobbyCreateOutcome(bool success, ulong lobbyId)
        {
            Success = success;
            LobbyId = lobbyId;
        }

        public bool Success { get; }

        public ulong LobbyId { get; }
    }

    /// <summary>Raison d'echec d'une tentative de join, sans exposer de type Steamworks (Story 2.3).</summary>
    public enum LobbyJoinFailureReason
    {
        None,
        Full,
        Expired,
        Failed
    }

    /// <summary>Resultat d'une tentative de join de lobby, sans exposer de type Steamworks (Story 2.3).</summary>
    public readonly struct LobbyJoinOutcome
    {
        public static readonly LobbyJoinOutcome Failed = new LobbyJoinOutcome(false, LobbyJoinFailureReason.Failed);

        public LobbyJoinOutcome(bool success, LobbyJoinFailureReason reason)
        {
            Success = success;
            Reason = reason;
        }

        public bool Success { get; }

        public LobbyJoinFailureReason Reason { get; }
    }
}
