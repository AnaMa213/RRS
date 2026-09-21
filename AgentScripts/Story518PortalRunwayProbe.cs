using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// De combien de metres un vehicule qui apparait a un portail dispose-t-il AVANT sa premiere
/// decision ? Retour terrain : "creer un troncon de route entre le rond-point et les tunnels,
/// afin de laisser les voitures qui spawn prendre les decisions et ne pas surprendre les voitures
/// dans les ronds points".
///
/// Mesure : distance le long du graphe entre le noeud de portail et le premier noeud d'anneau,
/// et entre le portail et la premiere approche authoree.
/// </summary>
public static class Story518PortalRunwayProbe
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

            report.AppendLine("portails d'entree : " + graph.EntryPortals.Count);
            report.AppendLine();

            foreach (var portal in graph.EntryPortals)
            {
                var ringDistance = WalkTo(graph, portal, n => graph.RingIdOf(n) != 0, out var ringNode, out var ringHops);
                var junctionDistance = WalkTo(graph, portal,
                    n => n != portal && graph.TryGetJunctionApproach(n, out var a) && a.JunctionKey > 0,
                    out var junctionNode, out var junctionHops);

                report.AppendLine("portail " + portal + " @" + Fmt(graph.GetNodePosition(portal)));
                report.AppendLine("   premier noeud d'ANNEAU     : "
                    + (ringNode < 0 ? "aucun" : ringNode + " a " + ringDistance.ToString("0.00") + " m (" + ringHops + " aretes)"));
                report.AppendLine("   premiere APPROCHE authoree : "
                    + (junctionNode < 0 ? "aucune" : junctionNode + " a " + junctionDistance.ToString("0.00") + " m (" + junctionHops + " aretes)"));
            }

            report.AppendLine();
            report.AppendLine("--- longueur des aretes sortant d'un portail ---");
            foreach (var portal in graph.EntryPortals)
            {
                foreach (var s in graph.GetSuccessors(portal))
                {
                    var d = Vector3.ProjectOnPlane(graph.GetNodePosition(s) - graph.GetNodePosition(portal), Vector3.up).magnitude;
                    report.AppendLine("   " + portal + " -> " + s + " : " + d.ToString("0.00") + " m"
                        + (graph.RingIdOf(s) != 0 ? "  [SUCCESSEUR DEJA SUR L'ANNEAU]" : string.Empty));
                }
            }
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }

        return report.ToString();
    }

    private static string Fmt(Vector3 v) { return "(" + v.x.ToString("0.0") + ";" + v.z.ToString("0.0") + ")"; }

    private static float WalkTo(LaneGraph graph, int start, System.Func<int, bool> stop, out int found, out int hops)
    {
        // Parcours en largeur pondere simple : la plus courte distance authoree jusqu'au predicat.
        var best = new Dictionary<int, float> { { start, 0f } };
        var depth = new Dictionary<int, int> { { start, 0 } };
        var queue = new Queue<int>();
        queue.Enqueue(start);
        found = -1; hops = 0;
        var bestDistance = float.PositiveInfinity;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (best[current] > 400f) continue;
            if (stop(current) && best[current] < bestDistance)
            {
                bestDistance = best[current]; found = current; hops = depth[current];
                continue;
            }
            foreach (var s in graph.GetSuccessors(current))
            {
                var d = best[current] + Vector3.ProjectOnPlane(
                    graph.GetNodePosition(s) - graph.GetNodePosition(current), Vector3.up).magnitude;
                if (best.TryGetValue(s, out var known) && known <= d) continue;
                best[s] = d; depth[s] = depth[current] + 1;
                queue.Enqueue(s);
            }
        }

        return bestDistance;
    }
}
