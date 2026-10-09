using System;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Policy
{
    /// <summary>Story 5.41 : surfaces qu'un conducteur accepte d'emprunter (ce qu'il veut, pas ce qui est permis).</summary>
    [Flags]
    public enum DrivingSurface
    {
        None = 0,
        /// <summary>Son corridor et les corridors de meme sens.</summary>
        Carriageway = 1,
        /// <summary>Le corridor de sens oppose (exige une exception OpposingCorridor acceptee).</summary>
        OpposingCorridor = 2,
        /// <summary>Le trottoir (exige une exception Sidewalk acceptee).</summary>
        Sidewalk = 4
    }

    /// <summary>Story 5.41 : candidats de manoeuvre de 5.42. La politique les type et les autorise, elle ne les construit pas.</summary>
    public enum ManeuverKind
    {
        CorridorOffset = 0,
        AdjacentCorridor = 1,
        OpposingCorridor = 2,
        AuthorizedSurface = 3
    }

    /// <summary>
    /// Story 5.41 (AD-39) : regles legales ou cooperatives, seules derogeables par exception. Les SimulationInvariants n'ont
    /// aucune valeur ici : elles ne sont pas representables comme regle.
    /// </summary>
    public enum TrafficRule
    {
        None = 0,
        SpeedLimit = 1,
        FollowingGap = 2,
        /// <summary>Stop, cedez-le-passage, priorite : jamais un grant de carrefour.</summary>
        JunctionControl = 3,
        SignalCompliance = 4,
        LaneChange = 5,
        KeepClear = 6,
        /// <summary>Interdiction du sens oppose.</summary>
        OpposingCorridor = 7,
        /// <summary>Interdiction du trottoir.</summary>
        Sidewalk = 8
    }

    /// <summary>Volonte et cout authores d'une manoeuvre.</summary>
    [Serializable]
    public struct ManeuverPreference
    {
        [SerializeField, Tooltip("Le conducteur accepte de tenter cette manoeuvre quand la geometrie l'offre.")]
        private bool willing;
        [SerializeField, Min(0f), Tooltip("Cout tactique relatif de la manoeuvre (sans unite, >= 0).")]
        private float cost;

        public ManeuverPreference(bool willing, float cost) { this.willing = willing; this.cost = cost; }

        public bool Willing { get { return willing; } }
        public float Cost { get { return cost; } }
    }

    /// <summary>
    /// Story 5.41 : politique authoree d'un conducteur, hors de <see cref="DriverProfile"/> (le V1 ne la lit pas), sur le
    /// patron de CollisionReactionWeights. Que des valeurs : une copie ne partage rien.
    /// </summary>
    [Serializable]
    public struct DrivingPolicyProfile
    {
        [SerializeField, Range(0f, 1f), Tooltip("Risque accepte, 0 = aucun, 1 = maximal.")]
        private float acceptedRisk;
        [SerializeField, Min(0f), Tooltip("Creneau minimal accepte (s) pour une manoeuvre face a un autre usager.")]
        private float acceptedGapSeconds;
        [SerializeField, Min(0f), Tooltip("Poids applique a la preference de route authoree (1 = telle quelle).")]
        private float routePreferenceWeight;
        [SerializeField, Tooltip("Surfaces que le conducteur accepte d'emprunter.")]
        private DrivingSurface allowedSurfaces;
        [SerializeField] private ManeuverPreference corridorOffset;
        [SerializeField] private ManeuverPreference adjacentCorridor;
        [SerializeField] private ManeuverPreference opposingCorridor;
        [SerializeField] private ManeuverPreference authorizedSurface;

        public DrivingPolicyProfile(float acceptedRisk, float acceptedGapSeconds, float routePreferenceWeight,
            DrivingSurface allowedSurfaces, ManeuverPreference corridorOffset, ManeuverPreference adjacentCorridor,
            ManeuverPreference opposingCorridor, ManeuverPreference authorizedSurface)
        {
            this.acceptedRisk = acceptedRisk; this.acceptedGapSeconds = acceptedGapSeconds;
            this.routePreferenceWeight = routePreferenceWeight; this.allowedSurfaces = allowedSurfaces;
            this.corridorOffset = corridorOffset; this.adjacentCorridor = adjacentCorridor;
            this.opposingCorridor = opposingCorridor; this.authorizedSurface = authorizedSurface;
        }

        /// <summary>Conducteur normal (decision D3 du 2026-10-09).</summary>
        public static DrivingPolicyProfile Default
        {
            get
            {
                return new DrivingPolicyProfile(0.3f, 4f, 1f, DrivingSurface.Carriageway | DrivingSurface.OpposingCorridor,
                    new ManeuverPreference(true, 1f), new ManeuverPreference(true, 1f), new ManeuverPreference(true, 1f),
                    new ManeuverPreference(false, 1f));
            }
        }

        public float AcceptedRisk { get { return acceptedRisk; } }
        public float AcceptedGapSeconds { get { return acceptedGapSeconds; } }
        public float RoutePreferenceWeight { get { return routePreferenceWeight; } }
        public DrivingSurface AllowedSurfaces { get { return allowedSurfaces; } }

        public ManeuverPreference PreferenceOf(ManeuverKind kind)
        {
            switch (kind)
            {
                case ManeuverKind.CorridorOffset: return corridorOffset;
                case ManeuverKind.AdjacentCorridor: return adjacentCorridor;
                case ManeuverKind.OpposingCorridor: return opposingCorridor;
                case ManeuverKind.AuthorizedSurface: return authorizedSurface;
                default: return default(ManeuverPreference);
            }
        }

        public bool TryValidate(out string error)
        {
            const DrivingSurface known = DrivingSurface.Carriageway | DrivingSurface.OpposingCorridor | DrivingSurface.Sidewalk;
            if (!(acceptedRisk >= 0f && acceptedRisk <= 1f)) { error = "Policy invalide : acceptedRisk fini dans [0, 1]."; return false; }
            if (!NonNegative(acceptedGapSeconds) || !NonNegative(routePreferenceWeight))
            { error = "Policy invalide : acceptedGapSeconds et routePreferenceWeight finis et positifs ou nuls."; return false; }
            if ((allowedSurfaces & ~known) != 0) { error = "Policy invalide : surface inconnue."; return false; }
            for (int i = 0; i < DrivingPolicy.ManeuverCount; i++)
                if (!NonNegative(PreferenceOf((ManeuverKind)i).Cost))
                { error = "Policy invalide : cout de manoeuvre fini et positif ou nul."; return false; }
            error = string.Empty;
            return true;
        }

        private static bool NonNegative(float value) { return float.IsFinite(value) && value >= 0f; }
    }

    /// <summary>Manoeuvre resolue : eligible = volontaire et surface requise permise.</summary>
    public readonly struct ManeuverPolicy
    {
        public readonly bool Eligible;
        public readonly float Cost;

        public ManeuverPolicy(bool eligible, float cost) { Eligible = eligible; Cost = cost; }
    }

    /// <summary>
    /// Story 5.41 (P1) : parametres effectifs d'un vehicule, immuables, derives sans muter la definition authoree. Ce que le
    /// conducteur accepte de tenter ; jamais ce qui est permis (autorite des regles) ni ce qui est faisable (tactique, 5.42).
    /// </summary>
    public readonly struct EffectivePolicy
    {
        private readonly DrivingPolicyProfile authored;
        private readonly float phase;

        public readonly RoadId TrafficId;
        /// <summary>Vitesse desiree, temps inter-vehiculaire et ecart minimal effectifs (5.41 : le profil authore).</summary>
        public readonly DriverProfile Driver;

        internal EffectivePolicy(RoadId trafficId, DriverProfile driver, DrivingPolicyProfile authored, float phase)
        {
            TrafficId = trafficId; Driver = driver; this.authored = authored; this.phase = phase;
        }

        public float AcceptedRisk { get { return authored.AcceptedRisk; } }
        public float AcceptedGapSeconds { get { return authored.AcceptedGapSeconds; } }
        public float RoutePreferenceWeight { get { return authored.RoutePreferenceWeight; } }
        public DrivingSurface AllowedSurfaces { get { return authored.AllowedSurfaces; } }
        /// <summary>Phase de variation dans ]0, 1[, tiree de (graine, TrafficId).</summary>
        public float VariationPhase { get { return phase; } }

        public bool Allows(DrivingSurface surface)
        {
            return surface != DrivingSurface.None && (authored.AllowedSurfaces & surface) == surface;
        }

        public ManeuverPolicy Maneuver(ManeuverKind kind)
        {
            var preference = authored.PreferenceOf(kind);
            return new ManeuverPolicy(preference.Willing && Allows(DrivingPolicy.SurfaceOf(kind)), preference.Cost);
        }

        /// <summary>P2 : vitesse desiree a l'instant donne, deterministe et bornee par l'enveloppe de Consistency.</summary>
        public float DesiredSpeedAt(float seconds)
        {
            return DriverModel.ResolveNoisyDesiredSpeed(Driver.DesiredSpeed, Driver.Consistency, seconds, phase);
        }

        public string ToText()
        {
            var text = "risk " + F(AcceptedRisk) + " gap " + F(AcceptedGapSeconds) + "s route " + F(RoutePreferenceWeight)
                + " surfaces " + AllowedSurfaces + " manoeuvres";
            for (int i = 0; i < DrivingPolicy.ManeuverCount; i++)
            {
                var maneuver = Maneuver((ManeuverKind)i);
                text += " " + (ManeuverKind)i + (maneuver.Eligible ? ":" + F(maneuver.Cost) : ":non");
            }
            return text;
        }

        private static string F(float value) { return value.ToString("0.###", CultureInfo.InvariantCulture); }
    }

    /// <summary>Story 5.41 (P3) : terminaison d'une exception, toujours bornee par une frame d'expiration.</summary>
    public enum RuleExceptionTerminationKind
    {
        /// <summary>Fin a l'expiration seule.</summary>
        Expiry = 0,
        /// <summary>Fin quand le demandeur, entre dans la portee, en sort ; au plus tard a l'expiration.</summary>
        ScopeExited = 1
    }

    public readonly struct RuleExceptionTermination
    {
        public readonly RuleExceptionTerminationKind Kind;
        /// <summary>Derniere frame ou l'exception peut etre effective.</summary>
        public readonly ulong ExpiryFrame;

        public RuleExceptionTermination(RuleExceptionTerminationKind kind, ulong expiryFrame) { Kind = kind; ExpiryFrame = expiryFrame; }

        public string ToText() { return Kind + "<=" + ExpiryFrame.ToString(CultureInfo.InvariantCulture); }
    }

    /// <summary>
    /// Story 5.41 (P3) : demande explicite de derogation. Une demande n'autorise rien : seule l'autorite des regles la decide.
    /// </summary>
    public sealed class RuleExceptionRequest
    {
        public RoadId Requester { get; }
        public TrafficRule Rule { get; }
        /// <summary>Corridor ou l'exception s'applique ; None si seule une cible est nommee.</summary>
        public RoadId Scope { get; }
        /// <summary>Usager vise ; None si seule une portee est nommee.</summary>
        public RoadId Target { get; }
        public string StartReason { get; }
        public RuleExceptionTermination Termination { get; }
        /// <summary>Frame N de la demande.</summary>
        public ulong SourceFrame { get; }

        public RuleExceptionRequest(RoadId requester, TrafficRule rule, RoadId scope, RoadId target, string startReason,
            RuleExceptionTermination termination, ulong sourceFrame)
        {
            Requester = requester; Rule = rule; Scope = scope; Target = target; StartReason = startReason;
            Termination = termination; SourceFrame = sourceFrame;
        }

        public string ToText()
        {
            return Requester + " " + Rule + (Scope.IsEmpty ? "" : " scope " + Scope) + (Target.IsEmpty ? "" : " target " + Target)
                + " '" + StartReason + "' " + Termination.ToText();
        }
    }

    /// <summary>
    /// Story 5.41 (AD-39, AD-41) : frontiere de politique de conduite. Resout la personnalite en parametres effectifs et peut
    /// proposer une exception. Elle ne lit aucune frame, aucune observation, aucun chemin ni aucun carrefour, et n'accorde rien.
    /// </summary>
    public static class DrivingPolicy
    {
        /// <summary>Domaine du tirage de phase de variation.</summary>
        public const string VariationDrawDomain = "policy";
        public const int ManeuverCount = 4;

        public static EffectivePolicy Resolve(DriverProfileDef definition, RoadId trafficId, ulong seed)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            return Resolve(definition.Profile, definition.Policy, trafficId, seed);
        }

        /// <summary>P1 : fonction pure ; les structs sont copiees, rien n'est partage entre vehicules.</summary>
        public static EffectivePolicy Resolve(DriverProfile driver, DrivingPolicyProfile authored, RoadId trafficId, ulong seed)
        {
            float phase = (float)RoutePlanner.UnitDraw(seed, trafficId, VariationDrawDomain, 0UL, RoadId.None);
            return new EffectivePolicy(trafficId, driver, authored, phase);
        }

        /// <summary>Surface requise par une manoeuvre.</summary>
        public static DrivingSurface SurfaceOf(ManeuverKind kind)
        {
            switch (kind)
            {
                case ManeuverKind.CorridorOffset:
                case ManeuverKind.AdjacentCorridor: return DrivingSurface.Carriageway;
                case ManeuverKind.OpposingCorridor: return DrivingSurface.OpposingCorridor;
                case ManeuverKind.AuthorizedSurface: return DrivingSurface.Sidewalk;
                default: return DrivingSurface.None;
            }
        }

        /// <summary>Surface dont la regle interdit l'usage ; None : aucune volonte authorable en 5.41.</summary>
        public static DrivingSurface SurfaceOf(TrafficRule rule)
        {
            switch (rule)
            {
                case TrafficRule.OpposingCorridor: return DrivingSurface.OpposingCorridor;
                case TrafficRule.Sidewalk: return DrivingSurface.Sidewalk;
                default: return DrivingSurface.None;
            }
        }

        /// <summary>
        /// P3 : demande de derogation au nom du vehicule de la politique, seulement si elle permet la surface liee a la regle.
        /// La demande est soumise a l'autorite des regles ; la politique ne la decide jamais.
        /// </summary>
        public static bool TryPropose(EffectivePolicy policy, TrafficRule rule, RoadId scope, RoadId target, string startReason,
            RuleExceptionTermination termination, ulong frameId, out RuleExceptionRequest request)
        {
            request = null;
            if (policy.TrafficId.IsEmpty || !policy.Allows(SurfaceOf(rule))) return false;
            request = new RuleExceptionRequest(policy.TrafficId, rule, scope, target, startReason, termination, frameId);
            return true;
        }
    }
}
