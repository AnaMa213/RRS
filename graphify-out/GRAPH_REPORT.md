# Graph Report - RRS  (2026-10-05)

## Corpus Check
- 174 files · ~233,536 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4428 nodes · 10205 edges · 203 communities (180 shown, 23 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 446 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `3416e754`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PairRefinement
- .Refine
- TrafficV2Composition.cs
- GreyboxAssetSeedMetadata
- SweepElement
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
- AuthoredRun
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
- .Parse
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
- AuthoringDecisions
- ConflictSweep
- IPathGeometry
- MotionPlan
- JunctionActorReport
- HistoricalMovementReader
- .Decide
- TrafficDriveOutcome
- .Regenerate
- MainMenuScreen
- ObservationChannel
- .ImportLaneModule
- List
- .Draw
- ImportedCurve
- .Summarize
- .FullPath
- AIVehicleBehaviorDebugView
- .FixedUpdate
- TrafficV2HazardCollector
- NetworkedAIVehicleState
- NetworkPlayerRegistry
- .Evaluate
- LobbyRosterService
- NetworkedRunSessionMonitor
- InterStepResult
- .Compute
- TrafficDecisionProjection
- ElementOccupant
- RageTuningDef
- .Assemble
- CampaignTraceability
- UserNotice
- .CheckVisuals
- TrafficLongitudinalOutcome
- PassengerActionVerdictCode
- PairReview
- PlayerProfile
- KinematicOffsetBounds
- MainMenuProfileFlowController
- JunctionCoordinator
- RageTuningCatalog
- PlayerMode
- StopHoldPhase
- LobbyFlowController
- RoadModelValidationIssue
- RefinementInputs
- .Track
- RoadLineage
- DefinitionId
- V1Node
- JunctionConflictIndex
- TrafficActorInput
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- PairReviewEntry
- .Create
- DispositionKind
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- TrafficActor
- RoadGeometryValidator
- NetworkedBossState.cs
- NetworkedVehicleState
- ReferenceTrack
- MatchSettings
- JunctionExitBound
- TrafficJunctionOutcome
- NetworkedCrewEconomyState.cs
- TrafficSettingsDef
- .Manifest
- DriverProfile
- LeafState
- RouteReason
- RoadRage.Features.Vehicles.Traffic.Planning
- MainMenuFlowController
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
- TrafficV2WorkCounters.cs
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- IHostOwnedRuntimeState
- NetworkedPlayerState
- RoadModelDocument
- RageDisposition
- LaneNode
- PassengerActionIntent
- RoadModelCanonicalWriter
- SpeedConstraint
- RunEscapeMenuScreen
- ImportContext
- .Step
- PlanningDecision
- NetworkedVehicleSeatIntent
- CharacterCatalog
- TrackingMeasurement.cs
- TrackPiece
- RoadRage.Features.Vehicles.Traffic.Migration
- LongitudinalDecision
- TrafficV2Insertion
- PathIssue
- NetworkedPlayerLifecycleService
- PlayerNameValidator
- .SpawnForClient
- .IsSurfaceOnlyCollision
- RoutePath
- .FromRoute
- PlayerProfileResolution
- NetworkedPassengerActionIntent
- ElementTrace
- SpeedPlanIssue
- V2FallbackReason
- NetworkedVehicleState.cs
- NetworkedPlayerLifecycleIntent
- LobbyPlayerSlotView
- PlanningReach
- TrafficV2Code
- FileLayout
- Func
- JunctionControlKind
- GateAEvidenceResult
- StatusFilter
- NetworkedPlayerReviveIntent
- RoadBoundsBox
- RoadCurveSample
- LaneGraphRouting
- CompiledJunctionMovement
- CompiledRoadModel
- HashSet
- MenuItem
- SortedDictionary
- NetworkedVehicleRecoveryIntent
- .Compile
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
4. `NetworkedVehicleState` - 67 edges
5. `ImportContext` - 67 edges
6. `AuthoredRoadModel` - 63 edges
7. `CompiledRoadModel` - 63 edges
8. `TrafficFrame` - 59 edges
9. `NetworkedVehicleDriverController` - 58 edges
10. `VehicleProfile` - 51 edges

## Surprising Connections (you probably didn't know these)
- `AppliedWidth` --references--> `WidthApplication`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs
- `PairReviewEntry` --references--> `ConflictDecision`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs
- `AuthoredRun` --references--> `AuthoringDecisions`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs
- `PairReviewModel` --references--> `PairSweep`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs
- `PairReviewModel` --references--> `ShortElement`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs

## Import Cycles
- None detected.

## Communities (203 total, 23 thin omitted)

### Community 0 - "PairRefinement"
Cohesion: 0.15
Nodes (13): ConflictKind, List, PairRefinement, RefinementOutcome, BudgetExhausted, ProvenDisjoint, Unresolved, Witness (+5 more)

### Community 1 - ".Refine"
Cohesion: 0.13
Nodes (18): IList, RoadId, RoadModelValidationProfile, SweepGraph, SweepPose, ConflictSweep, MovementSide, RefineContext (+10 more)

### Community 2 - "TrafficV2Composition.cs"
Cohesion: 0.08
Nodes (28): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+20 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.18
Nodes (12): CompiledRoadModel, Dictionary, IReadOnlyList, RoadCurve, RoadCurveSample, RoadId, ShortElement, SweepElement (+4 more)

### Community 5 - ".Core"
Cohesion: 0.18
Nodes (15): CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, RoadLocation (+7 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.12
Nodes (16): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+8 more)

### Community 7 - "RoadId"
Cohesion: 0.07
Nodes (32): Dictionary, IReadOnlyList, List, CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies (+24 more)

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
Nodes (9): Camera, Collider, GameObject, HashSet, Quaternion, Transform, Vector3, RunFlowController (+1 more)

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
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.06
Nodes (32): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+24 more)

