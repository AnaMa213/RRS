# Graph Report - RRS  (2026-10-09)

## Corpus Check
- 188 files · ~269,354 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 5308 nodes · 11850 edges · 232 communities (193 shown, 39 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 533 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `90d40171`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MotionPlan
- .Refine
- RoadRage.Features.Vehicles.Traffic.Planning
- MonoBehaviour
- ConflictSweep
- ManeuverPath
- GateAEvidenceParameters
- .TryGetMovement
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
- EffectiveRuleException
- V1RoadModelImporter.cs
- MainMenuProfileFlowController
- RoadId
- CollisionFacts
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .Run
- .Monitor
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- TacticalReason
- JunctionReason
- RoadCurve
- .Measure
- ManeuverSituation
- TrafficV2Composition.cs
- EffectivePolicy
- DefinitionId
- JunctionSnapshot
- AuthoredRoadModel
- TrafficV2StepRunner
- RoadRage.Features.Vehicles.Traffic.Migration
- PairReviewModel
- .ObserveTrackingTolerance
- ImportedCurve
- .Add
- TrafficDriveOutcome
- LongitudinalDecision
- NetworkedPassengerActionIntent
- .ZoneSpan
- TrafficV2HazardCollector
- NetworkedPlayerState
- ObservationChannel
- .Decide
- .FixedUpdate
- NpcReactionEffect
- TrafficV2Insertion
- ManeuverCandidate
- AgentObservation
- .Compute
- TrafficDecisionProjection
- RoadId
- PairReviewEntry
- .ObserveAlong
- .Draw
- UserNotice
- PassengerActionVerdictCode
- MotionCommand
- .NearField
- ModelDto
- JunctionTraversal
- KinematicOffsetBounds
- V1ImportResult
- JunctionCoordinator
- NetworkedPlayerSpawnService
- HistoricalMovementReader
- NetworkPlayerRegistry
- LobbyRosterScreen
- RoadModelValidationIssue
- MainMenuScreen
- IPathGeometry
- RoadLineage
- GateAEvidenceResult
- V1Node
- JunctionConflictIndex
- TrafficLongitudinalOutcome
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- RoutePlan
- NetworkedPlayerReviveIntent
- ManeuverGeometryCause
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- NetworkedRunSessionMonitor
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- HazardRootClass
- NetworkedPlayerLifecycleIntent
- RoadModelVersion
- .PrepareStep
- SpeedConstraint
- StopHoldState
- TrafficSettingsDef
- TrafficJunctionOutcome
- DriverProfile
- StatusFilter
- TrackingMeasurement.cs
- AIVehicleBehaviorDebugView
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- .Evaluate
- RoadRageBootstrap
- GridlockSupervisor
- CampaignTraceability
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- RoadRage.Shared.Definitions
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- DriverProfileDef
- LeafState
- RoadModelDocument
- .GapSeconds
- LaneNode
- RuleExceptionReason
- RoadModelCanonicalWriter
- ReferenceTrack
- TrafficV2Settings
- ImportContext
- SafetyReason
- TacticalDecision
- NetworkedVehicleSeatIntent
- .Run
- PathIssue
- SignalPhaseController
- ManeuverEvaluation.cs
- RightOfWayRelation
- TrackPiece
- RuleExceptionRecord
- NetworkedPlayerLifecycleService
- RightOfWayTable
- TrafficV2Admission
- .Build
- RoutePath
- DrivingPolicyProfile
- AuthoringDecisions
- .ProjectEntry
- ElementTrace
- VehicleDriveIntent
- V2FallbackReason
- TireSample
- RuleExceptionRequest
- PlayerNameValidator
- TrafficRule
- TrafficV2Code
- InterStepResult
- RouteReason
- ManeuverVerdict
- JunctionActorReport
- MainMenuFlowController
- .Step
- .ToText
- JunctionRequestRejection
- LaneGraphRouting
- RunEscapeMenuScreen
- .ValidateApproaches
- .Create
- LongitudinalArbitration.cs
- VehicleArcadeAssist
- NetworkedVehicleRecoveryIntent
- V2ComposerDiagnostic
- .TryGetCorridor
- .StaticBodyClearance
- RoadRage.Shared.Domain
- .ToRecord
- .TryApproachEnd
- LobbyCodeClipboard
- RoadElementKind
- GameObject
- GridlockEscalationTier
- NetworkedPlayerPresentation
- Quaternion
- RoadLocation
- TrafficRule
- HashSet
- IEnumerable
- PairReviewStatus
- JunctionSnapshot
- Collision
- BoxCollider
- Collider
- DriverProfileDef
- JunctionControlKind
- JunctionSnapshot
- Rigidbody
- Transform
- LaneGraph
- TrafficPerception
- VehicleCoverage
- VehicleDriveIntent
- VehicleFootprint
- VehicleFootprintPose
- VehiclePhysicsBody
- Bounds
- Comparison
- RoadBoundsBox
- PerceptionLimits
- RouteOutcome
- RouteReason
- TrafficActor
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 148 edges
2. `RoadId` - 102 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `TacticalDecision` - 59 edges
10. `NetworkedVehicleDriverController` - 58 edges

## Surprising Connections (you probably didn't know these)
- `TrafficV2HazardCollector` --references--> `TrafficHazardCollectorCounters`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficV2VehicleDriver` --references--> `VehicleCoverage`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficV2VehicleDriver` --references--> `TrafficDecisionProjection`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `PlanningDecision` --references--> `TrafficDecisionProjection`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `PortalTrafficSpawner` --references--> `TrafficComposition`  [EXTRACTED]
  Assets/RoadRage/App/Run/PortalTrafficSpawner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs

## Import Cycles
- None detected.

## Communities (232 total, 39 thin omitted)

### Community 0 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 1 - ".Refine"
Cohesion: 0.07
Nodes (26): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+18 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.08
Nodes (17): TrafficV2Work, TrafficV2WorkCounters, DriverProfile, HorizonEnd, ExitPortal, LookAheadLimit, PlanningReach, PlanningTolerances (+9 more)

### Community 3 - "MonoBehaviour"
Cohesion: 0.09
Nodes (15): Color, TMP_Text, LobbyPlayerSlotView, DevIndestructibleVehicle, GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy (+7 more)

### Community 4 - "ConflictSweep"
Cohesion: 0.10
Nodes (28): RoadModelValidationProfile, CompiledRoadModel, Dictionary, IList, IReadOnlyList, List, RoadBoundsBox, RoadCurve (+20 more)

### Community 5 - "ManeuverPath"
Cohesion: 0.05
Nodes (31): DrivabilityProfile, IReadOnlyList, PerceivedObstacleKind, ReferenceTrack, RoadCurve, RoadId, Vector3, ManeuverObstacle (+23 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - ".TryGetMovement"
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - "RecoverySupervisor"
Cohesion: 0.05
Nodes (39): IReadOnlyList, List, RoadId, ProgressLedger, ActualMeters, ExpectedMeters, RecoveryAttempt, Outcome (+31 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (16): TelemetrySample, Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip (+8 more)

### Community 10 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.17
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): Task, Task, LobbyCreateOutcome, LobbyId, Success, LobbyJoinFailureReason, Expired, Failed (+13 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.09
Nodes (25): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+17 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.06
Nodes (25): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+17 more)

### Community 19 - ".Measure"
Cohesion: 0.08
Nodes (34): Surface, Collider, CompiledRoadModel, GameObject, HashSet, IEnumerable, IReadOnlyList, List (+26 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.14
Nodes (12): Rigidbody, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile, Profile (+4 more)

### Community 22 - "LobbyFlowController"
Cohesion: 0.11
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 23 - "LobbyRosterService"
Cohesion: 0.07
Nodes (17): Difficulty, FacepunchSteamLobbyPlatform, Difficulty, ISteamLobbyPlatform, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby (+9 more)

### Community 24 - ".Core"
Cohesion: 0.19
Nodes (16): RouteResult, CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId (+8 more)

### Community 25 - "LobbyRoomService"
Cohesion: 0.15
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (19): CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, SortedDictionary, Vector3 (+11 more)

### Community 27 - "EffectiveRuleException"
Cohesion: 0.19
Nodes (15): Active, CompiledRoadModel, IReadOnlyDictionary, IReadOnlyList, List, RoadId, Active, EffectiveRuleException (+7 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.09
Nodes (24): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+16 more)

### Community 29 - "MainMenuProfileFlowController"
Cohesion: 0.10
Nodes (18): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, PlayerProfileBootstrapService (+10 more)

### Community 30 - "RoadId"
Cohesion: 0.08
Nodes (31): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+23 more)

### Community 31 - "CollisionFacts"
Cohesion: 0.08
Nodes (26): RoadId, CollisionAnalysis, CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, CollisionResponseRequest, Facts (+18 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.09
Nodes (28): CompiledRoadModel, IReadOnlyList, ProfilerMarker, RoadCurveSample, RoadElementKind, RoadModelVersion, SignalState, VehicleFootprint (+20 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.06
Nodes (33): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+25 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.09
Nodes (11): Action, Collider, Collision, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc (+3 more)

### Community 35 - ".Run"
Cohesion: 0.15
Nodes (8): Action, KeyValuePair, MenuItem, Scene, GateABinding, MenuItem, MenuItem, Func

### Community 36 - ".Monitor"
Cohesion: 0.06
Nodes (35): DriverProfile, GaugeBox, List, LongitudinalCandidateKind, MotionCommand, PerceivedObstacleKind, ReferenceTrack, RoadId (+27 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.12
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.15
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.11
Nodes (20): CompiledRoadModel, DrivabilityProfile, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, PathHorizon, End (+12 more)

### Community 41 - "TacticalReason"
Cohesion: 0.11
Nodes (18): TacticalReason, Accepted, CollisionPreempted, ExitPortalReached, FallbackLatched, GoalAlreadyActive, InvalidRequest, ManeuverAborted (+10 more)

### Community 42 - "JunctionReason"
Cohesion: 0.08
Nodes (25): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+17 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Measure"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+4 more)

### Community 45 - "ManeuverSituation"
Cohesion: 0.18
Nodes (15): EffectiveLaneCorridor, GaugeBox, ManeuverTiming, CompiledRoadModel, EffectiveLaneCorridor, EffectivePolicy, EffectiveRuleException, GaugeBox (+7 more)

### Community 46 - "TrafficV2Composition.cs"
Cohesion: 0.12
Nodes (21): IReadOnlyList, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label, MaxPopulation (+13 more)

### Community 47 - "EffectivePolicy"
Cohesion: 0.11
Nodes (21): DriverProfile, DriverProfileDef, RoadId, DrivingPolicy, DrivingSurface, Carriageway, None, OpposingCorridor (+13 more)

### Community 48 - "DefinitionId"
Cohesion: 0.10
Nodes (14): List, CharacterCatalog, Count, Color, GameObject, CharacterDef, DisplayName, Id (+6 more)

### Community 49 - "JunctionSnapshot"
Cohesion: 0.08
Nodes (24): JunctionControlKind, JunctionBatchCounters, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations (+16 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.06
Nodes (35): Bounds, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList (+27 more)

### Community 51 - "TrafficV2StepRunner"
Cohesion: 0.08
Nodes (25): Dictionary, JunctionActorReport, List, ProfilerMarker, RoadId, RuleExceptionRequest, Stopwatch, TrafficActorInput (+17 more)

### Community 52 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.09
Nodes (22): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, CompiledRoadModel, List (+14 more)

### Community 53 - "PairReviewModel"
Cohesion: 0.17
Nodes (8): CompiledRoadModel, Dictionary, List, RoadBoundsBox, RoadId, StringBuilder, PairReview, PairReviewModel

### Community 54 - ".ObserveTrackingTolerance"
Cohesion: 0.29
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

### Community 55 - "ImportedCurve"
Cohesion: 0.14
Nodes (16): IReadOnlyList, KeyValuePair, List, RoadCurve, RoadCurveSample, RoadRecordKind, ImportedCurve, ImportedSection (+8 more)

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.10
Nodes (19): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+11 more)

### Community 58 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 59 - "NetworkedPassengerActionIntent"
Cohesion: 0.05
Nodes (39): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+31 more)

### Community 60 - ".ZoneSpan"
Cohesion: 0.32
Nodes (4): CompiledRoadModel, Vector3, Bounds, RoadBoundsBox

### Community 61 - "TrafficV2HazardCollector"
Cohesion: 0.09
Nodes (28): Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState, ProfilerMarker (+20 more)

### Community 62 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 63 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 64 - ".Decide"
Cohesion: 0.12
Nodes (19): DriverProfile, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind (+11 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.22
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 66 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.09
Nodes (23): CompiledRoadModel, DriverProfile, Portal, RoadId, RoutePlan, RouteSeed, SpeedPlan, Vector3 (+15 more)

### Community 68 - "ManeuverCandidate"
Cohesion: 0.09
Nodes (21): ManeuverCandidate, Cost, DurationSeconds, ExceptionExpiryFrame, GeometryCause, Kind, OtherCorridorId, Path (+13 more)

### Community 69 - "AgentObservation"
Cohesion: 0.17
Nodes (21): LaneSide, RoadId, RoadLocation, Vector3, VehicleFootprint, AdjacentOccupantFact, AgentObservation, ExitOccupancyFact (+13 more)

### Community 70 - ".Compute"
Cohesion: 0.15
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.06
Nodes (31): EffectivePolicy, EffectiveRuleException, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind (+23 more)

### Community 72 - "RoadId"
Cohesion: 0.10
Nodes (18): Bounds, List, RoadId, RoadKinematicAnchor, RoadLocation, Vector3, VehicleFootprintPose, TrafficActor (+10 more)

### Community 74 - ".ObserveAlong"
Cohesion: 0.13
Nodes (12): SpatialQueryBuffer, Capacity, Count, Saturated, Total, ObstacleFact, PerceivedObstacleKind, RoadCurve (+4 more)

### Community 75 - ".Draw"
Cohesion: 0.21
Nodes (8): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 78 - "MotionCommand"
Cohesion: 0.22
Nodes (8): DrivabilityProfile, DriverProfile, IReadOnlyList, RoadCurve, Vector3, MotionCommand, IsFinite, RuleExceptions

### Community 79 - ".NearField"
Cohesion: 0.13
Nodes (17): Blocker, LongitudinalCandidateKind, LongitudinalDecision, LongitudinalPerception, ObservationChannel, ObstacleFact, PerceivedObstacleKind, PerceptionLimits (+9 more)

### Community 80 - "ModelDto"
Cohesion: 0.15
Nodes (13): AdjacencyDto, ModelDto, RoadLocalizationProfile, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto (+5 more)

### Community 81 - "JunctionTraversal"
Cohesion: 0.13
Nodes (9): TraversalComparer, JunctionText, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds (+1 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.09
Nodes (20): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+12 more)

### Community 83 - "V1ImportResult"
Cohesion: 0.14
Nodes (11): Dictionary, DrivabilityProfile, IList, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1ImportResult (+3 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.12
Nodes (26): CompiledRoadModel, Dictionary, HashSet, IReadOnlyList, JunctionRecord, JunctionSnapshot, KeyValuePair, List (+18 more)

### Community 85 - "NetworkedPlayerSpawnService"
Cohesion: 0.13
Nodes (13): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+5 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.12
Nodes (16): JunctionRecord, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, PairDecisionState, Confirmed (+8 more)

### Community 87 - "NetworkPlayerRegistry"
Cohesion: 0.27
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 88 - "LobbyRosterScreen"
Cohesion: 0.05
Nodes (17): Difficulty, Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Difficulty, Button (+9 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.14
Nodes (21): JunctionMovement, JunctionControl, RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement (+13 more)

### Community 90 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 91 - "IPathGeometry"
Cohesion: 0.12
Nodes (13): Vector3, Vector3, IPathGeometry, LengthMeters, Spans, PlanningDecision, Motion, Observation (+5 more)

### Community 92 - "RoadLineage"
Cohesion: 0.14
Nodes (16): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+8 more)

### Community 93 - "GateAEvidenceResult"
Cohesion: 0.15
Nodes (14): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+6 more)

### Community 94 - "V1Node"
Cohesion: 0.10
Nodes (29): Predicate, Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List (+21 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.10
Nodes (20): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+12 more)

### Community 96 - "TrafficLongitudinalOutcome"
Cohesion: 0.11
Nodes (17): AgentObservation, Blocker, LongitudinalDecision, TrafficHazardCollectorCounters, TrafficLongitudinalOutcome, Blockers, Collector, Decision (+9 more)

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (16): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+8 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.12
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.41
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 102 - "RoutePlan"
Cohesion: 0.07
Nodes (34): IReadOnlyList, PlanningRequest, CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation, RoadModelVersion, DecisionCounter (+26 more)

### Community 103 - "NetworkedPlayerReviveIntent"
Cohesion: 0.12
Nodes (13): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState (+5 more)

### Community 104 - "ManeuverGeometryCause"
Cohesion: 0.14
Nodes (15): ManeuverGeometryCause, CorridorEnd, CorridorTooShort, EnvelopeExceeded, EnvelopeNotContiguous, InvalidInput, NoClosingSpeed, NoLateralRoom (+7 more)

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MenuCharacterPreview"
Cohesion: 0.18
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.16
Nodes (13): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, Vector3 (+5 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.20
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - "HazardRootClass"
Cohesion: 0.14
Nodes (12): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer, HazardRootClass, Obstacle, Self (+4 more)

### Community 113 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 114 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 115 - ".PrepareStep"
Cohesion: 0.19
Nodes (5): StepContactAccumulator, ProfilerMarker, TrafficActorInput, VehicleDriveIntentComposer, VehicleFootprintPose

### Community 116 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 117 - "StopHoldState"
Cohesion: 0.12
Nodes (13): StopHoldPhase, Entered, Holding, None, Released, StopHoldRelease, GapOpened, GrantEffective (+5 more)

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "TrafficJunctionOutcome"
Cohesion: 0.14
Nodes (11): JunctionActorReport, TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame (+3 more)

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "TrackingMeasurement.cs"
Cohesion: 0.29
Nodes (8): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound, TrackingMeasurement

### Community 123 - "AIVehicleBehaviorDebugView"
Cohesion: 0.22
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.03
Nodes (81): AgentObservation, Blocker, CollisionFacts, CollisionResponseRequest, Dictionary, EffectivePolicy, ObservationChannel, ObstacleFact (+73 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.17
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

### Community 128 - ".Evaluate"
Cohesion: 0.11
Nodes (22): RoadId, AuthorizedContact, Valid, NearFieldSample, SafetyFilter, SafetyLimits, SafetyResult, Command (+14 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "GridlockSupervisor"
Cohesion: 0.19
Nodes (12): GridlockArc, Action, Dictionary, HashSet, IReadOnlyDictionary, IReadOnlyList, List, RoadId (+4 more)

### Community 131 - "CampaignTraceability"
Cohesion: 0.22
Nodes (5): Dictionary, HashSet, IReadOnlyDictionary, CampaignTraceability, Elements

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.18
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (67): CompiledConflictZone, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile, ImportManifest, ImportManifestEntry (+59 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.08
Nodes (35): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+27 more)

### Community 135 - "RoadRage.Shared.Definitions"
Cohesion: 0.08
Nodes (12): VehicleDamageType, Brake, Engine, Wheel, IHostOwnedRuntimeState, IsHostAuthority, RoadRage.Features.Vehicles.Traffic.Policy, RoadRage.Features.Rage (+4 more)

### Community 136 - "LobbyJoinService"
Cohesion: 0.14
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.25
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "DriverProfileDef"
Cohesion: 0.10
Nodes (17): DriverProfileDef, CollisionReaction, Id, Policy, Profile, RawId, CollisionReaction, Brake (+9 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (40): DrivabilityProfile, Comparison, RoadModelCompiler, Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto (+32 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "RuleExceptionReason"
Cohesion: 0.14
Nodes (14): RuleExceptionReason, Accepted, AlreadyActive, Expired, FrameUnavailable, Held, Malformed, RequesterAbsent (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.22
Nodes (7): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalPayload, RoadModelCanonicalWriter, ConflictZone

### Community 146 - "ReferenceTrack"
Cohesion: 0.19
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 147 - "TrafficV2Settings"
Cohesion: 0.15
Nodes (12): PerceptionLimits, TrackingTolerance, VehicleCoverage, TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, TrafficV2Verdict (+4 more)

### Community 148 - "ImportContext"
Cohesion: 0.22
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, AuthoringTask, CircleFit, ImportContext

### Community 149 - "SafetyReason"
Cohesion: 0.22
Nodes (9): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+1 more)

### Community 150 - "TacticalDecision"
Cohesion: 0.04
Nodes (58): CollisionFacts, CollisionResponseRequest, RecoveryRequest, RoadId, RouteSeed, CollisionGoalPhase, AwaitingRecovery, Braking (+50 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - ".Run"
Cohesion: 0.29
Nodes (8): CompiledRoadModel, IReadOnlyList, TrafficFrame, TrafficV2StepCost, GridlockCycle, IEnumerable, JunctionCoordinator, SignalPhaseController

### Community 153 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 154 - "SignalPhaseController"
Cohesion: 0.29
Nodes (6): CompiledRoadModel, IReadOnlyList, SignalPhaseController, Current, Model, CompiledSignalPlan

### Community 155 - "ManeuverEvaluation.cs"
Cohesion: 0.18
Nodes (10): ManeuverEvaluationResult, Candidates, Cause, FrameId, Ready, Selected, ManeuverSupervision, Abort (+2 more)

### Community 156 - "RightOfWayRelation"
Cohesion: 0.24
Nodes (8): Vector3, RightOfWay, RightOfWayRelation, Ambiguous, FromLeft, FromRight, Opposite, Same

### Community 157 - "TrackPiece"
Cohesion: 0.18
Nodes (10): RoadCurve, RoadElementKind, RoadId, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 158 - "RuleExceptionRecord"
Cohesion: 0.20
Nodes (7): RuleExceptionRecord, IsEffective, RuleExceptionStatus, Accepted, Denied, Ended, Held

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 160 - "RightOfWayTable"
Cohesion: 0.25
Nodes (9): ConditionalWeakTable, IReadOnlyList, RoadId, Entry, RightOfWayTable, AmbiguousCount, CanonicalText, Entries (+1 more)

### Community 161 - "TrafficV2Admission"
Cohesion: 0.27
Nodes (8): TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle, GameObject, GateAEvidenceResult

### Community 162 - ".Build"
Cohesion: 0.18
Nodes (11): JunctionKinematics, Known, ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId, Vector3 (+3 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - "DrivingPolicyProfile"
Cohesion: 0.21
Nodes (9): DrivingPolicyProfile, AcceptedGapSeconds, AcceptedRisk, AllowedSurfaces, Default, RoutePreferenceWeight, ManeuverPreference, Cost (+1 more)

### Community 165 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (52): AppliedWidth, ConflictKind, FileLayout, Func, IEnumerable, IList, JunctionControlKind, List (+44 more)

### Community 166 - ".ProjectEntry"
Cohesion: 0.20
Nodes (6): Dictionary, RoadCurve, RoadProjection, ProjectionKey, IEquatable, ProjectionKey

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 169 - "V2FallbackReason"
Cohesion: 0.08
Nodes (28): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2FallbackReason, ExitPortalReached, Faulted, FrameUnavailable, HorizonNonConforming (+20 more)

### Community 170 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 171 - "RuleExceptionRequest"
Cohesion: 0.14
Nodes (12): RuleExceptionRequest, Requester, Rule, Scope, SourceFrame, StartReason, Target, Termination (+4 more)

### Community 173 - "TrafficRule"
Cohesion: 0.20
Nodes (10): TrafficRule, FollowingGap, JunctionControl, KeepClear, LaneChange, None, OpposingCorridor, Sidewalk (+2 more)

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 175 - "InterStepResult"
Cohesion: 0.25
Nodes (8): IReadOnlyList, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees

### Community 176 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 177 - "ManeuverVerdict"
Cohesion: 0.20
Nodes (10): ManeuverVerdict, ExceptionDenied, ExceptionPending, Feasible, GeometryInfeasible, NotOffered, PolicyRefused, SafetyRejected (+2 more)

### Community 178 - "JunctionActorReport"
Cohesion: 0.06
Nodes (40): IReadOnlyList, RoadId, Vector3, GridlockCycle, Arcs, Key, Members, GridlockEscalationTier (+32 more)

### Community 180 - ".Step"
Cohesion: 0.11
Nodes (18): JunctionSnapshot, EffectiveLaneCorridor, EffectiveRuleException, IReadOnlyList, JunctionActorReport, LongitudinalDecision, TrafficActor, TrafficFrame (+10 more)

### Community 182 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 184 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 185 - ".ValidateApproaches"
Cohesion: 0.25
Nodes (7): Dictionary, JunctionControl, JunctionMovement, LaneCorridor, List, RoadModelSource, RoadModelValidationIssue

### Community 186 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 187 - "LongitudinalArbitration.cs"
Cohesion: 0.18
Nodes (13): IReadOnlyList, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason (+5 more)

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "V2ComposerDiagnostic"
Cohesion: 0.29
Nodes (7): V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None, ProfileScalarNonFinite, RollingBackward

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (16): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+8 more)

### Community 194 - ".ToRecord"
Cohesion: 0.50
Nodes (3): FileLayout, FileRecord, FileRecord

### Community 195 - ".TryApproachEnd"
Cohesion: 0.50
Nodes (3): CompiledJunctionControl, CompiledRoadModel, RoadCurveSample

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 206 - "PairReviewStatus"
Cohesion: 0.40
Nodes (5): PairReviewStatus, Modified, New, Removed, Unchanged

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.14
Nodes (21): AdjacentOccupantFact, AgentObservation, IPathGeometry, IReadOnlyList, List, ObservationChannel, RoadId, TrafficActor (+13 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (14): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+6 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1467 isolated node(s):** `FrameId`, `Observation`, `Decision`, `RoadLimits`, `Blockers` (+1462 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 2078 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **39 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `ManeuverPath`, `GateAEvidenceParameters`, `TacticalDecision`, `.Run`, `ManeuverEvaluation.cs`, `TrafficV2Admission`, `.Monitor`, `TrafficV2StepRunner`, `.Step`, `.ObserveTrackingTolerance`, `TrafficDriveOutcome`, `TrafficV2HazardCollector`, `TrafficV2Insertion`, `ManeuverCandidate`, `TrafficDecisionProjection`, `.NearField`, `NetworkedPlayerReviveIntent`, `.PrepareStep`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.203) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2Admission`, `TrafficV2Insertion`, `.Monitor`, `MonoBehaviour`, `RageRoadEventFlowController`, `TrafficV2Composition.cs`, `TrafficV2Code`, `TrafficV2StepRunner`, `TrafficSettingsDef`, `LaneGraph`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.143) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `RoadModelRecords.cs`, `RoutePlan`, `.TryGetMovement`, `.ProjectEntry`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadLineage`, `RoadModelCanonicalWriter`, `Blocker`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `PortalTrafficSpawner`, `TrafficV2HazardCollector`, `.TryGetCorridor`?**
  _High betweenness centrality (0.129) - this node is a cross-community bridge._
- **What connects `FrameId`, `Observation`, `Decision` to the rest of the system?**
  _1467 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MotionPlan` be split into smaller, more focused modules?**
  _Cohesion score 0.12681159420289856 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.07344632768361582 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.08403361344537816 - nodes in this community are weakly interconnected._