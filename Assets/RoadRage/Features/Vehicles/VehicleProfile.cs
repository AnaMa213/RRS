using System;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.11 : la donnee physique complete d'un vehicule, et rien d'autre. Masse, centre de
    /// masse, tenseur d'inertie, implantation des roues, ressort, amortisseur, longueur au repos,
    /// debattement, raideur anti-roulis et masque de sol.
    ///
    /// Portee par un <see cref="VehicleProfileDef"/> authore et lue telle quelle par
    /// <see cref="VehiclePhysicsBody"/>, le composant physique unique que partagent le prefab joueur
    /// et le prefab IA (AD-35). Rien de ce qui n'est pas du chassis n'entre ici : les seuils de
    /// recuperation (retournement, vide, blocage) et les parametres de conduite restent ou ils sont.
    ///
    /// Jamais mutee a l'execution : toute derivation produit une copie. Les roues ne sont pas
    /// exposees en tableau (elles se liraient par reference) mais une par une, donc en lecture seule.
    /// </summary>
    [Serializable]
    public struct VehicleProfile
    {
        [SerializeField]
        [Min(1f)]
        [Tooltip("Masse (kg). Ecrite telle quelle dans le Rigidbody : aucune valeur de repli implicite.")]
        private float mass;

        [SerializeField]
        [Tooltip("Centre de masse en coordonnees locales. Explicitement authore : automaticCenterOfMass est desactive sur les deux prefabs.")]
        private Vector3 centerOfMass;

        [SerializeField]
        [Tooltip("Tenseur d'inertie diagonal en coordonnees locales (kg.m2). Explicitement authore : automaticInertiaTensor est desactive sur les deux prefabs.")]
        private Vector3 inertiaTensor;

        [SerializeField]
        [Tooltip("Les roues du vehicule : leur implantation, et rien d'autre. Chaque essieu doit en porter exactement deux (paire anti-roulis).")]
        private VehicleWheel[] wheels;

        [SerializeField]
        [Min(1f)]
        [Tooltip("Raideur de ressort (N/m), identique aux quatre roues. La compression statique vaut masse x gravite / (nb roues x raideur).")]
        private float springRate;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Amortissement (N par m/s). Un ratio d'environ 0,3 de l'amortissement critique par quart de vehicule evite l'oscillation sans figer la caisse.")]
        private float damper;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Longueur au repos de la suspension (m) : distance ancrage -> contact roue detendue. Strictement superieure au debattement.")]
        private float restLength;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Debattement (m) : compression maximale utile. Doit couvrir la hauteur de bordure authoree, sinon la roue ne peut pas la monter.")]
        private float travel;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Raideur anti-roulis (N/m) : un couple proportionnel a l'ecart de compression entre les deux roues d'un essieu. Il reduit le roulis sans l'annuler.")]
        private float antiRollRate;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Couple de rappel d'assiette (N.m par radian d'inclinaison) : ramene la caisse sur ses roues quand elle s'est couchee. Indispensable hors contact -- des qu'une caisse s'incline trop, les rayons de roue ne touchent plus et la suspension cesse de la redresser.")]
        private float attitudeLevellingRate;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Amortissement du tangage et du roulis (N.m par rad/s). Le lacet n'est jamais amorti : il porte la direction.")]
        private float attitudeDamping;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Coefficient de frottement LATERAL de contact : resiste au glissement de travers. 1 = un pneu sur asphalte ; au-dela, c'est un choix arcade assume. Sans lui, un vehicule pose sur ses rayons glisse comme sur de la glace.")]
        private float lateralFrictionCoefficient;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Coefficient de resistance au ROULEMENT, dans l'axe de la roue. Volontairement faible : il ne doit pas lutter contre la conduite (Story 5.12).")]
        private float rollingResistanceCoefficient;

        [SerializeField]
        [Tooltip("Masque de sol des raycasts de roue. Vide : le composant refuse de s'activer, aucune roue ne peut toucher.")]
        private LayerMask groundMask;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Tolerance de hauteur (m) sous laquelle un contact ne touche que le dessous du vehicule -- donc franchit un relief (trottoir, levre de dalle, bordure authoree) au lieu de percuter un obstacle. Doit couvrir la bordure authoree (0,12 m) sans couvrir la face d'un mur.")]
        private float surfaceContactTolerance;

        public VehicleProfile(
            float mass,
            Vector3 centerOfMass,
            Vector3 inertiaTensor,
            VehicleWheel[] wheels,
            float springRate,
            float damper,
            float restLength,
            float travel,
            float antiRollRate,
            float attitudeLevellingRate,
            float attitudeDamping,
            float lateralFrictionCoefficient,
            float rollingResistanceCoefficient,
            LayerMask groundMask,
            float surfaceContactTolerance)
        {
            this.mass = mass;
            this.centerOfMass = centerOfMass;
            this.inertiaTensor = inertiaTensor;
            this.wheels = wheels;
            this.springRate = springRate;
            this.damper = damper;
            this.restLength = restLength;
            this.travel = travel;
            this.antiRollRate = antiRollRate;
            this.attitudeLevellingRate = attitudeLevellingRate;
            this.attitudeDamping = attitudeDamping;
            this.lateralFrictionCoefficient = lateralFrictionCoefficient;
            this.rollingResistanceCoefficient = rollingResistanceCoefficient;
            this.groundMask = groundMask;
            this.surfaceContactTolerance = surfaceContactTolerance;
        }

        /// <summary>Masse (kg).</summary>
        public float Mass
        {
            get { return mass; }
        }

        /// <summary>Centre de masse, en local.</summary>
        public Vector3 CenterOfMass
        {
            get { return centerOfMass; }
        }

        /// <summary>Tenseur d'inertie diagonal, en local.</summary>
        public Vector3 InertiaTensor
        {
            get { return inertiaTensor; }
        }

        /// <summary>Nombre de roues authorees.</summary>
        public int WheelCount
        {
            get { return wheels == null ? 0 : wheels.Length; }
        }

        /// <summary>Raideur de ressort (N/m).</summary>
        public float SpringRate
        {
            get { return springRate; }
        }

        /// <summary>Amortissement (N par m/s).</summary>
        public float Damper
        {
            get { return damper; }
        }

        /// <summary>Longueur au repos de la suspension (m).</summary>
        public float RestLength
        {
            get { return restLength; }
        }

        /// <summary>Debattement utile (m).</summary>
        public float Travel
        {
            get { return travel; }
        }

        /// <summary>Raideur anti-roulis (N/m).</summary>
        public float AntiRollRate
        {
            get { return antiRollRate; }
        }

        /// <summary>Couple de rappel d'assiette (N.m par radian d'inclinaison).</summary>
        public float AttitudeLevellingRate
        {
            get { return attitudeLevellingRate; }
        }

        /// <summary>Amortissement du tangage et du roulis (N.m par rad/s).</summary>
        public float AttitudeDamping
        {
            get { return attitudeDamping; }
        }

        /// <summary>Coefficient de frottement lateral de contact.</summary>
        public float LateralFrictionCoefficient
        {
            get { return lateralFrictionCoefficient; }
        }

        /// <summary>Coefficient de resistance au roulement, dans l'axe de la roue.</summary>
        public float RollingResistanceCoefficient
        {
            get { return rollingResistanceCoefficient; }
        }

        /// <summary>Tolerance de hauteur sous laquelle un contact ne touche que le dessous du vehicule (m).</summary>
        public float SurfaceContactTolerance
        {
            get { return surfaceContactTolerance; }
        }

        /// <summary>Masque de sol des raycasts de roue.</summary>
        public LayerMask GroundMask
        {
            get { return groundMask; }
        }

        /// <summary>
        /// Roue d'index donne, ou <c>default</c> hors bornes. Lecture par valeur : le tableau authore
        /// n'est jamais expose, donc jamais mute par un appelant.
        /// </summary>
        public VehicleWheel GetWheel(int index)
        {
            if (wheels == null || index < 0 || index >= wheels.Length)
            {
                return default;
            }

            return wheels[index];
        }

        /// <summary>
        /// Hauteur (locale) a laquelle la caisse se stabilise au repos, sur un sol plan et horizontal.
        /// Utile a deux choses : placer l'enfant visuel pour que ses roues touchent la route, et
        /// verifier que le dessous authore du collider degage la hauteur de bordure authoree -- sans
        /// ce degagement, une roue ne peut pas monter une bordure : c'est le chassis qui la percute.
        ///
        /// Les ancrages des roues sont authores a la meme hauteur (les gardes de la Story 5.11 le
        /// verifient) ; la moyenne ci-dessous vaut donc exactement cette hauteur.
        /// </summary>
        public float ResolveStaticRideHeight(float gravity)
        {
            if (WheelCount == 0 || !float.IsFinite(gravity))
            {
                return 0f;
            }

            var anchorHeight = 0f;
            for (var i = 0; i < WheelCount; i++)
            {
                anchorHeight += GetWheel(i).LocalPosition.y;
            }

            anchorHeight /= WheelCount;

            return restLength - VehicleSuspensionModel.ResolveStaticCompression(mass, gravity, WheelCount, springRate) - anchorHeight;
        }
    }
}
