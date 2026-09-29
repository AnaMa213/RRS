# Graph Report - RRS  (2026-09-29)

## Corpus Check
- 237 files · ~329,193 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 5519 nodes · 14628 edges · 189 communities (179 shown, 9 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 646 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `883a0734`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- GreyboxAssetSeedMetadata
- Story516LobbyConfigurableTrafficSettingsTests
- Story512TireForcesAndSteeringTests
- NetworkedVehicleState
- .Localize
- TireSample
- Story511VehicleChassisWheelsAndSuspensionTests
- TrafficFrame
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- NetworkedPassengerActionIntent
- LocalOnFootController
- Story12LobbyShellTests
- Story51NpcRageFearFoundationTests
- MenuCharacterPreview
- MigrationReport
- VehiclePhysicsBody
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- RunEscapeMenuScreen
- AuthoredRoadModel
- Story27PlayerLifecycleTests
- .CreatePlan
- Story513ArcadeAssistsAndUnevenGroundTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- Story550CompatibilityGolden
- AuthoringDecisions
- .Parse
- TraceDivergenceField
- NetworkedVehicleDriverController
- TrafficOracleTests
- PhysicsHarness
- RoadModelDocument
- VehicleProfile
- RoundaboutClearance
- .NewFixture
- LobbyJoinOutcome
- Story26InGameHudTests
- LobbyRosterSnapshot
- RoadRage.Features.UI
- FakeSteamLobbyPlatform
- TrafficSettingsDef
- Story514AiDrivesByIntentTests
- PassengerActionDef
- .FixedUpdate
- RoadId
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- JunctionClearance
- Story43PassengerActionOneChangesRageTests
- ImportContext
- CharacterDef
- Story54RageDrivenAiBehaviorStatesTests
- NetworkedAIVehicleState
- Story28Epic2OnlinePlayableCheckpointTests
- Story528AuthoringAndGateATests
- Story525RoadWorldModelTests
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- LobbyFlowController
- Story55NetworkedAiRageTargetingTests
- LobbyShellScreen
- LobbyJoinService
- LobbyRosterService
- RoadRage.Shared.Domain
- PairReviewEntry
- .Capture
- TrafficDecisionProjection
- PlanningSpine.cs
- RageTuningDef
- Story15EmptyMapEntryTests
- MonoBehaviour
- NetworkedRunSessionMonitor
- RoutePlan
- FakeSteamLobbyPlatform
- Story57AiTrafficClientPresentationTests
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- V1ImportResult
- RoadRage.Shared.Networking
- Story56RageRoadEventTriggerTests
- RoadRageScaffoldTests
- Story33SeatEntryExitAndPassengerPresenceTests
- RoadRage.Features.Players
- Story36Epic3DrivingPlayableCheckpointTests
- LobbyRosterScreen
- RunEscapeMenuFlowController
- MotionPlan.cs
- Story526GeometryAndLocalizationTests
- RoadLineage
- DefinitionId
- .Extract
- .Validate
- RoadRage.Features.Vehicles
- FakeSteamLobbyPlatform
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- Difficulty
- .WithScratchScene
- OracleEvidenceClassification
- .FullPath
- .Plan
- LaneNode
- MainMenuScreen
- Story512TireForcesAndSteeringPlayModeTests
- NetworkedRageState
- RoadCurveSample
- Story22HostCreatedPrivateRoomPlayModeTests
- Story58EscapeMenuTests
- SweepPose
- PassengerActionIntent
- NetworkedPlayerLifecycleIntent
- RoadRage.Shared.Input
- NpcReactionEffect
- .ReplayRoute
- VehicleProfileDef
- MotionPlan
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- .Load
- Story58EscapeMenuPlayModeTests
- NetworkedPlayerReviveIntent
- .HandleLifecycleChanged
- NetworkPlayerRegistry
- NetworkedVehicleSeatService
- LobbyPlayerSlotView
- RoadRageBootstrap
- NetworkedVehicleRecoveryIntent
- Story12LobbyShellPlayModeTests
- FakeSteamLobbyPlatform
- RoadModelRecords.cs
- ModelDto
- .Plan
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- RoadModelValidationCode
- PathHorizon
- .Compare
- Story510LaneGraphAndRoutedTrafficTests
- Story550DrivabilityTests
- .BuildSmoothCurve
- MainMenuProfileFlowController
- .MeasurePath
- RoadModelCanonicalPayload
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- .EnsureVehicleSandboxSeatHarness
- Story527MigrationTests
- .EnsureNetworkManagerRepairsExistingSingletonWithoutNetworkConfig
- .Read
- NetworkedVehicleSeatIntent
- VehicleWheel
- DrivabilityProfile
- .Manifest
- Story551JunctionClearanceTests
- .ComputeCollisionDamage
- StatusFilter
- UserNotice
- PlanningDecision
- .TearDown
- AppSceneRouter
- .Bind
- FacepunchSteamPlatform
- .RecoverVehicle
- PlayerMode
- MovementRole
- .UpdateSteeringState
- TestSuiteCategoryPartitionTests
- .Measure
- Story11MainMenuLaunchTests
- PlayerProfileResolution
- FakeSteamPlatform
- .Degrees
- NetworkedRunState
- NetworkedPlayerState
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- LocalVoidRespawnController
- Story21OnlineServicesPlayModeTests
- Lock-Rage Camera Fix Query
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RoadId` - 139 edges
2. `RunFlowController` - 118 edges
3. `Story526GeometryAndLocalizationTests` - 89 edges
4. `Story525RoadWorldModelTests` - 81 edges
5. `NetworkedVehicleState` - 79 edges
6. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
7. `RoadRage.Features.Vehicles` - 77 edges
8. `RunCheckpointHudScreen` - 69 edges
9. `ImportContext` - 67 edges
10. `CompiledRoadModel` - 66 edges

## Surprising Connections (you probably didn't know these)
- `RoadRageBootstrap` --references--> `AppSceneRouter`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/App/Services/AppSceneRouter.cs
- `RoadRageBootstrap` --references--> `ISteamIdentitySource`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/ISteamIdentitySource.cs
- `RoadRageBootstrap` --references--> `LobbyJoinService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyJoinService.cs
- `RoadRageBootstrap` --references--> `LobbyRoomService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyRoomService.cs
- `RoadRageBootstrap` --references--> `LobbyRosterService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyRosterService.cs

## Import Cycles
- None detected.

## Communities (189 total, 9 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.09
Nodes (18): List, CharacterCatalog, Count, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath (+10 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (11): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, ArgumentNullException, List, Object, TearDown, Test (+3 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.13
Nodes (8): Collider, GameObject, NetworkObject, Renderer, Test, TextMeshProUGUI, Type, Story25NetworkedPlayerSpawnTests

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.06
Nodes (28): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+20 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.10
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.12
Nodes (4): GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 7 - ".Localize"
Cohesion: 0.15
Nodes (18): Dictionary, IReadOnlyList, List, Vector3, Query, RoadElementKind, None, RoadLocalizer (+10 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.11
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "TrafficFrame"
Cohesion: 0.12
Nodes (26): CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation, RoadModelVersion, VehicleFootprintPose, TrafficActor, TrafficActorInput (+18 more)

### Community 11 - "RunFlowController"
Cohesion: 0.06
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.06
Nodes (27): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+19 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "NetworkedPassengerActionIntent"
Cohesion: 0.06
Nodes (33): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+25 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Story12LobbyShellTests"
Cohesion: 0.11
Nodes (14): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject (+6 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.17
Nodes (6): List, NetworkObject, Object, Test, Story51NpcRageFearFoundationTests, RageThreshold

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "MigrationReport"
Cohesion: 0.09
Nodes (28): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+20 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.10
Nodes (16): Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees (+8 more)

### Community 22 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 23 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (36): ArgumentOutOfRangeException, Bounds, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IReadOnlyList, List (+28 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.07
Nodes (13): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed (+5 more)

### Community 26 - ".CreatePlan"
Cohesion: 0.10
Nodes (18): Dictionary, HashSet, IList, List, RoadId, RoadModelValidationProfile, Vector3, AutomatedPairDecisionManifest (+10 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.06
Nodes (26): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, MeshFilter (+18 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.13
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "Story550CompatibilityGolden"
Cohesion: 0.09
Nodes (20): RoadModelVersion, High, IsEmpty, Low, SchemaVersion, RoadLocalizationProfile, LaneCorridor, Portal (+12 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.07
Nodes (32): Func, IList, JunctionControlKind, List, RoadCurveSample, AuthoringDecisions, ConflictRecord, ControlDecision (+24 more)

### Community 32 - ".Parse"
Cohesion: 0.10
Nodes (19): FileLayout, AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, ConflictDecision, ConflictDecisionKind (+11 more)

### Community 33 - "TraceDivergenceField"
Cohesion: 0.14
Nodes (14): TraceDivergenceField, Blockers, BrakeReverse, FrameCountMismatch, Goal, Handbrake, Position, RoadElementId (+6 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.09
Nodes (14): Collider, NetworkObjectReference, NetworkTransform, Rigidbody, Rpc, RpcParams, Transform, NetworkedVehicleDriverController (+6 more)

### Community 35 - "TrafficOracleTests"
Cohesion: 0.13
Nodes (4): Action, Scene, Test, TrafficOracleTests

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "RoadModelDocument"
Cohesion: 0.11
Nodes (23): Func, AdjacencyDto, ConnectionDto, ControlDto, CorridorDto, EntryDto, GroupDto, GroupStateDto (+15 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.09
Nodes (22): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+14 more)

### Community 40 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 44 - "LobbyRosterSnapshot"
Cohesion: 0.10
Nodes (23): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+15 more)

### Community 45 - "RoadRage.Features.UI"
Cohesion: 0.20
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "TrafficSettingsDef"
Cohesion: 0.08
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation (+12 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.20
Nodes (5): Func, GameObject, Rigidbody, Test, Story514AiDrivesByIntentTests

### Community 49 - "PassengerActionDef"
Cohesion: 0.09
Nodes (23): List, PassengerActionCatalog, Count, Version, PassengerActionDef, CooldownSeconds, DisplayName, Id (+15 more)

### Community 50 - ".FixedUpdate"
Cohesion: 0.08
Nodes (18): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+10 more)

### Community 51 - "RoadId"
Cohesion: 0.07
Nodes (32): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+24 more)

### Community 52 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 53 - "JunctionClearance"
Cohesion: 0.11
Nodes (17): Surface, Component, HashSet, IEnumerable, IReadOnlyList, List, Mesh, MeshFilter (+9 more)

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "ImportContext"
Cohesion: 0.11
Nodes (27): Dictionary, JunctionFeature, List, Predicate, RoadBoundsBox, RoadCurve, RoadCurveSample, ImportContext (+19 more)

### Community 56 - "CharacterDef"
Cohesion: 0.25
Nodes (8): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.12
Nodes (11): AiVehicleFixture, IRageDispositionSource, CurrentDisposition, GameObject, List, NetworkObject, Object, TearDown (+3 more)

### Community 58 - "NetworkedAIVehicleState"
Cohesion: 0.38
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 59 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.17
Nodes (10): ApprovalResult, NetworkPlayerConnectionPayload, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+2 more)

### Community 60 - "Story528AuthoringAndGateATests"
Cohesion: 0.08
Nodes (18): Action, BoxCollider, Collider, FormatException, GameObject, IReadOnlyList, KeyValuePair, List (+10 more)

### Community 61 - "Story525RoadWorldModelTests"
Cohesion: 0.07
Nodes (14): JunctionMovement, RoadModelSource, RoadModelCompilationException, Issues, Action, FormatException, JunctionMovement, LaneCorridor (+6 more)

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "LobbyFlowController"
Cohesion: 0.14
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.13
Nodes (12): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, IntentFixture (+4 more)

### Community 65 - "LobbyShellScreen"
Cohesion: 0.22
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 66 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 67 - "LobbyRosterService"
Cohesion: 0.06
Nodes (20): Difficulty, Task, ISteamLobbyPlatform, Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status (+12 more)

### Community 68 - "RoadRage.Shared.Domain"
Cohesion: 0.09
Nodes (7): SessionTrafficValue, RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.App.Run, RoadRage.Features.Rage, RoadRage.Features.PassengerActions

### Community 69 - "PairReviewEntry"
Cohesion: 0.05
Nodes (46): CompiledRoadModel, Dictionary, DrivabilityProfile, IReadOnlyList, List, RoadBoundsBox, RoadCurveSample, RoadId (+38 more)

### Community 70 - ".Capture"
Cohesion: 0.18
Nodes (8): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, Vector3, HistoricalPairFingerprintRecord, HistoricalPairFingerprintTable, PairGeometryFingerprint

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): IReadOnlyList, RoadElementKind, RoadId, TrafficDecisionProjection, Code, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "PlanningSpine.cs"
Cohesion: 0.24
Nodes (6): PlanningTolerances, PlanningSpine, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing

### Community 73 - "RageTuningDef"
Cohesion: 0.07
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 75 - "MonoBehaviour"
Cohesion: 0.11
Nodes (15): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+7 more)

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "RoutePlan"
Cohesion: 0.06
Nodes (39): CompiledRoadModel, IReadOnlyCollection, IReadOnlyList, RoadElementKind, RoadId, RoadLocation, RoadModelVersion, DecisionCounter (+31 more)

### Community 78 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 81 - "V1ImportResult"
Cohesion: 0.07
Nodes (29): RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift (+21 more)

### Community 82 - "RoadRage.Shared.Networking"
Cohesion: 0.11
Nodes (11): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel (+3 more)

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.08
Nodes (18): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, RageDisposition, Block, Calm, ConfrontationCapable, Flee (+10 more)

### Community 84 - "RoadRageScaffoldTests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 86 - "RoadRage.Features.Players"
Cohesion: 0.22
Nodes (3): RoadRage.Features.Players, RoadRage.Shared.Authoring, RoadRage.Shared.Definitions

### Community 87 - "Story36Epic3DrivingPlayableCheckpointTests"
Cohesion: 0.19
Nodes (8): AssemblyDefinition, Component, GameObject, NetworkObject, Scene, Test, AssemblyDefinition, Story36Epic3DrivingPlayableCheckpointTests

### Community 89 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 90 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (24): MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing, GateAEvidenceStale, HorizonNonConforming (+16 more)

### Community 91 - "Story526GeometryAndLocalizationTests"
Cohesion: 0.10
Nodes (6): Action, ArgumentException, LaneCorridor, Test, Vector3, Story526GeometryAndLocalizationTests

### Community 92 - "RoadLineage"
Cohesion: 0.13
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "DefinitionId"
Cohesion: 0.13
Nodes (14): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+6 more)

### Community 94 - ".Extract"
Cohesion: 0.14
Nodes (17): Func, GameObject, IReadOnlyList, LaneGraph, LaneNode, List, Scene, Vector3 (+9 more)

### Community 95 - ".Validate"
Cohesion: 0.17
Nodes (16): RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor, Movement, Section (+8 more)

### Community 96 - "RoadRage.Features.Vehicles"
Cohesion: 0.09
Nodes (7): IReadOnlyList, ShapeGuardEntry, ShapeGuardRegister, RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Tests.EditMode, RoadRage.Features.Vehicles, RoadRage.Features.Vehicles.Traffic

### Community 97 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (15): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+7 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.05
Nodes (35): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+27 more)

### Community 100 - "Vector3"
Cohesion: 0.28
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "Difficulty"
Cohesion: 0.18
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 102 - ".WithScratchScene"
Cohesion: 0.24
Nodes (7): Action, Collider, HashSet, Mesh, MeshCollider, Rigidbody, Vector3

### Community 103 - "OracleEvidenceClassification"
Cohesion: 0.18
Nodes (13): IReadOnlyList, BoundTest, OracleCatalog, OracleEvidenceClassification, AutoEdit, AutoPlay, Gap, Manual (+5 more)

### Community 104 - ".FullPath"
Cohesion: 0.19
Nodes (8): Action, IList, KeyValuePair, MenuItem, Scene, MenuItem, MenuItem, Func

### Community 105 - ".Plan"
Cohesion: 0.31
Nodes (4): LaneConnection, IReadOnlyCollection, Test, Story529RoutePlanTests

### Community 106 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.11
Nodes (11): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+3 more)

### Community 108 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 109 - "NetworkedRageState"
Cohesion: 0.10
Nodes (19): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner, Canvas, GameObject (+11 more)

### Community 110 - "RoadCurveSample"
Cohesion: 0.09
Nodes (23): Bounds, Vector3, RoadCurve, FullBounds, Length, StartS, RoadCurvePoint, RoadProjection (+15 more)

### Community 111 - "Story22HostCreatedPrivateRoomPlayModeTests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 113 - "SweepPose"
Cohesion: 0.07
Nodes (41): CompiledRoadModel, Dictionary, IList, IReadOnlyList, List, RoadBoundsBox, RoadCurve, RoadCurveSample (+33 more)

### Community 114 - "PassengerActionIntent"
Cohesion: 0.22
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 115 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 117 - "NpcReactionEffect"
Cohesion: 0.10
Nodes (14): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+6 more)

