using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.10 : le graphe de voies, le district greybox et le trafic portail-a-portail.
    ///
    /// Trois familles de gardes, toutes sans scene de jeu ni Netcode :
    /// 1. les decisions de parcours pures (<see cref="LaneGraphRouting"/>) et la matrice de cas limites ;
    /// 2. la topologie construite par <see cref="LaneGraph"/>, verifiee sur des doubles montes a la
    ///    main puis sur le district reellement authore dans MVP_Run ;
    /// 3. les invariants de code que rien d'autre ne garde : aucun effectif litteral hors du Def,
    ///    aucun retrait de vehicule ailleurs qu'a un portail, trottoirs hors du bake de l'agent
    ///    vehicule, et disparition de <c>RouteWaypoints</c>.
    /// </summary>
    public sealed class Story510LaneGraphAndRoutedTrafficTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string SpawnerSourcePath = "Assets/RoadRage/App/Run/PortalTrafficSpawner.cs";
        private const string DriverSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";
        private const string VehicleDriverSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string RunFlowSourcePath = "Assets/RoadRage/App/Run/RunFlowController.cs";
        private const string TrafficDefPath = "Assets/RoadRage/ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset";
        private const string NavMeshAreasPath = "ProjectSettings/NavMeshAreas.asset";

        private const string SegmentPrefabPath = "Assets/RoadRage/Prefabs/Greybox_RoadSegment_TwoWay.prefab";
        private const string IntersectionPrefabPath = "Assets/RoadRage/Prefabs/Greybox_Intersection.prefab";
        private const string TJunctionPrefabPath = "Assets/RoadRage/Prefabs/Greybox_TJunction.prefab";
        private const string RoundaboutPrefabPath = "Assets/RoadRage/Prefabs/Greybox_Roundabout.prefab";
        private const string TunnelPrefabPath = "Assets/RoadRage/Prefabs/Greybox_TunnelPortal.prefab";

        /// <summary>Le kit greybox complet : tout module pose dans le district sort de cette liste.</summary>
        private static readonly string[] ModulePrefabPaths =
        {
            SegmentPrefabPath, IntersectionPrefabPath, TJunctionPrefabPath, RoundaboutPrefabPath, TunnelPrefabPath
        };

        /// <summary>Index de l'aire NavMesh dediee aux trottoirs, tel qu'authore dans les ProjectSettings.</summary>
        private const int SidewalkAreaIndex = 3;

        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in spawned)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            spawned.Clear();
        }

        // ------------------------------------------------- tirage de virage pondere

        [Test]
        public void WeightedTurnDrawFollowsAuthoredRatiosAndIsDeterministicPerSeed()
        {
            var successors = new[] { 10, 20, 30 };
            var weights = new[] { 30f, 50f, 20f };

            var counts = new Dictionary<int, int> { { 10, 0 }, { 20, 0 }, { 30, 0 } };
            for (ulong seed = 1; seed <= 4000UL; seed++)
            {
                counts[LaneGraphRouting.SelectWeightedSuccessor(successors, weights, seed, 1, out _)]++;
            }

            Assert.That(counts[10] / 4000f, Is.EqualTo(0.30f).Within(0.03f), "ratio droite 30 (modele jtrrouter)");
            Assert.That(counts[20] / 4000f, Is.EqualTo(0.50f).Within(0.03f), "ratio tout droit 50");
            Assert.That(counts[30] / 4000f, Is.EqualTo(0.20f).Within(0.03f), "ratio gauche 20");

            var first = LaneGraphRouting.SelectWeightedSuccessor(successors, weights, 4242UL, 7, out _);
            var second = LaneGraphRouting.SelectWeightedSuccessor(successors, weights, 4242UL, 7, out _);
            Assert.That(second, Is.EqualTo(first),
                "Meme graine et meme pas : meme virage. Le parcours doit etre reproductible, jamais tire par UnityEngine.Random.");
        }

        [Test]
        public void WeightedTurnDrawFallsBackToFirstSuccessorWhenWeightsAreAbsentOrSumToZero()
        {
            var successors = new[] { 7, 8, 9 };

            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, null, 1UL, 1, out var noWeights), Is.EqualTo(7));
            Assert.That(noWeights, Is.True, "Poids absents : l'appelant doit pouvoir avertir une fois pour ce noeud.");

            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, new[] { 0f, -3f, float.NaN }, 1UL, 1, out var zeroSum), Is.EqualTo(7));
            Assert.That(zeroSum, Is.True, "Somme nulle : premier successeur, jamais de division par zero.");

            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(Array.Empty<int>(), null, 1UL, 1, out _), Is.EqualTo(-1),
                "Aucun successeur : -1, l'appelant reoriente vers la sortie la plus proche.");

            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(new[] { 5 }, null, 1UL, 1, out var single), Is.EqualTo(5));
            Assert.That(single, Is.False, "Successeur unique : aucun poids necessaire, donc aucun avertissement.");
        }

        [Test]
        public void EdgeBudgetOnlyTripsPastTheAuthoredMultipleOfTheGraphSize()
        {
            // Garde-fou --max-edges-factor de jtrrouter : facteur 2 sur un graphe de 10 noeuds = 20 aretes.
            Assert.That(LaneGraphRouting.IsEdgeBudgetExceeded(20, 10, 2f), Is.False, "Exactement au budget : pas encore depasse.");
            Assert.That(LaneGraphRouting.IsEdgeBudgetExceeded(21, 10, 2f), Is.True);
            Assert.That(LaneGraphRouting.IsEdgeBudgetExceeded(1000, 0, 2f), Is.False, "Graphe vide : aucune reorientation possible.");
            Assert.That(LaneGraphRouting.IsEdgeBudgetExceeded(1000, 10, 0f), Is.False, "Facteur nul : aucun budget authore, jamais de reorientation.");
        }

        [Test]
        public void RedirectPicksTheSuccessorThatClosesTheDistanceToTheExit()
        {
            var successors = new[] { 1, 2, 3 };
            var positions = new[] { new Vector3(0f, 0f, 0f), new Vector3(50f, 0f, 0f), new Vector3(10f, 0f, 0f) };

            Assert.That(LaneGraphRouting.SelectSuccessorTowardTarget(successors, positions, new Vector3(60f, 0f, 0f)), Is.EqualTo(2));
            Assert.That(LaneGraphRouting.SelectSuccessorTowardTarget(Array.Empty<int>(), positions, Vector3.zero), Is.EqualTo(-1));
        }

        // ------------------------------------------------- topologie du graphe (doubles)

        [Test]
        public void ConnectorsJoinBelowTheAuthoredThresholdAndAreReportedOrphanBeyondIt()
        {
            var settings = NewSettings(connectorJoinDistance: 0.75f);

            var graph = NewGraph(settings);
            var lead = NewNode(graph, "Lead", new Vector3(0f, 0f, 0f), 0f, LaneNodeRole.Normal);
            var outgoing = NewNode(graph, "Out", new Vector3(0f, 0f, 10f), 0f, LaneNodeRole.Connector);
            var incoming = NewNode(graph, "In", new Vector3(0f, 0f, 10.2f), 0f, LaneNodeRole.Connector);
            var tail = NewNode(graph, "Tail", new Vector3(0f, 0f, 20f), 0f, LaneNodeRole.Normal);

            Wire(lead, new[] { outgoing }, new[] { 1f });
            Wire(incoming, new[] { tail }, new[] { 1f });
            graph.Rebuild();

            Assert.That(graph.OrphanConnectors.Count, Is.EqualTo(0), "Deux connecteurs joints : une arete, aucune alerte.");
            Assert.That(graph.GetSuccessors(IndexOf(graph, outgoing)), Does.Contain(IndexOf(graph, incoming)),
                "Poser deux modules bout a bout doit suffire : l'arete de jointure est creee sans cablage manuel.");

            incoming.transform.position = new Vector3(0f, 0f, 12f);
            graph.Rebuild();

            Assert.That(graph.OrphanConnectors, Does.Contain(IndexOf(graph, outgoing)),
                "Connecteur hors seuil : aucune arete, et le connecteur est signale (gizmo d'alerte + validation).");
        }

        [Test]
        public void ConnectorsNeverJoinAgainstTheDirectionOfTravel()
        {
            var settings = NewSettings(connectorJoinDistance: 0.75f);
            var graph = NewGraph(settings);

            // Deux voies opposees a la meme jointure : la sortie de l'une ne doit jamais se brancher
            // sur l'entree de celle d'en face.
            var forwardOut = NewNode(graph, "FwdOut", new Vector3(0f, 0f, 0f), 0f, LaneNodeRole.Connector);
            var backwardIn = NewNode(graph, "BackIn", new Vector3(0f, 0f, 0.1f), 180f, LaneNodeRole.Connector);
            var tail = NewNode(graph, "Tail", new Vector3(0f, 0f, -10f), 180f, LaneNodeRole.Normal);
            Wire(backwardIn, new[] { tail }, new[] { 1f });
            graph.Rebuild();

            Assert.That(graph.GetSuccessors(IndexOf(graph, forwardOut)).Count, Is.EqualTo(0));
            Assert.That(graph.OrphanConnectors, Does.Contain(IndexOf(graph, forwardOut)));
        }

        [Test]
        public void AnEntryPortalMarkedReusableAlsoAcceptsOutgoingVehicles()
        {
            var graph = NewGraph(NewSettings(0.75f));
            var plain = NewNode(graph, "PlainEntry", Vector3.zero, 0f, LaneNodeRole.PortalEntry);
            var reusable = NewNode(graph, "ReusableEntry", new Vector3(50f, 0f, 0f), 0f, LaneNodeRole.PortalEntry);
            SetExitReusesEntry(reusable, true);
            graph.Rebuild();

            Assert.That(graph.EntryPortals.Count, Is.EqualTo(2), "Les deux restent des portails d'entree.");
            Assert.That(graph.IsExitPortal(IndexOf(graph, plain)), Is.False);
            Assert.That(graph.IsExitPortal(IndexOf(graph, reusable)), Is.True,
                "Portail configure sortie-reutilisant-l-entree : il accepte les sortants comme n'importe quelle sortie.");
        }

        [Test]
        public void AVehicleSpawnedOnAReusableExitPortalIsNotMarkedExitedBeforeItHasActuallyDriven()
        {
            // Le spawner pose le vehicule EXACTEMENT sur le noeud de portail (distance 0), donc le
            // tout premier appel de resolution de successeur recoit currentIndex == le noeud de
            // naissance lui-meme. Un portail sortie-reutilisant-l-entree doit rester traversable a la
            // naissance -- seul un retour ulterieur, apres avoir effectivement roule, doit declencher
            // la sortie (le defaut trouve en revue : sans cette distinction, ResolveNextNode marquait
            // reachedExitPortal des la naissance et le spawner redespawnait le vehicule sans qu'il
            // ait jamais avance).
            var graph = NewGraph(NewSettings(0.75f));
            var reusable = NewNode(graph, "ReusableEntry", Vector3.zero, 0f, LaneNodeRole.PortalEntry);
            var next = NewNode(graph, "Next", new Vector3(10f, 0f, 0f), 0f, LaneNodeRole.Normal);
            SetExitReusesEntry(reusable, true);
            Wire(reusable, new[] { next }, new[] { 1f });
            Wire(next, new[] { reusable }, new[] { 1f });
            graph.Rebuild();

            var reusableIndex = IndexOf(graph, reusable);

            var vehicleObject = new GameObject("Story510ReusablePortalVehicle");
            spawned.Add(vehicleObject);
            var controller = vehicleObject.AddComponent<NetworkedAIVehicleDriverController>();
            controller.BindLaneGraph(graph);

            // Naissance : currentIndex est le noeud de portail lui-meme, aucune arete encore parcourue.
            var firstResolved = InvokeResolveNextNode(controller, reusableIndex);

            Assert.That(controller.HasReachedExitPortal, Is.False,
                "Le vehicule vient de naitre sur ce noeud : ce n'est pas un retrait, c'est un depart.");
            Assert.That(firstResolved, Is.EqualTo(IndexOf(graph, next)),
                "La naissance sur un portail sortie-reutilisant-l-entree doit produire un vrai depart vers le graphe.");

            // Retour effectif : le vehicule a roule jusqu'a "Next" puis revient sur le portail.
            var secondResolved = InvokeResolveNextNode(controller, reusableIndex);

            Assert.That(controller.HasReachedExitPortal, Is.True,
                "Un retour ulterieur au meme noeud, apres avoir effectivement circule, doit bien declencher la sortie.");
            Assert.That(secondResolved, Is.EqualTo(reusableIndex));
        }

        [Test]
        public void AGraphWithoutAnyExitPortalReportsNoReachableExitInsteadOfRemovingAnything()
        {
            var graph = NewGraph(NewSettings(0.75f));
            // Le portail d'entree est indispensable au scenario : sans lui, la validation
            // s'arrete sur l'absence d'entree et le cas "aucune sortie" n'est jamais atteint.
            NewNode(graph, "Entry", Vector3.zero, 0f, LaneNodeRole.PortalEntry);
            NewNode(graph, "Lonely", new Vector3(50f, 0f, 0f), 0f, LaneNodeRole.Normal);
            graph.Rebuild();

            Assert.That(graph.NearestExitNodeIndex(Vector3.zero), Is.EqualTo(-1),
                "Aucune sortie atteignable : le driver rend le vehicule inerte, il ne le retire jamais.");
            Assert.That(graph.TryValidate(out var error), Is.False);
            Assert.That(error, Does.Contain("sortie"));
        }

        [Test]
        public void AGraphWithoutTrafficSettingsJoinsNothingAndFailsValidation()
        {
            var graph = NewGraph(null);
            var outgoing = NewNode(graph, "Out", Vector3.zero, 0f, LaneNodeRole.Connector);
            var incoming = NewNode(graph, "In", new Vector3(0f, 0f, 0.1f), 0f, LaneNodeRole.Connector);
            var tail = NewNode(graph, "Tail", new Vector3(0f, 0f, 10f), 0f, LaneNodeRole.Normal);
            Wire(incoming, new[] { tail }, new[] { 1f });
            graph.Rebuild();

            Assert.That(graph.GetSuccessors(IndexOf(graph, outgoing)).Count, Is.EqualTo(0),
                "Sans Def de trafic il n'existe aucun seuil de jointure authore : rien n'est invente.");
            Assert.That(graph.TryValidate(out var error), Is.False);
            Assert.That(error, Does.Contain("TrafficSettingsDef"));
        }

        // ------------------------------------------------- district authore dans MVP_Run

        [Test]
        public void MvpRunDistrictIsAConnectedPortalToPortalLaneGraph()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                graph.Rebuild();

                Assert.That(graph.TryValidate(out var error), Is.True, error);
                Assert.That(graph.NodeCount, Is.GreaterThan(0));
                Assert.That(graph.OrphanConnectors.Count, Is.EqualTo(0), "Aucun connecteur orphelin dans le district authore.");

                foreach (var entry in graph.EntryPortals)
                {
                    Assert.That(ReachesAnExit(graph, entry), Is.True,
                        "Le portail d'entree " + entry + " doit mener a un portail de sortie : sans cela un vehicule insere errerait sans jamais pouvoir sortir.");
                }
            });
        }

        [Test]
        public void MvpRunDistrictCarriesTwoWayRoadsJunctionsAndTunnelPortals()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var root = graph.transform;

                var avenues = CountModules(root, SegmentPrefabPath);
                var crossroads = CountModules(root, IntersectionPrefabPath);
                var tees = CountModules(root, TJunctionPrefabPath);
                var roundabouts = CountModules(root, RoundaboutPrefabPath);
                var tunnels = CountModules(root, TunnelPrefabPath);

                Assert.That(avenues, Is.GreaterThanOrEqualTo(2), "au moins deux routes a double sens");
                Assert.That(crossroads + tees + roundabouts, Is.GreaterThanOrEqualTo(2), "au moins deux intersections");
                Assert.That(tunnels, Is.GreaterThanOrEqualTo(2), "au moins deux tunnels-portails");

                // Topologie authoree : un anneau rectangulaire coupe par quatre jonctions en T,
                // quatre ronds-points aux coins, un carrefour en croix au centre.
                Assert.That(crossroads, Is.EqualTo(1), "un seul carrefour en croix, et il est au centre");
                Assert.That(tees, Is.EqualTo(4), "quatre jonctions en T, une a mi-cote de chaque avenue de l'anneau");
                Assert.That(roundabouts, Is.EqualTo(4), "quatre ronds-points, un par coin de l'anneau");
                Assert.That(tunnels, Is.EqualTo(4), "un tunnel-portail par coin, sur la branche sortante du rond-point");

                Assert.That(graph.EntryPortals.Count, Is.EqualTo(tunnels), "chaque tunnel porte un portail d'entree");
                Assert.That(graph.ExitPortals.Count, Is.EqualTo(tunnels), "chaque tunnel porte un portail de sortie");
            });
        }

        [Test]
        public void RoundaboutsAreGeometryOnlyWithAuthoredExitRatios()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RoundaboutPrefabPath);
            Assert.That(prefab, Is.Not.Null, RoundaboutPrefabPath + " attendu");

            var nodes = prefab.GetComponentsInChildren<LaneNode>(true);
            var splits = nodes.Where(node => node.name.StartsWith("Ring_Split", StringComparison.Ordinal)).ToArray();
            var merges = nodes.Where(node => node.name.StartsWith("Ring_Merge", StringComparison.Ordinal)).ToArray();

            Assert.That(splits.Length, Is.EqualTo(3), "un point de decision par branche du rond-point");
            Assert.That(merges.Length, Is.EqualTo(3), "un point d'insertion par branche");

            foreach (var split in splits)
            {
                Assert.That(split.Successors.Count, Is.EqualTo(2), split.name + " : sortir ici, ou continuer sur l'anneau.");
                Assert.That(split.TurnWeights.Count, Is.EqualTo(2), split.name + " : les deux choix portent un ratio authore.");
                foreach (var weight in split.TurnWeights)
                {
                    Assert.That(weight, Is.GreaterThan(0f), split.name + " : un ratio nul rendrait le tirage degenere.");
                }
            }

            // Un vehicule qui vient d'entrer ne peut pas ressortir aussitot par sa propre branche :
            // il s'insere APRES le point de sortie de celle-ci. On suit l'anneau jusqu'au PREMIER
            // point de decision rencontre -- en traversant les noeuds d'arc, qui ne decident rien --
            // et on verifie que ce n'est pas la sortie de sa propre branche.
            foreach (var merge in merges)
            {
                Assert.That(merge.Successors.Count, Is.EqualTo(1), merge.name + " : l'insertion ne decide rien, elle rejoint l'anneau.");

                var branch = merge.name.Substring("Ring_Merge_".Length);
                var cursor = merge.Successors[0];
                var guard = 0;
                while (cursor != null && cursor.Successors.Count == 1 && guard++ < 32)
                {
                    Assert.That(cursor.name, Does.StartWith("Ring_Arc"),
                        merge.name + " : seuls des noeuds d'arc peuvent s'intercaler avant le prochain point de decision.");
                    cursor = cursor.Successors[0];
                }

                Assert.That(cursor, Is.Not.Null);
                Assert.That(cursor.name, Does.StartWith("Ring_Split"), merge.name + " : l'anneau doit mener a un point de sortie.");
                Assert.That(cursor.name, Is.Not.EqualTo("Ring_Split_" + branch),
                    merge.name + " : un vehicule qui vient d'entrer ne doit pas pouvoir ressortir par sa propre branche.");
            }

            // Densite de l'anneau : le vehicule va TOUT DROIT d'un noeud au suivant. Un ecart angulaire
            // trop grand fait parcourir une corde qui coupe a l'interieur du giratoire -- c'est ce qui
            // envoyait les vehicules dans l'ilot central avant correction (trajectoire a 3,40 m du
            // centre, bord interieur du vehicule a 2,37 m, pour un ilot de 3 m de rayon).
            var islandRadius = prefab.transform.Find("Collision/Col_Island").localScale.x * 0.5f;
            var vehicle = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            var vehicleHalfWidth = vehicle.GetComponent<BoxCollider>().size.x * 0.5f;

            foreach (var node in nodes)
            {
                foreach (var successor in node.Successors)
                {
                    if (successor == null)
                    {
                        continue;
                    }

                    var clearance = DistanceFromModuleCentreToSegment(node.transform.localPosition, successor.transform.localPosition);
                    Assert.That(clearance - vehicleHalfWidth, Is.GreaterThan(islandRadius),
                        node.name + " -> " + successor.name + " : la corde passe a " + clearance.ToString("F2")
                        + " m du centre, le vehicule mordrait l'ilot de " + islandRadius.ToString("F2") + " m de rayon.");
                }
            }

            // Geometrie seulement : aucune regle de cession du passage, aucune priorite aux engages.
            // L'arbitrage d'intersection appartient a la Story 5.18 (ex-5.11).
            foreach (var path in new[] { DriverSourcePath, SpawnerSourcePath })
            {
                var source = CodeOnly(File.ReadAllText(path));
                foreach (var forbidden in new[] { "GiveWay", "Yield", "Priority", "Roundabout" })
                {
                    Assert.That(source, Does.Not.Contain(forbidden),
                        path + " : le rond-point est de la geometrie authoree, pas une regle de circulation (Story 5.18).");
                }
            }
        }

        [Test]
        public void TheMapCentreCarriesAFourBranchCrossroadsWithAuthoredTurnRatios()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                graph.Rebuild();

                // Identifie par son PREFAB, pas par le nom de l'instance : le critere d'acceptation
                // porte sur "un carrefour en croix a quatre branches au centre de la carte", pas sur
                // un libelle de hierarchie.
                var crossroads = graph.transform.Cast<Transform>()
                    .Where(child => IsInstanceOf(child, IntersectionPrefabPath))
                    .ToArray();
                Assert.That(crossroads.Length, Is.EqualTo(1), "Un seul carrefour en croix dans le district.");

                var centre = crossroads[0];
                Assert.That(Vector3.ProjectOnPlane(centre.position, Vector3.up).magnitude, Is.LessThan(0.01f),
                    "Le carrefour en croix doit etre au centre de la carte, pas decale sur une branche.");

                var junctions = centre.GetComponentsInChildren<LaneNode>(true)
                    .Select(node => graph.NearestNodeIndex(node.transform.position))
                    .Where(index => graph.GetSuccessors(index).Count >= 3)
                    .ToArray();

                Assert.That(junctions.Length, Is.EqualTo(4),
                    "Quatre branches : chaque sens d'arrivee au carrefour a son propre noeud de decision.");

                foreach (var junction in junctions)
                {
                    var successors = graph.GetSuccessors(junction);
                    var weights = graph.GetTurnWeights(junction);
                    Assert.That(successors.Count, Is.EqualTo(3), "gauche / tout droit / droite, jamais de demi-tour");
                    Assert.That(weights.Count, Is.EqualTo(3), "un ratio authore par virage");

                    var forward = graph.GetNodeRotation(junction) * Vector3.forward;
                    var origin = graph.GetNodePosition(junction);

                    var angles = successors
                        .Select(successor => Vector3.SignedAngle(forward, Flat(graph.GetNodePosition(successor) - origin), Vector3.up))
                        .ToArray();

                    var straight = Enumerable.Range(0, 3).Where(i => Mathf.Abs(angles[i]) < 15f).ToArray();
                    Assert.That(straight.Length, Is.EqualTo(1), "exactement un successeur tout droit");
                    Assert.That(angles.Count(angle => angle < -30f), Is.EqualTo(1), "exactement un successeur a gauche");
                    Assert.That(angles.Count(angle => angle > 30f), Is.EqualTo(1), "exactement un successeur a droite");

                    for (var i = 0; i < 3; i++)
                    {
                        Assert.That(weights[i], Is.GreaterThan(0f), "chaque virage porte un ratio authore strictement positif");
                    }

                    Assert.That(weights[straight[0]], Is.EqualTo(weights.Max()),
                        "Ratios jtrrouter : tout droit reste le virage dominant du carrefour.");
                }
            });
        }

        [Test]
        public void TwoVehiclesEnteringTheSameMvpRunPortalTakeDifferentRoutes()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                graph.Rebuild();

                var entry = graph.EntryPortals[0];
                var routes = new List<string>();
                for (ulong networkObjectId = 1; networkObjectId <= 8UL; networkObjectId++)
                {
                    routes.Add(WalkRoute(graph, entry, networkObjectId, out var endedAtExit));
                    Assert.That(endedAtExit, Is.True,
                        "Vehicule " + networkObjectId + " : un parcours tire doit finir a un portail de sortie, jamais s'arreter en chemin.");
                }

                Assert.That(routes.Distinct().Count(), Is.GreaterThan(1),
                    "Deux vehicules inseres au meme portail doivent diverger par le seul effet du tirage pondere aux jonctions -- aucun itineraire n'est pre-calcule.");
            });
        }

        [Test]
        public void MvpRunHasNoInScenePlacedTrafficAndAHostPortalSpawnerInstead()
        {
            WithMvpRun(scene =>
            {
                var runRoot = scene.GetRootGameObjects().First(candidate => candidate.name == "RunRoot");

                Assert.That(runRoot.GetComponentsInChildren<NetworkedAIVehicleState>(true).Length, Is.EqualTo(0),
                    "Plus aucun vehicule de trafic pose en scene : ils entrent tous par un portail a l'execution.");

                var spawner = runRoot.GetComponentInChildren<PortalTrafficSpawner>(true);
                Assert.That(spawner, Is.Not.Null, "PortalTrafficSpawner attendu dans MVP_Run");

                var serialized = new SerializedObject(spawner);
                Assert.That(serialized.FindProperty("laneGraph").objectReferenceValue, Is.Not.Null, "le spawner doit connaitre le graphe");
                Assert.That(serialized.FindProperty("vehiclePrefab").objectReferenceValue, Is.Not.Null, "le spawner doit connaitre le prefab reseau du trafic");

                var sceneText = File.ReadAllText(MvpRunScenePath);
                Assert.That(sceneText, Does.Not.Contain("value: AI_Vehicle_0"),
                    "Les trois instances de vehicule posees en scene (Story 5.2) sont retirees.");
                Assert.That(sceneText, Does.Not.Contain("m_Name: AIRoute"),
                    "La boucle de waypoints AIRoute est retiree : elle ne peut pas exprimer un parcours portail-a-portail.");
            });
        }

        // ------------------------------------------------- correctifs post-livraison (2026-09-16)

        [Test]
        public void SurfaceContactPredicateAcceptsWhatTouchesTheUndersideAndRejectsObstacles()
        {
            // Retour terrain n° 2 : monter sur une dalle affleurante, sur un trottoir ou franchir la
            // levre de 5 cm du plan de sol ne doit produire aucun degat. Ce qui separe une dalle d'un
            // mur n'est pas la vitesse de fermeture -- a vitesse de conduite, les deux sont
            // comparables -- mais la HAUTEUR a laquelle le contact touche le vehicule : une dalle
            // touche son dessous, un mur touche sa face. D'ou une comparaison de hauteurs, jamais un
            // seuil de vitesse supplementaire, qui laisserait le bump blesser des qu'on roule vite.
            const float tolerance = 0.1f; // le double de la levre de 5 cm mesuree (note de verification)
            const float underside = 0f;   // sur la chaussee, le dessous du vehicule est au plan de roulage

            Assert.That(NetworkedVehicleDriverController.IsSurfaceContact(underside, underside, tolerance), Is.True,
                "Dalle affleurante : le contact est exactement au niveau du dessous du vehicule.");
            Assert.That(NetworkedVehicleDriverController.IsSurfaceContact(-0.05f, underside, tolerance), Is.True,
                "Levre de 5 cm du plan de sol : le contact est SOUS le dessous du vehicule.");
            Assert.That(NetworkedVehicleDriverController.IsSurfaceContact(underside + tolerance, underside, tolerance), Is.True,
                "Exactement a la tolerance : dernier point encore accepte.");
            Assert.That(NetworkedVehicleDriverController.IsSurfaceContact(underside + tolerance + 0.01f, underside, tolerance), Is.False,
                "Au-dessus de la tolerance : c'est un obstacle (face d'un autre vehicule, mur), donc un vrai choc.");
            Assert.That(NetworkedVehicleDriverController.IsSurfaceContact(5f, underside, tolerance), Is.False,
                "Paroi de tunnel (5 m de haut) : tres au-dessus du dessous, les degats de la Story 3.5 restent appliques.");
            Assert.That(NetworkedVehicleDriverController.IsSurfaceContact(0.001f, underside, -1f), Is.False,
                "Tolerance negative : aucune marge. Une valeur mal authoree ne doit jamais transformer un obstacle en simple surface.");
        }

        [Test]
        public void OnlyAnEntryWhoseEveryContactIsASurfaceContactSkipsTheCollisionEvent()
        {
            var source = File.ReadAllText(VehicleDriverSourcePath);

            var enter = ExtractMethod(source, "private void OnCollisionEnter(Collision collision)");
            var surfaceCheck = enter.IndexOf("IsSurfaceOnlyCollision(collision)", StringComparison.Ordinal);
            var raiseEvent = enter.IndexOf("VehicleCollided?.Invoke(impactSpeed)", StringComparison.Ordinal);

            Assert.That(surfaceCheck, Is.GreaterThanOrEqualTo(0),
                "OnCollisionEnter doit consulter le predicat de contact de surface.");
            Assert.That(raiseEvent, Is.GreaterThan(surfaceCheck),
                "L'evenement -- donc sa RPC, donc les degats vehicule ET occupants -- se leve APRES la garde, jamais avant.");

            var predicate = CodeOnly(ExtractMethod(source, "private bool IsSurfaceOnlyCollision(Collision collision)"));

            Assert.That(predicate, Does.Contain("collision.contactCount"),
                "Le 'tous' porte sur l'entree de collision entiere : c'est lui qui preserve les vrais chocs, "
                + "puisqu'une paroi de tunnel produit aussi des points a hauteur de carrosserie.");
            Assert.That(predicate, Does.Contain("IsSurfaceContact("),
                "Chaque point de contact est juge par le predicat pur, jamais par un seuil de vitesse.");
            Assert.That(predicate, Does.Contain("return false;"),
                "Un seul contact qui n'est pas de surface requalifie l'entree en choc.");
            Assert.That(predicate, Does.Not.Contain("velocity"),
                "La regle de surface est une comparaison de hauteurs : un seuil de vitesse laisserait le bump "
                + "blesser des qu'on roule vite, ce qui est exactement le retour terrain a corriger.");
        }

        [Test]
        public void OccupantCollisionDamageIsGatedOnPositiveVehicleDamage()
        {
            // Retour terrain n° 2, premiere moitie : ApplyNetworkedCollisionDamage appliquait
            // PlayerCollisionDamage aux occupants AVANT de tester les degats vehicule, alors que sa
            // jumelle ApplySecondaryVehicleCollisionDamage portait deja la garde correcte. Toute
            // entree de collision -- donc toute arete de dalle a 0,2 m/s -- retirait des PV au joueur.
            const string occupantCharge = "service.ApplyCollisionDamage(clientId, NetworkedPlayerLifecycleService.PlayerCollisionDamage)";
            var source = File.ReadAllText(RunFlowSourcePath);

            var gate = CodeOnly(ExtractBracedBlock(source, "if (vehicleDamage > 0)"));
            Assert.That(gate, Does.Contain(occupantCharge),
                "La garde sur les degats VEHICULE precede l'application aux occupants : sous le seuil de 3 m/s, personne ne perd de PV.");
            Assert.That(gate, Does.Contain("subscribedVehicleState.ApplyDamage(vehicleDamage)"),
                "Le meme bloc applique les degats vehicule : les deux sont gates par la meme condition, jamais dissocies.");

            var secondary = CodeOnly(ExtractBracedBlock(source, "if (vehicleDamage <= 0)"));
            Assert.That(secondary, Does.Contain("return;"),
                "ApplySecondaryVehicleCollisionDamage sort immediatement quand la voiture ne perd aucun PV : "
                + "c'est le motif que la garde principale doit refleter.");

            Assert.That(
                NetworkedVehicleDriverController.ComputeCollisionDamage(NetworkedVehicleDriverController.MinCollisionDamageSpeed - 0.01f),
                Is.EqualTo(0),
                "Sous le seuil de degats vehicule, la condition de la garde est fausse : aucun occupant ne peut etre charge.");
        }

        [Test]
        public void EligibleTurnDrawOnlyDrawsAmongEligibleCandidatesAndReportsNoneEligibleAsMinusOne()
        {
            // Correctif post-livraison : le tirage de virage peut etre restreint a un sous-ensemble
            // eligible fourni par l'appelant. La fonction reste pure, statique, et ne connait pas la
            // notion de noeud parcouru -- elle sait seulement qu'un candidat donne est eligible ou non.
            var successors = new[] { 10, 20, 30 };
            var weights = new[] { 30f, 50f, 20f };

            // Un seul candidat eligible sur trois : c'est lui, quelle que soit la graine et quel que
            // soit le pas. Un successeur NON eligible n'est jamais rendu.
            for (ulong seed = 1; seed <= 200UL; seed++)
            {
                Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, weights, new[] { true, false, false }, seed, 1, out _), Is.EqualTo(10));
                Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, weights, new[] { false, true, false }, seed, 1, out _), Is.EqualTo(20));
                Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, weights, new[] { false, false, true }, seed, 1, out _), Is.EqualTo(30));
            }

            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, weights, new[] { false, false, false }, 1UL, 1, out _), Is.EqualTo(-1),
                "Aucun candidat eligible : -1, et c'est l'appelant qui reoriente -- jamais un ineligible.");
            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(Array.Empty<int>(), weights, Array.Empty<bool>(), 1UL, 1, out _), Is.EqualTo(-1),
                "Aucun successeur du tout : meme reponse qu'avec la restriction absente.");

            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, null, new[] { false, false, true }, 1UL, 1, out var singleEligible), Is.EqualTo(30));
            Assert.That(singleEligible, Is.False,
                "Un candidat unique ne fait l'objet d'aucune ponderation : il n'y a rien a signaler comme poids invalide.");
        }

        [Test]
        public void EligibleTurnDrawKeepsTheAuthoredRatiosAndTheUnrestrictedBehaviour()
        {
            var successors = new[] { 10, 20, 30 };
            var weights = new[] { 30f, 50f, 20f };

            // Tous eligibles : le tirage reste pondere par les ratios authores du noeud. La
            // restriction restreint, elle ne repondere pas.
            var counts = new Dictionary<int, int> { { 10, 0 }, { 20, 0 }, { 30, 0 } };
            for (ulong seed = 1; seed <= 4000UL; seed++)
            {
                counts[LaneGraphRouting.SelectWeightedSuccessor(successors, weights, new[] { true, true, true }, seed, 1, out _)]++;
            }

            Assert.That(counts[10] / 4000f, Is.EqualTo(0.30f).Within(0.03f), "ratio droite 30 (modele jtrrouter)");
            Assert.That(counts[20] / 4000f, Is.EqualTo(0.50f).Within(0.03f), "ratio tout droit 50");
            Assert.That(counts[30] / 4000f, Is.EqualTo(0.20f).Within(0.03f), "ratio gauche 20");

            // Meme graine, meme pas : une restriction qui n'exclut personne ne change rien, et son
            // absence (null) non plus. La surcharge historique reste donc valable telle quelle.
            var unrestricted = LaneGraphRouting.SelectWeightedSuccessor(successors, weights, 4242UL, 7, out _);
            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, weights, new[] { true, true, true }, 4242UL, 7, out _), Is.EqualTo(unrestricted));
            Assert.That(LaneGraphRouting.SelectWeightedSuccessor(successors, weights, null, 4242UL, 7, out _), Is.EqualTo(unrestricted));

            // Masque plus court que la liste : les candidats sans bit sont eligibles -- une restriction
            // absente ne restreint rien. Seul le candidat explicitement exclu ne sort jamais.
            for (ulong seed = 1; seed <= 200UL; seed++)
            {
                var drawn = LaneGraphRouting.SelectWeightedSuccessor(successors, weights, new[] { false }, seed, 1, out _);
                Assert.That(drawn == 20 || drawn == 30, Is.True,
                    "Le seul candidat explicitement ineligible (10) ne doit jamais sortir, mais les deux autres restent tirables.");
            }
        }

        [Test]
        public void ARingWalkByTheDriverNeverRevisitsANodeAndLeavesByABranch()
        {
            // Retour terrain n° 3 : un vehicule IA tournait en boucle dans les giratoires. Le parcours
            // est devenu une marche auto-evitative -- le noeud d'insertion puis chaque noeud atteint
            // sont marques parcourus, et le tirage ne porte que sur les successeurs non parcourus.
            WithRoundaboutGraph((graph, instance) =>
            {
                var ringNodes = instance.GetComponentsInChildren<LaneNode>(true)
                    .Where(node => node.name.StartsWith("Ring_", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(ringNodes.Length, Is.EqualTo(11),
                    "onze noeuds d'anneau authores : trois points de sortie, trois insertions, cinq arcs");

                var merges = instance.GetComponentsInChildren<LaneNode>(true)
                    .Where(node => node.name.StartsWith("Ring_Merge_", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(merges.Length, Is.EqualTo(3), "une insertion par branche du giratoire");

                foreach (var merge in merges)
                {
                    var vehicleObject = new GameObject("Story510RingWalkVehicle");
                    spawned.Add(vehicleObject);
                    var controller = vehicleObject.AddComponent<NetworkedAIVehicleDriverController>();
                    controller.BindLaneGraph(graph);

                    var current = IndexOf(graph, merge);

                    // Reproduit ce que fait l'INSERTION (OnNetworkSpawn -> ResetRouteMemoryAt) : le
                    // noeud de naissance est marque parcouru. Sans ce marquage, le vehicule pourrait
                    // revenir sur son propre noeud d'entree et le test mesurerait un artefact de
                    // montage, pas la regle.
                    InvokeResetRouteMemoryAt(controller, current);

                    var visited = new List<int> { current };
                    for (var step = 0; step < graph.NodeCount; step++)
                    {
                        var next = InvokeResolveNextNode(controller, current);

                        Assert.That(visited.Contains(next), Is.False,
                            merge.name + " : le vehicule a revu le noeud " + next + " (suite " + string.Join("-", visited)
                            + "). Un parcours IA ne rejoue jamais un noeud de voie : c'est ce qui rend impossible le tour complet de giratoire.");

                        visited.Add(next);

                        if (graph.GetSuccessors(next).Count == 0)
                        {
                            break;
                        }

                        current = next;
                    }

                    Assert.That(graph.GetSuccessors(visited[visited.Count - 1]).Count, Is.EqualTo(0),
                        merge.name + " : le vehicule doit sortir de l'anneau par une branche (fin de voie), pas revenir a son noeud d'entree.");
                    Assert.That(visited.Count, Is.LessThanOrEqualTo(ringNodes.Length + 1),
                        merge.name + " : plus de noeuds d'anneau traverses qu'il n'en existe, donc un tour complet a ete fait.");
                }
            });
        }

        [Test]
        public void TheRestrictedRingWalkStaysSelfAvoidingWhereTheUnrestrictedDrawCompletesLaps()
        {
            WithRoundaboutGraph((graph, instance) =>
            {
                var merges = instance.GetComponentsInChildren<LaneNode>(true)
                    .Where(node => node.name.StartsWith("Ring_Merge_", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(merges.Length, Is.EqualTo(3));

                var laps = 0;
                foreach (var merge in merges)
                {
                    var entry = IndexOf(graph, merge);

                    for (ulong seed = 1; seed <= 200UL; seed++)
                    {
                        if (WalkRingWithoutRestriction(graph, entry, seed))
                        {
                            laps++;
                        }

                        var visited = WalkRingSelfAvoiding(graph, entry, seed, out var escapeHatch);

                        Assert.That(escapeHatch, Is.False,
                            merge.name + " graine " + seed + " : le tirage restreint a epuise ses candidats eligibles avant de sortir de l'anneau. "
                            + "Sur un anneau, la branche sortante n'est jamais deja parcourue quand on atteint son point de decision : "
                            + "si l'echappatoire gloutonne se declenche ici, c'est la geometrie ou les ratios authores qui ont change.");

                        Assert.That(new HashSet<int>(visited).Count, Is.EqualTo(visited.Count),
                            merge.name + " graine " + seed + " : noeud revu (suite " + string.Join("-", visited) + ").");

                        Assert.That(graph.GetSuccessors(visited[visited.Count - 1]).Count, Is.EqualTo(0),
                            merge.name + " graine " + seed + " : le vehicule doit sortir par une branche, jamais boucler.");
                    }
                }

                Assert.That(laps, Is.GreaterThan(0),
                    "Sans restriction d'eligibilite, les ratios authores du giratoire (continuer 40 % aux trois points de decision) "
                    + "laissent passer des tours complets : c'est exactement ce que la regle de non-bouclage retire. "
                    + "Zero tour sur 600 parcours tires veut dire que ce test ne mesure plus rien.");
            });
        }

        // ------------------------------------------------- invariants de code

        [Test]
        public void RouteWaypointsIsGone()
        {
            Assert.That(File.Exists("Assets/RoadRage/Features/Vehicles/RouteWaypoints.cs"), Is.False,
                "La boucle structurelle est supprimee : NextIndex ne pouvait jamais terminer une route.");
        }

        [Test]
        public void NoTrafficPopulationLivesOutsideTheAuthoredDef()
        {
            foreach (var path in new[] { SpawnerSourcePath, DriverSourcePath })
            {
                var source = CodeOnly(File.ReadAllText(path));
                Assert.That(Regex.IsMatch(source, @"[Pp]opulation\s*=\s*-?\d"), Is.False,
                    path + " : aucun effectif de trafic litteral. L'effectif cible ne vient que du TrafficSettingsDef (AD-32).");
            }

            var spawnerSource = CodeOnly(File.ReadAllText(SpawnerSourcePath));
            Assert.That(spawnerSource, Does.Contain("ResolveSessionTargetPopulation"),
                "Un point de branchement unique et nomme doit porter l'effectif de session (Story 5.16).");
            Assert.That(Occurrences(spawnerSource, "ResolveSessionTargetPopulation"), Is.EqualTo(2),
                "Exactement une definition et un appel : le branchement de la Story 5.16 doit rester unique.");
            Assert.That(spawnerSource, Does.Contain("settings.ClampTargetPopulation(settings.DefaultTargetPopulation)"),
                "Tant que la valeur de session n'existe pas, la valeur par defaut authoree du Def est la seule source.");

            Assert.That(File.ReadAllText(MvpRunScenePath), Does.Not.Contain("TargetPopulation"),
                "Aucun effectif n'est authore dans la scene : il vit uniquement dans le Def.");
        }

        [Test]
        public void NothingRemovesATrafficVehicleAnywhereButAnExitPortal()
        {
            var driverSource = CodeOnly(File.ReadAllText(DriverSourcePath));
            Assert.That(driverSource, Does.Not.Contain("Despawn("), "Le driver ne retire jamais son propre vehicule.");
            Assert.That(driverSource, Does.Not.Contain("Destroy("), "Le driver ne detruit jamais son propre vehicule.");
            Assert.That(driverSource, Does.Contain("reachedExitPortal = true"),
                "Le driver SIGNALE l'arrivee au portail de sortie ; c'est tout ce qu'il fait du retrait.");

            var spawnerSource = File.ReadAllText(SpawnerSourcePath);
            Assert.That(Occurrences(CodeOnly(spawnerSource), "Despawn("), Is.EqualTo(1),
                "Un seul chemin de despawn dans tout le trafic.");

            var release = ExtractMethod(spawnerSource, "private void ReleaseVehiclesAtExitPortals()");
            Assert.That(release, Does.Contain("HasReachedExitPortal"),
                "Le despawn est declenche par l'arrivee a un portail de sortie, jamais par un compteur, une distance ou un echec.");
            Assert.That(release, Does.Contain("Despawn()"),
                "L'unique despawn vit dans ce chemin et nulle part ailleurs.");

            foreach (var forbidden in new[] { "distanceToPlayer", "despawnRadius", "maxLifetime", "IsRolledOver", "voidHeight" })
            {
                Assert.That(spawnerSource, Does.Not.Contain(forbidden),
                    "Aucun motif de retrait autre que le portail de sortie (AD-34) : '" + forbidden + "' n'a rien a faire ici.");
            }
        }

        [Test]
        public void MissingTrafficSettingsInsertsNothingAndWarnsOnce()
        {
            var spawnerSource = File.ReadAllText(SpawnerSourcePath);
            var update = ExtractMethod(spawnerSource, "private void FixedUpdate()");

            var guardIndex = update.IndexOf("if (settings == null)", StringComparison.Ordinal);
            var insertIndex = update.IndexOf("TryInsertAtPortal(", StringComparison.Ordinal);

            Assert.That(guardIndex, Is.GreaterThanOrEqualTo(0), "garde de Def absent attendue");
            Assert.That(insertIndex, Is.GreaterThan(guardIndex),
                "Def de trafic absent : la garde precede toute insertion, donc aucun vehicule n'entre.");
            Assert.That(spawnerSource, Does.Contain("WarnOnce(ref warnedMissingSettings"),
                "Un seul avertissement est emis, pas un par pas de simulation.");
        }

        [Test]
        public void ACrowdedPortalQueuesTheInsertionInsteadOfAbandoningIt()
        {
            var spawnerSource = File.ReadAllText(SpawnerSourcePath);
            var update = ExtractMethod(spawnerSource, "private void FixedUpdate()");

            Assert.That(Occurrences(CodeOnly(update), "deficit--"), Is.EqualTo(1),
                "Le deficit ne se decremente qu'a un seul endroit : sinon un portail encombre consommerait une place de la file.");
            Assert.That(CodeOnly(update), Does.Contain("if (TryInsertAtPortal(portalIndex))"),
                "Le deficit ne baisse que sur une insertion REUSSIE.");

            var clearanceGuard = CodeOnly(update).IndexOf("if (!IsPortalClear(", StringComparison.Ordinal);
            var insertion = CodeOnly(update).IndexOf("if (TryInsertAtPortal(portalIndex))", StringComparison.Ordinal);
            Assert.That(clearanceGuard, Is.GreaterThanOrEqualTo(0), "garde d'encombrement de portail attendue");
            Assert.That(clearanceGuard, Is.LessThan(insertion),
                "Portail encombre : on passe au suivant sans inserer. Le deficit reste, donc le pas suivant retente -- rien n'est abandonne.");

            Assert.That(CodeOnly(update), Does.Contain("continue;"),
                "Un portail encombre est saute, pas vide de sa file.");

            // La file est bornee par l'effectif cible par construction : elle EST le deficit.
            Assert.That(CodeOnly(update), Does.Contain("ResolveSessionTargetPopulation(settings) - liveVehicles.Count"),
                "La file d'attente est le deficit lui-meme, donc bornee par l'effectif cible.");
        }

        [Test]
        public void ADeadEndRedirectsTowardTheNearestExitInsteadOfEndingTheRoute()
        {
            var driverSource = File.ReadAllText(DriverSourcePath);
            var redirect = ExtractMethod(driverSource, "private int ResolveRedirectToNearestExit(int currentIndex, IReadOnlyList<int> candidates)");

            Assert.That(CodeOnly(redirect), Does.Contain("if (candidates.Count == 0)"),
                "Cul-de-sac : le cas doit etre traite explicitement, pas tomber dans le tirage.");
            Assert.That(CodeOnly(redirect), Does.Contain("return exitIndex;"),
                "Cul-de-sac : le vehicule est reoriente vers le portail de sortie le plus proche, jamais laisse errer ni retire.");
            Assert.That(redirect, Does.Contain("warnedDeadEnd"),
                "Un cul-de-sac est un defaut d'authoring : il doit s'entendre, une fois.");

            var resolve = ExtractMethod(driverSource, "private int ResolveNextNode(int currentIndex)");
            Assert.That(CodeOnly(resolve), Does.Contain("if (candidates.Count > 0 && !budgetExceeded)"),
                "Cul-de-sac ET budget depasse partagent la meme sortie de secours : la reorientation vers la sortie la plus proche.");
            Assert.That(CodeOnly(resolve), Does.Contain("return ResolveRedirectToNearestExit(currentIndex, candidates);"));
        }

        [Test]
        public void TrafficSettingsDefAssetIsAuthoredAndValid()
        {
            var def = AssetDatabase.LoadAssetAtPath<TrafficSettingsDef>(TrafficDefPath);
            Assert.That(def, Is.Not.Null, TrafficDefPath + " attendu");
            Assert.That(def.TryValidate(out var error), Is.True, error);
            Assert.That(def.RawId, Is.EqualTo("traffic_default"));
            Assert.That(def.ClampTargetPopulation(int.MaxValue), Is.EqualTo(def.MaxTargetPopulation), "Toute valeur de session sera ramenee dans [min, max].");
            Assert.That(def.ClampTargetPopulation(int.MinValue), Is.EqualTo(def.MinTargetPopulation));
        }

        // ------------------------------------------------- authoring des modules greybox

        [Test]
        public void LaneNodesLiveUnderTheGameplayRootNeverUnderTheReplaceableVisualChild()
        {
            foreach (var path in ModulePrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path + " attendu");

                var nodes = prefab.GetComponentsInChildren<LaneNode>(true);
                Assert.That(nodes.Length, Is.GreaterThan(0), path + " doit porter ses propres LaneNode");

                foreach (var node in nodes)
                {
                    for (var parent = node.transform; parent != null; parent = parent.parent)
                    {
                        Assert.That(parent.name.StartsWith("Visual_", StringComparison.Ordinal), Is.False,
                            path + " : " + node.name + " vit sous " + parent.name
                            + ". Les noeuds de voie restent sous la racine gameplay (AD-27), sinon le passage a l'art final deplace le graphe.");
                    }
                }

                foreach (var collider in prefab.GetComponentsInChildren<Collider>(true))
                {
                    for (var parent = collider.transform; parent != null; parent = parent.parent)
                    {
                        Assert.That(parent.name.StartsWith("Visual_", StringComparison.Ordinal), Is.False,
                            path + " : le collider " + collider.name + " vit sous " + parent.name
                            + ". Remplacer l'enfant Visual_* doit laisser les colliders intacts.");
                    }
                }
            }
        }

        [Test]
        public void NoModuleColliderRisesAboveTheDrivingPlane()
        {
            // Depuis la Story 5.9 le vehicule est pilote en ecrivant linearVelocity, sans roue ni
            // suspension : une face verticale, meme de 15 cm, ne se gravit pas -- PhysX resout
            // l'interpenetration par une impulsion qui devient de la vitesse verticale, et le vehicule
            // s'envole ou se colle. Le plan de roulage est donc y = 0 EXACTEMENT, et rien d'un module
            // ne depasse : trottoirs et ilots de giratoire se distinguent par leur materiau, pas par
            // leur hauteur. Seules les parois de tunnel ont le droit de s'elever, ce sont des
            // obstacles voulus. L'interdiction des trottoirs reste portee par le graphe de voies et le
            // masque d'aire NavMesh -- la physique n'a jamais ete le mecanisme d'interdiction.
            string[] allowedToRise = { "Col_Wall_Left", "Col_Wall_Right", "Col_Roof", "Col_Backstop" };

            foreach (var path in ModulePrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path + " attendu");

                foreach (var collider in prefab.GetComponentsInChildren<Collider>(true))
                {
                    if (allowedToRise.Contains(collider.name))
                    {
                        continue;
                    }

                    Assert.That(collider.bounds.max.y, Is.LessThanOrEqualTo(0.0001f),
                        path + " : le collider " + collider.name + " culmine a y = "
                        + collider.bounds.max.y.ToString("F4")
                        + ". Toute marche au-dessus du plan de roulage renvoie le vehicule en l'air (il est pilote en vitesse, sans suspension).");
                }
            }
        }

        [Test]
        public void TheGroundPlaneSitsBelowTheDrivingPlaneSoRoadsDoNotZFight()
        {
            WithMvpRun(scene =>
            {
                var runRoot = scene.GetRootGameObjects().First(candidate => candidate.name == "RunRoot");
                var ground = runRoot.transform.Find("GreyboxMap/Greybox_GroundPlane");
                Assert.That(ground, Is.Not.Null, "plan de sol greybox attendu");

                var top = ground.GetComponent<Renderer>().bounds.max.y;
                Assert.That(top, Is.LessThan(-0.001f),
                    "Le plan de sol doit passer SOUS le plan de roulage : coplanaire avec les chaussees, il les fait clignoter (z-fighting) sur toute l'emprise.");
                Assert.That(top, Is.GreaterThan(-0.2f),
                    "...mais rester assez haut pour que la levre au bord du district reste franchissable.");
            });
        }

        [Test]
        public void SidewalksCarryTheDedicatedAreaAndStayOutOfTheVehicleBake()
        {
            var areas = File.ReadAllText(NavMeshAreasPath);
            Assert.That(areas, Does.Contain("name: Sidewalk"), "L'aire NavMesh dediee aux trottoirs doit exister.");
            Assert.That(areas, Does.Contain("- Vehicle"), "Le type d'agent vehicule doit exister.");
            Assert.That(NamedAreaIndex(areas, "Sidewalk"), Is.EqualTo(SidewalkAreaIndex),
                "L'index de l'aire Sidewalk est celui reference par les modules greybox : le deplacer casserait silencieusement l'exclusion.");

            foreach (var path in ModulePrefabPaths)
            {
                var text = File.ReadAllText(path);
                var modifiers = Occurrences(text, "Unity.AI.Navigation.NavMeshModifier");
                Assert.That(modifiers, Is.GreaterThan(0), path + " doit porter au moins une surface non carrossable");

                Assert.That(Occurrences(text, "m_Area: " + SidewalkAreaIndex), Is.EqualTo(modifiers),
                    path + " : toute surface non carrossable porte l'aire Sidewalk dediee -- pas une aire Walkable rendue couteuse.");

                // Un trottoir borde de chaussee n'est pas bake du tout pour l'agent vehicule.
                var sidewalkColliders = Occurrences(text, "m_Name: Col_Sidewalk");
                Assert.That(Occurrences(text, "m_IgnoreFromBuild: 1"), Is.EqualTo(sidewalkColliders),
                    path + " : un trottoir borde de chaussee est hors du bake de l'agent vehicule, et rien d'autre ne l'est.");
            }

            // L'ilot central d'un rond-point repose SUR la chaussee : l'exclure du bake laisserait le
            // disque navigable dessous. C'est le seul cas ou l'aire dediee travaille seule, et c'est
            // exactement ce que le masque d'aire de l'agent vehicule doit refuser.
            var roundabout = File.ReadAllText(RoundaboutPrefabPath);
            Assert.That(roundabout, Does.Contain("m_Name: Col_Island"), "le rond-point doit porter un ilot central");
            Assert.That(Occurrences(roundabout, "m_IgnoreFromBuild: 1"), Is.EqualTo(0),
                "L'ilot reste dans le bake : il doit exister comme surface d'aire Sidewalk, hors du masque vehicule.");
            Assert.That(Occurrences(roundabout, "m_Name: Col_Sidewalk"), Is.EqualTo(0),
                "Le rond-point n'a pas de trottoir borde de chaussee : son ilot n'en est pas un.");
        }

        [Test]
        public void MvpRunBakesTheDistrictForTheVehicleAgentWithoutAnyNavMeshAgent()
        {
            var sceneText = File.ReadAllText(MvpRunScenePath);
            Assert.That(sceneText, Does.Contain("m_AgentTypeID: " + VehicleAgentTypeId()),
                "Le NavMeshSurface du district doit baker pour le type d'agent vehicule, pas pour l'Humanoid par defaut.");
            Assert.That(Regex.IsMatch(sceneText, @"m_NavMeshData: \{fileID: \d+, guid:"), Is.True,
                "Le district doit etre bake : la donnee de navigation est authoree, pas laissee a construire.");
            Assert.That(sceneText, Does.Not.Contain("!u!195 &"),
                "AD-33 : aucun NavMeshAgent (classe 195) dans la scene -- le NavMesh de cette story est de la donnee authoree, jamais un fournisseur de chemin actif.");
        }

        // ------------------------------------------------- outils

        private static int NamedAreaIndex(string areasText, string name)
        {
            var matches = Regex.Matches(areasText, @"^  - name: (.*)$", RegexOptions.Multiline);
            for (var i = 0; i < matches.Count; i++)
            {
                if (matches[i].Groups[1].Value.Trim() == name)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Identifiant du type d'agent nomme "Vehicle", lu dans l'ordre des m_SettingNames.</summary>
        private static string VehicleAgentTypeId()
        {
            var areas = File.ReadAllText(NavMeshAreasPath);
            var ids = Regex.Matches(areas, @"agentTypeID: (-?\d+)");

            var namesStart = areas.IndexOf("m_SettingNames:", StringComparison.Ordinal);
            Assert.That(namesStart, Is.GreaterThanOrEqualTo(0), "m_SettingNames attendu dans " + NavMeshAreasPath);

            var names = Regex.Matches(areas.Substring(namesStart), @"^  - (.+)$", RegexOptions.Multiline);
            for (var i = 0; i < names.Count && i < ids.Count; i++)
            {
                if (names[i].Groups[1].Value.Trim() == "Vehicle")
                {
                    return ids[i].Groups[1].Value;
                }
            }

            return "0";
        }

        /// <summary>Distance du centre du module au segment [a, b] : la trajectoire reelle, le vehicule allant tout droit d'un noeud au suivant.</summary>
        private static float DistanceFromModuleCentreToSegment(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            var ab = b - a;
            var t = ab.sqrMagnitude <= 0.0001f ? 0f : Mathf.Clamp01(Vector3.Dot(-a, ab) / ab.sqrMagnitude);
            return (a + (ab * t)).magnitude;
        }

        private static Vector3 Flat(Vector3 value)
        {
            value.y = 0f;
            return value;
        }

        private static bool ReachesAnExit(LaneGraph graph, int from)
        {
            var seen = new HashSet<int> { from };
            var pending = new Queue<int>();
            pending.Enqueue(from);

            while (pending.Count > 0)
            {
                var node = pending.Dequeue();
                if (node != from && graph.IsExitPortal(node))
                {
                    return true;
                }

                foreach (var successor in graph.GetSuccessors(node))
                {
                    if (seen.Add(successor))
                    {
                        pending.Enqueue(successor);
                    }
                }
            }

            return false;
        }

        /// <summary>Parcours tire pour un NetworkObjectId donne, rendu sous forme de suite d'index -- exactement ce que le driver resoudra.</summary>
        private static string WalkRoute(LaneGraph graph, int entry, ulong networkObjectId, out bool endedAtExit)
        {
            var node = entry;
            var edges = 0;
            var visited = new List<int> { node };
            var budget = Mathf.CeilToInt(2f * graph.NodeCount);

            while (edges <= budget)
            {
                if (edges > 0 && graph.IsExitPortal(node))
                {
                    break;
                }

                var successors = graph.GetSuccessors(node);
                if (successors.Count == 0)
                {
                    break;
                }

                edges++;
                node = LaneGraphRouting.SelectWeightedSuccessor(successors, graph.GetTurnWeights(node), networkObjectId, edges, out _);
                visited.Add(node);
            }

            endedAtExit = graph.IsExitPortal(node);
            return string.Join("-", visited);
        }

        // ------------------------------------------------- ANO-5.10-02 : orbite de poursuite

        /// <summary>
        /// Le coeur de l'anomalie, en une geometrie : un repere DEJA DEPASSE et tombe dans le cercle
        /// de braquage est hors d'atteinte -- c'est exactement l'etat stable mesure sur les vehicules
        /// en orbite (cible par le travers, a une distance egale au rayon de braquage).
        /// </summary>
        [Test]
        public void PassedWaypointInsideTurnCircleIsReportedUnreachable()
        {
            // 8 m/s sous 90 deg/s : rayon de braquage 5,09 m. Repere par le travers a 5 m.
            Assert.That(
                LaneGraphRouting.HasPassedUnreachableWaypoint(
                    Vector3.zero, Vector3.forward, new Vector3(5f, 0f, -0.5f), 8f, 90f),
                Is.True,
                "Repere depasse et interieur au cercle de braquage : la poursuite pure tournerait autour indefiniment.");
        }

        /// <summary>
        /// La garde qui empeche le predicat de couper les virages : tant que le repere est DEVANT, la
        /// poursuite normale s'en occupe. Sans cette garde, un vehicule sur un giratoire franchirait
        /// ses noeuds d'anneau en avance et couperait le trace.
        /// </summary>
        [Test]
        public void WaypointStillAheadIsNeverReportedUnreachable()
        {
            Assert.That(
                LaneGraphRouting.HasPassedUnreachableWaypoint(
                    Vector3.zero, Vector3.forward, new Vector3(1.7f, 0f, 3f), 8f, 90f),
                Is.False,
                "Noeud suivant d'un anneau de giratoire (3,4 m, 29 deg) : encore devant, donc jamais court-circuite.");
        }

        /// <summary>
        /// Le predicat ne porte jamais loin : un point interieur au cercle est a au plus 2R, donc un
        /// vehicule retourne a l'autre bout du district revient chercher son repere comme avant.
        /// </summary>
        [Test]
        public void DistantWaypointBehindIsNeverReportedUnreachable()
        {
            Assert.That(
                LaneGraphRouting.HasPassedUnreachableWaypoint(
                    Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -40f), 8f, 90f),
                Is.False,
                "Repere lointain derriere : atteignable en faisant demi-tour, le parcours ne doit pas sauter de noeud.");
        }

        /// <summary>
        /// Le rayon vient de la vitesse : un vehicule lent braque assez court pour revenir sur son
        /// repere, donc le predicat s'eteint de lui-meme. Aucun seuil supplementaire a authorer.
        /// </summary>
        [Test]
        public void SlowVehicleCanStillReachAPassedWaypoint()
        {
            Assert.That(
                LaneGraphRouting.HasPassedUnreachableWaypoint(
                    Vector3.zero, Vector3.forward, new Vector3(5f, 0f, -0.5f), 1f, 90f),
                Is.False,
                "A 1 m/s le rayon de braquage tombe a 0,64 m : le repere est hors du cercle, donc atteignable.");
        }

        /// <summary>Entrees degenerees : jamais de division par zero, jamais de franchissement invente.</summary>
        [Test]
        public void UnreachableWaypointPredicateIsInertOnDegenerateInputs()
        {
            Assert.That(LaneGraphRouting.HasPassedUnreachableWaypoint(Vector3.zero, Vector3.zero, new Vector3(5f, 0f, -0.5f), 8f, 90f), Is.False);
            Assert.That(LaneGraphRouting.HasPassedUnreachableWaypoint(Vector3.zero, Vector3.forward, Vector3.zero, 8f, 90f), Is.False);
            Assert.That(LaneGraphRouting.HasPassedUnreachableWaypoint(Vector3.zero, Vector3.forward, new Vector3(5f, 0f, -0.5f), 0f, 90f), Is.False);
            Assert.That(LaneGraphRouting.HasPassedUnreachableWaypoint(Vector3.zero, Vector3.forward, new Vector3(5f, 0f, -0.5f), 8f, 0f), Is.False);
            Assert.That(LaneGraphRouting.HasPassedUnreachableWaypoint(Vector3.zero, Vector3.forward, new Vector3(5f, 0f, -0.5f), float.NaN, 90f), Is.False);
        }

        /// <summary>
        /// La garde de non-regression de l'anomalie, sur le district REELLEMENT authore : on rejoue
        /// la cinematique exacte du driver (meme rayon d'arrivee, meme vitesse de lacet, meme pas de
        /// physique, meme resolution de noeud) depuis chaque portail d'entree, en bousculant le
        /// vehicule une fois en cours de route -- ce que fait un contact avec un autre vehicule ou le
        /// joueur. Chaque vehicule doit finir par atteindre un portail de sortie.
        ///
        /// Le meme balayage SANS le predicat de depassement laissait 46 % des vehicules en orbite
        /// permanente autour d'un noeud d'anneau de giratoire : c'est la garde qui echoue si le
        /// correctif est retire.
        /// </summary>
        [Test]
        public void PerturbedVehiclesAlwaysReachAnExitPortalInTheAuthoredDistrict()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                graph.Rebuild();

                var orbits = new List<string>();
                var runs = 0;

                foreach (var entry in graph.EntryPortals)
                {
                    foreach (var lateral in new[] { 0f, 3f, -4.5f })
                    {
                        foreach (var heading in new[] { 90f, 180f, -135f })
                        {
                            foreach (var bumpStep in new[] { 80, 500, 1400 })
                            {
                                runs++;
                                if (!ReplayReachesExit(graph, entry, lateral, heading, bumpStep))
                                {
                                    orbits.Add("portail " + entry + " lateral=" + lateral + " cap=" + heading + " choc=" + bumpStep);
                                }
                            }
                        }
                    }
                }

                Assert.That(orbits, Is.Empty,
                    "ANO-5.10-02 : " + orbits.Count + "/" + runs
                    + " vehicules bouscules restent en orbite au lieu d'atteindre un portail de sortie -- "
                    + string.Join(" ; ", orbits.Take(5)));
            });
        }

        /// <summary>
        /// Rejoue la cinematique du driver IA sur le graphe reel. Volontairement une copie des seules
        /// equations de <c>NetworkedAIVehicleDriverController</c> qui decident du franchissement de
        /// noeud (arrivee, depassement irrattrapable, poursuite, lacet borne) : la garde doit tomber
        /// si l'une d'elles change, pas si le reste du controleur bouge.
        /// </summary>
        private static bool ReplayReachesExit(LaneGraph graph, int entry, float lateral, float headingOffset, int bumpStep)
        {
            const float ArrivalRadius = 3f;          // Greybox_AIVehicle
            const float SteerFullLockDegrees = 45f;  // Greybox_AIVehicle
            const float SteerDegreesPerSecond = 90f; // Greybox_AIVehicle
            const float Speed = 8f;                  // DriverProfileDef_Default.desiredSpeed
            const float FixedDeltaTime = 0.02f;

            var traversed = new bool[graph.NodeCount];
            var node = entry;
            traversed[node] = true;

            var traversedEdges = 0;
            var departed = false;
            var position = graph.GetNodePosition(entry);
            var yaw = graph.GetNodeRotation(entry).eulerAngles.y;
            var target = graph.GetNodePosition(node);
            var lastNodeChangeStep = 0;

            for (var step = 0; step < 12000; step++)
            {
                if (step == bumpStep)
                {
                    position += Quaternion.Euler(0f, yaw, 0f) * Vector3.right * lateral;
                    yaw += headingOffset;
                }

                var forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                var toWaypoint = target - position;
                toWaypoint.y = 0f;

                var arrived = toWaypoint.sqrMagnitude <= ArrivalRadius * ArrivalRadius;
                if (arrived || LaneGraphRouting.HasPassedUnreachableWaypoint(
                        position, forward, target, Speed, SteerDegreesPerSecond))
                {
                    if (graph.IsExitPortal(node) && departed)
                    {
                        return true;
                    }

                    node = ReplayResolveNextNode(graph, node, traversed, ref traversedEdges, ref departed, position);
                    target = graph.GetNodePosition(node);
                    lastNodeChangeStep = step;

                    toWaypoint = target - position;
                    toWaypoint.y = 0f;
                    arrived = toWaypoint.sqrMagnitude <= ArrivalRadius * ArrivalRadius;
                }

                // Aucun noeud franchi depuis 18 s : le vehicule ne progresse plus, c'est l'orbite.
                if (step - lastNodeChangeStep > 900)
                {
                    return false;
                }

                if (arrived)
                {
                    continue;
                }

                var signedAngle = Vector3.SignedAngle(forward, toWaypoint.normalized, Vector3.up);
                yaw += Mathf.Clamp(signedAngle / SteerFullLockDegrees, -1f, 1f) * SteerDegreesPerSecond * FixedDeltaTime;
                position += Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * (Speed * FixedDeltaTime);
            }

            return false;
        }

        /// <summary>Tirage auto-evitant puis reorientation gloutonne : la resolution de noeud du driver.</summary>
        private static int ReplayResolveNextNode(
            LaneGraph graph, int current, bool[] traversed, ref int traversedEdges, ref bool departed, Vector3 position)
        {
            var candidates = graph.GetSuccessors(current);
            traversedEdges++;
            departed = true;

            var budgetExceeded = LaneGraphRouting.IsEdgeBudgetExceeded(
                traversedEdges, graph.NodeCount, graph.TrafficSettings.EdgeBudgetFactor);

            if (candidates.Count > 0 && !budgetExceeded)
            {
                var eligible = new List<bool>();
                for (var i = 0; i < candidates.Count; i++)
                {
                    eligible.Add(!traversed[candidates[i]]);
                }

                var drawn = LaneGraphRouting.SelectWeightedSuccessor(
                    candidates, graph.GetTurnWeights(current), eligible, 7UL, traversedEdges, out _);
                if (drawn >= 0)
                {
                    traversed[drawn] = true;
                    return drawn;
                }
            }

            var exitIndex = graph.NearestExitNodeIndex(position);
            if (exitIndex < 0)
            {
                return current;
            }

            if (candidates.Count == 0)
            {
                return exitIndex;
            }

            var positions = new List<Vector3>();
            for (var i = 0; i < candidates.Count; i++)
            {
                positions.Add(graph.GetNodePosition(candidates[i]));
            }

            return LaneGraphRouting.SelectSuccessorTowardTarget(candidates, positions, graph.GetNodePosition(exitIndex));
        }

        private static int CountModules(Transform root, string prefabPath)
        {
            var count = 0;
            foreach (Transform child in root)
            {
                if (IsInstanceOf(child, prefabPath))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Identite d'un module par son prefab source : stable au renommage de l'instance.</summary>
        private static bool IsInstanceOf(Transform instance, string prefabPath)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(instance.gameObject);
            return source != null && AssetDatabase.GetAssetPath(source) == prefabPath;
        }

        private static LaneGraph ResolveGraph(Scene scene)
        {
            var runRoot = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == "RunRoot");
            Assert.That(runRoot, Is.Not.Null, "RunRoot attendu dans MVP_Run");

            var graph = runRoot.GetComponentInChildren<LaneGraph>(true);
            Assert.That(graph, Is.Not.Null, "LaneGraph attendu sous RunRoot");
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
                if (!wasOpen)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        // ------------------------------------------------- doubles sans scene

        private TrafficSettingsDef NewSettings(float connectorJoinDistance)
        {
            var settings = ScriptableObject.CreateInstance<TrafficSettingsDef>();
            spawned.Add(settings);

            var serialized = new SerializedObject(settings);
            serialized.FindProperty("id").stringValue = "traffic_test";
            serialized.FindProperty("connectorJoinDistance").floatValue = connectorJoinDistance;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return settings;
        }

        private LaneGraph NewGraph(TrafficSettingsDef settings)
        {
            var root = new GameObject("Story510LaneGraph");
            spawned.Add(root);

            var graph = root.AddComponent<LaneGraph>();
            var serialized = new SerializedObject(graph);
            serialized.FindProperty("trafficSettings").objectReferenceValue = settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return graph;
        }

        private static LaneNode NewNode(LaneGraph graph, string name, Vector3 position, float yaw, LaneNodeRole role)
        {
            var go = new GameObject(name);
            go.transform.SetParent(graph.transform, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            var node = go.AddComponent<LaneNode>();
            var serialized = new SerializedObject(node);
            serialized.FindProperty("role").enumValueIndex = (int)role;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return node;
        }

        private static void Wire(LaneNode node, LaneNode[] successors, float[] weights)
        {
            var serialized = new SerializedObject(node);
            var successorArray = serialized.FindProperty("successors");
            var weightArray = serialized.FindProperty("turnWeights");
            successorArray.arraySize = successors.Length;
            weightArray.arraySize = weights.Length;

            for (var i = 0; i < successors.Length; i++)
            {
                successorArray.GetArrayElementAtIndex(i).objectReferenceValue = successors[i];
            }

            for (var i = 0; i < weights.Length; i++)
            {
                weightArray.GetArrayElementAtIndex(i).floatValue = weights[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetExitReusesEntry(LaneNode node, bool value)
        {
            var serialized = new SerializedObject(node);
            serialized.FindProperty("exitReusesEntry").boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static int IndexOf(LaneGraph graph, LaneNode node)
        {
            return graph.NearestNodeIndex(node.transform.position);
        }

        /// <summary>Invoque le point de branchement prive du parcours -- aucun Netcode requis : NetworkObjectId vaut 0 par defaut sur un double non spawn, une graine valide comme une autre.</summary>
        private static int InvokeResolveNextNode(NetworkedAIVehicleDriverController controller, int currentIndex)
        {
            var method = typeof(NetworkedAIVehicleDriverController).GetMethod("ResolveNextNode", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "method not found: ResolveNextNode");
            return (int)method.Invoke(controller, new object[] { currentIndex });
        }

        /// <summary>
        /// Invoque la remise a zero de la memoire de parcours sur un noeud donne : c'est ce que
        /// l'insertion fait (OnNetworkSpawn), et le seul autre appelant est la recuperation sur place.
        /// Le montage sans Netcode doit la reproduire, sinon le noeud d'entree du vehicule ne serait
        /// pas marque comme parcouru et le test mesurerait un artefact de montage.
        /// </summary>
        private static void InvokeResetRouteMemoryAt(NetworkedAIVehicleDriverController controller, int nodeIndex)
        {
            var method = typeof(NetworkedAIVehicleDriverController).GetMethod("ResetRouteMemoryAt", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "method not found: ResetRouteMemoryAt");
            method.Invoke(controller, new object[] { nodeIndex });
        }

        /// <summary>
        /// Monte un graphe sur le VRAI prefab de giratoire : la topologie et les ratios authores sont
        /// ceux du district, pas une maquette de test -- c'est ce qui rend la preuve de non-bouclage
        /// pertinente. Les connecteurs de branche restent orphelins (aucun module voisin dans ce
        /// montage), ce qui est sans effet sur le parcours : il s'arrete a la fin de la branche.
        /// </summary>
        private void WithRoundaboutGraph(Action<LaneGraph, GameObject> body)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RoundaboutPrefabPath);
            Assert.That(prefab, Is.Not.Null, RoundaboutPrefabPath + " attendu");

            var graph = NewGraph(NewSettings(connectorJoinDistance: 0.75f));
            var instance = UnityEngine.Object.Instantiate<GameObject>(prefab);
            spawned.Add(instance);
            instance.transform.SetParent(graph.transform, false);
            graph.Rebuild();

            body(graph, instance);
        }

        /// <summary>
        /// Marche auto-evitante d'un vehicule entrant dans un giratoire : le noeud d'entree est marque
        /// parcouru (comme a l'insertion), puis le tirage de chaque point de decision est restreint par
        /// le meme masque que <c>BuildEligibleSuccessorMask</c> -- successeurs non parcourus seulement.
        /// Rend la suite des noeuds parcourus ; <paramref name="escapeHatch"/> signale que plus aucun
        /// successeur n'etait eligible, cas ou le driver, lui, reoriente gloutonnement vers la sortie.
        /// </summary>
        private static List<int> WalkRingSelfAvoiding(LaneGraph graph, int entry, ulong seed, out bool escapeHatch)
        {
            escapeHatch = false;
            var visited = new List<int> { entry };
            var node = entry;

            for (var step = 1; step <= graph.NodeCount * 2; step++)
            {
                var successors = graph.GetSuccessors(node);
                if (successors.Count == 0)
                {
                    return visited;
                }

                var eligible = new List<bool>();
                for (var i = 0; i < successors.Count; i++)
                {
                    eligible.Add(!visited.Contains(successors[i]));
                }

                var drawn = LaneGraphRouting.SelectWeightedSuccessor(successors, graph.GetTurnWeights(node), eligible, seed, step, out _);
                if (drawn < 0)
                {
                    escapeHatch = true;
                    return visited;
                }

                visited.Add(drawn);
                node = drawn;
            }

            return visited;
        }

        /// <summary>
        /// Meme parcours SANS restriction d'eligibilite : c'est le tirage pondere de la livraison
        /// initiale. Rend vrai si le vehicule revient sur un noeud deja parcouru, donc s'il boucle.
        /// </summary>
        private static bool WalkRingWithoutRestriction(LaneGraph graph, int entry, ulong seed)
        {
            var visited = new HashSet<int> { entry };
            var node = entry;

            for (var step = 1; step <= graph.NodeCount * 2; step++)
            {
                var successors = graph.GetSuccessors(node);
                if (successors.Count == 0)
                {
                    return false;
                }

                node = LaneGraphRouting.SelectWeightedSuccessor(successors, graph.GetTurnWeights(node), seed, step, out _);
                if (!visited.Add(node))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Source privee de ses commentaires de ligne : les gardes portent sur du code, jamais sur de la prose (patron Story 5.6/5.7).</summary>
        private static string CodeOnly(string source)
        {
            return string.Join("\n", source.Split('\n').Where(line => !line.TrimStart().StartsWith("//")));
        }

        /// <summary>
        /// Bloc d'accolades qui suit un marqueur (signature de methode, garde, boucle). Sert a verifier
        /// ce qui vit DANS une garde donnee, et pas seulement autour d'elle.
        /// </summary>
        private static string ExtractBracedBlock(string source, string marker)
        {
            var start = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "marqueur introuvable : " + marker);

            var depth = 0;
            var opened = false;
            for (var i = start; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                    opened = true;
                }
                else if (source[i] == '}')
                {
                    depth--;
                    if (opened && depth == 0)
                    {
                        return source.Substring(start, i - start + 1);
                    }
                }
            }

            return source.Substring(start);
        }

        private static string ExtractMethod(string source, string signature)
        {
            return ExtractBracedBlock(source, signature);
        }

        private static int Occurrences(string source, string token)
        {
            var count = 0;
            var index = 0;
            while ((index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }
    }
}
