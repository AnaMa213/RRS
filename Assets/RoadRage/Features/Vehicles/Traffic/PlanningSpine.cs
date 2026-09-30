using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Features.Vehicles;

namespace RoadRage.Features.Vehicles.Traffic
{
    public readonly struct PlanningRequest
    {
        public readonly TrafficFrame Frame;
        public readonly RoadId TrafficId;
        public readonly RoutePlan ExistingRoute;
        public readonly RoadId DestinationExitId;
        public readonly RouteSeed SessionSeed;
        public readonly float LookAheadMeters;
        public readonly string ModelText;
        public readonly string SignoffText;
        public readonly string ReportText;
        public readonly TrackingTolerance Tracking;
        public readonly LongitudinalBounds Bounds;
        public readonly IReadOnlyList<SpeedProfilePoint> CandidateSpeedProfile;
        /// <summary>Liaison deja evaluee et mise en cache par modele (5.31) ; nulle : liaison sur les trois textes.</summary>
        public readonly GateAEvidenceResult? Evidence;
        /// <summary>Objectif de mouvement intermediaire (contrat §4), pose seulement sous run de mesure ; vide sinon.</summary>
        public readonly RoadId ViaMovementId;
        /// <summary>
        /// Ecart nominal e du vehicule (contrat §8) sur sa reference, au debut de l'horizon : plafond de braquage de
        /// la pose nominale (5.31, 2026-09-30). Nul : plafond de regime etabli du compilateur.
        /// </summary>
        public readonly float? NominalOffsetRadians;

        public PlanningRequest(TrafficFrame frame, RoadId trafficId, RoutePlan existingRoute,
            RoadId destinationExitId, RouteSeed sessionSeed, float lookAheadMeters,
            string modelText, string signoffText, string reportText, DriverProfile driver,
            TrackingTolerance tracking = default(TrackingTolerance),
            LongitudinalBounds? bounds = null,
            IReadOnlyList<SpeedProfilePoint> candidateSpeedProfile = null,
            GateAEvidenceResult? evidence = null, RoadId viaMovementId = default(RoadId),
            float? nominalOffsetRadians = null)
        {
            ViaMovementId = viaMovementId;
            NominalOffsetRadians = nominalOffsetRadians;
            Frame = frame; TrafficId = trafficId; ExistingRoute = existingRoute;
            DestinationExitId = destinationExitId; SessionSeed = sessionSeed;
            LookAheadMeters = lookAheadMeters; ModelText = modelText;
            SignoffText = signoffText; ReportText = reportText; Tracking = tracking;
            Bounds = bounds ?? new LongitudinalBounds(driver.MaxAcceleration, driver.SafeBrakingLimit);
            CandidateSpeedProfile = candidateSpeedProfile;
            Evidence = evidence;
        }
    }

    public sealed class PlanningDecision
    {
        public AgentObservation Observation { get; }
        public RouteResult Route { get; }
        public PathHorizon Path { get; }
        public MotionPlan Motion { get; }
        public SpeedProfileResult? SpeedProfile { get; }
        public TrafficDecisionProjection Projection { get; }

        internal PlanningDecision(AgentObservation observation, RouteResult route, PathHorizon path,
            MotionPlan motion, SpeedProfileResult? speedProfile, TrafficDecisionProjection projection)
        { Observation = observation; Route = route; Path = path; Motion = motion;
            SpeedProfile = speedProfile; Projection = projection; }
    }

    /// <summary>Stateless host decision over exactly one immutable frame.</summary>
    public static class PlanningSpine
    {
        public static PlanningDecision Evaluate(PlanningRequest request)
        {
            if (request.Frame == null) throw new ArgumentNullException("frame");
            if (!(request.LookAheadMeters > 0f) || float.IsInfinity(request.LookAheadMeters))
                throw new ArgumentException("InvalidLookAhead", "request");
            if (request.TrafficId.IsEmpty) throw new ArgumentException("EmptyTrafficId", "request");
            var frame = request.Frame;
            if (!frame.Model.DrivabilityProfile.Declared)
                throw new InvalidOperationException("UndeclaredDrivabilityProfile");
            TrafficActor actor;
            if (!frame.TryGetActor(request.TrafficId, out actor))
                throw new ArgumentException("UnknownTrafficId", "request");

            var observation = new AgentObservation(frame.FrameId, actor);
            var route = RoutePlanner.Plan(new RouteRequest(frame.Model, observation.Location,
                request.DestinationExitId, request.SessionSeed, request.TrafficId, "route",
                new DecisionCounter(frame.FrameId), request.ExistingRoute, false, null, request.ViaMovementId));
            PathHorizon path = route.Plan == null ? null
                : PathHorizon.Build(frame.Model, route.Plan, request.LookAheadMeters, request.NominalOffsetRadians);
            MotionPlan motion = null;
            SpeedProfileResult? checkedProfile = null;
            if (path != null)
            {
                var evidence = request.Evidence ?? GateAEvidenceBinding.Bind(frame.Model, request.ModelText,
                    request.SignoffText, request.ReportText);
                motion = new MotionPlan(path, frame.Model.DrivabilityProfile, evidence, request.Tracking);
                if (request.CandidateSpeedProfile != null)
                    checkedProfile = motion.VerifySpeedProfile(request.CandidateSpeedProfile, request.Bounds);
            }
            string code = route.Plan == null ? route.Reason.ToString()
                : path.Issue != PathIssue.None ? path.Issue.ToString()
                : motion.Issue != MotionIssue.None ? motion.Issue.ToString()
                : checkedProfile.HasValue && checkedProfile.Value.Issue != SpeedProfileIssue.None
                    ? checkedProfile.Value.Issue.ToString() : "None";
            var projection = new TrafficDecisionProjection(frame.FrameId, frame.Version, request.TrafficId,
                observation.Location, route, motion, code);
            return new PlanningDecision(observation, route, path, motion, checkedProfile, projection);
        }
    }
}
