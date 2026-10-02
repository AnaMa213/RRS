using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Frame
{
    public readonly struct TrafficActorInput
    {
        public readonly RoadId TrafficId;
        public readonly VehicleFootprintPose Pose;
        /// <summary>Vitesse signee le long de l'avant de l'empreinte (negative en marche arriere). Ce n'est pas la vitesse de progression sur la route.</summary>
        public readonly float TangentialSpeedMetersPerSecond;
        public readonly RoadId PreviousElementId;
        /// <summary>Elements de la route courante (5.31), bonus de classement de la localisation (AD-45) ; nul sans route.</summary>
        public readonly IReadOnlyList<RoadId> RouteElementIds;
        /// <summary>Ecarts nominaux connus de la route (contrat §8), orientation attendue des candidats ; nul : pose tangente.</summary>
        public readonly IReadOnlyList<RoadKinematicAnchor> Kinematics;
        /// <summary>Horizon d'intention publie a une frame anterieure (5.32) ; nul : aucun.</summary>
        public readonly PublishedIntentHorizon PublishedHorizon;

        public TrafficActorInput(RoadId trafficId, VehicleFootprintPose pose, float tangentialSpeedMetersPerSecond,
            RoadId previousElementId, IReadOnlyList<RoadId> routeElementIds = null,
            IReadOnlyList<RoadKinematicAnchor> kinematics = null, PublishedIntentHorizon publishedHorizon = null)
        {
            TrafficId = trafficId;
            Pose = pose;
            TangentialSpeedMetersPerSecond = tangentialSpeedMetersPerSecond;
            PreviousElementId = previousElementId;
            RouteElementIds = routeElementIds;
            Kinematics = kinematics;
            PublishedHorizon = publishedHorizon;
        }
    }

    public readonly struct TrafficActor
    {
        public readonly RoadId TrafficId;
        public readonly VehicleFootprintPose Pose;
        public readonly float TangentialSpeedMetersPerSecond;
        public readonly RoadId PreviousElementId;
        public readonly RoadLocation Location;
        /// <summary>Quatre extents strictement positifs (5.32). Faux : aucune emprise, aucune distance physique.</summary>
        public readonly bool FootprintDeclared;
        public readonly OccupancyExclusion OccupancyExclusion;
        public readonly PublishedIntentHorizon PublishedHorizon;

        internal TrafficActor(TrafficActorInput input, RoadLocation location, bool footprintDeclared,
            OccupancyExclusion exclusion)
        {
            TrafficId = input.TrafficId;
            Pose = input.Pose;
            TangentialSpeedMetersPerSecond = input.TangentialSpeedMetersPerSecond;
            PreviousElementId = input.PreviousElementId;
            Location = location;
            FootprintDeclared = footprintDeclared;
            OccupancyExclusion = exclusion;
            PublishedHorizon = input.PublishedHorizon;
        }
    }

    /// <summary>
    /// One host snapshot, shared by every consumer of the decision cycle. Actor order, localization and every
    /// index (occupancy, spatial, signals, closures) are fixed at construction; nothing public is writable.
    /// </summary>
    public sealed class TrafficFrame
    {
        /// <summary>Pas maximal de subdivision des bords de l'empreinte pour l'occupation conservatrice.</summary>
        public const float OccupancySampleStepMeters = 0.1f;

        private static readonly ElementOccupant[] NoOccupants = new ElementOccupant[0];

        private readonly Dictionary<RoadId, ElementOccupant[]> _occupantsByElement = new Dictionary<RoadId, ElementOccupant[]>();
        private readonly Dictionary<RoadId, ElementOccupant> _occupancyByActor = new Dictionary<RoadId, ElementOccupant>();
        private readonly Dictionary<RoadId, SignalState> _signalByMovement = new Dictionary<RoadId, SignalState>();
        private readonly SpatialEntry[] _spatial;
        private readonly float _maxSpatialWidthX;

        public ulong FrameId { get; }
        public RoadId ModelId { get { return Model.ModelId; } }
        public RoadModelVersion Version { get { return Model.Version; } }
        public CompiledRoadModel Model { get; }
        public IReadOnlyList<TrafficActor> Actors { get; }
        public IReadOnlyList<TrafficHazardInput> Hazards { get; }
        public IReadOnlyList<SignalPhaseInput> SignalPhases { get; }
        public IReadOnlyList<ElementClosureInput> Closures { get; }

        public TrafficFrame(ulong frameId, CompiledRoadModel model, IReadOnlyList<TrafficActorInput> inputs,
            IReadOnlyList<TrafficHazardInput> hazards = null, IReadOnlyList<SignalPhaseInput> signalPhases = null,
            IReadOnlyList<ElementClosureInput> closures = null)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (inputs == null) throw new ArgumentNullException("inputs");
            FrameId = frameId;
            Model = model;

            var ordered = new List<TrafficActorInput>(inputs.Count);
            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                if (input.TrafficId.IsEmpty) throw new ArgumentException("EmptyTrafficId", "inputs");
                if (float.IsNaN(input.TangentialSpeedMetersPerSecond) || float.IsInfinity(input.TangentialSpeedMetersPerSecond))
                    throw new ArgumentException("InvalidSpeed", "inputs");
                if (!FootprintFinite(input.Pose.Footprint)) throw new ArgumentException("InvalidFootprint", "inputs");
                if (input.PublishedHorizon != null) ValidateHorizon(model, frameId, input.PublishedHorizon);
                ordered.Add(input);
            }
            ordered.Sort((a, b) => a.TrafficId.CompareTo(b.TrafficId));

            var actors = new TrafficActor[ordered.Count];
            var byElement = new Dictionary<RoadId, List<ElementOccupant>>();
            var spatial = new List<SpatialEntry>();
            for (int i = 0; i < ordered.Count; i++)
            {
                var input = ordered[i];
                if (i > 0 && input.TrafficId == ordered[i - 1].TrafficId)
                    throw new ArgumentException("DuplicateTrafficId", "inputs");
                var location = RoadLocalizer.Localize(model, input.Pose, input.PreviousElementId, input.RouteElementIds,
                    input.Kinematics);
                bool declared = FootprintDeclared(input.Pose.Footprint);
                var exclusion = OccupancyExclusion.None;
                if (!location.Localized) exclusion = OccupancyExclusion.NotLocalized;
                else if (!declared) exclusion = OccupancyExclusion.UndeclaredFootprint;
                else
                {
                    ElementOccupant occupant;
                    if (TryOccupy(model, input, location, out occupant))
                    {
                        _occupancyByActor.Add(input.TrafficId, occupant);
                        List<ElementOccupant> bucket;
                        if (!byElement.TryGetValue(occupant.ElementId, out bucket))
                            byElement.Add(occupant.ElementId, bucket = new List<ElementOccupant>());
                        bucket.Add(occupant);
                    }
                    else exclusion = OccupancyExclusion.OccupancyNotBounded;
                }
                // Un acteur porte sa confiance de localisation (0 non localise) et sa vitesse le long de son avant.
                if (declared)
                    spatial.Add(new SpatialEntry(input.TrafficId, true, default(TrafficHazardKind),
                        FootprintBounds(input.Pose), input.Pose.Forward.normalized * input.TangentialSpeedMetersPerSecond,
                        location.Confidence));
                actors[i] = new TrafficActor(input, location, declared, exclusion);
            }
            Actors = Array.AsReadOnly(actors);
            foreach (var pair in byElement)
            {
                var values = pair.Value.ToArray();
                Array.Sort(values, CompareOccupants);
                _occupantsByElement.Add(pair.Key, values);
            }

            Hazards = BuildHazards(hazards, spatial);
            spatial.Sort(CompareSpatial);
            _spatial = spatial.ToArray();
            for (int i = 0; i < _spatial.Length; i++)
                _maxSpatialWidthX = Mathf.Max(_maxSpatialWidthX, _spatial[i].Bounds.size.x);

            SignalPhases = BuildSignals(model, signalPhases);
            Closures = BuildClosures(model, closures);
        }

        public bool TryGetActor(RoadId id, out TrafficActor actor)
        {
            int low = 0, high = Actors.Count - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                int order = Actors[mid].TrafficId.CompareTo(id);
                if (order == 0) { actor = Actors[mid]; return true; }
                if (order < 0) low = mid + 1; else high = mid - 1;
            }
            actor = default(TrafficActor);
            return false;
        }

        /// <summary>Occupants d'un element, tries par (sMin, TrafficId) ; vide si aucun.</summary>
        public IReadOnlyList<ElementOccupant> GetOccupants(RoadId elementId)
        {
            ElementOccupant[] values;
            return _occupantsByElement.TryGetValue(elementId, out values) ? Array.AsReadOnly(values) : Array.AsReadOnly(NoOccupants);
        }

        public bool TryGetOccupancy(RoadId trafficId, out ElementOccupant occupant)
        {
            return _occupancyByActor.TryGetValue(trafficId, out occupant);
        }

        public bool TryGetSignalState(RoadId movementId, out SignalState state)
        {
            return _signalByMovement.TryGetValue(movementId, out state);
        }

        public bool IsClosed(RoadId elementId)
        {
            for (int i = 0; i < Closures.Count; i++)
                if (Closures[i].ElementId == elementId) return true;
            return false;
        }

        /// <summary>
        /// Entrees dont la boite rejoint <paramref name="query"/>, dans l'ordre de l'index (x minimal, puis id).
        /// N'alloue pas ; le total compte tous les candidats, meme ceux precedant l'offset de pagination.
        /// </summary>
        // ponytail: balayage trie sur x, O(n) sur une bande x dense ; grille si la 5.46 mesure un cout.
        public void QuerySpatial(Bounds query, SpatialQueryBuffer buffer, int offset = 0)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0) throw new ArgumentException("InvalidSpatialOffset", "offset");
            buffer.Clear();
            float from = query.min.x - _maxSpatialWidthX;
            int low = 0, high = _spatial.Length;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (_spatial[mid].Bounds.min.x < from) low = mid + 1; else high = mid;
            }
            for (int i = low; i < _spatial.Length && _spatial[i].Bounds.min.x <= query.max.x; i++)
                if (_spatial[i].Bounds.Intersects(query)) buffer.Add(_spatial[i], offset);
        }

        /// <summary>Courbe et longueur d'un corridor ou d'un mouvement ; faux si l'element est inconnu.</summary>
        public static bool TryGetElement(CompiledRoadModel model, RoadId id, out RoadElementKind kind, out RoadCurve curve,
            out IReadOnlyList<RoadCurveSample> samples)
        {
            EffectiveLaneCorridor corridor;
            if (model.TryGetCorridor(id, out corridor))
            { kind = RoadElementKind.LaneCorridor; curve = corridor.Curve; samples = corridor.Samples; return true; }
            CompiledJunctionMovement movement;
            if (model.TryGetMovement(id, out movement))
            { kind = RoadElementKind.JunctionMovement; curve = movement.Curve; samples = movement.Samples; return true; }
            kind = RoadElementKind.None; curve = null; samples = null;
            return false;
        }

        /// <summary>
        /// Occupation conservatrice : bords subdivises a h ≤ <see cref="OccupancySampleStepMeters"/>, projetes sur la
        /// courbe de l'element (depassement aux bornes compris), plus r = (h/2) / (1 − κ_max·d_max). Faux si
        /// κ_max·d_max ≥ 1 : la projection n'y est plus lipschitzienne, aucun intervalle n'est publie.
        /// </summary>
        private static bool TryOccupy(CompiledRoadModel model, TrafficActorInput input, RoadLocation location,
            out ElementOccupant occupant)
        {
            occupant = default(ElementOccupant);
            RoadElementKind kind;
            RoadCurve curve;
            IReadOnlyList<RoadCurveSample> samples;
            if (!TryGetElement(model, location.ElementId, out kind, out curve, out samples)) return false;
            float kappaMax = 0f;
            for (int i = 0; i < samples.Count; i++) kappaMax = Mathf.Max(kappaMax, Mathf.Abs(samples[i].CurvaturePerMeter));

            var corners = Corners(input.Pose);
            float sMin = float.PositiveInfinity, sMax = float.NegativeInfinity, dMax = 0f, step = 0f;
            for (int edge = 0; edge < 4; edge++)
            {
                Vector3 a = corners[edge], b = corners[(edge + 1) % 4];
                float length = Vector3.Distance(a, b);
                int count = Mathf.Max(1, Mathf.CeilToInt(length / OccupancySampleStepMeters));
                step = Mathf.Max(step, length / count);
                for (int k = 0; k < count; k++)
                {
                    var projection = curve.Project(Vector3.Lerp(a, b, (float)k / count));
                    float s = projection.SMeters;
                    if (projection.LongitudinalOverrunMeters > 0f)
                        s = s <= curve.StartS + 1e-3f ? s - projection.LongitudinalOverrunMeters : s + projection.LongitudinalOverrunMeters;
                    sMin = Mathf.Min(sMin, s);
                    sMax = Mathf.Max(sMax, s);
                    dMax = Mathf.Max(dMax, projection.DistanceMeters);
                }
            }
            dMax += 0.5f * step;
            float bound = kappaMax * dMax;
            if (!(bound < 1f)) return false;
            float remainder = 0.5f * step / (1f - bound);
            occupant = new ElementOccupant(input.TrafficId, kind, location.ElementId, location.SMeters,
                sMin - remainder, sMax + remainder, remainder, input.TangentialSpeedMetersPerSecond, location.Confidence);
            return true;
        }

        /// <summary>Coins avant-droit, avant-gauche, arriere-gauche, arriere-droit (meme repere chassis que la localisation).</summary>
        public static Vector3[] Corners(VehicleFootprintPose pose)
        {
            var forward = pose.Forward.normalized;
            var up = (pose.Up - forward * Vector3.Dot(pose.Up, forward)).normalized;
            var right = Vector3.Cross(up, forward).normalized;
            var origin = pose.Footprint.ReferenceOriginLocal;
            var reference = pose.Position + right * origin.x + up * origin.y + forward * origin.z;
            var f = pose.Footprint;
            return new[]
            {
                reference + forward * f.FrontMeters + right * f.RightMeters,
                reference + forward * f.FrontMeters - right * f.LeftMeters,
                reference - forward * f.RearMeters - right * f.LeftMeters,
                reference - forward * f.RearMeters + right * f.RightMeters
            };
        }

        private static Bounds FootprintBounds(VehicleFootprintPose pose)
        {
            var corners = Corners(pose);
            var bounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < corners.Length; i++) bounds.Encapsulate(corners[i]);
            return bounds;
        }

        private static bool FootprintFinite(VehicleFootprint f)
        {
            return f.FrontMeters >= 0f && f.RearMeters >= 0f && f.LeftMeters >= 0f && f.RightMeters >= 0f
                && !float.IsInfinity(f.FrontMeters) && !float.IsInfinity(f.RearMeters)
                && !float.IsInfinity(f.LeftMeters) && !float.IsInfinity(f.RightMeters);
        }

        private static bool FootprintDeclared(VehicleFootprint f)
        {
            return f.FrontMeters > 0f && f.RearMeters > 0f && f.LeftMeters > 0f && f.RightMeters > 0f;
        }

        private static void ValidateHorizon(CompiledRoadModel model, ulong frameId, PublishedIntentHorizon horizon)
        {
            if (horizon.SourceFrameId >= frameId) throw new ArgumentException("FutureIntentHorizon", "inputs");
            for (int i = 0; i < horizon.Intervals.Count; i++)
            {
                var interval = horizon.Intervals[i];
                if (float.IsNaN(interval.StartSMeters) || float.IsInfinity(interval.StartSMeters)
                    || float.IsNaN(interval.EndSMeters) || float.IsInfinity(interval.EndSMeters)
                    || interval.StartSMeters > interval.EndSMeters)
                    throw new ArgumentException("InvalidIntentInterval", "inputs");
                RoadElementKind kind;
                RoadCurve curve;
                IReadOnlyList<RoadCurveSample> samples;
                if (!TryGetElement(model, interval.ElementId, out kind, out curve, out samples) || kind != interval.Kind)
                    throw new ArgumentException("UnknownElement", "inputs");
            }
        }

        private IReadOnlyList<TrafficHazardInput> BuildHazards(IReadOnlyList<TrafficHazardInput> hazards, List<SpatialEntry> spatial)
        {
            var copy = new TrafficHazardInput[hazards == null ? 0 : hazards.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                var hazard = hazards[i];
                if (hazard.Id.IsEmpty) throw new ArgumentException("EmptyHazardId", "hazards");
                TrafficActor actor;
                if (TryGetActor(hazard.Id, out actor)) throw new ArgumentException("DuplicateIdentity", "hazards");
                var e = hazard.Bounds.Extents;
                if (!Enum.IsDefined(typeof(TrafficHazardKind), hazard.Kind) || !Finite(hazard.Bounds.Center) || !Finite(e)
                    || !Finite(hazard.Velocity) || e.x < 0f || e.y < 0f || e.z < 0f)
                    throw new ArgumentException("InvalidHazard", "hazards");
                if (!(hazard.Confidence >= 0f && hazard.Confidence <= 1f)) throw new ArgumentException("InvalidConfidence", "hazards");
                copy[i] = hazard;
            }
            Array.Sort(copy, (a, b) => a.Id.CompareTo(b.Id));
            for (int i = 0; i < copy.Length; i++)
            {
                if (i > 0 && copy[i].Id == copy[i - 1].Id) throw new ArgumentException("DuplicateHazardId", "hazards");
                spatial.Add(new SpatialEntry(copy[i].Id, false, copy[i].Kind,
                    new Bounds(copy[i].Bounds.Center, 2f * copy[i].Bounds.Extents), copy[i].Velocity, copy[i].Confidence));
            }
            return Array.AsReadOnly(copy);
        }

        private IReadOnlyList<SignalPhaseInput> BuildSignals(CompiledRoadModel model, IReadOnlyList<SignalPhaseInput> phases)
        {
            var copy = new SignalPhaseInput[phases == null ? 0 : phases.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = phases[i];
            Array.Sort(copy, (a, b) => a.PlanId.CompareTo(b.PlanId));
            for (int i = 0; i < copy.Length; i++)
            {
                if (i > 0 && copy[i].PlanId == copy[i - 1].PlanId) throw new ArgumentException("DuplicateSignalPlan", "signalPhases");
                int plan = -1;
                for (int p = 0; p < model.SignalPlans.Count; p++)
                    if (model.SignalPlans[p].Id == copy[i].PlanId) plan = p;
                if (plan < 0) throw new ArgumentException("UnknownSignalPlan", "signalPhases");
                var compiled = model.SignalPlans[plan];
                int phase = -1;
                for (int p = 0; p < compiled.Phases.Count; p++)
                    if (compiled.Phases[p].PhaseId == copy[i].PhaseId) phase = p;
                if (phase < 0) throw new ArgumentException("UnknownSignalPhase", "signalPhases");
                var states = compiled.Phases[phase].GroupStates;
                for (int g = 0; g < compiled.Groups.Count; g++)
                    for (int s = 0; s < states.Count; s++)
                        if (states[s].GroupId == compiled.Groups[g].GroupId)
                            for (int m = 0; m < compiled.Groups[g].MemberMovementIds.Count; m++)
                                _signalByMovement[compiled.Groups[g].MemberMovementIds[m]] = states[s].State;
            }
            return Array.AsReadOnly(copy);
        }

        private static IReadOnlyList<ElementClosureInput> BuildClosures(CompiledRoadModel model, IReadOnlyList<ElementClosureInput> closures)
        {
            var copy = new ElementClosureInput[closures == null ? 0 : closures.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                var closure = closures[i];
                RoadElementKind kind;
                RoadCurve curve;
                IReadOnlyList<RoadCurveSample> samples;
                if (!TryGetElement(model, closure.ElementId, out kind, out curve, out samples))
                    throw new ArgumentException("UnknownElement", "closures");
                if (string.IsNullOrEmpty(closure.ReasonCode)) throw new ArgumentException("EmptyClosureReason", "closures");
                copy[i] = closure;
            }
            Array.Sort(copy, (a, b) => a.ElementId.CompareTo(b.ElementId));
            for (int i = 1; i < copy.Length; i++)
                if (copy[i].ElementId == copy[i - 1].ElementId) throw new ArgumentException("DuplicateClosure", "closures");
            return Array.AsReadOnly(copy);
        }

        private static int CompareOccupants(ElementOccupant a, ElementOccupant b)
        {
            int order = a.SMinMeters.CompareTo(b.SMinMeters);
            return order != 0 ? order : a.TrafficId.CompareTo(b.TrafficId);
        }

        private static int CompareSpatial(SpatialEntry a, SpatialEntry b)
        {
            int order = a.Bounds.min.x.CompareTo(b.Bounds.min.x);
            return order != 0 ? order : a.Id.CompareTo(b.Id);
        }

        private static bool Finite(Vector3 v)
        {
            return !float.IsNaN(v.x) && !float.IsInfinity(v.x) && !float.IsNaN(v.y) && !float.IsInfinity(v.y)
                && !float.IsNaN(v.z) && !float.IsInfinity(v.z);
        }
    }
}
