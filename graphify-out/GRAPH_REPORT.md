# Graph Report - RRS  (2026-10-07)

## Corpus Check
- 180 files · ~246,478 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4717 nodes · 10692 edges · 224 communities (180 shown, 44 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 461 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a313d500`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedVehicleSeatService
- .Refine
- RoadRage.Features.Vehicles.Traffic.Planning
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
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- LobbyJoinOutcome
- LocalOnFootController
- Blocker
- RageTuningDef
- LobbyRosterService
- MigrationReport
- VehiclePhysicsBody
- JunctionRecord
- .Draw
- .Core
- NetworkedRageState
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1RoadModelImporter.cs
- DefinitionId
- RoadId
- AuthoringDecisions
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .FullPath
- AgentObservation
- NetworkedVehicleState
- VehicleProfile
- V1ImportResult
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- .Measure
- PairSweep
- TrafficV2Composition.cs
- MotionPlan
- .Evaluate
- PairReviewEntry
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- PairReviewModel
- .Step
- V1Node
- PerceivedObstacleKind
- TrafficDriveOutcome
- ImportedCurve
- NetworkedAIVehicleState
- CampaignTraceability
- PassengerActionDef
- AIVehicleBehaviorDebugView
- ObservationChannel
- .Decide
- .FixedUpdate
- ReferenceTrack
- TrafficV2Insertion
- IPathGeometry
- PassengerActionVerdictCode
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- GateAEvidenceResult
- TrafficV2HazardCollector
- TrafficJunctionOutcome
- UserNotice
- .GapSeconds
- .HandleRosterChanged
- .CheckVisuals
- ModelDto
- PlayerMode
- KinematicOffsetBounds
- InterStepResult
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- .FingerprintWithInputs
- LobbyFlowController
- RoadModelValidationIssue
- PairReview
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
- .PrepareStep
- RouteReason
- StopHoldRelease
- .Track
- .UpdateSteeringState
- RageRoadEventFlowController
- MainMenuScreen
- JunctionDistances
- PlayerProfileFileStore
- RoadGeometryValidator
- RoadCurveSample
- DriverProfileDef
- RoadRage.Features.Vehicles.Traffic.Migration
- JunctionTraversal
- RoadModelCompilationException
- LongitudinalDecision
- JunctionClearanceResult
- TrafficSettingsDef
- .Entry
- DriverProfile
- StatusFilter
- PlayerProfileStore
- .Monitor
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- .Manifest
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- MatchSettings
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- SafetyReason
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- LongitudinalArbitration.cs
- RoadModelCanonicalWriter
- SpeedConstraint
- PathHorizon.cs
- ImportContext
- StopHoldPhase
- RoadBoundsBox
- NetworkedVehicleSeatIntent
- RoadRage.Shared.Networking
- TrackingMeasurement.cs
- RoadModelVersion
- .Read
- TrafficDecisionProjection.cs
- TrafficV2Admission
- .Accumulate
- NetworkedPlayerLifecycleService
- PlanningDecision
- .MeasurePath
- .Build
- RoutePath
- PairDecisionState
- FileLayout
- NetworkedPassengerActionIntent
- ElementTrace
- CharacterController
- V2FallbackReason
- Collider
- NetworkedPlayerLifecycleIntent
- IEnumerator
- NetworkObject
- TrafficV2Code
- ConflictKind
- RoadBoundsBox
- TrafficV2Settings
- JunctionSnapshot
- KeyValuePair
- NetworkedPlayerReviveIntent
- CompiledJunctionControl
- RoadModelValidationIssue
- LaneGraphRouting
- JunctionRecord
- DriverProfile
- Quaternion
- RoadLocation
- PerceptionLimits
- NetworkedPlayerState
- RoadModelSource
- .NextEntryPortal
- RouteOutcome
- RoadRage.Shared.Domain
- PlayerNameValidator
- .TryApproachEnd
- LobbyCodeClipboard
- .ValidationProfile
- RoadElementKind
- BoxCollider
- NetworkedPlayerPresentation
- Collider
- Collision
- Dictionary
- DriverProfile
- DriverProfileDef
- JunctionControlKind
- JunctionSnapshot
- List
- Portal
- ProfilerMarker
- Rigidbody
- Stopwatch
- Transform
- VehicleCoverage
- VehicleFootprint
- LaneGraph
- TrafficPerception
- VehicleFootprintPose
- VehiclePhysicsBody
- RouteOutcome
- RouteReason
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 105 edges
2. `RunFlowController` - 99 edges
3. `RoadId` - 88 edges
4. `ConflictSweep` - 72 edges
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 63 edges
9. `NetworkedVehicleDriverController` - 58 edges
10. `RoadModelValidationCode` - 56 edges

## Surprising Connections (you probably didn't know these)
- `TrafficV2HazardCollector` --references--> `TrafficHazardCollectorCounters`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficV2VehicleDriver` --references--> `VehicleCoverage`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficV2VehicleDriver` --references--> `TrafficDecisionProjection`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `PlanningDecision` --references--> `TrafficDecisionProjection`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `V2StepRecord` --references--> `V2FallbackReason`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs

## Import Cycles
- None detected.

## Communities (224 total, 44 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.19
Nodes (3): Vector3, NetworkedVehicleSeatService, Instance

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.09
Nodes (16): TrafficV2Work, TrafficV2WorkCounters, TrackingToleranceResponse, Latched, LatchedAtStep, PlanningTolerances, ProfilerMarker, PlanningSpine (+8 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.17
Nodes (17): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, GridPath (+9 more)

### Community 5 - "RoutePlan"
Cohesion: 0.08
Nodes (30): CompiledRoadModel, IReadOnlyList, PlanningRequest, CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation, RoadModelVersion (+22 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - ".Localize"
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (16): TelemetrySample, Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip (+8 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 11 - "RunFlowController"
Cohesion: 0.06
Nodes (8): Camera, CharacterController, Collider, GameObject, HashSet, Transform, RunFlowController, ActiveLocalPlayer

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "RoadRage.Features.Online"
Cohesion: 0.08
Nodes (16): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+8 more)

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
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.05
Nodes (32): TMP_Text, RageStateDebugView, List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue (+24 more)

### Community 19 - "LobbyRosterService"
Cohesion: 0.07
Nodes (18): Difficulty, FacepunchSteamLobbyPlatform, Difficulty, ISteamLobbyPlatform, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby (+10 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.14
Nodes (12): Rigidbody, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile, Profile (+4 more)

### Community 22 - "JunctionRecord"
Cohesion: 0.05
Nodes (38): JunctionControlKind, JunctionBatchCounters, JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant (+30 more)

### Community 23 - ".Draw"
Cohesion: 0.36
Nodes (5): Color, IReadOnlyList, RoadCurveSample, RoadModelValidationProfile, SceneView

### Community 24 - ".Core"
Cohesion: 0.18
Nodes (16): RouteResult, CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId (+8 more)

### Community 25 - "NetworkedRageState"
Cohesion: 0.14
Nodes (10): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, Quaternion, Vector3, NetworkVariable (+2 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (20): CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, RoadModelValidationProfile, SortedDictionary (+12 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.09
Nodes (23): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+15 more)

### Community 29 - "DefinitionId"
Cohesion: 0.07
Nodes (27): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+19 more)

### Community 30 - "RoadId"
Cohesion: 0.09
Nodes (28): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledRoadModel, Adjacencies, ConflictZones, Connections (+20 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (47): AppliedWidth, ConflictKind, Dictionary, FileLayout, Func, IEnumerable, IList, JunctionControlKind (+39 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (32): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+24 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.06
Nodes (33): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+25 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - ".FullPath"
Cohesion: 0.20
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 36 - "AgentObservation"
Cohesion: 0.15
Nodes (18): Func, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AgentObservation, ExitOccupancyFact, IntentOverlapKind (+10 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.10
Nodes (9): Func, NetworkVariable, Vector3, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged (+1 more)

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "V1ImportResult"
Cohesion: 0.14
Nodes (15): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+7 more)

### Community 40 - "PathHorizon"
Cohesion: 0.12
Nodes (18): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, PathHorizon, End, Intervals (+10 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.14
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

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
Cohesion: 0.21
Nodes (6): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "TrafficV2Composition.cs"
Cohesion: 0.11
Nodes (22): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+14 more)

### Community 47 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 48 - ".Evaluate"
Cohesion: 0.09
Nodes (27): MotionCommand, PerceivedObstacleKind, RoadId, TrafficFrame, AuthorizedContact, Valid, NearFieldSample, SafetyFilter (+19 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (34): Bounds, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList (+26 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReviewModel"
Cohesion: 0.38
Nodes (5): CompiledRoadModel, List, RoadId, StringBuilder, PairReviewModel

### Community 54 - ".Step"
Cohesion: 0.13
Nodes (11): JunctionActorReport, TrafficFrame, V2StageTimings, TotalMillisecondsPerStep, DriverProfile, HazardQueryReport, JunctionBlockerCause, JunctionConflictIndex (+3 more)

### Community 55 - "V1Node"
Cohesion: 0.13
Nodes (17): Dictionary, JunctionFeature, Predicate, ImportedConnection, ImportedJunction, Quaternion, Transform, V1Edge (+9 more)

### Community 56 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (17): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+9 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.19
Nodes (10): List, RoadCurve, RoadCurveSample, ImportedCurve, ImportedSection, MovementRole, RoundaboutContinuation, RoundaboutEntry (+2 more)

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.28
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference

### Community 60 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 61 - "PassengerActionDef"
Cohesion: 0.18
Nodes (10): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+2 more)

### Community 62 - "AIVehicleBehaviorDebugView"
Cohesion: 0.19
Nodes (7): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, IRageDispositionSource, CurrentDisposition, TextMeshPro

### Community 63 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 64 - ".Decide"
Cohesion: 0.12
Nodes (19): DriverProfile, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind (+11 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.22
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 66 - "ReferenceTrack"
Cohesion: 0.11
Nodes (15): RoadCurve, RoadElementKind, RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters (+7 more)

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.12
Nodes (18): CompiledRoadModel, Portal, RoadId, Vector3, TrafficV2Insertion, Code, EntryPortal, ExitPortalId (+10 more)

### Community 68 - "IPathGeometry"
Cohesion: 0.21
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 70 - ".Compute"
Cohesion: 0.16
Nodes (10): BinaryWriter, IReadOnlyList, MenuItem, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, HistoricalPairFingerprintTable (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.08
Nodes (26): RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus, ExitPortalId (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.07
Nodes (29): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+21 more)

### Community 73 - "GateAEvidenceResult"
Cohesion: 0.15
Nodes (14): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+6 more)

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.07
Nodes (35): Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState, ProfilerMarker (+27 more)

### Community 75 - "TrafficJunctionOutcome"
Cohesion: 0.17
Nodes (11): JunctionActorReport, TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame (+3 more)

### Community 76 - "UserNotice"
Cohesion: 0.10
Nodes (13): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+5 more)

### Community 78 - ".HandleRosterChanged"
Cohesion: 0.15
Nodes (9): Difficulty, LobbyRosterEntry, DisplayName, PortraitTint, Ready, Difficulty, Easy, Hard (+1 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.25
Nodes (5): Surface, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "ModelDto"
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 81 - "PlayerMode"
Cohesion: 0.16
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.09
Nodes (20): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+12 more)

### Community 83 - "InterStepResult"
Cohesion: 0.18
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.12
Nodes (23): CompiledRoadModel, Dictionary, HashSet, IReadOnlyList, List, RoadId, Grant, JunctionCoordinator (+15 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.16
Nodes (12): Vector2, ConflictSweep, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState, MovementSide (+4 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (12): JunctionRecord, RoadCurveSample, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, MovementRecord (+4 more)

### Community 87 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 88 - "LobbyFlowController"
Cohesion: 0.11
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.23
Nodes (15): Dictionary, JunctionControl, JunctionMovement, LaneCorridor, List, RoadCurveSample, RoadId, RoadModelSource (+7 more)

### Community 90 - "PairReview"
Cohesion: 0.29
Nodes (3): DrivabilityProfile, IReadOnlyList, PairReview

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
Nodes (17): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Scene (+9 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.10
Nodes (20): CompiledRoadModel, ConditionalWeakTable, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal, RoadId (+12 more)

### Community 96 - "TrafficLongitudinalOutcome"
Cohesion: 0.12
Nodes (15): AgentObservation, Blocker, LongitudinalDecision, TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant (+7 more)

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (17): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+9 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - ".PrepareStep"
Cohesion: 0.22
Nodes (5): VehicleProfile, DrivabilityProfile, ProfilerMarker, TrafficActorInput, VehicleFootprintPose

### Community 102 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 103 - "StopHoldRelease"
Cohesion: 0.33
Nodes (6): StopHoldRelease, GapOpened, GrantEffective, None, SourceDeparted, SourceGone

### Community 104 - ".Track"
Cohesion: 0.22
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.07
Nodes (22): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+14 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "PlayerProfileFileStore"
Cohesion: 0.27
Nodes (5): PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.18
Nodes (10): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, RoadModelValidationProfile (+2 more)

### Community 111 - "RoadCurveSample"
Cohesion: 0.16
Nodes (11): CompiledJunctionMovement, Vector3, JunctionMovement, RoadCurveSample, RoadLineSegment, IReadOnlyList, List, Vector2 (+3 more)

### Community 112 - "DriverProfileDef"
Cohesion: 0.36
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 113 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.18
Nodes (6): PairReviewStatus, Modified, New, Removed, Unchanged, RoadRage.Features.Vehicles.Traffic.Migration

### Community 114 - "JunctionTraversal"
Cohesion: 0.16
Nodes (8): TraversalComparer, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds, IEqualityComparer

### Community 115 - "RoadModelCompilationException"
Cohesion: 0.29
Nodes (4): IReadOnlyList, RoadModelCompilationException, Issues, Exception

### Community 116 - "LongitudinalDecision"
Cohesion: 0.11
Nodes (15): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+7 more)

### Community 117 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.12
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - ".Entry"
Cohesion: 0.29
Nodes (3): Vector3, HistoricalPairFingerprintRecord, RoadBoundsBox

### Community 120 - "DriverProfile"
Cohesion: 0.08
Nodes (21): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+13 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "PlayerProfileStore"
Cohesion: 0.22
Nodes (5): PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection

### Community 123 - ".Monitor"
Cohesion: 0.09
Nodes (25): IReadOnlyList, MotionCommand, PerceivedObstacleKind, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord, V2JunctionTrace (+17 more)

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (56): AgentObservation, Blocker, LongitudinalDecision, TrafficV2VehicleDriver, Blockers, ContactEpisodes, Contacts, CurrentRoute (+48 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.17
Nodes (6): IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.08
Nodes (26): GameObject, IReadOnlyList, Vector3, PortalTrafficSpawner, Composition, CompositionFrozen, LivePopulation, LiveV2Population (+18 more)

### Community 128 - ".Manifest"
Cohesion: 0.28
Nodes (7): IList, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys, ImportManifestEntry

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.08
Nodes (29): SignalPhaseInput, CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId (+21 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.07
Nodes (6): Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, TMP_InputField

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.05
Nodes (61): DrivabilityProfile, ImportManifest, ImportManifestEntry, Junction, JunctionControl, JunctionControlKind, Priority, Signalized (+53 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.07
Nodes (35): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+27 more)

### Community 135 - "MatchSettings"
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 136 - "LobbyJoinService"
Cohesion: 0.14
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.19
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "SafetyReason"
Cohesion: 0.22
Nodes (9): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+1 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.09
Nodes (23): IReadOnlyList, RoadId, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest (+15 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "LongitudinalArbitration.cs"
Cohesion: 0.18
Nodes (13): IReadOnlyList, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason (+5 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - "PathHorizon.cs"
Cohesion: 0.13
Nodes (12): DriverProfile, HorizonEnd, ExitPortal, LookAheadLimit, PathIssue, CurvatureSlope, MissingElement, None (+4 more)

### Community 148 - "ImportContext"
Cohesion: 0.19
Nodes (8): DrivabilityProfile, RoadCurvePoint, Vector3, AuthoringTask, CircleFit, ImportContext, V1RoadModelImporter, CircleFit

### Community 149 - "StopHoldPhase"
Cohesion: 0.40
Nodes (5): StopHoldPhase, Entered, Holding, None, Released

### Community 150 - "RoadBoundsBox"
Cohesion: 0.29
Nodes (7): CompiledConflictZone, RoadBoundsBox, ConflictKind, Crossing, Merge, ConflictZone, RoadBoundsBox

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "RoadRage.Shared.Networking"
Cohesion: 0.10
Nodes (13): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel (+5 more)

### Community 153 - "TrackingMeasurement.cs"
Cohesion: 0.29
Nodes (8): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound, TrackingMeasurement

### Community 154 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 157 - "TrafficV2Admission"
Cohesion: 0.23
Nodes (9): GameObject, TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle, GateAEvidenceResult (+1 more)

### Community 158 - ".Accumulate"
Cohesion: 0.36
Nodes (3): V2ContactEpisode, Collision, Transform

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 160 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 162 - ".Build"
Cohesion: 0.16
Nodes (12): JunctionKinematics, Known, JunctionMovementPosition, ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId (+4 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - "PairDecisionState"
Cohesion: 0.29
Nodes (6): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed

### Community 165 - "FileLayout"
Cohesion: 0.33
Nodes (6): FileLayout, ConflictRecord, ControlRecord, DeferredRecord, DispositionRecord, WidthRecord

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.07
Nodes (25): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+17 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.08
Nodes (33): MotionCommand, VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun (+25 more)

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 174 - "TrafficV2Code"
Cohesion: 0.10
Nodes (20): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+12 more)

### Community 177 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, StopHoldParameters

### Community 180 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedPlayerState"
Cohesion: 0.17
Nodes (11): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState (+3 more)

### Community 190 - "RoadModelSource"
Cohesion: 0.11
Nodes (24): DrivabilityProfile, EffectiveLaneCorridor, RoadLocalizationProfile, RoadModelSource, JunctionMovement, RoadModelCanonicalPayload, Comparison, RoadModelCompiler (+16 more)

### Community 191 - ".NextEntryPortal"
Cohesion: 0.40
Nodes (3): CompiledRoadModel, List, RoadId

### Community 192 - "RouteOutcome"
Cohesion: 0.40
Nodes (5): RouteOutcome, InvalidInput, NoRoute, Planned, Replanned

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (19): DevVehicleSpawner, SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run (+11 more)

### Community 195 - ".TryApproachEnd"
Cohesion: 0.50
Nodes (3): CompiledRoadModel, RoadCurveSample, CompiledJunctionControl

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.21
Nodes (7): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.18
Nodes (12): LaneSide, AdjacentOccupantFact, Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance (+4 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1182 isolated node(s):** `FrameId`, `Observation`, `Decision`, `RoadLimits`, `Blockers` (+1177 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1730 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **44 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `.PrepareStep`, `GateAEvidenceParameters`, `TrafficDecisionProjection`, `V2FallbackReason`, `TrafficV2HazardCollector`, `.Evaluate`, `.Step`, `TrafficV2Admission`, `TrafficDriveOutcome`, `.Monitor`, `NetworkedPlayerState`, `.Accumulate`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.154) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `RoutePlan`, `SpeedPlan`, `.Localize`, `PathHorizon`, `RoadModelRecords.cs`, `TrafficV2HazardCollector`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadCurveSample`, `RoadModelCanonicalWriter`, `AuthoredRoadModel`, `Blocker`, `RoadBoundsBox`, `AutomatedPairDecisionPolicy`, `RoadLineage`, `RoadModelSource`?**
  _High betweenness centrality (0.154) - this node is a cross-community bridge._
- **Why does `RoadRage.Features.Vehicles` connect `RoadRage.Shared.Domain` to `VehicleWheel`, `NetworkedVehicleDriverController`, `RoadRage.Features.Vehicles.Traffic.Planning`, `VehicleArcadeAssist`, `.UpdateSteeringState`, `VehicleProfileDef`, `VehicleSuspensionModel`, `.Read`, `LaneNode`, `LaneGraphRouting`, `DriverProfile`, `NetworkedAIVehicleState`, `RoadRage.Shared.Networking`, `AIVehicleBehaviorDebugView`?**
  _High betweenness centrality (0.129) - this node is a cross-community bridge._
- **What connects `FrameId`, `Observation`, `Decision` to the rest of the system?**
  _1182 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10668563300142248 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.0946969696969697 - nodes in this community are weakly interconnected._
- **Should `RoutePlan` be split into smaller, more focused modules?**
  _Cohesion score 0.07765151515151515 - nodes in this community are weakly interconnected._