using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.31 : admission et refus nommes, composition de session, identite deterministe, retrait au
    /// portail, borne du gabarit au pas et entre deux pas (modele M), H3 sur le prefab V2 et gardes
    /// structurelles (composeur unique, application unique, aucune ecriture de corps, jeton de mesure).
    /// </summary>
    [Category("Core")]
    [Category("Story531")]
    public sealed class Story531LifecycleAndCompositionTests
    {
        private const string V2PrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab";
        private const string TrafficRoot = "Assets/RoadRage/Features/Vehicles/Traffic";
        private const float Dt = 0.02f;

        private static string modelText, signoffText, reportText;
        private static TrafficV2Admission admission;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                {
                    modelText = File.ReadAllText(TrafficV2Settings.ModelPath);
                    signoffText = File.ReadAllText(TrafficV2Settings.SignoffPath);
                    reportText = File.ReadAllText(TrafficV2Settings.ReportPath);
                    admission = TrafficV2Lifecycle.Admit(modelText, signoffText, reportText);
                }
                return admission;
            }
        }

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>("Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset").Profile; }
        }

        [TearDown]
        public void TearDown()
        {
            TrafficV2Session.Reset();
        }

        // ------------------------------------------------------------------ admission et refus

        [Test]
        public void CommittedArtifactsAreAdmittedButNoVehicleDrivesOutsideMeasurement()
        {
            Assert.That(Admission.Code, Is.EqualTo(TrafficV2Code.Allowed));
            Assert.That(Admission.Evidence.Valid, Is.True);
            Assert.That(Admission.Evidence.TrackingAllowanceMeters, Is.EqualTo(0f), "a_e = 0 signe");
            // Etape 2 du protocole : epsilon_t declare par le proprietaire le 2026-09-30.
            Assert.That(TrafficV2Settings.DeclaredTrackingTolerance.Declared, Is.True);
            Assert.That(TrafficV2Settings.DeclaredTrackingTolerance.Meters, Is.EqualTo(0.34f));

            var undeclared = TrafficV2Lifecycle.EvaluateInsertion(Admission, null, TrackingTolerance.Undeclared);
            Assert.That(undeclared.Allowed, Is.False);
            Assert.That(undeclared.Code, Is.EqualTo(TrafficV2Code.TrackingToleranceUndeclared));
            Assert.That(undeclared.VehicleCoverage, Is.EqualTo(VehicleCoverage.TrackingToleranceUndeclared));

            // epsilon_t declare n'est pas couvert : la preuve signee est a pose tangente (contrat §8), et a_e = 0 ;
            // la regle 5.30 "compte 0" n'autorise rien. Hors mesure, aucune insertion jusqu'a la 5.52.
            Assert.That(Admission.Evidence.PoseModel, Is.EqualTo(NominalPoseModel.TangentAligned));
            var outside = TrafficV2Lifecycle.EvaluateInsertion(Admission, null, TrafficV2Settings.DeclaredTrackingTolerance);
            Assert.That(outside.Allowed, Is.False);
            Assert.That(outside.Code, Is.EqualTo(TrafficV2Code.NotCoveredByGateA));
            Assert.That(outside.VehicleCoverage, Is.EqualTo(VehicleCoverage.PoseModelMismatch));
            Assert.That(outside.MeasurementLabel, Is.Null);
            Assert.That(MotionPlan.EvaluateVehicleCoverage(Admission.Evidence, 0f, new TrackingTolerance(float.NaN)),
                Is.EqualTo(VehicleCoverage.NotCoveredByGateA));

            var run = new MeasurementRun(MeasurementKind.Exploratory, "unit", new[] { new CampaignTriplet(new RoadId(1, 1), new RoadId(1, 2), 3) });
            var measured = TrafficV2Lifecycle.EvaluateInsertion(Admission, run, TrafficV2Settings.DeclaredTrackingTolerance);
            Assert.That(measured.Allowed, Is.True);
            Assert.That(measured.MeasurementLabel, Is.EqualTo("Exploratory:unit"));
            Assert.That(measured.VehicleCoverage, Is.EqualTo(VehicleCoverage.PoseModelMismatch), "la couverture reste publiee sous mesure");
        }

        [Test]
        public void EvidenceAndModelRefusalsAreNamed()
        {
            var admitted = Admission;
            Assert.That(TrafficV2Lifecycle.Admit(null, signoffText, reportText).Code, Is.EqualTo(TrafficV2Code.RoadModelMissing));
            Assert.That(TrafficV2Lifecycle.Admit(modelText, signoffText, null).Code, Is.EqualTo(TrafficV2Code.GateAEvidenceMissing));
            Assert.That(TrafficV2Lifecycle.Admit(modelText, null, reportText).Code, Is.EqualTo(TrafficV2Code.GateAEvidenceMissing));
            string altered = reportText.Replace("\na_e = 0 m", "\na_e = 0.0 m");
            Assert.That(altered, Is.Not.EqualTo(reportText));
            var stale = TrafficV2Lifecycle.Admit(modelText, signoffText, altered);
            Assert.That(stale.Code, Is.EqualTo(TrafficV2Code.GateAEvidenceStale), "bloc hache altere : preuve perimee");

            string undeclared = modelText.Replace("\"Declared\":true", "\"Declared\":false");
            Assert.That(undeclared, Is.Not.EqualTo(modelText));
            var refused = TrafficV2Lifecycle.Admit(undeclared, signoffText, reportText);
            Assert.That(refused.Code, Is.EqualTo(TrafficV2Code.UndeclaredDrivabilityProfile));
            Assert.That(TrafficV2Lifecycle.EvaluateInsertion(refused, null, TrafficV2Settings.DeclaredTrackingTolerance).Code,
                Is.EqualTo(TrafficV2Code.UndeclaredDrivabilityProfile));
            Assert.That(TrafficV2Lifecycle.EvaluateInsertion(null, null, TrafficV2Settings.DeclaredTrackingTolerance).Code,
                Is.EqualTo(TrafficV2Code.RoadModelMissing));
            Assert.That(admitted.Code, Is.EqualTo(TrafficV2Code.Allowed));
        }

        [Test]
        public void EditorAdmissionIsCachedPerModelAndWritesNoEvidence()
        {
            var before = new[] { TrafficV2Settings.ModelPath, TrafficV2Settings.SignoffPath, TrafficV2Settings.ReportPath }
                .Select(File.ReadAllBytes).ToArray();
            var first = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            var second = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(first.Code, Is.EqualTo(TrafficV2Code.Allowed));
            Assert.That(second, Is.SameAs(first), "resultat de liaison mis en cache par modele");
            var after = new[] { TrafficV2Settings.ModelPath, TrafficV2Settings.SignoffPath, TrafficV2Settings.ReportPath }
                .Select(File.ReadAllBytes).ToArray();
            for (int i = 0; i < before.Length; i++) Assert.That(after[i], Is.EqualTo(before[i]), "aucune preuve n'est ecrite");
        }

        // ------------------------------------------------------------------ composition

        [Test]
        public void CompositionDefaultsToV1AndIsFrozenBeforeTheFirstInsertion()
        {
            TrafficV2Session.Reset();
            Assert.That(TrafficV2Session.Composition, Is.EqualTo(TrafficComposition.V1));
            Assert.That(TrafficV2Session.Measurement, Is.Null);
            Assert.That(TrafficV2Settings.V2SliceMaxPopulation, Is.EqualTo(1));

            var host = new GameObject("Story531Spawner");
            try
            {
                var spawner = host.AddComponent<PortalTrafficSpawner>();
                var resolve = typeof(PortalTrafficSpawner).GetMethod("ResolveCompositionOnce", BindingFlags.Instance | BindingFlags.NonPublic);
                TrafficV2Session.Request(TrafficComposition.V2Slice, null);
                resolve.Invoke(spawner, null);
                Assert.That(spawner.CompositionFrozen, Is.True);
                Assert.That(spawner.Composition, Is.EqualTo(TrafficComposition.V2Slice));

                TrafficV2Session.Request(TrafficComposition.V1, null);
                LogAssert.Expect(LogType.Warning, new Regex("composition de trafic changee apres la premiere lecture"));
                resolve.Invoke(spawner, null);
                resolve.Invoke(spawner, null);
                Assert.That(spawner.Composition, Is.EqualTo(TrafficComposition.V2Slice), "choix fige, changement ignore");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        // ------------------------------------------------------------------ cycle de vie

        [Test]
        public void IdentityAndRouteAreDeterministicAndInsertionStartsAtThePortal()
        {
            Assert.That(TrafficV2Lifecycle.TrafficIdentity(7, 3), Is.EqualTo(TrafficV2Lifecycle.TrafficIdentity(7, 3)));
            Assert.That(TrafficV2Lifecycle.TrafficIdentity(7, 3), Is.Not.EqualTo(TrafficV2Lifecycle.TrafficIdentity(7, 4)));
            Assert.That(TrafficV2Lifecycle.TrafficIdentity(7, 3), Is.Not.EqualTo(TrafficV2Lifecycle.TrafficIdentity(8, 3)));
            Assert.That(TrafficV2Lifecycle.TrafficIdentity(0, 1).IsEmpty, Is.False, "graine de session 0 (jamais ecrite) : identite non vide");

            var model = Admission.Model;
            var entry = model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).First();
            var exit = model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id).First();
            var a = TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, RoadId.None, 11, 2, Driver, Dt);
            var b = TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, RoadId.None, 11, 2, Driver, Dt);
            Assert.That(a.Code, Is.EqualTo(TrafficV2Code.Allowed));
            Assert.That(a.TrafficId, Is.EqualTo(new RoadId(11, 2)));
            Assert.That(a.Route.Occurrences.Select(o => o.Id), Is.EqualTo(b.Route.Occurrences.Select(o => o.Id)));
            Assert.That(a.Route.ExitPortalId, Is.EqualTo(b.Route.ExitPortalId));

            EffectiveLaneCorridor corridor;
            model.TryGetCorridor(entry.CorridorId, out corridor);
            var sample = corridor.Curve.Sample(entry.SMeters);
            Assert.That(Vector3.Distance(a.Position, sample.Position), Is.LessThan(1e-4f), "point de reference au s du portail");
            Assert.That(Vector3.Angle(a.Rotation * Vector3.forward, sample.Tangent), Is.LessThan(1e-2f), "cap tangent");
            Assert.That(a.FirstSpeedPlan.Points[0].SpeedMetersPerSecond, Is.EqualTo(0f), "vitesse nulle");

            Assert.That(TrafficV2Lifecycle.PrepareInsertion(Admission, exit.Id, RoadId.None, 1, 1, Driver, Dt).Code,
                Is.EqualTo(TrafficV2Code.UnknownPortal), "on n'insere jamais a un portail de sortie");
            Assert.That(TrafficV2Lifecycle.PrepareInsertion(Admission, new RoadId(9, 9), RoadId.None, 1, 1, Driver, Dt).Code,
                Is.EqualTo(TrafficV2Code.UnknownPortal));
            Assert.That(TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, new RoadId(9, 9), 1, 1, Driver, Dt).Code,
                Is.EqualTo(TrafficV2Code.FirstDecisionNotDrivable), "sortie imposee inconnue : premiere decision non conduisible");
        }

        [Test]
        public void RemovalHappensOnlyOnTheExitCorridorAtOrBeyondThePortal()
        {
            var model = Admission.Model;
            var exit = model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id).First();
            var location = new RoadLocation { Localized = true, ElementKind = RoadElementKind.LaneCorridor,
                ElementId = exit.CorridorId, SMeters = exit.SMeters - 0.01f };
            Assert.That(TrafficV2Lifecycle.HasReachedExit(location, exit), Is.False);
            location.SMeters = exit.SMeters;
            Assert.That(TrafficV2Lifecycle.HasReachedExit(location, exit), Is.True);
            location.SMeters = exit.SMeters + 1f;
            Assert.That(TrafficV2Lifecycle.HasReachedExit(location, exit), Is.True);
            location.Localized = false;
            Assert.That(TrafficV2Lifecycle.HasReachedExit(location, exit), Is.False);
            location.Localized = true;
            location.ElementId = model.Corridors.First(c => c.CorridorId != exit.CorridorId).CorridorId;
            Assert.That(TrafficV2Lifecycle.HasReachedExit(location, exit), Is.False, "jamais ailleurs qu'au corridor du portail");
            location.ElementKind = RoadElementKind.JunctionMovement;
            location.ElementId = exit.CorridorId;
            Assert.That(TrafficV2Lifecycle.HasReachedExit(location, exit), Is.False);
        }

        // ------------------------------------------------------------------ mesure : au pas et entre deux pas

        private static readonly GaugeBox Gauge = new GaugeBox(1.03f, 4.5f, 0.02f, 1.44f);
        private static readonly Vector3 ComLocal = new Vector3(0f, -0.35f, 0f);

        private static TrackPiece Straight(float startDistance, Vector3 origin, Vector3 direction, float length, RoadId id)
        {
            var samples = new List<RoadCurveSample>();
            for (int i = 0; i <= 20; i++)
                samples.Add(new RoadCurveSample { SMeters = length * i / 20f, Position = origin + direction * (length * i / 20f),
                    Tangent = direction, Up = Vector3.up, HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f });
            return new TrackPiece(RoadElementKind.LaneCorridor, id, startDistance, 0f, length, samples);
        }

        /// <summary>Arc a droite (courbure positive), centre a droite du depart.</summary>
        private static TrackPiece Arc(float startDistance, Vector3 start, float headingRadians, float radius, float length, RoadId id)
        {
            var samples = new List<RoadCurveSample>();
            Vector3 forward0 = new Vector3(Mathf.Sin(headingRadians), 0f, Mathf.Cos(headingRadians));
            Vector3 right0 = Vector3.Cross(Vector3.up, forward0);
            Vector3 center = start + right0 * radius;
            int count = Mathf.CeilToInt(length / 0.05f);
            for (int i = 0; i <= count; i++)
            {
                float s = length * i / count;
                float angle = headingRadians + s / radius;
                Vector3 forward = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                samples.Add(new RoadCurveSample { SMeters = s, Position = center - right * radius, Tangent = forward, Up = Vector3.up,
                    CurvaturePerMeter = 1f / radius, HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f });
            }
            return new TrackPiece(RoadElementKind.JunctionMovement, id, startDistance, 0f, length, samples);
        }

        private static BodyState State(Vector3 position, Quaternion rotation, Vector3 linear, Vector3 angular)
        {
            return new BodyState(position, rotation, position + rotation * ComLocal, linear, angular);
        }

        /// <summary>Pas suivant qui respecte exactement le modele M (vitesses de fin de pas).</summary>
        private static BodyState Next(BodyState a, Vector3 linear, Vector3 angular)
        {
            var rotation = Quaternion.AngleAxis(angular.magnitude * Dt * Mathf.Rad2Deg, angular.sqrMagnitude > 0f ? angular.normalized : Vector3.up) * a.Rotation;
            var com = a.CenterOfMass + linear * Dt;
            return new BodyState(com - rotation * ComLocal, rotation, com, linear, angular);
        }

        private static float BruteForceDisplacement(Vector3 position, Quaternion rotation, NominalPose nominal)
        {
            float best = 0f;
            Vector3 right = Vector3.Cross(nominal.Up, nominal.Forward);
            for (int i = 0; i < 8; i++)
            {
                var c = Gauge.Corner(i);
                Vector3 offset = position + rotation * c - (nominal.Position + right * c.x + nominal.Up * c.y + nominal.Forward * c.z);
                offset.y = 0f;
                best = Math.Max(best, offset.magnitude);
            }
            return best;
        }

        // ------------------------------------------------------------------ pose nominale cinematique (contrat §8)

        private const float ReferenceAhead = 1.55f;

        [Test]
        public void AtARingSplitTheRouteBranchKeepsTheLocalizationUnderARealisticTrackingOffset()
        {
            // Decision du 2026-09-30 : cap contre l'orientation nominale du candidat (e transporte depuis les ancres)
            // et bonus de route = bande de score. Split NW : continuation d'anneau (route) contre sortie Ouest.
            var model = Admission.Model;
            var ringMerge = RoadId.Parse("4ec40e5f82f7a65bed1dc3d9679f8693");
            var ringContinuation = RoadId.Parse("469fe81415e729b4258c4b7eda9e65a2");
            var exit = RoadId.Parse("4ac98ed2e41d83c91f0714135aa67ba7");
            var start = new RoadLocation { ModelId = model.ModelId, ModelVersion = model.Version, Localized = true,
                ElementKind = RoadElementKind.LaneCorridor, ElementId = ringMerge, SMeters = 0f };
            var route = RoutePlanner.Plan(new RouteRequest(model, start, RoadId.None, new RouteSeed(0), new RoadId(0x531, 7),
                "route", new DecisionCounter(0), null, false, null, ringContinuation)).Plan;
            Assert.That(route, Is.Not.Null);
            Assert.That(route.Occurrences[1].Id, Is.EqualTo(ringContinuation));
            // Regime etabli de l'anneau (R = 6 m) des l'entree du corridor : e = -asin(a / R).
            var track = ReferenceTrack.FromRoute(model, route.Occurrences,
                -Mathf.Asin(model.DrivabilityProfile.ReferencePointAheadRearAxleMeters / 6f));
            var routeIds = TrafficV2Lifecycle.ExpectedElements(route);
            bool tangentFlips = false;
            // Ecart de suivi au split (campagnes 5.31 : ~6 cm et ~2,7 deg), majore : 10 cm vers l'exterieur, 2 deg de lacet.
            for (int k = 2; k <= 13; k++)
            {
                float d = track.Pieces[1].StartDistanceMeters + 0.1f * k;
                var nominal = track.Nominal(1, d);
                var reference = track.Pieces[1].Curve.Sample(track.Pieces[1].ElementS(d));
                var pose = new VehicleFootprintPose { Position = nominal.Position + reference.Right * 0.10f,
                    Forward = Quaternion.AngleAxis(2f, nominal.Up) * nominal.Forward, Up = nominal.Up };
                var located = RoadLocalizer.Localize(model, pose, ringMerge, routeIds, track.KinematicAnchors(1));
                Assert.That(located.ElementId, Is.EqualTo(ringContinuation), "u = " + (0.1f * k) + " m apres le split");
                tangentFlips |= RoadLocalizer.Localize(model, pose, ringMerge, routeIds).ElementId == exit;
            }
            Assert.That(tangentFlips, Is.True, "sans ancre, le cap contre la tangente bascule encore vers la sortie");
        }

        [Test]
        public void OutsideEnvelopeSeparatesASeamOverrunFromAWidthOverflow()
        {
            // Criteres de contact (contrat §8, 2026-09-30) : seule l'enveloppe de largeur compte comme sortie de route.
            // Le drapeau OutsideEnvelope reste l'union des deux causes (comportement 5.26 inchange).
            var model = Admission.Model;
            var ringMerge = RoadId.Parse("4ec40e5f82f7a65bed1dc3d9679f8693");
            var ringContinuation = RoadId.Parse("469fe81415e729b4258c4b7eda9e65a2");
            var start = new RoadLocation { ModelId = model.ModelId, ModelVersion = model.Version, Localized = true,
                ElementKind = RoadElementKind.LaneCorridor, ElementId = ringMerge, SMeters = 0f };
            var route = RoutePlanner.Plan(new RouteRequest(model, start, RoadId.None, new RouteSeed(0), new RoadId(0x531, 8),
                "route", new DecisionCounter(0), null, false, null, ringContinuation)).Plan;
            Assert.That(route.Occurrences[1].Id, Is.EqualTo(ringContinuation));
            var track = ReferenceTrack.FromRoute(model, route.Occurrences);
            var seam = track.Pieces[1];
            var past = seam.Curve.Sample(seam.ElementS(seam.StartDistanceMeters + 0.03f));
            var overrun = RoadLocalizer.Localize(model, new VehicleFootprintPose { Position = past.Position,
                Forward = past.Tangent, Up = past.Up }, ringMerge, null);
            Assert.That(overrun.ElementId, Is.EqualTo(ringMerge), "hysteresis : le precedent reste retenu 3 cm apres la couture");
            Assert.That(overrun.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.True, "le drapeau 5.26 couvre le depassement");
            Assert.That(overrun.LongitudinalOverrunMeters, Is.GreaterThan(0f));
            Assert.That(overrun.OutsideWidthEnvelope, Is.False, "depassement longitudinal : pas une sortie de route");

            var merge = track.Pieces[0];
            var middle = merge.Curve.Sample(merge.ElementS(0.5f * (merge.StartDistanceMeters + seam.StartDistanceMeters)));
            // Reference 0,3 m au-dela du bord droit de la voie.
            var aside = RoadLocalizer.Localize(model, new VehicleFootprintPose {
                Position = middle.Position + middle.Right * (middle.HalfWidthRightMeters + 0.3f),
                Forward = middle.Tangent, Up = middle.Up }, ringMerge, null);
            Assert.That(aside.ElementId, Is.EqualTo(ringMerge));
            Assert.That(aside.OutsideWidthEnvelope, Is.True, "reference hors de l'enveloppe de largeur");
            Assert.That(aside.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.True, "le drapeau couvre le debordement");
            Assert.That(aside.LongitudinalOverrunMeters, Is.EqualTo(0f));
        }

        [Test]
        public void KinematicOffsetStartsAtZeroAndConvergesMonotonicallyToTheSteadyValueOnAnArc()
        {
            const float radius = 6f;
            var track = new ReferenceTrack(new[] {
                Straight(0f, Vector3.zero, Vector3.forward, 10f, new RoadId(1, 1)),
                Arc(10f, new Vector3(0f, 0f, 10f), 0f, radius, 20f, new RoadId(1, 2)) }, ReferenceAhead, 0f);
            Assert.That(track.OffsetRadians(0, 0f), Is.EqualTo(0f), "e = 0 a l'insertion");
            Assert.That(track.OffsetRadians(0, 10f), Is.EqualTo(0f).Within(1e-6f), "ligne droite : aucun ecart");

            float steady = Mathf.Asin(ReferenceAhead / radius);
            float previous = 0f;
            for (float d = 10f; d <= 30f; d += 0.05f)
            {
                float e = track.OffsetRadians(1, d);
                Assert.That(e, Is.GreaterThanOrEqualTo(previous - 1e-6f), "convergence monotone");
                Assert.That(e, Is.LessThanOrEqualTo(steady + 1e-5f), "jamais au-dela du regime etabli");
                previous = e;
            }
            // 20 m ~ 12 longueurs de relaxation a / cos(e*) : la solution a rejoint asin(a kappa).
            Assert.That(track.OffsetRadians(1, 30f), Is.EqualTo(steady).Within(1e-4f));
            // Le regime etabli n'est pas la pose nominale : juste apres le raccord, e est encore loin de e*.
            Assert.That(track.OffsetRadians(1, 10.5f), Is.LessThan(0.5f * steady));
        }

        [Test]
        public void ASeamTangentJumpShiftsTheOffsetButKeepsTheBodyHeadingContinuous()
        {
            Vector3 turned = Quaternion.AngleAxis(3f, Vector3.up) * Vector3.forward;
            var track = new ReferenceTrack(new[] {
                Straight(0f, Vector3.zero, Vector3.forward, 10f, new RoadId(2, 1)),
                Straight(10f, new Vector3(0f, 0f, 10f), turned, 10f, new RoadId(2, 2)) }, ReferenceAhead, 0f);
            Assert.That(track.OffsetRadians(1, 10f) * Mathf.Rad2Deg, Is.EqualTo(3f).Within(1e-3f), "e+ = e- + saut de tangente signe");
            var left = track.Nominal(0, 10f);
            var right = track.Nominal(1, 10f);
            Assert.That(Vector3.Angle(left.Forward, right.Forward), Is.LessThan(1e-3f), "cap de caisse continu au raccord");
            Assert.That(Math.Abs(track.OffsetRadians(1, 20f)), Is.LessThan(Math.Abs(track.OffsetRadians(1, 10f))), "puis relaxation");
        }

        [Test]
        public void TheNominalBodyTurnsTowardTheOutsideOfTheCurveAndAMatchingBodyMeasuresZero()
        {
            const float radius = 6f;
            var track = new ReferenceTrack(new[] { Arc(0f, Vector3.zero, 0f, radius, 30f, new RoadId(3, 1)) }, ReferenceAhead, 0f);
            float e = track.OffsetRadians(0, 25f);
            Assert.That(e, Is.GreaterThan(0f), "virage a droite : e > 0");
            var tangentPose = track.Pieces[0].Nominal(25f);
            var nominal = track.Nominal(0, 25f);
            Assert.That(Vector3.SignedAngle(tangentPose.Forward, nominal.Forward, Vector3.up), Is.EqualTo(-e * Mathf.Rad2Deg).Within(1e-3f),
                "caisse tournee de -e : vers l'exterieur du virage");
            Assert.That(track.NominalHeadingErrorDegrees(0, 25f), Is.EqualTo(-e * Mathf.Rad2Deg).Within(1e-4f));

            var body = State(nominal.Position, Quaternion.LookRotation(nominal.Forward, nominal.Up), Vector3.zero, Vector3.zero);
            Assert.That(TrackingMeasurement.StepDisplacement(body, track, 25f, Gauge), Is.LessThan(1e-4f),
                "une caisse exactement sur sa pose nominale ne mesure rien");
            var tangentBody = State(tangentPose.Position, Quaternion.LookRotation(tangentPose.Forward, tangentPose.Up), Vector3.zero, Vector3.zero);
            Assert.That(TrackingMeasurement.StepDisplacement(tangentBody, track, 25f, Gauge), Is.GreaterThan(0.3f),
                "une caisse alignee sur la tangente est ecartee de sa pose nominale");
        }

        [Test]
        public void ANonZeroInitialOffsetIsCarriedAndATrackWithoutProfileStaysTangent()
        {
            var replanned = new ReferenceTrack(new[] { Straight(0f, Vector3.zero, Vector3.forward, 10f, new RoadId(4, 1)) },
                ReferenceAhead, 0.2f);
            Assert.That(replanned.OffsetRadians(0, 0f), Is.EqualTo(0.2f), "un replan reprend l'ecart courant, jamais 0");
            Assert.That(replanned.OffsetRadians(0, 10f), Is.LessThan(0.2f).And.GreaterThan(0f));

            var plain = new ReferenceTrack(new[] { Arc(0f, Vector3.zero, 0f, 6f, 10f, new RoadId(4, 2)) });
            Assert.That(plain.HasKinematicPose, Is.False);
            Assert.That(Vector3.Angle(plain.Nominal(0, 5f).Forward, plain.Pieces[0].Nominal(5f).Forward), Is.LessThan(1e-4f));
        }

        [Test]
        public void ConstantLateralOffsetOnAStraightIsBoundedWithinOneMillimetre()
        {
            var track = new ReferenceTrack(new[] { Straight(0f, Vector3.zero, Vector3.forward, 50f, new RoadId(1, 1)) });
            var velocity = new Vector3(0f, 0f, 8f);
            var a = State(new Vector3(0.3f, 0f, 10f), Quaternion.identity, velocity, Vector3.zero);
            var b = Next(a, velocity, Vector3.zero);
            Assert.That(TrackingMeasurement.StepDisplacement(a, track, 10f, Gauge), Is.EqualTo(0.3f).Within(1e-5f));
            var result = TrackingMeasurement.InterStepBound(a, b, Dt, 10f, 10f + 8f * Dt, track, Gauge,
                TrafficV2Settings.InterStepRemainderMeters, TrafficV2Settings.ModelPositionToleranceMeters,
                TrafficV2Settings.ModelRotationToleranceDegrees);
            Assert.That(result.ModelVerified, Is.True);
            Assert.That(result.BoundMeters, Is.GreaterThanOrEqualTo(0.3f - 1e-5f));
            Assert.That(result.BoundMeters, Is.LessThanOrEqualTo(0.3f + 0.001f + 1e-5f));
        }

        [Test]
        public void RotationRollAndPitchAreMeasuredAtTheEightCorners()
        {
            var nominal = new NominalPose(Vector3.zero, Vector3.forward, Vector3.up);
            foreach (var rotation in new[] { Quaternion.Euler(0f, 5f, 0f), Quaternion.Euler(0f, 0f, 3f), Quaternion.Euler(4f, 0f, 0f),
                Quaternion.Euler(2f, -7f, 1.5f) })
            {
                float measured = TrackingMeasurement.CornerDisplacement(Vector3.zero, rotation, nominal, Gauge);
                Assert.That(measured, Is.EqualTo(BruteForceDisplacement(Vector3.zero, rotation, nominal)).Within(1e-5f));
                Assert.That(measured, Is.GreaterThan(0f));
            }
            // Cap pur : chaque coin tourne de 2 rho sin(theta/2).
            float yaw = TrackingMeasurement.CornerDisplacement(Vector3.zero, Quaternion.Euler(0f, 5f, 0f), nominal, Gauge);
            Assert.That(yaw, Is.EqualTo(2f * Gauge.Rho * Mathf.Sin(2.5f * Mathf.Deg2Rad)).Within(1e-4f));
            // Roulis : seuls les coins hauts (hauteur du collider) le revelent ; un controle a 4 coins au sol le manquerait.
            var flat = new GaugeBox(Gauge.HalfWidthMeters, Gauge.LengthMeters, 0f, 0f);
            float roll = TrackingMeasurement.CornerDisplacement(Vector3.zero, Quaternion.Euler(0f, 0f, 3f), nominal, Gauge);
            float rollFlat = TrackingMeasurement.CornerDisplacement(Vector3.zero, Quaternion.Euler(0f, 0f, 3f), nominal, flat);
            Assert.That(roll, Is.GreaterThan(rollFlat + 0.05f));
        }

        [Test]
        public void SeamCrossingIsSplitAndBothOneSidedPosesAreEvaluated()
        {
            // Raccord avec saut de cap de 3 degres : les deux poses unilaterales different au raccord.
            var first = Straight(0f, Vector3.zero, Vector3.forward, 10f, new RoadId(1, 1));
            var direction = Quaternion.Euler(0f, 3f, 0f) * Vector3.forward;
            var second = Straight(10f, new Vector3(0f, 0f, 10f), direction, 10f, new RoadId(1, 2));
            var track = new ReferenceTrack(new[] { first, second });
            var velocity = new Vector3(0f, 0f, 8f);
            var a = State(new Vector3(0f, 0f, 9.95f), Quaternion.identity, velocity, Vector3.zero);
            var b = Next(a, velocity, Vector3.zero);
            var result = TrackingMeasurement.InterStepBound(a, b, Dt, 9.95f, 9.95f + 0.16f, track, Gauge, 0.001f, 0.002f, 0.05f);
            Assert.That(result.Pieces.Count, Is.EqualTo(2), "l'intervalle est decoupe au raccord");
            Assert.That(result.Pieces[0].Piece, Is.EqualTo(0));
            Assert.That(result.Pieces[1].Piece, Is.EqualTo(1));
            float left = TrackingMeasurement.CornerDisplacement(new Vector3(0f, 0f, 10f), Quaternion.identity, first.Nominal(10f), Gauge);
            float right = TrackingMeasurement.CornerDisplacement(new Vector3(0f, 0f, 10f), Quaternion.identity, second.Nominal(10f), Gauge);
            Assert.That(right, Is.GreaterThan(left + 0.1f), "la pose de droite porte le saut de cap");
            Assert.That(result.BoundMeters, Is.GreaterThanOrEqualTo(Math.Max(left, right) - 1e-4f));
        }

        [Test]
        public void ViolatedModelIsReportedNotVerified()
        {
            var track = new ReferenceTrack(new[] { Straight(0f, Vector3.zero, Vector3.forward, 50f, new RoadId(1, 1)) });
            var velocity = new Vector3(0f, 0f, 8f);
            var a = State(new Vector3(0f, 0f, 10f), Quaternion.identity, velocity, Vector3.zero);
            var jumped = State(new Vector3(0.2f, 0f, 10.16f), Quaternion.identity, velocity, Vector3.zero);
            var result = TrackingMeasurement.InterStepBound(a, jumped, Dt, 10f, 10.16f, track, Gauge, 0.001f,
                TrafficV2Settings.ModelPositionToleranceMeters, TrafficV2Settings.ModelRotationToleranceDegrees);
            Assert.That(result.ModelVerified, Is.False, "variation de pose incompatible avec les vitesses de fin de pas");
            Assert.That(result.PositionResidualMeters, Is.GreaterThan(0.1f));
            var turned = new BodyState(a.Position + velocity * Dt, Quaternion.Euler(0f, 2f, 0f), a.CenterOfMass + velocity * Dt, velocity, Vector3.zero);
            Assert.That(TrackingMeasurement.InterStepBound(a, turned, Dt, 10f, 10.16f, track, Gauge, 0.001f, 0.002f, 0.05f).ModelVerified,
                Is.False, "rotation sans vitesse angulaire");

            var traceability = new CampaignTraceability(Admission.Model, CampaignTraceability.RingSeamKeys(Admission.Model));
            var model = Admission.Model;
            var element = model.Corridors[0].CorridorId;
            var realTrack = new ReferenceTrack(new[] { new TrackPiece(RoadElementKind.LaneCorridor, element, 0f, 0f,
                model.Corridors[0].LengthMeters, model.Corridors[0].Samples) });
            traceability.BeginRun(0);
            var unverified = new InterStepResult();
            SetPieces(unverified, new[] { new PieceBound(0, 0.1f, 0.1f, 0.1f, 0.1f) }, false);
            traceability.RecordInterval(realTrack, unverified);
            Assert.That(traceability.Elements[element.ToString()].Status, Is.EqualTo(ElementStatus.NotMeasured),
                "parcouru seulement par des intervalles ModelNotVerified : non mesure");
            var verified = new InterStepResult();
            SetPieces(verified, new[] { new PieceBound(0, 0.1f, 0.12f, 0.1f, 0.1f) }, true);
            traceability.RecordInterval(realTrack, verified);
            Assert.That(traceability.Elements[element.ToString()].Status, Is.EqualTo(ElementStatus.Measured));
            Assert.That(traceability.Elements.Count, Is.EqualTo(72 + 44 + 24 * 2));
            Assert.That(traceability.CountStatus(ElementStatus.NotMeasured), Is.EqualTo(72 + 44 + 48 - 1));
            traceability.MarkNotSelectable(model.Movements[0].Id.ToString());
            Assert.That(traceability.Elements[model.Movements[0].Id.ToString()].Status, Is.EqualTo(ElementStatus.NotSelectable));
        }

        private static void SetPieces(InterStepResult result, PieceBound[] pieces, bool verified)
        {
            typeof(InterStepResult).GetProperty("Pieces").SetValue(result, Array.AsReadOnly(pieces));
            typeof(InterStepResult).GetProperty("ModelVerified").SetValue(result, verified);
        }

        [Test]
        public void BoundDominatesTheDenseMaximumOnACurvedTrack()
        {
            var track = new ReferenceTrack(new[] {
                Straight(0f, new Vector3(0f, 0f, -10f), Vector3.forward, 10f, new RoadId(1, 1)),
                Arc(10f, Vector3.zero, 0f, 6f, 10f, new RoadId(1, 2)) });
            var linear = new Vector3(0.6f, 0f, 7.9f);
            var angular = new Vector3(0f, 1.1f, 0.05f);
            var a = State(new Vector3(0.25f, 0f, -0.08f), Quaternion.Euler(0.5f, 4f, -1f), linear, angular);
            var b = Next(a, linear, angular);
            int piece;
            float sA = track.Project(a.Position, 0, out piece);
            float sB = track.Project(b.Position, piece, out piece);
            var result = TrackingMeasurement.InterStepBound(a, b, Dt, sA, sB, track, Gauge, 0.001f, 0.002f, 0.05f);
            Assert.That(result.ModelVerified, Is.True);
            Assert.That(result.Pieces.Count, Is.EqualTo(2), "le pas traverse le raccord droite -> arc");
            // Maximum dense par force brute sous M, pose nominale appariee (les deux cotes au raccord).
            Vector3 comLocal = Quaternion.Inverse(a.Rotation) * (a.CenterOfMass - a.Position);
            float dense = 0f;
            for (int i = 0; i <= 20000; i++)
            {
                float tau = i / 20000f;
                var rotation = Quaternion.AngleAxis(angular.magnitude * Dt * tau * Mathf.Rad2Deg, angular.normalized) * a.Rotation;
                var position = a.CenterOfMass + linear * (Dt * tau) - rotation * comLocal;
                float sigma = sA + (sB - sA) * tau;
                foreach (var candidate in track.Pieces)
                    if (sigma >= candidate.StartDistanceMeters && sigma <= candidate.EndDistanceMeters)
                        dense = Math.Max(dense, TrackingMeasurement.CornerDisplacement(position, rotation, candidate.Nominal(sigma), Gauge));
            }
            Assert.That(result.BoundMeters, Is.GreaterThanOrEqualTo(dense));
            Assert.That(result.BoundMeters, Is.LessThanOrEqualTo(dense + 0.0011f), "reste de Lipschitz <= 1 mm");
        }

        [Test]
        public void PerfectTrackingAtEightMetresPerSecondStaysFarBelowTheHalfDeltaRule()
        {
            const float radius = 6f;
            var arc = Arc(0f, Vector3.zero, 0f, radius, 20f, new RoadId(1, 1));
            var track = new ReferenceTrack(new[] { arc });
            float s0 = 5f, s1 = 5f + 8f * Dt;
            var n0 = arc.Nominal(s0);
            var n1 = arc.Nominal(s1);
            var r0 = Quaternion.LookRotation(n0.Forward, Vector3.up);
            var r1 = Quaternion.LookRotation(n1.Forward, Vector3.up);
            var angular = new Vector3(0f, 8f / radius, 0f);
            var com0 = n0.Position + r0 * ComLocal;
            var com1 = n1.Position + r1 * ComLocal;
            var linear = (com1 - com0) / Dt;
            var a = new BodyState(n0.Position, r0, com0, linear, angular);
            var b = new BodyState(n1.Position, r1, com1, linear, angular);
            Assert.That(TrackingMeasurement.StepDisplacement(a, track, s0, Gauge), Is.LessThan(1e-4f));
            Assert.That(TrackingMeasurement.StepDisplacement(b, track, s1, Gauge), Is.LessThan(1e-4f));
            var result = TrackingMeasurement.InterStepBound(a, b, Dt, s0, s1, track, Gauge, 0.001f, 0.002f, 0.05f);
            Assert.That(result.ModelVerified, Is.True);
            // Lemme 5.50 : max(d_k, d_k+1) + delta/2, delta = |dp| + rho dtheta = 0,16 + 2,4746 x 0,0267 : 0,113 m.
            float halfDelta = (8f * Dt + Gauge.Rho * (8f / radius) * Dt) * 0.5f;
            Assert.That(halfDelta, Is.EqualTo(0.113f).Within(0.001f));
            Assert.That(result.BoundMeters, Is.LessThanOrEqualTo(halfDelta));
            Assert.That(result.BoundMeters, Is.LessThan(0.01f), "a suivi parfait, borne appariee de l'ordre du millimetre");
        }

        // ------------------------------------------------------------------ H3 et prefab

        [Test]
        public void V2PrefabBoxIsInsideTheGaugeCentredOnTheReferencePointAndCarriesNoV1Type()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(V2PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            var collider = prefab.GetComponent<BoxCollider>();
            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.center.x, Is.EqualTo(0f));
            Assert.That(collider.center.z, Is.EqualTo(0f), "collider centre sur le point de reference");
            var profile = Admission.Model.ValidationProfile;
            var gauge = new GaugeBox(profile.MaxVehicleHalfWidthMeters, profile.MaxVehicleLengthMeters,
                collider.center.y - collider.size.y * 0.5f, collider.center.y + collider.size.y * 0.5f);
            Assert.That(gauge.Contains(collider.center, collider.size, 1e-4f), Is.True, "H3 : la boite du gabarit contient la caisse");
            var body = prefab.GetComponent<VehiclePhysicsBody>();
            Assert.That(body, Is.Not.Null);
            var vehicle = ((VehicleProfileDef)new SerializedObject(body).FindProperty("vehicleProfile").objectReferenceValue).Profile;
            float axleMid = 0f;
            for (int i = 0; i < vehicle.WheelCount; i++) axleMid += vehicle.GetWheel(i).LocalPosition.z;
            Assert.That(axleMid / vehicle.WheelCount, Is.EqualTo(0f).Within(1e-4f), "point de reference a mi-empattement = origine");

            Assert.That(prefab.GetComponent<TrafficV2VehicleDriver>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<TrafficV2VehicleDriver>().DriverProfileDefinition, Is.Not.Null);
            Assert.That(prefab.GetComponent<Unity.Netcode.NetworkObject>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<Unity.Netcode.Components.NetworkTransform>(), Is.Not.Null,
                "les clients recoivent le mouvement par le NetworkTransform seul");
            Assert.That(prefab.GetComponent<NetworkedAIVehicleDriverController>(), Is.Null);
            Assert.That(prefab.GetComponent<NetworkedAIVehicleState>(), Is.Null);
            Assert.That(prefab.GetComponent<AIVehicleBehaviorDebugView>(), Is.Null);
            var v1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            Assert.That(prefab.GetComponent<Unity.Netcode.NetworkObject>().PrefabIdHash,
                Is.Not.EqualTo(v1.GetComponent<Unity.Netcode.NetworkObject>().PrefabIdHash));
            var prefabGuid = AssetDatabase.AssetPathToGUID(V2PrefabPath);
            Assert.That(File.ReadAllText("Assets/DefaultNetworkPrefabs.asset"), Does.Contain(prefabGuid));
        }

        // ------------------------------------------------------------------ gardes structurelles

        private static IEnumerable<string> TrafficSources()
        {
            return Directory.GetFiles(TrafficRoot, "*.cs", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/'));
        }

        /// <summary>Chemin runtime V2 : epine 5.30 et ajouts 5.31 (hors outillage de migration).</summary>
        private static IEnumerable<string> V2RuntimeSources()
        {
            var files = new List<string> { TrafficRoot + "/PlanningSpine.cs" };
            foreach (var folder in new[] { "Frame", "Perception", "Planning", "Debug", "Intent", "Lifecycle" })
                files.AddRange(Directory.GetFiles(TrafficRoot + "/" + folder, "*.cs").Select(p => p.Replace('\\', '/')));
            return files;
        }

        private static string CodeOnly(string source)
        {
            source = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(source, @"//[^\n]*", string.Empty);
        }

        [Test]
        public void OnlyTheComposerBuildsAnIntentAndOnlyTheV2DriverAppliesIt()
        {
            var builders = TrafficSources().Where(p => CodeOnly(File.ReadAllText(p)).Contains("new VehicleDriveIntent(")).ToList();
            Assert.That(builders, Is.EqualTo(new[] { TrafficRoot + "/Intent/VehicleDriveIntentComposer.cs" }));
            var appliers = TrafficSources().Where(p => CodeOnly(File.ReadAllText(p)).Contains("ApplyDriveIntent(")).ToList();
            Assert.That(appliers, Is.EqualTo(new[] { TrafficRoot + "/Lifecycle/TrafficV2VehicleDriver.cs" }));
            var driver = CodeOnly(File.ReadAllText(TrafficRoot + "/Lifecycle/TrafficV2VehicleDriver.cs"));
            Assert.That(Regex.Matches(driver, @"ApplyDriveIntent\(").Count, Is.EqualTo(1), "un seul point d'application");
            Assert.That(Regex.Matches(driver, @"\.Compose\(").Count, Is.EqualTo(1), "un seul composeur, une composition par pas");
            Assert.That(TrafficSources().Count(p => CodeOnly(File.ReadAllText(p)).Contains("class VehicleDriveIntentComposer")), Is.EqualTo(1));
        }

        [Test]
        public void TrafficV2WritesNoBodyPoseOrVelocityAndNeverTeleports()
        {
            var write = new Regex(@"\.(position|rotation|localPosition|localRotation|linearVelocity|angularVelocity|velocity)\s*=(?!=)");
            int scanned = 0;
            foreach (var path in V2RuntimeSources())
            {
                string code = CodeOnly(File.ReadAllText(path));
                scanned++;
                Assert.That(write.IsMatch(code), Is.False, path + " : " + write.Match(code).Value);
                foreach (var forbidden in new[] { "MovePosition", "MoveRotation", "Teleport", "AddForce", "AddTorque",
                    "SetPositionAndRotation", "ResetSuspensionState" })
                    Assert.That(code, Does.Not.Contain(forbidden), path);
            }
            Assert.That(scanned, Is.GreaterThanOrEqualTo(14));
            var spawner = CodeOnly(File.ReadAllText("Assets/RoadRage/App/Run/PortalTrafficSpawner.cs"));
            Assert.That(spawner, Does.Not.Contain("Teleport"));
            Assert.That(spawner, Does.Not.Contain("linearVelocity ="));
        }

        [Test]
        public void MeasurementTokenIsNeverConstructedOutsideTests()
        {
            var offenders = Directory.GetFiles("Assets/RoadRage", "*.cs", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .Where(p => !p.StartsWith("Assets/RoadRage/Tests/", StringComparison.Ordinal))
                .Where(p => Regex.IsMatch(CodeOnly(File.ReadAllText(p)), @"new\s+MeasurementRun\s*\("))
                .ToList();
            Assert.That(offenders, Is.Empty, "le jeton MeasurementRun n'est construit que par les tests de la Story 5.31");
            Assert.That(typeof(MeasurementRun).GetConstructors().Length, Is.EqualTo(1));
        }

        [Test]
        public void V2HostPathAddsNoReplicationOrRpc()
        {
            foreach (var path in TrafficSources().Where(p => p.Contains("/Lifecycle/") || p.Contains("/Intent/")))
            {
                string code = CodeOnly(File.ReadAllText(path));
                Assert.That(code, Does.Not.Contain("NetworkVariable"), path);
                Assert.That(code, Does.Not.Contain("Rpc"), path);
                Assert.That(code, Does.Not.Contain("OnValueChanged"), path);
            }
            var spawner = CodeOnly(File.ReadAllText("Assets/RoadRage/App/Run/PortalTrafficSpawner.cs"));
            Assert.That(spawner, Does.Not.Contain("Rpc"));
            Assert.That(Directory.GetFiles(TrafficRoot, "*.asmdef", SearchOption.AllDirectories), Is.Empty);
        }
    }
}
