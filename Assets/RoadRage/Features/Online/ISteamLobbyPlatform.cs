using System.Threading.Tasks;
using RoadRage.Shared.Domain;

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

        /// <summary>Instantane du roster du lobby courant (Story 2.4) : membres, etats prets et difficulte partagee. Vide si aucun lobby actif.</summary>
        LobbyRosterSnapshot GetRosterSnapshot();

        /// <summary>Publie l'etat pret du membre local dans les donnees de membre du lobby courant (Story 2.4). Sans effet hors lobby actif.</summary>
        void SetLocalMemberReady(bool ready);

        /// <summary>Publie la difficulte choisie dans les donnees du lobby courant (Story 2.4). Sans effet hors lobby actif ; reserve a l'hote par l'appelant.</summary>
        void SetLobbyDifficulty(Difficulty difficulty);
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

    /// <summary>Membre du lobby courant avec son etat pret, sans exposer de type Steamworks (Story 2.4).</summary>
    public readonly struct LobbyMemberSnapshot
    {
        public LobbyMemberSnapshot(ulong steamId, string displayName, bool ready)
        {
            SteamId = steamId;
            DisplayName = displayName;
            Ready = ready;
        }

        public ulong SteamId { get; }

        public string DisplayName { get; }

        public bool Ready { get; }
    }

    /// <summary>Instantane complet du roster et des reglages partages du lobby courant (Story 2.4).</summary>
    public readonly struct LobbyRosterSnapshot
    {
        public static readonly LobbyRosterSnapshot Empty = new LobbyRosterSnapshot(false, 0, Difficulty.Normal, System.Array.Empty<LobbyMemberSnapshot>());

        public LobbyRosterSnapshot(bool hasLobby, ulong ownerId, Difficulty difficulty, LobbyMemberSnapshot[] members)
        {
            HasLobby = hasLobby;
            OwnerId = ownerId;
            Difficulty = difficulty;
            Members = members;
        }

        public bool HasLobby { get; }

        public ulong OwnerId { get; }

        public Difficulty Difficulty { get; }

        public LobbyMemberSnapshot[] Members { get; }
    }
}
