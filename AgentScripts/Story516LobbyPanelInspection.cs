using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lecture seule : etat authore du panneau de lobby de la Story 5.16 (MainMenuLobby).
///
/// Script hors Assets/ : compile par le pipeline, jamais importe par l'Editor, donc aucun reimport
/// d'asset ni rechargement de domaine. Il ne modifie rien -- il sert a connaitre les RectTransform
/// existants avant d'ajouter les deux lignes de reglage de trafic.
/// </summary>
public static class Story516LobbyPanelInspection
{
    public static string Run()
    {
        var screen = Object.FindFirstObjectByType<RoadRage.Features.UI.LobbyRosterScreen>(FindObjectsInactive.Include);
        if (screen == null)
        {
            return "LobbyRosterScreen introuvable dans les scenes chargees.";
        }

        var canvas = screen.GetComponentInParent<Canvas>();
        var builder = new StringBuilder();
        builder.AppendLine("Canvas: " + (canvas != null ? canvas.name : "<aucun>")
            + " renderMode=" + (canvas != null ? canvas.renderMode.ToString() : "?")
            + " scaler=" + Describe(canvas != null ? canvas.GetComponent<CanvasScaler>() : null));

        Append(builder, screen.transform, screen.transform, 0);
        return builder.ToString();
    }

    private static void Append(StringBuilder builder, Transform node, Transform root, int depth)
    {
        var rect = node as RectTransform;
        builder.Append(new string(' ', depth * 2));
        builder.Append(node.name);

        if (rect != null)
        {
            builder.Append(" | aMin=").Append(Short(rect.anchorMin));
            builder.Append(" aMax=").Append(Short(rect.anchorMax));
            builder.Append(" pivot=").Append(Short(rect.pivot));
            builder.Append(" pos=").Append(Short(rect.anchoredPosition));
            builder.Append(" size=").Append(Short(rect.sizeDelta));
            builder.Append(" rect=").Append(Short(rect.rect.size));
        }

        builder.Append(" | activeSelf=").Append(node.gameObject.activeSelf);
        builder.Append(" | components=");

        var components = node.GetComponents<Component>();
        for (var i = 0; i < components.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            builder.Append(components[i] == null ? "<null>" : components[i].GetType().Name);
        }

        var text = node.GetComponent<TMP_Text>();
        if (text != null)
        {
            builder.Append(" text='").Append(text.text).Append('\'');
        }

        builder.AppendLine();

        for (var i = 0; i < node.childCount; i++)
        {
            Append(builder, node.GetChild(i), root, depth + 1);
        }
    }

    private static string Describe(CanvasScaler scaler)
    {
        if (scaler == null)
        {
            return "<aucun>";
        }

        return scaler.uiScaleMode + " ref=" + scaler.referenceResolution + " match=" + scaler.matchWidthOrHeight;
    }

    private static string Short(Vector2 value)
    {
        return "(" + value.x.ToString("0.##") + "," + value.y.ToString("0.##") + ")";
    }
}
