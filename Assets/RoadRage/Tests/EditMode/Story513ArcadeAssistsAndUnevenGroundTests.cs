using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.13 : les aides arcade et le sol irregulier.
    ///
    /// Cinq familles de gardes, toutes sans scene de jeu ni Netcode :
    /// 1. les AIDES en fonctions pures (<see cref="VehicleArcadeAssist"/>) : inertes a leur valeur
    ///    authoree nulle, bornees par le budget de charge porte, et jamais capables de neutraliser
    ///    l'entree conducteur ;
    /// 2. l'AUTORITE par roues au sol : une proportion, jamais un reglage, et un facteur neutre plutot
    ///    qu'un NaN quand le profil ne porte aucune roue ;
    /// 3. la CONTINUITE du point de visee au franchissement de noeud, mesuree sur les deux lois -- la
    ///    rendue et l'ideale -- pour que l'assertion ne soit pas vide (la mesure de district, elle,
    ///    vit dans `Story510LaneGraphAndRoutedTrafficTests`) ;
    /// 4. l'AUTHORING : chaque aide est authoree individuellement, le refus de validation nomme le
    ///    champ fautif, et aucune valeur de repli silencieuse n'existe ;
    /// 5. le RELIEF DE RECETTE author e dans `MVP_Run` : deux objets de scene sur l'avenue, sous la
    ///    hauteur de bordure authoree, hors de l'emprise de tout `LaneNode`, et absents des prefabs de
    ///    module -- plus les invariants de code que rien d'autre ne garde.
    ///
    /// La matrice I/O du spec est couverte ligne par ligne : adhesion saturee, virage rapide a lacet
    /// libre, tete-a-queue, relief authore de 0,12 m, roues en l'air, autorite degradee, profil
    /// incomplet. L'absolu runtime (montee reelle de la bordure et du relief) est tenu par la recette
    /// humaine et par le banc PlayMode de la story : la porte de cet agent ne peut pas le produire.
    /// </summary>
    public sealed class Story513ArcadeAssistsAndUnevenGroundTests
    {
        private const string ProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";
        private const string SegmentPrefabPath = "Assets/RoadRage/Prefabs/Greybox_RoadSegment_TwoWay.prefab";

        private const string ArcadeAssistSourcePath = "Assets/RoadRage/Features/Vehicles/VehicleArcadeAssist.cs";
        private const string PhysicsBodySourcePath = "Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs";
        private const string SuspensionModelSourcePath = "Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs";
        private const string TireModelSourcePath = "Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs";
        private const string SteeringModelSourcePath = "Assets/RoadRage/Features/Vehicles/VehicleSteeringModel.cs";
        private const string RoutingSourcePath = "Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs";
        private const string PlayerControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string AiControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";

        /// <summary>
        /// Les fichiers de la couche physique. Meme liste que la Story 5.11, avec le fichier neuf des
        /// aides : c'est elle qui interdit un <c>WheelCollider</c> (AD-35) dans la couche.
        /// </summary>
        private static readonly string[] PhysicsLayerSourcePaths =
        {
            PhysicsBodySourcePath,
            SuspensionModelSourcePath,
            TireModelSourcePath,
            SteeringModelSourcePath,
            ArcadeAssistSourcePath,
            "Assets/RoadRage/Features/Vehicles/VehicleProfile.cs",
            "Assets/RoadRage/Features/Vehicles/VehicleWheel.cs",
            PlayerControllerSourcePath,
            AiControllerSourcePath
        };

        /// <summary>Nom de l'avenue qui porte le relief de recette : c'est la ou la recette se fait.</summary>
        private const string AvenueName = "Avenue_CenterToEast";

        /// <summary>Nom des deux objets de scene du relief. Le prefixe les distingue du decor du district.</summary>
        private const string BumpObjectName = "Relief_DosDane_AvenueCenterToEast";
        private const string StepObjectName = "Relief_MarcheBasse_AvenueCenterToEast";

        /// <summary>
        /// Pente moyenne maximale admise pour le dos-d'ane, en fraction. « Pente douce » n'est pas un
        /// adjectif : c'est une borne, et c'est elle qui empeche de remplacer la rampe par une marche.
        /// </summary>
        private const float MaximumBumpSlope = 0.15f;

        /// <summary>Largeur de la chaussee d'un module d'avenue (cotes figees de la Story 5.10).</summary>
        private const float CarriagewayWidth = 8f;

        /// <summary>Charge statique d'une roue du vehicule de reference (masse 1200 kg, 4 roues).</summary>
        private const float StaticLoad = 1200f * 9.81f / 4f;

        /// <summary>Demi-voie moyenne authoree du vehicule de reference : les roues sont a |x| = 0,85 m.</summary>
        private const float HalfTrack = 0.85f;

        private const float Epsilon = 0.0001f;

        // ---------------------------------------------------- autorite par roues au sol

        /// <summary>
        /// Ligne « autorite degradee » de la matrice : 1 a 3 roues au sol sur 4. Le facteur est une
        /// PROPORTION, pas un reglage -- un facteur authore aurait rendu la proportion fausse des que
        /// le nombre de roues change.
        /// </summary>
        [Test]
        public void TheAuthorityIsTheProportionOfWheelsStillOnTheGround()
        {
            const int Wheels = 4;

            Assert.That(VehicleArcadeAssist.ResolveGroundedAuthorityFactor(Wheels, Wheels), Is.EqualTo(1f),
                "Quatre roues sur quatre : l'autorite est entiere.");
            Assert.That(VehicleArcadeAssist.ResolveGroundedAuthorityFactor(Wheels - 1, Wheels), Is.EqualTo(3f / Wheels).Within(Epsilon),
                "Trois roues : l'autorite est la proportion du nombre de roues au sol.");
            Assert.That(VehicleArcadeAssist.ResolveGroundedAuthorityFactor(Wheels - 2, Wheels), Is.EqualTo(2f / Wheels).Within(Epsilon));
            Assert.That(VehicleArcadeAssist.ResolveGroundedAuthorityFactor(1, Wheels), Is.EqualTo(1f / Wheels).Within(Epsilon));

            Assert.That(VehicleArcadeAssist.ResolveGroundedAuthorityFactor(0, Wheels), Is.EqualTo(0f),
                "Ligne « roues en l'air » : aucune roue au sol reduit l'autorite -- de conduite ET de direction -- a zero. "
                + "C'est une reduction de ce que la couche PRODUIT, jamais une vitesse ecrite ni une rotation gelee.");

            var previous = -1f;
            for (var grounded = 0; grounded <= Wheels; grounded++)
            {
                var factor = VehicleArcadeAssist.ResolveGroundedAuthorityFactor(grounded, Wheels);
                Assert.That(factor, Is.GreaterThan(previous), "L'autorite croit strictement avec le nombre de roues au sol.");
                Assert.That(factor, Is.InRange(0f, 1f), "Et elle reste une proportion : jamais au-dela de 1.");
                previous = factor;
            }
        }

        /// <summary>
        /// Ligne « autorite degradee », cas degenere : <c>wheelCount &lt;= 0</c>. Le facteur est alors
        /// NEUTRE (1) plutot que NaN : un vehicule sans roue authoree n'a aucune autorite a perdre, et
        /// le profil refuse deja ce cas.
        /// </summary>
        [Test]
        public void TheAuthorityFactorIsNeutralRatherThanNaNWhenNoWheelIsAuthored()
        {
            Assert.That(VehicleArcadeAssist.ResolveGroundedAuthorityFactor(0, 0), Is.EqualTo(1f),
                "Aucune roue authoree : facteur neutre, jamais une division par zero.");
            Assert.That(VehicleArcadeAssist.ResolveGroundedAuthorityFactor(3, -4), Is.EqualTo(1f));
            Assert.That(VehicleArcadeAssist.ResolveGroundedAuthorityFactor(-2, 0), Is.EqualTo(1f));

            for (var wheelCount = -3; wheelCount <= 8; wheelCount++)
            {
                for (var grounded = -3; grounded <= 8; grounded++)
                {
                    var factor = VehicleArcadeAssist.ResolveGroundedAuthorityFactor(grounded, wheelCount);
                    Assert.That(float.IsNaN(factor), Is.False,
                        "Aucun couple d'entrees ne doit produire NaN (roues=" + grounded + ", total=" + wheelCount + ").");
                    Assert.That(float.IsFinite(factor), Is.True);
                    Assert.That(factor, Is.InRange(0f, 1f));
                }
            }
        }

        /// <summary>
        /// Le facteur publie par la couche decrit l'etat PRESENT : dimensionne a l'application du
        /// profil, remis a zero quand plus aucun profil valide n'est applique. C'est ce que lit la
        /// telemetrie, et ce n'est jamais un second calcul.
        /// </summary>
        [Test]
        public void ThePublishedGroundStateFollowsTheProfileAndNeverSurvivesItsRemoval()
        {
            var host = new GameObject("Story513AuthorityHost");
            try
            {
                var body = host.AddComponent<VehiclePhysicsBody>();

                Assert.That(body.HasProfile, Is.False);
                Assert.That(body.GroundedWheelCount, Is.EqualTo(0), "Sans profil, aucune roue n'est publiee.");
                Assert.That(body.GroundedAuthorityFactor, Is.EqualTo(0f), "Et aucune autorite n'est publiee.");

                body.BindProfile(LoadDefaultProfileDef());

                Assert.That(body.HasProfile, Is.True);
                Assert.That(body.GroundedWheelCount, Is.EqualTo(body.WheelCount),
                    "Aucun pas encore simule : le vehicule est suppose pose sur ses roues, donc aucune degradation d'autorite "
                    + "avant la premiere mesure.");
                Assert.That(body.GroundedAuthorityFactor, Is.EqualTo(1f));

                // Le retrait du profil efface l'etat publie : un BindProfile refuse ne doit pas laisser
                // derriere lui l'autorite de l'ancien vehicule.
                body.BindProfile(null);

                Assert.That(body.HasProfile, Is.False);
                Assert.That(body.GroundedWheelCount, Is.EqualTo(0));
                Assert.That(body.GroundedAuthorityFactor, Is.EqualTo(0f),
                    "L'etat publie decrit le present, jamais un souvenir : c'est la seule lecture qui ne mente pas en telemetrie.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        // ---------------------------------------------------- budget de lacet

        /// <summary>
        /// Le budget de lacet est DERIVE du vehicule -- adherence, charge portee par une roue, demi-voie
        /// -- et jamais un nombre absolu. C'est la borne de toutes les aides de lacet de la story.
        /// </summary>
        [Test]
        public void TheYawBudgetIsDerivedFromTheVehicleAndInertWithoutIt()
        {
            var adherence = 3f;
            var budget = VehicleArcadeAssist.ResolveYawTorqueBudget(adherence, StaticLoad, HalfTrack);

            Assert.That(budget, Is.EqualTo(adherence * StaticLoad * HalfTrack).Within(1f),
                "Le budget est le couple qu'UN SEUL pneu peut produire autour de la verticale a sa pleine adherence.");

            var doubledLoad = VehicleArcadeAssist.ResolveYawTorqueBudget(adherence, StaticLoad * 2f, HalfTrack);
            Assert.That(doubledLoad, Is.EqualTo(budget * 2f).Within(1f),
                "Il suit la charge du vehicule : une caisse deux fois plus lourde a deux fois plus de lacet a tenir.");

            Assert.That(budget, Is.GreaterThan(0f), "Le vehicule de reference doit avoir un budget mesurable, sinon les gardes qui suivent seraient vides.");

            Assert.That(VehicleArcadeAssist.ResolveYawTorqueBudget(0f, StaticLoad, HalfTrack), Is.EqualTo(0f),
                "Adherence nulle : aucun budget, donc les termes qu'il borne sont INERTES -- jamais non bornes.");
            Assert.That(VehicleArcadeAssist.ResolveYawTorqueBudget(adherence, 0f, HalfTrack), Is.EqualTo(0f));
            Assert.That(VehicleArcadeAssist.ResolveYawTorqueBudget(adherence, StaticLoad, 0f), Is.EqualTo(0f));
            Assert.That(VehicleArcadeAssist.ResolveYawTorqueBudget(float.NaN, StaticLoad, HalfTrack), Is.EqualTo(0f));
            Assert.That(VehicleArcadeAssist.ResolveYawTorqueBudget(adherence, float.PositiveInfinity, HalfTrack), Is.EqualTo(0f));

            var profile = LoadDefaultProfile();
            Assert.That(profile.ResolveMeanHalfTrack(), Is.GreaterThan(0f),
                "La demi-voie moyenne vient des roues authorees : c'est elle que la couche passe au budget, jamais une constante.");
        }

        // ---------------------------------------------------- stabilite en lacet

        /// <summary>
        /// Ligne « virage rapide, lacet libre » : le terme amortit la composante de lacet en s'opposant
        /// a elle, proportionnellement a son authoree, et il est EXACTEMENT nul quand la caisse ne
        /// tourne pas -- donc il ne peut pas tenir un cap contre le conducteur.
        /// </summary>
        [Test]
        public void TheYawStabilityOpposesTheRotationProportionallyAndNeverHoldsAHeading()
        {
            // Le budget doit couvrir la PLAGE DE TRAVAIL du test, sinon toutes les assertions de
            // proportionnalite comparent deux valeurs deja plafonnees et ne mesurent plus rien :
            // 60 x 80 = 4 800 doit rester sous le budget, et le plafonnement lui-meme est verifie plus bas.
            var rate = 60f;
            var budget = 6000f;

            Assert.That(VehicleArcadeAssist.ResolveYawStabilityTorque(0f, rate, budget), Is.EqualTo(0f),
                "Caisse qui ne tourne pas : aucun couple. C'est ce zero exact qui interdit a l'aide de tenir un cap a la place "
                + "du conducteur -- elle n'a aucune memoire d'un cap vise.");

            var turningLeft = VehicleArcadeAssist.ResolveYawStabilityTorque(40f, rate, budget);
            var turningRight = VehicleArcadeAssist.ResolveYawStabilityTorque(-40f, rate, budget);

            Assert.That(turningLeft, Is.LessThan(0f), "Lacet positif : le couple s'y oppose.");
            Assert.That(turningRight, Is.GreaterThan(0f), "Lacet negatif : sens inverse.");
            Assert.That(turningRight, Is.EqualTo(-turningLeft).Within(Epsilon), "Le terme est impair en taux de lacet.");

            var doubledRate = VehicleArcadeAssist.ResolveYawStabilityTorque(80f, rate, budget);
            Assert.That(doubledRate, Is.EqualTo(turningLeft * 2f).Within(0.01f), "Le terme est proportionnel au taux authore.");

            var halvedAuthoring = VehicleArcadeAssist.ResolveYawStabilityTorque(40f, rate / 2f, budget);
            Assert.That(halvedAuthoring, Is.EqualTo(turningLeft / 2f).Within(0.01f),
                "Et proportionnel a l'authoree : diviser le reglage par deux divise le couple par deux.");

            var saturated = VehicleArcadeAssist.ResolveYawStabilityTorque(100000f, rate, budget);
            Assert.That(Mathf.Abs(saturated), Is.EqualTo(budget).Within(0.01f),
                "Au-dela du budget, le terme est PLAFONNE : une aide de lacet qui depasserait ce couple ferait tourner la caisse "
                + "plus fort qu'un pneu ne peut la retenir, donc ce ne serait plus une aide mais un mouvement impose.");
            Assert.That(Mathf.Abs(VehicleArcadeAssist.ResolveYawStabilityTorque(1e9f, 1e9f, budget)), Is.LessThanOrEqualTo(budget + 0.01f));
        }

        /// <summary>
        /// Chaque aide est desactivable par sa SEULE valeur authoree nulle, sur le patron deja en place
        /// dans la couche (<c>rate &lt;= 0 -&gt; Vector3.zero</c>). Aucune valeur de repli inventee.
        /// </summary>
        [Test]
        public void EveryAssistIsInertAtItsAuthoredZeroValue()
        {
            var budget = 2000f;

            Assert.That(VehicleArcadeAssist.ResolveYawStabilityTorque(120f, 0f, budget), Is.EqualTo(0f),
                "Abattement de lacet nul : lacet totalement rendu a la geometrie du pneu, comportement d'avant cette story.");
            Assert.That(VehicleArcadeAssist.ResolveYawStabilityTorque(120f, -1f, budget), Is.EqualTo(0f),
                "Une authoree negative ne doit pas INVERSER l'aide, elle doit l'eteindre.");

            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(1.4f, 0.14f, 0f), Is.EqualTo(1f),
                "Controle de traction nul : le couple moteur est transmis tel quel, comportement d'avant cette story.");
            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(1.4f, 0.14f, -0.5f), Is.EqualTo(1f));

            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(400f, 1f, 30f, 0f, budget), Is.EqualTo(0f),
                "Recuperation de tete-a-queue nulle : aucun couple, le conducteur garde seul la main.");
            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(400f, 1f, 30f, -5f, budget), Is.EqualTo(0f));

            // Et l'inhiber n'est pas la seule facon de l'eteindre : une entree nulle la neutralise aussi.
            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(400f, 0f, 30f, 40f, budget), Is.EqualTo(0f),
                "Entree de direction nulle : le couple de recuperation est nul, donc il ne REMPLACE jamais l'entree.");
        }

        /// <summary>
        /// Une seule valeur finie ne doit jamais sortir d'un terme d'aide, quel que soit le couple
        /// d'entrees -- la matrice demande explicitement « aucune division par zero ».
        /// </summary>
        [Test]
        public void NoAssistEverProducesANonFiniteValue()
        {
            var inputs = new[] { 0f, -1f, 1e-6f, 1f, -1f, 1e6f, float.NaN, float.PositiveInfinity, float.NegativeInfinity };

            foreach (var a in inputs)
            {
                foreach (var b in inputs)
                {
                    Assert.That(float.IsFinite(VehicleArcadeAssist.ResolveYawStabilityTorque(a, b, StaticLoad)), Is.True);
                    Assert.That(float.IsFinite(VehicleArcadeAssist.ResolveYawStabilityTorque(a, 60f, b)), Is.True);
                    Assert.That(float.IsFinite(VehicleArcadeAssist.ResolveTractionControlFactor(a, b, 0.5f)), Is.True);
                    Assert.That(float.IsFinite(VehicleArcadeAssist.ResolveTractionControlFactor(a, 0.14f, b)), Is.True);
                    Assert.That(float.IsFinite(VehicleArcadeAssist.ResolveSpinRecoveryTorque(a, b, 30f, 40f, 2000f)), Is.True);
                    Assert.That(float.IsFinite(VehicleArcadeAssist.ResolveSpinRecoveryTorque(a, 1f, b, 40f, 2000f)), Is.True);
                    Assert.That(float.IsFinite(VehicleArcadeAssist.ResolveSpinRecoveryTorque(a, 1f, 30f, b, 2000f)), Is.True);
                    Assert.That(float.IsFinite(VehicleArcadeAssist.ResolveYawTorqueBudget(a, b, 0.85f)), Is.True);
                }
            }
        }

        // ---------------------------------------------------- controle de traction

        /// <summary>
        /// Ligne « adhesion saturee » : <c>Throttle = 1</c>, glissement au-dela du pic authore. Le terme
        /// attenue le couple moteur de la roue SANS L'ANNULER, et il est exactement neutre sous le pic.
        /// </summary>
        [Test]
        public void TractionControlIsNeutralBelowThePeakAndNeverCancelsTheTorque()
        {
            const float PeakSlip = 0.14f;
            const float Strength = 0.5f;

            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(0f, PeakSlip, Strength), Is.EqualTo(1f),
                "Glissement nul : aucune attenuation.");
            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(PeakSlip / 2f, PeakSlip, Strength), Is.EqualTo(1f),
                "Sous le pic, le pneu transmet ce qu'on lui demande : l'aide n'a rien a corriger. C'est une attenuation NULLE, "
                + "pas une attenuation faible.");
            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(PeakSlip, PeakSlip, Strength), Is.EqualTo(1f),
                "Au pic exactement, l'adherence n'est pas encore saturee.");
            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(-PeakSlip * 0.99f, PeakSlip, Strength), Is.EqualTo(1f),
                "Le glissement est lu en valeur absolue : freiner fort n'attenue pas le couple moteur d'une roue qui ne patine pas.");

            var overPeak = VehicleArcadeAssist.ResolveTractionControlFactor(PeakSlip * 2f, PeakSlip, Strength);
            Assert.That(overPeak, Is.LessThan(1f), "Au-dela du pic, l'attenuation s'engage.");
            Assert.That(overPeak, Is.GreaterThan(0f),
                "Mais elle ne peut JAMAIS annuler le couple moteur : a zero, ce serait un seuil binaire deguise, exactement ce "
                + "que la couche interdit.");

            var previous = 1f;
            for (var factor = 1.0f; factor <= 20f; factor += 0.05f)
            {
                var attenuation = VehicleArcadeAssist.ResolveTractionControlFactor(PeakSlip * factor, PeakSlip, Strength);
                Assert.That(attenuation, Is.LessThanOrEqualTo(previous), "L'attenuation est monotone en glissement.");
                Assert.That(attenuation, Is.GreaterThanOrEqualTo(Strength / 2f),
                    "Et elle est BORNEE : elle tend vers 1 - force sans jamais l'atteindre, donc jamais une annulation.");
                previous = attenuation;
            }
        }

        /// <summary>
        /// Le facteur est un MULTIPLICATEUR dans <c>]0, 1]</c>, et il ne depend que du glissement : c'est
        /// ce qui en fait un terme applicable au couple moteur d'une roue, jamais une force de pneu.
        /// </summary>
        [Test]
        public void TractionControlStaysAMultiplicativeFactorAndClampsItsAuthoredStrength()
        {
            const float PeakSlip = 0.14f;

            // Une authoree aberrante ne doit pas pouvoir annuler le couple : le plafond de la fonction
            // pure est le garde-fou, alors que la validation du profil refuse deja la valeur.
            var atFullStrength = VehicleArcadeAssist.ResolveTractionControlFactor(PeakSlip * 1e6f, PeakSlip, 1f);
            Assert.That(atFullStrength, Is.GreaterThan(0f),
                "Meme a une force authoree de 1 (que le profil refuse), le facteur reste strictement positif : la part retiree "
                + "est bornee par construction, ce qui interdit une annulation du couple.");
            Assert.That(atFullStrength, Is.LessThan(0.05f));

            var infinite = VehicleArcadeAssist.ResolveTractionControlFactor(PeakSlip, PeakSlip, float.PositiveInfinity);
            Assert.That(infinite, Is.EqualTo(1f), "Une authoree non finie rend le terme INERTE, jamais un facteur invente.");

            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(float.NaN, PeakSlip, 0.5f), Is.EqualTo(1f),
                "Un glissement non fini rend le facteur neutre : l'atténuation d'un glissement inconnu n'existe pas.");
            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(0.5f, 0f, 0.5f), Is.EqualTo(1f),
                "Un pic nul rend le facteur neutre : c'est la seule lecture qui ne divise pas par zero.");
            Assert.That(VehicleArcadeAssist.ResolveTractionControlFactor(0.5f, float.NaN, 0.5f), Is.EqualTo(1f));
        }

        // ---------------------------------------------------- recuperation de tete-a-queue

        /// <summary>
        /// Ligne « tete-a-queue » : vehicule en rotation, conducteur braquant a contre. Le couple de
        /// recuperation est borne, il ramene le cap vers l'entree conducteur, il ne remplace jamais
        /// cette entree, et il ne se declenche pas sous le seuil de derive authore.
        /// </summary>
        [Test]
        public void SpinRecoveryNeedsBothADriftAndADriverDemand()
        {
            // Meme raison que pour la stabilite en lacet : la plage de travail (40 x 170 = 6 800 au
            // plus fort) doit rester sous le budget, sinon « plus la derive est grande, plus le rappel
            // est fort » compare deux couples identiquement plafonnes. Le plafonnement est verifie par
            // `SpinRecoveryIsBoundedByTheVehicleBudgetAndReadsOnlyAuthoredInputs`.
            const float Threshold = 30f;
            const float Rate = 40f;
            const float Budget = 10000f;

            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(Threshold, 1f, Threshold, Rate, Budget), Is.EqualTo(0f),
                "A la derive exacte du seuil, le terme ne s'engage pas : une caisse qui tourne normalement dans un virage ne doit "
                + "rien sentir.");
            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(Threshold - 0.001f, 1f, Threshold, Rate, Budget), Is.EqualTo(0f),
                "Sous le seuil : aucun couple, sans exception.");
            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(-Threshold + 1f, 1f, Threshold, Rate, Budget), Is.EqualTo(0f),
                "Le seuil est lu en valeur absolue : une rotation en sens inverse non plus ne decline pas la caisse.");

            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(400f, 0f, Threshold, Rate, Budget), Is.EqualTo(0f),
                "Aucune entree conducteur : aucun couple. L'aide ne se substitue pas au conducteur et ne cree pas un pilote "
                + "automatique dans la couche physique.");

            var recoveringLeft = VehicleArcadeAssist.ResolveSpinRecoveryTorque(90f, 1f, Threshold, Rate, Budget);
            var recoveringRight = VehicleArcadeAssist.ResolveSpinRecoveryTorque(90f, -1f, Threshold, Rate, Budget);

            Assert.That(recoveringLeft, Is.GreaterThan(0f), "Le couple suit le SENS DEMANDE par le conducteur, pas le sens de la rotation.");
            Assert.That(recoveringRight, Is.EqualTo(-recoveringLeft).Within(Epsilon));
            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(-90f, 1f, Threshold, Rate, Budget), Is.EqualTo(recoveringLeft).Within(Epsilon),
                "Le couple ne depend du taux de lacet que par son EXCES de derive, jamais par son signe : c'est l'entree qui decide du cote.");

            var largerDrift = VehicleArcadeAssist.ResolveSpinRecoveryTorque(200f, 1f, Threshold, Rate, Budget);
            Assert.That(largerDrift, Is.GreaterThan(recoveringLeft), "Plus la derive depasse le seuil, plus le rappel est fort.");
        }

        /// <summary>
        /// Le couple de recuperation est borne par le meme budget que la stabilite en lacet, et son
        /// entre conducteur est bornee comme partout ailleurs.
        /// </summary>
        [Test]
        public void SpinRecoveryIsBoundedByTheVehicleBudgetAndReadsOnlyAuthoredInputs()
        {
            const float Threshold = 30f;
            const float Rate = 400f;
            const float Budget = 500f;

            var saturated = VehicleArcadeAssist.ResolveSpinRecoveryTorque(100000f, 1f, Threshold, Rate, Budget);
            Assert.That(Mathf.Abs(saturated), Is.EqualTo(Budget).Within(0.01f),
                "Le couple de recuperation ne depasse jamais le couple qu'un pneu peut produire : au-dela, ce serait une rotation "
                + "imposee, pas une aide.");

            var budgetless = VehicleArcadeAssist.ResolveSpinRecoveryTorque(100000f, 1f, Threshold, Rate, 0f);
            Assert.That(budgetless, Is.EqualTo(0f),
                "Sans budget mesurable, le terme est INERTE plutot que non borne -- le sens sur du garde-fou est toujours celui-la.");

            // L'entree conducteur est bornee a [-1, 1] : une manette qui depasse ne doit pas multiplier
            // le couple de recuperation au-dela de ce que le budget autorise.
            var outOfRangeSteer = VehicleArcadeAssist.ResolveSpinRecoveryTorque(90f, 25f, Threshold, Rate, Budget);
            var saturatedSteer = VehicleArcadeAssist.ResolveSpinRecoveryTorque(90f, 1f, Threshold, Rate, Budget);
            Assert.That(outOfRangeSteer, Is.EqualTo(saturatedSteer).Within(Epsilon),
                "Une entree hors bornes est ramenee dans [-1, 1] : elle ne donne pas un couple vingt-cinq fois plus fort.");

            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(90f, float.NaN, Threshold, Rate, Budget), Is.EqualTo(0f));
            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(90f, 1f, float.NaN, Rate, Budget), Is.EqualTo(0f));
            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(float.NaN, 1f, Threshold, Rate, Budget), Is.EqualTo(0f));
            Assert.That(VehicleArcadeAssist.ResolveSpinRecoveryTorque(90f, 1f, -50f, Rate, Budget), Is.GreaterThan(0f),
                "Un seuil negatif est ramene a zero plutot que d'inverser le declenchement.");
        }

        // ---------------------------------------------------- continuite du point de visee

        /// <summary>
        /// Ligne « continuite de la visee » : au franchissement de noeud, la cible IDEALE saute d'une
        /// branche a l'autre -- jusqu'a deux fois la distance de visee -- et c'est la cible RENDUE qui
        /// doit rester continue. Les deux lois sont mesurees ici, cote a cote, pour que l'assertion ne
        /// soit pas vide. La mesure sur les trajectoires reelles du district vit dans la Story 5.10.
        /// </summary>
        [Test]
        public void TheAimPointRecallNeverMovesMoreThanOneStepWhileTheIdealOneJumps()
        {
            const float LookAhead = 4.8f;
            const float Step = 0.24f;

            // Franchissement de noeud : le noeud vise est a 2 m devant, et le suivant est a 2 m sur la
            // droite. La cible ideale passe donc de la branche sortante du premier noeud a celle du
            // second : un echelon, par construction.
            var position = new Vector3(0f, 0f, 18f);
            var previousAim = new Vector3(0f, 0f, 22.8f);

            var idealBefore = LaneGraphRouting.ResolveLookAheadPoint(
                position, new Vector3(0f, 0f, 20f), Vector3.forward, LookAhead, previousAim, 0f);
            var idealAfter = LaneGraphRouting.ResolveLookAheadPoint(
                position, new Vector3(20f, 0f, 20f), Vector3.forward, LookAhead, previousAim, 0f);

            Assert.That(Vector3.Distance(idealBefore, idealAfter), Is.GreaterThan(Step),
                "La cible IDEALE doit vraiment sauter sur le cas mesure, sinon la garde ci-dessous ne prouverait rien "
                + "(mesure : " + Vector3.Distance(idealBefore, idealAfter).ToString("F2") + " m).");

            var rendered = LaneGraphRouting.ResolveLookAheadPoint(
                position, new Vector3(20f, 0f, 20f), Vector3.forward, LookAhead, previousAim, Step);

            Assert.That(Vector3.Distance(previousAim, rendered), Is.EqualTo(Step).Within(Epsilon),
                "La cible rendue avance d'EXACTEMENT un pas vers la cible ideale (mesure : "
                + Vector3.Distance(previousAim, rendered).ToString("F3") + " m pour un pas de " + Step.ToString("F3") + " m).");
            Assert.That(Vector3.Dot((rendered - previousAim).normalized, (idealAfter - previousAim).normalized),
                Is.EqualTo(1f).Within(1e-4f),
                "Et elle avance VERS la cible ideale, jamais a cote : la continuite n'est pas un retard.");

            // Quand l'echelon tient dans un pas, la cible rendue EST la cible ideale : le rappel ne
            // retarde alors rien du tout.
            var almostThere = idealAfter + new Vector3(0f, 0f, -0.05f);
            Assert.That(LaneGraphRouting.ResolveLookAheadPoint(
                    position, new Vector3(20f, 0f, 20f), Vector3.forward, LookAhead, almostThere, Step),
                Is.EqualTo(idealAfter),
                "Un echelon plus court qu'un pas est absorbe en entier : la cible rendue atteint l'ideale.");

            var nextAim = LaneGraphRouting.ResolveLookAheadPoint(
                position, new Vector3(20f, 0f, 20f), Vector3.forward, LookAhead, rendered, Step);
            Assert.That(Vector3.Distance(rendered, nextAim), Is.LessThanOrEqualTo(Step + Epsilon),
                "Le pas suivant avance de nouveau d'au plus un pas -- la continuite tient pas apres pas, pas une seule fois.");
            Assert.That(Vector3.Distance(nextAim, idealAfter), Is.LessThan(Vector3.Distance(rendered, idealAfter)),
                "Et le rappel se RAPPROCHE de la cible ideale au lieu de s'en eloigner.");
        }

        /// <summary>
        /// Le rappel est INERTE a un pas maximal nul ou non fini : c'est exactement la loi livree en
        /// 5.12, ce qui permet de mesurer les deux lois sur les memes trajectoires. Et une memoire non
        /// finie est ignoree plutot que propagee en NaN.
        /// </summary>
        [Test]
        public void TheAimPointRecallIsInertWithoutAStepAndIgnoresANonFiniteMemory()
        {
            var position = new Vector3(0f, 0f, 18f);
            var waypoint = new Vector3(0f, 0f, 20f);
            var previous = new Vector3(0f, 0f, 22.8f);

            var ideal = LaneGraphRouting.ResolveLookAheadPoint(position, waypoint, Vector3.forward, 4.8f, previous, 0f);

            Assert.That(LaneGraphRouting.ResolveLookAheadPoint(position, waypoint, Vector3.forward, 4.8f, previous, 0f),
                Is.EqualTo(ideal), "Pas nul : la cible rendue est la cible ideale -- la loi de la Story 5.12.");
            Assert.That(LaneGraphRouting.ResolveLookAheadPoint(position, waypoint, Vector3.forward, 4.8f, previous, -1f),
                Is.EqualTo(ideal), "Pas negatif : meme repli, jamais un rappel qui recule.");
            Assert.That(LaneGraphRouting.ResolveLookAheadPoint(position, waypoint, Vector3.forward, 4.8f, previous, float.NaN),
                Is.EqualTo(ideal));

            var nanMemory = LaneGraphRouting.ResolveLookAheadPoint(
                position, waypoint, Vector3.forward, 4.8f, new Vector3(float.NaN, 0f, 0f), 0.24f);
            Assert.That(nanMemory, Is.EqualTo(ideal),
                "Une memoire non finie est IGNOREE : la fonction rend une valeur utilisable plutot qu'un point invente.");
            Assert.That(float.IsNaN(nanMemory.x), Is.False);

            // La fonction ne mute jamais la memoire de l'appelant : elle la rend, et c'est le controleur
            // qui l'avance. Sans cela, la continuite dependrait d'un effet de bord.
            var source = CodeWithoutComments(File.ReadAllText(RoutingSourcePath));
            Assert.That(source, Does.Not.Contain("ref Vector3"), "La memoire de cible appartient a l'appelant, jamais a la fonction.");
        }

        // ---------------------------------------------------- authoring et validation

        /// <summary>
        /// Les quatre reglages des aides sont authores au MEME point de reglage, et le refus de
        /// validation NOMME le champ fautif -- ligne « profil incomplet » de la matrice : aucune valeur
        /// de repli silencieuse, le composant ne s'active pas.
        /// </summary>
        [Test]
        public void ProfileValidationRefusesAnUnauthoredAssistAndNamesTheField()
        {
            var def = ScriptableObject.CreateInstance<VehicleProfileDef>();
            try
            {
                var serialized = new SerializedObject(def);
                serialized.FindProperty("id").stringValue = "vehicle_test";
                serialized.ApplyModifiedPropertiesWithoutUndo();

                var profileProperty = serialized.FindProperty("profile");
                Assert.That(def.TryValidate(out _), Is.True,
                    "Le profil par defaut est valide : les refus ci-dessous viennent du champ modifie, pas d'un autre reglage.");

                AssertRefused(def, serialized, profileProperty, "yawStabilityRate", -1f, "YawStabilityRate",
                    "Une authoree negative inverserait le sens de l'aide au lieu de l'eteindre.");
                AssertRefused(def, serialized, profileProperty, "yawStabilityRate", float.NaN, "YawStabilityRate",
                    "Une authoree non finie produirait un couple non fini.");
                AssertRefused(def, serialized, profileProperty, "tractionControlStrength", 1f, "TractionControlStrength",
                    "Une attenuation totale annulerait le couple moteur : c'est le seuil binaire que la couche interdit.");
                AssertRefused(def, serialized, profileProperty, "tractionControlStrength", -0.1f, "TractionControlStrength",
                    "Une force negative amplifierait le couple moteur au lieu de l'attenuer.");
                AssertRefused(def, serialized, profileProperty, "spinRecoveryRate", -1f, "SpinRecoveryRate",
                    "Un taux de recuperation negatif redresserait dans le sens de la tete-a-queue.");
                AssertRefused(def, serialized, profileProperty, "spinDriftThresholdDegreesPerSecond", -1f, "SpinDriftThresholdDegreesPerSecond",
                    "Un seuil de derive negatif declencherait le rappel des le moindre virage.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(def);
            }
        }

        /// <summary>
        /// Le rappel de cible appartient au profil CONDUCTEUR, pas a la couche physique : c'est une
        /// donnee authoree, jamais une constante de controleur (Story 5.9). Sa validation est le seul
        /// champ de la story qui accepte le zero -- parce que zero veut dire « rappel inerte ».
        /// </summary>
        [Test]
        public void TheAimPointRecallSpeedIsAuthoredInTheDriverProfileAndRefusesOnlyNegativeValues()
        {
            var def = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath);
            Assert.That(def, Is.Not.Null, DriverProfilePath + " attendu");

            Assert.That(def.Profile.AimPointRecallSpeed, Is.GreaterThan(0f),
                "La vitesse de rappel est authoree et non nulle : c'est elle qui rend la visee continue au franchissement de noeud.");
            Assert.That(def.Profile.AimPointRecallSpeed, Is.GreaterThan(def.Profile.DesiredSpeed),
                "Elle est au-dessus de la vitesse de croisiere authoree, donc le rappel ne retarde pas la visee en conduite nominale : "
                + "il ne se voit que sur l'echelon qu'un franchissement de noeud produit.");

            var probe = ScriptableObject.CreateInstance<DriverProfileDef>();
            try
            {
                var serialized = new SerializedObject(probe);
                serialized.FindProperty("id").stringValue = "driver_test";

                var profileProperty = serialized.FindProperty("profile");
                var recallProperty = profileProperty.FindPropertyRelative("aimPointRecallSpeed");
                Assert.That(recallProperty, Is.Not.Null, "aimPointRecallSpeed attendu dans le profil conducteur");

                recallProperty.floatValue = -1f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(probe.TryValidate(out var error), Is.False,
                    "Une vitesse de rappel negative inverserait le sens du rappel au lieu de l'eteindre.");
                Assert.That(error, Does.Contain("AimPointRecallSpeed"), "Le refus nomme le champ fautif.");

                recallProperty.floatValue = 0f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(probe.TryValidate(out _), Is.True,
                    "Zero reste valide : c'est la valeur qui ETEINT le rappel, sur le patron de desactivation de la couche.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(probe);
            }
        }

        // ---------------------------------------------------- relief de recette dans MVP_Run

        /// <summary>
        /// Le district ne porte aucune denivellation LONGITUDINALE : la seule marche authoree est la
        /// bordure du carrefour, franchie lateralement. Sans relief sur une avenue, le franchissement
        /// d'une bosse et d'une marche n'est observable nulle part en Play Mode -- donc l'AC2 ne serait
        /// pas mesurable. Le relief est pose en OBJETS DE SCENE sous <c>GreyboxMap</c> : dans un prefab
        /// de module, il apparaitrait sur les quatorze instances du district, dont celles des routes IA,
        /// et ferait echouer la garde de plan de roulage de la Story 5.10.
        /// </summary>
        [Test]
        public void TheRecipeReliefIsASceneObjectOnTheAvenueAndStaysWithinTheCurbHeight()
        {
            WithMvpRun(scene =>
            {
                var map = ResolveGreyboxMap(scene);
                var bump = map.transform.Find(BumpObjectName);
                var step = map.transform.Find(StepObjectName);

                Assert.That(bump, Is.Not.Null, BumpObjectName + " attendu sous GreyboxMap");
                Assert.That(step, Is.Not.Null, StepObjectName + " attendu sous GreyboxMap");

                Assert.That(PrefabUtility.GetPrefabInstanceStatus(bump.gameObject), Is.EqualTo(PrefabInstanceStatus.NotAPrefab),
                    BumpObjectName + " doit etre un objet de SCENE : un prefab le repliquerait sur toutes les instances de l'avenue.");
                Assert.That(PrefabUtility.GetPrefabInstanceStatus(step.gameObject), Is.EqualTo(PrefabInstanceStatus.NotAPrefab));

                var bumpFootprint = Footprint(bump.gameObject);
                var stepFootprint = Footprint(step.gameObject);

                Assert.That(bumpFootprint.max.y, Is.EqualTo(Story511VehicleChassisWheelsAndSuspensionTests.CurbHeight).Within(0.002f),
                    "La crete du dos-d'ane culmine a la hauteur de bordure authoree (0,12 m), jamais au-dela : c'est la valeur "
                    + "de contrat du kit artistique, et 0,15 m avait deja ete retire pour cause de projection.");
                Assert.That(stepFootprint.max.y, Is.EqualTo(Story511VehicleChassisWheelsAndSuspensionTests.CurbHeight).Within(0.002f),
                    "La marche basse a exactement la meme hauteur : elle mesure la meme chose que la bordure, longitudinalement.");

                Assert.That(bumpFootprint.min.y, Is.LessThanOrEqualTo(0f), "Le relief repose sur le plan de roulage, il ne flotte pas.");
                Assert.That(stepFootprint.min.y, Is.LessThanOrEqualTo(0f));

                Assert.That(bumpFootprint.size.z, Is.EqualTo(CarriagewayWidth).Within(0.01f),
                    "Le dos-d'ane occupe la PLEINE largeur de chaussee : un dos-d'ane partiel se contourne et ne mesure plus rien.");
                Assert.That(stepFootprint.size.z, Is.EqualTo(CarriagewayWidth).Within(0.01f));
                Assert.That(bumpFootprint.center.z, Is.EqualTo(0f).Within(0.01f), "Et il est centre sur l'axe de la chaussee.");
                Assert.That(stepFootprint.center.z, Is.EqualTo(0f).Within(0.01f));

                var bumpLength = bumpFootprint.size.x;
                Assert.That(bumpLength * MaximumBumpSlope, Is.GreaterThanOrEqualTo(2f * Story511VehicleChassisWheelsAndSuspensionTests.CurbHeight - Epsilon),
                    "« Profil en pente douce » est une BORNE, pas un adjectif : " + bumpLength.ToString("F2") + " m de long pour "
                    + Story511VehicleChassisWheelsAndSuspensionTests.CurbHeight.ToString("F2") + " m de haut donne une pente moyenne sous "
                    + (MaximumBumpSlope * 100f).ToString("F0") + " % par rampe. Une marche de 0,12 m franchie en longueur reste une marche.");
                Assert.That(bumpLength, Is.LessThan(6f), "Et le relief reste un dos-d'ane, pas une colline.");

                Assert.That(bump.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(2),
                    "Deux rampes : sans elles, la crete serait une arete verticale et le dos-d'ane une marche.");

                // Le relief est pose sur la CHAUSSEE de l'avenue authoree nommee par la story.
                var avenue = ResolveAvenue(scene);
                var roadway = avenue.GetComponentsInChildren<Collider>(true).FirstOrDefault(c => c.name == "Col_Roadway");
                Assert.That(roadway, Is.Not.Null, "La chaussee de " + AvenueName + " doit exister");

                Assert.That(IsInside(roadway.bounds, bumpFootprint, 0.01f), Is.True,
                    "Le dos-d'ane est dans l'emprise de la chaussee de " + AvenueName + " (mesure : x " 
                    + bumpFootprint.min.x.ToString("F2") + " .. " + bumpFootprint.max.x.ToString("F2")
                    + " pour une chaussee a x " + roadway.bounds.min.x.ToString("F2") + " .. " + roadway.bounds.max.x.ToString("F2") + ").");
                Assert.That(IsInside(roadway.bounds, stepFootprint, 0.01f), Is.True,
                    "La marche basse aussi.");

                // Le relief est de la GEOMETRIE, pas une regle de jeu : aucun composant gameplay, aucune
                // surface roulable, aucun identifiant reseau. Les boites ne portent que leur rendu.
                foreach (var piece in bump.GetComponentsInChildren<Transform>(true).Concat(step.GetComponentsInChildren<Transform>(true)))
                {
                    foreach (var component in piece.GetComponents<Component>())
                    {
                        Assert.That(component, Is.InstanceOf<Transform>()
                            .Or.InstanceOf<MeshFilter>()
                            .Or.InstanceOf<MeshRenderer>()
                            .Or.InstanceOf<BoxCollider>(),
                            piece.name + " porte " + component.GetType().Name
                            + " : le relief de recette est un instrument de mise au point, pas un element du district "
                            + "(aucun LaneNode, aucun Collider nomme Col_*, aucun composant de gameplay).");
                    }
                }
            });
        }

        /// <summary>
        /// Le relief est author e dans la SCENE, donc aucun prefab de module ne le porte : c'est ce qui
        /// garantit qu'il n'apparait ni sur les quatorze instances du district, ni dans les routes IA,
        /// ni dans la garde de plan de roulage de la Story 5.10.
        /// </summary>
        [Test]
        public void NoModulePrefabCarriesTheRecipeReliefAndTheAvenueKeepsNoAddedChild()
        {
            var segment = AssetDatabase.LoadAssetAtPath<GameObject>(SegmentPrefabPath);
            Assert.That(segment, Is.Not.Null, SegmentPrefabPath + " attendu");

            var names = segment.GetComponentsInChildren<Transform>(true).Select(t => t.name).ToList();
            Assert.That(names.Any(n => n.StartsWith("Relief_", StringComparison.Ordinal)), Is.False,
                "Le prefab d'avenue ne porte AUCUN objet Relief_ : pose dans le prefab, le relief apparaitrait sur les quatorze "
                + "instances du district et heurterait NoModuleColliderRisesAboveTheDrivingPlane.");

            WithMvpRun(scene =>
            {
                var avenue = ResolveAvenue(scene);
                Assert.That(PrefabUtility.GetPrefabInstanceStatus(avenue.gameObject), Is.EqualTo(PrefabInstanceStatus.Connected),
                    "L'avenue reste une INSTANCE de prefab : la story ajoute un objet de scene a cote d'elle, elle ne defait pas le module.");

                var childNames = avenue.Cast<Transform>().Select(child => child.name).ToList();
                Assert.That(childNames.Any(n => n.StartsWith("Relief_", StringComparison.Ordinal)), Is.False,
                    "Le relief n'est pas un enfant AJOUTE a l'instance d'avenue : il vit sous GreyboxMap, a cote des modules.");
            });
        }

        /// <summary>
        /// Aucun <c>LaneNode</c> ne tombe dans l'emprise du relief. Le graphe de voies n'est pas modifie
        /// par cette story : un relief qui avale un noeud deplacerait le trace, ce que l'AC interdit.
        /// </summary>
        [Test]
        public void NoLaneNodeFallsInsideTheRecipeReliefFootprint()
        {
            WithMvpRun(scene =>
            {
                var map = ResolveGreyboxMap(scene);
                var footprints = new[]
                {
                    Footprint(map.transform.Find(BumpObjectName).gameObject),
                    Footprint(map.transform.Find(StepObjectName).gameObject)
                };

                var nodes = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<LaneNode>(true)).ToList();
                Assert.That(nodes.Count, Is.GreaterThan(0), "Le district porte ses noeuds de voie dans MVP_Run.");

                foreach (var node in nodes)
                {
                    var position = node.transform.position;

                    foreach (var footprint in footprints)
                    {
                        var planar = new Vector3(position.x, footprint.center.y, position.z);
                        Assert.That(footprint.Contains(planar), Is.False,
                            node.name + " (" + position.x.ToString("F2") + ", " + position.z.ToString("F2")
                            + ") tombe dans l'emprise du relief de recette. Le graphe de voies n'est pas modifie par cette story.");
                    }
                }
            });
        }

        // ---------------------------------------------------- plafonds de la couche

        /// <summary>
        /// L'anti-roulis etait le SEUL terme sans plafond de la couche : sa magnitude maximale
        /// atteignable (20 000 x 0,25 = 5 000 N) depassait la charge statique d'une roue (2 943 N). Il
        /// est desormais borne par la charge PORTEE, et ce que la couche lui passe est exactement cette
        /// charge -- une borne posee dans la fonction pure mais contournee par l'appelant ne garderait
        /// rien.
        /// </summary>
        [Test]
        public void TheLayerPassesTheCarriedLoadAsTheAntiRollBudget()
        {
            var source = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            var fixedUpdate = ExtractMethodBody(source, "private void FixedUpdate()");
            var antiRoll = ExtractMethodBody(source, "private void ApplyAntiRoll(Vector3 up, float loadBudget)");

            Assert.That(fixedUpdate, Does.Contain("ApplyAntiRoll(up, staticLoad)"),
                "L'appelant passe la charge statique d'une roue comme budget : c'est elle qui plafonne le transfert.");
            Assert.That(antiRoll, Does.Contain("ResolveAntiRollForces("),
                "Et le transfert lui-meme vient de la fonction pure, jamais d'un calcul refait sur place.");
            Assert.That(antiRoll, Does.Contain("loadBudget"),
                "Le budget traverse l'appelant sans etre reinterprete.");

            var suspension = CodeWithoutComments(File.ReadAllText(SuspensionModelSourcePath));
            var resolve = ExtractMethodBody(suspension, "public static void ResolveAntiRollForces(");
            Assert.That(resolve, Does.Contain("Mathf.Clamp"),
                "La fonction pure borne la magnitude plutot que de la laisser partir : un terme non borne est un defaut, pas un reglage.");
            Assert.That(resolve, Does.Contain("rightForce = -magnitude"),
                "Et la paire reste exactement opposee : la borne ne change NI le sens NI la paire de l'anti-roulis.");
        }

        /// <summary>
        /// Ligne « roues en l'air » : la rotation de roue continue d'etre integree quand plus aucun
        /// contact n'existe -- une roue motrice qui ne touche pas continue de tourner, et c'est ce que
        /// le glissement de l'atterrissage doit retrouver. Le garde doit donc porter sur le pneu, pas
        /// sur l'integration.
        /// </summary>
        [Test]
        public void TheWheelSpinIsStillIntegratedWhileNoWheelTouchesTheGround()
        {
            var source = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            var fixedUpdate = ExtractMethodBody(source, "private void FixedUpdate()");

            var tireBlockStart = fixedUpdate.LastIndexOf("if (normalLoad > 0f)", StringComparison.Ordinal);
            Assert.That(tireBlockStart, Is.GreaterThanOrEqualTo(0), "Le bloc de pneu garde par la charge doit exister");

            var tireBlock = ExtractBracedBlockAt(fixedUpdate, tireBlockStart);
            Assert.That(tireBlock, Does.Contain("ResolveTireForces("), "Le cas de test doit viser le bloc de PNEU, sinon il ne prouve rien.");
            Assert.That(tireBlock, Does.Not.Contain("IntegrateWheelAngularVelocity"),
                "L'integration de la rotation de roue ne doit PAS etre dans le garde de charge : en l'air, la roue continue de tourner.");

            Assert.That(fixedUpdate, Does.Contain("IntegrateWheelAngularVelocity"),
                "Elle doit exister quelque part dans le pas, sans quoi le glissement d'atterrissage serait toujours nul.");
            Assert.That(fixedUpdate.IndexOf("IntegrateWheelAngularVelocity", StringComparison.Ordinal),
                Is.GreaterThan(tireBlockStart + tireBlock.Length),
                "Et elle vient APRES le bloc de pneu : c'est ce qui en fait une integration inconditionnelle.");
        }

        /// <summary>
        /// L'amortissement d'assiette n'est plus conditionne a un contact : c'est lui qui tient
        /// l'attitude d'un vehicule en vol. Mais AUCUNE rotation n'est ecrite ni gelee pour autant --
        /// une caisse qui tournait sur elle-meme continue de tourner.
        /// </summary>
        [Test]
        public void TheAttitudeStaysStableInTheAirWithoutEverWritingARotation()
        {
            var source = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            var assist = ExtractMethodBody(source, "private void ApplyAttitudeAssist(Vector3 up, int groundedWheels)");

            var damping = assist.IndexOf("ResolveAttitudeDampingTorque", StringComparison.Ordinal);
            var levellingGuard = assist.IndexOf("if (groundedWheels > 0)", StringComparison.Ordinal);

            Assert.That(damping, Is.GreaterThanOrEqualTo(0), "L'amortissement d'assiette doit exister.");
            Assert.That(levellingGuard, Is.GreaterThan(damping),
                "L'amortissement est applique AVANT le garde de contact : c'est ce qui lui permet de tenir l'attitude en vol. "
                + "Conditionne au contact, il laissait une caisse en l'air garder la rotation qu'un choc lui avait donnee.");
            Assert.That(assist, Does.Not.Contain("linearVelocity"),
                "La couche ne fait que produire un couple : aucune vitesse n'est ecrite pour stabiliser l'attitude.");
            Assert.That(assist, Does.Not.Contain("angularVelocity ="),
                "Aucune vitesse angulaire n'est ecrite non plus : un amortissement qui ECRIT la vitesse angulaire serait une rotation gelee.");
            Assert.That(assist, Does.Not.Contain("MoveRotation"));
            Assert.That(assist, Does.Not.Contain("rotation ="));
            Assert.That(assist, Does.Contain("ForceMode.Force"),
                "Le terme est un couple applique, jamais un etat pose.");
        }

        /// <summary>
        /// Les aides de lacet de la couche sont bornees individuellement PUIS dans leur somme : aucun
        /// chemin ne peut depasser le budget, et aucun n'ecrit de rotation.
        /// </summary>
        [Test]
        public void TheYawAssistsAreBoundedIndividuallyAndAsASum()
        {
            var source = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            var assists = ExtractMethodBody(source, "private void ApplyYawAssists(Vector3 up, float yawTorqueBudget)");

            Assert.That(assists, Does.Contain("ResolveYawStabilityTorque("));
            Assert.That(assists, Does.Contain("ResolveSpinRecoveryTorque("));
            Assert.That(assists, Does.Contain("profile.YawStabilityRate"),
                "L'abattement vient du profil authore, jamais d'une constante du controleur ni de la couche.");
            Assert.That(assists, Does.Contain("profile.SpinRecoveryRate"));
            Assert.That(assists, Does.Contain("profile.SpinDriftThresholdDegreesPerSecond"));
            Assert.That(assists, Does.Contain("driveIntent.Steer"),
                "Le couple de recuperation suit l'ENTREE conducteur : c'est ce qui l'empeche de se substituer a elle.");
            Assert.That(assists, Does.Contain("Mathf.Clamp(torque, -yawTorqueBudget, yawTorqueBudget)"),
                "La somme des deux termes est bornee a son tour : aucun chemin ne depasse le budget.");
            Assert.That(assists, Does.Contain("AddTorque"),
                "Le terme est un couple applique -- jamais une rotation ecrite, donc une caisse en rotation continue de tourner.");
            Assert.That(assists, Does.Not.Contain("angularVelocity ="));
            Assert.That(assists, Does.Not.Contain("MoveRotation"));
        }

        // ---------------------------------------------------- invariants de code

        /// <summary>
        /// AD-35 : aucun <c>WheelCollider</c> dans la couche physique -- il ne se prouve que pendant un
        /// pas de physique, donc hors de la suite EditMode ou les fonctions pures se verifient.
        /// </summary>
        [Test]
        public void ThePhysicsLayerStillUsesNoWheelCollider()
        {
            foreach (var path in PhysicsLayerSourcePaths)
            {
                Assert.That(File.Exists(path), Is.True, path + " attendu");
                var code = CodeWithoutComments(File.ReadAllText(path));
                Assert.That(code, Does.Not.Contain("WheelCollider"),
                    path + " : AD-35 impose un modele a raycasts par roue, pour tous les termes -- y compris ceux de cette story.");
            }
        }

        /// <summary>
        /// La couche physique ne lit QUE le profil : jamais la rage, la peur, une disposition, ni un
        /// alea (AD-33 / AD-35). C'est l'AC qui l'exige pour chacune des trois aides.
        /// </summary>
        [Test]
        public void TheAssistsReadOnlyTheAuthoredProfileAndNeverRageFearOrRandom()
        {
            var assist = File.ReadAllText(ArcadeAssistSourcePath);
            var code = CodeWithoutComments(assist);

            Assert.That(code, Does.Not.Contain("MonoBehaviour"), "Les aides sont des fonctions pures, pas un composant.");
            Assert.That(code, Does.Not.Contain("Time."), "Aucune dependance a l'horloge : le pas de temps est un argument.");
            Assert.That(code, Does.Not.Contain("Rigidbody"), "Aucune dependance au Rigidbody : c'est ce qui les rend verifiables en EditMode.");
            Assert.That(code, Does.Not.Contain("Random"), "Aucun tirage : la meme entree donne la meme sortie.");
            Assert.That(code, Does.Not.Contain("Networked"), "Aucun etat reseau : les aides sont locales a la couche.");
            Assert.That(code, Does.Not.Contain("RageValue"));
            Assert.That(code, Does.Not.Contain("RageDisposition"));
            Assert.That(code, Does.Not.Contain("FearValue"));
            Assert.That(code, Does.Not.Contain("VehicleDriveIntent"),
                "Une aide ne touche jamais l'intent : elle produit un couple ou un facteur, et la neutralisation reste le fait de l'entree.");
            Assert.That(code, Does.Not.Contain("SceneManager"));
            Assert.That(code, Does.Contain("public static class VehicleArcadeAssist"), "Les aides forment un ensemble de fonctions pures.");

            var body = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            Assert.That(body, Does.Not.Contain("RageValue"));
            Assert.That(body, Does.Not.Contain("RageDisposition"));
            Assert.That(body, Does.Not.Contain("FearValue"));
            Assert.That(body, Does.Not.Contain("Random"));
            Assert.That(body, Does.Contain("VehicleArcadeAssist."), "La couche BRANCHE les aides, elle ne les reimplemente pas.");
        }

        /// <summary>
        /// Aucune ecriture de vitesse ni de rotation dans la couche : elle ne fait que produire des
        /// forces et des couples. L'unique ecriture de vitesse du projet reste la recuperation 3.4 du
        /// controleur joueur.
        /// </summary>
        [Test]
        public void TheArcadeAssistsAddNoVelocityOrRotationWriteToTheLayer()
        {
            var body = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));

            Assert.That(body, Does.Not.Contain("linearVelocity ="),
                "La couche lit la vitesse, elle ne l'ecrit pas : une aide qui ecrirait une vitesse masquerait un symptome au lieu de le corriger.");
            Assert.That(body, Does.Not.Contain("angularVelocity ="));
            Assert.That(body, Does.Not.Contain("MoveRotation"));
            Assert.That(body, Does.Not.Contain("body.rotation ="));
            Assert.That(body, Does.Not.Contain("body.position ="));
            Assert.That(body, Does.Not.Contain("isKinematic ="));

            var controller = CodeWithoutComments(File.ReadAllText(PlayerControllerSourcePath));
            Assert.That(Occurrences(controller, "linearVelocity ="), Is.EqualTo(1),
                "Il ne reste qu'UNE ecriture de vitesse dans tout le controleur joueur : la remise a zero de la recuperation (Story 3.4).");

            // La garde de la Story 5.2 compte exactement trois methodes statiques sur le controleur IA :
            // la visee continue se corrige dans la fonction pure, pas par un second chemin.
            Assert.That(Occurrences(File.ReadAllText(AiControllerSourcePath), "static "), Is.EqualTo(3),
                "Aucune methode statique supplementaire : la continuite de la visee vit dans LaneGraphRouting.");

            var ai = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));
            Assert.That(ai, Does.Contain("previousAimPoint"),
                "Le controleur PORTE la memoire de cible : elle appartient a l'appelant, comme la fonction pure le documente.");
            Assert.That(ai, Does.Contain("profile.AimPointRecallSpeed"),
                "Le pas de rappel vient de la donnee AUTHOREE du profil conducteur, jamais d'une constante du controleur (Story 5.9).");
        }

        // ---------------------------------------------------- consommation des sorties

        /// <summary>
        /// Les fonctions pures ne servent a rien si la couche ne CONSOMME pas leur sortie. Ce n'est pas
        /// une garde de style : sans elle, retirer `driveTorque *= groundedAuthority`, retirer
        /// `wheelDriveTorque *= tractionFactor` ou ecrire `ApplyYawAssists(up, 0f)` laisse les 601 tests
        /// EditMode verts, et la ligne « autorite degradee » de la matrice comme la ligne « adherence
        /// saturee » peuvent disparaitre sans que rien ne le signale. La garde de l'anti-roulis existait
        /// deja pour cette raison ; celle-ci couvre les trois autres points de consommation.
        /// </summary>
        [Test]
        public void TheLayerConsumesEveryAssistOutputAtItsPointOfUse()
        {
            var source = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            var fixedUpdate = ExtractMethodBody(source, "private void FixedUpdate()");
            var steering = ExtractMethodBody(source, "private void UpdateSteeringState(");

            Assert.That(fixedUpdate, Does.Contain("driveTorque *= groundedAuthority"),
                "L'autorite par roues au sol REDUIT le couple moteur. Sans cette multiplication, un vehicule "
                + "a une ou zero roue au sol conserve toute son autorite de conduite.");
            Assert.That(fixedUpdate, Does.Contain("UpdateSteeringState(current, fixedDeltaTime, longitudinalSpeed, groundedAuthority)"),
                "Et elle est PASSEE a la direction : l'angle de roue doit suivre le meme facteur.");
            Assert.That(steering, Does.Contain("authority"),
                "Le corps de UpdateSteeringState consomme l'autorite -- la passer sans l'utiliser serait une garde creuse.");
            Assert.That(steering, Does.Contain("driveSteerRateDegreesPerSecond * authority"),
                "Les DEUX taux sont mis a l'echelle : braquage et rappel.");
            Assert.That(steering, Does.Contain("current.SteerReturnRateDegreesPerSecond * authority"));

            Assert.That(fixedUpdate, Does.Contain("wheelDriveTorque *= tractionFactor"),
                "Le controle de traction agit sur le couple MOTEUR de la roue : c'est la cause du glissement "
                + "excessif, pas le symptome. Un facteur calcule mais non applique ne corrige rien.");
            Assert.That(fixedUpdate, Does.Contain("ResolveTractionControlFactor("),
                "Et le facteur vient de la fonction pure, jamais d'un calcul refait sur place.");

            Assert.That(fixedUpdate, Does.Contain("ApplyYawAssists(up, yawTorqueBudget)"),
                "Le budget de lacet traverse l'appelant sans etre remplace par une constante : a zero, les "
                + "deux aides de lacet seraient inertes a chaque pas, et rien ne le verrait.");
            Assert.That(fixedUpdate, Does.Contain("ResolveYawTorqueBudget("),
                "Et il est derive du vehicule, pas ecrit en dur.");

            Assert.That(fixedUpdate, Does.Contain("GroundedAuthorityFactor = VehicleArcadeAssist.ResolveGroundedAuthorityFactor(groundedWheels, current.WheelCount)"),
                "Compte et facteur publies ensemble, depuis la MEME mesure : la telemetrie est l'instrument "
                + "des controles humains, un couple qui n'a jamais coexiste la rendrait trompeuse.");
        }

        /// <summary>
        /// La memoire de cible de l'IA est effacee partout ou le vehicule est REPOSE a une pose connue.
        /// Sans cet effacement, le rappel repart de la cible d'AVANT la teleportation et l'IA converge
        /// vers elle pendant des dizaines de pas : le saut de consigne que la story supprime reapparait,
        /// invisible, exactement sur le cas pour lequel il a ete ecrit.
        /// </summary>
        [Test]
        public void TheAimPointMemoryIsPurgedWhereverTheVehicleIsRepositioned()
        {
            var source = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));
            var reset = ExtractMethodBody(source, "private void ResetRouteMemoryAt(");
            var recover = ExtractMethodBody(source, "private void RecoverAtWaypoint(");

            Assert.That(reset, Does.Contain("hasAimPoint = false;"),
                "L'effacement vit dans ResetRouteMemoryAt, seul point commun des repositionnements.");
            Assert.That(reset, Does.Contain("previousAimPoint = Vector3.zero;"),
                "Et la cible elle-meme est purgee : garder le point en laissant le drapeau faux ferait "
                + "repartir le rappel d'une cible perimee.");
            Assert.That(recover, Does.Contain("ResetRouteMemoryAt("),
                "La recuperation sur place repose le vehicule a plusieurs metres : c'est le cas qui compte.");
        }

        // ---------------------------------------------------- helpers

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

            if (!float.IsNaN(value))
            {
                Assert.That(property.floatValue, Is.EqualTo(value).Within(Epsilon),
                    fieldName + " : la valeur posee doit etre refusee par la validation, pas corrigee en silence.");
            }

            var valid = def.TryValidate(out var error);
            Assert.That(valid, Is.False, fieldName + " = " + value + " : " + because);
            Assert.That(error, Does.Contain(expectedInError), "Le refus nomme le champ fautif (" + fieldName + ").");

            property.floatValue = before;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Emprise monde d'un objet de relief : l'union des boites de tous ses colliders.</summary>
        private static Bounds Footprint(GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            Assert.That(colliders.Length, Is.GreaterThan(0), root.name + " : un relief sans collider ne franchit rien.");

            var bounds = colliders[0].bounds;
            for (var i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return bounds;
        }

        /// <summary>Vrai si <paramref name="inner"/> tient dans <paramref name="outer"/>, a la tolerance pres.</summary>
        private static bool IsInside(Bounds outer, Bounds inner, float tolerance)
        {
            return inner.min.x >= outer.min.x - tolerance
                && inner.max.x <= outer.max.x + tolerance
                && inner.min.z >= outer.min.z - tolerance
                && inner.max.z <= outer.max.z + tolerance;
        }

        private static Transform ResolveGreyboxMap(Scene scene)
        {
            var runRoot = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == "RunRoot");
            Assert.That(runRoot, Is.Not.Null, "RunRoot attendu dans MVP_Run");

            var map = runRoot.transform.Find("GreyboxMap");
            Assert.That(map, Is.Not.Null, "GreyboxMap attendu sous RunRoot : c'est lui qui porte le decor du district.");
            return map;
        }

        private static Transform ResolveAvenue(Scene scene)
        {
            var laneGraph = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(candidate => candidate.name == "LaneGraph");
            Assert.That(laneGraph, Is.Not.Null, "LaneGraph attendu dans MVP_Run");

            var avenue = laneGraph.Find(AvenueName);
            Assert.That(avenue, Is.Not.Null, AvenueName + " attendu : c'est l'avenue qui porte le relief de recette.");
            return avenue;
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

        /// <summary>Corps d'une methode, sans ses accolades englobantes : sert aux gardes de forme sur le code.</summary>
        private static string ExtractMethodBody(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), signature + " attendu dans le source");
            return ExtractBracedBlockAt(source, start);
        }

        /// <summary>Bloc d'accolades equilibre qui suit une position donnee.</summary>
        private static string ExtractBracedBlockAt(string source, int index)
        {
            var open = source.IndexOf('{', index);
            Assert.That(open, Is.GreaterThanOrEqualTo(0), "accolade ouvrante attendue");

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

            Assert.Fail("accolades non equilibrees");
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
