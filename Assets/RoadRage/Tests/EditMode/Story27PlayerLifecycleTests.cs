using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Domain;
using TMPro;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.7 : table de transitions de cycle de vie (IsValidTransition), detection
    /// "tous morts" (ShouldLogAllDeadRestartCondition), seuil de chute hors limites
    /// (IsBelowVoidHeightThreshold) et le nouveau predicat de declenchement transport-failure de
    /// NetworkedRunSessionMonitor -- tous des predicats statiques purs, testables sans NetworkManager
    /// reel, meme pattern que ShouldReturnClientToLobby (Story 2.5). Complete par des verifications
    /// de source pour l'invariant "Lifecycle mute uniquement par le service" et le wiring des
    /// nouveaux composants (prefab NetworkedPlayerRoot, TryGetState sur le spawner).
    /// </summary>
    public sealed class Story27PlayerLifecycleTests
    {
        [Test]
        public void PlayerLifecycleDisconnectedIsDistinctFromExistingValues()
        {
            Assert.That((int)PlayerLifecycle.Disconnected, Is.EqualTo(3));
            Assert.That(PlayerLifecycle.Disconnected, Is.Not.EqualTo(PlayerLifecycle.Alive));
            Assert.That(PlayerLifecycle.Disconnected, Is.Not.EqualTo(PlayerLifecycle.Downed));
            Assert.That(PlayerLifecycle.Disconnected, Is.Not.EqualTo(PlayerLifecycle.Dead));
        }

        [Test]
        public void IsValidTransitionAllowsAliveToDownedDeadOrDisconnectedOnly()
        {
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Alive, PlayerLifecycle.Downed), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Alive, PlayerLifecycle.Dead), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Alive, PlayerLifecycle.Disconnected), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Alive, PlayerLifecycle.Alive), Is.False);
        }

        [Test]
        public void IsValidTransitionAllowsDownedToDeadOrDisconnectedOnly()
        {
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Downed, PlayerLifecycle.Dead), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Downed, PlayerLifecycle.Disconnected), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Downed, PlayerLifecycle.Alive), Is.False,
                "Downed -> Alive doit passer par un respawn (Dead), jamais une transition directe.");
        }

        [Test]
        public void IsValidTransitionAllowsDeadToAliveOnlyViaRespawnPath()
        {
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Dead, PlayerLifecycle.Alive), Is.True,
                "seule sortie de Dead : le respawn (TryRespawn), jamais la reconnexion.");
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Dead, PlayerLifecycle.Disconnected), Is.False);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Dead, PlayerLifecycle.Downed), Is.False);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Dead, PlayerLifecycle.Dead), Is.False);
        }

        [Test]
        public void IsValidTransitionAllowsDisconnectedToAliveOnly()
        {
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Disconnected, PlayerLifecycle.Alive), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Disconnected, PlayerLifecycle.Dead), Is.False);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Disconnected, PlayerLifecycle.Downed), Is.False);
            Assert.That(NetworkedPlayerLifecycleService.IsValidTransition(PlayerLifecycle.Disconnected, PlayerLifecycle.Disconnected), Is.False);
        }

        [Test]
        public void ShouldLogAllDeadRestartConditionDetectsAllDeadFromOneToFourPlayers()
        {
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new[] { PlayerLifecycle.Dead }), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new[] { PlayerLifecycle.Dead, PlayerLifecycle.Dead }), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new[] { PlayerLifecycle.Dead, PlayerLifecycle.Dead, PlayerLifecycle.Dead }), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new[] { PlayerLifecycle.Dead, PlayerLifecycle.Dead, PlayerLifecycle.Dead, PlayerLifecycle.Dead }), Is.True);
        }

        [Test]
        public void ShouldLogAllDeadRestartConditionIsFalseForPartialDeathsOrNoSpawnedPlayer()
        {
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new PlayerLifecycle[0]), Is.False,
                "aucun clientId connecte n'a d'etat spawn : pas de condition de restart.");
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new[] { PlayerLifecycle.Dead, PlayerLifecycle.Alive }), Is.False,
                "cas partiel (2 joueurs, un seul mort) : ne doit pas declencher.");
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new[] { PlayerLifecycle.Dead, PlayerLifecycle.Dead, PlayerLifecycle.Alive, PlayerLifecycle.Dead }), Is.False,
                "cas partiel (4 joueurs, un seul vivant) : ne doit pas declencher.");
        }

        [Test]
        public void ShouldLogAllDeadRestartConditionResetsOnceAPlayerLeavesDead()
        {
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new[] { PlayerLifecycle.Dead, PlayerLifecycle.Dead }), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.ShouldLogAllDeadRestartCondition(new[] { PlayerLifecycle.Alive, PlayerLifecycle.Dead }), Is.False,
                "des qu'un respawn ramene un joueur a Alive, la condition ne doit plus etre active (garde reinitialisee).");
        }

        [Test]
        public void IsBelowVoidHeightThresholdUsesStrictComparisonAgainstConfiguredThreshold()
        {
            Assert.That(NetworkedPlayerLifecycleService.IsBelowVoidHeightThreshold(-10.01f, -10f), Is.True);
            Assert.That(NetworkedPlayerLifecycleService.IsBelowVoidHeightThreshold(-10f, -10f), Is.False, "egal au seuil : pas encore en dessous.");
            Assert.That(NetworkedPlayerLifecycleService.IsBelowVoidHeightThreshold(0f, -10f), Is.False);
        }

        [Test]
        public void ShouldReturnClientToLobbyOnTransportFailureIgnoresDoubleTrigger()
        {
            Assert.That(NetworkedRunSessionMonitor.ShouldReturnClientToLobbyOnTransportFailure(false), Is.True);
            Assert.That(NetworkedRunSessionMonitor.ShouldReturnClientToLobbyOnTransportFailure(true), Is.False,
                "un retour au lobby deja en cours ne doit pas se redeclencher.");
        }

        [Test]
        public void RunSessionMonitorSubscribesAndUnsubscribesTransportFailure()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/NetworkedRunSessionMonitor.cs");

            Assert.That(source, Does.Contain("manager.OnTransportFailure += HandleTransportFailure"));
            Assert.That(source, Does.Contain("manager.OnTransportFailure -= HandleTransportFailure"));
        }

        [Test]
        public void SpawnServiceTracksNetworkedPlayerStateAlongsideSpawnedClients()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs");

            Assert.That(source, Does.Contain("spawnedStates[clientId] = state"));
            Assert.That(source, Does.Contain("public bool TryGetState(ulong clientId, out NetworkedPlayerState state)"));
        }

        [Test]
        public void LifecycleIntentRpcValidatesSenderAndDelegatesRespawnToLifecycleService()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/NetworkedPlayerLifecycleIntent.cs");

            Assert.That(source, Does.Contain("[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]"));
            Assert.That(source, Does.Contain("state.ClientId.Value != rpcParams.Receive.SenderClientId"));
            Assert.That(source, Does.Contain("NetworkedPlayerLifecycleService.Instance.TryRespawn(state.ClientId.Value)"));
            Assert.That(source, Does.Not.Contain("Lifecycle.Value ="),
                "l'intention ne doit jamais ecrire Lifecycle elle-meme : seul le service en a le droit.");
            Assert.That(source, Does.Not.Contain("WorldPosition.Value ="),
                "l'intention ne doit jamais ecrire WorldPosition elle-meme : seul le service en a le droit.");
        }

        [Test]
        public void OnlyLifecycleServiceWritesLifecycleAndPoseOnRespawn()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/NetworkedPlayerLifecycleService.cs");

            Assert.That(source, Does.Contain("state.Lifecycle.Value = next"));
            Assert.That(source, Does.Contain("state.WorldPosition.Value = spawnPosition"));
            Assert.That(source, Does.Contain("state.YawDegrees.Value = spawnYaw"));
        }

        [Test]
        public void HudPresentationAndRunFlowNeverWriteLifecycleNetworkVariable()
        {
            var hudSource = File.ReadAllText("Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs");
            var runFlowSource = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");
            var presentationSource = File.ReadAllText("Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs");
            var intentSource = File.ReadAllText("Assets/RoadRage/App/Run/NetworkedPlayerLifecycleIntent.cs");

            Assert.That(hudSource, Does.Not.Contain("Lifecycle.Value ="));
            Assert.That(runFlowSource, Does.Not.Contain("Lifecycle.Value ="));
            Assert.That(presentationSource, Does.Not.Contain("Lifecycle.Value ="));
            Assert.That(intentSource, Does.Not.Contain("Lifecycle.Value ="));
        }

        [Test]
        public void NetworkedPlayerRootPrefabHasLifecycleIntentComponent()
        {
            var playerRootPrefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            Assert.That(playerRootPrefab, Is.Not.Null, "NetworkedPlayerRoot doit rester chargeable depuis Resources.");

            var intent = playerRootPrefab.GetComponent<NetworkedPlayerLifecycleIntent>();
            Assert.That(intent, Is.Not.Null, "NetworkedPlayerRoot doit porter le composant d'intention de respawn (Story 2.7).");
        }

        [Test]
        public void LocalVoidRespawnControllerThresholdPredicateMatchesNetworkedPathValues()
        {
            Assert.That(LocalVoidRespawnController.IsBelowVoidHeightThreshold(-10.01f, -10f), Is.True);
            Assert.That(LocalVoidRespawnController.IsBelowVoidHeightThreshold(-10f, -10f), Is.False, "egal au seuil : pas encore en dessous.");
            Assert.That(LocalVoidRespawnController.IsBelowVoidHeightThreshold(0f, -10f), Is.False);
        }

        [Test]
        public void LocalVoidRespawnControllerFreezesMovementOnFallAndTeleportsBackOnRespawn()
        {
            var root = new GameObject("LocalVoidRespawnTest");
            try
            {
                root.transform.position = new Vector3(1f, 2f, 3f);
                root.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

                var onFootController = root.AddComponent<LocalOnFootController>();
                var controller = root.AddComponent<LocalVoidRespawnController>();
                InvokePrivateMethod(controller, "Awake");

                Assert.That(onFootController, Is.Not.Null,
                    "LocalVoidRespawnController exige LocalOnFootController (RequireComponent) -- meme rig que le gel/teleport doit piloter.");
                Assert.That(controller.IsDead, Is.False, "au spawn, le joueur solo n'est pas mort.");
                Assert.That(onFootController.MovementEnabled, Is.True);

                root.transform.position = new Vector3(1f, -25f, 3f);
                InvokePrivateMethod(controller, "Die");

                Assert.That(controller.IsDead, Is.True, "sous le seuil : le joueur solo doit passer 'mort'.");
                Assert.That(onFootController.MovementEnabled, Is.False,
                    "mouvement fige : Update() du controller a pied doit etre gele (bug fix : desactiver seulement le CharacterController ne stoppait pas l'accumulation de gravite).");

                InvokePrivateMethod(controller, "Respawn");

                Assert.That(controller.IsDead, Is.False, "respawn : retour a l'etat vivant.");
                Assert.That(onFootController.MovementEnabled, Is.True, "respawn : mouvement reactive.");
                Assert.That(root.transform.position, Is.EqualTo(new Vector3(1f, 2f, 3f)),
                    "respawn : teleportation a la position de spawn capturee au Awake().");
                Assert.That(root.transform.rotation, Is.EqualTo(Quaternion.Euler(0f, 45f, 0f)),
                    "respawn : rotation de spawn restauree elle aussi.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LocalVoidRespawnControllerHasNoNetcodeDependency()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/LocalVoidRespawnController.cs");

            Assert.That(source, Does.Not.Contain("using Unity.Netcode"), "aucune dependance au package Netcode.");
            Assert.That(source, Does.Not.Contain(": NetworkBehaviour"), "doit rester un MonoBehaviour, jamais un NetworkBehaviour.");
            Assert.That(source, Does.Not.Contain("NetworkVariable<"), "aucune variable reseau : etat local pur (isDead).");
            Assert.That(source, Does.Not.Contain("[Rpc("), "aucune Rpc : chemin solo entierement local.");
        }

        [Test]
        public void RunFlowAttachesLocalVoidRespawnControllerOnlyWhenNotNetworked()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");

            Assert.That(source, Does.Contain("AttachNetworkPoseReporter(activeLocalPlayer);"));
            Assert.That(source, Does.Contain("AttachLocalVoidRespawnController(activeLocalPlayer, checkpointHud);"));
            Assert.That(source, Does.Contain("var isNetworked = manager != null && manager.IsListening;"));
        }

        [Test]
        public void LifecycleServiceZeroesHeartsOnDeathAndRestoresOnRespawn()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/NetworkedPlayerLifecycleService.cs");

            Assert.That(source, Does.Contain("state.Hearts.Value = 0"), "la vie doit se vider quand la transition aboutit a Dead.");
            Assert.That(source, Does.Contain("state.Hearts.Value = state.MaxHearts.Value"), "la vie doit se restaurer au maximum au respawn.");
        }

        [Test]
        public void RunFlowSubscribesAndUnsubscribesLifecycleForDeathOverlay()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");

            Assert.That(source, Does.Contain("state.Lifecycle.OnValueChanged += HandleLifecycleChanged;"));
            Assert.That(source, Does.Contain("localNetworkedPlayerState.Lifecycle.OnValueChanged -= HandleLifecycleChanged;"));
            Assert.That(source, Does.Contain("checkpointHud.ShowDeathOverlay();"));
            Assert.That(source, Does.Contain("checkpointHud.HideDeathOverlay();"));
        }

        [Test]
        public void LocalVoidRespawnControllerDrivesHudHeartsAndDeathOverlaySourceInvariant()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/LocalVoidRespawnController.cs");

            Assert.That(source, Does.Contain("CheckpointHud.SetHearts(0, NetworkedPlayerState.DefaultMaxHearts);"));
            Assert.That(source, Does.Contain("CheckpointHud.ShowDeathOverlay();"));
            Assert.That(source, Does.Contain("CheckpointHud.SetHearts(NetworkedPlayerState.DefaultMaxHearts, NetworkedPlayerState.DefaultMaxHearts);"));
            Assert.That(source, Does.Contain("CheckpointHud.HideDeathOverlay();"));
        }

        [Test]
        public void ShowDeathOverlaySetsTextAndActivatesPanelThenHideDeactivatesIt()
        {
            var root = new GameObject("DeathOverlayHudTest");
            try
            {
                var hud = root.AddComponent<RunCheckpointHudScreen>();
                var panel = new GameObject("DeathOverlayPanel");
                panel.transform.SetParent(root.transform);
                panel.SetActive(false);
                var label = AddLabel(root, "DeathOverlayLabel");

                SetPrivateField(hud, "deathOverlayPanel", panel);
                SetPrivateField(hud, "deathOverlayLabel", label);

                hud.ShowDeathOverlay();

                Assert.That(panel.activeSelf, Is.True, "l'overlay doit s'activer a l'affichage.");
                Assert.That(label.text, Is.EqualTo(RunCheckpointHudScreen.DeathOverlayText));

                hud.HideDeathOverlay();

                Assert.That(panel.activeSelf, Is.False, "l'overlay doit se masquer au retour a Alive.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LocalVoidRespawnControllerDieAndRespawnUpdateWiredHudHeartsAndOverlay()
        {
            var root = new GameObject("LocalVoidRespawnHudTest");
            var hudRoot = new GameObject("LocalVoidRespawnHudTestHud");
            try
            {
                var hud = hudRoot.AddComponent<RunCheckpointHudScreen>();
                var heartsLabel = AddLabel(hudRoot, "HeartsLabel");
                var panel = new GameObject("DeathOverlayPanel");
                panel.transform.SetParent(hudRoot.transform);
                panel.SetActive(false);
                var deathLabel = AddLabel(hudRoot, "DeathOverlayLabel");

                SetPrivateField(hud, "heartsLabel", heartsLabel);
                SetPrivateField(hud, "deathOverlayPanel", panel);
                SetPrivateField(hud, "deathOverlayLabel", deathLabel);

                var controller = root.AddComponent<LocalVoidRespawnController>();
                controller.CheckpointHud = hud;

                InvokePrivateMethod(controller, "Die");

                Assert.That(heartsLabel.text, Is.EqualTo("Vie : " + RunCheckpointHudScreen.FormatHearts(0, NetworkedPlayerState.DefaultMaxHearts)),
                    "la mort solo doit vider la barre de vie du HUD, comme cote reseau.");
                Assert.That(panel.activeSelf, Is.True, "la mort solo doit afficher l'overlay plein ecran.");

                InvokePrivateMethod(controller, "Respawn");

                Assert.That(heartsLabel.text, Is.EqualTo("Vie : " + RunCheckpointHudScreen.FormatHearts(NetworkedPlayerState.DefaultMaxHearts, NetworkedPlayerState.DefaultMaxHearts)),
                    "le respawn solo doit restaurer la vie au maximum.");
                Assert.That(panel.activeSelf, Is.False, "le respawn solo doit masquer l'overlay.");
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(hudRoot);
            }
        }

        [Test]
        public void LocalOnFootControllerMovementEnabledDefaultsTrueAndIsSettable()
        {
            var root = new GameObject("MovementEnabledTest");
            try
            {
                root.AddComponent<CharacterController>();
                var controller = root.AddComponent<LocalOnFootController>();

                Assert.That(controller.MovementEnabled, Is.True, "le mouvement doit etre actif par defaut.");

                controller.MovementEnabled = false;
                Assert.That(controller.MovementEnabled, Is.False);

                controller.MovementEnabled = true;
                Assert.That(controller.MovementEnabled, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LocalOnFootControllerUpdateGuardsOnMovementEnabledBeforeReadingInputOrGravity()
        {
            var source = File.ReadAllText("Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs");

            var updateIndex = source.IndexOf("private void Update()");
            var guardIndex = source.IndexOf("if (!MovementEnabled)");
            var stepCallIndex = source.IndexOf("Step(ReadInputIntent(), Time.deltaTime);");

            Assert.That(updateIndex, Is.GreaterThanOrEqualTo(0), "Update() doit exister.");
            Assert.That(guardIndex, Is.GreaterThan(updateIndex), "le garde-fou MovementEnabled doit etre dans Update().");
            Assert.That(stepCallIndex, Is.GreaterThan(guardIndex),
                "le garde-fou doit precede l'appel a Step() : aucune lecture d'input ni de gravite tant que le mouvement est gele.");
        }

        [Test]
        public void TeleportRepositionsAndResetsAccumulatedFallVelocityToGroundedValue()
        {
            var root = new GameObject("TeleportTest");
            try
            {
                var controller = root.AddComponent<LocalOnFootController>();

                var velocityField = typeof(LocalOnFootController).GetField("verticalVelocity", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(velocityField, Is.Not.Null, "verticalVelocity");

                // Simule une chute prolongee : une vitesse verticale enorme accumulee pendant que le
                // joueur etait "mort" -- exactement la cause racine du bug corrige (le respawn
                // relancait le joueur sous le seuil quasi aussitot).
                velocityField.SetValue(controller, -500f);

                var targetPosition = new Vector3(3f, 4f, 5f);
                var targetRotation = Quaternion.Euler(0f, 90f, 0f);
                controller.Teleport(targetPosition, targetRotation);

                Assert.That(root.transform.position, Is.EqualTo(targetPosition), "Teleport doit repositionner le rig.");
                Assert.That(Quaternion.Angle(root.transform.rotation, targetRotation), Is.LessThan(0.001f), "Teleport doit reorienter le rig.");

                var groundedVelocityField = typeof(LocalOnFootController).GetField("GroundedVerticalVelocity", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(groundedVelocityField, Is.Not.Null, "GroundedVerticalVelocity");
                var expectedGroundedVelocity = (float)groundedVelocityField.GetValue(null);

                var resetVelocity = (float)velocityField.GetValue(controller);
                Assert.That(resetVelocity, Is.EqualTo(expectedGroundedVelocity),
                    "Teleport doit remettre verticalVelocity a l'etat 'au sol' : c'est cette ligne, pas seulement le repositionnement, " +
                    "qui empeche la vitesse de chute accumulee pendant la mort de retraverser aussitot le seuil au respawn.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LocalVoidRespawnControllerUsesMovementEnabledAndTeleportInsteadOfCharacterControllerDirectly()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/LocalVoidRespawnController.cs");

            Assert.That(source, Does.Contain("onFootController.MovementEnabled = false;"));
            Assert.That(source, Does.Contain("onFootController.Teleport(spawnPosition, spawnRotation);"));
            Assert.That(source, Does.Contain("onFootController.MovementEnabled = true;"));
            Assert.That(source, Does.Not.Contain("characterController.enabled"),
                "le bug fix remplace la desactivation directe du CharacterController par le gel via LocalOnFootController.");
        }

        [Test]
        public void RunFlowFreezesAndTeleportsLocalRigOnNetworkedLifecycleTransitions()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");

            Assert.That(source, Does.Contain("localOnFootController.MovementEnabled = false;"));
            Assert.That(source, Does.Contain("localOnFootController.Teleport(spawnPosition, spawnRotation);"));
            Assert.That(source, Does.Contain("localOnFootController.MovementEnabled = true;"),
                "sans ce reveil/teleport du rig local reel, il continue de tomber et NetworkedLocalPlayerPoseReporter " +
                "re-ecrase la position que le host vient de remettre.");
        }

        private static TextMeshProUGUI AddLabel(GameObject root, string name)
        {
            var labelObject = new GameObject(name);
            labelObject.transform.SetParent(root.transform);
            return labelObject.AddComponent<TextMeshProUGUI>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }
    }
}
