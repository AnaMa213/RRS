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
}
