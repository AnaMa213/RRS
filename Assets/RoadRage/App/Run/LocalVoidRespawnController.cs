using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Chemin 100% local (Story 2.7) du declencheur de mort par chute hors limites et du respawn
    /// individuel, pour la partie solo hors-ligne ou NetworkedPlayerLifecycleService reste inerte
    /// (NetworkManager.IsListening == false, meme pattern d'inertie que Story 2.5/2.6). Composant
    /// MonoBehaviour local pur, sans aucune dependance au systeme reseau du projet (pas de classe de
    /// base reseau, pas de variable synchronisee, pas d'appel de procedure distante) -- meme seuil de
    /// chute et meme touche de respawn que le chemin reseau (NetworkedPlayerLifecycleService /
    /// NetworkedPlayerLifecycleIntent), mais entierement local, y compris le predicat de seuil,
    /// volontairement duplique pour ne creer aucun couplage avec le systeme host-owned. Le gel/
    /// teleport passe par LocalOnFootController.MovementEnabled/Teleport (bug fix post-implementation)
    /// plutot que par une simple desactivation du CharacterController : celle-ci n'empechait pas
    /// verticalVelocity de continuer a accumuler de la gravite pendant la mort, ce qui relancait le
    /// joueur sous le seuil quasi aussitot au respawn.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LocalOnFootController))]
    public sealed class LocalVoidRespawnController : MonoBehaviour
    {
        [SerializeField]
        private float voidHeightThreshold = -10f;

        private LocalOnFootController onFootController;

        private Vector3 spawnPosition;

        private Quaternion spawnRotation;

        private bool isDead;

        public bool IsDead
        {
            get { return isDead; }
        }

        /// <summary>
        /// HUD affecte au retour visuel de mort/respawn (Story 2.7), assigne par RunFlowController
        /// au moment de l'attache (AttachLocalVoidRespawnController) -- jamais serialise, ce composant
        /// est ajoute dynamiquement au joueur local solo, jamais scene-place.
        /// </summary>
        public RunCheckpointHudScreen CheckpointHud { get; set; }

        private void Awake()
        {
            onFootController = GetComponent<LocalOnFootController>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        private void Update()
        {
            if (!isDead)
            {
                if (IsBelowVoidHeightThreshold(transform.position.y, voidHeightThreshold))
                {
                    Die();
                }

                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                Respawn();
            }
        }

        /// <summary>
        /// Meme predicat pur que NetworkedPlayerLifecycleService.IsBelowVoidHeightThreshold,
        /// intentionnellement duplique ici pour que ce composant reste sans aucune dependance au
        /// systeme reseau, tout en gardant un contrat testable identique.
        /// </summary>
        public static bool IsBelowVoidHeightThreshold(float positionY, float voidHeightThreshold)
        {
            return positionY < voidHeightThreshold;
        }

        private void Die()
        {
            isDead = true;

            if (onFootController != null)
            {
                onFootController.MovementEnabled = false;
            }

            if (CheckpointHud != null)
            {
                CheckpointHud.SetHearts(0, NetworkedPlayerState.DefaultMaxHearts);
                CheckpointHud.ShowDeathOverlay();
            }
        }

        private void Respawn()
        {
            isDead = false;

            if (onFootController != null)
            {
                onFootController.Teleport(spawnPosition, spawnRotation);
                onFootController.MovementEnabled = true;
            }

            if (CheckpointHud != null)
            {
                CheckpointHud.SetHearts(NetworkedPlayerState.DefaultMaxHearts, NetworkedPlayerState.DefaultMaxHearts);
                CheckpointHud.HideDeathOverlay();
            }
        }
    }
}
