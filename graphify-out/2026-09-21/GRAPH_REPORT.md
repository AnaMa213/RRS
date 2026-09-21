# Graph Report - RRS  (2026-09-21)

## Corpus Check
- 198 files · ~233,286 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4040 nodes · 9862 edges · 166 communities (153 shown, 13 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 473 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `210f4881`
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
- Story518TrajectoryConflictModelTests
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- FakeSteamLobbyPlatform
- RoadRage.Shared.Domain
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- LobbyFlowController
- LocalOnFootController
- Story518LaneContainmentTests
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
- Test
- VehicleProfile
- VehicleProfileDef
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- RageRoadEventFlowController
- LobbyRosterSnapshot
- .RefreshPerception
- FakeSteamLobbyPlatform
- JunctionApproachInfo
- Story514AiDrivesByIntentTests
- Story517WiderPerceptionAndProgressiveUnblockingTests
- RoadRage.Features.Players
- List
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- Test
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- TrafficSignalPlan
- Story54RageDrivenAiBehaviorStatesTests
- TrafficSettingsDef
- RoadRageBootstrap
- NetworkedAIVehicleState
- LobbyShellScreen
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- AppSceneRouter.cs
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- RunFlowController
- FakeSteamLobbyPlatform
- Story58EscapeMenuTests
- LaneGraphRouting
- .NewFixture
- JunctionClaim
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- LobbyJoinService
- LobbyRosterService
- Story57AiTrafficClientPresentationTests
- NetworkedPlayerLifecycleService
- RoadRage.Shared.Networking
- TrafficPerception
- Story56RageRoadEventTriggerTests
- RoadRage.App.Run
- PassengerActionDef
- .TearDown
- PortalTrafficSpawner
- .NewTuning
- NetworkedPlayerState
- .TheRecipeReliefIsASceneObjectOnTheAvenueAndStaysWithinTheCurbHeight
- Story52BasicAiRouteFollowingAndRecoveryTests
- .TrafficLeavesItsEntryTunnelUnderItsOwnPhysics
- PlayerProfile
- TrafficPerceptionCandidate
- TrafficSignalLampView
- Story518RoundaboutPriorityTests
- RoadRage Scaffold Structure Tests
- Story16Epic1PlayableCheckpointTests
- LocalVehicleCameraRig
- Story518IntersectionRulesAndDeadlockPreventionTests
- GreyboxAssetSeedMetadata
- .RageSandboxShowsTheSharedIncidentMarker
- RunEscapeMenuFlowController
- .FixedUpdate
- .UpdateSteeringState
- TrafficDecisionReason
- MainMenuScreen
- DriverProfileDef
- .Create
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- Private Room Play Mode Tests
- RunEscapeMenuScreen
- .IsSurfaceOnlyCollision
- Netcode/Steamworks Smoke Tests
- VehicleArcadeAssist
- Story518TrafficAnomalyRegressionTests
- .ApplyHonkTarget
- PlayerLifecycle
- NetworkedVehicleRecoveryIntent
- MonoBehaviour
- CharacterCatalog
- NetworkedPassengerActionIntent
- Story33SeatEntryExitAndPassengerPresenceTests
- PlayerMode
- NetworkedPlayerLifecycleIntent
- MainMenuFlowController
- PassengerActionIntent
- VehicleDriveIntent
- DefinitionId
- TireSample
- OnFootMovementIntent
- .TickLocalTraffic
- FakeSteamPlatform
- Story58EscapeMenuPlayModeTests
- .RefreshVehicleDamageHud
- RunCompositionRoot
- Story518WaitCycleTests
- .OrdinaryDistrictTrafficClearsItsJunctionsWithoutUnblockingOrCircularWaits
- Difficulty
- Story16Epic1PlayableCheckpointTests.cs
- SeedExpectation
- JunctionApproachRule
- .ElectedId
- .TearDown
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- PassengerActionVerdictCode
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- LocalVoidRespawnController
- NetworkedRageState
- Story21OnlineServicesPlayModeTests
- Lock-Rage Camera Fix Query
- LaneNode
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedPlayerSpawnService
- ReactionChannel
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `NetworkedAIVehicleDriverController` - 168 edges
2. `RunFlowController` - 118 edges
3. `LaneGraph` - 90 edges
4. `RoadRage.Features.Vehicles` - 87 edges
5. `NetworkedVehicleState` - 79 edges
6. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
7. `DriverProfile` - 70 edges
8. `RunCheckpointHudScreen` - 69 edges
9. `RoadRage.Shared.Domain` - 68 edges
10. `NetworkedVehicleDriverController` - 65 edges

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

## Communities (166 total, 13 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.10
Nodes (15): ArgumentException, PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component, List (+7 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.05
Nodes (30): ApprovalResult, GameObject, NetworkManager, NetworkPrefabsList, NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId (+22 more)

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

### Community 7 - "Story518TrajectoryConflictModelTests"
Cohesion: 0.26
Nodes (5): IReadOnlyList, List, Test, Vector3, Story518TrajectoryConflictModelTests

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.07
Nodes (12): Vector3, NetworkedVehicleSeatService, Instance, Func, NetworkVariable, Vector3, NetworkedVehicleState, CurrentDamageThresholdsCrossed (+4 more)

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.12
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (9): Difficulty, Task, FakeSteamLobbyPlatform, LastAiVehicleTargetCount, LastLitterThrowerCount, NextCreateOutcome, NextJoinOutcome, NextRoster (+1 more)

### Community 11 - "RoadRage.Shared.Domain"
Cohesion: 0.09
Nodes (5): SessionTrafficValue, RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.Features.Lobby

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.11
Nodes (6): GameObject, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform, StringBuilder

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.06
Nodes (27): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+19 more)

### Community 14 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.14
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 15 - "LobbyFlowController"
Cohesion: 0.10
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.12
Nodes (15): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+7 more)

