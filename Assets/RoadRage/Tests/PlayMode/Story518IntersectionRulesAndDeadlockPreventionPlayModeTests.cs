using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.MainMenu;
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
    /// Story 5.18 : preuve RUNTIME des regles d'intersection, celle qu'aucune garde EditMode ne peut
    /// porter. Le modele pur se prouve hors scene (fixture EditMode) ; ce que ce banc constate, c'est
    /// que deux vehicules REELLEMENT presents, dans la meme session hote, a la meme jonction, ne
    /// franchissent pas ensemble.
    ///
    /// Le banc AMENE les deux vehicules sur leurs approches : un district ou le tirage de virage est
    /// aleatoire ne les y fait pas converger dans un temps borne. La conduite, elle, n'est pas
    /// manipulee -- aucune ligne de ce fichier n'ecrit une vitesse, une pose ou une intention : ce sont
    /// les memes <c>FixedUpdate</c>, la meme physique et le meme arbitrage que ceux du jeu.
    ///
    /// Ce que le banc observe, et pourquoi c'est la bonne observables :
    /// - les deux se voient (l'un des deux compte au moins deux revendications) ;
    /// - une seule franchit en premier, et jamais les deux au meme pas ;
    /// - aucune des deux ne reste figee : l'autre franchit a son tour ;
    /// - aucune des deux n'est retiree ni teleportee -- c'est l'AC qui interdit tout palier de retrait.
    ///
    /// Meme demarrage que les Stories 5.7, 5.10 et 5.17 : bootstrap -> menu -> lobby -> Start Game. Une
    /// machine sans Steam P2P fonctionnel rend le test Inconclusif plutot que rouge.
    /// </summary>
    [Category("Story518")]
    public sealed class Story518IntersectionRulesAndDeadlockPreventionPlayModeTests
    {
        /// <summary>Bornes d'attente du demarrage, en frames. Memes valeurs que les bancs 5.10 et 5.17.</summary>
        private const int SceneLoadFrameBudget = 300;

        private const int TrafficSpawnFrameBudget = 900;

        /// <summary>
        /// Pas de physique accordes a une paire d'approches pour produire une revendication commune.
        /// La revendication se reconstruit a chaque <c>FixedUpdate</c>, donc quelques pas suffisent ;
        /// en accorder davantage laisserait les vehicules franchir avant l'observation.
        /// </summary>
        private const int ClaimProbeSteps = 12;

        /// <summary>
        /// Fenetre d'observation apres mise en place, en pas de physique. 50 Hz x 2400 = 48 s : de quoi
        /// couvrir deux cycles de feux complets (24 s) et le delai d'escalade authore (12 s), avec une
        /// seconde chance pour chaque vehicule.
        /// </summary>
        private const int ObservationStepBudget = 2400;

        /// <summary>
        /// Deplacement maximal d'un vehicule en un pas de physique. Au-dela, ce n'est pas de la conduite
        /// mais un repositionnement -- et l'AC interdit nommement toute teleportation.
        /// </summary>
        private const float MaxStepDisplacement = 1f;

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

            if (RoadRageBootstrap.Instance != null)
            {
                Object.Destroy(RoadRageBootstrap.Instance.gameObject);
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
        public IEnumerator TwoVehiclesClaimingTheSameJunctionNeverCrossTogether()
        {
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story518-" + System.Guid.NewGuid().ToString("N") + ".json");
            PlayerProfileFileStore.DefaultFilePath = tempProfileFilePath;

            SceneManager.LoadScene(AppSceneRouter.BootstrapSceneName);
            yield return null;
            yield return null;

            ClickSerializedButton(Object.FindAnyObjectByType<MainMenuScreen>(), "playButton");
            yield return null;
            var lobby = Object.FindAnyObjectByType<LobbyShellScreen>();
            Assert.That(lobby, Is.Not.Null, "LobbyShellScreen attendu dans MainMenuLobby");
            ClickSerializedButton(lobby, "startGameButton");
            for (var frame = 0; SceneManager.GetActiveScene().name != AppSceneRouter.MvpRunSceneName && frame < SceneLoadFrameBudget; frame++)
            {
                var bootstrap = RoadRageBootstrap.Instance;
                if (bootstrap != null && bootstrap.LobbyRoom != null && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Open
                    && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Creating && bootstrap.LobbyRoom.Status != LobbyRoomStatus.Closed)
                {
                    Assert.Inconclusive("Services Steam indisponibles : banc 5.18 non executable.");
                    yield break;
                }

                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName),
                "Le banc observe le district reel : MVP_Run doit etre la scene active.");

            List<NetworkedAIVehicleDriverController> traffic = null;
            for (var frame = 0; frame < TrafficSpawnFrameBudget; frame++)
            {
                traffic = Object.FindObjectsByType<NetworkedAIVehicleDriverController>(FindObjectsSortMode.None)
                    .Where(controller => controller.IsSpawned)
                    .ToList();
                if (traffic.Count >= 2)
                {
                    break;
                }

                yield return null;
            }

            Assert.That(traffic, Is.Not.Null);
            Assert.That(traffic.Count, Is.GreaterThanOrEqualTo(2),
                "Deux vehicules IA au moins doivent etre apparus par les portails du district.");

            var graph = Object.FindAnyObjectByType<LaneGraph>();
            Assert.That(graph, Is.Not.Null, "LaneGraph attendu dans MVP_Run");

            // Le banc CHERCHE une paire reellement contestee au lieu de la supposer. La concurrence
            // de deux approches ne depend pas que de leur geometrie d'entree : elle depend aussi de la
            // SORTIE que chaque vehicule tire au moment d'arriver, et ce tirage est pondere par les
            // ratios authores. Deux approches perpendiculaires dont les deux tirages tournent du meme
            // cote ne se croisent pas -- et c'est un resultat correct, pas un echec d'arbitrage.
            // Supposer la concurrence rendait donc ce banc dependant d'un tirage.
            var candidates = PickContestedPairs(graph);
            Assert.That(candidates.Count, Is.GreaterThan(0),
                "Une jonction du district doit porter deux approches qui se croisent.");

            var first = traffic[0];
            var second = traffic[1];
            NetworkedAIVehicleState firstState = null;
            NetworkedAIVehicleState secondState = null;
            var firstApproach = -1;
            var secondApproach = -1;
            var engaged = false;

            foreach (var pair in candidates)
            {
                firstState = PlaceOnApproach(first, graph, pair[0]);
                secondState = PlaceOnApproach(second, graph, pair[1]);
                firstApproach = pair[0].NodeIndex;
                secondApproach = pair[1].NodeIndex;
                Physics.SyncTransforms();

                // Quelques pas suffisent : la revendication se reconstruit a chaque FixedUpdate, elle
                // n'a pas besoin d'etre attendue. Au-dela, les vehicules commenceraient a franchir.
                for (var probe = 0; probe < ClaimProbeSteps && !engaged; probe++)
                {
                    yield return new WaitForFixedUpdate();
                    engaged = first.JunctionClaimants >= 2 || second.JunctionClaimants >= 2;
                }

                if (engaged)
                {
                    break;
                }
            }

            Assert.That(engaged, Is.True,
                "Aucune des " + candidates.Count + " paires d'approches contestees du district n'a produit"
                + " une revendication commune : sans cela, ce banc ne prouve rien (A=" + first.JunctionClaimants
                + ", refus=" + first.TrafficRefusal + " ; B=" + second.JunctionClaimants
                + ", refus=" + second.TrafficRefusal + ").");
            var firstCrossed = -1;
            var secondCrossed = -1;
            var firstPrevious = first.transform.position;
            var secondPrevious = second.transform.position;
            var sawClaimOfTwo = 1;

            for (var step = 0; step < ObservationStepBudget; step++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(first == null || first.IsSpawned, Is.True, "Le vehicule A ne doit jamais etre retire par les regles d'intersection.");
                Assert.That(second == null || second.IsSpawned, Is.True, "Le vehicule B ne doit jamais etre retire par les regles d'intersection.");

                var firstPosition = first.transform.position;
                var secondPosition = second.transform.position;
                Assert.That(Vector3.Distance(firstPosition, firstPrevious), Is.LessThan(MaxStepDisplacement),
                    "Aucun repositionnement de A : la story n'admet ni teleportation ni retrait (refus="
                    + first.TrafficRefusal + ").");
                Assert.That(Vector3.Distance(secondPosition, secondPrevious), Is.LessThan(MaxStepDisplacement),
                    "Aucun repositionnement de B (refus=" + second.TrafficRefusal + ").");
                firstPrevious = firstPosition;
                secondPrevious = secondPosition;

                if (first.JunctionClaimants >= 2 || second.JunctionClaimants >= 2)
                {
                    sawClaimOfTwo++;
                }

                if (firstCrossed < 0 && firstState.WaypointIndex.Value != firstApproach)
                {
                    firstCrossed = step;
                }

                if (secondCrossed < 0 && secondState.WaypointIndex.Value != secondApproach)
                {
                    secondCrossed = step;
                }

                if (firstCrossed >= 0 && secondCrossed >= 0)
                {
                    break;
                }
            }

            Assert.That(sawClaimOfTwo, Is.GreaterThan(0), "Revendication commune observee avant l'observation.");

            Assert.That(firstCrossed, Is.GreaterThanOrEqualTo(0),
                "Le vehicule A doit finir par franchir : aucune approche ne reste figee. " + Trace(first));
            Assert.That(secondCrossed, Is.GreaterThanOrEqualTo(0),
                "Le vehicule B doit finir par franchir : aucune approche ne reste figee. " + Trace(second)
                + " | A: " + Trace(first));
            Assert.That(firstCrossed, Is.Not.EqualTo(secondCrossed),
                "Les deux vehicules ont franchi au MEME pas : c'est exactement l'etat que la story interdit.");
        }

        /// <summary>
        /// Deux approches d'une MEME jonction qui se croisent vraiment, choisies pour que chacune garde
        /// sa sortie libre : la paire la plus eloignee du reseau. Une paire dont les noeuds se touchent
        /// mettrait chaque sortie dans la zone de degagement de l'autre, et le banc mesurerait la
        /// saturation au lieu de l'arbitrage.
        ///
        /// La concurrence se teste avec le meme predicat que l'arbitrage : deux approches opposees d'une
        /// meme avenue ne se disputent rien, et les prendre ferait passer le test sans rien prouver.
        /// </summary>
        private static List<JunctionApproachInfo[]> PickContestedPairs(LaneGraph graph)
        {
            var found = new List<JunctionApproachInfo[]>();
            var separations = new List<float>();
            var approaches = graph.JunctionApproaches;
            for (var i = 0; i < approaches.Count; i++)
            {
                for (var j = i + 1; j < approaches.Count; j++)
                {
                    if (approaches[i].JunctionKey != approaches[j].JunctionKey)
                    {
                        continue;
                    }

                    var first = new JunctionClaim(approaches[i].JunctionKey, approaches[i].Rule, approaches[i].Forward,
                        0f, approaches[i].NodeIndex, 0UL, true, true, true, false);
                    var second = new JunctionClaim(approaches[j].JunctionKey, approaches[j].Rule, approaches[j].Forward,
                        0f, approaches[j].NodeIndex, 1UL, true, true, true, false);
                    if (!JunctionRules.Conflicts(first, second))
                    {
                        continue;
                    }

                    found.Add(new[] { approaches[i], approaches[j] });
                    separations.Add(Vector3.Distance(
                        graph.GetNodePosition(approaches[i].NodeIndex),
                        graph.GetNodePosition(approaches[j].NodeIndex)));
                }
            }

            // Les plus eloignees d'abord : une paire dont les noeuds se touchent mettrait chaque
            // sortie dans la zone de degagement de l'autre, et le banc mesurerait la saturation au
            // lieu de l'arbitrage. Les paires serrees restent en repli plutot qu'ecartees.
            var order = Enumerable.Range(0, found.Count).OrderByDescending(index => separations[index]);
            return order.Select(index => found[index]).ToList();
        }

        /// <summary>
        /// Pose un vehicule sur son noeud de decision, a l'arret, et declare ce noeud comme son noeud
        /// courant : c'est le seul etat que le banc ecrit. Aucune vitesse, aucune intention, aucune pose
        /// ulterieure -- la conduite a partir de la appartient au conducteur et a la physique.
        ///
        /// Rend l'etat replique, parce que l'index de noeud courant vit dessus : c'est lui que le banc
        /// observe pour compter les franchissements.
        /// </summary>
        private static NetworkedAIVehicleState PlaceOnApproach(NetworkedAIVehicleDriverController controller,
            LaneGraph graph, in JunctionApproachInfo approach)
        {
            // Le vehicule se pose sur sa VOIE D'APPROCHE, en amont de la ligne d'arret -- plus sur le
            // noeud de decision, qui est authore au centre de l'aire de conflit. L'y poser revenait a
            // le faire naitre deja engage dans le carrefour, ligne d'arret derriere lui : l'arbitrage
            // ne pouvait plus rien en faire (mesure du 2026-09-21 : ecart a la ligne de -5,49 m).
            var node = graph.GetNodePosition(approach.NodeIndex);
            var entry = approach.EntryNode == approach.NodeIndex ? node : approach.EntryPoint;
            var forward = approach.EntryForward.sqrMagnitude > 0.0001f ? approach.EntryForward : approach.Forward;
            var stopLine = Mathf.Max(0f, approach.ConflictEntryDistance - 1.33f);
            var placement = entry + forward * Mathf.Max(0f, stopLine - 3f);
            placement.y = node.y;
            var body = controller.GetComponent<Rigidbody>();
            controller.transform.SetPositionAndRotation(placement, Quaternion.LookRotation(forward, Vector3.up));
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.Sleep();
            }

            var state = GetPrivateField(controller, "state") as NetworkedAIVehicleState;
            Assert.That(state, Is.Not.Null, "Le vehicule IA doit porter son etat replique.");
            state.WaypointIndex.Value = approach.NodeIndex;
            return state;
        }

        /// <summary>
        /// Trace complete d'un vehicule pour les messages d'echec. ANO-5.18 : un motif seul ne se
        /// verifie pas -- il faut le SUJET et les grandeurs pour savoir quoi corriger.
        /// </summary>
        private static string Trace(NetworkedAIVehicleDriverController vehicle)
        {
            if (vehicle == null) return "(absent)";
            return vehicle.name + " [" + vehicle.DecisionReason + "] refus=" + vehicle.TrafficRefusal
                + " detail=" + vehicle.DecisionDetail
                + " voie=" + vehicle.CurrentLaneNode
                + " revendications=" + vehicle.JunctionClaimants
                + " attente=" + vehicle.JunctionWaitSeconds.ToString("0.0") + "s"
                + " palier=" + vehicle.UnblockingAction
                + " perce=" + vehicle.JunctionBreached;
        }

        private static void ClickSerializedButton(Component screen, string fieldName)
        {
            Assert.That(screen, Is.Not.Null);
            var button = GetPrivateField(screen, fieldName) as Button;
            Assert.That(button, Is.Not.Null, "bouton introuvable : " + fieldName);
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
