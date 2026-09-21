using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.14 : l'IA conduit par INTENTION, plus par ecriture.
    ///
    /// Quatre familles de gardes, toutes sans scene de jeu ni Netcode :
    ///
    /// 1. le chemin de conduite n'ECRIT plus le mouvement. C'est la garde centrale de la story : elle
    ///    scanne le corps de <c>FixedUpdate</c>, commentaires retires, et exige qu'aucune ecriture de
    ///    vitesse, de position ni de rotation n'y subsiste. La seule ecriture de vitesse qui reste dans
    ///    le fichier est la remise a zero de <c>RecoverAtWaypoint</c>, qui n'est pas de la conduite ;
    /// 2. la vitesse est LUE, plus entretenue : aucun champ de vitesse en boucle ouverte, aucun
    ///    integrateur, et la lecture sur le <c>Rigidbody</c> est presente ;
    /// 3. la decision de conduite de la Story 5.9 est intacte -- les quatre appels de <c>DriverModel</c>
    ///    et le diagnostic de profil absent restent en place. Un echec ici signale une fuite de la
    ///    physique dans la couche de decision ;
    /// 4. la traduction acceleration demandee -> POSITION DE PEDALE, verifiee sur une instance reelle
    ///    du prefab IA. Sans elle, la demande de 1,5 m/s2 du profil livre arriverait a plein gaz, soit
    ///    une acceleration dix fois superieure ; et sous le seuil de changement de sens, la couche
    ///    physique lirait l'axe de frein comme une MARCHE ARRIERE.
    ///
    /// Les comportements runtime de la story (une IA roule, la poussee la devie, le relief ne la fait
    /// plus decoller, les aides de la 5.13 l'affectent) ne sont PAS verifiables ici : ils appartiennent
    /// a la recette humaine. Aucun test PlayMode n'est ajoute, le harnais etant rouge et non filtrable.
    /// </summary>
    public sealed class Story514AiDrivesByIntentTests
    {
        private const string AiControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";
        private const string PlayerControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string AiPrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab";
        private const string PlayerPrefabPath = "Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab";
        private const string VehicleProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";

        private const float Epsilon = 0.0001f;

        // ------------------------------------------------ 1. le chemin de conduite n'ecrit plus

        /// <summary>
        /// Le coeur de la story : le pas de conduite ne pose plus de mouvement. Une ecriture qui
        /// reviendrait ici effacerait au pas suivant ce que la physique a gagne pendant un contact --
        /// donc une IA redevilerait impoussable -- et neutraliserait la gravite sur la composante
        /// verticale, donc une IA redecollerait sur le relief de recette.
        /// </summary>
        [Test]
        public void TheDrivingStepWritesNoVelocityPositionOrRotation()
        {
            var code = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));
            var fixedUpdate = ExtractMethodBody(code, "private void FixedUpdate()");

            Assert.That(fixedUpdate, Does.Not.Contain("linearVelocity ="),
                "Le pas de conduite LIT la vitesse et ne l'ecrit plus : c'est ce qui rend le vehicule poussable.");
            Assert.That(fixedUpdate, Does.Not.Contain("MoveRotation"),
                "Le lacet n'est plus impose a la caisse : il vient de l'angle de roue de la couche physique.");
            Assert.That(fixedUpdate, Does.Not.Contain("body.rotation"),
                "Aucune rotation de caisse ecrite depuis le pas de conduite.");
            Assert.That(fixedUpdate, Does.Not.Contain("body.position"),
                "Aucune position ecrite depuis le pas de conduite.");
            Assert.That(fixedUpdate, Does.Not.Contain("transform.SetPositionAndRotation"),
                "Aucun repositionnement depuis le pas de conduite.");

            Assert.That(Occurrences(code, "MoveRotation"), Is.EqualTo(0),
                "Aucun MoveRotation ne doit subsister dans le fichier : la loi de lacet a disparu, pas seulement son appel.");
        }

        /// <summary>
        /// L'unique ecriture de vitesse restante est la recuperation, qui n'est pas de la conduite. La
        /// borner ici distingue « la recuperation repositionne » (hors perimetre, et assume) de
        /// « la conduite ecrit » (le defaut de la story).
        /// </summary>
        [Test]
        public void TheOnlyRemainingVelocityWriteIsTheRecoveryReset()
        {
            var code = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));

            Assert.That(Occurrences(code, "linearVelocity ="), Is.EqualTo(1),
                "Une seule ecriture de vitesse dans le fichier : celle qui remet le vehicule au repos quand il est recupere.");

            var recover = ExtractMethodBody(code, "private void RecoverAtWaypoint(");
            Assert.That(recover, Does.Contain("body.linearVelocity = Vector3.zero;"),
                "Et c'est bien cette ecriture-la : la recuperation par teleportation, qui reste hors du perimetre de la story.");

            var fixedUpdate = ExtractMethodBody(code, "private void FixedUpdate()");
            Assert.That(Occurrences(fixedUpdate, "body.linearVelocity ="), Is.EqualTo(0),
                "Le pas de conduite ne remonte aucune ecriture de vitesse, meme a travers la recuperation qu'il appelle.");
        }

        /// <summary>
        /// Chemin unique vers la couche physique (AD-35) : le meme nom de point d'entree que le
        /// vehicule joueur, et aucune force appliquee sur place. Un second chemin -- une force locale,
        /// un jumeau de <c>ApplyDriveIntent</c> -- serait la porte par laquelle le mouvement
        /// recommencerait a etre ecrit ailleurs.
        /// </summary>
        [Test]
        public void TheIntentIsSubmittedToTheSamePhysicsLayerAsThePlayerVehicle()
        {
            var code = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));

            Assert.That(code, Does.Contain("physicsBody.ApplyDriveIntent("),
                "L'IA passe par le point d'entree unique de la couche physique, comme le joueur.");
            Assert.That(code, Does.Contain("private void SubmitIntentToPhysicsLayer("),
                "Sur le patron du controleur joueur -- meme nom, meme diagnostic de profil manquant.");
            Assert.That(code, Does.Not.Contain("AddForce"),
                "Aucune force appliquee depuis le controleur : la couche est la seule a produire des efforts.");
            Assert.That(code, Does.Not.Contain("WheelCollider"),
                "AD-35 : modele a raycasts par roue, pour les deux vehicules.");

            var player = File.ReadAllText(PlayerControllerSourcePath);
            Assert.That(player, Does.Contain("private void SubmitIntentToPhysicsLayer("),
                "Le chemin du joueur existe toujours : c'est celui que l'IA emprunte, pas un jumeau.");
        }

        /// <summary>
        /// Les deux prefabs portent le MEME composant physique et le MEME profil. Sans cette egalite,
        /// les aides arcade et la reduction d'autorite de la Story 5.13 ne s'appliqueraient pas au
        /// trafic « par construction », ce que l'AC de la story exige precisement.
        /// </summary>
        [Test]
        public void TheAiPrefabSharesTheSamePhysicsLayerAndProfileAsThePlayerPrefab()
        {
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var aiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AiPrefabPath);
            Assert.That(playerPrefab, Is.Not.Null, PlayerPrefabPath + " attendu");
            Assert.That(aiPrefab, Is.Not.Null, AiPrefabPath + " attendu");

            var playerBody = playerPrefab.GetComponent<VehiclePhysicsBody>();
            var aiBody = aiPrefab.GetComponent<VehiclePhysicsBody>();
            Assert.That(playerBody, Is.Not.Null, "Le prefab joueur porte la couche physique.");
            Assert.That(aiBody, Is.Not.Null,
                "AD-35 : le prefab IA doit porter la MEME couche physique. Sans elle, le nouveau chemin ne conduit nulle part.");

            var playerProfile = ResolveSerializedProfile(playerBody);
            var aiProfile = ResolveSerializedProfile(aiBody);
            Assert.That(aiProfile, Is.Not.Null,
                "Profil physique absent : le vehicule IA resterait inerte, et l'absence de mouvement se lirait comme un reglage.");
            Assert.That(AssetDatabase.GetAssetPath(aiProfile), Is.EqualTo(AssetDatabase.GetAssetPath(playerProfile)),
                "Les deux vehicules doivent tourner sur la meme enveloppe physique, sans quoi l'IA ne serait pas conduite par la couche du joueur.");
        }

        // ------------------------------------------------ 2. la vitesse est lue

        /// <summary>
        /// L'AC litterale : le champ de vitesse en boucle ouverte est supprime et la vitesse est relue
        /// du <c>Rigidbody</c>. Un champ remis ici -- meme « juste pour lisser » -- rendrait le vehicule
        /// impoussable au pas suivant, sans qu'aucune garde de comportement ne s'en apercoive en EditMode.
        /// </summary>
        [Test]
        public void TheOpenLoopSpeedFieldIsGoneAndTheSpeedIsReadFromTheRigidbody()
        {
            var code = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));

            Assert.That(code, Does.Not.Contain("private float currentSpeed"),
                "Plus aucun champ de vitesse entretenue par le controleur.");
            Assert.That(code, Does.Not.Contain("IntegrateLongitudinalSpeed"),
                "L'integrateur en boucle ouverte a disparu, il n'est pas laisse inerte.");
            Assert.That(code, Does.Not.Contain("ApplyMovement"),
                "Le chemin d'ecriture a disparu, il n'est pas laisse inerte.");

            var fixedUpdate = ExtractMethodBody(code, "private void FixedUpdate()");
            Assert.That(fixedUpdate, Does.Contain("Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up)"),
                "La vitesse est LUE sur le Rigidbody, projetee sur le plan.");
            Assert.That(fixedUpdate, Does.Contain("var currentSpeed = planarVelocity.magnitude;"),
                "Et elle est une variable LOCALE du pas : rien n'en survit au pas suivant.");
            Assert.That(fixedUpdate, Does.Contain("var longitudinalSpeed = Vector3.Dot(planarVelocity, ResolvePlanarForward());"),
                "La composante signee est lue sur le MEME vecteur : c'est celle que la couche physique lit pour choisir entre freiner et reculer.");
        }

        // ------------------------------------------------ 3. la decision de conduite est intacte

        /// <summary>
        /// Non-regression de la Story 5.9 (et de l'AD-33) : les appels du modele de conduite et le
        /// diagnostic de profil absent restent dans le controleur. La story change le chemin par lequel
        /// une decision atteint les roues, jamais la decision.
        /// </summary>
        [Test]
        public void TheDriverModelDecisionAndTheMissingProfileDiagnosticSurviveUnchanged()
        {
            var code = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));

            Assert.That(code, Does.Contain("DriverModel.ResolveEffectiveProfile(driverProfile.Profile, behavior)"),
                "Le profil effectif vient toujours du Def, module par la disposition publiee (AD-33).");
            Assert.That(code, Does.Contain("DriverModel.ComputeAcceleration("));
            Assert.That(code, Does.Contain("DriverModel.SmoothAcceleration("),
                "Le lissage garde sa memoire : c'est le temps de reaction du profil, donc une decision.");
            Assert.That(code, Does.Contain("DriverModel.ShouldEvaluateLaneChange("));
            Assert.That(code, Does.Contain("DriverModel.ResolveNoisyDesiredSpeed("));
            Assert.That(code, Does.Contain("DriverModel.IsDeliberateStop("),
                "L'arret voulu derriere un leader reste distingue du blocage.");
            Assert.That(code, Does.Contain("private RageDisposition ResolveBehavior()"),
                "Le comportement se resout comme avant : la story ne touche pas au chemin de la rage (AD-33).");

            Assert.That(code, Does.Contain("if (driverProfile == null)"));
            Assert.That(code, Does.Contain("warnedMissingDriverProfile"));
            Assert.That(code, Does.Not.Contain("new DriverProfile("),
                "Aucun repli code en dur : le vehicule reste inerte, comme quand la route est absente.");
        }

        /// <summary>
        /// La couche physique CONSOMME le dernier intent recu a chaque pas. Un chemin d'arret qui
        /// sortirait sans repousser l'intent neutre laisserait donc courir le plein gaz du pas
        /// precedent : le vehicule « inerte » continuerait d'accelerer. Meme garde que cote joueur.
        /// </summary>
        [Test]
        public void EveryInertExitOfTheDrivingStepPushesTheIdleIntent()
        {
            var code = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));
            var fixedUpdate = ExtractMethodBody(code, "private void FixedUpdate()");

            Assert.That(fixedUpdate, Does.Contain("laneGraph.NodeCount <= 0 || driverProfile == null"));
            Assert.That(Occurrences(fixedUpdate, "SubmitIntentToPhysicsLayer(VehicleDriveIntent.Idle)"), Is.EqualTo(1),
                "La garde commune graphe/profil absent soumet explicitement Idle.");
            Assert.That(fixedUpdate, Does.Contain("SubmitIntentToPhysicsLayer(ResolveStopIntent(longitudinalSpeed))"),
                "Une disposition immobilisante soumet le frein sans enclencher une marche arriere.");
            Assert.That(fixedUpdate, Does.Contain("SubmitIntentToPhysicsLayer(new VehicleDriveIntent(pedal.Throttle, seekIntent.Steer, pedal.BrakeReverse, pedal.Handbrake))"),
                "Et le chemin nominal soumet l'intent du pas, compose de la poursuite et de la decision longitudinale.");
        }

        /// <summary>
        /// La memoire de visee est purgee partout ou le vehicule est REPOSE a une pose connue. Sans
        /// cette purge, le vehicule recupere viserait l'ancien point pendant des dizaines de pas -- le
        /// saut de consigne que la Story 5.13 a supprime reviendrait, invisible, exactement sur le cas
        /// pour lequel il a ete ecrit.
        /// </summary>
        [Test]
        public void TheAimPointMemoryIsPurgedWhereverTheVehicleIsRepositioned()
        {
            var code = CodeWithoutComments(File.ReadAllText(AiControllerSourcePath));
            var reset = ExtractMethodBody(code, "private void ResetRouteMemoryAt(");
            var recover = ExtractMethodBody(code, "private void RecoverAtWaypoint(");

            Assert.That(reset, Does.Contain("hasAimPoint = false;"),
                "L'effacement vit dans ResetRouteMemoryAt, seul point commun des repositionnements.");
            Assert.That(reset, Does.Contain("previousAimPoint = Vector3.zero;"),
                "Et la cible elle-meme est purgee : garder le point en laissant le drapeau faux ferait repartir le rappel d'une cible perimee.");
            Assert.That(recover, Does.Contain("ResetRouteMemoryAt("),
                "La recuperation sur place repose le vehicule a plusieurs metres : c'est le cas qui compte.");
        }

        // ------------------------------------------------ 4. acceleration demandee -> pedale

        /// <summary>
        /// La demande d'acceleration du modele est traduite en FRACTION de la capacite du vehicule, pas
        /// envoyee telle quelle. Sans cette echelle, la demande de 1,5 m/s2 du profil livre partirait a
        /// plein gaz, soit l'enveloppe mesuree de la Story 5.12 (16,2 m/s2) : le conducteur ne serait
        /// plus conduit, il serait multiplie par la capacite du moteur.
        /// </summary>
        [Test]
        public void ARequestedAccelerationBecomesAPedalFractionOfTheVehicleCapacity()
        {
            WithAiVehicle((controller, physics) =>
            {
                var vehicle = physics.Profile;
                var capacity = ResolveDriveCapacity(vehicle);
                Assert.That(capacity, Is.GreaterThan(1f),
                    "Le profil livre doit avoir une capacite de conduite mesurable, sinon l'echelle de la pedale serait degeneree.");

                var intent = InvokePrivate<VehicleDriveIntent>(controller, "ResolvePedalIntent", 1.5f, 3f);

                Assert.That(intent.Throttle, Is.GreaterThan(0f), "Une demande positive donne un accelerateur engage.");
                var damping = controller.GetComponent<Rigidbody>().linearDamping;
                var netAcceleration = intent.Throttle * capacity * (1f - damping * Time.fixedDeltaTime) - damping * 3f;
                Assert.That(netAcceleration, Is.EqualTo(1.5f).Within(0.01f),
                    "La conversion doit conserver la demande NETTE apres l'amortissement du prefab.");
                Assert.That(intent.Throttle, Is.LessThan(0.25f),
                    "Donc tres loin du plein gaz -- c'est tout l'objet de cette echelle.");
                Assert.That(intent.BrakeReverse, Is.EqualTo(0f), "Accelerer et freiner ne se demandent pas ensemble.");
                Assert.That(intent.IsIdle, Is.False, "Une demande positive n'est pas un intent neutre.");
                return true;
            });
        }

        /// <summary>Le frein suit la meme regle d'echelle, sur la capacite de freinage du profil.</summary>
        [Test]
        public void ARequestedDecelerationBecomesABrakeFractionOfTheVehicleBrakeCapacity()
        {
            WithAiVehicle((controller, physics) =>
            {
                var vehicle = physics.Profile;
                var capacity = ResolveBrakeCapacity(vehicle);

                var intent = InvokePrivate<VehicleDriveIntent>(controller, "ResolvePedalIntent", -2f, vehicle.MinimumDirectionSpeed * 4f);

                Assert.That(intent.Throttle, Is.EqualTo(0f), "Freiner ne demande pas d'accelerateur.");
                Assert.That(intent.BrakeReverse, Is.GreaterThan(0f), "Au-dessus du seuil de changement de sens, la demande de ralentissement freine.");
                var damping = controller.GetComponent<Rigidbody>().linearDamping;
                var netAcceleration = -intent.BrakeReverse * capacity * (1f - damping * Time.fixedDeltaTime)
                    - damping * vehicle.MinimumDirectionSpeed * 4f;
                Assert.That(netAcceleration, Is.EqualTo(-2f).Within(0.01f));
                Assert.That(intent.BrakeReverse, Is.LessThan(0.25f), "Donc tres loin du freinage d'urgence.");
                return true;
            });
        }

        /// <summary>
        /// Sous le seuil de changement de sens, la couche physique ne lit plus le meme axe : l'entree de
        /// frein y devient une MARCHE ARRIERE. Le modele de conduite IA n'a aucune decision de recul --
        /// un vehicule qui freine pour s'arreter ne doit donc jamais partir en arriere parce qu'il a
        /// atteint l'arret.
        /// </summary>
        [Test]
        public void ASlowAiNeverEngagesTheReverseOfTheBrakeAxis()
        {
            WithAiVehicle((controller, physics) =>
            {
                var vehicle = physics.Profile;

                var atRest = InvokePrivate<VehicleDriveIntent>(controller, "ResolvePedalIntent", -2f, 0f);
                Assert.That(atRest.IsIdle, Is.True,
                    "A l'arret, la meme demande de ralentissement rend un intent neutre : au-dessous du seuil de changement de sens, "
                    + "l'axe de frein serait lu comme une marche arriere par la couche physique.");
                Assert.That(atRest.BrakeReverse, Is.EqualTo(0f));

                var reversing = InvokePrivate<VehicleDriveIntent>(controller, "ResolvePedalIntent", -2f, -vehicle.MinimumDirectionSpeed * 4f);
                Assert.That(reversing.IsIdle, Is.True,
                    "Un vehicule pousse en arriere ne recoit pas davantage de marche arriere : il ralentit par le frein moteur.");

                var rolling = InvokePrivate<VehicleDriveIntent>(controller, "ResolvePedalIntent", -2f, vehicle.MinimumDirectionSpeed * 4f);
                Assert.That(rolling.BrakeReverse, Is.GreaterThan(Epsilon),
                    "Au-dessus du seuil, la meme demande freine : la garde ne desactive pas le frein, elle l'empeche de devenir un recul.");
                return true;
            });
        }

        // ---------------------------------------------------------------- helpers

        /// <summary>
        /// Capacite de conduite du vehicule, recalculee depuis les MEMES donnees authorees que le
        /// controleur : c'est la grandeur contre laquelle la conversion est verifiee. Elle vaut 16,2
        /// m/s2 sur le profil livre, la valeur MESUREE de la Story 5.12.
        /// </summary>
        private static float ResolveDriveCapacity(VehicleProfile vehicle)
        {
            return ResolveScale(vehicle, vehicle.EngineTorque, onlyDrivenWheels: true);
        }

        /// <summary>Capacite de freinage : tous les essieux freinent, y compris ceux qui ne sont pas moteurs.</summary>
        private static float ResolveBrakeCapacity(VehicleProfile vehicle)
        {
            return ResolveScale(vehicle, vehicle.BrakeTorque, onlyDrivenWheels: false);
        }

        private static float ResolveScale(VehicleProfile vehicle, float torquePerWheel, bool onlyDrivenWheels)
        {
            var wheels = 0;
            var radiusSum = 0f;
            for (var i = 0; i < vehicle.WheelCount; i++)
            {
                var wheel = vehicle.GetWheel(i);
                radiusSum += wheel.Radius;
                if (!onlyDrivenWheels || wheel.IsDriven)
                {
                    wheels++;
                }
            }

            var meanRadius = vehicle.WheelCount > 0 ? radiusSum / vehicle.WheelCount : 0f;
            if (wheels == 0 || vehicle.Mass <= 0f || meanRadius <= 0f)
            {
                return 0f;
            }

            return wheels * torquePerWheel / (vehicle.Mass * meanRadius);
        }

        /// <summary>
        /// Inspecte le prefab IA sur une INSTANCE, comme la Story 5.11 : une instance d'asset ne
        /// declenche pas <c>Awake</c>, donc le profil physique est cable explicitement par
        /// <see cref="VehiclePhysicsBody.BindProfile"/> -- le chemin prevu pour un appelant qui compose
        /// un vehicule hors prefab (harnais de developpement, test).
        /// </summary>
        private static T WithAiVehicle<T>(Func<NetworkedAIVehicleDriverController, VehiclePhysicsBody, T> inspect)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AiPrefabPath);
            Assert.That(prefab, Is.Not.Null, AiPrefabPath + " attendu");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var controller = instance.GetComponent<NetworkedAIVehicleDriverController>();
                var physics = instance.GetComponent<VehiclePhysicsBody>();
                Assert.That(controller, Is.Not.Null, "Le controleur IA doit etre porte par le prefab.");
                Assert.That(physics, Is.Not.Null, "Le prefab IA doit porter la couche physique unique (AD-35).");

                var def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(VehicleProfilePath);
                Assert.That(def, Is.Not.Null, VehicleProfilePath + " attendu");
                physics.BindProfile(def);

                // Le controleur met sa couche en cache dans Awake, qui ne tourne pas sur une instance
                // d'asset : ce cablage nominal est repose explicitement, comme le fait OnNetworkSpawn.
                InvokePrivate<object>(controller, "CacheComponents");

                return inspect(controller, physics);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        /// <summary>
        /// Methode privee appelee par reflexion : la conversion acceleration -> pedale n'est pas un
        /// contrat public, et l'exposer pour la tester ouvrirait un second chemin vers la couche
        /// physique. C'est bien la fonction de PRODUCTION qui est mesuree ici, jamais une copie.
        /// </summary>
        private static T InvokePrivate<T>(object target, string methodName, params object[] arguments)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "methode introuvable : " + methodName);
            return (T)method.Invoke(target, arguments);
        }

        private static VehicleProfileDef ResolveSerializedProfile(VehiclePhysicsBody body)
        {
            var serialized = new SerializedObject(body);
            return serialized.FindProperty("vehicleProfile").objectReferenceValue as VehicleProfileDef;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "champ introuvable : " + fieldName);
            field.SetValue(target, value);
        }

        /// <summary>Retire les commentaires d'un source avant de le fouiller : le code DOCUMENTE ce qu'il ne fait plus.</summary>
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

            Assert.Fail("accolades non equilibrees dans " + signature);
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
