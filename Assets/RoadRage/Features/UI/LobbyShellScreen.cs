using System;
using RoadRage.Shared.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// Ecran UGUI de la coquille de lobby locale (Story 1.2). N'appelle jamais de chargement de scene
    /// direct et ne mute jamais l'objet de reglages de partie du feature Lobby : il n'emet que des
    /// intentions, la couche App traduit vers ce feature.
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
        private Button characterSetupButton;

        [SerializeField]
        private TMP_Text settingsSummaryLabel;

        private Difficulty displayedDifficulty = Difficulty.Normal;

        public event Action CreateLobbyRequested;

        public event Action<string> JoinByCodeRequested;

        public event Action StartGameRequested;

        public event Action<Difficulty> DifficultyChanged;

        /// <summary>Point d'entree du flux de setup de personnage (Story 1.3).</summary>
        public event Action CharacterSetupRequested;

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

            if (characterSetupButton != null)
            {
                characterSetupButton.onClick.AddListener(RaiseCharacterSetupRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyShellScreen sans reference vers characterSetupButton.");
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

        private void RaiseCharacterSetupRequested()
        {
            CharacterSetupRequested?.Invoke();
        }
    }
}
