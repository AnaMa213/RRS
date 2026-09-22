# Graph Report - RRS  (2026-09-22)

## Corpus Check
- 198 files · ~202,742 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3864 nodes · 9350 edges · 174 communities (161 shown, 12 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 437 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `ee7ffbed`
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
- PlayerProfileFileStore
- Story512TireForcesAndSteeringPlayModeTests
- Story510LaneGraphAndRoutedTrafficTests
- Story27PlayerLifecycleTests
- RoadId
- Story513ArcadeAssistsAndUnevenGroundTests
- Story11MainMenuLaunchPlayModeTests
- LobbyCodeClipboard
- RoadRage.Features.Vehicles
- VehicleSuspensionModel
- Story511VehicleChassisWheelsAndSuspensionPlayModeTests
- Story513ArcadeAssistsAndUnevenGroundPlayModeTests
- NetworkedVehicleDriverController
- TrafficOracleTests
- PhysicsHarness
- Story58EscapeMenuPlayModeTests
- VehicleProfile
- VehicleProfileDef
- RoadRage.Features.UI
- LobbyJoinOutcome
- Story26InGameHudTests
- List
- LobbyCreateOutcome
- DefinitionId
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
- FakeSteamLobbyPlatform
- LobbyJoinService
- NetworkedAIVehicleState
- Story525RoadWorldModelTests
- Story34SimpleRouteCollisionAndVehicleRecoveryTests
- FakeSteamLobbyPlatform
- Story55NetworkedAiRageTargetingTests
- .Author
- CharacterCatalog
- RoadModelSource
- LobbyRosterSnapshot
- RunEscapeMenuScreen
- RoadRageBootstrap
- .NewFixture
- .Inspect
- RageTuningDef
- Story15EmptyMapEntryTests
- AIVehicleBehaviorDebugView
- NetworkedRunSessionMonitor
- NetworkedPassengerActionIntent
- Story28Epic2OnlinePlayableCheckpointTests
- Story57AiTrafficClientPresentationTests
- .MenuResolvesProfileAndPublishesTheChosenCharacter
- NetworkedVehicleState.cs
- NetworkedVehicleSeatIntent
- Story56RageRoadEventTriggerTests
- RoadRage.Shared.Domain
- Story33SeatEntryExitAndPassengerPresenceTests
- .TearDown
- RoadModelVersion
- LobbyRoomService
- .EnsureVehicleSandboxSeatHarness
- .FixedUpdate
- DriverProfile
- NetworkedPlayerLifecycleIntent
- PlayerProfile
- Story36Epic3DrivingPlayableCheckpointTests
- RoadRecordKind
- PassengerActionVerdictCode
- RoadRageScaffoldTests
- Story58EscapeMenuTests
- LocalVehicleCameraRig
- Story16Epic1PlayableCheckpointTests
- .Inspect
- NetworkedPlayerSpawnService
- OracleEvidenceClassification
- GreyboxAssetSeedMetadata
- PlayerMode
- FakeSteamPlatform
- MainMenuScreen
- Story11MainMenuLaunchTests
- .Find
- DriverModel
- Private Room Play Mode Tests
- RoadModelCanonicalWriter
- NetworkPlayerRegistry
- Netcode/Steamworks Smoke Tests
- VehicleDriveIntent
- NetworkedPlayerReviveIntent
- .NewTuning
- .Append
- NetworkedVehicleRecoveryIntent
- RoadRage.App.Run
- .TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals
- RageRoadEventState
- .HandleLifecycleChanged
- HostOwnedNetworkStateBehaviour
- FacepunchSteamPlatform
- RageTuningCatalog
- DriverProfileDef
- NetworkedPlayerState
- ShapeGuardRegister.cs
- MainMenuFlowController
- Story516ForceAssetRefresh
- CompiledRoadModel
- ISteamPlatform
- .Run
- RageDisposition
- RoadModelValidationCode
- .Create
- Difficulty
- RunEscapeMenuFlowController
- OnlineServicesStatus
- .EnsureNetworkManagerRepairsExistingSingletonWithoutNetworkConfig
- FakeSteamPlatform
- TireSample
- AppSceneRouter.cs
- RunCompositionRoot
- .UpdateSteeringState
- .EnsureNetworkManager
- SeedExpectation
- .TearDown
- Action
- Test
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
- NetworkedRageState
- Story21OnlineServicesPlayModeTests
- Lock-Rage Camera Fix Query
- MonoBehaviour
- LaneNode
- .AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile
- NetworkedVehicleSeatService
- ReactionChannel
- .FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable
- .GreyboxPlayerCarHasNetworkIdentityAndLongitudinalColliderAndArt
- .MvpRunProvidesOfflineAndNetworkPassengerActionWiring

## God Nodes (most connected - your core abstractions)
1. `RunFlowController` - 118 edges
2. `RoadId` - 82 edges
3. `NetworkedVehicleState` - 80 edges
4. `Story510LaneGraphAndRoutedTrafficTests` - 79 edges
5. `RoadRage.Features.Vehicles` - 75 edges
6. `Story525RoadWorldModelTests` - 72 edges
7. `RunCheckpointHudScreen` - 69 edges
8. `RoadRage.Shared.Domain` - 66 edges
9. `NetworkedVehicleDriverController` - 65 edges
10. `LobbyFlowController` - 63 edges

## Surprising Connections (you probably didn't know these)
- `EffectiveLaneCorridor` --references--> `RoadId`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs
- `RoadModelCanonicalPayload` --references--> `EffectiveLaneCorridor`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCanonicalWriter.cs → Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs
- `CompiledRoadModel` --references--> `JunctionControl`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs
- `CompiledRoadModel` --references--> `JunctionMovement`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs
- `CompiledRoadModel` --references--> `LaneAdjacency`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs

## Import Cycles
- None detected.

## Communities (174 total, 12 thin omitted)

### Community 0 - "Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests"
Cohesion: 0.13
Nodes (10): ArgumentException, Component, List, Object, RawImage, Scene, TearDown, Test (+2 more)

### Community 1 - "Story13CharacterSetupTests"
Cohesion: 0.09
Nodes (10): AsmdefManifest, AssemblyDefinitionAsset, PlayerNameValidator, List, Object, TearDown, Test, TestCase (+2 more)

### Community 2 - "Story25NetworkedPlayerSpawnTests"
Cohesion: 0.13
Nodes (8): Collider, GameObject, NetworkObject, Renderer, Test, TextMeshProUGUI, Type, Story25NetworkedPlayerSpawnTests

### Community 3 - "Story14GreyboxAssetSeedTests"
Cohesion: 0.17
Nodes (10): Bounds, Collider, GameObject, NetworkObject, Renderer, Test, Transform, Story14GreyboxAssetSeedTests (+2 more)

### Community 4 - "Story516LobbyConfigurableTrafficSettingsTests"
Cohesion: 0.10
Nodes (7): Button, FakeSteamLobbyPlatform, MonoBehaviour, NetworkObject, TearDown, Test, Story516LobbyConfigurableTrafficSettingsTests

### Community 5 - "Story512TireForcesAndSteeringTests"
Cohesion: 0.12
Nodes (4): GameObject, Rigidbody, Test, Story512TireForcesAndSteeringTests

### Community 6 - "UserNotice"
Cohesion: 0.27
Nodes (7): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning

### Community 8 - "NetworkedVehicleState"
Cohesion: 0.10
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 9 - "Story511VehicleChassisWheelsAndSuspensionTests"
Cohesion: 0.12
Nodes (9): Collider, Func, GameObject, List, Rigidbody, Test, Vector3, Story511VehicleChassisWheelsAndSuspensionTests (+1 more)

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
Cohesion: 0.18
Nodes (6): OnlineServicesBootstrapService, Status, ArgumentNullException, Test, Story21OnlineServicesBootstrapTests, FakeSteamLobbyPlatform

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.07
Nodes (16): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, AssemblyDefinition, CharacterController (+8 more)

### Community 15 - "LobbyFlowController"
Cohesion: 0.11
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

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

### Community 20 - "RoadModelRecords.cs"
Cohesion: 0.07
Nodes (37): Vector3, ConflictZone, ImportManifest, ImportManifestEntry, Junction, JunctionControl, JunctionControlKind, Priority (+29 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.10
Nodes (15): Vector3, VehiclePhysicsTelemetryView, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor (+7 more)

### Community 22 - "PlayerProfileFileStore"
Cohesion: 0.14
Nodes (12): PersistentPlayerProfileRecord, PlayerProfileBootstrapService, PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist, Exception (+4 more)

### Community 23 - "Story512TireForcesAndSteeringPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 24 - "Story510LaneGraphAndRoutedTrafficTests"
Cohesion: 0.11
Nodes (9): BoxCollider, Collider, GameObject, Object, Renderer, TearDown, Test, Vector3 (+1 more)

### Community 25 - "Story27PlayerLifecycleTests"
Cohesion: 0.07
Nodes (13): NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed, CharacterController (+5 more)

### Community 26 - "RoadId"
Cohesion: 0.10
Nodes (16): JunctionMovement, RoadId, High, IsEmpty, Low, SignalGroup, SignalGroupState, SignalPhase (+8 more)

### Community 27 - "Story513ArcadeAssistsAndUnevenGroundTests"
Cohesion: 0.08
Nodes (15): VehicleArcadeAssist, Action, Bounds, BoxCollider, Collider, Component, GameObject, Scene (+7 more)

### Community 28 - "Story11MainMenuLaunchPlayModeTests"
Cohesion: 0.36
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story11MainMenuLaunchPlayModeTests

### Community 29 - "LobbyCodeClipboard"
Cohesion: 0.15
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 30 - "RoadRage.Features.Vehicles"
Cohesion: 0.10
Nodes (4): RoadRage.DevTools, RoadRage.Tests.EditMode, RoadRage.Features.Rage, RoadRage.Features.Vehicles

### Community 31 - "VehicleSuspensionModel"
Cohesion: 0.08
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 32 - "Story511VehicleChassisWheelsAndSuspensionPlayModeTests"
Cohesion: 0.22
Nodes (10): BoxCollider, Collider, GameObject, IEnumerator, Rigidbody, UnitySetUp, UnityTearDown, UnityTest (+2 more)

### Community 33 - "Story513ArcadeAssistsAndUnevenGroundPlayModeTests"
Cohesion: 0.21
Nodes (11): BoxCollider, GameObject, IEnumerator, List, Rigidbody, Scene, UnitySetUp, UnityTearDown (+3 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.10
Nodes (11): Action, Collider, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc, Transform (+3 more)

### Community 35 - "TrafficOracleTests"
Cohesion: 0.07
Nodes (26): Action, Scene, Test, Vector3, TrafficOracleTests, List, TraceDivergence, TraceDivergenceField (+18 more)

### Community 36 - "PhysicsHarness"
Cohesion: 0.12
Nodes (19): Collider, GameObject, List, MonoBehaviour, Rigidbody, Scene, Test, Vector3 (+11 more)

### Community 37 - "Story58EscapeMenuPlayModeTests"
Cohesion: 0.33
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story58EscapeMenuPlayModeTests

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "VehicleProfileDef"
Cohesion: 0.20
Nodes (7): Vector3, VehicleProfileDef, Id, Profile, RawId, SerializedObject, SerializedProperty

### Community 40 - "RoadRage.Features.UI"
Cohesion: 0.21
Nodes (9): RoadRage.App.Services, RoadRage.App, RoadRage.Features.UI, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.App.MainMenu, RoadRage.Features.Lobby, RoadRage.Shared.Presentation (+1 more)

### Community 41 - "LobbyJoinOutcome"
Cohesion: 0.08
Nodes (21): LobbyJoinOutcome, LobbyId, Reason, Success, ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform (+13 more)

### Community 42 - "Story26InGameHudTests"
Cohesion: 0.23
Nodes (5): GameObject, Test, TextMeshProUGUI, Type, Story26InGameHudTests

### Community 43 - "List"
Cohesion: 0.16
Nodes (8): Action, Bounds, List, Scene, Transform, CurbTrafficMeasurement, ReplayTrace, CurbTrafficMeasurement

### Community 44 - "LobbyCreateOutcome"
Cohesion: 0.13
Nodes (15): LobbyCreateOutcome, LobbyId, Success, LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId (+7 more)

### Community 45 - "DefinitionId"
Cohesion: 0.16
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

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
Cohesion: 0.18
Nodes (4): Button, TMP_InputField, TMP_Text, LobbyShellScreen

### Community 50 - "RoadRage.Features.Players"
Cohesion: 0.16
Nodes (3): RoadRage.Features.Players, RoadRage.Shared.Authoring, RoadRage.Shared.Definitions

### Community 51 - ".SelectWeightedSuccessor"
Cohesion: 0.22
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
Cohesion: 0.20
Nodes (6): GameObject, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState

### Community 57 - "Story54RageDrivenAiBehaviorStatesTests"
Cohesion: 0.13
Nodes (11): AiVehicleFixture, IRageDispositionSource, CurrentDisposition, GameObject, List, NetworkObject, Object, TearDown (+3 more)

### Community 58 - "FakeSteamLobbyPlatform"
Cohesion: 0.17
Nodes (5): Difficulty, Task, FakeSteamLobbyPlatform, NextCreateOutcome, NextRoster

### Community 59 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 60 - "NetworkedAIVehicleState"
Cohesion: 0.23
Nodes (8): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, IEnumerator, UnityTest, Story43PassengerActionOneMvpRunPlayModeTests

### Community 61 - "Story525RoadWorldModelTests"
Cohesion: 0.12
Nodes (5): Action, Vector3, Story525RoadWorldModelTests, DeltaMutation, Test

### Community 62 - "Story34SimpleRouteCollisionAndVehicleRecoveryTests"
Cohesion: 0.21
Nodes (5): AssemblyDefinition, GameObject, Test, AssemblyDefinition, Story34SimpleRouteCollisionAndVehicleRecoveryTests

### Community 63 - "FakeSteamLobbyPlatform"
Cohesion: 0.09
Nodes (16): ArgumentNullException, Difficulty, Exception, FakeSteamLobbyPlatform, Task, Test, FakeSteamLobbyPlatform, CreateLobbyCallCount (+8 more)

### Community 64 - "Story55NetworkedAiRageTargetingTests"
Cohesion: 0.14
Nodes (9): GameObject, List, NetworkObject, Object, TearDown, Test, Vector3, Story55NetworkedAiRageTargetingTests (+1 more)

### Community 65 - ".Author"
Cohesion: 0.28
Nodes (7): Bounds, Collider, GameObject, List, Transform, Vector3, Story513RecipeRelief

### Community 66 - "CharacterCatalog"
Cohesion: 0.20
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 67 - "RoadModelSource"
Cohesion: 0.09
Nodes (17): RoadModelCanonicalPayload, Comparison, RoadModelCompiler, LaneAdjacency, LaneChangePermission, Allowed, Forbidden, LaneSide (+9 more)

### Community 68 - "LobbyRosterSnapshot"
Cohesion: 0.11
Nodes (13): LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId, RunLaunchRequested (+5 more)

### Community 69 - "RunEscapeMenuScreen"
Cohesion: 0.13
Nodes (10): Button, RunEscapeMenuScreen, IsOpen, Canvas, EventSystem, Image, InputSystemUIInputModule, Scene (+2 more)

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

### Community 77 - "NetworkedPassengerActionIntent"
Cohesion: 0.11
Nodes (17): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, List, PassengerActionCatalog (+9 more)

### Community 78 - "Story28Epic2OnlinePlayableCheckpointTests"
Cohesion: 0.16
Nodes (10): ApprovalResult, NetworkPlayerConnectionPayload, GameObject, NetworkObject, Test, ApprovalResult, Approved, Profile (+2 more)

### Community 79 - "Story57AiTrafficClientPresentationTests"
Cohesion: 0.16
Nodes (9): GameObject, List, NetworkObject, NetworkTransform, Object, TearDown, Test, Type (+1 more)

### Community 80 - ".MenuResolvesProfileAndPublishesTheChosenCharacter"
Cohesion: 0.19
Nodes (8): PointerEventData, Component, IEnumerator, RawImage, TMP_InputField, UnityTearDown, UnityTest, Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests

### Community 81 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 82 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 83 - "Story56RageRoadEventTriggerTests"
Cohesion: 0.15
Nodes (8): IReadOnlyList, GameObject, List, NetworkObject, Object, Test, TMP_Text, Story56RageRoadEventTriggerTests

### Community 84 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (8): NetworkVariable, NetworkedCrewEconomyState, SessionTrafficValue, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.Features.Economy, RoadRage.Shared.Networking, RoadRage.Features.Boss

### Community 85 - "Story33SeatEntryExitAndPassengerPresenceTests"
Cohesion: 0.24
Nodes (6): AssemblyDefinition, GameObject, NetworkObject, Test, AssemblyDefinition, Story33SeatEntryExitAndPassengerPresenceTests

### Community 87 - "RoadModelVersion"
Cohesion: 0.08
Nodes (20): EffectiveLaneCorridor, RoadModelVersion, High, IsEmpty, Low, SchemaVersion, LaneCorridor, RoadCurveSample (+12 more)

### Community 88 - "LobbyRoomService"
Cohesion: 0.09
Nodes (14): Difficulty, Task, ISteamLobbyPlatform, Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status (+6 more)

### Community 89 - ".EnsureVehicleSandboxSeatHarness"
Cohesion: 0.29
Nodes (5): GameObject, IEnumerator, NetworkManager, NetworkObject, RoadRageNetcodeSmokeTestAutoStart

### Community 90 - ".FixedUpdate"
Cohesion: 0.18
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 91 - "DriverProfile"
Cohesion: 0.12
Nodes (13): DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold, MaxAcceleration (+5 more)

### Community 92 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 93 - "PlayerProfile"
Cohesion: 0.15
Nodes (12): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection (+4 more)

### Community 94 - "Story36Epic3DrivingPlayableCheckpointTests"
Cohesion: 0.19
Nodes (8): AssemblyDefinition, Component, GameObject, NetworkObject, Scene, Test, AssemblyDefinition, Story36Epic3DrivingPlayableCheckpointTests

### Community 95 - "RoadRecordKind"
Cohesion: 0.16
Nodes (19): RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 96 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): Rpc, RpcParams, FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, PassengerActionValidation, PassengerActionVerdict, Accepted (+19 more)

### Community 97 - "RoadRageScaffoldTests"
Cohesion: 0.22
Nodes (6): AssemblyDefinition, NetworkObject, Test, Type, AssemblyDefinition, RoadRageScaffoldTests

### Community 98 - "Story58EscapeMenuTests"
Cohesion: 0.21
Nodes (4): List, Object, Test, Story58EscapeMenuTests

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.05
Nodes (35): CinemachineCamera, CinemachineInputAxisController, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig (+27 more)

### Community 100 - "Story16Epic1PlayableCheckpointTests"
Cohesion: 0.24
Nodes (5): Component, Scene, Test, Transform, Story16Epic1PlayableCheckpointTests

### Community 101 - ".Inspect"
Cohesion: 0.38
Nodes (4): Collider, Component, Transform, Story513RageTargetInspection

### Community 102 - "NetworkedPlayerSpawnService"
Cohesion: 0.17
Nodes (9): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+1 more)

### Community 103 - "OracleEvidenceClassification"
Cohesion: 0.18
Nodes (13): IReadOnlyList, BoundTest, OracleCatalog, OracleEvidenceClassification, AutoEdit, AutoPlay, Gap, Manual (+5 more)

### Community 104 - "GreyboxAssetSeedMetadata"
Cohesion: 0.16
Nodes (10): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+2 more)

