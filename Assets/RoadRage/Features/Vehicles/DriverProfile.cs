using System;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.9 : personnalite de conduite d'un vehicule IA, exprimee comme une petite structure de
    /// flottants (recherche du 2026-09-15, R2). Les huit premiers parametres sont ceux d'IDM (modele
    /// de poursuite longitudinale) et de MOBIL (decision de changement de voie) ; les trois suivants
    /// sont des leviers de personnalite qui permettent a deux archetypes partageant la meme vitesse
    /// desiree de se sentir differents a conduire ; le dernier (Story 5.13) est la vitesse de rappel de
    /// la cible de visee.
    ///
    /// Unite transportee entre le <see cref="DriverProfileDef"/> (authoring), la modulation par
    /// disposition (<see cref="DriverModel.ResolveEffectiveProfile"/>) et les fonctions pures du
    /// modele. Jamais mutee : toute derivation produit une copie.
    /// </summary>
    [Serializable]
    public struct DriverProfile
    {
        // ------------------------------------------------------------------ IDM

        [SerializeField]
        [Min(0f)]
        [Tooltip("v0 -- vitesse desiree en flux libre (m/s).")]
        private float desiredSpeed;

        [SerializeField]
        [Min(0f)]
        [Tooltip("T -- temps inter-vehiculaire vise (s). Bas = collage au pare-chocs.")]
        private float timeHeadway;

        [SerializeField]
        [Min(0f)]
        [Tooltip("s0 -- ecart minimal conserve a l'arret (m).")]
        private float minimumGap;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("a -- acceleration maximale confortable (m/s2).")]
        private float maxAcceleration;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("b -- deceleration confortable (m/s2).")]
        private float comfortableDeceleration;

        // ------------------------------------------------------------------ MOBIL

        [SerializeField]
        [Tooltip("p -- politesse. 0 = egoiste, > 1 = altruiste, < 0 = prend plaisir a contrarier les autres.")]
        private float politeness;

        [SerializeField]
        [Min(0f)]
        [Tooltip("a_th -- gain minimal (m/s2) exige pour changer de voie. Haut = voies stables, bas = zigzag.")]
        private float laneChangeThreshold;

        [SerializeField]
        [Min(0f)]
        [Tooltip("b_safe -- freinage maximal (m/s2) qu'on accepte d'imposer au vehicule qu'on rabat.")]
        private float safeBrakingLimit;

        // ------------------------------------------------------------------ Personnalite

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Constante de temps (s) du lissage de l'acceleration appliquee vers l'acceleration IDM visee.")]
        private float reactionTime;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Intervalle (s) entre deux evaluations de changement de voie. Remplace tout gating cable en dur.")]
        private float laneChangeEvaluationInterval;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Regularite du conducteur. 1 = parfaitement regulier (aucun bruit), 0 = vitesse desiree la plus flottante.")]
        private float consistency;

        [SerializeField]
        [Min(0f)]
        [Tooltip("AIDE ARCADE (Story 5.13) -- vitesse de RAPPEL (m/s) du point de visee vers sa cible ideale : c'est elle qui rend la visee continue quand le vehicule franchit un noeud. Au-dessus de la vitesse de croisiere authoree (8 m/s), donc elle ne retarde pas la visee en conduite nominale. NUL : rappel inerte, la cible saute de nouveau a l'avancee de noeud.")]
        private float aimPointRecallSpeed;

        /// <summary>
        /// Valeur de rappel de cible utilisee UNIQUEMENT quand un appelant ne la fournit pas : les
        /// fixtures anterieures a la Story 5.13 construisent un profil a onze arguments, et leur
        /// intention ne portait pas sur la visee. L'authoring reel l'ecrit toujours explicitement dans
        /// <see cref="DriverProfileDef"/> -- ce n'est pas un repli de conduite.
        /// </summary>
        public const float DefaultAimPointRecallSpeed = 12f;

        public DriverProfile(
            float desiredSpeed,
            float timeHeadway,
            float minimumGap,
            float maxAcceleration,
            float comfortableDeceleration,
            float politeness,
            float laneChangeThreshold,
            float safeBrakingLimit,
            float reactionTime,
            float laneChangeEvaluationInterval,
            float consistency,
            float aimPointRecallSpeed = DefaultAimPointRecallSpeed)
        {
            this.desiredSpeed = desiredSpeed;
            this.timeHeadway = timeHeadway;
            this.minimumGap = minimumGap;
            this.maxAcceleration = maxAcceleration;
            this.comfortableDeceleration = comfortableDeceleration;
            this.politeness = politeness;
            this.laneChangeThreshold = laneChangeThreshold;
            this.safeBrakingLimit = safeBrakingLimit;
            this.reactionTime = reactionTime;
            this.laneChangeEvaluationInterval = laneChangeEvaluationInterval;
            this.consistency = consistency;
            this.aimPointRecallSpeed = aimPointRecallSpeed;
        }

        /// <summary>v0 -- vitesse desiree en flux libre (m/s). 0 signifie "cesse de poursuivre la route".</summary>
        public float DesiredSpeed
        {
            get { return desiredSpeed; }
        }

        /// <summary>T -- temps inter-vehiculaire vise (s).</summary>
        public float TimeHeadway
        {
            get { return timeHeadway; }
        }

        /// <summary>s0 -- ecart minimal a l'arret (m).</summary>
        public float MinimumGap
        {
            get { return minimumGap; }
        }

        /// <summary>a -- acceleration maximale (m/s2).</summary>
        public float MaxAcceleration
        {
            get { return maxAcceleration; }
        }

        /// <summary>b -- deceleration confortable (m/s2).</summary>
        public float ComfortableDeceleration
        {
            get { return comfortableDeceleration; }
        }

        /// <summary>p -- politesse MOBIL (peut etre negative : malveillance deliberee).</summary>
        public float Politeness
        {
            get { return politeness; }
        }

        /// <summary>a_th -- seuil de gain exige pour changer de voie (m/s2).</summary>
        public float LaneChangeThreshold
        {
            get { return laneChangeThreshold; }
        }

        /// <summary>b_safe -- freinage impose acceptable au nouveau suiveur (m/s2).</summary>
        public float SafeBrakingLimit
        {
            get { return safeBrakingLimit; }
        }

        /// <summary>Constante de temps du lissage de reaction (s).</summary>
        public float ReactionTime
        {
            get { return reactionTime; }
        }

        /// <summary>Intervalle authore entre deux evaluations de changement de voie (s).</summary>
        public float LaneChangeEvaluationInterval
        {
            get { return laneChangeEvaluationInterval; }
        }

        /// <summary>Regularite du conducteur, 0..1. 1 = aucun bruit de vitesse desiree.</summary>
        public float Consistency
        {
            get { return consistency; }
        }

        /// <summary>Vitesse de rappel (m/s) du point de visee vers sa cible ideale. Nulle : rappel inerte (Story 5.13).</summary>
        public float AimPointRecallSpeed
        {
            get { return aimPointRecallSpeed; }
        }

        /// <summary>
        /// Copie du profil avec une autre vitesse desiree. Support de l'ordre de composition de la
        /// spec : disposition (ResolveEffectiveProfile) puis bruit de personnalite
        /// (ResolveNoisyDesiredSpeed), juste avant ComputeAcceleration.
        /// </summary>
        public DriverProfile WithDesiredSpeed(float newDesiredSpeed)
        {
            var copy = this;
            copy.desiredSpeed = newDesiredSpeed;
            return copy;
        }
    }
}
