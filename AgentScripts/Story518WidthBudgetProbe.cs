using System.Collections.Generic;
using System.Text;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Le budget de largeur du district : combien de place une voie offre-t-elle, et combien la
/// trajectoire suivie en consomme-t-elle ?
///
/// La question posee est "faut-il elargir le LaneGraph". Elle se tranche par une soustraction :
/// place_disponible = distance a l'axe oppose - demi-gabarit du vehicule
/// place_consommee  = ecart lateral maximal de la courbe suivie a l'axe de voie
/// Si la place consommee depasse la place disponible alors que la voie est reguliere, c'est la
/// trajectoire qu'il faut corriger, pas le trace.
/// </summary>
public static class Story518WidthBudgetProbe
{
    private const int Samples = 48;

    public static string Run()
    {
        var report = new StringBuilder();
        var scene = EditorSceneManager.OpenScene("Assets/RoadRage/App/Scenes/MVP_Run.unity", OpenSceneMode.Additive);
        try
        {
            LaneGraph graph = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                graph = root.GetComponentInChildren<LaneGraph>(true);
                if (graph != null) break;
            }
            if (graph == null) return "LaneGraph introuvable.";
            graph.Rebuild();

            report.AppendLine("=== 1. GABARIT DU VEHICULE ===");
            var halfWidth = ReportVehicle(report);

            report.AppendLine();
            report.AppendLine("=== 2. PLACE DISPONIBLE : distance entre axes de sens opposes ===");
            var segments = CollectSegments(graph);
            report.AppendLine("segments diriges du graphe : " + segments.Count);

            var separations = new List<float>();
            var worst = float.PositiveInfinity;
            string worstLabel = "(aucun)";
            foreach (var s in segments)
            {
                var best = float.PositiveInfinity;
                foreach (var o in segments)
                {
                    if (Vector3.Dot(s.direction, o.direction) > -0.85f) continue;
                    // Distance laterale entre les deux axes, mesuree au milieu de notre segment et
                    // seulement si l'autre axe est effectivement en vis-a-vis (recouvrement long).
                    var mid = (s.start + s.end) * 0.5f;
                    var closest = TrafficPerception.ClosestPointOnSegment(mid, o.start, o.end);
                    var inside = Vector3.Dot(closest - o.start, o.direction);
                    var span = Vector3.Distance(o.start, o.end);
                    if (inside <= 0.05f || inside >= span - 0.05f) continue; // bout de segment : pas un vis-a-vis
                    var distance = Vector3.ProjectOnPlane(closest - mid, Vector3.up).magnitude;
                    if (distance < best) best = distance;
                }
                if (!float.IsFinite(best)) continue;
                separations.Add(best);
                if (best < worst) { worst = best; worstLabel = s.label; }
            }

            separations.Sort();
            if (separations.Count > 0)
            {
                report.AppendLine("paires en vis-a-vis mesurees : " + separations.Count);
                report.AppendLine("  minimum = " + worst.ToString("F2") + " m   (" + worstLabel + ")");
                report.AppendLine("  median  = " + separations[separations.Count / 2].ToString("F2") + " m");
                report.AppendLine("  maximum = " + separations[separations.Count - 1].ToString("F2") + " m");
                report.AppendLine("  -> demi-chaussee par sens (median/2) = "
                    + (separations[separations.Count / 2] * 0.5f).ToString("F2") + " m");
                report.AppendLine("  -> MARGE avant que la CAISSE morde l'axe oppose = "
                    + (separations[separations.Count / 2] - halfWidth).ToString("F2") + " m");
            }

            report.AppendLine();
            report.AppendLine("=== 3. PLACE CONSOMMEE : ecart de la courbe de virage a l'axe de voie ===");
            ReportTurns(graph, report, halfWidth);

            report.AppendLine();
            report.AppendLine("=== 4. GIRATOIRES : anneau, corde, nombre de voies ===");
            ReportRings(graph, report, halfWidth);

            return report.ToString();
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    private static float ReportVehicle(StringBuilder report)
    {
        var half = 0.9f;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null || prefab.GetComponentInChildren<NetworkedAIVehicleDriverController>(true) == null) continue;

            // bounds d'un collider de PREFAB ne sont pas calcules hors scene : on lit la forme
            // authoree elle-meme, ramenee dans le repere du prefab.
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            var any = false;
            foreach (var box in prefab.GetComponentsInChildren<BoxCollider>(true))
            {
                if (box.isTrigger) continue;
                var scale = box.transform.lossyScale;
                var size = new Vector3(box.size.x * scale.x, box.size.y * scale.y, box.size.z * scale.z);
                var centre = box.transform.localPosition + box.center;
                var local = new Bounds(centre, size);
                if (!any) { bounds = local; any = true; } else bounds.Encapsulate(local);
            }
            foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                var scale = filter.transform.lossyScale;
                var mesh = filter.sharedMesh.bounds;
                var size = new Vector3(mesh.size.x * scale.x, mesh.size.y * scale.y, mesh.size.z * scale.z);
                var local = new Bounds(filter.transform.localPosition + mesh.center, size);
                if (!any) { bounds = local; any = true; } else bounds.Encapsulate(local);
            }
            if (!any) continue;
            half = bounds.extents.x;
            report.AppendLine(path);
            report.AppendLine("  largeur=" + (bounds.size.x).ToString("F2") + " m  longueur=" + bounds.size.z.ToString("F2")
                + " m  -> demi-largeur=" + half.ToString("F2") + " m");
        }
        return half;
    }

    private struct Segment
    {
        public Vector3 start, end, direction;
        public string label;
    }

    private static List<Segment> CollectSegments(LaneGraph graph)
    {
        var list = new List<Segment>();
        for (var i = 0; i < graph.NodeCount; i++)
        {
            foreach (var s in graph.GetSuccessors(i))
            {
                var a = graph.GetNodePosition(i);
                var b = graph.GetNodePosition(s);
                var d = Vector3.ProjectOnPlane(b - a, Vector3.up);
                if (d.sqrMagnitude < 0.01f) continue;
                list.Add(new Segment { start = a, end = b, direction = d.normalized, label = i + "->" + s });
            }
        }
        return list;
    }

    private static void ReportTurns(LaneGraph graph, StringBuilder report, float halfWidth)
    {
        var axes = new List<(Vector3 point, Vector3 forward)>();
        for (var i = 0; i < graph.NodeCount; i++)
        {
            var forward = Vector3.ProjectOnPlane(graph.GetNodeRotation(i) * Vector3.forward, Vector3.up);
            if (forward.sqrMagnitude <= 0.0001f) continue;
            axes.Add((graph.GetNodePosition(i), forward.normalized));
        }

        var count = 0;
        var worstBite = float.NegativeInfinity;
        var worstLabel = "(aucun)";
        foreach (var a in graph.JunctionApproaches)
        {
            var node = graph.GetNodePosition(a.NodeIndex);
            var entry = a.EntryNode == a.NodeIndex ? node : a.EntryPoint;
            var entryForward = a.EntryForward.sqrMagnitude > 0.0001f ? a.EntryForward.normalized : a.Forward.normalized;

            foreach (var s in graph.GetSuccessors(a.NodeIndex))
            {
                var exit = graph.GetNodePosition(s);
                var exitForward = Vector3.ProjectOnPlane(graph.GetNodeRotation(s) * Vector3.forward, Vector3.up).normalized;
                var angle = Vector3.SignedAngle(entryForward, exitForward, Vector3.up);
                if (Mathf.Abs(angle) < 20f) continue;

                var minRadius = float.PositiveInfinity;
                var minOpposite = float.PositiveInfinity;
                for (var i = 0; i <= Samples; i++)
                {
                    var t = i / (float)Samples;
                    var point = LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, t);
                    if (i > 0 && i < Samples)
                    {
                        var before = LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, t - 1f / Samples);
                        var after = LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, t + 1f / Samples);
                        var radius = CircleRadius(before, point, after);
                        if (radius > 0.01f && radius < minRadius) minRadius = radius;
                    }

                    var ahead = LaneGraphRouting.ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, Mathf.Min(1f, t + 0.02f));
                    var tangent = Vector3.ProjectOnPlane(ahead - point, Vector3.up);
                    if (tangent.sqrMagnitude <= 0.0001f) continue;
                    tangent.Normalize();
                    foreach (var axis in axes)
                    {
                        if (Vector3.Dot(axis.forward, tangent) > -0.7f) continue;
                        var distance = DistanceToLine(point, axis.point, axis.forward);
                        if (distance < minOpposite) minOpposite = distance;
                    }
                }

                // Combien la CAISSE deborde-t-elle au-dela de l'axe oppose ? Positif = elle mord.
                var bite = halfWidth - minOpposite;
                if (bite > worstBite) { worstBite = bite; worstLabel = a.NodeIndex + "->" + s; }
                count++;
                report.AppendLine("  mvt " + (a.NodeIndex + "->" + s).PadRight(9)
                    + (angle > 0f ? "gauche " : "droite ") + Mathf.Abs(angle).ToString("F0").PadLeft(3) + " deg"
                    + "  rayon=" + minRadius.ToString("F2").PadLeft(6) + " m"
                    + "  axe_oppose=" + minOpposite.ToString("F2").PadLeft(6) + " m"
                    + "  marge_caisse=" + (minOpposite - halfWidth).ToString("F2").PadLeft(6) + " m");
            }
        }

        report.AppendLine("mouvements tournants mesures : " + count);
        report.AppendLine("  pire empietement de la CAISSE sur l'axe oppose (courbe authoree, suivi PARFAIT) = "
            + worstBite.ToString("F2") + " m  (" + worstLabel + ")");
        report.AppendLine("  (negatif = la courbe authoree tient dans sa moitie de chaussee)");
    }

    private static void ReportRings(LaneGraph graph, StringBuilder report, float halfWidth)
    {
        var seen = new HashSet<int>();
        var rings = 0;
        for (var start = 0; start < graph.NodeCount; start++)
        {
            if (seen.Contains(start)) continue;
            var ring = new List<int>();
            var current = start;
            for (var step = 0; step < 64; step++)
            {
                ring.Add(current);
                var successors = graph.GetSuccessors(current);
                if (successors.Count == 0) break;
                var forward = Vector3.ProjectOnPlane(graph.GetNodeRotation(current) * Vector3.forward, Vector3.up).normalized;
                var next = -1;
                var bestDot = -2f;
                foreach (var s in successors)
                {
                    var d = Vector3.ProjectOnPlane(graph.GetNodePosition(s) - graph.GetNodePosition(current), Vector3.up);
                    if (d.sqrMagnitude < 0.01f) continue;
                    var dot = Vector3.Dot(d.normalized, forward);
                    if (dot > bestDot) { bestDot = dot; next = s; }
                }
                if (next < 0) break;
                if (next == start && ring.Count >= 5)
                {
                    rings++;
                    ReportRing(graph, ring, report, halfWidth, rings);
                    foreach (var n in ring) seen.Add(n);
                    break;
                }
                if (ring.Contains(next)) break;
                current = next;
            }
        }
        if (rings == 0) report.AppendLine("aucun anneau ferme detecte.");
    }

    private static void ReportRing(LaneGraph graph, List<int> ring, StringBuilder report, float halfWidth, int index)
    {
        var centre = Vector3.zero;
        foreach (var n in ring) centre += graph.GetNodePosition(n);
        centre /= ring.Count;

        var radius = 0f;
        foreach (var n in ring) radius += Vector3.ProjectOnPlane(graph.GetNodePosition(n) - centre, Vector3.up).magnitude;
        radius /= ring.Count;

        var circumference = 0f;
        var maxGap = 0f;
        for (var i = 0; i < ring.Count; i++)
        {
            var a = graph.GetNodePosition(ring[i]);
            var b = graph.GetNodePosition(ring[(i + 1) % ring.Count]);
            var chord = Vector3.ProjectOnPlane(b - a, Vector3.up).magnitude;
            circumference += chord;
            if (chord > maxGap) maxGap = chord;
        }

        // Fleche : de combien la corde droite coupe A L'INTERIEUR du vrai cercle.
        var sagitta = radius - Mathf.Sqrt(Mathf.Max(0f, radius * radius - (maxGap * 0.5f) * (maxGap * 0.5f)));

        report.AppendLine("anneau #" + index + " : " + ring.Count + " noeuds, centre=" + F(centre));
        report.AppendLine("   rayon=" + radius.ToString("F2") + " m  perimetre_polyligne=" + circumference.ToString("F2")
            + " m  plus_grande_corde=" + maxGap.ToString("F2") + " m");
        report.AppendLine("   fleche de la corde = " + sagitta.ToString("F2") + " m   (demi-gabarit = " + halfWidth.ToString("F2") + " m)");
        report.AppendLine("   voies paralleles dans l'anneau : " + CountParallelLanes(graph, ring, centre, radius));
    }

    /// <summary>Combien d'axes distincts tournent a ce rayon ? 1 = anneau a file unique.</summary>
    private static int CountParallelLanes(LaneGraph graph, List<int> ring, Vector3 centre, float radius)
    {
        var radii = new List<float>();
        for (var i = 0; i < graph.NodeCount; i++)
        {
            var d = Vector3.ProjectOnPlane(graph.GetNodePosition(i) - centre, Vector3.up).magnitude;
            if (d > radius * 1.8f) continue;
            var merged = false;
            for (var r = 0; r < radii.Count; r++)
            {
                if (Mathf.Abs(radii[r] - d) < 1.2f) { merged = true; break; }
            }
            if (!merged) radii.Add(d);
        }
        radii.Sort();
        var text = "";
        foreach (var r in radii) text += r.ToString("F1") + " ";
        return radii.Count;
    }

    private static float CircleRadius(Vector3 a, Vector3 b, Vector3 c)
    {
        var ab = Vector3.ProjectOnPlane(b - a, Vector3.up).magnitude;
        var bc = Vector3.ProjectOnPlane(c - b, Vector3.up).magnitude;
        var ca = Vector3.ProjectOnPlane(a - c, Vector3.up).magnitude;
        var s = (ab + bc + ca) * 0.5f;
        var area = Mathf.Sqrt(Mathf.Max(0f, s * (s - ab) * (s - bc) * (s - ca)));
        return area < 0.0001f ? float.PositiveInfinity : (ab * bc * ca) / (4f * area);
    }

    private static float DistanceToLine(Vector3 point, Vector3 origin, Vector3 direction)
    {
        var offset = Vector3.ProjectOnPlane(point - origin, Vector3.up);
        return Vector3.ProjectOnPlane(offset - direction * Vector3.Dot(offset, direction), Vector3.up).magnitude;
    }

    private static string F(Vector3 v) => "(" + v.x.ToString("F2") + ";" + v.z.ToString("F2") + ")";
}
