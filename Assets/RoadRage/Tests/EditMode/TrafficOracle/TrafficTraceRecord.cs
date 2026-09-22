using System.Collections.Generic;
using RoadRage.Features.Vehicles;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// One behavior-neutral sample of a traffic agent's observable state at one simulation frame.
    ///
    /// This is the trace contract the replay/compare harness (<see cref="TrafficTraceComparer"/>)
    /// works against. It records contracted BEHAVIOR, not internal architecture (Design Notes): V1 and
    /// a later V2 are never required to compute a frame the same way internally, only to produce
    /// comparable values for the fields each can genuinely observe.
    ///
    /// Fields are split in two groups:
    /// - Common fields: V1 can genuinely populate all of these today from its own replay/decision step
    ///   (see <c>Story510LaneGraphAndRoutedTrafficTests.ReplayRoute</c> and
    ///   <c>NetworkedAIVehicleDriverController</c>).
    /// - Optional/versioned diagnostic fields: V1 has no such concept (junction grants, safety
    ///   verdicts, recovery state, a distinct path/motion plan id -- see BC-4/BC-5/BC-7/BC-9/BC-12 in
    ///   the sprint change proposal). They are nullable and V1 traces simply leave them null; no field
    ///   here implies V1 and V2 share internal decision representation.
    /// </summary>
    public sealed class TrafficTraceFrame
    {
        // ---------------------------------------------------------------- common fields (V1-capable)

        /// <summary>Simulation step index within the replayed scenario.</summary>
        public int Frame;

        /// <summary>Stable identity of the traced vehicle within the scenario (not a NetworkObjectId).</summary>
        public int VehicleId;

        /// <summary>World-space position, planar (y is not asserted by the comparer's default tolerance).</summary>
        public Vector3 Position;

        /// <summary>Heading around the world up axis, in degrees.</summary>
        public float YawDegrees;

        /// <summary>Forward planar speed, in meters/second.</summary>
        public float Speed;

        /// <summary>V1: the LaneGraph node index currently targeted. A structural/discrete id, not a coordinate.</summary>
        public int RoadElementId;

        /// <summary>
        /// V1: traversed-edge count on the current replay, the only progress signal V1's point-node
        /// routing genuinely has. Numeric/behavioral, compared with tolerance, not required to match a
        /// V2 lane-coordinate representation byte-for-byte.
        /// </summary>
        public float RouteProgress;

        /// <summary>V1: coarse textual goal ("SeekWaypoint", "Idle", ...). Discrete/structural: exact match.</summary>
        public string Goal;

        /// <summary>
        /// V1: named blockers active this frame (may be empty -- V1 has no multi-blocker set, so this
        /// is at most one entry today; the array shape exists so a V2 trace with several concurrent
        /// blockers compares against the same contract). Discrete/structural: exact match, order-sensitive.
        /// </summary>
        public string[] Blockers = System.Array.Empty<string>();

        /// <summary>The one VehicleDriveIntent this frame would submit to the physics layer. Discrete/structural: exact match.</summary>
        public VehicleDriveIntent FinalIntent;

        // ------------------------------------------------- optional/versioned diagnostic fields (V2-only)

        /// <summary>Junction movement grant/denial reason. Null for every V1 trace: V1 has no junction coordinator.</summary>
        public string JunctionGrant;

        /// <summary>SafetyFilter verdict/reason. Null for every V1 trace: V1 has no separate safety filter.</summary>
        public string SafetyResult;

        /// <summary>RecoverySupervisor state. Null for every V1 trace: V1's recovery is the rollover/void/stuck teleport path, not a supervised state machine.</summary>
        public string RecoveryState;

        /// <summary>Identity of the active path/motion plan. Null for every V1 trace: V1 does not separate route/path/motion plan identities (AD-34/BC-4).</summary>
        public int? PathPlanId;
    }

    /// <summary>An ordered sequence of <see cref="TrafficTraceFrame"/>, one per simulation step for one replayed vehicle.</summary>
    public sealed class TrafficTrace
    {
        public readonly List<TrafficTraceFrame> Frames = new List<TrafficTraceFrame>();
    }
}
