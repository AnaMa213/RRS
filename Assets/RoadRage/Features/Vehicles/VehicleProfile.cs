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
        [Tooltip("ADHERENCE du pneu : coefficient sans unite. 1 = un pneu sur asphalte ; au-dela, c'est un choix arcade assume. C'est la borne dure du modele -- aucune force de pneu ne depasse 'adherence x charge portee par la roue'. Point d'ancrage d'une future table par surface (report enregistre), mais une seule valeur aujourd'hui : une table indexee par un type de surface qu'aucun materiau ne porte serait de la configuration morte.")]
        private float lateralFrictionCoefficient;

        [SerializeField]
        [Tooltip("Masque de sol des raycasts de roue. Vide : le composant refuse de s'activer, aucune roue ne peut toucher.")]
        private LayerMask groundMask;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Tolerance de hauteur (m) sous laquelle un contact ne touche que le dessous du vehicule -- donc franchit un relief (trottoir, levre de dalle, bordure authoree) au lieu de percuter un obstacle. Doit couvrir la bordure authoree (0,12 m) sans couvrir la face d'un mur.")]
        private float surfaceContactTolerance;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Couple moteur par roue MOTRICE (N.m). Reparti sur les seules roues marquees IsDriven : une roue non motrice ne recoit jamais d'effort moteur.")]
        private float engineTorque;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Couple moteur de marche arriere par roue motrice (N.m). Il ne s'engage que sous 'minimumDirectionSpeed' : au-dessus, l'entree de frein freine et ne recule pas.")]
        private float reverseTorque;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Couple de frein de service par roue, en freinage (N.m).")]
        private float brakeTorque;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Couple de frein moteur par roue quand aucune entree n'est donnee (N.m). Volontairement faible : il ne doit pas lutter contre la conduite, seulement retenir un vehicule gare sans conducteur.")]
        private float coastTorque;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Couple de frein a main par roue ARRIERE (N.m). Il doit bloquer la roue, pas seulement la ralentir : c'est le blocage qui effondre l'adherence laterale arriere et fait entrer en derive.")]
        private float handbrakeTorque;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Inertie de rotation d'une roue (kg.m2). Elle fixe la vitesse avec laquelle le glissement longitudinal s'installe : trop grande, la roue ne patine jamais ; trop petite, elle se bloque sur la moindre sollicitation.")]
        private float wheelInertia;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Vitesse de pointe en marche avant (m/s). Elle est APPROCHEE par l'extinction progressive de l'effort moteur, jamais posee par une ecriture de vitesse. Le facteur de degats de la Story 3.5 la reduit.")]
        private float maxForwardSpeed;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Vitesse de pointe en marche arriere (m/s).")]
        private float maxReverseSpeed;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Vitesse sous laquelle le vehicule est traite comme quasi immobile (m/s) : seuil de changement de sens (frein -> marche arriere) et plancher de braquage. C'est le seuil 'minimumSteerSpeed' de la Story 3.4, deplace ici avec les autres reglages de conduite.")]
        private float minimumDirectionSpeed;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Angle de roue maximal a l'arret (degres).")]
        private float maxSteerAngleDegrees;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Angle de roue maximal a la vitesse de reduction complete (degres). Doit rester sous l'angle maximal a l'arret : c'est ce qui garde la direction lisible a vitesse elevee.")]
        private float highSpeedSteerAngleDegrees;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Vitesse (m/s) a laquelle la reduction d'angle est complete. Au-dela, l'angle ne diminue plus.")]
        private float steerFullReductionSpeed;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Vitesse de braquage des roues directrices (degres par seconde).")]
        private float steerRateDegreesPerSecond;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Vitesse de RETOUR AU CENTRE des roues directrices (degres par seconde). Le rappel a son propre taux : un retour trop lent se lit comme une roue bloquee, un retour instantane comme une remise sur rails.")]
        private float steerReturnRateDegreesPerSecond;

        [SerializeField]
        [Min(0.0001f)]
        [Tooltip("Glissement longitudinal auquel le pneu atteint son pic d'adherence. Au-dela, la force decroit progressivement.")]
        private float tirePeakSlipRatio;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Angle de glissement (degres) auquel le pneu atteint son pic d'adherence lateral. Au-dela, la force decroit progressivement jusqu'a la fraction de chute authoree.")]
        private float tirePeakSlipAngleDegrees;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Fraction du pic conservee en glissement total (0 a 1 exclus) : l'asymptote de la chute d'adherence. Elle doit rester STRICTEMENT positive -- une force qui tombe a zero serait un seuil binaire deguise, et la derive deviendrait irrecuperable.")]
        private float tireSlipFalloffFraction;

        [SerializeField]
        [Min(0f)]
        [Tooltip("AIDE ARCADE (Story 5.13) -- abattement du lacet (N.m par degre par seconde de vitesse de lacet). Amortit la rotation de la caisse sans jamais la figer ni tenir un cap. NUL : terme inerte, le lacet est entierement rendu a la geometrie du pneu.")]
        private float yawStabilityRate;

        [SerializeField]
        [Min(0f)]
        [Tooltip("AIDE ARCADE (Story 5.13) -- controle de traction : part du couple moteur d'une roue retiree quand son glissement depasse le pic author e (0 a 1 EXCLUS). NUL : terme inerte. STRICTEMENT inferieure a 1 : l'attenuation ne peut pas annuler le couple moteur.")]
        private float tractionControlStrength;

        [SerializeField]
        [Min(0f)]
        [Tooltip("AIDE ARCADE (Story 5.13) -- recuperation de tete-a-queue (N.m par degre par seconde de derive au-dela du seuil). Dirige du cote demande par le conducteur : entree nulle, aucun couple. NUL : terme inerte.")]
        private float spinRecoveryRate;

        [SerializeField]
        [Min(0f)]
        [Tooltip("AIDE ARCADE (Story 5.13) -- seuil de derive (degres par seconde) sous lequel la recuperation de tete-a-queue ne se declenche pas : une caisse qui tourne normalement dans un virage ne doit rien sentir. NUL : le terme agit des la moindre rotation, pourvu que son propre taux soit author e.")]
        private float spinDriftThresholdDegreesPerSecond;

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
            LayerMask groundMask,
            float surfaceContactTolerance,
            float engineTorque,
            float reverseTorque,
            float brakeTorque,
            float coastTorque,
            float handbrakeTorque,
            float wheelInertia,
            float maxForwardSpeed,
            float maxReverseSpeed,
            float minimumDirectionSpeed,
            float maxSteerAngleDegrees,
            float highSpeedSteerAngleDegrees,
            float steerFullReductionSpeed,
            float steerRateDegreesPerSecond,
            float steerReturnRateDegreesPerSecond,
            float tirePeakSlipRatio,
            float tirePeakSlipAngleDegrees,
            float tireSlipFalloffFraction,
            float yawStabilityRate,
            float tractionControlStrength,
            float spinRecoveryRate,
            float spinDriftThresholdDegreesPerSecond)
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
            this.groundMask = groundMask;
            this.surfaceContactTolerance = surfaceContactTolerance;
            this.engineTorque = engineTorque;
            this.reverseTorque = reverseTorque;
            this.brakeTorque = brakeTorque;
            this.coastTorque = coastTorque;
            this.handbrakeTorque = handbrakeTorque;
            this.wheelInertia = wheelInertia;
            this.maxForwardSpeed = maxForwardSpeed;
            this.maxReverseSpeed = maxReverseSpeed;
            this.minimumDirectionSpeed = minimumDirectionSpeed;
            this.maxSteerAngleDegrees = maxSteerAngleDegrees;
            this.highSpeedSteerAngleDegrees = highSpeedSteerAngleDegrees;
            this.steerFullReductionSpeed = steerFullReductionSpeed;
            this.steerRateDegreesPerSecond = steerRateDegreesPerSecond;
            this.steerReturnRateDegreesPerSecond = steerReturnRateDegreesPerSecond;
            this.tirePeakSlipRatio = tirePeakSlipRatio;
            this.tirePeakSlipAngleDegrees = tirePeakSlipAngleDegrees;
            this.tireSlipFalloffFraction = tireSlipFalloffFraction;
            this.yawStabilityRate = yawStabilityRate;
            this.tractionControlStrength = tractionControlStrength;
            this.spinRecoveryRate = spinRecoveryRate;
            this.spinDriftThresholdDegreesPerSecond = spinDriftThresholdDegreesPerSecond;
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

        /// <summary>Adherence du pneu (coefficient sans unite) : la borne dure du modele.</summary>
        public float LateralFrictionCoefficient
        {
            get { return lateralFrictionCoefficient; }
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

        /// <summary>Couple moteur par roue motrice (N.m).</summary>
        public float EngineTorque
        {
            get { return engineTorque; }
        }

        /// <summary>Couple moteur de marche arriere par roue motrice (N.m).</summary>
        public float ReverseTorque
        {
            get { return reverseTorque; }
        }

        /// <summary>Couple de frein de service par roue (N.m).</summary>
        public float BrakeTorque
        {
            get { return brakeTorque; }
        }

        /// <summary>Couple de frein moteur par roue quand aucune entree n'est donnee (N.m).</summary>
        public float CoastTorque
        {
            get { return coastTorque; }
        }

        /// <summary>Couple de frein a main par roue arriere (N.m).</summary>
        public float HandbrakeTorque
        {
            get { return handbrakeTorque; }
        }

        /// <summary>Inertie de rotation d'une roue (kg.m2).</summary>
        public float WheelInertia
        {
            get { return wheelInertia; }
        }

        /// <summary>Vitesse de pointe en marche avant (m/s), approchee par une pente de force.</summary>
        public float MaxForwardSpeed
        {
            get { return maxForwardSpeed; }
        }

        /// <summary>Vitesse de pointe en marche arriere (m/s).</summary>
        public float MaxReverseSpeed
        {
            get { return maxReverseSpeed; }
        }

        /// <summary>Vitesse sous laquelle le vehicule est quasi immobile (m/s).</summary>
        public float MinimumDirectionSpeed
        {
            get { return minimumDirectionSpeed; }
        }

        /// <summary>Angle de roue maximal a l'arret (degres).</summary>
        public float MaxSteerAngleDegrees
        {
            get { return maxSteerAngleDegrees; }
        }

        /// <summary>Angle de roue maximal a la vitesse de reduction complete (degres).</summary>
        public float HighSpeedSteerAngleDegrees
        {
            get { return highSpeedSteerAngleDegrees; }
        }

        /// <summary>Vitesse a laquelle la reduction d'angle de roue est complete (m/s).</summary>
        public float SteerFullReductionSpeed
        {
            get { return steerFullReductionSpeed; }
        }

        /// <summary>Vitesse de braquage des roues directrices (degres par seconde).</summary>
        public float SteerRateDegreesPerSecond
        {
            get { return steerRateDegreesPerSecond; }
        }

        /// <summary>Vitesse de retour au centre des roues directrices (degres par seconde).</summary>
        public float SteerReturnRateDegreesPerSecond
        {
            get { return steerReturnRateDegreesPerSecond; }
        }

        /// <summary>Glissement longitudinal du pic d'adherence.</summary>
        public float TirePeakSlipRatio
        {
            get { return tirePeakSlipRatio; }
        }

        /// <summary>Angle de glissement du pic d'adherence (degres).</summary>
        public float TirePeakSlipAngleDegrees
        {
            get { return tirePeakSlipAngleDegrees; }
        }

        /// <summary>Fraction du pic conservee en glissement total : l'asymptote de la chute d'adherence.</summary>
        public float TireSlipFalloffFraction
        {
            get { return tireSlipFalloffFraction; }
        }

        /// <summary>Abattement du lacet (N.m par degre par seconde). Nul : terme inerte (Story 5.13).</summary>
        public float YawStabilityRate
        {
            get { return yawStabilityRate; }
        }

        /// <summary>Part du couple moteur retiree en glissement excessif, dans <c>[0, 1[</c>. Nulle : terme inerte (Story 5.13).</summary>
        public float TractionControlStrength
        {
            get { return tractionControlStrength; }
        }

        /// <summary>Couple de recuperation de tete-a-queue (N.m par degre par seconde de derive). Nul : terme inerte (Story 5.13).</summary>
        public float SpinRecoveryRate
        {
            get { return spinRecoveryRate; }
        }

        /// <summary>Seuil de derive (degres par seconde) de la recuperation de tete-a-queue (Story 5.13).</summary>
        public float SpinDriftThresholdDegreesPerSecond
        {
            get { return spinDriftThresholdDegreesPerSecond; }
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

        /// <summary>
        /// Demi-voie moyenne (m) : moyenne des <c>|x|</c> locaux des roues authorees. C'est la distance
        /// a laquelle une force laterale produit un couple autour de l'axe vertical, donc l'echelle de
        /// toute borne angulaire de la couche -- les aides de lacet de la Story 5.13 la lisent pour
        /// borner leurs termes contre le budget de charge porte.
        ///
        /// Zero si le profil ne porte aucune roue : la borne qu'elle alimente est alors nulle, donc le
        /// terme qu'elle borne est inerte plutot que non borne.
        /// </summary>
        public float ResolveMeanHalfTrack()
        {
            if (WheelCount == 0)
            {
                return 0f;
            }

            var halfTrack = 0f;
            for (var i = 0; i < WheelCount; i++)
            {
                halfTrack += Mathf.Abs(GetWheel(i).LocalPosition.x);
            }

            return halfTrack / WheelCount;
        }
    }
}
