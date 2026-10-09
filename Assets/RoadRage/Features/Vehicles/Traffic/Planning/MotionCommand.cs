using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    /// <summary>
    /// Commande immediate de suivi (Story 5.31) : acceleration visee et angle de roue vise, avec la
    /// frame source et une fenetre de validite en pas physiques [ValidFromStep, ValidToStep]. Elle ne
    /// porte aucune pedale : la traduction en intent appartient au seul composeur.
    /// </summary>
    public readonly struct MotionCommand
    {
        /// <summary>Gain de correction laterale (1/s) de la loi de suivi, au point de reference.</summary>
        public const float LateralGainPerSecond = 1.5f;

        /// <summary>Vitesse d'adoucissement (m/s) du terme lateral : il reste borne a l'arret.</summary>
        public const float LateralSofteningSpeedMetersPerSecond = 1f;

        /// <summary>Distance de visee minimale du suivi de vitesse (m) : sans elle, un vehicule arrete ne repartirait pas.</summary>
        public const float PreviewFloorMeters = 0.001f;

        public readonly ulong SourceFrameId;
        public readonly ulong ValidFromStep;
        public readonly ulong ValidToStep;
        public readonly float TargetAccelerationMetersPerSecondSquared;
        public readonly float TargetWheelAngleDegrees;
        public readonly SpeedConstraint Binding;
        public readonly float LateralErrorMeters;
        public readonly float HeadingErrorDegrees;
        public readonly float ReferenceCurvaturePerMeter;
        public readonly float ReferenceSMeters;
        private readonly IReadOnlyList<EffectiveRuleException> ruleExceptions;

        public MotionCommand(ulong sourceFrameId, ulong validFromStep, ulong validToStep, float targetAcceleration,
            float targetWheelAngleDegrees, SpeedConstraint binding = SpeedConstraint.None, float lateralError = 0f,
            float headingError = 0f, float referenceCurvature = 0f, float referenceS = 0f)
        {
            SourceFrameId = sourceFrameId; ValidFromStep = validFromStep; ValidToStep = validToStep;
            TargetAccelerationMetersPerSecondSquared = targetAcceleration;
            TargetWheelAngleDegrees = targetWheelAngleDegrees; Binding = binding;
            LateralErrorMeters = lateralError; HeadingErrorDegrees = headingError;
            ReferenceCurvaturePerMeter = referenceCurvature; ReferenceSMeters = referenceS;
            ruleExceptions = null;
        }

        private MotionCommand(MotionCommand command, IReadOnlyList<EffectiveRuleException> exceptions)
        {
            this = command;
            ruleExceptions = exceptions;
        }

        /// <summary>
        /// Story 5.41 (P6) : exceptions effectives du vehicule a la frame de la commande, publiees par l'autorite des regles ;
        /// jamais nul. Aucun consommateur avant 5.42.
        /// </summary>
        public IReadOnlyList<EffectiveRuleException> RuleExceptions
        {
            get { return ruleExceptions ?? EffectiveRuleException.None; }
        }

        /// <summary>Copie portant les exceptions effectives ; nul vaut aucune.</summary>
        public MotionCommand WithRuleExceptions(IReadOnlyList<EffectiveRuleException> exceptions)
        {
            return new MotionCommand(this, exceptions);
        }

        public bool IsValidAt(ulong step) { return step >= ValidFromStep && step <= ValidToStep; }

        public bool IsFinite
        {
            get
            {
                return !float.IsNaN(TargetAccelerationMetersPerSecondSquared)
                    && !float.IsInfinity(TargetAccelerationMetersPerSecondSquared)
                    && !float.IsNaN(TargetWheelAngleDegrees) && !float.IsInfinity(TargetWheelAngleDegrees);
            }
        }

        /// <summary>
        /// Angle de roue (degres) qui fait suivre au point de reference une courbure donnee, par la
        /// geometrie declaree du modele (meme relation que <see cref="RoadModelCompiler.RadiusMeters"/>).
        /// Signe comme la courbure : positif vers la droite.
        /// </summary>
        public static float FeedforwardWheelAngleDegrees(DrivabilityProfile profile, float curvaturePerMeter)
        {
            float curvature = Math.Abs(curvaturePerMeter);
            if (!(curvature > 0f) || float.IsInfinity(curvature)) return 0f;
            double radius = 1d / curvature;
            double offset = profile.ReferencePointAheadRearAxleMeters;
            double angle = radius > offset
                ? Math.Atan(profile.WheelbaseMeters / Math.Sqrt(radius * radius - offset * offset)) * 180d / Math.PI
                : profile.LowSpeedLockDegrees;
            return (float)(Math.Sign(curvaturePerMeter) * Math.Min(angle, profile.LowSpeedLockDegrees));
        }

        /// <summary>
        /// Commande de suivi de la reference compilee. Acceleration : celle de l'arbitrage longitudinal (5.33) quand il
        /// est fourni, sinon min(IDM route libre, suivi du plan) et freinage a SafeBrakingLimit si le plafond est
        /// inatteignable (5.31). Angle : anticipation de la courbure au
        /// point de reference plus correction de cap et d'ecart lateral, bornee au braquage declare.
        /// </summary>
        /// <param name="nominalHeadingErrorDegrees">
        /// Ecart de cap nominal de la route (-e, pose nominale cinematique du contrat §8), fourni par le driver V2.
        /// Absent : ecart du regime etabli, qui saute aux discontinuites de courbure (bancs synthetiques seulement).
        /// </param>
        /// <param name="longitudinal">
        /// Decision de l'arbitrage longitudinal (5.33) : son acceleration appliquee devient l'acceleration visee.
        /// Absente : comportement 5.31 au bit pres.
        /// </param>
        public static MotionCommand Track(ulong frameId, int validitySteps, SpeedPlan plan, DriverProfile driver,
            DrivabilityProfile drivability, RoadCurve curve, float progressSMeters, Vector3 referencePoint,
            Vector3 forward, float speedMetersPerSecond, float fixedDeltaTime, float? nominalHeadingErrorDegrees = null,
            LongitudinalDecision longitudinal = null)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            if (curve == null) throw new ArgumentNullException("curve");
            if (validitySteps < 1) throw new ArgumentOutOfRangeException("validitySteps");
            float speed = Math.Max(0f, speedMetersPerSecond);

            float acceleration;
            if (longitudinal != null) acceleration = longitudinal.AppliedAccelerationMetersPerSecondSquared;
            else if (plan.Binding == SpeedConstraint.SteeringCeilingUnreachable || !(fixedDeltaTime > 0f))
                acceleration = -driver.SafeBrakingLimit;
            else
            {
                // Suivi du plan sur un pas : la vitesse visee au pas suivant est celle du plan a la distance
                // parcourue pendant ce pas (plancher 1 mm pour demarrer de l'arret).
                float preview = Math.Max(speed * fixedDeltaTime, PreviewFloorMeters);
                float tracking = (plan.SpeedAt(preview) - speed) / fixedDeltaTime;
                acceleration = Math.Min(DriverModel.ComputeAcceleration(driver, speed, 0f, DriverModel.NoLeaderGap), tracking);
            }

            float lateral, heading, curvature, referenceS;
            float angle = TrackingWheelAngleDegrees(drivability, curve, progressSMeters, referencePoint, forward, speed,
                nominalHeadingErrorDegrees, out lateral, out heading, out curvature, out referenceS);
            return new MotionCommand(frameId, frameId, frameId + (ulong)(validitySteps - 1), acceleration, angle,
                plan.Binding, lateral, heading, curvature, referenceS);
        }

        /// <summary>
        /// Loi d'angle de <see cref="Track"/> (anticipation de courbure, correction de cap et d'ecart lateral, bornee au
        /// braquage declare), partagee avec le realignement de recuperation (Story 5.39) : projection sur
        /// [s - 1, s + 4] de la courbe autour de <paramref name="progressSMeters"/>.
        /// </summary>
        /// <param name="speedMetersPerSecond">Vitesse positive ou nulle (adoucissement du terme lateral).</param>
        public static float TrackingWheelAngleDegrees(DrivabilityProfile drivability, RoadCurve curve, float progressSMeters,
            Vector3 referencePoint, Vector3 forward, float speedMetersPerSecond, float? nominalHeadingErrorDegrees,
            out float lateral, out float heading, out float curvature, out float referenceSMeters)
        {
            if (curve == null) throw new ArgumentNullException("curve");
            float speed = speedMetersPerSecond;
            var projection = curve.Project(referencePoint, progressSMeters - 1f, progressSMeters + 4f);
            lateral = projection.LateralOffsetMeters;
            heading = projection.Point.SignedHeadingDegrees(forward);
            curvature = projection.Point.CurvaturePerMeter;
            referenceSMeters = projection.SMeters;
            float feedforward;
            float expectedHeading;
            if (nominalHeadingErrorDegrees.HasValue)
            {
                // Pose nominale cinematique (contrat §8) : l'ecart e = -cap nominal fixe l'angle de roue qui garde
                // le point de reference sur la courbe, tan delta = (L/a) tan e. Continu, il anticipe kappa a travers
                // la solution de route au lieu de sauter avec elle (le regime etabli saute aux raccords).
                float e = -nominalHeadingErrorDegrees.Value * Mathf.Deg2Rad;
                feedforward = Mathf.Atan(drivability.WheelbaseMeters / Math.Max(1e-3f, drivability.ReferencePointAheadRearAxleMeters)
                    * Mathf.Tan(e)) * Mathf.Rad2Deg;
                expectedHeading = nominalHeadingErrorDegrees.Value;
            }
            else
            {
                feedforward = FeedforwardWheelAngleDegrees(drivability, curvature);
                // Regime etabli (bancs synthetiques) : le point de reference avance selon un cap tourne de
                // beta = atan(a tan(delta) / L) par rapport a la caisse.
                expectedHeading = -Mathf.Atan(drivability.ReferencePointAheadRearAxleMeters
                    * Mathf.Tan(feedforward * Mathf.Deg2Rad) / drivability.WheelbaseMeters) * Mathf.Rad2Deg;
            }
            float correction = -(heading - expectedHeading) - Mathf.Atan(LateralGainPerSecond * lateral
                / (LateralSofteningSpeedMetersPerSecond + speed)) * Mathf.Rad2Deg;
            return Mathf.Clamp(feedforward + correction, -drivability.LowSpeedLockDegrees, drivability.LowSpeedLockDegrees);
        }
    }
}
