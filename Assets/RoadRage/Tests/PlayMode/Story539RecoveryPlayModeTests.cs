using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Collisions;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Recovery;
using RoadRage.Features.Vehicles.Traffic.Tactical;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.39 dans MVP_Run, sur le vehicule V2 de production et le pilote reel : une impulsion physique pousse le vehicule
    /// lateralement, le fait de contact du meme pas est verse a l'accumulateur (l'extraction PhysX reste a la Gate D). Le but
    /// de collision freine, le vehicule stable mais deplace attend la recuperation, un realignement le ramene dans
    /// l'enveloppe sans teleportation, puis il sort normalement par son portail.
    /// </summary>
    [Category("Story539")]
    public sealed class Story539RecoveryPlayModeTests
    {
        private const int MaxFixedSteps = 9000;
        private Story533Harness harness;

        [SetUp]
        public void SetUp()
        {
            TrafficV2Session.Reset();
            harness = new Story533Harness();
        }

        [UnityTearDown]
        public IEnumerator TearDown() { yield return harness.Cleanup(); }

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator ADisplacedVehicleRealignsWithoutTeleportAndReachesItsExit()
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile scenarioFile;
            Story533Harness.Load(admission, "B", out scenarioFile);
            TrafficV2Session.Request(TrafficComposition.V2Slice, null);
            yield return harness.EnterMvpRun();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MVP_Run"));
            yield return harness.ParkHostPlayer(scenarioFile.Parking.Value, _ => { });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);

            // Rouler d'abord sur un corridor, en conduite nominale, a vitesse etablie.
            TrafficV2VehicleDriver driver = null;
            Rigidbody body = null;
            for (int i = 0; i < MaxFixedSteps; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(spawner.V2Removals, Is.Zero, "le vehicule doit etre pousse avant sa sortie");
                driver = Object.FindObjectsByType<TrafficV2VehicleDriver>().FirstOrDefault(d => d.IsBound && d.IntentsApplied > 0);
                if (driver == null) continue;
                body = driver.GetComponent<Rigidbody>();
                if (driver.IntentsApplied > 250 && driver.LastProjection != null && !driver.Tactical.Active
                    && driver.LastProjection.ElementKind == RoadElementKind.LaneCorridor && body.linearVelocity.magnitude > 3f) break;
            }
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver.MeasurementLabel, Is.Null, "hors run de mesure");
            Assert.That(driver.Recovery.Attempts, Is.Empty);

            // Poussee physique laterale (6 m/s) et fait de contact du pas : impact a gauche, vitesse relative 6 m/s.
            body.AddForce(driver.transform.right * body.mass * 6f, ForceMode.Impulse);
            var accumulator = (StepContactAccumulator)typeof(TrafficV2VehicleDriver)
                .GetField("contactsOfStep", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(driver);
            accumulator.Add(body.mass * 6f, 6f, -1f);

            var recovery = driver.Recovery;
            bool collisionAccepted = false, awaited = false, realigning = false;
            float maxDisplacement = 0f;
            for (int i = 0; i < MaxFixedSteps && spawner.V2Removals == 0; i++)
            {
                Vector3 before = body.position;
                float speedBefore = body.linearVelocity.magnitude;
                yield return new WaitForFixedUpdate();
                if (spawner.V2Removals > 0 || driver == null || body == null) break;
                Assert.That(Vector3.Distance(before, body.position),
                    Is.LessThanOrEqualTo(Mathf.Max(speedBefore, body.linearVelocity.magnitude) * Time.fixedDeltaTime + 0.01f),
                    "aucune teleportation au pas " + i);
                Assert.That(driver.Lifecycle, Is.EqualTo(TrafficV2LifecycleState.Active), recovery.ToText());
                Assert.That(body.isKinematic, Is.False);
                Assert.That(body.constraints, Is.EqualTo(RigidbodyConstraints.None));
                collisionAccepted |= driver.LastTacticalResponse.HasValue && driver.LastTacticalResponse.Value.Accepted;
                awaited |= driver.Tactical.AwaitingRecovery;
                realigning |= driver.Tactical.RecoveryActive && driver.Tactical.Maneuver == RecoveryManeuver.Realign;
                maxDisplacement = Mathf.Max(maxDisplacement, driver.LastCollisionFacts.DisplacementMeters);
                if (driver.Tactical.RecoveryActive)
                    StringAssert.Contains("\nRecovery ", driver.LastProjection.ToText(), "etat de recuperation publie");
            }

            string history = recovery.ToText() + " | " + string.Join(" ; ", recovery.Attempts.Select(a => a.ToText()));
            Assert.That(collisionAccepted, Is.True, "but de collision accepte au pas de la poussee");
            Assert.That(maxDisplacement, Is.GreaterThan(TrafficV2Settings.DeclaredTrackingTolerance.Meters), "vehicule deplace hors de epsilon_t");
            Assert.That(awaited, Is.True, "stable mais deplace : AwaitingRecovery");
            Assert.That(realigning, Is.True, history);
            var first = recovery.Attempts.First();
            Assert.That(first.Request.Cause, Is.EqualTo(RecoveryCause.Displaced), history);
            Assert.That(first.Request.Maneuver, Is.EqualTo(RecoveryManeuver.Realign), history);
            Assert.That(first.Response, Is.EqualTo(TacticalReason.Accepted), history);
            Assert.That(recovery.Attempts.Any(a => a.Outcome == TacticalReason.Resumed), Is.True, "rattache dans l'enveloppe : " + history);
            Assert.That(recovery.Faulted, Is.False, history);
            Assert.That(spawner.V2Removals, Is.EqualTo(1), "sortie normale au portail apres la recuperation : " + history);
        }

        /// <summary>
        /// Revue 5.39 (preuve au niveau du pilote) : pas injectes a la frontiere Prepare/Step, patron 5.38, sur le pilote reel
        /// d'un V2 de MVP_Run. Une attente derriere un leader legitime ne produit aucune requete ; un vehicule cale sans
        /// blocker legitime devient ProgressDeficit et recule par le pilote (gaz nul, BrakeReverse) ; les tentatives epuisees
        /// donnent Faulted : repli Faulted a chaque pas, ni planification, ni requete, vehicule present.
        /// </summary>
        [UnityTest]
        [Timeout(300000)]
        public IEnumerator TheLiveDriverSuppressesLegitimateWaitsReversesWhenStalledAndFaultsWhenExhausted()
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile scenarioFile;
            Story533Harness.Load(admission, "B", out scenarioFile);
            TrafficV2Session.Request(TrafficComposition.V2Slice, null);
            yield return harness.EnterMvpRun();
            yield return harness.ParkHostPlayer(scenarioFile.Parking.Value, _ => { });
            TrafficV2VehicleDriver driver = null;
            for (int i = 0; i < MaxFixedSteps; i++)
            {
                yield return new WaitForFixedUpdate();
                driver = Object.FindObjectsByType<TrafficV2VehicleDriver>().FirstOrDefault(d => d.IsBound && d.IntentsApplied > 0);
                if (driver != null && driver.IntentsApplied > 250 && driver.LastProjection != null
                    && driver.LastProjection.ElementKind == RoadElementKind.LaneCorridor && !driver.Tactical.Active) break;
            }
            Assert.That(driver, Is.Not.Null);
            // Aucun autre FixedUpdate pendant les pas injectes : le spawner reprend apres ce bloc.
            var state = Read<BodyState>(driver, "preparedState");
            var track = driver.Tracks.Last();
            int piece;
            float distance = track.Project(state.Position, 0, out piece);
            ulong frameId = driver.LastFrameId;

            // 1. Attente legitime : leader arrete a s0 devant, 60 s. Jamais de requete.
            var footprint = driver.Footprint;
            float gap = driver.DriverProfileDefinition.Profile.MinimumGap;
            float ahead = distance + footprint.FrontMeters + footprint.RearMeters + gap;
            var nominal = track.Nominal(track.PieceAt(ahead), ahead);
            var leader = new TrafficActorInput(new RoadId(0x539UL, 77UL), new VehicleFootprintPose { Position = nominal.Position,
                Forward = nominal.Forward, Up = nominal.Up, Footprint = footprint }, 0f, track.Pieces[track.PieceAt(ahead)].Id);
            for (int i = 0; i < 3000; i++)
            {
                Step(driver, admission, state, ++frameId, extra: leader);
                Assert.That(driver.LastRecoveryRequest, Is.Null, "attente legitime, pas " + i + " : " + driver.Recovery.ToText());
            }
            Assert.That(driver.LastLongitudinal, Is.Not.Null);
            Assert.That(driver.LastLongitudinal.Binding.Kind, Is.Not.EqualTo(LongitudinalCandidateKind.Profile),
                driver.LastLongitudinal.ToText());
            Assert.That(driver.Recovery.Cause, Is.EqualTo(RecoveryCause.None));

            // 2. Cale sans blocker : ProgressDeficit, recul par le pilote, puis alternance jusqu'a Faulted.
            int reverseSteps = 0;
            for (int i = 0; i < 6000 && !driver.Recovery.Faulted; i++)
            {
                bool reversing = driver.Tactical.Reversing;
                Step(driver, admission, state, ++frameId,
                    travel: reversing ? TrafficV2Settings.RecoveryReverseSpeedMetersPerSecond * Time.fixedDeltaTime : 0f);
                if (!driver.Tactical.Reversing) continue;
                reverseSteps++;
                Assert.That(driver.LastComposed.Fallback, Is.False, driver.Recovery.ToText());
                Assert.That(driver.LastComposed.Intent.Throttle, Is.Zero, "jamais de gaz en recul");
                Assert.That(driver.LastComposed.Intent.BrakeReverse, Is.GreaterThan(0f), "marche arriere engagee a l'arret");
            }
            string history = driver.Recovery.ToText() + " | " + string.Join(" ; ", driver.Recovery.Attempts.Select(a => a.ToText()));
            Assert.That(reverseSteps, Is.GreaterThan(0), history);
            var attempts = driver.Recovery.Attempts;
            Assert.That(attempts.First().Request.Cause, Is.EqualTo(RecoveryCause.ProgressDeficit), history);
            Assert.That(attempts.First().Request.Maneuver, Is.EqualTo(RecoveryManeuver.Reverse), history);
            Assert.That(attempts.Count(a => a.Outcome == TacticalReason.ManeuverCompleted), Is.GreaterThan(0), history);
            Assert.That(attempts.Count, Is.EqualTo(TrafficV2Settings.RecoveryMaxAttempts), history);

            // 3. Faulted : repli Faulted a chaque pas, ni planification ni requete, meme sous un choc ; vehicule present.
            Assert.That(driver.Recovery.Faulted, Is.True, history);
            Assert.That(driver.Recovery.FaultReason, Is.EqualTo("AttemptsExhausted"));
            for (int i = 0; i < 100; i++)
            {
                Step(driver, admission, state, ++frameId, contact: i == 10, normalSpeed: 6f);
                Assert.That(driver.Lifecycle, Is.EqualTo(TrafficV2LifecycleState.Faulted));
                Assert.That(driver.LastComposed.Fallback, Is.True);
                Assert.That(driver.LastComposed.Reason, Is.EqualTo(V2FallbackReason.Faulted));
                Assert.That(driver.LastComposed.Intent.Throttle, Is.Zero);
                Assert.That(driver.LastRecoveryRequest, Is.Null);
                Assert.That(driver.LastCollisionRequest, Is.Null, "aucune decision tactique en Faulted");
                Assert.That(driver.Tactical.Active, Is.False);
            }
            Assert.That(driver.LastComposed.Intent.Handbrake, Is.EqualTo(1f), "repli tenu");
            StringAssert.Contains("\nRecovery Faulted AttemptsExhausted", driver.LastProjection.ToText());
            Assert.That(driver != null && driver.IsSpawned && driver.gameObject.activeInHierarchy, Is.True, "vehicule present");
        }

        private static void Step(TrafficV2VehicleDriver driver, TrafficV2Admission admission, BodyState state, ulong frameId,
            bool contact = false, float normalSpeed = 0f, float travel = 0f, TrafficActorInput? extra = null)
        {
            var track = driver.Tracks.Last();
            int piece;
            float distance = track.Project(state.Position, 0, out piece);
            float displacement = TrackingMeasurement.StepDisplacement(state, track, distance, Read<GaugeBox>(driver, "gauge"));
            ulong step = Read<ulong>(driver, "stepCounter") + 1UL;
            var pose = new VehicleFootprintPose { Position = state.Position, Forward = state.Rotation * Vector3.forward,
                Up = state.Rotation * Vector3.up, Footprint = driver.Footprint };
            Write(driver, "stepCounter", step);
            Write(driver, "stepPrepared", true);
            Write(driver, "preparedTravel", travel);
            Write(driver, "preparedState", state);
            Write(driver, "preparedSpeed", 0f);
            Write(driver, "preparedPose", pose);
            Write(driver, "preparedTrack", track);
            Write(driver, "preparedOffset", track.OffsetRadians(piece, distance));
            Write(driver, "preparedDisplacement", (float?)displacement);
            Write(driver, "preparedFacts", new CollisionFacts(step, contact, 0f, contact ? normalSpeed : 0f, contact ? Time.fixedDeltaTime : 0f,
                1, 0f, 0f, 4, 4, displacement, driver.GetComponent<Rigidbody>().mass));
            var self = new TrafficActorInput(driver.TrafficId, pose, 0f, Read<RoadId>(driver, "previousElement"));
            var inputs = extra.HasValue ? new[] { self, extra.Value } : new[] { self };
            var method = typeof(TrafficV2VehicleDriver).GetMethod("Step", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            // Sans index de carrefour : ni demande ni contrainte d'entree, seul le leader decide de l'attente.
            method.Invoke(driver, new object[] { frameId, new TrafficFrame(frameId, admission.Model, inputs), default(HazardQueryReport),
                default(TrafficHazardCollectorCounters), 0d, null, null });
        }

        private static T Read<T>(TrafficV2VehicleDriver driver, string name) { return (T)Field(name).GetValue(driver); }

        private static void Write(TrafficV2VehicleDriver driver, string name, object value) { Field(name).SetValue(driver, value); }

        private static FieldInfo Field(string name)
        {
            var field = typeof(TrafficV2VehicleDriver).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name);
            return field;
        }
    }
}
