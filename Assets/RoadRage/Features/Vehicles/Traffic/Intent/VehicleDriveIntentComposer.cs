using System;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Intent
{
    /// <summary>Raison stable de l'emission de la commande de repli V2.</summary>
    public enum V2FallbackReason
    {
        None = 0,
        NoCommand = 1,
        OutsideValidityWindow = 2,
        ProfileRefused = 3,
        NonFiniteCommand = 4,
        NonFiniteAuthority = 5,
        NoRoute = 6,
        HorizonNonConforming = 7,
        PlanInfeasible = 8,
        /// <summary>Portail de sortie atteint : horizon de longueur nulle, retrait en attente.</summary>
        ExitPortalReached = 9,
        /// <summary>
        /// Story 5.52 (decision 2a) : borne de suivi depassee en fonctionnement normal ; repli jusqu'a l'arret
        /// maintenu, vehicule present et libre, sans recuperation (5.39).
        /// </summary>
        TrackingToleranceExceeded = 10,
        /// <summary>Story 5.33 : la frame partagee du pas n'a pas pu etre construite ; chaque vehicule recoit le repli.</summary>
        FrameUnavailable = 11,
        /// <summary>Story 5.37 : le SafetyFilter a rejete la commande pour une raison objective.</summary>
        SafetyRejected = 12
    }

    /// <summary>Etat terminal du repli, diagnostique et publie ; le vehicule reste present.</summary>
    public enum V2FallbackTerminal { None = 0, Held = 1, StopOverrun = 2 }

    [Flags]
    public enum V2ComposerDiagnostic
    {
        None = 0,
        Fallback = 1,
        RollingBackward = 2,
        FallbackHeld = 4,
        FallbackStopOverrun = 8,
        ProfileScalarNonFinite = 16
    }

    /// <summary>Sortie d'un pas du composeur : un intent et trois scalaires d'autorite, tous finis.</summary>
    public readonly struct ComposedDrive
    {
        public readonly ulong Step;
        public readonly VehicleDriveIntent Intent;
        public readonly float MaxForwardSpeed;
        public readonly float SteerRateDegreesPerSecond;
        public readonly float BrakeTorque;
        public readonly bool Fallback;
        public readonly V2FallbackReason Reason;
        public readonly V2FallbackTerminal Terminal;
        public readonly V2ComposerDiagnostic Diagnostics;
        public readonly ulong SourceFrameId;

        internal ComposedDrive(ulong step, VehicleDriveIntent intent, float maxForwardSpeed, float steerRate,
            float brakeTorque, bool fallback, V2FallbackReason reason, V2FallbackTerminal terminal,
            V2ComposerDiagnostic diagnostics, ulong sourceFrameId)
        {
            Step = step; Intent = intent; MaxForwardSpeed = maxForwardSpeed;
            SteerRateDegreesPerSecond = steerRate; BrakeTorque = brakeTorque; Fallback = fallback;
            Reason = reason; Terminal = terminal; Diagnostics = diagnostics; SourceFrameId = sourceFrameId;
        }
    }

    /// <summary>
    /// LE composeur V2 (Story 5.31) : exactement un <see cref="VehicleDriveIntent"/> fini par pas
    /// physique, depuis la commande valide courante, ou la commande de repli V2 quand il n'y en a pas
    /// (absente, hors fenetre, profil refuse, axe ou scalaire non fini).
    ///
    /// Repli recalcule a chaque pas depuis la vitesse longitudinale mesuree v :
    /// v &gt; v_s = v_dir + 2 b dt : frein de service seul (b converti par la capacite de frein) ;
    /// v &lt;= v_s, recul compris : frein a main, jamais de BrakeReverse (VehicleTireModel y engagerait la
    /// marche arriere). Etats terminaux Held et StopOverrun diagnostiques ; le vehicule reste present.
    /// Ce n'est pas le SafetyFilter (5.37), et <see cref="VehicleDriveIntent.Idle"/> est inchange.
    /// </summary>
    public sealed class VehicleDriveIntentComposer
    {
        public const float FallbackStoppedSpeedMetersPerSecond = 0.05f;
        public const int FallbackStoppedSteps = 10;
        public const float FallbackOverrunFactor = 2f;

        private readonly VehicleProfile vehicle;
        private readonly float safeBrakingLimit;
        private readonly float fixedDeltaTime;
        private readonly bool profileFinite;
        private readonly float driveCapacity;
        private readonly float brakeCapacity;

        private float lastValidSteer;
        private bool inFallback;
        private float fallbackStartSpeed;
        private int fallbackSteps;
        private int stoppedSteps;
        private V2FallbackTerminal terminal;

        public VehicleDriveIntentComposer(VehicleProfile vehicle, float safeBrakingLimit, float fixedDeltaTime)
        {
            this.vehicle = vehicle;
            this.safeBrakingLimit = safeBrakingLimit;
            this.fixedDeltaTime = fixedDeltaTime;
            profileFinite = Finite(vehicle.MaxForwardSpeed) && Finite(vehicle.SteerRateDegreesPerSecond)
                && Finite(vehicle.BrakeTorque) && Finite(vehicle.MinimumDirectionSpeed) && Finite(safeBrakingLimit)
                && safeBrakingLimit > 0f && Finite(fixedDeltaTime) && fixedDeltaTime > 0f;
            ResolveCapacities(vehicle, out driveCapacity, out brakeCapacity);
        }

        /// <summary>Bande de service v_s = v_dir + 2 b dt (0,41 m/s pour 0,25 m/s, 4 m/s2 et 0,02 s).</summary>
        public float ServiceBandMetersPerSecond
        {
            get { return vehicle.MinimumDirectionSpeed + 2f * safeBrakingLimit * fixedDeltaTime; }
        }

        public bool InFallback { get { return inFallback; } }
        public V2FallbackTerminal Terminal { get { return terminal; } }

        /// <summary>
        /// Compose l'intent du pas <paramref name="step"/>. <paramref name="command"/> nul : pas de commande,
        /// <paramref name="refusal"/> dit pourquoi.
        /// <paramref name="emergencyStop"/> : commande de freinage d'urgence, frein de service plein au-dessus
        /// de la bande de service, frein a main en dessous ; aucune compensation de trainee ne reduit ce freinage.
        /// <paramref name="propulsion"/> faux (but de reponse a collision, 5.38) : jamais de gaz ni de compensation de
        /// trainee, et une demande de freinage sous la bande de service donne le frein a main.
        /// </summary>
        public ComposedDrive Compose(ulong step, MotionCommand? command, V2FallbackReason refusal,
            float measuredLongitudinalSpeed, float linearDamping, bool emergencyStop = false, bool propulsion = true)
        {
            if (!profileFinite)
            {
                // Scalaire de profil non fini : vehicule inerte (maintien, aucune autorite), jamais un nombre invente.
                return new ComposedDrive(step, new VehicleDriveIntent(0f, 0f, 0f, 1f), 0f, 0f, 0f, true,
                    V2FallbackReason.NonFiniteAuthority, terminal,
                    V2ComposerDiagnostic.Fallback | V2ComposerDiagnostic.ProfileScalarNonFinite, 0UL);
            }

            float speed = Finite(measuredLongitudinalSpeed) ? measuredLongitudinalSpeed : 0f;
            V2FallbackReason reason = refusal == V2FallbackReason.None ? V2FallbackReason.NoCommand : refusal;
            if (command.HasValue)
            {
                var value = command.Value;
                if (!value.IsValidAt(step)) reason = V2FallbackReason.OutsideValidityWindow;
                else if (!value.IsFinite) reason = V2FallbackReason.NonFiniteCommand;
                else
                {
                    VehicleDriveIntent intent;
                    if (TryTranslate(value, speed, linearDamping, emergencyStop, propulsion, out intent))
                    {
                        inFallback = false;
                        terminal = V2FallbackTerminal.None;
                        lastValidSteer = intent.Steer;
                        return new ComposedDrive(step, intent, vehicle.MaxForwardSpeed, vehicle.SteerRateDegreesPerSecond,
                            vehicle.BrakeTorque, false, V2FallbackReason.None, V2FallbackTerminal.None,
                            V2ComposerDiagnostic.None, value.SourceFrameId);
                    }
                    reason = V2FallbackReason.NonFiniteCommand;
                }
            }
            return Fallback(step, reason, speed);
        }

        private ComposedDrive Fallback(ulong step, V2FallbackReason reason, float speed)
        {
            if (!inFallback)
            {
                inFallback = true;
                fallbackStartSpeed = Math.Abs(speed);
                fallbackSteps = 0;
                stoppedSteps = 0;
                terminal = V2FallbackTerminal.None;
            }
            fallbackSteps++;
            var diagnostics = V2ComposerDiagnostic.Fallback;
            float steer = Finite(lastValidSteer) ? lastValidSteer : 0f;
            VehicleDriveIntent intent;
            if (speed > ServiceBandMetersPerSecond)
            {
                float service = brakeCapacity > 0f && Finite(brakeCapacity)
                    ? Mathf.Clamp(safeBrakingLimit / brakeCapacity, 1e-4f, 1f) : 1f;
                intent = new VehicleDriveIntent(0f, steer, service, 0f);
            }
            else
            {
                // Maintien : aucune entree de gaz ni de marche arriere, frein a main serre.
                intent = new VehicleDriveIntent(0f, steer, 0f, 1f);
            }
            if (speed < -vehicle.MinimumDirectionSpeed) diagnostics |= V2ComposerDiagnostic.RollingBackward;

            stoppedSteps = Math.Abs(speed) <= FallbackStoppedSpeedMetersPerSecond ? stoppedSteps + 1 : 0;
            if (stoppedSteps >= FallbackStoppedSteps) terminal = V2FallbackTerminal.Held;
            else if (terminal != V2FallbackTerminal.Held
                && fallbackSteps * fixedDeltaTime > FallbackOverrunFactor * (fallbackStartSpeed / safeBrakingLimit))
                terminal = V2FallbackTerminal.StopOverrun;
            if (terminal == V2FallbackTerminal.Held) diagnostics |= V2ComposerDiagnostic.FallbackHeld;
            if (terminal == V2FallbackTerminal.StopOverrun) diagnostics |= V2ComposerDiagnostic.FallbackStopOverrun;
            return new ComposedDrive(step, intent, vehicle.MaxForwardSpeed, vehicle.SteerRateDegreesPerSecond,
                vehicle.BrakeTorque, true, reason, terminal, diagnostics, 0UL);
        }

        /// <summary>
        /// Acceleration et angle vises vers pedales (meme conversion que la capacite V1 : fraction de la
        /// capacite moteur ou de frein du profil, trainee lineaire compensee). Sous la bande de service,
        /// une demande de freinage ne passe jamais par BrakeReverse : roue libre, ou maintien a l'arret.
        /// </summary>
        private bool TryTranslate(MotionCommand command, float speed, float linearDamping, bool emergencyStop, bool propulsion,
            out VehicleDriveIntent intent)
        {
            float acceleration = command.TargetAccelerationMetersPerSecondSquared;
            if (!emergencyStop && propulsion && speed > 0f && Finite(linearDamping) && linearDamping > 0f)
            {
                float retained = Math.Max(0.01f, 1f - linearDamping * fixedDeltaTime);
                acceleration = (acceleration + linearDamping * speed) / retained;
            }
            float lock_ = VehicleSteeringModel.ResolveSteerAngleDegrees(1f, speed, vehicle.MinimumDirectionSpeed,
                vehicle.MaxSteerAngleDegrees, vehicle.HighSpeedSteerAngleDegrees, vehicle.SteerFullReductionSpeed);
            if (!(lock_ > 0f)) lock_ = vehicle.MaxSteerAngleDegrees;
            float steer = lock_ > 0f ? Mathf.Clamp(command.TargetWheelAngleDegrees / lock_, -1f, 1f) : 0f;
            float throttle = 0f, brake = 0f, handbrake = 0f;
            if (emergencyStop)
            {
                // Sous la bande de service, BrakeReverse engagerait la marche arriere : maintien au frein a main.
                if (speed > ServiceBandMetersPerSecond) brake = 1f;
                else handbrake = 1f;
            }
            else if (acceleration > 0f && propulsion)
            {
                float available = driveCapacity * VehicleTireModel.ResolveDriveTorqueFactor(speed, vehicle.MaxForwardSpeed);
                throttle = available > 0f ? Mathf.Clamp01(acceleration / available) : 0f;
            }
            else if (acceleration < 0f)
            {
                if (speed > ServiceBandMetersPerSecond)
                    brake = brakeCapacity > 0f ? Mathf.Clamp01(-acceleration / brakeCapacity) : 0f;
                else if (!propulsion || speed <= FallbackStoppedSpeedMetersPerSecond)
                    handbrake = 1f;
            }
            intent = new VehicleDriveIntent(throttle, steer, brake, handbrake);
            return Finite(throttle) && Finite(steer) && Finite(brake) && Finite(handbrake);
        }

        /// <summary>
        /// Plus petit couple moteur d'une roue motrice pour un intent et la vitesse reelle au moment de
        /// l'application, par la fonction meme de la couche physique. Negatif = marche arriere commandee.
        /// </summary>
        public static float MinimumWheelDriveTorque(VehicleProfile vehicle, VehicleDriveIntent intent,
            float maxForwardSpeed, float longitudinalSpeed)
        {
            float torque = VehicleTireModel.ResolveWheelDriveTorque(intent.Throttle, intent.BrakeReverse, longitudinalSpeed,
                vehicle.MinimumDirectionSpeed, vehicle.EngineTorque, vehicle.ReverseTorque, maxForwardSpeed, vehicle.MaxReverseSpeed);
            float minimum = float.PositiveInfinity;
            for (int i = 0; i < vehicle.WheelCount; i++)
            {
                var wheel = vehicle.GetWheel(i);
                if (!wheel.IsDriven) continue;
                bool handbrake = intent.Handbrake > 0.0001f && !wheel.IsSteering;
                minimum = Math.Min(minimum, handbrake ? 0f : torque);
            }
            return float.IsPositiveInfinity(minimum) ? 0f : minimum;
        }

        /// <summary>Capacites physiques moteur et frein (m/s2) d'un profil, partagees avec le SafetyFilter (5.37).</summary>
        internal static void ResolveCapacities(VehicleProfile vehicle, out float drive, out float brake)
        {
            int driven = 0;
            float radiusSum = 0f;
            for (int i = 0; i < vehicle.WheelCount; i++)
            {
                var wheel = vehicle.GetWheel(i);
                radiusSum += wheel.Radius;
                if (wheel.IsDriven) driven++;
            }
            float meanRadius = vehicle.WheelCount > 0 ? radiusSum / vehicle.WheelCount : 0f;
            float massLoad = vehicle.Mass > 0f && meanRadius > 0f ? 1f / (vehicle.Mass * meanRadius) : 0f;
            drive = driven * Math.Max(0f, vehicle.EngineTorque) * massLoad;
            brake = vehicle.WheelCount * Math.Max(0f, vehicle.BrakeTorque) * massLoad;
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
