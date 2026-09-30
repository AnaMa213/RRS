using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.MainMenu;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.31 : campagnes de mesure dans MVP_Run, hors suite par defaut ([Explicit], categorie
    /// Story531Campaign), lancees en plus de la suite complete et jamais a sa place. Sequence : (1)
    /// campagne exploratoire, qui ne vaut pas acceptation ; (2) declaration d'epsilon_t par le
    /// proprietaire ; (3) campagne d'acceptation. Chaque campagne publie ses rapports bruts separes sous
    /// traffic-v2-5-31-measurements/ : trace par pas, borne entre deux pas sous le modele M (verifie par
    /// pas), tracabilite par element, contacts, couple de repli et conditions.
    /// </summary>
    [Explicit]
    [Category("Story531Campaign")]
    public sealed class Story531MeasurementCampaignPlayModeTests
    {
        private const string Folder = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements";
        private const string CampaignPath = Folder + "/campaign-5-31.json";
        private const string ContactProofPath = "_bmad-output/implementation-artifacts/v1-regression-5-51/final-proof.txt";
        private const int MaxStepsPerVehicle = 9000;
        private const int ContactWindowSteps = 50;

        [Serializable]
        private sealed class ContactSignoff
        {
            public string RoadModelVersion;
            public string ModelHash;
        }

        private sealed class ContactEvidence
        {
            public string PhysicalFingerprint;
            public readonly HashSet<string> Reliefs = new HashSet<string>(StringComparer.Ordinal);
            public readonly HashSet<string> Carriageway = new HashSet<string>(StringComparer.Ordinal);
        }

        [Serializable]
        private sealed class CampaignTripletRecord
        {
            public int Index;
            public string Entry;
            public string Exit;
            public string Via;
            public ulong Seed;
            public ulong InsertionCounter;
            public string TrafficId;
            public string[] Elements;
        }

        [Serializable]
        private sealed class CampaignFile
        {
            public int Format;
            public string RoadModelVersion;
            public int SeedBudget;
            public string Status;
            public int RequiredElements;
            public CampaignTripletRecord[] Triplets;
            public string[] NotSelectable;
            public string ContactPhysicalFingerprint;
            public string SceneDependencyHash;
            public string SceneFileSha256;
        }

        private string originalProfileFilePath;
        private string tempProfileFilePath;

        [SetUp]
        public void SetUp()
        {
            TrafficV2Session.Reset();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TrafficV2Session.Reset();
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                if (manager.IsListening)
                {
                    manager.Shutdown();
                }

                Object.Destroy(manager.gameObject);
            }

            var survivor = RoadRageBootstrap.Instance;
            if (survivor != null)
            {
                Object.Destroy(survivor.gameObject);
            }

            if (originalProfileFilePath != null)
            {
                PlayerProfileFileStore.DefaultFilePath = originalProfileFilePath;
                originalProfileFilePath = null;
            }

            if (!string.IsNullOrEmpty(tempProfileFilePath) && File.Exists(tempProfileFilePath))
            {
                File.Delete(tempProfileFilePath);
            }

            yield return null;
        }

        [UnityTest]
        [Timeout(3600000)]
        public IEnumerator ExploratoryCampaignPublishesRawMeasurements()
        {
            var campaign = LoadCampaign();
            if (campaign == null)
            {
                Assert.Inconclusive("Campagne absente : lancer d'abord Story531DrivenReplayTests (constructeur, EditMode Geometry).");
                yield break;
            }

            var outcome = new CampaignOutcome();
            yield return RunCampaign(campaign, MeasurementKind.Exploratory, outcome);
            if (outcome.Inconclusive != null)
            {
                Assert.Inconclusive(outcome.Inconclusive);
                yield break;
            }

            Debug.Log("[Story531] campagne exploratoire : " + outcome.Summary);
            // Exploratoire : aucune acceptation ; une campagne incomplete reste un echec de mesure.
            Assert.That(outcome.Completed, Is.True, "campagne exploratoire incomplete : " + outcome.Summary);
            Assert.That(outcome.NegativeFallbackTorqueSteps, Is.EqualTo(0), "tout couple negatif pendant un repli est un echec publie : " + outcome.Summary);
        }

        [UnityTest]
        [Timeout(3600000)]
        public IEnumerator AcceptanceCampaignVerifiesTheDeclaredTolerance()
        {
            var declared = TrafficV2Settings.DeclaredTrackingTolerance;
            if (!declared.Declared)
            {
                Assert.Inconclusive("epsilon_t non declare : l'acceptation attend la declaration du proprietaire (etape 2 du protocole).");
                yield break;
            }

            var campaign = LoadCampaign();
            Assert.That(campaign, Is.Not.Null, "campagne absente");
            Assert.That(campaign.Status, Is.EqualTo("Accepted"), "campagne refusee (NotSelectable) : " + string.Join("; ", campaign.NotSelectable));
            var outcome = new CampaignOutcome();
            yield return RunCampaign(campaign, MeasurementKind.Acceptance, outcome);
            Assert.That(outcome.Inconclusive, Is.Null, outcome.Inconclusive);
            Debug.Log("[Story531] campagne d'acceptation : " + outcome.Summary);
            Assert.That(outcome.Completed, Is.True, "chaque vehicule s'insere a une entree et se retire a une sortie : " + outcome.Summary);
            Assert.That(outcome.NonMeasured, Is.Empty, "tout element doit etre Measured : " + string.Join("; ", outcome.NonMeasured));
            Assert.That(outcome.MaxStepDisplacement, Is.LessThanOrEqualTo(declared.Meters), "borne au pas (8 coins) <= epsilon_t");
            Assert.That(outcome.MaxInterStepBound, Is.LessThanOrEqualTo(declared.Meters), "borne entre deux pas (modele M) <= epsilon_t");
            Assert.That(outcome.StepsAboveCeiling, Is.EqualTo(0), "v <= v*(s)");
            Assert.That(outcome.BlockingContacts, Is.Empty, "contact hors chaussee ou consequence d'un contact routier");
            Assert.That(outcome.NegativeFallbackTorqueSteps, Is.EqualTo(0));
            Assert.That(outcome.ModelNotVerifiedIntervals, Is.Zero, "le modele M doit etre verifie a chaque intervalle");
            Assert.That(outcome.NominalPoseInfeasibleSteps, Is.Zero, "aucune route nominale infaisable");
            Assert.That(outcome.PhysicsStepValid, Is.True, "pas physique reel different de 0,02 s");
            Assert.That(outcome.ContactEvidenceValid, Is.True, "preuve physique 5.51 absente ou incompatible");
        }

        [UnityTest]
        [Category("Story531Targeted")]
        [Timeout(3600000)]
        public IEnumerator TargetedExplorationMeasuresMissingMovementsAndRepeatsMaxima()
        {
            var campaign = LoadCampaign();
            Assert.That(campaign, Is.Not.Null);
            var model = TrafficV2Lifecycle.AdmitCommittedArtifacts().Model;
            var missing = new[] {
                "431a11dff650b4115fa5bf99118b9b8a",
                "4aa676c5e3857524d5a9b386be4bdb90",
                "4d54e6de5a6bbf4d1eb20f8ed5a40cb2" };
            var selected = new List<CampaignTripletRecord>();
            foreach (var id in missing)
            {
                RoadId via;
                Assert.That(RoadId.TryParse(id, out via), Is.True);
                var best = DirectTriplet(model, via, selected.Count + 1);
                Assert.That(best, Is.Not.Null, "aucune paire entree/sortie pour " + id);
                selected.Add(best);
            }

            foreach (var index in new[] { 9, 10, 9, 10, 9, 10 })
                selected.Add(campaign.Triplets.Single(t => t.Index == index));
            campaign.Triplets = selected.ToArray();
            var outcome = new CampaignOutcome();
            yield return RunCampaign(campaign, MeasurementKind.Exploratory, outcome);
            Assert.That(outcome.Inconclusive, Is.Null, outcome.Inconclusive);
            Assert.That(outcome.Completed, Is.True, outcome.Summary);
            foreach (var id in missing)
                Assert.That(outcome.NonMeasured, Does.Not.Contain(id + "=NotMeasured"), "mouvement vise non traverse : " + id);
        }

        [UnityTest]
        [Category("Story531Contact")]
        [Timeout(3600000)]
        public IEnumerator TargetedExplorationNamesHistoricalContactCauses()
        {
            var campaign = LoadCampaign();
            Assert.That(campaign, Is.Not.Null);
            // Les sept premieres insertions conservent les identites des runs historiques 3 et 6.
            campaign.Triplets = campaign.Triplets.Take(7).ToArray();
            var outcome = new CampaignOutcome();
            yield return RunCampaign(campaign, MeasurementKind.Exploratory, outcome);
            Assert.That(outcome.Inconclusive, Is.Null, outcome.Inconclusive);
            Assert.That(outcome.Completed, Is.True, outcome.Summary);
            Assert.That(outcome.Contacts.Count, Is.GreaterThanOrEqualTo(4), "contacts historiques non reproduits");
        }

        [UnityTest]
        [Category("Story531Roundabout")]
        [Timeout(3600000)]
        public IEnumerator TargetedRoundaboutPassagesAreReplayedOnTheirHistoricalTriplets()
        {
            // Decision du 2026-09-30 (plafond de la pose nominale, limite de courbe, localisation cinematique) : memes
            // triplets que les maxima reproductibles de exploratory-20260930-081217. Runs 0-3 = triplets 5, 7
            // (entree NW 452ee31e puis 469fe814), 9 (4030253e) et 10 (453f130c), compares avant/apres.
            // Le run 2 passe le tourne-a-droite serre 4dc4e81b (R = 4,21 m) : sa saturation de direction est
            // acceptee en l'etat (choix 1 du 2026-09-30, physique differee), ce test garde le reste de sa conduite.
            var campaign = LoadCampaign();
            Assert.That(campaign, Is.Not.Null);
            campaign.Triplets = new[] { 5, 7, 9, 10 }.Select(i => campaign.Triplets.Single(t => t.Index == i)).ToArray();
            var outcome = new CampaignOutcome();
            yield return RunCampaign(campaign, MeasurementKind.Exploratory, outcome);
            Assert.That(outcome.Inconclusive, Is.Null, outcome.Inconclusive);
            Debug.Log("[Story531] giratoires cibles : " + outcome.Summary);
            Assert.That(outcome.Completed, Is.True, outcome.Summary);
            Assert.That(outcome.StepsAboveCeiling, Is.EqualTo(0), "v <= v*(s) : " + outcome.Summary);
            Assert.That(outcome.NegativeFallbackTorqueSteps, Is.EqualTo(0), outcome.Summary);
            Assert.That(outcome.BlockingContacts, Is.Empty, "contact hors chaussee ou consequence d'un contact routier");
            Assert.That(outcome.ModelNotVerifiedIntervals, Is.Zero, outcome.Summary);
            Assert.That(outcome.NominalPoseInfeasibleSteps, Is.Zero, outcome.Summary);
        }

        [UnityTest]
        [Category("Story531Missing")]
        [Timeout(3600000)]
        public IEnumerator AlternativeEntryMeasuresLastMissingMovement()
        {
            var campaign = LoadCampaign();
            Assert.That(campaign, Is.Not.Null);
            RoadId via, avoidedExit;
            Assert.That(RoadId.TryParse("4aa676c5e3857524d5a9b386be4bdb90", out via), Is.True);
            Assert.That(RoadId.TryParse("4ac98ed2e41d83c91f0714135aa67ba7", out avoidedExit), Is.True);
            var triplet = DirectTriplet(TrafficV2Lifecycle.AdmitCommittedArtifacts().Model, via, 1, true, avoidedExit);
            Assert.That(triplet, Is.Not.Null, "aucun trajet planifie sans la sortie d'anneau qui a precede la derive");
            campaign.Triplets = new[] { triplet, campaign.Triplets.Single(t => t.Index == 9),
                campaign.Triplets.Single(t => t.Index == 10) };
            var outcome = new CampaignOutcome();
            yield return RunCampaign(campaign, MeasurementKind.Exploratory, outcome);
            Assert.That(outcome.Inconclusive, Is.Null, outcome.Inconclusive);
            Assert.That(outcome.Completed, Is.True, outcome.Summary);
            Assert.That(outcome.NonMeasured, Does.Not.Contain(via + "=NotMeasured"), "mouvement non traverse");
        }

        private static CampaignTripletRecord DirectTriplet(CompiledRoadModel model, RoadId via, int insertionCounter,
            bool preferEarlyVia = false, RoadId avoid = default(RoadId))
        {
            CampaignTripletRecord best = null;
            double bestDistance = double.PositiveInfinity;
            var trafficId = TrafficV2Lifecycle.TrafficIdentity(0UL, (ulong)insertionCounter);
            foreach (var entry in model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id))
            {
                Portal portal; Vector3 position; Quaternion rotation;
                if (!TrafficV2Lifecycle.TryPortalPose(model, entry.Id, out portal, out position, out rotation)) continue;
                var location = RoadLocalizer.Localize(model, new VehicleFootprintPose {
                    Position = position, Forward = rotation * Vector3.forward, Up = rotation * Vector3.up
                }, entry.CorridorId, null);
                foreach (var exit in model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id))
                {
                    var plan = RoutePlanner.Plan(new RouteRequest(model, location, exit.Id, new RouteSeed(0UL),
                        trafficId, "route", new DecisionCounter(0), null, false, null, via)).Plan;
                    if (plan == null || plan.Occurrences.Any(o => o.Id == avoid)) continue;
                    double score = preferEarlyVia
                        ? plan.Occurrences.Take(plan.ViaOccurrenceIndex + 1).Sum(o => o.EndSMeters - o.StartSMeters)
                        : plan.DistanceMeters;
                    if (score >= bestDistance) continue;
                    bestDistance = score;
                    best = new CampaignTripletRecord { Index = insertionCounter - 1, Entry = entry.Id.ToString(),
                        Exit = exit.Id.ToString(), Via = via.ToString(), Seed = 0UL };
                }
            }
            return best;
        }

        private sealed class CampaignOutcome
        {
            public string Inconclusive;
            public bool Completed;
            public string Summary;
            public string StallReason;
            public float MaxStepDisplacement;
            public float MaxInterStepBound;
            public int StepsAboveCeiling;
            public int NegativeFallbackTorqueSteps;
            public List<string> Contacts = new List<string>();
            public List<string> BlockingContacts = new List<string>();
            public int NominalPoseInfeasibleSteps;
            public int ModelNotVerifiedIntervals;
            public bool PhysicsStepValid = true;
            public bool ContactEvidenceValid;
            public List<string> NonMeasured = new List<string>();
        }

        private static CampaignFile LoadCampaign()
        {
            return File.Exists(CampaignPath) ? JsonUtility.FromJson<CampaignFile>(File.ReadAllText(CampaignPath)) : null;
        }

        private IEnumerator RunCampaign(CampaignFile campaign, MeasurementKind kind, CampaignOutcome outcome)
        {
            Assert.That(campaign, Is.Not.Null, "fichier campagne present et lisible");
            Assert.That(campaign.Triplets, Is.Not.Null.And.Not.Empty, "triplets de campagne declares");
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Code, Is.EqualTo(TrafficV2Code.Allowed));
            Assert.That(campaign.RoadModelVersion, Is.EqualTo(admission.Model.Version.ToString()), "campagne construite sur le modele signe");
            var triplets = campaign.Triplets.Select(t =>
            {
                Assert.That(t, Is.Not.Null, "triplet de campagne non nul");
                RoadId entry, exit, via;
                Assert.That(RoadId.TryParse(t.Entry, out entry), Is.True, "ID de portail d'entree invalide : " + t.Entry);
                Assert.That(RoadId.TryParse(t.Exit, out exit), Is.True, "ID de portail de sortie invalide : " + t.Exit);
                if (string.IsNullOrEmpty(t.Via)) via = RoadId.None;
                else Assert.That(RoadId.TryParse(t.Via, out via), Is.True, "ID de mouvement via invalide : " + t.Via);
                return new CampaignTriplet(entry, exit, t.Seed, via);
            }).ToArray();
            string label = kind.ToString().ToLowerInvariant() + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            TrafficV2Session.Request(TrafficComposition.V2Slice, new MeasurementRun(kind, label, triplets));
            yield return EnterMvpRun(outcome);
            if (outcome.Inconclusive != null)
            {
                yield break;
            }

            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            var aliveSteps = 0;
            var idleSteps = 0;
            var stalled = false;
            var totalSteps = 0;
            TrafficV2VehicleDriver lastLive = null;
            while (!(spawner.V2LastCode == TrafficV2Code.CampaignCompleted && spawner.LiveV2Population == 0))
            {
                yield return new WaitForFixedUpdate();
                totalSteps++;
                Assert.That(spawner.LiveV2Population, Is.LessThanOrEqualTo(TrafficV2Settings.V2SliceMaxPopulation));
                if (spawner.LiveV2Population == 1)
                {
                    idleSteps = 0;
                    var live = spawner.LiveV2Vehicles[0].GetComponent<TrafficV2VehicleDriver>();
                    aliveSteps = live == lastLive ? aliveSteps + 1 : 1;
                    lastLive = live;
                    if (aliveSteps > MaxStepsPerVehicle)
                    {
                        stalled = true;
                        break;
                    }
                }
                else if (++idleSteps > MaxStepsPerVehicle)
                {
                    stalled = true;
                    break;
                }
            }

            var drivers = new List<V2DriveRecord>(spawner.RetiredV2Runs);
            if (stalled && lastLive != null)
            {
                drivers.Add(lastLive.CaptureRecord());
            }

            outcome.Completed = !stalled && spawner.V2Removals == triplets.Length && spawner.V2Insertions == triplets.Length;
            if (stalled) outcome.StallReason = spawner.V2LastCode.ToString();
            PostProcess(admission, campaign, kind, label, drivers, stalled, totalSteps, outcome);
        }

        private static void PostProcess(TrafficV2Admission admission, CampaignFile campaign, MeasurementKind kind, string label,
            List<V2DriveRecord> drivers, bool stalled, int totalSteps, CampaignOutcome outcome)
        {
            var model = admission.Model;
            var contactEvidence = LoadContactEvidence(model, campaign);
            outcome.ContactEvidenceValid = contactEvidence != null;
            var traceability = new CampaignTraceability(model, CampaignTraceability.RingSeamKeys(model));
            foreach (var key in campaign.NotSelectable)
            {
                if (key.StartsWith("s:", StringComparison.Ordinal))
                {
                    traceability.MarkNotSelectable(key.Substring(2) + ":left");
                    traceability.MarkNotSelectable(key.Substring(2) + ":right");
                }
                else
                {
                    traceability.MarkNotSelectable(key.Substring(2));
                }
            }

            float dt = Time.fixedDeltaTime;
            outcome.PhysicsStepValid = Mathf.Abs(dt - 0.02f) < 1e-6f;
            var steps = new StringBuilder("run\tstep\ttrack\telement\ts_route_m\td_step_m\tv\tvstar\tlateral_m\theading_deg\tfallback\treason\tterminal\tmin_drive_torque\tbinding\tinterstep_bound_m\tmodel_verified\tpos_residual_m\trot_residual_deg\tnominal_offset_deg\tnominal_steer_deg\tnominal_feasible\tgrounded_wheels\treference_error_m\treference_lateral_m\treference_longitudinal_m\theading_nominal_deg\trotation_at_worst_corner_m\tlocation_flags\tlimiting\ttarget_speed\tcmd_wheel_deg\tapplied_wheel_deg\tthrottle\tbrake\tsteer_input\toutside_width\n");
            int modelNotVerified = 0, intervals = 0;
            var runs = new StringBuilder();
            for (int run = 0; run < drivers.Count; run++)
            {
                var driver = drivers[run];
                outcome.PhysicsStepValid &= Mathf.Abs(driver.FixedDeltaTimeSeconds - 0.02f) < 1e-6f;
                traceability.BeginRun(run);
                var trace = driver.Trace;
                for (int i = 0; i < trace.Count; i++)
                {
                    var record = trace[i];
                    var nominal = driver.Tracks[record.TrackIndex].Nominal(record.Piece, record.RouteDistanceMeters);
                    float referenceError, referenceLateral, referenceLongitudinal, rotationAtWorstCorner;
                    DecomposeStep(record, nominal, driver.Gauge, out referenceError, out referenceLateral,
                        out referenceLongitudinal, out rotationAtWorstCorner);
                    float headingNominal = Vector3.SignedAngle(nominal.Forward,
                        Vector3.ProjectOnPlane(record.State.Rotation * Vector3.forward, nominal.Up), nominal.Up);
                    traceability.RecordStep(record.ElementId, record.StepDisplacementMeters, record.SpeedRatio);
                    outcome.MaxStepDisplacement = Math.Max(outcome.MaxStepDisplacement, record.StepDisplacementMeters);
                    if (!float.IsNaN(record.SpeedRatio) && record.SpeedRatio > 1f)
                    {
                        outcome.StepsAboveCeiling++;
                    }

                    if (record.Fallback && record.MinimumDriveTorque < 0f)
                    {
                        outcome.NegativeFallbackTorqueSteps++;
                    }

                    string bound = "", verified = "", pos = "", rot = "";
                    if (i + 1 < trace.Count && trace[i + 1].Step == record.Step + 1)
                    {
                        var next = trace[i + 1];
                        var track = driver.Tracks[record.TrackIndex];
                        // Le changement de reference se decide au pas suivant. L'intervalle physique
                        // precedent appartient encore a l'ancienne reference.
                        int nextPiece;
                        float nextDistance = next.TrackIndex == record.TrackIndex ? next.RouteDistanceMeters
                            : track.Project(next.State.Position, record.Piece, out nextPiece);
                        var result = TrackingMeasurement.InterStepBound(record.State, next.State, dt, record.RouteDistanceMeters,
                            nextDistance, track, driver.Gauge, TrafficV2Settings.InterStepRemainderMeters,
                            TrafficV2Settings.ModelPositionToleranceMeters, TrafficV2Settings.ModelRotationToleranceDegrees);
                        traceability.RecordInterval(track, result, record.SpeedRatio, next.SpeedRatio);
                        intervals++;
                        if (!result.ModelVerified)
                        {
                            modelNotVerified++;
                        }
                        else
                        {
                            outcome.MaxInterStepBound = Math.Max(outcome.MaxInterStepBound, result.BoundMeters);
                        }

                        bound = F(result.BoundMeters);
                        verified = result.ModelVerified ? "1" : "0";
                        pos = F(result.PositionResidualMeters);
                        rot = F(result.RotationResidualDegrees);
                    }

                    steps.Append(run).Append('\t').Append(record.Step).Append('\t').Append(record.TrackIndex).Append('\t')
                        .Append(record.ElementId).Append('\t').Append(F(record.RouteDistanceMeters)).Append('\t')
                        .Append(F(record.StepDisplacementMeters)).Append('\t').Append(F(record.LongitudinalSpeed)).Append('\t')
                        .Append(record.CeilingUnbounded ? "inf" : F(record.CeilingMetersPerSecond)).Append('\t')
                        .Append(F(record.LateralErrorMeters)).Append('\t').Append(F(record.HeadingErrorDegrees)).Append('\t')
                        .Append(record.Fallback ? "1" : "0").Append('\t').Append(record.Reason).Append('\t').Append(record.Terminal).Append('\t')
                        .Append(F(record.MinimumDriveTorque)).Append('\t').Append(record.Binding).Append('\t').Append(bound).Append('\t')
                        .Append(verified).Append('\t').Append(pos).Append('\t').Append(rot).Append('\t')
                        .Append(F(record.NominalOffsetDegrees)).Append('\t').Append(F(record.NominalSteerDegrees)).Append('\t')
                        .Append(record.NominalFeasible ? "1" : "0").Append('\t').Append(record.GroundedWheels).Append('\t')
                        .Append(F(referenceError)).Append('\t').Append(F(referenceLateral)).Append('\t')
                        .Append(F(referenceLongitudinal)).Append('\t').Append(F(headingNominal)).Append('\t')
                        .Append(F(rotationAtWorstCorner)).Append('\t').Append(record.LocationFlags).Append('\t')
                        .Append(record.Limiting).Append('\t').Append(F(record.TargetSpeedMetersPerSecond)).Append('\t')
                        .Append(F(record.CommandedWheelAngleDegrees)).Append('\t').Append(F(record.AppliedWheelAngleDegrees)).Append('\t')
                        .Append(F(record.Intent.Throttle)).Append('\t').Append(F(record.Intent.BrakeReverse)).Append('\t')
                        .Append(F(record.Intent.Steer)).Append('\t')
                        .Append(record.OutsideWidthEnvelope ? '1' : '0').Append('\n');
                }

                foreach (var contact in driver.ContactEpisodes)
                    RecordContact(run, driver, contact, contactEvidence, outcome);
                outcome.NominalPoseInfeasibleSteps += driver.NominalPoseInfeasibleSteps;
                runs.Append("| ").Append(run).Append(" | ").Append(driver.HasReachedExitPortal ? "sortie" : "en route").Append(" | ")
                    .Append(trace.Count).Append(" | ").Append(driver.ReplanCount).Append(" | ").Append(trace.Count(r => r.Fallback))
                    .Append(" | ").Append(F(driver.MaxStepDisplacementMeters)).Append(" | ").Append(driver.Timings).Append(" |\n");
            }
            outcome.ModelNotVerifiedIntervals = modelNotVerified;

            foreach (var element in traceability.Elements.Values.Where(e => e.Status != ElementStatus.Measured))
            {
                outcome.NonMeasured.Add(element.Key + "=" + element.Status);
            }

            var declared = TrafficV2Settings.DeclaredTrackingTolerance;
            Directory.CreateDirectory(Folder);
            string prefix = Folder + "/" + label;
            File.WriteAllText(prefix + "-steps.tsv", steps.ToString());
            File.WriteAllText(prefix + "-elements.tsv", traceability.ToReport(declared.Declared ? declared.Meters : (float?)null));
            outcome.Summary = string.Format(CultureInfo.InvariantCulture,
                "{0} vehicule(s), campagne complete {1}, cale {2}, pas totaux {3}, d max au pas {4:0.####} m, borne entre deux pas max (M verifie) {5:0.####} m, "
                + "intervalles {6} dont ModelNotVerified {7}, pas v > v* {8}, couple negatif en repli {9}, contacts {10}, elements Measured {11} / NotMeasured {12} / NotSelectable {13}, "
                + "NominalPoseInfeasible {14} pas (pose nominale cinematique, contrat §8), dernier code {15}",
                drivers.Count, outcome.Completed, stalled, totalSteps, outcome.MaxStepDisplacement, outcome.MaxInterStepBound, intervals,
                modelNotVerified, outcome.StepsAboveCeiling, outcome.NegativeFallbackTorqueSteps, outcome.Contacts.Count,
                traceability.CountStatus(ElementStatus.Measured), traceability.CountStatus(ElementStatus.NotMeasured),
                traceability.CountStatus(ElementStatus.NotSelectable), outcome.NominalPoseInfeasibleSteps,
                outcome.StallReason ?? "aucun");
            var summary = new StringBuilder();
            summary.Append("# Story 5.31 - campagne ").Append(kind).Append(" ").Append(label).Append("\n\n");
            summary.Append(kind == MeasurementKind.Exploratory ? "Campagne exploratoire : ne vaut pas acceptation.\n\n" : "Campagne d'acceptation.\n\n");
            summary.Append("## Conditions\n\n");
            summary.Append("- pas physique : ").Append(F(dt)).Append(" s ; Editeur en hote ; profils par defaut du prefab V2\n");
            summary.Append("- commit : ").Append(Commit()).Append("\n");
            summary.Append("- RoadModelVersion : ").Append(model.Version).Append(" ; budget de graines : ").Append(campaign.SeedBudget)
                .Append(" ; statut de campagne : ").Append(campaign.Status).Append("\n");
            summary.Append("- epsilon_t : ").Append(declared.Declared ? F(declared.Meters) + " m (declare)" : "non declare").Append("\n");
            // Verdict de couverture publie (AC 5.31) : max|o| + epsilon_t <= a_e d'une preuve a pose cinematique ; meme appel
            // que l'insertion (o = 0, la reference est la trajectoire suivie).
            summary.Append("- verdict de couverture : ").Append(MotionPlan.EvaluateVehicleCoverage(admission.Evidence, 0f, declared))
                .Append(" (max|o| = 0 m ; a_e = ").Append(F(admission.Evidence.TrackingAllowanceMeters))
                .Append(" m ; modele de pose de la preuve : ").Append(admission.Evidence.PoseModel).Append(")\n");
            summary.Append("- modele M : tolerances ").Append(F(TrafficV2Settings.ModelPositionToleranceMeters)).Append(" m / ")
                .Append(F(TrafficV2Settings.ModelRotationToleranceDegrees)).Append(" deg ; reste de Lipschitz vise ")
                .Append(F(TrafficV2Settings.InterStepRemainderMeters)).Append(" m\n\n");
            summary.Append("- preuve relief 5.51 : empreinte physique ")
                .Append(contactEvidence == null ? "absente ou incompatible" : contactEvidence.PhysicalFingerprint)
                .Append(" ; chaussées identifiées ")
                .Append(contactEvidence == null ? 0 : contactEvidence.Carriageway.Count).Append(" ; fenetre contact ±")
                .Append(ContactWindowSteps).Append(" pas = ").Append(F(ContactWindowSteps * dt)).Append(" s\n\n");
            summary.Append("## Resultat\n\n").Append(outcome.Summary).Append("\n\n");
            summary.Append("## Runs\n\n| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |\n|---|---|---|---|---|---|---|\n")
                .Append(runs).Append('\n');
            summary.Append("## Triplets\n\n");
            for (int i = 0; i < campaign.Triplets.Length; i++)
                summary.Append("- run ").Append(i).Append(" : ").Append(campaign.Triplets[i].Entry)
                    .Append(" -> ").Append(campaign.Triplets[i].Exit).Append(" ; via ")
                    .Append(campaign.Triplets[i].Via).Append(" ; graine ").Append(campaign.Triplets[i].Seed).Append('\n');
            summary.Append('\n');
            summary.Append("## Contacts\n\n").Append(outcome.Contacts.Count == 0 ? "aucun" : string.Join("\n", outcome.Contacts)).Append("\n\n");
            summary.Append("## Contacts bloquants ou consequences\n\n")
                .Append(outcome.BlockingContacts.Count == 0 ? "aucun" : string.Join("\n", outcome.BlockingContacts)).Append("\n\n");
            summary.Append("## NotSelectable (constructeur)\n\n").Append(campaign.NotSelectable.Length == 0 ? "aucun" : string.Join("\n", campaign.NotSelectable)).Append('\n');
            File.WriteAllText(prefix + "-summary.md", summary.ToString());
        }

        private static void DecomposeStep(V2StepRecord record, NominalPose nominal, GaugeBox gauge,
            out float referenceError, out float referenceLateral, out float referenceLongitudinal,
            out float rotationAtWorstCorner)
        {
            Vector3 up = nominal.Up.normalized;
            Vector3 translation = Vector3.ProjectOnPlane(record.State.Position - nominal.Position, up);
            Quaternion nominalRotation = Quaternion.LookRotation(nominal.Forward, up);
            referenceError = translation.magnitude;
            referenceLateral = Vector3.Dot(translation, Vector3.Cross(up, nominal.Forward));
            referenceLongitudinal = Vector3.Dot(translation, nominal.Forward);
            rotationAtWorstCorner = 0f;
            float worst = -1f;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = gauge.Corner(i);
                Vector3 rotation = Vector3.ProjectOnPlane(record.State.Rotation * corner - nominalRotation * corner, up);
                float distance = (translation + rotation).magnitude;
                if (distance <= worst) continue;
                worst = distance;
                rotationAtWorstCorner = rotation.magnitude;
            }
        }

        private static ContactEvidence LoadContactEvidence(CompiledRoadModel model, CampaignFile campaign)
        {
#if !UNITY_EDITOR
            return null;
#else
            string modelVersion = model.Version.ToString();
            if (!File.Exists(ContactProofPath) || !File.Exists(TrafficV2Settings.SignoffPath)
                || !File.Exists(TrafficV2Settings.ReportPath)) return null;
            const string scenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
            if (campaign == null || string.IsNullOrEmpty(campaign.SceneDependencyHash)
                || string.IsNullOrEmpty(campaign.SceneFileSha256) || !File.Exists(scenePath)) return null;
            if (campaign.SceneDependencyHash != AssetDatabase.GetAssetDependencyHash(scenePath).ToString()) return null;
            using (var sha = SHA256.Create())
                if (campaign.SceneFileSha256 != BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(scenePath)))
                    .Replace("-", "").ToLowerInvariant()) return null;
            string[] lines = File.ReadAllLines(ContactProofPath);
            var signoff = JsonUtility.FromJson<ContactSignoff>(File.ReadAllText(TrafficV2Settings.SignoffPath));
            if (signoff == null || signoff.RoadModelVersion != modelVersion
                || Value(lines, "road-model-version=") != modelVersion
                || Value(lines, "model-hash=") != signoff.ModelHash
                || !lines.Any(line => line.Contains(" passed=True"))) return null;

            string fingerprint = Value(lines, "empreinte-physique=");
            if (fingerprint == null || fingerprint.Length != 64 || !fingerprint.All(Uri.IsHexDigit)) return null;
            if (fingerprint != campaign.ContactPhysicalFingerprint) return null;
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "MVP_Run") return null;
            string signedReport = File.ReadAllText(TrafficV2Settings.ReportPath);
            if (!signedReport.Contains("| Entrees physiques des carrefours classiques (5.51) | `"
                + fingerprint + "` |")) return null;
            var evidence = new ContactEvidence { PhysicalFingerprint = fingerprint };
            foreach (string line in lines.Where(line => line.StartsWith("relief: ", StringComparison.Ordinal)))
            {
                int height = line.IndexOf(" hauteur=", StringComparison.Ordinal);
                if (height < 0) return null;
                evidence.Reliefs.Add(line.Substring(8, height - 8));
            }

            // Chaussées réellement chargées dans MVP_Run : l'ensemble exact des colliders de route authores,
            // sous RunRoot/LaneGraph/.../Collision. Aucun autre collider ne devient routier par son seul nom.
            foreach (var root in scene.GetRootGameObjects())
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                {
                    string name = collider.name;
                    if (name != "Col_Roadway" && name != "Col_Roadway_Ring" && name != "Col_Roadway_Disc") continue;
                    string path = HierarchyPath(collider.transform);
                    if (path.StartsWith("MVP_Run/RunRoot/LaneGraph/", StringComparison.Ordinal)
                        && path.Contains("/Collision/")) evidence.Carriageway.Add(path);
                }
            return evidence.Carriageway.Count == 0 ? null : evidence;
