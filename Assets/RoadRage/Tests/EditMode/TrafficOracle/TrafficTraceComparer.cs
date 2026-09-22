using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>Names a field the comparer can report a divergence on.</summary>
    public enum TraceDivergenceField
    {
        FrameCountMismatch,
        VehicleId,
        RoadElementId,
        Goal,
        Blockers,
        Position,
        YawDegrees,
        Speed,
        RouteProgress,
        Throttle,
        Steer,
        BrakeReverse,
        Handbrake
    }

    /// <summary>One reported divergence: which frame, which named field, and what was compared.</summary>
    public readonly struct TraceDivergence
    {
        public readonly int Frame;
        public readonly TraceDivergenceField Field;
        public readonly string Detail;

        public TraceDivergence(int frame, TraceDivergenceField field, string detail)
        {
            Frame = frame;
            Field = field;
            Detail = detail;
        }

        public override string ToString()
        {
            return "frame " + Frame + ": " + Field + " diverged (" + Detail + ")";
        }
    }

    /// <summary>
    /// Compares two ordered <see cref="TrafficTrace"/> sequences against declared per-field tolerances
    /// (Story 5.24). Discrete/structural fields (<see cref="TrafficTraceFrame.VehicleId"/>,
    /// <see cref="TrafficTraceFrame.RoadElementId"/>, <see cref="TrafficTraceFrame.Goal"/>,
    /// <see cref="TrafficTraceFrame.Blockers"/>) require exact equality. Numeric behavioral fields --
    /// <see cref="TrafficTraceFrame.Position"/>, <see cref="TrafficTraceFrame.YawDegrees"/>,
    /// <see cref="TrafficTraceFrame.Speed"/>, <see cref="TrafficTraceFrame.RouteProgress"/>, and each of
    /// <see cref="TrafficTraceFrame.FinalIntent"/>'s four components -- use a declared named epsilon
    /// (Design Notes: continuous control outputs, not symbolic labels, despite the field's
    /// "discrete/structural"-sounding name).
    ///
    /// Every numeric comparison is written <c>!(delta &lt;= epsilon)</c>, not <c>delta &gt; epsilon</c>:
    /// the latter is <c>false</c> for NaN under IEEE 754, which would silently pass a NaN divergence.
    /// This form fails (reports a divergence) on NaN instead.
    /// </summary>
    public static class TrafficTraceComparer
    {
        public const float PositionEpsilonMeters = 0.05f;
        public const float YawDegreesEpsilon = 1f;
        public const float SpeedEpsilonMetersPerSecond = 0.05f;
        public const float RouteProgressEpsilon = 0.001f;
        public const float ThrottleEpsilon = 0.01f;
        public const float SteerEpsilon = 0.01f;
        public const float BrakeReverseEpsilon = 0.01f;
        public const float HandbrakeEpsilon = 0.01f;

        /// <summary>Compares two traces frame-by-frame. Empty result means "compares equal under the declared contract".</summary>
        public static List<TraceDivergence> Compare(TrafficTrace expected, TrafficTrace actual)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (actual == null)
            {
                throw new ArgumentNullException(nameof(actual));
            }

            var divergences = new List<TraceDivergence>();

            if (expected.Frames.Count != actual.Frames.Count)
            {
                divergences.Add(new TraceDivergence(
                    -1, TraceDivergenceField.FrameCountMismatch,
                    expected.Frames.Count + " vs " + actual.Frames.Count));
                return divergences;
            }

            for (var i = 0; i < expected.Frames.Count; i++)
            {
                CompareFrame(expected.Frames[i], actual.Frames[i], divergences);
            }

            return divergences;
        }

        private static void CompareFrame(TrafficTraceFrame expected, TrafficTraceFrame actual, List<TraceDivergence> into)
        {
            var frame = expected.Frame;

            if (expected.VehicleId != actual.VehicleId)
            {
                into.Add(new TraceDivergence(frame, TraceDivergenceField.VehicleId, expected.VehicleId + " vs " + actual.VehicleId));
            }

            if (expected.RoadElementId != actual.RoadElementId)
            {
                into.Add(new TraceDivergence(frame, TraceDivergenceField.RoadElementId, expected.RoadElementId + " vs " + actual.RoadElementId));
            }

            if (!string.Equals(expected.Goal, actual.Goal, StringComparison.Ordinal))
            {
                into.Add(new TraceDivergence(frame, TraceDivergenceField.Goal, "'" + expected.Goal + "' vs '" + actual.Goal + "'"));
            }

            if (!BlockersEqual(expected.Blockers, actual.Blockers))
            {
                into.Add(new TraceDivergence(
                    frame, TraceDivergenceField.Blockers,
                    "[" + string.Join(",", expected.Blockers ?? Array.Empty<string>()) + "] vs [" + string.Join(",", actual.Blockers ?? Array.Empty<string>()) + "]"));
            }

            CompareNumeric(frame, TraceDivergenceField.Position, Vector3.Distance(expected.Position, actual.Position), PositionEpsilonMeters, into);
            CompareNumeric(frame, TraceDivergenceField.YawDegrees, Mathf.Abs(Mathf.DeltaAngle(expected.YawDegrees, actual.YawDegrees)), YawDegreesEpsilon, into);
            CompareNumeric(frame, TraceDivergenceField.Speed, Mathf.Abs(expected.Speed - actual.Speed), SpeedEpsilonMetersPerSecond, into);
            CompareNumeric(frame, TraceDivergenceField.RouteProgress, Mathf.Abs(expected.RouteProgress - actual.RouteProgress), RouteProgressEpsilon, into);

            CompareNumeric(frame, TraceDivergenceField.Throttle, Mathf.Abs(expected.FinalIntent.Throttle - actual.FinalIntent.Throttle), ThrottleEpsilon, into);
            CompareNumeric(frame, TraceDivergenceField.Steer, Mathf.Abs(expected.FinalIntent.Steer - actual.FinalIntent.Steer), SteerEpsilon, into);
            CompareNumeric(frame, TraceDivergenceField.BrakeReverse, Mathf.Abs(expected.FinalIntent.BrakeReverse - actual.FinalIntent.BrakeReverse), BrakeReverseEpsilon, into);
            CompareNumeric(frame, TraceDivergenceField.Handbrake, Mathf.Abs(expected.FinalIntent.Handbrake - actual.FinalIntent.Handbrake), HandbrakeEpsilon, into);
        }

        private static void CompareNumeric(int frame, TraceDivergenceField field, float delta, float epsilon, List<TraceDivergence> into)
        {
            if (!(delta <= epsilon))
            {
                into.Add(new TraceDivergence(frame, field, "delta " + delta + " > epsilon " + epsilon));
            }
        }

        private static bool BlockersEqual(string[] expected, string[] actual)
        {
            expected ??= Array.Empty<string>();
            actual ??= Array.Empty<string>();

            if (expected.Length != actual.Length)
            {
                return false;
            }

            for (var i = 0; i < expected.Length; i++)
            {
                if (!string.Equals(expected[i], actual[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