### Community 105 - "PlayerMode"
Cohesion: 0.18
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 106 - "FakeSteamPlatform"
Cohesion: 0.17
Nodes (9): Exception, FakeSteamPlatform, InitCallCount, InitCalledWithAppId, IsLoggedOn, IsValid, RunCallbacksCallCount, ShutdownCallCount (+1 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 108 - "Story11MainMenuLaunchTests"
Cohesion: 0.13
Nodes (11): UserNoticeChannel, LastNotice, Canvas, CanvasScaler, Component, EventSystem, InputSystemUIInputModule, Scene (+3 more)

### Community 111 - "Private Room Play Mode Tests"
Cohesion: 0.41
Nodes (5): Component, IEnumerator, UnityTearDown, UnityTest, Story22HostCreatedPrivateRoomPlayModeTests

### Community 112 - "RoadModelCanonicalWriter"
Cohesion: 0.31
Nodes (4): Comparison, Vector3, RoadModelCanonicalWriter, BinaryWriter

### Community 113 - "NetworkPlayerRegistry"
Cohesion: 0.26
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 114 - "Netcode/Steamworks Smoke Tests"
Cohesion: 0.27
Nodes (5): MenuItem, RoadRageNetcodeSmokeTest, MenuItem, RoadRageSteamworksSmokeTest, RoadRage.Editor

### Community 115 - "VehicleDriveIntent"
Cohesion: 0.17
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 116 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 118 - ".Append"
Cohesion: 0.23
Nodes (8): Canvas, CanvasScaler, Component, StringBuilder, TMP_Text, Transform, Vector2, Story516LobbyPanelInspection

### Community 119 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 120 - "RoadRage.App.Run"
Cohesion: 0.15
Nodes (4): RoadRage.Features.OnFoot, RoadRage.App.Run, RoadRage.Features.PassengerActions, RoadRage.Shared.Input

### Community 121 - ".TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"
Cohesion: 0.22
Nodes (9): Component, Dictionary, IEnumerator, List, Rigidbody, UnityTearDown, UnityTest, Vector3 (+1 more)

### Community 123 - "RageRoadEventState"
Cohesion: 0.15
Nodes (8): IReadOnlyList, RageRoadEventLifecycle, RageRoadEventState, Confrontation, Idle, Resolved, RewardGranted, Triggered

### Community 125 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.22
Nodes (7): NetworkVariable, NetworkedBossState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority, NetworkBehaviour

### Community 126 - "FacepunchSteamPlatform"
Cohesion: 0.20
Nodes (4): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource

### Community 127 - "RageTuningCatalog"
Cohesion: 0.22
Nodes (4): List, RageTuningCatalog, Count, ScriptableObject

### Community 128 - "DriverProfileDef"
Cohesion: 0.27
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 129 - "NetworkedPlayerState"
Cohesion: 0.33
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 130 - "ShapeGuardRegister.cs"
Cohesion: 0.67
Nodes (3): IReadOnlyList, ShapeGuardEntry, ShapeGuardRegister

### Community 133 - "CompiledRoadModel"
Cohesion: 0.12
Nodes (17): Dictionary, IReadOnlyList, List, CompiledRoadModel, Adjacencies, ConflictZones, Connections, Controls (+9 more)

### Community 134 - "ISteamPlatform"
Cohesion: 0.14
Nodes (6): ISteamPlatform, IsLoggedOn, IsValid, FakeSteamPlatform, IsLoggedOn, IsValid

### Community 135 - ".Run"
Cohesion: 0.27
Nodes (10): Button, GameObject, Object, Scene, SerializedObject, StringBuilder, TMP_Text, Transform (+2 more)

### Community 136 - "RageDisposition"
Cohesion: 0.22
Nodes (7): RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated, Ram

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.11
Nodes (19): RoadModelValidationCode, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, CrossVersionReference, DuplicateId, DuplicateLateralOrder, EmptyId, IncompatibleManifestRemap (+11 more)

### Community 138 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 139 - "Difficulty"
Cohesion: 0.18
Nodes (6): Difficulty, Difficulty, Difficulty, Easy, Hard, Normal

### Community 140 - "RunEscapeMenuFlowController"
Cohesion: 0.15
Nodes (6): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, TearDown, CursorLockMode

### Community 141 - "OnlineServicesStatus"
Cohesion: 0.25
Nodes (6): OnlineServicesStatus, InitializationFailed, NotStarted, Offline, Online, SignInFailed

### Community 142 - ".EnsureNetworkManagerRepairsExistingSingletonWithoutNetworkConfig"
Cohesion: 0.43
Nodes (3): NetworkManager, NetworkPrefabsList, TearDown

### Community 143 - "FakeSteamPlatform"
Cohesion: 0.33
Nodes (3): FakeSteamPlatform, IsLoggedOn, IsValid

### Community 144 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 145 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 146 - "RunCompositionRoot"
Cohesion: 0.40
Nodes (4): Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot

### Community 148 - ".EnsureNetworkManager"
Cohesion: 0.53
Nodes (4): GameObject, NetworkManager, NetworkPrefabsList, FacepunchTransport

### Community 149 - "SeedExpectation"
Cohesion: 0.67
Nodes (3): Type, Vector3, SeedExpectation

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.15
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.11
Nodes (14): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+6 more)

