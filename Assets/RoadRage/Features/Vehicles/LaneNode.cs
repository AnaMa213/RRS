using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>Role d'authoring d'un <see cref="LaneNode"/> dans le graphe de voies (Story 5.10).</summary>
    public enum LaneNodeRole
    {
        /// <summary>Noeud de voie ordinaire : on y passe, on n'y nait ni n'y meurt.</summary>
        Normal = 0,

        /// <summary>Portail d'entree : le seul endroit ou un vehicule peut etre insere.</summary>
        PortalEntry = 1,

        /// <summary>Portail de sortie : le seul endroit ou un vehicule peut etre retire.</summary>
        PortalExit = 2,

        /// <summary>
        /// Jointure de module : une arete est creee automatiquement vers le connecteur voisin quand
        /// deux modules sont poses bout a bout, sans cablage manuel entre instances de prefab.
        /// </summary>
        Connector = 3
    }

    /// <summary>
    /// Unite d'authoring du graphe de voies (Story 5.10) : un GameObject pose sous la racine gameplay
    /// d'un module greybox -- jamais sous son enfant <c>Visual_*</c> (AD-27), pour que le passage a
    /// l'art final ne deplace pas le graphe. Sa position est le repere vise, son <c>forward</c> le sens
    /// de circulation.
    ///
    /// Convention de connecteur, celle qui rend la jointure automatique sans ambiguite de sens :
    /// un connecteur SANS successeur authore est une SORTIE de module (fin de voie) ; un connecteur
    /// AVEC au moins un successeur est une ENTREE de module (debut de voie). <see cref="LaneGraph"/>
    /// relie une sortie a l'entree voisine la plus proche sous le seuil authore, jamais l'inverse :
    /// poser deux modules bout a bout suffit.
    ///
    /// Composant d'authoring pur, sans etat reseau : le runtime ne le lit jamais directement, il
    /// n'appelle que <see cref="LaneGraph"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LaneNode : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Role du noeud. PortalEntry et PortalExit sont les SEULS points d'apparition et de disparition d'un vehicule (AD-34).")]
        private LaneNodeRole role = LaneNodeRole.Normal;

        [SerializeField]
        [Tooltip("Successeurs authores dans CE module. Les jointures entre modules sont resolues automatiquement par LaneGraph via les connecteurs.")]
        private LaneNode[] successors = Array.Empty<LaneNode>();

        [SerializeField]
        [Tooltip("Ratios de virage paralleles a 'successors' (modele jtrrouter, ex. 30/50/20 pour droite/tout droit/gauche). Tous nuls ou negatifs : premier successeur retenu et avertissement une fois.")]
        private float[] turnWeights = Array.Empty<float>();

        [SerializeField]
        [Tooltip("Portail d'entree qui accepte aussi les vehicules sortants : le meme tunnel sert d'entree et de sortie.")]
        private bool exitReusesEntry;

        [SerializeField]
        [Tooltip("Id de jonction, stable et identique sur toutes les approches d'une meme jonction. VIDE : ce noeud n'est pas une approche, aucune regle ne s'y applique (Story 5.18).")]
        private string junctionId = string.Empty;

        [SerializeField]
        [Tooltip("Regle de priorite authoree sur cette approche (Story 5.18). Priorite a droite est le repli quand aucune route prioritaire n'est authoree.")]
        private JunctionApproachRule junctionRule = JunctionApproachRule.PriorityToRight;

        [SerializeField]
        [Min(0)]
        [Tooltip("Groupe d'approche dans le plan de feux de la jonction (Story 5.18). Deux approches qui se croisent ne doivent jamais partager un groupe.")]
        private int signalGroup;

        public LaneNodeRole Role
        {
            get { return role; }
        }

        /// <summary>
        /// Id de jonction authore (Story 5.18), ou chaine vide. Il n'identifie PAS une jonction a lui
        /// seul : le graphe le porte par (module porteur, cet id), pour que quatre instances du meme
        /// prefab de jonction en T restent quatre jonctions distinctes. Vide veut dire "ce noeud n'est
        /// pas une approche" -- et aucune regle n'est alors inventee.
        /// </summary>
        public string JunctionId
        {
            get { return junctionId ?? string.Empty; }
        }

        /// <summary>Regle de priorite authoree sur cette approche. Sans objet quand <see cref="JunctionId"/> est vide.</summary>
        public JunctionApproachRule JunctionRule
        {
            get { return JunctionRules.NormalizeRule(junctionRule); }
        }

        /// <summary>Groupe d'approche dans le plan de feux. Sans objet hors regle feu.</summary>
        public int SignalGroup
        {
            get { return signalGroup; }
        }

        /// <summary>Vrai si ce noeud porte une approche de jonction, donc si l'arbitrage le concerne.</summary>
        public bool IsJunctionApproach
        {
            get { return !string.IsNullOrWhiteSpace(junctionId); }
        }

        public IReadOnlyList<LaneNode> Successors
        {
            get { return successors ?? Array.Empty<LaneNode>(); }
        }

        public IReadOnlyList<float> TurnWeights
        {
            get { return turnWeights ?? Array.Empty<float>(); }
        }

        /// <summary>Vrai si ce noeud accepte les vehicules sortants : PortalExit, ou PortalEntry marque reutilisable en sortie.</summary>
        public bool IsExitPortal
        {
            get { return role == LaneNodeRole.PortalExit || (role == LaneNodeRole.PortalEntry && exitReusesEntry); }
        }

        /// <summary>Vrai si ce noeud accepte l'insertion d'un vehicule.</summary>
        public bool IsEntryPortal
        {
            get { return role == LaneNodeRole.PortalEntry; }
        }

        /// <summary>Connecteur de fin de voie : c'est lui qui recoit une arete vers le module voisin.</summary>
        public bool IsOutgoingConnector
        {
            get { return role == LaneNodeRole.Connector && CountLiveSuccessors() == 0; }
        }

        /// <summary>Connecteur de debut de voie : cible possible d'une jointure venue du module voisin.</summary>
        public bool IsIncomingConnector
        {
            get { return role == LaneNodeRole.Connector && CountLiveSuccessors() > 0; }
        }

        private int CountLiveSuccessors()
        {
            if (successors == null)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < successors.Length; i++)
            {
                if (successors[i] != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
