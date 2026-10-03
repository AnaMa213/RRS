using System;
using System.Collections.Generic;
using UnityEngine;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Empreinte d'un vehicule (contrat de localisation AD-45) : l'origine de reference dans le
    /// repere chassis (x droite, y haut, z avant) et les extents avant/arriere/gauche/droite
    /// mesures depuis cette origine. Localisation, ligne d'arret et occupation lisent la meme.
    /// </summary>
    [Serializable]
    public struct VehicleFootprint
    {
        public Vector3 ReferenceOriginLocal;
        public float FrontMeters;
        public float RearMeters;
        public float LeftMeters;
        public float RightMeters;
    }

    /// <summary>Pose monde d'un chassis et son empreinte : l'entree de <see cref="RoadLocalizer"/>.</summary>
    [Serializable]
    public struct VehicleFootprintPose
    {
        /// <summary>Origine chassis, en monde.</summary>
        public Vector3 Position;

        public Vector3 Forward;
        public Vector3 Up;
        public VehicleFootprint Footprint;
    }

    public enum RoadElementKind
    {
        None = 0,
        LaneCorridor = 1,
        JunctionMovement = 2
    }

    /// <summary>Drapeaux composables : une pose deplacee a contresens les porte tous les deux.</summary>
    [Flags]
    public enum RoadLocationFlags
    {
        None = 0,

        /// <summary>La pose projetee ou l'empreinte depasse l'enveloppe de largeur de l'element retenu a s.</summary>
        OutsideEnvelope = 1,

        /// <summary>Erreur de cap absolue au-dela du seuil de contresens du profil.</summary>
        WrongWay = 2,

        /// <summary>Au moins deux candidats du meme rang dans la bande de score, element retenu ou non.</summary>
        Ambiguous = 4
    }

    /// <summary>Un element evalue par la localisation, retenu ou non.</summary>
    public struct RoadLocationCandidate
    {
        public RoadElementKind ElementKind;
        public RoadId ElementId;
        public float SMeters;
        public float LateralOffsetMeters;
        public float NormalOffsetMeters;
        public float HeadingErrorDegrees;

        /// <summary>Distance 3D du point de reference a l'enveloppe (0 dedans, lateral et longitudinal, plus le normal).</summary>
        public float DistanceToEnvelopeMeters;

        /// <summary>
        /// 0 si l'enveloppe (laterale et longitudinale) contient le point de reference, et pas un autre
        /// etage (<c>|normal|</c> dans le seuil d'acceptation) ; pour l'element precedent, un
        /// depassement jusqu'a l'hysteresis reste dans le rang 0. 1 sinon. Le rang prime sur le score.
        /// </summary>
        public int Rank;

        /// <summary>Score en metres, plus petit = meilleur, a l'interieur d'un rang.</summary>
        public float Score;

        /// <summary>Vrai si la distance a l'enveloppe passe le seuil d'acceptation.</summary>
        public bool Accepted;
    }

    /// <summary>
    /// Resultat de localisation (AD-45). <see cref="Localized"/> faux exclut toute identite
    /// d'element ; il peut porter des alternatives et <see cref="RoadLocationFlags.Ambiguous"/>.
    /// </summary>
    public struct RoadLocation
    {
        public RoadId ModelId;
        public RoadModelVersion ModelVersion;
        public bool Localized;
        public RoadElementKind ElementKind;
        public RoadId ElementId;
        public float SMeters;
        public float LateralOffsetMeters;
        public float NormalOffsetMeters;
        public float HeadingErrorDegrees;
        public RoadLocationFlags Flags;

        /// <summary>
        /// Causes de <see cref="RoadLocationFlags.OutsideEnvelope"/>, qui en est l'union : depassement longitudinal de
        /// l'element retenu (retention d'hysteresis a une couture, fin du portail de sortie) et debordement de
        /// l'enveloppe de largeur par la reference ou un coin de l'empreinte (contrat §8, criteres de contact, 2026-09-30).
        /// </summary>
        public float LongitudinalOverrunMeters;
        public bool OutsideWidthEnvelope;

        /// <summary>Deterministe dans [0, 1] : 1 sans rival du meme rang, 0 si non localise.</summary>
        public float Confidence;

        /// <summary>Candidats autres que l'element retenu, ordonnes par (rang, score, RoadId).</summary>
        public IReadOnlyList<RoadLocationCandidate> Alternatives;

        public bool HasFlag(RoadLocationFlags flag)
        {
            return (Flags & flag) == flag;
        }
    }

    /// <summary>
    /// Etat cinematique connu d'un vehicule (contrat §8) : l'ecart nominal e (radians) de sa route au debut
    /// d'un element, a l'abscisse <see cref="SMeters"/>. Le driver V2 les tire de sa reference de mesure.
    /// </summary>
    public readonly struct RoadKinematicAnchor
    {
        public readonly RoadId ElementId;
        public readonly float SMeters;
        public readonly float OffsetRadians;

        public RoadKinematicAnchor(RoadId elementId, float sMeters, float offsetRadians)
        {
            ElementId = elementId; SMeters = sMeters; OffsetRadians = offsetRadians;
        }
    }

    /// <summary>
    /// Localisation pure (Story 5.26) : fonction de (modele, pose d'empreinte, element precedent,
    /// elements de route, ancres cinematiques). Aucun etat, aucune ecriture, aucun snap ni avancee de
    /// route : une pose a contresens ou hors enveloppe reste une localisation observable.
    ///
    /// Collecte : balayage lineaire des bornes de chaque element, elargies du voisinage (deux fois
    /// le seuil d'acceptation). Classement a deux rangs puis score en metres :
    /// <c>|lateral| + |normal| + depassement longitudinal + 2 m x |ecart de cap|/180</c>, moins l'hysteresis
    /// pour l'element precedent, moins la moitie pour un voisin explicite du precedent, moins la bande de
    /// score pour un element de route (bonus cumulables ; 2026-09-30 : dans la bande d'ambiguite, la route
    /// decide). Le cap ne classe qu'a l'interieur d'un rang : il ne fait jamais preferer un element qui ne
    /// contient pas la pose a un element qui la contient. Le RoadId ne departage que les egalites exactes.
    ///
    /// Ecart de cap (2026-09-30). Sans ancre : cap de la caisse contre la tangente (pose tangente, e = 0).
    /// Avec ancres : contre l'orientation nominale du candidat, tangente tournee de -e, e etant transporte
    /// (de/ds = kappa - sin(e)/a, saut de tangente aux raccords) depuis l'ancre de l'element ou de son
    /// predecesseur explicite. Candidat sans ancre atteignable : bande morte |cap| - E, E = asin(a/R_admission)
    /// + tolerance de raccord, borne de |e| sur tout element admis (|kappa| &lt;= 1/R_admission), jamais le
    /// biais de la tangente.
    ///
    /// Le score additif ci-dessus, le cumul des bonus et le rayon de collecte de deux fois le seuil
    /// d'acceptation sont des CHOIX D'IMPLEMENTATION, pas des invariants d'architecture : le contrat
    /// de localisation (AD-45) exige un classement combinant geometrie, cap, route, element precedent
    /// et connectivite explicite, et un balayage qui ne manque aucun candidat pertinent. Un index
    /// spatial (AD-42 / 5.46) ou une autre ponderation des bonus peuvent les remplacer sans changer
    /// le contrat, tant que le cap ne classe jamais entre les rangs.
    /// </summary>
    public static class RoadLocalizer
    {
        /// <summary>Tolerance numerique de contenance dans l'enveloppe, en metres.</summary>
        private const float ContainEpsilonMeters = 1e-4f;

        /// <summary>Poids du cap : une erreur de 180 degres vaut 2 m de score.</summary>
        private const float HeadingWeightMeters = 2f;

        /// <param name="kinematics">Ecarts nominaux connus de la route du vehicule (contrat §8) ; nul : pose tangente.</param>
        public static RoadLocation Localize(
            CompiledRoadModel model,
            VehicleFootprintPose pose,
            RoadId previousElementId,
            IReadOnlyList<RoadId> routeElementIds,
            IReadOnlyList<RoadKinematicAnchor> kinematics = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            ValidatePose(pose);
            TrafficV2WorkCounters.Work.LocalizeCalls++;

            var profile = model.LocalizationProfile;
            Vector3 forward;
            Vector3 up;
            Vector3 right;
            ChassisFrame(pose, out forward, out up, out right);

            var origin = pose.Footprint.ReferenceOriginLocal;
            Vector3 reference = pose.Position + right * origin.x + up * origin.y + forward * origin.z;

            var query = new Query();
            query.Model = model;
            query.Profile = profile;
            query.Reference = reference;
            query.Forward = forward;
            query.Previous = previousElementId;
            query.Route = routeElementIds;
            var drivability = model.DrivabilityProfile;
            if (kinematics != null && drivability.Declared && drivability.ReferencePointAheadRearAxleMeters > 0f)
            {
                query.Kinematics = kinematics;
                query.ReferenceAheadMeters = drivability.ReferencePointAheadRearAxleMeters;
                query.OffsetBandDegrees = Mathf.Asin(Mathf.Min(1f, drivability.ReferencePointAheadRearAxleMeters
                    / RoadModelCompiler.AdmissionRadiusMeters(drivability))) * Mathf.Rad2Deg
                    + model.ValidationProfile.SeamTangentToleranceDegrees;
            }

            var candidates = new List<RoadLocationCandidate>();
            var projections = new Dictionary<RoadId, RoadProjection>();
            // Story 5.33, D15 : seuls les elements de la cellule du point (corridors puis mouvements, dans l'ordre du modele)
            // sont examines. La cellule contient tout element dont la boite elargie de Consider peut contenir le point : le
            // balayage complet ne retiendrait rien d'autre. Recherche globale par construction, ou que soit le vehicule.
            var index = ElementIndex.Of(model);
            var cell = index.Cell(reference);
            for (int k = 0; k < cell.Length; k++)
            {
                int e = cell[k];
                Consider(query, index.Kinds[e], index.Ids[e], index.Curves[e], candidates, projections);
            }

            candidates.Sort(Compare);

            // Le candidat retenu : le premier accepte dans l'ordre de tri (rang, score, RoadId).
            int chosen = -1;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Accepted)
                {
                    chosen = i;
                    break;
                }
            }

            bool localized = chosen >= 0;
            int primary = localized ? chosen : (candidates.Count > 0 ? 0 : -1);
            int second = -1;
            for (int i = primary + 1; primary >= 0 && i < candidates.Count; i++)
            {
                // Contrat AD-45 : le rival est tout candidat du meme rang, accepte ou non -- le drapeau
                // decrit l'ensemble des candidats, pas le seul resultat retenu.
                if (candidates[i].Rank == candidates[primary].Rank)
                {
                    second = i;
                    break;
                }
            }

            float margin = second >= 0 ? candidates[second].Score - candidates[primary].Score : float.PositiveInfinity;
            bool ambiguous = second >= 0 && margin < profile.ScoreBandMeters;

            var location = new RoadLocation();
            location.ModelId = model.ModelId;
            location.ModelVersion = model.Version;
            location.Localized = localized;
            location.Flags = ambiguous ? RoadLocationFlags.Ambiguous : RoadLocationFlags.None;

            var alternatives = new List<RoadLocationCandidate>(candidates.Count);
            for (int i = 0; i < candidates.Count; i++)
            {
                if (i != chosen)
                {
                    alternatives.Add(candidates[i]);
                }
            }

            location.Alternatives = alternatives.AsReadOnly();

            if (!localized)
            {
                location.ElementKind = RoadElementKind.None;
                location.ElementId = RoadId.None;
                location.Confidence = 0f;
                return location;
            }

            var best = candidates[chosen];
            location.ElementKind = best.ElementKind;
            location.ElementId = best.ElementId;
            location.SMeters = best.SMeters;
            location.LateralOffsetMeters = best.LateralOffsetMeters;
            location.NormalOffsetMeters = best.NormalOffsetMeters;
            location.HeadingErrorDegrees = best.HeadingErrorDegrees;
            location.Confidence = second < 0 || !(profile.ScoreBandMeters > 0f) ? 1f : Mathf.Clamp01(margin / profile.ScoreBandMeters);

            if (Mathf.Abs(best.HeadingErrorDegrees) > profile.WrongWayHeadingDegrees)
            {
                location.Flags |= RoadLocationFlags.WrongWay;
            }

            var accepted = projections[best.ElementId];
            location.LongitudinalOverrunMeters = accepted.LongitudinalOverrunMeters;
            location.OutsideWidthEnvelope = FootprintOutsideWidthEnvelope(pose.Footprint, reference, forward, right, accepted);
            if (accepted.LongitudinalOverrunMeters > ContainEpsilonMeters || location.OutsideWidthEnvelope)
            {
                location.Flags |= RoadLocationFlags.OutsideEnvelope;
            }

            return location;
        }

        // ------------------------------------------------------------------ interne

        /// <summary>
        /// Index spatial des elements d'un modele compile (Story 5.33, D15), construit une fois par modele : grille XZ de
        /// cellules de 8 m, chaque element range dans toutes les cellules que touche sa boite elargie de Consider (plus 5 cm,
        /// pour que l'arrondi ne puisse jamais l'exclure). Les elements d'une cellule sont tries dans l'ordre du balayage
        /// complet (corridors puis mouvements). Le modele est immuable : aucune invalidation.
        /// </summary>
        private sealed class ElementIndex
        {
            private const float CellMeters = 8f;
            private const float SlackMeters = 0.05f;
            private static readonly int[] Empty = new int[0];
            private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<CompiledRoadModel, ElementIndex> Indexes =
                new System.Runtime.CompilerServices.ConditionalWeakTable<CompiledRoadModel, ElementIndex>();

            public RoadElementKind[] Kinds;
            public RoadId[] Ids;
            public RoadCurve[] Curves;
            private float _originX, _originZ;
            private int _cellsX, _cellsZ;
            private int[][] _cells;

            public static ElementIndex Of(CompiledRoadModel model)
            {
                return Indexes.GetValue(model, Build);
            }

            public int[] Cell(Vector3 point)
            {
                if (_cells == null) return Empty;
                int x = (int)Math.Floor((point.x - _originX) / CellMeters), z = (int)Math.Floor((point.z - _originZ) / CellMeters);
                if (x < 0 || z < 0 || x >= _cellsX || z >= _cellsZ) return Empty;
                return _cells[z * _cellsX + x];
            }

            private static ElementIndex Build(CompiledRoadModel model)
            {
                int count = model.Corridors.Count + model.Movements.Count;
                var index = new ElementIndex { Kinds = new RoadElementKind[count], Ids = new RoadId[count], Curves = new RoadCurve[count] };
                for (int i = 0; i < model.Corridors.Count; i++)
                {
                    index.Kinds[i] = RoadElementKind.LaneCorridor; index.Ids[i] = model.Corridors[i].CorridorId; index.Curves[i] = model.Corridors[i].Curve;
                }
                for (int i = 0; i < model.Movements.Count; i++)
                {
                    int e = model.Corridors.Count + i;
                    index.Kinds[e] = RoadElementKind.JunctionMovement; index.Ids[e] = model.Movements[i].Id; index.Curves[e] = model.Movements[i].Curve;
                }
                if (count == 0) return index;
                // Meme boite que Consider : enveloppe complete elargie de 2 x voisinage, voisinage = 2 x acceptation.
                float neighbourhood = 2f * model.LocalizationProfile.AcceptanceDistanceMeters;
                var boxes = new Bounds[count];
                float minX = float.PositiveInfinity, minZ = float.PositiveInfinity, maxX = float.NegativeInfinity, maxZ = float.NegativeInfinity;
                for (int e = 0; e < count; e++)
                {
                    var bounds = index.Curves[e].FullBounds;
                    bounds.Expand(2f * neighbourhood + 2f * SlackMeters);
                    boxes[e] = bounds;
                    minX = Mathf.Min(minX, bounds.min.x); minZ = Mathf.Min(minZ, bounds.min.z);
                    maxX = Mathf.Max(maxX, bounds.max.x); maxZ = Mathf.Max(maxZ, bounds.max.z);
                }
                if (float.IsInfinity(minX) || float.IsNaN(minX) || float.IsNaN(maxX)) return index;
                index._originX = minX; index._originZ = minZ;
                index._cellsX = (int)Math.Floor((maxX - minX) / CellMeters) + 1;
                index._cellsZ = (int)Math.Floor((maxZ - minZ) / CellMeters) + 1;
                var lists = new List<int>[index._cellsX * index._cellsZ];
                for (int e = 0; e < count; e++)
                {
                    int x0 = Math.Max(0, (int)Math.Floor((boxes[e].min.x - minX) / CellMeters));
                    int x1 = Math.Min(index._cellsX - 1, (int)Math.Floor((boxes[e].max.x - minX) / CellMeters));
                    int z0 = Math.Max(0, (int)Math.Floor((boxes[e].min.z - minZ) / CellMeters));
                    int z1 = Math.Min(index._cellsZ - 1, (int)Math.Floor((boxes[e].max.z - minZ) / CellMeters));
                    for (int z = z0; z <= z1; z++)
                        for (int x = x0; x <= x1; x++)
                        {
                            var list = lists[z * index._cellsX + x];
                            if (list == null) lists[z * index._cellsX + x] = list = new List<int>();
                            list.Add(e);
                        }
                }
                index._cells = new int[lists.Length][];
                for (int c = 0; c < lists.Length; c++) index._cells[c] = lists[c] == null ? Empty : lists[c].ToArray();
                return index;
            }
        }

        /// <summary>
        /// Elements que la localisation examine pour ce point (Story 5.33, D15) : ceux de sa cellule, dans l'ordre du balayage
        /// complet. Expose pour la preuve d'inclusion ; aucun autre usage.
        /// </summary>
        public static IReadOnlyList<RoadId> ExaminedElements(CompiledRoadModel model, Vector3 point)
        {
            if (model == null) throw new ArgumentNullException("model");
            var index = ElementIndex.Of(model);
            var cell = index.Cell(point);
            var ids = new RoadId[cell.Length];
            for (int k = 0; k < cell.Length; k++) ids[k] = index.Ids[cell[k]];
            return ids;
        }

        private sealed class Query
        {
            public CompiledRoadModel Model;
            public RoadLocalizationProfile Profile;
            public Vector3 Reference;
            public Vector3 Forward;
            public RoadId Previous;
            public IReadOnlyList<RoadId> Route;
            public IReadOnlyList<RoadKinematicAnchor> Kinematics;
            public float ReferenceAheadMeters;
            public float OffsetBandDegrees;
        }

        /// <exception cref="ArgumentException">Pose non finie, repere chassis degenere ou extent negatif.</exception>
        private static void ValidatePose(VehicleFootprintPose pose)
        {
            var footprint = pose.Footprint;
            if (!IsFinite(pose.Position) || !IsFinite(pose.Forward) || !IsFinite(pose.Up) || !IsFinite(footprint.ReferenceOriginLocal))
            {
                throw new ArgumentException("Pose d'empreinte non finie.", "pose");
            }

            if (pose.Forward.sqrMagnitude < 1e-12f || pose.Up.sqrMagnitude < 1e-12f
                || Vector3.Cross(pose.Forward.normalized, pose.Up.normalized).sqrMagnitude < 1e-8f)
            {
                throw new ArgumentException("Repere chassis degenere : avant ou haut nul, ou avant parallele au haut.", "pose");
            }

            if (!(footprint.FrontMeters >= 0f && footprint.RearMeters >= 0f && footprint.LeftMeters >= 0f && footprint.RightMeters >= 0f)
                || float.IsInfinity(footprint.FrontMeters) || float.IsInfinity(footprint.RearMeters)
                || float.IsInfinity(footprint.LeftMeters) || float.IsInfinity(footprint.RightMeters))
            {
                throw new ArgumentException("Extent d'empreinte negatif ou non fini.", "pose");
            }
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x)
                && !float.IsNaN(value.y) && !float.IsInfinity(value.y)
                && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static void ChassisFrame(VehicleFootprintPose pose, out Vector3 forward, out Vector3 up, out Vector3 right)
        {
            forward = pose.Forward.normalized;
            up = (pose.Up - forward * Vector3.Dot(pose.Up, forward)).normalized;
            right = Vector3.Cross(up, forward).normalized;
        }

        private static void Consider(
            Query query,
            RoadElementKind kind,
            RoadId id,
            RoadCurve curve,
            List<RoadLocationCandidate> candidates,
            Dictionary<RoadId, RoadProjection> projections)
        {
            // Rayon de collecte : choix d'implementation (2 x acceptation), pas un invariant de contrat.
            float neighbourhood = 2f * query.Profile.AcceptanceDistanceMeters;
            var bounds = curve.FullBounds;
            bounds.Expand(2f * neighbourhood);
            TrafficV2WorkCounters.Work.LocalizeScanned++;
            if (!bounds.Contains(query.Reference))
            {
                return;
            }

            TrafficV2WorkCounters.Work.LocalizeProjected++;
            var projection = curve.Project(query.Reference);
            var at = projection.Point;
            float lateral = projection.LateralOffsetMeters;
            float lateralExcess = Mathf.Max(0f, Mathf.Max(lateral - at.HalfWidthRightMeters, -at.HalfWidthLeftMeters - lateral));
            float overrun = projection.LongitudinalOverrunMeters;
            float planar = Mathf.Sqrt(lateralExcess * lateralExcess + overrun * overrun);
            float normal = projection.NormalOffsetMeters;
            float distance = Mathf.Sqrt(planar * planar + normal * normal);
            if (!(distance <= neighbourhood))
            {
                return;
            }

            bool isPrevious = !query.Previous.IsEmpty && id == query.Previous;
            float hysteresis = query.Profile.HysteresisMeters;
            float heading = at.SignedHeadingDegrees(query.Forward);

            float headingGap = Mathf.Abs(heading);
            if (query.Kinematics != null)
            {
                // Caisse contre l'orientation nominale du candidat (tangente tournee de -e) ; sans ancre atteignable,
                // bande morte bornant |e| sur tout element admis.
                double offset;
                headingGap = TryNominalOffset(query, kind, id, curve, projection.SMeters, out offset)
                    ? Mathf.Abs(Mathf.DeltaAngle(0f, heading + (float)offset * Mathf.Rad2Deg))
                    : Mathf.Max(0f, Mathf.Abs(heading) - query.OffsetBandDegrees);
            }

            float score = Mathf.Abs(lateral) + Mathf.Abs(normal) + overrun + HeadingWeightMeters * headingGap / 180f;
            if (isPrevious)
            {
                score -= hysteresis;
            }

            if (IsExplicitNeighbourOfPrevious(query.Model, query.Previous, id))
            {
                score -= 0.5f * hysteresis;
            }

            if (Contains(query.Route, id))
            {
                score -= query.Profile.ScoreBandMeters;
            }

            var candidate = new RoadLocationCandidate();
            candidate.ElementKind = kind;
            candidate.ElementId = id;
            candidate.SMeters = projection.SMeters;
            candidate.LateralOffsetMeters = lateral;
            candidate.NormalOffsetMeters = normal;
            candidate.HeadingErrorDegrees = heading;
            candidate.DistanceToEnvelopeMeters = distance;
            // Contenance : enveloppe laterale et longitudinale, et pas un autre etage (un tablier
            // superpose contient la pose en plan mais pas en hauteur).
            bool contained = planar <= (isPrevious ? Mathf.Max(hysteresis, ContainEpsilonMeters) : ContainEpsilonMeters)
                && Mathf.Abs(normal) <= query.Profile.AcceptanceDistanceMeters;
            candidate.Rank = contained ? 0 : 1;
            candidate.Score = score;
            candidate.Accepted = distance <= query.Profile.AcceptanceDistanceMeters;
            TrafficV2WorkCounters.Work.LocalizeCandidates++;
            candidates.Add(candidate);
            projections[id] = projection;
        }

        /// <summary>Connectivite explicite seulement : connexions et extremites de mouvement, jamais la proximite.</summary>
        private static bool IsExplicitNeighbourOfPrevious(CompiledRoadModel model, RoadId previous, RoadId candidate)
        {
            if (previous.IsEmpty || previous == candidate)
            {
                return false;
            }

            EffectiveLaneCorridor corridor;
            if (model.TryGetCorridor(previous, out corridor))
            {
                if (Contains(model.GetSuccessorCorridors(previous), candidate) || Contains(model.GetPredecessorCorridors(previous), candidate))
                {
                    return true;
                }

                CompiledJunctionMovement movement;
                return model.TryGetMovement(candidate, out movement)
                    && (movement.FromCorridorId == previous || movement.ToCorridorId == previous);
            }

            CompiledJunctionMovement previousMovement;
            return model.TryGetMovement(previous, out previousMovement)
                && (previousMovement.FromCorridorId == candidate || previousMovement.ToCorridorId == candidate);
        }

        /// <summary>
        /// e nominal (radians) du candidat a <paramref name="s"/> : transporte depuis l'ancre de l'element la plus
        /// proche en amont de s (une ancre en aval donne son e tel quel), sinon depuis l'ancre d'un predecesseur
        /// explicite, portee jusqu'a sa fin puis au-dela du saut de tangente du raccord. Faux sans ancre atteignable.
        /// </summary>
        private static bool TryNominalOffset(Query query, RoadElementKind kind, RoadId id, RoadCurve curve, float s,
            out double offset)
        {
            float a = query.ReferenceAheadMeters;
            int own = -1;
            for (int i = 0; i < query.Kinematics.Count; i++)
            {
                var anchor = query.Kinematics[i];
                if (anchor.ElementId != id) continue;
                if (own < 0) { own = i; continue; }
                var best = query.Kinematics[own];
                bool before = anchor.SMeters <= s, bestBefore = best.SMeters <= s;
                if ((before && (!bestBefore || anchor.SMeters > best.SMeters))
                    || (!before && !bestBefore && anchor.SMeters < best.SMeters))
                    own = i;
            }

            if (own >= 0)
            {
                var anchor = query.Kinematics[own];
                offset = curve.AdvanceKinematicOffset(anchor.SMeters, s, anchor.OffsetRadians, a);
                return true;
            }

            for (int i = 0; i < query.Kinematics.Count; i++)
            {
                var anchor = query.Kinematics[i];
                RoadCurve predecessor;
                if (!IsExplicitSuccessor(query.Model, anchor.ElementId, kind, id, out predecessor)) continue;
                double end = predecessor.AdvanceKinematicOffset(anchor.SMeters, predecessor.Length, anchor.OffsetRadians, a);
                end += RoadCurve.SignedTangentJumpRadians(predecessor.Sample(predecessor.Length), curve.Sample(curve.StartS));
                offset = curve.AdvanceKinematicOffset(curve.StartS, s, end, a);
                return true;
            }

            offset = 0d;
            return false;
        }

        /// <summary>Vrai si <paramref name="to"/> commence la ou <paramref name="from"/> finit (connexion ou extremite de mouvement).</summary>
        private static bool IsExplicitSuccessor(CompiledRoadModel model, RoadId from, RoadElementKind toKind, RoadId to,
            out RoadCurve fromCurve)
        {
            EffectiveLaneCorridor corridor;
            CompiledJunctionMovement movement;
            if (model.TryGetCorridor(from, out corridor))
            {
                fromCurve = corridor.Curve;
                return toKind == RoadElementKind.JunctionMovement
                    ? model.TryGetMovement(to, out movement) && movement.FromCorridorId == from
                    : toKind == RoadElementKind.LaneCorridor && Contains(model.GetSuccessorCorridors(from), to);
            }

            if (model.TryGetMovement(from, out movement))
            {
                fromCurve = movement.Curve;
                return toKind == RoadElementKind.LaneCorridor && movement.ToCorridorId == to;
            }

            fromCurve = null;
            return false;
        }

        private static bool Contains(IReadOnlyList<RoadId> ids, RoadId id)
        {
            if (ids == null)
            {
                return false;
            }

            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static int Compare(RoadLocationCandidate a, RoadLocationCandidate b)
        {
            if (a.Rank != b.Rank)
            {
                return a.Rank < b.Rank ? -1 : 1;
            }

            if (a.Score != b.Score)
            {
                return a.Score < b.Score ? -1 : 1;
            }

            return a.ElementId.CompareTo(b.ElementId);
        }

        /// <summary>
        /// Coins de l'empreinte et point de reference, lateraux mesures dans le repere de l'element
        /// retenu a l'abscisse du point de reference.
        /// </summary>
        private static bool FootprintOutsideWidthEnvelope(
            VehicleFootprint footprint,
            Vector3 reference,
            Vector3 forward,
            Vector3 right,
            RoadProjection projection)
        {
            // Largeur seulement : le depassement longitudinal de la reference est l'autre cause du drapeau.
            // Les coins ne sont pas testes longitudinalement, sinon chaque franchissement de couture leverait le drapeau.
            var at = projection.Point;
            var corners = new[]
            {
                reference,
                reference + forward * footprint.FrontMeters + right * footprint.RightMeters,
                reference + forward * footprint.FrontMeters - right * footprint.LeftMeters,
                reference - forward * footprint.RearMeters + right * footprint.RightMeters,
                reference - forward * footprint.RearMeters - right * footprint.LeftMeters
            };

            for (int i = 0; i < corners.Length; i++)
            {
                float lateral = Vector3.Dot(corners[i] - at.Position, at.Right);
                if (lateral > at.HalfWidthRightMeters || lateral < -at.HalfWidthLeftMeters)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
