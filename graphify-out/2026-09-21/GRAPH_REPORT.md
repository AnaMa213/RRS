# Graph Report - RRS  (2026-09-20)

## Corpus Check
- 187 files · ~183,152 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3529 nodes · 8323 edges · 174 communities (148 shown, 24 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 418 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `03afb6af`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- Story14GreyboxAssetSeedTests
- Story516LobbyConfigurableTrafficSettingsTests
- Story512TireForcesAndSteeringTests
- Story11MainMenuLaunchTests
- .WithMvpRun
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- FakeSteamLobbyPlatform
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyFlowController
- LocalOnFootController
- UserNotice
- NpcReactionEffect
- MenuCharacterPreview
- NetworkedVehicleSeatIntent
- VehiclePhysicsBody
- .Create
- Story512TireForcesAndSteeringPlayModeTests
- Story510LaneGraphAndRoutedTrafficTests
- Story27PlayerLifecycleTests
- Story12LobbyShellTests
- Story513ArcadeAssistsAndUnevenGroundTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- RoadRage.Features.Vehicles
- VehicleSuspensionModel
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- Story513ArcadeAssistsAndUnevenGroundPlayModeTests
- NetworkedVehicleDriverController
- LobbyRosterScreen
- PhysicsHarness
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- VehicleProfile
- VehicleProfileDef
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- NetworkedPassengerActionIntent
- LobbyCreateOutcome
- NetworkedRunState
- FakeSteamLobbyPlatform
- PortalTrafficSpawner
- Story514AiDrivesByIntentTests
- .Awake
- RoadRage.Features.Players
- .SelectWeightedSuccessor
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- VehicleWheel
- PassengerActionDef
- Story12LobbyShellPlayModeTests
- RoadRage.Shared.Domain
- NetworkedRageState
- FakeSteamLobbyPlatform
- LobbyJoinService
- NetworkedAIVehicleState
- RunEscapeMenuFlowController
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- Story28Epic2OnlinePlayableCheckpointTests
- CharacterCatalog
- LobbyRoomService
- Story58EscapeMenuTests
- RoadRageBootstrap
- .NewFixture
- .Inspect
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- PassengerActionIntent
- RunEscapeMenuScreen
- Story57AiTrafficClientPresentationTests
- LaneGraph
- RoadRageScaffoldTests.cs
- TrafficSettingsDef
- Story56RageRoadEventTriggerTests
- RoadRage.Shared.Input
- Story33SeatEntryExitAndPassengerPresenceTests
- Story51NpcRageFearFoundationTests
- .RequestHonk
- PlayerProfileResolution
- .EnsureVehicleSandboxSeatHarness
- .ComputeCollisionDamage
- NetworkedPlayerReviveIntent
- NetworkedPlayerLifecycleIntent
- DefinitionId
- Story36Epic3DrivingPlayableCheckpointTests
- .Run
- NetworkedPlayerState
- RoadRage Scaffold Structure Tests
- Story58EscapeMenuPlayModeTests
- LocalVehicleCameraRig
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- NetworkPlayerRegistry
- NetworkedPlayerSpawnService
- Story16Epic1PlayableCheckpointTests
- .FixedUpdate
- .UpdateSteeringState
- MainMenuScreen
- FakeSteamLobbyPlatform
- .Find
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- Private Room Play Mode Tests
- GreyboxAssetSeedMetadata
- .RecoverVehicle
- Netcode/Steamworks Smoke Tests
- VehicleDriveIntent
- ISteamPlatform
- FakeSteamPlatform
- .Append
- NetworkedVehicleRecoveryIntent
- .TearDown
- Canvas
- NetworkedPassengerActionIncidentState
- CanvasScaler
- .HandleLifecycleChanged
- OnlineServicesStatus
- HashSet
- NetworkPrefabsList
- FacepunchSteamPlatform
- FakeSteamPlatform
- RunCompositionRoot
- OnFootMovementIntent
- Story516ForceAssetRefresh
- CharacterController
- SeedExpectation
- IntentFixture
- Difficulty
- Difficulty
- Difficulty
- Difficulty
- NetworkObjectReference
- NetworkVariable
- Color
- PointerEventData
- .EnsureNetworkManager
- Renderer
- Story52BasicAiRouteFollowingAndRecoveryTests
- SerializedProperty
- FakeSteamLobbyPlatform
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
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
- MonoBehaviour
- LaneNode
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedVehicleSeatService
- ReactionChannel
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- PassengerActionVerdictCode
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RunFlowController` - 118 edges
2. `NetworkedVehicleState` - 80 edges
3. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
4. `RoadRage.Features.Vehicles` - 73 edges
5. `RunCheckpointHudScreen` - 69 edges
6. `RoadRage.Shared.Domain` - 66 edges
7. `NetworkedVehicleDriverController` - 65 edges
8. `LobbyFlowController` - 63 edges
9. `VehicleProfile` - 56 edges
10. `Story516LobbyConfigurableTrafficSettingsTests` - 55 edges

## Surprising Connections (you probably didn't know these)
- `LobbyFlowController` --references--> `MatchSettings`  [EXTRACTED]
  Assets/RoadRage/App/Lobby/LobbyFlowController.cs → Assets/RoadRage/Features/Lobby/MatchSettings.cs
- `LobbyFlowController` --references--> `LobbyRosterService`  [EXTRACTED]
  Assets/RoadRage/App/Lobby/LobbyFlowController.cs → Assets/RoadRage/Features/Online/LobbyRosterService.cs
- `LobbyFlowController` --references--> `LobbyRosterScreen`  [EXTRACTED]
  Assets/RoadRage/App/Lobby/LobbyFlowController.cs → Assets/RoadRage/Features/UI/LobbyRosterScreen.cs
- `LobbyFlowController` --references--> `TrafficSettingsDef`  [EXTRACTED]
  Assets/RoadRage/App/Lobby/LobbyFlowController.cs → Assets/RoadRage/Features/Vehicles/TrafficSettingsDef.cs
- `PortalTrafficSpawner` --references--> `NetworkedRunState`  [EXTRACTED]
  Assets/RoadRage/App/Run/PortalTrafficSpawner.cs → Assets/RoadRage/Features/Run/NetworkedRunState.cs

## Import Cycles
- None detected.

## Communities (174 total, 24 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.09
Nodes (17): ArgumentException, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component (+9 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (11): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, ArgumentNullException, List, Object, TearDown, Test (+3 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.12
Nodes (9): NetworkPlayerConnectionPayload, Collider, GameObject, NetworkObject, Renderer, Test, TextMeshProUGUI, Type (+1 more)

### Community 3 - "Story14GreyboxAssetSeedTests"
Cohesion: 0.17
Nodes (10): Bounds, Collider, GameObject, NetworkObject, Renderer, Test, Transform, Story14GreyboxAssetSeedTests (+2 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.09
Nodes (11): Button, LobbyJoinService, LobbyRoomService, MonoBehaviour, NetworkObject, OnlineServicesBootstrapService, RoadRageBootstrap, TearDown (+3 more)

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.13
Nodes (4): GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.13
Nodes (11): UserNoticeChannel, LastNotice, Canvas, CanvasScaler, Component, EventSystem, InputSystemUIInputModule, Scene (+3 more)

### Community 7 - ".WithMvpRun"
Cohesion: 0.15
Nodes (7): Action, DriverProfileDef, Scene, Transform, ReplayTrace, NetworkedAIVehicleState, Renderer

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.10
Nodes (9): Func, NetworkVariable, Vector3, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged (+1 more)

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.11
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

### Community 11 - "RunFlowController"
Cohesion: 0.08
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.20
Nodes (6): GameObject, StringBuilder, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.12
Nodes (14): OnlineServicesBootstrapService, Status, ArgumentNullException, Exception, Test, FakeSteamPlatform, InitCallCount, InitCalledWithAppId (+6 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyFlowController"
Cohesion: 0.09
Nodes (12): LobbyJoinService, LobbyRoomService, OnlineServicesBootstrapService, RoadRageBootstrap, LobbyFlowController, Settings, CharacterCatalog, ConnectionApprovalRequest (+4 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.13
Nodes (13): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Vector2, LocalOnFootController, IsDowned, MovementEnabled (+5 more)

### Community 17 - "UserNotice"
Cohesion: 0.27
Nodes (7): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning

### Community 18 - "NpcReactionEffect"
Cohesion: 0.18
Nodes (8): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Test, TestCase

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.09
Nodes (16): Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees (+8 more)

### Community 22 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.19
Nodes (13): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+5 more)

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.10
Nodes (10): BoxCollider, Collider, GameObject, LaneNode, NetworkedAIVehicleDriverController, Object, Test, Vector3 (+2 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.07
Nodes (13): NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed, CharacterController (+5 more)

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.12
Nodes (13): MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject, Image (+5 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.07
Nodes (20): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, DriverProfileDef, GameObject (+12 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.21
Nodes (6): TMP_Text, LobbyCodeClipboard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, PointerEventData

### Community 30 - "RoadRage.Features.Vehicles"
Cohesion: 0.11
Nodes (5): RoadRage.DevTools, RoadRage.Tests.EditMode, RoadRage.App.Run, RoadRage.Features.Rage, RoadRage.Features.Vehicles

### Community 31 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.20
Nodes (13): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+5 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.18
Nodes (5): Collider, NetworkTransform, Rigidbody, Transform, NetworkedVehicleDriverController

### Community 35 - "LobbyRosterScreen"
Cohesion: 0.09
Nodes (9): Button, Difficulty, LobbyRosterEntry, DisplayName, PortraitTint, Ready, LobbyRosterScreen, Color (+1 more)

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.20
Nodes (7): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.19
Nodes (7): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.App.MainMenu, RoadRage.Shared.Presentation, RoadRage.Tests.PlayMode

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.25
Nodes (8): LobbyJoinOutcome, LobbyId, Reason, Success, FakeSteamLobbyPlatform, Task, Test, Story23JoinByCodeTests

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "NetworkedPassengerActionIntent"
Cohesion: 0.11
Nodes (16): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, List, PassengerActionCatalog, Count (+8 more)

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 45 - "NetworkedRunState"
Cohesion: 0.11
Nodes (15): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkedRunState, RageRoadEventState, Confrontation, Idle, Resolved (+7 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "PortalTrafficSpawner"
Cohesion: 0.15
Nodes (10): Collider, GameObject, IEnumerator, LaneGraph, List, NetworkedAIVehicleDriverController, NetworkObject, PortalTrafficSpawner (+2 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.20
Nodes (5): Func, GameObject, Rigidbody, Test, Story514AiDrivesByIntentTests

### Community 49 - ".Awake"
Cohesion: 0.13
Nodes (5): LobbyJoinStatus, LobbyRoomStatus, OnlineServicesStatus, PlayerProfile, UserNoticeSeverity

### Community 50 - "RoadRage.Features.Players"
Cohesion: 0.12
Nodes (4): RoadRage.Features.Players, RoadRage.Shared.Authoring, RoadRage.Features.PassengerActions, RoadRage.Shared.Definitions

### Community 51 - ".SelectWeightedSuccessor"
Cohesion: 0.24
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 52 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 53 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 54 - "PassengerActionDef"
Cohesion: 0.10
Nodes (21): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+13 more)

### Community 55 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 56 - "RoadRage.Shared.Domain"
Cohesion: 0.11
Nodes (5): SessionTrafficValue, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.Features.Online, RoadRage.Features.Lobby

### Community 57 - "NetworkedRageState"
Cohesion: 0.08
Nodes (21): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, RageDisposition, Block (+13 more)

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 59 - "LobbyJoinService"
Cohesion: 0.11
Nodes (15): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+7 more)

### Community 60 - "NetworkedAIVehicleState"
Cohesion: 0.44
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 61 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.16
Nodes (6): Vector3, AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.14
Nodes (9): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, Story55NetworkedAiRageTargetingTests (+1 more)

### Community 65 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.17
Nodes (10): ApprovalResult, FakeSteamLobbyPlatform, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+2 more)

### Community 66 - "CharacterCatalog"
Cohesion: 0.12
Nodes (14): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+6 more)

### Community 68 - "LobbyRoomService"
Cohesion: 0.10
Nodes (13): Task, ISteamLobbyPlatform, Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus (+5 more)

### Community 69 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.08
Nodes (17): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+9 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 72 - ".Inspect"
Cohesion: 0.24
Nodes (6): Func, List, MonoBehaviour, Rigidbody, Transform, Story513DriverHandshakeInspection

### Community 73 - "RageTuningDef"
Cohesion: 0.07
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 75 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "PassengerActionIntent"
Cohesion: 0.22
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 78 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - "LaneGraph"
Cohesion: 0.21
Nodes (7): IReadOnlyList, Bounds, LaneGraph, List, CurbTrafficMeasurement, CurbTrafficMeasurement, HashSet

### Community 81 - "RoadRageScaffoldTests.cs"
Cohesion: 0.15
Nodes (7): VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking, RoadRage.Features.Boss

### Community 82 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (13): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+5 more)

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.11
Nodes (11): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, GameObject, List, NetworkObject, Object, TearDown (+3 more)

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 86 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.27
Nodes (4): List, Object, TearDown, Story51NpcRageFearFoundationTests

### Community 88 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 89 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 91 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 92 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 93 - "DefinitionId"
Cohesion: 0.14
Nodes (14): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+6 more)

### Community 94 - "Story36Epic3DrivingPlayableCheckpointTests"
Cohesion: 0.19
Nodes (8): AssemblyDefinition, Component, GameObject, NetworkObject, Scene, Test, AssemblyDefinition, Story36Epic3DrivingPlayableCheckpointTests

### Community 95 - ".Run"
Cohesion: 0.27
Nodes (10): Button, GameObject, Object, Scene, SerializedObject, StringBuilder, TMP_Text, Transform (+2 more)

### Community 96 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 98 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.05
Nodes (35): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+27 more)

### Community 100 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 101 - "NetworkPlayerRegistry"
Cohesion: 0.29
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 102 - "NetworkedPlayerSpawnService"
Cohesion: 0.18
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

### Community 103 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.24
Nodes (5): Component, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 104 - ".FixedUpdate"
Cohesion: 0.13
Nodes (12): TireSample, Vector2, TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce (+4 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.11
Nodes (11): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+3 more)

### Community 108 - "FakeSteamLobbyPlatform"
Cohesion: 0.13
Nodes (9): Difficulty, Exception, FakeSteamLobbyPlatform, JoinLobbyCallCount, LastJoinCode, LastLobbyId, NextOutcome, NextTask (+1 more)

### Community 110 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "GreyboxAssetSeedMetadata"
Cohesion: 0.16
Nodes (10): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+2 more)

### Community 113 - ".RecoverVehicle"
Cohesion: 0.29
Nodes (3): Action, Quaternion, Collision

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 115 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 116 - "ISteamPlatform"
Cohesion: 0.15
Nodes (6): ISteamPlatform, IsLoggedOn, IsValid, FakeSteamPlatform, IsLoggedOn, IsValid

### Community 117 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 118 - ".Append"
Cohesion: 0.23
Nodes (8): Component, StringBuilder, TMP_Text, Transform, Vector2, Story516LobbyPanelInspection, Canvas, CanvasScaler

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.12
Nodes (13): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent, NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState (+5 more)

### Community 122 - "NetworkedPassengerActionIncidentState"
Cohesion: 0.25
Nodes (7): NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, IEnumerator, UnityTearDown, UnityTest, Story44PassengerActionTwoMvpRunPlayModeTests

### Community 125 - "OnlineServicesStatus"
Cohesion: 0.29
Nodes (6): OnlineServicesStatus, InitializationFailed, NotStarted, Offline, Online, SignInFailed

### Community 128 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 129 - "FakeSteamPlatform"
Cohesion: 0.29
Nodes (4): FakeSteamPlatform, IsLoggedOn, IsValid, ISteamPlatform

### Community 130 - "RunCompositionRoot"
Cohesion: 0.40
Nodes (4): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot

### Community 131 - "OnFootMovementIntent"
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 134 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 135 - "IntentFixture"
Cohesion: 0.67
Nodes (3): IntentFixture, Intent, Target

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.24
Nodes (7): GameObject, NetworkManager, NetworkPrefabsList, NetworkManager, NetworkPrefabsList, TearDown, FacepunchTransport

### Community 146 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.25
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.13
Nodes (11): Color, HashSet, List, Quaternion, LaneGraph, EntryPortals, ExitPortals, NodeCount (+3 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (21): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+13 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.10
Nodes (10): Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None, Difficulty (+2 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "Story42PassengerActionFrameworkTests"
Cohesion: 0.14
Nodes (10): PassengerActionValidationContext, RunPhase, NotStarted, GameObject, List, NetworkObject, Object, TearDown (+2 more)

### Community 480 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.08
Nodes (13): Vector3, BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion (+5 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.16
Nodes (7): List, NetworkObject, Object, TearDown, Test, TestCase, Story41RageStateModuleAndDefinitionsTests

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.19
Nodes (7): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Quaternion, Vector3

### Community 645 - "RageSandboxAutoStart"
Cohesion: 0.22
Nodes (7): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, Transform, RageSandboxAutoStart

### Community 647 - "LobbyRosterSnapshot"
Cohesion: 0.09
Nodes (16): LobbyRosterSnapshot, AiVehicleTargetCount, HasLobby, LitterThrowerCount, Members, OwnerId, RunLaunchRequested, LobbyJoinService (+8 more)

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 697 - "LobbyShellScreen"
Cohesion: 0.12
Nodes (9): Difficulty, Button, TMP_InputField, TMP_Text, LobbyShellScreen, Difficulty, Easy, Hard (+1 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 736 - "MonoBehaviour"
Cohesion: 0.20
Nodes (6): Color, Image, TMP_Text, LobbyPlayerSlotView, DevIndestructibleVehicle, MonoBehaviour

### Community 824 - "LaneNode"
Cohesion: 0.08
Nodes (25): Collider, Component, Transform, Story513RageTargetInspection, Bounds, Collider, GameObject, List (+17 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.21
Nodes (3): Vector3, NetworkedVehicleSeatService, Instance

### Community 858 - "ReactionChannel"
Cohesion: 0.14
Nodes (8): TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage, ContextMenu

### Community 865 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 873 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **404 isolated node(s):** `Settings`, `LivePopulation`, `Difficulty`, `AiVehicleTargetCount`, `LitterThrowerCount` (+399 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 932 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **24 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `RunCompositionRoot`, `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `LocalOnFootController`, `NetworkedVehicleDriverController`, `Story26InGameHudTests`, `NetworkedPassengerActionIntent`, `PassengerActionDef`, `NetworkedAIVehicleState`, `Story55NetworkedAiRageTargetingTests`, `CharacterCatalog`, `.Update`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `AIVehicleBehaviorDebugView`, `Story56RageRoadEventTriggerTests`, `RoadRage.Shared.Input`, `Story36Epic3DrivingPlayableCheckpointTests`, `NetworkedPlayerState`, `MonoBehaviour`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `Story16Epic1PlayableCheckpointTests`, `.RefreshHudFromLocalNetworkedPlayerState`, `.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`, `.HandleLifecycleChanged`?**
  _High betweenness centrality (0.124) - this node is a cross-community bridge._
- **Why does `VehiclePhysicsBody` connect `VehiclePhysicsBody` to `MonoBehaviour`, `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, `NetworkedVehicleDriverController`, `NetworkedAIVehicleDriverController`, `Story512TireForcesAndSteeringTests`, `VehicleProfile`, `VehicleProfileDef`, `.Inspect`, `.FixedUpdate`, `.UpdateSteeringState`, `Story511VehicleChassisWheelsAndSuspensionTests`, `Story514AiDrivesByIntentTests`, `VehicleDriveIntent`, `LaneNode`, `Story513ArcadeAssistsAndUnevenGroundTests`, `VehicleSuspensionModel`?**
  _High betweenness centrality (0.113) - this node is a cross-community bridge._
- **Why does `RoadRage.Features.Vehicles` connect `RoadRage.Features.Vehicles` to `NetworkedVehicleDamageVfxController`, `VehiclePhysicsBody`, `Story513ArcadeAssistsAndUnevenGroundTests`, `VehicleSuspensionModel`, `VehicleProfile`, `RoadRage.Features.UI`, `RoadRage.Features.Players`, `.SelectWeightedSuccessor`, `VehicleWheel`, `LaneNode`, `RoadRage.Shared.Domain`, `.Inspect`, `RoadRageScaffoldTests.cs`, `TrafficSettingsDef`, `RoadRage.Shared.Input`, `LaneGraph`, `MonoBehaviour`, `.FixedUpdate`, `.UpdateSteeringState`, `VehicleDriveIntent`, `Story59ParameterizedDriverModelTests`?**
  _High betweenness centrality (0.093) - this node is a cross-community bridge._
- **What connects `Settings`, `LivePopulation`, `Difficulty` to the rest of the system?**
  _404 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09142857142857143 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.08941176470588236 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.11742424242424243 - nodes in this community are weakly interconnected._