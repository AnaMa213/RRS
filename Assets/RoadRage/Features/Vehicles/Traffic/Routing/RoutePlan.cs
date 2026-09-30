using System;
using System.Collections.Generic;

namespace RoadRage.Features.Vehicles.Traffic.Routing
{
    /// <summary>Issue globale d'une demande : planifie, replanifie, sans route, ou refusee en entree.</summary>
    public enum RouteOutcome { Planned = 0, Replanned = 1, NoRoute = 2, InvalidInput = 3 }

    /// <summary>
    /// Cause stable du resultat. Les chemins nominaux portent <see cref="Requested"/> ; les fautes
    /// d'appel (modele nul, identite de trafic vide, domaine vide) partagent <see cref="InvalidStart"/>
    /// avec une localisation inexploitable ; <see cref="StaleLocalization"/> signale une localisation
    /// absente ou d'une autre version du modele.
    /// </summary>
    public enum RouteReason { Requested = 0, StalePlan = 1, InvalidStart = 2, StaleLocalization = 3,
        DestinationUnavailable = 4, DestinationUnreachable = 5,
        // Objectif de mouvement intermediaire (contrat Road World Model §4, 2026-09-29) : phase en echec nommee.
        NoRouteToObjective = 6, NoRouteAfterObjective = 7, ObjectiveUnknown = 8 }

    /// <summary>Diagnostics non bloquants ; plusieurs drapeaux peuvent coexister.</summary>
    [Flags]
    public enum RouteDiagnostic { None = 0, ZeroWeightFallback = 1 }

