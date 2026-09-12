using RoadRage.Features.Rage;
using RoadRage.Features.Vehicles;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Story 4.3 : instanciation dev/MVP d'un vehicule (normal ou rage-target) hors RunFlowController.cs,
    /// qui doit rester exempt d'appel ".Spawn(" (garde Epic 1, Story16Epic1PlayableCheckpointTests) --
    /// RoadRage.Features.Vehicles ne peut pas referencer RoadRage.Features.Rage (garde Story 3.1+),
    /// donc cette composition reste dans l'assembly App aux cotes de RunFlowController.
    /// </summary>
    internal static class DevVehicleSpawner
    {
        public static GameObject Create(GameObject prefab, string objectName, Vector3 position, Quaternion rotation, bool rageTarget)
        {
            var vehicle = prefab == null
                ? GameObject.CreatePrimitive(PrimitiveType.Cube)
                : Object.Instantiate(prefab);
            vehicle.name = objectName;
            vehicle.transform.SetPositionAndRotation(position, rotation);

            if (prefab == null)
            {
                vehicle.transform.localScale = new Vector3(2.06f, 1.42f, 4.44f);
            }

            var body = vehicle.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = vehicle.AddComponent<Rigidbody>();
                body.mass = 1200f;
                body.linearDamping = 0.3f;
                body.angularDamping = 3f;
            }

            var networkObject = vehicle.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                networkObject = vehicle.AddComponent<NetworkObject>();
            }

            if (vehicle.GetComponent<NetworkedVehicleState>() == null)
            {
                vehicle.AddComponent<NetworkedVehicleState>();
            }

            if (vehicle.GetComponent<NetworkedVehicleDriverController>() == null)
            {
                vehicle.AddComponent<NetworkedVehicleDriverController>();
            }

            if (rageTarget && vehicle.GetComponent<NetworkedRageState>() == null)
            {
                vehicle.AddComponent<NetworkedRageState>();
            }

            var manager = NetworkManager.Singleton;
            if (manager != null && manager.IsListening && manager.IsServer && !networkObject.IsSpawned)
            {
                networkObject.Spawn();
            }

            return vehicle;
        }
    }
}
