using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Migration;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.31 (Geometry) : rejeu cinematique de toute la chaine V2 sur MVP_Run (frame, epine,
    /// plan de vitesse, commande de suivi) avec les limites de braquage reelles du profil vehicule, et
    /// constructeur deterministe de la campagne de mesure (budget de graines declare, incidence par
    /// element, NotSelectable explicite). Le rejeu ne simule pas les pneus : la mesure physique
    /// appartient aux campagnes PlayMode.
    /// </summary>
    [Category("Geometry")]
    [Category("Story531")]
    public sealed class Story531DrivenReplayTests
    {
        /// <summary>
        /// Budget de graines declare du constructeur (parametre de conception). Depuis le correct-course du
        /// 2026-09-29, la selectionnabilite ne depend plus de la graine : la couverture est dirigee par l'objectif
        /// de mouvement intermediaire (contrat §4), et une seule graine est utilisee.
        /// </summary>
        public const int CampaignSeedBudget = 1;

        public const string MeasurementsFolder = "_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements";
        public const string CampaignPath = MeasurementsFolder + "/campaign-5-31.json";
        private const string ContactProofPath = "_bmad-output/implementation-artifacts/v1-regression-5-51/final-proof.txt";
        private const float Dt = 0.02f;

        [Serializable]
        public sealed class CampaignTripletRecord
        {
            public int Index;
            public string Entry;
            public string Exit;
            /// <summary>Mouvement vise (objectif intermediaire) ; vide : route ordinaire.</summary>
            public string Via;
            public ulong Seed;
            public ulong InsertionCounter;
            public string TrafficId;
            public string[] Elements;
        }

        [Serializable]
        public sealed class IncidenceRecord
        {
            public string Element;
            public int[] Triplets;
        }

        [Serializable]
        public sealed class CampaignFile
        {
            // 2 : triplets a objectif de mouvement intermediaire (correct-course du 2026-09-29).
            public int Format = 2;
            public string RoadModelVersion;
            public int SeedBudget;
            public string Status;
            public int RequiredElements;
            public CampaignTripletRecord[] Triplets;
            public IncidenceRecord[] Incidence;
            public string[] NotSelectable;
            public string ContactPhysicalFingerprint;
            public string SceneDependencyHash;
            public string SceneFileSha256;
        }

        private static TrafficV2Admission admission;
        private static CampaignFile campaign;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                        File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
                return admission;
            }
        }

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>("Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset").Profile; }
        }

        private static VehicleProfile Vehicle
        {
            get { return AssetDatabase.LoadAssetAtPath<VehicleProfileDef>("Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset").Profile; }
        }

        /// <summary>Elements a couvrir : 72 mouvements, 44 corridors, 24 raccords d'anneau (m:/c:/s:).</summary>
        private static List<string> RequiredElements(CompiledRoadModel model, List<string> ring)
        {
            var required = new List<string>();
            required.AddRange(model.Movements.Select(m => "m:" + m.Id));
            required.AddRange(model.Corridors.Select(c => "c:" + c.CorridorId));
            required.AddRange(ring.Select(s => "s:" + s));
            required.Sort(StringComparer.Ordinal);
            return required;
        }

        private static HashSet<string> ElementsOf(RoutePlan plan, HashSet<string> ring)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var occurrence in plan.Occurrences)
            {
                if (occurrence.Kind == RoadElementKind.LaneCorridor) { set.Add("c:" + occurrence.Id); continue; }
                set.Add("m:" + occurrence.Id);
                if (ring.Contains(occurrence.Id + ":entry")) set.Add("s:" + occurrence.Id + ":entry");
                if (ring.Contains(occurrence.Id + ":exit")) set.Add("s:" + occurrence.Id + ":exit");
            }
            return set;
        }

        private sealed class Candidate
        {
            public Portal Entry;
            public CompiledJunctionMovement Via;
            public Portal Exit;
            public HashSet<string> Elements;
        }

        /// <summary>
        /// Constructeur deterministe a couverture dirigee (contrat §4, correct-course du 2026-09-29) :
        /// 1. chaque (entree, mouvement vise, sortie) est planifie par RoutePlanner avec l'objectif intermediaire
        ///    (graine 0, domaine "route", frame 0 : memes entrees que la premiere decision de l'insertion) ;
        /// 2. recouvrement glouton : a chaque tour, le candidat qui ajoute le plus d'elements non couverts
        ///    (egalite departagee par l'ordre entree, mouvement, sortie) ;
        /// 3. la route consignee est replanifiee avec l'identite trafic reelle de l'insertion (compteur = tour + 1),
        ///    car la preference en depend : c'est elle, et non l'estimation, qui compte pour la couverture.
        /// Un element n'est NotSelectable que si aucun candidat, sur toutes les combinaisons, ne le traverse.
        /// </summary>
        private static CampaignFile BuildCampaign()
        {
            if (campaign != null) return campaign;
            var model = Admission.Model;
            var ringKeys = CampaignTraceability.RingSeamKeys(model);
            var ring = new HashSet<string>(ringKeys, StringComparer.Ordinal);
            var required = RequiredElements(model, ringKeys);
            var entries = model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).ToList();
            var exits = model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id).ToList();
            var movements = model.Movements.OrderBy(m => m.Id).ToList();
            var locations = new Dictionary<RoadId, RoadLocation>();
            foreach (var entry in entries)
            {
                Portal portal; Vector3 position; Quaternion rotation;
                Assert.That(TrafficV2Lifecycle.TryPortalPose(model, entry.Id, out portal, out position, out rotation), Is.True);
                locations[entry.Id] = RoadLocalizer.Localize(model, new VehicleFootprintPose { Position = position,
                    Forward = rotation * Vector3.forward, Up = rotation * Vector3.up }, entry.CorridorId, null);
            }
            const ulong seed = 0UL;
            var estimateId = TrafficV2Lifecycle.TrafficIdentity(seed, 1UL);
            var candidates = new List<Candidate>();
            var reachable = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in entries)
                foreach (var movement in movements)
                    foreach (var exit in exits)
                    {
                        var plan = RoutePlanner.Plan(new RouteRequest(model, locations[entry.Id], exit.Id, new RouteSeed(seed),
                            estimateId, "route", new DecisionCounter(0), null, false, null, movement.Id)).Plan;
                        if (plan == null) continue;
                        var set = ElementsOf(plan, ring);
                        reachable.UnionWith(set);
                        candidates.Add(new Candidate { Entry = entry, Via = movement, Exit = exit, Elements = set });
                    }

            var covered = new HashSet<string>(StringComparer.Ordinal);
            var triplets = new List<CampaignTripletRecord>();
            for (int round = 0; required.Any(e => reachable.Contains(e) && !covered.Contains(e)); round++)
            {
                Candidate best = null;
                int bestGain = 0;
                foreach (var candidate in candidates)
                {
                    int gain = candidate.Elements.Count(k => !covered.Contains(k));
                    if (gain > bestGain) { bestGain = gain; best = candidate; }
                }
                if (best == null) break;
                ulong counter = (ulong)(round + 1);
                var id = TrafficV2Lifecycle.TrafficIdentity(seed, counter);
                var actual = RoutePlanner.Plan(new RouteRequest(model, locations[best.Entry.Id], best.Exit.Id, new RouteSeed(seed),
                    id, "route", new DecisionCounter(0), null, false, null, best.Via.Id)).Plan;
                Assert.That(actual, Is.Not.Null, "le candidat retenu reste planifiable avec l'identite de l'insertion");
                var actualSet = ElementsOf(actual, ring);
                if (!actualSet.Any(k => !covered.Contains(k)))
                {
                    // L'identite reelle a change le choix de route sans rien ajouter : on ecarte ce candidat.
                    candidates.Remove(best);
                    round--;
                    continue;
                }
                triplets.Add(new CampaignTripletRecord { Index = triplets.Count, Entry = best.Entry.Id.ToString(),
                    Exit = best.Exit.Id.ToString(), Via = best.Via.Id.ToString(), Seed = seed, InsertionCounter = counter,
                    TrafficId = id.ToString(), Elements = actualSet.OrderBy(k => k, StringComparer.Ordinal).ToArray() });
                covered.UnionWith(actualSet);
            }
            var incidence = required.Select(element => new IncidenceRecord { Element = element,
                Triplets = triplets.Where(t => t.Elements.Contains(element)).Select(t => t.Index).ToArray() }).ToArray();
            var notSelectable = incidence.Where(i => i.Triplets.Length == 0).Select(i => i.Element).ToArray();
            campaign = new CampaignFile { RoadModelVersion = model.Version.ToString(), SeedBudget = CampaignSeedBudget,
                Status = notSelectable.Length == 0 ? "Accepted" : "Refused", RequiredElements = required.Count,
                Triplets = triplets.ToArray(), Incidence = incidence, NotSelectable = notSelectable };
            return campaign;
        }

        [Test]
        public void CampaignBuilderPublishesIncidenceAndNotSelectableWithinTheDeclaredSeedBudget()
        {
            var built = BuildCampaign();
            var model = Admission.Model;
            Assert.That(built.SeedBudget, Is.EqualTo(CampaignSeedBudget));
            Assert.That(built.RequiredElements, Is.EqualTo(72 + 44 + 24), "72 mouvements, 44 corridors, 24 raccords d'anneau");
            Assert.That(built.Incidence.Length, Is.EqualTo(built.RequiredElements));
            Assert.That(built.Incidence.Count(i => i.Element.StartsWith("s:", StringComparison.Ordinal)), Is.EqualTo(24));
            // Chaque element est soit porte par au moins un triplet, soit publie NotSelectable : jamais absent.
            foreach (var record in built.Incidence)
                Assert.That(record.Triplets.Length > 0 || built.NotSelectable.Contains(record.Element), Is.True, record.Element);
            Assert.That(built.Status, Is.EqualTo(built.NotSelectable.Length == 0 ? "Accepted" : "Refused"));

            // Chaque triplet retenu est une insertion conduisible, et sa premiere decision suit la route construite.
            var driver = Driver;
            foreach (var triplet in built.Triplets)
            {
                RoadId entry, exit, via;
                Assert.That(RoadId.TryParse(triplet.Entry, out entry), Is.True);
                Assert.That(RoadId.TryParse(triplet.Exit, out exit), Is.True);
                Assert.That(RoadId.TryParse(triplet.Via, out via), Is.True, "chaque triplet porte son mouvement vise");
                var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, entry, exit, triplet.Seed, triplet.InsertionCounter,
                    driver, Dt, via);
                Assert.That(insertion.Route.ViaMovementId, Is.EqualTo(via), "l'objectif atteint la premiere decision");
                Assert.That(insertion.Code, Is.EqualTo(TrafficV2Code.Allowed), "triplet " + triplet.Index);
                Assert.That(insertion.TrafficId.ToString(), Is.EqualTo(triplet.TrafficId));
                var ring = new HashSet<string>(CampaignTraceability.RingSeamKeys(model), StringComparer.Ordinal);
                Assert.That(ElementsOf(insertion.Route, ring).OrderBy(k => k, StringComparer.Ordinal), Is.EqualTo(triplet.Elements),
                    "la route de l'insertion est celle du constructeur (triplet " + triplet.Index + ")");
            }

            // La classification des contacts ne peut utiliser la liste de reliefs 5.51 que si son empreinte
            // physique correspond encore à la scène courante. Le SHA-256 du fichier lie ensuite
            // le run PlayMode au même état disque, sans API Editor pendant PlayMode.
            const string scenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
            var scene = SceneManager.GetSceneByPath(scenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                Assert.That(scene.isLoaded && !scene.isDirty, Is.True, "MVP_Run doit être chargé et propre");
                var migration = MigrationReport.Run(scene, File.ReadAllText(MigrationReport.LineageFullPath));
                Assert.That(migration.Import != null && migration.Import.Succeeded, Is.True, string.Join("\n", migration.Failures));
                var sidewalkFailures = new List<string>();
                var sidewalks = SidewalkDeclarations.Read(scene, sidewalkFailures);
                Assert.That(sidewalkFailures, Is.Empty);
                var clearance = JunctionClearance.Measure(scene, migration.Import, model, sidewalks);
                Assert.That(clearance.Passed, Is.True, string.Join("\n", clearance.Failures));
                string proofFingerprint = File.ReadAllLines(ContactProofPath)
                    .Where(line => line.StartsWith("empreinte-physique=", StringComparison.Ordinal))
                    .Select(line => line.Substring("empreinte-physique=".Length)).Single();
                Assert.That(clearance.PhysicalFingerprint, Is.EqualTo(proofFingerprint),
                    "la preuve 5.51 n'est plus liée aux entrées physiques de MVP_Run");
                built.ContactPhysicalFingerprint = clearance.PhysicalFingerprint;
                built.SceneDependencyHash = AssetDatabase.GetAssetDependencyHash(scene.path).ToString();
                using (var sha = SHA256.Create())
                    built.SceneFileSha256 = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(scenePath)))
                        .Replace("-", "").ToLowerInvariant();
            }
            finally
            {
                if (opened && scene.isLoaded && !scene.isDirty) EditorSceneManager.CloseScene(scene, true);
            }

            Directory.CreateDirectory(MeasurementsFolder);
            string json = JsonUtility.ToJson(built, true).Replace("\r\n", "\n") + "\n";
            File.WriteAllText(CampaignPath, json);
            Assert.That(File.Exists(CampaignPath), Is.True);
            TestContext.WriteLine("campagne : " + built.Triplets.Length + " triplet(s), " + built.NotSelectable.Length
                + " element(s) NotSelectable, statut " + built.Status);
        }

        /// <summary>
        /// La campagne n'est acceptable que si chaque element est selectionnable dans le budget. Sinon elle
        /// est refusee et la decision revient au proprietaire (Ask First) : ce test l'annonce, il ne l'arbitre pas.
        /// </summary>
        [Test]
        public void CampaignIsAcceptableOnlyWhenEveryElementIsSelectable()
        {
            var built = BuildCampaign();
            var model = Admission.Model;
            var labels = built.NotSelectable.Select(key =>
            {
                RoadId id;
                CompiledJunctionMovement movement;
                if (key.StartsWith("m:", StringComparison.Ordinal) && RoadId.TryParse(key.Substring(2), out id)
                    && model.TryGetMovement(id, out movement)) return key + " (" + movement.Label + ")";
                return key;
            }).ToArray();
            Assert.That(built.NotSelectable, Is.Empty, "Campagne refusee : " + built.NotSelectable.Length
                + " element(s) NotSelectable : aucune paire (entree, sortie) ne les atteint, meme avec l'objectif intermediaire."
                + " Decision proprietaire requise (Ask First). " + string.Join("; ", labels));
        }

        // ------------------------------------------------------------------ rejeu cinematique

        private sealed class ReplayResult
        {
            public bool Exited;
            public int Steps;
            public int Fallbacks;
            public string FirstFallback;
            public float MaxSpeedRatio;
            public int CeilingExceeded;
            public float MaxLateralError;
            public string MaxLateralAt;
            public int Replans;
            /// <summary>Vitesse minimale localise sur un mouvement de carrefour (rampe des giratoires avant le 2026-09-30).</summary>
            public float MinMovementSpeed = float.PositiveInfinity;
            public string MinMovementAt;
            /// <summary>Pas localises sur un mouvement avec 0 &lt; v &lt; vitesse minimale de direction (direction inactive).</summary>
            public int InactiveSteeringSteps;
        }

        /// <summary>
        /// Modele bicyclette cinematique au point de reference du profil declare : vitesse integree depuis
        /// l'acceleration commandee, angle de roue borne par le braquage disponible a la vitesse, nul sous la
        /// vitesse minimale de direction, deplace a taux borne (fonctions de VehicleSteeringModel).
        /// </summary>
        private static ReplayResult Replay(TrafficV2Insertion insertion, int maxSteps)
        {
            var model = Admission.Model;
            var driver = Driver;
            var vehicle = Vehicle;
            var drivability = model.DrivabilityProfile;
            var route = insertion.Route;
            Portal exit = model.Portals.First(p => p.Id == route.ExitPortalId);
            Vector3 forward = insertion.Rotation * Vector3.forward;
            float yaw = Mathf.Atan2(forward.x, forward.z);
            Vector3 rear = insertion.Position - forward * drivability.ReferencePointAheadRearAxleMeters;
            float speed = 0f, wheel = 0f;
            var previous = insertion.EntryPortal.CorridorId;
            var result = new ReplayResult();
            // Meme chaine que le driver V2 : objectif garde jusqu'au franchissement, cap vise = cap nominal de la route.
            var via = insertion.ViaMovementId;
            bool onVia = false;
            var track = ReferenceTrack.FromRoute(model, route.Occurrences);
            int pieceHint = 0;
            for (int step = 1; step <= maxSteps; step++)
            {
                result.Steps = step;
                forward = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw));
                Vector3 reference = rear + forward * drivability.ReferencePointAheadRearAxleMeters;
                var pose = new VehicleFootprintPose { Position = reference, Forward = forward, Up = Vector3.up };
                // Comme le driver : e courant sur la reference (plafond de l'horizon, ancres de localisation).
                int currentPiece;
                float currentDistance = track.Project(reference, pieceHint, out currentPiece);
                float offset = track.OffsetRadians(currentPiece, currentDistance);
                var frame = new TrafficFrame((ulong)step, model, new[] {
                    new TrafficActorInput(insertion.TrafficId, pose, speed, previous, TrafficV2Lifecycle.ExpectedElements(route),
                        track.KinematicAnchors(currentPiece)) });
                TrafficActor actor;
                frame.TryGetActor(insertion.TrafficId, out actor);
                if (actor.Location.Localized) previous = actor.Location.ElementId;
                if (!via.IsEmpty && actor.Location.Localized)
                {
                    if (actor.Location.ElementId == via) onVia = true;
                    else if (onVia) { via = RoadId.None; onVia = false; }
                }
                if (TrafficV2Lifecycle.HasReachedExit(actor.Location, exit)) { result.Exited = true; break; }
                if (actor.Location.Localized && actor.Location.ElementKind == RoadElementKind.JunctionMovement)
                {
                    if (speed < result.MinMovementSpeed)
                    {
                        result.MinMovementSpeed = speed;
                        result.MinMovementAt = actor.Location.ElementId + " s=" + actor.Location.SMeters.ToString("0.##", CultureInfo.InvariantCulture);
                    }
                    if (speed > 0f && speed < vehicle.MinimumDirectionSpeed) result.InactiveSteeringSteps++;
                }
                var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, route, route.ExitPortalId,
                    insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver, TrackingTolerance.Undeclared,
                    null, null, Admission.Evidence, via, offset));
                if (decision.Route.Plan != null)
                {
                    if (decision.Route.Outcome == RouteOutcome.Replanned)
                    {
                        result.Replans++;
                        track = ReferenceTrack.FromRoute(model, decision.Route.Plan.Occurrences, offset);
                        pieceHint = 0;
                    }
                    route = decision.Route.Plan;
                }
                if (!via.IsEmpty && route.ViaMovementId == via && route.ViaOccurrenceIndex >= 0
                    && route.ProgressOccurrenceIndex > route.ViaOccurrenceIndex)
                { via = RoadId.None; onVia = false; }
                int piece;
                float distance = track.Project(reference, pieceHint, out piece);
                pieceHint = piece;
                float nominalHeading = track.NominalHeadingErrorDegrees(piece, distance);
                float acceleration = -driver.SafeBrakingLimit;
                float target = 0f;
                SpeedPlan plan = decision.Motion == null ? null : SpeedPlan.Build(decision.Motion, model, driver, speed,
                    vehicle.LateralFrictionCoefficient * Physics.gravity.magnitude);
                if (plan != null && plan.Accepted)
                {
                    var first = decision.Path.Intervals[0];
                    var curve = CurveOf(model, first);
                    var command = MotionCommand.Track((ulong)step, 1, plan, driver, drivability, curve, first.StartSMeters,
                        reference, forward, speed, Dt, nominalHeading);
                    acceleration = command.TargetAccelerationMetersPerSecondSquared;
                    target = command.TargetWheelAngleDegrees;
                    if (Math.Abs(command.LateralErrorMeters) > result.MaxLateralError)
                    {
                        result.MaxLateralError = Math.Abs(command.LateralErrorMeters);
                        result.MaxLateralAt = "pas " + step + " " + actor.Location.ElementKind + " " + actor.Location.ElementId
                            + " s=" + actor.Location.SMeters.ToString("0.##", CultureInfo.InvariantCulture)
                            + " v=" + speed.ToString("0.##", CultureInfo.InvariantCulture)
                            + " kappa=" + command.ReferenceCurvaturePerMeter.ToString("0.###", CultureInfo.InvariantCulture)
                            + " e_nom=" + (-nominalHeading).ToString("0.#", CultureInfo.InvariantCulture)
                            + " cap=" + command.HeadingErrorDegrees.ToString("0.#", CultureInfo.InvariantCulture);
                    }
                    // v*(s*) du moniteur du driver : plafond de la pose nominale de la reference.
                    float ceiling = PathHorizon.NominalSteeringCeilingMetersPerSecond(drivability, track.OffsetRadians(piece, distance));
                    if (!float.IsPositiveInfinity(ceiling))
                    {
                        result.MaxSpeedRatio = Math.Max(result.MaxSpeedRatio, speed / ceiling);
                        if (speed > ceiling) result.CeilingExceeded++;
                    }
                }
                else
                {
                    result.Fallbacks++;
                    if (result.FirstFallback == null)
                    {
                        result.FirstFallback = step + " : " + (plan == null ? decision.Route.Reason.ToString()
                            : plan.Issue + "/" + plan.Verification.Issue + " @ " + plan.Verification.DistanceMeters.ToString(CultureInfo.InvariantCulture));
                        if (plan != null)
                        {
                            float at = plan.Verification.DistanceMeters;
                            var near = plan.Points.OrderBy(p => Math.Abs(p.DistanceMeters - at)).Take(3).OrderBy(p => p.DistanceMeters);
                            result.FirstFallback += " ; v=" + speed.ToString("0.###", CultureInfo.InvariantCulture) + " liante=" + plan.Binding
                                + " " + actor.Location.ElementKind + " " + actor.Location.ElementId + " s="
                                + actor.Location.SMeters.ToString("0.##", CultureInfo.InvariantCulture) + " points ["
                                + string.Join(" | ", near.Select(p => string.Format(CultureInfo.InvariantCulture,
                                    "d={0:0.###} v={1:0.###} v*={2} arr={3:0.###} av={4:0.###} bas={5:0.###} {6}", p.DistanceMeters,
                                    p.SpeedMetersPerSecond, p.CeilingUnbounded ? "inf" : p.CeilingMetersPerSecond.ToString("0.###", CultureInfo.InvariantCulture),
                                    p.BackwardMetersPerSecond, p.ForwardMetersPerSecond, p.CurrentStateMetersPerSecond, p.Binding)).ToArray()) + "]";
                        }
                    }
                }
                float available = VehicleSteeringModel.ResolveSteerAngleDegrees(1f, speed, vehicle.MinimumDirectionSpeed,
                    vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);
                float goal = available > 0f ? Mathf.Clamp(target, -available, available) : 0f;
                float rate = VehicleSteeringModel.ResolveSteerRateDegreesPerSecond(wheel, goal,
                    vehicle.SteerRateDegreesPerSecond, vehicle.SteerReturnRateDegreesPerSecond);
                wheel = VehicleSteeringModel.MoveSteerAngleDegrees(wheel, goal, rate, Dt);
                speed = Mathf.Max(0f, speed + acceleration * Dt);
                yaw += speed * Mathf.Tan(wheel * Mathf.Deg2Rad) / drivability.WheelbaseMeters * Dt;
                rear += new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw)) * speed * Dt;
            }
            return result;
        }

        private static RoadCurve CurveOf(CompiledRoadModel model, PathInterval interval)
        {
            EffectiveLaneCorridor corridor;
            CompiledJunctionMovement movement;
            if (interval.Kind == RoadElementKind.LaneCorridor && model.TryGetCorridor(interval.Id, out corridor)) return corridor.Curve;
            Assert.That(model.TryGetMovement(interval.Id, out movement), Is.True);
            return movement.Curve;
        }

        [Test]
        [Timeout(1200000)]
        public void KinematicReplayOfTheWholeChainStaysUnderTheSteeringCeilingPortalToPortal()
        {
            var built = BuildCampaign();
            Assert.That(built.Triplets.Length, Is.GreaterThan(0));
            var driver = Driver;
            var lines = new List<string>();
            bool ok = true;
            foreach (var triplet in built.Triplets)
            {
                RoadId entry, exit, via;
                RoadId.TryParse(triplet.Entry, out entry);
                RoadId.TryParse(triplet.Exit, out exit);
                RoadId.TryParse(triplet.Via, out via);
                var insertion = TrafficV2Lifecycle.PrepareInsertion(Admission, entry, exit, triplet.Seed, triplet.InsertionCounter,
                    driver, Dt, via);
                Assert.That(insertion.Code, Is.EqualTo(TrafficV2Code.Allowed));
                var result = Replay(insertion, 6000);
                lines.Add("triplet " + triplet.Index + " : sortie " + result.Exited + ", " + result.Steps + " pas, replis "
                    + result.Fallbacks + " (premier " + result.FirstFallback + "), max v/v* "
                    + result.MaxSpeedRatio.ToString("0.####", CultureInfo.InvariantCulture) + " (" + result.CeilingExceeded
                    + " pas au-dessus), max ecart lateral " + result.MaxLateralError.ToString("0.###", CultureInfo.InvariantCulture)
                    + " (" + result.MaxLateralAt + ")"
                    + " m, replanifications " + result.Replans + ", v min en mouvement "
                    + result.MinMovementSpeed.ToString("0.###", CultureInfo.InvariantCulture) + " (" + result.MinMovementAt
                    + "), pas direction inactive " + result.InactiveSteeringSteps);
                // Decision du 2026-09-30 : plus de rampe a la vitesse minimale de direction dans les mouvements.
                ok &= result.Exited && result.Fallbacks == 0 && result.CeilingExceeded == 0
                    && result.InactiveSteeringSteps == 0 && result.MinMovementSpeed >= 1f;
            }
            string report = string.Join("\n", lines);
            TestContext.WriteLine(report);
            Assert.That(ok, Is.True, "chaque route sort au portail, sans repli, avec v <= v*(s) partout et sans rampe "
                + "sous 1 m/s dans les mouvements :\n" + report);
        }
    }
}
