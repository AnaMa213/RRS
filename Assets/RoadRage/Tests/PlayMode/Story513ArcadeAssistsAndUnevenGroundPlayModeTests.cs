using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RoadRage.App.Services;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.13 : la preuve runtime du sol irregulier et des aides arcade -- celle qu'aucune garde
    /// EditMode ne peut porter.
    ///
    /// BANC A EXECUTER PAR L'HUMAIN. Le harnais PlayMode de cette installation ne peut pas etre
    /// produit par l'agent (le mode play ne s'execute pas en synchrone, et la voie asynchrone revient
    /// avec zero test) : ce fichier est livre pour le Test Runner de l'Editeur, et aucun resultat ne
    /// doit en etre deduit avant d'avoir ete rapporte brut.
    ///
    /// DIFFERENCE DE FOND AVEC LE BANC 5.11 : celui-ci n'ecrit JAMAIS la vitesse. Le banc de la
    /// Story 5.11 imposait la vitesse horizontale parce que le controleur l'ecrivait, et il mesurait
    /// la suspension. Ici le banc ne fait que SOUMETTRE UNE INTENTION, exactement comme le fait le
    /// controleur joueur (`ApplyDriveIntent`) : tout le reste -- couple, glissement, pneu, suspension,
    /// aides -- appartient a la couche physique.
    ///
    /// LE RELIEF DU BANC REPRODUIT LES COTES AUTHORES DANS `MVP_Run` : crete de dos-d'ane a 0,12 m sur
    /// deux rampes, marche basse de 0,12 m, pleine largeur de chaussee. Il est construit ici plutot que
    /// charge depuis la scene d'integration pour que le banc reste executable seul, sans bootstrap
    /// reseau ; la garde EditMode de la story verifie, elle, que le relief AUTHOR E a bien ces cotes.
    /// La recette humaine dans `MVP_Run` (procedure dans la note de livraison) est la mesure qui fait
    /// foi pour l'absolu.
    /// </summary>
    [Category("Story513")]
    public sealed class Story513ArcadeAssistsAndUnevenGroundPlayModeTests
    {
        private const string ProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";

        /// <summary>Dimensions du collider du vehicule, copies des cotes figees du prefab joueur (NFR18).</summary>
        private static readonly Vector3 VehicleColliderSize = new Vector3(2.06f, 1.42f, 4.44f);

        private static readonly Vector3 VehicleColliderCenter = new Vector3(0f, 0.73f, 0f);

        /// <summary>Hauteur de bordure authoree (Story 5.11) : le relief de recette tient dans cette valeur, jamais au-dela.</summary>
        private const float CurbHeight = 0.12f;

        /// <summary>Largeur de chaussee d'un module d'avenue : le relief occupe toute la largeur.</summary>
        private const float CarriagewayWidth = 8f;

        /// <summary>Pas de physique maximal d'un scenario : borne le temps de banc, jamais une assertion.</summary>
        private const int MaxObservedSteps = 320;

        /// <summary>Bornes de non-regression du banc 5.11 : « la roue monte » contre « le vehicule est projete ».</summary>
        private const float MaxVerticalExcursion = 0.4f;

        private const float MaxVerticalSpeed = 4f;

        private GameObject ground;
        private GameObject vehicle;
        private VehiclePhysicsBody physics;
        private readonly List<GameObject> props = new List<GameObject>();
        private Scene benchScene;
        private Scene originalActiveScene;
        private string originalActiveScenePath;
        private readonly List<string> unloadedScenePaths = new List<string>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return EnterEmptyBenchScene("Story513_Bench");

            ground = BuildGround(0f);
            vehicle = BuildVehicle();
            physics = vehicle.GetComponent<VehiclePhysicsBody>();

            yield return Settle(vehicle.GetComponent<Rigidbody>());
        }

        /// <summary>
        /// Ce banc construit son propre relief et conduit par les pneus : il ne doit donc trouver
        /// AUCUNE autre geometrie dans le monde physique par defaut, celui que
        /// <see cref="VehiclePhysicsBody"/> interroge par <c>Physics.Raycast</c>.
        ///
        /// Or les bancs precedents (Story 5.10) laissent <c>MVP_Run</c> charge et actif : mesure du
        /// 2026-09-20, avant correction, le vehicule s'arretait net a z = 37,48 m sur un module du
        /// district au lieu d'atteindre la bordure ou le relief que ce fichier construit lui-meme.
        /// Les trois echecs de ce banc en decoulaient (aucune roue delestee, aucune roue en l'air,
        /// vitesse residuelle nulle apres la bordure) : le vehicule ne parcourait jamais le scenario.
        ///
        /// Une scene de banc dediee et active, les scenes de l'application dechargees, rendent la
        /// mesure independante de l'ordre d'execution des fixtures.
        /// </summary>
        private IEnumerator EnterEmptyBenchScene(string benchName)
        {
            originalActiveScene = SceneManager.GetActiveScene();
            originalActiveScenePath = originalActiveScene.path;
            benchScene = SceneManager.CreateScene(benchName + "_" + System.Guid.NewGuid());
            SceneManager.SetActiveScene(benchScene);

            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene == benchScene)
                {
                    continue;
                }

                if (scene.name == AppSceneRouter.BootstrapSceneName
                    || scene.name == AppSceneRouter.MainMenuLobbySceneName
                    || scene.name == AppSceneRouter.MvpRunSceneName)
                {
                    if (!string.IsNullOrEmpty(scene.path))
                    {
                        unloadedScenePaths.Add(scene.path);
                    }

                    yield return SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var prop in props)
            {
                if (prop != null)
                {
                    Object.Destroy(prop);
                }
            }

            props.Clear();

            Object.Destroy(ground);
            Object.Destroy(vehicle);
            yield return null;

            if (benchScene.IsValid() && benchScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(benchScene);
            }

            foreach (var scenePath in unloadedScenePaths)
            {
                var loaded = SceneManager.GetSceneByPath(scenePath);
                if (!loaded.IsValid() || !loaded.isLoaded)
                {
                    yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                }
            }

            var sceneToRestore = originalActiveScene;
            if ((!sceneToRestore.IsValid() || !sceneToRestore.isLoaded) && !string.IsNullOrEmpty(originalActiveScenePath))
            {
                sceneToRestore = SceneManager.GetSceneByPath(originalActiveScenePath);
            }

            if (sceneToRestore.IsValid() && sceneToRestore.isLoaded)
            {
                SceneManager.SetActiveScene(sceneToRestore);
            }

            unloadedScenePaths.Clear();
            benchScene = default;
            originalActiveScene = default;
            originalActiveScenePath = null;
        }

        /// <summary>
        /// La bordure authoree franchie par UN SEUL COTE : c'est le cas qu'une bordure produit, et
        /// celui que l'anti-roulis non borne pouvait retourner -- sa magnitude maximale depassait la
        /// charge statique d'un coin. Attendu : le vehicule monte, n'est pas projete, et n'est pas
        /// arrete contre la levre.
        /// </summary>
        [UnityTest]
        public IEnumerator TheVehicleClimbsTheAuthoredCurbOnOneSideWithoutBeingLaunched()
        {
            // Bande de 0,12 m sur la voie de GAUCHE seulement : les roues de droite restent au sol.
            props.Add(BuildBox("Story513_Curb", new Vector3(-1.4f, CurbHeight * 0.5f, 14f), new Vector3(1.6f, CurbHeight, 12f)));

            var body = vehicle.GetComponent<Rigidbody>();
            var restHeight = vehicle.transform.position.y;

            yield return Drive(throttle: 1f, steer: 0f, steps: 120);

            var speedBefore = PlanarSpeed(body);
            var excursion = 0f;
            var maxVerticalSpeed = 0f;
            var minimumGrounded = physics.WheelCount;
            var uprightAtTheEnd = 1f;

            for (var step = 0; step < 200; step++)
            {
                Command(throttle: 0.4f, steer: 0f, brakeReverse: 0f, handbrake: 0f);
                yield return new WaitForFixedUpdate();

                excursion = Mathf.Max(excursion, vehicle.transform.position.y - restHeight);
                maxVerticalSpeed = Mathf.Max(maxVerticalSpeed, Mathf.Abs(body.linearVelocity.y));
                minimumGrounded = Mathf.Min(minimumGrounded, physics.GroundedWheelCount);
                uprightAtTheEnd = Vector3.Dot(vehicle.transform.up, Vector3.up);

                Assert.That(float.IsNaN(body.linearVelocity.magnitude), Is.False, "Aucune grandeur ne doit devenir NaN en franchissant une bordure.");
            }

            var speedAfter = PlanarSpeed(body);
            Debug.Log("[Story513] bordure 0,12 m d'un seul cote : excursion verticale maximale " + excursion.ToString("F3")
                + " m, vitesse verticale maximale " + maxVerticalSpeed.ToString("F2") + " m/s, vitesse "
                + speedBefore.ToString("F2") + " -> " + speedAfter.ToString("F2") + " m/s, roues au sol minimales "
                + minimumGrounded + ", z final " + vehicle.transform.position.z.ToString("F2") + " m.");

            Assert.That(excursion, Is.LessThan(MaxVerticalExcursion),
                "Le vehicule ne doit pas etre PROJETE : excursion verticale mesuree " + excursion.ToString("F3")
                + " m. C'est la borne de non-regression du banc 5.11 (0,4 m).");
            Assert.That(maxVerticalSpeed, Is.LessThan(MaxVerticalSpeed),
                "Ni decolle : vitesse verticale maximale mesuree " + maxVerticalSpeed.ToString("F2") + " m/s (borne 4 m/s).");
            Assert.That(vehicle.transform.position.z, Is.GreaterThan(21f),
                "Et il a bien FRANCHI la bande : z final " + vehicle.transform.position.z.ToString("F2")
                + " m pour une bande qui s'arrete a z = 20 m. « Coince contre la levre » est exactement ce que ce chiffre exclut.");
            Assert.That(speedAfter, Is.GreaterThan(2f),
                "Il continue d'avancer apres la bordure : vitesse residuelle " + speedAfter.ToString("F2") + " m/s.");
            Assert.That(Vector3.Dot(vehicle.transform.up, Vector3.up), Is.GreaterThan(0.7f),
                "Et il ne s'est pas couche (axe haut a " + Vector3.Dot(vehicle.transform.up, Vector3.up).ToString("F2") + " de la verticale).");
            Assert.That(uprightAtTheEnd, Is.GreaterThan(0.7f));
        }

        /// <summary>
        /// Le relief de recette franchi LONGITUDINALEMENT a vitesse de conduite : d'abord le dos-d'ane,
        /// puis la marche basse. C'est le seul cas ou la clause « ni coince contre la levre » se teste :
        /// la bordure du carrefour se franchit lateralement.
        /// </summary>
        [UnityTest]
        public IEnumerator TheVehicleCrossesTheRecipeBumpAndStepAtDrivingSpeed()
        {
            BuildRecipeRelief();

            var body = vehicle.GetComponent<Rigidbody>();
            var restHeight = vehicle.transform.position.y;

            yield return Drive(throttle: 1f, steer: 0f, steps: 110);

            var speedBefore = PlanarSpeed(body);
            var excursion = 0f;
            var maxVerticalSpeed = 0f;
            var passedTheStep = false;

            for (var step = 0; step < 260; step++)
            {
                Command(throttle: 0.45f, steer: 0f, brakeReverse: 0f, handbrake: 0f);
                yield return new WaitForFixedUpdate();

                excursion = Mathf.Max(excursion, vehicle.transform.position.y - restHeight);
                maxVerticalSpeed = Mathf.Max(maxVerticalSpeed, Mathf.Abs(body.linearVelocity.y));
                passedTheStep |= vehicle.transform.position.z > 14.5f;
            }

            var speedAfter = PlanarSpeed(body);
            Debug.Log("[Story513] relief de recette a vitesse de conduite : excursion verticale maximale " + excursion.ToString("F3")
                + " m, vitesse verticale maximale " + maxVerticalSpeed.ToString("F2") + " m/s, vitesse "
                + speedBefore.ToString("F2") + " -> " + speedAfter.ToString("F2") + " m/s, z final "
                + vehicle.transform.position.z.ToString("F2") + " m.");

            Assert.That(passedTheStep, Is.True,
                "Le vehicule doit passer la marche basse : il doit depasser z = 14,5 m, or il finit a z = "
                + vehicle.transform.position.z.ToString("F2") + " m. Une immobilisation contre la face serait le symptome que la "
                + "story devait trancher -- et s'il ne se reproduit pas, il faut le DIRE plutot que d'ajouter un mecanisme.");
            Assert.That(excursion, Is.LessThan(MaxVerticalExcursion),
                "Aucun decollage : excursion verticale mesuree " + excursion.ToString("F3") + " m.");
            Assert.That(maxVerticalSpeed, Is.LessThan(MaxVerticalSpeed),
                "Ni projection : vitesse verticale maximale mesuree " + maxVerticalSpeed.ToString("F2") + " m/s.");
            Assert.That(speedAfter, Is.GreaterThan(2f),
                "Le relief est franchi, pas subi : vitesse residuelle " + speedAfter.ToString("F2") + " m/s.");
        }

        /// <summary>
        /// Le meme relief franchi a BASSE vitesse : un dos-d'ane et une marche doivent passer lentement
        /// aussi. Sous le seuil de direction authore, la consigne d'angle est nulle, donc le vehicule
        /// avance droit -- ce qui est exactement le cas a mesurer ici.
        /// </summary>
        [UnityTest]
        public IEnumerator TheVehicleCrossesTheRecipeStepAtLowSpeed()
        {
            BuildRecipeRelief();

            var body = vehicle.GetComponent<Rigidbody>();
            var restHeight = vehicle.transform.position.y;

            yield return Drive(throttle: 0.25f, steer: 0f, steps: 200);

            var excursion = 0f;
            var maxVerticalSpeed = 0f;

            for (var step = 0; step < 320; step++)
            {
                Command(throttle: 0.25f, steer: 0f, brakeReverse: 0f, handbrake: 0f);
                yield return new WaitForFixedUpdate();

                excursion = Mathf.Max(excursion, vehicle.transform.position.y - restHeight);
                maxVerticalSpeed = Mathf.Max(maxVerticalSpeed, Mathf.Abs(body.linearVelocity.y));
            }

            Debug.Log("[Story513] relief de recette a basse vitesse : excursion verticale maximale " + excursion.ToString("F3")
                + " m, vitesse verticale maximale " + maxVerticalSpeed.ToString("F2") + " m/s, z final "
                + vehicle.transform.position.z.ToString("F2") + " m, vitesse "
                + PlanarSpeed(body).ToString("F2") + " m/s.");

            Assert.That(vehicle.transform.position.z, Is.GreaterThan(14.5f),
                "A basse vitesse aussi, la marche basse est franchie : z final "
                + vehicle.transform.position.z.ToString("F2") + " m.");
            Assert.That(excursion, Is.LessThan(MaxVerticalExcursion), "Et sans decollage : excursion " + excursion.ToString("F3") + " m.");
            Assert.That(maxVerticalSpeed, Is.LessThan(MaxVerticalSpeed));
        }

        /// <summary>
        /// Perte de contact PARTIELLE : le vehicule monte sur un plateau par une rampe douce, puis son
        /// essieu avant quitte le bord avant l'arriere. L'autorite publiee doit suivre le nombre de
        /// roues au sol, dans la proportion exacte, et jamais au-dela.
        ///
        /// Le facteur applique par un pas est celui du nombre de roues du pas PRECEDENT -- c'est dit
        /// dans la couche, et c'est inherent : le compte du pas courant n'existe qu'apres les raycasts.
        /// Le banc mesure donc cette relation-la, pas une egalite qui n'existe pas.
        /// </summary>
        [UnityTest]
        public IEnumerator TheAuthorityFollowsTheGroundedWheelCountInProportion()
        {
            BuildPlateau();

            yield return Drive(throttle: 1f, steer: 0f, steps: 130);

            var observedCounts = new HashSet<int>();
            var sawPartialContact = false;
            var mismatches = 0;
            var samples = 0;
            var previousCount = physics.GroundedWheelCount;

            for (var step = 0; step < 300; step++)
            {
                Command(throttle: 0.5f, steer: 0f, brakeReverse: 0f, handbrake: 0f);
                yield return new WaitForFixedUpdate();

                var count = physics.GroundedWheelCount;
                var expected = VehicleArcadeAssist.ResolveGroundedAuthorityFactor(previousCount, physics.WheelCount);
                var applied = physics.GroundedAuthorityFactor;

                samples++;
                observedCounts.Add(count);
                sawPartialContact |= count > 0 && count < physics.WheelCount;

                if (Mathf.Abs(applied - expected) > 0.0001f)
                {
                    mismatches++;
                }

                Assert.That(float.IsNaN(applied), Is.False, "Le facteur d'autorite ne doit jamais etre NaN.");
                Assert.That(applied, Is.InRange(0f, 1f), "C'est une proportion : jamais au-dela de 1.");

                previousCount = count;
            }

            Debug.Log("[Story513] autorite par roues au sol : " + samples + " pas mesures, comptes observes ["
                + string.Join(", ", observedCounts) + "], ecarts au facteur attendu " + mismatches + ".");

            Assert.That(sawPartialContact, Is.True,
                "Le scenario doit reellement delester une PARTIE des roues (comptes observes : " + string.Join(", ", observedCounts)
                + ") : sans cela, la garde ne mesurerait qu'un vehicule pose sur ses quatre roues.");
            Assert.That(mismatches, Is.EqualTo(0),
                "L'autorite appliquee est exactement la proportion du nombre de roues au sol du pas precedent (ecarts mesures : "
                + mismatches + " sur " + samples + " pas).");
        }

        /// <summary>
        /// Perte des QUATRE contacts : le vehicule quitte le bord d'un plateau de 0,5 m. Attendu :
        /// autorite nulle, donc plus aucune direction ne bouge malgre une demande pleine ; l'attitude
        /// reste tenue (le vehicule ne se retourne pas) sans qu'aucune rotation soit ecrite ; la vitesse
        /// horizontale n'augmente plus (aucun couple moteur) ; et le vehicule retombe sur ses roues.
        /// </summary>
        [UnityTest]
        public IEnumerator LosingAllFourContactsAppliesNoAuthorityAndKeepsTheAttitudeStable()
        {
            BuildPlateau();

            yield return Drive(throttle: 1f, steer: 0f, steps: 130);

            var body = vehicle.GetComponent<Rigidbody>();

            var airborneSteps = 0;
            var speedAtTakeOff = 0f;
            var speedAtLanding = 0f;
            var steerAngleAtTakeOff = 0f;
            var steerDriftWhileAirborne = 0f;
            var lowestUpright = 1f;
            var wasAirborne = false;
            var landedAgain = false;

            for (var step = 0; step < MaxObservedSteps; step++)
            {
                // La demande de direction n'arrive qu'UNE FOIS EN L'AIR, et elle est alors PLEINE :
                // c'est ce qui rend la garde discriminante. Si l'autorite n'etait pas nulle, l'angle de
                // roue bondirait de pres de trente degres en sept pas ; avec l'autorite nulle, il ne
                // bouge pas du tout. Demander le braquage avant le saut ferait tourner le vehicule
                // pendant la montee de la rampe et rendrait le scenario indetermine.
                Command(throttle: 0.5f, steer: wasAirborne ? 1f : 0f, brakeReverse: 0f, handbrake: 0f);
                yield return new WaitForFixedUpdate();

                var authority = physics.GroundedAuthorityFactor;
                var airborneNow = authority <= 0f && physics.GroundedWheelCount == 0;

                Assert.That(float.IsNaN(body.linearVelocity.magnitude), Is.False, "La vitesse ne doit jamais devenir NaN en vol.");

                if (airborneNow)
                {
                    if (!wasAirborne)
                    {
                        wasAirborne = true;
                        airborneSteps = 0;
                        speedAtTakeOff = PlanarSpeed(body);
                        steerAngleAtTakeOff = physics.CurrentSteerAngleDegrees;
                    }

                    airborneSteps++;
                    steerDriftWhileAirborne = Mathf.Max(steerDriftWhileAirborne,
                        Mathf.Abs(physics.CurrentSteerAngleDegrees - steerAngleAtTakeOff));
                    lowestUpright = Mathf.Min(lowestUpright, Vector3.Dot(vehicle.transform.up, Vector3.up));
                }
                else if (wasAirborne && !landedAgain)
                {
                    landedAgain = true;
                    speedAtLanding = PlanarSpeed(body);
                }
            }

            Debug.Log("[Story513] perte des quatre contacts : " + airborneSteps + " pas en l'air, vitesse "
                + speedAtTakeOff.ToString("F2") + " -> " + speedAtLanding.ToString("F2") + " m/s, derive d'angle de roue pendant le vol "
                + steerDriftWhileAirborne.ToString("F3") + " deg, axe haut le plus incline "
                + lowestUpright.ToString("F3") + ", retombe sur ses roues : " + landedAgain + ".");

            Assert.That(airborneSteps, Is.GreaterThan(0),
                "Le scenario doit reellement mettre les quatre roues en l'air, sinon les assertions ci-dessous ne prouvent rien.");
            Assert.That(steerDriftWhileAirborne, Is.LessThan(0.01f),
                "Autorite de direction NULLE en vol : malgre une demande de braquage pleine, l'angle de roue ne bouge pas d'un millieme "
                + "de degre (derive mesuree " + steerDriftWhileAirborne.ToString("F4") + " deg). C'est la lecture litterale de l'AC, et "
                + "elle n'ecrit toujours aucune rotation.");
            Assert.That(lowestUpright, Is.GreaterThan(0.5f),
                "L'attitude reste tenue en vol : l'axe haut ne descend pas sous " + lowestUpright.ToString("F3")
                + " -- sans l'amortissement d'assiette, une caisse en l'air gardait l'inclinaison qu'un choc lui avait donnee.");
            Assert.That(speedAtLanding - speedAtTakeOff, Is.LessThan(1f),
                "Aucun couple moteur en vol, donc aucune acceleration : vitesse " + speedAtTakeOff.ToString("F2") + " -> "
                + speedAtLanding.ToString("F2") + " m/s. Un gain signifierait qu'une force est produite sans contact.");
            Assert.That(landedAgain, Is.True, "Et le vehicule retombe sur ses roues : le saut se termine, il ne se poursuit pas.");
        }

        // ---------------------------------------------------- geometrie du banc

        /// <summary>
        /// Le relief de recette, aux cotes AUTHOR EES dans `MVP_Run` : un dos-d'ane a crete de 0,12 m
        /// sur deux rampes douces, et une marche basse de 0,12 m, tous deux a pleine largeur de
        /// chaussee.
        /// </summary>
        private void BuildRecipeRelief()
        {
            props.Add(BuildRamp("Story513_BumpUp", new Vector3(0f, 0f, 8f), new Vector3(0f, CurbHeight, 9.2f), CarriagewayWidth, 0.3f));
            props.Add(BuildRamp("Story513_BumpDown", new Vector3(0f, CurbHeight, 9.2f), new Vector3(0f, 0f, 10.4f), CarriagewayWidth, 0.3f));

            props.Add(BuildBox("Story513_Step", new Vector3(0f, CurbHeight * 0.5f, 13f), new Vector3(CarriagewayWidth, CurbHeight, 1.6f)));
        }

        /// <summary>Rampe douce puis plateau de 0,5 m : c'est le bord du plateau qui met les quatre roues en l'air.</summary>
        private void BuildPlateau()
        {
            // Largeur volontairement grande : le plateau n'a pas a etre une chaussee authoree, il doit
            // seulement garantir que le vehicule quitte le bord par l'AVANT et pas par le cote.
            props.Add(BuildRamp("Story513_PlateauRamp", new Vector3(0f, 0f, 8f), new Vector3(0f, 0.5f, 10.4f), 30f, 0.3f));
            props.Add(BuildBox("Story513_Plateau", new Vector3(0f, 0.25f, 13.4f), new Vector3(30f, 0.5f, 6f)));
        }

        private GameObject BuildGround(float top)
        {
            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "Story513_Ground";
            slab.transform.localScale = new Vector3(1200f, 1f, 1200f);
            slab.transform.position = new Vector3(0f, top - 0.5f, 0f);
            return slab;
        }

        private GameObject BuildBox(string name, Vector3 centre, Vector3 size)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = centre;
            box.transform.localScale = size;
            return box;
        }

        /// <summary>
        /// Rampe dont la FACE SUPERIEURE va exactement de <paramref name="start"/> a
        /// <paramref name="end"/> : le solide est decale sous cette face, donc la surface roulante est
        /// celle demandee et pas une approximation.
        /// </summary>
        private GameObject BuildRamp(string name, Vector3 start, Vector3 end, float width, float thickness)
        {
            var run = new Vector3(end.x - start.x, 0f, end.z - start.z).magnitude;
            var rise = end.y - start.y;

            Assert.That(run, Is.GreaterThan(0.01f), name + " : une rampe a besoin d'une course horizontale.");
            Assert.That(rise, Is.GreaterThan(0f), name + " : une rampe monte.");

            var angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            var yaw = Mathf.Atan2(end.x - start.x, end.z - start.z) * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(-angle, yaw, 0f);
            var length = Mathf.Sqrt((run * run) + (rise * rise));

            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = name;
            piece.transform.rotation = rotation;
            piece.transform.localScale = new Vector3(width, thickness, length);
            piece.transform.position = ((start + end) * 0.5f) - (rotation * Vector3.up * (thickness * 0.5f));
            return piece;
        }

        // ---------------------------------------------------- conduite du banc

        /// <summary>
        /// Soumet l'intention a la couche physique, exactement comme le fait le controleur joueur : ni
        /// vitesse, ni position, ni rotation ne sont ecrites ici. C'est tout l'objet du banc.
        /// </summary>
        private void Command(float throttle, float steer, float brakeReverse, float handbrake)
        {
            var profile = physics.Profile;
            physics.ApplyDriveIntent(
                new VehicleDriveIntent(throttle, steer, brakeReverse, handbrake),
                profile.MaxForwardSpeed,
                profile.SteerRateDegreesPerSecond,
                profile.BrakeTorque);
        }

        private IEnumerator Drive(float throttle, float steer, int steps)
        {
            for (var step = 0; step < steps; step++)
            {
                Command(throttle, steer, 0f, 0f);
                yield return new WaitForFixedUpdate();
            }
        }

        private GameObject BuildVehicle()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);
            Assert.That(profile, Is.Not.Null, ProfilePath + " attendu");

            var root = new GameObject("Story513_Vehicle");
            root.SetActive(false);
            root.transform.position = new Vector3(0f, 0.5f, 0f);

            var box = root.AddComponent<BoxCollider>();
            box.center = VehicleColliderCenter;
            box.size = VehicleColliderSize;

            var body = root.AddComponent<Rigidbody>();
            body.mass = 1200f;
            body.linearDamping = 0.3f;
            body.angularDamping = 3f;

            var layer = root.AddComponent<VehiclePhysicsBody>();
            layer.BindProfile(profile);

            root.SetActive(true);
            return root;
        }

        private IEnumerator Settle(Rigidbody body)
        {
            for (var i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
                if (body.IsSleeping())
                {
                    break;
                }
            }
        }

        private static float PlanarSpeed(Rigidbody body)
        {
            return Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
        }
    }
}
