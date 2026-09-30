using RoadRage.Features.Vehicles.Traffic.Planning;

namespace RoadRage.Features.Vehicles.Traffic.Lifecycle
{
    /// <summary>
    /// Reponse de fonctionnement normal a <c>TrackingToleranceExceeded</c> (Story 5.52, decision proprietaire 2a).
    /// Hors run de mesure, le premier pas dont la borne depasse l'epsilon_t declare verrouille le vehicule en repli
    /// V2 : a chaque pas suivant, aucune commande, refus <c>TrackingToleranceExceeded</c>, freinage jusqu'a l'arret
    /// maintenu puis maintien. Rien d'autre : ni retrait, ni teleportation, ni realignement, ni contrainte du corps ;
    /// la recuperation apres sortie de couverture appartient a la Story 5.39. Un run de mesure garde le comportement
    /// 5.31 : la campagne juge le depassement.
    /// </summary>
    public sealed class TrackingToleranceResponse
    {
        public bool Latched { get; private set; }
        public ulong LatchedAtStep { get; private set; }

        public static bool Exceeds(TrackingTolerance declared, float displacementMeters)
        {
            return declared.Declared && displacementMeters > declared.Meters;
        }

        /// <summary>Observe la borne du pas ; vrai seulement au pas qui verrouille.</summary>
        public bool Observe(ulong step, bool measurementRun, TrackingTolerance declared, float displacementMeters)
        {
            if (Latched || measurementRun || !Exceeds(declared, displacementMeters)) return false;
            Latched = true;
            LatchedAtStep = step;
            return true;
        }
    }
}
