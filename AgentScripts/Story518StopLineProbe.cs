using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// OU s'arrete reellement un vehicule a une approche authoree, et de combien son emprise mord-elle
/// sur le couloir balaye par les mouvements des AUTRES approches ?
///
/// La ligne d'arret courante se deduit de <c>ConflictEntryDistance</c>, qui est l'abscisse de la
/// premiere INTERSECTION des axes centraux -- deux polylignes SANS EPAISSEUR. Cette sonde mesure
/// l'ecart entre cette abscisse et celle a laquelle les deux COULOIRS (axes + demi-largeurs +
/// marge) se touchent reellement. C'est cet ecart qui dit si la ligne est trop avancee, et de
/// combien.
/// </summary>
public static class Story518StopLineProbe
{
    private const float HalfWidth = 1.03f;   // demi-largeur mesuree du Greybox_AIVehicle
    private const float FrontOffset = 2.22f; // demi-longueur mesuree
    private const float Margin = 0.30f;      // SafetyMargin authoree

    public static string Run()
    {
        var report = new StringBuilder();
        // La scene deja chargee fait foi quand il y en a une : la sonde doit pouvoir tourner
        // pendant une partie sans la perturber.
        var live = Object.FindAnyObjectByType<LaneGraph>();
        var scene = live != null ? default(UnityEngine.SceneManagement.Scene)
            : EditorSceneManager.OpenScene("Assets/RoadRage/App/Scenes/MVP_Run.unity", OpenSceneMode.Additive);
        try
        {
            var graph = live;
            if (graph == null)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    graph = root.GetComponentInChildren<LaneGraph>(true);
                    if (graph != null) break;
                }
            }
            graph.Rebuild();

            var approaches = new List<JunctionApproachInfo>();
            for (var i = 0; i < graph.NodeCount; i++)
            {
                if (graph.TryGetJunctionApproach(i, out var info) && info.JunctionKey > 0) approaches.Add(info);
            }

            report.AppendLine("approaches authorees : " + approaches.Count);
            report.AppendLine("gabarit : demi-largeur " + HalfWidth.ToString("0.00") + " m, avant "
                + FrontOffset.ToString("0.00") + " m, marge " + Margin.ToString("0.00") + " m");
            report.AppendLine("degagement requis entre deux axes : "
                + (2f * HalfWidth + Margin).ToString("0.00") + " m");
            report.AppendLine();

            var worstIntrusion = 0f;
            var worstLabel = string.Empty;

            foreach (var own in approaches)
            {
                // Ligne d'arret ACTUELLE, telle que TickJunctionRules la calcule.
                var conflictEntry = own.ConflictEntryDistance > 0.01f
                    ? own.ConflictEntryDistance
                    : Vector3.Dot(Vector3.ProjectOnPlane(graph.GetNodePosition(own.NodeIndex) - own.EntryPoint, Vector3.up), own.EntryForward);
                var stopLine = Mathf.Max(0f, conflictEntry - HalfWidth - Margin);

                // Ligne d'arret REQUISE : premiere abscisse ou notre couloir touche celui d'un autre
                // mouvement de la meme jonction.
                var required = ResolveWidenedMeeting(graph, approaches, own, 2f * HalfWidth + Margin);
                var requiredStop = float.IsFinite(required) ? Mathf.Max(0f, required - HalfWidth - Margin) : stopLine;

                // Intrusion : de combien l'AVANT du vehicule arrete a la ligne actuelle depasse la
                // ligne requise.
                var intrusion = stopLine - requiredStop;
                if (intrusion > worstIntrusion)
                {
                    worstIntrusion = intrusion;
                    worstLabel = own.JunctionId + " noeud " + own.NodeIndex;
                }

                report.AppendLine(own.JunctionId + " noeud " + own.NodeIndex + " regle=" + own.Rule);
                report.AppendLine("   rencontre axes  : " + conflictEntry.ToString("0.00")
                    + " m   -> ligne actuelle " + stopLine.ToString("0.00") + " m");
                report.AppendLine("   rencontre couloirs : "
                    + (float.IsFinite(required) ? required.ToString("0.00") + " m" : "aucune")
                    + "   -> ligne requise " + requiredStop.ToString("0.00") + " m");
                report.AppendLine("   INTRUSION de l'avant dans l'aire utile : " + intrusion.ToString("0.00") + " m");
            }

            report.AppendLine();
            report.AppendLine("intrusion maximale : " + worstIntrusion.ToString("0.00") + " m (" + worstLabel + ")");
        }
        finally
        {
            if (live == null) EditorSceneManager.CloseScene(scene, true);
        }

        return report.ToString();
    }

    /// <summary>
    /// Premiere abscisse, le long de l'approche, ou l'axe d'un de NOS mouvements passe a moins de
    /// <paramref name="clearance"/> de l'axe d'un mouvement d'une AUTRE approche de la meme jonction.
    /// </summary>
    private static float ResolveWidenedMeeting(LaneGraph graph, List<JunctionApproachInfo> approaches,
        in JunctionApproachInfo own, float clearance)
    {
        var nearest = float.PositiveInfinity;
        foreach (var ownExit in graph.GetSuccessors(own.NodeIndex))
        {
            var mine = Movement(graph, own, ownExit);
            foreach (var other in approaches)
            {
                if (other.JunctionKey != own.JunctionKey || other.NodeIndex == own.NodeIndex) continue;
                foreach (var otherExit in graph.GetSuccessors(other.NodeIndex))
                {
                    var theirs = Movement(graph, other, otherExit);
                    for (var i = 0; i < mine.Count; i++)
                    {
                        var close = false;
                        for (var j = 0; j < theirs.Count && !close; j++)
                        {
                            close = Vector3.ProjectOnPlane(mine[i] - theirs[j], Vector3.up).magnitude <= clearance;
                        }

                        if (!close) continue;
                        var along = Vector3.Dot(Vector3.ProjectOnPlane(mine[i] - own.EntryPoint, Vector3.up), own.EntryForward);
                        if (along > 0.01f && along < nearest) nearest = along;
                        break;
                    }
                }
            }
        }

        return nearest;
    }

    /// <summary>Le mouvement echantillonne, tel que la conduite le parcourt reellement (Bezier).</summary>
    private static List<Vector3> Movement(LaneGraph graph, in JunctionApproachInfo approach, int exitNode)
    {
        var points = new List<Vector3>();
        var start = approach.EntryPoint;
        var end = graph.GetNodePosition(exitNode);
        var exitForward = graph.GetNodeRotation(exitNode) * Vector3.forward;
        exitForward = Vector3.ProjectOnPlane(exitForward, Vector3.up).normalized;
        for (var i = 0; i <= 40; i++)
        {
            points.Add(LaneGraphRouting.ResolveJunctionTurnPoint(start, approach.EntryForward, end, exitForward, i / 40f));
        }

        return points;
    }
}
