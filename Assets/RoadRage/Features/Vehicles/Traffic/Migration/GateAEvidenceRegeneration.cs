#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>Etat d'une paire du differentiel contre les decisions signees (vocabulaire du differentiel 5.50).</summary>
    public enum CandidateDiffState { New, Changed, Confirmed, Orphan }

    public sealed class CandidateDiffEntry
    {
        public string PairKey;
        public string Junction;
        public string Relation;
        public float ExactSlackMeters;
        public string PriorDecision;
        public CandidateDiffState State;
    }

    /// <summary>Regeneration de la preuve Gate A sous des parametres donnes : jamais ecrite dans un artefact signe.</summary>
    public sealed class GateAEvidenceRegenerationResult
    {
        public GateAEvidenceParameters Parameters;
        public AuthoredRun Run;
        public JunctionClearanceResult Junctions;
        public JunctionClearanceResult Roundabouts;
        public readonly List<RoundaboutMeasurement> Rings = new List<RoundaboutMeasurement>();
        public readonly List<CandidateDiffEntry> Diff = new List<CandidateDiffEntry>();

        /// <summary>Mesures impossibles ou pipeline en echec : la regeneration est incomplete.</summary>
        public readonly List<string> Failures = new List<string>();

        /// <summary>Residus non positifs, bornes d'ecart non fermees, poses infaisables : HALT proprietaire.</summary>
        public readonly List<string> Deficits = new List<string>();

        public string ReportText;
        public string DiffText;

        public bool Complete { get { return Failures.Count == 0; } }
        public bool Covered { get { return Complete && Deficits.Count == 0; } }
    }

    /// <summary>
    /// Story 5.52 : regeneration des quatre familles de preuves Gate A sous des parametres donnes (candidats de
    /// conflit, degagement physique et Sidewalk des carrefours classiques, balayage et residus d'anneau des
    /// giratoires), avec le differentiel des candidats contre les decisions signees. Les preuves de degagement se
    /// calculent sur le modele des candidats (meme geometrie ; les zones de conflit n'y entrent pas), puisque le
    /// rapprochement des decisions echoue par construction tant que la reevaluation deleguee n'a pas eu lieu.
    /// Rien n'est ecrit : l'appelant publie les textes.
    /// </summary>
    public static class GateAEvidenceRegeneration
    {
        public const string OutputDirectory = "_bmad-output/implementation-artifacts/gate-a-5-52";

        public static GateAEvidenceRegenerationResult Regenerate(Scene scene, string lineageText, string decisionsText,
            GateAEvidenceParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            var result = new GateAEvidenceRegenerationResult { Parameters = parameters };
            var run = AuthoredRoadModel.Run(V1SourceSet.Extract(scene), lineageText, decisionsText, parameters);
            result.Run = run;
            foreach (var failure in run.Failures)
            {
                if (PairReview.IsReconciliationFailure(failure)) continue;
                if (failure.StartsWith(KinematicOffsetBounds.NotClosedCode, StringComparison.Ordinal)) result.Deficits.Add(failure);
                else result.Failures.Add(failure);
            }

            if (run.CandidateModel != null && run.PairSweeps != null && result.Failures.Count == 0)
            {
                var sidewalks = SidewalkDeclarations.Read(scene, result.Failures);
                result.Junctions = JunctionClearance.Measure(scene, run.Import, run.CandidateModel, sidewalks,
                    JunctionClearance.DefaultStepMeters, false, parameters, run.OffsetBounds);
                result.Roundabouts = RoundaboutClearance.Sweep(scene, run.Import, run.CandidateModel, sidewalks, parameters, run.OffsetBounds);
                Split(result.Junctions, result);
                Split(result.Roundabouts, result);
                result.Rings.AddRange(RoundaboutClearance.Measure(run.Import, run.CandidateModel, result.Failures, parameters, run.OffsetBounds));
                foreach (var ring in result.Rings)
                {
                    if (!(ring.EnvelopeResidual > 0f) || !(ring.PhysicalResidual > 0f))
                    {
                        result.Deficits.Add("Giratoire '" + ring.Module.Label + "' : residu d'anneau a deux gabarits non positif (V2 "
                            + R(ring.EnvelopeResidual) + ", physique " + R(ring.PhysicalResidual) + ", |e| max "
                            + KinematicOffsetBounds.Deg(ring.OffsetMaxRadians) + " deg).");
                    }
                }

                if (run.OffsetBounds != null) result.Deficits.AddRange(run.OffsetBounds.Infeasible);
                CheckCoverage(result);
                BuildDiff(result);
            }
            else if (result.Failures.Count == 0 && result.Deficits.Count == 0)
            {
                result.Failures.Add("Pipeline arrete avant le balayage des candidats.");
            }

            result.Failures.Sort(StringComparer.Ordinal);
            result.Deficits.Sort(StringComparer.Ordinal);
            result.ReportText = RenderReport(result);
            result.DiffText = RenderDiff(result);
            return result;
        }

        private static void Split(JunctionClearanceResult measured, GateAEvidenceRegenerationResult result)
        {
            var residuals = new HashSet<string>(measured.ResidualFailures, StringComparer.Ordinal);
            foreach (var failure in measured.Failures)
            {
                (residuals.Contains(failure) ? result.Deficits : result.Failures).Add(failure);
            }
        }

        private static void CheckCoverage(GateAEvidenceRegenerationResult result)
        {
            foreach (var module in result.Run.Import.SourceSet.Modules)
            {
                var evidence = module.Kind == V1ModuleKind.Roundabout ? result.Roundabouts
                    : module.Kind == V1ModuleKind.Crossroads || module.Kind == V1ModuleKind.TJunction ? result.Junctions : null;
                if (evidence != null && !evidence.Rows.Exists(row => row.Junction == module.Label))
                {
                    result.Failures.Add("Carrefour '" + module.Label + "' (" + module.Kind + ") sans aucune ligne de preuve : non mesure.");
                }
            }
        }

        private static void BuildDiff(GateAEvidenceRegenerationResult result)
        {
            var run = result.Run;
            var keys = AuthoringDecisions.KeysById(run.Import);
            var decisions = new Dictionary<string, ConflictDecision>(StringComparer.Ordinal);
            foreach (var decision in run.Decisions.Conflicts)
            {
                decisions[AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB)] = decision;
            }

            var fresh = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var candidate in run.Candidates)
            {
                fresh[AuthoredRoadModel.PairKey(keys[candidate.MovementA], keys[candidate.MovementB])] = candidate.GeometryFingerprint;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var sweep in run.PairSweeps)
            {
                string pair = AuthoredRoadModel.PairKey(keys[sweep.MovementA], keys[sweep.MovementB]);
                seen.Add(pair);
                ConflictDecision prior;
                bool decided = decisions.TryGetValue(pair, out prior);
                CandidateDiffState state;
                if (sweep.IsCandidate)
                {
                    string fingerprint;
                    fresh.TryGetValue(pair, out fingerprint);
                    state = !decided ? CandidateDiffState.New
                        : string.Equals(prior.GeometryFingerprint, fingerprint, StringComparison.Ordinal) ? CandidateDiffState.Confirmed
                        : CandidateDiffState.Changed;
                }
                else if (decided)
                {
                    state = CandidateDiffState.Orphan;
                }
                else
                {
                    continue;
                }

                result.Diff.Add(new CandidateDiffEntry
                {
                    PairKey = pair,
                    Junction = JunctionLabel(run.CandidateModel, sweep.JunctionId),
                    Relation = sweep.Relation.ToString(),
                    ExactSlackMeters = sweep.ExactSlackMeters,
                    PriorDecision = decided ? prior.Decision.ToString() : "-",
                    State = state
                });
            }

            foreach (var entry in decisions)
            {
                if (seen.Contains(entry.Key)) continue;
                result.Diff.Add(new CandidateDiffEntry
                {
                    PairKey = entry.Key,
                    Junction = "-",
                    Relation = "absente",
                    ExactSlackMeters = float.PositiveInfinity,
                    PriorDecision = entry.Value.Decision.ToString(),
                    State = CandidateDiffState.Orphan
                });
            }

            result.Diff.Sort((a, b) => string.CompareOrdinal(a.PairKey, b.PairKey));
        }

        private static string JunctionLabel(CompiledRoadModel model, RoadId id)
        {
            Junction junction;
            return model.TryGetJunction(id, out junction) && !string.IsNullOrEmpty(junction.Label) ? junction.Label : id.ToString();
        }

        // ============================================================ rendu canonique

        public static string RenderReport(GateAEvidenceRegenerationResult result)
        {
            var parameters = result.Parameters;
            var run = result.Run;
            var text = new StringBuilder();
            text.Append("# Regeneration de la preuve Gate A -- Story 5.52\n\n");
            if (!parameters.Kinematic)
            {
                text.Append("> **DIAGNOSTIC INTERMEDIAIRE** -- allocation declaree a pose tangente. Ne vaut pas preuve : jamais lie, signe ni utilise comme couverture.\n\n");
            }

            text.Append("## Parametres\n\n");
            var profile = run != null && run.CandidateModel != null ? run.CandidateModel.ValidationProfile : default(RoadModelValidationProfile);
            text.Append("- modele de pose : `").Append(parameters.PoseModelLabel).Append("`\n");
            text.Append("- a_e = ").Append(R(parameters.TrackingAllowanceMeters)).Append(" m (max|o| ")
                .Append(R(GateAEvidenceParameters.MaximumAbsolutePlanningOffsetMeters)).Append(" + epsilon_t declare) ; marge reservee ")
                .Append(R(profile.LateralClearanceMarginMeters)).Append(" m ; delta_c ").Append(R(V1RoadModelImporter.ChordToleranceMeters)).Append(" m\n");
            if (parameters.Kinematic)
            {
                float conflictRemainder = ConflictSweep.OffsetGridRemainder(profile, parameters);
                float clearanceRho = Mathf.Sqrt(Mathf.Pow(JunctionClearance.HalfLength(profile, parameters.TrackingAllowanceMeters), 2f)
                    + Mathf.Pow(JunctionClearance.HalfWidth(profile, parameters.TrackingAllowanceMeters), 2f));
                text.Append("- h_e = ").Append(R(parameters.OffsetGridStepRadians)).Append(" rad : reste rho.h_e/2 ").Append(R(conflictRemainder))
                    .Append(" m (candidats), ").Append(R(clearanceRho * parameters.OffsetGridStepRadians * 0.5f)).Append(" m (degagement, rho gonfle)\n");
                text.Append("- eta = ").Append(R(parameters.OffsetToleranceRadians)).Append(" rad ; budget ").Append(parameters.ClosureIterationBudget).Append(" iterations\n");
                text.Append("- faisabilite : taux de braquage ").Append(R(parameters.Feasibility.SteerRateDegreesPerSecond)).Append(" deg/s, adherence ")
                    .Append(R(parameters.Feasibility.LateralGripMetersPerSecondSquared)).Append(" m/s2, vitesse desiree ")
                    .Append(R(parameters.Feasibility.DesiredSpeedMetersPerSecond)).Append(" m/s\n");
            }

            text.Append("- parametres canoniques : `").Append(V1SourceSet.Sha256Hex(parameters.CanonicalText)).Append("`\n");
            text.Append("- versions : balayage v").Append(ConflictSweep.AlgorithmVersionFor(parameters)).Append(", empreinte de paire v")
                .Append(PairGeometryFingerprint.SchemaVersionFor(parameters)).Append(", degagement v").Append(JunctionClearance.AlgorithmVersion).Append('\n');
            if (run != null && run.CandidateModel != null)
            {
                text.Append("- modele des candidats : `").Append(run.CandidateModel.Version).Append("` ; source `").Append(run.Migration.SourceSet.SourceHash)
                    .Append("` ; decisions `").Append(V1SourceSet.Sha256Hex(run.DecisionsText)).Append("`\n");
            }

            if (result.Junctions != null && result.Roundabouts != null)
            {
                text.Append("- empreintes d'entree : physique carrefours `").Append(result.Junctions.PhysicalFingerprint).Append("`, giratoires `")
                    .Append(result.Roundabouts.PhysicalFingerprint).Append("`, Sidewalk `").Append(result.Junctions.SemanticFingerprint).Append("`\n");
            }

            text.Append("\n## Verdict\n\n");
            text.Append(result.Covered ? "COUVERT" : result.Complete ? "NON COUVERT" : "INCOMPLET").Append(" -- ")
                .Append(result.Deficits.Count).Append(" deficit(s), ").Append(result.Failures.Count).Append(" echec(s) de mesure.\n\n");
            AppendMinima(text, result);
            foreach (var deficit in result.Deficits) text.Append("- DEFICIT ").Append(deficit).Append('\n');
            foreach (var failure in result.Failures) text.Append("- ECHEC ").Append(failure).Append('\n');

            var bounds = run == null ? null : run.OffsetBounds;
            if (bounds != null)
            {
                text.Append("\n## Bornes d'ecart (contrat §8)\n\n");
                text.Append("- iterations ").Append(bounds.Iterations).Append(bounds.Converged ? " (convergence)" : " (budget epuise)")
                    .Append(", fermeture ").Append(bounds.Closed ? "acceptee" : "refusee").Append(", elements inatteignables ").Append(bounds.Unreachable.Count).Append('\n');
                text.Append("- faisabilite : ").Append(bounds.FeasibilityChecks).Append(" noeud(s), marge de vitesse de braquage min ")
                    .Append(R((float)bounds.MinimumLockSpeedMarginMetersPerSecond)).Append(" m/s, marge de taux min ")
                    .Append(R((float)bounds.MinimumSteerRateMarginDegreesPerSecond)).Append(" deg/s\n\n");
                text.Append(bounds.Render());
            }

            text.Append("\n## Residus des carrefours et giratoires\n\n");
            text.Append("| Genre | Carrefour | Trajectoire | Surface Sidewalk | Residu physique (m) | Obstacle temoin | Couture | Residu Sidewalk (m) | Couture | Temoin physique (element @ s) | e physique [lo, hi] (deg) | Reste de grille (m) |\n");
            text.Append("|---|---|---|---|---:|---|---|---:|---|---|---|---:|\n");
            var lines = new List<string>();
            foreach (var evidence in new[] { result.Junctions, result.Roundabouts })
            {
                if (evidence == null) continue;
                foreach (var row in evidence.Rows)
                {
                    var physical = row.Physical;
                    lines.Add("| degagement | " + row.Junction + " | " + row.Movement + " | " + (row.Surface ?? "-") + " | " + R(physical.Residual) + " | "
                        + (string.IsNullOrEmpty(physical.Obstacle) ? "aucun" : physical.Obstacle) + " | " + (physical.Seam ? "couture" : "-") + " | "
                        + (row.Semantic == null ? "-" : R(row.Semantic.Residual)) + " | " + (row.Semantic == null ? "-" : row.Semantic.Seam ? "couture" : "-") + " | "
                        + (physical.ElementId.IsEmpty ? "-" : physical.ElementId + " @ " + R(physical.SMeters)) + " | "
                        + (physical.ElementId.IsEmpty ? "-" : "[" + KinematicOffsetBounds.Deg(physical.OffsetLoRadians) + ", " + KinematicOffsetBounds.Deg(physical.OffsetHiRadians) + "]")
                        + " | " + R(physical.GridRemainder) + " |");
                }
            }

            lines.Sort(StringComparer.Ordinal);
            foreach (var line in lines) text.Append(line).Append('\n');

            text.Append("\n## Anneaux a deux gabarits\n\n| Giratoire | a_e (m) | pire abs(e) (deg) | Residu V2 (m) | Residu physique (m) | Obstacle |\n|---|---:|---:|---:|---:|---|\n");
            var rings = new List<string>();
            foreach (var ring in result.Rings)
            {
                rings.Add("| " + ring.Module.Label + " | " + R(ring.AllowanceMeters) + " | " + KinematicOffsetBounds.Deg(ring.OffsetMaxRadians) + " | "
                    + R(ring.EnvelopeResidual) + " | " + R(ring.PhysicalResidual) + " | " + (ring.NearestObstacle ?? "aucun") + " |");
            }

            rings.Sort(StringComparer.Ordinal);
            foreach (var line in rings) text.Append(line).Append('\n');
            return text.ToString();
        }

        private static void AppendMinima(StringBuilder text, GateAEvidenceRegenerationResult result)
        {
            float physical = float.PositiveInfinity;
            float semantic = float.PositiveInfinity;
            foreach (var evidence in new[] { result.Junctions, result.Roundabouts })
            {
                if (evidence == null) continue;
                foreach (var row in evidence.Rows)
                {
                    physical = Mathf.Min(physical, row.Physical.Residual);
                    if (row.Semantic != null) semantic = Mathf.Min(semantic, row.Semantic.Residual);
                }
            }

            float ring = float.PositiveInfinity;
            foreach (var measurement in result.Rings) ring = Mathf.Min(ring, Mathf.Min(measurement.EnvelopeResidual, measurement.PhysicalResidual));
            text.Append("Residus minimaux : physique ").Append(R(physical)).Append(" m, Sidewalk ").Append(R(semantic)).Append(" m, anneau ")
                .Append(R(ring)).Append(" m.\n\n");
        }

        public static string RenderDiff(GateAEvidenceRegenerationResult result)
        {
            var text = new StringBuilder();
            text.Append("# Differentiel des candidats de conflit -- Story 5.52\n\n");
            text.Append("Parametres : `").Append(result.Parameters.PoseModelLabel).Append("`, a_e = ").Append(R(result.Parameters.TrackingAllowanceMeters))
                .Append(" m. Reference : decisions signees (empreintes v1). Aucune decision n'est reutilisee sur une enveloppe changee.\n\n");
            foreach (CandidateDiffState state in Enum.GetValues(typeof(CandidateDiffState)))
            {
                text.Append("- ").Append(state).Append(" : ").Append(result.Diff.Count(entry => entry.State == state)).Append('\n');
            }

            text.Append("\n| Carrefour | Nouvelles | Changees | Confirmees | Orphelines |\n|---|---:|---:|---:|---:|\n");
            foreach (var junction in result.Diff.Select(entry => entry.Junction).Distinct().OrderBy(label => label, StringComparer.Ordinal))
            {
                var rows = result.Diff.Where(entry => entry.Junction == junction).ToList();
                text.Append("| ").Append(junction).Append(" | ").Append(rows.Count(e => e.State == CandidateDiffState.New)).Append(" | ")
                    .Append(rows.Count(e => e.State == CandidateDiffState.Changed)).Append(" | ").Append(rows.Count(e => e.State == CandidateDiffState.Confirmed))
                    .Append(" | ").Append(rows.Count(e => e.State == CandidateDiffState.Orphan)).Append(" |\n");
            }

            text.Append("\n| Paire | Carrefour | Relation fraiche | Ecart exact (m) | Decision signee | Etat |\n|---|---|---|---:|---|---|\n");
            foreach (var entry in result.Diff)
            {
                text.Append("| ").Append(entry.PairKey.Replace("\n", " x ")).Append(" | ").Append(entry.Junction).Append(" | ").Append(entry.Relation)
                    .Append(" | ").Append(R(entry.ExactSlackMeters)).Append(" | ").Append(entry.PriorDecision).Append(" | ").Append(entry.State).Append(" |\n");
            }

            return text.ToString();
        }

        private static string R(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
#endif
