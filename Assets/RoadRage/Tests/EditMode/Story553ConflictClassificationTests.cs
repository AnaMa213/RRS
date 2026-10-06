using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.53 -- politique v3 sur fixtures synthetiques : raffinement borne des paires candidates sans temoin
    /// (matrice I/O de la spec), typage Crossing / Merge, determinisme, puis schema de compilation 5 (genre et debuts
    /// de contact) avec relecture du schema 4 historique signe.
    /// </summary>
    [Category("Geometry")]
    [Category("Story553")]
    public sealed class Story553ConflictClassificationTests
    {
        private const string HistoricalModelPath = "_bmad-output/implementation-artifacts/gate-a-5-52/historical-signed-5-51/MVP_Run.road-model.json";

        private static readonly RoadId E1 = new RoadId(53UL, 1UL);
        internal static readonly RoadId M1 = new RoadId(53UL, 2UL);
        private static readonly RoadId X1 = new RoadId(53UL, 3UL);
        private static readonly RoadId E2 = new RoadId(53UL, 4UL);
        internal static readonly RoadId M2 = new RoadId(53UL, 5UL);
        private static readonly RoadId X2 = new RoadId(53UL, 6UL);
        private static readonly RoadId E3 = new RoadId(53UL, 7UL);
        internal static readonly RoadId M3 = new RoadId(53UL, 8UL);
        private static readonly RoadId X3 = new RoadId(53UL, 9UL);
        private static readonly RoadId E4 = new RoadId(53UL, 10UL);
        internal static readonly RoadId M4 = new RoadId(53UL, 11UL);

        private SweepGraph _graph;
        private KinematicOffsetBounds _bounds;

        [OneTimeSetUp]
        public void BuildFixture()
        {
            _graph = new SweepGraph();
            // M1 : tout droit vers +z sur x = 0, un seul intervalle de 20 m (borne d'intervalle large, comme une queue de corridor).
            Straight(E1, new Vector3(0f, 0f, -30f), 0f, 20f, 20f);
            Straight(M1, new Vector3(0f, 0f, -10f), 0f, 20f, 20f, true);
            Straight(X1, new Vector3(0f, 0f, 10f), 0f, 20f, 20f);
            // M2 : voie opposee, axes paralleles a 4,0 m.
            Straight(E2, new Vector3(4f, 0f, 30f), Mathf.PI, 20f, 20f);
            Straight(M2, new Vector3(4f, 0f, 10f), Mathf.PI, 20f, 20f, true);
            Straight(X2, new Vector3(4f, 0f, -10f), Mathf.PI, 20f, 20f);
            // M3 : traversee perpendiculaire a M1 en z = 0.
            Straight(E3, new Vector3(-30f, 0f, 0f), 0.5f * Mathf.PI, 20f, 20f);
            Straight(M3, new Vector3(-10f, 0f, 0f), 0.5f * Mathf.PI, 20f, 20f, true);
            Straight(X3, new Vector3(10f, 0f, 0f), 0.5f * Mathf.PI, 20f, 20f);
            // M4 : quart de cercle de rayon 12 qui rejoint M1 tangentiellement en (0, 10) et sort par X1 (corridor aval commun).
            Straight(E4, new Vector3(32f, 0f, -2f), -0.5f * Mathf.PI, 20f, 20f);
            var position = new Vector3(12f, 0f, -2f);
            float heading = -0.5f * Mathf.PI;
            _graph.Add(M4, true, Arc(ref position, ref heading, 1f / 12f, 0.5f * Mathf.PI * 12f, 0.1f));
            Assert.That(Vector3.Distance(position, new Vector3(0f, 0f, 10f)), Is.LessThan(1e-3f), "M4 finit sur l'entree de X1.");

            _graph.Link(E1, M1);
            _graph.Link(M1, X1);
            _graph.Link(E2, M2);
            _graph.Link(M2, X2);
            _graph.Link(E3, M3);
            _graph.Link(M3, X3);
            _graph.Link(E4, M4);
            _graph.Link(M4, X1);

            var seeds = new List<KeyValuePair<RoadId, float>>
            {
                new KeyValuePair<RoadId, float>(E1, 0f), new KeyValuePair<RoadId, float>(E2, 0f),
                new KeyValuePair<RoadId, float>(E3, 0f), new KeyValuePair<RoadId, float>(E4, 0f)
            };
            _bounds = KinematicOffsetBounds.Compute(_graph, seeds, Drivability(), Parameters());
            Assert.That(_bounds.Failures, Is.Empty, "Bornes d'ecart fermees sur la fixture.");
        }

        // ============================================================ matrice I/O

        [Test]
        public void OpposedStraightLanesAreProvenDisjointAfterRefinement()
        {
            var sweep = Sweep(M1, M2);
            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.Candidate), "La borne a gros intervalle autorise un contact.");
            Assert.That(Witness(sweep), Is.False, "Aucun temoin : c'est un ConservativeConflict en v2.");

            var refinement = Refine(M1, M2);
            Assert.That(refinement.Outcome, Is.EqualTo(RefinementOutcome.ProvenDisjoint), refinement.Canonical());
            Assert.That(refinement.MinimumSeparationMeters, Is.GreaterThan(AutomatedPairDecisionPolicy.ProofToleranceMeters));
            Assert.That(refinement.Leaves, Is.InRange(1, AutomatedPairDecisionPolicy.RefinementLeafBudget));
            Assert.That(refinement.ContactA, Is.Empty, "Aucune zone : rien n'est projete.");
        }

        [Test]
        public void ARealCrossingIsProvenAndTypedCrossing()
        {
            var refinement = Refine(M1, M3);
            Assert.That(refinement.Outcome, Is.EqualTo(RefinementOutcome.Witness), refinement.Canonical());
            Assert.That(refinement.Complete, Is.True);
            var typing = ZoneTyping.Of(refinement, 20f, 20f, false, true);
            Assert.That(typing.Kind, Is.EqualTo(ConflictKind.Crossing));
            Assert.That(typing.StartA, Is.GreaterThan(0f).And.LessThan(10f), "Le contact devient possible avant l'axe croise (s = 10).");
            Assert.That(typing.StartB, Is.GreaterThan(0f).And.LessThan(10f));
            Assert.That(refinement.ContactA, Has.Count.EqualTo(1));
            Assert.That(refinement.ContactA[0].y, Is.LessThan(20f), "Un croisement se separe avant la fin du mouvement.");
        }

        [Test]
        public void AMergeIntoACommonExitCorridorIsTypedMergeWithItsContactStarts()
        {
            var sweep = Sweep(M1, M4);
            Assert.That(sweep.Relation, Is.EqualTo(PairRelation.Candidate));
            Assert.That(Witness(sweep), Is.True, "Les deux mouvements finissent au meme point : ConflictProven.");

            var refinement = Refine(M1, M4);
            float lengthM4 = _graph.Elements[M4].Length;
            Assert.That(refinement.Complete, Is.True, refinement.Canonical());
            var typing = ZoneTyping.Of(refinement, 20f, lengthM4, true, true);
            Assert.That(typing.Kind, Is.EqualTo(ConflictKind.Merge), refinement.Canonical());
            Assert.That(typing.StartA, Is.GreaterThan(0f).And.LessThan(20f));
            Assert.That(typing.StartB, Is.GreaterThan(0f).And.LessThan(lengthM4));

            Assert.That(ZoneTyping.Of(refinement, 20f, lengthM4, false, true).Kind, Is.EqualTo(ConflictKind.Crossing),
                "Sans corridor aval commun, jamais Merge.");
            Assert.That(ZoneTyping.Of(refinement, 20f, lengthM4, true, false).Kind, Is.EqualTo(ConflictKind.Crossing),
                "Une paire conservative n'est jamais Merge.");
        }

        [Test]
        public void AMergeThatCrossesBeforeTheConvergenceStaysCrossing()
        {
            var crossesFirst = new PairRefinement { Complete = true, Outcome = RefinementOutcome.Witness };
            crossesFirst.ContactA.Add(new Vector2(2f, 4f));
            crossesFirst.ContactA.Add(new Vector2(9f, 10f));
            crossesFirst.ContactB.Add(new Vector2(5f, 10f));
            var typing = ZoneTyping.Of(crossesFirst, 10f, 10f, true, true);
            Assert.That(typing.Kind, Is.EqualTo(ConflictKind.Crossing), "Contact possible, separation, convergence : Crossing.");
            Assert.That(typing.StartA, Is.EqualTo(2f), "Debut de contact derive de la preuve : le premier intervalle.");
            Assert.That(typing.StartB, Is.EqualTo(5f));

            var notTerminal = new PairRefinement { Complete = true, Outcome = RefinementOutcome.Witness };
            notTerminal.ContactA.Add(new Vector2(6f, 9f));
            notTerminal.ContactB.Add(new Vector2(6f, 10f));
            Assert.That(ZoneTyping.Of(notTerminal, 10f, 10f, true, true).Kind, Is.EqualTo(ConflictKind.Crossing),
                "Un intervalle qui n'atteint pas la fin du mouvement n'est pas terminal.");

            var terminal = new PairRefinement { Complete = true, Outcome = RefinementOutcome.Witness };
            terminal.ContactA.Add(new Vector2(6f, 10f));
            terminal.ContactB.Add(new Vector2(7f, 10f));
            Assert.That(ZoneTyping.Of(terminal, 10f, 10f, true, true).Kind, Is.EqualTo(ConflictKind.Merge));

            terminal.Complete = false;
            var incomplete = ZoneTyping.Of(terminal, 10f, 10f, true, true);
            Assert.That(incomplete.Kind, Is.EqualTo(ConflictKind.Crossing), "Preuve incomplete : Crossing.");
            Assert.That(incomplete.StartA, Is.EqualTo(0f), "Preuve incomplete : debut de contact conservateur.");
            Assert.That(incomplete.StartB, Is.EqualTo(0f));
        }

        [Test]
        public void ASeparationNotStrictlyAboveTheToleranceStaysConservative()
        {
            // Sur deux droites paralleles a pose fixe, toute feuille tend vers la meme separation : avec une tolerance
            // a cette limite, aucune feuille n'est prouvee ni temoin. Le raffinement s'arrete conservateur, motif publie.
            float limit = (4f - 2f * Profile().MaxVehicleHalfWidthMeters) - 2f * ConflictSweep.EvidenceInflation(Profile(), Parameters());
            var unresolved = Refine(M1, M2, limit + 1e-6f, 4, AutomatedPairDecisionPolicy.RefinementLeafBudget);
            Assert.That(unresolved.Outcome, Is.EqualTo(RefinementOutcome.Unresolved), unresolved.Canonical());
            Assert.That(unresolved.Complete, Is.True);
            Assert.That(unresolved.ContactA, Is.Not.Empty, "Les feuilles non prouvees sont publiees.");

            // Resolution du modele : derivee de rho et h_e, jamais une constante ; publiee dans la preuve.
            float resolution = ConflictSweep.RefinementResolution(Profile(), Parameters());
            Assert.That(resolution, Is.EqualTo(ConflictSweep.Rho(Profile()) * Parameters().OffsetGridStepRadians).Within(1e-6f));
            Assert.That(unresolved.ResolutionMeters, Is.EqualTo(resolution));
            StringAssert.Contains("|order=" + ConflictSweep.RefinementOrder + "|resolution=", unresolved.Canonical());
        }

        [Test]
        public void AnExhaustedBudgetStaysConservativeAndNeverDisjoint()
        {
            float limit = (4f - 2f * Profile().MaxVehicleHalfWidthMeters) - 2f * ConflictSweep.EvidenceInflation(Profile(), Parameters());
            var exhausted = Refine(M1, M2, limit + 1e-6f, AutomatedPairDecisionPolicy.MaxSubdivisionDepth, 64);
            Assert.That(exhausted.Outcome, Is.EqualTo(RefinementOutcome.BudgetExhausted), exhausted.Canonical());
            Assert.That(exhausted.Leaves, Is.EqualTo(64), "Le budget est un plafond dur.");
            Assert.That(exhausted.Complete, Is.False);
            Assert.That(ZoneTyping.Of(exhausted, 20f, 20f, true, true).Kind, Is.EqualTo(ConflictKind.Crossing));

            var provable = Refine(M1, M2, AutomatedPairDecisionPolicy.ProofToleranceMeters, AutomatedPairDecisionPolicy.MaxSubdivisionDepth, 1);
            Assert.That(provable.Outcome, Is.EqualTo(RefinementOutcome.BudgetExhausted), "Une paire prouvable mais hors budget n'est pas disjointe.");
        }

        [Test]
        public void ARerunWithIdenticalInputsProposesNoChange()
        {
            foreach (var other in new[] { M2, M3, M4 })
            {
                Assert.That(Refine(M1, other).Canonical(), Is.EqualTo(Refine(M1, other).Canonical()));
            }
        }

        [Test]
        public void ShuffledPathsAndSwappedMembersGiveTheSameDecisions()
        {
            foreach (var other in new[] { M2, M3, M4 })
            {
                var pathsA = Paths(M1);
                var pathsB = Paths(other);
                var forward = ConflictSweep.Refine(_graph, M1, other, pathsA, pathsB, Profile(), Parameters(), _bounds,
                    AutomatedPairDecisionPolicy.ProofToleranceMeters, AutomatedPairDecisionPolicy.MaxSubdivisionDepth,
                    AutomatedPairDecisionPolicy.RefinementLeafBudget);
                pathsA.Reverse();
                pathsB.Reverse();
                var shuffled = ConflictSweep.Refine(_graph, M1, other, pathsA, pathsB, Profile(), Parameters(), _bounds,
                    AutomatedPairDecisionPolicy.ProofToleranceMeters, AutomatedPairDecisionPolicy.MaxSubdivisionDepth,
                    AutomatedPairDecisionPolicy.RefinementLeafBudget);
                Assert.That(shuffled.Canonical(), Is.EqualTo(forward.Canonical()), "Ordre des trajectoires sans effet.");

                var swapped = Refine(other, M1);
                Assert.That(swapped.Outcome, Is.EqualTo(forward.Outcome), "Membres echanges : meme classification.");
                bool common = other == M4;
                float lengthOther = _graph.Elements[other].Length;
                Assert.That(ZoneTyping.Of(swapped, lengthOther, 20f, common, forward.Outcome == RefinementOutcome.Witness).Kind,
                    Is.EqualTo(ZoneTyping.Of(forward, 20f, lengthOther, common, forward.Outcome == RefinementOutcome.Witness).Kind),
                    "Membres echanges : meme genre.");
            }
        }

        // ============================================================ schema 5

        [Test]
        public void AHistoricalSchema4DocumentStillLoadsToItsSignedVersion()
        {
            string text = File.ReadAllText(HistoricalModelPath);
            var source = RoadModelDocument.Load(text);
            Assert.That(source.CompilerSchemaVersion, Is.EqualTo(RoadModelCompiler.MinimumReadableSchemaVersion));
            var compiled = RoadModelCompiler.Compile(source);
            Assert.That(compiled.Version.SchemaVersion, Is.EqualTo(4));
            StringAssert.Contains("\"ModelVersion\":\"" + compiled.Version + "\"", text, "La version liee signee reste verifiable.");
            Assert.Throws<System.FormatException>(() => RoadModelDocument.Serialize(source, default(RoadModelProvenance)),
                "Un schema historique se relit, il ne s'ecrit jamais.");
        }

        [Test]
        public void TypedZonesRoundTripThroughTheSchema5DocumentAndEnterTheVersion()
        {
            var source = CurrentSchemaSource();
            Assert.That(source.ConflictZones, Is.Not.Empty);
            var untyped = RoadModelCompiler.Compile(source).Version;
            Assert.That(untyped.SchemaVersion, Is.EqualTo(RoadModelCompiler.CompilerSchemaVersion));

            source.ConflictZones[0].ContactStartSMeters = new[] { 0.5f, 1f };
            string text = RoadModelDocument.Serialize(source, default(RoadModelProvenance));
            StringAssert.StartsWith("{\"Format\":3,", text);
            var loaded = RoadModelDocument.Load(text);
            Assert.That(loaded.ConflictZones[0].Kind, Is.EqualTo(ConflictKind.Crossing));
            Assert.That(loaded.ConflictZones[0].ContactStartSMeters, Is.EqualTo(new[] { 0.5f, 1f }));
            var typed = RoadModelCompiler.Compile(loaded);
            Assert.That(typed.Version, Is.Not.EqualTo(untyped), "Les debuts de contact entrent dans la charge canonique.");
            Assert.That(typed.ConflictZones[IndexOf(typed, source.ConflictZones[0].Id)].ContactStartSMeters, Is.EqualTo(new[] { 0.5f, 1f }));
        }

        [Test]
        public void TypedZoneVersionsPreserveMemberStartPairsUnderPermutation()
        {
            var source = CurrentSchemaSource();
            source.ConflictZones[0].ContactStartSMeters = new[] { 0.5f, 1f };
            Assert.That(TypingIssues(source), Is.Empty);
            var original = RoadModelCompiler.Compile(source).Version;

            System.Array.Reverse(source.ConflictZones[0].MemberMovementIds);
            System.Array.Reverse(source.ConflictZones[0].ContactStartSMeters);
            Assert.That(TypingIssues(source), Is.Empty);
            Assert.That(RoadModelCompiler.Compile(source).Version, Is.EqualTo(original),
                "Inverser ensemble membres et debuts conserve le meme typage canonique.");

            System.Array.Reverse(source.ConflictZones[0].ContactStartSMeters);
            Assert.That(TypingIssues(source), Is.Empty);
            Assert.That(RoadModelCompiler.Compile(source).Version, Is.Not.EqualTo(original),
                "Echanger seulement les debuts change leur association aux membres et la version.");
        }

        [Test]
        public void TheValidatorRefusesAMergeWithoutCommonExitAndOutOfRangeContactStarts()
        {
            var source = CurrentSchemaSource();
            var movements = new Dictionary<RoadId, JunctionMovement>();
            foreach (var movement in source.Movements) movements[movement.Id] = movement;

            int common = -1;
            int distinct = -1;
            for (int i = 0; i < source.ConflictZones.Length; i++)
            {
                var members = source.ConflictZones[i].MemberMovementIds;
                bool same = movements[members[0]].ToCorridorId == movements[members[1]].ToCorridorId;
                if (same && common < 0) common = i;
                if (!same && distinct < 0) distinct = i;
            }

            Assert.That(common, Is.GreaterThanOrEqualTo(0), "MVP_Run porte des fusions a corridor aval commun.");
            Assert.That(distinct, Is.GreaterThanOrEqualTo(0));

            source.ConflictZones[common].Kind = ConflictKind.Merge;
            source.ConflictZones[common].ContactStartSMeters = new[] { 0f, 0f };
            Assert.That(TypingIssues(source), Is.Empty, "Merge a corridor aval commun et debuts valides : accepte.");

            source.ConflictZones[distinct].Kind = ConflictKind.Merge;
            source.ConflictZones[distinct].ContactStartSMeters = new[] { 0f, 0f };
            Assert.That(TypingIssues(source), Has.Count.EqualTo(1), "Merge sans corridor aval commun : refuse.");
            source.ConflictZones[distinct].Kind = ConflictKind.Crossing;

            float length = movements[source.ConflictZones[distinct].MemberMovementIds[0]].LengthMeters;
            source.ConflictZones[distinct].ContactStartSMeters = new[] { length + 0.01f, 0f };
            Assert.That(TypingIssues(source), Has.Count.EqualTo(1), "Debut de contact hors de [0, L] : refuse.");
            source.ConflictZones[distinct].ContactStartSMeters = new[] { 0f };
            Assert.That(TypingIssues(source), Has.Count.EqualTo(1), "Un debut par membre, ou aucun.");
            source.ConflictZones[distinct].ContactStartSMeters = new float[0];
            source.ConflictZones[common].ContactStartSMeters = new float[0];
            Assert.That(TypingIssues(source), Has.Count.EqualTo(1), "Merge sans debut de contact : refuse.");

            source.ConflictZones[common].ContactStartSMeters = new[] { 0f, 0f };
            source.CompilerSchemaVersion = RoadModelCompiler.MinimumReadableSchemaVersion;
            Assert.That(TypingIssues(source), Is.Not.Empty, "Le schema 4 n'encode aucun typage : refuse.");
        }

        // ============================================================ outils

        private static RoadModelSource CurrentSchemaSource()
        {
            var source = RoadModelDocument.Load(File.ReadAllText(HistoricalModelPath));
            source.CompilerSchemaVersion = 0;
            return source;
        }

        private static int IndexOf(CompiledRoadModel model, RoadId zone)
        {
            for (int i = 0; i < model.ConflictZones.Count; i++)
            {
                if (model.ConflictZones[i].Id == zone) return i;
            }

            return -1;
        }

        private static List<RoadModelValidationIssue> TypingIssues(RoadModelSource source)
        {
            var issues = new List<RoadModelValidationIssue>();
            foreach (var issue in RoadModelValidator.Validate(source))
            {
                if (issue.Code == RoadModelValidationCode.ConflictZoneTypingInvalid) issues.Add(issue);
            }

            return issues;
        }

        internal PairSweep Sweep(RoadId a, RoadId b)
        {
            return ConflictSweep.EvaluateKinematic(Paths(a), Paths(b), Profile(), Parameters(), _bounds);
        }

        internal static bool Witness(PairSweep sweep)
        {
            var profile = Profile();
            return sweep.HasWitness && ConflictSweep.RectangleDistance(sweep.WitnessA, sweep.WitnessB,
                ConflictSweep.HalfLength(profile), profile.MaxVehicleHalfWidthMeters)
                <= 2f * sweep.BaseInflationMeters + AutomatedPairDecisionPolicy.ProofToleranceMeters;
        }

        private PairRefinement Refine(RoadId a, RoadId b)
        {
            return Refine(a, b, AutomatedPairDecisionPolicy.ProofToleranceMeters, AutomatedPairDecisionPolicy.MaxSubdivisionDepth,
                AutomatedPairDecisionPolicy.RefinementLeafBudget);
        }

        private PairRefinement Refine(RoadId a, RoadId b, float tolerance, int depth, int budget)
        {
            return ConflictSweep.Refine(_graph, a, b, Paths(a), Paths(b), Profile(), Parameters(), _bounds, tolerance, depth, budget);
        }

        /// <summary>Raffinement de la politique (tolerance et profondeur declarees), typage v1 ou v2 (Story 5.53a).</summary>
        internal PairRefinement Refine(RoadId a, RoadId b, List<List<SweepPose>> pathsA, List<List<SweepPose>> pathsB, int budget, bool containmentTerminal)
        {
            return ConflictSweep.Refine(_graph, a, b, pathsA, pathsB, Profile(), Parameters(), _bounds,
                AutomatedPairDecisionPolicy.ProofToleranceMeters, AutomatedPairDecisionPolicy.MaxSubdivisionDepth, budget, containmentTerminal);
        }

        internal float Length(RoadId movement)
        {
            return _graph.Elements[movement].Length;
        }

        internal List<List<SweepPose>> Paths(RoadId movement)
        {
            string failure;
            var paths = ConflictSweep.Paths(_graph, movement, ConflictSweep.Reach(Profile()), out failure);
            Assert.That(failure, Is.Null, failure);
            return paths;
        }

        private void Straight(RoadId id, Vector3 start, float heading, float length, float step, bool movement = false)
        {
            _graph.Add(id, movement, Arc(ref start, ref heading, 0f, length, step));
        }

        internal static RoadModelValidationProfile Profile()
        {
            var profile = new RoadModelValidationProfile();
            profile.MaxVehicleHalfWidthMeters = 1.03f;
            profile.MaxVehicleLengthMeters = 4.5f;
            profile.LateralClearanceMarginMeters = 0.25f;
            return profile;
        }

        internal static DrivabilityProfile Drivability()
        {
            return new DrivabilityProfile
            {
                Declared = true,
                WheelbaseMeters = 3.1f,
                ReferencePointAheadRearAxleMeters = 1.55f,
                LowSpeedLockDegrees = 40f,
                HighSpeedLockDegrees = 16f,
                FullReductionSpeedMetersPerSecond = 26f,
                SteeringInactiveBelowMetersPerSecond = 0.25f
            };
        }

        internal static GateAEvidenceParameters Parameters()
        {
            return GateAEvidenceParameters.Create(NominalPoseModel.Kinematic, 0.34f, 0.008f, 0.002f, 256,
                new NominalPoseFeasibilityInputs(300f, 9.81f, 8f));
        }

        /// <summary>Arc (ou droite) de courbure constante, cap mesure de +z vers +x (courbure positive a droite).</summary>
        internal static List<RoadCurveSample> Arc(ref Vector3 position, ref float heading, float curvature, float length, float step)
        {
            var samples = new List<RoadCurveSample>();
            int count = Mathf.Max(1, Mathf.CeilToInt(length / step - 1e-4f));
            Vector3 start = position;
            float h0 = heading;
            for (int i = 0; i <= count; i++)
            {
                float s = length * i / count;
                float h = h0 + curvature * s;
                Vector3 p = Mathf.Abs(curvature) < 1e-9f
                    ? start + new Vector3(Mathf.Sin(h0), 0f, Mathf.Cos(h0)) * s
                    : start + new Vector3((Mathf.Cos(h0) - Mathf.Cos(h)) / curvature, 0f, (Mathf.Sin(h) - Mathf.Sin(h0)) / curvature);
                samples.Add(new RoadCurveSample
                {
                    SMeters = s,
                    Position = p,
                    Tangent = new Vector3(Mathf.Sin(h), 0f, Mathf.Cos(h)),
                    Up = Vector3.up,
                    CurvaturePerMeter = curvature,
                    HalfWidthLeftMeters = 1.75f,
                    HalfWidthRightMeters = 1.75f
                });
            }

            position = samples[samples.Count - 1].Position;
            heading = h0 + curvature * length;
            return samples;
        }
    }
}
