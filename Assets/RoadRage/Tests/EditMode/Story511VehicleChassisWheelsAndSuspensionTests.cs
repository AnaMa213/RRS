using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.11 : le chassis, les roues et la suspension.
    ///
    /// Quatre familles de gardes, toutes sans scene de jeu ni Netcode :
    /// 1. les fonctions pures de <see cref="VehicleSuspensionModel"/> et la matrice de cas limites
    ///    (roue au sol, roue en l'air, ecart de roulis nul) ;
    /// 2. l'authoring : profil complet et valide, chassis explicite, parite joueur/IA ;
    /// 3. les invariants de code que rien d'autre ne garde : aucun <c>WheelCollider</c> dans la
    ///    couche physique, aucun reglage de chassis serialise sur un controleur, disparition du
    ///    contournement de contact de surface ;
    /// 4. la geometrie de la bordure prototype sur le carrefour central -- aucun noeud de voie dans
    ///    son emprise, degagement de voie preserve, hauteur compatible avec le debattement.
    /// </summary>
    public sealed class Story511VehicleChassisWheelsAndSuspensionTests
    {
        /// <summary>
        /// Hauteur authoree de la bordure prototype, en metres. C'est LA valeur de contrat du kit
        /// artistique : la note de district la documente, ce test la garde, et les deux doivent
        /// bouger ensemble.
        /// </summary>
        public const float CurbHeight = 0.12f;

        /// <summary>Epaisseur authoree de la bordure, en metres (elle est entierement dans la bande trottoir).</summary>
        private const float CurbThickness = 0.3f;

        /// <summary>Demi-largeur de la chaussee d'un module (cotes figees de la Story 5.10).</summary>
        private const float RoadwayHalfWidth = 4f;

        /// <summary>Deport de l'axe de voie par rapport a l'axe de la chaussee (cotes figees).</summary>
        private const float LaneAxisOffset = 2f;

        /// <summary>Largeur du collider du vehicule (cotes figees, NFR18 : elle ne bouge pas).</summary>
        private const float VehicleWidth = 2.06f;

        /// <summary>Degagement attendu entre l'axe de voie et la face interieure de la bordure, en metres.</summary>
        private const float ExpectedLaneClearance = RoadwayHalfWidth - LaneAxisOffset - (VehicleWidth / 2f);

        private const float Epsilon = 0.0001f;

        /// <summary>Charge statique d'une roue du vehicule de reference (masse 1200 kg, 4 roues) : la borne de force en derive.</summary>
        private const float StaticLoad = 1200f * 9.81f / 4f;

        private const string ProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string PlayerPrefabPath = "Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab";
        private const string AiPrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab";
        private const string IntersectionPrefabPath = "Assets/RoadRage/Prefabs/Greybox_Intersection.prefab";
        private const string SegmentPrefabPath = "Assets/RoadRage/Prefabs/Greybox_RoadSegment_TwoWay.prefab";
        private const string PlayerControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs";
        private const string AiControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";
        private const string RunFlowSourcePath = "Assets/RoadRage/App/Run/RunFlowController.cs";
        private const string PhysicsBodySourcePath = "Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs";
        private const string SuspensionModelSourcePath = "Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs";
        private const string TelemetryViewSourcePath = "Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs";

        private static readonly string[] PhysicsLayerSourcePaths =
        {
            "Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs",
            "Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs",
            "Assets/RoadRage/Features/Vehicles/VehicleProfile.cs",
            "Assets/RoadRage/Features/Vehicles/VehicleWheel.cs",
            PlayerControllerSourcePath,
            AiControllerSourcePath
        };

        // ------------------------------------------------- matrice de cas limites (fonctions pures)

        [Test]
        public void WheelCompressionIsClampedToZeroAndToTravel()
        {
            // Roue au sol : compression = restLength - distance.
            Assert.That(VehicleSuspensionModel.ResolveCompression(true, 0.3f, 0.45f, 0.25f), Is.EqualTo(0.15f).Within(1e-5f),
                "Roue posee : la compression est la longueur au repos moins la distance effectivement parcourue.");

            // Roue au sol mais au-dela de la butee : bornee au debattement.
            Assert.That(VehicleSuspensionModel.ResolveCompression(true, 0.1f, 0.45f, 0.25f), Is.EqualTo(0.25f).Within(1e-5f),
                "Au-dela du debattement, la compression est bornee : la butee ne s'ecrase pas davantage.");

            // Rayon plus long que la longueur au repos : la roue pend, elle ne touche pas.
            Assert.That(VehicleSuspensionModel.IsWheelGrounded(true, 0.5f, 0.45f), Is.False,
                "Un rayon plus long que la longueur au repos signifie que la roue pend dans le vide.");
            Assert.That(VehicleSuspensionModel.ResolveCompression(false, 0.5f, 0.45f, 0.25f), Is.EqualTo(0f),
                "Roue en l'air : compression exactement nulle, jamais une valeur inventee qui alimenterait l'anti-roulis.");
        }

        [Test]
        public void AWheelWithNoGroundContactProducesNoSuspensionForce()
        {
            var grounded = VehicleSuspensionModel.ResolveSuspensionForce(0.1f, 0f, 32000f, 1900f, StaticLoad);
            var airborne = VehicleSuspensionModel.ResolveSuspensionForce(0f, 0f, 32000f, 1900f, StaticLoad);

            Assert.That(grounded, Is.GreaterThan(0f), "Une roue comprimee porte le vehicule.");
            Assert.That(airborne, Is.EqualTo(0f), "Aucun contact : aucune force, sans exception ni valeur de repli.");
        }

        [Test]
        public void SuspensionNeverPullsTheBodyDownwards()
        {
            // Detente rapide : l'amortisseur seul donnerait une force negative, donc une aspiration de
            // la caisse sous sa roue. Le total est borne a zero.
            var extending = VehicleSuspensionModel.ResolveSuspensionForce(0.05f, -20f, 32000f, 1900f, StaticLoad);

            Assert.That(extending, Is.EqualTo(0f),
                "Un ressort de suspension ne tire pas le chassis vers le sol : la detente ne peut pas aspirer la caisse.");
        }

        [Test]
        public void ASuspensionForceIsCappedByTheLoadTheWheelCarries()
        {
            // Choc violent : la force d'amortisseur vient d'une difference finie, donc rien ne la borne.
            // Sans plafond, elle devient un propulseur -- le vehicule decolle et continue de monter au
            // lieu de retomber, parce que chaque frame de contact re-injecte plus d'energie que la
            // gravite n'en retire.
            var violent = VehicleSuspensionModel.ResolveSuspensionForce(0.25f, 20f, 32000f, 1900f, StaticLoad);
            var cap = StaticLoad * 4f;

            // Le ressort seul a pleine course reste SOUS le plafond (2,7 fois la charge statique) : c'est
            // l'amortisseur, alimente par une difference finie, qui fait sauter la borne. Autrement dit,
            // le plafond ne change rien au repos -- il n'agit que dans le choc, qui est le probleme.
            Assert.That((32000f * 0.25f) + (1900f * 20f), Is.GreaterThan(cap),
                "Le cas de test doit vraiment depasser le plafond, sinon il ne prouve rien.");
            Assert.That(violent, Is.EqualTo(cap).Within(0.01f),
                "Une suspension ne rend pas plus de quatre fois la charge qu'elle porte : c'est la borne qui empeche le decollage.");

            var gentle = VehicleSuspensionModel.ResolveSuspensionForce(0.05f, 0f, 32000f, 1900f, StaticLoad);
            Assert.That(gentle, Is.LessThan(cap), "Sous le plafond, la force reste celle du ressort : le plafond ne fausse pas le repos.");
            Assert.That(gentle, Is.EqualTo(32000f * 0.05f).Within(0.01f));

            Assert.That(VehicleSuspensionModel.ResolveStaticLoad(1200f, -9.81f, 4), Is.EqualTo(StaticLoad).Within(0.5f),
                "La charge statique derive de la masse et du nombre de roues : la borne suit le vehicule, elle n'est pas un nombre absolu.");
        }

        [Test]
        public void GroundFrictionOpposesASlideAndIsCappedByTheLoadCarried()
        {
            var forward = Vector3.forward;
            var right = Vector3.right;

            // Glissement de flanc a 8 m/s : le frottement lateral s'y oppose, borne par la charge.
            var sliding = VehicleSuspensionModel.ResolveGroundFrictionForce(
                right * 8f, forward, right, StaticLoad, 1.2f, 0.03f);

            Assert.That(Vector3.Dot(sliding, right), Is.LessThan(0f),
                "Le frottement lateral s'oppose au glissement de travers : sans lui, un choc de flanc emporte le vehicule sur des metres.");
            Assert.That(Vector3.Dot(sliding, right), Is.EqualTo(-1.2f * StaticLoad).Within(0.5f),
                "Un pneu ne transmet pas plus que ce que le sol lui rend : la force est bornee par la charge portee.");
            Assert.That(Vector3.Dot(sliding, forward), Is.EqualTo(0f).Within(0.01f),
                "Pas de vitesse dans l'axe : pas de resistance au roulement.");

            // A l'arret, aucun frottement : sinon le vehicule brouterait au lieu de s'immobiliser.
            Assert.That(VehicleSuspensionModel.ResolveGroundFrictionForce(
                Vector3.zero, forward, right, StaticLoad, 1.2f, 0.03f), Is.EqualTo(Vector3.zero));

            // Pres de l'arret, le frottement s'attenue progressivement.
            var creeping = VehicleSuspensionModel.ResolveGroundFrictionForce(
                right * 0.05f, forward, right, StaticLoad, 1.2f, 0.03f);
            Assert.That(Mathf.Abs(Vector3.Dot(creeping, right)), Is.LessThan(1.2f * StaticLoad * 0.2f),
                "Sous la vitesse d'attenuation, la force decroit avec la vitesse : c'est ce qui evite le broutement a l'arret.");

            // Resistance au roulement : faible, et elle ne doit pas lutter contre la conduite.
            var rolling = VehicleSuspensionModel.ResolveGroundFrictionForce(
                forward * 8f, forward, right, StaticLoad, 1.2f, 0.03f);
            Assert.That(Vector3.Dot(rolling, forward), Is.LessThan(0f));
            Assert.That(Mathf.Abs(Vector3.Dot(rolling, forward)), Is.LessThan(0.05f * StaticLoad),
                "La resistance au roulement reste marginale devant la poussee du moteur : c'est un contact, pas un frein.");
        }

        [Test]
        public void AntiRollLoadsTheMostCompressedSideAndIsZeroWhenLevel()
        {
            VehicleSuspensionModel.ResolveAntiRollForces(0.1f, 0.1f, 20000f, out var levelLeft, out var levelRight);
            Assert.That(levelLeft, Is.EqualTo(0f), "Essieu a plat : l'anti-roulis ne fait rien.");
            Assert.That(levelRight, Is.EqualTo(0f));

            VehicleSuspensionModel.ResolveAntiRollForces(0.12f, 0.10f, 20000f, out var leftForce, out var rightForce);
            Assert.That(leftForce, Is.GreaterThan(0f),
                "Le cote le plus comprime est celui ou la caisse s'affaisse : c'est lui qu'il faut CHARGER.");
            Assert.That(rightForce, Is.EqualTo(-leftForce).Within(1e-4f),
                "La paire est un couple : ce qui est charge d'un cote est soulage de l'autre.");

            VehicleSuspensionModel.ResolveAntiRollForces(0.16f, 0.10f, 20000f, out var largeLeft, out _);
            VehicleSuspensionModel.ResolveAntiRollForces(0.13f, 0.10f, 20000f, out var smallLeft, out _);
            Assert.That(largeLeft / smallLeft, Is.EqualTo(2f).Within(1e-4f),
                "Le terme est strictement proportionnel a l'ecart. C'est ce qui fait qu'il REDUIT le roulis sans "
                + "l'annuler : en virage l'equilibre s'etablit sur un ecart non nul, donc la caisse s'incline toujours.");

            VehicleSuspensionModel.ResolveAntiRollForces(0.10f, 0.16f, 20000f, out var reversedLeft, out var reversedRight);
            Assert.That(reversedLeft, Is.LessThan(0f), "Sens inverse quand la droite est la plus comprimee.");
            Assert.That(reversedRight, Is.GreaterThan(0f));
        }

        [Test]
        public void AntiRollPairIsAppliedWithoutReinterpretingItsSign()
        {
            // Le sens de la paire a deja ete inverse une fois par l'appelant : l'anti-roulis chargeait le
            // cote SOULAGE, donc il entretenait l'inclinaison au lieu de la corriger, et un vehicule
            // touche restait couche sur le flanc. La paire rendue par la fonction pure ne doit donc etre
            // reinterprettee nulle part -- ni signe inverse, ni echange gauche/droite.
            var source = CodeWithoutComments(File.ReadAllText(PhysicsBodySourcePath));
            var antiRoll = ExtractMethodBody(source, "private void ApplyAntiRoll(Vector3 up)");

            Assert.That(antiRoll, Does.Contain("ResolveAntiRollForces("),
                "Les forces viennent de la fonction pure, jamais d'un calcul refait sur place.");
            Assert.That(antiRoll, Does.Contain("up * leftForce"),
                "Le cote gauche est charge de 'leftForce', tel quel.");
            Assert.That(antiRoll, Does.Contain("up * rightForce"),
                "Le cote droit est charge de 'rightForce', tel quel.");
            Assert.That(antiRoll, Does.Not.Contain("-leftForce"), "Aucune inversion de signe sur le cote gauche.");
            Assert.That(antiRoll, Does.Not.Contain("-rightForce"), "Aucune inversion de signe sur le cote droit.");
        }

        [Test]
        public void LevellingTorqueIsZeroWhenLevelAndPushesBackTowardsTheVertical()
        {
            Assert.That(VehicleSuspensionModel.ResolveLevellingTorque(Vector3.up, Vector3.up, 18000f), Is.EqualTo(Vector3.zero),
                "Caisse d'aplomb : aucun couple de rappel, donc aucune oscillation entretenue au repos.");

            var tilted = Vector3.Normalize(Vector3.up + (Vector3.right * 0.5f));
            var torque = VehicleSuspensionModel.ResolveLevellingTorque(tilted, Vector3.up, 18000f);

            Assert.That(torque.sqrMagnitude, Is.GreaterThan(0f), "Caisse inclinee : un couple doit la ramener.");

            // Le couple produit une vitesse angulaire qui ramene l'axe haut vers la verticale :
            // d(up)/dt = torque x up doit avoir une composante positive sur worldUp.
            var correction = Vector3.Dot(Vector3.Cross(torque, tilted).normalized, Vector3.up);

            Assert.That(correction, Is.GreaterThan(0f),
                "Le couple doit RAMENER la caisse vers la verticale, jamais l'en ecarter : c'est exactement "
                + "l'erreur de signe qui faisait pencher les vehicules au lieu de les redresser.");

            var scaled = VehicleSuspensionModel.ResolveLevellingTorque(tilted, Vector3.up, 9000f);
            Assert.That(scaled.magnitude, Is.EqualTo(torque.magnitude / 2f).Within(1e-4f),
                "Le couple est proportionnel au reglage authore.");
        }

        [Test]
        public void AttitudeDampingNeverTouchesYaw()
        {
            var forward = Vector3.forward;
            var right = Vector3.right;

            var yawing = VehicleSuspensionModel.ResolveAttitudeDampingTorque(Vector3.up * 5f, forward, right, 6000f);
            Assert.That(yawing, Is.EqualTo(Vector3.zero),
                "Le lacet porte la direction : l'amortir retirerait au conducteur son autorite sur le cap.");

            var rolling = VehicleSuspensionModel.ResolveAttitudeDampingTorque(forward * 2f, forward, right, 6000f);
            Assert.That(Vector3.Dot(rolling, forward), Is.LessThan(0f),
                "Le roulis est amorti, et dans le sens qui s'oppose a la rotation.");

            var pitching = VehicleSuspensionModel.ResolveAttitudeDampingTorque(right * 2f, forward, right, 6000f);
            Assert.That(Vector3.Dot(pitching, right), Is.LessThan(0f), "Le tangage aussi.");

            var mixed = VehicleSuspensionModel.ResolveAttitudeDampingTorque((forward * 2f) + (Vector3.up * 5f), forward, right, 6000f);
            Assert.That(Vector3.Dot(mixed, Vector3.up), Is.EqualTo(0f).Within(1e-6f),
                "Le lacet present dans une vitesse angulaire mixte n'est pas amorti pour autant.");
        }

        [Test]
        public void SurfaceContactDiscriminatesASidewalkFromAWall()
        {
            // Le dessous du vehicule au repos, mesure sur le prefab joueur : c'est lui qui decide.
            const float underside = 0.158f;

            // La tolerance est LUE dans le profil authore, jamais redeclaree : un test qui se donne sa
            // propre valeur reste vert quand l'authoring derive, et la bande sans degat pourrait alors
            // s'elargir jusqu'a couvrir des chocs bas entre vehicules sans qu'aucune assertion ne bouge.
            var def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);
            var tolerance = def.Profile.SurfaceContactTolerance;

            Assert.That(VehicleSuspensionModel.IsSurfaceContact(CurbHeight, underside, tolerance), Is.True,
                "Une bordure authoree de 0,12 m est un relief franchi : elle ne doit produire aucun degat.");
            Assert.That(VehicleSuspensionModel.IsSurfaceContact(-0.05f, underside, tolerance), Is.True,
                "La levre de 5 cm du plan de sol passe SOUS le dessous du vehicule.");
            Assert.That(VehicleSuspensionModel.IsSurfaceContact((underside + tolerance) - 0.001f, underside, tolerance), Is.True,
                "Juste sous la tolerance : dernier point encore accepte.");
            Assert.That(VehicleSuspensionModel.IsSurfaceContact((underside + tolerance) + 0.001f, underside, tolerance), Is.False,
                "Juste au-dessus : c'est un obstacle, donc un vrai choc. La marge de 1 mm evite de tester une egalite "
                + "flottante a la limite exacte, qui depend de l'arrondi du compilateur et non de la regle.");
            Assert.That(VehicleSuspensionModel.IsSurfaceContact(0.45f, underside, tolerance), Is.False,
                "Un pare-chocs adverse touche bien plus haut que le dessous : les degats de la Story 3.5 restent appliques.");
            Assert.That(VehicleSuspensionModel.IsSurfaceContact(5f, underside, tolerance), Is.False,
                "Paroi de tunnel : aucun contact de surface, le choc blesse.");
        }

        [Test]
        public void TelemetrySampleReportsBodyStateAndHandlesRest()
        {
            var forward = Vector3.forward;
            var right = Vector3.right;
            var sample = VehicleSuspensionModel.SampleTelemetry((forward * 10f) + (right * 1f), forward, right);

            Assert.That(sample.Speed, Is.EqualTo(Mathf.Sqrt(101f)).Within(1e-3f));
            Assert.That(sample.LongitudinalSpeed, Is.EqualTo(10f).Within(1e-3f));
            Assert.That(sample.LateralSpeed, Is.EqualTo(1f).Within(1e-3f));
            Assert.That(sample.Slip, Is.EqualTo(1f / Mathf.Sqrt(101f)).Within(1e-3f));
            Assert.That(sample.SlipAngleDegrees, Is.EqualTo(Mathf.Rad2Deg * Mathf.Atan2(1f, 10f)).Within(0.01f));

            var atRest = VehicleSuspensionModel.SampleTelemetry(Vector3.zero, forward, right);
            Assert.That(atRest.Speed, Is.EqualTo(0f));
            Assert.That(atRest.Slip, Is.EqualTo(0f), "A l'arret, le glissement vaut 0 : jamais un rapport sur une vitesse nulle.");
            Assert.That(atRest.SlipAngleDegrees, Is.EqualTo(0f));

            var verticalOnly = VehicleSuspensionModel.SampleTelemetry(Vector3.up * 5f, forward, right);
            Assert.That(verticalOnly.Speed, Is.EqualTo(0f),
                "La telemetrie de caisse est planaire : une chute ne compte pas comme une vitesse de deplacement.");
        }

        // ------------------------------------------------- authoring du profil et du chassis

        [Test]
        public void DefaultProfileIsFullyAuthoredAndValid()
        {
            var def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);
            Assert.That(def, Is.Not.Null, ProfilePath + " attendu (AD-12, AD-25)");

            string error;
            Assert.That(def.TryValidate(out error), Is.True, "Le profil authore doit etre valide : " + error);
            Assert.That(def.RawId, Is.EqualTo(def.RawId.ToLowerInvariant()), "NFR13 : id stable en minuscules.");

            var profile = def.Profile;
            Assert.That(profile.Mass, Is.GreaterThan(0f), "La masse est authoree, jamais implicite.");
            Assert.That(profile.InertiaTensor.x, Is.GreaterThan(0f));
            Assert.That(profile.InertiaTensor.y, Is.GreaterThan(0f));
            Assert.That(profile.InertiaTensor.z, Is.GreaterThan(0f));
            Assert.That(profile.WheelCount, Is.EqualTo(4), "Un chassis a quatre roues.");
            Assert.That(profile.SpringRate, Is.GreaterThan(0f), "Raideur de ressort authoree.");
            Assert.That(profile.Damper, Is.GreaterThan(0f), "Amortissement authore.");
            Assert.That(profile.AntiRollRate, Is.GreaterThan(0f), "Anti-roulis authore.");
            Assert.That(profile.RestLength, Is.GreaterThan(profile.Travel), "La course utile tient dans la longueur au repos.");
            Assert.That(profile.GroundMask.value, Is.Not.EqualTo(0), "Sans masque de sol, aucune roue ne peut toucher.");
            Assert.That(profile.AttitudeLevellingRate, Is.GreaterThan(0f),
                "Le rappel d'assiette est author e : sans lui un vehicule couche ne se redresse jamais, parce que les "
                + "rayons de roue ne touchent plus des que la caisse s'incline trop.");
            Assert.That(profile.AttitudeDamping, Is.GreaterThan(0f), "Le tangage et le roulis sont amortis.");
            Assert.That(profile.LateralFrictionCoefficient, Is.GreaterThan(0f),
                "Un frottement lateral nul laisse le vehicule glisser comme sur de la glace des qu'un choc le prend de flanc.");
            Assert.That(profile.RollingResistanceCoefficient, Is.GreaterThan(0f), "La resistance au roulement est author ee.");
            Assert.That(profile.SurfaceContactTolerance, Is.GreaterThanOrEqualTo(CurbHeight),
                "La tolerance de contact de surface doit couvrir la bordure authoree, sinon monter une bordure blesse.");
        }

        [Test]
        public void ProfileValidationNamesTheFieldItRefuses()
        {
            var def = ScriptableObject.CreateInstance<VehicleProfileDef>();
            try
            {
                var serialized = new SerializedObject(def);
                var idProperty = serialized.FindProperty("id");

                idProperty.stringValue = "MiXeD";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(def.TryValidate(out var idError), Is.False);
                Assert.That(idError, Does.Contain("Id"), "Un id non minuscule est refuse en le nommant.");

                idProperty.stringValue = "vehicle_test";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(def.TryValidate(out _), Is.True, "Le profil par defaut est valide : les refus ci-dessous viennent du champ modifie.");

                var profileProperty = serialized.FindProperty("profile");
                profileProperty.FindPropertyRelative("mass").floatValue = 0f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(def.TryValidate(out var massError), Is.False);
                Assert.That(massError, Does.Contain("Mass"), "Une masse nulle est refusee en nommant le champ.");

                profileProperty.FindPropertyRelative("mass").floatValue = 1200f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(def.TryValidate(out _), Is.True);

                var travelBefore = profileProperty.FindPropertyRelative("travel").floatValue;
                profileProperty.FindPropertyRelative("travel").floatValue = profileProperty.FindPropertyRelative("restLength").floatValue;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(def.TryValidate(out var travelError), Is.False);
                Assert.That(travelError, Does.Contain("RestLength"),
                    "Sans course utilisable, la roue resterait sur sa butee : le refus nomme la longueur au repos.");

                profileProperty.FindPropertyRelative("travel").floatValue = travelBefore;
                profileProperty.FindPropertyRelative("groundMask").intValue = 0;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(def.TryValidate(out var maskError), Is.False);
                Assert.That(maskError, Does.Contain("GroundMask"), "Un masque de sol vide est refuse : aucune roue ne toucherait jamais le sol.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void BothVehiclePrefabsCarryTheSamePhysicsComponentConfiguredByTheSameAuthoredProfile()
        {
            var authored = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);
            var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var ai = AssetDatabase.LoadAssetAtPath<GameObject>(AiPrefabPath);

            Assert.That(player, Is.Not.Null, PlayerPrefabPath + " attendu");
            Assert.That(ai, Is.Not.Null, AiPrefabPath + " attendu");

            var playerBody = player.GetComponent<VehiclePhysicsBody>();
            var aiBody = ai.GetComponent<VehiclePhysicsBody>();

            Assert.That(playerBody, Is.Not.Null, "AD-35 : le prefab joueur porte le composant physique partage.");
            Assert.That(aiBody, Is.Not.Null, "AD-35 : le prefab IA porte LE MEME composant physique, pas un equivalent.");
            Assert.That(aiBody.GetType(), Is.EqualTo(playerBody.GetType()));

            var playerProfile = ResolveSerializedProfile(playerBody);
            var aiProfile = ResolveSerializedProfile(aiBody);

            Assert.That(playerProfile, Is.SameAs(authored), "Le prefab joueur est configure par le profil authore, pas par des champs serialises.");
            Assert.That(aiProfile, Is.SameAs(authored), "Le prefab IA prend le meme profil : deux profils divergeraient en silence.");
        }

        [Test]
        public void MassCenterOfMassAndInertiaAreExplicitOnBothPrefabs()
        {
            var def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);

            foreach (var path in new[] { PlayerPrefabPath, AiPrefabPath })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var body = prefab.GetComponent<Rigidbody>();

                Assert.That(body, Is.Not.Null, path + " attendu avec un Rigidbody");
                Assert.That(body.automaticCenterOfMass, Is.False,
                    path + " : le centre de masse implicite est exactement ce que la story supprime.");
                Assert.That(body.automaticInertiaTensor, Is.False,
                    path + " : le tenseur d'inertie est authore, jamais deduit du collider.");
                Assert.That(body.mass, Is.EqualTo(def.Profile.Mass).Within(0.01f),
                    path + " : la masse du prefab est celle du profil, pas une seconde verite.");

                // Les valeurs elles-memes se lisent dans le YAML. Sur un prefab non instancie, Unity
                // ne rend pas un Rigidbody exploitable -- le collider n'est pas enregistre dans la
                // scene physique -- donc `centerOfMass` y vaut (0,0,0) quelle que soit la valeur
                // authoree. Une assertion runtime y serait donc fausse : ni la preuve de l'authoring,
                // ni une garde utile.
                var yaml = File.ReadAllText(path);
                Assert.That(yaml, Does.Contain("m_ImplicitCom: 0"),
                    path + " : 'm_ImplicitCom: 0' est la trace authoree que le centre de masse n'est plus implicite.");
                Assert.That(yaml, Does.Contain("m_ImplicitTensor: 0"),
                    path + " : 'm_ImplicitTensor: 0' est la trace authoree que le tenseur n'est plus deduit.");

                var centerOfMass = ReadVector3(yaml, "m_CenterOfMass");
                Assert.That(centerOfMass.y, Is.EqualTo(def.Profile.CenterOfMass.y).Within(0.0001f),
                    path + " : le centre de masse pose dans le prefab est celui du profil.");

                var inertia = ReadVector3(yaml, "m_InertiaTensor");
                Assert.That(inertia.x, Is.EqualTo(def.Profile.InertiaTensor.x).Within(0.01f));
                Assert.That(inertia.y, Is.EqualTo(def.Profile.InertiaTensor.y).Within(0.01f));
                Assert.That(inertia.z, Is.EqualTo(def.Profile.InertiaTensor.z).Within(0.01f));
            }
        }

        // ------------------------------------------------- invariants de code

        [Test]
        public void NoWheelColliderIsUsedByThePhysicsLayer()
        {
            foreach (var path in PhysicsLayerSourcePaths)
            {
                var code = CodeWithoutComments(File.ReadAllText(path));
                Assert.That(code, Does.Not.Contain("WheelCollider"),
                    path + " : AD-35 impose un modele a raycasts par roue. Un WheelCollider ne se prouve que pendant "
                    + "un pas de physique, donc hors de la suite EditMode ou les fonctions pures se verifient.");
            }

            foreach (var path in new[] { PlayerPrefabPath, AiPrefabPath })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab.GetComponentsInChildren<WheelCollider>(true), Is.Empty,
                    path + " : aucun WheelCollider sur le vehicule.");
            }
        }

        [Test]
        public void NoChassisOrSuspensionTuningRemainsSerializedOnTheControllers()
        {
            // Les seuils de recuperation (retournement, vide, blocage) ne sont PAS des parametres de
            // chassis : ils restent ou ils sont. Ce test ne garde que ce qui a migre dans le profil.
            string[] migratedMembers = { "rollStabilityAssist", "centerOfMassOffset" };

            foreach (var path in new[] { PlayerControllerSourcePath, AiControllerSourcePath })
            {
                var source = File.ReadAllText(path);
                foreach (var member in migratedMembers)
                {
                    Assert.That(source, Does.Not.Contain(member),
                        path + " : '" + member + "' a migre dans VehicleProfileDef. Un reglage de chassis survive "
                        + "ici redeviendrait la seconde source de verite que la story supprime.");
                }
            }

            foreach (var path in new[] { PlayerPrefabPath, AiPrefabPath })
            {
                var yaml = File.ReadAllText(path);
                foreach (var member in migratedMembers)
                {
                    Assert.That(yaml, Does.Not.Contain(member + ":"),
                        path + " : la valeur serialisee de '" + member + "' doit disparaitre avec le champ.");
                }
            }
        }

        [Test]
        public void TheSurfaceContactRuleIsProfileDrivenAndTheDamageGuardIsUntouched()
        {
            var driver = File.ReadAllText(PlayerControllerSourcePath);
            var code = CodeWithoutComments(driver);

            Assert.That(code, Does.Contain("IsSurfaceOnlyCollision(collision)"),
                "Le tri des entrees de collision par hauteur de contact precede l'evenement de degats.");
            Assert.That(code.IndexOf("IsSurfaceOnlyCollision(collision)", StringComparison.Ordinal),
                Is.LessThan(code.IndexOf("VehicleCollided?.Invoke(impactSpeed)", StringComparison.Ordinal)),
                "Le tri precede l'evenement -- donc sa RPC, donc les degats vehicule et occupants.");
            Assert.That(code, Does.Contain("physicsBody.Profile.SurfaceContactTolerance"),
                "La tolerance vient du profil physique authore, jamais d'une constante du controleur : le meme "
                + "reglage vaut pour la voiture joueur et pour tout vehicule IA.");
            Assert.That(driver, Does.Not.Contain("private float surfaceContactTolerance"),
                "La valeur n'est plus un champ serialise du controleur : elle a migre dans le profil (AC de la story).");

            var model = CodeWithoutComments(File.ReadAllText(SuspensionModelSourcePath));
            Assert.That(model, Does.Contain("IsSurfaceContact"),
                "Le predicat vit dans la couche de fonctions pures, ou il se prouve en EditMode.");

            var runFlow = File.ReadAllText(RunFlowSourcePath);
            Assert.That(runFlow, Does.Contain("if (vehicleDamage > 0)"),
                "Le garde 'vehicleDamage > 0' de RunFlowController reste intact : seuil independant et toujours valide, "
                + "jamais un contournement de la couche physique.");
        }

        // ------------------------------------------------- bordure prototype

        [Test]
        public void TheCurbIsAuthoredOnTheCrossroadsAndNeverOnAStraight()
        {
            var junctionCurbCount = WithInstantiated(IntersectionPrefabPath, instance => FindCurbColliders(instance).Count);

            Assert.That(junctionCurbCount, Is.GreaterThan(0),
                "La bordure prototype est authoree sur le carrefour central (Story 5.11, AC de verification).");

            WithInstantiated(IntersectionPrefabPath, instance =>
            {
                foreach (var collider in FindCurbColliders(instance))
                {
                    Assert.That(collider.enabled, Is.True, collider.name + " : un collider de bordure desactive ne prouve rien.");
                }

                return true;
            });

            WithInstantiated(SegmentPrefabPath, instance =>
            {
                Assert.That(FindCurbColliders(instance), Is.Empty,
                    "Aucune bordure sur une ligne droite : les vehicules coupent l'interieur des virages, donc c'est sur une "
                    + "jonction que la bordure se prouve -- et une droite n'a jamais ete le cas a verifier.");
                return true;
            });
        }

        [Test]
        public void TheCurbStaysInsideTheSidewalkBandAndLeavesTheAuthoredLaneClearance()
        {
            WithInstantiated(IntersectionPrefabPath, instance =>
            {
                var bounds = FindCurbColliders(instance).Select(collider => collider.bounds).ToList();
                Assert.That(bounds, Is.Not.Empty, "La bordure prototype doit exister pour que ses cotes soient verifiees.");

                foreach (var curb in bounds)
                {
                    var innerFace = Mathf.Min(Mathf.Abs(curb.min.z), Mathf.Abs(curb.max.z));

                    // La bordure est entierement du cote trottoir de l'arete de chaussee : aucune partie
                    // dans les 8 m de chaussee.
                    var insideRoadway = curb.min.z > -RoadwayHalfWidth && curb.max.z < RoadwayHalfWidth;
                    Assert.That(insideRoadway, Is.False,
                        "La bordure reste dans la bande trottoir de 4 m, jamais dans les 8 m de chaussee (son emprise z est "
                        + curb.min.z.ToString("F3") + " .. " + curb.max.z.ToString("F3") + ").");

                    Assert.That(Mathf.Abs(innerFace - RoadwayHalfWidth), Is.LessThan(0.001f),
                        "La face interieure de la bordure est exactement a l'arete de chaussee, donc le degagement de voie "
                        + "est celui des cotes figees (mesure : " + innerFace.ToString("F3") + " m).");

                    Assert.That(curb.size.z, Is.EqualTo(CurbThickness).Within(0.001f),
                        "Epaisseur authoree de la bordure.");

                    Assert.That(curb.max.y, Is.EqualTo(CurbHeight).Within(0.001f),
                        "Hauteur authoree de la bordure -- la valeur contractuelle du kit artistique.");
                    Assert.That(curb.min.y, Is.EqualTo(0f).Within(0.001f),
                        "La bordure repose sur le plan de roulage, elle ne flotte pas.");
                }

                return true;
            });

            Assert.That(ExpectedLaneClearance, Is.EqualTo(0.97f).Within(0.001f),
                "Cotes figees : 2 m de l'axe de voie a l'arete de chaussee contre un vehicule de 2,06 m, soit 0,97 m par cote. "
                + "Si cette valeur bouge, ce n'est plus la meme geometrie qui est verifiee.");
        }

        [Test]
        public void NoLaneNodeFallsOnOrInsideTheCurb()
        {
            WithInstantiated(IntersectionPrefabPath, instance =>
            {
                var curb = FindCurbColliders(instance);
                var nodes = instance.GetComponentsInChildren<LaneNode>(true);

                Assert.That(nodes.Length, Is.GreaterThan(0), "Le carrefour porte ses propres noeuds de voie.");

                foreach (var node in nodes)
                {
                    var position = node.transform.position;
                    foreach (var collider in curb)
                    {
                        var footprint = collider.bounds;
                        Assert.That(footprint.Contains(new Vector3(position.x, footprint.center.y, position.z)), Is.False,
                            node.name + " tombe dans l'emprise de " + collider.name
                            + ". Le graphe de voies n'est pas modifie par cette story : une bordure qui avale un noeud "
                            + "deplacerait le trace, ce que l'AC interdit.");
                    }
                }

                return true;
            });
        }

        [Test]
        public void TheVehicleUndersideAndTheSuspensionTravelClearTheAuthoredCurb()
        {
            var def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);
            var profile = def.Profile;

            Assert.That(profile.Travel, Is.GreaterThan(CurbHeight),
                "Le debattement doit couvrir la hauteur de bordure : sinon la roue reste sur sa butee et ne peut pas monter.");

            WithInstantiated(PlayerPrefabPath, instance =>
            {
                var collider = instance.GetComponent<Collider>();
                Assert.That(collider, Is.Not.Null, "Le prefab joueur porte son collider de caisse.");

                // Hauteur de caisse au repos (locale), puis dessous du collider ramene en LOCAL : melanger
                // une hauteur locale et une bounds monde ne vaut que tant que l'instance est a y = 0.
                var rideHeight = profile.ResolveStaticRideHeight(Physics.gravity.y);
                var underside = rideHeight + (collider.bounds.min.y - instance.transform.position.y);

                Assert.That(underside, Is.GreaterThan(CurbHeight),
                    "Le dessous du chassis (" + underside.ToString("F3") + " m) doit degager la hauteur de bordure authoree ("
                    + CurbHeight.ToString("F3") + " m). Autrement dit : raideur, longueur au repos, implantation des roues et "
                    + "hauteur de bordure sont un seul contrat, et ce test les tient ensemble.");

                return true;
            });
        }

        // ------------------------------------------------- helpers

        [Test]
        public void TheTelemetryViewLivesInDevToolsAndHoldsNoGameplayState()
        {
            Assert.That(File.Exists(TelemetryViewSourcePath), Is.True,
                "La vue de telemetrie vit dans RoadRage.DevTools (AC de la story).");

            var source = CodeWithoutComments(File.ReadAllText(TelemetryViewSourcePath));

            Assert.That(source, Does.Contain("Debug.isDebugBuild"),
                "La vue se desactive hors build de developpement. L'assembly DevTools est compilee dans les builds "
                + "joueur (contraintes vides) : la garde est donc au niveau source, pas dans l'asmdef.");
            Assert.That(source, Does.Not.Contain(".Value ="),
                "La vue ne mute aucun etat partage : elle est un instrument, pas une fonctionnalite.");
            Assert.That(source, Does.Not.Contain("linearVelocity ="),
                "Elle n'ecrit jamais dans la simulation qu'elle observe.");
            Assert.That(source, Does.Contain("TryGetWheelState"),
                "Elle lit l'etat publie par la couche physique au lieu de recalculer une seconde verite.");
        }

        private static VehicleProfileDef ResolveSerializedProfile(VehiclePhysicsBody body)
        {
            var serialized = new SerializedObject(body);
            return serialized.FindProperty("vehicleProfile").objectReferenceValue as VehicleProfileDef;
        }

        /// <summary>
        /// Inspecte un prefab sur une INSTANCE, jamais sur l'asset. Sur un prefab non instancie, Unity
        /// ne rend pas de <c>bounds</c> exploitables : le collider n'est pas enregistre dans la scene
        /// physique, et sa boite est de taille nulle. Une garde de hauteur ecrite sur l'asset serait
        /// donc vraie par construction -- elle ne garderait rien.
        /// </summary>
        private static T WithInstantiated<T>(string prefabPath, Func<GameObject, T> inspect)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, prefabPath + " attendu");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                return inspect(instance);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Corps d'une methode privee, sans ses accolades englobantes : sert aux gardes de forme sur le code.</summary>
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

        /// <summary>Lit un Vector3 serialise en YAML Unity, sous la forme <c>m_Key: {x: 1, y: 2, z: 3}</c>.</summary>
        private static Vector3 ReadVector3(string yaml, string key)
        {
            var match = Regex.Match(yaml, key + @": \{x: (?<x>[-0-9.]+), y: (?<y>[-0-9.]+), z: (?<z>[-0-9.]+)\}");
            Assert.That(match.Success, Is.True, key + " attendu dans le prefab");

            return new Vector3(
                float.Parse(match.Groups["x"].Value, CultureInfo.InvariantCulture),
                float.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture),
                float.Parse(match.Groups["z"].Value, CultureInfo.InvariantCulture));
        }

        private static List<Collider> FindCurbColliders(GameObject prefab)
        {
            return prefab.GetComponentsInChildren<Collider>(true)
                .Where(candidate => candidate.name.StartsWith("Col_Curb", StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>
        /// Retire les commentaires d'un source avant de le fouiller : la couche physique DOCUMENTE
        /// qu'elle n'utilise pas <c>WheelCollider</c>, et cette phrase ne doit pas etre lue comme un
        /// usage. Seul le code compte.
        /// </summary>
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
    }
}
