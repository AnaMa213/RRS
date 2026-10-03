using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.31 : plan de vitesse a contraintes nommees, commande de suivi et composeur unique avec la
    /// commande de repli V2. Logique pure sur le modele signe et le profil vehicule authore.
    /// </summary>
    [Category("Core")]
    [Category("Story531")]
    [Category("Story533")]
    public sealed class Story531SpeedPlanAndComposerTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string VehicleProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string HistoricalSignedDirectory = "_bmad-output/implementation-artifacts/gate-a-5-52/historical-signed-5-51/";
        private const float Dt = 0.02f;

        private static TrafficV2Admission admission;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(HistoricalSignedDirectory + "MVP_Run.road-model.json"),
                        File.ReadAllText(HistoricalSignedDirectory + "MVP_Run.road-signoff.json"),
                        File.ReadAllText(HistoricalSignedDirectory + "migration-report-5-28-mvp-run.md"));
                return admission;
            }
        }

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; }
        }

        private static VehicleProfile Vehicle
        {
            get { return AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath).Profile; }
        }

        private static List<Portal> Entries(CompiledRoadModel model)
        {
            return model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).ToList();
        }

        private static PlanningDecision Decide(RoutePlan route, RoadId trafficId, Vector3 position, Vector3 forward,
            float speed, RoadId previous, ulong frameId = 1)
        {
            var model = Admission.Model;
            var pose = new VehicleFootprintPose { Position = position, Forward = forward, Up = Vector3.up };
            var ids = route == null ? null : route.Occurrences.Select(o => o.Id).ToArray();
            var frame = new TrafficFrame(frameId, model, new[] { new TrafficActorInput(trafficId, pose, speed, previous, ids) });
            return PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, route,
                route == null ? RoadId.None : route.ExitPortalId, new RouteSeed(0), TrafficV2Settings.LookAheadMeters,
                null, null, null, Driver, TrackingTolerance.Undeclared, null, null, Admission.Evidence));
        }

        /// <summary>Pose sur la reference de la route a une distance de route donnee.</summary>
        private static RoadCurvePoint PointAt(RoutePlan route, float distance, out RoadId element)
        {
            var track = ReferenceTrack.FromRoute(Admission.Model, route.Occurrences);
            int piece = track.PieceAt(distance);
            element = track.Pieces[piece].Id;
            return track.Pieces[piece].Curve.Sample(track.Pieces[piece].ElementS(distance));
        }

        private static TrafficV2Insertion Insert(int entryIndex)
        {
            var entry = Entries(Admission.Model)[entryIndex];
            var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, RoadId.None, 0UL, 1UL, Driver, Dt);
            Assert.That(insertion.Code, Is.EqualTo(TrafficV2Code.Allowed));
            return insertion;
        }

        // ------------------------------------------------------------------ plan de vitesse

        [Test]
        public void EveryEntryPlanAppliesNamedConstraintsAndPassesTheVerifier()
        {
            var driver = Driver;
            var bindings = new HashSet<SpeedConstraint>();
            for (int e = 0; e < 4; e++)
            {
                var insertion = Insert(e);
                var plan = insertion.FirstSpeedPlan;
                Assert.That(plan.Accepted, Is.True, "entree " + e);
                Assert.That(plan.Verification.Issue, Is.EqualTo(SpeedProfileIssue.None));
                Assert.That(plan.Points[0].SpeedMetersPerSecond, Is.EqualTo(0f), "le plan part de l'etat courant (vitesse nulle)");
                for (int i = 0; i < plan.Points.Count; i++)
                {
                    var point = plan.Points[i];
                    Assert.That(point.Binding, Is.Not.EqualTo(SpeedConstraint.None), "contrainte liante nommee en chaque point");
                    Assert.That(point.SpeedMetersPerSecond, Is.LessThanOrEqualTo(driver.DesiredSpeed));
                    if (!point.CeilingUnbounded)
                        Assert.That(point.SpeedMetersPerSecond, Is.LessThanOrEqualTo(point.CeilingMetersPerSecond));
                    else
                        Assert.That(point.CeilingMetersPerSecond, Is.EqualTo(0f), "Unbounded n'est jamais lu comme une vitesse");
                    Assert.That(float.IsInfinity(point.SpeedMetersPerSecond) || float.IsNaN(point.SpeedMetersPerSecond), Is.False);
                }
                // Alternatives rejetees conservees en chaque point : desiree, plafond, passe arriere, passe avant, etat courant.
                foreach (var point in plan.Points)
                {
                    bindings.Add(point.Binding);
                    Assert.That(point.SpeedMetersPerSecond, Is.LessThanOrEqualTo(Math.Max(point.BackwardMetersPerSecond,
                        point.CurrentStateMetersPerSecond) + 1e-5f));
                    Assert.That(point.DesiredMetersPerSecond, Is.EqualTo(driver.DesiredSpeed));
                }
                // L'horizon couvre toute la route restante : fin au portail, pas de vitesse terminale imposee.
                Assert.That(plan.Points[plan.Points.Count - 1].Binding, Is.Not.EqualTo(SpeedConstraint.HorizonTerminalStop));
            }
            // Plafond de la pose nominale (2026-09-30) : sur ce reseau, la limite de courbe lie avant lui.
            Assert.That(bindings, Is.SupersetOf(new[] { SpeedConstraint.MaxAcceleration, SpeedConstraint.CurveLimit,
                SpeedConstraint.AnticipatedDeceleration }));
            // Vitesse desiree liante sur un plan lance en ligne droite.
            TrafficV2Insertion cruise = null;
            TrackPiece straight = null;
            for (int e = 0; e < 4; e++)
            {
                var candidate = Insert(e);
                foreach (var piece in ReferenceTrack.FromRoute(Admission.Model, candidate.Route.Occurrences).Pieces)
                    if (piece.Kind == RoadElementKind.LaneCorridor && (straight == null
                        || piece.EndDistanceMeters - piece.StartDistanceMeters > straight.EndDistanceMeters - straight.StartDistanceMeters))
                    { straight = piece; cruise = candidate; }
            }
            RoadId element;
            var at = PointAt(cruise.Route, straight.StartDistanceMeters + 1f, out element);
            var decision = Decide(cruise.Route, cruise.TrafficId, at.Position, at.Tangent, driver.DesiredSpeed, element);
            var cruising = SpeedPlan.Build(decision.Motion, Admission.Model, driver, driver.DesiredSpeed);
            Assert.That(cruising.Points.Any(p => p.Binding == SpeedConstraint.DesiredSpeed), Is.True);
        }

        [Test]
        public void EveryProfileAlongARouteIsVerifiedWithTheSameBounds()
        {
            var driver = Driver;
            var insertion = Insert(0);
            var route = insertion.Route;
            var track = ReferenceTrack.FromRoute(Admission.Model, route.Occurrences);
            int checkedPlans = 0;
            for (float distance = 0.5f; distance < track.LengthMeters - 0.5f; distance += 3.7f)
            {
                RoadId element;
                var point = PointAt(route, distance, out element);
                foreach (float speed in new[] { 0f, 1f, 4f })
                {
                    var decision = Decide(route, insertion.TrafficId, point.Position, point.Tangent, speed, element);
                    Assert.That(decision.Motion, Is.Not.Null);
                    var plan = SpeedPlan.Build(decision.Motion, Admission.Model, driver, speed);
                    // Tout profil produit passe par le verificateur 5.30 avec (MaxAcceleration, SafeBrakingLimit).
                    var again = decision.Motion.VerifySpeedProfile(plan.ToProfile(),
                        new LongitudinalBounds(driver.MaxAcceleration, driver.SafeBrakingLimit));
                    Assert.That(again.Issue, Is.EqualTo(plan.Verification.Issue));
                    if (plan.Binding != SpeedConstraint.SteeringCeilingUnreachable)
                        Assert.That(plan.Accepted, Is.True, "d = " + distance + ", v = " + speed + " : " + plan.Verification.Issue
                            + " @ " + plan.Verification.DistanceMeters);
                    checkedPlans++;
                }
            }
            Assert.That(checkedPlans, Is.GreaterThan(30));
        }

        [Test]
        public void RingSeamsTakeTheMinimumCeilingOfBothSides()
        {
            int ringSeams = 0;
            for (int e = 0; e < 4; e++)
            {
                var insertion = Insert(e);
                var decision = Decide(insertion.Route, insertion.TrafficId, insertion.Position,
                    insertion.Rotation * Vector3.forward, 0f, insertion.EntryPortal.CorridorId);
                var plan = SpeedPlan.Build(decision.Motion, Admission.Model, Driver, 0f);
                foreach (var seam in decision.Path.Seams.Where(s => s.RoundaboutDiscontinuity))
                {
                    ringSeams++;
                    Assert.That(plan.SpeedAt(seam.DistanceMeters), Is.LessThanOrEqualTo(seam.MinimumCeilingMetersPerSecond));
                    var nearest = plan.Points.OrderBy(p => Math.Abs(p.DistanceMeters - seam.DistanceMeters)).First();
                    if (!float.IsPositiveInfinity(seam.MinimumCeilingMetersPerSecond))
                    {
                        Assert.That(nearest.CeilingUnbounded, Is.False);
                        Assert.That(nearest.CeilingMetersPerSecond, Is.LessThanOrEqualTo(seam.MinimumCeilingMetersPerSecond));
                    }
                }
            }
            Assert.That(ringSeams, Is.GreaterThan(0), "les routes des quatre entrees traversent des raccords d'anneau");
        }

        [Test]
        public void SteeringCeilingUnreachableBrakesAtSafeBrakingLimit()
        {
            var driver = Driver;
            var insertion = Insert(0);
            // Horizon sans pose nominale (plafond de regime etabli du compilateur) : le mecanisme est le meme.
            var plan0 = SpeedPlan.Build(Decide(insertion.Route, insertion.TrafficId, insertion.Position,
                insertion.Rotation * Vector3.forward, 0f, insertion.EntryPortal.CorridorId).Motion, Admission.Model, driver, 0f);
            // Premier point tres contraint (v* < 1 m/s) de la route.
            float tight = plan0.Points.First(p => !p.CeilingUnbounded && p.CeilingMetersPerSecond < 1f).DistanceMeters;

            // Atteignable seulement au freinage fort : 6 m/s a 6 m du point serre (6^2/(2x2) = 9 m > 6 m > 4,5 m).
            RoadId element;
            var near = PointAt(insertion.Route, tight - 6f, out element);
            var decision = Decide(insertion.Route, insertion.TrafficId, near.Position, near.Tangent, 6f, element);
            var hard = SpeedPlan.Build(decision.Motion, Admission.Model, driver, 6f);
            Assert.That(hard.Binding, Is.EqualTo(SpeedConstraint.SteeringCeilingUnreachable));
            Assert.That(hard.PlanningDecelerationMetersPerSecondSquared,
                Is.EqualTo(driver.SafeBrakingLimit * SpeedPlan.PlanningBoundMargin).Within(1e-4f));
            Assert.That(hard.Accepted, Is.True, "atteignable a SafeBrakingLimit : le verificateur admet le profil");
            var curve = CurveOf(decision.Path.Intervals[0]);
            var command = MotionCommand.Track(1, 1, hard, driver, Admission.Model.DrivabilityProfile, curve,
                decision.Path.Intervals[0].StartSMeters, near.Position, near.Tangent, 6f, Dt);
            Assert.That(command.TargetAccelerationMetersPerSecondSquared, Is.EqualTo(-driver.SafeBrakingLimit));

            // Inatteignable meme a SafeBrakingLimit : 8 m/s a 2 m du point serre -> refus du verificateur, donc repli.
            near = PointAt(insertion.Route, tight - 2f, out element);
            decision = Decide(insertion.Route, insertion.TrafficId, near.Position, near.Tangent, 8f, element);
            var impossible = SpeedPlan.Build(decision.Motion, Admission.Model, driver, 8f);
            Assert.That(impossible.Binding, Is.EqualTo(SpeedConstraint.SteeringCeilingUnreachable));
            Assert.That(impossible.Accepted, Is.False);
            Assert.That(impossible.Verification.Issue, Is.EqualTo(SpeedProfileIssue.SteeringCeilingExceeded));

            // Anticipation : assez loin du point serre pour que le freinage de confort suffise.
            float from = Math.Max(0.5f, tight - 30f);
            float speedEarly = Math.Min(driver.DesiredSpeed, (float)Math.Sqrt(2f * 0.8f * driver.ComfortableDeceleration * (tight - from - 1f)));
            near = PointAt(insertion.Route, from, out element);
            decision = Decide(insertion.Route, insertion.TrafficId, near.Position, near.Tangent, speedEarly, element);
            var early = SpeedPlan.Build(decision.Motion, Admission.Model, driver, speedEarly);
            Assert.That(speedEarly, Is.GreaterThan(1f));
            Assert.That(early.Accepted, Is.True);
            Assert.That(early.Binding, Is.Not.EqualTo(SpeedConstraint.SteeringCeilingUnreachable));
            Assert.That(early.Points.Any(p => p.Binding == SpeedConstraint.AnticipatedDeceleration), Is.True);
        }

        [Test]
        public void TheRoadLimitIsAppliedOrUnauthoredWhileTheCurveLimitIsApplied()
        {
            var plan = Insert(0).FirstSpeedPlan;
            // Story 5.33 (D6) : la limite de route n'est plus reportee ; elle est appliquee, ou publiee Unauthored a 0.
            Assert.That(plan.DeferredLimits, Is.Empty, "plus aucune limite reportee");
            Assert.That(plan.RoadLimits.Count, Is.GreaterThan(0));
            Assert.That(plan.RoadLimits.All(l => l.State == RoadLimitState.Unauthored), Is.True,
                "limites a 0 dans le modele signe : aucun plafond, jamais presente comme une limite appliquee");
            Assert.That(plan.LateralAccelerationMetersPerSecondSquared, Is.EqualTo(Driver.ComfortableDeceleration));
            Assert.That(plan.Points.Any(p => p.Binding == SpeedConstraint.CurveLimit
                || p.Binding == SpeedConstraint.AnticipatedDeceleration), Is.True);
            foreach (var point in plan.Points)
                Assert.That(point.SpeedMetersPerSecond, Is.LessThanOrEqualTo(point.CurveLimitMetersPerSecond * 1.0001f + 1e-4f),
                    "v <= racine(a_lat / |kappa|) au noeud " + point.DistanceMeters);

            // Valeur authoree synthetique non nulle : publiee Applied(3) et appliquee (Story 5.33).
            var source = RoadModelDocument.Load(File.ReadAllText(HistoricalSignedDirectory + "MVP_Run.road-model.json"));
            for (int i = 0; i < source.Sections.Length; i++) source.Sections[i].DefaultSpeedLimitMetersPerSecond = 3f;
            var model = RoadModelCompiler.Compile(source);
            const string modelText = "synthetic 5.31 model\n";
            const string block = "a_e = 0 m\n| Genre | Valeur |\n|---|---|\n| test | 0 |\n";
            string signoff = "{\"RoadModelVersion\":\"" + model.Version + "\",\"ModelHash\":\"" + Sha256(modelText)
                + "\",\"ClearanceHash\":\"" + Sha256(block) + "\"}";
            var evidence = GateAEvidenceBinding.Bind(model, modelText, signoff, "### Residus\n" + block + "\nfin\n");
            Assert.That(evidence.Valid, Is.True);
            var entry = model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).First();
            EffectiveLaneCorridor corridor;
            model.TryGetCorridor(entry.CorridorId, out corridor);
            var start = corridor.Curve.Sample(entry.SMeters);
            var trafficId = new RoadId(0x531, 1);
            var frame = new TrafficFrame(0, model, new[] { new TrafficActorInput(trafficId,
                new VehicleFootprintPose { Position = start.Position, Forward = start.Tangent, Up = start.Up }, 0f, entry.CorridorId) });
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, trafficId, null, RoadId.None, new RouteSeed(0),
                TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null, null, evidence));
            var authored = SpeedPlan.Build(decision.Motion, model, Driver, 0f);
            Assert.That(authored.Accepted, Is.True);
            Assert.That(authored.DeferredLimits, Is.Empty);
            var roadLimits = authored.RoadLimits.Where(l => l.ElementKind == RoadElementKind.LaneCorridor).ToList();
            Assert.That(roadLimits, Is.Not.Empty);
            Assert.That(roadLimits.All(l => l.State == RoadLimitState.Applied && l.MetersPerSecond == 3f), Is.True);
            Assert.That(authored.Points.Max(p => p.SpeedMetersPerSecond), Is.LessThanOrEqualTo(3f + 1e-4f),
                "une limite authoree est appliquee");
            Assert.That(authored.Points.Any(p => p.Binding == SpeedConstraint.RoadLimit), Is.True, "et nommee comme liante");
        }

        // ------------------------------------------------------------------ giratoires (decision du 2026-09-30)

        private static readonly RoadId NorthWestDiagonalApproach = RoadId.Parse("4f3543b1218b82af65b5b8fc58457fb3");
        private static readonly RoadId NorthWestDiagonalEntry = RoadId.Parse("452ee31e83feea5ebc05406c271424a9");

        /// <summary>Route reelle par l'entree d'anneau NW, dont le pic de courbure est au rayon d'admission (4,03 m).</summary>
        private static RoutePlan ThroughNorthWestEntry(float approachS)
        {
            var model = Admission.Model;
            var start = new RoadLocation { ModelId = model.ModelId, ModelVersion = model.Version, Localized = true,
                ElementKind = RoadElementKind.LaneCorridor, ElementId = NorthWestDiagonalApproach, SMeters = approachS };
            var result = RoutePlanner.Plan(new RouteRequest(model, start, RoadId.None, new RouteSeed(0), new RoadId(0x531, 9),
                "route", new DecisionCounter(0), null, false, null, NorthWestDiagonalEntry));
            Assert.That(result.Plan, Is.Not.Null, result.Reason.ToString());
            return result.Plan;
        }

        [Test]
        public void TheRoundaboutEntryIsPlannedAtTheCurveLimitNotAtTheSteadyStateCeiling()
        {
            var model = Admission.Model;
            var driver = Driver;
            var route = ThroughNorthWestEntry(0f);
            var steady = PathHorizon.Build(model, route, TrafficV2Settings.LookAheadMeters);
            var nominal = PathHorizon.Build(model, route, TrafficV2Settings.LookAheadMeters, 0f);
            var entrySteady = steady.Intervals.First(i => i.Id == NorthWestDiagonalEntry);
            var entry = nominal.Intervals.First(i => i.Id == NorthWestDiagonalEntry);
            // Regime etabli : 39,77 deg de roue au pic, disponibles seulement sous 0,25 m/s.
            Assert.That(entrySteady.Points.Min(p => p.SteeringCeilingMetersPerSecond), Is.LessThan(0.3f));
            Assert.That(entrySteady.Points.All(p => float.IsNaN(p.NominalOffsetRadians)), Is.True, "sans e0 : comportement 5.30");
            // Pose nominale : au plus ~32,7 deg de roue, disponibles jusqu'a ~7,9 m/s.
            Assert.That(entry.Points.Min(p => p.SteeringCeilingMetersPerSecond), Is.GreaterThan(7.5f));

            // L'horizon transporte le meme e que la reference de mesure (meme route, meme e0).
            var track = ReferenceTrack.FromRoute(model, route.Occurrences, 0f);
            for (int i = 0; i < nominal.Intervals.Count && nominal.Intervals[i].StartDistanceMeters < 60f; i++)
                foreach (var point in nominal.Intervals[i].Points)
                    Assert.That(point.NominalOffsetRadians, Is.EqualTo(track.OffsetRadians(i, point.DistanceMeters)).Within(2e-4f),
                        "e a d = " + point.DistanceMeters);

            var plan = SpeedPlan.Build(new MotionPlan(nominal, model.DrivabilityProfile, Admission.Evidence,
                TrackingTolerance.Undeclared), model, driver, 0f);
            Assert.That(plan.Accepted, Is.True, plan.Verification.Issue.ToString());
            float peak = entry.Points.Max(p => Math.Abs(p.Reference.CurvaturePerMeter));
            float curveLimit = (float)Math.Sqrt(driver.ComfortableDeceleration / peak);
            var inside = plan.Points.Where(p => p.DistanceMeters >= entry.StartDistanceMeters
                && p.DistanceMeters <= entry.EndDistanceMeters).ToList();
            // Ni rampe a 0,25 m/s ni depassement : le pic se passe a la limite de courbe (~2,8 m/s).
            Assert.That(inside.Min(p => p.SpeedMetersPerSecond), Is.GreaterThan(0.95f * curveLimit));
            Assert.That(inside.Min(p => p.SpeedMetersPerSecond), Is.LessThanOrEqualTo(curveLimit + 1e-3f));
            Assert.That(inside.Any(p => p.Binding == SpeedConstraint.CurveLimit), Is.True);

            // A l'approche, la contrainte qui borne le plan est nommee : la limite de courbe du pic.
            EffectiveLaneCorridor approach;
            model.TryGetCorridor(NorthWestDiagonalApproach, out approach);
            var near = PathHorizon.Build(model, ThroughNorthWestEntry(approach.LengthMeters - 1.5f),
                TrafficV2Settings.LookAheadMeters, 0f);
            var approaching = SpeedPlan.Build(new MotionPlan(near, model.DrivabilityProfile, Admission.Evidence,
                TrackingTolerance.Undeclared), model, driver, 4f);
            Assert.That(approaching.Accepted, Is.True, approaching.Verification.Issue.ToString());
            Assert.That(approaching.LimitingConstraint, Is.EqualTo(SpeedConstraint.CurveLimit));
        }

        [Test]
        public void TrackingAccelerationStaysInsideTheUnchangedIdmEnvelope()
        {
            var driver = Driver;
            var insertion = Insert(1);
            var track = ReferenceTrack.FromRoute(Admission.Model, insertion.Route.Occurrences);
            for (float distance = 1f; distance < track.LengthMeters - 1f; distance += 5.3f)
            {
                RoadId element;
                var point = PointAt(insertion.Route, distance, out element);
                foreach (float speed in new[] { 0f, 2f, 7.9f, 8.4f })
                {
                    var decision = Decide(insertion.Route, insertion.TrafficId, point.Position, point.Tangent, speed, element);
                    var plan = SpeedPlan.Build(decision.Motion, Admission.Model, driver, speed);
                    if (!plan.Accepted) continue;
                    var command = MotionCommand.Track(3, 1, plan, driver, Admission.Model.DrivabilityProfile,
                        CurveOf(decision.Path.Intervals[0]), decision.Path.Intervals[0].StartSMeters, point.Position,
                        point.Tangent, speed, Dt);
                    float idm = DriverModel.ComputeAcceleration(driver, speed, 0f, DriverModel.NoLeaderGap);
                    Assert.That(command.TargetAccelerationMetersPerSecondSquared, Is.LessThanOrEqualTo(idm + 1e-5f));
                    Assert.That(command.IsFinite, Is.True);
                    Assert.That(Math.Abs(command.TargetWheelAngleDegrees),
                        Is.LessThanOrEqualTo(Admission.Model.DrivabilityProfile.LowSpeedLockDegrees));
                }
            }
            // Enveloppe IDM 5.9 inchangee : nulle a v0, negative au-dela.
            Assert.That(DriverModel.ComputeAcceleration(driver, driver.DesiredSpeed, 0f, DriverModel.NoLeaderGap), Is.EqualTo(0f).Within(1e-4f));
            Assert.That(DriverModel.ComputeAcceleration(driver, 0f, 0f, DriverModel.NoLeaderGap), Is.EqualTo(driver.MaxAcceleration).Within(1e-4f));
        }

        [Test]
        public void CommandIsValidForExactlyOneStepAndCarriesItsSourceFrame()
        {
            var insertion = Insert(2);
            var decision = Decide(insertion.Route, insertion.TrafficId, insertion.Position, insertion.Rotation * Vector3.forward,
                0f, insertion.EntryPortal.CorridorId, 42);
            var plan = SpeedPlan.Build(decision.Motion, Admission.Model, Driver, 0f);
            var command = MotionCommand.Track(42, TrafficV2Settings.PlanValiditySteps, plan, Driver,
                Admission.Model.DrivabilityProfile, CurveOf(decision.Path.Intervals[0]), decision.Path.Intervals[0].StartSMeters,
                insertion.Position, insertion.Rotation * Vector3.forward, 0f, Dt);
            Assert.That(TrafficV2Settings.PlanValiditySteps, Is.EqualTo(1));
            Assert.That(command.SourceFrameId, Is.EqualTo(42UL));
            Assert.That(command.ValidFromStep, Is.EqualTo(42UL));
            Assert.That(command.ValidToStep, Is.EqualTo(42UL));
            Assert.That(command.IsValidAt(41), Is.False);
            Assert.That(command.IsValidAt(42), Is.True);
            Assert.That(command.IsValidAt(43), Is.False);
            Assert.That(command.TargetAccelerationMetersPerSecondSquared, Is.GreaterThan(0f), "depart de l'arret");
        }

        [Test]
        public void FeedforwardFollowsTheDeclaredSteeringGeometry()
        {
            var profile = Admission.Model.DrivabilityProfile;
            Assert.That(MotionCommand.FeedforwardWheelAngleDegrees(profile, 0f), Is.EqualTo(0f));
            float radius = RoadModelCompiler.RadiusMeters(profile, 20f);
            Assert.That(MotionCommand.FeedforwardWheelAngleDegrees(profile, 1f / radius), Is.EqualTo(20f).Within(1e-3f));
            Assert.That(MotionCommand.FeedforwardWheelAngleDegrees(profile, -1f / radius), Is.EqualTo(-20f).Within(1e-3f));
            Assert.That(MotionCommand.FeedforwardWheelAngleDegrees(profile, 10f), Is.EqualTo(profile.LowSpeedLockDegrees));
        }

        // ------------------------------------------------------------------ composeur

        private static CompiledRoadModel ModelOf() { return Admission.Model; }

        private static RoadCurve CurveOf(PathInterval interval)
        {
            EffectiveLaneCorridor corridor;
            CompiledJunctionMovement movement;
            if (interval.Kind == RoadElementKind.LaneCorridor && ModelOf().TryGetCorridor(interval.Id, out corridor)) return corridor.Curve;
            Assert.That(ModelOf().TryGetMovement(interval.Id, out movement), Is.True);
            return movement.Curve;
        }

        private static void AssertFinite(ComposedDrive drive)
        {
            foreach (float value in new[] { drive.Intent.Throttle, drive.Intent.Steer, drive.Intent.BrakeReverse,
                drive.Intent.Handbrake, drive.MaxForwardSpeed, drive.SteerRateDegreesPerSecond, drive.BrakeTorque })
                Assert.That(float.IsNaN(value) || float.IsInfinity(value), Is.False);
        }

        [Test]
        public void ComposerEmitsExactlyOneFiniteIntentPerStep()
        {
            var composer = new VehicleDriveIntentComposer(Vehicle, Driver.SafeBrakingLimit, Dt);
            var commands = new MotionCommand?[] { null, new MotionCommand(1, 1, 1, 1f, 5f),
                new MotionCommand(0, 0, 0, float.NaN, 0f), new MotionCommand(7, 7, 7, -3f, float.PositiveInfinity) };
            for (ulong step = 1; step <= 200; step++)
            {
                var command = commands[(int)(step % (ulong)commands.Length)];
                if (command.HasValue && command.Value.SourceFrameId != 0)
                    command = new MotionCommand(step, step, step, command.Value.TargetAccelerationMetersPerSecondSquared,
                        command.Value.TargetWheelAngleDegrees);
                var drive = composer.Compose(step, command, V2FallbackReason.None, 8f - step * 0.04f, 0.3f);
                Assert.That(drive.Step, Is.EqualTo(step));
                AssertFinite(drive);
            }
        }

        [Test]
        public void EveryNonFiniteAxisFallsBack()
        {
            foreach (var bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                var composer = new VehicleDriveIntentComposer(Vehicle, Driver.SafeBrakingLimit, Dt);
                var acceleration = composer.Compose(5, new MotionCommand(5, 5, 5, bad, 0f), V2FallbackReason.None, 3f, 0.3f);
                Assert.That(acceleration.Fallback, Is.True);
                Assert.That(acceleration.Reason, Is.EqualTo(V2FallbackReason.NonFiniteCommand));
                AssertFinite(acceleration);
                var angle = composer.Compose(6, new MotionCommand(6, 6, 6, 1f, bad), V2FallbackReason.None, 3f, 0.3f);
                Assert.That(angle.Fallback, Is.True);
                Assert.That(angle.Reason, Is.EqualTo(V2FallbackReason.NonFiniteCommand));
                AssertFinite(angle);
                // Vitesse mesuree ou trainee non finies : jamais propagees.
                AssertFinite(composer.Compose(7, new MotionCommand(7, 7, 7, 1f, 2f), V2FallbackReason.None, bad, bad));
            }
        }

        [Test]
        public void EveryNonFiniteAuthorityScalarMakesTheVehicleInert()
        {
            foreach (var field in new[] { "maxForwardSpeed", "steerRateDegreesPerSecond", "brakeTorque" })
                foreach (var bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                {
                    object boxed = Vehicle;
                    var info = typeof(VehicleProfile).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
                    Assert.That(info, Is.Not.Null, field);
                    info.SetValue(boxed, bad);
                    var composer = new VehicleDriveIntentComposer((VehicleProfile)boxed, Driver.SafeBrakingLimit, Dt);
                    var drive = composer.Compose(1, new MotionCommand(1, 1, 1, 1f, 0f), V2FallbackReason.None, 2f, 0.3f);
                    Assert.That(drive.Fallback, Is.True, field);
                    Assert.That(drive.Reason, Is.EqualTo(V2FallbackReason.NonFiniteAuthority));
                    Assert.That((drive.Diagnostics & V2ComposerDiagnostic.ProfileScalarNonFinite) != 0, Is.True);
                    Assert.That(drive.Intent.Throttle, Is.EqualTo(0f));
                    Assert.That(drive.Intent.BrakeReverse, Is.EqualTo(0f));
                    AssertFinite(drive);
                }
            var noBraking = new VehicleDriveIntentComposer(Vehicle, float.NaN, Dt);
            Assert.That(noBraking.Compose(1, new MotionCommand(1, 1, 1, 1f, 0f), V2FallbackReason.None, 2f, 0.3f).Reason,
                Is.EqualTo(V2FallbackReason.NonFiniteAuthority));
        }

        [Test]
        public void StaleOrAbsentCommandFallsBackWithItsReason()
        {
            var composer = new VehicleDriveIntentComposer(Vehicle, Driver.SafeBrakingLimit, Dt);
            var command = new MotionCommand(10, 10, 10, 1f, 3f);
            Assert.That(composer.Compose(10, command, V2FallbackReason.None, 5f, 0.3f).Fallback, Is.False);
            var late = composer.Compose(11, command, V2FallbackReason.None, 5f, 0.3f);
            Assert.That(late.Fallback, Is.True);
            Assert.That(late.Reason, Is.EqualTo(V2FallbackReason.OutsideValidityWindow));
            var early = composer.Compose(9, command, V2FallbackReason.None, 5f, 0.3f);
            Assert.That(early.Reason, Is.EqualTo(V2FallbackReason.OutsideValidityWindow));
            Assert.That(composer.Compose(12, null, V2FallbackReason.ProfileRefused, 5f, 0.3f).Reason,
                Is.EqualTo(V2FallbackReason.ProfileRefused));
            Assert.That(composer.Compose(13, null, V2FallbackReason.None, 5f, 0.3f).Reason, Is.EqualTo(V2FallbackReason.NoCommand));
            Assert.That(composer.Compose(14, null, V2FallbackReason.NoRoute, 5f, 0.3f).Reason, Is.EqualTo(V2FallbackReason.NoRoute));
        }

        [Test]
        public void FallbackRuleFollowsTheMeasuredSpeedEveryStep()
        {
            var vehicle = Vehicle;
            float safe = Driver.SafeBrakingLimit;
            var composer = new VehicleDriveIntentComposer(vehicle, safe, Dt);
            Assert.That(composer.ServiceBandMetersPerSecond, Is.EqualTo(0.41f).Within(1e-5f), "v_s = v_dir + 2 b dt");
            // Volant precedent valide conserve pendant le repli.
            var steering = composer.Compose(1, new MotionCommand(1, 1, 1, 0f, 10f), V2FallbackReason.None, 5f, 0f);
            Assert.That(steering.Fallback, Is.False);
            ulong step = 2;
            foreach (float speed in new[] { 8f, 1f, 0.42f, 0.41f, 0.35f, 0.3f, 0.25f, 0.2f, 0f, -0.1f, -0.5f, -3f })
            {
                var drive = composer.Compose(step++, null, V2FallbackReason.NoCommand, speed, 0.3f);
                Assert.That(drive.Fallback, Is.True);
                Assert.That(drive.Intent.Throttle, Is.EqualTo(0f));
                Assert.That(drive.Intent.Steer, Is.EqualTo(steering.Intent.Steer), "dernier volant fini valide");
                if (speed > composer.ServiceBandMetersPerSecond)
                {
                    Assert.That(drive.Intent.BrakeReverse, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f), speed + " m/s : frein de service");
                    Assert.That(drive.Intent.Handbrake, Is.EqualTo(0f));
                }
                else
                {
                    Assert.That(drive.Intent.BrakeReverse, Is.EqualTo(0f), speed + " m/s : jamais de BrakeReverse dans la bande");
                    Assert.That(drive.Intent.Handbrake, Is.EqualTo(1f), speed + " m/s : maintien au frein a main");
                }
                // Couple moteur recalcule par la couche physique, vitesse reelle : jamais negatif.
                Assert.That(VehicleDriveIntentComposer.MinimumWheelDriveTorque(vehicle, drive.Intent, drive.MaxForwardSpeed, speed),
                    Is.GreaterThanOrEqualTo(0f), speed + " m/s");
                bool rolling = (drive.Diagnostics & V2ComposerDiagnostic.RollingBackward) != 0;
                Assert.That(rolling, Is.EqualTo(speed < -vehicle.MinimumDirectionSpeed), speed + " m/s : RollingBackward");
            }
            // Au-dessus de la bande, le frein de service vaut SafeBrakingLimit converti par la capacite de frein.
            var fresh = new VehicleDriveIntentComposer(vehicle, safe, Dt);
            float capacity = vehicle.WheelCount * vehicle.BrakeTorque / (vehicle.Mass * vehicle.GetWheel(0).Radius);
            Assert.That(fresh.Compose(1, null, V2FallbackReason.NoCommand, 8f, 0f).Intent.BrakeReverse,
                Is.EqualTo(safe / capacity).Within(1e-4f));
        }

        [Test]
        public void FallbackEndsInHeldOrStopOverrunAndAValidCommandResumes()
        {
            var composer = new VehicleDriveIntentComposer(Vehicle, Driver.SafeBrakingLimit, Dt);
            float speed = 8f;
            ulong step = 1;
            V2FallbackTerminal terminal = V2FallbackTerminal.None;
            for (; step < 400 && terminal != V2FallbackTerminal.Held; step++)
            {
                terminal = composer.Compose(step, null, V2FallbackReason.NoCommand, speed, 0f).Terminal;
                speed = Math.Max(0f, speed - Driver.SafeBrakingLimit * Dt);
            }
            Assert.That(terminal, Is.EqualTo(V2FallbackTerminal.Held));
            Assert.That(composer.Compose(step++, null, V2FallbackReason.NoCommand, 0f, 0f).Diagnostics
                & V2ComposerDiagnostic.FallbackHeld, Is.EqualTo(V2ComposerDiagnostic.FallbackHeld));
            var resumed = composer.Compose(step, new MotionCommand(step, step, step, 1f, 0f), V2FallbackReason.None, 0f, 0f);
            Assert.That(resumed.Fallback, Is.False, "une commande valide reprend la main");
            Assert.That(resumed.Terminal, Is.EqualTo(V2FallbackTerminal.None));

            // Pas d'arret dans FallbackOverrunFactor x v0 / b : diagnostic, et le maintien continue.
            var overrun = new VehicleDriveIntentComposer(Vehicle, Driver.SafeBrakingLimit, Dt);
            int steps = 0;
            ComposedDrive drive;
            do { drive = overrun.Compose((ulong)(++steps), null, V2FallbackReason.NoCommand, 1f, 0f); }
            while (drive.Terminal != V2FallbackTerminal.StopOverrun && steps < 200);
            Assert.That(drive.Terminal, Is.EqualTo(V2FallbackTerminal.StopOverrun));
            Assert.That(steps * Dt, Is.GreaterThan(VehicleDriveIntentComposer.FallbackOverrunFactor * 1f / Driver.SafeBrakingLimit));
            Assert.That(drive.Fallback, Is.True);
            Assert.That(drive.Intent.BrakeReverse, Is.GreaterThan(0f), "le freinage continue apres le diagnostic");
            Assert.That(VehicleDriveIntentComposer.FallbackStoppedSpeedMetersPerSecond, Is.EqualTo(0.05f));
            Assert.That(VehicleDriveIntentComposer.FallbackStoppedSteps, Is.EqualTo(10));
            Assert.That(VehicleDriveIntentComposer.FallbackOverrunFactor, Is.EqualTo(2f));
        }

        [Test]
        public void NormalCommandNeverUsesBrakeReverseInsideTheServiceBand()
        {
            var vehicle = Vehicle;
            var composer = new VehicleDriveIntentComposer(vehicle, Driver.SafeBrakingLimit, Dt);
            ulong step = 1;
            foreach (float speed in new[] { 0.41f, 0.3f, 0.25f, 0.1f, 0.04f, 0f })
            {
                var drive = composer.Compose(step, new MotionCommand(step, step, step, -3f, 0f), V2FallbackReason.None, speed, 0.3f);
                step++;
                Assert.That(drive.Fallback, Is.False);
                Assert.That(drive.Intent.BrakeReverse, Is.EqualTo(0f), speed + " m/s");
                Assert.That(VehicleDriveIntentComposer.MinimumWheelDriveTorque(vehicle, drive.Intent, drive.MaxForwardSpeed, speed),
                    Is.GreaterThanOrEqualTo(0f));
            }
            var fast = composer.Compose(step, new MotionCommand(step, step, step, -3f, 0f), V2FallbackReason.None, 5f, 0f);
            Assert.That(fast.Intent.BrakeReverse, Is.GreaterThan(0f));
            Assert.That(fast.Intent.Throttle, Is.EqualTo(0f));
        }

        [Test]
        public void IdleKeepsItsV1Semantics()
        {
            Assert.That(VehicleDriveIntent.Idle.Throttle, Is.EqualTo(0f));
            Assert.That(VehicleDriveIntent.Idle.Steer, Is.EqualTo(0f));
            Assert.That(VehicleDriveIntent.Idle.BrakeReverse, Is.EqualTo(0f));
            Assert.That(VehicleDriveIntent.Idle.Handbrake, Is.EqualTo(0f));
            Assert.That(VehicleDriveIntent.Idle.IsIdle, Is.True);
        }

        private static string Sha256(string value)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }
    }
}
