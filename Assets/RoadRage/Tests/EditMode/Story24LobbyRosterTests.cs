using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RoadRage.Features.Online;
using RoadRage.Shared.Domain;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.4 : roster, etats prets et difficulte synchronisee via LobbyRosterService, avec
    /// une fausse plateforme de lobby (aucun client Steam requis) pilotant LobbyRoomService/LobbyJoinService
    /// vers Open/Joined.
    /// </summary>
    public sealed class Story24LobbyRosterTests
    {
        private const uint TestAppId = 480;

        [Test]
        public void TickDoesNothingWhenNoLobbyIsActive()
        {
            var roster = BuildService(out var lobbyPlatform, out _, out _);

            roster.Tick();

            Assert.That(lobbyPlatform.GetRosterSnapshotCallCount, Is.EqualTo(0), "aucune interrogation de la plateforme hors lobby actif");
            Assert.That(roster.IsSynchronized, Is.False);
            Assert.That(roster.AllMembersReady, Is.False);
        }

        [Test]
        public void TickUpdatesCurrentAndRaisesEventWhenRoomOpenAndSnapshotChanges()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);
            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 123456UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();
            Assert.That(lobbyRoom.Status, Is.EqualTo(LobbyRoomStatus.Open));

            LobbyRosterSnapshot? raised = null;
            roster.RosterChanged += snapshot => raised = snapshot;

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Hard, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", true),
                new LobbyMemberSnapshot(2UL, "Invite", false)
            });

            roster.Tick();

            Assert.That(raised, Is.Not.Null);
            Assert.That(roster.Current.Members.Length, Is.EqualTo(2));
            Assert.That(roster.IsSynchronized, Is.True);
            Assert.That(roster.AllMembersReady, Is.False, "un membre non pret doit empecher AllMembersReady");
        }

        [Test]
        public void TickDoesNotRaiseEventWhenSnapshotIsUnchanged()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);
            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 123456UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", true)
            });
            roster.Tick();

            var raiseCount = 0;
            roster.RosterChanged += _ => raiseCount++;

            roster.Tick();

            Assert.That(raiseCount, Is.EqualTo(0), "un instantane identique ne doit jamais republier d'evenement");
        }

        [Test]
        public void AllMembersReadyIsTrueOnlyWhenEveryMemberIsReady()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);
            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", true),
                new LobbyMemberSnapshot(2UL, "Invite", true)
            });
            roster.Tick();

            Assert.That(roster.AllMembersReady, Is.True);
        }

        [Test]
        public void SetLocalReadyUpdatesLocalReadyAndCallsPlatformWhenRoomOpen()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);
            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();
            lobbyPlatform.SetReadyCallCount = 0;

            roster.SetLocalReady(true);

            Assert.That(roster.LocalReady, Is.True);
            Assert.That(lobbyPlatform.SetReadyCallCount, Is.EqualTo(1));
            Assert.That(lobbyPlatform.LastReadyValue, Is.True);
        }

        [Test]
        public void SetLocalReadyDoesNotCallPlatformWhenNoLobbyIsActive()
        {
            var roster = BuildService(out var lobbyPlatform, out _, out _);

            roster.SetLocalReady(true);

            Assert.That(roster.LocalReady, Is.True, "l'intention locale est retenue meme sans lobby actif");
            Assert.That(lobbyPlatform.SetReadyCallCount, Is.EqualTo(0));
        }

        [Test]
        public void RoomOpeningResetsLocalReadyToFalse()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);
            roster.SetLocalReady(true);

            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            Assert.That(roster.LocalReady, Is.False, "une room fraichement ouverte ne doit jamais heriter d'un etat pret perime");
        }

        [Test]
        public void RoomClosingResetsCurrentToEmptyAndRaisesEvent()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);
            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", true)
            });
            roster.Tick();
            Assert.That(roster.Current.HasLobby, Is.True);

            LobbyRosterSnapshot? raised = null;
            roster.RosterChanged += snapshot => raised = snapshot;

            lobbyRoom.CloseRoom();

            Assert.That(raised, Is.Not.Null);
            Assert.That(roster.Current.HasLobby, Is.False);
        }

        [Test]
        public void PublishDifficultyOnlyCallsPlatformWhenRoomIsOpen()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);

            roster.PublishDifficulty(Difficulty.Hard);
            Assert.That(lobbyPlatform.SetDifficultyCallCount, Is.EqualTo(0), "sans room hote ouverte, aucune publication de difficulte");

            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            roster.PublishDifficulty(Difficulty.Hard);
            Assert.That(lobbyPlatform.SetDifficultyCallCount, Is.EqualTo(1));
            Assert.That(lobbyPlatform.LastDifficulty, Is.EqualTo(Difficulty.Hard));
        }

        [Test]
        public void ConstructorRejectsNullArguments()
        {
            var steamPlatform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = true };
            var onlineServices = new OnlineServicesBootstrapService(steamPlatform, TestAppId);
            var lobbyPlatform = new FakeSteamLobbyPlatform();
            var lobbyRoom = new LobbyRoomService(lobbyPlatform, onlineServices);
            var lobbyJoin = new LobbyJoinService(lobbyPlatform, onlineServices);

            Assert.Throws<ArgumentNullException>(() => new LobbyRosterService(null, lobbyRoom, lobbyJoin));
            Assert.Throws<ArgumentNullException>(() => new LobbyRosterService(lobbyPlatform, null, lobbyJoin));
            Assert.Throws<ArgumentNullException>(() => new LobbyRosterService(lobbyPlatform, lobbyRoom, null));
        }

        private static LobbyRosterService BuildService(out FakeSteamLobbyPlatform lobbyPlatform, out LobbyRoomService lobbyRoom, out LobbyJoinService lobbyJoin)
        {
            var steamPlatform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = true };
            var onlineServices = new OnlineServicesBootstrapService(steamPlatform, TestAppId);
            onlineServices.TryInitialize();
            Assert.That(onlineServices.Status, Is.EqualTo(OnlineServicesStatus.Online));

            lobbyPlatform = new FakeSteamLobbyPlatform();
            lobbyRoom = new LobbyRoomService(lobbyPlatform, onlineServices);
            lobbyJoin = new LobbyJoinService(lobbyPlatform, onlineServices);
            return new LobbyRosterService(lobbyPlatform, lobbyRoom, lobbyJoin);
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
            public LobbyCreateOutcome NextCreateOutcome { get; set; } = LobbyCreateOutcome.Failed;

            public LobbyRosterSnapshot NextRoster { get; set; } = LobbyRosterSnapshot.Empty;

            public int GetRosterSnapshotCallCount { get; private set; }

            public int SetReadyCallCount { get; set; }

            public bool LastReadyValue { get; private set; }

            public int SetDifficultyCallCount { get; private set; }

            public Difficulty LastDifficulty { get; private set; }

            public Task<LobbyCreateOutcome> CreateLobbyAsync(int maxMembers)
            {
                return Task.FromResult(NextCreateOutcome);
            }

            public Task<LobbyJoinOutcome> JoinLobbyAsync(ulong lobbyId)
            {
                return Task.FromResult(LobbyJoinOutcome.Failed);
            }

            public void LeaveCurrentLobby()
            {
            }

            public LobbyRosterSnapshot GetRosterSnapshot()
            {
                GetRosterSnapshotCallCount++;
                return NextRoster;
            }

            public void SetLocalMemberReady(bool ready)
            {
                SetReadyCallCount++;
                LastReadyValue = ready;
            }

            public void SetLobbyDifficulty(Difficulty difficulty)
            {
                SetDifficultyCallCount++;
                LastDifficulty = difficulty;
            }
        }
    }
}
