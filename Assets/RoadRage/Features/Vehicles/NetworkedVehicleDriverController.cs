using System;
using RoadRage.Shared.Domain;
using RoadRage.Shared.Input;
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
    /// calculees et appliquees cote host uniquement (Story 5.3 : le solo est desormais toujours
    /// host-authoritative, meme garde d'autorite que le reste du fichier), jamais cote client reseau. Les collisions et
    /// recuperations sont exposees en evenements C# purs (aucune reference UI ici) pour rester
    /// consommables uniquement depuis App/Run, conformement a la frontiere du module Vehicules.
    /// Story 5.11 : la couche physique du vehicule vit dans <see cref="VehiclePhysicsBody"/> (AD-35),
    /// porte par ce prefab comme par le prefab IA. Ce controleur ne garde que le longitudinal et le
    /// lateral ; l'axe vertical appartient a la suspension et n'est plus ecrit ici.
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
        private Collider bodyCollider;
        private VehiclePhysicsBody physicsBody;
        private DevIndestructibleVehicle devIndestructible;
        private NetworkTransform networkTransform;
        private VehicleDriveIntent latestIntent = VehicleDriveIntent.Idle;
        private float rolloverElapsedSeconds;
        private Vector3 fallbackRecoveryPosition;
        private Quaternion fallbackRecoveryRotation;
        private bool fallbackRecoveryCaptured;

        /// <summary>Story 5.5 : portee/magnitude/canal authores du klaxon, pousses par App/Run (donnee de tuning de rage) via <see cref="ConfigureHonkReaction"/> -- inertes tant qu'ils ne sont pas configures.</summary>
        private float honkRange;

        private float honkMagnitude;

        private ReactionChannel honkChannel = ReactionChannel.None;

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

        /// <summary>Klaxon : evenement purement presentation, aucun etat mute -- toujours leve, meme sans cible resolue.</summary>
        public event Action VehicleHonked;

        /// <summary>
        /// Story 5.5 : cible unique resolue par le klaxon (host-authoritative), avec l'effet rage/peur
        /// authore a lui appliquer. Features/Vehicles ne mute jamais la rage directement (Epic 5,
        /// Technical Decisions) : App/Run s'abonne et effectue lui-meme la mutation cote host.
        /// </summary>
        public event Action<NetworkedAIVehicleState, NpcReactionEffect> HonkTargetResolved;

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
                // Story 5.11 : masse, centre de masse et tenseur d'inertie ne sont plus poses ici --
                // VehiclePhysicsBody les applique depuis le VehicleProfileDef authore (AD-35).
                body.isKinematic = !IsServer;
            }

            if (IsServer && state != null)
            {
                state.EnsureDamageStateInitialized();
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

            if (state.DriverClientId.Value != localClientId)
            {
                return;
            }

            if (state.IsInoperable())
            {
                latestIntent = VehicleDriveIntent.Idle;
                return;
            }

            if (LocalInputGate.IsBlocked)
            {
                // Menu d'echappement ouvert (Story 5.8) : l'intention doit continuer d'etre soumise a
                // zero, sinon le host conserve la derniere intention recue (plein gaz) et la voiture
                // continuerait d'accelerer pendant que le joueur a son menu ouvert.
                SubmitDriveIntent(VehicleDriveIntent.Idle, localClientId);
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

            UpdateRecoveryDetection(Time.fixedDeltaTime);

            if (state.IsInoperable())
            {
                latestIntent = VehicleDriveIntent.Idle;
                return;
            }

            if (state.DriverClientId.Value == NetworkedVehicleState.UnclaimedDriverClientId)
            {
                return;
            }

            ApplyPhysics(latestIntent);
        }

        /// <summary>
        /// Detection host/solo-only (Story 3.4) : retournement soutenu N secondes, avec la meme garde de
        /// duree contre les faux positifs que le reste du fichier, et sortie de zone via le meme motif de seuil de vide
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
            if (!IsServer || body == null)
            {
                return;
            }

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = position;
            body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
            rolloverElapsedSeconds = 0f;

            // Deplacement discontinu : la memoire de suspension est purgee, sinon la difference finie
            // de compression produirait au pas suivant un pic d'amortisseur que rien ne justifie.
            if (physicsBody != null)
            {
                physicsBody.ResetSuspensionState();
            }

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
        ///
        /// Depuis la Story 5.11 les roues portent le vehicule, donc heurter un trottoir ou monter une
        /// levre de dalle n'est plus un choc : c'est de la compression de suspension. Un contact qui ne
        /// touche que le DESSOUS du vehicule -- donc un relief franchi, pas un obstacle -- ne leve ni
        /// evenement ni degat. Un mur (5 m de haut) et une autre voiture touchent la caisse bien plus
        /// haut, donc leurs degats de la Story 3.5 restent intacts.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (!IsServer)
            {
                return;
            }

            // Vehicule de developpement marque indestructible : aucun evenement de collision n'est
            // leve, donc ni le vehicule ni ses occupants ne perdent de PV, et aucune RPC de degat n'est
            // relayee. Un seul point a garder, et le code de jeu n'a pas a connaitre le developpement.
            // Le marqueur doit etre ACTIF : un composant desactive doit rendre l'immunite, pas la laisser.
            if (devIndestructible != null && devIndestructible.isActiveAndEnabled)
            {
                return;
            }

            if (IsSurfaceOnlyCollision(collision))
            {
                return;
            }

            var impactSpeed = collision.relativeVelocity.magnitude;
            VehicleCollided?.Invoke(impactSpeed);
            NotifyClientsIfNetworked(NotifyVehicleCollidedRpc, impactSpeed);
        }

        /// <summary>
        /// Vrai si TOUS les points de contact de cette entree ne touchent que le dessous du vehicule :
        /// l'entree est alors un relief franchi -- trottoir, levre de dalle, bordure authoree -- et non
        /// un obstacle, donc elle ne produit aucun degat.
        ///
        /// Le "tous" est ce qui preserve les vrais chocs : une paroi de tunnel monte a 5 m et touche
        /// aussi la face de la caisse, donc l'entree reste un choc. Une entree sans aucun point de
        /// contact n'est pas classee de surface : dans le doute, le comportement de la Story 3.5 est
        /// conserve.
        ///
        /// La tolerance vient du PROFIL PHYSIQUE authore, jamais d'une constante de ce fichier : elle
        /// doit couvrir la bordure authoree (0,12 m) sans jamais couvrir la face d'un mur, et c'est le
        /// meme reglage pour la voiture joueur et pour tout vehicule IA.
        /// </summary>
        private bool IsSurfaceOnlyCollision(Collision collision)
        {
            if (physicsBody == null || !physicsBody.HasProfile || bodyCollider == null)
            {
                return false;
            }

            var contactCount = collision.contactCount;
            if (contactCount <= 0)
            {
                return false;
            }

            var underside = bodyCollider.bounds.min.y;
            var tolerance = physicsBody.Profile.SurfaceContactTolerance;

            for (var i = 0; i < contactCount; i++)
            {
                if (!VehicleSuspensionModel.IsSurfaceContact(collision.GetContact(i).point.y, underside, tolerance))
                {
                    return false;
                }
            }

            return true;
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
        /// Story 5.5 : pousse les valeurs authorees du klaxon depuis App/Run. Inertes tant qu'aucun
        /// appel n'a eu lieu (honkChannel = None), donc sans effet plutot que de deviner une portee --
        /// meme convention de repli silencieux que le reste du systeme de reaction rage/peur.
        /// </summary>
        public void ConfigureHonkReaction(float range, float magnitude, ReactionChannel channel)
        {
            honkRange = range;
            honkMagnitude = magnitude;
            honkChannel = channel;
        }

        /// <summary>
        /// Klaxon : declenchable par le conducteur local, host ou solo comme le reste du fichier.
        /// Toujours purement cosmetique pour la presentation (HonkRpc, SendTo.Everyone, inchange
        /// depuis Story 3.4/regression Collision-Recuperation). Story 5.5 ajoute la resolution
        /// host-authoritative de la cible unique (lock du joueur si fourni, sinon plus proche eligible
        /// dans la portee klaxon), via le meme point de resolution partage que les provocations
        /// passager (AiRageTargetResolution.ResolveTarget) -- jamais applique directement ici
        /// (Features/Vehicles ne mute jamais la rage), seulement leve en evenement pour App/Run.
        /// </summary>
        public void RequestHonk()
        {
            RequestHonk(null, false);
        }

        public void RequestHonk(NetworkedAIVehicleState lockedTarget, bool lockSpecified)
        {
            if (!IsSpawned)
            {
                VehicleHonked?.Invoke();
                return;
            }

            HonkRpc();

            var lockedReference = lockSpecified
                ? ResolveHonkTargetReference(lockedTarget)
                : new NetworkObjectReference((NetworkObject)null);
            if (IsServer)
            {
                ApplyHonkTarget(lockedReference, lockSpecified, NetworkManager.Singleton == null ? 0UL : NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                SubmitHonkTargetRpc(lockedReference, lockSpecified);
            }
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
        private void HonkRpc()
        {
            VehicleHonked?.Invoke();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SubmitHonkTargetRpc(NetworkObjectReference lockedTargetReference, bool lockSpecified, RpcParams rpcParams = default)
        {
            ApplyHonkTarget(lockedTargetReference, lockSpecified, rpcParams.Receive.SenderClientId);
        }

        /// <summary>
        /// Host-authoritative (Story 5.5) : revalide l'acteur (conducteur courant, klaxon reste
        /// conducteur-only), resout la cible unique via le point de resolution partage puis leve
        /// HonkTargetResolved -- aucune mutation ici, App/Run effectue la mutation cote host.
        /// </summary>
        private void ApplyHonkTarget(NetworkObjectReference lockedTargetReference, bool lockSpecified, ulong senderClientId)
        {
            if (!IsServer || state == null || state.DriverClientId.Value != senderClientId)
            {
                return;
            }

            NetworkedAIVehicleState lockedCandidate = null;
            if (lockSpecified
                && lockedTargetReference.TryGet(out var lockedObject)
                && lockedObject != null
                && lockedObject.IsSpawned)
            {
                lockedCandidate = lockedObject.GetComponent<NetworkedAIVehicleState>();
            }

            var candidates = AiRageTargetResolution.FindEligibleCandidates();
            var resolved = AiRageTargetResolution.ResolveTarget(transform.position, lockSpecified, lockedCandidate, candidates, honkRange, true);
            if (resolved == null)
            {
                return;
            }

            HonkTargetResolved?.Invoke(resolved, new NpcReactionEffect(honkChannel, honkMagnitude));
        }

        private static NetworkObjectReference ResolveHonkTargetReference(NetworkedAIVehicleState lockedTarget)
        {
            if (lockedTarget == null || lockedTarget.NetworkObject == null || !lockedTarget.IsSpawned)
            {
                return new NetworkObjectReference((NetworkObject)null);
            }

            return new NetworkObjectReference(lockedTarget.NetworkObject);
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
            return state != null && state.IsEngineDamaged ? maxForwardSpeed * EngineDamageSpeedMultiplier : maxForwardSpeed;
        }

        /// <summary>Roue endommagee (Story 3.5) : maniabilite reduite.</summary>
        private float ResolveEffectiveSteerDegreesPerSecond()
        {
            return state != null && state.IsWheelDamaged ? steerDegreesPerSecond * WheelDamageSteerMultiplier : steerDegreesPerSecond;
        }

        /// <summary>Freins endommages (Story 3.5) : deceleration de freinage reduite.</summary>
        private float ResolveEffectiveBrakeDeceleration()
        {
            return state != null && state.IsBrakeDamaged ? brakeDeceleration * BrakeDamageDecelerationMultiplier : brakeDeceleration;
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

            // Story 5.11 : l'axe vertical appartient a VehiclePhysicsBody (gravite, ressort,
            // amortisseur, anti-roulis). La composante verticale courante est donc relue et reecrite
            // a l'identique -- un read-modify-write, jamais une decision de mouvement. L'ecriture en
            // bloc disparait en Story 5.12, qui remplace le longitudinal et le lateral par des efforts
            // aux roues.
            body.linearVelocity = (forward * nextLongitudinalSpeed) + (right * nextLateralSpeed) + verticalVelocity;

            ApplySteering(intent, nextLongitudinalSpeed, fixedDeltaTime);
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

            if (bodyCollider == null)
            {
                bodyCollider = GetComponent<Collider>();
            }

            if (physicsBody == null)
            {
                physicsBody = GetComponent<VehiclePhysicsBody>();
            }

            if (devIndestructible == null)
            {
                devIndestructible = GetComponent<DevIndestructibleVehicle>();
            }
        }
    }
}
