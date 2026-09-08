using Steamworks;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Seule classe du projet a toucher directement le SDK Facepunch.Steamworks (Story 2.1).
    /// Enveloppe fine sans logique d'etat : OnlineServicesBootstrapService porte toute la logique
    /// de resolution de statut, ce qui permet de la tester sans client Steam installe.
    /// </summary>
    public sealed class FacepunchSteamPlatform : ISteamPlatform
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
            SteamClient.Init(appId, false);
        }

        public void Shutdown()
        {
            SteamClient.Shutdown();
        }
    }
}