### Community 118 - ".ReplayRoute"
Cohesion: 0.14
Nodes (9): Action, Bounds, List, Scene, Transform, CurbTrafficMeasurement, ReplayTrace, CurbTrafficMeasurement (+1 more)

### Community 119 - "VehicleProfileDef"
Cohesion: 0.20
Nodes (7): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty

### Community 120 - "MotionPlan"
Cohesion: 0.16
Nodes (12): DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue, IssueDistanceMeters (+4 more)

### Community 121 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 122 - ".Load"
Cohesion: 0.22
Nodes (7): BindingDto, DocumentDto, RoadModelProvenance, FormatException, BindingDto, DocumentDto, ModelDto

### Community 123 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 124 - "NetworkedPlayerReviveIntent"
Cohesion: 0.16
Nodes (9): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority (+1 more)

### Community 126 - "NetworkPlayerRegistry"
Cohesion: 0.29
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 127 - "NetworkedVehicleSeatService"
Cohesion: 0.14
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 128 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, Image, TMP_Text, LobbyPlayerSlotView

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.11
Nodes (17): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+9 more)

### Community 130 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 131 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 132 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (70): Comparison, Vector3, ConflictZone, DrivabilityProfile, ImportManifest, ImportManifestEntry, Junction, JunctionControl (+62 more)

