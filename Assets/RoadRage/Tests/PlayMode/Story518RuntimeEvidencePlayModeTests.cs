using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
    /// ANO-5.18-04 : RELEVE, pas verdict. Le banc laisse le district tourner 75 s -- la duree de la
    /// video de recette -- puis ecrit deux preuves que le code source seul ne donne pas :
    ///
    /// 1. pour chaque virage effectivement parcouru : vitesse d'entree, rayon demande par le
    ///    connecteur, rayon reellement decrit, ecart lateral maximal a la courbe voulue, distance la
    ///    plus courte a un axe de voie oppose ;
    /// 2. pour chaque vehicule immobile : son bloc de decision complet, puis le graphe d'attente
    ///    reconstruit de proche en proche avec detection de cycle.
    ///
    /// Le fichier part dans le dossier temporaire de la machine ; rien n'est assere ici.
    /// </summary>
    [Category("Story518Evidence")]
    public sealed class Story518RuntimeEvidencePlayModeTests
    {
        private const int SceneLoadFrameBudget = 300;
        private const int TrafficSpawnFrameBudget = 900;
        private const int ObservationStepBudget = 7500; // 150 s a 50 Hz

        private string originalProfileFilePath;
        private string tempProfileFilePath;

        private sealed class TurnTrace
        {
            public string Vehicle;
            public Vector3 Entry, EntryForward, Exit, ExitForward;
            public readonly List<Vector3> Points = new List<Vector3>();
            public readonly List<float> Speeds = new List<float>();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (originalProfileFilePath != null)
            {
                PlayerProfileFileStore.DefaultFilePath = originalProfileFilePath;
                originalProfileFilePath = null;
            }
            if (tempProfileFilePath != null && File.Exists(tempProfileFilePath)) File.Delete(tempProfileFilePath);
            tempProfileFilePath = null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator DumpTurnTrajectoriesAndWaitGraph()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(),
                "roadrage-story518-evidence-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;

            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;

            ClickSerializedButton(Object.FindAnyObjectByType<MainMenuScreen>(), "playButton");
            yield return null;
            var lobby = Object.FindAnyObjectByType<LobbyShellScreen>();
            if (lobby == null) { Assert.Inconclusive("LobbyShellScreen absent."); yield break; }

            ClickSerializedButton(lobby, "startGameButton");
            for (var frame = 0; SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frame < SceneLoadFrameBudget; frame++)
            {
                var bootstrap = RoadRageBootstrap.Instance;
                if (bootstrap != null && bootstrap.LobbyRoom != null && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                    && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                {
                    Assert.Inconclusive("Services Steam indisponibles.");
                    yield break;
                }
                yield return null;
            }
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));

            List<NetworkedAIVehicleDriverController> traffic = null;
            for (var frame = 0; frame < TrafficSpawnFrameBudget; frame++)
            {
                traffic = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsSortMode.None)
                    .Where(c => c.IsSpawned).ToList();
                if (traffic.Count >= 3) break;
                yield return null;
            }
            if (traffic == null || traffic.Count < 2) { Assert.Inconclusive("District non peuple."); yield break; }

            var graph = Object.FindAnyObjectByType<LaneGraph>();
            Assert.That(graph, Is.Not.Null);

            var axes = new List<(Vector3 point, Vector3 forward)>();
            for (var i = 0; i < graph.NodeCount; i++)
            {
                var forward = Vector3.ProjectOnPlane(graph.GetNodeRotation(i) * Vector3.forward, Vector3.up);
                if (forward.sqrMagnitude > 0.0001f) axes.Add((graph.GetNodePosition(i), forward.normalized));
            }

            var open = new Dictionary<NetworkedAIVehicleDriverController, TurnTrace>();
            var finished = new List<TurnTrace>();
            var stillSince = new Dictionary<NetworkedAIVehicleDriverController, float>();
            var report = new StringBuilder();

            for (var step = 0; step < ObservationStepBudget; step++)
            {
                yield return new WaitForFixedUpdate();
                var live = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsSortMode.None)
                    .Where(c => c != null && c.IsSpawned).ToList();

                foreach (var vehicle in live)
                {
                    if (vehicle.TryGetCommittedMovement(out var entry, out var entryForward, out var exit, out var exitForward))
                    {
                        if (!open.TryGetValue(vehicle, out var trace))
                        {
                            trace = new TurnTrace
                            {
                                Vehicle = vehicle.name, Entry = entry, EntryForward = entryForward,
                                Exit = exit, ExitForward = exitForward
                            };
                            open[vehicle] = trace;
                        }
                        if (trace.Points.Count == 0
                            || Vector3.Distance(trace.Points[trace.Points.Count - 1], vehicle.transform.position) > 0.25f)
                        {
                            trace.Points.Add(vehicle.transform.position);
                            trace.Speeds.Add(vehicle.PlanarSpeed);
                        }
                    }
                    else if (open.TryGetValue(vehicle, out var done))
                    {
                        open.Remove(vehicle);
                        if (done.Points.Count >= 4) finished.Add(done);
                    }

                    if (vehicle.PlanarSpeed <= 0.2f)
                    {
                        if (!stillSince.ContainsKey(vehicle)) stillSince[vehicle] = Time.time;
                    }
                    else stillSince.Remove(vehicle);
                }
            }

            // ---------------------------------------------------------------- 1. virages parcourus
            report.AppendLine("=== VIRAGES PARCOURUS (" + finished.Count + ") ===");
            foreach (var trace in finished.Take(24))
            {
                var angle = Vector3.SignedAngle(
                    Vector3.ProjectOnPlane(trace.EntryForward, Vector3.up),
                    Vector3.ProjectOnPlane(trace.ExitForward, Vector3.up), Vector3.up);
                var demanded = ConnectorMinRadius(trace);
                var deviation = 0f;
                var deviationAt = Vector3.zero;
                var opposite = float.PositiveInfinity;
                var oppositeAt = Vector3.zero;
                for (var i = 0; i < trace.Points.Count; i++)
                {
                    var point = trace.Points[i];
                    // La trace couvre TOUT le mouvement engage, donc elle commence avant la courbe et
                    // finit apres elle (l'engagement ne se relache qu'une fois l'arriere sorti). Les
                    // echantillons qui se projettent sur une EXTREMITE de la courbe ne mesurent pas un
                    // ecart de suivi mais la distance a son bout : les compter donnait 2,30 m d'ecart
                    // a 1,74 m/s, ce qu'aucun virage ne produit.
                    var parameter = LaneGraphRouting.ResolveJunctionTurnProgress(trace.Entry, trace.EntryForward,
                        trace.Exit, trace.ExitForward, point, 64);
                    if (parameter <= 0.02f || parameter >= 0.98f) continue;
                    var closest = ClosestOnConnector(trace, point);
                    var offset = Vector3.ProjectOnPlane(point - closest, Vector3.up).magnitude;
                    if (offset > deviation) { deviation = offset; deviationAt = point; }

                    var tangent = i + 1 < trace.Points.Count
                        ? Vector3.ProjectOnPlane(trace.Points[i + 1] - point, Vector3.up)
                        : Vector3.ProjectOnPlane(trace.ExitForward, Vector3.up);
                    if (tangent.sqrMagnitude <= 0.0001f) continue;
                    tangent.Normalize();
                    foreach (var axis in axes)
                    {
                        if (Vector3.Dot(axis.forward, tangent) > -0.7f) continue;
                        var offsetToAxis = Vector3.ProjectOnPlane(point - axis.point, Vector3.up);
                        var lateral = Vector3.ProjectOnPlane(offsetToAxis - axis.forward * Vector3.Dot(offsetToAxis, axis.forward), Vector3.up).magnitude;
                        var along = Vector3.Dot(offsetToAxis, axis.forward);
                        if (Mathf.Abs(along) > 14f) continue; // axe hors de portee de ce virage
                        if (lateral < opposite) { opposite = lateral; oppositeAt = point; }
                    }
                }

                var described = TraceRadius(trace);
                report.AppendLine(trace.Vehicle + "  " + (angle > 0f ? "gauche " : "droite ") + angle.ToString("F0") + " deg"
                    + "  P0=" + F(trace.Entry) + " P2=" + F(trace.Exit));
                report.AppendLine("   v_entree=" + trace.Speeds[0].ToString("F2") + " m/s  v_max="
                    + trace.Speeds.Max().ToString("F2") + "  v_min=" + trace.Speeds.Min().ToString("F2"));
                report.AppendLine("   rayon_demande=" + demanded.ToString("F2") + " m  rayon_decrit="
                    + described.ToString("F2") + " m  a_lat_demandee="
                    + (trace.Speeds[0] * trace.Speeds[0] / Mathf.Max(0.01f, demanded)).ToString("F1") + " m/s2");
                report.AppendLine("   ecart_max_a_la_courbe=" + deviation.ToString("F2") + " m en " + F(deviationAt));
                report.AppendLine("   axe_oppose_le_plus_proche=" + opposite.ToString("F2") + " m en " + F(oppositeAt));
            }

            // ---------------------------------------------------------------- 2. graphe d'attente
            var stalled = stillSince.Where(e => e.Key != null && Time.time - e.Value > 5f)
                .OrderByDescending(e => Time.time - e.Value).ToList();
            report.AppendLine();
            report.AppendLine("=== VEHICULES IMMOBILES (" + stalled.Count + ") ===");
            foreach (var entry in stalled)
            {
                report.AppendLine("--- immobile depuis " + (Time.time - entry.Value).ToString("F1") + " s  pos="
                    + F(entry.Key.transform.position));
                report.AppendLine(entry.Key.DescribeDecision());
                report.AppendLine();
            }

            report.AppendLine("=== GRAPHE D'ATTENTE ===");
            foreach (var entry in stalled)
            {
                var chain = new List<string>();
                var seen = new HashSet<NetworkedAIVehicleDriverController>();
                var cursor = entry.Key;
                var cycle = false;
                while (cursor != null && chain.Count < 12)
                {
                    if (!seen.Add(cursor)) { cycle = true; break; }
                    chain.Add(cursor.name + " [" + cursor.DecisionReason + "]");
                    cursor = cursor.WaitsFor;
                }
                if (cursor != null) chain.Add(cursor.name + (cycle ? " <== CYCLE" : string.Empty));
                report.AppendLine(string.Join("  ->  ", chain));
            }

            var path = Path.Combine(Path.GetTempPath(), "story518-evidence.txt");
            File.WriteAllText(path, report.ToString());
            Debug.Log("[ANO-5.18-04] releve ecrit dans " + path);

            // CRITERE DE RECETTE : aucun vehicule ne se bloque. Un arret de feu dure un cycle ; au-dela
            // de 15 s d'immobilite continue, ce n'est plus une attente, c'est un blocage.
            var blocked = stalled
                .Where(entry => Time.time - entry.Value > 15f)
                .Select(entry => (Time.time - entry.Value).ToString("0.0") + " s : " + entry.Key.DescribeDecision())
                .ToList();
            Assert.That(blocked, Is.Empty, "Des vehicules sont bloques :"
                + System.Environment.NewLine + string.Join(System.Environment.NewLine, blocked.Take(4)));
        }

        private static Vector3 ClosestOnConnector(TurnTrace trace, Vector3 point)
        {
            var best = trace.Entry;
            var bestSquared = float.PositiveInfinity;
            for (var i = 0; i <= 64; i++)
            {
                var candidate = LaneGraphRouting.ResolveJunctionTurnPoint(trace.Entry, trace.EntryForward,
                    trace.Exit, trace.ExitForward, i / 64f);
                var squared = Vector3.ProjectOnPlane(candidate - point, Vector3.up).sqrMagnitude;
                if (squared >= bestSquared) continue;
                bestSquared = squared;
                best = candidate;
            }
            return best;
        }

        private static float ConnectorMinRadius(TurnTrace trace)
        {
            var minimum = float.PositiveInfinity;
            for (var i = 1; i < 32; i++)
            {
                var before = LaneGraphRouting.ResolveJunctionTurnPoint(trace.Entry, trace.EntryForward, trace.Exit, trace.ExitForward, (i - 1) / 32f);
                var point = LaneGraphRouting.ResolveJunctionTurnPoint(trace.Entry, trace.EntryForward, trace.Exit, trace.ExitForward, i / 32f);
                var after = LaneGraphRouting.ResolveJunctionTurnPoint(trace.Entry, trace.EntryForward, trace.Exit, trace.ExitForward, (i + 1) / 32f);
                var radius = CircleRadius(before, point, after);
                if (radius > 0.01f && radius < minimum) minimum = radius;
            }
            return minimum;
        }

        private static float TraceRadius(TurnTrace trace)
        {
            var minimum = float.PositiveInfinity;
            for (var i = 1; i + 1 < trace.Points.Count; i++)
            {
                var radius = CircleRadius(trace.Points[i - 1], trace.Points[i], trace.Points[i + 1]);
                if (radius > 0.01f && radius < minimum) minimum = radius;
            }
            return minimum;
        }

        private static float CircleRadius(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = Vector3.ProjectOnPlane(b - a, Vector3.up);
            var bc = Vector3.ProjectOnPlane(c - b, Vector3.up);
            var ca = Vector3.ProjectOnPlane(a - c, Vector3.up);
            var area = Mathf.Abs(ab.x * ca.z - ca.x * ab.z) * 0.5f;
            if (area < 0.000001f) return float.PositiveInfinity;
            return ab.magnitude * bc.magnitude * ca.magnitude / (4f * area);
        }

        private static string F(Vector3 v)
        {
            return "(" + v.x.ToString("F2") + "," + v.z.ToString("F2") + ")";
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
