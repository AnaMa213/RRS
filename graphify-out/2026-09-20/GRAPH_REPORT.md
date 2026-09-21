# Graph Report - RRS  (2026-09-20)

## Corpus Check
- 188 files · ~197,653 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3708 nodes · 8905 edges · 164 communities (150 shown, 13 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 446 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `210f4881`
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
- .ReplayRoute
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- FakeSteamLobbyPlatform
- RoadRage.Shared.Domain
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- LobbyFlowController
- LocalOnFootController
- UserNotice
- Story51NpcRageFearFoundationTests
- MenuCharacterPreview
- NetworkedVehicleDamageVfxController
- VehiclePhysicsBody
- DriverProfile
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
- Story518IntersectionRulesAndDeadlockPreventionTests
- VehicleProfile
- VehicleProfileDef
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- RunEscapeMenuFlowController
- LobbyCreateOutcome
- NetworkedRunState
- FakeSteamLobbyPlatform
- TrafficSettingsDef
- Story514AiDrivesByIntentTests
- TrafficPerceptionCandidate
- RoadRage.Features.Players
- .SelectWeightedSuccessor
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- VehicleWheel
- Story43PassengerActionOneChangesRageTests
- LobbyShellScreen
- TrafficSignalPlan
- NetworkedRageState
- PortalTrafficSpawner
- LobbyJoinService
- NetworkedAIVehicleState
- RunEscapeMenuScreen
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- Story22HostCreatedPrivateRoomTests
- Story55NetworkedAiRageTargetingTests
- Story28Epic2OnlinePlayableCheckpointTests
- MainMenuProfileFlowController
- RunFlowController
- LobbyRosterSnapshot
- Story58EscapeMenuTests
- RoadRageBootstrap
- .NewFixture
- JunctionClaim
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- DefinitionId
- LobbyRoomService
- Story57AiTrafficClientPresentationTests
- NetworkedPlayerLifecycleService
- RoadRage.Shared.Networking
- NetworkPlayerRegistry
- Story56RageRoadEventTriggerTests
- RoadRage.App.Run
- Story33SeatEntryExitAndPassengerPresenceTests
- .TearDown
- .RequestHonk
- .NewTuning
- .EnsureVehicleSandboxSeatHarness
- .TrySpawnSelectedProfile
- DriverModel
- .TrafficLeavesItsEntryTunnelUnderItsOwnPhysics
- PlayerProfile
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- TrafficSignalLampView
- NetworkedPlayerState
- RoadRage Scaffold Structure Tests
- NetworkedVehicleSeatIntent
- LocalVehicleCameraRig
- .AssertDecorationIsVisualOnly
- PassengerActionIntent
- MonoBehaviour
- .Resolve
- .FixedUpdate
- .UpdateSteeringState
- FakeSteamLobbyPlatform
- MainMenuScreen
- DriverProfileDef
- .Create
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- Private Room Play Mode Tests
- TrafficUnblockingAction
- .IsSurfaceOnlyCollision
- Netcode/Steamworks Smoke Tests
- VehicleDriveIntent
- NetworkedPlayerLifecycleIntent
- .RageSandboxShowsTheSharedIncidentMarker
- .NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId
- NetworkedVehicleRecoveryIntent
- FakeSteamPlatform
- ISteamPlatform
- NetworkedPassengerActionIntent
- Story58EscapeMenuPlayModeTests
- .HandleLifecycleChanged
- FakeSteamPlatform
- HostOwnedNetworkStateBehaviour
- RageTuningCatalog
- FacepunchSteamPlatform
- OnlineServicesStatus
- JunctionApproachRule
- PlayerLifecycle
- FakeSteamPlatform
- PlayerProfileResolution
- .ComputeCollisionDamage
- .FindTransformInScene
- Story16Epic1PlayableCheckpointTests.cs
- Difficulty
- .EnsureNetworkManager
- Story52BasicAiRouteFollowingAndRecoveryTests
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- PassengerActionDef
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- LocalVoidRespawnController
- RageSandboxAutoStart
- Story21OnlineServicesPlayModeTests
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
2. `NetworkedAIVehicleDriverController` - 109 edges
3. `NetworkedVehicleState` - 79 edges
4. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
5. `RoadRage.Features.Vehicles` - 77 edges
6. `LaneGraph` - 70 edges
7. `RunCheckpointHudScreen` - 69 edges
8. `RoadRage.Shared.Domain` - 68 edges
9. `NetworkedVehicleDriverController` - 65 edges
10. `DriverProfile` - 63 edges

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

## Communities (164 total, 13 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.10
Nodes (15): ArgumentException, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component (+7 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.14
Nodes (8): GameObject, NetworkManager, NetworkObject, NetworkPrefabsList, TearDown, Test, TextMeshProUGUI, Story25NetworkedPlayerSpawnTests

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.06
Nodes (28): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+20 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.10
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.13
Nodes (4): GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.18
Nodes (9): Canvas, Component, EventSystem, InputSystemUIInputModule, Scene, SerializeField, Test, Story11MainMenuLaunchTests (+1 more)

### Community 7 - ".ReplayRoute"
Cohesion: 0.25
Nodes (6): Bounds, List, CurbTrafficMeasurement, ReplayTrace, CurbTrafficMeasurement, ReplayTrace

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.11
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

### Community 11 - "RoadRage.Shared.Domain"
Cohesion: 0.10
Nodes (4): SessionTrafficValue, RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Features.Run

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.14
Nodes (6): GameObject, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform, StringBuilder

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.22
Nodes (5): OnlineServicesBootstrapService, Status, ArgumentNullException, Test, Story21OnlineServicesBootstrapTests

### Community 14 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 15 - "LobbyFlowController"
Cohesion: 0.12
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.10
Nodes (19): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Vector2, LocalOnFootController, IsDowned, MovementEnabled (+11 more)

### Community 17 - "UserNotice"
Cohesion: 0.16
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, MaterialPropertyBlock, RawImage, Renderer, Transform, MenuCharacterPreview (+2 more)

### Community 20 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.07
Nodes (19): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, Color, Quaternion, Renderer, Transform (+11 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.10
Nodes (15): Vector3, VehiclePhysicsTelemetryView, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor (+7 more)

### Community 22 - "DriverProfile"
Cohesion: 0.06
Nodes (35): DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, HornDelay, JunctionAcceptedGap, JunctionApproachRadius (+27 more)

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.08
Nodes (12): Action, BoxCollider, Collider, GameObject, Object, Renderer, Scene, TearDown (+4 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.10
Nodes (6): CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests, IEnumerable

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.11
Nodes (14): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject (+6 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.08
Nodes (15): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, MeshFilter (+7 more)

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
Cohesion: 0.09
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.13
Nodes (8): DevIndestructibleVehicle, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "Story518IntersectionRulesAndDeadlockPreventionTests"
Cohesion: 0.14
Nodes (6): Action, Scene, Test, Transform, Vector3, Story518IntersectionRulesAndDeadlockPreventionTests

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.19
Nodes (7): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.21
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "RunEscapeMenuFlowController"
Cohesion: 0.14
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 45 - "NetworkedRunState"
Cohesion: 0.14
Nodes (12): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, RageRoadEventState, Confrontation (+4 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (14): IReadOnlyList, TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers (+6 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.20
Nodes (5): Func, GameObject, Rigidbody, Test, Story514AiDrivesByIntentTests

### Community 49 - "TrafficPerceptionCandidate"
Cohesion: 0.07
Nodes (27): IReadOnlyList, Quaternion, Vector3, TrafficPerception, TrafficPerceptionCandidate, AngleDegrees, Distance, Extents (+19 more)

### Community 50 - "RoadRage.Features.Players"
Cohesion: 0.13
Nodes (3): RoadRage.Features.Players, RoadRage.Features.PassengerActions, RoadRage.Shared.Definitions

### Community 51 - ".SelectWeightedSuccessor"
Cohesion: 0.16
Nodes (4): IReadOnlyList, Vector3, LaneGraphRouting, IReadOnlyList

### Community 52 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 53 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "LobbyShellScreen"
Cohesion: 0.16
Nodes (9): Button, TMP_InputField, TMP_Text, LobbyShellScreen, Component, IEnumerator, UnityTearDown, UnityTest (+1 more)

### Community 56 - "TrafficSignalPlan"
Cohesion: 0.16
Nodes (12): IReadOnlyList, JunctionVerdict, Proceed, Wait, TrafficSignalPhase, DurationSeconds, GreenGroups, TrafficSignalPlan (+4 more)

### Community 57 - "NetworkedRageState"
Cohesion: 0.08
Nodes (21): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, RageDisposition, Block (+13 more)

### Community 58 - "PortalTrafficSpawner"
Cohesion: 0.15
Nodes (8): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation

### Community 59 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "NetworkedAIVehicleState"
Cohesion: 0.41
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 61 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.20
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "Story22HostCreatedPrivateRoomTests"
Cohesion: 0.31
Nodes (5): ArgumentNullException, FakeSteamLobbyPlatform, Task, Test, Story22HostCreatedPrivateRoomTests

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.12
Nodes (12): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, IntentFixture (+4 more)

### Community 65 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.15
Nodes (11): ApprovalResult, NetworkPlayerConnectionPayload, FakeSteamLobbyPlatform, GameObject, NetworkObject, Test, ApprovalResult, Approved (+3 more)

### Community 66 - "MainMenuProfileFlowController"
Cohesion: 0.33
Nodes (3): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex

### Community 67 - "RunFlowController"
Cohesion: 0.09
Nodes (4): Camera, HashSet, RunFlowController, ActiveLocalPlayer

### Community 68 - "LobbyRosterSnapshot"
Cohesion: 0.06
Nodes (20): Difficulty, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId (+12 more)

### Community 69 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.08
Nodes (17): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+9 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 72 - "JunctionClaim"
Cohesion: 0.09
Nodes (18): Vector3, JunctionClaim, ApproachForward, Breach, DistanceToJunction, Eligibility, EntryPoint, ExitPoint (+10 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): TMP_Text, RageStateDebugView, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity, HonkChannel (+10 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.17
Nodes (8): AssemblyDefinition, GameObject, Object, Scene, Test, Transform, AssemblyDefinition, Story15EmptyMapEntryTests

### Community 75 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "DefinitionId"
Cohesion: 0.11
Nodes (14): List, CharacterCatalog, Count, Color, GameObject, CharacterDef, DisplayName, Id (+6 more)

### Community 78 - "LobbyRoomService"
Cohesion: 0.10
Nodes (13): Task, ISteamLobbyPlatform, Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus (+5 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 81 - "RoadRage.Shared.Networking"
Cohesion: 0.15
Nodes (7): VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking, RoadRage.Features.Boss

### Community 82 - "NetworkPlayerRegistry"
Cohesion: 0.26
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.11
Nodes (11): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, GameObject, List, NetworkObject, Object, TearDown (+3 more)

### Community 84 - "RoadRage.App.Run"
Cohesion: 0.16
Nodes (3): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Shared.Input

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.23
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 89 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 90 - ".TrySpawnSelectedProfile"
Cohesion: 0.15
Nodes (5): Collider, GameObject, Quaternion, Transform, Vector3

### Community 92 - ".TrafficLeavesItsEntryTunnelUnderItsOwnPhysics"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story517WiderPerceptionAndProgressiveUnblockingPlayModeTests

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 95 - "TrafficSignalLampView"
Cohesion: 0.33
Nodes (4): Color, MaterialPropertyBlock, Renderer, TrafficSignalLampView

### Community 96 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 98 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.05
Nodes (35): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+27 more)

### Community 100 - ".AssertDecorationIsVisualOnly"
Cohesion: 0.29
Nodes (5): Collider, Component, GameObject, MeshFilter, MeshRenderer

### Community 101 - "PassengerActionIntent"
Cohesion: 0.22
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 102 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 104 - ".FixedUpdate"
Cohesion: 0.11
Nodes (13): RaycastHit, TireSample, Vector2, TireSample, Adherence, ForceMagnitude, GripUsage, Grounded (+5 more)

### Community 106 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (8): Difficulty, Exception, FakeSteamLobbyPlatform, CreateLobbyCallCount, LastMaxMembers, LeaveCallCount, NextOutcome, ThrowOnCreate

### Community 107 - "MainMenuScreen"
Cohesion: 0.11
Nodes (11): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+3 more)

### Community 108 - "DriverProfileDef"
Cohesion: 0.31
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 109 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 110 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "TrafficUnblockingAction"
Cohesion: 0.25
Nodes (7): TrafficUnblockingAction, FollowRoute, IntersectionBreach, Replan, Reverse, RoadDetour, SidewalkDetour

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.24
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 115 - "VehicleDriveIntent"
Cohesion: 0.15
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 116 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 117 - ".RageSandboxShowsTheSharedIncidentMarker"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story44PassengerActionTwoMvpRunPlayModeTests

### Community 118 - ".NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId"
Cohesion: 0.33
Nodes (3): Collider, Renderer, Type

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 120 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 121 - "ISteamPlatform"
Cohesion: 0.14
Nodes (6): ISteamPlatform, IsLoggedOn, IsValid, FakeSteamPlatform, IsLoggedOn, IsValid

### Community 122 - "NetworkedPassengerActionIntent"
Cohesion: 0.12
Nodes (13): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, List, PassengerActionCatalog (+5 more)

### Community 123 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 125 - "FakeSteamPlatform"
Cohesion: 0.17
Nodes (9): Exception, FakeSteamPlatform, InitCallCount, InitCalledWithAppId, IsLoggedOn, IsValid, RunCallbacksCallCount, ShutdownCallCount (+1 more)

### Community 126 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.18
Nodes (9): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority (+1 more)

### Community 127 - "RageTuningCatalog"
Cohesion: 0.22
Nodes (4): List, RageTuningCatalog, Count, ScriptableObject

### Community 128 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 129 - "OnlineServicesStatus"
Cohesion: 0.25
Nodes (6): OnlineServicesStatus, InitializationFailed, NotStarted, Offline, Online, SignInFailed

### Community 131 - "JunctionApproachRule"
Cohesion: 0.29
Nodes (5): JunctionApproachRule, PriorityRoad, PriorityToRight, Stop, TrafficLight

### Community 132 - "PlayerLifecycle"
Cohesion: 0.33
Nodes (5): PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 133 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 134 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 139 - "Difficulty"
Cohesion: 0.14
Nodes (10): Difficulty, Color, LobbyRosterEntry, DisplayName, PortraitTint, Ready, Difficulty, Easy (+2 more)

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 146 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.21
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.06
Nodes (30): Color, Dictionary, HashSet, IReadOnlyList, List, Quaternion, Transform, Vector3 (+22 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.15
Nodes (3): GameObject, Test, Story59ParameterizedDriverModelTests

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.11
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "PassengerActionDef"
Cohesion: 0.10
Nodes (19): Transform, PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot (+11 more)

### Community 480 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.06
Nodes (28): BoxCollider, CharacterController, Collider, List, NetworkTransform, Quaternion, Rigidbody, Vector3 (+20 more)

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
Cohesion: 0.26
Nodes (6): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, RageSandboxAutoStart

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
Cohesion: 0.11
Nodes (18): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsJunctionApproach, IsOutgoingConnector, JunctionId (+10 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 858 - "ReactionChannel"
Cohesion: 0.29
Nodes (5): ReactionChannel, Both, Fear, None, Rage

### Community 865 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 873 - "PassengerActionVerdictCode"
Cohesion: 0.10
Nodes (21): Rpc, RpcParams, PassengerActionValidation, PassengerActionVerdict, Accepted, PassengerActionVerdictCode, Accepted, ActorMismatch (+13 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **500 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+495 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 991 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **13 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `.ApplyRageTargetLock`, `GreyboxAssetSeedMetadata`, `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `LocalOnFootController`, `NetworkedVehicleDamageVfxController`, `NetworkedVehicleDriverController`, `Story26InGameHudTests`, `Story43PassengerActionOneChangesRageTests`, `NetworkedAIVehicleState`, `Story55NetworkedAiRageTargetingTests`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `AIVehicleBehaviorDebugView`, `DefinitionId`, `Story56RageRoadEventTriggerTests`, `RoadRage.App.Run`, `.TrySpawnSelectedProfile`, `NetworkedPlayerState`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `MonoBehaviour`, `Empty Map Entry Playmode Tests`, `.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`, `NetworkedPassengerActionIntent`, `.HandleLifecycleChanged`?**
  _High betweenness centrality (0.167) - this node is a cross-community bridge._
- **Why does `NetworkedAIVehicleDriverController` connect `NetworkedAIVehicleDriverController` to `Story512TireForcesAndSteeringTests`, `RoadRage.Shared.Domain`, `Story52BasicAiRouteFollowingAndRecoveryTests`, `VehiclePhysicsBody`, `Story510LaneGraphAndRoutedTrafficTests`, `Story514AiDrivesByIntentTests`, `TrafficPerceptionCandidate`, `.SelectWeightedSuccessor`, `NetworkedRageState`, `PortalTrafficSpawner`, `NetworkedAIVehicleState`, `JunctionClaim`, `.AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile`, `LaneGraph`, `DriverModel`, `.TrafficLeavesItsEntryTunnelUnderItsOwnPhysics`, `.TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals`, `DriverProfileDef`, `TrafficUnblockingAction`, `VehicleDriveIntent`, `Story59ParameterizedDriverModelTests`, `HostOwnedNetworkStateBehaviour`?**
  _High betweenness centrality (0.116) - this node is a cross-community bridge._
- **Why does `VehiclePhysicsBody` connect `VehiclePhysicsBody` to `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, `Story513ArcadeAssistsAndUnevenGroundPlayModeTests`, `NetworkedVehicleDriverController`, `NetworkedAIVehicleDriverController`, `Story512TireForcesAndSteeringTests`, `VehicleProfile`, `VehicleProfileDef`, `.FixedUpdate`, `.UpdateSteeringState`, `MonoBehaviour`, `Story511VehicleChassisWheelsAndSuspensionTests`, `Story514AiDrivesByIntentTests`, `TrafficPerceptionCandidate`, `VehicleDriveIntent`, `Story512TireForcesAndSteeringPlayModeTests`, `Story513ArcadeAssistsAndUnevenGroundTests`, `VehicleSuspensionModel`?**
  _High betweenness centrality (0.098) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _500 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10202020202020202 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.13911290322580644 - nodes in this community are weakly interconnected._