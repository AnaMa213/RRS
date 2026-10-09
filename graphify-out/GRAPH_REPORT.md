# Graph Report - RRS  (2026-10-09)

## Corpus Check
- 191 files · ~273,148 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 5250 nodes · 12044 edges · 205 communities (191 shown, 14 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 541 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `76bd6d11`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MotionPlan
- .Refine
- RoadRage.Features.Vehicles.Traffic.Planning
- GreyboxAssetSeedMetadata
- SweepPose
- ManeuverPath
- GateAEvidenceParameters
- .Localize
- RecoverySupervisor
- .FixedUpdate
- MonoBehaviour
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyRosterService
- LocalOnFootController
- Blocker
- RageTuningDef
- JunctionClearance
- MigrationReport
- VehiclePhysicsBody
- LobbyFlowController
- EmotionGains
- .Core
- LobbyRoomService
- AutomatedPairDecisionPolicy
- EffectiveRuleException
- V1RoadModelImporter.cs
- CharacterCatalog
- RoadId
- .Step
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- ConflictSweep
- DriverProfileDef
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- HorizonEnd
- TacticalReason
- JunctionReason
- RoadCurve
- .Measure
- ManeuverEvaluation
- TrafficV2Composition.cs
- EffectivePolicy
- NetworkedPassengerActionIntent
- JunctionActorReport
- AuthoredRoadModel
- LobbyJoinOutcome
- .Regenerate
- PairReview
- RunEscapeMenuFlowController
- DefinitionId
- .Add
- TrafficDriveOutcome
- LongitudinalDecision
- V1ImportResult
- .Monitor
- TrafficV2HazardCollector
- .Evaluate
- ObservationChannel
- .Decide
- EmotionMeters
- RoutePlan
- TrafficV2Insertion
- ManeuverCandidate
- AgentObservation
- .Compute
- TrafficDecisionProjection
- SpatialEntry
- PairReviewEntry
- SpatialQueryBuffer
- .Draw
- UserNotice
- TrafficLongitudinalOutcome
- MotionCommand
- .EvaluateManeuver
- .Compile
- JunctionTraversal
- KinematicOffsetBounds
- IRageDispositionSource
- JunctionCoordinator
- TrafficJunctionOutcome
- HistoricalMovementReader
- TrafficV2LifecycleState
- LobbyRosterScreen
- RoadModelValidationIssue
- MainMenuScreen
- IPathGeometry
- RoadLineage
- GateAEvidenceResult
- V1Node
- JunctionConflictIndex
- RightOfWayRelation
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- .Evaluate
- NetworkedPlayerReviveIntent
- ManeuverGeometryCause
- .UpdateSteeringState
- RageRoadEventFlowController
- RightOfWayTable
- JunctionDistances
- PairSweep
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- PerceivedObstacleKind
- NetworkedPlayerLifecycleIntent
- PerceptionUnavailableReason
- RouteReason
- SpeedConstraint
- LongitudinalArbitration.cs
- TrafficSettingsDef
- ManeuverVerdict
- DriverProfile
- StatusFilter
- TrackingMeasurement.cs
- RageDisposition
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- SafetyResult
- RoadRageBootstrap
- TrafficV2StepRunner
- CampaignTraceability
- TelemetrySample
- RoadModelRecords.cs
- SpeedPlan
- .ValidateApproaches
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- CollisionReactionWeights
- LeafState
- RoadModelDocument
- .CheckVisuals
- LaneNode
- RuleExceptionReason
- RoadModelCanonicalWriter
- ReferenceTrack
- JunctionClearanceResult
- ImportContext
- .IsSurfaceOnlyCollision
- TacticalDecision
- NetworkedVehicleSeatIntent
- .FingerprintWithInputs
- PathIssue
- PlayerProfileStore
- OccupancyExclusion
- .ToRecord
- TrackPiece
- PlanningDecision
- NetworkedPlayerState
- .FullPath
- .Read
- ImportedCurve
- RoutePath
- DrivingPolicyProfile
- AuthoringDecisions
- DrivabilityProfile
- ElementTrace
- .PrepareStep
- V2FallbackReason
- CompiledRoadModel
- VehicleCoverage
- TrackingTolerance
- List
- TrafficV2Code
- InterStepResult
- RoadRage.Shared.Networking
- RoadElementKind
- DriverProfileDef
- RoadRage.Features.Vehicles.Traffic.Migration
- EmotionModulation
- JunctionRecords.cs
- LaneGraphRouting
- ManeuverPhase
- TrafficRule
- .Create
- TireSample
- PlayerNameValidator
- NetworkedVehicleRecoveryIntent
- ManeuverSupervision
- LobbyPlayerSlotView
- .Measure
- RoadRage.Shared.Domain
- VehicleArcadeAssist
- LobbyCodeClipboard
- TrafficV2WorkCounters.cs
- .MeasurePath
- NetworkedPlayerPresentation
- PairDecisionState
- PairReviewModel
- .TryApproachEnd
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 151 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `TrafficFrame` - 74 edges
5. `ConflictSweep` - 72 edges
6. `ImportContext` - 67 edges
7. `NetworkedVehicleState` - 67 edges
8. `AuthoredRoadModel` - 66 edges
9. `CompiledRoadModel` - 64 edges
10. `TacticalDecision` - 60 edges

## Surprising Connections (you probably didn't know these)
- `NetworkedRageState` --references--> `EmotionMeterState`  [EXTRACTED]
  Assets/RoadRage/Features/Rage/NetworkedRageState.cs → Assets/RoadRage/Features/Rage/EmotionMeters.cs
- `TrafficV2Verdict` --references--> `VehicleCoverage`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs → Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs
- `SpeedPlan` --references--> `MotionIssue`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Planning/SpeedPlan.cs → Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs
- `TrafficV2Settings` --references--> `TrackingTolerance`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs → Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs
- `TrafficV2VehicleDriver` --references--> `TrackingTolerance`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs

## Import Cycles
- None detected.

## Communities (205 total, 14 thin omitted)

### Community 0 - "MotionPlan"
Cohesion: 0.14
Nodes (16): IReadOnlyList, PathHorizon, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue, IssueDistanceMeters (+8 more)

### Community 1 - ".Refine"
Cohesion: 0.10
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.12
Nodes (16): PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Policy, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing, RoadRage.Features.Vehicles.Traffic.Signals (+8 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.18
Nodes (15): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+7 more)

### Community 5 - "ManeuverPath"
Cohesion: 0.07
Nodes (19): RoadCurveSample, ManeuverPath, CorridorId, DepartEndSMeters, LengthMeters, MaximumAbsoluteCurvaturePerMeter, Piece, Reference (+11 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.08
Nodes (21): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+13 more)

### Community 7 - ".Localize"
Cohesion: 0.12
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - "RecoverySupervisor"
Cohesion: 0.05
Nodes (37): IReadOnlyList, List, RoadId, ProgressLedger, ActualMeters, ExpectedMeters, RecoveryAttempt, Outcome (+29 more)

### Community 9 - ".FixedUpdate"
Cohesion: 0.10
Nodes (11): Vector3, Vector3, VehicleSuspensionModel, WheelState, Compression, ContactPoint, Grounded, HubPosition (+3 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.09
Nodes (17): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+9 more)

### Community 11 - "RunFlowController"
Cohesion: 0.04
Nodes (17): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+9 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyRosterService"
Cohesion: 0.07
Nodes (18): Difficulty, FacepunchSteamLobbyPlatform, Difficulty, ISteamLobbyPlatform, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby (+10 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.11
Nodes (19): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+11 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.05
Nodes (38): NetworkVariable, NetworkedRageState, CurrentDisposition, CurrentEmotion, TMP_Text, RageStateDebugView, RageThreshold, Disposition (+30 more)

### Community 19 - "JunctionClearance"
Cohesion: 0.28
Nodes (4): IReadOnlyList, Vector2, JunctionClearance, PoseGrid

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.12
Nodes (13): RaycastHit, Rigidbody, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile (+5 more)

### Community 22 - "LobbyFlowController"
Cohesion: 0.09
Nodes (13): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, Difficulty, MatchSettings, AiVehicleTargetCount (+5 more)

### Community 23 - "EmotionGains"
Cohesion: 0.12
Nodes (14): EmotionGains, AcceptedGap, AcceptedRisk, ComfortableDeceleration, DesiredSpeed, ManeuverCost, MaxAcceleration, MinimumGap (+6 more)

### Community 24 - ".Core"
Cohesion: 0.18
Nodes (16): RouteResult, CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId (+8 more)

### Community 25 - "LobbyRoomService"
Cohesion: 0.16
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.08
Nodes (21): CompiledRoadModel, Dictionary, HashSet, IList, List, MenuItem, RoadId, RoadModelValidationProfile (+13 more)

### Community 27 - "EffectiveRuleException"
Cohesion: 0.08
Nodes (30): Active, CompiledRoadModel, IReadOnlyDictionary, IReadOnlyList, List, RoadId, Active, EffectiveRuleException (+22 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.09
Nodes (23): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+15 more)

### Community 29 - "CharacterCatalog"
Cohesion: 0.08
Nodes (21): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PersistentPlayerProfileRecord, PlayerProfile (+13 more)

### Community 30 - "RoadId"
Cohesion: 0.08
Nodes (31): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+23 more)

### Community 31 - ".Step"
Cohesion: 0.08
Nodes (27): RoadId, CollisionAnalysis, CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, CollisionResponseRequest, Facts (+19 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (33): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+25 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (23): MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing, GateAEvidenceStale, HorizonNonConforming (+15 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.08
Nodes (17): DevIndestructibleVehicle, Collider, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc, RpcParams (+9 more)

### Community 35 - "ConflictSweep"
Cohesion: 0.15
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 36 - "DriverProfileDef"
Cohesion: 0.27
Nodes (6): DriverProfileDef, CollisionReaction, Id, Policy, Profile, RawId

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.09
Nodes (9): Func, NetworkVariable, Vector3, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged (+1 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.16
Nodes (12): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+4 more)

### Community 40 - "HorizonEnd"
Cohesion: 0.67
Nodes (3): HorizonEnd, ExitPortal, LookAheadLimit

### Community 41 - "TacticalReason"
Cohesion: 0.11
Nodes (18): TacticalReason, Accepted, CollisionPreempted, ExitPortalReached, FallbackLatched, GoalAlreadyActive, InvalidRequest, ManeuverAborted (+10 more)

### Community 42 - "JunctionReason"
Cohesion: 0.08
Nodes (25): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+17 more)

### Community 43 - "RoadCurve"
Cohesion: 0.07
Nodes (33): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, PathHorizon, End (+25 more)

### Community 44 - ".Measure"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+4 more)

### Community 45 - "ManeuverEvaluation"
Cohesion: 0.19
Nodes (10): ManeuverTiming, CompiledRoadModel, EffectiveLaneCorridor, IReadOnlyList, ManeuverKind, ManeuverPath, PerceptionLimits, RoadId (+2 more)

### Community 46 - "TrafficV2Composition.cs"
Cohesion: 0.08
Nodes (30): GridlockEscalationTier, IReadOnlyList, PerceptionLimits, TrafficRule, MeasurementKind, Acceptance, Exploratory, MeasurementRun (+22 more)

### Community 47 - "EffectivePolicy"
Cohesion: 0.12
Nodes (19): DriverProfile, EmotionReading, RoadId, DrivingPolicy, DrivingSurface, Carriageway, None, OpposingCorridor (+11 more)

### Community 48 - "NetworkedPassengerActionIntent"
Cohesion: 0.04
Nodes (50): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+42 more)

### Community 49 - "JunctionActorReport"
Cohesion: 0.07
Nodes (18): JunctionPriority, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, InFallback (+10 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (35): Bounds, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList (+27 more)

### Community 51 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): Task, Task, LobbyCreateOutcome, LobbyId, Success, LobbyJoinFailureReason, Expired, Failed (+13 more)

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReview"
Cohesion: 0.26
Nodes (3): Dictionary, RoadBoundsBox, PairReview

### Community 54 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 55 - "DefinitionId"
Cohesion: 0.09
Nodes (15): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+7 more)

### Community 56 - ".Add"
Cohesion: 0.20
Nodes (4): JunctionFeature, Predicate, AuthoringTask, ImportedJunction

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.09
Nodes (20): IReadOnlyList, VehicleCoverage, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints (+12 more)

### Community 58 - "LongitudinalDecision"
Cohesion: 0.12
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 59 - "V1ImportResult"
Cohesion: 0.15
Nodes (10): DrivabilityProfile, IList, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1ImportResult, Succeeded (+2 more)

### Community 60 - ".Monitor"
Cohesion: 0.11
Nodes (13): DriverProfile, JunctionControlKind, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord, V2JunctionTrace (+5 more)

### Community 61 - "TrafficV2HazardCollector"
Cohesion: 0.07
Nodes (36): TrafficHazardCollectorCounters, Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState (+28 more)

### Community 62 - ".Evaluate"
Cohesion: 0.31
Nodes (5): RoadId, AuthorizedContact, Valid, SafetyFilter, SafetyLimits

### Community 63 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 64 - ".Decide"
Cohesion: 0.15
Nodes (14): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing (+6 more)

### Community 65 - "EmotionMeters"
Cohesion: 0.38
Nodes (4): EmotionReading, EmotionMeters, EmotionMeterState, RageTuningDef

### Community 66 - "RoutePlan"
Cohesion: 0.09
Nodes (21): CompiledRoadModel, IReadOnlyList, RoadModelVersion, RouteDiagnostic, None, ZeroWeightFallback, RoutePlan, Diagnostics (+13 more)

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.09
Nodes (26): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+18 more)

