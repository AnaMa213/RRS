using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using RoadRage.App.Lobby;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre le checkpoint Story 2.8 : gate d'approbation host-side, refus du Start client premature,
    /// propagation des changements de personnage dans le roster, et invariants scene/prefab/HUD/lifecycle
    /// deja necessaires au parcours online jouable de l'Epic 2.
    /// </summary>
    [Category("Core")]
    public sealed class Story28Epic2OnlinePlayableCheckpointTests
    {
        private const uint TestAppId = 480;

        [Test]
        public void ConnectionApprovalAcceptsValidProfileBelowFourReservedSlots()
        {
            var result = ResolveApproval(NetworkPlayerConnectionPayload.Encode("Invite", "char_rookie"), 3);

            Assert.That(result.Approved, Is.True);
            Assert.That(result.Profile.DisplayName, Is.EqualTo("Invite"));
            Assert.That(result.Profile.CharacterId, Is.EqualTo("char_rookie"));
            Assert.That(result.Reason, Is.Empty);
        }

        [Test]
        public void ConnectionApprovalRejectsInvalidPayloadBeforeSpawn()
        {
            var result = ResolveApproval(new byte[] { 0xFF, 0x01, 0x02 }, 0);

            Assert.That(result.Approved, Is.False);
            Assert.That(result.Reason, Is.EqualTo(LobbyFlowController.ApprovalRejectedInvalidPayloadReason));
        }

        [Test]
        public void ConnectionApprovalRejectsMissingCharacterProfileBeforeDefaultFallback()
        {
            var result = ResolveApproval(NetworkPlayerConnectionPayload.Encode("Invite", string.Empty), 0);

            Assert.That(result.Approved, Is.False);
            Assert.That(result.Reason, Is.EqualTo(LobbyFlowController.ApprovalRejectedMissingProfileReason));
        }

        [Test]
        public void ConnectionApprovalRejectsFifthReservedPlayerBeforeSpawn()
        {
            var result = ResolveApproval(NetworkPlayerConnectionPayload.Encode("Cinquieme", "char_rookie"), 4);

            Assert.That(result.Approved, Is.False);
            Assert.That(result.Reason, Is.EqualTo(LobbyFlowController.ApprovalRejectedRoomFullReason));
        }

        /// <summary>
        /// Story 5.3 (AD-26), correction post-implementation d'un bug de regression rapporte : un
        /// invite deja rejoint ne doit jamais demarrer StartClient() depuis son propre clic sur Start
        /// Game (HandleStartGameRequested continue de refuser et de publier
        /// StartRefusedWaitingForHostMessage), mais DOIT le demarrer automatiquement une fois que
        /// l'hote a publie son signal de lancement (snapshot.RunLaunchRequested, via
        /// HandleRosterChanged) -- sans quoi un second joueur qui rejoint par code une partie deja
        /// demarree par Start Game n'entre jamais dans le jeu. L'ancienne assertion "jamais
        /// StartNetworkedRun(false) nulle part dans le fichier" etait trop large : elle pinnait
        /// l'absence totale du mecanisme plutot que son emplacement correct.
        /// </summary>
        [Test]
        public void JoinedClientStartButtonWaitsForHostInsteadOfStartingClient()
        {
            var source = File.ReadAllText(AssetsPath("RoadRage", "App", "Lobby", "LobbyFlowController.cs"));

            Assert.That(source, Does.Contain("lobbyJoin.Status == LobbyJoinStatus.Joined"));
            Assert.That(source, Does.Contain("StartRefusedWaitingForHostMessage"));

            var startGameRequestedStart = source.IndexOf("private async void HandleStartGameRequested()", StringComparison.Ordinal);
            var startNetworkedRunStart = source.IndexOf("private void StartNetworkedRun(bool asHost)", StringComparison.Ordinal);
            Assert.That(startGameRequestedStart, Is.GreaterThanOrEqualTo(0), "HandleStartGameRequested introuvable.");
            Assert.That(startNetworkedRunStart, Is.GreaterThan(startGameRequestedStart), "StartNetworkedRun doit rester defini apres HandleStartGameRequested.");
            var startGameRequestedBody = source.Substring(startGameRequestedStart, startNetworkedRunStart - startGameRequestedStart);

            Assert.That(startGameRequestedBody, Does.Not.Contain("StartNetworkedRun(false)"),
                "un invite ne doit jamais demarrer StartClient depuis le bouton Start avant le signal hote.");
            Assert.That(source, Does.Contain("lobbyRoster.Current.RunLaunchRequested"),
                "le signal de lancement publie par l'hote doit declencher StartNetworkedRun(false) cote invite, evalue sur l'etat courant du roster (et non sur un front RosterChanged) pour couvrir aussi l'invite qui rejoint APRES le lancement.");
        }

        [Test]
        public void RosterRaisesEventWhenOnlyCharacterIdChanges()
        {
            var roster = BuildRosterService(out var lobbyPlatform, out var lobbyRoom, out _);
            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true)
            });
            roster.Tick();

            var raiseCount = 0;
            roster.RosterChanged += _ => raiseCount++;

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", "char_runner", true)
            });
            roster.Tick();

            Assert.That(raiseCount, Is.EqualTo(1), "un changement de personnage seul doit rafraichir les silhouettes du roster.");
        }

        [Test]
        public void NetworkedCheckpointKeepsScenePrefabHudAndLifecycleWiring()
        {
            Assert.That(AppSceneRouter.MvpRunSceneName, Is.EqualTo("MVP_Run"));

            var playerRootPrefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            Assert.That(playerRootPrefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable depuis Resources.");
            Assert.That(playerRootPrefab.GetComponent<NetworkObject>(), Is.Not.Null, "NetworkedPlayerRoot doit porter NetworkObject.");
            Assert.That(playerRootPrefab.GetComponent<NetworkedPlayerState>(), Is.Not.Null, "NetworkedPlayerRoot doit porter NetworkedPlayerState.");
            Assert.That(playerRootPrefab.GetComponent<NetworkedPlayerLifecycleIntent>(), Is.Not.Null,
                "NetworkedPlayerRoot doit garder l'intention de respawn reseau.");

            Assert.That(RunCheckpointHudScreen.NetworkHostLobbyState, Is.Not.EqualTo(RunCheckpointHudScreen.NetworkClientLobbyState));

            var bootstrapSource = File.ReadAllText(AssetsPath("RoadRage", "App", "Bootstrap", "RoadRageBootstrap.cs"));
            var runFlowSource = File.ReadAllText(AssetsPath("RoadRage", "App", "Run", "RunFlowController.cs"));
            var sessionMonitorSource = File.ReadAllText(AssetsPath("RoadRage", "App", "Run", "NetworkedRunSessionMonitor.cs"));

            Assert.That(bootstrapSource, Does.Contain("NetworkConfig.ConnectionApproval = true"));
            Assert.That(bootstrapSource, Does.Contain("NetworkConfig.EnableSceneManagement = true"));
            Assert.That(runFlowSource, Does.Contain("AttachNetworkPoseReporter(activeLocalPlayer);"));
            Assert.That(runFlowSource, Does.Contain("state.Lifecycle.OnValueChanged += HandleLifecycleChanged;"));
            Assert.That(sessionMonitorSource, Does.Contain("manager.OnTransportFailure += HandleTransportFailure"));
        }

        [Test]
        public void MainMenuLobbyContrastRegressionCoverageRemainsRegistered()
        {
            var source = File.ReadAllText(AssetsPath("RoadRage", "Tests", "EditMode", "Story12LobbyShellTests.cs"));

            Assert.That(source, Does.Contain("MainMenuLobbyStatusLabelsContrastWithPanelBackgrounds"));
            Assert.That(source, Does.Contain("MinimumReadableContrast = 4.5f"));
        }

        private static ApprovalResult ResolveApproval(byte[] payload, int reservedNetworkSlots)
        {
            var method = typeof(LobbyFlowController).GetMethod("TryResolveApprovedNetworkProfile", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "TryResolveApprovedNetworkProfile doit rester le predicat testable du gate approval.");

            var args = new object[] { payload, reservedNetworkSlots, default(NetworkPlayerProfile), string.Empty };
            var approved = (bool)method.Invoke(null, args);

            return new ApprovalResult(approved, (NetworkPlayerProfile)args[2], (string)args[3]);
        }

        private static LobbyRosterService BuildRosterService(out FakeSteamLobbyPlatform lobbyPlatform, out LobbyRoomService lobbyRoom, out LobbyJoinService lobbyJoin)
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

        private static string AssetsPath(params string[] segments)
        {
            var path = Application.dataPath;
            foreach (var segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return path;
        }

        private readonly struct ApprovalResult
        {
            public ApprovalResult(bool approved, NetworkPlayerProfile profile, string reason)
            {
                Approved = approved;
                Profile = profile;
                Reason = reason ?? string.Empty;
            }

            public bool Approved { get; }

            public NetworkPlayerProfile Profile { get; }

            public string Reason { get; }
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
                return NextRoster;
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
