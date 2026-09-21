using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// PRIORITE A L'ANNEAU. Recette : "les voitures ne comprennent pas que les personnes dans le
    /// rond-point sont toujours prioritaires".
    ///
    /// Le defaut n'etait pas un reglage : la regle n'existait pas. Les quatre giratoires du district
    /// ne portaient AUCUNE regle d'approche, et la priorite a l'anneau etait seulement esperee de
    /// l'ordre d'arrivee au point de conflit. Or un vehicule lance vers l'entree peut atteindre ce
    /// point avant celui qui fait le tour : l'ordre d'arrivee lui donnait alors le passage.
    ///
    /// Un giratoire n'est pas une course. Ces tests prouvent la regle et la donnee dont elle depend.
    /// </summary>
    public sealed class Story518RoundaboutPriorityTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

        // ------------------------------------------------------------------ la regle, pure

        [Test]
        public void ACirculatingVehicleNeverYieldsToAnEnteringOne()
        {
            Assert.That(JunctionRules.CompareRingPrecedence(2, 0), Is.EqualTo(1), "Nous circulons : nous passons.");
            Assert.That(JunctionRules.CompareRingPrecedence(0, 2), Is.EqualTo(-1), "Nous entrons : nous cedons.");
        }

        [Test]
        public void TheRuleIsAntisymmetricSoTheTwoPeersNeverAgreeToBothStopOrBothGo()
        {
            foreach (var own in new[] { 0, 1, 2, 3 })
            foreach (var peer in new[] { 0, 1, 2, 3 })
            {
                Assert.That(JunctionRules.CompareRingPrecedence(own, peer),
                    Is.EqualTo(-JunctionRules.CompareRingPrecedence(peer, own)),
                    "Anneaux " + own + " et " + peer + " : le verdict doit s'inverser avec les roles.");
            }
        }

        [Test]
        public void TwoVehiclesOnTheSameRingAreLeftToTheArrivalOrder()
        {
            // Entre deux vehicules du meme anneau il n'y a pas d'entrant, seulement une file : la
            // regle se tait et laisse l'arbitrage ordinaire faire son travail.
            Assert.That(JunctionRules.CompareRingPrecedence(2, 2), Is.EqualTo(0));
            Assert.That(JunctionRules.CompareRingPrecedence(0, 0), Is.EqualTo(0),
                "Hors giratoire la regle n'a rien a dire.");
        }

        [Test]
        public void TwoSeparateRoundaboutsDoNotArbitrateAgainstEachOther()
        {
            Assert.That(JunctionRules.CompareRingPrecedence(1, 3), Is.EqualTo(0),
                "Deux anneaux distincts n'ont aucun point de conflit commun a se disputer.");
        }

        // ------------------------------------------------------------------ la donnee, sur la scene

        [Test]
        public void TheDistrictRoundaboutsAreRecognisedAsRingsWithoutAnyAuthoring()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var rings = new Dictionary<int, int>();
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    var ring = graph.RingIdOf(i);
                    if (ring == 0) continue;
                    rings.TryGetValue(ring, out var count);
                    rings[ring] = count + 1;
                }

                Assert.That(rings.Count, Is.EqualTo(4), "Le district porte quatre giratoires.");
                foreach (var pair in rings)
                {
                    Assert.That(pair.Value, Is.GreaterThanOrEqualTo(5),
                        "Anneau " + pair.Key + " : " + pair.Value + " noeuds, trop peu pour un giratoire.");
                }
            });
        }

        [Test]
        public void EveryRoundaboutEntryNowYieldsToTheRing()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var entries = 0;

                for (var i = 0; i < graph.NodeCount; i++)
                {
                    var fromRing = graph.RingIdOf(i);
                    if (fromRing != 0) continue;

                    foreach (var successor in graph.GetSuccessors(i))
                    {
                        var intoRing = graph.RingIdOf(successor);
                        if (intoRing == 0) continue;

                        entries++;
                        Assert.That(JunctionRules.CompareRingPrecedence(fromRing, intoRing), Is.EqualTo(-1),
                            "L'entree " + i + "->" + successor + " doit ceder a l'anneau " + intoRing + ".");
                        Assert.That(JunctionRules.CompareRingPrecedence(intoRing, fromRing), Is.EqualTo(1),
                            "Et l'anneau " + intoRing + " doit se savoir prioritaire sur " + i + ".");
                    }
                }

                Assert.That(entries, Is.EqualTo(12),
                    "Le district porte douze entrees de giratoire ; " + entries + " ont ete vues.");
            });
        }

        [Test]
        public void ARingNodeIsNeverConfusedWithTheRoadThatFeedsIt()
        {
            // Le noeud d'entree est HORS anneau : c'est ce qui fait que le vehicule qui l'occupe cede
            // encore. S'il en faisait partie, il se declarerait prioritaire sur la ligne de cession.
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var feeders = 0;
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    if (graph.RingIdOf(i) != 0) continue;
                    if (graph.GetSuccessors(i).Any(s => graph.RingIdOf(s) != 0)) feeders++;
                }

                Assert.That(feeders, Is.GreaterThan(0), "Les entrees doivent rester hors de l'anneau.");
            });
        }

        // ------------------------------------------------------------------ utilitaires

        private static LaneGraph ResolveGraph(Scene scene)
        {
            var runRoot = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == "RunRoot");
            Assert.That(runRoot, Is.Not.Null, "RunRoot attendu dans MVP_Run");
            var graph = runRoot.GetComponentInChildren<LaneGraph>(true);
            Assert.That(graph, Is.Not.Null, "LaneGraph attendu sous RunRoot");
            graph.Rebuild();
            return graph;
        }

        private static void WithMvpRun(Action<Scene> body)
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MvpRunScenePath);
            var wasOpen = alreadyOpen.IsValid();
            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);
            try
            {
                body(scene);
            }
            finally
            {
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
