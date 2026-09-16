using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.MainMenu;
using RoadRage.App.Run;
using RoadRage.App.Services;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.10 : preuve d'integration du trafic portail-a-portail dans MVP_Run (exigence
    /// AGENTS.md). Les gardes EditMode decrivent le contrat du graphe et du code ; ce fixture
    /// constate sur un pair hote reel ce qu'aucune d'elles ne peut prouver -- que sur la duree d'une
    /// session, TOUT vehicule apparu est apparu a un portail d'entree, que TOUT vehicule disparu a
    /// disparu a un portail de sortie, et que l'effectif tend vers la cible authoree.
    ///
    /// Correctif post-livraison du 2026-09-16 : le fixture porte aussi la preuve de NON-BOUCLAGE,
    /// observee sur le district reel. Sont echantillonnes la suite de <c>WaypointIndex</c> de chaque
    /// vehicule (qui doit ne presenter aucun noeud revu) et son temps de sejour dans l'emprise de
    /// chaque giratoire (qui doit rester sous un tour).
    ///
    /// Meme demarrage que les Stories 5.7 et 5.9 : bootstrap -> menu -> lobby -> Start Game. Une
    /// machine sans Steam P2P fonctionnel rend le test Inconclusif plutot que rouge.
    /// </summary>
    [Category("Story510")]
    public sealed class Story510RoutedTrafficPlayModeTests
    {
        /// <summary>
        /// Tolerance d'observation, en metres. Un vehicule est constate au moins une frame apres son
        /// insertion : il a deja quitte le noeud exact du portail. Large devant cette derive, etroite
        /// devant la distance entre deux portails (au moins 28 m dans le district authore).
        /// </summary>
        private const float PortalObservationRadius = 8f;

        /// <summary>Duree d'observation : un parcours portail-a-portail du district fait 160 a 900 m a ~8 m/s, selon les virages tires. La boucle sort des qu une sortie ET son remplacement sont constates.</summary>
        private const float ObservationSeconds = 90f;

        /// <summary>Prefixe des quatre instances de giratoire du district : c'est par lui que la preuve de non-bouclage retrouve l'emprise de chaque rond-point.</summary>
        private const string RoundaboutNamePrefix = "Roundabout_";

        /// <summary>Rayon de l'emprise d'un giratoire : 6 m d'axe d'anneau + 2 m de marge (cotes figees du module, note de verification).</summary>
        private const float RoundaboutFootprintRadius = 8f;

        /// <summary>
        /// Budget d'un tour, en secondes. Un tour d'anneau fait 2 x pi x 6 = 37,7 m, soit ~4,7 s a
        /// 8 m/s. 25 s laisse cinq fois la marge : un vehicule ralenti par un leader passe, un
        /// vehicule qui tourne en rond ne passe pas.
        /// </summary>
        private const float RoundaboutLapBudgetSeconds = 25f;

        private string originalProfileFilePath;

        private string tempProfileFilePath;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                if (manager.IsListening)
                {
                    manager.Shutdown();
                }

                Object.Destroy(manager.gameObject);
            }

            var survivor = RoadRageBootstrap.Instance;
            if (survivor != null)
            {
                Object.Destroy(survivor.gameObject);
            }

            if (originalProfileFilePath != null)
            {
                PlayerProfileFileStore.DefaultFilePath = originalProfileFilePath;
                originalProfileFilePath = null;
            }

            if (!string.IsNullOrEmpty(tempProfileFilePath) && File.Exists(tempProfileFilePath))
            {
                File.Delete(tempProfileFilePath);
            }

            tempProfileFilePath = null;

            yield return null;
        }

        [UnityTest]
        public IEnumerator TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story510-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;

            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MainMenuLobbySceneName));

            var menuScreen = Object.FindAnyObjectByType<MainMenuScreen>();
            Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");
            ClickSerializedButton(menuScreen, "playButton");
            yield return null;

            var lobbyScreen = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu apres Play");

            // Meme fenetre de tolerance que Story 5.7 / 5.9 : la creation du lobby Steam peut logguer
            // par frame de polling sans que le chemin lobby -> host -> MVP_Run soit en cause.
            LogAssert.ignoreFailingMessages = true;
            try
            {
                ClickSerializedButton(lobbyScreen, "startGameButton");

                var frames = 0;
                while (SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frames < 300)
                {
                    var bootstrap = RoadRageBootstrap.Instance;
                    if (bootstrap != null && bootstrap.LobbyRoom != null
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating
                        && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                    {
                        Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier le trafic portail-a-portail.");
                        yield break;
                    }

                    yield return null;
                    frames++;
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));
            Assert.That(NetworkManager.Singleton, Is.Not.Null, "Start Game doit avoir demarre un NetworkManager (AD-26).");
            Assert.That(NetworkManager.Singleton.IsServer, Is.True, "le pair local est l'hote du run.");

            var graph = Object.FindAnyObjectByType<LaneGraph>();
            Assert.That(graph, Is.Not.Null, "LaneGraph attendu dans MVP_Run.");
            Assert.That(graph.TrafficSettings, Is.Not.Null, "TrafficSettingsDef attendu sur le LaneGraph.");

            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null, "PortalTrafficSpawner attendu dans MVP_Run.");

            // Aucun effectif litteral ici : la cible vient du Def authore, comme dans le spawner.
            var targetPopulation = graph.TrafficSettings.ClampTargetPopulation(graph.TrafficSettings.DefaultTargetPopulation);
            Assert.That(targetPopulation, Is.GreaterThan(0), "Le Def doit authorer un effectif cible exploitable.");

            var entryPositions = PortalPositions(graph, graph.EntryPortals);
            var exitPositions = PortalPositions(graph, graph.ExitPortals);
            Assert.That(entryPositions.Count, Is.GreaterThan(0), "Le district doit porter des portails d'entree.");
            Assert.That(exitPositions.Count, Is.GreaterThan(0), "Le district doit porter des portails de sortie.");

            // Emprises des giratoires : sans elles, la preuve de non-bouclage ne pourrait pas mesurer
            // la duree de sejour dans un anneau. Le compte est verifie pour qu'un district renomme ne
            // rende pas la mesure silencieusement vide.
            var roundaboutCentres = RoundaboutCentres(graph);
            Assert.That(roundaboutCentres.Count, Is.EqualTo(4),
                "Quatre giratoires aux coins du district : la mesure de non-bouclage a besoin de leur emprise.");

            var lastKnownPosition = new Dictionary<ulong, Vector3>();
            var visitedNodes = new Dictionary<ulong, List<int>>();
            var dwellSeconds = new Dictionary<ulong, float[]>();
            var peakPopulation = 0;
            var departures = 0;
            var refilledAfterDeparture = false;

            var elapsed = 0f;
            while (elapsed < ObservationSeconds)
            {
                elapsed += Time.deltaTime;

                var live = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsInactive.Exclude);
                peakPopulation = Mathf.Max(peakPopulation, live.Length);

                Assert.That(live.Length, Is.LessThanOrEqualTo(targetPopulation),
                    "L'effectif ne depasse jamais la cible resolue a l'execution.");

                // Une place liberee par une sortie doit etre reprise : un remplacant entre, par un
                // portail, et l'effectif revient a la cible.
                if (departures > 0 && live.Length >= targetPopulation)
                {
                    refilledAfterDeparture = true;
                    break;
                }

                var stillAlive = new HashSet<ulong>();
                foreach (var controller in live)
                {
                    if (!controller.IsSpawned)
                    {
                        continue;
                    }

                    var id = controller.NetworkObjectId;
                    stillAlive.Add(id);

                    var position = controller.transform.position;

                    if (!lastKnownPosition.ContainsKey(id))
                    {
                        // Premiere observation : le vehicule vient d'etre insere. Il ne peut l'avoir
                        // ete qu'a un portail d'entree -- aucun autre mecanisme ne fait naitre un
                        // vehicule dans le district.
                        Assert.That(NearestDistance(position, entryPositions), Is.LessThan(PortalObservationRadius),
                            controller.name + " est apparu a " + position + ", loin de tout portail d'entree. Un vehicule n'entre que par un portail (AD-34).");

                        visitedNodes[id] = new List<int>();
                    }

                    lastKnownPosition[id] = position;

                    var state = controller.GetComponent<NetworkedAIVehicleState>();
                    if (state != null && state.IsSpawned)
                    {
                        AppendWaypoint(visitedNodes[id], state.WaypointIndex.Value);
                    }

                    TickRoundaboutDwell(dwellSeconds, roundaboutCentres, id, position, controller.name);

                    var velocity = controller.GetComponent<Rigidbody>().linearVelocity;
                    Assert.That(float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z), Is.False,
                        controller.name + " : une vitesse NaN contaminerait le NetworkTransform.");
                }

                var vanished = new List<ulong>();
                foreach (var tracked in lastKnownPosition.Keys)
                {
                    if (!stillAlive.Contains(tracked))
                    {
                        vanished.Add(tracked);
                    }
                }

                foreach (var gone in vanished)
                {
                    // La seule porte de sortie du trafic : un vehicule ne disparait qu'apres avoir
                    // atteint un noeud de portail de sortie. Ni blocage, ni distance au joueur, ni
                    // echec de trajet ne peuvent le retirer.
                    Assert.That(NearestDistance(lastKnownPosition[gone], exitPositions), Is.LessThan(PortalObservationRadius),
                        "Un vehicule a disparu en " + lastKnownPosition[gone] + ", loin de tout portail de sortie. Seul un portail retire un vehicule (AD-34).");

                    departures++;
                    lastKnownPosition.Remove(gone);
                    dwellSeconds.Remove(gone);
                }

                yield return null;
            }

            Assert.That(peakPopulation, Is.EqualTo(targetPopulation),
                "Le spawner doit faire converger l'effectif vers la cible authoree, en inserant a plusieurs portails.");
            Assert.That(departures, Is.GreaterThan(0),
                "Au moins un vehicule doit avoir traverse le district et etre ressorti par un portail pendant la fenetre d'observation.");
            Assert.That(refilledAfterDeparture, Is.True,
                "La place liberee par une sortie doit etre reprise : un remplacant entre par un portail et l'effectif revient a la cible.");

            // Preuve de non-bouclage (correctif post-livraison du 2026-09-16). La suite echantillonnee
            // est celle des noeuds VISES -- les repetitions consecutives ont ete absorbees a l'ajout --
            // donc une repetition y signale un noeud reellement rejoue. Un vehicule IA ne parcourt
            // jamais deux fois le meme noeud de voie : aucun tour complet de giratoire, aucun circuit
            // autour d'une jonction carree. Les suites des vehicules deja sortis sont conservees, sinon
            // un vehicule qui boucle puis sortirait par un portail echapperait a cette mesure.
            //
            // Deux mecanismes peuvent legitimement rouvrir la marche et faire echouer cette assertion :
            // la recuperation sur place, qui remet la memoire de parcours a zero, et la reorientation
            // gloutonne quand plus aucun successeur n'est eligible. Si elle saute, ce sont ces deux-la
            // qu'il faut instruire d'abord, pas la regle de non-bouclage elle-meme.
            foreach (var pair in visitedNodes)
            {
                var revisits = pair.Value.Count - new HashSet<int>(pair.Value).Count;
                Assert.That(revisits, Is.EqualTo(0),
                    "Vehicule " + pair.Key + " : " + revisits + " noeud(s) de voie revu(s), suite observee "
                    + string.Join(">", pair.Value) + ".");
            }

            var routes = new List<string>();
            foreach (var route in visitedNodes.Values)
            {
                var ordered = new List<int>(route);
                ordered.Sort();
                routes.Add(string.Join("-", ordered));
            }

            Assert.That(new HashSet<string>(routes).Count, Is.GreaterThan(1),
                "Les vehicules observes doivent avoir emprunte des parcours differents : le tirage pondere aux jonctions les separe, aucun itineraire n'est pre-calcule.");
        }

        /// <summary>
        /// Emprises des quatre giratoires, lues sur les enfants du graphe. Le nom d'instance est la
        /// seule forme d'authoring que ce test consomme, et il vient du prefab : le district les nomme
        /// <c>Roundabout_*</c> dans MVP_Run.
        /// </summary>
        private static List<Vector3> RoundaboutCentres(LaneGraph graph)
        {
            var centres = new List<Vector3>();
            foreach (Transform child in graph.transform)
            {
                if (child.name.StartsWith(RoundaboutNamePrefix, System.StringComparison.Ordinal))
                {
                    centres.Add(child.position);
                }
            }

            return centres;
        }

        /// <summary>
        /// Ajoute l'index a la suite echantillonnee en absorbant les repetitions CONSECUTIVES : un
        /// vehicule occupe le meme noeud de voie pendant plusieurs frames, donc l'echantillon brut
        /// repete legitimement. Ce qui compte est la suite des noeuds vises.
        ///
        /// Limite assumee de l'echantillonnage : un noeud revu puis requitte ENTRE deux observations
        /// serait absorbe. Cela demande deux decisions dans la meme frame rendue, ce que la regle de
        /// non-bouclage rend de toute facon impossible a l'echelle d'un noeud (un evenement de
        /// decision par FixedUpdate au plus) ; la preuve deterministe de la regle vit en EditMode.
        /// </summary>
        private static void AppendWaypoint(List<int> sequence, int waypointIndex)
        {
            if (sequence.Count == 0 || sequence[sequence.Count - 1] != waypointIndex)
            {
                sequence.Add(waypointIndex);
            }
        }

        /// <summary>
        /// Mesure le temps passe dans l'emprise de chaque giratoire, remis a zero des que le vehicule
        /// en sort. Un vehicule IA ne boucle jamais plus d'un tour : un anneau fait 37,7 m, soit moins
        /// de 5 s a 8 m/s.
        /// </summary>
        private static void TickRoundaboutDwell(
            Dictionary<ulong, float[]> dwellSeconds,
            List<Vector3> roundaboutCentres,
            ulong vehicleId,
            Vector3 position,
            string vehicleName)
        {
            if (!dwellSeconds.TryGetValue(vehicleId, out var perRoundabout))
            {
                perRoundabout = new float[roundaboutCentres.Count];
                dwellSeconds[vehicleId] = perRoundabout;
            }

            for (var i = 0; i < roundaboutCentres.Count; i++)
            {
                var offset = position - roundaboutCentres[i];
                offset.y = 0f;

                if (offset.sqrMagnitude > RoundaboutFootprintRadius * RoundaboutFootprintRadius)
                {
                    perRoundabout[i] = 0f;
                    continue;
                }

                perRoundabout[i] += Time.deltaTime;

                Assert.That(perRoundabout[i], Is.LessThan(RoundaboutLapBudgetSeconds),
                    vehicleName + " est reste " + perRoundabout[i].ToString("F1") + " s dans l'emprise du giratoire "
                    + i + " (" + roundaboutCentres[i] + ") : plus qu'un tour. Le parcours d'un vehicule IA ne revient "
                    + "jamais sur un noeud de voie, donc un anneau ne peut pas le retenir -- verifier la regle de non-bouclage.");
            }
        }

        private static List<Vector3> PortalPositions(LaneGraph graph, IReadOnlyList<int> portals)
        {
            var positions = new List<Vector3>(portals.Count);
            for (var i = 0; i < portals.Count; i++)
            {
                positions.Add(graph.GetNodePosition(portals[i]));
            }

            return positions;
        }

        private static float NearestDistance(Vector3 position, List<Vector3> candidates)
        {
            var best = float.PositiveInfinity;
            for (var i = 0; i < candidates.Count; i++)
            {
                var offset = candidates[i] - position;
                offset.y = 0f;
                best = Mathf.Min(best, offset.magnitude);
            }

            return best;
        }

        private static void ClickSerializedButton(Component screen, string buttonFieldName)
        {
            var button = GetPrivateField(screen, buttonFieldName) as Button;
            Assert.That(button, Is.Not.Null, screen.GetType().Name + "." + buttonFieldName + " doit referencer un Button de la scene.");
            button.onClick.Invoke();
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return field.GetValue(target);
        }
    }
}
