using System;
using System.Collections.Generic;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Version de contenu deterministe d'un modele compile : version de schema du compilateur plus
    /// l'empreinte 128 bits de la charge canonique. Le constructeur est <c>internal</c> et
    /// <see cref="RoadModelCompiler"/> est son seul appelant : aucun consommateur ne recalcule une
    /// version, il compare la valeur emise.
    /// </summary>
    public readonly struct RoadModelVersion : IEquatable<RoadModelVersion>
    {
        private readonly int _schemaVersion;
        private readonly ulong _high;
        private readonly ulong _low;

        internal RoadModelVersion(int schemaVersion, ulong high, ulong low)
        {
            _schemaVersion = schemaVersion;
            _high = high;
            _low = low;
        }

        public int SchemaVersion
        {
            get { return _schemaVersion; }
        }

        public ulong High
        {
            get { return _high; }
        }

        public ulong Low
        {
            get { return _low; }
        }

        public bool IsEmpty
        {
            get { return _schemaVersion == 0 && _high == 0UL && _low == 0UL; }
        }

        public bool Equals(RoadModelVersion other)
        {
            return _schemaVersion == other._schemaVersion && _high == other._high && _low == other._low;
        }

        public override bool Equals(object obj)
        {
            return obj is RoadModelVersion other && Equals(other);
        }

        public override int GetHashCode()
        {
            ulong mixed = _high ^ _low ^ (ulong)_schemaVersion;
            return (int)(mixed ^ (mixed >> 32));
        }

        public override string ToString()
        {
            return "v" + _schemaVersion.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":"
                + _high.ToString("x16", System.Globalization.CultureInfo.InvariantCulture)
                + _low.ToString("x16", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool operator ==(RoadModelVersion left, RoadModelVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RoadModelVersion left, RoadModelVersion right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Vue effective d'un corridor apres resolution des defauts de section. Un defaut ne change
    /// jamais la propriete : le corridor reste l'unite geometrique et dirigee.
    /// </summary>
    public struct EffectiveLaneCorridor
    {
        public RoadId CorridorId;
        public RoadId SectionId;
        public float SpeedLimitMetersPerSecond;
        public RoadSurface Surface;
        public VehicleClassMask AllowedVehicleClasses;
        public float LengthMeters;

        /// <summary>Lecture seule : dans le modele compile, une copie non convertible en tableau.</summary>
        public IReadOnlyList<RoadCurveSample> Samples;

        /// <summary>Position laterale authoree reportee depuis le corridor (AD-48).</summary>
        public int LateralOrder;

        /// <summary>Datum de repere transversal de la section, reporte depuis le corridor (AD-48).</summary>
        public bool IsCrossSectionDatum;
    }

    // Vues compilees des enregistrements qui portent une collection imbriquee. Les records
    // sources gardent des tableaux (serialisation Unity, 5.27) ; le modele compile n'expose que
    // ces vues, dont chaque collection est une copie en lecture seule.

    /// <summary>Vue compilee immuable d'un <see cref="JunctionMovement"/>.</summary>
    public readonly struct CompiledJunctionMovement
    {
        public readonly RoadId Id;

        /// <summary>Diagnostic seul.</summary>
        public readonly string Label;

        public readonly RoadId JunctionId;
        public readonly RoadId FromCorridorId;
        public readonly RoadId ToCorridorId;
        public readonly IReadOnlyList<RoadCurveSample> Samples;
        public readonly float LengthMeters;
        public readonly float RoutePreferenceWeight;

        internal CompiledJunctionMovement(JunctionMovement source)
        {
            Id = source.Id;
            Label = source.Label;
            JunctionId = source.JunctionId;
            FromCorridorId = source.FromCorridorId;
            ToCorridorId = source.ToCorridorId;
            Samples = CompiledRoadModel.ReadOnlyCopy(source.Samples);
            LengthMeters = source.LengthMeters;
            RoutePreferenceWeight = source.RoutePreferenceWeight;
        }
    }

    /// <summary>Vue compilee immuable d'un <see cref="JunctionControl"/> (AD-46).</summary>
    public readonly struct CompiledJunctionControl
    {
        public readonly RoadId Id;
        public readonly RoadId JunctionId;
        public readonly JunctionControlKind Kind;
        public readonly IReadOnlyList<RoadId> ControlledMovementIds;
        public readonly bool HasStopLine;
        public readonly RoadLineSegment StopLine;

        internal CompiledJunctionControl(JunctionControl source)
        {
            Id = source.Id;
            JunctionId = source.JunctionId;
            Kind = source.Kind;
            ControlledMovementIds = CompiledRoadModel.ReadOnlyCopy(source.ControlledMovementIds);
            HasStopLine = source.HasStopLine;
            StopLine = source.StopLine;
        }
    }

    /// <summary>Vue compilee immuable d'une <see cref="ConflictZone"/>.</summary>
    public readonly struct CompiledConflictZone
    {
        public readonly RoadId Id;
        public readonly RoadId JunctionId;
        public readonly RoadBoundsBox Volume;
        public readonly IReadOnlyList<RoadId> MemberMovementIds;

        internal CompiledConflictZone(ConflictZone source)
        {
            Id = source.Id;
            JunctionId = source.JunctionId;
            Volume = source.Volume;
            MemberMovementIds = CompiledRoadModel.ReadOnlyCopy(source.MemberMovementIds);
        }
    }

    /// <summary>Vue compilee immuable d'un <see cref="SignalGroup"/>.</summary>
    public readonly struct CompiledSignalGroup
    {
        public readonly RoadId GroupId;
        public readonly IReadOnlyList<RoadId> MemberMovementIds;

        internal CompiledSignalGroup(SignalGroup source)
        {
            GroupId = source.GroupId;
            MemberMovementIds = CompiledRoadModel.ReadOnlyCopy(source.MemberMovementIds);
        }
    }

    /// <summary>Vue compilee immuable d'une <see cref="SignalPhase"/>.</summary>
    public readonly struct CompiledSignalPhase
    {
        public readonly RoadId PhaseId;
        public readonly float DurationSeconds;
        public readonly IReadOnlyList<SignalGroupState> GroupStates;

        internal CompiledSignalPhase(SignalPhase source)
        {
            PhaseId = source.PhaseId;
            DurationSeconds = source.DurationSeconds;
            GroupStates = CompiledRoadModel.ReadOnlyCopy(source.GroupStates);
        }
    }

    /// <summary>Vue compilee immuable d'un <see cref="SignalPlan"/> ; l'ordre des phases est conserve.</summary>
    public readonly struct CompiledSignalPlan
    {
        public readonly RoadId Id;
        public readonly RoadId JunctionId;
        public readonly IReadOnlyList<CompiledSignalGroup> Groups;
        public readonly IReadOnlyList<CompiledSignalPhase> Phases;

        internal CompiledSignalPlan(SignalPlan source)
        {
            Id = source.Id;
            JunctionId = source.JunctionId;

            var groups = source.Groups ?? new SignalGroup[0];
            var compiledGroups = new CompiledSignalGroup[groups.Length];
            for (int g = 0; g < groups.Length; g++)
            {
                compiledGroups[g] = new CompiledSignalGroup(groups[g]);
            }

            var phases = source.Phases ?? new SignalPhase[0];
            var compiledPhases = new CompiledSignalPhase[phases.Length];
            for (int p = 0; p < phases.Length; p++)
            {
                compiledPhases[p] = new CompiledSignalPhase(phases[p]);
            }

            Groups = Array.AsReadOnly(compiledGroups);
            Phases = Array.AsReadOnly(compiledPhases);
        }
    }

    /// <summary>
    /// Vue compilee immuable du Road World Model : copies defensives des enregistrements, defauts
    /// de section resolus, collections inverses et index derives par le compilateur, version portee
    /// en lecture seule.
    /// </summary>
    /// <remarks>
    /// Tout est copie a la construction, donc muter la source apres coup n'affecte pas le modele
    /// compile. Aucune collection imbriquee n'est exposee en tableau : chacune est une copie en
    /// lecture seule, non convertible en tableau, donc un consommateur ne peut pas desynchroniser
    /// le modele de sa version ou de ses index derives.
    /// </remarks>
    public sealed class CompiledRoadModel
    {
        private static readonly RoadId[] EmptyIds = new RoadId[0];

        private readonly RoadSection[] _sections;
        private readonly EffectiveLaneCorridor[] _corridors;
        private readonly LaneConnection[] _connections;
        private readonly LaneAdjacency[] _adjacencies;
        private readonly Junction[] _junctions;
        private readonly CompiledJunctionMovement[] _movements;
        private readonly CompiledJunctionControl[] _controls;
        private readonly CompiledConflictZone[] _conflictZones;
        private readonly CompiledSignalPlan[] _signalPlans;
        private readonly Portal[] _portals;

        private readonly Dictionary<RoadId, int> _corridorIndex = new Dictionary<RoadId, int>();
        private readonly Dictionary<RoadId, int> _junctionIndex = new Dictionary<RoadId, int>();
        private readonly Dictionary<RoadId, int> _movementIndex = new Dictionary<RoadId, int>();
        private readonly Dictionary<RoadId, int> _controlIndex = new Dictionary<RoadId, int>();

        private readonly Dictionary<RoadId, RoadId[]> _corridorsBySection;
        private readonly Dictionary<RoadId, RoadId[]> _movementsByJunction;
        private readonly Dictionary<RoadId, RoadId[]> _controlsByJunction;
        private readonly Dictionary<RoadId, RoadId[]> _conflictZonesByJunction;
        private readonly Dictionary<RoadId, RoadId[]> _signalPlansByJunction;
        private readonly Dictionary<RoadId, RoadId[]> _successorCorridors;
        private readonly Dictionary<RoadId, RoadId[]> _predecessorCorridors;
        private readonly Dictionary<RoadId, RoadId[]> _portalsByCorridor;
        private readonly Dictionary<RoadId, RoadId> _controlByMovement = new Dictionary<RoadId, RoadId>();

        internal CompiledRoadModel(
            RoadId modelId,
            RoadModelVersion version,
            RoadModelValidationProfile profile,
            RoadSection[] sections,
            EffectiveLaneCorridor[] corridors,
            LaneConnection[] connections,
            LaneAdjacency[] adjacencies,
            Junction[] junctions,
            JunctionMovement[] movements,
            JunctionControl[] controls,
            ConflictZone[] conflictZones,
            SignalPlan[] signalPlans,
            Portal[] portals)
        {
            ModelId = modelId;
            Version = version;
            ValidationProfile = profile;

            _sections = (RoadSection[])sections.Clone();
            _connections = (LaneConnection[])connections.Clone();
            _adjacencies = (LaneAdjacency[])adjacencies.Clone();
            _junctions = (Junction[])junctions.Clone();
            _portals = (Portal[])portals.Clone();

            _corridors = new EffectiveLaneCorridor[corridors.Length];
            for (int i = 0; i < corridors.Length; i++)
            {
                _corridors[i] = corridors[i];
                _corridors[i].Samples = ReadOnlyCopy(corridors[i].Samples);
                _corridorIndex[_corridors[i].CorridorId] = i;
            }

            _movements = new CompiledJunctionMovement[movements.Length];
            for (int i = 0; i < movements.Length; i++)
            {
                _movements[i] = new CompiledJunctionMovement(movements[i]);
                _movementIndex[_movements[i].Id] = i;
            }

            _controls = new CompiledJunctionControl[controls.Length];
            for (int i = 0; i < controls.Length; i++)
            {
                _controls[i] = new CompiledJunctionControl(controls[i]);
                _controlIndex[_controls[i].Id] = i;
                for (int m = 0; m < _controls[i].ControlledMovementIds.Count; m++)
                {
                    _controlByMovement[_controls[i].ControlledMovementIds[m]] = _controls[i].Id;
                }
            }

            _conflictZones = new CompiledConflictZone[conflictZones.Length];
            for (int i = 0; i < conflictZones.Length; i++)
            {
                _conflictZones[i] = new CompiledConflictZone(conflictZones[i]);
            }

            _signalPlans = new CompiledSignalPlan[signalPlans.Length];
            for (int i = 0; i < signalPlans.Length; i++)
            {
                _signalPlans[i] = new CompiledSignalPlan(signalPlans[i]);
            }

            for (int i = 0; i < _junctions.Length; i++)
            {
                _junctionIndex[_junctions[i].Id] = i;
            }

            // ------------------------------------------------ collections inverses derivees
            var corridorsBySection = new Dictionary<RoadId, List<RoadId>>();
            for (int i = 0; i < _corridors.Length; i++)
            {
                Append(corridorsBySection, _corridors[i].SectionId, _corridors[i].CorridorId);
            }

            var movementsByJunction = new Dictionary<RoadId, List<RoadId>>();
            for (int i = 0; i < _movements.Length; i++)
            {
                Append(movementsByJunction, _movements[i].JunctionId, _movements[i].Id);
            }

            var controlsByJunction = new Dictionary<RoadId, List<RoadId>>();
            for (int i = 0; i < _controls.Length; i++)
            {
                Append(controlsByJunction, _controls[i].JunctionId, _controls[i].Id);
            }

            var conflictZonesByJunction = new Dictionary<RoadId, List<RoadId>>();
            for (int i = 0; i < _conflictZones.Length; i++)
            {
                Append(conflictZonesByJunction, _conflictZones[i].JunctionId, _conflictZones[i].Id);
            }

            var signalPlansByJunction = new Dictionary<RoadId, List<RoadId>>();
            for (int i = 0; i < _signalPlans.Length; i++)
            {
                Append(signalPlansByJunction, _signalPlans[i].JunctionId, _signalPlans[i].Id);
            }

            var successors = new Dictionary<RoadId, List<RoadId>>();
            var predecessors = new Dictionary<RoadId, List<RoadId>>();
            for (int i = 0; i < _connections.Length; i++)
            {
                Append(successors, _connections[i].FromCorridorId, _connections[i].ToCorridorId);
                Append(predecessors, _connections[i].ToCorridorId, _connections[i].FromCorridorId);
            }

            var portalsByCorridor = new Dictionary<RoadId, List<RoadId>>();
            for (int i = 0; i < _portals.Length; i++)
            {
                Append(portalsByCorridor, _portals[i].CorridorId, _portals[i].Id);
            }

            _corridorsBySection = FreezeCorridorsBySection(corridorsBySection);
            _movementsByJunction = Freeze(movementsByJunction);
            _controlsByJunction = Freeze(controlsByJunction);
            _conflictZonesByJunction = Freeze(conflictZonesByJunction);
            _signalPlansByJunction = Freeze(signalPlansByJunction);
            _successorCorridors = Freeze(successors);
            _predecessorCorridors = Freeze(predecessors);
            _portalsByCorridor = Freeze(portalsByCorridor);
        }

        public RoadId ModelId { get; private set; }

        /// <summary>Emise par le seul compilateur. Se compare, ne se recalcule pas.</summary>
        public RoadModelVersion Version { get; private set; }

        public RoadModelValidationProfile ValidationProfile { get; private set; }

        public IReadOnlyList<RoadSection> Sections
        {
            get { return _sections; }
        }

        public IReadOnlyList<EffectiveLaneCorridor> Corridors
        {
            get { return _corridors; }
        }

        public IReadOnlyList<LaneConnection> Connections
        {
            get { return _connections; }
        }

        public IReadOnlyList<LaneAdjacency> Adjacencies
        {
            get { return _adjacencies; }
        }

        public IReadOnlyList<Junction> Junctions
        {
            get { return _junctions; }
        }

        public IReadOnlyList<CompiledJunctionMovement> Movements
        {
            get { return _movements; }
        }

        public IReadOnlyList<CompiledJunctionControl> Controls
        {
            get { return _controls; }
        }

        public IReadOnlyList<CompiledConflictZone> ConflictZones
        {
            get { return _conflictZones; }
        }

        public IReadOnlyList<CompiledSignalPlan> SignalPlans
        {
            get { return _signalPlans; }
        }

        public IReadOnlyList<Portal> Portals
        {
            get { return _portals; }
        }

        // ------------------------------------------------------------------ index chauds

        public bool TryGetCorridor(RoadId corridorId, out EffectiveLaneCorridor corridor)
        {
            int index;
            if (_corridorIndex.TryGetValue(corridorId, out index))
            {
                corridor = _corridors[index];
                return true;
            }

            corridor = default(EffectiveLaneCorridor);
            return false;
        }

        public bool TryGetJunction(RoadId junctionId, out Junction junction)
        {
            int index;
            if (_junctionIndex.TryGetValue(junctionId, out index))
            {
                junction = _junctions[index];
                return true;
            }

            junction = default(Junction);
            return false;
        }

        public bool TryGetMovement(RoadId movementId, out CompiledJunctionMovement movement)
        {
            int index;
            if (_movementIndex.TryGetValue(movementId, out index))
            {
                movement = _movements[index];
                return true;
            }

            movement = default(CompiledJunctionMovement);
            return false;
        }

        public bool TryGetControl(RoadId controlId, out CompiledJunctionControl control)
        {
            int index;
            if (_controlIndex.TryGetValue(controlId, out index))
            {
                control = _controls[index];
                return true;
            }

            control = default(CompiledJunctionControl);
            return false;
        }

        /// <summary>
        /// Liaison de controle unique et faisant autorite d'un mouvement (AD-46). Derivee par le
        /// compilateur depuis l'appartenance possedee par <see cref="JunctionControl"/> ; un
        /// mouvement ne stocke jamais son controle.
        /// </summary>
        public bool TryGetControlForMovement(RoadId movementId, out CompiledJunctionControl control)
        {
            RoadId controlId;
            if (_controlByMovement.TryGetValue(movementId, out controlId))
            {
                return TryGetControl(controlId, out control);
            }

            control = default(CompiledJunctionControl);
            return false;
        }

        // ------------------------------------------------------------------ collections inverses

        public IReadOnlyList<RoadId> GetCorridorsInSection(RoadId sectionId)
        {
            return Lookup(_corridorsBySection, sectionId);
        }

        public IReadOnlyList<RoadId> GetMovementsInJunction(RoadId junctionId)
        {
            return Lookup(_movementsByJunction, junctionId);
        }

        public IReadOnlyList<RoadId> GetControlsInJunction(RoadId junctionId)
        {
            return Lookup(_controlsByJunction, junctionId);
        }

        public IReadOnlyList<RoadId> GetConflictZonesInJunction(RoadId junctionId)
        {
            return Lookup(_conflictZonesByJunction, junctionId);
        }

        public IReadOnlyList<RoadId> GetSignalPlansInJunction(RoadId junctionId)
        {
            return Lookup(_signalPlansByJunction, junctionId);
        }

        public IReadOnlyList<RoadId> GetSuccessorCorridors(RoadId corridorId)
        {
            return Lookup(_successorCorridors, corridorId);
        }

        public IReadOnlyList<RoadId> GetPredecessorCorridors(RoadId corridorId)
        {
            return Lookup(_predecessorCorridors, corridorId);
        }

        public IReadOnlyList<RoadId> GetPortalsOnCorridor(RoadId corridorId)
        {
            return Lookup(_portalsByCorridor, corridorId);
        }

        // ------------------------------------------------------------------ helpers

        private static IReadOnlyList<RoadId> Lookup(Dictionary<RoadId, RoadId[]> index, RoadId key)
        {
            RoadId[] values;
            return index.TryGetValue(key, out values) ? values : EmptyIds;
        }

        private static void Append(Dictionary<RoadId, List<RoadId>> index, RoadId key, RoadId value)
        {
            List<RoadId> bucket;
            if (!index.TryGetValue(key, out bucket))
            {
                bucket = new List<RoadId>();
                index.Add(key, bucket);
            }

            bucket.Add(value);
        }

        /// <summary>
        /// Seule collection inverse porteuse de sens : l'ordre transversal authore (AD-48). Trie par
        /// <c>LateralOrder</c> croissant ; le <see cref="RoadId"/> ne sert que de depart d'egalite
        /// deterministe pour un modele qui a deja echoue a la validation, ou l'unicite n'est plus
        /// garantie. Le <c>RoadId</c> ne porte jamais de semantique de voie.
        /// </summary>
        private Dictionary<RoadId, RoadId[]> FreezeCorridorsBySection(Dictionary<RoadId, List<RoadId>> index)
        {
            var frozen = new Dictionary<RoadId, RoadId[]>(index.Count);
            foreach (var pair in index)
            {
                var values = pair.Value.ToArray();
                Array.Sort(values, CompareByLateralOrder);
                frozen.Add(pair.Key, values);
            }

            return frozen;
        }

        private int CompareByLateralOrder(RoadId a, RoadId b)
        {
            int orderA = LateralOrderOf(a);
            int orderB = LateralOrderOf(b);
            if (orderA != orderB)
            {
                return orderA < orderB ? -1 : 1;
            }

            return a.CompareTo(b);
        }

        private int LateralOrderOf(RoadId corridorId)
        {
            int index;
            return _corridorIndex.TryGetValue(corridorId, out index) ? _corridors[index].LateralOrder : 0;
        }

        private static Dictionary<RoadId, RoadId[]> Freeze(Dictionary<RoadId, List<RoadId>> index)
        {
            var frozen = new Dictionary<RoadId, RoadId[]>(index.Count);
            foreach (var pair in index)
            {
                var values = pair.Value.ToArray();

                // Ordre stable et independant de l'ordre d'insertion : les index sont des caches
                // compiles, jamais une identite authoree.
                Array.Sort(values);
                frozen.Add(pair.Key, values);
            }

            return frozen;
        }

        /// <summary>
        /// Copie en lecture seule : un <see cref="System.Collections.ObjectModel.ReadOnlyCollection{T}"/>
        /// sur un tableau neuf, donc ni muable par indexeur ni convertible en tableau.
        /// </summary>
        internal static IReadOnlyList<T> ReadOnlyCopy<T>(IReadOnlyList<T> values)
        {
            var copy = new T[values == null ? 0 : values.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = values[i];
            }

            return Array.AsReadOnly(copy);
        }
    }
}
