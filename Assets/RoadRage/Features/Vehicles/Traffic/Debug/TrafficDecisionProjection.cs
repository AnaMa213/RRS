using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Blockers;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;

namespace RoadRage.Features.Vehicles.Traffic.Diagnostics
{
    /// <summary>Compteurs du collecteur de dangers sur un pas hote (5.33) : requetes, saturations, exclusions.</summary>
    public readonly struct TrafficHazardCollectorCounters
    {
        public readonly int Queries;
        public readonly int SaturatedQueries;
        public readonly int Hits;
        public readonly int Emitted;
        public readonly int ExcludedStatic;
        public readonly int ExcludedSelf;
        public readonly int ExcludedTrafficV2;
        public readonly int IdentityCollisions;
        public readonly int NonFinite;

        public TrafficHazardCollectorCounters(int queries, int saturatedQueries, int hits, int emitted, int excludedStatic,
            int excludedSelf, int excludedTrafficV2, int identityCollisions, int nonFinite)
        {
            Queries = queries; SaturatedQueries = saturatedQueries; Hits = hits; Emitted = emitted;
            ExcludedStatic = excludedStatic; ExcludedSelf = excludedSelf; ExcludedTrafficV2 = excludedTrafficV2;
            IdentityCollisions = identityCollisions; NonFinite = nonFinite;
        }

        public string ToText()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "requetes {0} saturees {1} / colliders {2} / dangers {3} / ecartes statiques {4} propre {5} V2 {6} / "
                + "HazardIdentityCollision {7} / non finis {8}", Queries, SaturatedQueries, Hits, Emitted, ExcludedStatic,
                ExcludedSelf, ExcludedTrafficV2, IdentityCollisions, NonFinite);
        }
    }

    /// <summary>
    /// Partie perception, arbitrage et blockers d'une decision (5.33) : faits retenus et statuts, candidats, liante,
    /// acceleration visee, limites de route, blockers et dominant, compteurs du collecteur. Hote seul.
    /// </summary>
    public sealed class TrafficLongitudinalOutcome
    {
        private static readonly RoadLimitValue[] NoRoadLimits = new RoadLimitValue[0];

        public ulong FrameId { get; }
        /// <summary>Perceived faux : perception non evaluee a ce pas.</summary>
        public AgentObservation Observation { get; }
        /// <summary>Nulle : aucun arbitrage a ce pas (pas de plan accepte).</summary>
        public LongitudinalDecision Decision { get; }
        public IReadOnlyList<RoadLimitValue> RoadLimits { get; }
        public IReadOnlyList<Blocker> Blockers { get; }
        public bool HasDominant { get; }
        public Blocker Dominant { get; }
        /// <summary>Colliders rendus par la requete du collecteur emise pour ce vehicule.</summary>
        public int HazardQueryHits { get; }
        public bool HazardQuerySaturated { get; }
        public TrafficHazardCollectorCounters Collector { get; }

        public TrafficLongitudinalOutcome(ulong frameId, AgentObservation observation, LongitudinalDecision decision,
            IReadOnlyList<RoadLimitValue> roadLimits, IReadOnlyList<Blocker> blockers, int hazardQueryHits,
            bool hazardQuerySaturated, TrafficHazardCollectorCounters collector)
        {
            FrameId = frameId; Observation = observation; Decision = decision;
            var limits = roadLimits == null ? NoRoadLimits : new RoadLimitValue[roadLimits.Count];
            for (int i = 0; i < limits.Length; i++) limits[i] = roadLimits[i];
            RoadLimits = Array.AsReadOnly(limits);
            Blockers = blockers ?? BlockerTracker.Empty;
            Blocker dominant;
            HasDominant = BlockerTracker.TryGetDominant(Blockers, out dominant);
            Dominant = dominant;
            HazardQueryHits = hazardQueryHits; HazardQuerySaturated = hazardQuerySaturated; Collector = collector;
        }

        /// <summary>Texte deterministe et invariant de culture.</summary>
        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Observation ");
            if (Observation.Perceived) text.Append('\n').Append(Observation.ToText());
            else text.Append("non evaluee\n");
            text.Append(Decision == null ? "Longitudinal aucun arbitrage\n" : Decision.ToText());
            text.Append("Road limits");
            if (RoadLimits.Count == 0) text.Append(" aucune");
            for (int i = 0; i < RoadLimits.Count; i++)
            {
                var limit = RoadLimits[i];
                text.Append(i == 0 ? " " : ", ").Append(limit.ElementId).Append(' ').Append(limit.State);
                if (limit.State == RoadLimitState.Applied)
                    text.Append(' ').Append(limit.MetersPerSecond.ToString("0.####", CultureInfo.InvariantCulture));
            }
            text.Append('\n').Append("Blockers ").Append(Blockers.Count.ToString(CultureInfo.InvariantCulture));
            for (int i = 0; i < Blockers.Count; i++) text.Append("\n  ").Append(Blockers[i].ToText());
            text.Append('\n').Append("Dominant ").Append(HasDominant ? Dominant.Id : "aucun").Append('\n');
            text.Append("Hazard query ").Append(HazardQueryHits.ToString(CultureInfo.InvariantCulture))
                .Append(HazardQuerySaturated ? " saturee" : "").Append(" / collector ").Append(Collector.ToText());
            return text.ToString();
        }
    }

    /// <summary>
    /// Partie coordination de carrefour d'une decision (5.34) : demande de la frame (traversee, d, D_stop, D_engage, D_request,
    /// tete de file, engagement), records de l'instantane lu pour ce vehicule (statut, raison, titulaire, occupant ou zone,
    /// anciennete) et compteurs du lot, dont EnteredWithoutGrant et IncompatibleOccupancy. Hote seul.
    /// </summary>
    public sealed class TrafficJunctionOutcome
    {
        private static readonly JunctionRecord[] NoRecords = new JunctionRecord[0];

        public ulong FrameId { get; }
        /// <summary>Rapport de la frame ; nul sans frame ni localisation exploitable.</summary>
        public JunctionActorReport Report { get; }
        /// <summary>Records de ce vehicule dans l'instantane lu a cette frame.</summary>
        public IReadOnlyList<JunctionRecord> Records { get; }
        public JunctionBatchCounters Counters { get; }
        /// <summary>EffectiveFrame de l'instantane lu ; un instantane decale vaut « aucun grant ».</summary>
        public ulong SnapshotEffectiveFrame { get; }
        public bool SnapshotStale { get; }
        /// <summary>La contrainte JunctionEntry etait active a ce pas.</summary>
        public bool EntryActive { get; }

        public TrafficJunctionOutcome(ulong frameId, JunctionActorReport report, JunctionSnapshot snapshot, bool entryActive)
        {
            FrameId = frameId; Report = report; EntryActive = entryActive;
            var mine = new List<JunctionRecord>();
            if (snapshot != null && report != null)
                foreach (var record in snapshot.Records)
                    if (record.TrafficId == report.TrafficId) mine.Add(record);
            Records = mine.Count == 0 ? Array.AsReadOnly(NoRecords) : mine.AsReadOnly();
            Counters = snapshot != null ? snapshot.Counters : default(JunctionBatchCounters);
            SnapshotEffectiveFrame = snapshot != null ? snapshot.EffectiveFrame : 0UL;
            SnapshotStale = snapshot == null || snapshot.EffectiveFrame != frameId;
        }

        /// <summary>Texte deterministe et invariant de culture.</summary>
        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Junction snapshot effectif ").Append(SnapshotEffectiveFrame.ToString(CultureInfo.InvariantCulture))
                .Append(SnapshotStale ? " (decale : aucun grant)" : "").Append(" / entree ").Append(EntryActive ? "active" : "inactive")
                .Append('\n');
            text.Append(Report == null ? "Junction report aucun" : Report.ToText()).Append('\n');
            text.Append("Junction records ").Append(Records.Count.ToString(CultureInfo.InvariantCulture));
            for (int i = 0; i < Records.Count; i++) text.Append("\n  ").Append(Records[i].ToText());
            text.Append('\n').Append("Junction batch ").Append(Counters.ToText());
            return text.ToString();
        }
    }

    /// <summary>
    /// Partie conduite d'une decision (5.31) : contraintes appliquees et reportees, liante, epoques,
    /// intent final en quatre flottants, repli et raison, couverture vehicule et etiquette de mesure.
    /// Hote seul : elle ne cree aucun chemin de synchronisation client.
    /// </summary>
    public sealed class TrafficDriveOutcome
    {
        public ulong DecisionEpoch { get; }
        public ulong PhysicsEpoch { get; }
        /// <summary>Frame source de la commande appliquee ; 0 pendant un repli.</summary>
        public ulong SourceFrameId { get; }
        public float Throttle { get; }
        public float Steer { get; }
        public float BrakeReverse { get; }
        public float Handbrake { get; }
        public bool Fallback { get; }
        public string FallbackReason { get; }
        public string Binding { get; }
        public IReadOnlyList<string> AppliedConstraints { get; }
        public IReadOnlyList<string> DeferredConstraints { get; }
        public VehicleCoverage VehicleCoverage { get; }
        /// <summary>Null hors run de mesure.</summary>
        public string MeasurementLabel { get; }
        /// <summary>Verdict du SafetyFilter (5.37) ; null sans commande evaluee.</summary>
        public string Safety { get; }

        public TrafficDriveOutcome(ulong decisionEpoch, ulong physicsEpoch, ulong sourceFrameId, float throttle,
            float steer, float brakeReverse, float handbrake, bool fallback, string fallbackReason, string binding,
            IReadOnlyList<string> appliedConstraints, IReadOnlyList<string> deferredConstraints,
            VehicleCoverage vehicleCoverage, string measurementLabel, string safety = null)
        {
            DecisionEpoch = decisionEpoch; PhysicsEpoch = physicsEpoch; SourceFrameId = sourceFrameId;
            Throttle = throttle; Steer = steer; BrakeReverse = brakeReverse; Handbrake = handbrake;
            Fallback = fallback; FallbackReason = fallbackReason ?? "None"; Binding = binding ?? "None";
            AppliedConstraints = Array.AsReadOnly(Copy(appliedConstraints));
            DeferredConstraints = Array.AsReadOnly(Copy(deferredConstraints));
            VehicleCoverage = vehicleCoverage; MeasurementLabel = measurementLabel; Safety = safety;
        }

        private static string[] Copy(IReadOnlyList<string> source)
        {
            if (source == null) return new string[0];
            var result = new string[source.Count];
            for (int i = 0; i < result.Length; i++) result[i] = source[i];
            return result;
        }
    }

    public sealed class TrafficDecisionProjection
    {
        public ulong FrameId { get; }
        public RoadModelVersion RoadModelVersion { get; }
        public RoadId TrafficId { get; }
        public RoadElementKind ElementKind { get; }
        public RoadId ElementId { get; }
        public float SMeters { get; }
        public RouteOutcome RouteOutcome { get; }
        public RouteReason RouteReason { get; }
        public RoadId ExitPortalId { get; }
        public IReadOnlyList<RouteOccurrence> RouteOccurrences { get; }
        public RoadId NextMovementId { get; }
        public string PathPlanId { get; }
        public string MotionPlanId { get; }
        /// <summary>Null quand aucune liaison n'a ete evaluee (pas de plan de route).</summary>
        public GateAEvidenceStatus? EvidenceStatus { get; }
        /// <summary>Null quand aucune couverture n'a ete evaluee (pas de plan de route).</summary>
        public ReferenceCoverage? ReferenceCoverage { get; }
        public VehicleCoverage VehicleCoverage { get; }
        public string SteeringCeilingText { get; }
        public string Code { get; }
        /// <summary>Partie conduite (5.31) ; nulle pour une decision sans conduite (5.30).</summary>
        public TrafficDriveOutcome Drive { get; private set; }
        /// <summary>Partie perception, arbitrage et blockers (5.33) ; nulle sans perception ni arbitrage a ce pas.</summary>
        public TrafficLongitudinalOutcome Longitudinal { get; private set; }
        /// <summary>Partie coordination de carrefour (5.34) ; nulle sans rapport de coordination a ce pas.</summary>
        public TrafficJunctionOutcome Junction { get; private set; }

        internal TrafficDecisionProjection(ulong frameId, RoadModelVersion version, RoadId trafficId,
            RoadLocation location, RouteResult route, MotionPlan motion, string code)
        {
            FrameId = frameId; RoadModelVersion = version; TrafficId = trafficId;
            ElementKind = location.ElementKind; ElementId = location.ElementId; SMeters = location.SMeters;
            RouteOutcome = route.Outcome; RouteReason = route.Reason;
            ExitPortalId = route.Plan == null ? RoadId.None : route.Plan.ExitPortalId;
            var occurrences = route.Plan == null ? new RouteOccurrence[0] : Copy(route.Plan.Occurrences);
            RouteOccurrences = Array.AsReadOnly(occurrences);
            NextMovementId = RoadId.None;
            if (route.Plan != null)
                for (int i = route.Plan.ProgressOccurrenceIndex; i < occurrences.Length; i++)
                    if (occurrences[i].Kind == RoadElementKind.JunctionMovement)
                    { NextMovementId = occurrences[i].Id; break; }
            string key = frameId.ToString(CultureInfo.InvariantCulture) + ":" + trafficId;
            PathPlanId = motion == null ? "aucun" : key + ":path";
            MotionPlanId = motion == null ? "aucun" : key + ":motion";
            if (motion != null)
            {
                EvidenceStatus = motion.Evidence.Status;
                ReferenceCoverage = motion.ReferenceCoverage;
            }
            VehicleCoverage = VehicleCoverage.NotEstablished;
            float minimum = float.PositiveInfinity;
            if (motion != null)
                foreach (var interval in motion.Path.Intervals)
                    foreach (var point in interval.Points)
                        minimum = Math.Min(minimum, point.SteeringCeilingMetersPerSecond);
            SteeringCeilingText = motion == null ? NotEvaluated
                : float.IsPositiveInfinity(minimum) ? "aucun"
                : minimum.ToString("R", CultureInfo.InvariantCulture) + " m/s";
            Code = code;
        }

        private const string NotEvaluated = "non evalue";

        /// <summary>Copie immuable portant la partie conduite ; la decision d'origine est inchangee.</summary>
        public TrafficDecisionProjection WithDrive(TrafficDriveOutcome drive)
        {
            var copy = (TrafficDecisionProjection)MemberwiseClone();
            copy.Drive = drive;
            return copy;
        }

        /// <summary>Copie immuable portant la partie longitudinale (5.33) ; nulle l'efface.</summary>
        public TrafficDecisionProjection WithLongitudinal(TrafficLongitudinalOutcome longitudinal)
        {
            var copy = (TrafficDecisionProjection)MemberwiseClone();
            copy.Longitudinal = longitudinal;
            return copy;
        }

        /// <summary>Copie immuable portant la partie coordination de carrefour (5.34) ; nulle l'efface.</summary>
        public TrafficDecisionProjection WithJunction(TrafficJunctionOutcome junction)
        {
            var copy = (TrafficDecisionProjection)MemberwiseClone();
            copy.Junction = junction;
            return copy;
        }

        private static string F(float value) { return value.ToString("R", CultureInfo.InvariantCulture); }

        private static RouteOccurrence[] Copy(IReadOnlyList<RouteOccurrence> source)
        {
            var result = new RouteOccurrence[source.Count];
            for (int i = 0; i < source.Count; i++) result[i] = source[i];
            return result;
        }

        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Frame ").Append(FrameId.ToString(CultureInfo.InvariantCulture))
                .Append(" / RoadModelVersion ").Append(RoadModelVersion).Append('\n');
            text.Append("Vehicle ").Append(TrafficId).Append(" / Element ").Append(ElementKind)
                .Append(' ').Append(ElementId).Append(" / s ")
                .Append(SMeters.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
            text.Append("RoutePlan ").Append(RouteOutcome).Append(" / ").Append(RouteReason)
                .Append(" / exit ").Append(ExitPortalId).Append(" / IDs ");
            for (int i = 0; i < RouteOccurrences.Count; i++)
            { if (i > 0) text.Append(','); text.Append(RouteOccurrences[i].Id); }
            text.Append('\n').Append("Next movement ").Append(NextMovementId).Append('\n');
            text.Append("Path/Motion plan ").Append(PathPlanId).Append(" / ").Append(MotionPlanId).Append('\n');
            text.Append("Steering ceiling ").Append(SteeringCeilingText).Append('\n');
            text.Append("Gate A ").Append(EvidenceStatus.HasValue ? EvidenceStatus.Value.ToString() : NotEvaluated)
                .Append(" / reference ")
                .Append(ReferenceCoverage.HasValue ? ReferenceCoverage.Value.ToString() : NotEvaluated)
                .Append(" / vehicle ").Append(VehicleCoverage).Append('\n');
            text.Append("Code ").Append(Code);
            if (Drive != null)
            {
                text.Append('\n').Append("Speed constraints applied ").Append(string.Join(", ", Drive.AppliedConstraints))
                    .Append(" / binding ").Append(Drive.Binding).Append('\n');
                text.Append("Speed constraints deferred ").Append(string.Join(", ", Drive.DeferredConstraints)).Append('\n');
                text.Append("Epochs decision ").Append(Drive.DecisionEpoch.ToString(CultureInfo.InvariantCulture))
                    .Append(" / physics ").Append(Drive.PhysicsEpoch.ToString(CultureInfo.InvariantCulture))
                    .Append(" / source frame ").Append(Drive.SourceFrameId.ToString(CultureInfo.InvariantCulture)).Append('\n');
                text.Append("Final intent ").Append(F(Drive.Throttle)).Append(' ').Append(F(Drive.Steer)).Append(' ')
                    .Append(F(Drive.BrakeReverse)).Append(' ').Append(F(Drive.Handbrake))
                    .Append(" / fallback ").Append(Drive.Fallback ? "yes " + Drive.FallbackReason : "no").Append('\n');
                text.Append("Vehicle coverage ").Append(Drive.VehicleCoverage)
                    .Append(" / measurement ").Append(Drive.MeasurementLabel ?? "hors mesure");
                if (Drive.Safety != null) text.Append('\n').Append("Safety ").Append(Drive.Safety);
            }
            if (Longitudinal != null) text.Append('\n').Append(Longitudinal.ToText());
            if (Junction != null) text.Append('\n').Append(Junction.ToText());
            return text.ToString();
        }
    }
}