### Community 134 - "ModelDto"
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 135 - ".Plan"
Cohesion: 0.17
Nodes (13): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+5 more)

### Community 136 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (44): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference, DrivabilityAdmissionFailed (+36 more)

### Community 138 - "PathHorizon"
Cohesion: 0.07
Nodes (29): CompiledRoadModel, HashSet, IReadOnlyList, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd, ExitPortal (+21 more)

### Community 139 - ".Compare"
Cohesion: 0.19
Nodes (8): Vector3, List, TraceDivergence, TrafficTraceComparer, List, Vector3, TrafficTrace, TrafficTraceFrame

### Community 140 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.10
Nodes (9): BoxCollider, Collider, GameObject, Object, Renderer, TearDown, Test, Vector3 (+1 more)

### Community 141 - "Story550DrivabilityTests"
Cohesion: 0.27
Nodes (4): DrivabilityProfile, Action, Test, Story550DrivabilityTests

### Community 142 - ".BuildSmoothCurve"
Cohesion: 0.29
Nodes (3): RoadCurvePoint, Vector3, CircleFit

### Community 143 - "MainMenuProfileFlowController"
Cohesion: 0.20
Nodes (5): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, Component, RawImage

### Community 145 - "RoadModelCanonicalPayload"
Cohesion: 0.24
Nodes (6): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalPayload, RoadModelCanonicalWriter

