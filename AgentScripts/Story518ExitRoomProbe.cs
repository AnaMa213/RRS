using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sonde ANO-5.18-01 : que contient REELLEMENT la sphere de degagement de sortie (6 m authores)
/// autour de chaque noeud de sortie de jonction ? Lecture seule.
/// </summary>
public static class Story518ExitRoomProbe
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

            const float radius = 6f;   // profile.JunctionExitClearanceRadius authore
            var approaches = graph.JunctionApproaches;
            report.AppendLine("Rayon de degagement authore = " + radius + " m");
            report.AppendLine("approches=" + approaches.Count);

            for (var i = 0; i < approaches.Count; i++)
            {
                var a = approaches[i];
                if (!graph.TryGetJunctionExitProbe(a.NodeIndex, out var exit)) { report.AppendLine("  approche " + a.NodeIndex + " : AUCUNE sonde de sortie -> lue SATUREE d'office"); continue; }

                // Quels NOEUDS DE VOIE tombent dans la sphere, et quel est leur cap vs celui de la sortie ?
                var inside = new StringBuilder();
                var count = 0;
                for (var n = 0; n < graph.NodeCount; n++)
                {
                    var d = Vector3.ProjectOnPlane(graph.GetNodePosition(n) - exit, Vector3.up).magnitude;
                    if (d > radius) continue;
                    var f = graph.GetNodeRotation(n) * Vector3.forward; f.y = 0f; f.Normalize();
                    var dot = Vector3.Dot(f, a.Forward);
                    var sens = dot > 0.5f ? "meme sens" : dot < -0.5f ? "SENS OPPOSE" : "transversal";
                    inside.Append("n").Append(n).Append("(").Append(d.ToString("F2")).Append("m,").Append(sens).Append(") ");
                    count++;
                }
                report.AppendLine("  approche " + a.NodeIndex + " regle=" + a.Rule + " -> sortie a " + exit.ToString("F1")
                    + " : " + count + " noeuds de voie dans la sphere");
                report.AppendLine("      " + inside);
            }
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
        return report.ToString();
    }
}
