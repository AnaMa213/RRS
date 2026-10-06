using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.MainMenu;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Harness PlayMode des scenarios de la Story 5.33 dans MVP_Run. Scenarios lus dans le fichier ecrit par le
    /// constructeur EditMode (Story533SharedFrameTests) ; bootstrap -> lobby -> run comme 5.31/5.52 ; joueur hote
    /// stationne sur une plateforme de test loin des routes ; obstacles crees a l'execution et detruits a la fin (la
    /// scene n'est jamais sauvegardee) ; invariants controles a chaque pas ; traces brutes et resume publies.
    /// </summary>
    internal sealed class Story533Harness
    {
        public const string Folder = "_bmad-output/implementation-artifacts/traffic-v2-5-33-explorations";
        public const string ScenarioPath = Folder + "/scenarios-5-33.json";

        [Serializable]
        public sealed class VectorRecord
        {
            public float x, y, z;
            public Vector3 Value { get { return new Vector3(x, y, z); } }
        }

        [Serializable]
        public sealed class InsertionRecord
        {
            public string Entry;
            public string Exit;
            public ulong Seed;
            public ulong EarliestStep;
            public string[] Elements;
        }

        [Serializable]
        public sealed class ObstacleRecord
        {
            public string Label;
            public string ElementId;
            public float RouteDistanceMeters;
            public VectorRecord Center;
            public VectorRecord Forward;
            public VectorRecord Size;
            public int FromStep;
            public int UntilStep;
        }

        [Serializable]
        public sealed class PushRecord
        {
            public int Insertion;
            public int AtStep;
            public float LateralImpulsePerKilogram;
        }

        [Serializable]
        public sealed class ScenarioRecord
        {
            public string Label;
            public int MaxPopulation;
            public int MaxSteps;
            public InsertionRecord[] Insertions;
            public ObstacleRecord[] Obstacles;
            public PushRecord[] Pushes;
        }

        [Serializable]
        public sealed class ScenarioFile
        {
            public int Format;
            public string RoadModelVersion;
            public float VehicleLengthMeters;
            public float MinimumGapMeters;
            public float MinimumObstacleDistanceMeters;
            public VectorRecord Parking;
            public ScenarioRecord[] Scenarios;
        }

        /// <param name="path">Fichier de scenarios ; celui de la 5.33 par defaut (la 5.34 ecrit le sien).</param>
        public static ScenarioRecord Load(TrafficV2Admission admission, string label, out ScenarioFile file, string path = ScenarioPath)
        {
            Assert.That(File.Exists(path), Is.True, "fichier de scenarios absent : lancer d'abord le constructeur EditMode (" + path + ")");
            file = JsonUtility.FromJson<ScenarioFile>(File.ReadAllText(path));
            Assert.That(file.RoadModelVersion, Is.EqualTo(admission.Model.Version.ToString()), "fichier de scenarios perime");
            var record = file.Scenarios.FirstOrDefault(s => s.Label == label);
            Assert.That(record, Is.Not.Null, "scenario " + label);
            return record;
        }

        public static TrafficV2Scenario ToScenario(ScenarioRecord record)
        {
            return new TrafficV2Scenario(record.Label, record.MaxPopulation, record.Insertions.Select(i => new ScenarioInsertion(
                RoadId.Parse(i.Entry), RoadId.Parse(i.Exit), i.Seed, i.EarliestStep)).ToArray());
        }

        // ------------------------------------------------------------------ session

        private string originalProfilePath;
        private string tempProfilePath;
        private readonly List<GameObject> created = new List<GameObject>();

        /// <summary>Bootstrap -> menu -> lobby -> Start Game ; Inconclusif sans services en ligne (patron 5.31/5.52).</summary>
        public IEnumerator EnterMvpRun()
        {
            originalProfilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfilePath = Path.Combine(Path.GetTempPath(), "roadrage-story533-" + Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfilePath;
            // Le premier CreateLobbyAsync Steam apres un redemarrage de l'Editeur a depasse 3 s, puis echoue a 10 s
            // (runs du 2026-10-02) ; l'appel suivant repond en ~0,3 s. Le scenario verifie le trafic, pas le lobby :
            // une seconde tentative journalisee, budget en temps reel.
            bool lobbyFailed = false;
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                lobbyFailed = false;
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
                    float deadline = Time.realtimeSinceStartup + 30f;
                    while (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && Time.realtimeSinceStartup < deadline)
                    {
                        var bootstrap = RoadRageBootstrap.Instance;
                        if (bootstrap != null && bootstrap.LobbyRoom != null
                            && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                            && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating
                            && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                        {
                            lobbyFailed = true;
                            break;
                        }
                        yield return null;
                    }
                }
                finally { LogAssert.ignoreFailingMessages = false; }
                if (SceneManager.GetActiveScene().name == AppSceneRouter.MvpRunSceneName || attempt == 2) break;
                Debug.Log("[Story533] entree MVP_Run : tentative 1 en echec (" + (lobbyFailed ? "lobby refuse" : "delai 30 s")
                    + "), seconde tentative.");
                yield return ShutdownSession();
            }
            if (lobbyFailed) Assert.Inconclusive("Services en ligne indisponibles apres deux tentatives : scenario MVP_Run non verifiable.");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));
            Assert.That(NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer, Is.True, "le pair local est l'hote du run");
        }

        public IEnumerator Cleanup()
        {
            foreach (var go in created) if (go != null) Object.Destroy(go);
            created.Clear();
            TrafficV2Session.Reset();
            yield return ShutdownSession();
            if (originalProfilePath != null) PlayerProfileFileStore.DefaultFilePath = originalProfilePath;
            if (tempProfilePath != null && File.Exists(tempProfilePath)) File.Delete(tempProfilePath);
            originalProfilePath = null;
            tempProfilePath = null;
        }

        private static IEnumerator ShutdownSession()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                if (manager.IsListening) manager.Shutdown();
                Object.Destroy(manager.gameObject);
            }
            if (RoadRageBootstrap.Instance != null) Object.Destroy(RoadRageBootstrap.Instance.gameObject);
            yield return null;
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

        // ------------------------------------------------------------------ monde de test

        /// <summary>
        /// Joueur hote stationne a la pose declaree, sur une plateforme statique creee par le test, loin de tout couloir
        /// balaye et a plus du rayon du collecteur des routes du scenario. Rend le nombre de joueurs deplaces.
        /// </summary>
        public IEnumerator ParkHostPlayer(Vector3 parking, Action<int> moved)
        {
            CharacterController[] players = new CharacterController[0];
            for (int frame = 0; frame < 120 && players.Length == 0; frame++)
            {
                players = Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Exclude);
                if (players.Length == 0) yield return null;
            }
            var platform = new GameObject("Story533_Parking");
            created.Add(platform);
            platform.transform.position = parking - Vector3.up * 0.5f;
            platform.AddComponent<BoxCollider>().size = new Vector3(30f, 1f, 30f);
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                player.enabled = false;
                player.transform.position = parking + new Vector3(3f * i, 1.2f, 0f);
                player.enabled = true;
            }
            Physics.SyncTransforms();
            moved(players.Length);
        }

        /// <summary>Distance minimale du joueur hote aux echantillons des routes du scenario (m) ; +inf sans joueur.</summary>
        public static float PlayerDistanceToRoutes(CompiledRoadModel model, ScenarioRecord scenario)
        {
            var players = Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Exclude);
            float best = float.PositiveInfinity;
            var elements = new HashSet<string>(scenario.Insertions.SelectMany(i => i.Elements));
            foreach (var player in players)
                foreach (var id in elements)
                {
                    EffectiveLaneCorridor corridor;
                    CompiledJunctionMovement movement;
                    IReadOnlyList<RoadCurveSample> samples = model.TryGetCorridor(RoadId.Parse(id), out corridor) ? corridor.Samples
                        : model.TryGetMovement(RoadId.Parse(id), out movement) ? movement.Samples : null;
                    if (samples == null) continue;
                    foreach (var sample in samples) best = Math.Min(best, Vector3.Distance(player.transform.position, sample.Position));
                }
            return best;
        }

        /// <summary>Obstacle cinematique de test : BoxCollider et Rigidbody cinematique, sans aucun composant vehicule.</summary>
        public GameObject CreateObstacle(ObstacleRecord record)
        {
            var go = new GameObject("Story533_Obstacle_" + record.Label);
            created.Add(go);
            go.transform.SetPositionAndRotation(record.Center.Value, Quaternion.LookRotation(record.Forward.Value, Vector3.up));
            go.AddComponent<BoxCollider>().size = record.Size.Value;
            var body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            Physics.SyncTransforms();
            return go;
        }

        public void Destroy(GameObject go)
        {
            if (go == null) return;
            created.Remove(go);
            Object.Destroy(go);
        }

        public static TrafficV2VehicleDriver DriverOf(PortalTrafficSpawner spawner, RoadId trafficId)
        {
            foreach (var networkObject in spawner.LiveV2Vehicles)
            {
                var driver = networkObject.GetComponent<TrafficV2VehicleDriver>();
                if (driver != null && driver.TrafficId == trafficId) return driver;
            }
            return null;
        }

        // ------------------------------------------------------------------ invariants par pas

        /// <summary>
        /// Invariants controles apres chaque pas physique : un pas hote et au plus une frame par pas, un intent et un seul
        /// par vehicule lie, decisions sur la frame du pas, valeurs finies, population bornee, retrait au portail seulement,
        /// joueur hote absent de tout fait V2.
        /// </summary>
        public sealed class Observer
        {
            private readonly PortalTrafficSpawner spawner;
            private readonly int maxPopulation;
            private ulong lastFrameId;
            private int lastFramesBuilt;
            private readonly Dictionary<RoadId, int> intents = new Dictionary<RoadId, int>();
            public readonly List<string> Violations = new List<string>();
            public int Steps;
            public int MaxLive;
            public int PlayerFacts;
            public int CollectorSaturatedSteps;
            public int MaxCollectorHits;
            public readonly Dictionary<PerceptionUnavailableReason, int> Unavailable = new Dictionary<PerceptionUnavailableReason, int>();
            /// <summary>
            /// Premier verrou TrackingToleranceExceeded (decision 2a, 5.52) : point de contamination de la scene, faute du
            /// mecanisme de reprise 5.39. En scenario nominal c'est un echec immediat ; en exploratoire perturbe, un constat qui
            /// clot la fenetre d'observation fonctionnelle. Nul tant qu'aucun vehicule n'est verrouille.
            /// </summary>
            public string ToleranceLatch;
            public ulong ContaminationFrame;

            // Verdicts O6 de la Story 5.34 (la regle D12 de la 5.33 est retiree) : compteurs publies, assertes par les
            // scenarios 5.34 et les campagnes, jamais ajoutes aux violations des scenarios A et B de la 5.33.

            /// <summary>Lots du coordinateur observes (un par pas hote) et lots resolus sur une frame refusee.</summary>
            public int Batches, RefusedBatches;
            /// <summary>Pas ou deux grants effectifs incompatibles d'acteurs distincts coexistent : echec 5.34.</summary>
            public int IncompatibleGrantSteps;
            public string FirstIncompatibleGrants;
            /// <summary>Sommes des compteurs de lot ; EnteredWithoutGrant &gt; 0 est un echec 5.34.</summary>
            public long EnteredWithoutGrant, IncompatibleOccupancy;
            /// <summary>Cout des pas hote avec vehicule : pas Traffic V2 total et lot du coordinateur (ms).</summary>
            public readonly List<double> HostStepMilliseconds = new List<double>();
            public readonly List<double> CoordinatorMilliseconds = new List<double>();
            /// <summary>Pas hote en population pleine seulement : base du p95 D13, comme dans Story533Perf.</summary>
            public readonly List<double> FullPopulationHostStepMilliseconds = new List<double>();
            /// <summary>Story 5.35 : sommes des compteurs de lot des regles authorees (publiees, observationnelles).</summary>
            public long StopRequired, YieldToPriority, MergeGapGrants, DeadlockBreaks, CrossingRefusals, MergeGapRefusals;
            /// <summary>
            /// Story 5.35, cout par frontiere des pas hote avec vehicule : construction de la frame et pas des vehicules (ms), et
            /// travail de priorite du lot (preseances lues + creneaux evalues, compteurs de travail), dont le temps est compris
            /// dans celui du coordinateur.
            /// </summary>
            public readonly List<double> FrameBuildMilliseconds = new List<double>();
            public readonly List<double> DriveMilliseconds = new List<double>();
            public readonly List<double> PriorityWork = new List<double>();
            private long lastPriorityWork = -1L;
            /// <summary>Changements de decision du coordinateur par (vehicule, traversee), dans l'ordre des lots (borne).</summary>
            public readonly List<string> JunctionLog = new List<string>();
            private const int JunctionLogLimit = 20000;
            /// <summary>
            /// Constats 5.35, publies et non bloquants : a une fusion d'anneau, une entree servie avant une continuation. En 5.34
            /// l'ordre de service est le FIFO generique, jamais une priorite d'anneau.
            /// </summary>
            public readonly List<string> RingMergeFindings = new List<string>();
            private readonly Dictionary<string, string> lastDecision = new Dictionary<string, string>();
            private readonly HashSet<string> ringMergeSeen = new HashSet<string>();
            private Dictionary<RoadId, IReadOnlyList<RoadId>> zoneMembers;

            public Observer(PortalTrafficSpawner spawner, int maxPopulation, CompiledRoadModel model = null)
            {
                this.spawner = spawner;
                this.maxPopulation = maxPopulation;
                if (model != null) zoneMembers = model.ConflictZones.ToDictionary(z => z.Id, z => z.MemberMovementIds);
            }

            /// <summary>Lot publie a ce pas : compteurs, grants incompatibles simultanes, journal et constats de fusion d'anneau.</summary>
            private void ObserveJunctions(TrafficV2StepRunner runner)
            {
                var snapshot = runner.JunctionSnapshot;
                if (snapshot == null || runner.Coordinator == null || snapshot.SourceFrame != runner.FrameId) return;
                var index = runner.Coordinator.Index;
                if (zoneMembers == null) zoneMembers = index.Model.ConflictZones.ToDictionary(z => z.Id, z => z.MemberMovementIds);
                Batches++;
                if (!snapshot.Counters.FrameValid) RefusedBatches++;
                EnteredWithoutGrant += snapshot.Counters.EnteredWithoutGrant;
                IncompatibleOccupancy += snapshot.Counters.IncompatibleOccupancy;
                StopRequired += snapshot.Counters.StopRequired;
                YieldToPriority += snapshot.Counters.YieldToPriority;
                MergeGapGrants += snapshot.Counters.MergeGapGrants;
                DeadlockBreaks += snapshot.Counters.DeadlockBreaks;
                CrossingRefusals += snapshot.Counters.CrossingRefusals;
                MergeGapRefusals += snapshot.Counters.MergeGapRefusals;
                var effective = snapshot.Records.Where(r => r.IsEffectiveGrant).ToList();
                bool clash = false;
                for (int i = 0; i < effective.Count; i++)
                    for (int j = i + 1; j < effective.Count; j++)
                    {
                        if (effective[i].TrafficId == effective[j].TrafficId) continue;
                        foreach (var a in effective[i].MovementIds)
                            foreach (var b in effective[j].MovementIds)
                            {
                                RoadId zone;
                                ConflictKind kind;
                                float startA, startB;
                                if (!index.TryGetConflict(a, b, out zone, out kind, out startA, out startB)) continue;
                                // Invariant 5.34 amende (5.35) : un grant par creneau de fusion coexiste avec un grant incompatible,
                                // seulement sur une zone Merge.
                                if (kind == ConflictKind.Merge && (effective[i].MergeGap || effective[j].MergeGap)) continue;
                                if (!clash && FirstIncompatibleGrants == null)
                                    FirstIncompatibleGrants = "lot " + snapshot.SourceFrame + " : " + effective[i].ToText() + " / "
                                        + effective[j].ToText() + " zone " + zone;
                                clash = true;
                            }
                    }
                if (clash) IncompatibleGrantSteps++;
                foreach (var record in snapshot.Records)
                {
                    string key = record.TrafficId + "|" + record.TraversalId;
                    string value = record.Status + "(" + record.Reason + ")" + (record.CauseActorId.IsEmpty ? "" : " @" + record.CauseActorId)
                        + (record.ZoneId.IsEmpty ? "" : " zone " + record.ZoneId) + " " + record.MovementIds.Count;
                    string previous;
                    if (lastDecision.TryGetValue(key, out previous) && previous == value) continue;
                    lastDecision[key] = value;
                    if (JunctionLog.Count < JunctionLogLimit) JunctionLog.Add(record.ToText());
                    else if (JunctionLog.Count == JunctionLogLimit) JunctionLog.Add("journal tronque a " + JunctionLogLimit + " lignes");
                    if (record.Status == JunctionGrantStatus.Denied) RingMerge(record, snapshot, index);
                }
            }

            /// <summary>Fusion d'anneau : la continuation de la traversee refusee attend l'entree servie au titulaire.</summary>
            private void RingMerge(JunctionRecord denied, JunctionSnapshot snapshot, JunctionConflictIndex index)
            {
                IReadOnlyList<RoadId> members;
                if (denied.ZoneId.IsEmpty || denied.CauseActorId.IsEmpty || !zoneMembers.TryGetValue(denied.ZoneId, out members)
                    || members.Count != 2) return;
                RoadId mine = denied.Contains(members[0]) ? members[0] : members[1];
                RoadId other = mine == members[0] ? members[1] : members[0];
                if (!denied.Contains(mine)) return;
                bool continuation = !index.EntersJunction(mine) && index.SameJunctionSuccessors(mine).Count > 0;
                bool entry = index.EntersJunction(other) && index.SameJunctionSuccessors(other).Count > 0;
                if (!continuation || !entry || !ringMergeSeen.Add(denied.ZoneId + "|" + denied.TrafficId)) return;
                RingMergeFindings.Add("lot " + snapshot.SourceFrame + " : fusion " + denied.ZoneId + ", entree " + other + " de "
                    + denied.CauseActorId + " servie avant la continuation " + mine + " de " + denied.TrafficId + " (" + denied.Reason + ")");
            }

            private void Violation(string text)
            {
                if (Violations.Count < 50) Violations.Add("pas hote " + spawner.V2Runner.FrameId.ToString(CultureInfo.InvariantCulture) + " : " + text);
            }

            public void Observe()
            {
                Steps++;
                var runner = spawner.V2Runner;
                if (lastFrameId != 0UL && runner.FrameId != lastFrameId + 1UL)
                    Violation("FrameId " + runner.FrameId + " apres " + lastFrameId + " : pas hote manquant ou double");
                if (runner.FramesBuilt - lastFramesBuilt > 1) Violation("plusieurs frames construites en un pas");
                lastFrameId = runner.FrameId;
                lastFramesBuilt = runner.FramesBuilt;
                var live = spawner.LiveV2Vehicles;
                MaxLive = Math.Max(MaxLive, live.Count);
                if (live.Count > maxPopulation) Violation("population " + live.Count + " au-dela du maximum " + maxPopulation);
                foreach (var networkObject in live)
                {
                    var driver = networkObject.GetComponent<TrafficV2VehicleDriver>();
                    if (driver == null) continue;
                    int previous;
                    bool known = intents.TryGetValue(driver.TrafficId, out previous);
                    if (known && driver.IntentsApplied != previous + 1)
                        Violation(driver.TrafficId + " : " + (driver.IntentsApplied - previous) + " intent(s) en un pas");
                    if (!known && driver.IntentsApplied > 1) Violation(driver.TrafficId + " : intents avant le premier pas observe");
                    intents[driver.TrafficId] = driver.IntentsApplied;
                    if (driver.IntentsApplied > 0 && known && driver.LastFrameId != runner.FrameId)
                        Violation(driver.TrafficId + " decide sur la frame " + driver.LastFrameId + " au lieu de " + runner.FrameId);
                    var drive = driver.LastProjection != null ? driver.LastProjection.Drive : null;
                    if (driver.IntentsApplied > 0 && known && drive != null && drive.DecisionEpoch != runner.FrameId)
                        Violation(driver.TrafficId + " : epoque de decision " + drive.DecisionEpoch + " au lieu de la frame " + runner.FrameId);
                    var position = driver.transform.position;
                    if (float.IsNaN(position.sqrMagnitude) || float.IsInfinity(position.sqrMagnitude)) Violation(driver.TrafficId + " : pose non finie");
                    if (driver.LastLongitudinal != null)
                    {
                        float a = driver.LastLongitudinal.AppliedAccelerationMetersPerSecondSquared;
                        if (float.IsNaN(a) || float.IsInfinity(a)) Violation(driver.TrafficId + " : acceleration non finie");
                        if (driver.LastLongitudinal.PerceptionReason != PerceptionUnavailableReason.None)
                        {
                            int count;
                            Unavailable.TryGetValue(driver.LastLongitudinal.PerceptionReason, out count);
                            Unavailable[driver.LastLongitudinal.PerceptionReason] = count + 1;
                        }
                    }
                    var observation = driver.LastObservation;
                    if (observation.Perceived && observation.Obstacles.Items.Any(o => o.Kind == PerceivedObstacleKind.WalkingPlayer))
                        PlayerFacts++;
                    if (driver.LastHazardQuery.Saturated) CollectorSaturatedSteps++;
                    MaxCollectorHits = Math.Max(MaxCollectorHits, driver.LastHazardQuery.Hits);
                    if (ToleranceLatch == null && driver.ToleranceResponse.Latched)
                    {
                        ContaminationFrame = runner.FrameId;
                        ToleranceLatch = "pas hote " + runner.FrameId.ToString(CultureInfo.InvariantCulture) + " : " + driver.TrafficId
                            + " TrackingToleranceExceeded, d max " + driver.MaxStepDisplacementMeters.ToString("0.####", CultureInfo.InvariantCulture)
                            + " m (epsilon_t " + TrafficV2Settings.DeclaredTrackingTolerance.Meters.ToString("0.##", CultureInfo.InvariantCulture) + " m)";
                    }
                }
                if (spawner.V2Removals != spawner.RetiredV2Runs.Count) Violation("retrait hors du chemin de retrait");
                if (spawner.RetiredV2Runs.Any(r => !r.HasReachedExitPortal)) Violation("retrait hors portail de sortie");
                if (spawner.V2Insertions != live.Count + spawner.V2Removals) Violation("vehicule disparu hors portail");
                var cost = runner.LastCost;
                var work = Features.Vehicles.Traffic.Diagnostics.TrafficV2WorkCounters.Work;
                long priorityWork = work.JunctionPrecedenceChecks + work.JunctionGapEvaluations;
                if (cost.Vehicles > 0)
                {
                    HostStepMilliseconds.Add(cost.TotalMilliseconds);
                    CoordinatorMilliseconds.Add(cost.CoordinatorMilliseconds);
                    FrameBuildMilliseconds.Add(cost.FrameBuildMilliseconds);
                    DriveMilliseconds.Add(cost.DriveMilliseconds);
                    if (lastPriorityWork >= 0L) PriorityWork.Add(priorityWork - lastPriorityWork);
                    if (cost.Vehicles == maxPopulation) FullPopulationHostStepMilliseconds.Add(cost.TotalMilliseconds);
                }
                lastPriorityWork = priorityWork;
                ObserveJunctions(runner);
            }
        }

        // ------------------------------------------------------------------ publication

        public sealed class VehicleRun
        {
            public RoadId TrafficId;
            public V2DriveRecord Record;
            public bool Retired;
            public int Index;
            public ulong InsertedAtFrame;
        }

        /// <summary>Releves de tous les vehicules du scenario : retires (enregistrement conserve) et encore presents.</summary>
        public static List<VehicleRun> Runs(PortalTrafficSpawner spawner)
        {
            var runs = new List<VehicleRun>();
            foreach (var record in spawner.RetiredV2Runs)
                runs.Add(new VehicleRun { TrafficId = record.TrafficId, Record = record, Retired = true });
            foreach (var networkObject in spawner.LiveV2Vehicles)
            {
                var driver = networkObject.GetComponent<TrafficV2VehicleDriver>();
                if (driver != null) runs.Add(new VehicleRun { TrafficId = driver.TrafficId, Record = driver.CaptureRecord(), Retired = false });
            }
            foreach (var run in runs)
                foreach (var insertion in spawner.ScenarioInsertions)
                    if (insertion.TrafficId == run.TrafficId) { run.Index = insertion.Index; run.InsertedAtFrame = insertion.FrameId; }
            return runs.OrderBy(r => r.Index).ToList();
        }

        public static string Stamp()
        {
            return DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        }

        private static string F(float value)
        {
            return float.IsNaN(value) ? "nan" : float.IsInfinity(value) ? "inf" : value.ToString("0.#####", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Trace brute par pas et par vehicule, triee par (FrameId, rang d'insertion). Les colonnes junction_* (Story 5.34) portent
        /// la demande de la frame et la decision du coordinateur lue dans l'instantane.
        /// </summary>
        /// <param name="folder">Dossier de publication ; celui de la 5.33 par defaut.</param>
        public static string WriteTrace(string label, string stamp, List<VehicleRun> runs, string folder = Folder)
        {
            Directory.CreateDirectory(folder);
            string path = folder + "/" + label + "-" + stamp + "-steps.tsv";
            var text = new StringBuilder();
            text.Append("frame\tinsertion\ttraffic_id\tstep\telement\ts_route_m\tv\tvstar\td_step_m\tbinding_kind\tbinding\tplan_binding\t"
                + "limiting\ttarget_a\tapplied_a\tsmoothed\tperception\tleader\tleader_gap_m\tleader_v\tobstacle\tobstacle_kind\t"
                + "obstacle_near_m\tobstacle_facts\twalking_player_facts\tblockers\tdominant\thazard_hits\thazard_saturated\tfallback\t"
                + "reason\tthrottle\tsteer\tbrake_reverse\thandbrake\tx\ty\tz\thold\thold_cause\thold_source\thold_gap_m\thold_release\t"
                + "junction_traversal\tjunction_movements\tjunction_d_m\tjunction_d_stop\tjunction_d_engage\tjunction_d_request\tjunction_head\t"
                + "junction_engaged\tjunction_grant\tjunction_request_valid\tjunction_rejection\tjunction_entry_active\tjunction_stale\t"
                + "junction_occupied\tjunction_status\tjunction_reason\tjunction_cause\tjunction_zone\tjunction_since\tjunction_exit_free_m\t"
                + "junction_exit_required_m\tjunction_exit_bound\tjunction_kind\tjunction_b_m\tjunction_t_gap_s\tjunction_eta_s\t"
                + "junction_stop_marked\n");
            var rows = new List<KeyValuePair<ulong, string>>();
            foreach (var run in runs)
                foreach (var r in run.Record.Trace)
                {
                    var i = r.Interaction;
                    var line = new StringBuilder();
                    line.Append(i.FrameId).Append('\t').Append(run.Index).Append('\t').Append(run.TrafficId).Append('\t').Append(r.Step)
                        .Append('\t').Append(r.ElementId).Append('\t').Append(F(r.RouteDistanceMeters)).Append('\t').Append(F(r.LongitudinalSpeed))
                        .Append('\t').Append(r.CeilingUnbounded ? "inf" : F(r.CeilingMetersPerSecond)).Append('\t').Append(F(r.StepDisplacementMeters))
                        .Append('\t').Append(i.Arbitrated ? i.BindingKind.ToString() : "-").Append('\t').Append(i.BindingConstraint)
                        .Append('\t').Append(r.Binding).Append('\t').Append(r.Limiting).Append('\t').Append(F(i.TargetAccelerationMetersPerSecondSquared))
                        .Append('\t').Append(F(i.AppliedAccelerationMetersPerSecondSquared)).Append('\t').Append(i.Smoothed ? 1 : 0)
                        .Append('\t').Append(i.PerceptionReason).Append('\t').Append(i.LeaderId).Append('\t').Append(F(i.LeaderGapMeters))
                        .Append('\t').Append(F(i.LeaderSpeedMetersPerSecond)).Append('\t').Append(i.ObstacleId).Append('\t').Append(i.ObstacleId == RoadId.None ? "-" : i.ObstacleKind.ToString())
                        .Append('\t').Append(F(i.ObstacleNearMeters)).Append('\t').Append(i.ObstacleFacts).Append('\t').Append(i.WalkingPlayerFacts)
                        .Append('\t').Append(i.BlockerCount).Append('\t').Append(i.DominantBlocker ?? "-").Append('\t').Append(i.HazardQueryHits)
                        .Append('\t').Append(i.HazardQuerySaturated ? 1 : 0).Append('\t').Append(r.Fallback ? 1 : 0).Append('\t').Append(r.Reason)
                        .Append('\t').Append(F(r.Intent.Throttle)).Append('\t').Append(F(r.Intent.Steer)).Append('\t').Append(F(r.Intent.BrakeReverse))
                        .Append('\t').Append(F(r.Intent.Handbrake)).Append('\t').Append(F(r.State.Position.x)).Append('\t').Append(F(r.State.Position.y))
                        .Append('\t').Append(F(r.State.Position.z)).Append('\t').Append(i.Hold.Phase)
                        .Append('\t').Append(i.Hold.Phase == StopHoldPhase.None ? "-" : i.Hold.Cause.ToString())
                        .Append('\t').Append(i.Hold.Phase == StopHoldPhase.None ? "-" : i.Hold.SourceId.ToString())
                        .Append('\t').Append(F(i.Hold.Phase == StopHoldPhase.None ? float.NaN : i.Hold.GapMeters)).Append('\t').Append(i.Hold.Release);
                    var j = i.Junction;
                    line.Append('\t').Append(j.HasRequest ? j.TraversalId.ToString() : "-").Append('\t').Append(j.MovementCount)
                        .Append('\t').Append(F(j.DistanceMeters)).Append('\t').Append(F(j.StopMeters)).Append('\t').Append(F(j.EngageMeters))
                        .Append('\t').Append(F(j.RequestMeters)).Append('\t').Append(j.HeadOfQueue ? 1 : 0).Append('\t').Append(j.Engaged ? 1 : 0)
                        .Append('\t').Append(j.GrantEffective ? 1 : 0).Append('\t').Append(j.RequestValid ? 1 : 0).Append('\t').Append(j.Rejection)
                        .Append('\t').Append(j.EntryActive ? 1 : 0).Append('\t').Append(j.SnapshotStale ? 1 : 0).Append('\t').Append(j.OccupiedMovements)
                        .Append('\t').Append(j.HasDecision ? j.Status.ToString() : "-").Append('\t').Append(j.HasDecision ? j.Reason.ToString() : "-")
                        .Append('\t').Append(j.CauseActorId.IsEmpty ? "-" : j.CauseActorId.ToString())
                        .Append('\t').Append(j.ZoneId.IsEmpty ? "-" : j.ZoneId.ToString()).Append('\t').Append(j.HasDecision ? j.RequestSinceFrame.ToString(CultureInfo.InvariantCulture) : "-")
                        .Append('\t').Append(F(j.ExitFreeMeters)).Append('\t').Append(F(j.ExitRequiredMeters)).Append('\t').Append(j.ExitBound)
                        .Append('\t').Append(j.HasDecision ? j.ControlKind.ToString() : "-").Append('\t').Append(F(j.BoundaryMeters))
                        .Append('\t').Append(F(j.GapSeconds)).Append('\t').Append(F(j.EtaSeconds)).Append('\t').Append(j.StopMarked ? 1 : 0).Append('\n');
                    rows.Add(new KeyValuePair<ulong, string>(i.FrameId * 16UL + (ulong)Math.Max(0, run.Index), line.ToString()));
                }
            foreach (var row in rows.OrderBy(r => r.Key)) text.Append(row.Value);
            File.WriteAllText(path, text.ToString());
            return path;
        }

        /// <summary>Cout moyen par vehicule et par pas, par etape (ms) : frame (collecteur compris), perception, spine, plan+arbitrage, composition.</summary>
        public static double[] CostPerVehicleStep(List<VehicleRun> runs)
        {
            double frame = 0, perception = 0, spine = 0, plan = 0, compose = 0;
            int steps = 0;
            foreach (var run in runs)
            {
                var t = run.Record.Timings;
                frame += t.FrameMilliseconds; perception += t.PerceptionMilliseconds; spine += t.SpineMilliseconds;
                plan += t.SpeedPlanMilliseconds; compose += t.ComposeMilliseconds; steps += t.Steps;
            }
            int n = Math.Max(1, steps);
            return new[] { frame / n, perception / n, spine / n, plan / n, compose / n, (frame + perception + spine + plan + compose) / n };
        }

        public static string CostText(double[] cost)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "frame {0:0.###} ms / perception {1:0.###} ms / spine {2:0.###} ms / plan+arbitrage {3:0.###} ms / composition {4:0.###} ms / total {5:0.###} ms",
                cost[0], cost[1], cost[2], cost[3], cost[4], cost[5]);
        }

        /// <summary>
        /// Rapport du maintien a l'arret D11 par vehicule : entree (frame, cause, jeu), vitesse minimale pendant le maintien,
        /// depart de la cause (retrait de l'obstacle, ou liberation / vitesse de depart du leader), liberation (frame, raison,
        /// jeu), duree, reprise (premiere vitesse &gt;= vitesse d'entree), rampement, fin d'approche, d max et repli 2a.
        /// Mouvement lent pousse : acceleration appliquee &gt; 0 entre 0,02 m/s et la vitesse d'entree, liante d'interaction,
        /// hors maintien, derriere une source qui ne repart pas. Rampement (motif du defaut du scenario A, decision du
        /// 2026-10-02) : ce mouvement APRES un arret (v &lt; 0,05 m/s) derriere une cause d'interaction, tant qu'aucune
        /// liberation explicite du maintien n'est intervenue. Fin d'approche : ce mouvement sans arret prealable, publie a part.
        /// </summary>
        public static List<string> StopHoldReport(List<VehicleRun> runs, ulong obstacleRemovedAt, out float worstCrawlMeters)
        {
            var lines = new List<string>();
            worstCrawlMeters = 0f;
            var stopHold = TrafficV2Settings.StopHold;
            float band = stopHold.EntrySpeedMetersPerSecond;
            float dt = Time.fixedDeltaTime;
            foreach (var run in runs)
            {
                var trace = run.Record.Trace;
                int entry = -1, approachSteps = 0;
                float crawl = 0f, approach = 0f, approachD = 0f, dMax = 0f;
                bool latched = false, stoppedBehindCause = false;
                for (int k = 0; k < trace.Count; k++)
                {
                    var r = trace[k];
                    var i = r.Interaction;
                    if (entry < 0 && i.Hold.Phase == StopHoldPhase.Entered) entry = k;
                    dMax = Math.Max(dMax, r.StepDisplacementMeters);
                    if (r.Reason == V2FallbackReason.TrackingToleranceExceeded) latched = true;
                    bool interaction = i.Arbitrated && LongitudinalArbitration.IsInteraction(i.BindingConstraint);
                    if (i.Hold.Phase == StopHoldPhase.Released || !interaction) stoppedBehindCause = false;
                    else if (Math.Abs(r.LongitudinalSpeed) < 0.05f) stoppedBehindCause = true;
                    float sourceSpeed = i.BindingConstraint == SpeedConstraint.LeaderFollowing ? i.LeaderSpeedMetersPerSecond : 0f;
                    if (interaction && !i.Hold.Active && i.AppliedAccelerationMetersPerSecondSquared > 0f && r.LongitudinalSpeed > 0.02f
                        && r.LongitudinalSpeed < band && !(sourceSpeed >= stopHold.SourceDepartureSpeedMetersPerSecond))
                    {
                        if (stoppedBehindCause) crawl += r.LongitudinalSpeed * dt;
                        else { approach += r.LongitudinalSpeed * dt; approachSteps++; approachD = Math.Max(approachD, r.StepDisplacementMeters); }
                    }
                }
                worstCrawlMeters = Math.Max(worstCrawlMeters, crawl);
                var text = new StringBuilder();
                text.Append('#').Append(run.Index).Append(' ').Append(run.TrafficId).Append(" : ");
                if (entry < 0) text.Append("aucun maintien");
                else
                {
                    var e = trace[entry];
                    int release = -1;
                    for (int k = entry + 1; k < trace.Count && release < 0; k++)
                        if (trace[k].Interaction.Hold.Phase == StopHoldPhase.Released) release = k;
                    int end = release < 0 ? trace.Count - 1 : release;
                    float vMin = float.PositiveInfinity;
                    for (int k = entry; k <= end; k++) vMin = Math.Min(vMin, Math.Abs(trace[k].LongitudinalSpeed));
                    text.Append("entree StopHold fr ").Append(e.Interaction.FrameId).Append(" (").Append(e.Interaction.Hold.Cause).Append(" @")
                        .Append(e.Interaction.Hold.SourceId).Append(", jeu ").Append(F(e.Interaction.Hold.GapMeters)).Append(" m, v ")
                        .Append(F(e.LongitudinalSpeed)).Append(" m/s), v min pendant le maintien ").Append(F(vMin)).Append(" m/s");
                    ulong departure = 0UL;
                    if (e.Interaction.Hold.Cause == LongitudinalCandidateKind.Obstacle) departure = obstacleRemovedAt;
                    else
                    {
                        var leader = runs.FirstOrDefault(o => o.TrafficId == e.Interaction.Hold.SourceId);
                        if (leader != null)
                            foreach (var l in leader.Record.Trace)
                                if (l.Interaction.FrameId > e.Interaction.FrameId && (l.Interaction.Hold.Phase == StopHoldPhase.Released
                                    || l.LongitudinalSpeed >= stopHold.SourceDepartureSpeedMetersPerSecond))
                                { departure = l.Interaction.FrameId; break; }
                    }
                    text.Append(", depart de la cause ").Append(departure == 0UL ? "-" : "fr " + departure);
                    if (release < 0) text.Append(", jamais libere (fin de trace fr ").Append(trace[trace.Count - 1].Interaction.FrameId).Append(')');
                    else
                    {
                        var rr = trace[release];
                        long held = (long)rr.Interaction.FrameId - (long)e.Interaction.FrameId;
                        text.Append(", liberation fr ").Append(rr.Interaction.FrameId).Append(" (").Append(rr.Interaction.Hold.Release).Append(", jeu ")
                            .Append(F(rr.Interaction.Hold.GapMeters)).Append(" m), duree du maintien ").Append(held).Append(" pas (")
                            .Append(F(held * dt)).Append(" s)");
                        if (departure > 0UL)
                            text.Append(", delai depart -> liberation ").Append(F(((long)rr.Interaction.FrameId - (long)departure) * dt)).Append(" s");
                        int resume = -1;
                        for (int k = release; k < trace.Count && resume < 0; k++) if (trace[k].LongitudinalSpeed >= band) resume = k;
                        text.Append(", reprise v >= ").Append(F(band)).Append(" m/s ").Append(resume < 0 ? "jamais"
                            : "fr " + trace[resume].Interaction.FrameId + " (+" + F(((long)trace[resume].Interaction.FrameId - (long)rr.Interaction.FrameId) * dt) + " s)");
                    }
                    int entries = 0;
                    foreach (var r in trace) if (r.Interaction.Hold.Phase == StopHoldPhase.Entered) entries++;
                    text.Append(", entrees au total ").Append(entries);
                }
                text.Append(" ; rampement apres arret ").Append(F(crawl)).Append(" m ; fin d'approche lente poussee ").Append(F(approach))
                    .Append(" m en ").Append(approachSteps).Append(" pas (d max ").Append(F(approachD)).Append(" m) ; d max ").Append(F(dMax))
                    .Append(" m ; TrackingToleranceExceeded ").Append(latched ? "OUI" : "non");
                lines.Add(text.ToString());
            }
            return lines;
        }

        /// <summary>Journal des changements de decision du coordinateur, un record par ligne.</summary>
        public static string WriteJunctionLog(string label, string stamp, Observer observer, string folder = Folder)
        {
            Directory.CreateDirectory(folder);
            string path = folder + "/" + label + "-" + stamp + "-junction.txt";
            File.WriteAllText(path, string.Join("\n", observer.JunctionLog) + "\n");
            return path;
        }

        public static double Percentile(List<double> values, double p)
        {
            if (values == null || values.Count == 0) return double.NaN;
            var sorted = values.OrderBy(v => v).ToList();
            return sorted[Math.Min(sorted.Count - 1, (int)Math.Floor(p * (sorted.Count - 1) + 0.5))];
        }

        /// <summary>« moyenne / mediane / p95 / max ms (n pas) », invariant de culture.</summary>
        public static string PercentileText(List<double> values)
        {
            if (values == null || values.Count == 0) return "aucun pas";
            return string.Format(CultureInfo.InvariantCulture, "moyenne {0:0.###} / mediane {1:0.###} / p95 {2:0.###} / max {3:0.###} ms ({4} pas)",
                values.Average(), Percentile(values, 0.5), Percentile(values, 0.95), values.Max(), values.Count);
        }

        private const string V2VehicleName = "AI_VehicleV2_Portal_";

        /// <summary>
        /// Verdicts O6 des contacts entre vehicules V2 (Story 5.34), classes a leur premier pas depuis les elements des deux
        /// vehicules : deux mouvements distincts d'une meme zone de conflit -> echec 5.34 (giratoires compris) ; tout autre
        /// contact V2-V2 -> echec « autre ». Un contact vu par les deux vehicules n'est compte qu'une fois.
        /// </summary>
        public static void ClassifyVehicleContacts(List<VehicleRun> runs, CompiledRoadModel model, out List<string> junction534,
            out List<string> other)
        {
            junction534 = new List<string>();
            other = new List<string>();
            var zones = new Dictionary<string, RoadId>();
            foreach (var zone in model.ConflictZones)
                foreach (var a in zone.MemberMovementIds)
                    foreach (var b in zone.MemberMovementIds)
                        if (a != b && !zones.ContainsKey(a + "|" + b)) zones.Add(a + "|" + b, zone.Id);
            var byCounter = new Dictionary<ulong, VehicleRun>();
            foreach (var run in runs) byCounter[run.TrafficId.Low] = run;
            var seen = new HashSet<string>();
            foreach (var run in runs)
                foreach (var contact in run.Record.ContactEpisodes)
                {
                    int at = contact.ColliderPath.LastIndexOf(V2VehicleName, StringComparison.Ordinal);
                    if (at < 0) continue;
                    var digits = new string(contact.ColliderPath.Substring(at + V2VehicleName.Length).TakeWhile(char.IsDigit).ToArray());
                    ulong counter;
                    VehicleRun hit;
                    if (!ulong.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out counter)) continue;
                    var mine = run.Record.Trace.LastOrDefault(t => t.Step == contact.FirstStep);
                    ulong frame = mine.Interaction.FrameId;
                    var theirs = byCounter.TryGetValue(counter, out hit) ? hit.Record.Trace.LastOrDefault(t => t.Interaction.FrameId == frame)
                        : default(V2StepRecord);
                    RoadId self = run.TrafficId, peer = hit != null ? hit.TrafficId : RoadId.None;
                    string pair = (self.CompareTo(peer) < 0 ? self + "|" + peer : peer + "|" + self) + "|" + frame;
                    if (!seen.Add(pair)) continue;
                    RoadId zoneId = RoadId.None;
                    bool inZone = mine.ElementId != theirs.ElementId && zones.TryGetValue(mine.ElementId + "|" + theirs.ElementId, out zoneId);
                    string line = "pas hote " + frame + " : " + self + " sur " + mine.ElementId + " / " + peer + " sur " + theirs.ElementId
                        + (inZone ? " (zone " + zoneId + ")" : "") + ", impulsion " + F(contact.MaxImpulseNewtonSeconds) + " N.s";
                    (inZone ? junction534 : other).Add(line);
                }
        }

        /// <summary>Episodes de contact de caisse classes : entre vehicules V2, avec un obstacle de test, autres.</summary>
        public static void Contacts(List<VehicleRun> runs, out List<string> vehicle, out List<string> obstacle, out List<string> other)
        {
            vehicle = new List<string>(); obstacle = new List<string>(); other = new List<string>();
            foreach (var run in runs)
                foreach (var c in run.Record.ContactEpisodes)
                {
                    string line = run.TrafficId + " " + c.ColliderPath + " pas " + c.FirstStep + "-" + c.LastStep + " impulsion "
                        + F(c.MaxImpulseNewtonSeconds) + " v " + F(c.SpeedAtFirstContact) + "->" + F(c.MinimumSpeedDuringContact);
                    if (c.ColliderPath.Contains("AI_VehicleV2_Portal_")) vehicle.Add(line);
                    else if (c.ColliderPath.Contains("Story533_Obstacle_")) obstacle.Add(line);
                    else other.Add(line);
                }
        }
    }
}
