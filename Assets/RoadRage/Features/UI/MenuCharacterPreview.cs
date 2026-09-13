using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoadRage.Features.UI
{
    /// <summary>
    /// Apercu 3D inspectable du menu principal (Story 4.5) : instancie le modele du personnage
    /// selectionne devant une camera dediee qui rend dans un RenderTexture, affiche par le RawImage
    /// porte par ce GameObject.
    /// Presentation locale pure : aucune autorite de gameplay, aucun etat synchronise, aucune lecture
    /// du catalogue (la couche App fournit le prefab et la teinte). Le banc de rendu est cree a
    /// l'execution, a l'ecart de la scene et sur une couche dediee, pour qu'aucune camera de scene ne
    /// le voie et que lui ne voie que le personnage.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuCharacterPreview : MonoBehaviour, IDragHandler
    {
        /// <summary>Couche du modele d'apercu. Autorisee dans le projet, avec repli sur le dernier emplacement libre.</summary>
        public const string PreviewLayerName = "CharacterPreview";

        private const int FallbackPreviewLayer = 31;
        private const int RenderTextureSize = 512;
        private const float RigDistance = 3000f;
        private const float DefaultDistance = 4f;
        private const float FitPadding = 1.15f;
        private const float DegreesPerPixel = 0.35f;
        private const float ModelYaw = 180f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private RawImage target;
        private RenderTexture renderTexture;
        private Camera previewCamera;
        private Transform anchor;
        private GameObject instance;
        private MaterialPropertyBlock tintBlock;

        private void Awake()
        {
            target = GetComponent<RawImage>();
            tintBlock = new MaterialPropertyBlock();

            if (target == null)
            {
                Debug.LogWarning("[UI] MenuCharacterPreview sans RawImage sur son GameObject : l'apercu ne sera pas visible.");
            }

            BuildRig();
        }

        private void OnDestroy()
        {
            Clear();

            if (previewCamera != null && previewCamera.targetTexture == renderTexture)
            {
                previewCamera.targetTexture = null;
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }

            if (target != null)
            {
                target.texture = null;
            }
        }

        /// <summary>
        /// Affiche le modele du personnage selectionne. Un prefab absent laisse l'apercu vide, jamais
        /// un ancien modele : un personnage retire du catalogue ne doit pas rester visible.
        /// </summary>
        public void Show(GameObject previewPrefab, Color previewTint)
        {
            Clear();

            if (previewPrefab == null || anchor == null)
            {
                return;
            }

            anchor.localRotation = Quaternion.identity;
            instance = Instantiate(previewPrefab, anchor, false);
            instance.name = previewPrefab.name + " (apercu menu)";
            ApplyLayer(instance);
            instance.transform.localRotation = Quaternion.Euler(0f, ModelYaw, 0f);
            ApplyTint(instance, previewTint);
            FrameInstance(instance);
        }

        /// <summary>Rotation libre du modele, locale au menu : elle ne touche aucun etat de jeu.</summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (anchor == null || eventData == null)
            {
                return;
            }

            anchor.Rotate(Vector3.up, -eventData.delta.x * DegreesPerPixel, Space.Self);
        }

        private void Clear()
        {
            if (instance != null)
            {
                Destroy(instance);
                instance = null;
            }
        }

        private void BuildRig()
        {
            var layer = ResolvePreviewLayer();

            var rig = new GameObject("MenuCharacterPreviewRig");
            rig.layer = layer;
            rig.transform.position = new Vector3(0f, 0f, RigDistance);

            var anchorObject = new GameObject("Anchor");
            anchorObject.layer = layer;
            anchorObject.transform.SetParent(rig.transform, false);
            anchor = anchorObject.transform;

            // La camera vit sur son propre GameObject. Posee sur le banc, ses localPosition
            // deplacaient le banc entier (ancrage compris) et la camera restait confondue avec le
            // modele : l'apercu ne montrait alors que l'interieur du mesh, jamais le personnage.
            var cameraObject = new GameObject("PreviewCamera");
            cameraObject.layer = layer;
            cameraObject.transform.SetParent(rig.transform, false);

            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            previewCamera.orthographic = false;
            previewCamera.fieldOfView = 30f;
            previewCamera.nearClipPlane = 0.05f;
            previewCamera.farClipPlane = 100f;
            previewCamera.transform.localPosition = new Vector3(0f, 0f, -DefaultDistance);
            // Couche d'apercu plus couche par defaut : sans cette derniere, les lumieres de scene
            // (couche 0) sont eliminees du culling de cette camera et le modele s'afficherait sans
            // eclairage direct.
            previewCamera.cullingMask = (1 << layer) | 1;

            renderTexture = new RenderTexture(RenderTextureSize, RenderTextureSize, 16, RenderTextureFormat.ARGB32);
            renderTexture.name = "MenuCharacterPreview";
            previewCamera.targetTexture = renderTexture;

            if (target != null)
            {
                target.texture = renderTexture;
                // Le RawImage est authored transparent pour ne rien afficher dans l'editeur tant que le
                // banc n'existe pas ; au runtime il doit rester neutre pour ne pas teinter le modele.
                target.color = Color.white;
            }
        }

        private void FrameInstance(GameObject subject)
        {
            var renderers = subject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                previewCamera.transform.localPosition = new Vector3(0f, 0f, -DefaultDistance);
                return;
            }

            var min = renderers[0].bounds.min;
            var max = renderers[0].bounds.max;
            for (var i = 1; i < renderers.Length; i++)
            {
                min = Vector3.Min(min, renderers[i].bounds.min);
                max = Vector3.Max(max, renderers[i].bounds.max);
            }

            var size = max - min;
            var center = (min + max) * 0.5f;
            var localCenter = anchor.InverseTransformPoint(center);
            subject.transform.localPosition -= new Vector3(localCenter.x, localCenter.y, 0f);

            var height = Mathf.Max(size.y, 0.01f);
            var distance = height * 0.5f * FitPadding / Mathf.Tan(previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            previewCamera.transform.localPosition = new Vector3(0f, 0f, -distance);
        }

        private static void ApplyLayer(GameObject subject)
        {
            var layer = ResolvePreviewLayer();
            subject.layer = layer;

            var transforms = subject.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.layer = layer;
            }
        }

        private static void ApplyTint(GameObject subject, Color tint)
        {
            var renderers = subject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            var usesBaseColor = false;
            for (var i = 0; i < renderers.Length; i++)
            {
                var material = renderers[i].sharedMaterial;
                if (material != null && material.HasProperty(BaseColorId))
                {
                    usesBaseColor = true;
                    break;
                }
            }

            var block = new MaterialPropertyBlock();
            block.SetColor(usesBaseColor ? BaseColorId : ColorId, tint);

            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].SetPropertyBlock(block);
            }
        }

        private static int ResolvePreviewLayer()
        {
            var named = LayerMask.NameToLayer(PreviewLayerName);
            return named >= 0 ? named : FallbackPreviewLayer;
        }
    }
}
