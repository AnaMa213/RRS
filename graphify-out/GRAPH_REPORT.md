# Graph Report - RRS  (2026-10-06)

## Corpus Check
- 177 files · ~238,863 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 4564 nodes · 10419 edges · 214 communities (182 shown, 31 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 454 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `593b8630`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedVehicleSeatService
- .Refine
- TrafficV2Composition.cs
- GreyboxAssetSeedMetadata
- SweepPose
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
- List
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
- LongitudinalDecision
- .EvaluateKinematic
- IPathGeometry
- MotionPlan
- JunctionActorReport
- PairReviewModel
- .Measure
- TrafficDriveOutcome
- .Regenerate
- MainMenuScreen
- RoadRage.Features.Vehicles.Traffic.Migration
- ImportContext
- DefinitionId
- PlayerProfile
- ImportedCurve
- NetworkedAIVehicleState
- .Sha256Hex
- PairReviewEntry
- .FixedUpdate
- NpcReactionEffect
- .HandleLifecycleChanged
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
- RoadId
- UserNotice
- CharacterCatalog
- TrafficLongitudinalOutcome
- .CheckVisuals
- .FingerprintWithInputs
- NetworkedPlayerState
- KinematicOffsetBounds
- .HandleRosterChanged
- JunctionCoordinator
- ConflictSweep
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
- .Create
- PerceivedObstacleKind
- JunctionRequestRejection
- .UpdateSteeringState
- RageRoadEventFlowController
- MenuCharacterPreview
- JunctionDistances
- CampaignTraceability
- RoadGeometryValidator
- .CheckControlKindsAndLines
- RouteReason
- RoadModelVersion
- ReferenceTrack
- JunctionTraversal
- TrafficJunctionOutcome
- VehicleClassMask
- TrafficSettingsDef
- .Assemble
- DriverProfile
- StatusFilter
- .Evaluate
- RoadRage.Features.Vehicles.Traffic.Planning
- MainMenuFlowController
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- LocalVoidRespawnController
- RoadRageBootstrap
- TrafficV2StepRunner
- LobbyRosterScreen
- RunEscapeMenuFlowController
- RoadId
- SpeedPlan
- AIVehicleBehaviorDebugView
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- .Manifest
- LeafState
- RoadModelDocument
- PassengerActionIntent
- LaneNode
- TrafficV2Settings
- RoadModelCanonicalWriter
- SpeedConstraint
- RunEscapeMenuScreen
- .BuildSmoothCurve
- .Step
- PlayerProfileBootstrapService
- NetworkedVehicleSeatIntent
- InterStepResult
- TrackingMeasurement.cs
- VehicleCoverage
- .Read
- RoadModelCompilationException
- TrafficV2Insertion
- PathIssue
- NetworkedPlayerLifecycleService
- .Configure
- EffectiveLaneCorridor
- RoadClass
- RoutePath
- AutomatedPairClassification
- CompiledJunctionControl
- NetworkedPassengerActionIntent
- ElementTrace
- SpeedPlanIssue
- V2FallbackReason
- HostOwnedNetworkStateBehaviour
- NetworkedPlayerLifecycleIntent
- .TryGetCorridor
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
- .Compile
- PairRelation
- CompiledJunctionMovement
- RoadRage.Shared.Domain
- CompiledRoadModel
- ConflictDecision
- LobbyCodeClipboard
- Dictionary
- HashSet
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
- LaneGraph
- TrafficPerception
- DriverProfileDef
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 107 edges
2. `RunFlowController` - 99 edges
3. `ConflictSweep` - 72 edges
4. `ImportContext` - 67 edges
5. `NetworkedVehicleState` - 67 edges
6. `AuthoredRoadModel` - 66 edges
7. `CompiledRoadModel` - 64 edges
8. `RoadId` - 62 edges
9. `TrafficFrame` - 59 edges
10. `NetworkedVehicleDriverController` - 58 edges

## Surprising Connections (you probably didn't know these)
- `RoadModelCanonicalPayload` --references--> `EffectiveLaneCorridor`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCanonicalWriter.cs → Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs
- `Query` --references--> `CompiledRoadModel`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs → Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs
- `RoadModelCanonicalPayload` --references--> `DrivabilityProfile`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCanonicalWriter.cs → Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs
- `RoadModelSource` --references--> `DrivabilityProfile`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs → Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs
- `AppliedWidth` --references--> `WidthApplication`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs

## Import Cycles
- None detected.

## Communities (214 total, 31 thin omitted)

### Community 0 - "NetworkedVehicleSeatService"
Cohesion: 0.20
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (17): ConflictKind, IList, List, RoadId, RoadModelValidationProfile, StringBuilder, Vector2, MovementSide (+9 more)

### Community 2 - "TrafficV2Composition.cs"
Cohesion: 0.11
Nodes (23): IReadOnlyList, CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label (+15 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepPose"
Cohesion: 0.17
Nodes (13): IReadOnlyList, List, RoadCurve, RoadCurveSample, SweepElement, EndS, Length, StartS (+5 more)

### Community 5 - ".Core"
Cohesion: 0.23
Nodes (13): Dictionary, HashSet, List, Portal, RoadElementKind, RoadId, Edge, Node (+5 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.09
Nodes (18): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+10 more)

### Community 7 - ".Localize"
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

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
Cohesion: 0.15
Nodes (6): GameObject, StringBuilder, TMP_Text, RunCheckpointHudScreen, RectTransform, TextMeshProUGUI

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 14 - "NetworkedVehicleDamageVfxController"
Cohesion: 0.16
Nodes (7): Color, Quaternion, Renderer, Transform, Vector3, NetworkedVehicleDamageVfxController, ParticleSystem

### Community 15 - "LobbyRosterService"
Cohesion: 0.09
Nodes (15): Difficulty, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby, LitterThrowerCount, Members, OwnerId (+7 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.09
Nodes (21): Camera, CharacterController, CinemachineCamera, CinemachineOrbitalFollow, Quaternion, Vector2, Vector3, LocalOnFootController (+13 more)

### Community 17 - "Blocker"
Cohesion: 0.11
Nodes (19): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+11 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.09
Nodes (18): List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue, RageTuningDef, FearSensitivity (+10 more)

### Community 19 - ".Build"
Cohesion: 0.22
Nodes (9): JunctionMovementPosition, ConditionalWeakTable, DriverProfile, IReadOnlyList, RoadId, Vector3, JunctionRequestBuilder, RoadElementKind (+1 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 23 - "AgentObservation"
Cohesion: 0.13
Nodes (23): Func, LaneSide, RoadId, RoadLocation, StringBuilder, Vector3, VehicleFootprint, AdjacentOccupantFact (+15 more)

### Community 24 - "AuthoredRoadModel"
Cohesion: 0.06
Nodes (39): CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, GateAEvidenceParameters, HashSet, IReadOnlyList (+31 more)

### Community 25 - "List"
Cohesion: 0.18
Nodes (9): Collider, List, Transform, Vector3, JunctionClearanceSurface, Surface, Colliders, Name (+1 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.09
Nodes (23): CompiledRoadModel, Dictionary, HashSet, IList, List, PairSweep, RoadId, RoadModelValidationProfile (+15 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.07
Nodes (33): RoadId, AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed (+25 more)

### Community 29 - "JunctionSnapshot"
Cohesion: 0.11
Nodes (15): RoadId, JunctionBatchCounters, JunctionGrantStatus, Denied, Granted, Held, Released, Revoked (+7 more)

### Community 30 - "CompiledRoadModel"
Cohesion: 0.07
Nodes (25): Dictionary, List, RoadModelValidationProfile, RoadSection, CompiledRoadModel, Adjacencies, ConflictZones, Connections (+17 more)

### Community 31 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (48): ConflictKind, Dictionary, IList, JunctionControlKind, List, RoadBoundsBox, RoadCurveSample, RoadId (+40 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (32): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+24 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.11
Nodes (20): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+12 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.07
Nodes (18): Action, Collider, Collision, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody, Rpc (+10 more)

### Community 35 - "JunctionClearance"
Cohesion: 0.23
Nodes (5): IReadOnlyList, Vector2, JunctionClearance, Vector2, PoseGrid

### Community 36 - "RoutePlan"
Cohesion: 0.10
Nodes (19): IReadOnlyList, RoadModelVersion, RoutePlan, Diagnostics, DistanceMeters, ExitPortalId, ModelId, ModelVersion (+11 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "RoundaboutClearance"
Cohesion: 0.14
Nodes (13): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadId (+5 more)

### Community 40 - "PathHorizon"
Cohesion: 0.11
Nodes (24): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, RoadElementKind, RoadId, HorizonEnd, ExitPortal (+16 more)

### Community 41 - "LobbyRoomService"
Cohesion: 0.14
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 42 - "JunctionReason"
Cohesion: 0.11
Nodes (19): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+11 more)

### Community 43 - "RoadCurve"
Cohesion: 0.11
Nodes (13): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+5 more)

### Community 44 - "LongitudinalDecision"
Cohesion: 0.05
Nodes (58): DriverProfile, IReadOnlyList, List, RoadId, JunctionEntryInput, Active, LongitudinalArbitration, LongitudinalCandidate (+50 more)

### Community 45 - ".EvaluateKinematic"
Cohesion: 0.18
Nodes (11): CompiledRoadModel, Dictionary, IList, RoadBoundsBox, RoadId, RoadModelValidationProfile, Vector3, PairSweep (+3 more)

### Community 46 - "IPathGeometry"
Cohesion: 0.23
Nodes (5): Vector3, Vector3, IPathGeometry, LengthMeters, Spans

### Community 47 - "MotionPlan"
Cohesion: 0.15
Nodes (15): VehicleCoverage, DrivabilityProfile, IReadOnlyList, MotionPlan, Diagnostics, Evidence, GeometricallyFeasible, Issue (+7 more)

### Community 48 - "JunctionActorReport"
Cohesion: 0.09
Nodes (21): IReadOnlyList, Vector3, JunctionActorReport, Approaches, Corners, ElementId, HasRequest, Localized (+13 more)

### Community 49 - "PairReviewModel"
Cohesion: 0.14
Nodes (13): CompiledRoadModel, Dictionary, List, RoadBoundsBox, RoadId, StringBuilder, PairReview, PairReviewModel (+5 more)

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

### Community 54 - "RoadRage.Features.Vehicles.Traffic.Migration"
Cohesion: 0.17
Nodes (5): RoadBoundsBox, Vector3, HistoricalPairFingerprintRecord, HistoricalPairFingerprintTable, RoadRage.Features.Vehicles.Traffic.Migration

### Community 55 - "ImportContext"
Cohesion: 0.24
Nodes (7): Dictionary, JunctionFeature, Predicate, ImportContext, ImportedJunction, V1Edge, Key

### Community 56 - "DefinitionId"
Cohesion: 0.14
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 57 - "PlayerProfile"
Cohesion: 0.11
Nodes (13): PersistentPlayerProfileRecord, PlayerProfile, CharacterId, DisplayName, Exception, PlayerProfileFileStore, DefaultFilePath, FilePath (+5 more)

### Community 58 - "ImportedCurve"
Cohesion: 0.24
Nodes (6): List, RoadCurve, RoadCurveSample, ImportedCurve, ImportedSection, CircleFit

### Community 59 - "NetworkedAIVehicleState"
Cohesion: 0.44
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 60 - ".Sha256Hex"
Cohesion: 0.11
Nodes (11): Action, IList, MenuItem, SignoffLayout, MenuItem, MenuItem, Func, DateTime (+3 more)

### Community 61 - "PairReviewEntry"
Cohesion: 0.19
Nodes (8): PairDecisionState, Confirmed, Missing, Orphan, Stale, Unconfirmed, PairReviewActions, PairReviewEntry

### Community 62 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 63 - "NpcReactionEffect"
Cohesion: 0.11
Nodes (13): TMP_Text, RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel (+5 more)

### Community 65 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 66 - "TrackPiece"
Cohesion: 0.18
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 67 - "FacepunchSteamLobbyPlatform"
Cohesion: 0.07
Nodes (24): Difficulty, Task, FacepunchSteamLobbyPlatform, Task, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId, Success (+16 more)

### Community 68 - "NetworkedRunSessionMonitor"
Cohesion: 0.18
Nodes (4): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice

### Community 69 - "PassengerActionVerdictCode"
Cohesion: 0.07
Nodes (27): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+19 more)

### Community 70 - ".Compute"
Cohesion: 0.35
Nodes (6): BinaryWriter, IReadOnlyList, RoadCurveSample, RoadId, RoadModelValidationProfile, PairGeometryFingerprint

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (26): RoadElementKind, RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus (+18 more)

### Community 72 - "ElementOccupant"
Cohesion: 0.08
Nodes (30): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+22 more)

### Community 73 - "ObservationChannel"
Cohesion: 0.17
Nodes (12): IReadOnlyList, ObservationChannel, Items, RangeMeters, Saturated, Status, Total, PerceptionStatus (+4 more)

### Community 74 - "TrafficV2HazardCollector"
Cohesion: 0.07
Nodes (35): Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState, ProfilerMarker (+27 more)

### Community 75 - "RoadId"
Cohesion: 0.25
Nodes (9): ConflictKind, IReadOnlyList, RoadBoundsBox, RoadId, CompiledConflictZone, CompiledSignalGroup, CompiledSignalPhase, CompiledSignalPlan (+1 more)

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "CharacterCatalog"
Cohesion: 0.21
Nodes (6): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count

### Community 78 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.24
Nodes (6): Surface, HashSet, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - ".FingerprintWithInputs"
Cohesion: 0.33
Nodes (5): IEnumerable, StringBuilder, Component, Mesh, MeshFilter

### Community 81 - "NetworkedPlayerState"
Cohesion: 0.15
Nodes (11): FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState, PlayerMode, Driver, OnFoot, OnFootRageRoad (+3 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.13
Nodes (15): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+7 more)

### Community 83 - ".HandleRosterChanged"
Cohesion: 0.24
Nodes (5): Difficulty, Difficulty, Easy, Hard, Normal

### Community 84 - "JunctionCoordinator"
Cohesion: 0.15
Nodes (20): CompiledRoadModel, Dictionary, IReadOnlyList, JunctionRecord, JunctionSnapshot, List, RoadId, Grant (+12 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.14
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.20
Nodes (10): JunctionRecord, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, Document, HistoricalMovement (+2 more)

### Community 88 - "LobbyFlowController"
Cohesion: 0.09
Nodes (13): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, Difficulty, MatchSettings, AiVehicleTargetCount (+5 more)

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
Nodes (41): CompiledJunctionControl, CompiledRoadModel, Dictionary, IReadOnlyList, List, RoadCurveSample, RoadId, Vector3 (+33 more)

### Community 93 - ".Measure"
Cohesion: 0.16
Nodes (14): BoxCollider, CompiledRoadModel, GameObject, RoadId, RoadModelValidationProfile, Scene, VehiclePhysicsBody, VehicleProfile (+6 more)

### Community 94 - "V1Node"
Cohesion: 0.10
Nodes (27): Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List, Quaternion (+19 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.15
Nodes (12): CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, Portal, RoadBoundsBox, RoadId, Vector3 (+4 more)

### Community 96 - "TrackingTolerance"
Cohesion: 0.36
Nodes (5): TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.11
Nodes (20): OverlayInstance, Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2 (+12 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 102 - ".Create"
Cohesion: 0.29
Nodes (6): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3, DevVehicleSpawner

### Community 103 - "PerceivedObstacleKind"
Cohesion: 0.29
Nodes (6): PerceivedObstacleKind, Obstacle, Pedestrian, TrafficActor, Vehicle, WalkingPlayer

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
Cohesion: 0.30
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.18
Nodes (11): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, RoadCurveSample (+3 more)

### Community 111 - ".CheckControlKindsAndLines"
Cohesion: 0.24
Nodes (9): IReadOnlyList, List, RoadCurveSample, RoadLineSegment, Vector3, StopLineProjection, JunctionControl, JunctionMovement (+1 more)

### Community 112 - "RouteReason"
Cohesion: 0.12
Nodes (19): RouteDiagnostic, None, ZeroWeightFallback, RouteOutcome, InvalidInput, NoRoute, Planned, Replanned (+11 more)

### Community 113 - "RoadModelVersion"
Cohesion: 0.32
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 114 - "ReferenceTrack"
Cohesion: 0.19
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 115 - "JunctionTraversal"
Cohesion: 0.12
Nodes (17): JunctionApproach, Crossed, JunctionExitAssessment, JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant (+9 more)

### Community 116 - "TrafficJunctionOutcome"
Cohesion: 0.22
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 117 - "VehicleClassMask"
Cohesion: 0.15
Nodes (13): LaneCorridor, RoadSurface, Asphalt, Concrete, Dirt, Gravel, VehicleClassMask, All (+5 more)

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - ".Assemble"
Cohesion: 0.20
Nodes (6): DrivabilityProfile, RoadBoundsBox, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, V1RoadModelImporter

### Community 120 - "DriverProfile"
Cohesion: 0.12
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - ".Evaluate"
Cohesion: 0.14
Nodes (13): DriverProfile, PlanningReach, IReadOnlyList, ProfilerMarker, PlanningRequest, PlanningSpine, CompiledRoadModel, RoadId (+5 more)

### Community 123 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.12
Nodes (12): TrafficHazardCollectorCounters, TrafficV2Work, TrafficV2WorkCounters, PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Planning (+4 more)

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (51): BoxCollider, Collider, Collision, Dictionary, DriverProfileDef, Portal, ProfilerMarker, Rigidbody (+43 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.18
Nodes (6): IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.09
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "LocalVoidRespawnController"
Cohesion: 0.25
Nodes (5): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead

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

### Community 133 - "RoadId"
Cohesion: 0.05
Nodes (73): JunctionMovement, RoadModelCanonicalPayload, Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile (+65 more)

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

### Community 139 - ".Manifest"
Cohesion: 0.28
Nodes (7): IList, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys, ImportManifestEntry

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.08
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "PassengerActionIntent"
Cohesion: 0.25
Nodes (5): FixedString32Bytes, NetworkObjectReference, PassengerActionIntent, BufferSerializer, INetworkSerializable

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "TrafficV2Settings"
Cohesion: 0.40
Nodes (5): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, PerceptionLimits

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "SpeedConstraint"
Cohesion: 0.08
Nodes (21): DrivabilityProfile, DriverProfile, RoadCurve, Vector3, MotionCommand, IsFinite, SpeedConstraint, AnticipatedDeceleration (+13 more)

### Community 147 - "RunEscapeMenuScreen"
Cohesion: 0.29
Nodes (3): Button, RunEscapeMenuScreen, IsOpen

### Community 148 - ".BuildSmoothCurve"
Cohesion: 0.29
Nodes (3): RoadCurvePoint, Vector3, CircleFit

### Community 149 - ".Step"
Cohesion: 0.13
Nodes (14): DriverProfile, IReadOnlyList, JunctionSnapshot, List, RoadId, VehicleDriveIntent, V2DriveRecord, V2InteractionRecord (+6 more)

### Community 150 - "PlayerProfileBootstrapService"
Cohesion: 0.18
Nodes (8): PlayerNameValidator, PlayerProfileBootstrapService, PlayerProfileResolution, Error, IsResolved, Profile, ShouldPersist, PlayerProfileResolution

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "InterStepResult"
Cohesion: 0.25
Nodes (8): IReadOnlyList, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees

### Community 153 - "TrackingMeasurement.cs"
Cohesion: 0.29
Nodes (8): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, PieceBound, TrackingMeasurement

### Community 154 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - "RoadModelCompilationException"
Cohesion: 0.29
Nodes (4): IReadOnlyList, RoadModelCompilationException, Issues, Exception

### Community 157 - "TrafficV2Insertion"
Cohesion: 0.09
Nodes (25): CompiledRoadModel, DriverProfile, GameObject, Portal, Quaternion, RoadId, RoadLocation, Vector3 (+17 more)

### Community 158 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 160 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 161 - "EffectiveLaneCorridor"
Cohesion: 0.40
Nodes (6): RoadCurve, RoadCurveSample, CompiledJunctionMovement, EffectiveLaneCorridor, RoadSurface, VehicleClassMask

### Community 162 - "RoadClass"
Cohesion: 0.33
Nodes (6): RoadClass, Arterial, Highway, Local, Service, Unspecified

### Community 163 - "RoutePath"
Cohesion: 0.21
Nodes (8): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - "AutomatedPairClassification"
Cohesion: 0.22
Nodes (8): AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, CompiledJunctionMovement, PairRefinement, ZoneTyping

### Community 165 - "CompiledJunctionControl"
Cohesion: 0.50
Nodes (3): JunctionControlKind, RoadLineSegment, CompiledJunctionControl

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.09
Nodes (21): Func, Rpc, RpcParams, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive (+13 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - "SpeedPlanIssue"
Cohesion: 0.40
Nodes (5): SpeedPlanIssue, InvalidInput, None, PlanInfeasible, ProfileRefused

### Community 169 - "V2FallbackReason"
Cohesion: 0.07
Nodes (32): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None (+24 more)

### Community 170 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.10
Nodes (16): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel (+8 more)

### Community 171 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 173 - "ReferenceCoverage"
Cohesion: 0.40
Nodes (5): ReferenceCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 178 - "GateAEvidenceResult"
Cohesion: 0.14
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

### Community 190 - ".Compile"
Cohesion: 0.13
Nodes (15): AdjacencyDto, DrivabilityProfile, Comparison, RoadModelCompiler, ModelDto, ConnectionDto, ControlDto, CorridorDto (+7 more)

### Community 191 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (19): SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run, RoadRage.Features.OnFoot (+11 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.14
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.13
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 216 - "LaneGraph"
Cohesion: 0.12
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.14
Nodes (14): Bounds, Comparison, CompiledRoadModel, IReadOnlyList, List, RoadBoundsBox, RoadId, Vector3 (+6 more)

### Community 249 - "DriverProfileDef"
Cohesion: 0.31
Nodes (5): DriverProfileDef, Id, Profile, RawId, ScriptableObject

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1141 isolated node(s):** `SchemaVersion`, `High`, `Low`, `IsEmpty`, `ModelId` (+1136 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1659 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **31 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrackingTolerance`, `TrafficV2StepRunner`, `RoutePlan`, `GateAEvidenceParameters`, `TrafficDecisionProjection`, `ElementOccupant`, `V2FallbackReason`, `TrafficV2HazardCollector`, `HostOwnedNetworkStateBehaviour`, `LongitudinalDecision`, `JunctionActorReport`, `Blocker`, `ReferenceTrack`, `.Step`, `AgentObservation`, `TrackingMeasurement.cs`, `TrafficV2Insertion`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.183) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2Composition.cs`, `TrafficV2StepRunner`, `RageRoadEventFlowController`, `MonoBehaviour`, `TrafficV2Code`, `TrafficV2VehicleDriver`, `TrafficSettingsDef`, `.Step`, `LaneGraph`, `TrafficV2Insertion`?**
  _High betweenness centrality (0.178) - this node is a cross-community bridge._
- **Why does `RoadRage.Features.Vehicles.Traffic.Migration` connect `RoadRage.Features.Vehicles.Traffic.Migration` to `.Refine`, `TrafficV2Composition.cs`, `RoundaboutClearance`, `RoadRage.Features.Vehicles.Traffic.Planning`, `.EvaluateKinematic`, `V1ImportResult`, `KinematicOffsetBounds`, `PairReviewEntry`, `.Regenerate`, `MigrationReport`, `.Measure`, `AuthoredRoadModel`, `AutomatedPairDecisionPolicy`, `.Read`, `RoadLineage`, `.Measure`, `V1Node`, `AuthoringDecisions`?**
  _High betweenness centrality (0.110) - this node is a cross-community bridge._
- **What connects `SchemaVersion`, `High`, `Low` to the rest of the system?**
  _1141 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.11428571428571428 - nodes in this community are weakly interconnected._
- **Should `TrafficV2Composition.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.10541310541310542 - nodes in this community are weakly interconnected._
- **Should `GateAEvidenceParameters` be split into smaller, more focused modules?**
  _Cohesion score 0.08735632183908046 - nodes in this community are weakly interconnected._