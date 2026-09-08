using RoadRage.Features.OnFoot;
using RoadRage.Features.Players;
using RoadRage.Features.Run;
using RoadRage.Shared.Presentation;
using UnityEngine;

namespace RoadRage.App.Run
{
    [DisallowMultipleComponent]
    public sealed class RunFlowController : MonoBehaviour
    {
        public const string MissingProfileMessage = "Entree monde refusee : aucun profil joueur confirme.";

        public const string MissingCharacterMessage = "Entree monde refusee : le personnage selectionne est absent du catalogue.";

        [SerializeField]
        private RunCompositionRoot compositionRoot;

        [SerializeField]
        private CharacterCatalog characterCatalog;

        [SerializeField]
        private Camera playerCamera;

        [SerializeField]
        private Transform playerSpawnPoint;

        private GameObject activeLocalPlayer;

        public GameObject ActiveLocalPlayer
        {
            get { return activeLocalPlayer; }
        }

        private void Start()
        {
            string error;
            if (!TrySpawnSelectedProfile(out error))
            {
                Debug.LogWarning("[Run] " + error);
                PublishWarning(error);
            }
        }

        public bool TrySpawnSelectedProfile(out string error)
        {
            if (activeLocalPlayer != null)
            {
                error = string.Empty;
                return true;
            }

            var bootstrap = RoadRageBootstrap.EnsureInstance();
            if (bootstrap == null || bootstrap.Profiles == null || !bootstrap.Profiles.HasProfile)
            {
                error = MissingProfileMessage;
                return false;
            }

            if (characterCatalog == null || !characterCatalog.TryGetById(bootstrap.Profiles.Current.CharacterId, out var character) || character == null)
            {
                error = MissingCharacterMessage;
                return false;
            }

            if (character.PreviewPrefab == null)
            {
                error = MissingCharacterMessage;
                return false;
            }

            var parent = compositionRoot == null || compositionRoot.RuntimeRoot == null
                ? transform
                : compositionRoot.RuntimeRoot;
            var spawn = playerSpawnPoint == null
                ? (compositionRoot == null ? null : compositionRoot.SpawnRoot)
                : playerSpawnPoint;

            activeLocalPlayer = new GameObject("LocalPlayer_" + character.RawId);
            activeLocalPlayer.transform.SetParent(parent, false);
            activeLocalPlayer.transform.position = spawn == null ? Vector3.zero : spawn.position;
            activeLocalPlayer.transform.rotation = spawn == null ? Quaternion.identity : spawn.rotation;

            var controller = activeLocalPlayer.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 50f;

            var visual = Instantiate(character.PreviewPrefab, activeLocalPlayer.transform);
            visual.name = character.PreviewPrefab.name + "_Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            DisableVisualColliders(visual);

            var onFootController = activeLocalPlayer.AddComponent<LocalOnFootController>();
            onFootController.AttachCamera(playerCamera);

            Debug.Log("[Run] Joueur local spawn : " + bootstrap.Profiles.Current.DisplayName + " / " + character.Id);
            error = string.Empty;
            return true;
        }

        private static void DisableVisualColliders(GameObject visual)
        {
            var colliders = visual.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private void PublishWarning(string message)
        {
            var bootstrap = RoadRageBootstrap.Instance;
            if (bootstrap != null && bootstrap.Notices != null)
            {
                bootstrap.Notices.Publish(new UserNotice(UserNoticeSeverity.Warning, message));
            }
        }
    }
}
