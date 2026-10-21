# Graph Report - RRS  (2026-10-21)

## Corpus Check
- 191 files · ~273,148 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 435 file(s) not represented in the graph (top: .meta 347, .asset 36, .asmdef 14)

## Summary
- 4617 nodes · 12589 edges · 186 communities (153 shown, 33 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 768 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `37a3a46c`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- NetworkedPassengerActionIntent
- PairRefinement
- system
- GreyboxAssetSeedMetadata
- SweepPose
- ManeuverPath
- GateAEvidenceParameters
- .Localize
- RecoveryRequest
- .FixedUpdate
- MonoBehaviour
- RunFlowController
- RunCheckpointHudScreen
- OnlineServicesBootstrapService
- NetworkedVehicleDamageVfxController
- LobbyRosterService
- LocalOnFootController
- Blocker
- RageTuningDef
- LobbyFlowController.cs
- MigrationReport
- VehiclePhysicsBody
- LobbyFlowController
- EmotionGains
- .Core
- LobbyRoomService
- AutomatedPairDecisionPolicy
- EffectiveRuleException
- V1ImportResult
- CharacterCatalog
- RoadId
- CollisionFacts
- TrafficFrame
- MotionPlan
- NetworkedVehicleDriverController
- ConflictSweep
- PlayerMode
- NetworkedVehicleState
- VehicleProfile
- RoundaboutClearance
- RecoverySupervisor
- TacticalReason
- NpcReactionEffect
- RoadCurve
- .Measure
- ManeuverEvaluation
- TrafficV2Scenario
- EffectivePolicy
- PassengerActionVerdictCode
- NetworkPlayerRegistry
- AuthoredRoadModel
- LobbyJoinOutcome
- .Regenerate
- NetworkedRunSessionMonitor
- RunEscapeMenuFlowController
- DefinitionId
- .Meters
- TrafficDriveOutcome
- LongitudinalDecision
- RoadRage.Features.UI
- .Monitor
- TrafficV2HazardCollector
- CollisionResponseRequest
- RoadRecordKind
- .Decide
- Difficulty
- RoutePlan
- TrafficV2Insertion
- ManeuverCandidate
- PassengerActionDef
- .Compute
- TrafficDecisionProjection
- StabilityBlocker
- DispositionKind
- MeasurementRun
- DrivabilityProfile
- UserNotice
- TrafficLongitudinalOutcome
- NetworkedPlayerState
- .EvaluateManeuver
- ModelDto
- JunctionSnapshot
- KinematicOffsetBounds
- PlayerProfileFileStore
- Dictionary
- TrafficJunctionOutcome
- .Entry
- TrafficV2Admission
- LobbyRosterScreen
- RoadModelValidationIssue
- MainMenuScreen
- PathHorizon
- RoadLineage
- .Bind
- V1SourceSet
- JunctionConflictIndex
- RightOfWayRelation
- VehicleWheel
- GateAReviewWindow
- LocalVehicleCameraRig
- Vector3
- NetworkedAIVehicleState
- RageTuningCatalog
- NetworkedPlayerReviveIntent
- ManeuverGeometryCause
- .UpdateSteeringState
- RageRoadEventFlowController
- JunctionDistances
- PairSweep
- RoadGeometryValidator
- MovementRole
- VehicleClassMask
- NetworkedPlayerLifecycleIntent
- LongitudinalArbitration.cs
- RouteReason
- SpeedConstraint
- LongitudinalArbitration.cs
- TrafficSettingsDef
- ConflictSweepRefinement.cs
- DriverProfile
- StatusFilter
- ReferenceTrack
- RageDisposition
- PairRelation
- TrafficV2VehicleDriver
- PairReviewWindow
- PortalTrafficSpawner
- SafetyResult
- RoadRageBootstrap
- TrafficV2StepRunner
- SpeedProfileIssue
- TelemetrySample
- RoadModelRecords.cs
- SpeedPlan
- JunctionFeature
- LobbyJoinService
- RoadModelValidationCode
- VehicleProfileDef
- CollisionReactionWeights
- LeafState
- RoadModelDocument
- RoadClass
- LaneNode
- RuleExceptionReason
- RoadModelCanonicalWriter
- HostOwnedNetworkStateBehaviour
- RuleExceptionStatus
- ImportContext
- Collision
- TacticalDecision
- NetworkedVehicleSeatIntent
- CandidateDiffState
- CollisionGoalPhase
- TacticalGoalKind
- WheelState
- GridlockOutcome
- PlanningDecision
- NetworkedPlayerState
- unityengine
- ImportContext
- DrivingPolicyProfile
- AuthoringDecisions
- ElementTrace
- V2FallbackReason
- VehicleCoverage
- TrackingTolerance
- TrafficV2Code
- VehicleDamageType
- JunctionRequestRejection
- LaneGraphRouting
- ManeuverPhase
- TireSample
- PlayerNameValidator
- NetworkedVehicleRecoveryIntent
- LobbyPlayerSlotView
- Vector2
- RoadRage.Shared.Domain
- VehicleArcadeAssist
- AIVehicleBehaviorDebugView
- TrafficV2WorkCounters.cs
- NetworkedPlayerPresentation
- LaneGraph
- TrafficPerception
- NetworkedAIVehicleDriverController
- Lock-Rage Camera Fix Query

## God Nodes (most connected - your core abstractions)
1. `RoadId` - 372 edges
2. `TrafficV2VehicleDriver` - 151 edges
3. `CompiledRoadModel` - 133 edges
4. `RunFlowController` - 99 edges
5. `TrafficFrame` - 75 edges
6. `ConflictSweep` - 72 edges
7. `RoadCurve` - 69 edges
8. `NetworkedVehicleState` - 67 edges
9. `ImportContext` - 67 edges
10. `AuthoredRoadModel` - 66 edges

## Surprising Connections (you probably didn't know these)
- `CompiledRoadModel` --calls--> `RoadCurve`  [INFERRED]
  Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadCurve.cs
- `MovementRecord` --references--> `RoadCurveSample`  [EXTRACTED]
  Assets/RoadRage/Features/Vehicles/Traffic/Migration/PairReview.cs → Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs
- `RoadRageBootstrap` --references--> `ISteamIdentitySource`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/ISteamIdentitySource.cs
- `RoadRageBootstrap` --references--> `LobbyJoinService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyJoinService.cs
- `RoadRageBootstrap` --references--> `LobbyRoomService`  [EXTRACTED]
  Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs → Assets/RoadRage/Features/Online/LobbyRoomService.cs

## Import Cycles
- None detected.

## Communities (186 total, 33 thin omitted)

### Community 0 - "NetworkedPassengerActionIntent"
Cohesion: 0.11
Nodes (9): NetworkedPassengerActionIntent, NetworkedPassengerActionIncidentState, IsActive, PassengerActionCatalog, Count, Version, PassengerActionDebugView, IncidentText (+1 more)

### Community 1 - "PairRefinement"
Cohesion: 0.27
Nodes (3): PairRefinement, ZoneTyping, Conservative

### Community 2 - "system"
Cohesion: 0.09
Nodes (17): PlanningTolerances, RoadRage.Features.Vehicles.Traffic.Frame, RoadRage.Features.Vehicles.Traffic.Coordination, RoadRage.Features.Vehicles.Traffic.Migration, RoadRage.Features.Vehicles.Traffic.Diagnostics, RoadRage.Features.Vehicles.Traffic.Policy, RoadRage.Features.Vehicles.Traffic.Planning, RoadRage.Features.Vehicles.Traffic.Routing (+9 more)

### Community 3 - "GreyboxAssetSeedMetadata"
Cohesion: 0.22
Nodes (8): GreyboxAssetSeedMetadata, ColliderPlan, ExportAssetPath, ReplacementPolicy, ScaleCheck, SourceAssetPath, StableId, VisualReadability

### Community 4 - "SweepPose"
Cohesion: 0.18
Nodes (10): RefinementInputs, ShortElement, SweepElement, EndS, Length, StartS, SweepGraph, SweepPose (+2 more)

### Community 5 - "ManeuverPath"
Cohesion: 0.09
Nodes (18): ManeuverPath, CorridorId, DepartEndSMeters, LengthMeters, MaximumAbsoluteCurvaturePerMeter, Piece, Reference, ReturnEndSMeters (+10 more)

### Community 6 - "GateAEvidenceParameters"
Cohesion: 0.11
Nodes (17): GateAEvidenceParameters, CanonicalText, ClosureIterationBudget, Feasibility, IsLegacy, Kinematic, OffsetGridStepRadians, OffsetToleranceRadians (+9 more)

### Community 7 - ".Localize"
Cohesion: 0.09
Nodes (15): TrafficActorInput, TrafficV2Lifecycle, LocalizationFixture, ElementIndex, Query, RoadElementKind, JunctionMovement, LaneCorridor (+7 more)

### Community 8 - "RecoveryRequest"
Cohesion: 0.09
Nodes (21): RecoveryAttempt, Outcome, OutcomeFrame, Request, Response, ResponseFrame, RecoveryCause, Displaced (+13 more)

### Community 10 - "MonoBehaviour"
Cohesion: 0.11
Nodes (5): NetworkedPlayerSpawnService, RunCompositionRoot, RuntimeRoot, SpawnRoot, MenuCharacterPreview

### Community 13 - "OnlineServicesBootstrapService"
Cohesion: 0.08
Nodes (15): FacepunchSteamPlatform, IsLoggedOn, IsValid, ISteamIdentitySource, ISteamPlatform, IsLoggedOn, IsValid, OnlineServicesBootstrapService (+7 more)

### Community 15 - "LobbyRosterService"
Cohesion: 0.08
Nodes (18): LobbyMemberSnapshot, CharacterId, DisplayName, Ready, SteamId, LobbyRosterSnapshot, AiVehicleTargetCount, Difficulty (+10 more)

### Community 16 - "LocalOnFootController"
Cohesion: 0.07
Nodes (15): LocalVoidRespawnController, CheckpointHud, IsDead, LocalOnFootController, IsDowned, MovementEnabled, PlayerCamera, SprintSpeed (+7 more)

### Community 17 - "Blocker"
Cohesion: 0.13
Nodes (16): Blocker, BlockerKind, BlockedExit, JunctionGrant, Leader, Obstacle, PolicyImmobilization, BlockerRules (+8 more)

### Community 18 - "RageTuningDef"
Cohesion: 0.09
Nodes (22): EmotionMeters, EmotionMeterState, NetworkedRageState, CurrentDisposition, CurrentEmotion, RageTuningDef, EscapeEnterPercent, EscapeExitPercent (+14 more)

### Community 19 - "LobbyFlowController.cs"
Cohesion: 0.11
Nodes (6): AppPlayModeEntry, RoadRage.App.Services, RoadRage.App, RoadRage.App.Lobby, RoadRage.Features.Online, RoadRage.Features.Lobby

### Community 20 - "MigrationReport"
Cohesion: 0.11
Nodes (14): MeasureClass, DeviationToCorrect, JustifiedException, PublishedDisplacement, Within, MeasureValue, Metric, MigrationBinding (+6 more)

### Community 21 - "VehiclePhysicsBody"
Cohesion: 0.12
Nodes (8): VehiclePhysicsBody, CurrentSteerAngleDegrees, GroundedAuthorityFactor, GroundedWheelCount, HasProfile, Profile, WheelCount, WheelPair

### Community 23 - "EmotionGains"
Cohesion: 0.08
Nodes (24): EmotionGains, AcceptedGap, AcceptedRisk, ComfortableDeceleration, DesiredSpeed, ManeuverCost, MaxAcceleration, MinimumGap (+16 more)

### Community 24 - ".Core"
Cohesion: 0.27
Nodes (4): Edge, Node, ObjectivePhase, RoutePlanner

### Community 25 - "LobbyRoomService"
Cohesion: 0.17
Nodes (10): LobbyRoomService, DisplayJoinCode, JoinCode, Status, LobbyRoomStatus, Closed, Creating, CreationFailed (+2 more)

### Community 26 - "AutomatedPairDecisionPolicy"
Cohesion: 0.10
Nodes (6): AutomatedPairDecisionManifest, AutomatedPairDecisionPlan, AutomatedPairDecisionPolicy, AutomatedPairDecisionRecord, ClassificationSummary, RefinedPair

### Community 27 - "EffectiveRuleException"
Cohesion: 0.09
Nodes (19): Active, EffectiveRuleException, AcceptedFrame, ExpiryFrame, Request, Requester, Rule, RuleExceptionRecord (+11 more)

### Community 28 - "V1ImportResult"
Cohesion: 0.12
Nodes (18): AuthoringTask, DisplacementKind, PortalBoundaryTrim, RingAnchorShift, ImportedConnection, ImportedPortal, ImportedSection, PublishedDisplacement (+10 more)

### Community 29 - "CharacterCatalog"
Cohesion: 0.16
Nodes (11): MainMenuProfileFlowController, CurrentIndex, PlayerProfile, CharacterId, DisplayName, PlayerProfileBootstrapService, PlayerProfileResolution, Error (+3 more)

### Community 30 - "RoadId"
Cohesion: 0.06
Nodes (27): CompiledConflictZone, CompiledJunctionControl, CompiledJunctionMovement, CompiledRoadModel, Adjacencies, ConflictZones, Connections, Controls (+19 more)

### Community 31 - "CollisionFacts"
Cohesion: 0.25
Nodes (5): CollisionFacts, DeltaVMetersPerSecond, IsFinite, CollisionPredicates, RecoveryMotion

### Community 32 - "TrafficFrame"
Cohesion: 0.04
Nodes (34): RoadModelVersion, High, IsEmpty, Low, SchemaVersion, ProjectionKey, TrafficActor, TrafficFrame (+26 more)

### Community 33 - "MotionPlan"
Cohesion: 0.07
Nodes (32): LongitudinalBounds, Valid, MotionDiagnostic, HorizonTruncated, None, SteeringInactiveSpan, MotionIssue, GateAEvidenceMissing (+24 more)

### Community 35 - "ConflictSweep"
Cohesion: 0.12
Nodes (5): ConflictSweep, GridPath, PoseFrame, RefineNode, RefineSegment

### Community 36 - "PlayerMode"
Cohesion: 0.14
Nodes (7): PlayerMode, Driver, OnFoot, OnFootRageRoad, OnFootStop, Passenger, Spectating

### Community 37 - "NetworkedVehicleState"
Cohesion: 0.08
Nodes (8): NetworkedVehicleSeatService, Instance, NetworkedVehicleState, CurrentDamageThresholdsCrossed, CurrentHp, IsBrakeDamaged, IsEngineDamaged, IsWheelDamaged

### Community 38 - "VehicleProfile"
Cohesion: 0.05
Nodes (36): VehicleProfile, AntiRollRate, AttitudeDamping, AttitudeLevellingRate, BrakeTorque, CenterOfMass, CoastTorque, Damper (+28 more)

### Community 40 - "RecoverySupervisor"
Cohesion: 0.14
Nodes (12): ProgressLedger, ActualMeters, ExpectedMeters, RecoverySupervisor, ActualMeters, Attempts, Cause, EpisodeAttempts (+4 more)

### Community 41 - "TacticalReason"
Cohesion: 0.11
Nodes (18): TacticalReason, Accepted, CollisionPreempted, ExitPortalReached, FallbackLatched, GoalAlreadyActive, InvalidRequest, ManeuverAborted (+10 more)

### Community 42 - "NpcReactionEffect"
Cohesion: 0.13
Nodes (11): RageStateDebugView, NpcReactionEffect, AffectsFear, AffectsRage, Channel, Magnitude, ReactionChannel, Both (+3 more)

### Community 43 - "RoadCurve"
Cohesion: 0.12
Nodes (8): RoadCurve, FullBounds, Length, MaximumAbsoluteCurvaturePerMeter, MaximumChordTangentAngleRadians, SampleCount, StartS, RoadProjection

### Community 44 - ".Measure"
Cohesion: 0.26
Nodes (4): Result, Passed, Row, StopLineSeparation

### Community 45 - "ManeuverEvaluation"
Cohesion: 0.14
Nodes (11): ManeuverTiming, ManeuverKind, AdjacentCorridor, AuthorizedSurface, CorridorOffset, OpposingCorridor, ManeuverEvaluation, ManeuverSupervision (+3 more)

### Community 46 - "TrafficV2Scenario"
Cohesion: 0.16
Nodes (12): ScenarioInsertion, TrafficComposition, V1, V2Slice, TrafficV2Scenario, Insertions, Label, MaxPopulation (+4 more)

### Community 47 - "EffectivePolicy"
Cohesion: 0.08
Nodes (28): DrivingPolicy, DrivingSurface, Carriageway, None, OpposingCorridor, Sidewalk, EffectivePolicy, AcceptedGapSeconds (+20 more)

### Community 48 - "PassengerActionVerdictCode"
Cohesion: 0.11
Nodes (19): PassengerActionValidation, PassengerActionValidationContext, PassengerActionVerdict, Accepted, PassengerActionVerdictCode, Accepted, ActorMismatch, ActorNotAlive (+11 more)

### Community 49 - "NetworkPlayerRegistry"
Cohesion: 0.15
Nodes (5): NetworkPlayerConnectionPayload, NetworkPlayerProfile, CharacterId, DisplayName, NetworkPlayerRegistry

### Community 50 - "AuthoredRoadModel"
Cohesion: 0.09
Nodes (6): AuthoredRoadModel, AuthoredRun, Import, Succeeded, GateABinding, SignoffLayout

### Community 51 - "LobbyJoinOutcome"
Cohesion: 0.08
Nodes (14): FacepunchSteamLobbyPlatform, ISteamLobbyPlatform, LobbyCreateOutcome, LobbyId, Success, LobbyJoinFailureReason, Expired, Failed (+6 more)

### Community 52 - ".Regenerate"
Cohesion: 0.22
Nodes (9): SidewalkDeclarations, CandidateDiffEntry, GateAEvidenceRegeneration, GateAEvidenceRegenerationResult, Complete, Covered, JunctionClearanceRelief, JunctionClearanceResult (+1 more)

### Community 54 - "RunEscapeMenuFlowController"
Cohesion: 0.13
Nodes (6): RunEscapeMenuFlowController, IsOpen, RunEscapeMenuScreen, IsOpen, LocalInputGate, IsBlocked

### Community 55 - "DefinitionId"
Cohesion: 0.13
Nodes (11): CharacterCatalog, Count, CharacterDef, DisplayName, Id, PreviewPrefab, PreviewTint, RawId (+3 more)

### Community 57 - "TrafficDriveOutcome"
Cohesion: 0.10
Nodes (18): TrafficDriveOutcome, AppliedConstraints, Binding, BrakeReverse, DecisionEpoch, DeferredConstraints, Fallback, FallbackReason (+10 more)

### Community 58 - "LongitudinalDecision"
Cohesion: 0.15
Nodes (13): LongitudinalDecision, AppliedAccelerationMetersPerSecondSquared, Binding, Candidates, FreeRoadAccelerationMetersPerSecondSquared, Hold, HoldCauses, Memory (+5 more)

### Community 60 - ".Monitor"
Cohesion: 0.17
Nodes (5): RoadLocationFlags, Ambiguous, None, OutsideEnvelope, WrongWay

### Community 61 - "TrafficV2HazardCollector"
Cohesion: 0.08
Nodes (25): TrafficHazardCollectorCounters, TrafficHazardKind, Obstacle, Pedestrian, Vehicle, WalkingPlayer, HazardQuery, HazardQueryReport (+17 more)

### Community 62 - "CollisionResponseRequest"
Cohesion: 0.14
Nodes (13): CollisionAnalysis, CollisionResponseRequest, Facts, Significance, SourceFrameId, TrafficId, Version, CollisionSignificance (+5 more)

### Community 63 - "RoadRecordKind"
Cohesion: 0.14
Nodes (14): LineageKeyRegistry, Keys, RoadRecordKind, Adjacency, ConflictZone, Connection, Control, Corridor (+6 more)

### Community 64 - ".Decide"
Cohesion: 0.14
Nodes (14): LongitudinalArbitration, LongitudinalCandidate, LongitudinalCandidateKind, DesiredSpeed, JunctionEntry, LeaderFollowing, Obstacle, PerceptionUnavailable (+6 more)

### Community 65 - "Difficulty"
Cohesion: 0.19
Nodes (8): MatchSettings, AiVehicleTargetCount, Difficulty, LitterThrowerCount, Difficulty, Easy, Hard, Normal

### Community 66 - "RoutePlan"
Cohesion: 0.09
Nodes (27): DecisionCounter, RouteDiagnostic, None, ZeroWeightFallback, RouteOutcome, InvalidInput, NoRoute, Planned (+19 more)

### Community 67 - "TrafficV2Insertion"
Cohesion: 0.18
Nodes (11): TrafficV2Insertion, Code, EntryPortal, ExitPortalId, FirstSpeedPlan, Position, Rotation, Route (+3 more)

### Community 68 - "ManeuverCandidate"
Cohesion: 0.05
Nodes (38): ManeuverCandidate, Cost, DurationSeconds, ExceptionExpiryFrame, GeometryCause, Kind, OtherCorridorId, Path (+30 more)

### Community 69 - "PassengerActionDef"
Cohesion: 0.17
Nodes (10): PassengerActionDef, CooldownSeconds, DisplayName, Id, MaxRange, RawId, Slot, Version (+2 more)

### Community 71 - "TrafficDecisionProjection"
Cohesion: 0.06
Nodes (29): TrafficDecisionProjection, Code, Drive, ElementId, ElementKind, EvidenceStatus, ExitPortalId, FrameId (+21 more)

### Community 72 - "StabilityBlocker"
Cohesion: 0.20
Nodes (9): StabilityBlocker, AngularSpeed, None, NonFiniteFacts, SignificantContact, WheelsAirborne, YawRate, TacticalResponse (+1 more)

### Community 73 - "DispositionKind"
Cohesion: 0.18
Nodes (11): DispositionKind, Connection, ControlRouteSeed, CorridorInterior, CorridorVertex, MergedIntoCorridorEndpoint, Movement, MovementApproachPath (+3 more)

### Community 74 - "MeasurementRun"
Cohesion: 0.22
Nodes (9): CampaignTriplet, MeasurementKind, Acceptance, Exploratory, MeasurementRun, Kind, Label, MaxPopulation (+1 more)

### Community 76 - "UserNotice"
Cohesion: 0.15
Nodes (9): UserNotice, Message, Severity, UserNoticeSeverity, Error, Info, Warning, UserNoticeChannel (+1 more)

### Community 77 - "TrafficLongitudinalOutcome"
Cohesion: 0.17
Nodes (11): TrafficLongitudinalOutcome, Blockers, Collector, Decision, Dominant, FrameId, HasDominant, HazardQueryHits (+3 more)

### Community 79 - ".EvaluateManeuver"
Cohesion: 0.24
Nodes (5): ManeuverTrigger, CauseId, CauseKind, CauseSpeedMetersPerSecond, ObstacleKind

### Community 81 - "JunctionSnapshot"
Cohesion: 0.07
Nodes (22): GridlockEscalationTier, None, PrecedenceRelaxation, GridlockResolution, JunctionBatchCounters, JunctionGrantStatus, Denied, Granted (+14 more)

### Community 82 - "KinematicOffsetBounds"
Cohesion: 0.12
Nodes (9): ElementOffsets, Reachable, KinematicOffsetBounds, Closed, KinematicPoseSet, OffsetTrace, EndS, StartS (+1 more)

### Community 83 - "PlayerProfileFileStore"
Cohesion: 0.36
Nodes (4): PersistentPlayerProfileRecord, PlayerProfileFileStore, DefaultFilePath, FilePath

### Community 84 - "Dictionary"
Cohesion: 0.03
Nodes (70): Grant, JunctionCoordinator, ActiveRuleExceptions, Batches, Current, FrameFailures, Index, Model (+62 more)

### Community 85 - "TrafficJunctionOutcome"
Cohesion: 0.22
Nodes (8): TrafficJunctionOutcome, Counters, EntryActive, FrameId, Records, Report, SnapshotEffectiveFrame, SnapshotStale

### Community 86 - ".Entry"
Cohesion: 0.09
Nodes (13): HistoricalPairFingerprintRecord, Document, HistoricalMovement, HistoricalMovementReader, JunctionRecord, ModelRecord, MovementRecord, PairDecisionState (+5 more)

### Community 87 - "TrafficV2Admission"
Cohesion: 0.29
Nodes (5): TrafficV2Admission, Admitted, Code, Evidence, Model

### Community 89 - "RoadModelValidationIssue"
Cohesion: 0.18
Nodes (4): RoadModelCompilationException, Issues, RoadModelValidationIssue, RoadModelValidator

### Community 90 - "MainMenuScreen"
Cohesion: 0.11
Nodes (10): MainMenuFlowController, PlayerProfileStore, Current, HasProfile, IsFrozen, SessionSelection, CharacterOption, Primary (+2 more)

### Community 91 - "PathHorizon"
Cohesion: 0.06
Nodes (29): HorizonEnd, ExitPortal, LookAheadLimit, IPathGeometry, LengthMeters, Spans, PathHorizon, End (+21 more)

### Community 92 - "RoadLineage"
Cohesion: 0.13
Nodes (8): Entry, FileLayout, FileRecord, LineageResolution, RoadLineage, Live, ModelId, Tombstones

### Community 94 - "V1SourceSet"
Cohesion: 0.14
Nodes (8): V1ModuleKind, Crossroads, RoadSegment, Roundabout, TJunction, TunnelPortal, V1SourceSet, IsValid

### Community 95 - "JunctionConflictIndex"
Cohesion: 0.06
Nodes (26): Entry, JunctionConflictIndex, IncompatiblePairCount, Model, PairFact, TraversalComparer, TraversalFacts, TraversalPairFacts (+18 more)

### Community 96 - "RightOfWayRelation"
Cohesion: 0.16
Nodes (13): Entry, RightOfWay, RightOfWayRelation, Ambiguous, FromLeft, FromRight, Opposite, Same (+5 more)

### Community 97 - "VehicleWheel"
Cohesion: 0.33
Nodes (6): VehicleWheel, AxleIndex, IsDriven, IsSteering, LocalPosition, Radius

### Community 98 - "GateAReviewWindow"
Cohesion: 0.11
Nodes (10): OverlayInstance, OverlayPrimitive, IsBox, ConflictFilter, All, Approach, Movement, Zone (+2 more)

### Community 100 - "Vector3"
Cohesion: 0.16
Nodes (7): CircleFit, HermiteSegment, RoadCurveBuilder, SegmentWalker, RoadCurveSample, RoadLineSegment, StopLineProjection

### Community 104 - "ManeuverGeometryCause"
Cohesion: 0.09
Nodes (20): ManeuverGeometryCause, AdjacencyNotCovered, CorridorEnd, CorridorTooShort, EnvelopeExceeded, EnvelopeNotContiguous, InvalidInput, LateralAccelerationExceeded (+12 more)

### Community 106 - "RageRoadEventFlowController"
Cohesion: 0.13
Nodes (9): RageRoadEventFlowController, NetworkedRunState, RageRoadEventLifecycle, RageRoadEventState, Confrontation, Idle, Resolved, RewardGranted (+1 more)

### Community 108 - "JunctionDistances"
Cohesion: 0.36
Nodes (3): JunctionDistances, EngageThresholdMeters, RequestThresholdMeters

### Community 109 - "PairSweep"
Cohesion: 0.21
Nodes (5): ConflictCandidate, PairSweep, IsCandidate, RefineContext, RoadModelValidationProfile

### Community 110 - "RoadGeometryValidator"
Cohesion: 0.17
Nodes (4): DatumTrace, GroundedCorridor, RoadGeometryValidator, LaneCorridor

### Community 111 - "MovementRole"
Cohesion: 0.29
Nodes (5): MovementRole, RoundaboutContinuation, RoundaboutEntry, RoundaboutExit, Turn

### Community 112 - "VehicleClassMask"
Cohesion: 0.29
Nodes (7): VehicleClassMask, All, Bus, Car, Emergency, None, Truck

### Community 114 - "LongitudinalArbitration.cs"
Cohesion: 0.17
Nodes (14): JunctionEntryInput, Active, LongitudinalLeader, LongitudinalObstacle, LongitudinalPerception, HasLeader, Leader, Obstacles (+6 more)

### Community 115 - "RouteReason"
Cohesion: 0.20
Nodes (10): RouteReason, DestinationUnavailable, DestinationUnreachable, InvalidStart, NoRouteAfterObjective, NoRouteToObjective, ObjectiveUnknown, Requested (+2 more)

### Community 116 - "SpeedConstraint"
Cohesion: 0.13
Nodes (15): SpeedConstraint, AnticipatedDeceleration, CurrentSpeedDeceleration, CurveLimit, DesiredSpeed, HorizonTerminalStop, JunctionEntry, LeaderFollowing (+7 more)

### Community 117 - "LongitudinalArbitration.cs"
Cohesion: 0.15
Nodes (13): StopHoldPhase, Entered, Holding, None, Released, StopHoldRelease, GapOpened, GrantEffective (+5 more)

### Community 118 - "TrafficSettingsDef"
Cohesion: 0.13
Nodes (12): TrafficSettingsDef, ConnectorJoinDistance, DefaultLitterThrowers, DefaultTargetPopulation, EdgeBudgetFactor, Id, MaxLitterThrowers, MaxTargetPopulation (+4 more)

### Community 119 - "ConflictSweepRefinement.cs"
Cohesion: 0.33
Nodes (5): RefinementOutcome, BudgetExhausted, ProvenDisjoint, Unresolved, Witness

### Community 120 - "DriverProfile"
Cohesion: 0.13
Nodes (14): DriverModel, DriverProfile, AimPointRecallSpeed, ComfortableDeceleration, Consistency, DesiredSpeed, LaneChangeEvaluationInterval, LaneChangeThreshold (+6 more)

### Community 121 - "StatusFilter"
Cohesion: 0.29
Nodes (7): StatusFilter, Inchangees, Modifiees, Nouvelles, Retirees, SansDecisionConfirmee, Tous

### Community 122 - "ReferenceTrack"
Cohesion: 0.08
Nodes (25): BodyState, GaugeBox, Rho, InterStepResult, BoundMeters, LipschitzMeters, ModelVerified, Pieces (+17 more)

### Community 123 - "RageDisposition"
Cohesion: 0.11
Nodes (12): RageThreshold, Disposition, MinValue, IRageDispositionSource, CurrentDisposition, RageDisposition, Block, Calm (+4 more)

### Community 124 - "PairRelation"
Cohesion: 0.29
Nodes (7): PairRelation, Candidate, EnvelopeOnly, FailClosed, Following, NoContact, SameApproach

### Community 125 - "TrafficV2VehicleDriver"
Cohesion: 0.04
Nodes (53): TrafficV2LifecycleState, Active, Faulted, TrafficV2VehicleDriver, Blockers, ContactEpisodes, Contacts, CurrentRoute (+45 more)

### Community 126 - "PairReviewWindow"
Cohesion: 0.09
Nodes (10): PairReview, PairReviewActions, PairReviewEntry, PairReviewModel, PairReviewStatus, Modified, New, Removed (+2 more)

### Community 127 - "PortalTrafficSpawner"
Cohesion: 0.12
Nodes (12): PortalTrafficSpawner, Composition, CompositionFrozen, LivePopulation, LiveV2Population, LiveV2Vehicles, ScenarioInsertions, V2Insertions (+4 more)

### Community 128 - "SafetyResult"
Cohesion: 0.07
Nodes (32): MotionCommand, IsFinite, RuleExceptions, AuthorizedContact, Valid, NearFieldSample, SafetyFilter, SafetyLimits (+24 more)

### Community 129 - "RoadRageBootstrap"
Cohesion: 0.11
Nodes (14): RoadRageBootstrap, Instance, LobbyJoin, LobbyRoom, LobbyRoster, NetworkPlayers, Notices, OnlineServices (+6 more)

### Community 130 - "TrafficV2StepRunner"
Cohesion: 0.07
Nodes (26): SignalPhaseInput, GridlockArc, GridlockCycle, Arcs, Key, Members, TrafficV2StepCost, TrafficV2StepRunner (+18 more)

### Community 131 - "SpeedProfileIssue"
Cohesion: 0.33
Nodes (6): SpeedProfileIssue, AccelerationBoundExceeded, InvalidSpeedProfile, None, PlanInfeasible, SteeringCeilingExceeded

### Community 132 - "TelemetrySample"
Cohesion: 0.25
Nodes (6): TelemetrySample, LateralSpeed, LongitudinalSpeed, Slip, SlipAngleDegrees, Speed

### Community 133 - "RoadModelRecords.cs"
Cohesion: 0.08
Nodes (48): RoadModelCanonicalPayload, ConflictZone, ImportManifest, ImportManifestEntry, Junction, JunctionControl, JunctionControlKind, Priority (+40 more)

### Community 134 - "SpeedPlan"
Cohesion: 0.08
Nodes (27): DeferredLimit, DeferredLimitKind, RoadLimit, DeferredLimitState, DeferredAuthored, DeferredUnauthored, RoadLimitState, Applied (+19 more)

### Community 135 - "JunctionFeature"
Cohesion: 0.33
Nodes (6): JunctionFeature, Crossroads, Other, Roundabout, TJunction, Unspecified

### Community 136 - "LobbyJoinService"
Cohesion: 0.15
Nodes (13): LobbyJoinService, JoinedJoinCode, JoinedLobbyId, Status, LobbyJoinStatus, Idle, InvalidCode, Joined (+5 more)

### Community 137 - "RoadModelValidationCode"
Cohesion: 0.04
Nodes (53): RoadModelValidationCode, ArcPositionOutOfDomain, ConflictingMovementsGreenTogether, ConflictZoneMembershipInvalid, ConflictZoneTypingInvalid, ConnectionSeamBroken, ControlApproachInconsistent, ControlApproachMissing (+45 more)

### Community 138 - "VehicleProfileDef"
Cohesion: 0.21
Nodes (4): VehicleProfileDef, Id, Profile, RawId

### Community 139 - "CollisionReactionWeights"
Cohesion: 0.16
Nodes (11): CollisionReaction, Brake, Evade, MisReact, CollisionReactionWeights, Brake, Default, Evade (+3 more)

### Community 140 - "LeafState"
Cohesion: 0.33
Nodes (6): LeafState, Proven, Split, Unresolved, Witness, WitnessSplit

### Community 141 - "RoadModelDocument"
Cohesion: 0.07
Nodes (23): AdjacencyDto, BindingDto, ConnectionDto, ControlDto, CorridorDto, DocumentDto, EntryDto, GroupDto (+15 more)

### Community 142 - "RoadClass"
Cohesion: 0.33
Nodes (6): RoadClass, Arterial, Highway, Local, Service, Unspecified

### Community 143 - "LaneNode"
Cohesion: 0.14
Nodes (13): LaneNode, IsEntryPortal, IsExitPortal, IsIncomingConnector, IsOutgoingConnector, Role, Successors, TurnWeights (+5 more)

### Community 144 - "RuleExceptionReason"
Cohesion: 0.14
Nodes (14): RuleExceptionReason, Accepted, AlreadyActive, Expired, FrameUnavailable, Held, Malformed, RequesterAbsent (+6 more)

### Community 146 - "HostOwnedNetworkStateBehaviour"
Cohesion: 0.33
Nodes (4): HostOwnedNetworkStateBehaviour, IsHostAuthority, IHostOwnedRuntimeState, IsHostAuthority

### Community 147 - "RuleExceptionStatus"
Cohesion: 0.40
Nodes (5): RuleExceptionStatus, Accepted, Denied, Ended, Held

### Community 150 - "TacticalDecision"
Cohesion: 0.06
Nodes (33): RecoveryCommandInput, TacticalDecision, AcceptedAtFrame, Active, AwaitingRecovery, CollisionActive, Goal, GoalVersion (+25 more)

### Community 152 - "CandidateDiffState"
Cohesion: 0.40
Nodes (5): CandidateDiffState, Changed, Confirmed, New, Orphan

### Community 153 - "CollisionGoalPhase"
Cohesion: 0.40
Nodes (5): CollisionGoalPhase, AwaitingRecovery, Braking, None, Reacting

### Community 154 - "TacticalGoalKind"
Cohesion: 0.40
Nodes (5): TacticalGoalKind, CollisionResponse, Maneuver, Nominal, Recovery

### Community 155 - "WheelState"
Cohesion: 0.40
Nodes (5): WheelState, Compression, ContactPoint, Grounded, HubPosition

### Community 156 - "GridlockOutcome"
Cohesion: 0.50
Nodes (4): GridlockOutcome, Escalated, Exhausted, Progressed

### Community 158 - "PlanningDecision"
Cohesion: 0.25
Nodes (8): PlanningDecision, Motion, Observation, Path, PerceptionPath, Projection, Route, SpeedProfile

### Community 159 - "NetworkedPlayerState"
Cohesion: 0.14
Nodes (7): NetworkedPlayerLifecycleService, Instance, PlayerLifecycle, Alive, Dead, Disconnected, Downed

### Community 161 - "unityengine"
Cohesion: 0.11
Nodes (3): DevVehicleSpawner, RoadRage.Shared.Authoring, RoadRage.Features.Vehicles

### Community 162 - "ImportContext"
Cohesion: 0.17
Nodes (12): ImportContext, ImportedCurve, ImportedJunction, V1Edge, Key, V1Module, IsJunction, V1Node (+4 more)

### Community 164 - "DrivingPolicyProfile"
Cohesion: 0.11
Nodes (16): DriverProfileDef, CollisionReaction, EmotionModulation, Id, Policy, Profile, RawId, DrivingPolicyProfile (+8 more)

### Community 165 - "AuthoringDecisions"
Cohesion: 0.06
Nodes (35): AppliedWidth, AuthoringDecisions, AutomatedPairClassification, ConflictProven, ConservativeConflict, Following, ProvenDisjoint, ConflictDecision (+27 more)

### Community 167 - "ElementTrace"
Cohesion: 0.09
Nodes (18): CampaignTraceability, Elements, ElementStatus, Measured, NotMeasured, NotSelectable, ElementTrace, Key (+10 more)

### Community 169 - "V2FallbackReason"
Cohesion: 0.08
Nodes (31): ComposedDrive, V2ComposerDiagnostic, Fallback, FallbackHeld, FallbackStopOverrun, None, ProfileScalarNonFinite, RollingBackward (+23 more)

### Community 171 - "VehicleCoverage"
Cohesion: 0.11
Nodes (16): TrafficV2Settings, DeclaredTrackingTolerance, PerceptionLimits, StopHold, TrafficV2Verdict, Allowed, TrackingTolerance, Undeclared (+8 more)

### Community 172 - "TrackingTolerance"
Cohesion: 0.21
Nodes (4): StepContactAccumulator, TrackingToleranceResponse, Latched, LatchedAtStep

### Community 174 - "TrafficV2Code"
Cohesion: 0.12
Nodes (17): TrafficV2Code, Allowed, CampaignCompleted, DriverProfileMissing, FirstDecisionNotDrivable, GateAEvidenceMissing, GateAEvidenceStale, NotCoveredByGateA (+9 more)

### Community 176 - "VehicleDamageType"
Cohesion: 0.50
Nodes (4): VehicleDamageType, Brake, Engine, Wheel

### Community 182 - "JunctionRequestRejection"
Cohesion: 0.22
Nodes (9): JunctionRequestRejection, Fallback, NoDriver, None, NoOccupancy, NotHeadOfQueue, NotLocalized, NoTraversal (+1 more)

### Community 184 - "ManeuverPhase"
Cohesion: 0.33
Nodes (6): ManeuverPhase, Departure, None, Passing, Return, Settling

### Community 187 - "TireSample"
Cohesion: 0.22
Nodes (9): TireSample, Adherence, ForceMagnitude, GripUsage, Grounded, MaximumForce, NormalLoad, SlipAngleDegrees (+1 more)

### Community 191 - "LobbyPlayerSlotView"
Cohesion: 0.22
Nodes (5): LobbyPlayerSlotView, LobbyRosterEntry, DisplayName, PortraitTint, Ready

### Community 192 - "Vector2"
Cohesion: 0.08
Nodes (10): JunctionClearance, JunctionClearanceRow, PhysicalSetEmpty, JunctionClearanceSurface, JunctionClearanceWitness, Surface, Colliders, Name (+2 more)

### Community 193 - "RoadRage.Shared.Domain"
Cohesion: 0.07
Nodes (17): NetworkedBossState, NetworkedCrewEconomyState, SessionTrafficValue, ThirdPersonCameraConfiguration, RoadRage.Features.Players, RoadRage.Shared.Domain, RoadRage.Features.Run, RoadRage.Features.OnFoot (+9 more)

### Community 200 - "NetworkedPlayerPresentation"
Cohesion: 0.19
Nodes (4): NetworkedPlayerPresentation, CharacterCatalog, RenderedCharacterId, VisualInstance

### Community 216 - "LaneGraph"
Cohesion: 0.16
Nodes (6): LaneGraph, EntryPortals, ExitPortals, NodeCount, OrphanConnectors, TrafficSettings

### Community 217 - "TrafficPerception"
Cohesion: 0.05
Nodes (48): OccupancyExclusion, None, NotLocalized, OccupancyNotBounded, UndeclaredFootprint, SpatialEntry, SpatialQueryBuffer, Capacity (+40 more)

### Community 612 - "NetworkedAIVehicleDriverController"
Cohesion: 0.10
Nodes (8): NetworkedAIVehicleDriverController, HasReachedExitPortal, VehicleDriveIntent, BrakeReverse, Handbrake, IsIdle, Steer, Throttle

### Community 718 - "Lock-Rage Camera Fix Query"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle, Source Nodes

## Knowledge Gaps
- **1499 isolated node(s):** `Instance`, `Router`, `Notices`, `Profiles`, `ProfileFiles` (+1494 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1672 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **33 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RoadId` connect `JunctionConflictIndex` to `SafetyResult`, `TrafficV2StepRunner`, `SweepPose`, `RoadModelRecords.cs`, `ManeuverPath`, `.Localize`, `SpeedPlan`, `RecoveryRequest`, `RoadModelDocument`, `Blocker`, `RoadModelCanonicalWriter`, `MigrationReport`, `TacticalDecision`, `EmotionGains`, `.Core`, `AutomatedPairDecisionPolicy`, `EffectiveRuleException`, `V1ImportResult`, `RoadId`, `TrafficFrame`, `ConflictSweep`, `AuthoringDecisions`, `RoundaboutClearance`, `RoadCurve`, `.Measure`, `ManeuverEvaluation`, `TrafficV2Scenario`, `EffectivePolicy`, `AuthoredRoadModel`, `TrafficV2HazardCollector`, `CollisionResponseRequest`, `Vector2`, `.Decide`, `RoutePlan`, `TrafficV2Insertion`, `ManeuverCandidate`, `.Compute`, `TrafficDecisionProjection`, `StabilityBlocker`, `MeasurementRun`, `.EvaluateManeuver`, `JunctionSnapshot`, `KinematicOffsetBounds`, `Dictionary`, `.Entry`, `TrafficPerception`, `RoadModelValidationIssue`, `PathHorizon`, `RoadLineage`, `RightOfWayRelation`, `GateAReviewWindow`, `Vector3`, `ManeuverGeometryCause`, `PairSweep`, `RoadGeometryValidator`, `LongitudinalArbitration.cs`, `LongitudinalArbitration.cs`, `ReferenceTrack`, `TrafficV2VehicleDriver`, `PairReviewWindow`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.170) - this node is a cross-community bridge._
- **What connects `Instance`, `Router`, `Notices` to the rest of the system?**
  _1499 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `NetworkedPassengerActionIntent` be split into smaller, more focused modules?**
  _Cohesion score 0.10967741935483871 - nodes in this community are weakly interconnected._
