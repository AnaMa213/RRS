# Graph Report - RRS  (2026-10-07)

## Corpus Check
- 183 files · ~253,801 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4923 nodes · 11030 edges · 247 communities (189 shown, 57 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 484 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a52ca8cb`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MotionPlan
- .Refine
- RoadRage.Features.Vehicles.Traffic.Planning
- GreyboxAssetSeedMetadata
- SweepPose
- RoutePlan
- GateAEvidenceParameters
- .Localize
- RecoverySupervisor
- VehicleSuspensionModel
- RouteRequest
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- LobbyRosterService
- LocalOnFootController
- Blocker
- RageTuningDef
- .Draw
- MigrationReport
- VehiclePhysicsBody
- CampaignTraceability
- TrackPiece
- .Core
- NetworkedVehicleSeatService
- AutomatedPairDecisionPolicy
- PerceivedObstacleKind
- V1ImportResult
- PlayerProfile
- RoadId
- CollisionFacts
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .Sha256Hex
- ReferenceTrack
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
- MotionCommand
- .Evaluate
- DefinitionId
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- PairReviewModel
- .Monitor
- V1Node
- V2StageTimings
- TrafficDriveOutcome
- ImportedCurve
- PassengerActionDef
- PlanningDecision
- .EvaluateInsertion
- NpcReactionEffect
- CharacterCatalog
- .Decide
- .FixedUpdate
- .TrySpawnSelectedProfile
- TrafficV2Insertion
- IPathGeometry
- AgentObservation
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- .Record
- .Collect
- PathIssue
- UserNotice
- LongitudinalArbitration.cs
- PlanningRequest
- .CheckVisuals
- RoadModelSource
- NetworkedPlayerState
- KinematicOffsetBounds
- TrackingMeasurement.cs
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- .FingerprintWithInputs
- LobbyFlowController
- RoadModelValidationIssue
- TrafficV2HazardCollector
- LobbyPlayerSlotView
- RoadLineage
- .Measure
- V1SourceSet
- JunctionConflictIndex
- TrafficLongitudinalOutcome
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- .Entry
- NetworkedPlayerReviveIntent
- RouteReason
- .UpdateSteeringState
- RageRoadEventFlowController
- MainMenuScreen
- JunctionDistances
- .HandleLifecycleChanged
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- JunctionRequestRejection
- NetworkedPlayerLifecycleIntent
- JunctionTraversal
- NetworkedRageState
- LongitudinalDecision
- JunctionClearanceResult
- TrafficSettingsDef
- RageDisposition
- DriverProfile
- StatusFilter
- MatchSettings
- ProfilerMarker
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- DrivabilityProfile
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- JunctionRecords.cs
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- DriverProfileDef
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- .FromRoute
- RoadModelCanonicalWriter
- SpeedConstraint
- .Bind
- ImportContext
- StopHoldState
- .Step
- NetworkedVehicleSeatIntent
- NetworkedVehicleState.cs
- .InterStepBound
- RoadModelVersion
- .Read
- TrafficJunctionOutcome
- SpeedPlanIssue
- AIVehicleBehaviorDebugView
- NetworkedPlayerLifecycleService
- .MergeGapAdmits
- TrafficV2Admission
- .Build
- RoutePath
- LongitudinalCandidateKind
- AuthoringDecisions
- NetworkedPassengerActionIntent
- ElementTrace
- .RenderSignoff
- V2FallbackReason
- TireSample
- .MeasurePath
- PlanningReach
- HazardRootClass
- TrafficV2Code
- BoxCollider
- Collider
- VehicleCoverage
- JunctionSnapshot
- Collision
- Dictionary
- .Import
- DriverProfileDef
- LaneGraphRouting
- JunctionControlKind
- JunctionSnapshot
- PairReviewEntry
- SignalPhaseController
- VehicleArcadeAssist
- NetworkedVehicleRecoveryIntent
- Rigidbody
- Stopwatch
- Transform
- RoadRage.Shared.Domain
- VehicleCoverage
- .Run
- LobbyCodeClipboard
- VehicleFootprint
- VehicleFootprintPose
- VehiclePhysicsBody
- NetworkedPlayerPresentation
- TrafficHazardKind
- FileLayout
- MovementRole
- NetworkedLocalPlayerPoseReporter
- PairReviewStatus
- CollisionGoalPhase
- NetworkedBossState.cs
- NetworkedCrewEconomyState.cs
- RoadElementKind
- VehicleProfile
- CompiledRoadModel
- GameObject
- Quaternion
- RoadLocation
- LaneGraph
- TrafficPerception
- TrafficDecisionProjection
- RoadCurve
- MotionCommand
- Blocker
- ComposedDrive
- DriverProfile
- JunctionActorReport
- List
- LongitudinalDecision
- PerceptionLimits
- Portal
- RouteOutcome
- RouteReason
- SpeedConstraint
- SpeedPlan
- TrackingToleranceResponse
- TrafficDriveOutcome
- TrafficHazardCollectorCounters
- TrafficV2Admission
- TrafficV2Insertion
- TrafficV2Verdict
- V2ComposerDiagnostic
- V2FallbackReason
- V2FallbackTerminal
- VehicleCoverage
- VehicleDriveIntent
- VehicleDriveIntentComposer
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 129 edges
2. `RoadId` - 101 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `TrafficFrame` - 58 edges
10. `NetworkedVehicleDriverController` - 58 edges

## Surprising Connections (you probably didn't know these)
- `TrafficV2HazardCollector` --references--> `TrafficHazardCollectorCounters`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficV2VehicleDriver` --references--> `VehicleCoverage`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficV2VehicleDriver` --references--> `TrafficDecisionProjection`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `V2StepRecord` --references--> `V2FallbackReason`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs
- `SafetyResult` --references--> `V2FallbackReason`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs → Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs

## Import Cycles
- None detected.

## Communities (247 total, 57 thin omitted)

### Community 0 - "MotionPlan"
Cohesion: 0.12
Nodes (20): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+12 more)

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (17): ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2, MovementSide (+9 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.08
Nodes (20): TrafficV2Work, TrafficV2WorkCounters, TrafficV2LifecycleState, Active, Faulted, PlanningTolerances, TacticalGoalKind, CollisionResponse (+12 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.16
Nodes (17): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, GridPath (+9 more)

### Community 5 - "RoutePlan"
Cohesion: 0.10
Nodes (19): IReadOnlyList, RoadModelVersion, RoutePlan, Diagnostics, DistanceMeters, ExitPortalId, ModelId, ModelVersion (+11 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (19): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+11 more)

### Community 7 - ".Localize"
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - "RecoverySupervisor"
Cohesion: 0.05
Nodes (40): Blocker, IReadOnlyList, List, LongitudinalDecision, RoadId, TrafficFrame, ProgressLedger, ActualMeters (+32 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "RouteRequest"
Cohesion: 0.15
Nodes (15): CompiledRoadModel, RoadLocation, DecisionCounter, RouteDiagnostic, None, ZeroWeightFallback, RouteOutcome, InvalidInput (+7 more)

### Community 11 - "RunFlowController"
Cohesion: 0.07
Nodes (4): Camera, HashSet, RunFlowController, ActiveLocalPlayer

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.13
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
Nodes (21): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+13 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.09
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 19 - ".Draw"
Cohesion: 0.24
Nodes (6): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 23 - "TrackPiece"
Cohesion: 0.18
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 24 - ".Core"
Cohesion: 0.23
Nodes (13): Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, Edge, Node (+5 more)

### Community 25 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (21): CompiledJunctionMovement, CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, RoadModelValidationProfile (+13 more)

### Community 27 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.07
Nodes (29): RoadId, RoadModelSource, AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection (+21 more)

### Community 29 - "PlayerProfile"
Cohesion: 0.11
Nodes (15): PersistentPlayerProfileRecord, PlayerNameValidator, PlayerProfile, CharacterId, DisplayName, PlayerProfileResolution, Error, IsResolved (+7 more)

### Community 30 - "RoadId"
Cohesion: 0.08
Nodes (30): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+22 more)

### Community 31 - "CollisionFacts"
Cohesion: 0.09
Nodes (26): RoadId, CollisionAnalysis, CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, CollisionResponseRequest, Facts (+18 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (32): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+24 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - ".Sha256Hex"
Cohesion: 0.11
Nodes (10): Action, KeyValuePair, MenuItem, Scene, MenuItem, MenuItem, HistoricalPairFingerprintTable, Dictionary (+2 more)

### Community 36 - "ReferenceTrack"
Cohesion: 0.21
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.11
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

### Community 41 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 42 - "JunctionReason"
Cohesion: 0.08
Nodes (25): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+17 more)

### Community 43 - "RoadCurve"
Cohesion: 0.15
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Measure"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+4 more)

### Community 45 - "PairSweep"
Cohesion: 0.19
Nodes (7): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate, RoadModelValidationProfile

### Community 46 - "TrafficV2Composition.cs"
Cohesion: 0.11
Nodes (22): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+14 more)

### Community 48 - ".Evaluate"
Cohesion: 0.06
Nodes (40): DriverProfile, LongitudinalDecision, SpeedConstraint, SpeedPlan, Vector3, MotionCommand, IsFinite, RoadId (+32 more)

### Community 49 - "DefinitionId"
Cohesion: 0.14
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.08
Nodes (31): Bounds, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IReadOnlyList (+23 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReviewModel"
Cohesion: 0.25
Nodes (5): CompiledRoadModel, List, StringBuilder, PairReview, PairReviewModel

### Community 54 - ".Monitor"
Cohesion: 0.08
Nodes (26): TrackingTolerance, TrackingToleranceResponse, Latched, LatchedAtStep, DriverProfile, List, RoadId, SpeedConstraint (+18 more)

### Community 55 - "V1Node"
Cohesion: 0.17
Nodes (11): JunctionFeature, Predicate, ImportedJunction, V1Edge, Key, V1Node, IsEntryPortal, IsExitPortal (+3 more)

### Community 56 - "V2StageTimings"
Cohesion: 0.17
Nodes (7): JunctionActorReport, TrafficFrame, V2StageTimings, TotalMillisecondsPerStep, JunctionBlockerCause, JunctionConflictIndex, JunctionSnapshot

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.10
Nodes (19): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+11 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.16
Nodes (13): Dictionary, IList, IReadOnlyList, KeyValuePair, List, RoadCurve, RoadCurveSample, RoadRecordKind (+5 more)

### Community 59 - "PassengerActionDef"
Cohesion: 0.11
Nodes (14): List, PassengerActionCatalog, Count, Version, PassengerActionDef, CooldownSeconds, DisplayName, Id (+6 more)

### Community 60 - "PlanningDecision"
Cohesion: 0.13
Nodes (17): AgentObservation, ProfilerMarker, TrafficDecisionProjection, PlanningDecision, Motion, Observation, Path, PerceptionPath (+9 more)

### Community 61 - ".EvaluateInsertion"
Cohesion: 0.20
Nodes (9): TrackingTolerance, VehicleCoverage, TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, TrafficV2Verdict, Allowed (+1 more)

### Community 62 - "NpcReactionEffect"
Cohesion: 0.11
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 63 - "CharacterCatalog"
Cohesion: 0.19
Nodes (7): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PlayerProfileBootstrapService

### Community 64 - ".Decide"
Cohesion: 0.24
Nodes (5): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, StopHoldParameters

### Community 65 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 66 - ".TrySpawnSelectedProfile"
Cohesion: 0.13
Nodes (11): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner, Collider, GameObject (+3 more)

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.10
Nodes (21): DriverProfile, Portal, RoadId, RoutePlan, RouteSeed, SpeedPlan, Vector3, TrafficV2Insertion (+13 more)

### Community 68 - "IPathGeometry"
Cohesion: 0.23
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 69 - "AgentObservation"
Cohesion: 0.14
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 70 - ".Compute"
Cohesion: 0.33
Nodes (7): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, PairGeometryFingerprint

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.08
Nodes (26): RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus, ExitPortalId (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.09
Nodes (25): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+17 more)

### Community 73 - ".Record"
Cohesion: 0.12
Nodes (15): IReadOnlyList, JunctionRecord, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations (+7 more)

### Community 74 - ".Collect"
Cohesion: 0.16
Nodes (13): TrafficHazardCollectorCounters, Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3 (+5 more)

### Community 75 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "LongitudinalArbitration.cs"
Cohesion: 0.14
Nodes (18): IReadOnlyList, RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalMemory, None, LongitudinalObstacle (+10 more)

### Community 78 - "PlanningRequest"
Cohesion: 0.20
Nodes (10): IReadOnlyList, RoadId, RoutePlan, RouteSeed, TrackingTolerance, TrafficFrame, PlanningRequest, LongitudinalBounds (+2 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.25
Nodes (5): Surface, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "RoadModelSource"
Cohesion: 0.07
Nodes (32): AdjacencyDto, DrivabilityProfile, EffectiveLaneCorridor, Comparison, RoadModelCompiler, ModelDto, LaneCorridor, RoadLocalizationProfile (+24 more)

### Community 81 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.09
Nodes (20): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+12 more)

### Community 83 - "TrackingMeasurement.cs"
Cohesion: 0.25
Nodes (8): InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees, PieceBound

### Community 84 - "JunctionCoordinator"
Cohesion: 0.15
Nodes (20): CompiledRoadModel, Dictionary, HashSet, KeyValuePair, List, RoadId, Grant, JunctionCoordinator (+12 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.16
Nodes (12): Vector2, ConflictSweep, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState, MovementSide (+4 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (13): JunctionRecord, RoadCurveSample, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord (+5 more)

### Community 87 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 88 - "LobbyFlowController"
Cohesion: 0.07
Nodes (19): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, Task, LobbyRoomService, DisplayJoinCode (+11 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.14
Nodes (21): JunctionMovement, JunctionControl, RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement (+13 more)

### Community 90 - "TrafficV2HazardCollector"
Cohesion: 0.12
Nodes (16): Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity, Counters (+8 more)

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.05
Nodes (43): ConditionalWeakTable, Dictionary, IReadOnlyList, JunctionControl, JunctionMovement, LaneCorridor, List, RoadId (+35 more)

### Community 93 - ".Measure"
Cohesion: 0.15
Nodes (15): BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, Scene, Vector3 (+7 more)

### Community 94 - "V1SourceSet"
Cohesion: 0.13
Nodes (20): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+12 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.10
Nodes (20): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+12 more)

### Community 96 - "TrafficLongitudinalOutcome"
Cohesion: 0.12
Nodes (15): AgentObservation, Blocker, LongitudinalDecision, TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant (+7 more)

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.11
Nodes (17): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+9 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.31
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference

### Community 102 - ".Entry"
Cohesion: 0.29
Nodes (3): Vector3, HistoricalPairFingerprintRecord, RoadBoundsBox

### Community 103 - "NetworkedPlayerReviveIntent"
Cohesion: 0.20
Nodes (7): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, HostOwnedNetworkStateBehaviour, IsHostAuthority, NetworkBehaviour

### Community 104 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.07
Nodes (22): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+14 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.40
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

### Community 114 - "JunctionTraversal"
Cohesion: 0.22
Nodes (8): TraversalComparer, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds, IEqualityComparer

### Community 115 - "NetworkedRageState"
Cohesion: 0.22
Nodes (3): NetworkVariable, NetworkedRageState, CurrentDisposition

### Community 116 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 117 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 120 - "DriverProfile"
Cohesion: 0.11
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "MatchSettings"
Cohesion: 0.29
Nodes (6): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, RoadRage.Features.Lobby

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.03
Nodes (79): StepContactAccumulator, AgentObservation, Blocker, CollisionFacts, CollisionResponseRequest, IReadOnlyList, LongitudinalDecision, Portal (+71 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.18
Nodes (6): IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.05
Nodes (31): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+23 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (18): HashSet, List, ProfilerMarker, RoadId, Stopwatch, TrafficV2StepRunner, Collector, Coordinator (+10 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (11): Difficulty, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, Difficulty, Easy (+3 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.07
Nodes (12): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen (+4 more)

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.05
Nodes (55): CompiledConflictZone, Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile, ImportManifest (+47 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - "JunctionRecords.cs"
Cohesion: 0.15
Nodes (10): JunctionApproach, Crossed, JunctionExitAssessment, JunctionMovementPosition, JunctionMovementStatus, Ahead, Behind, Occupied (+2 more)

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
Cohesion: 0.11
Nodes (16): DriverProfileDef, CollisionReaction, Id, Profile, RawId, CollisionReaction, Brake, Evade (+8 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.09
Nodes (18): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, InFallback (+10 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - ".FromRoute"
Cohesion: 0.50
Nodes (3): CompiledRoadModel, IReadOnlyList, List

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.22
Nodes (7): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalPayload, RoadModelCanonicalWriter, ConflictZone

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - ".Bind"
Cohesion: 0.18
Nodes (11): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+3 more)

### Community 148 - "ImportContext"
Cohesion: 0.25
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 149 - "StopHoldState"
Cohesion: 0.12
Nodes (13): StopHoldPhase, Entered, Holding, None, Released, StopHoldRelease, GapOpened, GrantEffective (+5 more)

### Community 150 - ".Step"
Cohesion: 0.05
Nodes (45): CollisionFacts, CollisionResponseRequest, RouteSeed, RecoveryCommandInput, RecoveryMotion, TacticalDecision, AcceptedAtFrame, Active (+37 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "NetworkedVehicleState.cs"
Cohesion: 0.21
Nodes (6): VehicleDamageType, Brake, Engine, Wheel, IHostOwnedRuntimeState, IsHostAuthority

### Community 153 - ".InterStepBound"
Cohesion: 0.29
Nodes (7): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, TrackingMeasurement

### Community 154 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "TrafficJunctionOutcome"
Cohesion: 0.17
Nodes (11): JunctionActorReport, TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame (+3 more)

### Community 157 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 158 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 161 - "TrafficV2Admission"
Cohesion: 0.27
Nodes (8): TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle, GameObject, GateAEvidenceResult

### Community 162 - ".Build"
Cohesion: 0.16
Nodes (12): JunctionKinematics, Known, ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId, Vector3 (+4 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - "LongitudinalCandidateKind"
Cohesion: 0.22
Nodes (9): LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable, Profile, SteeringCeilingUnreachable (+1 more)

### Community 165 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (46): ConflictKind, Dictionary, FileLayout, Func, IEnumerable, IList, JunctionControlKind, List (+38 more)

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.06
Nodes (36): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+28 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - ".RenderSignoff"
Cohesion: 0.15
Nodes (7): CompiledJunctionControl, CompiledRoadModel, RoadCurveSample, IList, SignoffLayout, DateTime, SignoffLayout

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (35): VehicleDriveIntent, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None, ProfileScalarNonFinite (+27 more)

### Community 170 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 173 - "HazardRootClass"
Cohesion: 0.25
Nodes (7): HazardRootClass, Obstacle, Self, Static, TrafficV2Vehicle, Vehicle, WalkingPlayer

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 177 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 178 - "JunctionSnapshot"
Cohesion: 0.18
Nodes (11): JunctionSnapshot, JunctionControlKind, RoadId, JunctionBatchCounters, JunctionRecord, IsEffectiveGrant, JunctionSnapshot, Counters (+3 more)

### Community 181 - ".Import"
Cohesion: 0.25
Nodes (4): DrivabilityProfile, RoadLocalizationProfile, RoadModelValidationProfile, V1RoadModelImporter

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 186 - "PairReviewEntry"
Cohesion: 0.17
Nodes (8): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, PairReviewActions, PairReviewEntry

### Community 187 - "SignalPhaseController"
Cohesion: 0.29
Nodes (6): CompiledRoadModel, IReadOnlyList, SignalPhaseController, Current, Model, CompiledSignalPlan

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (18): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+10 more)

### Community 195 - ".Run"
Cohesion: 0.48
Nodes (4): CompiledRoadModel, IEnumerable, JunctionSnapshot, TrafficV2StepCost

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.15
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.14
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 201 - "TrafficHazardKind"
Cohesion: 0.33
Nodes (5): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer

### Community 202 - "FileLayout"
Cohesion: 0.33
Nodes (6): FileLayout, ConflictRecord, ControlRecord, DeferredRecord, DispositionRecord, WidthRecord

### Community 203 - "MovementRole"
Cohesion: 0.33
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 206 - "PairReviewStatus"
Cohesion: 0.40
Nodes (5): PairReviewStatus, Modified, New, Removed, Unchanged

### Community 207 - "CollisionGoalPhase"
Cohesion: 0.40
Nodes (5): CollisionGoalPhase, AwaitingRecovery, Braking, None, Reacting

### Community 208 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 209 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 216 - "LaneGraph"
Cohesion: 0.11
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.12
Nodes (22): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+14 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1293 isolated node(s):** `FrameId`, `Observation`, `Decision`, `RoadLimits`, `Blockers` (+1288 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1874 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **57 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2Admission`, `RoadRage.Features.Vehicles.Traffic.Planning`, `.Run`, `TrafficV2StepRunner`, `TrafficV2Insertion`, `GateAEvidenceParameters`, `TrafficDecisionProjection`, `RecoverySupervisor`, `V2FallbackReason`, `.Collect`, `NetworkedPlayerReviveIntent`, `.Monitor`, `.Step`, `V2StageTimings`, `TrafficDriveOutcome`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.190) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2Admission`, `TrafficV2StepRunner`, `MonoBehaviour`, `RageRoadEventFlowController`, `TrafficV2Composition.cs`, `TrafficV2Code`, `TrafficSettingsDef`, `LaneGraph`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.171) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `AutomatedPairDecisionPolicy`, `RoadModelRecords.cs`, `SpeedPlan`, `.Localize`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadModelSource`, `RoadModelCanonicalWriter`, `AuthoredRoadModel`, `RoadModelValidationIssue`, `TrafficV2HazardCollector`, `RoadLineage`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.108) - this node is a cross-community bridge._
- **What connects `FrameId`, `Observation`, `Decision` to the rest of the system?**
  _1293 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MotionPlan` be split into smaller, more focused modules?**
  _Cohesion score 0.11692307692307692 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10960960960960961 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.07665505226480836 - nodes in this community are weakly interconnected._