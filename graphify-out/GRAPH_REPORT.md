# Graph Report - RRS  (2026-09-18)

## Corpus Check
- 266 files · ~334,350 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2846 nodes · 6786 edges · 128 communities (112 shown, 13 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 372 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a6c24df7`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- Story14GreyboxAssetSeedTests
- Difficulty
- Invariants & Rules
- Story28Epic2OnlinePlayableCheckpointTests
- .WithMvpRun
- NetworkedVehicleState
- NetworkedPlayerReviveIntent
- DefinitionId
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyFlowController
- LocalOnFootController
- UserNotice
- Story51NpcRageFearFoundationTests
- MenuCharacterPreview
- NetworkedVehicleSeatIntent
- NetworkedPlayerState
- PassengerActionIntent
- .RequestHonk
- Story510LaneGraphAndRoutedTrafficTests
- Story27PlayerLifecycleTests
- Story12LobbyShellTests
- CharacterOption
- Story16Epic1PlayableCheckpointTests
- LobbyCodeClipboard
- VehicleDriveIntent
- .OnDrag
- GreyboxAssetSeedMetadata
- NetworkPlayerRegistry
- NetworkedVehicleDriverController
- LobbyRosterScreen
- PassengerActionCatalog
- .NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId
- PlayerProfileResolution
- LobbyJoinOutcome
- Story26InGameHudTests
- .HandleLifecycleChanged
- LobbyCreateOutcome
- RunCompositionRoot
- FakeSteamLobbyPlatform
- Story32DriverControlAndLocalCameraTests
- SeedExpectation
- .TearDown
- .TearDown
- .TearDown
- .TearDown
- NetworkedAIVehicleState
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- deferred-work.md
- Story54RageDrivenAiBehaviorStatesTests
- FakeSteamLobbyPlatform
- Lobby Join Service
- HostOwnedNetworkStateBehaviour
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- NetworkedPassengerActionIntent
- RoadRageBootstrap
- .NewFixture
- RageTuningDef
- Story15EmptyMapEntryTests
- Story57AiTrafficClientPresentationTests
- Story58EscapeMenuTests
- .ResetDamageState
- Story56RageRoadEventTriggerTests
- TrafficSettingsDef
- .SelectWeightedSuccessor
- PlayerProfile
- CharacterCatalog
- .FindRecursive
- RoadRage Scaffold Structure Tests
- LocalVehicleCameraRig
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- MainMenuScreen
- AIVehicleBehaviorDebugView
- NetworkedRageState
- Private Room Play Mode Tests
- NetworkedPlayerLifecycleIntent
- Netcode/Steamworks Smoke Tests
- NetworkedVehicleRecoveryIntent
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FacepunchSteamPlatform
- PlayerMode
- MainMenuFlowController
- OnFootMovementIntent
- .EnsureNetworkManager
- Story52BasicAiRouteFollowingAndRecoveryTests
- .NewTuning
- NetworkedPlayerPresentation
- LaneGraph
- LobbyRosterSnapshot
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- NetworkedPlayerSpawnService
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- PassengerActionDef
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- LocalVoidRespawnController
- RageSandboxAutoStart
- LobbyRoomService
- Story21OnlineServicesPlayModeTests
- LobbyShellScreen
- Lock-Rage Camera Fix Query
- RoadRage.Shared.Domain
- LobbyPlayerSlotView
- Story33SeatEntryExitAndPassengerPresenceTests
- .EnsureVehicleSandboxSeatHarness
- LaneNode
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedVehicleSeatService
- MonoBehaviour
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- PassengerActionVerdictCode
- .Create
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RunFlowController` - 118 edges
2. `NetworkedVehicleState` - 78 edges
3. `Story510LaneGraphAndRoutedTrafficTests` - 72 edges
4. `RunCheckpointHudScreen` - 69 edges
5. `NetworkedVehicleDriverController` - 66 edges
6. `RoadRage.Shared.Domain` - 63 edges
7. `RoadRage.Features.Players` - 50 edges
8. `LaneGraph` - 49 edges
9. `NetworkedAIVehicleDriverController` - 49 edges
10. `RoadRage.Features.Vehicles` - 49 edges

## Surprising Connections (you probably didn't know these)
- `RoadRageBootstrap` --references--> `PlayerProfileFileStore`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Players/PlayerProfileFileStore.cs
- `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` --references--> `CharacterCatalog`  [EXTRACTED]
  Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs → Assets/RoadRage/Features/Players/CharacterCatalog.cs
- `LobbyFlowController` --references--> `CharacterCatalog`  [EXTRACTED]
  Assets/RoadRage/App/Lobby/LobbyFlowController.cs → Assets/RoadRage/Features/Players/CharacterCatalog.cs
- `NetworkedPlayerSpawnService` --references--> `CharacterCatalog`  [EXTRACTED]
  Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs → Assets/RoadRage/Features/Players/CharacterCatalog.cs
- `RunFlowController` --references--> `CharacterCatalog`  [EXTRACTED]
  Assets/RoadRage/App/Run/RunFlowController.cs → Assets/RoadRage/Features/Players/CharacterCatalog.cs

## Import Cycles
- None detected.

## Communities (128 total, 13 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.11
Nodes (14): ArgumentException, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component (+6 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.14
Nodes (8): GameObject, NetworkManager, NetworkObject, NetworkPrefabsList, TearDown, Test, TextMeshProUGUI, Story25NetworkedPlayerSpawnTests

### Community 3 - "Story14GreyboxAssetSeedTests"
Cohesion: 0.17
Nodes (10): Collider, GameObject, NetworkObject, Renderer, Test, Transform, Story14GreyboxAssetSeedTests, Bounds (+2 more)

### Community 4 - "Difficulty"
Cohesion: 0.20
Nodes (6): Difficulty, Difficulty, Difficulty, Easy, Hard, Normal

### Community 5 - "Invariants & Rules"
Cohesion: 0.08
Nodes (23): AD-10 — Aucun reviewer LLM n'est empile par defaut, AD-11 — Chaque client agent garde sa propre declaration MCP, AD-12 — Les dossiers de skills dupliques restent en place, AD-13 — Un outil n'entre qu'avec un besoin mesure, un pilote et un rollback ecrit, AD-14 — Les `.csproj` generes ne sont jamais une configuration persistante, AD-1 — BMAD est la source de verite unique [ADOPTED], AD-2 — Graphify n'indexe que le code applicatif, AD-3 — Un symbole nomme est prouve avant d'etre interroge (+15 more)

### Community 6 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.17
Nodes (10): ApprovalResult, NetworkPlayerConnectionPayload, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+2 more)

### Community 7 - ".WithMvpRun"
Cohesion: 0.21
Nodes (4): Action, Renderer, Scene, Transform

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.14
Nodes (7): NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 10 - "DefinitionId"
Cohesion: 0.16
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 11 - "RunFlowController"
Cohesion: 0.08
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.13
Nodes (6): GameObject, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform, StringBuilder

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.06
Nodes (27): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+19 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.09
Nodes (15): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, AssemblyDefinition, Component (+7 more)

### Community 15 - "LobbyFlowController"
Cohesion: 0.14
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.12
Nodes (14): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector3, LocalOnFootController, IsDowned (+6 more)

### Community 17 - "UserNotice"
Cohesion: 0.06
Nodes (26): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+18 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 21 - "NetworkedPlayerState"
Cohesion: 0.24
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 22 - "PassengerActionIntent"
Cohesion: 0.25
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.11
Nodes (7): BoxCollider, Collider, GameObject, Object, Test, Vector3, Story510LaneGraphAndRoutedTrafficTests

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.07
Nodes (13): NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed, CharacterController (+5 more)

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.12
Nodes (12): Difficulty, MatchSettings, Difficulty, Color, Component, GameObject, Image, Scene (+4 more)

### Community 27 - "CharacterOption"
Cohesion: 0.67
Nodes (3): CharacterOption, Primary, Secondary

### Community 28 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.24
Nodes (5): Component, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.13
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "VehicleDriveIntent"
Cohesion: 0.13
Nodes (6): RpcParams, VehicleDriveIntent, BrakeReverse, IsIdle, Steer, Throttle

### Community 32 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (10): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+2 more)

### Community 33 - "NetworkPlayerRegistry"
Cohesion: 0.29
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.14
Nodes (9): Action, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController (+1 more)

### Community 38 - "PassengerActionCatalog"
Cohesion: 0.33
Nodes (4): List, PassengerActionCatalog, Count, Version

### Community 39 - ".NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId"
Cohesion: 0.33
Nodes (3): Collider, Renderer, Type

### Community 40 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.22
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 45 - "RunCompositionRoot"
Cohesion: 0.40
Nodes (4): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.15
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 48 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 53 - "NetworkedAIVehicleState"
Cohesion: 0.38
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.09
Nodes (18): AiVehicleFixture, IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee (+10 more)

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 59 - "Lobby Join Service"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 62 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.18
Nodes (9): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority (+1 more)

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.15
Nodes (8): GameObject, List, NetworkObject, Object, Test, Vector3, Story55NetworkedAiRageTargetingTests, IntentFixture

### Community 67 - "NetworkedPassengerActionIntent"
Cohesion: 0.13
Nodes (13): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, Action, PassengerActionDebugView (+5 more)

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.10
Nodes (14): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+6 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.08
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - "Story58EscapeMenuTests"
Cohesion: 0.05
Nodes (25): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, Canvas (+17 more)

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.05
Nodes (29): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+21 more)

