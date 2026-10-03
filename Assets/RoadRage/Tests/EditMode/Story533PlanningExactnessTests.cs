using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.33, D14 : preuves au bit pres des optimisations de la planification et de la portee bornee. Projection elaguee
    /// contre RoadCurve.Project ; horizon optimise contre l'algorithme d'origine (recopie ici) ; verification fusionnee du
    /// profil contre l'algorithme d'origine ; planification bornee contre l'horizon complet (meme commande, memes liantes,
    /// meme acceptation) ; perception sur le chemin complet sans points contre la perception sur l'horizon complet.
    /// </summary>
    [Category("Core")]
    [Category("Story533")]
    public sealed class Story533PlanningExactnessTests
    {
        private const float Dt = 0.02f;
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string CampaignPath = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/campaign-5-31.json";

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };

        private static TrafficV2Admission admission;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                        File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
                Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
                return admission;
            }
        }

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; }
        }

        // ================================================================== routes de reference

        [Serializable]
        private sealed class CampaignTripletRecord
        {
            public int Index;
            public string Entry;
            public string Exit;
            public string Via;
            public ulong Seed;
            public ulong InsertionCounter;
        }

        [Serializable]
        private sealed class CampaignFile
        {
            public string RoadModelVersion;
            public CampaignTripletRecord[] Triplets;
        }

        private sealed class Lane
        {
            public ReferenceTrack Track;
            public RoadId[] Ids;
            public TrafficV2Insertion Insertion;
        }

        private sealed class State
        {
            public Lane Lane;
            public float Distance;
            public int Piece;
            public float Offset;
            public RoutePlan Route;
            public RoadId Via;
            public VehicleFootprintPose Pose;
        }

        private static List<Lane> lanes;

        private static List<Lane> Lanes()
        {
            if (lanes != null) return lanes;
            var model = Admission.Model;
            var campaign = JsonUtility.FromJson<CampaignFile>(File.ReadAllText(CampaignPath));
            Assert.That(campaign.Triplets.Length, Is.EqualTo(11));
            lanes = new List<Lane>();
            foreach (var triplet in campaign.Triplets)
            {
                var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, RoadId.Parse(triplet.Entry), RoadId.Parse(triplet.Exit),
                    triplet.Seed, triplet.InsertionCounter, Driver, Dt, string.IsNullOrEmpty(triplet.Via) ? RoadId.None : RoadId.Parse(triplet.Via));
                Assert.That(insertion.Code, Is.EqualTo(TrafficV2Code.Allowed), "triplet " + triplet.Index);
                lanes.Add(new Lane { Insertion = insertion, Track = ReferenceTrack.FromRoute(model, insertion.Route.Occurrences, 0f),
                    Ids = insertion.Route.Occurrences.Select(o => o.Id).ToArray() });
            }
            return lanes;
        }

        private static TrafficActorInput ActorAt(RoadId id, Lane lane, float distance, float speed)
        {
            int piece = lane.Track.PieceAt(distance);
            var nominal = lane.Track.Nominal(piece, distance);
            return new TrafficActorInput(id, new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward,
                Up = nominal.Up, Footprint = Car }, speed, lane.Track.Pieces[piece].Id, lane.Ids, lane.Track.KinematicAnchors(piece));
        }

        /// <summary>Poses nominales le long des 11 routes, progression de route tenue comme le driver.</summary>
        private static List<State> States(float spacing)
        {
            var model = Admission.Model;
            var states = new List<State>();
            foreach (var lane in Lanes())
            {
                var route = lane.Insertion.Route;
                var via = lane.Insertion.ViaMovementId;
                var distances = new SortedSet<float>();
                for (float d = 0f; d < lane.Track.LengthMeters - 0.25f; d += spacing) distances.Add(d);
                foreach (var piece in lane.Track.Pieces) distances.Add(0.5f * (piece.StartDistanceMeters + piece.EndDistanceMeters));
                foreach (float distance in distances)
                {
                    int piece = lane.Track.PieceAt(distance);
                    var input = ActorAt(lane.Insertion.TrafficId, lane, distance, 4f);
                    var frame = new TrafficFrame(1, model, new[] { input });
                    if (!via.IsEmpty && route.ViaOccurrenceIndex >= 0 && route.ProgressOccurrenceIndex > route.ViaOccurrenceIndex)
                        via = RoadId.None;
                    float offset = lane.Track.OffsetRadians(piece, distance);
                    var decision = PlanningSpine.Evaluate(Request(frame, lane, route, via, offset, null));
                    if (decision.Route.Plan == null) continue;
                    route = decision.Route.Plan;
                    states.Add(new State { Lane = lane, Distance = distance, Piece = piece, Offset = offset, Route = route, Via = via,
                        Pose = input.Pose });
                }
            }
            return states;
        }

        private static PlanningRequest Request(TrafficFrame frame, Lane lane, RoutePlan route, RoadId via, float offset, PlanningReach? reach)
        {
            return new PlanningRequest(frame, lane.Insertion.TrafficId, route, route.ExitPortalId, lane.Insertion.Seed,
                TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null, null, Admission.Evidence,
                via, offset, reach);
        }

        private static IReadOnlyList<string> RingSeams
        {
            get
            {
                var evidence = Admission.Evidence;
                return evidence.Valid && evidence.PoseModel == NominalPoseModel.Kinematic ? evidence.SignedRingSeams : null;
            }
        }

        // ================================================================== projection elaguee

        [Test]
        public void ThePrunedProjectionEqualsTheFullProjectionToTheBitOnEveryElement()
        {
            var model = Admission.Model;
            var random = new System.Random(5332014);
            int compared = 0, mismatches = 0;
            var curves = model.Corridors.Select(c => c.Curve).Concat(model.Movements.Select(m => m.Curve)).ToList();
            foreach (var curve in curves)
            {
                for (int k = 0; k < 24; k++)
                {
                    float s = (float)(random.NextDouble() * curve.Length);
                    var frame = curve.Sample(s);
                    // Pres de la courbe (empreintes), loin, au-dela des bouts, en hauteur.
                    float lateral = (float)(random.NextDouble() * 8.0 - 4.0) * (k % 6 == 5 ? 10f : 1f);
                    float along = k % 7 == 6 ? (float)(random.NextDouble() * 20.0 - 10.0) : 0f;
                    var point = frame.Position + frame.Right * lateral + frame.Tangent * along + frame.Up * (float)(random.NextDouble() * 1.5);
                    foreach (float hint in new[] { s, 0f, curve.Length, (float)(random.NextDouble() * curve.Length) })
                    {
                        var full = curve.Project(point);
                        var pruned = curve.ProjectNearest(point, hint);
                        compared++;
                        if (!SameProjection(full, pruned)) mismatches++;
                    }
                }
            }
            Debug.Log("[Story533] projection elaguee : " + compared + " projections comparees, " + mismatches + " differences.");
            Assert.That(compared, Is.GreaterThan(1000));
            Assert.That(mismatches, Is.Zero);
        }

        private static bool SameProjection(RoadProjection a, RoadProjection b)
        {
            return a.SMeters == b.SMeters && a.DistanceMeters == b.DistanceMeters && a.LateralOffsetMeters == b.LateralOffsetMeters
                && a.NormalOffsetMeters == b.NormalOffsetMeters && a.LongitudinalOverrunMeters == b.LongitudinalOverrunMeters
                && a.Point.Position.Equals(b.Point.Position) && a.Point.Tangent.Equals(b.Point.Tangent) && a.Point.SMeters == b.Point.SMeters;
        }

        // ================================================================== horizon optimise contre l'algorithme d'origine

        private struct ReferencePoint
        {
            public float Distance, ElementS, Ceiling, E;
            public RoadCurvePoint Reference;
        }

        /// <summary>PathHorizon.Build d'avant D14, recopie a l'identique sur l'API publique (points seulement).</summary>
        private static List<List<ReferencePoint>> OriginalHorizon(RoutePlan route, float lookAhead, float offset)
        {
            var model = Admission.Model;
            var profile = model.DrivabilityProfile;
            bool kinematic = profile.Declared && profile.ReferencePointAheadRearAxleMeters > 0f;
            double e = kinematic ? offset : 0d;
            var intervals = new List<List<ReferencePoint>>();
            float travelled = 0f;
            for (int i = route.ProgressOccurrenceIndex; i < route.Occurrences.Count && travelled < lookAhead; i++)
            {
                var occurrence = route.Occurrences[i];
                RoadElementKind kind;
                RoadCurve curve;
                IReadOnlyList<RoadCurveSample> samples;
                if (!TrafficFrame.TryGetElement(model, occurrence.Id, out kind, out curve, out samples)) break;
                float s0 = i == route.ProgressOccurrenceIndex ? route.ProgressSMeters : occurrence.StartSMeters;
                float s1 = Math.Min(occurrence.EndSMeters, s0 + lookAhead - travelled);
                var points = new List<ReferencePoint>();
                Func<float, RoadCurvePoint, ReferencePoint> make = (distance, reference) => new ReferencePoint { Distance = distance,
                    ElementS = reference.SMeters, Reference = reference,
                    Ceiling = RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profile, reference.CurvaturePerMeter), E = float.NaN };
                points.Add(make(travelled, curve.Sample(s0)));
                for (int j = 0; j < samples.Count; j++)
                    if (samples[j].SMeters > s0 && samples[j].SMeters < s1)
                        points.Add(make(travelled + samples[j].SMeters - s0, curve.Sample(samples[j].SMeters)));
                if (s1 > s0) points.Add(make(travelled + s1 - s0, curve.Sample(s1)));
                if (kinematic)
                {
                    if (intervals.Count > 0)
                    {
                        var previous = intervals[intervals.Count - 1];
                        e += RoadCurve.SignedTangentJumpRadians(previous[previous.Count - 1].Reference, points[0].Reference);
                    }
                    for (int j = 0; j < points.Count; j++)
                    {
                        if (j > 0) e = curve.AdvanceKinematicOffset(points[j - 1].ElementS, points[j].ElementS, e, profile.ReferencePointAheadRearAxleMeters);
                        var p = points[j];
                        p.Ceiling = PathHorizon.NominalSteeringCeilingMetersPerSecond(profile, (float)e);
                        p.E = (float)e;
                        points[j] = p;
                    }
                }
                intervals.Add(points);
                travelled += s1 - s0;
            }
            return intervals;
        }

        [Test]
        public void TheOptimizedHorizonEqualsTheOriginalAlgorithmToTheBit()
        {
            var model = Admission.Model;
            int points = 0, mismatches = 0;
            var states = States(6f);
            Assert.That(states.Count, Is.GreaterThan(300));
            foreach (var state in states)
            {
                var horizon = PathHorizon.Build(model, state.Route, TrafficV2Settings.LookAheadMeters, state.Offset, RingSeams);
                var original = OriginalHorizon(state.Route, TrafficV2Settings.LookAheadMeters, state.Offset);
                if (horizon.Intervals.Count != original.Count) { mismatches++; continue; }
                for (int i = 0; i < original.Count; i++)
                {
                    var r = horizon.Intervals[i].Points;
                    if (r.Count != original[i].Count) { mismatches++; continue; }
                    for (int j = 0; j < r.Count; j++)
                    {
                        var p = r[j]; var q = original[i][j];
                        points++;
                        if (p.DistanceMeters != q.Distance || p.ElementSMeters != q.ElementS || p.SteeringCeilingMetersPerSecond != q.Ceiling
                            || !(p.NominalOffsetRadians == q.E || (float.IsNaN(p.NominalOffsetRadians) && float.IsNaN(q.E)))
                            || !p.Reference.Position.Equals(q.Reference.Position) || !p.Reference.Tangent.Equals(q.Reference.Tangent)
                            || !p.Reference.Up.Equals(q.Reference.Up) || !p.Reference.Right.Equals(q.Reference.Right)
                            || p.Reference.CurvaturePerMeter != q.Reference.CurvaturePerMeter
                            || p.Reference.HalfWidthLeftMeters != q.Reference.HalfWidthLeftMeters
                            || p.Reference.HalfWidthRightMeters != q.Reference.HalfWidthRightMeters)
                            mismatches++;
                    }
                }
            }
            Debug.Log("[Story533] horizon optimise : " + states.Count + " horizons, " + points + " points compares, " + mismatches + " differences.");
            Assert.That(mismatches, Is.Zero);
        }

        // ================================================================== verification fusionnee contre l'algorithme d'origine

        /// <summary>MotionPlan.VerifySpeedProfile d'avant D14 (boucles de noeuds), recopie a l'identique.</summary>
        private static SpeedProfileResult OriginalVerification(MotionPlan motion, IReadOnlyList<SpeedProfilePoint> candidate,
            LongitudinalBounds bounds, out bool decided)
        {
            // Les controles prealables sont identiques : seule la boucle des noeuds est recopiee, sur un plan sans issue.
            decided = motion.Issue == MotionIssue.None && candidate.Count >= 2 && bounds.Valid;
            var verdict = motion.VerifySpeedProfile(candidate, bounds);
            if (!decided || verdict.Issue == SpeedProfileIssue.InvalidSpeedProfile || verdict.Issue == SpeedProfileIssue.AccelerationBoundExceeded)
            { decided = false; return verdict; }
            var drivability = Admission.Model.DrivabilityProfile;
            MotionDiagnostic diagnostics = motion.Diagnostics;
            foreach (var interval in motion.Path.Intervals)
            {
                var knots = new List<float>();
                float from = Math.Max(interval.StartDistanceMeters, candidate[0].DistanceMeters);
                float to = Math.Min(interval.EndDistanceMeters, candidate[candidate.Count - 1].DistanceMeters);
                if (to < from) continue;
                knots.Add(from);
                foreach (var point in interval.Points)
                    if (point.DistanceMeters > from && point.DistanceMeters < to) knots.Add(point.DistanceMeters);
                foreach (var point in candidate)
                    if (point.DistanceMeters > from && point.DistanceMeters < to) knots.Add(point.DistanceMeters);
                if (to > from) knots.Add(to);
                knots.Sort();
                for (int i = 0; i < knots.Count; i++)
                {
                    float s = knots[i];
                    float speed = SpeedAt(candidate, s);
                    float ceiling = CeilingAt(interval, s);
                    if (speed > ceiling) return Result(SpeedProfileIssue.SteeringCeilingExceeded, s, diagnostics);
                    if (speed > 0f && speed < drivability.SteeringInactiveBelowMetersPerSecond && Math.Abs(CurvatureAt(interval, s)) > 0f)
                        diagnostics |= MotionDiagnostic.SteeringInactiveSpan;
                    if (i > 0)
                    {
                        float previous = knots[i - 1];
                        float previousSpeed = SpeedAt(candidate, previous);
                        if (Math.Max(previousSpeed, speed) > 0f && Math.Min(previousSpeed, speed) < drivability.SteeringInactiveBelowMetersPerSecond
                            && (Math.Abs(CurvatureAt(interval, previous)) > 0f || Math.Abs(CurvatureAt(interval, s)) > 0f))
                            diagnostics |= MotionDiagnostic.SteeringInactiveSpan;
                        float minimumCeiling = Math.Min(CeilingAt(interval, previous), ceiling);
                        if (Math.Max(previousSpeed, speed) > minimumCeiling) return Result(SpeedProfileIssue.SteeringCeilingExceeded, previous, diagnostics);
                    }
                }
            }
            return Result(SpeedProfileIssue.None, 0f, diagnostics);
        }

        private static readonly ConstructorInfo ResultConstructor = typeof(SpeedProfileResult).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Single();

        private static SpeedProfileResult Result(SpeedProfileIssue issue, float distance, MotionDiagnostic diagnostics)
        {
            return (SpeedProfileResult)ResultConstructor.Invoke(new object[] { issue, distance, diagnostics, MotionIssue.None });
        }

        private static int FirstAtOrAfter(IReadOnlyList<SpeedProfilePoint> points, float s)
        {
            int low = 1, high = points.Count - 1, found = -1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (s <= points[mid].DistanceMeters) { found = mid; high = mid - 1; } else low = mid + 1;
            }
            return found;
        }

        private static int FirstAtOrAfter(IReadOnlyList<PathPoint> points, float s)
        {
            int low = 1, high = points.Count - 1, found = -1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (s <= points[mid].DistanceMeters) { found = mid; high = mid - 1; } else low = mid + 1;
            }
            return found;
        }

        private static float SpeedAt(IReadOnlyList<SpeedProfilePoint> points, float s)
        {
            int i = FirstAtOrAfter(points, s);
            if (i > 0)
            {
                var a = points[i - 1]; var b = points[i];
                double t = (s - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                double v2 = a.SpeedMetersPerSecond * (double)a.SpeedMetersPerSecond * (1d - t) + b.SpeedMetersPerSecond * (double)b.SpeedMetersPerSecond * t;
                return (float)Math.Sqrt(Math.Max(0d, v2));
            }
            return points[points.Count - 1].SpeedMetersPerSecond;
        }

        private static float CeilingAt(PathInterval interval, float s)
        {
            int i = FirstAtOrAfter(interval.Points, s);
            if (i > 0) return Math.Min(interval.Points[i - 1].SteeringCeilingMetersPerSecond, interval.Points[i].SteeringCeilingMetersPerSecond);
            return interval.Points[interval.Points.Count - 1].SteeringCeilingMetersPerSecond;
        }

        private static float CurvatureAt(PathInterval interval, float s)
        {
            int i = FirstAtOrAfter(interval.Points, s);
            if (i > 0)
            {
                var a = interval.Points[i - 1]; var b = interval.Points[i];
                float t = (s - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                return a.Reference.CurvaturePerMeter + t * (b.Reference.CurvaturePerMeter - a.Reference.CurvaturePerMeter);
            }
            return interval.Points[interval.Points.Count - 1].Reference.CurvaturePerMeter;
        }

        [Test]
        public void TheMergedProfileVerificationEqualsTheOriginalLoopToTheBit()
        {
            var model = Admission.Model;
            var driver = Driver;
            var bounds = new LongitudinalBounds(driver.MaxAcceleration, driver.SafeBrakingLimit);
            int compared = 0, failures = 0, mismatches = 0;
            foreach (var state in States(9f))
            {
                var horizon = PathHorizon.Build(model, state.Route, TrafficV2Settings.LookAheadMeters, state.Offset, RingSeams);
                var motion = new MotionPlan(horizon, model.DrivabilityProfile, Admission.Evidence, TrackingTolerance.Undeclared);
                if (motion.Issue != MotionIssue.None) continue;
                foreach (float speed in new[] { 0f, 4f, 8f })
                {
                    var profile = SpeedPlan.Build(motion, model, driver, speed).ToProfile();
                    // Le profil du plan, puis des profils deformes qui franchissent le plafond (premiere defaillance comparee).
                    foreach (float scale in new[] { 1f, 1.25f, 2f })
                    {
                        var candidate = profile.Select(p => new SpeedProfilePoint(p.DistanceMeters, p.SpeedMetersPerSecond * scale)).ToArray();
                        bool decided;
                        var original = OriginalVerification(motion, candidate, new LongitudinalBounds(bounds.MaxAcceleration * scale * scale,
                            bounds.MaxDeceleration * scale * scale), out decided);
                        if (!decided) continue;
                        var merged = motion.VerifySpeedProfile(candidate, new LongitudinalBounds(bounds.MaxAcceleration * scale * scale,
                            bounds.MaxDeceleration * scale * scale));
                        compared++;
                        if (original.Issue != SpeedProfileIssue.None) failures++;
                        if (merged.Issue != original.Issue || merged.DistanceMeters != original.DistanceMeters || merged.Diagnostics != original.Diagnostics)
                            mismatches++;
                    }
                }
            }
            Debug.Log("[Story533] verification fusionnee : " + compared + " profils compares (" + failures + " defaillances), " + mismatches + " differences.");
            Assert.That(compared, Is.GreaterThan(500));
            Assert.That(failures, Is.GreaterThan(0), "des profils deformes exercent les defaillances");
            Assert.That(mismatches, Is.Zero);
        }

        // ================================================================== planification bornee

        [Test]
        public void TheBoundedPlanningGivesTheSameCommandBindingAndAcceptanceAsTheFullHorizon()
        {
            var model = Admission.Model;
            var driver = Driver;
            int commands = 0, mismatches = 0, acceptance = 0;
            float longest = 0f, shortest = float.PositiveInfinity;
            var differences = new List<string>();
            foreach (var state in States(6f))
            {
                foreach (float speed in new[] { 0f, 2f, 4f, 6f, driver.DesiredSpeed, driver.DesiredSpeed * 1.1f })
                {
                    var input = new TrafficActorInput(state.Lane.Insertion.TrafficId, state.Pose, speed,
                        state.Lane.Track.Pieces[state.Piece].Id, state.Lane.Ids, state.Lane.Track.KinematicAnchors(state.Piece));
                    var frame = new TrafficFrame(1, model, new[] { input });
                    float preview = Math.Max(Math.Max(0f, speed) * Dt, MotionCommand.PreviewFloorMeters);
                    var reach = PlanningReach.For(driver, speed, preview, TrafficV2Settings.PlanningReachMarginMeters);
                    var full = PlanningSpine.Evaluate(Request(frame, state.Lane, state.Route, state.Via, state.Offset, null));
                    var bounded = PlanningSpine.Evaluate(Request(frame, state.Lane, state.Route, state.Via, state.Offset, reach));
                    if (full.Motion == null || bounded.Motion == null) { if ((full.Motion == null) != (bounded.Motion == null)) acceptance++; continue; }
                    Assert.That(bounded.PerceptionPath.LengthMeters, Is.EqualTo(full.Path.LengthMeters), "la perception garde toute la route");
                    longest = Math.Max(longest, bounded.Path.LengthMeters);
                    shortest = Math.Min(shortest, bounded.Path.LengthMeters);
                    var fullPlan = SpeedPlan.Build(full.Motion, model, driver, speed);
                    var boundedPlan = SpeedPlan.Build(bounded.Motion, model, driver, speed);
                    if (fullPlan.Accepted != boundedPlan.Accepted) { acceptance++; continue; }
                    if (!fullPlan.Accepted) continue;
                    var interval = bounded.Path.Intervals[0];
                    RoadElementKind kind;
                    RoadCurve curve;
                    IReadOnlyList<RoadCurveSample> samples;
                    Assert.That(TrafficFrame.TryGetElement(model, interval.Id, out kind, out curve, out samples), Is.True);
                    float heading = state.Lane.Track.NominalHeadingErrorDegrees(state.Piece, state.Distance);
                    var a = MotionCommand.Track(9, 1, fullPlan, driver, model.DrivabilityProfile, curve, interval.StartSMeters,
                        state.Pose.Position, state.Pose.Forward, speed, Dt, heading);
                    var b = MotionCommand.Track(9, 1, boundedPlan, driver, model.DrivabilityProfile, curve, interval.StartSMeters,
                        state.Pose.Position, state.Pose.Forward, speed, Dt, heading);
                    commands++;
                    bool same = a.TargetAccelerationMetersPerSecondSquared == b.TargetAccelerationMetersPerSecondSquared
                        && a.TargetWheelAngleDegrees == b.TargetWheelAngleDegrees
                        && fullPlan.SpeedAt(preview) == boundedPlan.SpeedAt(preview) && fullPlan.Binding == boundedPlan.Binding
                        && fullPlan.LimitingConstraint == boundedPlan.LimitingConstraint
                        && fullPlan.PlanningDecelerationMetersPerSecondSquared == boundedPlan.PlanningDecelerationMetersPerSecondSquared;
                    if (!same)
                    {
                        mismatches++;
                        if (differences.Count < 10)
                            differences.Add(state.Lane.Insertion.TrafficId + " d " + state.Distance + " v " + speed + " : " + fullPlan.Binding + "/"
                                + boundedPlan.Binding + " a " + a.TargetAccelerationMetersPerSecondSquared + "/" + b.TargetAccelerationMetersPerSecondSquared);
                    }
                }
            }
            Debug.Log("[Story533] planification bornee : " + commands + " commandes comparees, " + mismatches + " differences, "
                + acceptance + " acceptations differentes ; H de " + shortest + " a " + longest + " m. " + string.Join(" | ", differences.ToArray()));
            Assert.That(commands, Is.GreaterThan(2000));
            Assert.That(acceptance, Is.Zero, "meme acceptation du plan, dans les deux sens");
            Assert.That(mismatches, Is.Zero, string.Join(" | ", differences.ToArray()));
        }

        // ================================================================== perception sur le chemin complet

        [Test]
        public void ThePerceptionOnTheRoutePathEqualsThePerceptionOnTheFullHorizonWithNearAndFarActors()
        {
            var model = Admission.Model;
            var driver = Driver;
            var buffer = new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity);
            int observations = 0, mismatches = 0, leaders = 0, obstacles = 0;
            var differences = new List<string>();
            ulong counter = 100;
            foreach (var lane in Lanes())
            {
                var route = lane.Insertion.Route;
                for (float distance = 1f; distance < lane.Track.LengthMeters - 10f; distance += 20f)
                {
                    int piece = lane.Track.PieceAt(distance);
                    var actors = new List<TrafficActorInput> { ActorAt(lane.Insertion.TrafficId, lane, distance, 6f) };
                    // Leaders proches et lointains sur la meme route ; dangers dans le couloir et a cote.
                    foreach (float gap in new[] { 7f, 30f, 75f, 160f })
                        if (distance + gap < lane.Track.LengthMeters - 3f)
                            actors.Add(ActorAt(new RoadId(0x5331UL, counter++), lane, distance + gap, 3f));
                    var hazards = new List<TrafficHazardInput>();
                    foreach (var at in new[] { new Vector2(14f, 0f), new Vector2(50f, 0.5f), new Vector2(120f, 0f), new Vector2(40f, 3f) })
                        if (distance + at.x < lane.Track.LengthMeters - 2f)
                        {
                            var nominal = lane.Track.Nominal(lane.Track.PieceAt(distance + at.x), distance + at.x);
                            var right = Vector3.Cross(nominal.Up, nominal.Forward).normalized;
                            hazards.Add(new TrafficHazardInput(new RoadId(0x48415A4152440000UL, counter++), TrafficHazardKind.Obstacle,
                                new RoadBoundsBox { Center = nominal.Position + right * at.y + nominal.Up * 0.75f, Extents = new Vector3(0.5f, 0.75f, 0.5f) },
                                nominal.Forward * 1.5f, 1f));
                        }
                    var frame = new TrafficFrame(1, model, actors, hazards);
                    float offset = lane.Track.OffsetRadians(piece, distance);
                    var reach = PlanningReach.For(driver, 6f, 6f * Dt, TrafficV2Settings.PlanningReachMarginMeters);
                    var full = PlanningSpine.Evaluate(Request(frame, lane, route, RoadId.None, offset, null));
                    var bounded = PlanningSpine.Evaluate(Request(frame, lane, route, RoadId.None, offset, reach));
                    if (full.Route.Plan == null || full.Path == null || bounded.PerceptionPath == null) continue;
                    route = full.Route.Plan;
                    var a = TrafficPerception.Observe(frame, lane.Insertion.TrafficId, full.Path, TrafficV2Settings.PerceptionLimits, buffer);
                    var b = TrafficPerception.Observe(frame, lane.Insertion.TrafficId, bounded.PerceptionPath, TrafficV2Settings.PerceptionLimits, buffer);
                    var la = LongitudinalPerception.From(a, full.Path, LongitudinalPerception.FrontDistanceMeters(frame, lane.Insertion.TrafficId, full.Path), false);
                    var lb = LongitudinalPerception.From(b, bounded.PerceptionPath,
                        LongitudinalPerception.FrontDistanceMeters(frame, lane.Insertion.TrafficId, bounded.PerceptionPath), false);
                    observations++;
                    if (a.Perceived && a.Leader.Items.Count > 0) leaders++;
                    if (a.Perceived) obstacles += a.Obstacles.Items.Count;
                    string difference = Difference("observation", a, b, 0) ?? Difference("longitudinale", la, lb, 0);
                    if (difference != null)
                    {
                        mismatches++;
                        if (differences.Count < 10) differences.Add(lane.Insertion.TrafficId + " d " + distance + " : " + difference);
                    }
                }
            }
            Debug.Log("[Story533] perception sur le chemin complet : " + observations + " observations, " + leaders + " avec leader, "
                + obstacles + " faits obstacles, " + mismatches + " differences. " + string.Join(" | ", differences.ToArray()));
            Assert.That(observations, Is.GreaterThan(50));
            Assert.That(leaders, Is.GreaterThan(20));
            Assert.That(obstacles, Is.GreaterThan(20));
            Assert.That(mismatches, Is.Zero, string.Join(" | ", differences.ToArray()));
        }

        /// <summary>Premiere difference entre deux valeurs, champs et proprietes publics compris, flottants au bit pres ; nul si egales.</summary>
        private static string Difference(string path, object a, object b, int depth)
        {
            if (ReferenceEquals(a, b)) return null;
            if (a == null || b == null) return path + " : nul d'un seul cote";
            var type = a.GetType();
            if (type != b.GetType()) return path + " : types differents";
            if (a is float) return BitConverter.ToInt32(BitConverter.GetBytes((float)a), 0) == BitConverter.ToInt32(BitConverter.GetBytes((float)b), 0)
                ? null : path + " : " + a + " / " + b;
            if (a is double) return ((double)a).Equals((double)b) ? null : path + " : " + a + " / " + b;
            if (type.IsPrimitive || type.IsEnum || a is string || a is RoadId || a is decimal) return a.Equals(b) ? null : path + " : " + a + " / " + b;
            if (a is Vector3) return ((Vector3)a).Equals((Vector3)b) ? null : path + " : " + a + " / " + b;
            if (depth > 6) return null;
            var enumerableA = a as IEnumerable;
            if (enumerableA != null)
            {
                var listA = enumerableA.Cast<object>().ToList();
                var listB = ((IEnumerable)b).Cast<object>().ToList();
                if (listA.Count != listB.Count) return path + " : " + listA.Count + " / " + listB.Count + " elements";
                for (int i = 0; i < listA.Count; i++)
                {
                    var d = Difference(path + "[" + i + "]", listA[i], listB[i], depth + 1);
                    if (d != null) return d;
                }
                return null;
            }
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var d = Difference(path + "." + field.Name, field.GetValue(a), field.GetValue(b), depth + 1);
                if (d != null) return d;
            }
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetIndexParameters().Length > 0) continue;
                object va, vb;
                string ea = null, eb = null;
                try { va = property.GetValue(a, null); } catch (TargetInvocationException e) { va = null; ea = e.InnerException.GetType().Name; }
                try { vb = property.GetValue(b, null); } catch (TargetInvocationException e) { vb = null; eb = e.InnerException.GetType().Name; }
                if (ea != null || eb != null)
                {
                    if (ea != eb) return path + "." + property.Name + " : exception " + ea + " / " + eb;
                    continue;
                }
                var d = Difference(path + "." + property.Name, va, vb, depth + 1);
                if (d != null) return d;
            }
            return null;
        }
    }
}