### Community 249 - "Story59ParameterizedDriverModelTests"
Cohesion: 0.13
Nodes (3): GameObject, Test, Story59ParameterizedDriverModelTests

### Community 254 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.10
Nodes (10): Difficulty, Task, FacepunchSteamLobbyPlatform, LobbyJoinFailureReason, Expired, Failed, Full, None (+2 more)

### Community 267 - ".BootstrapToWorldCompletesEpic1PlayableCheckpoint"
Cohesion: 0.27
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story16Epic1PlayableCheckpointPlayModeTests

### Community 312 - "PassengerActionDef"
Cohesion: 0.11
Nodes (18): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+10 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.07
Nodes (15): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+7 more)

### Community 614 - "Empty Map Entry Playmode Tests"
Cohesion: 0.31
Nodes (6): CharacterController, Component, IEnumerator, UnityTearDown, UnityTest, Story15EmptyMapEntryPlayModeTests

### Community 628 - "Story41RageStateModuleAndDefinitionsTests"
Cohesion: 0.16
Nodes (7): List, NetworkObject, Object, TearDown, Test, TestCase, Story41RageStateModuleAndDefinitionsTests

### Community 640 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 645 - "NetworkedRageState"
Cohesion: 0.15
Nodes (13): Canvas, GameObject, IEnumerator, NetworkManager, NetworkObject, Transform, RageSandboxAutoStart, NetworkVariable (+5 more)

