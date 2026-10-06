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
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.35, Gate C : campagnes nominales a N = 2, 4 et 8 dans MVP_Run (scenarios gatec-* de Story535ScenarioBuilderTests,
    /// chacune par la croix, un T et un giratoire). Verdicts : tous les vehicules atteignent un portail de sortie, sans
    /// interblocage durable ni retrait, teleportation ou reinsertion ; aucune entree sans place en sortie (aucun vehicule
    /// immobilise dans un mouvement hors attente a sa frontiere) ; EnteredWithoutGrant = 0, aucun grant incompatible hors
    /// creneau de fusion, aucun contact V2-V2, aucun TrackingToleranceExceeded ; p95 du pas hote en population pleine &lt; 10 ms a
    /// N = 8. Briseurs d'interblocage et cout par frontiere (frame, coordinateur, priorite, pas vehicule) publies, observationnels.
    /// </summary>
    [Explicit]
    [Category("Story535GateC")]
    public sealed class Story535GateCPlayModeTests
    {
        private const string Folder = "_bmad-output/implementation-artifacts/traffic-v2-5-35-explorations";
        private const string ScenarioPath = Folder + "/scenarios-5-35.json";
        /// <summary>Pas consecutifs a l'arret dans un mouvement, hors attente a sa frontiere, qui signalent une entree sans place (0,5 s).</summary>
        private const int StuckInsideSteps = 25;

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
        public IEnumerator GateCTwoVehiclesThroughTheCrossroads()
        {
            yield return Campaign("gatec-2");
        }

        [UnityTest, Order(2), Timeout(1800000)]
        public IEnumerator GateCFourVehiclesMeetingAtTheCrossroads()
        {
            yield return Campaign("gatec-4");
        }

        [UnityTest, Order(3), Timeout(1800000)]
        public IEnumerator GateCEightVehiclesFromTheFourEntries()
        {
            yield return Campaign("gatec-8");
        }

        /// <summary>
        /// Immobilisations dans un mouvement de carrefour (v &lt; 0,05 m/s pendant StuckInsideSteps pas), sauf a l'attente sans grant
        /// devant la frontiere de ce mouvement (ligne d'arret dans le mouvement).
        /// </summary>
        private static List<string> StuckInsideMovements(List<Story533Harness.VehicleRun> runs, CompiledRoadModel model)
        {
            var lines = new List<string>();
            foreach (var run in runs)
            {
                int still = 0;
                foreach (var r in run.Record.Trace)
                {
                    CompiledJunctionMovement movement;
                    var j = r.Interaction.Junction;
                    bool waitingAtBoundary = j.HasRequest && !j.GrantEffective && j.TraversalId == r.ElementId && j.DistanceMeters > 0f;
                    bool inside = model.TryGetMovement(r.ElementId, out movement) && !waitingAtBoundary;
                    still = inside && Mathf.Abs(r.LongitudinalSpeed) < 0.05f ? still + 1 : 0;
                    if (still == StuckInsideSteps)
                        lines.Add(run.TrafficId + " immobilise dans " + r.ElementId + " au pas hote " + r.Interaction.FrameId + ", dominant "
                            + (r.Interaction.DominantBlocker ?? "-"));
                }
            }
            return lines;
        }

        private IEnumerator Campaign(string label)
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile file;
            var record = Story533Harness.Load(admission, label, out file, ScenarioPath);
            Assert.That(record.Obstacles.Length == 0 && record.Pushes.Length == 0, Is.True, "campagne nominale");
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
            for (int step = 0; step < record.MaxSteps; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0 || observer.ToleranceLatch != null) break;
                if (spawner.ScenarioInsertions.Count == record.Insertions.Length && spawner.LiveV2Population == 0) break;
            }

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace(label, stamp, runs, Folder);
            string journal = Story533Harness.WriteJunctionLog(label, stamp, observer, Folder);
            var cost = Story533Harness.CostPerVehicleStep(runs);
            List<string> vehicleContacts, obstacleContacts, otherContacts, junctionFailures, otherVehicleContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            Story533Harness.ClassifyVehicleContacts(runs, admission.Model, out junctionFailures, out otherVehicleContacts);
            var stuck = StuckInsideMovements(runs, admission.Model);
            double hostP95 = Story533Harness.Percentile(observer.FullPopulationHostStepMilliseconds, 0.95);
            var alive = runs.Where(r => !r.Retired).ToList();

            var text = new StringBuilder();
            text.Append("# ").Append(label).Append(' ').Append(stamp).Append(" (Gate C 5.35)\n\n");
            text.Append("- Scenario ").Append(record.Label).Append(" : ").Append(record.Insertions.Length).Append(" insertions, population maximale ")
                .Append(record.MaxPopulation).Append(", pas hote ").Append(spawner.V2Runner.FrameId).Append(" / ").Append(record.MaxSteps)
                .Append(", fixedDeltaTime ").Append(Time.fixedDeltaTime.ToString("0.####", CultureInfo.InvariantCulture)).Append(" s\n");
            text.Append("- Sorties atteintes : ").Append(spawner.V2Removals).Append('/').Append(record.Insertions.Length)
                .Append(", encore presents : ").Append(alive.Count).Append('\n');
            foreach (var run in alive)
            {
                var last = run.Record.Trace.Count > 0 ? run.Record.Trace[run.Record.Trace.Count - 1] : default(V2StepRecord);
                text.Append("  - #").Append(run.Index).Append(' ').Append(run.TrafficId).Append(" sur ").Append(last.ElementId).Append(", dominant ")
                    .Append(last.Interaction.DominantBlocker ?? "-").Append(", decision ").Append(last.Interaction.Junction.HasDecision
                        ? last.Interaction.Junction.Status + "(" + last.Interaction.Junction.Reason + ")" : "-").Append('\n');
            }
            text.Append("- Coordination : ").Append(observer.Batches).Append(" lots, ").Append(observer.RefusedBatches).Append(" sur frame refusee ; ")
                .Append("grants incompatibles hors creneau ").Append(observer.IncompatibleGrantSteps)
                .Append(observer.FirstIncompatibleGrants == null ? "" : " (" + observer.FirstIncompatibleGrants + ")")
                .Append(" ; EnteredWithoutGrant ").Append(observer.EnteredWithoutGrant).Append(" ; IncompatibleOccupancy ").Append(observer.IncompatibleOccupancy).Append('\n');
            text.Append("- Regles 5.35 : StopRequired ").Append(observer.StopRequired).Append(", YieldToPriority ").Append(observer.YieldToPriority)
                .Append(", GrantedMergeGap ").Append(observer.MergeGapGrants).Append(", refus de creneau ").Append(observer.MergeGapRefusals)
                .Append(", refus Crossing ").Append(observer.CrossingRefusals).Append(", briseurs d'interblocage ").Append(observer.DeadlockBreaks).Append('\n');
            text.Append("- Immobilisations dans un mouvement (entree sans place) : ").Append(stuck.Count).Append('\n');
            foreach (var line in stuck) text.Append("  - ").Append(line).Append('\n');
            text.Append("- Contacts V2-V2 : en zone de conflit ").Append(junctionFailures.Count).Append(", autres ").Append(otherVehicleContacts.Count)
                .Append(" ; obstacles ").Append(obstacleContacts.Count).Append(", autres colliders ").Append(otherContacts.Count).Append('\n');
            foreach (var line in junctionFailures.Concat(otherVehicleContacts)) text.Append("  - ").Append(line).Append('\n');
            text.Append("- Cout par frontiere (pas hote avec vehicule) : frame ").Append(Story533Harness.PercentileText(observer.FrameBuildMilliseconds))
                .Append(" ; coordinateur ").Append(Story533Harness.PercentileText(observer.CoordinatorMilliseconds))
                .Append(" ; priorite (preseances + creneaux par lot, temps compris dans le coordinateur) ")
                .Append(observer.PriorityWork.Count == 0 ? "aucun lot" : string.Format(CultureInfo.InvariantCulture, "moyenne {0:0.##} / max {1:0}",
                    observer.PriorityWork.Average(), observer.PriorityWork.Max()))
                .Append(" ; pas vehicules ").Append(Story533Harness.PercentileText(observer.DriveMilliseconds)).Append('\n');
            text.Append("- Pas hote : ").Append(Story533Harness.PercentileText(observer.HostStepMilliseconds)).Append(" ; en population pleine : ")
                .Append(Story533Harness.PercentileText(observer.FullPopulationHostStepMilliseconds)).Append('\n');
            text.Append("- Cout par vehicule et par pas : ").Append(Story533Harness.CostText(cost)).Append('\n');
            text.Append("- Couverture : ").Append(observer.ToleranceLatch == null ? "aucun TrackingToleranceExceeded" : "ECHEC " + observer.ToleranceLatch).Append('\n');
            text.Append("- Invariants : ").Append(observer.Violations.Count == 0 && observer.PlayerFacts == 0 ? "verts"
                : string.Join(" | ", observer.Violations) + (observer.PlayerFacts > 0 ? " | joueur hote dans un fait V2" : "")).Append('\n');
            text.Append("- Trace brute : `").Append(Path.GetFileName(trace)).Append("` ; journal de coordination : `").Append(Path.GetFileName(journal)).Append("`\n");
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Folder + "/" + label + "-" + stamp + "-summary.md", text.ToString());
            Debug.Log("[Story535] " + label + " : " + text.ToString().Replace("\n", " "));

            Assert.That(observer.PlayerFacts, Is.Zero, "invariant : joueur hote dans un fait leader ou obstacle V2");
            Assert.That(observer.Violations, Is.Empty, string.Join("\n", observer.Violations));
            Assert.That(observer.ToleranceLatch, Is.Null, "verrou 2a nominal : " + observer.ToleranceLatch);
            Assert.That(spawner.ScenarioInsertions.Count, Is.EqualTo(record.Insertions.Length), "toutes les insertions servies");
            Assert.That(alive, Is.Empty, "interblocage durable ou vehicule bloque : tous doivent atteindre un portail de sortie");
            Assert.That(runs.All(r => r.Record.HasReachedExitPortal), Is.True, "retrait hors portail");
            Assert.That(observer.IncompatibleGrantSteps, Is.Zero, "grants incompatibles hors creneau : " + observer.FirstIncompatibleGrants);
            Assert.That(observer.EnteredWithoutGrant, Is.Zero, "EnteredWithoutGrant");
            Assert.That(stuck, Is.Empty, "entree sans place en sortie : " + string.Join(" | ", stuck));
            Assert.That(junctionFailures, Is.Empty, "contact en zone de conflit : " + string.Join(" | ", junctionFailures));
            Assert.That(otherVehicleContacts, Is.Empty, "contact V2-V2 : " + string.Join(" | ", otherVehicleContacts));
            if (record.MaxPopulation == 8)
                Assert.That(hostP95, Is.LessThan(10.0), "D13 : p95 du pas hote en population pleine a N = 8 sous 10 ms ("
                    + Story533Harness.PercentileText(observer.FullPopulationHostStepMilliseconds) + ")");
        }
    }
}
