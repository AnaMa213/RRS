# Graph Report - RRS  (2026-09-18)

## Corpus Check
- 172 files · ~148,034 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3137 nodes · 7389 edges · 149 communities (121 shown, 24 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 409 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `1b7ae528`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests
- Story13CharacterSetupTests
- Story25NetworkedPlayerSpawnTests
- GreyboxAssetSeedMetadata
- Difficulty
- Story512TireForcesAndSteeringTests
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
- Story51NpcRageFearFoundationTests
- MenuCharacterPreview
- NetworkedVehicleSeatIntent
- VehiclePhysicsBody
- NetworkedRageState
- Story512TireForcesAndSteeringPlayModeTests
- RoadRage.Features.UI
- Story27PlayerLifecycleTests
- Story12LobbyShellTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- .ResolveTarget
- .FixedUpdate
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- RoadRage.Features.Players
- NetworkedVehicleDriverController
- LobbyRosterScreen
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- PassengerActionDef
- VehicleProfile
- VehicleProfileDef
- RoadRage.Shared.Domain
- LobbyJoinOutcome
- Story26InGameHudTests
- LobbyRosterSnapshot
- .UpdateSteeringState
- FakeSteamLobbyPlatform
- Story32DriverControlAndLocalCameraTests
- .SelectWeightedSuccessor
- .RequestHonk
- RoadRageScaffoldTests.cs
- LobbyRoomService
- VehicleWheel
- Story43PassengerActionOneChangesRageTests
- Story12LobbyShellPlayModeTests
- MainMenuFlowController
- Story54RageDrivenAiBehaviorStatesTests
- FakeSteamLobbyPlatform
- Lobby Join Service
- .FindRecursive
- FakeSteamPlatform
- .TearDown
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- HostOwnedNetworkStateBehaviour
- CharacterCatalog
- PassengerActionCatalog
- NetworkedPassengerActionIntent
- Story58EscapeMenuTests
- RoadRageBootstrap
- .NewFixture
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- .ApplyAuthoritative
- PlayerProfileResolution
- Story57AiTrafficClientPresentationTests
- NetworkedVehicleState.cs
- VehicleDriveIntent
- CharacterController
- Story56RageRoadEventTriggerTests
- NetworkObjectReference
- CharacterOption
- Rpc
- RpcParams
- Vector2
- NetworkedPlayerState
- Bounds
- Object
- Renderer
- PlayerProfile
- DefinitionId
- Scene
- TearDown
- RoadRage Scaffold Structure Tests
- Func
- LocalVehicleCameraRig
- Story35VehicleDamageHookAndTeamWipeContractStubTests
- .OnDrag
- .TearDown
- Vector3
- GameObject
- Rigidbody
- Test
- MainMenuScreen
- .MvpRunShowsTopRightRageHudAndMultipleRageVehicles
- Private Room Play Mode Tests
- NetworkedPlayerLifecycleIntent
- Netcode/Steamworks Smoke Tests
- NetworkedVehicleRecoveryIntent
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FacepunchSteamPlatform
- OnFootMovementIntent
- .EnsureNetworkManager
- Story52BasicAiRouteFollowingAndRecoveryTests
- .NewTuning
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
- LobbyRosterService
- Story21OnlineServicesPlayModeTests
- LobbyShellScreen
- Lock-Rage Camera Fix Query
- RoadRage.Features.Vehicles
- MonoBehaviour
- Story33SeatEntryExitAndPassengerPresenceTests
- LaneNode
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedVehicleSeatService
- RageStateDebugView
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- PassengerActionVerdictCode
- .Create
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RunFlowController` - 118 edges
2. `Story510LaneGraphAndRoutedTrafficTests` - 77 edges
3. `NetworkedVehicleState` - 77 edges
4. `RunCheckpointHudScreen` - 69 edges
5. `NetworkedVehicleDriverController` - 64 edges
6. `RoadRage.Features.Vehicles` - 62 edges
7. `RoadRage.Shared.Domain` - 61 edges
8. `RoadRage.Features.Players` - 50 edges
9. `NetworkedAIVehicleDriverController` - 49 edges
10. `LobbyFlowController` - 48 edges

## Surprising Connections (you probably didn't know these)
- `VehiclePhysicsBody` --references--> `VehicleProfileDef`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs → Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs
- `MainMenuProfileFlowController` --references--> `PlayerProfileBootstrapService`  [EXTRACTED]
  Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs → Assets/RoadRage/Features/Players/PlayerProfileBootstrapService.cs
- `PlayerProfileBootstrapService` --references--> `CharacterCatalog`  [EXTRACTED]
  Assets/RoadRage/Features/Players/PlayerProfileBootstrapService.cs → Assets/RoadRage/Features/Players/CharacterCatalog.cs
- `RoadRageBootstrap` --references--> `PlayerProfileFileStore`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Players/PlayerProfileFileStore.cs
- `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` --references--> `CharacterCatalog`  [EXTRACTED]
  Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs → Assets/RoadRage/Features/Players/CharacterCatalog.cs

## Import Cycles
- None detected.

## Communities (149 total, 24 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.11
Nodes (14): ArgumentException, PersistentPlayerProfileRecord, PlayerProfileBootstrapService, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath, Component (+6 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (11): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, ArgumentNullException, List, Object, TearDown, Test (+3 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.06
Nodes (26): ApprovalResult, NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry, Collider (+18 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.06
Nodes (28): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+20 more)

### Community 4 - "Difficulty"
Cohesion: 0.22
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.11
Nodes (8): VehicleProfile, Story512TireForcesAndSteeringTests, GameObject, NetworkedAIVehicleDriverController, Rigidbody, Test, Vector2, VehiclePhysicsBody

### Community 6 - "Story11MainMenuLaunchTests"
Cohesion: 0.18
Nodes (9): Canvas, Component, EventSystem, InputSystemUIInputModule, Scene, SerializeField, Test, Story11MainMenuLaunchTests (+1 more)

### Community 7 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.09
Nodes (17): Action, BoxCollider, Collider, GameObject, LaneNode, NetworkedAIVehicleState, Test, Transform (+9 more)

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.07
Nodes (20): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+12 more)

### Community 10 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (17): CharacterController, Collider, GameObject, IEnumerator, List, NetworkObject, PortalTrafficSpawner, LivePopulation (+9 more)

### Community 11 - "RunFlowController"
Cohesion: 0.09
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.18
Nodes (6): GameObject, TextMeshProUGUI, TMP_Text, RunCheckpointHudScreen, RectTransform, StringBuilder

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (23): ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted (+15 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.07
Nodes (19): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, Color, Quaternion, Renderer, Transform (+11 more)

### Community 15 - "LobbyFlowController"
Cohesion: 0.15
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 16 - "LocalOnFootController"
Cohesion: 0.13
Nodes (13): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Vector2, LocalOnFootController, IsDowned, MovementEnabled (+5 more)

### Community 17 - "UserNotice"
Cohesion: 0.18
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 18 - "Story51NpcRageFearFoundationTests"
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, NetworkObject, Object, Test (+2 more)

### Community 19 - "MenuCharacterPreview"
Cohesion: 0.21
Nodes (10): Camera, Color, GameObject, RawImage, Renderer, Transform, MenuCharacterPreview, IDragHandler (+2 more)

### Community 20 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.10
Nodes (14): Vector3, VehiclePhysicsTelemetryView, RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees (+6 more)

### Community 22 - "NetworkedRageState"
Cohesion: 0.09
Nodes (21): AiVehicleFixture, NetworkVariable, NetworkedRageState, CurrentDisposition, NetworkVariable, NetworkedAIVehicleState, IRageDispositionSource, CurrentDisposition (+13 more)

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.25
Nodes (9): BoxCollider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest, Vector3 (+1 more)

### Community 24 - "RoadRage.Features.UI"
Cohesion: 0.19
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.07
Nodes (13): NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed, CharacterController (+5 more)

### Community 26 - "Story12LobbyShellTests"
Cohesion: 0.12
Nodes (12): Difficulty, MatchSettings, Difficulty, Color, Component, GameObject, Image, Scene (+4 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - ".ResolveTarget"
Cohesion: 0.47
Nodes (3): IReadOnlyList, Vector3, AiRageTargetResolution

### Community 31 - ".FixedUpdate"
Cohesion: 0.10
Nodes (16): WheelState, Compression, ContactPoint, Grounded, HubPosition, TireSample, TireSample, Adherence (+8 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.23
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "RoadRage.Features.Players"
Cohesion: 0.13
Nodes (4): RoadRage.Features.Players, RoadRage.Shared.Authoring, RoadRage.Shared.Input, RoadRage.Shared.Definitions

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.11
Nodes (12): Action, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController (+4 more)

### Community 36 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.20
Nodes (10): Component, Dictionary, IEnumerator, IReadOnlyList, List, Rigidbody, UnityTearDown, UnityTest (+2 more)

### Community 37 - "PassengerActionDef"
Cohesion: 0.15
Nodes (11): Transform, PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot (+3 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (34): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+26 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.18
Nodes (9): VehicleProfile, VehicleProfileDef, Id, Profile, RawId, DefinitionId, SerializedProperty, Vector3 (+1 more)

### Community 40 - "RoadRage.Shared.Domain"
Cohesion: 0.11
Nodes (6): RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.App.Run, RoadRage.Features.Rage, RoadRage.Features.PassengerActions, RoadRage.Shared.Networking

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.08
Nodes (26): LobbyJoinFailureReason, Expired, Failed, Full, None, LobbyJoinOutcome, LobbyId, Reason (+18 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 44 - "LobbyRosterSnapshot"
Cohesion: 0.14
Nodes (17): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+9 more)

### Community 46 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): Difficulty, Task, FakeSteamLobbyPlatform, GetRosterSnapshotCallCount, LastDifficulty, LastLaunchRequested, LastProfileCharacterId, LastProfileDisplayName (+8 more)

### Community 47 - "Story32DriverControlAndLocalCameraTests"
Cohesion: 0.15
Nodes (11): AssemblyDefinition, BoxCollider, CinemachineCamera, GameObject, NetworkBehaviour, NetworkTransform, Rigidbody, Test (+3 more)

### Community 48 - ".SelectWeightedSuccessor"
Cohesion: 0.15
Nodes (4): IReadOnlyList, Vector3, LaneGraphRouting, Vector3

### Community 50 - ".RequestHonk"
Cohesion: 0.38
Nodes (3): NetworkedAIVehicleState, NetworkObjectReference, Rpc

### Community 51 - "RoadRageScaffoldTests.cs"
Cohesion: 0.29
Nodes (4): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy, RoadRage.Features.Boss

### Community 52 - "LobbyRoomService"
Cohesion: 0.13
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 53 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 54 - "Story43PassengerActionOneChangesRageTests"
Cohesion: 0.14
Nodes (13): Fixture, Func, GameObject, List, NetworkObject, Object, TearDown, Test (+5 more)

### Community 55 - "Story12LobbyShellPlayModeTests"
Cohesion: 0.37
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story12LobbyShellPlayModeTests

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.17
Nodes (6): List, NetworkObject, Object, TearDown, Test, Story54RageDrivenAiBehaviorStatesTests

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.10
Nodes (9): Difficulty, FakeSteamLobbyPlatform, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster, FakeSteamPlatform, IsLoggedOn (+1 more)

### Community 59 - "Lobby Join Service"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 61 - "FakeSteamPlatform"
Cohesion: 0.25
Nodes (4): ArgumentNullException, FakeSteamPlatform, IsLoggedOn, IsValid

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.15
Nodes (8): GameObject, List, NetworkObject, Object, Test, Vector3, Story55NetworkedAiRageTargetingTests, IntentFixture

### Community 65 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.22
Nodes (7): NetworkVariable, NetworkedBossState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority, NetworkBehaviour

### Community 66 - "CharacterCatalog"
Cohesion: 0.20
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 67 - "PassengerActionCatalog"
Cohesion: 0.40
Nodes (4): List, PassengerActionCatalog, Count, Version

### Community 68 - "NetworkedPassengerActionIntent"
Cohesion: 0.15
Nodes (13): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, Action, PassengerActionDebugView (+5 more)

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

### Community 75 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 76 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 77 - ".ApplyAuthoritative"
Cohesion: 0.11
Nodes (13): Rpc, RpcParams, FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, PassengerActionValidation, PassengerActionValidationContext, PassengerActionVerdict (+5 more)

### Community 78 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.17
Nodes (8): GameObject, List, NetworkObject, Object, TearDown, Test, Type, Story57AiTrafficClientPresentationTests

### Community 80 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 81 - "VehicleDriveIntent"
Cohesion: 0.14
Nodes (7): VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle, RpcParams

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.05
Nodes (29): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+21 more)

### Community 85 - "CharacterOption"
Cohesion: 0.67
Nodes (3): CharacterOption, Primary, Secondary

### Community 89 - "NetworkedPlayerState"
Cohesion: 0.20
Nodes (9): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart, FixedString32Bytes, NetworkVariable, Vector3 (+1 more)

### Community 93 - "PlayerProfile"
Cohesion: 0.17
Nodes (11): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+3 more)

### Community 94 - "DefinitionId"
Cohesion: 0.14
Nodes (12): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+4 more)

### Community 97 - "RoadRage Scaffold Structure Tests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.08
Nodes (24): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+16 more)

### Community 100 - "Story35VehicleDamageHookAndTeamWipeContractStubTests"
Cohesion: 0.15
Nodes (7): AssemblyDefinition, CharacterController, GameObject, NetworkObject, Test, AssemblyDefinition, Story35VehicleDamageHookAndTeamWipeContractStubTests

### Community 107 - "MainMenuScreen"
Cohesion: 0.18
Nodes (6): Button, Color, GameObject, TMP_Text, MainMenuScreen, CharacterOption

### Community 110 - ".MvpRunShowsTopRightRageHudAndMultipleRageVehicles"
Cohesion: 0.48
Nodes (3): IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

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
Cohesion: 0.29
Nodes (6): Vector2, OnFootMovementIntent, IsIdle, Look, Move, SprintRequested

### Community 144 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 153 - "PlayerMode"
Cohesion: 0.22
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.09
Nodes (17): Color, HashSet, IReadOnlyList, List, Quaternion, LaneGraph, EntryPortals, ExitPortals (+9 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.06
Nodes (20): DriverModel, DriverProfile, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold, MaxAcceleration (+12 more)

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FacepunchSteamLobbyPlatform, Lobby, RoomEnter

### Community 266 - "NetworkedPlayerSpawnService"
Cohesion: 0.18
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
Nodes (19): Vector3, BoxCollider, Collider, IReadOnlyList, LaneGraph, List, NetworkedAIVehicleState, NetworkTransform (+11 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.17
Nodes (7): List, NetworkObject, Object, TearDown, Test, TestCase, Story41RageStateModuleAndDefinitionsTests

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.19
Nodes (7): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Quaternion, Vector3

### Community 645 - "RageSandboxAutoStart"
Cohesion: 0.26
Nodes (6): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, RageSandboxAutoStart

### Community 647 - "LobbyRosterService"
Cohesion: 0.11
Nodes (9): Difficulty, Task, ISteamLobbyPlatform, Difficulty, LobbyRosterService, AllMembersReady, Current, IsSynchronized (+1 more)

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

### Community 697 - "LobbyShellScreen"
Cohesion: 0.22
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

### Community 722 - "RoadRage.Features.Vehicles"
Cohesion: 0.09
Nodes (4): RoadRage.DevTools, RoadRage.Tests.EditMode, RoadRage.Features.OnFoot, RoadRage.Features.Vehicles

### Community 736 - "MonoBehaviour"
Cohesion: 0.20
Nodes (6): Color, Image, TMP_Text, LobbyPlayerSlotView, DevIndestructibleVehicle, MonoBehaviour

### Community 780 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.23
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 824 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 854 - ".AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile"
Cohesion: 0.29
Nodes (6): Component, IEnumerator, Rigidbody, UnityTearDown, UnityTest, Story59ParameterizedDriverModelPlayModeTests

### Community 857 - "NetworkedVehicleSeatService"
Cohesion: 0.18
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 858 - "RageStateDebugView"
Cohesion: 0.15
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

### Community 874 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **382 isolated node(s):** `Id`, `RawId`, `Profile`, `AsmdefManifest`, `AssemblyDefinition` (+377 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 828 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **24 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RunFlowController` connect `RunFlowController` to `GreyboxAssetSeedMetadata`, `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `NetworkedVehicleDamageVfxController`, `LocalOnFootController`, `NetworkedRageState`, `.RefreshHudFromLocalNetworkedPlayerState`, `NetworkedVehicleDriverController`, `RoadRage.Shared.Domain`, `Story26InGameHudTests`, `.ShowVehicleSeatMessage`, `.Update`, `Story43PassengerActionOneChangesRageTests`, `Story55NetworkedAiRageTargetingTests`, `CharacterCatalog`, `PassengerActionCatalog`, `NetworkedPassengerActionIntent`, `.ApplySecondaryVehicleCollisionDamage`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `AIVehicleBehaviorDebugView`, `Story56RageRoadEventTriggerTests`, `NetworkedPlayerState`, `MonoBehaviour`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Empty Map Entry Playmode Tests`, `.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`?**
  _High betweenness centrality (0.152) - this node is a cross-community bridge._
- **Why does `VehiclePhysicsBody` connect `VehiclePhysicsBody` to `MonoBehaviour`, `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, `NetworkedVehicleDriverController`, `NetworkedAIVehicleDriverController`, `VehicleProfile`, `VehicleProfileDef`, `Story511VehicleChassisWheelsAndSuspensionTests`, `.UpdateSteeringState`, `VehicleDriveIntent`, `Story512TireForcesAndSteeringPlayModeTests`, `.FixedUpdate`?**
  _High betweenness centrality (0.112) - this node is a cross-community bridge._
- **Why does `RoadRage.Shared.Domain` connect `RoadRage.Shared.Domain` to `RoadRage.Features.Players`, `PlayerMode`, `Difficulty`, `PassengerActionDef`, `LobbyJoinOutcome`, `.ApplyAuthoritative`, `RoadRage.Features.Vehicles`, `Story56RageRoadEventTriggerTests`, `NetworkedRageState`, `RoadRage.Features.UI`, `Story27PlayerLifecycleTests`, `RageStateDebugView`?**
  _High betweenness centrality (0.096) - this node is a cross-community bridge._
- **What connects `Id`, `RawId`, `Profile` to the rest of the system?**
  _382 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.10631229235880399 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.08941176470588236 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.05707762557077625 - nodes in this community are weakly interconnected._