using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RoadRage.Features.Online;
using RoadRage.Shared.Domain;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.3 : join par code via LobbyJoinService, avec une fausse plateforme de lobby
    /// (aucun client Steam requis) et une fausse plateforme Steam pour piloter OnlineServicesBootstrapService
    /// vers l'etat Online ou non.
    /// </summary>
    public sealed class Story23JoinByCodeTests
    {
        private const uint TestAppId = 480;

        [Test]
        public async Task JoinByCodeAsyncRejectsEmptyCodeWithoutCallingPlatform()
        {
            var service = BuildService(online: true, out var lobbyPlatform);

            await service.JoinByCodeAsync(string.Empty);

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.InvalidCode));
            Assert.That(lobbyPlatform.JoinLobbyCallCount, Is.EqualTo(0), "un code invalide ne doit jamais declencher d'appel Steam");
        }

        [Test]
        public async Task JoinByCodeAsyncRejectsWhitespaceOnlyCode()
        {
            var service = BuildService(online: true, out var lobbyPlatform);

            await service.JoinByCodeAsync("   ");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.InvalidCode));
            Assert.That(lobbyPlatform.JoinLobbyCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task JoinByCodeAsyncRejectsCodeThatIsNotExactlyFiveAlphanumericCharacters()
        {
            var service = BuildService(online: true, out var lobbyPlatform);

            await service.JoinByCodeAsync("AB-12");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.InvalidCode));
            Assert.That(lobbyPlatform.JoinLobbyCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task JoinByCodeAsyncTrimsWhitespaceAndJoinsOnSuccess()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.NextOutcome = new LobbyJoinOutcome(true, LobbyJoinFailureReason.None);

            LobbyJoinStatus? raised = null;
            service.StatusChanged += status => raised = status;

            await service.JoinByCodeAsync("  ab12z  ");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.Joined));
            Assert.That(service.JoinedLobbyId, Is.EqualTo(123456UL));
            Assert.That(service.JoinedJoinCode, Is.EqualTo("AB12Z"));
            Assert.That(raised, Is.EqualTo(LobbyJoinStatus.Joined));
            Assert.That(lobbyPlatform.JoinLobbyCallCount, Is.EqualTo(1));
            Assert.That(lobbyPlatform.LastJoinCode, Is.EqualTo("AB12Z"));
        }

        [Test]
        public async Task JoinByCodeAsyncRefusesWhenServicesNotOnline()
        {
            var service = BuildService(online: false, out var lobbyPlatform);

            await service.JoinByCodeAsync("AB123");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.ServicesUnavailable));
            Assert.That(lobbyPlatform.JoinLobbyCallCount, Is.EqualTo(0), "aucune tentative de join Steam si les services ne sont pas Online");
        }

        [Test]
        public async Task JoinByCodeAsyncResolvesToRoomFullWhenPlatformReportsFull()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.NextOutcome = new LobbyJoinOutcome(false, LobbyJoinFailureReason.Full);

            await service.JoinByCodeAsync("AB123");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.RoomFull));
            Assert.That(service.JoinedLobbyId, Is.EqualTo(0UL));
        }

        [Test]
        public async Task JoinByCodeAsyncResolvesToSessionExpiredWhenPlatformReportsExpired()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.NextOutcome = new LobbyJoinOutcome(false, LobbyJoinFailureReason.Expired);

            await service.JoinByCodeAsync("AB123");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.SessionExpired));
        }

        [Test]
        public async Task JoinByCodeAsyncResolvesToJoinFailedWhenPlatformReportsGenericFailure()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.NextOutcome = LobbyJoinOutcome.Failed;

            await service.JoinByCodeAsync("AB123");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.JoinFailed));
        }

        [Test]
        public async Task JoinByCodeAsyncResolvesToJoinFailedWhenPlatformThrows()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.ThrowOnJoin = new InvalidOperationException("Networking Sockets indisponible");

            await service.JoinByCodeAsync("AB123");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.JoinFailed));
        }

        [Test]
        public async Task JoinByCodeAsyncIsANoOpWhenAlreadyJoined()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            lobbyPlatform.NextOutcome = new LobbyJoinOutcome(true, LobbyJoinFailureReason.None);
            await service.JoinByCodeAsync("AAAAA");
            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.Joined));

            await service.JoinByCodeAsync("BBBBB");

            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.Joined));
            Assert.That(service.JoinedJoinCode, Is.EqualTo("AAAAA"), "un lobby deja rejoint ne doit jamais etre remplace sans leave explicite");
            Assert.That(lobbyPlatform.JoinLobbyCallCount, Is.EqualTo(1), "aucune seconde tentative de join tant que le premier lobby n'est pas quitte");
        }

        [Test]
        public void JoinByCodeAsyncIsANoOpWhenAJoinIsAlreadyInProgress()
        {
            var service = BuildService(online: true, out var lobbyPlatform);
            var gate = new TaskCompletionSource<LobbyJoinOutcome>();
            lobbyPlatform.NextTask = gate.Task;

            var firstJoin = service.JoinByCodeAsync("AAAAA");
            Assert.That(service.Status, Is.EqualTo(LobbyJoinStatus.Joining));

            var secondJoin = service.JoinByCodeAsync("BBBBB");

            Assert.That(lobbyPlatform.JoinLobbyCallCount, Is.EqualTo(1), "un double-clic ne doit jamais declencher un second join concurrent");

            gate.SetResult(new LobbyJoinOutcome(true, LobbyJoinFailureReason.None));
            firstJoin.GetAwaiter().GetResult();
            secondJoin.GetAwaiter().GetResult();

            Assert.That(service.JoinedJoinCode, Is.EqualTo("AAAAA"), "la premiere tentative en cours ne doit jamais etre remplacee par le second code");
        }

        [Test]
        public void ConstructorRejectsNullPlatform()
        {
            var onlineServices = new OnlineServicesBootstrapService(new FakeSteamPlatform { IsValid = true, IsLoggedOn = true }, TestAppId);
            Assert.Throws<ArgumentNullException>(() => new LobbyJoinService(null, onlineServices));
        }

        [Test]
        public void ConstructorRejectsNullOnlineServices()
        {
            Assert.Throws<ArgumentNullException>(() => new LobbyJoinService(new FakeSteamLobbyPlatform(), null));
        }

        private static LobbyJoinService BuildService(bool online, out FakeSteamLobbyPlatform lobbyPlatform)
        {
            var steamPlatform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = online };
            var onlineServices = new OnlineServicesBootstrapService(steamPlatform, TestAppId);
            onlineServices.TryInitialize();
            Assert.That(onlineServices.Status, Is.EqualTo(online ? OnlineServicesStatus.Online : OnlineServicesStatus.SignInFailed));

            lobbyPlatform = new FakeSteamLobbyPlatform();
            return new LobbyJoinService(lobbyPlatform, onlineServices);
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
            public LobbyJoinOutcome NextOutcome { get; set; } = LobbyJoinOutcome.Failed;

            public Task<LobbyJoinOutcome> NextTask { get; set; }

            public Exception ThrowOnJoin { get; set; }

            public int JoinLobbyCallCount { get; private set; }

            public ulong LastLobbyId { get; private set; }

            public string LastJoinCode { get; private set; }

            public Task<LobbyCreateOutcome> CreateLobbyAsync(int maxMembers)
            {
                return Task.FromResult(LobbyCreateOutcome.Failed);
            }

            public Task<LobbyJoinOutcome> JoinLobbyAsync(ulong lobbyId)
            {
                JoinLobbyCallCount++;
                LastLobbyId = lobbyId;

                if (ThrowOnJoin != null)
                {
                    throw ThrowOnJoin;
                }

                return NextTask ?? Task.FromResult(NextOutcome);
            }

            public Task<LobbyJoinOutcome> JoinLobbyByCodeAsync(string joinCode)
            {
                JoinLobbyCallCount++;
                LastJoinCode = joinCode;

                if (ThrowOnJoin != null)
                {
                    throw ThrowOnJoin;
                }

                if (NextTask != null)
                {
                    return NextTask;
                }

                return Task.FromResult(new LobbyJoinOutcome(NextOutcome.Success, NextOutcome.Reason, 123456UL));
            }

            public void LeaveCurrentLobby()
            {
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

            public void SetLobbyRunLaunchRequested(bool launchRequested)
            {
            }
        }
    }
}
