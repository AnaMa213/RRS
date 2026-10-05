# Graph Report - RRS  (2026-10-05)

## Corpus Check
- 174 files · ~233,301 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4403 nodes · 10217 edges · 186 communities (177 shown, 7 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 445 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `d23ae105`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .Refine
- DefinitionId
- MeasurementRun
- GreyboxAssetSeedMetadata
- SweepPose
- MainMenuScreen
- GateAEvidenceParameters
- RoadId
- TireSample
- VehicleSuspensionModel
- PairReviewStatus
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- .Localize
- LocalOnFootController
- Blocker
- NpcReactionEffect
- RoadModelSource
- MigrationReport
- VehiclePhysicsBody
- PairReviewEntry
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
- PerceivedObstacleKind
- RoutePlan
- NetworkedVehicleSeatService
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- ImportedCurve
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
- .Add
- LongitudinalArbitration.cs
- .Draw
- .AddMovement
- PairSweep
- .FullPath
- AIVehicleBehaviorDebugView
- .FixedUpdate
- TrafficV2HazardCollector
- NetworkedAIVehicleState
- NetworkedRunSessionMonitor
- .Collect
- LobbyRosterService
- .Build
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
- PairReviewModel
- RunEscapeMenuScreen
- KinematicOffsetBounds
- RageDisposition
- JunctionCoordinator
- JunctionClearanceResult
- NetworkedPlayerState
- StopHoldPhase
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
- PairDecisionState
- .Create
- DispositionKind
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- PlayerProfileStore
- RoadGeometryValidator
- NetworkedBossState.cs
- NetworkedVehicleState
- HazardRootClass
- MatchSettings
- JunctionRecords.cs
- .ApplyHonkTarget
- NetworkedCrewEconomyState.cs
- TrafficSettingsDef
- TrafficHazardKind
- DriverProfile
- LeafState
- LongitudinalMemory
- RoadRage.Features.Vehicles.Traffic.Planning
- PassengerActionIntent
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- MonoBehaviour
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- .CacheComponents
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- RoadModelVersion
- PlanningDecision
- RoadModelDocument
- PathIssue
- LaneNode
- .IsSurfaceOnlyCollision
- RoadModelCanonicalWriter
- SpeedConstraint
- JunctionGrantStatus
- ImportContext
- .Step
- NetworkedVehicleSeatIntent
- TrafficV2Settings
- ReferenceTrack
- TrackPiece
- RoadRage.Features.Vehicles.Traffic.Migration
- LongitudinalDecision
- TrafficV2Insertion
- PerceptionUnavailableReason
- NetworkedPlayerLifecycleService
- PlayerNameValidator
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
- NetworkedVehicleRecoveryIntent
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

## Communities (186 total, 7 thin omitted)

### Community 0 - ".Refine"
Cohesion: 0.12
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 1 - "DefinitionId"
Cohesion: 0.14
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 2 - "MeasurementRun"
Cohesion: 0.11
Nodes (19): IReadOnlyList, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label, Triplets (+11 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.17
Nodes (15): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+7 more)

### Community 5 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - "RoadId"
Cohesion: 0.08
Nodes (32): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+24 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "PairReviewStatus"
Cohesion: 0.33
Nodes (5): PairReviewStatus, Modified, New, Removed, Unchanged

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

### Community 15 - ".Localize"
Cohesion: 0.12
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 19 - "RoadModelSource"
Cohesion: 0.14
Nodes (12): DrivabilityProfile, JunctionMovement, RoadModelCanonicalPayload, Comparison, RoadModelCompiler, BindingDto, RoadModelProvenance, ZoneTypeDto (+4 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 23 - "AgentObservation"
Cohesion: 0.13
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (34): Bounds, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, IReadOnlyList (+26 more)

### Community 25 - "JunctionClearance"
Cohesion: 0.21
Nodes (6): IReadOnlyList, RoadModelValidationProfile, Vector2, JunctionClearance, Vector2, PoseGrid

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.10
Nodes (21): ConflictDecision, CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, RoadModelValidationProfile (+13 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.14
Nodes (16): AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedConnection, ImportedPortal, PublishedDisplacement, SourceDisposition (+8 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.15
Nodes (9): RoadId, JunctionBatchCounters, JunctionRecord, IsEffectiveGrant, JunctionSnapshot, Counters, EffectiveFrame, Records (+1 more)

### Community 30 - ".Measure"
Cohesion: 0.14
Nodes (17): Surface, BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, Scene (+9 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (46): ConflictKind, Dictionary, FileLayout, Func, IList, JunctionControlKind, List, RoadBoundsBox (+38 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (33): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+25 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (23): MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing, GateAEvidenceStale, HorizonNonConforming (+15 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.15
Nodes (8): DevIndestructibleVehicle, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController

### Community 35 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

### Community 36 - "RoutePlan"
Cohesion: 0.05
Nodes (58): CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation, RoadModelVersion, DecisionCounter, RouteDiagnostic, None (+50 more)

### Community 37 - "NetworkedVehicleSeatService"
Cohesion: 0.18
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.14
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.08
Nodes (28): CompiledRoadModel, DrivabilityProfile, DriverProfile, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId (+20 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.15
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - "ImportedCurve"
Cohesion: 0.23
Nodes (11): Dictionary, IReadOnlyList, KeyValuePair, List, RoadCurve, RoadCurveSample, RoadRecordKind, ImportedCurve (+3 more)

### Community 45 - "ConflictSweep"
Cohesion: 0.14
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 46 - "IPathGeometry"
Cohesion: 0.21
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 47 - "MotionPlan"
Cohesion: 0.11
Nodes (21): VehicleCoverage, DrivabilityProfile, IReadOnlyList, LongitudinalBounds, Valid, MotionPlan, Diagnostics, Evidence (+13 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.09
Nodes (22): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, Localized (+14 more)

### Community 49 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (13): JunctionRecord, RoadCurveSample, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord (+5 more)

### Community 50 - ".Decide"
Cohesion: 0.28
Nodes (5): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, StopHoldParameters

### Community 51 - "TrafficDriveOutcome"
Cohesion: 0.12
Nodes (15): TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback, FallbackReason (+7 more)

### Community 52 - ".Regenerate"
Cohesion: 0.15
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "VehicleDriveIntent"
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 54 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 55 - ".Add"
Cohesion: 0.25
Nodes (3): JunctionFeature, Predicate, ImportedJunction

### Community 56 - "LongitudinalArbitration.cs"
Cohesion: 0.20
Nodes (13): IReadOnlyList, RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader (+5 more)

### Community 57 - ".Draw"
Cohesion: 0.27
Nodes (6): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 58 - ".AddMovement"
Cohesion: 0.21
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 59 - "PairSweep"
Cohesion: 0.21
Nodes (7): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate, RoadModelValidationProfile

### Community 60 - ".FullPath"
Cohesion: 0.18
Nodes (7): Action, KeyValuePair, MenuItem, Scene, MenuItem, MenuItem, Func

### Community 61 - "AIVehicleBehaviorDebugView"
Cohesion: 0.24
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 62 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 63 - "TrafficV2HazardCollector"
Cohesion: 0.12
Nodes (16): Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity, Counters (+8 more)

### Community 64 - "NetworkedAIVehicleState"
Cohesion: 0.44
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 65 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 66 - ".Collect"
Cohesion: 0.16
Nodes (13): TrafficHazardCollectorCounters, Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3 (+5 more)

### Community 67 - "LobbyRosterService"
Cohesion: 0.04
Nodes (39): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+31 more)

### Community 68 - ".Build"
Cohesion: 0.26
Nodes (8): ConditionalWeakTable, DriverProfile, IReadOnlyList, RoadId, Vector3, JunctionRequestBuilder, RoadElementKind, RouteOccurrence

### Community 69 - "InterStepResult"
Cohesion: 0.17
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 70 - ".Compute"
Cohesion: 0.13
Nodes (11): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+3 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.04
Nodes (46): IReadOnlyList, RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind (+38 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.09
Nodes (24): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementOccupant, IntentInterval (+16 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.10
Nodes (15): List, RageTuningCatalog, Count, RageTuningDef, FearSensitivity, HonkChannel, HonkMagnitude, HonkRange (+7 more)

### Community 74 - ".Assemble"
Cohesion: 0.16
Nodes (8): DrivabilityProfile, IList, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter, ImportManifestEntry

### Community 75 - "CharacterCatalog"
Cohesion: 0.21
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - ".FingerprintWithInputs"
Cohesion: 0.17
Nodes (8): IEnumerable, Renderer, StringBuilder, Transform, Component, Mesh, MeshFilter, VisibleFaces

### Community 79 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+19 more)

### Community 80 - "PairReviewModel"
Cohesion: 0.23
Nodes (6): CompiledRoadModel, List, RoadBoundsBox, StringBuilder, PairReview, PairReviewModel

### Community 81 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.10
Nodes (18): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+10 more)

### Community 83 - "RageDisposition"
Cohesion: 0.14
Nodes (12): RageThreshold, Disposition, MinValue, IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm (+4 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.14
Nodes (20): CompiledRoadModel, Dictionary, IReadOnlyList, JunctionRecord, JunctionSnapshot, List, RoadId, Grant (+12 more)

### Community 85 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 86 - "NetworkedPlayerState"
Cohesion: 0.14
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 87 - "StopHoldPhase"
Cohesion: 0.40
Nodes (5): StopHoldPhase, Entered, Holding, None, Released

### Community 88 - "LobbyFlowController"
Cohesion: 0.08
Nodes (13): Difficulty, HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, Difficulty (+5 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.14
Nodes (20): RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor, Movement, Portal (+12 more)

### Community 90 - "LongitudinalCandidateKind"
Cohesion: 0.22
Nodes (9): LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable, Profile, SteeringCeilingUnreachable (+1 more)

### Community 91 - ".Track"
Cohesion: 0.24
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "PlayerProfile"
Cohesion: 0.13
Nodes (15): PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, PlayerProfileBootstrapService, PlayerProfileResolution, Error, IsResolved (+7 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (29): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+21 more)

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
Nodes (16): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+8 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.12
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.30
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "PairDecisionState"
Cohesion: 0.33
Nodes (6): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed

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

### Community 109 - "PlayerProfileStore"
Cohesion: 0.22
Nodes (5): PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

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

### Community 115 - "JunctionRecords.cs"
Cohesion: 0.11
Nodes (17): JunctionApproach, Crossed, JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant (+9 more)

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
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 122 - "LongitudinalMemory"
Cohesion: 0.22
Nodes (8): LongitudinalMemory, None, StopHoldRelease, GapOpened, GrantEffective, None, SourceDeparted, SourceGone

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.15
Nodes (10): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing (+2 more)

### Community 124 - "PassengerActionIntent"
Cohesion: 0.25
Nodes (5): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, INetworkSerializable

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.17
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

### Community 128 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (21): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+13 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.07
Nodes (6): Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, TMP_InputField

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.18
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (69): Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile, ImportManifest, ImportManifestEntry (+61 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.08
Nodes (35): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+27 more)

### Community 135 - ".CacheComponents"
Cohesion: 0.29
Nodes (3): BoxCollider, NetworkTransform, Rigidbody

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (46): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference (+38 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 140 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 141 - "RoadModelDocument"
Cohesion: 0.08
Nodes (32): Func, AdjacencyDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto, GroupDto (+24 more)

### Community 142 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - "JunctionGrantStatus"
Cohesion: 0.33
Nodes (6): JunctionGrantStatus, Denied, Granted, Held, Released, Revoked

### Community 148 - "ImportContext"
Cohesion: 0.23
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 149 - ".Step"
Cohesion: 0.11
Nodes (15): DriverProfile, IReadOnlyList, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord (+7 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 153 - "ReferenceTrack"
Cohesion: 0.14
Nodes (14): Quaternion, RoadKinematicAnchor, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound (+6 more)

### Community 154 - "TrackPiece"
Cohesion: 0.18
Nodes (10): RoadCurve, RoadElementKind, RoadId, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 155 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.17
Nodes (7): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, RoadRage.Features.Vehicles.Traffic.Migration

### Community 156 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.08
Nodes (30): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+22 more)

### Community 158 - "PerceptionUnavailableReason"
Cohesion: 0.40
Nodes (5): PerceptionUnavailableReason, ChannelSaturated, ChannelUnavailable, HazardCollectorSaturated, None

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.08
Nodes (26): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+18 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.09
Nodes (21): Dictionary, HashSet, IReadOnlyDictionary, CampaignTraceability, Elements, ElementStatus, Measured, NotMeasured (+13 more)

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
Cohesion: 0.07
Nodes (18): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+10 more)

### Community 194 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.22
Nodes (10): Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance, PerceptionLimits, TrafficPerception (+2 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.31
Nodes (5): DriverProfileDef, Id, Profile, RawId, ScriptableObject

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.12
Nodes (9): CharacterController, Collider, IReadOnlyList, List, Quaternion, RaycastHit, Vector3, NetworkedAIVehicleDriverController (+1 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1127 isolated node(s):** `RoadRage.App`, `Instance`, `Router`, `Notices`, `Profiles` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1583 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `Blocker`, `.Step`, `AgentObservation`, `ReferenceTrack`, `LongitudinalDecision`, `TrafficV2Insertion`, `RoutePlan`, `NetworkedPassengerActionIntent`, `V2FallbackReason`, `JunctionActorReport`, `.Collect`, `InterStepResult`, `TrafficDecisionProjection`, `ElementOccupant`, `TrackingTolerance`, `LongitudinalMemory`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.247) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `MonoBehaviour`, `RoadRage.Shared.Domain`, `MeasurementRun`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `.Step`, `LaneGraph`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.170) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `RoadModelRecords.cs`, `RoadModelDocument`, `RoadGeometryValidator`, `MotionPlan`, `.Localize`, `RoadModelCanonicalWriter`, `RoadLineage`, `RoadModelSource`, `Blocker`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `PortalTrafficSpawner`, `TrafficV2Insertion`, `TrafficV2HazardCollector`?**
  _High betweenness centrality (0.117) - this node is a cross-community bridge._
- **What connects `RoadRage.App`, `Instance`, `Router` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.11596638655462185 - nodes in this community are weakly interconnected._
- **Should `DefinitionId` be split into smaller, more focused modules?**
  _Cohesion score 0.13725490196078433 - nodes in this community are weakly interconnected._
- **Should `MeasurementRun` be split into smaller, more focused modules?**
  _Cohesion score 0.11428571428571428 - nodes in this community are weakly interconnected._