using System;

namespace RoadRage.Features.Vehicles.Traffic.Coordination
{
    /// <summary>
    /// Cinematique declaree d'un acteur pour l'acceptation de creneau (Story 5.35, P6) : vitesse tangentielle de la frame,
    /// acceleration maximale et vitesse desiree du profil, acceleration laterale admise (min(confort, adherence)) et longueur
    /// de l'empreinte. Une cinematique absente ou non finie vaut <see cref="Known"/> faux : aucune preuve de creneau.
    /// </summary>
    public readonly struct JunctionKinematics
    {
        public readonly float SpeedMetersPerSecond;
        public readonly float MaxAccelerationMetersPerSecondSquared;
        public readonly float DesiredSpeedMetersPerSecond;
        public readonly float LateralAccelerationMetersPerSecondSquared;
        public readonly float LengthMeters;

        public JunctionKinematics(float speed, float maxAcceleration, float desiredSpeed, float lateralAcceleration, float length)
        {
            SpeedMetersPerSecond = speed; MaxAccelerationMetersPerSecondSquared = maxAcceleration; DesiredSpeedMetersPerSecond = desiredSpeed;
            LateralAccelerationMetersPerSecondSquared = lateralAcceleration; LengthMeters = length;
        }

        public bool Known
        {
            get
            {
                return Finite(SpeedMetersPerSecond) && SpeedMetersPerSecond >= 0f && Finite(MaxAccelerationMetersPerSecondSquared)
                    && MaxAccelerationMetersPerSecondSquared > 0f && Finite(DesiredSpeedMetersPerSecond) && DesiredSpeedMetersPerSecond > 0f
                    && LateralAccelerationMetersPerSecondSquared > 0f && Finite(LengthMeters) && LengthMeters >= 0f;
            }
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    /// <summary>
    /// Fonctions pures et bornees de l'acceptation de creneau (Story 5.35, P6), en forme close :
    /// <list type="bullet">
    /// <item>t_clear(D, v, a, v_max) : temps minimal pour parcourir D depuis v, acceleration a jusqu'a v_max, sans depasser v_max
    /// (une vitesse initiale superieure est ramenee a v_max : temps plus long, donc conservatif pour le demandeur) ;</item>
    /// <item>ETA(D, v, a, v0) : meme forme, sans ramener instantanement une vitesse reelle superieure a v0 (borne basse conservative) ;</item>
    /// <item>t_gap = t_clear + t_lat + m_gap.</item>
    /// </list>
    /// </summary>
    public static class JunctionPriority
    {
        /// <summary>m_gap (P6) : seule marge declaree, avant mesure.</summary>
        public const float JunctionGapMarginSeconds = 1.0f;

        /// <summary>Stop (P6) : arret marque sous cette vitesse tangentielle, pare-chocs dans la fenetre d'arret de la frontiere.</summary>
        public const float StopHaltSpeedMetersPerSecond = 0.05f;

        /// <summary>Tolerance sous m_ctrl de l'arret marque P6, decision proprietaire du 2026-10-06 (m).</summary>
        public const float StopHaltIntegrationToleranceMeters = 0.02f;

        /// <summary>Temps minimal pour parcourir <paramref name="distance"/> (m) ; +inf si impossible, 0 si distance &lt;= 0.</summary>
        public static float TravelSeconds(float distance, float speed, float acceleration, float maxSpeed)
        {
            if (float.IsNaN(distance) || float.IsNaN(speed) || float.IsNaN(acceleration) || float.IsNaN(maxSpeed)) return float.PositiveInfinity;
            if (!(distance > 0f)) return 0f;
            if (!(maxSpeed > 0f)) return float.PositiveInfinity;
            double v = Math.Min(Math.Max(0d, speed), maxSpeed);
            double a = Math.Max(0d, acceleration);
            if (!(a > 0d)) return v > 0d ? (float)(distance / v) : float.PositiveInfinity;
            double accelerating = (maxSpeed * (double)maxSpeed - v * v) / (2d * a);
            if (distance <= accelerating) return (float)((Math.Sqrt(v * v + 2d * a * distance) - v) / a);
            return (float)((maxSpeed - v) / a + (distance - accelerating) / maxSpeed);
        }

        /// <summary>
        /// ETA minimal : acceleration jusqu'a v0, ou vitesse reelle conservee si elle depasse deja v0. Ne suppose aucune
        /// deceleration instantanee du prioritaire/titulaire ; le temps de degagement du demandeur garde son plafond conservatif.
        /// </summary>
        public static float EarliestArrivalSeconds(float distance, float speed, float acceleration, float desiredSpeed)
        {
            return TravelSeconds(distance, speed, acceleration, Math.Max(desiredSpeed, speed));
        }

        /// <summary>Plafond de vitesse d'un mouvement : min(v0, √(a_lat / κmax)) ; v0 si le mouvement est droit.</summary>
        public static float MovementSpeedCap(float desiredSpeed, float lateralAcceleration, float maxCurvature)
        {
            float cap = desiredSpeed;
            if (maxCurvature > 0f && lateralAcceleration > 0f) cap = Math.Min(cap, (float)Math.Sqrt(lateralAcceleration / maxCurvature));
            return cap;
        }

        /// <summary>t_gap = t_clear + t_lat + m_gap.</summary>
        public static float GapSeconds(float clearSeconds, float latencySeconds)
        {
            return clearSeconds + latencySeconds + JunctionGapMarginSeconds;
        }
    }
}
