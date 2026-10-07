# Graph Report - RRS  (2026-10-07)

## Corpus Check
- 179 files · ~244,190 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4628 nodes · 10574 edges · 221 communities (172 shown, 47 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 461 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `9b024cdd`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedVehicleSeatService
- .Refine
- RoadRage.Features.Vehicles.Traffic.Migration
- GreyboxAssetSeedMetadata
- SweepPose
- .Core
- GateAEvidenceParameters
- .TryGetMovement
- TireSample
- VehicleSuspensionModel
- MonoBehaviour
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- FacepunchSteamLobbyPlatform
- LocalOnFootController
- Blocker
- RageTuningDef
- RoutePlan
- MigrationReport
- VehiclePhysicsBody
- JunctionRecord
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
- MotionPlan
- NetworkedVehicleDriverController
- .Run
- NetworkedRunSessionMonitor
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
- LobbyRosterService
- JunctionRecords.cs
- GateAEvidenceResult
- AuthoredRoadModel
- .ApplyDecisions
- .Regenerate
- PairReviewModel
- .Add
- .AddMovement
- .Summarize
- CharacterCatalog
- ImportedCurve
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
- TrafficFrameInputs.cs
- .Draw
- TrafficV2HazardCollector
- PerceivedObstacleKind
- UserNotice
- RoadBoundsBox
- MainMenuFlowController
- PlayerProfile
- ModelDto
- NetworkedPlayerState
- KinematicOffsetBounds
- InterStepResult
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- PlayerProfileFileStore
- LobbyFlowController
- RoadModelValidationIssue
- IEnumerable
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
- RageDisposition
- .Create
- LocalVoidRespawnController
- .HandleLifecycleChanged
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- .Resolve
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- DriverProfileDef
- PlanningDecision
- .Manifest
- .GapSeconds
- LongitudinalDecision
- TaskDispositionKind
- TrafficSettingsDef
- PairDecisionState
- DriverProfile
- StatusFilter
- PlanningReach
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
- AutomatedPairClassification
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- LongitudinalArbitration.cs
- RoadModelCanonicalWriter
- SpeedConstraint
- PathIssue
- ImportContext
- NetworkedVehicleSeatIntent
- .Obstacles
- ReferenceTrack
- HostOwnedNetworkStateBehaviour
- .Read
- NetworkPlayerRegistry
- TrafficV2Insertion
- .ApplyHonkTarget
- NetworkedPlayerLifecycleService
- PassengerActionIntent
- .Accumulate
- ConditionalWeakTable
- RoutePath
- DriverProfile
- Vector3
- NetworkedPassengerActionIntent
- CampaignTraceability
- .Build
- V2FallbackReason
- RoadModelCompilationException
- NetworkedPlayerLifecycleIntent
- .Configure
- .SubmitPoseRpc
- TrafficV2Code
- ProjectionKey
- MovementRole
- TrafficV2Settings
- .IsSurfaceOnlyCollision
- TrafficV2WorkCounters.cs
- NetworkedPlayerReviveIntent
- JunctionActorReport
- JunctionConflictIndex
- LaneGraphRouting
- JunctionRecord
- JunctionTraversal
- JunctionControlKind
- ProfilerMarker
- Stopwatch
- NetworkedVehicleRecoveryIntent
- .Compile
- Action
- Bounds
- RoadRage.Shared.Domain
- CompiledJunctionControl
- CompiledJunctionMovement
- LobbyCodeClipboard
- IList
- MenuItem
- Predicate
- NetworkedPlayerPresentation
- RoadBoundsBox
- RoadCurve
- RoadLocation
- RoadModelValidationProfile
- Scene
- SortedDictionary
- StringBuilder
- VehicleFootprint
- CompiledRoadModel
- Dictionary
- JunctionApproach
- JunctionExitBound
- JunctionGrantStatus
- JunctionReason
- KeyValuePair
- LaneGraph
- TrafficPerception
- Pending
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 104 edges
2. `RunFlowController` - 99 edges
3. `RoadId` - 90 edges
4. `ConflictSweep` - 72 edges
5. `NetworkedVehicleState` - 67 edges
6. `ImportContext` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `NetworkedVehicleDriverController` - 58 edges
10. `SweepPose` - 56 edges

## Surprising Connections (you probably didn't know these)
- `JunctionCoordinator` --references--> `JunctionSnapshot`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs
- `TrafficV2StepRunner` --references--> `JunctionCoordinator`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs
- `Grant` --references--> `JunctionReason`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs
- `Pending` --references--> `JunctionTraversal`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs
- `V2JunctionTrace` --references--> `JunctionGrantStatus`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs

## Import Cycles
- None detected.

## Communities (221 total, 47 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.17
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (17): ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2, MovementSide (+9 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.12
Nodes (11): PlanningTolerances, ProfilerMarker, PlanningSpine, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning (+3 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.18
Nodes (15): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+7 more)

### Community 5 - ".Core"
Cohesion: 0.16
Nodes (19): RouteDiagnostic, None, ZeroWeightFallback, RouteResult, CompiledRoadModel, Dictionary, HashSet, List (+11 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - ".TryGetMovement"
Cohesion: 0.10
Nodes (25): CompiledJunctionControl, RoadCurveSample, ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3 (+17 more)

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
Cohesion: 0.07
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.13
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.07
Nodes (24): Difficulty, Task, FacepunchSteamLobbyPlatform, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId, Success (+16 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.09
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 19 - "RoutePlan"
Cohesion: 0.05
Nodes (40): CompiledRoadModel, IReadOnlyList, RoadElementKind, RoadId, RoadLocation, RoadModelVersion, DecisionCounter, RouteOccurrence (+32 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "JunctionRecord"
Cohesion: 0.08
Nodes (24): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale (+16 more)

### Community 23 - "AgentObservation"
Cohesion: 0.15
Nodes (18): Func, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AgentObservation, ExitOccupancyFact, IntentOverlapKind (+10 more)

### Community 24 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 25 - "NpcReactionEffect"
Cohesion: 0.11
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.15
Nodes (12): ConflictDecision, Dictionary, IList, RoadId, RoadModelValidationProfile, AutomatedPairDecisionManifest, AutomatedPairDecisionPlan, AutomatedPairDecisionPolicy (+4 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.08
Nodes (25): RoadId, AuthoringTask, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint (+17 more)

### Community 29 - "DefinitionId"
Cohesion: 0.14
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 30 - "RoadId"
Cohesion: 0.08
Nodes (31): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+23 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.08
Nodes (31): ConflictKind, FileLayout, Func, IList, JunctionControlKind, List, RoadBoundsBox, RoadCurveSample (+23 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.08
Nodes (30): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+22 more)

### Community 33 - "MotionPlan"
Cohesion: 0.05
Nodes (45): VehicleCoverage, TrackingToleranceResponse, Latched, LatchedAtStep, DrivabilityProfile, IReadOnlyList, LongitudinalBounds, Valid (+37 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.14
Nodes (8): Action, Collider, NetworkTransform, Quaternion, Rigidbody, Transform, Vector3, NetworkedVehicleDriverController

### Community 35 - ".Run"
Cohesion: 0.14
Nodes (10): Action, KeyValuePair, GateABinding, MenuItem, MenuItem, Func, GateAEvidenceParameters, MenuItem (+2 more)

### Community 36 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.15
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.09
Nodes (25): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd, ExitPortal (+17 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.14
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.08
Nodes (25): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+17 more)

### Community 43 - "RoadCurve"
Cohesion: 0.15
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Measure"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+4 more)

### Community 45 - "PairSweep"
Cohesion: 0.28
Nodes (6): IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "MeasurementRun"
Cohesion: 0.11
Nodes (20): IReadOnlyList, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label, Triplets (+12 more)

### Community 47 - "LobbyRosterService"
Cohesion: 0.09
Nodes (15): Difficulty, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId (+7 more)

### Community 48 - "JunctionRecords.cs"
Cohesion: 0.08
Nodes (24): JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations, RouteEnd (+16 more)

### Community 49 - "GateAEvidenceResult"
Cohesion: 0.13
Nodes (15): CompiledRoadModel, CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing (+7 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.10
Nodes (19): CompiledRoadModel, List, AuthoredRoadModel, AuthoredRun, Import, Succeeded, SignoffLayout, AuthoringDecisions (+11 more)

### Community 51 - ".ApplyDecisions"
Cohesion: 0.14
Nodes (10): Dictionary, HashSet, RoadCurveSample, RoadModelSource, AppliedWidth, AuthoringTask, ImportedJunction, V1ImportResult (+2 more)

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReviewModel"
Cohesion: 0.24
Nodes (9): CompiledRoadModel, List, StringBuilder, PairReviewModel, PairReviewStatus, Modified, New, Removed (+1 more)

### Community 54 - ".Add"
Cohesion: 0.14
Nodes (17): IReadOnlyList, RoadId, Vector3, LocalizationFixture, OverlayInstance, OverlayPrimitive, IsBox, Bounds (+9 more)

### Community 56 - ".Summarize"
Cohesion: 0.16
Nodes (8): CompiledRoadModel, HashSet, List, SortedDictionary, ClassificationSummary, RefinedPair, ClassificationSummary, RefinedPair

### Community 57 - "CharacterCatalog"
Cohesion: 0.19
Nodes (7): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PlayerProfileBootstrapService

### Community 58 - "ImportedCurve"
Cohesion: 0.19
Nodes (9): List, RoadCurve, RoadCurveSample, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedCurve, PublishedDisplacement (+1 more)

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.44
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 62 - "AIVehicleBehaviorDebugView"
Cohesion: 0.25
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 63 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 64 - ".Decide"
Cohesion: 0.15
Nodes (14): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing (+6 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 66 - "TrackPiece"
Cohesion: 0.16
Nodes (10): RoadCurve, RoadElementKind, RoadId, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id (+2 more)

### Community 67 - "VehicleDriveIntent"
Cohesion: 0.16
Nodes (7): RpcParams, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 68 - "IPathGeometry"
Cohesion: 0.22
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (29): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+21 more)

### Community 70 - ".Compute"
Cohesion: 0.16
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.04
Nodes (53): IReadOnlyList, RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind (+45 more)

### Community 72 - "TrafficFrameInputs.cs"
Cohesion: 0.16
Nodes (14): IReadOnlyList, RoadElementKind, RoadId, ElementClosureInput, IntentInterval, OccupancyExclusion, None, NotLocalized (+6 more)

### Community 73 - ".Draw"
Cohesion: 0.20
Nodes (9): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, Color, IReadOnlyList, RoadCurveSample, RoadModelValidationProfile (+1 more)

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.06
Nodes (44): TrafficHazardCollectorCounters, RoadBoundsBox, Vector3, TrafficHazardInput, TrafficHazardKind, Obstacle, Pedestrian, Vehicle (+36 more)

### Community 75 - "PerceivedObstacleKind"
Cohesion: 0.13
Nodes (13): Bounds, SpatialEntry, SpatialQueryBuffer, Capacity, Count, Saturated, Total, PerceivedObstacleKind (+5 more)

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "RoadBoundsBox"
Cohesion: 0.15
Nodes (14): CompiledConflictZone, Vector3, ConflictKind, Crossing, Merge, ConflictZone, Junction, JunctionFeature (+6 more)

### Community 79 - "PlayerProfile"
Cohesion: 0.17
Nodes (8): PlayerProfile, CharacterId, DisplayName, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection

### Community 80 - "ModelDto"
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 81 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.09
Nodes (21): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+13 more)

### Community 83 - "InterStepResult"
Cohesion: 0.17
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.12
Nodes (23): CompiledRoadModel, Dictionary, IReadOnlyList, KeyValuePair, List, RoadId, TrafficFrame, Grant (+15 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.15
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.20
Nodes (11): JunctionRecord, RoadId, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, Document (+3 more)

### Community 87 - "PlayerProfileFileStore"
Cohesion: 0.31
Nodes (5): PersistentPlayerProfileRecord, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath

### Community 88 - "LobbyFlowController"
Cohesion: 0.09
Nodes (13): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, Difficulty, MatchSettings, AiVehicleTargetCount (+5 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.22
Nodes (16): ConflictZone, Dictionary, List, RoadCurveSample, RoadId, RoadModelSource, Vector3, RoadModelValidationIssue (+8 more)

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.06
Nodes (38): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadId, Vector3, Entry, RightOfWay (+30 more)

### Community 93 - ".Measure"
Cohesion: 0.07
Nodes (36): Surface, BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, IEnumerable, IReadOnlyList (+28 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (30): Predicate, Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List (+22 more)

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
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 102 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 103 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.10
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MenuCharacterPreview"
Cohesion: 0.18
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - ".Resolve"
Cohesion: 0.20
Nodes (7): PlayerNameValidator, PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist, PlayerProfileResolution

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.20
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - "DriverProfileDef"
Cohesion: 0.36
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 113 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 114 - ".Manifest"
Cohesion: 0.24
Nodes (8): Dictionary, IList, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys, ImportManifestEntry

### Community 116 - "LongitudinalDecision"
Cohesion: 0.09
Nodes (18): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+10 more)

### Community 117 - "TaskDispositionKind"
Cohesion: 0.20
Nodes (8): IEnumerable, TaskDispositionKind, AuthoredOnControls, Deferred, GenericEntryFallback, NotRequiredForCurrentControlKind, Reviewed, Unsignalized

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

### Community 123 - ".Step"
Cohesion: 0.10
Nodes (16): DriverProfile, IReadOnlyList, JunctionControlKind, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord (+8 more)

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (48): BoxCollider, Collider, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody, Stopwatch (+40 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.16
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (19): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+11 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.08
Nodes (21): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+13 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.07
Nodes (35): CompiledRoadModel, HashSet, JunctionSnapshot, List, RoadId, TrafficFrame, TrafficV2StepCost, TrafficV2StepRunner (+27 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (11): Difficulty, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, Difficulty, Easy (+3 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.18
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.05
Nodes (72): RoadLocalizationProfile, JunctionMovement, RoadModelCanonicalPayload, DrivabilityProfile, ImportManifest, ImportManifestEntry, JunctionControl, JunctionControlKind (+64 more)

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
Nodes (50): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference (+42 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "AutomatedPairClassification"
Cohesion: 0.20
Nodes (7): AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, CompiledJunctionMovement, Vector3

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.07
Nodes (29): IReadOnlyList, RoadId, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest (+21 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "LongitudinalArbitration.cs"
Cohesion: 0.10
Nodes (26): IReadOnlyList, RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalMemory, None, LongitudinalObstacle (+18 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.08
Nodes (21): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite, SpeedConstraint, AnticipatedDeceleration (+13 more)

### Community 147 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 148 - "ImportContext"
Cohesion: 0.18
Nodes (9): DrivabilityProfile, RoadBoundsBox, RoadCurvePoint, RoadModelSource, RoadModelValidationProfile, Vector3, CircleFit, ImportContext (+1 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - ".Obstacles"
Cohesion: 0.22
Nodes (7): Vector3, ObstacleFact, InSweptPath, Bounds, CompiledRoadModel, RoadBoundsBox, Vector3

### Community 153 - "ReferenceTrack"
Cohesion: 0.14
Nodes (14): Quaternion, RoadKinematicAnchor, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound (+6 more)

### Community 154 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.13
Nodes (11): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority (+3 more)

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "NetworkPlayerRegistry"
Cohesion: 0.27
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.08
Nodes (30): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+22 more)

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 160 - "PassengerActionIntent"
Cohesion: 0.25
Nodes (5): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, INetworkSerializable

### Community 161 - ".Accumulate"
Cohesion: 0.36
Nodes (3): Collision, Transform, V2ContactEpisode

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.10
Nodes (19): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+11 more)

### Community 167 - "CampaignTraceability"
Cohesion: 0.09
Nodes (21): Dictionary, HashSet, IReadOnlyDictionary, CampaignTraceability, Elements, ElementStatus, Measured, NotMeasured (+13 more)

### Community 168 - ".Build"
Cohesion: 0.36
Nodes (3): ConflictZone, Dictionary, Dictionary

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (32): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+24 more)

### Community 170 - "RoadModelCompilationException"
Cohesion: 0.29
Nodes (4): IReadOnlyList, RoadModelCompilationException, Issues, Exception

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 172 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 173 - ".SubmitPoseRpc"
Cohesion: 0.43
Nodes (3): Rpc, RpcParams, Vector3

### Community 174 - "TrafficV2Code"
Cohesion: 0.07
Nodes (27): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+19 more)

### Community 175 - "ProjectionKey"
Cohesion: 0.33
Nodes (3): ProjectionKey, IEquatable, ProjectionKey

### Community 176 - "MovementRole"
Cohesion: 0.33
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 177 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 180 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - ".Compile"
Cohesion: 0.16
Nodes (8): DrivabilityProfile, RoadModelVersion, High, IsEmpty, Low, SchemaVersion, Comparison, RoadModelCompiler

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.06
Nodes (24): VehicleDamageType, Brake, Engine, Wheel, SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App (+16 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.18
Nodes (7): Collider, FixedString32Bytes, GameObject, NetworkedPlayerPresentation, CharacterCatalog, RenderedCharacterId, VisualInstance

### Community 216 - "LaneGraph"
Cohesion: 0.11
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.17
Nodes (12): LaneSide, AdjacentOccupantFact, Comparison, IReadOnlyList, List, RoadId, Context, FrontDistance (+4 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1155 isolated node(s):** `Index`, `Model`, `Current`, `FrameFailures`, `Batches` (+1150 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1685 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **47 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `RoadRage.Features.Vehicles.Traffic.Migration`, `GateAEvidenceParameters`, `JunctionActorReport`, `LongitudinalArbitration.cs`, `Blocker`, `RoutePlan`, `AgentObservation`, `ReferenceTrack`, `HostOwnedNetworkStateBehaviour`, `TrafficV2Insertion`, `MotionPlan`, `.Accumulate`, `V2FallbackReason`, `TrafficDecisionProjection`, `TrafficV2HazardCollector`, `PerceivedObstacleKind`, `InterStepResult`, `LongitudinalDecision`, `.Step`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.207) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `MonoBehaviour`, `MeasurementRun`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `LaneGraph`, `.Step`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.177) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `RoadModelRecords.cs`, `.TryGetMovement`, `TrafficV2HazardCollector`, `RoadBoundsBox`, `RoadGeometryValidator`, `RoadModelDocument`, `ProjectionKey`, `RoadModelCanonicalWriter`, `Blocker`, `.Summarize`, `RoadLineage`, `TrafficV2Insertion`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.137) - this node is a cross-community bridge._
- **What connects `Index`, `Model`, `Current` to the rest of the system?**
  _1155 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10931174089068826 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Migration` be split into smaller, more focused modules?**
  _Cohesion score 0.12473118279569892 - nodes in this community are weakly interconnected._
- **Should `GateAEvidenceParameters` be split into smaller, more focused modules?**
  _Cohesion score 0.11594202898550725 - nodes in this community are weakly interconnected._