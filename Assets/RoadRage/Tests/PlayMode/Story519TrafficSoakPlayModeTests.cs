using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// BANC DE SOAK (revue post-5.18). Le district tourne longtemps, sans mise en scene, et le banc
    /// ENREGISTRE. Il n'ecrit jamais une pose, une vitesse ni une intention.
    ///
    /// Sa raison d'etre : toute la preuve 5.17/5.18 est au niveau du modele (EditMode sur la
    /// geometrie reelle). Le critere de recette -- "aucun vehicule ne se bloque sur un run complet"
    /// -- n'avait jamais ete mesure en conditions reelles.
    ///
    /// Le rapport est ECRIT SUR DISQUE AVANT toute assertion : un banc qui echoue doit livrer ses
    /// mesures, sinon l'echec n'apprend rien. Le chemin est trace dans la Console.
    /// </summary>
    [Category("Story519")]
    public sealed class Story519TrafficSoakPlayModeTests
    {
        private const int SceneLoadFrameBudget = 300;
        private const int TrafficSpawnFrameBudget = 900;

        /// <summary>Duree simulee. Le palier 4 de l'echelle s'atteint a 38 s : il faut plusieurs fois cela.</summary>
        private const float SoakSeconds = 180f;

        /// <summary>Seuil d'immobilite, aligne sur stuckSpeedThreshold du controleur.</summary>
        private const float StillSpeed = 0.2f;

        /// <summary>Un arret plus long que ceci n'est plus explicable par le trafic nominal.</summary>
        private const float StuckSeconds = 45f;

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

        private sealed class Track
        {
            public string Name;
            public float StillSince = -1f;
            public float WorstStill;
            public string WorstStillTrace = string.Empty;
            public float WrongWaySeconds;
            public float MaxLateral;
            public int MaxStage;
            public readonly Dictionary<int, float> StageSeconds = new Dictionary<int, float>();
            public readonly Dictionary<TrafficDecisionReason, float> ReasonSeconds =
                new Dictionary<TrafficDecisionReason, float>();
            public int Breaches;
            public int Episodes;
            public bool EpisodeOpen;
            public float RingStillSeconds;
            public float LastSeen;
        }

        // Le banc dure volontairement plus que le defaut NUnit (180 s) : l'echelle de recuperation
        // atteint son dernier palier a 38 s, donc observer moins ne prouverait rien.
        [UnityTest]
        [Timeout(900000)]
        public IEnumerator TheDistrictRunsLongEnoughToProveNoVehicleGetsPermanentlyStuck()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(),
                "roadrage-story519-soak-" + System.Guid.NewGuid().ToString("N") + ".json");
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
                    Assert.Inconclusive("Services Steam indisponibles : banc de soak non executable.");
                    yield break;
                }

                yield return null;
            }

            if (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName)
            {
                Assert.Inconclusive("MVP_Run n'a pas ete atteint : banc non executable.");
                yield break;
            }

            for (var frame = 0; frame < TrafficSpawnFrameBudget; frame++)
            {
                var seen = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsSortMode.None)
                    .Count(controller => controller.IsSpawned);
                if (seen >= 3) break;
                yield return null;
            }

            var graph = Object.FindAnyObjectByType<LaneGraph>();
            if (graph == null)
            {
                Assert.Inconclusive("LaneGraph absent de MVP_Run.");
                yield break;
            }

            var tracks = new Dictionary<string, Track>();
            var circular = new List<string>();
            var mutualYield = new List<string>();
            var despawned = new HashSet<string>();
            var everSeen = new HashSet<string>();
            var reasonTotals = new Dictionary<TrafficDecisionReason, float>();
            var start = Time.time;
            var peakLive = 0;

            while (Time.time - start < SoakSeconds)
            {
                yield return null;
                var dt = Time.deltaTime;
                if (dt <= 0f) continue;

                var live = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsSortMode.None)
                    .Where(controller => controller != null && controller.IsSpawned).ToList();
                peakLive = Mathf.Max(peakLive, live.Count);

                foreach (var vehicle in live)
                {
                    everSeen.Add(vehicle.name);
                    if (!tracks.TryGetValue(vehicle.name, out var track))
                    {
                        track = new Track { Name = vehicle.name };
                        tracks[vehicle.name] = track;
                    }

                    track.LastSeen = Time.time;

                    var reason = vehicle.DecisionReason;
                    Accumulate(track.ReasonSeconds, reason, dt);
                    Accumulate(reasonTotals, reason, dt);

                    var stage = vehicle.RecoveryStage;
                    track.MaxStage = Mathf.Max(track.MaxStage, stage);
                    if (!track.StageSeconds.ContainsKey(stage)) track.StageSeconds[stage] = 0f;
                    track.StageSeconds[stage] += dt;
                    if (stage > 0 && !track.EpisodeOpen) { track.EpisodeOpen = true; track.Episodes++; }
                    if (stage == 0) track.EpisodeOpen = false;

                    if (vehicle.JunctionBreached) track.Breaches++;

                    // Ecart a l'axe routier et sens de circulation : la semantique LaneGraph fait foi.
                    if (graph.TryGetRoadPosition(vehicle.transform.position, out _, out var axis, out var lateral))
                    {
                        track.MaxLateral = Mathf.Max(track.MaxLateral, lateral);
                        var heading = Vector3.ProjectOnPlane(vehicle.transform.forward, Vector3.up);
                        if (heading.sqrMagnitude > 0.0001f && Vector3.Dot(heading.normalized, axis) < 0f)
                        {
                            track.WrongWaySeconds += dt;
                        }
                    }

                    var still = vehicle.PlanarSpeed <= StillSpeed;
                    if (still)
                    {
                        if (track.StillSince < 0f) track.StillSince = Time.time;
                        var held = Time.time - track.StillSince;
                        if (held > track.WorstStill)
                        {
                            track.WorstStill = held;
                            track.WorstStillTrace = Trace(vehicle);
                        }

                        if (vehicle.RingId > 0) track.RingStillSeconds += dt;
                    }
                    else
                    {
                        track.StillSince = -1f;
                    }
                }

                for (var i = 0; i < live.Count; i++)
                {
                    for (var j = i + 1; j < live.Count; j++)
                    {
                        if (NamesAsLeader(live[i], live[j]) && NamesAsLeader(live[j], live[i]))
                        {
                            Remember(circular, Trace(live[i]) + "  <->  " + Trace(live[j]));
                        }

                        if (live[i].IsYielding && live[j].IsYielding
                            && live[i].ConflictPartner == live[j] && live[j].ConflictPartner == live[i])
                        {
                            Remember(mutualYield, Trace(live[i]) + "  <->  " + Trace(live[j]));
                        }
                    }
                }

                var names = new HashSet<string>(live.Select(v => v.name));
                foreach (var name in everSeen)
                {
                    if (!names.Contains(name)) despawned.Add(name);
                }
            }

            var report = Compose(tracks, circular, mutualYield, despawned, everSeen, reasonTotals,
                peakLive, Time.time - start);
            var path = Path.Combine(Application.temporaryCachePath, "story519-soak-report.txt");
            File.WriteAllText(path, report);
            Debug.Log("[Soak] rapport ecrit : " + path + "\n" + report);

            var stuck = tracks.Values
                .Where(t => t.WorstStill >= StuckSeconds)
                .Select(t => t.Name + " immobile " + t.WorstStill.ToString("0.0") + " s -- " + t.WorstStillTrace)
                .ToList();

            Assert.That(everSeen.Count, Is.GreaterThanOrEqualTo(2),
                "Moins de deux vehicules observes : le district n'a pas peuple ses portails.");
            Assert.That(stuck, Is.Empty,
                "Des vehicules sont restes bloques (>= " + StuckSeconds + " s) :"
                + System.Environment.NewLine + string.Join(System.Environment.NewLine, stuck.Take(8))
                + System.Environment.NewLine + "Rapport complet : " + path);
            Assert.That(circular, Is.Empty,
                "Attente circulaire observee :" + System.Environment.NewLine
                + string.Join(System.Environment.NewLine, circular.Take(5)));
            Assert.That(mutualYield, Is.Empty,
                "Cession mutuelle observee (les deux cedent le meme point) :" + System.Environment.NewLine
                + string.Join(System.Environment.NewLine, mutualYield.Take(5)));
        }

        private static string Compose(Dictionary<string, Track> tracks, List<string> circular,
            List<string> mutualYield, HashSet<string> despawned, HashSet<string> everSeen,
            Dictionary<TrafficDecisionReason, float> reasonTotals, int peakLive, float elapsed)
        {
            var text = new StringBuilder();
            text.AppendLine("=== SOAK TRAFIC -- district MVP_Run ===");
            text.AppendLine("duree simulee      : " + elapsed.ToString("0.0") + " s");
            text.AppendLine("vehicules vus      : " + everSeen.Count + " (pic simultane " + peakLive + ")");
            text.AppendLine("sorties (despawn)  : " + despawned.Count);
            text.AppendLine();

            text.AppendLine("--- temps par etat de decision (toutes voitures cumulees) ---");
            foreach (var entry in reasonTotals.OrderByDescending(e => e.Value))
            {
                text.AppendLine("  " + entry.Key.ToString().PadRight(24) + entry.Value.ToString("0.0") + " s");
            }

            text.AppendLine();
            text.AppendLine("--- par vehicule ---");
            foreach (var track in tracks.Values.OrderByDescending(t => t.WorstStill))
            {
                text.AppendLine(track.Name);
                text.AppendLine("   arret continu max : " + track.WorstStill.ToString("0.0") + " s");
                if (track.WorstStill > 5f) text.AppendLine("      au pire : " + track.WorstStillTrace);
                text.AppendLine("   palier max        : " + track.MaxStage + "   episodes : " + track.Episodes);
                text.AppendLine("   contresens        : " + track.WrongWaySeconds.ToString("0.0") + " s");
                text.AppendLine("   ecart axe max     : " + track.MaxLateral.ToString("0.00") + " m");
                text.AppendLine("   immobile sur anneau : " + track.RingStillSeconds.ToString("0.0") + " s");
                if (track.Breaches > 0) text.AppendLine("   deblocages        : " + track.Breaches);
                var top = track.ReasonSeconds.OrderByDescending(e => e.Value).Take(4)
                    .Select(e => e.Key + " " + e.Value.ToString("0.0") + "s");
                text.AppendLine("   etats dominants   : " + string.Join(", ", top));
            }

            text.AppendLine();
            text.AppendLine("--- attentes circulaires (" + circular.Count + ") ---");
            foreach (var line in circular.Take(20)) text.AppendLine("  " + line);
            text.AppendLine("--- cessions mutuelles (" + mutualYield.Count + ") ---");
            foreach (var line in mutualYield.Take(20)) text.AppendLine("  " + line);
            return text.ToString();
        }

        private static void Accumulate<T>(Dictionary<T, float> map, T key, float dt)
        {
            if (!map.ContainsKey(key)) map[key] = 0f;
            map[key] += dt;
        }

        private static void Remember(List<string> sink, string line)
        {
            if (sink.Count < 200 && !sink.Contains(line)) sink.Add(line);
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
            return vehicle.name + " [" + vehicle.DecisionReason + "]"
                + " refus=" + vehicle.TrafficRefusal
                + " motif=" + vehicle.ConflictVerdictReason
                + " palier=" + vehicle.RecoveryStage
                + " sansProgres=" + vehicle.StalledSeconds.ToString("0.0") + "s"
                + " borne=" + vehicle.HeldByBoundedWait
                + " voie=" + vehicle.CurrentLaneNode
                + " anneau=" + vehicle.RingId
                + " ligne=" + vehicle.JunctionStopGap.ToString("0.00") + "m"
                + " perm=" + vehicle.JunctionPermission
                + " pair=" + (vehicle.ConflictPartner != null ? vehicle.ConflictPartner.name : "-");
        }

        private static void ClickSerializedButton(Object screen, string fieldName)
        {
            if (screen == null) return;
            var field = screen.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var button = field != null ? field.GetValue(screen) as Button : null;
            if (button != null) button.onClick.Invoke();
        }
    }
}

