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
        [SerializeField]
        [Tooltip("Angle maximal (degres) entre l'axe de conduite et le regard quand une cible est verrouillee. Knob de feel : monter pour suivre la cible plus loin sur les cotes, baisser pour privilegier la lisibilite de la route.")]
        [Range(0f, 90f)]
        private float rageTargetMaxLookAngle = 40f;

        private Transform rageTargetLookOverride;
        private Transform rageTargetLookProxy;
        private float rageTargetLookSide = 1f;

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

        private void LateUpdate()
        {
            RefreshRageTargetLookProxy();
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
        /// ET conducteur. Passer null retablit le LookAt d'origine du siege.
        ///
        /// Story 5.5 (correctif feel, 2026-09-15) : le lock ne vise plus la cible en dur. Viser la
        /// cible sans limite faisait pivoter la vue hors de la route des que la cible passait sur le
        /// cote ou derriere -- conduite illisible. La camera vise desormais un point relais
        /// (<see cref="rageTargetLookProxy"/>) place par <see cref="ResolveRageTargetLookPoint"/> :
        /// suivi exact tant que la cible reste dans le cone de conduite, puis maintien au bord du cone
        /// du cote de la cible (soft lock facon Mad Max / World of Tanks plutot que ball-cam Rocket
        /// League). L'identification de la cible hors cone est portee par le libelle monde
        /// (<see cref="AIVehicleBehaviorDebugView"/>) et le HUD rage, pas par la rotation de la vue.
        /// </summary>
        public void SetRageTargetLookOverride(Transform target)
        {
            rageTargetLookOverride = target;
            rageTargetLookSide = 1f;
            RefreshRageTargetLookProxy();
            ApplyLookAt(activeCamera);
        }

        public bool HasRageTargetLookOverride
        {
            get { return rageTargetLookOverride != null; }
        }

        /// <summary>
        /// Angle au-dela duquel le cote du cone est fige : sans cette hysteresis, une cible pile
        /// derriere le joueur fait osciller le lacet entre +180 et -180 degres, donc la vue claque de
        /// gauche a droite a chaque embardee de la cible.
        /// </summary>
        private const float RearSideHoldAngle = 150f;

        /// <summary>
        /// Point vise par la camera en lock. Dans le cone (+/- maxAngleDegrees autour de l'axe de
        /// conduite) : la cible exacte. Hors cone : un point a la meme distance, ramene au bord du
        /// cone du cote de la cible -- la vue penche vers la cible sans jamais quitter la route.
        /// Fonction pure (testee en EditMode) : anchorRotation doit deja etre aplatie en lacet par
        /// l'appelant pour qu'un vehicule sur le toit ne fasse pas rouler le cone.
        /// </summary>
        public static Vector3 ResolveRageTargetLookPoint(
            Vector3 anchorPosition,
            Quaternion anchorRotation,
            Vector3 targetPosition,
            float maxAngleDegrees,
            ref float previousSide)
        {
            var local = Quaternion.Inverse(anchorRotation) * (targetPosition - anchorPosition);
            var planarDistance = new Vector2(local.x, local.z).magnitude;
            if (planarDistance < 0.01f)
            {
                return targetPosition;
            }

            var yaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
            if (Mathf.Abs(yaw) <= maxAngleDegrees)
            {
                previousSide = yaw >= 0f ? 1f : -1f;
                return targetPosition;
            }

            var side = Mathf.Abs(yaw) > RearSideHoldAngle ? previousSide : Mathf.Sign(yaw);
            previousSide = side;
            var clampedYaw = side * maxAngleDegrees * Mathf.Deg2Rad;
            var clamped = new Vector3(
                Mathf.Sin(clampedYaw) * planarDistance,
                local.y,
                Mathf.Cos(clampedYaw) * planarDistance);
            return anchorPosition + (anchorRotation * clamped);
        }

        /// <summary>Repositionne le relais de visee : la cible bougeant seule, la camera continue de la suivre sans action du joueur.</summary>
        private void RefreshRageTargetLookProxy()
        {
            if (rageTargetLookOverride == null)
            {
                return;
            }

            if (rageTargetLookProxy == null)
            {
                rageTargetLookProxy = new GameObject("RageTargetLookProxy").transform;
                rageTargetLookProxy.SetParent(transform, false);
            }

            rageTargetLookProxy.position = ResolveRageTargetLookPoint(
                transform.position,
                ResolveDrivingYawRotation(),
                rageTargetLookOverride.position,
                rageTargetMaxLookAngle,
                ref rageTargetLookSide);
        }

        /// <summary>Axe de conduite aplati : un vehicule incline ou retourne ne doit pas faire rouler le cone de visee.</summary>
        private Quaternion ResolveDrivingYawRotation()
        {
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            return forward.sqrMagnitude < 0.0001f ? transform.rotation : Quaternion.LookRotation(forward, Vector3.up);
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

            camera.LookAt = rageTargetLookOverride != null && rageTargetLookProxy != null
                ? rageTargetLookProxy
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
