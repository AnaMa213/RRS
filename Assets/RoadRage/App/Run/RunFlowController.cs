using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using RoadRage.Shared.Presentation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private GameObject activeLocalPlayerVisual;

        private NetworkedPlayerState localNetworkedPlayerState;

        private NetworkedVehicleState localSoloVehicleState;

        private NetworkedVehicleDriverController localSoloVehicleDriver;

        private LocalVehicleCameraRig localSoloVehicleCameraRig;

        private int localSoloSeatIndex = NetworkedVehicleState.NoSeatIndex;

        private bool localSoloVehicleSeated;

        private bool localSoloPlayerWasDead;

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
            HandleLocalSoloVehicleInteraction();
            RefreshLocalSoloDeathRecovery();
            ResolveLocalNetworkedPlayerStateIfNeeded();
            SynchronizeLocalSeatedPose();
        }

        private void ResolveLocalNetworkedPlayerStateIfNeeded()
        {
            if (!hudRuntimeBound || localNetworkedPlayerState != null)
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
            RefreshLocalSeatMode(candidate.Mode.Value, candidate.Mode.Value);
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
            var spawn = ResolveSpawnPoint();

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
            activeLocalPlayerVisual = visual;

            var onFootController = activeLocalPlayer.AddComponent<LocalOnFootController>();
            onFootController.AttachCamera(playerCamera);
            AttachNetworkPoseReporter(activeLocalPlayer);
            AttachLocalVoidRespawnController(activeLocalPlayer, checkpointHud);

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
            state.Lifecycle.OnValueChanged += HandleLifecycleChanged;
            state.Mode.OnValueChanged += HandleModeChanged;
            state.SeatIndex.OnValueChanged += HandleSeatIndexChanged;
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
            localNetworkedPlayerState.Lifecycle.OnValueChanged -= HandleLifecycleChanged;
            localNetworkedPlayerState.Mode.OnValueChanged -= HandleModeChanged;
            localNetworkedPlayerState.SeatIndex.OnValueChanged -= HandleSeatIndexChanged;
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

        private void HandleModeChanged(PlayerMode previousValue, PlayerMode newValue)
        {
            RefreshLocalSeatMode(previousValue, newValue);
        }

        private void HandleSeatIndexChanged(int previousValue, int newValue)
        {
            SynchronizeLocalSeatedPose();
        }

        private void RefreshLocalSeatMode(PlayerMode previousMode, PlayerMode currentMode)
        {
            var localOnFootController = activeLocalPlayer == null ? null : activeLocalPlayer.GetComponent<LocalOnFootController>();
            if (IsVehicleSeatMode(currentMode))
            {
                if (localOnFootController != null)
                {
                    localOnFootController.MovementEnabled = false;
                }

                SetLocalPlayerBodyActive(false);
                SynchronizeLocalSeatedPose();
                return;
            }

            if (!IsVehicleSeatMode(previousMode))
            {
                return;
            }

            var position = localNetworkedPlayerState == null ? Vector3.zero : localNetworkedPlayerState.WorldPosition.Value;
            var yaw = localNetworkedPlayerState == null ? 0f : localNetworkedPlayerState.YawDegrees.Value;
            var rotation = Quaternion.Euler(0f, yaw, 0f);
            var shouldReactivateBody = localNetworkedPlayerState == null || localNetworkedPlayerState.Lifecycle.Value != PlayerLifecycle.Dead;
            SuppressVehicleCamerasForLocalSeatExit();
            SetLocalPlayerBodyActive(shouldReactivateBody);

            if (localOnFootController != null)
            {
                localOnFootController.Teleport(position, rotation);
                RestoreOnFootCamera(localOnFootController);
                localOnFootController.MovementEnabled = shouldReactivateBody;
            }
            else if (activeLocalPlayer != null)
            {
                activeLocalPlayer.transform.SetPositionAndRotation(position, rotation);
            }
        }

        // Le ressenti gameplay doit rester le meme en solo et en reseau ; seule l'autorite change.
        private void HandleLocalSoloVehicleInteraction()
        {
            if (IsNetworkSessionActive() || activeLocalPlayer == null)
            {
                return;
            }

            if (localSoloVehicleSeated)
            {
                SynchronizeLocalSoloSeatedPose();
            }

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.eKey.wasPressedThisFrame)
            {
                return;
            }

            if (localSoloVehicleSeated)
            {
                ExitLocalSoloVehicle();
                return;
            }

            var preferPassenger = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            TryEnterLocalSoloVehicle(preferPassenger);
        }

        private void TryEnterLocalSoloVehicle(bool preferPassenger)
        {
            if (!TryResolveNearestLocalSoloVehicle(out var vehicleState, out var driverController, out var cameraRig, out var distance))
            {
                ShowVehicleSeatMessage(NetworkedVehicleSeatService.VehicleUnavailableMessage);
                return;
            }

            if (distance > NetworkedVehicleSeatService.DefaultEntryRadius)
            {
                ShowVehicleSeatMessage(NetworkedVehicleSeatService.PlayerTooFarMessage);
                return;
            }

            localSoloVehicleState = vehicleState;
            localSoloVehicleDriver = driverController;
            localSoloVehicleCameraRig = cameraRig;
            localSoloSeatIndex = ResolveLocalSoloSeatIndex(preferPassenger);
            localSoloVehicleSeated = true;

            var localOnFootController = activeLocalPlayer.GetComponent<LocalOnFootController>();
            if (localOnFootController != null)
            {
                localOnFootController.MovementEnabled = false;
            }

            SetLocalPlayerBodyActive(false);
            localSoloVehicleDriver.SetLocalSoloDriverActive(IsDriverSeat(localSoloSeatIndex));
            if (localSoloVehicleCameraRig != null)
            {
                localSoloVehicleCameraRig.SetLocalSoloCameraActive(IsDriverSeat(localSoloSeatIndex));
            }

            SynchronizeLocalSoloSeatedPose();
            ShowVehicleSeatMessage(ResolveSeatOccupiedMessage(localSoloSeatIndex));
        }

        private void ExitLocalSoloVehicle()
        {
            if (!TryResolveCurrentLocalSoloVehicle(out var vehicleState, out var driverController, out var cameraRig))
            {
                RestoreLocalSoloOnFootControl();
                return;
            }

            driverController.SetLocalSoloDriverActive(false);
            if (cameraRig != null)
            {
                cameraRig.SetLocalSoloCameraActive(false);
            }

            var exitSeatIndex = NetworkedVehicleState.IsValidSeatIndex(localSoloSeatIndex)
                ? localSoloSeatIndex
                : NetworkedVehicleState.DriverSeatIndex;
            var exitPosition = vehicleState.transform.TransformPoint(NetworkedVehicleState.ResolveExitLocalOffset(exitSeatIndex));
            var exitRotation = Quaternion.Euler(0f, vehicleState.transform.eulerAngles.y, 0f);
            var localOnFootController = activeLocalPlayer.GetComponent<LocalOnFootController>();

            localSoloVehicleSeated = false;
            localSoloVehicleState = null;
            localSoloVehicleDriver = null;
            localSoloVehicleCameraRig = null;
            localSoloSeatIndex = NetworkedVehicleState.NoSeatIndex;

            SetLocalPlayerBodyActive(true);
            if (localOnFootController != null)
            {
                localOnFootController.Teleport(exitPosition, exitRotation);
                RestoreOnFootCamera(localOnFootController);
                localOnFootController.MovementEnabled = true;
            }
            else
            {
                activeLocalPlayer.transform.SetPositionAndRotation(exitPosition, exitRotation);
            }

            ShowVehicleSeatMessage("Sortie vehicule.");
        }

        private void SynchronizeLocalSoloSeatedPose()
        {
            if (!localSoloVehicleSeated || activeLocalPlayer == null)
            {
                return;
            }

            if (!TryResolveCurrentLocalSoloVehicle(out var vehicleState, out _, out _))
            {
                RestoreLocalSoloOnFootControl();
                return;
            }

            var localOnFootController = activeLocalPlayer.GetComponent<LocalOnFootController>();
            if (localOnFootController != null)
            {
                localOnFootController.MovementEnabled = false;
            }

            var seatIndex = NetworkedVehicleState.IsValidSeatIndex(localSoloSeatIndex)
                ? localSoloSeatIndex
                : NetworkedVehicleState.DriverSeatIndex;

            SetLocalPlayerBodyActive(false);
            activeLocalPlayer.transform.SetPositionAndRotation(
                vehicleState.transform.TransformPoint(NetworkedVehicleState.ResolveSeatLocalOffset(seatIndex)),
                Quaternion.Euler(0f, vehicleState.transform.eulerAngles.y, 0f));
        }

        private bool TryResolveCurrentLocalSoloVehicle(
            out NetworkedVehicleState vehicleState,
            out NetworkedVehicleDriverController driverController,
            out LocalVehicleCameraRig cameraRig)
        {
            vehicleState = localSoloVehicleState;
            driverController = localSoloVehicleDriver;
            cameraRig = localSoloVehicleCameraRig;

            if (vehicleState == null)
            {
                return false;
            }

            if (driverController == null)
            {
                driverController = vehicleState.GetComponent<NetworkedVehicleDriverController>();
                localSoloVehicleDriver = driverController;
            }

            if (cameraRig == null)
            {
                cameraRig = vehicleState.GetComponent<LocalVehicleCameraRig>();
                localSoloVehicleCameraRig = cameraRig;
            }

            return driverController != null;
        }

        private bool TryResolveNearestLocalSoloVehicle(
            out NetworkedVehicleState vehicleState,
            out NetworkedVehicleDriverController driverController,
            out LocalVehicleCameraRig cameraRig,
            out float distance)
        {
            vehicleState = null;
            driverController = null;
            cameraRig = null;
            distance = float.PositiveInfinity;

            var vehicles = FindObjectsByType<NetworkedVehicleState>(FindObjectsInactive.Exclude);
            for (var i = 0; i < vehicles.Length; i++)
            {
                var candidate = vehicles[i];
                if (candidate == null)
                {
                    continue;
                }

                var candidateDriver = candidate.GetComponent<NetworkedVehicleDriverController>();
                if (candidateDriver == null)
                {
                    continue;
                }

                var candidateDistance = Vector3.Distance(activeLocalPlayer.transform.position, candidate.transform.position);
                if (candidateDistance >= distance)
                {
                    continue;
                }

                vehicleState = candidate;
                driverController = candidateDriver;
                cameraRig = candidate.GetComponent<LocalVehicleCameraRig>();
                distance = candidateDistance;
            }

            return vehicleState != null;
        }

        private void SuppressVehicleCamerasForLocalSeatExit()
        {
            var cameraRigs = FindObjectsByType<LocalVehicleCameraRig>(FindObjectsInactive.Exclude);
            for (var i = 0; i < cameraRigs.Length; i++)
            {
                if (cameraRigs[i] != null)
                {
                    cameraRigs[i].SuppressNetworkCameraUntilReleased();
                }
            }
        }

        private void RestoreLocalSoloOnFootControl()
        {
            RestoreLocalSoloOnFootControl(true, true);
        }

        private void RestoreLocalSoloOnFootControl(bool reactivateBody, bool enableMovement)
        {
            if (localSoloVehicleDriver != null)
            {
                localSoloVehicleDriver.SetLocalSoloDriverActive(false);
            }

            if (localSoloVehicleCameraRig != null)
            {
                localSoloVehicleCameraRig.SetLocalSoloCameraActive(false);
            }

            localSoloVehicleSeated = false;
            localSoloVehicleState = null;
            localSoloVehicleDriver = null;
            localSoloVehicleCameraRig = null;
            localSoloSeatIndex = NetworkedVehicleState.NoSeatIndex;

            SetLocalPlayerBodyActive(reactivateBody);
            var localOnFootController = activeLocalPlayer == null ? null : activeLocalPlayer.GetComponent<LocalOnFootController>();
            if (localOnFootController != null)
            {
                RestoreOnFootCamera(localOnFootController);
                localOnFootController.MovementEnabled = enableMovement;
            }
        }

        private void RestoreOnFootCamera(LocalOnFootController localOnFootController)
        {
            if (localOnFootController == null)
            {
                return;
            }

            var cameraToAttach = playerCamera == null ? localOnFootController.PlayerCamera : playerCamera;
            localOnFootController.AttachCamera(cameraToAttach);
        }

        private void ShowVehicleSeatMessage(string message)
        {
            if (checkpointHud != null)
            {
                checkpointHud.ShowVehicleSeatMessage(message);
            }
        }

        private void SynchronizeLocalSeatedPose()
        {
            if (activeLocalPlayer == null || localNetworkedPlayerState == null || !IsVehicleSeatMode(localNetworkedPlayerState.Mode.Value))
            {
                return;
            }

            var localOnFootController = activeLocalPlayer.GetComponent<LocalOnFootController>();
            if (localOnFootController != null)
            {
                localOnFootController.MovementEnabled = false;
            }

            SetLocalPlayerBodyActive(false);
            activeLocalPlayer.transform.SetPositionAndRotation(
                localNetworkedPlayerState.WorldPosition.Value,
                Quaternion.Euler(0f, localNetworkedPlayerState.YawDegrees.Value, 0f));
        }

        /// <summary>
        /// Pilote l'overlay plein ecran de mort (Story 2.7) depuis la transition reseau du joueur
        /// local -- jamais depuis le HUD lui-meme, meme invariant lecture-seule que Hearts/Stamina/
        /// Money. Dead n'ayant qu'une seule sortie possible (vers Alive, via TryRespawn), masquer des
        /// que l'etat precedent etait Dead suffit a couvrir tout retour au jeu. Gele/teleporte aussi
        /// le rig local reel (bug fix post-implementation) : NetworkedPlayerLifecycleService.TryRespawn
        /// ne repositionne que le proxy NetworkedPlayerState.WorldPosition, jamais ce GameObject --
        /// sans ce geste, il continuerait de tomber et NetworkedLocalPlayerPoseReporter re-ecraserait
        /// aussitot la position que le host vient de remettre.
        /// </summary>
        private void HandleLifecycleChanged(PlayerLifecycle previousValue, PlayerLifecycle newValue)
        {
            var localOnFootController = activeLocalPlayer == null ? null : activeLocalPlayer.GetComponent<LocalOnFootController>();

            if (newValue == PlayerLifecycle.Dead)
            {
                if (localOnFootController != null)
                {
                    localOnFootController.MovementEnabled = false;
                }

                if (checkpointHud != null)
                {
                    checkpointHud.ShowDeathOverlay();
                }

                SuppressVehicleCamerasForLocalSeatExit();
            }
            else if (previousValue == PlayerLifecycle.Dead)
            {
                if (localOnFootController != null)
                {
                    var spawn = ResolveSpawnPoint();
                    var spawnPosition = spawn == null ? Vector3.zero : spawn.position;
                    var spawnRotation = spawn == null ? Quaternion.identity : spawn.rotation;
                    var canReactivateBody = localNetworkedPlayerState == null || !IsVehicleSeatMode(localNetworkedPlayerState.Mode.Value);
                    SetLocalPlayerBodyActive(canReactivateBody);
                    localOnFootController.Teleport(spawnPosition, spawnRotation);
                    if (canReactivateBody)
                    {
                        RestoreOnFootCamera(localOnFootController);
                    }

                    localOnFootController.MovementEnabled = canReactivateBody;
                }

                if (checkpointHud != null)
                {
                    checkpointHud.HideDeathOverlay();
                }
            }
        }

        private void RefreshLocalSoloDeathRecovery()
        {
            if (IsNetworkSessionActive() || activeLocalPlayer == null)
            {
                return;
            }

            var localVoidRespawnController = activeLocalPlayer.GetComponent<LocalVoidRespawnController>();
            if (localVoidRespawnController == null)
            {
                return;
            }

            if (localVoidRespawnController.IsDead)
            {
                if (localSoloVehicleSeated)
                {
                    RestoreLocalSoloOnFootControl(false, false);
                }

                localSoloPlayerWasDead = true;
                return;
            }

            if (!localSoloPlayerWasDead)
            {
                return;
            }

            localSoloPlayerWasDead = false;
            SetLocalPlayerBodyActive(true);

            var localOnFootController = activeLocalPlayer.GetComponent<LocalOnFootController>();
            if (localOnFootController != null)
            {
                RestoreOnFootCamera(localOnFootController);
                localOnFootController.MovementEnabled = true;
            }
        }

        /// <summary>
        /// Meme resolution de point de spawn que TrySpawnSelectedProfile (playerSpawnPoint si
        /// renseigne, sinon compositionRoot.SpawnRoot) -- factorisee pour que HandleLifecycleChanged
        /// (bug fix post-implementation) teleporte le rig local reel vers exactement le meme point.
        /// </summary>
        private Transform ResolveSpawnPoint()
        {
            return playerSpawnPoint == null
                ? (compositionRoot == null ? null : compositionRoot.SpawnRoot)
                : playerSpawnPoint;
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

        /// <summary>
        /// Gating inverse de AttachNetworkPoseReporter (Story 2.7) : n'attache le chemin de chute/
        /// respawn 100% local que lorsque le chemin reseau host-owned est inactif, pour que la partie
        /// solo hors-ligne dispose aussi d'une boucle de test chute/respawn -- jamais les deux a la
        /// fois sur le meme joueur.
        /// </summary>
        private static void AttachLocalVoidRespawnController(GameObject localPlayer, RunCheckpointHudScreen checkpointHud)
        {
            var manager = NetworkManager.Singleton;
            var isNetworked = manager != null && manager.IsListening;
            if (localPlayer == null || isNetworked)
            {
                return;
            }

            var controller = localPlayer.GetComponent<LocalVoidRespawnController>();
            if (controller == null)
            {
                controller = localPlayer.AddComponent<LocalVoidRespawnController>();
            }

            controller.CheckpointHud = checkpointHud;
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

        private static bool IsNetworkSessionActive()
        {
            var manager = NetworkManager.Singleton;
            return manager != null && manager.IsListening;
        }

        private static int ResolveLocalSoloSeatIndex(bool preferPassenger)
        {
            return preferPassenger ? NetworkedVehicleState.FirstPassengerSeatIndex : NetworkedVehicleState.DriverSeatIndex;
        }

        private static bool IsDriverSeat(int seatIndex)
        {
            return seatIndex == NetworkedVehicleState.DriverSeatIndex;
        }

        private static string ResolveSeatOccupiedMessage(int seatIndex)
        {
            return IsDriverSeat(seatIndex) ? "Siege conducteur occupe." : "Siege passager " + seatIndex + " occupe.";
        }

        private static bool IsVehicleSeatMode(PlayerMode mode)
        {
            return mode == PlayerMode.Driver || mode == PlayerMode.Passenger;
        }

        private void SetLocalPlayerBodyActive(bool active)
        {
            if (activeLocalPlayerVisual != null && activeLocalPlayerVisual.activeSelf != active)
            {
                activeLocalPlayerVisual.SetActive(active);
            }

            var characterController = activeLocalPlayer == null ? null : activeLocalPlayer.GetComponent<CharacterController>();
            if (characterController != null && characterController.enabled != active)
            {
                characterController.enabled = active;
            }
        }
    }
}
