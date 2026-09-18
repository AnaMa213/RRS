# Graph Report - RRS  (2026-09-18)

## Corpus Check
- 168 files · ~132,772 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2972 nodes · 7072 edges · 138 communities (128 shown, 8 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 380 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `7718e7e9`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- Story14GreyboxAssetSeedTests
- Difficulty
- RageDisposition
- Story11MainMenuLaunchTests
- Story510LaneGraphAndRoutedTrafficTests
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- PortalTrafficSpawner
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
- VehiclePhysicsBody
- PassengerActionIntent
- RunEscapeMenuFlowController
- .WithMvpRun
- Story27PlayerLifecycleTests
- Story12LobbyShellTests
- DriverProfile
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- VehicleDriveIntent
- VehicleSuspensionModel
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- NetworkedRunState
- NetworkedVehicleDriverController
- LobbyRosterScreen
- RunEscapeMenuScreen
- RageRoadEventState
- VehicleProfile
- VehicleProfileDef
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- LobbyJoinOutcome
- Story26InGameHudTests
- .HandleLifecycleChanged
- LobbyRosterSnapshot
- DriverProfileDef
- FakeSteamLobbyPlatform
- Story32DriverControlAndLocalCameraTests
- VehicleWheel
- FakeSteamPlatform
- NetworkedPlayerLifecycleService
- FakeSteamPlatform
- LobbyRoomService
- Story16Epic1PlayableCheckpointTests
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- .TearDown
- Story54RageDrivenAiBehaviorStatesTests
- FakeSteamLobbyPlatform
- Lobby Join Service
- .TearDown
- GreyboxAssetSeedMetadata
- HostOwnedNetworkStateBehaviour
- FakeSteamLobbyPlatform
- NetworkedAIVehicleState
- DriverModel
- CharacterCatalog
- NetworkedPassengerActionIntent
- Story58EscapeMenuPlayModeTests
- Story58EscapeMenuTests
- RoadRageBootstrap
- .NewFixture
- RageTuningDef
- Story15EmptyMapEntryTests
- NetworkedPlayerReviveIntent
- NetworkedRunSessionMonitor
- NetworkedPassengerActionIncidentState
- NetworkedPlayerState
- Story57AiTrafficClientPresentationTests
- TelemetrySample
- .MainMenuLobbySceneContainsWiredComponents
- Story56RageRoadEventTriggerTests
- PlayerLifecycle
- .IsSurfaceOnlyCollision
- SeedExpectation
- TrafficSettingsDef
- PlayerProfile
- DefinitionId
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
- .EnsureNetworkManager
- .SelectWeightedSuccessor
- .NewTuning
- PlayerMode
- NetworkedPlayerPresentation
- LaneGraph
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
- LobbyRosterService
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
- ReactionChannel
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- PassengerActionVerdictCode
- NetworkedRageState
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RunFlowController` - 118 edges
2. `NetworkedVehicleState` - 79 edges
3. `Story510LaneGraphAndRoutedTrafficTests` - 70 edges
4. `RunCheckpointHudScreen` - 69 edges
5. `NetworkedVehicleDriverController` - 64 edges
6. `RoadRage.Shared.Domain` - 63 edges
7. `RoadRage.Features.Vehicles` - 58 edges
8. `RoadRage.Features.Players` - 50 edges
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

## Communities (138 total, 8 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.10
Nodes (15): ArgumentException, PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component, List (+7 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.06
Nodes (26): ApprovalResult, NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry, Collider (+18 more)

### Community 3 - "Story14GreyboxAssetSeedTests"
Cohesion: 0.17
Nodes (10): Collider, GameObject, NetworkObject, Renderer, Test, Transform, Story14GreyboxAssetSeedTests, Bounds (+2 more)

### Community 4 - "Difficulty"
Cohesion: 0.18
Nodes (6): Difficulty, Difficulty, Difficulty, Easy, Hard, Normal

### Community 5 - "RageDisposition"
Cohesion: 0.20
Nodes (7): RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated, Ram

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.22
Nodes (5): UserNoticeChannel, LastNotice, Scene, Test, Story11MainMenuLaunchTests

### Community 7 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.12
Nodes (8): BoxCollider, Collider, GameObject, Object, TearDown, Test, Vector3, Story510LaneGraphAndRoutedTrafficTests

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.13
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "PortalTrafficSpawner"
Cohesion: 0.18
Nodes (8): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation

### Community 11 - "RunFlowController"
Cohesion: 0.08
Nodes (7): Camera, Collider, GameObject, HashSet, Transform, RunFlowController, ActiveLocalPlayer

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.16
Nodes (6): GameObject, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform, StringBuilder

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.07
Nodes (24): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+16 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.09
Nodes (15): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, AssemblyDefinition, Component (+7 more)

### Community 15 - "LobbyFlowController"
Cohesion: 0.13
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.12
Nodes (15): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+7 more)

### Community 17 - "UserNotice"
Cohesion: 0.27
Nodes (7): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.13
Nodes (11): Vector3, VehiclePhysicsTelemetryView, Rigidbody, TelemetrySample, VehiclePhysicsBody, HasProfile, Profile, WheelCount (+3 more)

### Community 22 - "PassengerActionIntent"
Cohesion: 0.25
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 23 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 24 - ".WithMvpRun"
Cohesion: 0.20
Nodes (5): Action, List, Renderer, Scene, Transform

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.10
Nodes (6): CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests, IEnumerable

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.12
Nodes (12): Difficulty, MatchSettings, Difficulty, Color, Component, GameObject, Image, Scene (+4 more)

### Community 27 - "DriverProfile"
Cohesion: 0.14
Nodes (12): DriverProfile, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold, MaxAcceleration, MinimumGap (+4 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.15
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (5): VehicleDriveIntent, BrakeReverse, IsIdle, Steer, Throttle

### Community 31 - "VehicleSuspensionModel"
Cohesion: 0.13
Nodes (8): Vector3, Vector3, VehicleSuspensionModel, WheelState, Compression, ContactPoint, Grounded, HubPosition

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.22
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Vector3 (+2 more)

### Community 33 - "NetworkedRunState"
Cohesion: 0.20
Nodes (6): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.14
Nodes (9): Collider, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc, RpcParams, Transform (+1 more)

### Community 36 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 37 - "RageRoadEventState"
Cohesion: 0.13
Nodes (9): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, RageRoadEventState, Confrontation, Idle, Resolved, RewardGranted (+1 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.10
Nodes (18): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, CenterOfMass, Damper, GroundMask (+10 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.36
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 40 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.08
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 44 - "LobbyRosterSnapshot"
Cohesion: 0.15
Nodes (17): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+9 more)

### Community 45 - "DriverProfileDef"
Cohesion: 0.24
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.15
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 48 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 49 - "FakeSteamPlatform"
Cohesion: 0.25
Nodes (4): ArgumentNullException, FakeSteamPlatform, IsLoggedOn, IsValid

### Community 51 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 52 - "LobbyRoomService"
Cohesion: 0.16
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 53 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.24
Nodes (5): Component, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.16
Nodes (8): AiVehicleFixture, List, NetworkObject, Object, TearDown, Test, AiVehicleFixture, Story54RageDrivenAiBehaviorStatesTests

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 59 - "Lobby Join Service"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 61 - "GreyboxAssetSeedMetadata"
Cohesion: 0.16
Nodes (10): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+2 more)

### Community 62 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.22
Nodes (7): NetworkVariable, NetworkedBossState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority, NetworkBehaviour

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 64 - "NetworkedAIVehicleState"
Cohesion: 0.10
Nodes (17): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, GameObject, List, NetworkObject (+9 more)

### Community 66 - "CharacterCatalog"
Cohesion: 0.12
Nodes (13): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PlayerProfileBootstrapService, PlayerProfileResolution (+5 more)

### Community 67 - "NetworkedPassengerActionIntent"
Cohesion: 0.12
Nodes (15): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, List, PassengerActionCatalog, Count (+7 more)

### Community 68 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 69 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.10
Nodes (14): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+6 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.09
Nodes (16): List, RageTuningCatalog, Count, RageTuningDef, FearSensitivity, HonkChannel, HonkMagnitude, HonkRange (+8 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.12
Nodes (12): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot, AssemblyDefinition, GameObject, Object, Scene (+4 more)

### Community 75 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "NetworkedPassengerActionIncidentState"
Cohesion: 0.25
Nodes (7): NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, IEnumerator, UnityTearDown, UnityTest, Story44PassengerActionTwoMvpRunPlayModeTests

### Community 78 - "NetworkedPlayerState"
Cohesion: 0.39
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - "TelemetrySample"
Cohesion: 0.22
Nodes (7): TelemetrySample, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees, Speed

### Community 81 - ".MainMenuLobbySceneContainsWiredComponents"
Cohesion: 0.25
Nodes (6): Canvas, Component, EventSystem, InputSystemUIInputModule, SerializeField, CanvasScaler

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.16
Nodes (7): GameObject, List, NetworkObject, Object, Test, TMP_Text, Story56RageRoadEventTriggerTests

### Community 84 - "PlayerLifecycle"
Cohesion: 0.33
Nodes (5): PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 86 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 88 - "TrafficSettingsDef"
Cohesion: 0.16
Nodes (9): TrafficSettingsDef, ConnectorJoinDistance, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxTargetPopulation, MinTargetPopulation, PortalClearanceRadius (+1 more)

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - "DefinitionId"
Cohesion: 0.16
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

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
Cohesion: 0.09
Nodes (13): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+5 more)

### Community 108 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
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
Cohesion: 0.15
Nodes (6): Vector3, AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 128 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 131 - "OnFootMovementIntent"
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 146 - ".SelectWeightedSuccessor"
Cohesion: 0.11
Nodes (6): IReadOnlyList, Vector3, LaneGraphRouting, Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 153 - "PlayerMode"
Cohesion: 0.19
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.15
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.17
Nodes (3): GameObject, Test, Story59ParameterizedDriverModelTests

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.12
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 266 - "NetworkedPlayerSpawnService"
Cohesion: 0.17
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "PassengerActionDef"
Cohesion: 0.10
Nodes (19): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+11 more)

### Community 480 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (15): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, Rigidbody (+7 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.14
Nodes (10): RageThreshold, Disposition, MinValue, List, NetworkObject, Object, TearDown, Test (+2 more)

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 645 - "RageSandboxAutoStart"
Cohesion: 0.26
Nodes (6): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, RageSandboxAutoStart

### Community 647 - "LobbyRosterService"
Cohesion: 0.11
Nodes (8): Difficulty, Task, ISteamLobbyPlatform, LobbyRosterService, AllMembersReady, Current, IsSynchronized, LocalReady

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 697 - "LobbyShellScreen"
Cohesion: 0.18
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 722 - "RoadRage.Shared.Domain"
Cohesion: 0.05
Nodes (31): NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel, RoadRage.App.Services, RoadRage.DevTools (+23 more)

### Community 736 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, Image, TMP_Text, LobbyPlayerSlotView

### Community 780 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.23
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 798 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 824 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.20
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 858 - "ReactionChannel"
Cohesion: 0.15
Nodes (8): TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage, ContextMenu

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
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 874 - "NetworkedRageState"
Cohesion: 0.12
Nodes (12): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner, Quaternion, Vector3 (+4 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **356 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+351 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 769 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **8 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `NetworkedVehicleDamageVfxController`, `LocalOnFootController`, `NetworkedVehicleDriverController`, `Story26InGameHudTests`, `.HandleLifecycleChanged`, `Story16Epic1PlayableCheckpointTests`, `Story43PassengerActionOneChangesRageTests`, `NetworkedAIVehicleState`, `CharacterCatalog`, `NetworkedPassengerActionIntent`, `.RefreshVehicleDamageHud`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `NetworkedPlayerState`, `RoadRage.Shared.Domain`, `.RefreshHudFromLocalNetworkedPlayerState`, `Story56RageRoadEventTriggerTests`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `NetworkedRageState`, `MainMenuScreen`, `AIVehicleBehaviorDebugView`, `.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`?**
  _High betweenness centrality (0.203) - this node is a cross-community bridge._
- **Why does `LobbyFlowController` connect `LobbyFlowController` to `CharacterCatalog`, `LobbyRosterScreen`, `Difficulty`, `Story25NetworkedPlayerSpawnTests`, `LobbyRosterService`, `MainMenuScreen`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `OnlineServicesBootstrapService`, `RoadRage.Shared.Domain`, `LobbyRoomService`, `Story16Epic1PlayableCheckpointTests`, `Story12LobbyShellPlayModeTests`, `LobbyShellScreen`, `Story12LobbyShellTests`, `Lobby Join Service`, `DefinitionId`?**
  _High betweenness centrality (0.074) - this node is a cross-community bridge._
- **Why does `RunCheckpointHudScreen` connect `RunCheckpointHudScreen` to `LocalVoidRespawnController`, `Story25NetworkedPlayerSpawnTests`, `NetworkedPlayerSpawnService`, `RunFlowController`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `NetworkedVehicleDamageVfxController`, `Story27PlayerLifecycleTests`, `NetworkedRunState`, `RunEscapeMenuScreen`, `.HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels`, `Story26InGameHudTests`, `.HandleLifecycleChanged`, `Story16Epic1PlayableCheckpointTests`, `NetworkedPassengerActionIntent`, `.RefreshVehicleDamageHud`, `.RefreshHudFromLocalNetworkedPlayerState`, `Story56RageRoadEventTriggerTests`, `NetworkedVehicleSeatService`, `MainMenuScreen`?**
  _High betweenness centrality (0.070) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _356 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09898989898989899 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.05875251509054326 - nodes in this community are weakly interconnected._