# Graph Report - RRS  (2026-09-28)

## Corpus Check
- 232 files · ~323,720 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 5332 nodes · 14183 edges · 181 communities (172 shown, 9 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 578 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f77f04b0`
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
- Story527MigrationTests
- TelemetrySample
- Story511VehicleChassisWheelsAndSuspensionTests
- FakeSteamLobbyPlatform
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- NetworkedVehicleState
- LocalOnFootController
- Story12LobbyShellTests
- Story51NpcRageFearFoundationTests
- MenuCharacterPreview
- MigrationReport
- VehiclePhysicsBody
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- .Measure
- AuthoredRoadModel
- Story27PlayerLifecycleTests
- .CreatePlan
- Story513ArcadeAssistsAndUnevenGroundTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- Story550CompatibilityGolden
- AuthoringDecisions
- Story550PairReviewTests
- Story28Epic2OnlinePlayableCheckpointTests
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
- RoadRage.App.Run
- FakeSteamLobbyPlatform
- TrafficSettingsDef
- Story514AiDrivesByIntentTests
- PassengerActionDef
- TireSample
- RoadId
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- VehicleProfileDef
- Story43PassengerActionOneChangesRageTests
- ImportContext
- CharacterCatalog
- NetworkedRageState
- .Localize
- RoadRageBootstrap
- Story528AuthoringAndGateATests
- Story525RoadWorldModelTests
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- LobbyFlowController
- Story55NetworkedAiRageTargetingTests
- .Author
- List
- LobbyRosterSnapshot
- RoadRage.Shared.Domain
- PassengerActionVerdictCode
- .Capture
- .NewFixture
- .Inspect
- RageTuningDef
- Story15EmptyMapEntryTests
- NetworkedPassengerActionIntent
- NetworkedRunSessionMonitor
- Story551JunctionClearanceTests
- FakeSteamLobbyPlatform
- Story57AiTrafficClientPresentationTests
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- V1ImportResult
- Story513ArcadeAssistsAndUnevenGroundPlayModeTests
- Story56RageRoadEventTriggerTests
- RoadRageScaffoldTests
- Story33SeatEntryExitAndPassengerPresenceTests
- RoadRage.Features.Players
- Story36Epic3DrivingPlayableCheckpointTests
- LobbyRosterScreen
- RunEscapeMenuFlowController
- Vector3
- Story526GeometryAndLocalizationTests
- RoadLineage
- DefinitionId
- .Extract
- .Validate
- RoadRage.Features.Vehicles
- PairReviewEntry
- GateAReviewWindow
- LocalVehicleCameraRig
- RunEscapeMenuScreen
- Difficulty
- ModelDto
- OracleEvidenceClassification
- .SelectWeightedSuccessor
- .Load
- LaneNode
- MainMenuScreen
- NetworkedPlayerReviveIntent
- .Find
- RoadCurveSample
- Story22HostCreatedPrivateRoomPlayModeTests
- Story58EscapeMenuTests
- SweepPose
- Netcode/Steamworks Smoke Tests
- LobbyJoinService
- Story515CredibleCollisionsAndDamageIntegrationTests
- ReactionChannel
- Story550DrivabilityTests
- NetworkedVehicleRecoveryIntent
- NetworkedPlayerSpawnService
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- .FullPath
- .NewTuning
- NetworkedPlayerLifecycleIntent
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .MeasurePath
- NetworkedVehicleSeatService
- LobbyPlayerSlotView
- .EnsureNetworkManager
- NetworkedPlayerState
- LobbyShellScreen
- Story516ForceAssetRefresh
- RoadModelSource
- Story58EscapeMenuPlayModeTests
- .Run
- Story512TireForcesAndSteeringPlayModeTests
- RoadModelValidationCode
- .Append
- .Meters
- Story510LaneGraphAndRoutedTrafficTests
- .HandleLifecycleChanged
- .BuildSmoothCurve
- GameObject
- FakeSteamPlatform
- RoadModelCanonicalWriter
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- RageTuningCatalog
- .Manifest
- .Read
- NetworkedVehicleSeatIntent
- NetworkedAIVehicleState
- RageSandboxAutoStart
- .Create
- .WithScratchScene
- StatusFilter
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- FakeSteamLobbyPlatform
- AppSceneRouter.cs
- PlayerProfileResolution
- NetworkedVehicleState.cs
- ShapeGuardRegister.cs
- JunctionClearanceResult
- UserNotice
- DocumentDto
- .TearDown
- NetworkedRunState
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- MonoBehaviour
- Story21OnlineServicesPlayModeTests
- Lock-Rage Camera Fix Query
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RoadId` - 129 edges
2. `RunFlowController` - 118 edges
3. `Story526GeometryAndLocalizationTests` - 89 edges
4. `RoadRage.Features.Vehicles` - 81 edges
5. `Story525RoadWorldModelTests` - 81 edges
6. `NetworkedVehicleState` - 80 edges
7. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
8. `RunCheckpointHudScreen` - 69 edges
9. `ImportContext` - 67 edges
10. `RoadRage.Shared.Domain` - 66 edges

## Surprising Connections (you probably didn't know these)
- `RoadRageBootstrap` --references--> `LobbyJoinService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyJoinService.cs
- `RoadRageBootstrap` --references--> `LobbyRoomService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyRoomService.cs
- `RoadRageBootstrap` --references--> `LobbyRosterService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyRosterService.cs
- `RoadRageBootstrap` --references--> `OnlineServicesBootstrapService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/OnlineServicesBootstrapService.cs
- `RoadRageBootstrap` --references--> `NetworkPlayerRegistry`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Players/NetworkPlayerRegistry.cs

## Import Cycles
- None detected.

## Communities (181 total, 9 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.09
Nodes (17): PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, ArgumentException, Component (+9 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.14
Nodes (7): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry, Test, Story25NetworkedPlayerSpawnTests

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.06
Nodes (28): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+20 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.10
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.08
Nodes (8): VehicleSteeringModel, TireSample, Vector2, VehicleTireModel, GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.13
Nodes (11): UserNoticeChannel, LastNotice, Canvas, CanvasScaler, Component, EventSystem, InputSystemUIInputModule, Scene (+3 more)

### Community 7 - "Story527MigrationTests"
Cohesion: 0.14
Nodes (6): Action, GameObject, Scene, Test, TestCase, Story527MigrationTests

### Community 8 - "TelemetrySample"
Cohesion: 0.33
Nodes (6): TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees, Speed

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.07
Nodes (17): TelemetrySample, Vector3, VehicleSuspensionModel, WheelState, Compression, ContactPoint, Grounded, HubPosition (+9 more)

### Community 10 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

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

### Community 15 - "NetworkedVehicleState"
Cohesion: 0.12
Nodes (7): NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

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
Nodes (28): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+20 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.07
Nodes (21): Collider, Component, Transform, Story513RageTargetInspection, Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody (+13 more)

### Community 22 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.13
Nodes (8): Func, AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 23 - ".Measure"
Cohesion: 0.09
Nodes (28): Surface, BoxCollider, Collider, CompiledRoadModel, Component, GameObject, HashSet, IEnumerable (+20 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.08
Nodes (31): ArgumentOutOfRangeException, Bounds, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IReadOnlyList, List (+23 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.07
Nodes (13): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed (+5 more)

### Community 26 - ".CreatePlan"
Cohesion: 0.10
Nodes (18): Dictionary, HashSet, IList, List, MenuItem, RoadId, Vector3, AutomatedPairDecisionManifest (+10 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.08
Nodes (15): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, MeshFilter (+7 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.24
Nodes (6): PointerEventData, TMP_Text, LobbyCodeClipboard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

### Community 30 - "Story550CompatibilityGolden"
Cohesion: 0.09
Nodes (19): RoadModelVersion, High, IsEmpty, Low, SchemaVersion, LaneCorridor, Portal, Test (+11 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (44): Dictionary, FileLayout, Func, IList, JunctionControlKind, List, RoadCurveSample, RoadId (+36 more)

### Community 32 - "Story550PairReviewTests"
Cohesion: 0.11
Nodes (10): PairReviewActions, Action, FormatException, IEnumerable, InvalidOperationException, List, OneTimeTearDown, Scene (+2 more)

### Community 33 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.17
Nodes (10): ApprovalResult, NetworkPlayerConnectionPayload, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+2 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.09
Nodes (12): Action, Collider, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc, RpcParams (+4 more)

### Community 35 - "TrafficOracleTests"
Cohesion: 0.06
Nodes (32): VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle, Action, Scene (+24 more)

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "RoadModelDocument"
Cohesion: 0.11
Nodes (23): Func, AdjacencyDto, ConnectionDto, ControlDto, CorridorDto, EntryDto, GroupDto, GroupStateDto (+15 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.04
Nodes (45): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+37 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.08
Nodes (23): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+15 more)

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.20
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.08
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.22
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.25
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 45 - "RoadRage.App.Run"
Cohesion: 0.16
Nodes (3): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Shared.Input

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "TrafficSettingsDef"
Cohesion: 0.09
Nodes (19): Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation, TrafficSettingsDef (+11 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.20
Nodes (5): Func, GameObject, Rigidbody, Test, Story514AiDrivesByIntentTests

### Community 49 - "PassengerActionDef"
Cohesion: 0.07
Nodes (25): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+17 more)

### Community 50 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 51 - "RoadId"
Cohesion: 0.06
Nodes (36): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+28 more)

### Community 52 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 53 - "VehicleProfileDef"
Cohesion: 0.20
Nodes (7): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "ImportContext"
Cohesion: 0.13
Nodes (20): Dictionary, JunctionFeature, List, Predicate, RoadBoundsBox, RoadCurve, RoadCurveSample, ImportContext (+12 more)

### Community 56 - "CharacterCatalog"
Cohesion: 0.12
Nodes (14): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+6 more)

### Community 57 - "NetworkedRageState"
Cohesion: 0.11
Nodes (14): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, GameObject, List (+6 more)

### Community 58 - ".Localize"
Cohesion: 0.15
Nodes (18): Dictionary, IReadOnlyList, List, Vector3, Query, RoadElementKind, None, RoadLocalizer (+10 more)

### Community 59 - "RoadRageBootstrap"
Cohesion: 0.07
Nodes (18): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+10 more)

### Community 60 - "Story528AuthoringAndGateATests"
Cohesion: 0.09
Nodes (16): SignoffLayout, Action, BoxCollider, Collider, FormatException, GameObject, IReadOnlyList, KeyValuePair (+8 more)

### Community 61 - "Story525RoadWorldModelTests"
Cohesion: 0.07
Nodes (12): RoadModelCompilationException, Issues, Action, JunctionMovement, LaneCorridor, Portal, SignalPlan, Test (+4 more)

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "LobbyFlowController"
Cohesion: 0.12
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.12
Nodes (12): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, IntentFixture (+4 more)

### Community 65 - ".Author"
Cohesion: 0.28
Nodes (7): Bounds, Collider, GameObject, List, Transform, Vector3, Story513RecipeRelief

### Community 66 - "List"
Cohesion: 0.14
Nodes (8): Action, Bounds, List, Renderer, Scene, Transform, CurbTrafficMeasurement, CurbTrafficMeasurement

### Community 67 - "LobbyRosterSnapshot"
Cohesion: 0.05
Nodes (28): Difficulty, Task, ISteamLobbyPlatform, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount (+20 more)

### Community 68 - "RoadRage.Shared.Domain"
Cohesion: 0.06
Nodes (10): NetworkVariable, NetworkedCrewEconomyState, SessionTrafficValue, RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.Features.Rage, RoadRage.Features.Economy (+2 more)

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 70 - ".Capture"
Cohesion: 0.18
Nodes (9): BinaryWriter, IReadOnlyList, MenuItem, RoadBoundsBox, RoadCurveSample, Vector3, HistoricalPairFingerprintRecord, HistoricalPairFingerprintTable (+1 more)

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

### Community 75 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (22): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+14 more)

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "Story551JunctionClearanceTests"
Cohesion: 0.15
Nodes (12): BoxCollider, GameObject, IReadOnlyList, List, MeshFilter, MeshRenderer, Scene, Test (+4 more)

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
Cohesion: 0.06
Nodes (37): RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift (+29 more)

### Community 82 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

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
Cohesion: 0.13
Nodes (4): RoadRage.Features.Players, RoadRage.Shared.Authoring, RoadRage.Features.PassengerActions, RoadRage.Shared.Definitions

### Community 87 - "Story36Epic3DrivingPlayableCheckpointTests"
Cohesion: 0.19
Nodes (8): AssemblyDefinition, Component, GameObject, NetworkObject, Scene, Test, AssemblyDefinition, Story36Epic3DrivingPlayableCheckpointTests

### Community 89 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 90 - "Vector3"
Cohesion: 0.28
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 91 - "Story526GeometryAndLocalizationTests"
Cohesion: 0.10
Nodes (6): Action, ArgumentException, LaneCorridor, Test, Vector3, Story526GeometryAndLocalizationTests

### Community 92 - "RoadLineage"
Cohesion: 0.13
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "DefinitionId"
Cohesion: 0.13
Nodes (15): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+7 more)

### Community 94 - ".Extract"
Cohesion: 0.12
Nodes (21): Func, GameObject, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion, Scene (+13 more)

### Community 95 - ".Validate"
Cohesion: 0.18
Nodes (15): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+7 more)

### Community 96 - "RoadRage.Features.Vehicles"
Cohesion: 0.09
Nodes (4): RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Tests.EditMode, RoadRage.Features.Vehicles, RoadRage.Features.Vehicles.Traffic

### Community 97 - "PairReviewEntry"
Cohesion: 0.05
Nodes (45): CompiledRoadModel, Dictionary, DrivabilityProfile, IReadOnlyList, List, RoadBoundsBox, RoadCurveSample, RoadId (+37 more)

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (15): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+7 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.05
Nodes (35): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+27 more)

### Community 100 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 101 - "Difficulty"
Cohesion: 0.14
Nodes (10): Difficulty, Color, LobbyRosterEntry, DisplayName, PortraitTint, Ready, Difficulty, Easy (+2 more)

### Community 102 - "ModelDto"
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 103 - "OracleEvidenceClassification"
Cohesion: 0.18
Nodes (13): IReadOnlyList, BoundTest, OracleCatalog, OracleEvidenceClassification, AutoEdit, AutoPlay, Gap, Manual (+5 more)

### Community 104 - ".SelectWeightedSuccessor"
Cohesion: 0.30
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 105 - ".Load"
Cohesion: 0.30
Nodes (4): BindingDto, RoadModelProvenance, FormatException, DocumentDto

### Community 106 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.11
Nodes (11): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+3 more)

