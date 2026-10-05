using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.33 : campagne exploratoire multi-vehicules V2 dans MVP_Run (2, 4 et 8 vehicules, plus un rejeu a 2 pour la
    /// reproductibilite physique mesuree). Elle publie des constats : suivi, files et resorption, vehicule arrete devant,
    /// sortie occupee, carrefour en croix, giratoires, stabilite de la frame, saturations, cout par etape selon N. Les
    /// campagnes 2/4/8 sont nominales (politique pre-5.39) : aucune poussee, et un TrackingToleranceExceeded y est un echec
    /// immediat, comme un invariant viole (NaN, intent manquant ou double, frame multiple par pas, retrait hors portail,
    /// population au-dela du maximum, joueur hote dans un fait V2) ou une anomalie de cout D7.
    /// Story 5.34 : la coordination de carrefour est branchee et l'exemption D12 est retiree, sans successeur. Verdicts O6 des
    /// campagnes nominales : chaque contact V2-V2 est classe a son premier pas ; deux mouvements distincts d'une meme zone de
    /// conflit, deux grants incompatibles effectifs ou EnteredWithoutGrant &gt; 0 sont un echec 5.34 ; tout autre contact V2-V2
    /// est un echec. L'ordre de service a une fusion d'anneau qui differe de « l'anneau d'abord » est un constat 5.35 publie,
    /// sans effet sur le verdict. Le pas hote et le coordinateur sont publies par N (moyenne, mediane, p95, max) ; a N = 8 le
    /// p95 du pas hote reste sous 10 ms (D13). La poussee vit dans un scenario exploratoire separe : son premier verrou marque
    /// le point de contamination, clot la fenetre d'observation fonctionnelle et arrete le scenario.
    /// </summary>
    [Explicit]
    [Category("Story533Exploration")]
    public sealed class Story533ExplorationPlayModeTests
    {
        /// <summary>D7 : pire moyenne 5.31 par vehicule et par pas (route+horizon 2,283 ms + plan de vitesse 2,66 ms), declaree avant mesure.</summary>
        private const double WorstStory531MeanMilliseconds = 2.283 + 2.66;

        private static readonly Dictionary<string, double[]> Costs = new Dictionary<string, double[]>();
        private static readonly Dictionary<string, Dictionary<string, Vector3[]>> Trajectories = new Dictionary<string, Dictionary<string, Vector3[]>>();

        private Story533Harness harness;

        [SetUp]
        public void SetUp()
        {
            TrafficV2Session.Reset();
            harness = new Story533Harness();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return harness.Cleanup();
        }

        [UnityTest, Order(1), Timeout(1800000)]
        public IEnumerator ExploreTwoVehiclesFollowing()
        {
            yield return Explore("explore-2", "explore-2");
            var cost = Costs["explore-2"];
            Assert.That(cost[5], Is.LessThanOrEqualTo(2.0 * WorstStory531MeanMilliseconds),
                "anomalie de cout D7 : cout total par vehicule a N = 2 au-dela de 2 x la pire moyenne 5.31");
        }

        [UnityTest, Order(2), Timeout(1800000)]
        public IEnumerator ExploreTwoVehiclesAgainToMeasureThePhysicalReproducibility()
        {
            yield return Explore("explore-2", "explore-2-rejeu");
        }

        [UnityTest, Order(3), Timeout(1800000)]
        public IEnumerator ExploreFourVehiclesQueueFormationAndResorption()
        {
            yield return Explore("explore-4", "explore-4");
        }

        [UnityTest, Order(4), Timeout(1800000)]
        public IEnumerator ExploreEightVehiclesFromTheFourEntries()
        {
            yield return Explore("explore-8", "explore-8");
            double[] two;
            if (Costs.TryGetValue("explore-2", out two))
                Assert.That(Costs["explore-8"][5], Is.LessThanOrEqualTo(2.0 * two[5]),
                    "anomalie de cout D7 : cout par vehicule a N = 8 au-dela de 2 x celui a N = 2 (croissance non lineaire)");
        }

        /// <summary>Exploratoire perturbe, hors jugement de 5.33 : leader pousse lateralement jusqu'a sa sortie de couverture.</summary>
        [UnityTest, Order(5), Timeout(1800000)]
        public IEnumerator ExploreAPushedLeaderUntilItsCoverageIsLost()
        {
            yield return Explore("explore-poussee", "explore-poussee", true);
        }

        /// <summary>Pas observes apres le point de contamination d'un scenario perturbe : etat du vehicule verrouille, puis arret.</summary>
        private const int StepsAfterContamination = 150;

        private IEnumerator Explore(string scenarioLabel, string runLabel, bool perturbed = false)
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile file;
            var record = Story533Harness.Load(admission, scenarioLabel, out file);
            Assert.That(perturbed || record.Pushes.Length == 0, Is.True, "campagne nominale : aucune poussee volontaire");
            TrafficV2Session.Request(TrafficComposition.V2Slice, null, Story533Harness.ToScenario(record));
            yield return harness.EnterMvpRun();
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            int parked = 0;
            yield return harness.ParkHostPlayer(file.Parking.Value, n => parked = n);
            float playerDistance = Story533Harness.PlayerDistanceToRoutes(admission.Model, record);
            Assert.That(parked == 0 || playerDistance > TrafficV2Settings.HazardQueryRadiusMeters + 5f, Is.True,
                "joueur hote stationne hors des routes : " + playerDistance + " m");

            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation, admission.Model);
            var obstacles = new GameObject[record.Obstacles.Length];
            var removedObstacle = new bool[record.Obstacles.Length];
            var pushed = new bool[record.Pushes.Length];
            var findings = new List<string>();
            int maxQueued = 0, maxStopped = 0;
            float minimumGap = float.PositiveInfinity;
            int closeAt = -1;
            for (int step = 0; step < record.MaxSteps; step++)
            {
                ulong frame = spawner.V2Runner.FrameId;
                for (int o = 0; o < obstacles.Length; o++)
                {
                    var obstacle = record.Obstacles[o];
                    if (obstacles[o] == null && !removedObstacle[o] && frame >= (ulong)obstacle.FromStep) obstacles[o] = harness.CreateObstacle(obstacle);
                    if (obstacles[o] != null && obstacle.UntilStep >= 0 && frame >= (ulong)obstacle.UntilStep)
                    {
                        harness.Destroy(obstacles[o]);
                        obstacles[o] = null;
                        removedObstacle[o] = true;
                        findings.Add("obstacle " + obstacle.Label + " retire au pas hote " + frame);
                    }
                }
                for (int p = 0; p < pushed.Length; p++)
                {
                    var push = record.Pushes[p];
                    if (pushed[p] || frame < (ulong)push.AtStep || push.Insertion >= spawner.ScenarioInsertions.Count) continue;
                    var driver = Story533Harness.DriverOf(spawner, spawner.ScenarioInsertions[push.Insertion].TrafficId);
                    if (driver == null) continue;
                    var body = driver.GetComponent<Rigidbody>();
                    body.AddForce(driver.transform.right * body.mass * push.LateralImpulsePerKilogram, ForceMode.Impulse);
                    pushed[p] = true;
                    findings.Add("vehicule #" + push.Insertion + " pousse au pas hote " + frame + " (" + push.LateralImpulsePerKilogram + " m/s lateral)");
                }
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0) break;
                if (observer.ToleranceLatch != null)
                {
                    // Nominal : echec immediat, sans exemption (D12 retiree par la 5.34). Perturbe : point de contamination.
                    if (!perturbed) break;
                    if (closeAt < 0)
                    {
                        closeAt = step + StepsAfterContamination;
                        findings.Add("point de contamination : " + observer.ToleranceLatch + " ; fenetre d'observation fonctionnelle close, "
                            + "le comportement des vehicules bloques par le vehicule terminal n'est plus une preuve du trafic nominal");
                    }
                    if (step >= closeAt)
                    {
                        findings.Add("scenario arrete au pas hote " + spawner.V2Runner.FrameId + " : objectif diagnostique atteint");
                        break;
                    }
                }
                int queued = 0, stopped = 0;
                foreach (var networkObject in spawner.LiveV2Vehicles)
                {
                    var driver = networkObject.GetComponent<TrafficV2VehicleDriver>();
                    if (driver == null || driver.Trace.Count == 0) continue;
                    var last = driver.Trace[driver.Trace.Count - 1];
                    if (Mathf.Abs(last.LongitudinalSpeed) < 0.1f) stopped++;
                    if (driver.Blockers.Count > 0) queued++;
                    if (!float.IsNaN(last.Interaction.LeaderGapMeters)) minimumGap = Math.Min(minimumGap, last.Interaction.LeaderGapMeters);
                }
                maxQueued = Math.Max(maxQueued, queued);
                maxStopped = Math.Max(maxStopped, stopped);
                if (spawner.ScenarioInsertions.Count == record.Insertions.Length && spawner.LiveV2Population == 0) break;
            }

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace(runLabel, stamp, runs);
            string journal = Story533Harness.WriteJunctionLog(runLabel, stamp, observer);
            var cost = Story533Harness.CostPerVehicleStep(runs);
            Costs[runLabel] = cost;
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            List<string> junctionFailures, otherVehicleContacts;
            Story533Harness.ClassifyVehicleContacts(runs, admission.Model, out junctionFailures, out otherVehicleContacts);
            double hostP95 = Story533Harness.Percentile(observer.FullPopulationHostStepMilliseconds, 0.95);

            // Reproductibilite : ecart maximal de position, meme vehicule et meme pas propre, entre deux executions.
            var trajectory = runs.ToDictionary(r => r.TrafficId.ToString(), r => r.Record.Trace.Select(t => t.State.Position).ToArray());
            Dictionary<string, Vector3[]> previous;
            string replay = "non mesure (premiere execution du scenario dans cette campagne)";
            if (Trajectories.TryGetValue(scenarioLabel, out previous))
            {
                float worst = 0f;
                int compared = 0;
                foreach (var pair in trajectory)
                {
                    Vector3[] other;
                    if (!previous.TryGetValue(pair.Key, out other)) continue;
                    for (int i = 0; i < Math.Min(other.Length, pair.Value.Length); i++)
                    { worst = Math.Max(worst, Vector3.Distance(other[i], pair.Value[i])); compared++; }
                }
                replay = "ecart maximal " + worst.ToString("0.####", CultureInfo.InvariantCulture) + " m sur " + compared + " pas-vehicule compares";
            }
            else Trajectories[scenarioLabel] = trajectory;

            var alive = runs.Where(r => !r.Retired).ToList();
            foreach (var run in alive)
            {
                var last = run.Record.Trace.Count > 0 ? run.Record.Trace[run.Record.Trace.Count - 1] : default(V2StepRecord);
                findings.Add("encore present en fin de campagne : #" + run.Index + " " + run.TrafficId + " sur " + last.ElementId + " a "
                    + last.LongitudinalSpeed.ToString("0.##", CultureInfo.InvariantCulture) + " m/s, dominant "
                    + (last.Interaction.DominantBlocker ?? "-") + ", liante " + last.Interaction.BindingConstraint
                    + (last.Fallback ? ", repli " + last.Reason : ""));
            }
            int latched = runs.Count(r => r.Record.Trace.Any(t => t.Reason == RoadRage.Features.Vehicles.Traffic.Intent.V2FallbackReason.TrackingToleranceExceeded));

            var text = new StringBuilder();
            text.Append("# ").Append(runLabel).Append(" ").Append(stamp).Append(" (exploratoire : constats publies, jamais juges)\n\n");
            text.Append("- Scenario ").Append(record.Label).Append(" : ").Append(record.Insertions.Length).Append(" insertions, population maximale ")
                .Append(record.MaxPopulation).Append(", pas hote ").Append(spawner.V2Runner.FrameId).Append(" / ").Append(record.MaxSteps)
                .Append(", fixedDeltaTime ").Append(Time.fixedDeltaTime.ToString("0.####", CultureInfo.InvariantCulture)).Append(" s\n");
            text.Append("- Insertions reelles : ").Append(string.Join(" ; ", spawner.ScenarioInsertions.Select(i => "#" + i.Index + " au plus tot "
                + record.Insertions[i.Index].EarliestStep + ", reelle " + i.FrameId).ToArray())).Append('\n');
            text.Append("- Sorties atteintes : ").Append(spawner.V2Removals).Append('/').Append(spawner.ScenarioInsertions.Count)
                .Append(", vehicules encore presents : ").Append(alive.Count).Append('\n');
            text.Append("- Frame : ").Append(spawner.V2Runner.FramesBuilt).Append(" construites, ").Append(spawner.V2Runner.FrameFailures)
                .Append(" refusees, population maximale observee ").Append(observer.MaxLive).Append('\n');
            text.Append("- Files : jusqu'a ").Append(maxQueued).Append(" vehicules avec blocker, ").Append(maxStopped)
                .Append(" arretes simultanement ; jeu percu minimal ").Append(minimumGap.ToString("0.###", CultureInfo.InvariantCulture)).Append(" m\n");
            text.Append("- Collecteur : ").Append(observer.MaxCollectorHits).Append(" colliders au plus par requete (capacite ")
                .Append(TrafficV2Settings.HazardQueryCapacity).Append("), ").Append(observer.CollectorSaturatedSteps).Append(" pas-vehicule satures\n");
            text.Append("- PerceptionUnavailable : ").Append(observer.Unavailable.Count == 0 ? "aucun"
                : string.Join(", ", observer.Unavailable.Select(p => p.Key + " " + p.Value).ToArray())).Append('\n');
            text.Append("- Repli 2a (TrackingToleranceExceeded) : ").Append(latched).Append(" vehicule(s)")
                .Append(observer.ToleranceLatch == null ? "" : perturbed ? ", constat attendu pre-5.39 (" + observer.ToleranceLatch + ")"
                    : ", ECHEC nominal (" + observer.ToleranceLatch + ")").Append('\n');
            text.Append("- Coordination (verdicts O6) : ").Append(observer.Batches).Append(" lots, ").Append(observer.RefusedBatches)
                .Append(" sur frame refusee ; grants incompatibles simultanes ").Append(observer.IncompatibleGrantSteps)
                .Append(observer.FirstIncompatibleGrants == null ? "" : " (" + observer.FirstIncompatibleGrants + ")")
                .Append(" ; EnteredWithoutGrant ").Append(observer.EnteredWithoutGrant).Append(" ; IncompatibleOccupancy ")
                .Append(observer.IncompatibleOccupancy).Append('\n');
            text.Append("- Contacts V2-V2 classes : echec 5.34 ").Append(junctionFailures.Count).Append(", autres ").Append(otherVehicleContacts.Count).Append('\n');
            foreach (var line in junctionFailures) text.Append("  - echec 5.34 : ").Append(line).Append('\n');
            foreach (var line in otherVehicleContacts) text.Append("  - autre : ").Append(line).Append('\n');
            text.Append("- Constats 5.35 (fusion d'anneau, non bloquants) : ").Append(observer.RingMergeFindings.Count == 0 ? "aucun" : "").Append('\n');
            foreach (var line in observer.RingMergeFindings) text.Append("  - ").Append(line).Append('\n');
            text.Append("- Pas hote Traffic V2 (N = ").Append(record.MaxPopulation).Append(") : ").Append(Story533Harness.PercentileText(observer.HostStepMilliseconds))
                .Append(" ; en population pleine : ").Append(Story533Harness.PercentileText(observer.FullPopulationHostStepMilliseconds))
                .Append(" ; coordinateur : ").Append(Story533Harness.PercentileText(observer.CoordinatorMilliseconds)).Append('\n');
            float crawl;
            text.Append("- Maintien a l'arret (D11) :\n");
            foreach (var line in Story533Harness.StopHoldReport(runs, 0UL, out crawl)) text.Append("  - ").Append(line).Append('\n');
            text.Append("- Contacts entre vehicules : ").Append(vehicleContacts.Count).Append(", avec un obstacle : ").Append(obstacleContacts.Count)
                .Append(", autres : ").Append(otherContacts.Count).Append('\n');
            foreach (var line in vehicleContacts.Concat(obstacleContacts).Concat(otherContacts)) text.Append("  - ").Append(line).Append('\n');
            text.Append("- Reproductibilite physique : ").Append(replay).Append('\n');
            text.Append("- Cout par vehicule et par pas (N = ").Append(record.MaxPopulation).Append(") : ").Append(Story533Harness.CostText(cost)).Append('\n');
            text.Append("- Constats :\n");
            foreach (var finding in findings) text.Append("  - ").Append(finding).Append('\n');
            text.Append("- Invariants : ").Append(observer.Violations.Count == 0 && observer.PlayerFacts == 0 ? "verts"
                : string.Join(" | ", observer.Violations) + (observer.PlayerFacts > 0 ? " | joueur hote dans un fait V2" : "")).Append('\n');
            text.Append("- Trace brute : `").Append(Path.GetFileName(trace)).Append("` ; journal de coordination : `")
                .Append(Path.GetFileName(journal)).Append("`\n");
            File.WriteAllText(Story533Harness.Folder + "/" + runLabel + "-" + stamp + "-summary.md", text.ToString());
            Debug.Log("[Story533] " + runLabel + " : " + text.ToString().Replace("\n", " "));

            Assert.That(observer.PlayerFacts, Is.Zero, "invariant : joueur hote dans un fait leader ou obstacle V2");
            Assert.That(observer.Violations, Is.Empty, string.Join("\n", observer.Violations));
            if (perturbed) yield break;
            // Verdicts O6 (Story 5.34) : aucune exemption, D12 n'a pas de successeur.
            Assert.That(observer.ToleranceLatch, Is.Null, "verrou 2a nominal : " + observer.ToleranceLatch);
            Assert.That(observer.IncompatibleGrantSteps, Is.Zero, "echec 5.34 : " + observer.FirstIncompatibleGrants);
            Assert.That(observer.EnteredWithoutGrant, Is.Zero, "echec 5.34 : EnteredWithoutGrant");
            Assert.That(junctionFailures, Is.Empty, "echec 5.34 : " + string.Join(" | ", junctionFailures));
            Assert.That(otherVehicleContacts, Is.Empty, "contact V2-V2 autre : " + string.Join(" | ", otherVehicleContacts));
            if (record.MaxPopulation == 8)
                Assert.That(hostP95, Is.LessThan(10.0), "D13 : p95 du pas hote Traffic V2 en population pleine a N = 8 sous 10 ms ("
                    + Story533Harness.PercentileText(observer.FullPopulationHostStepMilliseconds) + ")");
        }
    }
}
