# Graph Report - RRS  (2026-10-06)

## Corpus Check
- 174 files · ~234,317 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4438 nodes · 10226 edges · 187 communities (167 shown, 20 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 447 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `d9cdbe86`
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
- RoadId
- TireSample
- VehicleSuspensionModel
- MonoBehaviour
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
- .CheckVisuals
- AgentObservation
- AuthoredRoadModel
- JunctionClearance
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1ImportResult
- JunctionSnapshot
- .Measure
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
- PairSweep
- IPathGeometry
- MotionPlan
- JunctionActorReport
- PairReviewModel
- .Decide
- TrafficDriveOutcome
- .Regenerate
- MainMenuScreen
- ConflictSweep
- .ImportLaneModule
- CharacterCatalog
- DefinitionId
- ImportedCurve
- NetworkedAIVehicleState
- .FullPath
- LongitudinalCandidateKind
- .FixedUpdate
- TrafficV2HazardCollector
- PlayerMode
- EffectiveLaneCorridor
- TrackPiece
- LobbyRosterService
- NetworkedRunSessionMonitor
- TrafficV2Admission
- .Compute
- TrafficDecisionProjection
- SpatialEntry
- ObservationChannel
- .Assemble
- SweepPose
- UserNotice
- VehicleCoverage
- TrafficLongitudinalOutcome
- PassengerActionVerdictCode
- RageDisposition
- StopHoldRelease
- KinematicOffsetBounds
- LocalVoidRespawnController
- JunctionCoordinator
- TrafficV2Settings
- LongitudinalCandidate
- StopHoldState
- LobbyFlowController
- RoadModelValidationIssue
- Dictionary
- ConflictKind
- RoadLineage
- PlayerProfile
- V1Node
- JunctionConflictIndex
- TrackingTolerance
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- .Track
- .Create
- DispositionKind
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
- .Manifest
- DriverProfile
- .Configure
- .Evaluate
- RoadRage.Features.Vehicles.Traffic.Planning
- MainMenuFlowController
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- NetworkedLocalPlayerPoseReporter
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- RoadModelDocument
- LaneNode
- RoadModelCanonicalWriter
- SpeedConstraint
- RunEscapeMenuScreen
- ImportContext
- .Step
- NetworkedVehicleSeatIntent
- ReferenceTrack
- RoadRage.Features.Vehicles.Traffic.Migration
- LongitudinalDecision
- TrafficV2Insertion
- PathIssue
- NetworkedPlayerLifecycleService
- RoutePath
- NetworkedPassengerActionIntent
- CampaignTraceability
- SpeedPlan.cs
- V2FallbackReason
- NetworkedVehicleState.cs
- NetworkedPlayerLifecycleIntent
- TrafficV2Code
- FileLayout
- Func
- JunctionControlKind
- GateAEvidenceResult
- NetworkedPlayerState
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
- TrafficPerception
- DriverProfileDef
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 107 edges
2. `RoadId` - 102 edges
3. `RunFlowController` - 99 edges
4. `ImportContext` - 67 edges
5. `NetworkedVehicleState` - 67 edges
6. `AuthoredRoadModel` - 63 edges
7. `CompiledRoadModel` - 63 edges
8. `TrafficFrame` - 59 edges
9. `NetworkedVehicleDriverController` - 58 edges
10. `VehicleProfile` - 51 edges

## Surprising Connections (you probably didn't know these)
- `RoadRageBootstrap` --references--> `LobbyRosterService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyRosterService.cs
- `LobbyFlowController` --references--> `LobbyRosterService`  [EXTRACTED]
  Assets/RoadRage/App/Lobby/LobbyFlowController.cs → Assets/RoadRage/Features/Online/LobbyRosterService.cs
- `LobbyRosterService` --references--> `LobbyJoinService`  [EXTRACTED]
  Assets/RoadRage/Features/Online/LobbyRosterService.cs → Assets/RoadRage/Features/Online/LobbyJoinService.cs
- `LobbyRosterService` --references--> `LobbyRoomService`  [EXTRACTED]
  Assets/RoadRage/Features/Online/LobbyRosterService.cs → Assets/RoadRage/Features/Online/LobbyRoomService.cs
- `SegmentWalker` --references--> `RoadCurveSample`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/RoadCurveBuilder.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs

## Import Cycles
- None detected.

## Communities (187 total, 20 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.16
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 1 - ".Refine"
Cohesion: 0.07
Nodes (38): IList, List, RoadId, RoadModelValidationProfile, SweepGraph, SweepPose, ConflictSweep, LeafState (+30 more)

### Community 2 - "TrafficV2Composition.cs"
Cohesion: 0.12
Nodes (20): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+12 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.17
Nodes (13): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+5 more)

### Community 5 - ".Core"
Cohesion: 0.19
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (19): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+11 more)

### Community 7 - "RoadId"
Cohesion: 0.08
Nodes (30): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+22 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (16): TelemetrySample, Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip (+8 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.12
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 11 - "RunFlowController"
Cohesion: 0.06
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.09
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
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.08
Nodes (26): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+18 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 19 - "JunctionTraversal"
Cohesion: 0.11
Nodes (17): RoadId, JunctionApproach, Crossed, JunctionExitAssessment, JunctionMovementPosition, JunctionMovementStatus, Ahead, Behind (+9 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.15
Nodes (12): Rigidbody, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile, Profile (+4 more)

### Community 22 - ".CheckVisuals"
Cohesion: 0.13
Nodes (12): Surface, HashSet, IEnumerable, Renderer, StringBuilder, Transform, VisibleFaces, Component (+4 more)

### Community 23 - "AgentObservation"
Cohesion: 0.15
Nodes (19): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+11 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (33): Bounds, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, IReadOnlyList (+25 more)

### Community 25 - "JunctionClearance"
Cohesion: 0.19
Nodes (9): Collider, IReadOnlyList, List, Vector2, JunctionClearance, JunctionClearanceSurface, Surface, Colliders (+1 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.08
Nodes (27): IList, List, RoadId, RoadModelValidationProfile, SweepGraph, SweepPose, AutomatedPairDecisionManifest, AutomatedPairDecisionPlan (+19 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.14
Nodes (17): AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedConnection, ImportedPortal, ImportedSection, PublishedDisplacement (+9 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.12
Nodes (14): JunctionBatchCounters, JunctionGrantStatus, Denied, Granted, Held, Released, Revoked, JunctionRecord (+6 more)

### Community 30 - ".Measure"
Cohesion: 0.14
Nodes (15): BoxCollider, CompiledRoadModel, GameObject, RoadId, RoadModelValidationProfile, Scene, Vector3, VehiclePhysicsBody (+7 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (49): AppliedWidth, ConflictKind, Dictionary, IList, List, RoadId, AuthoringDecisions, AutomatedPairClassification (+41 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (34): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+26 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - "NpcReactionEffect"
Cohesion: 0.12
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
Cohesion: 0.14
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.09
Nodes (26): CompiledRoadModel, DriverProfile, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd (+18 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.15
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - "LongitudinalArbitration.cs"
Cohesion: 0.17
Nodes (13): IReadOnlyList, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason (+5 more)

### Community 45 - "PairSweep"
Cohesion: 0.28
Nodes (6): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "IPathGeometry"
Cohesion: 0.12
Nodes (13): Vector3, Vector3, IPathGeometry, LengthMeters, Spans, PlanningDecision, Motion, Observation (+5 more)

### Community 47 - "MotionPlan"
Cohesion: 0.15
Nodes (15): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+7 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.12
Nodes (16): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, Localized (+8 more)

### Community 49 - "PairReviewModel"
Cohesion: 0.09
Nodes (23): CompiledRoadModel, Dictionary, JunctionRecord, List, RoadBoundsBox, RoadId, StringBuilder, Document (+15 more)

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

### Community 55 - ".ImportLaneModule"
Cohesion: 0.24
Nodes (4): Dictionary, JunctionFeature, Predicate, ImportedJunction

### Community 56 - "CharacterCatalog"
Cohesion: 0.18
Nodes (8): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PlayerProfileBootstrapService, ScriptableObject

### Community 57 - "DefinitionId"
Cohesion: 0.14
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.22
Nodes (9): List, RoadCurve, RoadCurveSample, ImportedCurve, MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit (+1 more)

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.29
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference

### Community 60 - ".FullPath"
Cohesion: 0.21
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 61 - "LongitudinalCandidateKind"
Cohesion: 0.18
Nodes (11): LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable, Profile, SteeringCeilingUnreachable (+3 more)

### Community 62 - ".FixedUpdate"
Cohesion: 0.20
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 63 - "TrafficV2HazardCollector"
Cohesion: 0.07
Nodes (36): TrafficHazardCollectorCounters, Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState (+28 more)

### Community 64 - "PlayerMode"
Cohesion: 0.16
Nodes (8): CharacterController, PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 65 - "EffectiveLaneCorridor"
Cohesion: 0.15
Nodes (14): EffectiveLaneCorridor, LaneCorridor, RoadSurface, Asphalt, Concrete, Dirt, Gravel, VehicleClassMask (+6 more)

### Community 66 - "TrackPiece"
Cohesion: 0.19
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 67 - "LobbyRosterService"
Cohesion: 0.04
Nodes (39): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+31 more)

### Community 68 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 69 - "TrafficV2Admission"
Cohesion: 0.22
Nodes (9): GameObject, TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle, TrafficV2Verdict (+1 more)

### Community 70 - ".Compute"
Cohesion: 0.15
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "SpatialEntry"
Cohesion: 0.08
Nodes (28): Bounds, IReadOnlyList, RoadBoundsBox, RoadId, Vector3, ElementClosureInput, IntentInterval, OccupancyExclusion (+20 more)

### Community 73 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 74 - ".Assemble"
Cohesion: 0.24
Nodes (5): DrivabilityProfile, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter

### Community 75 - "SweepPose"
Cohesion: 0.22
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

### Community 79 - "PassengerActionVerdictCode"
Cohesion: 0.06
Nodes (34): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+26 more)

### Community 80 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

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
Cohesion: 0.14
Nodes (20): CompiledRoadModel, Dictionary, IReadOnlyList, JunctionRecord, JunctionSnapshot, List, RoadId, Grant (+12 more)

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
Cohesion: 0.08
Nodes (13): Difficulty, HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, Difficulty (+5 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.14
Nodes (19): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Portal, Section (+11 more)

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "PlayerProfile"
Cohesion: 0.10
Nodes (15): PersistentPlayerProfileRecord, PlayerNameValidator, PlayerProfile, CharacterId, DisplayName, PlayerProfileResolution, Error, IsResolved (+7 more)

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
Cohesion: 0.30
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - ".Track"
Cohesion: 0.22
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 102 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 103 - "DispositionKind"
Cohesion: 0.18
Nodes (11): DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint, Movement, MovementApproachPath (+3 more)

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
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.18
Nodes (11): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, RoadCurveSample (+3 more)

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
Cohesion: 0.29
Nodes (7): JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations, RouteEnd

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
Cohesion: 0.11
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 122 - ".Evaluate"
Cohesion: 0.13
Nodes (17): ProfilerMarker, PlanningSpine, CompiledRoadModel, RoadId, RoadLocation, DecisionCounter, RouteDiagnostic, None (+9 more)

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.17
Nodes (10): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing (+2 more)

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.06
Nodes (29): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, PairDecisionState, Confirmed, Missing, Orphan (+21 more)

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (18): Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3, PortalTrafficSpawner (+10 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.05
Nodes (31): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+23 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (21): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+13 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (10): Color, TMP_Text, LobbyPlayerSlotView, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen (+2 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.16
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.05
Nodes (48): CompiledConflictZone, Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile, Junction (+40 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.12
Nodes (20): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, RoadLimitValue, CapMetersPerSecond (+12 more)

### Community 136 - "LobbyJoinService"
Cohesion: 0.14
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (46): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference (+38 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 141 - "RoadModelDocument"
Cohesion: 0.06
Nodes (48): Comparison, Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto (+40 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.21
Nodes (8): JunctionMovement, BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalPayload, RoadModelCanonicalWriter, ConflictZone

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
Cohesion: 0.11
Nodes (15): DriverProfile, IReadOnlyList, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord (+7 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 153 - "ReferenceTrack"
Cohesion: 0.11
Nodes (21): Quaternion, RoadKinematicAnchor, Vector3, BodyState, GaugeBox, Rho, InterStepResult, BoundMeters (+13 more)

### Community 155 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.17
Nodes (7): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, RoadRage.Features.Vehicles.Traffic.Migration

### Community 156 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.10
Nodes (23): CompiledRoadModel, DriverProfile, Portal, Quaternion, RoadId, RoadLocation, Vector3, ScenarioInsertion (+15 more)

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
Cohesion: 0.07
Nodes (24): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+16 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.07
Nodes (25): CompiledRoadModel, Dictionary, HashSet, IReadOnlyDictionary, IReadOnlyList, List, RoadId, CampaignTraceability (+17 more)

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

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 178 - "GateAEvidenceResult"
Cohesion: 0.18
Nodes (11): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+3 more)

### Community 180 - "NetworkedPlayerState"
Cohesion: 0.21
Nodes (8): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 183 - "LaneGraphRouting"
Cohesion: 0.24
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "ModelDto"
Cohesion: 0.11
Nodes (16): AdjacencyDto, DrivabilityProfile, RoadModelCompiler, ModelDto, RoadLocalizationProfile, ConnectionDto, ControlDto, CorridorDto (+8 more)

### Community 191 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (18): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+10 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.14
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (13): CharacterController, Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph (+5 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.12
Nodes (18): Vector3, ObstacleFact, InSweptPath, VehicleGapFact, Bounds, Comparison, CompiledRoadModel, IReadOnlyList (+10 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.36
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1127 isolated node(s):** `ProvenDisjoint`, `Witness`, `Unresolved`, `BudgetExhausted`, `Conservative` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1600 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **20 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `Blocker`, `.Step`, `AgentObservation`, `ReferenceTrack`, `LongitudinalDecision`, `TrafficV2Insertion`, `RoutePlan`, `NetworkedPassengerActionIntent`, `CampaignTraceability`, `V2FallbackReason`, `JunctionActorReport`, `LongitudinalCandidateKind`, `TrafficV2HazardCollector`, `TrafficV2Admission`, `TrafficDecisionProjection`, `SpatialEntry`, `JunctionConflictIndex`, `TrackingTolerance`, `RoadRage.Features.Vehicles.Traffic.Planning`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.205) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2Composition.cs`, `TrafficV2StepRunner`, `TrafficV2Admission`, `RageRoadEventFlowController`, `MonoBehaviour`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `.Step`, `LaneGraph`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.154) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `EffectiveLaneCorridor`, `RoadModelRecords.cs`, `RoadModelDocument`, `RoadGeometryValidator`, `.TryGetMovement`, `RoadModelCanonicalWriter`, `TrafficV2HazardCollector`, `Blocker`, `RoadModelValidationIssue`, `RoadLineage`, `TrafficV2Insertion`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.143) - this node is a cross-community bridge._
- **What connects `ProvenDisjoint`, `Witness`, `Unresolved` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.07067603160667252 - nodes in this community are weakly interconnected._
- **Should `TrafficV2Composition.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.1225296442687747 - nodes in this community are weakly interconnected._
- **Should `GateAEvidenceParameters` be split into smaller, more focused modules?**
  _Cohesion score 0.10256410256410256 - nodes in this community are weakly interconnected._