#endif
        }

        private static string Value(IEnumerable<string> lines, string prefix)
        {
            string line = lines.FirstOrDefault(item => item.StartsWith(prefix, StringComparison.Ordinal));
            return line == null ? null : line.Substring(prefix.Length);
        }

        private static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
                path = parent.name + "/" + path;
            return transform.gameObject.scene.name + "/" + path;
        }

        private static void RecordContact(int run, V2DriveRecord driver, V2ContactEpisode contact,
            ContactEvidence evidence, CampaignOutcome outcome)
        {
            string classification = evidence != null && evidence.Reliefs.Contains(contact.ColliderPath) ? "DrivableRelief"
                : evidence != null && evidence.Carriageway.Contains(contact.ColliderPath) ? "Carriageway" : "Other";
            var trace = driver.Trace;
            var window = trace.Where(r => r.Step + ContactWindowSteps >= contact.FirstStep
                && r.Step <= contact.LastStep + ContactWindowSteps).ToArray();
            // Criteres du premier contact a la fin du run (contrat §8) ; la fenetre de +-50 pas ne sert qu'a la publication.
            // Sortie de route = enveloppe de largeur ; le depassement longitudinal a une couture ou au portail de sortie
            // n'en est pas une, et le repli terminal ExitPortalReached n'est pas une perte de controle (2026-09-30).
            var after = trace.Where(r => r.Step >= contact.FirstStep).ToArray();
            float? tolerance = TrafficV2Settings.DeclaredTrackingTolerance.Declared
                ? TrafficV2Settings.DeclaredTrackingTolerance.Meters : (float?)null;
            var consequences = new List<string>();
            var boundsCauses = new List<string>();
            if (after.Any(r => r.OutsideWidthEnvelope)) boundsCauses.Add("OutsideEnvelope");
            if (after.Any(r => (r.LocationFlags & RoadLocationFlags.WrongWay) != 0)) boundsCauses.Add("WrongWay");
            if (tolerance.HasValue && after.Any(r => r.StepDisplacementMeters > tolerance.Value))
                boundsCauses.Add("EpsilonExceeded");
            if (boundsCauses.Count > 0) consequences.Add("OutOfBounds(" + string.Join(",", boundsCauses) + ")");
            if (after.Any(r => r.SpeedRatio > 1f || (r.Fallback && r.Reason != V2FallbackReason.ExitPortalReached)))
                consequences.Add("LossOfControl");
            int stopped = 0;
            float minimumDirectionSpeed = driver.MinimumDirectionSpeed;
            foreach (var record in after)
            {
                stopped = Math.Abs(record.LongitudinalSpeed) <= VehicleDriveIntentComposer.FallbackStoppedSpeedMetersPerSecond
                    && record.TargetSpeedMetersPerSecond > minimumDirectionSpeed ? stopped + 1 : 0;
                if (stopped >= VehicleDriveIntentComposer.FallbackStoppedSteps) { consequences.Add("AbnormalStop"); break; }
            }
            if (!driver.HasReachedExitPortal) consequences.Add("FailedContinuation");
            if (classification == "Other") consequences.Add("OtherCollider");

            float before = window.Length == 0 ? contact.SpeedAtFirstContact : window[0].LongitudinalSpeed;
            float afterSpeed = window.Length == 0 ? contact.MinimumSpeedDuringContact : window[window.Length - 1].LongitudinalSpeed;
            float maxD = window.Length == 0 ? 0f : window.Max(r => r.StepDisplacementMeters);
            float maxHeading = window.Length == 0 ? 0f : window.Max(r =>
            {
                var nominal = driver.Tracks[r.TrackIndex].Nominal(r.Piece, r.RouteDistanceMeters);
                return Math.Abs(Vector3.SignedAngle(nominal.Forward, r.State.Rotation * Vector3.forward, nominal.Up));
            });
            float maxRoll = window.Length == 0 ? 0f : window.Max(r =>
            {
                var nominal = driver.Tracks[r.TrackIndex].Nominal(r.Piece, r.RouteDistanceMeters);
                return Math.Abs(Vector3.SignedAngle(nominal.Up, r.State.Rotation * Vector3.up, nominal.Forward));
            });
            float maxPitch = window.Length == 0 ? 0f : window.Max(r =>
            {
                var nominal = driver.Tracks[r.TrackIndex].Nominal(r.Piece, r.RouteDistanceMeters);
                return Math.Abs(Vector3.SignedAngle(nominal.Up, r.State.Rotation * Vector3.up,
                    Vector3.Cross(nominal.Up, nominal.Forward)));
            });
            int minGrounded = window.Length == 0 ? 0 : window.Min(r => r.GroundedWheels);
            string recordText = string.Format(CultureInfo.InvariantCulture,
                "run {0}: {1} [{2}] pas {3}-{4}, impulsion {5:0.###} N.s, v_n {6:0.###} m/s, "
                + "v avant/min/apres {7:0.###}/{8:0.###}/{9:0.###} m/s, d max {10:0.####} m, cap max {11:0.###} deg, "
                + "roulis/tangage max {12:0.###}/{13:0.###} deg, roues au sol min {14}, sortie {15}, criteres {16}",
                run, contact.ColliderPath, classification, contact.FirstStep, contact.LastStep,
                contact.MaxImpulseNewtonSeconds, contact.MaxRelativeNormalSpeed, before,
                contact.MinimumSpeedDuringContact, afterSpeed, maxD, maxHeading, maxRoll, maxPitch,
                minGrounded, driver.HasReachedExitPortal, consequences.Count == 0 ? "aucun" : string.Join(",", consequences));
            outcome.Contacts.Add(recordText);
            if (consequences.Count > 0) outcome.BlockingContacts.Add(recordText);
        }

        private static string Commit()
        {
            try
            {
                var head = File.ReadAllText(".git/HEAD").Trim();
                if (!head.StartsWith("ref: ", StringComparison.Ordinal))
                {
                    return head;
                }

                var reference = head.Substring(5);
                return File.Exists(".git/" + reference) ? File.ReadAllText(".git/" + reference).Trim() + " (" + reference + ")" : reference;
            }
            catch (IOException)
            {
                return "inconnu";
            }
        }

        private static string F(float value)
        {
            return float.IsNaN(value) ? "nan" : value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private IEnumerator EnterMvpRun(CampaignOutcome outcome)
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story531c-" + Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;
            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;
            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null);
            Click(menuScreen, "playButton");
            yield return null;
            var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                Click(lobbyScreen, "startGameButton");
                var frames = 0;
                while (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frames < 300)
                {
                    var bootstrap = RoadRageBootstrap.Instance;
                    if (bootstrap != null && bootstrap.LobbyRoom != null && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                    {
                        outcome.Inconclusive = "Services en ligne non disponibles.";
                        yield break;
                    }

                    yield return null;
                    frames++;
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));
        }

        private static void Click(Component screen, string field)
        {
            var info = screen.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, field);
            ((Button)info.GetValue(screen)).onClick.Invoke();
        }
    }
}
