namespace RoadRage.Features.Online
{
    /// <summary>
    /// Etat du join par code cote joueur (Story 2.3). Idle est uniquement l'etat initial, avant toute
    /// tentative ; un code invalide releve de son propre etat (InvalidCode), jamais d'un retour a Idle.
    /// Tous les etats d'echec sont retentables avec un nouveau code ou une nouvelle tentative.
    /// </summary>
    public enum LobbyJoinStatus
    {
        /// <summary>Etat initial, avant toute tentative de join.</summary>
        Idle,

        /// <summary>Tentative de join Steam en cours ; empeche une seconde tentative concurrente.</summary>
        Joining,

        /// <summary>Le lobby a ete rejoint avec succes : le joueur apparait dans le roster de lobby.</summary>
        Joined,

        /// <summary>Code vide, non numerique, ou contenant des caracteres invalides : aucun appel Steam tente.</summary>
        InvalidCode,

        /// <summary>Join refuse : les services en ligne Steam ne sont pas dans l'etat Online.</summary>
        ServicesUnavailable,

        /// <summary>Le lobby cible a deja atteint son plafond de quatre joueurs.</summary>
        RoomFull,

        /// <summary>Le lobby cible n'existe plus (ferme par l'hote, ou session expiree).</summary>
        SessionExpired,

        /// <summary>Le join Steam (ou le transport Networking Sockets) a echoue pour une autre raison.</summary>
        JoinFailed
    }
}