### Community 68 - "ManeuverCandidate"
Cohesion: 0.06
Nodes (29): ManeuverCandidate, Cost, DurationSeconds, ExceptionExpiryFrame, GeometryCause, Kind, OtherCorridorId, Path (+21 more)

### Community 69 - "AgentObservation"
Cohesion: 0.13
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 70 - ".Compute"
Cohesion: 0.16
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.06
Nodes (30): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+22 more)

### Community 72 - "SpatialEntry"
Cohesion: 0.09
Nodes (25): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, IntentInterval (+17 more)

### Community 74 - "SpatialQueryBuffer"
Cohesion: 0.32
Nodes (5): SpatialQueryBuffer, Capacity, Count, Saturated, Total

### Community 75 - ".Draw"
Cohesion: 0.21
Nodes (7): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, RoadModelValidationProfile, SceneView

### Community 76 - "UserNotice"
Cohesion: 0.10
Nodes (13): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+5 more)

### Community 77 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 78 - "MotionCommand"
Cohesion: 0.18
Nodes (11): DrivabilityProfile, DriverProfile, IReadOnlyList, RoadCurve, Vector3, MotionCommand, IsFinite, RuleExceptions (+3 more)

### Community 79 - ".EvaluateManeuver"
Cohesion: 0.15
Nodes (9): EffectiveLaneCorridor, IReadOnlyList, JunctionSnapshot, TrafficActor, ManeuverTrigger, CauseId, CauseKind, CauseSpeedMetersPerSecond (+1 more)

