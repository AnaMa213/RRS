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
        public const int ImporterVersion = 1;

        /// <summary>Tolerance de corde du contrat (0,05 m) passee au constructeur de courbe 5.26.</summary>
        public const float ChordToleranceMeters = 0.05f;

        /// <summary>Pas des points de controle d'un mouvement Hermite, en metres.</summary>
        private const float MovementControlStepMeters = 0.25f;

        /// <summary>Pas angulaire maximal entre deux points de controle d'un mouvement, en degres.</summary>
        private const float MovementControlStepDegrees = 1f;

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

                // Hermite cubique dense : tangentes d'extremite = tangentes des corridors, echelle =
                // corde. Le constructeur 5.26 passe ensuite par ces points.
                Vector3 m0 = fromEnd.Tangent * chord;
                Vector3 m1 = toStart.Tangent * chord;
                // Densite : au plus un pas de longueur ET un pas d'angle. Le constructeur 5.26 estime
                // la tangente d'extremite sur les derniers points : sur un virage serre, un pas
                // purement metrique laissait ~13 degres d'ecart de couture (mesure du premier rapport).
                float estimate = 0f;
                float turn = 0f;
                Vector3 previous = p0;
                Vector3 previousDirection = fromEnd.Tangent;
                for (int k = 1; k <= 64; k++)
                {
                    Vector3 p = Hermite(p0, m0, p1, m1, k / 64f);
                    Vector3 step = p - previous;
                    estimate += step.magnitude;
                    if (step.sqrMagnitude > 1e-12f)
                    {
                        turn += Vector3.Angle(previousDirection, step);
                        previousDirection = step;
                    }

                    previous = p;
                }

                turn += Vector3.Angle(previousDirection, toStart.Tangent);
                int steps = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(estimate / MovementControlStepMeters, turn / MovementControlStepDegrees)), 8, 512);
                var positions = new Vector3[steps + 1];
                var ups = new Vector3[steps + 1];
                var left = new float[steps + 1];
                var right = new float[steps + 1];
                for (int k = 0; k <= steps; k++)
                {
                    float u = (float)k / steps;
                    positions[k] = Hermite(p0, m0, p1, m1, u);
                    ups[k] = Vector3.Lerp(fromEnd.Up, toStart.Up, u).normalized;
                    left[k] = Mathf.Lerp(fromEnd.HalfWidthLeftMeters, toStart.HalfWidthLeftMeters, u);
                    right[k] = Mathf.Lerp(fromEnd.HalfWidthRightMeters, toStart.HalfWidthRightMeters, u);
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

                try
                {
                    float deviation;
                    movement.Samples = RoadCurveBuilder.Build(positions, ups, left, right, ChordToleranceMeters, out deviation);
                    movement.ChordDeviationMeters = deviation;
                    movement.Curve = new RoadCurve(movement.Samples);
                }
                catch (ArgumentException exception)
                {
                    Fail("Courbe du mouvement " + movement.Label + " : " + exception.Message);
                    return false;
                }

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

            private static Vector3 Hermite(Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1, float u)
            {
                float u2 = u * u;
                float u3 = u2 * u;
                return (2f * u3 - 3f * u2 + 1f) * p0 + (u3 - 2f * u2 + u) * m0 + (-2f * u3 + 3f * u2) * p1 + (u3 - u2) * m1;
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
                curve.Samples = BuildCurve(nodes, halfWidth, halfWidth, curve.Label, out deviation);
                if (curve.Samples == null)
                {
                    return false;
                }

                curve.ChordDeviationMeters = deviation;
                curve.Curve = new RoadCurve(curve.Samples);
                return Register(curve.Key, RoadRecordKind.Corridor, "voie " + curve.Label);
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
