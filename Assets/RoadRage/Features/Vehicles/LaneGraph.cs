using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.10 : racine du graphe de voies authore en scene. Elle collecte les <see cref="LaneNode"/>
    /// poses sous elle (directement ou via des instances de modules greybox), relie automatiquement
    /// les connecteurs de modules voisins, et expose la SEULE surface que le runtime connait --
    /// <see cref="NetworkedAIVehicleDriverController"/> et le spawner de trafic n'appellent qu'elle,
    /// jamais la hierarchie ni les noms. Changer la forme d'authoring plus tard (outil Editor, asset,
    /// ADDON-008) ne touchera donc ni le driver ni le spawner.
    ///
    /// Un trace routier est de la donnee de scene : ce qui se reutilise d'une ville a l'autre est le
    /// code (resolution, tirage, budget, insertion), pas le trace. Le motif est celui deja en place
    /// dans le projet -- composant racine + enfants de hierarchie.
    ///
    /// L'index de noeud expose ici est l'ordre de collecte de la hierarchie : identique sur tous les
    /// pairs puisque la scene l'est, donc directement utilisable comme
    /// <c>NetworkedAIVehicleState.WaypointIndex</c> sans NetworkVariable supplementaire.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LaneGraph : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reglages de trafic authores (AD-32). Non assigne : aucun vehicule n'est insere et les connecteurs ne sont pas joints -- il n'existe aucun repli code en dur.")]
        private TrafficSettingsDef trafficSettings;

        private readonly List<LaneNode> nodes = new List<LaneNode>();
        private readonly List<List<int>> successors = new List<List<int>>();
        private readonly List<List<float>> turnWeights = new List<List<float>>();
        private readonly List<int> entryPortals = new List<int>();
        private readonly List<int> exitPortals = new List<int>();
        private readonly List<int> orphanConnectors = new List<int>();
        private readonly HashSet<int> warnedInvalidWeightNodes = new HashSet<int>();

        private bool built;

        /// <summary>Reglages de trafic authores, ou null. Point d'acces unique : le spawner ne cable pas son propre Def.</summary>
        public TrafficSettingsDef TrafficSettings
        {
            get { return trafficSettings; }
        }

        public int NodeCount
        {
            get
            {
                EnsureBuilt();
                return nodes.Count;
            }
        }

        /// <summary>Indices des portails d'entree : les seuls points d'insertion autorises.</summary>
        public IReadOnlyList<int> EntryPortals
        {
            get
            {
                EnsureBuilt();
                return entryPortals;
            }
        }

        /// <summary>Indices des portails de sortie : les seuls points de retrait autorises.</summary>
        public IReadOnlyList<int> ExitPortals
        {
            get
            {
                EnsureBuilt();
                return exitPortals;
            }
        }

        /// <summary>Connecteurs de fin de voie restes sans voisin : signale en gizmo et a la validation.</summary>
        public IReadOnlyList<int> OrphanConnectors
        {
            get
            {
                EnsureBuilt();
                return orphanConnectors;
            }
        }

        private void Awake()
        {
            EnsureBuilt();
        }

        /// <summary>Reconstruit la topologie : a appeler apres une modification de la hierarchie a l'execution.</summary>
        public void Rebuild()
        {
            built = false;
            EnsureBuilt();
        }

        public bool IsValidIndex(int index)
        {
            EnsureBuilt();
            return index >= 0 && index < nodes.Count;
        }

        public Vector3 GetNodePosition(int index)
        {
            EnsureBuilt();
            return IsValidIndex(index) ? nodes[index].transform.position : transform.position;
        }

        public Quaternion GetNodeRotation(int index)
        {
            EnsureBuilt();
            return IsValidIndex(index) ? nodes[index].transform.rotation : transform.rotation;
        }

        public IReadOnlyList<int> GetSuccessors(int index)
        {
            EnsureBuilt();
            return IsValidIndex(index) ? successors[index] : (IReadOnlyList<int>)System.Array.Empty<int>();
        }

        public IReadOnlyList<float> GetTurnWeights(int index)
        {
            EnsureBuilt();
            return IsValidIndex(index) ? turnWeights[index] : (IReadOnlyList<float>)System.Array.Empty<float>();
        }

        public bool IsExitPortal(int index)
        {
            EnsureBuilt();
            return IsValidIndex(index) && nodes[index].IsExitPortal;
        }

        /// <summary>Noeud le plus proche en distance planaire : point de depart d'un vehicule pose n'importe ou.</summary>
        public int NearestNodeIndex(Vector3 position)
        {
            EnsureBuilt();
            if (nodes.Count == 0)
            {
                return -1;
            }

            var best = -1;
            var bestSquared = float.PositiveInfinity;
            for (var i = 0; i < nodes.Count; i++)
            {
                var offset = nodes[i].transform.position - position;
                offset.y = 0f;
                if (offset.sqrMagnitude < bestSquared)
                {
                    bestSquared = offset.sqrMagnitude;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Portail de sortie le plus proche, ou -1 si le graphe n'en porte aucun (vehicule inerte, jamais retire).</summary>
        public int NearestExitNodeIndex(Vector3 position)
        {
            EnsureBuilt();
            if (exitPortals.Count == 0)
            {
                return -1;
            }

            var best = -1;
            var bestSquared = float.PositiveInfinity;
            for (var i = 0; i < exitPortals.Count; i++)
            {
                var offset = nodes[exitPortals[i]].transform.position - position;
                offset.y = 0f;
                if (offset.sqrMagnitude < bestSquared)
                {
                    bestSquared = offset.sqrMagnitude;
                    best = exitPortals[i];
                }
            }

            return best;
        }

        /// <summary>
        /// Avertissement de poids invalides emis UNE FOIS PAR NOEUD, pas une fois par vehicule : le
        /// defaut est dans l'authoring du noeud, pas dans le vehicule qui le traverse.
        /// </summary>
        public void ReportInvalidTurnWeights(int index)
        {
            EnsureBuilt();
            if (!IsValidIndex(index) || !warnedInvalidWeightNodes.Add(index))
            {
                return;
            }

            Debug.LogWarning(
                "[Vehicles] LaneGraph : poids de virage absents ou de somme nulle sur '" + nodes[index].name
                + "', premier successeur retenu.", nodes[index]);
        }

        /// <summary>Etat d'authoring lisible : ce que la validation de scene et le gizmo signalent.</summary>
        public bool TryValidate(out string error)
        {
            EnsureBuilt();

            if (trafficSettings == null)
            {
                error = "Aucun TrafficSettingsDef assigne : aucun vehicule ne sera insere.";
                return false;
            }

            if (nodes.Count == 0)
            {
                error = "Aucun LaneNode sous la racine du graphe.";
                return false;
            }

            if (entryPortals.Count == 0)
            {
                error = "Aucun portail d'entree : aucun vehicule ne peut entrer dans le district.";
                return false;
            }

            if (exitPortals.Count == 0)
            {
                error = "Aucun portail de sortie : aucun vehicule ne pourrait jamais quitter le district.";
                return false;
            }

            if (orphanConnectors.Count > 0)
            {
                error = orphanConnectors.Count + " connecteur(s) orphelin(s) : un module est pose hors du seuil de jointure.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void EnsureBuilt()
        {
            if (built)
            {
                return;
            }

            built = true;
            nodes.Clear();
            successors.Clear();
            turnWeights.Clear();
            entryPortals.Clear();
            exitPortals.Clear();
            orphanConnectors.Clear();
            warnedInvalidWeightNodes.Clear();

            // Ordre de hierarchie : deterministe et identique sur tous les pairs, donc utilisable tel
            // quel comme index replique.
            GetComponentsInChildren(true, nodes);

            var indexOf = new Dictionary<LaneNode, int>(nodes.Count);
            for (var i = 0; i < nodes.Count; i++)
            {
                indexOf[nodes[i]] = i;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var edges = new List<int>();
                var weights = new List<float>();

                var authored = node.Successors;
                var authoredWeights = node.TurnWeights;
                for (var s = 0; s < authored.Count; s++)
                {
                    var successor = authored[s];
                    if (successor == null || !indexOf.TryGetValue(successor, out var successorIndex))
                    {
                        continue;
                    }

                    edges.Add(successorIndex);
                    weights.Add(s < authoredWeights.Count ? authoredWeights[s] : 0f);
                }

                successors.Add(edges);
                turnWeights.Add(weights);

                if (node.IsEntryPortal)
                {
                    entryPortals.Add(i);
                }

                if (node.IsExitPortal)
                {
                    exitPortals.Add(i);
                }
            }

            JoinConnectors();
        }

        /// <summary>
        /// Jointure automatique des modules : un connecteur de SORTIE (fin de voie, sans successeur
        /// authore) recoit une arete vers le connecteur d'ENTREE (debut de voie) le plus proche sous
        /// le seuil authore et de meme sens de circulation. Poser deux modules bout a bout suffit --
        /// c'est le seul point de douleur reel du motif "noeud = GameObject", et il disparait ici.
        ///
        /// ponytail: balayage O(n^2) sur les seuls connecteurs -- trivial a l'echelle du district
        /// (quelques dizaines de noeuds). Indexer par cellule si le reseau depasse quelques centaines.
        /// </summary>
        private void JoinConnectors()
        {
            if (trafficSettings == null)
            {
                return;
            }

            var threshold = trafficSettings.ConnectorJoinDistance;
            var thresholdSquared = threshold * threshold;

            for (var i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].IsOutgoingConnector)
                {
                    continue;
                }

                var from = nodes[i].transform;
                var best = -1;
                var bestSquared = float.PositiveInfinity;

                for (var j = 0; j < nodes.Count; j++)
                {
                    if (j == i || !nodes[j].IsIncomingConnector)
                    {
                        continue;
                    }

                    var to = nodes[j].transform;
                    var offset = to.position - from.position;
                    if (offset.sqrMagnitude > thresholdSquared || offset.sqrMagnitude >= bestSquared)
                    {
                        continue;
                    }

                    // Meme sens de circulation : sans ce test, les deux voies opposees d'une meme
                    // jointure se relieraient a contresens.
                    if (Vector3.Dot(from.forward, to.forward) <= 0f)
                    {
                        continue;
                    }

                    bestSquared = offset.sqrMagnitude;
                    best = j;
                }

                if (best < 0)
                {
                    orphanConnectors.Add(i);
                    continue;
                }

                successors[i].Add(best);
                turnWeights[i].Add(1f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // EnsureBuilt, jamais Rebuild : la construction doit rester unique. Un Rebuild ici
            // remettrait a zero l'etat "averti une fois par noeud" a chaque repeint de la vue Scene
            // -- un defaut d'authoring de poids se remettrait a crier en boucle -- et rejouerait la
            // jointure O(n^2) des connecteurs des 204 noeuds du district a la meme frequence. La
            // reconstruction reste disponible la ou elle a un sens : Rebuild(), pour une modification
            // de hierarchie a l'execution.
            EnsureBuilt();

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var origin = node.transform.position;

                Gizmos.color = ResolveNodeColor(node);
                Gizmos.DrawWireSphere(origin, node.Role == LaneNodeRole.Normal ? 0.6f : 1.2f);

                var edges = successors[i];
                Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.9f);
                for (var e = 0; e < edges.Count; e++)
                {
                    var target = nodes[edges[e]].transform.position;
                    Gizmos.DrawLine(origin, target);

                    // Pointe de fleche : le sens de l'arete doit etre lisible dans la vue Scene,
                    // sinon une voie authoree a contresens ne se voit qu'en Play Mode.
                    var direction = target - origin;
                    if (direction.sqrMagnitude <= 0.0001f)
                    {
                        continue;
                    }

                    direction.Normalize();
                    var head = target - (direction * 1.5f);
                    var side = Vector3.Cross(direction, Vector3.up) * 0.6f;
                    Gizmos.DrawLine(target, head + side);
                    Gizmos.DrawLine(target, head - side);
                }
            }

            Gizmos.color = Color.red;
            for (var i = 0; i < orphanConnectors.Count; i++)
            {
                var origin = nodes[orphanConnectors[i]].transform.position;
                Gizmos.DrawWireCube(origin + (Vector3.up * 2f), Vector3.one * 2f);
                Gizmos.DrawLine(origin, origin + (Vector3.up * 4f));
            }
        }

        private static Color ResolveNodeColor(LaneNode node)
        {
            if (node.IsEntryPortal)
            {
                return Color.green;
            }

            if (node.IsExitPortal)
            {
                return Color.magenta;
            }

            return node.Role == LaneNodeRole.Connector ? Color.yellow : Color.cyan;
        }
    }
}
