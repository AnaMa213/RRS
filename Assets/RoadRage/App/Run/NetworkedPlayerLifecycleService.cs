using System.Collections.Generic;
using RoadRage.Features.Players;
using RoadRage.Features.Run;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Service host-only du cycle de vie joueur : applique les transitions validees de
    /// NetworkedPlayerState.Lifecycle (jamais mute ailleurs), marque un client deconnecte sans jamais
    /// detruire son NetworkedPlayerRoot, detecte (et logge une seule fois) la condition "tous les
    /// joueurs connectes sont morts" sans declencher de restart reel, et fournit un declencheur de
    /// mort par chute hors limites. Story 3.5 ajoute le contrat de degats reel : ApplyCollisionDamage
    /// (pont RunFlowController -> Hp joueur) fait passer Alive -> Downed a 0 HP et demarre une
    /// fenetre de resurrection de ReviveWindowSeconds ; TryReviveNearestDowned resout et valide cote
    /// host la resurrection par un coequipier proche (Downed -> Alive, aucune heart depensee) ;
    /// TryRespawn remplace l'ancien remplissage inconditionnel de Hearts ; la depense de heart reste
    /// reservee a l'expiration non-resuscitee de la fenetre Downed, et Hearts n'est plus jamais remis
    /// a 0 sur simple passage a Dead (matrice I/O Story 3.5 : Hearts == 0 => Dead permanent pour le
    /// reste du run, refuse par TryRespawn).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedPlayerLifecycleService : MonoBehaviour
    {
        /// <summary>Degats fixes (Story 3.5) appliques a chaque joueur assis lors d'une collision vehicule.</summary>
        public const int PlayerCollisionDamage = 5;

        /// <summary>HP du joueur resuscite a temps (avant expiration de fenetre) -- aucune heart depensee.</summary>
        public const int ReviveHp = 30;

        /// <summary>Rayon de proximite minimal (greybox) pour qu'un coequipier Alive puisse resusciter un joueur Downed.</summary>
        public const float ReviveProximityRadius = 4f;

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
                    continue;
                }

                if (lifecycle == PlayerLifecycle.Downed)
                {
                    ExpireReviveWindowIfNeeded(clientId, state);
                }
            }
        }

        /// <summary>
        /// Table de transitions valides, pure et testable sans NetworkManager reel (meme pattern que
        /// ShouldReturnClientToLobby, Story 2.5). Dead n'est plus un puits terminal : sa seule sortie
        /// est Alive, exclusivement via TryRespawn (jamais via le chemin de reconnexion). Story 3.5
        /// ajoute Downed -> Alive : resurrection par un coequipier (TryReviveNearestDowned) ou respawn
        /// automatique a l'expiration non-resuscitee de la fenetre (TryRespawn depuis Update()).
        /// </summary>
        public static bool IsValidTransition(PlayerLifecycle from, PlayerLifecycle to)
        {
            switch (from)
            {
                case PlayerLifecycle.Alive:
                    return to == PlayerLifecycle.Downed || to == PlayerLifecycle.Dead || to == PlayerLifecycle.Disconnected;
                case PlayerLifecycle.Downed:
                    return to == PlayerLifecycle.Alive || to == PlayerLifecycle.Dead || to == PlayerLifecycle.Disconnected;
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
        /// spawn tardif, cf. matrice I/O de la Story 2.7. Story 3.5 : en quittant Downed (quelle que
        /// soit la destination), la fenetre de resurrection est toujours purgee ici -- point central
        /// unique, jamais duplique dans TryRespawn/TryReviveNearestDowned/ExpireReviveWindowIfNeeded.
        /// Hearts n'est plus jamais force a 0 sur un simple passage a Dead (remplace par le flux
        /// consommable : depense uniquement dans TryRespawn, a l'expiration non-resuscitee).
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

            if (current == PlayerLifecycle.Downed && next != PlayerLifecycle.Downed)
            {
                state.ReviveDeadlineTime.Value = NetworkedPlayerState.NoActiveReviveWindow;
            }

            CheckAllDeadRestartCondition();
        }

        /// <summary>
        /// Degats de collision vehicule (Story 3.5), host-authoritative uniquement -- applique par le
        /// pont RunFlowController.HandleVehicleCollided, jamais par un client. Ignore un joueur deja
        /// Downed/Dead (pas de double-transition). A 0 HP : passage Downed, ejection de siege
        /// (reutilise ShouldReleaseOccupant/ReleaseInvalidOccupants existants, 3.3/3.4), demarrage de
        /// la fenetre de resurrection.
        /// </summary>
        public void ApplyCollisionDamage(ulong clientId, int amount)
        {
            if (!isActiveHost || spawnService == null || amount <= 0)
            {
                return;
            }

            if (!spawnService.TryGetState(clientId, out var state) || state.Lifecycle.Value != PlayerLifecycle.Alive)
            {
                return;
            }

            var nextHp = Mathf.Max(0, state.Hp.Value - amount);
            state.Hp.Value = nextHp;

            if (nextHp <= 0)
            {
                EnterDowned(clientId, state);
            }
        }

        /// <summary>
        /// Resurrection (Story 3.5), miroir hote de NetworkedVehicleRecoveryIntent : le client ne
        /// choisit jamais lui-meme sa cible (NetworkedPlayerReviveIntent ne transmet que l'identite de
        /// l'emetteur) -- le host resout ici le coequipier Downed le plus proche dans
        /// ReviveProximityRadius et valide son etat avant d'appliquer quoi que ce soit. Aucune heart
        /// depensee (matrice I/O : resurrection a temps == gratuite).
        /// </summary>
        public void TryReviveNearestDowned(ulong reviverClientId)
        {
            if (!isActiveHost || spawnService == null)
            {
                return;
            }

            if (!spawnService.TryGetState(reviverClientId, out var reviverState) || reviverState.Lifecycle.Value != PlayerLifecycle.Alive)
            {
                return;
            }

            var manager = NetworkManager.Singleton;
            if (manager == null)
            {
                return;
            }

            NetworkedPlayerState bestState = null;
            var bestClientId = 0UL;
            var bestDistance = float.PositiveInfinity;

            foreach (var clientId in manager.ConnectedClientsIds)
            {
                if (clientId == reviverClientId || !spawnService.TryGetState(clientId, out var candidate) || candidate.Lifecycle.Value != PlayerLifecycle.Downed)
                {
                    continue;
                }

                var distance = Vector3.Distance(reviverState.WorldPosition.Value, candidate.WorldPosition.Value);
                if (distance > ReviveProximityRadius || distance >= bestDistance)
                {
                    continue;
                }

                bestState = candidate;
                bestClientId = clientId;
                bestDistance = distance;
            }

            if (bestState == null)
            {
                return;
            }

            TrySetLifecycle(bestClientId, PlayerLifecycle.Alive);
            bestState.Hp.Value = ReviveHp;

            Debug.Log("[Run] Resurrection : client " + bestClientId + " par " + reviverClientId + ".");
        }

        /// <summary>
        /// Point d'entree du respawn manuel (Story 2.7) depuis Dead : il ne remplit plus Hearts
        /// inconditionnellement et refuse silencieusement si Hearts == 0. Story 3.5 reserve la
        /// depense de heart au chemin prive TryRespawn(..., consumeHeart: true), appele uniquement par
        /// ExpireReviveWindowIfNeeded quand une fenetre Downed expire sans resurrection.
        /// </summary>
        public void TryRespawn(ulong clientId)
        {
            TryRespawn(clientId, false);
        }

        private void TryRespawn(ulong clientId, bool consumeHeart)
        {
            if (!isActiveHost || spawnService == null || !spawnService.TryGetState(clientId, out var state))
            {
                return;
            }

            var current = state.Lifecycle.Value;
            if (current != PlayerLifecycle.Dead && (current != PlayerLifecycle.Downed || !consumeHeart))
            {
                return;
            }

            if (state.Hearts.Value <= 0)
            {
                return;
            }

            if (consumeHeart)
            {
                state.Hearts.Value -= 1;
            }

            TrySetLifecycle(clientId, PlayerLifecycle.Alive);
            state.Hp.Value = NetworkedPlayerState.DefaultMaxHp;

            var spawnPoint = compositionRoot == null ? null : compositionRoot.SpawnRoot;
            var spawnPosition = spawnPoint == null ? Vector3.zero : spawnPoint.position;
            var spawnYaw = spawnPoint == null ? 0f : spawnPoint.eulerAngles.y;

            state.WorldPosition.Value = spawnPosition;
            state.YawDegrees.Value = spawnYaw;

            Debug.Log("[Run] Respawn : client " + clientId + " -> " + spawnPosition + ".");
        }

        /// <summary>Passage Alive -> Downed a 0 HP : demarre la fenetre de resurrection si la transition aboutit.</summary>
        private void EnterDowned(ulong clientId, NetworkedPlayerState state)
        {
            TrySetLifecycle(clientId, PlayerLifecycle.Downed);
            if (state.Lifecycle.Value != PlayerLifecycle.Downed)
            {
                return;
            }

            state.ReviveDeadlineTime.Value = ResolveNetworkTime() + NetworkedPlayerState.ReviveWindowSeconds;
        }

        /// <summary>
        /// Verifie l'expiration de la fenetre de resurrection d'un joueur Downed (appele depuis
        /// Update() pour chaque joueur Downed). Hearts restantes : respawn automatique (via
        /// TryRespawn, qui depense la heart). Plus de heart : Dead permanent pour le reste du run.
        /// </summary>
        private void ExpireReviveWindowIfNeeded(ulong clientId, NetworkedPlayerState state)
        {
            if (state.ReviveDeadlineTime.Value < 0d || ResolveNetworkTime() < state.ReviveDeadlineTime.Value)
            {
                return;
            }

            if (state.Hearts.Value > 0)
            {
                TryRespawn(clientId, true);
            }
            else
            {
                TrySetLifecycle(clientId, PlayerLifecycle.Dead);
            }
        }

        private static double ResolveNetworkTime()
        {
            var manager = NetworkManager.Singleton;
            return manager != null ? manager.ServerTime.Time : Time.timeAsDouble;
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
