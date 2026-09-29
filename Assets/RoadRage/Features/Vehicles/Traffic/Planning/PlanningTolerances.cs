namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    public static class PlanningTolerances
    {
        public const float SeamCurvatureJumpPerMeter = 0.001f;
        public const float MaximumCurvatureSlopePerSquareMeter = 0.30f;
        public const float RoundaboutCurvaturePerMeter = 1f / 6f;

        // Verificateur de profil : a = (v1^2 - v0^2) / (2 ds) est calcule en double a partir de
        // flottants (ds est une soustraction flottante, un profil genere arrondit v en flottant) ;
        // l'erreur relative ~1e-7 sur v^2 / 2 ds reste sous 1e-3 m/s2 tant que v^2 / 2 ds < ~8 000.
        // 1e-3 m/s2 est bien en dessous de tout depassement de borne physiquement distinct.
        public const float AccelerationBoundToleranceMetersPerSecondSquared = 1e-3f;

        // Le profil candidat doit couvrir tout l'horizon [0, LengthMeters] ; 1 mm absorbe l'arrondi
        // flottant de la longueur, dix fois sous le raccord tolere (0,05 m).
        public const float ProfileSpanToleranceMeters = 1e-3f;
    }
}
