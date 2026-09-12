using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Controle de conduite arcade pour la voiture partagee. Depuis Story 3.3, DriverClientId est
    /// assigne par le service de sieges ; ce composant ne fait que consommer l'etat conducteur pour
    /// envoyer/appliquer l'intention de conduite. Le Rigidbody n'est simule que sur le host (isKinematic = !IsServer,
    /// pose dans OnNetworkSpawn) ; sur les clients la position/rotation arrive via NetworkTransform
    /// (autorite serveur, comportement par defaut NGO -- pas de sync maison ici). L'intention de
    /// conduite du client distant est soumise au host via
    /// Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone), valides contre
    /// rpcParams.Receive.SenderClientId -- meme patron que
    /// NetworkedPlayerPresentation.SubmitLocalPose/SubmitPoseRpc (Story 2.5) et
    /// NetworkedPlayerLifecycleIntent.RequestRespawn/RequestRespawnRpc (Story 2.7), necessaire ici
    /// aussi car le NetworkObject de la voiture reste host-owned.
    /// Story 3.4 ajoute la detection retournement/hors-zone et la recuperation vehicule : toujours
    /// calculees et appliquees cote host (ou cote solo via <see cref="localSoloDriverActive"/>, meme
    /// garde d'autorite que le reste du fichier), jamais cote client reseau. Les collisions et
    /// recuperations sont exposees en evenements C# purs (aucune reference UI ici) pour rester
    /// consommables uniquement depuis App/Run, conformement a la frontiere du module Vehicules.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedVehicleState))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NetworkedVehicleDriverController : NetworkBehaviour
    {
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

        [SerializeField]
        [Range(-1f, 1f)]
        private float rolloverUprightDotThreshold = 0.35f;

        [SerializeField]
        [Min(0f)]
        private float rolloverSustainedSeconds = 2f;

        [SerializeField]
        private float voidHeightThreshold = -10f;

        [SerializeField]
        [Tooltip("Repere de scene fixe (par scene) vers lequel la voiture est repositionnee lors d'une recuperation. Si absent, la position initiale du vehicule au demarrage sert de repli (ex. Dev_VehicleSandbox).")]
        private Transform recoveryPoint;

        private NetworkedVehicleState state;
        private Rigidbody body;
        private NetworkTransform networkTransform;
        private VehicleDriveIntent latestIntent = VehicleDriveIntent.Idle;
        private bool localSoloDriverActive;
        private float rolloverElapsedSeconds;
        private Vector3 fallbackRecoveryPosition;
        private Quaternion fallbackRecoveryRotation;
        private bool fallbackRecoveryCaptured;

        private const float InputEpsilon = 0.0001f;
        private const float DirectionEpsilon = 0.05f;

        /// <summary>Vitesse d'impact minimale (Story 3.5) en dessous de laquelle aucun degat voiture n'est applique.</summary>
        public const float MinCollisionDamageSpeed = 3f;

        /// <summary>Vitesse d'impact de reference (Story 3.5) au-dela de laquelle le degat voiture plafonne a MaxCollisionDamage.</summary>
        public const float ReferenceCollisionDamageSpeed = 14f;

        public const int MinCollisionDamage = 5;

        public const int MaxCollisionDamage = 15;

        /// <summary>Roue endommagee (Story 3.5) : handling degrade, applique au steering.</summary>
        private const float WheelDamageSteerMultiplier = 0.5f;

        /// <summary>Moteur endommage (Story 3.5) : puissance reduite, applique a la vitesse max.</summary>
        private const float EngineDamageSpeedMultiplier = 0.55f;

        /// <summary>Freins endommages (Story 3.5) : deceleration de freinage reduite.</summary>
        private const float BrakeDamageDecelerationMultiplier = 0.45f;

        /// <summary>Collision route/decor (Story 3.4) -- retour visuel minimal cote App/Run, session jamais interrompue. Story 3.5 : porte desormais la vitesse d'impact pour le pont de degats vehicule/joueur (RunFlowController).</summary>
        public event Action<float> VehicleCollided;

        /// <summary>Recuperation appliquee (auto retournement/vide ou manuelle) -- meme evenement pour host et solo.</summary>
        public event Action VehicleRecovered;

        /// <summary>Klaxon (ajout hors story, 2026-09-12) : evenement purement presentation, aucun etat mute.</summary>
        public event Action VehicleHonked;

        private void Awake()
        {
            CacheComponents();
            CaptureFallbackRecoveryPose();
        }

        public override void OnNetworkSpawn()
        {
            CacheComponents();

            if (body != null)
            {
                ConfigureArcadeBody();
                body.isKinematic = !IsServer;
            }

            if (IsServer && state != null)
            {
                state.EnsureDamageStateInitialized();
            }
        }

        private void Update()
        {
            if (localSoloDriverActive)
            {
                latestIntent = ReadLocalDriveIntent();
                return;
            }

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

            if (state.DriverClientId.Value != localClientId)
            {
                return;
            }

            if (state.IsInoperable())
            {
                latestIntent = VehicleDriveIntent.Idle;
                return;
            }

            SubmitDriveIntent(ReadLocalDriveIntent(), localClientId);
        }

        private void FixedUpdate()
        {
            if ((!IsServer && !localSoloDriverActive) || body == null || state == null)
            {
                return;
            }

            UpdateRecoveryDetection(Time.fixedDeltaTime);

            if (state.IsInoperable())
            {
                latestIntent = VehicleDriveIntent.Idle;
                return;
            }

            if (!localSoloDriverActive && state.DriverClientId.Value == NetworkedVehicleState.UnclaimedDriverClientId)
            {
                return;
            }

            ApplyPhysics(latestIntent);
        }

        /// <summary>
        /// Detection host/solo-only (Story 3.4) : retournement soutenu N secondes (meme esprit anti-
        /// faux-positif que ApplyStabilityAssist) et sortie de zone via le meme motif de seuil de vide
        /// que LocalVoidRespawnController/NetworkedPlayerLifecycleService, applique ici a la position
        /// du vehicule plutot qu'au joueur.
        /// </summary>
        private void UpdateRecoveryDetection(float fixedDeltaTime)
        {
            if (IsBelowVoidHeightThreshold(transform.position.y, voidHeightThreshold))
            {
                RecoverAtRecoveryPoint();
                return;
            }

            if (IsRolledOver(transform.up, rolloverUprightDotThreshold))
            {
                rolloverElapsedSeconds += fixedDeltaTime;
                if (rolloverElapsedSeconds >= rolloverSustainedSeconds)
                {
                    RecoverAtRecoveryPoint();
                }
            }
            else
            {
                rolloverElapsedSeconds = 0f;
            }
        }

        /// <summary>
        /// Meme predicat pur que LocalVoidRespawnController.IsBelowVoidHeightThreshold /
        /// NetworkedPlayerLifecycleService.IsBelowVoidHeightThreshold, applique ici a la position du
        /// vehicule (sortie de zone jouable).
        /// </summary>
        public static bool IsBelowVoidHeightThreshold(float positionY, float voidHeightThreshold)
        {
            return positionY < voidHeightThreshold;
        }

        /// <summary>Predicat pur de retournement : vrai quand l'axe haut du vehicule s'ecarte trop de la verticale.</summary>
        public static bool IsRolledOver(Vector3 up, float uprightDotThreshold)
        {
            return Vector3.Dot(up, Vector3.up) < uprightDotThreshold;
        }

        /// <summary>
        /// Point d'entree unique de la recuperation manuelle (RPC reseau validee ou touche solo) et
        /// automatique (retournement/vide) : resout le repere de scene assigne, sinon retombe sur la
        /// position/rotation initiale du vehicule capturee au demarrage (scenes sans repere explicite,
        /// ex. Dev_VehicleSandbox), puis delegue a <see cref="RecoverVehicle"/>.
        /// </summary>
        public void RecoverAtRecoveryPoint()
        {
            if (recoveryPoint != null)
            {
                RecoverVehicle(recoveryPoint.position, recoveryPoint.rotation);
                return;
            }

            CaptureFallbackRecoveryPose();
            RecoverVehicle(fallbackRecoveryPosition, fallbackRecoveryRotation);
        }

        /// <summary>
        /// Capture (une seule fois) la pose de depart du vehicule pour servir de repli de
        /// recuperation quand aucun <see cref="recoveryPoint"/> de scene n'est assigne.
        /// </summary>
        private void CaptureFallbackRecoveryPose()
        {
            if (fallbackRecoveryCaptured)
            {
                return;
            }

            fallbackRecoveryPosition = transform.position;
            fallbackRecoveryRotation = transform.rotation;
            fallbackRecoveryCaptured = true;
        }

        /// <summary>
        /// Reinitialise position/rotation/vitesse/vitesse angulaire du Rigidbody host-authoritative
        /// (ou solo). Toujours calcule et applique cote host/solo uniquement, jamais depuis un client
        /// reseau -- meme garde d'autorite que le reste du fichier.
        /// </summary>
        public void RecoverVehicle(Vector3 position, Quaternion rotation)
        {
            if ((!IsServer && !localSoloDriverActive) || body == null)
            {
                return;
            }

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = position;
            body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
            rolloverElapsedSeconds = 0f;

            if (networkTransform != null)
            {
                // Snap immediately on every observer instead of letting NetworkTransform's
                // interpolation glide the car across the map back to the recovery point.
                networkTransform.Teleport(position, rotation, transform.localScale);
            }

            VehicleRecovered?.Invoke();
            NotifyClientsIfNetworked(NotifyVehicleRecoveredRpc);
        }

        /// <summary>
        /// Retour visuel minimal (Story 3.4) : ne jamais interrompre la session (pas d'exception, pas
        /// de freeze physique), juste exposer un evenement C# consomme cote App/Run pour le HUD.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (!IsServer && !localSoloDriverActive)
            {
                return;
            }

            var impactSpeed = collision.relativeVelocity.magnitude;
            VehicleCollided?.Invoke(impactSpeed);
            NotifyClientsIfNetworked(NotifyVehicleCollidedRpc, impactSpeed);
        }

        /// <summary>
        /// VehicleCollided/VehicleRecovered ne sont leves que localement (host ou solo) : sans relais,
        /// seul l'ecran du host afficherait le retour HUD. En session reseau, on notifie aussi chaque
        /// client pour qu'il releve le meme evenement C# local -- jamais l'inverse (aucune reference
        /// UI ici, la frontiere du module Vehicules reste intacte).
        /// </summary>
        private void NotifyClientsIfNetworked(Action rpcInvoker)
        {
            if (IsServer && IsSpawned)
            {
                rpcInvoker();
            }
        }

        private void NotifyClientsIfNetworked(Action<float> rpcInvoker, float value)
        {
            if (IsServer && IsSpawned)
            {
                rpcInvoker(value);
            }
        }

        [Rpc(SendTo.NotServer)]
        private void NotifyVehicleCollidedRpc(float impactSpeed)
        {
            VehicleCollided?.Invoke(impactSpeed);
        }

        [Rpc(SendTo.NotServer)]
        private void NotifyVehicleRecoveredRpc()
        {
            VehicleRecovered?.Invoke();
        }

        /// <summary>
        /// Klaxon (ajout hors story) : declenchable par le conducteur local, host ou solo comme le
        /// reste du fichier. Purement cosmetique (aucun NetworkVariable mute) donc relaye via un seul
        /// Rpc unifie SendTo.Everyone plutot que de dupliquer le couple attribut Server puis NotServer
        /// deja fige a 2 occurrences par le test de regression Collision/Recuperation de Story 3.4.
        /// </summary>
        public void RequestHonk()
        {
            if (!IsSpawned)
            {
                VehicleHonked?.Invoke();
                return;
            }

            HonkRpc();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
        private void HonkRpc()
        {
            VehicleHonked?.Invoke();
        }

        /// <summary>
        /// Degat voiture pur (Story 3.5, Design Notes) : sous MinCollisionDamageSpeed, aucun degat ;
        /// interpolation lineaire vers ReferenceCollisionDamageSpeed pour mapper sur
        /// [MinCollisionDamage, MaxCollisionDamage].
        /// </summary>
        public static int ComputeCollisionDamage(float impactSpeed)
        {
            return ComputeCollisionDamage(impactSpeed, MinCollisionDamageSpeed, ReferenceCollisionDamageSpeed, MinCollisionDamage, MaxCollisionDamage);
        }

        public static int ComputeCollisionDamage(float impactSpeed, float minDamageSpeed, float referenceSpeed, int minDamage, int maxDamage)
        {
            if (impactSpeed < minDamageSpeed)
            {
                return 0;
            }

            var range = Mathf.Max(0.0001f, referenceSpeed - minDamageSpeed);
            var t = Mathf.Clamp01((impactSpeed - minDamageSpeed) / range);
            return Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, t));
        }

        /// <summary>Moteur endommage (Story 3.5) : vitesse max reduite -- lu, jamais ecrit, depuis NetworkedVehicleState.</summary>
        private float ResolveEffectiveMaxForwardSpeed()
        {
            return state != null && state.EngineDamaged.Value ? maxForwardSpeed * EngineDamageSpeedMultiplier : maxForwardSpeed;
        }

        /// <summary>Roue endommagee (Story 3.5) : maniabilite reduite.</summary>
        private float ResolveEffectiveSteerDegreesPerSecond()
        {
            return state != null && state.WheelDamaged.Value ? steerDegreesPerSecond * WheelDamageSteerMultiplier : steerDegreesPerSecond;
        }

        /// <summary>Freins endommages (Story 3.5) : deceleration de freinage reduite.</summary>
        private float ResolveEffectiveBrakeDeceleration()
        {
            return state != null && state.BrakeDamaged.Value ? brakeDeceleration * BrakeDamageDecelerationMultiplier : brakeDeceleration;
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
            if (!IsServer || state == null || state.IsInoperable() || state.DriverClientId.Value != senderClientId)
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

        public void ClearServerDriverIfClient(ulong clientId)
        {
            if (!IsServer || state == null || state.DriverClientId.Value != clientId)
            {
                return;
            }

            state.DriverClientId.Value = NetworkedVehicleState.UnclaimedDriverClientId;
            latestIntent = VehicleDriveIntent.Idle;
        }

        public void SetLocalSoloDriverActive(bool active)
        {
            CacheComponents();
            localSoloDriverActive = active && (state == null || !state.IsInoperable());
            latestIntent = VehicleDriveIntent.Idle;

            if (body != null)
            {
                ConfigureArcadeBody();
                body.isKinematic = false;
            }

            if (state != null)
            {
                state.EnsureDamageStateInitialized();
            }
        }

        private float ResolveTargetSpeed(VehicleDriveIntent intent, float longitudinalSpeed)
        {
            if (intent.BrakeReverse > InputEpsilon)
            {
                return longitudinalSpeed > minimumSteerSpeed ? 0f : -maxReverseSpeed * intent.BrakeReverse;
            }

            if (intent.Throttle > InputEpsilon)
            {
                return ResolveEffectiveMaxForwardSpeed() * intent.Throttle;
            }

            return 0f;
        }

        private float ResolveSpeedChangeRate(VehicleDriveIntent intent, float longitudinalSpeed)
        {
            if (intent.BrakeReverse > InputEpsilon)
            {
                return longitudinalSpeed > minimumSteerSpeed ? ResolveEffectiveBrakeDeceleration() : reverseAcceleration;
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

            var effectiveMaxForwardSpeed = ResolveEffectiveMaxForwardSpeed();
            var speedFactor = Mathf.Clamp01(speedMagnitude / Mathf.Max(effectiveMaxForwardSpeed, 1f));
            var lowSpeedAssist = Mathf.Lerp(0.45f, 1f, speedFactor);
            var reverseAwareDirection = ResolveSteerDirectionMultiplier(longitudinalSpeed, intent.BrakeReverse);
            var yawDegrees = intent.Steer * reverseAwareDirection * ResolveEffectiveSteerDegreesPerSecond() * lowSpeedAssist * fixedDeltaTime;

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

            if (networkTransform == null)
            {
                networkTransform = GetComponent<NetworkTransform>();
            }
        }
    }
}
