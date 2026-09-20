using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Story 5.16 : renomme les libelles "+" / "-" des quatre boutons de trafic. Ils portaient tous le nom
/// herite du gabarit duplique (LobbyDifficultyButtonLabel), ce qui rendait la hierarchie ambigue pour
/// toute recherche par nom -- le cablage par fileID n'en souffrait pas, mais la scene ne disait plus ce
/// que chaque libelle affiche.
///
/// Script hors Assets/ : compile par le pipeline, jamais importe par l'Editor.
/// Idempotent : un libelle deja correctement nomme est laisse tel quel.
/// </summary>
public static class Story516TrafficLabelRename
{
    private const string ScenePath = "Assets/RoadRage/App/Scenes/MainMenuLobby.unity";

    private static readonly string[] ButtonNames =
    {
        "LobbyTrafficVehiclesDecreaseButton",
        "LobbyTrafficVehiclesIncreaseButton",
        "LobbyTrafficLitterThrowersDecreaseButton",
        "LobbyTrafficLitterThrowersIncreaseButton"
    };

    public static string Run()
    {
        var scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return "MainMenuLobby n'est pas chargee : ouvre-la avant de lancer ce renommage.";
        }

        var canvas = FindRoot();
        if (canvas == null)
        {
            return "Canvas introuvable dans MainMenuLobby.";
        }

        var report = new System.Text.StringBuilder();
        var renamed = 0;

        foreach (var buttonName in ButtonNames)
        {
            var button = Find(canvas, buttonName);
            if (button == null)
            {
                report.AppendLine("MANQUANT: " + buttonName);
                continue;
            }

            if (button.childCount != 1)
            {
                report.AppendLine("INATTENDU: " + buttonName + " porte " + button.childCount + " enfant(s)");
                continue;
            }

            var label = button.GetChild(0);
            var expected = buttonName + "Label";
            if (label.name == expected)
            {
                report.AppendLine("ok (deja): " + expected);
                continue;
            }

            Undo.RecordObject(label.gameObject, "Story 5.16 - renommage des libelles de trafic");
            label.name = expected;
            report.AppendLine("renomme: " + buttonName + " -> " + expected);
            renamed++;
        }

        if (renamed == 0)
        {
            return "Aucun libelle a renommer.\n" + report;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        var saved = EditorSceneManager.SaveScene(scene);
        return renamed + " libelle(s) renomme(s). Scene sauvee : " + saved + "\n" + report;
    }

    private static Transform FindRoot()
    {
        var scene = SceneManager.GetSceneByPath(ScenePath);
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Canvas")
            {
                return root.transform;
            }
        }

        return null;
    }

    private static Transform Find(Transform parent, string name)
    {
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == name)
            {
                return child;
            }

            var nested = Find(child, name);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }
}
