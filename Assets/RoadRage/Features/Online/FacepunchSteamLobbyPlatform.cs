using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Seule classe du projet a toucher directement Steamworks.SteamMatchmaking pour la creation de
    /// room (Story 2.2). CreateLobbyAsync cree un lobby Steam prive (comportement par defaut du SDK,
    /// aucune ouverture de port ni connexion directe cote hote) avec Networking Sockets comme chemin
    /// reseau. LobbyRoomService porte toute la logique d'etat, ce qui permet de la tester sans client
    /// Steam installe.
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

        public void LeaveCurrentLobby()
        {
            if (!currentLobby.HasValue)
            {
                return;
            }

            currentLobby.Value.Leave();
            currentLobby = null;
        }
    }
}
