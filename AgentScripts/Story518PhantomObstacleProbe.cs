using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sonde ANO-5.18-02 : QUEL objet le couloir de voie retient-il comme obstacle immobile autour des
/// giratoires et de leur branche de tunnel ? Rejoue la meme decision que la perception (trajectoire
/// de voie + degagement a l'emprise) sur la scene reelle, sans Play Mode. Lecture seule.
/// </summary>
public static class Story518PhantomObstacleProbe
{
    public static string Run()
    {
        var report = new StringBuilder();
        var scene = EditorSceneManager.OpenScene("Assets/RoadRage/App/Scenes/MVP_Run.unity", OpenSceneMode.Additive);
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

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            var box = prefab.GetComponent<BoxCollider>();
            var half = box.size * 0.5f;
            const float margin = 0.3f;
            const float corridor = 1.03f + margin;      // demi-largeur + marge authorees
            const float perception = 20f;
            const float reach = 24f;

            report.AppendLine("corridor=" + corridor.ToString("F2") + "m demi-extents=" + half);

            var flagged = new Dictionary<string, int>();
            var closest = new Dictionary<string, float>();

            for (var n = 0; n < graph.NodeCount; n++)
            {
                // Trajectoire prevue depuis ce noeud : meme marche que BuildPathFromLaneGraph.
                var path = new List<Vector3> { graph.GetNodePosition(n) };
                var node = n;
                var heading = graph.GetNodeRotation(n) * Vector3.forward; heading.y = 0f; heading.Normalize();
                var travelled = 0f;
                for (var step = 0; step < 11 && travelled < reach; step++)
                {
                    var from = graph.GetNodePosition(node);
                    var successors = graph.GetSuccessors(node);
                    var best = -1; var bestDot = float.NegativeInfinity;
                    for (var i = 0; i < successors.Count; i++)
                    {
                        var dir = Vector3.ProjectOnPlane(graph.GetNodePosition(successors[i]) - from, Vector3.up);
                        if (dir.sqrMagnitude <= 0.0001f) continue;
                        var dot = Vector3.Dot(dir.normalized, heading);
                        if (dot > bestDot) { bestDot = dot; best = successors[i]; }
                    }
                    if (best < 0) break;
                    var next = graph.GetNodePosition(best);
                    var leg = Vector3.ProjectOnPlane(next - path[path.Count - 1], Vector3.up);
                    if (leg.magnitude < 0.0001f) break;
                    heading = leg.normalized;
                    travelled += leg.magnitude;
                    path.Add(next);
                    node = best;
                }
                if (path.Count < 2) continue;

                var center = path[0] + Vector3.up * half.y;
                var hits = Physics.OverlapSphere(center, perception, ~0, QueryTriggerInteraction.Ignore);
                foreach (var hit in hits)
                {
                    if (hit == null || hit.attachedRigidbody != null) continue;               // statiques seuls
                    if (hit.GetComponentInParent<CharacterController>() != null) continue;
                    var label = hit.name;
                    // Miroir de IsLowSurface APRES correction ANO-5.18-02 : toute surface statique
                    // sous la hauteur franchissable authoree se franchit, quel que soit son nom.
                    const float maxCurbHeight = 0.15f;
                    var roadLevel = path[0].y;
                    if (hit.bounds.max.y <= roadLevel + maxCurbHeight) continue;
                    if (label == "Col_Roadway" || label.StartsWith("Col_Roadway_")
                        || label.StartsWith("Col_Sidewalk_") || label.StartsWith("Col_Curb_")
                        || label == "Greybox_GroundPlane") continue;
                    var b = hit.bounds;
                    if (Mathf.Abs(b.center.y - center.y) > b.extents.y + half.y) continue;    // bande de hauteur

                    Vector3 c, e; Quaternion r;
                    if (hit is BoxCollider bc)
                    {
                        c = bc.transform.TransformPoint(bc.center);
                        var sc = bc.transform.lossyScale;
                        e = Vector3.Scale(bc.size * 0.5f, new Vector3(Mathf.Abs(sc.x), Mathf.Abs(sc.y), Mathf.Abs(sc.z)));
                        r = bc.transform.rotation;
                    }
                    else { c = b.center; e = b.extents; r = Quaternion.identity; }

                    if (!TrafficPerception.TryPathClearance(path, path.Count, c, e, r, out var clearance, out _)) continue;
                    if (clearance > corridor) continue;

                    var key = hit.name;
                    flagged.TryGetValue(key, out var seen);
                    flagged[key] = seen + 1;
                    if (!closest.TryGetValue(key, out var best2) || clearance < best2) closest[key] = clearance;
                }
            }

            if (flagged.Count == 0) report.AppendLine("AUCUN objet statique ne tombe dans le couloir de voie.");
            foreach (var kv in flagged)
                report.AppendLine("  OBSTACLE RETENU: " + kv.Key + " sur " + kv.Value
                    + " noeuds, degagement mini=" + closest[kv.Key].ToString("F2") + "m");
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
        return report.ToString();
    }
}
