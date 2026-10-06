# Graph Report - RRS  (2026-10-06)

## Corpus Check
- 178 files · ~242,351 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4607 nodes · 10521 edges · 224 communities (185 shown, 38 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 458 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `1d16c4e7`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedVehicleSeatService
- .Refine
- TrafficV2Composition.cs
- GreyboxAssetSeedMetadata
- SweepElement
- .Core
- GateAEvidenceParameters
- .Localize
- TireSample
- VehicleSuspensionModel
- MonoBehaviour
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyRosterService
- LocalOnFootController
- Blocker
- RageTuningDef
- .Build
- MigrationReport
- VehiclePhysicsBody
- Bounds
- AgentObservation
- AuthoredRoadModel
- SweepPose
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1ImportResult
- JunctionSnapshot
- CompiledRoadModel
- AuthoringDecisions
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- JunctionClearance
- RoutePlan
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- .Decide
- ConflictSweep
- IPathGeometry
- MotionPlan
- JunctionActorReport
- PairReviewModel
- .Measure
- TrafficDriveOutcome
- .Regenerate
- MainMenuScreen
- .Add
- .Add
- DefinitionId
- PlayerProfile
- ImportedCurve
- NetworkedAIVehicleState
- .Run
- PairReviewEntry
- .FixedUpdate
- NetworkPlayerRegistry
- LongitudinalArbitration.cs
- PlanningDecision
- TrackPiece
- FacepunchSteamLobbyPlatform
- NetworkedRunSessionMonitor
- PassengerActionVerdictCode
- .Compute
- TrafficDecisionProjection
- SpatialEntry
- ObservationChannel
- TrafficV2HazardCollector
- RoadId
- UserNotice
- StopHoldState
- TrafficLongitudinalOutcome
- .CheckVisuals
- .Collect
- NetworkedPlayerState
- KinematicOffsetBounds
- RageRoadEventState
- JunctionCoordinator
- .EvaluatePaths
- HistoricalMovementReader
- LobbyFlowController
- RoadModelValidationIssue
- RageDisposition
- .Draw
- RoadLineage
- .Measure
- V1Node
- JunctionConflictIndex
- TrackingTolerance
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- LobbyPlayerSlotView
- RoadRage.Shared.Definitions
- PerceivedObstacleKind
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- VehicleDriveIntent
- RoadGeometryValidator
- .CheckControlKindsAndLines
- .Evaluate
- RoadModelVersion
- .AddMovement
- JunctionRecords.cs
- LongitudinalDecision
- DispositionKind
- TrafficSettingsDef
- .Assemble
- DriverProfile
- StatusFilter
- PlanningReach
- RoadRage.Features.Vehicles.Traffic.Planning
- MainMenuFlowController
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- LocalVoidRespawnController
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyShellScreen
- RunEscapeMenuFlowController
- RoadId
- SpeedPlan
- AIVehicleBehaviorDebugView
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- .Track
- LeafState
- RoadModelDocument
- .GapSeconds
- LaneNode
- RouteReason
- RoadModelCanonicalWriter
- SpeedConstraint
- .ApplyHonkTarget
- ImportContext
- .Step
- HazardRootClass
- NetworkedVehicleSeatIntent
- .ZoneSpan
- ReferenceTrack
- .Run
- RoadRage.Features.Vehicles.Traffic.Migration
- RoadModelCompilationException
- TrafficV2Insertion
- PathIssue
- NetworkedPlayerLifecycleService
- .Configure
- ProjectionKey
- RoadModelSource
- RoutePath
- AutomatedPairClassification
- CompiledJunctionControl
- NetworkedPassengerActionIntent
- CampaignTraceability
- SpeedPlanIssue
- V2FallbackReason
- HostOwnedNetworkStateBehaviour
- NetworkedPlayerLifecycleIntent
- .TryGetMovement
- ReferenceCoverage
- TrafficV2Code
- Action
- KeyValuePair
- Predicate
- GateAEvidenceResult
- RoadLocation
- NetworkedPlayerReviveIntent
- Scene
- VehicleFootprint
- LaneGraphRouting
- FileLayout
- Func
- AuthoredRun
- AuthoringDecisions
- AutomatedPairClassification
- NetworkedVehicleRecoveryIntent
- ModelDto
- TrafficHazardKind
- CompiledJunctionMovement
- RoadRage.Shared.Domain
- CompiledRoadModel
- ConflictDecision
- LobbyRosterScreen
- Dictionary
- RoadRage.Features.Vehicles.Traffic
- IList
- NetworkedPlayerPresentation
- List
- MenuItem
- PairSweep
- RoadId
- RoadModelValidationProfile
- SortedDictionary
- SweepPose
- Vector3
- MatchSettings
- NetworkedVehicleState.cs
- BlockerSource
- TrafficV2WorkCounters.cs
- IHostOwnedRuntimeState
- Portal
- RoadBoundsBox
- LaneGraph
- TrafficPerception
- JunctionRecord
- JunctionSnapshot
- DriverProfile
- DriverProfileDef
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 107 edges
2. `RunFlowController` - 99 edges
3. `ConflictSweep` - 72 edges
4. `NetworkedVehicleState` - 67 edges
5. `ImportContext` - 67 edges
6. `AuthoredRoadModel` - 66 edges
7. `CompiledRoadModel` - 64 edges
8. `RoadId` - 62 edges
9. `NetworkedVehicleDriverController` - 58 edges
10. `TrafficFrame` - 56 edges

## Surprising Connections (you probably didn't know these)
- `JunctionCoordinator` --references--> `JunctionConflictIndex`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionConflictIndex.cs
- `JunctionCoordinator` --references--> `JunctionSnapshot`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs
- `TrafficV2StepRunner` --references--> `JunctionCoordinator`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs
- `Grant` --references--> `JunctionReason`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs
- `Pending` --references--> `JunctionTraversal`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs

## Import Cycles
- None detected.

## Communities (224 total, 38 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (17): ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2, MovementSide (+9 more)

### Community 2 - "TrafficV2Composition.cs"
Cohesion: 0.09
Nodes (28): IReadOnlyList, RoadId, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind (+20 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.13
Nodes (18): Dictionary, IReadOnlyList, RoadCurve, RoadCurveSample, RoadId, PairRelation, Candidate, EnvelopeOnly (+10 more)

### Community 5 - ".Core"
Cohesion: 0.19
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - ".Localize"
Cohesion: 0.14
Nodes (18): ConditionalWeakTable, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query, RoadKinematicAnchor (+10 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.11
Nodes (15): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+7 more)

### Community 11 - "RunFlowController"
Cohesion: 0.05
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.11
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.17
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyRosterService"
Cohesion: 0.10
Nodes (15): Difficulty, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId (+7 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.13
Nodes (15): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+7 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.05
Nodes (31): TMP_Text, RageStateDebugView, List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue (+23 more)

### Community 19 - ".Build"
Cohesion: 0.25
Nodes (10): ConditionalWeakTable, IReadOnlyList, List, RoadId, JunctionRequestBuilder, DriverProfile, ElementOccupant, RouteOccurrence (+2 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 23 - "AgentObservation"
Cohesion: 0.12
Nodes (22): Func, LaneSide, RoadId, RoadLocation, StringBuilder, Vector3, VehicleFootprint, AdjacentOccupantFact (+14 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.08
Nodes (26): CompiledJunctionControl, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, KinematicOffsetBounds, List (+18 more)

### Community 25 - "SweepPose"
Cohesion: 0.15
Nodes (11): Vector2, GridPath, PoseFrame, SweepPose, Plan, IReadOnlyList, Vector2, KinematicPoseSet (+3 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (23): CompiledRoadModel, Dictionary, HashSet, IList, List, PairSweep, RoadId, RoadModelValidationProfile (+15 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.14
Nodes (16): AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedConnection, ImportedPortal, PublishedDisplacement, SourceDisposition (+8 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.11
Nodes (14): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale (+6 more)

### Community 30 - "CompiledRoadModel"
Cohesion: 0.07
Nodes (24): Dictionary, List, RoadModelValidationProfile, RoadSection, CompiledRoadModel, Adjacencies, ConflictZones, Connections (+16 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (47): AppliedWidth, ConflictKind, Dictionary, IList, JunctionControlKind, List, RoadCurveSample, RoadId (+39 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.08
Nodes (30): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+22 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.08
Nodes (28): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+20 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.11
Nodes (9): Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3 (+1 more)

### Community 35 - "JunctionClearance"
Cohesion: 0.24
Nodes (4): IReadOnlyList, RoadModelValidationProfile, Vector2, JunctionClearance

### Community 36 - "RoutePlan"
Cohesion: 0.11
Nodes (20): IReadOnlyList, RoadElementKind, RoadId, RoadModelVersion, RouteOccurrence, RoutePlan, Diagnostics, DistanceMeters (+12 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.14
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.10
Nodes (22): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, HorizonEnd, ExitPortal, LookAheadLimit (+14 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.14
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.09
Nodes (23): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+15 more)

### Community 43 - "RoadCurve"
Cohesion: 0.13
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Decide"
Cohesion: 0.14
Nodes (16): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing (+8 more)

### Community 45 - "ConflictSweep"
Cohesion: 0.21
Nodes (10): RoadBoundsBox, ConflictCandidate, CompiledRoadModel, IList, List, RoadBoundsBox, RoadModelValidationProfile, ConflictSweep (+2 more)

### Community 46 - "IPathGeometry"
Cohesion: 0.23
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 47 - "MotionPlan"
Cohesion: 0.13
Nodes (18): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+10 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.07
Nodes (30): IReadOnlyList, RoadId, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest (+22 more)

### Community 49 - "PairReviewModel"
Cohesion: 0.14
Nodes (12): CompiledRoadModel, Dictionary, List, RoadBoundsBox, StringBuilder, PairReview, PairReviewModel, PairReviewStatus (+4 more)

### Community 50 - ".Measure"
Cohesion: 0.14
Nodes (16): CompiledRoadModel, GateAEvidenceParameters, IReadOnlyList, KinematicOffsetBounds, List, PairSweep, RoadId, RoadModelValidationProfile (+8 more)

### Community 51 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (16): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+8 more)

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 54 - ".Add"
Cohesion: 0.13
Nodes (16): CompiledJunctionMovement, IReadOnlyList, RoadBoundsBox, RoadCurve, RoadId, RoadModelValidationProfile, Vector3, LocalizationFixture (+8 more)

### Community 55 - ".Add"
Cohesion: 0.25
Nodes (3): JunctionFeature, Predicate, ImportedJunction

### Community 56 - "DefinitionId"
Cohesion: 0.07
Nodes (20): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+12 more)

### Community 57 - "PlayerProfile"
Cohesion: 0.08
Nodes (18): PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, PlayerProfileResolution, Error, IsResolved, Profile (+10 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.23
Nodes (11): Dictionary, IReadOnlyList, KeyValuePair, List, RoadCurve, RoadCurveSample, RoadRecordKind, ImportedCurve (+3 more)

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.36
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 60 - ".Run"
Cohesion: 0.14
Nodes (10): Action, GateAEvidenceParameters, MenuItem, GateABinding, MenuItem, MenuItem, Func, KeyValuePair (+2 more)

### Community 61 - "PairReviewEntry"
Cohesion: 0.19
Nodes (8): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, PairReviewActions, PairReviewEntry

### Community 62 - ".FixedUpdate"
Cohesion: 0.27
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 63 - "NetworkPlayerRegistry"
Cohesion: 0.12
Nodes (9): HashSet, NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry, ConnectionApprovalRequest (+1 more)

### Community 64 - "LongitudinalArbitration.cs"
Cohesion: 0.16
Nodes (16): IReadOnlyList, RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader (+8 more)

### Community 65 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 66 - "TrackPiece"
Cohesion: 0.16
Nodes (10): RoadCurve, RoadElementKind, RoadId, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 67 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.07
Nodes (24): Difficulty, Task, FacepunchSteamLobbyPlatform, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId, Success (+16 more)

### Community 68 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.05
Nodes (37): Rpc, RpcParams, PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId (+29 more)

### Community 70 - ".Compute"
Cohesion: 0.17
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "SpatialEntry"
Cohesion: 0.09
Nodes (24): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementOccupant, IntentInterval (+16 more)

### Community 73 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.11
Nodes (17): TrafficHazardCollectorCounters, Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity (+9 more)

### Community 75 - "RoadId"
Cohesion: 0.18
Nodes (15): ConflictKind, IReadOnlyList, RoadBoundsBox, RoadCurve, RoadCurveSample, RoadId, CompiledConflictZone, CompiledJunctionMovement (+7 more)

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "StopHoldState"
Cohesion: 0.12
Nodes (13): StopHoldPhase, Entered, Holding, None, Released, StopHoldRelease, GapOpened, GrantEffective (+5 more)

### Community 78 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.13
Nodes (11): Surface, IEnumerable, Renderer, StringBuilder, Transform, VisibleFaces, Component, Mesh (+3 more)

### Community 80 - ".Collect"
Cohesion: 0.19
Nodes (12): Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3, VehiclePhysicsBody (+4 more)

### Community 81 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+9 more)

### Community 83 - "RageRoadEventState"
Cohesion: 0.18
Nodes (8): IReadOnlyList, RageRoadEventLifecycle, RageRoadEventState, Confrontation, Idle, Resolved, RewardGranted, Triggered

### Community 84 - "JunctionCoordinator"
Cohesion: 0.14
Nodes (21): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadId, Grant, JunctionCoordinator, Batches (+13 more)

### Community 85 - ".EvaluatePaths"
Cohesion: 0.18
Nodes (9): Vector3, RefineNode, RefineSegment, GridPath, LeafState, MovementSide, RefineContext, RefineNode (+1 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.20
Nodes (11): JunctionRecord, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, Document (+3 more)

### Community 88 - "LobbyFlowController"
Cohesion: 0.10
Nodes (4): NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.26
Nodes (13): ConflictZone, Dictionary, List, RoadCurveSample, RoadId, RoadModelSource, RoadSection, Vector3 (+5 more)

### Community 90 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 91 - ".Draw"
Cohesion: 0.23
Nodes (8): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 92 - "RoadLineage"
Cohesion: 0.06
Nodes (38): Dictionary, IReadOnlyList, List, RoadId, Vector3, Entry, RightOfWay, RightOfWayRelation (+30 more)

### Community 93 - ".Measure"
Cohesion: 0.13
Nodes (21): BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, RoadId, Scene (+13 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (29): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+21 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.11
Nodes (16): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, RoadId, Vector3, Entry, JunctionConflictIndex (+8 more)

### Community 96 - "TrackingTolerance"
Cohesion: 0.31
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

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
Cohesion: 0.30
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 102 - "RoadRage.Shared.Definitions"
Cohesion: 0.11
Nodes (9): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner, RoadRage.Features.Rage, RoadRage.Features.PassengerActions (+1 more)

### Community 103 - "PerceivedObstacleKind"
Cohesion: 0.33
Nodes (6): PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle, WalkingPlayer

### Community 104 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.18
Nodes (7): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState

### Community 107 - "MenuCharacterPreview"
Cohesion: 0.20
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - ".CheckControlKindsAndLines"
Cohesion: 0.24
Nodes (9): IReadOnlyList, List, RoadCurveSample, RoadLineSegment, Vector3, StopLineProjection, JunctionControl, JunctionMovement (+1 more)

### Community 112 - ".Evaluate"
Cohesion: 0.16
Nodes (14): CompiledRoadModel, RoadLocation, DecisionCounter, RouteDiagnostic, None, ZeroWeightFallback, RouteOutcome, InvalidInput (+6 more)

### Community 113 - "RoadModelVersion"
Cohesion: 0.32
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 114 - ".AddMovement"
Cohesion: 0.21
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 115 - "JunctionRecords.cs"
Cohesion: 0.09
Nodes (21): JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations, RouteEnd (+13 more)

### Community 116 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 117 - "DispositionKind"
Cohesion: 0.18
Nodes (11): DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint, Movement, MovementApproachPath (+3 more)

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - ".Assemble"
Cohesion: 0.16
Nodes (8): DrivabilityProfile, IList, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter, ImportManifestEntry

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.15
Nodes (10): PlanningTolerances, ProfilerMarker, PlanningSpine, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing (+2 more)

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.17
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

### Community 128 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.08
Nodes (21): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+13 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.12
Nodes (17): HashSet, List, ProfilerMarker, RoadId, Stopwatch, TrafficV2StepRunner, Collector, Coordinator (+9 more)

### Community 131 - "LobbyShellScreen"
Cohesion: 0.11
Nodes (9): Difficulty, Button, TMP_Text, LobbyShellScreen, Difficulty, Easy, Hard, Normal (+1 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.12
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadId"
Cohesion: 0.05
Nodes (67): Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile, ImportManifest, ImportManifestEntry (+59 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - "AIVehicleBehaviorDebugView"
Cohesion: 0.25
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (48): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference (+40 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - ".Track"
Cohesion: 0.22
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.08
Nodes (36): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+28 more)

### Community 142 - ".GapSeconds"
Cohesion: 0.24
Nodes (3): JunctionKinematics, Known, JunctionPriority

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 148 - "ImportContext"
Cohesion: 0.23
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 149 - ".Step"
Cohesion: 0.10
Nodes (15): DriverProfile, IReadOnlyList, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord (+7 more)

### Community 150 - "HazardRootClass"
Cohesion: 0.25
Nodes (7): HazardRootClass, Obstacle, Self, Static, TrafficV2Vehicle, Vehicle, WalkingPlayer

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - ".ZoneSpan"
Cohesion: 0.32
Nodes (4): Bounds, CompiledRoadModel, RoadBoundsBox, Vector3

### Community 153 - "ReferenceTrack"
Cohesion: 0.11
Nodes (22): IReadOnlyList, Quaternion, RoadKinematicAnchor, Vector3, BodyState, GaugeBox, Rho, InterStepResult (+14 more)

### Community 154 - ".Run"
Cohesion: 0.48
Nodes (4): CompiledRoadModel, IEnumerable, JunctionSnapshot, TrafficV2StepCost

### Community 155 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.17
Nodes (7): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, RoadRage.Features.Vehicles.Traffic.Migration

### Community 156 - "RoadModelCompilationException"
Cohesion: 0.29
Nodes (4): IReadOnlyList, RoadModelCompilationException, Issues, Exception

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadLocation, Vector3, TrafficV2Admission (+21 more)

### Community 158 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 160 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 161 - "ProjectionKey"
Cohesion: 0.33
Nodes (3): ProjectionKey, IEquatable, ProjectionKey

### Community 162 - "RoadModelSource"
Cohesion: 0.10
Nodes (25): DrivabilityProfile, JunctionMovement, RoadModelCanonicalPayload, Comparison, RoadModelCompiler, RoadClass, Arterial, Highway (+17 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - "AutomatedPairClassification"
Cohesion: 0.22
Nodes (8): AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, CompiledJunctionMovement, PairRefinement, ZoneTyping

### Community 165 - "CompiledJunctionControl"
Cohesion: 0.50
Nodes (3): JunctionControlKind, RoadLineSegment, CompiledJunctionControl

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.12
Nodes (16): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, List, PassengerActionCatalog (+8 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.09
Nodes (21): Dictionary, HashSet, IReadOnlyDictionary, CampaignTraceability, Elements, ElementStatus, Measured, NotMeasured (+13 more)

### Community 168 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (32): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+24 more)

### Community 170 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.17
Nodes (9): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, HostOwnedNetworkStateBehaviour, IsHostAuthority, RoadRage.Features.Economy, RoadRage.Features.Boss (+1 more)

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 172 - ".TryGetMovement"
Cohesion: 0.15
Nodes (10): CompiledJunctionControl, CompiledRoadModel, RoadCurveSample, CompiledRoadModel, List, Dictionary, RoadElementKind, None (+2 more)

### Community 173 - "ReferenceCoverage"
Cohesion: 0.40
Nodes (5): ReferenceCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA

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
Cohesion: 0.24
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "ModelDto"
Cohesion: 0.15
Nodes (13): AdjacencyDto, ModelDto, RoadLocalizationProfile, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto (+5 more)

### Community 191 - "TrafficHazardKind"
Cohesion: 0.33
Nodes (5): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (17): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+9 more)

### Community 196 - "LobbyRosterScreen"
Cohesion: 0.08
Nodes (13): Button, Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint (+5 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 209 - "MatchSettings"
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 210 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 211 - "BlockerSource"
Cohesion: 0.40
Nodes (5): BlockerSource, DrivingPolicy, JunctionCoordination, LeaderObservation, ObstacleObservation

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.19
Nodes (11): VehicleGapFact, Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance, PerceptionLimits (+3 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.27
Nodes (5): DriverProfileDef, Id, Profile, RawId, ScriptableObject

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1148 isolated node(s):** `Model`, `IncompatiblePairCount`, `Index`, `Model`, `Current` (+1143 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1671 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **38 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `Blocker`, `.Step`, `AgentObservation`, `ReferenceTrack`, `.Run`, `TrafficV2Insertion`, `RoutePlan`, `V2FallbackReason`, `HostOwnedNetworkStateBehaviour`, `.Decide`, `JunctionActorReport`, `TrafficDecisionProjection`, `SpatialEntry`, `.Collect`, `TrackingTolerance`, `LongitudinalDecision`, `RoadRage.Features.Vehicles.Traffic.Planning`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.192) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `TrafficV2Composition.cs`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `MonoBehaviour`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `.Step`, `LaneGraph`, `RoadRage.Features.Vehicles.Traffic.Planning`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.185) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `ProjectionKey`, `RoadModelSource`, `.Localize`, `TrafficV2HazardCollector`, `.TryGetMovement`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadModelCanonicalWriter`, `AutomatedPairDecisionPolicy`, `RoadLineage`, `TrafficV2Insertion`, `CompiledRoadModel`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.101) - this node is a cross-community bridge._
- **What connects `Model`, `IncompatiblePairCount`, `Index` to the rest of the system?**
  _1148 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10931174089068826 - nodes in this community are weakly interconnected._
- **Should `TrafficV2Composition.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.09032258064516129 - nodes in this community are weakly interconnected._
- **Should `SweepElement` be split into smaller, more focused modules?**
  _Cohesion score 0.13054187192118227 - nodes in this community are weakly interconnected._