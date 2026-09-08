namespace RoadRage.Features.Online
{
    /// <summary>
    /// Etat de la room hote (Story 2.2). Closed et Open sont les etats stables ; ServicesUnavailable
    /// et CreationFailed sont des echecs retentables, jamais bloquants pour une nouvelle tentative de
    /// creation.
    /// </summary>
    public enum LobbyRoomStatus
    {
        /// <summary>Aucune room ouverte : etat initial, et etat apres fermeture explicite par l'hote.</summary>
        Closed,

        /// <summary>Creation de lobby Steam en cours ; empeche une seconde creation concurrente.</summary>
        Creating,

        /// <summary>Room privee ouverte : le code de join (JoinCode) est valide.</summary>
        Open,

        /// <summary>Creation refusee : les services en ligne Steam ne sont pas dans l'etat Online.</summary>
        ServicesUnavailable,

        /// <summary>La creation du lobby Steam (ou du transport Networking Sockets) a echoue.</summary>
        CreationFailed
    }
}
