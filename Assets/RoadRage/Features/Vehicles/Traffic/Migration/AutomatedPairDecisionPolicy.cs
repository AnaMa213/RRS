#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>Plan complet, valide avant toute ecriture, de 5.50-AUTO-DECISIONS-v1.</summary>
    public sealed class AutomatedPairDecisionPlan
    {
        public string DecisionRunId;
        public string DecisionsText;
        public string ManifestText;
        public readonly Dictionary<string, RoadId> AllocatedIds = new Dictionary<string, RoadId>(StringComparer.Ordinal);

        /// <summary>Duree du raffinement par paire (cle de paire), mesure publiee hors des textes deterministes.</summary>
        public readonly Dictionary<string, double> RefinementSeconds = new Dictionary<string, double>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Fonction deleguee de disposition des paires 5.50. Elle ne rejette que les intervalles dont
    /// la borne conservatrice du balayage est strictement positive ; tout echec d'hypothese ou
    /// contact seulement possible devient un conflit conservateur.
    /// </summary>
    public static class AutomatedPairDecisionPolicy
    {
        public const int DecisionPolicyVersion = 1;
        public const int ManifestFormatVersion = 1;
        public const int KinematicDecisionPolicyVersion = 2;
        public const int KinematicManifestFormatVersion = 2;

        /// <summary>
        /// Politique v3 (Story 5.53) : raffinement borne des seules paires candidates sans temoin, typage
        /// Crossing / Merge de chaque zone acceptee. v1 et v2 restent lisibles.
        /// </summary>
        public const int RefinedDecisionPolicyVersion = 3;
        public const int RefinedManifestFormatVersion = 3;

        /// <summary>Plafond dur de feuilles par paire (decision proprietaire du 2026-10-05) ; un maximum, pas un objectif.</summary>
        public const int RefinementLeafBudget = 65536;

        public const float ProofToleranceMeters = 0.0001f;
        public const int MaxSubdivisionDepth = 20;

        /// <summary>Motif d'une paire ConflictProven par le temoin du balayage, sans raffinement de classification.</summary>
        public const string SweepWitnessReasonCode = "inflated-rectangles-overlap";
        public const string ApprovalId = "5.50-AUTO-DECISIONS-v1";
        public const string ApprovalSha256 = "d12de07eb0891b47d24083cd41629e53825d63fbdb43035d3578c3e9b334d2bb";
        public const string ManifestPath = "_bmad-output/implementation-artifacts/v1-regression-5-50/automated-pair-decisions.json";

        /// <summary>Construit un plan en memoire. Les identites nouvelles du premier plan sont reutilisables par le second.</summary>
        public static AutomatedPairDecisionPlan CreatePlan(
            AuthoredRun run,
            string existingManifestText = null,
            IDictionary<string, RoadId> allocatedIds = null)
        {
            RequireRun(run);
            GateAEvidenceParameters parameters = run.EvidenceParameters;
            bool legacy = parameters.IsLegacy;
            int policyVersion = legacy ? DecisionPolicyVersion : RefinedDecisionPolicyVersion;
            int sweepVersion = ConflictSweep.AlgorithmVersionFor(parameters);
            int fingerprintVersion = PairGeometryFingerprint.SchemaVersionFor(parameters);
            string parametersHash = legacy ? string.Empty : Hash(parameters.CanonicalText);
            AutomatedPairDecisionManifest previous = ParseManifest(existingManifestText, false);
            var keys = AuthoringDecisions.KeysById(run.Import);
            var old = new Dictionary<string, ConflictDecision>(StringComparer.Ordinal);
            foreach (var decision in run.Decisions.Conflicts)
            {
                old.Add(AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB), decision);
            }

            string modelVersion = run.CandidateModel.Version.ToString();
            string sourceHash = run.Migration.SourceSet.SourceHash;
            string lineageHash = V1SourceSet.Sha256Hex(run.LineageText);
            string freshDiffHash = DiffHash(run.PairSweeps, keys);
            string profileHash = ProfileHash(run.CandidateModel.ValidationProfile);
            bool sameInputs = previous != null && SameInputs(previous, modelVersion, sourceHash, lineageHash, freshDiffHash,
                profileHash, parametersHash, policyVersion, sweepVersion, fingerprintVersion);
            string inputDecisionsHash = sameInputs
                ? previous.InputDecisionsHash
                : V1SourceSet.Sha256Hex(run.DecisionsText);
            string supersededManifest = previous == null ? string.Empty
                : sameInputs ? previous.SupersededManifestText ?? string.Empty : existingManifestText;
            string engineCommit = GitCommit();
            string runIdentity = string.Join("\n", new[]
            {
                ApprovalId, ApprovalSha256, engineCommit, modelVersion, sourceHash, lineageHash, profileHash,
                inputDecisionsHash, freshDiffHash, V1RoadModelImporter.ImporterVersion.ToString(CultureInfo.InvariantCulture),
                AuthoredRoadModel.PipelineVersion.ToString(CultureInfo.InvariantCulture),
                RoadModelCompiler.CompilerSchemaVersion.ToString(CultureInfo.InvariantCulture),
                fingerprintVersion.ToString(CultureInfo.InvariantCulture), sweepVersion.ToString(CultureInfo.InvariantCulture),
                policyVersion.ToString(CultureInfo.InvariantCulture)
            });
            if (!legacy)
            {
                runIdentity += "\n" + parametersHash + "\n" + RefinementIdentity(run);
            }

            string runId = Hash(runIdentity);
            var plan = new AutomatedPairDecisionPlan { DecisionRunId = runId };
            var refinement = legacy ? null : new RefinementInputs(run, plan.RefinementSeconds);

            var manifest = new AutomatedPairDecisionManifest
            {
                Format = legacy ? ManifestFormatVersion : RefinedManifestFormatVersion,
                ApprovalId = ApprovalId,
                ApprovalSha256 = ApprovalSha256,
                EngineCommit = engineCommit,
                DecisionRunId = runId,
                ModelVersion = modelVersion,
                SourceHash = sourceHash,
                LineageHash = lineageHash,
                ProfileHash = profileHash,
                InputDecisionsHash = inputDecisionsHash,
                FreshDifferentialHash = freshDiffHash,
                ImporterVersion = V1RoadModelImporter.ImporterVersion,
                PipelineVersion = AuthoredRoadModel.PipelineVersion,
                CompilerSchemaVersion = RoadModelCompiler.CompilerSchemaVersion,
                FingerprintSchemaVersion = fingerprintVersion,
                ConflictSweepAlgorithmVersion = sweepVersion,
                DecisionPolicyVersion = policyVersion,
                EvidenceParametersCanonical = legacy ? string.Empty : parameters.CanonicalText,
                EvidenceParametersHash = parametersHash,
                SupersededManifestText = supersededManifest,
                SupersededManifestHash = string.IsNullOrEmpty(supersededManifest) ? string.Empty : Hash(supersededManifest),
                ProofToleranceMeters = ProofToleranceMeters,
                MaxSubdivisionDepth = MaxSubdivisionDepth,
                SubdivisionOrder = legacy ? "dyadic-a-then-b" : ConflictSweep.RefinementOrder,
                RefinementLeafBudget = legacy ? 0 : RefinementLeafBudget,
                Records = new AutomatedPairDecisionRecord[run.PairSweeps.Count]
            };

            var output = CopyNonConflicts(run.Decisions);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < run.PairSweeps.Count; i++)
            {
                PairSweep sweep = run.PairSweeps[i];
                string pair = AuthoredRoadModel.PairKey(keys[sweep.MovementA], keys[sweep.MovementB]);
                if (!seen.Add(pair))
                {
                    throw new InvalidOperationException("Politique 5.50 : paire dupliquee " + Display(pair) + ".");
                }

                ConflictDecision prior;
                old.TryGetValue(pair, out prior);
                AutomatedPairDecisionRecord record = Classify(run, sweep, pair, keys, runId, modelVersion, prior, policyVersion, refinement);
                manifest.Records[i] = record;
                if (!record.Active)
                {
                    continue;
                }

                RoadId id = RoadId.None;
                if (record.Decision == ConflictDecisionKind.Accepted.ToString())
                {
                    if (prior.Decision == ConflictDecisionKind.Accepted && !prior.Id.IsEmpty)
                    {
                        id = prior.Id;
                    }
                    else if (allocatedIds != null && allocatedIds.TryGetValue(pair, out id))
                    {
                        // Identite preallouee par le premier plan.
                    }
                    else
                    {
                        id = RoadId.New();
                    }

                    plan.AllocatedIds[pair] = id;
                    record.RoadId = id.ToString();
                }

                output.Conflicts.Add(ToDecision(record, id, pair, fingerprintVersion, sweepVersion, policyVersion));
            }

            foreach (var pair in old.Keys)
            {
                if (!seen.Contains(pair))
                {
                    throw new InvalidOperationException("Politique 5.50 : decision liee a une paire inter-carrefours ou absente du balayage " + Display(pair) + ".");
                }
            }

            Array.Sort(manifest.Records, delegate(AutomatedPairDecisionRecord a, AutomatedPairDecisionRecord b)
            {
                return string.CompareOrdinal(a.PairKey, b.PairKey);
            });
            plan.DecisionsText = output.Serialize();
            plan.ManifestText = JsonUtility.ToJson(manifest, true).Replace("\r\n", "\n") + "\n";
            ValidatePlan(plan, run.PairSweeps.Count);
            ValidateTrafficScenarios(run, plan);
            return plan;
        }

        /// <summary>
        /// Harnais Editor-only des neuf carrefours. Un ordonnanceur tournant construit a chaque tour
        /// un ensemble independant maximal ; le premier demandeur change a chaque tour, ce qui borne
        /// l'attente continue a M mouvements.
        /// </summary>
        public static void ValidateTrafficScenarios(AuthoredRun run, AutomatedPairDecisionPlan plan)
        {
            AutomatedPairDecisionManifest manifest = ParseManifest(plan.ManifestText, true);
            var blocked = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in manifest.Records)
            {
                if (record.Active && record.Decision == ConflictDecisionKind.Accepted.ToString())
                {
                    blocked.Add(IdPair(record.MovementAId, record.MovementBId));
                }
            }

            if (run.CandidateModel.Junctions.Count != 9)
            {
                throw new InvalidOperationException("Harnais 5.50 : 9 carrefours attendus, " + run.CandidateModel.Junctions.Count + " recus.");
            }

            foreach (var junction in run.CandidateModel.Junctions)
            {
                var movements = new List<RoadId>(run.CandidateModel.GetMovementsInJunction(junction.Id));
                movements.Sort();
                if (movements.Count == 0)
                {
                    throw new InvalidOperationException("Harnais 5.50 : carrefour sans mouvement " + junction.Id + ".");
                }

                var served = new HashSet<RoadId>();
                for (int turn = 0; turn < movements.Count; turn++)
                {
                    List<RoadId> grant = MaximalGrant(movements, blocked, turn);
                    if (grant.Count == 0)
                    {
                        throw new InvalidOperationException("Harnais 5.50 : aucun progres au carrefour " + junction.Id + ".");
                    }

                    for (int i = 0; i < grant.Count; i++)
                    {
                        served.Add(grant[i]);
                        for (int k = i + 1; k < grant.Count; k++)
                        {
                            if (blocked.Contains(IdPair(grant[i].ToString(), grant[k].ToString())))
                            {
                                throw new InvalidOperationException("Harnais 5.50 : deux mouvements en conflit sont accordes ensemble.");
                            }
                        }
                    }

                    foreach (var waiting in movements)
                    {
                        if (grant.Contains(waiting))
                        {
                            continue;
                        }

                        bool compatible = true;
                        foreach (var accepted in grant)
                        {
                            if (blocked.Contains(IdPair(waiting.ToString(), accepted.ToString())))
                            {
                                compatible = false;
                                break;
                            }
                        }

                        if (compatible)
                        {
                            throw new InvalidOperationException("Harnais 5.50 : ensemble accorde non maximal.");
                        }
                    }
                }

                if (served.Count != movements.Count)
                {
                    throw new InvalidOperationException("Harnais 5.50 : attente superieure au nombre de mouvements du carrefour " + junction.Id + ".");
                }
            }
        }

        private static List<RoadId> MaximalGrant(List<RoadId> movements, HashSet<string> blocked, int cursor)
        {
            var grant = new List<RoadId>();
            for (int offset = 0; offset < movements.Count; offset++)
            {
                RoadId candidate = movements[(cursor + offset) % movements.Count];
                bool compatible = true;
                foreach (var accepted in grant)
                {
                    if (blocked.Contains(IdPair(candidate.ToString(), accepted.ToString())))
                    {
                        compatible = false;
                        break;
                    }
                }

                if (compatible)
                {
                    grant.Add(candidate);
                }
            }

            return grant;
        }

        private static string IdPair(string a, string b)
        {
            return string.CompareOrdinal(a, b) < 0 ? a + "|" + b : b + "|" + a;
        }

        /// <summary>Relit et verifie le schema, la couverture, les preuves et les revisions d'un plan.</summary>
        public static void ValidatePlan(AutomatedPairDecisionPlan plan, int expectedPairs)
        {
            if (plan == null)
            {
                throw new InvalidOperationException("Politique 5.50 : plan absent.");
            }

            AuthoringDecisions decisions = AuthoringDecisions.Parse(plan.DecisionsText);
            AutomatedPairDecisionManifest manifest = ParseManifest(plan.ManifestText, true);
            if (manifest.Records == null || manifest.Records.Length != expectedPairs || manifest.DecisionRunId != plan.DecisionRunId)
            {
                throw new InvalidOperationException("Politique 5.50 : couverture ou identite de run incoherente.");
            }

            var active = new Dictionary<string, AutomatedPairDecisionRecord>(StringComparer.Ordinal);
            string previous = null;
            foreach (var record in manifest.Records)
            {
                if (record == null || string.IsNullOrEmpty(record.PairKey) || (previous != null && string.CompareOrdinal(previous, record.PairKey) >= 0))
                {
                    throw new InvalidOperationException("Politique 5.50 : manifeste non canonique ou paire dupliquee.");
                }

                previous = record.PairKey;
                if (record.EvidenceHash != Hash(record.EvidenceCanonical)
                    || record.DecisionRevisionId != Revision(record.PairKey, record.GeometryFingerprint, record.Decision,
                        record.Classification, record.EvidenceHash, manifest.DecisionPolicyVersion, record.TypingCanonical))
                {
                    throw new InvalidOperationException("Politique 5.50 : preuve ou revision alteree pour " + Display(record.PairKey) + ".");
                }

                if (record.ConflictKind == ConflictKind.Merge.ToString()
                    && (!record.Active || record.Classification != AutomatedPairClassification.ConflictProven.ToString()
                        || !record.CommonExitCorridor || !record.RefinementComplete))
                {
                    throw new InvalidOperationException("Politique v3 : Merge sans preuve complete de fusion pour " + Display(record.PairKey) + ".");
                }

                if (manifest.DecisionPolicyVersion >= RefinedDecisionPolicyVersion && !TypingMatches(record))
                {
                    throw new InvalidOperationException("Politique v3 : genre ou debuts de contact non lies au typage revise pour " + Display(record.PairKey) + ".");
                }

                if (record.Classification == AutomatedPairClassification.ProvenDisjoint.ToString()
                    && (!(record.MinimumSeparationMeters > ProofToleranceMeters) || record.Decision != ConflictDecisionKind.Rejected.ToString()))
                {
                    throw new InvalidOperationException("Politique 5.50 : rejet sans certificat positif pour " + Display(record.PairKey) + ".");
                }

                if (record.Active)
                {
                    active.Add(record.PairKey, record);
                }
            }

            if (decisions.Conflicts.Count != active.Count)
            {
                throw new InvalidOperationException("Politique 5.50 : couverture active incomplete.");
            }

            foreach (var decision in decisions.Conflicts)
            {
                string pair = AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB);
                AutomatedPairDecisionRecord record;
                if (!active.TryGetValue(pair, out record)
                    || decision.DecisionRevisionId != record.DecisionRevisionId
                    || decision.EvidenceHash != record.EvidenceHash
                    || decision.DecisionRunId != manifest.DecisionRunId
                    || decision.GeometryFingerprint != record.GeometryFingerprint
                    || (manifest.DecisionPolicyVersion >= RefinedDecisionPolicyVersion
                        && (decision.Kind.ToString() != record.ConflictKind
                            || decision.ContactStartSMetersA != record.ContactStartSMetersA
                            || decision.ContactStartSMetersB != record.ContactStartSMetersB)))
                {
                    throw new InvalidOperationException("Politique 5.50 : decision active non liee au manifeste pour " + Display(pair) + ".");
                }
            }
        }

        /// <summary>Ecrit les deux artefacts seulement apres validation complete des textes temporaires.</summary>
        public static bool TryApply(AutomatedPairDecisionPlan plan, int expectedPairs, out string error)
        {
            try
            {
                ValidatePlan(plan, expectedPairs);
                RequireCleanPinnedCommit(plan);
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }

            return AuthoredRoadModel.TryWriteAll(new[]
            {
                new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath), plan.DecisionsText),
                new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(ManifestPath), plan.ManifestText)
            }, out error);
        }

        private static void RequireCleanPinnedCommit(AutomatedPairDecisionPlan plan)
        {
            AutomatedPairDecisionManifest manifest = ParseManifest(plan.ManifestText, true);
            if (string.IsNullOrEmpty(manifest.EngineCommit) || manifest.EngineCommit == "unavailable"
                || !string.Equals(manifest.EngineCommit, GitCommit(), StringComparison.Ordinal))
                throw new InvalidOperationException("Politique 5.50 : commit du moteur absent ou different du plan.");

            var start = new System.Diagnostics.ProcessStartInfo("git", "status --porcelain --untracked-files=all")
            {
                WorkingDirectory = Directory.GetParent(Application.dataPath).FullName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var process = System.Diagnostics.Process.Start(start))
            {
                string status = process.StandardOutput.ReadToEnd();
                string diagnostic = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0 || status.Length > 0)
                    throw new InvalidOperationException("Politique 5.50 : arbre Git non propre ou statut indisponible : " + diagnostic);
            }
        }

        [MenuItem("RoadRage/Traffic V2/Appliquer les decisions automatisees 5.52")]
        public static void ApplyMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != MigrationReport.ScenePath || scene.isDirty)
            {
                Debug.LogError("[Traffic V2] Application 5.52 refusee : MVP_Run doit etre la scene active, chargee et propre.");
                return;
            }

            string decisionsText = AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath));
            try
            {
                var regenerated = GateAEvidenceRegeneration.Regenerate(scene,
                    AuthoredRoadModel.ReadIfExists(MigrationReport.LineageFullPath), decisionsText, GateAEvidenceParameters.Declared());
                if (!regenerated.Covered)
                    throw new InvalidOperationException("regeneration cinematique non couverte : "
                        + string.Join(" ; ", regenerated.Failures) + " ; " + string.Join(" ; ", regenerated.Deficits));
                AuthoredRun run = regenerated.Run;
                var first = CreatePlan(run, AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(ManifestPath)));
                var second = CreatePlan(run, AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(ManifestPath)), first.AllocatedIds);
                if (first.DecisionsText != second.DecisionsText || first.ManifestText != second.ManifestText)
                {
                    throw new InvalidOperationException("Politique 5.50 non deterministe : les deux plans different.");
                }

                // Les decisions planifiees doivent compiler le modele complet avant toute ecriture.
                var compiled = AuthoredRoadModel.Run(V1SourceSet.Extract(scene), AuthoredRoadModel.ReadIfExists(MigrationReport.LineageFullPath),
                    first.DecisionsText, GateAEvidenceParameters.Declared());
                if (!compiled.Succeeded)
                    throw new InvalidOperationException("le pipeline refuse les decisions planifiees : " + string.Join(" ; ", compiled.Failures.ToArray()));

                string error;
                if (!TryApply(first, run.PairSweeps.Count, out error))
                {
                    throw new InvalidOperationException(error);
                }

                AssetDatabase.ImportAsset(AuthoredRoadModel.DecisionsPath);
                Debug.Log("[Traffic V2] Decisions automatisees 5.52 appliquees transactionnellement (run " + first.DecisionRunId + "). Gate A reste fermee et non signee.");
            }
            catch (Exception exception)
            {
                Debug.LogError("[Traffic V2] Application 5.52 refusee, rien n'est ecrit : " + exception.Message);
            }
        }

        private static AutomatedPairDecisionRecord Classify(
            AuthoredRun run,
            PairSweep sweep,
            string pair,
            Dictionary<RoadId, string> keys,
            string runId,
            string modelVersion,
            ConflictDecision prior,
            int policyVersion,
            RefinementInputs refinementInputs)
        {
            CompiledJunctionMovement a;
            CompiledJunctionMovement b;
            if (!run.CandidateModel.TryGetMovement(sweep.MovementA, out a) || !run.CandidateModel.TryGetMovement(sweep.MovementB, out b))
            {
                throw new InvalidOperationException("Politique 5.50 : mouvement introuvable pour " + Display(pair) + ".");
            }

            string geometry = AuthoredRoadModel.CandidateFingerprint(run, keys, a, b, sweep.HasVolume ? sweep.Volume : default(RoadBoundsBox));
            string movementAHash = PairGeometryFingerprint.ComputeMovement(keys[sweep.MovementA], a.Samples);
            string movementBHash = PairGeometryFingerprint.ComputeMovement(keys[sweep.MovementB], b.Samples);
            AutomatedPairClassification classification;
            string decision;
            string reasonCode;
            string reason;
            bool active;
            float separation = float.NaN;
            PairRefinement refinement = null;
            bool witness = sweep.Relation == PairRelation.Candidate && ContactWitness(sweep, run.CandidateModel.ValidationProfile);

            // Politique v3 : toute paire candidate est raffinee, pour la classifier si elle n'a pas de temoin et
            // pour typer la zone si elle en a un. Une paire avec temoin garde sa classification, sa raison et sa preuve.
            // Typage v2 (Story 5.53a) pour la seule paire a temoin du balayage et corridor aval commun : sa classification
            // ne depend pas du raffinement, que l'elagage par contenance ne peut donc pas modifier.
            if (refinementInputs != null && sweep.Relation == PairRelation.Candidate)
            {
                refinement = refinementInputs.Refine(sweep, pair, witness && a.ToCorridorId == b.ToCorridorId);
            }

            if (sweep.Relation == PairRelation.SameApproach || sweep.Relation == PairRelation.Following)
            {
                classification = AutomatedPairClassification.Following;
                decision = "Excluded";
                reasonCode = sweep.Relation == PairRelation.SameApproach ? "same-approach" : "directed-corridor-following";
                reason = "Suivi ordinaire prouve topologiquement ; aucune zone de conflit.";
                active = false;
            }
            else if (sweep.Relation == PairRelation.FailClosed)
            {
                classification = AutomatedPairClassification.ConservativeConflict;
                decision = ConflictDecisionKind.Accepted.ToString();
                reasonCode = "sweep-hypothesis-failure";
                reason = "Hypothese de balayage non satisfaite (" + sweep.FailClosedReason + ") : conflit accepte par prudence.";
                active = true;
            }
            else if (witness)
            {
                classification = AutomatedPairClassification.ConflictProven;
                decision = ConflictDecisionKind.Accepted.ToString();
                reasonCode = SweepWitnessReasonCode;
                reason = "Un temoin de poses donne un recouvrement des rectangles orientes gonfles.";
                active = true;
            }
            else if (refinement != null && refinement.Outcome == RefinementOutcome.ProvenDisjoint)
            {
                classification = AutomatedPairClassification.ProvenDisjoint;
                decision = ConflictDecisionKind.Rejected.ToString();
                reasonCode = "refined-disjoint";
                reason = "Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance.";
                // La paire reste candidate au balayage : le rapprochement exige une decision, ici un rejet sans zone.
                active = true;
                separation = refinement.MinimumSeparationMeters;
            }
            else if (refinement != null && refinement.Outcome == RefinementOutcome.Witness)
            {
                classification = AutomatedPairClassification.ConflictProven;
                decision = ConflictDecisionKind.Accepted.ToString();
                reasonCode = "refined-witness";
                reason = "Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent.";
                active = true;
            }
            else if (refinement != null && refinement.Outcome == RefinementOutcome.BudgetExhausted)
            {
                classification = AutomatedPairClassification.ConservativeConflict;
                decision = ConflictDecisionKind.Accepted.ToString();
                reasonCode = "refinement-budget";
                reason = "Budget de raffinement epuise sans preuve ni temoin : conflit accepte par prudence.";
                active = true;
            }
            else if (refinement != null)
            {
                classification = AutomatedPairClassification.ConservativeConflict;
                decision = ConflictDecisionKind.Accepted.ToString();
                reasonCode = "refinement-unresolved";
                reason = "Feuille non subdivisable (couture ou profondeur maximale) sans preuve ni temoin : conflit accepte par prudence.";
                active = true;
            }
            else if (sweep.Relation == PairRelation.Candidate)
            {
                classification = AutomatedPairClassification.ConservativeConflict;
                decision = ConflictDecisionKind.Accepted.ToString();
                reasonCode = "continuous-contact-possible";
                reason = "La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence.";
                active = true;
            }
            else
            {
                separation = sweep.EnvelopeSelected ? sweep.ExactSlackMeters : sweep.EnvelopeSlackMeters;
                if (!(separation > ProofToleranceMeters) || float.IsNaN(separation))
                {
                    classification = AutomatedPairClassification.ConservativeConflict;
                    decision = ConflictDecisionKind.Accepted.ToString();
                    reasonCode = "separation-not-strict";
                    reason = "La separation n'est pas strictement superieure a la tolerance : conflit accepte par prudence.";
                    active = true;
                }
                else
                {
                    classification = AutomatedPairClassification.ProvenDisjoint;
                    decision = ConflictDecisionKind.Rejected.ToString();
                    reasonCode = sweep.Relation == PairRelation.EnvelopeOnly ? "oriented-rectangles-separated" : "envelopes-separated";
                    reason = "Chaque combinaison d'intervalles possede une borne de separation strictement positive.";
                    active = false;
                }
            }

            string priorRevision = PreviousRevision(prior, pair);
            string evidence = Evidence(pair, sweep, classification, reasonCode, separation, movementAHash, movementBHash);
            if (refinement != null && !witness)
            {
                // Seule une paire sans temoin change de preuve : le raffinement en fait partie.
                evidence += "|" + refinement.Canonical();
            }

            string evidenceHash = Hash(evidence);
            var typing = TypeZone(refinement, a, b, classification, active && decision == ConflictDecisionKind.Accepted.ToString(),
                keys[sweep.MovementA] == pair.Split('\n')[0]);
            string typingCanonical = policyVersion >= RefinedDecisionPolicyVersion ? typing.Canonical(refinement) : string.Empty;
            string revision = Revision(pair, geometry, decision, classification.ToString(), evidenceHash, policyVersion, typingCanonical);
            string supersedes = prior.DecisionRevisionId;
            if (string.IsNullOrEmpty(supersedes) && (!string.IsNullOrEmpty(prior.MovementKeyA)))
            {
                supersedes = priorRevision;
            }
            else if (supersedes == revision)
            {
                supersedes = prior.SupersedesDecisionRevisionId;
            }

            return new AutomatedPairDecisionRecord
            {
                PairKey = pair,
                JunctionId = sweep.JunctionId.ToString(),
                MovementAId = sweep.MovementA.ToString(),
                MovementBId = sweep.MovementB.ToString(),
                MovementAHash = movementAHash,
                MovementBHash = movementBHash,
                GeometryFingerprint = geometry,
                Classification = classification.ToString(),
                Decision = decision,
                Active = active,
                Tombstone = !active && !string.IsNullOrEmpty(prior.MovementKeyA),
                ReasonCode = reasonCode,
                Reason = reason,
                EvidenceCanonical = evidence,
                EvidenceHash = evidenceHash,
                DecisionRevisionId = revision,
                SupersedesDecisionRevisionId = supersedes ?? string.Empty,
                DecisionRunId = runId,
                ModelVersion = modelVersion,
                Relation = sweep.Relation.ToString(),
                PathsA = sweep.PathsA,
                PathsB = sweep.PathsB,
                MinimumSeparationMeters = separation,
                ExactSlackMeters = sweep.ExactSlackMeters,
                EnvelopeSlackMeters = sweep.EnvelopeSlackMeters,
                RefinementOutcome = refinement == null ? string.Empty : refinement.Outcome.ToString(),
                RefinementRoots = refinement == null ? 0 : refinement.Roots,
                RefinementLeaves = refinement == null ? 0 : refinement.Leaves,
                RefinementLeavesAtDecision = refinement == null ? 0 : refinement.LeavesAtDecision,
                RefinementResolutionLeaves = refinement == null ? 0 : refinement.ResolutionLeaves,
                RefinementComplete = refinement != null && refinement.Complete,
                CommonExitCorridor = a.ToCorridorId == b.ToCorridorId,
                ConflictKind = typing.Kind.ToString(),
                ContactStartSMetersA = typing.StartA,
                ContactStartSMetersB = typing.StartB,
                TypingCanonical = typingCanonical
            };
        }

        // ============================================================ diff de classification (Story 5.53)

        /// <summary>Compteurs et listes publies du passage d'un manifeste a un autre.</summary>
        public sealed class ClassificationSummary
        {
            public int Pairs;
            public int ConservativeBefore;
            public int ConservativeToDisjoint;
            public int ConservativeToProven;
            public int ConservativeRemaining;
            public int ProvenBefore;
            public int ProvenUnchanged;
            public int Merge;
            public int Crossing;
            public readonly List<string> ChangedOutsideConservative = new List<string>();
            public readonly List<string> RemainingConservative = new List<string>();
            public readonly List<int> RefinementLeaves = new List<int>();

            /// <summary>Paires raffinees : cle, carrefour, mouvements, feuilles, issue, completude, classification, genre.</summary>
            public readonly List<RefinedPair> Refined = new List<RefinedPair>();

            /// <summary>Paires de mouvements d'approches differentes sans zone, par carrefour (libelle), apres v3.</summary>
            public readonly SortedDictionary<string, List<string>> CompatibleAfter = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
            public readonly SortedDictionary<string, int> CompatibleBefore = new SortedDictionary<string, int>(StringComparer.Ordinal);
            public readonly SortedDictionary<string, int> InterApproachPairs = new SortedDictionary<string, int>(StringComparer.Ordinal);
        }

        public sealed class RefinedPair
        {
            public string PairKey;
            public string Junction;
            public string Movements;
            public int Leaves;
            public int ResolutionLeaves;
            public string Outcome;
            public bool Complete;
            public string Classification;
            public string Kind;
        }

        /// <summary>Compare deux manifestes sur le modele des candidats ; une classification non conservative qui change est listee.</summary>
        public static ClassificationSummary Summarize(CompiledRoadModel model, string beforeManifestText, string afterManifestText)
        {
            var before = ByPair(ParseManifest(beforeManifestText, true));
            var after = ParseManifest(afterManifestText, true);
            var summary = new ClassificationSummary();
            var acceptedBefore = new HashSet<string>(StringComparer.Ordinal);
            var acceptedAfter = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in after.Records)
            {
                summary.Pairs++;
                AutomatedPairDecisionRecord old;
                before.TryGetValue(record.PairKey, out old);
                string was = old == null ? string.Empty : old.Classification;
                if (old != null && old.Active && old.Decision == ConflictDecisionKind.Accepted.ToString()) acceptedBefore.Add(IdPair(old.MovementAId, old.MovementBId));
                if (record.Active && record.Decision == ConflictDecisionKind.Accepted.ToString())
                {
                    acceptedAfter.Add(IdPair(record.MovementAId, record.MovementBId));
                    if (record.ConflictKind == ConflictKind.Merge.ToString()) summary.Merge++;
                    else summary.Crossing++;
                }

                if (!string.IsNullOrEmpty(record.RefinementOutcome))
                {
                    summary.RefinementLeaves.Add(record.RefinementLeaves);
                    summary.Refined.Add(new RefinedPair
                    {
                        PairKey = record.PairKey,
                        Junction = JunctionText(model, record.JunctionId),
                        Movements = MovementText(model, record.MovementAId) + " | " + MovementText(model, record.MovementBId),
                        Leaves = record.RefinementLeaves,
                        ResolutionLeaves = record.RefinementResolutionLeaves,
                        Outcome = record.RefinementOutcome,
                        Complete = record.RefinementComplete,
                        Classification = record.Classification,
                        Kind = IsZone(record) ? record.ConflictKind : "-"
                    });
                }
                if (was == AutomatedPairClassification.ConservativeConflict.ToString() && old.ReasonCode == "continuous-contact-possible")
                {
                    summary.ConservativeBefore++;
                    if (record.Classification == AutomatedPairClassification.ProvenDisjoint.ToString()) summary.ConservativeToDisjoint++;
                    else if (record.Classification == AutomatedPairClassification.ConflictProven.ToString()) summary.ConservativeToProven++;
                    else
                    {
                        summary.ConservativeRemaining++;
                        summary.RemainingConservative.Add(record.PairKey);
                    }

                    continue;
                }

                if (was == AutomatedPairClassification.ConflictProven.ToString()) summary.ProvenBefore++;
                bool unchanged = old != null && old.Classification == record.Classification && old.ReasonCode == record.ReasonCode
                    && old.EvidenceHash == record.EvidenceHash;
                if (!unchanged) summary.ChangedOutsideConservative.Add(record.PairKey);
                else if (was == AutomatedPairClassification.ConflictProven.ToString()) summary.ProvenUnchanged++;
            }

            // Une paire disparue du nouveau manifeste est un changement, jamais un silence.
            var afterKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in after.Records) afterKeys.Add(record.PairKey);
            foreach (var key in before.Keys)
            {
                if (!afterKeys.Contains(key)) summary.ChangedOutsideConservative.Add(key);
            }

            foreach (var junction in model.Junctions)
            {
                var ids = model.GetMovementsInJunction(junction.Id);
                string label = string.IsNullOrEmpty(junction.Label) ? junction.Id.ToString() : junction.Label;
                var compatible = new List<string>();
                int inter = 0;
                int compatibleBefore = 0;
                for (int i = 0; i < ids.Count; i++)
                {
                    CompiledJunctionMovement a;
                    model.TryGetMovement(ids[i], out a);
                    for (int j = i + 1; j < ids.Count; j++)
                    {
                        CompiledJunctionMovement b;
                        model.TryGetMovement(ids[j], out b);
                        if (a.FromCorridorId == b.FromCorridorId) continue;
                        inter++;
                        string pair = IdPair(ids[i].ToString(), ids[j].ToString());
                        if (!acceptedBefore.Contains(pair)) compatibleBefore++;
                        if (!acceptedAfter.Contains(pair)) compatible.Add(ShortLabel(a.Label) + " | " + ShortLabel(b.Label));
                    }
                }

                compatible.Sort(StringComparer.Ordinal);
                summary.CompatibleAfter[label] = compatible;
                summary.CompatibleBefore[label] = compatibleBefore;
                summary.InterApproachPairs[label] = inter;
            }

            return summary;
        }

        /// <summary>Diff complet paire par paire avant / apres, compteurs, cout du raffinement et traversees compatibles.</summary>
        public static string RenderClassificationDiff(AuthoredRun run, string beforeManifestText, string afterManifestText)
        {
            RequireRun(run);
            CompiledRoadModel model = run.CandidateModel;
            var keys = AuthoringDecisions.KeysById(run.Import);
            var before = ByPair(ParseManifest(beforeManifestText, true));
            var after = ParseManifest(afterManifestText, true);
            var summary = Summarize(model, beforeManifestText, afterManifestText);
            var text = new StringBuilder();
            text.Append("# Diff de classification des paires -- Story 5.53 (politique v").Append(after.DecisionPolicyVersion).Append(")\n\n");
            text.Append("Run avant `").Append(ParseManifest(beforeManifestText, true).DecisionRunId).Append("` (politique v")
                .Append(ParseManifest(beforeManifestText, true).DecisionPolicyVersion).Append("), run apres `").Append(after.DecisionRunId)
                .Append("`, modele des candidats `").Append(after.ModelVersion).Append("`, moteur `").Append(after.EngineCommit).Append("`.\n");
            text.Append("Budget ").Append(after.RefinementLeafBudget).Append(" feuilles par paire, profondeur ").Append(after.MaxSubdivisionDepth)
                .Append(", ordre `").Append(after.SubdivisionOrder).Append("`, tolerance ").Append(F(after.ProofToleranceMeters))
                .Append(" m. Aucune classification manuelle : chaque ligne est produite par la fonction deterministe.\n\n");

            text.Append("## Compteurs\n\n| Mesure | Valeur |\n|---|---|\n");
            text.Append("| Paires balayees | ").Append(summary.Pairs).Append(" |\n");
            text.Append("| `ConservativeConflict` avant (`continuous-contact-possible`) | ").Append(summary.ConservativeBefore).Append(" |\n");
            text.Append("| -> `ProvenDisjoint` | ").Append(summary.ConservativeToDisjoint).Append(" |\n");
            text.Append("| -> `ConflictProven` | ").Append(summary.ConservativeToProven).Append(" |\n");
            text.Append("| restees `ConservativeConflict` | ").Append(summary.ConservativeRemaining).Append(" |\n");
            text.Append("| `ConflictProven` avant / inchangees (classification, raison, preuve) | ").Append(summary.ProvenBefore).Append(" / ")
                .Append(summary.ProvenUnchanged).Append(" |\n");
            text.Append("| Autres classifications changees (HALT si non nul) | ").Append(summary.ChangedOutsideConservative.Count).Append(" |\n");
            text.Append("| Zones acceptees apres : `Crossing` / `Merge` | ").Append(summary.Crossing).Append(" / ").Append(summary.Merge).Append(" |\n\n");

            var leaves = new List<int>(summary.RefinementLeaves);
            leaves.Sort();
            long total = 0;
            foreach (int count in leaves) total += count;
            text.Append("## Cout du raffinement (feuilles par paire raffinee)\n\n");
            if (leaves.Count == 0)
            {
                text.Append("Aucune paire raffinee.\n\n");
            }
            else
            {
                text.Append("| Paires raffinees | Total | Mediane | p95 | Maximum |\n|---|---|---|---|---|\n| ").Append(leaves.Count).Append(" | ")
                    .Append(total).Append(" | ").Append(Percentile(leaves, 0.5)).Append(" | ").Append(Percentile(leaves, 0.95)).Append(" | ")
                    .Append(leaves[leaves.Count - 1]).Append(" |\n\n");
            }

            text.Append("## Traversees compatibles par carrefour (approches differentes, aucune zone)\n\n| Carrefour | Paires inter-approches | Compatibles avant | Compatibles apres |\n|---|---|---|---|\n");
            foreach (var entry in summary.CompatibleAfter)
            {
                text.Append("| ").Append(entry.Key).Append(" | ").Append(summary.InterApproachPairs[entry.Key]).Append(" | ")
                    .Append(summary.CompatibleBefore[entry.Key]).Append(" | ").Append(entry.Value.Count).Append(" |\n");
            }

            text.Append('\n');
            foreach (var entry in summary.CompatibleAfter)
            {
                text.Append("### ").Append(entry.Key).Append("\n\n");
                if (entry.Value.Count == 0) text.Append("Aucune paire compatible.\n");
                foreach (var pair in entry.Value) text.Append("- ").Append(pair).Append('\n');
                text.Append('\n');
            }

            text.Append("## Diff paire par paire\n\n");
            text.Append("Ordre : carrefour, puis cle de paire. Separation : borne prouvee (`ProvenDisjoint`) ; slack : `ExactSlackMeters` du balayage. ");
            text.Append("Feuilles : consommees / a la decision. Debuts de contact dans l'ordre de la cle (A, B).\n\n");
            text.Append("| Carrefour | Mouvement A | Mouvement B | Avant | Apres | Motif apres | Slack / separation (m) | Feuilles | Genre | Debut contact A / B (m) |\n");
            text.Append("|---|---|---|---|---|---|---|---|---|---|\n");
            var rows = new List<KeyValuePair<string, AutomatedPairDecisionRecord>>();
            foreach (var record in after.Records)
            {
                rows.Add(new KeyValuePair<string, AutomatedPairDecisionRecord>(JunctionText(model, record.JunctionId) + "\n" + record.PairKey, record));
            }

            rows.Sort(delegate(KeyValuePair<string, AutomatedPairDecisionRecord> x, KeyValuePair<string, AutomatedPairDecisionRecord> y)
            {
                return string.CompareOrdinal(x.Key, y.Key);
            });
            foreach (var row in rows)
            {
                var record = row.Value;
                AutomatedPairDecisionRecord old;
                before.TryGetValue(record.PairKey, out old);
                string[] labels = PairLabels(model, keys, record);
                text.Append("| ").Append(JunctionText(model, record.JunctionId)).Append(" | ").Append(labels[0]).Append(" | ").Append(labels[1])
                    .Append(" | ").Append(old == null ? "nouvelle" : old.Classification + " (" + old.ReasonCode + ")")
                    .Append(" | ").Append(record.Classification).Append(" | ").Append(record.ReasonCode)
                    .Append(" | ").Append(F(record.ExactSlackMeters)).Append(" / ").Append(F(record.MinimumSeparationMeters))
                    .Append(" | ").Append(string.IsNullOrEmpty(record.RefinementOutcome) ? "-" : record.RefinementLeaves + " / " + record.RefinementLeavesAtDecision
                        + (record.RefinementComplete ? string.Empty : " (incomplet)"))
                    .Append(" | ").Append(IsZone(record) ? record.ConflictKind : "-")
                    .Append(" | ").Append(IsZone(record) ? F(record.ContactStartSMetersA) + " / " + F(record.ContactStartSMetersB) : "-")
                    .Append(" |\n");
            }

            return text.ToString();
        }

        /// <summary>Cle de la paire dont la zone acceptee porte <paramref name="zoneId"/> dans ce manifeste ; nul si aucune.</summary>
        public static string PairKeyOfZone(string manifestText, string zoneId)
        {
            foreach (var record in ParseManifest(manifestText, true).Records)
            {
                if (record.RoadId == zoneId) return record.PairKey;
            }

            return null;
        }

        /// <summary>
        /// Perimetre du typage v2 (Story 5.53a, option B) : paire ConflictProven par le temoin du balayage, a corridor aval
        /// commun. Une paire classee par le raffinement garde v1 : un elagage avant son premier temoin changerait sa preuve.
        /// </summary>
        public static bool TypingV2Applies(string classification, string reasonCode, bool commonExitCorridor)
        {
            return classification == AutomatedPairClassification.ConflictProven.ToString()
                && reasonCode == SweepWitnessReasonCode && commonExitCorridor;
        }

        /// <summary>
        /// Le genre et les debuts publies sont ceux du typage lie a la revision (« typing-v1|genre|A|B|... »). La version de
        /// typage est celle du perimetre : « typing-v2 » dedans, « typing-v1 » dehors ; un ecart signale une derive de
        /// <c>Classify</c> ou un manifeste anterieur a la 5.53a.
        /// </summary>
        private static bool TypingMatches(AutomatedPairDecisionRecord record)
        {
            string[] parts = (record.TypingCanonical ?? string.Empty).Split('|');
            float startA;
            float startB;
            return parts.Length >= 4
                && parts[0] == (TypingV2Applies(record.Classification, record.ReasonCode, record.CommonExitCorridor) ? "typing-v2" : "typing-v1")
                && parts[1] == record.ConflictKind
                && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out startA)
                && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out startB)
                && Same(startA, record.ContactStartSMetersA) && Same(startB, record.ContactStartSMetersB);
        }

        private static bool Same(float a, float b)
        {
            return Math.Abs(a - b) <= 1e-6f * Math.Max(1f, Math.Abs(a));
        }

        private static bool IsZone(AutomatedPairDecisionRecord record)
        {
            return record.Active && record.Decision == ConflictDecisionKind.Accepted.ToString();
        }

        private static Dictionary<string, AutomatedPairDecisionRecord> ByPair(AutomatedPairDecisionManifest manifest)
        {
            var map = new Dictionary<string, AutomatedPairDecisionRecord>(StringComparer.Ordinal);
            foreach (var record in manifest.Records) map[record.PairKey] = record;
            return map;
        }

        private static string JunctionText(CompiledRoadModel model, string junctionId)
        {
            RoadId id;
            Junction junction;
            return RoadId.TryParse(junctionId, out id) && model.TryGetJunction(id, out junction) && !string.IsNullOrEmpty(junction.Label)
                ? junction.Label : junctionId;
        }

        /// <summary>Libelles dans l'ordre de la cle de paire, comme les debuts de contact.</summary>
        private static string[] PairLabels(CompiledRoadModel model, Dictionary<RoadId, string> keys, AutomatedPairDecisionRecord record)
        {
            string a = MovementText(model, record.MovementAId);
            string b = MovementText(model, record.MovementBId);
            RoadId idA;
            string keyA;
            bool sweepAFirst = RoadId.TryParse(record.MovementAId, out idA) && keys.TryGetValue(idA, out keyA)
                && record.PairKey.StartsWith(keyA + "\n", StringComparison.Ordinal);
            return sweepAFirst ? new[] { a, b } : new[] { b, a };
        }

        private static string MovementText(CompiledRoadModel model, string movementId)
        {
            RoadId id;
            CompiledJunctionMovement movement;
            return RoadId.TryParse(movementId, out id) && model.TryGetMovement(id, out movement) ? ShortLabel(movement.Label) : movementId;
        }

        private static string ShortLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return "?";
            int at = label.IndexOf(": ", StringComparison.Ordinal);
            return at < 0 ? label : label.Substring(at + 2);
        }

        private static int Percentile(List<int> sorted, double q)
        {
            int index = (int)Math.Ceiling(q * sorted.Count) - 1;
            return sorted[Math.Max(0, Math.Min(sorted.Count - 1, index))];
        }

        private static string RefinementIdentity(AuthoredRun run)
        {
            return "refinement-v1|budget=" + RefinementLeafBudget.ToString(CultureInfo.InvariantCulture)
                + "|depth=" + MaxSubdivisionDepth.ToString(CultureInfo.InvariantCulture) + "|" + ConflictSweep.RefinementOrder
                + "|resolution=rho.h_e=" + ConflictSweep.RefinementResolution(run.CandidateModel.ValidationProfile, run.EvidenceParameters)
                    .ToString("R", CultureInfo.InvariantCulture)
                + "|tolerance=" + ProofToleranceMeters.ToString("R", CultureInfo.InvariantCulture)
                + "|typing-v2=containment-terminal,exact-root-dedup,sweep-witness,common-exit-corridor";
        }

        /// <summary>Graphe et trajectoires prolongees du modele des candidats, construits une fois par plan.</summary>
        private sealed class RefinementInputs
        {
            private readonly AuthoredRun _run;
            private readonly SweepGraph _graph;
            private readonly Dictionary<RoadId, List<List<SweepPose>>> _paths = new Dictionary<RoadId, List<List<SweepPose>>>();
            private readonly Dictionary<string, double> _seconds;

            public RefinementInputs(AuthoredRun run, Dictionary<string, double> seconds)
            {
                _seconds = seconds;
                if (run.OffsetBounds == null || !run.OffsetBounds.Closed)
                {
                    throw new InvalidOperationException("Politique v3 : bornes d'ecart cinematiques fermees requises.");
                }

                _run = run;
                _graph = SweepGraph.FromModel(run.CandidateModel);
            }

            public PairRefinement Refine(PairSweep sweep, string pair, bool containmentTerminal)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var refinement = ConflictSweep.Refine(_graph, sweep.MovementA, sweep.MovementB, Paths(sweep.MovementA), Paths(sweep.MovementB),
                    _run.CandidateModel.ValidationProfile, _run.EvidenceParameters, _run.OffsetBounds,
                    ProofToleranceMeters, MaxSubdivisionDepth, RefinementLeafBudget, containmentTerminal);
                _seconds[pair] = watch.Elapsed.TotalSeconds;
                return refinement;
            }

            private List<List<SweepPose>> Paths(RoadId movement)
            {
                List<List<SweepPose>> paths;
                if (!_paths.TryGetValue(movement, out paths))
                {
                    string failure;
                    paths = ConflictSweep.Paths(_graph, movement, ConflictSweep.Reach(_run.CandidateModel.ValidationProfile), out failure);
                    if (failure != null)
                    {
                        throw new InvalidOperationException("Politique v3 : trajectoires du mouvement " + movement + " : " + failure);
                    }

                    _paths[movement] = paths;
                }

                return paths;
            }
        }

        /// <summary>Typage de la zone dans l'ordre de la cle de paire (MovementKeyA, MovementKeyB).</summary>
        private static ZoneTyping TypeZone(PairRefinement refinement, CompiledJunctionMovement a, CompiledJunctionMovement b,
            AutomatedPairClassification classification, bool active, bool sweepAFirst)
        {
            if (!active)
            {
                return ZoneTyping.Conservative;
            }

            var typing = ZoneTyping.Of(refinement, a.LengthMeters, b.LengthMeters, a.ToCorridorId == b.ToCorridorId,
                classification == AutomatedPairClassification.ConflictProven);
            return sweepAFirst ? typing : typing.Swapped();
        }

        private static bool ContactWitness(PairSweep sweep, RoadModelValidationProfile profile)
        {
            // Gonflement du balayage lui-meme (a_e compris, restes exclus : le temoin est une paire de poses reelles).
            float inflation = sweep.BaseInflationMeters > 0f ? sweep.BaseInflationMeters : ConflictSweep.Inflation(profile);
            return sweep.HasWitness && !sweep.WitnessA.Degenerate && !sweep.WitnessB.Degenerate
                && ConflictSweep.RectangleDistance(sweep.WitnessA, sweep.WitnessB,
                    ConflictSweep.HalfLength(profile), profile.MaxVehicleHalfWidthMeters)
                    <= 2f * inflation + ProofToleranceMeters;
        }

        private static string Evidence(string pair, PairSweep sweep, AutomatedPairClassification classification, string reasonCode,
            float separation, string movementAHash, string movementBHash)
        {
            return string.Join("|", new[]
            {
                pair.Replace("\n", "|"), classification.ToString(), reasonCode, sweep.Relation.ToString(),
                movementAHash, movementBHash, sweep.PathsA.ToString(CultureInfo.InvariantCulture), sweep.PathsB.ToString(CultureInfo.InvariantCulture),
                F(sweep.ExactSlackMeters), F(sweep.EnvelopeSlackMeters), F(separation),
                sweep.HasWitness ? V(sweep.WitnessA.Position) : "none", sweep.HasWitness ? V(sweep.WitnessB.Position) : "none",
                sweep.FailClosedReason ?? string.Empty, ProofToleranceMeters.ToString("R", CultureInfo.InvariantCulture),
                MaxSubdivisionDepth.ToString(CultureInfo.InvariantCulture), "dyadic-a-then-b"
            });
        }

        private static ConflictDecision ToDecision(AutomatedPairDecisionRecord record, RoadId id, string pair,
            int fingerprintVersion, int sweepVersion, int policyVersion)
        {
            string[] keys = pair.Split('\n');
            return new ConflictDecision
            {
                Id = id,
                MovementKeyA = keys[0],
                MovementKeyB = keys[1],
                Decision = (ConflictDecisionKind)Enum.Parse(typeof(ConflictDecisionKind), record.Decision),
                Reason = record.Reason,
                GeometryFingerprint = record.GeometryFingerprint,
                DecisionRevisionId = record.DecisionRevisionId,
                EvidenceHash = record.EvidenceHash,
                DecisionRunId = record.DecisionRunId,
                SupersedesDecisionRevisionId = record.SupersedesDecisionRevisionId,
                Classification = (AutomatedPairClassification)Enum.Parse(typeof(AutomatedPairClassification), record.Classification),
                ModelVersion = record.ModelVersion,
                ImporterVersion = V1RoadModelImporter.ImporterVersion,
                PipelineVersion = AuthoredRoadModel.PipelineVersion,
                CompilerSchemaVersion = RoadModelCompiler.CompilerSchemaVersion,
                FingerprintSchemaVersion = fingerprintVersion,
                ConflictSweepAlgorithmVersion = sweepVersion,
                DecisionPolicyVersion = policyVersion,
                Kind = string.IsNullOrEmpty(record.ConflictKind) ? ConflictKind.Crossing
                    : (ConflictKind)Enum.Parse(typeof(ConflictKind), record.ConflictKind),
                ContactStartSMetersA = record.ContactStartSMetersA,
                ContactStartSMetersB = record.ContactStartSMetersB
            };
        }

        private static AuthoringDecisions CopyNonConflicts(AuthoringDecisions source)
        {
            var copy = new AuthoringDecisions();
            copy.Controls.AddRange(source.Controls);
            copy.Widths.AddRange(source.Widths);
            copy.Dispositions.AddRange(source.Dispositions);
            copy.DeferredFields.AddRange(source.DeferredFields);
            return copy;
        }

        /// <summary>Revision d'une decision ; en v3, le typage (genre, debuts de contact, raffinement) y est lie.</summary>
        private static string Revision(string pair, string geometry, string decision, string classification,
            string evidenceHash, int policyVersion, string typingCanonical)
        {
            string revision = pair + "\n" + geometry + "\n" + decision + "\n" + classification + "\n"
                + policyVersion.ToString(CultureInfo.InvariantCulture) + "\n" + evidenceHash;
            return Hash(policyVersion >= RefinedDecisionPolicyVersion ? revision + "\n" + Hash(typingCanonical) : revision);
        }

        private static string PreviousRevision(ConflictDecision decision, string pair)
        {
            if (string.IsNullOrEmpty(decision.MovementKeyA))
            {
                return string.Empty;
            }

            return Hash("historical\n" + pair + "\n" + decision.Decision + "\n" + (decision.GeometryFingerprint ?? string.Empty)
                + "\n" + decision.Id + "\n" + (decision.Reason ?? string.Empty));
        }

        private static string DiffHash(IList<PairSweep> sweeps, Dictionary<RoadId, string> keys)
        {
            var rows = new List<string>(sweeps.Count);
            foreach (var sweep in sweeps)
            {
                rows.Add(AuthoredRoadModel.PairKey(keys[sweep.MovementA], keys[sweep.MovementB]) + "|" + sweep.Relation + "|"
                    + F(sweep.ExactSlackMeters) + "|" + F(sweep.EnvelopeSlackMeters) + "|" + (sweep.FailClosedReason ?? string.Empty));
            }

            rows.Sort(StringComparer.Ordinal);
            return Hash(string.Join("\n", rows.ToArray()));
        }

        private static string ProfileHash(RoadModelValidationProfile profile)
        {
            return Hash(string.Join("|", new[]
            {
                F(profile.MaxVehicleLengthMeters), F(profile.MaxVehicleHalfWidthMeters), F(profile.LateralClearanceMarginMeters),
                F(V1RoadModelImporter.ChordToleranceMeters), F(ProofToleranceMeters), MaxSubdivisionDepth.ToString(CultureInfo.InvariantCulture)
            }));
        }

        private static bool SameInputs(AutomatedPairDecisionManifest manifest, string modelVersion, string sourceHash,
            string lineageHash, string diffHash, string profileHash, string parametersHash,
            int policyVersion, int sweepVersion, int fingerprintVersion)
        {
            return manifest.ModelVersion == modelVersion && manifest.SourceHash == sourceHash && manifest.LineageHash == lineageHash
                && manifest.FreshDifferentialHash == diffHash && manifest.ProfileHash == profileHash
                && manifest.ApprovalSha256 == ApprovalSha256 && manifest.DecisionPolicyVersion == policyVersion
                && manifest.ConflictSweepAlgorithmVersion == sweepVersion && manifest.FingerprintSchemaVersion == fingerprintVersion
                && (manifest.EvidenceParametersHash ?? string.Empty) == parametersHash;
        }

        private static void RequireRun(AuthoredRun run)
        {
            if (run == null || run.Import == null || run.Decisions == null || run.CandidateModel == null || run.PairSweeps == null)
            {
                throw new InvalidOperationException("Politique 5.50 : pipeline frais incomplet ; aucune decision ne peut etre produite.");
            }
        }

        private static AutomatedPairDecisionManifest ParseManifest(string text, bool required)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (required)
                {
                    throw new InvalidOperationException("Politique 5.50 : manifeste absent.");
                }

                return null;
            }

            AutomatedPairDecisionManifest manifest;
            try
            {
                manifest = JsonUtility.FromJson<AutomatedPairDecisionManifest>(text);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException("Politique 5.50 : manifeste illisible : " + exception.Message);
            }

            if (manifest == null || manifest.ApprovalId != ApprovalId || manifest.ApprovalSha256 != ApprovalSha256)
            {
                throw new InvalidOperationException("Politique 5.50 : manifeste ou versions incompatibles.");
            }

            bool legacy = manifest.Format == ManifestFormatVersion
                && manifest.DecisionPolicyVersion == DecisionPolicyVersion
                && manifest.FingerprintSchemaVersion == PairGeometryFingerprint.FingerprintSchemaVersion
                && manifest.ConflictSweepAlgorithmVersion == ConflictSweep.AlgorithmVersion;
            bool kinematic = manifest.Format == KinematicManifestFormatVersion
                && manifest.DecisionPolicyVersion == KinematicDecisionPolicyVersion
                && manifest.FingerprintSchemaVersion == PairGeometryFingerprint.EvidenceFingerprintSchemaVersion
                && manifest.ConflictSweepAlgorithmVersion == ConflictSweep.KinematicAlgorithmVersion
                && !string.IsNullOrEmpty(manifest.EvidenceParametersCanonical)
                && manifest.EvidenceParametersHash == Hash(manifest.EvidenceParametersCanonical);
            bool refined = manifest.Format == RefinedManifestFormatVersion
                && manifest.DecisionPolicyVersion == RefinedDecisionPolicyVersion
                && manifest.FingerprintSchemaVersion == PairGeometryFingerprint.EvidenceFingerprintSchemaVersion
                && manifest.ConflictSweepAlgorithmVersion == ConflictSweep.KinematicAlgorithmVersion
                && manifest.RefinementLeafBudget == RefinementLeafBudget
                && manifest.SubdivisionOrder == ConflictSweep.RefinementOrder
                && !string.IsNullOrEmpty(manifest.EvidenceParametersCanonical)
                && manifest.EvidenceParametersHash == Hash(manifest.EvidenceParametersCanonical);
            if (!legacy && !kinematic && !refined)
                throw new InvalidOperationException("Politique 5.50 : manifeste ou versions incompatibles.");

            if (string.IsNullOrEmpty(manifest.SupersededManifestText)
                ? !string.IsNullOrEmpty(manifest.SupersededManifestHash)
                : manifest.SupersededManifestHash != Hash(manifest.SupersededManifestText))
                throw new InvalidOperationException("Politique 5.50 : archive du manifeste historique alteree.");

            return manifest;
        }

        private static string GitCommit()
        {
            try
            {
                string root = Directory.GetParent(Application.dataPath).FullName;
                string head = File.ReadAllText(Path.Combine(root, ".git", "HEAD")).Trim();
                if (!head.StartsWith("ref: ", StringComparison.Ordinal))
                {
                    return head;
                }

                return File.ReadAllText(Path.Combine(root, ".git", head.Substring(5).Replace('/', Path.DirectorySeparatorChar))).Trim();
            }
            catch
            {
                return "unavailable";
            }
        }

        private static string Hash(string value)
        {
            return V1SourceSet.Sha256Hex(value ?? string.Empty);
        }

        private static string F(float value)
        {
            if (float.IsPositiveInfinity(value)) return "+inf";
            if (float.IsNegativeInfinity(value)) return "-inf";
            if (float.IsNaN(value)) return "nan";
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string V(Vector3 value)
        {
            return F(value.x) + "," + F(value.y) + "," + F(value.z);
        }

        private static string Display(string pair)
        {
            return pair.Replace("\n", " x ");
        }
    }

    [Serializable]
    internal sealed class AutomatedPairDecisionManifest
    {
        public int Format;
        public string ApprovalId;
        public string ApprovalSha256;
        public string EngineCommit;
        public string DecisionRunId;
        public string ModelVersion;
        public string SourceHash;
        public string LineageHash;
        public string ProfileHash;
        public string InputDecisionsHash;
        public string FreshDifferentialHash;
        public int ImporterVersion;
        public int PipelineVersion;
        public int CompilerSchemaVersion;
        public int FingerprintSchemaVersion;
        public int ConflictSweepAlgorithmVersion;
        public int DecisionPolicyVersion;
        public string EvidenceParametersCanonical;
        public string EvidenceParametersHash;
        public string SupersededManifestText;
        public string SupersededManifestHash;
        public float ProofToleranceMeters;
        public int MaxSubdivisionDepth;
        public string SubdivisionOrder;
        public int RefinementLeafBudget;
        public AutomatedPairDecisionRecord[] Records;
    }

    [Serializable]
    internal sealed class AutomatedPairDecisionRecord
    {
        public string PairKey;
        public string JunctionId;
        public string MovementAId;
        public string MovementBId;
        public string MovementAHash;
        public string MovementBHash;
        public string GeometryFingerprint;
        public string Classification;
        public string Decision;
        public bool Active;
        public bool Tombstone;
        public string RoadId;
        public string ReasonCode;
        public string Reason;
        public string EvidenceCanonical;
        public string EvidenceHash;
        public string DecisionRevisionId;
        public string SupersedesDecisionRevisionId;
        public string DecisionRunId;
        public string ModelVersion;
        public string Relation;
        public int PathsA;
        public int PathsB;
        public float MinimumSeparationMeters;
        public float ExactSlackMeters;
        public float EnvelopeSlackMeters;

        // Politique v3 (Story 5.53) : raffinement et typage, vides pour une politique anterieure.
        public string RefinementOutcome;
        public int RefinementRoots;
        public int RefinementLeaves;
        public int RefinementLeavesAtDecision;
        public int RefinementResolutionLeaves;
        public bool RefinementComplete;
        public bool CommonExitCorridor;
        public string ConflictKind;
        public float ContactStartSMetersA;
        public float ContactStartSMetersB;
        public string TypingCanonical;
    }
}
#endif
