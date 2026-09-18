# Graph Report - RRS  (2026-09-18)

## Corpus Check
- 168 files · ~133,384 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3013 nodes · 7073 edges · 153 communities (126 shown, 23 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 381 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `bdb9330b`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- Story14GreyboxAssetSeedTests
- Difficulty
- RoadRage.Features.UI
- Story11MainMenuLaunchTests
- Story510LaneGraphAndRoutedTrafficTests
- NetworkedVehicleState
- Story511VehicleChassisWheelsAndSuspensionTests
- PortalTrafficSpawner
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
- NetworkedAIVehicleState
- RoadRage.Features.Players
- .WithMvpRun
- Story27PlayerLifecycleTests
- Story12LobbyShellTests
- RoadRage.Shared.Networking
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- VehicleDriveIntent
- VehicleSuspensionModel
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- RoadRage.App.Run
- NetworkedVehicleDriverController
- LobbyRosterScreen
- PassengerActionDef
- VehicleProfile
- VehicleProfileDef
- .HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels
- LobbyJoinOutcome
- Story26InGameHudTests
- LobbyCreateOutcome
- FakeSteamLobbyPlatform
- Story32DriverControlAndLocalCameraTests
- .SelectWeightedSuccessor
- PlayerProfileBootstrapService
- .RequestHonk
- Story28Epic2OnlinePlayableCheckpointTests
- LobbyRoomService
- Story16Epic1PlayableCheckpointTests
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- MainMenuFlowController
- Story54RageDrivenAiBehaviorStatesTests
- FakeSteamLobbyPlatform
- Lobby Join Service
- .MainMenuSceneWiresTheSelectionSurfaceAndTheProfileFlow
- GreyboxAssetSeedMetadata
- HostOwnedNetworkStateBehaviour
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- RoadRage.Features.Run
- CharacterCatalog
- .ApplyAuthoritative
- .SubmitPoseRpc
- Story58EscapeMenuTests
- RoadRageBootstrap
- .NewFixture
- RageTuningDef
- Story15EmptyMapEntryTests
- NetworkedPlayerReviveIntent
- NetworkedRunSessionMonitor
- NetworkedPassengerActionIntent
- NetworkedPlayerState
- Story57AiTrafficClientPresentationTests
- PassengerActionCatalog
- VehicleDriveIntent
- CharacterOption
- Story56RageRoadEventTriggerTests
- .ResolveExitLocalOffset
- .IsSurfaceOnlyCollision
- SeedExpectation
- .OnDrag
- .TearDown
- NetworkObject
- CharacterController
- IReadOnlyList
- NetworkObjectReference
- PlayerProfile
- DefinitionId
- Rpc
- RpcParams
- RoadRage Scaffold Structure Tests
- Object
- LocalVehicleCameraRig
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- Renderer
- Scene
- TearDown
- Func
- UnityTearDown
- UnityTest
- MainMenuScreen
- AIVehicleBehaviorDebugView
- RaycastHit
- NetworkedRageState
- Private Room Play Mode Tests
- NetworkedPlayerLifecycleIntent
- Netcode/Steamworks Smoke Tests
- NetworkedVehicleRecoveryIntent
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FacepunchSteamPlatform
- OnFootMovementIntent
- .EnsureNetworkManager
- Story52BasicAiRouteFollowingAndRecoveryTests
- Story51NpcRageFearFoundationTests
- PlayerMode
- NetworkedPlayerPresentation
- LaneGraph
- Story59ParameterizedDriverModelTests
- FacepunchSteamLobbyPlatform
- NetworkedPlayerSpawnService
- .BootstrapToWorldCompletesEpic1PlayableCheckpoint
- Story42PassengerActionFrameworkTests
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- NetworkedAIVehicleDriverController
- Empty Map Entry Playmode Tests
- Story41RageStateModuleAndDefinitionsTests
- LocalVoidRespawnController
- RageSandboxAutoStart
- LobbyRosterSnapshot
- Story21OnlineServicesPlayModeTests
- LobbyShellScreen
- Lock-Rage Camera Fix Query
- RoadRage.Shared.Domain
- MonoBehaviour
- Story33SeatEntryExitAndPassengerPresenceTests
- NetworkManager
- LaneNode
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedVehicleSeatService
- RageStateDebugView
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- PassengerActionVerdictCode
- .Create
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RunFlowController` - 118 edges
2. `NetworkedVehicleState` - 75 edges
3. `Story510LaneGraphAndRoutedTrafficTests` - 70 edges
4. `RunCheckpointHudScreen` - 69 edges
5. `NetworkedVehicleDriverController` - 64 edges
6. `RoadRage.Shared.Domain` - 60 edges
7. `RoadRage.Features.Vehicles` - 58 edges
8. `NetworkedAIVehicleDriverController` - 49 edges
9. `RoadRage.Features.Players` - 49 edges
10. `LobbyFlowController` - 48 edges

## Surprising Connections (you probably didn't know these)
- `RunFlowController` --references--> `NetworkedVehicleDriverController`  [EXTRACTED]
  Assets/RoadRage/App/Run/RunFlowController.cs → Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs
- `NetworkedVehicleDriverController` --references--> `VehiclePhysicsBody`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs → Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs
- `VehiclePhysicsBody` --references--> `VehicleProfile`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs → Assets/RoadRage/Features/Vehicles/VehicleProfile.cs
- `VehiclePhysicsBody` --references--> `VehicleProfileDef`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs → Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs
- `VehicleProfileDef` --references--> `VehicleProfile`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs → Assets/RoadRage/Features/Vehicles/VehicleProfile.cs

## Import Cycles
- None detected.

## Communities (153 total, 23 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.13
Nodes (11): ArgumentException, PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, List, Object (+3 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.08
Nodes (17): NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry, Collider, GameObject (+9 more)

### Community 3 - "Story14GreyboxAssetSeedTests"
Cohesion: 0.17
Nodes (10): Collider, GameObject, NetworkObject, Renderer, Test, Transform, Story14GreyboxAssetSeedTests, Bounds (+2 more)

### Community 4 - "Difficulty"
Cohesion: 0.18
Nodes (6): Difficulty, Difficulty, Difficulty, Easy, Hard, Normal

### Community 5 - "RoadRage.Features.UI"
Cohesion: 0.25
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.18
Nodes (9): Canvas, Component, EventSystem, InputSystemUIInputModule, Scene, SerializeField, Test, Story11MainMenuLaunchTests (+1 more)

### Community 7 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.10
Nodes (11): BoxCollider, Collider, GameObject, LaneNode, Test, Vector3, Story510LaneGraphAndRoutedTrafficTests, LaneNodeRole (+3 more)

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.16
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.13
Nodes (10): Collider, GameObject, LaneNode, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+2 more)

### Community 10 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (17): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation (+9 more)

### Community 11 - "RunFlowController"
Cohesion: 0.10
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.16
Nodes (6): GameObject, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform, StringBuilder

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (23): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+15 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.09
Nodes (15): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, AssemblyDefinition, Component (+7 more)

### Community 15 - "LobbyFlowController"
Cohesion: 0.13
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.12
Nodes (15): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+7 more)

### Community 17 - "UserNotice"
Cohesion: 0.18
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

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
Cohesion: 0.12
Nodes (12): Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody, TelemetrySample, VehiclePhysicsBody, HasProfile, Profile (+4 more)

### Community 22 - "NetworkedAIVehicleState"
Cohesion: 0.19
Nodes (7): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, Test, Vector3

### Community 23 - "RoadRage.Features.Players"
Cohesion: 0.15
Nodes (3): RoadRage.Features.Players, RoadRage.Features.PassengerActions, RoadRage.Shared.Definitions

### Community 24 - ".WithMvpRun"
Cohesion: 0.21
Nodes (5): Action, NetworkedAIVehicleState, Transform, PortalTrafficSpawner, Scene

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.07
Nodes (13): NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed, CharacterController (+5 more)

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.12
Nodes (12): Difficulty, MatchSettings, Difficulty, Color, Component, GameObject, Image, Scene (+4 more)

### Community 27 - "RoadRage.Shared.Networking"
Cohesion: 0.12
Nodes (9): NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel, RoadRage.Features.Economy, RoadRage.Shared.Networking (+1 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.15
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 31 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, Vector3, Story511VehicleChassisWheelsAndSuspensionPlayModeTests, UnitySetUp (+2 more)

### Community 33 - "RoadRage.App.Run"
Cohesion: 0.22
Nodes (3): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Shared.Input

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.14
Nodes (11): DevIndestructibleVehicle, Action, Collider, NetworkedVehicleState, NetworkTransform, Quaternion, Rigidbody, Transform (+3 more)

### Community 37 - "PassengerActionDef"
Cohesion: 0.15
Nodes (11): Transform, PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot (+3 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.07
Nodes (25): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, CenterOfMass, Damper, GroundMask (+17 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (6): Vector3, VehicleProfileDef, Id, Profile, RawId, DefinitionId

### Community 40 - ".HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels"
Cohesion: 0.27
Nodes (6): Component, IEnumerator, NetworkTransform, UnityTearDown, UnityTest, Story57AiTrafficClientPresentationPlayModeTests

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.14
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.15
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 48 - ".SelectWeightedSuccessor"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 49 - "PlayerProfileBootstrapService"
Cohesion: 0.27
Nodes (7): PlayerProfileBootstrapService, PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist, PlayerProfileResolution

### Community 50 - ".RequestHonk"
Cohesion: 0.38
Nodes (3): NetworkedAIVehicleState, NetworkObjectReference, Rpc

### Community 51 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.12
Nodes (13): ApprovalResult, FakeSteamLobbyPlatform, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+5 more)

### Community 52 - "LobbyRoomService"
Cohesion: 0.11
Nodes (13): Task, ISteamLobbyPlatform, Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus (+5 more)

### Community 53 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.24
Nodes (5): Component, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.12
Nodes (11): AiVehicleFixture, IRageDispositionSource, CurrentDisposition, GameObject, List, NetworkObject, Object, TearDown (+3 more)

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 59 - "Lobby Join Service"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - ".MainMenuSceneWiresTheSelectionSurfaceAndTheProfileFlow"
Cohesion: 0.28
Nodes (4): Component, RawImage, Scene, Transform

### Community 61 - "GreyboxAssetSeedMetadata"
Cohesion: 0.16
Nodes (10): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+2 more)

### Community 62 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.22
Nodes (7): NetworkVariable, NetworkedBossState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority, NetworkBehaviour

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.17
Nodes (7): GameObject, List, NetworkObject, Object, TearDown, Story55NetworkedAiRageTargetingTests, IntentFixture

### Community 66 - "CharacterCatalog"
Cohesion: 0.20
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 67 - ".ApplyAuthoritative"
Cohesion: 0.11
Nodes (13): Rpc, RpcParams, FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, PassengerActionValidation, PassengerActionValidationContext, PassengerActionVerdict (+5 more)

### Community 68 - ".SubmitPoseRpc"
Cohesion: 0.43
Nodes (3): Rpc, RpcParams, Vector3

### Community 69 - "Story58EscapeMenuTests"
Cohesion: 0.05
Nodes (25): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, Canvas (+17 more)

### Community 70 - "RoadRageBootstrap"
Cohesion: 0.10
Nodes (14): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+6 more)

### Community 71 - ".NewFixture"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.08
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 74 - "Story15EmptyMapEntryTests"
Cohesion: 0.12
Nodes (12): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot, AssemblyDefinition, GameObject, Object, Scene (+4 more)

### Community 75 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - "NetworkedPassengerActionIntent"
Cohesion: 0.15
Nodes (13): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, Action, PassengerActionDebugView (+5 more)

### Community 78 - "NetworkedPlayerState"
Cohesion: 0.27
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - "PassengerActionCatalog"
Cohesion: 0.40
Nodes (4): List, PassengerActionCatalog, Count, Version

### Community 81 - "VehicleDriveIntent"
Cohesion: 0.33
Nodes (5): VehicleDriveIntent, BrakeReverse, IsIdle, Steer, Throttle

### Community 82 - "CharacterOption"
Cohesion: 0.67
Nodes (3): CharacterOption, Primary, Secondary

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.05
Nodes (30): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+22 more)

### Community 86 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - "DefinitionId"
Cohesion: 0.16
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.08
Nodes (24): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+16 more)

### Community 100 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.14
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 107 - "MainMenuScreen"
Cohesion: 0.18
Nodes (6): Button, Color, GameObject, TMP_Text, MainMenuScreen, CharacterOption

### Community 108 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 110 - "NetworkedRageState"
Cohesion: 0.18
Nodes (9): NetworkVariable, NetworkedRageState, CurrentDisposition, IntentFixture, Intent, Target, IEnumerator, UnityTest (+1 more)

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 113 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 120 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 128 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 131 - "OnFootMovementIntent"
Cohesion: 0.20
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 146 - "Story52BasicAiRouteFollowingAndRecoveryTests"
Cohesion: 0.25
Nodes (3): Test, Vector3, Story52BasicAiRouteFollowingAndRecoveryTests

### Community 150 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.27
Nodes (4): List, Object, TearDown, Story51NpcRageFearFoundationTests

### Community 153 - "PlayerMode"
Cohesion: 0.17
Nodes (8): CharacterController, PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.19
Nodes (7): Collider, FixedString32Bytes, GameObject, NetworkedPlayerPresentation, CharacterCatalog, RenderedCharacterId, VisualInstance

### Community 216 - "LaneGraph"
Cohesion: 0.11
Nodes (14): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+6 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (20): DriverModel, DriverProfile, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold, MaxAcceleration (+12 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.11
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 266 - "NetworkedPlayerSpawnService"
Cohesion: 0.15
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

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
Cohesion: 0.08
Nodes (19): BoxCollider, Collider, LaneGraph, List, NetworkedAIVehicleState, NetworkTransform, Quaternion, RaycastHit (+11 more)

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

### Community 647 - "LobbyRosterSnapshot"
Cohesion: 0.10
Nodes (12): Difficulty, LobbyRosterSnapshot, Difficulty, HasLobby, Members, OwnerId, RunLaunchRequested, LobbyRosterService (+4 more)

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 697 - "LobbyShellScreen"
Cohesion: 0.20
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 722 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (5): RoadRage.DevTools, RoadRage.Shared.Domain, RoadRage.Tests.EditMode, RoadRage.Features.Rage, RoadRage.Features.Vehicles

### Community 736 - "MonoBehaviour"
Cohesion: 0.12
Nodes (13): GameObject, IEnumerator, NetworkedVehicleState, RoadRageNetcodeSmokeTestAutoStart, Color, Image, TMP_Text, LobbyPlayerSlotView (+5 more)

### Community 780 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 824 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.21
Nodes (3): Vector3, NetworkedVehicleSeatService, Instance

### Community 858 - "RageStateDebugView"
Cohesion: 0.15
Nodes (8): TMP_Text, RageStateDebugView, ReactionChannel, Both, Fear, None, Rage, ContextMenu

### Community 861 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 865 - ".FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable"
Cohesion: 0.32
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests

### Community 871 - ".GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt"
Cohesion: 0.18
Nodes (10): AssemblyDefinition, BoxCollider, Collider, GameObject, NetworkObject, Renderer, Test, Vector3 (+2 more)

### Community 873 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 874 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **356 isolated node(s):** `HasReachedExitPortal`, `HasProfile`, `WheelCount`, `Profile`, `Mass` (+351 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 793 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **23 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `NetworkedVehicleDamageVfxController`, `LocalOnFootController`, `NetworkedAIVehicleState`, `PlayerMode`, `RoadRage.App.Run`, `NetworkedVehicleDriverController`, `.Update`, `Story26InGameHudTests`, `.HandleLifecycleChanged`, `Story16Epic1PlayableCheckpointTests`, `Story43PassengerActionOneChangesRageTests`, `Story55NetworkedAiRageTargetingTests`, `CharacterCatalog`, `.ApplySecondaryVehicleCollisionDamage`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `NetworkedPassengerActionIntent`, `NetworkedPlayerState`, `PassengerActionCatalog`, `Story56RageRoadEventTriggerTests`, `MonoBehaviour`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `AIVehicleBehaviorDebugView`, `NetworkedRageState`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`?**
  _High betweenness centrality (0.151) - this node is a cross-community bridge._
- **Why does `LobbyFlowController` connect `LobbyFlowController` to `MonoBehaviour`, `CharacterCatalog`, `LobbyRosterScreen`, `Difficulty`, `RoadRage.Features.UI`, `Story25NetworkedPlayerSpawnTests`, `LobbyRosterSnapshot`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `OnlineServicesBootstrapService`, `LobbyRoomService`, `Story16Epic1PlayableCheckpointTests`, `Story12LobbyShellPlayModeTests`, `LobbyShellScreen`, `Story12LobbyShellTests`, `Lobby Join Service`, `DefinitionId`?**
  _High betweenness centrality (0.092) - this node is a cross-community bridge._
- **Why does `RoadRageBootstrap` connect `RoadRageBootstrap` to `FacepunchSteamPlatform`, `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests`, `Story25NetworkedPlayerSpawnTests`, `MonoBehaviour`, `RoadRage.Features.UI`, `Story11MainMenuLaunchTests`, `LobbyRosterSnapshot`, `Story58EscapeMenuTests`, `NetworkedRunSessionMonitor`, `OnlineServicesBootstrapService`, `.EnsureNetworkManager`, `UserNotice`, `LobbyRoomService`, `Lobby Join Service`, `Story11MainMenuLaunchPlayModeTests`, `PlayerProfile`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **What connects `HasReachedExitPortal`, `HasProfile`, `WheelCount` to the rest of the system?**
  _356 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.12857142857142856 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.07982583454281568 - nodes in this community are weakly interconnected._