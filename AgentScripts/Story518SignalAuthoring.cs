using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Authoring des prefabs de module de la Story 5.18 : renseigne l'approche authoree sur les LaneNode de
/// decision du carrefour et de la jonction en T, et pose la signalisation Synty en enfant VISUEL.
///
/// Script hors Assets/ : compile par le pipeline, jamais importe par l'Editor, donc aucun reimport
/// d'asset ni rechargement de domaine pendant l'execution.
///
/// Pourquoi un script et pas une edition manuelle : les meshes Synty sont regeneres a chaque reimport du
/// pack, donc tout prefab qui les reference se reecrit. Un script rejouable est la seule forme d'authoring
/// qui survit a cette regeneration, et il tient les deux prefabs dans le meme etat.
///
/// La signalisation est posee comme MESH + MATERIAL copies, jamais comme instance de prefab Synty :
/// - un enfant visuel, sans aucun collider (la signalisation est decorative, AD-27) ;
/// - aucune instance imbriquee d'un prefab tiers dans un prefab du projet ;
/// - le motif deja en place sur <c>Greybox_CityBlock_A.prefab</c>, qui reference les meshes Synty
///   directement.
///
/// Idempotent : un noeud de signalisation deja present est conserve tel quel, et un LaneNode deja
/// renseigne est reecrit avec les memes valeurs.
/// </summary>
public static class Story518SignalAuthoring
{
    private const string IntersectionPath = "Assets/RoadRage/Prefabs/Greybox_Intersection.prefab";
    private const string TJunctionPath = "Assets/RoadRage/Prefabs/Greybox_TJunction.prefab";
    private const string LampMaterialPath = "Assets/RoadRage/Materials/Greybox_Signal_Mat.mat";
    private const string PropsRoot = "Assets/Synty/PolygonCity/Prefabs/Props/";
    private const string TrafficLightHeadPath = PropsRoot + "SM_Prop_TrafficLight_01.prefab";
    private const string SignalPolePath = PropsRoot + "SM_Prop_LightPole_Base_01.prefab";
    private const string StopSignPath = PropsRoot + "SM_Prop_Sign_Stop_01.prefab";

    /// <summary>
    /// Hauteur a laquelle le pack Synty attache ses unites sur ce mat : mesure du 2026-09-20 sur
    /// <c>SM_Prop_LightPole_Lights_01</c>, dont la base se trouve a 3,375 m. La tete de feu nue
    /// (<c>SM_Prop_TrafficLight_01</c>, 0,905 m, pivot en HAUT) n'a aucun mat : la poser au sol la
    /// reduirait a un plots de 0,9 m.
    /// </summary>
    private const float SignalHeadHeight = 3.375f;

    private const string IntersectionJunctionId = "junction_district_intersection";
    private const string TJunctionJunctionId = "junction_district_tjunction";

    private static readonly Vector3 LampScale = new Vector3(0.3f, 0.3f, 0.12f);

    public static string Run()
    {
        var report = new StringBuilder();

        var material = EnsureLampMaterial();
        if (material == null)
        {
            return "Materiau de lampe introuvable et non creable : authoring abandonne.";
        }

        report.AppendLine(AuthorIntersection(material));
        report.AppendLine(AuthorTJunction(material));

        AssetDatabase.SaveAssets();
        return report.ToString();
    }

