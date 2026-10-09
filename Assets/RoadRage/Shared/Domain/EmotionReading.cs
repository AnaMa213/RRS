namespace RoadRage.Shared.Domain
{
    /// <summary>Story 5.43 (E3) : canal qui gouverne la conduite apres arbitrage hote rage/peur.</summary>
    public enum EmotionGovernor
    {
        /// <summary>La rage gouverne (y compris a rage nulle : calme).</summary>
        Rage = 0,
        /// <summary>Fuite : peur au seuil d'entree et superieure a la rage, tenue jusqu'au seuil de sortie.</summary>
        Escape = 1,
        /// <summary>Peur au seuil de saturation : l'emporte sur toute rage.</summary>
        FearSaturated = 2
    }

    /// <summary>
    /// Story 5.43 (E4, E5) : lecture etroite des jauges d'un vehicule, normalisees dans [0, 1], et du canal gouvernant.
    /// Valeur pure : une composante non finie rend la lecture <see cref="Calm"/> entiere.
    /// </summary>
    public readonly struct EmotionReading
    {
        public readonly float Rage01;
        public readonly float Fear01;
        public readonly EmotionGovernor Governor;

        public EmotionReading(float rage01, float fear01, EmotionGovernor governor)
        {
            bool known = governor == EmotionGovernor.Rage || governor == EmotionGovernor.Escape
                || governor == EmotionGovernor.FearSaturated;
            if (!float.IsFinite(rage01) || !float.IsFinite(fear01) || !known)
            {
                Rage01 = 0f; Fear01 = 0f; Governor = EmotionGovernor.Rage;
                return;
            }
            Rage01 = Clamp01(rage01); Fear01 = Clamp01(fear01); Governor = governor;
        }

        /// <summary>Aucune emotion : poids nuls, politique authoree inchangee.</summary>
        public static EmotionReading Calm { get { return default(EmotionReading); } }

        /// <summary>Poids de rage de la modulation : la jauge si la rage gouverne, sinon 0.</summary>
        public float RageWeight { get { return Governor == EmotionGovernor.Rage ? Rage01 : 0f; } }

        /// <summary>Poids de peur de la modulation : la jauge si la peur gouverne, sinon 0.</summary>
        public float FearWeight { get { return Governor == EmotionGovernor.Rage ? 0f : Fear01; } }

        private static float Clamp01(float value) { return value < 0f ? 0f : value > 1f ? 1f : value; }
    }

    /// <summary>
    /// Story 5.43 : lecture hote de l'emotion d'un vehicule, sur le patron de <see cref="IRageDispositionSource"/>.
    /// Features/Vehicles la lit sans dependre de Features/Rage et sans jamais muter les jauges.
    /// </summary>
    public interface IEmotionSource
    {
        /// <summary>Lecture hote courante ; <see cref="EmotionReading.Calm"/> tant qu'aucune jauge n'a ete appliquee.</summary>
        EmotionReading CurrentEmotion { get; }
    }
}