### Community 80 - ".Compile"
Cohesion: 0.09
Nodes (20): AdjacencyDto, DrivabilityProfile, RoadModelVersion, High, IsEmpty, Low, SchemaVersion, Comparison (+12 more)

### Community 81 - "JunctionTraversal"
Cohesion: 0.06
Nodes (32): TraversalComparer, IReadOnlyList, JunctionControlKind, RoadId, JunctionApproach, Crossed, JunctionBatchCounters, JunctionExitAssessment (+24 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.10
Nodes (19): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+11 more)

### Community 83 - "IRageDispositionSource"
Cohesion: 0.20
Nodes (5): BoxCollider, NetworkTransform, Rigidbody, IRageDispositionSource, CurrentDisposition

### Community 84 - "JunctionCoordinator"
Cohesion: 0.12
Nodes (26): CompiledRoadModel, Dictionary, HashSet, IReadOnlyList, JunctionRecord, JunctionSnapshot, KeyValuePair, List (+18 more)

### Community 85 - "TrafficJunctionOutcome"
Cohesion: 0.18
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (12): JunctionRecord, RoadCurveSample, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, MovementRecord (+4 more)

### Community 87 - "TrafficV2LifecycleState"
Cohesion: 0.67
Nodes (3): TrafficV2LifecycleState, Active, Faulted

