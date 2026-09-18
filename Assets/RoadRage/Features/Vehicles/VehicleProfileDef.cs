using RoadRage.Shared.Definitions;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.11 : donnee auteur de la couche physique d'un vehicule. Vit comme asset sous
    /// Assets/RoadRage/ScriptableObjects/Vehicles/ et porte un id globalement unique en minuscules,
    /// stable, sur le meme gabarit que <c>DriverProfileDef</c> (Features.Vehicles),
    /// <c>RageTuningDef</c> (Features.Rage) et <c>CharacterDef</c> (Features.Players). Jamais mute a
    /// l'execution.
    ///
    /// Pas de catalogue associe : comme <c>DriverProfileDef</c>, aucun appelant ne fait de lookup par
    /// id avant les Stories 5.16 et 5.19 -- le Def est reference directement par le prefab. Inventer
    /// un catalogue serait de la configuration morte.
    ///
    /// Un profil invalide n'a **aucune** valeur de repli : <see cref="VehiclePhysicsBody"/> ne
    /// s'active pas dessus, donc le vehicule garde son Rigidbody tel quel plutot que de rouler sur
    /// des constantes inventees.
    /// </summary>
    [CreateAssetMenu(fileName = "VehicleProfileDef", menuName = "RoadRage/Vehicles/Vehicle Profile Def")]
    public sealed class VehicleProfileDef : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Id globalement unique en minuscules, ex. vehicle_default. Stable : ne jamais le renommer une fois publie.")]
        private string id = string.Empty;

        [SerializeField]
        [Tooltip("La donnee physique complete du vehicule. Point de reglage unique : aucun parametre de chassis, de roue ou de suspension ne vit dans un controleur.")]
        private VehicleProfile profile = new VehicleProfile(
            mass: 1200f,
            centerOfMass: new Vector3(0f, -0.35f, 0f),
            inertiaTensor: new Vector3(2173f, 2396f, 626f),
            // Roues avant directrices ET motrices : le vehicule est a quatre roues motrices. Le couple
            // est reparti sur les quatre, donc chaque pneu reste LOIN de sa limite d'adherence (c'est
            // ce qui evite le patinage -- et le patinage est ce qui consommait le budget lateral du
            // pneu arriere, donc ce qui faisait deraper la voiture). Les roues arriere ne braquent
            // toujours pas : le frein a main y garde ses roues a bloquer.
            wheels: new[]
            {
                new VehicleWheel(new Vector3(-0.85f, 0.22f, 1.55f), 0.33f, 0, true, true),
                new VehicleWheel(new Vector3(0.85f, 0.22f, 1.55f), 0.33f, 0, true, true),
                new VehicleWheel(new Vector3(-0.85f, 0.22f, -1.55f), 0.33f, 1, false, true),
                new VehicleWheel(new Vector3(0.85f, 0.22f, -1.55f), 0.33f, 1, false, true)
            },
            springRate: 32000f,
            damper: 1900f,
            restLength: 0.45f,
            travel: 0.25f,
            antiRollRate: 20000f,
            attitudeLevellingRate: 18000f,
            attitudeDamping: 6000f,
            lateralFrictionCoefficient: 3f,
            groundMask: 1,
            surfaceContactTolerance: 0.15f,
            engineTorque: 1600f,
            reverseTorque: 1400f,
            brakeTorque: 2000f,
            coastTorque: 260f,
            handbrakeTorque: 4500f,
            wheelInertia: 3f,
            maxForwardSpeed: 18f,
            maxReverseSpeed: 7f,
            minimumDirectionSpeed: 0.25f,
            maxSteerAngleDegrees: 40f,
            highSpeedSteerAngleDegrees: 16f,
            steerFullReductionSpeed: 26f,
            steerRateDegreesPerSecond: 300f,
            steerReturnRateDegreesPerSecond: 260f,
            tirePeakSlipRatio: 0.14f,
            tirePeakSlipAngleDegrees: 8f,
            tireSlipFalloffFraction: 0.8f,
            // Aides arcade (Story 5.13), toutes actives et CHACUNE bornee par le vehicule : un
            // abattement de lacet de 60 N.m par degre/s, une attenuation de traction de 50 %, un rappel
            // de tete-a-queue de 40 N.m par degre/s au-dela de 30 deg/s de derive. Les bornes viennent du
            // vehicule (adherence x charge x demi-voie, soit 7 504 N.m ici), donc ces valeurs restent
            // un REGLAGE de ressenti, jamais une limite de la couche : les mesures de la recette
            // humaine du 2026-09-18 les ajustent sans toucher au code.
            yawStabilityRate: 60f,
            tractionControlStrength: 0.5f,
            spinRecoveryRate: 40f,
            spinDriftThresholdDegreesPerSecond: 30f);

        /// <summary>Id stable expose sous la forme partagee attendue par les autres couches.</summary>
        public DefinitionId Id
        {
            get { return new DefinitionId(id); }
        }

        /// <summary>Valeur brute de l'id, exposee pour les gardes de validation.</summary>
        public string RawId
        {
            get { return id ?? string.Empty; }
        }

        /// <summary>Profil authore, lu tel quel par le composant physique.</summary>
        public VehicleProfile Profile
        {
            get { return profile; }
        }

        /// <summary>
        /// Refuse tout profil qui ne peut pas tenir un vehicule : masse ou raideur non positive,
        /// longueur au repos qui n'excede pas le debattement (la roue n'aurait aucune course
        /// utilisable), essieu sans paire anti-roulis, masque de sol vide, ou toute valeur non finie.
        /// Le message nomme le champ fautif, jamais un "profil invalide" muet.
        /// </summary>
        public bool TryValidate(out string error)
        {
            if (string.IsNullOrWhiteSpace(RawId) || RawId != RawId.Trim() || RawId != RawId.ToLowerInvariant())
            {
                error = "Id de profil physique invalide.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.Mass, 0f))
            {
                error = "Mass invalide : 'mass' doit etre finie et strictement positive.";
                return false;
            }

            if (!IsFinite(profile.CenterOfMass))
            {
                error = "CenterOfMass invalide : 'centerOfMass' doit etre fini sur les trois axes.";
                return false;
            }

            if (!IsFiniteAndAboveAll(profile.InertiaTensor, 0f))
            {
                error = "InertiaTensor invalide : 'inertiaTensor' doit etre fini et strictement positif sur les trois axes.";
                return false;
            }

            if (profile.WheelCount == 0)
            {
                error = "Wheels invalide : 'wheels' doit porter au moins une roue.";
                return false;
            }

            for (var i = 0; i < profile.WheelCount; i++)
            {
                var wheel = profile.GetWheel(i);
                if (!IsFinite(wheel.LocalPosition))
                {
                    error = "Wheel.position invalide : la roue " + i + " doit avoir une position locale finie sur les trois axes.";
                    return false;
                }

                if (!IsFiniteAndAbove(wheel.Radius, 0f))
                {
                    error = "Wheel.radius invalide : le rayon de la roue " + i + " doit etre fini et strictement positif.";
                    return false;
                }

                if (wheel.AxleIndex < 0)
                {
                    error = "Wheel.axleIndex invalide : l'essieu de la roue " + i + " doit etre superieur ou egal a 0.";
                    return false;
                }
            }

            // L'anti-roulis est un terme de PAIRE : une roue seule sur son essieu n'aurait rien a
            // compenser, et le taux authore y serait silencieusement inerte.
            for (var i = 0; i < profile.WheelCount; i++)
            {
                var axle = profile.GetWheel(i).AxleIndex;
                var count = 0;
                var driven = false;
                var hasReference = false;
                for (var j = 0; j < profile.WheelCount; j++)
                {
                    if (profile.GetWheel(j).AxleIndex != axle)
                    {
                        continue;
                    }

                    count++;

                    // Les deux roues d'un essieu partagent isDriven : une seule roue motrice produirait un
                    // couple de lacet permanent, donc un vehicule qui tire d'un cote en acceleration -- un
                    // reglage casse, jamais un reglage.
                    if (!hasReference)
                    {
                        driven = profile.GetWheel(j).IsDriven;
                        hasReference = true;
                    }
                    else if (profile.GetWheel(j).IsDriven != driven)
                    {
                        error = "Wheels invalide : les deux roues de l'essieu " + axle
                            + " doivent partager 'isDriven', sinon le vehicule tire d'un cote en acceleration.";
                        return false;
                    }
                }

                if (count != 2)
                {
                    error = "Wheels invalide : l'essieu " + axle + " porte " + count
                        + " roue(s), or l'anti-roulis apparie exactement deux roues par essieu.";
                    return false;
                }
            }

            if (!IsFiniteAndAbove(profile.SpringRate, 0f))
            {
                error = "SpringRate invalide : 'springRate' doit etre fini et strictement positif.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.Damper, 0f))
            {
                error = "Damper invalide : 'damper' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.RestLength, 0f))
            {
                error = "RestLength invalide : 'restLength' doit etre fini et strictement positif.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.Travel, 0f))
            {
                error = "Travel invalide : 'travel' doit etre fini et strictement positif.";
                return false;
            }

            // Aucune course utilisable sinon : la roue resterait collee a sa butee, et la garde au
            // sol du chassis ne pourrait jamais absorber la moindre bordure.
            if (profile.RestLength <= profile.Travel)
            {
                error = "RestLength invalide : 'restLength' doit etre strictement superieur a 'travel'.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.AntiRollRate, 0f))
            {
                error = "AntiRollRate invalide : 'antiRollRate' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            // Un rappel d'assiette nul laisse le vehicule couche sur le flanc apres un choc : hors
            // contact, la suspension ne redresse plus rien, donc c'est le seul terme qui le fait.
            if (!IsFiniteAndAbove(profile.AttitudeLevellingRate, 0f))
            {
                error = "AttitudeLevellingRate invalide : 'attitudeLevellingRate' doit etre fini et strictement positif, sinon un vehicule couche ne se redresse jamais.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.AttitudeDamping, 0f))
            {
                error = "AttitudeDamping invalide : 'attitudeDamping' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.SurfaceContactTolerance, 0f))
            {
                error = "SurfaceContactTolerance invalide : 'surfaceContactTolerance' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            // Un frottement lateral nul rend le vehicule insensible a un choc de flanc : il glisse sur
            // de la glace. C'est un contact, pas un confort -- donc il est exige.
            if (!IsFiniteAndAbove(profile.LateralFrictionCoefficient, 0f))
            {
                error = "LateralFrictionCoefficient invalide : 'lateralFrictionCoefficient' (adherence du pneu) doit etre fini et strictement positif, sinon un choc de flanc emporte le vehicule.";
                return false;
            }

            if (profile.GroundMask.value == 0)
            {
                error = "GroundMask invalide : 'groundMask' ne peut pas etre vide, aucune roue ne pourrait toucher le sol.";
                return false;
            }

            // --------------------------------------------------------------- modele de pneu (5.12)

            // Un pic de glissement nul rendrait la courbe de force plate a l'infini : le pneu ne
            // transmettrait jamais rien, ou transmettrait tout d'un coup -- exactement le seuil binaire
            // que le modele interdit.
            if (!IsFiniteAndAbove(profile.TirePeakSlipRatio, 0f))
            {
                error = "TirePeakSlipRatio invalide : 'tirePeakSlipRatio' doit etre fini et strictement positif, sinon le pneu n'a pas de courbe de force.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.TirePeakSlipAngleDegrees, 0f) || profile.TirePeakSlipAngleDegrees > 90f)
            {
                error = "TirePeakSlipAngleDegrees invalide : 'tirePeakSlipAngleDegrees' doit etre fini, strictement positif et au plus egal a 90 degres (au-dela, ce n'est plus un angle de glissement).";
                return false;
            }

            // Une asymptote nulle serait un seuil binaire deguise : au-dela du pic, la force tomberait
            // a zero et la derive deviendrait irrecuperable. Une asymptote de 1 supprimerait la chute.
            if (!IsFiniteAndAtLeast(profile.TireSlipFalloffFraction, 0f) || profile.TireSlipFalloffFraction >= 1f)
            {
                error = "TireSlipFalloffFraction invalide : 'tireSlipFalloffFraction' doit etre fini, superieur ou egal a 0 et strictement inferieur a 1 -- la chute d'adherence doit etre progressive, jamais totale.";
                return false;
            }

            // ------------------------------------------------------------ modele de direction (5.12)

            if (!IsFiniteAndAbove(profile.MaxSteerAngleDegrees, 0f) || profile.MaxSteerAngleDegrees > 90f)
            {
                error = "MaxSteerAngleDegrees invalide : 'maxSteerAngleDegrees' doit etre fini, strictement positif et au plus egal a 90 degres (hors bornes).";
                return false;
            }

            if (!IsFiniteAndAbove(profile.HighSpeedSteerAngleDegrees, 0f)
                || profile.HighSpeedSteerAngleDegrees > profile.MaxSteerAngleDegrees)
            {
                error = "HighSpeedSteerAngleDegrees invalide : 'highSpeedSteerAngleDegrees' doit etre fini, strictement positif et inferieur ou egal a 'maxSteerAngleDegrees' -- sinon la direction s'ouvrirait avec la vitesse.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.SteerFullReductionSpeed, 0f))
            {
                error = "SteerFullReductionSpeed invalide : 'steerFullReductionSpeed' doit etre fini et strictement positif.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.SteerRateDegreesPerSecond, 0f))
            {
                error = "SteerRateDegreesPerSecond invalide : 'steerRateDegreesPerSecond' doit etre fini et strictement positif, sinon les roues ne braquent jamais.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.SteerReturnRateDegreesPerSecond, 0f))
            {
                error = "SteerReturnRateDegreesPerSecond invalide : 'steerReturnRateDegreesPerSecond' doit etre fini et strictement positif, sinon une roue braquee ne revient jamais au centre.";
                return false;
            }

            // ------------------------------------------------------------- train roulant (5.12)

            if (!IsFiniteAndAbove(profile.EngineTorque, 0f))
            {
                error = "EngineTorque invalide : 'engineTorque' doit etre fini et strictement positif, sinon le vehicule ne peut pas demarrer.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.ReverseTorque, 0f))
            {
                error = "ReverseTorque invalide : 'reverseTorque' doit etre fini et strictement positif, sinon la marche arriere est inatteignable.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.BrakeTorque, 0f))
            {
                error = "BrakeTorque invalide : 'brakeTorque' doit etre fini et strictement positif, sinon le frein de service ne freine rien.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.CoastTorque, 0f))
            {
                error = "CoastTorque invalide : 'coastTorque' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.HandbrakeTorque, 0f))
            {
                error = "HandbrakeTorque invalide : 'handbrakeTorque' doit etre fini et strictement positif, sinon le frein a main ne bloque pas les roues arriere et ne fait pas entrer en derive.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.WheelInertia, 0f))
            {
                error = "WheelInertia invalide : 'wheelInertia' doit etre fini et strictement positif, sinon la rotation de la roue n'est pas integrable.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.MaxForwardSpeed, 0f))
            {
                error = "MaxForwardSpeed invalide : 'maxForwardSpeed' doit etre fini et strictement positif.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.MaxReverseSpeed, 0f))
            {
                error = "MaxReverseSpeed invalide : 'maxReverseSpeed' doit etre fini et strictement positif.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.MinimumDirectionSpeed, 0f))
            {
                error = "MinimumDirectionSpeed invalide : 'minimumDirectionSpeed' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            // Au moins une roue motrice et une roue directrice : un vehicule sans roue motrice ne peut
            // pas bouger, un vehicule sans roue directrice ne peut pas tourner -- et les deux seraient
            // des drapeaux authores inertes, exactement ce que la story consomme enfin.
            if (!HasDrivenWheel(profile))
            {
                error = "Wheels invalide : aucune roue n'est motrice ('isDriven'), le vehicule ne pourrait pas se deplacer par ses roues.";
                return false;
            }

            if (!HasSteeringWheel(profile))
            {
                error = "Wheels invalide : aucune roue n'est directrice ('isSteering'), le vehicule ne pourrait pas tourner.";
                return false;
            }

            // Le frein a main agit sur les roues NON directrices : un profil ou toutes les roues braquent
            // rendrait la voie de frein a main silencieusement inerte.
            if (!HasNonSteeringWheel(profile))
            {
                error = "Wheels invalide : aucune roue non directrice, le frein a main n'aurait aucune roue a bloquer.";
                return false;
            }

            // --------------------------------------------------------------- aides arcade (5.13)

            // Trois aides et un seuil. Chacune est DESACTIVABLE par sa propre valeur authoree (nulle =
            // terme inerte, patron deja en place dans la couche), donc aucune n'est exigee strictement
            // positive : ce qui est exige est qu'elle soit FINIE et NON NEGATIVE. Une valeur negative
            // inverserait le sens de l'aide au lieu de l'eteindre, ce qui serait un reglage casse.
            if (!IsFiniteAndAtLeast(profile.YawStabilityRate, 0f))
            {
                error = "YawStabilityRate invalide : 'yawStabilityRate' (abattement du lacet) doit etre fini et superieur ou egal a 0 -- une valeur negative inverserait le sens de l'aide au lieu de l'eteindre.";
                return false;
            }

            // A 1, une roue en glissement total perdrait tout son couple moteur : c'est un seuil binaire
            // deguise, et la valeur doit rester STRICTEMENT sous 1 pour que l'attenuation ne puisse
            // jamais annuler le couple.
            if (!IsFiniteAndAtLeast(profile.TractionControlStrength, 0f) || profile.TractionControlStrength >= 1f)
            {
                error = "TractionControlStrength invalide : 'tractionControlStrength' doit etre fini, superieur ou egal a 0 et strictement inferieur a 1 -- une attenuation totale annulerait le couple moteur, ce qui est exactement le seuil binaire que la couche interdit.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.SpinRecoveryRate, 0f))
            {
                error = "SpinRecoveryRate invalide : 'spinRecoveryRate' (recuperation de tete-a-queue) doit etre fini et superieur ou egal a 0 -- une valeur negative inverserait le sens de l'aide au lieu de l'eteindre.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.SpinDriftThresholdDegreesPerSecond, 0f))
            {
                error = "SpinDriftThresholdDegreesPerSecond invalide : 'spinDriftThresholdDegreesPerSecond' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool HasDrivenWheel(VehicleProfile candidate)
        {
            for (var i = 0; i < candidate.WheelCount; i++)
            {
                if (candidate.GetWheel(i).IsDriven)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSteeringWheel(VehicleProfile candidate)
        {
            for (var i = 0; i < candidate.WheelCount; i++)
            {
                if (candidate.GetWheel(i).IsSteering)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasNonSteeringWheel(VehicleProfile candidate)
        {
            for (var i = 0; i < candidate.WheelCount; i++)
            {
                if (!candidate.GetWheel(i).IsSteering)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        }

        private static bool IsFiniteAndAboveAll(Vector3 value, float exclusiveMinimum)
        {
            return IsFinite(value)
                && value.x > exclusiveMinimum
                && value.y > exclusiveMinimum
                && value.z > exclusiveMinimum;
        }

        private static bool IsFiniteAndAtLeast(float value, float minimum)
        {
            return float.IsFinite(value) && value >= minimum;
        }

        private static bool IsFiniteAndAbove(float value, float exclusiveMinimum)
        {
            return float.IsFinite(value) && value > exclusiveMinimum;
        }

        private void OnValidate()
        {
            if (!TryValidate(out var error))
            {
                Debug.LogWarning("[Vehicles] VehicleProfileDef invalide : " + error, this);
            }
        }
    }
}
