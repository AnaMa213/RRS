using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// UN SEUL MODELE DE RECUPERATION (ANO-5.18-10), et une arbitration qui ne peut plus se figer
    /// (ANO-5.18-11).
    ///
    /// Quatre blocages distincts avaient ete rapportes en recette : cession inexplicable a un pair
    /// immobile, vehicules arretes trop avant dans le carrefour, vehicule tournant prisonnier, et
    /// enchevetrement apres contact. Ils ne sont pas quatre defauts mais deux :
    ///
    /// 1. une ARBITRATION pouvait accorder la priorite a un vehicule qui n'arriverait jamais ;
    /// 2. l'ECHELLE DE RECUPERATION n'etait atteignable que par le predicat de file, donc le
    ///    face-a-face, la cession sans issue et l'enchevetrement n'y entraient jamais.
    /// </summary>
    public sealed class Story518UnifiedRecoveryTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const float HalfWidth = 1.03f;
        private const float Margin = 0.30f;

        // ================================================================ l'escalier, pur

        [Test]
        public void NothingEscalatesWhileTheVehicleStillMakesProgress()
        {
            Assert.That(JunctionRules.ResolveRecoveryStage(0f, 2f, 12f), Is.Zero);
            Assert.That(JunctionRules.ResolveRecoveryStage(1.9f, 2f, 12f), Is.Zero);
        }

        [Test]
        public void TheFirstRungOpensAtTheAuthoredHornDelayAndNotBefore()
        {
            Assert.That(JunctionRules.ResolveRecoveryStage(2f, 2f, 12f), Is.EqualTo(1),
                "Le premier palier est l'evitement local, au delai de klaxon deja authore.");
        }

        [Test]
        public void EachFurtherRungCostsOneAuthoredEscalationDelay()
        {
            // 2 s klaxon puis 12 s par palier : aucune constante de temps nouvelle n'entre ici.
            Assert.That(JunctionRules.ResolveRecoveryStage(13.9f, 2f, 12f), Is.EqualTo(1));
            Assert.That(JunctionRules.ResolveRecoveryStage(14f, 2f, 12f), Is.EqualTo(2), "recul");
            Assert.That(JunctionRules.ResolveRecoveryStage(26f, 2f, 12f), Is.EqualTo(3), "trottoir");
            Assert.That(JunctionRules.ResolveRecoveryStage(38f, 2f, 12f), Is.EqualTo(4), "dernier recours");
        }

        [Test]
        public void TheLadderIsMonotonicAndNeverExceedsItsLastRung()
        {
            var previous = 0;
            for (var seconds = 0f; seconds < 400f; seconds += 0.5f)
            {
                var stage = JunctionRules.ResolveRecoveryStage(seconds, 2f, 12f);
                Assert.That(stage, Is.GreaterThanOrEqualTo(previous), "L'escalier ne redescend jamais seul.");
                Assert.That(stage, Is.InRange(0, 4), "Il n'existe aucun palier au-dela du dernier recours.");
                previous = stage;
            }
        }

        [Test]
        public void DegenerateAuthoringNeverProducesAnInstantLastResort()
        {
            // Des delais nuls ou negatifs ne doivent pas propulser un vehicule au dernier recours au
            // premier pas physique : les seuils sont plancherises, pas lus tels quels.
            Assert.That(JunctionRules.ResolveRecoveryStage(0.05f, 0f, 0f), Is.Zero);
            Assert.That(JunctionRules.ResolveRecoveryStage(0.2f, -5f, -5f), Is.InRange(1, 4));
            Assert.That(JunctionRules.ResolveRecoveryStage(float.NaN, 2f, 12f), Is.Zero);
            Assert.That(JunctionRules.ResolveRecoveryStage(float.PositiveInfinity, 2f, 12f), Is.Zero,
                "Une duree non finie n'est pas une mesure : elle ne declenche rien.");
        }

        // ================================================================ l'arbitration

        [Test]
        public void AStoppedPeerCanNeverWinTheArbitration()
        {
            // "cede le point de conflit, arrivee=0,00s vs +Infini" : une arrivee infinie ne dit pas
            // "il passe en premier", elle dit "il est a l'arret".
            var source = Source("NetworkedAIVehicleDriverController.Traffic.cs");
            var body = Body(source, "private bool ResolveConflictYield()");

            var finiteness = body.IndexOf("var weArrive = float.IsFinite(conflictArrival);", StringComparison.Ordinal);
            var ring = body.IndexOf("JunctionRules.CompareRingPrecedence", StringComparison.Ordinal);
            var arrival = body.IndexOf("TrafficPerception.YieldsAtConflict", StringComparison.Ordinal);

            Assert.That(finiteness, Is.GreaterThanOrEqualTo(0),
                "Le verdict doit commencer par constater qui est reellement en mouvement.");
            Assert.That(finiteness, Is.LessThan(ring),
                "La finitude passe AVANT la priorite a l'anneau : c'est la regle d'anneau qui accordait"
                + " la priorite a un vehicule immobilise sur l'anneau, et la figeait pour toujours.");
            Assert.That(ring, Is.LessThan(arrival),
                "La priorite a l'anneau reste au-dessus de l'ordre d'arrivee.");
        }

        [Test]
        public void EveryVerdictCarriesItsReasonSoAYieldIsNeverUnexplained()
        {
            var body = Body(Source("NetworkedAIVehicleDriverController.Traffic.cs"),
                "private bool ResolveConflictYield()");
            // Chaque chemin de sortie nomme son motif : sans cela "cede le point de conflit" ne se
            // verifie pas en recette, ce qui est exactement le rapport recu.
            var assignments = Regex.Matches(body, @"conflictVerdictReason\s*=").Count;
            var returns = Regex.Matches(body, @"\breturn\b").Count;
            Assert.That(assignments, Is.GreaterThanOrEqualTo(returns),
                "Chaque verdict rendu doit avoir pose son motif : " + assignments + " motifs pour "
                + returns + " sorties.");

            Assert.That(Source("NetworkedAIVehicleDriverController.Traffic.cs"),
                Does.Contain("\" motif=\" + Describe(conflictVerdictReason)"),
                "Le motif doit apparaitre dans la trace de decision, pas seulement en memoire.");
        }

        // ================================================================ l'entree unique

        [Test]
        public void TheRecoveryLadderIsEnteredByAbsenceOfProgressAndNotByTheQueuePredicate()
        {
            var body = Body(Source("NetworkedAIVehicleDriverController.Traffic.cs"),
                "private bool TryEscalateRecovery(DriverProfile profile, float longitudinalSpeed)");
            Assert.That(body, Does.Contain("recoveryStage"),
                "L'echelle se pilote au palier, donc a la duree sans progres.");
            Assert.That(body, Does.Not.Contain("blockedElapsedSeconds"),
                "Elle ne doit plus dependre du predicat de file : c'est ce qui en excluait le"
                + " face-a-face, la cession sans issue et l'enchevetrement.");
        }

        [Test]
        public void OnlyTheMisplacedVehicleLeavesItsLaneInAHeadOnBeforeTheLastRungs()
        {
            var body = Body(Source("NetworkedAIVehicleDriverController.Traffic.cs"),
                "private bool TryEscalateRecovery(DriverProfile profile, float longitudinalSpeed)");
            Assert.That(body, Does.Match(@"headOnConflict && !headOnYield && recoveryStage < 3"),
                "Le vehicule correctement place ne manoeuvre qu'au palier 3, quand l'autre a echoue.");
        }

        [Test]
        public void ABoundedWaitFreezesTheStallClockSoARedLightIsNeverBypassed()
        {
            var source = Source("NetworkedAIVehicleDriverController.Traffic.cs");
            Assert.That(Body(source, "private void TickRouteProgress(DriverProfile profile, float dt)"),
                Does.Match(@"if \(!heldByBoundedWait\) stalledElapsedSeconds \+= dt;"),
                "Sans cette garde, un vehicule correctement arrete a un feu rouge partirait en"
                + " contournement au bout du delai de klaxon.");
            Assert.That(source, Does.Match(@"junctionHeldBySignal = !granted && !signalAllows;"),
                "Le feu est la seule attente dont la donnee authoree garantit la fin.");
            Assert.That(source, Does.Match(
                    @"heldByBoundedWait = junctionHeldBySignal\s*\|\| \(rawQueueing && leaderPeer != null && leaderPeer\.heldByBoundedWait\)"),
                "L'etat doit remonter la file, sinon seule sa tete serait protegee.");
        }

        [Test]
        public void TheLastResortNeverMakesAPedestrianOrTheSceneryPassable()
        {
            var body = Body(Source("NetworkedAIVehicleDriverController.Traffic.cs"),
                "private bool IsStalledPeerVehicle(Collider hit)");
            Assert.That(body, Does.Contain("CharacterController"),
                "Un pieton reste infranchissable a tous les paliers.");
            Assert.That(body, Does.Contain("NetworkedAIVehicleDriverController"),
                "Seul un vehicule IA entre dans l'exemption : le decor n'en fait jamais partie.");
            Assert.That(body, Does.Contain("stuckSpeedThreshold"),
                "Et seulement quand il est A L'ARRET : un vehicule qui roule ne se franchit pas.");

            Assert.That(Body(Source("NetworkedAIVehicleDriverController.Traffic.cs"),
                    "private bool IsPoseClear(Vector3 position, Vector3 previous, Quaternion rotation, float margin,"),
                Does.Match(@"if \(lastResort && IsStalledPeerVehicle\(hit\)\) continue;"),
                "L'exemption ne s'applique qu'au dernier recours.");
        }

        // ================================================================ la ligne d'arret

        [Test]
        public void NoStoppedVehicleKeepsItsNoseInsideTheTurningCorridorAnyMore()
        {
            // Mesure d'avant correctif : 2,34 m d'intrusion a l'intersection et a chaque branche de
            // stop du district. La frontiere etait l'intersection stricte de deux axes SANS
            // EPAISSEUR ; elle est desormais la rencontre des deux COULOIRS.
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var clearance = graph.TrafficSettings != null
                    ? graph.TrafficSettings.JunctionMovementClearance : 0f;
                Assert.That(clearance, Is.GreaterThan(2f),
                    "Le degagement de mouvement doit etre authore : sans lui la frontiere retombe"
                    + " sur des axes sans epaisseur.");

                var approaches = new List<JunctionApproachInfo>();
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    if (graph.TryGetJunctionApproach(i, out var info) && info.JunctionKey > 0) approaches.Add(info);
                }

                Assert.That(approaches.Count, Is.EqualTo(16), "Le district porte seize approches authorees.");

                var worst = 0f;
                var worstLabel = string.Empty;
                foreach (var own in approaches)
                {
                    var stopLine = Mathf.Max(0f, own.ConflictEntryDistance - HalfWidth - Margin);
                    var required = RequiredStopLine(graph, approaches, own, clearance);
                    var intrusion = stopLine - required;
                    if (intrusion > worst) { worst = intrusion; worstLabel = own.JunctionId + " noeud " + own.NodeIndex; }
                }

                Assert.That(worst, Is.LessThan(0.05f),
                    "L'avant d'un vehicule arrete a sa ligne doit rester HORS du couloir de virage ;"
                    + " intrusion mesuree " + worst.ToString("0.00") + " m (" + worstLabel + ").");
            });
        }

        [Test]
        public void AStopLineIsNeverPlacedBeyondItsOwnDecisionNode()
        {
            // Une ligne posee au-dela ferait attendre le vehicule DANS le carrefour qu'il doit
            // justement tenir libre.
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    if (!graph.TryGetJunctionApproach(i, out var approach) || approach.JunctionKey <= 0) continue;
                    var nodeAlong = Vector3.Dot(
                        Vector3.ProjectOnPlane(graph.GetNodePosition(approach.NodeIndex) - approach.EntryPoint, Vector3.up),
                        approach.EntryForward);
                    Assert.That(approach.ConflictEntryDistance, Is.LessThanOrEqualTo(nodeAlong + 0.01f),
                        "Approche " + i + " : frontiere " + approach.ConflictEntryDistance.ToString("0.00")
                        + " m au-dela du noeud de decision a " + nodeAlong.ToString("0.00") + " m.");
                }
            });
        }

        [Test]
        public void TheStopLineMovedUpstreamComparedToTheZeroThicknessRule()
        {
            // Preuve de non-regression du correctif lui-meme : au moins une approche du district doit
            // s'arreter STRICTEMENT plus tot qu'avec l'ancienne regle, sinon rien n'a change.
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                var moved = 0;
                for (var i = 0; i < graph.NodeCount; i++)
                {
                    if (!graph.TryGetJunctionApproach(i, out var approach) || approach.JunctionKey <= 0) continue;
                    var nodeAlong = Vector3.Dot(
                        Vector3.ProjectOnPlane(graph.GetNodePosition(approach.NodeIndex) - approach.EntryPoint, Vector3.up),
                        approach.EntryForward);
                    if (approach.ConflictEntryDistance < nodeAlong - 0.5f) moved++;
                }

                Assert.That(moved, Is.GreaterThanOrEqualTo(8),
                    "Le district compte huit approches au moins dont la frontiere recule reellement.");
            });
        }

        // ================================================================ utilitaires

        private static float RequiredStopLine(LaneGraph graph, List<JunctionApproachInfo> approaches,
            in JunctionApproachInfo own, float clearance)
        {
            var nearest = float.PositiveInfinity;
            foreach (var ownExit in graph.GetSuccessors(own.NodeIndex))
            {
                var mine = Movement(graph, own, ownExit);
                foreach (var other in approaches)
                {
                    if (other.JunctionKey != own.JunctionKey || other.NodeIndex == own.NodeIndex) continue;
                    foreach (var otherExit in graph.GetSuccessors(other.NodeIndex))
                    {
                        var theirs = Movement(graph, other, otherExit);
                        for (var i = 0; i < mine.Length; i++)
                        {
                            if (!theirs.Any(point =>
                                    Vector3.ProjectOnPlane(mine[i] - point, Vector3.up).magnitude <= clearance))
                                continue;

                            var along = Vector3.Dot(
                                Vector3.ProjectOnPlane(mine[i] - own.EntryPoint, Vector3.up), own.EntryForward);
                            if (along > 0.01f && along < nearest) nearest = along;
                            break;
                        }
                    }
                }
            }

            var nodeAlong = Vector3.Dot(
                Vector3.ProjectOnPlane(graph.GetNodePosition(own.NodeIndex) - own.EntryPoint, Vector3.up),
                own.EntryForward);
            if (nodeAlong > 0.01f) nearest = Mathf.Min(nearest, nodeAlong);
            return Mathf.Max(0f, nearest - HalfWidth - Margin);
        }

        private static Vector3[] Movement(LaneGraph graph, in JunctionApproachInfo approach, int exitNode)
        {
            const int samples = 41;
            var points = new Vector3[samples];
            var end = graph.GetNodePosition(exitNode);
            var exitForward = Vector3.ProjectOnPlane(graph.GetNodeRotation(exitNode) * Vector3.forward, Vector3.up).normalized;
            for (var i = 0; i < samples; i++)
            {
                points[i] = LaneGraphRouting.ResolveJunctionTurnPoint(
                    approach.EntryPoint, approach.EntryForward, end, exitForward, i / (float)(samples - 1));
            }

            return points;
        }

        private static string Source(string fileName)
        {
            var path = "Assets/RoadRage/Features/Vehicles/" + fileName;
            Assert.That(System.IO.File.Exists(path), Is.True, path + " introuvable");
            return System.IO.File.ReadAllText(path).Replace("\r\n", "\n");
        }

        /// <summary>Corps d'une methode, delimite par accolades equilibrees depuis sa signature.</summary>
        private static string Body(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "Signature introuvable : " + signature);
            var open = source.IndexOf('{', start + signature.Length);
            Assert.That(open, Is.GreaterThanOrEqualTo(0));
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}' && --depth == 0) return source.Substring(open, i - open + 1);
            }

            Assert.Fail("Corps non equilibre : " + signature);
            return string.Empty;
        }

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
