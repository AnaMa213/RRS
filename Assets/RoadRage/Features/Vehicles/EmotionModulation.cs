using System;
using RoadRage.Shared.Domain;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.43 (E4) : gains signes d'un canal d'emotion, un par levier de politique. Facteur effectif d'un levier :
    /// max(0, 1 + gR.wR + gP.wP). Que des valeurs : une copie ne partage rien.
    /// </summary>
    [Serializable]
    public struct EmotionGains
    {
        [SerializeField] private float desiredSpeed;
        [SerializeField] private float timeHeadway;
        [SerializeField] private float minimumGap;
        [SerializeField, Tooltip("Gain sur a : strictement superieur a -1 (a reste > 0).")] private float maxAcceleration;
        [SerializeField, Tooltip("Gain sur b : strictement superieur a -1 (b reste > 0).")] private float comfortableDeceleration;
        [SerializeField] private float safeBrakingLimit;
        [SerializeField] private float acceptedRisk;
        [SerializeField] private float acceptedGap;
        [SerializeField] private float maneuverCost;

        public EmotionGains(float desiredSpeed, float timeHeadway, float minimumGap, float maxAcceleration,
            float comfortableDeceleration, float safeBrakingLimit, float acceptedRisk, float acceptedGap, float maneuverCost)
        {
            this.desiredSpeed = desiredSpeed; this.timeHeadway = timeHeadway; this.minimumGap = minimumGap;
            this.maxAcceleration = maxAcceleration; this.comfortableDeceleration = comfortableDeceleration;
            this.safeBrakingLimit = safeBrakingLimit; this.acceptedRisk = acceptedRisk; this.acceptedGap = acceptedGap;
            this.maneuverCost = maneuverCost;
        }

        public float DesiredSpeed { get { return desiredSpeed; } }
        public float TimeHeadway { get { return timeHeadway; } }
        public float MinimumGap { get { return minimumGap; } }
        public float MaxAcceleration { get { return maxAcceleration; } }
        public float ComfortableDeceleration { get { return comfortableDeceleration; } }
        public float SafeBrakingLimit { get { return safeBrakingLimit; } }
        public float AcceptedRisk { get { return acceptedRisk; } }
        public float AcceptedGap { get { return acceptedGap; } }
        public float ManeuverCost { get { return maneuverCost; } }

        public bool TryValidate(string channel, out string error)
        {
            // Valide a chaque pas resolu : aucune allocation sur le chemin nominal.
            if (!AtLeastMinusOne(desiredSpeed) || !AtLeastMinusOne(timeHeadway) || !AtLeastMinusOne(minimumGap)
                || !AtLeastMinusOne(safeBrakingLimit) || !AtLeastMinusOne(acceptedRisk) || !AtLeastMinusOne(acceptedGap)
                || !AtLeastMinusOne(maneuverCost))
            { error = "Modulation " + channel + " invalide : chaque gain doit etre fini et superieur ou egal a -1."; return false; }
            if (!float.IsFinite(maxAcceleration) || !(maxAcceleration > -1f)
                || !float.IsFinite(comfortableDeceleration) || !(comfortableDeceleration > -1f))
            { error = "Modulation " + channel + " invalide : les gains de a et b doivent etre finis et strictement superieurs a -1."; return false; }
            error = string.Empty;
            return true;
        }

        private static bool AtLeastMinusOne(float gain) { return float.IsFinite(gain) && gain >= -1f; }
    }

    /// <summary>
    /// Story 5.43 (E4) : modulation authoree d'une personnalite par la rage et la peur, hors de <see cref="DriverProfile"/>
    /// (le V1 ne la lit pas). Elle n'atteint jamais <c>VehicleProfileDef</c> ni aucun parametre physique.
    /// </summary>
    [Serializable]
    public struct EmotionModulation
    {
        [SerializeField, Tooltip("Gains appliques au poids de rage (la jauge, quand la rage gouverne).")]
        private EmotionGains rage;
        [SerializeField, Tooltip("Gains appliques au poids de peur (la jauge, en fuite ou peur saturee).")]
        private EmotionGains fear;

        public EmotionModulation(EmotionGains rage, EmotionGains fear) { this.rage = rage; this.fear = fear; }

        /// <summary>Gains par defaut de la spec 5.43 (Design Notes).</summary>
        public static EmotionModulation Default
        {
            get
            {
                return new EmotionModulation(
                    new EmotionGains(0.3f, -0.5f, -0.5f, 0.5f, 0.3f, 0.5f, 1f, -0.5f, -0.5f),
                    new EmotionGains(0.3f, 0.5f, 0.5f, 0.3f, 0f, 0f, -0.5f, 0.5f, 0.5f));
            }
        }

        public EmotionGains Rage { get { return rage; } }
        public EmotionGains Fear { get { return fear; } }

        public bool TryValidate(out string error)
        {
            return rage.TryValidate("rage", out error) && fear.TryValidate("peur", out error);
        }

        /// <summary>
        /// Facteur continu d'un levier : max(0, 1 + gR.wR + gP.wP). Exactement 1 pour une lecture calme (poids nuls).
        /// </summary>
        public static float Factor(float rageGain, float fearGain, EmotionReading emotion)
        {
            return Mathf.Max(0f, 1f + rageGain * emotion.RageWeight + fearGain * emotion.FearWeight);
        }
    }
}
