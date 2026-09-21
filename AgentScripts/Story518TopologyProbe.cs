using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Sonde ANO-5.18-03 : topologie complete autour des jonctions et du giratoire.</summary>
public static class Story518TopologyProbe
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

            var preds = new Dictionary<int, List<int>>();
            for (var n = 0; n < graph.NodeCount; n++)
            {
                foreach (var s in graph.GetSuccessors(n))
                {
                    if (!preds.TryGetValue(s, out var list)) { list = new List<int>(); preds[s] = list; }
                    list.Add(n);
                }
            }

            // Noeuds autour du carrefour central (rayon 20 m).
            report.AppendLine("== NOEUDS a moins de 20 m du carrefour central ==");
            for (var n = 0; n < graph.NodeCount; n++)
            {
                var p = graph.GetNodePosition(n);
                if (p.magnitude > 20f) continue;
                var f = graph.GetNodeRotation(n) * Vector3.forward;
                var sb = new StringBuilder();
                foreach (var s in graph.GetSuccessors(n)) sb.Append(s + " ");
                var pb = new StringBuilder();
                if (preds.TryGetValue(n, out var pl)) foreach (var q in pl) pb.Append(q + " ");
                report.AppendLine("  n=" + n + " pos=(" + p.x.ToString("F1") + "," + p.z.ToString("F1")
                    + ") cap=(" + f.x.ToString("F1") + "," + f.z.ToString("F1") + ")"
                    + " approche=" + graph.TryGetJunctionApproach(n, out _)
                    + " pred=[" + pb.ToString().Trim() + "] succ=[" + sb.ToString().Trim() + "]");
            }

            // Giratoire : ou est-il ?
            report.AppendLine();
            report.AppendLine("== NOEUDS avec plus d'un successeur, hors approches ==");
            for (var n = 0; n < graph.NodeCount; n++)
            {
                if (graph.GetSuccessors(n).Count <= 1) continue;
                if (graph.TryGetJunctionApproach(n, out _)) continue;
                var p = graph.GetNodePosition(n);
                var sb = new StringBuilder();
                foreach (var s in graph.GetSuccessors(n)) sb.Append(s + " ");
                report.AppendLine("  n=" + n + " pos=(" + p.x.ToString("F1") + "," + p.z.ToString("F1") + ") succ=[" + sb.ToString().Trim() + "]");
            }

            // Entraxe et longueur des troncons : pas d'echantillonnage entre noeuds ?
            report.AppendLine();
            report.AppendLine("== LONGUEUR DES ARETES ==");
            var min = float.PositiveInfinity; var max = 0f; var total = 0f; var count = 0;
            for (var n = 0; n < graph.NodeCount; n++)
                foreach (var s in graph.GetSuccessors(n))
                {
                    var d = Vector3.Distance(graph.GetNodePosition(n), graph.GetNodePosition(s));
                    min = Mathf.Min(min, d); max = Mathf.Max(max, d); total += d; count++;
                }
            report.AppendLine("  aretes=" + count + " min=" + min.ToString("F2") + " max=" + max.ToString("F2")
                + " moyenne=" + (total / Mathf.Max(1, count)).ToString("F2"));

            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }
}
