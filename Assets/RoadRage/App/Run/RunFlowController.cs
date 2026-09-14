using System;
using System.Collections.Generic;
using RoadRage.Features.OnFoot;
using RoadRage.Features.PassengerActions;
using RoadRage.Features.Players;
using RoadRage.Features.Rage;
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

        public const string NoRageTargetMessage = "Rage : aucune cible.";

        private const float PassengerActionOneRageDelta = 25f;

        private const float DevReactionEffectMagnitude = 25f;

        private const int MinimumMvpRageTargetCount = 3;

        private const int MinimumMvpDriveableRageVehicleCount = 2;

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

        [SerializeField]
        private PassengerActionCatalog passengerActionCatalog;

        [SerializeField]
        private NetworkedRageState passengerActionTarget;

        [SerializeField]
        private RageTuningDef passengerActionRageTuning;

        [SerializeField]
        private GameObject devVehiclePrefab;

        private GameObject activeLocalPlayer;

        private GameObject activeLocalPlayerVisual;

        private LocalOnFootController activeLocalOnFootController;

        private NetworkedPlayerState localNetworkedPlayerState;

        private bool hudRuntimeBound;

        private bool networkHudBridgeActive;

        private PassengerActionDebugView passengerActionView;

        private NetworkedPassengerActionIntent boundPassengerActionIntent;

        private NetworkedPassengerActionIntent subscribedPassengerActionIntent;

        private readonly HashSet<NetworkedPassengerActionIntent> authoritativePassengerActionIntents = new HashSet<NetworkedPassengerActionIntent>();

        private int focusedRageTargetIndex;

        private NetworkedVehicleDriverController subscribedVehicleDriverController;

        private NetworkedVehicleState subscribedVehicleState;

        private bool subscribedVehicleStateCallbacks;

        private readonly HashSet<NetworkedVehicleDriverController> secondaryDamageWiredVehicles = new HashSet<NetworkedVehicleDriverController>();

        public GameObject ActiveLocalPlayer
        {
            get { return activeLocalPlayer; }
        }

        private void Start()
        {
            EnsureNetworkSessionMonitor();
            EnsureNetworkHudBridge();
            EnsureMinimumMvpRageTargets();
            RefreshFocusedRageTarget();

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
            ResolveLocalNetworkedPlayerStateIfNeeded();
            EnsurePassengerActionBinding();
            EnsureAuthoritativePassengerActionBindings();
            RefreshPassengerActionIncidentHud();
            HandleRageTargetDevControls();
            RefreshFocusedRageHud();
            RefreshLocalReviveCountdown();
            SynchronizeLocalSeatedPose();
            EnsureVehicleEventBridge();
            EnsureSecondaryVehicleDamageBridges();
            HandleVehicleHornInteraction();
            HandleRageTargetCameraLockInteraction();
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

        private void RefreshPassengerActionIncidentHud()
        {
            if (checkpointHud == null)
            {
                return;
            }

            var incidentState = FindAnyObjectByType<NetworkedPassengerActionIncidentState>();
            checkpointHud.ShowPassengerActionIncidentStatus(incidentState == null ? 0 : incidentState.ActivationCount.Value);
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
            UnsubscribeFromLocalOnFootController();
            UnsubscribeFromVehicleEvents();
            UnsubscribeFromPassengerActionIntent();
            foreach (var intent in authoritativePassengerActionIntents)
            {
                if (intent != null)
                {
                    intent.ActionValidated -= HandlePassengerActionValidated;
                }
            }
            authoritativePassengerActionIntents.Clear();
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

            // Le spawn solo consomme la meme selection gelee que le payload reseau (Story 4.6) : plus
            // aucune lecture live du profil courant en aval du menu.
            if (characterCatalog == null || !characterCatalog.TryGetById(bootstrap.Profiles.SessionSelection.CharacterId, out var character) || character == null)
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
            SubscribeToLocalOnFootController(onFootController);
            AttachNetworkPoseReporter(activeLocalPlayer);
            AttachLocalVoidRespawnController(activeLocalPlayer, checkpointHud);

            if (checkpointHud != null)
            {
                checkpointHud.ShowRunState(ResolveLobbyState(), bootstrap.Profiles.SessionSelection.DisplayName, character.DisplayName);
            }

            BindHudRuntimeState();
            EnsurePassengerActionBinding();

            Debug.Log("[Run] Joueur local spawn : " + bootstrap.Profiles.SessionSelection.DisplayName + " / " + character.Id);
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
                NetworkedPlayerState.DefaultMaxHp,
                NetworkedPlayerState.DefaultMaxHp,
                NetworkedPlayerState.DefaultStaminaNormalized,
                ResolveConnectedPlayerCount(),
                NetworkedPlayerState.DefaultMoney);
        }

        private void EnsurePassengerActionBinding()
        {
            if (activeLocalPlayer == null || passengerActionCatalog == null)
            {
                return;
            }

            if (passengerActionTarget == null)
            {
                passengerActionTarget = FindAnyObjectByType<NetworkedRageState>();
            }
            else
            {
                RefreshFocusedRageTarget();
            }

            if (passengerActionView == null)
            {
                var viewRoot = checkpointHud == null ? gameObject : checkpointHud.gameObject;
                passengerActionView = viewRoot.GetComponent<PassengerActionDebugView>();
                if (passengerActionView == null)
                {
                    passengerActionView = viewRoot.AddComponent<PassengerActionDebugView>();
                }
            }

            NetworkedPassengerActionIntent intent;
            if (IsNetworkSessionActive())
            {
                intent = localNetworkedPlayerState == null ? null : localNetworkedPlayerState.GetComponent<NetworkedPassengerActionIntent>();
            }
            else
            {
                intent = activeLocalPlayer.GetComponent<NetworkedPassengerActionIntent>();
                if (intent == null)
                {
                    intent = activeLocalPlayer.AddComponent<NetworkedPassengerActionIntent>();
                }
            }

            if (intent == null || intent == boundPassengerActionIntent)
            {
                if (intent != null)
                {
                    intent.SetTarget(passengerActionTarget);
                }

                return;
            }

            UnsubscribeFromPassengerActionIntent();
            boundPassengerActionIntent = intent;
            subscribedPassengerActionIntent = intent;
            intent.ActionValidated += HandlePassengerActionValidated;
            intent.Configure(
                passengerActionCatalog,
                FindAnyObjectByType<NetworkedRunState>(),
                FindAnyObjectByType<NetworkedVehicleState>(),
                passengerActionTarget,
                checkpointHud,
                passengerActionView,
                () => localNetworkedPlayerState != null && NetworkedVehicleState.IsPassengerSeatIndex(localNetworkedPlayerState.SeatIndex.Value),
                () => activeLocalPlayer == null ? Vector3.zero : activeLocalPlayer.transform.position,
                FindAnyObjectByType<NetworkedPassengerActionIncidentState>());
        }

        private void UnsubscribeFromPassengerActionIntent()
        {
            if (subscribedPassengerActionIntent != null)
            {
                subscribedPassengerActionIntent.ActionValidated -= HandlePassengerActionValidated;
                subscribedPassengerActionIntent = null;
            }
        }

        private void EnsureAuthoritativePassengerActionBindings()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || !manager.IsServer)
            {
                return;
            }

            // ponytail: scans player intents while the host is active; subscribe from the spawn service if player counts grow.
            var intents = FindObjectsByType<NetworkedPassengerActionIntent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var intent in intents)
            {
                if (intent != null && intent != subscribedPassengerActionIntent && authoritativePassengerActionIntents.Add(intent))
                {
                    intent.ActionValidated += HandlePassengerActionValidated;
                }
            }
        }

        private void HandlePassengerActionValidated(PassengerActionDef action, Transform actor, NetworkedRageState target)
        {
            if (action == null || action.Slot != 0 || target == null)
            {
                return;
            }

            target.ApplyRageDelta(PassengerActionOneRageDelta, passengerActionRageTuning);
            if (target == passengerActionTarget)
            {
                RefreshFocusedRageHud();
            }
        }

        public void CycleFocusedRageTarget()
        {
            var targets = FindRageTargets();
            if (targets.Length == 0)
            {
                passengerActionTarget = null;
                checkpointHud?.ShowPassengerActionVerdict(NoRageTargetMessage);
                checkpointHud?.ShowRageStatus(null, 0f, null);
                return;
            }

            var currentIndex = Array.IndexOf(targets, passengerActionTarget);
            focusedRageTargetIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % targets.Length;
            SetFocusedRageTarget(targets[focusedRageTargetIndex], true);
        }

        public bool TrySpawnDevVehicle(bool rageTarget)
        {
            if (!TryFindDevVehicleSpawnPose(out var position, out var rotation))
            {
                checkpointHud?.ShowVehicleSeatMessage("Spawn dev refuse : aucune position libre.");
                return false;
            }

            var vehicle = CreateDevVehicle(rageTarget ? "DevRageTargetVehicle" : "DevVehicle", position, rotation, rageTarget);
            if (vehicle == null)
            {
                return false;
            }

            checkpointHud?.ShowVehicleSeatMessage(rageTarget ? "Spawn dev rage-target." : "Spawn dev vehicule.");
            if (rageTarget)
            {
                SetFocusedRageTarget(vehicle.GetComponent<NetworkedRageState>(), true);
            }

            return true;
        }

        private void HandleRageTargetDevControls()
        {
            if (IsNetworkSessionActive())
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.cKey.wasPressedThisFrame)
            {
                CycleFocusedRageTarget();
            }

            if (keyboard.vKey.wasPressedThisFrame)
            {
                TrySpawnDevVehicle(keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
            }

            if (keyboard.yKey.wasPressedThisFrame)
            {
                TryApplyFocusedRageReaction(
                    keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed
                        ? ReactionChannel.Both
                        : ReactionChannel.Fear);
            }
        }

        /// <summary>
        /// Declencheur de verification Story 5.1 dans MVP_Run : Y applique la peur a la cible
        /// focalisee, Shift+Y applique rage et peur. Reserve au solo et a l'hote.
        /// </summary>
        public bool TryApplyFocusedRageReaction(ReactionChannel channel)
        {
            if (!IsAuthoritativeForDamage()
                || passengerActionTarget == null
                || passengerActionRageTuning == null
                || channel == ReactionChannel.None)
            {
                return false;
            }

            var effect = new NpcReactionEffect(channel, DevReactionEffectMagnitude);
            if (!effect.TryValidate(out _))
            {
                return false;
            }

            passengerActionTarget.ApplyReactionEffect(effect, passengerActionRageTuning);
            RefreshFocusedRageHud();
            checkpointHud?.ShowPassengerActionVerdict(channel == ReactionChannel.Fear
                ? "Test peur applique a la cible focalisee."
                : "Test rage et peur applique a la cible focalisee.");
            return true;
        }

        private void RefreshFocusedRageTarget()
        {
            var targets = FindRageTargets();
            if (targets.Length == 0)
            {
                passengerActionTarget = null;
                return;
            }

            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i] == passengerActionTarget)
                {
                    focusedRageTargetIndex = i;
                    return;
                }
            }

            focusedRageTargetIndex = Mathf.Clamp(focusedRageTargetIndex, 0, targets.Length - 1);
            SetFocusedRageTarget(targets[focusedRageTargetIndex], false);
        }

        private void SetFocusedRageTarget(NetworkedRageState target, bool moveCamera)
        {
            passengerActionTarget = target;
            boundPassengerActionIntent?.SetTarget(target);
            RefreshFocusedRageHud();

            if (!moveCamera || target == null || playerCamera == null)
            {
                return;
            }

            playerCamera.transform.position = target.transform.position + new Vector3(0f, 5f, -8f);
            playerCamera.transform.LookAt(target.transform.position + Vector3.up);
        }

        private void RefreshFocusedRageHud()
        {
            if (passengerActionTarget == null)
            {
                checkpointHud?.ShowRageStatus(null, 0f, null);
                return;
            }

            checkpointHud?.ShowRageStatus(
                passengerActionTarget.gameObject.name,
                passengerActionTarget.RageValue.Value,
                passengerActionTarget.FearValue.Value,
                passengerActionTarget.Disposition.Value.ToString());
        }

        /// <summary>
        /// Bug fix (hors Story 4.3, regression report du 2026-09-12) : sans garde d'autorite, un client
        /// reseau non-host executait aussi cette creation localement (Start() tourne sur chaque pair),
        /// donc en plus des vehicules repliques par le host il instanciait ses propres doublons non
        /// reseautes -- d'ou des positions differentes entre solo/host et host/client. Seul le host (ou
        /// l'absence de session reseau, cas solo) peut creer ces vehicules, comme IsAuthoritativeForDamage.
        ///
        /// Bug fix (hors Story 5.4, regression report du 2026-09-14) : depuis que les IA de route
        /// (Story 5.4) portent leur propre NetworkedRageState, elles suffisent seules a atteindre
        /// MinimumMvpRageTargetCount sans jamais etre pilotables -- le premier repli tombait alors a
        /// zero vehicule de secours pilotable. Les deux minimums sont donc verifies independamment.
        /// </summary>
        private void EnsureMinimumMvpRageTargets()
        {
            if (!string.Equals(gameObject.scene.name, "MVP_Run", StringComparison.Ordinal) || !IsAuthoritativeForDamage())
            {
                return;
            }

            var targets = FindRageTargets();
            for (var i = targets.Length; i < MinimumMvpRageTargetCount; i++)
            {
                CreateDevVehicle("MVP_RageTargetVehicle_" + i, new Vector3(6f + (i * 4f), 0f, -44f), Quaternion.identity, true);
            }

            var driveableRageVehicleCount = CountDriveableRageVehicles(FindRageTargets());
            for (var i = driveableRageVehicleCount; i < MinimumMvpDriveableRageVehicleCount; i++)
            {
                CreateDevVehicle("MVP_DriveableRageTargetVehicle_" + i, new Vector3(6f + ((MinimumMvpRageTargetCount + i) * 4f), 0f, -44f), Quaternion.identity, true);
            }
        }

        private static int CountDriveableRageVehicles(NetworkedRageState[] targets)
        {
            var count = 0;
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i].GetComponent<NetworkedVehicleDriverController>() != null)
                {
                    count++;
                }
            }

            return count;
        }

        private bool TryFindDevVehicleSpawnPose(out Vector3 position, out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            var origin = ResolveSpawnPoint();
            var basePosition = origin == null ? Vector3.zero : origin.position;
            var baseRotation = origin == null ? Quaternion.identity : origin.rotation;
            rotation = baseRotation;

            for (var i = 0; i < 8; i++)
            {
                position = basePosition + (baseRotation * new Vector3(4f + (i * 3f), 0f, 0f));
                if (!Physics.CheckBox(position + new Vector3(0f, 0.75f, 0f), new Vector3(1.2f, 0.75f, 2.4f), rotation))
                {
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        private static NetworkedRageState[] FindRageTargets()
        {
            var targets = FindObjectsByType<NetworkedRageState>(FindObjectsInactive.Exclude);
            Array.Sort(targets, (left, right) => left.GetEntityId().CompareTo(right.GetEntityId()));
            return targets;
        }

        private GameObject CreateDevVehicle(string objectName, Vector3 position, Quaternion rotation, bool rageTarget)
        {
            return DevVehicleSpawner.Create(devVehiclePrefab, objectName, position, rotation, rageTarget);
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

        /// <summary>
        /// Relais evenement C# -> HUD (Story 3.4) : la voiture partagee est un objet unique de scene,
        /// retrouve paresseusement via FindAnyObjectByType, puis abonne une seule fois. Depuis la
        /// Story 5.3, le solo est toujours host-authoritative comme l'en ligne : les evenements sont
        /// toujours leves cote host -- RunFlowController ne fait ici que consommer l'evenement C#
        /// expose par le module Vehicules, jamais l'inverse.
        /// </summary>
        private void EnsureVehicleEventBridge()
        {
            if (subscribedVehicleDriverController != null)
            {
                return;
            }

            var driverController = FindAnyObjectByType<NetworkedVehicleDriverController>();
            if (driverController == null)
            {
                return;
            }

            driverController.VehicleCollided += HandleVehicleCollided;
            driverController.VehicleRecovered += HandleVehicleRecovered;
            subscribedVehicleDriverController = driverController;
            subscribedVehicleState = driverController.GetComponent<NetworkedVehicleState>();
            SubscribeToVehicleState(subscribedVehicleState);
            RefreshVehicleDamageHud();
        }

        private void UnsubscribeFromVehicleEvents()
        {
            if (subscribedVehicleDriverController == null)
            {
                return;
            }

            subscribedVehicleDriverController.VehicleCollided -= HandleVehicleCollided;
            subscribedVehicleDriverController.VehicleRecovered -= HandleVehicleRecovered;
            UnsubscribeFromVehicleState();
            subscribedVehicleDriverController = null;
            subscribedVehicleState = null;
        }

        /// <summary>
        /// Bug fix (hors Story 4.3, regression report du 2026-09-12) : EnsureVehicleEventBridge ne
        /// cable les degats que sur UN SEUL vehicule (celui du joueur). Avec Story 4.3 ajoutant
        /// plusieurs vehicules rage-target/dev, tous les autres ne prenaient jamais de degats, meme
        /// vides ou avec un passager. Chaque vehicule additionnel recoit ici son propre pont de degats,
        /// independant du HUD/joueur solo qui reste sur subscribedVehicleDriverController.
        /// </summary>
        private void EnsureSecondaryVehicleDamageBridges()
        {
            var vehicles = FindObjectsByType<NetworkedVehicleDriverController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var vehicle in vehicles)
            {
                if (vehicle == null || vehicle == subscribedVehicleDriverController || !secondaryDamageWiredVehicles.Add(vehicle))
                {
                    continue;
                }

                var vehicleState = vehicle.GetComponent<NetworkedVehicleState>();
                vehicle.VehicleCollided += impactSpeed => ApplySecondaryVehicleCollisionDamage(vehicleState, impactSpeed);
            }
        }

        /// <summary>
        /// Meme calcul de degats que ApplyNetworkedCollisionDamage (Story 3.5), mais applique au
        /// vehicule qui a effectivement collisionne plutot qu'au seul vehicule observe par le HUD.
        /// Fonctionne sans conducteur ni passager : seul le PV du vehicule est mute dans ce cas.
        /// </summary>
        private void ApplySecondaryVehicleCollisionDamage(NetworkedVehicleState vehicleState, float impactSpeed)
        {
            if (!IsAuthoritativeForDamage() || vehicleState == null)
            {
                return;
            }

            var vehicleDamage = NetworkedVehicleDriverController.ComputeCollisionDamage(impactSpeed);
            if (vehicleDamage <= 0)
            {
                return;
            }

            if (IsNetworkSessionActive())
            {
                var service = NetworkedPlayerLifecycleService.Instance;
                if (service != null)
                {
                    for (var seatIndex = NetworkedVehicleState.DriverSeatIndex; seatIndex < NetworkedVehicleState.SeatCount; seatIndex++)
                    {
                        if (vehicleState.TryGetSeatOccupant(seatIndex, out var clientId) && clientId != NetworkedVehicleState.UnoccupiedSeatClientId)
                        {
                            service.ApplyCollisionDamage(clientId, NetworkedPlayerLifecycleService.PlayerCollisionDamage);
                        }
                    }
                }
            }

            vehicleState.ApplyDamage(vehicleDamage);
        }

        /// <summary>
        /// Relais HUD (Story 3.4) toujours affiche sur chaque client, puis pont de degats Story 3.5
        /// (joueurs assis + voiture) -- host-authoritative uniquement (AD-3/AD-18) : chaque client
        /// recoit le meme evenement (relaye par NotifyVehicleCollidedRpc), mais seul le host (ou la
        /// partie solo, ou aucun NetworkManager n'ecoute) calcule et applique reellement les degats ;
        /// les autres clients reseau se contentent de lire NetworkedVehicleState.Hp (deja synchronise)
        /// pour le message "voiture hors d'usage", jamais de le muter.
        /// </summary>
        private void HandleVehicleCollided(float impactSpeed)
        {
            if (checkpointHud != null)
            {
                checkpointHud.ShowVehicleCollisionMessage();
            }

            ApplyCollisionConsequencesIfAuthoritative(impactSpeed);
            ShowVehicleInoperableMessageIfNeeded();
        }

        private void ApplyCollisionConsequencesIfAuthoritative(float impactSpeed)
        {
            if (!IsAuthoritativeForDamage())
            {
                return;
            }

            var vehicleDamage = NetworkedVehicleDriverController.ComputeCollisionDamage(impactSpeed);
            ApplyNetworkedCollisionDamage(vehicleDamage);
        }

        private void ApplyNetworkedCollisionDamage(int vehicleDamage)
        {
            if (subscribedVehicleState == null)
            {
                return;
            }

            var service = NetworkedPlayerLifecycleService.Instance;
            if (service != null)
            {
                for (var seatIndex = NetworkedVehicleState.DriverSeatIndex; seatIndex < NetworkedVehicleState.SeatCount; seatIndex++)
                {
                    if (subscribedVehicleState.TryGetSeatOccupant(seatIndex, out var clientId) && clientId != NetworkedVehicleState.UnoccupiedSeatClientId)
                    {
                        service.ApplyCollisionDamage(clientId, NetworkedPlayerLifecycleService.PlayerCollisionDamage);
                    }
                }
            }

            if (vehicleDamage > 0)
            {
                subscribedVehicleState.ApplyDamage(vehicleDamage);
            }

            RefreshVehicleDamageHud();
        }

        private void ShowVehicleInoperableMessageIfNeeded()
        {
            var vehicleState = ResolveObservedVehicleState();
            if (vehicleState != null && vehicleState.IsInoperable() && checkpointHud != null)
            {
                checkpointHud.ShowVehicleInoperableMessage();
            }
        }

        private void SubscribeToVehicleState(NetworkedVehicleState vehicleState)
        {
            if (vehicleState == null || subscribedVehicleStateCallbacks)
            {
                return;
            }

            vehicleState.Hp.OnValueChanged += HandleVehicleHpChanged;
            vehicleState.WheelDamaged.OnValueChanged += HandleVehicleDamageFlagChanged;
            vehicleState.EngineDamaged.OnValueChanged += HandleVehicleDamageFlagChanged;
            vehicleState.BrakeDamaged.OnValueChanged += HandleVehicleDamageFlagChanged;
            subscribedVehicleStateCallbacks = true;
        }

        private void UnsubscribeFromVehicleState()
        {
            if (subscribedVehicleState == null || !subscribedVehicleStateCallbacks)
            {
                subscribedVehicleStateCallbacks = false;
                return;
            }

            subscribedVehicleState.Hp.OnValueChanged -= HandleVehicleHpChanged;
            subscribedVehicleState.WheelDamaged.OnValueChanged -= HandleVehicleDamageFlagChanged;
            subscribedVehicleState.EngineDamaged.OnValueChanged -= HandleVehicleDamageFlagChanged;
            subscribedVehicleState.BrakeDamaged.OnValueChanged -= HandleVehicleDamageFlagChanged;
            subscribedVehicleStateCallbacks = false;
        }

        private void HandleVehicleHpChanged(int previousValue, int newValue)
        {
            RefreshVehicleDamageHud();
            ShowVehicleInoperableMessageIfNeeded();
        }

        private void HandleVehicleDamageFlagChanged(bool previousValue, bool newValue)
        {
            RefreshVehicleDamageHud();
        }

        private void RefreshVehicleDamageHud()
        {
            if (checkpointHud == null)
            {
                return;
            }

            var vehicleState = ResolveObservedVehicleState();
            if (vehicleState == null)
            {
                return;
            }

            checkpointHud.SetVehicleDamageStatus(
                vehicleState.CurrentHp,
                NetworkedVehicleState.DefaultMaxHp,
                vehicleState.IsWheelDamaged,
                vehicleState.IsEngineDamaged,
                vehicleState.IsBrakeDamaged);
        }

        private NetworkedVehicleState ResolveObservedVehicleState()
        {
            return subscribedVehicleState;
        }

        /// <summary>
        /// Autorite de degats (Story 3.5, AD-3/AD-18) : le host (ou l'absence totale de NetworkManager
        /// en ecoute, cas solo) peut calculer/appliquer des degats ; un client reseau non-host ne le
        /// peut jamais -- meme garde que les autres chemins ApplyServer*/Try* de ce fichier.
        /// </summary>
        private static bool IsAuthoritativeForDamage()
        {
            var manager = NetworkManager.Singleton;
            return manager == null || !manager.IsListening || manager.IsServer;
        }

        private void HandleVehicleRecovered()
        {
            if (checkpointHud != null)
            {
                checkpointHud.ShowVehicleRecoveredMessage();
            }
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

        private void SubscribeToLocalOnFootController(LocalOnFootController controller)
        {
            if (activeLocalOnFootController == controller)
            {
                return;
            }

            UnsubscribeFromLocalOnFootController();
            activeLocalOnFootController = controller;

            if (activeLocalOnFootController != null)
            {
                activeLocalOnFootController.StaminaChanged += HandleLocalStaminaChanged;
            }
        }

        private void UnsubscribeFromLocalOnFootController()
        {
            if (activeLocalOnFootController == null)
            {
                return;
            }

            activeLocalOnFootController.StaminaChanged -= HandleLocalStaminaChanged;
            activeLocalOnFootController = null;
        }

        private void HandleLocalStaminaChanged(float staminaNormalized)
        {
            if (checkpointHud != null)
            {
                checkpointHud.SetStamina(staminaNormalized);
            }
        }

        private void SubscribeToLocalNetworkedPlayerState(NetworkedPlayerState state)
        {
            state.Hp.OnValueChanged += HandleHpChanged;
            state.ReviveDeadlineTime.OnValueChanged += HandleReviveDeadlineChanged;
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

            localNetworkedPlayerState.Hp.OnValueChanged -= HandleHpChanged;
            localNetworkedPlayerState.ReviveDeadlineTime.OnValueChanged -= HandleReviveDeadlineChanged;
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

            checkpointHud.SetHp(localNetworkedPlayerState.Hp.Value, NetworkedPlayerState.DefaultMaxHp);
            RefreshLocalReviveCountdown();
            checkpointHud.SetStamina(activeLocalOnFootController == null
                ? localNetworkedPlayerState.StaminaNormalized.Value
                : activeLocalOnFootController.StaminaNormalized);
            checkpointHud.SetMoney(localNetworkedPlayerState.Money.Value);
        }

        private void HandleHpChanged(int previousValue, int newValue)
        {
            if (checkpointHud == null)
            {
                return;
            }

            checkpointHud.SetHp(newValue, NetworkedPlayerState.DefaultMaxHp);
        }

        private void HandleReviveDeadlineChanged(double previousValue, double newValue)
        {
            RefreshLocalReviveCountdown();
        }

        private void RefreshLocalReviveCountdown()
        {
            if (checkpointHud == null || localNetworkedPlayerState == null || localNetworkedPlayerState.Lifecycle.Value != PlayerLifecycle.Downed)
            {
                return;
            }

            var deadline = localNetworkedPlayerState.ReviveDeadlineTime.Value;
            if (deadline < 0d)
            {
                checkpointHud.ShowPlayerDownedMessage();
                return;
            }

            checkpointHud.ShowPlayerDownedCountdown((float)(deadline - ResolveNetworkTime()));
        }

        private static double ResolveNetworkTime()
        {
            var manager = NetworkManager.Singleton;
            return manager != null ? manager.ServerTime.Time : Time.timeAsDouble;
        }

        private void HandleStaminaChanged(float previousValue, float newValue)
        {
            if (checkpointHud != null && activeLocalOnFootController == null)
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
                    localOnFootController.SetCameraActive(false);
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

        /// <summary>Klaxon (hors story, 2026-09-12) : reserve au conducteur, en solo comme en reseau.</summary>
        private void HandleVehicleHornInteraction()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.hKey.wasPressedThisFrame)
            {
                return;
            }

            if (TryResolveLocalSeatedVehicle(out var vehicleState, out var driverController)
                && IsDriverSeat(vehicleState.FindSeatIndex(NetworkManager.Singleton.LocalClientId)))
            {
                driverController.RequestHonk();
            }
        }

        /// <summary>
        /// Verrouillage camera sur la rage target focalisee (hors story, 2026-09-12) : demande passager
        /// ET conducteur, bascule avec T. La camera suit la cible en continu (LocalVehicleCameraRig)
        /// tant que le verrouillage reste actif, y compris si la cible bouge encore.
        /// </summary>
        private void HandleRageTargetCameraLockInteraction()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.tKey.wasPressedThisFrame)
            {
                return;
            }

            if (!TryResolveLocalSeatedVehicleCameraRig(out var cameraRig))
            {
                return;
            }

            if (cameraRig.HasRageTargetLookOverride)
            {
                cameraRig.SetRageTargetLookOverride(null);
                ShowVehicleSeatMessage("Verrouillage camera desactive.");
                return;
            }

            if (passengerActionTarget == null)
            {
                ShowVehicleSeatMessage(NoRageTargetMessage);
                return;
            }

            cameraRig.SetRageTargetLookOverride(passengerActionTarget.transform);
            ShowVehicleSeatMessage("Camera verrouillee sur la cible rage.");
        }

        private bool TryResolveLocalSeatedVehicle(out NetworkedVehicleState vehicleState, out NetworkedVehicleDriverController driverController)
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsClient)
            {
                vehicleState = null;
                driverController = null;
                return false;
            }

            var states = FindObjectsByType<NetworkedVehicleState>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var candidate in states)
            {
                if (candidate.FindSeatIndex(manager.LocalClientId) == NetworkedVehicleState.NoSeatIndex)
                {
                    continue;
                }

                vehicleState = candidate;
                driverController = candidate.GetComponent<NetworkedVehicleDriverController>();
                return driverController != null;
            }

            vehicleState = null;
            driverController = null;
            return false;
        }

        private bool TryResolveLocalSeatedVehicleCameraRig(out LocalVehicleCameraRig cameraRig)
        {
            if (TryResolveLocalSeatedVehicle(out var vehicleState, out _))
            {
                cameraRig = vehicleState.GetComponent<LocalVehicleCameraRig>();
                return cameraRig != null;
            }

            cameraRig = null;
            return false;
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
                localOnFootController.SetCameraActive(false);
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

            if (localOnFootController != null)
            {
                if (newValue == PlayerLifecycle.Downed)
                {
                    localOnFootController.IsDowned = true;
                }
                else if (previousValue == PlayerLifecycle.Downed)
                {
                    localOnFootController.IsDowned = false;
                }
            }

            if (newValue == PlayerLifecycle.Downed)
            {
                if (checkpointHud != null)
                {
                    checkpointHud.ShowPlayerDownedMessage();
                    RefreshLocalReviveCountdown();
                }
            }
            else if (previousValue == PlayerLifecycle.Downed && newValue == PlayerLifecycle.Alive)
            {
                if (checkpointHud != null)
                {
                    checkpointHud.ShowPlayerRevivedMessage();
                }
            }

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

        private static bool IsDriverSeat(int seatIndex)
        {
            return seatIndex == NetworkedVehicleState.DriverSeatIndex;
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
