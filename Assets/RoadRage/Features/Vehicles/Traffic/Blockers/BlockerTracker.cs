using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Planning;

namespace RoadRage.Features.Vehicles.Traffic.Blockers
{
    /// <summary>
    /// Ensemble de blockers d'un pas (5.33), fonction pure de la decision longitudinale et de l'ensemble precedent. Le
    /// driver porte l'ensemble d'un pas au suivant ; rien d'autre n'est conserve. Un blocker est une cause reelle
    /// d'immobilisation (D11) : pendant un maintien a l'arret, sa source et chaque autre leader ou obstacle du couloir
    /// balaye qui tiendrait seul le maintien (<see cref="LongitudinalDecision.HoldCauses"/>) ; a tout moment,
    /// l'immobilisation de politique (v0 effectif &lt;= 0,001 m/s, candidat DesiredSpeed). Un candidat &lt;= 0 en roulant
    /// n'est pas un blocker, une vitesse basse non plus.
    /// </summary>
    public static class BlockerTracker
    {
        private static readonly Blocker[] None = new Blocker[0];

        /// <summary>Ensemble vide (aucune decision longitudinale au pas : repli, sortie, frame absente).</summary>
        public static IReadOnlyList<Blocker> Empty { get { return Array.AsReadOnly(None); } }

        /// <summary>
        /// Ensemble du pas <paramref name="frameId"/>, trie par (genre, id). SinceFrame est repris d'un blocker du meme
        /// (genre, bloqueur) present au pas precedent, sinon il vaut <paramref name="frameId"/> : une absence d'un pas
        /// reinitialise la continuite.
        /// </summary>
        /// <param name="junction">Cause de coordination d'un maintien a l'entree d'une traversee (5.34) ; sans objet sinon.</param>
        public static IReadOnlyList<Blocker> Update(IReadOnlyList<Blocker> previous, LongitudinalDecision decision,
            DriverProfile driver, ulong frameId, JunctionBlockerCause junction = default(JunctionBlockerCause))
        {
            if (decision == null) return Empty;
            var current = new List<Blocker>();
            int unseenHold = -1;
            for (int i = 0; i < decision.HoldCauses.Count; i++)
            {
                var cause = decision.HoldCauses[i];
                if (cause.Kind == LongitudinalCandidateKind.StopHold && float.IsNaN(cause.GapMeters)) unseenHold = current.Count;
                bool leader = cause.Kind == LongitudinalCandidateKind.LeaderFollowing
                    || (cause.Kind == LongitudinalCandidateKind.StopHold && cause.Constraint == SpeedConstraint.LeaderFollowing);
                bool entry = cause.Kind == LongitudinalCandidateKind.JunctionEntry
                    || (cause.Kind == LongitudinalCandidateKind.StopHold && cause.Constraint == SpeedConstraint.JunctionEntry);
                current.Add(entry ? BlockerRules.Junction(junction, cause.SourceId.ToString(), cause.AccelerationMetersPerSecondSquared, frameId)
                    : leader
                    ? BlockerRules.Leader(cause.SourceId, cause.GapMeters, driver.MinimumGap, cause.AccelerationMetersPerSecondSquared, frameId)
                    : BlockerRules.Obstacle(cause.SourceId, cause.ObstacleKind, cause.AccelerationMetersPerSecondSquared, frameId));
            }
            float desired = float.IsNaN(driver.DesiredSpeed) || float.IsInfinity(driver.DesiredSpeed) ? 0f : driver.DesiredSpeed;
            if (desired <= BlockerRules.PolicyImmobilizationSpeedMetersPerSecond
                && decision.FreeRoadAccelerationMetersPerSecondSquared <= 0f)
                current.Add(BlockerRules.PolicyImmobilization(decision.FreeRoadAccelerationMetersPerSecondSquared, frameId));

            for (int i = 0; i < current.Count; i++)
                if (previous != null)
                    for (int k = 0; k < previous.Count; k++)
                        if (previous[k].Kind == current[i].Kind
                            && string.Equals(previous[k].BlockingActorOrRule, current[i].BlockingActorOrRule, StringComparison.Ordinal))
                        {
                            // Source d'un maintien non vue (perception indisponible, jeu NaN) : le blocker garde les
                            // proprietes connues ; un jeu inconnu ne fait pas d'un arret voulu un encastrement.
                            current[i] = i == unseenHold
                                ? previous[k].WithCandidate(current[i].CandidateAccelerationMetersPerSecondSquared)
                                : current[i].WithSince(previous[k].SinceFrame);
                            break;
                        }
            current.Sort(CompareKindThenId);
            return current.AsReadOnly();
        }

        /// <summary>
        /// Dominant d'un ensemble non vide : le plus petit candidat, egalite (1e-6) departagee par genre puis id. Il
        /// peut differer de la liante, par exemple quand une limite de courbe freine davantage.
        /// </summary>
        public static bool TryGetDominant(IReadOnlyList<Blocker> blockers, out Blocker dominant)
        {
            dominant = default(Blocker);
            if (blockers == null || blockers.Count == 0) return false;
            float minimum = float.PositiveInfinity;
            for (int i = 0; i < blockers.Count; i++)
                minimum = Math.Min(minimum, blockers[i].CandidateAccelerationMetersPerSecondSquared);
            bool found = false;
            for (int i = 0; i < blockers.Count; i++)
            {
                var b = blockers[i];
                if (b.CandidateAccelerationMetersPerSecondSquared > minimum + LongitudinalArbitration.TieToleranceMetersPerSecondSquared)
                    continue;
                if (!found || CompareKindThenId(b, dominant) < 0) { dominant = b; found = true; }
            }
            return found;
        }

        private static int CompareKindThenId(Blocker a, Blocker b)
        {
            int order = a.Kind.CompareTo(b.Kind);
            return order != 0 ? order : string.CompareOrdinal(a.Id, b.Id);
        }
    }
}
