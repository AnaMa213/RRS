# Graph Report - RRS  (2026-10-08)

## Corpus Check
- 183 files · ~253,938 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4874 nodes · 11051 edges · 202 communities (179 shown, 23 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 487 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `693accab`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MotionPlan
- List
- RoadRage.Features.Vehicles.Traffic.Planning
- GreyboxAssetSeedMetadata
- SweepPose
- RoutePlan
- GateAEvidenceParameters
- .TryGetMovement
- RecoverySupervisor
- VehicleSuspensionModel
- NetworkedVehicleSeatService
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- LobbyRosterService
- LocalOnFootController
- Blocker
- RageTuningDef
- .Measure
- MigrationReport
- VehiclePhysicsBody
- .Add
- TrackPiece
- .Core
- LobbyRoomService
- AutomatedPairDecisionPolicy
- PerceivedObstacleKind
- V1RoadModelImporter.cs
- MainMenuProfileFlowController
- RoadId
- CollisionFacts
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .FullPath
- .Step
- NetworkedVehicleState
- VehicleProfile
- V1ImportResult
- PathHorizon
- MonoBehaviour
- JunctionReason
- RoadCurve
- .Measure
- PairSweep
- MeasurementRun
- NpcReactionEffect
- SafetyResult
- DefinitionId
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- PairReview
- .PrepareStep
- ImportedCurve
- V2StepRecord
- TrafficDriveOutcome
- MovementRole
- PassengerActionVerdictCode
- .GridPaths
- .Evaluate
- NetworkedPlayerState
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
- .Record
- TrafficV2HazardCollector
- .Draw
- UserNotice
- LongitudinalArbitration.cs
- JunctionSnapshot
- .CheckVisuals
- RoadModelSource
- CampaignTraceability
- KinematicOffsetBounds
- InterStepResult
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- ReferenceTrack
- LobbyFlowController
- RoadModelValidationIssue
- AIVehicleBehaviorDebugView
- LobbyPlayerSlotView
- RoadLineage
- RageDisposition
- V1Node
- JunctionConflictIndex
- TrafficLongitudinalOutcome
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- IRageDispositionSource
- NetworkedPlayerReviveIntent
- .FingerprintWithInputs
- .UpdateSteeringState
- RageRoadEventFlowController
- MainMenuScreen
- JunctionDistances
- .ResolveNextNode
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- JunctionRequestRejection
- NetworkedPlayerLifecycleIntent
- RoadModelVersion
- DriverProfileDef
- LongitudinalDecision
- JunctionClearanceResult
- TrafficSettingsDef
- V2ComposerDiagnostic
- DriverProfile
- StatusFilter
- MatchSettings
- HostOwnedNetworkStateBehaviour
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- .Track
- RoadRageBootstrap
- TrafficV2StepRunner
- TrafficV2Settings
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- VehicleDamageType
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- CollisionReactionWeights
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- .MeasurePath
- RoadModelCanonicalWriter
- SpeedConstraint
- BoxCollider
- ImportContext
- StopHoldState
- TacticalDecision
- NetworkedVehicleSeatIntent
- Collider
- TrackingMeasurement.cs
- Collision
- .Read
- TrafficJunctionOutcome
- SpeedPlanIssue
- Dictionary
- NetworkedPlayerLifecycleService
- PlayerProfileBootstrapService
- DriverProfile
- .Build
- RoutePath
- DriverProfileDef
- AuthoringDecisions
- NetworkedPassengerActionIntent
- ElementTrace
- JunctionControlKind
- V2FallbackReason
- TireSample
- JunctionSnapshot
- Portal
- ProfilerMarker
- TrafficV2Code
- Rigidbody
- Stopwatch
- VehicleCoverage
- JunctionTraversal
- .AccumulateStep
- Transform
- Vector3
- VehicleCoverage
- LaneGraphRouting
- VehicleDriveIntent
- VehicleFootprint
- PairDecisionState
- VehicleFootprintPose
- VehicleArcadeAssist
- NetworkedVehicleRecoveryIntent
- VehiclePhysicsBody
- RoadRage.Shared.Domain
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- FileLayout
- PairReviewModel
- NetworkedBossState.cs
- NetworkedCrewEconomyState.cs
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 129 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `TrafficFrame` - 60 edges
10. `NetworkedVehicleDriverController` - 58 edges

## Surprising Connections (you probably didn't know these)
- `PortalTrafficSpawner` --references--> `V2DriveRecord`  [EXTRACTED]
  Assets/RoadRage/App/Run/PortalTrafficSpawner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs
- `PortalTrafficSpawner` --references--> `TrafficV2VehicleDriver`  [EXTRACTED]
  Assets/RoadRage/App/Run/PortalTrafficSpawner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs
- `TrafficV2StepRunner` --references--> `TrafficV2VehicleDriver`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs
- `TrafficV2VehicleDriver` --references--> `RecoveryRequest`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs
- `TrafficV2VehicleDriver` --references--> `RecoverySupervisor`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs

## Import Cycles
- None detected.

## Communities (202 total, 23 thin omitted)

### Community 0 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 1 - "List"
Cohesion: 0.12
Nodes (15): CompiledJunctionMovement, ConflictKind, IList, List, StringBuilder, Vector2, PairRefinement, RefineContext (+7 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.10
Nodes (14): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning (+6 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.17
Nodes (14): CompiledRoadModel, Dictionary, IReadOnlyList, RoadCurve, RoadCurveSample, RoadId, ShortElement, SweepElement (+6 more)

### Community 5 - "RoutePlan"
Cohesion: 0.05
Nodes (41): CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation, RoadModelVersion, DecisionCounter, RouteDiagnostic, None (+33 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (19): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+11 more)

### Community 7 - ".TryGetMovement"
Cohesion: 0.09
Nodes (26): CompiledJunctionControl, CompiledRoadModel, RoadCurveSample, ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion (+18 more)

### Community 8 - "RecoverySupervisor"
Cohesion: 0.05
Nodes (42): Blocker, IReadOnlyList, List, LongitudinalDecision, RoadId, TacticalResponse, TrafficFrame, ProgressLedger (+34 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.08
Nodes (16): TelemetrySample, Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip (+8 more)

### Community 10 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (13): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+5 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.10
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "RoadRage.Features.Online"
Cohesion: 0.07
Nodes (16): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+8 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyRosterService"
Cohesion: 0.04
Nodes (39): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+31 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.09
Nodes (15): List, RageTuningCatalog, Count, RageTuningDef, FearSensitivity, HonkChannel, HonkMagnitude, HonkRange (+7 more)

### Community 19 - ".Measure"
Cohesion: 0.15
Nodes (15): BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, Scene, Vector3 (+7 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.13
Nodes (12): Rigidbody, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile, Profile (+4 more)

### Community 22 - ".Add"
Cohesion: 0.16
Nodes (12): IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation, RoadModelValidationProfile, Vector3 (+4 more)

### Community 23 - "TrackPiece"
Cohesion: 0.19
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 24 - ".Core"
Cohesion: 0.19
Nodes (16): RouteResult, CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId (+8 more)

### Community 25 - "LobbyRoomService"
Cohesion: 0.16
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.08
Nodes (21): CompiledRoadModel, Dictionary, HashSet, IList, List, MenuItem, RoadId, RoadModelValidationProfile (+13 more)

### Community 27 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.08
Nodes (25): AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior (+17 more)

### Community 29 - "MainMenuProfileFlowController"
Cohesion: 0.15
Nodes (11): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, Exception (+3 more)

### Community 30 - "RoadId"
Cohesion: 0.08
Nodes (30): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+22 more)

### Community 31 - "CollisionFacts"
Cohesion: 0.09
Nodes (26): RoadId, CollisionAnalysis, CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, CollisionResponseRequest, Facts (+18 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (33): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+25 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - ".FullPath"
Cohesion: 0.19
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 36 - ".Step"
Cohesion: 0.10
Nodes (16): List, TrafficFrame, DrivabilityProfile, RecoveryCommandInput, ComposedDrive, DriverProfile, HazardQueryReport, JunctionActorReport (+8 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "V1ImportResult"
Cohesion: 0.14
Nodes (15): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+7 more)

### Community 40 - "PathHorizon"
Cohesion: 0.07
Nodes (31): CompiledRoadModel, DriverProfile, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, HorizonEnd, ExitPortal (+23 more)

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
Cohesion: 0.22
Nodes (7): IList, List, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "MeasurementRun"
Cohesion: 0.10
Nodes (21): IReadOnlyList, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label, MaxPopulation (+13 more)

### Community 47 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 48 - "SafetyResult"
Cohesion: 0.08
Nodes (23): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+15 more)

### Community 49 - "DefinitionId"
Cohesion: 0.11
Nodes (14): List, CharacterCatalog, Count, Color, GameObject, CharacterDef, DisplayName, Id (+6 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.08
Nodes (20): CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, List (+12 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReview"
Cohesion: 0.26
Nodes (3): Dictionary, RoadBoundsBox, PairReview

### Community 54 - ".PrepareStep"
Cohesion: 0.11
Nodes (12): StepContactAccumulator, TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared, BodyState, ProfilerMarker (+4 more)

### Community 55 - "ImportedCurve"
Cohesion: 0.16
Nodes (13): Dictionary, IReadOnlyList, JunctionFeature, KeyValuePair, List, Predicate, RoadCurve, RoadRecordKind (+5 more)

### Community 56 - "V2StepRecord"
Cohesion: 0.08
Nodes (27): IReadOnlyList, RoadId, TrafficV2LifecycleState, Active, Faulted, V2DriveRecord, V2InteractionRecord, V2JunctionTrace (+19 more)

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.10
Nodes (19): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+11 more)

### Community 58 - "MovementRole"
Cohesion: 0.29
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 59 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+19 more)

### Community 60 - ".GridPaths"
Cohesion: 0.18
Nodes (8): Vector2, GridPath, PoseFrame, Vector2, KinematicPoseSet, PoseGrid, RoadModelValidationProfile, PoseFrame

### Community 61 - ".Evaluate"
Cohesion: 0.20
Nodes (8): MotionCommand, IsFinite, RoadId, AuthorizedContact, Valid, NearFieldSample, SafetyFilter, SafetyLimits

### Community 62 - "NetworkedPlayerState"
Cohesion: 0.14
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 63 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 64 - ".Decide"
Cohesion: 0.15
Nodes (14): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing (+6 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.20
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 66 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.05
Nodes (44): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+36 more)

### Community 68 - "IPathGeometry"
Cohesion: 0.12
Nodes (13): Vector3, Vector3, IPathGeometry, LengthMeters, Spans, PlanningDecision, Motion, Observation (+5 more)

### Community 69 - "AgentObservation"
Cohesion: 0.14
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 70 - ".Compute"
Cohesion: 0.19
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.08
Nodes (29): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementOccupant, IntentInterval (+21 more)

### Community 73 - ".Record"
Cohesion: 0.13
Nodes (14): JunctionRecord, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations, RouteEnd (+6 more)

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.07
Nodes (36): TrafficHazardCollectorCounters, Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState (+28 more)

### Community 75 - ".Draw"
Cohesion: 0.20
Nodes (6): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 76 - "UserNotice"
Cohesion: 0.09
Nodes (13): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+5 more)

### Community 77 - "LongitudinalArbitration.cs"
Cohesion: 0.15
Nodes (17): RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalMemory, None, LongitudinalObstacle, LongitudinalPerception (+9 more)

### Community 78 - "JunctionSnapshot"
Cohesion: 0.16
Nodes (8): IReadOnlyList, JunctionSnapshot, JunctionBatchCounters, JunctionSnapshot, Counters, EffectiveFrame, Records, SourceFrame

### Community 79 - ".CheckVisuals"
Cohesion: 0.25
Nodes (5): Surface, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "RoadModelSource"
Cohesion: 0.09
Nodes (27): AdjacencyDto, DrivabilityProfile, EffectiveLaneCorridor, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, JunctionMovement, RoadModelCanonicalPayload (+19 more)

### Community 81 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.10
Nodes (18): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+10 more)

### Community 83 - "InterStepResult"
Cohesion: 0.18
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.14
Nodes (21): CompiledRoadModel, Dictionary, HashSet, KeyValuePair, List, RoadId, Grant, JunctionCoordinator (+13 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.18
Nodes (12): ConflictSweep, RoadId, RoadModelValidationProfile, MovementSide, RefineNode, RefineSegment, GridPath, LeafState (+4 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (12): JunctionRecord, RoadCurveSample, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, MovementRecord (+4 more)

### Community 87 - "ReferenceTrack"
Cohesion: 0.19
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 88 - "LobbyFlowController"
Cohesion: 0.04
Nodes (19): Difficulty, HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, Button (+11 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (19): JunctionControl, RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 90 - "AIVehicleBehaviorDebugView"
Cohesion: 0.24
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.05
Nodes (43): ConditionalWeakTable, Dictionary, IReadOnlyList, JunctionControl, JunctionMovement, LaneCorridor, List, RoadId (+35 more)

### Community 93 - "RageDisposition"
Cohesion: 0.18
Nodes (10): RageThreshold, Disposition, MinValue, RageDisposition, Block, Calm, ConfrontationCapable, Flee (+2 more)

### Community 94 - "V1Node"
Cohesion: 0.10
Nodes (29): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+21 more)

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
Cohesion: 0.10
Nodes (19): Bounds, OverlayInstance, Color, HashSet, List, MenuItem, RoadId, SceneView (+11 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.31
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference

### Community 102 - "IRageDispositionSource"
Cohesion: 0.20
Nodes (5): BoxCollider, NetworkTransform, Rigidbody, IRageDispositionSource, CurrentDisposition

### Community 103 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

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
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

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

### Community 115 - "DriverProfileDef"
Cohesion: 0.27
Nodes (6): DriverProfileDef, CollisionReaction, Id, Profile, RawId, ScriptableObject

### Community 116 - "LongitudinalDecision"
Cohesion: 0.14
Nodes (14): IReadOnlyList, LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses (+6 more)

### Community 117 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "V2ComposerDiagnostic"
Cohesion: 0.29
Nodes (7): V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None, ProfileScalarNonFinite, RollingBackward

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "MatchSettings"
Cohesion: 0.29
Nodes (6): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, RoadRage.Features.Lobby

### Community 123 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.29
Nodes (5): HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority, NetworkBehaviour

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.03
Nodes (77): AgentObservation, Blocker, LongitudinalDecision, TacticalResponse, TrafficV2VehicleDriver, Blockers, ContactEpisodes, Contacts (+69 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.15
Nodes (8): PairReviewActions, PairReviewEntry, IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - ".Track"
Cohesion: 0.39
Nodes (4): DrivabilityProfile, DriverProfile, RoadCurve, Vector3

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.05
Nodes (31): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+23 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.08
Nodes (28): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+20 more)

### Community 131 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (69): CompiledConflictZone, IList, Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile (+61 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - "VehicleDamageType"
Cohesion: 0.50
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.25
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "CollisionReactionWeights"
Cohesion: 0.16
Nodes (11): CollisionReaction, Brake, Evade, MisReact, CollisionReactionWeights, Brake, Default, Evade (+3 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.08
Nodes (21): JunctionKinematics, Known, JunctionPriority, IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners (+13 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 148 - "ImportContext"
Cohesion: 0.14
Nodes (9): DrivabilityProfile, RoadBoundsBox, RoadCurvePoint, RoadCurveSample, Vector3, CircleFit, ImportContext, V1RoadModelImporter (+1 more)

### Community 149 - "StopHoldState"
Cohesion: 0.12
Nodes (13): StopHoldPhase, Entered, Holding, None, Released, StopHoldRelease, GapOpened, GrantEffective (+5 more)

### Community 150 - "TacticalDecision"
Cohesion: 0.05
Nodes (48): CollisionGoalPhase, AwaitingRecovery, Braking, None, Reacting, RecoveryMotion, TacticalDecision, AcceptedAtFrame (+40 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 153 - "TrackingMeasurement.cs"
Cohesion: 0.29
Nodes (8): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound, TrackingMeasurement

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "TrafficJunctionOutcome"
Cohesion: 0.18
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 157 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 160 - "PlayerProfileBootstrapService"
Cohesion: 0.19
Nodes (8): PlayerNameValidator, PlayerProfileBootstrapService, PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist, PlayerProfileResolution

### Community 162 - ".Build"
Cohesion: 0.22
Nodes (9): ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId, Vector3, JunctionRequestBuilder, RoadElementKind (+1 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 165 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (47): AppliedWidth, ConflictKind, Dictionary, FileLayout, Func, IEnumerable, IList, JunctionControlKind (+39 more)

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.07
Nodes (24): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+16 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.08
Nodes (29): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2FallbackReason, ExitPortalReached, Faulted, FrameUnavailable, HorizonNonConforming (+21 more)

### Community 170 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 177 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 178 - "JunctionTraversal"
Cohesion: 0.09
Nodes (22): TraversalComparer, JunctionControlKind, RoadId, JunctionApproach, Crossed, JunctionExitAssessment, JunctionMovementPosition, JunctionMovementStatus (+14 more)

### Community 179 - ".AccumulateStep"
Cohesion: 0.33
Nodes (3): V2ContactEpisode, Collision, Transform

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 186 - "PairDecisionState"
Cohesion: 0.33
Nodes (6): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (17): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+9 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 202 - "FileLayout"
Cohesion: 0.33
Nodes (6): FileLayout, ConflictRecord, ControlRecord, DeferredRecord, DispositionRecord, WidthRecord

### Community 206 - "PairReviewModel"
Cohesion: 0.23
Nodes (10): CompiledRoadModel, List, RoadId, StringBuilder, PairReviewModel, PairReviewStatus, Modified, New (+2 more)

### Community 208 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 209 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 216 - "LaneGraph"
Cohesion: 0.13
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.20
Nodes (10): Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance, PerceptionLimits, TrafficPerception (+2 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.14
Nodes (8): CharacterController, Collider, List, Quaternion, RaycastHit, Vector3, NetworkedAIVehicleDriverController, HasReachedExitPortal

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1293 isolated node(s):** `Active`, `Faulted`, `SpeedRatio`, `TotalMillisecondsPerStep`, `Footprint` (+1288 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1832 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **23 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `.Step`, `GateAEvidenceParameters`, `RecoverySupervisor`, `TrafficV2HazardCollector`, `.AccumulateStep`, `.PrepareStep`, `V2StepRecord`, `HostOwnedNetworkStateBehaviour`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.136) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `TrafficV2Insertion`, `MonoBehaviour`, `RageRoadEventFlowController`, `MeasurementRun`, `TrafficV2Code`, `TrafficSettingsDef`, `LaneGraph`, `V2StepRecord`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.135) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `TrafficV2Insertion`, `RoadModelRecords.cs`, `.TryGetMovement`, `TrafficV2HazardCollector`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadModelSource`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `RoadLineage`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.131) - this node is a cross-community bridge._
- **What connects `Active`, `Faulted`, `SpeedRatio` to the rest of the system?**
  _1293 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MotionPlan` be split into smaller, more focused modules?**
  _Cohesion score 0.12681159420289856 - nodes in this community are weakly interconnected._
- **Should `List` be split into smaller, more focused modules?**
  _Cohesion score 0.12258064516129032 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.10099573257467995 - nodes in this community are weakly interconnected._