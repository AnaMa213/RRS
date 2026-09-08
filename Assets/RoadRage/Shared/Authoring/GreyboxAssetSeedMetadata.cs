using System;
using UnityEngine;

namespace RoadRage.Shared.Authoring
{
    /// <summary>
    /// Metadata auteur posee sur les prefabs greybox pour garder la source, l'echelle, le plan collider
    /// et la politique de remplacement testables avec le prefab lui-meme.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GreyboxAssetSeedMetadata : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Id stable minuscule, identique a la definition ou au role de gameplay futur.")]
        private string stableId = string.Empty;

        [SerializeField]
        private string sourceAssetPath = string.Empty;

        [SerializeField]
        private string exportAssetPath = string.Empty;

        [SerializeField]
        private string scaleCheck = string.Empty;

        [SerializeField]
        private string colliderPlan = string.Empty;

        [SerializeField]
        private string replacementPolicy = string.Empty;

        [SerializeField]
        private string visualReadability = string.Empty;

        public string StableId
        {
            get { return stableId ?? string.Empty; }
        }

        public string SourceAssetPath
        {
            get { return sourceAssetPath ?? string.Empty; }
        }

        public string ExportAssetPath
        {
            get { return exportAssetPath ?? string.Empty; }
        }

        public string ScaleCheck
        {
            get { return scaleCheck ?? string.Empty; }
        }

        public string ColliderPlan
        {
            get { return colliderPlan ?? string.Empty; }
        }

        public string ReplacementPolicy
        {
            get { return replacementPolicy ?? string.Empty; }
        }

        public string VisualReadability
        {
            get { return visualReadability ?? string.Empty; }
        }

        public bool TryValidate(out string error)
        {
            if (string.IsNullOrWhiteSpace(StableId))
            {
                error = "StableId requis.";
                return false;
            }

            if (StableId != StableId.Trim())
            {
                error = "StableId borde d'espaces : " + StableId + ".";
                return false;
            }

            if (StableId != StableId.ToLowerInvariant())
            {
                error = "StableId doit etre minuscule : " + StableId + ".";
                return false;
            }

            if (!HasExpectedPath(SourceAssetPath, "Assets/RoadRage/ArtSource/Blender/", ".blend"))
            {
                error = "SourceAssetPath doit pointer vers un .blend sous Assets/RoadRage/ArtSource/Blender/.";
                return false;
            }

            if (!ExportAssetPath.StartsWith("Assets/RoadRage/ArtExports/", StringComparison.Ordinal)
                || (!ExportAssetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)
                    && !ExportAssetPath.EndsWith(".glb", StringComparison.OrdinalIgnoreCase)))
            {
                error = "ExportAssetPath doit pointer vers un .fbx ou .glb sous Assets/RoadRage/ArtExports/.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ScaleCheck))
            {
                error = "ScaleCheck requis.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ColliderPlan) || ColliderPlan.IndexOf("Collider", StringComparison.OrdinalIgnoreCase) < 0)
            {
                error = "ColliderPlan doit decrire le collider authoring separe.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ReplacementPolicy))
            {
                error = "ReplacementPolicy requis.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(VisualReadability))
            {
                error = "VisualReadability requis.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool HasExpectedPath(string path, string prefix, string extension)
        {
            return !string.IsNullOrWhiteSpace(path)
                && path.StartsWith(prefix, StringComparison.Ordinal)
                && path.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
