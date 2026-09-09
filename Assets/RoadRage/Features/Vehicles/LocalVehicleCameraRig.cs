using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 3.2 : rig camera Cinemachine strictement local pour la voiture partagee. N'est jamais
    /// un NetworkBehaviour -- bascule seulement l'activation locale de la CinemachineCamera enfant
    /// selon que NetworkedVehicleState.DriverClientId correspond au client local. Aucune donnee de
    /// camera n'est repliquee sur le reseau.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkedVehicleState))]
    public sealed class LocalVehicleCameraRig : MonoBehaviour
    {
        [SerializeField]
        private CinemachineCamera vehicleCamera;

        private NetworkedVehicleState state;

        private void Awake()
        {
            CacheState();
            SetCameraActive(false);
        }

        private void Update()
        {
            RefreshActivation();
        }

        private void RefreshActivation()
        {
            CacheState();

            if (vehicleCamera == null || state == null || !state.IsSpawned)
            {
                SetCameraActive(false);
                return;
            }

            var manager = NetworkManager.Singleton;
            var isLocalDriver = manager != null && manager.IsClient
                && state.DriverClientId.Value == manager.LocalClientId;

            SetCameraActive(isLocalDriver);
        }

        private void SetCameraActive(bool active)
        {
            if (vehicleCamera != null && vehicleCamera.gameObject.activeSelf != active)
            {
                vehicleCamera.gameObject.SetActive(active);
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
