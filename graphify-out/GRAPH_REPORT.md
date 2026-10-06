# Graph Report - RRS  (2026-10-06)

## Corpus Check
- 174 files · ~234,228 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4437 nodes · 10222 edges · 187 communities (165 shown, 22 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 447 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `b467be26`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PairRefinement
- .Refine
- TrafficV2Composition.cs
- GreyboxAssetSeedMetadata
- SweepPose
- .Core
- GateAEvidenceParameters
- RoadId
- TireSample
- VehicleSuspensionModel
- LobbyRosterService
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- .TryGetMovement
- LocalOnFootController
- Blocker
- RageTuningDef
- JunctionTraversal
- MigrationReport
- VehiclePhysicsBody
- LobbyRosterScreen
- AgentObservation
- AuthoredRoadModel
- .Measure
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1ImportResult
- JunctionSnapshot
- TrafficV2HazardCollector
- AuthoringDecisions
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- TrafficPerception
- RoutePlan
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- LongitudinalArbitration.cs
- ConflictSweep
- IPathGeometry
- MotionPlan
- JunctionActorReport
- PairReviewModel
- .Decide
- TrafficDriveOutcome
- .Regenerate
- MainMenuScreen
- .GridPaths
- .ImportLaneModule
- RoadBoundsBox
- VehicleDriveIntent
- ImportedCurve
- .MaximumSteerRateDegreesPerSecond
- .FullPath
- LongitudinalCandidateKind
- .FixedUpdate
- .Collect
- .HandleRosterChanged
- TrafficHazardInput
- .Evaluate
- FacepunchSteamLobbyPlatform
- NetworkedRunSessionMonitor
- .EvaluateLeaf
- .Compute
- TrafficDecisionProjection
- SpatialEntry
- HazardRootClass
- .Assemble
- .Build
- UserNotice
- VehicleCoverage
- TrafficLongitudinalOutcome
- PassengerActionVerdictCode
- OffsetTrace
- StopHoldRelease
- KinematicOffsetBounds
- AppSceneRouter.cs
- JunctionCoordinator
- TrafficV2Settings
- LongitudinalCandidate
- StopHoldState
- LobbyFlowController
- RoadModelValidationIssue
- Dictionary
- ConflictKind
- RoadLineage
- DefinitionId
- V1Node
- JunctionConflictIndex
- TrackingTolerance
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- .Create
- DispositionKind
- JunctionRequestRejection
- .UpdateSteeringState
- NetworkedAIVehicleState
- MenuCharacterPreview
- JunctionDistances
- RoadGeometryValidator
- NetworkedBossState.cs
- MatchSettings
- JunctionExitBound
- TrafficJunctionOutcome
- NetworkedCrewEconomyState.cs
- TrafficSettingsDef
- .Manifest
- DriverProfile
- LeafState
- RouteReason
- RoadRage.Features.Vehicles.Traffic.Planning
- MainMenuFlowController
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- RoadRageBootstrap
- TrafficV2StepRunner
- .Awake
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- IHostOwnedRuntimeState
- RoadModelDocument
- LaneNode
- RoadModelCanonicalWriter
- SpeedConstraint
- RunEscapeMenuScreen
- ImportContext
- .Step
- PlanningDecision
- NetworkedVehicleSeatIntent
- ReferenceTrack
- RoadRage.Features.Vehicles.Traffic.Migration
- LongitudinalDecision
- TrafficV2Insertion
- PathIssue
- NetworkedPlayerState
- PlayerNameValidator
- RoutePath
- NetworkedPassengerActionIntent
- CampaignTraceability
- SpeedPlan.cs
- V2FallbackReason
- NetworkedVehicleState.cs
- NetworkedPlayerLifecycleIntent
- LobbyPlayerSlotView
- TrafficV2Code
- FileLayout
- Func
- JunctionControlKind
- GateAEvidenceResult
- StatusFilter
- NetworkedPlayerReviveIntent
- RoadBoundsBox
- RoadCurveSample
- LaneGraphRouting
- CompiledJunctionMovement
- CompiledRoadModel
- HashSet
- MenuItem
- SortedDictionary
- NetworkedVehicleRecoveryIntent
- ModelDto
- PairRelation
- Vector3
- RoadRage.Shared.Domain
- StringBuilder
- Vector2
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- LaneGraph
- .Observe
- DriverProfileDef
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 107 edges
2. `RoadId` - 102 edges
3. `RunFlowController` - 99 edges
4. `NetworkedVehicleState` - 67 edges
5. `ImportContext` - 67 edges
6. `AuthoredRoadModel` - 63 edges
7. `CompiledRoadModel` - 63 edges
8. `TrafficFrame` - 59 edges
9. `NetworkedVehicleDriverController` - 58 edges
10. `VehicleProfile` - 51 edges

## Surprising Connections (you probably didn't know these)
- `PairReviewModel` --references--> `PairSweep`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs
- `PairReviewModel` --references--> `ShortElement`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs
- `PairReviewModel` --references--> `V1ImportResult`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs
- `PairReviewWindow` --references--> `PairReviewModel`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReviewWindow.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs
- `SegmentWalker` --references--> `RoadCurveSample`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/RoadCurveBuilder.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs

## Import Cycles
- None detected.

## Communities (187 total, 22 thin omitted)

### Community 0 - "PairRefinement"
Cohesion: 0.14
Nodes (12): PairRefinement, RefinementOutcome, BudgetExhausted, ProvenDisjoint, Unresolved, Witness, ZoneTyping, Conservative (+4 more)

### Community 1 - ".Refine"
Cohesion: 0.15
Nodes (18): IList, List, RoadId, RoadModelValidationProfile, SweepGraph, SweepPose, ConflictSweep, MovementSide (+10 more)

### Community 2 - "TrafficV2Composition.cs"
Cohesion: 0.11
Nodes (23): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+15 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.17
Nodes (16): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, GridPath (+8 more)

### Community 5 - ".Core"
Cohesion: 0.18
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.13
Nodes (14): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+6 more)

