namespace RoadRage.Features.Online
{
    /// <summary>
    /// Frontiere dediee de l'identite Steam locale (Story 4.5). Volontairement separee de
    /// ISteamPlatform : ce dernier reste inchange pour ne pas casser les fakes de test de l'Epic 2
    /// qui l'implementent. Seul FacepunchSteamPlatform, qui connait le SDK, l'implemente.
    /// Aucun repli d'identite n'est prevu ici : une identite absente se dit par un retour faux.
    /// </summary>
    public interface ISteamIdentitySource
    {
        /// <summary>
        /// Lit l'identite Steam locale. Retourne faux quand aucune session Steam exploitable n'existe :
        /// l'appelant decide alors du repli, jamais cette frontiere.
        /// </summary>
        bool TryGetLocalIdentity(out string playerName, out ulong steamId);
    }
}
