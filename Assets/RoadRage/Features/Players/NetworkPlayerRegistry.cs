using System.Collections.Generic;

namespace RoadRage.Features.Players
{
    /// <summary>Profil decode d'un client reseau au moment de l'approbation de connexion (Story 2.5).</summary>
    public readonly struct NetworkPlayerProfile
    {
        public NetworkPlayerProfile(string displayName, string characterId)
        {
            DisplayName = displayName ?? string.Empty;
            CharacterId = characterId ?? string.Empty;
        }

        public string DisplayName { get; }

        public string CharacterId { get; }
    }

    /// <summary>
    /// Depot host-only ClientId -> profil declare (nom + personnage), reconstruit a chaque connexion
    /// via NetworkPlayerConnectionPayload (Story 2.5). Objet C# pur porte par l'objet persistant du
    /// bootstrap : le callback d'approbation de connexion de LobbyFlowController l'alimente pendant
    /// que la scene est encore MainMenuLobby, le spawner reseau de MVP_Run le lit apres chargement.
    /// </summary>
    public sealed class NetworkPlayerRegistry
    {
        private readonly Dictionary<ulong, NetworkPlayerProfile> profiles = new Dictionary<ulong, NetworkPlayerProfile>();

        public void Register(ulong clientId, NetworkPlayerProfile profile)
        {
            profiles[clientId] = profile;
        }

        public bool TryGet(ulong clientId, out NetworkPlayerProfile profile)
        {
            return profiles.TryGetValue(clientId, out profile);
        }

        public void Unregister(ulong clientId)
        {
            profiles.Remove(clientId);
        }

        public void Clear()
        {
            profiles.Clear();
        }
    }
}