### Community 7 - "RoadId"
Cohesion: 0.06
Nodes (35): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+27 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "LobbyRosterService"
Cohesion: 0.09
Nodes (15): Difficulty, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId (+7 more)

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.10
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "RoadRage.Features.Online"
Cohesion: 0.07
Nodes (16): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+8 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - ".TryGetMovement"
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (19): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+11 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.05
Nodes (31): TMP_Text, RageStateDebugView, List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue (+23 more)

### Community 19 - "JunctionTraversal"
Cohesion: 0.11
Nodes (17): RoadId, JunctionApproach, Crossed, JunctionExitAssessment, JunctionMovementPosition, JunctionMovementStatus, Ahead, Behind (+9 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 23 - "AgentObservation"
Cohesion: 0.14
Nodes (17): Func, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AgentObservation, ExitOccupancyFact, IntentOverlapKind (+9 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (35): Bounds, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, IReadOnlyList (+27 more)

### Community 25 - ".Measure"
Cohesion: 0.07
Nodes (36): Surface, BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, IEnumerable, IReadOnlyList (+28 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (26): IList, List, RoadId, RoadModelValidationProfile, SweepGraph, SweepPose, AutomatedPairDecisionManifest, AutomatedPairDecisionPlan (+18 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.14
Nodes (17): AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedConnection, ImportedPortal, ImportedSection, PublishedDisplacement (+9 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.18
Nodes (9): JunctionSnapshot, JunctionBatchCounters, JunctionRecord, IsEffectiveGrant, JunctionSnapshot, Counters, EffectiveFrame, Records (+1 more)

### Community 30 - "TrafficV2HazardCollector"
Cohesion: 0.12
Nodes (16): Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity, Counters (+8 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (47): AppliedWidth, ConflictKind, Dictionary, IList, List, RoadId, AuthoringDecisions, AutomatedPairClassification (+39 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (32): CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample, RoadElementKind (+24 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.09
Nodes (11): Action, Collider, Collision, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc (+3 more)

### Community 35 - "TrafficPerception"
Cohesion: 0.13
Nodes (16): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+8 more)

### Community 36 - "RoutePlan"
Cohesion: 0.12
Nodes (17): IReadOnlyList, RoadModelVersion, RoutePlan, Diagnostics, DistanceMeters, ExitPortalId, ModelId, ModelVersion (+9 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.08
Nodes (12): Vector3, NetworkedVehicleSeatService, Instance, Func, NetworkVariable, Vector3, NetworkedVehicleState, CurrentDamageThresholdsCrossed (+4 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.14
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.10
Nodes (24): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd, ExitPortal (+16 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.11
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.15
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - "LongitudinalArbitration.cs"
Cohesion: 0.17
Nodes (13): IReadOnlyList, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason (+5 more)

### Community 45 - "ConflictSweep"
Cohesion: 0.26
Nodes (7): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, ConflictSweep, PairSweep, IsCandidate

### Community 46 - "IPathGeometry"
Cohesion: 0.23
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 47 - "MotionPlan"
Cohesion: 0.15
Nodes (15): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+7 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.11
Nodes (16): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, Localized (+8 more)

### Community 49 - "PairReviewModel"
Cohesion: 0.09
Nodes (21): RoadBoundsBox, Vector3, HistoricalPairFingerprintRecord, CompiledRoadModel, Dictionary, JunctionRecord, List, RoadBoundsBox (+13 more)

### Community 50 - ".Decide"
Cohesion: 0.27
Nodes (4): DriverProfile, List, LongitudinalArbitration, StopHoldParameters

### Community 51 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (16): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+8 more)

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 54 - ".GridPaths"
Cohesion: 0.28
Nodes (4): Vector2, PoseFrame, RoadModelValidationProfile, PoseFrame

### Community 55 - ".ImportLaneModule"
Cohesion: 0.24
Nodes (4): Dictionary, JunctionFeature, Predicate, ImportedJunction

### Community 56 - "RoadBoundsBox"
Cohesion: 0.15
Nodes (14): CompiledConflictZone, Vector3, ConflictKind, Crossing, Merge, ConflictZone, Junction, JunctionFeature (+6 more)

### Community 57 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 58 - "ImportedCurve"
Cohesion: 0.22
Nodes (9): List, RoadCurve, RoadCurveSample, ImportedCurve, MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit (+1 more)

### Community 59 - ".MaximumSteerRateDegreesPerSecond"
Cohesion: 0.20
Nodes (5): DrivabilityProfile, NominalPoseFeasibility, NominalPoseFeasibilityInputs, Valid, DrivabilityProfile

### Community 60 - ".FullPath"
Cohesion: 0.19
Nodes (7): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func, MenuItem

### Community 61 - "LongitudinalCandidateKind"
Cohesion: 0.18
Nodes (11): LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable, Profile, SteeringCeilingUnreachable (+3 more)

### Community 62 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 63 - ".Collect"
Cohesion: 0.16
Nodes (13): TrafficHazardCollectorCounters, Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3 (+5 more)

### Community 64 - ".HandleRosterChanged"
Cohesion: 0.24
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 65 - "TrafficHazardInput"
Cohesion: 0.22
Nodes (8): RoadBoundsBox, Vector3, TrafficHazardInput, TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer

### Community 66 - ".Evaluate"
Cohesion: 0.14
Nodes (13): DriverProfile, PlanningReach, IReadOnlyList, ProfilerMarker, PlanningRequest, PlanningSpine, CompiledRoadModel, RoadId (+5 more)

### Community 67 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.07
Nodes (24): Difficulty, Task, FacepunchSteamLobbyPlatform, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId, Success (+16 more)

### Community 68 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 70 - ".Compute"
Cohesion: 0.23
Nodes (7): BinaryWriter, IReadOnlyList, RoadCurveSample, RoadId, RoadModelValidationProfile, HistoricalPairFingerprintTable, PairGeometryFingerprint

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "SpatialEntry"
Cohesion: 0.10
Nodes (22): Bounds, Bounds, IReadOnlyList, RoadElementKind, RoadId, ElementClosureInput, IntentInterval, OccupancyExclusion (+14 more)

### Community 73 - "HazardRootClass"
Cohesion: 0.25
Nodes (7): HazardRootClass, Obstacle, Self, Static, TrafficV2Vehicle, Vehicle, WalkingPlayer

### Community 74 - ".Assemble"
Cohesion: 0.24
Nodes (5): DrivabilityProfile, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter

### Community 75 - ".Build"
Cohesion: 0.36
Nodes (4): IReadOnlyList, Vector2, KinematicPoseSet, PoseGrid

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 78 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - "PassengerActionVerdictCode"
Cohesion: 0.08
Nodes (22): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+14 more)

### Community 80 - "OffsetTrace"
Cohesion: 0.38
Nodes (3): OffsetTrace, EndS, StartS

### Community 81 - "StopHoldRelease"
Cohesion: 0.33
Nodes (6): StopHoldRelease, GapOpened, GrantEffective, None, SourceDeparted, SourceGone

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.19
Nodes (12): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+4 more)

### Community 83 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 84 - "JunctionCoordinator"
Cohesion: 0.18
Nodes (17): CompiledRoadModel, Dictionary, List, RoadId, Grant, JunctionCoordinator, Batches, Current (+9 more)

### Community 85 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 86 - "LongitudinalCandidate"
Cohesion: 0.50
Nodes (4): RoadId, JunctionEntryInput, Active, LongitudinalCandidate

### Community 87 - "StopHoldState"
Cohesion: 0.18
Nodes (7): StopHoldPhase, Entered, Holding, None, Released, StopHoldState, Active

### Community 88 - "LobbyFlowController"
Cohesion: 0.11
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (19): RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "DefinitionId"
Cohesion: 0.05
Nodes (32): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+24 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (29): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+21 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.11
Nodes (20): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, Portal, RoadBoundsBox, RoadId, Vector3 (+12 more)

### Community 96 - "TrackingTolerance"
Cohesion: 0.31
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

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

### Community 102 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 103 - "DispositionKind"
Cohesion: 0.18
Nodes (11): DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint, Movement, MovementApproachPath (+3 more)

### Community 104 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 106 - "NetworkedAIVehicleState"
Cohesion: 0.06
Nodes (31): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, IReadOnlyList, RageRoadEventLifecycle, IReadOnlyList, Vector3 (+23 more)

### Community 107 - "MenuCharacterPreview"
Cohesion: 0.18
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 114 - "MatchSettings"
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 115 - "JunctionExitBound"
Cohesion: 0.12
Nodes (15): IReadOnlyList, JunctionRecord, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations (+7 more)

### Community 116 - "TrafficJunctionOutcome"
Cohesion: 0.18
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 117 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - ".Manifest"
Cohesion: 0.24
Nodes (8): IList, IReadOnlyList, KeyValuePair, RoadId, RoadRecordKind, LineageKeyRegistry, Keys, ImportManifestEntry

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 122 - "RouteReason"
Cohesion: 0.12
Nodes (19): RouteDiagnostic, None, ZeroWeightFallback, RouteOutcome, InvalidInput, NoRoute, Planned, Replanned (+11 more)

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.15
Nodes (11): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing (+3 more)

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.07
Nodes (27): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, PairDecisionState, Confirmed, Missing, Orphan (+19 more)

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.05
Nodes (28): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+20 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (21): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+13 more)

### Community 131 - ".Awake"
Cohesion: 0.12
Nodes (4): Button, TMP_Text, LobbyShellScreen, TMP_InputField

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.16
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.05
Nodes (66): DrivabilityProfile, EffectiveLaneCorridor, JunctionMovement, RoadModelCanonicalPayload, Comparison, RoadModelCompiler, DrivabilityProfile, ImportManifest (+58 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.12
Nodes (20): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, RoadLimitValue, CapMetersPerSecond (+12 more)

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (46): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference (+38 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (36): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+28 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 148 - "ImportContext"
Cohesion: 0.22
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 149 - ".Step"
Cohesion: 0.08
Nodes (20): DriverProfile, IReadOnlyList, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord (+12 more)

### Community 150 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 153 - "ReferenceTrack"
Cohesion: 0.07
Nodes (32): IReadOnlyList, Quaternion, RoadCurve, RoadElementKind, RoadId, RoadKinematicAnchor, Vector3, BodyState (+24 more)

### Community 155 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.17
Nodes (7): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, RoadRage.Features.Vehicles.Traffic.Migration

### Community 156 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.09
Nodes (25): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+17 more)

### Community 158 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 159 - "NetworkedPlayerState"
Cohesion: 0.06
Nodes (27): IEnumerable, NetworkedPlayerLifecycleService, Instance, Dictionary, GameObject, HashSet, IEnumerator, NetworkManager (+19 more)

### Community 163 - "RoutePath"
Cohesion: 0.21
Nodes (8): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.05
Nodes (44): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+36 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.08
Nodes (23): CompiledRoadModel, Dictionary, HashSet, IReadOnlyDictionary, List, CampaignTraceability, Elements, ElementStatus (+15 more)

### Community 168 - "SpeedPlan.cs"
Cohesion: 0.14
Nodes (15): DeferredLimit, DeferredLimitKind, RoadLimit, DeferredLimitState, DeferredAuthored, DeferredUnauthored, RoadLimitState, Applied (+7 more)

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
Cohesion: 0.14
Nodes (14): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+6 more)

### Community 179 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 180 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "ModelDto"
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 191 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (19): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+11 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.11
Nodes (13): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+5 more)

### Community 217 - ".Observe"
Cohesion: 0.10
Nodes (22): IReadOnlyList, LaneSide, AdjacentOccupantFact, ObservationChannel, Items, RangeMeters, Saturated, Status (+14 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.27
Nodes (5): DriverProfileDef, Id, Profile, RawId, ScriptableObject

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.11
Nodes (11): BoxCollider, CharacterController, Collider, List, NetworkTransform, Quaternion, RaycastHit, Rigidbody (+3 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1127 isolated node(s):** `ProvenDisjoint`, `Witness`, `Unresolved`, `BudgetExhausted`, `Conservative` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1600 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **22 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `Blocker`, `.Step`, `AgentObservation`, `ReferenceTrack`, `LongitudinalDecision`, `TrafficV2Insertion`, `RoutePlan`, `NetworkedPassengerActionIntent`, `V2FallbackReason`, `JunctionActorReport`, `LongitudinalCandidateKind`, `.Collect`, `TrafficDecisionProjection`, `SpatialEntry`, `JunctionConflictIndex`, `TrackingTolerance`, `RoadRage.Features.Vehicles.Traffic.Planning`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.178) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2Composition.cs`, `TrafficV2StepRunner`, `NetworkedPassengerActionIntent`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `.Step`, `LaneGraph`, `TrafficV2Insertion`, `NetworkedPlayerState`?**
  _High betweenness centrality (0.166) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `.Evaluate`, `RoadModelRecords.cs`, `RoadModelDocument`, `RoadGeometryValidator`, `.TryGetMovement`, `RoadModelCanonicalWriter`, `Blocker`, `RoadBoundsBox`, `RoadModelValidationIssue`, `RoadLineage`, `TrafficV2Insertion`, `TrafficV2HazardCollector`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.147) - this node is a cross-community bridge._
- **What connects `ProvenDisjoint`, `Witness`, `Unresolved` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PairRefinement` be split into smaller, more focused modules?**
  _Cohesion score 0.14285714285714285 - nodes in this community are weakly interconnected._
- **Should `TrafficV2Composition.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.10541310541310542 - nodes in this community are weakly interconnected._
- **Should `GateAEvidenceParameters` be split into smaller, more focused modules?**
  _Cohesion score 0.12857142857142856 - nodes in this community are weakly interconnected._