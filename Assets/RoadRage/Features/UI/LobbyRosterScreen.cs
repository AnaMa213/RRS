using System;
using RoadRage.Shared.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// Ecran UGUI dedie au lobby une fois une room hote ouverte ou rejointe (Story 2.4) : roster
    /// jusqu'a quatre joueurs (personnage + nom + etat pret), bouton pret, difficulte (hote), exception
    /// de test solo, Start Game et fermeture de room. LobbyFlowController masque LobbyShellScreen et
    /// affiche cet ecran des qu'une room devient active, et inversement a la fermeture. N'appelle jamais
    /// de chargement de scene direct, n'emet que des intentions.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LobbyRosterScreen : MonoBehaviour
    {
        private static readonly Difficulty[] CycleOrder = { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard };

        [SerializeField]
        private TMP_Text roomCodeLabel;

        [SerializeField]
        private LobbyPlayerSlotView[] slots = new LobbyPlayerSlotView[4];

        [SerializeField]
        private Button readyButton;

        [SerializeField]
        private TMP_Text readyButtonLabel;

        [SerializeField]
        private Button difficultyButton;

        [SerializeField]
        private TMP_Text settingsSummaryLabel;

        [SerializeField]
        private TMP_Text trafficVehiclesLabel;

        [SerializeField]
        private Button trafficVehiclesDecreaseButton;

        [SerializeField]
        private Button trafficVehiclesIncreaseButton;

        [SerializeField]
        private TMP_Text litterThrowersLabel;

        [SerializeField]
        private Button litterThrowersDecreaseButton;

        [SerializeField]
        private Button litterThrowersIncreaseButton;

        [SerializeField]
        private Button soloTestExceptionButton;

        [SerializeField]
        private TMP_Text soloTestExceptionButtonLabel;

        [SerializeField]
        private Button startGameButton;

        [SerializeField]
        private Button closeRoomButton;

        private Difficulty displayedDifficulty = Difficulty.Normal;

        public event Action ReadyToggleRequested;

        public event Action<Difficulty> DifficultyChanged;

        /// <summary>
        /// Story 5.16 : pas demande sur l'effectif de vehicules IA (+1 ou -1). L'ecran ne decide ni
        /// borne ni refus : LobbyFlowController valide contre le Def et publie un refus visible.
        /// </summary>
        public event Action<int> AiVehicleTargetStepRequested;

        /// <summary>Story 5.16 : pas demande sur le nombre de jeteurs de detritus (+1 ou -1).</summary>
        public event Action<int> LitterThrowerStepRequested;

        /// <summary>Demande de bascule de l'exception de test solo qui contourne le gate "tous prets" (Story 2.4).</summary>
        public event Action SoloTestExceptionToggleRequested;

        public event Action StartGameRequested;

        /// <summary>Demande de fermeture de la room hote depuis l'ecran de lobby. Sans effet cote invite.</summary>
        public event Action CloseRoomRequested;

        private void Awake()
        {
            if (readyButton != null)
            {
                readyButton.onClick.AddListener(RaiseReadyToggleRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers readyButton.");
            }

            if (difficultyButton != null)
            {
                difficultyButton.onClick.AddListener(CycleDifficulty);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers difficultyButton.");
            }

            if (soloTestExceptionButton != null)
            {
                soloTestExceptionButton.onClick.AddListener(RaiseSoloTestExceptionToggleRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers soloTestExceptionButton.");
            }

            if (trafficVehiclesDecreaseButton != null)
            {
                trafficVehiclesDecreaseButton.onClick.AddListener(RaiseAiVehicleTargetDecreaseRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers trafficVehiclesDecreaseButton.");
            }

            if (trafficVehiclesIncreaseButton != null)
            {
                trafficVehiclesIncreaseButton.onClick.AddListener(RaiseAiVehicleTargetIncreaseRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers trafficVehiclesIncreaseButton.");
            }

            if (litterThrowersDecreaseButton != null)
            {
                litterThrowersDecreaseButton.onClick.AddListener(RaiseLitterThrowerDecreaseRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers litterThrowersDecreaseButton.");
            }

            if (litterThrowersIncreaseButton != null)
            {
                litterThrowersIncreaseButton.onClick.AddListener(RaiseLitterThrowerIncreaseRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers litterThrowersIncreaseButton.");
            }

            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(RaiseStartGameRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers startGameButton.");
            }

            if (closeRoomButton != null)
            {
                closeRoomButton.onClick.AddListener(RaiseCloseRoomRequested);
            }
            else
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers closeRoomButton.");
            }

            if (roomCodeLabel == null)
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers roomCodeLabel.");
            }

            if (readyButtonLabel == null)
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers readyButtonLabel.");
            }

            if (settingsSummaryLabel == null)
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers settingsSummaryLabel.");
            }

            if (trafficVehiclesLabel == null)
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers trafficVehiclesLabel.");
            }

            if (litterThrowersLabel == null)
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers litterThrowersLabel.");
            }

            if (soloTestExceptionButtonLabel == null)
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans reference vers soloTestExceptionButtonLabel.");
            }

            if (slots == null || slots.Length == 0)
            {
                Debug.LogWarning("[UI] LobbyRosterScreen sans slots de roster assignes.");
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ShowRoomCode(string code)
        {
            if (roomCodeLabel != null)
            {
                roomCodeLabel.text = "Code : " + code;
                var clipboard = roomCodeLabel.GetComponent<LobbyCodeClipboard>();
                if (clipboard == null)
                {
                    clipboard = roomCodeLabel.gameObject.AddComponent<LobbyCodeClipboard>();
                }

                clipboard.SetCode(code);
            }
        }

        /// <summary>
        /// Synchronise le libelle du bouton pret sur l'etat pret local courant. Appele par la couche App
        /// apres bascule : l'ecran ne mute jamais LobbyRosterService lui-meme.
        /// </summary>
        public void ShowReadyState(bool ready)
        {
            if (readyButtonLabel != null)
            {
                readyButtonLabel.text = ready ? "Pret !" : "Marquer pret";
            }
        }

        public void ShowSettingsSummary(Difficulty difficulty)
        {
            displayedDifficulty = difficulty;

            if (settingsSummaryLabel != null)
            {
                settingsSummaryLabel.text = "Difficulte : " + difficulty;
            }
        }

        /// <summary>Seul l'hote d'une room ouverte peut editer la difficulte ; un invite la recoit en lecture seule.</summary>
        public void SetDifficultyEditable(bool editable)
        {
            if (difficultyButton != null)
            {
                difficultyButton.interactable = editable;
            }
        }

        /// <summary>
        /// Story 5.16 : affiche les deux reglages de trafic avec leurs bornes EFFECTIVES, et n'active
        /// un bouton de pas que si la valeur courante n'est pas deja a la borne correspondante. Les
        /// bornes sont fournies par l'appelant : l'ecran n'en invente aucune et ne connait ni le
        /// TrafficSettingsDef ni l'invariant "jeteurs &lt;= effectif". La borne haute des jeteurs est
        /// deja minoree par l'appelant, ce qui rend cet invariant visible par la butee.
        /// </summary>
        public void ShowTrafficSettings(
            int aiVehicleTargetCount,
            int minAiVehicleTargetCount,
            int maxAiVehicleTargetCount,
            int litterThrowerCount,
            int minLitterThrowerCount,
            int maxLitterThrowerCount,
            bool editable)
        {
            if (trafficVehiclesLabel != null)
            {
                trafficVehiclesLabel.text = "Vehicules IA : " + aiVehicleTargetCount
                    + "  (" + minAiVehicleTargetCount + "-" + maxAiVehicleTargetCount + ")";
            }

            if (litterThrowersLabel != null)
            {
                litterThrowersLabel.text = "Jeteurs de detritus : " + litterThrowerCount
                    + "  (" + minLitterThrowerCount + "-" + maxLitterThrowerCount + ")";
            }

            SetSteppable(trafficVehiclesDecreaseButton, editable && aiVehicleTargetCount > minAiVehicleTargetCount);
            SetSteppable(trafficVehiclesIncreaseButton, editable && aiVehicleTargetCount < maxAiVehicleTargetCount);
            SetSteppable(litterThrowersDecreaseButton, editable && litterThrowerCount > minLitterThrowerCount);
            SetSteppable(litterThrowersIncreaseButton, editable && litterThrowerCount < maxLitterThrowerCount);
        }

        /// <summary>Desactive les controles de trafic quand aucune borne authorable n'est disponible.</summary>
        public void SetTrafficSettingsEditable(bool editable)
        {
            SetSteppable(trafficVehiclesDecreaseButton, editable);
            SetSteppable(trafficVehiclesIncreaseButton, editable);
            SetSteppable(litterThrowersDecreaseButton, editable);
            SetSteppable(litterThrowersIncreaseButton, editable);
        }

        /// <summary>Un bouton de pas n'est actif que si l'appelant autorise l'edition ET que la valeur n'est pas en butee.</summary>
        private static void SetSteppable(Button button, bool steppable)
        {
            if (button != null)
            {
                button.interactable = steppable;
            }
        }

        /// <summary>
        /// Cette exception locale contourne uniquement le gate "tous prets" de Start Game, jamais les
        /// autres conditions (services en ligne, roster synchronise).
        /// </summary>
        public void ShowSoloTestException(bool enabled)
        {
            if (soloTestExceptionButtonLabel != null)
            {
                soloTestExceptionButtonLabel.text = "Ignorer \"tous prets\" (test solo) : " + (enabled ? "ON" : "OFF");
            }
        }

        /// <summary>
        /// Seul l'hote peut lancer la session reseau (Story 5.3, AD-26) ; un invite le recoit desactive,
        /// meme motif que SetDifficultyEditable -- le clic reste possible cote hote uniquement, un invite
        /// qui cliquerait quand meme sur un bouton actif obtiendrait de toute facon un refus explicite
        /// (StartRefusedWaitingForHostMessage), ce controle en est le reflet visuel.
        /// </summary>
        public void SetStartGameInteractable(bool interactable)
        {
            if (startGameButton != null)
            {
                startGameButton.interactable = interactable;
            }
        }

        /// <summary>Le bouton de fermeture de room n'a de sens que cote hote ; masque cote invite.</summary>
        public void SetCloseRoomVisible(bool visible)
        {
            if (closeRoomButton != null)
            {
                closeRoomButton.gameObject.SetActive(visible);
            }
        }

        /// <summary>Affiche jusqu'a quatre entrees de roster ; les slots restants sont marques vides.</summary>
        public void ShowRoster(LobbyRosterEntry[] entries)
        {
            if (slots == null)
            {
                return;
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    continue;
                }

                if (entries != null && i < entries.Length)
                {
                    slots[i].ShowPlayer(entries[i].DisplayName, entries[i].PortraitTint, entries[i].Ready);
                }
                else
                {
                    slots[i].ShowEmpty();
                }
            }
        }

        private void CycleDifficulty()
        {
            var currentIndex = Array.IndexOf(CycleOrder, displayedDifficulty);
            var nextIndex = (currentIndex + 1) % CycleOrder.Length;
            var next = CycleOrder[nextIndex];

            DifficultyChanged?.Invoke(next);
        }

        private void RaiseReadyToggleRequested()
        {
            ReadyToggleRequested?.Invoke();
        }

        private void RaiseAiVehicleTargetDecreaseRequested()
        {
            AiVehicleTargetStepRequested?.Invoke(-1);
        }

        private void RaiseAiVehicleTargetIncreaseRequested()
        {
            AiVehicleTargetStepRequested?.Invoke(1);
        }

        private void RaiseLitterThrowerDecreaseRequested()
        {
            LitterThrowerStepRequested?.Invoke(-1);
        }

        private void RaiseLitterThrowerIncreaseRequested()
        {
            LitterThrowerStepRequested?.Invoke(1);
        }

        private void RaiseSoloTestExceptionToggleRequested()
        {
            SoloTestExceptionToggleRequested?.Invoke();
        }

        private void RaiseStartGameRequested()
        {
            StartGameRequested?.Invoke();
        }

        private void RaiseCloseRoomRequested()
        {
            CloseRoomRequested?.Invoke();
        }
    }

    /// <summary>Rend le code de salon existant cliquable sans ajouter de surface UI.</summary>
    public sealed class LobbyCodeClipboard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private TMP_Text label;

        private string code;

        private Color normalColor;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            if (label != null)
            {
                normalColor = label.color;
            }
        }

        public void SetCode(string value)
        {
            code = value ?? string.Empty;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(code))
            {
                GUIUtility.systemCopyBuffer = code;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (label != null && !string.IsNullOrEmpty(code))
            {
                label.color = Color.cyan;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (label != null)
            {
                label.color = normalColor;
            }
        }
    }

    /// <summary>Entree de roster deja resolue pour l'affichage (Story 2.4) : nom, teinte de personnage, etat pret.</summary>
    public readonly struct LobbyRosterEntry
    {
        public LobbyRosterEntry(string displayName, Color portraitTint, bool ready)
        {
            DisplayName = displayName;
            PortraitTint = portraitTint;
            Ready = ready;
        }

        public string DisplayName { get; }

        public Color PortraitTint { get; }

        public bool Ready { get; }
    }
}
