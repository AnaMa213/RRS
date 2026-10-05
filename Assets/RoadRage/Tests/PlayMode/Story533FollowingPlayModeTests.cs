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
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.33 : scenarios d'acceptation PlayMode dans MVP_Run, hors run de mesure (couverture Covered, repli 2a actif).
    /// A : trois vehicules, meme route, file derriere un obstacle cinematique place par le test, puis resorption. B : deux
    /// vehicules, meme route, insertion decalee. Les traces brutes et le resume sont publies dans
    /// traffic-v2-5-33-explorations/. Ni la coordination de carrefour ni la Gate C ne sont revendiquees.
    /// </summary>
    [Category("Story533")]
    public sealed class Story533FollowingPlayModeTests
    {
        /// <summary>Intervalle de jeu a l'arret declare avant mesure : [s0 - 0,10 ; s0 + 0,50] m.</summary>
        private const float BelowMinimumGap = 0.10f;
        private const float AboveMinimumGap = 0.50f;

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

        private static float Speed(TrafficV2VehicleDriver driver)
        {
            return driver.Trace.Count == 0 ? 0f : driver.Trace[driver.Trace.Count - 1].LongitudinalSpeed;
        }

        private static V2InteractionRecord Last(TrafficV2VehicleDriver driver)
        {
            return driver.Trace.Count == 0 ? default(V2InteractionRecord) : driver.Trace[driver.Trace.Count - 1].Interaction;
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator ScenarioAThreeVehiclesQueueBehindAKinematicObstacleAndTheQueueDissolves()
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile file;
            var record = Story533Harness.Load(admission, "A", out file);
            float s0 = file.MinimumGapMeters;
            TrafficV2Session.Request(TrafficComposition.V2Slice, null, Story533Harness.ToScenario(record));
            yield return harness.EnterMvpRun();
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            int parked = 0;
            yield return harness.ParkHostPlayer(file.Parking.Value, n => parked = n);
            float playerDistance = Story533Harness.PlayerDistanceToRoutes(admission.Model, record);
            Assert.That(parked == 0 || playerDistance > TrafficV2Settings.HazardQueryRadiusMeters + 5f, Is.True,
                "joueur hote stationne hors des routes : " + playerDistance + " m");
            var obstacle = harness.CreateObstacle(record.Obstacles[0]);
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation);

            int stoppedSteps = 0;
            ulong removedAt = 0UL;
            string queue = null;
            var blockerSteps = new Dictionary<RoadId, int>();
            var illegitimate = new List<string>();
            for (int step = 0; step < record.MaxSteps && spawner.V2Removals < record.Insertions.Length; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0) Assert.Inconclusive("run invalide : joueur hote dans un fait leader ou obstacle V2");
                // Scenario nominal : un verrou TrackingToleranceExceeded est un echec immediat (politique pre-5.39).
                if (observer.ToleranceLatch != null) break;
                if (queue != null) continue;
                var drivers = spawner.ScenarioInsertions.Select(i => Story533Harness.DriverOf(spawner, i.TrafficId)).ToList();
                bool all = drivers.Count == record.Insertions.Length && drivers.All(d => d != null);
                bool stopped = all && drivers.All(d => Mathf.Abs(Speed(d)) < 0.1f);
                stoppedSteps = stopped ? stoppedSteps + 1 : 0;
                if (stopped)
                    foreach (var driver in drivers)
                    {
                        int count;
                        blockerSteps.TryGetValue(driver.TrafficId, out count);
                        blockerSteps[driver.TrafficId] = count + (driver.Blockers.Count > 0 ? 1 : 0);
                        foreach (var blocker in driver.Blockers.Where(b => !b.Legitimate)) illegitimate.Add(driver.TrafficId + " " + blocker.ToText());
                    }
                if (stoppedSteps < 50) continue;
                // File stable : jeu percu de chacun dans l'intervalle declare, puis retrait de l'obstacle.
                queue = QueueReport(drivers, s0);
                harness.Destroy(obstacle);
                removedAt = spawner.V2Runner.FrameId;
            }

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace("acceptance-A", stamp, runs);
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            float crawl;
            var holds = Story533Harness.StopHoldReport(runs, removedAt, out crawl);
            WriteSummary("acceptance-A", stamp, record, spawner, observer, runs, queue, removedAt, vehicleContacts, obstacleContacts,
                otherContacts, trace, holds);

            Assert.That(observer.ToleranceLatch, Is.Null, "scenario nominal sorti de sa couverture : " + observer.ToleranceLatch);
            Assert.That(observer.Violations, Is.Empty, string.Join("\n", observer.Violations));
            Assert.That(crawl, Is.LessThanOrEqualTo(0.05f), "rampement apres arret vers s0 : " + string.Join(" | ", holds));
            Assert.That(queue, Is.Not.Null, "la file ne s'est pas stabilisee derriere l'obstacle");
            Assert.That(queue, Does.Not.Contain("HORS"), queue);
            Assert.That(blockerSteps.Count, Is.EqualTo(3));
            Assert.That(blockerSteps.Values.All(n => n > 0), Is.True, "chaque vehicule arrete porte un blocker : " + queue);
            Assert.That(illegitimate, Is.Empty, "blockers legitimes : " + string.Join("; ", illegitimate));
            Assert.That(spawner.V2Removals, Is.EqualTo(3), "la file se resorbe et chacun atteint sa sortie");
            AssertCleanRuns(runs, vehicleContacts, obstacleContacts);
            foreach (var insertion in spawner.ScenarioInsertions)
                Assert.That(insertion.FrameId, Is.GreaterThanOrEqualTo(record.Insertions[insertion.Index].EarliestStep));
        }

        /// <summary>
        /// Jeu percu a l'arret : premier vehicule face a l'obstacle, suiveurs face a leur leader. Amendement 5.34 (O11) : un
        /// suiveur tenu avant l'entree d'un carrefour par un blocker de coordination legitime (JunctionGrant ou BlockedExit),
        /// a 0 &lt; d &lt;= fenetre de maintien de cette entree (D_stop a la vitesse d'entree du maintien, O14), occupe une position de
        /// file valide.
        /// </summary>
        private static string QueueReport(List<TrafficV2VehicleDriver> drivers, float s0)
        {
            var text = new StringBuilder();
            for (int i = 0; i < drivers.Count; i++)
            {
                var interaction = Last(drivers[i]);
                var junction = interaction.Junction;
                bool junctionHeld = i > 0 && junction.HasRequest && junction.DistanceMeters > 0f
                    && junction.DistanceMeters <= JunctionDistances.For(drivers[i].DriverProfileDefinition.Profile, 0f, Time.fixedDeltaTime,
                        TrafficV2Settings.JunctionStopControlMarginMeters, TrafficV2Settings.StopHold.EntrySpeedMetersPerSecond).HoldWindowMeters
                    && drivers[i].Blockers.Any(b => b.Legitimate && b.Source == BlockerSource.JunctionCoordination
                        && (b.Kind == BlockerKind.JunctionGrant || b.Kind == BlockerKind.BlockedExit));
                if (junctionHeld)
                {
                    text.Append("entree ").Append(drivers[i].TrafficId).Append(" distance ")
                        .Append(junction.DistanceMeters.ToString("0.###", CultureInfo.InvariantCulture)).Append(" ok (O11)")
                        .Append(" dominant ").Append(interaction.DominantBlocker ?? "-").Append(" ; ");
                    continue;
                }
                float gap = i == 0 ? interaction.ObstacleNearMeters : interaction.LeaderGapMeters;
                bool inside = !float.IsNaN(gap) && gap >= s0 - BelowMinimumGap && gap <= s0 + AboveMinimumGap;
                bool expected = i == 0 ? interaction.ObstacleId != RoadId.None : interaction.LeaderId == drivers[i - 1].TrafficId;
                text.Append(i == 0 ? "obstacle " : "leader ").Append(drivers[i].TrafficId).Append(" jeu ")
                    .Append(gap.ToString("0.###", CultureInfo.InvariantCulture)).Append(inside && expected ? " ok" : " HORS")
                    .Append(" dominant ").Append(interaction.DominantBlocker ?? "-").Append(" ; ");
            }
            return text.ToString();
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator ScenarioBAStaggeredFollowerNeverGoesBelowTheMinimumGapAndBothReachTheirExit()
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile file;
            var record = Story533Harness.Load(admission, "B", out file);
            float s0 = file.MinimumGapMeters;
            TrafficV2Session.Request(TrafficComposition.V2Slice, null, Story533Harness.ToScenario(record));
            yield return harness.EnterMvpRun();
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            int parked = 0;
            yield return harness.ParkHostPlayer(file.Parking.Value, n => parked = n);
            float playerDistance = Story533Harness.PlayerDistanceToRoutes(admission.Model, record);
            Assert.That(parked == 0 || playerDistance > TrafficV2Settings.HazardQueryRadiusMeters + 5f, Is.True,
                "joueur hote stationne hors des routes : " + playerDistance + " m");
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation);
            for (int step = 0; step < record.MaxSteps && spawner.V2Removals < record.Insertions.Length; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0) Assert.Inconclusive("run invalide : joueur hote dans un fait leader ou obstacle V2");
                if (observer.ToleranceLatch != null) break;
            }

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace("acceptance-B", stamp, runs);
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            var follower = runs.FirstOrDefault(r => r.Index == 1);
            float minimumGap = follower == null ? float.NaN : follower.Record.Trace.Where(r => !float.IsNaN(r.Interaction.LeaderGapMeters))
                .Select(r => r.Interaction.LeaderGapMeters).DefaultIfEmpty(float.NaN).Min();
            int followed = follower == null ? 0 : follower.Record.Trace.Count(r => r.Interaction.BindingConstraint
                == RoadRage.Features.Vehicles.Traffic.Planning.SpeedConstraint.LeaderFollowing);
            float crawl;
            var holds = Story533Harness.StopHoldReport(runs, 0UL, out crawl);
            WriteSummary("acceptance-B", stamp, record, spawner, observer, runs, "jeu percu minimal du suiveur "
                + minimumGap.ToString("0.###", CultureInfo.InvariantCulture) + " m, " + followed + " pas lies par LeaderFollowing",
                0UL, vehicleContacts, obstacleContacts, otherContacts, trace, holds);

            Assert.That(observer.ToleranceLatch, Is.Null, "scenario nominal sorti de sa couverture : " + observer.ToleranceLatch);
            Assert.That(observer.Violations, Is.Empty, string.Join("\n", observer.Violations));
            Assert.That(spawner.V2Removals, Is.EqualTo(2), "les deux atteignent leur sortie");
            Assert.That(follower, Is.Not.Null);
            Assert.That(spawner.ScenarioInsertions[1].FrameId, Is.GreaterThanOrEqualTo(record.Insertions[1].EarliestStep), "insertion decalee");
            foreach (var r in follower.Record.Trace)
                if (!float.IsNaN(r.Interaction.LeaderGapMeters))
                    Assert.That(r.Interaction.LeaderGapMeters, Is.GreaterThanOrEqualTo(s0 - BelowMinimumGap),
                        "jeu percu du suiveur au pas " + r.Step);
            foreach (var run in runs)
                foreach (var r in run.Record.Trace)
                    if (!float.IsNaN(r.SpeedRatio))
                        Assert.That(r.SpeedRatio, Is.LessThanOrEqualTo(1f), "v > v* au pas " + r.Step + " de " + run.TrafficId);
            AssertCleanRuns(runs, vehicleContacts, obstacleContacts);
        }

        /// <summary>Aucun contact entre vehicules ni avec l'obstacle, aucun TrackingToleranceExceeded, repli seulement au portail.</summary>
        private static void AssertCleanRuns(List<Story533Harness.VehicleRun> runs, List<string> vehicleContacts, List<string> obstacleContacts)
        {
            Assert.That(vehicleContacts, Is.Empty, "contact de caisse entre vehicules : " + string.Join("; ", vehicleContacts));
            Assert.That(obstacleContacts, Is.Empty, "contact avec l'obstacle : " + string.Join("; ", obstacleContacts));
            foreach (var run in runs)
            {
                Assert.That(run.Retired, Is.True, run.TrafficId + " encore present : retrait au portail de sortie attendu");
                Assert.That(run.Record.HasReachedExitPortal, Is.True);
                Assert.That(run.Record.MaxStepDisplacementMeters, Is.LessThanOrEqualTo(TrafficV2Settings.DeclaredTrackingTolerance.Meters),
                    "TrackingToleranceExceeded : " + run.TrafficId);
                foreach (var r in run.Record.Trace)
                {
                    Assert.That(r.Reason, Is.Not.EqualTo(V2FallbackReason.TrackingToleranceExceeded), run.TrafficId + " pas " + r.Step);
                    if (r.Fallback)
                        Assert.That(r.Reason, Is.EqualTo(V2FallbackReason.ExitPortalReached), "repli hors portail : " + run.TrafficId + " pas " + r.Step);
                }
            }
        }

        private static void WriteSummary(string label, string stamp, Story533Harness.ScenarioRecord record, PortalTrafficSpawner spawner,
            Story533Harness.Observer observer, List<Story533Harness.VehicleRun> runs, string queue, ulong removedAt,
            List<string> vehicleContacts, List<string> obstacleContacts, List<string> otherContacts, string trace, List<string> holds)
        {
            var text = new StringBuilder();
            text.Append("# ").Append(label).Append(" ").Append(stamp).Append("\n\n");
            text.Append("- Scenario : ").Append(record.Label).Append(", population maximale ").Append(record.MaxPopulation)
                .Append(", fixedDeltaTime ").Append(Time.fixedDeltaTime.ToString("0.####", CultureInfo.InvariantCulture)).Append(" s\n");
            text.Append("- Pas hote : ").Append(spawner.V2Runner.FrameId).Append(", frames construites ").Append(spawner.V2Runner.FramesBuilt)
                .Append(", refus de frame ").Append(spawner.V2Runner.FrameFailures).Append(", population maximale observee ").Append(observer.MaxLive).Append('\n');
            text.Append("- Insertions : ").Append(string.Join(" ; ", spawner.ScenarioInsertions.Select(i => "#" + i.Index + " " + i.TrafficId
                + " au plus tot " + record.Insertions[i.Index].EarliestStep + ", reelle " + i.FrameId).ToArray())).Append('\n');
            text.Append("- Retraits au portail : ").Append(spawner.V2Removals).Append('/').Append(record.Insertions.Length).Append('\n');
            if (queue != null) text.Append("- Interaction : ").Append(queue).Append(removedAt > 0 ? " (obstacle retire au pas hote " + removedAt + ")" : "").Append('\n');
            text.Append("- Collecteur : ").Append(observer.MaxCollectorHits).Append(" colliders au plus par requete (capacite ")
                .Append(TrafficV2Settings.HazardQueryCapacity).Append("), ").Append(observer.CollectorSaturatedSteps).Append(" pas-vehicule satures\n");
            text.Append("- PerceptionUnavailable : ").Append(observer.Unavailable.Count == 0 ? "aucun"
                : string.Join(", ", observer.Unavailable.Select(p => p.Key + " " + p.Value).ToArray())).Append('\n');
            text.Append("- Contacts entre vehicules : ").Append(vehicleContacts.Count).Append(", avec l'obstacle : ").Append(obstacleContacts.Count)
                .Append(", autres : ").Append(otherContacts.Count).Append('\n');
            foreach (var line in vehicleContacts.Concat(obstacleContacts).Concat(otherContacts)) text.Append("  - ").Append(line).Append('\n');
            text.Append("- d max au pas : ").Append(string.Join(", ", runs.Select(r => r.TrafficId + " " + r.Record.MaxStepDisplacementMeters
                .ToString("0.####", CultureInfo.InvariantCulture)).ToArray())).Append('\n');
            text.Append("- Couverture : ").Append(observer.ToleranceLatch == null ? "aucun TrackingToleranceExceeded"
                : "ECHEC nominal, point de contamination " + observer.ToleranceLatch).Append('\n');
            text.Append("- Maintien a l'arret (D11) :\n");
            foreach (var line in holds) text.Append("  - ").Append(line).Append('\n');
            text.Append("- Cout par vehicule et par pas : ").Append(Story533Harness.CostText(Story533Harness.CostPerVehicleStep(runs))).Append('\n');
            text.Append("- Invariants : ").Append(observer.Violations.Count == 0 ? "verts" : string.Join(" | ", observer.Violations)).Append('\n');
            text.Append("- Trace brute : `").Append(Path.GetFileName(trace)).Append("`\n");
            File.WriteAllText(Story533Harness.Folder + "/" + label + "-" + stamp + "-summary.md", text.ToString());
            Debug.Log("[Story533] " + label + " : " + text.ToString().Replace("\n", " "));
        }
    }
}