### Community 88 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (11): Difficulty, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, Difficulty, Easy (+3 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.21
Nodes (10): SignalPlan, Dictionary, IReadOnlyList, List, Vector3, RoadModelCompilationException, Issues, RoadModelValidationIssue (+2 more)

### Community 90 - "MainMenuScreen"
Cohesion: 0.07
Nodes (22): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+14 more)

### Community 91 - "IPathGeometry"
Cohesion: 0.13
Nodes (12): IReadOnlyList, Vector3, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason, Vector3 (+4 more)

### Community 92 - "RoadLineage"
Cohesion: 0.14
Nodes (16): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+8 more)

### Community 93 - "GateAEvidenceResult"
Cohesion: 0.18
Nodes (11): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+3 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (30): ImportedConnection, Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List (+22 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.07
Nodes (31): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+23 more)

### Community 96 - "RightOfWayRelation"
Cohesion: 0.24
Nodes (8): Vector3, RightOfWay, RightOfWayRelation, Ambiguous, FromLeft, FromRight, Opposite, Same

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

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.16
Nodes (10): IReadOnlyList, Vector3, AiRageTargetResolution, Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, NetworkVariable (+2 more)

### Community 102 - ".Evaluate"
Cohesion: 0.10
Nodes (20): LongitudinalBounds, Valid, DriverProfile, PlanningReach, IReadOnlyList, ProfilerMarker, PlanningRequest, PlanningSpine (+12 more)

