using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.6 : conversion vie/coeurs, statut reseau solo/hote/client, mise a jour
    /// ciblee (nombre de joueurs, vie/stamina/argent) sans reconstruire le HUD, et l'invariant
    /// lecture-seule stricte (aucun setter public exploitable sur NetworkedPlayerState, aucune
    /// ecriture directe de NetworkVariable depuis le HUD/RunFlowController).
    /// </summary>
    public sealed class Story26InGameHudTests
    {
        [Test]
        public void FormatHeartsRendersFilledAndEmptyGlyphs()
        {
            Assert.That(RunCheckpointHudScreen.FormatHearts(2, 3), Is.EqualTo("♥♥♡"));
            Assert.That(RunCheckpointHudScreen.FormatHearts(0, 3), Is.EqualTo("♡♡♡"));
            Assert.That(RunCheckpointHudScreen.FormatHearts(3, 3), Is.EqualTo("♥♥♥"));
        }

        [Test]
        public void FormatHeartsClampsOutOfRangeValuesWithoutThrowing()
        {
            Assert.That(RunCheckpointHudScreen.FormatHearts(5, 3), Is.EqualTo("♥♥♥"));
            Assert.That(RunCheckpointHudScreen.FormatHearts(-1, 3), Is.EqualTo("♡♡♡"));
            Assert.That(RunCheckpointHudScreen.FormatHearts(0, 0), Is.Empty);
        }

        [Test]
        public void ShowHudStateBindsHeartsStaminaPlayerCountAndMoneyPlaceholders()
        {
            var root = new GameObject("HudTest");
            try
            {
                var hud = root.AddComponent<RunCheckpointHudScreen>();
                var heartsLabel = AddLabel(root, "HeartsLabel");
                var staminaLabel = AddLabel(root, "StaminaLabel");
                var playerCountLabel = AddLabel(root, "PlayerCountLabel");
                var moneyLabel = AddLabel(root, "MoneyLabel");

                SetPrivateField(hud, "heartsLabel", heartsLabel);
                SetPrivateField(hud, "staminaLabel", staminaLabel);
                SetPrivateField(hud, "playerCountLabel", playerCountLabel);
                SetPrivateField(hud, "moneyLabel", moneyLabel);

                hud.ShowHudState(2, 3, 0.5f, 3, 150);

                Assert.That(heartsLabel.text, Is.EqualTo("Vie : ♥♥♡"));
                Assert.That(staminaLabel.text, Is.EqualTo("Stamina : 50%"));
                Assert.That(playerCountLabel.text, Is.EqualTo("Joueurs : 3"));
                Assert.That(moneyLabel.text, Is.EqualTo("Argent : 150 $"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SetPlayerCountUpdatesOnlyThePlayerCountLabel()
        {
            var root = new GameObject("HudTest");
            try
            {
                var hud = root.AddComponent<RunCheckpointHudScreen>();
                var heartsLabel = AddLabel(root, "HeartsLabel");
                var staminaLabel = AddLabel(root, "StaminaLabel");
                var playerCountLabel = AddLabel(root, "PlayerCountLabel");
                var moneyLabel = AddLabel(root, "MoneyLabel");

                SetPrivateField(hud, "heartsLabel", heartsLabel);
                SetPrivateField(hud, "staminaLabel", staminaLabel);
                SetPrivateField(hud, "playerCountLabel", playerCountLabel);
                SetPrivateField(hud, "moneyLabel", moneyLabel);

                hud.ShowHudState(2, 3, 0.5f, 1, 150);

                hud.SetPlayerCount(4);

                Assert.That(playerCountLabel.text, Is.EqualTo("Joueurs : 4"));
                Assert.That(heartsLabel.text, Is.EqualTo("Vie : ♥♥♡"), "seul le compteur de joueurs doit changer.");
                Assert.That(staminaLabel.text, Is.EqualTo("Stamina : 50%"), "seul le compteur de joueurs doit changer.");
                Assert.That(moneyLabel.text, Is.EqualTo("Argent : 150 $"), "seul le compteur de joueurs doit changer.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SetHeartsUpdatesOnlyTheHeartsLabel()
        {
            var root = new GameObject("HudTest");
            try
            {
                var hud = root.AddComponent<RunCheckpointHudScreen>();
                var heartsLabel = AddLabel(root, "HeartsLabel");
                var staminaLabel = AddLabel(root, "StaminaLabel");
                var moneyLabel = AddLabel(root, "MoneyLabel");

                SetPrivateField(hud, "heartsLabel", heartsLabel);
                SetPrivateField(hud, "staminaLabel", staminaLabel);
                SetPrivateField(hud, "moneyLabel", moneyLabel);

                hud.ShowHudState(3, 3, 1f, 1, 0);

                hud.SetHearts(1, 3);

                Assert.That(heartsLabel.text, Is.EqualTo("Vie : ♥♡♡"));
                Assert.That(staminaLabel.text, Is.EqualTo("Stamina : 100%"), "seule la vie doit changer.");
                Assert.That(moneyLabel.text, Is.EqualTo("Argent : 0 $"), "seule la vie doit changer.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void NetworkStatusConstantsAreDistinctForSoloHostAndClient()
        {
            Assert.That(RunCheckpointHudScreen.LocalLobbyState, Is.Not.EqualTo(RunCheckpointHudScreen.NetworkHostLobbyState));
            Assert.That(RunCheckpointHudScreen.LocalLobbyState, Is.Not.EqualTo(RunCheckpointHudScreen.NetworkClientLobbyState));
            Assert.That(RunCheckpointHudScreen.NetworkHostLobbyState, Is.Not.EqualTo(RunCheckpointHudScreen.NetworkClientLobbyState));
        }

        [Test]
        public void ShowAwaitingProfileDefaultsToLocalSoloNetworkStatus()
        {
            var root = new GameObject("HudTest");
            try
            {
                var hud = root.AddComponent<RunCheckpointHudScreen>();
                var lobbyLabel = AddLabel(root, "LobbyLabel");
                SetPrivateField(hud, "lobbyStateLabel", lobbyLabel);

                hud.ShowAwaitingProfile();

                Assert.That(lobbyLabel.text, Is.EqualTo(RunCheckpointHudScreen.LocalLobbyState));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void NetworkedPlayerStateExposesHeartsStaminaAndMoneyAsServerWriteNetworkVariables()
        {
            var source = File.ReadAllText("Assets/RoadRage/Features/Players/NetworkedPlayerState.cs");

            Assert.That(source, Does.Contain("NetworkVariable<int> MaxHearts"));
            Assert.That(source, Does.Contain("NetworkVariable<int> Hearts"));
            Assert.That(source, Does.Contain("NetworkVariable<float> StaminaNormalized"));
            Assert.That(source, Does.Contain("NetworkVariable<int> Money"));
        }

        [Test]
        public void NetworkedPlayerStateExposesNoPublicSetterMethod()
        {
            var declaredMethods = typeof(NetworkedPlayerState).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.That(declaredMethods, Is.Empty,
                "NetworkedPlayerState ne doit exposer aucune methode publique : seules des NetworkVariable en champ public, ecrites uniquement par le host (Story 2.5/2.6).");
        }

        [Test]
        public void HudAndRunFlowNeverWriteNetworkedPlayerStateNetworkVariables()
        {
            var hudSource = File.ReadAllText("Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs");
            var runFlowSource = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");

            foreach (var variableName in new[] { "Hearts", "MaxHearts", "StaminaNormalized", "Money" })
            {
                Assert.That(hudSource, Does.Not.Contain(variableName + ".Value ="), "le HUD ne doit jamais ecrire " + variableName + ".");
                Assert.That(runFlowSource, Does.Not.Contain(variableName + ".Value ="), "RunFlowController ne doit jamais ecrire " + variableName + ".");
            }
        }

        [Test]
        public void RunFlowResolvesLocalNetworkedPlayerStateByClientIdAndSpawnState()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");

            Assert.That(source, Does.Contain("candidate.IsSpawned && candidate.ClientId.Value == localClientId"));
            Assert.That(source, Does.Contain("state.Hearts.OnValueChanged += HandleHeartsChanged"));
            Assert.That(source, Does.Contain("localNetworkedPlayerState.Hearts.OnValueChanged -= HandleHeartsChanged"));
        }

        [Test]
        public void HandleNetworkPlayerCountChangedIgnoresCallbackBeforeHudIsBound()
        {
            var root = new GameObject("RunFlowHudBridgeTest");
            var hudRoot = new GameObject("HudBridgeTest");
            try
            {
                var controller = root.AddComponent<RunFlowController>();
                var hud = hudRoot.AddComponent<RunCheckpointHudScreen>();
                var playerCountLabel = AddLabel(hudRoot, "PlayerCountLabel");
                SetPrivateField(hud, "playerCountLabel", playerCountLabel);
                SetPrivateField(controller, "checkpointHud", hud);

                Assert.That(string.IsNullOrEmpty(playerCountLabel.text), Is.True, "aucune valeur ne doit etre affichee avant liaison du HUD.");

                InvokePrivateMethod(controller, "HandleNetworkPlayerCountChanged", 0UL);

                Assert.That(string.IsNullOrEmpty(playerCountLabel.text), Is.True, "le callback doit etre ignore si le HUD n'est pas encore lie (spawn pas termine).");
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(hudRoot);
            }
        }

        [Test]
        public void HandleNetworkPlayerCountChangedUpdatesOnlyPlayerCountLabelWhenHudIsBound()
        {
            var root = new GameObject("RunFlowHudBridgeTest");
            var hudRoot = new GameObject("HudBridgeTest");
            try
            {
                var controller = root.AddComponent<RunFlowController>();
                var hud = hudRoot.AddComponent<RunCheckpointHudScreen>();
                var playerCountLabel = AddLabel(hudRoot, "PlayerCountLabel");
                var heartsLabel = AddLabel(hudRoot, "HeartsLabel");
                SetPrivateField(hud, "playerCountLabel", playerCountLabel);
                SetPrivateField(hud, "heartsLabel", heartsLabel);
                SetPrivateField(controller, "checkpointHud", hud);
                SetPrivateField(controller, "hudRuntimeBound", true);

                InvokePrivateMethod(controller, "HandleNetworkPlayerCountChanged", 0UL);

                var expectedCount = 1;
                var manager = NetworkManager.Singleton;
                if (manager != null && manager.IsListening)
                {
                    expectedCount = manager.ConnectedClientsIds.Count;
                }

                Assert.That(playerCountLabel.text, Is.EqualTo("Joueurs : " + expectedCount));
                Assert.That(string.IsNullOrEmpty(heartsLabel.text), Is.True, "seul le compteur de joueurs doit etre touche par ce callback.");
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(hudRoot);
            }
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

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, ToTypes(args), null);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, args);
        }

        private static System.Type[] ToTypes(object[] args)
        {
            var types = new System.Type[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                types[i] = args[i].GetType();
            }

            return types;
        }
    }
}
