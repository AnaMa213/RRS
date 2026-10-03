using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.33 : frame partagee et ordonnanceur (parties pures), collecteur de dangers (classification, identite,
    /// deduplication, saturation), propagation des saturations, jeton de scenario, constructeur deterministe des
    /// scenarios PlayMode, disponibilite des canaux a emprise le long des 11 routes de la campagne 5.31 et place
    /// laissee par le decor de MVP_Run dans le tampon du collecteur.
    /// </summary>
    [Category("Core")]
    [Category("Story533")]
    public sealed class Story533SharedFrameTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string V2PrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string CampaignPath = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/campaign-5-31.json";
        private const string ScenarioFolder = "_bmad-output/implementation-artifacts/traffic-v2-5-33-explorations";
        private const string ScenarioPath = ScenarioFolder + "/scenarios-5-33.json";
        private const float Dt = 0.02f;

        /// <summary>Longueur d'obstacle du scenario A (m) ; largeur et hauteur ci-dessous.</summary>
        private const float ObstacleLengthMeters = 1f;
        private const float ObstacleWidthMeters = 2f;
        private const float ObstacleHeightMeters = 1.5f;

        private static TrafficV2Admission admission;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                        File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
                Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
                return admission;
            }
        }

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; }
        }

        /// <summary>Arbitrage avec le maintien a l'arret des reglages runtime (D11), comme le driver.</summary>
        private static LongitudinalDecision Decide(SpeedPlan plan, DriverProfile driver, float speed, float dt,
            LongitudinalPerception perception, LongitudinalMemory memory)
        {
            return LongitudinalArbitration.Decide(plan, driver, speed, dt, perception, memory, TrafficV2Settings.StopHold);
        }

        private static VehicleFootprint Car
        {
            get
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(V2PrefabPath);
                return TrafficV2VehicleDriver.FootprintOf(prefab.GetComponent<BoxCollider>());
            }
        }

        // ================================================================== empreinte et ordonnanceur

        [Test]
        public void TheV2PrefabDeclaresTheFootprintOfItsBodyBoxFromTheReferencePoint()
        {
            var car = Car;
            Assert.That(car.FrontMeters, Is.EqualTo(2.22f).Within(1e-4f));
            Assert.That(car.RearMeters, Is.EqualTo(2.22f).Within(1e-4f));
            Assert.That(car.LeftMeters, Is.EqualTo(1.03f).Within(1e-4f));
            Assert.That(car.RightMeters, Is.EqualTo(1.03f).Within(1e-4f));
            Assert.That(car.ReferenceOriginLocal, Is.EqualTo(Vector3.zero));
            Assert.That(TrafficV2VehicleDriver.FootprintOf(null).FrontMeters, Is.EqualTo(0f), "sans collider : non declaree");
        }

        private sealed class Lane
        {
            public RoutePlan Route;
            public ReferenceTrack Track;
            public RoadId[] Ids;
            public RoadId TrafficId;
            public TrafficV2Insertion Insertion;
        }

        private static Lane LaneOf(TrafficV2Insertion insertion)
        {
            return new Lane { Route = insertion.Route, TrafficId = insertion.TrafficId, Insertion = insertion,
                Track = ReferenceTrack.FromRoute(Admission.Model, insertion.Route.Occurrences, 0f),
                Ids = insertion.Route.Occurrences.Select(o => o.Id).ToArray() };
        }

        private static TrafficActorInput ActorAt(RoadId id, Lane lane, float distance, float speed)
        {
            int piece = lane.Track.PieceAt(distance);
            var nominal = lane.Track.Nominal(piece, distance);
            return new TrafficActorInput(id, new VehicleFootprintPose { Position = nominal.Position,
                Forward = nominal.Forward, Up = nominal.Up, Footprint = Car }, speed, lane.Track.Pieces[piece].Id, lane.Ids,
                lane.Track.KinematicAnchors(piece));
        }

        private static string DecideText(TrafficFrame frame, RoadId id, Lane lane, float distance, float speed)
        {
            int piece = lane.Track.PieceAt(distance);
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, id, lane.Route, lane.Route.ExitPortalId,
                lane.Insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null,
                null, Admission.Evidence, RoadId.None, lane.Track.OffsetRadians(piece, distance)));
            var observation = TrafficPerception.Observe(frame, id, decision.Path, TrafficV2Settings.PerceptionLimits,
                new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity));
            var plan = SpeedPlan.Build(decision.Motion, Admission.Model, Driver, speed);
            var perception = LongitudinalPerception.From(observation, decision.Path,
                LongitudinalPerception.FrontDistanceMeters(frame, id, decision.Path), false);
            return observation.ToText() + Decide(plan, Driver, speed, Dt, perception, LongitudinalMemory.None).ToText();
        }

        [Test]
        public void OneSharedFrameGivesTheSameDecisionsWhateverTheEvaluationOrderAndLeaksNoState()
        {
            var entry = Admission.Model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).First();
            var lanes = new List<Lane>();
            for (ulong counter = 1; counter <= 3; counter++)
                lanes.Add(LaneOf(TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, RoadId.None, 0UL, counter, Driver, Dt)));
            var distances = new[] { 4f, 14f, 26f };
            var speeds = new[] { 5f, 3f, 0f };
            var inputs = Enumerable.Range(0, 3).Select(i => ActorAt(lanes[i].TrafficId, lanes[0], distances[i], speeds[i])).ToList();
            var frame = new TrafficFrame(41, Admission.Model, inputs);
            string before = string.Join("|", frame.Actors.Select(a => a.TrafficId + ":" + a.Location.ElementId + ":" + a.Location.SMeters));
            var forward = Enumerable.Range(0, 3).Select(i => DecideText(frame, lanes[i].TrafficId, LaneOn(lanes[i], lanes[0]), distances[i], speeds[i])).ToList();
            var backward = Enumerable.Range(0, 3).Reverse().Select(i => DecideText(frame, lanes[i].TrafficId, LaneOn(lanes[i], lanes[0]), distances[i], speeds[i]))
                .Reverse().ToList();
            Assert.That(backward, Is.EqualTo(forward), "decisions identiques quel que soit l'ordre d'evaluation");
            Assert.That(string.Join("|", frame.Actors.Select(a => a.TrafficId + ":" + a.Location.ElementId + ":" + a.Location.SMeters)),
                Is.EqualTo(before), "frame inchangee");
            Assert.That(forward.All(text => text.StartsWith("Frame 41 ", StringComparison.Ordinal)), Is.True, "toutes citent la meme frame");
            StringAssert.Contains("Longitudinal binding LeaderFollowing", forward[0], "le dernier suit le vehicule du milieu");
        }

        /// <summary>Meme route physique que la reference, identite propre du vehicule.</summary>
        private static Lane LaneOn(Lane own, Lane reference)
        {
            Assert.That(own.Ids, Is.EqualTo(reference.Ids), "meme entree, meme sortie : meme route");
            return own;
        }

        [Test]
        public void TheRunnerHoldsNoDecisionStateAndTheDriverNoLongerClocksItself()
        {
            var runner = new TrafficV2StepRunner();
            Assert.That(runner.FrameId, Is.EqualTo(0UL));
            runner.Step(null, null);
            runner.Step(Admission.Model, new TrafficV2VehicleDriver[0]);
            Assert.That(runner.FrameId, Is.EqualTo(2UL), "le compteur global avance a chaque pas hote");
            Assert.That(runner.FramesBuilt, Is.EqualTo(0), "aucune frame sans vehicule");
            Assert.That(runner.LastFrame, Is.Null);
            var fields = typeof(TrafficV2StepRunner).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
                Assert.That(field.FieldType == typeof(PlanningDecision) || field.FieldType == typeof(LongitudinalDecision)
                    || field.FieldType == typeof(LongitudinalMemory), Is.False, "aucun etat de decision : " + field.Name);
            foreach (var callback in new[] { "FixedUpdate", "Update", "LateUpdate" })
                Assert.That(typeof(TrafficV2VehicleDriver).GetMethod(callback, BindingFlags.Instance | BindingFlags.NonPublic
                    | BindingFlags.Public | BindingFlags.DeclaredOnly), Is.Null, "le driver ne s'auto-cadence plus : " + callback);
            string spawner = File.ReadAllText("Assets/RoadRage/App/Run/PortalTrafficSpawner.cs");
            int release = spawner.IndexOf("ReleaseVehiclesAtExitPortals();", StringComparison.Ordinal);
            int step = spawner.IndexOf("StepV2Vehicles();", StringComparison.Ordinal);
            int insert = spawner.IndexOf("TickV2Slice();", StringComparison.Ordinal);
            Assert.That(release, Is.GreaterThan(0));
            Assert.That(step, Is.GreaterThan(release), "retrait, puis frame et pas");
            Assert.That(insert, Is.GreaterThan(step), "puis insertion : un vehicule insere conduit au pas suivant");
        }

        // ================================================================== collecteur

        [Test]
        public void TheCollectorClassificationNeverTurnsAV2RootIntoAHazard()
        {
            Assert.That(TrafficV2HazardCollector.Classify(false, false, false, false, false), Is.EqualTo(HazardRootClass.Static));
            Assert.That(TrafficV2HazardCollector.Classify(true, false, true, true, true), Is.EqualTo(HazardRootClass.Self));
            Assert.That(TrafficV2HazardCollector.Classify(false, false, true, true, true), Is.EqualTo(HazardRootClass.TrafficV2Vehicle),
                "un vehicule V2 porte aussi VehiclePhysicsBody : jamais un danger Vehicle");
            Assert.That(TrafficV2HazardCollector.Classify(false, false, true, false, true), Is.EqualTo(HazardRootClass.TrafficV2Vehicle));
            Assert.That(TrafficV2HazardCollector.Classify(false, true, false, false, false), Is.EqualTo(HazardRootClass.WalkingPlayer));
            Assert.That(TrafficV2HazardCollector.Classify(false, true, true, false, false), Is.EqualTo(HazardRootClass.WalkingPlayer));
            Assert.That(TrafficV2HazardCollector.Classify(false, false, true, true, false), Is.EqualTo(HazardRootClass.Vehicle));
            Assert.That(TrafficV2HazardCollector.Classify(false, false, true, false, false), Is.EqualTo(HazardRootClass.Obstacle));
            // Aucune classification par motif de nom.
            string source = File.ReadAllText("Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs");
            foreach (var forbidden in new[] { ".name", "CompareTag", ".tag", "Contains(\"" })
                Assert.That(source, Does.Not.Contain(forbidden));
        }

        private readonly List<GameObject> created = new List<GameObject>();

        [TearDown]
        public void DestroyCreatedObjects()
        {
            foreach (var go in created) if (go != null) UnityEngine.Object.DestroyImmediate(go);
            created.Clear();
        }

        /// <summary>Objet de test detruit au TearDown ; la scene n'est jamais sauvegardee.</summary>
        private GameObject Box(string name, Vector3 position, Vector3 size)
        {
            var go = new GameObject(name);
            created.Add(go);
            go.transform.position = position;
            go.AddComponent<BoxCollider>().size = size;
            return go;
        }

        [Test]
        public void TheCollectorDeduplicatesByRootKeepsSessionIdentitiesAndCountsEveryExclusion()
        {
            {
                var origin = new Vector3(1000f, -500f, 1000f);
                Box("decor", origin + new Vector3(5f, 0f, 0f), Vector3.one);
                var player = new GameObject("pieton");
                created.Add(player);
                player.transform.position = origin + new Vector3(0f, 0f, 6f);
                player.AddComponent<CharacterController>();
                var crate = Box("caisse", origin + new Vector3(-6f, 0f, 0f), Vector3.one);
                var crateBody = crate.AddComponent<Rigidbody>();
                crateBody.isKinematic = true;
                var lid = Box("couvercle", origin + new Vector3(-6f, 1f, 0f), new Vector3(1f, 0.2f, 1f));
                lid.transform.SetParent(crate.transform, true);
                var car = Box("voiture", origin + new Vector3(0f, 0f, -8f), new Vector3(2f, 1.4f, 4.4f));
                car.AddComponent<Rigidbody>().isKinematic = true;
                car.AddComponent<VehiclePhysicsBody>();
                var self = Box("propre", origin, new Vector3(2f, 1.4f, 4.4f));
                var selfBody = self.AddComponent<Rigidbody>();
                selfBody.isKinematic = true;
                Physics.SyncTransforms();

                var collector = new TrafficV2HazardCollector(16, 20f);
                var queryId = new RoadId(0x533UL, 1UL);
                var hazards = collector.Collect(new[] { new HazardQuery(queryId, origin, selfBody) }, new HashSet<RoadId>()).ToList();
                Assert.That(collector.Reports.Single().Hits, Is.EqualTo(6), "decor, pieton, caisse (deux colliders), voiture, propre");
                Assert.That(collector.Reports[0].Saturated, Is.False);
                Assert.That(hazards.Select(h => h.Kind).OrderBy(k => k), Is.EqualTo(new[] { TrafficHazardKind.WalkingPlayer,
                    TrafficHazardKind.Obstacle, TrafficHazardKind.Vehicle }.OrderBy(k => k)), "une entree par racine");
                Assert.That(collector.Counters.ExcludedStatic, Is.EqualTo(1));
                Assert.That(collector.Counters.ExcludedSelf, Is.EqualTo(1));
                Assert.That(collector.Counters.Emitted, Is.EqualTo(3));
                var crateHazard = hazards.Single(h => h.Kind == TrafficHazardKind.Obstacle);
                Assert.That(crateHazard.Bounds.Extents.y, Is.GreaterThan(0.55f), "boite = union des colliders de la racine");
                Assert.That(hazards.All(h => h.Confidence == 1f), Is.True);
                Assert.That(hazards.All(h => h.Id.High == TrafficV2HazardCollector.HazardIdentityDomain), Is.True, "domaine danger");
                Assert.That(hazards.Select(h => h.Id).Distinct().Count(), Is.EqualTo(3));

                // Identites stables pendant la session ; un id egal a un acteur n'est pas emis.
                var again = collector.Collect(new[] { new HazardQuery(queryId, origin, selfBody) }, new HashSet<RoadId>()).ToList();
                Assert.That(again.Select(h => h.Id), Is.EqualTo(hazards.Select(h => h.Id)), "stable pendant la session");
                var collision = collector.Collect(new[] { new HazardQuery(queryId, origin, selfBody) },
                    new HashSet<RoadId> { crateHazard.Id }).ToList();
                Assert.That(collision.Any(h => h.Id == crateHazard.Id), Is.False);
                Assert.That(collector.Counters.IdentityCollisions, Is.EqualTo(1), "HazardIdentityCollision");

                // Le corps propre d'une autre requete est un danger pour elle : deduplication par racine entre requetes.
                var twoQueries = collector.Collect(new[] { new HazardQuery(queryId, origin, selfBody),
                    new HazardQuery(new RoadId(0x533UL, 2UL), origin + new Vector3(0f, 0f, -8f), null) }, new HashSet<RoadId>()).ToList();
                Assert.That(twoQueries.Count, Is.EqualTo(4), "propre (vu par la seconde), pieton, caisse, voiture : une fois chacun");
                Assert.That(collector.Reports.Count, Is.EqualTo(2));

                // Saturation : requete pleine publiee avec le vehicule emetteur.
                var small = new TrafficV2HazardCollector(3, 20f);
                small.Collect(new[] { new HazardQuery(queryId, origin, selfBody) }, null);
                HazardQueryReport report;
                Assert.That(small.TryGetReport(queryId, out report), Is.True);
                Assert.That(report.Saturated, Is.True, "resultats = capacite");
                Assert.That(small.Counters.SaturatedQueries, Is.EqualTo(1));
                Assert.That(small.SaturatedQueries, Is.EqualTo(1));
            }
        }

        [Test]
        public void AV2ActorIsNeverAlsoAHazardAndNoIdIsBothLeaderAndObstacle()
        {
            var entry = Admission.Model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).First();
            var lane = LaneOf(TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, RoadId.None, 0UL, 1UL, Driver, Dt));
            var agent = lane.TrafficId;
            var leader = new RoadId(0x533UL, 7UL);
            var pushed = new RoadId(0x533UL, 8UL);
            // Caisse longue poussee hors chaussee, en travers (patron 5.32) : reference a 2,8 m de l'enveloppe, cote droit
            // sans autre voie, avant sur le chemin. Non localisee, encore dans le couloir balaye.
            var at = lane.Track.Pieces[0].Nominal(7f);
            var right = Vector3.Cross(at.Up, at.Forward).normalized;
            var crosswise = new TrafficActorInput(pushed, new VehicleFootprintPose { Position = at.Position + right * 4.8f, Forward = -right,
                Up = at.Up, Footprint = new VehicleFootprint { FrontMeters = 5f, RearMeters = 1f, LeftMeters = 1.03f, RightMeters = 1.03f } },
                1.5f, RoadId.None);
            var inputs = new[] { ActorAt(agent, lane, 1f, 0f), ActorAt(leader, lane, 20f, 0f), crosswise };
            var frame = new TrafficFrame(5, Admission.Model, inputs);
            TrafficActor pushedActor;
            Assert.That(frame.TryGetActor(pushed, out pushedActor), Is.True);
            Assert.That(pushedActor.Location.Localized, Is.False, "vehicule pousse non localise");
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, agent, lane.Route, lane.Route.ExitPortalId, lane.Insertion.Seed,
                TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null, null, Admission.Evidence));
            var observation = TrafficPerception.Observe(frame, agent, decision.Path, TrafficV2Settings.PerceptionLimits,
                new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity));
            var leaders = observation.Leader.Items.Select(f => f.TrafficId).ToList();
            var obstacles = observation.Obstacles.Items.Select(f => f.Id).ToList();
            Assert.That(leaders, Is.EqualTo(new[] { leader }));
            Assert.That(obstacles.Count(id => id == pushed), Is.EqualTo(1), "obstacle TrafficActor unique");
            Assert.That(observation.Obstacles.Items.Single(f => f.Id == pushed).Kind, Is.EqualTo(PerceivedObstacleKind.TrafficActor));
            Assert.That(leaders.Intersect(obstacles), Is.Empty, "aucun id a la fois leader et obstacle");
            // Lui : aucune route depuis une pose non localisee, donc le repli 5.31/5.52.
            var own = PlanningSpine.Evaluate(new PlanningRequest(frame, pushed, null, RoadId.None, new RouteSeed(0),
                TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null, null, Admission.Evidence));
            Assert.That(own.Motion, Is.Null);
            // Un danger qui reprend l'id d'un acteur n'entre jamais dans la frame.
            Assert.Throws<ArgumentException>(() => new TrafficFrame(6, Admission.Model, inputs, new[] { new TrafficHazardInput(leader,
                TrafficHazardKind.Vehicle, new RoadBoundsBox { Center = Vector3.zero, Extents = Vector3.one }, Vector3.zero, 1f) }));
        }

        [Test]
        public void ASaturatedCollectorQueryPropagatesToPerceptionUnavailableForItsVehicleOnly()
        {
            var entry = Admission.Model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).First();
            var lane = LaneOf(TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, RoadId.None, 0UL, 1UL, Driver, Dt));
            var frame = new TrafficFrame(3, Admission.Model, new[] { ActorAt(lane.TrafficId, lane, 2f, 0f) });
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, lane.TrafficId, lane.Route, lane.Route.ExitPortalId,
                lane.Insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null, null,
                Admission.Evidence));
            var observation = TrafficPerception.Observe(frame, lane.TrafficId, decision.Path, TrafficV2Settings.PerceptionLimits,
                new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity));
            float front = LongitudinalPerception.FrontDistanceMeters(frame, lane.TrafficId, decision.Path);
            var plan = SpeedPlan.Build(decision.Motion, Admission.Model, Driver, 0f);
            var saturated = Decide(plan, Driver, 0f, Dt,
                LongitudinalPerception.From(observation, decision.Path, front, true), LongitudinalMemory.None);
            Assert.That(saturated.PerceptionReason, Is.EqualTo(PerceptionUnavailableReason.HazardCollectorSaturated));
            Assert.That(saturated.Binding.Kind, Is.EqualTo(LongitudinalCandidateKind.PerceptionUnavailable));
            Assert.That(saturated.AppliedAccelerationMetersPerSecondSquared, Is.EqualTo(0f), "pas d'acceleration");
            var complete = Decide(plan, Driver, 0f, Dt,
                LongitudinalPerception.From(observation, decision.Path, front, false), LongitudinalMemory.None);
            Assert.That(complete.PerceptionReason, Is.EqualTo(PerceptionUnavailableReason.None));
            Assert.That(complete.AppliedAccelerationMetersPerSecondSquared, Is.GreaterThan(0f));
        }

        // ================================================================== jeton de scenario

        [Test]
        public void TheScenarioTokenIsBuiltOnlyUnderTestsAndGrantsNoPermission()
        {
            var offenders = Directory.GetFiles("Assets/RoadRage", "*.cs", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .Where(p => !p.StartsWith("Assets/RoadRage/Tests/", StringComparison.Ordinal))
                .Where(p => Regex.IsMatch(Regex.Replace(Regex.Replace(File.ReadAllText(p), @"/\*.*?\*/", "", RegexOptions.Singleline),
                    @"//[^\n]*", ""), @"new\s+TrafficV2Scenario\s*\("))
                .ToList();
            Assert.That(offenders, Is.Empty, "le jeton de scenario n'est construit que sous Tests/");
            Assert.That(typeof(TrafficV2Scenario).GetConstructors().Length, Is.EqualTo(1));
            var entry = new RoadId(1, 1);
            var exit = new RoadId(1, 2);
            var one = new[] { new ScenarioInsertion(entry, exit, 0, 0) };
            Assert.Throws<ArgumentException>(() => new TrafficV2Scenario("zero", 0, one));
            Assert.Throws<ArgumentException>(() => new TrafficV2Scenario("trop", TrafficV2Settings.ScenarioMaxPopulation + 1, one));
            Assert.Throws<ArgumentException>(() => new TrafficV2Scenario("vide", 2, new ScenarioInsertion[0]));
            Assert.Throws<ArgumentException>(() => new TrafficV2Scenario("sans sortie", 2, new[] { new ScenarioInsertion(entry, RoadId.None, 0, 0) }));
            Assert.That(new TrafficV2Scenario("huit", TrafficV2Settings.ScenarioMaxPopulation, one).MaxPopulation, Is.EqualTo(8));
            Assert.That(TrafficV2Settings.ScenarioMaxPopulation, Is.EqualTo(8));
            Assert.That(TrafficV2Settings.V2SliceMaxPopulation, Is.EqualTo(1), "sans scenario, la population reste 1");
            // Aucune permission : un scenario n'est pas une mesure ; la verification d'insertion reste celle de la production.
            var verdict = TrafficV2Lifecycle.EvaluateInsertion(Admission, null, TrafficV2Settings.DeclaredTrackingTolerance);
            Assert.That(verdict.Allowed, Is.True, "couverture Covered exigee et etablie par la preuve signee");
            Assert.That(verdict.MeasurementLabel, Is.Null, "hors mesure : repli 2a actif");
            string spawner = File.ReadAllText("Assets/RoadRage/App/Run/PortalTrafficSpawner.cs");
            Assert.That(spawner, Does.Contain("EvaluateInsertion(v2Admission, measurement, TrafficV2Settings.DeclaredTrackingTolerance)"),
                "le scenario ne change pas le verdict d'insertion");
        }

        [Test]
        public void AScenarioWithAMeasurementRunInsertsNothingWithANamedRefusal()
        {
            var host = new GameObject("Story533Spawner");
            try
            {
                var spawner = host.AddComponent<PortalTrafficSpawner>();
                var run = new MeasurementRun(MeasurementKind.Exploratory, "533", new[] { new CampaignTriplet(new RoadId(1, 1), new RoadId(1, 2), 0) });
                var scenario = new TrafficV2Scenario("533", 2, new[] { new ScenarioInsertion(new RoadId(1, 1), new RoadId(1, 2), 0, 0) });
                TrafficV2Session.Request(TrafficComposition.V2Slice, run, scenario);
                Assert.That(TrafficV2Session.Scenario, Is.SameAs(scenario));
                typeof(PortalTrafficSpawner).GetMethod("ResolveCompositionOnce", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(spawner, null);
                LogAssert.Expect(LogType.Warning, new Regex("scenario de test et run de mesure demandes ensemble"));
                typeof(PortalTrafficSpawner).GetMethod("TickV2Slice", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(spawner, null);
                Assert.That(spawner.V2LastCode, Is.EqualTo(TrafficV2Code.ScenarioWithMeasurement));
                Assert.That(spawner.V2Insertions, Is.Zero);
                TrafficV2Session.Reset();
                Assert.That(TrafficV2Session.Scenario, Is.Null, "Reset efface le scenario");
            }
            finally
            {
                TrafficV2Session.Reset();
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        // ================================================================== routes de reference 5.31

        [Serializable]
        private sealed class CampaignTripletRecord
        {
            public int Index;
            public string Entry;
            public string Exit;
            public string Via;
            public ulong Seed;
            public ulong InsertionCounter;
        }

        [Serializable]
        private sealed class CampaignFile
        {
            public string RoadModelVersion;
            public CampaignTripletRecord[] Triplets;
        }

        private static List<Lane> CampaignLanes()
        {
            var campaign = JsonUtility.FromJson<CampaignFile>(File.ReadAllText(CampaignPath));
            Assert.That(campaign.Triplets.Length, Is.EqualTo(11));
            var lanes = new List<Lane>();
            foreach (var triplet in campaign.Triplets)
            {
                var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, RoadId.Parse(triplet.Entry), RoadId.Parse(triplet.Exit),
                    triplet.Seed, triplet.InsertionCounter, Driver, Dt, string.IsNullOrEmpty(triplet.Via) ? RoadId.None : RoadId.Parse(triplet.Via));
                Assert.That(insertion.Code, Is.EqualTo(TrafficV2Code.Allowed), "triplet " + triplet.Index);
                lanes.Add(LaneOf(insertion));
            }
            return lanes;
        }

        [Test]
        public void FootprintChannelsAreAvailableAlongTheElevenReferenceRoutesAndPerceptionUnavailableNeverBindsOnAFreeRoad()
        {
            int evaluated = 0, outsideWidth = 0;
            float worstMargin = float.PositiveInfinity;
            var buffer = new SpatialQueryBuffer(TrafficV2Settings.SpatialQueryCapacity);
            foreach (var lane in CampaignLanes())
            {
                var route = lane.Route;
                var via = lane.Insertion.ViaMovementId;
                // Poses tous les 1,5 m et au milieu de chaque morceau : la progression n'avance que d'une occurrence contigue.
                var distances = new SortedSet<float>();
                for (float distance = 0f; distance < lane.Track.LengthMeters - 0.25f; distance += 1.5f) distances.Add(distance);
                foreach (var piece in lane.Track.Pieces)
                    distances.Add(0.5f * (piece.StartDistanceMeters + piece.EndDistanceMeters));
                foreach (float distance in distances)
                {
                    var input = ActorAt(lane.TrafficId, lane, distance, 4f);
                    var frame = new TrafficFrame(1, Admission.Model, new[] { input });
                    int piece = lane.Track.PieceAt(distance);
                    // Objectif intermediaire garde jusqu'au franchissement de son occurrence (comme le driver).
                    if (!via.IsEmpty && route.ViaOccurrenceIndex >= 0 && route.ProgressOccurrenceIndex > route.ViaOccurrenceIndex)
                        via = RoadId.None;
                    var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, lane.TrafficId, route, route.ExitPortalId,
                        lane.Insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, Driver, TrackingTolerance.Undeclared, null,
                        null, Admission.Evidence, via, lane.Track.OffsetRadians(piece, distance)));
                    Assert.That(decision.Path, Is.Not.Null, "route " + lane.TrafficId + " d = " + distance + " : " + decision.Projection.Code);
                    if (decision.Route.Plan != null) route = decision.Route.Plan;
                    var observation = TrafficPerception.Observe(frame, lane.TrafficId, decision.Path, TrafficV2Settings.PerceptionLimits, buffer);
                    var perception = LongitudinalPerception.From(observation, decision.Path,
                        LongitudinalPerception.FrontDistanceMeters(frame, lane.TrafficId, decision.Path), false);
                    Assert.That(perception.UnavailableReason, Is.EqualTo(PerceptionUnavailableReason.None),
                        "canaux a emprise indisponibles en route libre : " + observation.Leader.Status + " a d = " + distance
                        + " sur " + frame.Actors[0].Location.ElementId);
                    if (decision.Motion != null)
                    {
                        var plan = SpeedPlan.Build(decision.Motion, Admission.Model, Driver, 4f);
                        if (plan.Accepted)
                            Assert.That(Decide(plan, Driver, 4f, Dt, perception, LongitudinalMemory.None).Binding.Kind,
                                Is.Not.EqualTo(LongitudinalCandidateKind.PerceptionUnavailable));
                    }
                    var location = frame.Actors[0].Location;
                    if (location.OutsideWidthEnvelope) outsideWidth++;
                    worstMargin = Math.Min(worstMargin, WidthMargin(input.Pose, location));
                    evaluated++;
                }
            }
            Assert.That(evaluated, Is.GreaterThan(500));
            // Constat D4 : controle aux quatre coins sur la pose nominale (le jalon 5.52 le juge sur la pose reelle).
            Debug.Log("[Story533] canaux a emprise : " + evaluated + " poses nominales evaluees, OutsideWidthEnvelope aux coins "
                + outsideWidth + ", marge laterale minimale des coins " + worstMargin.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + " m.");
        }

        /// <summary>Plus petite marge laterale des coins de l'empreinte a l'enveloppe, dans le repere de l'element retenu.</summary>
        private static float WidthMargin(VehicleFootprintPose pose, RoadLocation location)
        {
            RoadElementKind kind;
            RoadCurve curve;
            IReadOnlyList<RoadCurveSample> samples;
            if (!location.Localized || !TrafficFrame.TryGetElement(Admission.Model, location.ElementId, out kind, out curve, out samples))
                return float.NaN;
            var at = curve.Sample(location.SMeters);
            float margin = float.PositiveInfinity;
            foreach (var corner in TrafficFrame.Corners(pose))
            {
                float lateral = Vector3.Dot(corner - at.Position, at.Right);
                margin = Math.Min(margin, Math.Min(at.HalfWidthRightMeters - lateral, at.HalfWidthLeftMeters + lateral));
            }
            return margin;
        }

        [Test]
        public void TheStaticDecorOfMvpRunLeavesRoomInTheCollectorBufferForAFullScenario()
        {
            var lanes = CampaignLanes();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(V2PrefabPath);
            int collidersPerVehicle = prefab.GetComponentsInChildren<Collider>(true).Count(c => !c.isTrigger);
            var scene = EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
            int max = 0, samples = 0, saturated = 0, trueMax = 0, staticMax = 0;
            Vector3 worst = Vector3.zero;
            var trueCounts = new List<int>();
            var smaller = new[] { 25f, 30f, 35f }.Select(r => new TrafficV2HazardCollector(4096, r)).ToArray();
            try
            {
                Physics.SyncTransforms();
                var collector = new TrafficV2HazardCollector(TrafficV2Settings.HazardQueryCapacity, TrafficV2Settings.HazardQueryRadiusMeters);
                // Mesure seulement : tampon non borne pour connaitre le vrai nombre de colliders de la requete.
                var unbounded = new TrafficV2HazardCollector(4096, TrafficV2Settings.HazardQueryRadiusMeters);
                var id = new RoadId(0x533UL, 1UL);
                foreach (var lane in lanes)
                    for (float distance = 0f; distance < lane.Track.LengthMeters; distance += 2f)
                    {
                        int piece = lane.Track.PieceAt(distance);
                        var nominal = lane.Track.Pieces[piece].Nominal(distance);
                        var center = nominal.Position + nominal.Up * 0.5f;
                        collector.Collect(new[] { new HazardQuery(id, center, null) }, null);
                        int hits = collector.Reports[0].Hits;
                        if (collector.Reports[0].Saturated) saturated++;
                        if (hits > max) { max = hits; worst = nominal.Position; }
                        unbounded.Collect(new[] { new HazardQuery(id, center, null) }, null);
                        trueCounts.Add(unbounded.Reports[0].Hits);
                        if (unbounded.Reports[0].Hits > trueMax) { trueMax = unbounded.Reports[0].Hits; worst = nominal.Position; }
                        staticMax = Math.Max(staticMax, unbounded.Counters.ExcludedStatic);
                        foreach (var probe in smaller) probe.Collect(new[] { new HazardQuery(id, center, null) }, null);
                        samples++;
                    }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
            trueCounts.Sort();
            int room = TrafficV2Settings.HazardQueryCapacity - 1 - max;
            Debug.Log("[Story533] collecteur sur le decor de MVP_Run : " + samples + " requetes le long des 11 routes 5.31 (pas 2 m), "
                + "capacite " + TrafficV2Settings.HazardQueryCapacity + ", rayon " + TrafficV2Settings.HazardQueryRadiusMeters + " m : "
                + saturated + " requetes saturees ; tampon non borne : maximum " + trueMax + " colliders (dont " + staticMax
                + " statiques) a " + worst + ", mediane " + trueCounts[trueCounts.Count / 2] + ", p90 " + trueCounts[trueCounts.Count * 9 / 10]
                + " ; " + collidersPerVehicle + " collider(s) par vehicule V2, place restante " + room + " ; maximum non borne a "
                + string.Join(", ", smaller.Select(c => c.RadiusMeters + " m = " + c.MaxHits).ToArray()) + ".");
            Assert.That(max, Is.GreaterThan(0), "la requete EditMode voit bien le decor");
            Assert.That(max + TrafficV2Settings.ScenarioMaxPopulation * collidersPerVehicle, Is.LessThan(TrafficV2Settings.HazardQueryCapacity),
                "une requete pleine serait saturee : un vehicule seul ne pourrait plus accelerer (HALT, capacite revisee par le proprietaire)");
        }

        // ================================================================== constructeur de scenarios

        [Serializable]
        private sealed class VectorRecord
        {
            public float x, y, z;
            public static VectorRecord Of(Vector3 v) { return new VectorRecord { x = v.x, y = v.y, z = v.z }; }
        }

        [Serializable]
        private sealed class InsertionRecord
        {
            public string Entry;
            public string Exit;
            public ulong Seed;
            public ulong EarliestStep;
            public string[] Elements;
        }

        [Serializable]
        private sealed class ObstacleRecord
        {
            public string Label;
            public string ElementId;
            public float RouteDistanceMeters;
            public VectorRecord Center;
            public VectorRecord Forward;
            public VectorRecord Size;
            public int FromStep;
            /// <summary>-1 : retire par le test quand la file est stable.</summary>
            public int UntilStep;
        }

        [Serializable]
        private sealed class PushRecord
        {
            public int Insertion;
            public int AtStep;
            public float LateralImpulsePerKilogram;
        }

        [Serializable]
        private sealed class ScenarioRecord
        {
            public string Label;
            public int MaxPopulation;
            public int MaxSteps;
            public InsertionRecord[] Insertions;
            public ObstacleRecord[] Obstacles;
            public PushRecord[] Pushes;
        }

        [Serializable]
        private sealed class ScenarioFile
        {
            public int Format;
            public string RoadModelVersion;
            public float VehicleLengthMeters;
            public float MinimumGapMeters;
            public float MinimumObstacleDistanceMeters;
            public VectorRecord Parking;
            public ScenarioRecord[] Scenarios;
        }

        private static InsertionRecord Insertion(Portal entry, Portal exit, ulong counter, ulong earliest, List<RoutePlan> routes)
        {
            var prepared = TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, exit.Id, 0UL, counter, Driver, Dt);
            if (prepared.Code != TrafficV2Code.Allowed) return null;
            routes.Add(prepared.Route);
            return new InsertionRecord { Entry = entry.Id.ToString(), Exit = exit.Id.ToString(), Seed = 0UL, EarliestStep = earliest,
                Elements = prepared.Route.Occurrences.Select(o => o.Id.ToString()).ToArray() };
        }

        private static bool IsStraight(TrackPiece piece)
        {
            EffectiveLaneCorridor corridor;
            if (piece.Kind != RoadElementKind.LaneCorridor || !Admission.Model.TryGetCorridor(piece.Id, out corridor)) return false;
            return corridor.Samples.All(s => Math.Abs(s.CurvaturePerMeter) < 1e-4f);
        }

        /// <summary>Obstacle sur un corridor droit de la route, face proche a au moins <paramref name="minimum"/> du depart.</summary>
        private static ObstacleRecord Obstacle(ReferenceTrack track, float minimum, string label, int from, int until)
        {
            foreach (var piece in track.Pieces)
            {
                if (!IsStraight(piece)) continue;
                float near = Math.Max(minimum, piece.StartDistanceMeters + 0.5f);
                if (near + ObstacleLengthMeters + 0.5f > piece.EndDistanceMeters) continue;
                var nominal = piece.Nominal(near + ObstacleLengthMeters * 0.5f);
                return new ObstacleRecord { Label = label, ElementId = piece.Id.ToString(), RouteDistanceMeters = near,
                    Center = VectorRecord.Of(nominal.Position + nominal.Up * (ObstacleHeightMeters * 0.5f)), Forward = VectorRecord.Of(nominal.Forward),
                    Size = VectorRecord.Of(new Vector3(ObstacleWidthMeters, ObstacleHeightMeters, ObstacleLengthMeters)), FromStep = from, UntilStep = until };
            }
            return null;
        }

        private static string BuildScenarioText()
        {
            var model = Admission.Model;
            var car = Car;
            float length = car.FrontMeters + car.RearMeters;
            float minimumDistance = 3f * (length + Driver.MinimumGap) + 10f;
            var entries = model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).ToList();
            var exits = model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id).ToList();

            // A et B : premiere paire (entree, sortie) dont la route, identique pour chaque identite d'insertion, porte un
            // corridor droit ou placer l'obstacle a la distance minimale.
            ReferenceTrack trackA = null;
            ObstacleRecord obstacleA = null;
            List<InsertionRecord> insertionsA = null;
            foreach (var entry in entries)
            {
                foreach (var exit in exits)
                {
                    var routes = new List<RoutePlan>();
                    var records = new List<InsertionRecord>();
                    for (ulong counter = 1; counter <= 4; counter++) records.Add(Insertion(entry, exit, counter, 0UL, routes));
                    if (records.Any(r => r == null)) continue;
                    if (routes.Any(r => !r.Occurrences.Select(o => o.Id).SequenceEqual(routes[0].Occurrences.Select(o => o.Id)))) continue;
                    var track = ReferenceTrack.FromRoute(model, routes[0].Occurrences, 0f);
                    var obstacle = Obstacle(track, minimumDistance, "A", 0, -1);
                    if (obstacle == null) continue;
                    trackA = track; obstacleA = obstacle; insertionsA = records;
                    break;
                }
                if (obstacleA != null) break;
            }
            Assert.That(obstacleA, Is.Not.Null, "aucune route ne porte un corridor droit a " + minimumDistance + " m du portail d'entree");

            var scenarios = new List<ScenarioRecord>
            {
                new ScenarioRecord { Label = "A", MaxPopulation = 3, MaxSteps = 9000, Insertions = insertionsA.Take(3).ToArray(),
                    Obstacles = new[] { obstacleA }, Pushes = new PushRecord[0] },
                new ScenarioRecord { Label = "B", MaxPopulation = 2, MaxSteps = 9000, Insertions = new[] { insertionsA[0], new InsertionRecord {
                    Entry = insertionsA[1].Entry, Exit = insertionsA[1].Exit, Seed = 0UL, EarliestStep = 150UL, Elements = insertionsA[1].Elements } },
                    Obstacles = new ObstacleRecord[0], Pushes = new PushRecord[0] }
            };

            // Campagnes nominales 2/4/8, sans aucune poussee (politique pre-5.39) : suivi (2) ; file, resorption et vehicule
            // arrete devant (4) ; quatre entrees, carrefour en croix, giratoires et sortie occupee (8).
            scenarios.Add(new ScenarioRecord { Label = "explore-2", MaxPopulation = 2, MaxSteps = 6000, Insertions = insertionsA.Take(2).ToArray(),
                Obstacles = new ObstacleRecord[0], Pushes = new PushRecord[0] });
            var queue = Obstacle(trackA, minimumDistance, "file", 0, 1800);
            scenarios.Add(new ScenarioRecord { Label = "explore-4", MaxPopulation = 4, MaxSteps = 6000, Insertions = insertionsA.ToArray(),
                Obstacles = new[] { queue }, Pushes = new PushRecord[0] });
            // Deux insertions par entree, vers deux sorties distinctes atteignables (rotation par entree), en deux vagues ;
            // le calendrier est ordonne par pas d'insertion (le spawner sert les insertions dans l'ordre).
            var pairs = new List<KeyValuePair<Portal, Portal>>[] { new List<KeyValuePair<Portal, Portal>>(), new List<KeyValuePair<Portal, Portal>>() };
            for (int e = 0; e < entries.Count; e++)
            {
                int wave = 0;
                for (int k = 0; k < exits.Count && wave < 2; k++)
                {
                    var exit = exits[(e + 2 + k) % exits.Count];
                    if (TrafficV2Lifecycle.PrepareInsertion(Admission, entries[e].Id, exit.Id, 0UL, 1UL, Driver, Dt).Code != TrafficV2Code.Allowed)
                        continue;
                    pairs[wave++].Add(new KeyValuePair<Portal, Portal>(entries[e], exit));
                }
                Assert.That(wave, Is.EqualTo(2), "deux sorties atteignables depuis l'entree " + entries[e].Id);
            }
            var eight = new List<InsertionRecord>();
            var eightRoutes = new List<RoutePlan>();
            ulong slot = 1;
            for (int wave = 0; wave < 2; wave++)
                foreach (var pair in pairs[wave])
                {
                    var record = Insertion(pair.Key, pair.Value, slot++, wave == 0 ? 0UL : 300UL, eightRoutes);
                    Assert.That(record, Is.Not.Null);
                    eight.Add(record);
                }
            // Sortie occupee : obstacle temporaire sur le premier corridor droit apres le premier mouvement de la premiere route.
            var exitTrack = ReferenceTrack.FromRoute(model, eightRoutes[0].Occurrences, 0f);
            int firstMovement = exitTrack.Pieces.ToList().FindIndex(p => p.Kind == RoadElementKind.JunctionMovement);
            var occupied = Obstacle(exitTrack, exitTrack.Pieces[firstMovement].EndDistanceMeters, "sortie", 0, 1500);
            scenarios.Add(new ScenarioRecord { Label = "explore-8", MaxPopulation = 8, MaxSteps = 7500, Insertions = eight.ToArray(),
                Obstacles = occupied == null ? new ObstacleRecord[0] : new[] { occupied }, Pushes = new PushRecord[0] });
            // Exploratoire perturbe, hors campagne nominale : le leader de deux vehicules est pousse lateralement ; le premier
            // TrackingToleranceExceeded y est un constat attendu (limitation pre-5.39), pas un echec.
            scenarios.Add(new ScenarioRecord { Label = "explore-poussee", MaxPopulation = 2, MaxSteps = 6000, Insertions = insertionsA.Take(2).ToArray(),
                Obstacles = new ObstacleRecord[0], Pushes = new[] { new PushRecord { Insertion = 0, AtStep = 700, LateralImpulsePerKilogram = 6f } } });

            // Pose de stationnement declaree du joueur hote : plateforme creee par le test loin de toute route du district.
            float maxX = float.NegativeInfinity, minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
            foreach (var corridor in model.Corridors)
                foreach (var sample in corridor.Samples)
                { maxX = Math.Max(maxX, sample.Position.x); minZ = Math.Min(minZ, sample.Position.z); maxZ = Math.Max(maxZ, sample.Position.z); }
            var parking = new Vector3(maxX + 150f, 0f, 0.5f * (minZ + maxZ));

            var file = new ScenarioFile { Format = 1, RoadModelVersion = model.Version.ToString(), VehicleLengthMeters = length,
                MinimumGapMeters = Driver.MinimumGap, MinimumObstacleDistanceMeters = minimumDistance, Parking = VectorRecord.Of(parking),
                Scenarios = scenarios.ToArray() };
            return JsonUtility.ToJson(file, true);
        }

        [Test]
        public void TheScenarioBuilderIsDeterministicAndWritesTheScenarioFile()
        {
            string first = BuildScenarioText();
            string second = BuildScenarioText();
            Assert.That(second, Is.EqualTo(first), "memes entrees, memes scenarios");
            var file = JsonUtility.FromJson<ScenarioFile>(first);
            Assert.That(file.Scenarios.Select(s => s.Label), Is.EqualTo(new[] { "A", "B", "explore-2", "explore-4", "explore-8", "explore-poussee" }));
            Assert.That(file.Scenarios.Where(s => s.Label != "explore-poussee").All(s => s.Pushes.Length == 0), Is.True,
                "scenarios nominaux : aucune poussee volontaire");
            Assert.That(file.Scenarios.Last().Pushes.Length, Is.EqualTo(1), "la poussee vit dans son scenario exploratoire");
            var a = file.Scenarios[0];
            Assert.That(a.Insertions.Length, Is.EqualTo(3));
            Assert.That(a.Insertions.Select(i => string.Join(",", i.Elements)).Distinct().Count(), Is.EqualTo(1), "meme route");
            Assert.That(a.Obstacles.Single().RouteDistanceMeters, Is.GreaterThanOrEqualTo(file.MinimumObstacleDistanceMeters));
            Assert.That(file.MinimumObstacleDistanceMeters, Is.EqualTo(3f * (4.44f + 2f) + 10f).Within(1e-3f));
            Assert.That(file.Scenarios[1].Insertions.Select(i => i.EarliestStep), Is.EqualTo(new[] { 0UL, 150UL }), "insertion decalee");
            Assert.That(file.Scenarios.Select(s => s.MaxPopulation), Is.EqualTo(new[] { 3, 2, 2, 4, 8, 2 }));
            Assert.That(file.Scenarios.All(s => s.Insertions.Length <= TrafficV2Settings.ScenarioMaxPopulation), Is.True);
            Directory.CreateDirectory(ScenarioFolder);
            File.WriteAllText(ScenarioPath, first);
            Debug.Log("[Story533] scenarios ecrits : " + ScenarioPath + " (obstacle A a " + a.Obstacles[0].RouteDistanceMeters + " m sur "
                + a.Obstacles[0].ElementId + ").");
        }
    }
}