### Community 108 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 110 - "RoadCurveSample"
Cohesion: 0.09
Nodes (23): DrivabilityProfile, Bounds, Vector3, RoadCurve, FullBounds, Length, StartS, RoadCurvePoint (+15 more)

### Community 111 - "Story22HostCreatedPrivateRoomPlayModeTests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 113 - "SweepPose"
Cohesion: 0.06
Nodes (45): RoadBoundsBox, ConflictCandidate, RoadModelValidationProfile, CompiledRoadModel, Dictionary, IList, IReadOnlyList, List (+37 more)

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 115 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 117 - "ReactionChannel"
Cohesion: 0.29
Nodes (5): ReactionChannel, Both, Fear, None, Rage

### Community 118 - "Story550DrivabilityTests"
Cohesion: 0.29
Nodes (4): DrivabilityProfile, Action, Test, Story550DrivabilityTests

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.14
Nodes (11): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent, NetworkVariable, NetworkedBossState, HostOwnedNetworkStateBehaviour, IsHostAuthority (+3 more)

### Community 120 - "NetworkedPlayerSpawnService"
Cohesion: 0.17
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

### Community 121 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 122 - ".FullPath"
Cohesion: 0.21
Nodes (6): Action, IList, KeyValuePair, MenuItem, Scene, Func

