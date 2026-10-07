using RoadRage.Features.Vehicles.Traffic.Tactical;
using RoadRage.Shared.Definitions;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.9 : donnee auteur du style de conduite d'un vehicule IA. Vit comme asset sous
    /// Assets/RoadRage/ScriptableObjects/Vehicles/ et porte un id globalement unique en minuscules,
    /// stable, sur le meme gabarit que <c>RageTuningDef</c> (Features.Rage) et <c>CharacterDef</c>
    /// (Features.Players). Jamais mute a l'execution.
    ///
    /// Pas de catalogue associe : aucun appelant ne fait de lookup par id avant les Stories 5.19 et
    /// 5.16 -- le Def est reference directement par le prefab du vehicule.
    /// </summary>
    [CreateAssetMenu(fileName = "DriverProfileDef", menuName = "RoadRage/Vehicles/Driver Profile Def")]
    public sealed class DriverProfileDef : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Id globalement unique en minuscules, ex. driver_default. Stable : ne jamais le renommer une fois publie.")]
        private string id = string.Empty;

        [SerializeField]
        [Tooltip("Les douze parametres de conduite. Point de reglage unique : aucune valeur de conduite ne vit dans le controleur.")]
        private DriverProfile profile = new DriverProfile(
            desiredSpeed: 8f,
            timeHeadway: 1.5f,
            minimumGap: 2f,
            maxAcceleration: 1.5f,
            comfortableDeceleration: 2f,
            politeness: 0.25f,
            laneChangeThreshold: 0.2f,
            safeBrakingLimit: 4f,
            reactionTime: 0.3f,
            laneChangeEvaluationInterval: 1f,
            consistency: 0.8f,
            // Story 5.13 : au-dessus de la vitesse de croisiere authoree (8 m/s), donc le rappel ne
            // retarde PAS la visee en conduite nominale -- il ne se voit que sur les 4,8 m d'echelon
            // qu'un franchissement de noeud produit. En dessous de la pointe authoree (18 m/s), donc
            // une visee rapide reste legerement lissee.
            aimPointRecallSpeed: 12f);

        [SerializeField]
        [Tooltip("Story 5.38 : distribution authoree des reactions a une collision significative (Traffic V2) et duree de leur premiere phase. Hors de la struct de conduite : le V1 ne la lit pas.")]
        private CollisionReactionWeights collisionReaction = CollisionReactionWeights.Default;

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

        /// <summary>Profil authore, lu tel quel par le controleur avant modulation par la disposition.</summary>
        public DriverProfile Profile
        {
            get { return profile; }
        }

        /// <summary>Reactions authorees a une collision significative (Story 5.38), lues par la tactique V2.</summary>
        public CollisionReactionWeights CollisionReaction
        {
            get { return collisionReaction; }
        }

        public bool TryValidate(out string error)
        {
            if (string.IsNullOrWhiteSpace(RawId) || RawId != RawId.Trim() || RawId != RawId.ToLowerInvariant())
            {
                error = "Id de profil de conduite invalide.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.DesiredSpeed, 0f))
            {
                error = "DesiredSpeed invalide : 'desiredSpeed' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.TimeHeadway, 0f))
            {
                error = "TimeHeadway invalide : 'timeHeadway' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.MinimumGap, 0f))
            {
                error = "MinimumGap invalide : 'minimumGap' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            // a et b divisent dans l'IDM (racine de a*b) : strictement positifs, pas seulement finis.
            if (!IsFiniteAndAbove(profile.MaxAcceleration, 0f))
            {
                error = "MaxAcceleration invalide : 'maxAcceleration' doit etre fini et strictement positif.";
                return false;
            }

            if (!IsFiniteAndAbove(profile.ComfortableDeceleration, 0f))
            {
                error = "ComfortableDeceleration invalide : 'comfortableDeceleration' doit etre fini et strictement positif.";
                return false;
            }

            // Une politesse negative reste valide : c'est le cadran de malveillance de MOBIL.
            if (!float.IsFinite(profile.Politeness))
            {
                error = "Politeness invalide : 'politeness' doit etre fini.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.LaneChangeThreshold, 0f))
            {
                error = "LaneChangeThreshold invalide : 'laneChangeThreshold' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.SafeBrakingLimit, 0f))
            {
                error = "SafeBrakingLimit invalide : 'safeBrakingLimit' doit etre fini et superieur ou egal a 0.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.ReactionTime, DriverModel.MinReactionTime))
            {
                error = "ReactionTime invalide : 'reactionTime' doit etre fini et superieur ou egal au plancher du modele.";
                return false;
            }

            if (!IsFiniteAndAtLeast(profile.LaneChangeEvaluationInterval, DriverModel.MinLaneChangeEvaluationInterval))
            {
                error = "LaneChangeEvaluationInterval invalide : 'laneChangeEvaluationInterval' doit etre fini et superieur ou egal au plancher du modele.";
                return false;
            }

            if (!float.IsFinite(profile.Consistency) || profile.Consistency < 0f || profile.Consistency > 1f)
            {
                error = "Consistency invalide : 'consistency' doit etre fini et compris entre 0 et 1.";
                return false;
            }

            // Story 5.13 : la vitesse de rappel de la cible est desactivable (nulle = rappel inerte),
            // donc elle n'est pas exigee strictement positive -- mais une valeur negative inverserait le
            // sens du rappel au lieu de l'eteindre.
            if (!IsFiniteAndAtLeast(profile.AimPointRecallSpeed, 0f))
            {
                error = "AimPointRecallSpeed invalide : 'aimPointRecallSpeed' doit etre fini et superieur ou egal a 0 -- une valeur negative inverserait le sens du rappel de cible au lieu de l'eteindre.";
                return false;
            }

            if (!collisionReaction.TryValidate(out error))
            {
                return false;
            }

            error = string.Empty;
            return true;
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
                Debug.LogWarning("[Vehicles] DriverProfileDef invalide : " + error, this);
            }
        }
    }
}
