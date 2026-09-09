using RoadRage.Shared.Definitions;
using RoadRage.Shared.Domain;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Presentation minimale Story 2.5 d'un joueur reseau : les joueurs distants sont rendus avec
    /// le prefab greybox du personnage, tandis que le joueur local garde son avatar/camera local-only.
    /// La pose reste ecrite par le serveur ; les clients ne soumettent que leur propre pose.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkedPlayerState))]
    public sealed class NetworkedPlayerPresentation : NetworkBehaviour
    {
        [SerializeField]
        private CharacterCatalog characterCatalog;

        private NetworkedPlayerState state;
        private GameObject visualInstance;
        private string renderedCharacterId = string.Empty;

        public GameObject VisualInstance
        {
            get { return visualInstance; }
        }

        public string RenderedCharacterId
        {
            get { return renderedCharacterId; }
        }

        public CharacterCatalog CharacterCatalog
        {
            get { return characterCatalog; }
        }

        private void Awake()
        {
            CacheState();
        }

        public override void OnNetworkSpawn()
        {
            CacheState();

            if (state == null)
            {
                return;
            }

            state.ClientId.OnValueChanged += HandleClientIdChanged;
            state.CharacterId.OnValueChanged += HandleCharacterIdChanged;
            RefreshVisual();
            ApplyRemotePose();
        }

        public override void OnNetworkDespawn()
        {
            if (state != null)
            {
                state.ClientId.OnValueChanged -= HandleClientIdChanged;
                state.CharacterId.OnValueChanged -= HandleCharacterIdChanged;
            }

            ClearVisual();
        }

        private void LateUpdate()
        {
            if (!IsSpawned)
            {
                return;
            }

            if (IsRepresentingLocalClient() || IsVehicleSeatMode())
            {
                ClearVisual();
                return;
            }

            RefreshVisual();
            ApplyRemotePose();
        }

        public bool RepresentsClient(ulong clientId)
        {
            CacheState();
            return state != null && state.ClientId.Value == clientId;
        }

        public void SubmitLocalPose(Vector3 worldPosition, float yawDegrees)
        {
            CacheState();

            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || state == null || state.ClientId.Value != manager.LocalClientId)
            {
                return;
            }

            if (IsServer)
            {
                ApplyServerPose(worldPosition, yawDegrees);
                return;
            }

            SubmitPoseRpc(worldPosition, yawDegrees);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SubmitPoseRpc(Vector3 worldPosition, float yawDegrees, RpcParams rpcParams = default)
        {
            CacheState();

            if (state == null || state.ClientId.Value != rpcParams.Receive.SenderClientId)
            {
                return;
            }

            ApplyServerPose(worldPosition, yawDegrees);
        }

        private void ApplyServerPose(Vector3 worldPosition, float yawDegrees)
        {
            if (!IsServer || state == null)
            {
                return;
            }

            state.WorldPosition.Value = worldPosition;
            state.YawDegrees.Value = NormalizeYaw(yawDegrees);
        }

        private void ApplyRemotePose()
        {
            if (state == null)
            {
                return;
            }

            transform.SetPositionAndRotation(
                state.WorldPosition.Value,
                Quaternion.Euler(0f, state.YawDegrees.Value, 0f));
        }

        private void HandleClientIdChanged(ulong previousValue, ulong newValue)
        {
            RefreshVisual();
        }

        private void HandleCharacterIdChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            CacheState();

            if (state == null)
            {
                return;
            }

            if (IsRepresentingLocalClient() || IsVehicleSeatMode())
            {
                ClearVisual();
                return;
            }

            RefreshVisual(state.CharacterId.Value.ToString());
        }

        private void RefreshVisual(string rawCharacterId)
        {
            var character = ResolveCharacter(rawCharacterId);
            if (character == null || character.PreviewPrefab == null)
            {
                ClearVisual();
                renderedCharacterId = string.Empty;
                return;
            }

            if (visualInstance != null && renderedCharacterId == character.RawId)
            {
                return;
            }

            ClearVisual();

            visualInstance = Instantiate(character.PreviewPrefab, transform);
            visualInstance.name = character.PreviewPrefab.name + "_NetworkVisual";
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.transform.localScale = Vector3.one;
            DisableVisualColliders(visualInstance);
            renderedCharacterId = character.RawId;
        }

        private CharacterDef ResolveCharacter(string rawCharacterId)
        {
            if (characterCatalog == null)
            {
                return null;
            }

            CharacterDef character;
            if (!string.IsNullOrWhiteSpace(rawCharacterId)
                && characterCatalog.TryGetById(new DefinitionId(rawCharacterId), out character)
                && character != null)
            {
                return character;
            }

            return characterCatalog.GetAt(0);
        }

        private bool IsRepresentingLocalClient()
        {
            var manager = NetworkManager.Singleton;
            return manager != null && manager.IsClient && state != null && state.ClientId.Value == manager.LocalClientId;
        }

        private bool IsVehicleSeatMode()
        {
            return state != null && (state.Mode.Value == PlayerMode.Driver || state.Mode.Value == PlayerMode.Passenger);
        }

        private void ClearVisual()
        {
            if (visualInstance == null)
            {
                return;
            }

            Destroy(visualInstance);
            visualInstance = null;
            renderedCharacterId = string.Empty;
        }

        private void CacheState()
        {
            if (state == null)
            {
                state = GetComponent<NetworkedPlayerState>();
            }
        }

        private static float NormalizeYaw(float yawDegrees)
        {
            var normalized = yawDegrees % 360f;
            return normalized < 0f ? normalized + 360f : normalized;
        }

        private static void DisableVisualColliders(GameObject visual)
        {
            var colliders = visual.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }
    }
}
