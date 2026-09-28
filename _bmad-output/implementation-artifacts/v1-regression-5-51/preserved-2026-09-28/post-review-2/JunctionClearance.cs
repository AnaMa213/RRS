#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>
    /// Surface Sidewalk authoree, fournie par un adaptateur Editor hors Traffic (AD-33) : ce mesureur
    /// ne lit jamais lui-meme la classification V1.
    /// </summary>
    public sealed class JunctionClearanceSurface
    {
        public string Name;
        public Component Declaration;

        /// <summary>Champs authores canoniques de la declaration, inclus dans l'empreinte semantique.</summary>
        public string DeclarationSignature;

        /// <summary>Colliders declares, actifs ou non : la semantique ne depend pas de leur activation.</summary>
        public readonly List<Collider> Colliders = new List<Collider>();
    }

    public sealed class JunctionClearanceWitness
    {
        public float Residual = float.PositiveInfinity;
        public Vector3 Position;
        public string Obstacle;
        public bool Seam;
        public float IntervalDelta;
        public float LargestDelta;
    }

    public sealed class JunctionClearanceRow
    {
        public string Junction;
        public string Movement;
        public string Surface;
        public JunctionClearanceWitness Physical;
        public JunctionClearanceWitness Semantic;

        /// <summary>Aucun obstacle physique participant n'a ete trouve pour ce mouvement ; le residu +infini est alors intentionnel.</summary>
        public bool PhysicalSetEmpty { get { return Physical != null && float.IsPositiveInfinity(Physical.Residual) && string.IsNullOrEmpty(Physical.Obstacle); } }
    }

    /// <summary>Relief routier franchissable retire du gate physique, publie pour la tracabilite.</summary>
    public sealed class JunctionClearanceRelief
    {
        public string Collider;
        public float HeightMeters;
        public float ClearanceMeters;
    }

    public sealed class JunctionClearanceResult
    {
        public float StepMeters;
        public float VehicleTopMeters;

        /// <summary>Garde au sol statique du vehicule IA : hauteur maximale d'un relief routier franchissable.</summary>
        public float ReliefClearanceMeters;

        public readonly List<JunctionClearanceRelief> DrivableReliefs = new List<JunctionClearanceRelief>();
        public string PhysicalFingerprint;
        public string SemanticFingerprint;
        public readonly List<JunctionClearanceRow> Rows = new List<JunctionClearanceRow>();
        public readonly List<string> Failures = new List<string>();

        public bool Passed { get { return Failures.Count == 0 && Rows.Count > 0 && Rows.All(r => r.Physical.Residual > 0f && r.Semantic.Residual > 0f); } }
    }

    /// <summary>Preuve EditMode 5.51 : deux gates independants sur les courbes compilees 5.50.</summary>
    public static class JunctionClearance
    {
        public const int AlgorithmVersion = 5; // 2 : relief routier ; 3 : appui exact ; 4 : empreinte UV et concordance visuelle renforcee ; 5 : roles de trottoir symetriques, UV hors empreinte physique
        public const float ReliefSupportToleranceMeters = 0.001f;
        public const float ReliefOverlapToleranceMeters = 0.01f;
        public const float DefaultStepMeters = 0.05f;
        private const float GeometryTolerance = 0.01f;
        private const float UnitTolerance = 0.001f; // meme tolerance que RoadGeometryValidator

        // Concordance declaration / visible : pas de grille ; deux cellules adjacentes de discordance echouent.
        public const float VisualStepMeters = 0.05f;
        private const float SidewalkLookShare = 0.75f;

        private sealed class Surface
        {
            public JunctionClearanceSurface Input;
            public string Name { get { return Input.Name; } }
            public List<Collider> Colliders { get { return Input.Colliders; } }
            public readonly List<Vector2[]> Polygons = new List<Vector2[]>();
        }

        /// <summary>Le profil versionne fournit le gabarit et la marge ; la porte de courbe compilee vaut 0,05 m.</summary>
        public static float HalfLength(RoadModelValidationProfile profile)
        {
            return profile.MaxVehicleLengthMeters * 0.5f + profile.LateralClearanceMarginMeters + V1RoadModelImporter.ChordToleranceMeters;
        }

        public static float HalfWidth(RoadModelValidationProfile profile)
        {
            return profile.MaxVehicleHalfWidthMeters + profile.LateralClearanceMarginMeters + V1RoadModelImporter.ChordToleranceMeters;
        }

        /// <summary>Genere toutes les poses, y compris les echantillons compiles et les deux cotes de chaque couture.</summary>
        public static List<SweepPose> Subdivide(IReadOnlyList<SweepPose> coarse, SweepGraph graph, float h)
        {
            if (coarse == null || coarse.Count == 0 || !(h > 0f))
            {
                throw new ArgumentException("Trajectoire vide ou pas h non positif.");
            }

            var dense = new List<SweepPose> { coarse[0] };
            if (coarse[0].Degenerate)
            {
                throw new ArgumentException("Tangente horizontale nulle a la premiere pose.");
            }

            for (int i = 1; i < coarse.Count; i++)
            {
                SweepPose a = coarse[i - 1];
                SweepPose b = coarse[i];
                if (a.Degenerate || b.Degenerate)
                {
                    throw new ArgumentException("Tangente horizontale nulle a la pose " + i + ".");
                }

                if (a.ElementId != b.ElementId)
                {
                    dense.Add(b); // couture explicite, jamais interpolee comme un segment compile
                    continue;
                }

                if ((a.Heading + b.Heading).magnitude <= UnitTolerance)
                {
                    throw new ArgumentException("Tangentes horizontales antiparalleles dans l'element " + a.ElementId + ".");
                }

                if (!graph.Elements.TryGetValue(a.ElementId, out SweepElement element) || !(b.SMeters > a.SMeters))
                {
                    throw new ArgumentException("Segment compile ou abscisse invalide dans l'element " + a.ElementId + ".");
                }

                int steps = Mathf.Max(1, Mathf.CeilToInt((b.SMeters - a.SMeters) / h));
                for (int step = 1; step <= steps; step++)
                {
                    if (step == steps)
                    {
                        dense.Add(b); // pose compilee exacte, jamais re-echantillonnee
                        break;
                    }

                    float s = Mathf.Lerp(a.SMeters, b.SMeters, (float)step / steps);
                    RoadCurvePoint point = element.Curve.Sample(s);
                    SweepPose pose = SweepPose.From(point.Position, point.Tangent);
                    if (pose.Degenerate)
                    {
                        throw new ArgumentException("Tangente horizontale nulle dans l'element " + a.ElementId + ".");
                    }

                    pose.ElementId = a.ElementId;
                    pose.SMeters = s;
                    dense.Add(pose);
                }
            }

            return dense;
        }

        /// <summary>Distance exacte de deux polygones convexes du plan, zero en contact.</summary>
        public static float Distance(IReadOnlyList<Vector2> a, IReadOnlyList<Vector2> b)
        {
            if (a == null || b == null || a.Count < 3 || b.Count < 3)
            {
                throw new ArgumentException("Deux polygones convexes non degeneres sont requis.");
            }

            if (!Separated(a, b) && !Separated(b, a))
            {
                return 0f;
            }

            float best = float.PositiveInfinity;
            for (int i = 0; i < a.Count; i++)
            {
                for (int j = 0; j < b.Count; j++)
                {
                    best = Mathf.Min(best, PointEdge(a[i], b[j], b[(j + 1) % b.Count]));
                    best = Mathf.Min(best, PointEdge(b[j], a[i], a[(i + 1) % a.Count]));
                }
            }

            return best;
        }

        /// <summary>Residu continu min(d_a,d_b) - (|Delta p| + rho Delta theta)/2.</summary>
        public static JunctionClearanceWitness MeasurePath(IReadOnlyList<SweepPose> poses, IReadOnlyList<Vector2[]> obstacles,
            RoadModelValidationProfile profile, string obstacleName = null)
        {
            var witness = new JunctionClearanceWitness { Obstacle = obstacleName };
            if (poses == null || poses.Count == 0 || obstacles == null || obstacles.Count == 0)
            {
                return witness;
            }

            float halfLength = HalfLength(profile);
            float halfWidth = HalfWidth(profile);
            float rho = Mathf.Sqrt(halfLength * halfLength + halfWidth * halfWidth);
            foreach (Vector2[] obstacle in obstacles)
            {
                var previous = ConflictSweep.Corners(poses[0], halfLength, halfWidth);
                float d0 = Distance(previous, obstacle);
                if (poses.Count == 1 && d0 < witness.Residual)
                {
                    witness.Residual = d0;
                    witness.Position = poses[0].Position;
                }

                for (int i = 1; i < poses.Count; i++)
                {
                    var current = ConflictSweep.Corners(poses[i], halfLength, halfWidth);
                    float d1 = Distance(current, obstacle);
                    float delta = ConflictSweep.Delta(poses[i - 1], poses[i], rho);
                    witness.LargestDelta = Mathf.Max(witness.LargestDelta, delta);
                    float residual = Mathf.Min(d0, d1) - 0.5f * delta;
                    if (residual < witness.Residual)
                    {
                        witness.Residual = residual;
                        witness.Position = d0 <= d1 ? poses[i - 1].Position : poses[i].Position;
                        witness.Seam = poses[i - 1].ElementId != poses[i].ElementId;
                        witness.IntervalDelta = delta;
                    }

                    previous = current;
                    d0 = d1;
                }
            }

            return witness;
        }

        /// <param name="sidewalks">Surfaces Sidewalk de toute la scene, lues par l'adaptateur Editor de la classification V1.</param>
        public static JunctionClearanceResult Measure(Scene scene, V1ImportResult import, CompiledRoadModel model,
            IReadOnlyList<JunctionClearanceSurface> sidewalks, float h = DefaultStepMeters)
        {
            var result = new JunctionClearanceResult { StepMeters = h };
            if (!scene.IsValid() || !scene.isLoaded || import == null || model == null || !(h > 0f))
            {
                result.Failures.Add("Scene, import, modele ou pas h invalide.");
                return result;
            }

            if (sidewalks == null || sidewalks.Count == 0)
            {
                result.Failures.Add("Aucune surface Sidewalk fournie : preuve semantique impossible.");
                return result;
            }

            GameObject ai = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            BoxCollider aiBox = ai == null ? null : ai.GetComponent<BoxCollider>();
            if (aiBox == null)
            {
                result.Failures.Add("Collider du vehicule IA absent.");
                return result;
            }

            result.VehicleTopMeters = aiBox.center.y + aiBox.size.y * 0.5f;
            var physicsBody = ai.GetComponent<VehiclePhysicsBody>();
            var profileDef = physicsBody == null ? null : new SerializedObject(physicsBody).FindProperty("vehicleProfile").objectReferenceValue as VehicleProfileDef;
            string reliefInputs;
            if (profileDef == null || !profileDef.TryValidate(out string profileError))
            {
                // Sans profil, aucun relief n'est juge roulable : tout volume reste obstacle.
                result.Failures.Add("Profil vehicule IA absent ou invalide : regle du relief routier inapplicable.");
                reliefInputs = "relief:none";
            }
            else
            {
                result.ReliefClearanceMeters = StaticBodyClearance(profileDef.Profile, aiBox);
                reliefInputs = "relief:" + result.ReliefClearanceMeters.ToString("R", CultureInfo.InvariantCulture) + "|"
                    + GlobalObjectId.GetGlobalObjectIdSlow(profileDef) + "|" + JsonUtility.ToJson(profileDef);
            }

            Physics.SyncTransforms();
            var graph = SweepGraph.FromModel(model);
            var allColliders = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Collider>(true)).ToArray();
            var physicalInputs = new HashSet<Collider>();
            var semanticInputs = new HashSet<Collider>();
            var semanticVisuals = new HashSet<Renderer>();

            // Les trajectoires prolongees longent aussi les trottoirs des raccords : toute surface
            // Sidewalk de la scene est candidate, quel que soit son module.
            var surfaces = Surfaces(sidewalks, result.Failures);
            var inProof = new HashSet<Surface>();
            var sidewalkPolygons = surfaces.SelectMany(surface => surface.Polygons).ToList();
            var sidewalkColliders = new HashSet<Collider>(surfaces.SelectMany(surface => surface.Colliders));
            var reliefs = new HashSet<Collider>();
            var supportCache = new Dictionary<(Collider, float, float), bool>();

            foreach (V1Module module in import.SourceSet.Modules)
            {
                if (module.Kind != V1ModuleKind.Crossroads && module.Kind != V1ModuleKind.TJunction)
                {
                    continue;
                }

                var movements = import.Movements.Where(movement => movement.Module == module).ToArray();
                foreach (ImportedCurve movement in movements)
                {
                    string pathFailure;
                    var paths = ConflictSweep.Paths(graph, import.IdOf(movement.Key), HalfLength(model.ValidationProfile), out pathFailure);
                    if (pathFailure != null)
                    {
                        result.Failures.Add(module.Label + "/" + movement.Label + " : " + pathFailure);
                        continue;
                    }

                    var physical = new JunctionClearanceWitness();
                    var semantic = new Dictionary<Surface, JunctionClearanceWitness>();
                    foreach (var coarse in paths)
                    {
                        List<SweepPose> poses;
                        try { poses = Subdivide(coarse, graph, h); }
                        catch (ArgumentException error) { result.Failures.Add(module.Label + "/" + movement.Label + " : " + error.Message); continue; }

                        var nearby = Nearby(allColliders, poses, model.ValidationProfile);
                        foreach (Collider collider in nearby)
                        {
                            physicalInputs.Add(collider); // participation et geometrie, y compris inactif
                            if (!Participates(collider, aiBox)) continue;
                            if (!VerticalOverlap(collider, poses, result.VehicleTopMeters, result.Failures, out float minRoad, out float maxRoad)) continue;
                            Vector2[] polygon = Footprint(collider);
                            if (polygon == null)
                            {
                                result.Failures.Add("Obstacle proche non supporte : " + Describe(collider));
                                continue;
                            }

                            float height = collider.bounds.max.y - minRoad;
                            var supportKey = (collider, minRoad, maxRoad);
                            if (!supportCache.TryGetValue(supportKey, out bool supported))
                                supportCache.Add(supportKey, supported = height <= result.ReliefClearanceMeters && RoadSupported(collider, polygon, minRoad, maxRoad, sidewalkColliders));
                            if (IsDrivableRelief(polygon, supported, height, sidewalkPolygons, result.ReliefClearanceMeters))
                            {
                                if (reliefs.Add(collider))
                                    result.DrivableReliefs.Add(new JunctionClearanceRelief { Collider = Describe(collider), HeightMeters = height, ClearanceMeters = result.ReliefClearanceMeters });
                                continue;
                            }

                            var measured = MeasurePath(poses, new[] { polygon }, model.ValidationProfile, Describe(collider));
                            Merge(physical, measured);
                        }

                        float reach = Mathf.Sqrt(Mathf.Pow(HalfLength(model.ValidationProfile), 2f) + Mathf.Pow(HalfWidth(model.ValidationProfile), 2f));
                        foreach (Surface surface in surfaces)
                        {
                            if (!surface.Polygons.Any(polygon => Near(polygon, poses, reach))) continue;
                            if (!semantic.TryGetValue(surface, out JunctionClearanceWitness witness))
                            {
                                semantic.Add(surface, witness = new JunctionClearanceWitness());
                                inProof.Add(surface);
                                foreach (Collider collider in surface.Colliders) semanticInputs.Add(collider);
                            }

                            Merge(witness, MeasurePath(poses, surface.Polygons, model.ValidationProfile, surface.Name));
                        }
                    }

                    if (semantic.Count == 0)
                    {
                        result.Failures.Add(module.Label + "/" + movement.Label + " : aucune surface Sidewalk a portee, preuve semantique vide.");
                    }

                    foreach (Surface surface in semantic.Keys)
                    {
                        var row = new JunctionClearanceRow
                        {
                            Junction = module.Label,
                            Movement = movement.Label,
                            Surface = surface.Name,
                            Physical = physical,
                            Semantic = semantic[surface]
                        };
                        result.Rows.Add(row);
                        if (!(row.Physical.Residual > 0f) || !(row.Semantic.Residual > 0f))
                        {
                            result.Failures.Add(module.Label + "/" + movement.Label + "/" + surface.Name + " : residu non positif (physique "
                                + row.Physical.Residual.ToString("R", CultureInfo.InvariantCulture) + ", Sidewalk "
                                + row.Semantic.Residual.ToString("R", CultureInfo.InvariantCulture) + ").");
                        }
                    }
                }
            }

            // Concordance visuelle sur les modules dont une surface entre dans la preuve (jonctions et
            // raccords a portee) ; giratoires et portails hors portee ne sont pas juges ici.
            CheckVisuals(import, surfaces.Where(surface => inProof.Contains(surface) || inProof.Any(m => SameModule(import, m, surface))).ToList(), result.Failures, semanticVisuals);

            // Le gabarit IA (couche, include/exclude, boite) fait partie des entrees physiques.
            physicalInputs.Add(aiBox);
            result.PhysicalFingerprint = Fingerprint(physicalInputs.Cast<Component>(), reliefInputs, ai.layer);

            // Toutes les declarations entrent dans l'empreinte semantique, meme hors portee : en ajouter
            // ou en retirer une change la couverture visuelle verifiee.
            var declarations = new StringBuilder();
            foreach (Surface surface in surfaces.OrderBy(s => GlobalObjectId.GetGlobalObjectIdSlow(s.Input.Declaration).ToString(), StringComparer.Ordinal))
            {
                declarations.Append(GlobalObjectId.GetGlobalObjectIdSlow(surface.Input.Declaration)).Append('|')
                    .Append(surface.Input.DeclarationSignature).Append('|').Append(surface.Colliders.Count).Append('\n');
            }

            result.SemanticFingerprint = Fingerprint(semanticInputs.Cast<Component>().Concat(semanticVisuals.Cast<Component>()), declarations.ToString(), ai.layer);
            return result;
        }

        private static List<Surface> Surfaces(IReadOnlyList<JunctionClearanceSurface> inputs, List<string> failures)
        {
            var surfaces = new List<Surface>();
            foreach (JunctionClearanceSurface input in inputs)
            {
                if (input == null || input.Declaration == null)
                {
                    failures.Add("Declaration Sidewalk nulle.");
                    continue;
                }

                var surface = new Surface { Input = input };
                if (surface.Colliders.Count == 0) failures.Add("Sidewalk sans collider : " + surface.Name);
                foreach (Collider collider in surface.Colliders)
                {
                    Vector2[] polygon = Footprint(collider);
                    if (polygon == null) failures.Add("Sidewalk sans forme supportee : " + Describe(collider));
                    else surface.Polygons.Add(polygon);
                }

                surfaces.Add(surface);
            }

            return surfaces;
        }

        /// <summary>
        /// Concordance declaration / trottoir visible, module par module. Grille de pas
        /// <see cref="VisualStepMeters"/> sur les polygones declares du module elargis de 1 m ; chaque
        /// cellule prend la surface visible la plus haute parmi les faces non verticales des renderers
        /// actifs du module. Un aspect (mesh + materiaux) est « trottoir » si, n'importe ou dans la scene,
        /// un renderer qui le porte a au moins 3/4 de son empreinte dans une region declaree, ou si son
        /// nom authoring indique un trottoir (y compris un nouveau couple mesh/materiau). Echec :
        /// cellule declaree sans trottoir visible au sommet, trottoir visible non declare, ou egalite de
        /// hauteur entre un trottoir et autre chose. Deux cellules adjacentes de 5 cm suffisent ; une
        /// cellule isolee reste sous la precision de la grille.
        /// ponytail: seuls les renderers du module sont vus ; un visuel de trottoir pose hors de la
        /// hierarchie du module echappe au controle -- etendre a la scene (hors props) si ce cas apparait.
        /// </summary>
        private static void CheckVisuals(V1ImportResult import, List<Surface> surfaces, List<string> failures, HashSet<Renderer> visuals)
        {
            var groups = new Dictionary<V1Module, List<Vector2[]>>();
            foreach (Surface surface in surfaces)
            {
                if (surface.Polygons.Count == 0) continue;
                V1Module owner = ModuleOf(import, surface);
                if (owner == null)
                {
                    failures.Add("Sidewalk hors module V1, visuel non verifiable : " + surface.Name);
                    continue;
                }

                if (!groups.TryGetValue(owner, out List<Vector2[]> declared)) groups.Add(owner, declared = new List<Vector2[]>());
                declared.AddRange(surface.Polygons);
            }

            var faces = new Dictionary<V1Module, List<VisibleFaces>>();
            var sidewalkLook = new HashSet<string>();
            foreach (var group in groups)
            {
                var list = new List<VisibleFaces>();
                foreach (MeshRenderer renderer in group.Key.Root.GetComponentsInChildren<MeshRenderer>(false))
                {
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (!renderer.enabled || filter == null || filter.sharedMesh == null) continue;
                    var entry = new VisibleFaces { Renderer = renderer, Look = Look(renderer, filter.sharedMesh) };
                    Vector3[] vertices = filter.sharedMesh.vertices;
                    int[] triangles = filter.sharedMesh.triangles;
                    float inside = 0f, all = 0f;
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        Vector3 a = renderer.transform.TransformPoint(vertices[triangles[i]]);
                        Vector3 b = renderer.transform.TransformPoint(vertices[triangles[i + 1]]);
                        Vector3 c = renderer.transform.TransformPoint(vertices[triangles[i + 2]]);
                        Vector3 normal = Vector3.Cross(b - a, c - a);
                        if (normal.sqrMagnitude < 1e-12f || Mathf.Abs(normal.normalized.y) < 0.05f) continue; // faces verticales
                        entry.Triangles.Add(a);
                        entry.Triangles.Add(b);
                        entry.Triangles.Add(c);
                        float plan = 0.5f * Mathf.Abs(normal.y);
                        all += plan;
                        var centroid = new Vector2(a.x + b.x + c.x, a.z + b.z + c.z) / 3f;
                        if (group.Value.Any(polygon => Inside(polygon, centroid))) inside += plan;
                    }

                    if (all > 0f && inside >= SidewalkLookShare * all) sidewalkLook.Add(entry.Look);
                    list.Add(entry);
                }

                faces.Add(group.Key, list);
            }

            foreach (var group in groups)
            {
                List<Vector2[]> declared = group.Value;
                List<VisibleFaces> list = faces[group.Key];
                float minX = declared.Min(p => p.Min(v => v.x)) - 1f, maxX = declared.Max(p => p.Max(v => v.x)) + 1f;
                float minZ = declared.Min(p => p.Min(v => v.y)) - 1f, maxZ = declared.Max(p => p.Max(v => v.y)) + 1f;
                int nx = Mathf.CeilToInt((maxX - minX) / VisualStepMeters), nz = Mathf.CeilToInt((maxZ - minZ) / VisualStepMeters);
                var top = new float[nx * nz];
                var owner = new int[nx * nz];
                var tie = new bool[nx * nz];
                for (int k = 0; k < top.Length; k++) { top[k] = float.NegativeInfinity; owner[k] = -1; }

                for (int r = 0; r < list.Count; r++)
                {
                    bool look = SidewalkRole(list[r], sidewalkLook);
                    List<Vector3> t = list[r].Triangles;
                    for (int i = 0; i < t.Count; i += 3)
                    {
                        Vector3 a = t[i], b = t[i + 1], c = t[i + 2];
                        float det = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
                        if (Mathf.Abs(det) < 1e-10f) continue;
                        int i0 = Mathf.Max(0, Mathf.FloorToInt((Mathf.Min(a.x, Mathf.Min(b.x, c.x)) - minX) / VisualStepMeters));
                        int i1 = Mathf.Min(nx - 1, Mathf.FloorToInt((Mathf.Max(a.x, Mathf.Max(b.x, c.x)) - minX) / VisualStepMeters));
                        int k0 = Mathf.Max(0, Mathf.FloorToInt((Mathf.Min(a.z, Mathf.Min(b.z, c.z)) - minZ) / VisualStepMeters));
                        int k1 = Mathf.Min(nz - 1, Mathf.FloorToInt((Mathf.Max(a.z, Mathf.Max(b.z, c.z)) - minZ) / VisualStepMeters));
                        for (int ix = i0; ix <= i1; ix++)
                        {
                            float px = minX + (ix + 0.5f) * VisualStepMeters;
                            for (int iz = k0; iz <= k1; iz++)
                            {
                                float pz = minZ + (iz + 0.5f) * VisualStepMeters;
                                float w0 = ((b.z - c.z) * (px - c.x) + (c.x - b.x) * (pz - c.z)) / det;
                                float w1 = ((c.z - a.z) * (px - c.x) + (a.x - c.x) * (pz - c.z)) / det;
                                float w2 = 1f - w0 - w1;
                                if (w0 < -1e-6f || w1 < -1e-6f || w2 < -1e-6f) continue;
                                float y = w0 * a.y + w1 * b.y + w2 * c.y;
                                int cell = ix * nz + iz;
                                if (y > top[cell] + 1e-4f) { top[cell] = y; owner[cell] = r; tie[cell] = false; }
                                else if (y >= top[cell] - 1e-4f && owner[cell] != r && SidewalkRole(list[owner[cell]], sidewalkLook) != look) tie[cell] = true;
                            }
                        }
                    }
                }

                // 0 concordant, 1 declare sans trottoir visible, 2 trottoir visible non declare, 3 egalite ambigue
                var state = new byte[nx * nz];
                for (int ix = 0; ix < nx; ix++)
                {
                    for (int iz = 0; iz < nz; iz++)
                    {
                        int cell = ix * nz + iz;
                        var p = new Vector2(minX + (ix + 0.5f) * VisualStepMeters, minZ + (iz + 0.5f) * VisualStepMeters);
                        bool isDeclared = declared.Any(polygon => Inside(polygon, p));
                        bool isSidewalk = owner[cell] >= 0 && SidewalkRole(list[owner[cell]], sidewalkLook);
                        if (isDeclared || isSidewalk)
                        {
                            if (owner[cell] >= 0) visuals.Add(list[owner[cell]].Renderer);
                            state[cell] = tie[cell] ? (byte)3 : isDeclared && !isSidewalk ? (byte)1 : !isDeclared && isSidewalk ? (byte)2 : (byte)0;
                        }
                    }
                }

                var counts = new int[4];
                var witnesses = new Vector2[4];
                for (int ix = 0; ix < nx; ix++)
                {
                    for (int iz = 0; iz < nz; iz++)
                    {
                        byte s = state[ix * nz + iz];
                        if (s == 0) continue;
                        if (!HasAdjacentMismatch(state, nx, nz, ix, iz, s)) continue;
                        if (counts[s]++ == 0) witnesses[s] = new Vector2(minX + (ix + 0.5f) * VisualStepMeters, minZ + (iz + 0.5f) * VisualStepMeters);
                    }
                }

                string[] labels = { null, "declare sans trottoir visible", "trottoir visible non declare", "egalite de hauteur trottoir/autre" };
                for (int s = 1; s <= 3; s++)
                {
                    if (counts[s] > 0)
                        failures.Add("Visuel Sidewalk discordant " + group.Key.Label + " : " + counts[s] + " cellules " + labels[s]
                            + ", ex. (" + witnesses[s].x.ToString("F2", CultureInfo.InvariantCulture) + " ; " + witnesses[s].y.ToString("F2", CultureInfo.InvariantCulture) + ").");
                }
            }
        }

        private static V1Module ModuleOf(V1ImportResult import, Surface surface)
        {
            return import.SourceSet.Modules.FirstOrDefault(m => m.Root != null && surface.Input.Declaration.transform.IsChildOf(m.Root));
        }

        private static bool SameModule(V1ImportResult import, Surface a, Surface b)
        {
            V1Module module = ModuleOf(import, a);
            return module != null && module == ModuleOf(import, b);
        }

        private sealed class VisibleFaces
        {
            public MeshRenderer Renderer;
            public string Look;
            public readonly List<Vector3> Triangles = new List<Vector3>();
        }

        /// <summary>Role trottoir d'une face : nom authoring ou aspect classe ; la meme regle sert au candidat, au titulaire en sommet et a l'egalite de hauteur.</summary>
        private static bool SidewalkRole(VisibleFaces face, HashSet<string> sidewalkLook)
        {
            return IsAuthoredSidewalkVisual(face.Renderer) || sidewalkLook.Contains(face.Look);
        }

        private static bool IsAuthoredSidewalkVisual(Renderer renderer)
        {
            string name = renderer.name;
            return name.StartsWith("Sidewalk_", StringComparison.Ordinal) || name.StartsWith("Chamfer_Visual_", StringComparison.Ordinal);
        }

        private static bool HasAdjacentMismatch(byte[] state, int nx, int nz, int ix, int iz, byte mismatch)
        {
            return (ix > 0 && state[(ix - 1) * nz + iz] == mismatch)
                || (ix + 1 < nx && state[(ix + 1) * nz + iz] == mismatch)
                || (iz > 0 && state[ix * nz + iz - 1] == mismatch)
                || (iz + 1 < nz && state[ix * nz + iz + 1] == mismatch);
        }

        private static string Look(Renderer renderer, Mesh mesh)
        {
            var look = new StringBuilder(GlobalObjectId.GetGlobalObjectIdSlow(mesh).ToString());
            foreach (Material material in renderer.sharedMaterials)
                look.Append('|').Append(material == null ? "null" : GlobalObjectId.GetGlobalObjectIdSlow(material).ToString());
            return look.ToString();
        }

        private static bool Inside(Vector2[] convex, Vector2 point)
        {
            bool positive = false, negative = false;
            for (int i = 0; i < convex.Length; i++)
            {
                float cross = Cross(convex[i], convex[(i + 1) % convex.Length], point);
                if (cross > 0f) positive = true;
                if (cross < 0f) negative = true;
            }

            return !(positive && negative);
        }

        private static List<Collider> Nearby(IEnumerable<Collider> colliders, IReadOnlyList<SweepPose> poses, RoadModelValidationProfile profile)
        {
            // + 1 m : un obstacle juste hors portee pourrait encore rendre negatif le residu entre poses.
            float reach = Mathf.Sqrt(Mathf.Pow(HalfLength(profile), 2f) + Mathf.Pow(HalfWidth(profile), 2f)) + 1f;
            float minX = poses.Min(p => p.Position.x) - reach;
            float maxX = poses.Max(p => p.Position.x) + reach;
            float minZ = poses.Min(p => p.Position.z) - reach;
            float maxZ = poses.Max(p => p.Position.z) + reach;
            var nearby = new List<Collider>();
            foreach (Collider collider in colliders)
            {
                Bounds bounds = collider.bounds;
                if (!collider.enabled || !collider.gameObject.activeInHierarchy)
                {
                    Vector2[] polygon = Footprint(collider);
                    if (polygon != null)
                    {
                        if (polygon.Max(p => p.x) < minX || polygon.Min(p => p.x) > maxX
                            || polygon.Max(p => p.y) < minZ || polygon.Min(p => p.y) > maxZ) continue;
                        nearby.Add(collider);
                    }
                    continue;
                }

                if (bounds.max.x >= minX && bounds.min.x <= maxX && bounds.max.z >= minZ && bounds.min.z <= maxZ)
                    nearby.Add(collider);
            }

            return nearby;
        }

        /// <summary>Contact possible avec le vehicule IA ; en cas de doute (include/exclude), l'obstacle participe.</summary>
        /// <summary>Boite du chemin elargie de la portee + 1 m : au-dela, le residu est forcement positif.</summary>
        private static bool Near(Vector2[] polygon, IReadOnlyList<SweepPose> poses, float reach)
        {
            float pad = reach + 1f;
            return polygon.Max(p => p.x) >= poses.Min(p => p.Position.x) - pad && polygon.Min(p => p.x) <= poses.Max(p => p.Position.x) + pad
                && polygon.Max(p => p.y) >= poses.Min(p => p.Position.z) - pad && polygon.Min(p => p.y) <= poses.Max(p => p.Position.z) + pad;
        }

        private static bool Participates(Collider collider, Collider ai)
        {
            Rigidbody body = collider.attachedRigidbody;
            int layer = collider.gameObject.layer;
            int aiLayer = ai.gameObject.layer;
            bool matrix = !Physics.GetIgnoreLayerCollision(aiLayer, layer)
                || (collider.includeLayers.value & (1 << aiLayer)) != 0 || (ai.includeLayers.value & (1 << layer)) != 0;
            bool excluded = (collider.excludeLayers.value & (1 << aiLayer)) != 0 && (ai.excludeLayers.value & (1 << layer)) != 0;
            return collider.enabled && collider.gameObject.activeInHierarchy && !collider.isTrigger
                && (body == null || body.isKinematic) && matrix && !excluded;
        }

        private static bool VerticalOverlap(Collider collider, IReadOnlyList<SweepPose> poses, float vehicleTop, List<string> failures,
            out float minRoad, out float maxRoad)
        {
            minRoad = float.PositiveInfinity;
            maxRoad = float.NegativeInfinity;
            foreach (SweepPose pose in poses)
            {
                float road = RoadTop(pose.Position, collider.gameObject.scene);
                if (float.IsNegativeInfinity(road))
                {
                    failures.Add("Surface roulable introuvable sous " + pose.Position + ".");
                    return false;
                }

                minRoad = Mathf.Min(minRoad, road);
                maxRoad = Mathf.Max(maxRoad, road);
            }

            return collider.bounds.max.y > minRoad + 0.0001f && collider.bounds.min.y < maxRoad + vehicleTop;
        }

        /// <summary>Garde au sol statique : hauteur de caisse au repos du profil plus dessous du collider de caisse (porte par la racine).</summary>
        public static float StaticBodyClearance(VehicleProfile profile, BoxCollider body)
        {
            return profile.ResolveStaticRideHeight(Mathf.Abs(Physics.gravity.y)) + body.center.y - body.size.y * 0.5f;
        }

        /// <summary>
        /// Regle du relief routier franchissable (decision proprietaire du 2026-09-28) : un volume dans la
        /// tranche du vehicule est une surface roulable, et non un obstacle, si et seulement si
        /// (a) son empreinte ne chevauche aucune surface Sidewalk declaree -- le simple contact a moins
        /// de <see cref="ReliefOverlapToleranceMeters"/> est admis ; (b) il repose sur la chaussee
        /// (<paramref name="supportedByRoad"/>, voir <see cref="RoadSupported"/>) ; (c) il depasse la route
        /// d'au plus <paramref name="clearance"/>, la garde au sol statique du vehicule IA, que les bancs
        /// 5.11/5.13 eprouvent jusqu'a la hauteur de bordure authoree. Trottoirs, bordures posees sur un
        /// trottoir, murs, batiments et obstacles plus hauts restent des obstacles.
        /// </summary>
        public static bool IsDrivableRelief(Vector2[] footprint, bool supportedByRoad, float heightAboveRoad,
            IReadOnlyList<Vector2[]> sidewalks, float clearance)
        {
            if (footprint == null || footprint.Length < 3 || !supportedByRoad || !(heightAboveRoad <= clearance)) return false;
            foreach (Vector2[] sidewalk in sidewalks)
            {
                if (Overlaps(footprint, sidewalk, ReliefOverlapToleranceMeters)) return false;
            }

            return true;
        }

        /// <summary>Chevauchement d'interieurs de deux polygones convexes ; un contact a moins de <paramref name="tolerance"/> n'en est pas un.</summary>
        public static bool Overlaps(IReadOnlyList<Vector2> a, IReadOnlyList<Vector2> b, float tolerance)
        {
            return !SeparatedBy(a, b, tolerance) && !SeparatedBy(b, a, tolerance);
        }

        private static bool SeparatedBy(IReadOnlyList<Vector2> a, IReadOnlyList<Vector2> b, float tolerance)
        {
            for (int i = 0; i < a.Count; i++)
            {
                Vector2 edge = a[(i + 1) % a.Count] - a[i];
                if (edge.sqrMagnitude < 1e-12f) continue;
                Vector2 axis = new Vector2(-edge.y, edge.x).normalized;
                float aMin = float.PositiveInfinity, aMax = float.NegativeInfinity, bMin = float.PositiveInfinity, bMax = float.NegativeInfinity;
                foreach (Vector2 point in a) { float d = Vector2.Dot(point, axis); aMin = Mathf.Min(aMin, d); aMax = Mathf.Max(aMax, d); }
                foreach (Vector2 point in b) { float d = Vector2.Dot(point, axis); bMin = Mathf.Min(bMin, d); bMax = Mathf.Max(bMax, d); }
                if (aMax <= bMin + tolerance || bMax <= aMin + tolerance) return true;
            }

            return false;
        }

        /// <summary>
        /// (b) de la regle du relief : toute l'empreinte repose sur la chaussee, par inclusion geometrique
        /// complete et non par echantillonnage. Supports : colliders actifs, non-trigger, sans Rigidbody
        /// dynamique, BoxCollider a face superieure horizontale dont le dessus est a hauteur de route
        /// (entre <paramref name="minRoad"/> et <paramref name="maxRoad"/>, a 1 cm pres) et qui ne sont pas
        /// Sidewalk. L'empreinte privee de l'union des supports doit etre vide ; les supports sont dilates
        /// de <see cref="ReliefSupportToleranceMeters"/> (joint de dalles jointives, dilatation en onglet : au plus
        /// 1,5 mm aux angles droits), donc toute fente de plus de 3 mm, ou qu'elle soit, laisse un reste et refuse le relief.
        /// Tout chevauchement d'un collider Sidewalk (actif ou non, a toute hauteur) refuse aussi : le
        /// trottoir l'emporte. Plan de sol plus bas que la route, rampe inclinee ou mesh ne portent pas.
        /// </summary>
        public static bool RoadSupported(Collider relief, Vector2[] footprint, float minRoad, float maxRoad, HashSet<Collider> sidewalkColliders)
        {
            if (footprint == null || footprint.Length < 3) return false;
            foreach (Collider sidewalk in sidewalkColliders)
            {
                Vector2[] polygon = Footprint(sidewalk);
                if (polygon == null || Overlaps(footprint, polygon, ReliefOverlapToleranceMeters)) return false;
            }

            float xMin = footprint.Min(p => p.x), xMax = footprint.Max(p => p.x), zMin = footprint.Min(p => p.y), zMax = footprint.Max(p => p.y);
            var center = new Vector3((xMin + xMax) * 0.5f, (minRoad + maxRoad) * 0.5f, (zMin + zMax) * 0.5f);
            var extents = new Vector3((xMax - xMin) * 0.5f + GeometryTolerance, (maxRoad - minRoad) * 0.5f + GeometryTolerance, (zMax - zMin) * 0.5f + GeometryTolerance);
            var supports = new List<Vector2[]>();
            foreach (Collider candidate in Physics.OverlapBox(center, extents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
            {
                if (candidate.gameObject.scene != relief.gameObject.scene || candidate == relief || sidewalkColliders.Contains(candidate) || !(candidate is BoxCollider)) continue;
                if (candidate.attachedRigidbody != null && !candidate.attachedRigidbody.isKinematic) continue;
                if (Vector3.Dot(candidate.transform.up, Vector3.up) < 1f - 1e-5f) continue; // dessus incline : pas un support prouve
                float top = candidate.bounds.max.y;
                if (top < minRoad - GeometryTolerance || top > maxRoad + GeometryTolerance) continue;
                supports.Add(Footprint(candidate));
            }

            return Covered(footprint, supports, ReliefSupportToleranceMeters);
        }

        /// <summary>
        /// Couverture exacte d'un polygone convexe par une union de polygones convexes (sens trigonometrique),
        /// chacun dilate de <paramref name="tolerance"/> : soustraction successive par demi-plans ; chaque
        /// morceau restant est convexe. Un reste d'aire superieure a 1e-8 m2 est un trou.
        /// </summary>
        public static bool Covered(Vector2[] target, IReadOnlyList<Vector2[]> covers, float tolerance)
        {
            var pieces = new List<List<Vector2>> { new List<Vector2>(target) };
            foreach (Vector2[] cover in covers)
            {
                var next = new List<List<Vector2>>();
                foreach (List<Vector2> piece in pieces)
                {
                    List<Vector2> inside = piece;
                    for (int i = 0; i < cover.Length && inside.Count >= 3; i++)
                    {
                        Vector2 a = cover[i], b = cover[(i + 1) % cover.Length];
                        if ((b - a).sqrMagnitude < 1e-12f) continue;
                        List<Vector2> outside = Clip(inside, a, b, tolerance, -1f);
                        if (Area(outside) > 1e-8f) next.Add(outside);
                        inside = Clip(inside, a, b, tolerance, 1f);
                    }
                }

                pieces = next;
                if (pieces.Count == 0) return true;
            }

            return pieces.Count == 0;
        }

        /// <summary>Sutherland-Hodgman sur le demi-plan a gauche de ab recule de <paramref name="offset"/> (<paramref name="side"/> = 1) ou son complement (-1).</summary>
        private static List<Vector2> Clip(List<Vector2> polygon, Vector2 a, Vector2 b, float offset, float side)
        {
            var result = new List<Vector2>();
            float length = (b - a).magnitude;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p = polygon[i], q = polygon[(i + 1) % polygon.Count];
                float fp = side * (Cross(a, b, p) / length + offset), fq = side * (Cross(a, b, q) / length + offset);
                if (fp >= 0f) result.Add(p);
                if ((fp < 0f) != (fq < 0f)) result.Add(p + (q - p) * (fp / (fp - fq)));
            }

            return result;
        }

        private static float Area(List<Vector2> polygon)
        {
            float twice = 0f;
            for (int i = 0; i < polygon.Count; i++) twice += polygon[i].x * polygon[(i + 1) % polygon.Count].y - polygon[(i + 1) % polygon.Count].x * polygon[i].y;
            return polygon.Count < 3 ? 0f : Mathf.Abs(twice) * 0.5f;
        }

        /// <summary>Surface roulable sous une pose : seuls les colliders de <paramref name="scene"/> comptent, une scene chargee en parallele ne fournit jamais la route mesuree.</summary>
        private static float RoadTop(Vector3 position, Scene scene)
        {
            float top = float.NegativeInfinity;
            foreach (RaycastHit hit in Physics.RaycastAll(position + Vector3.up * 10f, Vector3.down, 20f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject.scene != scene) continue;
                if (hit.point.y <= position.y + GeometryTolerance) top = Mathf.Max(top, hit.point.y);
            }

            return top;
        }

        private static void Merge(JunctionClearanceWitness into, JunctionClearanceWitness candidate)
        {
            into.LargestDelta = Mathf.Max(into.LargestDelta, candidate.LargestDelta);
            if (candidate.Residual >= into.Residual) return;
            into.Residual = candidate.Residual;
            into.Position = candidate.Position;
            into.Obstacle = candidate.Obstacle;
            into.Seam = candidate.Seam;
            into.IntervalDelta = candidate.IntervalDelta;
        }

        /// <summary>
        /// Projection plane exacte d'un volume convexe : enveloppe des 8 sommets d'une boite, quelle que
        /// soit son orientation (rampe inclinee comprise), ou des sommets d'un MeshCollider convexe.
        /// L'enveloppe couvre le volume entier a toute hauteur : une partie haute en porte-a-faux d'une
        /// boite inclinee y figure, et la tranche verticale est jugee sur la boite englobante monde, donc
        /// aucune interaction en hauteur n'echappe a la preuve. Toute autre forme est refusee (null).
        /// </summary>
        public static Vector2[] Footprint(Collider collider)
        {
            var points = new List<Vector2>();
            if (collider is BoxCollider box)
            {
                for (int i = 0; i < 8; i++)
                {
                    var local = box.center + Vector3.Scale(box.size * 0.5f,
                        new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                    Vector3 world = box.transform.TransformPoint(local);
                    points.Add(new Vector2(world.x, world.z));
                }
            }
            else if (collider is MeshCollider mesh && mesh.convex && mesh.sharedMesh != null)
            {
                foreach (Vector3 vertex in mesh.sharedMesh.vertices)
                {
                    Vector3 world = mesh.transform.TransformPoint(vertex);
                    points.Add(new Vector2(world.x, world.z));
                }
            }
            else return null;

            return Hull(points);
        }

        private static Vector2[] Hull(List<Vector2> points)
        {
            points.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            var hull = new Vector2[2 * points.Count];
            int count = 0;
            for (int i = 0; i < points.Count; i++)
            {
                while (count >= 2 && Cross(hull[count - 2], hull[count - 1], points[i]) <= 0f) count--;
                hull[count++] = points[i];
            }

            for (int i = points.Count - 2, lower = count + 1; i >= 0; i--)
            {
                while (count >= lower && Cross(hull[count - 2], hull[count - 1], points[i]) <= 0f) count--;
                hull[count++] = points[i];
            }

            Array.Resize(ref hull, Mathf.Max(0, count - 1));
            return hull.Length >= 3 ? hull : null;
        }

        private static float Cross(Vector2 a, Vector2 b, Vector2 c) { return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x); }

        private static bool Separated(IReadOnlyList<Vector2> a, IReadOnlyList<Vector2> b)
        {
            for (int i = 0; i < a.Count; i++)
            {
                Vector2 edge = a[(i + 1) % a.Count] - a[i];
                Vector2 axis = new Vector2(-edge.y, edge.x);
                float aMin = float.PositiveInfinity, aMax = float.NegativeInfinity, bMin = float.PositiveInfinity, bMax = float.NegativeInfinity;
                foreach (Vector2 point in a) { float d = Vector2.Dot(point, axis); aMin = Mathf.Min(aMin, d); aMax = Mathf.Max(aMax, d); }
                foreach (Vector2 point in b) { float d = Vector2.Dot(point, axis); bMin = Mathf.Min(bMin, d); bMax = Mathf.Max(bMax, d); }
                if (aMax < bMin || bMax < aMin) return true;
            }

            return false;
        }

        private static float PointEdge(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 edge = b - a;
            float t = edge.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(point - a, edge) / edge.sqrMagnitude) : 0f;
            return (point - a - t * edge).magnitude;
        }

        private static string Describe(Collider collider)
        {
            string path = collider.name;
            for (Transform t = collider.transform.parent; t != null; t = t.parent) path = t.name + "/" + path;
            return collider.gameObject.scene.name + "/" + path;
        }

        private static string Fingerprint(IEnumerable<Component> components, string declarations, int aiLayer)
        {
            var text = new StringBuilder("junction-clearance-v").Append(AlgorithmVersion).Append('|').Append(declarations).Append('|');
            foreach (Component component in components.OrderBy(c => GlobalObjectId.GetGlobalObjectIdSlow(c).ToString(), StringComparer.Ordinal))
            {
                text.Append(GlobalObjectId.GetGlobalObjectIdSlow(component)).Append('|').Append(component.GetType().FullName).Append('|');
                for (Transform t = component.transform; t != null; t = t.parent)
                    text.Append(GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject)).Append(JsonUtility.ToJson(t.localPosition))
                        .Append(JsonUtility.ToJson(t.localRotation)).Append(JsonUtility.ToJson(t.localScale));
                text.Append(component.gameObject.activeInHierarchy).Append('|').Append(component.gameObject.layer).Append('|');
                if (component is Collider collider)
                {
                    text.Append(collider.enabled).Append('|').Append(collider.isTrigger).Append('|')
                        .Append(collider.includeLayers.value).Append('|').Append(collider.excludeLayers.value).Append('|')
                        .Append(Physics.GetIgnoreLayerCollision(aiLayer, collider.gameObject.layer)).Append('|')
                        .Append(collider.attachedRigidbody == null ? "none" : collider.attachedRigidbody.isKinematic ? "kinematic" : "dynamic");
                    if (collider is BoxCollider box) text.Append(JsonUtility.ToJson(box.center)).Append(JsonUtility.ToJson(box.size));
                    if (collider is MeshCollider mesh) { text.Append(mesh.cookingOptions).Append(mesh.convex); AppendMesh(text, mesh.sharedMesh, false); }
                }
                else if (component is Renderer renderer)
                {
                    text.Append(renderer.enabled).Append('|');
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    AppendMesh(text, filter == null ? null : filter.sharedMesh, true);
                    foreach (Material material in renderer.sharedMaterials)
                        text.Append('|').Append(material == null ? "null" : GlobalObjectId.GetGlobalObjectIdSlow(material).ToString());
                }

                text.Append('\n');
            }

            return V1SourceSet.Sha256Hex(text.ToString());
        }

        /// <summary>Empreinte d'un mesh : geometrie toujours ; canaux UV seulement pour une surface visuelle (aucun effet sur une collision).</summary>
        private static void AppendMesh(StringBuilder text, Mesh mesh, bool includeUvs)
        {
            if (mesh == null) { text.Append("mesh:null"); return; }
            text.Append(GlobalObjectId.GetGlobalObjectIdSlow(mesh));
            foreach (Vector3 vertex in mesh.vertices) text.Append(JsonUtility.ToJson(vertex));
            foreach (int triangle in mesh.triangles) text.Append(triangle).Append(',');
            if (!includeUvs) return;
            var uvs = new List<Vector4>();
            for (int channel = 0; channel < 8; channel++)
            {
                mesh.GetUVs(channel, uvs);
                text.Append("uv").Append(channel).Append(':').Append(uvs.Count).Append(':');
                foreach (Vector4 uv in uvs) text.Append(JsonUtility.ToJson(uv));
            }
        }
    }
}
#endif
