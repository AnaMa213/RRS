using System.Collections;
using RoadRage.App.Services;
using RoadRage.Shared.Presentation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Surveillance minimale Story 2.5 d'une session reseau deja entree dans MVP_Run.
    /// Si un client pur perd l'hote, il quitte Netcode et revient au lobby avec une erreur visible.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedRunSessionMonitor : MonoBehaviour
    {
        public const string HostDisconnectedMessage = "Connexion perdue : l'hote a quitte la session.";

        private bool wasClientOnlySession;
        private bool isReturningToLobby;

        private void Awake()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening)
            {
                return;
            }

            wasClientOnlySession = manager.IsClient && !manager.IsServer;
            manager.OnClientDisconnectCallback += HandleClientDisconnected;
            manager.OnTransportFailure += HandleTransportFailure;
        }

        private void OnDestroy()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                manager.OnClientDisconnectCallback -= HandleClientDisconnected;
                manager.OnTransportFailure -= HandleTransportFailure;
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || isReturningToLobby || !ShouldReturnClientToLobby(wasClientOnlySession, clientId, manager.LocalClientId))
            {
                return;
            }

            StartCoroutine(ReturnClientToLobby(manager, HostDisconnectedNotice));
        }

        private void HandleTransportFailure()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !ShouldReturnClientToLobbyOnTransportFailure(isReturningToLobby))
            {
                return;
            }

            StartCoroutine(ReturnClientToLobby(manager, HostDisconnectedNotice));
        }

        /// <summary>Notice d'erreur publiee uniquement sur les declencheurs subis (perte de session, panne de transport).</summary>
        private static UserNotice HostDisconnectedNotice
        {
            get { return new UserNotice(UserNoticeSeverity.Error, HostDisconnectedMessage); }
        }

        /// <summary>
        /// Sortie volontaire vers le menu principal (Story 5.8), second declencheur du meme teardown :
        /// quitter n'est pas un echec, donc aucune notice n'est publiee. Idempotent -- un second appel
        /// pendant qu'un retour est deja en cours est ignore (garde isReturningToLobby), ce qui garantit
        /// un seul Shutdown() suivi d'un seul LoadMainMenu(). Le drapeau de sortie volontaire du
        /// bootstrap est leve pour que le lobby ne re-embarque pas le client revenu au menu.
        /// </summary>
        public bool RequestVoluntaryExitToMainMenu()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null)
            {
                // Distinct du double quit : ici il n'y a plus de session du tout, donc rien a arreter.
                // Le silence ferait passer une session deja perdue pour un bouton casse.
                Debug.LogWarning("[Run] Sortie volontaire ignoree : aucune session reseau active.");
                return false;
            }

            if (isReturningToLobby)
            {
                // Retour deja en cours : cas nominal du double quit, ignore sans bruit.
                return false;
            }

            var bootstrap = RoadRageBootstrap.EnsureInstance();
            if (bootstrap != null)
            {
                bootstrap.SessionExitRequested = true;
            }

            StartCoroutine(ReturnClientToLobby(manager, null));
            return true;
        }

        public static bool ShouldReturnClientToLobby(bool wasClientOnlySession, ulong disconnectedClientId, ulong localClientId)
        {
            return wasClientOnlySession
                && (disconnectedClientId == NetworkManager.ServerClientId || disconnectedClientId == localClientId);
        }

        /// <summary>
        /// Voie de declenchement transport-failure (Story 2.7) : contrairement a
        /// ShouldReturnClientToLobby, elle ne depend pas de wasClientOnlySession -- une panne de
        /// transport ramene au lobby que la session locale soit hote ou client, seul un retour deja
        /// en cours l'empeche de se redeclencher.
        /// </summary>
        public static bool ShouldReturnClientToLobbyOnTransportFailure(bool isReturningToLobby)
        {
            return !isReturningToLobby;
        }

        /// <summary>
        /// Teardown unique de la session en cours. Story 5.8 : la notice est un parametre -- nulle pour
        /// une sortie volontaire (aucun echec a signaler), l'erreur d'hote perdu pour les declencheurs
        /// subis -- plutot qu'un second chemin parallele de Shutdown() + LoadMainMenu().
        /// </summary>
        private IEnumerator ReturnClientToLobby(NetworkManager manager, UserNotice? notice)
        {
            isReturningToLobby = true;

            var bootstrap = RoadRageBootstrap.EnsureInstance();

            if (notice.HasValue)
            {
                Debug.LogWarning("[Run] " + notice.Value.Message);

                if (bootstrap != null && bootstrap.Notices != null)
                {
                    bootstrap.Notices.Publish(notice.Value);
                }
            }
            else
            {
                Debug.Log("[Run] Sortie volontaire : retour au menu principal sans notice d'erreur.");
            }

            if (manager != null && manager.IsListening)
            {
                manager.Shutdown();
            }

            yield return null;

            if (bootstrap != null && bootstrap.Router != null)
            {
                bootstrap.Router.LoadMainMenu();
            }
            else
            {
                SceneManager.LoadScene(AppSceneRouter.MainMenuLobbySceneName);
            }
        }
    }
}
