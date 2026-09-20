using RoadRage.App.Lobby;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Authoring de scene de la Story 5.16 (MainMenuLobby) : ajoute sous /Canvas/LobbyPanel les deux
/// lignes de reglage de trafic (libelle + bouton - + bouton +), puis assigne les champs serialises de
/// LobbyRosterScreen et le TrafficSettingsDef sur LobbyFlowController.
///
/// Script hors Assets/ : compile par le pipeline, jamais importe par l'Editor, donc aucun reimport
/// d'asset ni rechargement de domaine pendant l'execution.
///
/// Idempotent : si la premiere ligne existe deja, rien n'est reconstruit.
/// </summary>
public static class Story516LobbyTrafficRowsAuthoring
{
    private const string ScenePath = "Assets/RoadRage/App/Scenes/MainMenuLobby.unity";

    private const string TrafficDefPath = "Assets/RoadRage/ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset";

    private const string VehiclesLabelName = "LobbyTrafficVehiclesLabel";

    private const string VehiclesDecreaseName = "LobbyTrafficVehiclesDecreaseButton";

    private const string VehiclesIncreaseName = "LobbyTrafficVehiclesIncreaseButton";

    private const string LitterLabelName = "LobbyTrafficLitterThrowersLabel";

    private const string LitterDecreaseName = "LobbyTrafficLitterThrowersDecreaseButton";

    private const string LitterIncreaseName = "LobbyTrafficLitterThrowersIncreaseButton";

    private static readonly Vector2 ButtonSize = new Vector2(60f, 60f);

    private static readonly Vector2 LabelSize = new Vector2(380f, 50f);

    public static string Run()
    {
        var scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return "MainMenuLobby n'est pas chargee : ouvre-la avant de lancer cet authoring.";
        }

        var screen = FindSceneComponent<LobbyRosterScreen>(scene);
        if (screen == null)
        {
            return "LobbyRosterScreen introuvable dans les scenes chargees.";
        }

        var panel = screen.transform;
        var alreadyAuthored = panel.Find(VehiclesLabelName) != null;

        var buttonTemplate = panel.Find("LobbyDifficultyButton");
        var labelTemplate = panel.Find("LobbySettingsSummaryLabel");
        if (buttonTemplate == null || labelTemplate == null)
        {
            return "Gabarits introuvables (LobbyDifficultyButton / LobbySettingsSummaryLabel) : authoring abandonne.";
        }

        if (buttonTemplate.GetComponent<Button>() == null
            || buttonTemplate.GetComponentInChildren<TMP_Text>(true) == null
            || labelTemplate.GetComponent<TMP_Text>() == null)
        {
            return "Composants Button/TMP_Text introuvables dans les gabarits : authoring abandonne.";
        }

        var vehiclesDecrease = EnsureButton(panel, buttonTemplate, VehiclesDecreaseName, new Vector2(-250f, -335f), "-", alreadyAuthored);
        var vehiclesLabel = EnsureLabel(panel, labelTemplate, VehiclesLabelName, new Vector2(-10f, -335f), "Vehicules IA", alreadyAuthored);
        var vehiclesIncrease = EnsureButton(panel, buttonTemplate, VehiclesIncreaseName, new Vector2(240f, -335f), "+", alreadyAuthored);

        var litterDecrease = EnsureButton(panel, buttonTemplate, LitterDecreaseName, new Vector2(-250f, -405f), "-", alreadyAuthored);
        var litterLabel = EnsureLabel(panel, labelTemplate, LitterLabelName, new Vector2(-10f, -405f), "Jeteurs de detritus", alreadyAuthored);
        var litterIncrease = EnsureButton(panel, buttonTemplate, LitterIncreaseName, new Vector2(240f, -405f), "+", alreadyAuthored);

