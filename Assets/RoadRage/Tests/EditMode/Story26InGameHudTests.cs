using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 2.6, mise a jour par la Story 3.5 : statut reseau solo/hote/client, HUD
    /// cible (HP numeriques, stamina, joueurs, argent), stamina de sprint locale, et invariant
    /// lecture-seule stricte cote HUD.
    /// </summary>
    public sealed class Story26InGameHudTests
    {
        [Test]
        public void FormatHpRendersClampedNumericValue()
        {
            Assert.That(RunCheckpointHudScreen.FormatHp(75, 100), Is.EqualTo("75/100"));
            Assert.That(RunCheckpointHudScreen.FormatHp(150, 100), Is.EqualTo("100/100"));
            Assert.That(RunCheckpointHudScreen.FormatHp(-10, 100), Is.EqualTo("0/100"));
            Assert.That(RunCheckpointHudScreen.FormatHp(0, 0), Is.EqualTo("0/0"));
        }

        [Test]
        public void ShowHudStateBindsHpStaminaPlayerCountAndMoneyPlaceholders()
        {
            var root = new GameObject("HudTest");
            try
            {
                var hud = root.AddComponent<RunCheckpointHudScreen>();
                var hpLabel = AddLabel(root, "HpLabel");
                var staminaLabel = AddLabel(root, "StaminaLabel");
                var playerCountLabel = AddLabel(root, "PlayerCountLabel");
                var moneyLabel = AddLabel(root, "MoneyLabel");

                SetPrivateField(hud, "hpLabel", hpLabel);
                SetPrivateField(hud, "staminaLabel", staminaLabel);
                SetPrivateField(hud, "playerCountLabel", playerCountLabel);
                SetPrivateField(hud, "moneyLabel", moneyLabel);

                hud.ShowHudState(75, 100, 0.5f, 3, 150);

                Assert.That(hpLabel.text, Is.EqualTo("HP : 75/100"));
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
                var hpLabel = AddLabel(root, "HpLabel");
                var staminaLabel = AddLabel(root, "StaminaLabel");
                var playerCountLabel = AddLabel(root, "PlayerCountLabel");
                var moneyLabel = AddLabel(root, "MoneyLabel");

                SetPrivateField(hud, "hpLabel", hpLabel);
                SetPrivateField(hud, "staminaLabel", staminaLabel);
                SetPrivateField(hud, "playerCountLabel", playerCountLabel);
                SetPrivateField(hud, "moneyLabel", moneyLabel);

                hud.ShowHudState(80, 100, 0.5f, 1, 150);

                hud.SetPlayerCount(4);

                Assert.That(playerCountLabel.text, Is.EqualTo("Joueurs : 4"));
                Assert.That(hpLabel.text, Is.EqualTo("HP : 80/100"), "seul le compteur de joueurs doit changer.");
                Assert.That(staminaLabel.text, Is.EqualTo("Stamina : 50%"), "seul le compteur de joueurs doit changer.");
                Assert.That(moneyLabel.text, Is.EqualTo("Argent : 150 $"), "seul le compteur de joueurs doit changer.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SetHpUpdatesOnlyTheHpLabel()
        {
            var root = new GameObject("HudTest");
            try
            {
                var hud = root.AddComponent<RunCheckpointHudScreen>();
                var hpLabel = AddLabel(root, "HpLabel");
                var staminaLabel = AddLabel(root, "StaminaLabel");
                var moneyLabel = AddLabel(root, "MoneyLabel");

                SetPrivateField(hud, "hpLabel", hpLabel);
                SetPrivateField(hud, "staminaLabel", staminaLabel);
                SetPrivateField(hud, "moneyLabel", moneyLabel);

                hud.ShowHudState(100, 100, 1f, 1, 0);

                hud.SetHp(35, 100);

                Assert.That(hpLabel.text, Is.EqualTo("HP : 35/100"));
                Assert.That(staminaLabel.text, Is.EqualTo("Stamina : 100%"), "seuls les HP doivent changer.");
                Assert.That(moneyLabel.text, Is.EqualTo("Argent : 0 $"), "seuls les HP doivent changer.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LocalOnFootControllerConsumesSprintStaminaAndBlocksSprintAtZero()
        {
            Assert.That(LocalOnFootController.ShouldSprint(Vector2.up, true, false, 1f), Is.True);
            Assert.That(LocalOnFootController.ShouldSprint(Vector2.up, true, false, 0f), Is.False,
                "a 0 stamina, le joueur ne peut plus courir.");
            Assert.That(LocalOnFootController.ShouldSprint(Vector2.up, true, true, 1f), Is.False,
                "Downed garde son plafond de vitesse et ne court pas.");
            Assert.That(LocalOnFootController.ShouldSprint(Vector2.zero, true, false, 1f), Is.False,
                "tenir sprint sans mouvement ne consomme pas de stamina.");

            Assert.That(LocalOnFootController.ComputeNextStamina(1f, true, false, 1f, 0.28f, 0.22f), Is.EqualTo(0.72f).Within(0.001f));
            Assert.That(LocalOnFootController.ComputeNextStamina(0.5f, false, false, 1f, 0.28f, 0.22f), Is.EqualTo(0.5f).Within(0.001f),
                "avant le delai de recuperation, la stamina ne remonte pas.");
            Assert.That(LocalOnFootController.ComputeNextStamina(0.5f, false, true, 1f, 0.28f, 0.22f), Is.EqualTo(0.72f).Within(0.001f),
                "apres le delai de recuperation, la stamina remonte progressivement.");
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
        public void NetworkedPlayerStateExposesHpHeartsStaminaAndMoneyAsServerWriteNetworkVariables()
        {
            var source = File.ReadAllText("Assets/RoadRage/Features/Players/NetworkedPlayerState.cs");

            Assert.That(source, Does.Contain("NetworkVariable<int> Hp"));
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
                "NetworkedPlayerState ne doit exposer aucune methode publique : seules des NetworkVariable en champ public, ecrites uniquement par le host.");
        }

        [Test]
        public void HudAndRunFlowNeverWriteNetworkedPlayerStateNetworkVariables()
        {
            var hudSource = File.ReadAllText("Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs");
            var runFlowSource = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");

            foreach (var variableName in new[] { "Hp", "Hearts", "MaxHearts", "StaminaNormalized", "Money" })
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
            Assert.That(source, Does.Contain("state.Hp.OnValueChanged += HandleHpChanged"));
            Assert.That(source, Does.Contain("localNetworkedPlayerState.Hp.OnValueChanged -= HandleHpChanged"));
        }

        [Test]
        public void RunFlowSubscribesToLocalOnFootStaminaForLiveHud()
        {
            var source = File.ReadAllText("Assets/RoadRage/App/Run/RunFlowController.cs");

            Assert.That(source, Does.Contain("activeLocalOnFootController.StaminaChanged += HandleLocalStaminaChanged"));
            Assert.That(source, Does.Contain("activeLocalOnFootController.StaminaChanged -= HandleLocalStaminaChanged"));
            Assert.That(source, Does.Contain("checkpointHud.SetStamina(staminaNormalized);"));
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

                Assert.That(string.IsNullOrEmpty(playerCountLabel.text), Is.True, "le callback doit etre ignore si le HUD n'est pas encore lie.");
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
                var hpLabel = AddLabel(hudRoot, "HpLabel");
                SetPrivateField(hud, "playerCountLabel", playerCountLabel);
                SetPrivateField(hud, "hpLabel", hpLabel);
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
                Assert.That(string.IsNullOrEmpty(hpLabel.text), Is.True, "seul le compteur de joueurs doit etre touche par ce callback.");
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
