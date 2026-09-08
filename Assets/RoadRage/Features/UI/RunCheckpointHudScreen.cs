using TMPro;
using UnityEngine;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// HUD placeholder du checkpoint Epic 1. Il affiche l'etat local lu par la couche App,
    /// sans connaitre les types gameplay ni ecrire l'etat de run.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunCheckpointHudScreen : MonoBehaviour
    {
        public const string LocalLobbyState = "Lobby : local hors ligne";

        public const string AwaitingPlayerState = "Joueur : aucun profil confirme";

        public const string FutureHudState = "HUD futur : rage, argent, actions passager";

        [SerializeField]
        private TMP_Text lobbyStateLabel;

        [SerializeField]
        private TMP_Text playerStateLabel;

        [SerializeField]
        private TMP_Text futureHudLabel;

        private void Awake()
        {
            ShowAwaitingProfile();
        }

        public void ShowAwaitingProfile()
        {
            SetText(lobbyStateLabel, LocalLobbyState);
            SetText(playerStateLabel, AwaitingPlayerState);
            SetText(futureHudLabel, FutureHudState);
        }

        public void ShowLocalRunState(string playerName, string characterName)
        {
            SetText(lobbyStateLabel, LocalLobbyState);
            SetText(playerStateLabel, "Joueur : " + SafeText(playerName) + " / " + SafeText(characterName));
            SetText(futureHudLabel, FutureHudState);
        }

        public void ShowBlockedState(string reason)
        {
            SetText(lobbyStateLabel, LocalLobbyState);
            SetText(playerStateLabel, "Joueur : entree monde bloquee - " + SafeText(reason));
            SetText(futureHudLabel, FutureHudState);
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
