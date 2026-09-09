using System.Collections.Generic;
using RoadRage.Features.Players;
using RoadRage.Features.Run;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Service host-only Story 2.7 du cycle de vie joueur : applique les transitions validees de
    /// NetworkedPlayerState.Lifecycle (jamais mute ailleurs), marque un client deconnecte sans jamais
    /// detruire son NetworkedPlayerRoot, detecte (et logge une seule fois) la condition "tous les
    /// joueurs connectes sont morts" sans declencher de restart reel, et fournit un declencheur de
    /// mort par chute hors limites ainsi qu'un respawn individuel appele par la Rpc de
    /// NetworkedPlayerLifecycleIntent via le singleton statique Instance.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedPlayerLifecycleService : MonoBehaviour
    {
        public static NetworkedPlayerLifecycleService Instance { get; private set; }

        [SerializeField]
        private RunCompositionRoot compositionRoot;

        [SerializeField]
        private NetworkedPlayerSpawnService spawnService;

        [SerializeField]
        private float voidHeightThreshold = -10f;

        private bool isActiveHost;

        private bool allDeadRestartLogged;

        private void Awake()
        {
            Instance = this;

            var manager = NetworkManager.Singleton;
            isActiveHost = manager != null && manager.IsListening && manager.IsServer;

            if (!isActiveHost)
            {
                return;
            }

            manager.OnClientDisconnectCallback += HandleClientDisconnected;
            manager.OnClientConnectedCallback += HandleClientConnected;
        }

        private void OnDestroy()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                manager.OnClientDisconnectCallback -= HandleClientDisconnected;
                manager.OnClientConnectedCallback -= HandleClientConnected;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!isActiveHost)
            {
                return;
            }

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || !manager.IsServer || spawnService == null)
            {
                return;
            }

            foreach (var clientId in manager.ConnectedClientsIds)
            {
                if (!spawnService.TryGetState(clientId, out var state))
                {
                    continue;
                }

                var lifecycle = state.Lifecycle.Value;
                if (lifecycle != PlayerLifecycle.Alive && lifecycle != PlayerLifecycle.Downed)
                {
                    continue;
                }

                if (IsBelowVoidHeightThreshold(state.WorldPosition.Value.y, voidHeightThreshold))
                {
                    TrySetLifecycle(clientId, PlayerLifecycle.Dead);
                }
            }
        }

        /// <summary>
        /// Table de transitions valides, pure et testable sans NetworkManager reel (meme pattern que
        /// ShouldReturnClientToLobby, Story 2.5). Dead n'est plus un puits terminal : sa seule sortie
        /// est Alive, exclusivement via TryRespawn (jamais via le chemin de reconnexion).
        /// </summary>
        public static bool IsValidTransition(PlayerLifecycle from, PlayerLifecycle to)
        {
            switch (from)
            {
                case PlayerLifecycle.Alive:
                    return to == PlayerLifecycle.Downed || to == PlayerLifecycle.Dead || to == PlayerLifecycle.Disconnected;
                case PlayerLifecycle.Downed:
                    return to == PlayerLifecycle.Dead || to == PlayerLifecycle.Disconnected;
                case PlayerLifecycle.Dead:
                    return to == PlayerLifecycle.Alive;
                case PlayerLifecycle.Disconnected:
                    return to == PlayerLifecycle.Alive;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Seuil de chute hors limites, pur et testable : le declencheur de mort manuel de la Story
        /// 2.7 (aucun systeme de degats reel derriere).
        /// </summary>
        public static bool IsBelowVoidHeightThreshold(float worldPositionY, float voidHeightThreshold)
        {
            return worldPositionY < voidHeightThreshold;
        }

        /// <summary>
        /// Predicat pur "condition de restart detectee" : vrai seulement si au moins un joueur spawn
        /// est connecte et que tous ses etats resolus sont Dead. Meme pattern static-pur que
        /// IsValidTransition, teste sans NetworkManager reel.
        /// </summary>
        public static bool ShouldLogAllDeadRestartCondition(IEnumerable<PlayerLifecycle> connectedSpawnedLifecycles)
        {
            var any = false;
            foreach (var lifecycle in connectedSpawnedLifecycles)
            {
                any = true;
                if (lifecycle != PlayerLifecycle.Dead)
                {
                    return false;
                }
            }

            return any;
        }

        /// <summary>
        /// Applique une transition si valide, sinon logge un avertissement et laisse la valeur
        /// inchangee. Ignore silencieusement (pas de warning) un clientId jamais spawn -- echec de
        /// spawn tardif, cf. matrice I/O de la Story 2.7.
        /// </summary>
        public void TrySetLifecycle(ulong clientId, PlayerLifecycle next)
        {
            if (!isActiveHost || spawnService == null || !spawnService.TryGetState(clientId, out var state))
            {
                return;
            }

            var current = state.Lifecycle.Value;
            if (!IsValidTransition(current, next))
            {
                Debug.LogWarning("[Run] Transition de cycle de vie refusee pour le client " + clientId + " : " + current + " -> " + next + ".");
                return;
            }

            state.Lifecycle.Value = next;
            Debug.Log("[Run] Cycle de vie : client " + clientId + " " + current + " -> " + next + ".");

            if (next == PlayerLifecycle.Dead)
            {
                state.Hearts.Value = 0;
            }

            CheckAllDeadRestartCondition();
        }

        /// <summary>
        /// Point d'entree du respawn individuel (Story 2.7), appele par la Rpc de
        /// NetworkedPlayerLifecycleIntent une fois l'emetteur valide. Ignore si l'etat courant n'est
        /// pas Dead. Remet le joueur Alive et le teleporte sur compositionRoot.SpawnRoot sans
        /// redemarrer le run.
        /// </summary>
        public void TryRespawn(ulong clientId)
        {
            if (!isActiveHost || spawnService == null || !spawnService.TryGetState(clientId, out var state))
            {
                return;
            }

            if (state.Lifecycle.Value != PlayerLifecycle.Dead)
            {
                return;
            }

            TrySetLifecycle(clientId, PlayerLifecycle.Alive);
            state.Hearts.Value = state.MaxHearts.Value;

            var spawnPoint = compositionRoot == null ? null : compositionRoot.SpawnRoot;
            var spawnPosition = spawnPoint == null ? Vector3.zero : spawnPoint.position;
            var spawnYaw = spawnPoint == null ? 0f : spawnPoint.eulerAngles.y;

            state.WorldPosition.Value = spawnPosition;
            state.YawDegrees.Value = spawnYaw;

            Debug.Log("[Run] Respawn : client " + clientId + " -> " + spawnPosition + ".");
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            TrySetLifecycle(clientId, PlayerLifecycle.Disconnected);
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (spawnService == null || !spawnService.TryGetState(clientId, out var state) || state.Lifecycle.Value != PlayerLifecycle.Disconnected)
            {
                return;
            }

            TrySetLifecycle(clientId, PlayerLifecycle.Alive);
        }

        private void CheckAllDeadRestartCondition()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || spawnService == null)
            {
                return;
            }

            var lifecycles = new List<PlayerLifecycle>();
            foreach (var clientId in manager.ConnectedClientsIds)
            {
                if (spawnService.TryGetState(clientId, out var state))
                {
                    lifecycles.Add(state.Lifecycle.Value);
                }
            }

            if (ShouldLogAllDeadRestartCondition(lifecycles))
            {
                if (!allDeadRestartLogged)
                {
                    Debug.LogWarning("[Run] Condition de restart detectee : tous les joueurs connectes sont morts.");
                    allDeadRestartLogged = true;
                }
            }
            else
            {
                allDeadRestartLogged = false;
            }
        }
    }
}