### Community 88 - "TrafficSettingsDef"
Cohesion: 0.08
Nodes (17): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation (+9 more)

### Community 90 - ".SelectWeightedSuccessor"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - "CharacterCatalog"
Cohesion: 0.19
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.07
Nodes (24): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+16 more)

### Community 100 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 107 - "MainMenuScreen"
Cohesion: 0.18
Nodes (6): Button, Color, GameObject, TMP_Text, MainMenuScreen, CharacterOption

### Community 108 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 110 - "NetworkedRageState"
Cohesion: 0.22
Nodes (9): NetworkVariable, NetworkedRageState, CurrentDisposition, IntentFixture, Intent, Target, IEnumerator, UnityTest (+1 more)

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 113 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 120 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 128 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 135 - "PlayerMode"
Cohesion: 0.22
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 141 - "OnFootMovementIntent"
Cohesion: 0.21
Nodes (7): Vector2, Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 146 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.25
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.15
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.13
Nodes (13): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+5 more)

### Community 222 - "LobbyRosterSnapshot"
Cohesion: 0.10
Nodes (12): Difficulty, LobbyRosterSnapshot, Difficulty, HasLobby, Members, OwnerId, RunLaunchRequested, LobbyRosterService (+4 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (20): DriverModel, DriverProfile, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold, MaxAcceleration (+12 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.11
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 266 - "NetworkedPlayerSpawnService"
Cohesion: 0.15
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "PassengerActionDef"
Cohesion: 0.11
Nodes (18): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+10 more)

### Community 480 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.23
Nodes (7): Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.11
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, Rigidbody (+4 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.16
Nodes (7): List, NetworkObject, Object, TearDown, Test, TestCase, Story41RageStateModuleAndDefinitionsTests

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 645 - "RageSandboxAutoStart"
Cohesion: 0.22
Nodes (7): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, Transform, RageSandboxAutoStart

### Community 647 - "LobbyRoomService"
Cohesion: 0.10
Nodes (13): Task, ISteamLobbyPlatform, Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus (+5 more)

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 697 - "LobbyShellScreen"
Cohesion: 0.20
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 722 - "RoadRage.Shared.Domain"
Cohesion: 0.06
Nodes (29): VehicleDamageType, Brake, Engine, Wheel, RoadRage.App.Services, RoadRage.DevTools, RoadRage.Features.Players, RoadRage.App (+21 more)

### Community 736 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, Image, TMP_Text, LobbyPlayerSlotView

### Community 780 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.23
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 798 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.31
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 824 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.23
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 858 - "MonoBehaviour"
Cohesion: 0.13
Nodes (9): TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage, ContextMenu (+1 more)

### Community 861 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 865 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 873 - "PassengerActionVerdictCode"
Cohesion: 0.09
Nodes (21): Rpc, RpcParams, PassengerActionValidation, PassengerActionVerdict, Accepted, PassengerActionVerdictCode, Accepted, ActorMismatch (+13 more)

### Community 874 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **342 isolated node(s):** `Deferred Work`, `Design Paradigm`, `AD-1 — BMAD est la source de verite unique [ADOPTED]`, `AD-2 — Graphify n'indexe que le code applicatif`, `AD-3 — Un symbole nomme est prouve avant d'etre interroge` (+337 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 743 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **13 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `NetworkedVehicleDamageVfxController`, `LocalOnFootController`, `NetworkedPlayerState`, `Story16Epic1PlayableCheckpointTests`, `NetworkedVehicleDriverController`, `.RefreshHudFromLocalNetworkedPlayerState`, `.ApplyRageTargetLock`, `PassengerActionCatalog`, `Story26InGameHudTests`, `.HandleLifecycleChanged`, `RunCompositionRoot`, `NetworkedAIVehicleState`, `Story43PassengerActionOneChangesRageTests`, `Story55NetworkedAiRageTargetingTests`, `NetworkedPassengerActionIntent`, `.RefreshVehicleDamageHud`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `RoadRage.Shared.Domain`, `Story56RageRoadEventTriggerTests`, `MonoBehaviour`, `CharacterCatalog`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `AIVehicleBehaviorDebugView`, `NetworkedRageState`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`?**
  _High betweenness centrality (0.204) - this node is a cross-community bridge._
- **Why does `LobbyFlowController` connect `LobbyFlowController` to `Story25NetworkedPlayerSpawnTests`, `LobbyRosterScreen`, `Difficulty`, `LobbyRoomService`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `OnlineServicesBootstrapService`, `RoadRage.Shared.Domain`, `LobbyShellScreen`, `MonoBehaviour`, `Story12LobbyShellPlayModeTests`, `CharacterCatalog`, `Story12LobbyShellTests`, `Lobby Join Service`, `Story16Epic1PlayableCheckpointTests`, `LobbyCodeClipboard`, `LobbyRosterSnapshot`?**
  _High betweenness centrality (0.087) - this node is a cross-community bridge._
- **Why does `RoadRage.Shared.Domain` connect `RoadRage.Shared.Domain` to `Difficulty`, `PlayerMode`, `PassengerActionVerdictCode`, `RunCheckpointHudScreen`, `MonoBehaviour`, `Story56RageRoadEventTriggerTests`, `Story27PlayerLifecycleTests`, `FacepunchSteamLobbyPlatform`, `PassengerActionDef`, `LobbyShellScreen`, `Story12LobbyShellTests`, `LobbyCodeClipboard`, `LobbyRosterSnapshot`, `Story54RageDrivenAiBehaviorStatesTests`?**
  _High betweenness centrality (0.087) - this node is a cross-community bridge._
- **What connects `Deferred Work`, `Design Paradigm`, `AD-1 — BMAD est la source de verite unique [ADOPTED]` to the rest of the system?**
  _342 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10676532769556026 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.13911290322580644 - nodes in this community are weakly interconnected._