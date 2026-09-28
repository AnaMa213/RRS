#if UNITY_EDITOR
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Adaptateur Editor de la classification V1 authoree (AD-33 : le NavMesh reste une donnee V1,
    /// jamais une source Traffic V2). Lit les NavMeshModifier de la scene et livre a
    /// <see cref="JunctionClearance"/> les surfaces Sidewalk sous forme neutre : colliders declares,
    /// actifs ou non, et signature des champs authores pour l'empreinte semantique (Story 5.51).
    /// </summary>
    public static class SidewalkDeclarations
    {
        /// <summary>Resolution par nom d'aire, jamais par index code en dur.</summary>
        public const string AreaName = "Sidewalk";

        // Lu par nom de type : aucun assemblage du projet ne reference Unity.AI.Navigation.
        private const string ModifierType = "Unity.AI.Navigation.NavMeshModifier";

        public static List<JunctionClearanceSurface> Read(Scene scene, List<string> failures)
        {
            var surfaces = new List<JunctionClearanceSurface>();
            int area = NavMesh.GetAreaFromName(AreaName);
            if (area < 0)
            {
                failures.Add("Aire NavMesh '" + AreaName + "' absente.");
                return surfaces;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (component == null || component.GetType().FullName != ModifierType) continue;
                    var authoring = new SerializedObject(component);
                    if (!authoring.FindProperty("m_OverrideArea").boolValue || authoring.FindProperty("m_Area").intValue != area) continue;

                    bool children = authoring.FindProperty("m_ApplyToChildren").boolValue;
                    var surface = new JunctionClearanceSurface
                    {
                        Name = Path(component.transform),
                        Declaration = component,
                        DeclarationSignature = AreaName + "=" + area
                            + "|ignoreFromBuild=" + authoring.FindProperty("m_IgnoreFromBuild").boolValue
                            + "|applyToChildren=" + children
                            + "|enabled=" + component.enabled
                    };
                    surface.Colliders.AddRange(children ? component.GetComponentsInChildren<Collider>(true) : component.GetComponents<Collider>());
                    surfaces.Add(surface);
                }
            }

            return surfaces;
        }

        private static string Path(Transform transform)
        {
            string path = transform.name;
            for (Transform t = transform.parent; t != null; t = t.parent) path = t.name + "/" + path;
            return path;
        }
    }
}
#endif
