using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Qui a la priorite a l'entree d'un giratoire, et la regle est-elle seulement authoree ?
///
/// Pour chaque anneau : ses noeuds, leurs regles d'approche s'il y en a, et les noeuds EXTERIEURS
/// qui s'y deversent. C'est la donnee qui dit si la priorite a l'anneau existe comme regle, ou si
/// elle est seulement esperee de l'ordre d'arrivee.
/// </summary>
public static class Story518RingPriorityProbe
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
            graph.Rebuild();

            var rings = FindRings(graph);
            report.AppendLine("anneaux detectes : " + rings.Count);

            var onRing = new HashSet<int>();
            foreach (var ring in rings) foreach (var n in ring) onRing.Add(n);

            for (var r = 0; r < rings.Count; r++)
            {
                var ring = rings[r];
                report.AppendLine();
                report.AppendLine("=== anneau #" + (r + 1) + " : noeuds " + string.Join(",", ring));

                var approaches = 0;
                foreach (var n in ring)
                {
                    if (graph.TryGetJunctionApproach(n, out var info) && info.JunctionKey > 0)
                    {
                        approaches++;
                        report.AppendLine("   noeud " + n + " EST une approche authoree : regle=" + info.Rule
                            + " jonction=" + info.JunctionKey);
                    }
                }
                if (approaches == 0) report.AppendLine("   AUCUN noeud de l'anneau ne porte de regle d'approche.");

                // Entrees : un noeud HORS anneau dont un successeur est SUR l'anneau.
                var entries = new List<string>();
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    if (onRing.Contains(i)) continue;
                    foreach (var s in graph.GetSuccessors(i))
                    {
                        if (!ring.Contains(s)) continue;
                        var rule = graph.TryGetJunctionApproach(i, out var info) && info.JunctionKey > 0
                            ? info.Rule.ToString() + " (jonction " + info.JunctionKey + ")"
                            : "AUCUNE REGLE";
                        entries.Add(i + "->" + s + " : " + rule);
                    }
                }

                report.AppendLine("   entrees depuis l'exterieur : " + entries.Count);
                foreach (var e in entries) report.AppendLine("      " + e);
            }

            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static List<List<int>> FindRings(LaneGraph graph)
    {
        var rings = new List<List<int>>();
        var seen = new HashSet<int>();
        for (var start = 0; start < graph.NodeCount; start++)
        {
            if (seen.Contains(start)) continue;
            var ring = new List<int>();
            var current = start;
            for (var step = 0; step < 64; step++)
            {
                ring.Add(current);
                var next = Best(graph, current);
                if (next < 0) break;
                if (next == start && ring.Count >= 5)
                {
                    rings.Add(ring);
                    foreach (var n in ring) seen.Add(n);
                    break;
                }
                if (ring.Contains(next)) break;
                current = next;
            }
        }
        return rings;
    }

    private static int Best(LaneGraph graph, int node)
    {
        var f = Vector3.ProjectOnPlane(graph.GetNodeRotation(node) * Vector3.forward, Vector3.up);
        if (f.sqrMagnitude < 0.0001f) return -1;
        f.Normalize();
        var best = -1; var bestDot = -2f;
        foreach (var s in graph.GetSuccessors(node))
        {
            var d = Vector3.ProjectOnPlane(graph.GetNodePosition(s) - graph.GetNodePosition(node), Vector3.up);
            if (d.sqrMagnitude < 0.01f) continue;
            var dot = Vector3.Dot(d.normalized, f);
            if (dot > bestDot) { bestDot = dot; best = s; }
        }
        return best;
    }
}
