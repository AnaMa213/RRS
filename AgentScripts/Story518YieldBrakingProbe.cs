using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Profil de freinage d'une cession, AVANT et APRES la ligne de cession.
///
/// Avant : la cession posait <c>ResolveStopIntent</c> -- frein a fond -- des que le conflit etait
/// vu, donc jusqu'a la portee de prediction en amont. Apres : la ligne devient un leader immobile
/// virtuel et c'est l'IDM qui pose la decelaration.
///
/// La mesure rend la decelaration de pointe et la distance reellement parcourue avant l'arret.
/// </summary>
public static class Story518YieldBrakingProbe
{
    public static string Run()
    {
        var report = new StringBuilder();
        var def = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(
            "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset");
        var profile = def.Profile;

        report.AppendLine("profil : vDesiree=" + profile.DesiredSpeed.ToString("0.00")
            + " aMax=" + profile.MaxAcceleration.ToString("0.00")
            + " freinConfort=" + profile.ComfortableDeceleration.ToString("0.00")
            + " ecartMin=" + profile.MinimumGap.ToString("0.00")
            + " reaction=" + profile.ReactionTime.ToString("0.00")
            + " perception=" + profile.PerceptionRadius.ToString("0.0")
            + " prediction=" + profile.PredictionSeconds.ToString("0.0"));
        report.AppendLine();

        foreach (var distance in new[] { 25f, 15f, 8f })
        {
            report.AppendLine("=== conflit vu a " + distance.ToString("0") + " m, vitesse 8,00 m/s");
            Simulate(profile, distance, report);
        }

        return report.ToString();
    }

    private static void Simulate(DriverProfile profile, float lineDistance, StringBuilder report)
    {
        const float dt = 0.02f;
        var speed = 8f;
        var travelled = 0f;
        var applied = 0f;
        var peak = 0f;
        var peakJerk = 0f;
        var previous = 0f;

        for (var step = 0; step < 2000 && speed > 0.05f; step++)
        {
            var gap = Mathf.Max(0f, lineDistance - travelled);
            var target = DriverModel.ComputeAcceleration(profile, speed, 0f, gap);
            applied = DriverModel.SmoothAcceleration(applied, target, profile.ReactionTime, dt);
            peak = Mathf.Min(peak, applied);
            peakJerk = Mathf.Max(peakJerk, Mathf.Abs(applied - previous) / dt);
            previous = applied;
            speed = Mathf.Max(0f, speed + applied * dt);
            travelled += speed * dt;
        }

        report.AppendLine("   IDM vers la ligne : arret a " + travelled.ToString("0.00")
            + " m parcourus, soit " + (lineDistance - travelled).ToString("0.00") + " m avant la ligne");
        report.AppendLine("   decelaration de pointe " + peak.ToString("0.00")
            + " m/s2, a-coup de pointe " + peakJerk.ToString("0.0") + " m/s3");
        report.AppendLine("   ancien comportement : frein a fond immediat, arret sur place, "
            + lineDistance.ToString("0.00") + " m AVANT le point de conflit");
    }
}
