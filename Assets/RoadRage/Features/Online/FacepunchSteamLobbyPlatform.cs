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
    /// CreateLobbyAsync cree un lobby Steam prive (comportement par defaut du SDK, aucune ouverture de
    /// port ni connexion directe cote hote) avec Networking Sockets comme chemin reseau. JoinLobbyAsync
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

        private const string DisplayNameDataKey = "displayName";

        private const string CharacterIdDataKey = "characterId";

        private Lobby? currentLobby;

        public async Task<LobbyCreateOutcome> CreateLobbyAsync(int maxMembers)
        {
            var lobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
            if (!lobby.HasValue)
            {
                return LobbyCreateOutcome.Failed;
            }

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
            return new LobbyJoinOutcome(true, LobbyJoinFailureReason.None);
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

            var members = new List<LobbyMemberSnapshot>();
            foreach (var member in lobby.Members)
            {
                var ready = lobby.GetMemberData(member, ReadyDataKey) == ReadyValue;
                var publishedName = lobby.GetMemberData(member, DisplayNameDataKey);
                var displayName = string.IsNullOrEmpty(publishedName) ? member.Name : publishedName;
                var characterId = lobby.GetMemberData(member, CharacterIdDataKey);
                members.Add(new LobbyMemberSnapshot(member.Id, displayName, characterId, ready));
            }

            return new LobbyRosterSnapshot(true, lobby.Owner.Id, difficulty, members.ToArray());
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

        private static Difficulty ParseDifficulty(string raw)
        {
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && System.Enum.IsDefined(typeof(Difficulty), value))
            {
                return (Difficulty)value;
            }

            return Difficulty.Normal;
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
