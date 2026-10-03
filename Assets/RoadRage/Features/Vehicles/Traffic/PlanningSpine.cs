using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using RoadRage.Features.Vehicles;
using Unity.Profiling;

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
        /// <summary>
        /// Portee bornee de la planification (Story 5.33, D14) : nulle, horizon sur <see cref="LookAheadMeters"/>. Fournie,
        /// l'horizon de planification s'arrete a H et la perception lit le chemin complet (<see cref="PlanningDecision.PerceptionPath"/>).
        /// </summary>
        public readonly PlanningReach? Reach;

        public PlanningRequest(TrafficFrame frame, RoadId trafficId, RoutePlan existingRoute,
            RoadId destinationExitId, RouteSeed sessionSeed, float lookAheadMeters,
            string modelText, string signoffText, string reportText, DriverProfile driver,
            TrackingTolerance tracking = default(TrackingTolerance),
            LongitudinalBounds? bounds = null,
            IReadOnlyList<SpeedProfilePoint> candidateSpeedProfile = null,
            GateAEvidenceResult? evidence = null, RoadId viaMovementId = default(RoadId),
            float? nominalOffsetRadians = null, PlanningReach? reach = null)
        {
            ViaMovementId = viaMovementId;
            Reach = reach;
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
        /// <summary>
        /// Chemin lu par la perception (Story 5.33, D14) : la route restante entiere sur <see cref="PlanningRequest.LookAheadMeters"/>,
        /// meme quand la planification est bornee. Sans portee bornee, c'est l'horizon lui-meme. Nul sans route.
        /// </summary>
        public IPathGeometry PerceptionPath { get; }

        internal PlanningDecision(AgentObservation observation, RouteResult route, PathHorizon path,
            MotionPlan motion, SpeedProfileResult? speedProfile, TrafficDecisionProjection projection, IPathGeometry perceptionPath)
        { Observation = observation; Route = route; Path = path; Motion = motion;
            SpeedProfile = speedProfile; Projection = projection; PerceptionPath = perceptionPath; }
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
            RouteMarker.Begin();
            var route = RoutePlanner.Plan(new RouteRequest(frame.Model, observation.Location,
                request.DestinationExitId, request.SessionSeed, request.TrafficId, "route",
                new DecisionCounter(frame.FrameId), request.ExistingRoute, false, null, request.ViaMovementId));
            RouteMarker.End();
            var evidence = request.Evidence ?? GateAEvidenceBinding.Bind(frame.Model, request.ModelText,
                request.SignoffText, request.ReportText);
            HorizonMarker.Begin();
            PathHorizon path = route.Plan == null ? null
                : PathHorizon.Build(frame.Model, route.Plan, request.LookAheadMeters, request.NominalOffsetRadians,
                    evidence.Valid && evidence.PoseModel == NominalPoseModel.Kinematic ? evidence.SignedRingSeams : null, request.Reach);
            // Perception sur toute la route restante : un leader ou un obstacle lointain reste percu (D14).
            IPathGeometry perceptionPath = route.Plan == null ? null
                : request.Reach.HasValue ? RoutePath.Build(frame.Model, route.Plan, request.LookAheadMeters) : (IPathGeometry)path;
            HorizonMarker.End();
            MotionPlan motion = null;
            SpeedProfileResult? checkedProfile = null;
            MotionMarker.Begin();
            if (path != null)
            {
                motion = new MotionPlan(path, frame.Model.DrivabilityProfile, evidence, request.Tracking);
                if (request.CandidateSpeedProfile != null)
                    checkedProfile = motion.VerifySpeedProfile(request.CandidateSpeedProfile, request.Bounds);
            }
            MotionMarker.End();
            string code = route.Plan == null ? route.Reason.ToString()
                : path.Issue != PathIssue.None ? path.Issue.ToString()
                : motion.Issue != MotionIssue.None ? motion.Issue.ToString()
                : checkedProfile.HasValue && checkedProfile.Value.Issue != SpeedProfileIssue.None
                    ? checkedProfile.Value.Issue.ToString() : "None";
            ProjectionMarker.Begin();
            var projection = new TrafficDecisionProjection(frame.FrameId, frame.Version, request.TrafficId,
                observation.Location, route, motion, code);
            ProjectionMarker.End();
            return new PlanningDecision(observation, route, path, motion, checkedProfile, projection, perceptionPath);
        }

        // Marqueurs de profilage (diagnostic de performance 5.33) : aucune influence sur la decision.
        private static readonly ProfilerMarker RouteMarker = new ProfilerMarker("TrafficV2.Spine.Route");
        private static readonly ProfilerMarker HorizonMarker = new ProfilerMarker("TrafficV2.Spine.Horizon");
        private static readonly ProfilerMarker MotionMarker = new ProfilerMarker("TrafficV2.Spine.Motion");
        private static readonly ProfilerMarker ProjectionMarker = new ProfilerMarker("TrafficV2.Spine.Projection");
    }
}
