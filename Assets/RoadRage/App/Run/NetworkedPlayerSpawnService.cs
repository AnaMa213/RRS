using System.Collections;
using System.Collections.Generic;
using RoadRage.Features.Players;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Shared.Definitions;
using RoadRage.Shared.Domain;
using RoadRage.Shared.Presentation;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Spawn reseau host-only du checkpoint Story 2.5 : chaque client connecte (hote inclus) recoit
    /// un unique NetworkedPlayerRoot host-owned porteur de NetworkedPlayerState. Un balayage initial
    /// couvre l'hote et tout client deja connecte au moment ou MVP_Run charge (chargement synchronise
    /// via NetworkManager.SceneManager), puis OnClientConnectedCallback couvre les arrivees tardives.
    /// Inerte sur un client pur et sur une partie solo locale (NetworkManager non en ecoute) : ces
    /// chemins restent portes par RunFlowController (Story 1.5), inchange.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedPlayerSpawnService : MonoBehaviour
    {
        public const string PlayerRootResourceName = "NetworkedPlayerRoot";

        [SerializeField]
        private RunCompositionRoot compositionRoot;

        [SerializeField]
        private CharacterCatalog characterCatalog;

        [SerializeField]
        private RunCheckpointHudScreen checkpointHud;

        private readonly HashSet<ulong> spawnedClients = new HashSet<ulong>();

        private readonly Dictionary<ulong, NetworkedPlayerState> spawnedStates = new Dictionary<ulong, NetworkedPlayerState>();

        private GameObject playerRootPrefab;

        private bool isActiveHost;

        private NetworkedVehicleSeatService vehicleSeatService;

        private void Awake()
        {
            var manager = NetworkManager.Singleton;
            isActiveHost = manager != null && manager.IsListening && manager.IsServer;

            if (!isActiveHost)
            {
                return;
            }

            playerRootPrefab = Resources.Load<GameObject>(PlayerRootResourceName);
            if (playerRootPrefab == null)
            {
                LogAndShow("Spawn reseau impossible : prefab NetworkedPlayerRoot introuvable.", true);
                isActiveHost = false;
                return;
            }

            manager.OnClientConnectedCallback += HandleClientConnected;
            EnsureVehicleSeatService();
            StartCoroutine(SpawnConnectedClientsAfterSceneProcessing(manager));
        }

        private IEnumerator SpawnConnectedClientsAfterSceneProcessing(NetworkManager manager)
        {
            yield return null;

            if (!isActiveHost || manager == null || !manager.IsListening || !manager.IsServer)
            {
                yield break;
            }

            foreach (var clientId in manager.ConnectedClientsIds)
            {
                SpawnForClient(clientId, false);
            }
        }

        private void OnDestroy()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                manager.OnClientConnectedCallback -= HandleClientConnected;
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (!isActiveHost)
            {
                return;
            }

            SpawnForClient(clientId, true);
        }

        private void SpawnForClient(ulong clientId, bool isLateJoin)
        {
            if (spawnedClients.Contains(clientId))
            {
                LogAndShow("Doublon de spawn reseau ignore pour le client " + clientId + ".", true);
                return;
            }

            var bootstrap = RoadRageBootstrap.EnsureInstance();
            NetworkPlayerProfile profile;
            if (bootstrap == null || bootstrap.NetworkPlayers == null || !bootstrap.NetworkPlayers.TryGet(clientId, out profile))
            {
                profile = new NetworkPlayerProfile(string.Empty, string.Empty);
                LogAndShow("Aucun profil declare pour le client " + clientId + " ; personnage par defaut applique.", true);
            }

            CharacterDef character = null;
            if (characterCatalog == null || !characterCatalog.TryGetById(new DefinitionId(profile.CharacterId), out character) || character == null)
            {
                character = characterCatalog == null ? null : characterCatalog.GetAt(0);
                if (character == null)
                {
                    LogAndShow("Spawn reseau echoue pour le client " + clientId + " : aucun personnage disponible dans le catalogue.", true);
                    return;
                }

                LogAndShow("Personnage '" + profile.CharacterId + "' introuvable pour le client " + clientId + " ; repli sur " + character.RawId + ".", true);
            }

            var instance = Instantiate(playerRootPrefab);
            var networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Destroy(instance);
                LogAndShow("Spawn reseau echoue pour le client " + clientId + " : NetworkedPlayerRoot sans NetworkObject.", true);
                return;
            }

            var state = instance.GetComponent<NetworkedPlayerState>();
            if (state == null)
            {
                Destroy(instance);
                LogAndShow("Spawn reseau echoue pour le client " + clientId + " : NetworkedPlayerRoot sans NetworkedPlayerState.", true);
                return;
            }

            if (instance.GetComponent<NetworkedVehicleSeatIntent>() == null)
            {
                LogAndShow("NetworkedPlayerRoot sans NetworkedVehicleSeatIntent : entree vehicule indisponible.", true);
            }

            if (instance.GetComponent<NetworkedPassengerActionIntent>() == null)
            {
                LogAndShow("NetworkedPlayerRoot sans NetworkedPassengerActionIntent : actions passager indisponibles.", true);
            }

            var spawnPoint = compositionRoot == null ? null : compositionRoot.SpawnRoot;
            var spawnPosition = ResolveSpawnPosition(spawnPoint);
            var spawnRotation = spawnPoint == null ? Quaternion.identity : spawnPoint.rotation;
            instance.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

            instance.name = "NetworkedPlayer_" + clientId;

            // Spawn host-owned (aucun clientId passe) : gameplay-authoritative NetworkObject, jamais
            // client-owned, conformement au contrat d'autorite reseau (NFR4/NFR6).
            networkObject.Spawn();

            state.Mode.Value = PlayerMode.OnFoot;
            state.Lifecycle.Value = PlayerLifecycle.Alive;
            state.ClientId.Value = clientId;
            state.CharacterId.Value = character.RawId;
            state.WorldPosition.Value = spawnPosition;
            state.YawDegrees.Value = spawnRotation.eulerAngles.y;

            spawnedClients.Add(clientId);
            spawnedStates[clientId] = state;

            var label = isLateJoin ? "[Run] Spawn reseau tardif" : "[Run] Spawn reseau";
            Debug.Log(label + " : client " + clientId + " -> " + character.RawId);
        }

        /// <summary>
        /// Resout l'etat reseau spawn pour un client, sans dupliquer le tracking de spawn existant.
        /// Utilise par NetworkedPlayerLifecycleService (Story 2.7) pour appliquer les transitions de
        /// cycle de vie sans que ce service reconstruise sa propre table de resolution clientId -> etat.
        /// </summary>
        public bool TryGetState(ulong clientId, out NetworkedPlayerState state)
        {
            return spawnedStates.TryGetValue(clientId, out state);
        }

        private void EnsureVehicleSeatService()
        {
            if (!isActiveHost)
            {
                return;
            }

            vehicleSeatService = NetworkedVehicleSeatService.Instance;
            if (vehicleSeatService == null)
            {
                vehicleSeatService = FindAnyObjectByType<NetworkedVehicleSeatService>();
            }

            if (vehicleSeatService == null)
            {
                vehicleSeatService = gameObject.AddComponent<NetworkedVehicleSeatService>();
            }

            vehicleSeatService.Configure(this, checkpointHud);
        }

        private Vector3 ResolveSpawnPosition(Transform spawnPoint)
        {
            if (spawnPoint == null)
            {
                return Vector3.zero;
            }

            var index = spawnedClients.Count;
            var lateralSlot = index % 4;
            var row = index / 4;
            var offset = new Vector3((lateralSlot - 1.5f) * 1.2f, 0f, row * 1.2f);
            return spawnPoint.TransformPoint(offset);
        }

        private void LogAndShow(string message, bool isWarning)
        {
            if (isWarning)
            {
                Debug.LogWarning("[Run] " + message);
            }
            else
            {
                Debug.Log("[Run] " + message);
            }

            var bootstrap = RoadRageBootstrap.EnsureInstance();
            if (bootstrap != null && bootstrap.Notices != null)
            {
                bootstrap.Notices.Publish(new UserNotice(UserNoticeSeverity.Warning, message));
            }

            if (checkpointHud != null)
            {
                checkpointHud.ShowSpawnIssue(message);
            }
        }
    }
}
