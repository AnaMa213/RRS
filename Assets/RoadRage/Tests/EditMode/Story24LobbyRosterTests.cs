using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Shared.Definitions;
using RoadRage.Shared.Domain;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.4 : roster, etats prets et difficulte synchronisee via LobbyRosterService, avec
    /// une fausse plateforme de lobby (aucun client Steam requis) pilotant LobbyRoomService/LobbyJoinService
    /// vers Open/Joined.
    /// </summary>
    [Category("Core")]
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
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true),
                new LobbyMemberSnapshot(2UL, "Invite", "char_rookie", false)
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
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true)
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
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true),
                new LobbyMemberSnapshot(2UL, "Invite", "char_rookie", true)
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
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true)
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

        /// <summary>
        /// Story 5.3 (AD-26), bug de regression : sans ce champ compare, un invite deja rejoint ne
        /// recevait jamais RosterChanged quand seul RunLaunchRequested changeait (roster autrement
        /// identique), donc ne demarrait jamais son propre StartClient() -- symptome rapporte : un
        /// second joueur rejoint le lobby par code mais n'entre jamais dans la partie que l'hote lance.
        /// </summary>
        [Test]
        public void TickRaisesEventWhenOnlyRunLaunchRequestedChanges()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);
            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true)
            }, runLaunchRequested: false);
            roster.Tick();

            var raiseCount = 0;
            LobbyRosterSnapshot? raised = null;
            roster.RosterChanged += snapshot =>
            {
                raiseCount++;
                raised = snapshot;
            };

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true)
            }, runLaunchRequested: true);
            roster.Tick();

            Assert.That(raiseCount, Is.EqualTo(1), "le seul changement de RunLaunchRequested doit republier le roster.");
            Assert.That(raised, Is.Not.Null);
            Assert.That(raised.Value.RunLaunchRequested, Is.True);
        }

        /// <summary>
        /// Story 5.3 (AD-26), AC2 "un second joueur rejoint la partie EN COURS" : quand l'invite
        /// rejoint APRES le lancement, il n'observe aucune transition du signal -- son tout premier
        /// instantane porte deja RunLaunchRequested = true. Current doit donc refleter ce signal des
        /// le premier Tick suivant le join, y compris si un roster identique avait deja ete memorise
        /// lors d'une session precedente (sinon SnapshotsEqual l'avale et l'invite reste bloque dans
        /// le lobby).
        /// </summary>
        [Test]
        public void JoiningAfterTheHostLaunchedExposesRunLaunchRequestedOnTheFirstTick()
        {
            var roster = BuildService(out var lobbyPlatform, out _, out var lobbyJoin);

            var launchedRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", false),
                new LobbyMemberSnapshot(2UL, "Invite", "char_rookie", false)
            }, runLaunchRequested: true);

            // Etat perime d'une session precedente, strictement identique a celui du lobby rejoint.
            lobbyPlatform.NextJoinOutcome = new LobbyJoinOutcome(true, LobbyJoinFailureReason.None);
            lobbyPlatform.NextRoster = launchedRoster;
            lobbyJoin.JoinByCodeAsync("AB123").GetAwaiter().GetResult();
            roster.Tick();
            Assert.That(roster.Current.RunLaunchRequested, Is.True, "premier join : le signal doit deja etre visible.");

            roster.Tick();

            Assert.That(roster.Current.RunLaunchRequested, Is.True,
                "l'invite qui rejoint une partie deja lancee doit voir RunLaunchRequested sur son etat courant, sans dependre d'une transition.");
            Assert.That(roster.Current.OwnerId, Is.EqualTo(1UL), "l'hote a rejoindre doit rester resolu pour StartClient.");
        }

        [Test]
        public void PublishRunLaunchRequestedOnlyCallsPlatformWhenRoomIsOpen()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);

            roster.PublishRunLaunchRequested(true);
            Assert.That(lobbyPlatform.SetLobbyRunLaunchRequestedCallCount, Is.EqualTo(0), "sans room hote ouverte, aucune publication du signal de lancement");

            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();
            var callCountAfterRoomOpen = lobbyPlatform.SetLobbyRunLaunchRequestedCallCount;

            roster.PublishRunLaunchRequested(true);
            Assert.That(lobbyPlatform.SetLobbyRunLaunchRequestedCallCount, Is.EqualTo(callCountAfterRoomOpen + 1));
            Assert.That(lobbyPlatform.LastLaunchRequested, Is.True);
        }

        [Test]
        public void RoomOpeningResetsRunLaunchRequestedToFalse()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);

            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            Assert.That(lobbyPlatform.SetLobbyRunLaunchRequestedCallCount, Is.GreaterThanOrEqualTo(1),
                "une room fraichement ouverte doit remettre a zero un signal de lancement perime d'une session precedente.");
            Assert.That(lobbyPlatform.LastLaunchRequested, Is.False);
        }

        [Test]
        public void PublishLocalProfileCallsPlatformWhenRoomOpenOrJoinedButNotOtherwise()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);

            roster.PublishLocalProfile("Alice", "char_rookie");
            Assert.That(lobbyPlatform.SetProfileCallCount, Is.EqualTo(0), "sans lobby actif, aucune publication de profil");

            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            roster.PublishLocalProfile("Alice", "char_rookie");
            Assert.That(lobbyPlatform.SetProfileCallCount, Is.EqualTo(1));
            Assert.That(lobbyPlatform.LastProfileDisplayName, Is.EqualTo("Alice"));
            Assert.That(lobbyPlatform.LastProfileCharacterId, Is.EqualTo("char_rookie"));
        }

        [Test]
        public void JoinedLobbyPublishesTheFrozenSessionSelection()
        {
            var roster = BuildService(out var lobbyPlatform, out _, out var lobbyJoin);
            lobbyPlatform.NextJoinOutcome = new LobbyJoinOutcome(true, LobbyJoinFailureReason.None);
            lobbyJoin.JoinByCodeAsync("AB123").GetAwaiter().GetResult();

            var profiles = new PlayerProfileStore();
            profiles.Set(new PlayerProfile("Invite", new DefinitionId("char_veteran")));
            profiles.Freeze();
            var selection = profiles.SessionSelection;
            roster.PublishLocalProfile(selection.DisplayName, selection.CharacterId.Value);

            Assert.That(lobbyJoin.Status, Is.EqualTo(LobbyJoinStatus.Joined));
            Assert.That(profiles.IsFrozen, Is.True);
            Assert.That(lobbyPlatform.LastProfileDisplayName, Is.EqualTo("Invite"));
            Assert.That(lobbyPlatform.LastProfileCharacterId, Is.EqualTo("char_veteran"));
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

            public LobbyJoinOutcome NextJoinOutcome { get; set; } = LobbyJoinOutcome.Failed;

            public LobbyRosterSnapshot NextRoster { get; set; } = LobbyRosterSnapshot.Empty;

            public int GetRosterSnapshotCallCount { get; private set; }

            public int SetReadyCallCount { get; set; }

            public bool LastReadyValue { get; private set; }

            public int SetDifficultyCallCount { get; private set; }

            public Difficulty LastDifficulty { get; private set; }

            public int SetLobbyRunLaunchRequestedCallCount { get; private set; }

            public bool LastLaunchRequested { get; private set; }

            public Task<LobbyCreateOutcome> CreateLobbyAsync(int maxMembers)
            {
                return Task.FromResult(NextCreateOutcome);
            }

            public Task<LobbyJoinOutcome> JoinLobbyAsync(ulong lobbyId)
            {
                return Task.FromResult(NextJoinOutcome);
            }

            public Task<LobbyJoinOutcome> JoinLobbyByCodeAsync(string joinCode)
            {
                return Task.FromResult(new LobbyJoinOutcome(NextJoinOutcome.Success, NextJoinOutcome.Reason, 1UL));
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

            public int SetProfileCallCount { get; private set; }

            public string LastProfileDisplayName { get; private set; }

            public string LastProfileCharacterId { get; private set; }

            public void SetLocalMemberProfile(string displayName, string characterId)
            {
                SetProfileCallCount++;
                LastProfileDisplayName = displayName;
                LastProfileCharacterId = characterId;
            }

            public void SetLobbyDifficulty(Difficulty difficulty)
            {
                SetDifficultyCallCount++;
                LastDifficulty = difficulty;
            }

            public void SetLobbyRunLaunchRequested(bool launchRequested)
            {
                SetLobbyRunLaunchRequestedCallCount++;
                LastLaunchRequested = launchRequested;
            }
        }
    }
}
