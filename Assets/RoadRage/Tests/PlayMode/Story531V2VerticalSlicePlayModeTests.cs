using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.31 : tranche verticale Traffic V2 dans MVP_Run (bootstrap -> lobby -> MVP_Run, patron
    /// Story 5.10). Tests courts de la suite par defaut : admission hors run de mesure sous la preuve 5.52,
    /// refus d'un modele non declare, et un run de mesure court sur une route, de portail a
    /// portail. Aucune acceptation n'est revendiquee ici : les mesures sont publiees dans la sortie.
    /// </summary>
    [Category("Story531")]
    public sealed class Story531V2VerticalSlicePlayModeTests
    {
        private const float PortalObservationRadius = 8f;
        private const int ShortRunMaxFixedSteps = 6000;

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

            tempProfileFilePath = null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator OutsideAMeasurementRunTheSignedV2VehicleIsInserted()
        {
            var signed = SignedArtifactBytes();
            TrafficV2Session.Request(TrafficComposition.V2Slice, null);
            yield return EnterMvpRun();
            if (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName)
            {
                yield break;
            }

            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            for (var i = 0; i < 150; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(spawner.CompositionFrozen, Is.True);
            Assert.That(spawner.Composition, Is.EqualTo(TrafficComposition.V2Slice));
            Assert.That(spawner.V2Insertions, Is.GreaterThan(0), "la preuve cinematique signee admet le V2 hors mesure");
            Assert.That(spawner.LiveV2Population, Is.GreaterThan(0));
            Assert.That(spawner.V2LastCode, Is.EqualTo(TrafficV2Code.Allowed));
            Assert.That(Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsInactive.Include).Length, Is.EqualTo(0),
                "V2Slice : aucun vehicule IA V1");
            Assert.That(Object.FindObjectsByType<TrafficV2VehicleDriver>(FindObjectsInactive.Include).Length, Is.GreaterThan(0));
            var after = SignedArtifactBytes();
            for (var i = 0; i < signed.Length; i++)
            {
                Assert.That(after[i], Is.EqualTo(signed[i]), "aucune preuve n'est ecrite");
            }
        }

        [UnityTest]
        public IEnumerator AnUndeclaredModelIsRefusedWithANamedCode()
        {
            var modelText = File.ReadAllText(TrafficV2Settings.ModelPath);
            var undeclared = modelText.Replace("\"Declared\":true", "\"Declared\":false");
            Assert.That(undeclared, Is.Not.EqualTo(modelText));
            var admission = TrafficV2Lifecycle.Admit(undeclared, File.ReadAllText(TrafficV2Settings.SignoffPath),
                File.ReadAllText(TrafficV2Settings.ReportPath));
            Assert.That(admission.Code, Is.EqualTo(TrafficV2Code.UndeclaredDrivabilityProfile));
            var run = new MeasurementRun(MeasurementKind.Exploratory, "undeclared", new[] { new CampaignTriplet(new RoadId(1, 1), new RoadId(1, 2), 0) });
            Assert.That(TrafficV2Lifecycle.EvaluateInsertion(admission, run, TrafficV2Settings.DeclaredTrackingTolerance).Code,
                Is.EqualTo(TrafficV2Code.UndeclaredDrivabilityProfile), "meme sous run de mesure, un modele non declare n'est jamais admis");
            Assert.That(TrafficV2Lifecycle.PrepareInsertion(admission, new RoadId(1, 1), RoadId.None, 0, 1, default(DriverProfile), 0.02f).Code,
                Is.Not.EqualTo(TrafficV2Code.Allowed));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AShortMeasurementRunDrivesOneRoutePortalToPortal()
        {
            // Une route : premiere entree (ordre des identites), sortie retenue par la premiere decision, graine 0.
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Code, Is.EqualTo(TrafficV2Code.Allowed));
            var model = admission.Model;
            var entry = model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).First();
            var driverDef = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab")
                .GetComponent<TrafficV2VehicleDriver>().DriverProfileDefinition;
            var planned = TrafficV2Lifecycle.PrepareInsertion(admission, entry.Id, RoadId.None, 0, 1, driverDef.Profile, 0.02f);
            Assert.That(planned.Code, Is.EqualTo(TrafficV2Code.Allowed));
            var exit = model.Portals.First(p => p.Id == planned.Route.ExitPortalId);
            Portal ignored;
            Vector3 entryPosition, exitPosition;
            Quaternion rotation;
            TrafficV2Lifecycle.TryPortalPose(model, entry.Id, out ignored, out entryPosition, out rotation);
            TrafficV2Lifecycle.TryPortalPose(model, exit.Id, out ignored, out exitPosition, out rotation);

            var run = new MeasurementRun(MeasurementKind.Exploratory, "playmode-short",
                new[] { new CampaignTriplet(entry.Id, exit.Id, 0) });
            TrafficV2Session.Request(TrafficComposition.V2Slice, run);
            yield return EnterMvpRun();
            if (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName)
            {
                yield break;
            }

            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            TrafficV2VehicleDriver driver = null;
            Vector3? firstSeen = null;
            var lastSeen = Vector3.zero;
            var fixedSteps = 0;
            var aliveSteps = 0;
            while (spawner.V2Removals == 0 && fixedSteps < ShortRunMaxFixedSteps)
            {
                yield return new WaitForFixedUpdate();
                fixedSteps++;
                Assert.That(Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsInactive.Include).Length, Is.EqualTo(0),
                    "V2Slice : aucun vehicule IA V1");
                Assert.That(spawner.LiveV2Population, Is.LessThanOrEqualTo(TrafficV2Settings.V2SliceMaxPopulation));
                if (spawner.LiveV2Population == 1)
                {
                    var live = spawner.LiveV2Vehicles[0].GetComponent<TrafficV2VehicleDriver>();
                    driver = live;
                    aliveSteps++;
                    lastSeen = live.transform.position;
                    if (!firstSeen.HasValue)
                    {
                        firstSeen = lastSeen;
                    }
                }
            }

            Assert.That(driver, Is.Not.Null, "un vehicule V2 doit etre insere sous run de mesure (code " + spawner.V2LastCode + ")");
            var summary = Summarize(driver, fixedSteps);
            Debug.Log("[Story531] run de mesure court : " + summary);
            WriteRawTrace(driver, summary);
            Assert.That(spawner.V2Removals, Is.EqualTo(1), "le vehicule doit se retirer a la sortie dans la fenetre : " + summary);
            Assert.That(driver.HasReachedExitPortal, Is.True, "retrait seulement au portail de sortie");
            Assert.That(spawner.V2Insertions, Is.EqualTo(1), "une seule insertion : la campagne ne porte qu'un triplet");
            Assert.That(Distance(firstSeen.Value, entryPosition), Is.LessThan(PortalObservationRadius), "insertion au portail d'entree");
            Assert.That(Distance(lastSeen, exitPosition), Is.LessThan(PortalObservationRadius), "retrait au portail de sortie");
            Assert.That(driver.IntentsApplied, Is.EqualTo(driver.Timings.Steps), "un intent par pas decide");
            Assert.That(driver.IntentsApplied, Is.GreaterThanOrEqualTo(aliveSteps - 2), "un intent par pas physique vivant");
            Assert.That(driver.NegativeFallbackTorqueSteps, Is.EqualTo(0), "aucun couple de marche arriere pendant un repli : " + summary);
            Assert.That(driver.MeasurementLabel, Is.EqualTo("Exploratory:playmode-short"));
            Assert.That(driver.LastProjection, Is.Not.Null);
            var text = driver.LastProjection.ToText();
            StringAssert.Contains("Speed constraints applied", text);
            StringAssert.Contains("Speed constraints deferred", text);
            StringAssert.Contains("Epochs decision", text);
            StringAssert.Contains("Final intent", text);
            StringAssert.Contains("measurement Exploratory:playmode-short", text);
        }

        private static string Summarize(TrafficV2VehicleDriver driver, int fixedSteps)
        {
            var trace = driver.Trace;
            var fallbacks = trace.Count(r => r.Fallback);
            var maxRatio = trace.Where(r => !float.IsNaN(r.SpeedRatio)).Select(r => r.SpeedRatio).DefaultIfEmpty(0f).Max();
            var above = trace.Count(r => !float.IsNaN(r.SpeedRatio) && r.SpeedRatio > 1f);
            var maxLateral = trace.Where(r => !float.IsNaN(r.LateralErrorMeters)).Select(r => Mathf.Abs(r.LateralErrorMeters)).DefaultIfEmpty(0f).Max();
            var maxResidualHeading = trace.Where(r => !float.IsNaN(r.HeadingErrorDegrees))
                .Select(r => Mathf.Abs(r.HeadingErrorDegrees + r.NominalOffsetDegrees)).DefaultIfEmpty(0f).Max();
            var maxOffset = trace.Select(r => Mathf.Abs(r.NominalOffsetDegrees)).DefaultIfEmpty(0f).Max();
            return string.Format(CultureInfo.InvariantCulture,
                "pas fixes {0}, pas decides {1}, fixedDeltaTime {2:0.####} s, replis {3}, d max au pas {4:0.####} m (pose nominale cinematique), "
                + "max v/v* {5:0.####} ({6} pas au-dessus), max ecart lateral {7:0.###} m, max |e| nominal {8:0.##} deg, "
                + "max ecart de cap residuel {9:0.##} deg, NominalPoseInfeasible {10} pas, replanifications {11}, contacts [{12}], "
                + "couple negatif en repli {13} ; temps {14}",
                fixedSteps, driver.IntentsApplied, driver.FixedDeltaTimeSeconds, fallbacks, driver.MaxStepDisplacementMeters, maxRatio,
                above, maxLateral, maxOffset, maxResidualHeading, driver.NominalPoseInfeasibleSteps, driver.ReplanCount,
                string.Join(" ; ", driver.ContactEpisodes.Select(Describe).ToArray()), driver.NegativeFallbackTorqueSteps, driver.Timings);
        }

        private static string Describe(V2ContactEpisode c)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} pas {1}-{2} impulsion {3:0.###} N.s v_n {4:0.###} m/s v {5:0.###}->{6:0.###} m/s",
                c.ColliderPath, c.FirstStep, c.LastStep, c.MaxImpulseNewtonSeconds, c.MaxRelativeNormalSpeed, c.SpeedAtFirstContact,
                c.MinimumSpeedDuringContact);
        }

        /// <summary>Trace brute du run court (exploratoire, jamais une acceptation).</summary>
        private static void WriteRawTrace(TrafficV2VehicleDriver driver, string summary)
        {
            const string folder = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements";
            Directory.CreateDirectory(folder);
            var text = new System.Text.StringBuilder("# " + summary + "\n# replans: " + string.Join(" ; ", driver.Replans.ToArray()) + "\n");
            text.Append("step\ttrack\tpiece\telement\ts_route_m\td_step_m\tv\tvstar\tlateral_m\theading_deg\tfallback\treason\tterminal\tthrottle\tsteer\tbrake_reverse\thandbrake\tmin_drive_torque\tbinding\tx\ty\tz"
                + "\tnominal_offset_deg\tnominal_steer_deg\tnominal_steer_rate_dps\tnominal_feasible\tgrounded_wheels\troll_deg\tpitch_deg\n");
            foreach (var r in driver.Trace)
            {
                text.Append(r.Step).Append('\t').Append(r.TrackIndex).Append('\t').Append(r.Piece).Append('\t').Append(r.ElementId).Append('\t')
                    .Append(F(r.RouteDistanceMeters)).Append('\t').Append(F(r.StepDisplacementMeters)).Append('\t').Append(F(r.LongitudinalSpeed))
                    .Append('\t').Append(r.CeilingUnbounded ? "inf" : F(r.CeilingMetersPerSecond)).Append('\t').Append(F(r.LateralErrorMeters))
                    .Append('\t').Append(F(r.HeadingErrorDegrees)).Append('\t').Append(r.Fallback ? 1 : 0).Append('\t').Append(r.Reason).Append('\t')
                    .Append(r.Terminal).Append('\t').Append(F(r.Intent.Throttle)).Append('\t').Append(F(r.Intent.Steer)).Append('\t')
                    .Append(F(r.Intent.BrakeReverse)).Append('\t').Append(F(r.Intent.Handbrake)).Append('\t').Append(F(r.MinimumDriveTorque))
                    .Append('\t').Append(r.Binding).Append('\t').Append(F(r.State.Position.x)).Append('\t').Append(F(r.State.Position.y))
                    .Append('\t').Append(F(r.State.Position.z))
                    .Append('\t').Append(F(r.NominalOffsetDegrees)).Append('\t').Append(F(r.NominalSteerDegrees))
                    .Append('\t').Append(F(r.NominalSteerRateDegreesPerSecond)).Append('\t').Append(r.NominalFeasible ? 1 : 0)
                    .Append('\t').Append(r.GroundedWheels).Append('\t').Append(F(SignedAngle(r.State.Rotation.eulerAngles.z)))
                    .Append('\t').Append(F(SignedAngle(r.State.Rotation.eulerAngles.x))).Append('\n');
            }

            File.WriteAllText(folder + "/playmode-short-run-steps.tsv", text.ToString());
        }

        private static string F(float value)
        {
            return float.IsNaN(value) ? "nan" : value.ToString("0.#####", CultureInfo.InvariantCulture);
        }

        /// <summary>Angle d'Euler ramene dans [-180, 180] (roulis z, tangage x).</summary>
        private static float SignedAngle(float degrees)
        {
            return Mathf.DeltaAngle(0f, degrees);
        }

        private static float Distance(Vector3 a, Vector3 b)
        {
            var offset = a - b;
            offset.y = 0f;
            return offset.magnitude;
        }

        private static byte[][] SignedArtifactBytes()
        {
            return new[] { TrafficV2Settings.ModelPath, TrafficV2Settings.SignoffPath, TrafficV2Settings.ReportPath }
                .Select(File.ReadAllBytes).ToArray();
        }

        /// <summary>Bootstrap -> menu -> lobby -> Start Game ; Inconclusif sans services en ligne (patron Story 5.10).</summary>
        private IEnumerator EnterMvpRun()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story531-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;

            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null);
            ClickSerializedButton(menuScreen, "playButton");
            yield return null;

            var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null);
            LogAssert.ignoreFailingMessages = true;
            try
            {
                ClickSerializedButton(lobbyScreen, "startGameButton");
                var frames = 0;
                while (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frames < 300)
                {
                    var bootstrap = RoadRageBootstrap.Instance;
                    if (bootstrap != null && bootstrap.LobbyRoom != null
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                    {
                        Assert.Inconclusive("Services en ligne non disponibles : impossible de verifier la tranche V2 dans MVP_Run.");
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
            Assert.That(NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer, Is.True, "le pair local est l'hote du run");
        }

        private static void ClickSerializedButton(Component screen, string buttonFieldName)
        {
            var field = screen.GetType().GetField(buttonFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, buttonFieldName);
            var button = field.GetValue(screen) as Button;
            Assert.That(button, Is.Not.Null);
            button.onClick.Invoke();
        }
    }
}
