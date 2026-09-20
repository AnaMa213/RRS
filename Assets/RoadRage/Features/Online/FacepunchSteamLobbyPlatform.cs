using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using RoadRage.Shared.Domain;
using Steamworks;
using Steamworks.Data;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Seule classe du projet a toucher directement Steamworks.SteamMatchmaking pour la creation et le
    /// join de room (Story 2.2, Story 2.3), ainsi que pour le roster/donnees de lobby (Story 2.4).
    /// CreateLobbyAsync cree le lobby en Public (Story 5.3) -- SteamMatchmaking.CreateLobbyAsync le
    /// cree Private par defaut, mais un lobby Private est exclu de RequestLobbyList (aucun moyen de le
    /// retrouver par recherche, meme avec le bon code) ; aucune ouverture de port ni connexion directe
    /// cote hote dans les deux cas, Networking Sockets restant le chemin reseau. JoinLobbyAsync
    /// rejoint un lobby existant par son identifiant et traduit la reponse Steamworks (Lobby.Join ->
    /// RoomEnter) en raison d'echec neutre vis-a-vis du SDK. GetRosterSnapshot/SetLocalMemberReady/
    /// SetLobbyDifficulty lisent et ecrivent les donnees de lobby et de membre Steam (repliquees par
    /// Steam a tous les membres), sans callback statique : LobbyRosterService les interroge par polling
    /// (Tick), comme OnlineServicesBootstrapService le fait deja pour les callbacks Steamworks generaux.
    /// LobbyRoomService, LobbyJoinService et LobbyRosterService portent toute la logique d'etat, ce qui
    /// les rend testables sans client Steam installe.
    /// </summary>
    public sealed class FacepunchSteamLobbyPlatform : ISteamLobbyPlatform
    {
        private const string ReadyDataKey = "ready";

        private const string ReadyValue = "1";

        private const string NotReadyValue = "0";

        private const string DifficultyDataKey = "difficulty";

        private const string AiVehicleTargetCountDataKey = "aiVehicleTargetCount";

        private const string LitterThrowerCountDataKey = "litterThrowerCount";

        private const string DisplayNameDataKey = "displayName";

        private const string CharacterIdDataKey = "characterId";

        private const string RunLaunchRequestedDataKey = "runLaunchRequested";

        private const string JoinCodeDataKey = "joinCode";

        private Lobby? currentLobby;

        public async Task<LobbyCreateOutcome> CreateLobbyAsync(int maxMembers)
        {
            var lobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
            if (!lobby.HasValue)
            {
                return LobbyCreateOutcome.Failed;
            }

            // Story 5.3 : SteamMatchmaking.CreateLobbyAsync cree un lobby Private par defaut, or
            // RequestLobbyList (JoinLobbyByCodeAsync) n'y a structurellement jamais acces -- un lobby
            // Private n'est joignable que par son identifiant direct (invite), jamais par recherche.
            // Passe en Public pour que le code court partage reste le seul moyen pratique de le
            // retrouver (filtre WithKeyValue), sans dependre d'une relation d'amis Steam entre les
            // joueurs.
            lobby.Value.SetPublic();

            currentLobby = lobby;
            return new LobbyCreateOutcome(true, lobby.Value.Id);
        }

        public async Task<LobbyJoinOutcome> JoinLobbyAsync(ulong lobbyId)
        {
            var lobby = new Lobby(lobbyId);
            var enter = await lobby.Join();

            if (enter != RoomEnter.Success)
            {
                return new LobbyJoinOutcome(false, MapFailureReason(enter));
            }

            currentLobby = lobby;
            return new LobbyJoinOutcome(true, LobbyJoinFailureReason.None, lobbyId);
        }

        public async Task<LobbyJoinOutcome> JoinLobbyByCodeAsync(string joinCode)
        {
            var lobbies = await SteamMatchmaking.LobbyList
                .FilterDistanceWorldwide()
                .WithKeyValue(JoinCodeDataKey, joinCode)
                .WithMaxResults(1)
                .RequestAsync();

            if (lobbies == null || lobbies.Length == 0)
            {
                return new LobbyJoinOutcome(false, LobbyJoinFailureReason.Expired);
            }

            var lobby = lobbies[0];
            var enter = await lobby.Join();
            if (enter != RoomEnter.Success)
            {
                return new LobbyJoinOutcome(false, MapFailureReason(enter));
            }

            currentLobby = lobby;
            return new LobbyJoinOutcome(true, LobbyJoinFailureReason.None, lobby.Id);
        }

        public void SetLobbyJoinCode(string joinCode)
        {
            if (currentLobby.HasValue)
            {
                currentLobby.Value.SetData(JoinCodeDataKey, joinCode ?? string.Empty);
            }
        }

        public void LeaveCurrentLobby()
        {
            if (!currentLobby.HasValue)
            {
                return;
            }

            currentLobby.Value.Leave();
            currentLobby = null;
        }

        public LobbyRosterSnapshot GetRosterSnapshot()
        {
            if (!currentLobby.HasValue)
            {
                return LobbyRosterSnapshot.Empty;
            }

            var lobby = currentLobby.Value;
            var difficulty = ParseDifficulty(lobby.GetData(DifficultyDataKey));
            var runLaunchRequested = lobby.GetData(RunLaunchRequestedDataKey) == ReadyValue;

            // Story 5.16 : meme forme que la difficulte -- une valeur absente ou illisible reste
            // "non publiee" (-1), jamais une valeur de repli inventee ici.
            var aiVehicleTargetCount = ParseTrafficValue(lobby.GetData(AiVehicleTargetCountDataKey));
            var litterThrowerCount = ParseTrafficValue(lobby.GetData(LitterThrowerCountDataKey));

            var members = new List<LobbyMemberSnapshot>();
            foreach (var member in lobby.Members)
            {
                var ready = lobby.GetMemberData(member, ReadyDataKey) == ReadyValue;
                var publishedName = lobby.GetMemberData(member, DisplayNameDataKey);
                var displayName = string.IsNullOrEmpty(publishedName) ? member.Name : publishedName;
                var characterId = lobby.GetMemberData(member, CharacterIdDataKey);
                members.Add(new LobbyMemberSnapshot(member.Id, displayName, characterId, ready));
            }

            return new LobbyRosterSnapshot(true, lobby.Owner.Id, difficulty, members.ToArray(), runLaunchRequested, aiVehicleTargetCount, litterThrowerCount);
        }

        public void SetLocalMemberReady(bool ready)
        {
            if (!currentLobby.HasValue)
            {
                return;
            }

            currentLobby.Value.SetMemberData(ReadyDataKey, ready ? ReadyValue : NotReadyValue);
        }

        public void SetLocalMemberProfile(string displayName, string characterId)
        {
            if (!currentLobby.HasValue)
            {
                return;
            }

            currentLobby.Value.SetMemberData(DisplayNameDataKey, displayName ?? string.Empty);
            currentLobby.Value.SetMemberData(CharacterIdDataKey, characterId ?? string.Empty);
        }

        public void SetLobbyDifficulty(Difficulty difficulty)
        {
            if (!currentLobby.HasValue)
            {
                return;
            }

            currentLobby.Value.SetData(DifficultyDataKey, ((int)difficulty).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Story 5.16 : deux cles de lobby distinctes, meme porteur que la difficulte.</summary>
        public void SetLobbyTrafficSettings(int aiVehicleTargetCount, int litterThrowerCount)
        {
            if (!currentLobby.HasValue)
            {
                return;
            }

            currentLobby.Value.SetData(AiVehicleTargetCountDataKey, FormatTrafficValue(aiVehicleTargetCount));
            currentLobby.Value.SetData(LitterThrowerCountDataKey, FormatTrafficValue(litterThrowerCount));
        }

        public void SetLobbyRunLaunchRequested(bool launchRequested)
        {
            if (!currentLobby.HasValue)
            {
                return;
            }

            currentLobby.Value.SetData(RunLaunchRequestedDataKey, launchRequested ? ReadyValue : NotReadyValue);
        }

        private static Difficulty ParseDifficulty(string raw)
        {
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && System.Enum.IsDefined(typeof(Difficulty), value))
            {
                return (Difficulty)value;
            }

            return Difficulty.Normal;
        }

        /// <summary>
        /// Story 5.16 : valeur de trafic publiee, ou <see cref="SessionTrafficValue.Unresolved"/> quand la
        /// cle est absente ou illisible. Aucune borne n'est inventee ici : le clamp appartient au
        /// TrafficSettingsDef.
        /// </summary>
        private static string FormatTrafficValue(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static int ParseTrafficValue(string raw)
        {
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value >= 0)
            {
                return value;
            }

            return SessionTrafficValue.Unresolved;
        }

        private static LobbyJoinFailureReason MapFailureReason(RoomEnter enter)
        {
            switch (enter)
            {
                case RoomEnter.Full:
                    return LobbyJoinFailureReason.Full;
                case RoomEnter.DoesntExist:
                    return LobbyJoinFailureReason.Expired;
                default:
                    return LobbyJoinFailureReason.Failed;
            }
        }
    }
}
