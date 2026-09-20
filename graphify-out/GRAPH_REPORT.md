# Graph Report - RRS  (2026-09-20)

## Corpus Check
- 187 files · ~180,630 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3442 nodes · 8215 edges · 166 communities (150 shown, 14 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 410 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `9078d7ac`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- Story14GreyboxAssetSeedTests
- Story516LobbyConfigurableTrafficSettingsTests
- Story512TireForcesAndSteeringTests
- Story11MainMenuLaunchTests
- .ReplayRoute
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- LobbyRosterSnapshot
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyFlowController
- LocalOnFootController
- UserNotice
- NpcReactionEffect
- MenuCharacterPreview
- NetworkedVehicleSeatIntent
- VehiclePhysicsBody
- .Create
- Story512TireForcesAndSteeringPlayModeTests
- Story510LaneGraphAndRoutedTrafficTests
- Story27PlayerLifecycleTests
- Story12LobbyShellTests
- Story513ArcadeAssistsAndUnevenGroundTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- RoadRage.Features.Vehicles
- .FixedUpdate
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- Story513ArcadeAssistsAndUnevenGroundPlayModeTests
- NetworkedVehicleDriverController
- LobbyRosterScreen
- PhysicsHarness
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- VehicleProfile
- VehicleProfileDef
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- NetworkedPassengerActionIntent
- LobbyCreateOutcome
- Story32DriverControlAndLocalCameraTests
- FakeSteamLobbyPlatform
- TrafficSettingsDef
- Story514AiDrivesByIntentTests
- .Author
- RoadRage.Features.Players
- .SelectWeightedSuccessor
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- VehicleWheel
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- RoadRage.Shared.Domain
- NetworkedRageState
- FakeSteamLobbyPlatform
- Lobby Join Service
- NetworkedAIVehicleState
- RunEscapeMenuFlowController
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- Story22HostCreatedPrivateRoomTests
- Story55NetworkedAiRageTargetingTests
- Story28Epic2OnlinePlayableCheckpointTests
- CharacterCatalog
- LobbyRoomService
- Story58EscapeMenuTests
- RoadRageBootstrap
- .NewFixture
- .Inspect
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- PassengerActionIntent
- RunEscapeMenuScreen
- Story57AiTrafficClientPresentationTests
- .FindRecursive
- NetworkedVehicleState.cs
- PassengerActionDef
- Story56RageRoadEventTriggerTests
- RoadRage.App.Run
- Story33SeatEntryExitAndPassengerPresenceTests
- Story51NpcRageFearFoundationTests
- .RequestHonk
- PlayerProfileResolution
- .EnsureVehicleSandboxSeatHarness
- .ApplySecondaryVehicleCollisionDamage
- NetworkedPlayerReviveIntent
- NetworkedPlayerLifecycleIntent
- PlayerProfile
- DefinitionId
- .TheRecipeReliefIsASceneObjectOnTheAvenueAndStaysWithinTheCurbHeight
- NetworkedPlayerState
- RoadRage Scaffold Structure Tests
- Story58EscapeMenuPlayModeTests
- LocalVehicleCameraRig
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- NetworkPlayerRegistry
- NetworkedPlayerLifecycleService
- Story16Epic1PlayableCheckpointTests
- TireSample
- .UpdateSteeringState
- .Inspect
- MainMenuScreen
- FakeSteamLobbyPlatform
- HostOwnedNetworkStateBehaviour
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- Private Room Play Mode Tests
- GreyboxAssetSeedMetadata
- .IsSurfaceOnlyCollision
- Netcode/Steamworks Smoke Tests
- VehicleDriveIntent
- ISteamPlatform
- AppSceneRouter.cs
- .Append
- NetworkedVehicleRecoveryIntent
- VehicleArcadeAssist
- FakeSteamPlatform
- NetworkedPassengerActionIncidentState
- MainMenuFlowController
- OnlineServicesStatus
- PlayerLifecycle
- .NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId
- FacepunchSteamPlatform
- FakeSteamPlatform
- RunCompositionRoot
- OnFootMovementIntent
- Story516ForceAssetRefresh
- CharacterOption
- SeedExpectation
- IntentFixture
- NetworkPlayerConnectionPayload
- .OnDrag
- .TearDown
- .EnsureNetworkManager
- Story52BasicAiRouteFollowingAndRecoveryTests
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
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
- MonoBehaviour
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
2. `NetworkedVehicleState` - 80 edges
3. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
4. `RoadRage.Features.Vehicles` - 73 edges
5. `RunCheckpointHudScreen` - 69 edges
6. `RoadRage.Shared.Domain` - 66 edges
7. `NetworkedVehicleDriverController` - 65 edges
8. `LobbyFlowController` - 61 edges
9. `VehiclePhysicsBody` - 59 edges
10. `NetworkedAIVehicleDriverController` - 57 edges

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

## Communities (166 total, 14 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.10
Nodes (15): ArgumentException, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component (+7 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.17
Nodes (5): GameObject, NetworkObject, Test, TextMeshProUGUI, Story25NetworkedPlayerSpawnTests

### Community 3 - "Story14GreyboxAssetSeedTests"
Cohesion: 0.17
Nodes (10): Bounds, Collider, GameObject, NetworkObject, Renderer, Test, Transform, Story14GreyboxAssetSeedTests (+2 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.14
Nodes (5): Button, FakeSteamLobbyPlatform, MonoBehaviour, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.12
Nodes (4): GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.13
Nodes (11): UserNoticeChannel, LastNotice, Canvas, CanvasScaler, Component, EventSystem, InputSystemUIInputModule, Scene (+3 more)

### Community 7 - ".ReplayRoute"
Cohesion: 0.15
Nodes (8): Bounds, List, Scene, Transform, CurbTrafficMeasurement, ReplayTrace, CurbTrafficMeasurement, ReplayTrace

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.11
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

### Community 10 - "LobbyRosterSnapshot"
Cohesion: 0.08
Nodes (17): LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId, RunLaunchRequested (+9 more)

### Community 11 - "RunFlowController"
Cohesion: 0.08
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.16
Nodes (6): GameObject, StringBuilder, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.22
Nodes (5): OnlineServicesBootstrapService, Status, ArgumentNullException, Test, Story21OnlineServicesBootstrapTests

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.09
Nodes (15): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, AssemblyDefinition, Component (+7 more)

### Community 15 - "LobbyFlowController"
Cohesion: 0.10
Nodes (8): Difficulty, HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.12
Nodes (15): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+7 more)

### Community 17 - "UserNotice"
Cohesion: 0.27
Nodes (7): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning

### Community 18 - "NpcReactionEffect"
Cohesion: 0.18
Nodes (8): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Test, TestCase

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.10
Nodes (15): Vector3, VehiclePhysicsTelemetryView, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor (+7 more)

### Community 22 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.25
Nodes (9): BoxCollider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest, Vector3 (+1 more)

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.11
Nodes (10): Action, BoxCollider, Collider, GameObject, Object, Renderer, TearDown, Test (+2 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.11
Nodes (6): CharacterController, GameObject, Test, TextMeshProUGUI, Story27PlayerLifecycleTests, IEnumerable

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.11
Nodes (14): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Color, Component, GameObject (+6 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.16
Nodes (3): SerializedObject, Test, Story513ArcadeAssistsAndUnevenGroundTests

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "RoadRage.Features.Vehicles"
Cohesion: 0.11
Nodes (4): RoadRage.Shared.Authoring, RoadRage.Tests.EditMode, RoadRage.Features.OnFoot, RoadRage.Features.Vehicles

### Community 31 - ".FixedUpdate"
Cohesion: 0.08
Nodes (18): RaycastHit, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+10 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.24
Nodes (10): BoxCollider, GameObject, IEnumerator, List, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.15
Nodes (7): Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.12
Nodes (19): Button, GameObject, Object, SerializedObject, StringBuilder, TMP_Text, Transform, Vector2 (+11 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.18
Nodes (8): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty, SerializedProperty

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.15
Nodes (10): Story516FieldDiagnostic, RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby (+2 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "NetworkedPassengerActionIntent"
Cohesion: 0.12
Nodes (15): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, List, PassengerActionCatalog, Count (+7 more)

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 45 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.15
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "TrafficSettingsDef"
Cohesion: 0.08
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation (+12 more)

### Community 48 - "Story514AiDrivesByIntentTests"
Cohesion: 0.20
Nodes (5): Func, GameObject, Rigidbody, Test, Story514AiDrivesByIntentTests

### Community 49 - ".Author"
Cohesion: 0.28
Nodes (7): Bounds, Collider, GameObject, List, Transform, Vector3, Story513RecipeRelief

### Community 51 - ".SelectWeightedSuccessor"
Cohesion: 0.17
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
Cohesion: 0.23
Nodes (9): Difficulty, Easy, Hard, Normal, Component, IEnumerator, UnityTearDown, UnityTest (+1 more)

### Community 56 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (6): SessionTrafficValue, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.Features.Economy, RoadRage.Shared.Networking, RoadRage.Features.Boss

### Community 57 - "NetworkedRageState"
Cohesion: 0.08
Nodes (21): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, IRageDispositionSource, CurrentDisposition, RageDisposition, Block (+13 more)

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 59 - "Lobby Join Service"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "NetworkedAIVehicleState"
Cohesion: 0.44
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 61 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "Story22HostCreatedPrivateRoomTests"
Cohesion: 0.31
Nodes (5): ArgumentNullException, FakeSteamLobbyPlatform, Task, Test, Story22HostCreatedPrivateRoomTests

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.15
Nodes (8): GameObject, List, NetworkObject, Object, Test, Vector3, Story55NetworkedAiRageTargetingTests, IntentFixture

### Community 65 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.12
Nodes (13): ApprovalResult, FakeSteamLobbyPlatform, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+5 more)

### Community 66 - "CharacterCatalog"
Cohesion: 0.20
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 68 - "LobbyRoomService"
Cohesion: 0.13
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 69 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

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

### Community 77 - "PassengerActionIntent"
Cohesion: 0.22
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, IEquatable, INetworkSerializable

### Community 78 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 81 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 82 - "PassengerActionDef"
Cohesion: 0.16
Nodes (12): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+4 more)

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.06
Nodes (23): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+15 more)

### Community 84 - "RoadRage.App.Run"
Cohesion: 0.13
Nodes (5): RoadRage.DevTools, RoadRage.App.Run, RoadRage.Features.Rage, RoadRage.Features.PassengerActions, RoadRage.Shared.Input

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 86 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.27
Nodes (4): List, Object, TearDown, Story51NpcRageFearFoundationTests

### Community 88 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 89 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 91 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 92 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - "DefinitionId"
Cohesion: 0.16
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 95 - ".TheRecipeReliefIsASceneObjectOnTheAvenueAndStaysWithinTheCurbHeight"
Cohesion: 0.18
Nodes (10): Action, Bounds, BoxCollider, Collider, Component, GameObject, Scene, Transform (+2 more)

### Community 96 - "NetworkedPlayerState"
Cohesion: 0.13
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 98 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.08
Nodes (24): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+16 more)

### Community 100 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 101 - "NetworkPlayerRegistry"
Cohesion: 0.29
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 103 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.24
Nodes (5): Component, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 104 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 106 - ".Inspect"
Cohesion: 0.38
Nodes (4): Collider, Component, Transform, Story513RageTargetInspection

### Community 107 - "MainMenuScreen"
Cohesion: 0.18
Nodes (6): Button, Color, GameObject, TMP_Text, MainMenuScreen, CharacterOption

### Community 108 - "FakeSteamLobbyPlatform"
Cohesion: 0.12
Nodes (8): Difficulty, Exception, FakeSteamLobbyPlatform, CreateLobbyCallCount, LastMaxMembers, LeaveCallCount, NextOutcome, ThrowOnCreate

### Community 109 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.18
Nodes (9): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority (+1 more)

### Community 110 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "GreyboxAssetSeedMetadata"
Cohesion: 0.16
Nodes (10): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+2 more)

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 115 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 116 - "ISteamPlatform"
Cohesion: 0.14
Nodes (6): ISteamPlatform, IsLoggedOn, IsValid, FakeSteamPlatform, IsLoggedOn, IsValid

### Community 117 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 118 - ".Append"
Cohesion: 0.23
Nodes (8): Canvas, CanvasScaler, Component, StringBuilder, TMP_Text, Transform, Vector2, Story516LobbyPanelInspection

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 121 - "FakeSteamPlatform"
Cohesion: 0.17
Nodes (9): Exception, FakeSteamPlatform, InitCallCount, InitCalledWithAppId, IsLoggedOn, IsValid, RunCallbacksCallCount, ShutdownCallCount (+1 more)

### Community 122 - "NetworkedPassengerActionIncidentState"
Cohesion: 0.25
Nodes (7): NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, IEnumerator, UnityTearDown, UnityTest, Story44PassengerActionTwoMvpRunPlayModeTests

### Community 125 - "OnlineServicesStatus"
Cohesion: 0.25
Nodes (6): OnlineServicesStatus, InitializationFailed, NotStarted, Offline, Online, SignInFailed

### Community 126 - "PlayerLifecycle"
Cohesion: 0.33
Nodes (5): PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 127 - ".NetworkedPlayerPresentationCreatesGreyboxVisualFromCharacterId"
Cohesion: 0.33
Nodes (3): Collider, Renderer, Type

### Community 128 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 129 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 130 - "RunCompositionRoot"
Cohesion: 0.40
Nodes (4): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot

### Community 131 - "OnFootMovementIntent"
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 133 - "CharacterOption"
Cohesion: 0.67
Nodes (3): CharacterOption, Primary, Secondary

### Community 134 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 135 - "IntentFixture"
Cohesion: 0.67
Nodes (3): IntentFixture, Intent, Target

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.24
Nodes (7): GameObject, NetworkManager, NetworkPrefabsList, NetworkManager, NetworkPrefabsList, TearDown, FacepunchTransport

### Community 146 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.25
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.13
Nodes (11): Color, HashSet, IReadOnlyList, List, Quaternion, LaneGraph, EntryPortals, ExitPortals (+3 more)

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

### Community 480 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.23
Nodes (7): Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (13): Vector3, BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion (+5 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.14
Nodes (10): RageThreshold, Disposition, MinValue, List, NetworkObject, Object, TearDown, Test (+2 more)

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.23
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 645 - "RageSandboxAutoStart"
Cohesion: 0.22
Nodes (7): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, Transform, RageSandboxAutoStart

### Community 647 - "LobbyRosterService"
Cohesion: 0.11
Nodes (9): Difficulty, Task, ISteamLobbyPlatform, Difficulty, LobbyRosterService, AllMembersReady, Current, IsSynchronized (+1 more)

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 697 - "LobbyShellScreen"
Cohesion: 0.20
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

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
Cohesion: 0.10
Nodes (13): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+5 more)

### Community 858 - "ReactionChannel"
Cohesion: 0.14
Nodes (8): TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage, ContextMenu

### Community 865 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 873 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **405 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+400 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 882 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **14 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `RunCompositionRoot`, `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `NetworkedVehicleDamageVfxController`, `LocalOnFootController`, `NetworkedVehicleDriverController`, `Story26InGameHudTests`, `NetworkedPassengerActionIntent`, `Story43PassengerActionOneChangesRageTests`, `NetworkedAIVehicleState`, `Story55NetworkedAiRageTargetingTests`, `CharacterCatalog`, `.Update`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `AIVehicleBehaviorDebugView`, `Story56RageRoadEventTriggerTests`, `RoadRage.App.Run`, `.ApplySecondaryVehicleCollisionDamage`, `NetworkedPlayerState`, `MonoBehaviour`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `Story16Epic1PlayableCheckpointTests`, `.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`, `.ShowVehicleSeatMessage`?**
  _High betweenness centrality (0.133) - this node is a cross-community bridge._
- **Why does `VehiclePhysicsBody` connect `VehiclePhysicsBody` to `MonoBehaviour`, `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, `NetworkedVehicleDriverController`, `Story513ArcadeAssistsAndUnevenGroundPlayModeTests`, `NetworkedAIVehicleDriverController`, `Story512TireForcesAndSteeringTests`, `VehicleProfile`, `VehicleProfileDef`, `.Inspect`, `.UpdateSteeringState`, `.Inspect`, `Story511VehicleChassisWheelsAndSuspensionTests`, `Story514AiDrivesByIntentTests`, `VehicleDriveIntent`, `Story512TireForcesAndSteeringPlayModeTests`, `VehicleArcadeAssist`, `Story513ArcadeAssistsAndUnevenGroundTests`, `.FixedUpdate`?**
  _High betweenness centrality (0.117) - this node is a cross-community bridge._
- **Why does `LobbyFlowController` connect `LobbyFlowController` to `MonoBehaviour`, `CharacterCatalog`, `LobbyRosterScreen`, `LobbyRoomService`, `.TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals`, `Story25NetworkedPlayerSpawnTests`, `LobbyRosterService`, `RoadRage.Features.UI`, `Story16Epic1PlayableCheckpointTests`, `Story516LobbyConfigurableTrafficSettingsTests`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `OnlineServicesBootstrapService`, `TrafficSettingsDef`, `Story12LobbyShellPlayModeTests`, `LobbyShellScreen`, `Story12LobbyShellTests`, `Lobby Join Service`, `OnlineServicesStatus`?**
  _High betweenness centrality (0.089) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _405 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10202020202020202 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story516LobbyConfigurableTrafficSettingsTests` be split into smaller, more focused modules?**
  _Cohesion score 0.13765182186234817 - nodes in this community are weakly interconnected._