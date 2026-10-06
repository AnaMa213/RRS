using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Intent;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.34 : scenarios PlayMode de coordination dans MVP_Run, hors run de mesure (couverture Covered, repli 2a actif),
    /// avec le harnais 5.33 et les scenarios ecrits par Story534JunctionEntryTests. C : deux demandes incompatibles sur
    /// ConflictZones[23] servies l'une apres l'autre, axe Priority avant branche Yield depuis la 5.35, sequence publiee. D : sortie insuffisante refusee avant tout
    /// conflit, jamais franchie, puis servie une fois liberee. Saturation : la requete pleine du collecteur ne rend la
    /// perception indisponible que pour son vehicule. La Gate C n'est pas revendiquee.
    /// </summary>
    [Category("Story534")]
    [Category("Story535")]
    public sealed class Story534JunctionPlayModeTests
    {
        private const string Folder = "_bmad-output/implementation-artifacts/traffic-v2-5-34-explorations";
        private const string ScenarioPath = Folder + "/scenarios-5-34.json";
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private static readonly RoadId EastStraight = RoadId.Parse("40ca7f10a97f50a918e8c3a2a1e58493");
        private static readonly RoadId SouthLeft = RoadId.Parse("4e437f94525852d3a072a538537c2093");
        private static readonly RoadId Zone23 = RoadId.Parse("4258af5419bba1365a3f0ad6ed3d44aa");

        /// <summary>Pas d'arret stable exiges avant de retirer un obstacle d'attente (le temps que le maintien s'installe).</summary>
        private const int SettleSteps = 50;

        /// <summary>Pas pendant lesquels D observe le refus de sortie avant de retirer l'obstacle qui la tient (5 s).</summary>
        private const int ExitBlockedObservationSteps = 250;

        /// <summary>Tolerance d'integration sous m_ctrl de la bande d'arret declaree (decisions O13, O14), m.</summary>
        private const float StopBandIntegrationTolerance = 0.02f;

        /// <summary>Plafond declare entre le premier pas a grant effectif et l'entree dans le mouvement (decision O13), s.</summary>
        private const float ResumeCeilingSeconds = 3f;

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
            return driver == null || driver.Trace.Count == 0 ? 0f : driver.Trace[driver.Trace.Count - 1].LongitudinalSpeed;
        }

        private static V2InteractionRecord Last(TrafficV2VehicleDriver driver)
        {
            return driver.Trace.Count == 0 ? default(V2InteractionRecord) : driver.Trace[driver.Trace.Count - 1].Interaction;
        }

        /// <summary>Vehicule arrete et maintenu (StopHold) depuis le dernier pas.</summary>
        private static bool Held(TrafficV2VehicleDriver driver)
        {
            return driver != null && driver.Trace.Count > 0 && Mathf.Abs(Speed(driver)) < 0.05f && Last(driver).Hold.Active;
        }

        private static List<TrafficV2VehicleDriver> Drivers(PortalTrafficSpawner spawner, int count)
        {
            var drivers = spawner.ScenarioInsertions.Select(i => Story533Harness.DriverOf(spawner, i.TrafficId)).ToList();
            return drivers.Count == count && drivers.All(d => d != null) ? drivers : null;
        }

        /// <summary>Records d'un lot : l'instantane publie apres le pas hote N (source N, effectif N+1).</summary>
        private sealed class Batch
        {
            public ulong Frame;
            public JunctionRecord[] Records;
        }

        private static void Capture(PortalTrafficSpawner spawner, List<Batch> batches)
        {
            var snapshot = spawner.V2Runner.JunctionSnapshot;
            if (snapshot != null && snapshot.SourceFrame == spawner.V2Runner.FrameId)
                batches.Add(new Batch { Frame = snapshot.SourceFrame, Records = snapshot.Records.ToArray() });
        }

        private static string F(float value)
        {
            return float.IsNaN(value) ? "nan" : value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private IEnumerator Enter(string label, Action<Story533Harness.ScenarioRecord, Story533Harness.ScenarioFile, TrafficV2Admission> loaded)
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile file;
            var record = Story533Harness.Load(admission, label, out file, ScenarioPath);
            TrafficV2Session.Request(TrafficComposition.V2Slice, null, Story533Harness.ToScenario(record));
            yield return harness.EnterMvpRun();
            int parked = 0;
            yield return harness.ParkHostPlayer(file.Parking.Value, n => parked = n);
            float playerDistance = Story533Harness.PlayerDistanceToRoutes(admission.Model, record);
            Assert.That(parked == 0 || playerDistance > TrafficV2Settings.HazardQueryRadiusMeters + 5f, Is.True,
                "joueur hote stationne hors des routes : " + playerDistance + " m");
            loaded(record, file, admission);
        }

        /// <summary>Aucun contact, aucun TrackingToleranceExceeded, repli seulement au portail, retrait des deux au portail.</summary>
        private static void AssertCleanRuns(Story533Harness.Observer observer, List<Story533Harness.VehicleRun> runs,
            List<string> vehicleContacts, List<string> obstacleContacts)
        {
            Assert.That(observer.ToleranceLatch, Is.Null, "scenario nominal sorti de sa couverture : " + observer.ToleranceLatch);
            Assert.That(observer.Violations, Is.Empty, string.Join("\n", observer.Violations));
            Assert.That(observer.IncompatibleGrantSteps, Is.Zero, "deux grants incompatibles effectifs : " + observer.FirstIncompatibleGrants);
            Assert.That(observer.EnteredWithoutGrant, Is.Zero, "EnteredWithoutGrant");
            Assert.That(observer.IncompatibleOccupancy, Is.Zero, "IncompatibleOccupancy");
            Assert.That(vehicleContacts, Is.Empty, "contact entre vehicules : " + string.Join("; ", vehicleContacts));
            Assert.That(obstacleContacts, Is.Empty, "contact avec un obstacle : " + string.Join("; ", obstacleContacts));
            foreach (var run in runs)
            {
                Assert.That(run.Retired, Is.True, run.TrafficId + " encore present : retrait au portail de sortie attendu");
                Assert.That(run.Record.HasReachedExitPortal, Is.True);
                Assert.That(run.Record.MaxStepDisplacementMeters, Is.LessThanOrEqualTo(TrafficV2Settings.DeclaredTrackingTolerance.Meters),
                    "TrackingToleranceExceeded : " + run.TrafficId);
                foreach (var r in run.Record.Trace)
                    if (r.Fallback)
                        Assert.That(r.Reason, Is.EqualTo(V2FallbackReason.ExitPortalReached), "repli hors portail : " + run.TrafficId + " pas " + r.Step);
            }
        }

        /// <summary>Distance minimale du pare-chocs avant a la frontiere b tant que la traversee n'a pas de grant effectif.</summary>
        private static float MinimumDistanceWithoutGrant(Story533Harness.VehicleRun run, RoadId traversal, out ulong lastWaitingFrame)
        {
            float minimum = float.PositiveInfinity;
            lastWaitingFrame = 0UL;
            foreach (var r in run.Record.Trace)
            {
                var j = r.Interaction.Junction;
                if (!j.HasRequest || j.TraversalId != traversal || j.GrantEffective) continue;
                minimum = Math.Min(minimum, j.DistanceMeters);
                lastWaitingFrame = r.Interaction.FrameId;
            }
            return minimum;
        }

        /// <summary>
        /// Decisions O13 et O14 : arret sans grant dans la bande declaree [m_ctrl − tolerance ; fenetre de maintien] de l'entree
        /// (fenetre = D_stop a la vitesse d'entree du maintien, profil par defaut), puis, des le
        /// premier pas a grant effectif, ni maintien ni contrainte JunctionEntry jusqu'au franchissement de la frontiere b, atteint en au
        /// plus ResumeCeilingSeconds. Rend la ligne de sequence publiee.
        /// </summary>
        private static string AssertStopBandAndResume(Story533Harness.VehicleRun run, RoadId traversal, float stopDistance, float frontMeters)
        {
            float low = TrafficV2Settings.JunctionStopControlMarginMeters - StopBandIntegrationTolerance;
            var profile = UnityEditor.AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile;
            float high = JunctionDistances.For(profile, 0f, Time.fixedDeltaTime, TrafficV2Settings.JunctionStopControlMarginMeters,
                TrafficV2Settings.StopHold.EntrySpeedMetersPerSecond).HoldWindowMeters;
            Assert.That(stopDistance, Is.InRange(low, high), "O14 : arret sans grant hors de la bande declaree");
            var rows = run.Record.Trace.ToList();
            int granted = rows.FindIndex(t => t.Interaction.Junction.HasRequest && t.Interaction.Junction.TraversalId == traversal
                && t.Interaction.Junction.GrantEffective);
            Assert.That(granted, Is.GreaterThanOrEqualTo(0), "O13 : aucun pas a grant effectif");
            Assert.That(frontMeters, Is.GreaterThan(0f), "empreinte du vehicule mesuree au staging");
            float boundary = rows[granted].Interaction.Junction.BoundaryMeters;
            int entered = rows.FindIndex(granted, t =>
            {
                var piece = run.Record.Tracks[t.TrackIndex].Pieces.FirstOrDefault(p => p.Id == traversal);
                return piece != null && piece.StartDistanceMeters + boundary - piece.ElementStartSMeters
                    - t.RouteDistanceMeters - frontMeters <= 0f;
            });
            Assert.That(entered, Is.GreaterThan(granted), "O13 : frontiere b jamais franchie apres le grant");
            for (int k = granted; k < entered; k++)
            {
                Assert.That(rows[k].Interaction.Hold.Active, Is.False, "O13 : maintien apres le grant, pas hote " + rows[k].Interaction.FrameId);
                Assert.That(rows[k].Interaction.Junction.EntryActive, Is.False,
                    "O13 : contrainte JunctionEntry apres le grant, pas hote " + rows[k].Interaction.FrameId);
            }
            float seconds = (rows[entered].Interaction.FrameId - rows[granted].Interaction.FrameId) * Time.fixedDeltaTime;
            Assert.That(seconds, Is.LessThanOrEqualTo(ResumeCeilingSeconds), "O13 : reprise trop lente apres le grant");
            return "O14 : arret a d = " + F(stopDistance) + " m, bande [" + F(low) + " ; " + F(high) + "] m ; grant effectif au pas hote "
                + rows[granted].Interaction.FrameId + ", frontiere b = " + F(rows[granted].Interaction.Junction.BoundaryMeters)
                + " m franchie au pas hote " + rows[entered].Interaction.FrameId + " ("
                + seconds.ToString("0.##", CultureInfo.InvariantCulture) + " s, " + F(rows[entered].LongitudinalSpeed) + " m/s)";
        }

        private static string WriteSummary(string label, string stamp, Story533Harness.ScenarioRecord record, PortalTrafficSpawner spawner,
            Story533Harness.Observer observer, List<Story533Harness.VehicleRun> runs, IEnumerable<string> sequence,
            List<string> vehicleContacts, List<string> obstacleContacts, List<string> otherContacts, string trace, string journal)
        {
            var text = new StringBuilder();
            text.Append("# ").Append(label).Append(' ').Append(stamp).Append("\n\n");
            text.Append("- Scenario ").Append(record.Label).Append(", population ").Append(record.MaxPopulation).Append(", fixedDeltaTime ")
                .Append(Time.fixedDeltaTime.ToString("0.####", CultureInfo.InvariantCulture)).Append(" s, pas hote ").Append(spawner.V2Runner.FrameId)
                .Append(", lots ").Append(observer.Batches).Append(" dont refuses ").Append(observer.RefusedBatches).Append('\n');
            text.Append("- Insertions : ").Append(string.Join(" ; ", spawner.ScenarioInsertions.Select(i => "#" + i.Index + " " + i.TrafficId
                + " au pas " + i.FrameId).ToArray())).Append('\n');
            text.Append("- Retraits au portail : ").Append(spawner.V2Removals).Append('/').Append(record.Insertions.Length).Append('\n');
            text.Append("- Sequence :\n");
            foreach (var line in sequence) text.Append("  - ").Append(line).Append('\n');
            text.Append("- Coordination : grants incompatibles simultanes ").Append(observer.IncompatibleGrantSteps).Append(", EnteredWithoutGrant ")
                .Append(observer.EnteredWithoutGrant).Append(", IncompatibleOccupancy ").Append(observer.IncompatibleOccupancy).Append('\n');
            text.Append("- Cout du coordinateur : ").Append(Story533Harness.PercentileText(observer.CoordinatorMilliseconds))
                .Append(" ; pas hote ").Append(Story533Harness.PercentileText(observer.HostStepMilliseconds)).Append('\n');
            text.Append("- Contacts entre vehicules ").Append(vehicleContacts.Count).Append(", obstacles ").Append(obstacleContacts.Count)
                .Append(", autres ").Append(otherContacts.Count).Append('\n');
            foreach (var line in vehicleContacts.Concat(obstacleContacts).Concat(otherContacts)) text.Append("  - ").Append(line).Append('\n');
            text.Append("- d max au pas : ").Append(string.Join(", ", runs.Select(r => r.TrafficId + " "
                + r.Record.MaxStepDisplacementMeters.ToString("0.####", CultureInfo.InvariantCulture)).ToArray())).Append('\n');
            text.Append("- Couverture : ").Append(observer.ToleranceLatch == null ? "aucun TrackingToleranceExceeded" : "ECHEC " + observer.ToleranceLatch).Append('\n');
            text.Append("- Invariants : ").Append(observer.Violations.Count == 0 ? "verts" : string.Join(" | ", observer.Violations)).Append('\n');
            text.Append("- Trace brute : `").Append(Path.GetFileName(trace)).Append("` ; journal de coordination : `").Append(Path.GetFileName(journal)).Append("`\n");
            string path = Folder + "/" + label + "-" + stamp + "-summary.md";
            File.WriteAllText(path, text.ToString());
            Debug.Log("[Story534] " + label + " : " + text.ToString().Replace("\n", " "));
            return path;
        }

        // ================================================================== scenario C

        /// <summary>Conserve les distances d'attente de C a sa frontiere 5.35 sur les corridors droits du montage 5.34.</summary>
        private static void StageScenarioC(Story533Harness.ScenarioRecord record, TrafficV2Admission admission)
        {
            var index = JunctionConflictIndex.For(admission.Model);
            Assert.That(index.ControlKindOf(EastStraight), Is.EqualTo(JunctionControlKind.Priority));
            Assert.That(index.ControlKindOf(SouthLeft), Is.EqualTo(JunctionControlKind.Yield));
            Assert.That(index.HasPrecedence(EastStraight, SouthLeft), Is.True, "l'axe a preseance sur la branche");
            Assert.That(index.HasPrecedence(SouthLeft, EastStraight), Is.False);
            Assert.That(index.BoundaryOf(SouthLeft), Is.GreaterThan(0f), "C couvre la StopLine authoree de la branche");
            foreach (var movement in new[] { EastStraight, SouthLeft })
            {
                var corridorId = index.FromCorridorOf(movement);
                var obstacle = record.Obstacles.Single(o => RoadId.Parse(o.ElementId) == corridorId);
                EffectiveLaneCorridor corridor;
                Assert.That(admission.Model.TryGetCorridor(corridorId, out corridor), Is.True);
                Assert.That(corridor.Samples.All(s => Mathf.Abs(s.CurvaturePerMeter) < 1e-4f), Is.True, "obstacle sur approche droite");
                float b = index.BoundaryOf(movement);
                Assert.That(b + obstacle.Size.z, Is.LessThan(8f), "l'obstacle a 8 m de b reste entierement dans le corridor d'approche");
                var center = obstacle.Center.Value + obstacle.Forward.Value * b;
                obstacle.Center = new Story533Harness.VectorRecord { x = center.x, y = center.y, z = center.z };
                obstacle.RouteDistanceMeters += b;
            }
        }

        [UnityTest]
        [Timeout(900000)]
        public IEnumerator ScenarioCTwoIncompatibleRequestsOfOneBatchAreServedOneAfterTheOther()
        {
            Story533Harness.ScenarioRecord record = null;
            yield return Enter("C", (r, f, a) => { record = r; StageScenarioC(r, a); });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            var obstacles = record.Obstacles.Select(o => harness.CreateObstacle(o)).ToList();
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation);
            var batches = new List<Batch>();
            ulong removedAt = 0UL, southRemovedAt = 0UL;
            int settled = 0;
            float frontMeters = float.NaN;
            for (int step = 0; step < record.MaxSteps && spawner.V2Removals < record.Insertions.Length; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0) Assert.Inconclusive("run invalide : joueur hote dans un fait leader ou obstacle V2");
                if (observer.ToleranceLatch != null) break;
                Capture(spawner, batches);
                if (removedAt != 0UL) continue;
                var drivers = Drivers(spawner, 2);
                settled = drivers != null && drivers.All(Held) ? settled + 1 : 0;
                if (settled < SettleSteps) continue;
                frontMeters = TrafficV2VehicleDriver.FootprintOf(drivers[0].GetComponent<BoxCollider>()).FrontMeters;
                // Distances mesurees a b : retraits simultanes. Liberer l'est un pas avant le sud lui donnait un grant
                // au lot precedant leur premiere rencontre (trace 20261006-181212 : 1531 puis 1532), sans arbitrage frais.
                harness.Destroy(obstacles[0]);
                harness.Destroy(obstacles[1]);
                removedAt = spawner.V2Runner.FrameId;
                southRemovedAt = removedAt;
            }

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace("scenario-C", stamp, runs, Folder);
            string journal = Story533Harness.WriteJunctionLog("scenario-C", stamp, observer, Folder);
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            var sequence = new List<string> { "obstacle est retire au pas hote " + removedAt + ", obstacle sud au pas hote " + southRemovedAt };

            try
            {
                AssertCleanRuns(observer, runs, vehicleContacts, obstacleContacts);
                Assert.That(removedAt, Is.GreaterThan(0UL), "les deux vehicules ne se sont pas arretes derriere leur obstacle d'attente");
                Assert.That(southRemovedAt, Is.EqualTo(removedAt), "staging des obstacles C liberes au meme pas physique");
                var east = runs.Single(r => r.Index == 1);
                var south = runs.Single(r => r.Index == 0);
                var eastFrames = new HashSet<ulong>(east.Record.Trace.Where(t => t.Interaction.Junction.RequestValid
                    && t.Interaction.Junction.TraversalId == EastStraight).Select(t => t.Interaction.FrameId));
                var both = south.Record.Trace.Where(t => t.Interaction.Junction.RequestValid && t.Interaction.Junction.TraversalId == SouthLeft
                    && eastFrames.Contains(t.Interaction.FrameId)).Select(t => t.Interaction.FrameId).OrderBy(f => f).ToList();
                if (both.Count == 0)
                {
                    Assert.Fail("precondition absente : aucune paire de demandes actives au meme lot sur 40ca7f10 et 4e437f94 (run invalide, jamais reussi)");
                }
                ulong meet = both[0];
                sequence.Add("1. demandes actives au meme lot " + meet + " sur 40ca7f10 (" + east.TrafficId + ") et 4e437f94 (" + south.TrafficId + ")");

                Assert.That(batches.Where(b => b.Frame < meet).SelectMany(b => b.Records).Any(r => r.IsEffectiveGrant
                    && ((r.TrafficId == east.TrafficId && r.Contains(EastStraight))
                        || (r.TrafficId == south.TrafficId && r.Contains(SouthLeft)))), Is.False,
                    "precondition C invalide : grant anterieur au lot commun ; aucune arbitration fraiche demontree");

                var atMeet = batches.Single(b => b.Frame == meet).Records;
                bool eastHolds = atMeet.Any(r => r.TrafficId == east.TrafficId && r.IsEffectiveGrant && r.Contains(EastStraight));
                bool southHolds = atMeet.Any(r => r.TrafficId == south.TrafficId && r.IsEffectiveGrant && r.Contains(SouthLeft));
                Assert.That(eastHolds, Is.True, "l'axe Priority recoit le premier grant au lot commun");
                Assert.That(southHolds, Is.False, "la branche Yield cede a l'axe Priority");
                var holder = east;
                var waiter = south;
                RoadId holderTraversal = EastStraight, waiterTraversal = SouthLeft;
                var grant = atMeet.First(r => r.TrafficId == holder.TrafficId && r.IsEffectiveGrant && r.Contains(holderTraversal));
                Assert.That(grant.Status, Is.EqualTo(JunctionGrantStatus.Granted), "C.2 : grant frais au lot commun, jamais Held");
                var denial = atMeet.Single(r => r.TrafficId == waiter.TrafficId && r.TraversalId == waiterTraversal);
                Assert.That(denial.Status, Is.EqualTo(JunctionGrantStatus.Denied), denial.ToText());
                Assert.That(denial.Reason, Is.EqualTo(JunctionReason.YieldToPriority), "C sous 5.35 : refus initial par preseance : " + denial.ToText());
                Assert.That(grant.ControlKind, Is.EqualTo(JunctionControlKind.Priority), grant.ToText());
                Assert.That(denial.ControlKind, Is.EqualTo(JunctionControlKind.Yield), denial.ToText());
                Assert.That(denial.CauseActorId, Is.EqualTo(holder.TrafficId), "le refus cite le titulaire");
                Assert.That(denial.ZoneId, Is.EqualTo(Zone23), "le refus cite ConflictZones[23]");
                Assert.That(waiter.TrafficId.CompareTo(holder.TrafficId), Is.LessThan(0), "C conserve la branche au plus petit TrafficId");
                Assert.That(batches.SelectMany(b => b.Records).Any(r => r.Reason == JunctionReason.GrantedDeadlockBreak), Is.False,
                    "une preseance resoluble ne requiert aucun briseur d'interblocage");
                sequence.Add("2. un seul grant : " + grant.ToText());
                sequence.Add("3. refus : " + denial.ToText());

                ulong lastWaiting;
                float minimum = MinimumDistanceWithoutGrant(waiter, waiterTraversal, out lastWaiting);
                Assert.That(minimum, Is.GreaterThan(0f), "pare-chocs avant jamais au-dela de la frontiere b sans grant");
                var stop = waiter.Record.Trace.FirstOrDefault(t => t.Interaction.FrameId > meet && t.Interaction.FrameId <= lastWaiting
                    && Mathf.Abs(t.LongitudinalSpeed) < 0.05f && t.Interaction.Hold.Active);
                Assert.That(stop.Interaction.FrameId, Is.GreaterThan(0UL), "le second s'arrete avant l'entree");
                Assert.That(stop.Interaction.Junction.BoundaryMeters, Is.GreaterThan(0f), "arret mesure a la StopLine b>0 de la branche");
                if (stop.Interaction.Junction.Reason == JunctionReason.ExitBlocked)
                    Assert.That(stop.Interaction.DominantBlocker, Does.StartWith("BlockedExit:"), "sortie commune encore tenue par l'axe");
                else
                    Assert.That(stop.Interaction.DominantBlocker, Is.EqualTo("JunctionGrant:" + holder.TrafficId), "blocker de preseance ou de grant legitime");
                sequence.Add("   arret du second au pas hote " + stop.Interaction.FrameId + " a d = " + F(stop.Interaction.Junction.DistanceMeters)
                    + " m, distance minimale sans grant " + F(minimum) + " m, blocker " + stop.Interaction.DominantBlocker);

                var release = batches.FirstOrDefault(b => b.Records.Any(r => r.TrafficId == holder.TrafficId && r.TraversalId == holderTraversal
                    && r.Status == JunctionGrantStatus.Released && r.Reason == JunctionReason.Cleared));
                Assert.That(release, Is.Not.Null, "Released(Cleared) du premier");
                foreach (var refused in batches.Where(b => b.Frame > meet && b.Frame < release.Frame).SelectMany(b => b.Records)
                    .Where(r => r.TrafficId == waiter.TrafficId && r.TraversalId == waiterTraversal && r.Reason == JunctionReason.ConflictGranted))
                {
                    Assert.That(refused.Status, Is.EqualTo(JunctionGrantStatus.Denied), refused.ToText());
                    Assert.That(refused.CauseActorId, Is.EqualTo(holder.TrafficId), "ConflictGranted subsequent cite l'axe");
                    Assert.That(refused.ZoneId, Is.EqualTo(Zone23), "ConflictGranted subsequent cite le meme conflit");
                }
                sequence.Add("4. Released(Cleared) du premier au lot " + release.Frame);
                var served = batches.FirstOrDefault(b => b.Frame >= release.Frame && b.Records.Any(r => r.TrafficId == waiter.TrafficId
                    && r.TraversalId == waiterTraversal && r.Status == JunctionGrantStatus.Granted));
                Assert.That(served, Is.Not.Null, "Granted du second");
                // O12 : 40ca7f10 et 4e437f94 sortent sur 40e937a9. Entre le Cleared et le service, seuls des refus ExitBlocked(Occupant) ;
                // service au premier lot ou la sortie rapportee par le second atteint L + s0, ou au suivant.
                foreach (var r in batches.Where(b => b.Frame >= release.Frame && b.Frame < served.Frame).SelectMany(b => b.Records)
                    .Where(r => r.TrafficId == waiter.TrafficId && r.TraversalId == waiterTraversal))
                    Assert.That(r.Status == JunctionGrantStatus.Denied && r.Reason == JunctionReason.ExitBlocked && r.ExitBound == JunctionExitBound.Occupant,
                        Is.True, "entre Released(Cleared) et le service, seul ExitBlocked(Occupant) : " + r.ToText());
                var sufficient = waiter.Record.Trace.FirstOrDefault(t => t.Interaction.FrameId >= release.Frame && t.Interaction.Junction.HasRequest
                    && t.Interaction.Junction.TraversalId == waiterTraversal
                    && t.Interaction.Junction.ExitFreeMeters >= t.Interaction.Junction.ExitRequiredMeters);
                Assert.That(sufficient.Interaction.FrameId, Is.GreaterThan(0UL), "sortie du second jamais suffisante apres Released(Cleared)");
                ulong exitReady = sufficient.Interaction.FrameId;
                Assert.That(served.Frame >= exitReady && served.Frame - exitReady <= 1UL, Is.True,
                    "servi au lot ou la sortie devient suffisante (" + exitReady + ") ou au suivant, servi au lot " + served.Frame);
                sequence.Add("5. Granted du second au lot " + served.Frame + " (O12 : sortie suffisante au lot " + exitReady + ", "
                    + F(sufficient.Interaction.Junction.ExitFreeMeters) + " m >= " + F(sufficient.Interaction.Junction.ExitRequiredMeters)
                    + " m ; " + (served.Frame - release.Frame) + " lots apres Released(Cleared))");
                var entered = waiter.Record.Trace.FirstOrDefault(t => t.Interaction.FrameId > served.Frame && t.ElementId == waiterTraversal);
                Assert.That(entered.Interaction.FrameId, Is.GreaterThan(0UL), "le second entre dans son mouvement");
                sequence.Add("6. entree du second au pas hote " + entered.Interaction.FrameId + ", sortie au portail : " + waiter.Record.HasReachedExitPortal);
                sequence.Add("7. " + AssertStopBandAndResume(waiter, waiterTraversal, stop.Interaction.Junction.DistanceMeters, frontMeters));
                foreach (var batch in batches)
                    Assert.That(batch.Records.Count(r => r.IsEffectiveGrant && r.Contains(EastStraight))
                        + batch.Records.Count(r => r.IsEffectiveGrant && r.Contains(SouthLeft)), Is.LessThanOrEqualTo(1),
                        "au lot " + batch.Frame + " : 40ca7f10 et 4e437f94 tenus ensemble");
            }
            catch (AssertionException failure)
            {
                sequence.Add("ECHEC : " + failure.Message);
                throw;
            }
            finally
            {
                WriteSummary("scenario-C", stamp, record, spawner, observer, runs, sequence, vehicleContacts, obstacleContacts, otherContacts, trace, journal);
            }
        }

        // ================================================================== scenario D

        [UnityTest]
        [Timeout(900000)]
        public IEnumerator ScenarioDAnInsufficientExitIsRefusedBeforeAnyConflictNeverEnteredThenServed()
        {
            Story533Harness.ScenarioRecord record = null;
            yield return Enter("D", (r, f, a) => record = r);
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            var exitObstacle = harness.CreateObstacle(record.Obstacles[0]);
            var waitObstacle = harness.CreateObstacle(record.Obstacles[1]);
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation);
            var batches = new List<Batch>();
            ulong waitRemovedAt = 0UL, exitRemovedAt = 0UL;
            int settled = 0, blocked = 0;
            float frontMeters = float.NaN;
            for (int step = 0; step < record.MaxSteps && spawner.V2Removals < record.Insertions.Length; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0) Assert.Inconclusive("run invalide : joueur hote dans un fait leader ou obstacle V2");
                if (observer.ToleranceLatch != null) break;
                Capture(spawner, batches);
                var drivers = Drivers(spawner, 2);
                if (drivers == null) continue;
                if (waitRemovedAt == 0UL)
                {
                    // Le premier, passe par 40ca7f10, est tenu sur 40e937a9 ; le second attend sur son approche.
                    settled = Held(drivers[0]) && Held(drivers[1]) && drivers[0].Trace.Any(t => t.ElementId == EastStraight) ? settled + 1 : 0;
                    if (settled < SettleSteps) continue;
                    frontMeters = TrafficV2VehicleDriver.FootprintOf(drivers[1].GetComponent<BoxCollider>()).FrontMeters;
                    harness.Destroy(waitObstacle);
                    waitRemovedAt = spawner.V2Runner.FrameId;
                    continue;
                }
                if (exitRemovedAt != 0UL) continue;
                var junction = Last(drivers[1]).Junction;
                blocked = Held(drivers[1]) && junction.TraversalId == SouthLeft && !junction.GrantEffective ? blocked + 1 : 0;
                if (blocked < ExitBlockedObservationSteps) continue;
                harness.Destroy(exitObstacle);
                exitRemovedAt = spawner.V2Runner.FrameId;
            }

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace("scenario-D", stamp, runs, Folder);
            string journal = Story533Harness.WriteJunctionLog("scenario-D", stamp, observer, Folder);
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            var sequence = new List<string> { "obstacle d'attente retire au pas hote " + waitRemovedAt + ", obstacle de sortie retire au pas hote " + exitRemovedAt };
            var first = runs.Single(r => r.Index == 0);
            var second = runs.Single(r => r.Index == 1);
            var exitCorridor = RoadId.Parse(record.Obstacles[0].ElementId);
            WriteSummary("scenario-D", stamp, record, spawner, observer, runs, sequence, vehicleContacts, obstacleContacts, otherContacts, trace, journal);

            Assert.That(waitRemovedAt, Is.GreaterThan(0UL), "le premier n'a pas ete tenu sur le corridor de sortie");
            Assert.That(exitRemovedAt, Is.GreaterThan(0UL), "le second n'a pas ete tenu devant une sortie insuffisante");
            var firstHold = first.Record.Trace.Last(t => t.Interaction.FrameId <= waitRemovedAt);
            Assert.That(firstHold.ElementId, Is.EqualTo(exitCorridor), "le premier est tenu sur le corridor de sortie commun 40e937a9");
            Assert.That(batches.Where(b => b.Frame <= waitRemovedAt).SelectMany(b => b.Records).Any(r => r.TrafficId == first.TrafficId
                && r.TraversalId == EastStraight && r.Status == JunctionGrantStatus.Released), Is.True,
                "le premier est entierement hors du mouvement : grant libere");
            sequence.Add("premier tenu sur " + exitCorridor + " au pas hote " + firstHold.Interaction.FrameId + ", grant 40ca7f10 libere");
            var firstDecision = batches.Where(b => b.Frame > waitRemovedAt).SelectMany(b => b.Records)
                .FirstOrDefault(r => r.TrafficId == second.TrafficId && r.TraversalId == SouthLeft);
            Assert.That(firstDecision.Status, Is.EqualTo(JunctionGrantStatus.Denied), "premiere decision sur 4e437f94 : " + firstDecision.ToText());
            Assert.That(firstDecision.Reason, Is.EqualTo(JunctionReason.ExitBlocked), "sortie evaluee avant tout conflit : " + firstDecision.ToText());
            sequence.Add("refus du second : " + firstDecision.ToText());
            Assert.That(batches.Where(b => b.Frame < exitRemovedAt).SelectMany(b => b.Records)
                .Any(r => r.TrafficId == second.TrafficId && r.IsEffectiveGrant && r.Contains(SouthLeft)), Is.False,
                "aucun grant tant que la sortie est insuffisante");
            ulong lastWaiting;
            float minimum = MinimumDistanceWithoutGrant(second, SouthLeft, out lastWaiting);
            Assert.That(minimum, Is.GreaterThan(0f), "pare-chocs avant jamais au-dela de la frontiere b");
            float boundary = second.Record.Trace.First(t => t.Interaction.Junction.HasRequest
                && t.Interaction.Junction.TraversalId == SouthLeft).Interaction.Junction.BoundaryMeters;
            Assert.That(second.Record.Trace.Where(t => t.Interaction.FrameId < exitRemovedAt).Any(t =>
            {
                var piece = second.Record.Tracks[t.TrackIndex].Pieces.FirstOrDefault(p => p.Id == SouthLeft);
                return piece != null && piece.StartDistanceMeters + boundary - piece.ElementStartSMeters
                    - t.RouteDistanceMeters - frontMeters <= 0f;
            }), Is.False, "jamais franchi b tant que la sortie est insuffisante");
            var held = second.Record.Trace.Last(t => t.Interaction.FrameId < exitRemovedAt);
            Assert.That(held.Interaction.DominantBlocker, Does.StartWith("BlockedExit:"), "blocker BlockedExit legitime");
            var served = batches.FirstOrDefault(b => b.Frame >= exitRemovedAt && b.Records.Any(r => r.TrafficId == second.TrafficId
                && r.TraversalId == SouthLeft && r.Status == JunctionGrantStatus.Granted));
            Assert.That(served, Is.Not.Null, "servi apres le retrait de l'obstacle");
            sequence.Add("arret du second a d min " + F(minimum) + " m, blocker " + held.Interaction.DominantBlocker + ", servi au lot " + served.Frame);
            sequence.Add(AssertStopBandAndResume(second, SouthLeft, held.Interaction.Junction.DistanceMeters, frontMeters));
            WriteSummary("scenario-D", stamp, record, spawner, observer, runs, sequence, vehicleContacts, obstacleContacts, otherContacts, trace, journal);
            AssertCleanRuns(observer, runs, vehicleContacts, obstacleContacts);
        }

        // ================================================================== saturation du collecteur

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator ASaturatedCollectorQueryMakesOnlyItsOwnVehiclePerceptionUnavailable()
        {
            Story533Harness.ScenarioRecord record = null;
            TrafficV2Admission admission = null;
            yield return Enter("C", (r, f, a) => { record = r; admission = a; });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            foreach (var obstacle in record.Obstacles) harness.CreateObstacle(obstacle);
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation);
            List<TrafficV2VehicleDriver> drivers = null;
            int settled = 0;
            for (int step = 0; step < record.MaxSteps && settled < SettleSteps; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.ToleranceLatch != null) break;
                drivers = Drivers(spawner, 2);
                settled = drivers != null && drivers.All(Held) ? settled + 1 : 0;
            }
            Assert.That(settled, Is.GreaterThanOrEqualTo(SettleSteps), "les deux vehicules ne se sont pas arretes");

            // Le spawner ne cadence plus : un runner a collecteur dedie pilote seul les deux vehicules. La caisse compte elle-meme
            // plusieurs colliders : capacite = compte du vehicule sature au plus petit rayon ou il depasse strictement l'epargne.
            spawner.enabled = false;
            var saturated = drivers[0];
            var spared = drivers[1];
            var center = saturated.GetComponent<Rigidbody>().position;
            var right = saturated.transform.right;
            foreach (float side in new[] { -1f, 1f })
            {
                var box = harness.CreateObstacle(new Story533Harness.ObstacleRecord { Label = "saturation" + side,
                    Center = new Story533Harness.VectorRecord { x = center.x + right.x * 2.6f * side, y = center.y + 0.3f, z = center.z + right.z * 2.6f * side },
                    Forward = new Story533Harness.VectorRecord { x = 0f, y = 0f, z = 1f },
                    Size = new Story533Harness.VectorRecord { x = 0.3f, y = 0.3f, z = 0.3f } });
                Assert.That(box, Is.Not.Null);
            }
            Physics.SyncTransforms();
            float radius = float.NaN;
            int capacity = 0;
            var counts = new StringBuilder();
            for (float r = 0.5f; r <= 6f; r += 0.25f)
            {
                int a = Physics.OverlapSphere(center, r, ~0, QueryTriggerInteraction.Ignore).Length;
                int b = Physics.OverlapSphere(spared.GetComponent<Rigidbody>().position, r, ~0, QueryTriggerInteraction.Ignore).Length;
                counts.Append(r.ToString("0.##", CultureInfo.InvariantCulture)).Append(':').Append(a).Append('/').Append(b).Append(' ');
                if (a > b) { radius = r; capacity = a; break; }
            }
            Assert.That(float.IsNaN(radius), Is.False, "aucun rayon ne separe les deux requetes (colliders sature/epargne par rayon) : " + counts);

            var runner = new TrafficV2StepRunner(new TrafficV2HazardCollector(capacity, radius));
            var stepped = new List<TrafficV2VehicleDriver> { saturated, spared };
            int checkedSteps = 0;
            for (int k = 0; k < 20; k++)
            {
                yield return new WaitForFixedUpdate();
                runner.Step(admission.Model, stepped);
                Assert.That(runner.LastSteppedCount, Is.EqualTo(2));
                Assert.That(saturated.LastHazardQuery.Saturated, Is.True, "requete pleine du vehicule sature au pas " + k);
                Assert.That(spared.LastHazardQuery.Saturated, Is.False, "requete du vehicule epargne au pas " + k);
                Assert.That(saturated.LastLongitudinal, Is.Not.Null);
                Assert.That(saturated.LastLongitudinal.PerceptionReason, Is.EqualTo(PerceptionUnavailableReason.HazardCollectorSaturated),
                    "perception indisponible pour le seul vehicule sature");
                Assert.That(spared.LastLongitudinal == null || spared.LastLongitudinal.PerceptionReason != PerceptionUnavailableReason.HazardCollectorSaturated,
                    Is.True, "jamais pour le vehicule epargne");
                Assert.That(saturated.LastLongitudinal.AppliedAccelerationMetersPerSecondSquared, Is.LessThanOrEqualTo(0f),
                    "perception tronquee : aucune acceleration");
                checkedSteps++;
            }
            spawner.enabled = true;
            Debug.Log("[Story534] saturation du collecteur : rayon " + radius.ToString("0.##", CultureInfo.InvariantCulture) + " m, capacite " + capacity + ", "
                + checkedSteps + " pas, comptes " + counts);
        }
    }
}