### Community 648 - "Story21OnlineServicesPlayModeTests"
Cohesion: 0.43
Nodes (4): IEnumerator, UnityTearDown, UnityTest, Story21OnlineServicesPlayModeTests

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
Cohesion: 0.20
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

### Community 881 - ".MvpRunProvidesOfflineAndNetworkPassengerActionWiring"
Cohesion: 0.33
Nodes (5): GameObject, IEnumerator, UnityTearDown, UnityTest, Story42PassengerActionMvpRunPlayModeTests

## Knowledge Gaps
- **509 isolated node(s):** `SchemaVersion`, `High`, `Low`, `IsEmpty`, `ModelId` (+504 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1009 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **12 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RoadRage.Tests.EditMode` connect `RoadRage.Features.Vehicles` to `ShapeGuardRegister.cs`, `RoadModelSource`, `TrafficOracleTests`, `OracleEvidenceClassification`, `RoadRage.Features.UI`, `OnlineServicesBootstrapService`, `RoadRage.Features.Players`, `RoadRage.Shared.Domain`, `RoadRage.App.Run`?**
  _High betweenness centrality (0.129) - this node is a cross-community bridge._
- **Why does `DefinitionId` connect `DefinitionId` to `PassengerActionVerdictCode`, `DriverProfileDef`, `CharacterCatalog`, `Story13CharacterSetupTests`, `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `NetworkedPlayerSpawnService`, `VehicleProfileDef`, `Empty Map Entry Playmode Tests`, `RageTuningDef`, `TrafficSettingsDef`, `.MenuResolvesProfileAndPublishesTheChosenCharacter`, `PlayerProfileFileStore`, `PassengerActionDef`, `PlayerProfile`, `RageTuningCatalog`?**
  _High betweenness centrality (0.118) - this node is a cross-community bridge._
- **Why does `RunFlowController` connect `RunFlowController` to `NetworkedPlayerState`, `NetworkedVehicleState`, `.BootstrapToWorldCompletesEpic1PlayableCheckpoint`, `RunCheckpointHudScreen`, `LocalOnFootController`, `RunCompositionRoot`, `NetworkedVehicleDriverController`, `Story26InGameHudTests`, `Story43PassengerActionOneChangesRageTests`, `NetworkedAIVehicleState`, `Story55NetworkedAiRageTargetingTests`, `CharacterCatalog`, `RageTuningDef`, `Story15EmptyMapEntryTests`, `AIVehicleBehaviorDebugView`, `NetworkedPassengerActionIntent`, `Story56RageRoadEventTriggerTests`, `Story36Epic3DrivingPlayableCheckpointTests`, `MonoBehaviour`, `.FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`, `Story16Epic1PlayableCheckpointTests`, `Empty Map Entry Playmode Tests`, `.MvpRunProvidesOfflineAndNetworkPassengerActionWiring`, `RoadRage.App.Run`, `.RefreshLocalReviveCountdown`, `.HandleLifecycleChanged`?**
  _High betweenness centrality (0.099) - this node is a cross-community bridge._
- **What connects `SchemaVersion`, `High`, `Low` to the rest of the system?**
  _509 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` be split into smaller, more focused modules?**
  _Cohesion score 0.1265597147950089 - nodes in this community are weakly interconnected._
- **Should `Story13CharacterSetupTests` be split into smaller, more focused modules?**
  _Cohesion score 0.09438775510204081 - nodes in this community are weakly interconnected._
- **Should `Story25NetworkedPlayerSpawnTests` be split into smaller, more focused modules?**
  _Cohesion score 0.1310344827586207 - nodes in this community are weakly interconnected._