# Graph Report - RRS  (2026-10-06)

## Corpus Check
- 178 files · ~242,976 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4555 nodes · 10540 edges · 183 communities (171 shown, 12 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 456 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `ad3b94d4`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedVehicleSeatService
- .Refine
- RoadRage.Features.Vehicles.Traffic.Migration
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
- RoadRage.Features.Online
- NetworkedVehicleDamageVfxController
- LobbyRosterService
- LocalOnFootController
- Blocker
- RageTuningDef
- RoutePlan
- MigrationReport
- VehiclePhysicsBody
- MotionPlan
- AgentObservation
- MainMenuScreen
- NpcReactionEffect
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1ImportResult
- DefinitionId
- RoadId
- AuthoringDecisions
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- JunctionClearance
- NetworkedRunSessionMonitor
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- .Measure
- SweepPose
- TrafficV2Composition.cs
- RouteReason
- JunctionRecords.cs
- GateAEvidenceResult
- AuthoredRoadModel
- NetworkedVehicleState.cs
- .Regenerate
- PairReviewModel
- .Add
- ImportContext
- TrafficDriveOutcome
- MainMenuProfileFlowController
- .Collect
- NetworkedAIVehicleState
- PairReview
- PairReviewEntry
- AIVehicleBehaviorDebugView
- ObservationChannel
- .Decide
- .FixedUpdate
- TrackPiece
- VehicleDriveIntent
- IPathGeometry
- PassengerActionVerdictCode
- .Compute
- TrafficDecisionProjection
- SpatialEntry
- .Draw
- TrafficV2HazardCollector
- HazardRootClass
- UserNotice
- DispositionKind
- MainMenuFlowController
- .CheckVisuals
- .PrepareStep
- NetworkedPlayerState
- KinematicOffsetBounds
- InterStepResult
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- PlayerProfileFileStore
- LobbyFlowController
- RoadModelValidationIssue
- .Run
- LobbyPlayerSlotView
- RoadLineage
- .Measure
- V1Node
- JunctionConflictIndex
- RunEscapeMenuScreen
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- VehicleCoverage
- .Create
- JunctionClearanceResult
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- .FingerprintWithInputs
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- DriverProfileDef
- PlanningDecision
- LineageKeyRegistry
- .MergeGapAdmits
- LongitudinalDecision
- TrafficHazardKind
- TrafficSettingsDef
- PairDecisionState
- DriverProfile
- StatusFilter
- .Evaluate
- .Step
- PairRelation
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
- FileLayout
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- .Track
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- StopHoldRelease
- RoadModelCanonicalWriter
- SpeedConstraint
- PathIssue
- .BuildSmoothCurve
- StopHoldPhase
- SpeedPlanIssue
- NetworkedVehicleSeatIntent
- PerceivedObstacleKind
- ReferenceTrack
- NetworkedCrewEconomyState.cs
- .Read
- CompiledRoadModel
- TrafficV2Insertion
- .MeasurePath
- NetworkedPlayerLifecycleService
- Dictionary
- KeyValuePair
- ConditionalWeakTable
- RoutePath
- DriverProfile
- Vector3
- NetworkedPassengerActionIntent
- CampaignTraceability
- V2FallbackReason
- NetworkedBossState.cs
- NetworkedPlayerLifecycleIntent
- TrafficV2Code
- NetworkedPlayerReviveIntent
- LaneGraphRouting
- NetworkedVehicleRecoveryIntent
- .Compile
- RoadRage.Shared.Domain
- LobbyCodeClipboard
- NetworkedPlayerPresentation
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 107 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `NetworkedVehicleDriverController` - 58 edges
10. `TrafficFrame` - 56 edges

## Surprising Connections (you probably didn't know these)
- `TrafficV2StepRunner` --references--> `JunctionCoordinator`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs
- `JunctionActorReport` --references--> `JunctionKinematics`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionPriority.cs
- `NetworkedPlayerSpawnService` --references--> `NetworkedVehicleSeatService`  [EXTRACTED]
  Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs → Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs
- `NetworkedVehicleSeatService` --references--> `RunCheckpointHudScreen`  [EXTRACTED]
  Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs → Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs
- `PairRefinement` --references--> `SweepPose`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweepRefinement.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs

## Import Cycles
- None detected.

## Communities (183 total, 12 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 1 - ".Refine"
Cohesion: 0.10
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.10
Nodes (13): TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, ProfilerMarker, PlanningSpine, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Migration (+5 more)

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

### Community 7 - ".Localize"
Cohesion: 0.12
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

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
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyRosterService"
Cohesion: 0.04
Nodes (38): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+30 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.08
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 19 - "RoutePlan"
Cohesion: 0.11
Nodes (20): IReadOnlyList, RoadElementKind, RoadId, RoadModelVersion, RouteOccurrence, RoutePlan, Diagnostics, DistanceMeters (+12 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "MotionPlan"
Cohesion: 0.15
Nodes (15): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+7 more)

### Community 23 - "AgentObservation"
Cohesion: 0.17
Nodes (12): Func, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AgentObservation, ExitOccupancyFact, IntentOverlapKind (+4 more)

### Community 24 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 25 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (19): CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, SortedDictionary, Vector3 (+11 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.11
Nodes (24): RoadCurve, AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedConnection, ImportedCurve, ImportedPortal (+16 more)

### Community 29 - "DefinitionId"
Cohesion: 0.11
Nodes (15): List, CharacterCatalog, Count, Color, GameObject, CharacterDef, DisplayName, Id (+7 more)

### Community 30 - "RoadId"
Cohesion: 0.07
Nodes (36): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+28 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (46): ConflictKind, Dictionary, FileLayout, Func, IEnumerable, IList, JunctionControlKind, List (+38 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (33): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+25 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.08
Nodes (13): DevIndestructibleVehicle, Action, Collider, Collision, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody (+5 more)

### Community 35 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 36 - "NetworkedRunSessionMonitor"
Cohesion: 0.21
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.12
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.14
Nodes (14): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+6 more)

### Community 40 - "PathHorizon"
Cohesion: 0.10
Nodes (25): CompiledRoadModel, DrivabilityProfile, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd (+17 more)

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

### Community 45 - "SweepPose"
Cohesion: 0.17
Nodes (13): RoadModelValidationProfile, IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate, SweepPose (+5 more)

### Community 46 - "TrafficV2Composition.cs"
Cohesion: 0.09
Nodes (28): IReadOnlyList, RoadId, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind (+20 more)

### Community 47 - "RouteReason"
Cohesion: 0.12
Nodes (19): RouteDiagnostic, None, ZeroWeightFallback, RouteOutcome, InvalidInput, NoRoute, Planned, Replanned (+11 more)

### Community 48 - "JunctionRecords.cs"
Cohesion: 0.07
Nodes (30): JunctionControlKind, JunctionBatchCounters, JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant (+22 more)

### Community 49 - "GateAEvidenceResult"
Cohesion: 0.15
Nodes (14): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+6 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (26): Action, CompiledJunctionControl, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IList, KeyValuePair (+18 more)

### Community 51 - "NetworkedVehicleState.cs"
Cohesion: 0.40
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 52 - ".Regenerate"
Cohesion: 0.15
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReviewModel"
Cohesion: 0.23
Nodes (10): CompiledRoadModel, List, RoadId, StringBuilder, PairReviewModel, PairReviewStatus, Modified, New (+2 more)

### Community 54 - ".Add"
Cohesion: 0.13
Nodes (15): Bounds, CompiledJunctionMovement, IReadOnlyList, Predicate, RoadBoundsBox, RoadCurve, RoadCurveSample, RoadId (+7 more)

### Community 55 - "ImportContext"
Cohesion: 0.17
Nodes (8): JunctionFeature, List, Predicate, RoadCurveSample, ImportContext, ImportedJunction, V1RoadModelImporter, CircleFit

### Community 56 - "TrafficDriveOutcome"
Cohesion: 0.12
Nodes (15): TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback, FallbackReason (+7 more)

### Community 57 - "MainMenuProfileFlowController"
Cohesion: 0.12
Nodes (14): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, PlayerNameValidator, PlayerProfile, CharacterId, DisplayName, PlayerProfileBootstrapService (+6 more)

### Community 58 - ".Collect"
Cohesion: 0.19
Nodes (12): Bounds, CharacterController, IReadOnlyList, NetworkedAIVehicleState, Rigidbody, RoadId, Vector3, VehiclePhysicsBody (+4 more)

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.16
Nodes (14): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, IRageDispositionSource, CurrentDisposition, RageDisposition (+6 more)

### Community 60 - "PairReview"
Cohesion: 0.26
Nodes (3): Dictionary, RoadBoundsBox, PairReview

### Community 62 - "AIVehicleBehaviorDebugView"
Cohesion: 0.24
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 63 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 64 - ".Decide"
Cohesion: 0.10
Nodes (23): DriverProfile, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind (+15 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 66 - "TrackPiece"
Cohesion: 0.19
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 67 - "VehicleDriveIntent"
Cohesion: 0.23
Nodes (6): VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 68 - "IPathGeometry"
Cohesion: 0.23
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+19 more)

### Community 70 - ".Compute"
Cohesion: 0.13
Nodes (12): BinaryWriter, IReadOnlyList, MenuItem, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3 (+4 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.04
Nodes (46): IReadOnlyList, RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind (+38 more)

### Community 72 - "SpatialEntry"
Cohesion: 0.10
Nodes (23): Bounds, IReadOnlyList, RoadBoundsBox, RoadId, Vector3, ElementClosureInput, IntentInterval, OccupancyExclusion (+15 more)

### Community 73 - ".Draw"
Cohesion: 0.23
Nodes (7): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, RoadModelValidationProfile, SceneView

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.11
Nodes (17): TrafficHazardCollectorCounters, Collider, Dictionary, List, ProfilerMarker, Stopwatch, TrafficV2HazardCollector, Capacity (+9 more)

### Community 75 - "HazardRootClass"
Cohesion: 0.25
Nodes (7): HazardRootClass, Obstacle, Self, Static, TrafficV2Vehicle, Vehicle, WalkingPlayer

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "DispositionKind"
Cohesion: 0.18
Nodes (11): DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint, Movement, MovementApproachPath (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.24
Nodes (6): Surface, HashSet, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - ".PrepareStep"
Cohesion: 0.19
Nodes (6): TrackingToleranceResponse, Latched, LatchedAtStep, ProfilerMarker, TrackingTolerance, Undeclared

### Community 81 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (16): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+8 more)

### Community 83 - "InterStepResult"
Cohesion: 0.17
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.12
Nodes (30): IReadOnlyList, JunctionActorReport, JunctionConflictIndex, JunctionRecord, JunctionSnapshot, JunctionTraversal, List, RoadId (+22 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.15
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.18
Nodes (12): JunctionRecord, RoadCurveSample, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, MovementRecord (+4 more)

### Community 87 - "PlayerProfileFileStore"
Cohesion: 0.31
Nodes (5): PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath

### Community 88 - "LobbyFlowController"
Cohesion: 0.08
Nodes (13): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, Difficulty, MatchSettings, AiVehicleTargetCount (+5 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.20
Nodes (11): JunctionMovement, SignalPlan, Dictionary, IReadOnlyList, List, Vector3, RoadModelCompilationException, Issues (+3 more)

### Community 90 - ".Run"
Cohesion: 0.39
Nodes (4): CompiledRoadModel, IEnumerable, JunctionSnapshot, TrafficV2StepCost

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.06
Nodes (41): CompiledJunctionControl, CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadCurveSample, RoadId (+33 more)

### Community 93 - ".Measure"
Cohesion: 0.17
Nodes (13): BoxCollider, Collider, CompiledRoadModel, GameObject, List, Scene, Vector3, VehiclePhysicsBody (+5 more)

### Community 94 - "V1Node"
Cohesion: 0.10
Nodes (28): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+20 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.07
Nodes (35): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+27 more)

### Community 96 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

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
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 102 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 103 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

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

### Community 109 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.16
Nodes (13): RoadModelValidationProfile, Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor (+5 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.20
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - "DriverProfileDef"
Cohesion: 0.36
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 113 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 114 - "LineageKeyRegistry"
Cohesion: 0.38
Nodes (6): Dictionary, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys

### Community 115 - ".MergeGapAdmits"
Cohesion: 0.18
Nodes (5): JunctionPriority, RoadId, JunctionApproach, Crossed, MergeConflict

### Community 116 - "LongitudinalDecision"
Cohesion: 0.09
Nodes (24): IReadOnlyList, LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses (+16 more)

### Community 117 - "TrafficHazardKind"
Cohesion: 0.33
Nodes (5): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "PairDecisionState"
Cohesion: 0.33
Nodes (6): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - ".Evaluate"
Cohesion: 0.15
Nodes (11): DriverProfile, PlanningReach, CompiledRoadModel, IReadOnlyList, PlanningRequest, CompiledRoadModel, RoadLocation, DecisionCounter (+3 more)

### Community 123 - ".Step"
Cohesion: 0.10
Nodes (17): DriverProfile, IReadOnlyList, JunctionControlKind, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord (+9 more)

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (50): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, Rigidbody, Stopwatch (+42 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.15
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.05
Nodes (31): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+23 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.12
Nodes (17): HashSet, List, ProfilerMarker, RoadId, Stopwatch, TrafficV2StepRunner, Collector, Coordinator (+9 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (12): Difficulty, Difficulty, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, Difficulty (+4 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.16
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (82): EffectiveLaneCorridor, IList, RoadLocalizationProfile, RoadModelSource, RoadModelCanonicalPayload, DrivabilityProfile, ImportManifest, ImportManifestEntry (+74 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

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
Cohesion: 0.07
Nodes (24): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, InFallback (+16 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "StopHoldRelease"
Cohesion: 0.33
Nodes (6): StopHoldRelease, GapOpened, GrantEffective, None, SourceDeparted, SourceGone

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 148 - ".BuildSmoothCurve"
Cohesion: 0.20
Nodes (5): DrivabilityProfile, RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit

### Community 149 - "StopHoldPhase"
Cohesion: 0.40
Nodes (5): StopHoldPhase, Entered, Holding, None, Released

### Community 150 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "PerceivedObstacleKind"
Cohesion: 0.16
Nodes (11): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+3 more)

### Community 153 - "ReferenceTrack"
Cohesion: 0.15
Nodes (14): Quaternion, RoadKinematicAnchor, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound (+6 more)

### Community 154 - "NetworkedCrewEconomyState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedCrewEconomyState, RoadRage.Features.Economy

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.08
Nodes (26): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadLocation, Vector3, TrafficV2Admission (+18 more)

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.07
Nodes (27): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+19 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.08
Nodes (22): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements, ElementStatus, Measured (+14 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.08
Nodes (31): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+23 more)

### Community 170 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

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

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.06
Nodes (21): SessionTrafficValue, IHostOwnedRuntimeState, IsHostAuthority, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI (+13 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.14
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.11
Nodes (13): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+5 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.12
Nodes (20): LaneSide, AdjacentOccupantFact, ObservationMetadata, ObservationSource, PublishedHorizon, SpatialQuery, StructuredOccupancy, VehicleGapFact (+12 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.11
Nodes (11): BoxCollider, CharacterController, Collider, List, NetworkTransform, Quaternion, RaycastHit, Rigidbody (+3 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1149 isolated node(s):** `Index`, `Model`, `Current`, `FrameFailures`, `Batches` (+1144 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1634 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **12 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `JunctionActorReport`, `Blocker`, `RoutePlan`, `AgentObservation`, `ReferenceTrack`, `TrafficV2Insertion`, `V2FallbackReason`, `NetworkedPlayerReviveIntent`, `.Collect`, `.Decide`, `TrafficDecisionProjection`, `SpatialEntry`, `.PrepareStep`, `InterStepResult`, `.Run`, `LongitudinalDecision`, `.Step`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.188) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `MonoBehaviour`, `TrafficV2Composition.cs`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `LaneGraph`, `.Run`, `.Step`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.164) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `AutomatedPairDecisionPolicy`, `RoadModelRecords.cs`, `SpeedPlan`, `.Localize`, `NetworkedPassengerActionIntent`, `TrafficV2HazardCollector`, `RoadCurve`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadModelCanonicalWriter`, `.Regenerate`, `RoadModelValidationIssue`, `.Evaluate`, `RoadLineage`, `TrafficV2Insertion`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.150) - this node is a cross-community bridge._
- **What connects `Index`, `Model`, `Current` to the rest of the system?**
  _1149 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10365853658536585 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Migration` be split into smaller, more focused modules?**
  _Cohesion score 0.09879032258064516 - nodes in this community are weakly interconnected._
- **Should `GateAEvidenceParameters` be split into smaller, more focused modules?**
  _Cohesion score 0.11594202898550725 - nodes in this community are weakly interconnected._