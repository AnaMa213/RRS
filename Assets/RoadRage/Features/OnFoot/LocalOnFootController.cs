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
        private float lookSensitivity = 0.12f;

        [SerializeField]
        private float pitchLimit = 70f;

        [SerializeField]
        private float gravity = -18f;

        private CharacterController characterController;
        private float pitch;
        private float verticalVelocity;

        /// <summary>
        /// Gel complet du mouvement (Story 2.7, bug fix post-implementation) : quand faux, Update()
        /// ne lit plus l'input et n'applique plus la gravite -- contrairement a la seule desactivation
        /// du CharacterController, ceci empeche verticalVelocity de continuer a accumuler de la vitesse
        /// de chute pendant que le joueur est "mort" (cause racine du respawn qui remourrait aussitot).
        /// </summary>
        public bool MovementEnabled { get; set; } = true;

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

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (!MovementEnabled)
            {
                return;
            }

            Step(ReadInputIntent(), Time.deltaTime);
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
            var speed = intent.SprintRequested ? sprintSpeed : walkSpeed;
            var velocity = (horizontal * speed) + (Vector3.up * verticalVelocity);

            characterController.Move(velocity * deltaTime);
        }
    }
}
