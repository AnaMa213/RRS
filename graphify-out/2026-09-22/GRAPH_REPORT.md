# Graph Report - RRS  (2026-09-22)

## Corpus Check
- 198 files · ~201,027 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3846 nodes · 9286 edges · 176 communities (161 shown, 12 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 435 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `d35b5ad1`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- Story14GreyboxAssetSeedTests
- Story516LobbyConfigurableTrafficSettingsTests
- Story512TireForcesAndSteeringTests
- UserNotice
- LobbyRosterScreen
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- FakeSteamLobbyPlatform
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyFlowController
- LocalOnFootController
- TraceDivergenceField
- Story51NpcRageFearFoundationTests
- MenuCharacterPreview
- RoadModelRecords.cs
- VehiclePhysicsBody
- Story32DriverControlAndLocalCameraTests
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
- TrafficOracleTests
- PhysicsHarness
- Story58EscapeMenuPlayModeTests
- VehicleProfile
- VehicleProfileDef
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- .RageSandboxShowsTheSharedIncidentMarker
- LobbyCreateOutcome
- DefinitionId
- FakeSteamLobbyPlatform
- TrafficSettingsDef
- Story514AiDrivesByIntentTests
- LobbyShellScreen
- RoadRage.Features.Players
- .SelectWeightedSuccessor
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- VehicleWheel
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- NetworkedRunState
- NetworkedRageState
- FakeSteamLobbyPlatform
- LobbyJoinService
- NetworkedAIVehicleState
- Story525RoadWorldModelTests
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- .Author
- CharacterCatalog
- PassengerActionDef
- LobbyRosterSnapshot
- RunEscapeMenuScreen
- RoadRageBootstrap
- .NewFixture
- .Inspect
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- NetworkedPassengerActionIntent
- Story28Epic2OnlinePlayableCheckpointTests
- Story57AiTrafficClientPresentationTests
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- RoadRage.Shared.Networking
- NetworkedVehicleSeatIntent
- Story56RageRoadEventTriggerTests
- RoadRage.Shared.Domain
- Story33SeatEntryExitAndPassengerPresenceTests
- .TearDown
- RoadCurveSample
- PlayerProfileResolution
- .EnsureVehicleSandboxSeatHarness
- .ComputeCollisionDamage
- Story16Epic1PlayableCheckpointTests
- NetworkedPlayerLifecycleIntent
- PlayerProfile
- Story36Epic3DrivingPlayableCheckpointTests
- RoadRecordKind
- PassengerActionVerdictCode
- RoadRageScaffoldTests
- Story58EscapeMenuTests
- LocalVehicleCameraRig
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- .Inspect
- NetworkedPlayerSpawnService
- OracleEvidenceClassification
- ThirdPersonCameraTests
- PlayerMode
- MainMenuScreen
- GreyboxAssetSeedMetadata
- .Find
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- Private Room Play Mode Tests
- RoadModelCanonicalWriter
- NetworkPlayerRegistry
- Netcode/Steamworks Smoke Tests
- VehicleDriveIntent
- NetworkedPlayerState
- .NewTuning
- .Append
- NetworkedVehicleRecoveryIntent
- .FixedUpdate
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- TrafficTraceFrame
- NetworkedPlayerLifecycleService
- GameObject
- .RequestHonk
- PassengerActionIntent
- JunctionControlKind
- FakeSteamPlatform
- ShapeGuardRegister.cs
- RoadModelVersion
- Story516ForceAssetRefresh
- RoadId
- .ResolveEffectiveCorridors
- .Run
- .Configure
- RoadModelValidationCode
- .Create
- Difficulty
- RunEscapeMenuFlowController
- OnFootMovementIntent
- PassengerActionCatalog
- TireSample
- AppSceneRouter.cs
- Story52BasicAiRouteFollowingAndRecoveryTests
- .UpdateSteeringState
- .EnsureNetworkManager
- PlayerLifecycle
- .NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId
- SeedExpectation
- .TearDown
- Action
- Test
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- Story42PassengerActionFrameworkTests
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- LocalVoidRespawnController
- RageSandboxAutoStart
- Story21OnlineServicesPlayModeTests
- Lock-Rage Camera Fix Query
- MonoBehaviour
- LaneNode
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedVehicleSeatService
- ReactionChannel
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RunFlowController` - 118 edges
2. `NetworkedVehicleState` - 80 edges
3. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
4. `RoadId` - 76 edges
5. `RoadRage.Features.Vehicles` - 75 edges
6. `RunCheckpointHudScreen` - 69 edges
7. `RoadRage.Shared.Domain` - 66 edges
8. `NetworkedVehicleDriverController` - 65 edges
9. `Story525RoadWorldModelTests` - 64 edges
10. `LobbyFlowController` - 63 edges

## Surprising Connections (you probably didn't know these)
- `EffectiveLaneCorridor` --references--> `RoadCurveSample`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs
- `EffectiveLaneCorridor` --references--> `RoadSurface`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs
- `EffectiveLaneCorridor` --references--> `VehicleClassMask`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs
- `CompiledRoadModel` --references--> `LaneAdjacency`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs
- `CompiledRoadModel` --references--> `LaneConnection`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs

## Import Cycles
- None detected.

## Communities (176 total, 12 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.09
Nodes (17): ArgumentException, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component (+9 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.17
Nodes (4): NetworkObject, Test, TextMeshProUGUI, Story25NetworkedPlayerSpawnTests

### Community 3 - "Story14GreyboxAssetSeedTests"
Cohesion: 0.17
Nodes (10): Bounds, Collider, GameObject, NetworkObject, Renderer, Test, Transform, Story14GreyboxAssetSeedTests (+2 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.10
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.11
Nodes (6): GameObject, Rigidbody, SerializedObject, SerializedProperty, Test, Story512TireForcesAndSteeringTests

### Community 6 - "UserNotice"
Cohesion: 0.09
Nodes (18): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+10 more)

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.11
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

### Community 11 - "RunFlowController"
Cohesion: 0.08
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.14
Nodes (6): GameObject, StringBuilder, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.06
Nodes (27): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+19 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyFlowController"
Cohesion: 0.11
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.12
Nodes (15): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+7 more)

### Community 17 - "TraceDivergenceField"
Cohesion: 0.13
Nodes (17): List, TraceDivergence, TraceDivergenceField, Blockers, BrakeReverse, FrameCountMismatch, Goal, Handbrake (+9 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "RoadModelRecords.cs"
Cohesion: 0.07
Nodes (37): ConflictZone, ImportManifest, ImportManifestEntry, Junction, JunctionFeature, Crossroads, Other, Roundabout (+29 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.09
Nodes (16): Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees (+8 more)

### Community 22 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.15
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.08
Nodes (17): Action, Bounds, BoxCollider, Collider, GameObject, List, Object, Renderer (+9 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.10
Nodes (6): CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests, IEnumerable

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.11
Nodes (14): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject (+6 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.08
Nodes (13): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, Scene (+5 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "RoadRage.Features.Vehicles"
Cohesion: 0.12
Nodes (4): RoadRage.DevTools, RoadRage.Shared.Authoring, RoadRage.Tests.EditMode, RoadRage.Features.Vehicles

### Community 31 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.22
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.11
Nodes (9): Action, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController (+1 more)

### Community 35 - "TrafficOracleTests"
Cohesion: 0.15
Nodes (3): Test, Vector3, TrafficOracleTests

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.20
Nodes (7): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.21
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.07
Nodes (26): LobbyJoinFailureReason, Expired, Failed, Full, None, LobbyJoinOutcome, LobbyId, Reason (+18 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - ".RageSandboxShowsTheSharedIncidentMarker"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story44PassengerActionTwoMvpRunPlayModeTests

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 45 - "DefinitionId"
Cohesion: 0.16
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "TrafficSettingsDef"
Cohesion: 0.08
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation (+12 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.20
Nodes (5): Func, GameObject, Rigidbody, Test, Story514AiDrivesByIntentTests

### Community 49 - "LobbyShellScreen"
Cohesion: 0.20
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 50 - "RoadRage.Features.Players"
Cohesion: 0.12
Nodes (4): RoadRage.Features.Players, RoadRage.Features.OnFoot, RoadRage.Shared.Input, RoadRage.Shared.Definitions

### Community 51 - ".SelectWeightedSuccessor"
Cohesion: 0.21
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

### Community 56 - "NetworkedRunState"
Cohesion: 0.14
Nodes (12): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, RageRoadEventState, Confrontation (+4 more)

### Community 57 - "NetworkedRageState"
Cohesion: 0.09
Nodes (21): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, RageDisposition, Block (+13 more)

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 59 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "NetworkedAIVehicleState"
Cohesion: 0.36
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 61 - "Story525RoadWorldModelTests"
Cohesion: 0.11
Nodes (7): Action, RoadModelSource, SignalPlan, Vector3, Story525RoadWorldModelTests, DeltaMutation, Test

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.12
Nodes (12): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, IntentFixture (+4 more)

### Community 65 - ".Author"
Cohesion: 0.28
Nodes (7): Bounds, Collider, GameObject, List, Transform, Vector3, Story513RecipeRelief

### Community 66 - "CharacterCatalog"
Cohesion: 0.17
Nodes (7): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, CharacterOption

### Community 67 - "PassengerActionDef"
Cohesion: 0.15
Nodes (12): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+4 more)

### Community 68 - "LobbyRosterSnapshot"
Cohesion: 0.06
Nodes (27): Difficulty, Task, ISteamLobbyPlatform, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount (+19 more)

### Community 69 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.07
Nodes (18): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+10 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 72 - ".Inspect"
Cohesion: 0.24
Nodes (6): Func, List, MonoBehaviour, Rigidbody, Transform, Story513DriverHandshakeInspection

### Community 73 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.12
Nodes (12): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot, AssemblyDefinition, GameObject, Object, Scene (+4 more)

### Community 75 - "AIVehicleBehaviorDebugView"
Cohesion: 0.25
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "NetworkedPassengerActionIntent"
Cohesion: 0.11
Nodes (14): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+6 more)

### Community 78 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.17
Nodes (10): ApprovalResult, NetworkPlayerConnectionPayload, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+2 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 81 - "RoadRage.Shared.Networking"
Cohesion: 0.13
Nodes (9): NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking (+1 more)

### Community 82 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.10
Nodes (11): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, GameObject, List, NetworkObject, Object, TearDown (+3 more)

### Community 84 - "RoadRage.Shared.Domain"
Cohesion: 0.12
Nodes (6): SessionTrafficValue, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.App.Run, RoadRage.Features.Rage, RoadRage.Features.PassengerActions

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 87 - "RoadCurveSample"
Cohesion: 0.09
Nodes (21): LaneCorridor, RoadClass, Arterial, Highway, Local, Service, Unspecified, RoadCurveSample (+13 more)

### Community 88 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 89 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 91 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.24
Nodes (5): Component, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 92 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - "Story36Epic3DrivingPlayableCheckpointTests"
Cohesion: 0.19
Nodes (8): AssemblyDefinition, Component, GameObject, NetworkObject, Scene, Test, AssemblyDefinition, Story36Epic3DrivingPlayableCheckpointTests

### Community 95 - "RoadRecordKind"
Cohesion: 0.16
Nodes (19): RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 96 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 97 - "RoadRageScaffoldTests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 98 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.15
Nodes (9): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+1 more)

### Community 100 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 101 - ".Inspect"
Cohesion: 0.38
Nodes (4): Collider, Component, Transform, Story513RageTargetInspection

### Community 102 - "NetworkedPlayerSpawnService"
Cohesion: 0.18
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

### Community 103 - "OracleEvidenceClassification"
Cohesion: 0.18
Nodes (13): IReadOnlyList, BoundTest, OracleCatalog, OracleEvidenceClassification, AutoEdit, AutoPlay, Gap, Manual (+5 more)

### Community 104 - "ThirdPersonCameraTests"
Cohesion: 0.25
Nodes (9): Camera, CinemachineCamera, CinemachineDeoccluder, CinemachineInputAxisController, CinemachineOrbitalFollow, GameObject, Test, ThirdPersonCameraTests (+1 more)

### Community 105 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 107 - "MainMenuScreen"
Cohesion: 0.11
Nodes (10): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+2 more)

### Community 108 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (10): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+2 more)

### Community 110 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "RoadModelCanonicalWriter"
Cohesion: 0.31
Nodes (4): Comparison, Vector3, RoadModelCanonicalWriter, BinaryWriter

### Community 113 - "NetworkPlayerRegistry"
Cohesion: 0.26
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 115 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 116 - "NetworkedPlayerState"
Cohesion: 0.12
Nodes (15): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, NetworkVariable, NetworkedBossState, FixedString32Bytes, NetworkVariable (+7 more)

### Community 118 - ".Append"
Cohesion: 0.23
Nodes (8): Canvas, CanvasScaler, Component, StringBuilder, TMP_Text, Transform, Vector2, Story516LobbyPanelInspection

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 120 - ".FixedUpdate"
Cohesion: 0.22
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 121 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 122 - "TrafficTraceFrame"
Cohesion: 0.25
Nodes (6): Action, Scene, List, Vector3, TrafficTrace, TrafficTraceFrame

### Community 125 - "GameObject"
Cohesion: 0.36
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, TearDown

### Community 127 - "PassengerActionIntent"
Cohesion: 0.25
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 128 - "JunctionControlKind"
Cohesion: 0.22
Nodes (8): Vector3, JunctionControlKind, Priority, Signalized, Stop, Uncontrolled, Yield, RoadLineSegment

### Community 129 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 130 - "ShapeGuardRegister.cs"
Cohesion: 0.67
Nodes (3): IReadOnlyList, ShapeGuardEntry, ShapeGuardRegister

### Community 131 - "RoadModelVersion"
Cohesion: 0.32
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 133 - "RoadId"
Cohesion: 0.07
Nodes (30): Dictionary, IReadOnlyList, List, CompiledRoadModel, Adjacencies, ConflictZones, Connections, Controls (+22 more)

### Community 134 - ".ResolveEffectiveCorridors"
Cohesion: 0.29
Nodes (3): Comparison, RoadModelCompiler, RoadRage.Features.Vehicles.Traffic

### Community 135 - ".Run"
Cohesion: 0.27
Nodes (10): Button, GameObject, Object, Scene, SerializedObject, StringBuilder, TMP_Text, Transform (+2 more)

### Community 136 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineDeoccluder, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineRotationComposer

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.13
Nodes (15): RoadModelValidationCode, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, CrossVersionReference, DuplicateId, EmptyId, IncompatibleManifestRemap, InvalidGeometryPayload (+7 more)

### Community 138 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 139 - "Difficulty"
Cohesion: 0.20
Nodes (6): Difficulty, Difficulty, Difficulty, Easy, Hard, Normal

### Community 140 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 142 - "OnFootMovementIntent"
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 143 - "PassengerActionCatalog"
Cohesion: 0.40
Nodes (4): List, PassengerActionCatalog, Count, Version

### Community 144 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 145 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 146 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.25
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 148 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 149 - "PlayerLifecycle"
Cohesion: 0.33
Nodes (5): PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 150 - ".NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId"
Cohesion: 0.33
Nodes (3): Collider, Renderer, Type

### Community 151 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (22): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+14 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.15
Nodes (5): Difficulty, Task, FacepunchSteamLobbyPlatform, Lobby, RoomEnter

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "Story42PassengerActionFrameworkTests"
Cohesion: 0.19
Nodes (7): GameObject, List, NetworkObject, Object, TearDown, Test, Story42PassengerActionFrameworkTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

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
Cohesion: 0.26
Nodes (6): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, RageSandboxAutoStart

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 736 - "MonoBehaviour"
Cohesion: 0.20
Nodes (6): Color, Image, TMP_Text, LobbyPlayerSlotView, DevIndestructibleVehicle, MonoBehaviour

### Community 824 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.14
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 858 - "ReactionChannel"
Cohesion: 0.12
Nodes (9): Transform, TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage (+1 more)

### Community 865 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **505 isolated node(s):** `SchemaVersion`, `High`, `Low`, `IsEmpty`, `ModelId` (+500 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1007 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **12 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `VehiclePhysicsBody` connect `VehiclePhysicsBody` to `MonoBehaviour`, `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, `NetworkedVehicleDriverController`, `Story513ArcadeAssistsAndUnevenGroundPlayModeTests`, `NetworkedAIVehicleDriverController`, `.Inspect`, `VehicleProfile`, `VehicleProfileDef`, `.Inspect`, `Story511VehicleChassisWheelsAndSuspensionTests`, `Story512TireForcesAndSteeringTests`, `Story514AiDrivesByIntentTests`, `VehicleDriveIntent`, `.UpdateSteeringState`, `Story512TireForcesAndSteeringPlayModeTests`, `.FixedUpdate`, `Story513ArcadeAssistsAndUnevenGroundTests`, `VehicleSuspensionModel`?**
  _High betweenness centrality (0.130) - this node is a cross-community bridge._
- **Why does `RunFlowController` connect `RunFlowController` to `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `.Update`, `PassengerActionCatalog`, `LocalOnFootController`, `NetworkedVehicleDriverController`, `Story26InGameHudTests`, `Story43PassengerActionOneChangesRageTests`, `NetworkedAIVehicleState`, `Story55NetworkedAiRageTargetingTests`, `CharacterCatalog`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `AIVehicleBehaviorDebugView`, `NetworkedPassengerActionIntent`, `Story56RageRoadEventTriggerTests`, `RoadRage.Shared.Domain`, `Story16Epic1PlayableCheckpointTests`, `Story36Epic3DrivingPlayableCheckpointTests`, `MonoBehaviour`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `.RefreshVehicleDamageHud`, `.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`, `NetworkedPlayerState`, `.HandleLifecycleChanged`?**
  _High betweenness centrality (0.117) - this node is a cross-community bridge._
- **Why does `DefinitionId` connect `DefinitionId` to `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests`, `Story13CharacterSetupTests`, `CharacterCatalog`, `PassengerActionDef`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `NetworkedPlayerSpawnService`, `VehicleProfileDef`, `Empty Map Entry Playmode Tests`, `RageTuningDef`, `NetworkedPassengerActionIntent`, `TrafficSettingsDef`, `.MenuResolvesProfileAndPublishesTheChosenCharacter`, `RoadRage.Features.Players`, `Story59ParameterizedDriverModelTests`, `PlayerProfile`, `PassengerActionIntent`?**
  _High betweenness centrality (0.104) - this node is a cross-community bridge._
- **What connects `SchemaVersion`, `High`, `Low` to the rest of the system?**
  _505 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.0935374149659864 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story516LobbyConfigurableTrafficSettingsTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10102843315184513 - nodes in this community are weakly interconnected._