### Community 124 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 125 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 127 - "NetworkedVehicleSeatService"
Cohesion: 0.14
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 128 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, Image, TMP_Text, LobbyPlayerSlotView

### Community 129 - ".EnsureNetworkManager"
Cohesion: 0.24
Nodes (7): GameObject, NetworkManager, NetworkPrefabsList, NetworkManager, NetworkPrefabsList, TearDown, FacepunchTransport

### Community 130 - "NetworkedPlayerState"
Cohesion: 0.11
Nodes (16): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart, FixedString32Bytes, NetworkVariable, Vector3 (+8 more)

### Community 131 - "LobbyShellScreen"
Cohesion: 0.16
Nodes (9): Button, TMP_InputField, TMP_Text, LobbyShellScreen, Component, IEnumerator, UnityTearDown, UnityTest (+1 more)

### Community 133 - "RoadModelSource"
Cohesion: 0.04
Nodes (72): EffectiveLaneCorridor, JunctionMovement, RoadModelCanonicalPayload, DrivabilityProfile, ImportManifest, ImportManifestEntry, Junction, JunctionControl (+64 more)

### Community 134 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 135 - ".Run"
Cohesion: 0.27
Nodes (10): Button, GameObject, Object, Scene, SerializedObject, StringBuilder, TMP_Text, Transform (+2 more)

