# Graph Report - RRS  (2026-09-18)

## Corpus Check
- 159 files · ~120,423 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2820 nodes · 6762 edges · 120 communities (111 shown, 7 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 372 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5af99b67`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- GreyboxAssetSeedMetadata
- Difficulty
- NetworkedRageState
- Story11MainMenuLaunchTests
- Story510LaneGraphAndRoutedTrafficTests
- NetworkedVehicleState
- NetworkedPlayerReviveIntent
- PortalTrafficSpawner
- .TrySpawnSelectedProfile
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyFlowController
- LocalOnFootController
- UserNotice
- NpcReactionEffect
- MenuCharacterPreview
- NetworkedVehicleSeatIntent
- NetworkedPlayerState
- PassengerActionIntent
- .RequestHonk
- .WithMvpRun
- Story27PlayerLifecycleTests
- Story12LobbyShellTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- VehicleDriveIntent
- CharacterOption
- .ResolveExitLocalOffset
- .OnDrag
- NetworkedVehicleDriverController
- LobbyRosterScreen
- LobbyRoomService
- LobbyJoinOutcome
- Story26InGameHudTests
- RunFlowController
- LobbyCreateOutcome
- FakeSteamLobbyPlatform
- Story32DriverControlAndLocalCameraTests
- NetworkedAIVehicleState
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- Story54RageDrivenAiBehaviorStatesTests
- FakeSteamLobbyPlatform
- Lobby Join Service
- HostOwnedNetworkStateBehaviour
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- MainMenuProfileFlowController
- NetworkedPassengerActionIntent
- Story58EscapeMenuTests
- RoadRageBootstrap
- .NewFixture
- RageTuningDef
- Story15EmptyMapEntryTests
- NetworkedRunSessionMonitor
- PassengerActionDef
- Story57AiTrafficClientPresentationTests
- Story56RageRoadEventTriggerTests
- MainMenuFlowController
- TrafficSettingsDef
- .SelectWeightedSuccessor
- DefinitionId
- CharacterCatalog
- RoadRage Scaffold Structure Tests
- LocalVehicleCameraRig
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- MainMenuScreen
- AIVehicleBehaviorDebugView
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- Private Room Play Mode Tests
- NetworkedPlayerLifecycleIntent
- Netcode/Steamworks Smoke Tests
- NetworkedVehicleRecoveryIntent
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FacepunchSteamPlatform
- OnFootMovementIntent
- .HandleLifecycleChanged
- .EnsureNetworkManager
- Story52BasicAiRouteFollowingAndRecoveryTests
- Story51NpcRageFearFoundationTests
- PlayerMode
- NetworkedPassengerActionIncidentState
- NetworkedPlayerPresentation
- .FindRecursive
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- NetworkedPlayerSpawnService
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- Story42PassengerActionFrameworkTests
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- LocalVoidRespawnController
- RageSandboxAutoStart
- LobbyRosterSnapshot
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
8. `RoadRage.Features.Vehicles` - 49 edges
9. `LaneGraph` - 49 edges
10. `NetworkedAIVehicleDriverController` - 49 edges

## Surprising Connections (you probably didn't know these)
- `RoadRageBootstrap` --references--> `ISteamIdentitySource`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/ISteamIdentitySource.cs
- `RoadRageBootstrap` --references--> `LobbyJoinService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyJoinService.cs
- `RoadRageBootstrap` --references--> `LobbyRoomService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyRoomService.cs
- `RoadRageBootstrap` --references--> `LobbyRosterService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyRosterService.cs
- `RoadRageBootstrap` --references--> `OnlineServicesBootstrapService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/OnlineServicesBootstrapService.cs

## Import Cycles
- None detected.

## Communities (120 total, 7 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.11
Nodes (13): ArgumentException, PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component, List (+5 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.06
Nodes (26): ApprovalResult, NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry, Collider (+18 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.06
Nodes (28): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+20 more)

### Community 4 - "Difficulty"
Cohesion: 0.22
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 5 - "NetworkedRageState"
Cohesion: 0.10
Nodes (16): NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm (+8 more)

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.18
Nodes (9): Canvas, Component, EventSystem, InputSystemUIInputModule, Scene, SerializeField, Test, Story11MainMenuLaunchTests (+1 more)

### Community 7 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.11
Nodes (9): Action, BoxCollider, Collider, GameObject, Object, TearDown, Test, Vector3 (+1 more)

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.15
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 10 - "PortalTrafficSpawner"
Cohesion: 0.18
Nodes (8): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation

### Community 11 - ".TrySpawnSelectedProfile"
Cohesion: 0.14
Nodes (5): Collider, GameObject, Quaternion, Transform, Vector3

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform, StringBuilder

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.06
Nodes (27): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+19 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.09
Nodes (15): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, AssemblyDefinition, Component (+7 more)

### Community 15 - "LobbyFlowController"
Cohesion: 0.15
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.12
Nodes (14): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector3, LocalOnFootController, IsDowned (+6 more)

### Community 17 - "UserNotice"
Cohesion: 0.18
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 18 - "NpcReactionEffect"
Cohesion: 0.18
Nodes (8): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Test, TestCase

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 21 - "NetworkedPlayerState"
Cohesion: 0.39
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 22 - "PassengerActionIntent"
Cohesion: 0.25
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 24 - ".WithMvpRun"
Cohesion: 0.23
Nodes (3): Renderer, Scene, Transform

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.07
Nodes (13): NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed, CharacterController (+5 more)

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.12
Nodes (12): Difficulty, MatchSettings, Difficulty, Color, Component, GameObject, Image, Scene (+4 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "VehicleDriveIntent"
Cohesion: 0.13
Nodes (6): RpcParams, VehicleDriveIntent, BrakeReverse, IsIdle, Steer, Throttle

### Community 31 - "CharacterOption"
Cohesion: 0.67
Nodes (3): CharacterOption, Primary, Secondary

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.14
Nodes (9): Action, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController (+1 more)

### Community 39 - "LobbyRoomService"
Cohesion: 0.13
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "RunFlowController"
Cohesion: 0.09
Nodes (4): Camera, HashSet, RunFlowController, ActiveLocalPlayer

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.14
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 53 - "NetworkedAIVehicleState"
Cohesion: 0.36
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.15
Nodes (8): AiVehicleFixture, List, NetworkObject, Object, TearDown, Test, AiVehicleFixture, Story54RageDrivenAiBehaviorStatesTests

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
Cohesion: 0.13
Nodes (9): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, Story55NetworkedAiRageTargetingTests (+1 more)

### Community 66 - "MainMenuProfileFlowController"
Cohesion: 0.33
Nodes (3): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex

### Community 67 - "NetworkedPassengerActionIntent"
Cohesion: 0.11
Nodes (15): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, List, PassengerActionCatalog, Count (+7 more)

### Community 69 - "Story58EscapeMenuTests"
Cohesion: 0.05
Nodes (25): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, Canvas (+17 more)

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.10
Nodes (14): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+6 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.07
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.12
Nodes (12): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot, AssemblyDefinition, GameObject, Object, Scene (+4 more)

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "PassengerActionDef"
Cohesion: 0.16
Nodes (12): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+4 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.05
Nodes (29): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+21 more)

### Community 88 - "TrafficSettingsDef"
Cohesion: 0.15
Nodes (9): TrafficSettingsDef, ConnectorJoinDistance, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxTargetPopulation, MinTargetPopulation, PortalClearanceRadius (+1 more)

### Community 90 - ".SelectWeightedSuccessor"
Cohesion: 0.18
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 93 - "DefinitionId"
Cohesion: 0.13
Nodes (15): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+7 more)

### Community 94 - "CharacterCatalog"
Cohesion: 0.09
Nodes (18): List, CharacterCatalog, Count, Color, GameObject, CharacterDef, DisplayName, Id (+10 more)

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.08
Nodes (24): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+16 more)

### Community 100 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 107 - "MainMenuScreen"
Cohesion: 0.18
Nodes (6): Button, Color, GameObject, TMP_Text, MainMenuScreen, CharacterOption

### Community 108 - "AIVehicleBehaviorDebugView"
Cohesion: 0.25
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 110 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

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

### Community 131 - "OnFootMovementIntent"
Cohesion: 0.21
Nodes (7): Vector2, Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 146 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.23
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 150 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.27
Nodes (4): List, Object, TearDown, Story51NpcRageFearFoundationTests

### Community 153 - "PlayerMode"
Cohesion: 0.22
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 180 - "NetworkedPassengerActionIncidentState"
Cohesion: 0.21
Nodes (7): NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, IEnumerator, UnityTearDown, UnityTest, Story44PassengerActionTwoMvpRunPlayModeTests

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.14
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.13
Nodes (13): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+5 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (20): DriverModel, DriverProfile, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold, MaxAcceleration (+12 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.11
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 266 - "NetworkedPlayerSpawnService"
Cohesion: 0.17
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "Story42PassengerActionFrameworkTests"
Cohesion: 0.19
Nodes (7): GameObject, List, NetworkObject, Object, TearDown, Test, Story42PassengerActionFrameworkTests

### Community 480 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.23
Nodes (7): Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
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

### Community 647 - "LobbyRosterSnapshot"
Cohesion: 0.08
Nodes (15): Difficulty, Task, ISteamLobbyPlatform, LobbyRosterSnapshot, Difficulty, HasLobby, Members, OwnerId (+7 more)

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
Cohesion: 0.21
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
Cohesion: 0.22
Nodes (3): Vector3, NetworkedVehicleSeatService, Instance

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
Cohesion: 0.16
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 873 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 874 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **321 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+316 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 720 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `GreyboxAssetSeedMetadata`, `.HandleLifecycleChanged`, `NetworkedVehicleState`, `.TrySpawnSelectedProfile`, `RunCheckpointHudScreen`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `NetworkedVehicleDamageVfxController`, `LocalOnFootController`, `NetworkedPlayerState`, `NetworkedVehicleDriverController`, `Story26InGameHudTests`, `NetworkedAIVehicleState`, `Story43PassengerActionOneChangesRageTests`, `Story55NetworkedAiRageTargetingTests`, `NetworkedPassengerActionIntent`, `.ApplySecondaryVehicleCollisionDamage`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `RoadRage.Shared.Domain`, `Story56RageRoadEventTriggerTests`, `MonoBehaviour`, `CharacterCatalog`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `AIVehicleBehaviorDebugView`, `.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`?**
  _High betweenness centrality (0.204) - this node is a cross-community bridge._
- **Why does `LobbyFlowController` connect `LobbyFlowController` to `Story25NetworkedPlayerSpawnTests`, `LobbyRosterScreen`, `Difficulty`, `GreyboxAssetSeedMetadata`, `LobbyRoomService`, `LobbyRosterSnapshot`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `OnlineServicesBootstrapService`, `RoadRage.Shared.Domain`, `Story12LobbyShellPlayModeTests`, `LobbyShellScreen`, `Story12LobbyShellTests`, `Lobby Join Service`, `MonoBehaviour`, `CharacterCatalog`?**
  _High betweenness centrality (0.077) - this node is a cross-community bridge._
- **Why does `RunCheckpointHudScreen` connect `RunCheckpointHudScreen` to `LocalVoidRespawnController`, `Story25NetworkedPlayerSpawnTests`, `NetworkedPassengerActionIntent`, `GreyboxAssetSeedMetadata`, `Story58EscapeMenuTests`, `.HandleLifecycleChanged`, `NetworkedPlayerSpawnService`, `RunFlowController`, `.TrySpawnSelectedProfile`, `Story26InGameHudTests`, `NetworkedVehicleDamageVfxController`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RoadRage.Shared.Domain`, `Story56RageRoadEventTriggerTests`, `NetworkedVehicleSeatService`, `MonoBehaviour`, `Story27PlayerLifecycleTests`?**
  _High betweenness centrality (0.071) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _321 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10853658536585366 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.057902973395931145 - nodes in this community are weakly interconnected._