using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RoadRage.Features.Online;
using RoadRage.Shared.Domain;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.2 : creation/fermeture de la room hote via LobbyRoomService, avec une fausse
    /// plateforme de lobby (aucun client Steam requis) et une fausse plateforme Steam pour piloter
    /// OnlineServicesBootstrapService vers l'etat Online ou non.
    /// </summary>
    public sealed class Story22HostCreatedPrivateRoomTests
    {
        private const uint TestAppId = 480;

        [Test]
        public async Task CreateRoomAsyncOpensRoomWithJoinCodeWhenServicesOnlineAndPlatformSucceeds()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.NextOutcome = new LobbyCreateOutcome(true, 123456789UL);

            LobbyRoomStatus? raised = null;
            service.StatusChanged += status => raised = status;

            await service.CreateRoomAsync();

            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.Open));
            Assert.That(service.JoinCode, Is.EqualTo(123456789UL));
            Assert.That(raised, Is.EqualTo(LobbyRoomStatus.Open));
            Assert.That(lobbyPlatform.CreateLobbyCallCount, Is.EqualTo(1));
            Assert.That(lobbyPlatform.LastMaxMembers, Is.EqualTo(LobbyRoomService.MaxMembers));
        }

        [Test]
        public async Task CreateRoomAsyncRefusesWhenServicesNotOnline()
        {
            var service = BuildService(online: false, out var lobbyPlatform);

            await service.CreateRoomAsync();

            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.ServicesUnavailable));
            Assert.That(service.JoinCode, Is.EqualTo(0UL));
            Assert.That(lobbyPlatform.CreateLobbyCallCount, Is.EqualTo(0), "aucune tentative de creation de lobby Steam si les services ne sont pas Online");
        }

        [Test]
        public async Task CreateRoomAsyncResolvesToCreationFailedWhenPlatformReturnsFailure()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.NextOutcome = LobbyCreateOutcome.Failed;

            await service.CreateRoomAsync();

            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.CreationFailed));
            Assert.That(service.JoinCode, Is.EqualTo(0UL));
        }

        [Test]
        public async Task CreateRoomAsyncResolvesToCreationFailedWhenPlatformThrows()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.ThrowOnCreate = new InvalidOperationException("Networking Sockets indisponible");

            await service.CreateRoomAsync();

            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.CreationFailed));
        }

        [Test]
        public async Task CreateRoomAsyncIsANoOpWhenARoomIsAlreadyOpen()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.NextOutcome = new LobbyCreateOutcome(true, 111UL);
            await service.CreateRoomAsync();
            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.Open));

            lobbyPlatform.NextOutcome = new LobbyCreateOutcome(true, 222UL);
            await service.CreateRoomAsync();

            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.Open));
            Assert.That(service.JoinCode, Is.EqualTo(111UL), "une room deja ouverte ne doit jamais etre remplacee par un second appel");
            Assert.That(lobbyPlatform.CreateLobbyCallCount, Is.EqualTo(1), "un double-clic ne doit jamais declencher une seconde creation concurrente");
        }

        [Test]
        public void CloseRoomLeavesThePlatformLobbyAndResetsState()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            SetOpenViaReflectionFreeCreate(service, lobbyPlatform, 555UL);

            LobbyRoomStatus? raised = null;
            service.StatusChanged += status => raised = status;

            service.CloseRoom();

            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.Closed));
            Assert.That(service.JoinCode, Is.EqualTo(0UL));
            Assert.That(raised, Is.EqualTo(LobbyRoomStatus.Closed));
            Assert.That(lobbyPlatform.LeaveCallCount, Is.EqualTo(1));
        }

        [Test]
        public void CloseRoomIsANoOpWhenNoRoomIsOpen()
        {
            var service = BuildService(online: true, out var lobbyPlatform);

            service.CloseRoom();

            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.Closed));
            Assert.That(lobbyPlatform.LeaveCallCount, Is.EqualTo(0));
        }

        [Test]
        public void ConstructorRejectsNullPlatform()
        {
            var onlineServices = new OnlineServicesBootstrapService(new FakeSteamPlatform { IsValid = true, IsLoggedOn = true }, TestAppId);
            Assert.Throws<ArgumentNullException>(() => new LobbyRoomService(null, onlineServices));
        }

        [Test]
        public void ConstructorRejectsNullOnlineServices()
        {
            Assert.Throws<ArgumentNullException>(() => new LobbyRoomService(new FakeSteamLobbyPlatform(), null));
        }

        private static LobbyRoomService BuildService(bool online, out FakeSteamLobbyPlatform lobbyPlatform)
        {
            var steamPlatform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = online };
            var onlineServices = new OnlineServicesBootstrapService(steamPlatform, TestAppId);
            onlineServices.TryInitialize();
            Assert.That(onlineServices.Status, Is.EqualTo(online ? OnlineServicesStatus.Online : OnlineServicesStatus.SignInFailed));

            lobbyPlatform = new FakeSteamLobbyPlatform();
            return new LobbyRoomService(lobbyPlatform, onlineServices);
        }

        /// <summary>Ouvre une room via le chemin de creation reel : CloseRoom ne doit jamais etre teste contre un etat force artificiellement.</summary>
        private static void SetOpenViaReflectionFreeCreate(LobbyRoomService service, FakeSteamLobbyPlatform lobbyPlatform, ulong joinCode)
        {
            lobbyPlatform.NextOutcome = new LobbyCreateOutcome(true, joinCode);
            service.CreateRoomAsync().GetAwaiter().GetResult();
            Assert.That(service.Status, Is.EqualTo(LobbyRoomStatus.Open));
        }

        private sealed class FakeSteamPlatform : ISteamPlatform
        {
            public bool IsValid { get; set; }

            public bool IsLoggedOn { get; set; }

            public void Init(uint appId)
            {
            }

            public void Shutdown()
            {
            }

            public void RunCallbacks()
            {
            }
        }

        private sealed class FakeSteamLobbyPlatform : ISteamLobbyPlatform
        {
            public LobbyCreateOutcome NextOutcome { get; set; } = LobbyCreateOutcome.Failed;

            public Exception ThrowOnCreate { get; set; }

            public int CreateLobbyCallCount { get; private set; }

            public int LeaveCallCount { get; private set; }

            public int LastMaxMembers { get; private set; }

            public Task<LobbyCreateOutcome> CreateLobbyAsync(int maxMembers)
            {
                CreateLobbyCallCount++;
                LastMaxMembers = maxMembers;

                if (ThrowOnCreate != null)
                {
                    throw ThrowOnCreate;
                }

                return Task.FromResult(NextOutcome);
            }

            public void LeaveCurrentLobby()
            {
                LeaveCallCount++;
            }

            public Task<LobbyJoinOutcome> JoinLobbyAsync(ulong lobbyId)
            {
                return Task.FromResult(LobbyJoinOutcome.Failed);
            }

            public LobbyRosterSnapshot GetRosterSnapshot()
            {
                return LobbyRosterSnapshot.Empty;
            }

            public void SetLocalMemberReady(bool ready)
            {
            }

            public void SetLocalMemberProfile(string displayName, string characterId)
            {
            }

            public void SetLobbyDifficulty(Difficulty difficulty)
            {
            }
        }
    }
}
