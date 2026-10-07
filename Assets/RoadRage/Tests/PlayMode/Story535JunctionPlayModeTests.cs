using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Story 5.35 : scenarios PlayMode des genres de controle authores dans MVP_Run, avec le harnais 5.33 et les scenarios
    /// ecrits par Story535ScenarioBuilderTests. E : rencontre ZC23, la branche Yield cede a l'axe Priority (YieldToPriority qui
    /// cite l'axe), s'arrete dans la bande de sa StopLine, puis est servie. F : a la croix, l'approche de droite passe d'abord,
    /// malgre l'anciennete et le plus petit TrafficId du vehicule de gauche. G : l'entree ouest de Roundabout_SouthWest attend
    /// la liberation de l'anneau. Dans les trois : aucun contact, aucun TrackingToleranceExceeded, EnteredWithoutGrant = 0.
    /// </summary>
    [Category("Story535")]
    public sealed class Story535JunctionPlayModeTests
    {
        private const string Folder = "_bmad-output/implementation-artifacts/traffic-v2-5-35-explorations";
        private const string ScenarioPath = Folder + "/scenarios-5-35.json";
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private static readonly RoadId EastStraight = RoadId.Parse("40ca7f10a97f50a918e8c3a2a1e58493");
        private static readonly RoadId SouthLeft = RoadId.Parse("4e437f94525852d3a072a538537c2093");
        private const string WestEntryPrefix = "43605e56";
        private const string WestContinuationPrefix = "4e0c96d3";

        /// <summary>Pas d'arret stable exiges avant de retirer un obstacle d'attente (patron 5.34).</summary>
        private const int SettleSteps = 50;
        /// <summary>Tolerance d'integration sous m_ctrl de la bande d'arret declaree (O13, O14), m.</summary>
        private const float StopBandIntegrationTolerance = 0.02f;
        /// <summary>Plafond declare entre le premier pas a grant effectif et le franchissement de la frontiere (O13), s.</summary>
        private const float ResumeCeilingSeconds = 3f;

        private Story533Harness harness;
        private DriverProfileDef slowHolderProfile;
        private float stagedFrontMeters;

        [SetUp]
        public void SetUp()
        {
            TrafficV2Session.Reset();
            harness = new Story533Harness();
            stagedFrontMeters = float.NaN;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return harness.Cleanup();
            if (slowHolderProfile != null) Object.Destroy(slowHolderProfile);
        }

        private static float Speed(TrafficV2VehicleDriver driver)
        {
            return driver == null || driver.Trace.Count == 0 ? 0f : driver.Trace[driver.Trace.Count - 1].LongitudinalSpeed;
        }

        private static bool Held(TrafficV2VehicleDriver driver)
        {
            return driver != null && driver.Trace.Count > 0 && Mathf.Abs(Speed(driver)) < 0.05f
                && driver.Trace[driver.Trace.Count - 1].Interaction.Hold.Active;
        }

        private static TrafficV2VehicleDriver DriverAt(PortalTrafficSpawner spawner, int index)
        {
            foreach (var insertion in spawner.ScenarioInsertions)
                if (insertion.Index == index) return Story533Harness.DriverOf(spawner, insertion.TrafficId);
            return null;
        }

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

        private IEnumerator Enter(string label, Action<Story533Harness.ScenarioRecord, TrafficV2Admission> loaded)
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Story533Harness.ScenarioFile file;
            var record = Story533Harness.Load(admission, label, out file, ScenarioPath);
            if (label == "G-gap")
            {
                var objectives = new[] { WestContinuationPrefix, WestEntryPrefix };
                var triplets = record.Insertions.Select((r, i) => new CampaignTriplet(RoadId.Parse(r.Entry), RoadId.Parse(r.Exit), r.Seed,
                    admission.Model.Movements.Single(m => m.Id.ToString().StartsWith(objectives[i], StringComparison.Ordinal)).Id)).ToArray();
                TrafficV2Session.Request(TrafficComposition.V2Slice, new MeasurementRun(MeasurementKind.Exploratory, label, triplets, 2));
            }
            else TrafficV2Session.Request(TrafficComposition.V2Slice, null, Story533Harness.ToScenario(record));
            yield return harness.EnterMvpRun();
            int parked = 0;
            yield return harness.ParkHostPlayer(file.Parking.Value, n => parked = n);
            float playerDistance = Story533Harness.PlayerDistanceToRoutes(admission.Model, record);
            Assert.That(parked == 0 || playerDistance > TrafficV2Settings.HazardQueryRadiusMeters + 5f, Is.True,
                "joueur hote stationne hors des routes : " + playerDistance + " m");
            loaded(record, admission);
        }

        /// <summary>Premier lot ou le vehicule tient un grant effectif sur la traversee ; ulong.MaxValue sinon.</summary>
        private static ulong FirstGrant(List<Batch> batches, RoadId trafficId, RoadId traversal)
        {
            var batch = batches.FirstOrDefault(b => b.Records.Any(r => r.TrafficId == trafficId && r.IsEffectiveGrant && r.Contains(traversal)));
            return batch == null ? ulong.MaxValue : batch.Frame;
        }

        /// <summary>Aucun contact, aucun TrackingToleranceExceeded, repli seulement au portail, retrait de tous au portail.</summary>
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

        /// <summary>
        /// Arret sans grant dans la bande declaree [m_ctrl - tolerance ; fenetre de maintien] de la frontiere b (d est mesure a b),
        /// jamais au-dela ; puis, des le premier pas a grant effectif, ni maintien ni contrainte JunctionEntry jusqu'au
        /// franchissement de b (d &lt;= 0), atteint en au plus ResumeCeilingSeconds. Rend la ligne de sequence publiee.
        /// </summary>
        private string AssertStopBandAndResume(Story533Harness.VehicleRun run, RoadId traversal)
        {
            var rows = run.Record.Trace.ToList();
            var waiting = rows.Where(t => t.Interaction.Junction.HasRequest && t.Interaction.Junction.TraversalId == traversal
                && !t.Interaction.Junction.GrantEffective).ToList();
            Assert.That(waiting.Count, Is.GreaterThan(0), "aucune attente sans grant");
            Assert.That(waiting.Min(t => t.Interaction.Junction.DistanceMeters), Is.GreaterThan(0f), "pare-chocs jamais au-dela de b sans grant");
            var stop = waiting.FirstOrDefault(t => Mathf.Abs(t.LongitudinalSpeed) < 0.05f && t.Interaction.Hold.Active
                && t.Interaction.Hold.Cause == LongitudinalCandidateKind.JunctionEntry && t.Interaction.Junction.EntryActive);
            Assert.That(stop.Interaction.FrameId, Is.GreaterThan(0UL), "arret avant la frontiere");
            float low = TrafficV2Settings.JunctionStopControlMarginMeters - StopBandIntegrationTolerance;
            var profile = UnityEditor.AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile;
            float high = JunctionDistances.For(profile, 0f, Time.fixedDeltaTime, TrafficV2Settings.JunctionStopControlMarginMeters,
                TrafficV2Settings.StopHold.EntrySpeedMetersPerSecond).HoldWindowMeters;
            float d = stop.Interaction.Junction.DistanceMeters;
            Assert.That(d, Is.InRange(low, high), "arret sans grant hors de la bande declaree de la frontiere");
            int granted = rows.FindIndex(t => t.Interaction.Junction.HasRequest && t.Interaction.Junction.TraversalId == traversal
                && t.Interaction.Junction.GrantEffective);
            Assert.That(granted, Is.GreaterThanOrEqualTo(0), "aucun pas a grant effectif");
            Assert.That(stagedFrontMeters, Is.GreaterThan(0f), "empreinte du vehicule mesuree au staging");
            float boundary = stop.Interaction.Junction.BoundaryMeters;
            int crossed = rows.FindIndex(granted, t =>
            {
                var piece = run.Record.Tracks[t.TrackIndex].Pieces.FirstOrDefault(p => p.Id == traversal);
                return piece != null && piece.StartDistanceMeters + boundary - piece.ElementStartSMeters
                    - t.RouteDistanceMeters - stagedFrontMeters <= 0f;
            });
            Assert.That(crossed, Is.GreaterThan(granted), "frontiere jamais franchie apres le grant");
            for (int k = granted; k < crossed; k++)
            {
                Assert.That(rows[k].Interaction.Hold.Active, Is.False, "maintien apres le grant, pas hote " + rows[k].Interaction.FrameId);
                Assert.That(rows[k].Interaction.Junction.EntryActive, Is.False, "contrainte JunctionEntry apres le grant, pas hote " + rows[k].Interaction.FrameId);
            }
            float seconds = (rows[crossed].Interaction.FrameId - rows[granted].Interaction.FrameId) * Time.fixedDeltaTime;
            Assert.That(seconds, Is.LessThanOrEqualTo(ResumeCeilingSeconds), "reprise trop lente apres le grant");
            return "arret a d = " + F(d) + " m de la frontiere (b = " + F(stop.Interaction.Junction.BoundaryMeters) + " m), bande [" + F(low)
                + " ; " + F(high) + "] m ; grant effectif au pas hote " + rows[granted].Interaction.FrameId + ", frontiere franchie au pas hote "
                + rows[crossed].Interaction.FrameId + " (" + seconds.ToString("0.##", CultureInfo.InvariantCulture) + " s)";
        }

        private static void WriteSummary(string label, string stamp, Story533Harness.ScenarioRecord record, PortalTrafficSpawner spawner,
            Story533Harness.Observer observer, IEnumerable<string> sequence, List<string> vehicleContacts, List<string> obstacleContacts,
            List<string> otherContacts, string trace, string journal)
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
            text.Append("- Regles 5.35 : StopRequired ").Append(observer.StopRequired).Append(", YieldToPriority ").Append(observer.YieldToPriority)
                .Append(", GrantedMergeGap ").Append(observer.MergeGapGrants).Append(", refus de creneau ").Append(observer.MergeGapRefusals)
                .Append(", refus Crossing ").Append(observer.CrossingRefusals).Append(", briseurs ").Append(observer.DeadlockBreaks).Append('\n');
            text.Append("- Cout du coordinateur : ").Append(Story533Harness.PercentileText(observer.CoordinatorMilliseconds))
                .Append(" ; pas hote ").Append(Story533Harness.PercentileText(observer.HostStepMilliseconds)).Append('\n');
            text.Append("- Contacts entre vehicules ").Append(vehicleContacts.Count).Append(", obstacles ").Append(obstacleContacts.Count)
                .Append(", autres ").Append(otherContacts.Count).Append('\n');
            foreach (var line in vehicleContacts.Concat(obstacleContacts).Concat(otherContacts)) text.Append("  - ").Append(line).Append('\n');
            text.Append("- Couverture : ").Append(observer.ToleranceLatch == null ? "aucun TrackingToleranceExceeded" : "ECHEC " + observer.ToleranceLatch).Append('\n');
            text.Append("- Invariants : ").Append(observer.Violations.Count == 0 ? "verts" : string.Join(" | ", observer.Violations)).Append('\n');
            text.Append("- Trace brute : `").Append(Path.GetFileName(trace)).Append("` ; journal de coordination : `").Append(Path.GetFileName(journal)).Append("`\n");
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Folder + "/" + label + "-" + stamp + "-summary.md", text.ToString());
            Debug.Log("[Story535] " + label + " : " + text.ToString().Replace("\n", " "));
        }

        /// <summary>
        /// Mise en scene a deux vehicules (E, F) : les deux s'arretent derriere leur obstacle d'attente ; celui de l'insertion 0 est
        /// libere, celui de l'insertion 1 au pas suivant. Rend les pas hote des deux retraits.
        /// </summary>
        private IEnumerator RunPair(PortalTrafficSpawner spawner, Story533Harness.ScenarioRecord record, Story533Harness.Observer observer,
            List<Batch> batches, ulong[] removed)
        {
            var obstacles = record.Obstacles.Select(o => harness.CreateObstacle(o)).ToList();
            int settled = 0;
            for (int step = 0; step < record.MaxSteps && spawner.V2Removals < record.Insertions.Length; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0) Assert.Inconclusive("run invalide : joueur hote dans un fait leader ou obstacle V2");
                if (observer.ToleranceLatch != null) break;
                Capture(spawner, batches);
                if (removed[0] != 0UL)
                {
                    if (removed[1] == 0UL && spawner.V2Runner.FrameId == removed[0] + 1UL)
                    {
                        harness.Destroy(obstacles[1]);
                        removed[1] = spawner.V2Runner.FrameId;
                    }
                    continue;
                }
                var first = DriverAt(spawner, 0);
                var second = DriverAt(spawner, 1);
                settled = Held(first) && Held(second) ? settled + 1 : 0;
                if (settled < SettleSteps) continue;
                stagedFrontMeters = TrafficV2VehicleDriver.FootprintOf(first.GetComponent<BoxCollider>()).FrontMeters;
                harness.Destroy(obstacles[0]);
                removed[0] = spawner.V2Runner.FrameId;
            }
        }

        // ================================================================== scenario E

        [UnityTest]
        [Timeout(900000)]
        public IEnumerator ScenarioEBranchYieldsToTheAxisStopsAtItsLineAndIsServedAfterIt()
        {
            Story533Harness.ScenarioRecord record = null;
            TrafficV2Admission admission = null;
            yield return Enter("E", (r, a) => { record = r; admission = a; });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation, admission.Model);
            var batches = new List<Batch>();
            var removed = new ulong[2];
            yield return RunPair(spawner, record, observer, batches, removed);

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace("scenario-E", stamp, runs, Folder);
            string journal = Story533Harness.WriteJunctionLog("scenario-E", stamp, observer, Folder);
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            var sequence = new List<string> { "obstacle de la branche retire au pas hote " + removed[0] + ", obstacle de l'axe au pas hote " + removed[1] };
            try
            {
                Assert.That(removed[0], Is.GreaterThan(0UL), "les deux vehicules ne se sont pas arretes derriere leur obstacle d'attente");
                var branch = runs.Single(r => r.Index == 0);
                var axis = runs.Single(r => r.Index == 1);
                var yielded = batches.SelectMany(b => b.Records).FirstOrDefault(r => r.TrafficId == branch.TrafficId && r.TraversalId == SouthLeft
                    && r.Status == JunctionGrantStatus.Denied && r.Reason == JunctionReason.YieldToPriority);
                Assert.That(yielded.TrafficId, Is.EqualTo(branch.TrafficId), "la branche n'a jamais cede (Denied(YieldToPriority))");
                Assert.That(yielded.CauseActorId, Is.EqualTo(axis.TrafficId), "le refus cite le vehicule d'axe : " + yielded.ToText());
                Assert.That(yielded.ControlKind, Is.EqualTo(JunctionControlKind.Yield), yielded.ToText());
                sequence.Add("1. " + yielded.ToText());
                ulong axisGrant = FirstGrant(batches, axis.TrafficId, EastStraight), branchGrant = FirstGrant(batches, branch.TrafficId, SouthLeft);
                Assert.That(axisGrant, Is.LessThan(branchGrant), "l'axe est servi avant la branche");
                sequence.Add("2. grant de l'axe au lot " + axisGrant + ", de la branche au lot " + branchGrant);
                sequence.Add("3. " + AssertStopBandAndResume(branch, SouthLeft));
                AssertCleanRuns(observer, runs, vehicleContacts, obstacleContacts);
            }
            catch (AssertionException failure)
            {
                sequence.Add("ECHEC : " + failure.Message);
                throw;
            }
            finally
            {
                WriteSummary("scenario-E", stamp, record, spawner, observer, sequence, vehicleContacts, obstacleContacts, otherContacts, trace, journal);
            }
        }

        // ================================================================== scenario F

        /// <summary>Premier mouvement de la croix sur la route de l'insertion.</summary>
        private static RoadId CrossMovement(CompiledRoadModel model, Story533Harness.InsertionRecord insertion)
        {
            var cross = new HashSet<RoadId>(model.Junctions.Where(j => j.Feature == JunctionFeature.Crossroads).Select(j => j.Id));
            foreach (var id in insertion.Elements)
            {
                CompiledJunctionMovement movement;
                if (model.TryGetMovement(RoadId.Parse(id), out movement) && cross.Contains(movement.JunctionId)) return movement.Id;
            }
            Assert.Fail("route sans mouvement de la croix");
            return RoadId.None;
        }

        [UnityTest]
        [Timeout(900000)]
        public IEnumerator ScenarioFTheApproachFromTheRightPassesFirstAtTheCrossroads()
        {
            Story533Harness.ScenarioRecord record = null;
            TrafficV2Admission admission = null;
            yield return Enter("F", (r, a) => { record = r; admission = a; });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation, admission.Model);
            var batches = new List<Batch>();
            var removed = new ulong[2];
            yield return RunPair(spawner, record, observer, batches, removed);

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace("scenario-F", stamp, runs, Folder);
            string journal = Story533Harness.WriteJunctionLog("scenario-F", stamp, observer, Folder);
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            var sequence = new List<string> { "obstacle de gauche retire au pas hote " + removed[0] + ", obstacle de droite au pas hote " + removed[1] };
            try
            {
                Assert.That(removed[0], Is.GreaterThan(0UL), "les deux vehicules ne se sont pas arretes derriere leur obstacle d'attente");
                var left = runs.Single(r => r.Index == 0);
                var right = runs.Single(r => r.Index == 1);
                RoadId leftMovement = CrossMovement(admission.Model, record.Insertions[0]), rightMovement = CrossMovement(admission.Model, record.Insertions[1]);
                var index = JunctionConflictIndex.For(admission.Model);
                Assert.That(index.HasPrecedence(rightMovement, leftMovement), Is.True, "precondition : l'insertion 1 vient de la droite");
                Assert.That(left.TrafficId.CompareTo(right.TrafficId), Is.LessThan(0), "precondition : le vehicule de gauche a le plus petit TrafficId");
                ulong rightGrant = FirstGrant(batches, right.TrafficId, rightMovement), leftGrant = FirstGrant(batches, left.TrafficId, leftMovement);
                Assert.That(rightGrant, Is.LessThan(leftGrant), "l'approche de droite passe d'abord");
                var refusal = batches.SelectMany(b => b.Records).FirstOrDefault(r => r.TrafficId == left.TrafficId && r.TraversalId == leftMovement
                    && r.Status == JunctionGrantStatus.Denied);
                sequence.Add("1. grant de droite au lot " + rightGrant + ", de gauche au lot " + leftGrant);
                sequence.Add("2. premier refus de gauche : " + (refusal.TrafficId.IsEmpty ? "aucun" : refusal.ToText()));
                Assert.That(left.Record.Trace.Any(t => t.ElementId == leftMovement && t.Interaction.FrameId <= rightGrant), Is.False,
                    "le vehicule de gauche n'entre pas avant le grant de droite");
                AssertCleanRuns(observer, runs, vehicleContacts, obstacleContacts);
            }
            catch (AssertionException failure)
            {
                sequence.Add("ECHEC : " + failure.Message);
                throw;
            }
            finally
            {
                WriteSummary("scenario-F", stamp, record, spawner, observer, sequence, vehicleContacts, obstacleContacts, otherContacts, trace, journal);
            }
        }

        // ================================================================== scenario G

        [UnityTest]
        [Timeout(900000)]
        public IEnumerator ScenarioGGapTheEntryClearsTheMergeWhileTheDistantRingHolderKeepsItsGrant()
        {
            Story533Harness.ScenarioRecord record = null;
            TrafficV2Admission admission = null;
            yield return Enter("G-gap", (r, a) => { record = r; admission = a; });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            var entry = admission.Model.Movements.Single(m => m.Id.ToString().StartsWith(WestEntryPrefix, StringComparison.Ordinal)).Id;
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation, admission.Model);
            var batches = new List<Batch>();
            var waiting = harness.CreateObstacle(record.Obstacles.Single(o => o.Label == "attente-entree"));
            var distant = harness.CreateObstacle(record.Obstacles.Single(o => o.Label == "attente-anneau-lointain"));
            slowHolderProfile = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath));
            var d = slowHolderProfile.Profile;
            var slow = new DriverProfile(d.DesiredSpeed, d.TimeHeadway, d.MinimumGap, d.MaxAcceleration / 4f,
                d.ComfortableDeceleration, d.Politeness, d.LaneChangeThreshold, d.SafeBrakingLimit,
                d.ReactionTime, d.LaneChangeEvaluationInterval, d.Consistency, d.AimPointRecallSpeed);
            typeof(DriverProfileDef).GetField("profile", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(slowHolderProfile, slow);
            ulong entrantReleased = 0, holderReleased = 0, gapFrame = 0;
            JunctionRecord gap = default(JunctionRecord);
            int settled = 0;
            for (int step = 0; step < record.MaxSteps && spawner.V2Removals < record.Insertions.Length; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0) Assert.Inconclusive("run invalide : joueur hote dans un fait V2");
                if (observer.ToleranceLatch != null) break;
                Capture(spawner, batches);
                var holder = Story533Harness.DriverOf(spawner, TrafficV2Lifecycle.TrafficIdentity(record.Insertions[0].Seed, 1));
                var entrant = Story533Harness.DriverOf(spawner, TrafficV2Lifecycle.TrafficIdentity(record.Insertions[1].Seed, 2));
                if (holder != null && holder.DriverProfileDefinition != slowHolderProfile)
                    typeof(TrafficV2VehicleDriver).GetField("driverProfile", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(holder, slowHolderProfile);
                if (entrantReleased == 0)
                {
                    settled = Held(holder) && Held(entrant) ? settled + 1 : 0;
                    if (settled >= SettleSteps) { harness.Destroy(waiting); entrantReleased = spawner.V2Runner.FrameId; }
                    continue;
                }
                if (holderReleased != 0 || holder == null || entrant == null) continue;
                var snapshot = spawner.V2Runner.JunctionSnapshot;
                var granted = snapshot.Records.FirstOrDefault(r => r.TrafficId == entrant.TrafficId && r.TraversalId == entry
                    && r.IsEffectiveGrant && r.Reason == JunctionReason.GrantedMergeGap);
                if (gapFrame == 0 && !granted.TrafficId.IsEmpty)
                {
                    var continuation = admission.Model.Movements.Single(m => m.Id.ToString().StartsWith(WestContinuationPrefix, StringComparison.Ordinal)).Id;
                    Assert.That(snapshot.Records.Any(r => r.TrafficId == holder.TrafficId && r.IsEffectiveGrant && r.Contains(continuation)), Is.True,
                        "le titulaire garde son grant pendant l'admission par creneau");
                    Assert.That(Held(holder), Is.True, "H reste retenu pendant l'admission de R");
                    gap = granted; gapFrame = snapshot.SourceFrame;
                }
                // Le nez et l'arriere de R ont libere la fusion avant de laisser H repartir.
                if (gapFrame != 0 && snapshot.Records.Any(r => r.TrafficId == entrant.TrafficId && r.IsEffectiveGrant
                    && r.TraversalId == entry && !r.Contains(entry)))
                { harness.Destroy(distant); holderReleased = snapshot.SourceFrame; }
            }
            var runs = Story533Harness.Runs(spawner);
            foreach (var run in runs)
                for (int i = 0; i < record.Insertions.Length; i++)
                    if (run.TrafficId == TrafficV2Lifecycle.TrafficIdentity(record.Insertions[i].Seed, (ulong)i + 1)) run.Index = i;
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace("scenario-G-gap", stamp, runs, Folder);
            string journal = Story533Harness.WriteJunctionLog("scenario-G-gap", stamp, observer, Folder);
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            var holderRun = runs.Single(r => r.Index == 0);
            var entrantRun = runs.Single(r => r.Index == 1);
            var following = holderRun.Record.Trace.Where(r => r.Interaction.FrameId > holderReleased
                && r.Interaction.LeaderId == entrantRun.TrafficId && float.IsFinite(r.Interaction.LeaderGapMeters)).ToArray();
            int boundFollowing = following.Count(r => r.Interaction.BindingConstraint == SpeedConstraint.LeaderFollowing);
            var sequence = new List<string> { "entrant libere au pas " + entrantReleased,
                "grant par creneau au lot " + gapFrame + " : " + (gapFrame == 0 ? "absent" : gap.ToText()),
                "anneau libere apres degagement au pas " + holderReleased,
                "poursuite de R par H : " + following.Length + " pas, dont " + boundFollowing + " bornes par LeaderFollowing",
                "profil H en memoire : acceleration " + F(slow.MaxAcceleration) + " m/s2 ; vitesse desiree " + F(slow.DesiredSpeed) + " m/s" };
            try
            {
                Assert.That(entrantReleased, Is.GreaterThan(0UL), "staging des deux vehicules");
                Assert.That(gapFrame, Is.GreaterThan(0UL), "aucun GrantedMergeGap sur poses et routes reelles");
                Assert.That(gap.EtaSeconds, Is.GreaterThanOrEqualTo(gap.GapSeconds));
                Assert.That(holderReleased, Is.GreaterThan(gapFrame), "H ne repart qu'apres le degagement de R");
                Assert.That(observer.MergeGapGrants, Is.GreaterThan(0));
                Assert.That(observer.MaxLive, Is.EqualTo(2));
                Assert.That(spawner.V2Removals, Is.EqualTo(2));
                Assert.That(following, Is.Not.Empty, "H doit percevoir R comme leader apres la fusion");
                Assert.That(boundFollowing, Is.GreaterThan(0), "poursuite longitudinale effectivement exercee");
                Assert.That(following.All(r => r.Interaction.LeaderGapMeters > 0f), Is.True, "jeu longitudinal positif");
                AssertCleanRuns(observer, runs, vehicleContacts, obstacleContacts);
            }
            catch (AssertionException failure) { sequence.Add("ECHEC : " + failure.Message); throw; }
            finally { WriteSummary("scenario-G-gap", stamp, record, spawner, observer, sequence, vehicleContacts, obstacleContacts,
                otherContacts, trace, journal); }
        }

        [UnityTest]
        [Timeout(900000)]
        public IEnumerator ScenarioGTheRoundaboutEntryWaitsForTheRingToClear()
        {
            Story533Harness.ScenarioRecord record = null;
            TrafficV2Admission admission = null;
            yield return Enter("G", (r, a) => { record = r; admission = a; });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);
            var model = admission.Model;
            var entry = model.Movements.Single(m => m.Id.ToString().StartsWith(WestEntryPrefix, StringComparison.Ordinal)).Id;
            var continuation = model.Movements.Single(m => m.Id.ToString().StartsWith(WestContinuationPrefix, StringComparison.Ordinal)).Id;
            RoadId mergeZone;
            Assert.That(JunctionConflictIndex.For(model).TryGetConflict(entry, continuation, out mergeZone), Is.True);
            var observer = new Story533Harness.Observer(spawner, record.MaxPopulation, model);
            var batches = new List<Batch>();
            var waiting = harness.CreateObstacle(record.Obstacles[0]);
            GameObject ringWaiting = null;
            ulong ringReleasedAt = 0UL;
            ulong removedAt = 0UL;
            int settled = 0;
            for (int step = 0; step < record.MaxSteps && spawner.V2Removals < record.Insertions.Length; step++)
            {
                yield return new WaitForFixedUpdate();
                observer.Observe();
                if (observer.PlayerFacts > 0) Assert.Inconclusive("run invalide : joueur hote dans un fait leader ou obstacle V2");
                if (observer.ToleranceLatch != null) break;
                Capture(spawner, batches);
                // L'approche initiale est dans le rayon de degagement du portail (12 m). L'obstacle ne doit exister
                // qu'apres la premiere insertion, alors que le vehicule est encore loin de sa face proche.
                if (ringWaiting == null && ringReleasedAt == 0UL && DriverAt(spawner, 0) != null)
                    ringWaiting = harness.CreateObstacle(record.Obstacles.Single(o => o.Label == "attente-anneau"));
                if (removedAt != 0UL) continue;
                settled = Held(DriverAt(spawner, 1)) && (ringReleasedAt != 0UL || Held(DriverAt(spawner, 0))) ? settled + 1 : 0;
                if (settled < SettleSteps) continue;
                if (ringReleasedAt == 0UL)
                {
                    harness.Destroy(ringWaiting);
                    ringReleasedAt = spawner.V2Runner.FrameId;
                }
                // Retrait quand un vehicule d'anneau aborde l'element qui precede la fusion ouest, sans l'avoir encore passee.
                foreach (int ring in new[] { 0, 2 })
                {
                    if (removedAt != 0UL) break;
                    var driver = DriverAt(spawner, ring);
                    if (driver == null || driver.Trace.Count == 0) continue;
                    var elements = record.Insertions[ring].Elements;
                    int merge = Array.IndexOf(elements, continuation.ToString());
                    int at = Array.IndexOf(elements, driver.Trace[driver.Trace.Count - 1].ElementId.ToString());
                    if (merge > 0 && at == merge - 1)
                    {
                        harness.Destroy(waiting);
                        removedAt = spawner.V2Runner.FrameId;
                    }
                }
            }

            var runs = Story533Harness.Runs(spawner);
            string stamp = Story533Harness.Stamp();
            string trace = Story533Harness.WriteTrace("scenario-G", stamp, runs, Folder);
            string journal = Story533Harness.WriteJunctionLog("scenario-G", stamp, observer, Folder);
            List<string> vehicleContacts, obstacleContacts, otherContacts;
            Story533Harness.Contacts(runs, out vehicleContacts, out obstacleContacts, out otherContacts);
            var sequence = new List<string> { "obstacle d'anneau retire au pas hote " + ringReleasedAt,
                "obstacle de l'entree retire au pas hote " + removedAt };
            try
            {
                Assert.That(ringReleasedAt, Is.GreaterThan(0UL), "le vehicule d'anneau et l'entrant ne se sont pas stabilises derriere leurs obstacles");
                Assert.That(removedAt, Is.GreaterThan(0UL), "aucun vehicule d'anneau n'a aborde la fusion pendant l'attente de l'entrant");
                Assert.That(removedAt, Is.GreaterThan(ringReleasedAt), "l'anneau est libere avant la rencontre a la fusion");
                var entering = runs.Single(r => r.Index == 1);
                var ring = new HashSet<RoadId>(runs.Where(r => r.Index == 0 || r.Index == 2).Select(r => r.TrafficId));
                ulong granted = FirstGrant(batches, entering.TrafficId, entry);
                Assert.That(granted, Is.LessThan(ulong.MaxValue), "l'entrant n'est jamais servi");
                var waited = batches.Where(b => b.Frame < granted).SelectMany(b => b.Records).Where(r => r.TrafficId == entering.TrafficId
                    && r.TraversalId == entry && r.Status == JunctionGrantStatus.Denied && ring.Contains(r.CauseActorId)
                    && r.ZoneId == mergeZone && (r.Reason == JunctionReason.ConflictGranted || r.Reason == JunctionReason.YieldToPriority)).ToList();
                Assert.That(waited, Is.Not.Empty, "l'entree n'a jamais attendu un vehicule d'anneau (precondition absente, run invalide)");
                Assert.That(waited.Any(r => r.EtaSeconds < r.GapSeconds), Is.True,
                    "au moins un refus doit prouver un creneau insuffisant a la fusion ouest, pas seulement une sortie bloquee");
                sequence.Add("1. premier refus de l'entrant : " + waited[0].ToText());
                sequence.Add("2. raisons des refus : " + string.Join(", ", waited.Select(r => r.Reason.ToString()).Distinct().ToArray()));
                sequence.Add("3. grant de l'entrant au lot " + granted);
                Assert.That(entering.Record.Trace.Any(t => t.ElementId == entry && t.Interaction.FrameId <= granted), Is.False,
                    "l'entrant n'entre pas avant son grant");
                AssertCleanRuns(observer, runs, vehicleContacts, obstacleContacts);
            }
            catch (AssertionException failure)
            {
                sequence.Add("ECHEC : " + failure.Message);
                throw;
            }
            finally
            {
                WriteSummary("scenario-G", stamp, record, spawner, observer, sequence, vehicleContacts, obstacleContacts, otherContacts, trace, journal);
            }
        }
    }
}