        var report = new System.Text.StringBuilder();
        var screenSerialized = new SerializedObject(screen);
        Assign(report, screenSerialized, "trafficVehiclesLabel", vehiclesLabel.GetComponent<TMP_Text>());
        Assign(report, screenSerialized, "trafficVehiclesDecreaseButton", vehiclesDecrease.GetComponent<Button>());
        Assign(report, screenSerialized, "trafficVehiclesIncreaseButton", vehiclesIncrease.GetComponent<Button>());
        Assign(report, screenSerialized, "litterThrowersLabel", litterLabel.GetComponent<TMP_Text>());
        Assign(report, screenSerialized, "litterThrowersDecreaseButton", litterDecrease.GetComponent<Button>());
        Assign(report, screenSerialized, "litterThrowersIncreaseButton", litterIncrease.GetComponent<Button>());
        screenSerialized.ApplyModifiedPropertiesWithoutUndo();

        var flowController = FindSceneComponent<LobbyFlowController>(scene);
        if (flowController == null)
        {
            return "LobbyFlowController introuvable : lignes creees mais Def non assigne.";
        }

        var trafficDef = AssetDatabase.LoadAssetAtPath<TrafficSettingsDef>(TrafficDefPath);
        if (trafficDef == null)
        {
            return "TrafficSettingsDef_Default.asset introuvable : lignes creees mais Def non assigne.";
        }

        var flowSerialized = new SerializedObject(flowController);
        Assign(report, flowSerialized, "trafficSettings", trafficDef);
        flowSerialized.ApplyModifiedPropertiesWithoutUndo();

        // Ne jamais sauver une scene a moitie cablee : la premiere execution de ce script l'a fait
        // (base d'assets perimee, tous les FindProperty nuls), et la scene paraissait authorée alors
        // qu'aucune reference n'etait posee. Mieux vaut echouer bruyamment que figer un demi-cablage.
        if (report.ToString().Contains("MANQUANT"))
        {
            return "Assignations manquantes : scene NON sauvee.\n" + report;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        var saved = EditorSceneManager.SaveScene(scene);
        return (alreadyAuthored ? "Lignes deja presentes : references reassignees. " : "Authoring termine. ")
            + "Scene sauvee : " + saved + "\n" + report;
    }

    private static void Assign(System.Text.StringBuilder report, SerializedObject target, string propertyName, Object value)
    {
        var property = target.FindProperty(propertyName);
        if (property == null)
        {
            report.AppendLine("MANQUANT: " + propertyName + " sur " + target.targetObject.GetType().Name);
            return;
        }

        if (value == null)
        {
            report.AppendLine("MANQUANT: composant pour " + propertyName);
            return;
        }

        property.objectReferenceValue = value;
        report.AppendLine("ok: " + propertyName + " -> " + (value != null ? value.name : "<null>"));
    }

    private static T FindSceneComponent<T>(Scene scene) where T : Component
    {
        var candidates = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var candidate in candidates)
        {
            if (candidate.gameObject.scene == scene)
            {
                return candidate;
            }
        }

        return null;
    }

    private static GameObject EnsureButton(Transform parent, Transform template, string name, Vector2 position, string glyph, bool reuseExisting)
    {
        if (reuseExisting)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }
        }

        var clone = Clone(parent, template, name, position, ButtonSize);
        var label = clone.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = glyph;
        }

        return clone;
    }

    private static GameObject EnsureLabel(Transform parent, Transform template, string name, Vector2 position, string text, bool reuseExisting)
    {
        if (reuseExisting)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }
        }

        var clone = Clone(parent, template, name, position, LabelSize);
        var label = clone.GetComponent<TMP_Text>();
        if (label != null)
        {
            label.text = text;
        }

        return clone;
    }

    private static GameObject Clone(Transform parent, Transform template, string name, Vector2 position, Vector2 size)
    {
        var clone = Object.Instantiate(template.gameObject, parent);
        clone.name = name;
        Undo.RegisterCreatedObjectUndo(clone, "Story 5.16 - lignes de reglage de trafic");

        var rect = (RectTransform)clone.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        return clone;
    }
}
