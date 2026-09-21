using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// ANO-5.18-03 (recette manuelle du 2026-09-21, video ANO-5.18-03.mp4). Trois symptomes -- virages
    /// en grand arc, arrets AU MILIEU du carrefour, interblocage central -- et une seule cause de
    /// representation : le noeud de decision d'une jonction est authore au CENTRE de l'aire de
    /// conflit, et tout se calculait depuis lui.
    ///
    /// Mesures du district (sondes Story518JunctionGeometryProbe / Story518TopologyProbe, 2026-09-21) :
    /// carrefour central en 16x16 centre sur l'origine, chaussee transversale de 8 m de large
    /// (z dans [-4, 4]), quatre approches a 2,00 m du centre exact, quatre noeuds de frontiere deja
    /// presents a 8,00 m. Ces nombres sont RELEVES, pas choisis.
    /// </summary>
    public sealed class Story518JunctionGeometryAndAdmissionTests
    {
        private const string ScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string TrafficSourcePath =
            "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs";

        /// <summary>Demi-largeur et demi-longueur mesurees du vehicule IA, et marge authoree.</summary>
        private const float HalfWidth = 1.03f;
        private const float HalfLength = 2.22f;
        private const float Margin = 0.3f;

        // ----------------------------------------------------------------- geometrie pure

        [Test]
        public void TheLaneAxisCornerIsTheMeetingPointOfTheTwoLaneAxes()
        {
            // Frontiere sud de l'arm nord, virage a droite vers la voie ouest du district.
            var entry = new Vector3(-2f, 0f, 8f);
            var exit = new Vector3(-8f, 0f, 2f);

            Assert.That(LaneGraphRouting.TryResolveLaneAxisCorner(entry, Vector3.back, exit, Vector3.left, out var corner),
                Is.True, "Deux axes perpendiculaires ont toujours un coin.");
            Assert.That(corner.x, Is.EqualTo(-2f).Within(0.001f));
            Assert.That(corner.z, Is.EqualTo(2f).Within(0.001f));
        }

        [Test]
        public void AMovementThatStartsPastItsOwnCornerIsRefusedInsteadOfCurvedBackwards()
        {
            // C'est LE cas d'avant le correctif : le mouvement partait du noeud de decision (-2, 0),
            // deja 2 m au-dela du coin (-2, 2). Aucune courbe ne rattrape cela.
            var pastTheCorner = new Vector3(-2f, 0f, 0f);
            var exit = new Vector3(-8f, 0f, 2f);

            Assert.That(LaneGraphRouting.TryResolveLaneAxisCorner(pastTheCorner, Vector3.back, exit, Vector3.left, out _),
                Is.False, "Un coin situe derriere le depart n'est pas un coin.");
        }

        [Test]
        public void TwoParallelLanesHaveNoCornerAndTheMovementStaysStraight()
        {
            var entry = new Vector3(-2f, 0f, 8f);
            var exit = new Vector3(-2f, 0f, -8f);

            Assert.That(LaneGraphRouting.TryResolveLaneAxisCorner(entry, Vector3.back, exit, Vector3.back, out _), Is.False);

            var middle = LaneGraphRouting.ResolveJunctionTurnPoint(entry, Vector3.back, exit, Vector3.back, 0.5f);
            Assert.That(middle.x, Is.EqualTo(-2f).Within(0.01f), "Un tout droit ne doit pas devier lateralement.");
        }

        [Test]
        public void TheTurnConnectorNeverTravelsBackwardsAlongTheApproach()
        {
            var entry = new Vector3(-2f, 0f, 8f);
            var exit = new Vector3(-8f, 0f, 2f);
            var previous = float.NegativeInfinity;

            for (var i = 0; i <= 32; i++)
            {
                var point = LaneGraphRouting.ResolveJunctionTurnPoint(entry, Vector3.back, exit, Vector3.left, i / 32f);
                var along = Vector3.Dot(point - entry, Vector3.back);
                Assert.That(along, Is.GreaterThanOrEqualTo(previous - 0.001f),
                    "Le connecteur repart en arriere a t=" + (i / 32f) + " : c'est la boucle de ANO-5.18-03.");
                previous = along;
            }
        }

        [Test]
        public void TheTurnProgressGrowsWithTheTravelAlongTheConnector()
        {
            var entry = new Vector3(-2f, 0f, 8f);
            var exit = new Vector3(-8f, 0f, 2f);
            var previous = -1f;

            for (var i = 0; i <= 16; i++)
            {
                var onCurve = LaneGraphRouting.ResolveJunctionTurnPoint(entry, Vector3.back, exit, Vector3.left, i / 16f);
                var progress = LaneGraphRouting.ResolveJunctionTurnProgress(entry, Vector3.back, exit, Vector3.left, onCurve, 32);
                Assert.That(progress, Is.GreaterThanOrEqualTo(previous),
                    "L'avancement doit croitre le long de la courbe.");
                previous = progress;
            }

            Assert.That(previous, Is.GreaterThan(0.9f), "Le dernier point de la courbe est son extremite.");
        }

        [Test]
        public void TheConnectorLengthFollowsTheCornerAndNotTheChord()
        {
            var entry = new Vector3(-2f, 0f, 8f);
            var exit = new Vector3(-8f, 0f, 2f);

            var chord = Vector3.Distance(entry, exit);
            var length = LaneGraphRouting.ResolveJunctionTurnLength(entry, Vector3.back, exit, Vector3.left);

            Assert.That(length, Is.EqualTo(12f).Within(0.01f), "6 m jusqu'au coin, puis 6 m jusqu'a la sortie.");
            Assert.That(length, Is.GreaterThan(chord), "Sous-estimer la longueur fait viser trop loin et couper le virage.");
        }

        [Test]
        public void SegmentsThatTouchAtAnEndpointStillCount()
        {
            // Mesure du 2026-09-21 : sans tolerance d'extremite, l'approche 28 du district lisait sa
            // frontiere de conflit a 10,00 m au lieu de 6,00.
            var meeting = LaneGraphRouting.TrySegmentIntersection(
                new Vector3(-8f, 0f, -34f), new Vector3(0f, 0f, -34f),
                new Vector3(-2f, 0f, -24f), new Vector3(-2f, 0f, -34f), out var point);

            Assert.That(meeting, Is.True, "Un tourne-a-gauche qui rejoint une voie la rencontre bien.");
            Assert.That(point.x, Is.EqualTo(-2f).Within(0.01f));
        }

        [Test]
        public void ParallelLanesThatMergeAreNotReadAsACrossing()
        {
            Assert.That(LaneGraphRouting.TrySegmentIntersection(
                new Vector3(-8f, 0f, 2f), new Vector3(8f, 0f, 2f),
                new Vector3(-4f, 0f, 2f), new Vector3(4f, 0f, 2f), out _), Is.False,
                "Une fusion de voies releve de la place en sortie, pas du croisement.");
        }

        // ----------------------------------------------------------------- admission

        [Test]
        public void AnOccupantOfTheJunctionBeatsAnApproachingClaimantAndTheVerdictStaysAntisymmetric()
        {
            // Deux mouvements qui se croisent : le tout droit nord-sud et le tout droit est-ouest.
            var occupant = new JunctionClaim(1, JunctionApproachRule.PriorityToRight, Vector3.back, 0f, 8, 1UL,
                true, true, true, false, new Vector3(-2f, 0f, 8f), new Vector3(-2f, 0f, -8f), true);
            var approaching = new JunctionClaim(1, JunctionApproachRule.PriorityToRight, Vector3.left, 5f, 10, 2UL,
                true, true, true, false, new Vector3(8f, 0f, 2f), new Vector3(-8f, 0f, 2f));

            Assert.That(JunctionRules.Conflicts(occupant, approaching), Is.True, "Les deux trajets se coupent.");
            Assert.That(JunctionRules.Resolve(occupant, approaching), Is.EqualTo(JunctionVerdict.Proceed),
                "Un mouvement accorde va jusqu'au bout.");
            Assert.That(JunctionRules.Resolve(approaching, occupant), Is.EqualTo(JunctionVerdict.Wait),
                "Et le verdict inverse doit etre exactement l'oppose.");
        }

        [Test]
        public void OccupancyOutranksPriorityToTheRight()
        {
            // L'occupant vient de la GAUCHE de l'autre : sans la notion d'occupation, il perdrait et
            // s'arreterait au milieu de l'aire de conflit.
            var occupant = new JunctionClaim(1, JunctionApproachRule.PriorityToRight, Vector3.forward, 0f, 9, 1UL,
                true, true, true, false, new Vector3(2f, 0f, -8f), new Vector3(2f, 0f, 8f), true);
            var fromTheRight = new JunctionClaim(1, JunctionApproachRule.PriorityToRight, Vector3.left, 3f, 10, 2UL,
                true, true, true, false, new Vector3(8f, 0f, 2f), new Vector3(-8f, 0f, 2f));

            Assert.That(JunctionRules.ComesFromTheRight(occupant, fromTheRight), Is.True,
                "L'autre arrive bien par la droite de l'occupant.");
            Assert.That(JunctionRules.Resolve(occupant, fromTheRight), Is.EqualTo(JunctionVerdict.Proceed));
            Assert.That(JunctionRules.Resolve(fromTheRight, occupant), Is.EqualTo(JunctionVerdict.Wait));
        }

        [Test]
        public void TwoOccupantsFallBackOnTheOrdinaryArbitration()
        {
            // Etat qui ne devrait pas exister -- le second n'aurait pas ete admis. Il ne doit surtout
            // pas produire deux "Proceed" ni deux "Wait".
            var first = new JunctionClaim(1, JunctionApproachRule.PriorityToRight, Vector3.back, 0f, 8, 1UL,
                true, true, true, false, new Vector3(-2f, 0f, 8f), new Vector3(-2f, 0f, -8f), true);
            var second = new JunctionClaim(1, JunctionApproachRule.PriorityToRight, Vector3.left, 0f, 10, 2UL,
                true, true, true, false, new Vector3(8f, 0f, 2f), new Vector3(-8f, 0f, 2f), true);

            var forward = JunctionRules.Resolve(first, second);
            var backward = JunctionRules.Resolve(second, first);
            Assert.That(forward, Is.Not.EqualTo(backward), "L'arbitrage doit rester antisymetrique.");
        }

        // ----------------------------------------------------------------- district reel

        [Test]
        public void EveryJunctionApproachCarriesABoundaryNodeUpstreamOfItsDecisionNode()
        {
            WithLaneGraph(graph =>
            {
                Assert.That(graph.JunctionApproaches.Count, Is.GreaterThan(0));
                foreach (var approach in graph.JunctionApproaches)
                {
                    var node = graph.GetNodePosition(approach.NodeIndex);
                    Assert.That(approach.EntryNode, Is.Not.EqualTo(approach.NodeIndex),
                        "Approche " + approach.NodeIndex + " : sans frontiere, tout se calcule depuis le centre du carrefour.");

                    var along = Vector3.Dot(Vector3.ProjectOnPlane(node - approach.EntryPoint, Vector3.up), approach.EntryForward);
                    Assert.That(along, Is.GreaterThan(1f),
                        "Approche " + approach.NodeIndex + " : la frontiere doit etre EN AMONT du noeud de decision.");
                }
            });
        }

        [Test]
        public void TheStopLineLeavesTheVehicleFrontClearOfTheConflictArea()
        {
            WithLaneGraph(graph =>
            {
                foreach (var approach in graph.JunctionApproaches)
                {
                    Assert.That(approach.ConflictEntryDistance, Is.GreaterThan(0f),
                        "Approche " + approach.NodeIndex + " : aucune frontiere de conflit mesuree.");

                    var stopAlong = Mathf.Max(0f, approach.ConflictEntryDistance - HalfWidth - Margin);
                    Assert.That(stopAlong, Is.GreaterThan(0f),
                        "Approche " + approach.NodeIndex + " : la ligne d'arret tombe sur la frontiere elle-meme.");

                    var node = graph.GetNodePosition(approach.NodeIndex);
                    var nodeAlong = Vector3.Dot(Vector3.ProjectOnPlane(node - approach.EntryPoint, Vector3.up), approach.EntryForward);
                    Assert.That(stopAlong, Is.LessThan(nodeAlong),
                        "Approche " + approach.NodeIndex + " : le vehicule s'arreterait AU NOEUD, donc au milieu du carrefour.");

                    Assert.That(approach.ConflictEntryDistance - stopAlong, Is.GreaterThanOrEqualTo(HalfWidth),
                        "Approche " + approach.NodeIndex + " : l'avant du vehicule mordrait sur l'aire de conflit.");
                }
            });
        }

        [Test]
        public void NoJunctionMovementOfTheDistrictEverLeavesItsOwnTurnCorridor()
        {
            WithLaneGraph(graph =>
            {
                var checkedMovements = 0;
                foreach (var approach in graph.JunctionApproaches)
                {
                    foreach (var exitNode in graph.GetSuccessors(approach.NodeIndex))
                    {
                        var entry = approach.EntryPoint;
                        var forward = approach.EntryForward;
                        var exit = graph.GetNodePosition(exitNode);
                        var exitForward = Vector3.ProjectOnPlane(graph.GetNodeRotation(exitNode) * Vector3.forward, Vector3.up).normalized;
                        if (!LaneGraphRouting.TryResolveLaneAxisCorner(entry, forward, exit, exitForward, out var corner))
                        {
                            continue; // Tout droit : rien a contenir.
                        }

                        checkedMovements++;
                        for (var i = 0; i <= 20; i++)
                        {
                            var point = LaneGraphRouting.ResolveJunctionTurnPoint(entry, forward, exit, exitForward, i / 20f);
                            Assert.That(InsideTriangle(entry, corner, exit, point), Is.True,
                                "Mouvement " + approach.NodeIndex + "->" + exitNode + " : le point a t=" + (i / 20f)
                                + " sort du couloir entree/coin/sortie. C'est le grand arc de ANO-5.18-03.");
                        }
                    }
                }

                Assert.That(checkedMovements, Is.GreaterThan(8), "Le district compte bien des virages a verifier.");
            });
        }

        [Test]
        public void ARightTurnNeverCrossesTheCentreLineOfEitherArm()
        {
            // Le tourne-a-droite du carrefour central : sud depuis (-2, 8) vers l'ouest (-8, 2). Avant
            // le correctif, l'arc plongeait a (-2,70 ; -0,96), soit dans le couloir de la voie est.
            var entry = new Vector3(-2f, 0f, 8f);
            var exit = new Vector3(-8f, 0f, 2f);

            for (var i = 0; i <= 20; i++)
            {
                var point = LaneGraphRouting.ResolveJunctionTurnPoint(entry, Vector3.back, exit, Vector3.left, i / 20f);
                Assert.That(point.x, Is.LessThanOrEqualTo(0.001f), "Le virage franchit l'axe nord-sud du carrefour.");
                Assert.That(point.z, Is.GreaterThanOrEqualTo(1.999f), "Le virage descend dans la voie de sens oppose.");
            }
        }

        // ----------------------------------------------------------------- garde structurelle

        [Test]
        public void AVehicleFacingUsIsNeverReadAsASameLaneQueue()
        {
            var source = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), TrafficSourcePath));

            Assert.That(source, Does.Not.Contain("Mathf.Abs(Vector3.Dot(heading, pathTangent))"),
                "La valeur absolue rendait un vehicule QUI NOUS FAIT FACE aligne sur notre voie : les deux "
                + "se declaraient mutuellement 'leader arrete' et l'attente reciproque devenait legitime.");
            Assert.That(source, Does.Contain("var facing = mobile"),
                "Le sens oppose doit etre nomme, pour que le face-a-face se distingue de la file.");
            Assert.That(source, Does.Contain("decisionReason = TrafficDecisionReason.EmergencyBrake;"),
                "Un face-a-face doit porter son propre motif, avant celui de file.");
        }

        [Test]
        public void ACommittedTraversalIsPublishedToPeersAndReleasedOnlyOnceCleared()
        {
            var source = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), TrafficSourcePath));

            Assert.That(source, Does.Contain("if (committedJunctionKey > 0 && committedApproachNode >= 0"),
                "Un vehicule engage doit rester visible de l'arbitrage : sans cela il disparait a l'instant ou il entre.");
            Assert.That(source, Does.Contain("HasClearedCommittedJunction"),
                "La liberation doit se decider sur le degagement reel, pas sur un minuteur.");
            Assert.That(source, Does.Contain("rearAlong"),
                "Le degagement se mesure sur l'ARRIERE du vehicule.");
        }

        [Test]
        public void ExitSaturationIsALengthComparisonAndNotAPresenceTest()
        {
            var source = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), TrafficSourcePath));

            Assert.That(source, Does.Contain("junctionExitFreeLength >= required"),
                "'Voie de sortie saturee' doit signifier qu'il manque la LONGUEUR de degagement.");
            Assert.That(source, Does.Contain("junctionExitRequiredLength"),
                "La place requise doit etre nommee et exposee au diagnostic.");
            Assert.That(source, Does.Contain("libre=") , "Le diagnostic doit porter la longueur libre mesuree.");
        }

        // ----------------------------------------------------------------- utilitaires

        private static bool InsideTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 point)
        {
            var first = Sign(point, a, b);
            var second = Sign(point, b, c);
            var third = Sign(point, c, a);
            var negative = first < -0.01f || second < -0.01f || third < -0.01f;
            var positive = first > 0.01f || second > 0.01f || third > 0.01f;
            return !(negative && positive);
        }

        private static float Sign(Vector3 first, Vector3 second, Vector3 third)
        {
            return (first.x - third.x) * (second.z - third.z) - (second.x - third.x) * (first.z - third.z);
        }

        private static void WithLaneGraph(System.Action<LaneGraph> body)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                LaneGraph graph = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    graph = root.GetComponentInChildren<LaneGraph>(true);
                    if (graph != null) break;
                }

                Assert.That(graph, Is.Not.Null, "MVP_Run doit porter un LaneGraph.");
                graph.Rebuild();
                body(graph);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
