# Graph Report - RRS  (2026-09-23)

## Corpus Check
- 218 files · ~270,509 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4680 nodes · 11982 edges · 178 communities (163 shown, 13 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 523 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `9ab80d60`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- GreyboxAssetSeedMetadata
- Story516LobbyConfigurableTrafficSettingsTests
- Story512TireForcesAndSteeringTests
- Story11MainMenuLaunchTests
- LobbyRosterScreen
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- FakeSteamLobbyPlatform
- .TrySpawnSelectedProfile
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- RunEscapeMenuScreen
- LocalOnFootController
- Story12LobbyShellTests
- Story51NpcRageFearFoundationTests
- MenuCharacterPreview
- Story527MigrationTests
- VehiclePhysicsBody
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- Story549RoundaboutWideningTests
- AuthoredRoadModel
- Story27PlayerLifecycleTests
- List
- Story513ArcadeAssistsAndUnevenGroundTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- RoadRage.Shared.Input
- AuthoringDecisions
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- Story513ArcadeAssistsAndUnevenGroundPlayModeTests
- NetworkedVehicleDriverController
- TrafficOracleTests
- PhysicsHarness
- RoadModelDocument
- VehicleProfile
- RoundaboutClearance
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- Story52BasicAiRouteFollowingAndRecoveryTests
- LobbyCreateOutcome
- RoadModelCanonicalWriter
- FakeSteamLobbyPlatform
- TrafficSettingsDef
- Story514AiDrivesByIntentTests
- RoadRage.Shared.Domain
- RoadRage.Features.Players
- CompiledRoadModel
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- RunEscapeMenuFlowController
- Story43PassengerActionOneChangesRageTests
- LobbyShellScreen
- NetworkedRunState
- NetworkedRageState
- .Localize
- LobbyJoinService
- Story528AuthoringAndGateATests
- Story525RoadWorldModelTests
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- RoadRage.Features.Vehicles
- Story55NetworkedAiRageTargetingTests
- .Author
- MainMenuProfileFlowController
- LobbyRosterSnapshot
- NetworkedPlayerLifecycleService
- NetworkedVehicleSeatIntent
- RoadRageBootstrap
- .NewFixture
- .Inspect
- RageTuningDef
- Story15EmptyMapEntryTests
- VehicleProfileDef
- NetworkedRunSessionMonitor
- NetworkedPassengerActionIntent
- LobbyRoomService
- Story57AiTrafficClientPresentationTests
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- .Create
- Story36Epic3DrivingPlayableCheckpointTests
- Story56RageRoadEventTriggerTests
- RoadRageScaffoldTests
- Story33SeatEntryExitAndPassengerPresenceTests
- ImportContext
- .Compare
- LobbyFlowController
- NetworkedPlayerState
- .Build
- Story526GeometryAndLocalizationTests
- RoadLineage
- PlayerProfile
- .Extract
- .Validate
- PassengerActionVerdictCode
- Story512TireForcesAndSteeringPlayModeTests
- GateAReviewWindow
- LocalVehicleCameraRig
- .SelectWeightedSuccessor
- V1ImportResult
- NetworkedPlayerReviveIntent
- OracleEvidenceClassification
- RoadModelSource
- .Add
- FakeSteamPlatform
- MainMenuScreen
- RunFlowController
- .Find
- RoadCurve
- Private Room Play Mode Tests
- Story58EscapeMenuTests
- .ProposeDecisionsMenu
- Netcode/Steamworks Smoke Tests
- .EnsureNetworkManager
- ReactionChannel
- Difficulty
- NetworkedVehicleRecoveryIntent
- LaneNode
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- MonoBehaviour
- .NewTuning
- TraceDivergenceField
- NetworkPlayerRegistry
- UserNotice
- HostOwnedNetworkStateBehaviour
- LobbyPlayerSlotView
- RageSandboxAutoStart
- ShapeGuardRegister.cs
- Story12LobbyShellPlayModeTests
- Story516ForceAssetRefresh
- RoadId
- Story58EscapeMenuPlayModeTests
- .Run
- NetworkedAIVehicleState
- RoadModelValidationCode
- FacepunchSteamPlatform
- RageTuningCatalog
- TireSample
- .Assemble
- VehicleDriveIntent
- .UpdateSteeringState
- .RequestHonk
- VehicleWheel
- .FixedUpdate
- .Inspect
- FileLayout
- .NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId
- PlayerLifecycle
- .ComputeCollisionDamage
- PlayerProfileResolution
- PlayerMode
- AppSceneRouter.cs
- DefinitionId
- RoadRage.Shared.Networking
- .FindRecursive
- .TearDown
- NetworkPlayerConnectionPayload
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- PassengerActionDef
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- LocalVoidRespawnController
- Story21OnlineServicesPlayModeTests
- Lock-Rage Camera Fix Query
- Story510LaneGraphAndRoutedTrafficTests
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedVehicleSeatService
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RoadId` - 120 edges
2. `RunFlowController` - 118 edges
3. `Story526GeometryAndLocalizationTests` - 89 edges
4. `Story525RoadWorldModelTests` - 81 edges
5. `NetworkedVehicleState` - 80 edges
6. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
7. `RoadRage.Features.Vehicles` - 76 edges
8. `RunCheckpointHudScreen` - 69 edges
9. `RoadRage.Shared.Domain` - 66 edges
10. `NetworkedVehicleDriverController` - 65 edges

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

## Communities (178 total, 13 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.10
Nodes (16): List, CharacterCatalog, Count, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath (+8 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.17
Nodes (5): GameObject, NetworkObject, Test, TextMeshProUGUI, Story25NetworkedPlayerSpawnTests

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.06
Nodes (28): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+20 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.10
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.12
Nodes (4): GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.13
Nodes (11): UserNoticeChannel, LastNotice, Canvas, CanvasScaler, Component, EventSystem, InputSystemUIInputModule, Scene (+3 more)

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.11
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

### Community 11 - ".TrySpawnSelectedProfile"
Cohesion: 0.11
Nodes (5): Collider, GameObject, Quaternion, Transform, Vector3

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.14
Nodes (6): GameObject, StringBuilder, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (23): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+15 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Story12LobbyShellTests"
Cohesion: 0.11
Nodes (14): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject (+6 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "Story527MigrationTests"
Cohesion: 0.05
Nodes (34): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+26 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.10
Nodes (15): Vector3, VehiclePhysicsTelemetryView, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor (+7 more)

### Community 22 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 23 - "Story549RoundaboutWideningTests"
Cohesion: 0.14
Nodes (11): RoadModelValidationProfile, Action, BoxCollider, IReadOnlyList, MeshCollider, Scene, Test, Vector3 (+3 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.10
Nodes (21): CompiledRoadModel, ConflictZone, Dictionary, HashSet, List, Predicate, RoadCurveSample, RoadLocation (+13 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.10
Nodes (6): IEnumerable, CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests

### Community 26 - "List"
Cohesion: 0.14
Nodes (9): Action, Bounds, List, Renderer, Scene, Transform, CurbTrafficMeasurement, ReplayTrace (+1 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.08
Nodes (13): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, Scene (+5 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.13
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.08
Nodes (36): Dictionary, FileLayout, Func, IList, JunctionControlKind, List, RoadBoundsBox, RoadCurveSample (+28 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.22
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.11
Nodes (10): DevIndestructibleVehicle, Action, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3 (+2 more)

### Community 35 - "TrafficOracleTests"
Cohesion: 0.13
Nodes (4): Action, Scene, Test, TrafficOracleTests

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "RoadModelDocument"
Cohesion: 0.06
Nodes (41): AdjacencyDto, Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto (+33 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.14
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.19
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.08
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.23
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 45 - "RoadModelCanonicalWriter"
Cohesion: 0.07
Nodes (21): ApprovalResult, Rpc, RpcParams, NetworkedPlayerLifecycleIntent, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter (+13 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "TrafficSettingsDef"
Cohesion: 0.08
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation (+12 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.20
Nodes (5): Func, GameObject, Rigidbody, Test, Story514AiDrivesByIntentTests

### Community 49 - "RoadRage.Shared.Domain"
Cohesion: 0.09
Nodes (7): SessionTrafficValue, RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.App.Run, RoadRage.Features.Rage, RoadRage.Features.PassengerActions

### Community 51 - "CompiledRoadModel"
Cohesion: 0.08
Nodes (28): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+20 more)

### Community 52 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 53 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "LobbyShellScreen"
Cohesion: 0.18
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 56 - "NetworkedRunState"
Cohesion: 0.10
Nodes (17): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, Camera, TMP_Text (+9 more)

### Community 57 - "NetworkedRageState"
Cohesion: 0.09
Nodes (17): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, GameObject, List (+9 more)

### Community 58 - ".Localize"
Cohesion: 0.14
Nodes (18): Dictionary, IReadOnlyList, List, Vector3, Query, RoadElementKind, None, RoadLocalizer (+10 more)

### Community 59 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "Story528AuthoringAndGateATests"
Cohesion: 0.18
Nodes (6): Action, FormatException, KeyValuePair, Scene, Test, Story528AuthoringAndGateATests

### Community 61 - "Story525RoadWorldModelTests"
Cohesion: 0.08
Nodes (8): Action, JunctionMovement, LaneCorridor, SignalPlan, Test, Vector3, Story525RoadWorldModelTests, DeltaMutation

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "RoadRage.Features.Vehicles"
Cohesion: 0.14
Nodes (4): RoadRage.Shared.Authoring, RoadRage.Tests.EditMode, RoadRage.Features.OnFoot, RoadRage.Features.Vehicles

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.12
Nodes (12): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, IntentFixture (+4 more)

### Community 65 - ".Author"
Cohesion: 0.28
Nodes (7): Bounds, Collider, GameObject, List, Transform, Vector3, Story513RecipeRelief

### Community 66 - "MainMenuProfileFlowController"
Cohesion: 0.22
Nodes (5): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, Component, RawImage

### Community 67 - "LobbyRosterSnapshot"
Cohesion: 0.06
Nodes (21): Difficulty, Task, ISteamLobbyPlatform, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount (+13 more)

### Community 69 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.11
Nodes (14): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+6 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 72 - ".Inspect"
Cohesion: 0.24
Nodes (6): Func, List, MonoBehaviour, Rigidbody, Transform, Story513DriverHandshakeInspection

### Community 73 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): TMP_Text, RageStateDebugView, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity, HonkChannel (+10 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 75 - "VehicleProfileDef"
Cohesion: 0.16
Nodes (9): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty, SerializedObject (+1 more)

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (22): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+14 more)

### Community 78 - "LobbyRoomService"
Cohesion: 0.06
Nodes (27): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+19 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 81 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 82 - "Story36Epic3DrivingPlayableCheckpointTests"
Cohesion: 0.19
Nodes (8): AssemblyDefinition, Component, GameObject, NetworkObject, Scene, Test, AssemblyDefinition, Story36Epic3DrivingPlayableCheckpointTests

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.08
Nodes (18): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, RageDisposition, Block, Calm, ConfrontationCapable, Flee (+10 more)

### Community 84 - "RoadRageScaffoldTests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 86 - "ImportContext"
Cohesion: 0.16
Nodes (20): Dictionary, JunctionFeature, List, Predicate, RoadCurve, RoadCurveSample, ImportContext, ImportedCurve (+12 more)

### Community 87 - ".Compare"
Cohesion: 0.19
Nodes (8): Vector3, List, TraceDivergence, TrafficTraceComparer, List, Vector3, TrafficTrace, TrafficTraceFrame

### Community 88 - "LobbyFlowController"
Cohesion: 0.10
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "NetworkedPlayerState"
Cohesion: 0.20
Nodes (9): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart, FixedString32Bytes, NetworkVariable, Vector3 (+1 more)

### Community 90 - ".Build"
Cohesion: 0.33
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 91 - "Story526GeometryAndLocalizationTests"
Cohesion: 0.10
Nodes (7): RoadCurveSample, Action, ArgumentException, LaneCorridor, Test, Vector3, Story526GeometryAndLocalizationTests

### Community 92 - "RoadLineage"
Cohesion: 0.13
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - ".Extract"
Cohesion: 0.13
Nodes (19): Func, GameObject, IReadOnlyList, LaneGraph, LaneNode, List, Scene, Vector3 (+11 more)

### Community 95 - ".Validate"
Cohesion: 0.15
Nodes (19): RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 96 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 97 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (15): Color, HashSet, List, MenuItem, RoadId, Vector2, ConflictFilter, All (+7 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.05
Nodes (35): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+27 more)

### Community 100 - ".SelectWeightedSuccessor"
Cohesion: 0.22
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 101 - "V1ImportResult"
Cohesion: 0.07
Nodes (30): RoadId, AuthoringTask, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint (+22 more)

### Community 102 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 103 - "OracleEvidenceClassification"
Cohesion: 0.18
Nodes (13): IReadOnlyList, BoundTest, OracleCatalog, OracleEvidenceClassification, AutoEdit, AutoPlay, Gap, Manual (+5 more)

### Community 104 - "RoadModelSource"
Cohesion: 0.07
Nodes (24): EffectiveLaneCorridor, JunctionMovement, RoadModelCanonicalPayload, Comparison, RoadModelCompiler, LaneCorridor, RoadLocalizationProfile, RoadModelSource (+16 more)

### Community 105 - ".Add"
Cohesion: 0.25
Nodes (10): ArgumentOutOfRangeException, Bounds, IReadOnlyList, RoadBoundsBox, RoadCurve, RoadId, Vector3, OverlayInstance (+2 more)

### Community 106 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 107 - "MainMenuScreen"
Cohesion: 0.08
Nodes (16): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+8 more)

### Community 108 - "RunFlowController"
Cohesion: 0.09
Nodes (5): Camera, CharacterController, HashSet, RunFlowController, ActiveLocalPlayer

### Community 110 - "RoadCurve"
Cohesion: 0.13
Nodes (20): Bounds, Vector3, RoadCurve, FullBounds, Length, StartS, RoadCurvePoint, RoadProjection (+12 more)

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 113 - ".ProposeDecisionsMenu"
Cohesion: 0.16
Nodes (7): Action, IList, KeyValuePair, MenuItem, Scene, SignoffLayout, DateTime

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 115 - ".EnsureNetworkManager"
Cohesion: 0.24
Nodes (7): GameObject, NetworkManager, NetworkPrefabsList, NetworkManager, NetworkPrefabsList, TearDown, FacepunchTransport

### Community 117 - "ReactionChannel"
Cohesion: 0.29
Nodes (5): ReactionChannel, Both, Fear, None, Rage

### Community 118 - "Difficulty"
Cohesion: 0.20
Nodes (6): Difficulty, Difficulty, Difficulty, Easy, Hard, Normal

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 120 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 121 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 122 - "MonoBehaviour"
Cohesion: 0.12
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 124 - "TraceDivergenceField"
Cohesion: 0.14
Nodes (14): TraceDivergenceField, Blockers, BrakeReverse, FrameCountMismatch, Goal, Handbrake, Position, RoadElementId (+6 more)

### Community 125 - "NetworkPlayerRegistry"
Cohesion: 0.29
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 126 - "UserNotice"
Cohesion: 0.27
Nodes (7): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning

### Community 127 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.18
Nodes (9): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority (+1 more)

### Community 128 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, Image, TMP_Text, LobbyPlayerSlotView

### Community 129 - "RageSandboxAutoStart"
Cohesion: 0.22
Nodes (7): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, Transform, RageSandboxAutoStart

### Community 130 - "ShapeGuardRegister.cs"
Cohesion: 0.67
Nodes (3): IReadOnlyList, ShapeGuardEntry, ShapeGuardRegister

### Community 131 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 133 - "RoadId"
Cohesion: 0.04
Nodes (63): CompiledConflictZone, Vector3, ConflictZone, ImportManifest, ImportManifestEntry, Junction, JunctionControl, JunctionControlKind (+55 more)

### Community 134 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 135 - ".Run"
Cohesion: 0.12
Nodes (18): Canvas, CanvasScaler, Component, StringBuilder, TMP_Text, Transform, Vector2, Story516LobbyPanelInspection (+10 more)

### Community 136 - "NetworkedAIVehicleState"
Cohesion: 0.38
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.06
Nodes (34): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference, DuplicateId (+26 more)

### Community 138 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 139 - "RageTuningCatalog"
Cohesion: 0.22
Nodes (4): List, RageTuningCatalog, Count, ScriptableObject

### Community 140 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 141 - ".Assemble"
Cohesion: 0.13
Nodes (11): IList, ImportManifestEntry, IReadOnlyList, KeyValuePair, RoadBoundsBox, RoadLocalizationProfile, RoadModelSource, RoadRecordKind (+3 more)

### Community 142 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 146 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 147 - ".FixedUpdate"
Cohesion: 0.07
Nodes (19): RaycastHit, Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip (+11 more)

### Community 148 - ".Inspect"
Cohesion: 0.38
Nodes (4): Collider, Component, Transform, Story513RageTargetInspection

### Community 149 - "FileLayout"
Cohesion: 0.33
Nodes (6): FileLayout, ConflictRecord, ControlRecord, DeferredRecord, DispositionRecord, WidthRecord

### Community 150 - ".NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId"
Cohesion: 0.33
Nodes (3): Collider, Renderer, Type

### Community 151 - "PlayerLifecycle"
Cohesion: 0.33
Nodes (5): PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 153 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 154 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 155 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 156 - "DefinitionId"
Cohesion: 0.15
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 157 - "RoadRage.Shared.Networking"
Cohesion: 0.15
Nodes (7): VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking, RoadRage.Features.Boss

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (13): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+5 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (21): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+13 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.10
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "PassengerActionDef"
Cohesion: 0.07
Nodes (25): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+17 more)

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

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 824 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.12
Nodes (8): BoxCollider, Collider, GameObject, Object, TearDown, Test, Vector3, Story510LaneGraphAndRoutedTrafficTests

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.16
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **593 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+588 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1194 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **13 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RoadRage.Tests.EditMode` connect `RoadRage.Features.Vehicles` to `ShapeGuardRegister.cs`, `RoundaboutClearance`, `RoadRage.Features.UI`, `RoadModelSource`, `OracleEvidenceClassification`, `OnlineServicesBootstrapService`, `RoadRage.Shared.Domain`, `RoadRage.Features.Players`, `.Compare`, `RoadRage.Shared.Networking`, `RoadRage.Shared.Input`?**
  _High betweenness centrality (0.166) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `RoadModelDocument`, `RoundaboutClearance`, `RoadModelSource`, `RoadModelCanonicalWriter`, `RoadCurve`, `CompiledRoadModel`, `PassengerActionDef`, `.Localize`, `Story526GeometryAndLocalizationTests`, `Story525RoadWorldModelTests`, `.Validate`, `AuthoringDecisions`?**
  _High betweenness centrality (0.125) - this node is a cross-community bridge._
- **Why does `DefinitionId` connect `DefinitionId` to `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests`, `Story13CharacterSetupTests`, `Empty Map Entry Playmode Tests`, `NetworkedAIVehicleState`, `RageTuningDef`, `RageTuningCatalog`, `VehicleProfileDef`, `MainMenuScreen`, `TrafficSettingsDef`, `.MenuResolvesProfileAndPublishesTheChosenCharacter`, `LobbyCodeClipboard`, `PassengerActionDef`, `Story59ParameterizedDriverModelTests`, `MonoBehaviour`, `PlayerProfile`?**
  _High betweenness centrality (0.121) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _593 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10303030303030303 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `GreyboxAssetSeedMetadata` be split into smaller, more focused modules?**
  _Cohesion score 0.06233766233766234 - nodes in this community are weakly interconnected._