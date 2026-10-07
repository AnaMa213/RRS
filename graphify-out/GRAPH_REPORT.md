# Graph Report - RRS  (2026-10-07)

## Corpus Check
- 182 files · ~249,914 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4794 nodes · 10879 edges · 205 communities (180 shown, 25 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 473 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `04753e4a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MotionPlan
- .Refine
- RoadRage.Features.Vehicles.Traffic.Migration
- GreyboxAssetSeedMetadata
- SweepElement
- RoutePlan
- GateAEvidenceParameters
- .Localize
- .Add
- .FixedUpdate
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
- PassengerActionVerdictCode
- AutomatedPairDecisionPolicy
- .Obstacles
- V1RoadModelImporter.cs
- MainMenuProfileFlowController
- RoadId
- CollisionFacts
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .FullPath
- ReferenceTrack
- NetworkedVehicleState
- VehicleProfile
- V1ImportResult
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- .Measure
- SweepPose
- MeasurementRun
- .Monitor
- .Evaluate
- DefinitionId
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- PairReviewModel
- V2StepRecord
- .AddMovement
- .Step
- TrafficDriveOutcome
- ImportedCurve
- SpatialEntry
- PlanningDecision
- TrackingTolerance
- NpcReactionEffect
- RightOfWayRelation
- .Decide
- VehicleTireModel
- .Create
- TrafficV2Insertion
- IPathGeometry
- AgentObservation
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- RightOfWayTable
- TrafficV2HazardCollector
- PathIssue
- UserNotice
- LongitudinalPerception
- PlanningRequest
- .CheckVisuals
- ModelDto
- PlayerMode
- KinematicOffsetBounds
- InterStepResult
- JunctionCoordinator
- ConflictSweep
- .Build
- .FingerprintWithInputs
- LobbyFlowController
- RoadModelValidationIssue
- .AccumulateStep
- LobbyPlayerSlotView
- RoadLineage
- .Measure
- V1Node
- JunctionConflictIndex
- TrafficLongitudinalOutcome
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- .Entry
- .Build
- RouteReason
- .UpdateSteeringState
- RageRoadEventFlowController
- MainMenuScreen
- JunctionDistances
- NetworkedRunSessionMonitor
- RoadGeometryValidator
- .Crossings
- JunctionRequestRejection
- SafetyReason
- JunctionTraversal
- .ValidateApproaches
- LongitudinalDecision
- JunctionClearanceResult
- TrafficSettingsDef
- RageDisposition
- DriverProfile
- StatusFilter
- MatchSettings
- .PrepareStep
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- .Track
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- JunctionRecord
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- DriverProfileDef
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- .TryGetCorridor
- RoadModelCanonicalWriter
- SpeedConstraint
- .ToRecord
- ImportContext
- StopHoldPhase
- TacticalDecision
- NetworkedVehicleSeatIntent
- NetworkedVehicleState.cs
- TrackingMeasurement.cs
- RoadModelVersion
- .Read
- PathSpan
- SpeedPlanIssue
- AIVehicleBehaviorDebugView
- NetworkedPlayerState
- ObservationChannel
- SafetyVerdict
- .Build
- RoutePath
- PlayerNameValidator
- AuthoringDecisions
- NetworkedPassengerActionIntent
- ElementTrace
- .TryApproachEnd
- V2FallbackReason
- .Q
- .MeasurePath
- PlanningReach
- TrafficV2WorkCounters.cs
- TrafficV2Code
- BoxCollider
- Collider
- VehicleCoverage
- JunctionSnapshot
- Collision
- Dictionary
- DriverProfile
- DriverProfileDef
- LaneGraphRouting
- JunctionControlKind
- JunctionSnapshot
- PairReviewEntry
- List
- Portal
- NetworkedVehicleRecoveryIntent
- Rigidbody
- Stopwatch
- Transform
- RoadRage.Shared.Domain
- VehicleCoverage
- VehicleDriveIntent
- LobbyCodeClipboard
- VehicleFootprint
- VehicleFootprintPose
- VehiclePhysicsBody
- NetworkedPlayerPresentation
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 121 edges
2. `RoadId` - 102 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `TrafficFrame` - 59 edges
10. `NetworkedVehicleDriverController` - 58 edges

## Surprising Connections (you probably didn't know these)
- `TrafficV2VehicleDriver` --references--> `CollisionFacts`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs
- `TacticalDecision` --references--> `StabilityBlocker`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs → Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs
- `TrafficV2VehicleDriver` --references--> `StepContactAccumulator`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs
- `TrafficV2VehicleDriver` --references--> `CollisionResponseRequest`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs
- `TrafficV2VehicleDriver` --references--> `CollisionAnalysis`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs

## Import Cycles
- None detected.

## Communities (205 total, 25 thin omitted)

### Community 0 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 1 - ".Refine"
Cohesion: 0.10
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.12
Nodes (10): PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing, RoadRage.Features.Vehicles.Traffic.Signals (+2 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.18
Nodes (13): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+5 more)

### Community 5 - "RoutePlan"
Cohesion: 0.10
Nodes (19): IReadOnlyList, RoadModelVersion, RoutePlan, Diagnostics, DistanceMeters, ExitPortalId, ModelId, ModelVersion (+11 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - ".Localize"
Cohesion: 0.12
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - ".Add"
Cohesion: 0.17
Nodes (14): Bounds, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation, RoadModelValidationProfile (+6 more)

### Community 9 - ".FixedUpdate"
Cohesion: 0.09
Nodes (16): VehicleArcadeAssist, Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip (+8 more)

### Community 10 - "RouteRequest"
Cohesion: 0.15
Nodes (16): CompiledRoadModel, RoadId, RoadLocation, DecisionCounter, RouteDiagnostic, None, ZeroWeightFallback, RouteOutcome (+8 more)

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
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyRosterService"
Cohesion: 0.04
Nodes (38): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+30 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.08
Nodes (26): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+18 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 19 - ".Draw"
Cohesion: 0.23
Nodes (7): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, RoadModelValidationProfile, SceneView

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "CampaignTraceability"
Cohesion: 0.22
Nodes (5): Dictionary, HashSet, IReadOnlyDictionary, CampaignTraceability, Elements

### Community 23 - "TrackPiece"
Cohesion: 0.18
Nodes (10): RoadCurve, RoadElementKind, RoadId, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 24 - ".Core"
Cohesion: 0.23
Nodes (13): Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, Edge, Node (+5 more)

### Community 25 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (20): CompiledRoadModel, Dictionary, HashSet, IList, List, MenuItem, RoadId, SortedDictionary (+12 more)

### Community 27 - ".Obstacles"
Cohesion: 0.22
Nodes (7): Vector3, ObstacleFact, InSweptPath, Bounds, CompiledRoadModel, RoadBoundsBox, Vector3

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.07
Nodes (29): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+21 more)

### Community 29 - "MainMenuProfileFlowController"
Cohesion: 0.10
Nodes (18): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, PlayerProfileBootstrapService (+10 more)

### Community 30 - "RoadId"
Cohesion: 0.08
Nodes (31): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+23 more)

### Community 31 - "CollisionFacts"
Cohesion: 0.09
Nodes (25): RoadId, CollisionAnalysis, CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, CollisionResponseRequest, Facts (+17 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (32): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+24 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (19): DevIndestructibleVehicle, Action, Collider, Collision, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody (+11 more)

### Community 35 - ".FullPath"
Cohesion: 0.15
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 36 - "ReferenceTrack"
Cohesion: 0.19
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.07
Nodes (12): Vector3, NetworkedVehicleSeatService, Instance, Func, NetworkVariable, Vector3, NetworkedVehicleState, CurrentDamageThresholdsCrossed (+4 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "V1ImportResult"
Cohesion: 0.14
Nodes (15): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+7 more)

### Community 40 - "PathHorizon"
Cohesion: 0.12
Nodes (14): HorizonEnd, ExitPortal, LookAheadLimit, PathHorizon, End, Intervals, Issue, IssueDistanceMeters (+6 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.16
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.07
Nodes (26): JunctionRecord, JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted (+18 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Measure"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+4 more)

### Community 45 - "SweepPose"
Cohesion: 0.17
Nodes (13): RoadModelValidationProfile, IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate, SweepPose (+5 more)

### Community 46 - "MeasurementRun"
Cohesion: 0.10
Nodes (21): IReadOnlyList, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label, MaxPopulation (+13 more)

### Community 47 - ".Monitor"
Cohesion: 0.21
Nodes (8): MotionCommand, DrivabilityProfile, BodyState, ComposedDrive, DriverProfile, List, SpeedPlan, TrafficDriveOutcome

### Community 48 - ".Evaluate"
Cohesion: 0.10
Nodes (21): MotionCommand, IsFinite, DrivabilityProfile, RoadId, VehicleProfile, AuthorizedContact, Valid, NearFieldSample (+13 more)

### Community 49 - "DefinitionId"
Cohesion: 0.11
Nodes (15): List, CharacterCatalog, Count, Color, GameObject, CharacterDef, DisplayName, Id (+7 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.09
Nodes (20): CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, List (+12 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReviewModel"
Cohesion: 0.17
Nodes (11): CompiledRoadModel, List, RoadBoundsBox, StringBuilder, PairReview, PairReviewModel, PairReviewStatus, Modified (+3 more)

### Community 54 - "V2StepRecord"
Cohesion: 0.08
Nodes (26): IReadOnlyList, RoadId, V2DriveRecord, V2InteractionRecord, V2JunctionTrace, V2StageTimings, TotalMillisecondsPerStep, V2StepRecord (+18 more)

### Community 55 - ".AddMovement"
Cohesion: 0.24
Nodes (3): JunctionFeature, AuthoringTask, ImportedJunction

### Community 56 - ".Step"
Cohesion: 0.22
Nodes (8): TrafficFrame, MotionCommand, HazardQueryReport, JunctionActorReport, JunctionBlockerCause, JunctionConflictIndex, JunctionSnapshot, TrafficHazardCollectorCounters

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.10
Nodes (18): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+10 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.17
Nodes (12): Dictionary, IReadOnlyList, KeyValuePair, List, Predicate, RoadCurve, RoadCurveSample, RoadRecordKind (+4 more)

### Community 59 - "SpatialEntry"
Cohesion: 0.21
Nodes (7): Bounds, SpatialEntry, SpatialQueryBuffer, Capacity, Count, Saturated, Total

### Community 60 - "PlanningDecision"
Cohesion: 0.13
Nodes (17): AgentObservation, ProfilerMarker, TrafficDecisionProjection, PlanningDecision, Motion, Observation, Path, PerceptionPath (+9 more)

### Community 61 - "TrackingTolerance"
Cohesion: 0.20
Nodes (10): TrackingToleranceResponse, Latched, LatchedAtStep, TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, TrackingTolerance (+2 more)

### Community 62 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 63 - "RightOfWayRelation"
Cohesion: 0.24
Nodes (8): Vector3, RightOfWay, RightOfWayRelation, Ambiguous, FromLeft, FromRight, Opposite, Same

### Community 64 - ".Decide"
Cohesion: 0.09
Nodes (29): DriverProfile, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind (+21 more)

### Community 65 - "VehicleTireModel"
Cohesion: 0.12
Nodes (12): TireSample, Vector2, TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce (+4 more)

### Community 66 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.05
Nodes (43): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+35 more)

### Community 68 - "IPathGeometry"
Cohesion: 0.23
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 69 - "AgentObservation"
Cohesion: 0.14
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 70 - ".Compute"
Cohesion: 0.32
Nodes (7): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, PairGeometryFingerprint

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.13
Nodes (18): IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant, IntentInterval (+10 more)

### Community 73 - "RightOfWayTable"
Cohesion: 0.25
Nodes (9): ConditionalWeakTable, IReadOnlyList, RoadId, Entry, RightOfWayTable, AmbiguousCount, CanonicalText, Entries (+1 more)

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.06
Nodes (41): TrafficHazardCollectorCounters, TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer, Bounds, CharacterController (+33 more)

### Community 75 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "LongitudinalPerception"
Cohesion: 0.18
Nodes (11): IReadOnlyList, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason, PerceptionUnavailableReason, ChannelSaturated (+3 more)

### Community 78 - "PlanningRequest"
Cohesion: 0.18
Nodes (11): IReadOnlyList, RoadId, RoutePlan, RouteSeed, TrackingTolerance, TrafficFrame, PlanningRequest, GateAEvidenceResult (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.25
Nodes (5): Surface, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "ModelDto"
Cohesion: 0.13
Nodes (14): AdjacencyDto, RoadLocalizationProfile, ModelDto, RoadLocalizationProfile, ConnectionDto, ControlDto, CorridorDto, JunctionDto (+6 more)

### Community 81 - "PlayerMode"
Cohesion: 0.22
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.10
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+9 more)

### Community 83 - "InterStepResult"
Cohesion: 0.25
Nodes (8): IReadOnlyList, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees

### Community 84 - "JunctionCoordinator"
Cohesion: 0.14
Nodes (21): CompiledRoadModel, Dictionary, HashSet, KeyValuePair, List, RoadId, Grant, JunctionCoordinator (+13 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.15
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 86 - ".Build"
Cohesion: 0.15
Nodes (14): Dictionary, JunctionRecord, RoadCurveSample, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord (+6 more)

### Community 87 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 88 - "LobbyFlowController"
Cohesion: 0.10
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.13
Nodes (23): JunctionMovement, JunctionControl, RoadModelSource, RoadRecordKind, Adjacency, ConflictZone, Connection, Control (+15 more)

### Community 90 - ".AccumulateStep"
Cohesion: 0.33
Nodes (3): V2ContactEpisode, Collision, Transform

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.14
Nodes (16): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+8 more)

### Community 93 - ".Measure"
Cohesion: 0.15
Nodes (15): BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, Scene, Vector3 (+7 more)

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
Cohesion: 0.11
Nodes (17): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+9 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.44
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 102 - ".Entry"
Cohesion: 0.31
Nodes (3): Vector3, HistoricalPairFingerprintRecord, HistoricalPairFingerprintTable

### Community 103 - ".Build"
Cohesion: 0.36
Nodes (4): CompiledRoadModel, IReadOnlyList, RoadCurve, PathInterval

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
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "NetworkedRunSessionMonitor"
Cohesion: 0.23
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - ".Crossings"
Cohesion: 0.36
Nodes (6): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection

### Community 112 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 113 - "SafetyReason"
Cohesion: 0.22
Nodes (9): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+1 more)

### Community 114 - "JunctionTraversal"
Cohesion: 0.24
Nodes (8): TraversalComparer, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds, IEqualityComparer

### Community 115 - ".ValidateApproaches"
Cohesion: 0.25
Nodes (7): Dictionary, JunctionControl, JunctionMovement, LaneCorridor, List, RoadModelSource, RoadModelValidationIssue

### Community 116 - "LongitudinalDecision"
Cohesion: 0.12
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
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "MatchSettings"
Cohesion: 0.29
Nodes (6): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, RoadRage.Features.Lobby

### Community 123 - ".PrepareStep"
Cohesion: 0.19
Nodes (5): StepContactAccumulator, ProfilerMarker, TrafficActorInput, VehicleDriveIntentComposer, VehicleFootprintPose

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.03
Nodes (66): AgentObservation, RoutePlan, TrackingTolerance, TrafficDecisionProjection, TrafficV2VehicleDriver, Blockers, ContactEpisodes, Contacts (+58 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.16
Nodes (6): IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

### Community 128 - ".Track"
Cohesion: 0.33
Nodes (4): DrivabilityProfile, DriverProfile, RoadCurve, Vector3

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.05
Nodes (31): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+23 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.08
Nodes (28): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+20 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (12): Difficulty, Difficulty, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, Difficulty (+4 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (68): CompiledConflictZone, Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile, ImportManifest (+60 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - "JunctionRecord"
Cohesion: 0.07
Nodes (27): JunctionControlKind, RoadId, JunctionApproach, Crossed, JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound (+19 more)

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
Cohesion: 0.08
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.08
Nodes (19): JunctionPriority, IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest (+11 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.13
Nodes (11): DrivabilityProfile, BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalPayload, RoadModelCanonicalWriter, Comparison (+3 more)

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - ".ToRecord"
Cohesion: 0.50
Nodes (3): FileLayout, FileRecord, FileRecord

### Community 148 - "ImportContext"
Cohesion: 0.14
Nodes (12): DrivabilityProfile, IList, RoadBoundsBox, RoadCurvePoint, RoadModelSource, RoadModelValidationProfile, Vector3, CircleFit (+4 more)

### Community 149 - "StopHoldPhase"
Cohesion: 0.40
Nodes (5): StopHoldPhase, Entered, Holding, None, Released

### Community 150 - "TacticalDecision"
Cohesion: 0.07
Nodes (32): RouteSeed, CollisionGoalPhase, AwaitingRecovery, Braking, None, Reacting, TacticalDecision, AcceptedAtFrame (+24 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.06
Nodes (23): Rpc, RpcParams, NetworkedPlayerLifecycleIntent, Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, Key (+15 more)

### Community 152 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 153 - "TrackingMeasurement.cs"
Cohesion: 0.29
Nodes (8): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound, TrackingMeasurement

### Community 154 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "PathSpan"
Cohesion: 0.40
Nodes (4): RoadElementKind, RoadId, PathSpan, CompiledRoadModel

### Community 157 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 158 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 159 - "NetworkedPlayerState"
Cohesion: 0.07
Nodes (26): IEnumerable, NetworkedPlayerLifecycleService, Instance, Dictionary, GameObject, HashSet, IEnumerator, NetworkManager (+18 more)

### Community 160 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 161 - "SafetyVerdict"
Cohesion: 0.40
Nodes (5): SafetyVerdict, Clamp, EmergencyStop, Pass, Reject

### Community 162 - ".Build"
Cohesion: 0.18
Nodes (11): JunctionKinematics, Known, ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId, Vector3 (+3 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 165 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (53): AppliedWidth, ConflictKind, Dictionary, FileLayout, Func, IEnumerable, IList, JunctionControlKind (+45 more)

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.05
Nodes (38): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+30 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - ".TryApproachEnd"
Cohesion: 0.50
Nodes (3): CompiledJunctionControl, CompiledRoadModel, RoadCurveSample

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (32): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+24 more)

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 177 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 178 - "JunctionSnapshot"
Cohesion: 0.09
Nodes (16): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale (+8 more)

### Community 183 - "LaneGraphRouting"
Cohesion: 0.24
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 186 - "PairReviewEntry"
Cohesion: 0.20
Nodes (8): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, PairReviewActions, PairReviewEntry

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (18): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+10 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.15
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.13
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
- **1241 isolated node(s):** `DeltaVMetersPerSecond`, `IsFinite`, `CollisionThresholds`, `None`, `RelativeSpeed` (+1236 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1785 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **25 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `TrafficV2Insertion`, `RageRoadEventFlowController`, `MeasurementRun`, `TrafficV2Code`, `TrafficSettingsDef`, `V2StepRecord`, `LaneGraph`, `TrafficV2VehicleDriver`, `NetworkedPlayerState`?**
  _High betweenness centrality (0.159) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `TrafficV2Insertion`, `RoadModelRecords.cs`, `.Localize`, `TrafficV2HazardCollector`, `RoadModelDocument`, `RoadGeometryValidator`, `.TryGetCorridor`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `RoadLineage`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.141) - this node is a cross-community bridge._
- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `TrafficV2HazardCollector`, `.Monitor`, `V2StepRecord`, `TacticalDecision`, `.Step`, `NetworkedVehicleSeatIntent`, `.AccumulateStep`, `.PrepareStep`, `PortalTrafficSpawner`, `CollisionFacts`?**
  _High betweenness centrality (0.126) - this node is a cross-community bridge._
- **What connects `DeltaVMetersPerSecond`, `IsFinite`, `CollisionThresholds` to the rest of the system?**
  _1241 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MotionPlan` be split into smaller, more focused modules?**
  _Cohesion score 0.12681159420289856 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10365853658536585 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Migration` be split into smaller, more focused modules?**
  _Cohesion score 0.12258064516129032 - nodes in this community are weakly interconnected._