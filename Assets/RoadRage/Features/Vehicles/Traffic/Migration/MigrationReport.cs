#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    // =====================================================================================
    // Story 5.27 -- mesures, validation et rapport de migration machine.
    //
    // Le rapport est lie a sa source (AD-47) : hash de la source V1 extraite, version de
    // l'importeur, CompilerSchemaVersion, ModelId, hash de la lignee et hash de son propre corps.
    // Un rapport perime ou retouche a la main est REJETE par Verify, jamais repare. Tant que le
    // modele ne passe pas Compile, aucune RoadModelVersion n'existe et le rapport le dit : il
    // n'en fabrique aucun equivalent (5.26).
    // =====================================================================================

    public enum MeasureClass
    {
        Within = 0,
        DeviationToCorrect = 1,
        JustifiedException = 2
    }

    public struct MeasureValue
    {
        public string Subject;
        public float Value;
        public MeasureClass Class;
    }

    public sealed class Metric
    {
        public string Title;
        public string Unit;
        public string Threshold;
        public string Origin;
        public readonly List<MeasureValue> Values = new List<MeasureValue>();

        public int Count(MeasureClass measureClass)
        {
            int count = 0;
            foreach (var value in Values)
            {
                count += value.Class == measureClass ? 1 : 0;
            }

            return count;
        }
    }

    /// <summary>Champs de liaison d'un rapport a sa source.</summary>
    public sealed class MigrationBinding
    {
        public const string Header = "<!-- rrs-migration-binding";

        public string SourceHash;
        public int ImporterVersion;
        public int CompilerSchemaVersion;
        public string ModelId;
        public string LineageHash;
        public string BodyHash;
    }

    /// <summary>Un passage complet : extraction, import, mesures, rapport. Rien n'est ecrit ici.</summary>
    public sealed class MigrationRun
    {
        public V1SourceSet SourceSet;
        public V1ImportResult Import;
        public readonly List<string> Failures = new List<string>();
        public readonly List<Metric> Metrics = new List<Metric>();
        public readonly List<RoadModelValidationIssue> StructuralIssues = new List<RoadModelValidationIssue>();
        public readonly List<RoadModelValidationIssue> GeometricIssues = new List<RoadModelValidationIssue>();
        public CompiledRoadModel Compiled;
        public string LineageJson;
        public string ReportText;
        public MigrationBinding Binding;

        /// <summary>Portee : pour chaque portail d'entree, les portails de sortie atteints.</summary>
        public readonly SortedDictionary<string, List<string>> Reachability = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

        public bool Succeeded
        {
            get { return Failures.Count == 0; }
        }

        public bool EveryEntryReachesAnExit
        {
            get
            {
                foreach (var pair in Reachability)
                {
                    if (pair.Value.Count == 0)
                    {
                        return false;
                    }
                }

                return Reachability.Count > 0;
            }
        }
    }

    public static class MigrationReport
    {
        public const string ScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        public const string LineagePath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-lineage.json";
        public const string ReportPath = "_bmad-output/implementation-artifacts/migration-report-5-27-mvp-run.md";

        /// <summary>Chemin absolu de la lignee committée, resolu depuis la racine du projet.</summary>
        public static string LineageFullPath
        {
            get { return Path.Combine(Directory.GetParent(Application.dataPath).FullName, LineagePath); }
        }

        /// <summary>Chemin absolu du rapport committé, resolu depuis la racine du projet.</summary>
        public static string ReportFullPath
        {
            get { return Path.Combine(Directory.GetParent(Application.dataPath).FullName, ReportPath); }
        }

        // ---------------------------------------------------------------- seuils publies tels quels
        private const float ConnectorAngleGate = 90f;
        /// <summary>Derive maximale admise entre un noeud source et la courbe qui le porte, en metres.</summary>
        private const float NodeDriftGate = 0.10f;

        /// <summary>
        /// Borne d'ecart hors axe admise pour l'exception approuvee (graine developpee par un virage).
        /// Meme valeur que <see cref="NodeDriftGate" /> aujourd'hui, mais c'est une autre decision :
        /// deux criteres distincts ne doivent pas partager une constante. Garde de cout.
        /// </summary>
        private const float SeedOffAxisGate = 0.10f;
        private const float PortalDriftGate = 0.05f;

        /// <summary>Passage complet en memoire. Echec (source, import, lignee) = aucun texte produit.</summary>
        public static MigrationRun Run(Scene scene, string priorLineageJson)
        {
            return Run(V1SourceSet.Extract(scene), priorLineageJson);
        }

        public static MigrationRun Run(V1SourceSet set, string priorLineageJson)
        {
            var run = new MigrationRun();
            run.SourceSet = set;

            RoadLineage prior;
            try
            {
                prior = RoadLineage.Parse(priorLineageJson);
            }
            catch (FormatException exception)
            {
                run.Failures.Add(exception.Message);
                return run;
            }

            run.Import = V1RoadModelImporter.Import(set, prior);
            if (!run.Import.Succeeded)
            {
                run.Failures.AddRange(run.Import.Failures);
                return run;
            }

            Validate(run);
            Measure(run);
            ComputeReachability(run);

            run.LineageJson = run.Import.Lineage.Next.Serialize();
            var binding = new MigrationBinding();
            binding.SourceHash = set.SourceHash;
            binding.ImporterVersion = V1RoadModelImporter.ImporterVersion;
            binding.CompilerSchemaVersion = RoadModelCompiler.CompilerSchemaVersion;
            binding.ModelId = run.Import.Source.ModelId.ToString();
            binding.LineageHash = V1SourceSet.Sha256Hex(run.LineageJson);

            string body = RenderBody(run, binding);
            binding.BodyHash = V1SourceSet.Sha256Hex(body);
            run.Binding = binding;
            run.ReportText = RenderHeader(binding) + body;
            return run;
        }

        // ============================================================ validation

        private static void Validate(MigrationRun run)
        {
            run.Compiled = ValidateSource(run.Import.Source, run.StructuralIssues, run.GeometricIssues);
        }

        /// <summary>
        /// Classe les echecs d'une source sans jamais les compter deux fois. <c>Validate</c> ne lance
        /// la geometrie que sur une source structurellement propre : si son resultat ne porte que
        /// des codes geometriques, la geometrie a deja tourne ; sinon on la lance ici pour la
        /// LISTER. Seul <c>Compile</c> emet une version, et seulement sans aucun echec.
        /// </summary>
        public static CompiledRoadModel ValidateSource(RoadModelSource source, List<RoadModelValidationIssue> structural, List<RoadModelValidationIssue> geometric)
        {
            var issues = RoadModelValidator.Validate(source);
            CompiledRoadModel compiled = null;
            if (issues.Count == 0)
            {
                compiled = RoadModelCompiler.Compile(source);
            }
            else if (AllGeometric(issues))
            {
                geometric.AddRange(issues);
            }
            else
            {
                structural.AddRange(issues);
                RoadGeometryValidator.Validate(source, geometric);
            }

            SortIssues(structural);
            SortIssues(geometric);
            return compiled;
        }

        /// <summary>Codes emis par le seul validateur geometrique, et eux seuls.</summary>
        private static bool AllGeometric(IReadOnlyList<RoadModelValidationIssue> issues)
        {
            foreach (var issue in issues)
            {
                if (!IsGeometricCode(issue.Code))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Le validateur geometrique (Story 5.26) emet les codes a partir de 19, avec une exception :
        /// `ProfileToleranceAboveApprovedCeiling` (31) vit dans le validateur semantique. Un code
        /// numerote par erreur sous 19, ou une nouvelle valeur d'enum non classee ici, ferait tomber la
        /// source dans la branche mixte. C'est ce que pin `TheValidationSplitNamesTheGeometricCodesOnly`.
        /// </summary>
        private static bool IsGeometricCode(RoadModelValidationCode code)
        {
            if (code < RoadModelValidationCode.NonOrthonormalFrame)
            {
                return false;
            }

            return code != RoadModelValidationCode.ProfileToleranceAboveApprovedCeiling;
        }

        private static void SortIssues(List<RoadModelValidationIssue> issues)
        {
            issues.Sort(delegate(RoadModelValidationIssue a, RoadModelValidationIssue b)
            {
                int byCode = a.Code.CompareTo(b.Code);
                if (byCode != 0)
                {
                    return byCode;
                }

                int bySubject = a.SubjectId.CompareTo(b.SubjectId);
                return bySubject != 0 ? bySubject : string.CompareOrdinal(a.Message, b.Message);
            });
        }

        // ============================================================ mesures

        private static void Measure(MigrationRun run)
        {
            var import = run.Import;
            var profile = import.Source.ValidationProfile;

            var gapGate = import.SourceSet.ConnectorJoinDistanceMeters;
            var gap = NewMetric(run, "Ecart de connecteur (source V1)", "m", "<= " + MigrationFormat.Meters(gapGate),
                "herite de V1 : decouverte de jointure (TrafficSettingsDef.connectorJoinDistance, lue dans la source)");
            var angle = NewMetric(run, "Angle de connecteur (source V1)", "deg", "< " + MigrationFormat.Degrees(ConnectorAngleGate),
                "herite de V1 : test Dot > 0");
            foreach (var join in import.SourceSet.Joins)
            {
                string subject = join.From.Module.Label + " / " + join.From.Label + " -> " + join.To.Module.Label + " / " + join.To.Label;
                Add(gap, subject, join.GapMeters, join.GapMeters <= gapGate);
                Add(angle, subject, join.AngleDegrees, join.AngleDegrees < ConnectorAngleGate);
            }

            var seamGap = NewMetric(run, "Couture : ecart de position", "m", "<= " + MigrationFormat.Meters(profile.SeamGapToleranceMeters),
                "cible V2 proposee, jamais mesuree avant ce rapport");
            var seamTangent = NewMetric(run, "Couture : ecart de tangente", "deg", "<= " + MigrationFormat.Degrees(profile.SeamTangentToleranceDegrees),
                "cible V2 proposee, jamais mesuree avant ce rapport");
            var seamWidth = NewMetric(run, "Couture de mouvement : ecart de demi-largeur", "m", "<= " + MigrationFormat.Meters(profile.SeamGapToleranceMeters),
                "cible V2 proposee (meme tolerance que la position)");
            foreach (var movement in import.Movements)
            {
                Seam(seamGap, seamTangent, seamWidth, movement.Label + " [debut]", movement.From.Curve.Sample(movement.From.Curve.Length), movement.Curve.Sample(0f), profile);
                Seam(seamGap, seamTangent, seamWidth, movement.Label + " [fin]", movement.Curve.Sample(movement.Curve.Length), movement.To.Curve.Sample(0f), profile);
            }

            foreach (var connection in import.Connections)
            {
                Seam(seamGap, seamTangent, null, connection.From.Label + " -> " + connection.To.Label,
                    connection.From.Curve.Sample(connection.From.Curve.Length), connection.To.Curve.Sample(0f), profile);
            }

            var chord = NewMetric(run, "Corde compilee (critere de subdivision 5.26)", "m", "<= " + MigrationFormat.Meters(V1RoadModelImporter.ChordToleranceMeters),
                "cible V2 proposee ; mesure = ecart spline-corde, borne par construction a la moitie ; l'ecart spline-intention n'est pas mesure");
            var clearance = NewMetric(run, "Degagement lateral (demi-largeur - demi-gabarit - marge)", "m", ">= 0",
                "cible V2 proposee : demi-gabarit " + MigrationFormat.Meters(profile.MaxVehicleHalfWidthMeters) + " + marge " + MigrationFormat.Meters(profile.LateralClearanceMarginMeters));
            var curves = new List<ImportedCurve>(import.Corridors);
            curves.AddRange(import.Movements);
            foreach (var curve in curves)
            {
                Add(chord, curve.Label, curve.ChordDeviationMeters, curve.ChordDeviationMeters <= V1RoadModelImporter.ChordToleranceMeters);
                float worst = float.PositiveInfinity;
                foreach (var sample in curve.Samples)
                {
                    worst = Mathf.Min(worst, Mathf.Min(sample.HalfWidthLeftMeters, sample.HalfWidthRightMeters));
                }

                float margin = worst - profile.MaxVehicleHalfWidthMeters - profile.LateralClearanceMarginMeters;
                Add(clearance, curve.Label, margin, margin >= 0f);
            }

            var drift = NewMetric(run, "Derive de noeud source vers la courbe", "m", "<= " + MigrationFormat.Meters(NodeDriftGate),
                "cible V2 proposee ; seule exception approuvee : noeud de decision lisse par un mouvement tournant, "
                + "et seulement si la graine reste sur l'axe de son approche (ecart <= " + MigrationFormat.Meters(NodeDriftGate) + " m) ; sinon deviation a corriger");
            foreach (var corridor in import.Corridors)
            {
                foreach (var node in corridor.VertexNodes)
                {
                    float value = corridor.Curve.Project(node.Position).DistanceMeters;
                    Add(drift, node.Module.Label + " / " + node.Label + " -> " + corridor.Label, value, value <= NodeDriftGate);
                }

                foreach (var node in corridor.MergedNodes)
                {
                    float value = corridor.Curve.Project(node.Position).DistanceMeters;
                    Add(drift, node.Module.Label + " / " + node.Label + " (fusionne) -> " + corridor.Label, value, value <= NodeDriftGate);
                }
            }

            foreach (var movement in import.Movements)
            {
                if (movement.Seed == null)
                {
                    continue;
                }

                float value = movement.Curve.Project(movement.Seed.Position).DistanceMeters;
                var measure = new MeasureValue();
                measure.Subject = movement.Seed.Module.Label + " / " + movement.Seed.Label + " (graine) -> " + movement.Label;
                measure.Value = value;
                // Le lissage n'excuse que l'ecart DU au virage : une graine deplacee hors de l'axe de
                // son approche reste une deviation, quel que soit le virage (borne de l'exception).
                var approachEnd = movement.From.Curve.Sample(movement.From.Curve.Length);
                Vector3 offset = movement.Seed.Position - approachEnd.Position;
                float along = Vector3.Dot(offset, approachEnd.Tangent);
                float offAxis = (offset - approachEnd.Tangent * along).magnitude;
                bool onApproachAxis = along >= 0f && offAxis <= SeedOffAxisGate;
                measure.Class = value <= NodeDriftGate ? MeasureClass.Within
                    : Mathf.Abs(movement.TurnDegrees) >= V1RoadModelImporter.TurningThresholdDegrees && onApproachAxis ? MeasureClass.JustifiedException
                    : MeasureClass.DeviationToCorrect;
                drift.Values.Add(measure);
            }

            var portalDrift = NewMetric(run, "Derive de portail", "m", "<= " + MigrationFormat.Meters(PortalDriftGate),
                "cible V2 proposee, jamais mesuree avant ce rapport");
            foreach (var portal in import.Portals)
            {
                float value = (portal.Node.Position - portal.Corridor.Curve.Sample(portal.SMeters).Position).magnitude;
                Add(portalDrift, portal.Node.Module.Label + " / " + portal.Node.Label + " (" + portal.Role + ")", value, value <= PortalDriftGate);
            }
        }

        private static void Seam(Metric gap, Metric tangent, Metric width, string subject, RoadCurvePoint a, RoadCurvePoint b, RoadModelValidationProfile profile)
        {
            float position = (a.Position - b.Position).magnitude;
            float angle = Vector3.Angle(a.Tangent, b.Tangent);
            Add(gap, subject, position, position <= profile.SeamGapToleranceMeters);
            Add(tangent, subject, angle, angle <= profile.SeamTangentToleranceDegrees);
            if (width != null)
            {
                float delta = Mathf.Max(Mathf.Abs(a.HalfWidthLeftMeters - b.HalfWidthLeftMeters), Mathf.Abs(a.HalfWidthRightMeters - b.HalfWidthRightMeters));
                Add(width, subject, delta, delta <= profile.SeamGapToleranceMeters);
            }
        }

        private static Metric NewMetric(MigrationRun run, string title, string unit, string threshold, string origin)
        {
            var metric = new Metric();
            metric.Title = title;
            metric.Unit = unit;
            metric.Threshold = threshold;
            metric.Origin = origin;
            run.Metrics.Add(metric);
            return metric;
        }

        private static void Add(Metric metric, string subject, float value, bool within)
        {
            var measure = new MeasureValue();
            measure.Subject = subject;
            measure.Value = value;
            measure.Class = within ? MeasureClass.Within : MeasureClass.DeviationToCorrect;
            metric.Values.Add(measure);
        }

        // ============================================================ portee

        /// <summary>
        /// Parcours de la topologie V2 (connexions et mouvements), voisins en ordre de RoadId :
        /// jamais l'ordre de hierarchie. Contrat (AD-47, garde V1) : chaque entree atteint au
        /// moins une sortie ; la matrice complete est publiee sans etre un gate.
        /// </summary>
        private static void ComputeReachability(MigrationRun run)
        {
            foreach (var pair in ComputeReachability(run.Import.Source))
            {
                run.Reachability.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>Portee d'une source : pour chaque portail d'entree, les portails de sortie atteints.</summary>
        public static SortedDictionary<string, List<string>> ComputeReachability(RoadModelSource source)
        {
            var reachability = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
            var next = new Dictionary<RoadId, List<RoadId>>();
            foreach (var connection in source.Connections)
            {
                Link(next, connection.FromCorridorId, connection.ToCorridorId);
            }

            foreach (var movement in source.Movements)
            {
                Link(next, movement.FromCorridorId, movement.ToCorridorId);
            }

            foreach (var list in next.Values)
            {
                list.Sort();
            }

            foreach (var entry in source.Portals)
            {
                if (entry.Role != PortalRole.Entry)
                {
                    continue;
                }

                var reached = new HashSet<RoadId>();
                var queue = new Queue<RoadId>();
                List<RoadId> first;
                if (next.TryGetValue(entry.CorridorId, out first))
                {
                    foreach (var id in first)
                    {
                        if (reached.Add(id))
                        {
                            queue.Enqueue(id);
                        }
                    }
                }

                while (queue.Count > 0)
                {
                    List<RoadId> successors;
                    if (next.TryGetValue(queue.Dequeue(), out successors))
                    {
                        foreach (var id in successors)
                        {
                            if (reached.Add(id))
                            {
                                queue.Enqueue(id);
                            }
                        }
                    }
                }

                var exits = new List<string>();
                foreach (var exit in source.Portals)
                {
                    // Strictement en avant : un portail d'entree reutilise en sortie (meme s) ne
                    // s'atteint pas lui-meme, sinon le contrat passerait a vide.
                    bool sameCorridorAhead = exit.CorridorId == entry.CorridorId && exit.SMeters > entry.SMeters;
                    if (exit.Role == PortalRole.Exit && (reached.Contains(exit.CorridorId) || sameCorridorAhead))
                    {
                        exits.Add(exit.Label + " " + exit.Id);
                    }
                }

                exits.Sort(StringComparer.Ordinal);
                reachability[entry.Label + " " + entry.Id] = exits;
            }

            return reachability;
        }

        private static void Link(Dictionary<RoadId, List<RoadId>> next, RoadId from, RoadId to)
        {
            List<RoadId> list;
            if (!next.TryGetValue(from, out list))
            {
                list = new List<RoadId>();
                next.Add(from, list);
            }

            if (!list.Contains(to))
            {
                list.Add(to);
            }
        }

        // ============================================================ rendu

        private static string RenderHeader(MigrationBinding binding)
        {
            var text = new StringBuilder();
            text.Append(MigrationBinding.Header).Append('\n');
            text.Append("source-hash: ").Append(binding.SourceHash).Append('\n');
            text.Append("importer-version: ").Append(binding.ImporterVersion).Append('\n');
            text.Append("compiler-schema-version: ").Append(binding.CompilerSchemaVersion).Append('\n');
            text.Append("model-id: ").Append(binding.ModelId).Append('\n');
            text.Append("lineage-hash: ").Append(binding.LineageHash).Append('\n');
            text.Append("body-hash: ").Append(binding.BodyHash).Append('\n');
            text.Append("-->\n");
            return text.ToString();
        }

        private static string RenderBody(MigrationRun run, MigrationBinding binding)
        {
            var import = run.Import;
            var source = import.Source;
            var set = import.SourceSet;
            var text = new StringBuilder();

            text.Append("# Rapport de migration V1 -> V2 : MVP_Run (Story 5.27)\n\n");
            text.Append("Genere par le menu `RoadRage/Traffic V2/Migrer MVP_Run`. Ne pas editer : un rapport retouche ou detache de sa source est rejete, jamais repare.\n\n");

            // ------------------------------------------------ liaison
            text.Append("## Liaison\n\n| Champ | Valeur |\n|---|---|\n");
            Row(text, "Hash de la source V1 extraite", "`" + binding.SourceHash + "`");
            Row(text, "Version de l'importeur", binding.ImporterVersion.ToString());
            Row(text, "CompilerSchemaVersion", binding.CompilerSchemaVersion.ToString());
            Row(text, "RoadModelId", "`" + binding.ModelId + "`");
            Row(text, "Hash de la lignee", "`" + binding.LineageHash + "` (`" + LineagePath + "`)");
            Row(text, "RoadModelVersion", run.Compiled != null
                ? "`" + run.Compiled.Version + "`"
                : "absente -- Compile refuse : " + (run.StructuralIssues.Count + run.GeometricIssues.Count) + " erreurs (" + CountByCode(run) + ")");
            text.Append('\n');

            // ------------------------------------------------ ensemble source
            text.Append("## Ensemble source\n\n");
            text.Append("Ensemble prouve : chaque LaneNode appartient a une instance d'un prefab reconnu, sous l'unique LaneGraph. Seuil de jointure V1 : ")
                .Append(MigrationFormat.Meters(set.ConnectorJoinDistanceMeters)).Append(" m.\n\n");
            text.Append("| Prefab reconnu | Instances | Noeuds |\n|---|---:|---:|\n");
            var prefabs = new List<string>(V1SourceSet.RecognisedPrefabs.Keys);
            prefabs.Sort(StringComparer.Ordinal);
            int modules = 0;
            foreach (var prefab in prefabs)
            {
                int instances = 0;
                int nodes = 0;
                foreach (var module in set.Modules)
                {
                    if (module.PrefabPath == prefab)
                    {
                        instances++;
                        nodes += module.Nodes.Count;
                    }
                }

                modules += instances;
                text.Append("| `").Append(prefab).Append("` | ").Append(instances).Append(" | ").Append(nodes).Append(" |\n");
            }

            text.Append("| **Total** | **").Append(modules).Append("** | **").Append(set.Nodes.Count).Append("** |\n\n");
            text.Append("Aretes authorees : ").Append(set.Edges.Count - set.Joins.Count).Append(" ; jointures de connecteurs : ").Append(set.Joins.Count)
                .Append(" ; noeuds hors module : aucun (un noeud hors module fait echouer l'extraction).\n\n");

            // ------------------------------------------------ modele candidat
            text.Append("## Modele candidat\n\n| Enregistrement | Nombre |\n|---|---:|\n");
            Row(text, "RoadSection", source.Sections.Length.ToString());
            Row(text, "LaneCorridor", source.Corridors.Length.ToString());
            Row(text, "LaneConnection", source.Connections.Length.ToString());
            Row(text, "LaneAdjacency", source.Adjacencies.Length + " (aucune adjacence de meme sens dans la source)");
            Row(text, "Junction", source.Junctions.Length.ToString());
            Row(text, "JunctionMovement", source.Movements.Length.ToString());
            Row(text, "JunctionControl", source.Controls.Length + " (jamais inventes)");
            Row(text, "ConflictZone", source.ConflictZones.Length + " (jamais inventees)");
            Row(text, "SignalPlan", source.SignalPlans.Length + " (aucun signal en V1)");
            Row(text, "Portal", source.Portals.Length.ToString());
            text.Append('\n');

            // ------------------------------------------------ lignee
            var lineage = import.Lineage;
            // ------------------------------------------------ coupes transversales (AD-48)
            // AD-48 exige de la 5.27 qu'elle renseigne ET dispose `LateralOrder` et le datum de chaque
            // section dans le rapport : un datum arbitraire qui n'y apparait pas est un echec admis.
            text.Append("## Coupes transversales (AD-48)\n\n");
            text.Append("Ordre lateral et datum de chaque section du modele candidat, tels qu'importes (V1 n'en porte aucun) : AD-48 exige que la 5.27 dispose les deux explicitement.\n\n");
            text.Append("| Section | `LateralOrder` | Corridor | Datum de coupe |\n|---|---:|---|---|\n");
            var sections = new List<RoadSection>(import.Source.Sections);
            sections.Sort(delegate(RoadSection a, RoadSection b) { return a.Id.CompareTo(b.Id); });
            foreach (var section in sections)
            {
                var members = new List<LaneCorridor>();
                foreach (var corridor in import.Source.Corridors)
                {
                    if (corridor.SectionId == section.Id)
                    {
                        members.Add(corridor);
                    }
                }

                members.Sort(delegate(LaneCorridor a, LaneCorridor b) { return a.LateralOrder.CompareTo(b.LateralOrder); });
                foreach (var corridor in members)
                {
                    text.Append("| `").Append(section.Id).Append("` | ").Append(corridor.LateralOrder).Append(" | `").Append(corridor.Id).Append("` | ")
                        .Append(corridor.IsCrossSectionDatum ? "oui" : "-").Append(" |\n");
                }
            }

            text.Append('\n');

            text.Append("## Lignee et identites\n\n");
            text.Append("- Identites preservees : ").Append(lineage.Preserved.Count).Append('\n');
            text.Append("- Identites frappees (nouvelles) : ").Append(lineage.Minted.Count).Append(lineage.ModelIdMinted ? " (premier import : RoadModelId frappe)" : string.Empty).Append('\n');
            text.Append("- Entites retirees (tombstonees, jamais recyclees) : ").Append(lineage.Retired.Count).Append('\n');
            text.Append("- Tombstones cumules : ").Append(lineage.Next.Tombstones.Count).Append("\n\n");
            if (lineage.Minted.Count > 0 && !lineage.ModelIdMinted)
            {
                text.Append("| Nouvelle entite | Identite |\n|---|---|\n");
                var minted = new List<string>(lineage.Minted);
                minted.Sort(StringComparer.Ordinal);
                foreach (var key in minted)
                {
                    Row(text, "`" + key + "`", "`" + import.IdOf(key) + "`");
                }

                text.Append('\n');
            }

            if (lineage.Retired.Count > 0)
            {
                text.Append("| Entite retiree | Genre | Identite tombstonee |\n|---|---|---|\n");
                var retired = new List<RoadLineage.Entry>(lineage.Retired);
                retired.Sort(delegate(RoadLineage.Entry a, RoadLineage.Entry b) { return string.CompareOrdinal(a.Key, b.Key); });
                foreach (var entry in retired)
                {
                    text.Append("| `").Append(entry.Key).Append("` | ").Append(entry.Kind).Append(" | `").Append(entry.Id).Append("` |\n");
                }

                text.Append('\n');
            }

            // ------------------------------------------------ validation
            text.Append("## Validation\n\n");
            if (run.Compiled != null)
            {
                text.Append("Le modele passe `Compile` : zero erreur dure.\n\n");
            }
            else
            {
                text.Append("**Le modele ne passe pas encore la validation.** `Compile` le refuse, donc aucune `RoadModelVersion` n'existe. ")
                    .Append("Les erreurs structurelles viennent de `RoadModelValidator.Validate` ; la geometrie, que `Validate` ne lance pas sur une source structurellement invalide, est listee a part.\n\n");
                IssueTable(text, "Structurelles", run.StructuralIssues);
                IssueTable(text, "Geometriques", run.GeometricIssues);
            }

            // ------------------------------------------------ mesures
            text.Append("## Mesures geometriques\n\n");
            text.Append("Valeurs mesurees sur la source et le modele candidat. Aucun seuil n'est relache ici : un depassement est une deviation a corriger, sauf categorie d'exception approuvee par le spec.\n\n");
            text.Append("| Mesure | Unite | n | min | p50 | p95 | max | Seuil | Dans le seuil | Deviations | Exceptions |\n|---|---|---:|---:|---:|---:|---:|---|---:|---:|---:|\n");
            foreach (var metric in run.Metrics)
            {
                // Les statistiques portent sur la population DANS le seuil : melanger a la population
                // nominale les exceptions approuvees (lissage des noeuds de decision) produit un p95 qui
                // ne decrit ni l'une ni l'autre. Deviations et exceptions sont listees plus bas, chacune
                // avec sa valeur propre et sa deviation max.
                var values = new List<float>();
                foreach (var value in metric.Values)
                {
                    if (value.Class == MeasureClass.Within)
                    {
                        values.Add(value.Value);
                    }
                }

                values.Sort();
                text.Append("| ").Append(metric.Title).Append(" | ").Append(metric.Unit).Append(" | ").Append(values.Count)
                    .Append(" | ").Append(Stat(values, 0f, metric)).Append(" | ").Append(Stat(values, 0.5f, metric))
                    .Append(" | ").Append(Stat(values, 0.95f, metric)).Append(" | ").Append(Stat(values, 1f, metric))
                    .Append(" | ").Append(metric.Threshold).Append(" | ").Append(metric.Count(MeasureClass.Within))
                    .Append(" | ").Append(metric.Count(MeasureClass.DeviationToCorrect)).Append(" | ").Append(metric.Count(MeasureClass.JustifiedException)).Append(" |\n");
            }

            text.Append("\nOrigine des seuils :\n\n");
            foreach (var metric in run.Metrics)
            {
                text.Append("- ").Append(metric.Title).Append(" : ").Append(metric.Origin).Append(".\n");
            }

            text.Append("\nLes colonnes min, p50, p95 et max portent sur la population dans le seuil (`n`) ; ")
                .Append("les valeurs hors seuil sont listees ci-dessous avec leur classe, jamais fondues dans ces statistiques.\n");

            text.Append('\n');
            MeasureList(text, run, MeasureClass.DeviationToCorrect, "Deviations a corriger (authoring)");
            MeasureList(text, run, MeasureClass.JustifiedException, "Exceptions justifiees (lissage des noeuds de decision par les virages)");

            // ------------------------------------------------ portee
            text.Append("## Portee entrees -> sorties\n\n");
            text.Append("Contrat (AD-47, garde V1 `Story510`) : chaque portail d'entree atteint au moins un portail de sortie. Resultat : **")
                .Append(run.EveryEntryReachesAnExit ? "respecte" : "VIOLE").Append("**. La matrice complete est publiee sans etre un gate.\n\n");
            text.Append("| Entree | Sorties atteintes |\n|---|---|\n");
            foreach (var pair in run.Reachability)
            {
                text.Append("| ").Append(Cell(pair.Key)).Append(" | ").Append(pair.Value.Count == 0 ? "aucune" : Cell(string.Join("<br>", pair.Value.ToArray()))).Append(" |\n");
            }

            text.Append('\n');

            // ------------------------------------------------ carrefours
            text.Append("## Carrefours et mouvements\n\n");
            foreach (var junction in import.Junctions)
            {
                text.Append("### ").Append(junction.Module.Label).Append(" (").Append(junction.Feature).Append(", `").Append(import.IdOf(junction.Key)).Append("`)\n\n");
                if (junction.Feature == JunctionFeature.Roundabout)
                {
                    text.Append("Representation par primitives ordinaires, sans inference de cycle : ")
                        .Append(CountRole(junction, MovementRole.RoundaboutEntry)).Append(" entrees, ")
                        .Append(CountRole(junction, MovementRole.RoundaboutExit)).Append(" sorties, ")
                        .Append(CountRole(junction, MovementRole.RoundaboutContinuation)).Append(" continuations d'anneau.\n\n");
                }
                else
                {
                    text.Append(junction.Movements.Count).Append(" mouvements explicites.\n\n");
                }

                text.Append("| Mouvement | Identite | Poids (plus grand = prefere) | Cap (deg) |\n|---|---|---:|---:|\n");
                var movements = new List<ImportedCurve>(junction.Movements);
                movements.Sort(delegate(ImportedCurve a, ImportedCurve b) { return string.CompareOrdinal(a.Key, b.Key); });
                foreach (var movement in movements)
                {
                    text.Append("| ").Append(Cell(movement.Label)).Append(" | `").Append(import.IdOf(movement.Key)).Append("` | ")
                        .Append(MigrationFormat.Weight(movement.KeyEdge.Weight)).Append(" | ").Append(MigrationFormat.Degrees(movement.TurnDegrees)).Append(" |\n");
                }

                text.Append('\n');
            }

            // ------------------------------------------------ taches
            text.Append("## Taches d'authoring (Story 5.28)\n\n");
            text.Append("Semantique absente de V1, jamais inventee. ")
                .Append(import.Source.Adjacencies.Length == 0
                    ? "Aucune adjacence de meme sens n'existe dans la source : aucune tache d'adjacence hors celles listees.\n\n"
                    : "La source porte " + import.Source.Adjacencies.Length + " adjacence(s) de meme sens, disposee(s) ci-dessous.\n\n");
            text.Append("| Categorie | Sujet | Tache |\n|---|---|---|\n");
            var tasks = new List<AuthoringTask>(import.Tasks);
            tasks.Sort(delegate(AuthoringTask a, AuthoringTask b)
            {
                int byCategory = string.CompareOrdinal(a.Category, b.Category);
                return byCategory != 0 ? byCategory : string.CompareOrdinal(a.SubjectKey, b.SubjectKey);
            });
            foreach (var task in tasks)
            {
                text.Append("| ").Append(task.Category).Append(" | `").Append(import.IdOf(task.SubjectKey)).Append("` ").Append(Cell(task.SubjectKey)).Append(" | ").Append(Cell(task.Text)).Append(" |\n");
            }

            text.Append('\n');
            text.Append("Fixtures de localisation sur la carte reelle : bloquees tant que le modele ne compile pas (`RoadLocalizer` exige un modele compile) ; reportees a la 5.28.\n\n");

            // ------------------------------------------------ dispositions
            text.Append("## Dispositions des elements source\n\n");
            text.Append("Chaque element source recoit une disposition typee ; toute forme non disposable fait ")
                .Append("echouer l'import, donc un rapport produit ne peut pas porter de rejet.\n\n");
            foreach (SourceItemKind item in Enum.GetValues(typeof(SourceItemKind)))
            {
                int count = 0;
                foreach (var disposition in import.Dispositions)
                {
                    count += disposition.Item == item ? 1 : 0;
                }

                text.Append("### ").Append(ItemTitle(item)).Append(" (").Append(count).Append(")\n\n");
                text.Append("| Source | Cle source | Disposition | Cible | Detail |\n|---|---|---|---|---|\n");
                foreach (var disposition in import.Dispositions)
                {
                    if (disposition.Item != item)
                    {
                        continue;
                    }

                    text.Append("| ").Append(Cell(disposition.SourceLabel)).Append(" | `").Append(disposition.SourceKey).Append("` | ").Append(disposition.Kind)
                        .Append(" | `").Append(import.IdOf(disposition.TargetKey)).Append("` | ").Append(Cell(disposition.Detail)).Append(" |\n");
                }

                text.Append('\n');
            }

            return text.ToString();
        }

        private static string ItemTitle(SourceItemKind item)
        {
            switch (item)
            {
                case SourceItemKind.Node:
                    return "Noeuds";
                case SourceItemKind.Edge:
                    return "Aretes (authorees et jointures)";
                case SourceItemKind.TurnWeight:
                    return "Poids de virage authores";
                case SourceItemKind.ConnectorMatch:
                    return "Jointures de connecteurs";
                case SourceItemKind.PortalRole:
                    return "Roles de portail";
                default:
                    throw new ArgumentException("Categorie de source sans titre : " + item + ". Ajouter son libelle ici, jamais un repli silencieux.");
            }
        }

        private static int CountRole(ImportedJunction junction, MovementRole role)
        {
            int count = 0;
            foreach (var movement in junction.Movements)
            {
                count += movement.Role == role ? 1 : 0;
            }

            return count;
        }

        private static string CountByCode(MigrationRun run)
        {
            var counts = new SortedDictionary<RoadModelValidationCode, int>();
            var all = new List<RoadModelValidationIssue>(run.StructuralIssues);
            all.AddRange(run.GeometricIssues);
            foreach (var issue in all)
            {
                int count;
                counts.TryGetValue(issue.Code, out count);
                counts[issue.Code] = count + 1;
            }

            var parts = new List<string>();
            foreach (var pair in counts)
            {
                parts.Add(pair.Key + " (" + (int)pair.Key + ") : " + pair.Value);
            }

            return string.Join(", ", parts.ToArray());
        }

        private static void IssueTable(StringBuilder text, string title, List<RoadModelValidationIssue> issues)
        {
            text.Append("### ").Append(title).Append(" (").Append(issues.Count).Append(")\n\n");
            if (issues.Count == 0)
            {
                text.Append("Aucune.\n\n");
                return;
            }

            text.Append("| Code | Sujet | Message |\n|---|---|---|\n");
            foreach (var issue in issues)
            {
                text.Append("| ").Append(issue.Code).Append(" (").Append((int)issue.Code).Append(") | `").Append(issue.SubjectId).Append("` | ")
                    .Append(Cell(issue.Message)).Append(" |\n");
            }

            text.Append('\n');
        }

        private static void MeasureList(StringBuilder text, MigrationRun run, MeasureClass measureClass, string title)
        {
            text.Append("### ").Append(title).Append("\n\n");
            var rows = new List<string>();
            foreach (var metric in run.Metrics)
            {
                foreach (var value in metric.Values)
                {
                    if (value.Class == measureClass)
                    {
                        rows.Add("| " + metric.Title + " | " + Cell(value.Subject) + " | " + FormatValue(value.Value, metric) + " | " + metric.Threshold + " |");
                    }
                }
            }

            if (rows.Count == 0)
            {
                text.Append("Aucune.\n\n");
                return;
            }

            rows.Sort(StringComparer.Ordinal);
            text.Append("| Mesure | Sujet | Valeur | Seuil |\n|---|---|---:|---|\n");
            foreach (var row in rows)
            {
                text.Append(row).Append('\n');
            }

            text.Append('\n');
        }

        /// <summary>Rang le plus proche (nearest-rank) sur les valeurs triees ; tiret si vide.</summary>
        private static string Stat(List<float> sorted, float quantile, Metric metric)
        {
            if (sorted.Count == 0)
            {
                return "-";
            }

            int rank = quantile <= 0f ? 0 : Mathf.Clamp(Mathf.CeilToInt(quantile * sorted.Count) - 1, 0, sorted.Count - 1);
            return FormatValue(sorted[rank], metric);
        }

        private static string FormatValue(float value, Metric metric)
        {
            return metric.Unit == "deg" ? MigrationFormat.Degrees(value) : MigrationFormat.Meters(value);
        }

        private static void Row(StringBuilder text, string field, string value)
        {
            text.Append("| ").Append(field).Append(" | ").Append(value).Append(" |\n");
        }

        private static string Cell(string value)
        {
            return (value ?? string.Empty).Replace("|", "/").Replace("\r", " ").Replace("\n", " ");
        }

        // ============================================================ verification du binding

        /// <summary>Relit la liaison d'un rapport. Nul si l'en-tete est absent ou illisible.</summary>
        public static MigrationBinding ParseBinding(string reportText, out string body)
        {
            body = null;
            if (reportText == null || !reportText.StartsWith(MigrationBinding.Header + "\n", StringComparison.Ordinal))
            {
                return null;
            }

            int end = reportText.IndexOf("\n-->\n", StringComparison.Ordinal);
            if (end < 0)
            {
                return null;
            }

            body = reportText.Substring(end + "\n-->\n".Length);
            var binding = new MigrationBinding();
            foreach (var line in reportText.Substring(0, end).Split('\n'))
            {
                int colon = line.IndexOf(": ", StringComparison.Ordinal);
                if (colon < 0)
                {
                    continue;
                }

                string field = line.Substring(0, colon);
                string value = line.Substring(colon + 2);
                int number;
                switch (field)
                {
                    case "source-hash":
                        binding.SourceHash = value;
                        break;
                    case "importer-version":
                        binding.ImporterVersion = int.TryParse(value, out number) ? number : -1;
                        break;
                    case "compiler-schema-version":
                        binding.CompilerSchemaVersion = int.TryParse(value, out number) ? number : -1;
                        break;
                    case "model-id":
                        binding.ModelId = value;
                        break;
                    case "lineage-hash":
                        binding.LineageHash = value;
                        break;
                    case "body-hash":
                        binding.BodyHash = value;
                        break;
                }
            }

            return binding;
        }

        /// <summary>
        /// Verifie un rapport contre la lignee qui l'accompagne et la liaison attendue (import
        /// frais). Rend les motifs de rejet ; vide = accepte. Ne repare jamais rien.
        /// </summary>
        public static List<string> Verify(string reportText, string lineageText, MigrationBinding expected)
        {
            var reasons = new List<string>();
            string body;
            var binding = ParseBinding(reportText, out body);
            if (binding == null)
            {
                reasons.Add("En-tete de liaison absent ou illisible : rapport detache.");
                return reasons;
            }

            if (binding.BodyHash != V1SourceSet.Sha256Hex(body))
            {
                reasons.Add("Corps du rapport modifie depuis sa generation (body-hash).");
            }

            if (binding.LineageHash != V1SourceSet.Sha256Hex(lineageText ?? string.Empty))
            {
                reasons.Add("La lignee presente n'est pas celle du rapport (lineage-hash).");
            }

            if (expected == null)
            {
                reasons.Add("Liaison attendue absente : verification incomplete, rapport non accepte.");
            }
            else
            {
                Compare(reasons, "source-hash", binding.SourceHash, expected.SourceHash);
                Compare(reasons, "importer-version", binding.ImporterVersion.ToString(), expected.ImporterVersion.ToString());
                Compare(reasons, "compiler-schema-version", binding.CompilerSchemaVersion.ToString(), expected.CompilerSchemaVersion.ToString());
                Compare(reasons, "model-id", binding.ModelId, expected.ModelId);
                Compare(reasons, "lineage-hash (attendu)", binding.LineageHash, expected.LineageHash);
            }

            return reasons;
        }

        private static void Compare(List<string> reasons, string field, string actual, string expected)
        {
            if (actual != expected)
            {
                reasons.Add("Rapport perime : " + field + " = " + actual + ", attendu " + expected + ".");
            }
        }

        // ============================================================ menu et ecriture

        [MenuItem("RoadRage/Traffic V2/Migrer MVP_Run")]
        public static void MigrateMvpRun()
        {
            var open = SceneManager.GetSceneByPath(ScenePath);
            bool inHierarchy = open.IsValid();
            bool wasLoaded = inHierarchy && open.isLoaded;
            string refusal = RefusalReason(EditorApplication.isPlayingOrWillChangePlaymode, wasLoaded, inHierarchy && open.isDirty);
            if (refusal != null)
            {
                Debug.LogError(refusal);
                return;
            }

            var scene = wasLoaded ? open : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("[Traffic V2] MVP_Run illisible : migration annulee, rien n'est ecrit.");
                return;
            }

            try
            {
                string error;
                if (!Migrate(scene, LineageFullPath, ReportFullPath, out error))
                {
                    Debug.LogError("[Traffic V2] " + error);
                    return;
                }

                AssetDatabase.ImportAsset(LineagePath);
                Debug.Log("[Traffic V2] Migration ecrite : " + LineagePath + " et " + ReportPath + ".");
            }
            finally
            {
                if (!wasLoaded)
                {
                    // Une scene presente mais dechargee est seulement redechargee, jamais retiree.
                    EditorSceneManager.CloseScene(scene, !inHierarchy);
                }
            }
        }

        /// <summary>
        /// Ecriture fail-closed : un passage en echec n'ecrit RIEN et laisse les fichiers existants
        /// intacts. Sinon, les deux textes deja complets en memoire sont ecrits en fichiers
        /// temporaires, puis la lignee est remplacee avant le rapport.
        /// ponytail: deux fichiers ne sont pas atomiques ensemble ; un arret entre les deux
        /// remplacements laisse un couple que Verify rejette (lineage-hash), relancer le menu le repare.
        /// </summary>
        public static bool TryWrite(MigrationRun run, string lineageFile, string reportFile, out string error)
        {
            if (run == null || !run.Succeeded || run.LineageJson == null || run.ReportText == null)
            {
                error = "Migration refusee, rien n'est ecrit :\n- "
                    + (run == null ? "aucun passage" : string.Join("\n- ", run.Failures.ToArray()));
                return false;
            }

            var encoding = new UTF8Encoding(false);
            string temp = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp");
            Directory.CreateDirectory(temp);
            string lineageTemp = Path.Combine(temp, "rrs-migration-lineage.tmp");
            string reportTemp = Path.Combine(temp, "rrs-migration-report.tmp");
            try
            {
                File.WriteAllText(lineageTemp, run.LineageJson, encoding);
                File.WriteAllText(reportTemp, run.ReportText, encoding);
                Directory.CreateDirectory(Path.GetDirectoryName(lineageFile));
                Directory.CreateDirectory(Path.GetDirectoryName(reportFile));
                Replace(lineageTemp, lineageFile);
                Replace(reportTemp, reportFile);
            }
            catch (IOException exception)
            {
                // Le remplacement peut echouer pour une raison du systeme de fichiers (destination
                // verrouillee, disque plein) : c'est un echec de migration, pas une exception brute.
                error = "Ecriture impossible : " + exception.Message;
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                error = "Ecriture refusee par le systeme de fichiers : " + exception.Message;
                return false;
            }
            finally
            {
                if (File.Exists(lineageTemp))
                {
                    File.Delete(lineageTemp);
                }

                if (File.Exists(reportTemp))
                {
                    File.Delete(reportTemp);
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Refus pre-vol d'une migration : <c>null</c> si le passage peut etre tente, sinon le message
        /// exact que le menu journalise. Extrait pour etre eprouve sans toucher aux fichiers reels :
        /// aucun refus n'ecrit quoi que ce soit, et c'est la seule chose que le menu decide avant
        /// d'ouvrir la scene.
        /// </summary>
        public static string RefusalReason(bool playMode, bool wasLoaded, bool isDirty)
        {
            if (playMode)
            {
                return "[Traffic V2] Migration interdite en Play Mode : la source serait l'etat runtime, pas l'authoring.";
            }

            if (wasLoaded && isDirty)
            {
                return "[Traffic V2] MVP_Run a des modifications non sauvegardees : la source ne serait pas celle du disque. Migration annulee.";
            }

            return null;
        }

        /// <summary>
        /// Le passage lui-meme, sans scene a ouvrir ni refus pre-vol : relit la lignee a l'emplacement
        /// demande (fichier absent = premier import, fichier vide ou incoherent = echec dur dans
        /// `Parse`), importe la scene fournie, puis ecrit fail-closed aux deux chemins demandes.
        ///
        /// C'est ce que le menu appelle avec les chemins du projet. Les tests l'appellent avec des
        /// chemins temporaires : c'est la seule facon d'eprouver la selection de chemin, la relecture
        /// de la lignee et l'ecriture sans toucher aux artefacts committes.
        /// </summary>
        public static bool Migrate(Scene scene, string lineageFile, string reportFile, out string error)
        {
            string prior = File.Exists(lineageFile) ? File.ReadAllText(lineageFile) : null;
            return TryWrite(Run(scene, prior), lineageFile, reportFile, out error);
        }

        private static void Replace(string temp, string destination)
        {
            if (File.Exists(destination))
            {
                File.Replace(temp, destination, null);
            }
            else
            {
                File.Move(temp, destination);
            }
        }
    }
}
#endif
