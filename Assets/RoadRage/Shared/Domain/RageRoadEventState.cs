namespace RoadRage.Shared.Domain
{
    /// <summary>
    /// Cycle de vie d'un evenement Rage Road (AD-22), vocabulaire unique partage par Run, UI et tests
    /// -- comme <see cref="RunPhase"/> : un enum de domaine, sans logique et sans dependance.
    ///
    /// L'Epic 5 ne cable que <see cref="Idle"/> -> <see cref="Triggered"/> ; la suite du cycle
    /// (<see cref="Confrontation"/>, <see cref="Resolved"/>, <see cref="RewardGranted"/>) appartient a
    /// l'Epic 6 (resolution, confrontation, recompense). Les valeurs sont explicites parce qu'un
    /// decalage d'ordre changerait le sens de la table de transitions.
    /// </summary>
    public enum RageRoadEventState
    {
        /// <summary>Aucun evenement Rage Road : le creneau unique est libre (AD-16).</summary>
        Idle = 0,

        /// <summary>Evenement declenche par l'hote avec une cible identifiee ; visible par tous.</summary>
        Triggered = 1,

        /// <summary>Confrontation en cours (Epic 6).</summary>
        Confrontation = 2,

        /// <summary>Confrontation resolue (Epic 6).</summary>
        Resolved = 3,

        /// <summary>Recompense accordee (Epic 6, apres Resolved).</summary>
        RewardGranted = 4
    }
}
