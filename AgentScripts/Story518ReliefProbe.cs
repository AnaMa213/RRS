using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Sonde : geometrie exacte des objets retenus comme obstacles par ANO-5.18-02.</summary>
public static class Story518ReliefProbe
{
    private static readonly string[] Names = { "Rampe_Ouest", "Rampe_Est", "Relief_MarcheBasse_AvenueCenterToEast" };

    public static string Run()
    {
        var report = new StringBuilder();
        var scene = EditorSceneManager.OpenScene("Assets/RoadRage/App/Scenes/MVP_Run.unity", OpenSceneMode.Additive);
        try
        {
            foreach (var root in scene.GetRootGameObjects()) Walk(root.transform, report);
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
        return report.ToString();
    }

    private static void Walk(Transform t, StringBuilder report)
    {
        foreach (var n in Names)
        {
            if (t.name != n) continue;
            var col = t.GetComponent<Collider>();
            report.AppendLine(t.name + " parent=" + (t.parent != null ? t.parent.name : "(racine)"));
            report.AppendLine("   pos=" + t.position + " scale=" + t.lossyScale + " rot=" + t.eulerAngles);
            if (col != null)
                report.AppendLine("   collider=" + col.GetType().Name + " bounds.min.y=" + col.bounds.min.y.ToString("F2")
                    + " bounds.max.y=" + col.bounds.max.y.ToString("F2") + " taille=" + col.bounds.size
                    + " centre=" + col.bounds.center);
            else report.AppendLine("   AUCUN collider");
        }
        for (var i = 0; i < t.childCount; i++) Walk(t.GetChild(i), report);
    }
}
