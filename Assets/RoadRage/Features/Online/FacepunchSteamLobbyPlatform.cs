using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Seule classe du projet a toucher directement Steamworks.SteamMatchmaking pour la creation et le
    /// join de room (Story 2.2, Story 2.3). CreateLobbyAsync cree un lobby Steam prive (comportement par
    /// defaut du SDK, aucune ouverture de port ni connexion directe cote hote) avec Networking Sockets
    /// comme chemin reseau. JoinLobbyAsync rejoint un lobby existant par son identifiant et traduit la
    /// reponse Steamworks (Lobby.Join -> RoomEnter) en raison d'echec neutre vis-a-vis du SDK.
    /// LobbyRoomService et LobbyJoinService portent toute la logique d'etat, ce qui permet de les tester
    /// sans client Steam installe.
    /// </summary>
    public sealed class FacepunchSteamLobbyPlatform : ISteamLobbyPlatform
    {
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
