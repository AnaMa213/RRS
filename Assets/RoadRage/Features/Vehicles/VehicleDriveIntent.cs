using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Intention locale de conduite lue depuis l'input, avant soumission au host pour validation
    /// et simulation. Meme patron que OnFootMovementIntent (Epic 1) : struct immuable, clampee au
    /// constructeur, aucune dependance reseau.
    /// </summary>
    public readonly struct VehicleDriveIntent
    {
        public static readonly VehicleDriveIntent Idle = new VehicleDriveIntent(0f, 0f, 0f);

        public VehicleDriveIntent(float throttle, float steer, float brakeReverse)
        {
            Throttle = Mathf.Clamp01(throttle);
            Steer = Mathf.Clamp(steer, -1f, 1f);
            BrakeReverse = Mathf.Clamp01(brakeReverse);
        }

        /// <summary>Acceleration avant, 0 (relache) a 1 (plein gaz).</summary>
        public float Throttle { get; }

        /// <summary>Direction, -1 (gauche) a 1 (droite).</summary>
        public float Steer { get; }

        /// <summary>Frein si le vehicule avance, marche arriere sinon -- 0 (relache) a 1.</summary>
        public float BrakeReverse { get; }

        public bool IsIdle
        {
            get { return Throttle <= 0.0001f && BrakeReverse <= 0.0001f && Mathf.Abs(Steer) <= 0.0001f; }
        }
    }
}
