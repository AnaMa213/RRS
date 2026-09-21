using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Sonde ANO-5.18-03 : frontiere d'entree derivee et forme du connecteur de virage.</summary>
public static class Story518BoundaryProbe
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

            // Gabarit mesure du vehicule IA (demi-extents) pour la ligne d'arret.
            var halfWidth = 1.03f;
            var margin = 0.30f;
            var frontOffset = 2.22f;

            foreach (var a in graph.JunctionApproaches)
            {
                var node = graph.GetNodePosition(a.NodeIndex);
                report.AppendLine("approche n=" + a.NodeIndex + " jonction=" + a.JunctionKey
                    + " frontiere n=" + a.EntryNode + " " + F(a.EntryPoint)
                    + " cap=" + F(a.EntryForward)
                    + " conflit_a=" + a.ConflictEntryDistance.ToString("F2") + " m"
                    + " (noeud de decision a " + Vector3.Distance(a.EntryPoint, node).ToString("F2") + " m)");

                var stopAlong = Mathf.Max(0f, a.ConflictEntryDistance - halfWidth - margin);
                var frontStop = a.EntryPoint + a.EntryForward * stopAlong;
                report.AppendLine("     ligne d'arret (AVANT du vehicule) = " + F(frontStop)
                    + " -> centre a " + F(frontStop - a.EntryForward * frontOffset));

                foreach (var s in graph.GetSuccessors(a.NodeIndex))
                {
                    var exit = graph.GetNodePosition(s);
                    var exitFwd = Vector3.ProjectOnPlane(graph.GetNodeRotation(s) * Vector3.forward, Vector3.up).normalized;
                    var hasCorner = LaneGraphRouting.TryResolveLaneAxisCorner(a.EntryPoint, a.EntryForward, exit, exitFwd, out var corner);
                    var mid = LaneGraphRouting.ResolveJunctionTurnPoint(a.EntryPoint, a.EntryForward, exit, exitFwd, 0.5f);
                    // Recul maximal : le connecteur repart-il jamais en arriere ?
                    var worstBack = 0f;
                    var previous = -1f;
                    for (var t = 0; t <= 20; t++)
                    {
                        var pt = LaneGraphRouting.ResolveJunctionTurnPoint(a.EntryPoint, a.EntryForward, exit, exitFwd, t / 20f);
                        var along = Vector3.Dot(Vector3.ProjectOnPlane(pt - a.EntryPoint, Vector3.up), a.EntryForward);
                        if (previous >= 0f && along < previous) worstBack = Mathf.Max(worstBack, previous - along);
                        previous = along;
                    }
                    report.AppendLine("     -> sortie " + s + " " + F(exit)
                        + " coin=" + (hasCorner ? F(corner) : "(aucun, tout droit)")
                        + " milieu=" + F(mid) + " recul_max=" + worstBack.ToString("F2") + " m");
                }
            }

            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static string F(Vector3 v)
    {
        return "(" + v.x.ToString("F2") + "," + v.z.ToString("F2") + ")";
    }
}
