using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// UNE CESSION A UNE LIGNE (ANO-5.18-08).
    ///
    /// Recette : "les voitures cedent leur point de conflit trop tot au milieu des troncons de route
    /// et pas juste devant l'intersection". Le defaut n'etait pas la portee de perception -- il faut
    /// voir loin pour freiner doux -- mais l'absence de CIBLE D'ARRET : une cession de trajectoire
    /// posait un freinage d'urgence a la position courante, jusqu'a la portee de prediction en amont.
    ///
    /// Les jonctions authorees n'avaient pas ce defaut (elles portent une ligne d'arret). Les quatre
    /// giratoires du district n'en portent aucune : c'est pourquoi ils concentraient le symptome.
    /// </summary>
    public sealed class Story518YieldStopLineTests
    {
        private const float FrontOffset = 2.22f;   // demi-longueur mesuree du Greybox_AIVehicle
        private const float HalfWidth = 1.03f;     // demi-largeur mesuree
        private const float Margin = 0.30f;        // SafetyMargin authoree

        private static float Gap(float metresAhead)
        {
            return TrafficPerception.ResolveConflictStopGap(Vector3.zero, Vector3.forward,
                new Vector3(0f, 0f, metresAhead), FrontOffset, HalfWidth, Margin);
        }

        [Test]
        public void TheLineSitsBeforeTheMeetingPointByTheVehicleFootprintAndTheAuthoredMargin()
        {
            // 20 m devant : l'avant du vehicule doit s'arreter 3,55 m avant le point de rencontre.
            Assert.That(Gap(20f), Is.EqualTo(20f - FrontOffset - HalfWidth - Margin).Within(0.001f));
        }

        [Test]
        public void ADistantConflictNoLongerCommandsAnImmediateStop()
        {
            // C'est LE cas de recette : le conflit est vu a 25 m, au milieu du troncon. Avant le
            // correctif la cession y posait le frein ; desormais elle laisse 21,45 m a parcourir.
            Assert.That(Gap(25f), Is.GreaterThan(20f),
                "Un conflit lointain doit laisser de la route avant la ligne, pas arreter sur place.");
        }

        [Test]
        public void TheGapShrinksMetreForMetreAsTheVehicleCloses()
        {
            var previous = float.PositiveInfinity;
            for (var distance = 30f; distance >= 4f; distance -= 1f)
            {
                var gap = Gap(distance);
                Assert.That(gap, Is.LessThan(previous), "L'ecart a la ligne doit decroitre en approchant.");
                previous = gap;
            }
        }

        [Test]
        public void ReachingTheLineTurnsTheApproachIntoAFullStop()
        {
            // En deca de l'emprise plus la marge, l'ecart passe negatif : le controleur repasse alors
            // a l'arret ferme. La bascule est continue, il n'y a pas de zone morte entre les deux.
            var threshold = FrontOffset + HalfWidth + Margin;
            Assert.That(Gap(threshold + 0.01f), Is.GreaterThan(0f));
            Assert.That(Gap(threshold - 0.01f), Is.LessThan(0f));
        }

        [Test]
        public void AConflictBehindUsNeverProducesAPositiveGap()
        {
            Assert.That(Gap(-5f), Is.LessThan(0f), "Un point deja depasse ne se cede plus.");
            Assert.That(Gap(0f), Is.LessThan(0f));
        }

        [Test]
        public void ADegenerateHeadingIsRefusedInsteadOfInventingRoom()
        {
            Assert.That(TrafficPerception.ResolveConflictStopGap(Vector3.zero, Vector3.zero,
                    new Vector3(0f, 0f, 20f), FrontOffset, HalfWidth, Margin),
                Is.EqualTo(float.NegativeInfinity),
                "Sans cap, aucune distance longitudinale n'est definie : l'arret ferme reste le repli.");
        }

        [Test]
        public void TheLineIsMeasuredOnTheChordSoACurveErrsOnTheEarlySide()
        {
            // Point de conflit a 20 m d'arc mais 45 deg sur le cote : la corde projetee est plus
            // courte que l'arc, donc le vehicule s'arrete AVANT la ligne, jamais apres.
            var oblique = new Vector3(Mathf.Sin(Mathf.PI / 4f), 0f, Mathf.Cos(Mathf.PI / 4f)) * 20f;
            var gap = TrafficPerception.ResolveConflictStopGap(Vector3.zero, Vector3.forward, oblique,
                FrontOffset, HalfWidth, Margin);
            Assert.That(gap, Is.LessThan(Gap(20f)));
        }

        // ------------------------------------------------------------------ cablage

        [Test]
        public void TheDriverUsesTheNearestOfTheTwoStopTargetsForItsPedal()
        {
            // Une seule pedale entre dans la couche physique : la contrainte la plus proche commande.
            var source = ReadSource("NetworkedAIVehicleDriverController.Traffic.cs");
            Assert.That(source, Does.Match(@"PlannedStopGap\s*=>\s*Mathf\.Min\(junctionSpeedLimitGap,\s*conflictSpeedLimitGap\)"),
                "La contrainte longitudinale doit combiner la ligne de jonction et la ligne de cession.");

            var driver = ReadSource("NetworkedAIVehicleDriverController.cs");
            Assert.That(driver, Does.Contain("var junctionGap = PlannedStopGap;"),
                "Le calcul de pedale doit lire la contrainte combinee, pas la seule jonction.");
        }

        [Test]
        public void AYieldingVehicleNeverCommitsAJunctionTraversal()
        {
            // S'engager pendant une cession avancerait le waypoint dans l'aire de conflit alors que
            // le vehicule freine encore vers sa ligne.
            var source = ReadSource("NetworkedAIVehicleDriverController.Traffic.cs");
            Assert.That(source, Does.Match(@"junctionStopGap\s*<=\s*0f\s*&&\s*!yieldingTrajectory"),
                "L'engagement doit etre conditionne a l'absence de cession en cours.");
        }

        private static string ReadSource(string fileName)
        {
            var path = "Assets/RoadRage/Features/Vehicles/" + fileName;
            Assert.That(System.IO.File.Exists(path), Is.True, path + " introuvable");
            return Regex.Replace(System.IO.File.ReadAllText(path), @"\r\n", "\n");
        }
    }

    /// <summary>
    /// UN FACE-A-FACE CONTRE UN VEHICULE A L'ARRET EST UN OBSTACLE (ANO-5.18-07).
    ///
    /// Recette : une voiture arrivee pendant la manoeuvre de virage d'un prioritaire s'arrete a son
    /// stop EN TRAVERS de sa trajectoire. Le prioritaire se declare en face-a-face et freine ; le
    /// second ne peut plus avancer, sa permission de jonction etant refusee. Plus rien ne bouge.
    ///
    /// Deux verrous distincts produisaient ce blocage, et il fallait les deux pour l'obtenir :
    ///
    /// 1. le predicat de FILE lisait l'attente reguliere de l'autre (JunctionYield) comme une file
    ///    legitime -- alors qu'une file se resout quand sa tete AVANCE, et que celle-la, en
    ///    avancant, entre dans nous ;
    /// 2. la garde de face-a-face excluait l'echelle de deblocage pour celui qui ne cede pas -- ce
    ///    qui est juste tant que l'autre ROULE, et faux des qu'il est arrete.
    ///
    /// Le contournement lui-meme n'est pas nouveau : c'est l'echelle de la Story 5.17, et
    /// <c>ValidatePath</c> continue de refuser une echappatoire sans place sure.
    /// </summary>
    public sealed class Story518StalledHeadOnTests
    {
        private static string Source()
        {
            const string path = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs";
            Assert.That(System.IO.File.Exists(path), Is.True, path + " introuvable");
            return Regex.Replace(System.IO.File.ReadAllText(path), @"\s+", " ");
        }

        [Test]
        public void AVehicleFacingUsIsNeverCountedAsALegitimateQueue()
        {
            Assert.That(Source(), Does.Match(
                    @"var rawQueueing = hasPerceivedLeader && !headOnConflict && IsLegitimateWait\(leaderPeer\)"),
                "Le predicat de file doit exclure le face-a-face : une file se resout quand sa tete"
                + " avance, or celle-la avancerait dans nous.");
        }

        [Test]
        public void AStalledOncomingVehicleReopensTheUnblockingLadder()
        {
            Assert.That(Source(), Does.Match(
                    @"\(!headOnConflict \|\| headOnYield \|\| headOnStationary\)"),
                "Le prioritaire doit pouvoir contourner un face-a-face IMMOBILE ; contre un vehicule"
                + " qui roule, la garde d'arret reste entiere.");
        }

        [Test]
        public void TheStalledFlagIsDerivedFromTheOtherVehicleSpeedAndNothingElse()
        {
            Assert.That(Source(), Does.Match(
                    @"headOnStationary = closingHeadOn && otherSpeed <= stuckSpeedThreshold"),
                "Le seuil est celui deja authore pour l'immobilite, pas un delai nouveau.");
        }

        [Test]
        public void TheStalledFlagIsClearedOnEveryPerceptionPass()
        {
            // Un drapeau de perception qui survit a son tour de scrutation transforme un etat
            // transitoire en etat permanent : c'est le mode de defaillance de toute cette famille.
            Assert.That(Source(), Does.Match(@"headOnYield = false; headOnStationary = false;"),
                "headOnStationary doit etre remis a zero avec les autres drapeaux de conflit.");
        }
    }
}
