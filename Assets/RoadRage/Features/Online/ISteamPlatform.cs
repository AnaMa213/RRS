namespace RoadRage.Features.Online
{
    /// <summary>
    /// Abstraction fine du SDK Steamworks (Story 2.1). Seul FacepunchSteamPlatform en connait
    /// l'implementation reelle : le reste du feature Online, et OnlineServicesBootstrapService en
    /// particulier, ne depend jamais directement du SDK, ce qui le rend testable sans client Steam.
    /// </summary>
    public interface ISteamPlatform
    {
        bool IsValid { get; }

        bool IsLoggedOn { get; }

        void Init(uint appId);

        void Shutdown();

        /// <summary>
        /// Pompe les callbacks Steamworks. SteamClient.Init est appele sans thread de callbacks
        /// automatique : sans cet appel periodique, toute operation asynchrone du SDK (creation de
        /// lobby, etc.) reste bloquee indefiniment. Sans effet si aucune session Steam n'est valide.
        /// </summary>
        void RunCallbacks();
    }
}
