using System;
using RoadRage.Features.PassengerActions;
using RoadRage.Features.Players;
using RoadRage.Features.Rage;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Definitions;
using RoadRage.Shared.Domain;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkedPassengerActionIntent : NetworkBehaviour
    {
        [SerializeField]
        private PassengerActionCatalog catalog;

        private readonly double[] cooldownEndsAt = new double[PassengerActionCatalog.SlotCount];
        private NetworkedPlayerState state;
        private NetworkedRunState runState;
        private NetworkedVehicleState vehicleState;
        private NetworkedRageState target;
        private bool targetSpecified;
        private NetworkedPassengerActionIncidentState incidentState;
        private PassengerActionDebugView debugView;
        private RunCheckpointHudScreen checkpointHud;
        private Func<bool> localPassengerContext;
        private Func<Vector3> localActorPosition;
        private ulong nextSequence;
        private ulong lastProcessedSequence;

        public event Action<PassengerActionDef, Transform, NetworkedRageState> ActionValidated;
        public event Action<PassengerActionVerdict> VerdictReceived;

        private void Awake()
        {
            state = GetComponent<NetworkedPlayerState>();
        }

        public void Configure(
            PassengerActionCatalog actionCatalog,
            NetworkedRunState currentRunState,
            NetworkedVehicleState currentVehicleState,
            NetworkedRageState currentTarget,
            RunCheckpointHudScreen hud,
            PassengerActionDebugView view,
            Func<bool> offlinePassenger = null,
            Func<Vector3> offlineActorPosition = null,
            NetworkedPassengerActionIncidentState currentIncidentState = null,
            bool currentTargetSpecified = false)
        {
            if (actionCatalog != null)
            {
                catalog = actionCatalog;
            }

            runState = currentRunState;
            vehicleState = currentVehicleState;
            target = currentTarget;
            targetSpecified = currentTargetSpecified || currentTarget != null;
            checkpointHud = hud;
            debugView = view;
            localPassengerContext = offlinePassenger;
            localActorPosition = offlineActorPosition;
            incidentState = currentIncidentState;

            if (debugView != null)
            {
                debugView.Bind(catalog, RequestSlot, false, incidentState);
            }
        }

        public void SetTarget(NetworkedRageState currentTarget, bool currentTargetSpecified = false)
        {
            target = currentTarget;
            targetSpecified = currentTargetSpecified || currentTarget != null;
        }

        public void RequestSlot(int slot)
        {
            var action = catalog == null ? null : catalog.GetAtSlot(slot);
            if (action == null)
            {
                Report(new PassengerActionVerdict(PassengerActionVerdictCode.InvalidAction, "Action refusee : slot inconnu."), false, 0UL);
                return;
            }

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening)
            {
                var localIntent = new PassengerActionIntent(
                    slot,
                    action.RawId,
                    catalog.Version,
                    action.Version,
                    ++nextSequence,
                    new NetworkObjectReference((NetworkObject)null),
                    targetSpecified);
                ApplyAuthoritative(localIntent, 0UL, target, false);
                return;
            }

            var targetReference = !targetSpecified || target == null || target.NetworkObject == null || !target.IsSpawned
                ? new NetworkObjectReference((NetworkObject)null)
                : new NetworkObjectReference(target.NetworkObject);
            var intent = new PassengerActionIntent(slot, action.RawId, catalog.Version, action.Version, ++nextSequence, targetReference, targetSpecified);
            if (state == null || !manager.IsClient || state.ClientId.Value != manager.LocalClientId)
            {
                Report(new PassengerActionVerdict(PassengerActionVerdictCode.ActorMismatch, "Action refusee : acteur local invalide."), false, 0UL);
                return;
            }

            if (IsServer)
            {
                ApplyAuthoritative(intent, manager.LocalClientId, targetSpecified && target != null && target.IsSpawned ? target : null, true);
            }
            else
            {
                SubmitIntentRpc(intent);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SubmitIntentRpc(PassengerActionIntent intent, RpcParams rpcParams = default)
        {
            NetworkedRageState resolvedLockTarget = null;
            if (intent.HasTarget && intent.Target.TryGet(out var targetObject) && targetObject != null && targetObject.IsSpawned)
            {
                resolvedLockTarget = targetObject.GetComponent<NetworkedRageState>();
            }

            ApplyAuthoritative(intent, rpcParams.Receive.SenderClientId, resolvedLockTarget, true);
        }

        /// <summary>
        /// Story 5.5 : resolvedLockTarget/intent.HasTarget portent desormais le lock client-local du
        /// joueur (per-player), pas une cible globale partagee. HasTarget=false (pas de lock) :
        /// resolution hote de la plus proche IA eligible dans la portee de l'action, sans creer de
        /// lock persistant. HasTarget=true : le lock doit lui-meme resoudre a une IA eligible
        /// (NetworkedAIVehicleState + source rage/peur, spawnee) en portee, sinon refus -- jamais de
        /// repli sur une autre cible. Meme point de resolution partage que le klaxon conducteur
        /// (AiRageTargetResolution.ResolveTarget).
        /// </summary>
        private void ApplyAuthoritative(PassengerActionIntent intent, ulong senderClientId, NetworkedRageState resolvedLockTarget, bool networked)
        {
            var manager = NetworkManager.Singleton;
            if (networked && (!IsServer || manager == null))
            {
                return;
            }

            if (runState == null)
            {
                runState = FindAnyObjectByType<NetworkedRunState>();
            }

            var action = catalog != null && catalog.TryGetById(new DefinitionId(intent.ActionId.ToString()), out var found)
                ? found
                : null;
            var now = networked ? manager.ServerTime.Time : Time.timeAsDouble;
            var actorPosition = state == null
                ? (localActorPosition == null ? transform.position : localActorPosition())
                : state.WorldPosition.Value;
            var seatIndex = state == null ? NetworkedVehicleState.FirstPassengerSeatIndex : state.SeatIndex.Value;
            var actorId = state == null ? senderClientId : state.ClientId.Value;
            var seatMatches = !networked
                ? localPassengerContext != null && localPassengerContext()
                : TryFindActorVehicle(seatIndex, actorId, out vehicleState);
            var catalogValid = catalog != null && catalog.TryValidate(out _);

            var lockedCandidate = resolvedLockTarget == null ? null : resolvedLockTarget.GetComponent<NetworkedAIVehicleState>();
            var resolvedCandidate = action == null
                ? null
                : AiRageTargetResolution.ResolveTarget(
                    actorPosition,
                    intent.HasTarget,
                    lockedCandidate,
                    AiRageTargetResolution.FindEligibleCandidates(),
                    action.MaxRange,
                    networked);
            var resolvedTarget = resolvedCandidate == null ? null : resolvedCandidate.GetComponent<NetworkedRageState>();

            var targetValid = resolvedTarget != null && (!networked || (resolvedTarget.IsSpawned && resolvedTarget.NetworkObject != null));
            var context = new PassengerActionValidationContext(
                !networked || manager.ConnectedClients.ContainsKey(senderClientId),
                !networked || (state != null && actorId == senderClientId),
                state == null ? PlayerLifecycle.Alive : state.Lifecycle.Value,
                state == null && localPassengerContext != null && localPassengerContext() ? PlayerMode.Passenger : state == null ? PlayerMode.OnFoot : state.Mode.Value,
                NetworkedVehicleState.IsPassengerSeatIndex(seatIndex),
                seatMatches,
                runState == null ? RunPhase.NotStarted : runState.Phase.Value,
                catalogValid,
                targetValid,
                targetValid ? Vector3.Distance(actorPosition, resolvedTarget.transform.position) : float.PositiveInfinity,
                lastProcessedSequence,
                intent.Slot < cooldownEndsAt.Length ? cooldownEndsAt[intent.Slot] : double.PositiveInfinity,
                now);

            var verdict = PassengerActionValidation.Validate(catalog, action, intent, context);
            if (intent.Sequence > lastProcessedSequence)
            {
                lastProcessedSequence = intent.Sequence;
            }

            if (verdict.Accepted)
            {
                cooldownEndsAt[action.Slot] = now + action.CooldownSeconds;
                if (action.Slot == 1)
                {
                    ResolveIncidentState()?.ActivateOrRefresh();
                }

                ActionValidated?.Invoke(action, state == null ? transform : state.transform, resolvedTarget);
            }

            Report(verdict, networked, senderClientId);
        }

        private void Report(PassengerActionVerdict verdict, bool networked, ulong targetClientId = 0UL)
        {
            var manager = NetworkManager.Singleton;
            var isLocalActor = !networked || manager == null || targetClientId == manager.LocalClientId;
            if (isLocalActor)
            {
                ShowVerdict(verdict);
            }

            if (networked && IsServer && IsSpawned && !isLocalActor)
            {
                NotifyVerdictRpc(verdict.Code, new FixedString128Bytes(verdict.Message), RpcTarget.Single(targetClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void NotifyVerdictRpc(PassengerActionVerdictCode code, FixedString128Bytes message, RpcParams rpcParams = default)
        {
            ShowVerdict(new PassengerActionVerdict(code, message.ToString()));
        }

        private void ShowVerdict(PassengerActionVerdict verdict)
        {
            if (checkpointHud == null)
            {
                checkpointHud = FindAnyObjectByType<RunCheckpointHudScreen>();
            }

            if (debugView == null)
            {
                debugView = FindAnyObjectByType<PassengerActionDebugView>();
            }

            VerdictReceived?.Invoke(verdict);
            debugView?.ShowVerdict(verdict);
            checkpointHud?.ShowPassengerActionVerdict(verdict.Message);
        }

        private NetworkedPassengerActionIncidentState ResolveIncidentState()
        {
            if (incidentState == null)
            {
                incidentState = FindAnyObjectByType<NetworkedPassengerActionIncidentState>();
            }

            return incidentState;
        }

        private static bool TryFindActorVehicle(int seatIndex, ulong actorId, out NetworkedVehicleState resolvedVehicle)
        {
            resolvedVehicle = null;
            var vehicles = FindObjectsByType<NetworkedVehicleState>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var candidate in vehicles)
            {
                if (candidate != null && candidate.TryGetSeatOccupant(seatIndex, out var occupant) && occupant == actorId)
                {
                    resolvedVehicle = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
