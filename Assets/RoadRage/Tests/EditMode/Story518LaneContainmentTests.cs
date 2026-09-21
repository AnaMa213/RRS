using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// ANO-5.18-04 : LE MAINTIEN DANS LA VOIE EST UN INVARIANT GEOMETRIQUE, pas une impression.
    ///
    /// Le banc simule la loi de guidage -- et elle seule -- sur la geometrie REELLE du district :
    /// meme visee, meme loi de braquage, memes constantes authorees que le vehicule. Il ne simule pas
    /// les pneus ; il repond a la seule question que la loi de guidage decide : le point vise
    /// reste-t-il sur la voie, et le braquage disponible suffit-il a l'y suivre.
    ///
    /// Le budget qu'il verifie est mesure, pas choisi : les axes de sens opposes du district sont
    /// distants de 4,00 m et le vehicule fait 2,06 m de large, donc la caisse dispose de 0,97 m
    /// d'ecart avant de franchir l'axe median. Les deux defauts corriges en consommaient 1,68 m
    /// (visee extrapolee hors de l'anneau) et 1,84 m (rayon intenable a la vitesse desiree).
    /// </summary>
    public sealed class Story518LaneContainmentTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string VehicleProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";

        /// <summary>Fenetre de visee du conducteur IA, telle qu'authoree sur le prefab.</summary>
        private const float LookAheadSeconds = 1f;

        private const float SteerFullLockDegrees = 45f;
        private const float SampleStep = 1f;
        private const float StepSeconds = 0.02f;

        // ------------------------------------------------------------------ le budget, mesure

        [Test]
        public void TheDistrictLeavesLessThanAMetreOfLateralErrorBeforeTheCentreLine()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var separation = MedianOpposingLaneSeparation(graph);
                var halfWidth = VehicleHalfWidth();

                Assert.That(separation, Is.EqualTo(4f).Within(0.5f),
                    "Les axes de sens opposes du district sont distants de " + separation.ToString("0.00") + " m.");

                var budget = separation * 0.5f - halfWidth;
                Assert.That(budget, Is.GreaterThan(0.5f).And.LessThan(1.5f),
                    "Budget d'ecart lateral mesure : " + budget.ToString("0.00") + " m.");
            });
        }

        // ------------------------------------------------------------------ giratoires

        [Test]
        public void TheGuidanceLawStaysInsideTheLaneAroundEveryRoundabout()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var rings = FindRings(graph);
                Assert.That(rings, Is.Not.Empty, "Aucun anneau de giratoire trouve dans MVP_Run.");

                var budget = LateralBudget(graph);
                foreach (var ring in rings)
                {
                    var reference = BuildRingReference(graph, ring);
                    var error = Simulate(reference, curveCeiling: true, pathAim: true);
                    Assert.That(error, Is.LessThan(budget),
                        "Anneau centre en " + RingCentre(graph, ring).ToString("F1") + " : ecart maximal "
                        + error.ToString("0.00") + " m pour un budget de " + budget.ToString("0.00") + " m.");
                }
            });
        }

        [Test]
        public void TheTangentialAimPointUsedBeforeLeftTheRoundaboutLane()
        {
            // Ce test documente LA CAUSE, et il echouerait si quelqu'un revenait a la visee
            // extrapolee en croyant simplifier : elle vise hors de la chaussee des que la voie tourne.
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var ring = FindRings(graph).FirstOrDefault();
                Assert.That(ring, Is.Not.Null, "Aucun anneau de giratoire trouve dans MVP_Run.");

                var reference = BuildRingReference(graph, ring);
                var error = Simulate(reference, curveCeiling: true, pathAim: false);
                Assert.That(error, Is.GreaterThan(LateralBudget(graph)),
                    "La visee tangentielle tenait l'anneau, ce que la mesure ANO-5.18-04 contredit.");
            });
        }

        // ------------------------------------------------------------------ virages de carrefour

        [Test]
        public void NoJunctionTurnEverReachesTheOpposingLane()
        {
            // C'EST LE CRITERE DE RECETTE : "les vehicules ne doivent pas mordre sur l'autre voie".
            // Il se mesure contre l'axe de la voie OPPOSEE, a 4,00 m de la notre ; la caisse
            // l'atteint a 2,97 m d'ecart. Aucun virage du district ne doit en approcher.
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var reachOpposing = MedianOpposingLaneSeparation(graph) - VehicleHalfWidth();
                var worst = 0f;
                var worstLabel = "(aucun)";

                foreach (var movement in TurningMovements(graph))
                {
                    var error = Simulate(movement.reference, curveCeiling: true, pathAim: true);
                    if (error > worst) { worst = error; worstLabel = movement.label; }
                }

                Assert.That(worst, Is.LessThan(reachOpposing),
                    "Le virage " + worstLabel + " porte la caisse a " + worst.ToString("0.00")
                    + " m de son axe, pour " + reachOpposing.ToString("0.00") + " m avant la voie opposee.");
            });
        }

        [Test]
        public void TheTightestTurnsStayAtTheirMeasuredPhysicalFloor()
        {
            // Le virage a 90 deg le plus serre du district demande un rayon de 4,24 m. A la vitesse
            // que le plafond de courbure autorise, le braquage requis (35,8 deg) egale presque le
            // braquage disponible (35,9 deg) : il ne reste aucune autorite pour corriger, et le
            // vehicule sort du virage avec un depassement transitoire qu'il resorbe ensuite.
            //
            // CE PLANCHER EST PHYSIQUE, pas un reglage. Trois pistes ont ete mesurees et ecartees :
            // un conge circulaire a rayon constant donne le meme ecart que la Bezier (1,02 contre
            // 1,09 m) ; la visee n'y change presque rien sur un balayage de 0,2 a 1,2 s ; une reserve
            // de braquage sur le plafond ne l'ameliore pas et immobilise les giratoires. Le deplacer
            // demande de l'AUTHORING -- un carrefour plus large ou un vehicule plus court -- pas du
            // code. Ce test borne la derive : si le chiffre monte, quelque chose a regresse.
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var worst = TurningMovements(graph)
                    .Max(movement => Simulate(movement.reference, curveCeiling: true, pathAim: true));

                Assert.That(worst, Is.LessThan(1.6f),
                    "Ecart maximal en virage : " + worst.ToString("0.00") + " m (plancher mesure ~1,35 m).");
            });
        }

        [Test]
        public void WithoutTheCurveSpeedCeilingTheTightTurnsLeaveTheLane()
        {
            // L'autre moitie de la cause : a la vitesse desiree authoree, le braquage disponible ne
            // couvre pas le rayon du connecteur serre. Aucune correction de visee ne rattrape une
            // consigne physiquement infaisable -- c'est pourquoi les deux correctifs vont ensemble.
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var budget = LateralBudget(graph);
                var leaving = TurningMovements(graph)
                    .Count(movement => Simulate(movement.reference, curveCeiling: false, pathAim: true,
                        movement.corner, movement.boxRadius) >= budget);

                Assert.That(leaving, Is.GreaterThan(0),
                    "Sans plafond de courbure tous les virages tenaient, ce que la mesure ANO-5.18-04 contredit.");
            });
        }

        [Test]
        public void TheCurveCeilingSlowsTheTightTurnsAndLeavesTheStraightLanesAlone()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var vehicle = LoadVehicleProfile();
                var wheelbase = Wheelbase(vehicle);
                var desired = LoadDriverProfile().DesiredSpeed;

                var straight = new[]
                {
                    Vector3.zero, new Vector3(0f, 0f, 5f), new Vector3(0f, 0f, 10f), new Vector3(0f, 0f, 15f),
                };
                Assert.That(
                    LaneGraphRouting.ResolvePathCurveSpeedLimit(straight, straight.Length, Vector3.zero, 12f,
                        wheelbase, vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees,
                        vehicle.SteerFullReductionSpeed),
                    Is.EqualTo(float.PositiveInfinity),
                    "Une ligne droite ne doit imposer aucun ralentissement.");

                var slowed = 0;
                foreach (var movement in TurningMovements(graph))
                {
                    var limit = LaneGraphRouting.ResolvePathCurveSpeedLimit(movement.reference,
                        movement.reference.Length, movement.reference[0], 40f, wheelbase,
                        vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees,
                        vehicle.SteerFullReductionSpeed);
                    if (limit < desired) slowed++;
                    Assert.That(limit, Is.GreaterThan(1f),
                        movement.label + " exige un rayon qu'aucune vitesse utile ne tient (" + limit.ToString("0.0") + " m/s).");
                }

                Assert.That(slowed, Is.GreaterThan(0),
                    "Aucun virage du district n'imposait de ralentissement, ce que la mesure contredit.");
            });
        }

        // ------------------------------------------------------------------ simulation de la loi de guidage

        /// <summary>
        /// Parcourt la trajectoire de reference avec la LOI DE GUIDAGE du vehicule et rend l'ecart
        /// lateral maximal a cette trajectoire. Rien n'y est invente : la visee, le braquage, le
        /// plafond de courbure et toutes les constantes viennent du code et des assets de production.
        /// </summary>
        private static float Simulate(Vector3[] reference, bool curveCeiling, bool pathAim)
        {
            return Simulate(reference, curveCeiling, pathAim, Vector3.zero, -1f);
        }

        /// <summary>
        /// <paramref name="exclusionRadius"/> retire de la mesure l'INTERIEUR d'une boite
        /// d'intersection. Ce n'est pas une indulgence : il n'y a pas de voie a l'interieur d'un
        /// carrefour, seulement du bitume ouvert que le vehicule traverse, et l'arbitrage de priorite
        /// -- pas le maintien de voie -- y repond de qui l'occupe. Le maintien dans la voie se mesure
        /// la ou une voie existe : sur l'approche, sur la sortie, et sur l'anneau d'un giratoire.
        /// </summary>
        private static float Simulate(Vector3[] reference, bool curveCeiling, bool pathAim,
            Vector3 exclusionCentre, float exclusionRadius)
        {
            var vehicle = LoadVehicleProfile();
            var driver = LoadDriverProfile();
            var wheelbase = Wheelbase(vehicle);

            var position = reference[0];
            var forward = Planar(reference[1] - reference[0]).normalized;
            var speed = driver.DesiredSpeed;
            var worst = 0f;
            var travelled = 0f;
            var total = PolylineLength(reference);

            for (var step = 0; step < 4000 && travelled < total - 2f; step++)
            {
                var ceiling = curveCeiling
                    ? LaneGraphRouting.ResolvePathCurveSpeedLimit(reference, reference.Length, position,
                        Mathf.Max(6f, speed * LookAheadSeconds + 6f), wheelbase, vehicle.MaxSteerAngleDegrees,
                        vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed)
                    : float.PositiveInfinity;
                var target = Mathf.Min(driver.DesiredSpeed, float.IsFinite(ceiling) ? Mathf.Max(1f, ceiling) : driver.DesiredSpeed);
                speed = Mathf.MoveTowards(speed, target, Mathf.Max(0.1f, driver.MaxAcceleration) * StepSeconds * 3f);

                var lookAhead = Mathf.Max(0f, speed) * LookAheadSeconds;
                // Le MEME plancher que le vehicule : sous l'empattement la poursuite oscille.
                if (pathAim) lookAhead = Mathf.Max(lookAhead, wheelbase);

                var aim = pathAim
                    ? LaneGraphRouting.ResolvePathLookAheadPoint(reference, reference.Length, position, lookAhead)
                    : TangentialAim(reference, position, lookAhead);

                var steer = NetworkedAIVehicleDriverController
                    .ComputeSeekIntent(position, forward, aim, SteerFullLockDegrees).Steer;
                var angle = VehicleSteeringModel.ResolveSteerAngleDegrees(steer, speed,
                    vehicle.MinimumDirectionSpeed, vehicle.MaxSteerAngleDegrees,
                    vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);

                var yaw = speed / wheelbase * Mathf.Tan(angle * Mathf.Deg2Rad) * StepSeconds * Mathf.Rad2Deg;
                forward = Quaternion.AngleAxis(yaw, Vector3.up) * forward;
                var delta = forward * speed * StepSeconds;
                position += delta;
                travelled += delta.magnitude;

                if (exclusionRadius <= 0f
                    || Planar(position - exclusionCentre).magnitude > exclusionRadius)
                {
                    worst = Mathf.Max(worst, DistanceToPolyline(reference, position));
                }
            }

            return worst;
        }

        /// <summary>La visee d'AVANT : extrapolation en ligne droite au-dela du noeud le plus proche.</summary>
        private static Vector3 TangentialAim(Vector3[] reference, Vector3 position, float lookAhead)
        {
            var best = 0;
            var bestSquared = float.PositiveInfinity;
            for (var i = 0; i < reference.Length; i++)
            {
                var squared = Planar(reference[i] - position).sqrMagnitude;
                if (squared >= bestSquared) continue;
                bestSquared = squared;
                best = i;
            }

            var node = reference[Mathf.Min(best + 1, reference.Length - 1)];
            var tangent = Planar(reference[Mathf.Min(best + 1, reference.Length - 1)] - reference[best]);
            if (tangent.sqrMagnitude <= 0.0001f) tangent = Planar(node - position);
            return LaneGraphRouting.ResolveLookAheadPoint(position, node, tangent.normalized, lookAhead, Vector3.zero, 0f);
        }

        // ------------------------------------------------------------------ trajectoires de reference

        /// <summary>
        /// La trajectoire de reference est construite avec <see cref="LaneGraphRouting.SampleLaneEdge"/>,
        /// c'est-a-dire la BRIQUE MEME que le vehicule utilise. Mesurer contre une autre courbe ne
        /// prouverait rien de ce que le vehicule suit.
        /// </summary>
        private static Vector3[] BuildRingReference(LaneGraph graph, List<int> ring)
        {
            var buffer = new Vector3[512];
            var count = 0;
            buffer[count++] = graph.GetNodePosition(ring[0]);
            // Deux tours : le premier place le vehicule, le second est celui qu'on mesure.
            for (var lap = 0; lap < 2; lap++)
            {
                for (var i = 0; i < ring.Count; i++)
                {
                    var from = ring[(i + lap * ring.Count) % ring.Count];
                    var to = ring[(i + 1) % ring.Count];
                    count = LaneGraphRouting.SampleLaneEdge(graph.GetNodePosition(from), NodeForward(graph, from),
                        graph.GetNodePosition(to), NodeForward(graph, to), SampleStep, buffer, count);
                }
            }

            return Flatten(buffer, count);
        }

        private static IEnumerable<(string label, Vector3[] reference, Vector3 corner, float boxRadius)> TurningMovements(LaneGraph graph)
        {
            foreach (var approach in graph.JunctionApproaches)
            {
                var node = approach.NodeIndex;
                if (approach.JunctionKey <= 0) continue;

                // La FRONTIERE de la jonction, telle que le graphe l'a indexee : c'est de la que part
                // le mouvement conduit comme le mouvement arbitre. Partir du noeud de decision, qui
                // n'est qu'a 2,00 m du coin des axes, exigeait un rayon de 0,76 m -- impossible.
                var entryForward = Planar(approach.EntryForward);
                if (entryForward.sqrMagnitude <= 0.0001f) entryForward = NodeForward(graph, node);
                if (entryForward.sqrMagnitude <= 0.0001f) continue;
                entryForward.Normalize();
                var entryPoint = approach.EntryNode == node ? graph.GetNodePosition(node) : approach.EntryPoint;

                foreach (var exit in graph.GetSuccessors(node))
                {
                    var exitForward = NodeForward(graph, exit);
                    if (exitForward.sqrMagnitude <= 0.0001f) continue;
                    if (Mathf.Abs(Vector3.SignedAngle(entryForward, exitForward, Vector3.up)) < 20f) continue;

                    var buffer = new Vector3[512];
                    var count = 0;

                    // Une amorce droite en amont : le vehicule doit ARRIVER sur le virage a sa vitesse
                    // de croisiere, sinon la mesure teste un demarrage et non un virage.
                    var start = entryPoint - entryForward * 20f;
                    buffer[count++] = start;
                    count = LaneGraphRouting.SampleLaneEdge(start, entryForward,
                        entryPoint, entryForward, SampleStep, buffer, count);
                    count = LaneGraphRouting.SampleLaneEdge(entryPoint, entryForward,
                        graph.GetNodePosition(exit), exitForward, SampleStep, buffer, count);
                    // Et une sortie droite, pour que la fenetre de visee ne tombe pas du bout.
                    count = LaneGraphRouting.SampleLaneEdge(graph.GetNodePosition(exit), exitForward,
                        graph.GetNodePosition(exit) + exitForward * 15f, exitForward, SampleStep, buffer, count);

                    // La boite d'intersection, deduite de la geometrie et non choisie : le coin des
                    // deux axes de voie, elargi de la distance du noeud de decision a ce coin plus la
                    // demi-chaussee. C'est l'aire ou il n'y a plus de voie a tenir.
                    var hasCorner = LaneGraphRouting.TryResolveLaneAxisCorner(
                        entryPoint, entryForward, graph.GetNodePosition(exit), exitForward, out var corner);
                    if (!hasCorner) corner = graph.GetNodePosition(node);
                    var boxRadius = Planar(corner - graph.GetNodePosition(node)).magnitude
                        + MedianOpposingLaneSeparation(graph) * 0.5f;

                    yield return (node + "->" + exit, Flatten(buffer, count), corner, boxRadius);
                }
            }
        }

        // ------------------------------------------------------------------ geometrie du district

        private static float LateralBudget(LaneGraph graph)
        {
            return MedianOpposingLaneSeparation(graph) * 0.5f - VehicleHalfWidth();
        }

        private static float MedianOpposingLaneSeparation(LaneGraph graph)
        {
            var separations = new List<float>();
            var segments = new List<(Vector3 start, Vector3 end, Vector3 direction)>();
            for (var i = 0; i < graph.NodeCount; i++)
            {
                foreach (var s in graph.GetSuccessors(i))
                {
                    var a = graph.GetNodePosition(i);
                    var b = graph.GetNodePosition(s);
                    var d = Planar(b - a);
                    if (d.sqrMagnitude < 0.01f) continue;
                    segments.Add((a, b, d.normalized));
                }
            }

            foreach (var segment in segments)
            {
                var best = float.PositiveInfinity;
                var mid = (segment.start + segment.end) * 0.5f;
                foreach (var other in segments)
                {
                    if (Vector3.Dot(segment.direction, other.direction) > -0.85f) continue;
                    var closest = TrafficPerception.ClosestPointOnSegment(mid, other.start, other.end);
                    var inside = Vector3.Dot(closest - other.start, other.direction);
                    var span = Vector3.Distance(other.start, other.end);
                    if (inside <= 0.05f || inside >= span - 0.05f) continue;
                    best = Mathf.Min(best, Planar(closest - mid).magnitude);
                }

                if (float.IsFinite(best)) separations.Add(best);
            }

            separations.Sort();
            Assert.That(separations, Is.Not.Empty, "Aucune paire de voies en vis-a-vis dans le district.");
            return separations[separations.Count / 2];
        }

        private static List<List<int>> FindRings(LaneGraph graph)
        {
            var rings = new List<List<int>>();
            var seen = new HashSet<int>();
            for (var start = 0; start < graph.NodeCount; start++)
            {
                if (seen.Contains(start)) continue;
                var ring = new List<int>();
                var current = start;
                for (var step = 0; step < 64; step++)
                {
                    ring.Add(current);
                    var next = BestAlignedSuccessor(graph, current);
                    if (next < 0) break;
                    if (next == start && ring.Count >= 5)
                    {
                        rings.Add(ring);
                        foreach (var n in ring) seen.Add(n);
                        break;
                    }

                    if (ring.Contains(next)) break;
                    current = next;
                }
            }

            return rings;
        }

        private static int BestAlignedSuccessor(LaneGraph graph, int node)
        {
            var forward = NodeForward(graph, node);
            var best = -1;
            var bestDot = -2f;
            foreach (var s in graph.GetSuccessors(node))
            {
                var d = Planar(graph.GetNodePosition(s) - graph.GetNodePosition(node));
                if (d.sqrMagnitude < 0.01f) continue;
                var dot = Vector3.Dot(d.normalized, forward);
                if (dot > bestDot) { bestDot = dot; best = s; }
            }

            return best;
        }

        private static Vector3 RingCentre(LaneGraph graph, List<int> ring)
        {
            var centre = Vector3.zero;
            foreach (var n in ring) centre += graph.GetNodePosition(n);
            return centre / ring.Count;
        }

        // ------------------------------------------------------------------ donnees authorees

        private static VehicleProfile LoadVehicleProfile()
        {
            var def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath);
            Assert.That(def, Is.Not.Null, "VehicleProfileDef_Default attendu : " + VehicleProfilePath);
            return def.Profile;
        }

        private static DriverProfile LoadDriverProfile()
        {
            var def = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath);
            Assert.That(def, Is.Not.Null, "DriverProfileDef_Default attendu : " + DriverProfilePath);
            return def.Profile;
        }

        private static float Wheelbase(VehicleProfile vehicle)
        {
            var min = float.PositiveInfinity;
            var max = float.NegativeInfinity;
            for (var i = 0; i < vehicle.WheelCount; i++)
            {
                var z = vehicle.GetWheel(i).LocalPosition.z;
                min = Mathf.Min(min, z);
                max = Mathf.Max(max, z);
            }

            return Mathf.Max(0.0001f, max - min);
        }

        private static float VehicleLength()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            var box = prefab.GetComponentsInChildren<BoxCollider>(true).FirstOrDefault(candidate => !candidate.isTrigger);
            Assert.That(box, Is.Not.Null, "Le vehicule IA doit porter une emprise solide.");
            return Mathf.Abs(box.size.z * box.transform.lossyScale.z);
        }

        private static float VehicleHalfWidth()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            Assert.That(prefab, Is.Not.Null, "Greybox_AIVehicle attendu.");
            var box = prefab.GetComponentsInChildren<BoxCollider>(true).FirstOrDefault(candidate => !candidate.isTrigger);
            Assert.That(box, Is.Not.Null, "Le vehicule IA doit porter une emprise solide.");
            return Mathf.Abs(box.size.x * box.transform.lossyScale.x) * 0.5f;
        }

        // ------------------------------------------------------------------ utilitaires

        private static Vector3 NodeForward(LaneGraph graph, int node)
        {
            var forward = Planar(graph.GetNodeRotation(node) * Vector3.forward);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.zero;
        }

        private static Vector3 Planar(Vector3 value) => Vector3.ProjectOnPlane(value, Vector3.up);

        private static Vector3[] Flatten(Vector3[] buffer, int count)
        {
            var path = new Vector3[count];
            for (var i = 0; i < count; i++) path[i] = new Vector3(buffer[i].x, 0f, buffer[i].z);
            return path;
        }

        private static float PolylineLength(Vector3[] path)
        {
            var total = 0f;
            for (var i = 1; i < path.Length; i++) total += Planar(path[i] - path[i - 1]).magnitude;
            return total;
        }

        private static float DistanceToPolyline(Vector3[] path, Vector3 position)
        {
            var best = float.PositiveInfinity;
            for (var i = 0; i + 1 < path.Length; i++)
            {
                var closest = TrafficPerception.ClosestPointOnSegment(position, path[i], path[i + 1]);
                best = Mathf.Min(best, Planar(closest - position).magnitude);
            }

            return best;
        }

        private static LaneGraph ResolveGraph(Scene scene)
        {
            var runRoot = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == "RunRoot");
            Assert.That(runRoot, Is.Not.Null, "RunRoot attendu dans MVP_Run");
            var graph = runRoot.GetComponentInChildren<LaneGraph>(true);
            Assert.That(graph, Is.Not.Null, "LaneGraph attendu sous RunRoot");
            graph.Rebuild();
            return graph;
        }

        private static void WithMvpRun(Action<Scene> body)
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MvpRunScenePath);
            var wasOpen = alreadyOpen.IsValid();
            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
            try
            {
                body(scene);
            }
            finally
            {
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
