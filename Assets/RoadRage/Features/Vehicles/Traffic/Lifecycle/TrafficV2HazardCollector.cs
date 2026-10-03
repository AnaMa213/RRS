using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using Unity.Profiling;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Lifecycle
{
    /// <summary>Classe d'une racine rencontree par le collecteur de dangers (5.33).</summary>
    public enum HazardRootClass
    {
        /// <summary>Ni Rigidbody ni CharacterController : decor statique, ecarte et compte.</summary>
        Static = 0,
        /// <summary>Le vehicule qui emet la requete : ecarte et compte.</summary>
        Self = 1,
        /// <summary>Racine portant un TrafficV2VehicleDriver, liee ou non : un acteur de la frame, jamais un danger.</summary>
        TrafficV2Vehicle = 2,
        WalkingPlayer = 3,
        Vehicle = 4,
        Obstacle = 5
    }

    /// <summary>Requete d'un vehicule V2 lie : son identite, le centre de la sphere et son propre corps.</summary>
    public readonly struct HazardQuery
    {
        public readonly RoadId VehicleId;
        public readonly Vector3 Center;
        public readonly Rigidbody Self;

        public HazardQuery(RoadId vehicleId, Vector3 center, Rigidbody self)
        {
            VehicleId = vehicleId; Center = center; Self = self;
        }
    }

    /// <summary>Requete d'un vehicule : colliders rendus et saturation, publiee avec le vehicule emetteur.</summary>
    public readonly struct HazardQueryReport
    {
        public readonly RoadId VehicleId;
        public readonly int Hits;
        /// <summary>Requete pleine (resultats = capacite) : un danger a pu manquer.</summary>
        public readonly bool Saturated;

        public HazardQueryReport(RoadId vehicleId, int hits, bool saturated)
        {
            VehicleId = vehicleId; Hits = hits; Saturated = saturated;
        }
    }

    /// <summary>
    /// Collecteur hote des dangers physiques (Story 5.33). Une requete OverlapSphereNonAlloc par vehicule V2 et par pas,
    /// triggers ignores, tampon fixe. Colliders dedupliques par racine (attachedRigidbody, sinon CharacterController),
    /// puis classes : CharacterController -> WalkingPlayer ; Rigidbody portant VehiclePhysicsBody ou
    /// NetworkedAIVehicleState -> Vehicle ; autre Rigidbody -> Obstacle. Decor statique, vehicule propre et toute racine
    /// V2 sont ecartes et comptes : un vehicule V2 n'est jamais aussi un danger. Boite = AABB monde du collider (union
    /// par racine), vitesse du corps, confiance 1. Identite d'un domaine danger, stable pendant la session ; un id egal a
    /// celui d'un acteur n'est pas emis (HazardIdentityCollision). Aucune classification par motif de nom ; aucune decision.
    /// </summary>
    public sealed class TrafficV2HazardCollector
    {
        /// <summary>Mot haut du domaine d'identite des dangers ; le mot bas est un compteur de session.</summary>
        public const ulong HazardIdentityDomain = 0x48415A4152440000UL;

        private sealed class Sighting
        {
            public HazardRootClass Class;
            public Bounds Bounds;
            public Vector3 Velocity;
            public RoadId Id;
        }

        private readonly Collider[] hits;
        private readonly float radius;
        private readonly Dictionary<UnityEngine.Object, RoadId> identities = new Dictionary<UnityEngine.Object, RoadId>();
        private readonly Dictionary<UnityEngine.Object, Sighting> sightings = new Dictionary<UnityEngine.Object, Sighting>();
        private readonly List<Sighting> order = new List<Sighting>();
        private readonly List<TrafficHazardInput> hazards = new List<TrafficHazardInput>();
        private readonly List<HazardQueryReport> reports = new List<HazardQueryReport>();
        private ulong nextIdentity;

        public TrafficV2HazardCollector(int capacity, float radiusMeters)
        {
            if (capacity < 1) throw new ArgumentException("InvalidHazardCapacity", "capacity");
            if (!(radiusMeters > 0f) || float.IsInfinity(radiusMeters)) throw new ArgumentException("InvalidHazardRadius", "radiusMeters");
            hits = new Collider[capacity];
            radius = radiusMeters;
        }

        public int Capacity { get { return hits.Length; } }
        public float RadiusMeters { get { return radius; } }
        /// <summary>Requetes du dernier pas, dans l'ordre des vehicules.</summary>
        public IReadOnlyList<HazardQueryReport> Reports { get { return reports; } }
        public TrafficHazardCollectorCounters Counters { get; private set; }
        /// <summary>Plus grand nombre de colliders rendus par une requete depuis le debut de la session.</summary>
        public int MaxHits { get; private set; }
        /// <summary>Requetes saturees depuis le debut de la session.</summary>
        public int SaturatedQueries { get; private set; }

        /// <summary>Classification d'une racine, independante de la physique Unity.</summary>
        public static HazardRootClass Classify(bool isSelf, bool hasCharacterController, bool hasRigidbody, bool hasVehicleBody,
            bool hasTrafficV2Driver)
        {
            if (!hasCharacterController && !hasRigidbody) return HazardRootClass.Static;
            if (isSelf) return HazardRootClass.Self;
            if (hasTrafficV2Driver) return HazardRootClass.TrafficV2Vehicle;
            if (hasCharacterController) return HazardRootClass.WalkingPlayer;
            return hasVehicleBody ? HazardRootClass.Vehicle : HazardRootClass.Obstacle;
        }

        public bool TryGetReport(RoadId vehicleId, out HazardQueryReport report)
        {
            for (int i = 0; i < reports.Count; i++)
                if (reports[i].VehicleId == vehicleId) { report = reports[i]; return true; }
            report = default(HazardQueryReport);
            return false;
        }

        /// <summary>Dangers du pas, une entree par racine (ordre de premiere rencontre), identites hors de celles des acteurs.</summary>
        /// <summary>Requetes OverlapSphereNonAlloc du dernier Collect et leur temps cumule (ms) : diagnostic de performance.</summary>
        public int LastOverlapCount { get; private set; }
        public double LastOverlapMilliseconds { get; private set; }

        private static readonly ProfilerMarker OverlapMarker = new ProfilerMarker("TrafficV2.Collect.Overlap");
        private readonly System.Diagnostics.Stopwatch overlapWatch = new System.Diagnostics.Stopwatch();

        public IReadOnlyList<TrafficHazardInput> Collect(IReadOnlyList<HazardQuery> queries, ICollection<RoadId> actorIds)
        {
            if (queries == null) throw new ArgumentNullException("queries");
            sightings.Clear(); order.Clear(); hazards.Clear(); reports.Clear();
            int saturated = 0, total = 0, excludedStatic = 0, excludedSelf = 0, excludedV2 = 0, collisions = 0, nonFinite = 0;
            overlapWatch.Reset();
            LastOverlapCount = queries.Count;
            for (int q = 0; q < queries.Count; q++)
            {
                var query = queries[q];
                overlapWatch.Start();
                OverlapMarker.Begin();
                int found = Physics.OverlapSphereNonAlloc(query.Center, radius, hits, ~0, QueryTriggerInteraction.Ignore);
                OverlapMarker.End();
                overlapWatch.Stop();
                bool full = found >= hits.Length;
                if (full) { saturated++; SaturatedQueries++; }
                MaxHits = Math.Max(MaxHits, found);
                total += found;
                reports.Add(new HazardQueryReport(query.VehicleId, found, full));
                for (int i = 0; i < found; i++)
                {
                    var collider = hits[i];
                    if (collider == null) { excludedStatic++; continue; }
                    var body = collider.attachedRigidbody;
                    var character = body == null ? collider as CharacterController : body.GetComponent<CharacterController>();
                    UnityEngine.Object root = body != null ? (UnityEngine.Object)body : character;
                    bool v2 = root != null && (body != null ? body.GetComponent<TrafficV2VehicleDriver>() : character.GetComponent<TrafficV2VehicleDriver>()) != null;
                    bool vehicleBody = body != null
                        && (body.GetComponent<VehiclePhysicsBody>() != null || body.GetComponent<NetworkedAIVehicleState>() != null);
                    var kind = Classify(body != null && body == query.Self, character != null, body != null, vehicleBody, v2);
                    if (kind == HazardRootClass.Static) { excludedStatic++; continue; }
                    if (kind == HazardRootClass.Self) { excludedSelf++; continue; }
                    if (kind == HazardRootClass.TrafficV2Vehicle) { excludedV2++; continue; }
                    Sighting sighting;
                    if (sightings.TryGetValue(root, out sighting)) { sighting.Bounds.Encapsulate(collider.bounds); continue; }
                    sighting = new Sighting { Class = kind, Bounds = collider.bounds, Id = IdentityOf(root),
                        Velocity = body != null ? body.linearVelocity : character.velocity };
                    sightings.Add(root, sighting);
                    order.Add(sighting);
                }
            }
            for (int i = 0; i < order.Count; i++)
            {
                var sighting = order[i];
                if (actorIds != null && actorIds.Contains(sighting.Id)) { collisions++; continue; }
                var bounds = sighting.Bounds;
                if (!Finite(bounds.center) || !Finite(bounds.extents) || !Finite(sighting.Velocity)) { nonFinite++; continue; }
                hazards.Add(new TrafficHazardInput(sighting.Id, KindOf(sighting.Class),
                    new RoadBoundsBox { Center = bounds.center, Extents = bounds.extents }, sighting.Velocity, 1f));
            }
            Counters = new TrafficHazardCollectorCounters(queries.Count, saturated, total, hazards.Count, excludedStatic,
                excludedSelf, excludedV2, collisions, nonFinite);
            LastOverlapMilliseconds = overlapWatch.Elapsed.TotalMilliseconds;
            return hazards;
        }

        /// <summary>Identite stable pendant la session : premiere rencontre d'une racine, compteur croissant.</summary>
        private RoadId IdentityOf(UnityEngine.Object root)
        {
            RoadId id;
            if (!identities.TryGetValue(root, out id))
            {
                id = new RoadId(HazardIdentityDomain, ++nextIdentity);
                identities.Add(root, id);
            }
            return id;
        }

        private static TrafficHazardKind KindOf(HazardRootClass kind)
        {
            switch (kind)
            {
                case HazardRootClass.WalkingPlayer: return TrafficHazardKind.WalkingPlayer;
                case HazardRootClass.Vehicle: return TrafficHazardKind.Vehicle;
                default: return TrafficHazardKind.Obstacle;
            }
        }

        private static bool Finite(Vector3 v)
        {
            return !float.IsNaN(v.x) && !float.IsInfinity(v.x) && !float.IsNaN(v.y) && !float.IsInfinity(v.y)
                && !float.IsNaN(v.z) && !float.IsInfinity(v.z);
        }
    }
}
