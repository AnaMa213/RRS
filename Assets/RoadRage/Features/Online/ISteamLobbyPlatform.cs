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

        /// <summary>Publie le code court affichable sur le lobby courant.</summary>
        void SetLobbyJoinCode(string joinCode)
        {
        }

        /// <summary>Tente de rejoindre un lobby Steam existant par son identifiant (Story 2.3).</summary>
        Task<LobbyJoinOutcome> JoinLobbyAsync(ulong lobbyId);

        /// <summary>Resout puis rejoint un lobby par son code court affichable.</summary>
        Task<LobbyJoinOutcome> JoinLobbyByCodeAsync(string joinCode)
        {
            return Task.FromResult(LobbyJoinOutcome.Failed);
        }

        void LeaveCurrentLobby();

        /// <summary>Instantane du roster du lobby courant (Story 2.4) : membres, etats prets et difficulte partagee. Vide si aucun lobby actif.</summary>
        LobbyRosterSnapshot GetRosterSnapshot();

        /// <summary>Publie l'etat pret du membre local dans les donnees de membre du lobby courant (Story 2.4). Sans effet hors lobby actif.</summary>
        void SetLocalMemberReady(bool ready);

        /// <summary>Publie le nom affiche et l'id de personnage choisi du membre local (Story 2.4). Sans effet hors lobby actif.</summary>
        void SetLocalMemberProfile(string displayName, string characterId);

        /// <summary>Publie la difficulte choisie dans les donnees du lobby courant (Story 2.4). Sans effet hors lobby actif ; reserve a l'hote par l'appelant.</summary>
        void SetLobbyDifficulty(Difficulty difficulty);

        /// <summary>
        /// Publie les reglages de trafic choisis par l'hote dans les donnees du lobby courant
        /// (Story 5.16). Sans effet hors lobby actif ; reserve a l'hote par l'appelant. Corps par
        /// defaut vide pour que les doubles de test existants compilent sans modification.
        /// </summary>
        void SetLobbyTrafficSettings(int aiVehicleTargetCount, int litterThrowerCount)
        {
        }

        /// <summary>Publie le signal de lancement reseau dans les donnees du lobby courant. Sans effet hors lobby actif ; reserve a l'hote par l'appelant.</summary>
        void SetLobbyRunLaunchRequested(bool launchRequested);
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

        public LobbyJoinOutcome(bool success, LobbyJoinFailureReason reason, ulong lobbyId = 0)
        {
            Success = success;
            Reason = reason;
            LobbyId = lobbyId;
        }

        public bool Success { get; }

        public LobbyJoinFailureReason Reason { get; }

        public ulong LobbyId { get; }
    }

    /// <summary>Membre du lobby courant avec son etat pret, sans exposer de type Steamworks (Story 2.4).</summary>
    public readonly struct LobbyMemberSnapshot
    {
        public LobbyMemberSnapshot(ulong steamId, string displayName, string characterId, bool ready)
        {
            SteamId = steamId;
            DisplayName = displayName;
            CharacterId = characterId;
            Ready = ready;
        }

        public ulong SteamId { get; }

        /// <summary>Nom affiche RoadRage si publie par ce membre, sinon son nom Steam en repli.</summary>
        public string DisplayName { get; }

        /// <summary>Id brut du CharacterDef choisi par ce membre, vide si non publie.</summary>
        public string CharacterId { get; }

        public bool Ready { get; }
    }

    /// <summary>Instantane complet du roster et des reglages partages du lobby courant (Story 2.4).</summary>
    public readonly struct LobbyRosterSnapshot
    {
        public static readonly LobbyRosterSnapshot Empty = new LobbyRosterSnapshot(false, 0, Difficulty.Normal, System.Array.Empty<LobbyMemberSnapshot>());

        public LobbyRosterSnapshot(
            bool hasLobby,
            ulong ownerId,
            Difficulty difficulty,
            LobbyMemberSnapshot[] members,
            bool runLaunchRequested = false,
            int aiVehicleTargetCount = SessionTrafficValue.Unresolved,
            int litterThrowerCount = SessionTrafficValue.Unresolved)
        {
            HasLobby = hasLobby;
            OwnerId = ownerId;
            Difficulty = difficulty;
            Members = members;
            RunLaunchRequested = runLaunchRequested;
            AiVehicleTargetCount = aiVehicleTargetCount;
            LitterThrowerCount = litterThrowerCount;
        }

        public bool HasLobby { get; }

        public ulong OwnerId { get; }

        public Difficulty Difficulty { get; }

        public LobbyMemberSnapshot[] Members { get; }

        public bool RunLaunchRequested { get; }

        /// <summary>
        /// Effectif de vehicules IA publie par l'hote (Story 5.16), ou
        /// <see cref="SessionTrafficValue.Unresolved"/> si l'hote n'en a publie aucun. L'instantane ne
        /// porte que la valeur brute publiee : les bornes restent dans le Def.
        /// </summary>
        public int AiVehicleTargetCount { get; }

        /// <summary>Nombre de jeteurs publie par l'hote (Story 5.16), ou <see cref="SessionTrafficValue.Unresolved"/>.</summary>
        public int LitterThrowerCount { get; }
    }
}
