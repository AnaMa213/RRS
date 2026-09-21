using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sonde de lecture de la Story 5.18 : verifie que les valeurs authorées hors focus de l'Editeur sont
/// bien relues par le moteur (plans de feux du Def de trafic, seuils d'intersection du profil de
/// conduite). Une valeur ecrite a la main dans un YAML peut etre perdue en silence si sa forme ne
/// correspond pas a ce que le serialiseur attend : cette sonde le rend visible avant les tests.
/// </summary>
public static class Story518AuthoringProbe
{
    private const string TrafficDefPath = "Assets/RoadRage/ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset";
    private const string DriverDefPath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";

    /// <summary>
    /// Mesure la geometrie REELLE de la signalisation posee. Les <c>bounds</c> d'un renderer lus sur un
    /// asset de prefab non instancie sont degeneres (mesure du 2026-09-18 sur les colliders, meme piege
    /// ici) : seule une instance dit ou le mesh se trouve vraiment par rapport au plan roulant.
    /// </summary>
    public static string RunGeometry()
    {
        var report = new StringBuilder();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_Intersection.prefab");
        if (prefab == null)
        {
            return "Greybox_Intersection.prefab introuvable.";
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        try
        {
            foreach (var child in instance.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.StartsWith("Visual_Signal_", System.StringComparison.Ordinal))
                {
                    continue;
                }

                var min = float.PositiveInfinity;
                var max = float.NegativeInfinity;
                var renderers = child.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    if (renderer.bounds.min.y < min) min = renderer.bounds.min.y;
                    if (renderer.bounds.max.y > max) max = renderer.bounds.max.y;
                }

                report.AppendLine(child.name + " : y [" + min.ToString("0.000") + " .. " + max.ToString("0.000")
                    + "] renderers=" + renderers.Length + " colliders=" + child.GetComponentsInChildren<Collider>(true).Length);
            }
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }

        return report.ToString();
    }

    /// <summary>
    /// Inventaire dimensionnel des props Synty candidats : hauteur totale, position de la base sous
    /// l'origine, nombre de renderers. Mesure sur INSTANCE, seule fiable.
    /// </summary>
    public static string RunProps()
    {
        var names = new[]
        {
            "SM_Prop_TrafficLight_01", "SM_Prop_TrafficLight_02", "SM_Prop_TrafficLight_03",
            "SM_Prop_LightPole_Base_01", "SM_Prop_LightPole_Arm_01", "SM_Prop_Light_Attachment_01",
            "SM_Prop_LightPole_Lights_01",
            "SM_Prop_Sign_Stop_01", "SM_Prop_Sign_Attachment_01", "SM_Prop_Sign_Attachment_02"
        };

        var report = new StringBuilder();
        foreach (var name in names)
        {
            var path = "Assets/Synty/PolygonCity/Prefabs/Props/" + name + ".prefab";
            var prop = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prop == null)
            {
                report.AppendLine(name + " : INTROUVABLE");
                continue;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prop);
            try
            {
                var renderers = instance.GetComponentsInChildren<Renderer>();
                var min = renderers[0].bounds.min;
                var max = renderers[0].bounds.max;
                for (var i = 1; i < renderers.Length; i++)
                {
                    min = Vector3.Min(min, renderers[i].bounds.min);
                    max = Vector3.Max(max, renderers[i].bounds.max);
                }

                var origin = instance.transform.position;
                report.AppendLine(name + " : base=" + (min.y - origin.y).ToString("0.000")
                    + " hauteur=" + (max.y - min.y).ToString("0.000")
                    + " largeur=" + (max.x - min.x).ToString("0.000")
                    + " profondeur=" + (max.z - min.z).ToString("0.000")
                    + " renderers=" + renderers.Length
                    + " echelle=" + instance.transform.localScale.ToString("0.00"));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        return report.ToString();
    }

    public static string Run()
    {
        var report = new StringBuilder();

        var traffic = AssetDatabase.LoadAssetAtPath<TrafficSettingsDef>(TrafficDefPath);
        report.AppendLine("TrafficSettingsDef : " + (traffic != null));
        if (traffic != null)
        {
            report.AppendLine("TryValidate : " + traffic.TryValidate(out var trafficError) + " " + trafficError);
            report.AppendLine("SignalPlans : " + traffic.SignalPlans.Count);
            for (var i = 0; i < traffic.SignalPlans.Count; i++)
            {
                var plan = traffic.SignalPlans[i];
                report.Append("  [" + i + "] id=\"" + plan.JunctionId + "\" minGreen=" + plan.MinimumGreenSeconds
                    + " cycle=" + plan.CycleSeconds + " phases=" + plan.Phases.Count);
                for (var p = 0; p < plan.Phases.Count; p++)
                {
                    var phase = plan.Phases[p];
                    report.Append(" {d=" + phase.DurationSeconds + " groups=[");
                    for (var g = 0; g < phase.GreenGroups.Count; g++)
                    {
                        report.Append(phase.GreenGroups[g]);
                        if (g + 1 < phase.GreenGroups.Count) report.Append(',');
                    }
                    report.Append("]}");
                }
                report.AppendLine();
            }
            report.AppendLine("TryGetSignalPlan(junction_district_intersection) : "
                + traffic.TryGetSignalPlan("junction_district_intersection", out _));
            report.AppendLine("TryGetSignalPlan(inconnu) : " + traffic.TryGetSignalPlan("inconnu", out _));
        }

        var driver = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverDefPath);
        report.AppendLine("DriverProfileDef : " + (driver != null));
        if (driver != null)
        {
            report.AppendLine("TryValidate : " + driver.TryValidate(out var driverError) + " " + driverError);
            var profile = driver.Profile;
            report.AppendLine("junctionApproachRadius=" + profile.JunctionApproachRadius
                + " stopHold=" + profile.JunctionStopHoldSeconds
                + " acceptedGap=" + profile.JunctionAcceptedGap
                + " escalation=" + profile.JunctionEscalationDelay
                + " exitClearance=" + profile.JunctionExitClearanceRadius);
        }

        return report.ToString();
    }
}
