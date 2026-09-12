using System;
using System.Collections;
using RoadRage.App.Run;
using RoadRage.Features.PassengerActions;
using RoadRage.Features.Players;
using RoadRage.Features.Rage;
using RoadRage.Features.Run;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.DevTools
{
    /// <summary>
    /// Story 4.1 : auto-start editeur simplifie pour Dev_RageSandbox, sur le modele de
    /// RoadRageNetcodeSmokeTestAutoStart (Dev_VehicleSandbox) mais sans harness de siege --
    /// demarre l'hote si aucun NetworkManager n'ecoute deja, pour que la scene soit jouable
    /// directement en Play Mode. Editor-only behavior.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RageSandboxAutoStart : MonoBehaviour
    {
        private const string RageSandboxSceneName = "Dev_RageSandbox";

        [SerializeField]
        private PassengerActionCatalog passengerActionCatalog;

        [SerializeField]
        private RageTuningDef passengerActionRageTuning;

        private const float PassengerActionOneRageDelta = 25f;

#if UNITY_EDITOR
        private IEnumerator Start()
        {
            yield return null;

            if (!string.Equals(SceneManager.GetActiveScene().name, RageSandboxSceneName, StringComparison.Ordinal))
            {
                yield break;
            }

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[RageSandboxAutoStart] No NetworkManager.Singleton found.");
                yield break;
            }

            var manager = NetworkManager.Singleton;
            if (manager.IsListening)
            {
                Debug.Log("[RageSandboxAutoStart] NetworkManager already listening; auto-start skipped.");
                EnsurePassengerActionHarness(manager);
                yield break;
            }

            var started = manager.StartHost();
            Debug.Log($"[RageSandboxAutoStart] StartHost() returned {started}.");
            if (started)
            {
                yield return null;
                EnsurePassengerActionHarness(manager);
            }
        }

        private void EnsurePassengerActionHarness(NetworkManager manager)
        {
            var target = FindAnyObjectByType<NetworkedRageState>();
            if (passengerActionCatalog == null || target == null)
            {
                Debug.LogWarning("[RageSandboxAutoStart] Catalogue PassengerActions ou cible Rage absent.");
                return;
            }

            var runState = FindAnyObjectByType<NetworkedRunState>();
            if (runState == null)
            {
                var runObject = new GameObject("PassengerActionRunState");
                var runNetworkObject = runObject.AddComponent<NetworkObject>();
                runState = runObject.AddComponent<NetworkedRunState>();
                runNetworkObject.Spawn();
            }

            var vehicleState = FindAnyObjectByType<NetworkedVehicleState>();
            if (vehicleState == null)
            {
                var vehicleObject = new GameObject("PassengerActionVehicleSeatHarness");
                var vehicleNetworkObject = vehicleObject.AddComponent<NetworkObject>();
                vehicleState = vehicleObject.AddComponent<NetworkedVehicleState>();
                vehicleNetworkObject.Spawn();
            }

            var prefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            if (prefab == null)
            {
                Debug.LogWarning("[RageSandboxAutoStart] NetworkedPlayerRoot absent.");
                return;
            }

            var actor = Instantiate(prefab, target.transform.position, Quaternion.identity);
            actor.name = "PassengerActionActor";
            var actorNetworkObject = actor.GetComponent<NetworkObject>();
            var actorState = actor.GetComponent<NetworkedPlayerState>();
            var intent = actor.GetComponent<NetworkedPassengerActionIntent>();
            if (actorNetworkObject == null || actorState == null || intent == null)
            {
                Debug.LogWarning("[RageSandboxAutoStart] NetworkedPlayerRoot incomplet pour PassengerActions.");
                Destroy(actor);
                return;
            }

            actorNetworkObject.Spawn();
            actorState.ClientId.Value = manager.LocalClientId;
            actorState.Lifecycle.Value = PlayerLifecycle.Alive;
            actorState.Mode.Value = PlayerMode.Passenger;
            actorState.SeatIndex.Value = NetworkedVehicleState.FirstPassengerSeatIndex;
            actorState.WorldPosition.Value = actor.transform.position;
            vehicleState.TryAssignSeat(NetworkedVehicleState.FirstPassengerSeatIndex, manager.LocalClientId);

            var view = FindAnyObjectByType<PassengerActionDebugView>();
            if (view == null)
            {
                view = FindAnyObjectByType<Canvas>().gameObject.AddComponent<PassengerActionDebugView>();
            }

            var incidentState = FindAnyObjectByType<NetworkedPassengerActionIncidentState>();
            intent.Configure(passengerActionCatalog, runState, vehicleState, target, null, view, null, null, incidentState);
            intent.ActionValidated += HandlePassengerActionValidated;
            view.Bind(passengerActionCatalog, intent.RequestSlot, true, incidentState);
        }

        private void HandlePassengerActionValidated(PassengerActionDef action, Transform actor, NetworkedRageState target)
        {
            if (action != null && action.Slot == 0 && target != null)
            {
                target.ApplyRageDelta(PassengerActionOneRageDelta, passengerActionRageTuning);
            }
        }
#else
        private void Start()
        {
            enabled = false;
        }
#endif
    }
}
