using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sonde de lecture de la Story 5.18 : relit l'authoring des approches de jonction tel que le graphe
/// l'indexe dans <c>MVP_Run</c>, et mesure la charge de colliders que l'arbitrage doit traverser.
///
/// La mesure dimensionne les tampons NonAlloc de la collecte de revendications : un tampon sature
/// n'est pas une erreur visible, c'est un revendiquant manque -- donc deux vehicules qui entrent.
/// </summary>
public static class Story518JunctionProbe
{
    private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

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

            if (graph == null)
            {
                return "LaneGraph introuvable dans MVP_Run.";
            }

            graph.Rebuild();
            report.AppendLine("Noeuds=" + graph.NodeCount
                + " approches=" + graph.JunctionApproaches.Count
                + " jonctions=" + CountKeys(graph)
                + " contradictoires=" + graph.ContradictoryJunctions.Count);
            report.AppendLine("TrafficSettings=" + (graph.TrafficSettings != null));
            if (graph.TrafficSettings != null)
            {
                report.AppendLine("Plans de feux=" + graph.TrafficSettings.SignalPlans.Count);
            }

            var bufferBig = new Collider[128];
            foreach (var approach in graph.JunctionApproaches)
            {
                var approachPosition = graph.GetNodePosition(approach.NodeIndex);
                var exitKnown = graph.TryGetJunctionExitProbe(approach.NodeIndex, out var exitPoint);
                report.AppendLine("  noeud " + approach.NodeIndex + " cle=" + approach.JunctionKey
                    + " id=\"" + approach.JunctionId + "\" regle=" + approach.Rule
                    + " groupe=" + approach.SignalGroup
                    + " cap=" + approach.Forward.ToString("0.00")
                    + " sonde=" + approach.ExitProbeNode + (exitKnown ? "" : " (inconnue)")
                    + " dist=" + Vector3.ProjectOnPlane(approachPosition - graph.transform.position, Vector3.up).magnitude.ToString("0.0"));

                if (exitKnown)
                {
                    report.AppendLine("      sortie " + exitPoint.ToString("0.0")
                        + " -> sphere 6 m : " + Physics.OverlapSphereNonAlloc(exitPoint, 6f, bufferBig, ~0, QueryTriggerInteraction.Ignore));
                }

                report.AppendLine("      noeud -> sphere 12 m : " + Physics.OverlapSphereNonAlloc(approachPosition, 12f, bufferBig, ~0, QueryTriggerInteraction.Ignore)
                    + " ; sphere 24 m : " + Physics.OverlapSphereNonAlloc(approachPosition, 24f, bufferBig, ~0, QueryTriggerInteraction.Ignore));
            }

            return report.ToString();
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static int CountKeys(LaneGraph graph)
    {
        var keys = new System.Collections.Generic.HashSet<int>();
        foreach (var approach in graph.JunctionApproaches)
        {
            keys.Add(approach.JunctionKey);
        }

        return keys.Count;
    }
}
