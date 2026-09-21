using RoadRage.Shared.Definitions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.10 (AD-32) : donnee auteur du trafic route. Vit comme asset sous
    /// Assets/RoadRage/ScriptableObjects/Vehicles/ et porte un id globalement unique en minuscules,
    /// stable, sur le meme gabarit que <see cref="DriverProfileDef"/>. Jamais mute a l'execution.
    ///
    /// C'est le SEUL endroit ou un effectif de trafic est ecrit : aucun controleur, aucun prefab,
    /// aucune scene ne porte de valeur litterale. Si ce Def n'est pas assigne, aucun vehicule n'est
    /// insere -- il n'existe aucun repli code en dur.
    ///
    /// Pas de catalogue associe : aucun appelant ne fait de lookup par id.
    /// </summary>
    [CreateAssetMenu(fileName = "TrafficSettingsDef", menuName = "RoadRage/Vehicles/Traffic Settings Def")]
    public sealed class TrafficSettingsDef : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Id globalement unique en minuscules, ex. traffic_default. Stable : ne jamais le renommer une fois publie.")]
        private string id = string.Empty;

        [SerializeField]
        [Min(0)]
        [Tooltip("Effectif cible par defaut du district. Valeur utilisee tant qu'aucun effectif de session n'existe (Story 5.16).")]
        private int defaultTargetPopulation = 8;

        [SerializeField]
        [Min(0)]
        [Tooltip("Borne basse de l'effectif cible : toute valeur de session sera ramenee dans [min, max].")]
        private int minTargetPopulation;

        [SerializeField]
        [Min(0)]
        [Tooltip("Borne haute de l'effectif cible.")]
        private int maxTargetPopulation = 30;

        [SerializeField]
        [Min(0)]
        [Tooltip("Nombre de jeteurs de detritus par defaut (Story 5.16). Valeur utilisee tant qu'aucune valeur de session n'existe. Aucun systeme de detritus n'est livre ici : la Story 5.20 consommera la valeur resolue.")]
        private int defaultLitterThrowers = 2;

        [SerializeField]
        [Min(0)]
        [Tooltip("Borne basse du nombre de jeteurs. 0 est legal : un run sans jeteur est un reglage valide.")]
        private int minLitterThrowers;

        [SerializeField]
        [Min(0)]
        [Tooltip("Borne haute du nombre de jeteurs. Le nombre de jeteurs ne depasse jamais l'effectif cible de la session.")]
        private int maxLitterThrowers = 8;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Budget d'aretes, en multiples du nombre de noeuds du graphe (--max-edges-factor de jtrrouter, defaut 2). Au-dela, le parcours est reoriente vers la sortie la plus proche -- jamais retire.")]
        private float edgeBudgetFactor = 2f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Rayon autour d'un portail d'entree en dessous duquel la presence d'un vehicule bloque l'insertion. Le vehicule reste en file et retente au pas suivant.")]
        private float portalClearanceRadius = 12f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Distance en dessous de laquelle deux connecteurs de modules voisins forment une arete. Poser deux modules bout a bout suffit.")]
        private float connectorJoinDistance = 0.75f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Largeur du couloir a laisser libre entre deux mouvements d'une meme jonction : somme des deux demi-largeurs de vehicule plus la marge de securite. C'est elle qui recule la ligne d'arret hors de l'aire de virage. Donnee de MONDE : deux conducteurs qui partagent une jonction doivent en lire la meme valeur.")]
        private float junctionMovementClearance = 2.36f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Rayon autour d'un noeud de jonction dans lequel une approche est revendiquee. Donnee de MONDE : l'ensemble arbitre doit etre le meme pour tous les observateurs d'une jonction, sinon deux conducteurs de profils differents arbitrent deux ensembles differents et peuvent se croire prioritaires tous les deux. 0 rend la main au profil de conduite.")]
        private float junctionApproachRadius;

        [SerializeField]
        [Tooltip("Plans de feux authorés (Story 5.18), un par jonction signalee. Un plan absent, ou un id de jonction inconnu, laisse le comportement authore precedent : aucune regle de feu n'est inventee.")]
        private TrafficSignalPlan[] signalPlans = Array.Empty<TrafficSignalPlan>();

        /// <summary>Id stable expose sous la forme partagee attendue par les autres couches.</summary>
        public DefinitionId Id
        {
            get { return new DefinitionId(id); }
        }

        /// <summary>Valeur brute de l'id, exposee pour les gardes de validation.</summary>
        public string RawId
        {
            get { return id ?? string.Empty; }
        }

        /// <summary>Effectif cible authore, deja ramene dans ses propres bornes.</summary>
        public int DefaultTargetPopulation
        {
            get { return ClampTargetPopulation(defaultTargetPopulation); }
        }

        public int MinTargetPopulation
        {
            get { return minTargetPopulation; }
        }

        public int MaxTargetPopulation
        {
            get { return maxTargetPopulation; }
        }

        /// <summary>Nombre de jeteurs authore, deja ramene dans ses propres bornes (Story 5.16).</summary>
        public int DefaultLitterThrowers
        {
            get { return ClampLitterThrowers(defaultLitterThrowers); }
        }

        public int MinLitterThrowers
        {
            get { return minLitterThrowers; }
        }

        public int MaxLitterThrowers
        {
            get { return maxLitterThrowers; }
        }

        public float EdgeBudgetFactor
        {
            get { return edgeBudgetFactor; }
        }

        public float PortalClearanceRadius
        {
            get { return portalClearanceRadius; }
        }

        public float ConnectorJoinDistance
        {
            get { return connectorJoinDistance; }
        }

        /// <summary>
        /// Largeur du couloir a laisser libre entre deux mouvements d'une meme jonction.
        ///
        /// La frontiere de conflit d'une approche etait calculee sur l'INTERSECTION STRICTE de deux
        /// axes centraux, c'est-a-dire sur deux polylignes sans epaisseur. Un vehicule arrete a la
        /// ligne qui en decoule a pourtant une largeur : mesure du district, son avant se trouvait
        /// 2,34 m a l'interieur du couloir que le trafic tournant doit emprunter. La valeur est la
        /// somme des deux demi-largeurs de vehicule et de la marge de securite (2 x 1,03 + 0,30).
        /// </summary>
        public float JunctionMovementClearance
        {
            get { return Mathf.Max(0f, junctionMovementClearance); }
        }

        /// <summary>
        /// Rayon d'approche d'une jonction, en metres, ou 0 quand la scene n'en authore pas -- le
        /// profil de conduite reprend alors la main.
        ///
        /// C'est une donnee de MONDE et non de conducteur, et la raison est la correction de
        /// l'arbitrage, pas le rangement. L'admission (<see cref="JunctionRules.IsAdmitted"/>) se
        /// calcule sur l'ENSEMBLE des revendications : deux observateurs qui ne rassemblent pas le
        /// meme ensemble n'obtiennent pas le meme resultat. Lire ce rayon sur le profil de
        /// l'OBSERVATEUR faisait donc dependre la geometrie de l'ensemble de la personnalite de
        /// celui qui regarde -- latent tant qu'un seul profil est authore, faux des le second.
        /// (Review Finding #6.)
        /// </summary>
        public float JunctionApproachRadius
        {
            get { return Mathf.Max(0f, junctionApproachRadius); }
        }

        /// <summary>
        /// Plans de feux authores (Story 5.18). Les feux sont de la donnee de MONDE : ils vivent ici,
        /// pas dans un profil de conduite, parce que deux conducteurs qui partagent une jonction
        /// partagent aussi son plan.
        /// </summary>
        public IReadOnlyList<TrafficSignalPlan> SignalPlans
        {
            get { return signalPlans ?? Array.Empty<TrafficSignalPlan>(); }
        }

        /// <summary>
        /// Plan de feux de la jonction donnee, par id authore. Faux quand aucun plan ne porte cet id :
        /// une approche marquee "feu" sans plan garde alors le comportement authore precedent, elle
        /// n'attend pas un feu qui n'existe pas.
        /// </summary>
        public bool TryGetSignalPlan(string junctionId, out TrafficSignalPlan plan)
        {
            var key = junctionId ?? string.Empty;
            var plans = SignalPlans;
            for (var i = 0; i < plans.Count; i++)
            {
                if (string.Equals(plans[i].JunctionId, key, StringComparison.Ordinal))
                {
                    plan = plans[i];
                    return true;
                }
            }

            plan = default;
            return false;
        }

        /// <summary>Ramene un effectif (authore ou de session) dans les bornes authorees.</summary>
        public int ClampTargetPopulation(int population)
        {
            var low = Mathf.Max(0, minTargetPopulation);
            var high = Mathf.Max(low, maxTargetPopulation);
            return Mathf.Clamp(population, low, high);
        }

        /// <summary>
        /// Story 5.16 : ramene un nombre de jeteurs (authore ou de session) dans les bornes authorees.
        /// L'invariant "jeteurs &lt;= effectif cible" n'est PAS applique ici : il depend de l'effectif
        /// courant de la session, que ce Def ne connait pas. Son porteur est la valeur de session.
        /// </summary>
        public int ClampLitterThrowers(int litterThrowers)
        {
            var low = Mathf.Max(0, minLitterThrowers);
            var high = Mathf.Max(low, maxLitterThrowers);
            return Mathf.Clamp(litterThrowers, low, high);
        }

        public bool TryValidate(out string error)
        {
            if (string.IsNullOrWhiteSpace(RawId) || RawId != RawId.Trim() || RawId != RawId.ToLowerInvariant())
            {
                error = "Id de reglages de trafic invalide.";
                return false;
            }

            if (minTargetPopulation < 0)
            {
                error = "MinTargetPopulation invalide : 'minTargetPopulation' doit etre superieur ou egal a 0.";
                return false;
            }

            if (maxTargetPopulation < minTargetPopulation)
            {
                error = "MaxTargetPopulation invalide : 'maxTargetPopulation' doit etre superieur ou egal a 'minTargetPopulation'.";
                return false;
            }

            if (defaultTargetPopulation < minTargetPopulation || defaultTargetPopulation > maxTargetPopulation)
            {
                error = "DefaultTargetPopulation invalide : 'defaultTargetPopulation' doit tomber dans [min, max].";
                return false;
            }

            if (minLitterThrowers < 0)
            {
                error = "MinLitterThrowers invalide : 'minLitterThrowers' doit etre superieur ou egal a 0.";
                return false;
            }

            if (maxLitterThrowers < minLitterThrowers)
            {
                error = "MaxLitterThrowers invalide : 'maxLitterThrowers' doit etre superieur ou egal a 'minLitterThrowers'.";
                return false;
            }

            if (defaultLitterThrowers < minLitterThrowers || defaultLitterThrowers > maxLitterThrowers)
            {
                error = "DefaultLitterThrowers invalide : 'defaultLitterThrowers' doit tomber dans [min, max].";
                return false;
            }

            // Story 5.16 : l'invariant "jeteurs <= effectif" est une regle de SESSION, que ce Def ne peut
            // pas appliquer. En revanche il peut refuser une authoring qui rend ce defaut inatteignable :
            // sans ces deux gardes, une session amorcee sur ses propres defauts demarrerait au-dessus de
            // l'invariant (jeteurs > effectif) ou sous la borne basse des jeteurs, et les deux boutons de
            // la ligne seraient morts des l'ouverture du lobby.
            if (defaultLitterThrowers > defaultTargetPopulation)
            {
                error = "DefaultLitterThrowers invalide : le defaut de jeteurs ne peut pas depasser l'effectif cible par defaut.";
                return false;
            }

            if (minLitterThrowers > minTargetPopulation)
            {
                error = "MinLitterThrowers invalide : la borne basse des jeteurs ne peut pas depasser la borne basse de l'effectif, sinon aucun reglage de session ne satisfait les deux.";
                return false;
            }

            if (!IsFiniteAndAbove(edgeBudgetFactor, 0f))
            {
                error = "EdgeBudgetFactor invalide : 'edgeBudgetFactor' doit etre fini et strictement positif.";
                return false;
            }

            if (!IsFiniteAndAbove(portalClearanceRadius, 0f))
            {
                error = "PortalClearanceRadius invalide : 'portalClearanceRadius' doit etre fini et strictement positif.";
                return false;
            }

            if (!IsFiniteAndAbove(connectorJoinDistance, 0f))
            {
                error = "ConnectorJoinDistance invalide : 'connectorJoinDistance' doit etre fini et strictement positif.";
                return false;
            }

            if (!TryValidateSignalPlans(out error))
            {
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Story 5.18 : un plan de feux inexploitable est refuse a l'authoring, jamais repare a
        /// l'execution. Trois choses le rendent inexploitable, et chacune se traduirait par un feu qui
        /// ne signifie rien au lieu d'une erreur visible :
        ///
        /// - un plan sans id ne s'applique a aucune jonction ;
        /// - un plan a une seule phase n'arbitre rien (tout le monde est vert en permanence) ;
        /// - une phase plus courte que la garde de vert minimal fait clignoter le feu, donc osciller
        ///   l'autorisation d'approche a chaque cycle.
        ///
        /// Deux plans qui partagent le meme id sont egalement refuses : l'un des deux serait ignore a
        /// la lecture, et c'est le genre de reglage qui ne se voit qu'au volant.
        /// </summary>
        private bool TryValidateSignalPlans(out string error)
        {
            var plans = SignalPlans;
            for (var i = 0; i < plans.Count; i++)
            {
                var plan = plans[i];
                var id = plan.JunctionId;
                if (string.IsNullOrWhiteSpace(id) || id != id.Trim())
                {
                    error = "Plan de feux invalide : 'junctionId' doit etre renseigne et sans espace de bord.";
                    return false;
                }

                for (var j = i + 1; j < plans.Count; j++)
                {
                    if (string.Equals(plans[j].JunctionId, id, StringComparison.Ordinal))
                    {
                        error = "Plans de feux invalides : 'junctionId' \"" + id + "\" est porte par deux plans, le second serait ignore.";
                        return false;
                    }
                }

                if (!IsFiniteAndAbove(plan.MinimumGreenSeconds, 0f))
                {
                    error = "Plan de feux invalide : 'minimumGreenSeconds' doit etre fini et strictement positif.";
                    return false;
                }

                var phases = plan.Phases;
                if (phases.Count < 2)
                {
                    error = "Plan de feux invalide : un plan a moins de deux phases n'arbitre rien.";
                    return false;
                }

                for (var p = 0; p < phases.Count; p++)
                {
                    if (!IsFiniteAndAtLeast(phases[p].DurationSeconds, plan.MinimumGreenSeconds))
                    {
                        error = "Plan de feux invalide : une phase duree " + phases[p].DurationSeconds
                            + " s est sous la garde de vert minimal de " + plan.MinimumGreenSeconds + " s.";
                        return false;
                    }

                    for (var g = 0; g < phases[p].GreenGroups.Count; g++)
                    {
                        if (phases[p].GreenGroups[g] < 0)
                        {
                            error = "Plan de feux invalide : un groupe d'approche negatif n'existe pas.";
                            return false;
                        }
                    }
                }
            }

            error = string.Empty;
            return true;
        }

        private static bool IsFiniteAndAbove(float value, float exclusiveMinimum)
        {
            return float.IsFinite(value) && value > exclusiveMinimum;
        }

        /// <summary>Variante inclusive, pour la garde de vert minimal : une phase peut valoir exactement la garde.</summary>
        private static bool IsFiniteAndAtLeast(float value, float minimum)
        {
            return float.IsFinite(value) && value >= minimum;
        }

        private void OnValidate()
        {
            if (!TryValidate(out var error))
            {
                Debug.LogWarning("[Vehicles] TrafficSettingsDef invalide : " + error, this);
            }
        }
    }
}