    private static string AuthorIntersection(Material lampMaterial)
    {
        var contents = PrefabUtility.LoadPrefabContents(IntersectionPath);
        if (contents == null)
        {
            return IntersectionPath + " : chargement impossible.";
        }

        var report = new StringBuilder();
        try
        {
            var visualRoot = FindVisualRoot(contents);
            if (visualRoot == null)
            {
                return IntersectionPath + " : racine Visual_* introuvable, authoring abandonne.";
            }

            // Avenue nord-sud = groupe 0, avenue est-ouest = groupe 1. Les deux approches d'une meme
            // avenue sont opposees, donc elles ne se disputent pas la traversee et peuvent partager un
            // groupe -- c'est exactement ce qu'un carrefour a feux autorise.
            report.AppendLine(SetApproach(contents, "Junction_FromNorth", IntersectionJunctionId, JunctionApproachRule.TrafficLight, 0));
            report.AppendLine(SetApproach(contents, "Junction_FromSouth", IntersectionJunctionId, JunctionApproachRule.TrafficLight, 0));
            report.AppendLine(SetApproach(contents, "Junction_FromEast", IntersectionJunctionId, JunctionApproachRule.TrafficLight, 1));
            report.AppendLine(SetApproach(contents, "Junction_FromWest", IntersectionJunctionId, JunctionApproachRule.TrafficLight, 1));

            foreach (var approach in new[] { "North", "South", "East", "West" })
            {
                var group = approach == "North" || approach == "South" ? 0 : 1;
                var node = FindChild(contents, "Junction_From" + approach);
                if (node == null)
                {
                    report.AppendLine("MANQUANT: Junction_From" + approach);
                    continue;
                }

                report.AppendLine(PlaceSignalHead(visualRoot.transform, node.transform, "Visual_Signal_" + approach,
                    IntersectionJunctionId, group, lampMaterial, true));
            }

            PrefabUtility.SaveAsPrefabAsset(contents, IntersectionPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        return "--- Greybox_Intersection ---\n" + report;
    }

    private static string AuthorTJunction(Material lampMaterial)
    {
        var contents = PrefabUtility.LoadPrefabContents(TJunctionPath);
        if (contents == null)
        {
            return TJunctionPath + " : chargement impossible.";
        }

        var report = new StringBuilder();
        try
        {
            var visualRoot = FindVisualRoot(contents);
            if (visualRoot == null)
            {
                return TJunctionPath + " : racine Visual_* introuvable, authoring abandonne.";
            }

            // La voie traversante (ouest <-> est) porte la priorite ; la branche sud est la rue
            // secondaire, donc un stop. Aucune approche n'est au feu : la jonction en T n'a pas de plan.
            report.AppendLine(SetApproach(contents, "Junction_FromWest", TJunctionJunctionId, JunctionApproachRule.PriorityRoad, 0));
            report.AppendLine(SetApproach(contents, "Junction_FromEast", TJunctionJunctionId, JunctionApproachRule.PriorityRoad, 0));
            report.AppendLine(SetApproach(contents, "Junction_FromSouth", TJunctionJunctionId, JunctionApproachRule.Stop, 0));

            var stopNode = FindChild(contents, "Junction_FromSouth");
            if (stopNode == null)
            {
                report.AppendLine("MANQUANT: Junction_FromSouth");
            }
            else
            {
                report.AppendLine(PlaceSignalHead(visualRoot.transform, stopNode.transform, "Visual_Sign_Stop",
                    TJunctionJunctionId, 0, lampMaterial, false));
            }

            PrefabUtility.SaveAsPrefabAsset(contents, TJunctionPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        return "--- Greybox_TJunction ---\n" + report;
    }

    /// <summary>
    /// Renseigne l'approche authoree d'un LaneNode de decision par SerializedObject : les champs sont
    /// prives et serialises, donc c'est le contrat d'authoring reel qui est ecrit, pas un contournement.
    /// </summary>
    private static string SetApproach(GameObject contents, string nodeName, string junctionId,
        JunctionApproachRule rule, int signalGroup)
    {
        var node = FindChild(contents, nodeName);
        if (node == null)
        {
            return "MANQUANT: " + nodeName;
        }

        var laneNode = node.GetComponent<LaneNode>();
        if (laneNode == null)
        {
            return "MANQUANT: LaneNode sur " + nodeName;
        }

        var serialized = new SerializedObject(laneNode);
        var idProperty = serialized.FindProperty("junctionId");
        var ruleProperty = serialized.FindProperty("junctionRule");
        var groupProperty = serialized.FindProperty("signalGroup");
        if (idProperty == null || ruleProperty == null || groupProperty == null)
        {
            return "MANQUANT: champs d'approche sur " + nodeName
                + " (base d'assets perimee ? recompiler puis relancer).";
        }

        idProperty.stringValue = junctionId;
        ruleProperty.enumValueIndex = (int)rule;
        groupProperty.intValue = signalGroup;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return nodeName + " -> " + junctionId + " / " + rule + " / groupe " + signalGroup;
    }

    /// <summary>
    /// Pose une tete de signalisation sous la racine visuelle : le mesh Synty copie (jamais une instance
    /// de prefab tiers, jamais un collider), et -- pour un feu -- deux lampes que
    /// <see cref="TrafficSignalLampView"/> teinte selon la phase.
    ///
    /// Les lampes sont des cubes du projet, jamais les sous-materiaux du mesh Synty : mesure du
    /// 2026-09-20, le prop de feu porte UN SEUL materiau, donc ses lampes ne sont pas separables.
    /// </summary>
    private static string PlaceSignalHead(Transform visualRoot, Transform approachNode, string name,
        string junctionId, int signalGroup, Material lampMaterial, bool withLamps)
    {
        var existing = visualRoot.Find(name);
        if (existing != null)
        {
            // Reconstruction, pas conservation : c'est ce qui rend le script rejouable apres une
            // correction de geometrie. Rien ne reference ces enfants hors du composant de lampes.
            Object.DestroyImmediate(existing.gameObject);
        }

        var forward = approachNode.forward;
        forward.y = 0f;
        forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        var right = Vector3.Cross(Vector3.up, forward);

        // A droite de l'approche, juste avant le noeud de decision : l'endroit ou un conducteur regarde.
        var head = new GameObject(name);
        head.transform.SetParent(visualRoot, false);
        head.transform.position = approachNode.position + right * 3.2f - forward * 1.2f;
        // Tournee vers le conducteur qui arrive, donc dans le sens inverse de son cap.
        head.transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);

        var report = new StringBuilder();
        if (!withLamps)
        {
            report.Append(CopyProp(head.transform, "Synty_Panneau", StopSignPath, 0f, out var signHeight, out _, out _));
            return name + " : " + report + "panneau a y=0, hauteur " + signHeight.ToString("0.00") + " m, sans collider.";
        }

        report.Append(CopyProp(head.transform, "Synty_Mat", SignalPolePath, 0f, out _, out _, out _));
        report.Append(CopyProp(head.transform, "Synty_Feu", TrafficLightHeadPath, SignalHeadHeight,
            out var headHeight, out var centreX, out var frontZ));
        if (headHeight <= 0f)
        {
            return name + " : " + report;
        }

        // Lampes posees juste DEVANT la face que le conducteur voit : c'est l'extremite du mesh dans
        // l'axe qui pointe vers lui, donc la bonne quel que soit le "devant" artistique du prop.
        var lampZ = frontZ + 0.06f;
        var green = CreateLamp(head.transform, "Lamp_Green",
            new Vector3(centreX, SignalHeadHeight + 0.15f, lampZ), lampMaterial);
        var red = CreateLamp(head.transform, "Lamp_Red",
            new Vector3(centreX, SignalHeadHeight + headHeight - 0.25f, lampZ), lampMaterial);

        var view = head.AddComponent<TrafficSignalLampView>();
        var serialized = new SerializedObject(view);
        var missing = new StringBuilder();
        Set(serialized, missing, "junctionId", p => p.stringValue = junctionId);
        Set(serialized, missing, "signalGroup", p => p.intValue = signalGroup);
        Set(serialized, missing, "greenLamp", p => p.objectReferenceValue = green);
        Set(serialized, missing, "redLamp", p => p.objectReferenceValue = red);
        if (missing.Length > 0)
        {
            return "MANQUANT sur " + name + " : " + missing + " (base d'assets perimee ? recompiler puis relancer).";
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        return name + " : mat + tete a y=" + SignalHeadHeight.ToString("0.00") + ", lampes a y="
            + (SignalHeadHeight + 0.15f).ToString("0.00") + "/"
            + (SignalHeadHeight + headHeight - 0.25f).ToString("0.00")
            + ", groupe " + signalGroup + ", sans collider.";
    }

    /// <summary>
    /// Copie un prop Synty sous une tete de signalisation : mesh et materiaux, JAMAIS une instance de
    /// prefab tiers (donc aucune dependance imbriquee vers le pack), jamais un collider.
    /// </summary>
    private static string CopyProp(Transform parent, string name, string propPath, float baseY,
        out float height, out float centreX, out float frontZ)
    {
        height = 0f;
        centreX = 0f;
        frontZ = 0f;
        var prop = AssetDatabase.LoadAssetAtPath<GameObject>(propPath);
        if (prop == null)
        {
            return "MANQUANT: prop " + propPath + " (pack Synty non importe ?). ";
        }

        if (!MeasureProp(prop, out var baseOffset, out height, out centreX, out frontZ))
        {
            height = 0f;
            return "MANQUANT: mesure du prop " + propPath + ". ";
        }

        var sourceMesh = prop.GetComponentInChildren<MeshFilter>();
        var sourceRenderer = prop.GetComponentInChildren<MeshRenderer>();
        if (sourceMesh == null || sourceRenderer == null || sourceMesh.sharedMesh == null)
        {
            height = 0f;
            return "MANQUANT: mesh du prop " + propPath + ". ";
        }

        var copy = new GameObject(name);
        copy.transform.SetParent(parent, false);
        copy.transform.localScale = prop.transform.localScale;
        // Plusieurs props du pack ont leur PIVOT AU MILIEU ou EN HAUT (mesure du 2026-09-20 : la tete de
        // feu descend de 0,905 m sous son origine) : la copie est remontee de ce qu'elle descend, puis
        // posee a la hauteur demandee. Les bounds d'un renderer d'asset non instancie sont degeneres,
        // d'ou la mesure sur INSTANCE dans MeasureProp.
        copy.transform.localPosition = new Vector3(0f, baseY - baseOffset, 0f);
        var filter = copy.AddComponent<MeshFilter>();
        filter.sharedMesh = sourceMesh.sharedMesh;
        var renderer = copy.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = sourceRenderer.sharedMaterials;
        // Aucun collider n'est copie : la signalisation est decorative, et un collider dessus
        // condamnerait la jonction en permanence (la zone de degagement serait toujours occupee).
        return string.Empty;
    }

    /// <summary>
    /// Mesure un prop Synty sur une INSTANCE : position de sa base sous l'origine, hauteur totale,
    /// centre lateral et face la plus proche de l'observateur. Les <c>bounds</c> lus sur l'asset non
    /// instancie sont degeneres (mesure du 2026-09-18 sur les colliders, meme piege ici).
    /// </summary>
    private static bool MeasureProp(GameObject prop, out float baseOffset, out float height,
        out float centreX, out float frontZ)
    {
        baseOffset = 0f;
        height = 0f;
        centreX = 0f;
        frontZ = 0f;
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prop);
        if (instance == null)
        {
            return false;
        }

        try
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return false;
            }

            var min = renderers[0].bounds.min;
            var max = renderers[0].bounds.max;
            for (var i = 1; i < renderers.Length; i++)
            {
                min = Vector3.Min(min, renderers[i].bounds.min);
                max = Vector3.Max(max, renderers[i].bounds.max);
            }

            var origin = instance.transform.position;
            baseOffset = min.y - origin.y;
            height = max.y - min.y;
            centreX = ((min.x + max.x) * 0.5f) - origin.x;
            frontZ = max.z - origin.z;
            return true;
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    private static Renderer CreateLamp(Transform parent, string name, Vector3 localPosition, Material material)
    {
        var lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lamp.name = name;
        lamp.transform.SetParent(parent, false);
        lamp.transform.localPosition = localPosition;
        lamp.transform.localScale = LampScale;
        var collider = lamp.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        lamp.GetComponent<MeshRenderer>().sharedMaterial = material;
        return lamp.GetComponent<MeshRenderer>();
    }

    private static void Set(SerializedObject target, StringBuilder missing, string propertyName, System.Action<SerializedProperty> assign)
    {
        var property = target.FindProperty(propertyName);
        if (property == null)
        {
            missing.Append(propertyName).Append(' ');
            return;
        }

        assign(property);
    }

    private static Transform FindVisualRoot(GameObject contents)
    {
        foreach (var child in contents.GetComponentsInChildren<Transform>(true))
        {
            if (child != contents.transform && child.name.StartsWith("Visual_", System.StringComparison.Ordinal))
            {
                return child;
            }
        }

        return null;
    }

    private static GameObject FindChild(GameObject contents, string name)
    {
        foreach (var child in contents.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static Material EnsureLampMaterial()
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(LampMaterialPath);
        if (existing != null)
        {
            return existing;
        }

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader) { name = "Greybox_Signal_Mat" };
        AssetDatabase.CreateAsset(material, LampMaterialPath);
        return material;
    }
}
