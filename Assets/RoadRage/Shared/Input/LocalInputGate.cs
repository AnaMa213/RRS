namespace RoadRage.Shared.Input
{
    /// <summary>
    /// Portail statique unique de blocage des entrees locales (Story 5.8). Les lecteurs d'entree
    /// vivent dans plusieurs assemblies de feature qui n'ont pas le droit de referencer RoadRage.App,
    /// mais toutes referencent RoadRage.Shared : un seul drapeau partage evite autant de plomberies
    /// d'activation distinctes qu'il y a de lecteurs.
    ///
    /// Le portail ne concerne que les entrees locales (conduite, deplacement a pied, actions). Il ne
    /// touche ni Time.timeScale ni aucune donnee partagee : une session hebergee continue de simuler
    /// pendant qu'un joueur a son menu ouvert. Il doit etre remis a zero par le proprietaire du menu
    /// (RunEscapeMenuFlowController), sans quoi une scene dechargee menu ouvert laisserait le
    /// drapeau statique bloque pour la scene suivante.
    /// </summary>
    public static class LocalInputGate
    {
        private static bool blocked;

        /// <summary>Vrai tant que les lecteurs d'entree locaux doivent ignorer leurs entrees.</summary>
        public static bool IsBlocked
        {
            get { return blocked; }
        }

        /// <summary>Bloque les entrees locales. Idempotent.</summary>
        public static void Block()
        {
            blocked = true;
        }

        /// <summary>Rend les entrees locales. Idempotent.</summary>
        public static void Release()
        {
            blocked = false;
        }

        /// <summary>Remise a zero du portail, utilisee au teardown et a l'entree du proprietaire du menu.</summary>
        public static void Reset()
        {
            Release();
        }
    }
}
