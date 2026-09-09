using System.Text;
using TMPro;
using UnityEngine;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// HUD de checkpoint. Il affiche l'etat local/reseau lu par la couche App (Story 1.5, 2.5, 2.6),
    /// sans connaitre les types gameplay ni jamais ecrire l'etat de run ou de joueur reseau : chaque
    /// methode publique se limite a mettre a jour l'affichage concerne.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunCheckpointHudScreen : MonoBehaviour
    {
        public const string LocalLobbyState = "Lobby : local hors ligne";

        public const string NetworkHostLobbyState = "Lobby : en ligne (hote)";

        public const string NetworkClientLobbyState = "Lobby : en ligne (client)";

        public const string AwaitingPlayerState = "Joueur : aucun profil confirme";

        public const string FutureHudState = "HUD futur : rage, argent, actions passager";

        private const string PlaceholderValue = "-";

        private const char FilledHeartGlyph = '♥';

        private const char EmptyHeartGlyph = '♡';

        [SerializeField]
        private TMP_Text lobbyStateLabel;

        [SerializeField]
        private TMP_Text playerStateLabel;

        [SerializeField]
        private TMP_Text futureHudLabel;

        [SerializeField]
        private TMP_Text heartsLabel;

        [SerializeField]
        private TMP_Text staminaLabel;

        [SerializeField]
        private TMP_Text playerCountLabel;

        [SerializeField]
        private TMP_Text moneyLabel;

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

        /// <summary>
        /// Valeurs neutres (Story 2.6) pour vie/stamina/joueurs/argent avant que
        /// <see cref="RoadRage.App.Run.RunFlowController.TrySpawnSelectedProfile"/> ne lie l'etat reseau reel :
        /// evite que ces 4 lignes restent vides ou perimees pendant l'attente/le blocage d'entree monde.
        /// </summary>
        private void ShowPlaceholderHudValues()
        {
            SetText(heartsLabel, "Vie : " + PlaceholderValue);
            SetText(staminaLabel, "Stamina : " + PlaceholderValue);
            SetText(playerCountLabel, "Joueurs : " + PlaceholderValue);
            SetText(moneyLabel, "Argent : " + PlaceholderValue);
        }

        /// <summary>Retour visible host-only pour un spawn reseau tardif, en echec ou en doublon (Story 2.5).</summary>
        public void ShowSpawnIssue(string message)
        {
            SetText(lobbyStateLabel, "Reseau : " + SafeText(message));
        }

        /// <summary>
        /// Bind initial (Story 2.6) des valeurs placeholder de checkpoint : vie/coeurs, stamina,
        /// nombre de joueurs et argent. Chaque valeur peut ensuite etre rafraichie individuellement
        /// via les setters dedies sans reconstruire tout le HUD.
        /// </summary>
        public void ShowHudState(int hearts, int maxHearts, float staminaNormalized, int playerCount, int money)
        {
            SetHearts(hearts, maxHearts);
            SetStamina(staminaNormalized);
            SetPlayerCount(playerCount);
            SetMoney(money);
        }

        /// <summary>Met a jour uniquement l'affichage vie/coeurs (lecture seule, pas de mutation reseau).</summary>
        public void SetHearts(int hearts, int maxHearts)
        {
            SetText(heartsLabel, "Vie : " + FormatHearts(hearts, maxHearts));
        }

        /// <summary>Met a jour uniquement l'affichage stamina (lecture seule, pas de mutation reseau).</summary>
        public void SetStamina(float staminaNormalized)
        {
            var percent = Mathf.RoundToInt(Mathf.Clamp01(staminaNormalized) * 100f);
            SetText(staminaLabel, "Stamina : " + percent + "%");
        }

        /// <summary>Met a jour uniquement le nombre de joueurs affiche, sans reconstruire le HUD.</summary>
        public void SetPlayerCount(int playerCount)
        {
            SetText(playerCountLabel, "Joueurs : " + Mathf.Max(0, playerCount));
        }

        /// <summary>Met a jour uniquement l'affichage argent (lecture seule, pas de mutation reseau).</summary>
        public void SetMoney(int money)
        {
            SetText(moneyLabel, "Argent : " + money + " $");
        }

        /// <summary>Convertit une vie/maximum en glyphes de coeurs pleins/vides (pas de sprite, texte only).</summary>
        public static string FormatHearts(int hearts, int maxHearts)
        {
            var clampedMax = Mathf.Max(0, maxHearts);
            var clampedHearts = Mathf.Clamp(hearts, 0, clampedMax);

            var builder = new StringBuilder(clampedMax);
            for (var i = 0; i < clampedHearts; i++)
            {
                builder.Append(FilledHeartGlyph);
            }

            for (var i = clampedHearts; i < clampedMax; i++)
            {
                builder.Append(EmptyHeartGlyph);
            }

            return builder.ToString();
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
