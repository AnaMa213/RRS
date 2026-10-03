using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Perception;

namespace RoadRage.Features.Vehicles.Traffic.Blockers
{
    /// <summary>Genre d'un blocker (5.33). L'ordre des valeurs departage le dominant a candidat egal.</summary>
    public enum BlockerKind
    {
        Obstacle = 0,
        Leader = 1,
        PolicyImmobilization = 2
    }

    /// <summary>Fait d'origine d'un blocker.</summary>
    public enum BlockerSource
    {
        LeaderObservation = 0,
        ObstacleObservation = 1,
        DrivingPolicy = 2
    }

    /// <summary>
    /// Cause externe qui immobilise le vehicule a une frame (contrat « Blockers and waiting ») : la source d'un maintien
    /// a l'arret D11 et les autres faits qui le tiendraient seuls, ou l'immobilisation de politique. Ni un candidat
    /// &lt;= 0 en roulant, ni une vitesse basse n'en font un. Plusieurs causes forment un ensemble ; aucune n'ecrase les
    /// autres.
    /// </summary>
    public readonly struct Blocker
    {
        /// <summary>Genre et bloqueur : "Leader:&lt;id&gt;", "Obstacle:&lt;id&gt;", "PolicyImmobilization:PolicyImmobilization".</summary>
        public readonly string Id;
        public readonly BlockerSource Source;
        public readonly BlockerKind Kind;
        /// <summary>Identite de l'acteur ou du danger, ou code de la regle.</summary>
        public readonly string BlockingActorOrRule;
        public readonly bool Legitimate;
        public readonly bool ExpectedToClear;
        public readonly bool Recoverable;
        /// <summary>Premiere frame de presence continue du meme (genre, bloqueur).</summary>
        public readonly ulong SinceFrame;
        /// <summary>Candidat longitudinal de la cause : StopHold pour la source du maintien, son propre candidat sinon.</summary>
        public readonly float CandidateAccelerationMetersPerSecondSquared;

        public Blocker(BlockerSource source, BlockerKind kind, string blockingActorOrRule, bool legitimate,
            bool expectedToClear, bool recoverable, ulong sinceFrame, float candidateAcceleration)
        {
            Id = kind + ":" + blockingActorOrRule;
            Source = source; Kind = kind; BlockingActorOrRule = blockingActorOrRule; Legitimate = legitimate;
            ExpectedToClear = expectedToClear; Recoverable = recoverable; SinceFrame = sinceFrame;
            CandidateAccelerationMetersPerSecondSquared = candidateAcceleration;
        }

        internal Blocker WithSince(ulong frame)
        {
            return new Blocker(Source, Kind, BlockingActorOrRule, Legitimate, ExpectedToClear, Recoverable, frame,
                CandidateAccelerationMetersPerSecondSquared);
        }

        internal Blocker WithCandidate(float candidateAcceleration)
        {
            return new Blocker(Source, Kind, BlockingActorOrRule, Legitimate, ExpectedToClear, Recoverable, SinceFrame,
                candidateAcceleration);
        }

        /// <summary>Texte deterministe et invariant de culture.</summary>
        public string ToText()
        {
            return Id + " " + Source + (Legitimate ? " legitime" : " illegitime")
                + (ExpectedToClear ? " liberation attendue" : " sans liberation attendue")
                + (Recoverable ? " recouvrable" : " non recouvrable") + " depuis "
                + SinceFrame.ToString(CultureInfo.InvariantCulture) + " candidat "
                + CandidateAccelerationMetersPerSecondSquared.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Table des genres (5.33, documentee pour 5.39) :
    /// Leader : legitime si jeu &gt;= s0/2 (sinon encastrement), liberation attendue, recouvrable seulement s'il est
    /// illegitime ; Obstacle mobile (joueur, pieton, vehicule, acteur) : legitime, liberation attendue, non
    /// recouvrable ; Obstacle fixe : legitime, sans liberation attendue, recouvrable (5.42) ; PolicyImmobilization :
    /// legitime, sans liberation attendue, non recouvrable.
    /// </summary>
    public static class BlockerRules
    {
        /// <summary>
        /// v0 effectif sous lequel le profil immobilise le vehicule : meme seuil que le noyau IDM (DriverModel,
        /// constante privee de 0,001 m/s), epingle par les tests de la 5.33.
        /// </summary>
        public const float PolicyImmobilizationSpeedMetersPerSecond = 0.001f;

        public const string PolicyImmobilizationRule = "PolicyImmobilization";

        public static Blocker Leader(RoadId leaderId, float gapMeters, float minimumGapMeters, float candidate, ulong sinceFrame)
        {
            bool legitimate = DriverModel.IsDeliberateStop(true, gapMeters, minimumGapMeters);
            return new Blocker(BlockerSource.LeaderObservation, BlockerKind.Leader, leaderId.ToString(), legitimate, true,
                !legitimate, sinceFrame, candidate);
        }

        public static Blocker Obstacle(RoadId obstacleId, PerceivedObstacleKind kind, float candidate, ulong sinceFrame)
        {
            bool mobile = IsMobile(kind);
            return new Blocker(BlockerSource.ObstacleObservation, BlockerKind.Obstacle, obstacleId.ToString(), true, mobile,
                !mobile, sinceFrame, candidate);
        }

        public static Blocker PolicyImmobilization(float candidate, ulong sinceFrame)
        {
            return new Blocker(BlockerSource.DrivingPolicy, BlockerKind.PolicyImmobilization, PolicyImmobilizationRule, true,
                false, false, sinceFrame, candidate);
        }

        /// <summary>Joueur, pieton, vehicule et acteur de trafic sont mobiles ; un obstacle non classe est fixe.</summary>
        public static bool IsMobile(PerceivedObstacleKind kind)
        {
            return kind != PerceivedObstacleKind.Obstacle;
        }
    }
}
