using System.Collections;
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
using RoadRage.Features.Rage;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.7 : preuve hote de la presentation client du trafic IA, dans MVP_Run. Les gardes
    /// EditMode decrivent le contrat ; ce fixture constate sur un pair reel que le trafic est
    /// effectivement spawe (Story 5.10 : insere aux portails par l'hote, plus pose en scene, donc
    /// l'effectif attendu est la cible authoree et non un 3 fige), que chacun expose un Behavior lisible depuis son propre
    /// NetworkedRageState, et que les libelles crees a l'execution rendent cette meme valeur repliquee
    /// (comportement IA en monde, et ligne Rage Road du HUD). Le Test Runner ne sait pas ouvrir deux
    /// pairs Steam : la lecture cote client est garantie par la permission Everyone verifiee en
    /// EditMode, ce fixture ne sert qu'a prouver que le chemin host-side produit bien cet etat.
    ///
    /// Le run est demarre par le chemin unique de la Story 5.3 (AD-26) : bootstrap -> menu -> lobby ->
    /// Start Game (StartHost puis chargement synchronise de MVP_Run). Comme Story15/16, une machine
    /// sans Steam P2P pleinement fonctionnel rend le test Inconclusif plutot que rouge.
    /// </summary>
    [Category("Story57")]
    public sealed class Story57AiTrafficClientPresentationPlayModeTests
    {
        private const string AiBehaviorLabelName = "AIBehaviorDebugLabel";

        private const string RageRoadEventLabelName = "RageRoadEventStatusLabel";

        private string originalProfileFilePath;

        private string tempProfileFilePath;

        /// <summary>
        /// Story 5.3 (AD-26) : Start Game demarre un vrai NetworkManager (StartHost) et le bootstrap
        /// survit en DontDestroyOnLoad. Sans arret explicite ici, la session hote et le bootstrap
        /// resteraient actifs et pollueraient les fixtures suivantes (meme risque que Story15/16/42).
        /// </summary>
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
        public IEnumerator HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels()
        {
            // Le profil persistant est redirige hors du dossier utilisateur AVANT tout chargement de
            // scene : le bootstrap lit ce chemin une seule fois dans Awake, donc une redirection plus
            // tardive laisserait le test lire et ecraser le profil reel de la machine.
            originalProfileFilePath = PlayerProfileFileStore.DefaultFilePath;
            tempProfileFilePath = Path.Combine(Path.GetTempPath(), "roadrage-story57-" + System.Guid.NewGuid().ToString("N") + ".json");
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

            // Fenetre reseau sensible (creation du lobby Steam + StartHost) : FacepunchTransport peut
            // logguer une exception par frame de polling sans que le chemin lobby -> host -> MVP_Run ne
            // soit en cause. La fenetre couvre aussi les quelques frames de stabilisation du spawn, et
            // est refermee avant les assertions, qui restent donc strictes.
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
                        Assert.Inconclusive("Services en ligne Steam non disponibles sur cette machine : impossible de verifier le spawn hote du trafic IA.");
                        yield break;
                    }

                    yield return null;
                    frames++;
                }

                for (var settleFrame = 0; settleFrame < 8; settleFrame++)
                {
                    yield return null;
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(AppSceneRouter.MvpRunSceneName));

            var manager = NetworkManager.Singleton;
            Assert.That(manager, Is.Not.Null, "Start Game doit avoir demarre un NetworkManager (AD-26).");
            Assert.That(manager.IsServer, Is.True, "le pair local est l'hote du run.");

            // Story 5.10 : le trafic entre par les portails du graphe de voies. L'effectif attendu est
            // celui que le Def authore (AD-32) -- aucun nombre litteral ici -- et il faut laisser le
            // spawner hote converger avant de constater la presentation.
            var graph = Object.FindAnyObjectByType<LaneGraph>();
            Assert.That(graph, Is.Not.Null, "LaneGraph attendu dans MVP_Run : c'est lui qui porte les portails.");
            Assert.That(graph.TrafficSettings, Is.Not.Null, "TrafficSettingsDef attendu sur le LaneGraph.");

            var targetPopulation = graph.TrafficSettings.ClampTargetPopulation(graph.TrafficSettings.DefaultTargetPopulation);

            var aiVehicles = new NetworkedAIVehicleState[0];
            for (var frame = 0; frame < 900; frame++)
            {
                aiVehicles = Object.FindObjectsByType<NetworkedAIVehicleState>(FindObjectsInactive.Exclude);
                if (aiVehicles.Length >= targetPopulation)
                {
                    break;
                }

                yield return null;
            }

            // Quelques frames de stabilisation : les libelles de comportement sont crees a l'execution.
            for (var settleFrame = 0; settleFrame < 4; settleFrame++)
            {
                yield return null;
            }

            aiVehicles = Object.FindObjectsByType<NetworkedAIVehicleState>(FindObjectsInactive.Exclude);
            Assert.That(aiVehicles.Length, Is.GreaterThan(0),
                "L'hote doit avoir insere du trafic aux portails : sans vehicule spawe, aucun client ne voit quoi que ce soit.");
            Assert.That(aiVehicles.Length, Is.LessThanOrEqualTo(targetPopulation),
                "L'effectif ne depasse jamais la cible resolue a l'execution.");

            foreach (var ai in aiVehicles)
            {
                Assert.That(ai.IsSpawned, Is.True, ai.name + " doit etre spawn.");
                Assert.That(ai.IsServer, Is.True, ai.name + " est simule par l'hote.");

                var rage = ai.GetComponent<NetworkedRageState>();
                Assert.That(rage, Is.Not.Null, ai.name + " doit porter l'etat de rage replique.");
                Assert.That(rage.IsSpawned, Is.True, ai.name + " : la rage doit etre repliquee, pas locale.");

                Assert.That(ai.GetComponent<NetworkTransform>(), Is.Not.Null,
                    ai.name + " doit repliquer sa position par le NetworkTransform existant.");

                Assert.That(ai.Behavior.Value, Is.EqualTo(rage.CurrentDisposition),
                    ai.name + " : Behavior est la projection publiee de sa propre rage, jamais une seconde verite.");

                var label = ai.transform.Find(AiBehaviorLabelName)?.GetComponent<TMP_Text>();
                Assert.That(label, Is.Not.Null,
                    ai.name + " : le libelle de comportement est cree a l'execution, sans cablage de scene.");
                Assert.That(label.text, Does.Contain("IA : " + ai.Behavior.Value),
                    ai.name + " : le libelle rend le Behavior replique.");
            }

            var runState = Object.FindAnyObjectByType<NetworkedRunState>();
            Assert.That(runState, Is.Not.Null, "NetworkedRunState attendu dans MVP_Run.");
            Assert.That(runState.RageRoadEvent.Value, Is.EqualTo(RageRoadEventState.Idle),
                "Aucun evenement Rage Road au demarrage du run.");

            var hud = Object.FindAnyObjectByType<RunCheckpointHudScreen>();
            Assert.That(hud, Is.Not.Null, "HUD attendu dans MVP_Run.");

            var eventLabel = hud.transform.Find(RageRoadEventLabelName)?.GetComponent<TMP_Text>();
            Assert.That(eventLabel, Is.Not.Null, "Le libelle Rage Road est cree a l'execution.");
            Assert.That(eventLabel.text, Is.EqualTo(RunCheckpointHudScreen.NoRageRoadEventState),
                "Le HUD rend l'etat replique (Idle), jamais une valeur locale de secours.");
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
