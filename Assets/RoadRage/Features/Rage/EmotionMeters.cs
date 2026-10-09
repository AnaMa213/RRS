using RoadRage.Shared.Domain;
using UnityEngine;

namespace RoadRage.Features.Rage
{
    /// <summary>
    /// Story 5.43 : etat hote des jauges d'un vehicule. Les valeurs refletent les NetworkVariables de
    /// <see cref="NetworkedRageState"/> ; le reste (palier, gel, maintien, fuite) est hote seul, jamais replique.
    /// </summary>
    public struct EmotionMeterState
    {
        public float Rage;
        public float Fear;
        /// <summary>Nombre de paliers de rage franchis (seuils de <see cref="RageTuningDef.Thresholds"/>).</summary>
        public int Tier;
        /// <summary>Gel de decroissance de rage restant (s), arme a l'entree d'un palier superieur (E1).</summary>
        public float FreezeSeconds;
        /// <summary>Maintien verrouille arme par une montee au max (E2).</summary>
        public bool RageHold;
        /// <summary>L'evenement Rage Road associe a ete vu actif pendant le maintien.</summary>
        public bool HoldEventSeen;
        /// <summary>Fuite ouverte, tenue jusqu'au seuil de sortie (E3).</summary>
        public bool Escape;
    }

    /// <summary>
    /// Story 5.43 (E1 a E3) : evolution des jauges, fonctions pures sans Netcode. Decroissance, gel de palier, maintien
    /// verrouille a 100 % et arbitrage rage/peur ; tous les seuils viennent de <see cref="RageTuningDef"/>.
    /// </summary>
    public static class EmotionMeters
    {
        /// <summary>
        /// Apres une variation explicite des jauges (delta, effet) : un palier superieur gele la decroissance, une montee
        /// qui atteint le max arme le maintien, une rage sous le max le libere, puis l'arbitrage est reevalue.
        /// </summary>
        public static EmotionMeterState Change(EmotionMeterState previous, float rage, float fear, RageTuningDef tuning)
        {
            if (tuning == null) return previous;
            var next = previous;
            next.Rage = rage;
            next.Fear = fear;
            int tier = TierOf(rage, tuning);
            if (tier > previous.Tier) next.FreezeSeconds = Mathf.Max(0f, tuning.TierFreezeSeconds);
            next.Tier = tier;
            float max = tuning.MaxRageValue;
            if (rage >= max && previous.Rage < max) { next.RageHold = true; next.HoldEventSeen = false; }
            if (rage < max) { next.RageHold = false; next.HoldEventSeen = false; }
            next.Escape = EscapeAfter(next, tuning);
            return next;
        }

        /// <summary>
        /// Avance de <paramref name="deltaTime"/> sans source : le maintien se libere quand l'evenement associe, vu actif,
        /// ne l'est plus ; la rage decroit hors gel et hors maintien, la peur decroit toujours ; plancher 0.
        /// </summary>
        public static EmotionMeterState Advance(EmotionMeterState state, float deltaTime, RageTuningDef tuning,
            bool associatedEventActive)
        {
            if (tuning == null || !float.IsFinite(deltaTime) || !(deltaTime > 0f)) return state;
            var next = state;
            if (next.RageHold)
            {
                if (associatedEventActive) next.HoldEventSeen = true;
                else if (next.HoldEventSeen) { next.RageHold = false; next.HoldEventSeen = false; }
            }
            float decayTime = Mathf.Max(0f, deltaTime - next.FreezeSeconds);
            next.FreezeSeconds = Mathf.Max(0f, next.FreezeSeconds - deltaTime);
            float rage = next.RageHold ? next.Rage : Decay(next.Rage, tuning.RageDecayPercentPerSecond, tuning.MaxRageValue, decayTime);
            float fear = Decay(next.Fear, tuning.FearDecayPercentPerSecond, tuning.MaxFearValue, deltaTime);
            return Change(next, rage, fear, tuning);
        }

        /// <summary>Lecture arbitree (E3) : peur saturee, sinon fuite ouverte, sinon la rage gouverne.</summary>
        public static EmotionReading Read(EmotionMeterState state, RageTuningDef tuning)
        {
            if (tuning == null || !float.IsFinite(state.Rage) || !float.IsFinite(state.Fear)) return EmotionReading.Calm;
            float rage = Percent(state.Rage, tuning.MaxRageValue), fear = Percent(state.Fear, tuning.MaxFearValue);
            var governor = fear >= tuning.FearOverridePercent ? EmotionGovernor.FearSaturated
                : state.Escape ? EmotionGovernor.Escape : EmotionGovernor.Rage;
            return new EmotionReading(rage / 100f, fear / 100f, governor);
        }

        /// <summary>Nombre de seuils franchis, sur des paliers tries par seuil ascendant.</summary>
        public static int TierOf(float rage, RageTuningDef tuning)
        {
            int tier = 0;
            var thresholds = tuning.Thresholds;
            for (int i = 0; i < thresholds.Length; i++)
                if (rage >= thresholds[i].MinValue) tier = i + 1;
            return tier;
        }

        private static bool EscapeAfter(EmotionMeterState state, RageTuningDef tuning)
        {
            float fear = Percent(state.Fear, tuning.MaxFearValue);
            if (state.Escape) return fear > tuning.EscapeExitPercent;
            return fear >= tuning.EscapeEnterPercent && fear > Percent(state.Rage, tuning.MaxRageValue);
        }

        private static float Decay(float value, float percentPerSecond, float max, float seconds)
        {
            if (!(seconds > 0f) || !(percentPerSecond > 0f) || !(max > 0f)) return value;
            return Mathf.Max(0f, value - percentPerSecond * 0.01f * max * seconds);
        }

        private static float Percent(float value, float max)
        {
            // Normaliser en double garde les seuils exacts (30/100 -> 30) et borne les entrees finies enormes.
            return max > 0f && float.IsFinite(value) ? Mathf.Clamp((float)((double)value / max * 100d), 0f, 100f) : 0f;
        }
    }
}
