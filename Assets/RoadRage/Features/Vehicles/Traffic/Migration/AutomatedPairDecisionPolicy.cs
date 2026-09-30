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
        public const float ProofToleranceMeters = 0.0001f;
        public const int MaxSubdivisionDepth = 20;
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
            int policyVersion = legacy ? DecisionPolicyVersion : KinematicDecisionPolicyVersion;
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
            string runId = Hash(legacy ? runIdentity : runIdentity + "\n" + parametersHash);

            var manifest = new AutomatedPairDecisionManifest
            {
                Format = legacy ? ManifestFormatVersion : KinematicManifestFormatVersion,
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
                SubdivisionOrder = "dyadic-a-then-b",
                Records = new AutomatedPairDecisionRecord[run.PairSweeps.Count]
            };

            var output = CopyNonConflicts(run.Decisions);
            var plan = new AutomatedPairDecisionPlan { DecisionRunId = runId };
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
                AutomatedPairDecisionRecord record = Classify(run, sweep, pair, keys, runId, modelVersion, prior, policyVersion);
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
                        record.Classification, record.EvidenceHash, manifest.DecisionPolicyVersion))
                {
                    throw new InvalidOperationException("Politique 5.50 : preuve ou revision alteree pour " + Display(record.PairKey) + ".");
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
                    || decision.GeometryFingerprint != record.GeometryFingerprint)
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
            int policyVersion)
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
            else if (sweep.Relation == PairRelation.Candidate && ContactWitness(sweep, run.CandidateModel.ValidationProfile))
            {
                classification = AutomatedPairClassification.ConflictProven;
                decision = ConflictDecisionKind.Accepted.ToString();
                reasonCode = "inflated-rectangles-overlap";
                reason = "Un temoin de poses donne un recouvrement des rectangles orientes gonfles.";
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
            string evidenceHash = Hash(evidence);
            string revision = Revision(pair, geometry, decision, classification.ToString(), evidenceHash, policyVersion);
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
                EnvelopeSlackMeters = sweep.EnvelopeSlackMeters
            };
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
                DecisionPolicyVersion = policyVersion
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

        private static string Revision(string pair, string geometry, string decision, string classification,
            string evidenceHash, int policyVersion)
        {
            return Hash(pair + "\n" + geometry + "\n" + decision + "\n" + classification + "\n"
                + policyVersion.ToString(CultureInfo.InvariantCulture) + "\n" + evidenceHash);
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
            if (!legacy && !kinematic)
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
    }
}
#endif
