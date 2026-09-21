using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Trace pas a pas la loi de guidage sur UN virage : ou l'ecart nait, avec quelle vitesse, quelle
/// visee et quel rayon lu. Sans cette trace, corriger revient a deviner.
/// </summary>
public static class Story518TraceProbe
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

            var vehicle = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(
                "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset").Profile;
            var driver = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(
                "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset").Profile;
            var min = float.PositiveInfinity; var max = float.NegativeInfinity;
            for (var i = 0; i < vehicle.WheelCount; i++)
            {
                var z = vehicle.GetWheel(i).LocalPosition.z;
                min = Mathf.Min(min, z); max = Mathf.Max(max, z);
            }
            var wheelbase = max - min;

            // Le pire virage mesure : 8->7.
            foreach (var approach in graph.JunctionApproaches)
            {
                if (approach.NodeIndex != 8) continue;
                var entryForward = Vector3.ProjectOnPlane(approach.EntryForward, Vector3.up).normalized;
                var entryPoint = approach.EntryNode == 8 ? graph.GetNodePosition(8) : approach.EntryPoint;
                report.AppendLine("approche 8 : EntryNode=" + approach.EntryNode + " EntryPoint=" + F(entryPoint)
                    + " noeud=" + F(graph.GetNodePosition(8)) + " cap=" + F(entryForward));

                foreach (var exit in new[] { 7, 5 })
                {
                    var exitForward = Vector3.ProjectOnPlane(graph.GetNodeRotation(exit) * Vector3.forward, Vector3.up).normalized;
                    var buffer = new Vector3[512];
                    var count = 0;
                    var start = entryPoint - entryForward * 20f;
                    buffer[count++] = start;
                    count = LaneGraphRouting.SampleLaneEdge(start, entryForward, entryPoint, entryForward, 1f, buffer, count);
                    var afterLead = count;
                    count = LaneGraphRouting.SampleLaneEdge(entryPoint, entryForward, graph.GetNodePosition(exit), exitForward, 1f, buffer, count);
                    var afterTurn = count;
                    count = LaneGraphRouting.SampleLaneEdge(graph.GetNodePosition(exit), exitForward,
                        graph.GetNodePosition(exit) + exitForward * 15f, exitForward, 1f, buffer, count);

                    var path = new Vector3[count];
                    for (var i = 0; i < count; i++) path[i] = new Vector3(buffer[i].x, 0f, buffer[i].z);

                    report.AppendLine();
                    report.AppendLine("=== mvt 8->" + exit + "  points=" + count
                        + " (amorce jusqu'a " + afterLead + ", virage jusqu'a " + afterTurn + ")");

                    // Rayons lus le long de la trajectoire.
                    var radii = new StringBuilder("   rayons: ");
                    for (var i = 1; i + 1 < count; i++)
                    {
                        var r = LaneGraphRouting.ResolvePathRadius(path, count, i);
                        radii.Append(i + ":" + (float.IsInfinity(r) ? "inf" : r.ToString("F1")) + " ");
                    }
                    report.AppendLine(radii.ToString());

                    foreach (var look in new[] { 0.8f, 1.0f })
                    foreach (var reserve in new[] { 1.0f, 0.85f, 0.7f, 0.55f })
                    {
                        report.Append("   look=" + look.ToString("F1") + "s reserve=" + reserve.ToString("F2") + "  ");
                        Simulate(path, vehicle, driver, wheelbase, report, look, 999f, reserve);
                    }
                }
            }

            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static void Simulate(Vector3[] path, VehicleProfile vehicle, DriverProfile driver,
        float wheelbase, StringBuilder report, float lookSeconds, float boundFactor, float reserve)
    {
        var position = path[0];
        var forward = Planar(path[1] - path[0]).normalized;
        var speed = driver.DesiredSpeed;
        var travelled = 0f;
        var total = Length(path);
        var worst = 0f;
        var worstAt = 0f;
        var lines = new List<string>();

        for (var step = 0; step < 4000 && travelled < total - 2f; step++)
        {
            var look = Mathf.Max(0f, speed) * lookSeconds;
            var window = Mathf.Max(6f, look + 6f);
            var radius = LaneGraphRouting.ResolvePathMinimumRadius(path, path.Length, position, window);
            var ceiling = VehicleSteeringModel.ResolveCurveSpeedLimit(radius * reserve, wheelbase,
                vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);
            var target = Mathf.Min(driver.DesiredSpeed, float.IsFinite(ceiling) ? Mathf.Max(1f, ceiling) : driver.DesiredSpeed);
            speed = Mathf.MoveTowards(speed, target, Mathf.Max(0.1f, driver.MaxAcceleration) * 0.02f * 3f);

            var upper = float.IsFinite(radius) ? Mathf.Max(wheelbase, radius * boundFactor) : Mathf.Max(wheelbase, look);
            var bounded = Mathf.Clamp(look, wheelbase, upper);
            var aim = LaneGraphRouting.ResolvePathLookAheadPoint(path, path.Length, position, bounded);

            var steer = NetworkedAIVehicleDriverController.ComputeSeekIntent(position, forward, aim, 45f).Steer;
            var angle = VehicleSteeringModel.ResolveSteerAngleDegrees(steer, speed, vehicle.MinimumDirectionSpeed,
                vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);

            var yaw = speed / wheelbase * Mathf.Tan(angle * Mathf.Deg2Rad) * 0.02f * Mathf.Rad2Deg;
            forward = Quaternion.AngleAxis(yaw, Vector3.up) * forward;
            var delta = forward * speed * 0.02f;
            position += delta;
            travelled += delta.magnitude;

            var error = DistanceTo(path, position);
            if (error > worst) { worst = error; worstAt = travelled; }
            if (step % 10 == 0)
            {
                lines.Add("   s=" + travelled.ToString("F1").PadLeft(5)
                    + " v=" + speed.ToString("F1").PadLeft(4)
                    + " R=" + (float.IsInfinity(radius) ? " inf" : radius.ToString("F1").PadLeft(4))
                    + " plafond=" + (float.IsInfinity(ceiling) ? " inf" : ceiling.ToString("F1").PadLeft(4))
                    + " visee=" + bounded.ToString("F1").PadLeft(4)
                    + " braquage=" + angle.ToString("F0").PadLeft(4)
                    + " ecart=" + error.ToString("F2"));
            }
        }

        report.AppendLine("ECART MAXIMAL = " + worst.ToString("F2") + " m a s=" + worstAt.ToString("F1") + " m");
    }


    /// <summary>Conge tangent : droite -> arc de rayon constant -> droite.</summary>
    private static Vector3[] BuildFillet(Vector3 entry, Vector3 u, Vector3 exit, Vector3 v, float radius)
    {
        if (!LaneGraphRouting.TryResolveLaneAxisCorner(entry, u, exit, v, out var corner)) return null;
        var deflection = Vector3.SignedAngle(u, v, Vector3.up);
        var half = Mathf.Abs(deflection) * 0.5f * Mathf.Deg2Rad;
        var tangent = radius * Mathf.Tan(half);
        if (tangent > Planar(corner - entry).magnitude || tangent > Planar(exit - corner).magnitude) return null;

        var inPoint = corner - u * tangent;
        var outPoint = corner + v * tangent;
        var side = deflection > 0f ? 1f : -1f;
        var centre = inPoint + Vector3.Cross(Vector3.up, u).normalized * (side * radius);

        var points = new List<Vector3>();
        var lead = Planar(inPoint - entry).magnitude + 20f;
        var start = entry - u * 20f;
        points.Add(start);
        var leadSteps = Mathf.Max(1, Mathf.CeilToInt(lead / 2f));
        for (var i = 1; i <= leadSteps; i++) points.Add(Vector3.Lerp(start, inPoint, i / (float)leadSteps));

        var sweep = Mathf.Abs(deflection);
        var arcSteps = Mathf.Max(2, Mathf.CeilToInt(radius * sweep * Mathf.Deg2Rad / 1f));
        for (var i = 1; i <= arcSteps; i++)
        {
            var angle = side * sweep * (i / (float)arcSteps);
            points.Add(centre + Quaternion.AngleAxis(angle, Vector3.up) * (inPoint - centre));
        }

        var tail = outPoint + v * 15f;
        var tailSteps = Mathf.Max(1, Mathf.CeilToInt(Planar(tail - outPoint).magnitude / 2f));
        for (var i = 1; i <= tailSteps; i++) points.Add(Vector3.Lerp(outPoint, tail, i / (float)tailSteps));

        var path = new Vector3[points.Count];
        for (var i = 0; i < points.Count; i++) path[i] = new Vector3(points[i].x, 0f, points[i].z);
        return path;
    }

    private static Vector3 Planar(Vector3 v) => Vector3.ProjectOnPlane(v, Vector3.up);


    private static float Length(Vector3[] p)
    {
        var t = 0f;
        for (var i = 1; i < p.Length; i++) t += Planar(p[i] - p[i - 1]).magnitude;
        return t;
    }

    private static float DistanceTo(Vector3[] p, Vector3 q)
    {
        var best = float.PositiveInfinity;
        for (var i = 0; i + 1 < p.Length; i++)
            best = Mathf.Min(best, Planar(TrafficPerception.ClosestPointOnSegment(q, p[i], p[i + 1]) - q).magnitude);
        return best;
    }

    private static string F(Vector3 v) => "(" + v.x.ToString("F2") + ";" + v.z.ToString("F2") + ")";
}