### Community 103 - "NetworkedPlayerReviveIntent"
Cohesion: 0.20
Nodes (7): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, HostOwnedNetworkStateBehaviour, IsHostAuthority, NetworkBehaviour

### Community 104 - "ManeuverGeometryCause"
Cohesion: 0.10
Nodes (19): DrivabilityProfile, EffectiveLaneCorridor, ManeuverGeometryCause, AdjacencyNotCovered, CorridorEnd, CorridorTooShort, EnvelopeExceeded, EnvelopeNotContiguous (+11 more)

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "RightOfWayTable"
Cohesion: 0.25
Nodes (9): ConditionalWeakTable, IReadOnlyList, RoadId, Entry, RightOfWayTable, AmbiguousCount, CanonicalText, Entries (+1 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "PairSweep"
Cohesion: 0.24
Nodes (8): RoadBoundsBox, ConflictCandidate, IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (13): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, Vector3 (+5 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.20
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - "PerceivedObstacleKind"
Cohesion: 0.14
Nodes (15): PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle, WalkingPlayer, IReadOnlyList, RoadCurve (+7 more)

### Community 113 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 114 - "PerceptionUnavailableReason"
Cohesion: 0.40
Nodes (5): PerceptionUnavailableReason, ChannelSaturated, ChannelUnavailable, HazardCollectorSaturated, None

### Community 115 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 116 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 117 - "LongitudinalArbitration.cs"
Cohesion: 0.13
Nodes (20): RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalMemory, None, LongitudinalObstacle, StopHoldPhase (+12 more)

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "ManeuverVerdict"
Cohesion: 0.20
Nodes (10): ManeuverVerdict, ExceptionDenied, ExceptionPending, Feasible, GeometryInfeasible, NotOffered, PolicyRefused, SafetyRejected (+2 more)

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "TrackingMeasurement.cs"
Cohesion: 0.29
Nodes (8): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound, TrackingMeasurement

### Community 123 - "RageDisposition"
Cohesion: 0.18
Nodes (8): EmotionModulation, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated, Ram

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.03
Nodes (66): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+58 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.15
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "SafetyResult"
Cohesion: 0.08
Nodes (23): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+15 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.06
Nodes (42): GridlockArc, GridlockCycle, Arcs, Key, Members, CompiledRoadModel, Dictionary, HashSet (+34 more)

### Community 131 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 132 - "TelemetrySample"
Cohesion: 0.20
Nodes (8): TelemetrySample, TelemetrySample, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees, Speed

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (78): JunctionMovement, RoadModelCanonicalPayload, DrivabilityProfile, ImportManifest, ImportManifestEntry, Junction, JunctionControl, JunctionControlKind (+70 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.07
Nodes (36): DriverProfile, IReadOnlyList, PathHorizon, RoadId, DeferredLimit, DeferredLimitKind, RoadLimit, DeferredLimitState (+28 more)

### Community 135 - ".ValidateApproaches"
Cohesion: 0.25
Nodes (7): Dictionary, JunctionControl, JunctionMovement, LaneCorridor, List, RoadModelSource, RoadModelValidationIssue

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "CollisionReactionWeights"
Cohesion: 0.16
Nodes (11): CollisionReaction, Brake, Evade, MisReact, CollisionReactionWeights, Brake, Default, Evade (+3 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.06
Nodes (40): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+32 more)

### Community 142 - ".CheckVisuals"
Cohesion: 0.24
Nodes (6): Surface, HashSet, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "RuleExceptionReason"
Cohesion: 0.14
Nodes (14): RuleExceptionReason, Accepted, AlreadyActive, Expired, FrameUnavailable, Held, Malformed, RequesterAbsent (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "ReferenceTrack"
Cohesion: 0.21
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 147 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 148 - "ImportContext"
Cohesion: 0.26
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 150 - "TacticalDecision"
Cohesion: 0.04
Nodes (47): ManeuverKind, ManeuverPath, RoadId, CollisionGoalPhase, AwaitingRecovery, Braking, None, Reacting (+39 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 153 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 154 - "PlayerProfileStore"
Cohesion: 0.22
Nodes (5): PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection

### Community 155 - "OccupancyExclusion"
Cohesion: 0.40
Nodes (5): OccupancyExclusion, None, NotLocalized, OccupancyNotBounded, UndeclaredFootprint

### Community 156 - ".ToRecord"
Cohesion: 0.50
Nodes (3): FileLayout, FileRecord, FileRecord

### Community 157 - "TrackPiece"
Cohesion: 0.19
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 158 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 159 - "NetworkedPlayerState"
Cohesion: 0.12
Nodes (12): IEnumerable, NetworkedPlayerLifecycleService, Instance, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerLifecycle (+4 more)

### Community 160 - ".FullPath"
Cohesion: 0.23
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 161 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 162 - "ImportedCurve"
Cohesion: 0.15
Nodes (16): Dictionary, IReadOnlyList, KeyValuePair, List, RoadCurve, RoadCurveSample, RoadRecordKind, ImportedCurve (+8 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - "DrivingPolicyProfile"
Cohesion: 0.11
Nodes (18): DrivingPolicyProfile, AcceptedGapSeconds, AcceptedRisk, AllowedSurfaces, Default, RoutePreferenceWeight, ManeuverKind, AdjacentCorridor (+10 more)

### Community 165 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (50): ConflictKind, Dictionary, FileLayout, Func, IEnumerable, IList, JunctionControlKind, List (+42 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - ".PrepareStep"
Cohesion: 0.10
Nodes (13): StepContactAccumulator, DrivabilityProfile, VehicleProfile, EmotionGovernor, Escape, FearSaturated, Rage, EmotionReading (+5 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (33): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+25 more)

### Community 171 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 172 - "TrackingTolerance"
Cohesion: 0.31
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 175 - "InterStepResult"
Cohesion: 0.17
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 176 - "RoadRage.Shared.Networking"
Cohesion: 0.10
Nodes (13): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel (+5 more)

### Community 182 - "JunctionRecords.cs"
Cohesion: 0.07
Nodes (29): GridlockEscalationTier, None, PrecedenceRelaxation, GridlockOutcome, Escalated, Exhausted, Progressed, GridlockResolution (+21 more)

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 184 - "ManeuverPhase"
Cohesion: 0.33
Nodes (6): ManeuverPhase, Departure, None, Passing, Return, Settling

### Community 185 - "TrafficRule"
Cohesion: 0.20
Nodes (10): TrafficRule, FollowingGap, JunctionControl, KeepClear, LaneChange, None, OpposingCorridor, Sidewalk (+2 more)

### Community 186 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 187 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "ManeuverSupervision"
Cohesion: 0.50
Nodes (4): ManeuverSupervision, Abort, Commit, Continue

### Community 191 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 192 - ".Measure"
Cohesion: 0.15
Nodes (14): BoxCollider, Collider, CompiledRoadModel, GameObject, List, Scene, Vector3, VehiclePhysicsBody (+6 more)

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (19): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+11 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 205 - "PairDecisionState"
Cohesion: 0.33
Nodes (6): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed

### Community 206 - "PairReviewModel"
Cohesion: 0.23
Nodes (10): CompiledRoadModel, List, RoadId, StringBuilder, PairReviewModel, PairReviewStatus, Modified, New (+2 more)

### Community 213 - ".TryApproachEnd"
Cohesion: 0.50
Nodes (3): CompiledJunctionControl, CompiledRoadModel, RoadCurveSample

### Community 216 - "LaneGraph"
Cohesion: 0.13
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.13
Nodes (18): Vector3, ObstacleFact, InSweptPath, Bounds, Comparison, CompiledRoadModel, IReadOnlyList, List (+10 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.12
Nodes (9): CharacterController, Collider, IReadOnlyList, List, Quaternion, RaycastHit, Vector3, NetworkedAIVehicleDriverController (+1 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1499 isolated node(s):** `Covered`, `NotCoveredByGateA`, `GateAEvidenceMissing`, `GateAEvidenceStale`, `NotEstablished` (+1494 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 2028 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **14 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `SafetyResult`, `TrafficV2StepRunner`, `RoadRage.Features.Vehicles.Traffic.Planning`, `GateAEvidenceParameters`, `RecoverySupervisor`, `Blocker`, `ReferenceTrack`, `TacticalDecision`, `EffectiveRuleException`, `.Step`, `.PrepareStep`, `V2FallbackReason`, `TrackingTolerance`, `InterStepResult`, `EffectivePolicy`, `JunctionActorReport`, `LongitudinalDecision`, `.Monitor`, `TrafficV2HazardCollector`, `.Evaluate`, `ObservationChannel`, `RoutePlan`, `TrafficV2Insertion`, `ManeuverCandidate`, `AgentObservation`, `TrafficDecisionProjection`, `SpatialQueryBuffer`, `.EvaluateManeuver`, `TrafficV2LifecycleState`, `TrafficPerception`, `NetworkedPlayerReviveIntent`, `PerceivedObstacleKind`, `LongitudinalArbitration.cs`, `TrackingMeasurement.cs`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.244) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `TrafficV2Insertion`, `RageRoadEventFlowController`, `MonoBehaviour`, `TrafficV2Composition.cs`, `TrafficV2Code`, `TrafficSettingsDef`, `LaneGraph`, `.Monitor`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.174) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `TrafficV2Insertion`, `RoadModelRecords.cs`, `SpeedPlan`, `.Evaluate`, `.Localize`, `RoadCurve`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `RoadLineage`, `TrafficV2HazardCollector`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.129) - this node is a cross-community bridge._
- **What connects `Covered`, `NotCoveredByGateA`, `GateAEvidenceMissing` to the rest of the system?**
  _1499 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MotionPlan` be split into smaller, more focused modules?**
  _Cohesion score 0.14285714285714285 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10365853658536585 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.12427409988385599 - nodes in this community are weakly interconnected._