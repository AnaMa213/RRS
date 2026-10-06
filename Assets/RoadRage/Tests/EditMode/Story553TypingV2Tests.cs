using System.Collections.Generic;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.53a -- typing-v2 sur les fixtures synthetiques de la 5.53 : une feuille non prouvee contenue dans l'union de
    /// contact courante est terminale. Equivalence exacte avec v1 (unions, genre, debuts) quand v1 aboutit, aboutissement
    /// sous un budget ou v1 s'arrete, perimetre de la politique, determinisme. Les 36 paires de MVP_Run :
    /// <see cref="Story553TypingV2CampaignTests"/>.
    /// </summary>
    [Category("Geometry")]
    [Category("Story553")]
    public sealed class Story553TypingV2Tests
    {
        private Story553ConflictClassificationTests _fixture;

        [OneTimeSetUp]
        public void BuildFixture()
        {
            _fixture = new Story553ConflictClassificationTests();
            _fixture.BuildFixture();
        }

        [Test]
        public void TypingV2ReproducesV1WheneverV1CompletesAndPrunesSomeLeaves()
        {
            var m1 = Story553ConflictClassificationTests.M1;
            var m4 = Story553ConflictClassificationTests.M4;
            int contained = 0;
            foreach (var pair in new[]
            {
                new[] { m1, Story553ConflictClassificationTests.M2 }, new[] { m1, Story553ConflictClassificationTests.M3 },
                new[] { Story553ConflictClassificationTests.M3, m1 }, new[] { m1, m4 }, new[] { m4, m1 }
            })
            {
                var v1 = Refine(pair[0], pair[1], false);
                var v2 = Refine(pair[0], pair[1], true);
                string label = pair[0] + " x " + pair[1];
                Assert.That(v1.Complete, Is.True, label + " : " + v1.Canonical());
                Assert.That(v2.Complete, Is.True, label + " : " + v2.Canonical());
                Assert.That(v2.ContactA, Is.EqualTo(v1.ContactA), label + " : union A identique.");
                Assert.That(v2.ContactB, Is.EqualTo(v1.ContactB), label + " : union B identique.");
                Assert.That(v2.Leaves, Is.LessThanOrEqualTo(v1.Leaves), label);

                bool common = pair[0] == m4 || pair[1] == m4;
                var typingV1 = ZoneTyping.Of(v1, _fixture.Length(pair[0]), _fixture.Length(pair[1]), common, true);
                var typingV2 = ZoneTyping.Of(v2, _fixture.Length(pair[0]), _fixture.Length(pair[1]), common, true);
                Assert.That(typingV2.Kind, Is.EqualTo(typingV1.Kind), label);
                Assert.That(typingV2.StartA, Is.EqualTo(typingV1.StartA), label);
                Assert.That(typingV2.StartB, Is.EqualTo(typingV1.StartB), label);
                StringAssert.StartsWith("typing-v1|", typingV1.Canonical(v1));
                StringAssert.DoesNotContain("|v2|", v1.Canonical(), "v1 publie la meme preuve qu'avant la 5.53a.");
                StringAssert.StartsWith("typing-v2|", typingV2.Canonical(v2));
                StringAssert.Contains("|v2|proven=" + v2.ProvenLeaves + "|witness=" + v2.WitnessLeaves + "|resolution=" + v2.ResolutionLeaves
                    + "|contained=" + v2.ContainedLeaves, v2.Canonical(), "Feuilles par etat publiees.");
                Assert.That(v1.ContainedLeaves, Is.Zero, "v1 n'elague jamais.");
                contained += v2.ContainedLeaves;
            }

            Assert.That(contained, Is.GreaterThan(0), "L'equivalence n'est pas vide : des feuilles sont terminales par contenance.");
        }

        [Test]
        public void UnderTheBudgetV2NeedsTheMergeIsTypedWhereV1StopsAtZero()
        {
            var m1 = Story553ConflictClassificationTests.M1;
            var m4 = Story553ConflictClassificationTests.M4;
            Assert.That(Story553ConflictClassificationTests.Witness(_fixture.Sweep(m1, m4)), Is.True, "ConflictProven par le balayage.");
            float lengthA = _fixture.Length(m1);
            float lengthB = _fixture.Length(m4);
            var full = ZoneTyping.Of(Refine(m1, m4, false), lengthA, lengthB, true, true);
            var v2 = Refine(m1, m4, true);
            Assert.That(v2.ContainedLeaves, Is.GreaterThan(0), v2.Canonical());

            // Meme plafond pour les deux : celui que v2 consomme. v1 s'y arrete, v2 aboutit (analogue des fusions ouest).
            var v1 = Refine(m1, m4, v2.Leaves, false);
            Assert.That(v1.Complete, Is.False, v1.Canonical());
            var stopped = ZoneTyping.Of(v1, lengthA, lengthB, true, true);
            Assert.That(stopped.Kind, Is.EqualTo(ConflictKind.Crossing));
            Assert.That(stopped.StartA, Is.Zero);
            Assert.That(stopped.StartB, Is.Zero);

            var typed = ZoneTyping.Of(Refine(m1, m4, v2.Leaves, true), lengthA, lengthB, true, true);
            Assert.That(typed.Kind, Is.EqualTo(ConflictKind.Merge));
            Assert.That(typed.StartA, Is.EqualTo(full.StartA), "Debuts derives de la preuve, ceux de v1 complet.");
            Assert.That(typed.StartB, Is.EqualTo(full.StartB));
        }

        [Test]
        public void TypingV2AppliesOnlyToSweepWitnessPairsWithACommonExitCorridor()
        {
            string proven = AutomatedPairClassification.ConflictProven.ToString();
            string witness = AutomatedPairDecisionPolicy.SweepWitnessReasonCode;
            Assert.That(AutomatedPairDecisionPolicy.TypingV2Applies(proven, witness, true), Is.True, "Temoin du balayage, aval commun.");
            Assert.That(AutomatedPairDecisionPolicy.TypingV2Applies(proven, witness, false), Is.False, "Pas d'aval commun : Crossing, aucune passe v2.");
            Assert.That(AutomatedPairDecisionPolicy.TypingV2Applies(proven, "refined-witness", true), Is.False,
                "Classee par le raffinement : v1 inchange.");
            Assert.That(AutomatedPairDecisionPolicy.TypingV2Applies(AutomatedPairClassification.ConservativeConflict.ToString(), witness, true),
                Is.False);
        }

        [Test]
        public void TypingV2IsDeterministicUnderRerun()
        {
            var m1 = Story553ConflictClassificationTests.M1;
            var m4 = Story553ConflictClassificationTests.M4;
            string first = Refine(m1, m4, true).Canonical();
            Assert.That(Refine(m1, m4, true).Canonical(), Is.EqualTo(first), "Seconde execution : aucun changement.");
        }

        [Test]
        public void DuplicateRootsFromBranchingPathsAreSkippedWithoutChangingTheTyping()
        {
            // M1 tout droit ; M4 rejoint M1 tangentiellement puis la sortie bifurque : X1 tout droit, X4 a droite. M4 a donc
            // deux trajectoires prolongees qui partagent tout son parcours, d'ou des racines identiques.
            var e1 = new RoadId(531UL, 1UL);
            var m1 = new RoadId(531UL, 2UL);
            var x1 = new RoadId(531UL, 3UL);
            var e4 = new RoadId(531UL, 4UL);
            var m4 = new RoadId(531UL, 5UL);
            var x4 = new RoadId(531UL, 6UL);
            var graph = new SweepGraph();
            Add(graph, e1, false, new Vector3(0f, 0f, -30f), 0f, 0f, 20f, 20f);
            Add(graph, m1, true, new Vector3(0f, 0f, -10f), 0f, 0f, 20f, 20f);
            Add(graph, x1, false, new Vector3(0f, 0f, 10f), 0f, 0f, 20f, 20f);
            Add(graph, e4, false, new Vector3(32f, 0f, -2f), -0.5f * Mathf.PI, 0f, 20f, 20f);
            Add(graph, m4, true, new Vector3(12f, 0f, -2f), -0.5f * Mathf.PI, 1f / 12f, 0.5f * Mathf.PI * 12f, 0.1f);
            Add(graph, x4, false, new Vector3(0f, 0f, 10f), 0f, 1f / 20f, 20f, 0.5f);
            graph.Link(e1, m1);
            graph.Link(m1, x1);
            graph.Link(e4, m4);
            graph.Link(m4, x1);
            graph.Link(m4, x4);
            var bounds = KinematicOffsetBounds.Compute(graph, new List<KeyValuePair<RoadId, float>>
            {
                new KeyValuePair<RoadId, float>(e1, 0f), new KeyValuePair<RoadId, float>(e4, 0f)
            }, Story553ConflictClassificationTests.Drivability(), Story553ConflictClassificationTests.Parameters());
            Assert.That(bounds.Failures, Is.Empty);

            var pathsA = BranchPaths(graph, m1);
            var pathsB = BranchPaths(graph, m4);
            Assert.That(pathsB, Has.Count.EqualTo(2), "Deux trajectoires prolongees pour M4.");
            System.Func<bool, PairRefinement> refine = v2 => ConflictSweep.Refine(graph, m1, m4, pathsA, pathsB,
                Story553ConflictClassificationTests.Profile(), Story553ConflictClassificationTests.Parameters(), bounds,
                AutomatedPairDecisionPolicy.ProofToleranceMeters, AutomatedPairDecisionPolicy.MaxSubdivisionDepth,
                AutomatedPairDecisionPolicy.RefinementLeafBudget, v2);
            var v1 = refine(false);
            var v2r = refine(true);
            Assert.That(v1.Complete, Is.True, v1.Canonical());
            Assert.That(v2r.Complete, Is.True, v2r.Canonical());
            Assert.That(v2r.DuplicateRoots, Is.GreaterThan(0), v2r.Canonical());
            Assert.That(v2r.Roots, Is.EqualTo(v1.Roots), "Roots reste le nombre de racines criblees.");
            Assert.That(v2r.Leaves, Is.LessThan(v1.Leaves));
            Assert.That(v2r.ContactA, Is.EqualTo(v1.ContactA));
            Assert.That(v2r.ContactB, Is.EqualTo(v1.ContactB));
            Assert.That(v2r.HasWitness, Is.EqualTo(v1.HasWitness));
            StringAssert.Contains("|duplicates=" + v2r.DuplicateRoots, v2r.Canonical());
            Assert.That(v1.DuplicateRoots, Is.Zero, "v1 evalue toutes les racines.");

            // Ordre des trajectoires sans effet, doublons compris : la premiere occurrence retenue ne depend pas de l'entree.
            var reversed = new List<List<SweepPose>>(pathsB);
            reversed.Reverse();
            Assert.That(ConflictSweep.Refine(graph, m1, m4, pathsA, reversed, Story553ConflictClassificationTests.Profile(),
                Story553ConflictClassificationTests.Parameters(), bounds, AutomatedPairDecisionPolicy.ProofToleranceMeters,
                AutomatedPairDecisionPolicy.MaxSubdivisionDepth, AutomatedPairDecisionPolicy.RefinementLeafBudget, true).Canonical(),
                Is.EqualTo(v2r.Canonical()));

            float lengthA = graph.Elements[m1].Length;
            float lengthB = graph.Elements[m4].Length;
            var typingV1 = ZoneTyping.Of(v1, lengthA, lengthB, true, true);
            var typingV2 = ZoneTyping.Of(v2r, lengthA, lengthB, true, true);
            Assert.That(typingV2.Kind, Is.EqualTo(typingV1.Kind));
            Assert.That(typingV2.StartA, Is.EqualTo(typingV1.StartA));
            Assert.That(typingV2.StartB, Is.EqualTo(typingV1.StartB));
        }

        private static void Add(SweepGraph graph, RoadId id, bool movement, Vector3 start, float heading, float curvature, float length, float step)
        {
            graph.Add(id, movement, Story553ConflictClassificationTests.Arc(ref start, ref heading, curvature, length, step));
        }

        private static List<List<SweepPose>> BranchPaths(SweepGraph graph, RoadId movement)
        {
            string failure;
            var paths = ConflictSweep.Paths(graph, movement, ConflictSweep.Reach(Story553ConflictClassificationTests.Profile()), out failure);
            Assert.That(failure, Is.Null, failure);
            return paths;
        }

        private PairRefinement Refine(RoadId a, RoadId b, bool containmentTerminal)
        {
            return Refine(a, b, AutomatedPairDecisionPolicy.RefinementLeafBudget, containmentTerminal);
        }

        private PairRefinement Refine(RoadId a, RoadId b, int budget, bool containmentTerminal)
        {
            return _fixture.Refine(a, b, _fixture.Paths(a), _fixture.Paths(b), budget, containmentTerminal);
        }
    }
}
