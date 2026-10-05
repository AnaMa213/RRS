# Graph Report - RRS  (2026-10-05)

## Corpus Check
- 173 files · ~226,998 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4340 nodes · 9973 edges · 183 communities (166 shown, 17 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 425 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a5196724`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .Add
- DefinitionId
- .Build
- GreyboxAssetSeedMetadata
- SweepElement
- MainMenuScreen
- GateAEvidenceParameters
- CompiledRoadModel
- TireSample
- VehicleSuspensionModel
- PairReviewModel
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- .Localize
- LocalOnFootController
- JunctionActorReport
- NpcReactionEffect
- .CheckVisuals
- MigrationReport
- VehiclePhysicsBody
- PairReviewEntry
- AgentObservation
- AuthoredRoadModel
- JunctionClearance
- .CreatePlan
- VehicleArcadeAssist
- V1ImportResult
- JunctionRecord
- PassengerActionIntent
- AuthoringDecisions
- TrafficFrame
- CampaignTraceability
- NetworkedVehicleDriverController
- .ZoneSpan
- RoutePlan
- NetworkedVehicleSeatService
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- .Manifest
- ConflictSweep
- RageDisposition
- MotionPlan
- JunctionRecords.cs
- HistoricalMovementReader
- .Decide
- ReferenceTrack
- .Regenerate
- VehicleDriveIntent
- ObservationChannel
- .Add
- .TickV2Slice
- .Draw
- ImportedCurve
- PairSweep
- .FullPath
- AIVehicleBehaviorDebugView
- .FixedUpdate
- TrafficV2HazardCollector
- NetworkedAIVehicleState
- NetworkedRunSessionMonitor
- .RequestHonk
- LobbyRosterService
- Blocker
- TrackingMeasurement.cs
- .Compute
- TrafficDecisionProjection
- SpatialEntry
- RageTuningDef
- .Assemble
- MainMenuProfileFlowController
- UserNotice
- RouteReason
- MainMenuFlowController
- PassengerActionVerdictCode
- NetworkedPlayerState
- RunEscapeMenuScreen
- KinematicOffsetBounds
- RoadModelVersion
- JunctionCoordinator
- .Measure
- PlayerMode
- .Build
- LobbyFlowController
- RoadModelValidationIssue
- CharacterCatalog
- DispositionKind
- RoadLineage
- PlayerProfileBootstrapService
- V1Node
- JunctionConflictIndex
- .ObserveTrackingTolerance
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- JunctionControlKind
- .Create
- .TryGetCorridor
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- .IsPortalClear
- RoadGeometryValidator
- NetworkedBossState.cs
- NetworkedVehicleState
- HazardRootClass
- PlayerProfile
- JunctionExitBound
- IEnumerable
- NetworkedCrewEconomyState.cs
- TrafficSettingsDef
- .IsSurfaceOnlyCollision
- DriverProfile
- IHostOwnedRuntimeState
- Dictionary
- RoadRage.Features.Vehicles.Traffic.Planning
- PairReviewStatus
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- MonoBehaviour
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadId
- SpeedPlan
- ProjectionKey
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- ConditionalWeakTable
- DriverProfile
- RoadModelDocument
- Vector3
- LaneNode
- HashSet
- RoadModelCanonicalWriter
- SpeedConstraint
- JunctionSnapshot
- ImportContext
- .Step
- ProfilerMarker
- NetworkedVehicleSeatIntent
- Stopwatch
- .InterStepBound
- TrackPiece
- RoadRage.Features.Vehicles.Traffic.Migration
- LongitudinalDecision
- TrafficV2Insertion
- NetworkedPlayerLifecycleService
- RoutePath
- NetworkedPassengerActionIntent
- ElementTrace
- V2FallbackReason
- NetworkedVehicleState.cs
- NetworkedPlayerLifecycleIntent
- LobbyPlayerSlotView
- TrafficV2Code
- GateAEvidenceResult
- StatusFilter
- NetworkedPlayerReviveIntent
- LaneGraphRouting
- NetworkedVehicleRecoveryIntent
- ModelDto
- PairRelation
- RoadRage.Shared.Domain
- TrackingTolerance
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- LaneGraph
- TrafficPerception
- DriverProfileDef
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 104 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `NetworkedVehicleState` - 67 edges
5. `ImportContext` - 67 edges
6. `AuthoredRoadModel` - 63 edges
7. `CompiledRoadModel` - 63 edges
8. `NetworkedVehicleDriverController` - 58 edges
9. `TrafficFrame` - 53 edges
10. `VehicleProfile` - 51 edges

## Surprising Connections (you probably didn't know these)
- `TrafficV2StepRunner` --references--> `JunctionCoordinator`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs
- `PortalTrafficSpawner` --references--> `TrafficV2StepRunner`  [EXTRACTED]
  Assets/RoadRage/App/Run/PortalTrafficSpawner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs
- `PairReviewModel` --references--> `ShortElement`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs
- `LobbyFlowController` --references--> `CharacterCatalog`  [EXTRACTED]
  Assets/RoadRage/App/Lobby/LobbyFlowController.cs → Assets/RoadRage/Features/Players/CharacterCatalog.cs
- `MainMenuProfileFlowController` --references--> `CharacterCatalog`  [EXTRACTED]
  Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs → Assets/RoadRage/Features/Players/CharacterCatalog.cs

## Import Cycles
- None detected.

## Communities (183 total, 17 thin omitted)

### Community 0 - ".Add"
Cohesion: 0.13
Nodes (16): Bounds, CompiledRoadModel, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation (+8 more)

### Community 1 - "DefinitionId"
Cohesion: 0.16
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 2 - ".Build"
Cohesion: 0.18
Nodes (13): IReadOnlyList, JunctionActorReport, JunctionConflictIndex, RoadId, TrafficFrame, JunctionRequestBuilder, ConditionalWeakTable, DriverProfile (+5 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.18
Nodes (13): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+5 more)

### Community 5 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (17): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+9 more)

### Community 7 - "CompiledRoadModel"
Cohesion: 0.09
Nodes (24): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+16 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "PairReviewModel"
Cohesion: 0.21
Nodes (6): CompiledRoadModel, Dictionary, List, StringBuilder, PairReview, PairReviewModel

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "RoadRage.Features.Online"
Cohesion: 0.08
Nodes (16): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+8 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.17
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - ".Localize"
Cohesion: 0.13
Nodes (19): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+11 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "JunctionActorReport"
Cohesion: 0.11
Nodes (16): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, Localized (+8 more)

### Community 18 - "NpcReactionEffect"
Cohesion: 0.11
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 19 - ".CheckVisuals"
Cohesion: 0.15
Nodes (11): Surface, HashSet, IEnumerable, Renderer, StringBuilder, VisibleFaces, Component, Mesh (+3 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.14
Nodes (12): Rigidbody, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile, Profile (+4 more)

### Community 22 - "PairReviewEntry"
Cohesion: 0.19
Nodes (8): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, PairReviewActions, PairReviewEntry

### Community 23 - "AgentObservation"
Cohesion: 0.12
Nodes (22): Func, LaneSide, RoadId, RoadLocation, StringBuilder, Vector3, VehicleFootprint, AdjacentOccupantFact (+14 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.10
Nodes (17): ConflictZone, Dictionary, HashSet, IList, List, RoadCurveSample, RoadModelSource, SortedDictionary (+9 more)

### Community 25 - "JunctionClearance"
Cohesion: 0.15
Nodes (12): Collider, IReadOnlyList, List, Transform, Vector2, Vector3, JunctionClearance, JunctionClearanceSurface (+4 more)

### Community 26 - ".CreatePlan"
Cohesion: 0.11
Nodes (19): AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, ConflictDecision, Dictionary, HashSet (+11 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.14
Nodes (17): AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedConnection, ImportedPortal, ImportedSection, PublishedDisplacement (+9 more)

### Community 29 - "JunctionRecord"
Cohesion: 0.10
Nodes (17): JunctionBatchCounters, JunctionGrantStatus, Denied, Granted, Held, Released, Revoked, JunctionRecord (+9 more)

### Community 30 - "PassengerActionIntent"
Cohesion: 0.29
Nodes (5): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, INetworkSerializable

### Community 31 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (41): AppliedWidth, Dictionary, FileLayout, Func, IList, JunctionControlKind, List, RoadBoundsBox (+33 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.08
Nodes (30): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+22 more)

### Community 33 - "CampaignTraceability"
Cohesion: 0.21
Nodes (5): Dictionary, HashSet, IReadOnlyDictionary, CampaignTraceability, Elements

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.13
Nodes (8): DevIndestructibleVehicle, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController

### Community 35 - ".ZoneSpan"
Cohesion: 0.32
Nodes (4): Bounds, CompiledRoadModel, RoadBoundsBox, Vector3

### Community 36 - "RoutePlan"
Cohesion: 0.06
Nodes (49): CompiledRoadModel, IReadOnlyList, RoadElementKind, RoadId, RoadLocation, RoadModelVersion, DecisionCounter, RouteDiagnostic (+41 more)

### Community 37 - "NetworkedVehicleSeatService"
Cohesion: 0.15
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.15
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.06
Nodes (34): Vector3, CompiledRoadModel, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, Vector3, HorizonEnd (+26 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.16
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.11
Nodes (16): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+8 more)

### Community 44 - ".Manifest"
Cohesion: 0.24
Nodes (8): IList, IReadOnlyList, KeyValuePair, RoadId, RoadRecordKind, LineageKeyRegistry, Keys, ImportManifestEntry

### Community 45 - "ConflictSweep"
Cohesion: 0.22
Nodes (9): Vector2, ConflictSweep, GridPath, PoseFrame, SweepPose, Plan, RoadModelValidationProfile, GridPath (+1 more)

### Community 46 - "RageDisposition"
Cohesion: 0.12
Nodes (12): BoxCollider, NetworkTransform, Rigidbody, IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm (+4 more)

### Community 47 - "MotionPlan"
Cohesion: 0.05
Nodes (52): DrivabilityProfile, IReadOnlyList, LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan (+44 more)

### Community 48 - "JunctionRecords.cs"
Cohesion: 0.12
Nodes (17): RoadId, JunctionApproach, Crossed, JunctionExitAssessment, JunctionMovementPosition, JunctionMovementStatus, Ahead, Behind (+9 more)

### Community 49 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (13): JunctionRecord, RoadCurveSample, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord (+5 more)

### Community 50 - ".Decide"
Cohesion: 0.07
Nodes (40): DriverProfile, IReadOnlyList, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate (+32 more)

### Community 51 - "ReferenceTrack"
Cohesion: 0.22
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "VehicleDriveIntent"
Cohesion: 0.21
Nodes (6): VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 54 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 55 - ".Add"
Cohesion: 0.25
Nodes (4): Dictionary, JunctionFeature, Predicate, ImportedJunction

### Community 57 - ".Draw"
Cohesion: 0.27
Nodes (6): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 58 - "ImportedCurve"
Cohesion: 0.23
Nodes (9): List, RoadCurve, RoadCurveSample, ImportedCurve, MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit (+1 more)

### Community 59 - "PairSweep"
Cohesion: 0.31
Nodes (6): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 60 - ".FullPath"
Cohesion: 0.21
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 61 - "AIVehicleBehaviorDebugView"
Cohesion: 0.22
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 62 - ".FixedUpdate"
Cohesion: 0.22
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 63 - "TrafficV2HazardCollector"
Cohesion: 0.09
Nodes (29): TrafficHazardCollectorCounters, Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState (+21 more)

### Community 64 - "NetworkedAIVehicleState"
Cohesion: 0.36
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 65 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 66 - ".RequestHonk"
Cohesion: 0.33
Nodes (3): NetworkObjectReference, Rpc, RpcParams

### Community 67 - "LobbyRosterService"
Cohesion: 0.04
Nodes (39): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+31 more)

### Community 68 - "Blocker"
Cohesion: 0.08
Nodes (26): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+18 more)

### Community 69 - "TrackingMeasurement.cs"
Cohesion: 0.22
Nodes (9): IReadOnlyList, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees (+1 more)

### Community 70 - ".Compute"
Cohesion: 0.18
Nodes (11): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+3 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.03
Nodes (61): IReadOnlyList, RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind (+53 more)

### Community 72 - "SpatialEntry"
Cohesion: 0.09
Nodes (24): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementOccupant, IntentInterval (+16 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.09
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 74 - ".Assemble"
Cohesion: 0.24
Nodes (5): DrivabilityProfile, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter

### Community 75 - "MainMenuProfileFlowController"
Cohesion: 0.33
Nodes (3): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex

### Community 76 - "UserNotice"
Cohesion: 0.17
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 79 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+19 more)

### Community 80 - "NetworkedPlayerState"
Cohesion: 0.39
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 81 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.13
Nodes (16): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+8 more)

### Community 83 - "RoadModelVersion"
Cohesion: 0.32
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 84 - "JunctionCoordinator"
Cohesion: 0.13
Nodes (27): CompiledRoadModel, IReadOnlyList, JunctionActorReport, JunctionConflictIndex, JunctionRecord, JunctionTraversal, List, RoadId (+19 more)

### Community 85 - ".Measure"
Cohesion: 0.18
Nodes (14): BoxCollider, CompiledRoadModel, GameObject, RoadId, RoadModelValidationProfile, Scene, VehiclePhysicsBody, VehicleProfile (+6 more)

### Community 86 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 87 - ".Build"
Cohesion: 0.38
Nodes (3): IReadOnlyList, Vector2, KinematicPoseSet

### Community 88 - "LobbyFlowController"
Cohesion: 0.10
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (18): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+10 more)

### Community 90 - "CharacterCatalog"
Cohesion: 0.40
Nodes (3): List, CharacterCatalog, Count

### Community 91 - "DispositionKind"
Cohesion: 0.18
Nodes (11): DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint, Movement, MovementApproachPath (+3 more)

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "PlayerProfileBootstrapService"
Cohesion: 0.18
Nodes (8): PlayerNameValidator, PlayerProfileBootstrapService, PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist, PlayerProfileResolution

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (29): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+21 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.15
Nodes (12): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, Portal, RoadBoundsBox, RoadId, Vector3 (+4 more)

### Community 96 - ".ObserveTrackingTolerance"
Cohesion: 0.47
Nodes (3): TrackingToleranceResponse, Latched, LatchedAtStep

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.11
Nodes (17): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+9 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.13
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.33
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "JunctionControlKind"
Cohesion: 0.33
Nodes (6): JunctionControlKind, Priority, Signalized, Stop, Uncontrolled, Yield

### Community 102 - ".Create"
Cohesion: 0.33
Nodes (5): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3

### Community 104 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MenuCharacterPreview"
Cohesion: 0.18
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.18
Nodes (11): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, RoadCurveSample (+3 more)

### Community 111 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 112 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 113 - "HazardRootClass"
Cohesion: 0.14
Nodes (12): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer, HazardRootClass, Obstacle, Self (+4 more)

### Community 114 - "PlayerProfile"
Cohesion: 0.21
Nodes (8): PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath

### Community 115 - "JunctionExitBound"
Cohesion: 0.29
Nodes (7): JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations, RouteEnd

### Community 117 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 120 - "DriverProfile"
Cohesion: 0.11
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.07
Nodes (31): TrafficV2Work, TrafficV2WorkCounters, IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun (+23 more)

### Community 124 - "PairReviewStatus"
Cohesion: 0.40
Nodes (5): PairReviewStatus, Modified, New, Removed, Unchanged

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.18
Nodes (6): IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.12
Nodes (16): Collider, GameObject, IEnumerator, IReadOnlyList, List, PortalTrafficSpawner, Composition, CompositionFrozen (+8 more)

### Community 128 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.05
Nodes (31): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+23 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.09
Nodes (27): JunctionSnapshot, CompiledRoadModel, JunctionActorReport, List, RoadId, TrafficFrame, TrafficV2StepCost, TrafficV2StepRunner (+19 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.05
Nodes (16): Difficulty, Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Button, LobbyRosterScreen (+8 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.18
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadId"
Cohesion: 0.05
Nodes (68): CompiledConflictZone, JunctionMovement, RoadModelCanonicalPayload, Vector3, ConflictZone, DrivabilityProfile, ImportManifest, ImportManifestEntry (+60 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.08
Nodes (35): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+27 more)

### Community 135 - "ProjectionKey"
Cohesion: 0.47
Nodes (3): ProjectionKey, IEquatable, ProjectionKey

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.05
Nodes (44): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference, DrivabilityAdmissionFailed (+36 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.26
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 141 - "RoadModelDocument"
Cohesion: 0.08
Nodes (30): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+22 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.25
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.08
Nodes (21): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite, SpeedConstraint, AnticipatedDeceleration (+13 more)

### Community 148 - "ImportContext"
Cohesion: 0.22
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 149 - ".Step"
Cohesion: 0.11
Nodes (15): DriverProfile, IReadOnlyList, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord (+7 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 153 - ".InterStepBound"
Cohesion: 0.34
Nodes (6): Quaternion, Vector3, BodyState, GaugeBox, Rho, TrackingMeasurement

### Community 154 - "TrackPiece"
Cohesion: 0.16
Nodes (11): RoadCurve, RoadElementKind, RoadId, NominalPose, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters (+3 more)

### Community 155 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.17
Nodes (7): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, RoadRage.Features.Vehicles.Traffic.Migration

### Community 156 - "LongitudinalDecision"
Cohesion: 0.10
Nodes (18): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+10 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.06
Nodes (31): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+23 more)

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (21): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+13 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (32): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+24 more)

### Community 170 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 172 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 178 - "GateAEvidenceResult"
Cohesion: 0.15
Nodes (14): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+6 more)

### Community 179 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 180 - "NetworkedPlayerReviveIntent"
Cohesion: 0.22
Nodes (7): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, HostOwnedNetworkStateBehaviour, IsHostAuthority, NetworkBehaviour

### Community 183 - "LaneGraphRouting"
Cohesion: 0.20
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "ModelDto"
Cohesion: 0.08
Nodes (23): AdjacencyDto, DrivabilityProfile, EffectiveLaneCorridor, Comparison, RoadModelCompiler, ModelDto, VehicleClassMask, All (+15 more)

### Community 191 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.06
Nodes (20): DevVehicleSpawner, SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run (+12 more)

### Community 194 - "TrackingTolerance"
Cohesion: 0.11
Nodes (18): VehicleCoverage, TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, TrafficV2Verdict, Allowed, TrackingTolerance (+10 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.15
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.13
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.19
Nodes (11): VehicleGapFact, Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance, PerceptionLimits (+3 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.31
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.12
Nodes (9): CharacterController, Collider, IReadOnlyList, List, Quaternion, RaycastHit, Vector3, NetworkedAIVehicleDriverController (+1 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1113 isolated node(s):** `Index`, `Model`, `Current`, `FrameFailures`, `Batches` (+1108 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1572 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **17 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `GateAEvidenceParameters`, `JunctionActorReport`, `.Step`, `AgentObservation`, `.InterStepBound`, `LongitudinalDecision`, `TrafficV2Insertion`, `RoutePlan`, `V2FallbackReason`, `.Decide`, `ReferenceTrack`, `NetworkedPlayerReviveIntent`, `.TickV2Slice`, `TrafficV2HazardCollector`, `TrackingTolerance`, `Blocker`, `TrafficDecisionProjection`, `SpatialEntry`, `JunctionConflictIndex`, `.ObserveTrackingTolerance`, `RoadRage.Features.Vehicles.Traffic.Planning`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.251) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `MonoBehaviour`, `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `.IsPortalClear`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `.Step`, `.TickV2Slice`, `RoadRage.Features.Vehicles.Traffic.Planning`, `TrafficV2Insertion`, `LaneGraph`?**
  _High betweenness centrality (0.136) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `.Add`, `CampaignTraceability`, `CompiledRoadModel`, `.TryGetCorridor`, `ProjectionKey`, `RoadCurve`, `RoadModelDocument`, `RoadGeometryValidator`, `.Localize`, `RoadModelCanonicalWriter`, `TrafficV2HazardCollector`, `RoadModelValidationIssue`, `RoadLineage`, `TrafficV2Insertion`, `ModelDto`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.123) - this node is a cross-community bridge._
- **What connects `Index`, `Model`, `Current` to the rest of the system?**
  _1113 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Add` be split into smaller, more focused modules?**
  _Cohesion score 0.13054187192118227 - nodes in this community are weakly interconnected._
- **Should `MainMenuScreen` be split into smaller, more focused modules?**
  _Cohesion score 0.14210526315789473 - nodes in this community are weakly interconnected._
- **Should `GateAEvidenceParameters` be split into smaller, more focused modules?**
  _Cohesion score 0.09523809523809523 - nodes in this community are weakly interconnected._
