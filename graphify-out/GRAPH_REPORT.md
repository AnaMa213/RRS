# Graph Report - RRS  (2026-10-07)

## Corpus Check
- 179 files · ~244,945 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4571 nodes · 10638 edges · 183 communities (176 shown, 6 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 459 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `da77340a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedVehicleSeatService
- List
- RoadRage.Features.Vehicles.Traffic.Migration
- GreyboxAssetSeedMetadata
- SweepPose
- RoutePlan
- GateAEvidenceParameters
- .Localize
- TireSample
- VehicleSuspensionModel
- MonoBehaviour
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- FacepunchSteamLobbyPlatform
- LocalOnFootController
- Blocker
- RageTuningDef
- LobbyRosterService
- MigrationReport
- VehiclePhysicsBody
- JunctionRecord
- .GridPaths
- MainMenuScreen
- NpcReactionEffect
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1ImportResult
- DefinitionId
- RoadId
- AuthoringDecisions
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .FullPath
- NetworkedRunSessionMonitor
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- .Measure
- PairSweep
- MeasurementRun
- MotionPlan
- JunctionRequestRejection
- PairReviewEntry
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- PairReviewModel
- .Add
- .ImportLaneModule
- CampaignTraceability
- .Collect
- ImportedCurve
- NetworkedAIVehicleState
- ReferenceTrack
- TrackingTolerance
- AIVehicleBehaviorDebugView
- AgentObservation
- .Decide
- .FixedUpdate
- TrackPiece
- IPathGeometry
- PassengerActionVerdictCode
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- .Draw
- TrafficV2HazardCollector
- PassengerActionIntent
- UserNotice
- .MergeGapAdmits
- MainMenuFlowController
- .CheckVisuals
- ModelDto
- NetworkedPlayerState
- KinematicOffsetBounds
- TrackingMeasurement.cs
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- .FingerprintWithInputs
- LobbyFlowController
- RoadModelValidationIssue
- NetworkPlayerRegistry
- LobbyPlayerSlotView
- RoadLineage
- .Measure
- V1Node
- JunctionConflictIndex
- RunEscapeMenuScreen
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- RageDisposition
- .TrySpawnSelectedProfile
- LocalVoidRespawnController
- .Track
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionApproach
- MainMenuProfileFlowController
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- DriverProfileDef
- PlanningDecision
- JunctionTraversal
- .Build
- LongitudinalDecision
- JunctionClearanceResult
- TrafficSettingsDef
- PlayerProfileFileStore
- DriverProfile
- StatusFilter
- HazardRootClass
- .Step
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- SignalPhaseController
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- .Create
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- .Run
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- LongitudinalArbitration.cs
- RoadModelCanonicalWriter
- SpeedConstraint
- PathIssue
- ImportContext
- TrafficHazardKind
- .FromRoute
- NetworkedVehicleSeatIntent
- HostOwnedNetworkStateBehaviour
- .InterStepBound
- PairDecisionState
- .Read
- LineageKeyRegistry
- TrafficV2Insertion
- SpeedPlan.cs
- NetworkedPlayerLifecycleService
- NetworkedLocalPlayerPoseReporter
- .MeasurePath
- .Build
- RoutePath
- NetworkedPassengerActionIntent
- ElementTrace
- V2FallbackReason
- NetworkedPlayerLifecycleIntent
- .Configure
- TrafficV2Code
- TrafficV2Settings
- NetworkedPlayerReviveIntent
- LaneGraphRouting
- JunctionExitBound
- NetworkedVehicleRecoveryIntent
- .Compile
- RoadRage.Shared.Domain
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 107 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `NetworkedVehicleState` - 67 edges
6. `ImportContext` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `TrafficFrame` - 61 edges
10. `NetworkedVehicleDriverController` - 58 edges

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

## Communities (183 total, 6 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 1 - "List"
Cohesion: 0.12
Nodes (15): CompiledJunctionMovement, ConflictKind, IList, List, StringBuilder, Vector2, PairRefinement, RefineContext (+7 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.12
Nodes (12): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning (+4 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.17
Nodes (14): CompiledRoadModel, Dictionary, IReadOnlyList, RoadCurve, RoadCurveSample, RoadId, ShortElement, SweepElement (+6 more)

### Community 5 - "RoutePlan"
Cohesion: 0.05
Nodes (58): CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation, RoadModelVersion, DecisionCounter, RouteDiagnostic, None (+50 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (19): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+11 more)

### Community 7 - ".Localize"
Cohesion: 0.12
Nodes (22): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+14 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 11 - "RunFlowController"
Cohesion: 0.07
Nodes (5): Camera, HashSet, Transform, RunFlowController, ActiveLocalPlayer

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.07
Nodes (24): Difficulty, Task, FacepunchSteamLobbyPlatform, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId, Success (+16 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.08
Nodes (26): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+18 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 19 - "LobbyRosterService"
Cohesion: 0.09
Nodes (15): Difficulty, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId (+7 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "JunctionRecord"
Cohesion: 0.13
Nodes (17): JunctionSnapshot, JunctionControlKind, RoadId, JunctionBatchCounters, JunctionMovementPosition, JunctionMovementStatus, Ahead, Behind (+9 more)

### Community 23 - ".GridPaths"
Cohesion: 0.17
Nodes (8): Vector2, GridPath, PoseFrame, IReadOnlyList, Vector2, KinematicPoseSet, PoseGrid, PoseFrame

### Community 24 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 25 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (20): CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, RoadModelValidationProfile, SortedDictionary (+12 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.08
Nodes (28): RoadId, AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed (+20 more)

### Community 29 - "DefinitionId"
Cohesion: 0.11
Nodes (15): List, CharacterCatalog, Count, Color, GameObject, CharacterDef, DisplayName, Id (+7 more)

### Community 30 - "RoadId"
Cohesion: 0.07
Nodes (35): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+27 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (51): AppliedWidth, ConflictKind, FileLayout, Func, IEnumerable, IList, JunctionControlKind, List (+43 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (31): CompiledRoadModel, Dictionary, IReadOnlyList, ProfilerMarker, RoadCurve, RoadCurveSample, RoadElementKind, RoadId (+23 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.07
Nodes (31): MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing, GateAEvidenceStale, HorizonNonConforming (+23 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - ".FullPath"
Cohesion: 0.18
Nodes (7): Action, KeyValuePair, MenuItem, Scene, MenuItem, MenuItem, Func

### Community 36 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.12
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.16
Nodes (12): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+4 more)

### Community 40 - "PathHorizon"
Cohesion: 0.10
Nodes (22): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, HorizonEnd, ExitPortal, LookAheadLimit (+14 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.13
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.08
Nodes (25): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+17 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (13): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+5 more)

### Community 44 - ".Measure"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+4 more)

### Community 45 - "PairSweep"
Cohesion: 0.22
Nodes (7): IList, List, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "MeasurementRun"
Cohesion: 0.10
Nodes (21): IReadOnlyList, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label, MaxPopulation (+13 more)

### Community 47 - "MotionPlan"
Cohesion: 0.07
Nodes (35): VehicleCoverage, CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing (+27 more)

### Community 48 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 49 - "PairReviewEntry"
Cohesion: 0.21
Nodes (7): PairReviewActions, PairReviewEntry, PairReviewStatus, Modified, New, Removed, Unchanged

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.08
Nodes (21): CompiledJunctionControl, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, List, RoadCurveSample (+13 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReviewModel"
Cohesion: 0.20
Nodes (6): CompiledRoadModel, List, RoadBoundsBox, StringBuilder, PairReview, PairReviewModel

### Community 54 - ".Add"
Cohesion: 0.16
Nodes (15): Bounds, CompiledJunctionMovement, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation (+7 more)

### Community 55 - ".ImportLaneModule"
Cohesion: 0.24
Nodes (4): Dictionary, JunctionFeature, Predicate, ImportedJunction

### Community 56 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 57 - ".Collect"
Cohesion: 0.19
Nodes (12): Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3, VehiclePhysicsBody (+4 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.19
Nodes (10): List, RoadCurve, RoadCurveSample, ImportedCurve, ImportedSection, MovementRole, RoundaboutContinuation, RoundaboutEntry (+2 more)

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.31
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference

### Community 60 - "ReferenceTrack"
Cohesion: 0.21
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 61 - "TrackingTolerance"
Cohesion: 0.36
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

### Community 62 - "AIVehicleBehaviorDebugView"
Cohesion: 0.25
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 63 - "AgentObservation"
Cohesion: 0.13
Nodes (17): Func, IReadOnlyList, RoadLocation, StringBuilder, VehicleFootprint, AgentObservation, ObservationChannel, Items (+9 more)

### Community 64 - ".Decide"
Cohesion: 0.14
Nodes (14): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing (+6 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 66 - "TrackPiece"
Cohesion: 0.18
Nodes (10): RoadCurve, RoadElementKind, NominalPose, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 68 - "IPathGeometry"
Cohesion: 0.22
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (29): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+21 more)

### Community 70 - ".Compute"
Cohesion: 0.21
Nodes (9): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+1 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.03
Nodes (61): IReadOnlyList, RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind (+53 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.08
Nodes (26): Bounds, List, Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3 (+18 more)

### Community 73 - ".Draw"
Cohesion: 0.23
Nodes (7): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, RoadModelValidationProfile, SceneView

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.11
Nodes (17): TrafficHazardCollectorCounters, Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity (+9 more)

### Community 75 - "PassengerActionIntent"
Cohesion: 0.17
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, RoadRage.Features.PassengerActions, INetworkSerializable

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.25
Nodes (5): Surface, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "ModelDto"
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 81 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+9 more)

### Community 83 - "TrackingMeasurement.cs"
Cohesion: 0.25
Nodes (8): InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees, PieceBound

### Community 84 - "JunctionCoordinator"
Cohesion: 0.16
Nodes (20): CompiledRoadModel, Dictionary, HashSet, KeyValuePair, List, RoadId, Grant, JunctionCoordinator (+12 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.18
Nodes (12): ConflictSweep, RoadId, RoadModelValidationProfile, MovementSide, RefineNode, RefineSegment, GridPath, LeafState (+4 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (13): JunctionRecord, RoadCurveSample, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord (+5 more)

### Community 87 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 88 - "LobbyFlowController"
Cohesion: 0.11
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (18): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+10 more)

### Community 90 - "NetworkPlayerRegistry"
Cohesion: 0.27
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.05
Nodes (43): ConditionalWeakTable, Dictionary, IReadOnlyList, JunctionControl, JunctionMovement, LaneCorridor, List, RoadId (+35 more)

### Community 93 - ".Measure"
Cohesion: 0.15
Nodes (15): BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, Scene, Vector3 (+7 more)

### Community 94 - "V1Node"
Cohesion: 0.10
Nodes (28): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+20 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.10
Nodes (20): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+12 more)

### Community 96 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (16): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+8 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 102 - ".TrySpawnSelectedProfile"
Cohesion: 0.14
Nodes (5): CharacterController, Collider, GameObject, Quaternion, Vector3

### Community 103 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 104 - ".Track"
Cohesion: 0.22
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.10
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MenuCharacterPreview"
Cohesion: 0.18
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 108 - "JunctionApproach"
Cohesion: 0.15
Nodes (8): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters, JunctionApproach, Crossed, JunctionExitAssessment, JunctionText

### Community 109 - "MainMenuProfileFlowController"
Cohesion: 0.12
Nodes (14): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, PlayerNameValidator, PlayerProfile, CharacterId, DisplayName, PlayerProfileBootstrapService (+6 more)

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.15
Nodes (14): RoadModelValidationProfile, Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor (+6 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.20
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - "DriverProfileDef"
Cohesion: 0.36
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 113 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 114 - "JunctionTraversal"
Cohesion: 0.22
Nodes (8): TraversalComparer, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds, IEqualityComparer

### Community 115 - ".Build"
Cohesion: 0.27
Nodes (3): Dictionary, HistoricalPairFingerprintTable, Dictionary

### Community 116 - "LongitudinalDecision"
Cohesion: 0.09
Nodes (19): IReadOnlyList, LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses (+11 more)

### Community 117 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "PlayerProfileFileStore"
Cohesion: 0.31
Nodes (5): PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "HazardRootClass"
Cohesion: 0.25
Nodes (7): HazardRootClass, Obstacle, Self, Static, TrafficV2Vehicle, Vehicle, WalkingPlayer

### Community 123 - ".Step"
Cohesion: 0.09
Nodes (18): DriverProfile, IReadOnlyList, JunctionControlKind, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord (+10 more)

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.15
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "SignalPhaseController"
Cohesion: 0.29
Nodes (6): CompiledRoadModel, IReadOnlyList, SignalPhaseController, Current, Model, CompiledSignalPlan

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (18): HashSet, List, ProfilerMarker, RoadId, Stopwatch, TrafficV2StepRunner, Collector, Coordinator (+10 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.05
Nodes (16): Difficulty, Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Button, LobbyRosterScreen (+8 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.16
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (73): IList, RoadLocalizationProfile, RoadModelSource, JunctionMovement, RoadModelCanonicalPayload, DrivabilityProfile, ImportManifest, ImportManifestEntry (+65 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.12
Nodes (21): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, RoadLimitValue, CapMetersPerSecond (+13 more)

### Community 135 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - ".Run"
Cohesion: 0.48
Nodes (4): CompiledRoadModel, IEnumerable, JunctionSnapshot, TrafficV2StepCost

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (40): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+32 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.09
Nodes (18): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, InFallback (+10 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "LongitudinalArbitration.cs"
Cohesion: 0.10
Nodes (25): RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalMemory, None, LongitudinalObstacle, LongitudinalPerception (+17 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 148 - "ImportContext"
Cohesion: 0.20
Nodes (8): DrivabilityProfile, RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, V1RoadModelImporter, CircleFit

### Community 149 - "TrafficHazardKind"
Cohesion: 0.33
Nodes (5): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer

### Community 150 - ".FromRoute"
Cohesion: 0.40
Nodes (3): CompiledRoadModel, IReadOnlyList, List

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.09
Nodes (16): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel (+8 more)

### Community 153 - ".InterStepBound"
Cohesion: 0.33
Nodes (6): Quaternion, Vector3, BodyState, GaugeBox, Rho, TrackingMeasurement

### Community 154 - "PairDecisionState"
Cohesion: 0.33
Nodes (6): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "LineageKeyRegistry"
Cohesion: 0.47
Nodes (5): IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.07
Nodes (28): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+20 more)

### Community 158 - "SpeedPlan.cs"
Cohesion: 0.15
Nodes (14): DeferredLimit, DeferredLimitKind, RoadLimit, DeferredLimitState, DeferredAuthored, DeferredUnauthored, RoadLimitState, Applied (+6 more)

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 162 - ".Build"
Cohesion: 0.17
Nodes (11): JunctionKinematics, Known, ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId, Vector3 (+3 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (19): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+11 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (32): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+24 more)

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 172 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 177 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 180 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 183 - "LaneGraphRouting"
Cohesion: 0.24
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 184 - "JunctionExitBound"
Cohesion: 0.12
Nodes (15): IReadOnlyList, JunctionRecord, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations (+7 more)

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - ".Compile"
Cohesion: 0.16
Nodes (8): DrivabilityProfile, RoadModelVersion, High, IsEmpty, Low, SchemaVersion, Comparison, RoadModelCompiler

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (18): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+10 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.14
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.09
Nodes (32): LaneSide, RoadId, Vector3, AdjacentOccupantFact, ExitOccupancyFact, IntentOverlapKind, ConflictZone, SameElement (+24 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1159 isolated node(s):** `RoadRage.App`, `Instance`, `Router`, `Notices`, `Profiles` (+1154 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1640 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **6 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `RoadRage.Features.Vehicles.Traffic.Migration`, `TrafficV2StepRunner`, `RoutePlan`, `GateAEvidenceParameters`, `.Run`, `JunctionActorReport`, `LongitudinalArbitration.cs`, `Blocker`, `.FromRoute`, `HostOwnedNetworkStateBehaviour`, `.InterStepBound`, `TrafficV2Insertion`, `V2FallbackReason`, `.Collect`, `ReferenceTrack`, `TrackingTolerance`, `AgentObservation`, `TrafficDecisionProjection`, `ElementOccupant`, `JunctionConflictIndex`, `LongitudinalDecision`, `.Step`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.241) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `MonoBehaviour`, `MeasurementRun`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `LaneGraph`, `.Step`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.159) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `RoadModelRecords.cs`, `.Localize`, `TrafficV2HazardCollector`, `RoadCurve`, `RoadModelDocument`, `RoadGeometryValidator`, `MotionPlan`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `RoadLineage`, `TrafficV2Insertion`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.107) - this node is a cross-community bridge._
- **What connects `RoadRage.App`, `Instance`, `Router` to the rest of the system?**
  _1159 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `List` be split into smaller, more focused modules?**
  _Cohesion score 0.12258064516129032 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Migration` be split into smaller, more focused modules?**
  _Cohesion score 0.11587301587301588 - nodes in this community are weakly interconnected._
- **Should `RoutePlan` be split into smaller, more focused modules?**
  _Cohesion score 0.05093167701863354 - nodes in this community are weakly interconnected._