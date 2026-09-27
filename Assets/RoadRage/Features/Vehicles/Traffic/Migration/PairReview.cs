#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    public enum PairReviewStatus
    {
        Unchanged,
        Modified,
        New,
        Removed
    }

    public enum PairDecisionState
    {
        /// <summary>Aucune decision pour cette paire.</summary>
        Missing,

        /// <summary>Decision sans empreinte : proposition historique non reconfirmee.</summary>
        Unconfirmed,

        /// <summary>Empreinte decidee differente de l'empreinte fraiche.</summary>
        Stale,

        /// <summary>Empreinte decidee egale a l'empreinte fraiche.</summary>
        Confirmed,

        /// <summary>Decision d'une paire qui n'est plus candidate : a disposer explicitement.</summary>
        Orphan
    }

    /// <summary>Un ecart du differentiel 5.50, avec tout ce que la revue du proprietaire affiche.</summary>
    public sealed class PairReviewEntry
    {
        public string PairKey;
        public string KeyA;
        public string KeyB;
        public PairReviewStatus Status;

        public RoadId JunctionId;
        public string JunctionLabel;
        public RoadId MovementA;
        public RoadId MovementB;
        public string LabelA;
        public string LabelB;

        public IReadOnlyList<RoadCurveSample> OldSamplesA;
        public IReadOnlyList<RoadCurveSample> OldSamplesB;
        public IReadOnlyList<RoadCurveSample> NewSamplesA;
        public IReadOnlyList<RoadCurveSample> NewSamplesB;

        public bool HasOldVolume;
        public RoadBoundsBox OldVolume;
        public bool HasNewVolume;
        public RoadBoundsBox NewVolume;

        public string OldFingerprint;
        public string NewFingerprint;
        public string HistoricalDecision;

        public bool GeometryAChanged;
        public bool GeometryBChanged;
        public bool VolumeChanged;

        public bool HasDecision;
        public ConflictDecision Decision;
        public PairDecisionState DecisionState;

        /// <summary>Balayage frais de la paire, y compris pour une paire retiree ; nul si un mouvement a disparu.</summary>
        public PairSweep Sweep;

        public string Reason;
    }

    public sealed class PairReviewModel
    {
        public readonly List<PairReviewEntry> Entries = new List<PairReviewEntry>();
        public readonly List<PairSweep> Following = new List<PairSweep>();
        public readonly List<PairSweep> EnvelopeOnly = new List<PairSweep>();
        public readonly List<PairSweep> FailClosed = new List<PairSweep>();
        public readonly List<ShortElement> ShortElements = new List<ShortElement>();
        public readonly Dictionary<RoadId, string> Keys = new Dictionary<RoadId, string>();
        public string SourceHash;
        public string HistoricalModelVersion;
        public int HistoricalSchema;
        public int HistoricalPairs;
        public int FreshPairs;
        public CompiledRoadModel Model;
        public V1ImportResult Import;

        public int Count(PairReviewStatus status)
        {
            int count = 0;
            foreach (var entry in Entries)
            {
                if (entry.Status == status)
                {
                    count++;
                }
            }

            return count;
        }

        public PairReviewEntry Find(string pairKey)
        {
            return Entries.Find(delegate(PairReviewEntry e) { return e.PairKey == pairKey; });
        }

        public string Label(RoadId id)
        {
            CompiledJunctionMovement movement;
            if (Model != null && Model.TryGetMovement(id, out movement))
            {
                return movement.Label;
            }

            string key;
            return Keys.TryGetValue(id, out key) ? key : id.ToString();
        }
    }

    /// <summary>
    /// Lecture isolee des trajectoires du modele committe avant la 5.50 (format 1). Donnee de revue
    /// Editeur seulement : ne produit jamais de <see cref="RoadModelSource"/> ni de
    /// <see cref="CompiledRoadModel"/> et n'est jamais lue par le chargement, le compilateur ou le runtime.
    /// </summary>
    public sealed class HistoricalMovementReader
    {
        public const int Format = 1;

        public readonly Dictionary<RoadId, string> JunctionLabels = new Dictionary<RoadId, string>();
        public readonly Dictionary<RoadId, HistoricalMovement> Movements = new Dictionary<RoadId, HistoricalMovement>();

        public static HistoricalMovementReader Parse(string baselineModelText)
        {
            Document document;
            try
            {
                document = JsonUtility.FromJson<Document>(baselineModelText);
            }
            catch (Exception exception)
            {
                throw new FormatException("Modele historique 5.50 illisible : " + exception.Message);
            }

            if (document == null || document.Format != Format || document.Model == null)
            {
                throw new FormatException("Modele historique 5.50 : format " + Format + " attendu.");
            }

            var reader = new HistoricalMovementReader();
            foreach (var junction in document.Model.Junctions ?? new JunctionRecord[0])
            {
                RoadId id;
                if (RoadId.TryParse(junction.Id, out id))
                {
                    reader.JunctionLabels[id] = junction.Label;
                }
            }

            foreach (var movement in document.Model.Movements ?? new MovementRecord[0])
            {
                RoadId id;
                RoadId junctionId;
                if (!RoadId.TryParse(movement.Id, out id) || !RoadId.TryParse(movement.JunctionId, out junctionId))
                {
                    throw new FormatException("Modele historique 5.50 : identifiant de mouvement illisible.");
                }

                reader.Movements[id] = new HistoricalMovement
                {
                    Label = movement.Label,
                    JunctionId = junctionId,
                    Samples = movement.Samples ?? new RoadCurveSample[0]
                };
            }

            return reader;
        }

        public sealed class HistoricalMovement
        {
            public string Label;
            public RoadId JunctionId;
            public RoadCurveSample[] Samples;
        }

        [Serializable]
        private sealed class Document
        {
            public int Format;
            public ModelRecord Model;
        }

        [Serializable]
        private sealed class ModelRecord
        {
            public JunctionRecord[] Junctions;
            public MovementRecord[] Movements;
        }

        [Serializable]
        private sealed class JunctionRecord
        {
            public string Id;
            public string Label;
        }

        [Serializable]
        private sealed class MovementRecord
        {
            public string Id;
            public string Label;
            public string JunctionId;
            public RoadCurveSample[] Samples;
        }
    }

    /// <summary>
    /// Differentiel 5.50 structure : chaque paire historique ou fraiche, son statut, sa raison et son
    /// etat de decision. Lecture seule : rien n'est ecrit ici.
    /// </summary>
    public static class PairReview
    {
        public const string HistoricalReason = "Absente de la table historique : l'ancien balayage 5.28 (lateral seul, sans longueur de gabarit ni borne d'intervalle) ne la retenait pas.";

        /// <summary>
        /// Construit la revue depuis un passage du pipeline (candidats produits, rapprochement
        /// eventuellement en echec), la table historique verifiee et le modele format 1 lie a la table.
        /// </summary>
        /// <exception cref="FormatException">Passage sans candidats, echec autre que de rapprochement, ou entree historique incoherente.</exception>
        public static PairReviewModel Build(AuthoredRun run, HistoricalPairFingerprintTable historical, string baselineModelText)
        {
            if (run == null || run.Candidates == null || run.PairSweeps == null || run.CandidateModel == null
                || run.Decisions == null || run.Import == null)
            {
                throw new FormatException("Revue 5.50 refusee avant la production des candidats :\n- "
                    + (run == null ? "passage absent" : string.Join("\n- ", run.Failures.ToArray())));
            }

            foreach (var failure in run.Failures)
            {
                if (!IsReconciliationFailure(failure))
                {
                    throw new FormatException("Revue 5.50 refusee : " + failure);
                }
            }

            if (historical == null)
            {
                throw new FormatException("Revue 5.50 : table historique absente.");
            }

            HistoricalMovementReader baseline = HistoricalMovementReader.Parse(baselineModelText);
            var review = new PairReviewModel();
            review.Model = run.CandidateModel;
            review.Import = run.Import;
            review.HistoricalModelVersion = historical.SourceModelVersion;
            review.HistoricalSchema = historical.FingerprintSchemaVersion;
            review.HistoricalPairs = historical.Pairs.Length;
            review.FreshPairs = run.Candidates.Count;
            review.SourceHash = run.Import.SourceSet == null ? null : run.Import.SourceSet.SourceHash;
            foreach (var pair in AuthoringDecisions.KeysById(run.Import))
            {
                review.Keys[pair.Key] = pair.Value;
            }

            var freshByPair = new Dictionary<string, ConflictCandidate>(StringComparer.Ordinal);
            foreach (var candidate in run.Candidates)
            {
                string pair = PairKeyOf(review.Keys, candidate.MovementA, candidate.MovementB);
                if (string.IsNullOrEmpty(candidate.GeometryFingerprint) || freshByPair.ContainsKey(pair))
                {
                    throw new FormatException("Revue 5.50 : candidat sans empreinte ou paire dupliquee.");
                }

                freshByPair.Add(pair, candidate);
            }

            var sweepByPair = new Dictionary<string, PairSweep>(StringComparer.Ordinal);
            foreach (var sweep in run.PairSweeps)
            {
                sweepByPair[PairKeyOf(review.Keys, sweep.MovementA, sweep.MovementB)] = sweep;
                switch (sweep.Relation)
                {
                    case PairRelation.SameApproach:
                    case PairRelation.Following:
                        review.Following.Add(sweep);
                        break;
                    case PairRelation.EnvelopeOnly:
                        review.EnvelopeOnly.Add(sweep);
                        break;
                    case PairRelation.FailClosed:
                        review.FailClosed.Add(sweep);
                        break;
                }
            }

            var oldByPair = new Dictionary<string, HistoricalPairFingerprintRecord>(StringComparer.Ordinal);
            foreach (var record in historical.Pairs)
            {
                oldByPair.Add(AuthoredRoadModel.PairKey(record.MovementKeyA, record.MovementKeyB), record);
            }

            var decisionByPair = new Dictionary<string, ConflictDecision>(StringComparer.Ordinal);
            foreach (var decision in run.Decisions.Conflicts)
            {
                decisionByPair[AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB)] = decision;
            }

            var keys = new SortedSet<string>(StringComparer.Ordinal);
            keys.UnionWith(freshByPair.Keys);
            keys.UnionWith(oldByPair.Keys);
            foreach (var pair in keys)
            {
                ConflictCandidate candidate;
                bool fresh = freshByPair.TryGetValue(pair, out candidate);
                HistoricalPairFingerprintRecord old;
                oldByPair.TryGetValue(pair, out old);
                PairSweep sweep;
                sweepByPair.TryGetValue(pair, out sweep);
                ConflictDecision decision;
                bool hasDecision = decisionByPair.TryGetValue(pair, out decision);
                review.Entries.Add(Entry(review, baseline, pair, fresh ? (ConflictCandidate?)candidate : null, old, sweep, hasDecision, decision));
            }

            review.ShortElements.AddRange(ConflictSweep.ShortElements(run.CandidateModel));
            return review;
        }

        public static bool IsReconciliationFailure(string failure)
        {
            return failure.StartsWith("Proposition historique non reconfirmee : ", StringComparison.Ordinal)
                || failure.StartsWith("Candidat de conflit non dispose : ", StringComparison.Ordinal)
                || failure.StartsWith("Decision de conflit sans candidat : ", StringComparison.Ordinal);
        }

        private static string PairKeyOf(Dictionary<RoadId, string> keys, RoadId a, RoadId b)
        {
            return AuthoredRoadModel.PairKey(keys[a], keys[b]);
        }

        private static PairReviewEntry Entry(
            PairReviewModel review,
            HistoricalMovementReader baseline,
            string pair,
            ConflictCandidate? candidate,
            HistoricalPairFingerprintRecord old,
            PairSweep sweep,
            bool hasDecision,
            ConflictDecision decision)
        {
            string[] parts = pair.Split('\n');
            var entry = new PairReviewEntry();
            entry.PairKey = pair;
            entry.KeyA = parts[0];
            entry.KeyB = parts[1];
            entry.Sweep = sweep;
            entry.HasDecision = hasDecision;
            entry.Decision = decision;

            RoadId idA;
            RoadId idB;
            bool knownA = review.Import.IdByKey.TryGetValue(entry.KeyA, out idA);
            bool knownB = review.Import.IdByKey.TryGetValue(entry.KeyB, out idB);
            entry.MovementA = idA;
            entry.MovementB = idB;

            CompiledJunctionMovement newA;
            CompiledJunctionMovement newB;
            bool hasNewA = review.Model.TryGetMovement(idA, out newA) && knownA;
            bool hasNewB = review.Model.TryGetMovement(idB, out newB) && knownB;
            HistoricalMovementReader.HistoricalMovement oldA;
            HistoricalMovementReader.HistoricalMovement oldB;
            bool hasOldA = baseline.Movements.TryGetValue(idA, out oldA) && knownA;
            bool hasOldB = baseline.Movements.TryGetValue(idB, out oldB) && knownB;

            entry.NewSamplesA = hasNewA ? newA.Samples : null;
            entry.NewSamplesB = hasNewB ? newB.Samples : null;
            entry.OldSamplesA = hasOldA ? oldA.Samples : null;
            entry.OldSamplesB = hasOldB ? oldB.Samples : null;
            entry.LabelA = hasNewA ? newA.Label : hasOldA ? oldA.Label : entry.KeyA;
            entry.LabelB = hasNewB ? newB.Label : hasOldB ? oldB.Label : entry.KeyB;

            if (candidate.HasValue)
            {
                entry.JunctionId = candidate.Value.JunctionId;
                entry.NewFingerprint = candidate.Value.GeometryFingerprint;
                entry.HasNewVolume = true;
                entry.NewVolume = candidate.Value.Volume;
            }
            else if (sweep != null)
            {
                entry.JunctionId = sweep.JunctionId;
            }
            else if (hasOldA)
            {
                entry.JunctionId = oldA.JunctionId;
            }

            Junction junction;
            string oldJunctionLabel;
            entry.JunctionLabel = review.Model.TryGetJunction(entry.JunctionId, out junction) ? junction.Label
                : baseline.JunctionLabels.TryGetValue(entry.JunctionId, out oldJunctionLabel) ? oldJunctionLabel
                : entry.JunctionId.ToString();

            if (old != null)
            {
                entry.OldFingerprint = old.Fingerprint;
                entry.HistoricalDecision = old.Decision;
                entry.HasOldVolume = true;
                entry.OldVolume = old.Volume();
            }

            string oldHashA = old != null ? old.MovementHashA : hasOldA ? PairGeometryFingerprint.ComputeMovement(entry.KeyA, oldA.Samples) : null;
            string oldHashB = old != null ? old.MovementHashB : hasOldB ? PairGeometryFingerprint.ComputeMovement(entry.KeyB, oldB.Samples) : null;
            string newHashA = hasNewA ? PairGeometryFingerprint.ComputeMovement(entry.KeyA, newA.Samples) : null;
            string newHashB = hasNewB ? PairGeometryFingerprint.ComputeMovement(entry.KeyB, newB.Samples) : null;
            entry.GeometryAChanged = !string.Equals(oldHashA, newHashA, StringComparison.Ordinal);
            entry.GeometryBChanged = !string.Equals(oldHashB, newHashB, StringComparison.Ordinal);
            entry.VolumeChanged = entry.HasOldVolume != entry.HasNewVolume
                || (entry.HasOldVolume && !SameMillimeters(entry.OldVolume, entry.NewVolume));

            if (candidate.HasValue && old != null)
            {
                entry.Status = string.Equals(old.Fingerprint, entry.NewFingerprint, StringComparison.Ordinal)
                    ? PairReviewStatus.Unchanged
                    : PairReviewStatus.Modified;
            }
            else
            {
                entry.Status = candidate.HasValue ? PairReviewStatus.New : PairReviewStatus.Removed;
            }

            entry.DecisionState = DecisionStateOf(entry);
            entry.Reason = ReasonOf(entry);
            return entry;
        }

        private static PairDecisionState DecisionStateOf(PairReviewEntry entry)
        {
            if (!entry.HasDecision)
            {
                return PairDecisionState.Missing;
            }

            if (entry.Status == PairReviewStatus.Removed)
            {
                return PairDecisionState.Orphan;
            }

            if (string.IsNullOrEmpty(entry.Decision.GeometryFingerprint))
            {
                return PairDecisionState.Unconfirmed;
            }

            return string.Equals(entry.Decision.GeometryFingerprint, entry.NewFingerprint, StringComparison.Ordinal)
                ? PairDecisionState.Confirmed
                : PairDecisionState.Stale;
        }

        private static bool SameMillimeters(RoadBoundsBox a, RoadBoundsBox b)
        {
            var x = HistoricalPairFingerprintRecord.Create("a", "b", "Accepted", null, null, a, null);
            var y = HistoricalPairFingerprintRecord.Create("a", "b", "Accepted", null, null, b, null);
            return x.CenterXMillimeters == y.CenterXMillimeters && x.CenterYMillimeters == y.CenterYMillimeters
                && x.CenterZMillimeters == y.CenterZMillimeters && x.ExtentXMillimeters == y.ExtentXMillimeters
                && x.ExtentYMillimeters == y.ExtentYMillimeters && x.ExtentZMillimeters == y.ExtentZMillimeters;
        }

        public static string ReasonOf(PairReviewEntry entry)
        {
            string geometry = "trajectoire A " + (entry.GeometryAChanged ? "modifiee" : "inchangee")
                + ", trajectoire B " + (entry.GeometryBChanged ? "modifiee" : "inchangee");
            switch (entry.Status)
            {
                case PairReviewStatus.Unchanged:
                    return "Geometrie des deux mouvements et volume de conflit identiques a la table historique.";
                case PairReviewStatus.Modified:
                    return "Empreinte differente de la table historique : " + geometry
                        + ", volume de conflit " + (entry.VolumeChanged ? "modifie" : "inchange") + ". " + ProofOf(entry.Sweep);
                case PairReviewStatus.New:
                    return HistoricalReason + " " + Capitalize(geometry) + ". " + ProofOf(entry.Sweep);
                default:
                    return RemovalOf(entry) + " " + Capitalize(geometry) + ".";
            }
        }

        private static string RemovalOf(PairReviewEntry entry)
        {
            if (entry.Sweep == null)
            {
                return "Plus candidate : un des mouvements est absent du modele frais.";
            }

            switch (entry.Sweep.Relation)
            {
                case PairRelation.SameApproach:
                    return "Plus candidate : meme corridor d'approche (suivi, jamais un conflit).";
                case PairRelation.Following:
                    return "Plus candidate : l'un suit l'autre sur un chemin a une voie (suivi publie).";
                case PairRelation.EnvelopeOnly:
                    return "Plus candidate : retenue par les seules boites englobantes, ecartee par la distance exacte des empreintes (marge exacte "
                        + Meters(entry.Sweep.ExactSlackMeters) + ").";
                case PairRelation.NoContact:
                    return "Plus candidate : aucun contact possible entre les trajectoires fraiches prolongees (marge englobante "
                        + Meters(entry.Sweep.EnvelopeSlackMeters) + ").";
                default:
                    return "Plus candidate.";
            }
        }

        /// <summary>Resume de la preuve fraiche : echec ferme, ou marge exacte minimale (&lt;= 0 : contact possible).</summary>
        public static string ProofOf(PairSweep sweep)
        {
            if (sweep == null)
            {
                return "Preuve : balayage absent.";
            }

            if (sweep.Relation == PairRelation.FailClosed)
            {
                return "Preuve : echec ferme (" + sweep.FailClosedReason + ").";
            }

            return "Preuve : marge exacte minimale " + Meters(sweep.ExactSlackMeters) + " (<= 0 : contact possible), "
                + sweep.PathsA + " x " + sweep.PathsB + " trajectoire(s) prolongee(s).";
        }

        private static string Capitalize(string text)
        {
            return text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text.Substring(1);
        }

        public static string Meters(float value)
        {
            if (float.IsPositiveInfinity(value))
            {
                return "infinie";
            }

            return value.ToString("F3", CultureInfo.InvariantCulture) + " m";
        }

        public static string FormatVolume(RoadBoundsBox volume)
        {
            return "centre (" + volume.Center.x.ToString("F3", CultureInfo.InvariantCulture) + ", "
                + volume.Center.y.ToString("F3", CultureInfo.InvariantCulture) + ", "
                + volume.Center.z.ToString("F3", CultureInfo.InvariantCulture) + "), etendues ("
                + volume.Extents.x.ToString("F3", CultureInfo.InvariantCulture) + ", "
                + volume.Extents.y.ToString("F3", CultureInfo.InvariantCulture) + ", "
                + volume.Extents.z.ToString("F3", CultureInfo.InvariantCulture) + ")";
        }

        // ============================================================ conduisibilite affichee

        /// <summary>Rayon minimal (1 / |courbure| max) d'une trajectoire ; infini pour une droite.</summary>
        public static float MinRadiusMeters(IReadOnlyList<RoadCurveSample> samples)
        {
            float curvature = 0f;
            if (samples != null)
            {
                foreach (var sample in samples)
                {
                    curvature = Math.Max(curvature, Math.Abs(sample.CurvaturePerMeter));
                }
            }

            return curvature > 0f ? 1f / curvature : float.PositiveInfinity;
        }

        /// <summary>Plafond de braquage minimal le long d'une trajectoire, pour un profil declare.</summary>
        public static float MinCeilingMetersPerSecond(IReadOnlyList<RoadCurveSample> samples, DrivabilityProfile profile)
        {
            float ceiling = float.PositiveInfinity;
            if (samples == null || !profile.Declared)
            {
                return ceiling;
            }

            foreach (var sample in samples)
            {
                float value = RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profile, sample.CurvaturePerMeter);
                if (float.IsNaN(value))
                {
                    return float.NaN;
                }

                ceiling = Math.Min(ceiling, value);
            }

            return ceiling;
        }

        // ============================================================ rendu du differentiel

        public static string Render(PairReviewModel review)
        {
            var text = new StringBuilder();
            text.AppendLine("# Story 5.50 -- differentiel des paires de conflit");
            text.AppendLine();
            text.AppendLine("Generation en lecture seule : aucun modele, overlay, rapport 5.28 ou fichier de decisions n'a ete ecrit.");
            text.AppendLine();
            text.AppendLine("- source V1 : `" + review.SourceHash + "`");
            text.AppendLine("- table historique : `" + review.HistoricalModelVersion + "` / schema " + review.HistoricalSchema);
            text.AppendLine("- balayage : distance exacte des empreintes, trajectoires prolongees d'un demi-gabarit (coutures et elements courts traverses)");
            text.AppendLine("- paires historiques : " + review.HistoricalPairs);
            text.AppendLine("- paires fraiches : " + review.FreshPairs);
            text.AppendLine("- inchangees : " + review.Count(PairReviewStatus.Unchanged));
            text.AppendLine("- modifiees : " + review.Count(PairReviewStatus.Modified));
            text.AppendLine("- nouvelles : " + review.Count(PairReviewStatus.New));
            text.AppendLine("- retirees : " + review.Count(PairReviewStatus.Removed));
            text.AppendLine("- ecartees par la distance exacte (boites englobantes seules) : " + review.EnvelopeOnly.Count);
            text.AppendLine("- suivi (meme approche ou chemin a une voie) : " + review.Following.Count);
            text.AppendLine("- echecs fermes : " + review.FailClosed.Count);
            text.AppendLine("- elements plus courts que le gabarit : " + review.ShortElements.Count);
            AppendEntries(text, review, "Paires modifiees", PairReviewStatus.Modified);
            AppendEntries(text, review, "Paires nouvelles", PairReviewStatus.New);
            AppendEntries(text, review, "Paires retirees", PairReviewStatus.Removed);
            AppendEntries(text, review, "Paires inchangees", PairReviewStatus.Unchanged);
            AppendSweeps(text, review, "Paires ecartees par la distance exacte (faux candidats des boites englobantes)", review.EnvelopeOnly);
            AppendSweeps(text, review, "Echecs fermes (candidats par prudence)", review.FailClosed);
            AppendSweeps(text, review, "Paires de suivi (publiees, jamais candidates)", review.Following);
            AppendShort(text, review);
            return text.ToString().Replace("\r\n", "\n");
        }

        private static void AppendEntries(StringBuilder text, PairReviewModel review, string title, PairReviewStatus status)
        {
            var rows = review.Entries.FindAll(delegate(PairReviewEntry e) { return e.Status == status; });
            text.AppendLine();
            text.AppendLine("## " + title + " (" + rows.Count + ")");
            text.AppendLine();
            if (rows.Count == 0)
            {
                text.AppendLine("Aucune.");
                return;
            }

            foreach (var entry in rows)
            {
                text.Append("- **").Append(entry.JunctionLabel).Append("** : ").Append(entry.LabelA)
                    .Append(" x ").Append(entry.LabelB).AppendLine();
                text.Append("  - cles : `").Append(entry.KeyA).Append("` x `").Append(entry.KeyB).AppendLine("`");
                text.Append("  - raison : ").AppendLine(entry.Reason);
                text.Append("  - decision : ").AppendLine(entry.HasDecision
                    ? entry.Decision.Decision + " (" + entry.DecisionState + ")"
                    : "aucune (" + entry.DecisionState + ")");
                if (entry.OldFingerprint != null)
                {
                    text.Append("  - ancien : `").Append(entry.OldFingerprint).Append("`, volume ")
                        .AppendLine(FormatVolume(entry.OldVolume));
                }

                if (entry.NewFingerprint != null)
                {
                    text.Append("  - nouveau : `").Append(entry.NewFingerprint).Append("`, volume ")
                        .AppendLine(FormatVolume(entry.NewVolume));
                }
            }
        }

        private static void AppendSweeps(StringBuilder text, PairReviewModel review, string title, List<PairSweep> sweeps)
        {
            var rows = new List<string>();
            foreach (var sweep in sweeps)
            {
                Junction junction;
                string label = review.Model.TryGetJunction(sweep.JunctionId, out junction) ? junction.Label : sweep.JunctionId.ToString();
                string detail = sweep.Relation == PairRelation.FailClosed ? sweep.FailClosedReason
                    : sweep.Relation == PairRelation.EnvelopeOnly ? "marge englobante " + Meters(sweep.EnvelopeSlackMeters) + ", marge exacte " + Meters(sweep.ExactSlackMeters)
                    : sweep.Relation == PairRelation.SameApproach ? "meme approche" : "chemin a une voie";
                rows.Add("- **" + label + "** : " + review.Label(sweep.MovementA) + " x " + review.Label(sweep.MovementB) + " ; " + detail);
            }

            rows.Sort(StringComparer.Ordinal);
            text.AppendLine();
            text.AppendLine("## " + title + " (" + rows.Count + ")");
            text.AppendLine();
            if (rows.Count == 0)
            {
                text.AppendLine("Aucune.");
                return;
            }

            foreach (var row in rows)
            {
                text.AppendLine(row);
            }
        }

        private static void AppendShort(StringBuilder text, PairReviewModel review)
        {
            text.AppendLine();
            text.AppendLine("## Elements plus courts que le gabarit (" + review.ShortElements.Count + ")");
            text.AppendLine();
            if (review.ShortElements.Count == 0)
            {
                text.AppendLine("Aucun.");
                return;
            }

            foreach (var element in review.ShortElements)
            {
                string key;
                string name = element.IsMovement ? review.Label(element.Id) : review.Keys.TryGetValue(element.Id, out key) ? key : element.Id.ToString();
                text.Append("- ").Append(element.IsMovement ? "mouvement " : "corridor ").Append(name).Append(" : ")
                    .AppendLine(Meters(element.LengthMeters));
            }
        }
    }

    /// <summary>
    /// Actions de decision reservees au proprietaire. Chacune transforme le texte des decisions et
    /// n'est appelee que par un geste explicite dans la fenetre de revue ; le pipeline ne les appelle
    /// jamais. Aucune ne reconfirme une paire dont la geometrie ou le volume a change sans que le
    /// proprietaire l'ait designee.
    /// </summary>
    public static class PairReviewActions
    {
        /// <summary>
        /// Reconfirme en lot les seules paires inchangees : empreinte historique = empreinte fraiche.
        /// Une paire modifiee, nouvelle, retiree ou absente de la table n'est jamais touchee.
        /// </summary>
        public static string ReconfirmUnchanged(string decisionsText, PairReviewModel review, out int count)
        {
            count = 0;
            var decisions = AuthoringDecisions.Parse(decisionsText);
            for (int i = 0; i < decisions.Conflicts.Count; i++)
            {
                ConflictDecision decision = decisions.Conflicts[i];
                PairReviewEntry entry = review.Find(AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB));
                if (entry == null || entry.Status != PairReviewStatus.Unchanged || entry.OldFingerprint == null
                    || !string.Equals(entry.OldFingerprint, entry.NewFingerprint, StringComparison.Ordinal)
                    || !PairGeometryFingerprint.IsSha256(entry.NewFingerprint)
                    || string.Equals(decision.GeometryFingerprint, entry.NewFingerprint, StringComparison.Ordinal))
                {
                    continue;
                }

                decision.GeometryFingerprint = entry.NewFingerprint;
                decisions.Conflicts[i] = decision;
                count++;
            }

            return decisions.Serialize();
        }

        /// <summary>Reconfirme la decision existante d'une paire designee, avec son empreinte fraiche.</summary>
        public static string ReconfirmPair(string decisionsText, PairReviewEntry entry)
        {
            RequireFresh(entry, "reconfirmer");
            var decisions = AuthoringDecisions.Parse(decisionsText);
            int index = IndexOf(decisions, entry);
            if (index < 0)
            {
                throw new InvalidOperationException("Aucune decision a reconfirmer pour " + entry.LabelA + " x " + entry.LabelB + ".");
            }

            ConflictDecision decision = decisions.Conflicts[index];
            decision.GeometryFingerprint = entry.NewFingerprint;
            decisions.Conflicts[index] = decision;
            return decisions.Serialize();
        }

        /// <summary>Decide (ou change) la decision d'une paire candidate, avec son empreinte fraiche.</summary>
        public static string Decide(string decisionsText, PairReviewEntry entry, ConflictDecisionKind kind, string reason)
        {
            RequireFresh(entry, "decider");
            if (reason == null || reason.Trim().Length == 0)
            {
                throw new InvalidOperationException("Un motif est obligatoire pour decider " + entry.LabelA + " x " + entry.LabelB + ".");
            }

            var decisions = AuthoringDecisions.Parse(decisionsText);
            int index = IndexOf(decisions, entry);
            var decision = index >= 0 ? decisions.Conflicts[index] : new ConflictDecision
            {
                MovementKeyA = entry.KeyA,
                MovementKeyB = entry.KeyB
            };

            if (kind == ConflictDecisionKind.Accepted)
            {
                if (index < 0 || decision.Decision != ConflictDecisionKind.Accepted || decision.Id == RoadId.None)
                {
                    decision.Id = RoadId.New();
                }
            }
            else
            {
                decision.Id = RoadId.None;
            }

            decision.Decision = kind;
            decision.Reason = reason.Trim();
            decision.GeometryFingerprint = entry.NewFingerprint;
            if (index >= 0)
            {
                decisions.Conflicts[index] = decision;
            }
            else
            {
                decisions.Conflicts.Add(decision);
            }

            return decisions.Serialize();
        }

        /// <summary>Retire explicitement la decision orpheline d'une paire qui n'est plus candidate.</summary>
        public static string DisposeRemoved(string decisionsText, PairReviewEntry entry)
        {
            if (entry == null || entry.Status != PairReviewStatus.Removed)
            {
                throw new InvalidOperationException("Seule la decision d'une paire retiree peut etre disposee.");
            }

            var decisions = AuthoringDecisions.Parse(decisionsText);
            int index = IndexOf(decisions, entry);
            if (index < 0)
            {
                throw new InvalidOperationException("Aucune decision orpheline pour " + entry.LabelA + " x " + entry.LabelB + ".");
            }

            decisions.Conflicts.RemoveAt(index);
            return decisions.Serialize();
        }

        private static void RequireFresh(PairReviewEntry entry, string action)
        {
            if (entry == null || entry.Status == PairReviewStatus.Removed || !PairGeometryFingerprint.IsSha256(entry.NewFingerprint))
            {
                throw new InvalidOperationException("Impossible de " + action + " : la paire n'est pas candidate dans le balayage frais.");
            }
        }

        private static int IndexOf(AuthoringDecisions decisions, PairReviewEntry entry)
        {
            return decisions.Conflicts.FindIndex(delegate(ConflictDecision d)
            {
                return d.MovementKeyA == entry.KeyA && d.MovementKeyB == entry.KeyB;
            });
        }
    }
}
#endif