### Community 17 - "Story518LaneContainmentTests"
Cohesion: 0.15
Nodes (13): Action, BoxCollider, GameObject, IEnumerable, List, Scene, Test, Vector3 (+5 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, MaterialPropertyBlock, RawImage, Renderer, Transform, MenuCharacterPreview (+2 more)

### Community 20 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.05
Nodes (24): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, Key, Rpc, RpcParams, NetworkedVehicleSeatIntent (+16 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (15): Vector3, VehiclePhysicsTelemetryView, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor (+7 more)

### Community 22 - "DriverProfile"
Cohesion: 0.05
Nodes (35): DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, HornDelay, JunctionAcceptedGap, JunctionApproachRadius (+27 more)

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.10
Nodes (9): Action, BoxCollider, Collider, GameObject, Object, TearDown, Test, Vector3 (+1 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.10
Nodes (6): IEnumerable, CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.11
Nodes (14): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject (+6 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.14
Nodes (4): SerializedObject, SerializedProperty, Test, Story513ArcadeAssistsAndUnevenGroundTests

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "RoadRage.Features.Vehicles"
Cohesion: 0.09
Nodes (3): RoadRage.Tests.EditMode, RoadRage.Features.Rage, RoadRage.Features.Vehicles

### Community 31 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (14): TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees, Speed (+6 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.13
Nodes (7): Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "Test"
Cohesion: 0.16
Nodes (3): JunctionRules, Test, Vector3

### Community 38 - "VehicleProfile"
Cohesion: 0.04
Nodes (45): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+37 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.24
Nodes (8): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Shared.Presentation, RoadRage.Tests.PlayMode

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.08
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "RageRoadEventFlowController"
Cohesion: 0.14
Nodes (9): GameObject, NetworkObjectReference, RageRoadEventFlowController, RageRoadEventState, Confrontation, Idle, Resolved, RewardGranted (+1 more)

### Community 44 - "LobbyRosterSnapshot"
Cohesion: 0.10
Nodes (23): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+15 more)

### Community 45 - ".RefreshPerception"
Cohesion: 0.19
Nodes (4): CharacterController, Collider, Quaternion, Vector3

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "JunctionApproachInfo"
Cohesion: 0.08
Nodes (19): JunctionApproachInfo, ConflictEntryDistance, EntryForward, EntryNode, EntryPoint, ExitProbeNode, Forward, JunctionId (+11 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.20
Nodes (5): Func, GameObject, Rigidbody, Test, Story514AiDrivesByIntentTests

### Community 49 - "Story517WiderPerceptionAndProgressiveUnblockingTests"
Cohesion: 0.19
Nodes (7): Action, GameObject, Rigidbody, Test, TestCase, Story517WiderPerceptionAndProgressiveUnblockingTests, FieldInfo

### Community 50 - "RoadRage.Features.Players"
Cohesion: 0.14
Nodes (3): RoadRage.Features.Players, RoadRage.Features.PassengerActions, RoadRage.Shared.Definitions

### Community 51 - "List"
Cohesion: 0.14
Nodes (8): Bounds, List, Renderer, Scene, Transform, CurbTrafficMeasurement, ReplayTrace, CurbTrafficMeasurement

### Community 52 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 53 - "Test"
Cohesion: 0.23
Nodes (3): Test, Story518StalledHeadOnTests, Story518YieldStopLineTests

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 56 - "TrafficSignalPlan"
Cohesion: 0.16
Nodes (12): IReadOnlyList, JunctionVerdict, Proceed, Wait, TrafficSignalPhase, DurationSeconds, GreenGroups, TrafficSignalPlan (+4 more)

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.09
Nodes (18): AiVehicleFixture, IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee (+10 more)

### Community 58 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (14): IReadOnlyList, TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers (+6 more)

### Community 59 - "RoadRageBootstrap"
Cohesion: 0.07
Nodes (18): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+10 more)

### Community 60 - "NetworkedAIVehicleState"
Cohesion: 0.32
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, CharacterController, NetworkVariable, NetworkedAIVehicleState

### Community 61 - "LobbyShellScreen"
Cohesion: 0.20
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.10
Nodes (15): ArgumentNullException, Difficulty, Exception, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount, LastMaxMembers (+7 more)

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.13
Nodes (11): GameObject, List, NetworkObject, Object, Test, Vector3, IntentFixture, Intent (+3 more)

### Community 65 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 66 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 67 - "RunFlowController"
Cohesion: 0.07
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 68 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 69 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 70 - "LaneGraphRouting"
Cohesion: 0.06
Nodes (20): Func, IReadOnlyList, Vector3, LaneGraphRouting, Test, Vector3, Story518CurveFollowingTests, Action (+12 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 72 - "JunctionClaim"
Cohesion: 0.09
Nodes (18): Vector3, JunctionClaim, ApproachForward, Breach, Committed, DistanceToJunction, Eligibility, EntryPoint (+10 more)

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

### Community 77 - "LobbyJoinService"
Cohesion: 0.12
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 78 - "LobbyRosterService"
Cohesion: 0.06
Nodes (20): Difficulty, Task, ISteamLobbyPlatform, Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status (+12 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 81 - "RoadRage.Shared.Networking"
Cohesion: 0.13
Nodes (9): NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking (+1 more)

### Community 82 - "TrafficPerception"
Cohesion: 0.28
Nodes (4): IReadOnlyList, Quaternion, Vector3, TrafficPerception

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.11
Nodes (11): IReadOnlyList, IReadOnlyList, RageRoadEventLifecycle, GameObject, List, NetworkObject, Object, TearDown (+3 more)

### Community 84 - "RoadRage.App.Run"
Cohesion: 0.16
Nodes (3): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Shared.Input

### Community 85 - "PassengerActionDef"
Cohesion: 0.11
Nodes (18): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+10 more)

### Community 87 - "PortalTrafficSpawner"
Cohesion: 0.18
Nodes (8): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation

### Community 89 - "NetworkedPlayerState"
Cohesion: 0.16
Nodes (9): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart, FixedString32Bytes, NetworkVariable, Vector3 (+1 more)

### Community 90 - ".TheRecipeReliefIsASceneObjectOnTheAvenueAndStaysWithinTheCurbHeight"
Cohesion: 0.18
Nodes (10): Action, Bounds, BoxCollider, Collider, Component, GameObject, MeshFilter, MeshRenderer (+2 more)

### Community 91 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.21
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 92 - ".TrafficLeavesItsEntryTunnelUnderItsOwnPhysics"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story517WiderPerceptionAndProgressiveUnblockingPlayModeTests

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - "TrafficPerceptionCandidate"
Cohesion: 0.10
Nodes (19): TrafficPerceptionCandidate, AngleDegrees, ConflictArrival, Distance, Extents, HalfWidth, IsBehind, IsCollisionThreat (+11 more)

### Community 95 - "TrafficSignalLampView"
Cohesion: 0.33
Nodes (4): Color, MaterialPropertyBlock, Renderer, TrafficSignalLampView

### Community 96 - "Story518RoundaboutPriorityTests"
Cohesion: 0.33
Nodes (4): Action, Scene, Test, Story518RoundaboutPriorityTests

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 98 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.24
Nodes (5): Component, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.05
Nodes (35): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+27 more)

### Community 100 - "Story518IntersectionRulesAndDeadlockPreventionTests"
Cohesion: 0.13
Nodes (9): Action, Collider, Component, GameObject, MeshFilter, MeshRenderer, Scene, Transform (+1 more)

### Community 101 - "GreyboxAssetSeedMetadata"
Cohesion: 0.16
Nodes (10): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+2 more)

### Community 102 - ".RageSandboxShowsTheSharedIncidentMarker"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story44PassengerActionTwoMvpRunPlayModeTests

### Community 103 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 104 - ".FixedUpdate"
Cohesion: 0.18
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 106 - "TrafficDecisionReason"
Cohesion: 0.09
Nodes (21): PathConflict, Exists, OtherArrival, OwnArrival, Point, Time, TrafficDecisionReason, Cruise (+13 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 108 - "DriverProfileDef"
Cohesion: 0.36
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

### Community 112 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.24
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 116 - "Story518TrafficAnomalyRegressionTests"
Cohesion: 0.21
Nodes (4): Collider, Test, Vector3, Story518TrafficAnomalyRegressionTests

### Community 118 - "PlayerLifecycle"
Cohesion: 0.33
Nodes (5): PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 120 - "MonoBehaviour"
Cohesion: 0.20
Nodes (6): Color, Image, TMP_Text, LobbyPlayerSlotView, DevIndestructibleVehicle, MonoBehaviour

### Community 121 - "CharacterCatalog"
Cohesion: 0.20
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 122 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (23): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+15 more)

### Community 123 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 124 - "PlayerMode"
Cohesion: 0.15
Nodes (8): CharacterController, PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 125 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 127 - "PassengerActionIntent"
Cohesion: 0.22
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 128 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 129 - "DefinitionId"
Cohesion: 0.10
Nodes (18): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+10 more)

### Community 130 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 131 - "OnFootMovementIntent"
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 132 - ".TickLocalTraffic"
Cohesion: 0.11
Nodes (8): Vector3, TrafficUnblockingAction, FollowRoute, IntersectionBreach, Replan, Reverse, RoadDetour, SidewalkDetour

### Community 133 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 134 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 136 - "RunCompositionRoot"
Cohesion: 0.40
Nodes (4): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot

### Community 137 - "Story518WaitCycleTests"
Cohesion: 0.38
Nodes (3): List, Test, Story518WaitCycleTests

### Community 138 - ".OrdinaryDistrictTrafficClearsItsJunctionsWithoutUnblockingOrCircularWaits"
Cohesion: 0.27
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story518NormalTrafficFlowPlayModeTests

### Community 139 - "Difficulty"
Cohesion: 0.20
Nodes (6): Difficulty, Difficulty, Difficulty, Easy, Hard, Normal

### Community 141 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 142 - "JunctionApproachRule"
Cohesion: 0.25
Nodes (5): JunctionApproachRule, PriorityRoad, PriorityToRight, Stop, TrafficLight

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.08
Nodes (17): Color, Dictionary, HashSet, IReadOnlyList, List, Quaternion, Transform, Vector3 (+9 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.10
Nodes (4): DriverModel, GameObject, Test, Story59ParameterizedDriverModelTests

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.11
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "PassengerActionVerdictCode"
Cohesion: 0.09
Nodes (21): Rpc, RpcParams, PassengerActionValidation, PassengerActionVerdict, Accepted, PassengerActionVerdictCode, Accepted, ActorMismatch (+13 more)

### Community 480 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.03
Nodes (44): BoxCollider, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, Rigidbody, HasReachedExitPortal (+36 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.14
Nodes (10): RageThreshold, Disposition, MinValue, List, NetworkObject, Object, TearDown, Test (+2 more)

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 645 - "NetworkedRageState"
Cohesion: 0.19
Nodes (10): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, Transform, RageSandboxAutoStart, NetworkVariable (+2 more)

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 824 - "LaneNode"
Cohesion: 0.11
Nodes (18): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsJunctionApproach, IsOutgoingConnector, JunctionId (+10 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedPlayerSpawnService"
Cohesion: 0.18
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

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
- **548 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+543 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1055 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **13 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `NetworkedAIVehicleDriverController` connect `NetworkedAIVehicleDriverController` to `.TickLocalTraffic`, `Story512TireForcesAndSteeringTests`, `.OrdinaryDistrictTrafficClearsItsJunctionsWithoutUnblockingOrCircularWaits`, `RoadRage.Shared.Domain`, `VehiclePhysicsBody`, `Story510LaneGraphAndRoutedTrafficTests`, `.RefreshPerception`, `JunctionApproachInfo`, `Story514AiDrivesByIntentTests`, `Story517WiderPerceptionAndProgressiveUnblockingTests`, `Story54RageDrivenAiBehaviorStatesTests`, `NetworkedAIVehicleState`, `.TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals`, `LaneGraphRouting`, `JunctionClaim`, `AIVehicleBehaviorDebugView`, `.AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile`, `PortalTrafficSpawner`, `LaneGraph`, `Story52BasicAiRouteFollowingAndRecoveryTests`, `.TrafficLeavesItsEntryTunnelUnderItsOwnPhysics`, `TrafficPerceptionCandidate`, `TrafficDecisionReason`, `DriverProfileDef`, `Story59ParameterizedDriverModelTests`, `NetworkedPassengerActionIntent`?**
  _High betweenness centrality (0.171) - this node is a cross-community bridge._
- **Why does `RunFlowController` connect `RunFlowController` to `.RefreshVehicleDamageHud`, `RunCompositionRoot`, `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `LocalOnFootController`, `NetworkedVehicleDamageVfxController`, `NetworkedVehicleDriverController`, `Story26InGameHudTests`, `Story43PassengerActionOneChangesRageTests`, `NetworkedAIVehicleState`, `Story55NetworkedAiRageTargetingTests`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `AIVehicleBehaviorDebugView`, `Story56RageRoadEventTriggerTests`, `RoadRage.App.Run`, `NetworkedPlayerState`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Story16Epic1PlayableCheckpointTests`, `Empty Map Entry Playmode Tests`, `.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`, `MonoBehaviour`, `CharacterCatalog`, `NetworkedPassengerActionIntent`, `PlayerMode`?**
  _High betweenness centrality (0.145) - this node is a cross-community bridge._
- **Why does `VehiclePhysicsBody` connect `VehiclePhysicsBody` to `VehicleDriveIntent`, `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, `NetworkedVehicleDriverController`, `Story513ArcadeAssistsAndUnevenGroundPlayModeTests`, `NetworkedAIVehicleDriverController`, `Story512TireForcesAndSteeringTests`, `VehicleProfile`, `VehicleProfileDef`, `.FixedUpdate`, `.UpdateSteeringState`, `Story511VehicleChassisWheelsAndSuspensionTests`, `Story514AiDrivesByIntentTests`, `Story517WiderPerceptionAndProgressiveUnblockingTests`, `VehicleArcadeAssist`, `Story512TireForcesAndSteeringPlayModeTests`, `MonoBehaviour`, `Story513ArcadeAssistsAndUnevenGroundTests`, `VehicleSuspensionModel`?**
  _High betweenness centrality (0.112) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _548 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09797979797979799 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.052614052614052616 - nodes in this community are weakly interconnected._