# Graph Report - RRS  (2026-10-07)

## Corpus Check
- 180 files · ~246,567 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4679 nodes · 10708 edges · 219 communities (189 shown, 29 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 462 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `9dabe235`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .TryGetMovement
- .Refine
- RoadRage.Features.Vehicles.Traffic.Planning
- GreyboxAssetSeedMetadata
- SweepElement
- RoutePlan
- GateAEvidenceParameters
- .Localize
- TireSample
- VehicleSuspensionModel
- SweepPose
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyJoinOutcome
- LocalOnFootController
- Blocker
- RageTuningDef
- LobbyRosterService
- MigrationReport
- VehiclePhysicsBody
- JunctionRecord
- .Draw
- .Core
- PassengerActionVerdictCode
- AutomatedPairDecisionPolicy
- VehicleArcadeAssist
- V1RoadModelImporter.cs
- PlayerProfile
- RoadId
- AutomatedPairClassification
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .FullPath
- NetworkedVehicleSeatService
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- PathHorizon
- LobbyRoomService
- JunctionReason
- RoadCurve
- RoadRage.Features.Vehicles.Traffic.Migration
- PairSweep
- TrafficV2Composition.cs
- MotionPlan
- .Evaluate
- DefinitionId
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- PairReviewModel
- V2StepRecord
- .Add
- LongitudinalArbitration.cs
- TrafficDriveOutcome
- ImportedCurve
- .Step
- CampaignTraceability
- CharacterCatalog
- NpcReactionEffect
- MonoBehaviour
- .Decide
- .FixedUpdate
- TrackPiece
- TrafficV2Insertion
- IPathGeometry
- .TrySpawnSelectedProfile
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- GateAEvidenceResult
- TrafficV2HazardCollector
- TrafficJunctionOutcome
- UserNotice
- .GapSeconds
- .HandleRosterChanged
- .CheckVisuals
- ModelDto
- NetworkedPlayerState
- KinematicOffsetBounds
- InterStepResult
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- .FingerprintWithInputs
- LobbyFlowController
- RoadModelValidationIssue
- MainMenuScreen
- LobbyPlayerSlotView
- RoadLineage
- .Measure
- V1Node
- JunctionConflictIndex
- TrafficLongitudinalOutcome
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- RouteReason
- ReferenceTrack
- .Track
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- NetworkedRunSessionMonitor
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- DriverProfileDef
- NetworkPlayerRegistry
- JunctionTraversal
- SafetyResult
- LongitudinalDecision
- JunctionClearanceResult
- TrafficSettingsDef
- RageDisposition
- DriverProfile
- StatusFilter
- JunctionRequestRejection
- .PrepareStep
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- .Run
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- MatchSettings
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- V2FallbackReason
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- TrafficV2Admission
- RoadModelCanonicalWriter
- SpeedConstraint
- PassengerActionIntent
- ImportContext
- LocalVoidRespawnController
- NetworkedVehicleSeatIntent
- HostOwnedNetworkStateBehaviour
- TrackingMeasurement.cs
- RoadModelVersion
- .Read
- LongitudinalPerception
- SpeedPlanIssue
- AIVehicleBehaviorDebugView
- NetworkedPlayerLifecycleService
- PlanningDecision
- .MeasurePath
- .Build
- RoutePath
- MainMenuFlowController
- AuthoringDecisions
- NetworkedPassengerActionIntent
- ElementTrace
- SafetyReason
- VehicleDriveIntentComposer
- RunEscapeMenuScreen
- NetworkedPlayerLifecycleIntent
- TrafficDecisionProjection.cs
- .Accumulate
- TrafficV2Code
- .Configure
- .Create
- VehicleCoverage
- JunctionSnapshot
- V2ComposerDiagnostic
- NetworkedPlayerReviveIntent
- PathIssue
- NetworkedLocalPlayerPoseReporter
- LaneGraphRouting
- OccupancyExclusion
- TrafficV2Settings
- RoadLocationFlags
- RoadElementKind
- VehicleProfile
- NetworkedVehicleRecoveryIntent
- RoadModelSource
- BoxCollider
- Collider
- RoadRage.Shared.Domain
- Collision
- Dictionary
- LobbyCodeClipboard
- V1ImportResult
- DriverProfile
- DriverProfileDef
- NetworkedPlayerPresentation
- JunctionControlKind
- JunctionSnapshot
- List
- Portal
- ProfilerMarker
- Rigidbody
- Stopwatch
- Transform
- VehicleCoverage
- VehicleFootprint
- VehicleFootprintPose
- VehiclePhysicsBody
- RouteOutcome
- RouteReason
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 109 edges
2. `RoadId` - 103 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `TrafficFrame` - 60 edges
10. `NetworkedVehicleDriverController` - 58 edges

## Surprising Connections (you probably didn't know these)
- `TrafficV2HazardCollector` --references--> `TrafficHazardCollectorCounters`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficV2VehicleDriver` --references--> `VehicleCoverage`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficV2VehicleDriver` --references--> `TrafficDecisionProjection`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `PlanningDecision` --references--> `TrafficDecisionProjection`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `V2StepRecord` --references--> `V2FallbackReason`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs → Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs

## Import Cycles
- None detected.

## Communities (219 total, 29 thin omitted)

### Community 0 - ".TryGetMovement"
Cohesion: 0.18
Nodes (11): Bounds, CompiledJunctionMovement, IReadOnlyList, RoadBoundsBox, RoadCurve, RoadId, RoadModelValidationProfile, Vector3 (+3 more)

### Community 1 - ".Refine"
Cohesion: 0.10
Nodes (18): CompiledJunctionMovement, ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2 (+10 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.11
Nodes (13): TrafficV2Work, TrafficV2WorkCounters, JunctionKinematics, Known, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics (+5 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.18
Nodes (13): CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId, ShortElement (+5 more)

### Community 5 - "RoutePlan"
Cohesion: 0.07
Nodes (37): IReadOnlyList, ProfilerMarker, PlanningRequest, PlanningSpine, CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation (+29 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - ".Localize"
Cohesion: 0.15
Nodes (17): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+9 more)

### Community 8 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "SweepPose"
Cohesion: 0.22
Nodes (7): SweepPose, Plan, IReadOnlyList, Vector2, KinematicPoseSet, PoseGrid, RoadModelValidationProfile

### Community 11 - "RunFlowController"
Cohesion: 0.07
Nodes (5): Camera, HashSet, Transform, RunFlowController, ActiveLocalPlayer

### Community 12 - "RunCheckpointHudScreen"
Cohesion: 0.12
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyJoinOutcome"
Cohesion: 0.12
Nodes (16): Task, Task, LobbyCreateOutcome, LobbyId, Success, LobbyJoinFailureReason, Expired, Failed (+8 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.08
Nodes (19): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+11 more)

### Community 19 - "LobbyRosterService"
Cohesion: 0.06
Nodes (22): Difficulty, FacepunchSteamLobbyPlatform, Difficulty, ISteamLobbyPlatform, LobbyMemberSnapshot, CharacterId, DisplayName, Ready (+14 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "JunctionRecord"
Cohesion: 0.12
Nodes (17): JunctionRecord, JunctionControlKind, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations (+9 more)

### Community 23 - ".Draw"
Cohesion: 0.33
Nodes (5): IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 24 - ".Core"
Cohesion: 0.19
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 25 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+19 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (19): CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, SortedDictionary, Vector3 (+11 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.09
Nodes (23): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+15 more)

### Community 29 - "PlayerProfile"
Cohesion: 0.11
Nodes (15): PersistentPlayerProfileRecord, PlayerNameValidator, PlayerProfile, CharacterId, DisplayName, PlayerProfileResolution, Error, IsResolved (+7 more)

### Community 30 - "RoadId"
Cohesion: 0.08
Nodes (32): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+24 more)

### Community 31 - "AutomatedPairClassification"
Cohesion: 0.40
Nodes (5): AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (32): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+24 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): DevIndestructibleVehicle, Action, Collider, Collision, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - ".FullPath"
Cohesion: 0.17
Nodes (7): Action, KeyValuePair, MenuItem, Scene, MenuItem, MenuItem, Func

### Community 36 - "NetworkedVehicleSeatService"
Cohesion: 0.20
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

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
Cohesion: 0.08
Nodes (27): CompiledRoadModel, DriverProfile, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd (+19 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.15
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.08
Nodes (25): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+17 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (13): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+5 more)

### Community 44 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.13
Nodes (13): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+5 more)

### Community 45 - "PairSweep"
Cohesion: 0.26
Nodes (7): RoadModelValidationProfile, IList, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "TrafficV2Composition.cs"
Cohesion: 0.11
Nodes (22): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+14 more)

