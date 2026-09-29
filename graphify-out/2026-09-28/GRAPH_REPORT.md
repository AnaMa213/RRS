# Graph Report - RRS  (2026-09-28)

## Corpus Check
- 227 files · ~321,964 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 5315 nodes · 14191 edges · 182 communities (174 shown, 7 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 592 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `e1e9996c`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- Story14GreyboxAssetSeedTests
- Story516LobbyConfigurableTrafficSettingsTests
- Story512TireForcesAndSteeringTests
- NetworkedVehicleState
- Story527MigrationTests
- TireSample
- Story511VehicleChassisWheelsAndSuspensionTests
- Story16Epic1PlayableCheckpointTests
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
- JunctionClearance
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
- LobbyShellScreen
- LobbyJoinOutcome
- Story26InGameHudTests
- Story52BasicAiRouteFollowingAndRecoveryTests
- LobbyRosterSnapshot
- RoadRage.Features.UI
- FakeSteamLobbyPlatform
- TrafficSettingsDef
- Story514AiDrivesByIntentTests
- PassengerActionDef
- NetworkedPlayerLifecycleService
- RoadId
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- .CheckVisuals
- Story43PassengerActionOneChangesRageTests
- ImportContext
- CharacterDef
- NetworkedRageState
- .Localize
- LobbyJoinService
- Story528AuthoringAndGateATests
- Story525RoadWorldModelTests
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- LobbyFlowController
- NetworkedAIVehicleState
- CharacterCatalog
- List
- LobbyRosterService
- RoadRage.Shared.Domain
- PairReviewEntry
- .Capture
- .NewFixture
- .FullPath
- RageTuningDef
- Story15EmptyMapEntryTests
- PassengerActionVerdictCode
- NetworkedRunSessionMonitor
- .Plan
- LobbyRoomService
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
- RunEscapeMenuScreen
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
- Story32DriverControlAndLocalCameraTests
- OracleEvidenceClassification
- Story28Epic2OnlinePlayableCheckpointTests
- RoadCurveSample
- LaneNode
- MainMenuScreen
- Story512TireForcesAndSteeringPlayModeTests
- RoadCurve
- Story22HostCreatedPrivateRoomPlayModeTests
- Story58EscapeMenuTests
- SweepPose
- .SelectWeightedSuccessor
- NetworkedPlayerLifecycleIntent
- NetworkPlayerRegistry
- ReactionChannel
- DrivabilityProfile
- VehicleProfileDef
- NetworkedPlayerSpawnService
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- VehicleDriveIntent
- .NewTuning
- NetworkedPlayerState
- RageTuningCatalog
- FakeSteamLobbyPlatform
- NetworkedVehicleSeatService
- MonoBehaviour
- RoadRageBootstrap
- NetworkedVehicleRecoveryIntent
- Story12LobbyShellPlayModeTests
- UserNotice
- RoadModelSource
- Story58EscapeMenuPlayModeTests
- TrafficTraceFrame
- OnFootMovementIntent
- RoadModelValidationCode
- MovementRole
- .NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId
- Story510LaneGraphAndRoutedTrafficTests
- PlayerProfileResolution
- .BuildSmoothCurve
- .DeterministicSeededReplayProducesEqualTraceSequencesAndReachesAnExitPortal
- RoadModelCanonicalWriter
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- .EnsureVehicleSandboxSeatHarness
- .Manifest
- .MeasureFresh
- NetworkedVehicleSeatIntent
- .MeasurePath
- Story551JunctionClearanceTests
- StatusFilter
- GreyboxAssetSeedMetadata
- AppSceneRouter.cs
- RoadRage.Shared.Networking
- TestSuiteCategoryPartitionTests
- .Measure
- Story11MainMenuLaunchTests
- .Create
- FakeSteamPlatform
- RunCompositionRoot
- RoadRage.App.Run
- SeedExpectation
- .TearDown
- NetworkedRunState
- .Degrees
- NetworkedPlayerPresentation
- ShapeGuardRegister.cs
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
1. `RoadId` - 132 edges
2. `RunFlowController` - 118 edges
3. `Story526GeometryAndLocalizationTests` - 89 edges
4. `Story525RoadWorldModelTests` - 81 edges
5. `NetworkedVehicleState` - 79 edges
6. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
7. `RoadRage.Features.Vehicles` - 76 edges
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

## Communities (182 total, 7 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.10
Nodes (15): PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, ArgumentException, List (+7 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.14
Nodes (8): GameObject, NetworkManager, NetworkObject, NetworkPrefabsList, TearDown, Test, TextMeshProUGUI, Story25NetworkedPlayerSpawnTests

### Community 3 - "Story14GreyboxAssetSeedTests"
Cohesion: 0.17
Nodes (10): Bounds, Collider, GameObject, MeshCollider, NetworkObject, Renderer, Test, Transform (+2 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.10
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.08
Nodes (8): VehicleSteeringModel, TireSample, Vector2, VehicleTireModel, GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 7 - "Story527MigrationTests"
Cohesion: 0.14
Nodes (6): Action, GameObject, Scene, Test, TestCase, Story527MigrationTests

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.06
Nodes (24): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+16 more)

### Community 10 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.22
Nodes (6): Component, GameObject, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (12): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+4 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.13
Nodes (6): GameObject, StringBuilder, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.06
Nodes (27): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+19 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (20): Func, Vector3, NetworkedPassengerActionIntent, Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject (+12 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.12
Nodes (15): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+7 more)

### Community 17 - "Story12LobbyShellTests"
Cohesion: 0.11
Nodes (14): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject (+6 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.16
Nodes (12): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, Component (+4 more)

### Community 20 - "MigrationReport"
Cohesion: 0.09
Nodes (28): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+20 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.10
Nodes (16): Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees (+8 more)

### Community 22 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 23 - "JunctionClearance"
Cohesion: 0.17
Nodes (11): Collider, IReadOnlyList, List, Scene, Transform, Vector2, Vector3, JunctionClearance (+3 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (33): Bounds, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IReadOnlyList, List, Predicate (+25 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.10
Nodes (6): IEnumerable, CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests

### Community 26 - ".CreatePlan"
Cohesion: 0.08
Nodes (25): AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, ConflictDecision, Dictionary, HashSet (+17 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.08
Nodes (13): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, MeshFilter (+5 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.13
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "Story550CompatibilityGolden"
Cohesion: 0.09
Nodes (19): RoadModelVersion, High, IsEmpty, Low, SchemaVersion, LaneCorridor, Portal, Test (+11 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.07
Nodes (33): Func, IList, JunctionControlKind, List, RoadCurveSample, RoadId, AuthoringDecisions, ConflictRecord (+25 more)

### Community 32 - ".Parse"
Cohesion: 0.10
Nodes (13): FileLayout, ConflictDecisionKind, Accepted, Rejected, PairReviewActions, Action, FormatException, IEnumerable (+5 more)

### Community 33 - "TraceDivergenceField"
Cohesion: 0.13
Nodes (17): List, TraceDivergence, TraceDivergenceField, Blockers, BrakeReverse, FrameCountMismatch, Goal, Handbrake (+9 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.08
Nodes (12): Action, Collider, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc, RpcParams (+4 more)

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "RoadModelDocument"
Cohesion: 0.05
Nodes (46): AdjacencyDto, DrivabilityProfile, Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto (+38 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.04
Nodes (45): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+37 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.09
Nodes (22): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+14 more)

### Community 40 - "LobbyShellScreen"
Cohesion: 0.22
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

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

### Community 45 - "RoadRage.Features.UI"
Cohesion: 0.22
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
Cohesion: 0.08
Nodes (23): List, PassengerActionCatalog, Count, Version, PassengerActionDef, CooldownSeconds, DisplayName, Id (+15 more)

### Community 50 - "NetworkedPlayerLifecycleService"
Cohesion: 0.17
Nodes (7): NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 51 - "RoadId"
Cohesion: 0.07
Nodes (34): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+26 more)

### Community 52 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 53 - ".CheckVisuals"
Cohesion: 0.15
Nodes (11): Surface, Component, HashSet, IEnumerable, Mesh, MeshFilter, MeshRenderer, Renderer (+3 more)

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "ImportContext"
Cohesion: 0.11
Nodes (27): Dictionary, JunctionFeature, List, Predicate, RoadBoundsBox, RoadCurve, RoadCurveSample, ImportContext (+19 more)

### Community 56 - "CharacterDef"
Cohesion: 0.20
Nodes (8): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId

### Community 57 - "NetworkedRageState"
Cohesion: 0.07
Nodes (24): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, RageDisposition, Block (+16 more)

### Community 58 - ".Localize"
Cohesion: 0.15
Nodes (18): Dictionary, IReadOnlyList, List, Vector3, Query, RoadElementKind, None, RoadLocalizer (+10 more)

### Community 59 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "Story528AuthoringAndGateATests"
Cohesion: 0.09
Nodes (17): SignoffLayout, Action, BoxCollider, Collider, FormatException, GameObject, IReadOnlyList, KeyValuePair (+9 more)

### Community 61 - "Story525RoadWorldModelTests"
Cohesion: 0.07
Nodes (10): Action, FormatException, JunctionMovement, LaneCorridor, Portal, SignalPlan, Test, Vector3 (+2 more)

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "LobbyFlowController"
Cohesion: 0.14
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 64 - "NetworkedAIVehicleState"
Cohesion: 0.10
Nodes (17): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, GameObject, List, NetworkObject (+9 more)

### Community 65 - "CharacterCatalog"
Cohesion: 0.13
Nodes (11): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Component, IEnumerator (+3 more)

### Community 66 - "List"
Cohesion: 0.14
Nodes (9): Action, Bounds, List, Renderer, Scene, Transform, CurbTrafficMeasurement, ReplayTrace (+1 more)

### Community 67 - "LobbyRosterService"
Cohesion: 0.10
Nodes (9): Difficulty, Task, ISteamLobbyPlatform, Difficulty, LobbyRosterService, AllMembersReady, Current, IsSynchronized (+1 more)

### Community 68 - "RoadRage.Shared.Domain"
Cohesion: 0.10
Nodes (4): SessionTrafficValue, RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Features.Run

### Community 69 - "PairReviewEntry"
Cohesion: 0.05
Nodes (46): CompiledRoadModel, Dictionary, DrivabilityProfile, IReadOnlyList, List, RoadBoundsBox, RoadCurveSample, RoadId (+38 more)

### Community 70 - ".Capture"
Cohesion: 0.20
Nodes (9): ArgumentOutOfRangeException, BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, Vector3, HistoricalPairFingerprintRecord, HistoricalPairFingerprintTable (+1 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 72 - ".FullPath"
Cohesion: 0.17
Nodes (9): Action, IList, KeyValuePair, MenuItem, Scene, MenuItem, Func, Category (+1 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): TMP_Text, RageStateDebugView, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity, HonkChannel (+10 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 75 - "PassengerActionVerdictCode"
Cohesion: 0.08
Nodes (26): Rpc, RpcParams, FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, PassengerActionVerdict, Accepted, PassengerActionVerdictCode (+18 more)

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - ".Plan"
Cohesion: 0.06
Nodes (44): IReadOnlyList, RoadElementKind, RoadId, RoadModelVersion, RouteDiagnostic, None, ZeroWeightFallback, RouteOccurrence (+36 more)

### Community 78 - "LobbyRoomService"
Cohesion: 0.06
Nodes (27): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+19 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 81 - "V1ImportResult"
Cohesion: 0.07
Nodes (29): RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift (+21 more)

### Community 82 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.20
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.09
Nodes (17): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, RageRoadEventState, Confrontation, Idle, Resolved, RewardGranted (+9 more)

### Community 84 - "RoadRageScaffoldTests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 86 - "RoadRage.Features.Players"
Cohesion: 0.12
Nodes (4): RoadRage.Features.Players, RoadRage.Shared.Authoring, RoadRage.Features.PassengerActions, RoadRage.Shared.Definitions

### Community 87 - "Story36Epic3DrivingPlayableCheckpointTests"
Cohesion: 0.19
Nodes (8): AssemblyDefinition, Component, GameObject, NetworkObject, Scene, Test, AssemblyDefinition, Story36Epic3DrivingPlayableCheckpointTests

### Community 89 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 90 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

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
Cohesion: 0.13
Nodes (17): Func, GameObject, IReadOnlyList, LaneGraph, LaneNode, List, Scene, Vector3 (+9 more)

### Community 95 - ".Validate"
Cohesion: 0.15
Nodes (18): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+10 more)

### Community 96 - "RoadRage.Features.Vehicles"
Cohesion: 0.09
Nodes (5): RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Tests.EditMode, RoadRage.Features.Rage, RoadRage.Features.Vehicles, RoadRage.Features.Vehicles.Traffic

### Community 97 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (15): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+7 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.08
Nodes (24): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+16 more)

### Community 100 - "Vector3"
Cohesion: 0.28
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "Difficulty"
Cohesion: 0.18
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 102 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.15
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 103 - "OracleEvidenceClassification"
Cohesion: 0.18
Nodes (13): IReadOnlyList, BoundTest, OracleCatalog, OracleEvidenceClassification, AutoEdit, AutoPlay, Gap, Manual (+5 more)

### Community 104 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.17
Nodes (10): ApprovalResult, NetworkPlayerConnectionPayload, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+2 more)

### Community 105 - "RoadCurveSample"
Cohesion: 0.29
Nodes (5): JunctionMovement, RoadCurveSample, JunctionMovement, Test, Story529RoutePlanTests

### Community 106 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.10
Nodes (11): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+3 more)

### Community 108 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 110 - "RoadCurve"
Cohesion: 0.11
Nodes (18): Bounds, Vector3, RoadCurve, FullBounds, Length, StartS, RoadCurvePoint, RoadProjection (+10 more)

### Community 111 - "Story22HostCreatedPrivateRoomPlayModeTests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 113 - "SweepPose"
Cohesion: 0.07
Nodes (41): CompiledRoadModel, Dictionary, IList, IReadOnlyList, List, RoadBoundsBox, RoadCurve, RoadCurveSample (+33 more)

### Community 114 - ".SelectWeightedSuccessor"
Cohesion: 0.22
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 115 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 116 - "NetworkPlayerRegistry"
Cohesion: 0.29
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 117 - "ReactionChannel"
Cohesion: 0.29
Nodes (5): ReactionChannel, Both, Fear, None, Rage

### Community 118 - "DrivabilityProfile"
Cohesion: 0.36
Nodes (3): DrivabilityProfile, Comparison, RoadModelCompiler

### Community 119 - "VehicleProfileDef"
Cohesion: 0.17
Nodes (9): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty, SerializedObject (+1 more)

### Community 120 - "NetworkedPlayerSpawnService"
Cohesion: 0.18
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

### Community 121 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 122 - "VehicleDriveIntent"
Cohesion: 0.29
Nodes (6): VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 124 - "NetworkedPlayerState"
Cohesion: 0.11
Nodes (17): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState (+9 more)

### Community 125 - "RageTuningCatalog"
Cohesion: 0.22
Nodes (4): List, RageTuningCatalog, Count, ScriptableObject

### Community 126 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 127 - "NetworkedVehicleSeatService"
Cohesion: 0.11
Nodes (11): Vector3, NetworkedVehicleSeatService, Instance, Vector3, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 128 - "MonoBehaviour"
Cohesion: 0.20
Nodes (6): Color, Image, TMP_Text, LobbyPlayerSlotView, DevIndestructibleVehicle, MonoBehaviour

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (22): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+14 more)

### Community 130 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 131 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 132 - "UserNotice"
Cohesion: 0.27
Nodes (7): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning

### Community 133 - "RoadModelSource"
Cohesion: 0.04
Nodes (72): EffectiveLaneCorridor, JunctionMovement, RoadModelCanonicalPayload, DrivabilityProfile, ImportManifest, ImportManifestEntry, Junction, JunctionControl (+64 more)

### Community 134 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 135 - "TrafficTraceFrame"
Cohesion: 0.29
Nodes (5): Vector3, List, Vector3, TrafficTrace, TrafficTraceFrame

### Community 136 - "OnFootMovementIntent"
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (44): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference, DrivabilityAdmissionFailed (+36 more)

### Community 138 - "MovementRole"
Cohesion: 0.33
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 139 - ".NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId"
Cohesion: 0.33
Nodes (3): Collider, Renderer, Type

### Community 140 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.12
Nodes (8): BoxCollider, Collider, GameObject, Object, TearDown, Test, Vector3, Story510LaneGraphAndRoutedTrafficTests

### Community 141 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 142 - ".BuildSmoothCurve"
Cohesion: 0.29
Nodes (3): RoadCurvePoint, Vector3, CircleFit

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.25
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.22
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 147 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 148 - ".Manifest"
Cohesion: 0.27
Nodes (7): IList, ImportManifestEntry, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys

### Community 150 - ".MeasureFresh"
Cohesion: 0.14
Nodes (14): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, BoxCollider, IReadOnlyList (+6 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 155 - "Story551JunctionClearanceTests"
Cohesion: 0.15
Nodes (11): Action, ArgumentException, Collider, HashSet, Mesh, MeshCollider, Rigidbody, Test (+3 more)

### Community 157 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 159 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 161 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 165 - "RoadRage.Shared.Networking"
Cohesion: 0.14
Nodes (7): VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking, RoadRage.Features.Boss

### Community 168 - "TestSuiteCategoryPartitionTests"
Cohesion: 0.36
Nodes (4): List, Test, TestSuiteCategoryPartitionTests, MethodInfo

### Community 169 - ".Measure"
Cohesion: 0.16
Nodes (14): BoxCollider, CompiledRoadModel, GameObject, VehiclePhysicsBody, VehicleProfile, JunctionClearanceRelief, JunctionClearanceResult, Passed (+6 more)

### Community 170 - "Story11MainMenuLaunchTests"
Cohesion: 0.13
Nodes (11): UserNoticeChannel, LastNotice, Canvas, Component, EventSystem, InputSystemUIInputModule, Scene, SerializeField (+3 more)

### Community 172 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 175 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 176 - "RunCompositionRoot"
Cohesion: 0.40
Nodes (4): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot

### Community 177 - "RoadRage.App.Run"
Cohesion: 0.23
Nodes (3): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Shared.Input

### Community 178 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 186 - "NetworkedRunState"
Cohesion: 0.12
Nodes (11): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, Camera, TMP_Text (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 203 - "ShapeGuardRegister.cs"
Cohesion: 0.67
Nodes (3): IReadOnlyList, ShapeGuardEntry, ShapeGuardRegister

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
- **669 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+664 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1310 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RoadRage.Tests.EditMode` connect `RoadRage.Features.Vehicles` to `TraceDivergenceField`, `RoadRage.Shared.Domain`, `RoadRage.Shared.Networking`, `OracleEvidenceClassification`, `TestSuiteCategoryPartitionTests`, `TrafficTraceFrame`, `ShapeGuardRegister.cs`, `OnlineServicesBootstrapService`, `RoadRage.Features.UI`, `RoadRage.App.Run`, `RoadRage.Features.Players`?**
  _High betweenness centrality (0.185) - this node is a cross-community bridge._
- **Why does `RoadRage.Features.Vehicles` connect `RoadRage.Features.Vehicles` to `MonoBehaviour`, `Story512TireForcesAndSteeringTests`, `TrafficTraceFrame`, `Story511VehicleChassisWheelsAndSuspensionTests`, `NetworkedVehicleDamageVfxController`, `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, `VehiclePhysicsBody`, `Story513ArcadeAssistsAndUnevenGroundTests`, `RoadRage.Shared.Networking`, `VehicleProfile`, `RoadRage.Features.UI`, `RoadRage.App.Run`, `RoadRage.Shared.Domain`, `RoadRage.Features.Players`, `LaneGraph`, `LaneNode`, `.SelectWeightedSuccessor`, `Story59ParameterizedDriverModelTests`, `VehicleDriveIntent`?**
  _High betweenness centrality (0.126) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `.CreatePlan`, `RoadModelSource`, `RoadModelDocument`, `Story527MigrationTests`, `.MeasurePath`, `RoadCurveSample`, `PassengerActionVerdictCode`, `.Plan`, `RoadCurve`, `RoadModelCanonicalWriter`, `SweepPose`, `.Localize`, `Story526GeometryAndLocalizationTests`, `Story525RoadWorldModelTests`, `Story550CompatibilityGolden`, `.Validate`?**
  _High betweenness centrality (0.105) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _669 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10144927536231885 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.13911290322580644 - nodes in this community are weakly interconnected._