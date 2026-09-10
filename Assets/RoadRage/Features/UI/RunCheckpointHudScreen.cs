using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoadRage.Features.UI
{
    [DisallowMultipleComponent]
    public sealed class RunCheckpointHudScreen : MonoBehaviour
    {
        public const string LocalLobbyState = "Lobby : local hors ligne";

        public const string NetworkHostLobbyState = "Lobby : en ligne (hote)";

        public const string NetworkClientLobbyState = "Lobby : en ligne (client)";

        public const string AwaitingPlayerState = "Joueur : aucun profil confirme";

        public const string FutureHudState = "HUD futur : rage, argent, actions passager";

        public const string DeathOverlayText = "TU ES MORT";

        public const string VehicleCollisionMessage = "Collision !";

        public const string VehicleRecoveredMessage = "Vehicule recupere.";

        public const string PlayerDownedMessage = "Joueur a terre : resurrection en cours...";

        public const string PlayerRevivedMessage = "Joueur reanime.";

        public const string VehicleInoperableMessage = "Voiture hors d'usage.";

        private const string PlaceholderValue = "-";

        [SerializeField]
        private TMP_Text lobbyStateLabel;

        [SerializeField]
        private TMP_Text playerStateLabel;

        [SerializeField]
        private TMP_Text futureHudLabel;

        [SerializeField]
        [FormerlySerializedAs("heartsLabel")]
        private TMP_Text hpLabel;

        [SerializeField]
        private TMP_Text staminaLabel;

        [SerializeField]
        private TMP_Text playerCountLabel;

        [SerializeField]
        private TMP_Text moneyLabel;

        [SerializeField]
        private GameObject deathOverlayPanel;

        [SerializeField]
        private TMP_Text deathOverlayLabel;

        private int currentHp = -1;

        private int currentMaxHp = -1;

        private void Awake()
        {
            ShowAwaitingProfile();
        }

        public void ShowAwaitingProfile()
        {
            SetText(lobbyStateLabel, LocalLobbyState);
            SetText(playerStateLabel, AwaitingPlayerState);
            SetText(futureHudLabel, FutureHudState);
            ShowPlaceholderHudValues();
        }

        public void ShowLocalRunState(string playerName, string characterName)
        {
            ShowRunState(LocalLobbyState, playerName, characterName);
        }

        public void ShowRunState(string lobbyState, string playerName, string characterName)
        {
            SetText(lobbyStateLabel, SafeText(lobbyState));
            SetText(playerStateLabel, "Joueur : " + SafeText(playerName) + " / " + SafeText(characterName));
            SetText(futureHudLabel, FutureHudState);
            ShowPlaceholderHudValues();
        }

        public void ShowBlockedState(string reason)
        {
            SetText(lobbyStateLabel, LocalLobbyState);
            SetText(playerStateLabel, "Joueur : entree monde bloquee - " + SafeText(reason));
            SetText(futureHudLabel, FutureHudState);
            ShowPlaceholderHudValues();
        }

        private void ShowPlaceholderHudValues()
        {
            currentHp = -1;
            currentMaxHp = -1;
            SetText(hpLabel, "HP : " + PlaceholderValue);
            SetText(staminaLabel, "Stamina : " + PlaceholderValue);
            SetText(playerCountLabel, "Joueurs : " + PlaceholderValue);
            SetText(moneyLabel, "Argent : " + PlaceholderValue);
        }

        public void ShowSpawnIssue(string message)
        {
            SetText(lobbyStateLabel, "Reseau : " + SafeText(message));
        }

        public void ShowVehicleSeatMessage(string message)
        {
            SetText(futureHudLabel, "Vehicule : " + SafeText(message));
        }

        public void ShowVehicleCollisionMessage()
        {
            ShowVehicleSeatMessage(VehicleCollisionMessage);
        }

        public void ShowVehicleRecoveredMessage()
        {
            ShowVehicleSeatMessage(VehicleRecoveredMessage);
        }

        public void ShowPlayerDownedMessage()
        {
            ShowVehicleSeatMessage(PlayerDownedMessage);
        }

        public void ShowPlayerRevivedMessage()
        {
            ShowVehicleSeatMessage(PlayerRevivedMessage);
        }

        public void ShowVehicleInoperableMessage()
        {
            ShowVehicleSeatMessage(VehicleInoperableMessage);
        }

        public void ShowPlayerDownedCountdown(float secondsRemaining)
        {
            ShowVehicleSeatMessage("Joueur a terre : " + Mathf.CeilToInt(Mathf.Max(0f, secondsRemaining)) + " s");
        }

        public void SetVehicleDamageStatus(int hp, int maxHp, bool wheelDamaged, bool engineDamaged, bool brakeDamaged)
        {
            var safeMaxHp = Mathf.Max(1, maxHp);
            var safeHp = Mathf.Clamp(hp, 0, safeMaxHp);
            SetText(futureHudLabel, "Vehicule : HP " + safeHp + "/" + safeMaxHp + " | Degats : " + FormatVehicleDamageTypes(wheelDamaged, engineDamaged, brakeDamaged));
        }

        public void ShowHudState(int hp, int maxHp, float staminaNormalized, int playerCount, int money)
        {
            SetHp(hp, maxHp);
            SetStamina(staminaNormalized);
            SetPlayerCount(playerCount);
            SetMoney(money);
        }

        public void SetHp(int hp, int maxHp)
        {
            currentHp = hp;
            currentMaxHp = maxHp;
            RenderHpLabel();
        }

        public void SetStamina(float staminaNormalized)
        {
            var percent = Mathf.RoundToInt(Mathf.Clamp01(staminaNormalized) * 100f);
            SetText(staminaLabel, "Stamina : " + percent + "%");
        }

        public void SetPlayerCount(int playerCount)
        {
            SetText(playerCountLabel, "Joueurs : " + Mathf.Max(0, playerCount));
        }

        public void SetMoney(int money)
        {
            SetText(moneyLabel, "Argent : " + money + " $");
        }

        public void ShowDeathOverlay()
        {
            SetText(deathOverlayLabel, DeathOverlayText);

            if (deathOverlayPanel != null)
            {
                deathOverlayPanel.SetActive(true);
            }
        }

        public void HideDeathOverlay()
        {
            if (deathOverlayPanel != null)
            {
                deathOverlayPanel.SetActive(false);
            }
        }

        public static string FormatHp(int hp, int maxHp)
        {
            var clampedMax = Mathf.Max(0, maxHp);
            var clampedHp = Mathf.Clamp(hp, 0, clampedMax);
            return clampedHp + "/" + clampedMax;
        }

        public static string FormatVehicleDamageTypes(bool wheelDamaged, bool engineDamaged, bool brakeDamaged)
        {
            var builder = new StringBuilder();
            AppendDamageType(builder, wheelDamaged, "roue");
            AppendDamageType(builder, engineDamaged, "moteur");
            AppendDamageType(builder, brakeDamaged, "frein");
            return builder.Length == 0 ? "aucun" : builder.ToString();
        }

        private void RenderHpLabel()
        {
            if (currentHp >= 0 && currentMaxHp >= 0)
            {
                SetText(hpLabel, "HP : " + FormatHp(currentHp, currentMaxHp));
                return;
            }

            SetText(hpLabel, "HP : " + PlaceholderValue);
        }

        private static void AppendDamageType(StringBuilder builder, bool active, string label)
        {
            if (!active)
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(label);
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private static string SafeText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "indisponible" : value;
        }
    }
}
