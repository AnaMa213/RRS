namespace RoadRage.Shared.Domain
{
    /// <summary>
    /// Reglage de difficulte partage entre l'UI de lobby et les reglages de partie.
    /// Mappable plus tard sur un etat reseau (NetworkedLobbyState) sans duplication de verite.
    /// </summary>
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }
}
