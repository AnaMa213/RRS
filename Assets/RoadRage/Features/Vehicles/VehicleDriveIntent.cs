using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Intention locale de conduite lue depuis l'input, avant soumission au host pour validation
    /// et simulation. Meme patron que OnFootMovementIntent (Epic 1) : struct immuable, clampee au
    /// constructeur, aucune dependance reseau.
    ///
    /// Story 5.12 : la voie <see cref="Handbrake"/> voyage sur CE chemin d'intent, et sur lui seul --
    /// meme RPC, meme validation serveur, nom de RPC inchange (NFR5). Un second intent ou un second
    /// RPC pour un cinquieme axe aurait cree un second chemin de verite pour la meme intention.
    /// </summary>
    public readonly struct VehicleDriveIntent
    {
        public static readonly VehicleDriveIntent Idle = new VehicleDriveIntent(0f, 0f, 0f, 0f);

        public VehicleDriveIntent(float throttle, float steer, float brakeReverse, float handbrake)
        {
            Throttle = Mathf.Clamp01(throttle);
            Steer = Mathf.Clamp(steer, -1f, 1f);
            BrakeReverse = Mathf.Clamp01(brakeReverse);
            Handbrake = Mathf.Clamp01(handbrake);
        }

        /// <summary>Acceleration avant, 0 (relache) a 1 (plein gaz).</summary>
        public float Throttle { get; }

        /// <summary>Direction, -1 (gauche) a 1 (droite).</summary>
        public float Steer { get; }

        /// <summary>Frein si le vehicule avance, marche arriere sinon -- 0 (relache) a 1.</summary>
        public float BrakeReverse { get; }

        /// <summary>
        /// Frein a main, 0 (relache) a 1 (serre). Il agit sur les roues ARRIERE : il les bloque, ce qui
        /// effondre leur adherence laterale et fait entrer le vehicule en derive. Il ne fait pas partie
        /// de <see cref="IsIdle"/> : un vehicule lance au frein a main n'est pas a l'arret.
        /// </summary>
        public float Handbrake { get; }

        public bool IsIdle
        {
            get { return Throttle <= 0.0001f && BrakeReverse <= 0.0001f && Mathf.Abs(Steer) <= 0.0001f; }
        }
    }
}
