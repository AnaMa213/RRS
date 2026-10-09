# Graph Report - RRS  (2026-10-09)

## Corpus Check
- 186 files · ~259,508 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 5050 nodes · 11397 edges · 228 communities (194 shown, 33 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 497 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c321da6f`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MotionPlan
- .Refine
- RoadRage.Features.Vehicles.Traffic.Planning
- GreyboxAssetSeedMetadata
- SweepElement
- RouteOutcome
- GateAEvidenceParameters
- .Localize
- RecoverySupervisor
- VehicleSuspensionModel
- NetworkedVehicleSeatService
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyJoinOutcome
- LocalOnFootController
- Blocker
- RageTuningDef
- .Measure
- MigrationReport
- VehiclePhysicsBody
- LobbyFlowController
- LobbyRosterService
- .Core
- LobbyRoomService
- AutomatedPairDecisionPolicy
- EffectiveRuleException
- V1RoadModelImporter.cs
- CharacterCatalog
- RoadId
- CollisionFacts
- TrafficFrame
- MotionPlan.cs
- NetworkedVehicleDriverController
- .Run
- .Monitor
- NetworkedVehicleState
- VehicleProfile
- V1ImportResult
- PathHorizon
- MonoBehaviour
- JunctionReason
- RoadCurve
- .Measure
- PairSweep
- TrafficV2Composition.cs
- EffectivePolicy
- .Evaluate
- MenuCharacterPreview
- AuthoredRoadModel
- JunctionClearance
- .Regenerate
- PairReview
- .PrepareStep
- ImportedCurve
- .ImportLaneModule
- TrafficDriveOutcome
- LongitudinalDecision
- PassengerActionVerdictCode
- GridlockCycle
- TrafficV2HazardCollector
- PlayerMode
- AgentObservation
- .Decide
- .FixedUpdate
- .Create
- TrafficV2Insertion
- IPathGeometry
- ObservationSource
- .Compute
- TrafficDecisionProjection
- PerceivedObstacleKind
- PairReviewEntry
- HazardRootClass
- .Draw
- UserNotice
- TrafficV2Admission
- MotionCommand
- .CheckVisuals
- ModelDto
- JunctionTraversal
- KinematicOffsetBounds
- RoadModelSource
- JunctionCoordinator
- ConflictSweep
- HistoricalMovementReader
- NetworkPlayerRegistry
- LobbyRosterScreen
- RoadModelValidationIssue
- RageDisposition
- LobbyPlayerSlotView
- RoadLineage
- SweepPose
- V1Node
- JunctionConflictIndex
- TrafficLongitudinalOutcome
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- RoutePlan
- NetworkedPlayerState
- .FingerprintWithInputs
- .UpdateSteeringState
- RageRoadEventFlowController
- MainMenuScreen
- JunctionDistances
- V2FallbackReason
- RoadGeometryValidator
- RoadRage.Features.Vehicles.Traffic
- JunctionRequestRejection
- NetworkedPlayerLifecycleIntent
- RoadModelVersion
- CampaignTraceability
- SpeedConstraint
- LongitudinalArbitration.cs
- TrafficSettingsDef
- TrafficJunctionOutcome
- DriverProfile
- StatusFilter
- .InterStepBound
- AIVehicleBehaviorDebugView
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- SafetyResult
- RoadRageBootstrap
- TrafficV2StepRunner
- DriverProfileDef
- RunEscapeMenuFlowController
- RoadModelRecords.cs
- SpeedPlan
- RoadRage.Shared.Networking
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- CollisionReactionWeights
- LeafState
- RoadModelDocument
- JunctionActorReport
- LaneNode
- RuleExceptionReason
- RoadModelCanonicalWriter
- DefinitionId
- RouteReason
- ImportContext
- SafetyReason
- .Step
- NetworkedVehicleSeatIntent
- VehicleCoverage
- PathIssue
- .Configure
- .Read
- .Bind
- TrackPiece
- TrafficV2Settings
- NetworkedPlayerLifecycleService
- PlanningReach
- ReferenceTrack
- .Build
- RoutePath
- DrivingPolicyProfile
- AuthoringDecisions
- NetworkedPassengerActionIntent
- ElementTrace
- VehicleDriveIntent
- VehicleDriveIntentComposer
- TireSample
- RuleExceptionRequest
- DrivingPolicy.cs
- TrafficRule
- TrafficV2Code
- TrackingMeasurement.cs
- LongitudinalMemory
- LongitudinalCandidateKind
- JunctionSnapshot
- JunctionExitBound
- V2StageTimings
- .Register
- V2ComposerDiagnostic
- LaneGraphRouting
- .RenderSignoff
- MatchSettings
- .FromRoute
- PerceptionUnavailableReason
- VehicleArcadeAssist
- NetworkedVehicleRecoveryIntent
- StopHoldPhase
- CollisionGoalPhase
- RoadRage.Shared.Domain
- .Q
- RoadElementKind
- LobbyCodeClipboard
- HashSet
- JunctionRecord
- JunctionSnapshot
- NetworkedPlayerPresentation
- KeyValuePair
- GameObject
- Quaternion
- RoadLocation
- BoxCollider
- PairReviewModel
- Collider
- Collision
- JunctionSnapshot
- ProfilerMarker
- Rigidbody
- Stopwatch
- Transform
- VehicleCoverage
- VehicleDriveIntent
- LaneGraph
- TrafficPerception
- VehicleFootprint
- VehicleFootprintPose
- VehiclePhysicsBody
- DrivabilityProfile
- RoadCurve
- PerceptionLimits
- RouteOutcome
- RouteReason
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `TrafficV2VehicleDriver` - 131 edges
2. `RoadId` - 102 edges
3. `RunFlowController` - 99 edges
4. `ConflictSweep` - 72 edges
5. `ImportContext` - 67 edges
6. `NetworkedVehicleState` - 67 edges
7. `AuthoredRoadModel` - 66 edges
8. `CompiledRoadModel` - 64 edges
9. `JunctionCoordinator` - 59 edges
10. `TrafficFrame` - 59 edges

## Surprising Connections (you probably didn't know these)
- `DriverProfileDef` --references--> `DrivingPolicyProfile`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs → Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs
- `NetworkedAIVehicleDriverController` --references--> `DriverProfileDef`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs → Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs
- `TrafficV2HazardCollector` --references--> `TrafficHazardCollectorCounters`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs → Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs
- `TrafficJunctionOutcome` --references--> `JunctionActorReport`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs
- `TrafficJunctionOutcome` --references--> `JunctionBatchCounters`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs → Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs

## Import Cycles
- None detected.

## Communities (228 total, 33 thin omitted)

### Community 0 - "MotionPlan"
Cohesion: 0.09
Nodes (25): VehicleCoverage, IReadOnlyList, GateAEvidenceResult, Valid, GateAEvidenceStatus, GateAEvidenceMissing, GateAEvidenceStale, Valid (+17 more)

### Community 1 - ".Refine"
Cohesion: 0.11
Nodes (16): CompiledJunctionMovement, ConflictKind, IList, List, RoadModelValidationProfile, StringBuilder, Vector2, PairRefinement (+8 more)

### Community 2 - "RoadRage.Features.Vehicles.Traffic.Planning"
Cohesion: 0.08
Nodes (22): TrafficV2Work, TrafficV2WorkCounters, TrafficV2LifecycleState, Active, Faulted, PlanningTolerances, TacticalGoalKind, CollisionResponse (+14 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.18
Nodes (9): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability (+1 more)

### Community 4 - "SweepElement"
Cohesion: 0.16
Nodes (12): CompiledRoadModel, Dictionary, IReadOnlyList, RoadCurve, RoadCurveSample, RoadId, ShortElement, SweepElement (+4 more)

### Community 5 - "RouteOutcome"
Cohesion: 0.40
Nodes (5): RouteOutcome, InvalidInput, NoRoute, Planned, Replanned

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.10
Nodes (19): GameObject, VehiclePhysicsBody, GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic (+11 more)

### Community 7 - ".Localize"
Cohesion: 0.11
Nodes (23): ConditionalWeakTable, Dictionary, IReadOnlyList, List, RoadModelVersion, Vector3, ElementIndex, Query (+15 more)

### Community 8 - "RecoverySupervisor"
Cohesion: 0.05
Nodes (36): IReadOnlyList, List, RoadId, ProgressLedger, ActualMeters, ExpectedMeters, RecoveryAttempt, Outcome (+28 more)

### Community 9 - "VehicleSuspensionModel"
Cohesion: 0.10
Nodes (15): Vector3, TelemetrySample, Vector3, TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees (+7 more)

### Community 10 - "NetworkedVehicleSeatService"
Cohesion: 0.15
Nodes (4): Vector3, NetworkedVehicleSeatService, Instance, Vector3

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

### Community 15 - "LobbyJoinOutcome"
Cohesion: 0.09
Nodes (21): Task, Task, LobbyCreateOutcome, LobbyId, Success, LobbyJoinFailureReason, Expired, Failed (+13 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.07
Nodes (26): Quaternion, Vector3, LocalVoidRespawnController, CheckpointHud, IsDead, Camera, CharacterController, CinemachineCamera (+18 more)

### Community 17 - "Blocker"
Cohesion: 0.10
Nodes (20): RoadId, Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization (+12 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.05
Nodes (32): TMP_Text, RageStateDebugView, List, RageTuningCatalog, Count, RageThreshold, Disposition, MinValue (+24 more)

### Community 19 - ".Measure"
Cohesion: 0.15
Nodes (14): BoxCollider, CompiledRoadModel, GameObject, RoadId, RoadModelValidationProfile, Scene, VehiclePhysicsBody, VehicleProfile (+6 more)

### Community 20 - "MigrationReport"
Cohesion: 0.08
Nodes (29): CompiledRoadModel, Dictionary, IReadOnlyList, List, MenuItem, RoadCurvePoint, RoadId, RoadModelSource (+21 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.11
Nodes (14): RaycastHit, Rigidbody, TelemetrySample, TireSample, VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount (+6 more)

### Community 22 - "LobbyFlowController"
Cohesion: 0.10
Nodes (8): HashSet, NetworkPrefabsList, RoadRageBootstrap, LobbyFlowController, Settings, NetworkPlayerConnectionPayload, ConnectionApprovalRequest, ConnectionApprovalResponse

### Community 23 - "LobbyRosterService"
Cohesion: 0.07
Nodes (17): Difficulty, FacepunchSteamLobbyPlatform, Difficulty, ISteamLobbyPlatform, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty, HasLobby (+9 more)

### Community 24 - ".Core"
Cohesion: 0.19
Nodes (16): RouteResult, CompiledRoadModel, Dictionary, HashSet, List, Portal, RoadElementKind, RoadId (+8 more)

### Community 25 - "LobbyRoomService"
Cohesion: 0.16
Nodes (11): Task, LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating (+3 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.10
Nodes (20): CompiledRoadModel, Dictionary, HashSet, IList, List, RoadId, RoadModelValidationProfile, SortedDictionary (+12 more)

### Community 27 - "EffectiveRuleException"
Cohesion: 0.12
Nodes (22): Active, CompiledRoadModel, IReadOnlyList, List, RoadId, Active, EffectiveRuleException, AcceptedFrame (+14 more)

### Community 28 - "V1RoadModelImporter.cs"
Cohesion: 0.09
Nodes (24): DisplacementKind, PortalBoundaryTrim, RingAnchorShift, DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex (+16 more)

### Community 29 - "CharacterCatalog"
Cohesion: 0.07
Nodes (22): RoadRageBootstrap, MainMenuProfileFlowController, CurrentIndex, List, CharacterCatalog, Count, PersistentPlayerProfileRecord, PlayerNameValidator (+14 more)

### Community 30 - "RoadId"
Cohesion: 0.07
Nodes (29): Dictionary, IReadOnlyList, List, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones (+21 more)

### Community 31 - "CollisionFacts"
Cohesion: 0.09
Nodes (26): RoadId, CollisionAnalysis, CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, CollisionResponseRequest, Facts (+18 more)

### Community 32 - "TrafficFrame"
Cohesion: 0.07
Nodes (32): Bounds, CompiledRoadModel, Dictionary, IReadOnlyList, List, ProfilerMarker, RoadCurve, RoadCurveSample (+24 more)

### Community 33 - "MotionPlan.cs"
Cohesion: 0.09
Nodes (25): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+17 more)

### Community 34 - "NetworkedVehicleDriverController"
Cohesion: 0.08
Nodes (13): DevIndestructibleVehicle, Action, Collider, Collision, NetworkObjectReference, NetworkTransform, Quaternion, Rigidbody (+5 more)

### Community 35 - ".Run"
Cohesion: 0.14
Nodes (8): Action, KeyValuePair, MenuItem, Scene, GateABinding, MenuItem, MenuItem, Func

### Community 36 - ".Monitor"
Cohesion: 0.10
Nodes (21): DriverProfile, JunctionControlKind, List, RoadId, SpeedConstraint, SpeedPlan, V2InteractionRecord, V2JunctionTrace (+13 more)

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.11
Nodes (8): Func, NetworkVariable, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (38): Vector3, VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque (+30 more)

### Community 39 - "V1ImportResult"
Cohesion: 0.13
Nodes (16): Bounds, Collider, CompiledRoadModel, IReadOnlyList, List, RoadCurve, RoadCurveSample, RoadModelValidationProfile (+8 more)

### Community 40 - "PathHorizon"
Cohesion: 0.10
Nodes (22): CompiledRoadModel, IReadOnlyList, RoadCurve, RoadElementKind, RoadId, HorizonEnd, ExitPortal, LookAheadLimit (+14 more)

### Community 41 - "MonoBehaviour"
Cohesion: 0.13
Nodes (14): Dictionary, GameObject, HashSet, IEnumerator, NetworkManager, NetworkObject, Transform, Vector3 (+6 more)

### Community 42 - "JunctionReason"
Cohesion: 0.08
Nodes (25): JunctionReason, ActorGone, Cleared, ClearedUnlocalized, Committed, CommittedCarried, ConflictGranted, ConflictOccupied (+17 more)

### Community 43 - "RoadCurve"
Cohesion: 0.15
Nodes (12): Action, Bounds, Vector3, RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians (+4 more)

### Community 44 - ".Measure"
Cohesion: 0.13
Nodes (13): CompiledRoadModel, IReadOnlyList, List, RoadId, RoadModelValidationProfile, StringBuilder, Result, Passed (+5 more)

### Community 45 - "PairSweep"
Cohesion: 0.29
Nodes (7): IList, List, RoadBoundsBox, RoadModelValidationProfile, Vector3, PairSweep, IsCandidate

### Community 46 - "TrafficV2Composition.cs"
Cohesion: 0.12
Nodes (21): CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label, MaxPopulation (+13 more)

### Community 47 - "EffectivePolicy"
Cohesion: 0.13
Nodes (16): DriverProfile, DriverProfileDef, RoadId, DrivingPolicy, DrivingSurface, Carriageway, None, OpposingCorridor (+8 more)

### Community 48 - ".Evaluate"
Cohesion: 0.25
Nodes (8): RoadId, AuthorizedContact, Valid, NearFieldSample, SafetyFilter, SafetyLimits, RoadRage.Features.Vehicles.Traffic.Intent, RoadRage.Features.Vehicles.Traffic.Safety

### Community 49 - "MenuCharacterPreview"
Cohesion: 0.18
Nodes (11): Camera, Color, GameObject, PointerEventData, Renderer, Transform, MenuCharacterPreview, IDragHandler (+3 more)

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.07
Nodes (31): Bounds, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, ConflictZone, Dictionary, HashSet, IReadOnlyList (+23 more)

### Community 51 - "JunctionClearance"
Cohesion: 0.17
Nodes (10): Collider, IReadOnlyList, List, Vector2, Vector3, JunctionClearance, JunctionClearanceSurface, Surface (+2 more)

### Community 52 - ".Regenerate"
Cohesion: 0.14
Nodes (15): CompiledRoadModel, List, RoadId, Scene, StringBuilder, CandidateDiffEntry, CandidateDiffState, Changed (+7 more)

### Community 54 - ".PrepareStep"
Cohesion: 0.11
Nodes (11): StepContactAccumulator, TrackingToleranceResponse, Latched, LatchedAtStep, TrackingTolerance, Undeclared, BodyState, ProfilerMarker (+3 more)

### Community 55 - "ImportedCurve"
Cohesion: 0.20
Nodes (10): List, RoadCurve, RoadCurveSample, ImportedCurve, ImportedSection, MovementRole, RoundaboutContinuation, RoundaboutEntry (+2 more)

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.10
Nodes (19): IReadOnlyList, TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback (+11 more)

### Community 58 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 59 - "PassengerActionVerdictCode"
Cohesion: 0.05
Nodes (37): Rpc, RpcParams, PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId (+29 more)

### Community 60 - "GridlockCycle"
Cohesion: 0.16
Nodes (16): GridlockArc, GridlockCycle, Arcs, Key, Members, Action, Dictionary, HashSet (+8 more)

### Community 61 - "TrafficV2HazardCollector"
Cohesion: 0.09
Nodes (28): Bounds, CharacterController, Collider, Dictionary, IReadOnlyList, List, NetworkedAIVehicleState, ProfilerMarker (+20 more)

### Community 62 - "PlayerMode"
Cohesion: 0.25
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 63 - "AgentObservation"
Cohesion: 0.13
Nodes (17): Func, IReadOnlyList, RoadLocation, StringBuilder, VehicleFootprint, AgentObservation, ObservationChannel, Items (+9 more)

### Community 64 - ".Decide"
Cohesion: 0.28
Nodes (5): DriverProfile, List, LongitudinalArbitration, LongitudinalCandidate, StopHoldParameters

### Community 65 - ".FixedUpdate"
Cohesion: 0.24
Nodes (3): TireSample, Vector2, VehicleTireModel

### Community 66 - ".Create"
Cohesion: 0.33
Nodes (5): GameObject, NetworkObject, Quaternion, Rigidbody, Vector3

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.10
Nodes (21): CompiledRoadModel, DriverProfile, Portal, RoadId, RoutePlan, SpeedPlan, Vector3, TrafficV2Insertion (+13 more)

### Community 68 - "IPathGeometry"
Cohesion: 0.12
Nodes (13): Vector3, Vector3, IPathGeometry, LengthMeters, Spans, PlanningDecision, Motion, Observation (+5 more)

### Community 69 - "ObservationSource"
Cohesion: 0.50
Nodes (4): ObservationSource, PublishedHorizon, SpatialQuery, StructuredOccupancy

### Community 70 - ".Compute"
Cohesion: 0.14
Nodes (11): BinaryWriter, IReadOnlyList, RoadBoundsBox, RoadCurveSample, RoadId, RoadModelValidationProfile, Vector3, HistoricalPairFingerprintRecord (+3 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.07
Nodes (28): RoadId, TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus, ExitPortalId (+20 more)

### Community 72 - "PerceivedObstacleKind"
Cohesion: 0.07
Nodes (31): Bounds, IReadOnlyList, RoadBoundsBox, RoadElementKind, RoadId, Vector3, ElementClosureInput, ElementOccupant (+23 more)

### Community 74 - "HazardRootClass"
Cohesion: 0.14
Nodes (12): TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer, HazardRootClass, Obstacle, Self (+4 more)

### Community 75 - ".Draw"
Cohesion: 0.23
Nodes (7): DrivabilityProfile, IReadOnlyList, Color, IReadOnlyList, RoadCurveSample, RoadModelValidationProfile, SceneView

### Community 76 - "UserNotice"
Cohesion: 0.10
Nodes (13): IEnumerator, NetworkManager, NetworkedRunSessionMonitor, HostDisconnectedNotice, UserNotice, Message, Severity, UserNoticeSeverity (+5 more)

### Community 77 - "TrafficV2Admission"
Cohesion: 0.27
Nodes (8): TrafficV2Admission, Admitted, Code, Evidence, Model, TrafficV2Lifecycle, GameObject, GateAEvidenceResult

### Community 78 - "MotionCommand"
Cohesion: 0.17
Nodes (11): DriverProfile, IReadOnlyList, LongitudinalDecision, SpeedConstraint, SpeedPlan, Vector3, MotionCommand, IsFinite (+3 more)

### Community 79 - ".CheckVisuals"
Cohesion: 0.24
Nodes (6): Surface, HashSet, Renderer, VisibleFaces, MeshRenderer, VisibleFaces

### Community 80 - "ModelDto"
Cohesion: 0.17
Nodes (12): AdjacencyDto, ModelDto, ConnectionDto, ControlDto, CorridorDto, JunctionDto, ManifestDto, MovementDto (+4 more)

### Community 81 - "JunctionTraversal"
Cohesion: 0.16
Nodes (8): TraversalComparer, JunctionTraversal, ExitCorridorId, FirstMovementId, JunctionId, LastMovementId, MovementIds, IEqualityComparer

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.11
Nodes (17): CompiledRoadModel, Dictionary, DrivabilityProfile, IList, KeyValuePair, List, RoadId, ElementOffsets (+9 more)

### Community 83 - "RoadModelSource"
Cohesion: 0.15
Nodes (16): DrivabilityProfile, EffectiveLaneCorridor, RoadLocalizationProfile, RoadModelSource, RoadModelValidationProfile, JunctionMovement, RoadModelCanonicalPayload, Comparison (+8 more)

### Community 84 - "JunctionCoordinator"
Cohesion: 0.14
Nodes (25): CompiledRoadModel, Dictionary, IReadOnlyList, JunctionConflictIndex, List, RoadId, TrafficFrame, Grant (+17 more)

### Community 85 - "ConflictSweep"
Cohesion: 0.16
Nodes (13): Vector2, ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment, GridPath, LeafState (+5 more)

### Community 86 - "HistoricalMovementReader"
Cohesion: 0.11
Nodes (18): JunctionRecord, RoadCurveSample, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, MovementRecord (+10 more)

### Community 87 - "NetworkPlayerRegistry"
Cohesion: 0.27
Nodes (5): Dictionary, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 88 - "LobbyRosterScreen"
Cohesion: 0.06
Nodes (12): Difficulty, Difficulty, Button, LobbyRosterScreen, Button, TMP_Text, LobbyShellScreen, Difficulty (+4 more)

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.15
Nodes (19): JunctionControl, RoadRecordKind, Adjacency, Connection, Control, Corridor, Movement, Section (+11 more)

### Community 90 - "RageDisposition"
Cohesion: 0.17
Nodes (9): IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm, ConfrontationCapable, Flee, Irritated (+1 more)

### Community 91 - "LobbyPlayerSlotView"
Cohesion: 0.29
Nodes (4): Color, TMP_Text, LobbyPlayerSlotView, Image

### Community 92 - "RoadLineage"
Cohesion: 0.05
Nodes (46): CompiledJunctionControl, CompiledRoadModel, ConditionalWeakTable, Dictionary, IReadOnlyList, JunctionControl, JunctionMovement, LaneCorridor (+38 more)

### Community 93 - "SweepPose"
Cohesion: 0.22
Nodes (8): SweepPose, Plan, RoadId, MovementSide, IReadOnlyList, Vector2, KinematicPoseSet, PoseGrid

### Community 94 - "V1Node"
Cohesion: 0.09
Nodes (29): Predicate, Func, GameObject, IReadOnlyDictionary, IReadOnlyList, LaneGraph, LaneNode, List (+21 more)

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.11
Nodes (20): CompiledRoadModel, ConditionalWeakTable, ConflictKind, Dictionary, HashSet, IReadOnlyList, JunctionControlKind, Portal (+12 more)

### Community 96 - "TrafficLongitudinalOutcome"
Cohesion: 0.11
Nodes (16): AgentObservation, Blocker, LongitudinalDecision, TrafficHazardCollectorCounters, TrafficLongitudinalOutcome, Blockers, Collector, Decision (+8 more)

### Community 97 - "VehicleWheel"
Cohesion: 0.25
Nodes (7): Vector3, VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.12
Nodes (17): Color, HashSet, List, MenuItem, RoadId, SceneView, Vector2, ConflictFilter (+9 more)

### Community 99 - "LocalVehicleCameraRig"
Cohesion: 0.16
Nodes (9): CinemachineCamera, CinemachineOrbitalFollow, Dictionary, Quaternion, Transform, Vector3, LocalVehicleCameraRig, HasRageTargetLookOverride (+1 more)

### Community 100 - "Vector3"
Cohesion: 0.27
Nodes (6): List, Vector3, HermiteSegment, RoadCurveBuilder, SegmentWalker, HermiteSegment

### Community 101 - "NetworkedAIVehicleState"
Cohesion: 0.34
Nodes (5): IReadOnlyList, Vector3, AiRageTargetResolution, NetworkVariable, NetworkedAIVehicleState

### Community 102 - "RoutePlan"
Cohesion: 0.07
Nodes (31): IReadOnlyList, ProfilerMarker, PlanningRequest, PlanningSpine, CompiledRoadModel, IReadOnlyList, RoadId, RoadLocation (+23 more)

### Community 103 - "NetworkedPlayerState"
Cohesion: 0.16
Nodes (11): Key, Rpc, RpcParams, NetworkedPlayerReviveIntent, FixedString32Bytes, NetworkVariable, Vector3, NetworkedPlayerState (+3 more)

### Community 104 - ".FingerprintWithInputs"
Cohesion: 0.25
Nodes (6): IEnumerable, StringBuilder, Transform, Component, Mesh, MeshFilter

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.10
Nodes (15): GameObject, IReadOnlyList, NetworkObjectReference, RageRoadEventFlowController, NetworkObjectReference, NetworkVariable, NetworkedRunState, IReadOnlyList (+7 more)

### Community 107 - "MainMenuScreen"
Cohesion: 0.10
Nodes (12): RoadRageBootstrap, MainMenuFlowController, Button, Color, GameObject, TMP_Text, CharacterOption, Primary (+4 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.35
Nodes (4): DriverProfile, JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "V2FallbackReason"
Cohesion: 0.13
Nodes (15): V2FallbackReason, ExitPortalReached, Faulted, FrameUnavailable, HorizonNonConforming, NoCommand, None, NonFiniteAuthority (+7 more)

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (12): Dictionary, List, Vector3, DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor, JunctionMovement (+4 more)

### Community 111 - "RoadRage.Features.Vehicles.Traffic"
Cohesion: 0.20
Nodes (7): RoadLineSegment, IReadOnlyList, List, Vector2, Vector3, StopLineProjection, RoadRage.Features.Vehicles.Traffic

### Community 112 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 113 - "NetworkedPlayerLifecycleIntent"
Cohesion: 0.32
Nodes (3): Rpc, RpcParams, NetworkedPlayerLifecycleIntent

### Community 114 - "RoadModelVersion"
Cohesion: 0.25
Nodes (5): RoadModelVersion, High, IsEmpty, Low, SchemaVersion

### Community 115 - "CampaignTraceability"
Cohesion: 0.20
Nodes (6): Dictionary, HashSet, IReadOnlyDictionary, RoadId, CampaignTraceability, Elements

### Community 116 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 117 - "LongitudinalArbitration.cs"
Cohesion: 0.20
Nodes (13): IReadOnlyList, RoadId, JunctionEntryInput, Active, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader (+5 more)

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "TrafficJunctionOutcome"
Cohesion: 0.22
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 120 - "DriverProfile"
Cohesion: 0.11
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - ".InterStepBound"
Cohesion: 0.29
Nodes (7): Quaternion, Vector3, BodyState, GaugeBox, Rho, NominalPose, TrackingMeasurement

### Community 123 - "AIVehicleBehaviorDebugView"
Cohesion: 0.16
Nodes (5): Camera, TMP_Text, Vector3, AIVehicleBehaviorDebugView, TextMeshPro

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.03
Nodes (81): AgentObservation, Blocker, Dictionary, DriverProfileDef, IReadOnlyList, LongitudinalDecision, Portal, RoutePlan (+73 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.18
Nodes (6): IList, List, MenuItem, Vector2, PairReviewWindow, StatusFilter

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.10
Nodes (20): CharacterController, Collider, GameObject, IEnumerator, IReadOnlyList, List, NetworkObject, Vector3 (+12 more)

### Community 128 - "SafetyResult"
Cohesion: 0.13
Nodes (14): SafetyResult, Command, HazardId, NearFieldStep, PhysicsStep, Reason, Refusal, SourceFrameId (+6 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.06
Nodes (26): GameObject, NetworkManager, NetworkPrefabsList, RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster (+18 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.07
Nodes (31): CompiledRoadModel, Dictionary, HashSet, IEnumerable, IReadOnlyList, JunctionSnapshot, List, ProfilerMarker (+23 more)

### Community 131 - "DriverProfileDef"
Cohesion: 0.18
Nodes (9): DriverProfile, DriverProfileDef, CollisionReaction, Id, Policy, Profile, RawId, CollisionReactionWeights (+1 more)

### Community 132 - "RunEscapeMenuFlowController"
Cohesion: 0.11
Nodes (8): RunEscapeMenuFlowController, IsOpen, Button, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked, CursorLockMode

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.04
Nodes (69): CompiledConflictZone, IList, Vector3, ConflictKind, Crossing, Merge, ConflictZone, DrivabilityProfile (+61 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.07
Nodes (35): CompiledRoadModel, DriverProfile, IReadOnlyList, List, RoadElementKind, RoadId, DeferredLimit, DeferredLimitKind (+27 more)

### Community 135 - "RoadRage.Shared.Networking"
Cohesion: 0.11
Nodes (13): NetworkVariable, NetworkedBossState, NetworkVariable, NetworkedCrewEconomyState, VehicleDamageType, Brake, Engine, Wheel (+5 more)

### Community 136 - "LobbyJoinService"
Cohesion: 0.13
Nodes (14): Task, LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode (+6 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (5): Vector3, VehicleProfileDef, Id, Profile, RawId

### Community 139 - "CollisionReactionWeights"
Cohesion: 0.16
Nodes (11): CollisionReaction, Brake, Evade, MisReact, CollisionReactionWeights, Brake, Default, Evade (+3 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (35): Func, AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto (+27 more)

### Community 142 - "JunctionActorReport"
Cohesion: 0.07
Nodes (25): JunctionPriority, IReadOnlyList, RoadId, Vector3, JunctionActorReport, Approaches, Corners, ElementId (+17 more)

### Community 143 - "LaneNode"
Cohesion: 0.13
Nodes (14): IReadOnlyList, LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors (+6 more)

### Community 144 - "RuleExceptionReason"
Cohesion: 0.14
Nodes (14): RuleExceptionReason, Accepted, AlreadyActive, Expired, FrameUnavailable, Held, Malformed, RequesterAbsent (+6 more)

### Community 145 - "RoadModelCanonicalWriter"
Cohesion: 0.26
Nodes (5): BinaryWriter, Comparison, IReadOnlyList, Vector3, RoadModelCanonicalWriter

### Community 146 - "DefinitionId"
Cohesion: 0.13
Nodes (11): Color, GameObject, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 147 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 148 - "ImportContext"
Cohesion: 0.17
Nodes (8): DrivabilityProfile, RoadBoundsBox, RoadCurvePoint, Vector3, CircleFit, ImportContext, V1RoadModelImporter, CircleFit

### Community 149 - "SafetyReason"
Cohesion: 0.22
Nodes (9): SafetyReason, ImminentUnintendedCollision, InvalidActorState, LocalPlanInvalidated, None, NonFiniteOutput, PhysicallyInvalidIntent, PhysicallyInvalidPath (+1 more)

### Community 150 - ".Step"
Cohesion: 0.05
Nodes (43): JunctionConflictIndex, TrafficFrame, RecoveryCommandInput, RecoveryMotion, TacticalDecision, AcceptedAtFrame, Active, AwaitingRecovery (+35 more)

### Community 151 - "NetworkedVehicleSeatIntent"
Cohesion: 0.25
Nodes (5): Key, Rpc, RpcParams, NetworkedVehicleSeatIntent, Keyboard

### Community 152 - "VehicleCoverage"
Cohesion: 0.25
Nodes (8): VehicleCoverage, Covered, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA, NotEstablished, PoseModelMismatch, TrackingToleranceUndeclared

### Community 153 - "PathIssue"
Cohesion: 0.29
Nodes (7): PathIssue, CurvatureSlope, MissingElement, None, SeamCurvature, SeamGap, SeamTangent

### Community 154 - ".Configure"
Cohesion: 0.25
Nodes (6): CinemachineCamera, CinemachineOrbitalFollow, Transform, ThirdPersonCameraConfiguration, CinemachineDeoccluder, CinemachineRotationComposer

### Community 155 - ".Read"
Cohesion: 0.25
Nodes (6): Collider, List, MonoBehaviour, Scene, Transform, SidewalkDeclarations

### Community 156 - ".Bind"
Cohesion: 0.40
Nodes (4): CompiledRoadModel, GateAEvidenceBinding, Signoff, Signoff

### Community 157 - "TrackPiece"
Cohesion: 0.18
Nodes (9): RoadCurve, RoadElementKind, TrackPiece, Curve, ElementStartSMeters, EndDistanceMeters, Id, Kind (+1 more)

### Community 158 - "TrafficV2Settings"
Cohesion: 0.15
Nodes (12): GridlockEscalationTier, IReadOnlyList, TrackingTolerance, TrafficRule, VehicleCoverage, TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits (+4 more)

### Community 159 - "NetworkedPlayerLifecycleService"
Cohesion: 0.15
Nodes (8): IEnumerable, NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 161 - "ReferenceTrack"
Cohesion: 0.21
Nodes (6): RoadKinematicAnchor, ReferenceTrack, HasKinematicPose, LengthMeters, Pieces, ReferenceAheadRearAxleMeters

### Community 162 - ".Build"
Cohesion: 0.14
Nodes (12): JunctionKinematics, Known, JunctionMovementPosition, ConditionalWeakTable, DriverProfile, IReadOnlyList, List, RoadId (+4 more)

### Community 163 - "RoutePath"
Cohesion: 0.26
Nodes (7): IReadOnlyList, RoadCurve, RoadCurvePoint, Vector3, RoutePath, LengthMeters, Spans

### Community 164 - "DrivingPolicyProfile"
Cohesion: 0.21
Nodes (9): DrivingPolicyProfile, AcceptedGapSeconds, AcceptedRisk, AllowedSurfaces, Default, RoutePreferenceWeight, ManeuverPreference, Cost (+1 more)

### Community 165 - "AuthoringDecisions"
Cohesion: 0.05
Nodes (53): AppliedWidth, ConflictKind, Dictionary, FileLayout, Func, IEnumerable, IList, JunctionControlKind (+45 more)

### Community 166 - "NetworkedPassengerActionIntent"
Cohesion: 0.10
Nodes (16): Func, Vector3, NetworkedPassengerActionIntent, NetworkVariable, NetworkedPassengerActionIncidentState, IsActive, List, PassengerActionCatalog (+8 more)

### Community 167 - "ElementTrace"
Cohesion: 0.12
Nodes (16): ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key, Kind, MaxInterStepBoundMeters (+8 more)

### Community 168 - "VehicleDriveIntent"
Cohesion: 0.20
Nodes (6): VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 169 - "VehicleDriveIntentComposer"
Cohesion: 0.16
Nodes (13): VehicleDriveIntent, VehicleProfile, ComposedDrive, V2FallbackTerminal, Held, None, StopOverrun, VehicleDriveIntentComposer (+5 more)

### Community 170 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 171 - "RuleExceptionRequest"
Cohesion: 0.18
Nodes (8): RuleExceptionRequest, Requester, Rule, Scope, SourceFrame, StartReason, Target, Termination

### Community 172 - "DrivingPolicy.cs"
Cohesion: 0.20
Nodes (9): ManeuverKind, AdjacentCorridor, AuthorizedSurface, CorridorOffset, OpposingCorridor, RuleExceptionTermination, RuleExceptionTerminationKind, Expiry (+1 more)

### Community 173 - "TrafficRule"
Cohesion: 0.20
Nodes (10): TrafficRule, FollowingGap, JunctionControl, KeepClear, LaneChange, None, OpposingCorridor, Sidewalk (+2 more)

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 175 - "TrackingMeasurement.cs"
Cohesion: 0.25
Nodes (8): InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces, PositionResidualMeters, RotationResidualDegrees, PieceBound

### Community 176 - "LongitudinalMemory"
Cohesion: 0.22
Nodes (8): LongitudinalMemory, None, StopHoldRelease, GapOpened, GrantEffective, None, SourceDeparted, SourceGone

### Community 177 - "LongitudinalCandidateKind"
Cohesion: 0.22
Nodes (9): LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable, Profile, SteeringCeilingUnreachable (+1 more)

### Community 178 - "JunctionSnapshot"
Cohesion: 0.06
Nodes (32): JunctionControlKind, GridlockEscalationTier, None, PrecedenceRelaxation, GridlockOutcome, Escalated, Exhausted, Progressed (+24 more)

### Community 179 - "JunctionExitBound"
Cohesion: 0.25
Nodes (7): JunctionExitBound, ExitPortal, ExitSearchBound, None, Occupant, Reservations, RouteEnd

### Community 180 - "V2StageTimings"
Cohesion: 0.25
Nodes (3): V2StageTimings, TotalMillisecondsPerStep, JunctionBlockerCause

### Community 181 - ".Register"
Cohesion: 0.36
Nodes (6): Dictionary, IReadOnlyList, KeyValuePair, RoadRecordKind, LineageKeyRegistry, Keys

### Community 182 - "V2ComposerDiagnostic"
Cohesion: 0.29
Nodes (7): V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None, ProfileScalarNonFinite, RollingBackward

### Community 183 - "LaneGraphRouting"
Cohesion: 0.27
Nodes (3): IReadOnlyList, Vector3, LaneGraphRouting

### Community 184 - ".RenderSignoff"
Cohesion: 0.29
Nodes (3): IList, SignoffLayout, DateTime

### Community 185 - "MatchSettings"
Cohesion: 0.40
Nodes (5): Difficulty, MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount

### Community 186 - ".FromRoute"
Cohesion: 0.50
Nodes (3): CompiledRoadModel, IReadOnlyList, List

### Community 187 - "PerceptionUnavailableReason"
Cohesion: 0.40
Nodes (5): PerceptionUnavailableReason, ChannelSaturated, ChannelUnavailable, HazardCollectorSaturated, None

### Community 189 - "NetworkedVehicleRecoveryIntent"
Cohesion: 0.30
Nodes (4): Key, Rpc, RpcParams, NetworkedVehicleRecoveryIntent

### Community 190 - "StopHoldPhase"
Cohesion: 0.40
Nodes (5): StopHoldPhase, Entered, Holding, None, Released

### Community 191 - "CollisionGoalPhase"
Cohesion: 0.40
Nodes (5): CollisionGoalPhase, AwaitingRecovery, Braking, None, Reacting

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.08
Nodes (19): DevVehicleSpawner, SessionTrafficValue, RoadRage.App.Services, RoadRage.Features.Players, RoadRage.App, RoadRage.Shared.Domain, RoadRage.Features.UI, RoadRage.Features.Run (+11 more)

### Community 196 - "LobbyCodeClipboard"
Cohesion: 0.15
Nodes (11): Color, PointerEventData, TMP_Text, LobbyCodeClipboard, LobbyRosterEntry, DisplayName, PortraitTint, Ready (+3 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.12
Nodes (11): NetworkedLocalPlayerPoseReporter, Collider, FixedString32Bytes, GameObject, Rpc, RpcParams, Vector3, NetworkedPlayerPresentation (+3 more)

### Community 206 - "PairReviewModel"
Cohesion: 0.23
Nodes (10): CompiledRoadModel, List, RoadId, StringBuilder, PairReviewModel, PairReviewStatus, Modified, New (+2 more)

### Community 216 - "LaneGraph"
Cohesion: 0.11
Nodes (12): Color, HashSet, IReadOnlyList, List, Quaternion, Vector3, LaneGraph, EntryPortals (+4 more)

### Community 217 - "TrafficPerception"
Cohesion: 0.10
Nodes (28): LaneSide, RoadId, Vector3, AdjacentOccupantFact, ExitOccupancyFact, IntentOverlapKind, ConflictZone, SameElement (+20 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.11
Nodes (12): BoxCollider, CharacterController, Collider, IReadOnlyList, List, NetworkTransform, Quaternion, RaycastHit (+4 more)

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1373 isolated node(s):** `Id`, `RawId`, `Profile`, `CollisionReaction`, `Policy` (+1368 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1932 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **33 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `TrafficV2StepRunner`, `RoadRage.Features.Vehicles.Traffic.Planning`, `TrafficV2Insertion`, `.Monitor`, `GateAEvidenceParameters`, `TrafficDecisionProjection`, `NetworkedPlayerState`, `TrafficV2Admission`, `JunctionActorReport`, `EffectivePolicy`, `V2StageTimings`, `.PrepareStep`, `.Step`, `TrafficDriveOutcome`, `TrafficV2HazardCollector`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.174) - this node is a cross-community bridge._
- **Why does `PortalTrafficSpawner` connect `PortalTrafficSpawner` to `RoadRage.Shared.Domain`, `TrafficV2StepRunner`, `MonoBehaviour`, `RageRoadEventFlowController`, `TrafficV2Admission`, `TrafficV2Composition.cs`, `TrafficV2Code`, `TrafficSettingsDef`, `LaneGraph`, `TrafficV2VehicleDriver`?**
  _High betweenness centrality (0.157) - this node is a cross-community bridge._
- **Why does `RoadId` connect `RoadId` to `TrafficFrame`, `RoadModelRecords.cs`, `SpeedPlan`, `RoutePlan`, `.Localize`, `RoadModelDocument`, `RoadGeometryValidator`, `RoadModelCanonicalWriter`, `Blocker`, `RoadModelSource`, `.Regenerate`, `RoadModelValidationIssue`, `RoadLineage`, `TrafficV2HazardCollector`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.127) - this node is a cross-community bridge._
- **What connects `Id`, `RawId`, `Profile` to the rest of the system?**
  _1373 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MotionPlan` be split into smaller, more focused modules?**
  _Cohesion score 0.09247311827956989 - nodes in this community are weakly interconnected._
- **Should `.Refine` be split into smaller, more focused modules?**
  _Cohesion score 0.1111111111111111 - nodes in this community are weakly interconnected._
- **Should `RoadRage.Features.Vehicles.Traffic.Planning` be split into smaller, more focused modules?**
  _Cohesion score 0.08383838383838384 - nodes in this community are weakly interconnected._