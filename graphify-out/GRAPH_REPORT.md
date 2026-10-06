# Graph Report - RRS  (2026-10-06)

## Corpus Check
- 174 files · ~234,483 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4410 nodes · 10244 edges · 184 communities (177 shown, 6 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 445 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `59bb0fb1`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedVehicleSeatService
- .Refine
- MeasurementRun
- GreyboxAssetSeedMetadata
- SweepElement
- .Core
- GateAEvidenceParameters
- RoadId
- TireSample
- VehicleSuspensionModel
- MonoBehaviour
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- .Localize
- LocalOnFootController
- Blocker
- RageTuningDef
- JunctionTraversal
- MigrationReport
- VehiclePhysicsBody
- .Add
- AgentObservation
- AuthoredRoadModel
- .Measure
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1RoadModelImporter.cs
- JunctionSnapshot
- V1Node
- AuthoringDecisions
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- NpcReactionEffect
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
- .Sha256Hex
- .Add
- CharacterCatalog
- DefinitionId
- ImportedCurve
- NetworkedAIVehicleState
- .FullPath
- PairReviewEntry
- .FixedUpdate
- TrafficV2HazardCollector
- PlayerMode
- PlanningDecision
- TrackPiece
- LobbyRosterService
- NetworkedRunSessionMonitor
- PassengerActionVerdictCode
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- PerceptionStatus
- V1ImportResult
- SweepPose
- UserNotice
- VehicleCoverage
- TrafficLongitudinalOutcome
- PassengerActionDef
- CampaignTraceability
- StopHoldRelease
- KinematicOffsetBounds
- LocalVoidRespawnController
- JunctionCoordinator
- .Screen
- HistoricalMovementReader
- StopHoldPhase
- NetworkPlayerRegistry
- RoadModelValidationIssue
- ReferenceTrack
- .Draw
- RoadLineage
- PlayerProfile
- V1SourceSet
- JunctionConflictIndex
- TrackingTolerance
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- .Track
- .Create
- PerceivedObstacleKind
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- AIVehicleBehaviorDebugView
- RoadGeometryValidator
- NetworkedBossState.cs
- RouteReason
- RoadModelVersion
- MatchSettings
- JunctionExitBound
- TrafficJunctionOutcome
- NetworkedCrewEconomyState.cs
- TrafficSettingsDef
- RoadRage.Features.Vehicles.Traffic.Migration
- DriverProfile
- .Configure
- .Evaluate
- RoadRage.Features.Vehicles.Traffic.Planning
- MainMenuFlowController
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- TrackingMeasurement.cs
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyFlowController
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- .FromRoute
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- FileLayout
- LeafState
- RoadModelDocument
- PairDecisionState
- LaneNode
- ConflictFilter
- RoadModelCanonicalWriter
- SpeedConstraint
- RunEscapeMenuScreen
- ImportContext
- V2StageTimings
- PlayerNameValidator
- NetworkedVehicleSeatIntent
- .InterStepBound
- IHostOwnedRuntimeState
- .Read
- LongitudinalDecision
- TrafficV2Insertion
- PathIssue
- NetworkedPlayerLifecycleService
- RoutePath
- NetworkedPassengerActionIntent
- ElementTrace
- SpeedPlanIssue
- V2FallbackReason
- NetworkedVehicleState.cs
- NetworkedPlayerState
- TrafficV2Code
- GateAEvidenceResult
- NetworkedPlayerReviveIntent
- LaneGraphRouting
- .DiffHash
- NetworkedVehicleRecoveryIntent
- ModelDto
- PairRelation
- .Classify
- RoadRage.Shared.Domain
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
4. `ConflictSweep` - 72 edges
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

## Communities (184 total, 6 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.15
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 1 - ".Refine"
Cohesion: 0.10
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 2 - "MeasurementRun"
Cohesion: 0.10
Nodes (21): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+13 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.18
Nodes (13): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+5 more)

### Community 5 - ".Core"
Cohesion: 0.18
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - "RoadId"
Cohesion: 0.08
Nodes (31): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+23 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.16
Nodes (11): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkedPlayerSpawnService, Transform, RunCompositionRoot (+3 more)