- **Why does `TrafficV2VehicleDriver` connect `TrafficV2VehicleDriver` to `SafetyResult`, `TrafficV2StepRunner`, `system`, `SweepPose`, `RoadModelRecords.cs`, `GateAEvidenceParameters`, `.Localize`, `RecoveryRequest`, `Blocker`, `HostOwnedNetworkStateBehaviour`, `Collision`, `VehiclePhysicsBody`, `EmotionGains`, `TacticalDecision`, `EffectiveRuleException`, `RoadId`, `CollisionFacts`, `TrafficFrame`, `DrivingPolicyProfile`, `RecoverySupervisor`, `V2FallbackReason`, `VehicleCoverage`, `TrackingTolerance`, `EffectivePolicy`, `LongitudinalDecision`, `.Monitor`, `TrafficV2HazardCollector`, `CollisionResponseRequest`, `.Decide`, `Vector2`, `RoutePlan`, `TrafficV2Insertion`, `ManeuverCandidate`, `TrafficDecisionProjection`, `StabilityBlocker`, `.EvaluateManeuver`, `JunctionSnapshot`, `Dictionary`, `TrafficV2Admission`, `TrafficPerception`, `JunctionConflictIndex`, `NetworkedAIVehicleDriverController`, `Vector3`, `ManeuverGeometryCause`, `ReferenceTrack`, `PortalTrafficSpawner`?**
  _High betweenness centrality (0.105) - this node is a cross-community bridge._
- **Should `system` be split into smaller, more focused modules?**
  _Cohesion score 0.09208261617900172 - nodes in this community are weakly interconnected._
- **Why does `RunFlowController` connect `RunFlowController` to `NetworkedPassengerActionIntent`, `RoadRage.Shared.Domain`, `NetworkedVehicleDriverController`, `AIVehicleBehaviorDebugView`, `PlayerMode`, `NetworkedAIVehicleState`, `NetworkedVehicleState`, `MonoBehaviour`, `RunCheckpointHudScreen`, `NetworkedPlayerState`, `LocalOnFootController`, `RageTuningDef`, `Dictionary`, `DefinitionId`?**
  _High betweenness centrality (0.045) - this node is a cross-community bridge._
- **Should `ManeuverPath` be split into smaller, more focused modules?**
  _Cohesion score 0.08712121212121213 - nodes in this community are weakly interconnected._