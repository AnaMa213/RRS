#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    // =====================================================================================
    // Story 5.49 -- degagement a deux gabarits des giratoires, publie par instance.
    //
    // Deux gabarits max du profil versionne, cote a cote, cap tangent, marge m de chaque cote :
    // R_in = r_in + m + W/2 ; c_in = sqrt((R_in + W/2)^2 + (L/2)^2) ; R_out = c_in + 2m + W/2 ;
    // c_out = sqrt((R_out + W/2)^2 + (L/2)^2) ; residu = (r_out - m) - c_out. Evalue sur
    // l'enveloppe V2 appliquee et sur l'anneau physique mesure sur les colliders. Preuve
    // supplementaire seulement : un residu positif ne permet jamais de reduire la cible.
    // =====================================================================================

    public sealed class RoundaboutMeasurement
    {
        public V1Module Module;

        /// <summary>Enveloppe V2 appliquee : max des bords interieurs, min des bords exterieurs, residu.</summary>
        public float EnvelopeInnerRadius;
        public float EnvelopeOuterRadius;
        public float EnvelopeResidual;

        /// <summary>Portee max de <c>Col_Island</c> du module.</summary>
        public float IslandRadius;

        /// <summary>Min, sur 720 rayons, de la sortie de l'union des <c>Col_Roadway*</c>.</summary>
        public float PavedRadius;

        /// <summary>Collider non-chaussee le plus proche dans la tranche de sonde (meme au-dela du pave) ; nul si aucun.</summary>
        public string NearestObstacle;
        public float NearestObstacleRadius;

        public float PhysicalOuterRadius;
        public float PhysicalResidual;

        /// <summary>Story 5.52 : allocation a_e ajoutee a la marge de chaque cote, et pire |e| de l'anneau (gabarit tourne).</summary>
        public float AllowanceMeters;
        public float OffsetMaxRadians;
    }

    public static class RoundaboutClearance
    {
        public const string RoadwayPrefix = "Col_Roadway";
        public const string IslandName = "Col_Island";

        /// <summary>Hauteur de la tranche de sonde au-dessus du sommet de route.</summary>
        public const float ProbeHeightMeters = 2f;

        private const int RayCount = 720;
        private const float RayStepMeters = 0.01f;
        private const float RayMaxMeters = 60f;

        /// <summary>Tolerance d'affleurement : un collider dont le sommet egale celui de la route est dans la tranche, le sol a -0,05 m n'y est pas.</summary>
        private const float FlushToleranceMeters = 0.01f;

        /// <summary>
        /// Story 5.28 (reprise, correct-course 2026-09-25) : balayage conservateur de l'empreinte des 4
        /// giratoires sur les trajectoires finales 5.50 -- chaque mouvement du module prolonge de
        /// L/2 + marge + delta_c sur ses corridors adjacents, chaque corridor d'anneau entier -- par la
        /// fonction de la 5.51 (gabarit et marge du profil versionne gonfles de delta_c, tranche
        /// verticale, filtre d'obstacles, regle du relief). Publie les residus et l'empreinte des
        /// colliders mesures. Un residu non positif est un echec : HALT pour decision du proprietaire.
        /// </summary>
        public static JunctionClearanceResult Sweep(UnityEngine.SceneManagement.Scene scene, V1ImportResult import, CompiledRoadModel model,
            IReadOnlyList<JunctionClearanceSurface> sidewalks, GateAEvidenceParameters parameters = null, KinematicOffsetBounds bounds = null)
        {
            return JunctionClearance.Measure(scene, import, model, sidewalks, JunctionClearance.DefaultStepMeters, true, parameters, bounds);
        }

        /// <summary>Residu a deux gabarits (formule en tete de fichier), entrees du seul profil versionne.</summary>
        public static float Residual(float innerRadius, float outerRadius, RoadModelValidationProfile profile)
        {
            float halfWidth = profile.MaxVehicleHalfWidthMeters;
            float halfLength = 0.5f * profile.MaxVehicleLengthMeters;
            float margin = profile.LateralClearanceMarginMeters;
            float innerCentre = innerRadius + margin + halfWidth;
            float innerCorner = Mathf.Sqrt((innerCentre + halfWidth) * (innerCentre + halfWidth) + halfLength * halfLength);
            float outerCentre = innerCorner + 2f * margin + halfWidth;
            float outerCorner = Mathf.Sqrt((outerCentre + halfWidth) * (outerCentre + halfWidth) + halfLength * halfLength);
            return (outerRadius - margin) - outerCorner;
        }

        /// <summary>
        /// Residu a deux gabarits tournes (Story 5.52) : chaque gabarit fait l'angle <paramref name="offsetRadians"/>
        /// avec la tangente, et la marge de chaque cote vaut m + a_e. Le point le plus interieur est la distance du
        /// centre de l'anneau au rectangle tourne (bord, ou coin si le pied sort du bord), le plus exterieur le coin
        /// sqrt(R^2 + W^2 + (L/2)^2 + 2R(W cos e + (L/2)|sin e|)). Residu a cet angle seulement :
        /// WorstResidual couvre l'ensemble des caps. e = 0 et a_e = 0 : formule historique.
        /// </summary>
        public static float Residual(float innerRadius, float outerRadius, RoadModelValidationProfile profile,
            float trackingAllowanceMeters, float offsetRadians)
        {
            if (trackingAllowanceMeters == 0f && offsetRadians == 0f)
            {
                return Residual(innerRadius, outerRadius, profile);
            }

            double halfWidth = profile.MaxVehicleHalfWidthMeters;
            double halfLength = 0.5d * profile.MaxVehicleLengthMeters;
            double margin = profile.LateralClearanceMarginMeters + (double)trackingAllowanceMeters;
            double e = Math.Abs((double)offsetRadians);
            double innerCentre = CentreForInnermost(innerRadius + margin, e, halfLength, halfWidth);
            double innerCorner = OutermostCorner(innerCentre, e, halfLength, halfWidth);
            double outerCentre = CentreForInnermost(innerCorner + 2d * margin, e, halfLength, halfWidth);
            double outerCorner = OutermostCorner(outerCentre, e, halfLength, halfWidth);
            return (float)((outerRadius - margin) - outerCorner);
        }

        /// <summary>Plus petit rayon de centre dont le rectangle tourne de e reste a <paramref name="clearRadius"/> du centre de l'anneau.</summary>
        public static double CentreForInnermost(double clearRadius, double offsetRadians, double halfLength, double halfWidth)
        {
            double cos = Math.Cos(offsetRadians);
            double sin = Math.Abs(Math.Sin(offsetRadians));
            double edge = (clearRadius + halfWidth) / cos;
            if (edge * sin <= halfLength)
            {
                return edge;
            }

            // Le pied de la perpendiculaire sort du bord : le coin interieur est le plus proche.
            double p = halfLength * sin + halfWidth * cos;
            return p + Math.Sqrt(p * p - halfLength * halfLength - halfWidth * halfWidth + clearRadius * clearRadius);
        }

        /// <summary>Distance au centre de l'anneau du coin exterieur le plus eloigne d'un rectangle de centre R tourne de e.</summary>
        public static double OutermostCorner(double centreRadius, double offsetRadians, double halfLength, double halfWidth)
        {
            double cos = Math.Cos(offsetRadians);
            double sin = Math.Abs(Math.Sin(offsetRadians));
            return Math.Sqrt(centreRadius * centreRadius + halfWidth * halfWidth + halfLength * halfLength
                + 2d * centreRadius * (halfWidth * cos + halfLength * sin));
        }

        /// <summary>
        /// Pire residu sur |e| dans [0, offsetMax] : la portee radiale atteint son maximum a
        /// atan2(L/2, W) (W demi-largeur), puis decroit. Ce maximum depend du gabarit, pas d'un seuil fixe de 65 deg.
        /// Le sur-ensemble symetrique couvre aussi un intervalle de caps qui ne contient pas zero.
        /// </summary>
        public static float WorstResidual(float innerRadius, float outerRadius, RoadModelValidationProfile profile,
            float trackingAllowanceMeters, float offsetMaxRadians)
        {
            double peak = Math.Atan2(0.5d * profile.MaxVehicleLengthMeters, profile.MaxVehicleHalfWidthMeters);
            float worstOffset = (float)Math.Min(Math.Abs((double)offsetMaxRadians), peak);
            return Residual(innerRadius, outerRadius, profile, trackingAllowanceMeters, worstOffset);
        }

        /// <summary>
        /// Mesure de chaque giratoire de l'import : enveloppe V2 appliquee (corridors d'anneau et
        /// continuations du module, lus sur le modele compile) puis anneau physique (colliders de la
        /// scene du module). Un collider non supporte a portee du pave est un echec dur.
        /// </summary>
        public static List<RoundaboutMeasurement> Measure(V1ImportResult import, CompiledRoadModel model, List<string> failures,
            GateAEvidenceParameters parameters = null, KinematicOffsetBounds bounds = null)
        {
            parameters = parameters ?? GateAEvidenceParameters.Legacy;
            if (parameters.Kinematic && (bounds == null || !bounds.Closed))
            {
                failures.Add("Residus d'anneau cinematiques sans bornes d'ecart fermees (" + KinematicOffsetBounds.NotClosedCode + ").");
                return new List<RoundaboutMeasurement>();
            }

            var measurements = new List<RoundaboutMeasurement>();
            foreach (var module in import.SourceSet.Modules)
            {
                if (module.Kind != V1ModuleKind.Roundabout)
                {
                    continue;
                }

                if (module.Root == null)
                {
                    failures.Add("Giratoire '" + module.Label + "' sans racine d'instance : mesure impossible.");
                    continue;
                }

                var measurement = new RoundaboutMeasurement();
                measurement.Module = module;
                measurement.AllowanceMeters = parameters.TrackingAllowanceMeters;
                if (!MeasureEnvelope(import, model, module, measurement))
                {
                    failures.Add("Giratoire '" + module.Label + "' : aucun corridor d'anneau ni continuation dans le modele compile, enveloppe V2 vide.");
                    continue;
                }

                if (parameters.Kinematic)
                {
                    float offset;
                    string failure = RingOffsetMax(import, model, module, bounds, out offset);
                    if (failure != null)
                    {
                        failures.Add("Giratoire '" + module.Label + "' : " + failure);
                        continue;
                    }

                    measurement.OffsetMaxRadians = offset;
                }

                if (!parameters.IsLegacy)
                {
                    measurement.EnvelopeResidual = WorstResidual(measurement.EnvelopeInnerRadius, measurement.EnvelopeOuterRadius,
                        model.ValidationProfile, measurement.AllowanceMeters, measurement.OffsetMaxRadians);
                }

                if (MeasurePhysical(module, measurement, failures))
                {
                    measurement.PhysicalOuterRadius = measurement.NearestObstacle == null
                        ? measurement.PavedRadius
                        : Mathf.Min(measurement.PavedRadius, measurement.NearestObstacleRadius);
                    measurement.PhysicalResidual = WorstResidual(measurement.IslandRadius, measurement.PhysicalOuterRadius, model.ValidationProfile,
                        measurement.AllowanceMeters, measurement.OffsetMaxRadians);
                }

                measurements.Add(measurement);
            }

            return measurements;
        }

        /// <summary>Pire |e| atteignable sur les corridors d'anneau et les continuations du module (enveloppe entiere de chaque element).</summary>
        private static string RingOffsetMax(V1ImportResult import, CompiledRoadModel model, V1Module module, KinematicOffsetBounds bounds,
            out float offset)
        {
            offset = 0f;
            bool any = false;
            foreach (var corridor in import.Corridors)
            {
                if (!corridor.IsRing || corridor.Module != module) continue;
                string failure = Widen(bounds, import.IdOf(corridor.Key), ref offset, ref any);
                if (failure != null) return failure;
            }

            foreach (var movement in import.Movements)
            {
                if (movement.Role != MovementRole.RoundaboutContinuation || movement.Module != module) continue;
                string failure = Widen(bounds, import.IdOf(movement.Key), ref offset, ref any);
                if (failure != null) return failure;
            }

            return any ? null : "aucun element d'anneau dans les bornes d'ecart.";
        }

        private static string Widen(KinematicOffsetBounds bounds, RoadId id, ref float offset, ref bool any)
        {
            ElementOffsets offsets;
            if (!bounds.Elements.TryGetValue(id, out offsets)) return "element d'anneau " + id + " absent des bornes d'ecart.";
            double lo;
            double hi;
            if (!bounds.TryHull(id, offsets.Element.StartS, offsets.Element.EndS, out lo, out hi))
                return "element d'anneau " + id + " inatteignable.";
            offset = Mathf.Max(offset, (float)Math.Max(Math.Abs(lo), Math.Abs(hi)));
            any = true;
            return null;
        }

        // ------------------------------------------------------------------ enveloppe V2

        private static bool MeasureEnvelope(V1ImportResult import, CompiledRoadModel model, V1Module module, RoundaboutMeasurement measurement)
        {
            Vector3 centre = module.Root.position;
            float inner = float.NegativeInfinity;
            float outer = float.PositiveInfinity;
            foreach (var corridor in import.Corridors)
            {
                EffectiveLaneCorridor compiled;
                if (corridor.IsRing && corridor.Module == module && model.TryGetCorridor(import.IdOf(corridor.Key), out compiled))
                {
                    Edges(compiled.Samples, compiled.Curve, centre, ref inner, ref outer);
                }
            }

            foreach (var movement in import.Movements)
            {
                CompiledJunctionMovement compiled;
                if (movement.Role == MovementRole.RoundaboutContinuation && movement.Module == module && model.TryGetMovement(import.IdOf(movement.Key), out compiled))
                {
                    Edges(compiled.Samples, compiled.Curve, centre, ref inner, ref outer);
                }
            }

            if (float.IsInfinity(inner) || float.IsInfinity(outer))
            {
                return false;
            }

            measurement.EnvelopeInnerRadius = inner;
            measurement.EnvelopeOuterRadius = outer;
            measurement.EnvelopeResidual = Residual(inner, outer, model.ValidationProfile);
            return true;
        }

        /// <summary>Par echantillon : bord interieur = le plus proche du centre, exterieur = le plus lointain (plan XZ).</summary>
        private static void Edges(IReadOnlyList<RoadCurveSample> samples, RoadCurve curve, Vector3 centre, ref float inner, ref float outer)
        {
            foreach (var sample in samples)
            {
                var point = curve.Sample(sample.SMeters);
                float left = Flat(point.Position - point.Right * point.HalfWidthLeftMeters - centre);
                float right = Flat(point.Position + point.Right * point.HalfWidthRightMeters - centre);
                inner = Mathf.Max(inner, Mathf.Min(left, right));
                outer = Mathf.Min(outer, Mathf.Max(left, right));
            }
        }

        // ------------------------------------------------------------------ anneau physique

        /// <summary>
        /// ponytail: classement par nom de collider et tranche de 2 m, outil de mesure seulement ; a
        /// remplacer par une couche physique si un decor vient s'y ajouter.
        /// </summary>
        public static bool MeasurePhysical(V1Module module, RoundaboutMeasurement measurement, List<string> failures)
        {
            // En EditMode, Collider.bounds suit la physique, pas le Transform : une edition non synchronisee le laisserait perime.
            Physics.SyncTransforms();
            var root = module.Root;
            var centre = new Vector2(root.position.x, root.position.z);
            var colliders = new List<Collider>();
            foreach (var sceneRoot in root.gameObject.scene.GetRootGameObjects())
            {
                foreach (var collider in sceneRoot.GetComponentsInChildren<Collider>(true))
                {
                    if (collider.enabled && collider.gameObject.activeInHierarchy && !collider.isTrigger)
                    {
                        colliders.Add(collider);
                    }
                }
            }

            int before = failures.Count;
            Collider island = null;
            float roadTop = float.NegativeInfinity;
            var roadway = new List<Vector2[]>();
            foreach (var collider in colliders)
            {
                if (collider.name == IslandName && collider.transform.IsChildOf(root))
                {
                    if (island != null)
                    {
                        failures.Add("Giratoire '" + module.Label + "' : deux colliders " + IslandName + ".");
                    }

                    island = collider;
                }
                else if (collider.name.StartsWith(RoadwayPrefix, StringComparison.Ordinal))
                {
                    var hull = Footprint(collider);
                    if (hull == null)
                    {
                        // Loin du giratoire, une chaussee non supportee ne peut rien couvrir de l'anneau : ignoree.
                        if (BoundsDistance(collider.bounds, centre) >= RayMaxMeters)
                        {
                            continue;
                        }

                        failures.Add("Collider de chaussee non supporte '" + Describe(collider) + "' (Box ou MeshCollider convexe attendu) : mesure du giratoire '" + module.Label + "' impossible.");
                        continue;
                    }

                    if (Distance(hull, centre) < RayMaxMeters)
                    {
                        roadway.Add(hull);
                    }

                    if (collider.transform.IsChildOf(root))
                    {
                        roadTop = Mathf.Max(roadTop, collider.bounds.max.y);
                    }
                }
            }

            var islandHull = island == null ? null : Footprint(island);
            if (islandHull == null || islandHull.Length < 3)
            {
                failures.Add("Giratoire '" + module.Label + "' : " + IslandName + " absent ou non supporte.");
            }

            if (float.IsNegativeInfinity(roadTop))
            {
                failures.Add("Giratoire '" + module.Label + "' sans collider " + RoadwayPrefix + "* propre.");
            }

            if (failures.Count > before)
            {
                return false;
            }

            float islandRadius = 0f;
            foreach (var vertex in islandHull)
            {
                islandRadius = Mathf.Max(islandRadius, (vertex - centre).magnitude);
            }

            float paved = float.PositiveInfinity;
            for (int ray = 0; ray < RayCount; ray++)
            {
                float angle = ray * (2f * Mathf.PI / RayCount);
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float exit = 0f;
                // Un rayon plus long que le min courant ne peut plus l'abaisser : arret anticipe.
                for (int step = 1; step * RayStepMeters <= Mathf.Min(RayMaxMeters, paved + RayStepMeters); step++)
                {
                    if (!InsideAny(roadway, centre + direction * (step * RayStepMeters)))
                    {
                        break;
                    }

                    exit = step * RayStepMeters;
                }

                paved = Mathf.Min(paved, exit);
            }

            string nearest = null;
            float nearestRadius = float.PositiveInfinity;
            foreach (var collider in colliders)
            {
                if (collider == island || collider.name.StartsWith(RoadwayPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var bounds = collider.bounds;
                if (bounds.max.y < roadTop - FlushToleranceMeters || bounds.min.y > roadTop + ProbeHeightMeters)
                {
                    continue;
                }

                bool inReach = BoundsDistance(bounds, centre) < paved;
                var hull = Footprint(collider);
                if (hull == null)
                {
                    // Hors de portee du pave, un collider non supporte ne peut pas abaisser r_out : ignore.
                    if (!inReach)
                    {
                        continue;
                    }

                    failures.Add("Collider non supporte '" + Describe(collider) + "' (Box ou MeshCollider convexe attendu) dans la tranche de sonde, a portee du pave du giratoire '" + module.Label + "'.");
                    continue;
                }

                float distance = Distance(hull, centre);
                if (distance < nearestRadius)
                {
                    nearestRadius = distance;
                    nearest = Describe(collider);
                }
            }

            measurement.IslandRadius = islandRadius;
            measurement.PavedRadius = paved;
            measurement.NearestObstacle = nearest;
            measurement.NearestObstacleRadius = nearestRadius;
            return failures.Count == before;
        }

        /// <summary>Empreinte XZ : enveloppe convexe des coins d'un BoxCollider ou des sommets d'un MeshCollider convexe ; nul sinon.</summary>
        private static Vector2[] Footprint(Collider collider)
        {
            var points = new List<Vector2>();
            var transform = collider.transform;
            var box = collider as BoxCollider;
            var mesh = collider as MeshCollider;
            if (box != null)
            {
                for (int corner = 0; corner < 8; corner++)
                {
                    var local = box.center + Vector3.Scale(0.5f * box.size, new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f));
                    var world = transform.TransformPoint(local);
                    points.Add(new Vector2(world.x, world.z));
                }
            }
            else if (mesh != null && mesh.convex && mesh.sharedMesh != null)
            {
                foreach (var vertex in mesh.sharedMesh.vertices)
                {
                    var world = transform.TransformPoint(vertex);
                    points.Add(new Vector2(world.x, world.z));
                }
            }
            else
            {
                return null;
            }

            return ConvexHull(points);
        }

        /// <summary>Chaine monotone d'Andrew, sens trigonometrique, sans point colineaire.</summary>
        private static Vector2[] ConvexHull(List<Vector2> points)
        {
            points.Sort(delegate(Vector2 a, Vector2 b) { return a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y); });
            var hull = new Vector2[2 * points.Count];
            int count = 0;
            for (int i = 0; i < points.Count; i++)
            {
                while (count >= 2 && Cross(hull[count - 2], hull[count - 1], points[i]) <= 0f)
                {
                    count--;
                }

                hull[count++] = points[i];
            }

            for (int i = points.Count - 2, lower = count + 1; i >= 0; i--)
            {
                while (count >= lower && Cross(hull[count - 2], hull[count - 1], points[i]) <= 0f)
                {
                    count--;
                }

                hull[count++] = points[i];
            }

            Array.Resize(ref hull, Mathf.Max(count - 1, 0));
            return hull;
        }

        private static float Cross(Vector2 o, Vector2 a, Vector2 b)
        {
            return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
        }

        private static bool InsideAny(List<Vector2[]> hulls, Vector2 point)
        {
            foreach (var hull in hulls)
            {
                if (Inside(hull, point))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Inside(Vector2[] hull, Vector2 point)
        {
            for (int i = 0; i < hull.Length; i++)
            {
                if (Cross(hull[i], hull[(i + 1) % hull.Length], point) < 0f)
                {
                    return false;
                }
            }

            return hull.Length >= 3;
        }

        /// <summary>Distance du centre a un polygone convexe (0 si le centre est dedans).</summary>
        private static float Distance(Vector2[] hull, Vector2 point)
        {
            if (Inside(hull, point))
            {
                return 0f;
            }

            float best = float.PositiveInfinity;
            for (int i = 0; i < hull.Length; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Length];
                var ab = b - a;
                float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / ab.sqrMagnitude) : 0f;
                best = Mathf.Min(best, (a + t * ab - point).magnitude);
            }

            return best;
        }

        /// <summary>Distance horizontale du centre a la boite englobante monde (borne inferieure de la distance a l'empreinte).</summary>
        private static float BoundsDistance(Bounds bounds, Vector2 centre)
        {
            var min = new Vector2(bounds.min.x, bounds.min.z);
            var max = new Vector2(bounds.max.x, bounds.max.z);
            return (Vector2.Max(min, Vector2.Min(centre, max)) - centre).magnitude;
        }

        private static float Flat(Vector3 v)
        {
            return new Vector2(v.x, v.z).magnitude;
        }

        private static string Describe(Collider collider)
        {
            var parent = collider.transform.parent;
            string owner = parent == null ? string.Empty : (parent.parent != null ? parent.parent.name : parent.name) + "/";
            return owner + collider.name;
        }
    }
}
#endif
