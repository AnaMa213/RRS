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
        }

        private void OnDestroy()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                manager.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || isReturningToLobby || !ShouldReturnClientToLobby(wasClientOnlySession, clientId, manager.LocalClientId))
            {
                return;
            }

            StartCoroutine(ReturnClientToLobby(manager));
        }

        public static bool ShouldReturnClientToLobby(bool wasClientOnlySession, ulong disconnectedClientId, ulong localClientId)
        {
            return wasClientOnlySession
                && (disconnectedClientId == NetworkManager.ServerClientId || disconnectedClientId == localClientId);
        }

        private IEnumerator ReturnClientToLobby(NetworkManager manager)
        {
            isReturningToLobby = true;

            Debug.LogWarning("[Run] " + HostDisconnectedMessage);

            var bootstrap = RoadRageBootstrap.EnsureInstance();
            if (bootstrap != null && bootstrap.Notices != null)
            {
                bootstrap.Notices.Publish(new UserNotice(UserNoticeSeverity.Error, HostDisconnectedMessage));
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
