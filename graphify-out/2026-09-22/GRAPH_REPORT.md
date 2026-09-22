# Graph Report - RRS  (2026-09-22)

## Corpus Check
- 203 files · ~215,323 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4042 nodes · 9978 edges · 172 communities (152 shown, 18 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 457 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `0909f896`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- GreyboxAssetSeedMetadata
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
- Story12LobbyShellTests
- Story51NpcRageFearFoundationTests
- MenuCharacterPreview
- RoadModelRecords.cs
- VehiclePhysicsBody
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- Story512TireForcesAndSteeringPlayModeTests
- Story510LaneGraphAndRoutedTrafficTests
- Story27PlayerLifecycleTests
- List
- Story513ArcadeAssistsAndUnevenGroundTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- RoadRage.Shared.Domain
- .UpdateSteeringState
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- Story513ArcadeAssistsAndUnevenGroundPlayModeTests
- NetworkedVehicleDriverController
- TrafficOracleTests
- PhysicsHarness
- FakeSteamLobbyPlatform
- VehicleProfile
- VehicleProfileDef
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- Story52BasicAiRouteFollowingAndRecoveryTests
- LobbyRosterSnapshot
- .ResolveSpawnPosition
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
- Story54RageDrivenAiBehaviorStatesTests
- .Localize
- LobbyJoinService
- NetworkedAIVehicleState
- Story525RoadWorldModelTests
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- RoadRage.Features.Vehicles
- Story55NetworkedAiRageTargetingTests
- .Author
- DefinitionId
- RoadModelSource
- Story58EscapeMenuTests
- NetworkedPlayerLifecycleService
- RoadRageBootstrap
- .NewFixture
- .Inspect
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- NetworkedRageState
- FakeSteamLobbyPlatform
- Story57AiTrafficClientPresentationTests
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- RageSandboxAutoStart
- TraceDivergenceField
- Story56RageRoadEventTriggerTests
- RoadRage.Shared.Networking
- Story33SeatEntryExitAndPassengerPresenceTests
- .TearDown
- LobbyRosterService
- .EnsureVehicleSandboxSeatHarness
- Vector3
- Story526GeometryAndLocalizationTests
- NetworkedPlayerLifecycleIntent
- PlayerProfile
- Test
- RoadModelValidationIssue
- PassengerActionVerdictCode
- RoadRageScaffoldTests
- RunEscapeMenuFlowController
- LocalVehicleCameraRig
- OnFootMovementIntent
- PassengerActionDef
- VehicleDriveIntent
- OracleEvidenceClassification
- .ComputeCollisionDamage
- Story58EscapeMenuPlayModeTests
- MainMenuProfileFlowController
- MainMenuScreen
- Story11MainMenuLaunchTests
- .Find
- RoadCurve
- Private Room Play Mode Tests
- RoadModelCanonicalWriter
- NetworkPlayerRegistry
- Netcode/Steamworks Smoke Tests
- PortalRole
- TrafficTraceFrame
- .NewTuning
- .Append
- NetworkedVehicleRecoveryIntent
- RoadRage.App.Run
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- .MainMenuSceneWiresTheSelectionSurfaceAndTheProfileFlow
- .HandleLifecycleChanged
- HostOwnedNetworkStateBehaviour
- FacepunchSteamPlatform
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- RunEscapeMenuScreen
- NetworkedPlayerState
- ShapeGuardRegister.cs
- .RequestHonk
- Story516ForceAssetRefresh
- RoadId
- RoadModelVersion
- .Run
- .NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId
- RoadModelValidationCode
- .Create
- Difficulty
- .Inspect
- .RageSandboxShowsTheSharedIncidentMarker
- .EnsureNetworkManager
- .IsSurfaceOnlyCollision
- .FixedUpdate
- AppSceneRouter.cs
- .ApplyServerDriveIntent
- TelemetrySample
- RunCompositionRoot
- NetworkPlayerConnectionPayload
- PlayerProfileBootstrapService
- .DeterministicSeededReplayProducesEqualTraceSequencesAndReachesAnExitPortal
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
- Story21OnlineServicesPlayModeTests
- Lock-Rage Camera Fix Query
- LobbyPlayerSlotView
- LaneNode
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedVehicleSeatService
- ReactionChannel
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RunFlowController` - 118 edges
2. `RoadId` - 112 edges
3. `NetworkedVehicleState` - 80 edges
4. `Story525RoadWorldModelTests` - 80 edges
5. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
6. `RoadRage.Features.Vehicles` - 75 edges
7. `RunCheckpointHudScreen` - 69 edges
8. `RoadRage.Shared.Domain` - 66 edges
9. `NetworkedVehicleDriverController` - 65 edges
10. `LobbyFlowController` - 63 edges

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

## Communities (172 total, 18 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.13
Nodes (11): PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, ArgumentException, List, Object (+3 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (11): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, ArgumentNullException, List, Object, TearDown, Test (+3 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.17
Nodes (5): GameObject, NetworkObject, Test, TextMeshProUGUI, Story25NetworkedPlayerSpawnTests

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.06
Nodes (28): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+20 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.11
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.12
Nodes (4): GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "UserNotice"
Cohesion: 0.18
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.10
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.08
Nodes (13): Vector3, TelemetrySample, Vector3, VehicleSuspensionModel, Collider, Func, GameObject, List (+5 more)

### Community 10 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

### Community 11 - "RunFlowController"
Cohesion: 0.10
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.15
Nodes (6): GameObject, StringBuilder, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.07
Nodes (26): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+18 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.05
Nodes (24): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, Key, Rpc, RpcParams, NetworkedVehicleSeatIntent (+16 more)

### Community 15 - "LobbyFlowController"
Cohesion: 0.11
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.13
Nodes (13): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Vector2, LocalOnFootController, IsDowned, MovementEnabled (+5 more)

### Community 17 - "Story12LobbyShellTests"
Cohesion: 0.11
Nodes (14): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject (+6 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "RoadModelRecords.cs"
Cohesion: 0.05
Nodes (48): ImportManifest, ImportManifestEntry, Junction, JunctionControl, JunctionControlKind, Priority, Signalized, Stop (+40 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.09
Nodes (17): Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees (+9 more)

### Community 22 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.10
Nodes (8): BoxCollider, Collider, GameObject, Object, TearDown, Test, Vector3, Story510LaneGraphAndRoutedTrafficTests

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.11
Nodes (6): CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests, IEnumerable

### Community 26 - "List"
Cohesion: 0.15
Nodes (9): Action, Bounds, List, Renderer, Scene, Transform, CurbTrafficMeasurement, ReplayTrace (+1 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.08
Nodes (15): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, Scene (+7 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (6): SessionTrafficValue, RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.Features.Rage, RoadRage.Features.PassengerActions

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.14
Nodes (8): DevIndestructibleVehicle, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.20
Nodes (7): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.16
Nodes (8): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Tests.PlayMode

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.22
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 44 - "LobbyRosterSnapshot"
Cohesion: 0.10
Nodes (23): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+15 more)

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

### Community 56 - "NetworkedRunState"
Cohesion: 0.14
Nodes (12): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, RageRoadEventState, Confrontation (+4 more)

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.09
Nodes (18): AiVehicleFixture, IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee (+10 more)

### Community 58 - ".Localize"
Cohesion: 0.16
Nodes (18): Dictionary, IReadOnlyList, List, Vector3, Query, RoadElementKind, None, RoadLocalizer (+10 more)

### Community 59 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "NetworkedAIVehicleState"
Cohesion: 0.44
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 61 - "Story525RoadWorldModelTests"
Cohesion: 0.08
Nodes (11): RoadModelCompilationException, Issues, Action, JunctionMovement, LaneCorridor, SignalPlan, Test, Vector3 (+3 more)

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "RoadRage.Features.Vehicles"
Cohesion: 0.13
Nodes (3): RoadRage.Shared.Authoring, RoadRage.Tests.EditMode, RoadRage.Features.Vehicles

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.12
Nodes (12): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, IntentFixture (+4 more)

### Community 65 - ".Author"
Cohesion: 0.28
Nodes (7): Bounds, Collider, GameObject, List, Transform, Vector3, Story513RecipeRelief

### Community 66 - "DefinitionId"
Cohesion: 0.11
Nodes (16): NetworkObject, List, CharacterCatalog, Count, Color, GameObject, CharacterDef, DisplayName (+8 more)

### Community 67 - "RoadModelSource"
Cohesion: 0.08
Nodes (21): JunctionMovement, RoadModelCanonicalPayload, Comparison, RoadModelCompiler, LaneConnection, LaneConnectionKind, Continuation, Merge (+13 more)

### Community 68 - "Story58EscapeMenuTests"
Cohesion: 0.15
Nodes (10): Canvas, EventSystem, Image, InputSystemUIInputModule, List, Object, Scene, TMP_Text (+2 more)

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
Cohesion: 0.09
Nodes (15): List, RageTuningCatalog, Count, RageTuningDef, FearSensitivity, HonkChannel, HonkMagnitude, HonkRange (+7 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 75 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "NetworkedRageState"
Cohesion: 0.11
Nodes (16): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, List, PassengerActionCatalog (+8 more)

### Community 78 - "FakeSteamLobbyPlatform"
Cohesion: 0.07
Nodes (18): ApprovalResult, Difficulty, FakeSteamLobbyPlatform, GameObject, NetworkObject, Task, Test, ApprovalResult (+10 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 81 - "RageSandboxAutoStart"
Cohesion: 0.26
Nodes (6): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, RageSandboxAutoStart

### Community 82 - "TraceDivergenceField"
Cohesion: 0.13
Nodes (17): List, TraceDivergence, TraceDivergenceField, Blockers, BrakeReverse, FrameCountMismatch, Goal, Handbrake (+9 more)

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.11
Nodes (11): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, GameObject, List, NetworkObject, Object, TearDown (+3 more)

### Community 84 - "RoadRage.Shared.Networking"
Cohesion: 0.12
Nodes (9): NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking (+1 more)

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.23
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 88 - "LobbyRosterService"
Cohesion: 0.07
Nodes (19): Difficulty, Task, ISteamLobbyPlatform, Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status (+11 more)

### Community 89 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 90 - "Vector3"
Cohesion: 0.28
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 91 - "Story526GeometryAndLocalizationTests"
Cohesion: 0.12
Nodes (7): RoadCurveSample, Action, ArgumentException, LaneCorridor, Test, Vector3, Story526GeometryAndLocalizationTests

### Community 92 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 93 - "PlayerProfile"
Cohesion: 0.17
Nodes (11): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+3 more)

### Community 95 - "RoadModelValidationIssue"
Cohesion: 0.19
Nodes (15): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+7 more)

### Community 96 - "PassengerActionVerdictCode"
Cohesion: 0.08
Nodes (28): Rpc, RpcParams, FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, PassengerActionValidation, PassengerActionValidationContext, PassengerActionVerdict (+20 more)

### Community 97 - "RoadRageScaffoldTests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 98 - "RunEscapeMenuFlowController"
Cohesion: 0.14
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.05
Nodes (35): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+27 more)

### Community 100 - "OnFootMovementIntent"
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 101 - "PassengerActionDef"
Cohesion: 0.15
Nodes (11): Transform, PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot (+3 more)

### Community 102 - "VehicleDriveIntent"
Cohesion: 0.29
Nodes (6): VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 103 - "OracleEvidenceClassification"
Cohesion: 0.18
Nodes (13): IReadOnlyList, BoundTest, OracleCatalog, OracleEvidenceClassification, AutoEdit, AutoPlay, Gap, Manual (+5 more)

### Community 105 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 106 - "MainMenuProfileFlowController"
Cohesion: 0.33
Nodes (3): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex

### Community 107 - "MainMenuScreen"
Cohesion: 0.11
Nodes (11): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+3 more)

### Community 108 - "Story11MainMenuLaunchTests"
Cohesion: 0.18
Nodes (9): Canvas, CanvasScaler, Component, EventSystem, InputSystemUIInputModule, Scene, SerializeField, Test (+1 more)

### Community 110 - "RoadCurve"
Cohesion: 0.15
Nodes (18): Bounds, Vector3, RoadCurve, FullBounds, Length, StartS, RoadCurvePoint, RoadProjection (+10 more)

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "RoadModelCanonicalWriter"
Cohesion: 0.28
Nodes (5): Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter, BinaryWriter

### Community 113 - "NetworkPlayerRegistry"
Cohesion: 0.29
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 115 - "PortalRole"
Cohesion: 0.25
Nodes (7): Portal, PortalRole, Entry, Exit, Portal, Portal, Portal

### Community 116 - "TrafficTraceFrame"
Cohesion: 0.29
Nodes (5): Vector3, List, Vector3, TrafficTrace, TrafficTraceFrame

### Community 118 - ".Append"
Cohesion: 0.23
Nodes (8): Canvas, CanvasScaler, Component, StringBuilder, TMP_Text, Transform, Vector2, Story516LobbyPanelInspection

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 120 - "RoadRage.App.Run"
Cohesion: 0.15
Nodes (4): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Shared.Input, RoadRage.Shared.Presentation

### Community 121 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 123 - ".MainMenuSceneWiresTheSelectionSurfaceAndTheProfileFlow"
Cohesion: 0.28
Nodes (4): Component, RawImage, Scene, Transform

### Community 125 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.22
Nodes (7): NetworkVariable, NetworkedBossState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority, NetworkBehaviour

### Community 126 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 127 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

### Community 128 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 129 - "NetworkedPlayerState"
Cohesion: 0.10
Nodes (16): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerLifecycle, Alive, Dead, Disconnected (+8 more)

### Community 130 - "ShapeGuardRegister.cs"
Cohesion: 0.67
Nodes (3): IReadOnlyList, ShapeGuardEntry, ShapeGuardRegister

### Community 133 - "RoadId"
Cohesion: 0.07
Nodes (35): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+27 more)

### Community 134 - "RoadModelVersion"
Cohesion: 0.32
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 135 - ".Run"
Cohesion: 0.27
Nodes (10): Button, GameObject, Object, Scene, SerializedObject, StringBuilder, TMP_Text, Transform (+2 more)

### Community 136 - ".NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId"
Cohesion: 0.33
Nodes (3): Collider, Renderer, Type

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.06
Nodes (31): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference, DuplicateId (+23 more)

### Community 138 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 139 - "Difficulty"
Cohesion: 0.20
Nodes (6): Difficulty, Difficulty, Difficulty, Easy, Hard, Normal

### Community 140 - ".Inspect"
Cohesion: 0.38
Nodes (4): Collider, Component, Transform, Story513RageTargetInspection

### Community 141 - ".RageSandboxShowsTheSharedIncidentMarker"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story44PassengerActionTwoMvpRunPlayModeTests

### Community 142 - ".EnsureNetworkManager"
Cohesion: 0.24
Nodes (7): GameObject, NetworkManager, NetworkPrefabsList, NetworkManager, NetworkPrefabsList, TearDown, FacepunchTransport

### Community 144 - ".FixedUpdate"
Cohesion: 0.10
Nodes (17): WheelState, Compression, ContactPoint, Grounded, HubPosition, TireSample, Vector2, TireSample (+9 more)

### Community 145 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 147 - "TelemetrySample"
Cohesion: 0.33
Nodes (6): TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees, Speed

### Community 148 - "RunCompositionRoot"
Cohesion: 0.40
Nodes (4): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot

### Community 150 - "PlayerProfileBootstrapService"
Cohesion: 0.27
Nodes (7): PlayerProfileBootstrapService, PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist, PlayerProfileResolution

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
Cohesion: 0.11
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

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
Cohesion: 0.14
Nodes (10): RageThreshold, Disposition, MinValue, List, NetworkObject, Object, TearDown, Test (+2 more)

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.17
Nodes (7): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Quaternion, Vector3

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

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
Cohesion: 0.12
Nodes (10): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkedPlayerSpawnService, Vector3, NetworkedVehicleSeatService (+2 more)

### Community 858 - "ReactionChannel"
Cohesion: 0.14
Nodes (8): TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage, ContextMenu

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
- **530 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+525 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1040 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **18 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RoadRage.Tests.EditMode` connect `RoadRage.Features.Vehicles` to `ShapeGuardRegister.cs`, `RoadModelSource`, `OracleEvidenceClassification`, `RoadRage.Features.UI`, `RoadRage.Features.Players`, `TraceDivergenceField`, `RoadRage.Shared.Networking`, `TrafficTraceFrame`, `RoadRage.App.Run`, `RoadRage.Shared.Domain`?**
  _High betweenness centrality (0.146) - this node is a cross-community bridge._
- **Why does `DefinitionId` connect `DefinitionId` to `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests`, `PassengerActionVerdictCode`, `Story13CharacterSetupTests`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `PassengerActionDef`, `Empty Map Entry Playmode Tests`, `VehicleProfileDef`, `RageTuningDef`, `NetworkedRageState`, `TrafficSettingsDef`, `.MenuResolvesProfileAndPublishesTheChosenCharacter`, `PlayerProfileBootstrapService`, `Story59ParameterizedDriverModelTests`, `PlayerProfile`?**
  _High betweenness centrality (0.136) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `PassengerActionVerdictCode`, `RoadModelSource`, `RoadCurve`, `RoadModelCanonicalWriter`, `PortalRole`, `RoadModelRecords.cs`, `.Localize`, `Story526GeometryAndLocalizationTests`, `Story525RoadWorldModelTests`, `RoadModelValidationIssue`?**
  _High betweenness centrality (0.107) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _530 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.12857142857142856 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.08941176470588236 - nodes in this community are weakly interconnected._
- **Should `GreyboxAssetSeedMetadata` be split into smaller, more focused modules?**
  _Cohesion score 0.06233766233766234 - nodes in this community are weakly interconnected._