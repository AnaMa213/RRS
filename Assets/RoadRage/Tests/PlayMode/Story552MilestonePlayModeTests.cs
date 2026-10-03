using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
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
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Migration;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEditor;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>Story 5.52 : jalon 1 V2 sur MVP_Run, hors run de mesure, apres la signature Gate A.</summary>
    [Category("Story552")]
    public sealed class Story552MilestonePlayModeTests
    {
        private const int MaxFixedSteps = 9000;
        private string originalProfilePath;
        private string tempProfilePath;
        private GameObject profileProbe;
        private GameObject spawnerProbe;
        private VehicleProfileDef probeVehicleProfile;
        private DriverProfileDef probeDriverProfile;

        [SetUp]
        public void SetUp() { TrafficV2Session.Reset(); }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TrafficV2Session.Reset();
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                if (manager.IsListening) manager.Shutdown();
                Object.Destroy(manager.gameObject);
            }
            if (RoadRageBootstrap.Instance != null) Object.Destroy(RoadRageBootstrap.Instance.gameObject);
            if (originalProfilePath != null) PlayerProfileFileStore.DefaultFilePath = originalProfilePath;
            if (tempProfilePath != null && File.Exists(tempProfilePath)) File.Delete(tempProfilePath);
            Object.Destroy(profileProbe);
            Object.Destroy(spawnerProbe);
            Object.Destroy(probeVehicleProfile);
            Object.Destroy(probeDriverProfile);
            yield return null;
        }

        [Test]
        public void ChangedPrefabFeasibilityInputsInvalidateAdmissionAndItsCache()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GateAEvidenceParameters.V2PrefabPath);
            profileProbe = Object.Instantiate(prefab);
            profileProbe.SetActive(false);
            var physics = profileProbe.GetComponent<VehiclePhysicsBody>();
            var originalVehicle = (VehicleProfileDef)new SerializedObject(physics).FindProperty("vehicleProfile").objectReferenceValue;
            probeVehicleProfile = Object.Instantiate(originalVehicle);
            physics.BindProfile(probeVehicleProfile);
            var driver = profileProbe.GetComponent<TrafficV2VehicleDriver>();
            probeDriverProfile = Object.Instantiate(driver.DriverProfileDefinition);
            var serializedDriver = new SerializedObject(driver);
            serializedDriver.FindProperty("driverProfile").objectReferenceValue = probeDriverProfile;
            serializedDriver.ApplyModifiedPropertiesWithoutUndo();
            var initial = TrafficV2Lifecycle.AdmitCommittedArtifacts(profileProbe);
            Assert.That(initial.Admitted, Is.True);
            spawnerProbe = new GameObject("Story552 admission caller");
            spawnerProbe.SetActive(false);
            var spawner = spawnerProbe.AddComponent<PortalTrafficSpawner>();
            var prefabField = typeof(PortalTrafficSpawner).GetField("v2VehiclePrefab", BindingFlags.Instance | BindingFlags.NonPublic);
            var admissionField = typeof(PortalTrafficSpawner).GetField("v2Admission", BindingFlags.Instance | BindingFlags.NonPublic);
            var tick = typeof(PortalTrafficSpawner).GetMethod("TickV2Slice", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(prefabField, Is.Not.Null);
            Assert.That(admissionField, Is.Not.Null);
            Assert.That(tick, Is.Not.Null);
            prefabField.SetValue(spawner, profileProbe);
            foreach (var field in new[] { "steerRateDegreesPerSecond", "lateralFrictionCoefficient", "desiredSpeed" })
            {
                var definition = field == "desiredSpeed" ? (UnityEngine.Object)probeDriverProfile : probeVehicleProfile;
                var serialized = new SerializedObject(definition);
                var property = serialized.FindProperty("profile." + field);
                Assert.That(property, Is.Not.Null, field);
                float original = property.floatValue;
                property.floatValue = field == "steerRateDegreesPerSecond" ? 1f : original * 1.1f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var changed = TrafficV2Lifecycle.AdmitCommittedArtifacts(profileProbe);
                Assert.That(changed.Code, Is.EqualTo(TrafficV2Code.GateAEvidenceStale), field);
                Assert.That(TrafficV2Lifecycle.EvaluateInsertion(changed, null, TrafficV2Settings.DeclaredTrackingTolerance).Allowed,
                    Is.False, "Aucune insertion normale sur une preuve perimee : " + field);
                admissionField.SetValue(spawner, null);
                tick.Invoke(spawner, null);
                Assert.That(spawner.V2LastCode, Is.EqualTo(TrafficV2Code.GateAEvidenceStale), "Le spawner admet son propre prefab : " + field);
                Assert.That(spawner.V2Insertions, Is.Zero);
                property.floatValue = original;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(TrafficV2Lifecycle.AdmitCommittedArtifacts(profileProbe).Admitted, Is.True, "Profil restaure : " + field);
            }
            Vector3 gravity = Physics.gravity;
            try
            {
                Physics.gravity = gravity * 1.1f;
                Assert.That(TrafficV2Lifecycle.AdmitCommittedArtifacts(profileProbe).Code, Is.EqualTo(TrafficV2Code.GateAEvidenceStale));
            }
            finally { Physics.gravity = gravity; }
            Assert.That(TrafficV2Lifecycle.AdmitCommittedArtifacts().Admitted, Is.True, "Les vrais assets restent intacts.");
        }

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator AnActualToleranceExceedanceHoldsTheDriverAcrossPushesAndItsExit()
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True);
            TrafficV2Session.Request(TrafficComposition.V2Slice, null);
            yield return EnterMvpRun();
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            TrafficV2VehicleDriver driver = null;
            Rigidbody body = null;
            Portal exit = default(Portal);
            EffectiveLaneCorridor corridor = default(EffectiveLaneCorridor);
            int steps = 0;
            bool nearExit = false;
            while (steps++ < MaxFixedSteps && !nearExit)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(spawner.V2Removals, Is.Zero, "Le vehicule doit etre perturbe avant sa sortie.");
                if (spawner.LiveV2Population == 0) continue;
                driver = spawner.LiveV2Vehicles[0].GetComponent<TrafficV2VehicleDriver>();
                body = driver.GetComponent<Rigidbody>();
                exit = admission.Model.Portals.First(p => p.Id == driver.ExitPortalId);
                Assert.That(admission.Model.TryGetCorridor(exit.CorridorId, out corridor), Is.True);
                // Meme localisation que HasReachedExit, qui decide du retrait.
                var approachPose = new VehicleFootprintPose { Position = body.position, Forward = driver.transform.forward, Up = driver.transform.up };
                var approachFrame = new TrafficFrame((ulong)steps, admission.Model,
                    new[] { new TrafficActorInput(driver.TrafficId, approachPose, 0f, exit.CorridorId) });
                TrafficActor approach;
                float remaining = approachFrame.TryGetActor(driver.TrafficId, out approach) && approach.Location.Localized
                    && approach.Location.ElementId == exit.CorridorId ? exit.SMeters - approach.Location.SMeters : float.NaN;
                nearExit = remaining > 1f && remaining < 4f;
                Assert.That(driver.ToleranceResponse.Latched, Is.False, "Trajet sain avant la perturbation.");
            }
            Assert.That(nearExit, Is.True, "Le vehicule doit atteindre l'approche de son portail de sortie.");
            Assert.That(driver.MeasurementLabel, Is.Null);
            body.AddForce(driver.transform.right * body.mass * 6f, ForceMode.Impulse);
            for (int i = 0; i < 150 && !driver.ToleranceResponse.Latched; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(spawner.V2Removals, Is.Zero);
            }
            Assert.That(driver.ToleranceResponse.Latched, Is.True, "Le driver doit mesurer et verrouiller le depassement reel.");
            Assert.That(driver.ToleranceExceededCount, Is.GreaterThan(0));
            Assert.That(driver.MaxStepDisplacementMeters, Is.GreaterThan(TrafficV2Settings.DeclaredTrackingTolerance.Meters));
            int replans = driver.ReplanCount;
            int tracks = driver.Tracks.Count;
            float latchedS = corridor.Curve.Project(body.position).SMeters;
            for (int i = 0; i < 1500 && driver.LastComposed.Terminal != V2FallbackTerminal.Held; i++)
            {
                yield return new WaitForFixedUpdate();
                AssertLatchedVehicle(driver, spawner, body, replans, tracks);
            }
            Assert.That(driver.LastComposed.Terminal, Is.EqualTo(V2FallbackTerminal.Held));
            for (int i = 0; i < 50; i++)
            {
                yield return new WaitForFixedUpdate();
                AssertLatchedVehicle(driver, spawner, body, replans, tracks);
            }

            Assert.That(body.linearVelocity.magnitude, Is.LessThan(0.25f), "Arret physique maintenu avant les poussees externes.");
            Vector3 backwardStart = body.position;
            for (int i = 0; i < 600 && corridor.Curve.Project(body.position).SMeters >= latchedS - 1f; i++)
            {
                body.AddForce(-corridor.Curve.Sample(latchedS).Tangent * body.mass * 30f, ForceMode.Force);
                yield return new WaitForFixedUpdate();
                AssertLatchedVehicle(driver, spawner, body, replans, tracks);
            }
            Assert.That(corridor.Curve.Project(body.position).SMeters, Is.LessThan(latchedS - 1f), "Poussee en arriere de la progression acquise.");
            Assert.That(Vector3.Distance(backwardStart, body.position), Is.GreaterThan(0.02f), "Le maintien reste poussable.");
            for (int i = 0; i < 1500 && driver.LastComposed.Terminal != V2FallbackTerminal.Held; i++)
                yield return new WaitForFixedUpdate();
            bool crossed = false;
            for (int i = 0; i < 900 && !crossed; i++)
            {
                body.AddForce(corridor.Curve.Sample(exit.SMeters).Tangent * body.mass * 30f, ForceMode.Force);
                yield return new WaitForFixedUpdate();
                AssertLatchedVehicle(driver, spawner, body, replans, tracks);
                var pose = new VehicleFootprintPose { Position = body.position, Forward = driver.transform.forward, Up = driver.transform.up };
                var frame = new TrafficFrame((ulong)i, admission.Model, new[] { new TrafficActorInput(driver.TrafficId, pose, 0f, exit.CorridorId) });
                TrafficActor actor;
                Assert.That(frame.TryGetActor(driver.TrafficId, out actor), Is.True);
                crossed = TrafficV2Lifecycle.HasReachedExit(actor.Location, exit);
            }
            Assert.That(crossed, Is.True, "La poussee doit franchir physiquement le portail, avec une localisation de sortie valide.");
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
                AssertLatchedVehicle(driver, spawner, body, replans, tracks);
            }
        }

        private static void AssertLatchedVehicle(TrafficV2VehicleDriver driver, PortalTrafficSpawner spawner,
            Rigidbody body, int replans, int tracks)
        {
            Assert.That(driver != null && driver.IsSpawned && driver.gameObject.activeInHierarchy, Is.True, "Vehicule present et reseau actif.");
            Assert.That(spawner.V2Removals, Is.Zero, "Aucun despawn apres verrouillage, meme au portail.");
            Assert.That(driver.LastComposed.Fallback, Is.True);
            Assert.That(driver.LastComposed.Reason, Is.EqualTo(V2FallbackReason.TrackingToleranceExceeded));
            Assert.That(driver.LastComposed.Intent.Throttle, Is.Zero);
            Assert.That(driver.LastProjection, Is.Not.Null);
            Assert.That(driver.LastProjection.Drive.Fallback, Is.True, "Le diagnostic suit la conduite sans relancer la planification.");
            Assert.That(driver.LastProjection.Drive.FallbackReason, Is.EqualTo(V2FallbackReason.TrackingToleranceExceeded.ToString()));
            Assert.That(driver.LastProjection.Drive.Throttle, Is.Zero);
            Assert.That(driver.LastProjection.Drive.PhysicsEpoch, Is.EqualTo(driver.IntentsApplied));
            Assert.That(driver.ReplanCount, Is.EqualTo(replans));
            Assert.That(driver.Tracks.Count, Is.EqualTo(tracks), "Aucune adoption d'une nouvelle reference.");
            Assert.That(driver.HasReachedExitPortal, Is.False, "Le verrou interdit la fin normale du trajet.");
            Assert.That(body.isKinematic, Is.False);
            Assert.That(body.constraints, Is.EqualTo(RigidbodyConstraints.None));
            Assert.That(float.IsNaN(body.position.sqrMagnitude) || float.IsInfinity(body.position.sqrMagnitude), Is.False);
        }

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator TheFirstV2TripCrossesMvpRunWithinTheSignedLimitsOutsideMeasurement()
        {
            var signedBytes = new[] { TrafficV2Settings.ModelPath, TrafficV2Settings.SignoffPath, TrafficV2Settings.ReportPath }
                .Select(File.ReadAllBytes).ToArray();
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Code, Is.EqualTo(TrafficV2Code.Allowed));
            Assert.That(admission.Evidence.TrackingAllowanceMeters, Is.EqualTo(0.34f));
            Assert.That(admission.Evidence.PoseModel, Is.EqualTo(NominalPoseModel.Kinematic));
            Assert.That(TrafficV2Lifecycle.EvaluateInsertion(admission, null, TrafficV2Settings.DeclaredTrackingTolerance).Allowed,
                Is.True, "insertion admise sans run de mesure");

            TrafficV2Session.Request(TrafficComposition.V2Slice, null);
            yield return EnterMvpRun();
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);

            TrafficV2VehicleDriver liveDriver = null;
            RoadId exitPortalId = RoadId.None;
            Vector3? firstSeen = null;
            int steps = 0;
            while (spawner.V2Removals == 0 && steps++ < MaxFixedSteps)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(spawner.LiveV2Population, Is.LessThanOrEqualTo(1));
                if (spawner.LiveV2Population != 1) continue;
                var current = spawner.LiveV2Vehicles[0].GetComponent<TrafficV2VehicleDriver>();
                if (liveDriver == null)
                {
                    liveDriver = current;
                    // La trace est une observation de test : elle ne change ni la session ni le verdict hors mesure.
                    var traceFlag = typeof(TrafficV2VehicleDriver).GetField("recordTrace", BindingFlags.Instance | BindingFlags.NonPublic);
                    Assert.That(traceFlag, Is.Not.Null);
                    traceFlag.SetValue(liveDriver, true);
                    Assert.That(current.MeasurementLabel, Is.Null, "jalon hors mesure");
                    exitPortalId = current.ExitPortalId;
                    firstSeen = current.transform.position;
                }
            }

            Assert.That(liveDriver, Is.Not.Null, "aucun vehicule V2 insere : " + spawner.V2LastCode);
            Assert.That(spawner.Composition, Is.EqualTo(TrafficComposition.V2Slice));
            Assert.That(spawner.V2Removals, Is.EqualTo(1), "trajet portail a portail incomplet : " + spawner.V2LastCode);
            Assert.That(spawner.RetiredV2Runs.Count, Is.EqualTo(1));
            var run = spawner.RetiredV2Runs[0];
            Assert.That(run.HasReachedExitPortal, Is.True);
            Assert.That(run.Trace.Count, Is.GreaterThan(1), "trace au pas requise pour le modele M");
            Assert.That(run.Trace.Count, Is.EqualTo(run.Timings.Steps), "chaque pas conduit doit etre trace");
            Assert.That(run.Trace[0].Step, Is.EqualTo(1UL), "trace depuis le premier pas physique");

            var model = admission.Model;
            var exit = model.Portals.First(p => p.Id == exitPortalId);
            Portal ignored;
            Vector3 exitPosition;
            Quaternion rotation;
            Assert.That(TrafficV2Lifecycle.TryPortalPose(model, exit.Id, out ignored, out exitPosition, out rotation), Is.True);
            float entryDistance = float.PositiveInfinity;
            foreach (var entry in model.Portals.Where(p => p.Role == PortalRole.Entry))
            {
                Vector3 entryPosition;
                if (TrafficV2Lifecycle.TryPortalPose(model, entry.Id, out ignored, out entryPosition, out rotation))
                    entryDistance = Mathf.Min(entryDistance, PlanarDistance(firstSeen.Value, entryPosition));
            }
            Assert.That(entryDistance, Is.LessThan(8f), "insertion pres d'un portail d'entree");
            var finalPosition = run.Trace[run.Trace.Count - 1].State.Position;
            Assert.That(PlanarDistance(finalPosition, exitPosition), Is.LessThan(8f),
                "derniere pose mesuree hors du portail de sortie " + exitPortalId + " : " + finalPosition + " / " + exitPosition);

            float maxStep = 0f, maxBound = 0f, maxRatio = 0f;
            int intervals = 0;
            foreach (var record in run.Trace)
            {
                maxStep = Mathf.Max(maxStep, record.StepDisplacementMeters);
                Assert.That(record.StepDisplacementMeters, Is.LessThanOrEqualTo(TrafficV2Settings.DeclaredTrackingTolerance.Meters),
                    "d au pas " + record.Step);
                Assert.That(record.OutsideWidthEnvelope, Is.False, "sortie de chaussee au pas " + record.Step);
                if (!float.IsNaN(record.SpeedRatio))
                {
                    maxRatio = Mathf.Max(maxRatio, record.SpeedRatio);
                    Assert.That(record.SpeedRatio, Is.LessThanOrEqualTo(1f), "v > v* au pas " + record.Step);
                }
            }
            for (int i = 0; i + 1 < run.Trace.Count; i++)
            {
                var a = run.Trace[i];
                var b = run.Trace[i + 1];
                Assert.That(b.Step, Is.EqualTo(a.Step + 1), "aucun intervalle physique ne doit manquer");
                var track = run.Tracks[a.TrackIndex];
                int nextPiece;
                float nextDistance = b.TrackIndex == a.TrackIndex ? b.RouteDistanceMeters
                    : track.Project(b.State.Position, a.Piece, out nextPiece);
                var bound = TrackingMeasurement.InterStepBound(a.State, b.State, run.FixedDeltaTimeSeconds,
                    a.RouteDistanceMeters, nextDistance, track, run.Gauge, TrafficV2Settings.InterStepRemainderMeters,
                    TrafficV2Settings.ModelPositionToleranceMeters, TrafficV2Settings.ModelRotationToleranceDegrees);
                Assert.That(bound.ModelVerified, Is.True, "modele M non verifie aux pas " + a.Step + "-" + b.Step);
                maxBound = Mathf.Max(maxBound, bound.BoundMeters);
                Assert.That(bound.BoundMeters, Is.LessThanOrEqualTo(TrafficV2Settings.DeclaredTrackingTolerance.Meters),
                    "borne M aux pas " + a.Step + "-" + b.Step);
                intervals++;
                if ((i & 63) == 63) yield return null;
            }
            Assert.That(intervals, Is.GreaterThan(0));
            Assert.That(run.NominalPoseInfeasibleSteps, Is.Zero);
            Assert.That(run.ContactEpisodes.Where(c => !c.ColliderPath.Contains("/Collision/Col_Roadway")), Is.Empty,
                "aucun contact de caisse hors chaussee");
            for (int i = 0; i < signedBytes.Length; i++)
                Assert.That(File.ReadAllBytes(new[] { TrafficV2Settings.ModelPath, TrafficV2Settings.SignoffPath,
                    TrafficV2Settings.ReportPath }[i]), Is.EqualTo(signedBytes[i]), "aucune preuve ecrite par le jalon");

            Debug.Log("[Story552] Gate B jalon 1 : portail a portail, " + run.Trace.Count + " pas, " + intervals
                + " intervalles M, d max " + maxStep + " m, borne M max " + maxBound + " m, v/v* max " + maxRatio
                + ", contacts " + run.ContactEpisodes.Count + ".");
        }

        private IEnumerator EnterMvpRun()
        {
            originalProfilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfilePath = Path.Combine(Path.GetTempPath(), "roadrage-story552-" + Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfilePath;
            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));
            Click(Object.FindAnyObjectByType<MainMenuScreen>(), "playButton");
            yield return null;
            var lobby = Object.FindAnyObjectByType<LobbyShellScreen>();
            LogAssert.ignoreFailingMessages = true;
            try
            {
                Click(lobby, "startGameButton");
                // Premier test d'un Editeur redemarre : le premier CreateLobbyAsync Steam a froid depasse 300 frames
                // (~2,8 s, deux runs du 2026-10-02, decision proprietaire en Story 5.33) : budget en temps reel.
                float deadline = Time.realtimeSinceStartup + 30f;
                while (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && Time.realtimeSinceStartup < deadline)
                {
                    var bootstrap = RoadRageBootstrap.Instance;
                    if (bootstrap != null && bootstrap.LobbyRoom != null
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                    {
                        Assert.Inconclusive("Services en ligne indisponibles : jalon MVP_Run non verifiable.");
                        yield break;
                    }
                    yield return null;
                }
            }
            finally { LogAssert.ignoreFailingMessages = false; }
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));
            Assert.That(NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer, Is.True);
        }

        private static void Click(Component screen, string fieldName)
        {
            Assert.That(screen, Is.Not.Null);
            var field = screen.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            var button = field.GetValue(screen) as Button;
            Assert.That(button, Is.Not.Null);
            button.onClick.Invoke();
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            var delta = a - b;
            delta.y = 0f;
            return delta.magnitude;
        }
    }
}
