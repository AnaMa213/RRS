using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sonde de diagnostic du retour terrain Story 5.18 : mesure la GEOMETRIE que le modele de decision
/// consomme, avant toute modification. Lecture seule, aucune scene sauvegardee.
///
/// Trois mesures, une par faux positif rapporte :
/// 1. separation laterale de deux voies opposees vs bande laterale utilisee par la perception ;
/// 2. ecart entre l'extrapolation RECTILIGNE d'un noeud d'anneau de giratoire et le trace reel ;
/// 3. colliders remontes autour d'un noeud d'anneau (ilot central compris).
/// </summary>
public static class Story518TrafficDiagnosisProbe
{
    private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
    private const string VehiclePrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab";

    public static string Run()
    {
        var report = new StringBuilder();
        var scene = EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
        try
        {
            LaneGraph graph = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                graph = root.GetComponentInChildren<LaneGraph>(true);
                if (graph != null) break;
            }
            if (graph == null) return "LaneGraph introuvable.";
            graph.Rebuild();
            report.AppendLine("Noeuds=" + graph.NodeCount);

            // --- 1. voies opposees -------------------------------------------------
            var best = float.PositiveInfinity;
            var bestA = -1; var bestB = -1;
            for (var i = 0; i < graph.NodeCount; i++)
            {
                var pi = graph.GetNodePosition(i);
                var fi = graph.GetNodeRotation(i) * Vector3.forward; fi.y = 0f; fi.Normalize();
                for (var j = i + 1; j < graph.NodeCount; j++)
                {
                    var fj = graph.GetNodeRotation(j) * Vector3.forward; fj.y = 0f; fj.Normalize();
                    if (Vector3.Dot(fi, fj) > -0.95f) continue; // caps opposes seulement
                    var d = Vector3.ProjectOnPlane(graph.GetNodePosition(j) - pi, Vector3.up).magnitude;
                    if (d < best) { best = d; bestA = i; bestB = j; }
                }
            }
            report.AppendLine("Voies opposees les plus proches: noeuds " + bestA + "/" + bestB
                + " separation=" + best.ToString("F2") + " m");

            // --- 2. extrapolation rectiligne sur un anneau de giratoire ------------
            foreach (var root in scene.GetRootGameObjects())
            {
                var t = FindDeep(root.transform, "Roundabout_");
                if (t == null) continue;
                report.AppendLine("Giratoire: " + t.name + " origine=" + t.position);
                var ring = new List<int>();
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    var d = Vector3.ProjectOnPlane(graph.GetNodePosition(i) - t.position, Vector3.up).magnitude;
                    if (d < 8f) ring.Add(i);
                }
                report.AppendLine("  noeuds a moins de 8 m du centre: " + ring.Count);
                foreach (var n in ring)
                {
                    var p = graph.GetNodePosition(n);
                    var f = graph.GetNodeRotation(n) * Vector3.forward; f.y = 0f; f.Normalize();
                    // extrapolation rectiligne a 8 m/s sur l'horizon de prediction (3 s) = 24 m
                    var end = p + f * 24f;
                    var toCenter = Vector3.ProjectOnPlane(t.position - p, Vector3.up);
                    var lateralAtCenter = Vector3.Cross(Vector3.up, f);
                    report.AppendLine("  n" + n + " d(centre)=" + toCenter.magnitude.ToString("F2")
                        + " ecart lateral du centre a la tangente=" + Mathf.Abs(Vector3.Dot(toCenter, lateralAtCenter)).ToString("F2")
                        + " avance vers le centre=" + Vector3.Dot(toCenter, f).ToString("F2"));
                    // colliders traverses par le rayon tangent
                    var hits = Physics.RaycastAll(p + Vector3.up * 0.5f, f, 24f);
                    var names = new StringBuilder();
                    foreach (var h in hits) names.Append(h.collider.name).Append('@').Append(h.distance.ToString("F1")).Append(' ');
                    report.AppendLine("    tangente 24 m traverse: " + (names.Length == 0 ? "(rien)" : names.ToString()));
                }
                break;
            }

            // --- 3. gabarit vehicule ------------------------------------------------
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VehiclePrefabPath);
            if (prefab != null)
            {
                var box = prefab.GetComponent<BoxCollider>();
                if (box != null)
                    report.AppendLine("Vehicule_AI box size=" + box.size + " demi-largeur=" + (box.size.x * 0.5f).ToString("F2"));
            }
            else report.AppendLine("Prefab vehicule introuvable a " + VehiclePrefabPath);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
        return report.ToString();
    }

    private static Transform FindDeep(Transform root, string prefix)
    {
        if (root.name.StartsWith(prefix)) return root;
        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindDeep(root.GetChild(i), prefix);
            if (found != null) return found;
        }
        return null;
    }
}
