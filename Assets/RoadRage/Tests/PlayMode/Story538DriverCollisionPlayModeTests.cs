using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Collisions;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Recovery;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Features.Vehicles.Traffic.Tactical;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Regressions de revue 5.38 dans MVP_Run, sur un driver reel spawne par l'hote. Les faits et la pose prepares sont
    /// injectes a la frontiere PrepareStep/Step pour isoler les transitions du driver. Le test execute Step, Safety,
    /// Compose, ApplyDriveIntent et la projection ; il ne revendique pas l'extraction des contacts PhysX (Gate D).
    /// </summary>
    [Category("Story538")]
    public sealed class Story538DriverCollisionPlayModeTests
    {
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
        [Category("Story539")]
        public IEnumerator TheLiveDriverPreservesItsReferenceAndPublishesTerminalAndInvalidReasons()
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile scenarioFile;
            var scenario = Story533Harness.Load(admission, "B", out scenarioFile);
            TrafficV2Session.Request(TrafficComposition.V2Slice, null, Story533Harness.ToScenario(scenario));
            yield return harness.EnterMvpRun();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MVP_Run"));
            yield return harness.ParkHostPlayer(scenarioFile.Parking.Value, _ => { });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            TrafficV2VehicleDriver driver = null;
            for (int i = 0; i < 500 && driver == null; i++)
            {
                yield return new WaitForFixedUpdate();
                driver = Object.FindObjectsByType<TrafficV2VehicleDriver>().FirstOrDefault(d => d.IsBound && d.IntentsApplied > 0);
            }
            Assert.That(driver, Is.Not.Null, "driver V2 prepare et spawne dans MVP_Run");
            Assert.That(driver.IsSpawned && driver.IsServer, Is.True);
            Assert.That(driver.ToleranceResponse.Latched, Is.False);
            // Aucun autre FixedUpdate pendant les transitions injectees : le spawner reprend apres ce bloc de test.
            var route = Read<RoutePlan>(driver, "route");
            var insertion = Read<TrafficV2Insertion>(driver, "insertion");
            var originalState = Read<BodyState>(driver, "preparedState");
            var originalTrack = driver.Tracks.Last();
            int originalTracks = driver.Tracks.Count;
            int originalReplans = driver.ReplanCount;
            ulong frameId = driver.LastFrameId;

            // Trouver une pose sur une AUTRE voie pour laquelle le planificateur nominal replannerait effectivement.
            BodyState displacedState = default(BodyState);
            bool found = false;
            foreach (var corridor in admission.Model.Corridors)
            {
                var sample = corridor.Curve.Sample(corridor.LengthMeters * 0.5f);
                var state = new BodyState(sample.Position, Quaternion.LookRotation(sample.Tangent, sample.Up), sample.Position,
                    Vector3.zero, Vector3.zero);
                var frame = Frame(++frameId, admission, driver, state);
                var nominal = PlanningSpine.Evaluate(new PlanningRequest(frame, driver.TrafficId, route, driver.ExitPortalId,
                    insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver.DriverProfileDefinition.Profile,
                    evidence: admission.Evidence));
                int piece;
                float distance = originalTrack.Project(state.Position, 0, out piece);
                float displacement = TrackingMeasurement.StepDisplacement(state, originalTrack, distance, Read<GaugeBox>(driver, "gauge"));
                if (nominal.Route.Outcome != RouteOutcome.Replanned || nominal.Route.Plan == null
                    || displacement <= TrafficV2Settings.DeclaredTrackingTolerance.Meters) continue;
                displacedState = state;
                found = true;
                break;
            }
            Assert.That(found, Is.True, "precondition R1 : autre voie accessible, replan nominal et d > epsilon_t");

            // Premier choc : protege aussi le pas d'acceptation, pas seulement un but deja actif.
            Step(driver, admission, displacedState, ++frameId, contact: true, normalSpeed: 6f);
            Assert.That(driver.LastTacticalResponse.Value.Reason, Is.EqualTo(TacticalReason.Accepted));
            AssertFrozen(driver, route, originalTrack, originalTracks, originalReplans);
            // Story 5.39 (accord proprietaire du 2026-10-07) : une fois en AwaitingRecovery, la recuperation prend le relais par
            // un realignement qui propulse ; le gaz nul n'est exige que pendant le but de collision. Route et reference restent
            // figees pendant les deux buts.
            bool awaited = false;
            for (int i = 0; i < 100; i++)
            {
                Step(driver, admission, displacedState, ++frameId, travel: RealignTravel);
                AssertFrozen(driver, route, originalTrack, originalTracks, originalReplans);
                Assert.That(driver.Tactical.Active, Is.True, "la nouvelle voie ne remplace pas la reference de recovery");
                if (driver.Tactical.CollisionActive) Assert.That(driver.LastComposed.Intent.Throttle, Is.Zero);
                awaited |= driver.Tactical.AwaitingRecovery;
                Assert.That(driver.LastLongitudinal, Is.Null);
                Assert.That(driver.LastJunctionReport == null || !driver.LastJunctionReport.RequestValid, Is.True);
            }
            Assert.That(awaited, Is.True, "stable mais deplace : AwaitingRecovery");
            Assert.That(driver.Tactical.Goal, Is.EqualTo(TacticalGoalKind.Recovery), driver.Recovery.ToText());
            Assert.That(driver.Tactical.Maneuver, Is.EqualTo(RecoveryManeuver.Realign));

            // Retour dans l'enveloppe de la reference originale : raison visible seulement au pas de terminaison.
            for (int i = 0; i < 50 && driver.Tactical.Active; i++) Step(driver, admission, originalState, ++frameId, travel: RealignTravel);
            Assert.That(driver.Tactical.LastReason, Is.EqualTo(TacticalReason.Resumed));
            Assert.That(driver.LastTacticalResponse, Is.Null);
            StringAssert.Contains("Tactical Nominal Resumed", driver.LastProjection.ToText());
            Step(driver, admission, originalState, ++frameId);
            StringAssert.DoesNotContain("\nTactical ", driver.LastProjection.ToText());

            // Annulation sans requete au pas de sortie.
            Step(driver, admission, originalState, ++frameId, contact: true, normalSpeed: 6f);
            Assert.That(driver.Tactical.Active, Is.True);
            Write(driver, "reachedExitPortal", true);
            Step(driver, admission, originalState, ++frameId);
            Assert.That(driver.Tactical.Active, Is.False);
            Assert.That(driver.LastTacticalResponse, Is.Null);
            StringAssert.Contains("Tactical Nominal ExitPortalReached", driver.LastProjection.ToText());
            Write(driver, "reachedExitPortal", false);

            // Faits non finis : analyse -> Submit -> refus -> publication, sans but.
            Step(driver, admission, originalState, ++frameId, normalSpeed: float.NaN);
            Assert.That(driver.LastCollisionRequest, Is.Not.Null);
            Assert.That(driver.LastTacticalResponse.Value.Reason, Is.EqualTo(TacticalReason.InvalidRequest));
            Assert.That(driver.Tactical.Active, Is.False);
            StringAssert.Contains("Tactical Nominal InvalidRequest", driver.LastProjection.ToText());
        }

        private static void AssertFrozen(TrafficV2VehicleDriver driver, RoutePlan route, ReferenceTrack track, int trackCount, int replans)
        {
            Assert.That(Read<RoutePlan>(driver, "route"), Is.SameAs(route));
            Assert.That(driver.Tracks.Count, Is.EqualTo(trackCount));
            Assert.That(driver.Tracks.Last(), Is.SameAs(track));
            Assert.That(driver.ReplanCount, Is.EqualTo(replans));
            Assert.That(driver.ToleranceResponse.Latched, Is.False);
        }

        private static TrafficFrame Frame(ulong id, TrafficV2Admission admission, TrafficV2VehicleDriver driver, BodyState state)
        {
            var pose = new VehicleFootprintPose { Position = state.Position, Forward = state.Rotation * Vector3.forward,
                Up = state.Rotation * Vector3.up, Footprint = driver.Footprint };
            return new TrafficFrame(id, admission.Model, new[] { new TrafficActorInput(driver.TrafficId, pose, 0f,
                Read<RoadId>(driver, "previousElement")) }, null, null, null);
        }

        /// <summary>
        /// Parcours injecte d'un pas de realignement (5.39) : la pose injectee est figee, mais le realignement propulse ; sans
        /// parcours, le registre R5 le declarerait a juste titre Stalled.
        /// </summary>
        private static float RealignTravel { get { return TrafficV2Settings.RecoveryRealignSpeedMetersPerSecond * Time.fixedDeltaTime; } }

        private static void Step(TrafficV2VehicleDriver driver, TrafficV2Admission admission, BodyState state, ulong frameId,
            bool contact = false, float normalSpeed = 0f, float travel = 0f)
        {
            var track = driver.Tracks.Last();
            int piece;
            float distance = track.Project(state.Position, 0, out piece);
            float displacement = TrackingMeasurement.StepDisplacement(state, track, distance, Read<GaugeBox>(driver, "gauge"));
            ulong step = Read<ulong>(driver, "stepCounter") + 1UL;
            Write(driver, "stepCounter", step);
            Write(driver, "stepPrepared", true);
            Write(driver, "preparedTravel", travel);
            Write(driver, "preparedState", state);
            Write(driver, "preparedSpeed", 0f);
            Write(driver, "preparedPose", new VehicleFootprintPose { Position = state.Position,
                Forward = state.Rotation * Vector3.forward, Up = state.Rotation * Vector3.up, Footprint = driver.Footprint });
            Write(driver, "preparedTrack", track);
            Write(driver, "preparedOffset", track.OffsetRadians(piece, distance));
            Write(driver, "preparedDisplacement", (float?)displacement);
            Write(driver, "preparedFacts", new CollisionFacts(step, contact, 0f, normalSpeed, contact ? Time.fixedDeltaTime : 0f,
                1, 0f, 0f, 4, 4, displacement, driver.GetComponent<Rigidbody>().mass));
            var method = typeof(TrafficV2VehicleDriver).GetMethod("Step", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(driver, new object[] { frameId, Frame(frameId, admission, driver, state), default(HazardQueryReport),
                default(TrafficHazardCollectorCounters), 0d, null, JunctionConflictIndex.For(admission.Model) });
        }

        private static T Read<T>(TrafficV2VehicleDriver driver, string name)
        {
            return (T)Field(name).GetValue(driver);
        }

        private static void Write(TrafficV2VehicleDriver driver, string name, object value) { Field(name).SetValue(driver, value); }

        private static FieldInfo Field(string name)
        {
            var field = typeof(TrafficV2VehicleDriver).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name);
            return field;
        }
    }
}
