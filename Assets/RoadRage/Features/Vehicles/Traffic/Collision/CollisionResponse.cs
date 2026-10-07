using System;
using System.Globalization;

namespace RoadRage.Features.Vehicles.Traffic.Collisions
{
    /// <summary>
    /// Faits de collision d'un pas physique (Story 5.38, C1) : contacts de caisse hors dessous (filtre 5.15), impulsion et
    /// vitesse normale relative maximales du pas, duree du contact continu, cote d'impact, rotation, roues au sol et ecart d
    /// de la caisse a sa pose nominale (meme mesure qu'epsilon_t). Des faits, jamais une decision.
    /// </summary>
    public readonly struct CollisionFacts
    {
        public readonly ulong Step;
        /// <summary>Contact de caisse hors dessous pendant le pas.</summary>
        public readonly bool InContact;
        /// <summary>Plus grande impulsion d'une paire de contact du pas (N.s).</summary>
        public readonly float ImpulseNewtonSeconds;
        /// <summary>Plus grande vitesse normale relative d'un point de contact du pas (m/s).</summary>
        public readonly float RelativeNormalSpeedMetersPerSecond;
        /// <summary>Duree du contact continu en cours (s) ; 0 sans contact.</summary>
        public readonly float ContactSeconds;
        /// <summary>Cote d'impact dans le repere de la caisse : +1 droite, -1 gauche, 0 inconnu.</summary>
        public readonly int ImpactSide;
        /// <summary>Vitesse de lacet (rad/s, autour de l'axe haut de la caisse).</summary>
        public readonly float YawRateRadiansPerSecond;
        /// <summary>Norme de la vitesse angulaire (rad/s).</summary>
        public readonly float AngularSpeedRadiansPerSecond;
        public readonly int GroundedWheels;
        public readonly int WheelCount;
        /// <summary>Ecart d de la caisse a sa pose nominale (TrackingMeasurement.StepDisplacement, m).</summary>
        public readonly float DisplacementMeters;
        public readonly float MassKilograms;

        public CollisionFacts(ulong step, bool inContact, float impulse, float relativeNormalSpeed, float contactSeconds,
            int impactSide, float yawRate, float angularSpeed, int groundedWheels, int wheelCount, float displacement, float mass)
        {
            Step = step; InContact = inContact; ImpulseNewtonSeconds = impulse;
            RelativeNormalSpeedMetersPerSecond = relativeNormalSpeed; ContactSeconds = contactSeconds;
            ImpactSide = Math.Sign(impactSide); YawRateRadiansPerSecond = yawRate; AngularSpeedRadiansPerSecond = angularSpeed;
            GroundedWheels = groundedWheels; WheelCount = wheelCount; DisplacementMeters = displacement; MassKilograms = mass;
        }

        /// <summary>Delta-v du choc, J / m (m/s) ; 0 sans masse positive.</summary>
        public float DeltaVMetersPerSecond { get { return MassKilograms > 0f ? ImpulseNewtonSeconds / MassKilograms : 0f; } }

        public bool IsFinite
        {
            get
            {
                return Finite(ImpulseNewtonSeconds) && Finite(RelativeNormalSpeedMetersPerSecond) && Finite(ContactSeconds)
                    && Finite(YawRateRadiansPerSecond) && Finite(AngularSpeedRadiansPerSecond) && Finite(DisplacementMeters)
                    && Finite(MassKilograms);
            }
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    /// <summary>
    /// Seuils declares de la Story 5.38 (decision D1, 2026-10-07), calibres par la recette Gate D. Toute revision est une
    /// decision proprietaire (Ask First).
    /// </summary>
    public static class CollisionThresholds
    {
        /// <summary>Choc : vitesse normale relative minimale, seuil de degat mesure par le banc 5.15 (m/s).</summary>
        public const float SignificantRelativeSpeedMetersPerSecond = NetworkedVehicleDriverController.MinCollisionDamageSpeed;
        /// <summary>Choc : delta-v minimal J / m (m/s).</summary>
        public const float SignificantDeltaVMetersPerSecond = 1f;
        /// <summary>Poussee : duree minimale du contact continu (s).</summary>
        public const float SustainedPushSeconds = 0.25f;
        /// <summary>Stabilite : lacet maximal (rad/s).</summary>
        public const float StableYawRateRadiansPerSecond = 0.3f;
        /// <summary>Stabilite : vitesse angulaire maximale (rad/s).</summary>
        public const float StableAngularSpeedRadiansPerSecond = 0.5f;
        /// <summary>Stabilite : duree minimale ou toutes les conditions tiennent (s).</summary>
        public const float StableSeconds = 0.5f;
    }

    /// <summary>Motif de significativite, dans l'ordre d'evaluation.</summary>
    public enum CollisionSignificance { None = 0, RelativeSpeed = 1, DeltaV = 2, SustainedPush = 3 }

    /// <summary>Premiere cause qui empeche la stabilite physique (C6), dans l'ordre d'evaluation.</summary>
    public enum StabilityBlocker { None = 0, SignificantContact = 1, YawRate = 2, AngularSpeed = 3, WheelsAirborne = 4, NonFiniteFacts = 5 }

    /// <summary>Predicats purs de significativite (C2) et de stabilite (C6).</summary>
    public static class CollisionPredicates
    {
        /// <param name="toleranceMeters">epsilon_t declare : une poussee qui deplace la caisse au-dela est instable.</param>
        public static CollisionSignificance Significance(CollisionFacts facts, float toleranceMeters)
        {
            if (!facts.InContact || !facts.IsFinite) return CollisionSignificance.None;
            if (facts.RelativeNormalSpeedMetersPerSecond >= CollisionThresholds.SignificantRelativeSpeedMetersPerSecond)
                return CollisionSignificance.RelativeSpeed;
            if (facts.DeltaVMetersPerSecond >= CollisionThresholds.SignificantDeltaVMetersPerSecond) return CollisionSignificance.DeltaV;
            if (facts.ContactSeconds >= CollisionThresholds.SustainedPushSeconds
                && (PhysicalInstability(facts) != StabilityBlocker.None || facts.DisplacementMeters > toleranceMeters))
                return CollisionSignificance.SustainedPush;
            return CollisionSignificance.None;
        }

        /// <summary>Premiere cause d'instabilite physique ; None si le vehicule est physiquement stable.</summary>
        public static StabilityBlocker Stability(CollisionFacts facts, float toleranceMeters)
        {
            if (!facts.IsFinite) return StabilityBlocker.NonFiniteFacts;
            if (Significance(facts, toleranceMeters) != CollisionSignificance.None) return StabilityBlocker.SignificantContact;
            return PhysicalInstability(facts);
        }

        /// <summary>
        /// Decision D3 : le verrou 2a d'epsilon_t est suspendu pendant un but de collision et tant qu'un contact de caisse
        /// dure. Une poussee lente qui deplace la caisse devient significative au plus tard a 0,25 s de contact, sans que le
        /// verrou ait pu la devancer.
        /// </summary>
        public static bool SuspendsToleranceLatch(bool goalActive, CollisionFacts facts)
        {
            return goalActive || facts.InContact;
        }

        private static StabilityBlocker PhysicalInstability(CollisionFacts facts)
        {
            if (Math.Abs(facts.YawRateRadiansPerSecond) > CollisionThresholds.StableYawRateRadiansPerSecond) return StabilityBlocker.YawRate;
            if (facts.AngularSpeedRadiansPerSecond > CollisionThresholds.StableAngularSpeedRadiansPerSecond) return StabilityBlocker.AngularSpeed;
            if (facts.GroundedWheels < facts.WheelCount) return StabilityBlocker.WheelsAirborne;
            return StabilityBlocker.None;
        }
    }

    /// <summary>
    /// Accumulateur des contacts d'un pas simule (C1) : le driver y verse chaque paire de contact hors dessous, puis la
    /// preparation du pas suivant le consomme en faits. Le cote d'impact est la position laterale moyenne des points de la
    /// paire la plus rapide, nul dans la zone morte (choc frontal ou arriere).
    /// </summary>
    public sealed class StepContactAccumulator
    {
        /// <summary>Zone morte laterale du cote d'impact, dans le repere de la caisse (m).</summary>
        public const float ImpactSideDeadbandMeters = 0.25f;

        private bool contact;
        private float impulse;
        private float relativeSpeed;
        private float impactLocalX;
        private ulong streakStart;

        /// <param name="meanLocalX">Position laterale moyenne des points de la paire, depuis le centre de la caisse (m).</param>
        public void Add(float pairImpulse, float pairMaxNormalSpeed, float meanLocalX)
        {
            contact = true;
            impulse = Math.Max(impulse, pairImpulse);
            if (pairMaxNormalSpeed < relativeSpeed) return;
            relativeSpeed = pairMaxNormalSpeed;
            impactLocalX = meanLocalX;
        }

        /// <summary>Oublie tout, contact continu compris (vehicule inerte : aucun pas ne consomme).</summary>
        public void Reset()
        {
            contact = false; impulse = 0f; relativeSpeed = 0f; impactLocalX = 0f; streakStart = 0UL;
        }

        /// <summary>Faits du pas, puis remise a zero des valeurs du pas ; le contact continu survit tant qu'il dure.</summary>
        public CollisionFacts Consume(ulong step, float deltaTimeSeconds, float yawRate, float angularSpeed, int groundedWheels,
            int wheelCount, float displacement, float mass)
        {
            if (!contact) streakStart = 0UL;
            else if (streakStart == 0UL) streakStart = step;
            float contactSeconds = contact ? (step - streakStart + 1UL) * deltaTimeSeconds : 0f;
            int side = Math.Abs(impactLocalX) > ImpactSideDeadbandMeters ? Math.Sign(impactLocalX) : 0;
            var facts = new CollisionFacts(step, contact, impulse, relativeSpeed, contactSeconds, side, yawRate, angularSpeed,
                groundedWheels, wheelCount, displacement, mass);
            contact = false; impulse = 0f; relativeSpeed = 0f; impactLocalX = 0f;
            return facts;
        }
    }

    /// <summary>Requete versionnee de reponse a collision (C3) : l'analyse la soumet, la tactique l'accepte ou la refuse.</summary>
    public sealed class CollisionResponseRequest
    {
        public ulong Version { get; }
        public ulong SourceFrameId { get; }
        public RoadId TrafficId { get; }
        public CollisionSignificance Significance { get; }
        public CollisionFacts Facts { get; }

        public CollisionResponseRequest(ulong version, ulong sourceFrameId, RoadId trafficId, CollisionSignificance significance,
            CollisionFacts facts)
        {
            Version = version; SourceFrameId = sourceFrameId; TrafficId = trafficId; Significance = significance; Facts = facts;
        }

        public string ToText()
        {
            return "v" + Version.ToString(CultureInfo.InvariantCulture) + " " + Significance + " source "
                + SourceFrameId.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Analyse de collision d'un vehicule (C3) : elle rend au plus une requete par pas, de version strictement croissante,
    /// et ne pose jamais de but.
    /// </summary>
    public sealed class CollisionAnalysis
    {
        private ulong lastVersion;

        public CollisionResponseRequest Analyze(CollisionFacts facts, ulong frameId, RoadId trafficId, float toleranceMeters)
        {
            var significance = CollisionPredicates.Significance(facts, toleranceMeters);
            // Faits invalides : la tactique doit publier InvalidRequest, pas une absence silencieuse de requete.
            if (facts.IsFinite && significance == CollisionSignificance.None) return null;
            return new CollisionResponseRequest(++lastVersion, frameId, trafficId, significance, facts);
        }
    }
}
