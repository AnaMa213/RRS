# Graph Report - RRS  (2026-10-08)

## Corpus Check
- 184 files · ~256,180 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4848 nodes · 11211 edges · 183 communities (177 shown, 6 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 494 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `b1db1de4`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MotionPlan
- .Refine
- RoadRage.Features.Vehicles.Traffic.Planning
- GreyboxAssetSeedMetadata
- SweepPose
- .Evaluate
- GateAEvidenceParameters
- .Localize
- RecoverySupervisor
- VehicleSuspensionModel
- NetworkedVehicleSeatService
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyJoinOutcome
- LocalOnFootController
- Blocker
- RageTuningDef
- .Measure
- MigrationReport
- VehiclePhysicsBody
- LobbyFlowController
- LobbyRosterService
- .Core
- LobbyRoomService
- AutomatedPairDecisionPolicy
- PerceivedObstacleKind
- V1RoadModelImporter.cs
- DefinitionId
- RoadId
- .PrepareStep
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .Capture
- .Step
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- MonoBehaviour
- JunctionReason
- RoadCurve
- .Measure
- PairSweep
- TrafficV2Composition.cs
- NpcReactionEffect
- .Evaluate
- .Add
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- .Entry
- TrackingTolerance
- ImportedCurve
- .AddMovement
- TrafficDriveOutcome
- LongitudinalDecision
- PassengerActionVerdictCode
- GridlockSupervisor
- TrafficV2HazardCollector
- PlayerMode
- ObservationChannel
- .Decide
- .FixedUpdate
- .Create
- TrafficV2Insertion
- IPathGeometry
- AgentObservation
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- PairReviewEntry
- .Collect
- .Draw
- UserNotice
- TrafficV2Admission
- PlanningRequest
- .CheckVisuals
- ModelDto
- JunctionTraversal
- KinematicOffsetBounds
- V1ImportResult
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- NetworkPlayerRegistry
- LobbyRosterScreen
- RoadModelValidationIssue
- RageDisposition
- LobbyPlayerSlotView
- RoadLineage
- LocalVoidRespawnController
- V1Node
- JunctionConflictIndex
- TrafficLongitudinalOutcome
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- RoutePlan
- NetworkedPlayerState
- .FingerprintWithInputs
- .UpdateSteeringState
- RageRoadEventFlowController
- MainMenuScreen
- JunctionDistances
- PlanningDecision
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- JunctionRequestRejection
- NetworkedPlayerLifecycleIntent
- RoadModelVersion
- SignalPhaseController
- SpeedConstraint
- JunctionClearanceResult
- TrafficSettingsDef
- TrafficJunctionOutcome
- DriverProfile
- StatusFilter
- ReferenceTrack
- AIVehicleBehaviorDebugView
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- .HandleRosterChanged
- RoadRageBootstrap
- TrafficV2StepRunner
- .Run
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- RoadRage.Shared.Networking
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- DriverProfileDef
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- OccupancyExclusion
- RoadModelCanonicalWriter
- RoadRage.Shared.Definitions
- RouteReason
- ImportContext
- SafetyReason
- TacticalDecision
- NetworkedVehicleSeatIntent
- VehicleCoverage
- PathIssue
- .Configure
- .Read
- .Bind
- AppSceneRouter.cs
- PairReviewStatus
- NetworkedPlayerLifecycleService
- PlanningReach
- SpeedPlanIssue
- .Build
- RoutePath
- .MeasurePath
- AuthoringDecisions
- NetworkedPassengerActionIntent
- CampaignTraceability
- V2FallbackReason
- TireSample
- TrafficV2Code
- JunctionRecord
- LaneGraphRouting
- VehicleArcadeAssist
- NetworkedVehicleRecoveryIntent
- RoadRage.Shared.Domain
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- PairReviewModel
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 130 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `NetworkedVehicleState` - 67 edges
6. `ImportContext` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `TrafficFrame` - 64 edges
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

### Community 0 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 1 - ".Refine"
Cohesion: 0.10
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.11
Nodes (15): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning (+7 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.18
Nodes (15): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+7 more)

### Community 5 - ".Evaluate"
Cohesion: 0.13
Nodes (18): ProfilerMarker, PlanningSpine, CompiledRoadModel, RoadId, RoadLocation, DecisionCounter, RouteDiagnostic, None (+10 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.09
Nodes (20): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+12 more)

### Community 7 - ".Localize"
Cohesion: 0.12
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - "RecoverySupervisor"
Cohesion: 0.05
Nodes (37): IReadOnlyList, List, RoadId, ProgressLedger, ActualMeters, ExpectedMeters, RecoveryAttempt, Outcome (+29 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "NetworkedVehicleSeatService"
Cohesion: 0.15
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 11 - "RunFlowController"
Cohesion: 0.06
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.11
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyJoinOutcome"
Cohesion: 0.12
Nodes (16): Task, Task, LobbyCreateOutcome, LobbyId, Success, LobbyJoinFailureReason, Expired, Failed (+8 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.09
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 19 - ".Measure"
Cohesion: 0.18
Nodes (13): BoxCollider, Collider, CompiledRoadModel, GameObject, List, Scene, Vector3, VehiclePhysicsBody (+5 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "LobbyFlowController"
Cohesion: 0.10
Nodes (12): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, Difficulty, MatchSettings, AiVehicleTargetCount (+4 more)

### Community 23 - "LobbyRosterService"
Cohesion: 0.06
Nodes (23): Difficulty, FacepunchSteamLobbyPlatform, Difficulty, ISteamLobbyPlatform, LobbyMemberSnapshot, CharacterId, DisplayName, Ready (+15 more)

### Community 24 - ".Core"
Cohesion: 0.18
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 25 - "LobbyRoomService"
Cohesion: 0.14
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (21): CompiledRoadModel, Dictionary, HashSet, IList, List, MenuItem, RoadId, RoadModelValidationProfile (+13 more)

### Community 27 - "PerceivedObstacleKind"
Cohesion: 0.29
Nodes (6): PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle, WalkingPlayer

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.07
Nodes (28): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+20 more)

### Community 29 - "DefinitionId"
Cohesion: 0.05
Nodes (33): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+25 more)

### Community 30 - "RoadId"
Cohesion: 0.07
Nodes (34): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+26 more)

### Community 31 - ".PrepareStep"
Cohesion: 0.08
Nodes (26): RoadId, CollisionAnalysis, CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, CollisionResponseRequest, Facts (+18 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (32): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+24 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (23): MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing, GateAEvidenceStale, HorizonNonConforming (+15 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - ".Capture"
Cohesion: 0.13
Nodes (9): Action, KeyValuePair, MenuItem, Scene, Dictionary, MenuItem, HistoricalPairFingerprintTable, Dictionary (+1 more)

### Community 36 - ".Step"
Cohesion: 0.09
Nodes (17): DriverProfile, IReadOnlyList, JunctionControlKind, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord (+9 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.10
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.15
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.10
Nodes (22): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, HorizonEnd, ExitPortal, LookAheadLimit (+14 more)

### Community 41 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 42 - "JunctionReason"
Cohesion: 0.08
Nodes (25): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+17 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Measure"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+4 more)

### Community 45 - "PairSweep"
Cohesion: 0.25
Nodes (7): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate, RoadModelValidationProfile

### Community 46 - "TrafficV2Composition.cs"
Cohesion: 0.09
Nodes (28): GridlockEscalationTier, IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind (+20 more)

### Community 47 - "NpcReactionEffect"
Cohesion: 0.11
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 48 - ".Evaluate"
Cohesion: 0.08
Nodes (28): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite, RoadId, AuthorizedContact (+20 more)

### Community 49 - ".Add"
Cohesion: 0.16
Nodes (15): Bounds, CompiledJunctionMovement, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation (+7 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.08
Nodes (21): CompiledJunctionControl, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, List, RoadCurveSample (+13 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.27
Nodes (4): IReadOnlyList, Vector2, JunctionClearance, PoseGrid

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - ".Entry"
Cohesion: 0.29
Nodes (3): Vector3, HistoricalPairFingerprintRecord, RoadBoundsBox

### Community 54 - "TrackingTolerance"
Cohesion: 0.31
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

### Community 55 - "ImportedCurve"
Cohesion: 0.18
Nodes (12): Dictionary, IReadOnlyList, KeyValuePair, List, Predicate, RoadCurve, RoadCurveSample, RoadRecordKind (+4 more)

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.10
Nodes (19): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+11 more)

### Community 58 - "LongitudinalDecision"
Cohesion: 0.09
Nodes (24): IReadOnlyList, LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses (+16 more)

### Community 59 - "PassengerActionVerdictCode"
Cohesion: 0.05
Nodes (33): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+25 more)

### Community 60 - "GridlockSupervisor"
Cohesion: 0.19
Nodes (12): GridlockArc, Action, Dictionary, HashSet, IReadOnlyDictionary, IReadOnlyList, List, RoadId (+4 more)

### Community 61 - "TrafficV2HazardCollector"
Cohesion: 0.11
Nodes (17): TrafficHazardCollectorCounters, Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity (+9 more)

### Community 62 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 63 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 64 - ".Decide"
Cohesion: 0.07
Nodes (34): DriverProfile, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind (+26 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 66 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.11
Nodes (19): CompiledRoadModel, DriverProfile, Portal, Quaternion, RoadId, RoadLocation, Vector3, ScenarioInsertionRecord (+11 more)

### Community 68 - "IPathGeometry"
Cohesion: 0.23
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 69 - "AgentObservation"
Cohesion: 0.12
Nodes (23): Func, LaneSide, RoadId, RoadLocation, StringBuilder, Vector3, VehicleFootprint, AdjacentOccupantFact (+15 more)

### Community 70 - ".Compute"
Cohesion: 0.33
Nodes (7): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, PairGeometryFingerprint

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.09
Nodes (25): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+17 more)

### Community 73 - "PairReviewEntry"
Cohesion: 0.17
Nodes (8): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, PairReviewActions, PairReviewEntry

### Community 74 - ".Collect"
Cohesion: 0.11
Nodes (19): Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3, VehiclePhysicsBody (+11 more)

### Community 75 - ".Draw"
Cohesion: 0.27
Nodes (6): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 76 - "UserNotice"
Cohesion: 0.10
Nodes (13): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+5 more)

### Community 77 - "TrafficV2Admission"
Cohesion: 0.22
Nodes (9): GameObject, TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle, TrafficV2Verdict (+1 more)

### Community 78 - "PlanningRequest"
Cohesion: 0.18
Nodes (11): IReadOnlyList, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale, Valid, LongitudinalBounds (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.24
Nodes (6): Surface, HashSet, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "ModelDto"
Cohesion: 0.14
Nodes (14): AdjacencyDto, DrivabilityProfile, RoadModelCompiler, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto (+6 more)

### Community 81 - "JunctionTraversal"
Cohesion: 0.22
Nodes (8): TraversalComparer, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds, IEqualityComparer

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.10
Nodes (19): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+11 more)

### Community 83 - "V1ImportResult"
Cohesion: 0.14
Nodes (10): DrivabilityProfile, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, AuthoringTask, ImportedConnection, V1ImportResult (+2 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.13
Nodes (25): CompiledRoadModel, Dictionary, HashSet, IReadOnlyList, JunctionRecord, JunctionSnapshot, KeyValuePair, List (+17 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.15
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (13): JunctionRecord, RoadCurveSample, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord (+5 more)

### Community 87 - "NetworkPlayerRegistry"
Cohesion: 0.16
Nodes (6): NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 88 - "LobbyRosterScreen"
Cohesion: 0.07
Nodes (6): Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, TMP_InputField

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (19): JunctionMovement, RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 90 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.05
Nodes (43): ConditionalWeakTable, Dictionary, IReadOnlyList, JunctionControl, JunctionMovement, LaneCorridor, List, RoadId (+35 more)

### Community 93 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 94 - "V1Node"
Cohesion: 0.10
Nodes (28): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+20 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.10
Nodes (20): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+12 more)

### Community 96 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (16): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+8 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.11
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.31
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference

### Community 102 - "RoutePlan"
Cohesion: 0.12
Nodes (17): IReadOnlyList, RoadModelVersion, RoutePlan, Diagnostics, DistanceMeters, ExitPortalId, ModelId, ModelVersion (+9 more)

### Community 103 - "NetworkedPlayerState"
Cohesion: 0.16
Nodes (11): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState (+3 more)

### Community 104 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.07
Nodes (22): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+14 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.19
Nodes (5): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters, JunctionText

### Community 109 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.20
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 113 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 114 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 115 - "SignalPhaseController"
Cohesion: 0.29
Nodes (6): CompiledRoadModel, IReadOnlyList, SignalPhaseController, Current, Model, CompiledSignalPlan

### Community 116 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 117 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.14
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "TrafficJunctionOutcome"
Cohesion: 0.18
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 120 - "DriverProfile"
Cohesion: 0.11
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "ReferenceTrack"
Cohesion: 0.08
Nodes (31): IReadOnlyList, Quaternion, RoadCurve, RoadElementKind, RoadKinematicAnchor, Vector3, BodyState, GaugeBox (+23 more)

### Community 123 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.03
Nodes (64): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+56 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.17
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

### Community 128 - ".HandleRosterChanged"
Cohesion: 0.24
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.07
Nodes (23): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+15 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.10
Nodes (20): Dictionary, HashSet, List, ProfilerMarker, RoadId, Stopwatch, TrafficV2StepRunner, Collector (+12 more)

### Community 131 - ".Run"
Cohesion: 0.36
Nodes (5): CompiledRoadModel, IEnumerable, IReadOnlyList, JunctionSnapshot, TrafficV2StepCost

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (73): IList, RoadModelCanonicalPayload, Comparison, DrivabilityProfile, ImportManifest, ImportManifestEntry, Junction, JunctionControl (+65 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - "RoadRage.Shared.Networking"
Cohesion: 0.11
Nodes (13): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel (+5 more)

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "DriverProfileDef"
Cohesion: 0.10
Nodes (16): DriverProfileDef, CollisionReaction, Id, Profile, RawId, CollisionReaction, Brake, Evade (+8 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (40): CompiledConflictZone, Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto (+32 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.10
Nodes (18): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, InFallback (+10 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "OccupancyExclusion"
Cohesion: 0.40
Nodes (5): OccupancyExclusion, None, NotLocalized, OccupancyNotBounded, UndeclaredFootprint

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 147 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 148 - "ImportContext"
Cohesion: 0.24
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 149 - "SafetyReason"
Cohesion: 0.22
Nodes (9): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+1 more)

### Community 150 - "TacticalDecision"
Cohesion: 0.05
Nodes (48): CollisionGoalPhase, AwaitingRecovery, Braking, None, Reacting, RecoveryMotion, TacticalDecision, AcceptedAtFrame (+40 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 153 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 154 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - ".Bind"
Cohesion: 0.40
Nodes (4): CompiledRoadModel, GateAEvidenceBinding, Signoff, Signoff

### Community 157 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 158 - "PairReviewStatus"
Cohesion: 0.40
Nodes (5): PairReviewStatus, Modified, New, Removed, Unchanged

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 161 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 162 - ".Build"
Cohesion: 0.12
Nodes (13): JunctionKinematics, Known, JunctionPriority, JunctionMovementPosition, ConditionalWeakTable, DriverProfile, IReadOnlyList, List (+5 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 165 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (51): AppliedWidth, ConflictKind, FileLayout, Func, IEnumerable, IList, JunctionControlKind, List (+43 more)

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (21): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+13 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.07
Nodes (24): CompiledRoadModel, Dictionary, HashSet, IReadOnlyDictionary, List, RoadId, CampaignTraceability, Elements (+16 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (35): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+27 more)

### Community 170 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 178 - "JunctionRecord"
Cohesion: 0.05
Nodes (44): JunctionControlKind, RoadId, GridlockCycle, Arcs, Key, Members, GridlockEscalationTier, None (+36 more)

### Community 183 - "LaneGraphRouting"
Cohesion: 0.24
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.09
Nodes (16): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+8 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 206 - "PairReviewModel"
Cohesion: 0.25
Nodes (5): CompiledRoadModel, List, StringBuilder, PairReview, PairReviewModel

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (13): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+5 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.15
Nodes (14): Bounds, Comparison, CompiledRoadModel, IReadOnlyList, List, RoadBoundsBox, RoadId, Vector3 (+6 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.11
Nodes (11): BoxCollider, CharacterController, Collider, List, NetworkTransform, Quaternion, RaycastHit, Rigidbody (+3 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1304 isolated node(s):** `RoadRage.App`, `Instance`, `Router`, `Notices`, `Profiles` (+1299 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1797 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **6 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `RoadRage.Features.Vehicles.Traffic.Planning`, `.Run`, `TrafficV2StepRunner`, `GateAEvidenceParameters`, `RecoverySupervisor`, `JunctionActorReport`, `Blocker`, `TacticalDecision`, `.PrepareStep`, `.Step`, `CampaignTraceability`, `V2FallbackReason`, `.Evaluate`, `TrackingTolerance`, `LongitudinalDecision`, `.Decide`, `TrafficV2Insertion`, `AgentObservation`, `TrafficDecisionProjection`, `ElementOccupant`, `.Collect`, `TrafficV2Admission`, `JunctionConflictIndex`, `RoutePlan`, `NetworkedPlayerState`, `ReferenceTrack`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.257) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `TrafficV2Insertion`, `.Step`, `MonoBehaviour`, `RageRoadEventFlowController`, `TrafficV2Admission`, `TrafficV2Composition.cs`, `TrafficV2Code`, `TrafficSettingsDef`, `LaneGraph`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.170) - this node is a cross-community bridge._
- **Why does `RoadRage.Features.Vehicles.Traffic.Migration` connect `RoadRage.Features.Vehicles.Traffic.Planning` to `.Refine`, `SweepPose`, `AuthoringDecisions`, `PairReviewEntry`, `V1RoadModelImporter.cs`, `TrafficV2Composition.cs`, `.Add`, `KinematicOffsetBounds`, `.Regenerate`, `JunctionClearanceResult`, `MigrationReport`, `.Entry`, `AutomatedPairDecisionPolicy`, `.Read`, `RoadLineage`, `V1Node`?**
  _High betweenness centrality (0.094) - this node is a cross-community bridge._
- **What connects `RoadRage.App`, `Instance`, `Router` to the rest of the system?**
  _1304 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MotionPlan` be split into smaller, more focused modules?**
  _Cohesion score 0.12681159420289856 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10365853658536585 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.10512820512820513 - nodes in this community are weakly interconnected._