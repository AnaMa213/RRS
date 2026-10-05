# Graph Report - RRS  (2026-10-05)

## Corpus Check
- 174 files · ~233,230 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4402 nodes · 10211 edges · 178 communities (172 shown, 6 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 445 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `90d7a06a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .Refine
- DefinitionId
- TrafficV2Composition.cs
- GreyboxAssetSeedMetadata
- SweepElement
- MainMenuScreen
- GateAEvidenceParameters
- CompiledRoadModel
- TireSample
- VehicleSuspensionModel
- PairReviewModel
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- .TryGetMovement
- LocalOnFootController
- .Core
- NpcReactionEffect
- .CheckVisuals
- MigrationReport
- VehiclePhysicsBody
- PairReviewEntry
- AgentObservation
- AuthoredRoadModel
- JunctionClearance
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1RoadModelImporter.cs
- JunctionSnapshot
- .Measure
- AuthoringDecisions
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- PerceivedObstacleKind
- RoutePlan
- NetworkedVehicleSeatService
- VehicleProfile
- V1ImportResult
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- .Manifest
- ConflictSweep
- IPathGeometry
- MotionPlan
- JunctionActorReport
- HistoricalMovementReader
- .Decide
- TrafficDriveOutcome
- .Regenerate
- VehicleDriveIntent
- ObservationChannel
- List
- LongitudinalArbitration.cs
- .Draw
- ImportedCurve
- SweepPose
- .FullPath
- NetworkedRageState
- .FixedUpdate
- TrafficV2HazardCollector
- NetworkedAIVehicleState
- NetworkedRunSessionMonitor
- .Collect
- LobbyRosterService
- TrafficLongitudinalOutcome
- InterStepResult
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- RageTuningDef
- .Assemble
- CharacterCatalog
- UserNotice
- .FingerprintWithInputs
- MainMenuFlowController
- PassengerActionVerdictCode
- PairReview
- RunEscapeMenuScreen
- KinematicOffsetBounds
- TrafficJunctionOutcome
- JunctionCoordinator
- JunctionClearanceResult
- PlayerMode
- StopHoldState
- LobbyFlowController
- RoadModelValidationIssue
- LongitudinalCandidateKind
- .Track
- RoadLineage
- PlayerProfile
- V1Node
- JunctionConflictIndex
- TrackingTolerance
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- RoadRage.Features.Vehicles.Traffic.Migration
- .Create
- SpatialQueryBuffer
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- HistoricalPairFingerprintRecord
- RoadGeometryValidator
- NetworkedBossState.cs
- NetworkedVehicleState
- HazardRootClass
- MatchSettings
- JunctionExitBound
- .Run
- NetworkedCrewEconomyState.cs
- TrafficSettingsDef
- TrafficHazardKind
- DriverProfile
- LeafState
- StopHoldRelease
- RoadRage.Features.Vehicles.Traffic.Planning
- LongitudinalCandidate
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- MonoBehaviour
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadId
- SpeedPlan
- ReferenceCoverage
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- SpeedPlanIssue
- .MeasurePath
- RoadModelDocument
- LaneNode
- RoadModelCanonicalWriter
- SpeedConstraint
- ImportContext
- .Step
- NetworkedVehicleSeatIntent
- ReferenceTrack
- TrackPiece
- .Read
- LongitudinalDecision
- TrafficV2Insertion
- NetworkedPlayerLifecycleService
- RoutePath
- NetworkedPassengerActionIntent
- CampaignTraceability
- V2FallbackReason
- NetworkedVehicleState.cs
- NetworkedPlayerLifecycleIntent
- LobbyPlayerSlotView
- TrafficV2Code
- GateAEvidenceResult
- StatusFilter
- NetworkedPlayerReviveIntent
- LaneGraphRouting
- NetworkedPlayerState
- ModelDto
- PairRelation
- RoadRage.Shared.Domain
- VehicleCoverage
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- LaneGraph
- TrafficPerception
- DriverProfileDef
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 107 edges
2. `RoadId` - 102 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 68 edges
5. `NetworkedVehicleState` - 67 edges
6. `ImportContext` - 67 edges
7. `CompiledRoadModel` - 63 edges
8. `AuthoredRoadModel` - 63 edges
9. `TrafficFrame` - 59 edges
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

## Communities (178 total, 6 thin omitted)

### Community 0 - ".Refine"
Cohesion: 0.12
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 1 - "DefinitionId"
Cohesion: 0.14
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 2 - "TrafficV2Composition.cs"
Cohesion: 0.09
Nodes (26): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+18 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.18
Nodes (13): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+5 more)

### Community 5 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (17): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+9 more)

### Community 7 - "CompiledRoadModel"
Cohesion: 0.07
Nodes (30): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+22 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (16): TelemetrySample, Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip (+8 more)

### Community 10 - "PairReviewModel"
Cohesion: 0.23
Nodes (10): CompiledRoadModel, List, RoadId, StringBuilder, PairReviewModel, PairReviewStatus, Modified, New (+2 more)

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.13
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "RoadRage.Features.Online"
Cohesion: 0.08
Nodes (16): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+8 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.17
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - ".TryGetMovement"
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - ".Core"
Cohesion: 0.19
Nodes (16): RouteResult, CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId (+8 more)

### Community 18 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 19 - ".CheckVisuals"
Cohesion: 0.24
Nodes (6): Surface, HashSet, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.14
Nodes (12): Rigidbody, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile, Profile (+4 more)

### Community 23 - "AgentObservation"
Cohesion: 0.14
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (34): Bounds, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, IReadOnlyList (+26 more)

### Community 25 - "JunctionClearance"
Cohesion: 0.30
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.10
Nodes (20): CompiledRoadModel, Dictionary, HashSet, IList, List, MenuItem, RoadId, SortedDictionary (+12 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.09
Nodes (23): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+15 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.12
Nodes (14): JunctionBatchCounters, JunctionGrantStatus, Denied, Granted, Held, Released, Revoked, JunctionRecord (+6 more)

### Community 30 - ".Measure"
Cohesion: 0.17
Nodes (13): BoxCollider, Collider, CompiledRoadModel, GameObject, List, Scene, Vector3, VehiclePhysicsBody (+5 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (46): ConflictKind, FileLayout, Func, IList, JunctionControlKind, List, RoadBoundsBox, RoadCurveSample (+38 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (31): CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample, RoadElementKind (+23 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.11
Nodes (20): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+12 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.08
Nodes (13): DevIndestructibleVehicle, Action, Collider, Collision, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody (+5 more)

### Community 35 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

### Community 36 - "RoutePlan"
Cohesion: 0.05
Nodes (41): CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation, RoadModelVersion, DecisionCounter, RouteDiagnostic, None (+33 more)

### Community 37 - "NetworkedVehicleSeatService"
Cohesion: 0.15
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "V1ImportResult"
Cohesion: 0.12
Nodes (17): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId (+9 more)

### Community 40 - "PathHorizon"
Cohesion: 0.08
Nodes (31): CompiledRoadModel, DriverProfile, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, HorizonEnd, ExitPortal (+23 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.14
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Manifest"
Cohesion: 0.28
Nodes (7): IList, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys, ImportManifestEntry

### Community 45 - "ConflictSweep"
Cohesion: 0.15
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 46 - "IPathGeometry"
Cohesion: 0.12
Nodes (13): Vector3, Vector3, IPathGeometry, LengthMeters, Spans, PlanningDecision, Motion, Observation (+5 more)

### Community 47 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.07
Nodes (33): IReadOnlyList, RoadId, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest (+25 more)

### Community 49 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (12): JunctionRecord, RoadCurveSample, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, MovementRecord (+4 more)

### Community 50 - ".Decide"
Cohesion: 0.27
Nodes (4): DriverProfile, List, LongitudinalArbitration, StopHoldParameters

### Community 51 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (16): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+8 more)

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "VehicleDriveIntent"
Cohesion: 0.24
Nodes (6): VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 54 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 55 - "List"
Cohesion: 0.20
Nodes (6): Dictionary, JunctionFeature, List, Predicate, ImportedJunction, ImportedSection

### Community 56 - "LongitudinalArbitration.cs"
Cohesion: 0.17
Nodes (13): IReadOnlyList, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason (+5 more)

### Community 57 - ".Draw"
Cohesion: 0.24
Nodes (6): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 58 - "ImportedCurve"
Cohesion: 0.16
Nodes (12): RoadCurve, RoadCurveSample, ImportedConnection, ImportedCurve, MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit (+4 more)

### Community 59 - "SweepPose"
Cohesion: 0.19
Nodes (10): RoadModelValidationProfile, IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate, SweepPose (+2 more)

### Community 60 - ".FullPath"
Cohesion: 0.20
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 61 - "NetworkedRageState"
Cohesion: 0.13
Nodes (8): NetworkVariable, NetworkedRageState, CurrentDisposition, Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 62 - ".FixedUpdate"
Cohesion: 0.22
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 63 - "TrafficV2HazardCollector"
Cohesion: 0.11
Nodes (17): TrafficHazardCollectorCounters, Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity (+9 more)

### Community 64 - "NetworkedAIVehicleState"
Cohesion: 0.41
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 65 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 66 - ".Collect"
Cohesion: 0.19
Nodes (12): Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3, VehiclePhysicsBody (+4 more)

### Community 67 - "LobbyRosterService"
Cohesion: 0.04
Nodes (39): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+31 more)

### Community 68 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 69 - "InterStepResult"
Cohesion: 0.17
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 70 - ".Compute"
Cohesion: 0.16
Nodes (9): Dictionary, BinaryWriter, IReadOnlyList, RoadCurveSample, RoadId, RoadModelValidationProfile, HistoricalPairFingerprintTable, PairGeometryFingerprint (+1 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.12
Nodes (20): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+12 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 74 - ".Assemble"
Cohesion: 0.24
Nodes (5): DrivabilityProfile, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter

### Community 75 - "CharacterCatalog"
Cohesion: 0.21
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - ".FingerprintWithInputs"
Cohesion: 0.23
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 79 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 81 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (19): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+11 more)

### Community 83 - "TrafficJunctionOutcome"
Cohesion: 0.18
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 84 - "JunctionCoordinator"
Cohesion: 0.15
Nodes (20): CompiledRoadModel, Dictionary, IReadOnlyList, JunctionRecord, JunctionSnapshot, List, RoadId, Grant (+12 more)

### Community 85 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 86 - "PlayerMode"
Cohesion: 0.22
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 87 - "StopHoldState"
Cohesion: 0.18
Nodes (7): StopHoldPhase, Entered, Holding, None, Released, StopHoldState, Active

### Community 88 - "LobbyFlowController"
Cohesion: 0.08
Nodes (13): Difficulty, HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, Difficulty (+5 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.14
Nodes (20): RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor, Movement, Portal (+12 more)

### Community 90 - "LongitudinalCandidateKind"
Cohesion: 0.18
Nodes (11): LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable, Profile, SteeringCeilingUnreachable (+3 more)

### Community 91 - ".Track"
Cohesion: 0.22
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "PlayerProfile"
Cohesion: 0.11
Nodes (16): PersistentPlayerProfileRecord, PlayerNameValidator, PlayerProfile, CharacterId, DisplayName, PlayerProfileBootstrapService, PlayerProfileResolution, Error (+8 more)

### Community 94 - "V1Node"
Cohesion: 0.10
Nodes (27): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+19 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.11
Nodes (20): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, Portal, RoadBoundsBox, RoadId, Vector3 (+12 more)

### Community 96 - "TrackingTolerance"
Cohesion: 0.36
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.06
Nodes (35): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+27 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.30
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.20
Nodes (7): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, RoadRage.Features.Vehicles.Traffic.Migration

### Community 102 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 103 - "SpatialQueryBuffer"
Cohesion: 0.28
Nodes (6): Bounds, SpatialQueryBuffer, Capacity, Count, Saturated, Total

### Community 104 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MenuCharacterPreview"
Cohesion: 0.18
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "HistoricalPairFingerprintRecord"
Cohesion: 0.36
Nodes (3): RoadBoundsBox, Vector3, HistoricalPairFingerprintRecord

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.18
Nodes (11): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, RoadCurveSample (+3 more)

### Community 111 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 112 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 113 - "HazardRootClass"
Cohesion: 0.25
Nodes (7): HazardRootClass, Obstacle, Self, Static, TrafficV2Vehicle, Vehicle, WalkingPlayer

### Community 114 - "MatchSettings"
Cohesion: 0.29
Nodes (6): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, RoadRage.Features.Lobby

### Community 115 - "JunctionExitBound"
Cohesion: 0.29
Nodes (7): JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations, RouteEnd

### Community 116 - ".Run"
Cohesion: 0.48
Nodes (4): CompiledRoadModel, IEnumerable, JunctionSnapshot, TrafficV2StepCost

### Community 117 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "TrafficHazardKind"
Cohesion: 0.33
Nodes (5): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer

### Community 120 - "DriverProfile"
Cohesion: 0.08
Nodes (21): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+13 more)

### Community 121 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 122 - "StopHoldRelease"
Cohesion: 0.33
Nodes (6): StopHoldRelease, GapOpened, GrantEffective, None, SourceDeparted, SourceGone

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.15
Nodes (11): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing (+3 more)

### Community 124 - "LongitudinalCandidate"
Cohesion: 0.50
Nodes (4): RoadId, JunctionEntryInput, Active, LongitudinalCandidate

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.15
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.05
Nodes (31): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+23 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.12
Nodes (17): HashSet, List, ProfilerMarker, RoadId, Stopwatch, TrafficV2StepRunner, Collector, Coordinator (+9 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.07
Nodes (6): Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, TMP_InputField

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.16
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadId"
Cohesion: 0.04
Nodes (80): CompiledConflictZone, EffectiveLaneCorridor, Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile (+72 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - "ReferenceCoverage"
Cohesion: 0.40
Nodes (5): ReferenceCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (46): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference (+38 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 141 - "RoadModelDocument"
Cohesion: 0.08
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.17
Nodes (8): JunctionMovement, BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalPayload, RoadModelCanonicalWriter, RoadRage.Features.Vehicles.Traffic

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 148 - "ImportContext"
Cohesion: 0.22
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, AuthoringTask, CircleFit, ImportContext

### Community 149 - ".Step"
Cohesion: 0.10
Nodes (15): DriverProfile, IReadOnlyList, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord (+7 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 153 - "ReferenceTrack"
Cohesion: 0.14
Nodes (14): Quaternion, RoadKinematicAnchor, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound (+6 more)

### Community 154 - "TrackPiece"
Cohesion: 0.16
Nodes (10): RoadCurve, RoadElementKind, RoadId, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.10
Nodes (23): CompiledRoadModel, DriverProfile, Portal, Quaternion, RoadId, RoadLocation, Vector3, TrafficV2Insertion (+15 more)

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.06
Nodes (35): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+27 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.09
Nodes (21): Dictionary, HashSet, IReadOnlyDictionary, CampaignTraceability, Elements, ElementStatus, Measured, NotMeasured (+13 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (32): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+24 more)

### Community 170 - "NetworkedVehicleState.cs"
Cohesion: 0.21
Nodes (6): VehicleDamageType, Brake, Engine, Wheel, IHostOwnedRuntimeState, IsHostAuthority

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 172 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 174 - "TrafficV2Code"
Cohesion: 0.08
Nodes (26): GameObject, TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Code, Allowed (+18 more)

### Community 178 - "GateAEvidenceResult"
Cohesion: 0.15
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

### Community 189 - "NetworkedPlayerState"
Cohesion: 0.17
Nodes (11): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState (+3 more)

### Community 190 - "ModelDto"
Cohesion: 0.13
Nodes (15): AdjacencyDto, DrivabilityProfile, Comparison, RoadModelCompiler, ModelDto, ConnectionDto, ControlDto, CorridorDto (+7 more)

### Community 191 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.06
Nodes (18): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+10 more)

### Community 194 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.20
Nodes (10): Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance, PerceptionLimits, TrafficPerception (+2 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.27
Nodes (5): DriverProfileDef, Id, Profile, RawId, ScriptableObject

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (14): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+6 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1127 isolated node(s):** `RoadRage.App`, `Instance`, `Router`, `Notices`, `Profiles` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1583 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **6 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `.Step`, `AgentObservation`, `ReferenceTrack`, `LongitudinalDecision`, `TrafficV2Insertion`, `RoutePlan`, `V2FallbackReason`, `TrafficV2Code`, `JunctionActorReport`, `NetworkedPlayerState`, `.Collect`, `InterStepResult`, `TrafficDecisionProjection`, `LongitudinalCandidateKind`, `TrackingTolerance`, `GateAReviewWindow`, `SpatialQueryBuffer`, `.Run`, `RoadRage.Features.Vehicles.Traffic.Planning`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.250) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `MonoBehaviour`, `RoadRage.Shared.Domain`, `TrafficV2Composition.cs`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `TrafficV2Code`, `.Step`, `TrafficSettingsDef`, `LaneGraph`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.169) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `GateAReviewWindow`, `CompiledRoadModel`, `RoadModelDocument`, `RoadGeometryValidator`, `.TryGetMovement`, `RoadLineage`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `PortalTrafficSpawner`, `TrafficV2Insertion`, `TrafficV2HazardCollector`?**
  _High betweenness centrality (0.116) - this node is a cross-community bridge._
- **What connects `RoadRage.App`, `Instance`, `Router` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.11942959001782531 - nodes in this community are weakly interconnected._
- **Should `DefinitionId` be split into smaller, more focused modules?**
  _Cohesion score 0.13725490196078433 - nodes in this community are weakly interconnected._
- **Should `TrafficV2Composition.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.09359605911330049 - nodes in this community are weakly interconnected._