# Graph Report - RRS  (2026-10-06)

## Corpus Check
- 174 files · ~234,483 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4430 nodes · 10237 edges · 196 communities (173 shown, 22 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 445 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `81f88abd`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedPlayerState
- .Refine
- TrafficV2Composition.cs
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
- LobbyRosterService
- LocalOnFootController
- Blocker
- RageTuningDef
- .Build
- MigrationReport
- VehiclePhysicsBody
- .Add
- AgentObservation
- AuthoredRun
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
- JunctionClearance
- RoutePlan
- NetworkedVehicleState
- VehicleProfile
- V1ImportResult
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- LongitudinalArbitration.cs
- PairSweep
- IPathGeometry
- MotionPlan
- JunctionActorReport
- PairReviewModel
- .Decide
- TrafficDriveOutcome
- .Regenerate
- MainMenuScreen
- RoadRage.Features.Vehicles.Traffic.Migration
- .AddMovement
- DefinitionId
- LobbyRosterScreen
- ImportedCurve
- NetworkedAIVehicleState
- AuthoredRoadModel
- PairReviewEntry
- .FixedUpdate
- .Collect
- V2StepRecord
- PlanningDecision
- TrackPiece
- FacepunchSteamLobbyPlatform
- NetworkedRunSessionMonitor
- PassengerActionVerdictCode
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- ObservationChannel
- TrafficV2HazardCollector
- SweepPose
- UserNotice
- TrafficV2Admission
- TrafficLongitudinalOutcome
- .CheckVisuals
- .FingerprintWithInputs
- LongitudinalCandidateKind
- KinematicOffsetBounds
- .HandleRosterChanged
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- StopHoldPhase
- LobbyFlowController
- RoadModelValidationIssue
- RageDisposition
- .Draw
- RoadLineage
- JunctionClearanceResult
- V1SourceSet
- JunctionConflictIndex
- TrackingTolerance
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- LobbyPlayerSlotView
- .Create
- PerceivedObstacleKind
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- HazardRootClass
- RoadGeometryValidator
- NetworkedBossState.cs
- RouteReason
- RoadModelVersion
- MatchSettings
- JunctionRecords.cs
- TrafficJunctionOutcome
- NetworkedCrewEconomyState.cs
- TrafficSettingsDef
- .ApplyWidths
- DriverProfile
- StatusFilter
- .Evaluate
- RoadRage.Features.Vehicles.Traffic.Planning
- MainMenuFlowController
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- .IsSurfaceOnlyCollision
- RoadRageBootstrap
- TrafficV2StepRunner
- .Awake
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- TrafficHazardKind
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- FileLayout
- LeafState
- RoadModelDocument
- AppSceneRouter.cs
- LaneNode
- TrafficV2Settings
- RoadModelCanonicalWriter
- SpeedConstraint
- RunEscapeMenuScreen
- ImportContext
- .Step
- PlayerNameValidator
- NetworkedVehicleSeatIntent
- PerceptionUnavailableReason
- ReferenceTrack
- IHostOwnedRuntimeState
- .Read
- LongitudinalDecision
- TrafficV2Insertion
- PathIssue
- NetworkedPlayerLifecycleService
- ConflictCandidate
- .MeasurePath
- RoutePath
- CompiledJunctionMovement
- CompiledRoadModel
- NetworkedPassengerActionIntent
- CampaignTraceability
- SpeedPlanIssue
- V2FallbackReason
- NetworkedVehicleState.cs
- NetworkedPlayerLifecycleIntent
- Dictionary
- IList
- TrafficV2Code
- List
- MenuItem
- RoadId
- GateAEvidenceResult
- SortedDictionary
- NetworkedPlayerReviveIntent
- Vector3
- LaneGraphRouting
- HashSet
- NetworkedVehicleRecoveryIntent
- ModelDto
- PairRelation
- RoadModelValidationProfile
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
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 63 edges
8. `CompiledRoadModel` - 63 edges
9. `TrafficFrame` - 59 edges
10. `NetworkedVehicleDriverController` - 58 edges

## Surprising Connections (you probably didn't know these)
- `NetworkedPlayerSpawnService` --references--> `NetworkedVehicleSeatService`  [EXTRACTED]
  Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs → Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs
- `NetworkedVehicleSeatService` --references--> `RunCheckpointHudScreen`  [EXTRACTED]
  Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs → Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs
- `PairRefinement` --references--> `SweepPose`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweepRefinement.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs
- `RefineContext` --references--> `SweepGraph`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweepRefinement.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs
- `RefineContext` --references--> `KinematicOffsetBounds`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweepRefinement.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/KinematicOffsetBounds.cs

## Import Cycles
- None detected.

## Communities (196 total, 22 thin omitted)

### Community 0 - "NetworkedPlayerState"
Cohesion: 0.10
Nodes (15): Vector3, NetworkedVehicleSeatService, Instance, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, Vector3 (+7 more)

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (17): ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2, MovementSide (+9 more)

### Community 2 - "TrafficV2Composition.cs"
Cohesion: 0.11
Nodes (23): IReadOnlyList, RoadId, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind (+15 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.18
Nodes (13): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+5 more)

### Community 5 - ".Core"
Cohesion: 0.19
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - "RoadId"
Cohesion: 0.05
Nodes (53): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+45 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.12
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "RoadRage.Features.Online"
Cohesion: 0.07
Nodes (16): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+8 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyRosterService"
Cohesion: 0.09
Nodes (15): Difficulty, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId (+7 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.05
Nodes (31): TMP_Text, RageStateDebugView, List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue (+23 more)

### Community 19 - ".Build"
Cohesion: 0.24
Nodes (9): JunctionMovementPosition, ConditionalWeakTable, DriverProfile, IReadOnlyList, RoadId, Vector3, JunctionRequestBuilder, RoadElementKind (+1 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - ".Add"
Cohesion: 0.20
Nodes (12): Bounds, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation, Vector3 (+4 more)

### Community 23 - "AgentObservation"
Cohesion: 0.13
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 24 - "AuthoredRun"
Cohesion: 0.11
Nodes (15): CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, RoadModelSource, RoadModelValidationProfile, SortedDictionary (+7 more)

### Community 25 - ".Measure"
Cohesion: 0.17
Nodes (13): BoxCollider, Collider, CompiledRoadModel, GameObject, List, Scene, Vector3, VehiclePhysicsBody (+5 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.08
Nodes (30): AutomatedPairDecisionManifest, AutomatedPairDecisionPlan, AutomatedPairDecisionPolicy, AutomatedPairDecisionRecord, ClassificationSummary, RefinedPair, RefinementInputs, AuthoredRun (+22 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.09
Nodes (22): DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint, Movement, MovementApproachPath (+14 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.17
Nodes (8): JunctionBatchCounters, JunctionRecord, IsEffectiveGrant, JunctionSnapshot, Counters, EffectiveFrame, Records, SourceFrame

### Community 30 - "V1Node"
Cohesion: 0.16
Nodes (12): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedPortal, PublishedDisplacement, PortalRole, V1Node, IsEntryPortal (+4 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (39): ConflictKind, Dictionary, FileLayout, Func, IList, JunctionControlKind, List, RoadCurveSample (+31 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (33): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+25 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.07
Nodes (31): MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing, GateAEvidenceStale, HorizonNonConforming (+23 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.08
Nodes (17): DevIndestructibleVehicle, Collider, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc, RpcParams (+9 more)

### Community 35 - "JunctionClearance"
Cohesion: 0.30
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 36 - "RoutePlan"
Cohesion: 0.11
Nodes (18): IReadOnlyList, RoadId, RoadModelVersion, RoutePlan, Diagnostics, DistanceMeters, ExitPortalId, ModelId (+10 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "V1ImportResult"
Cohesion: 0.12
Nodes (18): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+10 more)

### Community 40 - "PathHorizon"
Cohesion: 0.09
Nodes (25): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd, ExitPortal (+17 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.11
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - "LongitudinalArbitration.cs"
Cohesion: 0.13
Nodes (19): IReadOnlyList, RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader (+11 more)

### Community 45 - "PairSweep"
Cohesion: 0.32
Nodes (6): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "IPathGeometry"
Cohesion: 0.22
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 47 - "MotionPlan"
Cohesion: 0.13
Nodes (17): VehicleCoverage, DrivabilityProfile, IReadOnlyList, LongitudinalBounds, Valid, MotionPlan, Diagnostics, Evidence (+9 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.09
Nodes (25): IReadOnlyList, RoadId, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest (+17 more)

### Community 49 - "PairReviewModel"
Cohesion: 0.18
Nodes (8): CompiledRoadModel, Dictionary, List, RoadBoundsBox, RoadId, StringBuilder, PairReview, PairReviewModel

### Community 50 - ".Decide"
Cohesion: 0.26
Nodes (5): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, StopHoldParameters

### Community 51 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (16): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+8 more)

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 55 - ".AddMovement"
Cohesion: 0.24
Nodes (5): JunctionFeature, ImportedConnection, ImportedJunction, V1Edge, Key

### Community 56 - "DefinitionId"
Cohesion: 0.05
Nodes (32): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+24 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.16
Nodes (14): Dictionary, IList, IReadOnlyList, KeyValuePair, List, Predicate, RoadCurve, RoadCurveSample (+6 more)

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.17
Nodes (10): IReadOnlyList, Vector3, AiRageTargetResolution, Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, NetworkVariable (+2 more)

### Community 60 - "AuthoredRoadModel"
Cohesion: 0.10
Nodes (13): Action, IList, KeyValuePair, List, MenuItem, Scene, AuthoredRoadModel, SignoffLayout (+5 more)

### Community 61 - "PairReviewEntry"
Cohesion: 0.15
Nodes (13): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, PairReviewActions, PairReviewEntry (+5 more)

### Community 62 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 63 - ".Collect"
Cohesion: 0.16
Nodes (13): TrafficHazardCollectorCounters, Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3 (+5 more)

### Community 64 - "V2StepRecord"
Cohesion: 0.14
Nodes (16): V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None, ProfileScalarNonFinite, RollingBackward, IReadOnlyList (+8 more)

### Community 65 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 66 - "TrackPiece"
Cohesion: 0.19
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 67 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.07
Nodes (24): Difficulty, Task, FacepunchSteamLobbyPlatform, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId, Success (+16 more)

### Community 68 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 70 - ".Compute"
Cohesion: 0.22
Nodes (9): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+1 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.09
Nodes (24): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementOccupant, IntentInterval (+16 more)

### Community 73 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.12
Nodes (16): Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity, Counters (+8 more)

### Community 75 - "SweepPose"
Cohesion: 0.24
Nodes (9): Vector2, GridPath, PoseFrame, SweepPose, Plan, IReadOnlyList, Vector2, KinematicPoseSet (+1 more)

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "TrafficV2Admission"
Cohesion: 0.17
Nodes (10): DriverProfile, GameObject, TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle (+2 more)

### Community 78 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.24
Nodes (6): Surface, HashSet, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - ".FingerprintWithInputs"
Cohesion: 0.23
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 81 - "LongitudinalCandidateKind"
Cohesion: 0.18
Nodes (11): LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable, Profile, SteeringCeilingUnreachable (+3 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+9 more)

### Community 83 - ".HandleRosterChanged"
Cohesion: 0.24
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 84 - "JunctionCoordinator"
Cohesion: 0.15
Nodes (20): CompiledRoadModel, Dictionary, IReadOnlyList, JunctionRecord, JunctionSnapshot, List, RoadId, Grant (+12 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.16
Nodes (10): ConflictSweep, RefineNode, RefineSegment, GridPath, LeafState, MovementSide, PoseFrame, RefineContext (+2 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.20
Nodes (10): JunctionRecord, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, Document, HistoricalMovement (+2 more)

### Community 87 - "StopHoldPhase"
Cohesion: 0.40
Nodes (5): StopHoldPhase, Entered, Holding, None, Released

### Community 88 - "LobbyFlowController"
Cohesion: 0.11
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (19): RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 90 - "RageDisposition"
Cohesion: 0.22
Nodes (7): RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated, Ram

### Community 91 - ".Draw"
Cohesion: 0.20
Nodes (9): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, Color, IReadOnlyList, RoadCurveSample, RoadModelValidationProfile (+1 more)

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 94 - "V1SourceSet"
Cohesion: 0.12
Nodes (21): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+13 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.15
Nodes (12): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, Portal, RoadBoundsBox, RoadId, Vector3 (+4 more)

### Community 96 - "TrackingTolerance"
Cohesion: 0.31
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

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
Cohesion: 0.30
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 102 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 103 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

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

### Community 109 - "HazardRootClass"
Cohesion: 0.25
Nodes (7): HazardRootClass, Obstacle, Self, Static, TrafficV2Vehicle, Vehicle, WalkingPlayer

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.16
Nodes (13): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, Vector3 (+5 more)

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
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 115 - "JunctionRecords.cs"
Cohesion: 0.09
Nodes (20): JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations, RouteEnd (+12 more)

### Community 116 - "TrafficJunctionOutcome"
Cohesion: 0.18
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 117 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - ".Evaluate"
Cohesion: 0.14
Nodes (17): IReadOnlyList, PlanningRequest, CompiledRoadModel, RoadLocation, DecisionCounter, RouteDiagnostic, None, ZeroWeightFallback (+9 more)

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.14
Nodes (13): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, ProfilerMarker, PlanningSpine, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics (+5 more)

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.17
Nodes (6): IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

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
Nodes (70): JunctionMovement, RoadModelCanonicalPayload, DrivabilityProfile, ImportManifest, ImportManifestEntry, Junction, JunctionControl, JunctionControlKind (+62 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - "TrafficHazardKind"
Cohesion: 0.33
Nodes (5): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer

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
Cohesion: 0.06
Nodes (40): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+32 more)

### Community 142 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.21
Nodes (6): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter, ReadOnlyMemory

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 148 - "ImportContext"
Cohesion: 0.14
Nodes (10): DrivabilityProfile, RoadBoundsBox, RoadCurvePoint, RoadLocalizationProfile, RoadModelValidationProfile, Vector3, CircleFit, ImportContext (+2 more)

### Community 149 - ".Step"
Cohesion: 0.15
Nodes (6): DriverProfile, JunctionSnapshot, List, V2StageTimings, TotalMillisecondsPerStep, DriverProfile

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "PerceptionUnavailableReason"
Cohesion: 0.40
Nodes (5): PerceptionUnavailableReason, ChannelSaturated, ChannelUnavailable, HazardCollectorSaturated, None

### Community 153 - "ReferenceTrack"
Cohesion: 0.11
Nodes (22): IReadOnlyList, Quaternion, RoadKinematicAnchor, Vector3, BodyState, GaugeBox, Rho, InterStepResult (+14 more)

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.12
Nodes (16): CompiledRoadModel, Portal, Quaternion, RoadLocation, Vector3, TrafficV2Insertion, Code, EntryPortal (+8 more)

### Community 158 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 163 - "RoutePath"
Cohesion: 0.21
Nodes (8): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.05
Nodes (41): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+33 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.07
Nodes (24): CompiledRoadModel, Dictionary, HashSet, IReadOnlyDictionary, List, RoadId, CampaignTraceability, Elements (+16 more)

### Community 168 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (31): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2FallbackReason, ExitPortalReached, FrameUnavailable, HorizonNonConforming, NoCommand (+23 more)

### Community 170 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 178 - "GateAEvidenceResult"
Cohesion: 0.15
Nodes (14): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+6 more)

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
Cohesion: 0.10
Nodes (16): AdjacencyDto, DrivabilityProfile, Comparison, RoadModelCompiler, ModelDto, ConnectionDto, ControlDto, CorridorDto (+8 more)

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
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.22
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
- **1127 isolated node(s):** `PlanningTolerances`, `DrivabilityProfile`, `Signoff`, `SessionTrafficValue`, `JunctionRecord` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1597 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **22 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `Blocker`, `.Step`, `AgentObservation`, `ReferenceTrack`, `LongitudinalDecision`, `TrafficV2Insertion`, `RoutePlan`, `NetworkedPassengerActionIntent`, `CampaignTraceability`, `V2FallbackReason`, `JunctionActorReport`, `.Collect`, `V2StepRecord`, `TrafficDecisionProjection`, `ElementOccupant`, `TrafficV2Admission`, `LongitudinalCandidateKind`, `TrackingTolerance`, `RoadRage.Features.Vehicles.Traffic.Planning`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.233) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `V2StepRecord`, `RoadRage.Shared.Domain`, `TrafficV2Composition.cs`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `MonoBehaviour`, `TrafficV2Admission`, `TrafficV2Code`, `TrafficSettingsDef`, `LaneGraph`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.150) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `AutomatedPairDecisionPolicy`, `RoadModelRecords.cs`, `SpeedPlan`, `PathHorizon`, `TrafficV2HazardCollector`, `TrafficV2Admission`, `RoadGeometryValidator`, `RoadModelDocument`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `.Evaluate`, `RoadLineage`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.118) - this node is a cross-community bridge._
- **What connects `PlanningTolerances`, `DrivabilityProfile`, `Signoff` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `NetworkedPlayerState` be split into smaller, more focused modules?**
  _Cohesion score 0.10253699788583509 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.11095305832147938 - nodes in this community are weakly interconnected._
- **Should `TrafficV2Composition.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.11384615384615385 - nodes in this community are weakly interconnected._