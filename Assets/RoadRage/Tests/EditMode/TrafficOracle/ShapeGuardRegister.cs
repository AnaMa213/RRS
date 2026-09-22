using System.Collections.Generic;

namespace RoadRage.Tests.EditMode
{
    /// <summary>One SOURCE-GUARD test paired with the exact V1 symbol it pins.</summary>
    public readonly struct ShapeGuardEntry
    {
        /// <summary>Full name (fixture.method) of the guarding test, as it appears in <see cref="OracleCatalog"/>.</summary>
        public readonly string TestFullName;

        /// <summary>The V1 symbol whose shape/text the guard actually pins -- not a behavioral claim.</summary>
        public readonly string PinnedV1Symbol;

        /// <summary>The catalog row this guard is evidence for.</summary>
        public readonly string OracleRowId;

        public ShapeGuardEntry(string testFullName, string pinnedV1Symbol, string oracleRowId)
        {
            TestFullName = testFullName;
            PinnedV1Symbol = pinnedV1Symbol;
            OracleRowId = oracleRowId;
        }
    }

    /// <summary>
    /// Registry of every SOURCE-GUARD test referenced by <see cref="OracleCatalog"/>, separate from the
    /// tests that are direct behavioral evidence. This is the list Story 5.48 (V1 retirement) consumes
    /// to know what retires with V1 versus what is an architecture guard worth re-anchoring on a V2
    /// symbol. A SOURCE-GUARD test protects an implementation shape (source text, reflection, token
    /// counts) -- useful, but it is not proof that the guarded behavior actually occurs.
    /// </summary>
    public static class ShapeGuardRegister
    {
        public static readonly IReadOnlyList<ShapeGuardEntry> Entries = new List<ShapeGuardEntry>
        {
            new ShapeGuardEntry(
                "Story514AiDrivesByIntentTests.TheIntentIsSubmittedToTheSamePhysicsLayerAsThePlayerVehicle",
                "NetworkedAIVehicleDriverController (VehicleDriveIntent submission call site)",
                "V1-A02"),

            new ShapeGuardEntry(
                "Story510LaneGraphAndRoutedTrafficTests.NothingRemovesATrafficVehicleAnywhereButAnExitPortal",
                "PortalTrafficSpawner / NetworkedAIVehicleDriverController (vehicle removal call sites)",
                "V1-D01"),

            new ShapeGuardEntry(
                "Story52BasicAiRouteFollowingAndRecoveryTests.DriverControllerAppliesRecoveryWithoutRpcOrAdvancingWaypointIndex",
                "NetworkedAIVehicleDriverController.RecoverAtWaypoint",
                "V1-F02"),

            new ShapeGuardEntry(
                "Story59ParameterizedDriverModelTests.TheControllerNeverTeleportsAVehicleThatIsDeliberatelyStoppedOrWatchedByAPlayer",
                "NetworkedAIVehicleDriverController (teleport-recovery guard clause: DriverModel.IsDeliberateStop / IsAnyPlayerWithinClearanceRadius)",
                "V1-F03")
        };
    }
}
