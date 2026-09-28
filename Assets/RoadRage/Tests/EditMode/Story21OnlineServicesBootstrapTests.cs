using System;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Online;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.1 : resolution de statut d'OnlineServicesBootstrapService via une fausse
    /// plateforme (aucun client Steam requis), idempotence de TryInitialize, et deux garde-fous
    /// architecturaux verifies par lecture de source : isolation vis-a-vis de l'etat de gameplay et
    /// absence de secrets/cles/tokens en dur.
    /// </summary>
    [Category("Core")]
    public sealed class Story21OnlineServicesBootstrapTests
    {
        private const uint TestAppId = 480;

        [Test]
        public void TryInitializeResolvesToOnlineWhenValidAndLoggedOn()
        {
            var platform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = true };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            OnlineServicesStatus? raised = null;
            service.StatusChanged += status => raised = status;

            service.TryInitialize();

            Assert.That(service.Status, Is.EqualTo(OnlineServicesStatus.Online));
            Assert.That(raised, Is.EqualTo(OnlineServicesStatus.Online));
            Assert.That(platform.InitCalledWithAppId, Is.EqualTo(TestAppId));
        }

        [Test]
        public void TryInitializeResolvesToSignInFailedWhenValidButNotLoggedOn()
        {
            var platform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = false };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            service.TryInitialize();

            Assert.That(service.Status, Is.EqualTo(OnlineServicesStatus.SignInFailed));
        }

        [Test]
        public void TryInitializeResolvesToInitializationFailedWhenInitThrows()
        {
            var platform = new FakeSteamPlatform { ThrowOnInit = new InvalidOperationException("client Steam absent") };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            service.TryInitialize();

            Assert.That(service.Status, Is.EqualTo(OnlineServicesStatus.InitializationFailed));
        }

        [Test]
        public void TryInitializeResolvesToInitializationFailedWhenInitSucceedsButNotValid()
        {
            var platform = new FakeSteamPlatform { IsValid = false, IsLoggedOn = true };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            service.TryInitialize();

            Assert.That(service.Status, Is.EqualTo(OnlineServicesStatus.InitializationFailed));
        }

        [Test]
        public void TryInitializeResolvesToOfflineWhenSteamRuntimeIsUnavailable()
        {
            var platform = new FakeSteamPlatform { ThrowOnInit = new DllNotFoundException("steam_api64") };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            service.TryInitialize();

            Assert.That(service.Status, Is.EqualTo(OnlineServicesStatus.Offline));
        }

        [Test]
        public void TryInitializeIsIdempotentAndDoesNotReInitializeThePlatform()
        {
            var platform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = true };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            service.TryInitialize();
            var initCallsAfterFirst = platform.InitCallCount;

            var raisedCount = 0;
            service.StatusChanged += _ => raisedCount++;
            service.TryInitialize();

            Assert.That(platform.InitCallCount, Is.EqualTo(initCallsAfterFirst), "un second appel ne doit pas relancer Init");
            Assert.That(raisedCount, Is.EqualTo(1), "un second appel doit tout de meme republier le statut deja resolu");
        }

        [Test]
        public void ShutdownCallsPlatformShutdownOnlyWhenValid()
        {
            var platform = new FakeSteamPlatform { IsValid = false };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            service.Shutdown();
            Assert.That(platform.ShutdownCallCount, Is.EqualTo(0));

            platform.IsValid = true;
            service.Shutdown();
            Assert.That(platform.ShutdownCallCount, Is.EqualTo(1));
        }

        [Test]
        public void TickPumpsPlatformCallbacksOnlyWhenOnline()
        {
            var platform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = true };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            service.Tick();
            Assert.That(platform.RunCallbacksCallCount, Is.EqualTo(0), "avant resolution du statut, aucun pompage ne doit avoir lieu");

            service.TryInitialize();
            Assert.That(service.Status, Is.EqualTo(OnlineServicesStatus.Online));

            service.Tick();
            Assert.That(platform.RunCallbacksCallCount, Is.EqualTo(1), "une fois Online, Tick doit pomper les callbacks Steamworks a chaque appel");
        }

        [Test]
        public void TickNeverPumpsPlatformCallbacksWhenNotOnline()
        {
            var platform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = false };
            var service = new OnlineServicesBootstrapService(platform, TestAppId);

            service.TryInitialize();
            Assert.That(service.Status, Is.EqualTo(OnlineServicesStatus.SignInFailed));

            service.Tick();
            Assert.That(platform.RunCallbacksCallCount, Is.EqualTo(0), "un statut non-Online ne doit jamais pomper les callbacks Steamworks");
        }

        [Test]
        public void ConstructorRejectsNullPlatform()
        {
            Assert.Throws<ArgumentNullException>(() => new OnlineServicesBootstrapService(null, TestAppId));
        }

        [Test]
        public void OnlineFeatureSourceNeverReferencesGameplayState()
        {
            var forbiddenTypes = new[] { "NetworkedPlayerState", "NetworkedRunState", "NetworkedRageState", "NetworkedCrewEconomyState", "NetworkedAIVehicleState", "NetworkedBossState" };
            var sourceFiles = Directory.GetFiles("Assets/RoadRage/Features/Online", "*.cs", SearchOption.AllDirectories);
            Assert.That(sourceFiles.Length, Is.GreaterThan(0));

            foreach (var file in sourceFiles)
            {
                var source = File.ReadAllText(file);
                foreach (var forbiddenType in forbiddenTypes)
                {
                    Assert.That(source, Does.Not.Contain(forbiddenType), file + " ne doit jamais referencer d'etat de gameplay reseau (" + forbiddenType + ")");
                }
            }
        }

        [Test]
        public void FacepunchSteamPlatformInitDoesNotReinitializeExistingClient()
        {
            var source = File.ReadAllText("Assets/RoadRage/Features/Online/FacepunchSteamPlatform.cs");

            Assert.That(source, Does.Contain("if (SteamClient.IsValid)"));
            Assert.That(source.IndexOf("SteamClient.IsValid", StringComparison.Ordinal), Is.LessThan(source.IndexOf("SteamClient.Init", StringComparison.Ordinal)));
        }

        [Test]
        public void RoadRageSourceNeverHardcodesServiceCredentials()
        {
            var suspiciousMarkers = new[] { "SteamAPIKey", "WebApiKey", "PublisherKey", "ClientSecret", "client_secret", "SetAPIKey" };

            // Exclut Tests/ : ce fichier de test cite ces marqueurs dans le tableau ci-dessus,
            // ce qui le ferait echouer contre lui-meme si le dossier des tests etait scanne.
            var sourceFiles = Array.FindAll(
                Directory.GetFiles("Assets/RoadRage", "*.cs", SearchOption.AllDirectories),
                file => !file.Replace('\\', '/').Contains("/Tests/"));
            Assert.That(sourceFiles.Length, Is.GreaterThan(0));

            foreach (var file in sourceFiles)
            {
                var source = File.ReadAllText(file);
                foreach (var marker in suspiciousMarkers)
                {
                    Assert.That(source, Does.Not.Contain(marker), file + " ne doit jamais contenir de cle/secret de service en dur (" + marker + ")");
                }
            }
        }

        private sealed class FakeSteamPlatform : ISteamPlatform
        {
            public bool IsValid { get; set; }

            public bool IsLoggedOn { get; set; }

            public Exception ThrowOnInit { get; set; }

            public uint? InitCalledWithAppId { get; private set; }

            public int InitCallCount { get; private set; }

            public int ShutdownCallCount { get; private set; }

            public int RunCallbacksCallCount { get; private set; }

            public void Init(uint appId)
            {
                InitCallCount++;
                InitCalledWithAppId = appId;

                if (ThrowOnInit != null)
                {
                    throw ThrowOnInit;
                }
            }

            public void Shutdown()
            {
                ShutdownCallCount++;
            }

            public void RunCallbacks()
            {
                RunCallbacksCallCount++;
            }
        }
    }
}
