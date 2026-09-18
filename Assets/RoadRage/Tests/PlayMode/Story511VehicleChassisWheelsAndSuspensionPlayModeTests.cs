using System.Collections;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.11 : la preuve runtime de la couche physique, celle qu'aucune garde EditMode ne peut
    /// porter. Les fonctions pures se prouvent hors physique ; ce que la story doit prouver, c'est que
    /// le vehicule REELLEMENT pose sur une bordure la monte a basse vitesse et ne decolle pas quand il
    /// la heurte a vitesse de conduite -- les deux symptomes de recette a l'origine de la correction
    /// de cap du 2026-09-18.
    ///
    /// La bordure du banc est construite a la HAUTEUR AUTHOREE, lue sur le module de carrefour : le
    /// test physique et l'authoring de scene ne peuvent donc pas diverger en silence. Un montage
    /// local plutot que MVP_Run : la couche physique ne depend ni du reseau, ni de la scene, et un
    /// banc minimal isole la cause mesuree.
    ///
    /// Les seuils sont des BORNES DE NON-REGRESSION, pas des mesures de confort : ils distinguent
    /// « la roue monte » de « la caisse est proyectee », qui est tout l'objet de la story.
    /// </summary>
    [Category("Story511")]
    public sealed class Story511VehicleChassisWheelsAndSuspensionPlayModeTests
    {
        private const string ProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
        private const string JunctionPrefabPath = "Assets/RoadRage/Prefabs/Greybox_Intersection.prefab";

        /// <summary>Demi-largeur de la chaussee : la face interieure de la bordure est a cette distance de l'axe du banc.</summary>
        private const float RoadwayHalfWidth = 4f;

        /// <summary>
        /// Hauteur de bordure authoree, en metres -- la valeur du contrat de kit artistique, tenue cote
        /// EditMode (`Story511VehicleChassisWheelsAndSuspensionTests.CurbHeight`). Le banc la rejoue et
        /// la verifie : sans cette borne, une lecture d'asset non instancie rendait la MOITIE de la
        /// marche sans que rien ne le signale.
        /// </summary>
        private const float AuthoredCurbHeight = 0.12f;

        /// <summary>Epaisseur du banc : la bordure doit rester dans la bande trottoir.</summary>
        private const float CurbThickness = 0.3f;

        /// <summary>
        /// Profondeur de la bande trottoir derriere la bordure, en metres. La bande existe dans le banc
        /// parce qu'elle existe dans le district : une bordure isolee ne se gravit pas, elle se franchit
        /// et redescend aussitot -- c'est une marche de trottoir qu'on mesure, pas un dos-d'ane.
        /// </summary>
        private const float SidewalkBandDepth = 8f;

        /// <summary>Dimensions du collider du vehicule, copies des cotes figees du prefab joueur (NFR18).</summary>
        private static readonly Vector3 VehicleColliderSize = new Vector3(2.06f, 1.42f, 4.44f);

        private static readonly Vector3 VehicleColliderCenter = new Vector3(0f, 0.73f, 0f);

        /// <summary>Vitesse « basse » : le vehicule doit monter la bordure sans elan.</summary>
        private const float LowSpeed = 1.2f;

        /// <summary>Vitesse « de conduite » : dans la plage ou la recette voyait les vehicules proyectes.</summary>
        private const float DrivingSpeed = 12f;

        /// <summary>
        /// Excursion verticale toleree a vitesse de conduite, en metres. Une bordure de 12 cm doit
        /// soulever la roue de 12 cm, pas la caisse de plusieurs dizaines de centimetres : au-dela,
        /// c'est le symptome « vehicule projete » qui revient.
        /// </summary>
        private const float MaxVerticalExcursionAtDrivingSpeed = 0.4f;

        /// <summary>Pic de vitesse verticale tolere a vitesse de conduite, en m/s.</summary>
        private const float MaxVerticalSpeedAtDrivingSpeed = 4f;

        private GameObject ground;
        private GameObject curb;
        private GameObject sidewalkBand;
        private GameObject vehicle;

        private float curbHeight;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            curbHeight = ResolveAuthoredCurbHeight();
            Assert.That(curbHeight, Is.GreaterThan(0.02f),
                "La bordure authoree sur le carrefour doit etre mesurable : c'est elle que le banc physique rejoue.");
            Assert.That(curbHeight, Is.EqualTo(AuthoredCurbHeight).Within(0.001f),
                "Le banc doit rejouer la hauteur AUTHOREE (" + AuthoredCurbHeight.ToString("F3")
                + " m) et non ce que la lecture d'un asset non instancie laissait croire. La constante est celle du "
                + "contrat de kit artistique, gardee cote EditMode.");

            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Story511_Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(400f, 1f, 400f);

            curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curb.name = "Story511_Curb";
            curb.transform.position = new Vector3(0f, curbHeight / 2f, RoadwayHalfWidth + (CurbThickness / 2f));
            curb.transform.localScale = new Vector3(40f, curbHeight, CurbThickness);

            // Bande trottoir : meme hauteur que le sommet de la bordure, posee derriere elle. C'est
            // elle qui rend la montee observable -- sans elle, la roue suit une marche de 30 cm de
            // profondeur et redescend avant que la caisse ait bouge.
            sidewalkBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sidewalkBand.name = "Story511_SidewalkBand";
            sidewalkBand.transform.position = new Vector3(0f, curbHeight / 2f, RoadwayHalfWidth + CurbThickness + (SidewalkBandDepth / 2f));
            sidewalkBand.transform.localScale = new Vector3(40f, curbHeight, SidewalkBandDepth);

            vehicle = BuildVehicle();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(ground);
            Object.Destroy(curb);
            Object.Destroy(sidewalkBand);
            Object.Destroy(vehicle);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TheVehicleClimbsTheAuthoredCurbAtLowSpeed()
        {
            var body = vehicle.GetComponent<Rigidbody>();
            var physics = vehicle.GetComponent<VehiclePhysicsBody>();

            yield return Settle(body);

            var restedHeight = body.position.y;
            Assert.That(physics.TryGetWheelState(0, out _), Is.True, "Le profil doit etre applique : sans roues, rien ne porte le vehicule.");
            Assert.That(CountGroundedWheels(physics), Is.GreaterThanOrEqualTo(2),
                "Au repos sur le plat, au moins deux roues portent.");

            var highestContact = 0f;
            var peakHeight = restedHeight;
            var elapsed = 0f;
            while (elapsed < 8f && body.position.z < RoadwayHalfWidth + CurbThickness + 6f)
            {
                DriveAt(body, LowSpeed);
                yield return new WaitForFixedUpdate();

                elapsed += Time.fixedDeltaTime;
                peakHeight = Mathf.Max(peakHeight, body.position.y);

                for (var i = 0; i < physics.WheelCount; i++)
                {
                    if (physics.TryGetWheelState(i, out var wheel) && wheel.Grounded)
                    {
                        // Hauteur monde du point de contact : 0 sur la chaussee, la hauteur authoree sur
                        // la bande trottoir. C'est la mesure directe de « la roue suit la marche ».
                        highestContact = Mathf.Max(highestContact, wheel.ContactPoint.y);
                    }
                }
            }

            Assert.That(body.position.z, Is.GreaterThan(RoadwayHalfWidth + CurbThickness),
                "Le vehicule doit avoir franchi l'arete de chaussee, sinon rien n'a ete mis a l'epreuve.");

            var rise = peakHeight - restedHeight;
            Debug.Log("[Story511] bordure " + curbHeight.ToString("F3") + " m, hauteur au repos " + restedHeight.ToString("F3")
                + " m, elevation maximale de caisse a " + LowSpeed + " m/s : " + rise.ToString("F3") + " m, contact de roue le plus haut : "
                + highestContact.ToString("F3") + " m, en " + elapsed.ToString("F2") + " s.");

            Assert.That(highestContact, Is.GreaterThan(curbHeight * 0.9f),
                "La roue doit suivre la marche authoree et non s'y arreter : contact le plus haut mesure "
                + highestContact.ToString("F3") + " m pour une bordure de " + curbHeight.ToString("F3") + " m.");

            Assert.That(rise, Is.GreaterThan(curbHeight * 0.5f),
                "A basse vitesse, le vehicule doit MONTER la bordure authoree : il ne s'est eleve que de " + rise.ToString("F3")
                + " m. La hauteur authoree et le debattement de suspension sont un seul contrat, et ce test le met a l'epreuve "
                + "sur le vrai pas de physique.");

            Assert.That(Vector3.Dot(vehicle.transform.up, Vector3.up), Is.GreaterThan(0.5f),
                "Monter une bordure ne doit pas retourner le vehicule : monter n'est pas basculer.");
        }

        [UnityTest]
        public IEnumerator TheVehicleDoesNotLaunchWhenItHitsTheAuthoredCurbAtDrivingSpeed()
        {
            var body = vehicle.GetComponent<Rigidbody>();
            var physics = vehicle.GetComponent<VehiclePhysicsBody>();

            yield return Settle(body);

            var restedHeight = body.position.y;
            var peakHeight = restedHeight;
            var peakVerticalSpeed = 0f;
            var elapsed = 0f;
            while (elapsed < 2f && body.position.z < RoadwayHalfWidth + CurbThickness + 4f)
            {
                DriveAt(body, DrivingSpeed);
                yield return new WaitForFixedUpdate();

                elapsed += Time.fixedDeltaTime;
                peakHeight = Mathf.Max(peakHeight, body.position.y);
                peakVerticalSpeed = Mathf.Max(peakVerticalSpeed, Mathf.Abs(body.linearVelocity.y));
            }

            var excursion = peakHeight - restedHeight;
            Debug.Log("[Story511] bordure " + curbHeight.ToString("F3") + " m, impact a " + DrivingSpeed + " m/s : excursion verticale "
                + excursion.ToString("F3") + " m, pic de vitesse verticale " + peakVerticalSpeed.ToString("F3") + " m/s en "
                + elapsed.ToString("F2") + " s.");

            Assert.That(excursion, Is.LessThan(MaxVerticalExcursionAtDrivingSpeed),
                "A vitesse de conduite, heurter une bordure authoree ne doit pas projeter le vehicule : excursion mesuree "
                + excursion.ToString("F3") + " m. C'est le symptome de recette que la course correction du 2026-09-18 corrige, "
                + "et la correction est dans la couche physique, jamais dans la geometrie.");

            Assert.That(peakVerticalSpeed, Is.LessThan(MaxVerticalSpeedAtDrivingSpeed),
                "Aucun pic de vitesse verticale : pic mesure " + peakVerticalSpeed.ToString("F3") + " m/s. Une marche nue produisait "
                + "une impulsion de depenetration ; une roue portante produit de la compression.");

            // Traversee de la bande trottoir, bornee en temps ET en iterations : un vehicule qui se
            // coince dans l'emprise de la bordure ne doit pas figer la suite, il doit echouer lisiblement.
            var crossed = 0;
            while (body.position.z > RoadwayHalfWidth + CurbThickness && body.position.z < RoadwayHalfWidth + CurbThickness + 6f)
            {
                DriveAt(body, LowSpeed);
                yield return new WaitForFixedUpdate();

                if (++crossed > 600)
                {
                    Assert.Fail("Le vehicule ne franchit pas la bande trottoir : il est coince dans l'emprise de la bordure.");
                }
            }

            yield return Settle(body);

            Assert.That(CountGroundedWheels(physics), Is.GreaterThanOrEqualTo(2),
                "Apres le franchissement, le vehicule doit etre repose sur ses roues -- ni en l'air, ni couche sur la caisse.");
        }

        /// <summary>
        /// Moteur du banc : meme partage que le vrai conducteur de la Story 5.11. La vitesse HORIZONTALE
        /// est imposee (c'est ce que le controleur ecrit), et l'axe vertical est laisse a la couche
        /// physique -- sinon le banc annulerait la gravite et ne prouverait plus rien de la suspension.
        /// </summary>
        private static void DriveAt(Rigidbody body, float speed)
        {
            var current = body.linearVelocity;
            body.linearVelocity = new Vector3(0f, current.y, speed);
        }

        private GameObject BuildVehicle()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);
            Assert.That(profile, Is.Not.Null, ProfilePath + " attendu");

            var root = new GameObject("Story511_Vehicle");
            root.SetActive(false);
            root.transform.position = new Vector3(0f, 0.5f, -2f);

            var box = root.AddComponent<BoxCollider>();
            box.center = VehicleColliderCenter;
            box.size = VehicleColliderSize;

            var body = root.AddComponent<Rigidbody>();
            body.mass = 1200f;
            body.linearDamping = 0.3f;
            body.angularDamping = 3f;

            var physics = root.AddComponent<VehiclePhysicsBody>();
            physics.BindProfile(profile);

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

        private static int CountGroundedWheels(VehiclePhysicsBody physics)
        {
            var grounded = 0;
            for (var i = 0; i < physics.WheelCount; i++)
            {
                if (physics.TryGetWheelState(i, out var wheel) && wheel.Grounded)
                {
                    grounded++;
                }
            }

            return grounded;
        }

        /// <summary>
        /// Hauteur de la bordure authoree, lue sur une INSTANCE du prefab du carrefour -- jamais sur
        /// l'asset. Sur un prefab non instancie, `Collider.bounds` degenere en la position du transform
        /// (constat mesure au depot) : la lecture rendait 0,06 m, soit la moitie de la bordure
        /// authoree, et le banc rejouait donc une marche de 6 cm en croyant en rejouer une de 12.
        /// </summary>
        private static float ResolveAuthoredCurbHeight()
        {
            var junction = AssetDatabase.LoadAssetAtPath<GameObject>(JunctionPrefabPath);
            Assert.That(junction, Is.Not.Null, JunctionPrefabPath + " attendu");

            var probe = (GameObject)PrefabUtility.InstantiatePrefab(junction);
            try
            {
                var highest = 0f;
                foreach (var collider in probe.GetComponentsInChildren<Collider>(true))
                {
                    if (collider.name.StartsWith("Col_Curb", System.StringComparison.Ordinal))
                    {
                        highest = Mathf.Max(highest, collider.bounds.max.y - probe.transform.position.y);
                    }
                }

                return highest;
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }
    }
}
