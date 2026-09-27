#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    // =====================================================================================
    // Story 5.27 -- importeur V1 -> Road World Model candidat.
    //
    // Sens unique : lit un V1SourceSet prouve, produit un RoadModelSource candidat, ne touche ni
    // la scene ni V1. N'invente aucune semantique absente de V1 (controle, conflit, priorite,
    // signal, adjacence, ligne) : chacune devient une tache d'authoring. Toute forme source que
    // les regles ci-dessous ne savent pas disposer est un echec dur, jamais une approximation.
    //
    // Decoupage (spec 5.27, Design Notes) :
    //   - module a voies (segment, tunnel) : une section, un corridor par voie ;
    //   - module carrefour : un Junction ; les noeuds interieurs a successeur unique chaines entre
    //     eux forment des corridors d'anneau ; un noeud interieur isole est une graine de decision ;
    //   - chaque arete V1 qui choisit une sortie devient un JunctionMovement, courbe Hermite lisse
    //     entre la pose de fin d'approche et la pose de depart ;
    //   - jointure voie -> carrefour (ou inverse) : le connecteur du carrefour est fusionne a
    //     l'extremite du corridor voisin ; voie -> voie : LaneConnection ; carrefour -> carrefour :
    //     echec dur.
    // =====================================================================================

    public enum SourceItemKind
    {
        Node = 0,
        Edge = 1,
        TurnWeight = 2,
        ConnectorMatch = 3,
        PortalRole = 4
    }

    public enum DispositionKind
    {
        /// <summary>Noeud devenu point de controle d'une courbe de corridor.</summary>
        CorridorVertex = 0,

        /// <summary>Connecteur (noeud ou jointure) fusionne a l'extremite d'un corridor voisin.</summary>
        MergedIntoCorridorEndpoint = 1,

        /// <summary>Noeud de decision V1 : graine de controle et de route, hors de la courbe des virages.</summary>
        ControlRouteSeed = 2,

        /// <summary>Arete interieure a un corridor.</summary>
        CorridorInterior = 3,

        /// <summary>Arete d'approche absorbee par les mouvements d'un noeud de decision.</summary>
        MovementApproachPath = 4,

        /// <summary>Arete devenue un JunctionMovement.</summary>
        Movement = 5,

        /// <summary>Jointure devenue une LaneConnection.</summary>
        Connection = 6,

        /// <summary>Poids de choix preserve comme preference de route (plus grand = prefere).</summary>
        RoutePreference = 7,

        /// <summary>Poids d'un choix unique : sans alternative, preserve tel quel.</summary>
        TrivialSingleChoice = 8,

        /// <summary>Role de portail devenu un enregistrement Portal.</summary>
        PortalRecord = 9
    }

    public enum MovementRole
    {
        Turn = 0,
        RoundaboutEntry = 1,
        RoundaboutExit = 2,
        RoundaboutContinuation = 3
    }

    public struct SourceDisposition
    {
        public SourceItemKind Item;
        public string SourceKey;
        public string SourceLabel;
        public DispositionKind Kind;

        /// <summary>Cle de lignee de l'enregistrement cible.</summary>
        public string TargetKey;

        public string Detail;
    }

    public struct AuthoringTask
    {
        public string Category;
        public string SubjectKey;
        public string Text;
    }

    /// <summary>Courbe produite (corridor ou mouvement) et ce qu'il faut pour la mesurer.</summary>
    public sealed class ImportedCurve
    {
        public string Key;
        public RoadRecordKind Kind;
        public string Label;
        public V1Module Module;
        public RoadCurveSample[] Samples;
        public RoadCurve Curve;
        public float ChordDeviationMeters;

        /// <summary>Noeuds points de controle de la courbe.</summary>
        public readonly List<V1Node> VertexNodes = new List<V1Node>();

        /// <summary>Connecteurs fusionnes a une extremite.</summary>
        public readonly List<V1Node> MergedNodes = new List<V1Node>();

        // ---------------------------------------------------------------- corridor
        public string SectionKey;
        public int LateralOrder;
        public bool IsDatum;
        public bool IsRing;
        public bool StartTrimmedForDrivability;
        public bool EndTrimmedForDrivability;

        // ---------------------------------------------------------------- mouvement
        public V1Edge KeyEdge;
        public MovementRole Role;
        public ImportedCurve From;
        public ImportedCurve To;
        public V1Node Seed;
        public float TurnDegrees;
    }

    public sealed class ImportedPortal
    {
        public string Key;
        public V1Node Node;
        public PortalRole Role;
        public ImportedCurve Corridor;
        public float SMeters;
    }

    public sealed class ImportedConnection
    {
        public string Key;
        public V1Edge Join;
        public ImportedCurve From;
        public ImportedCurve To;
    }

    public sealed class ImportedJunction
    {
        public string Key;
        public V1Module Module;
        public JunctionFeature Feature;
        public readonly List<ImportedCurve> Movements = new List<ImportedCurve>();
    }

    public sealed class ImportedSection
    {
        public string Key;
        public string Label;
        public string WidthOrigin;
        public float HalfWidthMeters;
        public readonly List<ImportedCurve> Corridors = new List<ImportedCurve>();
    }

    /// <summary>Resultat complet d'un import. <see cref="Source"/> est nul tant qu'il reste un echec.</summary>
    public sealed class V1ImportResult
    {
        public V1SourceSet SourceSet;
        public RoadModelSource Source;
        public LineageResolution Lineage;
        public readonly List<string> Failures = new List<string>();
        public readonly List<ImportedSection> Sections = new List<ImportedSection>();
        public readonly List<ImportedCurve> Corridors = new List<ImportedCurve>();
        public readonly List<ImportedCurve> Movements = new List<ImportedCurve>();
        public readonly List<ImportedConnection> Connections = new List<ImportedConnection>();
        public readonly List<ImportedPortal> Portals = new List<ImportedPortal>();
        public readonly List<ImportedJunction> Junctions = new List<ImportedJunction>();
        public readonly List<SourceDisposition> Dispositions = new List<SourceDisposition>();
        public readonly List<AuthoringTask> Tasks = new List<AuthoringTask>();
        public readonly Dictionary<string, RoadId> IdByKey = new Dictionary<string, RoadId>(StringComparer.Ordinal);

        public bool Succeeded
        {
            get { return Failures.Count == 0 && Source != null; }
        }

        public RoadId IdOf(string key)
        {
            RoadId id;
            return key != null && IdByKey.TryGetValue(key, out id) ? id : RoadId.None;
        }
    }

    /// <summary>
    /// Registre des cles de lignee d'un import : une cle ne designe qu'une entite. Deux entites
    /// de meme cle sont un echec dur nommant les deux sources.
    /// </summary>
    public sealed class LineageKeyRegistry
    {
        private readonly Dictionary<string, string> _sourceByKey = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<KeyValuePair<string, RoadRecordKind>> _keys = new List<KeyValuePair<string, RoadRecordKind>>();

        public IReadOnlyList<KeyValuePair<string, RoadRecordKind>> Keys
        {
            get { return _keys; }
        }

        public bool TryRegister(string key, RoadRecordKind kind, string sourceDescription, List<string> failures)
        {
            string existing;
            if (_sourceByKey.TryGetValue(key, out existing))
            {
                failures.Add("Collision de cle de lignee '" + key + "' entre " + existing + " et " + sourceDescription + ".");
                return false;
            }

            _sourceByKey.Add(key, sourceDescription);
            _keys.Add(new KeyValuePair<string, RoadRecordKind>(key, kind));
            return true;
        }
    }

    public static class V1RoadModelImporter
    {
        /// <summary>Version de l'importeur : tout changement de regle de decoupage ou de cle l'incremente.</summary>
        public const int ImporterVersion = 2;

        /// <summary>Tolerance de corde du contrat (0,05 m) passee au constructeur de courbe 5.26.</summary>
        public const float ChordToleranceMeters = 0.05f;

        /// <summary>Pas des points de controle d'un mouvement Hermite, en metres.</summary>
        private const float MovementControlStepMeters = 0.25f;

        /// <summary>Pas angulaire maximal entre deux points de controle d'un mouvement, en degres.</summary>
        private const float MovementControlStepDegrees = 1f;

        /// <summary>
        /// Les ancres V1 merge/split sont 7,5 deg avant le raccord tangent. Les bornes du corridor
        /// d'anneau glissent davantage pour donner a la transition a courbure continue sa longueur ;
        /// les noeuds restent sur le meme cercle et sont couverts par les mouvements adjacents.
        /// </summary>
        private const float RoundaboutAnchorShiftDegrees = 28.85f;

        /// <summary>Au-dela, en valeur absolue, un mouvement tourne (etiquette et classement du lissage).</summary>
        public const float TurningThresholdDegrees = 30f;

        /// <summary>Profil de validation : valeurs du contrat, reprises des fixtures 5.26.</summary>
        public static RoadModelValidationProfile ValidationProfile()
        {
            var profile = new RoadModelValidationProfile();
            profile.MaxVehicleHalfWidthMeters = 1.03f;
            profile.MaxVehicleLengthMeters = 4.5f;
            profile.LateralClearanceMarginMeters = 0.25f;
            profile.SeamGapToleranceMeters = 0.05f;
            profile.SeamTangentToleranceDegrees = 5f;
            profile.LengthToleranceMeters = 0.05f;
            profile.EnvelopeOverlapToleranceMeters = 0.05f;
            profile.GroundingMaxOffAxisDegrees = 45f;
            return profile;
        }

        public static RoadLocalizationProfile LocalizationProfile()
        {
            var profile = new RoadLocalizationProfile();
            profile.ScoreBandMeters = 0.15f;
            profile.HysteresisMeters = 0.1f;
            profile.AcceptanceDistanceMeters = 2.5f;
            profile.WrongWayHeadingDegrees = 90f;
            return profile;
        }

        /// <summary>
        /// Copie des valeurs de VehicleProfileDef_Default qui gouvernent l'autorite de braquage.
        /// La geometrie des essieux donne L=3,10 m et le point de reference est a 1,55 m de
        /// l'essieu arriere ; les quatre autres valeurs sont les champs de direction du profil.
        /// </summary>
        public static DrivabilityProfile DrivabilityProfile()
        {
            var profile = new DrivabilityProfile();
            profile.Declared = true;
            profile.WheelbaseMeters = 3.10f;
            profile.ReferencePointAheadRearAxleMeters = 1.55f;
            profile.LowSpeedLockDegrees = 40f;
            profile.HighSpeedLockDegrees = 16f;
            profile.FullReductionSpeedMetersPerSecond = 26f;
            profile.SteeringInactiveBelowMetersPerSecond = 0.25f;
            return profile;
        }

        public static V1ImportResult Import(V1SourceSet set, RoadLineage prior)
        {
            var result = new V1ImportResult();
            result.SourceSet = set;
            if (!set.IsValid)
            {
                result.Failures.AddRange(set.Failures);
                return result;
            }

            var context = new ImportContext(set, result);
            context.Run(prior ?? RoadLineage.Empty());
            return result;
        }

        private sealed class ImportContext
        {
            private readonly V1SourceSet _set;
            private readonly V1ImportResult _result;
            private readonly List<string> _failures;
            private readonly LineageKeyRegistry _registry = new LineageKeyRegistry();

            private readonly Dictionary<V1Node, ImportedCurve> _corridorOfVertex = new Dictionary<V1Node, ImportedCurve>();
            private readonly Dictionary<V1Node, ImportedCurve> _laneStart = new Dictionary<V1Node, ImportedCurve>();
            private readonly Dictionary<V1Node, ImportedCurve> _laneEnd = new Dictionary<V1Node, ImportedCurve>();
            private readonly Dictionary<V1Node, ImportedCurve> _mergedAtEnd = new Dictionary<V1Node, ImportedCurve>();
            private readonly Dictionary<V1Node, ImportedCurve> _mergedAtStart = new Dictionary<V1Node, ImportedCurve>();
            private readonly Dictionary<V1Node, ImportedCurve> _ringStart = new Dictionary<V1Node, ImportedCurve>();
            private readonly Dictionary<V1Node, ImportedCurve> _ringEnd = new Dictionary<V1Node, ImportedCurve>();
            private readonly Dictionary<V1Edge, SourceDisposition> _edgeDisposition = new Dictionary<V1Edge, SourceDisposition>();
            private readonly Dictionary<V1Node, SourceDisposition> _nodeDisposition = new Dictionary<V1Node, SourceDisposition>();
            private readonly Dictionary<V1Node, ImportedJunction> _junctionOfSeed = new Dictionary<V1Node, ImportedJunction>();
            private readonly Dictionary<V1Module, CircleFit> _circleByModule = new Dictionary<V1Module, CircleFit>();

            private struct CircleFit
            {
                public Vector3 Center;
                public float Radius;
                public Vector3 Up;
            }

            public ImportContext(V1SourceSet set, V1ImportResult result)
            {
                _set = set;
                _result = result;
                _failures = result.Failures;
            }

            public void Run(RoadLineage prior)
            {
                foreach (var module in _set.Modules)
                {
                    if (!module.IsJunction)
                    {
                        ImportLaneModule(module);
                    }
                }

                if (_failures.Count > 0)
                {
                    return;
                }

                ClassifyJoins();
                foreach (var module in _set.Modules)
                {
                    if (module.IsJunction)
                    {
                        ImportJunctionModule(module);
                    }
                }

                if (_failures.Count > 0)
                {
                    return;
                }

                ImportPortals();
                DisposeWeightsAndJoins();
                CheckCompleteness();
                if (_failures.Count > 0)
                {
                    return;
                }

                try
                {
                    _result.Lineage = prior.Resolve(_registry.Keys);
                }
                catch (InvalidOperationException exception)
                {
                    _failures.Add(exception.Message);
                    return;
                }

                foreach (var entry in _result.Lineage.Next.Live)
                {
                    _result.IdByKey.Add(entry.Key, entry.Id);
                }

                AddTasks();
                _result.Dispositions.Sort(delegate(SourceDisposition a, SourceDisposition b)
                {
                    int byItem = a.Item.CompareTo(b.Item);
                    return byItem != 0 ? byItem : string.CompareOrdinal(a.SourceKey, b.SourceKey);
                });
                _result.Source = Assemble();
            }

            // ============================================================ modules a voies

            private void ImportLaneModule(V1Module module)
            {
                var lanes = Chains(module, delegate(V1Edge edge) { return true; }, true);
                if (lanes == null)
                {
                    return;
                }

                if (lanes.Count < 2)
                {
                    Fail("Module a voies '" + module.Label + "' : une seule voie, largeur candidate indeterminable sans voie appariee.");
                    return;
                }

                ImportedCurve datum = null;
                var directions = new List<ImportedCurve>();
                var byLane = new List<KeyValuePair<List<V1Node>, ImportedCurve>>();
                foreach (var lane in lanes)
                {
                    var curve = new ImportedCurve();
                    curve.Kind = RoadRecordKind.Corridor;
                    curve.Module = module;
                    curve.Key = "corridor:" + lane[0].Key + ">" + lane[lane.Count - 1].Key;
                    curve.Label = module.Label + ": " + lane[0].Label + " -> " + lane[lane.Count - 1].Label;
                    curve.VertexNodes.AddRange(lane);
                    byLane.Add(new KeyValuePair<List<V1Node>, ImportedCurve>(lane, curve));

                    // Datum : la voie dont le sens, exprime dans le repere local du prefab, suit +z.
                    // Fait structurel de la source, jamais un nom.
                    Vector3 local = Quaternion.Inverse(module.Rotation) * (lane[lane.Count - 1].Position - lane[0].Position);
                    if (local.sqrMagnitude > 1e-8f && Vector3.Dot(local.normalized, Vector3.forward) > 0.7071f)
                    {
                        directions.Add(curve);
                    }
                }

                if (directions.Count != 1)
                {
                    Fail("Module a voies '" + module.Label + "' : " + directions.Count + " voies de sens local +z, un datum unique attendu.");
                    return;
                }

                datum = directions[0];
                List<V1Node> datumLane = null;
                foreach (var pair in byLane)
                {
                    if (pair.Value == datum)
                    {
                        datumLane = pair.Key;
                    }
                }

                var probe = BuildCurve(datumLane, 1f, 1f, datum.Label);
                if (probe == null)
                {
                    return;
                }

                var lateral = new Dictionary<ImportedCurve, float>();
                foreach (var pair in byLane)
                {
                    var middle = pair.Key[pair.Key.Count / 2];
                    lateral[pair.Value] = pair.Value == datum ? 0f : new RoadCurve(probe).Project(middle.Position).LateralOffsetMeters;
                }

                byLane.Sort(delegate(KeyValuePair<List<V1Node>, ImportedCurve> a, KeyValuePair<List<V1Node>, ImportedCurve> b)
                {
                    return lateral[a.Value].CompareTo(lateral[b.Value]);
                });

                float spacing = float.PositiveInfinity;
                for (int i = 1; i < byLane.Count; i++)
                {
                    float gap = lateral[byLane[i].Value] - lateral[byLane[i - 1].Value];
                    if (gap < 0.01f)
                    {
                        Fail("Module a voies '" + module.Label + "' : deux voies a la meme position transversale, ordre lateral indeterminable.");
                        return;
                    }

                    spacing = Mathf.Min(spacing, gap);
                }

                var section = new ImportedSection();
                section.Key = "section:" + module.Key;
                section.Label = module.Label;
                section.HalfWidthMeters = 0.5f * spacing;
                section.WidthOrigin = "moitie de l'ecart mesure entre voies appariees";
                Register(section.Key, RoadRecordKind.Section, "module '" + module.Label + "'");

                for (int i = 0; i < byLane.Count; i++)
                {
                    var lane = byLane[i].Key;
                    var curve = byLane[i].Value;
                    curve.SectionKey = section.Key;
                    curve.LateralOrder = i;
                    curve.IsDatum = curve == datum;
                    if (!Finish(curve, lane, section.HalfWidthMeters))
                    {
                        return;
                    }

                    section.Corridors.Add(curve);
                    _laneStart[lane[0]] = curve;
                    _laneEnd[lane[lane.Count - 1]] = curve;
                    for (int n = 0; n < lane.Count; n++)
                    {
                        _corridorOfVertex[lane[n]] = curve;
                        DisposeNode(lane[n], DispositionKind.CorridorVertex, curve.Key, "point " + n + " de la courbe");
                        if (n > 0)
                        {
                            DisposeEdge(FindAuthored(lane[n - 1], lane[n]), DispositionKind.CorridorInterior, curve.Key, "arete interieure");
                        }
                    }
                }

                _result.Sections.Add(section);
            }

            // ============================================================ jointures

            private void ClassifyJoins()
            {
                foreach (var join in _set.Joins)
                {
                    bool fromJunction = join.From.Module.IsJunction;
                    bool toJunction = join.To.Module.IsJunction;
                    string detail = "ecart " + MigrationFormat.Meters(join.GapMeters) + " m, angle " + MigrationFormat.Degrees(join.AngleDegrees) + " deg";

                    if (fromJunction && toJunction)
                    {
                        Fail("Jointure directe entre deux carrefours ('" + join.From.Module.Label + "' -> '" + join.To.Module.Label + "') : aucun corridor ne les separe.");
                        continue;
                    }

                    if (!fromJunction && !_laneEnd.ContainsKey(join.From))
                    {
                        Fail("Jointure depuis '" + join.From.Label + "' qui n'est pas une fin de voie.");
                        continue;
                    }

                    if (!toJunction && !_laneStart.ContainsKey(join.To))
                    {
                        Fail("Jointure vers '" + join.To.Label + "' qui n'est pas un debut de voie.");
                        continue;
                    }

                    if (!fromJunction && !toJunction && _laneEnd[join.From] == _laneStart[join.To])
                    {
                        Fail("Jointure en boucle sur un meme corridor : '" + join.From.Label + "' -> '" + join.To.Label + "'.");
                        continue;
                    }

                    if (!fromJunction && !toJunction)
                    {
                        var connection = new ImportedConnection();
                        connection.Join = join;
                        connection.From = _laneEnd[join.From];
                        connection.To = _laneStart[join.To];
                        connection.Key = "connection:" + join.From.Key + ">" + join.To.Key;
                        Register(connection.Key, RoadRecordKind.Connection, "jointure '" + join.From.Label + "' -> '" + join.To.Label + "'");
                        _result.Connections.Add(connection);
                        DisposeEdge(join, DispositionKind.Connection, connection.Key, detail);
                    }
                    else if (toJunction)
                    {
                        var corridor = _laneEnd[join.From];
                        _mergedAtEnd[join.To] = corridor;
                        corridor.MergedNodes.Add(join.To);
                        DisposeNode(join.To, DispositionKind.MergedIntoCorridorEndpoint, corridor.Key, "fusionne a la fin, " + detail);
                        DisposeEdge(join, DispositionKind.MergedIntoCorridorEndpoint, corridor.Key, detail);
                    }
                    else
                    {
                        var corridor = _laneStart[join.To];
                        _mergedAtStart[join.From] = corridor;
                        corridor.MergedNodes.Add(join.From);
                        DisposeNode(join.From, DispositionKind.MergedIntoCorridorEndpoint, corridor.Key, "fusionne au debut, " + detail);
                        DisposeEdge(join, DispositionKind.MergedIntoCorridorEndpoint, corridor.Key, detail);
                    }
                }
            }

            // ============================================================ carrefours

            private void ImportJunctionModule(V1Module module)
            {
                foreach (var node in module.Nodes)
                {
                    if (node.Role == LaneNodeRole.PortalEntry || node.Role == LaneNodeRole.PortalExit)
                    {
                        Fail("Portail '" + node.Label + "' dans le carrefour '" + module.Label + "' : forme non supportee.");
                        return;
                    }
                }

                var junction = new ImportedJunction();
                junction.Module = module;
                junction.Key = "junction:" + module.Key;
                junction.Feature = module.Kind == V1ModuleKind.Crossroads ? JunctionFeature.Crossroads
                    : module.Kind == V1ModuleKind.TJunction ? JunctionFeature.TJunction
                    : JunctionFeature.Roundabout;
                Register(junction.Key, RoadRecordKind.Junction, "module '" + module.Label + "'");
                _result.Junctions.Add(junction);

                // Arete interieure : entre deux noeuds interieurs, depuis un noeud a successeur unique.
                Predicate<V1Edge> interior = delegate(V1Edge edge)
                {
                    return edge.From.Role == LaneNodeRole.Normal && edge.To.Role == LaneNodeRole.Normal
                        && edge.From.AuthoredSuccessorCount == 1;
                };

                var rings = Chains(module, interior, false);
                if (rings == null)
                {
                    return;
                }

                if (module.Kind == V1ModuleKind.Roundabout)
                {
                    var ringNodes = new List<V1Node>();
                    for (int r = 0; r < rings.Count; r++)
                    {
                        for (int n = 0; n < rings[r].Count; n++)
                        {
                            if (!ringNodes.Contains(rings[r][n]))
                            {
                                ringNodes.Add(rings[r][n]);
                            }
                        }
                    }

                    CircleFit fit;
                    if (!TryFitCircle(ringNodes, out fit))
                    {
                        Fail("Anneau de '" + module.Label + "' : cercle des noeuds V1 indeterminable.");
                        return;
                    }

                    _circleByModule[module] = fit;
                }

                var seeds = new List<V1Node>();
                foreach (var node in module.Nodes)
                {
                    if (node.Role == LaneNodeRole.Normal && !InAnyChain(rings, node))
                    {
                        seeds.Add(node);
                    }
                }

                foreach (var ring in rings)
                {
                    if (!ImportRing(module, ring))
                    {
                        return;
                    }
                }

                foreach (var node in module.Nodes)
                {
                    if (node.IsIncomingConnector && !ImportJunctionEntry(junction, node))
                    {
                        return;
                    }
                }

                foreach (var seed in seeds)
                {
                    if (!ImportSeed(junction, seed))
                    {
                        return;
                    }
                }

                foreach (var ring in rings)
                {
                    var end = ring[ring.Count - 1];
                    if (end.AuthoredSuccessorCount == 0)
                    {
                        Fail("Fin d'anneau '" + end.Label + "' sans successeur dans '" + module.Label + "'.");
                        return;
                    }

                    foreach (var edge in end.Outgoing)
                    {
                        if (edge.IsConnectorJoin)
                        {
                            continue;
                        }

                        MovementRole role = edge.To.IsOutgoingConnector ? MovementRole.RoundaboutExit : MovementRole.RoundaboutContinuation;
                        if (!AddMovement(junction, edge, _ringEnd[end], DepartureOf(edge.To, module), role, null))
                        {
                            return;
                        }
                    }
                }

                BoundaryCheck(junction);
            }

            private bool ImportRing(V1Module module, List<V1Node> ring)
            {
                var start = ring[0];
                ImportedCurve seedCorridor = null;
                foreach (var edge in start.Incoming)
                {
                    if (edge.From.IsIncomingConnector)
                    {
                        ImportedCurve approach;
                        if (!_mergedAtEnd.TryGetValue(edge.From, out approach))
                        {
                            Fail("Entree d'anneau '" + edge.From.Label + "' sans corridor d'approche dans '" + module.Label + "'.");
                            return false;
                        }

                        if (seedCorridor == null || string.CompareOrdinal(approach.Key, seedCorridor.Key) < 0)
                        {
                            seedCorridor = approach;
                        }
                    }
                }

                if (seedCorridor == null)
                {
                    Fail("Anneau '" + start.Label + "' de '" + module.Label + "' sans entree : largeur candidate indeterminable.");
                    return false;
                }

                float halfWidth = seedCorridor.Samples[seedCorridor.Samples.Length - 1].HalfWidthLeftMeters;
                var curve = new ImportedCurve();
                curve.Kind = RoadRecordKind.Corridor;
                curve.Module = module;
                curve.IsRing = true;
                curve.Key = "corridor:" + start.Key + ">" + ring[ring.Count - 1].Key;
                curve.Label = module.Label + ": " + start.Label + " -> " + ring[ring.Count - 1].Label;
                curve.VertexNodes.AddRange(ring);

                var section = new ImportedSection();
                section.Key = "section:" + start.Key + ">" + ring[ring.Count - 1].Key;
                section.Label = curve.Label;
                section.HalfWidthMeters = halfWidth;
                section.WidthOrigin = "amorcee depuis l'approche " + seedCorridor.Label;
                Register(section.Key, RoadRecordKind.Section, "anneau " + curve.Label);

                curve.SectionKey = section.Key;
                curve.LateralOrder = 0;
                curve.IsDatum = true;
                if (!Finish(curve, ring, halfWidth))
                {
                    return false;
                }

                section.Corridors.Add(curve);
                _result.Sections.Add(section);
                _ringStart[start] = curve;
                _ringEnd[ring[ring.Count - 1]] = curve;
                for (int n = 0; n < ring.Count; n++)
                {
                    _corridorOfVertex[ring[n]] = curve;
                    DisposeNode(ring[n], DispositionKind.CorridorVertex, curve.Key, "point " + n + " de l'anneau");
                    if (n > 0)
                    {
                        DisposeEdge(FindAuthored(ring[n - 1], ring[n]), DispositionKind.CorridorInterior, curve.Key, "arete interieure d'anneau");
                    }
                }

                return true;
            }

            /// <summary>Connecteur entrant du carrefour : fusionne a l'approche, vers une graine ou un anneau.</summary>
            private bool ImportJunctionEntry(ImportedJunction junction, V1Node entry)
            {
                if (!_mergedAtEnd.ContainsKey(entry))
                {
                    Fail("Branche '" + entry.Label + "' de '" + junction.Module.Label + "' sans corridor d'approche.");
                    return false;
                }

                if (entry.AuthoredSuccessorCount != 1)
                {
                    Fail("Connecteur entrant '" + entry.Label + "' de '" + junction.Module.Label + "' : un successeur unique attendu.");
                    return false;
                }

                var edge = AuthoredOf(entry)[0];
                ImportedCurve ring;
                if (_ringStart.TryGetValue(edge.To, out ring))
                {
                    return AddMovement(junction, edge, _mergedAtEnd[entry], ring, MovementRole.RoundaboutEntry, null);
                }

                if (edge.To.Role == LaneNodeRole.Normal && !_corridorOfVertex.ContainsKey(edge.To))
                {
                    // Chemin d'approche d'une graine : dispose avec la graine.
                    return true;
                }

                Fail("Connecteur entrant '" + entry.Label + "' de '" + junction.Module.Label + "' vers '" + edge.To.Label + "' : motif non supporte.");
                return false;
            }

            private bool ImportSeed(ImportedJunction junction, V1Node seed)
            {
                var module = junction.Module;
                if (seed.Incoming.Count != 1 || !seed.Incoming[0].From.IsIncomingConnector)
                {
                    Fail("Noeud de decision '" + seed.Label + "' de '" + module.Label + "' : un predecesseur unique, connecteur entrant, attendu.");
                    return false;
                }

                var approachEdge = seed.Incoming[0];
                var approach = _mergedAtEnd[approachEdge.From];
                if (seed.AuthoredSuccessorCount == 0)
                {
                    Fail("Noeud de decision '" + seed.Label + "' de '" + module.Label + "' sans successeur.");
                    return false;
                }

                var keys = new List<string>();
                foreach (var edge in AuthoredOf(seed))
                {
                    if (!edge.To.IsOutgoingConnector)
                    {
                        Fail("Noeud de decision '" + seed.Label + "' vers '" + edge.To.Label + "' : seule une sortie de carrefour est supportee.");
                        return false;
                    }

                    var departure = DepartureOf(edge.To, module);
                    if (departure == null || !AddMovement(junction, edge, approach, departure, MovementRole.Turn, seed))
                    {
                        return false;
                    }

                    keys.Add(MovementKey(edge));
                }

                _junctionOfSeed[seed] = junction;
                DisposeNode(seed, DispositionKind.ControlRouteSeed, junction.Key, "graine de " + keys.Count + " mouvements");
                DisposeEdge(approachEdge, DispositionKind.MovementApproachPath, junction.Key, "absorbee par " + string.Join(", ", keys.ToArray()));
                return true;
            }

            private ImportedCurve DepartureOf(V1Node node, V1Module module)
            {
                ImportedCurve curve;
                if (_ringStart.TryGetValue(node, out curve) || _mergedAtStart.TryGetValue(node, out curve))
                {
                    return curve;
                }

                Fail("Sortie '" + node.Label + "' de '" + module.Label + "' sans corridor de depart.");
                return null;
            }

            private static string MovementKey(V1Edge edge)
            {
                return "movement:" + edge.From.Key + ">" + edge.To.Key;
            }

            private bool AddMovement(ImportedJunction junction, V1Edge edge, ImportedCurve from, ImportedCurve to, MovementRole role, V1Node seed)
            {
                if (from == null || to == null)
                {
                    return false;
                }

                if (from == to)
                {
                    Fail("Mouvement en boucle sur un meme corridor : " + from.Label + ".");
                    return false;
                }

                // Les connecteurs V1 sont des ancres de topologie, pas des raccords tangentiels.
                // Pour les entrees/sorties d'anneau seulement, la borne V2 glisse d'un metre le
                // long de l'axe adjacent ; le mouvement couvre l'ancien connecteur et le rapport
                // publie le deplacement d'ancre.
                if (role == MovementRole.RoundaboutEntry && !from.EndTrimmedForDrivability)
                {
                    TrimEnd(from, 2.5f);
                    from.EndTrimmedForDrivability = true;
                }
                else if (role == MovementRole.RoundaboutExit && !to.StartTrimmedForDrivability)
                {
                    TrimStart(to, 2.5f);
                    to.StartTrimmedForDrivability = true;
                }

                var fromEnd = from.Curve.Sample(from.Curve.Length);
                var toStart = to.Curve.Sample(0f);
                Vector3 p0 = fromEnd.Position;
                Vector3 p1 = toStart.Position;
                float chord = (p1 - p0).magnitude;
                if (chord < 0.01f)
                {
                    Fail("Mouvement '" + edge.From.Label + "' -> '" + edge.To.Label + "' de longueur nulle.");
                    return false;
                }

                var movement = new ImportedCurve();
                movement.Kind = RoadRecordKind.Movement;
                movement.Module = junction.Module;
                movement.Key = MovementKey(edge);
                movement.KeyEdge = edge;
                movement.Role = role;
                movement.From = from;
                movement.To = to;
                movement.Seed = seed;
                movement.TurnDegrees = fromEnd.SignedHeadingDegrees(toStart.Tangent);
                movement.Label = junction.Module.Label + ": " + edge.From.Label + " -> " + edge.To.Label + " (" + Describe(role, movement.TurnDegrees) + ")";

                CircleFit circle;
                if (role == MovementRole.RoundaboutContinuation && from.IsRing && to.IsRing
                    && _circleByModule.TryGetValue(junction.Module, out circle))
                {
                    float sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(circle.Up,
                        (p0 - circle.Center).normalized), fromEnd.Tangent));
                    movement.Samples = BuildCircleArc(circle, p0, p1, sign,
                        fromEnd.HalfWidthLeftMeters, toStart.HalfWidthLeftMeters,
                        fromEnd.HalfWidthRightMeters, toStart.HalfWidthRightMeters);
                }
                else
                {
                    movement.Samples = BuildSmoothCurve(fromEnd, toStart);
                }

                movement.ChordDeviationMeters = MeasureChordDeviation(movement.Samples);
                movement.Curve = new RoadCurve(movement.Samples);

                Register(movement.Key, RoadRecordKind.Movement, "arete '" + edge.From.Label + "' -> '" + edge.To.Label + "' de '" + junction.Module.Label + "'");
                junction.Movements.Add(movement);
                _result.Movements.Add(movement);
                DisposeEdge(edge, DispositionKind.Movement, movement.Key, movement.Label);
                return true;
            }

            private static string Describe(MovementRole role, float turnDegrees)
            {
                switch (role)
                {
                    case MovementRole.RoundaboutEntry:
                        return "entree d'anneau";
                    case MovementRole.RoundaboutExit:
                        return "sortie d'anneau";
                    case MovementRole.RoundaboutContinuation:
                        return "continuation d'anneau";
                }

                if (Mathf.Abs(turnDegrees) < TurningThresholdDegrees)
                {
                    return "tout droit";
                }

                return turnDegrees > 0f ? "droite" : "gauche";
            }

            private void BoundaryCheck(ImportedJunction junction)
            {
                if (junction.Movements.Count == 0)
                {
                    Fail("Carrefour '" + junction.Module.Label + "' sans mouvement.");
                }
            }

            // ============================================================ portails

            private void ImportPortals()
            {
                foreach (var node in _set.Nodes)
                {
                    if (node.IsEntryPortal)
                    {
                        AddPortal(node, PortalRole.Entry);
                    }

                    if (node.IsExitPortal)
                    {
                        AddPortal(node, PortalRole.Exit);
                    }
                }
            }

            private void AddPortal(V1Node node, PortalRole role)
            {
                ImportedCurve corridor;
                if (!_corridorOfVertex.TryGetValue(node, out corridor) || corridor.IsRing)
                {
                    Fail("Portail '" + node.Label + "' hors d'un corridor de voie.");
                    return;
                }

                var portal = new ImportedPortal();
                portal.Key = "portal:" + node.Key + ":" + role;
                portal.Node = node;
                portal.Role = role;
                portal.Corridor = corridor;
                portal.SMeters = corridor.Curve.Project(node.Position).SMeters;
                Register(portal.Key, RoadRecordKind.Portal, "portail '" + node.Label + "' (" + role + ")");
                _result.Portals.Add(portal);
                Add(SourceItemKind.PortalRole, node.Key + ":" + role, node.Label + " (" + role + ", exitReusesEntry=" + (node.ExitReusesEntry ? "vrai" : "faux") + ")",
                    DispositionKind.PortalRecord, portal.Key, "s = " + MigrationFormat.Meters(portal.SMeters) + " m sur " + corridor.Label);
            }

            // ============================================================ poids et jointures

            private void DisposeWeightsAndJoins()
            {
                // Polarite « plus grand = prefere » : un poids negatif, ou un choix dont aucun poids
                // n'est positif (repli V1 sur le premier successeur), n'a pas de preference definie.
                foreach (var node in _set.Nodes)
                {
                    if (node.AuthoredSuccessorCount < 2)
                    {
                        continue;
                    }

                    bool anyPositive = false;
                    foreach (var edge in AuthoredOf(node))
                    {
                        if (edge.Weight < 0f)
                        {
                            Fail("Poids negatif sur '" + edge.From.Label + "' -> '" + edge.To.Label + "' : preference indefinie.");
                        }

                        anyPositive |= edge.Weight > 0f;
                    }

                    if (!anyPositive)
                    {
                        Fail("Choix sans aucun poids positif sur '" + node.Label + "' (module '" + node.Module.Label + "') : preference indefinie.");
                    }
                }

                foreach (var edge in _set.Edges)
                {
                    SourceDisposition edgeDisposition;
                    if (!_edgeDisposition.TryGetValue(edge, out edgeDisposition))
                    {
                        continue;
                    }

                    if (edge.IsConnectorJoin)
                    {
                        Add(SourceItemKind.ConnectorMatch, edge.Key, edge.From.Label + " -> " + edge.To.Label,
                            edgeDisposition.Kind, edgeDisposition.TargetKey, edgeDisposition.Detail);
                        continue;
                    }

                    bool choice = edge.From.AuthoredSuccessorCount >= 2;
                    string detail = "poids " + MigrationFormat.Weight(edge.Weight) + (choice
                        ? " sur " + edge.From.AuthoredSuccessorCount + " choix, plus grand = prefere, cible " + edgeDisposition.Detail
                        : ", choix unique sans alternative");
                    Add(SourceItemKind.TurnWeight, edge.Key, edge.From.Label + " -> " + edge.To.Label,
                        choice ? DispositionKind.RoutePreference : DispositionKind.TrivialSingleChoice, edgeDisposition.TargetKey, detail);
                }
            }

            private void CheckCompleteness()
            {
                foreach (var node in _set.Nodes)
                {
                    if (!_nodeDisposition.ContainsKey(node))
                    {
                        Fail("Noeud non dispose : '" + node.Label + "' (module '" + node.Module.Label + "').");
                    }
                }

                foreach (var edge in _set.Edges)
                {
                    if (!_edgeDisposition.ContainsKey(edge))
                    {
                        Fail("Arete non disposee : '" + edge.From.Label + "' -> '" + edge.To.Label + "' (module '" + edge.From.Module.Label + "').");
                    }
                }
            }

            // ============================================================ taches d'authoring

            private void AddTasks()
            {
                foreach (var section in _result.Sections)
                {
                    Task("Largeur", section.Key, "Largeur candidate " + MigrationFormat.Meters(section.HalfWidthMeters) + " m par cote (" + section.WidthOrigin + ") : revue requise avant d'etre authoritative.");
                    Task("Section", section.Key, "Vitesse limite, classes de vehicules et surface absentes de V1 : a authorer (0 m/s, aucune classe et le defaut d'enum `Asphalt` en attendant, jamais un defaut invente).");

                    if (CountSameDirectionPairs(section) > 0)
                    {
                        Task("Adjacence", section.Key, "Corridors de meme sens voisins : LaneAdjacency a authorer explicitement.");
                    }
                }

                foreach (var junction in _result.Junctions)
                {
                    Task("Controle", junction.Key, junction.Movements.Count + " mouvements sans JunctionControl : une liaison de controle par mouvement a authorer (Uncontrolled est un choix explicite, jamais un repli).");
                    Task("Ligne", junction.Key, "Lignes d'arret ou de cession absentes de V1 : a authorer sur les controles.");
                    Task("Conflit", junction.Key, "Zones de conflit absentes de V1 : a generer depuis les enveloppes puis revoir.");
                    Task("Signal", junction.Key, "Aucune signalisation en V1 : declarer explicitement le carrefour non signalise, sans plan implicite.");
                    Task("Largeur", junction.Key, "Largeurs des mouvements amorcees depuis l'approche et le depart : revue requise.");
                    Task("Frontiere", junction.Key, "Frontiere candidate (boite englobant les mouvements et leur enveloppe) : revue requise.");
                }

                foreach (var portal in _result.Portals)
                {
                    Task("Portail", portal.Key, "Enveloppe candidate (longueur = gabarit max du profil, demi-largeur = corridor) : revue requise.");
                }
            }

            private static int CountSameDirectionPairs(ImportedSection section)
            {
                int pairs = 0;
                for (int i = 0; i < section.Corridors.Count; i++)
                {
                    for (int j = i + 1; j < section.Corridors.Count; j++)
                    {
                        var a = section.Corridors[i].Curve.Sample(0f).Tangent;
                        var b = section.Corridors[j].Curve.Sample(0f).Tangent;
                        if (Vector3.Dot(a, b) > 0f)
                        {
                            pairs++;
                        }
                    }
                }

                return pairs;
            }

            private void Task(string category, string subjectKey, string text)
            {
                var task = new AuthoringTask();
                task.Category = category;
                task.SubjectKey = subjectKey;
                task.Text = text;
                _result.Tasks.Add(task);
            }

            // ============================================================ assemblage

            private RoadModelSource Assemble()
            {
                var ids = _result.IdByKey;
                var modelId = _result.Lineage.Next.ModelId;
                var source = new RoadModelSource();
                source.ModelId = modelId;
                source.Label = "MVP_Run (import V1, Story 5.27)";
                source.ValidationProfile = ValidationProfile();
                source.LocalizationProfile = LocalizationProfile();
                source.DrivabilityProfile = DrivabilityProfile();

                var manifest = new List<ImportManifestEntry>();

                source.Sections = new RoadSection[_result.Sections.Count];
                for (int i = 0; i < _result.Sections.Count; i++)
                {
                    var section = _result.Sections[i];
                    var record = new RoadSection();
                    record.Id = ids[section.Key];
                    record.Label = section.Label;
                    record.RoadClass = RoadClass.Unspecified;
                    record.DefaultAllowedVehicleClasses = VehicleClassMask.None;
                    source.Sections[i] = record;
                    var nodes = new List<V1Node>();
                    foreach (var corridor in section.Corridors)
                    {
                        nodes.AddRange(corridor.VertexNodes);
                    }

                    manifest.Add(Manifest(section.Key, RoadRecordKind.Section, record.Id, modelId, nodes));
                }

                var corridors = new List<ImportedCurve>();
                foreach (var section in _result.Sections)
                {
                    corridors.AddRange(section.Corridors);
                }

                source.Corridors = new LaneCorridor[corridors.Count];
                for (int i = 0; i < corridors.Count; i++)
                {
                    var curve = corridors[i];
                    _result.Corridors.Add(curve);
                    var record = new LaneCorridor();
                    record.Id = ids[curve.Key];
                    record.Label = curve.Label;
                    record.SectionId = ids[curve.SectionKey];
                    record.LateralOrder = curve.LateralOrder;
                    record.IsCrossSectionDatum = curve.IsDatum;
                    record.Samples = curve.Samples;
                    record.LengthMeters = curve.Curve.Length;
                    source.Corridors[i] = record;
                    var nodes = new List<V1Node>(curve.VertexNodes);
                    nodes.AddRange(curve.MergedNodes);
                    manifest.Add(Manifest(curve.Key, RoadRecordKind.Corridor, record.Id, modelId, nodes));
                }

                source.Connections = new LaneConnection[_result.Connections.Count];
                for (int i = 0; i < _result.Connections.Count; i++)
                {
                    var connection = _result.Connections[i];
                    var record = new LaneConnection();
                    record.Id = ids[connection.Key];
                    record.FromCorridorId = ids[connection.From.Key];
                    record.ToCorridorId = ids[connection.To.Key];
                    record.Kind = LaneConnectionKind.Continuation;
                    source.Connections[i] = record;
                    manifest.Add(Manifest(connection.Key, RoadRecordKind.Connection, record.Id, modelId,
                        new List<V1Node> { connection.Join.From, connection.Join.To }));
                }

                source.Adjacencies = new LaneAdjacency[0];

                source.Junctions = new Junction[_result.Junctions.Count];
                var movements = new List<JunctionMovement>();
                for (int i = 0; i < _result.Junctions.Count; i++)
                {
                    var junction = _result.Junctions[i];
                    var record = new Junction();
                    record.Id = ids[junction.Key];
                    record.Label = junction.Module.Label;
                    record.Feature = junction.Feature;
                    record.Boundary = Boundary(junction);
                    source.Junctions[i] = record;
                    manifest.Add(Manifest(junction.Key, RoadRecordKind.Junction, record.Id, modelId, junction.Module.Nodes));

                    foreach (var movement in junction.Movements)
                    {
                        var m = new JunctionMovement();
                        m.Id = ids[movement.Key];
                        m.Label = movement.Label;
                        m.JunctionId = record.Id;
                        m.FromCorridorId = ids[movement.From.Key];
                        m.ToCorridorId = ids[movement.To.Key];
                        m.Samples = movement.Samples;
                        m.LengthMeters = movement.Curve.Length;
                        m.RoutePreferenceWeight = movement.KeyEdge.Weight;
                        movements.Add(m);
                        var nodes = new List<V1Node> { movement.KeyEdge.From, movement.KeyEdge.To };
                        if (movement.Seed != null)
                        {
                            nodes.Add(movement.Seed.Incoming[0].From);
                        }

                        manifest.Add(Manifest(movement.Key, RoadRecordKind.Movement, m.Id, modelId, nodes));
                    }
                }

                source.Movements = movements.ToArray();
                source.Controls = new JunctionControl[0];
                source.ConflictZones = new ConflictZone[0];
                source.SignalPlans = new SignalPlan[0];

                source.Portals = new Portal[_result.Portals.Count];
                var profile = source.ValidationProfile;
                for (int i = 0; i < _result.Portals.Count; i++)
                {
                    var portal = _result.Portals[i];
                    var point = portal.Corridor.Curve.Sample(portal.SMeters);
                    var record = new Portal();
                    record.Id = ids[portal.Key];
                    record.Label = portal.Node.Label;
                    record.CorridorId = ids[portal.Corridor.Key];
                    record.Role = portal.Role;
                    record.SMeters = portal.SMeters;
                    record.EnvelopeLengthMeters = profile.MaxVehicleLengthMeters;
                    record.EnvelopeHalfWidthMeters = Mathf.Min(point.HalfWidthLeftMeters, point.HalfWidthRightMeters);
                    source.Portals[i] = record;
                    manifest.Add(Manifest(portal.Key, RoadRecordKind.Portal, record.Id, modelId, new List<V1Node> { portal.Node }));
                }

                manifest.Sort(delegate(ImportManifestEntry a, ImportManifestEntry b) { return string.CompareOrdinal(a.ImporterSlot, b.ImporterSlot); });
                var tombstones = _result.Lineage.Next.Tombstones;
                var tombstoneIds = new RoadId[tombstones.Count];
                for (int i = 0; i < tombstones.Count; i++)
                {
                    tombstoneIds[i] = tombstones[i].Id;
                }

                source.Manifest = new ImportManifest();
                source.Manifest.Entries = manifest.ToArray();
                source.Manifest.TombstonedIds = tombstoneIds;
                source.Manifest.Remaps = new RoadIdRemap[0];
                return source;
            }

            private static RoadBoundsBox Boundary(ImportedJunction junction)
            {
                var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                foreach (var movement in junction.Movements)
                {
                    foreach (var sample in movement.Samples)
                    {
                        float reach = Mathf.Max(sample.HalfWidthLeftMeters, sample.HalfWidthRightMeters);
                        min = Vector3.Min(min, sample.Position - Vector3.one * reach);
                        max = Vector3.Max(max, sample.Position + Vector3.one * reach);
                    }
                }

                var box = new RoadBoundsBox();
                box.Center = 0.5f * (min + max);
                box.Extents = 0.5f * (max - min);
                return box;
            }

            private static ImportManifestEntry Manifest(string key, RoadRecordKind kind, RoadId id, RoadId modelId, IList<V1Node> nodes)
            {
                var sorted = new List<V1Node>(nodes);
                sorted.Sort(delegate(V1Node a, V1Node b) { return string.CompareOrdinal(a.Key, b.Key); });
                var traces = new List<SourceTrace>();
                string last = null;
                foreach (var node in sorted)
                {
                    if (node.Key == last)
                    {
                        continue;
                    }

                    last = node.Key;
                    var trace = new SourceTrace();
                    trace.SourceAssetGuid = node.SceneGuid;
                    trace.SourceObjectFileId = node.FileId;
                    trace.RelationKey = node.Key;
                    traces.Add(trace);
                }

                var entry = new ImportManifestEntry();
                entry.RecordId = id;
                entry.Kind = kind;
                entry.ModelId = modelId;
                entry.ImporterSlot = key;
                entry.SourceKeys = traces.ToArray();
                return entry;
            }

            // ============================================================ outils

            /// <summary>
            /// Chaines de noeuds d'un module le long des aretes authorees retenues par
            /// <paramref name="follow"/>. Branche ou cycle = echec dur. Avec
            /// <paramref name="coverAll"/>, chaque noeud du module doit appartenir a une chaine.
            /// </summary>
            private List<List<V1Node>> Chains(V1Module module, Predicate<V1Edge> follow, bool coverAll)
            {
                var next = new Dictionary<V1Node, V1Node>();
                var hasPrevious = new HashSet<V1Node>();
                var members = new HashSet<V1Node>();
                foreach (var node in module.Nodes)
                {
                    foreach (var edge in AuthoredOf(node))
                    {
                        if (!follow(edge))
                        {
                            continue;
                        }

                        if (next.ContainsKey(node) || hasPrevious.Contains(edge.To))
                        {
                            Fail("Module '" + module.Label + "' : branche sur '" + node.Label + "' ou '" + edge.To.Label + "', chaine de voie attendue.");
                            return null;
                        }

                        next.Add(node, edge.To);
                        hasPrevious.Add(edge.To);
                        members.Add(node);
                        members.Add(edge.To);
                    }
                }

                var chains = new List<List<V1Node>>();
                var covered = new HashSet<V1Node>();
                foreach (var node in module.Nodes)
                {
                    bool isStart = coverAll ? !hasPrevious.Contains(node) : members.Contains(node) && !hasPrevious.Contains(node);
                    if (!isStart)
                    {
                        continue;
                    }

                    var chain = new List<V1Node> { node };
                    covered.Add(node);
                    V1Node cursor;
                    while (next.TryGetValue(chain[chain.Count - 1], out cursor))
                    {
                        chain.Add(cursor);
                        covered.Add(cursor);
                    }

                    if (chain.Count < 2)
                    {
                        Fail("Module '" + module.Label + "' : noeud isole '" + node.Label + "'.");
                        return null;
                    }

                    chains.Add(chain);
                }

                foreach (var node in coverAll ? (IEnumerable<V1Node>)module.Nodes : members)
                {
                    if (!covered.Contains(node))
                    {
                        Fail("Module '" + module.Label + "' : cycle ou noeud hors chaine '" + node.Label + "'.");
                        return null;
                    }
                }

                return chains;
            }

            private static bool InAnyChain(List<List<V1Node>> chains, V1Node node)
            {
                foreach (var chain in chains)
                {
                    if (chain.Contains(node))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static List<V1Edge> AuthoredOf(V1Node node)
            {
                var edges = new List<V1Edge>();
                foreach (var edge in node.Outgoing)
                {
                    if (!edge.IsConnectorJoin)
                    {
                        edges.Add(edge);
                    }
                }

                return edges;
            }

            private static V1Edge FindAuthored(V1Node from, V1Node to)
            {
                foreach (var edge in AuthoredOf(from))
                {
                    if (edge.To == to)
                    {
                        return edge;
                    }
                }

                return null;
            }

            private RoadCurveSample[] BuildCurve(List<V1Node> nodes, float halfWidthLeft, float halfWidthRight, string label)
            {
                float unused;
                return BuildCurve(nodes, halfWidthLeft, halfWidthRight, label, out unused);
            }

            private RoadCurveSample[] BuildCurve(List<V1Node> nodes, float halfWidthLeft, float halfWidthRight, string label, out float deviation)
            {
                deviation = 0f;
                var positions = new Vector3[nodes.Count];
                var ups = new Vector3[nodes.Count];
                var left = new float[nodes.Count];
                var right = new float[nodes.Count];
                for (int i = 0; i < nodes.Count; i++)
                {
                    positions[i] = nodes[i].Position;
                    ups[i] = nodes[i].Up;
                    left[i] = halfWidthLeft;
                    right[i] = halfWidthRight;
                }

                try
                {
                    return RoadCurveBuilder.Build(positions, ups, left, right, ChordToleranceMeters, out deviation);
                }
                catch (ArgumentException exception)
                {
                    Fail("Courbe de " + label + " : " + exception.Message);
                    return null;
                }
            }

            private bool Finish(ImportedCurve curve, List<V1Node> nodes, float halfWidth)
            {
                float deviation;
                CircleFit circle;
                if (curve.IsRing && _circleByModule.TryGetValue(curve.Module, out circle))
                {
                    curve.Samples = BuildCircleArc(circle, nodes, halfWidth);
                    deviation = MeasureChordDeviation(curve.Samples);
                }
                else
                {
                    curve.Samples = BuildCurve(nodes, halfWidth, halfWidth, curve.Label, out deviation);
                }
                if (curve.Samples == null)
                {
                    return false;
                }

                curve.ChordDeviationMeters = deviation;
                curve.Curve = new RoadCurve(curve.Samples);
                return Register(curve.Key, RoadRecordKind.Corridor, "voie " + curve.Label);
            }

            private static bool TryFitCircle(List<V1Node> nodes, out CircleFit fit)
            {
                fit = default(CircleFit);
                if (nodes == null || nodes.Count < 3)
                {
                    return false;
                }

                Vector3 origin = nodes[0].Position;
                double aa = 0d;
                double ab = 0d;
                double bb = 0d;
                double ac = 0d;
                double bc = 0d;
                for (int i = 1; i < nodes.Count; i++)
                {
                    double a = 2d * (nodes[i].Position.x - origin.x);
                    double b = 2d * (nodes[i].Position.z - origin.z);
                    double c = nodes[i].Position.x * nodes[i].Position.x + nodes[i].Position.z * nodes[i].Position.z
                        - origin.x * origin.x - origin.z * origin.z;
                    aa += a * a;
                    ab += a * b;
                    bb += b * b;
                    ac += a * c;
                    bc += b * c;
                }

                double determinant = aa * bb - ab * ab;
                if (Math.Abs(determinant) < 1e-9d)
                {
                    return false;
                }

                float cx = (float)((ac * bb - bc * ab) / determinant);
                float cz = (float)((bc * aa - ac * ab) / determinant);
                float y = 0f;
                float radius = 0f;
                Vector3 up = Vector3.zero;
                for (int i = 0; i < nodes.Count; i++)
                {
                    y += nodes[i].Position.y;
                    radius += new Vector2(nodes[i].Position.x - cx, nodes[i].Position.z - cz).magnitude;
                    up += nodes[i].Up;
                }

                fit.Center = new Vector3(cx, y / nodes.Count, cz);
                fit.Radius = radius / nodes.Count;
                fit.Up = up.normalized;
                return fit.Radius > 0f && fit.Up.sqrMagnitude > 0.99f;
            }

            private static RoadCurveSample[] BuildCircleArc(CircleFit circle, List<V1Node> nodes, float halfWidth)
            {
                Vector3 first = (nodes[0].Position - circle.Center).normalized;
                float total = 0f;
                Vector3 previous = first;
                for (int i = 1; i < nodes.Count; i++)
                {
                    Vector3 radial = (nodes[i].Position - circle.Center).normalized;
                    total += Vector3.SignedAngle(previous, radial, circle.Up);
                    previous = radial;
                }

                float sign = Mathf.Sign(total);
                float shift = Mathf.Min(RoundaboutAnchorShiftDegrees, Mathf.Abs(total) * 0.475f);
                Vector3 shiftedFirst = Quaternion.AngleAxis(sign * shift, circle.Up) * first;
                Vector3 last = (nodes[nodes.Count - 1].Position - circle.Center).normalized;
                Vector3 shiftedLast = Quaternion.AngleAxis(-sign * shift, circle.Up) * last;
                return BuildCircleArc(circle,
                    shiftedFirst * circle.Radius + circle.Center,
                    shiftedLast * circle.Radius + circle.Center,
                    sign,
                    halfWidth, halfWidth, halfWidth, halfWidth,
                    Mathf.Abs(total) - 2f * shift);
            }

            private static RoadCurveSample[] BuildCircleArc(
                CircleFit circle,
                Vector3 start,
                Vector3 end,
                float sign,
                float leftStart,
                float leftEnd,
                float rightStart,
                float rightEnd,
                float explicitDegrees = -1f)
            {
                Vector3 radial0 = (start - circle.Center).normalized;
                Vector3 radial1 = (end - circle.Center).normalized;
                float degrees = explicitDegrees >= 0f ? explicitDegrees : Vector3.Angle(radial0, radial1);
                int steps = Mathf.Max(2, Mathf.CeilToInt(degrees / 1f));
                var samples = new RoadCurveSample[steps + 1];
                float length = circle.Radius * degrees * Mathf.Deg2Rad;
                for (int i = 0; i <= steps; i++)
                {
                    float t = (float)i / steps;
                    Vector3 radial = Quaternion.AngleAxis(sign * degrees * t, circle.Up) * radial0;
                    samples[i].SMeters = length * t;
                    samples[i].Position = circle.Center + radial * circle.Radius;
                    samples[i].Tangent = sign * Vector3.Cross(circle.Up, radial).normalized;
                    samples[i].Up = circle.Up;
                    samples[i].CurvaturePerMeter = sign / circle.Radius;
                    samples[i].HalfWidthLeftMeters = Mathf.Lerp(leftStart, leftEnd, t);
                    samples[i].HalfWidthRightMeters = Mathf.Lerp(rightStart, rightEnd, t);
                }

                return samples;
            }

            private static RoadCurveSample[] BuildSmoothCurve(RoadCurvePoint start, RoadCurvePoint end)
            {
                Vector3 chordVector = end.Position - start.Position;
                float chord = chordVector.magnitude;
                float headingRadians = start.SignedHeadingDegrees(end.Tangent) * Mathf.Deg2Rad;
                const int integrationSteps = 256;
                const float residualToleranceMeters = 0.05005f;
                float bestC = 0f;
                float bestAngle = float.PositiveInfinity;
                Vector3 bestNormalizedEnd = Vector3.zero;
                float bestFeasibleCurvature = float.PositiveInfinity;
                float feasibleC = 0f;
                Vector3 feasibleNormalizedEnd = Vector3.zero;
                float admittedMaximumCurvature = 1f
                    / RoadModelCompiler.AdmissionRadiusMeters(DrivabilityProfile());
                float bestAdmittedResidual = float.PositiveInfinity;
                float admittedC = 0f;
                Vector3 admittedNormalizedEnd = Vector3.zero;
                for (int candidate = 0; candidate <= 400; candidate++)
                {
                    float c = Mathf.Lerp(-2f, 2f, candidate / 400f);
                    Vector3 normalizedEnd = IntegrateNormalizedEnd(start, headingRadians, c, integrationSteps);

                    float angle = Vector3.Angle(normalizedEnd, chordVector);
                    if (angle < bestAngle)
                    {
                        bestAngle = angle;
                        bestC = c;
                        bestNormalizedEnd = normalizedEnd;
                    }

                    float candidateLength;
                    float residualMeters;
                    float maximumCurvature;
                    EvaluateSmoothCandidate(start, chordVector, headingRadians, c, normalizedEnd,
                        out candidateLength, out residualMeters, out maximumCurvature);
                    if (candidateLength > 0f && residualMeters <= residualToleranceMeters
                        && maximumCurvature < bestFeasibleCurvature)
                    {
                        bestFeasibleCurvature = maximumCurvature;
                        feasibleC = c;
                        feasibleNormalizedEnd = normalizedEnd;
                    }

                    if (candidateLength > 0f && residualMeters < bestAdmittedResidual
                        && maximumCurvature <= admittedMaximumCurvature)
                    {
                        bestAdmittedResidual = residualMeters;
                        admittedC = c;
                        admittedNormalizedEnd = normalizedEnd;
                    }
                }

                if (!float.IsPositiveInfinity(bestAdmittedResidual)
                    || !float.IsPositiveInfinity(bestFeasibleCurvature))
                {
                    float coarseC = !float.IsPositiveInfinity(bestAdmittedResidual) ? admittedC : feasibleC;
                    for (int candidate = 0; candidate <= 200; candidate++)
                    {
                        float c = coarseC + Mathf.Lerp(-0.01f, 0.01f, candidate / 200f);
                        Vector3 normalizedEnd = IntegrateNormalizedEnd(start, headingRadians, c, integrationSteps);
                        float candidateLength;
                        float residualMeters;
                        float maximumCurvature;
                        EvaluateSmoothCandidate(start, chordVector, headingRadians, c, normalizedEnd,
                            out candidateLength, out residualMeters, out maximumCurvature);
                        if (candidateLength > 0f && residualMeters <= residualToleranceMeters
                            && maximumCurvature < bestFeasibleCurvature)
                        {
                            bestFeasibleCurvature = maximumCurvature;
                            feasibleC = c;
                            feasibleNormalizedEnd = normalizedEnd;
                        }


                        if (candidateLength > 0f && residualMeters < bestAdmittedResidual
                            && maximumCurvature <= admittedMaximumCurvature)
                        {
                            bestAdmittedResidual = residualMeters;
                            admittedC = c;
                            admittedNormalizedEnd = normalizedEnd;
                        }
                    }
                }

                if (!float.IsPositiveInfinity(bestAdmittedResidual))
                {
                    bestC = admittedC;
                    bestNormalizedEnd = admittedNormalizedEnd;
                }
                else if (!float.IsPositiveInfinity(bestFeasibleCurvature))
                {
                    bestC = feasibleC;
                    bestNormalizedEnd = feasibleNormalizedEnd;
                }

                float length = Vector3.Dot(chordVector, bestNormalizedEnd) / bestNormalizedEnd.sqrMagnitude;
                if (!(length > 0f))
                {
                    length = chord;
                }

                const int steps = 256;
                var samples = new RoadCurveSample[steps + 1];
                Vector3[] positions = new Vector3[steps + 1];
                positions[0] = start.Position;
                float ds = length / steps;
                for (int i = 1; i <= steps; i++)
                {
                    float middle = (i - 0.5f) / steps;
                    Vector3 direction = Quaternion.AngleAxis(
                        headingRadians * HeadingFraction(middle, bestC) * Mathf.Rad2Deg,
                        start.Up) * start.Tangent;
                    positions[i] = positions[i - 1] + direction * ds;
                }

                Vector3 residual = end.Position - positions[steps];
                for (int i = 0; i <= steps; i++)
                {
                    float t = (float)i / steps;
                    positions[i] += residual * QuinticSmoothStep(t);
                }

                float s = 0f;
                for (int i = 0; i <= steps; i++)
                {
                    float t = (float)i / steps;
                    Vector3 direction = Quaternion.AngleAxis(
                        headingRadians * HeadingFraction(t, bestC) * Mathf.Rad2Deg,
                        start.Up) * start.Tangent;
                    float headingDerivative = headingRadians * CurvatureShape(t, bestC);
                    Vector3 derivative = length * direction
                        + residual * QuinticSmoothStepDerivative(t);
                    Vector3 secondDerivative = length * headingDerivative
                        * Vector3.Cross(start.Up, direction)
                        + residual * QuinticSmoothStepSecondDerivative(t);
                    if (i > 0)
                    {
                        s += Vector3.Distance(positions[i - 1], positions[i]);
                    }

                    samples[i].SMeters = s;
                    samples[i].Position = positions[i];
                    samples[i].Tangent = derivative.normalized;
                    samples[i].Up = Vector3.Lerp(start.Up, end.Up, t).normalized;
                    samples[i].CurvaturePerMeter = Curvature(derivative, secondDerivative, samples[i].Up);
                    samples[i].HalfWidthLeftMeters = Mathf.Lerp(start.HalfWidthLeftMeters, end.HalfWidthLeftMeters, t);
                    samples[i].HalfWidthRightMeters = Mathf.Lerp(start.HalfWidthRightMeters, end.HalfWidthRightMeters, t);
                }

                samples[0].Position = start.Position;
                samples[0].Tangent = start.Tangent;
                samples[steps].Position = end.Position;
                samples[steps].Tangent = end.Tangent;

                return samples;
            }

            private static float QuinticSmoothStep(float t)
            {
                return t * t * t * (10f + t * (-15f + 6f * t));
            }

            private static float QuinticSmoothStepDerivative(float t)
            {
                return 30f * t * t * (1f - t) * (1f - t);
            }

            private static float QuinticSmoothStepSecondDerivative(float t)
            {
                return 60f * t * (1f - t) * (1f - 2f * t);
            }

            private static Vector3 IntegrateNormalizedEnd(
                RoadCurvePoint start,
                float headingRadians,
                float c,
                int integrationSteps)
            {
                Vector3 normalizedEnd = Vector3.zero;
                for (int i = 0; i < integrationSteps; i++)
                {
                    float t = (i + 0.5f) / integrationSteps;
                    normalizedEnd += Quaternion.AngleAxis(
                        headingRadians * HeadingFraction(t, c) * Mathf.Rad2Deg,
                        start.Up) * start.Tangent / integrationSteps;
                }

                return normalizedEnd;
            }

            private static void EvaluateSmoothCandidate(
                RoadCurvePoint start,
                Vector3 chordVector,
                float headingRadians,
                float c,
                Vector3 normalizedEnd,
                out float length,
                out float residualMeters,
                out float maximumCurvature)
            {
                length = Vector3.Dot(chordVector, normalizedEnd) / normalizedEnd.sqrMagnitude;
                residualMeters = (chordVector - normalizedEnd * length).magnitude;
                Vector3 residual = chordVector - normalizedEnd * length;
                maximumCurvature = 0f;
                for (int sample = 0; sample <= 256; sample++)
                {
                    float t = sample / 256f;
                    Vector3 direction = Quaternion.AngleAxis(
                        headingRadians * HeadingFraction(t, c) * Mathf.Rad2Deg,
                        start.Up) * start.Tangent;
                    float headingDerivative = headingRadians * CurvatureShape(t, c);
                    Vector3 derivative = length * direction
                        + residual * QuinticSmoothStepDerivative(t);
                    Vector3 secondDerivative = length * headingDerivative
                        * Vector3.Cross(start.Up, direction)
                        + residual * QuinticSmoothStepSecondDerivative(t);
                    maximumCurvature = Mathf.Max(maximumCurvature,
                        Mathf.Abs(Curvature(derivative, secondDerivative, start.Up)));
                }

                if (!(length > 0f))
                {
                    maximumCurvature = float.PositiveInfinity;
                }
            }

            private static float HeadingFraction(float t, float c)
            {
                float t2 = t * t;
                float t3 = t2 * t;
                float t4 = t3 * t;
                return 3f * t2 - 2f * t3 + c * (6f * t3 - 3f * t4 - 3f * t2);
            }

            private static float CurvatureShape(float t, float c)
            {
                return 6f * t * (1f - t) * (1f + c * (2f * t - 1f));
            }

            private static float MaximumCurvature(
                RoadCurvePoint start,
                RoadCurvePoint end,
                float startScale,
                float endScale,
                int steps)
            {
                float maximum = 0f;
                for (int i = 0; i <= steps; i++)
                {
                    Vector3 position;
                    Vector3 derivative;
                    Vector3 second;
                    Quintic(start, startScale, end, endScale,
                        (float)i / steps, out position, out derivative, out second);
                    maximum = Mathf.Max(maximum, Mathf.Abs(Curvature(derivative, second, start.Up)));
                }

                return maximum;
            }

            private static void Quintic(
                RoadCurvePoint start,
                float startScale,
                RoadCurvePoint end,
                float endScale,
                float t,
                out Vector3 position,
                out Vector3 derivative,
                out Vector3 second)
            {
                Vector3 p0 = start.Position;
                Vector3 p1 = end.Position;
                Vector3 v0 = start.Tangent * startScale;
                Vector3 v1 = end.Tangent * endScale;
                Vector3 a0 = Vector3.Cross(start.Up, start.Tangent).normalized
                    * start.CurvaturePerMeter * startScale * startScale;
                Vector3 a1 = Vector3.Cross(end.Up, end.Tangent).normalized
                    * end.CurvaturePerMeter * endScale * endScale;
                Vector3 c2 = 0.5f * a0;
                Vector3 d = p1 - p0 - v0 - c2;
                Vector3 v = v1 - v0 - a0;
                Vector3 a = a1 - a0;
                Vector3 c3 = 10f * d - 4f * v + 0.5f * a;
                Vector3 c4 = -15f * d + 7f * v - a;
                Vector3 c5 = 6f * d - 3f * v + 0.5f * a;
                float t2 = t * t;
                float t3 = t2 * t;
                float t4 = t3 * t;
                float t5 = t4 * t;
                position = p0 + v0 * t + c2 * t2 + c3 * t3 + c4 * t4 + c5 * t5;
                derivative = v0 + 2f * c2 * t + 3f * c3 * t2 + 4f * c4 * t3 + 5f * c5 * t4;
                second = 2f * c2 + 6f * c3 * t + 12f * c4 * t2 + 20f * c5 * t3;
            }

            private static float Curvature(Vector3 derivative, Vector3 second, Vector3 up)
            {
                float speed = derivative.magnitude;
                return speed <= 1e-6f ? float.PositiveInfinity
                    : Vector3.Dot(Vector3.Cross(derivative, second), up) / (speed * speed * speed);
            }

            private static float MeasureChordDeviation(RoadCurveSample[] samples)
            {
                float maximum = 0f;
                for (int i = 1; i < samples.Length; i++)
                {
                    float turn = Vector3.Angle(samples[i - 1].Tangent, samples[i].Tangent) * Mathf.Deg2Rad;
                    float curvature = Mathf.Max(Mathf.Abs(samples[i - 1].CurvaturePerMeter), Mathf.Abs(samples[i].CurvaturePerMeter));
                    if (curvature > 1e-6f)
                    {
                        maximum = Mathf.Max(maximum, (1f / curvature) * (1f - Mathf.Cos(turn * 0.5f)));
                    }
                }

                return maximum;
            }

            private static void TrimEnd(ImportedCurve curve, float meters)
            {
                float end = Mathf.Max(curve.Curve.StartS + 0.1f, curve.Curve.Length - meters);
                var kept = new List<RoadCurveSample>();
                for (int i = 0; i < curve.Samples.Length; i++)
                {
                    if (curve.Samples[i].SMeters < end - 1e-4f)
                    {
                        kept.Add(curve.Samples[i]);
                    }
                }

                kept.Add(Sample(curve.Curve.Sample(end), end));
                curve.Samples = kept.ToArray();
                curve.Curve = new RoadCurve(curve.Samples);
            }

            private static void TrimStart(ImportedCurve curve, float meters)
            {
                float start = Mathf.Min(curve.Curve.Length - 0.1f, curve.Curve.StartS + meters);
                var kept = new List<RoadCurveSample> { Sample(curve.Curve.Sample(start), 0f) };
                for (int i = 0; i < curve.Samples.Length; i++)
                {
                    if (curve.Samples[i].SMeters > start + 1e-4f)
                    {
                        RoadCurveSample sample = curve.Samples[i];
                        sample.SMeters -= start;
                        kept.Add(sample);
                    }
                }

                curve.Samples = kept.ToArray();
                curve.Curve = new RoadCurve(curve.Samples);
            }

            private static RoadCurveSample Sample(RoadCurvePoint point, float s)
            {
                var sample = new RoadCurveSample();
                sample.SMeters = s;
                sample.Position = point.Position;
                sample.Tangent = point.Tangent;
                sample.Up = point.Up;
                sample.CurvaturePerMeter = point.CurvaturePerMeter;
                sample.HalfWidthLeftMeters = point.HalfWidthLeftMeters;
                sample.HalfWidthRightMeters = point.HalfWidthRightMeters;
                return sample;
            }

            private bool Register(string key, RoadRecordKind kind, string sourceDescription)
            {
                return _registry.TryRegister(key, kind, sourceDescription, _failures);
            }

            private void DisposeNode(V1Node node, DispositionKind kind, string targetKey, string detail)
            {
                if (_nodeDisposition.ContainsKey(node))
                {
                    Fail("Noeud '" + node.Label + "' dispose deux fois.");
                    return;
                }

                var disposition = Add(SourceItemKind.Node, node.Key, node.Module.Label + " / " + node.Label + " (" + node.Role + ")", kind, targetKey, detail);
                _nodeDisposition.Add(node, disposition);
            }

            private void DisposeEdge(V1Edge edge, DispositionKind kind, string targetKey, string detail)
            {
                if (edge == null)
                {
                    Fail("Arete interieure introuvable pour " + targetKey + ".");
                    return;
                }

                if (_edgeDisposition.ContainsKey(edge))
                {
                    Fail("Arete '" + edge.From.Label + "' -> '" + edge.To.Label + "' disposee deux fois.");
                    return;
                }

                var disposition = Add(SourceItemKind.Edge, edge.Key,
                    edge.From.Label + " -> " + edge.To.Label + (edge.IsConnectorJoin ? " (jointure)" : string.Empty), kind, targetKey, detail);
                _edgeDisposition.Add(edge, disposition);
            }

            private SourceDisposition Add(SourceItemKind item, string sourceKey, string sourceLabel, DispositionKind kind, string targetKey, string detail)
            {
                var disposition = new SourceDisposition();
                disposition.Item = item;
                disposition.SourceKey = sourceKey;
                disposition.SourceLabel = sourceLabel;
                disposition.Kind = kind;
                disposition.TargetKey = targetKey;
                disposition.Detail = detail;
                _result.Dispositions.Add(disposition);
                return disposition;
            }

            private void Fail(string message)
            {
                _failures.Add(message);
            }
        }
    }

    /// <summary>Formats numeriques invariants et a precision fixe (rapport et details de disposition).</summary>
    public static class MigrationFormat
    {
        public static string Meters(float value)
        {
            return Fixed(value, "F4");
        }

        public static string Degrees(float value)
        {
            return Fixed(value, "F3");
        }

        public static string Weight(float value)
        {
            return Fixed(value, "F4");
        }

        private static string Fixed(float value, string format)
        {
            string text = ((double)value).ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            // Normalise -0 : un signe sans valeur ne doit pas changer les octets du rapport.
            return text.StartsWith("-", StringComparison.Ordinal) && text.Trim('-', '0', '.').Length == 0 ? text.Substring(1) : text;
        }
    }
}
#endif
