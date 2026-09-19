using System.Collections.Generic;
using System.Linq;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lecture seule : decrit ce que sont et ce que bloquent les deux objets `MVP_RageTargetVehicle_*`
/// de `MVP_Run`. Aucune mutation, aucune sauvegarde.
/// </summary>
public static class Story513RageTargetInspection
{
    private const string ScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

    public static object Inspect()
    {
        var scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid())
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        var transforms = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .ToList();

        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<LaneNode>(true))
            .ToList();

        var roads = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
            .Where(c => c.name == "Col_Roadway")
            .ToList();

        var report = new List<object>();

        foreach (var target in transforms.Where(t => t.name.StartsWith("MVP_RageTargetVehicle", System.StringComparison.Ordinal)))
        {
            var position = target.position;

            var components = target.GetComponents<Component>()
                .Select(c => c.GetType().Name)
                .Concat(target.GetComponentsInChildren<Component>(true).Select(c => c.GetType().Name))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var nearestNode = nodes
                .OrderBy(n => (n.transform.position - position).sqrMagnitude)
                .FirstOrDefault();

            // Test sur x/z seulement : un point a y = 0,01 sort des bornes d'une chaussee dont le
            // sommet est a y = 0, donc un `Bounds.Contains` complet rendrait un faux « hors chaussee ».
            var roadsAtPosition = roads
                .Where(c => position.x >= c.bounds.min.x - 0.01f && position.x <= c.bounds.max.x + 0.01f
                    && position.z >= c.bounds.min.z - 0.01f && position.z <= c.bounds.max.z + 0.01f)
                .Select(c => new
                {
                    owner = HierarchyPath(c.transform),
                    sizeX = c.bounds.size.x,
                    sizeZ = c.bounds.size.z,
                    minX = c.bounds.min.x,
                    maxX = c.bounds.max.x,
                    minZ = c.bounds.min.z,
                    maxZ = c.bounds.max.z,
                })
                .ToList();

            report.Add(new
            {
                name = target.name,
                hierarchyPath = HierarchyPath(target),
                position = new { x = position.x, y = position.y, z = position.z },
                isSceneRoot = target.parent == null,
                prefabStatus = PrefabUtility.GetPrefabInstanceStatus(target.gameObject).ToString(),
                sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(target.gameObject) == null
                    ? "(aucun)"
                    : PrefabUtility.GetCorrespondingObjectFromSource(target.gameObject).name,
                components,
                nearestLaneNode = nearestNode == null ? "(aucun)" : nearestNode.name,
                nearestLaneNodeDistance = nearestNode == null ? -1f : Vector3.Distance(nearestNode.transform.position, position),
                roadwaysContainingPosition = roadsAtPosition,
                hasPhysicsBodyWithProfile = target.GetComponentInChildren<VehiclePhysicsBody>(true) != null
                    && target.GetComponentInChildren<VehiclePhysicsBody>(true).HasProfile,
            });
        }

        return new
        {
            scene = ScenePath,
            sceneIsDirty = scene.isDirty,
            roadwayColliderCount = roads.Count,
            targets = report,
        };
    }

    private static string HierarchyPath(Transform transform)
    {
        var parts = new List<string>();
        var current = transform;
        while (current != null)
        {
            parts.Insert(0, current.name);
            current = current.parent;
        }

        return string.Join("/", parts);
    }
}