### Community 136 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (44): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference, DrivabilityAdmissionFailed (+36 more)

### Community 138 - ".Append"
Cohesion: 0.23
Nodes (8): Canvas, CanvasScaler, Component, StringBuilder, TMP_Text, Transform, Vector2, Story516LobbyPanelInspection

### Community 140 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.10
Nodes (9): BoxCollider, Collider, GameObject, Object, TearDown, Test, Vector3, ReplayTrace (+1 more)

### Community 142 - ".BuildSmoothCurve"
Cohesion: 0.29
Nodes (3): RoadCurvePoint, Vector3, CircleFit

### Community 143 - "GameObject"
Cohesion: 0.17
Nodes (6): Collider, GameObject, NetworkObject, Renderer, TextMeshProUGUI, Type

### Community 144 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.27
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 147 - "RageTuningCatalog"
Cohesion: 0.22
Nodes (4): List, RageTuningCatalog, Count, ScriptableObject

### Community 148 - ".Manifest"
Cohesion: 0.27
Nodes (7): IList, ImportManifestEntry, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys

### Community 150 - ".Read"
Cohesion: 0.14
Nodes (12): Collider, Component, Renderer, StringBuilder, Transform, Story551FinalProof, Collider, List (+4 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "NetworkedAIVehicleState"
Cohesion: 0.19
Nodes (10): IReadOnlyList, Vector3, AiRageTargetResolution, Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, NetworkVariable (+2 more)

### Community 153 - "RageSandboxAutoStart"
Cohesion: 0.22
Nodes (7): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, Transform, RageSandboxAutoStart

### Community 154 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 155 - ".WithScratchScene"
Cohesion: 0.24
Nodes (7): Action, Collider, HashSet, Mesh, MeshCollider, Rigidbody, Vector3

### Community 157 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 158 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

### Community 159 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 161 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 163 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 165 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 168 - "ShapeGuardRegister.cs"
Cohesion: 0.67
Nodes (3): IReadOnlyList, ShapeGuardEntry, ShapeGuardRegister

### Community 169 - "JunctionClearanceResult"
Cohesion: 0.43
Nodes (6): JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 170 - "UserNotice"
Cohesion: 0.27
Nodes (7): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning

### Community 171 - "DocumentDto"
Cohesion: 0.67
Nodes (3): DocumentDto, BindingDto, ModelDto

### Community 186 - "NetworkedRunState"
Cohesion: 0.13
Nodes (12): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, RageRoadEventState, Confrontation (+4 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.10
Nodes (14): CharacterController, Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph (+6 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (21): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+13 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.10
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.16
Nodes (7): List, NetworkObject, Object, TearDown, Test, TestCase, Story41RageStateModuleAndDefinitionsTests

### Community 640 - "MonoBehaviour"
Cohesion: 0.13
Nodes (11): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Transform, RunCompositionRoot, RuntimeRoot (+3 more)

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
- **647 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+642 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1303 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **9 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RoadRage.Tests.EditMode` connect `RoadRage.Features.Vehicles` to `TrafficOracleTests`, `RoadRage.Shared.Domain`, `OracleEvidenceClassification`, `RoadRage.Features.UI`, `ShapeGuardRegister.cs`, `RoadRage.App.Run`, `RoadRage.Features.Players`, `.CreatePlan`?**
  _High betweenness centrality (0.189) - this node is a cross-community bridge._
- **Why does `RoadRage.Features.Vehicles` connect `RoadRage.Features.Vehicles` to `MonoBehaviour`, `Story512TireForcesAndSteeringTests`, `Story511VehicleChassisWheelsAndSuspensionTests`, `NetworkedVehicleDamageVfxController`, `VehiclePhysicsBody`, `.Read`, `Story513ArcadeAssistsAndUnevenGroundTests`, `TrafficOracleTests`, `NetworkedVehicleState.cs`, `VehicleProfile`, `RoadRage.Features.UI`, `RoadRage.App.Run`, `.Author`, `RoadRage.Shared.Domain`, `.Inspect`, `RoadRage.Features.Players`, `LaneGraph`, `.SelectWeightedSuccessor`, `LaneNode`, `Story59ParameterizedDriverModelTests`?**
  _High betweenness centrality (0.112) - this node is a cross-community bridge._
- **Why does `DefinitionId` connect `DefinitionId` to `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests`, `Story13CharacterSetupTests`, `Difficulty`, `Empty Map Entry Playmode Tests`, `NetworkedPlayerPresentation`, `RageTuningDef`, `NetworkedPassengerActionIntent`, `TrafficSettingsDef`, `.MenuResolvesProfileAndPublishesTheChosenCharacter`, `PassengerActionDef`, `RageTuningCatalog`, `VehicleProfileDef`, `NetworkedPlayerSpawnService`, `Story59ParameterizedDriverModelTests`, `CharacterCatalog`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`?**
  _High betweenness centrality (0.097) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _647 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.0935374149659864 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.13548387096774195 - nodes in this community are weakly interconnected._