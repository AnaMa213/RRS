using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sonde ANO-5.18-03 : geometrie reelle des jonctions du district. Elle ne corrige rien, elle
/// mesure -- position des noeuds d'approche par rapport a l'aire de conflit, forme de l'arc de
/// virage effectivement suivi, et empietement de cet arc sur les voies voisines.
/// </summary>
public static class Story518JunctionGeometryProbe
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

            var approaches = graph.JunctionApproaches;
            report.AppendLine("noeuds=" + graph.NodeCount + " approches=" + approaches.Count);

            // Regroupement par cle de jonction.
            var keys = new List<int>();
            foreach (var a in approaches) if (!keys.Contains(a.JunctionKey)) keys.Add(a.JunctionKey);

            foreach (var key in keys)
            {
                var members = new List<JunctionApproachInfo>();
                foreach (var a in approaches) if (a.JunctionKey == key) members.Add(a);

                var centre = Vector3.zero;
                foreach (var m in members) centre += graph.GetNodePosition(m.NodeIndex);
                centre /= Mathf.Max(1, members.Count);

                report.AppendLine();
                report.AppendLine("== JONCTION cle=" + key + " id=" + members[0].JunctionId
                    + " approches=" + members.Count + " centre=" + F(centre));

                foreach (var m in members)
                {
                    var p = graph.GetNodePosition(m.NodeIndex);
                    var fwd = m.Forward;
                    var toCentre = Vector3.ProjectOnPlane(centre - p, Vector3.up);
                    var ahead = Vector3.Dot(toCentre, fwd);
                    report.AppendLine("  approche noeud=" + m.NodeIndex + " pos=" + F(p)
                        + " cap=" + F(fwd) + " regle=" + m.Rule + " groupe=" + m.SignalGroup
                        + " dist_centre=" + toCentre.magnitude.ToString("F2")
                        + " (centre " + (ahead >= 0f ? "DEVANT" : "DERRIERE") + " a " + ahead.ToString("F2") + " m)");

                    var succ = graph.GetSuccessors(m.NodeIndex);
                    for (var s = 0; s < succ.Count; s++)
                    {
                        var ex = graph.GetNodePosition(succ[s]);
                        var exFwd = Vector3.ProjectOnPlane(graph.GetNodeRotation(succ[s]) * Vector3.forward, Vector3.up).normalized;
                        var chord = Vector3.ProjectOnPlane(ex - p, Vector3.up);
                        var angle = Vector3.SignedAngle(fwd, exFwd, Vector3.up);

                        // Arc effectivement vise par le controleur.
                        var maxOffChord = 0f;
                        var minDistToAnyNode = float.PositiveInfinity;
                        var worstNode = -1;
                        Vector3 worstPoint = Vector3.zero;
                        for (var t = 0; t <= 20; t++)
                        {
                            var pt = LaneGraphRouting.ResolveJunctionTurnPoint(p, fwd, ex, exFwd, t / 20f);
                            var off = Vector3.Distance(pt, ClosestOnSegment(p, ex, pt));
                            if (off > maxOffChord) { maxOffChord = off; worstPoint = pt; }
                        }
                        // Voie etrangere la plus proche du sommet de l'arc.
                        for (var n = 0; n < graph.NodeCount; n++)
                        {
                            if (n == m.NodeIndex || n == succ[s]) continue;
                            var d = Vector3.ProjectOnPlane(graph.GetNodePosition(n) - worstPoint, Vector3.up).magnitude;
                            if (d < minDistToAnyNode) { minDistToAnyNode = d; worstNode = n; }
                        }

                        report.AppendLine("     -> sortie=" + succ[s] + " pos=" + F(ex)
                            + " corde=" + chord.magnitude.ToString("F2")
                            + " angle=" + angle.ToString("F0") + " deg"
                            + " fleche_arc=" + maxOffChord.ToString("F2")
                            + " sommet=" + F(worstPoint)
                            + " noeud_le_plus_proche_du_sommet=" + worstNode
                            + " a " + minDistToAnyNode.ToString("F2") + " m");
                    }
                }

                // Surface roulable sous le centre de la jonction : d'ou sort l'aire de conflit.
                var hits = Physics.OverlapSphere(centre + Vector3.up * 0.5f, 14f, ~0, QueryTriggerInteraction.Ignore);
                var seen = new List<string>();
                foreach (var h in hits)
                {
                    if (h == null || h.attachedRigidbody != null) continue;
                    var b = h.bounds;
                    if (b.size.y > 1f) continue; // on ne veut que le sol
                    var label = h.name + " taille=" + F(b.size) + " centre=" + F(b.center);
                    if (!seen.Contains(label)) seen.Add(label);
                }
                report.AppendLine("  sols proches du centre (" + seen.Count + ") :");
                foreach (var l in seen) report.AppendLine("     " + l);
            }

            return report.ToString();
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static Vector3 ClosestOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        var ab = b - a;
        var len = ab.sqrMagnitude;
        if (len < 0.0001f) return a;
        var t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / len);
        return a + ab * t;
    }

    private static string F(Vector3 v)
    {
        return "(" + v.x.ToString("F2") + ", " + v.y.ToString("F2") + ", " + v.z.ToString("F2") + ")";
    }
}