### Community 47 - "MotionPlan"
Cohesion: 0.15
Nodes (15): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+7 more)

### Community 48 - ".Evaluate"
Cohesion: 0.28
Nodes (7): RoadId, AuthorizedContact, Valid, NearFieldSample, SafetyFilter, SafetyLimits, RoadRage.Features.Vehicles.Traffic.Safety

### Community 49 - "DefinitionId"
Cohesion: 0.10
Nodes (14): NetworkObject, Transform, Vector3, Color, GameObject, CharacterDef, DisplayName, Id (+6 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.10
Nodes (18): CompiledJunctionControl, CompiledRoadModel, ConflictZone, Dictionary, HashSet, List, Predicate, RoadCurveSample (+10 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.28
Nodes (3): IReadOnlyList, Vector2, JunctionClearance

### Community 52 - ".Regenerate"
Cohesion: 0.16
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "PairReviewModel"
Cohesion: 0.14
Nodes (13): CompiledRoadModel, Dictionary, List, RoadBoundsBox, RoadId, StringBuilder, PairReview, PairReviewModel (+5 more)

### Community 54 - "V2StepRecord"
Cohesion: 0.10
Nodes (22): IReadOnlyList, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord, V2JunctionTrace, V2StageTimings, TotalMillisecondsPerStep (+14 more)

### Community 55 - ".Add"
Cohesion: 0.20
Nodes (4): JunctionFeature, Predicate, AuthoringTask, ImportedJunction

### Community 56 - "LongitudinalArbitration.cs"
Cohesion: 0.13
Nodes (20): RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalMemory, None, LongitudinalObstacle, StopHoldPhase (+12 more)

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (17): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+9 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.15
Nodes (16): Dictionary, IReadOnlyList, KeyValuePair, List, RoadCurve, RoadCurveSample, RoadRecordKind, ImportedCurve (+8 more)

### Community 59 - ".Step"
Cohesion: 0.13
Nodes (12): JunctionActorReport, MotionCommand, DrivabilityProfile, DriverProfile, HazardQueryReport, JunctionBlockerCause, JunctionConflictIndex, JunctionSnapshot (+4 more)

### Community 60 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 61 - "CharacterCatalog"
Cohesion: 0.19
Nodes (7): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PlayerProfileBootstrapService

### Community 62 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 63 - "MonoBehaviour"
Cohesion: 0.14
Nodes (11): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkedPlayerSpawnService, Transform, RunCompositionRoot (+3 more)

### Community 64 - ".Decide"
Cohesion: 0.15
Nodes (14): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing (+6 more)

### Community 65 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 66 - "TrackPiece"
Cohesion: 0.19
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.12
Nodes (18): CompiledRoadModel, DriverProfile, Portal, Quaternion, RoadId, RoadLocation, Vector3, TrafficV2Insertion (+10 more)

### Community 68 - "IPathGeometry"
Cohesion: 0.22
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 69 - ".TrySpawnSelectedProfile"
Cohesion: 0.14
Nodes (5): CharacterController, Collider, GameObject, Quaternion, Vector3

### Community 70 - ".Compute"
Cohesion: 0.16
Nodes (10): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.08
Nodes (26): RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus, ExitPortalId (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.09
Nodes (24): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+16 more)

### Community 73 - "GateAEvidenceResult"
Cohesion: 0.15
Nodes (14): CompiledRoadModel, IReadOnlyList, GateAEvidenceBinding, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale (+6 more)

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.07
Nodes (35): Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState, ProfilerMarker (+27 more)

### Community 75 - "TrafficJunctionOutcome"
Cohesion: 0.17
Nodes (11): JunctionActorReport, TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame (+3 more)

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 78 - ".HandleRosterChanged"
Cohesion: 0.12
Nodes (11): Difficulty, Difficulty, Color, LobbyRosterEntry, DisplayName, PortraitTint, Ready, Difficulty (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.25
Nodes (5): Surface, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "ModelDto"
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 81 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+9 more)

### Community 83 - "InterStepResult"
Cohesion: 0.18
Nodes (10): CompiledRoadModel, IReadOnlyList, List, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+2 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.15
Nodes (20): CompiledRoadModel, Dictionary, HashSet, KeyValuePair, List, RoadId, Grant, JunctionCoordinator (+12 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.16
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.11
Nodes (18): JunctionRecord, RoadCurveSample, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, MovementRecord (+10 more)

### Community 87 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 88 - "LobbyFlowController"
Cohesion: 0.11
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.14
Nodes (21): JunctionMovement, JunctionControl, RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor (+13 more)

### Community 90 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.05
Nodes (43): ConditionalWeakTable, Dictionary, IReadOnlyList, JunctionControl, JunctionMovement, LaneCorridor, List, RoadId (+35 more)

### Community 93 - ".Measure"
Cohesion: 0.15
Nodes (15): BoxCollider, Collider, CompiledRoadModel, GameObject, HashSet, List, Scene, Vector3 (+7 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (30): ImportedConnection, Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List (+22 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.09
Nodes (21): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+13 more)

### Community 96 - "TrafficLongitudinalOutcome"
Cohesion: 0.12
Nodes (15): AgentObservation, Blocker, LongitudinalDecision, TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant (+7 more)

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

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.31
Nodes (6): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState, NetworkObjectReference

### Community 102 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 103 - "ReferenceTrack"
Cohesion: 0.19
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 104 - ".Track"
Cohesion: 0.22
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.11
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MenuCharacterPreview"
Cohesion: 0.18
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.18
Nodes (11): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, RoadCurveSample (+3 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.18
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - "DriverProfileDef"
Cohesion: 0.36
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 113 - "NetworkPlayerRegistry"
Cohesion: 0.27
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 114 - "JunctionTraversal"
Cohesion: 0.13
Nodes (9): TraversalComparer, JunctionText, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds (+1 more)

### Community 115 - "SafetyResult"
Cohesion: 0.13
Nodes (14): SafetyResult, Command, HazardId, NearFieldStep, PhysicsStep, Reason, Refusal, SourceFrameId (+6 more)

### Community 116 - "LongitudinalDecision"
Cohesion: 0.12
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 117 - "JunctionClearanceResult"
Cohesion: 0.31
Nodes (7): RoadId, JunctionClearanceRelief, JunctionClearanceResult, Passed, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceWitness

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 123 - ".PrepareStep"
Cohesion: 0.15
Nodes (9): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared, BodyState, ProfilerMarker, TrafficActorInput (+1 more)

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (60): AgentObservation, Blocker, LongitudinalDecision, TrafficV2VehicleDriver, Blockers, ContactEpisodes, Contacts, CurrentRoute (+52 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.13
Nodes (9): PairReviewActions, PairReviewEntry, IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow (+1 more)

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - ".Run"
Cohesion: 0.12
Nodes (9): CompiledJunctionControl, CompiledRoadModel, RoadCurveSample, IList, StringBuilder, GateABinding, SignoffLayout, DateTime (+1 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.08
Nodes (29): SignalPhaseInput, CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId (+21 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.07
Nodes (6): Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, TMP_InputField

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.18
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (68): Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile, ImportManifest, ImportManifestEntry (+60 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 135 - "MatchSettings"
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "V2FallbackReason"
Cohesion: 0.14
Nodes (14): V2FallbackReason, ExitPortalReached, FrameUnavailable, HorizonNonConforming, NoCommand, None, NonFiniteAuthority, NonFiniteCommand (+6 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (33): Func, AdjacencyDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto, GroupDto (+25 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.08
Nodes (29): IReadOnlyList, RoadId, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest (+21 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "TrafficV2Admission"
Cohesion: 0.22
Nodes (9): GameObject, TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle, TrafficV2Verdict (+1 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 147 - "PassengerActionIntent"
Cohesion: 0.17
Nodes (6): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, RoadRage.Features.PassengerActions, INetworkSerializable

### Community 148 - "ImportContext"
Cohesion: 0.26
Nodes (6): RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, CircleFit

### Community 149 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.09
Nodes (16): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel (+8 more)

### Community 153 - "TrackingMeasurement.cs"
Cohesion: 0.29
Nodes (8): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound, TrackingMeasurement

### Community 154 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "LongitudinalPerception"
Cohesion: 0.18
Nodes (11): IReadOnlyList, LongitudinalPerception, HasLeader, Leader, Obstacles, UnavailableReason, PerceptionUnavailableReason, ChannelSaturated (+3 more)

### Community 157 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 158 - "AIVehicleBehaviorDebugView"
Cohesion: 0.29
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 160 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 162 - ".Build"
Cohesion: 0.28
Nodes (8): ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId, JunctionRequestBuilder, RoadElementKind, RouteOccurrence

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 165 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (47): ConflictKind, Dictionary, FileLayout, Func, IEnumerable, IList, JunctionControlKind, List (+39 more)

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (21): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+13 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - "SafetyReason"
Cohesion: 0.22
Nodes (9): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+1 more)

### Community 169 - "VehicleDriveIntentComposer"
Cohesion: 0.16
Nodes (14): MotionCommand, VehicleDriveIntent, ComposedDrive, V2FallbackTerminal, Held, None, StopOverrun, VehicleDriveIntentComposer (+6 more)

### Community 170 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 173 - ".Accumulate"
Cohesion: 0.36
Nodes (3): V2ContactEpisode, Collision, Transform

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 175 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 176 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 177 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 178 - "JunctionSnapshot"
Cohesion: 0.18
Nodes (8): IReadOnlyList, JunctionSnapshot, JunctionBatchCounters, JunctionSnapshot, Counters, EffectiveFrame, Records, SourceFrame

### Community 179 - "V2ComposerDiagnostic"
Cohesion: 0.29
Nodes (7): V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None, ProfileScalarNonFinite, RollingBackward

### Community 180 - "NetworkedPlayerReviveIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent

### Community 181 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 183 - "LaneGraphRouting"
Cohesion: 0.24
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 184 - "OccupancyExclusion"
Cohesion: 0.40
Nodes (5): OccupancyExclusion, None, NotLocalized, OccupancyNotBounded, UndeclaredFootprint

### Community 185 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 186 - "RoadLocationFlags"
Cohesion: 0.40
Nodes (5): RoadLocationFlags, Ambiguous, None, OutsideEnvelope, WrongWay

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "RoadModelSource"
Cohesion: 0.19
Nodes (11): DrivabilityProfile, DrivabilityProfile, RoadModelCanonicalPayload, Comparison, RoadModelCompiler, BindingDto, ZoneTypeDto, RoadLocalizationProfile (+3 more)

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (18): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+10 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.24
Nodes (6): PointerEventData, TMP_Text, LobbyCodeClipboard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

### Community 197 - "V1ImportResult"
Cohesion: 0.15
Nodes (10): DrivabilityProfile, IList, RoadId, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1ImportResult, Succeeded (+2 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.14
Nodes (10): Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation, CharacterCatalog (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.13
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.05
Nodes (55): Func, IReadOnlyList, LaneSide, RoadId, RoadLocation, StringBuilder, Vector3, VehicleFootprint (+47 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1183 isolated node(s):** `FrameId`, `Observation`, `Decision`, `RoadLimits`, `Blockers` (+1178 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1714 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **29 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `TrafficV2Composition.cs`, `TrafficV2Code`, `TrafficV2Admission`, `TrafficSettingsDef`, `V2StepRecord`, `LaneGraph`, `TrafficV2VehicleDriver`, `MonoBehaviour`?**
  _High betweenness centrality (0.157) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `.TryGetMovement`, `TrafficFrame`, `TrafficV2Insertion`, `RoutePlan`, `SpeedPlan`, `.Localize`, `RoadModelRecords.cs`, `TrafficV2HazardCollector`, `RoadCurve`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadModelCanonicalWriter`, `RoadModelValidationIssue`, `AutomatedPairDecisionPolicy`, `RoadLineage`, `RoadModelSource`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.152) - this node is a cross-community bridge._
- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `TrafficDecisionProjection`, `VehicleDriveIntentComposer`, `TrafficV2HazardCollector`, `.PrepareStep`, `.Accumulate`, `V2StepRecord`, `HostOwnedNetworkStateBehaviour`, `TrafficDriveOutcome`, `.Step`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.136) - this node is a cross-community bridge._
- **What connects `FrameId`, `Observation`, `Decision` to the rest of the system?**
  _1183 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.10365853658536585 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.11083743842364532 - nodes in this community are weakly interconnected._
- **Should `RoutePlan` be split into smaller, more focused modules?**
  _Cohesion score 0.06666666666666667 - nodes in this community are weakly interconnected._