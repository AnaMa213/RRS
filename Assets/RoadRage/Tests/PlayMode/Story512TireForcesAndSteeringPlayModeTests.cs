using System.Collections;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.12 : la preuve runtime du train roulant, celle qu'aucune garde EditMode ne peut porter.
    /// Les fonctions de pneu se prouvent hors physique ; ce que la story doit prouver, c'est que le
    /// vehicule REELLEMENT pose sur le sol accelere, freine et tourne **par ses roues** -- sans qu'une
    /// seule ligne de ce banc n'ecrive sa vitesse.
    ///
    /// C'est la difference de fond avec le banc de la Story 5.11 : celui-ci imposait la vitesse
    /// horizontale (`DriveAt`) parce que le controleur l'ecrivait, et il mesurait la suspension. Ici
    /// le banc ne fait que SOUMETTRE UNE INTENTION, comme le fait le controleur joueur, et tout le
    /// reste -- couple, glissement, force de pneu -- appartient a la couche physique.
    ///
    /// Les seuils sont des BORNES DE NON-REGRESSION, pas des mesures de confort : ils distinguent
    /// « les roues portent le vehicule » de « une vitesse est posee quelque part », qui est tout
    /// l'objet de la story.
    /// </summary>
    [Category("Story512")]
    public sealed class Story512TireForcesAndSteeringPlayModeTests
    {
        private const string ProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";

        /// <summary>Dimensions du collider du vehicule, copies des cotes figees du prefab joueur (NFR18).</summary>
        private static readonly Vector3 VehicleColliderSize = new Vector3(2.06f, 1.42f, 4.44f);

        private static readonly Vector3 VehicleColliderCenter = new Vector3(0f, 0.73f, 0f);

        /// <summary>Pas de physique maximal d'un scenario : borne le temps de banc, jamais une assertion.</summary>
        private const int MaxSteps = 900;

        private GameObject ground;
        private GameObject vehicle;
        private VehiclePhysicsBody physics;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Story512_Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(1200f, 1f, 1200f);

            vehicle = BuildVehicle();
            physics = vehicle.GetComponent<VehiclePhysicsBody>();

            yield return Settle(vehicle.GetComponent<Rigidbody>());
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(ground);
            Object.Destroy(vehicle);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TheVehicleAcceleratesBrakesAndTurnsThroughItsWheelsAlone()
        {
            var body = vehicle.GetComponent<Rigidbody>();

            Assert.That(physics.HasProfile, Is.True, "Le profil doit etre applique : sans roues, rien ne porte ni ne meut le vehicule.");
            Assert.That(body.linearVelocity.magnitude, Is.LessThan(0.5f), "Etat de depart : le vehicule est au repos, frein moteur serre.");

            // 1. Plein gaz, sans jamais ecrire la vitesse : seule l'intention est soumise.
            var throttleSteps = 0;
            var topSpeed = 0f;
            while (throttleSteps < 300)
            {
                Command(throttle: 1f, steer: 0f, brakeReverse: 0f, handbrake: 0f);
                yield return new WaitForFixedUpdate();
                throttleSteps++;

                var speed = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
                topSpeed = Mathf.Max(topSpeed, speed);

                Assert.That(float.IsNaN(speed), Is.False, "La vitesse projetee ne doit jamais devenir NaN, meme a plein gaz.");
            }

            Debug.Log("[Story512] plein gaz 6 s : vitesse maximale atteinte " + topSpeed.ToString("F2") + " m/s, "
                + "vitesse courante " + Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude.ToString("F2") + " m/s.");

            Assert.That(topSpeed, Is.GreaterThan(4f),
                "Le vehicule doit ACCELERER par ses roues : vitesse maximale mesuree " + topSpeed.ToString("F2")
                + " m/s. Une valeur proche de zero signifierait qu'aucun effort moteur n'atteint le sol -- couple moteur, "
                + "roue motrice ou integration du spin.");
            Assert.That(CountGroundedWheels(), Is.GreaterThanOrEqualTo(2), "Accelerer ne doit pas decoller le vehicule.");

            // 2. Braquage a vitesse de conduite : le lacet doit venir des efforts lateraux des pneus.
            var headingBefore = vehicle.transform.eulerAngles.y;
            var steered = 0;
            while (steered < 100)
            {
                Command(throttle: 0.4f, steer: 1f, brakeReverse: 0f, handbrake: 0f);
                yield return new WaitForFixedUpdate();
                steered++;
            }

            var headingDelta = Mathf.DeltaAngle(headingBefore, vehicle.transform.eulerAngles.y);
            var steerAngle = physics.CurrentSteerAngleDegrees;
            Debug.Log("[Story512] braquage 2 s : cap tourne de " + headingDelta.ToString("F1") + " deg, angle de roue effectif "
                + steerAngle.ToString("F1") + " deg a " + Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude.ToString("F2") + " m/s.");

            Assert.That(Mathf.Abs(headingDelta), Is.GreaterThan(15f),
                "Le vehicule doit TOURNER par ses roues : cap tourne de " + headingDelta.ToString("F1")
                + " deg. Aucun lacet n'est impose a la caisse (plus de MoveRotation de conduite), donc une valeur nulle "
                + "signifierait que l'angle de roue n'atteint pas les roues directrices ou que le pneu ne transmet rien.");
            Assert.That(Mathf.Abs(steerAngle), Is.GreaterThan(1f),
                "Et l'angle de roue doit etre effectivement applique : angle mesure " + steerAngle.ToString("F1") + " deg.");

            // 3. Freinage : le vehicule doit s'arreter, pas s'inverser.
            var braked = 0;
            while (braked < MaxSteps && Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude > 0.5f)
            {
                Command(throttle: 0f, steer: 0f, brakeReverse: 1f, handbrake: 0f);
                yield return new WaitForFixedUpdate();
                braked++;
            }

            var residual = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
            Debug.Log("[Story512] freinage : " + residual.ToString("F2") + " m/s apres " + (braked * Time.fixedDeltaTime).ToString("F2") + " s.");

            Assert.That(residual, Is.LessThan(1f),
                "Le frein de service doit arreter le vehicule : vitesse residuelle mesuree " + residual.ToString("F2") + " m/s.");

            Assert.That(body.linearVelocity.magnitude, Is.LessThan(30f), "Aucun emballement : la vitesse reste bornee par le profil.");
        }

        [UnityTest]
        public IEnumerator TheHandbrakeLocksTheRearWheelsAndBreaksTheirGrip()
        {
            var body = vehicle.GetComponent<Rigidbody>();
            var profile = physics.Profile;

            // Montee en vitesse, puis frein a main SEUL : c'est lui qui doit bloquer les roues arriere.
            var spinUp = 0;
            while (spinUp < 240)
            {
                Command(throttle: 1f, steer: 0f, brakeReverse: 0f, handbrake: 0f);
                yield return new WaitForFixedUpdate();
                spinUp++;
            }

            var speedBefore = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
            Assert.That(speedBefore, Is.GreaterThan(3f), "Le frein a main ne se mesure pas a l'arret : le vehicule doit rouler.");

            var handbrakeSteps = 0;
            while (handbrakeSteps < 60)
            {
                Command(throttle: 0f, steer: 0f, brakeReverse: 0f, handbrake: 1f);
                yield return new WaitForFixedUpdate();
                handbrakeSteps++;
            }

            var rearSlip = 0f;
            var frontSlip = 0f;
            for (var i = 0; i < profile.WheelCount; i++)
            {
                Assert.That(physics.TryGetTireSample(i, out var tire), Is.True, "La lecture de pneu doit etre publiee par la couche physique.");
                Assert.That(float.IsNaN(tire.SlipRatio), Is.False, "Le glissement publie ne doit jamais etre NaN.");

                if (profile.GetWheel(i).IsSteering)
                {
                    frontSlip = Mathf.Max(frontSlip, Mathf.Abs(tire.SlipRatio));
                }
                else
                {
                    rearSlip = Mathf.Max(rearSlip, Mathf.Abs(tire.SlipRatio));
                }
            }

            var speedAfter = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;
            Debug.Log("[Story512] frein a main 1,2 s : vitesse " + speedBefore.ToString("F2") + " -> " + speedAfter.ToString("F2")
                + " m/s, glissement arriere " + rearSlip.ToString("F3") + ", glissement avant " + frontSlip.ToString("F3") + ".");

            Assert.That(rearSlip, Is.GreaterThan(Mathf.Max(frontSlip * 2f, 0.4f)),
                "Le frein a main doit BLOQUER les roues arriere : glissement arriere mesure " + rearSlip.ToString("F3")
                + " contre " + frontSlip.ToString("F3") + " a l'avant. C'est ce blocage -- et lui seul -- qui effondre l'adherence "
                + "laterale arriere et fait entrer le vehicule en derive.");
            Assert.That(speedAfter, Is.LessThan(speedBefore),
                "Une roue arriere bloquee freine : la vitesse doit baisser (" + speedBefore.ToString("F2") + " -> "
                + speedAfter.ToString("F2") + " m/s).");
            Assert.That(CountGroundedWheels(), Is.GreaterThanOrEqualTo(2), "Le blocage des roues arriere ne doit pas coucher le vehicule.");
        }

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

        private GameObject BuildVehicle()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(ProfilePath);
            Assert.That(profile, Is.Not.Null, ProfilePath + " attendu");

            var root = new GameObject("Story512_Vehicle");
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

        private int CountGroundedWheels()
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
    }
}
