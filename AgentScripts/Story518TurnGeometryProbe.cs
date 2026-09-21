using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sonde ANO-5.18-04, question 1 a 3 : ou passe REELLEMENT le connecteur de virage.
///
/// Pour chaque mouvement de jonction elle imprime P0, le point de controle, P2, le rayon de
/// courbure minimal de la courbe, l'ecart lateral maximal a la polyligne d'intention
/// (entree -> coin -> sortie), et la distance la plus courte a l'axe de la voie OPPOSEE de la route
/// de sortie. Elle imprime enfin la vitesse maximale tenable sur ce rayon pour une acceleration
/// laterale donnee : c'est la grandeur qui manque au trafic.
/// </summary>
public static class Story518TurnGeometryProbe
{
    private const int Samples = 48;

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

            // Axes de voie de TOUT le graphe : sert a mesurer la distance a la voie opposee.
            var axes = new List<(Vector3 point, Vector3 forward)>();
            for (var i = 0; i < graph.NodeCount; i++)
            {
                var forward = Vector3.ProjectOnPlane(graph.GetNodeRotation(i) * Vector3.forward, Vector3.up);
                if (forward.sqrMagnitude <= 0.0001f) continue;
                axes.Add((graph.GetNodePosition(i), forward.normalized));
            }

            foreach (var a in graph.JunctionApproaches)
            {
                var node = graph.GetNodePosition(a.NodeIndex);
                var entry = a.EntryNode == a.NodeIndex ? node : a.EntryPoint;
                var entryForward = a.EntryForward.sqrMagnitude > 0.0001f ? a.EntryForward.normalized : a.Forward.normalized;

                foreach (var s in graph.GetSuccessors(a.NodeIndex))
                {
                    var exit = graph.GetNodePosition(s);
                    var exitForward = Vector3.ProjectOnPlane(graph.GetNodeRotation(s) * Vector3.forward, Vector3.up).normalized;
                    var angle = Vector3.SignedAngle(entryForward, exitForward, Vector3.up);
                    if (Mathf.Abs(angle) < 20f) continue; // tout droit : rien a mesurer

                    var hasCorner = LaneGraphRouting.TryResolveLaneAxisCorner(entry, entryForward, exit, exitForward, out var corner);
                    var kind = angle > 0f ? "gauche" : "droite";

                    var maxDeviation = 0f;
                    var maxDeviationAt = Vector3.zero;
                    var minRadius = float.PositiveInfinity;
                    var previous = LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, 0f);
                    var length = 0f;
                    var minOpposite = float.PositiveInfinity;
                    var oppositeAt = Vector3.zero;

                    for (var i = 0; i <= Samples; i++)
                    {
                        var t = i / (float)Samples;
                        var point = LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, t);
                        if (i > 0) length += Vector3.Distance(previous, point);

                        // Ecart a la polyligne d'intention.
                        var deviation = hasCorner
                            ? Mathf.Min(DistanceToSegment(point, entry, corner), DistanceToSegment(point, corner, exit))
                            : DistanceToSegment(point, entry, exit);
                        if (deviation > maxDeviation) { maxDeviation = deviation; maxDeviationAt = point; }

                        // Rayon de courbure par differences finies.
                        if (i > 0 && i < Samples)
                        {
                            var before = LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, t - 1f / Samples);
                            var after = LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, t + 1f / Samples);
                            var radius = CircleRadius(before, point, after);
                            if (radius > 0.01f && radius < minRadius) minRadius = radius;
                        }

                        // Voie opposee la plus proche : un axe dont le cap est oppose a notre cap local.
                        var tangent = (LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, Mathf.Min(1f, t + 0.02f)) - point);
                        tangent = Vector3.ProjectOnPlane(tangent, Vector3.up);
                        if (tangent.sqrMagnitude > 0.0001f)
                        {
                            tangent.Normalize();
                            foreach (var axis in axes)
                            {
                                if (Vector3.Dot(axis.forward, tangent) > -0.7f) continue; // pas une voie opposee
                                var distance = DistanceToLine(point, axis.point, axis.forward);
                                if (distance < minOpposite) { minOpposite = distance; oppositeAt = point; }
                            }
                        }

                        previous = point;
                    }

                    report.AppendLine("mvt " + a.NodeIndex + "->" + s + " " + kind + " " + angle.ToString("F0") + " deg  jonction=" + a.JunctionKey);
                    report.AppendLine("   P0=" + F(entry) + "  C=" + (hasCorner ? F(corner) : "(aucun)") + "  P2=" + F(exit));
                    report.AppendLine("   longueur=" + length.ToString("F2") + " m  rayon_min=" + minRadius.ToString("F2")
                        + " m  ecart_max=" + maxDeviation.ToString("F2") + " m en " + F(maxDeviationAt));
                    report.AppendLine("   axe_oppose_le_plus_proche=" + minOpposite.ToString("F2") + " m en " + F(oppositeAt));
                    report.AppendLine("   v_tenable(0,4g)=" + Mathf.Sqrt(0.4f * 9.81f * minRadius).ToString("F1")
                        + "  (0,7g)=" + Mathf.Sqrt(0.7f * 9.81f * minRadius).ToString("F1")
                        + "  m/s   (vitesse desiree authoree : 8,0 m/s)");
                }
            }

            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static float CircleRadius(Vector3 a, Vector3 b, Vector3 c)
    {
        var ab = Vector3.ProjectOnPlane(b - a, Vector3.up);
        var bc = Vector3.ProjectOnPlane(c - b, Vector3.up);
        var ca = Vector3.ProjectOnPlane(a - c, Vector3.up);
        var area = Mathf.Abs(ab.x * (-ca.z) - (-ca.x) * ab.z) * 0.5f;
        if (area < 0.000001f) return float.PositiveInfinity;
        return ab.magnitude * bc.magnitude * ca.magnitude / (4f * area);
    }

    private static float DistanceToSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        var segment = Vector3.ProjectOnPlane(end - start, Vector3.up);
        var length = segment.magnitude;
        if (length <= 0.0001f) return Vector3.ProjectOnPlane(point - start, Vector3.up).magnitude;
        var t = Mathf.Clamp01(Vector3.Dot(Vector3.ProjectOnPlane(point - start, Vector3.up), segment) / (length * length));
        return Vector3.ProjectOnPlane(point - (start + segment * t), Vector3.up).magnitude;
    }

    private static float DistanceToLine(Vector3 point, Vector3 origin, Vector3 direction)
    {
        var offset = Vector3.ProjectOnPlane(point - origin, Vector3.up);
        return Vector3.ProjectOnPlane(offset - direction * Vector3.Dot(offset, direction), Vector3.up).magnitude;
    }

    private static string F(Vector3 v)
    {
        return "(" + v.x.ToString("F2") + "," + v.z.ToString("F2") + ")";
    }
}
