#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    // =====================================================================================
    // Story 5.28 -- decisions d'authoring humaines, seules authoritatives.
    //
    // Le fichier porte ce que V1 ne sait pas dire : un controle par approche (seul `Uncontrolled`
    // admis), la decision sur chaque candidat de conflit, les largeurs revues et la disposition
    // explicite de chaque autre tache 5.27. Controles et zones sont des records AUTHORES : leurs
    // RoadId sont frappes une fois, ici, jamais par la lignee V1. Le fichier est trie ; Parse est
    // strict et ne repare rien.
    // =====================================================================================

    public enum ConflictDecisionKind
    {
        Accepted = 0,
        Rejected = 1
    }

    /// <summary>Classe de preuve qui autorise une decision de paire format 4.</summary>
    public enum AutomatedPairClassification
    {
        Following = 0,
        ConflictProven = 1,
        ProvenDisjoint = 2,
        ConservativeConflict = 3
    }

    /// <summary>Disposition d'une tache que ni un controle, ni un conflit, ni une largeur ne dispose.</summary>
    public enum TaskDispositionKind
    {
        /// <summary>Frontiere ou portail : revu a l'overlay Gate A.</summary>
        Reviewed = 0,

        /// <summary>Ligne : aucune ligne sous `Uncontrolled` ; reouverture 5.35.</summary>
        NotRequiredForCurrentControlKind = 1,

        /// <summary>Signal : carrefour declare non signalise, sans plan.</summary>
        Unsignalized = 2,

        /// <summary>Section : champs differes, chacun avec sa reouverture.</summary>
        Deferred = 3
    }

    /// <summary>
    /// Regle d'application d'une largeur revue (Story 5.49). Une section n'admet que
    /// <see cref="Uniform"/> ; un carrefour en <see cref="EndpointInterpolation"/> interpole ses
    /// mouvements entre les largeurs appliquees de leurs corridors d'extremite, sa decision valant
    /// plancher.
    /// </summary>
    public enum WidthApplication
    {
        Uniform = 0,
        EndpointInterpolation = 1
    }

    public enum DeferredFieldKind
    {
        SpeedLimit = 0,
        AllowedVehicleClasses = 1,
        Surface = 2
    }

    public struct ControlDecision
    {
        public RoadId Id;

        /// <summary>Cle de lignee du corridor d'approche.</summary>
        public string ApproachKey;

        public JunctionControlKind Kind;
    }

    public struct ConflictDecision
    {
        /// <summary>Identite de la zone materialisee ; vide pour un rejet.</summary>
        public RoadId Id;

        /// <summary>Cles de lignee des deux mouvements, A &lt; B en ordre ordinal.</summary>
        public string MovementKeyA;

        public string MovementKeyB;
        public ConflictDecisionKind Decision;
        public string Reason;

        /// <summary>Empreinte de geometrie et de volume explicitement reconfirmee par le proprietaire.</summary>
        public string GeometryFingerprint;

        /// <summary>Identites et preuve de la revision 5.50 qui porte cette decision.</summary>
        public string DecisionRevisionId;
        public string EvidenceHash;
        public string DecisionRunId;
        public string SupersedesDecisionRevisionId;
        public AutomatedPairClassification Classification;
        public string ModelVersion;
        public int ImporterVersion;
        public int PipelineVersion;
        public int CompilerSchemaVersion;
        public int FingerprintSchemaVersion;
        public int ConflictSweepAlgorithmVersion;
        public int DecisionPolicyVersion;
    }

    public struct WidthDecision
    {
        /// <summary>Cle de lignee d'une section ou d'un carrefour.</summary>
        public string SubjectKey;

        public float HalfWidthLeftMeters;
        public float HalfWidthRightMeters;

        /// <summary>Regle d'application (5.49) ; plancher en <see cref="WidthApplication.EndpointInterpolation"/>.</summary>
        public WidthApplication Application;
    }

    public struct TaskDisposition
    {
        public string Category;
        public string SubjectKey;
        public TaskDispositionKind Kind;
        public string Note;
    }

    public struct DeferredField
    {
        public DeferredFieldKind Field;
        public string Reopening;
    }

    /// <summary>Candidat de conflit genere hors ligne (voir <see cref="AuthoredRoadModel.Candidates"/>).</summary>
    public struct ConflictCandidate
    {
        public RoadId JunctionId;

        /// <summary>Paire ordonnee par RoadId : <c>MovementA &lt; MovementB</c>.</summary>
        public RoadId MovementA;

        public RoadId MovementB;
        public RoadBoundsBox Volume;
        public string GeometryFingerprint;
        public bool Following;
    }

    public sealed class AuthoringDecisions
    {
        public const int FormatVersion = 4;
        public const int HistoricalFormatVersion = 3;
        public const int LegacyFormatVersion = 2;

        /// <summary>Categories 5.27 disposees par leurs donnees typees, jamais par une disposition libre.</summary>
        public const string ControlCategory = "Controle";
        public const string ConflictCategory = "Conflit";
        public const string WidthCategory = "Largeur";

        public readonly List<ControlDecision> Controls = new List<ControlDecision>();
        public readonly List<ConflictDecision> Conflicts = new List<ConflictDecision>();
        public readonly List<WidthDecision> Widths = new List<WidthDecision>();
        public readonly List<TaskDisposition> Dispositions = new List<TaskDisposition>();
        public readonly List<DeferredField> DeferredFields = new List<DeferredField>();

        /// <summary>Disposition attendue d'une categorie de tache libre ; nul = categorie non disposable ici.</summary>
        public static TaskDispositionKind? ExpectedKind(string category)
        {
            switch (category)
            {
                case "Frontiere":
                case "Portail":
                    return TaskDispositionKind.Reviewed;
                case "Ligne":
                    return TaskDispositionKind.NotRequiredForCurrentControlKind;
                case "Signal":
                    return TaskDispositionKind.Unsignalized;
                case "Section":
                    return TaskDispositionKind.Deferred;
                default:
                    return null;
            }
        }

        // ================================================================== lecture

        /// <exception cref="FormatException">Fichier vide, illisible, non trie ou incoherent.</exception>
        public static AuthoringDecisions Parse(string json)
        {
            if (json == null || json.Trim().Length == 0)
            {
                throw new FormatException("Decisions d'authoring vides ou absentes.");
            }

            FileLayout layout;
            try
            {
                layout = JsonUtility.FromJson<FileLayout>(json);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException("Decisions illisibles : " + exception.Message);
            }

            if (layout == null || (layout.Format != FormatVersion && layout.Format != HistoricalFormatVersion
                && layout.Format != LegacyFormatVersion))
            {
                throw new FormatException("Decisions : format absent ou inconnu.");
            }

            var decisions = new AuthoringDecisions();
            var ids = new HashSet<RoadId>();

            foreach (var record in layout.Controls ?? new ControlRecord[0])
            {
                var control = new ControlDecision();
                control.Id = RequiredId(record.Id, "controle " + record.ApproachKey);
                control.ApproachKey = Required(record.ApproachKey, "Controls.ApproachKey");
                control.Kind = ParseEnum<JunctionControlKind>(record.Kind, "controle " + record.ApproachKey);
                if (!ids.Add(control.Id))
                {
                    throw new FormatException("Decisions : identifiant " + control.Id + " porte deux fois.");
                }

                decisions.Controls.Add(control);
            }

            Ordered(decisions.Controls, delegate(ControlDecision c) { return c.ApproachKey; }, "Controls");

            foreach (var record in layout.Conflicts ?? new ConflictRecord[0])
            {
                var conflict = new ConflictDecision();
                conflict.MovementKeyA = Required(record.MovementKeyA, "Conflicts.MovementKeyA");
                conflict.MovementKeyB = Required(record.MovementKeyB, "Conflicts.MovementKeyB");
                string pair = conflict.MovementKeyA + " x " + conflict.MovementKeyB;
                if (string.CompareOrdinal(conflict.MovementKeyA, conflict.MovementKeyB) >= 0)
                {
                    throw new FormatException("Decisions : paire de conflit non ordonnee (A < B attendu) : " + pair + ".");
                }

                conflict.Decision = ParseEnum<ConflictDecisionKind>(record.Decision, "conflit " + pair);
                conflict.Reason = record.Reason ?? string.Empty;
                conflict.GeometryFingerprint = layout.Format >= HistoricalFormatVersion
                    ? OptionalFingerprint(record.GeometryFingerprint, pair)
                    : string.Empty;
                if (layout.Format == FormatVersion)
                {
                    conflict.DecisionRevisionId = RequiredSha256(record.DecisionRevisionId, "revision " + pair);
                    conflict.EvidenceHash = RequiredSha256(record.EvidenceHash, "preuve " + pair);
                    conflict.DecisionRunId = RequiredSha256(record.DecisionRunId, "run " + pair);
                    conflict.SupersedesDecisionRevisionId = OptionalSha256(record.SupersedesDecisionRevisionId, "revision remplacee " + pair);
                    conflict.Classification = ParseEnum<AutomatedPairClassification>(record.Classification, "classification " + pair);
                    conflict.ModelVersion = Required(record.ModelVersion, "Conflicts.ModelVersion");
                    conflict.ImporterVersion = PositiveVersion(record.ImporterVersion, "ImporterVersion " + pair);
                    conflict.PipelineVersion = PositiveVersion(record.PipelineVersion, "PipelineVersion " + pair);
                    conflict.CompilerSchemaVersion = PositiveVersion(record.CompilerSchemaVersion, "CompilerSchemaVersion " + pair);
                    conflict.FingerprintSchemaVersion = PositiveVersion(record.FingerprintSchemaVersion, "FingerprintSchemaVersion " + pair);
                    conflict.ConflictSweepAlgorithmVersion = PositiveVersion(record.ConflictSweepAlgorithmVersion, "ConflictSweepAlgorithmVersion " + pair);
                    conflict.DecisionPolicyVersion = PositiveVersion(record.DecisionPolicyVersion, "DecisionPolicyVersion " + pair);
                    if ((conflict.Decision == ConflictDecisionKind.Rejected) != (conflict.Classification == AutomatedPairClassification.ProvenDisjoint)
                        || conflict.Classification == AutomatedPairClassification.Following)
                    {
                        throw new FormatException("Decisions : classification incompatible avec la decision " + pair + ".");
                    }
                }
                if (conflict.Decision == ConflictDecisionKind.Accepted)
                {
                    conflict.Id = RequiredId(record.Id, "conflit " + pair);
                    if (!ids.Add(conflict.Id))
                    {
                        throw new FormatException("Decisions : identifiant " + conflict.Id + " porte deux fois.");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(record.Id))
                    {
                        throw new FormatException("Decisions : un rejet ne materialise aucune zone et ne porte pas d'identifiant (" + pair + ").");
                    }

                    if (conflict.Reason.Trim().Length == 0)
                    {
                        throw new FormatException("Decisions : rejet sans motif (" + pair + ").");
                    }
                }

                decisions.Conflicts.Add(conflict);
            }

            Ordered(decisions.Conflicts, delegate(ConflictDecision c) { return c.MovementKeyA + "\n" + c.MovementKeyB; }, "Conflicts");

            foreach (var record in layout.Widths ?? new WidthRecord[0])
            {
                var width = new WidthDecision();
                width.SubjectKey = Required(record.SubjectKey, "Widths.SubjectKey");
                width.HalfWidthLeftMeters = Positive(record.HalfWidthLeftMeters, "largeur gauche de " + width.SubjectKey);
                width.HalfWidthRightMeters = Positive(record.HalfWidthRightMeters, "largeur droite de " + width.SubjectKey);
                width.Application = ParseEnum<WidthApplication>(record.Application, "application de largeur de " + width.SubjectKey);
                decisions.Widths.Add(width);
            }

            Ordered(decisions.Widths, delegate(WidthDecision w) { return w.SubjectKey; }, "Widths");

            foreach (var record in layout.Dispositions ?? new DispositionRecord[0])
            {
                var disposition = new TaskDisposition();
                disposition.Category = Required(record.Category, "Dispositions.Category");
                disposition.SubjectKey = Required(record.SubjectKey, "Dispositions.SubjectKey");
                disposition.Kind = ParseEnum<TaskDispositionKind>(record.Kind, "disposition " + record.Category + " " + record.SubjectKey);
                disposition.Note = Required(record.Note, "Dispositions.Note");
                decisions.Dispositions.Add(disposition);
            }

            Ordered(decisions.Dispositions, delegate(TaskDisposition d) { return d.Category + "\n" + d.SubjectKey; }, "Dispositions");

            foreach (var record in layout.DeferredFields ?? new DeferredRecord[0])
            {
                var field = new DeferredField();
                field.Field = ParseEnum<DeferredFieldKind>(record.Field, "DeferredFields.Field");
                field.Reopening = Required(record.Reopening, "DeferredFields.Reopening");
                decisions.DeferredFields.Add(field);
            }

            Ordered(decisions.DeferredFields, delegate(DeferredField f) { return f.Field.ToString(); }, "DeferredFields");
            return decisions;
        }

        /// <summary>Serialisation deterministe : chaque liste triee, LF, terminee par un saut de ligne.</summary>
        public string Serialize()
        {
            return Serialize(FormatVersion);
        }

        /// <summary>Compatibilite de calcul en memoire seulement ; un format historique n'est jamais persiste.</summary>
        internal string SerializeHistoricalForReadOnlyMigration()
        {
            return Serialize(HistoricalFormatVersion);
        }

        private string Serialize(int format)
        {
            Sort();
            var layout = new FileLayout();
            layout.Format = format;
            layout.Controls = Controls.ConvertAll(delegate(ControlDecision c)
            {
                return new ControlRecord { Id = c.Id.ToString(), ApproachKey = c.ApproachKey, Kind = c.Kind.ToString() };
            }).ToArray();
            layout.Conflicts = Conflicts.ConvertAll(delegate(ConflictDecision c)
            {
                return new ConflictRecord
                {
                    Id = c.Decision == ConflictDecisionKind.Accepted ? c.Id.ToString() : string.Empty,
                    MovementKeyA = c.MovementKeyA,
                    MovementKeyB = c.MovementKeyB,
                    Decision = c.Decision.ToString(),
                    Reason = c.Reason,
                    GeometryFingerprint = c.GeometryFingerprint,
                    DecisionRevisionId = c.DecisionRevisionId,
                    EvidenceHash = c.EvidenceHash,
                    DecisionRunId = c.DecisionRunId,
                    SupersedesDecisionRevisionId = c.SupersedesDecisionRevisionId,
                    Classification = c.Classification.ToString(),
                    ModelVersion = c.ModelVersion,
                    ImporterVersion = c.ImporterVersion,
                    PipelineVersion = c.PipelineVersion,
                    CompilerSchemaVersion = c.CompilerSchemaVersion,
                    FingerprintSchemaVersion = c.FingerprintSchemaVersion,
                    ConflictSweepAlgorithmVersion = c.ConflictSweepAlgorithmVersion,
                    DecisionPolicyVersion = c.DecisionPolicyVersion
                };
            }).ToArray();
            layout.Widths = Widths.ConvertAll(delegate(WidthDecision w)
            {
                return new WidthRecord
                {
                    SubjectKey = w.SubjectKey,
                    HalfWidthLeftMeters = w.HalfWidthLeftMeters,
                    HalfWidthRightMeters = w.HalfWidthRightMeters,
                    Application = w.Application.ToString()
                };
            }).ToArray();
            layout.Dispositions = Dispositions.ConvertAll(delegate(TaskDisposition d)
            {
                return new DispositionRecord { Category = d.Category, SubjectKey = d.SubjectKey, Kind = d.Kind.ToString(), Note = d.Note };
            }).ToArray();
            layout.DeferredFields = DeferredFields.ConvertAll(delegate(DeferredField f)
            {
                return new DeferredRecord { Field = f.Field.ToString(), Reopening = f.Reopening };
            }).ToArray();
            return JsonUtility.ToJson(layout, true).Replace("\r\n", "\n") + "\n";
        }

        private void Sort()
        {
            Controls.Sort(delegate(ControlDecision a, ControlDecision b) { return string.CompareOrdinal(a.ApproachKey, b.ApproachKey); });
            Conflicts.Sort(delegate(ConflictDecision a, ConflictDecision b)
            {
                int byA = string.CompareOrdinal(a.MovementKeyA, b.MovementKeyA);
                return byA != 0 ? byA : string.CompareOrdinal(a.MovementKeyB, b.MovementKeyB);
            });
            Widths.Sort(delegate(WidthDecision a, WidthDecision b) { return string.CompareOrdinal(a.SubjectKey, b.SubjectKey); });
            Dispositions.Sort(delegate(TaskDisposition a, TaskDisposition b)
            {
                int byCategory = string.CompareOrdinal(a.Category, b.Category);
                return byCategory != 0 ? byCategory : string.CompareOrdinal(a.SubjectKey, b.SubjectKey);
            });
            DeferredFields.Sort(delegate(DeferredField a, DeferredField b) { return string.CompareOrdinal(a.Field.ToString(), b.Field.ToString()); });
        }

        // ================================================================== amorce

        /// <summary>
        /// Amorce des decisions depuis un import et ses candidats, appliquant les decisions du
        /// proprietaire (2026-09-23) : un controle `Uncontrolled` par approche, chaque candidat
        /// accepte, largeurs importees reprises pour revue, dispositions et champs differes explicites.
        /// Ne lit ni n'ecrit aucun fichier : l'ecriture (jamais sur un fichier existant) est au menu.
        /// </summary>
        /// <exception cref="InvalidOperationException">Largeur non uniforme ou categorie de tache sans disposition connue.</exception>
        public static AuthoringDecisions Propose(V1ImportResult import, IList<ConflictCandidate> candidates)
        {
            var decisions = new AuthoringDecisions();
            var approaches = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var movement in import.Movements)
            {
                approaches.Add(movement.From.Key);
            }

            foreach (var approach in approaches)
            {
                decisions.Controls.Add(new ControlDecision { Id = RoadId.New(), ApproachKey = approach, Kind = JunctionControlKind.Uncontrolled });
            }

            var keyById = KeysById(import);
            foreach (var candidate in candidates)
            {
                string a = keyById[candidate.MovementA];
                string b = keyById[candidate.MovementB];
                decisions.Conflicts.Add(new ConflictDecision
                {
                    Id = RoadId.New(),
                    MovementKeyA = string.CompareOrdinal(a, b) < 0 ? a : b,
                    MovementKeyB = string.CompareOrdinal(a, b) < 0 ? b : a,
                    Decision = ConflictDecisionKind.Accepted,
                    GeometryFingerprint = candidate.GeometryFingerprint,
                    Reason = "Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee."
                });
            }

            foreach (var section in import.Sections)
            {
                var samples = new List<RoadCurveSample>();
                foreach (var corridor in section.Corridors)
                {
                    samples.AddRange(corridor.Samples);
                }

                decisions.Widths.Add(UniformWidth(section.Key, samples));
            }

            foreach (var junction in import.Junctions)
            {
                var samples = new List<RoadCurveSample>();
                foreach (var movement in junction.Movements)
                {
                    samples.AddRange(movement.Samples);
                }

                decisions.Widths.Add(UniformWidth(junction.Key, samples));
            }

            foreach (var task in import.Tasks)
            {
                if (task.Category == ControlCategory || task.Category == ConflictCategory || task.Category == WidthCategory)
                {
                    continue;
                }

                var kind = ExpectedKind(task.Category);
                if (kind == null)
                {
                    throw new InvalidOperationException("Categorie de tache sans disposition connue : " + task.Category + " (" + task.SubjectKey + "). Decision humaine requise.");
                }

                decisions.Dispositions.Add(new TaskDisposition { Category = task.Category, SubjectKey = task.SubjectKey, Kind = kind.Value, Note = Note(kind.Value) });
            }

            decisions.DeferredFields.Add(new DeferredField { Field = DeferredFieldKind.SpeedLimit, Reopening = "Story 5.33 (vitesse limite authoree)." });
            decisions.DeferredFields.Add(new DeferredField { Field = DeferredFieldKind.AllowedVehicleClasses, Reopening = "Admission d'une seconde classe de vehicule : recompilation (AD-44)." });
            decisions.DeferredFields.Add(new DeferredField { Field = DeferredFieldKind.Surface, Reopening = "Introduction d'une seconde surface roulable." });
            decisions.Sort();
            return decisions;
        }

        private static string Note(TaskDispositionKind kind)
        {
            switch (kind)
            {
                case TaskDispositionKind.Reviewed:
                    return "Revue a l'overlay Gate A (sign-off du proprietaire).";
                case TaskDispositionKind.NotRequiredForCurrentControlKind:
                    return "Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority.";
                case TaskDispositionKind.Unsignalized:
                    return "Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite.";
                default:
                    return "Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee.";
            }
        }

        /// <summary>Demi-largeurs d'un sujet : uniformes sur tous ses echantillons, sinon echec nommant le sujet.</summary>
        private static WidthDecision UniformWidth(string key, List<RoadCurveSample> samples)
        {
            if (samples.Count == 0)
            {
                throw new InvalidOperationException("Largeur de " + key + " : aucun echantillon.");
            }

            float left = samples[0].HalfWidthLeftMeters;
            float right = samples[0].HalfWidthRightMeters;
            foreach (var sample in samples)
            {
                if (!SameWidth(sample.HalfWidthLeftMeters, left) || !SameWidth(sample.HalfWidthRightMeters, right))
                {
                    throw new InvalidOperationException("Largeur de " + key + " non uniforme : aucune valeur unique a proposer, decision humaine requise.");
                }
            }

            // Valeur revue lisible : arrondie au pas canonique, donc egale a l'import a la quantification pres.
            return new WidthDecision { SubjectKey = key, HalfWidthLeftMeters = Canonical(left), HalfWidthRightMeters = Canonical(right), Application = WidthApplication.Uniform };
        }

        private static float Canonical(float meters)
        {
            return (float)(V1SourceSet.Q(meters, RoadModelCanonicalWriter.MeterStep) * RoadModelCanonicalWriter.MeterStep);
        }

        /// <summary>Egalite a la quantification canonique pres (pas metrique du writer).</summary>
        public static bool SameWidth(float a, float b)
        {
            return V1SourceSet.Q(a, RoadModelCanonicalWriter.MeterStep) == V1SourceSet.Q(b, RoadModelCanonicalWriter.MeterStep);
        }

        public static Dictionary<RoadId, string> KeysById(V1ImportResult import)
        {
            var keys = new Dictionary<RoadId, string>();
            foreach (var pair in import.IdByKey)
            {
                keys[pair.Value] = pair.Key;
            }

            return keys;
        }

        // ================================================================== outils de parse

        /// <summary>
        /// Empreinte reconfirmee par le proprietaire : vide = non reconfirmee (proposition historique),
        /// sinon un SHA-256 hexadecimal minuscule. Une action du proprietaire reecrit tout le fichier
        /// en format 3 alors que les autres paires ne sont pas encore reconfirmees.
        /// </summary>
        private static string OptionalFingerprint(string value, string pair)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (!PairGeometryFingerprint.IsSha256(value))
            {
                throw new FormatException("Decisions : empreinte de geometrie invalide (SHA-256 hexadecimal attendu) : " + pair + ".");
            }

            return value;
        }

        private static string RequiredSha256(string value, string what)
        {
            if (!PairGeometryFingerprint.IsSha256(value))
            {
                throw new FormatException("Decisions : SHA-256 invalide pour " + what + ".");
            }

            return value;
        }

        private static string OptionalSha256(string value, string what)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : RequiredSha256(value, what);
        }

        private static int PositiveVersion(int value, string what)
        {
            if (value <= 0)
            {
                throw new FormatException("Decisions : version non positive pour " + what + ".");
            }

            return value;
        }

        private static string Required(string value, string what)
        {
            if (string.IsNullOrEmpty(value) || value.Trim().Length == 0)
            {
                throw new FormatException("Decisions : champ vide " + what + ".");
            }

            return value;
        }

        private static RoadId RequiredId(string hex, string what)
        {
            RoadId id;
            if (!RoadId.TryParse(hex, out id) || id.IsEmpty)
            {
                throw new FormatException("Decisions : identifiant illisible pour " + what + ".");
            }

            return id;
        }

        private static float Positive(float value, string what)
        {
            if (!(value > 0f) || float.IsInfinity(value))
            {
                throw new FormatException("Decisions : " + what + " non strictement positive ou non finie.");
            }

            return value;
        }

        private static T ParseEnum<T>(string name, string what) where T : struct
        {
            T value;
            if (!RoadModelDocument.TryParseDeclaredEnum(name, out value))
            {
                throw new FormatException("Decisions : valeur inconnue '" + name + "' pour " + what + ".");
            }

            return value;
        }

        /// <summary>Strictement croissant : une liste non triee ou une cle en double est un refus.</summary>
        private static void Ordered<T>(List<T> items, Func<T, string> key, string list)
        {
            for (int i = 1; i < items.Count; i++)
            {
                if (string.CompareOrdinal(key(items[i - 1]), key(items[i])) >= 0)
                {
                    throw new FormatException("Decisions : " + list + " non trie ou cle en double ('" + key(items[i]).Replace("\n", " x ") + "').");
                }
            }
        }

        // ================================================================== disposition du fichier

        [Serializable]
        private sealed class FileLayout
        {
            /// <summary>Sans initialiseur : un champ absent se lit 0 et est rejete.</summary>
            public int Format;
            public ControlRecord[] Controls;
            public ConflictRecord[] Conflicts;
            public WidthRecord[] Widths;
            public DispositionRecord[] Dispositions;
            public DeferredRecord[] DeferredFields;
        }

        [Serializable]
        private sealed class ControlRecord
        {
            public string Id;
            public string ApproachKey;
            public string Kind;
        }

        [Serializable]
        private sealed class ConflictRecord
        {
            public string Id;
            public string MovementKeyA;
            public string MovementKeyB;
            public string Decision;
            public string Reason;
            public string GeometryFingerprint;
            public string DecisionRevisionId;
            public string EvidenceHash;
            public string DecisionRunId;
            public string SupersedesDecisionRevisionId;
            public string Classification;
            public string ModelVersion;
            public int ImporterVersion;
            public int PipelineVersion;
            public int CompilerSchemaVersion;
            public int FingerprintSchemaVersion;
            public int ConflictSweepAlgorithmVersion;
            public int DecisionPolicyVersion;
        }

        [Serializable]
        private sealed class WidthRecord
        {
            public string SubjectKey;
            public float HalfWidthLeftMeters;
            public float HalfWidthRightMeters;
            public string Application;
        }

        [Serializable]
        private sealed class DispositionRecord
        {
            public string Category;
            public string SubjectKey;
            public string Kind;
            public string Note;
        }

        [Serializable]
        private sealed class DeferredRecord
        {
            public string Field;
            public string Reopening;
        }
    }
}
#endif
