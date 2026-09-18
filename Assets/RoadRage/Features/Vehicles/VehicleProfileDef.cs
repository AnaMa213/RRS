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
            wheels: new[]
            {
                new VehicleWheel(new Vector3(-0.85f, 0.22f, 1.55f), 0.33f, 0, true, true),
                new VehicleWheel(new Vector3(0.85f, 0.22f, 1.55f), 0.33f, 0, true, false),
                new VehicleWheel(new Vector3(-0.85f, 0.22f, -1.55f), 0.33f, 1, false, true),
                new VehicleWheel(new Vector3(0.85f, 0.22f, -1.55f), 0.33f, 1, false, false)
            },
            springRate: 32000f,
            damper: 1900f,
            restLength: 0.45f,
            travel: 0.25f,
            antiRollRate: 20000f,
            attitudeLevellingRate: 18000f,
            attitudeDamping: 6000f,
            lateralFrictionCoefficient: 1.2f,
            rollingResistanceCoefficient: 0.03f,
            groundMask: 1,
            surfaceContactTolerance: 0.15f);

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
                for (var j = 0; j < profile.WheelCount; j++)
                {
                    if (profile.GetWheel(j).AxleIndex == axle)
                    {
                        count++;
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
                error = "LateralFrictionCoefficient invalide : 'lateralFrictionCoefficient' doit etre fini et strictement positif, sinon un choc de flanc emporte le vehicule.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.RollingResistanceCoefficient, 0f))
            {
                error = "RollingResistanceCoefficient invalide : 'rollingResistanceCoefficient' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (profile.GroundMask.value == 0)
            {
                error = "GroundMask invalide : 'groundMask' ne peut pas etre vide, aucune roue ne pourrait toucher le sol.";
                return false;
            }

            error = string.Empty;
            return true;
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