### Community 11 - "RunFlowController"
Cohesion: 0.06
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "RoadRage.Features.Online"
Cohesion: 0.08
Nodes (16): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+8 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - ".Localize"
Cohesion: 0.12
Nodes (22): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+14 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.11
Nodes (18): Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization, BlockerSource (+10 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.09
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 19 - "JunctionTraversal"
Cohesion: 0.17
Nodes (7): JunctionText, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - ".Add"
Cohesion: 0.15
Nodes (16): Bounds, CompiledJunctionMovement, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation (+8 more)

### Community 23 - "AgentObservation"
Cohesion: 0.14
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.10
Nodes (15): CompiledRoadModel, ConflictZone, Dictionary, HashSet, List, RoadCurveSample, RoadModelSource, SortedDictionary (+7 more)

### Community 25 - ".Measure"
Cohesion: 0.07
Nodes (37): Surface, BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, IEnumerable, IReadOnlyList (+29 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.15
Nodes (9): CompiledRoadModel, Dictionary, AutomatedPairDecisionManifest, AutomatedPairDecisionPlan, AutomatedPairDecisionPolicy, AutomatedPairDecisionRecord, RefinedPair, ClassificationSummary (+1 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.11
Nodes (16): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedPortal, MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit (+8 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.11
Nodes (15): JunctionSnapshot, JunctionBatchCounters, JunctionGrantStatus, Denied, Granted, Held, Released, Revoked (+7 more)

### Community 30 - "V1Node"
Cohesion: 0.14
Nodes (13): ImportedConnection, Quaternion, Transform, V1Edge, Key, V1Module, IsJunction, V1Node (+5 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.07
Nodes (34): ConflictKind, FileLayout, Func, IList, JunctionControlKind, List, RoadBoundsBox, RoadCurveSample (+26 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.06
Nodes (37): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+29 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - "NpcReactionEffect"
Cohesion: 0.11
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 36 - "RoutePlan"
Cohesion: 0.12
Nodes (17): IReadOnlyList, RoadModelVersion, RoutePlan, Diagnostics, DistanceMeters, ExitPortalId, ModelId, ModelVersion (+9 more)

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
Cohesion: 0.10
Nodes (22): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, HorizonEnd, ExitPortal, LookAheadLimit (+14 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.14
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (13): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+5 more)

### Community 44 - "LongitudinalArbitration.cs"
Cohesion: 0.18
Nodes (13): IReadOnlyList, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason (+5 more)

### Community 45 - "ConflictSweep"
Cohesion: 0.20
Nodes (8): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, ConflictSweep, PairSweep, IsCandidate, PoseFrame

### Community 46 - "IPathGeometry"
Cohesion: 0.23
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 47 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.09
Nodes (26): IReadOnlyList, RoadId, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest (+18 more)

### Community 49 - "PairReviewModel"
Cohesion: 0.20
Nodes (6): CompiledRoadModel, List, RoadBoundsBox, StringBuilder, PairReview, PairReviewModel

### Community 50 - ".Decide"
Cohesion: 0.12
Nodes (21): DriverProfile, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind (+13 more)

### Community 51 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (16): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+8 more)

### Community 52 - ".Regenerate"
Cohesion: 0.12
Nodes (18): IList, SignoffLayout, CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry (+10 more)

### Community 53 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 54 - ".Sha256Hex"
Cohesion: 0.19
Nodes (4): Dictionary, MenuItem, HistoricalPairFingerprintTable, Dictionary

### Community 55 - ".Add"
Cohesion: 0.14
Nodes (14): JunctionFeature, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint, Movement (+6 more)

### Community 56 - "CharacterCatalog"
Cohesion: 0.19
Nodes (7): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PlayerProfileBootstrapService

### Community 57 - "DefinitionId"
Cohesion: 0.10
Nodes (14): NetworkObject, Transform, Vector3, Color, GameObject, CharacterDef, DisplayName, Id (+6 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.19
Nodes (12): Dictionary, IReadOnlyList, KeyValuePair, List, Predicate, RoadCurve, RoadCurveSample, RoadRecordKind (+4 more)

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.29
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference

### Community 60 - ".FullPath"
Cohesion: 0.20
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 61 - "PairReviewEntry"
Cohesion: 0.23
Nodes (7): PairReviewActions, PairReviewEntry, PairReviewStatus, Modified, New, Removed, Unchanged

### Community 62 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 63 - "TrafficV2HazardCollector"
Cohesion: 0.07
Nodes (36): TrafficHazardCollectorCounters, Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState (+28 more)

### Community 64 - "PlayerMode"
Cohesion: 0.15
Nodes (8): CharacterController, PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 65 - "PlanningDecision"
Cohesion: 0.12
Nodes (17): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile (+9 more)

### Community 66 - "TrackPiece"
Cohesion: 0.18
Nodes (10): RoadCurve, RoadElementKind, NominalPose, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 67 - "LobbyRosterService"
Cohesion: 0.04
Nodes (39): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+31 more)

### Community 68 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 70 - ".Compute"
Cohesion: 0.33
Nodes (7): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, PairGeometryFingerprint

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.08
Nodes (26): Bounds, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant, IntentInterval (+18 more)

### Community 73 - "PerceptionStatus"
Cohesion: 0.33
Nodes (5): PerceptionStatus, AgentOccupancyUnavailable, Evaluated, NoMovementAhead, UndeclaredFootprint

### Community 74 - "V1ImportResult"
Cohesion: 0.15
Nodes (10): DrivabilityProfile, IList, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1ImportResult, Succeeded (+2 more)

### Community 75 - "SweepPose"
Cohesion: 0.18
Nodes (10): Vector2, GridPath, PoseFrame, SweepPose, Plan, IReadOnlyList, Vector2, KinematicPoseSet (+2 more)

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 78 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - "PassengerActionDef"
Cohesion: 0.16
Nodes (12): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+4 more)

### Community 80 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 81 - "StopHoldRelease"
Cohesion: 0.33
Nodes (6): StopHoldRelease, GapOpened, GrantEffective, None, SourceDeparted, SourceGone

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+9 more)

### Community 83 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 84 - "JunctionCoordinator"
Cohesion: 0.18
Nodes (17): CompiledRoadModel, Dictionary, List, RoadId, Grant, JunctionCoordinator, Batches, Current (+9 more)

### Community 85 - ".Screen"
Cohesion: 0.22
Nodes (8): RefineNode, RefineSegment, GridPath, LeafState, MovementSide, RefineContext, RefineNode, RefineSegment

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (13): JunctionRecord, RoadCurveSample, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord (+5 more)

### Community 87 - "StopHoldPhase"
Cohesion: 0.40
Nodes (5): StopHoldPhase, Entered, Holding, None, Released

### Community 88 - "NetworkPlayerRegistry"
Cohesion: 0.13
Nodes (8): NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (18): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+10 more)

### Community 90 - "ReferenceTrack"
Cohesion: 0.21
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 91 - ".Draw"
Cohesion: 0.27
Nodes (6): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "PlayerProfile"
Cohesion: 0.14
Nodes (14): PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, PlayerProfileResolution, Error, IsResolved, Profile (+6 more)

### Community 94 - "V1SourceSet"
Cohesion: 0.13
Nodes (17): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Scene (+9 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.11
Nodes (20): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, Portal, RoadBoundsBox, RoadId, Vector3 (+12 more)

### Community 96 - "TrackingTolerance"
Cohesion: 0.19
Nodes (10): TrackingToleranceResponse, Latched, LatchedAtStep, TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, TrackingTolerance (+2 more)

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.15
Nodes (11): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, GateAReviewWindow (+3 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.30
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - ".Track"
Cohesion: 0.22
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 102 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 103 - "PerceivedObstacleKind"
Cohesion: 0.24
Nodes (8): RoadId, BlockerRules, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle, WalkingPlayer

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

### Community 109 - "AIVehicleBehaviorDebugView"
Cohesion: 0.21
Nodes (7): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, IRageDispositionSource, CurrentDisposition, TextMeshPro

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 112 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 113 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 114 - "MatchSettings"
Cohesion: 0.29
Nodes (6): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, RoadRage.Features.Lobby

### Community 115 - "JunctionExitBound"
Cohesion: 0.20
Nodes (9): IReadOnlyList, JunctionRecord, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations (+1 more)

### Community 116 - "TrafficJunctionOutcome"
Cohesion: 0.18
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 117 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.22
Nodes (3): Vector3, HistoricalPairFingerprintRecord, RoadRage.Features.Vehicles.Traffic.Migration

### Community 120 - "DriverProfile"
Cohesion: 0.08
Nodes (21): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+13 more)

### Community 121 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 122 - ".Evaluate"
Cohesion: 0.15
Nodes (13): DriverProfile, PlanningReach, IReadOnlyList, ProfilerMarker, PlanningRequest, PlanningSpine, CompiledRoadModel, RoadId (+5 more)

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.16
Nodes (10): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing (+2 more)

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (55): BoxCollider, Collider, Collision, Dictionary, DriverProfile, DriverProfileDef, IReadOnlyList, List (+47 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.10
Nodes (14): IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter, Inchangees, Modifiees (+6 more)

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "TrackingMeasurement.cs"
Cohesion: 0.25
Nodes (8): InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees, PieceBound

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (21): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+13 more)

### Community 131 - "LobbyFlowController"
Cohesion: 0.04
Nodes (25): Difficulty, HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, Color, TMP_Text (+17 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.18
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (78): CompiledConflictZone, JunctionMovement, RoadModelCanonicalPayload, Comparison, Vector3, ConflictKind, Crossing, Merge (+70 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - ".FromRoute"
Cohesion: 0.40
Nodes (3): CompiledRoadModel, IReadOnlyList, List

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (46): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference (+38 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "FileLayout"
Cohesion: 0.33
Nodes (6): FileLayout, ConflictRecord, ControlRecord, DeferredRecord, DispositionRecord, WidthRecord

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "PairDecisionState"
Cohesion: 0.33
Nodes (6): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "ConflictFilter"
Cohesion: 0.40
Nodes (5): ConflictFilter, All, Approach, Movement, Zone

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.12
Nodes (18): RoadId, V2InteractionRecord, V2JunctionTrace, SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed (+10 more)

### Community 147 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 148 - "ImportContext"
Cohesion: 0.20
Nodes (7): RoadBoundsBox, RoadCurvePoint, Vector3, AuthoringTask, CircleFit, ImportContext, CircleFit

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 153 - ".InterStepBound"
Cohesion: 0.33
Nodes (6): Quaternion, Vector3, BodyState, GaugeBox, Rho, TrackingMeasurement

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "LongitudinalDecision"
Cohesion: 0.12
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.09
Nodes (25): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+17 more)

### Community 158 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.07
Nodes (26): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+18 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 169 - "V2FallbackReason"
Cohesion: 0.06
Nodes (37): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+29 more)

### Community 170 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 171 - "NetworkedPlayerState"
Cohesion: 0.17
Nodes (10): Rpc, RpcParams, NetworkedPlayerLifecycleIntent, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, HostOwnedNetworkStateBehaviour (+2 more)

### Community 174 - "TrafficV2Code"
Cohesion: 0.11
Nodes (19): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+11 more)

### Community 178 - "GateAEvidenceResult"
Cohesion: 0.15
Nodes (14): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+6 more)

### Community 180 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 186 - ".DiffHash"
Cohesion: 0.21
Nodes (8): HashSet, IList, List, RoadId, SortedDictionary, ClassificationSummary, RefinementInputs, RefinedPair

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "ModelDto"
Cohesion: 0.11
Nodes (15): AdjacencyDto, DrivabilityProfile, RoadModelCompiler, ModelDto, ConnectionDto, ControlDto, CorridorDto, RoadRage.Features.Vehicles.Traffic (+7 more)

### Community 191 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 192 - ".Classify"
Cohesion: 0.16
Nodes (9): AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, ConflictDecision, RoadModelValidationProfile, Vector3 (+1 more)

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (18): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+10 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.24
Nodes (6): PointerEventData, TMP_Text, LobbyCodeClipboard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.14
Nodes (11): Color, HashSet, IReadOnlyList, List, Quaternion, LaneGraph, EntryPortals, ExitPortals (+3 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.10
Nodes (24): IReadOnlyList, Vector3, ObservationChannel, Items, RangeMeters, Saturated, Status, Total (+16 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.31
Nodes (5): DriverProfileDef, Id, Profile, RawId, ScriptableObject

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (13): Vector3, BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion (+5 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1127 isolated node(s):** `RoadRage.App`, `Instance`, `Router`, `Notices`, `Profiles` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1583 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **6 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `.FromRoute`, `Blocker`, `SpeedConstraint`, `V2StageTimings`, `AgentObservation`, `.InterStepBound`, `LongitudinalDecision`, `TrafficV2Insertion`, `TrafficFrame`, `RoutePlan`, `V2FallbackReason`, `NetworkedPlayerState`, `JunctionActorReport`, `.Decide`, `TrafficV2HazardCollector`, `TrafficDecisionProjection`, `ElementOccupant`, `ReferenceTrack`, `TrackingTolerance`, `RoadRage.Features.Vehicles.Traffic.Planning`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.254) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `MeasurementRun`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `MonoBehaviour`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `LaneGraph`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.168) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `RoadModelRecords.cs`, `.DiffHash`, `RoadCurve`, `RoadModelDocument`, `RoadGeometryValidator`, `.Localize`, `RoadLineage`, `RoadModelCanonicalWriter`, `.TryParse`, `RoadModelValidationIssue`, `.Evaluate`, `PortalTrafficSpawner`, `TrafficV2Insertion`, `TrafficV2HazardCollector`?**
  _High betweenness centrality (0.105) - this node is a cross-community bridge._
- **What connects `RoadRage.App`, `Instance`, `Router` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `NetworkedVehicleSeatService` be split into smaller, more focused modules?**
  _Cohesion score 0.14795008912655971 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10365853658536585 - nodes in this community are weakly interconnected._
- **Should `MeasurementRun` be split into smaller, more focused modules?**
  _Cohesion score 0.10276679841897234 - nodes in this community are weakly interconnected._