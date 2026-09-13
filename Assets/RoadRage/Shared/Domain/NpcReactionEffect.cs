using System;
using UnityEngine;

namespace RoadRage.Shared.Domain
{
    /// <summary>
    /// Canal vise par un <see cref="NpcReactionEffect"/> (Story 5.1). Le canal est explicite pour
    /// qu'un effet qui n'influence rien reste distinguable d'un effet d'influence nulle (Design Notes).
    /// </summary>
    [Flags]
    public enum ReactionChannel
    {
        None = 0,
        Rage = 1,
        Fear = 2,
        Both = Rage | Fear
    }

    /// <summary>
    /// Effet de reaction applicable a une cible (Story 5.1) : une magnitude signee, modulee par les
    /// sensibilites authored du tuning, appliquee aux canaux vises. Vit dans Shared.Domain parce que
    /// Features.Vehicles doit pouvoir le referencer sans dependre de Features.Rage (garde
    /// RoadRageScaffoldTests). Donnee de session construite a l'execution, jamais un asset.
    /// </summary>
    [Serializable]
    public struct NpcReactionEffect
    {
        [SerializeField]
        private ReactionChannel channel;

        [SerializeField]
        private float magnitude;

        public NpcReactionEffect(ReactionChannel channel, float magnitude)
        {
            this.channel = channel;
            this.magnitude = magnitude;
        }

        public ReactionChannel Channel
        {
            get { return channel; }
        }

        public float Magnitude
        {
            get { return magnitude; }
        }

        /// <summary>Vrai si l'effet vise le canal rage.</summary>
        public bool AffectsRage
        {
            get { return (channel & ReactionChannel.Rage) != 0; }
        }

        /// <summary>Vrai si l'effet vise le canal peur.</summary>
        public bool AffectsFear
        {
            get { return (channel & ReactionChannel.Fear) != 0; }
        }

        /// <summary>
        /// Contrat des effets construits a l'execution, meme convention que RageTuningDef.TryValidate
        /// (messages sans accents nommant le champ fautif). Un canal None ou une magnitude nulle
        /// restent valides : ils produisent un no-op a l'application, pas un refus d'authoring.
        /// </summary>
        public bool TryValidate(out string error)
        {
            if (((int)channel & ~(int)ReactionChannel.Both) != 0)
            {
                error = "Channel invalide : 'channel' doit se limiter a Rage, Fear ou Both.";
                return false;
            }

            if (!float.IsFinite(magnitude))
            {
                error = "Magnitude invalide : 'magnitude' doit etre finie.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
