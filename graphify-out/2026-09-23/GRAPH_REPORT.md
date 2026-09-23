# Graph Report - RRS  (2026-09-23)

## Corpus Check
- 218 files · ~270,026 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4676 nodes · 11964 edges · 177 communities (164 shown, 10 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 522 edges (avg confidence: 0.82)
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
- MigrationReport
- VehiclePhysicsBody
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- Story549RoundaboutWideningTests
- AuthoredRoadModel
- Story27PlayerLifecycleTests
- List
- Story513ArcadeAssistsAndUnevenGroundTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- RoadRage.App.Run
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
- LobbyRosterSnapshot
- Story28Epic2OnlinePlayableCheckpointTests
- FakeSteamLobbyPlatform
- TrafficSettingsDef
- Story514AiDrivesByIntentTests
- RoadRage.Shared.Domain
- RoadRage.Features.Players
- RoadId
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
- CharacterCatalog
- LobbyRosterService
- NetworkedPlayerLifecycleService
- NetworkedVehicleSeatIntent
- RoadRageBootstrap
- .NewFixture
- .Inspect
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
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
- .EnsureVehicleSandboxSeatHarness
- Vector3
- Story526GeometryAndLocalizationTests
- RoadLineage
- DefinitionId
- .Extract
- .Validate
- PassengerActionVerdictCode
- RoadModelCanonicalWriter
- GateAReviewWindow
- LocalVehicleCameraRig
- .SelectWeightedSuccessor
- V1ImportResult
- NetworkedPlayerState
- OracleEvidenceClassification
- Story527MigrationTests
- .Add
- FakeSteamPlatform
- MainMenuScreen
- RunFlowController
- .Find
- RoadCurve
- Private Room Play Mode Tests
- Story58EscapeMenuTests
- NetworkedPlayerLifecycleIntent
- Netcode/Steamworks Smoke Tests
- ReactionChannel
- .Append
- NetworkedVehicleRecoveryIntent
- LaneNode
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- MonoBehaviour
- .NewTuning
- TraceDivergenceField
- NetworkPlayerRegistry
- UserNotice
- RageRoadEventState
- LobbyPlayerSlotView
- RageSandboxAutoStart
- ShapeGuardRegister.cs
- Story12LobbyShellPlayModeTests
- Story516ForceAssetRefresh
- RoadModelRecords.cs
- Story58EscapeMenuPlayModeTests
- .Run
- NetworkedAIVehicleState
- RoadModelValidationCode
- FacepunchSteamPlatform
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .ResolveFirstTriggerCandidate
- LineageKeyRegistry
- VehicleDriveIntent
- MainMenuFlowController
- .RequestHonk
- FakeSteamLobbyPlatform
- .FixedUpdate
- MovementRole
- FileLayout
- TelemetrySample
- PlayerLifecycle
- .ComputeCollisionDamage
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- PlayerMode
- AppSceneRouter.cs
- CharacterDef
- NetworkedVehicleState.cs
- .Degrees
- .TearDown
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

## Communities (177 total, 10 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.10
Nodes (16): PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, ArgumentException, Component (+8 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (11): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, ArgumentNullException, List, Object, TearDown, Test (+3 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.11
Nodes (11): Collider, GameObject, NetworkManager, NetworkObject, NetworkPrefabsList, Renderer, TearDown, Test (+3 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.06
Nodes (28): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+20 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.10
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.09
Nodes (7): VehicleSteeringModel, GameObject, Rigidbody, SerializedObject, SerializedProperty, Test, Story512TireForcesAndSteeringTests

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.13
Nodes (11): UserNoticeChannel, LastNotice, Canvas, CanvasScaler, Component, EventSystem, InputSystemUIInputModule, Scene (+3 more)

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.09
Nodes (12): Vector3, TelemetrySample, Vector3, Collider, Func, GameObject, List, Rigidbody (+4 more)

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

### Community 20 - "MigrationReport"
Cohesion: 0.09
Nodes (27): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+19 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.06
Nodes (31): Collider, Component, Transform, Story513RageTargetInspection, Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody (+23 more)

### Community 22 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 23 - "Story549RoundaboutWideningTests"
Cohesion: 0.10
Nodes (12): RoadModelValidationProfile, Action, BoxCollider, IReadOnlyList, MeshCollider, Scene, Test, Vector3 (+4 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.08
Nodes (24): Action, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, KeyValuePair, List (+16 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.10
Nodes (6): IEnumerable, CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests

### Community 26 - "List"
Cohesion: 0.16
Nodes (8): Action, Bounds, List, Scene, Transform, CurbTrafficMeasurement, ReplayTrace, CurbTrafficMeasurement

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.08
Nodes (15): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, Scene (+7 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.10
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "RoadRage.App.Run"
Cohesion: 0.15
Nodes (4): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Features.PassengerActions, RoadRage.Shared.Input

### Community 31 - "AuthoringDecisions"
Cohesion: 0.08
Nodes (37): AppliedWidth, Dictionary, FileLayout, Func, IList, JunctionControlKind, List, RoadBoundsBox (+29 more)

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
Cohesion: 0.05
Nodes (44): AdjacencyDto, Comparison, RoadModelCompiler, Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto (+36 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.04
Nodes (50): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+42 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.20
Nodes (11): Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile, Vector2 (+3 more)

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.21
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

### Community 44 - "LobbyRosterSnapshot"
Cohesion: 0.10
Nodes (23): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+15 more)

### Community 45 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.11
Nodes (14): ApprovalResult, NetworkPlayerConnectionPayload, FakeSteamLobbyPlatform, GameObject, NetworkObject, Test, ApprovalResult, Approved (+6 more)

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
Cohesion: 0.08
Nodes (8): NetworkVariable, NetworkedCrewEconomyState, SessionTrafficValue, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.Features.Economy, RoadRage.Shared.Networking, RoadRage.Features.Boss

### Community 50 - "RoadRage.Features.Players"
Cohesion: 0.16
Nodes (3): RoadRage.Features.Players, RoadRage.Shared.Authoring, RoadRage.Shared.Definitions

### Community 51 - "RoadId"
Cohesion: 0.06
Nodes (40): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+32 more)

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
Cohesion: 0.12
Nodes (9): Difficulty, Button, TMP_InputField, TMP_Text, LobbyShellScreen, Difficulty, Easy, Hard (+1 more)

### Community 56 - "NetworkedRunState"
Cohesion: 0.20
Nodes (6): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState

### Community 57 - "NetworkedRageState"
Cohesion: 0.09
Nodes (21): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, RageDisposition, Block (+13 more)

### Community 58 - ".Localize"
Cohesion: 0.11
Nodes (23): RoadModelVersion, High, IsEmpty, Low, SchemaVersion, Dictionary, IReadOnlyList, List (+15 more)

### Community 59 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "Story528AuthoringAndGateATests"
Cohesion: 0.17
Nodes (6): Action, FormatException, KeyValuePair, Scene, Test, Story528AuthoringAndGateATests

### Community 61 - "Story525RoadWorldModelTests"
Cohesion: 0.08
Nodes (12): JunctionMovement, ImportManifest, RoadIdRemap, RoadModelSource, Action, JunctionMovement, LaneCorridor, SignalPlan (+4 more)

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "RoadRage.Features.Vehicles"
Cohesion: 0.10
Nodes (4): RoadRage.DevTools, RoadRage.Tests.EditMode, RoadRage.Features.Rage, RoadRage.Features.Vehicles

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.13
Nodes (9): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, Story55NetworkedAiRageTargetingTests (+1 more)

### Community 65 - ".Author"
Cohesion: 0.28
Nodes (7): Bounds, Collider, GameObject, List, Transform, Vector3, Story513RecipeRelief

### Community 66 - "CharacterCatalog"
Cohesion: 0.20
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 67 - "LobbyRosterService"
Cohesion: 0.10
Nodes (9): Difficulty, Task, ISteamLobbyPlatform, Difficulty, LobbyRosterService, AllMembersReady, Current, IsSynchronized (+1 more)

### Community 69 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.09
Nodes (18): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+10 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 72 - ".Inspect"
Cohesion: 0.24
Nodes (6): Func, List, MonoBehaviour, Rigidbody, Transform, Story513DriverHandshakeInspection

### Community 73 - "RageTuningDef"
Cohesion: 0.09
Nodes (16): List, RageTuningCatalog, Count, RageTuningDef, FearSensitivity, HonkChannel, HonkMagnitude, HonkRange (+8 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 75 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "NetworkedPassengerActionIntent"
Cohesion: 0.12
Nodes (16): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, Action, PassengerActionDebugView (+8 more)

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
Cohesion: 0.15
Nodes (8): GameObject, List, NetworkObject, Object, TearDown, Test, TMP_Text, Story56RageRoadEventTriggerTests

### Community 84 - "RoadRageScaffoldTests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 86 - "ImportContext"
Cohesion: 0.12
Nodes (27): Dictionary, JunctionFeature, List, Predicate, RoadBoundsBox, RoadCurve, RoadCurveSample, Vector3 (+19 more)

### Community 87 - ".Compare"
Cohesion: 0.19
Nodes (8): Vector3, List, TraceDivergence, TrafficTraceComparer, List, Vector3, TrafficTrace, TrafficTraceFrame

### Community 88 - "LobbyFlowController"
Cohesion: 0.14
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 90 - "Vector3"
Cohesion: 0.28
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 91 - "Story526GeometryAndLocalizationTests"
Cohesion: 0.09
Nodes (9): JunctionMovement, RoadCurveSample, Action, ArgumentException, JunctionMovement, LaneCorridor, Test, Vector3 (+1 more)

### Community 92 - "RoadLineage"
Cohesion: 0.13
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "DefinitionId"
Cohesion: 0.14
Nodes (14): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+6 more)

### Community 94 - ".Extract"
Cohesion: 0.14
Nodes (16): Func, GameObject, IReadOnlyList, LaneGraph, LaneNode, List, Scene, V1ModuleKind (+8 more)

### Community 95 - ".Validate"
Cohesion: 0.15
Nodes (18): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+10 more)

### Community 96 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): Rpc, RpcParams, FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, PassengerActionValidation, PassengerActionVerdict, Accepted (+19 more)

### Community 97 - "RoadModelCanonicalWriter"
Cohesion: 0.28
Nodes (5): Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter, BinaryWriter

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
Cohesion: 0.10
Nodes (22): RoadId, AuthoringTask, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint (+14 more)

### Community 102 - "NetworkedPlayerState"
Cohesion: 0.12
Nodes (15): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, NetworkVariable, NetworkedBossState, FixedString32Bytes, NetworkVariable (+7 more)

### Community 103 - "OracleEvidenceClassification"
Cohesion: 0.18
Nodes (13): IReadOnlyList, BoundTest, OracleCatalog, OracleEvidenceClassification, AutoEdit, AutoPlay, Gap, Manual (+5 more)

### Community 104 - "Story527MigrationTests"
Cohesion: 0.14
Nodes (6): Action, GameObject, Scene, Test, TestCase, Story527MigrationTests

### Community 105 - ".Add"
Cohesion: 0.17
Nodes (14): ArgumentOutOfRangeException, Bounds, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation (+6 more)

### Community 106 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 107 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 108 - "RunFlowController"
Cohesion: 0.09
Nodes (5): Camera, CharacterController, HashSet, RunFlowController, ActiveLocalPlayer

### Community 110 - "RoadCurve"
Cohesion: 0.12
Nodes (20): Bounds, Vector3, RoadCurve, FullBounds, Length, StartS, RoadCurvePoint, RoadProjection (+12 more)

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 113 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 117 - "ReactionChannel"
Cohesion: 0.14
Nodes (8): TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage, ContextMenu

### Community 118 - ".Append"
Cohesion: 0.23
Nodes (8): Canvas, CanvasScaler, Component, StringBuilder, TMP_Text, Transform, Vector2, Story516LobbyPanelInspection

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
Cohesion: 0.13
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

### Community 127 - "RageRoadEventState"
Cohesion: 0.22
Nodes (6): RageRoadEventState, Confrontation, Idle, Resolved, RewardGranted, Triggered

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

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (66): IList, ImportManifestEntry, RoadLocalizationProfile, RoadModelSource, RoadModelCanonicalPayload, Vector3, ConflictZone, ImportManifestEntry (+58 more)

### Community 134 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 135 - ".Run"
Cohesion: 0.27
Nodes (10): Button, GameObject, Object, Scene, SerializedObject, StringBuilder, TMP_Text, Transform (+2 more)

### Community 136 - "NetworkedAIVehicleState"
Cohesion: 0.41
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.06
Nodes (34): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference, DuplicateId (+26 more)

### Community 138 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 139 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 140 - ".ResolveFirstTriggerCandidate"
Cohesion: 0.23
Nodes (3): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle

### Community 141 - "LineageKeyRegistry"
Cohesion: 0.43
Nodes (5): IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys

### Community 142 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 146 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 147 - ".FixedUpdate"
Cohesion: 0.09
Nodes (18): VehicleSuspensionModel, WheelState, Compression, ContactPoint, Grounded, HubPosition, TireSample, Vector2 (+10 more)

### Community 148 - "MovementRole"
Cohesion: 0.29
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 149 - "FileLayout"
Cohesion: 0.33
Nodes (6): FileLayout, ConflictRecord, ControlRecord, DeferredRecord, DispositionRecord, WidthRecord

### Community 150 - "TelemetrySample"
Cohesion: 0.33
Nodes (6): TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees, Speed

### Community 151 - "PlayerLifecycle"
Cohesion: 0.33
Nodes (5): PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 153 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

### Community 154 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 155 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 156 - "CharacterDef"
Cohesion: 0.12
Nodes (14): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+6 more)

### Community 157 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
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
Cohesion: 0.09
Nodes (22): List, PassengerActionCatalog, Count, Version, PassengerActionDef, CooldownSeconds, DisplayName, Id (+14 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.14
Nodes (10): RageThreshold, Disposition, MinValue, List, NetworkObject, Object, TearDown, Test (+2 more)

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
Cohesion: 0.11
Nodes (9): BoxCollider, Collider, GameObject, Object, Renderer, TearDown, Test, Vector3 (+1 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.15
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **593 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+588 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1193 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **10 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RoadRage.Tests.EditMode` connect `RoadRage.Features.Vehicles` to `ShapeGuardRegister.cs`, `RoadModelDocument`, `OracleEvidenceClassification`, `RoadRage.Features.UI`, `OnlineServicesBootstrapService`, `RoadRage.Shared.Domain`, `RoadRage.Features.Players`, `.Compare`, `Story549RoundaboutWideningTests`, `RoadRage.App.Run`?**
  _High betweenness centrality (0.154) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `PassengerActionVerdictCode`, `RoadModelCanonicalWriter`, `RoadModelDocument`, `RoadModelRecords.cs`, `RoadCurve`, `.Localize`, `Story526GeometryAndLocalizationTests`, `Story525RoadWorldModelTests`, `.Validate`, `AuthoringDecisions`?**
  _High betweenness centrality (0.134) - this node is a cross-community bridge._
- **Why does `DefinitionId` connect `DefinitionId` to `PassengerActionVerdictCode`, `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests`, `CharacterCatalog`, `Story13CharacterSetupTests`, `VehicleProfile`, `Empty Map Entry Playmode Tests`, `NetworkedPlayerPresentation`, `RageTuningDef`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `TrafficSettingsDef`, `.MenuResolvesProfileAndPublishesTheChosenCharacter`, `PassengerActionDef`, `Story59ParameterizedDriverModelTests`, `MonoBehaviour`, `CharacterDef`, `LobbyCodeClipboard`?**
  _High betweenness centrality (0.121) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _593 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.0971322849213691 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.08941176470588236 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.112375533428165 - nodes in this community are weakly interconnected._