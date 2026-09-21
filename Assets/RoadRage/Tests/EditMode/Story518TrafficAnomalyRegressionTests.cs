using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// ANO-5.18-01 / ANO-5.18-02 (recette manuelle du 2026-09-21). Cinq symptomes distincts, une seule
    /// cause de fond : l'APPARTENANCE et l'OCCUPATION etaient decidees par des enveloppes de
    /// proximite -- une sphere autour d'un noeud, une bande laterale gonflee par l'orientation de
    /// l'autre, une classification de surface par nom -- la ou seule l'identite de voie et l'emprise
    /// reelle repondent.
    ///
    /// Toutes les grandeurs employees ici sont MESUREES dans MVP_Run (sondes du 2026-09-21), jamais
    /// choisies : c'est ce qui distingue ces gardes des passes de reglage precedentes, qui passaient
    /// leurs tests sur des nombres inventes pendant que le jeu restait bloque.
    /// </summary>
    public sealed class Story518TrafficAnomalyRegressionTests
    {
        /// <summary>Entraxe mesure entre une voie et son sens oppose, et distance mesuree d'un noeud de sortie a cette voie.</summary>
        private const float OppositeLaneOffset = 4f;

        /// <summary>Rayon de degagement de sortie authore dans DriverProfileDef_Default.</summary>
        private const float AuthoredExitClearance = 6f;

        /// <summary>Separation mesuree entre deux approches concurrentes d'une meme jonction du district.</summary>
        private const float CrossingBranchSeparation = 2.83f;

        /// <summary>Hauteur franchissable authoree dans DriverProfileDef_Default.</summary>
        private const float AuthoredCurbHeight = 0.15f;

        private const float HalfWidth = 1.03f;
        private const float HalfLength = 2.22f;
        private const float Margin = 0.3f;
        private static readonly Vector3 VehicleExtents = new Vector3(HalfWidth, 0.71f, HalfLength);

        // ============================================================ ANO-5.18-01 : fausse saturation

        [Test]
        public void AnOncomingVehicleOnTheOppositeLaneNeverSaturatesOurExitLane()
        {
            // LE cas de la capture 1. Deux vehicules se font face a un feu ; chacun est a 4,00 m du
            // noeud de sortie de l'autre, donc a l'interieur du rayon authore de 6 m. La sphere de
            // proximite precedente rendait donc "voie de sortie saturee" DES DEUX COTES a la fois, et
            // le verdict ne pouvait plus se defaire : arret definitif des deux vehicules.
            Assert.That(OppositeLaneOffset, Is.LessThan(AuthoredExitClearance),
                "Repere : la voie opposee EST a l'interieur du rayon authore. C'est pour cela qu'une "
                + "sphere de proximite ne peut pas repondre a la question.");

            var exit = Vector3.zero;
            var lane = Vector3.forward;
            var oncoming = new Vector3(OppositeLaneOffset, 0f, 3f);

            Assert.That(TrafficPerception.OccupiesExitLane(exit, lane, AuthoredExitClearance,
                HalfWidth + Margin, oncoming, VehicleExtents, Quaternion.Euler(0f, 180f, 0f),
                Vector3.back * 8f, 0.5f), Is.False,
                "Un vehicule de la voie opposee n'occupe pas notre voie de sortie.");
        }

        [Test]
        public void AVehicleActuallyStoppedOnTheExitLaneStillSaturatesIt()
        {
            // Contrepartie indispensable : desserrer la saturation ne doit pas autoriser a s'engager
            // dans une intersection dont la sortie est reellement bouchee.
            var occupant = new Vector3(0f, 0f, 3f);
            Assert.That(TrafficPerception.OccupiesExitLane(Vector3.zero, Vector3.forward, AuthoredExitClearance,
                HalfWidth + Margin, occupant, VehicleExtents, Quaternion.identity, Vector3.zero, 0.5f), Is.True);
        }

        [Test]
        public void AVehicleClearingTheExitLaneDoesNotSaturateIt()
        {
            // Attendre un vehicule qui S'EN VA immobiliserait la jonction derriere quelqu'un qui part.
            var leaving = new Vector3(0f, 0f, 3f);
            Assert.That(TrafficPerception.OccupiesExitLane(Vector3.zero, Vector3.forward, AuthoredExitClearance,
                HalfWidth + Margin, leaving, VehicleExtents, Quaternion.identity, Vector3.forward * 6f, 0.5f), Is.False);
        }

        [Test]
        public void WhatSitsUpstreamOfTheExitNodeIsNotExitOccupancy()
        {
            // En amont du noeud de sortie, on est DANS l'intersection : c'est l'arbitrage de priorite
            // qui en repond, pas la place en aval. Les confondre faisait qu'un vehicule engage dans le
            // carrefour saturait la sortie de tous les autres.
            var insideJunction = new Vector3(0f, 0f, -6f);
            Assert.That(TrafficPerception.OccupiesExitLane(Vector3.zero, Vector3.forward, AuthoredExitClearance,
                HalfWidth + Margin, insideJunction, VehicleExtents, Quaternion.identity, Vector3.zero, 0.5f), Is.False);
        }

        [Test]
        public void APedestrianBesideTheExitLaneDoesNotSaturateIt()
        {
            // La version precedente comptait tout CharacterController dans les 6 m, trottoir compris.
            var onTheSidewalk = new Vector3(OppositeLaneOffset, 0f, 2f);
            Assert.That(TrafficPerception.OccupiesExitLane(Vector3.zero, Vector3.forward, AuthoredExitClearance,
                HalfWidth + Margin, onTheSidewalk, new Vector3(0.35f, 0.9f, 0.35f), Quaternion.identity,
                Vector3.zero, 0.5f), Is.False);
        }

        // ============================================================ ANO-5.18-02 : faux leader

        [Test]
        public void TheLaneCorridorIsMeasuredOnTheFootprintAndNotInflatedByOrientation()
        {
            // Cause racine du faux leader. Mesurer "centre de l'autre moins son extension projetee sur
            // notre normale" donne, pour un vehicule PERPENDICULAIRE, sa demi-LONGUEUR (2,22 m) au lieu
            // de sa demi-largeur (1,03 m) : le couloir gonflait de 2,36 m a 3,55 m et avalait la
            // branche transversale du carrefour, distante de 2,83 m.
            var inflated = HalfWidth + Margin + HalfLength;
            Assert.That(CrossingBranchSeparation, Is.LessThan(inflated),
                "Repere : la bande gonflee (" + inflated.ToString("F2") + " m) contient bien la branche "
                + "transversale (" + CrossingBranchSeparation + " m). C'est ainsi qu'un vehicule d'une "
                + "AUTRE branche devenait notre leader.");

            // La distance a l'EMPRISE, elle, ne depend pas de la maniere dont on la projette.
            var perpendicular = new Vector3(CrossingBranchSeparation, 0f, 0f);
            var distance = TrafficPerception.PlanarDistanceToBox(Vector3.zero, perpendicular,
                VehicleExtents, Quaternion.Euler(0f, 90f, 0f));
            Assert.That(distance, Is.EqualTo(CrossingBranchSeparation - HalfLength).Within(0.01f),
                "Le nez du vehicule transversal empiete reellement sur notre couloir : aucun seuil "
                + "geometrique ne peut donc trancher ce cas, et c'est bien la priorite qui doit le faire.");
        }

        [Test]
        public void LaneIdentityAndNotGeometryDecidesWhoIsOurLeader()
        {
            // Puisque la geometrie ne peut pas trancher (test precedent), c'est l'identite de voie qui
            // repond -- et elle repond sans aucun seuil.
            var crossing = new TrafficPerceptionCandidate(1.2f, 20f, 0f, true, false, true, false,
                lateralOffset: CrossingBranchSeparation, halfWidth: HalfLength, isOnOwnPath: false);
            Assert.That(TrafficPerception.TrySelectLeader(new[] { crossing }, 1, 20f, 100f, out _, out _,
                HalfWidth + Margin), Is.False,
                "Un vehicule d'une branche transversale n'est jamais une file a suivre.");

            var ahead = new TrafficPerceptionCandidate(6f, 0f, 0f, true, false, true, false,
                lateralOffset: 0.1f, halfWidth: HalfWidth, isOnOwnPath: true);
            Assert.That(TrafficPerception.TrySelectLeader(new[] { ahead }, 1, 20f, 100f, out var gap, out _,
                HalfWidth + Margin), Is.True);
            Assert.That(gap, Is.EqualTo(6f).Within(0.001f));
        }

        [Test]
        public void ThePathClearanceReadsTheNearestPointOfAnExtendedObstacle()
        {
            // Un obstacle allonge ne se resume pas a son centre : un mur de 8 m de long longeant la
            // voie a 3 m est degage, meme si son centre se projette au milieu de notre trajectoire.
            var path = new List<Vector3> { Vector3.zero, new Vector3(0f, 0f, 20f) };
            var wall = new Vector3(3f, 0f, 10f);
            Assert.That(TrafficPerception.TryPathClearance(path, path.Count, wall,
                new Vector3(0.2f, 1.5f, 4f), Quaternion.identity, out var clearance, out _), Is.True);
            Assert.That(clearance, Is.EqualTo(2.8f).Within(0.05f));
            Assert.That(clearance, Is.GreaterThan(HalfWidth + Margin), "Un mur longeant la voie ne l'obstrue pas.");
        }

        // ============================================================ ANO-5.18-02 : relief fantome

        [Test]
        public void TheAuthoredDrivableReliefIsBelowTheCrossableHeightAndMustNotBeAnObstacle()
        {
            // Objets NOMMES par la sonde du 2026-09-21 comme retenus "obstacle immobile dans la voie",
            // a un degagement mesure de 0,00 m -- la trajectoire leur passe dessus, ce qui est
            // exactement ce que la Story 5.13 demande au vehicule de faire.
            var relief = new[]
            {
                "Rampe_Ouest", "Rampe_Est", "Relief_MarcheBasse_AvenueCenterToEast"
            };

            var scene = EditorSceneManager.OpenScene("Assets/RoadRage/App/Scenes/MVP_Run.unity", OpenSceneMode.Additive);
            try
            {
                var seen = 0;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                    {
                        if (System.Array.IndexOf(relief, collider.name) < 0) continue;
                        seen++;
                        Assert.That(collider.bounds.max.y, Is.LessThanOrEqualTo(AuthoredCurbHeight),
                            collider.name + " culmine a " + collider.bounds.max.y.ToString("F2")
                            + " m : c'est SOUS la hauteur franchissable authoree (" + AuthoredCurbHeight
                            + " m), donc du relief roulant et jamais un obstacle. Le lire comme un mur "
                            + "produisait la boucle obstacle -> manoeuvre -> obstacle rapportee par ANO-5.18-02.");
                    }
                }

                Assert.That(seen, Is.EqualTo(relief.Length),
                    "Le relief roulant de la Story 5.13 doit toujours etre present dans MVP_Run : "
                    + "sans lui cette garde ne prouverait rien.");
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        [Test]
        public void TheCrossableHeightRuleNoLongerDependsOnTheColliderName()
        {
            // La regle de franchissement etait gardee derriere une reconnaissance par NOM
            // (Col_Roadway / Col_Sidewalk_ / Col_Curb_). Le relief de la Story 5.13 n'en porte aucun.
            // La garde verifie que le critere applique est la HAUTEUR, pas le nom.
            var source = System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs");
            var rule = source.Substring(source.IndexOf("private bool IsLowSurface", System.StringComparison.Ordinal));
            rule = rule.Substring(0, rule.IndexOf("\n        }", System.StringComparison.Ordinal));
            Assert.That(rule, Does.Contain("hit.attachedRigidbody == null"),
                "Le franchissement se juge sur une surface STATIQUE...");
            Assert.That(rule, Does.Contain("profile.MaxCurbHeight"),
                "...et sur la hauteur franchissable authoree, independamment du nom du collider.");
        }

        // ============================================================ ANO-5.18-02 : file vs blocage

        [Test]
        public void ALegitimateQueueIsNotABlockageAndNeverStartsAnUnblockingManeuver()
        {
            // Sans cette distinction, tout leader arrete devenait un blocage au bout du delai de
            // klaxon (2 s authorees) : la queue partait en manoeuvre d'evitement, revenait derriere le
            // meme leader, et recommencait -- la boucle rapportee par ANO-5.18-02.
            var legitimate = new[]
            {
                TrafficDecisionReason.Cruise,
                TrafficDecisionReason.FollowingSameLane,
                TrafficDecisionReason.TrafficQueue,
                TrafficDecisionReason.JunctionYield,
                TrafficDecisionReason.ExitSaturated,
                TrafficDecisionReason.YieldTrajectory
            };
            var blocking = new[]
            {
                TrafficDecisionReason.StaticObstacle,
                TrafficDecisionReason.EmergencyBrake,
                TrafficDecisionReason.PerceptionSaturated,
                TrafficDecisionReason.DeadlockRecovery,
                TrafficDecisionReason.UnblockingManeuver,
                TrafficDecisionReason.PlayerImmediateConflict
            };

            var method = typeof(NetworkedAIVehicleDriverController).GetMethod(
                "IsLegitimateWait", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Le predicat de file legitime doit exister.");

            foreach (var reason in legitimate)
                Assert.That(IsLegitimate(reason), Is.True, reason + " est une attente qui se resout seule.");
            foreach (var reason in blocking)
                Assert.That(IsLegitimate(reason), Is.False,
                    reason + " ne se resout pas seule : l'echelle de deblocage garde tout son sens.");
        }

        /// <summary>
        /// Le predicat prend un controleur, que l'EditMode ne peut pas faire naitre spawne. On verifie
        /// donc la table de decision elle-meme, qui est ce que la garde protege.
        /// </summary>
        private static bool IsLegitimate(TrafficDecisionReason reason)
        {
            switch (reason)
            {
                case TrafficDecisionReason.FollowingSameLane:
                case TrafficDecisionReason.TrafficQueue:
                case TrafficDecisionReason.JunctionYield:
                case TrafficDecisionReason.ExitSaturated:
                case TrafficDecisionReason.YieldTrajectory:
                case TrafficDecisionReason.Cruise:
                    return true;
                default:
                    return false;
            }
        }

        [Test]
        public void TheQueuePredicateInTheControllerMatchesTheDocumentedTable()
        {
            // La garde ci-dessus decrit la table ; celle-ci verifie que le controleur l'applique
            // vraiment, pour qu'une divergence entre les deux ne passe pas inapercue.
            var source = System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs");
            var predicate = source.Substring(source.IndexOf("private static bool IsLegitimateWait", System.StringComparison.Ordinal));
            predicate = predicate.Substring(0, predicate.IndexOf("\n        }", System.StringComparison.Ordinal));
            foreach (var reason in new[] { "FollowingSameLane", "TrafficQueue", "JunctionYield", "ExitSaturated", "YieldTrajectory", "Cruise" })
                Assert.That(predicate, Does.Contain(reason), reason + " doit figurer parmi les attentes legitimes.");
            Assert.That(predicate, Does.Not.Contain("case TrafficDecisionReason.StaticObstacle"),
                "Un obstacle immobile n'est jamais une attente qui se resout seule.");

            // La propriete verifiee est un ORDRE, pas une adjacence de texte : l'exclusion de la file
            // legitime precede la lecture de la vitesse. L'assertion litterale d'avant cassait des
            // qu'un terme s'intercalait entre les deux -- ici la garde de face-a-face -- alors que la
            // propriete tenait toujours.
            var blockedExpression = source.Substring(source.IndexOf("var blocked = hasPerceivedLeader", System.StringComparison.Ordinal));
            blockedExpression = blockedExpression.Substring(0, blockedExpression.IndexOf(";", System.StringComparison.Ordinal));
            Assert.That(blockedExpression.IndexOf("!queueing", System.StringComparison.Ordinal),
                Is.GreaterThanOrEqualTo(0).And
                    .LessThan(blockedExpression.IndexOf("perceivedLeaderSpeed <= stuckSpeedThreshold", System.StringComparison.Ordinal)),
                "Le blocage doit exclure la file legitime AVANT de regarder la vitesse.");
        }

        [Test]
        public void TheFollowingGapIsMeasuredWhereTheLeaderEntersTheCorridorNotWhereItIsNearest()
        {
            // Pour un leader LONG, la distance minimale a notre trajectoire tombe le long de son flanc,
            // donc plusieurs metres apres son pare-chocs arriere. S'en servir comme ecart de suivi le
            // surestimerait et ferait coller le suiveur -- exactement l'inverse du but.
            var path = new List<Vector3> { Vector3.zero, new Vector3(0f, 0f, 30f) };
            var leader = new Vector3(0f, 0f, 12f);   // centre a 12 m, donc arriere a 12 - 2,22 = 9,78 m

            Assert.That(TrafficPerception.TryPathClearance(path, path.Count, leader, VehicleExtents,
                Quaternion.identity, out _, out var entry, HalfWidth + Margin), Is.True);
            Assert.That(entry, Is.LessThanOrEqualTo(12f - HalfLength + 0.5f),
                "L'ecart se mesure a l'entree du leader dans le couloir, pas au milieu de son flanc.");
        }

        [Test]
        public void AQueueThatNeverAdvancesStopsBeingTreatedAsLegitimate()
        {
            // Consequence directe de la correction du § 5 : sur un anneau ferme, A peut suivre B, B
            // suivre C et C suivre A. Chacun voit devant lui une attente reguliere, donc plus personne
            // ne declencherait l'echelle de deblocage. Le garde-fou borne ce seul cas, avec le delai
            // d'escalade DEJA authore -- il ne remplace pas la decision nominale.
            var source = System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs");
            Assert.That(source, Does.Contain("queueElapsedSeconds >= profile.JunctionEscalationDelay"),
                "L'attente circulaire se rompt sur le delai d'escalade authore, jamais sur une valeur nouvelle.");
            Assert.That(source, Does.Contain("queueing = false"),
                "Une file immobile trop longtemps redevient un blocage, et l'echelle reprend la main.");
            Assert.That(source, Does.Contain("attente=") , "L'anciennete de la file doit etre lisible en recette.");
        }

        // ============================================================ ANO-5.18-01 : joueur trottoir

        [Test]
        public void ThePlayerOnlyEntersTheDecisionThroughAnImmediateOnLaneConflict()
        {
            var source = System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs");

            // 1. Un pieton hors chaussee ne participe a aucune prediction de conflit.
            Assert.That(source, Does.Contain("IsOnCarriageway"),
                "La participation d'un pieton se decide sur la SURFACE qu'il foule, pas sur une distance.");
            Assert.That(source, Does.Contain("collisionThreat && !offCarriageway"),
                "Un pieton hors chaussee est exclu de la prediction de conflit.");

            // 2. Le conflit joueur nomme exige simultanement voie, position et contact.
            Assert.That(source, Does.Contain("gap <= profile.MinimumGap + profile.SafetyMargin"),
                "Le seuil de contact est l'ecart minimal authore plus la marge (0,75 + 0,30 m), "
                + "jamais une constante nouvelle.");

            // 3. La reponse est un evitement, pas un arret.
            Assert.That(source, Does.Contain("TryStartDetour(profile, false) || TryStartDetour(profile, true)"),
                "Le joueur en travers declenche un contournement par l'echelle deja en place.");
        }

        [Test]
        public void EveryStopReasonCarriesAnIdentifiedSubject()
        {
            // Exigence de diagnostic de ANO-5.18 : "obstacle immobile" sans le NOM de l'objet est
            // inverifiable en recette, et c'est ce qui a laisse passer les passes precedentes.
            var source = System.IO.File.ReadAllText(
                "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs");
            Assert.That(source, Does.Contain("ComposeDecisionDetail"));
            foreach (var field in new[] { "leaderName", "exitSaturatedBy", "playerImmediateName", "leaderLaneNode" })
                Assert.That(source, Does.Contain(field), field + " doit alimenter la trace de decision.");
        }
    }
}
