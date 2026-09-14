using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 3.2 : rig camera Cinemachine strictement local pour la voiture partagee. N'est jamais
    /// un NetworkBehaviour -- bascule seulement l'activation locale de la CinemachineCamera enfant
    /// selon le siege occupe par le client local. Aucune donnee de camera n'est repliquee sur le
    /// reseau. Extension (2026-09-12, hors story, demande de test manuel) : un slot camera par siege
    /// passager en plus du siege conducteur -- si un slot passager n'est pas cable dans le prefab, on
    /// retombe sur la camera conducteur plutot que de laisser le joueur sans aucune vue. Depuis la
    /// Story 5.3, le solo est toujours host-authoritative comme l'en ligne : l'activation reseau
    /// (RefreshActivation via NetworkedVehicleState.FindSeatIndex) couvre desormais tous les cas de
    /// jeu reels ; SetManualCameraActive ne reste qu'un hook de test EditMode pur (ThirdPersonCameraTests)
    /// pour forcer une camera sans NetworkManager.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkedVehicleState))]
    public sealed class LocalVehicleCameraRig : MonoBehaviour
    {
        [SerializeField]
        private CinemachineCamera vehicleCamera;

        [SerializeField]
        private CinemachineCamera[] passengerSeatCameras = new CinemachineCamera[NetworkedVehicleState.PassengerSeatCount];

        private NetworkedVehicleState state;
        private bool manualCameraActive;
        private int manualCameraSeatIndex = NetworkedVehicleState.DriverSeatIndex;
        private bool suppressNetworkCameraUntilReleased;
        private CinemachineCamera activeCamera;
        private readonly Dictionary<CinemachineCamera, Transform> defaultLookAtBySeatCamera = new Dictionary<CinemachineCamera, Transform>();
        private Transform rageTargetLookOverride;

        private void Awake()
        {
            CacheState();
            CacheDefaultLookAt(vehicleCamera);
            InitializeCamera(vehicleCamera);
            foreach (var camera in passengerSeatCameras)
            {
                CacheDefaultLookAt(camera);
                InitializeCamera(camera);
            }

            ActivateCamera(null);
        }

        private void InitializeCamera(CinemachineCamera camera)
        {
            if (camera == null) return;
            camera.gameObject.SetActive(false);
            var input = camera.GetComponent<CinemachineInputAxisController>();
            if (input != null)
                input.ReadControlValueOverride = (action, hint, context, read) =>
                {
                    var value = read(action, hint, context, null);
                    // CancelDeltaTime is appropriate for mouse delta; sticks express a rate.
                    return action.activeControl != null && action.activeControl.device is Gamepad
                        ? value * 750f * Time.deltaTime : value;
                };
        }

        private void OnDisable()
        {
            ActivateCamera(null);
        }

        private void Update()
        {
            RefreshActivation();
        }

        private void RefreshActivation()
        {
            CacheState();

            if (state == null)
            {
                ActivateCamera(null);
                return;
            }

            if (manualCameraActive)
            {
                ActivateCamera(ResolveCameraForSeat(manualCameraSeatIndex));
                return;
            }

            if (!state.IsSpawned)
            {
                ActivateCamera(null);
                return;
            }

            var manager = NetworkManager.Singleton;
            var localSeatIndex = manager != null && manager.IsClient
                ? state.FindSeatIndex(manager.LocalClientId)
                : NetworkedVehicleState.NoSeatIndex;

            if (suppressNetworkCameraUntilReleased)
            {
                if (localSeatIndex == NetworkedVehicleState.NoSeatIndex)
                {
                    suppressNetworkCameraUntilReleased = false;
                }

                ActivateCamera(null);
                return;
            }

            ActivateCamera(localSeatIndex == NetworkedVehicleState.NoSeatIndex ? null : ResolveCameraForSeat(localSeatIndex));
        }

        /// <summary>
        /// Hook de test EditMode pur (ThirdPersonCameraTests) : force une camera de siege sans passer
        /// par un NetworkManager. Aucun appelant gameplay depuis la Story 5.3 (le solo est toujours
        /// host-authoritative, l'activation reseau normale de RefreshActivation suffit).
        /// </summary>
        public void SetManualCameraActive(bool active)
        {
            SetManualCameraActive(active, NetworkedVehicleState.DriverSeatIndex);
        }

        public void SetManualCameraActive(bool active, int seatIndex)
        {
            manualCameraActive = active;
            manualCameraSeatIndex = seatIndex;
            ActivateCamera(active ? ResolveCameraForSeat(seatIndex) : null);
        }

        public void SuppressNetworkCameraUntilReleased()
        {
            suppressNetworkCameraUntilReleased = true;
            ActivateCamera(null);
        }

        /// <summary>
        /// Ajout hors story (2026-09-12) : verrouillage camera sur une rage target, demande passager
        /// ET conducteur. Passer null retablit le LookAt d'origine du siege. Reapplique en continu via
        /// ActivateCamera/RefreshActivation, donc si la cible bouge encore, la camera continue de la
        /// suivre sans action supplementaire du joueur.
        /// </summary>
        public void SetRageTargetLookOverride(Transform target)
        {
            rageTargetLookOverride = target;
            ApplyLookAt(activeCamera);
        }

        public bool HasRageTargetLookOverride
        {
            get { return rageTargetLookOverride != null; }
        }

        private CinemachineCamera ResolveCameraForSeat(int seatIndex)
        {
            if (seatIndex == NetworkedVehicleState.DriverSeatIndex)
            {
                return vehicleCamera;
            }

            var passengerIndex = seatIndex - NetworkedVehicleState.FirstPassengerSeatIndex;
            if (passengerIndex >= 0 && passengerIndex < passengerSeatCameras.Length && passengerSeatCameras[passengerIndex] != null)
            {
                return passengerSeatCameras[passengerIndex];
            }

            return vehicleCamera;
        }

        private void ActivateCamera(CinemachineCamera camera)
        {
            if (activeCamera == camera)
            {
                return;
            }

            var previousOrbit = activeCamera == null ? null : activeCamera.GetComponent<CinemachineOrbitalFollow>();
            var yaw = previousOrbit == null ? transform.eulerAngles.y : previousOrbit.HorizontalAxis.Value;
            var pitch = previousOrbit == null ? 15f : previousOrbit.VerticalAxis.Value;
            if (activeCamera != null)
            {
                activeCamera.gameObject.SetActive(false);
            }

            activeCamera = camera;
            CacheDefaultLookAt(activeCamera);

            if (activeCamera != null)
            {
                activeCamera.gameObject.SetActive(true);
                var orbit = activeCamera.GetComponent<CinemachineOrbitalFollow>();
                if (orbit != null)
                {
                    orbit.HorizontalAxis.Value = orbit.HorizontalAxis.Center = yaw;
                    orbit.VerticalAxis.Value = pitch;
                }
            }

            ApplyLookAt(activeCamera);
        }

        private void ApplyLookAt(CinemachineCamera camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.LookAt = rageTargetLookOverride != null
                ? rageTargetLookOverride
                : defaultLookAtBySeatCamera.TryGetValue(camera, out var original) ? original : null;
        }

        private void CacheDefaultLookAt(CinemachineCamera camera)
        {
            if (camera != null && !defaultLookAtBySeatCamera.ContainsKey(camera))
            {
                defaultLookAtBySeatCamera.Add(camera, camera.LookAt);
            }
        }

        private void CacheState()
        {
            if (state == null)
            {
                state = GetComponent<NetworkedVehicleState>();
            }
        }
    }
}
