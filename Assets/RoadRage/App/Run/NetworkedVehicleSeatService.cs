using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Service host-only Story 3.3 : arbitre l'entree/sortie des sieges de la voiture partagee,
    /// puis projette cette verite dans NetworkedPlayerState.Mode/SeatIndex/WorldPosition. Le module
    /// Vehicules garde l'occupation brute ; App/Run seul connait les joueurs et le HUD.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedVehicleSeatService : MonoBehaviour
    {
        public const string VehicleUnavailableMessage = "Voiture partagee introuvable.";

        public const string PlayerUnavailableMessage = "Etat joueur introuvable.";

        public const string PlayerNotAliveMessage = "Entree vehicule refusee : joueur non vivant.";

        public const string PlayerNotOnFootMessage = "Entree vehicule refusee : joueur deja occupe.";

        public const string PlayerTooFarMessage = "Entree vehicule refusee : joueur trop loin.";

        public const string NoSeatAvailableMessage = "Entree vehicule refusee : aucun siege libre.";

        public const float DefaultEntryRadius = 5f;

        public static NetworkedVehicleSeatService Instance { get; private set; }

        [SerializeField]
        private NetworkedPlayerSpawnService spawnService;

        [SerializeField]
        private RunCheckpointHudScreen checkpointHud;

        [SerializeField]
        private NetworkedVehicleState vehicleState;

        [SerializeField]
        [Min(0f)]
        private float entryRadius = DefaultEntryRadius;

        private NetworkedVehicleDriverController driverController;
        private bool isActiveHost;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[Vehicles] Deuxieme NetworkedVehicleSeatService detecte ; le premier reste actif.");
                enabled = false;
                return;
            }

            Instance = this;
            CacheReferences();

            var manager = NetworkManager.Singleton;
            isActiveHost = manager != null && manager.IsListening && manager.IsServer;
            if (isActiveHost)
            {
                manager.OnClientDisconnectCallback += HandleClientDisconnected;
            }
        }

        private void OnDestroy()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                manager.OnClientDisconnectCallback -= HandleClientDisconnected;
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

            CacheReferences();
            if (vehicleState == null || spawnService == null)
            {
                return;
            }

            ReleaseInvalidOccupants();
            SynchronizeSeatedPlayerPoses();
        }

        public void Configure(NetworkedPlayerSpawnService playerSpawnService, RunCheckpointHudScreen hud)
        {
            if (playerSpawnService != null)
            {
                spawnService = playerSpawnService;
            }

            if (hud != null)
            {
                checkpointHud = hud;
            }

            CacheReferences();
        }

        public void RequestEnterOrExit(ulong clientId, bool preferPassenger)
        {
            if (!isActiveHost)
            {
                return;
            }

            CacheReferences();
            if (vehicleState == null)
            {
                Report(VehicleUnavailableMessage, true);
                return;
            }

            if (!TryResolvePlayer(clientId, out var playerState))
            {
                Report(PlayerUnavailableMessage, true);
                return;
            }

            if (playerState.SeatIndex.Value != NetworkedVehicleState.NoSeatIndex || IsSeatedMode(playerState.Mode.Value))
            {
                TryExitSeat(clientId, playerState);
                return;
            }

            TryEnterSeat(clientId, playerState, preferPassenger);
        }

        public static bool CanEnterSeat(PlayerMode mode, PlayerLifecycle lifecycle, int seatIndex, float distanceToVehicle, float entryRadius)
        {
            return lifecycle == PlayerLifecycle.Alive
                && IsOnFootMode(mode)
                && seatIndex == NetworkedVehicleState.NoSeatIndex
                && distanceToVehicle <= Mathf.Max(0f, entryRadius);
        }

        public static bool IsOnFootMode(PlayerMode mode)
        {
            return mode == PlayerMode.OnFoot || mode == PlayerMode.OnFootStop || mode == PlayerMode.OnFootRageRoad;
        }

        public static bool IsSeatedMode(PlayerMode mode)
        {
            return mode == PlayerMode.Driver || mode == PlayerMode.Passenger;
        }

        public static bool ShouldReleaseOccupant(PlayerLifecycle lifecycle)
        {
            return lifecycle == PlayerLifecycle.Dead || lifecycle == PlayerLifecycle.Disconnected;
        }

        private void TryEnterSeat(ulong clientId, NetworkedPlayerState playerState, bool preferPassenger)
        {
            var distanceToVehicle = Vector3.Distance(playerState.WorldPosition.Value, vehicleState.transform.position);
            if (!CanEnterSeat(playerState.Mode.Value, playerState.Lifecycle.Value, playerState.SeatIndex.Value, distanceToVehicle, entryRadius))
            {
                Report(ResolveEntryRefusalMessage(playerState, distanceToVehicle), true);
                return;
            }

            if (!vehicleState.TryFindAvailableSeat(preferPassenger, out var seatIndex) || !vehicleState.TryAssignSeat(seatIndex, clientId))
            {
                Report(NoSeatAvailableMessage, true);
                return;
            }

            ApplySeatedState(playerState, seatIndex);
            Report(seatIndex == NetworkedVehicleState.DriverSeatIndex ? "Siege conducteur occupe." : "Siege passager " + seatIndex + " occupe.", false);
        }

        private void TryExitSeat(ulong clientId, NetworkedPlayerState playerState)
        {
            var seatIndex = playerState.SeatIndex.Value;
            if (!NetworkedVehicleState.IsValidSeatIndex(seatIndex)
                || !vehicleState.TryGetSeatOccupant(seatIndex, out var occupant)
                || occupant != clientId)
            {
                return;
            }

            var exitPosition = vehicleState.transform.TransformPoint(NetworkedVehicleState.ResolveExitLocalOffset(seatIndex));
            var exitYaw = NormalizeYaw(vehicleState.transform.eulerAngles.y);

            ReleaseVehicleSeat(seatIndex, clientId);
            playerState.WorldPosition.Value = exitPosition;
            playerState.YawDegrees.Value = exitYaw;
            playerState.SeatIndex.Value = NetworkedVehicleState.NoSeatIndex;
            playerState.Mode.Value = PlayerMode.OnFoot;

            Report("Sortie vehicule.", false);
        }

        private void ReleaseInvalidOccupants()
        {
            for (var seatIndex = NetworkedVehicleState.DriverSeatIndex; seatIndex < NetworkedVehicleState.SeatCount; seatIndex++)
            {
                if (!vehicleState.TryGetSeatOccupant(seatIndex, out var clientId) || clientId == NetworkedVehicleState.UnoccupiedSeatClientId)
                {
                    continue;
                }

                if (!TryResolvePlayer(clientId, out var playerState) || ShouldReleaseOccupant(playerState.Lifecycle.Value))
                {
                    ReleaseVehicleSeat(seatIndex, clientId);

                    if (playerState != null)
                    {
                        playerState.SeatIndex.Value = NetworkedVehicleState.NoSeatIndex;
                        playerState.Mode.Value = PlayerMode.OnFoot;
                    }

                    Report("Siege libere pour le client " + clientId + ".", true);
                }
            }
        }

        private void SynchronizeSeatedPlayerPoses()
        {
            for (var seatIndex = NetworkedVehicleState.DriverSeatIndex; seatIndex < NetworkedVehicleState.SeatCount; seatIndex++)
            {
                if (!vehicleState.TryGetSeatOccupant(seatIndex, out var clientId)
                    || clientId == NetworkedVehicleState.UnoccupiedSeatClientId
                    || !TryResolvePlayer(clientId, out var playerState))
                {
                    continue;
                }

                playerState.WorldPosition.Value = vehicleState.transform.TransformPoint(NetworkedVehicleState.ResolveSeatLocalOffset(seatIndex));
                playerState.YawDegrees.Value = NormalizeYaw(vehicleState.transform.eulerAngles.y);
            }
        }

        private bool TryResolvePlayer(ulong clientId, out NetworkedPlayerState playerState)
        {
            playerState = null;
            if (spawnService != null && spawnService.TryGetState(clientId, out playerState))
            {
                return true;
            }

            var candidates = FindObjectsByType<NetworkedPlayerState>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (candidate != null && candidate.IsSpawned && candidate.ClientId.Value == clientId)
                {
                    playerState = candidate;
                    return true;
                }
            }

            return false;
        }

        private string ResolveEntryRefusalMessage(NetworkedPlayerState playerState, float distanceToVehicle)
        {
            if (playerState.Lifecycle.Value != PlayerLifecycle.Alive)
            {
                return PlayerNotAliveMessage;
            }

            if (!IsOnFootMode(playerState.Mode.Value) || playerState.SeatIndex.Value != NetworkedVehicleState.NoSeatIndex)
            {
                return PlayerNotOnFootMessage;
            }

            if (distanceToVehicle > entryRadius)
            {
                return PlayerTooFarMessage;
            }

            return NoSeatAvailableMessage;
        }

        private void ReleaseVehicleSeat(int seatIndex, ulong clientId)
        {
            if (seatIndex == NetworkedVehicleState.DriverSeatIndex)
            {
                CacheDriverController();
                if (driverController != null)
                {
                    driverController.ClearServerDriverIfClient(clientId);
                }
            }

            if (vehicleState.TryGetSeatOccupant(seatIndex, out var occupant) && occupant == clientId)
            {
                vehicleState.ReleaseSeat(seatIndex, clientId);
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (!isActiveHost || vehicleState == null)
            {
                return;
            }

            var seatIndex = vehicleState.FindSeatIndex(clientId);
            if (seatIndex == NetworkedVehicleState.NoSeatIndex)
            {
                return;
            }

            ReleaseVehicleSeat(seatIndex, clientId);
            Report("Siege libere apres deconnexion du client " + clientId + ".", true);
        }

        private void ApplySeatedState(NetworkedPlayerState playerState, int seatIndex)
        {
            playerState.WorldPosition.Value = vehicleState.transform.TransformPoint(NetworkedVehicleState.ResolveSeatLocalOffset(seatIndex));
            playerState.YawDegrees.Value = NormalizeYaw(vehicleState.transform.eulerAngles.y);
            playerState.SeatIndex.Value = seatIndex;
            playerState.Mode.Value = seatIndex == NetworkedVehicleState.DriverSeatIndex ? PlayerMode.Driver : PlayerMode.Passenger;
        }

        private void CacheReferences()
        {
            if (spawnService == null)
            {
                spawnService = FindAnyObjectByType<NetworkedPlayerSpawnService>();
            }

            if (vehicleState == null)
            {
                vehicleState = FindAnyObjectByType<NetworkedVehicleState>();
            }

            CacheDriverController();
        }

        private void CacheDriverController()
        {
            if (driverController == null && vehicleState != null)
            {
                driverController = vehicleState.GetComponent<NetworkedVehicleDriverController>();
            }
        }

        private void Report(string message, bool warning)
        {
            if (warning)
            {
                Debug.LogWarning("[Vehicles] " + message);
            }
            else
            {
                Debug.Log("[Vehicles] " + message);
            }

            if (checkpointHud != null)
            {
                checkpointHud.ShowVehicleSeatMessage(message);
            }
        }

        private static float NormalizeYaw(float yawDegrees)
        {
            var normalized = yawDegrees % 360f;
            return normalized < 0f ? normalized + 360f : normalized;
        }
    }
}
