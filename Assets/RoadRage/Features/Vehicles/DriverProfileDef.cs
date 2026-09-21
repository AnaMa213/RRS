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
            aimPointRecallSpeed: 12f,
            perceptionRadius: 20f,
            perceptionArcDegrees: 100f,
            perceptionInterval: 0.2f,
            hornDelay: 2f,
            reverseDuration: 3f,
            roadDetourGap: 5f,
            sidewalkClearanceRadius: 3f,
            predictionSeconds: 3f, safetyMargin: 0.3f, maneuverSpeed: 2f,
            reverseSpeed: 1.2f, maneuverTimeout: 25f, progressTimeout: 4f,
            progressDistance: 0.3f, retryCooldown: 2f, pathSampleDistance: 0.5f,
            maxCurbHeight: 0.15f,
            // Story 5.18 : seuils d'intersection. Ils vivent ici pour la meme raison que ceux de la
            // perception : deux archetypes peuvent franchir une jonction differemment, et le controleur
            // ne porte aucune distance, aucune duree et aucun ecart de conduite code en dur.
            junctionApproachRadius: 12f, junctionStopHoldSeconds: 1.2f,
            junctionAcceptedGap: 4f, junctionEscalationDelay: 12f,
            junctionExitClearanceRadius: 6f);

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

            if (!IsFiniteAndAbove(profile.PerceptionRadius, 0f)
                || !IsFiniteAndAbove(profile.PerceptionArcDegrees, 0f) || profile.PerceptionArcDegrees > 180f
                || !IsFiniteAndAbove(profile.PerceptionInterval, 0f)
                || !IsFiniteAndAtLeast(profile.HornDelay, 0f)
                || !IsFiniteAndAtLeast(profile.ReverseDuration, 0f)
                || !IsFiniteAndAtLeast(profile.RoadDetourGap, 0f)
                || !IsFiniteAndAtLeast(profile.SidewalkClearanceRadius, 0f)
                || !IsFiniteAndAbove(profile.PredictionSeconds, 0f)
                || !IsFiniteAndAbove(profile.SafetyMargin, 0f)
                || !IsFiniteAndAbove(profile.ManeuverSpeed, 0f)
                || !IsFiniteAndAbove(profile.ReverseSpeed, 0f)
                || !IsFiniteAndAbove(profile.ManeuverTimeout, 0f)
                || !IsFiniteAndAbove(profile.ProgressTimeout, 0f)
                || !IsFiniteAndAbove(profile.ProgressDistance, 0f)
                || !IsFiniteAndAbove(profile.RetryCooldown, 0f)
                || !IsFiniteAndAbove(profile.PathSampleDistance, 0f) || profile.PathSampleDistance > 0.5f
                || !IsFiniteAndAbove(profile.MaxCurbHeight, 0f)
                || profile.ProgressTimeout > profile.ManeuverTimeout)
            {
                error = "Reglages de perception ou de deverrouillage invalides.";
                return false;
            }
            // Story 5.18 : les seuils d'intersection suivent le meme chemin que ceux de la perception.
            // L'ecart accepte et le maintien a l'arret peuvent etre nuls (un stop immediat, un stop
            // colle), mais pas negatifs ; le rayon d'approche et la place exigee a la sortie, eux,
            // divisent la zone de degagement, donc ils sont strictement positifs.
            if (!IsFiniteAndAbove(profile.JunctionApproachRadius, 0f)
                || !IsFiniteAndAtLeast(profile.JunctionStopHoldSeconds, 0f)
                || !IsFiniteAndAtLeast(profile.JunctionAcceptedGap, 0f)
                || !IsFiniteAndAtLeast(profile.JunctionEscalationDelay, 0f)
                || !IsFiniteAndAbove(profile.JunctionExitClearanceRadius, 0f))
            {
                error = "Reglages d'intersection invalides.";
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
