using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Chaine exacte portail -> anneau : quels noeuds, a quelle distance, et ou se trouve le centre de
/// l'anneau. Mesure demandee par le retour terrain "creer un troncon de route entre le rond-point
/// et les tunnels" : elle dit s'il existe de la place pour l'allonger.
/// </summary>
public static class Story518PortalChainProbe
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

            // Emprise du graphe : la place disponible vers l'exterieur se lit dessus.
            var min = new Vector3(float.MaxValue, 0f, float.MaxValue);
            var max = new Vector3(float.MinValue, 0f, float.MinValue);
            for (var i = 0; i < graph.NodeCount; i++)
            {
                var p = graph.GetNodePosition(i);
                min.x = Mathf.Min(min.x, p.x); min.z = Mathf.Min(min.z, p.z);
                max.x = Mathf.Max(max.x, p.x); max.z = Mathf.Max(max.z, p.z);
            }
            report.AppendLine("emprise du graphe : x [" + min.x.ToString("0.0") + " ; " + max.x.ToString("0.0")
                + "]  z [" + min.z.ToString("0.0") + " ; " + max.z.ToString("0.0") + "]");
            report.AppendLine();

            foreach (var portal in graph.EntryPortals)
            {
                report.AppendLine("=== portail " + portal + " @" + Fmt(graph.GetNodePosition(portal)));
                var current = portal;
                var travelled = 0f;
                for (var step = 0; step < 8; step++)
                {
                    var successors = graph.GetSuccessors(current);
                    if (successors.Count == 0) break;
                    var next = successors[0];
                    var d = Vector3.ProjectOnPlane(
                        graph.GetNodePosition(next) - graph.GetNodePosition(current), Vector3.up).magnitude;
                    travelled += d;
                    var ring = graph.RingIdOf(next);
                    report.AppendLine("   -> " + next + " @" + Fmt(graph.GetNodePosition(next))
                        + "  +" + d.ToString("0.00") + " m  (cumul " + travelled.ToString("0.00") + " m)"
                        + "  successeurs=" + successors.Count
                        + (ring != 0 ? "  [ANNEAU " + ring + "]" : string.Empty));
                    if (ring != 0) break;
                    current = next;
                }
            }

            // Centre et rayon de chaque anneau.
            report.AppendLine();
            for (var ring = 1; ring <= 8; ring++)
            {
                var sum = Vector3.zero;
                var count = 0;
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    if (graph.RingIdOf(i) != ring) continue;
                    sum += graph.GetNodePosition(i); count++;
                }
                if (count == 0) continue;
                var centre = sum / count;
                var radius = 0f;
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    if (graph.RingIdOf(i) != ring) continue;
                    radius += Vector3.ProjectOnPlane(graph.GetNodePosition(i) - centre, Vector3.up).magnitude;
                }
                report.AppendLine("anneau " + ring + " : " + count + " noeuds, centre " + Fmt(centre)
                    + ", rayon moyen " + (radius / count).ToString("0.00") + " m");
            }
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }

        return report.ToString();
    }

    private static string Fmt(Vector3 v) { return "(" + v.x.ToString("0.0") + ";" + v.z.ToString("0.0") + ")"; }
}
