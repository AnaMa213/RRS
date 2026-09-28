using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.24 -- the V2 regression oracle bench itself: catalog completeness, the shape-guard
    /// register, the trace comparer's self-check, and a determinism replay over the real authored
    /// MVP_Run district. This fixture asserts the bench is internally sound; it is not V1 behavioral
    /// evidence by itself (the catalog rows cite that evidence separately).
    /// </summary>
    [Category("Core")]
    public sealed class TrafficOracleTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

        // ------------------------------------------------------------------ catalog completeness

        [Test]
        public void EveryCatalogRowResolvesToANamedClassification()
        {
            foreach (var row in OracleCatalog.Rows)
            {
                Assert.That(row.Classification, Is.Not.EqualTo(OracleEvidenceClassification.Unclassified),
                    row.Id + " has no named classification");
            }
        }

        [Test]
        public void GapRowsCarryNoInventedTest()
        {
            foreach (var row in OracleCatalog.Rows)
            {
                if (row.Classification == OracleEvidenceClassification.Gap)
                {
                    Assert.That(row.BoundTests, Is.Empty, row.Id + " is GAP but cites a test -- GAP rows must not fake evidence");
                }
            }
        }

        [Test]
        public void EvidenceBearingRowsCiteATestAndNonEvidenceRowsCiteNone()
        {
            foreach (var row in OracleCatalog.Rows)
            {
                switch (row.Classification)
                {
                    case OracleEvidenceClassification.AutoEdit:
                    case OracleEvidenceClassification.AutoPlay:
                    case OracleEvidenceClassification.SourceGuard:
                        Assert.That(row.BoundTests, Is.Not.Empty, row.Id + " claims automated evidence but cites no test");
                        break;
                    case OracleEvidenceClassification.OwnerAccepted:
                    case OracleEvidenceClassification.Manual:
                    case OracleEvidenceClassification.Negative:
                        Assert.That(row.BoundTests, Is.Empty, row.Id + " is " + row.Classification + " but cites a test");
                        break;
                }
            }
        }

        [Test]
        public void CatalogCoversEveryAToGRowId()
        {
            var ids = OracleCatalog.Rows.Select(r => r.Id).ToList();
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count), "duplicate row id in catalog");
            Assert.That(ids.Count, Is.EqualTo(41), "expected 41 transcribed rows (Design Notes: the source document's own count, not the story prose's '30')");
        }

        // ------------------------------------------------------------------ shape-guard register

        [Test]
        public void ShapeGuardRegisterIsNonEmptyAndEveryEntryIsNamed()
        {
            Assert.That(ShapeGuardRegister.Entries, Is.Not.Empty);

            foreach (var entry in ShapeGuardRegister.Entries)
            {
                Assert.That(entry.TestFullName, Is.Not.Null.And.Not.Empty);
                Assert.That(entry.PinnedV1Symbol, Is.Not.Null.And.Not.Empty);
                Assert.That(entry.OracleRowId, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void EveryShapeGuardEntryCitesABoundTestActuallyClassifiedSourceGuardOnItsRow()
        {
            // A row's own overall Classification can be AUTO-EDIT while it also cites a supplementary
            // SOURCE-GUARD BoundTest (e.g. V1-A02) -- so the invariant checked here is per-BoundTest,
            // not "the row's overall classification is SOURCE-GUARD".
            foreach (var entry in ShapeGuardRegister.Entries)
            {
                var row = OracleCatalog.Rows.FirstOrDefault(r => r.Id == entry.OracleRowId);
                Assert.That(row, Is.Not.Null, entry.TestFullName + " cites unknown row " + entry.OracleRowId);

                var boundAsSourceGuard = row.BoundTests.Any(
                    b => b.FullName == entry.TestFullName && b.Classification == OracleEvidenceClassification.SourceGuard);
                Assert.That(boundAsSourceGuard, Is.True,
                    entry.TestFullName + " is not bound as a SOURCE-GUARD test on row " + entry.OracleRowId + " in OracleCatalog");
            }
        }

        [Test]
        public void EveryCatalogSourceGuardTestIsRegisteredInTheShapeGuardRegister()
        {
            foreach (var row in OracleCatalog.Rows)
            {
                foreach (var bound in row.BoundTests.Where(b => b.Classification == OracleEvidenceClassification.SourceGuard))
                {
                    var registered = ShapeGuardRegister.Entries.Any(
                        e => e.TestFullName == bound.FullName && e.OracleRowId == row.Id);
                    Assert.That(registered, Is.True,
                        bound.FullName + " is a SOURCE-GUARD on row " + row.Id + " but has no ShapeGuardRegister entry");
                }
            }
        }

        // ------------------------------------------------------------------ comparer self-check

        [Test]
        public void IdenticalTracesCompareEqualUnderTheDeclaredContract()
        {
            var a = SingleFrameTrace(BaseFrame());
            var b = SingleFrameTrace(BaseFrame());

            Assert.That(TrafficTraceComparer.Compare(a, b), Is.Empty);
        }

        [Test]
        public void AVehicleIdDivergenceIsReportedByField()
        {
            AssertSingleFieldDivergence(frame => frame.VehicleId = 99, TraceDivergenceField.VehicleId);
        }

        [Test]
        public void ARoadElementIdDivergenceIsReportedByField()
        {
            AssertSingleFieldDivergence(frame => frame.RoadElementId = 99, TraceDivergenceField.RoadElementId);
        }

        [Test]
        public void AGoalDivergenceIsReportedByField()
        {
            AssertSingleFieldDivergence(frame => frame.Goal = "Idle", TraceDivergenceField.Goal);
        }

        [Test]
        public void ABlockersDivergenceIsReportedByField()
        {
            AssertSingleFieldDivergence(frame => frame.Blockers = new[] { "RedSignal" }, TraceDivergenceField.Blockers);
        }

        [Test]
        public void APositionDivergenceOutsideToleranceIsReportedByField()
        {
            AssertSingleFieldDivergence(
                frame => frame.Position += new Vector3(TrafficTraceComparer.PositionEpsilonMeters * 10f, 0f, 0f),
                TraceDivergenceField.Position);
        }

        [Test]
        public void AYawDivergenceOutsideToleranceIsReportedByField()
        {
            AssertSingleFieldDivergence(
                frame => frame.YawDegrees += TrafficTraceComparer.YawDegreesEpsilon * 10f,
                TraceDivergenceField.YawDegrees);
        }

        [Test]
        public void ASpeedDivergenceOutsideToleranceIsReportedByField()
        {
            AssertSingleFieldDivergence(
                frame => frame.Speed += TrafficTraceComparer.SpeedEpsilonMetersPerSecond * 10f,
                TraceDivergenceField.Speed);
        }

        [Test]
        public void ARouteProgressDivergenceOutsideToleranceIsReportedByField()
        {
            AssertSingleFieldDivergence(
                frame => frame.RouteProgress += TrafficTraceComparer.RouteProgressEpsilon * 10f,
                TraceDivergenceField.RouteProgress);
        }

        // FinalIntent's four components (review loop 1): continuous control outputs, epsilon-tolerant,
        // not the discrete/exact bucket their field name might suggest.

        [Test]
        public void AThrottleDivergenceOutsideToleranceIsReportedByField()
        {
            AssertSingleFieldDivergence(
                frame => frame.FinalIntent = WithThrottle(frame.FinalIntent, TrafficTraceComparer.ThrottleEpsilon * 10f),
                TraceDivergenceField.Throttle);
        }

        [Test]
        public void ASteerDivergenceOutsideToleranceIsReportedByField()
        {
            AssertSingleFieldDivergence(
                frame => frame.FinalIntent = WithSteer(frame.FinalIntent, TrafficTraceComparer.SteerEpsilon * 10f),
                TraceDivergenceField.Steer);
        }

        [Test]
        public void ABrakeReverseDivergenceOutsideToleranceIsReportedByField()
        {
            AssertSingleFieldDivergence(
                frame => frame.FinalIntent = WithBrakeReverse(frame.FinalIntent, TrafficTraceComparer.BrakeReverseEpsilon * 10f),
                TraceDivergenceField.BrakeReverse);
        }

        [Test]
        public void AHandbrakeDivergenceOutsideToleranceIsReportedByField()
        {
            AssertSingleFieldDivergence(
                frame => frame.FinalIntent = WithHandbrake(frame.FinalIntent, TrafficTraceComparer.HandbrakeEpsilon * 10f),
                TraceDivergenceField.Handbrake);
        }

        [Test]
        public void ANaNInANumericFieldIsReportedAsADivergenceNotASilentMatch()
        {
            // delta > epsilon is false for NaN (IEEE 754) -- the comparer must use !(delta <= epsilon)
            // instead, or this case would silently pass.
            AssertSingleFieldDivergence(frame => frame.Speed = float.NaN, TraceDivergenceField.Speed);
        }

        /// <summary>
        /// Review loop 2 finding: Compare collects EVERY divergence on a frame, not just the first one
        /// found -- but every other self-check above only ever mutates a single field, so a regression
        /// back to "stop after first divergence" would pass all of them undetected. This mutates two
        /// unrelated fields (one discrete, one numeric) on the same frame and requires both to be named.
        /// </summary>
        [Test]
        public void MultipleDivergencesOnTheSameFrameAreAllReportedNotJustTheFirst()
        {
            var expected = SingleFrameTrace(BaseFrame());
            var actualFrame = BaseFrame();
            actualFrame.VehicleId = 99;
            actualFrame.Speed += TrafficTraceComparer.SpeedEpsilonMetersPerSecond * 10f;
            var actual = SingleFrameTrace(actualFrame);

            var fields = TrafficTraceComparer.Compare(expected, actual).Select(d => d.Field).ToList();

            Assert.That(fields, Does.Contain(TraceDivergenceField.VehicleId));
            Assert.That(fields, Does.Contain(TraceDivergenceField.Speed));
        }

        private static void AssertSingleFieldDivergence(Action<TrafficTraceFrame> mutateActual, TraceDivergenceField expectedField)
        {
            var expected = SingleFrameTrace(BaseFrame());
            var actualFrame = BaseFrame();
            mutateActual(actualFrame);
            var actual = SingleFrameTrace(actualFrame);

            var divergences = TrafficTraceComparer.Compare(expected, actual);

            Assert.That(divergences.Select(d => d.Field), Does.Contain(expectedField),
                "expected a " + expectedField + " divergence, got: " + string.Join("; ", divergences));
        }

        private static TrafficTraceFrame BaseFrame()
        {
            return new TrafficTraceFrame
            {
                Frame = 0,
                VehicleId = 1,
                Position = new Vector3(10f, 0f, 5f),
                YawDegrees = 90f,
                Speed = 6f,
                RoadElementId = 3,
                RouteProgress = 2f,
                Goal = "SeekWaypoint",
                Blockers = new[] { "Leader" },
                FinalIntent = new VehicleDriveIntent(throttle: 0.5f, steer: 0.1f, brakeReverse: 0f, handbrake: 0f)
            };
        }

        private static TrafficTrace SingleFrameTrace(TrafficTraceFrame frame)
        {
            var trace = new TrafficTrace();
            trace.Frames.Add(frame);
            return trace;
        }

        private static VehicleDriveIntent WithThrottle(VehicleDriveIntent intent, float delta) =>
            new VehicleDriveIntent(intent.Throttle + delta, intent.Steer, intent.BrakeReverse, intent.Handbrake);

        private static VehicleDriveIntent WithSteer(VehicleDriveIntent intent, float delta) =>
            new VehicleDriveIntent(intent.Throttle, intent.Steer + delta, intent.BrakeReverse, intent.Handbrake);

        private static VehicleDriveIntent WithBrakeReverse(VehicleDriveIntent intent, float delta) =>
            new VehicleDriveIntent(intent.Throttle, intent.Steer, intent.BrakeReverse + delta, intent.Handbrake);

        private static VehicleDriveIntent WithHandbrake(VehicleDriveIntent intent, float delta) =>
            new VehicleDriveIntent(intent.Throttle, intent.Steer, intent.BrakeReverse, intent.Handbrake + delta);

        // ------------------------------------------------------------------ determinism + CPU baseline

        /// <summary>
        /// Same seeded scenario replayed twice over the real authored MVP_Run district must compare
        /// equal under the declared trace contract. Both replays are also asserted to have reached a
        /// departed exit portal: a route that merely exhausts the step budget or orbits would otherwise
        /// produce two identically-truncated traces that compare equal for the wrong reason (review
        /// loop 1 finding).
        /// </summary>
        [Test]
        public void DeterministicSeededReplayProducesEqualTraceSequencesAndReachesAnExitPortal()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                Assert.That(graph.EntryPortals, Is.Not.Empty, "MVP_Run graph should have at least one entry portal");
                var entry = graph.EntryPortals[0];

                var traceA = new TrafficTrace();
                var reachedExitA = ReplayForTrace(graph, entry, seed: 11UL, traceA);

                var traceB = new TrafficTrace();
                var reachedExitB = ReplayForTrace(graph, entry, seed: 11UL, traceB);

                Assert.That(reachedExitA, Is.True, "replay A must reach a departed exit portal, not merely exhaust the step budget or orbit");
                Assert.That(reachedExitB, Is.True, "replay B must reach a departed exit portal, not merely exhaust the step budget or orbit");
                Assert.That(traceA.Frames, Is.Not.Empty);

                var divergences = TrafficTraceComparer.Compare(traceA, traceB);
                Assert.That(divergences, Is.Empty, "same seed must replay identically: " + string.Join("; ", divergences));
            });
        }

        /// <summary>
        /// Observational only (Design Notes/Task list): measures the cost of this EditMode replay
        /// harness itself, over the real authored MVP_Run district -- not V1's full FixedUpdate/physics
        /// runtime cost. Logged for docs/setup/story-5-24-oracle-bench-notes.md; asserts only finiteness,
        /// never a performance budget (no regression-risk perf gate is in this story's scope).
        /// </summary>
        [Test]
        public void ReplayHarnessCpuCostIsMeasuredAsObservationalData()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                Assert.That(graph.EntryPortals, Is.Not.Empty);
                var entry = graph.EntryPortals[0];

                const int Samples = 40;
                var millisecondsPerRun = new List<double>(Samples);

                for (var i = 0; i < Samples; i++)
                {
                    var trace = new TrafficTrace();
                    var timer = Stopwatch.StartNew();
                    ReplayForTrace(graph, entry, seed: (ulong)(1000 + i), trace);
                    timer.Stop();
                    millisecondsPerRun.Add(timer.Elapsed.TotalMilliseconds);
                }

                millisecondsPerRun.Sort();
                var median = millisecondsPerRun[millisecondsPerRun.Count / 2];
                var p95 = millisecondsPerRun[(int)Mathf.Ceil(millisecondsPerRun.Count * 0.95f) - 1];

                Debug.Log("[Story5.24] EditMode replay-harness cost over " + Samples + " full portal-to-exit replays on MVP_Run: "
                    + "median " + median.ToString("F3") + " ms, p95 " + p95.ToString("F3") + " ms per full replay "
                    + "(this is the kinematic replay harness cost -- ComputeSeekIntent/ResolveLookAheadPoint/routing per step -- "
                    + "not V1's full FixedUpdate/physics/perception runtime cost).");

                Assert.That(median, Is.GreaterThanOrEqualTo(0d).And.LessThan(double.PositiveInfinity));
            });
        }

        // ------------------------------------------------------------------ reflection existence check

        /// <summary>
        /// Every <see cref="BoundTest"/> and <see cref="ShapeGuardEntry"/> full name must resolve to an
        /// actual <c>[Test]</c>/<c>[UnityTest]</c> method somewhere in the Editor's loaded assemblies
        /// (review loop 1 finding: previously only string shape was checked, so a rename/deletion of a
        /// cited V1 test would desync the catalog with zero red test). This searches every assembly
        /// loaded in the current AppDomain, not only this EditMode assembly: several AUTO-PLAY rows cite
        /// PlayMode fixtures that live in RoadRage.Tests.PlayMode, a sibling assembly the EditMode
        /// asmdef deliberately does not reference (Code Map: "no asmdef change needed"). Both assemblies
        /// are compiled and loaded in the Editor AppDomain regardless of which test mode is running.
        /// </summary>
        [Test]
        public void EveryCitedTestFullNameResolvesToAnExistingTestMethod()
        {
            var citedNames = OracleCatalog.Rows
                .SelectMany(r => r.BoundTests)
                .Select(b => b.FullName)
                .Concat(ShapeGuardRegister.Entries.Select(e => e.TestFullName))
                .Distinct()
                .ToList();

            Assert.That(citedNames, Is.Not.Empty);

            var missing = new List<string>();
            foreach (var fullName in citedNames)
            {
                if (!ResolvesToATestMethod(fullName))
                {
                    missing.Add(fullName);
                }
            }

            Assert.That(missing, Is.Empty, "cited test(s) not found as an existing [Test]/[UnityTest] method: " + string.Join("; ", missing));
        }

        /// <summary>
        /// Review loop 2 finding: the fixture-completeness test above only ever exercises
        /// <see cref="ResolvesToATestMethod"/> against the real, currently-valid catalog data, so a
        /// regression of that method into a no-op that always returns true would go undetected. This
        /// proves the negative case directly: a fabricated, nonexistent fixture/method name must not
        /// resolve.
        /// </summary>
        [Test]
        public void ResolvesToATestMethodReturnsFalseForAFabricatedNonexistentName()
        {
            Assert.That(ResolvesToATestMethod("NoSuchFixture.NoSuchMethod"), Is.False);
        }

        private static bool ResolvesToATestMethod(string fixtureDotMethod)
        {
            var separator = fixtureDotMethod.LastIndexOf('.');
            if (separator <= 0 || separator == fixtureDotMethod.Length - 1)
            {
                return false;
            }

            var typeName = fixtureDotMethod.Substring(0, separator);
            var methodName = fixtureDotMethod.Substring(separator + 1);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (type.Name != typeName && type.FullName != typeName)
                    {
                        continue;
                    }

                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m => m.Name == methodName);

                    foreach (var method in methods)
                    {
                        foreach (var attribute in method.GetCustomAttributes(true))
                        {
                            var attributeTypeName = attribute.GetType().Name;
                            if (attributeTypeName == "TestAttribute" || attributeTypeName == "UnityTestAttribute")
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        // ------------------------------------------------------------------ replay harness (independent composition)

        private const float ReplaySteerFullLockDegrees = 45f;   // Greybox_AIVehicle
        private const float ReplaySteerDegreesPerSecond = 90f;  // Greybox_AIVehicle
        private const float ReplaySpeed = 8f;                   // DriverProfileDef_Default.desiredSpeed
        private const float ReplayFixedDeltaTime = 0.02f;
        private const float ReplayArrivalRadius = 3f;
        private const float ReplayLookAheadSeconds = 0.6f;
        private const int ReplayMaxSteps = 12000;
        private const int ReplayOrbitStepBudget = 900;

        /// <summary>
        /// Independent composition of the same real <see cref="LaneGraphRouting"/> and
        /// <see cref="NetworkedAIVehicleDriverController"/> pure functions used by
        /// <c>Story510LaneGraphAndRoutedTrafficTests.ReplayRoute</c> (Code Map: "reuse as the
        /// determinism/trace source, do not copy its body") -- this method is its own implementation,
        /// producing a <see cref="TrafficTraceFrame"/> per step instead of a bare position list, so the
        /// oracle bench does not depend on a private method of another test fixture. Returns true only
        /// if the vehicle actually departed and reached an exit portal.
        /// </summary>
        private static bool ReplayForTrace(LaneGraph graph, int entry, ulong seed, TrafficTrace trace)
        {
            var traversed = new bool[graph.NodeCount];
            var node = entry;
            traversed[node] = true;

            var traversedEdges = 0;
            var departed = false;
            var position = graph.GetNodePosition(entry);
            var yaw = graph.GetNodeRotation(entry).eulerAngles.y;
            var target = graph.GetNodePosition(node);
            var lastNodeChangeStep = 0;
            var previousAimPoint = Vector3.zero;

            for (var step = 0; step < ReplayMaxSteps; step++)
            {
                var forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

                if (NetworkedAIVehicleDriverController.HasArrivedAtWaypoint(position, target, ReplayArrivalRadius)
                    || LaneGraphRouting.HasPassedUnreachableWaypoint(position, forward, target, ReplaySpeed, ReplaySteerDegreesPerSecond))
                {
                    if (graph.IsExitPortal(node) && departed)
                    {
                        return true;
                    }

                    var resolved = ResolveNextTraceNode(graph, node, traversed, ref traversedEdges, ref departed, position, seed);
                    if (resolved != node)
                    {
                        lastNodeChangeStep = step;
                    }

                    node = resolved;
                    target = graph.GetNodePosition(node);
                }

                if (step - lastNodeChangeStep > ReplayOrbitStepBudget)
                {
                    return false;
                }

                var nodeForward = graph.GetNodeRotation(node) * Vector3.forward;
                var lookAheadDistance = Mathf.Max(0f, ReplaySpeed) * ReplayLookAheadSeconds;
                var aimPoint = LaneGraphRouting.ResolveLookAheadPoint(position, target, nodeForward, lookAheadDistance, previousAimPoint, 0f);
                previousAimPoint = aimPoint;

                var intent = NetworkedAIVehicleDriverController.ComputeSeekIntent(position, forward, aimPoint, ReplaySteerFullLockDegrees);

                trace.Frames.Add(new TrafficTraceFrame
                {
                    Frame = step,
                    VehicleId = 0,
                    Position = position,
                    YawDegrees = yaw,
                    Speed = ReplaySpeed,
                    RoadElementId = node,
                    RouteProgress = traversedEdges,
                    Goal = "SeekWaypoint",
                    Blockers = Array.Empty<string>(),
                    FinalIntent = intent
                });

                yaw += intent.Steer * ReplaySteerDegreesPerSecond * ReplayFixedDeltaTime;
                position += Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * (ReplaySpeed * ReplayFixedDeltaTime);
            }

            return false;
        }

        private static int ResolveNextTraceNode(
            LaneGraph graph, int current, bool[] traversed, ref int traversedEdges, ref bool departed, Vector3 position, ulong seed)
        {
            var candidates = graph.GetSuccessors(current);
            traversedEdges++;
            departed = true;

            var budgetExceeded = LaneGraphRouting.IsEdgeBudgetExceeded(traversedEdges, graph.NodeCount, graph.TrafficSettings.EdgeBudgetFactor);

            if (candidates.Count > 0 && !budgetExceeded)
            {
                var eligible = new List<bool>();
                for (var i = 0; i < candidates.Count; i++)
                {
                    eligible.Add(!traversed[candidates[i]]);
                }

                var drawn = LaneGraphRouting.SelectWeightedSuccessor(candidates, graph.GetTurnWeights(current), eligible, seed, traversedEdges, out _);
                if (drawn >= 0)
                {
                    traversed[drawn] = true;
                    return drawn;
                }
            }

            var exitIndex = graph.NearestExitNodeIndex(position);
            if (exitIndex < 0)
            {
                return current;
            }

            if (candidates.Count == 0)
            {
                return exitIndex;
            }

            var positions = new List<Vector3>();
            for (var i = 0; i < candidates.Count; i++)
            {
                positions.Add(graph.GetNodePosition(candidates[i]));
            }

            return LaneGraphRouting.SelectSuccessorTowardTarget(candidates, positions, graph.GetNodePosition(exitIndex));
        }

        // ------------------------------------------------------------------ MVP_Run scene access

        private static void WithMvpRun(Action<Scene> body)
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MvpRunScenePath);
            var wasOpen = alreadyOpen.IsValid();
            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);

            try
            {
                body(scene);
            }
            finally
            {
                if (!wasOpen)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static LaneGraph ResolveGraph(Scene scene)
        {
            var runRoot = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == "RunRoot");
            Assert.That(runRoot, Is.Not.Null, "RunRoot attendu dans MVP_Run");

            var graph = runRoot.GetComponentInChildren<LaneGraph>(true);
            Assert.That(graph, Is.Not.Null, "LaneGraph attendu sous RunRoot");
            return graph;
        }
    }
}
