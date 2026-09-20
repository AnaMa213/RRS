# Graph Report - RRS  (2026-09-19)

## Corpus Check
- 179 files · ~172,602 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3321 nodes · 7821 edges · 162 communities (125 shown, 36 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 404 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `6f948ffe`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- GreyboxAssetSeedMetadata
- Difficulty
- Story512TireForcesAndSteeringTests
- Story11MainMenuLaunchTests
- .ReplayRoute
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- Story32DriverControlAndLocalCameraTests
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
- NetworkedRageState
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
- RoadRage.App.Run
- LobbyRoomService
- VehicleProfile
- VehicleProfileDef
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- RoadRage.Shared.Domain
- LobbyRosterSnapshot
- Story36Epic3DrivingPlayableCheckpointTests
- FakeSteamLobbyPlatform
- PortalTrafficSpawner
- Story514AiDrivesByIntentTests
- .Author
- NetworkedPlayerLifecycleService
- .SelectWeightedSuccessor
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- VehicleWheel
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- ThirdPersonCameraTests
- Story54RageDrivenAiBehaviorStatesTests
- FakeSteamLobbyPlatform
- Lobby Join Service
- NetworkedAIVehicleState
- RoadRage.Features.Players
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- NetworkedPassengerActionIntent
- CharacterCatalog
- PassengerActionDef
- NetworkedPlayerState
- Story58EscapeMenuTests
- RoadRageBootstrap
- .NewFixture
- .Configure
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- PassengerActionIntent
- RoadRage.Shared.Networking
- Story57AiTrafficClientPresentationTests
- Test
- FakeSteamPlatform
- VehicleDriveIntent
- Story56RageRoadEventTriggerTests
- MainMenuFlowController
- Bounds
- GameObject
- .RequestHonk
- PlayerProfileResolution
- .EnsureVehicleSandboxSeatHarness
- PlayerMode
- Vector3
- FakeSteamPlatform
- PlayerProfile
- DefinitionId
- PlayerLifecycle
- RoadRage Scaffold Structure Tests
- .FindRecursive
- LocalVehicleCameraRig
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- Story16Epic1PlayableCheckpointTests.cs
- CharacterController
- NetworkTransform
- Quaternion
- TireSample
- Object
- MainMenuScreen
- Renderer
- TearDown
- Func
- Private Room Play Mode Tests
- Component
- .MvpRunHasNoInScenePlacedTrafficAndAHostPortalSpawnerInstead
- Netcode/Steamworks Smoke Tests
- IEnumerator
- UnitySetUp
- UnityTearDown
- UnityTest
- NetworkedVehicleRecoveryIntent
- .TearDown
- IReadOnlyList
- RageDisposition
- TelemetrySample
- Action
- Bounds
- LaneNode
- Scene
- FacepunchSteamPlatform
- Transform
- SerializedObject
- OnFootMovementIntent
- SerializedProperty
- .EnsureNetworkManager
- Story52BasicAiRouteFollowingAndRecoveryTests
- .TearDown
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- MonoBehaviour
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- Story42PassengerActionFrameworkTests
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
- LobbyPlayerSlotView
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
2. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
3. `NetworkedVehicleState` - 79 edges
4. `RunCheckpointHudScreen` - 69 edges
5. `RoadRage.Features.Vehicles` - 66 edges
6. `NetworkedVehicleDriverController` - 62 edges
7. `RoadRage.Shared.Domain` - 61 edges
8. `VehiclePhysicsBody` - 57 edges
9. `NetworkedAIVehicleDriverController` - 55 edges
10. `RoadRage.Features.Players` - 50 edges

## Surprising Connections (you probably didn't know these)
- `NetworkedAIVehicleDriverController` --references--> `VehiclePhysicsBody`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs → Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs
- `NetworkedVehicleDriverController` --references--> `VehiclePhysicsBody`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs → Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs
- `Story512TireForcesAndSteeringPlayModeTests` --references--> `VehiclePhysicsBody`  [EXTRACTED]
  Assets/RoadRage/Tests/PlayMode/Story512TireForcesAndSteeringPlayModeTests.cs → Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs
- `Story513ArcadeAssistsAndUnevenGroundPlayModeTests` --references--> `VehiclePhysicsBody`  [EXTRACTED]
  Assets/RoadRage/Tests/PlayMode/Story513ArcadeAssistsAndUnevenGroundPlayModeTests.cs → Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs
- `MainMenuProfileFlowController` --references--> `PlayerProfileBootstrapService`  [EXTRACTED]
  Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs → Assets/RoadRage/Features/Players/PlayerProfileBootstrapService.cs

## Import Cycles
- None detected.

## Communities (162 total, 36 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.10
Nodes (15): ArgumentException, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component (+7 more)

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
Cohesion: 0.12
Nodes (11): Difficulty, Difficulty, Color, LobbyRosterEntry, DisplayName, PortraitTint, Ready, Difficulty (+3 more)

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.06
Nodes (20): VehicleSteeringModel, TireSample, Vector2, TireSample, Adherence, ForceMagnitude, GripUsage, Grounded (+12 more)

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.18
Nodes (9): Canvas, Component, EventSystem, InputSystemUIInputModule, Scene, SerializeField, Test, Story11MainMenuLaunchTests (+1 more)

### Community 7 - ".ReplayRoute"
Cohesion: 0.12
Nodes (12): Action, DriverProfileDef, LaneGraph, List, CurbTrafficMeasurement, ReplayTrace, Bounds, CurbTrafficMeasurement (+4 more)

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.12
Nodes (9): Collider, GameObject, LaneNode, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.15
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 11 - "RunFlowController"
Cohesion: 0.06
Nodes (8): Camera, CharacterController, Collider, GameObject, HashSet, Transform, RunFlowController, ActiveLocalPlayer

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.10
Nodes (6): GameObject, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform, StringBuilder

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (24): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+16 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyFlowController"
Cohesion: 0.13
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.13
Nodes (13): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Vector2, LocalOnFootController, IsDowned, MovementEnabled (+5 more)

### Community 17 - "UserNotice"
Cohesion: 0.18
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.14
Nodes (11): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, List, NetworkObject, Object (+3 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "NetworkedVehicleSeatIntent"
Cohesion: 0.05
Nodes (27): Rpc, RpcParams, NetworkedPlayerLifecycleIntent, Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, Key (+19 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.09
Nodes (19): Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody, VehicleDriveIntent, VehicleProfile, VehicleProfileDef, VehiclePhysicsBody (+11 more)

### Community 22 - "NetworkedRageState"
Cohesion: 0.10
Nodes (15): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner, Quaternion, Vector3 (+7 more)

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.25
Nodes (9): BoxCollider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest, Vector3 (+1 more)

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.10
Nodes (11): BoxCollider, Collider, GameObject, Test, Vector3, Story510LaneGraphAndRoutedTrafficTests, LaneNode, LaneNodeRole (+3 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.11
Nodes (6): CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests, IEnumerable

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.12
Nodes (12): Difficulty, MatchSettings, Difficulty, Color, Component, GameObject, Image, Scene (+4 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.07
Nodes (19): Collider, LaneNode, Transform, VehiclePhysicsBody, Story513RageTargetInspection, VehicleArcadeAssist, Action, Bounds (+11 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.24
Nodes (6): PointerEventData, TMP_Text, LobbyCodeClipboard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

### Community 30 - "RoadRage.Features.Vehicles"
Cohesion: 0.11
Nodes (3): RoadRage.Tests.EditMode, RoadRage.Features.Rage, RoadRage.Features.Vehicles

### Community 31 - "VehicleSuspensionModel"
Cohesion: 0.08
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.24
Nodes (10): BoxCollider, GameObject, List, Rigidbody, Vector3, Story513ArcadeAssistsAndUnevenGroundPlayModeTests, IEnumerator, UnitySetUp (+2 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.14
Nodes (10): DevIndestructibleVehicle, Action, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3 (+2 more)

### Community 36 - "RoadRage.App.Run"
Cohesion: 0.21
Nodes (3): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Shared.Input

### Community 37 - "LobbyRoomService"
Cohesion: 0.13
Nodes (12): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+4 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.15
Nodes (9): DefinitionId, Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty (+1 more)

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.25
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "RoadRage.Shared.Domain"
Cohesion: 0.10
Nodes (3): RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Features.Run

### Community 44 - "LobbyRosterSnapshot"
Cohesion: 0.14
Nodes (17): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+9 more)

### Community 45 - "Story36Epic3DrivingPlayableCheckpointTests"
Cohesion: 0.21
Nodes (7): AssemblyDefinition, Component, GameObject, Scene, Test, AssemblyDefinition, Story36Epic3DrivingPlayableCheckpointTests

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "PortalTrafficSpawner"
Cohesion: 0.07
Nodes (27): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation (+19 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.10
Nodes (14): List, Transform, VehiclePhysicsBody, Story513DriverHandshakeInspection, GameObject, Test, VehicleDriveIntent, VehicleProfile (+6 more)

### Community 49 - ".Author"
Cohesion: 0.26
Nodes (7): Collider, LaneNode, List, Transform, Story513RecipeRelief, GameObject, Vector3

### Community 51 - ".SelectWeightedSuccessor"
Cohesion: 0.33
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 52 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 53 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 56 - "ThirdPersonCameraTests"
Cohesion: 0.25
Nodes (9): Camera, CinemachineCamera, CinemachineDeoccluder, CinemachineInputAxisController, CinemachineOrbitalFollow, GameObject, Test, ThirdPersonCameraTests (+1 more)

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.15
Nodes (9): AiVehicleFixture, RageDisposition, List, NetworkObject, Object, TearDown, Test, AiVehicleFixture (+1 more)

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 59 - "Lobby Join Service"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "NetworkedAIVehicleState"
Cohesion: 0.44
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 61 - "RoadRage.Features.Players"
Cohesion: 0.16
Nodes (3): RoadRage.Features.Players, RoadRage.Features.PassengerActions, RoadRage.Shared.Definitions

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.10
Nodes (15): ArgumentNullException, Difficulty, Exception, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount, LastMaxMembers (+7 more)

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.15
Nodes (10): GameObject, List, NetworkObject, Object, TearDown, IntentFixture, Intent, Target (+2 more)

### Community 65 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (20): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+12 more)

### Community 66 - "CharacterCatalog"
Cohesion: 0.20
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 67 - "PassengerActionDef"
Cohesion: 0.18
Nodes (10): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+2 more)

### Community 68 - "NetworkedPlayerState"
Cohesion: 0.33
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 69 - "Story58EscapeMenuTests"
Cohesion: 0.06
Nodes (25): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, Canvas (+17 more)

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.10
Nodes (14): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+6 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 72 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineDeoccluder, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineRotationComposer

### Community 73 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 75 - "AIVehicleBehaviorDebugView"
Cohesion: 0.18
Nodes (8): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, IRageDispositionSource, CurrentDisposition, GameObject, TextMeshPro

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.17
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "PassengerActionIntent"
Cohesion: 0.22
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 78 - "RoadRage.Shared.Networking"
Cohesion: 0.14
Nodes (7): VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking, RoadRage.Features.Boss

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 81 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 82 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.05
Nodes (30): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+22 more)

### Community 88 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 89 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 90 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 92 - "FakeSteamPlatform"
Cohesion: 0.25
Nodes (4): ArgumentNullException, FakeSteamPlatform, IsLoggedOn, IsValid

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - "DefinitionId"
Cohesion: 0.16
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 96 - "PlayerLifecycle"
Cohesion: 0.33
Nodes (5): PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.14
Nodes (10): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+2 more)

### Community 100 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 107 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 128 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 131 - "OnFootMovementIntent"
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 146 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.22
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.14
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (23): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+15 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.12
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 266 - "MonoBehaviour"
Cohesion: 0.12
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "Story42PassengerActionFrameworkTests"
Cohesion: 0.19
Nodes (7): GameObject, List, NetworkObject, Object, TearDown, Test, Story42PassengerActionFrameworkTests

### Community 480 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.08
Nodes (19): BoxCollider, Collider, DriverProfileDef, LaneGraph, List, NetworkedAIVehicleState, RaycastHit, Rigidbody (+11 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.16
Nodes (7): List, NetworkObject, Object, TearDown, Test, TestCase, Story41RageStateModuleAndDefinitionsTests

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.17
Nodes (7): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Quaternion, Vector3

### Community 645 - "RageSandboxAutoStart"
Cohesion: 0.26
Nodes (6): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, RageSandboxAutoStart

### Community 647 - "LobbyRosterService"
Cohesion: 0.12
Nodes (8): Difficulty, Task, ISteamLobbyPlatform, LobbyRosterService, AllMembersReady, Current, IsSynchronized, LocalReady

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 697 - "LobbyShellScreen"
Cohesion: 0.20
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 736 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, Image, TMP_Text, LobbyPlayerSlotView

### Community 824 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.16
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 858 - "ReactionChannel"
Cohesion: 0.15
Nodes (8): TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage, ContextMenu

### Community 865 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 873 - "PassengerActionVerdictCode"
Cohesion: 0.11
Nodes (19): PassengerActionValidation, PassengerActionValidationContext, PassengerActionVerdict, Accepted, PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive (+11 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **389 isolated node(s):** `HasReachedExitPortal`, `HasProfile`, `WheelCount`, `GroundedWheelCount`, `GroundedAuthorityFactor` (+384 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 869 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **36 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `GreyboxAssetSeedMetadata`, `NetworkedVehicleState`, `MonoBehaviour`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `LocalOnFootController`, `NetworkedRageState`, `NetworkedVehicleDriverController`, `RoadRage.App.Run`, `Story26InGameHudTests`, `Story36Epic3DrivingPlayableCheckpointTests`, `Story43PassengerActionOneChangesRageTests`, `NetworkedAIVehicleState`, `Story55NetworkedAiRageTargetingTests`, `NetworkedPassengerActionIntent`, `CharacterCatalog`, `NetworkedPlayerState`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `AIVehicleBehaviorDebugView`, `Story56RageRoadEventTriggerTests`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`?**
  _High betweenness centrality (0.143) - this node is a cross-community bridge._
- **Why does `RoadRage.Features.Vehicles` connect `RoadRage.Features.Vehicles` to `Story512TireForcesAndSteeringTests`, `NetworkedVehicleDamageVfxController`, `VehiclePhysicsBody`, `Story513ArcadeAssistsAndUnevenGroundTests`, `VehicleSuspensionModel`, `NetworkedVehicleDriverController`, `RoadRage.App.Run`, `VehicleProfile`, `VehicleProfileDef`, `RoadRage.Features.UI`, `RoadRage.Shared.Domain`, `.SelectWeightedSuccessor`, `VehicleWheel`, `LaneNode`, `RoadRage.Features.Players`, `RoadRage.Shared.Networking`, `VehicleDriveIntent`, `LaneGraph`, `NetworkedAIVehicleDriverController`, `Story16Epic1PlayableCheckpointTests.cs`, `Story59ParameterizedDriverModelTests`?**
  _High betweenness centrality (0.107) - this node is a cross-community bridge._
- **Why does `VehiclePhysicsBody` connect `VehiclePhysicsBody` to `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, `Story513ArcadeAssistsAndUnevenGroundPlayModeTests`, `NetworkedVehicleDriverController`, `NetworkedAIVehicleDriverController`, `Story512TireForcesAndSteeringTests`, `Story511VehicleChassisWheelsAndSuspensionTests`, `MonoBehaviour`, `Story514AiDrivesByIntentTests`, `Story512TireForcesAndSteeringPlayModeTests`, `Story513ArcadeAssistsAndUnevenGroundTests`, `VehicleSuspensionModel`?**
  _High betweenness centrality (0.096) - this node is a cross-community bridge._
- **What connects `HasReachedExitPortal`, `HasProfile`, `WheelCount` to the rest of the system?**
  _389 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10202020202020202 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.057902973395931145 - nodes in this community are weakly interconnected._