using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 3.2 : revendication de conducteur host-arbitree et controle de conduite arcade pour
    /// la voiture partagee. Le Rigidbody n'est simule que sur le host (isKinematic = !IsServer,
    /// pose dans OnNetworkSpawn) ; sur les clients la position/rotation arrive via NetworkTransform
    /// (autorite serveur, comportement par defaut NGO -- pas de sync maison ici). Le claim/release
    /// et l'intention de conduite du client distant sont soumis au host via
    /// Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone), valides contre
    /// rpcParams.Receive.SenderClientId -- meme patron que
    /// NetworkedPlayerPresentation.SubmitLocalPose/SubmitPoseRpc (Story 2.5) et
    /// NetworkedPlayerLifecycleIntent.RequestRespawn/RequestRespawnRpc (Story 2.7), necessaire ici
    /// aussi car le NetworkObject de la voiture reste host-owned.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedVehicleState))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NetworkedVehicleDriverController : NetworkBehaviour
    {
        [SerializeField]
        private Key claimReleaseKey = Key.F;

        [SerializeField]
        [Min(0f)]
        private float maxForwardSpeed = 18f;

        [SerializeField]
        [Min(0f)]
        private float maxReverseSpeed = 7f;

        [SerializeField]
        [Min(0f)]
        private float acceleration = 28f;

        [SerializeField]
        [Min(0f)]
        private float reverseAcceleration = 18f;

        [SerializeField]
        [Min(0f)]
        private float brakeDeceleration = 42f;

        [SerializeField]
        [Min(0f)]
        private float coastDeceleration = 8f;

        [SerializeField]
        [Min(0f)]
        private float lateralGrip = 36f;

        [SerializeField]
        [Min(0f)]
        private float steerDegreesPerSecond = 125f;

        [SerializeField]
        [Min(0f)]
        private float minimumSteerSpeed = 0.25f;

        [SerializeField]
        [Min(0f)]
        private float rollStabilityAssist = 8f;

        [SerializeField]
        private Vector3 centerOfMassOffset = new Vector3(0f, -0.35f, 0f);

        private NetworkedVehicleState state;
        private Rigidbody body;
        private VehicleDriveIntent latestIntent = VehicleDriveIntent.Idle;

        private const float InputEpsilon = 0.0001f;
        private const float DirectionEpsilon = 0.05f;

        private void Awake()
        {
            CacheComponents();
        }

        public override void OnNetworkSpawn()
        {
            CacheComponents();

            if (body != null)
            {
                ConfigureArcadeBody();
                body.isKinematic = !IsServer;
            }
        }

        private void Update()
        {
            if (!IsSpawned || state == null)
            {
                return;
            }

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || !manager.IsClient)
            {
                return;
            }

            var localClientId = manager.LocalClientId;

            HandleClaimReleaseInput(localClientId);

            if (state.DriverClientId.Value != localClientId)
            {
                return;
            }

            SubmitDriveIntent(ReadLocalDriveIntent(), localClientId);
        }

        private void FixedUpdate()
        {
            if (!IsServer || body == null || state == null)
            {
                return;
            }

            if (state.DriverClientId.Value == NetworkedVehicleState.UnclaimedDriverClientId)
            {
                return;
            }

            ApplyPhysics(latestIntent);
        }

        private void HandleClaimReleaseInput(ulong localClientId)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[claimReleaseKey].wasPressedThisFrame)
            {
                return;
            }

            if (state.DriverClientId.Value == localClientId)
            {
                RequestRelease(localClientId);
            }
            else
            {
                RequestClaim(localClientId);
            }
        }

        private void RequestClaim(ulong localClientId)
        {
            if (IsServer)
            {
                ApplyServerClaim(localClientId);
                return;
            }

            RequestClaimDriverRpc();
        }

        private void RequestRelease(ulong localClientId)
        {
            if (IsServer)
            {
                ApplyServerRelease(localClientId);
                return;
            }

            RequestReleaseDriverRpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestClaimDriverRpc(RpcParams rpcParams = default)
        {
            ApplyServerClaim(rpcParams.Receive.SenderClientId);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestReleaseDriverRpc(RpcParams rpcParams = default)
        {
            ApplyServerRelease(rpcParams.Receive.SenderClientId);
        }

        private void ApplyServerClaim(ulong clientId)
        {
            if (!IsServer || state == null)
            {
                return;
            }

            var current = state.DriverClientId.Value;
            if (current == clientId)
            {
                // Reclaim idempotent : deja conducteur, aucun changement d'etat.
                return;
            }

            if (current != NetworkedVehicleState.UnclaimedDriverClientId)
            {
                Debug.LogWarning("[Vehicles] Revendication de conducteur refusee pour le client "
                    + clientId + " : siege deja tenu par le client " + current + ".");
                return;
            }

            state.DriverClientId.Value = clientId;
        }

        private void ApplyServerRelease(ulong clientId)
        {
            if (!IsServer || state == null || state.DriverClientId.Value != clientId)
            {
                return;
            }

            state.DriverClientId.Value = NetworkedVehicleState.UnclaimedDriverClientId;
            latestIntent = VehicleDriveIntent.Idle;
        }

        private void SubmitDriveIntent(VehicleDriveIntent intent, ulong localClientId)
        {
            if (IsServer)
            {
                ApplyServerDriveIntent(intent.Throttle, intent.Steer, intent.BrakeReverse, localClientId);
                return;
            }

            SubmitDriveIntentRpc(intent.Throttle, intent.Steer, intent.BrakeReverse);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SubmitDriveIntentRpc(float throttle, float steer, float brakeReverse, RpcParams rpcParams = default)
        {
            ApplyServerDriveIntent(throttle, steer, brakeReverse, rpcParams.Receive.SenderClientId);
        }

        private void ApplyServerDriveIntent(float throttle, float steer, float brakeReverse, ulong senderClientId)
        {
            if (!IsServer || state == null || state.DriverClientId.Value != senderClientId)
            {
                return;
            }

            latestIntent = new VehicleDriveIntent(throttle, steer, brakeReverse);
        }

        private void ApplyPhysics(VehicleDriveIntent intent)
        {
            var fixedDeltaTime = Time.fixedDeltaTime;
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude <= DirectionEpsilon * DirectionEpsilon)
            {
                forward = transform.forward;
            }

            forward.Normalize();
            var right = Vector3.Cross(Vector3.up, forward).normalized;

            var currentVelocity = body.linearVelocity;
            var planarVelocity = Vector3.ProjectOnPlane(currentVelocity, Vector3.up);
            var verticalVelocity = currentVelocity - planarVelocity;
            var longitudinalSpeed = Vector3.Dot(planarVelocity, forward);
            var lateralSpeed = Vector3.Dot(planarVelocity, right);

            var targetSpeed = ResolveTargetSpeed(intent, longitudinalSpeed);
            var speedChangeRate = ResolveSpeedChangeRate(intent, longitudinalSpeed);
            var nextLongitudinalSpeed = Mathf.MoveTowards(
                longitudinalSpeed,
                targetSpeed,
                speedChangeRate * fixedDeltaTime);

            var nextLateralSpeed = Mathf.MoveTowards(
                lateralSpeed,
                0f,
                lateralGrip * fixedDeltaTime);

            body.linearVelocity = (forward * nextLongitudinalSpeed) + (right * nextLateralSpeed) + verticalVelocity;

            ApplySteering(intent, nextLongitudinalSpeed, fixedDeltaTime);
            ApplyStabilityAssist(fixedDeltaTime);
        }

        public static float ResolveSteerDirectionMultiplier(float longitudinalSpeed, float brakeReverseInput)
        {
            if (longitudinalSpeed < -DirectionEpsilon)
            {
                return -1f;
            }

            if (Mathf.Abs(longitudinalSpeed) <= DirectionEpsilon && brakeReverseInput > InputEpsilon)
            {
                return -1f;
            }

            return 1f;
        }

        private float ResolveTargetSpeed(VehicleDriveIntent intent, float longitudinalSpeed)
        {
            if (intent.BrakeReverse > InputEpsilon)
            {
                return longitudinalSpeed > minimumSteerSpeed ? 0f : -maxReverseSpeed * intent.BrakeReverse;
            }

            if (intent.Throttle > InputEpsilon)
            {
                return maxForwardSpeed * intent.Throttle;
            }

            return 0f;
        }

        private float ResolveSpeedChangeRate(VehicleDriveIntent intent, float longitudinalSpeed)
        {
            if (intent.BrakeReverse > InputEpsilon)
            {
                return longitudinalSpeed > minimumSteerSpeed ? brakeDeceleration : reverseAcceleration;
            }

            if (intent.Throttle > InputEpsilon)
            {
                return acceleration;
            }

            return coastDeceleration;
        }

        private void ApplySteering(VehicleDriveIntent intent, float longitudinalSpeed, float fixedDeltaTime)
        {
            if (Mathf.Abs(intent.Steer) <= InputEpsilon)
            {
                return;
            }

            var speedMagnitude = Mathf.Abs(longitudinalSpeed);
            if (speedMagnitude < minimumSteerSpeed)
            {
                return;
            }

            var speedFactor = Mathf.Clamp01(speedMagnitude / Mathf.Max(maxForwardSpeed, 1f));
            var lowSpeedAssist = Mathf.Lerp(0.45f, 1f, speedFactor);
            var reverseAwareDirection = ResolveSteerDirectionMultiplier(longitudinalSpeed, intent.BrakeReverse);
            var yawDegrees = intent.Steer * reverseAwareDirection * steerDegreesPerSecond * lowSpeedAssist * fixedDeltaTime;

            body.MoveRotation(Quaternion.AngleAxis(yawDegrees, Vector3.up) * body.rotation);
        }

        private void ApplyStabilityAssist(float fixedDeltaTime)
        {
            if (rollStabilityAssist <= 0f)
            {
                return;
            }

            var angularVelocity = body.angularVelocity;
            angularVelocity.x = Mathf.MoveTowards(angularVelocity.x, 0f, rollStabilityAssist * fixedDeltaTime);
            angularVelocity.z = Mathf.MoveTowards(angularVelocity.z, 0f, rollStabilityAssist * fixedDeltaTime);
            body.angularVelocity = angularVelocity;
        }

        private void ConfigureArcadeBody()
        {
            body.centerOfMass = centerOfMassOffset;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private VehicleDriveIntent ReadLocalDriveIntent()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return VehicleDriveIntent.Idle;
            }

            var throttle = 0f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                throttle += 1f;
            }

            var brakeReverse = 0f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                brakeReverse += 1f;
            }

            var steer = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                steer -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                steer += 1f;
            }

            return new VehicleDriveIntent(throttle, steer, brakeReverse);
        }

        private void CacheComponents()
        {
            if (state == null)
            {
                state = GetComponent<NetworkedVehicleState>();
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }
        }
    }
}
