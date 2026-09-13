using System;
using RoadRage.Shared.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// Ecran UGUI de la coquille de lobby locale (Story 1.2) : creation/join de room et jeu solo
    /// local (difficulte + Start Game). Le choix de personnage n'est plus expose ici depuis la
    /// Story 4.5 : le menu principal est la seule surface de selection, et ce panneau ne fait que
    /// refléter le profil deja resolu.
    /// Depuis la Story 2.4, cet ecran ne represente plus le
    /// "lobby" une fois une room active : LobbyFlowController le masque au profit de LobbyRosterScreen
    /// des qu'une room est ouverte ou rejointe, et le restaure a la fermeture. N'appelle jamais de
    /// chargement de scene direct et ne mute jamais l'objet de reglages de partie du feature Lobby : il
    /// n'emet que des intentions, la couche App traduit vers ce feature.
    /// Le bouton Back n'appartient pas a cet ecran : il reste possede par MainMenuScreen, qui gere deja
    /// l'alternance menu/setup. Ne pas y ajouter de reference ni d'evenement Back en double.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LobbyShellScreen : MonoBehaviour
    {
        private static readonly Difficulty[] CycleOrder = { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard };

        [SerializeField]
        private Button createLobbyButton;

        [SerializeField]
        private TMP_Text createLobbyButtonLabel;

        [SerializeField]
        private TMP_Text roomCodeLabel;

        [SerializeField]
        private Button joinByCodeButton;

        [SerializeField]
        private TMP_InputField joinCodeInputField;

        [SerializeField]
        private Button startGameButton;

        [SerializeField]
        private Button difficultyButton;

        [SerializeField]
        private TMP_Text settingsSummaryLabel;

        private Difficulty displayedDifficulty = Difficulty.Normal;

        public event Action CreateLobbyRequested;

        public event Action<string> JoinByCodeRequested;

        public event Action StartGameRequested;

        public event Action<Difficulty> DifficultyChanged;

        private void Awake()
        {
            if (createLobbyButton != null)
            {
                createLobbyButton.onClick.AddListener(RaiseCreateLobbyRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers createLobbyButton.");
            }

            if (joinByCodeButton != null)
            {
                joinByCodeButton.onClick.AddListener(RaiseJoinByCodeRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers joinByCodeButton.");
            }

            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(RaiseStartGameRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers startGameButton.");
            }

            if (difficultyButton != null)
            {
                difficultyButton.onClick.AddListener(CycleDifficulty);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers difficultyButton.");
            }

            if (settingsSummaryLabel == null)
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers settingsSummaryLabel.");
            }

            if (createLobbyButtonLabel == null)
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers createLobbyButtonLabel.");
            }

            if (roomCodeLabel == null)
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers roomCodeLabel.");
            }

            if (joinCodeInputField == null)
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers joinCodeInputField.");
            }

            // Volontairement aucun appel a ShowSettingsSummary ici : la couche App (LobbyFlowController)
            // est le seul ecrivain du libelle, a partir de l'etat reel des reglages de partie. Cet ecran
            // vit sur un panneau inactif au chargement, donc son Awake s'execute apres celui du controller :
            // ecrire ici reintroduirait un second ecrivain, d'ordre non garanti, affichant un etat perime.
        }

        /// <summary>
        /// Synchronise le libelle affiche sur la difficulte courante. Appele par la couche App
        /// apres mise a jour des reglages de partie : l'ecran ne mute jamais cet objet lui-meme.
        /// </summary>
        public void ShowSettingsSummary(Difficulty difficulty)
        {
            displayedDifficulty = difficulty;

            if (settingsSummaryLabel != null)
            {
                settingsSummaryLabel.text = "Difficulte : " + difficulty;
            }
        }

        /// <summary>
        /// Affiche la room hote comme ouverte avec son code de join. Appele par la couche App
        /// (LobbyFlowController) une fois la creation reussie : l'ecran ne mute jamais LobbyRoomService.
        /// </summary>
        public void ShowRoomCreated(string joinCode)
        {
            if (createLobbyButtonLabel != null)
            {
                createLobbyButtonLabel.text = "Close Room";
            }

            if (roomCodeLabel != null)
            {
                roomCodeLabel.text = "Code : " + joinCode;
            }
        }

        /// <summary>
        /// Affiche la room hote comme fermee (etat initial, fermeture explicite, ou echec de creation).
        /// Appele par la couche App : l'ecran ne mute jamais LobbyRoomService.
        /// </summary>
        public void ShowRoomClosed()
        {
            if (createLobbyButtonLabel != null)
            {
                createLobbyButtonLabel.text = "Create Lobby";
            }

            if (roomCodeLabel != null)
            {
                roomCodeLabel.text = string.Empty;
            }
        }

        /// <summary>
        /// Affiche le lobby rejoint avec succes (Story 2.3). Distinct de ShowRoomCreated : le joueur
        /// invite n'est jamais proprietaire du bouton Create/Close Room, seul son libelle de code change.
        /// Appele par la couche App (LobbyFlowController) une fois le join reussi.
        /// </summary>
        public void ShowJoinedRoom(string joinCode)
        {
            if (roomCodeLabel != null)
            {
                roomCodeLabel.text = "Rejoint : " + joinCode;
            }
        }

        /// <summary>Masque ce panneau au profit de LobbyRosterScreen des qu'une room devient active (Story 2.4).</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>Restaure ce panneau (jeu solo, ou room hote fermee) (Story 2.4).</summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        private string GetRawJoinCode()
        {
            return joinCodeInputField == null ? string.Empty : joinCodeInputField.text;
        }

        private void CycleDifficulty()
        {
            var currentIndex = Array.IndexOf(CycleOrder, displayedDifficulty);
            var nextIndex = (currentIndex + 1) % CycleOrder.Length;
            var next = CycleOrder[nextIndex];

            DifficultyChanged?.Invoke(next);
        }

        private void RaiseCreateLobbyRequested()
        {
            CreateLobbyRequested?.Invoke();
        }

        private void RaiseJoinByCodeRequested()
        {
            JoinByCodeRequested?.Invoke(GetRawJoinCode());
        }

        private void RaiseStartGameRequested()
        {
            StartGameRequested?.Invoke();
        }
    }
}
