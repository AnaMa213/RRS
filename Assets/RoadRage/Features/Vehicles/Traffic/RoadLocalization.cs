using System;
using System.Collections.Generic;
using UnityEngine;

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

        /// <summary>0 si l'enveloppe contient le point de reference, 1 sinon. Le rang prime sur le score.</summary>
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
    /// Localisation pure (Story 5.26) : fonction de (modele, pose d'empreinte, element precedent,
    /// elements de route). Aucun etat, aucune ecriture, aucun snap ni avancee de route : une pose a
    /// contresens ou hors enveloppe reste une localisation observable.
    ///
    /// Collecte : balayage lineaire des bornes de chaque element, elargies du voisinage (deux fois
    /// le seuil d'acceptation). Classement a deux rangs puis score en metres :
    /// <c>|lateral| + |normal| + depassement longitudinal + 2 m x |cap|/180</c>, moins l'hysteresis
    /// pour l'element precedent, moins la moitie pour un voisin explicite du precedent, moins la
    /// moitie pour un element de route (bonus cumulables). Le cap ne classe qu'a l'interieur d'un
    /// rang : il ne fait jamais preferer un element qui ne contient pas la pose a un element qui
    /// la contient. Le RoadId ne departage que les egalites exactes.
    /// </summary>
    public static class RoadLocalizer
    {
        /// <summary>Tolerance numerique de contenance dans l'enveloppe, en metres.</summary>
        private const float ContainEpsilonMeters = 1e-4f;

        /// <summary>Poids du cap : une erreur de 180 degres vaut 2 m de score.</summary>
        private const float HeadingWeightMeters = 2f;

        public static RoadLocation Localize(
            CompiledRoadModel model,
            VehicleFootprintPose pose,
            RoadId previousElementId,
            IReadOnlyList<RoadId> routeElementIds)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            ValidatePose(pose);

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

            var candidates = new List<RoadLocationCandidate>();
            var projections = new Dictionary<RoadId, RoadProjection>();
            for (int i = 0; i < model.Corridors.Count; i++)
            {
                Consider(query, RoadElementKind.LaneCorridor, model.Corridors[i].CorridorId, model.Corridors[i].Curve, candidates, projections);
            }

            for (int i = 0; i < model.Movements.Count; i++)
            {
                Consider(query, RoadElementKind.JunctionMovement, model.Movements[i].Id, model.Movements[i].Curve, candidates, projections);
            }

            candidates.Sort(Compare);

            // Le vivier de l'ambiguite : les candidats acceptes s'il y en a, sinon tous.
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
                if ((!localized || candidates[i].Accepted) && candidates[i].Rank == candidates[primary].Rank)
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

            if (FootprintOutsideEnvelope(pose.Footprint, reference, forward, right, projections[best.ElementId]))
            {
                location.Flags |= RoadLocationFlags.OutsideEnvelope;
            }

            return location;
        }

        // ------------------------------------------------------------------ interne

        private sealed class Query
        {
            public CompiledRoadModel Model;
            public RoadLocalizationProfile Profile;
            public Vector3 Reference;
            public Vector3 Forward;
            public RoadId Previous;
            public IReadOnlyList<RoadId> Route;
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
            float neighbourhood = 2f * query.Profile.AcceptanceDistanceMeters;
            var bounds = curve.FullBounds;
            bounds.Expand(2f * neighbourhood);
            if (!bounds.Contains(query.Reference))
            {
                return;
            }

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

            float score = Mathf.Abs(lateral) + Mathf.Abs(normal) + overrun + HeadingWeightMeters * Mathf.Abs(heading) / 180f;
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
                score -= 0.5f * hysteresis;
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
        private static bool FootprintOutsideEnvelope(
            VehicleFootprint footprint,
            Vector3 reference,
            Vector3 forward,
            Vector3 right,
            RoadProjection projection)
        {
            // Reference au-dela d'une extremite de l'element : hors enveloppe. Les coins ne sont pas
            // testes longitudinalement, sinon chaque franchissement de couture leverait le drapeau.
            if (projection.LongitudinalOverrunMeters > ContainEpsilonMeters)
            {
                return true;
            }

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
