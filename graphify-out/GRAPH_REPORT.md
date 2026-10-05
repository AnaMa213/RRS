# Graph Report - RRS  (2026-10-05)

## Corpus Check
- 174 files · ~233,536 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4405 nodes · 10222 edges · 180 communities (173 shown, 7 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 445 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5a1a6c90`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .Refine
- .HandleRosterChanged
- TrafficV2Composition.cs
- GreyboxAssetSeedMetadata
- SweepPose
- .Core
- GateAEvidenceParameters
- RoadId
- TireSample
- VehicleSuspensionModel
- PairReviewModel
- RunFlowController
- RunCheckpointHudScreen
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- .Localize
- LocalOnFootController
- Blocker
- NpcReactionEffect
- LobbyJoinOutcome
- MigrationReport
- VehiclePhysicsBody
- .Add
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
- LobbyRosterScreen
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
- ImportContext
- LongitudinalArbitration.cs
- .Draw
- MovementRole
- PairSweep
- .FullPath
- AIVehicleBehaviorDebugView
- .FixedUpdate
- TrafficV2HazardCollector
- NetworkedAIVehicleState
- .SpawnForClient
- .Evaluate
- LobbyRosterService
- .Build
- InterStepResult
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- RageTuningDef
- .Assemble
- TrafficV2Admission
- UserNotice
- .CheckVisuals
- TrafficLongitudinalOutcome
- PassengerActionVerdictCode
- PairReview
- RageStateDebugView
- KinematicOffsetBounds
- RageThreshold
- JunctionCoordinator
- RageTuningCatalog
- NetworkedPlayerState
- StopHoldState
- LobbyFlowController
- RoadModelValidationIssue
- LongitudinalCandidateKind
- .Track
- RoadLineage
- DefinitionId
- V1Node
- JunctionConflictIndex
- .PrepareStep
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
- MainMenuScreen
- JunctionDistances
- SpatialQueryBuffer
- RoadGeometryValidator
- NetworkedBossState.cs
- NetworkedVehicleState
- HazardRootClass
- MatchSettings
- JunctionTraversal
- .ApplyHonkTarget
- NetworkedCrewEconomyState.cs
- TrafficSettingsDef
- JunctionMovementStatus
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
- .Awake
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- TrafficV2WorkCounters.cs
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- IHostOwnedRuntimeState
- RoadModelDocument
- LaneNode
- RoadModelCanonicalWriter
- SpeedConstraint
- JunctionGrantStatus
- .BuildSmoothCurve
- .Step
- NetworkedVehicleSeatIntent
- TrafficV2Settings
- ReferenceTrack
- TrackPiece
- RoadRage.Features.Vehicles.Traffic.Migration
- LongitudinalDecision
- TrafficV2Insertion
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
- .Compile
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

## Communities (180 total, 7 thin omitted)

### Community 0 - ".Refine"
Cohesion: 0.11
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 1 - ".HandleRosterChanged"
Cohesion: 0.13
Nodes (10): Difficulty, Difficulty, LobbyRosterEntry, DisplayName, PortraitTint, Ready, Difficulty, Easy (+2 more)

### Community 2 - "TrafficV2Composition.cs"
Cohesion: 0.12
Nodes (21): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+13 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.18
Nodes (15): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+7 more)

### Community 5 - ".Core"
Cohesion: 0.19
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.09
Nodes (18): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+10 more)

### Community 7 - "RoadId"
Cohesion: 0.07
Nodes (31): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+23 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "PairReviewModel"
Cohesion: 0.23
Nodes (10): CompiledRoadModel, List, RoadId, StringBuilder, PairReviewModel, PairReviewStatus, Modified, New (+2 more)

### Community 11 - "RunFlowController"
Cohesion: 0.06
Nodes (10): Camera, CharacterController, Collider, GameObject, HashSet, Quaternion, Transform, Vector3 (+2 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.11
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
Cohesion: 0.18
Nodes (10): NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel, Both, Fear (+2 more)

### Community 19 - "LobbyJoinOutcome"
Cohesion: 0.12
Nodes (16): Task, Task, LobbyCreateOutcome, LobbyId, Success, LobbyJoinFailureReason, Expired, Failed (+8 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - ".Add"
Cohesion: 0.17
Nodes (14): Bounds, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation, RoadModelValidationProfile (+6 more)

### Community 23 - "AgentObservation"
Cohesion: 0.14
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.09
Nodes (19): CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, List, RoadCurveSample (+11 more)

### Community 25 - "JunctionClearance"
Cohesion: 0.16
Nodes (11): Collider, IReadOnlyList, List, Transform, Vector2, JunctionClearance, JunctionClearanceSurface, Surface (+3 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (21): CompiledRoadModel, Dictionary, HashSet, IList, List, MenuItem, RoadId, RoadModelValidationProfile (+13 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.12
Nodes (25): Dictionary, IReadOnlyList, KeyValuePair, RoadCurve, RoadRecordKind, AuthoringTask, DisplacementKind, PortalBoundaryTrim (+17 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.18
Nodes (9): RoadId, JunctionBatchCounters, JunctionRecord, IsEffectiveGrant, JunctionSnapshot, Counters, EffectiveFrame, Records (+1 more)

### Community 30 - ".Measure"
Cohesion: 0.14
Nodes (15): BoxCollider, CompiledRoadModel, GameObject, RoadId, RoadModelValidationProfile, Scene, Vector3, VehiclePhysicsBody (+7 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (48): AppliedWidth, ConflictKind, Dictionary, FileLayout, Func, IList, JunctionControlKind, List (+40 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (31): CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample, RoadElementKind (+23 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (23): MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing, GateAEvidenceStale, HorizonNonConforming (+15 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.10
Nodes (10): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Transform (+2 more)

### Community 35 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

### Community 36 - "RoutePlan"
Cohesion: 0.06
Nodes (36): IReadOnlyList, RoadModelVersion, RouteDiagnostic, None, ZeroWeightFallback, RouteOutcome, InvalidInput, NoRoute (+28 more)

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
Nodes (32): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd, ExitPortal (+24 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.15
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.15
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 45 - "ConflictSweep"
Cohesion: 0.13
Nodes (15): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, Vector2, KinematicPoseSet (+7 more)

### Community 46 - "IPathGeometry"
Cohesion: 0.12
Nodes (13): Vector3, Vector3, IPathGeometry, LengthMeters, Spans, PlanningDecision, Motion, Observation (+5 more)

### Community 47 - "MotionPlan"
Cohesion: 0.15
Nodes (15): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+7 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.11
Nodes (16): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, Localized (+8 more)

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
Cohesion: 0.19
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 54 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 55 - "ImportContext"
Cohesion: 0.18
Nodes (7): JunctionFeature, List, Predicate, RoadCurveSample, ImportContext, ImportedJunction, CircleFit

### Community 56 - "LongitudinalArbitration.cs"
Cohesion: 0.17
Nodes (13): IReadOnlyList, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason (+5 more)

### Community 57 - ".Draw"
Cohesion: 0.20
Nodes (6): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 58 - "MovementRole"
Cohesion: 0.40
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 59 - "PairSweep"
Cohesion: 0.24
Nodes (7): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate, RoadModelValidationProfile

### Community 60 - ".FullPath"
Cohesion: 0.20
Nodes (6): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func

### Community 61 - "AIVehicleBehaviorDebugView"
Cohesion: 0.18
Nodes (7): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, IRageDispositionSource, CurrentDisposition, TextMeshPro

### Community 62 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 63 - "TrafficV2HazardCollector"
Cohesion: 0.09
Nodes (29): TrafficHazardCollectorCounters, Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState (+21 more)

### Community 64 - "NetworkedAIVehicleState"
Cohesion: 0.41
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 65 - ".SpawnForClient"
Cohesion: 0.15
Nodes (8): NetworkObject, Transform, Vector3, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 66 - ".Evaluate"
Cohesion: 0.14
Nodes (13): LongitudinalBounds, Valid, DriverProfile, PlanningReach, IReadOnlyList, PlanningRequest, CompiledRoadModel, RoadId (+5 more)

### Community 67 - "LobbyRosterService"
Cohesion: 0.06
Nodes (22): Difficulty, FacepunchSteamLobbyPlatform, Difficulty, ISteamLobbyPlatform, LobbyMemberSnapshot, CharacterId, DisplayName, Ready (+14 more)

### Community 68 - ".Build"
Cohesion: 0.25
Nodes (9): JunctionMovementPosition, ConditionalWeakTable, DriverProfile, IReadOnlyList, RoadId, Vector3, JunctionRequestBuilder, RoadElementKind (+1 more)

### Community 69 - "InterStepResult"
Cohesion: 0.17
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 70 - ".Compute"
Cohesion: 0.19
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.05
Nodes (34): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+26 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.12
Nodes (20): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+12 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.14
Nodes (12): RageTuningDef, FearSensitivity, HonkChannel, HonkMagnitude, HonkRange, Id, MaxFearValue, MaxRageValue (+4 more)

### Community 74 - ".Assemble"
Cohesion: 0.18
Nodes (8): DrivabilityProfile, IList, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter, ImportManifestEntry

### Community 75 - "TrafficV2Admission"
Cohesion: 0.22
Nodes (9): GameObject, TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle, TrafficV2Verdict (+1 more)

### Community 76 - "UserNotice"
Cohesion: 0.10
Nodes (13): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+5 more)

### Community 77 - ".CheckVisuals"
Cohesion: 0.15
Nodes (11): Surface, HashSet, IEnumerable, Renderer, StringBuilder, VisibleFaces, Component, Mesh (+3 more)

### Community 78 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - "PassengerActionVerdictCode"
Cohesion: 0.13
Nodes (15): PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive, ActorNotPassenger, CooldownActive, InvalidAction, InvalidCatalog (+7 more)

### Community 80 - "PairReview"
Cohesion: 0.26
Nodes (3): Dictionary, RoadBoundsBox, PairReview

### Community 81 - "RageStateDebugView"
Cohesion: 0.24
Nodes (3): TMP_Text, RageStateDebugView, ContextMenu

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+9 more)

### Community 83 - "RageThreshold"
Cohesion: 0.67
Nodes (3): RageThreshold, Disposition, MinValue

### Community 84 - "JunctionCoordinator"
Cohesion: 0.14
Nodes (20): CompiledRoadModel, Dictionary, IReadOnlyList, JunctionRecord, JunctionSnapshot, List, RoadId, Grant (+12 more)

### Community 85 - "RageTuningCatalog"
Cohesion: 0.25
Nodes (4): List, RageTuningCatalog, Count, ScriptableObject

### Community 86 - "NetworkedPlayerState"
Cohesion: 0.14
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 87 - "StopHoldState"
Cohesion: 0.18
Nodes (7): StopHoldPhase, Entered, Holding, None, Released, StopHoldState, Active

### Community 88 - "LobbyFlowController"
Cohesion: 0.11
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (18): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+10 more)

### Community 90 - "LongitudinalCandidateKind"
Cohesion: 0.18
Nodes (11): LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable, Profile, SteeringCeilingUnreachable (+3 more)

### Community 91 - ".Track"
Cohesion: 0.33
Nodes (4): DrivabilityProfile, DriverProfile, RoadCurve, Vector3

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "DefinitionId"
Cohesion: 0.05
Nodes (32): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, Color, GameObject (+24 more)

### Community 94 - "V1Node"
Cohesion: 0.10
Nodes (29): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+21 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.15
Nodes (12): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, Portal, RoadBoundsBox, RoadId, Vector3 (+4 more)

### Community 96 - ".PrepareStep"
Cohesion: 0.17
Nodes (6): TrackingToleranceResponse, Latched, LatchedAtStep, ProfilerMarker, TrackingTolerance, Undeclared

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.11
Nodes (17): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+9 more)

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
Cohesion: 0.08
Nodes (19): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, IReadOnlyList, RageRoadEventLifecycle, RageDisposition, Block (+11 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.07
Nodes (22): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+14 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "SpatialQueryBuffer"
Cohesion: 0.28
Nodes (6): Bounds, SpatialQueryBuffer, Capacity, Count, Saturated, Total

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
Cohesion: 0.14
Nodes (12): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer, HazardRootClass, Obstacle, Self (+4 more)

### Community 114 - "MatchSettings"
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 115 - "JunctionTraversal"
Cohesion: 0.11
Nodes (17): JunctionApproach, Crossed, JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant (+9 more)

### Community 117 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "JunctionMovementStatus"
Cohesion: 0.33
Nodes (5): JunctionMovementStatus, Ahead, Behind, Occupied, Unknown

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 122 - "StopHoldRelease"
Cohesion: 0.33
Nodes (6): StopHoldRelease, GapOpened, GrantEffective, None, SourceDeparted, SourceGone

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.16
Nodes (10): PlanningTolerances, ProfilerMarker, PlanningSpine, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing (+2 more)

### Community 124 - "LongitudinalCandidate"
Cohesion: 0.50
Nodes (4): RoadId, JunctionEntryInput, Active, LongitudinalCandidate

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (50): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, Rigidbody, Stopwatch (+42 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.15
Nodes (8): PairReviewActions, PairReviewEntry, IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "MonoBehaviour"
Cohesion: 0.16
Nodes (11): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkedPlayerSpawnService, Transform, RunCompositionRoot (+3 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (21): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+13 more)

### Community 131 - ".Awake"
Cohesion: 0.12
Nodes (4): Button, TMP_Text, LobbyShellScreen, TMP_InputField

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (77): CompiledConflictZone, JunctionMovement, RoadModelCanonicalPayload, Vector3, ConflictKind, Crossing, Merge, ConflictZone (+69 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.08
Nodes (35): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+27 more)

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
Cohesion: 0.06
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

### Community 147 - "JunctionGrantStatus"
Cohesion: 0.33
Nodes (6): JunctionGrantStatus, Denied, Granted, Held, Released, Revoked

### Community 148 - ".BuildSmoothCurve"
Cohesion: 0.23
Nodes (4): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit

### Community 149 - ".Step"
Cohesion: 0.12
Nodes (15): DriverProfile, IReadOnlyList, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord (+7 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 153 - "ReferenceTrack"
Cohesion: 0.15
Nodes (14): Quaternion, RoadKinematicAnchor, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound (+6 more)

### Community 154 - "TrackPiece"
Cohesion: 0.17
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 155 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.17
Nodes (7): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, RoadRage.Features.Vehicles.Traffic.Migration

### Community 156 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.12
Nodes (18): CompiledRoadModel, DriverProfile, Portal, Quaternion, RoadId, RoadLocation, Vector3, TrafficV2Insertion (+10 more)

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.05
Nodes (41): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+33 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.08
Nodes (22): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements, ElementStatus, Measured (+14 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (33): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+25 more)

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
Cohesion: 0.20
Nodes (7): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, HostOwnedNetworkStateBehaviour, IsHostAuthority, NetworkBehaviour

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - ".Compile"
Cohesion: 0.09
Nodes (20): AdjacencyDto, DrivabilityProfile, RoadModelVersion, High, IsEmpty, Low, SchemaVersion, Comparison (+12 more)

### Community 191 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (19): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+11 more)

### Community 194 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.21
Nodes (7): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.20
Nodes (10): Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance, PerceptionLimits, TrafficPerception (+2 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.36
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1127 isolated node(s):** `RoadRage.App`, `Instance`, `Router`, `Notices`, `Profiles` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1583 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `Blocker`, `.Step`, `AgentObservation`, `ReferenceTrack`, `LongitudinalDecision`, `TrafficV2Insertion`, `RoutePlan`, `V2FallbackReason`, `JunctionActorReport`, `NetworkedPlayerReviveIntent`, `TrafficV2HazardCollector`, `InterStepResult`, `TrafficDecisionProjection`, `TrafficV2Admission`, `LongitudinalCandidateKind`, `.PrepareStep`, `SpatialQueryBuffer`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.255) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `MonoBehaviour`, `RoadRage.Shared.Domain`, `TrafficV2Composition.cs`, `TrafficV2StepRunner`, `NetworkedPassengerActionIntent`, `TrafficV2Admission`, `TrafficV2Code`, `.Step`, `TrafficSettingsDef`, `LaneGraph`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.169) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `.Evaluate`, `RoadModelRecords.cs`, `RoadModelDocument`, `RoadGeometryValidator`, `.Localize`, `RoadLineage`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `PortalTrafficSpawner`, `TrafficV2Insertion`, `TrafficV2HazardCollector`?**
  _High betweenness centrality (0.113) - this node is a cross-community bridge._
- **What connects `RoadRage.App`, `Instance`, `Router` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.1111111111111111 - nodes in this community are weakly interconnected._
- **Should `.HandleRosterChanged` be split into smaller, more focused modules?**
  _Cohesion score 0.13071895424836602 - nodes in this community are weakly interconnected._
- **Should `TrafficV2Composition.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.11956521739130435 - nodes in this community are weakly interconnected._