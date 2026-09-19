using System;
using System.Collections.Generic;
using System.Linq;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Authoring du relief de recette de la Story 5.13 (dos-d'ane + marche basse) dans MVP_Run.
///
/// Script hors Assets/ : il est compile par le pipeline, pas importe par l'Editor, donc aucun
/// reimport d'asset ni rechargement de domaine.
///
/// Contrat vise, tire de la fixture
/// `Assets/RoadRage/Tests/EditMode/Story513ArcadeAssistsAndUnevenGroundTests.cs` :
///   - les deux objets vivent SOUS `GreyboxMap` (enfants directs) et sont des objets de SCENE ;
///   - l'emprise est l'union des AABB monde des colliders ;
///   - crete a 0,12 m exactement, base a y &lt;= 0, pleine largeur de chaussee (8 m), centree en z ;
///   - dos-d'ane : exactement 2 colliders (deux rampes), longueur x dans [1,6 ; 6[ ;
///   - chaque descendant ne porte que Transform / MeshFilter / MeshRenderer / BoxCollider ;
///   - aucun LaneNode dans l'emprise ;
///   - l'emprise reste dans celle de `Col_Roadway`.
///
/// Toute condition non tenue interrompt le script AVANT d'ecrire quoi que ce soit.
/// </summary>
public static class Story513RecipeRelief
{
    private const string ScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
    private const string AvenueName = "Avenue_CenterToEast";
    private const string MapName = "GreyboxMap";
    private const string BumpName = "Relief_DosDane_AvenueCenterToEast";
    private const string StepName = "Relief_MarcheBasse_AvenueCenterToEast";
    private const float CurbHeight = 0.12f;

    public static object Author()
    {
        var notes = new List<string>();

        var scene = SceneManager.GetSceneByPath(ScenePath);
        var wasOpen = scene.IsValid();
        if (!wasOpen)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        notes.Add("scene " + (wasOpen ? "deja ouverte" : "ouverte en additif"));

        // ---------------------------------------------------------------- cadrage mesure

        var all = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .ToList();

        var map = all.FirstOrDefault(t => t.name == MapName && t.parent != null && t.parent.name == "RunRoot");
        if (map == null)
        {
            throw new InvalidOperationException(MapName + " introuvable sous RunRoot : le relief doit vivre la.");
        }

        var laneGraph = all.FirstOrDefault(t => t.name == "LaneGraph");
        if (laneGraph == null)
        {
            throw new InvalidOperationException("LaneGraph introuvable dans " + ScenePath);
        }

        var avenue = laneGraph.Find(AvenueName);
        if (avenue == null)
        {
            throw new InvalidOperationException(AvenueName + " introuvable sous LaneGraph");
        }

        var roadwayTransform = avenue.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Col_Roadway");
        if (roadwayTransform == null)
        {
            throw new InvalidOperationException("Col_Roadway introuvable sous " + AvenueName);
        }

        var roadway = roadwayTransform.GetComponent<Collider>();
        if (roadway == null)
        {
            throw new InvalidOperationException("Col_Roadway ne porte aucun Collider");
        }

        var road = roadway.bounds;
        notes.Add("Col_Roadway x " + F(road.min.x) + " .. " + F(road.max.x)
            + " | z " + F(road.min.z) + " .. " + F(road.max.z)
            + " | y " + F(road.min.y) + " .. " + F(road.max.y));
        notes.Add("rotation monde de l'avenue " + avenue.rotation.eulerAngles.ToString("F3"));

        // Le contrat de la fixture suppose la chaussee alignee sur les axes monde : largeur en z,
        // longueur en x, centree en z = 0. On le verifie au lieu de l'esperer.
        if (road.size.z <= 0f)
        {
            throw new InvalidOperationException("Col_Roadway a une largeur nulle en z");
        }

        if (Mathf.Abs(road.size.z - 8f) > 0.005f)
        {
            throw new InvalidOperationException("Col_Roadway mesure " + F(road.size.z)
                + " m de large en z, or la fixture attend 8 m : le relief ne peut pas satisfaire les deux, on n'ecrit rien.");
        }

        if (Mathf.Abs(road.center.z) > 0.01f)
        {
            throw new InvalidOperationException("Col_Roadway n'est pas centre en z (centre " + F(road.center.z) + ")");
        }

        if (avenue.rotation.eulerAngles.y % 90f > 0.05f && Mathf.Abs(avenue.rotation.eulerAngles.y % 90f - 90f) > 0.05f)
        {
            throw new InvalidOperationException("L'avenue n'est pas alignee sur les axes monde (yaw "
                + F(avenue.rotation.eulerAngles.y) + ") : le relief devrait etre tourne, et la garde de largeur en z echouerait.");
        }

        var halfWidth = road.size.z * 0.5f;

        // ---------------------------------------------------------------- geometrie du dos-d'ane

        const float Slope = 0.10f;                        // pente de chaque rampe : 10 %, la borne de la fixture est 15 %
        const float RampRun = CurbHeight / Slope;         // 1,20 m de course horizontale par rampe
        const float SlabThickness = 0.08f;                // epaisseur de la dalle
        var theta = Mathf.Atan(Slope) * Mathf.Rad2Deg;    // 5,71 deg
        var slabLength = Mathf.Sqrt((RampRun * RampRun) + (CurbHeight * CurbHeight));

        var half = (slabLength * Mathf.Cos(theta * Mathf.Deg2Rad)) + (SlabThickness * Mathf.Sin(theta * Mathf.Deg2Rad));
        // Le coin haut de la face superieure culmine a 0,12 m : c'est le point le plus haut de la dalle,
        // donc le max.y de son AABB. Le coin bas de la face inferieure descend sous le plan de roulage.
        var slabMaxY = CurbHeight;
        var slabMinY = CurbHeight - CurbHeight - (SlabThickness * Mathf.Cos(theta * Mathf.Deg2Rad));
        notes.Add("dalle : L " + F(slabLength) + " m, pente " + F(Slope * 100f) + " %, theta " + F(theta)
            + " deg, AABB x +/-" + F(half) + " m, y " + F(slabMinY) + " .. " + F(slabMaxY));

        // ---------------------------------------------------------------- fenetres libres de noeud

        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<LaneNode>(true))
            .Select(n => n.transform.position)
            .ToList();

        if (nodes.Count == 0)
        {
            throw new InvalidOperationException("Aucun LaneNode dans " + ScenePath + " : la garde de graphe ne garderait rien.");
        }

        var xMin = road.min.x + 0.01f;
        var xMax = road.max.x - 0.01f;

        var bumpCentreX = float.NaN;
        var stepCentreX = float.NaN;

        for (var x = xMin + half + 1f; x <= xMax - half - 1f; x += 0.2f)
        {
            if (!IsClear(x - half, x + half, nodes, road))
            {
                continue;
            }

            if (float.IsNaN(bumpCentreX))
            {
                bumpCentreX = x;
                continue;
            }

            if (Mathf.Abs(x - bumpCentreX) >= 4f && IsClear(x - 0.2f, x + 0.2f, nodes, road))
            {
                stepCentreX = x;
                break;
            }
        }

        if (float.IsNaN(bumpCentreX) || float.IsNaN(stepCentreX))
        {
            throw new InvalidOperationException("Aucune fenetre libre de LaneNode trouvee sur " + AvenueName
                + " pour poser le dos-d'ane et la marche basse : rien n'est ecrit. Etendre l'avenue ou degager un noeud est une decision humaine.");
        }

        notes.Add("dos-d'ane centre a x = " + F(bumpCentreX) + ", marche basse a x = " + F(stepCentreX));

        // ---------------------------------------------------------------- ecriture

        var existing = new[] { BumpName, StepName };
        foreach (var name in existing)
        {
            var stale = map.Find(name);
            if (stale != null)
            {
                UnityEngine.Object.DestroyImmediate(stale.gameObject);
                notes.Add("objet " + name + " remplace");
            }
        }

        var bump = new GameObject(BumpName);
        Undo.RegisterCreatedObjectUndo(bump, "Story 5.13 relief");
        bump.transform.SetParent(map, false);
        // Le parent se pose SUR LE PLAN DE ROULAGE (y = 0) : ses enfants expriment la crete a +0,12 m
        // depuis ce plan. Poser le parent sur la crete remonterait tout le relief d'exactement 0,12 m --
        // ce que le controle ci-dessous avait attrape.
        bump.transform.position = new Vector3(bumpCentreX, 0f, road.center.z);

        CreateSlab(bump.transform, "Rampe_Ouest", new Vector3(-(slabLength * 0.5f * Mathf.Cos(theta * Mathf.Deg2Rad)) + ((SlabThickness * 0.5f) * Mathf.Sin(theta * Mathf.Deg2Rad)),
                CurbHeight - (0.5f * CurbHeight) - ((SlabThickness * 0.5f) * Mathf.Cos(theta * Mathf.Deg2Rad)), 0f),
            new Vector3(slabLength, SlabThickness, road.size.z), theta);

        CreateSlab(bump.transform, "Rampe_Est", new Vector3((slabLength * 0.5f * Mathf.Cos(theta * Mathf.Deg2Rad)) - ((SlabThickness * 0.5f) * Mathf.Sin(theta * Mathf.Deg2Rad)),
                CurbHeight - (0.5f * CurbHeight) - ((SlabThickness * 0.5f) * Mathf.Cos(theta * Mathf.Deg2Rad)), 0f),
            new Vector3(slabLength, SlabThickness, road.size.z), -theta);

        var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
        step.name = StepName;
        Undo.RegisterCreatedObjectUndo(step, "Story 5.13 relief");
        step.transform.SetParent(map, false);
        step.transform.position = new Vector3(stepCentreX, CurbHeight * 0.5f, road.center.z);
        step.transform.rotation = Quaternion.identity;
        step.transform.localScale = new Vector3(0.4f, CurbHeight, road.size.z);

        EditorSceneManager.MarkSceneDirty(scene);

        // Piege mesure : `Collider.bounds` n'est rafraichi qu'a la synchronisation physique. Sans cet
        // appel, la relecture rend les emprises d'ORIGINE des primitives (cube unite a l'origine), et le
        // controle croirait que le relief n'a pas ete place alors qu'il l'est.
        Physics.SyncTransforms();

        // ---------------------------------------------------------------- relecture et controle avant sauvegarde

        var bumpFootprint = Footprint(bump);
        var stepFootprint = Footprint(step);

        var problems = new List<string>();
        Check(problems, Mathf.Abs(bumpFootprint.max.y - CurbHeight) <= 0.002f, "crete du dos-d'ane " + F(bumpFootprint.max.y));
        Check(problems, Mathf.Abs(stepFootprint.max.y - CurbHeight) <= 0.002f, "haut de la marche " + F(stepFootprint.max.y));
        Check(problems, bumpFootprint.min.y <= 0f, "base du dos-d'ane " + F(bumpFootprint.min.y));
        Check(problems, stepFootprint.min.y <= 0f, "base de la marche " + F(stepFootprint.min.y));
        Check(problems, Mathf.Abs(bumpFootprint.size.z - 8f) <= 0.01f, "largeur du dos-d'ane " + F(bumpFootprint.size.z));
        Check(problems, Mathf.Abs(stepFootprint.size.z - 8f) <= 0.01f, "largeur de la marche " + F(stepFootprint.size.z));
        Check(problems, Mathf.Abs(bumpFootprint.center.z) <= 0.01f, "centrage z du dos-d'ane " + F(bumpFootprint.center.z));
        Check(problems, Mathf.Abs(stepFootprint.center.z) <= 0.01f, "centrage z de la marche " + F(stepFootprint.center.z));
        Check(problems, bumpFootprint.size.x * 0.15f >= (2f * CurbHeight) - 0.0001f, "pente moyenne du dos-d'ane sur " + F(bumpFootprint.size.x) + " m");
        Check(problems, bumpFootprint.size.x < 6f, "longueur du dos-d'ane " + F(bumpFootprint.size.x));
        Check(problems, bump.GetComponentsInChildren<Collider>(true).Length == 2, "nombre de colliders du dos-d'ane " + bump.GetComponentsInChildren<Collider>(true).Length);
        Check(problems, Inside(road, bumpFootprint), "dos-d'ane hors chaussee : x " + F(bumpFootprint.min.x) + " .. " + F(bumpFootprint.max.x));
        Check(problems, Inside(road, stepFootprint), "marche hors chaussee : x " + F(stepFootprint.min.x) + " .. " + F(stepFootprint.max.x));
        Check(problems, !nodes.Any(n => bumpFootprint.Contains(new Vector3(n.x, bumpFootprint.center.y, n.z))), "un LaneNode tombe dans le dos-d'ane");
        Check(problems, !nodes.Any(n => stepFootprint.Contains(new Vector3(n.x, stepFootprint.center.y, n.z))), "un LaneNode tombe dans la marche");
        Check(problems, PrefabUtility.GetPrefabInstanceStatus(bump) == PrefabInstanceStatus.NotAPrefab, "le dos-d'ane n'est pas un objet de scene");
        Check(problems, PrefabUtility.GetPrefabInstanceStatus(step) == PrefabInstanceStatus.NotAPrefab, "la marche n'est pas un objet de scene");
        Check(problems, !map.GetComponentsInChildren<Transform>(true).Any(t => t.name.StartsWith("Col_", StringComparison.Ordinal) && (t.name == BumpName || t.parent == bump.transform)), "un enfant du relief porte un nom Col_*");

        if (problems.Count > 0)
        {
            throw new InvalidOperationException("Controle avant sauvegarde en echec, la scene n'est PAS sauvegardee : " + string.Join(" | ", problems));
        }

        var saved = EditorSceneManager.SaveScene(scene);
        notes.Add("scene sauvegardee : " + saved);

        return new
        {
            scene = ScenePath,
            sceneSaved = saved,
            bump = new { x = bumpCentreX, footprintMinX = bumpFootprint.min.x, footprintMaxX = bumpFootprint.max.x, maxY = bumpFootprint.max.y, minY = bumpFootprint.min.y, sizeZ = bumpFootprint.size.z, centreZ = bumpFootprint.center.z, colliders = bump.GetComponentsInChildren<Collider>(true).Length },
            step = new { x = stepCentreX, maxY = stepFootprint.max.y, minY = stepFootprint.min.y, sizeZ = stepFootprint.size.z, centreZ = stepFootprint.center.z },
            roadway = new { minX = road.min.x, maxX = road.max.x, minZ = road.min.z, maxZ = road.max.z },
            laneNodeCount = nodes.Count,
            notes,
        };
    }

    private static void CreateSlab(Transform parent, string name, Vector3 localPosition, Vector3 size, float rollDegrees)
    {
        var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slab.name = name;
        Undo.RegisterCreatedObjectUndo(slab, "Story 5.13 relief");
        slab.transform.SetParent(parent, false);
        slab.transform.localPosition = localPosition;
        slab.transform.localRotation = Quaternion.Euler(0f, 0f, rollDegrees);
        slab.transform.localScale = size;
    }

    /// <summary>Aucun noeud de voie dans la bande [xMin ; xMax] de la chaussee.</summary>
    private static bool IsClear(float xMin, float xMax, List<Vector3> nodes, Bounds road)
    {
        foreach (var node in nodes)
        {
            if (node.z < road.min.z - 0.001f || node.z > road.max.z + 0.001f)
            {
                continue;
            }

            if (node.x >= xMin && node.x <= xMax)
            {
                return false;
            }
        }

        return true;
    }

    private static bool Inside(Bounds outer, Bounds inner)
    {
        return inner.min.x >= outer.min.x - 0.01f
            && inner.max.x <= outer.max.x + 0.01f
            && inner.min.z >= outer.min.z - 0.01f
            && inner.max.z <= outer.max.z + 0.01f;
    }

    private static Bounds Footprint(GameObject root)
    {
        var colliders = root.GetComponentsInChildren<Collider>(true);
        var bounds = colliders[0].bounds;
        for (var i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        return bounds;
    }

    private static void Check(List<string> problems, bool condition, string label)
    {
        if (!condition)
        {
            problems.Add(label);
        }
    }

    private static string F(float value)
    {
        return value.ToString("F4");
    }
}
