using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.18 : regles d'intersection et prevention de l'interblocage.
    ///
    /// Le coeur de la story est une propriete d'INVARIANCE : deux vehicules qui evaluent la meme
    /// situation en tirent le meme verdict, donc exactement un procede. Elle se prouve sans scene, sans
    /// reseau et sans physique -- c'est tout l'objet du modele pur de <see cref="JunctionRules"/> -- et
    /// c'est ce que la premiere moitie de ce fixture etablit.
    ///
    /// La seconde moitie porte la DONNEE : les seuils authores, les plans de feux, et la signalisation
    /// posee sur les prefabs de module. Une regle de priorite lue a l'authoring ne vaut que si elle est
    /// effectivement authoree, et une garde de modele pur ne le verrait pas.
    /// </summary>
    public sealed class Story518IntersectionRulesAndDeadlockPreventionTests
    {
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string IntersectionPrefabPath = "Assets/RoadRage/Prefabs/Greybox_Intersection.prefab";
        private const string TJunctionPrefabPath = "Assets/RoadRage/Prefabs/Greybox_TJunction.prefab";
        private const string TrafficDefPath = "Assets/RoadRage/ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset";
        private const string DriverDefPath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string IntersectionJunctionId = "junction_district_intersection";
        private const string TJunctionJunctionId = "junction_district_tjunction";
        private const int JunctionKey = 1;

        // ------------------------------------------------------------ modele pur : symetrie

        /// <summary>
        /// Matrice representative : les quatre regles, les deux sens d'une meme avenue, les quatre
        /// branches d'un carrefour, la meme voie, une autre jonction, et l'ecart de distance qui
        /// n'arrive jamais a egalite parfaite.
        /// </summary>
        private static JunctionClaim[] Matrix()
        {
            return new[]
            {
                Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 101UL),
                Approach(JunctionApproachRule.PriorityToRight, 90f, 6.4f, 11, 102UL),
                Approach(JunctionApproachRule.PriorityToRight, 180f, 5.2f, 12, 103UL),
                Approach(JunctionApproachRule.PriorityToRight, 270f, 6.8f, 13, 104UL),
                Approach(JunctionApproachRule.PriorityRoad, 0f, 7f, 20, 105UL),
                Approach(JunctionApproachRule.PriorityRoad, 90f, 7.2f, 21, 106UL),
                Approach(JunctionApproachRule.Stop, 0f, 4f, 30, 107UL),
                Approach(JunctionApproachRule.Stop, 0f, 4f, 31, 108UL),
                Approach(JunctionApproachRule.TrafficLight, 90f, 9f, 40, 109UL),
                Approach(JunctionApproachRule.TrafficLight, 90f, 9.2f, 41, 110UL, signalAllows: false),
                Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 111UL),
                Approach(JunctionApproachRule.PriorityToRight, 0f, 3f, 50, 112UL, exitRoom: false),
                Approach(JunctionApproachRule.PriorityRoad, 90f, 3f, 51, 113UL, exitRoom: false),
                Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 60, 114UL, breach: true),
                Approach(JunctionApproachRule.PriorityToRight, 90f, 6f, 61, 115UL),
                Approach(JunctionApproachRule.PriorityToRight, 180f, 6f, 62, 116UL),
                Approach(JunctionApproachRule.Stop, 90f, 6f, 70, 117UL, stopSatisfied: false),
                Approach(JunctionApproachRule.PriorityToRight, 135f, 6f, 80, 118UL),
                Approach(JunctionApproachRule.PriorityToRight, 135.5f, 6f, 81, 119UL),
                Approach(JunctionApproachRule.PriorityRoad, 225f, 12f, 90, 120UL, junctionKey: 2)
            };
        }

        [Test]
        public void EveryContestedPairIsDecidedWithoutBothWaiting()
        {
            var claims = Matrix();
            var contested = 0;
            for (var i = 0; i < claims.Length; i++)
            {
                for (var j = 0; j < claims.Length; j++)
                {
                    if (i == j || !JunctionRules.Conflicts(claims[i], claims[j]))
                    {
                        continue;
                    }

                    contested++;
                    var mine = JunctionRules.Resolve(claims[i], claims[j]);
                    var theirs = JunctionRules.Resolve(claims[j], claims[i]);
                    Assert.That(mine, Is.Not.EqualTo(theirs),
                        "Invariance par echange violee entre " + Describe(claims[i]) + " et " + Describe(claims[j])
                        + " : les deux verdicts valent " + mine + ".");
                    Assert.That(mine == JunctionVerdict.Proceed || theirs == JunctionVerdict.Proceed, Is.True,
                        "Exactement un des deux doit passer : les deux attendent entre " + Describe(claims[i])
                        + " et " + Describe(claims[j]) + ".");
                }
            }

            Assert.That(contested, Is.GreaterThan(40), "La matrice doit effectivement contenir des paires concurrentes.");
        }

        [Test]
        public void TheElectedWinnerIsTheSameWhicheverPeerBuildsTheSet()
        {
            // L'ordre du tampon de revendications vient d'une requete physique : il differe d'un pair a
            // l'autre. L'election ne doit donc pas en dependre, sinon "exactement un procede" ne tient
            // que tant que deux pairs lisent les colliders dans le meme ordre -- ce que rien ne garantit.
            var set = new[]
            {
                Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 101UL),
                Approach(JunctionApproachRule.PriorityToRight, 120f, 6.2f, 11, 102UL),
                Approach(JunctionApproachRule.PriorityToRight, 240f, 6.1f, 12, 103UL)
            };

            var reference = ElectedId(set);
            Assert.That(reference, Is.Not.EqualTo(0UL), "Un gagnant doit toujours etre designe.");

            for (var rotation = 1; rotation < set.Length; rotation++)
            {
                var rotated = new List<JunctionClaim>();
                for (var i = 0; i < set.Length; i++)
                {
                    rotated.Add(set[(i + rotation) % set.Length]);
                }

                Assert.That(ElectedId(rotated), Is.EqualTo(reference),
                    "L'election a change de gagnant pour la meme situation lue dans un autre ordre.");
            }
        }

        [Test]
        public void AThreeWayRightOfWayCycleStillElectsExactlyOneWinner()
        {
            // Trois approches a 120 degres forment un CYCLE de priorite a droite : chacune cede a sa
            // droite, donc chacune perd un duel et en gagne un. La somme des duels ne designe personne ;
            // c'est l'ordre total qui tranche. Sans lui, les trois attendraient -- l'etat que la story
            // interdit nommement.
            var cycle = new[]
            {
                Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 101UL),
                Approach(JunctionApproachRule.PriorityToRight, 120f, 6.2f, 11, 102UL),
                Approach(JunctionApproachRule.PriorityToRight, 240f, 6.1f, 12, 103UL)
            };

            var wins = new int[cycle.Length];
            var winner = JunctionRules.ResolveWinnerIndex(cycle, cycle.Length, wins);
            Assert.That(winner, Is.InRange(0, cycle.Length - 1), "Un cycle ne doit pas rendre l'election muette.");
            Assert.That(wins.All(count => count == 1), Is.True,
                "Les trois duels doivent etre gagnes par trois approches differentes : c'est ce qui fait le cycle.");

            // Le gagnant sort de l'ordre total, et il en sort identiquement quel que soit le vehicule
            // qui construit l'ensemble : c'est cette stabilite qui garantit qu'un seul se croit gagnant.
            var reference = cycle[winner].NetworkObjectId;
            for (var viewpoint = 0; viewpoint < cycle.Length; viewpoint++)
            {
                var observed = new JunctionClaim[cycle.Length];
                for (var i = 0; i < cycle.Length; i++)
                {
                    observed[i] = cycle[(i + viewpoint) % cycle.Length];
                }

                var elected = JunctionRules.ResolveWinnerIndex(observed, observed.Length, new int[observed.Length]);
                Assert.That(observed[elected].NetworkObjectId, Is.EqualTo(reference),
                    "Le pair " + viewpoint + " designe un autre gagnant : les deux entreraient.");
            }
        }

        [Test]
        public void TheTotalOrderNeverTiesOnTwoDistinctClaims()
        {
            // "Jamais d'egalite" est la propriete qui rend l'election deterministe. Elle porte sur le
            // comparateur lui-meme : l'identifiant n'est atteint qu'en dernier recours, et il rend
            // toujours un verdict strict.
            var reference = Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 7UL);
            Assert.That(JunctionRules.CompareTotalOrder(reference, reference), Is.EqualTo(0),
                "Seule une revendication identique a elle-meme est egale.");

            // Les trois composantes de l'ordre sont (distance, voie, identifiant) : deux revendications
            // qui les partagent toutes les trois ne sont pas deux revendications, c'est le meme vehicule
            // au meme endroit -- et il ne se dispute pas lui-meme (Conflicts refuse deux fois la meme
            // voie). Le quatrieme cas porte donc un identifiant distinct : ce qui est verifie ici est
            // qu'aucun autre champ (cap, palier de deblocage) ne participe a l'ordre, pas l'inverse.
            foreach (var other in new[]
                     {
                         Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 9UL),
                         Approach(JunctionApproachRule.PriorityToRight, 0f, 9f, 10, 7UL),
                         Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 11, 7UL),
                         Approach(JunctionApproachRule.PriorityToRight, 90f, 6f, 10, 9UL, breach: true)
                     })
            {
                var forward = JunctionRules.CompareTotalOrder(reference, other);
                Assert.That(forward, Is.Not.EqualTo(0), "Deux revendications distinctes ne doivent jamais etre egales.");
                Assert.That(JunctionRules.CompareTotalOrder(other, reference), Is.EqualTo(-forward),
                    "Le comparateur doit etre antisymetrique, sinon l'election depend de l'ordre de lecture.");
            }

            Assert.That(JunctionRules.CompareTotalOrder(reference, Approach(JunctionApproachRule.PriorityToRight, 0f, 20f, 10, 7UL)),
                Is.GreaterThan(0), "A defaut d'autre critere, l'approche la plus proche gagne.");
            Assert.That(JunctionRules.CompareTotalOrder(reference, Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 11, 7UL)),
                Is.GreaterThan(0), "puis la voie d'index le plus petit.");
            Assert.That(JunctionRules.CompareTotalOrder(reference, Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 9UL)),
                Is.GreaterThan(0), "puis l'identifiant le plus petit -- le dernier recours, atteint seulement si tout le reste est egal.");
        }

        [Test]
        public void ApproachesThatDoNotShareAConflictAreNeverHeld()
        {
            var lane = Approach(JunctionApproachRule.Stop, 0f, 4f, 10, 101UL, stopSatisfied: false);
            var follower = Approach(JunctionApproachRule.Stop, 0f, 9f, 10, 102UL, stopSatisfied: false);
            Assert.That(JunctionRules.Conflicts(lane, follower), Is.False,
                "Deux vehicules de la MEME voie se poursuivent : c'est la perception de la Story 5.17, pas l'arbitrage.");

            var southbound = Approach(JunctionApproachRule.PriorityToRight, 180f, 5f, 11, 103UL);
            Assert.That(JunctionRules.Conflicts(lane, southbound), Is.False,
                "Deux approches opposees ne se croisent pas : chacune continue tout droit de son cote.");

            var elsewhere = Approach(JunctionApproachRule.PriorityToRight, 90f, 5f, 12, 104UL, junctionKey: 4);
            Assert.That(JunctionRules.Conflicts(lane, elsewhere), Is.False, "Deux jonctions distinctes ne s'arbitrent pas.");
        }

        [Test]
        public void PlannedRoutesDistinguishOpposingThroughTrafficFromACrossingTurn()
        {
            var northThrough = Approach(JunctionApproachRule.PriorityToRight, 180f, 4f, 10, 101UL,
                entryPoint: new Vector3(1f, 0f, 10f), exitPoint: new Vector3(1f, 0f, -10f));
            var southThrough = Approach(JunctionApproachRule.PriorityToRight, 0f, 4f, 11, 102UL,
                entryPoint: new Vector3(-1f, 0f, -10f), exitPoint: new Vector3(-1f, 0f, 10f));
            Assert.That(JunctionRules.Conflicts(northThrough, southThrough), Is.False,
                "Deux tout-droits opposes sur leurs voies propres ne se retiennent pas.");

            var northTurningAcross = Approach(JunctionApproachRule.PriorityToRight, 180f, 4f, 10, 101UL,
                entryPoint: new Vector3(1f, 0f, 10f), exitPoint: new Vector3(-10f, 0f, -1f));
            Assert.That(JunctionRules.Conflicts(northTurningAcross, southThrough), Is.True);
            Assert.That(JunctionRules.Resolve(northTurningAcross, southThrough),
                Is.Not.EqualTo(JunctionRules.Resolve(southThrough, northTurningAcross)));
        }

        [Test]
        public void PlannedRoutesThatMergeOnTheSameExitStillConflict()
        {
            var first = Approach(JunctionApproachRule.PriorityToRight, 0f, 4f, 10, 101UL,
                entryPoint: new Vector3(0f, 0f, -10f), exitPoint: new Vector3(0f, 0f, 10f));
            var second = Approach(JunctionApproachRule.PriorityToRight, 0f, 4f, 11, 102UL,
                entryPoint: new Vector3(0f, 0f, -5f), exitPoint: new Vector3(0f, 0f, 12f));

            Assert.That(JunctionRules.Conflicts(first, second), Is.True,
                "Deux voies qui partagent un troncon de sortie doivent etre arbitrees.");
        }

        [Test]
        public void JunctionTurnUsesTheAuthoredEntryAndExitTangents()
        {
            var entry = new Vector3(1f, 0f, 10f);
            var exit = new Vector3(10f, 0f, -1f);
            Assert.That(LaneGraphRouting.ResolveJunctionTurnPoint(entry, Vector3.back, exit, Vector3.right, 0f), Is.EqualTo(entry));
            Assert.That(LaneGraphRouting.ResolveJunctionTurnPoint(entry, Vector3.back, exit, Vector3.right, 1f), Is.EqualTo(exit));
            var early = LaneGraphRouting.ResolveJunctionTurnPoint(entry, Vector3.back, exit, Vector3.right, .1f) - entry;
            Assert.That(Vector3.Dot(early.normalized, Vector3.back), Is.GreaterThan(.9f),
                "Le debut de virage reste tangent a la voie entrante au lieu de couper directement vers la sortie.");
        }

        [Test]
        public void RightOfWayDecidesWithoutAnyAuthoredPriorityRoad()
        {
            var headingNorth = Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 101UL);
            var fromTheRight = Approach(JunctionApproachRule.PriorityToRight, 270f, 6f, 11, 102UL);

            Assert.That(JunctionRules.ComesFromTheRight(headingNorth, fromTheRight), Is.True);
            Assert.That(JunctionRules.Resolve(headingNorth, fromTheRight), Is.EqualTo(JunctionVerdict.Wait));
            Assert.That(JunctionRules.Resolve(fromTheRight, headingNorth), Is.EqualTo(JunctionVerdict.Proceed));
        }

        [Test]
        public void AnUndecidableHeadingFallsBackToTheTotalOrderInsteadOfBlockingBoth()
        {
            // Caps EXACTEMENT paralleles : le test "vient de ma droite" ne tranche alors ni dans un sens
            // ni dans l'autre (les deux produits scalaires sont nuls), et c'est le SEUL cas ou le sens
            // est reellement indeterminable -- une difference d'un demi-degre suffit a designer un cote
            // et sortirait de la ligne de matrice visee. On ne bloque jamais les deux : l'ordre total
            // prend le relais.
            var first = Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 10, 101UL);
            var second = Approach(JunctionApproachRule.PriorityToRight, 0f, 6f, 11, 102UL);

            Assert.That(JunctionRules.Conflicts(first, second), Is.True);
            Assert.That(JunctionRules.ComesFromTheRight(first, second), Is.False);
            Assert.That(JunctionRules.ComesFromTheRight(second, first), Is.False);
            Assert.That(JunctionRules.Resolve(first, second), Is.EqualTo(JunctionVerdict.Proceed));
            Assert.That(JunctionRules.Resolve(second, first), Is.EqualTo(JunctionVerdict.Wait));
        }

        [Test]
        public void ExitRoomIsCheckedBeforeEveryPriorityRule()
        {
            var blockedPriorityRoad = Approach(JunctionApproachRule.PriorityRoad, 0f, 3f, 10, 101UL, exitRoom: false);
            var freeSecondary = Approach(JunctionApproachRule.PriorityToRight, 90f, 9f, 11, 102UL);

            Assert.That(blockedPriorityRoad.Eligibility, Is.EqualTo(0));
            Assert.That(freeSecondary.Eligibility, Is.EqualTo(3));
            Assert.That(JunctionRules.Resolve(blockedPriorityRoad, freeSecondary), Is.EqualTo(JunctionVerdict.Wait),
                "Une route prioritaire sans place a la sortie cede : le controle de place precede la priorite.");
            Assert.That(JunctionRules.Resolve(freeSecondary, blockedPriorityRoad), Is.EqualTo(JunctionVerdict.Proceed));
        }

        [Test]
        public void AnUnmeasuredExitIsNeverReadAsFree()
        {
            Assert.That(JunctionRules.HasExitRoom(true, false), Is.True);
            Assert.That(JunctionRules.HasExitRoom(true, true), Is.False, "Un usager de la route occupe la zone de degagement.");
            Assert.That(JunctionRules.HasExitRoom(false, false), Is.False,
                "Place indeterminee : traitee comme saturee, jamais comme libre.");
            Assert.That(JunctionRules.HasExitRoom(false, true), Is.False);
        }

        [Test]
        public void AStopHoldsAndThenRequiresAnAcceptedGap()
        {
            var stopping = Approach(JunctionApproachRule.Stop, 0f, 4f, 10, 101UL, stopSatisfied: false);
            var crossing = Approach(JunctionApproachRule.PriorityToRight, 90f, 4f, 11, 102UL);
            Assert.That(stopping.Eligibility, Is.EqualTo(2));
            Assert.That(JunctionRules.Resolve(stopping, crossing), Is.EqualTo(JunctionVerdict.Wait),
                "Un stop non satisfait attend, meme face a une approche sans priorite authoree.");

            Assert.That(JunctionRules.StopGapAccepted(0f, 4f, hasClaimants: false), Is.True,
                "Sans revendiquant concurrent, il n'y a pas d'ecart a respecter.");
            Assert.That(JunctionRules.StopGapAccepted(float.NaN, 4f, hasClaimants: true), Is.False,
                "Un ecart non mesurable n'est jamais accepte.");
            Assert.That(JunctionRules.StopGapAccepted(3.9f, 4f, hasClaimants: true), Is.False);
            Assert.That(JunctionRules.StopGapAccepted(4f, 4f, hasClaimants: true), Is.True);

            var held = stopping.WithStopSatisfied(JunctionRules.StopGapAccepted(6f, 4f, hasClaimants: true));
            Assert.That(held.Eligibility, Is.EqualTo(3), "Arret tenu ET ecart accepte : le stop ne retient plus.");
        }

        [Test]
        public void TheSignalOnlyHoldsTheApproachItsPhaseDoesNotAuthorise()
        {
            var denied = Approach(JunctionApproachRule.TrafficLight, 0f, 8f, 10, 101UL, signalAllows: false);
            var allowed = Approach(JunctionApproachRule.TrafficLight, 90f, 8f, 11, 102UL);

            Assert.That(denied.Eligibility, Is.EqualTo(1));
            Assert.That(allowed.Eligibility, Is.EqualTo(3), "L'approche autorisee ne paie aucun arret : elle franchit.");
            Assert.That(JunctionRules.Resolve(denied, allowed), Is.EqualTo(JunctionVerdict.Wait));
            Assert.That(JunctionRules.Resolve(allowed, denied), Is.EqualTo(JunctionVerdict.Proceed));
        }

        [Test]
        public void TheBreachTierLiftsAnApproachThatTheRulesWouldHoldForever()
        {
            var held = Approach(JunctionApproachRule.Stop, 0f, 4f, 10, 101UL, exitRoom: false, stopSatisfied: false);
            var blocking = Approach(JunctionApproachRule.PriorityRoad, 90f, 4f, 11, 102UL, exitRoom: false);

            Assert.That(JunctionRules.Resolve(held, blocking), Is.EqualTo(JunctionVerdict.Wait));
            Assert.That(held.WithBreach(true).Eligibility, Is.EqualTo(0),
                "Le palier de deblocage ne reecrit pas l'eligibilite : il prime par le RANG.");
            Assert.That(held.WithBreach(true).Rank, Is.EqualTo(4),
                "Le rang de deblocage doit primer tous les rangs authores.");
            Assert.That(JunctionRules.Resolve(held.WithBreach(true), blocking), Is.EqualTo(JunctionVerdict.Proceed),
                "Un vehicule en interblocage finit par entrer, sans qu'aucun palier de retrait n'existe.");
        }

        [Test]
        public void ADeadlockNeedsAnotherWaitingClaimantAndTheAuthoredDelay()
        {
            Assert.That(JunctionRules.IsJunctionDeadlock(60f, anotherClaimantWaiting: false, 12f), Is.False,
                "Un vehicule retenu seul n'est pas un interblocage : c'est un vehicule qui attend son tour.");
            Assert.That(JunctionRules.IsJunctionDeadlock(11.9f, true, 12f), Is.False);
            Assert.That(JunctionRules.IsJunctionDeadlock(12f, true, 12f), Is.True);
            Assert.That(JunctionRules.IsJunctionDeadlock(float.NaN, true, 12f), Is.False);

            var source = CodeOnly(System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs"));
            var tick = ExtractMethodBody(source, "private bool TickJunctionRules(DriverProfile profile, float dt, float speed, float longitudinalSpeed)");
            // Story 5.19 : l'arbitrage ne designe plus UN vainqueur mais un ENSEMBLE ADMIS, parce
            // qu'un tournoi note sur un ensemble filtre par observateur rendait deux verdicts
            // differents pour la meme jonction. L'intention du test est inchangee : le palier de
            // deblocage se decide APRES l'arbitrage, et un ensemble connu ne le laisse franchir
            // qu'a un revendiquant deja admis.
            Assert.That(tick.IndexOf("var admitted = JunctionRules.IsAdmitted", StringComparison.Ordinal),
                Is.LessThan(tick.IndexOf("JunctionRules.IsJunctionDeadlock", StringComparison.Ordinal)));
            Assert.That(tick, Does.Contain("(!known || admitted)"),
                "Un ensemble connu ne laisse franchir le palier qu'a un revendiquant admis.");
        }

        // ------------------------------------------------------------ modele pur : feux

        [Test]
        public void TheActivePhaseIsAFunctionOfTheSharedClockAlone()
        {
            var plan = new TrafficSignalPlan(IntersectionJunctionId, 6f, new[]
            {
                new TrafficSignalPhase(12f, new[] { 0 }),
                new TrafficSignalPhase(12f, new[] { 1 })
            });

            Assert.That(plan.CycleSeconds, Is.EqualTo(24f));
            Assert.That(JunctionRules.ResolvePhaseIndex(plan, 0f), Is.EqualTo(0));
            Assert.That(JunctionRules.ResolvePhaseIndex(plan, 11.99f), Is.EqualTo(0));
            Assert.That(JunctionRules.ResolvePhaseIndex(plan, 12f), Is.EqualTo(1));
            Assert.That(JunctionRules.ResolvePhaseIndex(plan, 23.99f), Is.EqualTo(1));
            Assert.That(JunctionRules.ResolvePhaseIndex(plan, 24f), Is.EqualTo(0), "Le cycle doit se refermer.");
            Assert.That(JunctionRules.ResolvePhaseIndex(plan, 25.5f), Is.EqualTo(0));

            // Deux pairs, la meme donnee et la meme horloge : la meme phase, sans rien se dire.
            foreach (var clock in new[] { 0.5f, 7f, 13f, 20f, 100.25f })
            {
                Assert.That(JunctionRules.ResolvePhaseIndex(plan, clock), Is.EqualTo(JunctionRules.ResolvePhaseIndex(plan, clock)));
            }

            Assert.That(JunctionRules.PhaseAllowsGroup(plan, 0f, 0), Is.True);
            Assert.That(JunctionRules.PhaseAllowsGroup(plan, 0f, 1), Is.False);
            Assert.That(JunctionRules.PhaseAllowsGroup(plan, 12f, 1), Is.True);
            Assert.That(JunctionRules.PhaseAllowsGroup(plan, 12f, 0), Is.False);
            Assert.That(JunctionRules.PhaseAllowsGroup(plan, 0f, -1), Is.False, "Un groupe negatif n'existe pas.");
        }

        [Test]
        public void AnUnusablePlanAuthorsNoRuleAtAll()
        {
            var empty = new TrafficSignalPlan(IntersectionJunctionId, 6f, Array.Empty<TrafficSignalPhase>());
            Assert.That(JunctionRules.ResolvePhaseIndex(empty, 3f), Is.EqualTo(-1));
            Assert.That(JunctionRules.PhaseAllowsGroup(empty, 3f, 0), Is.False);

            var zeroDurations = new TrafficSignalPlan(IntersectionJunctionId, 6f, new[]
            {
                new TrafficSignalPhase(0f, new[] { 0 }),
                new TrafficSignalPhase(0f, new[] { 1 })
            });
            Assert.That(zeroDurations.CycleSeconds, Is.EqualTo(0f));
            Assert.That(JunctionRules.ResolvePhaseIndex(zeroDurations, 3f), Is.EqualTo(-1));

            var overflowing = new TrafficSignalPlan(IntersectionJunctionId, 6f, new[]
            {
                new TrafficSignalPhase(float.MaxValue, new[] { 0 }),
                new TrafficSignalPhase(float.MaxValue, new[] { 1 })
            });
            Assert.That(overflowing.CycleSeconds, Is.EqualTo(0f),
                "Un cycle non fini est inexploitable, jamais une phase silencieusement introuvable.");

            var partial = new TrafficSignalPlan(IntersectionJunctionId, 6f, new[]
            {
                new TrafficSignalPhase(float.NaN, new[] { 0 }),
                new TrafficSignalPhase(8f, new[] { 1 })
            });
            Assert.That(JunctionRules.ResolvePhaseIndex(partial, 2f), Is.EqualTo(1),
                "Une phase de duree non finie est ignoree, la suivante gouverne.");

            var noGroups = new TrafficSignalPlan(IntersectionJunctionId, 6f, new[]
            {
                new TrafficSignalPhase(11f, Array.Empty<int>()),
                new TrafficSignalPhase(11f, new[] { 0 })
            });
            Assert.That(JunctionRules.PhaseAllowsGroup(noGroups, 0f, 0), Is.False,
                "Une phase sans groupe autorise n'autorise personne : il n'existe pas de troisieme etat.");
        }

        [Test]
        public void AnOutOfDomainRuleFallsBackToTheRightOfWayInsteadOfThrowing()
        {
            Assert.That(JunctionRules.NormalizeRule((JunctionApproachRule)99), Is.EqualTo(JunctionApproachRule.PriorityToRight));
            Assert.That(JunctionRules.NormalizeRule((JunctionApproachRule)(-1)), Is.EqualTo(JunctionApproachRule.PriorityToRight));
            Assert.That(JunctionRules.NormalizeRule(JunctionApproachRule.Stop), Is.EqualTo(JunctionApproachRule.Stop));
            Assert.That(Approach((JunctionApproachRule)99, 0f, 5f, 10, 101UL).Rule,
                Is.EqualTo(JunctionApproachRule.PriorityToRight),
                "Une valeur serialisee hors domaine est ramenee dans son domaine a la construction de la revendication.");
        }

        // ------------------------------------------------------------ authoring : donnee

        [Test]
        public void TheAuthoredTrafficSettingsCarryOneUsablePlanForTheDistrictCrossroads()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TrafficSettingsDef>(TrafficDefPath);
            Assert.That(settings, Is.Not.Null, TrafficDefPath + " attendu");
            Assert.That(settings.TryValidate(out var error), Is.True, error);

            Assert.That(settings.TryGetSignalPlan(IntersectionJunctionId, out var plan), Is.True,
                "Le carrefour du district doit porter un plan de feux authore.");
            Assert.That(plan.Phases.Count, Is.GreaterThanOrEqualTo(2), "Un plan a une phase n'arbitre rien.");
            Assert.That(plan.MinimumGreenSeconds, Is.GreaterThan(0f));

            var groups = new HashSet<int>();
            var total = 0f;
            foreach (var phase in plan.Phases)
            {
                Assert.That(phase.DurationSeconds, Is.GreaterThanOrEqualTo(plan.MinimumGreenSeconds),
                    "Chaque phase doit atteindre la garde de vert minimal, sinon le feu clignote.");
                total += phase.DurationSeconds;
                foreach (var group in phase.GreenGroups)
                {
                    groups.Add(group);
                }
            }

            Assert.That(groups.Count, Is.EqualTo(2),
                "Un carrefour a feux separe ses deux avenues en deux groupes : un seul groupe laisserait tout le monde passer ensemble.");
            Assert.That(plan.CycleSeconds, Is.EqualTo(total));

            Assert.That(settings.TryGetSignalPlan("jonction_inconnue", out _), Is.False,
                "Un id de jonction inconnu ne doit trouver aucun plan : aucune regle de feu n'est inventee.");
            Assert.That(settings.SignalPlans.Where(candidate => candidate.JunctionId == TJunctionJunctionId).ToArray(), Is.Empty,
                "La jonction en T se regle par priorite et stop : elle ne porte pas de plan de feux.");
        }

        [Test]
        public void TheAuthoredDriverProfileCarriesTheJunctionThresholdsAcrossEveryDisposition()
        {
            var def = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverDefPath);
            Assert.That(def, Is.Not.Null, DriverDefPath + " attendu");
            Assert.That(def.TryValidate(out var error), Is.True, error);

            var authored = def.Profile;
            Assert.That(authored.JunctionApproachRadius, Is.GreaterThan(0f));
            Assert.That(authored.JunctionExitClearanceRadius, Is.GreaterThan(0f));
            Assert.That(authored.JunctionEscalationDelay, Is.GreaterThan(0f));

            foreach (RageDisposition disposition in Enum.GetValues(typeof(RageDisposition)))
            {
                var effective = DriverModel.ResolveEffectiveProfile(authored, disposition);
                Assert.That(effective.JunctionApproachRadius, Is.EqualTo(authored.JunctionApproachRadius),
                    "La modulation de disposition ne doit pas remplacer un seuil authore par un defaut de constructeur.");
                Assert.That(effective.JunctionStopHoldSeconds, Is.EqualTo(authored.JunctionStopHoldSeconds));
                Assert.That(effective.JunctionAcceptedGap, Is.EqualTo(authored.JunctionAcceptedGap));
                Assert.That(effective.JunctionEscalationDelay, Is.EqualTo(authored.JunctionEscalationDelay));
                Assert.That(effective.JunctionExitClearanceRadius, Is.EqualTo(authored.JunctionExitClearanceRadius));
            }
        }

        [Test]
        public void NoJunctionRuleIsWritableAtRuntime()
        {
            // "Aucune regle n'est recalculee par frame" se traduit structurellement : la regle authoree
            // n'a pas de setter, donc rien a l'execution ne peut la reecrire.
            foreach (var name in new[] { "JunctionRule", "JunctionId", "SignalGroup" })
            {
                var property = typeof(LaneNode).GetProperty(name);
                Assert.That(property, Is.Not.Null, "LaneNode." + name + " attendu");
                Assert.That(property.CanWrite, Is.False, "LaneNode." + name + " ne doit pas etre modifiable a l'execution, meme en prive.");
            }

            var graphSource = CodeOnly(System.IO.File.ReadAllText("Assets/RoadRage/Features/Vehicles/LaneGraph.cs"));
            Assert.That(Occurrences(graphSource, "junctionApproaches.Add("), Is.EqualTo(1),
                "L'index des approches doit etre bati une seule fois, a la construction du graphe.");
            Assert.That(Occurrences(graphSource, "BuildJunctionIndex();"), Is.EqualTo(1),
                "et depuis un seul point d'appel.");
        }

        // ------------------------------------------------------------ authoring : prefabs

        [Test]
        public void TheCrossroadsAuthoredApproachesMatchTheirSignalPlan()
        {
            WithPrefab(IntersectionPrefabPath, instance =>
            {
                var nodes = instance.GetComponentsInChildren<LaneNode>(true);
                var approaches = nodes.Where(node => node.IsJunctionApproach).ToArray();
                Assert.That(approaches.Length, Is.EqualTo(4),
                    "Le carrefour en croix a quatre approches de decision, une par branche.");

                foreach (var approach in approaches)
                {
                    Assert.That(approach.JunctionId, Is.EqualTo(IntersectionJunctionId));
                    Assert.That(approach.JunctionRule, Is.EqualTo(JunctionApproachRule.TrafficLight),
                        approach.name + " doit se regler au feu, comme le plan du Def de trafic.");
                }

                // Les deux approches d'une MEME avenue sont opposees : elles peuvent partager un groupe,
                // c'est ce qui fait qu'un carrefour a feux laisse passer une avenue a la fois.
                var groups = approaches.Select(node => node.SignalGroup).Distinct().OrderBy(group => group).ToArray();
                Assert.That(groups.Length, Is.EqualTo(2),
                    "Deux groupes attendus, un par avenue : deux approches qui se CROISENT ne doivent jamais partager le meme.");

                foreach (var first in approaches)
                {
                    foreach (var second in approaches)
                    {
                        if (first == second || first.SignalGroup != second.SignalGroup)
                        {
                            continue;
                        }

                        var crossing = Vector3.Angle(first.transform.forward, second.transform.forward);
                        Assert.That(crossing, Is.GreaterThan(JunctionRules.OppositeApproachDegrees),
                            first.name + " et " + second.name + " partagent le groupe " + first.SignalGroup
                            + " mais se croisent a " + crossing.ToString("0") + " degres : leurs deux phases seraient vertes ensemble.");
                    }
                }

                var decoys = nodes.Count(node => !node.IsJunctionApproach);
                Assert.That(decoys, Is.GreaterThan(0), "Les connecteurs du module ne portent aucune approche.");
            });
        }

        [Test]
        public void TheTJunctionAuthoredApproachesSplitIntoAPriorityRoadAndAStop()
        {
            WithPrefab(TJunctionPrefabPath, instance =>
            {
                var approaches = instance.GetComponentsInChildren<LaneNode>(true)
                    .Where(node => node.IsJunctionApproach).ToArray();
                Assert.That(approaches.Length, Is.EqualTo(3), "La traversante a deux sens, la branche laterale un seul.");

                foreach (var approach in approaches)
                {
                    Assert.That(approach.JunctionId, Is.EqualTo(TJunctionJunctionId));
                }

                Assert.That(approaches.Count(node => node.JunctionRule == JunctionApproachRule.PriorityRoad), Is.EqualTo(2),
                    "La voie traversante porte la priorite dans ses deux sens.");
                Assert.That(approaches.Count(node => node.JunctionRule == JunctionApproachRule.Stop), Is.EqualTo(1),
                    "La branche laterale porte le stop.");
                Assert.That(approaches.Count(node => node.JunctionRule == JunctionApproachRule.TrafficLight), Is.EqualTo(0),
                    "Aucune approche de la jonction en T n'est au feu : elle n'a pas de plan.");
            });
        }

        [Test]
        public void TheDistrictJunctionsAreIndexedApartAndContradictNothing()
        {
            WithMvpRun(scene =>
            {
                var graph = ResolveGraph(scene);
                graph.Rebuild();

                var approaches = graph.JunctionApproaches;
                Assert.That(approaches.Count, Is.EqualTo(16),
                    "1 carrefour a 4 approches + 4 jonctions en T a 3 approches.");
                Assert.That(approaches.Select(approach => approach.JunctionKey).Distinct().Count(), Is.EqualTo(5),
                    "L'identite d'une jonction est le couple (module porteur, id authore) : quatre instances du meme prefab restent quatre jonctions.");

                Assert.That(graph.ContradictoryJunctions, Is.Empty,
                    "Une autoroute qui croise une autre autoroute, ou deux approches croisees dans le meme groupe de feux, se signalent.");

                var crossroads = 0;
                foreach (var approach in approaches)
                {
                    Assert.That(approach.Forward.magnitude, Is.EqualTo(1f).Within(0.001f), "Le cap d'une approche est normalise et planaire.");
                    Assert.That(approach.ExitProbeNode, Is.GreaterThanOrEqualTo(0),
                        "Chaque approche de decision a un successeur : sans lui, la place de sortie serait indeterminee, donc toujours saturee.");
                    Assert.That(graph.GetSuccessors(approach.NodeIndex), Does.Contain(approach.ExitProbeNode),
                        "La sonde de sortie est un SUCCESSEUR de l'approche, pas un noeud quelconque du graphe.");
                    Assert.That(graph.TryGetJunctionExitProbe(approach.NodeIndex, out var exitPoint), Is.True);
                    Assert.That(Vector3.Distance(exitPoint, graph.GetNodePosition(approach.ExitProbeNode)), Is.LessThan(0.001f));

                    if (approach.JunctionId != IntersectionJunctionId)
                    {
                        continue;
                    }

                    crossroads++;
                    Assert.That(Vector3.Angle(approach.Forward, Flat(exitPoint - graph.GetNodePosition(approach.NodeIndex))),
                        Is.LessThan(15f),
                        "Au carrefour, la voie de sortie sondee est celle qui continue tout droit : c'est la traversee dont la place se controle.");
                }

                Assert.That(crossroads, Is.EqualTo(4), "Le carrefour porte quatre approches signalees.");
            });
        }

        [Test]
        public void TheSignalDecorationIsAVisualChildWithoutAnyColliderOrGameplayState()
        {
            WithPrefab(IntersectionPrefabPath, instance =>
            {
                var views = instance.GetComponentsInChildren<TrafficSignalLampView>(true);
                Assert.That(views.Length, Is.EqualTo(4), "Un feu par branche du carrefour.");

                foreach (var view in views)
                {
                    AssertDecorationIsVisualOnly(view.gameObject, IntersectionJunctionId);
                    var serialized = new SerializedObject(view);
                    Assert.That(serialized.FindProperty("greenLamp").objectReferenceValue, Is.Not.Null,
                        view.name + " : la lampe verte doit etre cablee.");
                    Assert.That(serialized.FindProperty("redLamp").objectReferenceValue, Is.Not.Null,
                        view.name + " : la lampe rouge doit etre cablee.");
                }
            });

            WithPrefab(TJunctionPrefabPath, instance =>
            {
                Assert.That(instance.GetComponentsInChildren<TrafficSignalLampView>(true).Length, Is.EqualTo(0),
                    "La jonction en T n'a pas de feu : elle ne doit porter aucune tete de feu.");

                var sign = FindDescendant(instance.transform, "Visual_Sign_Stop");
                Assert.That(sign, Is.Not.Null, "La branche laterale porte un panneau stop visible.");
                AssertDecorationIsVisualOnly(sign.gameObject, TJunctionJunctionId);
            });

            // La presentation ne porte aucun etat : c'est ce qui permet aux deux pairs de tirer la meme
            // phase de la meme donnee, sans NetworkVariable ni RPC -- et donc sans second objet d'etat.
            var viewSource = CodeOnly(System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/TrafficSignalLampView.cs"));
            foreach (var forbidden in new[] { "NetworkVariable", "NetworkBehaviour", "Rpc(", "Collider", "LaneNode" })
            {
                Assert.That(viewSource, Does.Not.Contain(forbidden),
                    "La vue de feux est de la PRESENTATION : elle ne porte ni etat de gameplay ni etat reseau (" + forbidden + ").");
            }
        }

        [Test]
        public void TheSignalDecorationReferencesMeshesInsteadOfNestingTheThirdPartyPack()
        {
            foreach (var path in new[] { IntersectionPrefabPath, TJunctionPrefabPath })
            {
                var text = System.IO.File.ReadAllText(path);
                Assert.That(Occurrences(text, "--- !u!1001"), Is.EqualTo(0),
                    path + " : la signalisation est posee en mesh copie, jamais en instance de prefab tiers. "
                    + "Une instance imbriquee ferait dependre le prefab du projet d'un pack absent d'un clone frais.");

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                try
                {
                    var filters = instance.GetComponentsInChildren<MeshFilter>(true)
                        .Where(filter => filter.transform.name.StartsWith("Synty_", StringComparison.Ordinal))
                        .ToArray();
                    Assert.That(filters.Length, Is.GreaterThan(0), path + " : les meshes de signalisation doivent etre poses.");
                    foreach (var filter in filters)
                    {
                        Assert.That(filter.sharedMesh, Is.Not.Null,
                            path + " : " + filter.name + " reference un mesh nul, donc rien ne s'affiche.");
                        Assert.That(filter.GetComponent<MeshRenderer>().sharedMaterials.Length, Is.GreaterThan(0));
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }

        [Test]
        public void TheJunctionPathNeverRemovesOrTeleportsTheVehicle()
        {
            var source = System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs");
            var junctionBlock = CodeOnly(ExtractRegion(source, "Story 5.18 : jonctions"));

            foreach (var forbidden in new[] { "RecoverAtWaypoint(", "Despawn(", "Destroy(", "SetPositionAndRotation(", "MovePosition(" })
            {
                Assert.That(junctionBlock, Does.Not.Contain(forbidden),
                    "Aucun palier de retrait ni de teleportation n'existe dans les regles d'intersection : " + forbidden);
            }

            Assert.That(Occurrences(junctionBlock, "transform.position ="), Is.EqualTo(0),
                "Le conducteur ne repose jamais la caisse a la main dans le chemin d'intersection.");

            // Les cinq seuils sont lus dans le profil authore, jamais ecrits dans le controleur.
            foreach (var threshold in new[] { "JunctionApproachRadius", "JunctionStopHoldSeconds", "JunctionAcceptedGap",
                "JunctionEscalationDelay", "JunctionExitClearanceRadius" })
            {
                Assert.That(junctionBlock, Does.Contain("profile." + threshold),
                    "Le controle de conduite doit lire " + threshold + " dans DriverProfileDef, pas le porter en dur.");
            }
        }

        [Test]
        public void TheBreachTierRunsBeforeEveryManeuverTier()
        {
            var source = CodeOnly(System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs"));
            var ladder = ExtractMethodBody(source, "private bool TickLocalTraffic(DriverProfile profile, float dt, float speed, float longitudinalSpeed)");

            var breach = ladder.IndexOf("TrafficUnblockingAction.IntersectionBreach", StringComparison.Ordinal);
            Assert.That(breach, Is.GreaterThanOrEqualTo(0),
                "Le palier de deblocage d'intersection doit etre decide dans l'echelle de degagement.");
            Assert.That(ladder, Does.Contain("junctionBreached"),
                "Le palier doit se declencher sur l'etat d'interblocage, pas sur une duree locale.");

            // Le recul est le premier palier qui engage une MANOEUVRE : le deblocage d'intersection, qui
            // ne fait qu'assouplir une regle, doit etre decide avant lui.
            //
            // ANO-5.18-10 : l'echelle elle-meme a ete extraite dans TryEscalateRecovery, pour que les
            // quatre causes d'arret durable y entrent par le meme declencheur au lieu du seul predicat
            // de file. L'ordre verifie ne change pas, il se lit sur deux methodes : le palier de
            // deblocage est decide dans TickLocalTraffic, l'appel a l'echelle vient apres, et le recul
            // est un palier de cette echelle -- donc posterieur.
            var escalation = ladder.IndexOf("TryEscalateRecovery(", StringComparison.Ordinal);
            Assert.That(escalation, Is.GreaterThanOrEqualTo(0),
                "L'echelle de recuperation doit etre appelee depuis TickLocalTraffic.");
            Assert.That(breach, Is.LessThan(escalation),
                "Le palier de deblocage est le moins couteux : il precede l'echelle de manoeuvres.");

            var stairs = ExtractMethodBody(source,
                "private bool TryEscalateRecovery(DriverProfile profile, float longitudinalSpeed)");
            var reverse = stairs.IndexOf("BeginManeuver(TrafficUnblockingAction.Reverse", StringComparison.Ordinal);
            Assert.That(reverse, Is.GreaterThanOrEqualTo(0), "Le palier de recul doit rester dans l'echelle.");
            Assert.That(stairs.IndexOf("TryStartDetour(profile, false", StringComparison.Ordinal),
                Is.GreaterThanOrEqualTo(0).And.LessThan(reverse),
                "L'evitement local sur la chaussee reste moins couteux que le recul.");
            Assert.That(stairs.IndexOf("TryStartDetour(profile, true", StringComparison.Ordinal),
                Is.GreaterThan(reverse),
                "Le trottoir reste au-dessus du recul dans l'echelle.");
        }

        [Test]
        public void TheDetectedDeadlockIsLoggedInDevelopmentBuildsWithTheVehiclesInvolved()
        {
            // Garde de source : le journal vit derriere un symbole de compilation et ne s'observe pas
            // a l'execution depuis un test. La forme verifiee est celle de l'acceptation -- build de
            // developpement, vehicules concernes nommes, journal borne.
            var source = CodeOnly(System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs"));
            var report = ExtractMethodBody(source, "private void ReportJunctionDeadlock(in JunctionApproachInfo approach)");

            Assert.That(report, Does.Contain("#if DEVELOPMENT_BUILD || UNITY_EDITOR"),
                "Un interblocage ne se journalise qu'en build de developpement : en build de livraison il ne coute rien.");
            Assert.That(report, Does.Contain("Debug.LogWarning"), "Le journal doit etre lisible dans la Console.");
            Assert.That(report, Does.Contain("approach.JunctionId"), "Le journal doit nommer la jonction concernee.");
            Assert.That(report, Does.Contain("NetworkObjectId"), "Le journal doit nommer les vehicules concernes.");
            Assert.That(report, Does.Contain("claimantScratch"),
                "Les vehicules concernes sont ceux qui REVENDIQUENT la jonction, pas la perception entiere.");
            Assert.That(report, Does.Contain("warnedJunctionDeadlock"),
                "Un interblocage dure : le journal est borne a un avertissement, pas rejoue a chaque pas.");
            Assert.That(report, Does.Contain("Aucun retrait, aucune teleportation"),
                "Le journal rappelle l'absence de palier de retrait -- c'est ce qui evite de le chercher ailleurs.");
        }

        // ------------------------------------------------------------ outils

        private static JunctionClaim Approach(JunctionApproachRule rule, float yawDegrees, float distance, int laneId,
            ulong networkObjectId, bool exitRoom = true, bool stopSatisfied = true, bool signalAllows = true,
            bool breach = false, int junctionKey = JunctionKey, Vector3 entryPoint = default, Vector3 exitPoint = default)
        {
            return new JunctionClaim(junctionKey, rule, Quaternion.Euler(0f, yawDegrees, 0f) * Vector3.forward, distance,
                laneId, networkObjectId, exitRoom, stopSatisfied, signalAllows, breach, entryPoint, exitPoint);
        }

        private static string Describe(in JunctionClaim claim)
        {
            return claim.Rule + "/voie " + claim.LaneId + "/id " + claim.NetworkObjectId + "/d " + claim.DistanceToJunction;
        }

        /// <summary>Identifiant du gagnant tel qu'un pair le designe avec la meme liste, dans l'ordre qu'il a lu.</summary>
        private static ulong ElectedId(IReadOnlyList<JunctionClaim> claims)
        {
            var count = claims.Count;
            var pool = new JunctionClaim[count];
            for (var i = 0; i < count; i++)
            {
                pool[i] = claims[i];
            }

            var winner = JunctionRules.ResolveWinnerIndex(pool, count, new int[count]);
            Assert.That(winner, Is.InRange(0, count - 1), "Une election contestee doit toujours designer quelqu'un.");
            return pool[winner].NetworkObjectId;
        }

        private static Vector3 Flat(Vector3 value)
        {
            return new Vector3(value.x, 0f, value.z);
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
                if (!wasOpen)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static LaneGraph ResolveGraph(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var graph = root.GetComponentInChildren<LaneGraph>(true);
                if (graph != null)
                {
                    return graph;
                }
            }

            Assert.Fail("LaneGraph attendu dans MVP_Run");
            return null;
        }

        private static void WithPrefab(string path, Action<GameObject> body)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path + " attendu");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                body(instance);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static void AssertDecorationIsVisualOnly(GameObject decoration, string junctionId)
        {
            Assert.That(decoration.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0),
                decoration.name + " : la signalisation est decorative, elle ne porte aucun collider -- sinon la zone "
                + "de degagement serait occupee en permanence et la jonction condamnee.");

            var visualRoot = false;
            for (var parent = decoration.transform; parent != null; parent = parent.parent)
            {
                if (parent.name.StartsWith("Visual_", StringComparison.Ordinal))
                {
                    visualRoot = true;
                    break;
                }
            }

            Assert.That(visualRoot, Is.True,
                decoration.name + " : la signalisation doit vivre sous la racine Visual_* du module, donc disparaitre avec elle.");

            // Lu par NOM de type, jamais par reference d'assembly : l'assembly de test EditMode ne
            // depend pas de Unity.AI.Navigation, et l'invariant porte sur ce que porte l'objet, pas
            // sur la facon dont ce fichier le sait.
            foreach (var component in decoration.GetComponentsInChildren<Component>(true))
            {
                var typeName = component.GetType().Name;
                Assert.That(typeName == "NavMeshModifier" || typeName == "NavMeshAgent" || typeName == "NavMeshObstacle",
                    Is.False,
                    decoration.name + " : la signalisation n'entre dans aucun bake ni dans aucune navigation ("
                    + typeName + " trouve).");
            }

            foreach (var view in decoration.GetComponentsInChildren<TrafficSignalLampView>(true))
            {
                var serialized = new SerializedObject(view);
                Assert.That(serialized.FindProperty("junctionId").stringValue, Is.EqualTo(junctionId));
            }
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child != root && child.name == name)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>Retire les lignes de commentaire : une garde de source qui lit la prose se retourne contre elle-meme.</summary>
        private static string CodeOnly(string source)
        {
            var lines = source.Replace("\r\n", "\n").Split('\n');
            var kept = new List<string>(lines.Length);
            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                kept.Add(line);
            }

            return string.Join("\n", kept);
        }

        private static string ExtractRegion(string source, string marker)
        {
            var start = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "Repere introuvable dans la source : " + marker);
            var end = source.IndexOf("private void OnDrawGizmosSelected", start, StringComparison.Ordinal);
            Assert.That(end, Is.GreaterThan(start), "Fin de region introuvable apres : " + marker);
            return source.Substring(start, end - start);
        }

        private static string ExtractMethodBody(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "Methode introuvable : " + signature);

            var open = source.IndexOf('{', start);
            Assert.That(open, Is.GreaterThan(start));
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return source.Substring(open, i - open + 1);
                    }
                }
            }

            Assert.Fail("Corps de methode non ferme : " + signature);
            return string.Empty;
        }

        private static int Occurrences(string source, string token)
        {
            var count = 0;
            var index = source.IndexOf(token, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = source.IndexOf(token, index + token.Length, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