### Community 17 - "Blocker"
Cohesion: 0.11
Nodes (19): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+11 more)

### Community 18 - "NpcReactionEffect"
Cohesion: 0.12
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 19 - "AuthoredRun"
Cohesion: 0.11
Nodes (15): CompiledRoadModel, Predicate, RoadLocation, StringBuilder, VehicleFootprint, AppliedWidth, AuthoredRun, Import (+7 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - ".Add"
Cohesion: 0.22
Nodes (11): Bounds, CompiledJunctionMovement, IReadOnlyList, RoadBoundsBox, RoadCurve, RoadId, RoadModelValidationProfile, Vector3 (+3 more)

### Community 23 - "AgentObservation"
Cohesion: 0.15
Nodes (15): Func, RoadId, RoadLocation, StringBuilder, VehicleFootprint, AgentObservation, IntentOverlapKind, ConflictZone (+7 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.15
Nodes (8): ConflictZone, Dictionary, HashSet, RoadCurveSample, RoadModelSource, SortedDictionary, AuthoredRoadModel, WidthDecision

### Community 25 - "JunctionClearance"
Cohesion: 0.17
Nodes (10): Collider, IReadOnlyList, List, Transform, Vector2, JunctionClearance, JunctionClearanceSurface, Surface (+2 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.16
Nodes (9): Dictionary, IList, RoadModelValidationProfile, AutomatedPairDecisionPlan, AutomatedPairDecisionPolicy, IDictionary, PairSweep, RefinementInputs (+1 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.14
Nodes (17): AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedConnection, ImportedPortal, ImportedSection, PublishedDisplacement (+9 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.11
Nodes (21): RoadId, JunctionBatchCounters, JunctionGrantStatus, Denied, Granted, Held, Released, Revoked (+13 more)

### Community 30 - ".Measure"
Cohesion: 0.14
Nodes (15): BoxCollider, CompiledRoadModel, GameObject, RoadId, RoadModelValidationProfile, Scene, Vector3, VehiclePhysicsBody (+7 more)

### Community 31 - ".Parse"
Cohesion: 0.08
Nodes (26): ConflictKind, AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, ConflictDecision, ConflictDecisionKind (+18 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.08
Nodes (26): CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample, RoadElementKind (+18 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.07
Nodes (31): MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing, GateAEvidenceStale, HorizonNonConforming (+23 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.08
Nodes (17): DevIndestructibleVehicle, Collider, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc, RpcParams (+9 more)

### Community 35 - "PerceivedObstacleKind"
Cohesion: 0.16
Nodes (11): Vector3, ObstacleFact, InSweptPath, PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle (+3 more)

### Community 36 - "RoutePlan"
Cohesion: 0.11
Nodes (18): IReadOnlyList, RoadId, RoadModelVersion, RoutePlan, Diagnostics, DistanceMeters, ExitPortalId, ModelId (+10 more)

### Community 37 - "NetworkedVehicleSeatService"
Cohesion: 0.15
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.14
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.10
Nodes (25): CompiledRoadModel, DrivabilityProfile, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd (+17 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.16
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.14
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - "AuthoringDecisions"
Cohesion: 0.11
Nodes (17): IList, List, AuthoringDecisions, ConflictRecord, ControlRecord, DeferredRecord, DispositionRecord, FileLayout (+9 more)

### Community 45 - "ConflictSweep"
Cohesion: 0.15
Nodes (15): IList, List, RoadBoundsBox, RoadModelValidationProfile, Vector2, Vector3, ConflictSweep, GridPath (+7 more)

### Community 46 - "IPathGeometry"
Cohesion: 0.22
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 47 - "MotionPlan"
Cohesion: 0.13
Nodes (17): VehicleCoverage, DrivabilityProfile, IReadOnlyList, LongitudinalBounds, Valid, MotionPlan, Diagnostics, Evidence (+9 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.10
Nodes (16): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, Localized (+8 more)

### Community 49 - "HistoricalMovementReader"
Cohesion: 0.12
Nodes (16): JunctionRecord, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, PairDecisionState, Confirmed (+8 more)

### Community 50 - ".Decide"
Cohesion: 0.08
Nodes (29): DriverProfile, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind (+21 more)

### Community 51 - "TrafficDriveOutcome"
Cohesion: 0.11
Nodes (16): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+8 more)

### Community 52 - ".Regenerate"
Cohesion: 0.15
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 53 - "MainMenuScreen"
Cohesion: 0.14
Nodes (9): Button, Color, GameObject, TMP_Text, CharacterOption, Primary, Secondary, MainMenuScreen (+1 more)

### Community 54 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 55 - ".ImportLaneModule"
Cohesion: 0.24
Nodes (4): Dictionary, JunctionFeature, Predicate, ImportedJunction

### Community 56 - "List"
Cohesion: 0.17
Nodes (5): IList, List, SignoffLayout, DateTime, SignoffLayout

### Community 57 - ".Draw"
Cohesion: 0.23
Nodes (8): DrivabilityProfile, IReadOnlyList, RoadCurveSample, MovementRecord, Color, IReadOnlyList, RoadCurveSample, SceneView

### Community 58 - "ImportedCurve"
Cohesion: 0.22
Nodes (9): List, RoadCurve, RoadCurveSample, ImportedCurve, MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit (+1 more)

### Community 59 - ".Summarize"
Cohesion: 0.21
Nodes (6): AutomatedPairDecisionManifest, AutomatedPairDecisionRecord, RefinedPair, AuthoredRun, ClassificationSummary, CompiledRoadModel

### Community 60 - ".FullPath"
Cohesion: 0.18
Nodes (7): Action, KeyValuePair, MenuItem, Scene, MenuItem, Func, MenuItem

### Community 61 - "AIVehicleBehaviorDebugView"
Cohesion: 0.24
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 62 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 63 - "TrafficV2HazardCollector"
Cohesion: 0.06
Nodes (41): TrafficHazardCollectorCounters, TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer, Bounds, CharacterController (+33 more)

### Community 64 - "NetworkedAIVehicleState"
Cohesion: 0.38
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 65 - "NetworkPlayerRegistry"
Cohesion: 0.16
Nodes (6): NetworkPlayerConnectionPayload, Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 66 - ".Evaluate"
Cohesion: 0.13
Nodes (19): IReadOnlyList, ProfilerMarker, PlanningRequest, PlanningSpine, CompiledRoadModel, RoadLocation, DecisionCounter, RouteDiagnostic (+11 more)

### Community 67 - "LobbyRosterService"
Cohesion: 0.04
Nodes (38): Difficulty, Task, FacepunchSteamLobbyPlatform, Difficulty, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId (+30 more)

### Community 68 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 69 - "InterStepResult"
Cohesion: 0.29
Nodes (7): InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees

### Community 70 - ".Compute"
Cohesion: 0.15
Nodes (11): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+3 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.10
Nodes (21): Bounds, Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput (+13 more)

### Community 73 - "RageTuningDef"
Cohesion: 0.11
Nodes (15): RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity, HonkChannel, HonkMagnitude, HonkRange (+7 more)

### Community 74 - ".Assemble"
Cohesion: 0.24
Nodes (5): DrivabilityProfile, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter

### Community 75 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - ".CheckVisuals"
Cohesion: 0.15
Nodes (11): Surface, HashSet, IEnumerable, Renderer, StringBuilder, VisibleFaces, Component, Mesh (+3 more)

### Community 78 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - "PassengerActionVerdictCode"
Cohesion: 0.06
Nodes (31): List, PassengerActionCatalog, Count, Version, PassengerActionDef, CooldownSeconds, DisplayName, Id (+23 more)

### Community 81 - "PlayerProfile"
Cohesion: 0.19
Nodes (8): PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.09
Nodes (20): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, IReadOnlyList, KeyValuePair, List, RoadId (+12 more)

### Community 83 - "MainMenuProfileFlowController"
Cohesion: 0.29
Nodes (4): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, PlayerProfileBootstrapService

### Community 84 - "JunctionCoordinator"
Cohesion: 0.12
Nodes (26): CompiledRoadModel, Dictionary, IReadOnlyList, JunctionRecord, JunctionSnapshot, List, RoadId, Grant (+18 more)

### Community 85 - "RageTuningCatalog"
Cohesion: 0.29
Nodes (3): List, RageTuningCatalog, Count

### Community 86 - "PlayerMode"
Cohesion: 0.16
Nodes (8): CharacterController, PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 87 - "StopHoldPhase"
Cohesion: 0.40
Nodes (5): StopHoldPhase, Entered, Holding, None, Released

### Community 88 - "LobbyFlowController"
Cohesion: 0.12
Nodes (7): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (18): RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section, SignalPlan (+10 more)

### Community 90 - "RefinementInputs"
Cohesion: 0.23
Nodes (9): List, RoadId, SweepGraph, SweepPose, ClassificationSummary, RefinementInputs, HashSet, RefinedPair (+1 more)

### Community 91 - ".Track"
Cohesion: 0.22
Nodes (6): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite

### Community 92 - "RoadLineage"
Cohesion: 0.12
Nodes (19): FileLayout, HashSet, IEnumerable, IReadOnlyList, KeyValuePair, List, RoadId, RoadRecordKind (+11 more)

### Community 93 - "DefinitionId"
Cohesion: 0.13
Nodes (12): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+4 more)

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (29): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+21 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.11
Nodes (20): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, Portal, RoadBoundsBox, RoadId, Vector3 (+12 more)

### Community 96 - "TrafficActorInput"
Cohesion: 0.17
Nodes (8): RoadKinematicAnchor, TrafficActorInput, TrackingToleranceResponse, Latched, LatchedAtStep, ProfilerMarker, TrackingTolerance, Undeclared

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
Cohesion: 0.16
Nodes (8): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters, JunctionApproach, Crossed, JunctionExitAssessment, JunctionText

### Community 109 - "TrafficActor"
Cohesion: 0.18
Nodes (8): RoadLocation, VehicleFootprintPose, TrafficActor, OccupancyExclusion, None, NotLocalized, OccupancyNotBounded, UndeclaredFootprint

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - "NetworkedBossState.cs"
Cohesion: 0.50
Nodes (3): NetworkVariable, NetworkedBossState, RoadRage.Features.Boss

### Community 112 - "NetworkedVehicleState"
Cohesion: 0.12
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 113 - "ReferenceTrack"
Cohesion: 0.21
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

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
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 122 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.17
Nodes (9): PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing, RoadRage.Features.Vehicles.Traffic.Blockers, RoadRage.Features.Vehicles.Traffic.Lifecycle (+1 more)

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (50): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, Rigidbody, Stopwatch (+42 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.17
Nodes (7): IList, List, MenuItem, Vector2, PairReviewWindow, EditorWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "MonoBehaviour"
Cohesion: 0.15
Nodes (11): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkedPlayerSpawnService, Transform, RunCompositionRoot (+3 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.11
Nodes (21): CompiledRoadModel, HashSet, IEnumerable, JunctionSnapshot, List, ProfilerMarker, RoadId, Stopwatch (+13 more)

### Community 131 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (12): Difficulty, Difficulty, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, Difficulty (+4 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.18
Nodes (5): RunEscapeMenuFlowController, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (76): JunctionMovement, RoadModelCanonicalPayload, Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile (+68 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.09
Nodes (30): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+22 more)

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (46): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, CorridorNotGroundedOnDatum, CrossVersionReference (+38 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 140 - "NetworkedPlayerState"
Cohesion: 0.39
Nodes (5): NetworkedLocalPlayerPoseReporter, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "RageDisposition"
Cohesion: 0.22
Nodes (7): RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated, Ram

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "PassengerActionIntent"
Cohesion: 0.25
Nodes (5): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, INetworkSerializable

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

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

### Community 150 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "CharacterCatalog"
Cohesion: 0.33
Nodes (4): List, CharacterCatalog, Count, ScriptableObject

### Community 153 - "TrackingMeasurement.cs"
Cohesion: 0.29
Nodes (8): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound, TrackingMeasurement

### Community 154 - "TrackPiece"
Cohesion: 0.17
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 155 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.17
Nodes (7): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations, RoadRage.Features.Vehicles.Traffic.Migration

### Community 156 - "LongitudinalDecision"
Cohesion: 0.08
Nodes (24): IReadOnlyList, LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses (+16 more)

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.09
Nodes (25): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+17 more)

### Community 158 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 161 - ".SpawnForClient"
Cohesion: 0.33
Nodes (3): NetworkObject, Transform, Vector3

### Community 163 - "RoutePath"
Cohesion: 0.21
Nodes (8): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - ".FromRoute"
Cohesion: 0.40
Nodes (3): CompiledRoadModel, IReadOnlyList, List

### Community 165 - "PlayerProfileResolution"
Cohesion: 0.40
Nodes (5): PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.11
Nodes (17): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+9 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

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
Cohesion: 0.20
Nodes (7): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, HostOwnedNetworkStateBehaviour, IsHostAuthority, NetworkBehaviour

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - ".Compile"
Cohesion: 0.07
Nodes (21): AdjacencyDto, DrivabilityProfile, RoadModelVersion, High, IsEmpty, Low, SchemaVersion, Comparison (+13 more)

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
Cohesion: 0.11
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.14
Nodes (17): LaneSide, AdjacentOccupantFact, ExitOccupancyFact, ObservationMetadata, VehicleGapFact, Comparison, CompiledRoadModel, IReadOnlyList (+9 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.36
Nodes (4): DriverProfileDef, Id, Profile, RawId

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.09
Nodes (14): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+6 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1127 isolated node(s):** `Accepted`, `Rejected`, `Following`, `ConflictProven`, `ProvenDisjoint` (+1122 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1597 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **23 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `GateAEvidenceParameters`, `Blocker`, `.Step`, `AgentObservation`, `TrackingMeasurement.cs`, `LongitudinalDecision`, `TrafficV2Insertion`, `.FromRoute`, `RoutePlan`, `V2FallbackReason`, `JunctionActorReport`, `.Decide`, `NetworkedPlayerReviveIntent`, `TrafficV2HazardCollector`, `TrafficDecisionProjection`, `ElementOccupant`, `TrafficActorInput`, `ReferenceTrack`, `RoadRage.Features.Vehicles.Traffic.Planning`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.218) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `MonoBehaviour`, `RoadRage.Shared.Domain`, `TrafficV2Composition.cs`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `.Step`, `LaneGraph`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.199) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `.Evaluate`, `.Core`, `SpeedPlan`, `RoadModelRecords.cs`, `RoadModelDocument`, `RoadGeometryValidator`, `.Localize`, `RoadLineage`, `RoadModelCanonicalWriter`, `Blocker`, `RoadModelValidationIssue`, `PortalTrafficSpawner`, `TrafficV2Insertion`, `TrafficV2HazardCollector`?**
  _High betweenness centrality (0.122) - this node is a cross-community bridge._
- **What connects `Accepted`, `Rejected`, `Following` to the rest of the system?**
  _1127 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.13229018492176386 - nodes in this community are weakly interconnected._
- **Should `TrafficV2Composition.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.0846774193548387 - nodes in this community are weakly interconnected._
- **Should `GateAEvidenceParameters` be split into smaller, more focused modules?**
  _Cohesion score 0.11594202898550725 - nodes in this community are weakly interconnected._