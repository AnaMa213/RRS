using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Controleur de conduite IA basique (Story 5.2) : poursuite deterministe des
    /// <see cref="RouteWaypoints"/> assignes, calculee et appliquee host-only (IsServer), position
    /// repliquee vers les clients par le NetworkTransform existant -- aucune RPC de mouvement.
    /// Reutilise tel quel le predicat retournement/hors-zone de
    /// <see cref="NetworkedVehicleDriverController"/> (IsRolledOver / IsBelowVoidHeightThreshold) et
    /// applique la meme detection "soutenue N secondes" pour le blocage (vitesse quasi nulle). Toute
    /// recuperation (retournement, hors-zone ou blocage) reinitialise le vehicule au waypoint courant
    /// -- jamais a RouteIndex/WaypointIndex d'un autre vehicule : chaque instance ne porte que son
    /// propre etat (aucune collection statique/partagee), donc independante par construction.
    /// Desactivation/isolation (AC epic 5) : ce comportement passe entierement par FixedUpdate, donc
    /// decocher le composant (ou son GameObject) dans MVP_Run suffit a arreter le vehicule IA sans
    /// toucher au vehicule joueur -- pas de toggle applicatif dedie necessaire.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedAIVehicleState))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NetworkedAIVehicleDriverController : NetworkBehaviour
    {
        [SerializeField]
        [Tooltip("Geometrie de route (RouteWaypoints) suivie par ce vehicule. Aucune route assignee ou route vide : le vehicule reste immobile.")]
        private RouteWaypoints route;

        [SerializeField]
        [Min(0f)]
        private float cruiseSpeed = 8f;

        [SerializeField]
        [Min(0.01f)]
        private float arrivalRadius = 3f;

        [SerializeField]
        [Min(1f)]
        private float steerFullLockDegrees = 45f;

        [SerializeField]
        [Min(0f)]
        private float steerDegreesPerSecond = 90f;

        [SerializeField]
        [Range(-1f, 1f)]
        private float rolloverUprightDotThreshold = 0.35f;

        [SerializeField]
        [Min(0f)]
        private float rolloverSustainedSeconds = 2f;

        [SerializeField]
        private float voidHeightThreshold = -10f;

        [SerializeField]
        [Min(0f)]
        private float stuckSpeedThreshold = 0.5f;

        [SerializeField]
        [Min(0f)]
        private float stuckSustainedSeconds = 3f;

        private NetworkedAIVehicleState state;
        private Rigidbody body;
        private NetworkTransform networkTransform;
        private float rolloverElapsedSeconds;
        private float stuckElapsedSeconds;

        private void Awake()
        {
            CacheComponents();
        }

        public override void OnNetworkSpawn()
        {
            CacheComponents();

            if (body != null)
            {
                body.isKinematic = !IsServer;
            }

            // Depart host-only sur le repere le plus proche : plusieurs vehicules peuvent partager
            // une meme boucle sans index a cabler instance par instance dans la scene.
            if (IsServer && state != null && route != null && route.Count > 0)
            {
                state.WaypointIndex.Value = route.NearestIndex(transform.position);
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer || body == null || state == null || route == null || route.Count <= 0)
            {
                return;
            }

            var fixedDeltaTime = Time.fixedDeltaTime;
            var waypointIndex = state.WaypointIndex.Value;
            var waypointPosition = route.GetPosition(waypointIndex);

            if (NetworkedVehicleDriverController.IsBelowVoidHeightThreshold(transform.position.y, voidHeightThreshold))
            {
                RecoverAtWaypoint(waypointPosition);
                return;
            }

            if (NetworkedVehicleDriverController.IsRolledOver(transform.up, rolloverUprightDotThreshold))
            {
                rolloverElapsedSeconds += fixedDeltaTime;
                if (rolloverElapsedSeconds >= rolloverSustainedSeconds)
                {
                    RecoverAtWaypoint(waypointPosition);
                    return;
                }
            }
            else
            {
                rolloverElapsedSeconds = 0f;
            }

            var planarSpeed = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
            if (IsStuck(planarSpeed, stuckSpeedThreshold))
            {
                stuckElapsedSeconds += fixedDeltaTime;
                if (stuckElapsedSeconds >= stuckSustainedSeconds)
                {
                    RecoverAtWaypoint(waypointPosition);
                    return;
                }
            }
            else
            {
                stuckElapsedSeconds = 0f;
            }

            if (HasArrivedAtWaypoint(transform.position, waypointPosition, arrivalRadius))
            {
                waypointIndex = route.NextIndex(waypointIndex);
                state.WaypointIndex.Value = waypointIndex;
                waypointPosition = route.GetPosition(waypointIndex);
            }

            var intent = ComputeSeekIntent(transform.position, transform.forward, waypointPosition, arrivalRadius, steerFullLockDegrees);
            ApplyMovement(intent, fixedDeltaTime);
        }

        /// <summary>
        /// Predicat pur (Story 5.2) : intent de poursuite deterministe vers le waypoint -- plein gaz
        /// et direction bornee par steerFullLockDegrees tant que le waypoint est hors du rayon
        /// d'arrivee, sinon Idle (l'appelant avance alors WaypointIndex avant de rappeler avec le
        /// waypoint suivant).
        /// </summary>
        public static VehicleDriveIntent ComputeSeekIntent(Vector3 position, Vector3 forward, Vector3 waypointPosition, float arrivalRadius, float steerFullLockDegrees)
        {
            var toWaypoint = waypointPosition - position;
            toWaypoint.y = 0f;

            if (toWaypoint.sqrMagnitude <= arrivalRadius * arrivalRadius)
            {
                return VehicleDriveIntent.Idle;
            }

            var flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude <= 0.0001f)
            {
                flatForward = Vector3.forward;
            }

            flatForward.Normalize();

            var direction = toWaypoint.normalized;
            var signedAngle = Vector3.SignedAngle(flatForward, direction, Vector3.up);
            var steerLock = Mathf.Max(steerFullLockDegrees, 0.0001f);
            var steer = Mathf.Clamp(signedAngle / steerLock, -1f, 1f);

            return new VehicleDriveIntent(1f, steer, 0f);
        }

        /// <summary>Predicat pur (Story 5.2) : arrivee des lors que la distance planaire au waypoint passe sous le rayon d'arrivee.</summary>
        public static bool HasArrivedAtWaypoint(Vector3 position, Vector3 waypointPosition, float arrivalRadius)
        {
            var toWaypoint = waypointPosition - position;
            toWaypoint.y = 0f;
            return toWaypoint.sqrMagnitude <= arrivalRadius * arrivalRadius;
        }

        /// <summary>Predicat pur (Story 5.2) : vitesse planaire quasi nulle -- meme esprit instantane que IsRolledOver, accumule en FixedUpdate.</summary>
        public static bool IsStuck(float planarSpeed, float stuckSpeedThreshold)
        {
            return planarSpeed <= stuckSpeedThreshold;
        }

        private void ApplyMovement(VehicleDriveIntent intent, float fixedDeltaTime)
        {
            // La composante verticale est preservee comme dans NetworkedVehicleDriverController : la
            // conduite IA ne pilote que le plan horizontal. L'ecraser annulerait la gravite -- le
            // vehicule levite et ne peut plus jamais franchir le seuil de vide, ce qui rendrait la
            // recuperation hors-zone inatteignable.
            var verticalVelocity = Vector3.up * body.linearVelocity.y;

            if (intent.IsIdle)
            {
                body.linearVelocity = verticalVelocity;
                return;
            }

            var yawDegrees = intent.Steer * steerDegreesPerSecond * fixedDeltaTime;
            var rotation = Quaternion.AngleAxis(yawDegrees, Vector3.up) * body.rotation;
            body.MoveRotation(rotation);

            var forward = rotation * Vector3.forward;
            body.linearVelocity = (forward * cruiseSpeed * intent.Throttle) + verticalVelocity;
        }

        /// <summary>
        /// Recuperation (Story 5.2) : reinitialise position/rotation/vitesse au waypoint courant --
        /// jamais un repere de scene fixe (il n'y en a pas pour l'IA) -- et remet le vehicule a
        /// l'endroit (roll corrige), sans avancer WaypointIndex ni emettre de RPC.
        /// </summary>
        private void RecoverAtWaypoint(Vector3 waypointPosition)
        {
            var rotation = ResolveUprightRecoveryRotation();

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = waypointPosition;
            body.rotation = rotation;
            transform.SetPositionAndRotation(waypointPosition, rotation);
            rolloverElapsedSeconds = 0f;
            stuckElapsedSeconds = 0f;

            if (networkTransform != null)
            {
                networkTransform.Teleport(waypointPosition, rotation, transform.localScale);
            }
        }

        private Quaternion ResolveUprightRecoveryRotation()
        {
            var flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (flatForward.sqrMagnitude <= 0.0001f)
            {
                flatForward = Vector3.forward;
            }

            return Quaternion.LookRotation(flatForward.normalized, Vector3.up);
        }

        private void CacheComponents()
        {
            if (state == null)
            {
                state = GetComponent<NetworkedAIVehicleState>();
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
