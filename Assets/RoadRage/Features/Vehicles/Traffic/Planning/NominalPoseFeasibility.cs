using System;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    /// <summary>
    /// Bornes declarees du vehicule et du conducteur V2 pour la faisabilite de l'ensemble de poses nominales
    /// (contrat §8, Story 5.52) : taux de braquage, adherence laterale mu g et vitesse desiree.
    /// </summary>
    public readonly struct NominalPoseFeasibilityInputs
    {
        public readonly float SteerRateDegreesPerSecond;
        public readonly float LateralGripMetersPerSecondSquared;
        public readonly float DesiredSpeedMetersPerSecond;

        public NominalPoseFeasibilityInputs(float steerRateDegreesPerSecond, float lateralGripMetersPerSecondSquared,
            float desiredSpeedMetersPerSecond)
        {
            SteerRateDegreesPerSecond = steerRateDegreesPerSecond;
            LateralGripMetersPerSecondSquared = lateralGripMetersPerSecondSquared;
            DesiredSpeedMetersPerSecond = desiredSpeedMetersPerSecond;
        }

        public bool Valid
        {
            get
            {
                return Positive(SteerRateDegreesPerSecond) && Positive(LateralGripMetersPerSecondSquared)
                    && Positive(DesiredSpeedMetersPerSecond);
            }
        }

        private static bool Positive(float value)
        {
            return value > 0f && !float.IsInfinity(value);
        }
    }

    /// <summary>
    /// Faisabilite de la pose nominale cinematique (contrat §8) : angle de roue implique tan delta = (L/a) tan e
    /// et son taux v |d delta / ds|, avec d delta / ds = f(e) (kappa - sin e / a), f = r / (cos^2 e + r^2 sin^2 e),
    /// r = L/a. Les sauts de e aux raccords, bornes par la tolerance d'admission, ne sont pas des taux (regle du
    /// moniteur 5.31).
    /// </summary>
    public static class NominalPoseFeasibility
    {
        /// <summary>Controle par pas du moniteur 5.31, a la vitesse mesuree (arithmetique inchangee).</summary>
        public static bool FeasibleAtSpeed(DrivabilityProfile drivability, float offsetRadians, float curvature, float speed,
            float steerRateLimitDegreesPerSecond, out float nominalSteerDegrees, out float steerRateDegreesPerSecond)
        {
            float e = offsetRadians;
            float ratio = drivability.WheelbaseMeters / Math.Max(1e-3f, drivability.ReferencePointAheadRearAxleMeters);
            float nominalSteer = Mathf.Atan(ratio * Mathf.Tan(e)) * Mathf.Rad2Deg;
            float offsetRate = curvature - Mathf.Sin(e) / Math.Max(1e-3f, drivability.ReferencePointAheadRearAxleMeters);
            float tan = Mathf.Tan(e);
            float steerRate = Math.Abs(speed) * Math.Abs(ratio / (Mathf.Cos(e) * Mathf.Cos(e) * (1f + ratio * ratio * tan * tan))
                * offsetRate) * Mathf.Rad2Deg;
            nominalSteerDegrees = nominalSteer;
            steerRateDegreesPerSecond = steerRate;
            return Math.Abs(nominalSteer) <= RoadModelCompiler.AvailableLockDegrees(drivability, speed) + 1e-3f
                && steerRate <= steerRateLimitDegreesPerSecond + 1e-3f;
        }

        /// <summary>d delta / ds (rad/m) de l'angle de roue nominal : f(e) (kappa - sin e / a).</summary>
        public static double WheelAngleRatePerMeter(double ratio, double referenceAheadRearAxleMeters, double offsetRadians,
            double curvature)
        {
            double c = Math.Cos(offsetRadians);
            double s = Math.Sin(offsetRadians);
            return ratio / (c * c + ratio * ratio * s * s) * (curvature - s / referenceAheadRearAxleMeters);
        }

        /// <summary>
        /// Borne de Lipschitz en e de d delta / ds pour |kappa| &lt;= <paramref name="absoluteCurvature"/> :
        /// r |r^2 - 1| / min(1, r^2)^2 (|kappa| + 1/a) + max(r, 1/r) / a.
        /// </summary>
        public static double WheelAngleRateLipschitz(double ratio, double referenceAheadRearAxleMeters, double absoluteCurvature)
        {
            double minimumDenominator = Math.Min(1d, ratio * ratio);
            double maximumGain = Math.Max(ratio, 1d / ratio);
            return ratio * Math.Abs(ratio * ratio - 1d) / (minimumDenominator * minimumDenominator)
                * (absoluteCurvature + 1d / referenceAheadRearAxleMeters)
                + maximumGain / referenceAheadRearAxleMeters;
        }
    }
}