### Community 146 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 147 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 148 - "Story527MigrationTests"
Cohesion: 0.13
Nodes (6): Action, GameObject, Scene, Test, TestCase, Story527MigrationTests

### Community 149 - ".EnsureNetworkManagerRepairsExistingSingletonWithoutNetworkConfig"
Cohesion: 0.43
Nodes (3): NetworkManager, NetworkPrefabsList, TearDown

### Community 150 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 154 - ".Manifest"
Cohesion: 0.27
Nodes (7): IList, ImportManifestEntry, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys

### Community 155 - "Story551JunctionClearanceTests"
Cohesion: 0.14
Nodes (12): BoxCollider, GameObject, IReadOnlyList, List, MeshFilter, MeshRenderer, Scene, Test (+4 more)

### Community 157 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 158 - "UserNotice"
Cohesion: 0.27
Nodes (7): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning

### Community 159 - "PlanningDecision"
Cohesion: 0.15
Nodes (12): RoadId, RoadLocation, VehicleFootprint, AgentObservation, PlanningDecision, Motion, Observation, Path (+4 more)

### Community 161 - "AppSceneRouter"
Cohesion: 0.25
Nodes (4): AppPlayModeEntry, AppSceneRouter, PlayModeStateChange, SceneAsset

### Community 162 - ".Bind"
Cohesion: 0.19
Nodes (10): CompiledRoadModel, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale, Valid (+2 more)

