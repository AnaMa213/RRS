using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.12 : les pneumatiques et la direction.
    ///
    /// Quatre familles de gardes, toutes sans scene de jeu ni Netcode :
    /// 1. le modele de pneu pur (<see cref="VehicleTireModel"/>) : courbe de force avec pic puis chute
    ///    progressive, budget d'adherence unique partage par les deux glissements, blocage de roue qui
    ///    effondre l'adherence laterale, valeur finie partout ;
    /// 2. le train roulant : couple moteur qui s'eteint a l'approche de la pointe authoree, marche
    ///    arriere qui ne s'engage que sous le seuil de changement de sens, frein moteur qui retient un
    ///    vehicule gare, rotation de roue bornee ;
    /// 3. le modele de direction pur (<see cref="VehicleSteeringModel"/>) : angle qui diminue avec la
    ///    vitesse, retour au centre progressif, seule la roue authoree directrice braque ;
    /// 4. les invariants de code que rien d'autre ne garde : plus aucune ecriture de vitesse ni de
    ///    rotation dans le chemin joueur, frein a main transporte par le chemin d'intent EXISTANT,
    ///    couche physique sans disposition ni autorite, et etat intermediaire de l'IA nomme 5.14.
    /// </summary>
    public sealed class Story512TireForcesAndSteeringTests
    {
        private const string ProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string AiPrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab";
        private const string PlayerControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string AiControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";
        private const string PhysicsBodySourcePath = "Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs";
        private const string TireModelSourcePath = "Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs";
        private const string SteeringModelSourcePath = "Assets/RoadRage/Features/Vehicles/VehicleSteeringModel.cs";
        private const string IntentSourcePath = "Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs";

        /// <summary>Charge portee par une roue du vehicule de reference (masse 1200 kg, 4 roues).</summary>
        private const float StaticLoad = 1200f * 9.81f / 4f;

        private const float Epsilon = 0.0001f;

        // ------------------------------------------------------------------ le pneu

        [Test]
        public void GripFractionPeaksAtExactlyOneAndNeverExceedsIt()
        {
            const float PeakSlip = 0.14f;
            const float Falloff = 0.7f;

            Assert.That(VehicleTireModel.ResolveGripFraction(PeakSlip, PeakSlip, Falloff), Is.EqualTo(1f).Within(Epsilon),
                "Au pic, la part d'adherence transmise vaut exactement 1 : la force maximale est exactement adherence x charge.");
            Assert.That(VehicleTireModel.ResolveGripFraction(PeakSlip / 2f, PeakSlip, Falloff), Is.EqualTo(0.5f).Within(Epsilon),
                "Sous le pic, la montee est lineaire.");
            Assert.That(VehicleTireModel.ResolveGripFraction(0f, PeakSlip, Falloff), Is.EqualTo(0f),
                "Glissement nul : aucune force transmise, sans exception.");
            Assert.That(VehicleTireModel.ResolveGripFraction(PeakSlip, 0f, Falloff), Is.EqualTo(0f),
                "Pic nul : refuse par le profil, donc jamais de division par zero ici.");
        }

        [Test]
        public void GripFallsOffProgressivelyPastThePeakAndNeverToZero()
        {
            const float PeakSlip = 0.14f;
            const float Falloff = 0.7f;

            var previous = 1f;
            var largestStep = 0f;
            for (var i = 1; i <= 4000; i++)
            {
                var slip = PeakSlip + (i * 0.001f);
                var grip = VehicleTireModel.ResolveGripFraction(slip, PeakSlip, Falloff);

                Assert.That(grip, Is.LessThanOrEqualTo(previous), "Au-dela du pic la courbe ne remonte jamais (glissement " + slip + ").");
                Assert.That(grip, Is.GreaterThanOrEqualTo(Falloff - Epsilon),
                    "La force ne tombe jamais a zero : la derive reste atteignable ET recuperable (glissement " + slip + ").");
                largestStep = Mathf.Max(largestStep, previous - grip);
                previous = grip;
            }

            Assert.That(largestStep, Is.LessThan(0.01f),
                "Aucun saut : la perte d'adherence est une courbe, jamais un seuil binaire.");
        }

        [Test]
        public void TireForceSharesOneBudgetAndCarriesTheSignOfEachSlip()
        {
            const float Adherence = 1.2f;
            const float PeakSlipRatio = 0.14f;
            const float PeakSlipAngle = 8f;
            const float Falloff = 0.7f;

            var atPeak = VehicleTireModel.ResolveTireForces(
                PeakSlipRatio, PeakSlipRatio, 0f, PeakSlipAngle, Falloff, Adherence, StaticLoad);
            Assert.That(atPeak.magnitude, Is.EqualTo(Adherence * StaticLoad).Within(1f),
                "Au pic, la force transmise vaut exactement adherence x charge : jamais plus.");

            var driving = VehicleTireModel.ResolveTireForces(
                0.05f, PeakSlipRatio, 0f, PeakSlipAngle, Falloff, Adherence, StaticLoad);
            Assert.That(driving.x, Is.GreaterThan(0f), "Roue motrice (bande de roulement plus rapide que le sol) : force vers l'avant.");
            Assert.That(driving.y, Is.EqualTo(0f).Within(Epsilon), "Sans angle de glissement, aucune force laterale.");

            var braking = VehicleTireModel.ResolveTireForces(
                -0.05f, PeakSlipRatio, 0f, PeakSlipAngle, Falloff, Adherence, StaticLoad);
            Assert.That(braking.x, Is.LessThan(0f), "Roue freinee : la force s'oppose a l'avancee.");

            var slidingRight = VehicleTireModel.ResolveTireForces(
                0f, PeakSlipRatio, -6f, PeakSlipAngle, Falloff, Adherence, StaticLoad);
            Assert.That(slidingRight.y, Is.LessThan(0f),
                "Glissement vers la droite (angle negatif) : la force laterale s'y oppose, elle est dirigee vers la gauche.");

            for (var ratio = -1f; ratio <= 1f; ratio += 0.05f)
            {
                for (var angle = -30f; angle <= 30f; angle += 3f)
                {
                    var force = VehicleTireModel.ResolveTireForces(
                        ratio, PeakSlipRatio, angle, PeakSlipAngle, Falloff, Adherence, StaticLoad);
                    Assert.That(force.magnitude, Is.LessThanOrEqualTo((Adherence * StaticLoad) + 1f),
                        "Un pneu ne transmet jamais plus que adherence x charge (glissement " + ratio + " / " + angle + ").");
                }
            }
        }

        [Test]
        public void ALockedWheelLosesItsLateralGripSoTheHandbrakeDriftIsReachable()
        {
            const float Adherence = 1.2f;
            const float PeakSlipRatio = 0.14f;
            const float PeakSlipAngle = 8f;
            const float Falloff = 0.7f;

            var rolling = VehicleTireModel.ResolveTireForces(
                0f, PeakSlipRatio, PeakSlipAngle, PeakSlipAngle, Falloff, Adherence, StaticLoad);
            var locked = VehicleTireModel.ResolveTireForces(
                1f, PeakSlipRatio, PeakSlipAngle, PeakSlipAngle, Falloff, Adherence, StaticLoad);

            Assert.That(Mathf.Abs(rolling.y), Is.EqualTo(Adherence * StaticLoad).Within(1f),
                "Roue qui roule : l'adherence laterale disponible est pleine.");
            Assert.That(Mathf.Abs(locked.y), Is.LessThan(Mathf.Abs(rolling.y) * 0.25f),
                "Roue bloquee : le glissement longitudinal sature prend tout le budget, l'adherence laterale s'effondre. "
                + "C'est le mecanisme de la derive au frein a main -- un pneu bloque ne tient plus la caisse en travers.");
            Assert.That(Mathf.Abs(locked.x), Is.GreaterThan(Adherence * StaticLoad * 0.5f),
                "Et cette adherence perdue se retrouve en freinage : la roue bloque freine fort, en ligne.");
        }

        [Test]
        public void AWheelInTheAirTransmitsNothingAndNeverProducesANonFiniteValue()
        {
            Assert.That(VehicleTireModel.ResolveTireForces(0.1f, 0.14f, 5f, 8f, 0.7f, 1.2f, 0f), Is.EqualTo(Vector2.zero),
                "Charge nulle : aucun effort de pneu, roue en l'air.");
            Assert.That(VehicleTireModel.ResolveTireForces(0.1f, 0.14f, 5f, 8f, 0.7f, 0f, StaticLoad), Is.EqualTo(Vector2.zero),
                "Adherence nulle : aucun effort, jamais de division par zero.");

            var nan = VehicleTireModel.ResolveTireForces(float.NaN, 0.14f, 5f, 8f, 0.7f, 1.2f, StaticLoad);
            var infinite = VehicleTireModel.ResolveTireForces(0.1f, 0.14f, float.PositiveInfinity, 8f, 0.7f, 1.2f, StaticLoad);

            Assert.That(float.IsNaN(nan.x) || float.IsNaN(nan.y), Is.False, "Glissement non fini : repli sur zero, jamais NaN.");
            Assert.That(float.IsNaN(infinite.x) || float.IsNaN(infinite.y), Is.False, "Angle non fini : repli sur zero, jamais NaN.");

            var airborne = VehicleTireModel.SampleTire(false, 0f, 0f, 0f, 0f, 1.2f);
            Assert.That(airborne.Grounded, Is.False);
            Assert.That(airborne.MaximumForce, Is.EqualTo(0f), "En l'air : adherence disponible nulle, donc force maximale nulle.");
            Assert.That(airborne.GripUsage, Is.EqualTo(0f), "Aucune division par zero quand la roue ne porte pas.");
        }

        [Test]
        public void SlipRatioAndSlipAngleReportTheRealMotionRatherThanAnArbitraryValueAtRest()
        {
            Assert.That(VehicleTireModel.ResolveSlipRatio(0f, 10f), Is.EqualTo(-1f).Within(Epsilon),
                "Roue bloquee a 10 m/s : glissement -1, la bande de roulement n'avance plus.");
            Assert.That(VehicleTireModel.ResolveSlipRatio(10f, 10f), Is.EqualTo(0f).Within(Epsilon), "Roue qui roule sans glisser.");
            Assert.That(VehicleTireModel.ResolveSlipRatio(0f, 0f), Is.EqualTo(0f),
                "A l'arret, le plancher de vitesse evite un glissement arbitraire : sans lui, le rapport serait 0/0.");

            Assert.That(VehicleTireModel.ResolveSlipAngleDegrees(10f, 0f), Is.EqualTo(0f).Within(Epsilon));
            Assert.That(VehicleTireModel.ResolveSlipAngleDegrees(10f, 1f), Is.EqualTo(-5.7106f).Within(0.001f),
                "Glissement vers la droite : angle negatif (convention du modele).");
            Assert.That(VehicleTireModel.ResolveSlipAngleDegrees(-10f, 1f), Is.EqualTo(-5.7106f).Within(0.001f),
                "En marche arriere la convention ne s'inverse pas : le denominateur est en valeur absolue.");
            Assert.That(float.IsNaN(VehicleTireModel.ResolveSlipAngleDegrees(float.NaN, 0f)), Is.False);
        }

        [Test]
        public void TheLowSpeedRampKeepsAParkedVehicleFromSlidingAway()
        {
            Assert.That(VehicleTireModel.ResolveLowSpeedRamp(0f), Is.EqualTo(0f),
                "A l'arret, la force de contact est nulle : sans cette attenuation, un vehicule gare brouterait au lieu de s'immobiliser.");
            Assert.That(VehicleTireModel.ResolveLowSpeedRamp(VehicleTireModel.SlipReferenceSpeed), Is.EqualTo(1f));
            Assert.That(VehicleTireModel.ResolveLowSpeedRamp(30f), Is.EqualTo(1f), "Au-dessus de la vitesse de reference, pleine force.");
            Assert.That(VehicleTireModel.ResolveLowSpeedRamp(-5f), Is.EqualTo(1f), "La rampe ne depend pas du sens.");
        }

        // ---------------------------------------------------------- le train roulant

        [Test]
        public void DriveTorqueIsFullBelowTheBandAndDiesExactlyAtTheAuthoredTopSpeed()
        {
            Assert.That(VehicleTireModel.ResolveDriveTorqueFactor(0f, 18f), Is.EqualTo(1f), "Depart : effort moteur plein.");
            Assert.That(VehicleTireModel.ResolveDriveTorqueFactor(18f, 18f), Is.EqualTo(0f),
                "A la pointe authoree, l'effort s'eteint : la vitesse de pointe est APPROCHEE par une pente de force, jamais posee.");
            Assert.That(VehicleTireModel.ResolveDriveTorqueFactor(19f, 18f), Is.EqualTo(0f), "Au-dela de la pointe, aucun effort moteur.");

            var previous = 1f;
            for (var speed = 16f; speed <= 18f; speed += 0.05f)
            {
                var factor = VehicleTireModel.ResolveDriveTorqueFactor(speed, 18f);
                Assert.That(factor, Is.LessThanOrEqualTo(previous), "L'extinction est monotone.");
                previous = factor;
            }
        }

        [Test]
        public void ReverseEngagesOnlyBelowTheDirectionSpeedThreshold()
        {
            const float MinimumDirectionSpeed = 0.25f;

            var fromRest = VehicleTireModel.ResolveWheelDriveTorque(0f, 1f, 0f, MinimumDirectionSpeed, 900f, 700f, 18f, 7f);
            Assert.That(fromRest, Is.EqualTo(-700f).Within(Epsilon), "A l'arret, l'entree de frein engage la marche arriere.");

            var whileMoving = VehicleTireModel.ResolveWheelDriveTorque(0f, 1f, 5f, MinimumDirectionSpeed, 900f, 700f, 18f, 7f);
            Assert.That(whileMoving, Is.EqualTo(0f),
                "En mouvement, la meme entree FREINE et ne fait rien d'autre : aucune inversion instantanee (regle conservee de la Story 3.2).");

            var reversingAtTop = VehicleTireModel.ResolveWheelDriveTorque(0f, 1f, -7f, MinimumDirectionSpeed, 900f, 700f, 18f, 7f);
            Assert.That(reversingAtTop, Is.EqualTo(0f), "La marche arriere s'eteint a sa propre pointe authoree.");

            Assert.That(VehicleTireModel.ResolveWheelDriveTorque(1f, 0f, 0f, MinimumDirectionSpeed, 900f, 700f, 18f, 7f),
                Is.EqualTo(900f).Within(Epsilon), "Plein gaz depuis l'arret : couple moteur plein.");
            Assert.That(VehicleTireModel.ResolveWheelDriveTorque(0f, 0f, 5f, MinimumDirectionSpeed, 900f, 700f, 18f, 7f),
                Is.EqualTo(0f), "Aucune entree : aucun couple moteur.");
        }

        [Test]
        public void ServiceBrakeNeedsTheVehicleToBeMovingAndTheCoastTorqueHoldsAParkedVehicle()
        {
            const float MinimumDirectionSpeed = 0.25f;

            Assert.That(VehicleTireModel.ResolveWheelBrakeTorque(0f, 1f, 4f, MinimumDirectionSpeed, 700f, 120f),
                Is.EqualTo(700f).Within(Epsilon), "Frein de service au-dessus du seuil de changement de sens.");
            Assert.That(VehicleTireModel.ResolveWheelBrakeTorque(0f, 1f, 0.1f, MinimumDirectionSpeed, 700f, 120f),
                Is.EqualTo(0f), "Sous le seuil, l'entree de frein ne freine pas : elle va engager la marche arriere.");
            Assert.That(VehicleTireModel.ResolveWheelBrakeTorque(1f, 0f, 4f, MinimumDirectionSpeed, 700f, 120f),
                Is.EqualTo(0f), "Plein gaz : aucun frein de service.");
            Assert.That(VehicleTireModel.ResolveWheelBrakeTorque(0f, 0f, 4f, MinimumDirectionSpeed, 700f, 120f),
                Is.EqualTo(120f).Within(Epsilon),
                "Aucune entree : le frein moteur authore retient le vehicule. Sans lui, une roue libre n'opposerait rien au roulement "
                + "et un vehicule gare sans conducteur partirait tout seul.");
        }

        [Test]
        public void WheelSpinIsBoundedAndABrakeCanOnlyBringItBackToZero()
        {
            const float Radius = 0.33f;
            const float Inertia = 3f;
            const float Step = 0.02f;

            var spinning = VehicleTireModel.IntegrateWheelAngularVelocity(0f, 900f, 0f, 0f, Radius, Inertia, Step);
            Assert.That(spinning, Is.EqualTo(900f / Inertia * Step).Within(Epsilon), "Roue motrice libre : elle accelere de son couple.");

            var brakedToStop = VehicleTireModel.IntegrateWheelAngularVelocity(spinning, 0f, 3000f, 0f, Radius, Inertia, Step);
            Assert.That(brakedToStop, Is.EqualTo(0f),
                "Un frein ramene la rotation a zero, jamais en dessous : une roue ne se met pas a tourner a l'envers parce qu'on freine.");

            var bounded = 0f;
            for (var i = 0; i < 5000; i++)
            {
                bounded = VehicleTireModel.IntegrateWheelAngularVelocity(bounded, 900f, 0f, 0f, Radius, Inertia, Step);
            }

            Assert.That(float.IsFinite(bounded), Is.True);
            Assert.That(bounded, Is.LessThanOrEqualTo(400f),
                "Une roue en l'air a plein gaz est bornee : sans borne, le glissement qu'elle produirait a l'atterrissage serait absurde.");

            Assert.That(VehicleTireModel.IntegrateWheelAngularVelocity(spinning, 0f, 0f, 0f, Radius, Inertia, 0f),
                Is.EqualTo(spinning), "Pas de temps nul : la rotation ne bouge pas, aucune division par zero.");
            Assert.That(VehicleTireModel.IntegrateWheelAngularVelocity(5f, 0f, 0f, 0f, Radius, 0f, Step), Is.EqualTo(5f),
                "Inertie nulle : refusee par le profil, la rotation est rendue telle quelle plutot que divisee par zero.");
        }

        // ------------------------------------------------------------- la direction

        [Test]
        public void SteerAngleShrinksWithSpeedAndIsZeroBelowTheDirectionThreshold()
        {
            const float MinimumDirectionSpeed = 0.25f;

            var atLowSpeed = VehicleSteeringModel.ResolveSteerAngleDegrees(1f, 1f, MinimumDirectionSpeed, 32f, 10f, 18f);
            var atHalfSpeed = VehicleSteeringModel.ResolveSteerAngleDegrees(1f, 9f, MinimumDirectionSpeed, 32f, 10f, 18f);
            var atTopSpeed = VehicleSteeringModel.ResolveSteerAngleDegrees(1f, 18f, MinimumDirectionSpeed, 32f, 10f, 18f);

            Assert.That(atLowSpeed, Is.GreaterThan(atHalfSpeed), "L'angle de roue diminue avec la vitesse.");
            Assert.That(atHalfSpeed, Is.GreaterThan(atTopSpeed));
            Assert.That(atTopSpeed, Is.EqualTo(10f).Within(Epsilon), "A la vitesse de reduction complete, l'angle haut authore s'applique.");

            Assert.That(VehicleSteeringModel.ResolveSteerAngleDegrees(1f, 0.1f, MinimumDirectionSpeed, 32f, 10f, 18f), Is.EqualTo(0f),
                "Sous le seuil de vitesse, les roues ne braquent pas : l'angle ne s'accumule pas pour se liberer d'un coup au premier metre.");
            Assert.That(VehicleSteeringModel.ResolveSteerAngleDegrees(0f, 9f, MinimumDirectionSpeed, 32f, 10f, 18f), Is.EqualTo(0f),
                "Aucune entree de direction : aucune consigne d'angle.");
            Assert.That(VehicleSteeringModel.ResolveSteerAngleDegrees(-1f, 1f, MinimumDirectionSpeed, 32f, 10f, 18f), Is.LessThan(0f),
                "Le signe de l'entree est porte tel quel : la gauche et la droite restent distinctes.");
            Assert.That(float.IsNaN(VehicleSteeringModel.ResolveSteerAngleDegrees(float.NaN, 5f, MinimumDirectionSpeed, 32f, 10f, 18f)), Is.False);
        }

        [Test]
        public void SteerReturnsToCentreAtTheAuthoredReturnRateAndNeverJumps()
        {
            Assert.That(VehicleSteeringModel.ResolveSteerRateDegreesPerSecond(-25f, 0f, 180f, 140f), Is.EqualTo(140f),
                "Retour au centre : c'est le taux de rappel authore, pas le taux de braquage.");
            Assert.That(VehicleSteeringModel.ResolveSteerRateDegreesPerSecond(0f, -25f, 180f, 140f), Is.EqualTo(180f),
                "Braquage : le taux de braquage authore.");
            Assert.That(VehicleSteeringModel.ResolveSteerRateDegreesPerSecond(-25f, -5f, 180f, 140f), Is.EqualTo(140f),
                "L'angle revient vers le centre sans changer de signe : c'est encore un rappel.");

            var angle = VehicleSteeringModel.MoveSteerAngleDegrees(0f, -28f, 180f, 0.02f);
            Assert.That(angle, Is.EqualTo(-3.6f).Within(Epsilon), "Un pas de temps de braquage : l'angle avance a taux borne, il ne saute pas.");

            for (var i = 0; i < 200; i++)
            {
                angle = VehicleSteeringModel.MoveSteerAngleDegrees(angle, 0f, 140f, 0.02f);
            }

            Assert.That(angle, Is.EqualTo(0f).Within(Epsilon), "Relachement : la roue revient exactement au centre.");
            Assert.That(Mathf.Abs(angle), Is.LessThanOrEqualTo(90f), "Et jamais au-dela de sa consigne.");
            Assert.That(VehicleSteeringModel.MoveSteerAngleDegrees(12f, 0f, 0f, 0.02f), Is.EqualTo(12f),
                "Taux nul : l'angle ne bouge pas -- le profil refuse ce cas, la fonction ne divise pas pour autant.");
        }

        [Test]
        public void OnlyTheAuthoredSteeringWheelsTakeTheSteeringAngle()
        {
            Assert.That(VehicleSteeringModel.ResolveWheelSteerAngleDegrees(true, -28f), Is.EqualTo(-28f),
                "Roue authoree directrice : elle prend la consigne.");
            Assert.That(VehicleSteeringModel.ResolveWheelSteerAngleDegrees(false, -28f), Is.EqualTo(0f),
                "Roue non directrice : elle reste droite. Aucun braquage arriere n'est invente.");

            var profile = LoadDefaultProfile();
            var steeringWheels = 0;
            var drivenWheels = 0;
            for (var i = 0; i < profile.WheelCount; i++)
            {
                var wheel = profile.GetWheel(i);
                if (wheel.IsSteering)
                {
                    steeringWheels++;
                }

                if (wheel.IsDriven)
                {
                    drivenWheels++;
                }
            }

            Assert.That(steeringWheels, Is.EqualTo(2), "Un essieu directrice, deux roues.");
            Assert.That(drivenWheels, Is.EqualTo(4),
                "Quatre roues motrices : le couple est reparti sur les quatre, donc chaque pneu reste loin de sa limite "
                + "d'adherence -- c'est le patinage qui consommait le budget lateral et faisait deraper la voiture.");
        }

        // ---------------------------------------------------- la visee anticipee

        [Test]
        public void LookAheadPointStaysCollinearOnAStraightAndSlidesPastTheNodeWhenClose()
        {
            // Story 5.13 : la loi prend desormais une MEMOIRE de cible et un pas maximal de rappel.
            // Les deux sont NEUTRES ici (pas maximal nul) : c'est exactement la loi livree en 5.12,
            // que cette garde continue de verifier. La continuite du rappel elle-meme se prouve dans
            // la fixture 5.13 et sur les trajectoires rejouees du district (Story510).
            var far = LaneGraphRouting.ResolveLookAheadPoint(
                Vector3.zero, new Vector3(0f, 0f, 20f), Vector3.forward, 6f, Vector3.zero, 0f);
            Assert.That(far, Is.EqualTo(new Vector3(0f, 0f, 20f)),
                "Noeud plus loin que la distance de visee : la cible reste le noeud -- viser un point de la meme droite ne change pas le cap.");

            var close = LaneGraphRouting.ResolveLookAheadPoint(
                new Vector3(0f, 0f, 18f), new Vector3(0f, 0f, 20f), Vector3.forward, 6f, Vector3.zero, 0f);
            Assert.That(close, Is.EqualTo(new Vector3(0f, 0f, 24f)),
                "Noeud a moins de la distance de visee : la cible glisse AU-DELA du noeud, le long de son sens de circulation.");

            var turning = LaneGraphRouting.ResolveLookAheadPoint(
                new Vector3(0f, 0f, 18f), new Vector3(0f, 0f, 20f), Vector3.right, 6f, Vector3.zero, 0f);
            Assert.That(turning, Is.EqualTo(new Vector3(4f, 0f, 20f)),
                "Le point de visee suit le SENS DE CIRCULATION authore sur le noeud vise. A un carrefour, ce sens est la "
                + "direction traversante du noeud, pas la sortie du virage : celle-ci n'est tiree qu'a l'arrivee, et la deviner "
                + "maintenant reviendrait a decider deux fois (c'est dit et assume dans la documentation de la fonction).");
        }

        [Test]
        public void LookAheadPointHasNoDiscontinuityAtTheLookAheadDistanceAndNeverMutatesGeometry()
        {
            var waypoint = new Vector3(0f, 0f, 20f);
            const float LookAhead = 6f;

            var justOutside = LaneGraphRouting.ResolveLookAheadPoint(new Vector3(0f, 0f, 13.999f), waypoint, Vector3.forward, LookAhead, Vector3.zero, 0f);
            var justInside = LaneGraphRouting.ResolveLookAheadPoint(new Vector3(0f, 0f, 14.001f), waypoint, Vector3.forward, LookAhead, Vector3.zero, 0f);
            Assert.That(Vector3.Distance(justOutside, justInside), Is.LessThan(0.01f),
                "A la distance de visee exacte les deux formules se rejoignent : aucun basculement de cible, donc aucune a-coup de direction.");

            var unchanged = waypoint;
            LaneGraphRouting.ResolveLookAheadPoint(new Vector3(0f, 0f, 19f), waypoint, Vector3.forward, LookAhead, Vector3.zero, 0f);
            Assert.That(waypoint, Is.EqualTo(unchanged), "Lecture de geometrie : la position du noeud n'est jamais mutee.");

            Assert.That(LaneGraphRouting.ResolveLookAheadPoint(new Vector3(0f, 0f, 19f), waypoint, Vector3.forward, 0f, Vector3.zero, 0f),
                Is.EqualTo(waypoint), "Distance de visee nulle : repli sur le noeud.");
            Assert.That(LaneGraphRouting.ResolveLookAheadPoint(new Vector3(0f, 0f, 19f), waypoint, Vector3.zero, LookAhead, Vector3.zero, 0f),
                Is.EqualTo(waypoint), "Sens de circulation degenere : repli sur le noeud, jamais un point invente.");
        }

        [Test]
        public void TheAiAimsAtTheLookAheadPointAndTheArrivalDecisionStaysWhereItWasMade()
        {
            var source = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));

            Assert.That(source, Does.Contain("LaneGraphRouting.ResolveLookAheadPoint("),
                "La poursuite point-a-point est remplacee par un point de visee anticipe (Story 5.12).");
            Assert.That(source, Does.Contain("GetNodeRotation(waypointIndex)"),
                "Le sens de circulation vient du noeud lui-meme, jamais d'un successeur devine.");
            Assert.That(source, Does.Contain("ComputeSeekIntent(Vector3 position, Vector3 forward, Vector3 aimPoint, float steerFullLockDegrees)"),
                "La poursuite ne teste plus elle-meme l'arrivee : elle vise, la decision d'avancee de noeud reste au controleur.");
            Assert.That(source, Does.Contain("HasArrivedAtWaypoint("), "L'arrivee au noeud se decide toujours sur le rayon d'arrivee authore.");
        }

        // -------------------------------------------------- intent et couche physique

        [Test]
        public void TheHandbrakeTravelsTheExistingIntentPathAndOnlyOnTheNonSteeringWheels()
        {
            var intent = File.ReadAllText(IntentSourcePath);
            Assert.That(intent, Does.Contain("public float Handbrake"), "La voie de frein a main existe sur l'intent.");
            Assert.That(intent, Does.Contain("Handbrake = Mathf.Clamp01(handbrake)"), "Elle est bornee comme les autres voies.");

            var controller = CodeWithoutComments(File.ReadAllText(PlayerControllerSourcePath));
            Assert.That(controller, Does.Contain("intent.Handbrake"),
                "Le frein a main voyage sur le MEME chemin d'intent : aucune voie ni RPC dedie.");
            Assert.That(Occurrences(controller, "private void SubmitDriveIntentRpc("), Is.EqualTo(1),
                "Un seul RPC de conduite : un cinquieme axe ne justifie pas un second chemin de verite (NFR5).");
            Assert.That(controller, Does.Contain("private void SubmitDriveIntentRpc(float throttle, float steer, float brakeReverse, float handbrake, RpcParams rpcParams = default)"),
                "La voie de frein a main est portee par le RPC EXISTANT, dont le nom ne change pas.");
            Assert.That(controller, Does.Contain("keyboard.spaceKey"), "La touche est lue comme les quatre autres voies, sous LocalInputGate.");

            var body = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            Assert.That(body, Does.Contain("handbrakeEngaged && !wheel.IsSteering"),
                "Le frein a main agit sur les roues ARRIERE (non directrices) : ce sont elles qui se bloquent et font entrer en derive.");
            Assert.That(body, Does.Contain("current.HandbrakeTorque"), "Le couple de frein a main est authore, jamais un nombre en dur.");

            var profile = LoadDefaultProfile();
            Assert.That(profile.HandbrakeTorque, Is.GreaterThan(profile.BrakeTorque),
                "Le frein a main serre plus fort que le frein de service : c'est ce qui BLOQUE les roues arriere au lieu de simplement ralentir.");
        }

        [Test]
        public void ThePlayerDrivePathWritesNeitherVelocityNorRotation()
        {
            var controller = CodeWithoutComments(File.ReadAllText(PlayerControllerSourcePath));
            var driveStep = ExtractMethodBody(controller, "private void ApplyPhysics(VehicleDriveIntent intent)");

            Assert.That(driveStep, Does.Not.Contain("linearVelocity ="),
                "L'etape de conduite ne touche plus la vitesse : le longitudinal et le lateral viennent d'efforts aux roues (AD-35). "
                + "Elle la LIT (l'inversion marche arriere en depend), elle ne l'ecrit plus.");
            Assert.That(driveStep, Does.Not.Contain("MoveRotation"),
                "Le lacet n'est plus impose a la caisse : c'est un angle de roue suivi par les efforts lateraux des pneus.");
            Assert.That(controller, Does.Not.Contain("MoveRotation"),
                "Aucun lacet impose nulle part dans le controleur joueur.");
            Assert.That(Occurrences(controller, "linearVelocity ="), Is.EqualTo(1),
                "Il ne reste qu'UNE ecriture de vitesse dans tout le fichier, et c'est la remise a zero de la recuperation.");
            Assert.That(controller, Does.Contain("body.linearVelocity = Vector3.zero;"),
                "Cette ecriture unique est la recuperation (Story 3.4), pas le chemin de conduite.");
            Assert.That(driveStep, Does.Contain("SubmitIntentToPhysicsLayer("),
                "L'etape de conduite se termine par la soumission de l'intent : c'est LE chemin de deplacement du vehicule joueur.");
            Assert.That(controller, Does.Contain("physicsBody.ApplyDriveIntent("),
                "Et cette soumission passe par le point d'entree d'efforts de la couche physique, le meme que la 5.14 consommera.");
            Assert.That(controller, Does.Not.Contain("lateralGrip"),
                "Plus de borne de vitesse ni de grip lateral serialises ici : tout vient du profil authore.");
            Assert.That(controller, Does.Not.Contain("ResolveSteerDirectionMultiplier"),
                "La consigne de direction n'est plus inversee en marche arriere : le modele a effort produit ce sens par "
                + "la geometrie du pneu, donc une inversion d'entree la doublerait (retour de recette du 2026-09-18).");
            Assert.That(controller, Does.Contain("ResolveEffectiveMaxForwardSpeed()"),
                "Les degats de la Story 3.5 reduisent l'AUTORITE de conduite, et gardent leur contrat de source.");
        }

        [Test]
        public void ThePhysicsLayerStaysFreeOfDispositionAuthorityAndForeignWheelModels()
        {
            var body = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            var tire = CodeWithoutComments(File.ReadAllText(TireModelSourcePath));
            var steering = CodeWithoutComments(File.ReadAllText(SteeringModelSourcePath));

            Assert.That(body, Does.Not.Contain("WheelCollider"), "AD-35 : raycasts par roue, jamais WheelCollider.");
            Assert.That(tire, Does.Not.Contain("WheelCollider"));
            Assert.That(body, Does.Not.Contain("body.isKinematic = !IsServer"),
                "L'autorite est deja posee par les controleurs : un second test ici creerait un second chemin de verite (AD-21). "
                + "Le predicat de sol lit bien isKinematic, mais sur le Rigidbody d'un AUTRE corps, pour refuser une voiture comme sol.");
            Assert.That(body, Does.Not.Contain("NetworkedRageState"),
                "La couche physique ne lit ni rage ni peur ni disposition (AD-33). Le namespace 'RoadRage' n'est pas une lecture d'etat.");
            Assert.That(body, Does.Not.Contain("RageValue"));
            Assert.That(body, Does.Not.Contain("RageDisposition"));
            Assert.That(body, Does.Not.Contain("FearValue"));
            Assert.That(body, Does.Not.Contain("Random"), "Aucun tirage : la meme entree donne la meme sortie.");

            Assert.That(tire, Does.Not.Contain("MonoBehaviour"), "Le modele de pneu est un ensemble de fonctions pures, pas un composant.");
            Assert.That(tire, Does.Not.Contain("Time."), "Aucune dependance a l'horloge : le pas de temps est un argument.");
            Assert.That(tire, Does.Not.Contain("Rigidbody"), "Aucune dependance au Rigidbody : c'est ce qui le rend verifiable en EditMode.");
            Assert.That(steering, Does.Not.Contain("MonoBehaviour"));
            Assert.That(steering, Does.Not.Contain("Time."));
            Assert.That(steering, Does.Not.Contain("Rigidbody"));
        }

        // ------------------------------------------- Passation a la Story 5.14

        // La garde `TheTransientStateIsNamedAndTheStoryThatLiftsItIsNotThisOne` a vecu ici du
        // 2026-09-18 au 2026-09-19, et elle a fait exactement son travail : elle exigeait que le
        // controleur IA ecrive encore sa vitesse en bloc et impose son lacet, en documentant que c'est
        // l'etat intermediaire NOMME et que la Story 5.14 le leverait. La Story 5.14 l'a leve --
        // `ApplyMovement` est supprime, l'IA soumet un `VehicleDriveIntent` a la couche physique et
        // relit sa vitesse sur le `Rigidbody` -- donc la garde est devenue rouge le jour ou elle
        // devait le devenir, et son objet a disparu avec l'ecriture qu'elle protegeait.
        //
        // Elle est RETIREE, et non rearmee sur l'autre versant (decision humaine du 2026-09-19) :
        // l'invariant qu'elle portait -- aucune ecriture de vitesse, de position ni de rotation depuis
        // un chemin de conduite -- est desormais tenu par
        // `RoadRage.Tests.EditMode.Story514AiDrivesByIntentTests`, qui l'assere sur le PAS DE CONDUITE
        // de `FixedUpdate` au lieu du fichier entier. Deux copies de la meme assertion dans deux
        // fixtures de stories differentes divergeraient au premier changement.
        //
        // La lecon de cette garde est gardee ici parce qu'elle est reutilisable : rendre un etat
        // intermediaire VISIBLE et DATE est ce qui a permis de savoir, sans lire le code, que la 5.14
        // n'avait pas ete livree pendant trois stories, puis qu'elle l'etait.

        [Test]
        public void ThePhysicsBodySimulatesNothingWithoutAValidProfileAndFourWheelsWithOne()
        {
            var host = new GameObject("Story512PhysicsHost");
            try
            {
                var body = host.AddComponent<VehiclePhysicsBody>();
                Assert.That(body.HasProfile, Is.False, "Aucun profil assigne : la couche physique reste inerte.");
                Assert.That(body.WheelCount, Is.EqualTo(0));
                Assert.That(body.TryGetTireSample(0, out _), Is.False, "Sans profil, aucune roue a lire -- et aucune exception.");

                body.BindProfile(LoadDefaultProfileDef());

                Assert.That(body.HasProfile, Is.True, "Le profil par defaut est authore et valide.");
                Assert.That(body.WheelCount, Is.EqualTo(4));
                Assert.That(body.TryGetTireSample(0, out var sample), Is.True);
                Assert.That(sample.Grounded, Is.False, "Le tampon est dimensionne a l'application du profil, pas encore simule.");
                Assert.That(body.TryGetTireSample(4, out _), Is.False, "Index hors bornes : refus, jamais d'exception.");
                Assert.That(body.CurrentSteerAngleDegrees, Is.EqualTo(0f), "Roues droites au repos.");

                var rigidbody = host.GetComponent<Rigidbody>();
                Assert.That(rigidbody.mass, Is.EqualTo(LoadDefaultProfile().Mass).Within(0.001f), "La masse vient du profil authore.");
                Assert.That(rigidbody.automaticCenterOfMass, Is.False);
                Assert.That(rigidbody.automaticInertiaTensor, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void TheDefaultProfileAuthorsEveryTireSteeringAndDrivetrainParameter()
        {
            var profile = LoadDefaultProfile();

            Assert.That(profile.TirePeakSlipRatio, Is.GreaterThan(0f));
            Assert.That(profile.TirePeakSlipAngleDegrees, Is.GreaterThan(0f));
            Assert.That(profile.TireSlipFalloffFraction, Is.InRange(0f, 0.999f),
                "La chute d'adherence est progressive et partielle : jamais zero, jamais totale.");
            Assert.That(profile.MaxSteerAngleDegrees, Is.GreaterThan(profile.HighSpeedSteerAngleDegrees),
                "La direction se reduit avec la vitesse, elle ne s'ouvre pas.");
            Assert.That(profile.SteerFullReductionSpeed, Is.GreaterThan(0f));
            Assert.That(profile.SteerRateDegreesPerSecond, Is.GreaterThan(0f));
            Assert.That(profile.SteerReturnRateDegreesPerSecond, Is.GreaterThan(0f));
            Assert.That(profile.EngineTorque, Is.GreaterThan(0f));
            Assert.That(profile.ReverseTorque, Is.GreaterThan(0f));
            Assert.That(profile.BrakeTorque, Is.GreaterThan(0f));
            Assert.That(profile.CoastTorque, Is.GreaterThan(0f), "Le frein moteur retient un vehicule gare sans conducteur.");
            Assert.That(profile.HandbrakeTorque, Is.GreaterThan(0f));
            Assert.That(profile.WheelInertia, Is.GreaterThan(0f));
            Assert.That(profile.MinimumDirectionSpeed, Is.GreaterThan(0f));

            // Story 5.13 : les aides arcade s'ajoutent au meme point de reglage unique, et le profil
            // doit les porter sur SES DEUX copies (asset et valeurs par defaut du Def). Une aide
            // authoree dans une seule des deux divergerait en silence des le prochain prefab.
            Assert.That(profile.YawStabilityRate, Is.GreaterThan(0f), "L'abattement du lacet est authore (Story 5.13).");
            Assert.That(profile.TractionControlStrength, Is.InRange(0f, 0.999f),
                "Le controle de traction est authore et strictement sous 1 : l'attenuation ne peut pas annuler le couple.");
            Assert.That(profile.SpinRecoveryRate, Is.GreaterThan(0f), "La recuperation de tete-a-queue est authoree (Story 5.13).");
            Assert.That(profile.SpinDriftThresholdDegreesPerSecond, Is.GreaterThan(0f),
                "Le seuil de derive est authore : sous lui, le terme de recuperation ne se declenche pas.");

            var expectedDriveForce = profile.EngineTorque / profile.GetWheel(0).Radius;
            Assert.That(expectedDriveForce, Is.GreaterThan(0f),
                "Le couple moteur divise par le rayon de roue donne l'effort transmis au sol : c'est l'unite du modele.");
        }

        [Test]
        public void ProfileValidationRefusesTheTireSteeringAndDrivetrainParametersItCannotUse()
        {
            var def = ScriptableObject.CreateInstance<VehicleProfileDef>();
            try
            {
                var serialized = new SerializedObject(def);
                serialized.FindProperty("id").stringValue = "vehicle_test";
                var profileProperty = serialized.FindProperty("profile");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(def.TryValidate(out _), Is.True, "Profil par defaut valide : les refus ci-dessous viennent du champ modifie.");

                AssertRefused(def, serialized, profileProperty, "highSpeedSteerAngleDegrees", 41f, "HighSpeedSteerAngleDegrees",
                    "Un angle haut vitesse superieur a l'angle maximal ouvrirait la direction avec la vitesse.");
                AssertRefused(def, serialized, profileProperty, "steerRateDegreesPerSecond", 0f, "SteerRateDegreesPerSecond",
                    "Un taux de braquage nul : les roues ne braqueraient jamais.");
                AssertRefused(def, serialized, profileProperty, "steerReturnRateDegreesPerSecond", 0f, "SteerReturnRateDegreesPerSecond",
                    "Un taux de rappel nul : une roue braquee ne reviendrait jamais au centre.");
                AssertRefused(def, serialized, profileProperty, "tirePeakSlipAngleDegrees", 120f, "TirePeakSlipAngleDegrees",
                    "Au-dela de 90 degres, ce n'est plus un angle de glissement.");
                AssertRefused(def, serialized, profileProperty, "handbrakeTorque", 0f, "HandbrakeTorque",
                    "Un frein a main sans couple ne bloquerait pas les roues arriere, donc ne ferait pas deriver.");
                AssertRefused(def, serialized, profileProperty, "brakeTorque", 0f, "BrakeTorque",
                    "Un frein de service sans couple ne freinerait rien.");
                AssertRefused(def, serialized, profileProperty, "wheelInertia", 0f, "WheelInertia",
                    "Une inertie de roue nulle ferait une division par zero dans l'integration du spin.");
                AssertRefused(def, serialized, profileProperty, "maxReverseSpeed", 0f, "MaxReverseSpeed",
                    "Une pointe en marche arriere nulle rendrait la marche arriere inatteignable.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void TheDamageAuthorityIsProvenRatherThanSearched()
        {
            // Revue du 2026-09-18 : ces valeurs etaient gardees par des assertions de TEXTE, que deux
            // mutations laissaient vertes (neutraliser un multiplicateur de degats, supprimer l'intent
            // neutre pousse quand le conducteur est perdu). L'autorite de conduite est desormais une
            // fonction pure, donc mesuree. Le troisieme cas de cette revue -- l'inversion de direction en
            // marche arriere -- a lui ete SUPPRIME le meme jour : le modele a effort produisait deja ce
            // sens par la geometrie du pneu, donc la consigne inversee le doublait (retour de recette :
            // « en recule, les directions sont inversees »). Sa garde vit dans la Story 3.2.
            var profile = LoadDefaultProfile();

            NetworkedVehicleDriverController.ResolveDriveAuthority(
                profile, false, false, false, out var cleanSpeed, out var cleanSteer, out var cleanBrake);
            Assert.That(cleanSpeed, Is.EqualTo(profile.MaxForwardSpeed).Within(Epsilon));
            Assert.That(cleanSteer, Is.EqualTo(profile.SteerRateDegreesPerSecond).Within(Epsilon));
            Assert.That(cleanBrake, Is.EqualTo(profile.BrakeTorque).Within(Epsilon),
                "Vehicule intact : l'autorite est exactement celle du profil authore.");

            NetworkedVehicleDriverController.ResolveDriveAuthority(
                profile, true, true, true, out var damagedSpeed, out var damagedSteer, out var damagedBrake);
            Assert.That(damagedSpeed, Is.EqualTo(profile.MaxForwardSpeed * 0.55f).Within(0.001f),
                "Moteur endommage (Story 3.5) : la pointe est reduite -- et c'est bien la POINTE qui est reduite, pas une vitesse ecrite.");
            Assert.That(damagedSteer, Is.EqualTo(profile.SteerRateDegreesPerSecond * 0.5f).Within(0.001f),
                "Roue endommagee : le taux de braquage est reduit, donc la maniabilite.");
            Assert.That(damagedBrake, Is.EqualTo(profile.BrakeTorque * 0.45f).Within(0.001f),
                "Freins endommages : le couple de frein est reduit, donc la distance de freinage allongee.");
        }

        [Test]
        public void TheIdleIntentIsPushedWhenTheDriverIsLostOrUnclaimed()
        {
            var controller = CodeWithoutComments(File.ReadAllText(PlayerControllerSourcePath));
            var fixedUpdate = ExtractMethodBody(controller, "private void FixedUpdate()");

            Assert.That(Occurrences(fixedUpdate, "SubmitIntentToPhysicsLayer(VehicleDriveIntent.Idle)"), Is.EqualTo(2),
                "Les deux sorties d'arret (conducteur inoperant, siege non reclamme) doivent POUSSER l'intent neutre : sans cela, "
                + "un conducteur ejecte en plein gaz laisserait l'intention precedente appliquee par la couche physique.");
            Assert.That(fixedUpdate, Does.Contain("ApplyPhysics(latestIntent)"),
                "Et le chemin nominal soumet l'intent du pas.");
        }

        [Test]
        public void TheAiPrefabAuthorsTheLookAheadDurationAndTheProductionActuallyReadsIt()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AiPrefabPath);
            Assert.That(prefab, Is.Not.Null, AiPrefabPath + " attendu");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var controller = instance.GetComponent<NetworkedAIVehicleDriverController>();
                Assert.That(controller, Is.Not.Null, "Le controleur IA doit etre porte par le prefab.");

                var field = new SerializedObject(controller).FindProperty("lookAheadSeconds");
                Assert.That(field, Is.Not.Null, "La duree de visee doit etre authoree sur le prefab, jamais une constante de code.");
                Assert.That(field.floatValue, Is.GreaterThan(0f),
                    "Une duree de visee nulle ramenerait exactement la poursuite point-a-point que la 5.12 remplace, "
                    + "sans qu'aucune garde EditMode ne s'en apercoive -- c'est precisement ce que cette assertion empeche.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }

            var source = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));
            Assert.That(source, Does.Contain("* lookAheadSeconds"),
                "Et la production doit LIRE ce champ : une constante, ou un champ jamais consomme, rendrait la visee inerte.");
        }

        [Test]
        public void TheDrivetrainIsAllWheelDriveOnBothCopiesOfTheProfile()
        {
            var profile = LoadDefaultProfile();
            AssertDrivetrainIsAllWheelDrive(profile, "l'asset authore");

            var def = ScriptableObject.CreateInstance<VehicleProfileDef>();
            try
            {
                AssertDrivetrainIsAllWheelDrive(def.Profile, "les valeurs par defaut du Def");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(def);
            }
        }

        /// <summary>
        /// Recette humaine du 2026-09-18 : « beaucoup trop dur dans la conduite, ce n'est pas du tout
        /// arcade -- l'acceleration est super lente, tres difficile de tourner, le frein est super
        /// lent ». La cause etait le remplacement du modele precedant (vitesse ecrite, 28 m/s2
        /// d'acceleration et 42 m/s2 de freinage) par un modele a effort : l'enveloppe REELLE du profil
        /// author e tombait a 4,5 m/s2 en acceleration, 7,1 m/s2 au freinage et 5,9 m/s2 en virage.
        ///
        /// Cette garde fixe l'enveloppe calculee depuis le profil, pas un reglage au jugé : c'est le
        /// ressenti d'arcade rendu MESURABLE, pour qu'un futur reglage ne le fasse pas retomber en
        /// silence. Les bornes sont volontairement en dessous des valeurs livrees : elles disent
        /// « arcade », pas « exactement ces nombres-la ».
        /// </summary>
        [Test]
        public void TheAuthoredProfileKeepsAnArcadeEnvelope()
        {
            var profile = LoadDefaultProfile();
            var staticLoad = profile.Mass * Mathf.Abs(Physics.gravity.y) / profile.WheelCount;
            var gripPerWheel = profile.LateralFrictionCoefficient * staticLoad;

            var drivenWheels = 0;
            var brakeForce = 0f;
            for (var i = 0; i < profile.WheelCount; i++)
            {
                var wheel = profile.GetWheel(i);
                if (wheel.IsDriven)
                {
                    drivenWheels++;
                }

                brakeForce += Mathf.Min(profile.BrakeTorque / wheel.Radius, gripPerWheel);
            }

            var launchForce = drivenWheels * Mathf.Min(profile.EngineTorque / profile.GetWheel(0).Radius, gripPerWheel);
            var launchAcceleration = launchForce / profile.Mass;
            var brakingAcceleration = brakeForce / profile.Mass;
            var corneringAcceleration = (2f * gripPerWheel) / profile.Mass;

            // Recette du 2026-09-18 (2e retour) : « ca derape beaucoup trop, je veux un derapage tres tres
            // leger ». Le mecanisme du derapage etait le PATINAGE : un pneu qui glisse consomme son budget
            // d'adherence en longitudinal, donc il ne tient plus la caisse en travers. La garde qui encode ce
            // ressenti est donc une MARGE : l'effort demande a chaque roue motrice doit rester nettement sous
            // ce que son adherence peut transmettre.
            var driveForcePerWheel = profile.EngineTorque / profile.GetWheel(0).Radius;
            var gripUsageAtLaunch = driveForcePerWheel / gripPerWheel;
            Assert.That(gripUsageAtLaunch, Is.LessThanOrEqualTo(0.7f),
                "Depart sans patinage : chaque roue motrice utilise " + (gripUsageAtLaunch * 100f).ToString("F0")
                + " % de son adherence (" + driveForcePerWheel.ToString("F0") + " N pour " + gripPerWheel.ToString("F0")
                + " N disponibles). Au-dela, le pneu patine, perd son adherence laterale et la voiture derape : la borne est 70 %.");
            Assert.That(drivenWheels, Is.EqualTo(4),
                "Et l'effort est reparti sur quatre roues : c'est ce qui permet la marge ci-dessus sans sacrifier le depart.");

            Assert.That(launchAcceleration, Is.GreaterThanOrEqualTo(8f),
                "Acceleration d'arcade : l'enveloppe du profil donne " + launchAcceleration.ToString("F1")
                + " m/s2. La recette du 2026-09-18 a refuse 4,5 m/s2 ; la borne basse est 8.");

            Assert.That(brakingAcceleration, Is.GreaterThanOrEqualTo(10f),
                "Freinage d'arcade : l'enveloppe du profil donne " + brakingAcceleration.ToString("F1")
                + " m/s2. La recette a refuse 7,1 m/s2 ; la borne basse est 10.");

            Assert.That(corneringAcceleration, Is.GreaterThanOrEqualTo(9f),
                "Virage d'arcade : l'adherence par roue autorise " + corneringAcceleration.ToString("F1")
                + " m/s2 en appui sur l'essieu avant. La recette a refuse 5,9 m/s2 ; la borne basse est 9.");

            Assert.That(profile.MaxSteerAngleDegrees, Is.GreaterThanOrEqualTo(30f),
                "Braquage a basse vitesse : il faut du debattement pour tourner court, pas un rayon de camion.");
            Assert.That(profile.HighSpeedSteerAngleDegrees, Is.GreaterThanOrEqualTo(12f),
                "Braquage a vitesse de conduite : l'angle doit rester utilisable, sinon la voiture ne tourne qu'a l'arret.");
            Assert.That(profile.SteerRateDegreesPerSecond, Is.GreaterThanOrEqualTo(240f),
                "Vitesse de braquage : une roue qui met une seconde a atteindre son angle ne se sent pas.");
        }

        /// <summary>
        /// Le montage du train roulant doit etre VISIBLE : quatre roues motrices pour la marge d'adherence
        /// (recette du 2026-09-18 : « ca derape beaucoup trop »), et un essieu non directrice pour que le
        /// frein a main ait des roues a bloquer. Sans cette garde, revenir a un montage deux roues motrices
        /// ne ferait echouer aucun autre test tout en ramenant le patinage -- et le derapage.
        /// </summary>
        private static void AssertDrivetrainIsAllWheelDrive(VehicleProfile profile, string source)
        {
            Assert.That(profile.GetWheel(0).IsSteering, Is.True, source + " : l'essieu avant est directrice.");
            Assert.That(profile.GetWheel(0).IsDriven, Is.True, source + " : et il est moteur -- quatre roues motrices.");
            Assert.That(profile.GetWheel(2).IsSteering, Is.False, source + " : l'essieu arriere ne braque pas.");
            Assert.That(profile.GetWheel(2).IsDriven, Is.True,
                source + " : et il est moteur aussi, donc le frein a main bloque des roues motrices.");
        }

        // ------------------------------------------------------------------ helpers

        private static void AssertRefused(
            VehicleProfileDef def,
            SerializedObject serialized,
            SerializedProperty profileProperty,
            string fieldName,
            float value,
            string expectedInError,
            string because)
        {
            var property = profileProperty.FindPropertyRelative(fieldName);
            Assert.That(property, Is.Not.Null, fieldName + " attendu dans le profil");
            var before = property.floatValue;

            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(property.floatValue, Is.EqualTo(value).Within(Epsilon),
                fieldName + " : la valeur posee doit etre refusee par la validation, pas corrigee en silence.");

            var valid = def.TryValidate(out var error);
            Assert.That(valid, Is.False, fieldName + " = " + value + " : " + because);
            Assert.That(error, Does.Contain(expectedInError), "Le refus nomme le champ fautif (" + fieldName + ").");

            property.floatValue = before;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Corps d'une methode, sans ses accolades englobantes : sert aux gardes de forme sur le code.</summary>
        private static string ExtractMethodBody(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), signature + " attendu dans le source");

            var open = source.IndexOf('{', start);
            Assert.That(open, Is.GreaterThan(start), signature + " : accolade ouvrante attendue");

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
                        return source.Substring(open, (i - open) + 1);
                    }
                }
            }

            Assert.Fail(signature + " : accolades non equilibrees");
            return string.Empty;
        }

        private static VehicleProfile LoadDefaultProfile()
        {
            return LoadDefaultProfileDef().Profile;
        }

        private static VehicleProfileDef LoadDefaultProfileDef()
        {
            var def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);
            Assert.That(def, Is.Not.Null, ProfilePath + " attendu");
            return def;
        }

        /// <summary>Retire les commentaires d'un source avant de le fouiller : le code DOCUMENTE ce qu'il ne fait pas.</summary>
        private static string CodeWithoutComments(string source)
        {
            var lines = source.Split('\n');
            var kept = new List<string>(lines.Length);
            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                var comment = line.IndexOf("//", StringComparison.Ordinal);
                kept.Add(comment >= 0 ? line.Substring(0, comment) : line);
            }

            return string.Join("\n", kept);
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
