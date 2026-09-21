using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// De quelle place dispose un virage pour son rayon ?
///
/// Pour chaque mouvement tournant : le coin des deux axes de voie, la distance disponible le long de
/// l'axe d'entree et le long de l'axe de sortie, l'angle de deviation, le rayon du connecteur
/// ACTUEL, et le rayon d'un CONGE tangent que cette place autorise. La comparaison des deux dit si
/// le trace manque de place ou si c'est la courbe qui est mal construite.
/// </summary>
public static class Story518FilletProbe
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

            var def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(
                "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset");
            var vehicle = def.Profile;
            var min = float.PositiveInfinity; var max = float.NegativeInfinity;
            for (var i = 0; i < vehicle.WheelCount; i++)
            {
                var z = vehicle.GetWheel(i).LocalPosition.z;
                min = Mathf.Min(min, z); max = Mathf.Max(max, z);
            }
            var wheelbase = max - min;
            var tightest = wheelbase / Mathf.Tan(vehicle.MaxSteerAngleDegrees * Mathf.Deg2Rad);
            report.AppendLine("empattement=" + wheelbase.ToString("F2") + " m  braquage_max=" + vehicle.MaxSteerAngleDegrees
                + " deg  -> rayon le plus serre possible (au pas) = " + tightest.ToString("F2") + " m");
            report.AppendLine("braquage a 8 m/s = " + VehicleSteeringModel.ResolveSteerAngleDegrees(1f, 8f, 0f,
                vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed).ToString("F1")
                + " deg  -> rayon minimal a 8 m/s = "
                + (wheelbase / Mathf.Tan(VehicleSteeringModel.ResolveSteerAngleDegrees(1f, 8f, 0f,
                    vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed) * Mathf.Deg2Rad)).ToString("F2") + " m");
            report.AppendLine();

            foreach (var approach in graph.JunctionApproaches)
            {
                var node = approach.NodeIndex;
                var u = Forward(graph, node);
                foreach (var exit in graph.GetSuccessors(node))
                {
                    var v = Forward(graph, exit);
                    if (u.sqrMagnitude < 0.0001f || v.sqrMagnitude < 0.0001f) continue;
                    var deflection = Vector3.SignedAngle(u, v, Vector3.up);
                    if (Mathf.Abs(deflection) < 20f) continue;

                    var A = graph.GetNodePosition(node);
                    var B = graph.GetNodePosition(exit);
                    if (!LaneGraphRouting.TryResolveLaneAxisCorner(A, u, B, v, out var C))
                    {
                        report.AppendLine("mvt " + node + "->" + exit + " : PAS DE COIN");
                        continue;
                    }

                    var dA = Vector3.ProjectOnPlane(C - A, Vector3.up).magnitude;
                    var dB = Vector3.ProjectOnPlane(B - C, Vector3.up).magnitude;
                    var half = Mathf.Abs(deflection) * 0.5f * Mathf.Deg2Rad;
                    // Conge tangent : T = R tan(theta/2), donc R = T / tan(theta/2).
                    var room = Mathf.Min(dA, dB);
                    var filletRadius = room / Mathf.Tan(half);

                    // Rayon minimal du connecteur ACTUEL (Bezier quadratique, coin = point de controle).
                    var current = float.PositiveInfinity;
                    for (var i = 1; i < 48; i++)
                    {
                        var t = i / 48f;
                        var p0 = LaneGraphRouting.ResolveJunctionTurnPoint(A, u, B, v, t - 1f / 48f);
                        var p1 = LaneGraphRouting.ResolveJunctionTurnPoint(A, u, B, v, t);
                        var p2 = LaneGraphRouting.ResolveJunctionTurnPoint(A, u, B, v, t + 1f / 48f);
                        var r = CircleRadius(p0, p1, p2);
                        if (r > 0.01f && r < current) current = r;
                    }

                    // Ecart maximal du connecteur actuel a la polyligne A -> C -> B.
                    var deviation = 0f;
                    for (var i = 0; i <= 48; i++)
                    {
                        var p = LaneGraphRouting.ResolveJunctionTurnPoint(A, u, B, v, i / 48f);
                        deviation = Mathf.Max(deviation, Mathf.Min(Seg(p, A, C), Seg(p, C, B)));
                    }

                    report.AppendLine("mvt " + (node + "->" + exit).PadRight(9)
                        + (deflection > 0f ? "droite" : "gauche") + " " + Mathf.Abs(deflection).ToString("F0").PadLeft(3) + " deg"
                        + "  dA=" + dA.ToString("F2").PadLeft(6) + "  dB=" + dB.ToString("F2").PadLeft(6)
                        + "  R_actuel=" + current.ToString("F2").PadLeft(6)
                        + "  R_conge_possible=" + filletRadius.ToString("F2").PadLeft(6)
                        + "  ecart_connecteur=" + deviation.ToString("F2").PadLeft(6)
                        + (filletRadius >= tightest ? "  [conge FAISABLE]" : "  [place insuffisante]"));
                }
            }

            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static Vector3 Forward(LaneGraph graph, int node)
    {
        var f = Vector3.ProjectOnPlane(graph.GetNodeRotation(node) * Vector3.forward, Vector3.up);
        return f.sqrMagnitude > 0.0001f ? f.normalized : Vector3.zero;
    }

    private static float Seg(Vector3 p, Vector3 a, Vector3 b)
    {
        return Vector3.ProjectOnPlane(TrafficPerception.ClosestPointOnSegment(p, a, b) - p, Vector3.up).magnitude;
    }

    private static float CircleRadius(Vector3 a, Vector3 b, Vector3 c)
    {
        var ab = Vector3.ProjectOnPlane(b - a, Vector3.up).magnitude;
        var bc = Vector3.ProjectOnPlane(c - b, Vector3.up).magnitude;
        var ca = Vector3.ProjectOnPlane(a - c, Vector3.up).magnitude;
        var s = (ab + bc + ca) * 0.5f;
        var area = Mathf.Sqrt(Mathf.Max(0f, s * (s - ab) * (s - bc) * (s - ca)));
        return area < 0.0001f ? float.PositiveInfinity : ab * bc * ca / (4f * area);
    }
}
