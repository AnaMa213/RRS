using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// A quelle vitesse un vehicule parcourt-il REELLEMENT un anneau de giratoire, et son profil de
/// vitesse est-il lisse ?
///
/// Recette : "dans les ronds-points les voitures sont tres lentes, le ralentissement n'est pas
/// naturel et cree des a-coups". Les deux se mesurent : la vitesse minimale dit la lenteur, la plus
/// forte variation par seconde dit l'a-coup.
/// </summary>
public static class Story518RingSpeedProbe
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

            report.AppendLine("vitesse desiree authoree = " + driver.DesiredSpeed.ToString("F1") + " m/s");
            report.AppendLine("empattement = " + wheelbase.ToString("F2") + " m");
            report.AppendLine();

            var ring = FindRing(graph);
            if (ring == null) return "aucun anneau trouve";

            var path = BuildRing(graph, ring);
            report.AppendLine("anneau : " + ring.Count + " noeuds, " + path.Length + " points echantillonnes");

            // Rayons lus le long de l'anneau, ancienne et nouvelle mesure.
            var oldMin = float.PositiveInfinity;
            var newMin = float.PositiveInfinity;
            for (var i = 1; i + 1 < path.Length; i++)
            {
                oldMin = Mathf.Min(oldMin, LaneGraphRouting.ResolvePathRadius(path, path.Length, i));
                newMin = Mathf.Min(newMin, LaneGraphRouting.ResolveRadiusOverArc(path, path.Length, i, 2f));
            }
            report.AppendLine("rayon minimal lu -- 3 points voisins : " + oldMin.ToString("F2")
                + " m   |   sur arc de 2 m : " + newMin.ToString("F2") + " m");
            report.AppendLine("plafond de vitesse correspondant -- 3 points : "
                + Fmt(VehicleSteeringModel.ResolveCurveSpeedLimit(oldMin, wheelbase, vehicle.MaxSteerAngleDegrees,
                    vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed))
                + "   |   sur arc : "
                + Fmt(VehicleSteeringModel.ResolveCurveSpeedLimit(newMin, wheelbase, vehicle.MaxSteerAngleDegrees,
                    vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed)));
            report.AppendLine();

            foreach (var look in new[] { 0.8f, 1.0f })
            foreach (var reserve in new[] { 1.0f, 0.85f, 0.7f, 0.55f })
            {
                report.Append("look=" + look.ToString("F1") + "s reserve=" + reserve.ToString("F2") + "  ");
                Simulate(path, vehicle, driver, wheelbase, report, look, 999f, reserve);
            }
            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static string Fmt(float v) => float.IsInfinity(v) ? "aucun" : v.ToString("F1") + " m/s";

    private static void Simulate(Vector3[] path, VehicleProfile vehicle, DriverProfile driver,
        float wheelbase, StringBuilder report, float lookSeconds, float boundFactor, float reserve)
    {
        var position = path[0];
        var forward = Planar(path[1] - path[0]).normalized;
        var speed = driver.DesiredSpeed;
        var travelled = 0f;
        var total = Length(path);
        var slowest = float.PositiveInfinity;
        var worstError = 0f;
        var worstJerk = 0f;
        var previousSpeed = speed;
        var sum = 0f;
        var samples = 0;
        var lines = new List<string>();

        for (var step = 0; step < 6000 && travelled < total - 2f; step++)
        {
            var look = Mathf.Max(0f, speed) * lookSeconds;
            var window = Mathf.Max(6f, look + 6f);
            var radius = LaneGraphRouting.ResolvePathMinimumRadius(path, path.Length, position, window);
            var ceiling = VehicleSteeringModel.ResolveCurveSpeedLimit(radius * reserve, wheelbase,
                vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);

            var bounded = Mathf.Clamp(look, wheelbase,
                float.IsFinite(radius) ? Mathf.Max(wheelbase, radius * boundFactor) : Mathf.Max(wheelbase, look));
            var aim = LaneGraphRouting.ResolvePathLookAheadPoint(path, path.Length, position, bounded);

            var toAim = Planar(aim - position);
            var sine = Mathf.Abs(Mathf.Sin(Vector3.Angle(forward, toAim) * Mathf.Deg2Rad));
            var commandRadius = toAim.magnitude > 0.01f && sine >= 0.001f
                ? toAim.magnitude / (2f * sine) : float.PositiveInfinity;
            var commandCeiling = VehicleSteeringModel.ResolveCurveSpeedLimit(commandRadius, wheelbase,
                vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);

            var target = Mathf.Min(driver.DesiredSpeed,
                float.IsFinite(ceiling) ? Mathf.Max(1f, ceiling) : driver.DesiredSpeed);
            speed = Mathf.MoveTowards(speed, target, Mathf.Max(0.1f, driver.MaxAcceleration) * 0.02f * 3f);

            var steer = NetworkedAIVehicleDriverController.ComputeSeekIntent(position, forward, aim, 45f).Steer;
            var angle = VehicleSteeringModel.ResolveSteerAngleDegrees(steer, speed, vehicle.MinimumDirectionSpeed,
                vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);

            forward = Quaternion.AngleAxis(speed / wheelbase * Mathf.Tan(angle * Mathf.Deg2Rad) * 0.02f * Mathf.Rad2Deg,
                Vector3.up) * forward;
            var delta = forward * speed * 0.02f;
            position += delta;
            travelled += delta.magnitude;

            if (travelled > 4f)
            {
                worstError = Mathf.Max(worstError, DistanceTo(path, position));
                slowest = Mathf.Min(slowest, speed);
                sum += speed; samples++;
                worstJerk = Mathf.Max(worstJerk, Mathf.Abs(speed - previousSpeed) / 0.02f);
            }
            previousSpeed = speed;

            if (step % 25 == 0)
            {
                lines.Add("   s=" + travelled.ToString("F1").PadLeft(5)
                    + " v=" + speed.ToString("F2").PadLeft(5)
                    + " R=" + (float.IsInfinity(radius) ? "  inf" : radius.ToString("F1").PadLeft(5))
                    + " plafond_trace=" + Fmt(ceiling).PadLeft(9)
                    + " plafond_commande=" + Fmt(commandCeiling).PadLeft(9)
                    + " visee=" + bounded.ToString("F1"));
            }
        }

        report.AppendLine("v_min=" + slowest.ToString("F2") + "  v_moy=" + (samples > 0 ? sum / samples : 0f).ToString("F2")
            + "  a-coup=" + worstJerk.ToString("F1") + "  ECART=" + worstError.ToString("F2") + " m");
    }

    private static List<int> FindRing(LaneGraph graph)
    {
        for (var start = 0; start < graph.NodeCount; start++)
        {
            var ring = new List<int>();
            var current = start;
            for (var step = 0; step < 64; step++)
            {
                ring.Add(current);
                var next = Best(graph, current);
                if (next < 0) break;
                if (next == start && ring.Count >= 5) return ring;
                if (ring.Contains(next)) break;
                current = next;
            }
        }
        return null;
    }

    private static int Best(LaneGraph graph, int node)
    {
        var forward = Forward(graph, node);
        var best = -1; var bestDot = -2f;
        foreach (var s in graph.GetSuccessors(node))
        {
            var d = Planar(graph.GetNodePosition(s) - graph.GetNodePosition(node));
            if (d.sqrMagnitude < 0.01f) continue;
            var dot = Vector3.Dot(d.normalized, forward);
            if (dot > bestDot) { bestDot = dot; best = s; }
        }
        return best;
    }

    private static Vector3[] BuildRing(LaneGraph graph, List<int> ring)
    {
        var buffer = new Vector3[1024];
        var count = 0;
        buffer[count++] = graph.GetNodePosition(ring[0]);
        for (var lap = 0; lap < 2; lap++)
        {
            for (var i = 0; i < ring.Count; i++)
            {
                var from = ring[i];
                var to = ring[(i + 1) % ring.Count];
                count = LaneGraphRouting.SampleLaneEdge(graph.GetNodePosition(from), Forward(graph, from),
                    graph.GetNodePosition(to), Forward(graph, to), 1f, buffer, count);
            }
        }
        var path = new Vector3[count];
        for (var i = 0; i < count; i++) path[i] = new Vector3(buffer[i].x, 0f, buffer[i].z);
        return path;
    }

    private static Vector3 Forward(LaneGraph graph, int node)
    {
        var f = Planar(graph.GetNodeRotation(node) * Vector3.forward);
        return f.sqrMagnitude > 0.0001f ? f.normalized : Vector3.zero;
    }

    private static Vector3 Planar(Vector3 v) => Vector3.ProjectOnPlane(v, Vector3.up);

    private static float DistanceTo(Vector3[] p, Vector3 q)
    {
        var best = float.PositiveInfinity;
        for (var i = 0; i + 1 < p.Length; i++)
            best = Mathf.Min(best, Planar(TrafficPerception.ClosestPointOnSegment(q, p[i], p[i + 1]) - q).magnitude);
        return best;
    }

    private static float Length(Vector3[] p)
    {
        var t = 0f;
        for (var i = 1; i < p.Length; i++) t += Planar(p[i] - p[i - 1]).magnitude;
        return t;
    }
}