### Community 163 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 164 - ".RecoverVehicle"
Cohesion: 0.20
Nodes (4): Action, Quaternion, Vector3, Collision

### Community 165 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 166 - "MovementRole"
Cohesion: 0.33
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 168 - "TestSuiteCategoryPartitionTests"
Cohesion: 0.36
Nodes (4): List, Test, TestSuiteCategoryPartitionTests, MethodInfo

### Community 169 - ".Measure"
Cohesion: 0.13
Nodes (19): BoxCollider, Collider, CompiledRoadModel, GameObject, Scene, Vector3, VehiclePhysicsBody, VehicleProfile (+11 more)

### Community 170 - "Story11MainMenuLaunchTests"
Cohesion: 0.13
Nodes (11): UserNoticeChannel, LastNotice, Canvas, Component, EventSystem, InputSystemUIInputModule, Scene, SerializeField (+3 more)

### Community 171 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 175 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 186 - "NetworkedRunState"
Cohesion: 0.09
Nodes (17): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, Camera, TMP_Text (+9 more)

### Community 200 - "NetworkedPlayerState"
Cohesion: 0.11
Nodes (15): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+7 more)

### Community 216 - "LaneGraph"
Cohesion: 0.10
Nodes (15): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+7 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (21): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+13 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.11
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.07
Nodes (15): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+7 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.17
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

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **753 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+748 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1423 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **9 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RoadRage.Tests.EditMode` connect `RoadRage.Features.Vehicles` to `RoadRage.Shared.Domain`, `OracleEvidenceClassification`, `PlanningSpine.cs`, `TestSuiteCategoryPartitionTests`, `.Compare`, `RoadRage.Features.UI`, `RoadRage.Shared.Networking`, `RoadRage.Shared.Input`, `RoadRage.Features.Players`, `.CreatePlan`, `Story550CompatibilityGolden`?**
  _High betweenness centrality (0.162) - this node is a cross-community bridge._
- **Why does `RoadRage.Features.Vehicles` connect `RoadRage.Features.Vehicles` to `.Compare`, `NetworkedVehicleDamageVfxController`, `VehiclePhysicsBody`, `.Read`, `VehicleWheel`, `Story513ArcadeAssistsAndUnevenGroundTests`, `NetworkedVehicleDriverController`, `VehicleProfile`, `.UpdateSteeringState`, `RoadRage.Features.UI`, `.FixedUpdate`, `RoadRage.Shared.Domain`, `PlanningSpine.cs`, `MonoBehaviour`, `RoadRage.Shared.Networking`, `RoadRage.Features.Players`, `LaneGraph`, `LaneNode`, `RoadRage.Shared.Input`, `Story59ParameterizedDriverModelTests`?**
  _High betweenness centrality (0.142) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `RoadModelDocument`, `RoadModelRecords.cs`, `.Localize`, `.Plan`, `TrafficFrame`, `PathHorizon`, `RoadCurveSample`, `.MeasurePath`, `RoadModelCanonicalPayload`, `SweepPose`, `PassengerActionIntent`, `Story527MigrationTests`, `Story526GeometryAndLocalizationTests`, `Story525RoadWorldModelTests`, `Story550CompatibilityGolden`, `.Validate`?**
  _High betweenness centrality (0.124) - this node is a cross-community bridge._
- **Are the 6 inferred relationships involving `RoadId` (e.g. with `.FourEntriesReplayAcrossCompiledHorizonsToExit()` and `.AdmissionAndRouteFailuresHaveStableOutcomes()`) actually correct?**
  _`RoadId` has 6 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _753 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.08672699849170437 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.08941176470588236 - nodes in this community are weakly interconnected._