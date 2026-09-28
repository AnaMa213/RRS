using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Story 5.51 : regeneration des preuves finales hors tests -- entrees V1/V2 (version, empreintes,
/// compteurs), mesure de degagement au repos (algorithme courant), audit des douze angles, empreintes
/// de fichiers et lien NavMesh recharge. N'ecrit jamais dans la scene ; le fichier de sortie est
/// ecrit par sections (le delai serveur de run_script peut couper la reponse, jamais le travail).
/// </summary>
public static class Story551FinalProof
{
    public static string Run()
    {
        var root = Directory.GetParent(Application.dataPath).FullName;
        var output = Path.Combine(root, "_bmad-output", "implementation-artifacts", "v1-regression-5-51", "final-proof.txt");
        var text = new StringBuilder();
        try
        {
            var scene = SceneManager.GetSceneByPath("Assets/RoadRage/App/Scenes/MVP_Run.unity");
            text.Append("scene=MVP_Run loaded=").Append(scene.IsValid() && scene.isLoaded)
                .Append(" dirty=").Append(scene.IsValid() ? scene.isDirty.ToString() : "-").Append('\n');
            Flush(text, output);

            text.Append("\n== entrees V1/V2 ==\n");
            string lineage = File.ReadAllText(MigrationReport.LineageFullPath);
            string decisions = File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath));
            var run = AuthoredRoadModel.Run(scene, lineage, decisions);
            text.Append("succeeded=").Append(run.Succeeded).Append('\n');
            foreach (string failure in run.Failures) text.Append("failure: ").Append(failure).Append('\n');
            if (run.Binding != null)
            {
                text.Append("road-model-version=").Append(run.Binding.RoadModelVersion).Append('\n');
                text.Append("source-hash=").Append(run.Binding.SourceHash).Append('\n');
                text.Append("lineage-hash=").Append(run.Binding.LineageHash).Append('\n');
                text.Append("decisions-hash=").Append(run.Binding.DecisionsHash).Append('\n');
                text.Append("model-hash=").Append(run.Binding.ModelHash).Append('\n');
                text.Append("overlay-hash=").Append(run.Binding.OverlayHash).Append('\n');
            }
            if (run.Source != null)
            {
                text.Append("mouvements-compiles=").Append(run.Source.Movements.Length).Append('\n');
                text.Append("zones=").Append(run.Source.ConflictZones.Length).Append('\n');
            }
            text.Append("candidats=").Append(run.Candidates == null ? -1 : run.Candidates.Count).Append('\n');
            Flush(text, output);

            text.Append("\n== mesure de degagement (algorithme ").Append(JunctionClearance.AlgorithmVersion).Append(") ==\n");
            var readFailures = new List<string>();
            var sidewalks = SidewalkDeclarations.Read(scene, readFailures);
            foreach (string failure in readFailures) text.Append("lecture-sidewalk: ").Append(failure).Append('\n');
            var compiled = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.ModelPath))));
            var result = JunctionClearance.Measure(scene, run.Import, compiled, sidewalks);
            text.Append("pas=").Append(result.StepMeters.ToString("R", CultureInfo.InvariantCulture))
                .Append(" garde=").Append(result.ReliefClearanceMeters.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
            text.Append("lignes=").Append(result.Rows.Count)
                .Append(" echecs=").Append(result.Failures.Count)
                .Append(" passed=").Append(result.Passed).Append('\n');
            text.Append("empreinte-physique=").Append(result.PhysicalFingerprint).Append('\n');
            text.Append("empreinte-semantique=").Append(result.SemanticFingerprint).Append('\n');
            foreach (string failure in result.Failures) text.Append("echec: ").Append(failure).Append('\n');
            foreach (var relief in result.DrivableReliefs)
                text.Append("relief: ").Append(relief.Collider)
                    .Append(" hauteur=").Append(relief.HeightMeters.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
            foreach (string junction in result.Rows.Select(r => r.Junction).Distinct().OrderBy(j => j, StringComparer.Ordinal))
            {
                var rows = result.Rows.Where(r => r.Junction == junction).ToArray();
                text.Append("jonction: ").Append(junction)
                    .Append(" lignes=").Append(rows.Length)
                    .Append(" min-physique=").Append(rows.Min(r => r.Physical.Residual).ToString("R", CultureInfo.InvariantCulture))
                    .Append(" min-semantique=").Append(rows.Min(r => r.Semantic.Residual).ToString("R", CultureInfo.InvariantCulture))
                    .Append(" vides=").Append(rows.Count(r => r.PhysicalSetEmpty)).Append('\n');
            }
            Flush(text, output);

            foreach (var row in result.Rows)
            {
                text.Append("ligne: ").Append(row.Junction).Append(" | ").Append(row.Movement).Append(" | ").Append(row.Surface)
                    .Append(" | P=").Append(row.Physical.Residual.ToString("R", CultureInfo.InvariantCulture))
                    .Append(row.PhysicalSetEmpty ? " (ensemble vide admis)" : string.Empty)
                    .Append(" @").Append(row.Physical.Position.ToString("F3", CultureInfo.InvariantCulture))
                    .Append(" obst=").Append(row.Physical.Obstacle ?? "-").Append(row.Physical.Seam ? " couture" : string.Empty)
                    .Append(" | S=").Append(row.Semantic.Residual.ToString("R", CultureInfo.InvariantCulture))
                    .Append(" @").Append(row.Semantic.Position.ToString("F3", CultureInfo.InvariantCulture))
                    .Append(" obst=").Append(row.Semantic.Obstacle ?? "-").Append(row.Semantic.Seam ? " couture" : string.Empty)
                    .Append('\n');
            }
            Flush(text, output);

            text.Append("\n== audit des angles ==\n");
            foreach (string module in new[] { "Intersection_Center_Crossroads", "TJunction_South", "TJunction_North", "TJunction_East", "TJunction_West" })
            {
                var moduleRoot = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                    .FirstOrDefault(t => t.name == module && t.parent != null && t.parent.name == "LaneGraph");
                if (moduleRoot == null)
                {
                    text.Append(module).Append(": INTROUVABLE\n");
                    continue;
                }

                foreach (Transform child in moduleRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (!child.name.StartsWith("Chamfer_", StringComparison.Ordinal) && !child.name.StartsWith("Col_Curb_", StringComparison.Ordinal)) continue;
                    Collider collider = child.GetComponent<Collider>();
                    Renderer renderer = child.GetComponent<Renderer>();
                    text.Append(module).Append(" | ").Append(child.name)
                        .Append(" | collider=").Append(collider == null ? "aucun" : collider.enabled + " trigger=" + collider.isTrigger)
                        .Append(" | renderer=").Append(renderer == null ? "aucun" : renderer.enabled + " materiau=" + (renderer.sharedMaterial == null ? "null" : renderer.sharedMaterial.name))
                        .Append('\n');
                }
            }
            Flush(text, output);

            text.Append("\n== fichiers et NavMesh ==\n");
            AppendHash(text, root, "Assets/RoadRage/App/Scenes/MVP_Run.unity");
            AppendHash(text, root, "Assets/RoadRage/App/Scenes/MVP_Run/NavMesh-LaneGraph-Vehicle.asset");
            string metaPath = Path.Combine(root, "Assets/RoadRage/App/Scenes/MVP_Run/NavMesh-LaneGraph-Vehicle.asset.meta");
            string metaGuid = File.Exists(metaPath)
                ? File.ReadAllLines(metaPath).FirstOrDefault(l => l.StartsWith("guid:", StringComparison.Ordinal))
                : null;
            text.Append("navmesh-meta=").Append(metaGuid ?? "ABSENT").Append('\n');
            foreach (var component in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Component>(true)))
            {
                if (component == null || component.GetType().FullName != "Unity.AI.Navigation.NavMeshSurface") continue;
                var serialized = new SerializedObject(component);
                var data = serialized.FindProperty("m_NavMeshData").objectReferenceValue;
                string path = data == null ? "null" : AssetDatabase.GetAssetPath(data);
                text.Append("navmesh-surface=").Append(path);
                if (data != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(data, out string guid, out long fileId))
                {
                    text.Append(" guid=").Append(guid).Append(" fileId=").Append(fileId);
                    if (metaGuid != null) text.Append(" guid-meta-egale=").Append(metaGuid.EndsWith(guid, StringComparison.Ordinal));
                }
                text.Append('\n');
            }
        }
        catch (Exception exception)
        {
            text.Append("EXCEPTION: ").Append(exception).Append('\n');
        }

        Flush(text, output);
        return output;
    }

    private static void Flush(StringBuilder text, string output)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.AppendAllText(output, text.ToString());
        text.Clear();
    }

    private static void AppendHash(StringBuilder text, string root, string projectRelative)
    {
        string path = Path.Combine(root, projectRelative);
        if (!File.Exists(path))
        {
            text.Append("hash ").Append(projectRelative).Append("=ABSENT\n");
            return;
        }

        using (var sha = System.Security.Cryptography.SHA256.Create())
        using (var stream = File.OpenRead(path))
        {
            text.Append("hash ").Append(projectRelative).Append('=')
                .Append(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty)).Append('\n');
        }
    }
}
