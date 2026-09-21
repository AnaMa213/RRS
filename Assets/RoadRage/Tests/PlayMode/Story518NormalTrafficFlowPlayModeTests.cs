using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.MainMenu;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// ANO-5.18-03 : preuve RUNTIME que le trafic ORDINAIRE circule. Le banc 5.18 existant place deux
    /// vehicules face a face pour prouver qu'ils ne franchissent pas ensemble ; celui-ci ne place
    /// rien du tout. Il laisse le district tourner et constate les invariants que la recette manuelle
    /// a vus tomber :
    ///
    /// - aucun vehicule n'attend un feu ou une priorite AVEC SON AVANT DANS l'aire de conflit ;
    /// - aucune paire ne se declare mutuellement chef de file (l'attente circulaire de la video) ;
    /// - la circulation ordinaire ne recourt PAS a l'echelle de deblocage pour passer un carrefour ;
    /// - aucun vehicule ne reste immobile au-dela du delai d'escalade authore.
    ///
    /// Aucune ligne de ce fichier n'ecrit une pose, une vitesse ou une intention : le banc observe.
    /// Une machine sans Steam P2P fonctionnel rend le test Inconclusif plutot que rouge.
    /// </summary>
    [Category("Story518")]
    public sealed class Story518NormalTrafficFlowPlayModeTests
    {
        private const int SceneLoadFrameBudget = 300;
        private const int TrafficSpawnFrameBudget = 900;

        /// <summary>Environ 50 s de simulation a 50 Hz : de quoi voir plusieurs cycles de feux.</summary>
        private const int ObservationStepBudget = 2500;

        /// <summary>Tolerance sur la ligne d'arret : un pas de physique a vitesse de croisiere.</summary>
        private const float StopLineTolerance = 0.5f;

        private string originalProfileFilePath;
        private string tempProfileFilePath;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (originalProfileFilePath != null)
            {
                PlayerProfileFileStore.DefaultFilePath = originalProfileFilePath;
                originalProfileFilePath = null;
            }

            if (tempProfileFilePath != null && File.Exists(tempProfileFilePath))
            {
                File.Delete(tempProfileFilePath);
            }

            tempProfileFilePath = null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator OrdinaryDistrictTrafficClearsItsJunctionsWithoutUnblockingOrCircularWaits()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(),
                "roadrage-story518-flow-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;

            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;

            ClickSerializedButton(Object.FindAnyObjectByType<MainMenuScreen>(), "playButton");
            yield return null;
            var lobby = Object.FindAnyObjectByType<LobbyShellScreen>();
            if (lobby == null)
            {
                Assert.Inconclusive("LobbyShellScreen absent : banc non executable sur cette machine.");
                yield break;
            }

            ClickSerializedButton(lobby, "startGameButton");
            for (var frame = 0; SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frame < SceneLoadFrameBudget; frame++)
            {
                var bootstrap = RoadRageBootstrap.Instance;
                if (bootstrap != null && bootstrap.LobbyRoom != null && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                    && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                {
                    Assert.Inconclusive("Services Steam indisponibles : banc de flux non executable.");
                    yield break;
                }

                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName),
                "Le banc observe le district reel.");

            List<NetworkedAIVehicleDriverController> traffic = null;
            for (var frame = 0; frame < TrafficSpawnFrameBudget; frame++)
            {
                traffic = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsSortMode.None)
                    .Where(controller => controller.IsSpawned).ToList();
                if (traffic.Count >= 3) break;
                yield return null;
            }

            Assert.That(traffic, Is.Not.Null);
            if (traffic.Count < 2)
            {
                Assert.Inconclusive("Moins de deux vehicules inseres : le district n'a pas peuple ses portails.");
                yield break;
            }

            Assert.That(Object.FindAnyObjectByType<LaneGraph>(), Is.Not.Null, "LaneGraph attendu dans MVP_Run.");

            var stalledSince = new Dictionary<NetworkedAIVehicleDriverController, float>();
            var unblockingAtJunction = new List<string>();
            var stoppedInsideConflictArea = new List<string>();
            var circularWaits = new List<string>();

            for (var step = 0; step < ObservationStepBudget; step++)
            {
                yield return new WaitForFixedUpdate();

                var live = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsSortMode.None)
                    .Where(controller => controller != null && controller.IsSpawned).ToList();

                foreach (var vehicle in live)
                {
                    var waiting = vehicle.DecisionReason == TrafficDecisionReason.JunctionYield
                        || vehicle.DecisionReason == TrafficDecisionReason.ExitSaturated;

                    // 1. ARRET AVANT L'AIRE DE CONFLIT. JunctionStopGap est l'ecart de l'AVANT du
                    //    vehicule a la ligne : negatif veut dire que le capot l'a franchie.
                    if (waiting && vehicle.PlanarSpeed <= 0.5f && vehicle.JunctionStopGap < -StopLineTolerance)
                    {
                        stoppedInsideConflictArea.Add(Trace(vehicle));
                    }

                    // 2. LE TRAFIC ORDINAIRE NE PASSE PAS PAR LE DEBLOCAGE.
                    if (vehicle.JunctionBreached
                        || vehicle.DecisionReason == TrafficDecisionReason.JunctionBreach
                        || vehicle.DecisionReason == TrafficDecisionReason.DeadlockRecovery)
                    {
                        unblockingAtJunction.Add(Trace(vehicle));
                    }

                    // 3. IMMOBILITE PROLONGEE, quelle qu'en soit la cause nommee.
                    if (vehicle.PlanarSpeed <= 0.2f)
                    {
                        if (!stalledSince.ContainsKey(vehicle)) stalledSince[vehicle] = Time.time;
                    }
                    else
                    {
                        stalledSince.Remove(vehicle);
                    }
                }

                // 4. ATTENTE CIRCULAIRE : deux vehicules qui se nomment mutuellement.
                for (var i = 0; i < live.Count; i++)
                {
                    for (var j = i + 1; j < live.Count; j++)
                    {
                        if (!NamesAsLeader(live[i], live[j]) || !NamesAsLeader(live[j], live[i])) continue;
                        circularWaits.Add(Trace(live[i]) + "  <->  " + Trace(live[j]));
                    }
                }
            }

            var longStall = stalledSince
                .Where(entry => entry.Key != null && Time.time - entry.Value > 20f)
                .Select(entry => Trace(entry.Key) + " immobile depuis " + (Time.time - entry.Value).ToString("0.0") + " s")
                .ToList();

            Assert.That(stoppedInsideConflictArea, Is.Empty,
                "Des vehicules attendent une regle de jonction avec l'avant DANS l'aire de conflit :"
                + System.Environment.NewLine + string.Join(System.Environment.NewLine, stoppedInsideConflictArea.Take(5)));
            Assert.That(circularWaits, Is.Empty,
                "Attente circulaire : deux vehicules se declarent mutuellement chef de file."
                + System.Environment.NewLine + string.Join(System.Environment.NewLine, circularWaits.Take(5)));
            Assert.That(unblockingAtJunction, Is.Empty,
                "Le trafic ordinaire a eu besoin du deblocage d'intersection pour passer :"
                + System.Environment.NewLine + string.Join(System.Environment.NewLine, unblockingAtJunction.Take(5)));
            Assert.That(longStall, Is.Empty,
                "Un vehicule est reste immobile plus de 20 s :"
                + System.Environment.NewLine + string.Join(System.Environment.NewLine, longStall.Take(5)));
        }

        private static bool NamesAsLeader(NetworkedAIVehicleDriverController vehicle, NetworkedAIVehicleDriverController other)
        {
            if (vehicle == null || other == null) return false;
            var following = vehicle.DecisionReason == TrafficDecisionReason.FollowingSameLane
                || vehicle.DecisionReason == TrafficDecisionReason.TrafficQueue;
            return following && vehicle.DecisionSubject == other.name;
        }

        private static string Trace(NetworkedAIVehicleDriverController vehicle)
        {
            if (vehicle == null) return "(absent)";
            return vehicle.name + " [" + vehicle.DecisionReason + "] refus=" + vehicle.TrafficRefusal
                + " ligne=" + vehicle.JunctionStopGap.ToString("0.00") + "m"
                + " permission=" + vehicle.JunctionPermission
                + " maintien=" + vehicle.JunctionStopHeldSeconds.ToString("0.00") + "s"
                + " v=" + vehicle.PlanarSpeed.ToString("0.00")
                + " engage=" + vehicle.CommittedJunctionKey
                + " detail=" + vehicle.DecisionDetail;
        }

        private static void ClickSerializedButton(Component screen, string fieldName)
        {
            Assert.That(screen, Is.Not.Null);
            var button = GetPrivateField(screen, fieldName) as Button;
            Assert.That(button, Is.Not.Null, "bouton introuvable : " + fieldName);
            button.onClick.Invoke();
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }
    }
}
