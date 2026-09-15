using RoadRage.Features.UI;
using RoadRage.Shared.Input;
using RoadRage.Shared.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Proprietaire App du menu d'echappement de MVP_Run (Story 5.8) : lit Echap (Features.UI ne
    /// reference pas Unity.InputSystem), libere puis restaure le curseur, bloque les entrees locales
    /// par le portail partage LocalInputGate, et achemine le quit vers le teardown de session
    /// existant (NetworkedRunSessionMonitor), qui reste le seul proprietaire de Shutdown() +
    /// LoadMainMenu().
    ///
    /// Il ne touche jamais Time.timeScale et ne mute aucune donnee partagee : une session hebergee
    /// continue de simuler pendant qu'un joueur a son menu ouvert.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunEscapeMenuFlowController : MonoBehaviour
    {
        public const string NoSessionToQuitMessage = "Retour au menu principal ignore : aucune session reseau a quitter depuis cette scene.";

        [SerializeField]
        private RunEscapeMenuScreen screen;

        private bool isOpen;
        private CursorLockMode capturedCursorLockMode;
        private bool capturedCursorVisible;

        /// <summary>Vrai quand le menu est ouvert : source de verite du blocage des entrees locales.</summary>
        public bool IsOpen
        {
            get { return isOpen; }
        }

        private void Awake()
        {
            // Une scene precedente dechargee menu ouvert ne doit pas laisser le portail bloque : la
            // remise a zero est faite ici et a la destruction, jamais en dependant d'un appel externe.
            LocalInputGate.Reset();

            if (screen == null)
            {
                Debug.LogWarning("[App] RunEscapeMenuFlowController sans reference vers RunEscapeMenuScreen : le menu d'echappement reste indisponible.");
                return;
            }

            screen.ResumeRequested += Close;
            screen.QuitToMainMenuRequested += RequestQuitToMainMenu;
        }

        private void Update()
        {
            if (screen == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        private void OnDestroy()
        {
            if (screen != null)
            {
                screen.ResumeRequested -= Close;
                screen.QuitToMainMenuRequested -= RequestQuitToMainMenu;
            }

            if (isOpen)
            {
                // Etat de curseur capture a l'ouverture : une scene dechargee menu ouvert doit rendre
                // les entrees et restaurer le curseur, pas laisser la scene suivante dans l'etat libere.
                RestoreCursorState();
            }

            isOpen = false;
            LocalInputGate.Reset();
        }

        /// <summary>Ouvre le menu : panneau visible, entrees locales bloquees, curseur libere et visible.</summary>
        public void Open()
        {
            if (isOpen || screen == null)
            {
                return;
            }

            isOpen = true;
            capturedCursorLockMode = Cursor.lockState;
            capturedCursorVisible = Cursor.visible;

            screen.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            LocalInputGate.Block();
        }

        /// <summary>Ferme le menu : panneau masque, entrees rendues, curseur restaure a l'etat capture.</summary>
        public void Close()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;
            RestoreCursorState();
            LocalInputGate.Release();

            if (screen != null)
            {
                screen.Hide();
            }
        }

        /// <summary>Echap : ouvre le menu s'il est ferme, le ferme s'il est ouvert.</summary>
        public void Toggle()
        {
            if (isOpen)
            {
                Close();
                return;
            }

            Open();
        }

        /// <summary>
        /// Quit to Main Menu : emprunte le teardown de session existant, en sortie volontaire -- donc
        /// sans notice d'erreur. Un second appel pendant qu'un retour est deja en cours est ignore par
        /// le moniteur lui-meme ; ce composant n'ajoute aucun second chemin de sortie.
        ///
        /// Sans moniteur, il n'y a aucune session a quitter : c'est le cas d'une `MVP_Run` chargee
        /// directement (scene jouee seule, fixture de test), ou Netcode n'a jamais demarre. Le refus est
        /// alors rendu VISIBLE plutot que silencieux -- un bouton qui ne fait rien se lit comme casse --
        /// et il ne doit surtout pas ouvrir un repli de routage, ce que la contrainte "Toujours" de la
        /// story interdit (le moniteur reste l'unique proprietaire de Shutdown() + LoadMainMenu()).
        /// </summary>
        public void RequestQuitToMainMenu()
        {
            var monitor = ResolveSessionMonitor();
            if (monitor == null)
            {
                Debug.LogWarning("[App] " + NoSessionToQuitMessage);
                PublishNotice(new UserNotice(UserNoticeSeverity.Warning, NoSessionToQuitMessage));
                return;
            }

            monitor.RequestVoluntaryExitToMainMenu();
        }

        /// <summary>Publie une notice sur le canal persistant du bootstrap, s'il est disponible.</summary>
        private static void PublishNotice(UserNotice notice)
        {
            var bootstrap = RoadRageBootstrap.EnsureInstance();
            if (bootstrap != null && bootstrap.Notices != null)
            {
                bootstrap.Notices.Publish(notice);
            }
        }

        /// <summary>
        /// Resolution paresseuse du moniteur : NetworkedRunSessionMonitor est ajoute par
        /// RunFlowController.Start(), jamais a l'Awake de ce composant, pour ne dependre d'aucun ordre
        /// de composants sur RunRoot.
        /// </summary>
        private NetworkedRunSessionMonitor ResolveSessionMonitor()
        {
            var monitor = GetComponent<NetworkedRunSessionMonitor>();
            return monitor != null ? monitor : FindAnyObjectByType<NetworkedRunSessionMonitor>();
        }

        private void RestoreCursorState()
        {
            Cursor.lockState = capturedCursorLockMode;
            Cursor.visible = capturedCursorVisible;
        }
    }
}
