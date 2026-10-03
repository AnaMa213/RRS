using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Perception;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    /// <summary>Genre d'un candidat longitudinal (5.33). L'ordre des valeurs est l'ordre d'egalite de l'arbitrage.</summary>
    public enum LongitudinalCandidateKind
    {
        SteeringCeilingUnreachable = 0,
        PerceptionUnavailable = 1,
        /// <summary>Maintien a l'arret D11 : nomme par sa cause (LeaderFollowing ou Obstacle) et sa source.</summary>
        StopHold = 2,
        Obstacle = 3,
        LeaderFollowing = 4,
        /// <summary>Suivi du profil spatial 5.31, nomme par sa contrainte limitante.</summary>
        Profile = 5,
        /// <summary>IDM route libre.</summary>
        DesiredSpeed = 6
    }

    /// <summary>
    /// Parametres du maintien a l'arret D11 (5.33). Portes par les reglages runtime (TrafficV2Settings.StopHold) et
    /// calibres sur le banc EditMode ; l'arbitrage ne porte aucune valeur propre.
    /// </summary>
    public readonly struct StopHoldParameters
    {
        /// <summary>Maintien desactive : l'IDM seul (reference du banc de calibration).</summary>
        public static readonly StopHoldParameters Disabled = default(StopHoldParameters);

        public readonly bool Enabled;
        /// <summary>Vitesse sous laquelle le vehicule est quasi arrete : condition d'entree.</summary>
        public readonly float EntrySpeedMetersPerSecond;
        /// <summary>Delta_hold : entree quand le jeu de la cause liante vaut au plus s0 + Delta_hold.</summary>
        public readonly float HoldGapMarginMeters;
        /// <summary>Delta_release : liberation quand le jeu de la source atteint s0 + Delta_release.</summary>
        public readonly float ReleaseGapMarginMeters;
        /// <summary>Vitesse de la source au-dela de laquelle elle repart ; liberation si le jeu depasse aussi s0 + Delta_hold.</summary>
        public readonly float SourceDepartureSpeedMetersPerSecond;

        public StopHoldParameters(float entrySpeed, float holdGapMargin, float releaseGapMargin, float sourceDepartureSpeed)
        {
            if (!Positive(entrySpeed) || !(holdGapMargin >= 0f) || float.IsInfinity(holdGapMargin)
                || !Positive(releaseGapMargin) || !(releaseGapMargin > holdGapMargin) || !Positive(sourceDepartureSpeed))
                throw new ArgumentException("InvalidStopHoldParameters");
            Enabled = true;
            EntrySpeedMetersPerSecond = entrySpeed; HoldGapMarginMeters = holdGapMargin;
            ReleaseGapMarginMeters = releaseGapMargin; SourceDepartureSpeedMetersPerSecond = sourceDepartureSpeed;
        }

        private static bool Positive(float value) { return value > 0f && !float.IsInfinity(value); }
    }

    /// <summary>Phase du maintien a un pas.</summary>
    public enum StopHoldPhase { None = 0, Entered = 1, Holding = 2, Released = 3 }

    /// <summary>Raison d'une liberation du maintien.</summary>
    public enum StopHoldRelease
    {
        None = 0,
        /// <summary>La source n'est plus percue alors que la perception est disponible.</summary>
        SourceGone = 1,
        /// <summary>Le jeu de la source atteint s0 + Delta_release.</summary>
        GapOpened = 2,
        /// <summary>La source roule au moins a la vitesse de depart et le jeu depasse s0 + Delta_hold.</summary>
        SourceDeparted = 3
    }

    /// <summary>Etat publie du maintien a l'arret d'un pas : phase, cause, source et jeu.</summary>
    public readonly struct StopHoldState
    {
        public readonly StopHoldPhase Phase;
        /// <summary>LeaderFollowing ou Obstacle.</summary>
        public readonly LongitudinalCandidateKind Cause;
        public readonly RoadId SourceId;
        /// <summary>Jeu de la source ; NaN si la source n'est pas percue (perception indisponible pendant le maintien).</summary>
        public readonly float GapMeters;
        public readonly float SourceSpeedMetersPerSecond;
        public readonly StopHoldRelease Release;

        public StopHoldState(StopHoldPhase phase, LongitudinalCandidateKind cause, RoadId sourceId, float gapMeters,
            float sourceSpeed, StopHoldRelease release)
        {
            Phase = phase; Cause = cause; SourceId = sourceId; GapMeters = gapMeters; SourceSpeedMetersPerSecond = sourceSpeed;
            Release = release;
        }

        /// <summary>Maintenu a ce pas (entree ou maintien).</summary>
        public bool Active { get { return Phase == StopHoldPhase.Entered || Phase == StopHoldPhase.Holding; } }

        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("StopHold ").Append(Phase).Append(' ').Append(Cause).Append(" @").Append(SourceId)
                .Append(" jeu ").Append(LongitudinalArbitration.F(GapMeters)).Append(" v ").Append(LongitudinalArbitration.F(SourceSpeedMetersPerSecond));
            if (Release != StopHoldRelease.None) text.Append(" liberation ").Append(Release);
            return text.ToString();
        }
    }

    /// <summary>Raison publiee de PerceptionUnavailable (5.33).</summary>
    public enum PerceptionUnavailableReason
    {
        None = 0,
        /// <summary>Un canal a emprise (leader, suiveur, adjacence, obstacles) n'est pas Evaluated.</summary>
        ChannelUnavailable = 1,
        /// <summary>Un canal, ou la requete spatiale de la perception, rapporte Saturated.</summary>
        ChannelSaturated = 2,
        /// <summary>La requete du collecteur de dangers emise pour ce vehicule est saturee.</summary>
        HazardCollectorSaturated = 3
    }

    /// <summary>Leader percu : jeu d'arc pare-chocs a pare-chocs et vitesse.</summary>
    public readonly struct LongitudinalLeader
    {
        public readonly RoadId Id;
        public readonly float GapMeters;
        public readonly float SpeedMetersPerSecond;

        public LongitudinalLeader(RoadId id, float gapMeters, float speedMetersPerSecond)
        {
            Id = id; GapMeters = gapMeters; SpeedMetersPerSecond = speedMetersPerSecond;
        }
    }

    /// <summary>Obstacle du couloir balaye (ecart lateral &lt;= 0) : distance a sa face proche et vitesse le long de l'horizon.</summary>
    public readonly struct LongitudinalObstacle
    {
        public readonly RoadId Id;
        public readonly PerceivedObstacleKind Kind;
        public readonly float NearDistanceMeters;
        /// <summary>Vitesse du danger projetee sur la tangente d'horizon la plus proche de sa face proche, bornee a &gt;= 0.</summary>
        public readonly float SpeedAlongPathMetersPerSecond;

        public LongitudinalObstacle(RoadId id, PerceivedObstacleKind kind, float nearDistanceMeters, float speedAlongPath)
        {
            Id = id; Kind = kind; NearDistanceMeters = nearDistanceMeters; SpeedAlongPathMetersPerSecond = speedAlongPath;
        }
    }

    /// <summary>
    /// Faits de perception lus par l'arbitrage (5.33), extraits d'une <see cref="AgentObservation"/> par <see cref="From"/>.
    /// Une perception indisponible ou tronquee porte sa raison : elle n'est jamais lue comme complete.
    /// </summary>
    public sealed class LongitudinalPerception
    {
        private static readonly LongitudinalObstacle[] NoObstacles = new LongitudinalObstacle[0];

        public PerceptionUnavailableReason UnavailableReason { get; }
        public bool HasLeader { get; }
        public LongitudinalLeader Leader { get; }
        /// <summary>Obstacles du couloir balaye seulement ; ceux hors couloir restent publies par l'observation.</summary>
        public IReadOnlyList<LongitudinalObstacle> Obstacles { get; }

        public LongitudinalPerception(PerceptionUnavailableReason reason, LongitudinalLeader? leader,
            IReadOnlyList<LongitudinalObstacle> obstacles)
        {
            UnavailableReason = reason;
            HasLeader = leader.HasValue;
            Leader = leader ?? default(LongitudinalLeader);
            var copy = obstacles == null ? NoObstacles : new LongitudinalObstacle[obstacles.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = obstacles[i];
            Obstacles = Array.AsReadOnly(copy);
        }

        /// <summary>
        /// Raison d'indisponibilite (premiere qui s'applique : canal non evalue, puis saturation d'un canal ou de la
        /// requete spatiale, puis saturation du collecteur), leader retenu et obstacles du couloir balaye.
        /// </summary>
        /// <param name="frontDistanceMeters">Distance d'horizon du pare-chocs avant (<see cref="FrontDistanceMeters"/>).</param>
        public static LongitudinalPerception From(AgentObservation observation, IPathGeometry horizon, float frontDistanceMeters,
            bool hazardCollectorSaturated)
        {
            if (horizon == null) throw new ArgumentNullException("horizon");
            var reason = PerceptionUnavailableReason.None;
            if (!observation.Perceived || observation.Leader.Status != PerceptionStatus.Evaluated
                || observation.Follower.Status != PerceptionStatus.Evaluated
                || observation.Adjacent.Status != PerceptionStatus.Evaluated
                || observation.Obstacles.Status != PerceptionStatus.Evaluated)
                reason = PerceptionUnavailableReason.ChannelUnavailable;
            else if (observation.SpatialQuerySaturated || observation.Leader.Saturated || observation.Follower.Saturated
                || observation.Adjacent.Saturated || observation.Obstacles.Saturated || observation.IntentOverlaps.Saturated
                || observation.Exit.Saturated || observation.UnmeasuredActors.Saturated)
                reason = PerceptionUnavailableReason.ChannelSaturated;
            else if (hazardCollectorSaturated) reason = PerceptionUnavailableReason.HazardCollectorSaturated;
            if (!observation.Perceived) return new LongitudinalPerception(reason, null, null);

            LongitudinalLeader? leader = null;
            if (observation.Leader.Status == PerceptionStatus.Evaluated && observation.Leader.Items.Count > 0)
            {
                var fact = observation.Leader.Items[0];
                leader = new LongitudinalLeader(fact.TrafficId, fact.GapMeters, fact.SpeedMetersPerSecond);
            }
            var obstacles = new List<LongitudinalObstacle>();
            if (observation.Obstacles.Status == PerceptionStatus.Evaluated)
                for (int i = 0; i < observation.Obstacles.Items.Count; i++)
                {
                    var fact = observation.Obstacles.Items[i];
                    if (!fact.InSweptPath) continue;
                    float at = float.IsNaN(frontDistanceMeters) ? fact.NearDistanceMeters : frontDistanceMeters + fact.NearDistanceMeters;
                    float along = Vector3.Dot(fact.Velocity, TangentNearest(horizon, at));
                    obstacles.Add(new LongitudinalObstacle(fact.Id, fact.Kind, fact.NearDistanceMeters,
                        along > 0f && !float.IsNaN(along) && !float.IsInfinity(along) ? along : 0f));
                }
            return new LongitudinalPerception(reason, leader, obstacles);
        }

        /// <summary>Distance d'horizon du pare-chocs avant de l'agent (sMax conservateur) ; NaN sans occupation alignee.</summary>
        public static float FrontDistanceMeters(TrafficFrame frame, RoadId trafficId, IPathGeometry horizon)
        {
            if (frame == null || horizon == null || horizon.Spans.Count == 0) return float.NaN;
            ElementOccupant self;
            var first = horizon.Spans[0];
            if (!frame.TryGetOccupancy(trafficId, out self) || self.ElementId != first.Id) return float.NaN;
            return first.StartDistanceMeters + self.SMaxMeters - first.StartSMeters;
        }

        /// <summary>Tangente du point d'horizon le plus proche de la distance donnee (points tries par distance).</summary>
        private static Vector3 TangentNearest(IPathGeometry horizon, float distance)
        {
            var best = Vector3.zero;
            float bestDelta = float.PositiveInfinity;
            for (int i = 0; i < horizon.Spans.Count; i++)
            {
                int points = horizon.PointCount(i);
                for (int p = 0; p < points; p++)
                {
                    float delta = horizon.PointDistance(i, p) - distance;
                    if (delta > bestDelta) return best;
                    if (Math.Abs(delta) < bestDelta) { bestDelta = Math.Abs(delta); best = horizon.PointTangent(i, p); }
                }
            }
            return best;
        }
    }

    /// <summary>Un candidat nomme en acceleration (m/s2), retenu ou rejete ; les rejetes sont conserves avec leur valeur.</summary>
    public readonly struct LongitudinalCandidate
    {
        public readonly LongitudinalCandidateKind Kind;
        /// <summary>Nom publie : contrainte limitante du profil pour Profile, sinon la contrainte du genre.</summary>
        public readonly SpeedConstraint Constraint;
        public readonly float AccelerationMetersPerSecondSquared;
        /// <summary>Leader ou obstacle ; None sinon.</summary>
        public readonly RoadId SourceId;
        /// <summary>Jeu du leader ou distance a la face proche de l'obstacle ; NaN sinon.</summary>
        public readonly float GapMeters;
        /// <summary>Vitesse du leader ou vitesse projetee de l'obstacle ; NaN sinon.</summary>
        public readonly float SourceSpeedMetersPerSecond;
        /// <summary>Genre percu d'un obstacle (mobile ou fixe pour les blockers) ; sans objet pour un autre genre.</summary>
        public readonly PerceivedObstacleKind ObstacleKind;
        /// <summary>
        /// Profil et route libre sous SteeringCeilingUnreachable : la regle 5.31 les remplace par le freinage a
        /// SafeBrakingLimit. Publies, ils n'entrent pas dans le minimum.
        /// </summary>
        public readonly bool Superseded;

        public LongitudinalCandidate(LongitudinalCandidateKind kind, SpeedConstraint constraint, float acceleration,
            RoadId sourceId = default(RoadId), float gapMeters = float.NaN, float sourceSpeed = float.NaN,
            PerceivedObstacleKind obstacleKind = PerceivedObstacleKind.Obstacle, bool superseded = false)
        {
            Kind = kind; Constraint = constraint; AccelerationMetersPerSecondSquared = acceleration; SourceId = sourceId;
            GapMeters = gapMeters; SourceSpeedMetersPerSecond = sourceSpeed; ObstacleKind = obstacleKind; Superseded = superseded;
        }

        public string ToText()
        {
            var text = new StringBuilder();
            text.Append(Kind).Append(':').Append(Constraint).Append(' ').Append(LongitudinalArbitration.F(AccelerationMetersPerSecondSquared));
            if (!SourceId.IsEmpty) text.Append(" @").Append(SourceId);
            if (!float.IsNaN(GapMeters)) text.Append(" jeu ").Append(LongitudinalArbitration.F(GapMeters));
            if (!float.IsNaN(SourceSpeedMetersPerSecond)) text.Append(" v ").Append(LongitudinalArbitration.F(SourceSpeedMetersPerSecond));
            if (Kind == LongitudinalCandidateKind.Obstacle) text.Append(' ').Append(ObstacleKind);
            if (Superseded) text.Append(" (remplace)");
            return text.ToString();
        }
    }

    /// <summary>
    /// Seul etat de l'arbitrage (D5, D11), porte par le driver d'un pas au suivant : l'acceleration appliquee precedente,
    /// la liante du pas, la contrainte d'interaction dont la reprise est en cours de lissage et le maintien a l'arret.
    /// </summary>
    public readonly struct LongitudinalMemory
    {
        public readonly bool Valid;
        public readonly float AppliedAccelerationMetersPerSecondSquared;
        public readonly SpeedConstraint Binding;
        /// <summary>LeaderFollowing, Obstacle ou PerceptionUnavailable pendant une reprise lissee ; None sinon.</summary>
        public readonly SpeedConstraint SmoothingSource;
        /// <summary>Maintien a l'arret actif a la fin du pas (D11).</summary>
        public readonly bool Holding;
        public readonly LongitudinalCandidateKind HoldCause;
        public readonly RoadId HoldSourceId;

        public LongitudinalMemory(float appliedAcceleration, SpeedConstraint binding, SpeedConstraint smoothingSource)
            : this(appliedAcceleration, binding, smoothingSource, default(StopHoldState))
        {
        }

        public LongitudinalMemory(float appliedAcceleration, SpeedConstraint binding, SpeedConstraint smoothingSource,
            StopHoldState hold)
        {
            Valid = true; AppliedAccelerationMetersPerSecondSquared = appliedAcceleration; Binding = binding;
            SmoothingSource = smoothingSource;
            Holding = hold.Active; HoldCause = hold.Active ? hold.Cause : default(LongitudinalCandidateKind);
            HoldSourceId = hold.Active ? hold.SourceId : RoadId.None;
        }

        /// <summary>Aucun pas precedent : ni lissage ni reprise.</summary>
        public static LongitudinalMemory None { get { return default(LongitudinalMemory); } }
    }

    /// <summary>Resultat d'un arbitrage : candidats, liante, cible, acceleration appliquee et raison de perception.</summary>
    public sealed class LongitudinalDecision
    {
        /// <summary>Candidats du pas, dans l'ordre d'egalite ; le canal obstacles y est reduit a son plus contraignant.</summary>
        public IReadOnlyList<LongitudinalCandidate> Candidates { get; }
        /// <summary>Un candidat par obstacle du couloir balaye, tries par (acceleration, id) : causes des blockers.</summary>
        public IReadOnlyList<LongitudinalCandidate> ObstacleCandidates { get; }
        public LongitudinalCandidate Binding { get; }
        /// <summary>Minimum exact des candidats non remplaces.</summary>
        public float TargetAccelerationMetersPerSecondSquared { get; }
        /// <summary>Cible, ou reprise lissee plafonnee a la cible.</summary>
        public float AppliedAccelerationMetersPerSecondSquared { get; }
        public bool Smoothed { get; }
        /// <summary>Contrainte d'interaction dont la reprise est lissee ; None sans lissage.</summary>
        public SpeedConstraint SmoothingSource { get; }
        public PerceptionUnavailableReason PerceptionReason { get; }
        /// <summary>IDM route libre, evalue a chaque pas (candidat DesiredSpeed, remplace ou non).</summary>
        public float FreeRoadAccelerationMetersPerSecondSquared { get; }
        /// <summary>Maintien a l'arret D11 du pas : phase, cause, source, jeu et raison de liberation.</summary>
        public StopHoldState Hold { get; }
        /// <summary>
        /// Causes reelles d'immobilisation pendant un maintien : le candidat StopHold de la source, puis chaque autre fait
        /// d'interaction (leader, obstacle du couloir balaye) dont le jeu tiendrait seul le maintien (&lt;= s0 + Delta_hold),
        /// avec son propre candidat. Vide hors maintien : un candidat &lt;= 0 en roulant n'est pas une immobilisation.
        /// </summary>
        public IReadOnlyList<LongitudinalCandidate> HoldCauses { get; }
        /// <summary>Etat a donner a l'arbitrage du pas suivant.</summary>
        public LongitudinalMemory Memory { get; }

        internal LongitudinalDecision(List<LongitudinalCandidate> candidates, List<LongitudinalCandidate> obstacles,
            LongitudinalCandidate binding, float target, float applied, SpeedConstraint smoothingSource,
            PerceptionUnavailableReason reason, float freeRoad, StopHoldState hold, List<LongitudinalCandidate> holdCauses)
        {
            Candidates = candidates.AsReadOnly(); ObstacleCandidates = obstacles.AsReadOnly(); Binding = binding;
            TargetAccelerationMetersPerSecondSquared = target; AppliedAccelerationMetersPerSecondSquared = applied;
            SmoothingSource = smoothingSource; Smoothed = smoothingSource != SpeedConstraint.None;
            PerceptionReason = reason; FreeRoadAccelerationMetersPerSecondSquared = freeRoad;
            Hold = hold; HoldCauses = holdCauses.AsReadOnly();
            Memory = new LongitudinalMemory(applied, binding.Constraint, smoothingSource, hold);
        }

        /// <summary>Texte deterministe et invariant de culture.</summary>
        public string ToText()
        {
            var text = new StringBuilder();
            text.Append("Longitudinal binding ").Append(Binding.ToText()).Append(" / target ")
                .Append(LongitudinalArbitration.F(TargetAccelerationMetersPerSecondSquared)).Append(" / applied ")
                .Append(LongitudinalArbitration.F(AppliedAccelerationMetersPerSecondSquared));
            if (Smoothed) text.Append(" (reprise lissee depuis ").Append(SmoothingSource).Append(')');
            text.Append(" / perception ").Append(PerceptionReason).Append('\n');
            if (Hold.Phase != StopHoldPhase.None) text.Append(Hold.ToText()).Append('\n');
            text.Append("Longitudinal candidates");
            for (int i = 0; i < Candidates.Count; i++) text.Append(i == 0 ? " " : ", ").Append(Candidates[i].ToText());
            text.Append('\n');
            if (ObstacleCandidates.Count > 0)
            {
                text.Append("Obstacle candidates");
                for (int i = 0; i < ObstacleCandidates.Count; i++) text.Append(i == 0 ? " " : ", ").Append(ObstacleCandidates[i].ToText());
                text.Append('\n');
            }
            return text.ToString();
        }
    }

    /// <summary>
    /// Arbitrage longitudinal pur (5.33). Candidats nommes en acceleration : profil spatial 5.31 (sous le nom de sa
    /// contrainte limitante), IDM route libre, suivi de leader (IDM 5.9), obstacle du couloir balaye, perception
    /// indisponible (0) et plafond inatteignable (-SafeBrakingLimit, regle 5.31). Cible = minimum ; egalite a 1e-6 pres
    /// departagee dans l'ordre des genres. Aucun etat : l'acceleration appliquee precedente entre par
    /// <see cref="LongitudinalMemory"/>.
    ///
    /// Restriction immediate, reprise lissee (D5) : une cible qui baisse s'applique au pas meme ; seule une hausse qui
    /// suit une liante d'interaction (leader, obstacle, perception indisponible) passe par
    /// DriverModel.SmoothAcceleration, plafonnee a la cible, jusqu'a ce que la cible soit atteinte. Sans contrainte
    /// d'interaction, l'acceleration est celle de la 5.31 au bit pres : min(IDM route libre, suivi du profil), ou
    /// -SafeBrakingLimit si le plafond est inatteignable.
    ///
    /// Maintien a l'arret (D11) : l'IDM conduit l'approche ; quasi arrete (v &lt;= vitesse d'entree) avec une liante
    /// LeaderFollowing ou Obstacle dont le jeu vaut au plus s0 + Delta_hold, le vehicule passe en StopHold, candidat nomme
    /// par sa cause et sa source qui vise l'arret au lieu de l'approche asymptotique de s0. Liberation par hysteresis :
    /// source disparue d'une perception disponible, jeu &gt;= s0 + Delta_release, ou source repartie (vitesse de depart et
    /// jeu &gt; s0 + Delta_hold). Le maintien est porte par <see cref="LongitudinalMemory"/>, ses parametres par l'appelant.
    ///
    /// L'arbitrage n'ecrit aucune commande : il rend une acceleration visee que la commande de suivi porte au composeur.
    /// </summary>
    public static class LongitudinalArbitration
    {
        /// <summary>Tolerance d'egalite entre candidats (m/s2).</summary>
        public const float TieToleranceMetersPerSecondSquared = 1e-6f;

        /// <param name="plan">Plan accepte du pas.</param>
        /// <param name="previous">Etat du pas precedent ; <see cref="LongitudinalMemory.None"/> au premier pas ou apres un repli.</param>
        /// <param name="stopHold">Maintien a l'arret D11 ; <see cref="StopHoldParameters.Disabled"/> pour l'IDM seul.</param>
        public static LongitudinalDecision Decide(SpeedPlan plan, DriverProfile driver, float speedMetersPerSecond,
            float deltaTimeSeconds, LongitudinalPerception perception, LongitudinalMemory previous, StopHoldParameters stopHold)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            if (perception == null) throw new ArgumentNullException("perception");
            if (!(deltaTimeSeconds > 0f) || float.IsInfinity(deltaTimeSeconds))
                throw new ArgumentException("InvalidDeltaTime", "deltaTimeSeconds");
            float speed = float.IsNaN(speedMetersPerSecond) || float.IsInfinity(speedMetersPerSecond) ? 0f
                : Math.Max(0f, speedMetersPerSecond);

            // Memes expressions que MotionCommand.Track (5.31) : route libre identique au bit pres.
            float freeRoad = DriverModel.ComputeAcceleration(driver, speed, 0f, DriverModel.NoLeaderGap);
            float preview = Math.Max(speed * deltaTimeSeconds, MotionCommand.PreviewFloorMeters);
            float tracking = (plan.SpeedAt(preview) - speed) / deltaTimeSeconds;
            bool unreachable = plan.Binding == SpeedConstraint.SteeringCeilingUnreachable;

            var obstacles = new List<LongitudinalCandidate>(perception.Obstacles.Count);
            for (int i = 0; i < perception.Obstacles.Count; i++)
            {
                var o = perception.Obstacles[i];
                obstacles.Add(new LongitudinalCandidate(LongitudinalCandidateKind.Obstacle, SpeedConstraint.Obstacle,
                    DriverModel.ComputeAcceleration(driver, speed, o.SpeedAlongPathMetersPerSecond, o.NearDistanceMeters),
                    o.Id, o.NearDistanceMeters, o.SpeedAlongPathMetersPerSecond, o.Kind));
            }
            obstacles.Sort((x, y) =>
            {
                int order = x.AccelerationMetersPerSecondSquared.CompareTo(y.AccelerationMetersPerSecondSquared);
                return order != 0 ? order : x.SourceId.CompareTo(y.SourceId);
            });

            // Candidats dans l'ordre d'egalite.
            var candidates = new List<LongitudinalCandidate>(6);
            if (unreachable)
                candidates.Add(new LongitudinalCandidate(LongitudinalCandidateKind.SteeringCeilingUnreachable,
                    SpeedConstraint.SteeringCeilingUnreachable, -driver.SafeBrakingLimit));
            if (perception.UnavailableReason != PerceptionUnavailableReason.None)
                candidates.Add(new LongitudinalCandidate(LongitudinalCandidateKind.PerceptionUnavailable,
                    SpeedConstraint.PerceptionUnavailable, 0f));
            if (obstacles.Count > 0) candidates.Add(obstacles[0]);
            if (perception.HasLeader)
            {
                var leader = perception.Leader;
                candidates.Add(new LongitudinalCandidate(LongitudinalCandidateKind.LeaderFollowing, SpeedConstraint.LeaderFollowing,
                    DriverModel.ComputeAcceleration(driver, speed, leader.SpeedMetersPerSecond, leader.GapMeters),
                    leader.Id, leader.GapMeters, leader.SpeedMetersPerSecond));
            }
            candidates.Add(new LongitudinalCandidate(LongitudinalCandidateKind.Profile, plan.LimitingConstraint, tracking,
                superseded: unreachable));
            candidates.Add(new LongitudinalCandidate(LongitudinalCandidateKind.DesiredSpeed, SpeedConstraint.DesiredSpeed,
                freeRoad, superseded: unreachable));

            float target;
            var binding = Bind(candidates, out target);

            // Maintien a l'arret (D11) : la liberation d'abord, avec son hysteresis, puis l'entree sur la liante
            // d'interaction quand le vehicule est quasi arrete et le jeu dans la fenetre d'arret.
            float s0 = driver.MinimumGap;
            var hold = default(StopHoldState);
            var source = default(LongitudinalCandidate);
            bool sourceSeen = false;
            if (stopHold.Enabled && previous.Valid && previous.Holding)
            {
                sourceSeen = TryFindSource(previous.HoldCause, previous.HoldSourceId, candidates, obstacles, out source);
                if (!sourceSeen)
                    // Perception indisponible : l'absence de la source n'est pas un fait, le maintien tient.
                    hold = perception.UnavailableReason != PerceptionUnavailableReason.None
                        ? new StopHoldState(StopHoldPhase.Holding, previous.HoldCause, previous.HoldSourceId, float.NaN, float.NaN,
                            StopHoldRelease.None)
                        : Released(previous, float.NaN, float.NaN, StopHoldRelease.SourceGone);
                else if (perception.UnavailableReason == PerceptionUnavailableReason.None
                    && source.GapMeters >= s0 + stopHold.ReleaseGapMarginMeters)
                    hold = Released(previous, source.GapMeters, source.SourceSpeedMetersPerSecond, StopHoldRelease.GapOpened);
                else if (perception.UnavailableReason == PerceptionUnavailableReason.None
                    && source.SourceSpeedMetersPerSecond >= stopHold.SourceDepartureSpeedMetersPerSecond
                    && source.GapMeters > s0 + stopHold.HoldGapMarginMeters)
                    hold = Released(previous, source.GapMeters, source.SourceSpeedMetersPerSecond, StopHoldRelease.SourceDeparted);
                else
                    hold = new StopHoldState(StopHoldPhase.Holding, previous.HoldCause, previous.HoldSourceId, source.GapMeters,
                        source.SourceSpeedMetersPerSecond, StopHoldRelease.None);
            }
            if (stopHold.Enabled && !hold.Active && speed <= stopHold.EntrySpeedMetersPerSecond
                && (binding.Kind == LongitudinalCandidateKind.Obstacle || binding.Kind == LongitudinalCandidateKind.LeaderFollowing)
                && binding.GapMeters <= s0 + stopHold.HoldGapMarginMeters)
            {
                source = binding;
                sourceSeen = true;
                hold = new StopHoldState(StopHoldPhase.Entered, binding.Kind, binding.SourceId, binding.GapMeters,
                    binding.SourceSpeedMetersPerSecond, StopHoldRelease.None);
            }

            var causes = new List<LongitudinalCandidate>();
            if (hold.Active)
            {
                // Arret explicite a 0 m/s : deceleration confortable, ou plus si la source l'exige ; toujours < 0, donc
                // un maintien au frein a main a l'arret et jamais une roue libre.
                float holding = -driver.ComfortableDeceleration;
                if (!(holding < 0f) || float.IsInfinity(holding)) holding = -driver.SafeBrakingLimit;
                var held = new LongitudinalCandidate(LongitudinalCandidateKind.StopHold,
                    hold.Cause == LongitudinalCandidateKind.Obstacle ? SpeedConstraint.Obstacle : SpeedConstraint.LeaderFollowing,
                    sourceSeen ? Math.Min(source.AccelerationMetersPerSecondSquared, holding) : holding, hold.SourceId, hold.GapMeters,
                    hold.SourceSpeedMetersPerSecond, sourceSeen ? source.ObstacleKind : PerceivedObstacleKind.Obstacle);
                int at = 0;
                while (at < candidates.Count && candidates[at].Kind < LongitudinalCandidateKind.StopHold) at++;
                candidates.Insert(at, held);
                binding = Bind(candidates, out target);
                causes.Add(held);
                foreach (var other in candidates.Where(c => c.Kind == LongitudinalCandidateKind.LeaderFollowing).Concat(obstacles))
                    if (!(other.Kind == hold.Cause && other.SourceId == hold.SourceId) && other.GapMeters <= s0 + stopHold.HoldGapMarginMeters)
                        causes.Add(other);
            }

            // Reprise lissee (D5) : seulement une hausse apres une liante d'interaction, plafonnee a la cible. Apres un
            // maintien, le vehicule est a l'arret : la reprise part de 0, non de la deceleration du maintien.
            var smoothingSource = SpeedConstraint.None;
            if (previous.Valid)
                smoothingSource = previous.SmoothingSource != SpeedConstraint.None ? previous.SmoothingSource
                    : IsInteraction(previous.Binding) ? previous.Binding : SpeedConstraint.None;
            float from = previous.AppliedAccelerationMetersPerSecondSquared;
            if (previous.Valid && previous.Holding && !hold.Active) from = Math.Max(from, 0f);
            float applied = target;
            var smoothing = SpeedConstraint.None;
            if (smoothingSource != SpeedConstraint.None && target > from)
            {
                float smoothed = DriverModel.SmoothAcceleration(from, target, driver.ReactionTime, deltaTimeSeconds);
                if (smoothed < target) { applied = smoothed; smoothing = smoothingSource; }
            }
            return new LongitudinalDecision(candidates, obstacles, binding, target, applied, smoothing,
                perception.UnavailableReason, freeRoad, hold, causes);
        }

        /// <summary>Minimum exact des candidats non remplaces ; egalite (1e-6) departagee dans l'ordre de la liste.</summary>
        private static LongitudinalCandidate Bind(List<LongitudinalCandidate> candidates, out float target)
        {
            target = float.PositiveInfinity;
            for (int i = 0; i < candidates.Count; i++)
                if (!candidates[i].Superseded)
                    target = Math.Min(target, candidates[i].AccelerationMetersPerSecondSquared);
            for (int i = 0; i < candidates.Count; i++)
                if (!candidates[i].Superseded
                    && candidates[i].AccelerationMetersPerSecondSquared <= target + TieToleranceMetersPerSecondSquared)
                    return candidates[i];
            return candidates[0];
        }

        private static bool TryFindSource(LongitudinalCandidateKind cause, RoadId id, List<LongitudinalCandidate> candidates,
            List<LongitudinalCandidate> obstacles, out LongitudinalCandidate source)
        {
            var pool = cause == LongitudinalCandidateKind.Obstacle ? obstacles : candidates;
            for (int i = 0; i < pool.Count; i++)
                if (pool[i].Kind == cause && pool[i].SourceId == id) { source = pool[i]; return true; }
            source = default(LongitudinalCandidate);
            return false;
        }

        private static StopHoldState Released(LongitudinalMemory previous, float gap, float sourceSpeed, StopHoldRelease reason)
        {
            return new StopHoldState(StopHoldPhase.Released, previous.HoldCause, previous.HoldSourceId, gap, sourceSpeed, reason);
        }

        /// <summary>Contraintes d'interaction dont la reprise est lissee (D5).</summary>
        public static bool IsInteraction(SpeedConstraint constraint)
        {
            return constraint == SpeedConstraint.LeaderFollowing || constraint == SpeedConstraint.Obstacle
                || constraint == SpeedConstraint.PerceptionUnavailable;
        }

        internal static string F(float value)
        {
            return float.IsNaN(value) ? "NaN" : value.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
