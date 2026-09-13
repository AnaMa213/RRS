using System;
using System.Collections;
using System.Linq;
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
        private const string ClientTag = "Client";

        [SerializeField]
        private PassengerActionCatalog passengerActionCatalog;

        [SerializeField]
        private RageTuningDef passengerActionRageTuning;

        private const float PassengerActionOneRageDelta = 25f;

        private NetworkManager subscribedManager;
        private NetworkedRageState harnessTarget;
        private NetworkedRunState harnessRunState;
        private NetworkedVehicleState harnessVehicleState;
        private NetworkedPassengerActionIncidentState harnessIncidentState;

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
                Debug.Log("[RageSandboxAutoStart] NetworkManager already listening; harness check.");
                if (manager.IsServer)
                {
                    EnsurePassengerActionHarness(manager);
                }
                else
                {
                    StartCoroutine(BindLocalPassengerActionView(manager));
                }
                yield break;
            }

            var tags = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags;
            var hasClientTag = tags != null && tags.Any(tag => string.Equals(tag, ClientTag, StringComparison.OrdinalIgnoreCase));
            var isVirtualProject = Application.dataPath.Replace('\\', '/').Contains("/Library/VP/", StringComparison.OrdinalIgnoreCase);
            if (hasClientTag || isVirtualProject)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                var started = manager.StartClient();
                Debug.Log($"[RageSandboxAutoStart] StartClient() returned {started}.");
                if (started)
                {
                    StartCoroutine(BindLocalPassengerActionView(manager));
                }
                yield break;
            }

            var hostStarted = manager.StartHost();
            Debug.Log($"[RageSandboxAutoStart] StartHost() returned {hostStarted}.");
            if (hostStarted)
            {
                yield return null;
                EnsurePassengerActionHarness(manager);
            }
        }

        private void OnDestroy()
        {
            if (subscribedManager != null)
            {
                subscribedManager.OnClientConnectedCallback -= HandleClientConnected;
            }
        }

        private void EnsurePassengerActionHarness(NetworkManager manager)
        {
            harnessTarget = FindAnyObjectByType<NetworkedRageState>();
            if (passengerActionCatalog == null || harnessTarget == null)
            {
                Debug.LogWarning("[RageSandboxAutoStart] Catalogue PassengerActions ou cible Rage absent.");
                return;
            }

            if (!EnsureSpawned(harnessTarget.GetComponent<NetworkObject>(), manager))
            {
                return;
            }

            harnessRunState = FindAnyObjectByType<NetworkedRunState>();
            if (harnessRunState == null)
            {
                var runObject = new GameObject("PassengerActionRunState");
                var runNetworkObject = runObject.AddComponent<NetworkObject>();
                harnessRunState = runObject.AddComponent<NetworkedRunState>();
                runNetworkObject.Spawn();
            }
            else if (!EnsureSpawned(harnessRunState.GetComponent<NetworkObject>(), manager))
            {
                return;
            }

            harnessVehicleState = FindAnyObjectByType<NetworkedVehicleState>();
            if (harnessVehicleState == null)
            {
                var vehicleObject = new GameObject("PassengerActionVehicleSeatHarness");
                var vehicleNetworkObject = vehicleObject.AddComponent<NetworkObject>();
                harnessVehicleState = vehicleObject.AddComponent<NetworkedVehicleState>();
                vehicleNetworkObject.Spawn();
            }
            else if (!EnsureSpawned(harnessVehicleState.GetComponent<NetworkObject>(), manager))
            {
                return;
            }

            harnessIncidentState = FindAnyObjectByType<NetworkedPassengerActionIncidentState>();
            if (harnessIncidentState == null || !EnsureSpawned(harnessIncidentState.GetComponent<NetworkObject>(), manager))
            {
                return;
            }

            if (subscribedManager != manager)
            {
                if (subscribedManager != null)
                {
                    subscribedManager.OnClientConnectedCallback -= HandleClientConnected;
                }

                subscribedManager = manager;
                subscribedManager.OnClientConnectedCallback += HandleClientConnected;
            }

            EnsurePassengerActionActor(manager, manager.LocalClientId);
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (subscribedManager != null && subscribedManager.IsServer)
            {
                EnsurePassengerActionActor(subscribedManager, clientId);
            }
        }

        private void EnsurePassengerActionActor(NetworkManager manager, ulong clientId)
        {
            if (FindPassengerActor(clientId) != null)
            {
                return;
            }

            var prefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            if (prefab == null)
            {
                Debug.LogWarning("[RageSandboxAutoStart] NetworkedPlayerRoot absent.");
                return;
            }

            var actor = Instantiate(prefab, harnessTarget.transform.position, Quaternion.identity);
            actor.name = "PassengerActionActor_" + clientId;
            var actorNetworkObject = actor.GetComponent<NetworkObject>();
            var actorState = actor.GetComponent<NetworkedPlayerState>();
            var intent = actor.GetComponent<NetworkedPassengerActionIntent>();
            if (actorNetworkObject == null || actorState == null || intent == null)
            {
                Debug.LogWarning("[RageSandboxAutoStart] NetworkedPlayerRoot incomplet pour PassengerActions.");
                Destroy(actor);
                return;
            }

            if (!harnessVehicleState.TryFindAvailablePassengerSeat(out var seatIndex))
            {
                Debug.LogWarning("[RageSandboxAutoStart] Aucun siege passager disponible pour le client " + clientId + ".");
                Destroy(actor);
                return;
            }

            actorNetworkObject.Spawn();
            actorState.ClientId.Value = clientId;
            actorState.Lifecycle.Value = PlayerLifecycle.Alive;
            actorState.Mode.Value = PlayerMode.Passenger;
            actorState.SeatIndex.Value = seatIndex;
            actorState.WorldPosition.Value = actor.transform.position;
            harnessVehicleState.TryAssignSeat(seatIndex, clientId);

            var view = FindAnyObjectByType<PassengerActionDebugView>();
            if (view == null)
            {
                view = FindAnyObjectByType<Canvas>().gameObject.AddComponent<PassengerActionDebugView>();
            }

            intent.Configure(passengerActionCatalog, harnessRunState, harnessVehicleState, harnessTarget, null, view, null, null, harnessIncidentState);
            intent.ActionValidated += HandlePassengerActionValidated;
            view.Bind(passengerActionCatalog, intent.RequestSlot, true, harnessIncidentState);
            Debug.Log("[RageSandboxAutoStart] Acteur passager pret pour le client " + clientId + ".");
        }

        private IEnumerator BindLocalPassengerActionView(NetworkManager manager)
        {
            for (var frame = 0; frame < 120; frame++)
            {
                var intent = FindLocalPassengerIntent(manager.LocalClientId);
                if (intent != null)
                {
                    var view = FindAnyObjectByType<PassengerActionDebugView>();
                    var canvas = view == null ? FindAnyObjectByType<Canvas>() : null;
                    if (view == null && canvas != null)
                    {
                        view = canvas.gameObject.AddComponent<PassengerActionDebugView>();
                    }

                    var incidentState = FindAnyObjectByType<NetworkedPassengerActionIncidentState>();
                    intent.Configure(passengerActionCatalog, null, null, null, null, view, null, null, incidentState);
                    view?.Bind(passengerActionCatalog, intent.RequestSlot, true, incidentState);
                    Debug.Log("[RageSandboxAutoStart] Client passager pret : declenche les slots 1 et 2 une fois.");
                    yield break;
                }

                yield return null;
            }

            Debug.LogWarning("[RageSandboxAutoStart] Acteur passager local introuvable apres connexion client.");
        }

        private static NetworkedPassengerActionIntent FindLocalPassengerIntent(ulong localClientId)
        {
            var candidates = FindObjectsByType<NetworkedPassengerActionIntent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var candidate in candidates)
            {
                var state = candidate == null ? null : candidate.GetComponent<NetworkedPlayerState>();
                if (candidate != null && candidate.IsSpawned && state != null && state.ClientId.Value == localClientId)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static NetworkedPlayerState FindPassengerActor(ulong clientId)
        {
            var candidates = FindObjectsByType<NetworkedPlayerState>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var candidate in candidates)
            {
                if (candidate != null && candidate.IsSpawned && candidate.ClientId.Value == clientId)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool EnsureSpawned(NetworkObject networkObject, NetworkManager manager)
        {
            if (networkObject == null || manager == null || !manager.IsServer)
            {
                Debug.LogWarning("[RageSandboxAutoStart] Etat reseau non inscriptible par cet hote.");
                return false;
            }

            if (!networkObject.IsSpawned)
            {
                networkObject.Spawn();
            }

            return networkObject.IsSpawned;
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
