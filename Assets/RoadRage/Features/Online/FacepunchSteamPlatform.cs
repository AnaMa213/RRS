using Steamworks;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Seule classe du projet a toucher directement le SDK Facepunch.Steamworks (Story 2.1).
    /// Enveloppe fine sans logique d'etat : OnlineServicesBootstrapService porte toute la logique
    /// de resolution de statut, ce qui permet de la tester sans client Steam installe.
    /// Implemente aussi ISteamIdentitySource (Story 4.5) : l'identite locale est une lecture du meme
    /// SDK, elle n'a pas besoin d'un second acces au client Steam. ISteamPlatform reste inchange pour
    /// ne pas casser les fakes de test qui l'implementent.
    /// </summary>
    public sealed class FacepunchSteamPlatform : ISteamPlatform, ISteamIdentitySource
    {
        public bool IsValid
        {
            get { return SteamClient.IsValid; }
        }

        public bool IsLoggedOn
        {
            get { return SteamClient.IsLoggedOn; }
        }

        public void Init(uint appId)
        {
            if (SteamClient.IsValid)
            {
                return;
            }

            SteamClient.Init(appId, false);
        }

        public void Shutdown()
        {
            SteamClient.Shutdown();
        }

        public void RunCallbacks()
        {
            if (SteamClient.IsValid)
            {
                SteamClient.RunCallbacks();
            }
        }

        /// <summary>
        /// Lit l'identite Steam locale (Story 4.5). Une session invalide se dit par un retour faux :
        /// le repli d'identite est une decision de la couche App, jamais de cette frontiere.
        /// </summary>
        public bool TryGetLocalIdentity(out string playerName, out ulong steamId)
        {
            playerName = string.Empty;
            steamId = 0UL;

            if (!SteamClient.IsValid)
            {
                return false;
            }

            var localSteamId = SteamClient.SteamId;
            if (!localSteamId.IsValid)
            {
                return false;
            }

            playerName = SteamClient.Name ?? string.Empty;
            steamId = localSteamId.Value;
            return true;
        }
    }
}