    /// <summary>
    /// Graine de session de l'aleatoire de preference. Type distinct, sans conversion implicite,
    /// pour interdire l'echange silencieux avec <see cref="DecisionCounter"/>.
    /// </summary>
    public readonly struct RouteSeed
    {
        public readonly ulong Value;

        public RouteSeed(ulong value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Compteur de decision dans un domaine. Type distinct, sans conversion implicite, pour
    /// interdire l'echange silencieux avec <see cref="RouteSeed"/>.
    /// </summary>
    public readonly struct DecisionCounter
    {
        public readonly ulong Value;

        public DecisionCounter(ulong value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Requete immuable de planification strategique ; remplace la signature positionnelle.
    /// </summary>
    public readonly struct RouteRequest
    {
        /// <summary>Modele compile ; sa version et son identite sont verifiees contre la localisation.</summary>
        public readonly CompiledRoadModel Model;
        /// <summary>Localisation courante ; elle doit appartenir au modele et a sa version.</summary>
        public readonly RoadLocation Location;
        /// <summary>Sortie visee ; vide signifie "toute sortie disponible" et le resultat nomme le portail retenu.</summary>
        public readonly RoadId DestinationExitId;
        /// <summary>Graine de session de l'aleatoire de preference.</summary>
        public readonly RouteSeed Seed;
        /// <summary>Identite trafic stable, jamais `NetworkObjectId` seul.</summary>
        public readonly RoadId TrafficId;
        /// <summary>Domaine de decision, qui isole l'aleatoire par usage avec le compteur.</summary>
        public readonly string DecisionDomain;
        /// <summary>Compteur de decision dans le domaine.</summary>
        public readonly DecisionCounter Counter;
        /// <summary>Plan candidat a la reutilisation ; il doit etre re-emis tel quel (egalite exacte du contrat).</summary>
        public readonly RoutePlan Existing;
        /// <summary>Force un nouveau plan meme si `Existing` resterait reutilisable.</summary>
        public readonly bool Replan;
        /// <summary>Sorties considerees fermees ; `null` signifie aucune fermeture.</summary>
        public readonly IReadOnlyCollection<RoadId> ClosedPortalIds;
        /// <summary>
        /// Objectif de mouvement intermediaire (contrat §4) : mouvement a traverser avant la sortie ; vide sinon.
        /// Champ ordinaire : seul le chemin de mesure du cycle de vie V2 le pose (garde structurelle 5.31).
        /// </summary>
        public readonly RoadId ViaMovementId;

        public RouteRequest(CompiledRoadModel model, RoadLocation location, RoadId destinationExitId,
            RouteSeed seed, RoadId trafficId, string decisionDomain, DecisionCounter counter,
            RoutePlan existing = null, bool replan = false, IReadOnlyCollection<RoadId> closedPortalIds = null,
            RoadId viaMovementId = default(RoadId))
        {
            ViaMovementId = viaMovementId;
            Model = model;
            Location = location;
            DestinationExitId = destinationExitId;
            Seed = seed;
            TrafficId = trafficId;
            DecisionDomain = decisionDomain;
            Counter = counter;
            Existing = existing;
            Replan = replan;
            ClosedPortalIds = closedPortalIds;
        }
    }

    /// <summary>Une visite dirigee ; une meme identite peut apparaitre plusieurs fois apres une boucle.</summary>
    public readonly struct RouteOccurrence
    {
        public readonly RoadElementKind Kind;
        public readonly RoadId Id;
        public readonly float StartSMeters;
        public readonly float EndSMeters;

        public RouteOccurrence(RoadElementKind kind, RoadId id, float startSMeters, float endSMeters)
        {
            Kind = kind;
            Id = id;
            StartSMeters = startSMeters;
            EndSMeters = endSMeters;
        }
    }

    /// <summary>
    /// Plan strategique immuable : occurrences dirigees ordonnees, progression acquise et couts
    /// publies. Une meme identite peut apparaitre plusieurs fois apres une boucle legale. Aucun
    /// effet de conduite : ni chemin, ni consigne, ni mutation du modele ou du monde.
    /// </summary>
    public sealed class RoutePlan
    {
        public RoadId ModelId { get; }
        public RoadModelVersion ModelVersion { get; }
        public RoadId TrafficId { get; }
        public RoadId ExitPortalId { get; }
        public RouteReason Reason { get; }
        public IReadOnlyList<RouteOccurrence> Occurrences { get; }
        public int ProgressOccurrenceIndex { get; }
        public float ProgressSMeters { get; }
        public double DistanceMeters { get; }
        public double PreferenceCost { get; }
        public double TotalCost { get { return DistanceMeters + PreferenceCost; } }
        public RouteDiagnostic Diagnostics { get; }
        /// <summary>Mouvement vise par l'objectif intermediaire ; vide sans objectif.</summary>
        public RoadId ViaMovementId { get; }
        /// <summary>Occurrence du mouvement vise (frontiere des deux phases) ; -1 sans objectif.</summary>
        public int ViaOccurrenceIndex { get; }

        internal RoutePlan(RoadId modelId, RoadModelVersion version, RoadId trafficId, RoadId exitPortalId, RouteReason reason,
            List<RouteOccurrence> occurrences, double distanceMeters, double preferenceCost, RouteDiagnostic diagnostics,
            RoadId viaMovementId = default(RoadId), int viaOccurrenceIndex = -1)
        {
            ViaMovementId = viaMovementId;
            ViaOccurrenceIndex = viaOccurrenceIndex;
            if (occurrences == null || occurrences.Count == 0)
                throw new ArgumentException("Au moins une occurrence est requise.", "occurrences");
            ModelId = modelId;
            ModelVersion = version;
            TrafficId = trafficId;
            ExitPortalId = exitPortalId;
            Reason = reason;
            Occurrences = occurrences.AsReadOnly();
            ProgressOccurrenceIndex = 0;
            ProgressSMeters = occurrences[0].StartSMeters;
            DistanceMeters = distanceMeters;
            PreferenceCost = preferenceCost;
            Diagnostics = diagnostics;
        }

        private RoutePlan(RoutePlan source, int occurrenceIndex, float sMeters)
        {
            ModelId = source.ModelId;
            ModelVersion = source.ModelVersion;
            TrafficId = source.TrafficId;
            ExitPortalId = source.ExitPortalId;
            Reason = source.Reason;
            Occurrences = source.Occurrences;
            ProgressOccurrenceIndex = occurrenceIndex;
            ProgressSMeters = sMeters;
            DistanceMeters = source.DistanceMeters;
            PreferenceCost = source.PreferenceCost;
            Diagnostics = source.Diagnostics;
            ViaMovementId = source.ViaMovementId;
            ViaOccurrenceIndex = source.ViaOccurrenceIndex;
        }

        /// <summary>
        /// Progression sur la meme occurrence : l'egalite exacte du couple (index, s) est le contrat,
        /// les valeurs etant des donnees re-emises telles quelles ; une valeur quantifiee en amont
        /// doit produire un nouveau plan, jamais une reutilisation approximative.
        /// </summary>
        internal RoutePlan Advance(int occurrenceIndex, float sMeters)
        {
            return occurrenceIndex == ProgressOccurrenceIndex && sMeters == ProgressSMeters
                ? this : new RoutePlan(this, occurrenceIndex, sMeters);
        }
    }

    /// <summary>Resultat nomme d'une demande ; `Plan` est nul pour tout resultat non planifie.</summary>
    public readonly struct RouteResult
    {
        public readonly RouteOutcome Outcome;
        public readonly RouteReason Reason;
        public readonly RouteDiagnostic Diagnostics;
        public readonly RoutePlan Plan;

        internal RouteResult(RouteOutcome outcome, RouteReason reason, RoutePlan plan)
        {
            Outcome = outcome;
            Reason = reason;
            Plan = plan;
            Diagnostics = plan == null ? RouteDiagnostic.None : plan.Diagnostics;
        }
    }
}
