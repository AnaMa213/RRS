using RoadRage.Shared.Domain;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.9 : socle de decision de conduite IA, porte par IDM (acceleration longitudinale) et
    /// MOBIL (changement de voie), tel que specifie par la recherche du 2026-09-15. Toutes les
    /// fonctions sont pures et sans etat : elles se testent en EditMode sans scene ni Netcode, et
    /// retournent toujours une valeur finie. Aucun etat n'est conserve d'un appel a l'autre --
    /// l'appelant (le controleur) porte les scalaires par instance.
    ///
    /// Vivre ici plutot que dans <see cref="NetworkedAIVehicleDriverController"/> conserve la garde
    /// d'absence d'etat partage du controleur (Story 5.2) au lieu de la diluer.
    /// </summary>
    public static class DriverModel
    {
        /// <summary>Ecart a passer a <see cref="ComputeAcceleration"/> quand aucun leader n'est detecte.</summary>
        public const float NoLeaderGap = float.PositiveInfinity;

        /// <summary>Plancher de la constante de temps de reaction : jamais 0, sinon le lissage divise par zero.</summary>
        public const float MinReactionTime = 0.01f;

        /// <summary>Plancher de l'intervalle d'evaluation de changement de voie.</summary>
        public const float MinLaneChangeEvaluationInterval = 0.01f;

        /// <summary>Exposant d'acceleration de l'IDM (delta), valeur canonique du modele de Treiber.</summary>
        private const float AccelerationExponent = 4f;

        /// <summary>Ecart minimal avant division : sans lui un ecart nul produirait -Inf puis un linearVelocity NaN.</summary>
        private const float GapEpsilon = 0.05f;

        /// <summary>Plancher de vitesse desiree en dessous duquel le profil est lu comme "immobilisation".</summary>
        private const float DesiredSpeedEpsilon = 0.001f;

        /// <summary>
        /// Fraction de l'ecart minimal authore au-dessus de laquelle un arret derriere un leader est
        /// lu comme voulu plutot que comme un encastrement (cf. <see cref="IsDeliberateStop"/>).
        /// </summary>
        private const float DeliberateStopGapFactor = 0.5f;

        /// <summary>
        /// Garde-fou de finitude, pas un reglage : borne la deceleration renvoyee par l'IDM quand
        /// l'ecart s'effondre. Au-dela, la valeur n'est plus physique et contaminerait le Rigidbody.
        /// </summary>
        private const float MaxEmergencyDeceleration = 20f;

        // ponytail: l'enveloppe du bruit de vitesse desiree (amplitude et frequences) est partagee par
        // tous les profils pour cette story ; seul consistency varie. Si un playtest reclame un
        // archetype au tremblement plus large que les autres, en faire des champs du Def.
        private const float NoiseAmplitudeFraction = 0.06f;
        private const float NoisePrimaryFrequency = 0.13f;
        private const float NoiseSecondaryFrequency = 0.29f;
        private const float NoiseSecondaryPhaseRatio = 2.39f;

        /// <summary>
        /// IDM : acceleration longitudinale (m/s2) resultant du terme de route libre et du terme
        /// d'interaction avec le leader. Passer <see cref="NoLeaderGap"/> pour une route libre.
        /// Toujours finie : l'ecart est clampe avant division et le resultat est borne.
        /// </summary>
        public static float ComputeAcceleration(DriverProfile profile, float speed, float leaderSpeed, float gap)
        {
            var maxAcceleration = Mathf.Max(SafeFinite(profile.MaxAcceleration, 0f), GapEpsilon);
            var comfortableDeceleration = Mathf.Max(SafeFinite(profile.ComfortableDeceleration, 0f), GapEpsilon);
            var desiredSpeed = SafeFinite(profile.DesiredSpeed, 0f);
            var currentSpeed = Mathf.Max(SafeFinite(speed, 0f), 0f);

            // Vitesse desiree nulle (disposition immobilisante) : le rapport v/v0 diverge, donc le
            // terme de route libre perd son sens. Le vehicule freine au confort jusqu'a l'arret.
            if (desiredSpeed <= DesiredSpeedEpsilon)
            {
                return currentSpeed <= 0f ? 0f : -comfortableDeceleration;
            }

            var speedRatio = currentSpeed / desiredSpeed;
            var freeRoadTerm = 1f - Mathf.Pow(speedRatio, AccelerationExponent);

            var interactionTerm = 0f;
            if (!float.IsInfinity(gap) && !float.IsNaN(gap))
            {
                var clampedGap = Mathf.Max(gap, GapEpsilon);
                var approachRate = currentSpeed - Mathf.Max(SafeFinite(leaderSpeed, 0f), 0f);
                var desiredGap = SafeFinite(profile.MinimumGap, 0f)
                    + Mathf.Max(
                        0f,
                        (currentSpeed * Mathf.Max(SafeFinite(profile.TimeHeadway, 0f), 0f))
                        + ((currentSpeed * approachRate) / (2f * Mathf.Sqrt(maxAcceleration * comfortableDeceleration))));

                var gapRatio = desiredGap / clampedGap;
                interactionTerm = gapRatio * gapRatio;
            }

            var acceleration = maxAcceleration * (freeRoadTerm - interactionTerm);
            return Mathf.Clamp(SafeFinite(acceleration, -MaxEmergencyDeceleration), -MaxEmergencyDeceleration, maxAcceleration);
        }

        /// <summary>
        /// MOBIL : accepte le changement de voie si le critere de securite passe ET si le gain
        /// pondere par la politesse depasse le seuil du profil. Le veto de securite est evalue
        /// independamment du gain calcule -- un gain enorme ne l'achete pas. <paramref name="gain"/>
        /// est renseigne dans tous les cas, y compris quand le veto tombe.
        /// </summary>
        public static bool TryEvaluateLaneChange(
            DriverProfile profile,
            float selfAccelerationBefore,
            float selfAccelerationAfter,
            float newFollowerAccelerationBefore,
            float newFollowerAccelerationAfter,
            float oldFollowerAccelerationBefore,
            float oldFollowerAccelerationAfter,
            out float gain)
        {
            var selfGain = SafeFinite(selfAccelerationAfter, 0f) - SafeFinite(selfAccelerationBefore, 0f);
            var newFollowerDelta = SafeFinite(newFollowerAccelerationAfter, 0f) - SafeFinite(newFollowerAccelerationBefore, 0f);
            var oldFollowerDelta = SafeFinite(oldFollowerAccelerationAfter, 0f) - SafeFinite(oldFollowerAccelerationBefore, 0f);

            gain = selfGain + (SafeFinite(profile.Politeness, 0f) * (newFollowerDelta + oldFollowerDelta));

            var safeBrakingLimit = Mathf.Max(SafeFinite(profile.SafeBrakingLimit, 0f), 0f);
            if (SafeFinite(newFollowerAccelerationAfter, 0f) < -safeBrakingLimit)
            {
                return false;
            }

            return gain > Mathf.Max(SafeFinite(profile.LaneChangeThreshold, 0f), 0f);
        }

        /// <summary>
        /// Re-expression de la disposition publiee (Story 5.4) comme modulation du profil authore --
        /// la forme "effectif = base x f(...)" exigee par l'AD-33, en remplacement du seul
        /// multiplicateur de vitesse. La rage baisse T, s0, p et a_th, monte a, b, v0 et b_safe
        /// (carte des leviers de la recherche). Block et ConfrontationCapable mettent v0 a 0 : le
        /// vehicule cesse de poursuivre la route. Les trois parametres de personnalite ne sont pas
        /// touches : ils sont orthogonaux a l'emotion.
        /// La Story 5.19 (ex-5.13) remplacera cette entree discrete par les jauges continues rage/peur.
        /// </summary>
        public static DriverProfile ResolveEffectiveProfile(DriverProfile profile, RageDisposition disposition)
        {
            var intensity = ResolveRageIntensity(disposition);
            var immobilizing = disposition == RageDisposition.Block || disposition == RageDisposition.ConfrontationCapable;
            var desiredSpeed = immobilizing ? 0f : Mathf.Max(0f, profile.DesiredSpeed * (1f + intensity));

            return new DriverProfile(
                desiredSpeed,
                Mathf.Max(0f, profile.TimeHeadway * (1f - (0.5f * intensity))),
                Mathf.Max(0f, profile.MinimumGap * (1f - (0.5f * intensity))),
                profile.MaxAcceleration * (1f + intensity),
                profile.ComfortableDeceleration * (1f + intensity),
                profile.Politeness - intensity,
                Mathf.Max(0f, profile.LaneChangeThreshold * (1f - (0.5f * intensity))),
                profile.SafeBrakingLimit * (1f + intensity),
                profile.ReactionTime,
                profile.LaneChangeEvaluationInterval,
                profile.Consistency,
                // Story 5.13 : la vitesse de rappel de cible traverse la modulation SANS etre modifiee.
                // L'omettre laissait le defaut du constructeur (12 m/s) remplacer la valeur authoree :
                // le reglage du Def serait alors sans effet, ce qui est exactement la valeur en dur que
                // la Story 5.9 a supprimee.
                profile.AimPointRecallSpeed,
                profile.PerceptionRadius,
                profile.PerceptionArcDegrees,
                profile.PerceptionInterval,
                profile.HornDelay,
                profile.ReverseDuration,
                profile.RoadDetourGap,
                profile.SidewalkClearanceRadius,
                profile.PredictionSeconds, profile.SafetyMargin, profile.ManeuverSpeed,
                profile.ReverseSpeed, profile.ManeuverTimeout, profile.ProgressTimeout,
                profile.ProgressDistance, profile.RetryCooldown, profile.PathSampleDistance,
                profile.MaxCurbHeight,
                // Story 5.18 : les seuils d'intersection traversent la modulation SANS etre modifies,
                // exactement comme la vitesse de rappel de cible de la 5.13. Les omettre laisserait les
                // defauts du constructeur remplacer les valeurs authorees -- le reglage du Def serait
                // alors sans effet, ce qui est la valeur en dur que la Story 5.9 a supprimee.
                profile.JunctionApproachRadius, profile.JunctionStopHoldSeconds,
                profile.JunctionAcceptedGap, profile.JunctionEscalationDelay,
                profile.JunctionExitClearanceRadius);
        }

        /// <summary>
        /// Lissage de premier ordre de l'acceleration appliquee vers l'acceleration IDM visee : le
        /// mecanisme le plus simple qui exprime un temps de reaction sans tampon d'historique. Le
        /// facteur reste dans l'intervalle ]0, 1], donc la convergence ne depasse jamais la cible.
        /// </summary>
        public static float SmoothAcceleration(float currentAcceleration, float targetAcceleration, float reactionTime, float deltaTime)
        {
            var current = SafeFinite(currentAcceleration, 0f);
            var target = SafeFinite(targetAcceleration, 0f);
            var dt = SafeFinite(deltaTime, 0f);

            if (dt <= 0f)
            {
                return current;
            }

            var tau = Mathf.Max(SafeFinite(reactionTime, MinReactionTime), MinReactionTime);
            var blend = Mathf.Clamp01(1f - Mathf.Exp(-dt / tau));
            return current + ((target - current) * blend);
        }

        /// <summary>
        /// Gating du changement de voie : vrai des que l'intervalle authore du profil est ecoule.
        /// Remplace tout intervalle cable en dur ; la desynchronisation entre vehicules vient de la
        /// phase initiale que l'appelant donne a son compteur, pas d'ici.
        /// </summary>
        public static bool ShouldEvaluateLaneChange(float elapsedSeconds, float evaluationInterval)
        {
            var elapsed = SafeFinite(elapsedSeconds, 0f);
            var interval = Mathf.Max(SafeFinite(evaluationInterval, MinLaneChangeEvaluationInterval), MinLaneChangeEvaluationInterval);
            return elapsed >= interval;
        }

        /// <summary>
        /// Bruit de personnalite : seule la vitesse desiree effective flotte, jamais l'acceleration,
        /// la deceleration ni l'ecart. Deterministe (deux sinusoides de frequence fixe combinees a la
        /// phase de l'instance, aucun Random), borne par l'enveloppe partagee, et exactement nul
        /// quand consistency vaut 1.
        /// </summary>
        public static float ResolveNoisyDesiredSpeed(float desiredSpeed, float consistency, float time, float phase)
        {
            var baseSpeed = Mathf.Max(SafeFinite(desiredSpeed, 0f), 0f);
            var amplitude = NoiseAmplitudeFraction * (1f - Mathf.Clamp01(SafeFinite(consistency, 1f)));

            if (amplitude <= 0f || baseSpeed <= 0f)
            {
                return baseSpeed;
            }

            var t = SafeFinite(time, 0f);
            var p = SafeFinite(phase, 0f);
            var wave = 0.5f * (
                Mathf.Sin((2f * Mathf.PI * NoisePrimaryFrequency * t) + (2f * Mathf.PI * p))
                + Mathf.Sin((2f * Mathf.PI * NoiseSecondaryFrequency * t) + (NoiseSecondaryPhaseRatio * 2f * Mathf.PI * p)));

            return Mathf.Max(0f, baseSpeed * (1f + (amplitude * Mathf.Clamp(wave, -1f, 1f))));
        }

        /// <summary>
        /// Vrai quand l'arret du vehicule est VOULU par le modele : un leader est detecte devant lui
        /// et l'ecart reste raisonnable. Un arret voulu n'est pas un blocage -- il ne doit donc
        /// jamais alimenter la detection "stuck" ni sa recuperation (contrainte d'epique : un
        /// vehicule bloque n'est jamais teleporte).
        ///
        /// L'ecart doit rester au-dessus d'une fraction de l'ecart minimal authore : nez contre
        /// pare-chocs, ce n'est plus un arret propre derriere un leader mais un encastrement, et le
        /// compteur de blocage doit alors reprendre son cours.
        /// </summary>
        public static bool IsDeliberateStop(bool hasLeader, float gap, float minimumGap)
        {
            if (!hasLeader)
            {
                return false;
            }

            var observedGap = SafeFinite(gap, 0f);
            var floor = Mathf.Max(SafeFinite(minimumGap, 0f), 0f) * DeliberateStopGapFactor;
            return observedGap >= floor;
        }

        /// <summary>
        /// Intensite de rage de la disposition, 0 pour Calm (profil de reference, identite). L'ordre
        /// Calm, Irritated, Flee, Ram reproduit l'escalade lisible de la Story 5.4.
        /// </summary>
        private static float ResolveRageIntensity(RageDisposition disposition)
        {
            switch (disposition)
            {
                case RageDisposition.Irritated:
                    return 0.25f;
                case RageDisposition.Flee:
                    return 0.6f;
                case RageDisposition.Ram:
                    return 0.9f;
                default:
                    return 0f;
            }
        }

        /// <summary>Remplace NaN/Inf par un repli : chaque fonction du modele retourne une valeur finie.</summary>
        private static float SafeFinite(float value, float fallback)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? fallback : value;
        }
    }
}
