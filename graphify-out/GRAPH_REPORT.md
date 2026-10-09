# Graph Report - RRS  (2026-10-09)

## Corpus Check
- 186 files · ~259,508 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4971 nodes · 11430 edges · 197 communities (188 shown, 9 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 499 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `d60b86ac`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MotionPlan
- .Refine
- RoadRage.Features.Vehicles.Traffic.Planning
- GreyboxAssetSeedMetadata
- SweepPose
- .Add
- GateAEvidenceParameters
- .Localize
- RecoverySupervisor
- VehicleSuspensionModel
- NetworkedVehicleSeatService
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- FacepunchSteamLobbyPlatform
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
- DefinitionId
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
- PairSweep
- RoadRage.Features.Vehicles.Traffic.Migration
- EffectivePolicy
- .Evaluate
- JunctionSnapshot
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- PairReview
- TrafficV2Settings
- ImportedCurve
- .Add
- TrafficDriveOutcome
- LongitudinalDecision
- NetworkedPassengerActionIntent
- PerceivedObstacleKind
- TrafficV2HazardCollector
- PlayerMode
- ObservationChannel
- .Decide
- .FixedUpdate
- NetworkedRageState
- TrafficV2Insertion
- LobbyRosterScreen
- AgentObservation
- .Compute
- TrafficDecisionProjection
- SpatialEntry
- PairReviewEntry
- RecoveryRequest
- .Draw
- UserNotice
- PassengerActionVerdictCode
- MotionCommand
- .CheckVisuals
- ModelDto
- JunctionTraversal
- KinematicOffsetBounds
- V1ImportResult
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- NetworkPlayerRegistry
- .Awake
- RoadModelValidationIssue
- RageDisposition
- LobbyPlayerSlotView
- RoadLineage
- RoadBoundsBox
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
- JunctionExitBound
- OnlineServicesBootstrapService
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- .HandleRosterChanged
- NetworkedPlayerLifecycleIntent
- RoadModelVersion
- .PrepareStep
- SpeedConstraint
- LongitudinalArbitration.cs
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
- SafetyResult
- RoadRageBootstrap
- TrafficV2StepRunner
- DriverProfileDef
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- NetworkedVehicleState.cs
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- CollisionReactionWeights
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- RuleExceptionReason
- RoadModelCanonicalWriter
- RoadRageBootstrap.cs
- PlayerProfileFileStore
- ImportContext
- SafetyReason
- TacticalDecision
- NetworkedVehicleSeatIntent
- VehicleCoverage
- PathIssue
- SignalPhaseController
- .Read
- JunctionClearanceResult
- ReferenceTrack
- RuleExceptionStatus
- MonoBehaviour
- PlanningReach
- NetworkedBossState.cs
- .Build
- RoutePath
- DrivingPolicyProfile
- AuthoringDecisions
- NetworkedCrewEconomyState.cs
- CampaignTraceability
- VehicleDriveIntent
- V2FallbackReason
- TireSample
- RuleExceptionRequest
- PlayerNameValidator
- TrafficRule
- TrafficV2Code
- InterStepResult
- .MeasurePath
- TacticalGoalKind
- JunctionRecords.cs
- TrafficV2WorkCounters.cs
- .Step
- SessionTrafficValue.cs
- LaneGraphRouting
- MatchSettings
- LongitudinalPerception
- VehicleArcadeAssist
- NetworkedVehicleRecoveryIntent
- CollisionGoalPhase
- RoadRage.Shared.Domain
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- PairReviewModel
- Collision
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 132 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `NetworkedVehicleState` - 67 edges
6. `ImportContext` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `TrafficFrame` - 64 edges
10. `JunctionCoordinator` - 59 edges

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

## Communities (197 total, 9 thin omitted)

### Community 0 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (17): ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2, MovementSide (+9 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.11
Nodes (16): PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Policy, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing, RoadRage.Features.Vehicles.Traffic.Signals (+8 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.16
Nodes (17): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, GridPath (+9 more)

### Community 5 - ".Add"
Cohesion: 0.13
Nodes (16): Bounds, CompiledJunctionMovement, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation (+8 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (19): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+11 more)

### Community 7 - ".Localize"
Cohesion: 0.11
Nodes (22): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+14 more)

### Community 8 - "RecoverySupervisor"
Cohesion: 0.11
Nodes (16): IReadOnlyList, List, RoadId, ProgressLedger, ActualMeters, ExpectedMeters, RecoveryObservation, RecoverySupervisor (+8 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "NetworkedVehicleSeatService"
Cohesion: 0.09
Nodes (13): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+5 more)

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "RoadRage.Features.Online"
Cohesion: 0.10
Nodes (8): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, RoadRage.Features.Online

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.07
Nodes (24): Difficulty, Task, FacepunchSteamLobbyPlatform, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId, Success (+16 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.11
Nodes (19): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+11 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.08
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 19 - ".Measure"
Cohesion: 0.15
Nodes (15): BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, Scene, Vector3 (+7 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "LobbyFlowController"
Cohesion: 0.11
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 23 - "LobbyRosterService"
Cohesion: 0.09
Nodes (15): Difficulty, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId (+7 more)

### Community 24 - ".Core"
Cohesion: 0.16
Nodes (19): RouteDiagnostic, None, ZeroWeightFallback, RouteResult, CompiledRoadModel, Dictionary, HashSet, List (+11 more)

### Community 25 - "LobbyRoomService"
Cohesion: 0.12
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (21): CompiledJunctionMovement, CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, RoadModelValidationProfile (+13 more)

### Community 27 - "EffectiveRuleException"
Cohesion: 0.14
Nodes (17): Active, CompiledRoadModel, IReadOnlyDictionary, IReadOnlyList, List, RoadId, Active, EffectiveRuleException (+9 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.09
Nodes (23): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+15 more)

### Community 29 - "DefinitionId"
Cohesion: 0.07
Nodes (27): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+19 more)

### Community 30 - "RoadId"
Cohesion: 0.06
Nodes (34): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+26 more)

### Community 31 - "CollisionFacts"
Cohesion: 0.09
Nodes (25): RoadId, CollisionAnalysis, CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, CollisionResponseRequest, Facts (+17 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (33): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+25 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.09
Nodes (12): DevIndestructibleVehicle, Action, Collider, Collision, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody (+4 more)

### Community 35 - ".Run"
Cohesion: 0.15
Nodes (8): Action, KeyValuePair, MenuItem, Scene, GateABinding, MenuItem, MenuItem, Func

### Community 36 - ".Monitor"
Cohesion: 0.09
Nodes (20): V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None, ProfileScalarNonFinite, RollingBackward, IReadOnlyList (+12 more)

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
Cohesion: 0.06
Nodes (35): Vector3, CompiledRoadModel, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, Vector3, HorizonEnd (+27 more)

### Community 41 - "TacticalReason"
Cohesion: 0.08
Nodes (24): RecoveryAttempt, Outcome, OutcomeFrame, Request, Response, ResponseFrame, TacticalReason, Accepted (+16 more)

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
Cohesion: 0.19
Nodes (7): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate, RoadModelValidationProfile

### Community 46 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.09
Nodes (25): IReadOnlyList, RoadId, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind (+17 more)

### Community 47 - "EffectivePolicy"
Cohesion: 0.13
Nodes (16): DriverProfile, DriverProfileDef, RoadId, DrivingPolicy, DrivingSurface, Carriageway, None, OpposingCorridor (+8 more)

### Community 48 - ".Evaluate"
Cohesion: 0.35
Nodes (5): RoadId, AuthorizedContact, Valid, SafetyFilter, SafetyLimits

### Community 49 - "JunctionSnapshot"
Cohesion: 0.12
Nodes (15): IReadOnlyList, JunctionControlKind, RoadId, JunctionApproach, Crossed, JunctionBatchCounters, JunctionRecord, IsEffectiveGrant (+7 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.09
Nodes (18): CompiledJunctionControl, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, List, RoadCurveSample (+10 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReview"
Cohesion: 0.26
Nodes (3): Dictionary, RoadBoundsBox, PairReview

### Community 54 - "TrafficV2Settings"
Cohesion: 0.15
Nodes (12): TrackingToleranceResponse, Latched, LatchedAtStep, GridlockEscalationTier, TrafficRule, TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits (+4 more)

### Community 55 - "ImportedCurve"
Cohesion: 0.15
Nodes (16): Dictionary, IReadOnlyList, KeyValuePair, List, RoadCurve, RoadCurveSample, RoadRecordKind, ImportedCurve (+8 more)

### Community 56 - ".Add"
Cohesion: 0.20
Nodes (4): JunctionFeature, Predicate, AuthoringTask, ImportedJunction

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (18): TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback, FallbackReason (+10 more)

### Community 58 - "LongitudinalDecision"
Cohesion: 0.12
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 59 - "NetworkedPassengerActionIntent"
Cohesion: 0.06
Nodes (35): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+27 more)

### Community 60 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

### Community 61 - "TrafficV2HazardCollector"
Cohesion: 0.07
Nodes (36): TrafficHazardCollectorCounters, Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState (+28 more)

### Community 62 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 63 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 64 - ".Decide"
Cohesion: 0.15
Nodes (14): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing (+6 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 66 - "NetworkedRageState"
Cohesion: 0.07
Nodes (22): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner, NetworkVariable, NetworkedRageState (+14 more)

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.12
Nodes (16): CompiledRoadModel, Portal, Quaternion, RoadLocation, Vector3, TrafficV2Insertion, Code, EntryPortal (+8 more)

### Community 69 - "AgentObservation"
Cohesion: 0.14
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 70 - ".Compute"
Cohesion: 0.16
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.06
Nodes (29): IReadOnlyList, RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind (+21 more)

### Community 72 - "SpatialEntry"
Cohesion: 0.08
Nodes (28): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, IntentInterval (+20 more)

### Community 73 - "PairReviewEntry"
Cohesion: 0.19
Nodes (8): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, PairReviewActions, PairReviewEntry

### Community 74 - "RecoveryRequest"
Cohesion: 0.12
Nodes (15): RecoveryCause, Displaced, None, ProgressDeficit, ToleranceLatched, RecoveryManeuver, Realign, Reverse (+7 more)

### Community 75 - ".Draw"
Cohesion: 0.23
Nodes (8): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 76 - "UserNotice"
Cohesion: 0.10
Nodes (13): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+5 more)

### Community 77 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 78 - "MotionCommand"
Cohesion: 0.22
Nodes (8): DrivabilityProfile, DriverProfile, IReadOnlyList, RoadCurve, Vector3, MotionCommand, IsFinite, RuleExceptions

### Community 79 - ".CheckVisuals"
Cohesion: 0.25
Nodes (5): Surface, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "ModelDto"
Cohesion: 0.14
Nodes (14): AdjacencyDto, DrivabilityProfile, RoadModelCompiler, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto (+6 more)

### Community 81 - "JunctionTraversal"
Cohesion: 0.24
Nodes (8): TraversalComparer, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds, IEqualityComparer

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.09
Nodes (20): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+12 more)

### Community 83 - "V1ImportResult"
Cohesion: 0.15
Nodes (10): DrivabilityProfile, IList, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1ImportResult, Succeeded (+2 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.12
Nodes (26): CompiledRoadModel, Dictionary, HashSet, IReadOnlyList, JunctionRecord, JunctionSnapshot, KeyValuePair, List (+18 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.16
Nodes (12): Vector2, ConflictSweep, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState, MovementSide (+4 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.20
Nodes (11): JunctionRecord, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, Document (+3 more)

### Community 87 - "NetworkPlayerRegistry"
Cohesion: 0.27
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 88 - ".Awake"
Cohesion: 0.12
Nodes (4): Button, TMP_Text, LobbyShellScreen, TMP_InputField

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (18): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+10 more)

### Community 90 - "RageDisposition"
Cohesion: 0.18
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.06
Nodes (43): ConditionalWeakTable, Dictionary, IReadOnlyList, JunctionControl, JunctionMovement, LaneCorridor, List, RoadId (+35 more)

### Community 93 - "RoadBoundsBox"
Cohesion: 0.15
Nodes (14): CompiledConflictZone, Vector3, ConflictKind, Crossing, Merge, ConflictZone, Junction, JunctionFeature (+6 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (30): ImportedConnection, Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List (+22 more)

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
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.41
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 102 - "RoutePlan"
Cohesion: 0.05
Nodes (44): DriverProfile, IReadOnlyList, ProfilerMarker, PlanningRequest, PlanningSpine, CompiledRoadModel, IReadOnlyList, RoadId (+36 more)

### Community 103 - "NetworkedPlayerState"
Cohesion: 0.15
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

### Community 108 - "JunctionExitBound"
Cohesion: 0.11
Nodes (13): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters, JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound (+5 more)

### Community 109 - "OnlineServicesBootstrapService"
Cohesion: 0.21
Nodes (8): OnlineServicesBootstrapService, Status, OnlineServicesStatus, InitializationFailed, NotStarted, Offline, Online, SignInFailed

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.18
Nodes (11): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, RoadCurveSample (+3 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.20
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - ".HandleRosterChanged"
Cohesion: 0.24
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 113 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 114 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 115 - ".PrepareStep"
Cohesion: 0.22
Nodes (3): StepContactAccumulator, DrivabilityProfile, VehicleProfile

### Community 116 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 117 - "LongitudinalArbitration.cs"
Cohesion: 0.12
Nodes (20): RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalMemory, None, LongitudinalObstacle, StopHoldPhase (+12 more)

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.15
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "TrafficJunctionOutcome"
Cohesion: 0.18
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

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
Cohesion: 0.24
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.03
Nodes (64): BoxCollider, Collider, Dictionary, DriverProfile, DriverProfileDef, List, Portal, ProfilerMarker (+56 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.15
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

### Community 128 - "SafetyResult"
Cohesion: 0.13
Nodes (14): SafetyResult, Command, HazardId, NearFieldStep, PhysicsStep, Reason, Refusal, SourceFrameId (+6 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.07
Nodes (23): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+15 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.06
Nodes (41): GridlockArc, GridlockCycle, Arcs, Key, Members, CompiledRoadModel, Dictionary, HashSet (+33 more)

### Community 131 - "DriverProfileDef"
Cohesion: 0.27
Nodes (6): DriverProfileDef, CollisionReaction, Id, Policy, Profile, RawId

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.05
Nodes (64): EffectiveLaneCorridor, JunctionMovement, RoadModelCanonicalPayload, Comparison, DrivabilityProfile, ImportManifest, ImportManifestEntry, JunctionControl (+56 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.08
Nodes (35): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+27 more)

### Community 135 - "NetworkedVehicleState.cs"
Cohesion: 0.21
Nodes (6): VehicleDamageType, Brake, Engine, Wheel, IHostOwnedRuntimeState, IsHostAuthority

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
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.08
Nodes (20): JunctionKinematics, Known, JunctionPriority, Vector3, JunctionActorReport, Approaches, Corners, ElementId (+12 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "RuleExceptionReason"
Cohesion: 0.14
Nodes (14): RuleExceptionReason, Accepted, AlreadyActive, Expired, FrameUnavailable, Held, Malformed, RequesterAbsent (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "RoadRageBootstrap.cs"
Cohesion: 0.22
Nodes (5): AppPlayModeEntry, RoadRage.App.Services, RoadRage.App, PlayModeStateChange, SceneAsset

### Community 147 - "PlayerProfileFileStore"
Cohesion: 0.31
Nodes (5): PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath

### Community 148 - "ImportContext"
Cohesion: 0.26
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 149 - "SafetyReason"
Cohesion: 0.22
Nodes (9): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+1 more)

### Community 150 - "TacticalDecision"
Cohesion: 0.10
Nodes (21): RecoveryMotion, TacticalDecision, AcceptedAtFrame, Active, AwaitingRecovery, CollisionActive, Goal, GoalVersion (+13 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 153 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 154 - "SignalPhaseController"
Cohesion: 0.28
Nodes (7): SignalPhaseInput, CompiledRoadModel, IReadOnlyList, SignalPhaseController, Current, Model, CompiledSignalPlan

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 157 - "ReferenceTrack"
Cohesion: 0.11
Nodes (15): RoadCurve, RoadElementKind, RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters (+7 more)

### Community 158 - "RuleExceptionStatus"
Cohesion: 0.40
Nodes (5): RuleExceptionStatus, Accepted, Denied, Ended, Held

### Community 159 - "MonoBehaviour"
Cohesion: 0.11
Nodes (13): IEnumerable, NetworkedPlayerLifecycleService, Instance, Transform, RunCompositionRoot, RuntimeRoot, SpawnRoot, PlayerLifecycle (+5 more)

### Community 161 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 162 - ".Build"
Cohesion: 0.22
Nodes (10): JunctionMovementPosition, ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId, Vector3, JunctionRequestBuilder (+2 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - "DrivingPolicyProfile"
Cohesion: 0.14
Nodes (14): DrivingPolicyProfile, AcceptedGapSeconds, AcceptedRisk, AllowedSurfaces, Default, RoutePreferenceWeight, ManeuverKind, AdjacentCorridor (+6 more)

### Community 165 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (52): AppliedWidth, ConflictKind, FileLayout, Func, IEnumerable, IList, JunctionControlKind, List (+44 more)

### Community 166 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 167 - "CampaignTraceability"
Cohesion: 0.08
Nodes (22): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements, ElementStatus, Measured (+14 more)

### Community 168 - "VehicleDriveIntent"
Cohesion: 0.18
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 169 - "V2FallbackReason"
Cohesion: 0.09
Nodes (26): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2FallbackReason, ExitPortalReached, Faulted, FrameUnavailable, HorizonNonConforming (+18 more)

### Community 170 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 171 - "RuleExceptionRequest"
Cohesion: 0.15
Nodes (12): RuleExceptionRequest, Requester, Rule, Scope, SourceFrame, StartReason, Target, Termination (+4 more)

### Community 173 - "TrafficRule"
Cohesion: 0.20
Nodes (10): TrafficRule, FollowingGap, JunctionControl, KeepClear, LaneChange, None, OpposingCorridor, Sidewalk (+2 more)

### Community 174 - "TrafficV2Code"
Cohesion: 0.05
Nodes (37): GameObject, TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Code, Allowed (+29 more)

### Community 175 - "InterStepResult"
Cohesion: 0.17
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 177 - "TacticalGoalKind"
Cohesion: 0.50
Nodes (4): TacticalGoalKind, CollisionResponse, Nominal, Recovery

### Community 178 - "JunctionRecords.cs"
Cohesion: 0.06
Nodes (28): GridlockEscalationTier, None, PrecedenceRelaxation, GridlockOutcome, Escalated, Exhausted, Progressed, GridlockResolution (+20 more)

### Community 183 - "LaneGraphRouting"
Cohesion: 0.24
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 185 - "MatchSettings"
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 187 - "LongitudinalPerception"
Cohesion: 0.15
Nodes (12): IReadOnlyList, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason, PerceptionUnavailableReason, ChannelSaturated (+4 more)

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 191 - "CollisionGoalPhase"
Cohesion: 0.40
Nodes (5): CollisionGoalPhase, AwaitingRecovery, Braking, None, Reacting

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (16): RoadRage.Features.Players, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot, RoadRage.App.Lobby, RoadRage.App.Run, RoadRage.Features.Rage (+8 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 206 - "PairReviewModel"
Cohesion: 0.24
Nodes (9): CompiledRoadModel, List, StringBuilder, PairReviewModel, PairReviewStatus, Modified, New, Removed (+1 more)

### Community 208 - "Collision"
Cohesion: 0.33
Nodes (3): Collision, Transform, V2ContactEpisode

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.20
Nodes (10): Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance, PerceptionLimits, TrafficPerception (+2 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1373 isolated node(s):** `RoadRage.App`, `Instance`, `Router`, `Notices`, `Profiles` (+1368 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1870 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **9 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `SafetyResult`, `TrafficV2StepRunner`, `RoadRage.Features.Vehicles.Traffic.Planning`, `GateAEvidenceParameters`, `RecoverySupervisor`, `JunctionActorReport`, `Blocker`, `TacticalDecision`, `ReferenceTrack`, `CollisionFacts`, `.Monitor`, `V2FallbackReason`, `TacticalReason`, `TrafficV2Code`, `InterStepResult`, `EffectivePolicy`, `.Evaluate`, `.Step`, `TrafficV2Settings`, `LongitudinalDecision`, `TrafficV2HazardCollector`, `TrafficV2Insertion`, `AgentObservation`, `TrafficDecisionProjection`, `SpatialEntry`, `RecoveryRequest`, `Collision`, `RoutePlan`, `NetworkedPlayerState`, `.PrepareStep`, `LongitudinalArbitration.cs`, `TrackingMeasurement.cs`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.266) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `.Monitor`, `RageRoadEventFlowController`, `RoadRage.Features.Vehicles.Traffic.Migration`, `TrafficV2Code`, `LaneGraph`, `TrafficV2VehicleDriver`, `MonoBehaviour`?**
  _High betweenness centrality (0.164) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `RoadModelRecords.cs`, `RoutePlan`, `.Localize`, `AuthoringDecisions`, `RoadCurve`, `RoadModelDocument`, `TrafficV2Code`, `RoadGeometryValidator`, `RoadModelCanonicalWriter`, `Blocker`, `RoadBoundsBox`, `RoadModelValidationIssue`, `TrafficV2HazardCollector`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.100) - this node is a cross-community bridge._
- **What connects `RoadRage.App`, `Instance`, `Router` to the rest of the system?**
  _1373 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MotionPlan` be split into smaller, more focused modules?**
  _Cohesion score 0.12681159420289856 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10960960960960961 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.10963455149501661 - nodes in this community are weakly interconnected._