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

        /// <summary>
        /// HP local solo (Story 3.5) : distinct de la fenetre Downed/resurrection reseau (aucun
        /// coequipier n'existe en solo pur) -- reutilise directement le meme mecanisme Die()/Respawn()
        /// deja etabli pour la chute hors limites (Story 2.7), pour que le contrat de degats vehicule
        /// reste jouable/testable seul (Acceptance Criteria Story 3.5) sans dupliquer un second etat
        /// de cycle de vie complet.
        /// </summary>
        private int hp = NetworkedPlayerState.DefaultMaxHp;

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

        /// <summary>
        /// Degats de collision vehicule (Story 3.5) en solo pur : reduit le HP local, et declenche
        /// Die() (meme chemin que la chute hors limites) une fois a 0 -- aucune fenetre Downed/
        /// resurrection ici, un seul joueur existe en solo.
        /// </summary>
        public void ApplyVehicleCollisionDamage(int amount)
        {
            if (isDead || amount <= 0)
            {
                return;
            }

            hp = Mathf.Max(0, hp - amount);
            if (CheckpointHud != null)
            {
                CheckpointHud.SetHp(hp, NetworkedPlayerState.DefaultMaxHp);
            }

            if (hp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            hp = 0;

            if (onFootController != null)
            {
                onFootController.MovementEnabled = false;
            }

            if (CheckpointHud != null)
            {
                CheckpointHud.SetHp(0, NetworkedPlayerState.DefaultMaxHp);
                CheckpointHud.ShowDeathOverlay();
            }
        }

        private void Respawn()
        {
            isDead = false;
            hp = NetworkedPlayerState.DefaultMaxHp;

            if (onFootController != null)
            {
                onFootController.Teleport(spawnPosition, spawnRotation);
                onFootController.MovementEnabled = true;
            }

            if (CheckpointHud != null)
            {
                CheckpointHud.SetHp(NetworkedPlayerState.DefaultMaxHp, NetworkedPlayerState.DefaultMaxHp);
                CheckpointHud.HideDeathOverlay();
            }
        }
    }
}
