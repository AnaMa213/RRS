namespace RoadRage.Features.Vehicles.Traffic.Diagnostics
{
    /// <summary>
    /// Travail effectue par le pipeline Traffic V2, compte depuis le debut de la session (diagnostic de performance 5.33,
    /// rond-point contre ligne droite). Des increments seulement : aucune lecture runtime, aucune influence sur une decision.
    /// Un lecteur prend un instantane avant et apres une section et en fait la difference.
    /// </summary>
    public struct TrafficV2Work
    {
        /// <summary>RoadCurve.Project : appels, segments de la plage (ce que parcourait la boucle lineaire), segments evalues.</summary>
        public long ProjectCalls, ProjectSegmentsIterated, ProjectSegmentsEvaluated;
        /// <summary>RoadCurve.ProjectNearest (occupation) : appels, segments de la plage, segments evalues apres elagage.</summary>
        public long NearestCalls, NearestSegmentsIterated, NearestSegmentsEvaluated;
        /// <summary>Transport de e (RoadCurve.AdvanceKinematicOffset) : appels et pas RK4 (4 sinus chacun).</summary>
        public long KinematicCalls, KinematicSteps;
        /// <summary>RoadLocalizer.Localize : appels, elements examines, elements projetes (boite), candidats retenus.</summary>
        public long LocalizeCalls, LocalizeScanned, LocalizeProjected, LocalizeCandidates;
        /// <summary>PathHorizon.Build : constructions, points construits, intervalles.</summary>
        public long HorizonBuilds, HorizonPoints, HorizonSpans;
        /// <summary>RoutePath.Build (chemin de perception) : constructions et etendues.</summary>
        public long RoutePathBuilds, RoutePathSpans;
        /// <summary>RoutePlanner : recherches completes (hors reutilisation du plan existant).</summary>
        public long RouteSearches;
        /// <summary>
        /// TrafficPerception.Observe : appels ; obstacles : points lus pour la boite de requete, entrees spatiales
        /// examinees, projections (entree x etendue) ; intentions : paires (etendue publiee x etendue propre), zones de
        /// conflit examinees, plages de zone calculees.
        /// </summary>
        public long PerceptionCalls, ObstacleBoxPoints, ObstacleEntries, ObstacleProjections, IntentPairs, ConflictZoneTests,
            ZoneSpans;
        /// <summary>MotionPlan.VerifySpeedProfile : verifications et noeuds evalues.</summary>
        public long VerifyCalls, VerifyKnots;
        /// <summary>
        /// Coordination de carrefour (Story 5.34) : index construits (un par modele), rapports d'acteur construits,
        /// lots resolus, demandes examinees et paires de mouvements testees. Le travail d'un lot depend des demandes, des
        /// grants tenus et des occupants du carrefour concerne, jamais du nombre de mouvements ou de zones du modele.
        /// </summary>
        public long JunctionIndexBuilds, JunctionReports, JunctionBatches, JunctionRequests, JunctionPairChecks;
        /// <summary>
        /// Regles authorees (Story 5.35) : preseances lues (table precalculee), creneaux t_gap evalues, grants par creneau de
        /// fusion et ruptures d'interblocage. Bornes par les demandes et grants du carrefour concerne.
        /// </summary>
        public long JunctionPrecedenceChecks, JunctionGapEvaluations, JunctionMergeGapGrants, JunctionDeadlockBreaks;
        /// <summary>Faits de chaines et paires de traversees construits au premier acces, une fois par modele/chaine.</summary>
        public long JunctionTraversalBuilds, JunctionTraversalPairBuilds;
    }

    public static class TrafficV2WorkCounters
    {
        /// <summary>Compteurs cumules, thread principal seulement.</summary>
        public static TrafficV2Work Work;
    }
}
