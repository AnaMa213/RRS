#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    // =====================================================================================
    // Story 5.28 -- modele authore et Gate A.
    //
    // Le RoadModelSource persiste est DERIVE : import V1 relance en lecture seule sur la scene et
    // la lignee committees (zero identite frappee ou retiree), fusionne aux decisions humaines,
    // compile deux fois (controles, puis zones acceptees). Rien n'est applique sans entree de
    // decision ; tout est lie par hashes ; un artefact perime est rejete, jamais repare. Le
    // sign-off n'est ecrit que par le proprietaire, depuis GateAReviewWindow.
    // =====================================================================================

    public sealed class LocalizationFixture
    {
        public string Name;
        public string Expectation;
        public string Observed;
        public bool Passed;
    }

    /// <summary>
    /// Primitive d'overlay. Le texte canonique et le dessin Scene view lisent les memes : pour
    /// <c>box</c>, <see cref="Points"/> vaut { centre, demi-dimensions } ; sinon une polyligne.
    /// </summary>
    public sealed class OverlayPrimitive
    {
        public string Kind;
        public RoadId Subject;
        public string Note;
        public Vector3[] Points;

        public bool IsBox
        {
            get { return Kind == "frontiere" || Kind == "zone"; }
        }
    }

    public sealed class OverlayInstance
    {
        public string Key;
        public string Label;
        public V1ModuleKind ModuleKind;
        public Bounds Bounds;
        public readonly List<OverlayPrimitive> Primitives = new List<OverlayPrimitive>();
    }

    /// <summary>Largeur revue d'un sujet : importee (amorce) et appliquee (5.49), min / max sur ses echantillons.</summary>
    public sealed class AppliedWidth
    {
        public string SubjectKey;
        public WidthApplication Application;
        public float ImportedLeftMin;
        public float ImportedLeftMax;
        public float ImportedRightMin;
        public float ImportedRightMax;
        public float AppliedLeftMin;
        public float AppliedLeftMax;
        public float AppliedRightMin;
        public float AppliedRightMax;
    }

    /// <summary>Liaison du rapport Gate A : celle de la 5.27, plus decisions, modele, version et overlay.</summary>
    public sealed class GateABinding
    {
        public const string Header = "<!-- rrs-gate-a-binding";

        public string SourceHash;
        public int ImporterVersion;
        public int CompilerSchemaVersion;
        public int PipelineVersion;
        public string ModelId;
        public string LineageHash;
        public string DecisionsHash;
        public string ModelHash;
        public string RoadModelVersion;
        public string OverlayHash;
        public string BodyHash;

        /// <summary>Champs lies dans l'ordre d'ecriture (body-hash exclu, il se verifie a part).</summary>
        public KeyValuePair<string, string>[] Fields()
        {
            return new[]
            {
                new KeyValuePair<string, string>("source-hash", SourceHash),
                new KeyValuePair<string, string>("importer-version", ImporterVersion.ToString()),
                new KeyValuePair<string, string>("compiler-schema-version", CompilerSchemaVersion.ToString()),
                new KeyValuePair<string, string>("pipeline-version", PipelineVersion.ToString()),
                new KeyValuePair<string, string>("model-id", ModelId),
                new KeyValuePair<string, string>("lineage-hash", LineageHash),
                new KeyValuePair<string, string>("decisions-hash", DecisionsHash),
                new KeyValuePair<string, string>("model-hash", ModelHash),
                new KeyValuePair<string, string>("road-model-version", RoadModelVersion),
                new KeyValuePair<string, string>("overlay-hash", OverlayHash)
            };
        }
    }

    /// <summary>Un passage complet du pipeline authore. Rien n'est ecrit ici.</summary>
    public sealed class AuthoredRun
    {
        public MigrationRun Migration;
        public AuthoringDecisions Decisions;
        public string LineageText;
        public string DecisionsText;
        public readonly List<string> Failures = new List<string>();

        /// <summary>Candidats generes sur le modele compile sans zones ; nul tant que la compilation 1 n'a pas reussi.</summary>
        public List<ConflictCandidate> Candidates;

        /// <summary>Modele compile sans zones sur lequel les candidats ont ete balayes ; conserve meme si le rapprochement echoue.</summary>
        public CompiledRoadModel CandidateModel;

        /// <summary>Balayage de toutes les paires (candidats, suivi, selection englobante seule, sans contact).</summary>
        public List<PairSweep> PairSweeps;

        public RoadModelSource Source;
        public CompiledRoadModel Compiled;
        public readonly List<LocalizationFixture> Fixtures = new List<LocalizationFixture>();

        /// <summary>Largeur importee / appliquee par sujet, dans l'ordre des decisions.</summary>
        public readonly List<AppliedWidth> Widths = new List<AppliedWidth>();

        /// <summary>Mesure de chaque giratoire (5.49) : enveloppe V2 appliquee et anneau physique.</summary>
        public readonly List<RoundaboutMeasurement> Roundabouts = new List<RoundaboutMeasurement>();

        public readonly List<OverlayInstance> Overlay = new List<OverlayInstance>();
        public string ModelText;
        public string OverlayText;
        public string ReportText;
        public GateABinding Binding;

        public bool Succeeded
        {
            get { return Failures.Count == 0 && ReportText != null; }
        }

        public V1ImportResult Import
        {
            get { return Migration == null ? null : Migration.Import; }
        }
    }

    public static class AuthoredRoadModel
    {
        /// <summary>Version du pipeline : tout changement de regle (candidats, fixtures, overlay) l'incremente.</summary>
        public const int PipelineVersion = 3;

        public const string DecisionsPath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json";
        public const string ModelPath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json";
        public const string SignoffPath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-signoff.json";
        public const string OverlayPath = "_bmad-output/implementation-artifacts/overlay-5-28-mvp-run.txt";
        public const string ReportPath = "_bmad-output/implementation-artifacts/migration-report-5-28-mvp-run.md";
        public const string ReviewDiffPath = "_bmad-output/implementation-artifacts/review-5-50-diff.md";

        private const string AuthoredLabel = "MVP_Run (authoring 5.28)";

        public static string FullPath(string projectRelative)
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, projectRelative);
        }

        // ============================================================ pipeline

        public static AuthoredRun Run(Scene scene, string lineageText, string decisionsText)
        {
            return Run(V1SourceSet.Extract(scene), lineageText, decisionsText);
        }

        public static AuthoredRun Run(V1SourceSet set, string lineageText, string decisionsText)
        {
            var run = new AuthoredRun();
            run.LineageText = lineageText;
            run.DecisionsText = decisionsText;
            if (lineageText == null)
            {
                run.Failures.Add("Lignee absente : relancer la migration 5.27 (RoadRage/Traffic V2/Migrer MVP_Run) avant l'authoring.");
                return run;
            }

            run.Migration = MigrationReport.Run(set, lineageText);
            if (!run.Migration.Succeeded)
            {
                run.Failures.AddRange(run.Migration.Failures);
                return run;
            }

            var lineage = run.Import.Lineage;
            if (lineage.ModelIdMinted || lineage.Minted.Count > 0 || lineage.Retired.Count > 0)
            {
                run.Failures.Add("Lignee perimee : l'import frappe " + lineage.Minted.Count + " identite(s) et en retire " + lineage.Retired.Count
                    + ". Relancer la migration 5.27 (RoadRage/Traffic V2/Migrer MVP_Run) avant l'authoring ; rien n'est ecrit.");
                return run;
            }

            try
            {
                run.Decisions = AuthoringDecisions.Parse(decisionsText);
            }
            catch (FormatException exception)
            {
                run.Failures.Add(exception.Message);
                return run;
            }

            var withControls = ApplyDecisions(run);
            if (run.Failures.Count > 0)
            {
                return run;
            }

            var withoutZones = Compile(run, withControls, "compilation 1 (controles, sans zones)");
            if (withoutZones == null)
            {
                return run;
            }

            if (!(SweptRadius(withoutZones.ValidationProfile) > 0f))
            {
                run.Failures.Add("Rayon balaye non positif (demi-gabarit + marge = " + SweptRadius(withoutZones.ValidationProfile)
                    + " m) : aucun candidat ne peut etre genere.");
                return run;
            }

            run.CandidateModel = withoutZones;
            run.PairSweeps = ConflictSweep.Analyze(withoutZones);
            run.Candidates = ConflictSweep.ToCandidates(run.PairSweeps);
            PopulateCandidateFingerprints(run, withoutZones);
            var zones = MatchConflicts(run);
            if (run.Failures.Count > 0)
            {
                return run;
            }

            run.Source = Copy(withControls);
            run.Source.ConflictZones = zones.ToArray();
            run.Compiled = Compile(run, run.Source, "compilation 2 (zones acceptees)");
            if (run.Compiled == null)
            {
                return run;
            }

            RunFixtures(run);
            if (run.Failures.Count > 0)
            {
                return run;
            }

            run.Roundabouts.AddRange(RoundaboutClearance.Measure(run.Import, run.Compiled, run.Failures));
            if (run.Failures.Count > 0)
            {
                return run;
            }

            var provenance = new RoadModelProvenance();
            provenance.SourceHash = set.SourceHash;
            provenance.LineageHash = V1SourceSet.Sha256Hex(lineageText);
            provenance.DecisionsHash = V1SourceSet.Sha256Hex(decisionsText);
            provenance.ImporterVersion = V1RoadModelImporter.ImporterVersion;
            provenance.PipelineVersion = PipelineVersion;
            run.ModelText = RoadModelDocument.Serialize(run.Source, provenance);

            run.Overlay.AddRange(BuildOverlay(run));
            run.OverlayText = RenderOverlay(run.Overlay);

            var binding = new GateABinding();
            binding.SourceHash = set.SourceHash;
            binding.ImporterVersion = V1RoadModelImporter.ImporterVersion;
            binding.CompilerSchemaVersion = RoadModelCompiler.CompilerSchemaVersion;
            binding.PipelineVersion = PipelineVersion;
            binding.ModelId = run.Source.ModelId.ToString();
            binding.LineageHash = provenance.LineageHash;
            binding.DecisionsHash = provenance.DecisionsHash;
            binding.ModelHash = V1SourceSet.Sha256Hex(run.ModelText);
            binding.RoadModelVersion = run.Compiled.Version.ToString();
            binding.OverlayHash = V1SourceSet.Sha256Hex(run.OverlayText);
            string body = RenderBody(run, binding);
            binding.BodyHash = V1SourceSet.Sha256Hex(body);
            run.Binding = binding;
            run.ReportText = RenderHeader(binding) + body;
            return run;
        }

        /// <summary>
        /// Controles, largeurs, dispositions et champs differes. Chaque decision doit trouver son
        /// sujet et chaque sujet sa decision : orphelin ou manque = echec dur nommant la cle.
        /// </summary>
        private static RoadModelSource ApplyDecisions(AuthoredRun run)
        {
            var import = run.Import;
            var decisions = run.Decisions;
            var failures = run.Failures;

            // ---------------------------------------------------- controles : un par approche
            var junctionOfApproach = new Dictionary<string, ImportedJunction>(StringComparer.Ordinal);
            var movementsOfApproach = new Dictionary<string, List<RoadId>>(StringComparer.Ordinal);
            foreach (var junction in import.Junctions)
            {
                foreach (var movement in junction.Movements)
                {
                    string approach = movement.From.Key;
                    ImportedJunction known;
                    if (junctionOfApproach.TryGetValue(approach, out known) && known != junction)
                    {
                        failures.Add("Approche '" + approach + "' partagee entre deux carrefours : un controle ne peut lier qu'un carrefour.");
                        continue;
                    }

                    junctionOfApproach[approach] = junction;
                    if (!movementsOfApproach.ContainsKey(approach))
                    {
                        movementsOfApproach.Add(approach, new List<RoadId>());
                    }

                    movementsOfApproach[approach].Add(import.IdOf(movement.Key));
                }
            }

            var controls = new List<JunctionControl>();
            var covered = new HashSet<string>(StringComparer.Ordinal);
            foreach (var decision in decisions.Controls)
            {
                if (!junctionOfApproach.ContainsKey(decision.ApproachKey))
                {
                    failures.Add("Controle orphelin " + decision.Id + " : cle d'approche inconnue '" + decision.ApproachKey + "'.");
                    continue;
                }

                if (decision.Kind != JunctionControlKind.Uncontrolled)
                {
                    failures.Add("Genre de controle non admis pour '" + decision.ApproachKey + "' : " + decision.Kind
                        + ". Seul Uncontrolled est admis ; Stop, Yield, Priority et Signalized relevent de la Story 5.35.");
                    continue;
                }

                covered.Add(decision.ApproachKey);
                var members = new List<RoadId>(movementsOfApproach[decision.ApproachKey]);
                members.Sort();
                var control = new JunctionControl();
                control.Id = decision.Id;
                control.JunctionId = import.IdOf(junctionOfApproach[decision.ApproachKey].Key);
                control.Kind = decision.Kind;
                control.ControlledMovementIds = members.ToArray();
                controls.Add(control);
            }

            var approaches = new List<string>(junctionOfApproach.Keys);
            approaches.Sort(StringComparer.Ordinal);
            foreach (var approach in approaches)
            {
                if (!covered.Contains(approach) && !HasControlDecision(decisions, approach))
                {
                    failures.Add("Approche sans controle : '" + approach + "'.");
                }
            }

            // ---------------------------------------------------- largeurs revues
            var owned = new SortedDictionary<string, List<RoadCurveSample>>(StringComparer.Ordinal);
            foreach (var section in import.Sections)
            {
                var samples = new List<RoadCurveSample>();
                foreach (var corridor in section.Corridors)
                {
                    samples.AddRange(corridor.Samples);
                }

                owned[section.Key] = samples;
            }

            foreach (var junction in import.Junctions)
            {
                var samples = new List<RoadCurveSample>();
                foreach (var movement in junction.Movements)
                {
                    samples.AddRange(movement.Samples);
                }

                owned[junction.Key] = samples;
            }

            var widthByKey = new Dictionary<string, WidthDecision>(StringComparer.Ordinal);
            foreach (var width in decisions.Widths)
            {
                if (!owned.ContainsKey(width.SubjectKey))
                {
                    failures.Add("Decision de largeur orpheline : '" + width.SubjectKey + "'.");
                }

                widthByKey[width.SubjectKey] = width;
            }

            foreach (var pair in owned)
            {
                if (!widthByKey.ContainsKey(pair.Key))
                {
                    failures.Add("Largeur non revue : '" + pair.Key + "'.");
                }
                else if (pair.Value.Count == 0)
                {
                    failures.Add("Largeur sans echantillon : '" + pair.Key + "' ne possede aucun echantillon a comparer.");
                }
            }

            var source = Copy(import.Source);
            var applied = ApplyWidths(run, source, widthByKey);

            // ---------------------------------------------------- dispositions des autres taches
            var dispositionByTask = new Dictionary<string, TaskDisposition>(StringComparer.Ordinal);
            foreach (var disposition in decisions.Dispositions)
            {
                dispositionByTask[disposition.Category + "\n" + disposition.SubjectKey] = disposition;
            }

            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var task in import.Tasks)
            {
                if (IsTypedCategory(task.Category))
                {
                    if (!TypedCoverage(task, import, junctionOfApproach, covered, owned, widthByKey))
                    {
                        failures.Add("Tache non disposee : " + task.Category + " '" + task.SubjectKey + "' (aucune donnee typee ne couvre ce sujet).");
                    }

                    continue;
                }

                var expected = AuthoringDecisions.ExpectedKind(task.Category);
                TaskDisposition disposition;
                if (expected == null)
                {
                    failures.Add("Tache " + task.Category + " '" + task.SubjectKey + "' : aucune disposition possible dans la 5.28, decision humaine requise.");
                }
                else if (!dispositionByTask.TryGetValue(task.Category + "\n" + task.SubjectKey, out disposition))
                {
                    failures.Add("Tache non disposee : " + task.Category + " '" + task.SubjectKey + "'.");
                }
                else
                {
                    used.Add(task.Category + "\n" + task.SubjectKey);
                    if (disposition.Kind != expected.Value)
                    {
                        failures.Add("Disposition " + disposition.Kind + " pour " + task.Category + " '" + task.SubjectKey + "' : " + expected.Value + " attendu.");
                    }
                }
            }

            foreach (var disposition in decisions.Dispositions)
            {
                if (!used.Contains(disposition.Category + "\n" + disposition.SubjectKey))
                {
                    failures.Add("Disposition orpheline : " + disposition.Category + " '" + disposition.SubjectKey + "' ne correspond a aucune tache 5.27 libre.");
                }
            }

            foreach (DeferredFieldKind field in Enum.GetValues(typeof(DeferredFieldKind)))
            {
                if (!decisions.DeferredFields.Exists(delegate(DeferredField f) { return f.Field == field; }))
                {
                    failures.Add("Champ differe sans reouverture : " + field + " (aucune valeur n'est inventee, la reouverture est obligatoire).");
                }
            }

            source.Label = AuthoredLabel;
            source.Controls = controls.ToArray();
            foreach (var width in decisions.Widths)
            {
                List<RoadCurveSample> imported;
                List<RoadCurveSample> result;
                if (owned.TryGetValue(width.SubjectKey, out imported) && applied.TryGetValue(width.SubjectKey, out result) && imported.Count > 0)
                {
                    var record = new AppliedWidth();
                    record.SubjectKey = width.SubjectKey;
                    record.Application = width.Application;
                    Range(imported, out record.ImportedLeftMin, out record.ImportedLeftMax, out record.ImportedRightMin, out record.ImportedRightMax);
                    Range(result, out record.AppliedLeftMin, out record.AppliedLeftMax, out record.AppliedRightMin, out record.AppliedRightMax);
                    run.Widths.Add(record);
                }
            }

            return source;
        }

        /// <summary>
        /// Largeurs revues APPLIQUEES (5.49) sur des copies des echantillons importes (Copy est
        /// superficiel : les tableaux de l'import ne sont jamais ecrits). Sections d'abord
        /// (<see cref="WidthApplication.Uniform"/> seul) ; puis carrefours : Uniform ecrit g/d,
        /// EndpointInterpolation interpole en s/Length entre la fin appliquee de From et le debut
        /// applique de To, la decision valant plancher. Ensuite gabarit sur tout echantillon, et
        /// refus d'elargir un corridor portant un portail (enveloppe derivee de l'import). L'axe ne
        /// change pas. Rend les echantillons appliques par sujet.
        /// </summary>
        private static Dictionary<string, List<RoadCurveSample>> ApplyWidths(AuthoredRun run, RoadModelSource source, Dictionary<string, WidthDecision> widthByKey)
        {
            var import = run.Import;
            var failures = run.Failures;
            var applied = new Dictionary<string, List<RoadCurveSample>>(StringComparer.Ordinal);
            source.Corridors = (LaneCorridor[])source.Corridors.Clone();
            source.Movements = (JunctionMovement[])source.Movements.Clone();
            source.Junctions = (Junction[])source.Junctions.Clone();
            var profile = source.ValidationProfile;
            float gauge = profile.MaxVehicleHalfWidthMeters + profile.LateralClearanceMarginMeters;
            var corridorIndex = new Dictionary<RoadId, int>();
            for (int i = 0; i < source.Corridors.Length; i++)
            {
                corridorIndex[source.Corridors[i].Id] = i;
            }

            var movementIndex = new Dictionary<RoadId, int>();
            for (int i = 0; i < source.Movements.Length; i++)
            {
                movementIndex[source.Movements[i].Id] = i;
            }

            var widthOfCorridor = new Dictionary<string, WidthDecision>(StringComparer.Ordinal);
            foreach (var section in import.Sections)
            {
                WidthDecision decision;
                if (!widthByKey.TryGetValue(section.Key, out decision))
                {
                    continue;
                }

                if (decision.Application != WidthApplication.Uniform)
                {
                    failures.Add("Mode d'application de largeur interdit pour la section '" + section.Key + "' : " + decision.Application
                        + ". Une section n'admet que Uniform ; EndpointInterpolation est reserve aux carrefours.");
                    continue;
                }

                var result = new List<RoadCurveSample>();
                foreach (var corridor in section.Corridors)
                {
                    int index = corridorIndex[import.IdOf(corridor.Key)];
                    var samples = (RoadCurveSample[])source.Corridors[index].Samples.Clone();
                    for (int k = 0; k < samples.Length; k++)
                    {
                        samples[k].HalfWidthLeftMeters = decision.HalfWidthLeftMeters;
                        samples[k].HalfWidthRightMeters = decision.HalfWidthRightMeters;
                    }

                    source.Corridors[index].Samples = samples;
                    CheckGauge(failures, section.Key, corridor.Key, samples, gauge);
                    widthOfCorridor[corridor.Key] = decision;
                    result.AddRange(samples);
                }

                applied[section.Key] = result;
            }

            foreach (var junction in import.Junctions)
            {
                WidthDecision decision;
                if (!widthByKey.TryGetValue(junction.Key, out decision))
                {
                    continue;
                }

                var result = new List<RoadCurveSample>();
                bool widened = false;
                foreach (var movement in junction.Movements)
                {
                    int index = movementIndex[import.IdOf(movement.Key)];
                    var samples = (RoadCurveSample[])source.Movements[index].Samples.Clone();
                    WidthDecision from = decision;
                    WidthDecision to = decision;
                    if (decision.Application == WidthApplication.EndpointInterpolation
                        && (!widthOfCorridor.TryGetValue(movement.From.Key, out from) || !widthOfCorridor.TryGetValue(movement.To.Key, out to)))
                    {
                        failures.Add("Largeur de '" + junction.Key + "' : le mouvement '" + movement.Key + "' n'a pas de largeur appliquee a ses deux extremites.");
                        continue;
                    }

                    float length = source.Movements[index].LengthMeters;
                    bool floorBroken = false;
                    for (int k = 0; k < samples.Length; k++)
                    {
                        float u = length > 0f ? Mathf.Clamp01(samples[k].SMeters / length) : 0f;
                        float left = Mathf.Lerp(from.HalfWidthLeftMeters, to.HalfWidthLeftMeters, u);
                        float right = Mathf.Lerp(from.HalfWidthRightMeters, to.HalfWidthRightMeters, u);
                        if (!floorBroken && (Below(left, decision.HalfWidthLeftMeters) || Below(right, decision.HalfWidthRightMeters)))
                        {
                            floorBroken = true;
                            failures.Add("Plancher de largeur viole pour '" + junction.Key + "' : echantillon " + k + " du mouvement '" + movement.Key + "' interpole a "
                                + MigrationFormat.Meters(left) + " / " + MigrationFormat.Meters(right) + " m, plancher " + MigrationFormat.Meters(decision.HalfWidthLeftMeters)
                                + " / " + MigrationFormat.Meters(decision.HalfWidthRightMeters) + " m (gauche / droite).");
                        }

                        widened |= !AuthoringDecisions.SameWidth(left, samples[k].HalfWidthLeftMeters) || !AuthoringDecisions.SameWidth(right, samples[k].HalfWidthRightMeters);
                        samples[k].HalfWidthLeftMeters = left;
                        samples[k].HalfWidthRightMeters = right;
                    }

                    source.Movements[index].Samples = samples;
                    CheckGauge(failures, junction.Key, movement.Key, samples, gauge);
                    result.AddRange(samples);
                }

                applied[junction.Key] = result;

                // La frontiere du carrefour enveloppe les largeurs APPLIQUEES (regle de l'importeur :
                // position +/- max(g, d) sur les trois axes). Un carrefour non elargi garde la sienne.
                if (widened)
                {
                    RoadId junctionId = import.IdOf(junction.Key);
                    int junctionIndex = Array.FindIndex(source.Junctions, delegate(Junction j) { return j.Id == junctionId; });
                    source.Junctions[junctionIndex].Boundary = Envelope(result);
                }
            }

            foreach (var portal in import.Portals)
            {
                WidthDecision decision;
                if (!widthOfCorridor.TryGetValue(portal.Corridor.Key, out decision))
                {
                    continue;
                }

                foreach (var sample in portal.Corridor.Samples)
                {
                    if (!AuthoringDecisions.SameWidth(sample.HalfWidthLeftMeters, decision.HalfWidthLeftMeters)
                        || !AuthoringDecisions.SameWidth(sample.HalfWidthRightMeters, decision.HalfWidthRightMeters))
                    {
                        failures.Add("Portail sur sujet elargi : '" + portal.Corridor.SectionKey + "' porte le portail '" + portal.Key
                            + "', dont l'enveloppe derive de la largeur importee ; largeur appliquee differente refusee.");
                        break;
                    }
                }
            }

            return applied;
        }

        /// <summary>Gabarit (demi-gabarit max + marge du profil versionne) de chaque cote, sur tout echantillon possede ou interpole.</summary>
        private static void CheckGauge(List<string> failures, string subjectKey, string curveKey, RoadCurveSample[] samples, float gauge)
        {
            for (int k = 0; k < samples.Length; k++)
            {
                if (Below(samples[k].HalfWidthLeftMeters, gauge) || Below(samples[k].HalfWidthRightMeters, gauge))
                {
                    failures.Add("Largeur sous le gabarit pour '" + subjectKey + "' : echantillon " + k + " de '" + curveKey + "' a " + MigrationFormat.Meters(samples[k].HalfWidthLeftMeters)
                        + " / " + MigrationFormat.Meters(samples[k].HalfWidthRightMeters) + " m (gauche / droite), gabarit " + MigrationFormat.Meters(gauge) + " m (demi-gabarit + marge).");
                    return;
                }
            }
        }

        /// <summary>Boite englobant chaque echantillon a +/- max(g, d) : meme regle que la frontiere importee.</summary>
        private static RoadBoundsBox Envelope(List<RoadCurveSample> samples)
        {
            var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            foreach (var sample in samples)
            {
                float reach = Mathf.Max(sample.HalfWidthLeftMeters, sample.HalfWidthRightMeters);
                min = Vector3.Min(min, sample.Position - Vector3.one * reach);
                max = Vector3.Max(max, sample.Position + Vector3.one * reach);
            }

            var box = new RoadBoundsBox();
            box.Center = 0.5f * (min + max);
            box.Extents = 0.5f * (max - min);
            return box;
        }

        /// <summary>Strictement sous la valeur, au-dela de la quantification canonique.</summary>
        private static bool Below(float value, float floor)
        {
            return value < floor && !AuthoringDecisions.SameWidth(value, floor);
        }

        private static void Range(List<RoadCurveSample> samples, out float leftMin, out float leftMax, out float rightMin, out float rightMax)
        {
            leftMin = rightMin = float.PositiveInfinity;
            leftMax = rightMax = float.NegativeInfinity;
            foreach (var sample in samples)
            {
                leftMin = Mathf.Min(leftMin, sample.HalfWidthLeftMeters);
                leftMax = Mathf.Max(leftMax, sample.HalfWidthLeftMeters);
                rightMin = Mathf.Min(rightMin, sample.HalfWidthRightMeters);
                rightMax = Mathf.Max(rightMax, sample.HalfWidthRightMeters);
            }
        }

        /// <summary>
        /// Une tache typee n'est disposee que si ses donnees la couvrent : Controle = carrefour dont
        /// chaque approche a son controle ; Conflit = carrefour importe (ses candidats sont tous
        /// decides, sinon MatchConflicts echoue) ; Largeur = sujet possede avec sa largeur revue.
        /// </summary>
        private static bool TypedCoverage(AuthoringTask task, V1ImportResult import, Dictionary<string, ImportedJunction> junctionOfApproach,
            HashSet<string> covered, SortedDictionary<string, List<RoadCurveSample>> owned, Dictionary<string, WidthDecision> widthByKey)
        {
            if (task.Category == AuthoringDecisions.WidthCategory)
            {
                return owned.ContainsKey(task.SubjectKey) && widthByKey.ContainsKey(task.SubjectKey);
            }

            if (!import.Junctions.Exists(delegate(ImportedJunction j) { return j.Key == task.SubjectKey; }))
            {
                return false;
            }

            if (task.Category == AuthoringDecisions.ConflictCategory)
            {
                return true;
            }

            int approaches = 0;
            foreach (var pair in junctionOfApproach)
            {
                if (pair.Value.Key == task.SubjectKey)
                {
                    approaches++;
                    if (!covered.Contains(pair.Key))
                    {
                        return false;
                    }
                }
            }

            return approaches > 0;
        }

        private static bool HasControlDecision(AuthoringDecisions decisions, string approach)
        {
            // Une decision de genre non admis est deja un echec : ne pas la compter une seconde fois comme manque.
            return decisions.Controls.Exists(delegate(ControlDecision c) { return c.ApproachKey == approach; });
        }

        private static bool IsTypedCategory(string category)
        {
            return category == AuthoringDecisions.ControlCategory || category == AuthoringDecisions.ConflictCategory || category == AuthoringDecisions.WidthCategory;
        }

        private static CompiledRoadModel Compile(AuthoredRun run, RoadModelSource source, string stage)
        {
            var structural = new List<RoadModelValidationIssue>();
            var geometric = new List<RoadModelValidationIssue>();
            var compiled = MigrationReport.ValidateSource(source, structural, geometric);
            if (compiled == null)
            {
                structural.AddRange(geometric);
                foreach (var issue in structural)
                {
                    run.Failures.Add("Compile refuse, " + stage + " : " + issue);
                }
            }

            return compiled;
        }

        /// <summary>Chaque candidat a exactement une decision, chaque decision un candidat. Seules les acceptees deviennent des zones.</summary>
        private static List<ConflictZone> MatchConflicts(AuthoredRun run)
        {
            var keyById = AuthoringDecisions.KeysById(run.Import);
            var decisionByPair = new Dictionary<string, ConflictDecision>(StringComparer.Ordinal);
            foreach (var decision in run.Decisions.Conflicts)
            {
                decisionByPair[decision.MovementKeyA + "\n" + decision.MovementKeyB] = decision;
            }

            var zones = new List<ConflictZone>();
            var matched = new HashSet<string>(StringComparer.Ordinal);
            foreach (var candidate in run.Candidates)
            {
                string pair = PairKey(keyById[candidate.MovementA], keyById[candidate.MovementB]);
                ConflictDecision decision;
                if (!decisionByPair.TryGetValue(pair, out decision))
                {
                    run.Failures.Add("Candidat de conflit non dispose : " + pair.Replace("\n", " x ") + ".");
                    continue;
                }

                matched.Add(pair);
                if (string.IsNullOrEmpty(decision.GeometryFingerprint)
                    || !string.Equals(decision.GeometryFingerprint, candidate.GeometryFingerprint, StringComparison.Ordinal))
                {
                    run.Failures.Add("Proposition historique non reconfirmee : " + pair.Replace("\n", " x ")
                        + " (empreinte decidee " + (string.IsNullOrEmpty(decision.GeometryFingerprint) ? "absente" : decision.GeometryFingerprint)
                        + ", fraiche " + candidate.GeometryFingerprint + ").");
                    continue;
                }

                if (decision.Decision == ConflictDecisionKind.Accepted)
                {
                    var zone = new ConflictZone();
                    zone.Id = decision.Id;
                    zone.JunctionId = candidate.JunctionId;
                    zone.Volume = candidate.Volume;
                    zone.MemberMovementIds = new[] { candidate.MovementA, candidate.MovementB };
                    zones.Add(zone);
                }
            }

            foreach (var pair in decisionByPair.Keys)
            {
                if (!matched.Contains(pair))
                {
                    run.Failures.Add("Decision de conflit sans candidat : " + pair.Replace("\n", " x ") + ".");
                }
            }

            return zones;
        }

        public static string PairKey(string a, string b)
        {
            return string.CompareOrdinal(a, b) < 0 ? a + "\n" + b : b + "\n" + a;
        }

        private static RoadModelSource Copy(RoadModelSource source)
        {
            var copy = new RoadModelSource();
            copy.ModelId = source.ModelId;
            copy.Label = source.Label;
            copy.ValidationProfile = source.ValidationProfile;
            copy.LocalizationProfile = source.LocalizationProfile;
            copy.DrivabilityProfile = source.DrivabilityProfile;
            copy.Sections = source.Sections;
            copy.Corridors = source.Corridors;
            copy.Connections = source.Connections;
            copy.Adjacencies = source.Adjacencies;
            copy.Junctions = source.Junctions;
            copy.Movements = source.Movements;
            copy.Controls = source.Controls;
            copy.ConflictZones = source.ConflictZones;
            copy.SignalPlans = source.SignalPlans;
            copy.Portals = source.Portals;
            copy.Manifest = source.Manifest;
            return copy;
        }

        // ============================================================ candidats de conflit

        /// <summary>
        /// Candidats d'un modele compile : paires de mouvements du meme carrefour, ni de meme approche
        /// ni en suivi, dont le balayage conservateur prouve un contact possible (voir
        /// <see cref="ConflictSweep"/>). Hors ligne seulement.
        /// </summary>
        public static List<ConflictCandidate> Candidates(CompiledRoadModel model)
        {
            return ConflictSweep.ToCandidates(ConflictSweep.Analyze(model));
        }

        private static void PopulateCandidateFingerprints(AuthoredRun run, CompiledRoadModel model)
        {
            Dictionary<RoadId, string> keys = AuthoringDecisions.KeysById(run.Import);
            for (int i = 0; i < run.Candidates.Count; i++)
            {
                ConflictCandidate candidate = run.Candidates[i];
                CompiledJunctionMovement a;
                CompiledJunctionMovement b;
                if (!model.TryGetMovement(candidate.MovementA, out a) || !model.TryGetMovement(candidate.MovementB, out b))
                {
                    run.Failures.Add("Empreinte fraiche impossible : mouvement candidat introuvable.");
                    continue;
                }

                candidate.GeometryFingerprint = PairGeometryFingerprint.Compute(
                    keys[candidate.MovementA], a.Samples,
                    keys[candidate.MovementB], b.Samples,
                    candidate.Volume);
                run.Candidates[i] = candidate;
            }
        }

        /// <summary>Rayon balaye : demi-gabarit max plus marge laterale du profil versionne.</summary>
        public static float SweptRadius(RoadModelValidationProfile profile)
        {
            return profile.MaxVehicleHalfWidthMeters + profile.LateralClearanceMarginMeters;
        }

        /// <summary>
        /// Recoupement symetrique de deux enveloppes balayees de rayon <paramref name="radius"/> :
        /// chaque courbe est densifiee par <see cref="RoadCurve.Sample"/> a pas fixe r/4 en abscisse
        /// (extremites incluses), et un point a moins de 2r de sa projection sur l'autre courbe est en
        /// recoupement. Volume = boite des points en recoupement des deux cotes, elargie de r.
        /// ponytail: balayage lateral seul, la longueur du gabarit au-dela des extremites n'est pas
        /// balayee (les coutures relevent du suivi) ; a etendre si la 5.34 mesure un conflit manque.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Rayon non strictement positif ou non fini : le pas de densification serait nul.</exception>
        public static bool SweptOverlap(RoadCurve a, RoadCurve b, float radius, out RoadBoundsBox volume)
        {
            if (!(radius > 0f) || float.IsInfinity(radius))
            {
                throw new ArgumentOutOfRangeException("radius", radius, "Rayon balaye strictement positif et fini attendu.");
            }

            var hits = new List<Vector3>();
            CollectOverlap(a, b, radius, hits);
            CollectOverlap(b, a, radius, hits);
            volume = default(RoadBoundsBox);
            if (hits.Count == 0)
            {
                return false;
            }

            Vector3 min = hits[0];
            Vector3 max = hits[0];
            foreach (var hit in hits)
            {
                min = Vector3.Min(min, hit);
                max = Vector3.Max(max, hit);
            }

            volume.Center = 0.5f * (min + max);
            volume.Extents = 0.5f * (max - min) + Vector3.one * radius;
            return true;
        }

        /// <summary>
        /// Balayage conservateur 5.50 de deux trajectoires sans prolongement : delegue a
        /// <see cref="ConflictSweep.Evaluate"/> (distance exacte des empreintes, borne d'intervalle).
        /// </summary>
        public static bool SweptOverlap(
            IReadOnlyList<RoadCurveSample> a,
            IReadOnlyList<RoadCurveSample> b,
            RoadModelValidationProfile profile,
            out RoadBoundsBox volume)
        {
            volume = default(RoadBoundsBox);
            if (a == null || b == null || a.Count == 0 || b.Count == 0)
            {
                return false;
            }

            var sweep = ConflictSweep.Evaluate(
                new List<List<SweepPose>> { ConflictSweep.Poses(a) },
                new List<List<SweepPose>> { ConflictSweep.Poses(b) },
                profile);
            volume = sweep.Volume;
            return sweep.IsCandidate;
        }

        private static void CollectOverlap(RoadCurve from, RoadCurve other, float radius, List<Vector3> hits)
        {
            float step = 0.25f * radius;
            for (int i = 0; ; i++)
            {
                float s = i * step;
                bool last = s >= from.Length;
                var point = from.Sample(last ? from.Length : s).Position;
                if (other.Project(point).DistanceMeters < 2f * radius)
                {
                    hits.Add(point);
                }

                if (last)
                {
                    return;
                }
            }
        }

        // ============================================================ fixtures de localisation

        /// <summary>
        /// Six fixtures sur la carte reelle, derivees structurellement : corridor du portail d'entree
        /// de plus petit RoadId, et approche de plus petit RoadId portant au moins deux mouvements.
        /// Jamais un nom. Une fixture rouge est un echec dur.
        /// </summary>
        private static void RunFixtures(AuthoredRun run)
        {
            var model = run.Compiled;
            var profile = model.ValidationProfile;
            var localization = model.LocalizationProfile;

            Portal entry = default(Portal);
            bool found = false;
            foreach (var portal in model.Portals)
            {
                if (portal.Role == PortalRole.Entry && (!found || portal.Id.CompareTo(entry.Id) < 0))
                {
                    entry = portal;
                    found = true;
                }
            }

            var approachCounts = new SortedDictionary<RoadId, int>();
            foreach (var movement in model.Movements)
            {
                int count;
                approachCounts.TryGetValue(movement.FromCorridorId, out count);
                approachCounts[movement.FromCorridorId] = count + 1;
            }

            RoadId approachId = RoadId.None;
            foreach (var pair in approachCounts)
            {
                if (pair.Value >= 2)
                {
                    approachId = pair.Key;
                    break;
                }
            }

            EffectiveLaneCorridor corridor;
            EffectiveLaneCorridor approach;
            if (!found || !model.TryGetCorridor(entry.CorridorId, out corridor) || !model.TryGetCorridor(approachId, out approach))
            {
                run.Failures.Add("Fixtures de localisation : aucun portail d'entree ou aucune approche a deux mouvements.");
                return;
            }

            var curve = corridor.Curve;
            var mid = curve.Sample(0.5f * curve.Length);
            var end = curve.Sample(curve.Length);

            // Cote libre : oppose au voisin de coupe, pour qu'un deplacement ne tombe pas sur l'autre voie.
            float side = 1f;
            foreach (var siblingId in model.GetCorridorsInSection(corridor.SectionId))
            {
                EffectiveLaneCorridor sibling;
                if (siblingId != corridor.CorridorId && model.TryGetCorridor(siblingId, out sibling))
                {
                    side = curve.Project(sibling.Curve.Sample(0.5f * sibling.Curve.Length).Position).LateralOffsetMeters > 0f ? -1f : 1f;
                    break;
                }
            }

            float sideHalfWidth = side > 0f ? mid.HalfWidthRightMeters : mid.HalfWidthLeftMeters;
            var footprint = new VehicleFootprint();
            footprint.FrontMeters = 0.5f * profile.MaxVehicleLengthMeters;
            footprint.RearMeters = 0.5f * profile.MaxVehicleLengthMeters;
            footprint.LeftMeters = profile.MaxVehicleHalfWidthMeters;
            footprint.RightMeters = profile.MaxVehicleHalfWidthMeters;

            RoadId c = corridor.CorridorId;
            float displaced = sideHalfWidth - 0.5f * profile.MaxVehicleHalfWidthMeters;
            var approachEnd = approach.Curve.Sample(approach.Curve.Length);

            Fixture(run, model, "nominal", "localise sur le corridor du portail, aucun drapeau",
                mid.Position, mid.Tangent, mid.Up, footprint, RoadId.None,
                delegate(RoadLocation l) { return l.Localized && l.ElementId == c && l.Flags == RoadLocationFlags.None; });

            Fixture(run, model, "frontiere", "precedent retenu un quart d'hysteresis apres sa fin, OutsideEnvelope, pas de contresens",
                end.Position + end.Tangent * (0.25f * localization.HysteresisMeters), end.Tangent, end.Up, footprint, c,
                delegate(RoadLocation l) { return l.Localized && l.ElementId == c && l.HasFlag(RoadLocationFlags.OutsideEnvelope) && !l.HasFlag(RoadLocationFlags.WrongWay); });

            Fixture(run, model, "deplace", "reference dans l'enveloppe, empreinte debordante : meme corridor, lateral signe, OutsideEnvelope",
                mid.Position + mid.Right * (side * displaced), mid.Tangent, mid.Up, footprint, RoadId.None,
                delegate(RoadLocation l)
                {
                    return l.Localized && l.ElementId == c && l.HasFlag(RoadLocationFlags.OutsideEnvelope) && !l.HasFlag(RoadLocationFlags.WrongWay)
                        && Mathf.Abs(l.LateralOffsetMeters - side * displaced) < 1e-3f;
                });

            Fixture(run, model, "contresens", "cap oppose sur sa voie : meme corridor, WrongWay",
                mid.Position, -mid.Tangent, mid.Up, footprint, RoadId.None,
                delegate(RoadLocation l) { return l.Localized && l.ElementId == c && l.HasFlag(RoadLocationFlags.WrongWay); });

            RoadId approachCorridor = approach.CorridorId;
            Fixture(run, model, "carrefour ambigu", "debut des mouvements divergents d'une approche : un mouvement de cette approche, Ambiguous, confiance < 1",
                approachEnd.Position + approachEnd.Tangent * (0.5f * localization.HysteresisMeters), approachEnd.Tangent, approachEnd.Up, footprint, RoadId.None,
                delegate(RoadLocation l)
                {
                    CompiledJunctionMovement movement;
                    return l.Localized && l.ElementKind == RoadElementKind.JunctionMovement && l.HasFlag(RoadLocationFlags.Ambiguous) && l.Confidence < 1f
                        && model.TryGetMovement(l.ElementId, out movement) && movement.FromCorridorId == approachCorridor;
                });

            Fixture(run, model, "hors corridor", "reference hors enveloppe dans le seuil d'acceptation : reste localisee, OutsideEnvelope, sans snap",
                mid.Position + mid.Right * (side * (sideHalfWidth + 0.5f * localization.AcceptanceDistanceMeters)), mid.Tangent, mid.Up, footprint, RoadId.None,
                delegate(RoadLocation l) { return l.Localized && l.ElementId == c && l.HasFlag(RoadLocationFlags.OutsideEnvelope) && !l.HasFlag(RoadLocationFlags.WrongWay); });
        }

        private static void Fixture(AuthoredRun run, CompiledRoadModel model, string name, string expectation, Vector3 position, Vector3 forward, Vector3 up,
            VehicleFootprint footprint, RoadId previous, Predicate<RoadLocation> passes)
        {
            var pose = new VehicleFootprintPose();
            pose.Position = position;
            pose.Forward = forward;
            pose.Up = up;
            pose.Footprint = footprint;
            var location = RoadLocalizer.Localize(model, pose, previous, null);

            var fixture = new LocalizationFixture();
            fixture.Name = name;
            fixture.Expectation = expectation;
            fixture.Passed = passes(location);
            fixture.Observed = "localise=" + (location.Localized ? "oui" : "non") + ", " + location.ElementKind + " `" + location.ElementId + "`, drapeaux=" + location.Flags
                + ", lateral=" + MigrationFormat.Meters(location.LateralOffsetMeters) + " m, confiance=" + MigrationFormat.Weight(location.Confidence);
            run.Fixtures.Add(fixture);
            if (!fixture.Passed)
            {
                run.Failures.Add("Fixture de localisation rouge '" + name + "' : attendu " + expectation + " ; observe " + fixture.Observed + ".");
            }
        }

        // ============================================================ overlay

        /// <summary>
        /// Primitives d'overlay par instance de module (les 25 de l'ensemble source) : centres et
        /// bords des corridors et mouvements, frontieres, zones, enveloppes de portail, controles.
        /// Seule source du texte canonique ET du dessin Scene view.
        /// </summary>
        public static List<OverlayInstance> BuildOverlay(AuthoredRun run)
        {
            var import = run.Import;
            var model = run.Compiled;
            var byModule = new Dictionary<V1Module, OverlayInstance>();
            foreach (var module in import.SourceSet.Modules)
            {
                var instance = new OverlayInstance();
                instance.Key = module.Key;
                instance.Label = module.Label;
                instance.ModuleKind = module.Kind;
                byModule.Add(module, instance);
            }

            foreach (var section in import.Sections)
            {
                foreach (var curve in section.Corridors)
                {
                    EffectiveLaneCorridor corridor;
                    model.TryGetCorridor(import.IdOf(curve.Key), out corridor);
                    AddCurve(byModule[curve.Module], "corridor", corridor.CorridorId, corridor.Samples, corridor.Curve);
                }
            }

            var zoneById = new Dictionary<RoadId, CompiledConflictZone>();
            foreach (var zone in model.ConflictZones)
            {
                zoneById.Add(zone.Id, zone);
            }

            foreach (var imported in import.Junctions)
            {
                var instance = byModule[imported.Module];
                Junction junction;
                model.TryGetJunction(import.IdOf(imported.Key), out junction);
                AddBox(instance, "frontiere", junction.Id, junction.Feature.ToString(), junction.Boundary);

                foreach (var curve in imported.Movements)
                {
                    CompiledJunctionMovement movement;
                    model.TryGetMovement(import.IdOf(curve.Key), out movement);
                    AddCurve(instance, "mouvement", movement.Id, movement.Samples, movement.Curve);
                }

                foreach (var zoneId in model.GetConflictZonesInJunction(junction.Id))
                {
                    AddBox(instance, "zone", zoneId, zoneById[zoneId].MemberMovementIds[0] + " x " + zoneById[zoneId].MemberMovementIds[1], zoneById[zoneId].Volume);
                }

                foreach (var controlId in model.GetControlsInJunction(junction.Id))
                {
                    CompiledJunctionControl control;
                    model.TryGetControl(controlId, out control);
                    CompiledJunctionMovement first;
                    model.TryGetMovement(control.ControlledMovementIds[0], out first);
                    EffectiveLaneCorridor approach;
                    model.TryGetCorridor(first.FromCorridorId, out approach);
                    Add(instance, "controle", control.Id, control.Kind + ", approche " + approach.CorridorId + ", " + control.ControlledMovementIds.Count + " mouvements",
                        new[] { approach.Curve.Sample(approach.Curve.Length).Position });
                }
            }

            foreach (var imported in import.Portals)
            {
                Portal portal = default(Portal);
                foreach (var candidate in model.Portals)
                {
                    if (candidate.Id == import.IdOf(imported.Key))
                    {
                        portal = candidate;
                    }
                }

                EffectiveLaneCorridor corridor;
                model.TryGetCorridor(portal.CorridorId, out corridor);
                var curve = corridor.Curve;
                var start = curve.Sample(portal.SMeters - 0.5f * portal.EnvelopeLengthMeters);
                var stop = curve.Sample(portal.SMeters + 0.5f * portal.EnvelopeLengthMeters);
                float w = portal.EnvelopeHalfWidthMeters;
                Add(byModule[imported.Corridor.Module], "portail", portal.Id, portal.Role.ToString(), new[]
                {
                    start.Position - start.Right * w,
                    stop.Position - stop.Right * w,
                    stop.Position + stop.Right * w,
                    start.Position + start.Right * w,
                    start.Position - start.Right * w
                });
            }

            var instances = new List<OverlayInstance>(byModule.Values);
            instances.Sort(delegate(OverlayInstance a, OverlayInstance b) { return string.CompareOrdinal(a.Key, b.Key); });
            foreach (var instance in instances)
            {
                bool first = true;
                var bounds = new Bounds();
                foreach (var primitive in instance.Primitives)
                {
                    var points = primitive.IsBox
                        ? new[] { primitive.Points[0] - primitive.Points[1], primitive.Points[0] + primitive.Points[1] }
                        : primitive.Points;
                    foreach (var point in points)
                    {
                        if (first)
                        {
                            bounds = new Bounds(point, Vector3.zero);
                            first = false;
                        }

                        bounds.Encapsulate(point);
                    }
                }

                instance.Bounds = bounds;
            }

            return instances;
        }

        private static void AddCurve(OverlayInstance instance, string kind, RoadId id, IReadOnlyList<RoadCurveSample> samples, RoadCurve curve)
        {
            var center = new Vector3[samples.Count];
            var left = new Vector3[samples.Count];
            var right = new Vector3[samples.Count];
            for (int i = 0; i < samples.Count; i++)
            {
                var point = curve.Sample(samples[i].SMeters);
                center[i] = point.Position;
                left[i] = point.Position - point.Right * point.HalfWidthLeftMeters;
                right[i] = point.Position + point.Right * point.HalfWidthRightMeters;
            }

            Add(instance, kind, id, "centre", center);
            Add(instance, kind, id, "bord gauche", left);
            Add(instance, kind, id, "bord droit", right);
        }

        private static void AddBox(OverlayInstance instance, string kind, RoadId id, string note, RoadBoundsBox box)
        {
            Add(instance, kind, id, note, new[] { box.Center, box.Extents });
        }

        private static void Add(OverlayInstance instance, string kind, RoadId id, string note, Vector3[] points)
        {
            var primitive = new OverlayPrimitive();
            primitive.Kind = kind;
            primitive.Subject = id;
            primitive.Note = note;
            primitive.Points = points;
            instance.Primitives.Add(primitive);
        }

        /// <summary>Texte canonique de l'overlay : coordonnees quantifiees au centimetre, LF.</summary>
        public static string RenderOverlay(List<OverlayInstance> instances)
        {
            var text = new StringBuilder();
            text.Append("# Overlay Gate A -- MVP_Run (Story 5.28, pipeline ").Append(PipelineVersion).Append(")\n");
            text.Append("# Genere par RoadRage/Traffic V2/Compiler le modele authore ; coordonnees monde en centimetres ; box = centre ; demi-dimensions.\n");
            foreach (var instance in instances)
            {
                text.Append("\ninstance ").Append(instance.Key).Append(" ").Append(instance.Label).Append(" (").Append(instance.ModuleKind).Append(")\n");
                foreach (var primitive in instance.Primitives)
                {
                    text.Append(primitive.Kind).Append(' ').Append(primitive.Subject).Append(" [").Append(primitive.Note).Append("] ");
                    for (int i = 0; i < primitive.Points.Length; i++)
                    {
                        text.Append(i == 0 ? string.Empty : primitive.IsBox ? " ; " : " ").Append(V1SourceSet.Q(primitive.Points[i], 0.01d));
                    }

                    text.Append('\n');
                }
            }

            return text.ToString();
        }

        // ============================================================ rapport

        private static string RenderHeader(GateABinding binding)
        {
            var text = new StringBuilder();
            text.Append(GateABinding.Header).Append('\n');
            foreach (var field in binding.Fields())
            {
                text.Append(field.Key).Append(": ").Append(field.Value).Append('\n');
            }

            text.Append("body-hash: ").Append(binding.BodyHash).Append('\n');
            text.Append("-->\n");
            return text.ToString();
        }

        private static string RenderBody(AuthoredRun run, GateABinding binding)
        {
            var import = run.Import;
            var model = run.Compiled;
            var keyById = AuthoringDecisions.KeysById(import);
            var text = new StringBuilder();

            text.Append("# Rapport Gate A : modele authore MVP_Run (Story 5.28)\n\n");
            text.Append("Genere par le menu `RoadRage/Traffic V2/Compiler le modele authore`. Ne pas editer : un rapport retouche ou detache de ses entrees est rejete, jamais repare.\n\n");

            text.Append("## Liaison\n\n| Champ | Valeur |\n|---|---|\n");
            Row(text, "Hash de la source V1 extraite", "`" + binding.SourceHash + "`");
            Row(text, "Version de l'importeur / du pipeline", binding.ImporterVersion + " / " + binding.PipelineVersion);
            Row(text, "CompilerSchemaVersion", binding.CompilerSchemaVersion.ToString());
            Row(text, "RoadModelId", "`" + binding.ModelId + "`");
            Row(text, "Hash de la lignee", "`" + binding.LineageHash + "` (`" + MigrationReport.LineagePath + "`)");
            Row(text, "Hash des decisions", "`" + binding.DecisionsHash + "` (`" + DecisionsPath + "`)");
            Row(text, "Hash du modele persiste", "`" + binding.ModelHash + "` (`" + ModelPath + "`)");
            Row(text, "RoadModelVersion", "`" + binding.RoadModelVersion + "`");
            Row(text, "Hash de l'overlay", "`" + binding.OverlayHash + "` (`" + OverlayPath + "`)");
            text.Append('\n');

            text.Append("## Resultat\n\n");
            text.Append("- Erreurs dures : **0** (le modele passe `Compile` ; une seule erreur aurait bloque toute ecriture).\n");
            text.Append("- Lignee inchangee : 0 identite frappee, 0 retiree (import 5.27 relance en lecture seule).\n");
            text.Append("- Taches 5.27 non disposees : **0** sur ").Append(import.Tasks.Count).Append(".\n");
            text.Append("- Fixtures de localisation : ").Append(run.Fixtures.Count).Append(" vertes sur ").Append(run.Fixtures.Count).Append(".\n\n");

            text.Append("## Modele authore\n\n| Enregistrement | Nombre |\n|---|---:|\n");
            Row(text, "JunctionMovement", model.Movements.Count.ToString());
            Row(text, "JunctionControl", model.Controls.Count + " (un par approche, tous `Uncontrolled`)");
            Row(text, "ConflictZone", model.ConflictZones.Count + " (decisions acceptees seulement)");
            Row(text, "SignalPlan", model.SignalPlans.Count + " (carrefours declares non signalises)");
            Row(text, "LaneAdjacency", model.Adjacencies.Count + " (aucune adjacence authoree)");
            Row(text, "Ligne d'arret", CountStopLines(model) + " (aucune ligne sous `Uncontrolled`)");
            text.Append('\n');

            // ------------------------------------------------ controles
            text.Append("## Controles par approche\n\n");
            text.Append("Un `JunctionControl` par corridor d'approche, lie a tous les mouvements partant de cette approche ; seul genre admis : `Uncontrolled` (decision du proprietaire, 2026-09-23). Chaque mouvement a exactement un controle.\n\n");
            text.Append("| Carrefour | Controle | Genre | Approche | Mouvements |\n|---|---|---|---|---:|\n");
            foreach (var junction in model.Junctions)
            {
                foreach (var controlId in model.GetControlsInJunction(junction.Id))
                {
                    CompiledJunctionControl control;
                    model.TryGetControl(controlId, out control);
                    CompiledJunctionMovement first;
                    model.TryGetMovement(control.ControlledMovementIds[0], out first);
                    text.Append("| ").Append(Cell(junction.Label)).Append(" | `").Append(control.Id).Append("` | ").Append(control.Kind).Append(" | `")
                        .Append(first.FromCorridorId).Append("` ").Append(Cell(keyById[first.FromCorridorId])).Append(" | ").Append(control.ControlledMovementIds.Count).Append(" |\n");
                }
            }

            text.Append('\n');

            // ------------------------------------------------ candidats
            var decisionByPair = new Dictionary<string, ConflictDecision>(StringComparer.Ordinal);
            foreach (var decision in run.Decisions.Conflicts)
            {
                decisionByPair[decision.MovementKeyA + "\n" + decision.MovementKeyB] = decision;
            }

            text.Append("## Candidats de conflit et decisions\n\n");
            text.Append("Candidat = paire de mouvements du meme carrefour, d'approches differentes, dont les enveloppes balayees se recoupent (croisement ou convergence). ")
                .Append("Rayon balaye r = demi-gabarit max ").Append(MigrationFormat.Meters(model.ValidationProfile.MaxVehicleHalfWidthMeters)).Append(" m + marge ")
                .Append(MigrationFormat.Meters(model.ValidationProfile.LateralClearanceMarginMeters)).Append(" m = ").Append(MigrationFormat.Meters(SweptRadius(model.ValidationProfile)))
                .Append(" m (profil versionne) ; courbes densifiees a pas r/4, recoupement a moins de 2r. Genere hors ligne sur le modele compile sans zones ; seules les decisions acceptees deviennent des `ConflictZone`, aucun consommateur n'infere de conflit.\n\n");
            text.Append("| Carrefour | Candidats | Acceptes | Rejetes | Paires de meme approche (suivi, pas conflit) |\n|---|---:|---:|---:|---:|\n");
            foreach (var junction in model.Junctions)
            {
                int candidates = 0;
                int accepted = 0;
                foreach (var candidate in run.Candidates)
                {
                    if (candidate.JunctionId == junction.Id)
                    {
                        candidates++;
                        accepted += decisionByPair[PairKey(keyById[candidate.MovementA], keyById[candidate.MovementB])].Decision == ConflictDecisionKind.Accepted ? 1 : 0;
                    }
                }

                text.Append("| ").Append(Cell(junction.Label)).Append(" | ").Append(candidates).Append(" | ").Append(accepted).Append(" | ").Append(candidates - accepted)
                    .Append(" | ").Append(SameApproachPairs(model, junction.Id)).Append(" |\n");
            }

            text.Append("\n| Carrefour | Mouvement A | Mouvement B | Decision | Zone | Motif |\n|---|---|---|---|---|---|\n");
            foreach (var candidate in run.Candidates)
            {
                Junction junction;
                model.TryGetJunction(candidate.JunctionId, out junction);
                CompiledJunctionMovement a;
                CompiledJunctionMovement b;
                model.TryGetMovement(candidate.MovementA, out a);
                model.TryGetMovement(candidate.MovementB, out b);
                var decision = decisionByPair[PairKey(keyById[candidate.MovementA], keyById[candidate.MovementB])];
                text.Append("| ").Append(Cell(junction.Label)).Append(" | ").Append(Cell(a.Label)).Append(" | ").Append(Cell(b.Label)).Append(" | ").Append(decision.Decision)
                    .Append(" | ").Append(decision.Decision == ConflictDecisionKind.Accepted ? "`" + decision.Id + "`" : "-").Append(" | ").Append(Cell(decision.Reason)).Append(" |\n");
            }

            text.Append('\n');

            // ------------------------------------------------ largeurs
            text.Append("## Largeurs revues\n\n");
            text.Append("Demi-largeurs gauche et droite explicites (AD-45). La largeur revue est APPLIQUEE aux echantillons possedes (5.49) : `Uniform` ecrit la decision ; ")
                .Append("`EndpointInterpolation` (carrefours seulement) interpole chaque mouvement en s/Length entre les largeurs appliquees de ses corridors d'extremite, la decision valant plancher. ")
                .Append("Tout echantillon reste >= demi-gabarit + marge (").Append(MigrationFormat.Meters(SweptRadius(model.ValidationProfile))).Append(" m) de chaque cote. ")
                .Append("Importee = amorce de l'importeur, jamais une autorite ; min-max sur les echantillons du sujet.\n\n");
            text.Append("| Sujet | Decision g / d (m) | Application | Importee g / d (m) | Appliquee g / d (m) |\n|---|---:|---|---:|---:|\n");
            foreach (var width in run.Widths)
            {
                var decision = run.Decisions.Widths.Find(delegate(WidthDecision w) { return w.SubjectKey == width.SubjectKey; });
                text.Append("| `").Append(import.IdOf(width.SubjectKey)).Append("` ").Append(Cell(width.SubjectKey)).Append(" | ")
                    .Append(MigrationFormat.Meters(decision.HalfWidthLeftMeters)).Append(" / ").Append(MigrationFormat.Meters(decision.HalfWidthRightMeters)).Append(" | ")
                    .Append(width.Application).Append(" | ")
                    .Append(Span(width.ImportedLeftMin, width.ImportedLeftMax)).Append(" / ").Append(Span(width.ImportedRightMin, width.ImportedRightMax)).Append(" | ")
                    .Append(Span(width.AppliedLeftMin, width.AppliedLeftMax)).Append(" / ").Append(Span(width.AppliedRightMin, width.AppliedRightMax)).Append(" |\n");
            }

            text.Append('\n');

            // ------------------------------------------------ giratoires
            var profile = model.ValidationProfile;
            text.Append("## Giratoires : degagement a deux gabarits\n\n");
            text.Append("Deux gabarits max du profil versionne (W/2 = ").Append(MigrationFormat.Meters(profile.MaxVehicleHalfWidthMeters)).Append(" m, L = ")
                .Append(MigrationFormat.Meters(profile.MaxVehicleLengthMeters)).Append(" m, marge m = ").Append(MigrationFormat.Meters(profile.LateralClearanceMarginMeters))
                .Append(" m) cote a cote, cap tangent, au point le plus serre : R_in = r_in + m + W/2 ; c_in = sqrt((R_in + W/2)^2 + (L/2)^2) ; R_out = c_in + 2m + W/2 ; ")
                .Append("c_out = sqrt((R_out + W/2)^2 + (L/2)^2) ; residu = (r_out - m) - c_out. Preuve supplementaire : un residu positif ne reduit jamais la cible (anneau V2 4,0 / 4,0 m, ilot <= 1,75 m, pave >= 10,25 m).\n\n");
            text.Append("V2 : centre = racine du module ; corridors d'anneau et continuations appliques ; r_in = max des bords interieurs, r_out = min des bords exterieurs. ")
                .Append("Physique : empreintes XZ des colliders ; r_in = portee de `").Append(RoundaboutClearance.IslandName).Append("` ; pave = min sur 720 rayons (pas 1 cm) de la sortie de l'union des `")
                .Append(RoundaboutClearance.RoadwayPrefix).Append("*` ; obstacles = colliders non declencheurs hors chaussee et ilot dont la hauteur recoupe [sommet de route, +")
                .Append(MigrationFormat.Meters(RoundaboutClearance.ProbeHeightMeters)).Append(" m] ; r_out = min(pave, obstacle le plus proche).\n\n");
            text.Append("| Instance | V2 r_in (m) | V2 r_out (m) | Residu V2 (m) | Ilot (m) | Pave (m) | Obstacle le plus proche | r_out physique (m) | Residu physique (m) |\n|---|---:|---:|---:|---:|---:|---|---:|---:|\n");
            foreach (var roundabout in run.Roundabouts)
            {
                text.Append("| ").Append(Cell(roundabout.Module.Label)).Append(" `").Append(roundabout.Module.Key).Append("` | ")
                    .Append(MigrationFormat.Meters(roundabout.EnvelopeInnerRadius)).Append(" | ").Append(MigrationFormat.Meters(roundabout.EnvelopeOuterRadius)).Append(" | ")
                    .Append(MigrationFormat.Meters(roundabout.EnvelopeResidual)).Append(" | ").Append(MigrationFormat.Meters(roundabout.IslandRadius)).Append(" | ")
                    .Append(MigrationFormat.Meters(roundabout.PavedRadius)).Append(" | ")
                    .Append(roundabout.NearestObstacle == null ? "aucun" : Cell(roundabout.NearestObstacle) + " a " + MigrationFormat.Meters(roundabout.NearestObstacleRadius) + " m")
                    .Append(" | ").Append(MigrationFormat.Meters(roundabout.PhysicalOuterRadius)).Append(" | ").Append(MigrationFormat.Meters(roundabout.PhysicalResidual)).Append(" |\n");
            }

            text.Append('\n');

            // ------------------------------------------------ taches
            text.Append("## Disposition des taches 5.27\n\n");
            text.Append("Chaque tache est disposee exactement une fois. Controle, Conflit et Largeur par leurs donnees typees ; les autres par une disposition explicite.\n\n");
            text.Append("| Categorie | Sujet | Disposition | Detail |\n|---|---|---|---|\n");
            var tasks = new List<AuthoringTask>(import.Tasks);
            tasks.Sort(delegate(AuthoringTask a, AuthoringTask b)
            {
                int byCategory = string.CompareOrdinal(a.Category, b.Category);
                return byCategory != 0 ? byCategory : string.CompareOrdinal(a.SubjectKey, b.SubjectKey);
            });
            foreach (var task in tasks)
            {
                string disposition;
                string detail;
                DescribeDisposition(run, task, out disposition, out detail);
                text.Append("| ").Append(task.Category).Append(" | `").Append(import.IdOf(task.SubjectKey)).Append("` ").Append(Cell(task.SubjectKey)).Append(" | ")
                    .Append(disposition).Append(" | ").Append(Cell(detail)).Append(" |\n");
            }

            text.Append('\n');

            // ------------------------------------------------ champs differes
            text.Append("## Champs differes\n\n");
            text.Append("Aucune valeur inventee : les sections gardent 0 m/s, aucune classe et le defaut d'enum `Asphalt` jusqu'a leur reouverture.\n\n");
            text.Append("| Champ | Reouverture |\n|---|---|\n");
            foreach (var field in run.Decisions.DeferredFields)
            {
                Row(text, field.Field.ToString(), Cell(field.Reopening));
            }

            text.Append('\n');

            // ------------------------------------------------ fixtures
            text.Append("## Fixtures de localisation (carte reelle)\n\n");
            text.Append("Derivees structurellement, jamais par nom : corridor du portail d'entree de plus petit `RoadId`, approche de plus petit `RoadId` a au moins deux mouvements. ")
                .Append("Empreinte = gabarit max du profil.\n\n");
            text.Append("| Fixture | Attendu | Observe | Verdict |\n|---|---|---|---|\n");
            foreach (var fixture in run.Fixtures)
            {
                text.Append("| ").Append(fixture.Name).Append(" | ").Append(Cell(fixture.Expectation)).Append(" | ").Append(Cell(fixture.Observed)).Append(" | ")
                    .Append(fixture.Passed ? "vert" : "ROUGE").Append(" |\n");
            }

            text.Append('\n');

            // ------------------------------------------------ overlay et gate
            text.Append("## Overlay et Gate A\n\n");
            text.Append("Overlay canonique : `").Append(OverlayPath).Append("` (").Append(run.Overlay.Count).Append(" instances de module, hash `").Append(binding.OverlayHash)
                .Append("`), produit par la meme fonction que le dessin de la fenetre `RoadRage/Traffic V2/Revue Gate A`.\n\n");
            text.Append("La Gate A n'est ouverte que par `").Append(SignoffPath).Append("`, ecrit par le proprietaire depuis cette fenetre apres revue des ")
                .Append(run.Overlay.Count).Append(" instances, et lie aux hashes source, lignee, decisions, compilateur, modele, version et overlay d'un pipeline frais. ")
                .Append("Un sign-off absent ou perime garde la Gate A fermee, jamais repare.\n\n");
            text.Append("| Instance | Genre |\n|---|---|\n");
            foreach (var instance in run.Overlay)
            {
                Row(text, Cell(instance.Label) + " `" + instance.Key + "`", instance.ModuleKind.ToString());
            }

            text.Append('\n');
            return text.ToString();
        }

        private static void DescribeDisposition(AuthoredRun run, AuthoringTask task, out string disposition, out string detail)
        {
            var import = run.Import;
            var model = run.Compiled;
            if (task.Category == AuthoringDecisions.ControlCategory)
            {
                disposition = "Controles";
                detail = model.GetControlsInJunction(import.IdOf(task.SubjectKey)).Count + " controles Uncontrolled, un par approche";
                return;
            }

            if (task.Category == AuthoringDecisions.ConflictCategory)
            {
                var junctionId = import.IdOf(task.SubjectKey);
                int candidates = 0;
                foreach (var candidate in run.Candidates)
                {
                    candidates += candidate.JunctionId == junctionId ? 1 : 0;
                }

                disposition = "Conflits";
                detail = candidates + " candidat(s) decide(s), " + model.GetConflictZonesInJunction(junctionId).Count + " zone(s) materialisee(s)";
                return;
            }

            if (task.Category == AuthoringDecisions.WidthCategory)
            {
                var width = run.Decisions.Widths.Find(delegate(WidthDecision w) { return w.SubjectKey == task.SubjectKey; });
                disposition = "Largeur revue";
                detail = MigrationFormat.Meters(width.HalfWidthLeftMeters) + " / " + MigrationFormat.Meters(width.HalfWidthRightMeters) + " m (gauche / droite), " + width.Application;
                return;
            }

            var free = run.Decisions.Dispositions.Find(delegate(TaskDisposition d) { return d.Category == task.Category && d.SubjectKey == task.SubjectKey; });
            disposition = free.Kind.ToString();
            detail = free.Note;
        }

        /// <summary>Valeur unique, ou min-max quand les echantillons different au-dela de la quantification.</summary>
        private static string Span(float min, float max)
        {
            return AuthoringDecisions.SameWidth(min, max) ? MigrationFormat.Meters(min) : MigrationFormat.Meters(min) + "-" + MigrationFormat.Meters(max);
        }

        private static int SameApproachPairs(CompiledRoadModel model, RoadId junctionId)
        {
            var ids = model.GetMovementsInJunction(junctionId);
            int pairs = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                CompiledJunctionMovement a;
                model.TryGetMovement(ids[i], out a);
                for (int j = i + 1; j < ids.Count; j++)
                {
                    CompiledJunctionMovement b;
                    model.TryGetMovement(ids[j], out b);
                    pairs += a.FromCorridorId == b.FromCorridorId ? 1 : 0;
                }
            }

            return pairs;
        }

        private static int CountStopLines(CompiledRoadModel model)
        {
            int count = 0;
            foreach (var control in model.Controls)
            {
                count += control.HasStopLine ? 1 : 0;
            }

            return count;
        }

        private static void Row(StringBuilder text, string field, string value)
        {
            text.Append("| ").Append(field).Append(" | ").Append(value).Append(" |\n");
        }

        private static string Cell(string value)
        {
            return (value ?? string.Empty).Replace("|", "/").Replace("\r", " ").Replace("\n", " ");
        }

        // ============================================================ verification et Gate A

        /// <summary>Relit l'en-tete d'un rapport Gate A. Nul si absent ou illisible.</summary>
        public static Dictionary<string, string> ParseBinding(string reportText, out string body)
        {
            body = null;
            if (reportText == null || !reportText.StartsWith(GateABinding.Header + "\n", StringComparison.Ordinal))
            {
                return null;
            }

            int end = reportText.IndexOf("\n-->\n", StringComparison.Ordinal);
            if (end < 0)
            {
                return null;
            }

            body = reportText.Substring(end + "\n-->\n".Length);
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var line in reportText.Substring(0, end).Split('\n'))
            {
                int colon = line.IndexOf(": ", StringComparison.Ordinal);
                if (colon > 0)
                {
                    fields[line.Substring(0, colon)] = line.Substring(colon + 2);
                }
            }

            return fields;
        }

        /// <summary>Rapport contre un pipeline frais. Vide = accepte. Ne repare jamais rien.</summary>
        public static List<string> VerifyReport(string reportText, AuthoredRun fresh)
        {
            var reasons = new List<string>();
            string body;
            var fields = ParseBinding(reportText, out body);
            if (fields == null)
            {
                reasons.Add("Rapport Gate A absent, detache ou sans en-tete de liaison.");
                return reasons;
            }

            string bodyHash;
            if (!fields.TryGetValue("body-hash", out bodyHash) || bodyHash != V1SourceSet.Sha256Hex(body))
            {
                reasons.Add("Corps du rapport Gate A modifie depuis sa generation (body-hash).");
            }

            if (fresh == null || !fresh.Succeeded)
            {
                reasons.Add("Pipeline frais en echec : " + (fresh == null ? "absent" : string.Join(" ; ", fresh.Failures.ToArray())));
                return reasons;
            }

            foreach (var field in fresh.Binding.Fields())
            {
                string actual;
                fields.TryGetValue(field.Key, out actual);
                if (actual != field.Value)
                {
                    reasons.Add("Rapport Gate A perime : " + field.Key + " = " + (actual ?? "<absent>") + ", attendu " + field.Value + ".");
                }
            }

            // En-tete intact et body-hash recalcule, ou ligne d'en-tete en trop : seule l'egalite
            // octet pour octet avec le rapport frais fait foi.
            if (reasons.Count == 0 && reportText != fresh.ReportText)
            {
                reasons.Add("Rapport Gate A different, octet pour octet, du rapport d'un pipeline frais.");
            }

            return reasons;
        }

        [Serializable]
        private sealed class SignoffLayout
        {
            public int Format;
            public string Approver;
            public string ApproverEmail;
            public string SignedAtUtc;
            public string[] ReviewedInstances;
            public string OverlayHash;
            public string SourceHash;
            public string LineageHash;
            public string DecisionsHash;
            public int ImporterVersion;
            public int CompilerSchemaVersion;
            public int PipelineVersion;
            public string ModelHash;
            public string RoadModelVersion;
        }

        /// <summary>
        /// Texte d'un sign-off lie au pipeline frais. Seul <see cref="GateAReviewWindow"/> l'ecrit sur
        /// disque, apres revue et confirmation du proprietaire ; l'identite d'approbation n'entre dans
        /// aucun hash de modele.
        /// </summary>
        public static string RenderSignoff(AuthoredRun fresh, string approver, string approverEmail, IList<string> reviewedInstances, DateTime signedAtUtc)
        {
            var layout = new SignoffLayout();
            layout.Format = 1;
            layout.Approver = approver;
            layout.ApproverEmail = approverEmail;
            layout.SignedAtUtc = signedAtUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            var reviewed = new List<string>(reviewedInstances);
            reviewed.Sort(StringComparer.Ordinal);
            layout.ReviewedInstances = reviewed.ToArray();
            layout.OverlayHash = fresh.Binding.OverlayHash;
            layout.SourceHash = fresh.Binding.SourceHash;
            layout.LineageHash = fresh.Binding.LineageHash;
            layout.DecisionsHash = fresh.Binding.DecisionsHash;
            layout.ImporterVersion = fresh.Binding.ImporterVersion;
            layout.CompilerSchemaVersion = fresh.Binding.CompilerSchemaVersion;
            layout.PipelineVersion = fresh.Binding.PipelineVersion;
            layout.ModelHash = fresh.Binding.ModelHash;
            layout.RoadModelVersion = fresh.Binding.RoadModelVersion;
            return JsonUtility.ToJson(layout, true).Replace("\r\n", "\n") + "\n";
        }

        /// <summary>Sign-off contre un pipeline frais. Vide = Gate A ouverte cote sign-off.</summary>
        public static List<string> VerifySignoff(string signoffText, AuthoredRun fresh)
        {
            var reasons = new List<string>();
            if (signoffText == null)
            {
                reasons.Add("Sign-off absent (" + SignoffPath + ") : Gate A fermee, revue de l'overlay et signature du proprietaire requises.");
                return reasons;
            }

            SignoffLayout layout;
            try
            {
                layout = JsonUtility.FromJson<SignoffLayout>(signoffText);
            }
            catch (ArgumentException exception)
            {
                reasons.Add("Sign-off illisible : " + exception.Message);
                return reasons;
            }

            if (layout == null || layout.Format != 1)
            {
                reasons.Add("Sign-off : format absent ou inconnu.");
                return reasons;
            }

            if (string.IsNullOrEmpty(layout.Approver) || layout.Approver.Trim().Length == 0)
            {
                reasons.Add("Sign-off sans approbateur.");
            }

            if (fresh == null || !fresh.Succeeded)
            {
                reasons.Add("Pipeline frais en echec : sign-off non verifiable, Gate A fermee.");
                return reasons;
            }

            var expected = new List<string>();
            foreach (var instance in fresh.Overlay)
            {
                expected.Add(instance.Key);
            }

            var reviewed = new List<string>(layout.ReviewedInstances ?? new string[0]);
            reviewed.Sort(StringComparer.Ordinal);
            if (string.Join("\n", reviewed.ToArray()) != string.Join("\n", expected.ToArray()))
            {
                reasons.Add("Sign-off : instances revues (" + reviewed.Count + ") differentes des " + expected.Count + " instances de l'overlay frais.");
            }

            var binding = fresh.Binding;
            Stale(reasons, "overlay-hash", layout.OverlayHash, binding.OverlayHash);
            Stale(reasons, "source-hash", layout.SourceHash, binding.SourceHash);
            Stale(reasons, "lineage-hash", layout.LineageHash, binding.LineageHash);
            Stale(reasons, "decisions-hash", layout.DecisionsHash, binding.DecisionsHash);
            Stale(reasons, "importer-version", layout.ImporterVersion.ToString(), binding.ImporterVersion.ToString());
            Stale(reasons, "compiler-schema-version", layout.CompilerSchemaVersion.ToString(), binding.CompilerSchemaVersion.ToString());
            Stale(reasons, "pipeline-version", layout.PipelineVersion.ToString(), binding.PipelineVersion.ToString());
            Stale(reasons, "model-hash", layout.ModelHash, binding.ModelHash);
            Stale(reasons, "road-model-version", layout.RoadModelVersion, binding.RoadModelVersion);
            return reasons;
        }

        /// <summary>Modele et overlay committes contre le pipeline frais. Vide = a jour.</summary>
        public static List<string> VerifyArtifacts(AuthoredRun fresh, string modelText, string overlayText)
        {
            var reasons = new List<string>();
            if (fresh == null || !fresh.Succeeded)
            {
                reasons.Add("Pipeline frais en echec : artefacts non verifiables.");
                return reasons;
            }

            if (modelText != fresh.ModelText)
            {
                reasons.Add(ModelPath + " differe du pipeline frais : relancer RoadRage/Traffic V2/Compiler le modele authore.");
            }

            if (overlayText != fresh.OverlayText)
            {
                reasons.Add(OverlayPath + " differe du pipeline frais : relancer RoadRage/Traffic V2/Compiler le modele authore.");
            }

            return reasons;
        }

        /// <summary>Gate A : pipeline frais vert, modele, overlay et rapport committes a jour, sign-off lie. Vide = ouverte.</summary>
        public static List<string> EvaluateGateA(AuthoredRun fresh, string reportText, string modelText, string overlayText, string signoffText)
        {
            var reasons = VerifyReport(reportText, fresh);
            reasons.AddRange(VerifyArtifacts(fresh, modelText, overlayText));
            reasons.AddRange(VerifySignoff(signoffText, fresh));
            return reasons;
        }

        private static void Stale(List<string> reasons, string field, string actual, string expected)
        {
            if (actual != expected)
            {
                reasons.Add("Sign-off perime : " + field + " = " + actual + ", attendu " + expected + ".");
            }
        }

        // ============================================================ amorce des decisions

        /// <summary>
        /// Texte d'amorce des decisions (jamais ecrit ici) : controles, puis candidats generes sur le
        /// modele compile avec ces controles, puis decisions completes. Nul et <paramref name="error"/>
        /// en cas d'echec.
        /// </summary>
        public static string ProposeText(V1SourceSet set, string lineageText, out string error)
        {
            error = null;
            var migration = MigrationReport.Run(set, lineageText);
            if (!migration.Succeeded)
            {
                error = string.Join("\n", migration.Failures.ToArray());
                return null;
            }

            try
            {
                // Premier passage : controles seuls, pour compiler et generer les candidats. Ses
                // echecs « candidat non dispose » sont attendus ; toute autre etape doit passer.
                var seed = AuthoringDecisions.Propose(migration.Import, new ConflictCandidate[0]);
                var partial = Run(set, lineageText, seed.SerializeHistoricalForReadOnlyMigration());
                if (partial.Candidates == null)
                {
                    error = string.Join("\n", partial.Failures.ToArray());
                    return null;
                }

                var proposal = AuthoringDecisions.Propose(migration.Import, partial.Candidates);
                var proposedRun = Run(set, lineageText, proposal.SerializeHistoricalForReadOnlyMigration());
                if (proposedRun.CandidateModel == null || proposedRun.PairSweeps == null)
                {
                    error = string.Join("\n", proposedRun.Failures.ToArray());
                    return null;
                }

                return AutomatedPairDecisionPolicy.CreatePlan(proposedRun).DecisionsText;
            }
            catch (InvalidOperationException exception)
            {
                error = exception.Message;
                return null;
            }
        }

        // ============================================================ menus et ecriture

        /// <summary>
        /// Differentiel 5.50 en lecture seule. Le passage normal est volontairement laisse en echec
        /// sur les decisions historiques ; les candidats et leurs empreintes ont deja ete produits
        /// avant cette porte. Seuls ces echecs de rapprochement sont admis ici.
        /// </summary>
        public static string BuildReviewDiff(
            V1SourceSet set,
            string lineageText,
            string decisionsText,
            string historicalText,
            string baselineHashesText,
            string baselineModelText,
            out string error)
        {
            error = null;
            HistoricalPairFingerprintTable historical;
            try
            {
                historical = PairGeometryFingerprint.VerifyHistoricalTable(
                    historicalText, baselineHashesText, baselineModelText);
            }
            catch (FormatException exception)
            {
                error = exception.Message;
                return null;
            }

            AuthoredRun run = Run(set, lineageText, decisionsText);
            try
            {
                return PairReview.Render(PairReview.Build(run, historical, baselineModelText));
            }
            catch (FormatException exception)
            {
                error = "Differentiel 5.50 refuse : " + exception.Message;
                return null;
            }
        }

        [MenuItem("RoadRage/Traffic V2/Generer le differentiel 5.50")]
        public static void GenerateReviewDiffMenu()
        {
            WithMvpRun(delegate(Scene scene)
            {
                string error;
                string text = BuildReviewDiff(
                    V1SourceSet.Extract(scene),
                    ReadIfExists(MigrationReport.LineageFullPath),
                    ReadIfExists(FullPath(DecisionsPath)),
                    ReadIfExists(FullPath(PairGeometryFingerprint.HistoricalPath)),
                    ReadIfExists(FullPath(PairGeometryFingerprint.BaselineHashesPath)),
                    ReadIfExists(FullPath(PairGeometryFingerprint.BaselineModelPath)),
                    out error);
                if (text == null || !TryWriteAll(new[]
                    { new KeyValuePair<string, string>(FullPath(ReviewDiffPath), text) }, out error))
                {
                    Debug.LogError("[Traffic V2] Differentiel 5.50 refuse, rien n'est ecrit : " + error);
                    return;
                }

                Debug.Log("[Traffic V2] Differentiel 5.50 ecrit uniquement dans " + ReviewDiffPath + ".");
            });
        }

        /// <summary>L'amorce n'ecrit jamais sur un fichier existant : message de refus, ou nul si la destination est libre.</summary>
        public static string ProposeRefusal(string destinationFullPath)
        {
            return File.Exists(destinationFullPath)
                ? destinationFullPath + " existe deja : l'amorce n'ecrit jamais sur un fichier existant, rien n'est ecrit."
                : null;
        }

        [MenuItem("RoadRage/Traffic V2/Proposer les decisions d'authoring")]
        public static void ProposeDecisionsMenu()
        {
            string refusal = ProposeRefusal(FullPath(DecisionsPath));
            if (refusal != null)
            {
                Debug.LogError("[Traffic V2] " + refusal);
                return;
            }

            WithMvpRun(delegate(Scene scene)
            {
                string error;
                string text = ProposeText(V1SourceSet.Extract(scene), ReadIfExists(MigrationReport.LineageFullPath), out error);
                if (text == null || !TryWriteAll(new[] { new KeyValuePair<string, string>(FullPath(DecisionsPath), text) }, out error))
                {
                    Debug.LogError("[Traffic V2] Amorce refusee, rien n'est ecrit :\n" + error);
                    return;
                }

                AssetDatabase.ImportAsset(DecisionsPath);
                Debug.Log("[Traffic V2] Decisions amorcees : " + DecisionsPath + ". A relire avant de compiler.");
            });
        }

        [MenuItem("RoadRage/Traffic V2/Compiler le modele authore")]
        public static void CompileAuthoredModelMenu()
        {
            WithMvpRun(delegate(Scene scene)
            {
                var run = Run(scene, ReadIfExists(MigrationReport.LineageFullPath), ReadIfExists(FullPath(DecisionsPath)));
                string error;
                if (!TryWrite(run, FullPath(ModelPath), FullPath(OverlayPath), FullPath(ReportPath), out error))
                {
                    Debug.LogError("[Traffic V2] " + error);
                    return;
                }

                AssetDatabase.ImportAsset(ModelPath);
                Debug.Log("[Traffic V2] Modele authore ecrit : " + ModelPath + ", " + OverlayPath + " et " + ReportPath + " (" + run.Compiled.Version + ").");
            });
        }

        /// <summary>
        /// Ecriture fail-closed : un passage en echec n'ecrit RIEN. Sinon les trois textes, complets en
        /// memoire, passent par des temporaires puis remplacent les destinations.
        /// ponytail: trois fichiers ne sont pas atomiques ensemble ; un arret entre deux remplacements
        /// laisse un triplet que la verification rejette (hashes), relancer le menu le repare.
        /// </summary>
        public static bool TryWrite(AuthoredRun run, string modelFile, string overlayFile, string reportFile, out string error)
        {
            if (run == null || !run.Succeeded)
            {
                error = "Compilation authoree refusee, rien n'est ecrit :\n- " + (run == null ? "aucun passage" : string.Join("\n- ", run.Failures.ToArray()));
                return false;
            }

            return TryWriteAll(new[]
            {
                new KeyValuePair<string, string>(modelFile, run.ModelText),
                new KeyValuePair<string, string>(overlayFile, run.OverlayText),
                new KeyValuePair<string, string>(reportFile, run.ReportText)
            }, out error);
        }

        public static bool TryWriteAll(IList<KeyValuePair<string, string>> files, out string error)
        {
            var encoding = new UTF8Encoding(false);
            string temp = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp");
            Directory.CreateDirectory(temp);
            var temps = new List<string>();
            try
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string path = Path.Combine(temp, "rrs-authoring-" + i + ".tmp");
                    temps.Add(path);
                    File.WriteAllText(path, files[i].Value, encoding);
                }

                for (int i = 0; i < files.Count; i++)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(files[i].Key));
                    if (File.Exists(files[i].Key))
                    {
                        File.Replace(temps[i], files[i].Key, null);
                    }
                    else
                    {
                        File.Move(temps[i], files[i].Key);
                    }
                }
            }
            catch (IOException exception)
            {
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
                foreach (var path in temps)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }

            error = null;
            return true;
        }

        public static string ReadIfExists(string fullPath)
        {
            return File.Exists(fullPath) ? File.ReadAllText(fullPath) : null;
        }

        /// <summary>Garde de scene du menu 5.27 : pas de Play Mode, pas de MVP_Run modifie ; ouverture additive puis fermeture.</summary>
        public static void WithMvpRun(Action<Scene> body)
        {
            var open = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
            bool inHierarchy = open.IsValid();
            bool wasLoaded = inHierarchy && open.isLoaded;
            string refusal = MigrationReport.RefusalReason(EditorApplication.isPlayingOrWillChangePlaymode, wasLoaded, inHierarchy && open.isDirty);
            if (refusal != null)
            {
                Debug.LogError(refusal);
                return;
            }

            var scene = wasLoaded ? open : EditorSceneManager.OpenScene(MigrationReport.ScenePath, OpenSceneMode.Additive);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("[Traffic V2] MVP_Run illisible : rien n'est ecrit.");
                return;
            }

            try
            {
                body(scene);
            }
            finally
            {
                if (!wasLoaded)
                {
                    EditorSceneManager.CloseScene(scene, !inHierarchy);
                }
            }
        }
    }
}
#endif
