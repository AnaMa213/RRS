using RoadRage.Shared.Definitions;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Profil joueur de session (Story 1.3). Objet de donnees local pur, meme forme que l'objet de
    /// reglages de partie du feature Lobby : pas de MonoBehaviour, pas de ScriptableObject, pas de
    /// NetworkVariable. Les valeurs de session runtime ne vivent jamais dans des assets
    /// ScriptableObject (epic-1-context).
    /// Concu pour etre mappable sans duplication de verite vers NetworkedPlayerState (Epic 2) :
    /// meme nom affiche, meme id de personnage stable.
    /// </summary>
    public sealed class PlayerProfile
    {
        public PlayerProfile(string displayName, DefinitionId characterId)
        {
            DisplayName = displayName ?? string.Empty;
            CharacterId = characterId;
        }

        /// <summary>Nom affiche deja normalise par la couche App ; jamais du texte brut de saisie.</summary>
        public string DisplayName { get; }

        public DefinitionId CharacterId { get; }
    }
}
