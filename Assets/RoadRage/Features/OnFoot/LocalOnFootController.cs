using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.Features.OnFoot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class LocalOnFootController : MonoBehaviour
    {
        private const float GroundedVerticalVelocity = -2f;

        [SerializeField]
        private Camera playerCamera;

        [SerializeField]
        private float walkSpeed = 4f;

        [SerializeField]
        private float sprintSpeed = 6.5f;

        [SerializeField]
        [Range(0f, 1f)]
        private float staminaNormalized = 1f;

        [SerializeField]
        [Min(0f)]
        private float sprintStaminaDrainPerSecond = 0.28f;

        [SerializeField]
        [Min(0f)]
        private float staminaRecoveryPerSecond = 0.22f;

        [SerializeField]
        [Min(0f)]
        private float staminaRecoveryDelaySeconds = 1f;

        /// <summary>
        /// Multiplicateur de vitesse (Story 3.5) applique tant que IsDowned est vrai -- plafond de
        /// deplacement distinct du gel complet existant (MovementEnabled) : un joueur Downed reste
        /// mobile (pour se rapprocher d'un coequipier ou fuir) mais nettement ralenti en attendant la
        /// resurrection ou l'expiration de fenetre.
        /// </summary>
        [SerializeField]
        [Range(0.05f, 1f)]
        private float downedSpeedMultiplier = 0.35f;

        [SerializeField]
        private float lookSensitivity = 0.12f;

        [SerializeField]
        private float pitchLimit = 70f;

        [SerializeField]
        private float gravity = -18f;

        private CharacterController characterController;
        private float pitch;
        private float verticalVelocity;
        private float secondsSinceSprintStopped = 1f;

        /// <summary>
        /// Gel complet du mouvement (Story 2.7, bug fix post-implementation) : quand faux, Update()
        /// ne lit plus l'input et n'applique plus la gravite -- contrairement a la seule desactivation
        /// du CharacterController, ceci empeche verticalVelocity de continuer a accumuler de la vitesse
        /// de chute pendant que le joueur est "mort" (cause racine du respawn qui remourrait aussitot).
        /// </summary>
        public bool MovementEnabled { get; set; } = true;

        /// <summary>
        /// Plafond de vitesse (Story 3.5) reflete localement depuis NetworkedPlayerState.Lifecycle ==
        /// Downed (RunFlowController) ou l'equivalent solo (LocalVoidRespawnController) -- jamais mute
        /// depuis ce composant lui-meme, meme invariant "lecture seule" que MovementEnabled.
        /// </summary>
        public bool IsDowned { get; set; }

        public Camera PlayerCamera
        {
            get { return playerCamera; }
        }

        public float WalkSpeed
        {
            get { return walkSpeed; }
        }

        public float SprintSpeed
        {
            get { return sprintSpeed; }
        }

        public float StaminaNormalized
        {
            get { return staminaNormalized; }
        }

        public event Action<float> StaminaChanged;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            staminaNormalized = Mathf.Clamp01(staminaNormalized);

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            if (!MovementEnabled)
            {
                UpdateStamina(false, false, deltaTime);
                return;
            }

            Step(ReadInputIntent(), deltaTime);
        }

        public void AttachCamera(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            playerCamera = camera;
            playerCamera.transform.SetParent(transform, false);
            playerCamera.transform.localPosition = new Vector3(0f, 1.62f, -3.2f);
            playerCamera.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            pitch = 14f;
        }

        public void Step(OnFootMovementIntent intent, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            ApplyLook(intent.Look);
            ApplyMovement(intent, deltaTime);
        }

        /// <summary>
        /// Repositionne le rig et remet la vitesse de chute accumulee a l'etat "au sol" (Story 2.7,
        /// bug fix post-implementation) : c'est cette remise a zero de verticalVelocity -- pas
        /// seulement le repositionnement -- qui empeche le joueur de retraverser aussitot le seuil de
        /// chute au respawn avec l'ancienne vitesse enorme accumulee pendant qu'il etait "mort".
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            var wasControllerEnabled = characterController != null && characterController.enabled;
            if (wasControllerEnabled)
            {
                characterController.enabled = false;
            }

            transform.SetPositionAndRotation(position, rotation);
            verticalVelocity = GroundedVerticalVelocity;

            if (wasControllerEnabled)
            {
                characterController.enabled = true;
            }
        }

        private OnFootMovementIntent ReadInputIntent()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            var move = Vector2.zero;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    move.x -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    move.x += 1f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    move.y -= 1f;
                }

                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    move.y += 1f;
                }
            }

            var look = mouse == null ? Vector2.zero : mouse.delta.ReadValue();
            var sprint = keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

            return new OnFootMovementIntent(move, look, sprint);
        }

        private void ApplyLook(Vector2 look)
        {
            if (look.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.Rotate(Vector3.up, look.x * lookSensitivity, Space.World);

            if (playerCamera != null)
            {
                pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, -pitchLimit, pitchLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void ApplyMovement(OnFootMovementIntent intent, float deltaTime)
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (characterController == null)
            {
                return;
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = GroundedVerticalVelocity;
            }
            else
            {
                verticalVelocity += gravity * deltaTime;
            }

            var horizontal = (transform.right * intent.Move.x) + (transform.forward * intent.Move.y);
            var isSprinting = ShouldSprint(intent.Move, intent.SprintRequested, IsDowned, staminaNormalized);
            UpdateStamina(isSprinting, WantsSprint(intent.Move, intent.SprintRequested, IsDowned), deltaTime);

            var speed = isSprinting ? sprintSpeed : walkSpeed;
            if (IsDowned)
            {
                speed *= downedSpeedMultiplier;
            }

            var velocity = (horizontal * speed) + (Vector3.up * verticalVelocity);

            characterController.Move(velocity * deltaTime);
        }

        public static bool ShouldSprint(Vector2 move, bool sprintRequested, bool isDowned, float currentStaminaNormalized)
        {
            return WantsSprint(move, sprintRequested, isDowned) && currentStaminaNormalized > 0f;
        }

        public static bool WantsSprint(Vector2 move, bool sprintRequested, bool isDowned)
        {
            return sprintRequested && !isDowned && move.sqrMagnitude > 0.0001f;
        }

        public static float ComputeNextStamina(
            float currentStaminaNormalized,
            bool isSprinting,
            bool canRecover,
            float deltaTime,
            float drainPerSecond,
            float recoveryPerSecond)
        {
            if (deltaTime <= 0f)
            {
                return Mathf.Clamp01(currentStaminaNormalized);
            }

            if (isSprinting)
            {
                return Mathf.Clamp01(currentStaminaNormalized - (Mathf.Max(0f, drainPerSecond) * deltaTime));
            }

            if (canRecover)
            {
                return Mathf.Clamp01(currentStaminaNormalized + (Mathf.Max(0f, recoveryPerSecond) * deltaTime));
            }

            return Mathf.Clamp01(currentStaminaNormalized);
        }

        private void UpdateStamina(bool isSprinting, bool wantsSprint, float deltaTime)
        {
            if (isSprinting)
            {
                secondsSinceSprintStopped = 0f;
                SetStaminaNormalized(ComputeNextStamina(
                    staminaNormalized,
                    true,
                    false,
                    deltaTime,
                    sprintStaminaDrainPerSecond,
                    staminaRecoveryPerSecond));
                return;
            }

            if (!wantsSprint)
            {
                secondsSinceSprintStopped += Mathf.Max(0f, deltaTime);
            }

            SetStaminaNormalized(ComputeNextStamina(
                staminaNormalized,
                false,
                secondsSinceSprintStopped >= staminaRecoveryDelaySeconds,
                deltaTime,
                sprintStaminaDrainPerSecond,
                staminaRecoveryPerSecond));
        }

        private void SetStaminaNormalized(float value)
        {
            var clamped = Mathf.Clamp01(value);
            if (Mathf.Approximately(staminaNormalized, clamped))
            {
                return;
            }

            staminaNormalized = clamped;
            StaminaChanged?.Invoke(staminaNormalized);
        }
    }
}
