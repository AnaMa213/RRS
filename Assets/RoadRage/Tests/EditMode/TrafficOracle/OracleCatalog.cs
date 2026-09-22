using System.Collections.Generic;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Evidence vocabulary from <c>V1-BEHAVIORAL-ORACLE.md</c> (Section "Evidence vocabulary").
    /// <see cref="Unclassified"/> is the zero/default value on purpose: a row that is never assigned
    /// an explicit classification below stays at this value, which is exactly what
    /// <c>TrafficOracleTests.EveryCatalogRowResolvesToANamedClassification</c> fails on.
    /// </summary>
    public enum OracleEvidenceClassification
    {
        Unclassified = 0,
        AutoEdit,
        AutoPlay,
        SourceGuard,
        OwnerAccepted,
        Manual,
        Gap,
        Negative
    }

    /// <summary>One piece of evidence bound to a catalog row: a test's full name and its own evidence tier.</summary>
    public readonly struct BoundTest
    {
        public readonly string FullName;
        public readonly OracleEvidenceClassification Classification;

        public BoundTest(string fullName, OracleEvidenceClassification classification)
        {
            FullName = fullName;
            Classification = classification;
        }
    }

    /// <summary>One row of the accepted V1 Behavioral Oracle catalog.</summary>
    public sealed class OracleRow
    {
        public readonly string Id;
        public readonly string ContractedBehavior;
        public readonly OracleEvidenceClassification Classification;
        public readonly IReadOnlyList<BoundTest> BoundTests;

        public OracleRow(string id, string contractedBehavior, OracleEvidenceClassification classification, params BoundTest[] boundTests)
        {
            Id = id;
            ContractedBehavior = contractedBehavior;
            Classification = classification;
            BoundTests = boundTests;
        }
    }

    /// <summary>
    /// Static transcription of every <c>V1-A01</c>..<c>V1-G04</c> row of
    /// <c>_bmad-output/planning-artifacts/traffic-v2/V1-BEHAVIORAL-ORACLE.md</c> (accepted 2026-09-21,
    /// code baseline <c>210f48811e3f99fbb93c5d2aa75885e65edcf878</c>).
    ///
    /// Transcribed, not re-derived (Design Notes): the catalog carries 41 distinct row ids
    /// (<c>V1-A01</c>..<c>V1-A06</c>, <c>V1-B01</c>..<c>V1-B06</c>, <c>V1-C01</c>..<c>V1-C05</c>,
    /// <c>V1-D01</c>..<c>V1-D09</c>, <c>V1-E01</c>..<c>V1-E05</c>, <c>V1-F01</c>..<c>V1-F06</c>,
    /// <c>V1-G01</c>..<c>V1-G04</c>) -- the source document's own row count, not the "30" figure in
    /// this story's prose. The <c>Classification</c> here is the evidence-vocabulary tag
    /// (AUTO-EDIT/AUTO-PLAY/SOURCE-GUARD/OWNER-ACCEPTED/MANUAL/GAP/NEGATIVE), read out of each row's
    /// "Current evidence" cell -- a different axis from the source document's own KEEP/KEEP BUT
    /// REDESIGN/REMOVE-REPLACE/NEW V2 capability column, which is a V2-parity classification and is
    /// not reproduced here.
    /// </summary>
    public static class OracleCatalog
    {
        private const OracleEvidenceClassification AutoEdit = OracleEvidenceClassification.AutoEdit;
        private const OracleEvidenceClassification AutoPlay = OracleEvidenceClassification.AutoPlay;
        private const OracleEvidenceClassification SourceGuard = OracleEvidenceClassification.SourceGuard;

        private static BoundTest Edit(string fullName) => new BoundTest(fullName, AutoEdit);
        private static BoundTest Play(string fullName) => new BoundTest(fullName, AutoPlay);
        private static BoundTest Guard(string fullName) => new BoundTest(fullName, SourceGuard);

        public static readonly IReadOnlyList<OracleRow> Rows = new List<OracleRow>
        {
            // ---------------------------------------------------- A. Authority, intent and physics
            new OracleRow("V1-A01", "Only the host advances authoritative AI decisions and replicated AI state.", AutoEdit,
                Edit("Story54RageDrivenAiBehaviorStatesTests.BehaviorIsDerivedHostOnlyFromTheVehiclesOwnRage"),
                Edit("Story57AiTrafficClientPresentationTests.EveryReplicatedStateNetworkVariableIsEveryoneReadAndServerWrite")),

            new OracleRow("V1-A02", "One AI decision step submits one VehicleDriveIntent; normal driving does not write Rigidbody position, rotation or velocity.", AutoEdit,
                Edit("Story514AiDrivesByIntentTests.TheDrivingStepWritesNoVelocityPositionOrRotation"),
                Guard("Story514AiDrivesByIntentTests.TheIntentIsSubmittedToTheSamePhysicsLayerAsThePlayerVehicle")),

            new OracleRow("V1-A03", "Player and AI vehicles use the same VehiclePhysicsBody and physical profile contract.", AutoEdit,
                Edit("Story514AiDrivesByIntentTests.TheAiPrefabSharesTheSamePhysicsLayerAndProfileAsThePlayerPrefab")),

            new OracleRow("V1-A04", "Missing driver or physics profiles fail inert with a diagnostic, never with hard-coded driving fallback.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.AMissingProfileLeavesTheVehicleInertWithASingleWarningAndNoHardCodedFallback")),

            new OracleRow("V1-A05", "A pushed AI can inherit physical displacement instead of being snapped back by the nominal drive step.", OracleEvidenceClassification.OwnerAccepted),

            new OracleRow("V1-A06", "Vehicle collisions remain finite, bounded and damage remains host-only with existing occupant filters.", AutoEdit,
                Edit("Story515CredibleCollisionsAndDamageIntegrationTests.CollisionDamageRemainsHostOnlyAndPreservesExistingFilters"),
                Edit("Story515CredibleCollisionsAndDamageIntegrationTests.MeasuredProfileKeepsTheLeastCostNoTunnelModeAndDamageBreakpoints"),
                Play("Story515CredibleCollisionsAndDamageIntegrationPlayModeTests.PrefabCollisionsAreFiniteBoundedAndOrderedWithImpact"),
                Play("Story515CredibleCollisionsAndDamageIntegrationPlayModeTests.OffsetVehicleCollisionIsFiniteBoundedAndSeparatesAfterImpact"),
                Play("Story515CredibleCollisionsAndDamageIntegrationPlayModeTests.LeastCostCollisionModeWithoutTunnellingIsRetainedForThirtyPrefabBodies")),

            // -------------------------------------------- B. Longitudinal driving and perception
            new OracleRow("V1-B01", "Free-road acceleration is finite and fades to zero at desired speed.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.FreeRoadAcceleratesAndFadesAsSpeedApproachesDesiredSpeed"),
                Edit("Story59ParameterizedDriverModelTests.AtDesiredSpeedAccelerationIsZeroAndNeverPositive")),

            new OracleRow("V1-B02", "A stopped leader causes finite firm braking, including a zero/collapsed gap.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.StoppedLeaderProducesFirmFiniteBraking"),
                Edit("Story59ParameterizedDriverModelTests.ZeroGapProducesBoundedFiniteDeceleration")),

            new OracleRow("V1-B03", "Following settles near the authored minimum gap measured bumper-to-bumper.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.TheIdmSettlesAtTheAuthoredMinimumGapBehindAStoppedLeader"),
                Edit("Story59ParameterizedDriverModelTests.LeaderDetectionMeasuresTheGapFromTheBumperNotTheCenterOfMass")),

            new OracleRow("V1-B04", "Leader sensing covers the steered heading/curve and sees walking players, not only rigidbodies.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.LeaderDetectionSweepsAVolumeAlongTheSteeredHeadingNotAThinNoseRay"),
                Edit("Story59ParameterizedDriverModelTests.LeaderDetectionSeesWalkingPlayersNotJustRigidbodies")),

            new OracleRow("V1-B05", "Perception queries are allocation-bounded and report buffer saturation.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.PhysicsQueryBuffersAreMarginedWellBeyondCurrentSceneDensityAndWarnOnSaturation")),

            new OracleRow("V1-B06", "Acceleration response smoothing is finite, bounded and deterministic for the same inputs.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.SmoothedAccelerationConvergesTowardTheTargetWithoutOvershoot"),
                Edit("Story59ParameterizedDriverModelTests.ANearInstantReactionTimeTracksTheTargetFromTheFirstStep"),
                Edit("Story59ParameterizedDriverModelTests.AZeroOrNegativeReactionTimeIsClampedToAPositiveFloorAndStaysFinite")),

            // ------------------------------------------ C. Driver personality and emotional state
            new OracleRow("V1-C01", "Authored driver profiles own driving parameters; the controller owns no fallback drive constants.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.TheDefaultDriverProfileAssetIsAuthoredAndValid"),
                Edit("Story59ParameterizedDriverModelTests.TheControllerHoldsNoDriveConstantsAndReadsEveryParameterFromTheDef")),

            new OracleRow("V1-C02", "Desired-speed variation is deterministic and bounded by the consistency envelope.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.APerfectlyConsistentDriverHasNoDesiredSpeedNoise"),
                Edit("Story59ParameterizedDriverModelTests.AnErraticDriverStaysWithinTheSharedEnvelope"),
                Edit("Story59ParameterizedDriverModelTests.TheNoiseIsDeterministicForTheSameConsistencyPhaseAndInstant")),

            new OracleRow("V1-C03", "Rage disposition changes effective driver parameters without mutating the base personality.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.ImmobilizingDispositionsZeroTheEffectiveDesiredSpeed"),
                Edit("Story59ParameterizedDriverModelTests.DispositionModulationLeavesThePersonalityParametersUntouched")),

            new OracleRow("V1-C04", "Each vehicle derives behavior only from its own rage state; missing rage falls back to calm.", AutoEdit,
                Edit("Story54RageDrivenAiBehaviorStatesTests.EachVehicleDerivesItsOwnBehaviorWithoutAffectingTheOthers"),
                Edit("Story54RageDrivenAiBehaviorStatesTests.MissingRageComponentLeavesTheVehicleOnTheCalmFallback")),

            new OracleRow("V1-C05", "Immobilizing dispositions stop route pursuit and do not trigger stuck recovery.", AutoEdit,
                Edit("Story54RageDrivenAiBehaviorStatesTests.BlockAndConfrontationCapableStopPursuingTheRoute")),

            // ------------------------------------------- D. Road topology, routing and lifecycle
            new OracleRow("V1-D01", "Normal traffic enters and leaves only at authored portals.", AutoPlay,
                Play("Story510RoutedTrafficPlayModeTests.TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals"),
                Guard("Story510LaneGraphAndRoutedTrafficTests.NothingRemovesATrafficVehicleAnywhereButAnExitPortal")),

            new OracleRow("V1-D02", "A crowded entry defers insertion rather than silently dropping demand.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.ACrowdedPortalQueuesTheInsertionInsteadOfAbandoningIt")),

            new OracleRow("V1-D03", "A reusable entry/exit portal does not despawn a vehicle at birth but may accept it after circulation.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.AnEntryPortalMarkedReusableAlsoAcceptsOutgoingVehicles"),
                Edit("Story510LaneGraphAndRoutedTrafficTests.AVehicleSpawnedOnAReusableExitPortalIsNotMarkedExitedBeforeItHasActuallyDriven")),

            new OracleRow("V1-D04", "Authored turn weights produce deterministic varied routes and have a diagnosed fallback for invalid weights.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.WeightedTurnDrawFollowsAuthoredRatiosAndIsDeterministicPerSeed"),
                Edit("Story510LaneGraphAndRoutedTrafficTests.WeightedTurnDrawFallsBackToFirstSuccessorWhenWeightsAreAbsentOrSumToZero"),
                Edit("Story510LaneGraphAndRoutedTrafficTests.TwoVehiclesEnteringTheSameMvpRunPortalTakeDifferentRoutes")),

            new OracleRow("V1-D05", "Vehicles do not wander forever when a destination/exit cannot be reached normally.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.EdgeBudgetOnlyTripsPastTheAuthoredMultipleOfTheGraphSize"),
                Edit("Story510LaneGraphAndRoutedTrafficTests.RedirectPicksTheSuccessorThatClosesTheDistanceToTheExit"),
                Edit("Story510LaneGraphAndRoutedTrafficTests.ADeadEndRedirectsTowardTheNearestExitInsteadOfEndingTheRoute")),

            new OracleRow("V1-D06", "A graph with no exit reports failure and does not delete traffic.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.AGraphWithoutAnyExitPortalReportsNoReachableExitInsteadOfRemovingAnything")),

            new OracleRow("V1-D07", "Modular connectors join only within distance and travel-direction constraints; orphans are reported.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.ConnectorsJoinBelowTheAuthoredThresholdAndAreReportedOrphanBeyondIt"),
                Edit("Story510LaneGraphAndRoutedTrafficTests.ConnectorsNeverJoinAgainstTheDirectionOfTravel")),

            new OracleRow("V1-D08", "The current point-node representation, hierarchy-order IDs and runtime proximity joins are not V2 contracts.", OracleEvidenceClassification.Manual),

            new OracleRow("V1-D09", "Traffic population comes from synchronized session values within authored bounds.", AutoEdit,
                Edit("Story516LobbyConfigurableTrafficSettingsTests.TheSpawnerResolvesTheSessionValueFromTheRunStateAndFallsBackOnlyOnTheAuthoredDef"),
                Edit("Story516LobbyConfigurableTrafficSettingsTests.AnAcceptedStepUpdatesTheSessionValueAndPublishesIt")),

            // -------------------------------------------------- E. Path following and perturbation
            new OracleRow("V1-E01", "Nominal traffic progresses through the authored district without entering the curb footprint.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.NominalAiTrafficCrossesTheCurbedCrossroadsWithoutEnteringTheCurbFootprint")),

            new OracleRow("V1-E02", "A perturbed vehicle does not orbit a missed waypoint forever and eventually reaches an exit.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.PassedWaypointInsideTurnCircleIsReportedUnreachable"),
                Edit("Story510LaneGraphAndRoutedTrafficTests.PerturbedVehiclesAlwaysReachAnExitPortalInTheAuthoredDistrict")),

            new OracleRow("V1-E03", "Steering target remains continuous across node changes.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.TheAimPointStaysContinuousAcrossNodeChangesOnTheAuthoredDistrict")),

            new OracleRow("V1-E04", "Self-avoidance prevents the authored ring replay from cycling forever.", AutoEdit,
                Edit("Story510LaneGraphAndRoutedTrafficTests.ARingWalkByTheDriverNeverRevisitsANodeAndLeavesByABranch"),
                Edit("Story510LaneGraphAndRoutedTrafficTests.TheRestrictedRingWalkStaysSelfAvoidingWhereTheUnrestrictedDrawCompletesLaps")),

            new OracleRow("V1-E05", "AI drives along its route under the authored driver profile.", AutoPlay,
                Play("Story59ParameterizedDriverModelPlayModeTests.AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile")),

            // ----------------------------------------------------- F. Waiting, faults and recovery
            new OracleRow("V1-F01", "Correctly stopping behind a leader is not classified as a blockage.", AutoEdit,
                Edit("Story59ParameterizedDriverModelTests.StoppingBehindALeaderIsADeliberateStopNotABlockage"),
                Edit("Story59ParameterizedDriverModelTests.NoLeaderMeansNoDeliberateStopSoARealWedgeStillCounts"),
                Edit("Story59ParameterizedDriverModelTests.ACollapsedGapIsAWedgeNotADeliberateStop")),

            new OracleRow("V1-F02", "V1 rollover/void/stuck recovery resets velocity and teleports to the current waypoint without advancing route state.", SourceGuard,
                Guard("Story52BasicAiRouteFollowingAndRecoveryTests.DriverControllerAppliesRecoveryWithoutRpcOrAdvancingWaypointIndex")),

            new OracleRow("V1-F03", "V1 avoids a stuck teleport while a player is nearby or a leader wait is deliberate.", SourceGuard,
                Guard("Story59ParameterizedDriverModelTests.TheControllerNeverTeleportsAVehicleThatIsDeliberatelyStoppedOrWatchedByAPlayer")),

            new OracleRow("V1-F04", "A significant collision temporarily interrupts nominal driving until physical instability resolves.", OracleEvidenceClassification.Gap),
            new OracleRow("V1-F05", "After a collision, the vehicle can physically rejoin a valid lane and resume a valid route without teleport/despawn.", OracleEvidenceClassification.Gap),
            new OracleRow("V1-F06", "Collision reactions may vary by authored driver policy while simulation invariants remain enforced.", OracleEvidenceClassification.Gap),

            // ------------------------------------------------ G. Networking, presentation and scale
            new OracleRow("V1-G01", "Client presentation consumes replicated state without a second synchronization path.", AutoEdit,
                Edit("Story57AiTrafficClientPresentationTests.EveryReplicatedStateNetworkVariableIsEveryoneReadAndServerWrite"),
                Edit("Story57AiTrafficClientPresentationTests.ClientViewsOnlyReadReplicatedState"),
                Play("Story57AiTrafficClientPresentationPlayModeTests.HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels")),

            new OracleRow("V1-G02", "AI behavior has a text-accessible debug representation.", AutoEdit,
                Edit("Story54RageDrivenAiBehaviorStatesTests.BehaviorDebugViewRendersTextOnlyAndNeverWritesSharedState")),

            new OracleRow("V1-G03", "Roughly 30 vehicle bodies retain the chosen collision mode without tunnelling in the existing collision bench.", AutoPlay,
                Play("Story515CredibleCollisionsAndDamageIntegrationPlayModeTests.LeastCostCollisionModeWithoutTunnellingIsRetainedForThirtyPrefabBodies")),

            new OracleRow("V1-G04", "Near/mid/far traffic LOD behavior is not implemented in V1.", OracleEvidenceClassification.Negative)
        };
    }
}
