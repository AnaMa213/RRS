using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Shared.Presentation;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    [DisallowMultipleComponent]
    public sealed class RunFlowController : MonoBehaviour
    {
        public const string MissingProfileMessage = "Entree monde refusee : aucun profil joueur confirme.";

        public const string MissingCharacterMessage = "Entree monde refusee : le personnage selectionne est absent du catalogue.";

        public const string MissingCharacterPreviewMessage = "Entree monde refusee : le personnage selectionne n'a pas de prefab d'apercu assigne.";

        [SerializeField]
        private RunCompositionRoot compositionRoot;

        [SerializeField]
        private CharacterCatalog characterCatalog;

        [SerializeField]
        private Camera playerCamera;

        [SerializeField]
        private Transform playerSpawnPoint;

        [SerializeField]
        private RunCheckpointHudScreen checkpointHud;

        private GameObject activeLocalPlayer;

        private NetworkedPlayerState localNetworkedPlayerState;

        private bool hudRuntimeBound;

        private bool networkHudBridgeActive;

        public GameObject ActiveLocalPlayer
        {
            get { return activeLocalPlayer; }
        }

        private void Start()
        {
            EnsureNetworkSessionMonitor();
            EnsureNetworkHudBridge();

            if (checkpointHud != null)
            {
                checkpointHud.ShowAwaitingProfile();
            }

            string error;
            if (!TrySpawnSelectedProfile(out error))
            {
                Debug.LogWarning("[Run] " + error);
                if (checkpointHud != null)
                {
                    checkpointHud.ShowBlockedState(error);
                }

                PublishWarning(error);
            }
        }

        private void Update()
        {
            if (!hudRuntimeBound || checkpointHud == null || localNetworkedPlayerState != null)
            {
                return;
            }

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening)
            {
                return;
            }

            var candidate = ResolveLocalNetworkedPlayerState(manager.LocalClientId);
            if (candidate == null)
            {
                return;
            }

            localNetworkedPlayerState = candidate;
            SubscribeToLocalNetworkedPlayerState(candidate);
            RefreshHudFromLocalNetworkedPlayerState();
        }

        private void OnDestroy()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null && networkHudBridgeActive)
            {
                manager.OnClientConnectedCallback -= HandleNetworkPlayerCountChanged;
                manager.OnClientDisconnectCallback -= HandleNetworkPlayerCountChanged;
            }

            UnsubscribeFromLocalNetworkedPlayerState();
        }

        public bool TrySpawnSelectedProfile(out string error)
        {
            if (activeLocalPlayer != null)
            {
                error = string.Empty;
                return true;
            }

            var bootstrap = RoadRageBootstrap.EnsureInstance();
            if (bootstrap == null || bootstrap.Profiles == null || !bootstrap.Profiles.HasProfile)
            {
                error = MissingProfileMessage;
                return false;
            }

            if (characterCatalog == null || !characterCatalog.TryGetById(bootstrap.Profiles.Current.CharacterId, out var character) || character == null)
            {
                error = MissingCharacterMessage;
                return false;
            }

            if (character.PreviewPrefab == null)
            {
                error = MissingCharacterPreviewMessage;
                return false;
            }

            var parent = compositionRoot == null || compositionRoot.RuntimeRoot == null
                ? transform
                : compositionRoot.RuntimeRoot;
            var spawn = playerSpawnPoint == null
                ? (compositionRoot == null ? null : compositionRoot.SpawnRoot)
                : playerSpawnPoint;

            activeLocalPlayer = new GameObject("LocalPlayer_" + character.RawId);
            activeLocalPlayer.transform.SetParent(parent, false);
            activeLocalPlayer.transform.position = spawn == null ? Vector3.zero : spawn.position;
            activeLocalPlayer.transform.rotation = spawn == null ? Quaternion.identity : spawn.rotation;

            var controller = activeLocalPlayer.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 50f;

            var visual = Instantiate(character.PreviewPrefab, activeLocalPlayer.transform);
            visual.name = character.PreviewPrefab.name + "_Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            DisableVisualColliders(visual);

            var onFootController = activeLocalPlayer.AddComponent<LocalOnFootController>();
            onFootController.AttachCamera(playerCamera);
            AttachNetworkPoseReporter(activeLocalPlayer);

            if (checkpointHud != null)
            {
                checkpointHud.ShowRunState(ResolveLobbyState(), bootstrap.Profiles.Current.DisplayName, character.DisplayName);
            }

            BindHudRuntimeState();

            Debug.Log("[Run] Joueur local spawn : " + bootstrap.Profiles.Current.DisplayName + " / " + character.Id);
            error = string.Empty;
            return true;
        }

        private void BindHudRuntimeState()
        {
            hudRuntimeBound = true;

            if (checkpointHud == null)
            {
                return;
            }

            checkpointHud.ShowHudState(
                NetworkedPlayerState.DefaultMaxHearts,
                NetworkedPlayerState.DefaultMaxHearts,
                NetworkedPlayerState.DefaultStaminaNormalized,
                ResolveConnectedPlayerCount(),
                NetworkedPlayerState.DefaultMoney);
        }

        private void EnsureNetworkHudBridge()
        {
            if (networkHudBridgeActive)
            {
                return;
            }

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening)
            {
                return;
            }

            manager.OnClientConnectedCallback += HandleNetworkPlayerCountChanged;
            manager.OnClientDisconnectCallback += HandleNetworkPlayerCountChanged;
            networkHudBridgeActive = true;
        }

        private void HandleNetworkPlayerCountChanged(ulong clientId)
        {
            if (!hudRuntimeBound || checkpointHud == null)
            {
                return;
            }

            checkpointHud.SetPlayerCount(ResolveConnectedPlayerCount());
        }

        private static int ResolveConnectedPlayerCount()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening)
            {
                return 1;
            }

            return manager.ConnectedClientsIds.Count;
        }

        private static NetworkedPlayerState ResolveLocalNetworkedPlayerState(ulong localClientId)
        {
            var candidates = FindObjectsByType<NetworkedPlayerState>(FindObjectsInactive.Exclude);
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (candidate != null && candidate.IsSpawned && candidate.ClientId.Value == localClientId)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void SubscribeToLocalNetworkedPlayerState(NetworkedPlayerState state)
        {
            state.Hearts.OnValueChanged += HandleHeartsChanged;
            state.MaxHearts.OnValueChanged += HandleHeartsChanged;
            state.StaminaNormalized.OnValueChanged += HandleStaminaChanged;
            state.Money.OnValueChanged += HandleMoneyChanged;
        }

        private void UnsubscribeFromLocalNetworkedPlayerState()
        {
            if (localNetworkedPlayerState == null)
            {
                return;
            }

            localNetworkedPlayerState.Hearts.OnValueChanged -= HandleHeartsChanged;
            localNetworkedPlayerState.MaxHearts.OnValueChanged -= HandleHeartsChanged;
            localNetworkedPlayerState.StaminaNormalized.OnValueChanged -= HandleStaminaChanged;
            localNetworkedPlayerState.Money.OnValueChanged -= HandleMoneyChanged;
            localNetworkedPlayerState = null;
        }

        private void RefreshHudFromLocalNetworkedPlayerState()
        {
            if (checkpointHud == null || localNetworkedPlayerState == null)
            {
                return;
            }

            checkpointHud.SetHearts(localNetworkedPlayerState.Hearts.Value, localNetworkedPlayerState.MaxHearts.Value);
            checkpointHud.SetStamina(localNetworkedPlayerState.StaminaNormalized.Value);
            checkpointHud.SetMoney(localNetworkedPlayerState.Money.Value);
        }

        private void HandleHeartsChanged(int previousValue, int newValue)
        {
            if (checkpointHud == null || localNetworkedPlayerState == null)
            {
                return;
            }

            checkpointHud.SetHearts(localNetworkedPlayerState.Hearts.Value, localNetworkedPlayerState.MaxHearts.Value);
        }

        private void HandleStaminaChanged(float previousValue, float newValue)
        {
            if (checkpointHud != null)
            {
                checkpointHud.SetStamina(newValue);
            }
        }

        private void HandleMoneyChanged(int previousValue, int newValue)
        {
            if (checkpointHud != null)
            {
                checkpointHud.SetMoney(newValue);
            }
        }

        private void EnsureNetworkSessionMonitor()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening)
            {
                return;
            }

            if (GetComponent<NetworkedRunSessionMonitor>() == null)
            {
                gameObject.AddComponent<NetworkedRunSessionMonitor>();
            }
        }

        private static void AttachNetworkPoseReporter(GameObject localPlayer)
        {
            var manager = NetworkManager.Singleton;
            if (localPlayer == null || manager == null || !manager.IsListening || !manager.IsClient)
            {
                return;
            }

            if (localPlayer.GetComponent<NetworkedLocalPlayerPoseReporter>() == null)
            {
                localPlayer.AddComponent<NetworkedLocalPlayerPoseReporter>();
            }
        }

        private static void DisableVisualColliders(GameObject visual)
        {
            var colliders = visual.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private void PublishWarning(string message)
        {
            var bootstrap = RoadRageBootstrap.EnsureInstance();
            if (bootstrap.Notices != null)
            {
                bootstrap.Notices.Publish(new UserNotice(UserNoticeSeverity.Warning, message));
            }
        }

        private static string ResolveLobbyState()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening)
            {
                return RunCheckpointHudScreen.LocalLobbyState;
            }

            if (manager.IsHost || manager.IsServer)
            {
                return RunCheckpointHudScreen.NetworkHostLobbyState;
            }

            return RunCheckpointHudScreen.NetworkClientLobbyState;
        }
    }
}
