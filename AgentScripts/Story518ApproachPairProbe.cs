using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Sonde : separation de la paire d'approches que le banc PlayMode 5.18 selectionne.</summary>
public static class Story518ApproachPairProbe
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
            var a = graph.JunctionApproaches;
            report.AppendLine("approches=" + a.Count);
            var bestSep = -1f;
            string bestLabel = "(aucune)";
            for (var i = 0; i < a.Count; i++)
                for (var j = i + 1; j < a.Count; j++)
                {
                    if (a[i].JunctionKey != a[j].JunctionKey) continue;
                    var f = new JunctionClaim(a[i].JunctionKey, a[i].Rule, a[i].Forward, 0f, a[i].NodeIndex, 0UL, true, true, true, false);
                    var s = new JunctionClaim(a[j].JunctionKey, a[j].Rule, a[j].Forward, 0f, a[j].NodeIndex, 1UL, true, true, true, false);
                    var conflicts = JunctionRules.Conflicts(f, s);
                    var sep = Vector3.Distance(graph.GetNodePosition(a[i].NodeIndex), graph.GetNodePosition(a[j].NodeIndex));
                    report.AppendLine("  " + a[i].NodeIndex + "/" + a[j].NodeIndex + " key=" + a[i].JunctionKey
                        + " conflit=" + conflicts + " sep=" + sep.ToString("F2"));
                    if (conflicts && sep > bestSep) { bestSep = sep; bestLabel = a[i].NodeIndex + "/" + a[j].NodeIndex; }
                }
            report.AppendLine("PAIRE RETENUE PAR LE BANC: " + bestLabel + " separation=" + bestSep.ToString("F2")
                + " m (rayon d'approche authore = 12 m)");
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
        return report.ToString();
    }
}
