namespace RoadRage.Features.Online
{
    /// <summary>
    /// Etat des services en ligne Steam (Story 2.1). Les quatre etats terminaux sont ceux que
    /// l'UI de lobby doit distinguer : succes, echec d'initialisation, echec de connexion Steam,
    /// et services indisponibles sur cette machine.
    /// </summary>
    public enum OnlineServicesStatus
    {
        /// <summary>Aucune tentative d'initialisation n'a encore ete faite.</summary>
        NotStarted,

        /// <summary>Steamworks initialise et session Steam active.</summary>
        Online,

        /// <summary>L'initialisation Steamworks a echoue (client Steam non lance, AppID invalide, etc.).</summary>
        InitializationFailed,

        /// <summary>Steamworks initialise mais aucune session Steam connectee n'a ete trouvee.</summary>
        SignInFailed,

        /// <summary>Les services en ligne sont indisponibles sur cette machine (runtime Steam absent).</summary>
        Offline
    }
}
