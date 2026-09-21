using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sonde ANO-5.18-04, questions 4 a 10 : topologie du giratoire nord-est.
///
/// Elle imprime, pour chaque giratoire du district, ses noeuds d'anneau dans l'ordre, leurs
/// successeurs, la longueur de chaque arete, le rayon de l'anneau, les branches d'entree et de
/// sortie, et l'eventuelle presence d'une approche de jonction authoree.
/// </summary>
public static class Story518RoundaboutTopologyProbe
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

            // Les noeuds d'anneau se reconnaissent a leur GameObject d'authoring.
            var authored = new Dictionary<int, string>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var laneNode in root.GetComponentsInChildren<LaneNode>(true))
                {
                    var index = IndexOf(graph, laneNode.transform.position);
                    if (index < 0) continue;
                    var path = laneNode.name;
                    var parent = laneNode.transform.parent;
                    for (var depth = 0; parent != null && depth < 3; depth++)
                    {
                        path = parent.name + "/" + path;
                        parent = parent.parent;
                    }
                    authored[index] = path;
                }
            }

            var approaches = new Dictionary<int, JunctionApproachInfo>();
            foreach (var a in graph.JunctionApproaches) approaches[a.NodeIndex] = a;

            for (var i = 0; i < graph.NodeCount; i++)
            {
                if (!authored.TryGetValue(i, out var path) || !path.Contains("Roundabout")) continue;
                var position = graph.GetNodePosition(i);
                var forward = Vector3.ProjectOnPlane(graph.GetNodeRotation(i) * Vector3.forward, Vector3.up).normalized;
                var line = "n=" + i + " " + F(position) + " cap=" + F(forward) + "  " + path;
                if (approaches.ContainsKey(i)) line += "  [APPROCHE " + approaches[i].JunctionKey + "]";
                report.AppendLine(line);
                foreach (var s in graph.GetSuccessors(i))
                {
                    var target = graph.GetNodePosition(s);
                    var targetForward = Vector3.ProjectOnPlane(graph.GetNodeRotation(s) * Vector3.forward, Vector3.up).normalized;
                    report.AppendLine("      -> " + s + " " + F(target)
                        + " d=" + Vector3.ProjectOnPlane(target - position, Vector3.up).magnitude.ToString("F2") + " m"
                        + " virage=" + Vector3.SignedAngle(forward, targetForward, Vector3.up).ToString("F0") + " deg"
                        + "  " + (authored.TryGetValue(s, out var targetPath) ? targetPath : "(hors giratoire)"));
                }
            }

            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static int IndexOf(LaneGraph graph, Vector3 position)
    {
        for (var i = 0; i < graph.NodeCount; i++)
        {
            if (Vector3.SqrMagnitude(graph.GetNodePosition(i) - position) < 0.0001f) return i;
        }
        return -1;
    }

    private static string F(Vector3 v)
    {
        return "(" + v.x.ToString("F2") + "," + v.z.ToString("F2") + ")";
    }
}
