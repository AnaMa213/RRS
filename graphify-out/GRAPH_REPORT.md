# Graph Report - RRS  (2026-10-06)

## Corpus Check
- 178 files · ~242,976 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4531 nodes · 10549 edges · 182 communities (175 shown, 6 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 457 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a3c1faac`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedVehicleSeatService
- .Refine
- RoadRage.Features.Vehicles.Traffic.Migration
- MonoBehaviour
- SweepElement
- RoutePlan
- GateAEvidenceParameters
- .Localize
- TireSample
- VehicleSuspensionModel
- NetworkedPlayerSpawnService
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
- .CreatePlan
- AgentObservation
- AuthoredRoadModel
- NpcReactionEffect
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1ImportResult
- DefinitionId
- RoadId
- AuthoringDecisions
- TrafficFrame
- MotionPlan
- NetworkedVehicleDriverController
- JunctionClearance
- CharacterCatalog
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- .Measure
- PairSweep
- MeasurementRun
- ReferenceTrack
- JunctionRecords.cs
- .FullPath
- AuthoredRun
- RoadRage.Features.Vehicles
- .Regenerate
- ElementTrace
- .Add
- .AddMovement
- TrafficDriveOutcome
- PlayerProfile
- .RenderBody
- NetworkedAIVehicleState
- PairReviewModel
- ConflictDecisionKind
- AIVehicleBehaviorDebugView
- SweepPose
- .Decide
- .FixedUpdate
- TrackPiece
- LobbyJoinOutcome
- TrafficJunctionOutcome
- PassengerActionVerdictCode
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- .Draw
- TrafficV2HazardCollector
- HazardRootClass
- UserNotice
- LocalVoidRespawnController
- TrafficLongitudinalOutcome
- .CheckVisuals
- TrackingTolerance
- PlayerMode
- KinematicOffsetBounds
- TrackingMeasurement.cs
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- .RenderSignoff
- LobbyFlowController
- RoadModelValidationIssue
- RageDisposition
- TaskDispositionKind
- RoadLineage
- .Measure
- V1Node
- JunctionConflictIndex
- AutomatedPairClassification
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- RoadRage.Shared.Definitions
- NetworkedRageState
- JunctionClearanceResult
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MainMenuScreen
- JunctionDistances
- .FingerprintWithInputs
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- DriverProfileDef
- RoadModelVersion
- ImportedCurve
- .MergeGapAdmits
- LongitudinalDecision
- JunctionTraversal
- TrafficSettingsDef
- .Configure
- DriverProfile
- StatusFilter
- PlanningReach
- .Step
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- NetworkedPlayerState
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- FileLayout
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- .Track
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- MovementRole
- RoadModelCanonicalWriter
- SpeedConstraint
- PathIssue
- ImportContext
- AppSceneRouter.cs
- NetworkedVehicleSeatIntent
- PerceivedObstacleKind
- .InterStepBound
- ElementStatus
- .Read
- ConflictFilter
- TrafficV2Insertion
- .MeasurePath
- NetworkedPlayerLifecycleService
- IReadOnlyList
- RoutePath
- NetworkedPassengerActionIntent
- CampaignTraceability
- V2FallbackReason
- HostOwnedNetworkStateBehaviour
- NetworkedPlayerLifecycleIntent
- TrafficV2Code
- NetworkedPlayerReviveIntent
- LaneGraphRouting
- NetworkedVehicleRecoveryIntent
- ModelDto
- RoadRage.Shared.Domain
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- MatchSettings
- LaneGraph
- TrafficPerception
- JunctionExitBound
- JunctionRecord
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 107 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `NetworkedVehicleState` - 67 edges
6. `ImportContext` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
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

## Communities (182 total, 6 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.09
Nodes (18): TrafficV2Work, TrafficV2WorkCounters, TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PlanningTolerances, ProfilerMarker (+10 more)

### Community 3 - "MonoBehaviour"
Cohesion: 0.09
Nodes (15): Color, TMP_Text, LobbyPlayerSlotView, DevIndestructibleVehicle, GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy (+7 more)

### Community 4 - "SweepElement"
Cohesion: 0.17
Nodes (13): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+5 more)

### Community 5 - "RoutePlan"
Cohesion: 0.05
Nodes (60): IReadOnlyList, PlanningRequest, CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation, RoadModelVersion, DecisionCounter (+52 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (19): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+11 more)

### Community 7 - ".Localize"
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.09
Nodes (16): TelemetrySample, Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip (+8 more)

### Community 10 - "NetworkedPlayerSpawnService"
Cohesion: 0.13
Nodes (13): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+5 more)

### Community 11 - "RunFlowController"
Cohesion: 0.06
Nodes (8): Camera, CharacterController, Collider, GameObject, HashSet, Transform, RunFlowController, ActiveLocalPlayer

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.17
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyRosterService"
Cohesion: 0.06
Nodes (22): Difficulty, FacepunchSteamLobbyPlatform, Difficulty, ISteamLobbyPlatform, LobbyMemberSnapshot, CharacterId, DisplayName, Ready (+14 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 19 - ".Build"
Cohesion: 0.18
Nodes (11): JunctionKinematics, Known, ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId, Vector3 (+3 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.14
Nodes (12): Rigidbody, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile, Profile (+4 more)

### Community 22 - ".CreatePlan"
Cohesion: 0.17
Nodes (8): ConflictDecision, Dictionary, IList, RoadId, AutomatedPairDecisionPlan, RefinementInputs, IDictionary, RefinementInputs

### Community 23 - "AgentObservation"
Cohesion: 0.14
Nodes (20): Func, LaneSide, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AdjacentOccupantFact, AgentObservation (+12 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.20
Nodes (6): ConflictZone, Dictionary, HashSet, RoadCurveSample, SortedDictionary, AuthoredRoadModel

### Community 25 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.16
Nodes (11): CompiledRoadModel, HashSet, List, SortedDictionary, AutomatedPairDecisionManifest, AutomatedPairDecisionPolicy, AutomatedPairDecisionRecord, ClassificationSummary (+3 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.08
Nodes (28): RoadId, RoadModelSource, AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection (+20 more)

### Community 29 - "DefinitionId"
Cohesion: 0.14
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 30 - "RoadId"
Cohesion: 0.06
Nodes (39): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+31 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.09
Nodes (27): AppliedWidth, ConflictKind, FileLayout, Func, IList, JunctionControlKind, List, RoadCurveSample (+19 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (31): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+23 more)

### Community 33 - "MotionPlan"
Cohesion: 0.06
Nodes (40): VehicleCoverage, DrivabilityProfile, IReadOnlyList, LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None (+32 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (16): Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc, Transform (+8 more)

### Community 35 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 36 - "CharacterCatalog"
Cohesion: 0.16
Nodes (7): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PlayerProfileBootstrapService

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.16
Nodes (12): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+4 more)

### Community 40 - "PathHorizon"
Cohesion: 0.06
Nodes (38): Vector3, CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, Vector3 (+30 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.16
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.09
Nodes (23): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+15 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Measure"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+4 more)

### Community 45 - "PairSweep"
Cohesion: 0.22
Nodes (8): RoadBoundsBox, ConflictCandidate, IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "MeasurementRun"
Cohesion: 0.10
Nodes (21): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+13 more)

### Community 47 - "ReferenceTrack"
Cohesion: 0.21
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 48 - "JunctionRecords.cs"
Cohesion: 0.13
Nodes (10): JunctionApproach, Crossed, JunctionExitAssessment, JunctionMovementPosition, JunctionMovementStatus, Ahead, Behind, Occupied (+2 more)

### Community 49 - ".FullPath"
Cohesion: 0.17
Nodes (7): Action, KeyValuePair, MenuItem, Scene, MenuItem, MenuItem, Func

### Community 50 - "AuthoredRun"
Cohesion: 0.23
Nodes (6): List, RoadModelSource, AuthoredRun, Import, Succeeded, GateABinding

### Community 51 - "RoadRage.Features.Vehicles"
Cohesion: 0.11
Nodes (6): VehicleDamageType, Brake, Engine, Wheel, RoadRage.Shared.Networking, RoadRage.Features.Vehicles

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "ElementTrace"
Cohesion: 0.17
Nodes (12): ElementTrace, Key, Kind, MaxInterStepBoundMeters, MaxSpeedRatio, MaxStepDisplacementMeters, Passes, Runs (+4 more)

### Community 54 - ".Add"
Cohesion: 0.17
Nodes (14): Bounds, CompiledJunctionMovement, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadId, RoadLocation (+6 more)

### Community 55 - ".AddMovement"
Cohesion: 0.22
Nodes (6): JunctionFeature, Predicate, ImportedConnection, ImportedJunction, V1Edge, Key

### Community 56 - "TrafficDriveOutcome"
Cohesion: 0.12
Nodes (15): TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback, FallbackReason (+7 more)

### Community 57 - "PlayerProfile"
Cohesion: 0.07
Nodes (20): PersistentPlayerProfileRecord, PlayerNameValidator, PlayerProfile, CharacterId, DisplayName, PlayerProfileResolution, Error, IsResolved (+12 more)

### Community 58 - ".RenderBody"
Cohesion: 0.16
Nodes (5): CompiledJunctionControl, CompiledRoadModel, RoadModelValidationProfile, StringBuilder, Dictionary

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.28
Nodes (7): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference, RpcParams

### Community 60 - "PairReviewModel"
Cohesion: 0.18
Nodes (8): CompiledRoadModel, Dictionary, List, RoadBoundsBox, RoadId, StringBuilder, PairReview, PairReviewModel

### Community 61 - "ConflictDecisionKind"
Cohesion: 0.50
Nodes (3): ConflictDecisionKind, Accepted, Rejected

### Community 62 - "AIVehicleBehaviorDebugView"
Cohesion: 0.25
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 63 - "SweepPose"
Cohesion: 0.21
Nodes (8): GridPath, SweepPose, Plan, IReadOnlyList, Vector2, KinematicPoseSet, PoseGrid, RoadModelValidationProfile

### Community 64 - ".Decide"
Cohesion: 0.07
Nodes (34): DriverProfile, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind (+26 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.22
Nodes (4): RaycastHit, TireSample, Vector2, VehicleTireModel

### Community 66 - "TrackPiece"
Cohesion: 0.18
Nodes (10): RoadCurve, RoadElementKind, NominalPose, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 67 - "LobbyJoinOutcome"
Cohesion: 0.12
Nodes (16): Task, Task, LobbyCreateOutcome, LobbyId, Success, LobbyJoinFailureReason, Expired, Failed (+8 more)

### Community 68 - "TrafficJunctionOutcome"
Cohesion: 0.22
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+19 more)

### Community 70 - ".Compute"
Cohesion: 0.16
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.08
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.09
Nodes (25): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+17 more)

### Community 73 - ".Draw"
Cohesion: 0.23
Nodes (8): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.09
Nodes (29): TrafficHazardCollectorCounters, Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState (+21 more)

### Community 75 - "HazardRootClass"
Cohesion: 0.14
Nodes (12): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer, HazardRootClass, Obstacle, Self (+4 more)

### Community 76 - "UserNotice"
Cohesion: 0.10
Nodes (13): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+5 more)

### Community 77 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 78 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.25
Nodes (5): Surface, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "TrackingTolerance"
Cohesion: 0.36
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

### Community 81 - "PlayerMode"
Cohesion: 0.19
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+9 more)

### Community 83 - "TrackingMeasurement.cs"
Cohesion: 0.22
Nodes (9): IReadOnlyList, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees (+1 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.17
Nodes (19): CompiledRoadModel, Dictionary, KeyValuePair, List, RoadId, Grant, JunctionCoordinator, Batches (+11 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.15
Nodes (12): Vector2, ConflictSweep, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState, MovementSide (+4 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.09
Nodes (21): JunctionRecord, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, PairDecisionState, Confirmed (+13 more)

### Community 87 - ".RenderSignoff"
Cohesion: 0.22
Nodes (5): CompiledRoadModel, IList, SignoffLayout, DateTime, SignoffLayout

### Community 88 - "LobbyFlowController"
Cohesion: 0.10
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (19): JunctionMovement, RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 90 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 91 - "TaskDispositionKind"
Cohesion: 0.20
Nodes (8): IEnumerable, TaskDispositionKind, AuthoredOnControls, Deferred, GenericEntryFallback, NotRequiredForCurrentControlKind, Reviewed, Unsignalized

### Community 92 - "RoadLineage"
Cohesion: 0.06
Nodes (38): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadId, Vector3, Entry, RightOfWay (+30 more)

### Community 93 - ".Measure"
Cohesion: 0.15
Nodes (15): BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, Scene, Vector3 (+7 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (27): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+19 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.11
Nodes (16): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+8 more)

### Community 96 - "AutomatedPairClassification"
Cohesion: 0.20
Nodes (7): AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, RoadModelValidationProfile, Vector3

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.14
Nodes (12): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, GateAReviewWindow (+4 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.30
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "RoadRage.Shared.Definitions"
Cohesion: 0.22
Nodes (3): DevVehicleSpawner, RoadRage.Features.Rage, RoadRage.Shared.Definitions

### Community 102 - "NetworkedRageState"
Cohesion: 0.13
Nodes (10): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, Quaternion, Vector3, NetworkVariable (+2 more)

### Community 103 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 104 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.07
Nodes (22): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+14 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.10
Nodes (16): Vector3, Junction, JunctionFeature, Crossroads, Other, Roundabout, TJunction, Unspecified (+8 more)

### Community 112 - "DriverProfileDef"
Cohesion: 0.27
Nodes (5): DriverProfileDef, Id, Profile, RawId, ScriptableObject

### Community 113 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 114 - "ImportedCurve"
Cohesion: 0.18
Nodes (13): Dictionary, IList, IReadOnlyList, KeyValuePair, List, RoadCurve, RoadCurveSample, RoadRecordKind (+5 more)

### Community 116 - "LongitudinalDecision"
Cohesion: 0.09
Nodes (24): IReadOnlyList, LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses (+16 more)

### Community 117 - "JunctionTraversal"
Cohesion: 0.25
Nodes (6): JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 120 - "DriverProfile"
Cohesion: 0.11
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 123 - ".Step"
Cohesion: 0.10
Nodes (16): DriverProfile, IReadOnlyList, JunctionControlKind, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord (+8 more)

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.15
Nodes (8): PairReviewActions, PairReviewEntry, IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "NetworkedPlayerState"
Cohesion: 0.33
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.07
Nodes (23): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+15 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (21): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+13 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (12): Difficulty, Difficulty, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, Difficulty (+4 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.05
Nodes (65): DrivabilityProfile, EffectiveLaneCorridor, RoadModelCanonicalPayload, Comparison, RoadModelCompiler, DrivabilityProfile, ImportManifest, ImportManifestEntry (+57 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.08
Nodes (35): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+27 more)

### Community 135 - "FileLayout"
Cohesion: 0.33
Nodes (6): FileLayout, ConflictRecord, ControlRecord, DeferredRecord, DispositionRecord, WidthRecord

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
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.09
Nodes (18): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, InFallback (+10 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "MovementRole"
Cohesion: 0.33
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 148 - "ImportContext"
Cohesion: 0.14
Nodes (10): DrivabilityProfile, RoadBoundsBox, RoadCurvePoint, RoadLocalizationProfile, RoadModelValidationProfile, Vector3, CircleFit, ImportContext (+2 more)

### Community 149 - "AppSceneRouter.cs"
Cohesion: 0.40
Nodes (3): AppPlayModeEntry, PlayModeStateChange, SceneAsset

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+5 more)

### Community 153 - ".InterStepBound"
Cohesion: 0.33
Nodes (6): Quaternion, Vector3, BodyState, GaugeBox, Rho, TrackingMeasurement

### Community 154 - "ElementStatus"
Cohesion: 0.40
Nodes (4): ElementStatus, Measured, NotMeasured, NotSelectable

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "ConflictFilter"
Cohesion: 0.40
Nodes (5): ConflictFilter, All, Approach, Movement, Zone

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.06
Nodes (36): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+28 more)

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.07
Nodes (25): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+17 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (32): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+24 more)

### Community 170 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.14
Nodes (11): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority (+3 more)

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 174 - "TrafficV2Code"
Cohesion: 0.07
Nodes (27): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+19 more)

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
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.11
Nodes (15): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+7 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.14
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 209 - "MatchSettings"
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 216 - "LaneGraph"
Cohesion: 0.14
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.12
Nodes (22): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+14 more)

### Community 218 - "JunctionExitBound"
Cohesion: 0.12
Nodes (15): IReadOnlyList, JunctionRecord, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations (+7 more)

### Community 219 - "JunctionRecord"
Cohesion: 0.18
Nodes (11): JunctionSnapshot, JunctionControlKind, RoadId, JunctionBatchCounters, JunctionRecord, IsEffectiveGrant, JunctionSnapshot, Counters (+3 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1148 isolated node(s):** `RoadRage.App`, `Instance`, `Router`, `Notices`, `Profiles` (+1143 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1622 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **6 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `RoadRage.Features.Vehicles.Traffic.Migration`, `RoutePlan`, `GateAEvidenceParameters`, `JunctionActorReport`, `Blocker`, `AgentObservation`, `.InterStepBound`, `TrafficV2Insertion`, `TrafficFrame`, `V2FallbackReason`, `HostOwnedNetworkStateBehaviour`, `ReferenceTrack`, `.Decide`, `TrafficDecisionProjection`, `ElementOccupant`, `TrafficV2HazardCollector`, `TrackingTolerance`, `LongitudinalDecision`, `.Step`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.250) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `MonoBehaviour`, `RageRoadEventFlowController`, `MeasurementRun`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `LaneGraph`, `.Step`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.174) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `RoutePlan`, `RoadModelRecords.cs`, `.Localize`, `NetworkedPassengerActionIntent`, `TrafficV2HazardCollector`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadRage.Features.Vehicles.Traffic`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `RoadLineage`, `TrafficV2Insertion`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.117) - this node is a cross-community bridge._
- **What connects `RoadRage.App`, `Instance`, `Router` to the rest of the system?**
  _1148 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10810810810810811 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Migration` be split into smaller, more focused modules?**
  _Cohesion score 0.09390243902439024 - nodes in this community are weakly interconnected._
- **Should `MonoBehaviour` be split into smaller, more focused modules?**
  _Cohesion score 0.09486166007905138 - nodes in this community are weakly interconnected._