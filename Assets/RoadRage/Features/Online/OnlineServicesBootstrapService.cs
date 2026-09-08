using System;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Service unique d'initialisation des services en ligne Steam (Story 2.1). Objet C# pur, sans
    /// dependance Unity ni etat de gameplay : il ne connait que ISteamPlatform et ne mute jamais
    /// le moindre etat de gameplay reseau partage (owned par les features de gameplay elles-memes).
    /// </summary>
    public sealed class OnlineServicesBootstrapService
    {
        private readonly ISteamPlatform platform;

        private readonly uint appId;

        public OnlineServicesBootstrapService(ISteamPlatform platform, uint appId)
        {
            if (platform == null)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            this.platform = platform;
            this.appId = appId;
            Status = OnlineServicesStatus.NotStarted;
        }

        public OnlineServicesStatus Status { get; private set; }

        public event Action<OnlineServicesStatus> StatusChanged;

        /// <summary>
        /// Tente l'initialisation une seule fois par session applicative : les appels suivants
        /// republient le statut deja resolu sans relancer l'initialisation Steamworks.
        /// </summary>
        public void TryInitialize()
        {
            if (Status != OnlineServicesStatus.NotStarted)
            {
                RaiseStatusChanged(Status);
                return;
            }

            OnlineServicesStatus result;
            try
            {
                platform.Init(appId);
                result = ResolvePostInitStatus();
            }
            catch (DllNotFoundException)
            {
                result = OnlineServicesStatus.Offline;
            }
            catch (Exception)
            {
                result = OnlineServicesStatus.InitializationFailed;
            }

            Status = result;
            RaiseStatusChanged(result);
        }

        /// <summary>Ferme la session Steam si elle a ete ouverte. Appele par le proprietaire persistant a la destruction.</summary>
        public void Shutdown()
        {
            if (platform.IsValid)
            {
                platform.Shutdown();
            }
        }

        private OnlineServicesStatus ResolvePostInitStatus()
        {
            if (!platform.IsValid)
            {
                return OnlineServicesStatus.InitializationFailed;
            }

            if (!platform.IsLoggedOn)
            {
                return OnlineServicesStatus.SignInFailed;
            }

            return OnlineServicesStatus.Online;
        }

        private void RaiseStatusChanged(OnlineServicesStatus status)
        {
            var handler = StatusChanged;
            if (handler != null)
            {
                handler(status);
            }
        }
    }
}
