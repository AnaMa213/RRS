using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.18 : vue d'une approche de jonction, indexee UNE FOIS a la construction du graphe.
    /// C'est la donnee que l'arbitrage consomme ; aucune recherche de hierarchie n'a lieu par tick.
    /// </summary>
    public readonly struct JunctionApproachInfo
    {
        public JunctionApproachInfo(int junctionKey, string junctionId, int nodeIndex,
            JunctionApproachRule rule, int signalGroup, Vector3 forward, int exitProbeNode)
            : this(junctionKey, junctionId, nodeIndex, rule, signalGroup, forward, exitProbeNode,
                nodeIndex, Vector3.zero, forward, 0f)
        {
        }

        public JunctionApproachInfo(int junctionKey, string junctionId, int nodeIndex,
            JunctionApproachRule rule, int signalGroup, Vector3 forward, int exitProbeNode,
            int entryNode, Vector3 entryPoint, Vector3 entryForward, float conflictEntryDistance)
        {
            JunctionKey = junctionKey;
            JunctionId = junctionId ?? string.Empty;
            NodeIndex = nodeIndex;
            Rule = rule;
            SignalGroup = signalGroup;
            Forward = forward;
            ExitProbeNode = exitProbeNode;
            EntryNode = entryNode;
            EntryPoint = entryPoint;
            EntryForward = entryForward;
            ConflictEntryDistance = conflictEntryDistance;
        }

        /// <summary>Identite de la jonction, strictement positive pour une approche. Stable sur tous les pairs (hierarchie de scene identique).</summary>
        public int JunctionKey { get; }

        /// <summary>Id authore sur les noeuds, tel quel. Deux jonctions distinctes peuvent le partager (deux instances du meme prefab).</summary>
        public string JunctionId { get; }

        /// <summary>Index du noeud de decision dans le graphe.</summary>
        public int NodeIndex { get; }

        /// <summary>Regle de priorite authoree sur cette approche, jamais recalculee.</summary>
        public JunctionApproachRule Rule { get; }

        /// <summary>Groupe d'approche dans le plan de feux de la jonction.</summary>
        public int SignalGroup { get; }

        /// <summary>Cap de l'approche, projete sur le plan et normalise.</summary>
        public Vector3 Forward { get; }

        /// <summary>
        /// Noeud de sortie SONDE de cette approche : le successeur qui continue le plus droit devant.
        /// C'est lui qui porte le controle de place au-dela de la jonction, ou -1 si l'approche n'a
        /// aucun successeur (place alors indeterminee, donc saturee).
        /// </summary>
        public int ExitProbeNode { get; }

        /// <summary>
        /// Noeud de FRONTIERE de l'approche : le predecesseur du noeud de decision, pose la ou la voie
        /// entre dans la jonction. Egal a <see cref="NodeIndex"/> quand l'approche n'a pas de
        /// predecessor exploitable -- le comportement retombe alors sur celui d'avant ANO-5.18-03.
        ///
        /// Il existe parce que le noeud de decision, lui, est authore au CENTRE de l'aire de conflit :
        /// mesure du 2026-09-21 dans le district, les quatre approches du carrefour central sont a
        /// 2,00 m du centre exact, pour une chaussee transversale large de 8 m. Tout ce qui se calcule
        /// depuis le noeud de decision -- ligne d'arret, depart de virage -- se calcule donc depuis le
        /// milieu du carrefour. La frontiere est le repere correct pour les deux.
        /// </summary>
        public int EntryNode { get; }

        /// <summary>Position de la frontiere d'entree, sur la voie d'approche.</summary>
        public Vector3 EntryPoint { get; }

        /// <summary>Cap de la voie a la frontiere d'entree, projete et normalise.</summary>
        public Vector3 EntryForward { get; }

        /// <summary>
        /// Distance, depuis <see cref="EntryPoint"/> et le long de <see cref="EntryForward"/>, du
        /// PREMIER point ou un mouvement de cette approche rencontre le mouvement d'une autre approche
        /// de la meme jonction. C'est la frontiere de l'aire de conflit vue par cette approche, et
        /// c'est d'elle que se deduit la ligne d'arret -- une grandeur mesuree sur le graphe, pas une
        /// distance authoree de plus.
        ///
        /// Aucune rencontre (jonction a une seule approche, branches qui ne se croisent pas) : la
        /// distance tombe sur le noeud de decision lui-meme, soit le comportement anterieur.
        /// </summary>
        public float ConflictEntryDistance { get; }
    }

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

        private readonly List<JunctionApproachInfo> junctionApproaches = new List<JunctionApproachInfo>();
        private readonly List<int> contradictoryJunctions = new List<int>();
        private int[] junctionKeyByNode = new int[0];
        private int[] approachIndexByNode = new int[0];
        private int[] ringIdByNode = new int[0];
        private bool warnedContradictoryJunctions;

        [System.NonSerialized] private bool built;

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

        /// <summary>
        /// Approches de jonction indexees a la construction (Story 5.18) : c'est ici, et une seule
        /// fois, que les regles authorees sur les <see cref="LaneNode"/> sont collectees.
        /// </summary>
        public IReadOnlyList<JunctionApproachInfo> JunctionApproaches
        {
            get
            {
                EnsureBuilt();
                return junctionApproaches;
            }
        }

        /// <summary>
        /// Jonctions dont les approches se contredisent : deux routes prioritaires qui se croisent, ou
        /// deux approches concurrentes partageant le meme groupe de feux. Signale, jamais corrige --
        /// c'est un defaut d'authoring, pas une topologie invalide.
        /// </summary>
        public IReadOnlyList<int> ContradictoryJunctions
        {
            get
            {
                EnsureBuilt();
                return contradictoryJunctions;
            }
        }

        /// <summary>Cle de jonction du noeud donne, ou 0 quand ce noeud n'est pas une approche.</summary>
        public int JunctionKeyOf(int nodeIndex)
        {
            EnsureBuilt();
            return nodeIndex >= 0 && nodeIndex < junctionKeyByNode.Length ? junctionKeyByNode[nodeIndex] : 0;
        }

        /// <summary>
        /// Vue de l'approche portee par ce noeud. Faux quand le noeud n'est pas une approche : aucune
        /// regle n'est alors inventee pour lui.
        /// </summary>
        public bool TryGetJunctionApproach(int nodeIndex, out JunctionApproachInfo info)
        {
            EnsureBuilt();
            var approachIndex = nodeIndex >= 0 && nodeIndex < approachIndexByNode.Length ? approachIndexByNode[nodeIndex] : -1;
            if (approachIndex < 0)
            {
                info = default;
                return false;
            }

            info = junctionApproaches[approachIndex];
            return true;
        }

        /// <summary>
        /// Noeud de sortie sonde d'une approche : le successeur qui continue le plus droit devant.
        /// C'est la voie dont la place est controlee AVANT toute consideration de priorite.
        /// </summary>
        public bool TryGetJunctionExitProbe(int nodeIndex, out Vector3 exitPoint)
        {
            EnsureBuilt();
            exitPoint = Vector3.zero;
            if (!TryGetJunctionApproach(nodeIndex, out var approach) || approach.ExitProbeNode < 0)
            {
                return false;
            }

            exitPoint = GetNodePosition(approach.ExitProbeNode);
            return true;
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

        /// <summary>Nearest directed road segment, evaluated at perception cadence, without hierarchy scans.</summary>
        public bool TryGetRoadPosition(Vector3 position, out Vector3 point, out Vector3 direction, out float distance)
        {
            EnsureBuilt();
            point = position; direction = Vector3.zero; distance = float.PositiveInfinity;
            for (var i = 0; i < nodes.Count; i++)
            {
                for (var j = 0; j < successors[i].Count; j++)
                {
                    var start = nodes[i].transform.position;
                    var end = nodes[successors[i][j]].transform.position;
                    var delta = Vector3.ProjectOnPlane(end - start, Vector3.up);
                    if (delta.sqrMagnitude < 0.0001f) continue;
                    var candidate = TrafficPerception.ClosestPointOnSegment(position, start, end);
                    var gap = Vector3.ProjectOnPlane(candidate - position, Vector3.up).magnitude;
                    if (gap >= distance) continue;
                    point = candidate; direction = delta.normalized; distance = gap;
                }
            }
            return float.IsFinite(distance);
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
            junctionApproaches.Clear();
            contradictoryJunctions.Clear();
            warnedContradictoryJunctions = false;

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
            BuildJunctionIndex();
        }

        /// <summary>
        /// Story 5.18 : collecte les approches de jonction une seule fois, a la construction. La
        /// priorite est donc lue a l'authoring et jamais recalculee par frame, et rien ici ne depend
        /// d'un balayage de scene au tick.
        ///
        /// L'identite d'une jonction est le couple (module porteur, <c>junctionId</c> authore). Sans le
        /// module, les quatre instances du meme prefab de jonction en T porteraient le meme id et
        /// seraient vues comme UNE jonction : un vehicule du nord arbitrerait contre un vehicule a
        /// trente metres de la, qui ne le voit meme pas. Le module porteur est le plus haut ancetre
        /// sous la racine du graphe -- donc stable sur tous les pairs, la scene etant identique.
        /// </summary>
        /// <summary>
        /// Identifie les ANNEAUX DE CIRCULATION -- les giratoires -- par la seule topologie du graphe,
        /// une fois a la construction. Un anneau est un cycle ferme que l'on parcourt en prenant
        /// partout le successeur le mieux aligne sur l'axe authore du noeud, c'est-a-dire "tout
        /// droit" : c'est exactement ce que decrit un anneau a sens unique, et cela ne suppose ni
        /// nom, ni prefab, ni constante de scene. Un giratoire plus grand ou a cinq branches se
        /// detecte donc de lui-meme.
        ///
        /// La longueur minimale ecarte les allers-retours a deux noeuds, qui ne sont pas des anneaux.
        /// </summary>
        private void BuildRingIndex()
        {
            ringIdByNode = new int[nodes.Count];
            var rings = 0;

            for (var start = 0; start < nodes.Count; start++)
            {
                if (ringIdByNode[start] != 0) continue;

                var walk = new List<int>();
                var current = start;
                for (var step = 0; step < nodes.Count; step++)
                {
                    walk.Add(current);
                    var next = StraightAheadSuccessor(current);
                    if (next < 0) break;

                    if (next == start && walk.Count >= MinimumRingNodes)
                    {
                        rings++;
                        for (var i = 0; i < walk.Count; i++) ringIdByNode[walk[i]] = rings;
                        break;
                    }

                    if (walk.Contains(next)) break;
                    current = next;
                }
            }
        }

        /// <summary>Un anneau plus court qu'un demi-tour n'en est pas un.</summary>
        private const int MinimumRingNodes = 5;

        /// <summary>Successeur le mieux aligne sur l'axe AUTHORE du noeud : sa continuation naturelle.</summary>
        private int StraightAheadSuccessor(int index)
        {
            var forward = PlanarForward(nodes[index].transform.rotation * Vector3.forward);
            if (forward.sqrMagnitude <= 0.0001f) return -1;
            forward.Normalize();

            var best = -1;
            var bestDot = -2f;
            var from = nodes[index].transform.position;
            for (var i = 0; i < successors[index].Count; i++)
            {
                var direction = PlanarForward(nodes[successors[index][i]].transform.position - from);
                if (direction.sqrMagnitude <= 0.0001f) continue;
                var alignment = Vector3.Dot(direction.normalized, forward);
                if (alignment <= bestDot) continue;
                bestDot = alignment;
                best = successors[index][i];
            }

            return best;
        }

        /// <summary>
        /// Anneau de circulation auquel ce noeud appartient, ou 0 s'il n'en est pas. C'est la donnee
        /// dont depend la priorite au giratoire : elle est partagee, donc deux pairs en tirent le
        /// meme verdict.
        /// </summary>
        public int RingIdOf(int index)
        {
            EnsureBuilt();
            return IsValidIndex(index) && index < ringIdByNode.Length ? ringIdByNode[index] : 0;
        }

        private void BuildJunctionIndex()
        {
            BuildRingIndex();
            junctionKeyByNode = new int[nodes.Count];
            approachIndexByNode = new int[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                approachIndexByNode[i] = -1;
            }

            var keys = new Dictionary<string, int>(System.StringComparer.Ordinal);

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (!node.IsJunctionApproach)
                {
                    continue;
                }

                var junctionId = node.JunctionId.Trim();
                var key = ResolveJunctionKey(keys, node.transform, junctionId);
                junctionKeyByNode[i] = key;
                approachIndexByNode[i] = junctionApproaches.Count;
                junctionApproaches.Add(new JunctionApproachInfo(key, junctionId, i, node.JunctionRule,
                    node.SignalGroup, PlanarForward(node.transform.rotation * Vector3.forward), ResolveExitProbe(i)));
            }

            ResolveJunctionBoundaries();

            for (var i = 0; i < junctionApproaches.Count; i++)
            {
                var approaches = junctionApproaches;
                for (var j = i + 1; j < approaches.Count; j++)
                {
                    if (approaches[i].JunctionKey != approaches[j].JunctionKey)
                    {
                        continue;
                    }

                    if (!ApproachesContradict(approaches[i], approaches[j]))
                    {
                        continue;
                    }

                    if (!contradictoryJunctions.Contains(approaches[i].JunctionKey))
                    {
                        contradictoryJunctions.Add(approaches[i].JunctionKey);
                    }
                }
            }

            if (contradictoryJunctions.Count > 0 && !warnedContradictoryJunctions)
            {
                warnedContradictoryJunctions = true;
                Debug.LogWarning("[Vehicles] LaneGraph : " + contradictoryJunctions.Count
                    + " jonction(s) dont les approches se contredisent (deux routes prioritaires qui se croisent,"
                    + " ou deux approches concurrentes dans le meme groupe de feux). L'authoring est a corriger :"
                    + " l'ordre total departage, mais ce n'est pas la regle voulue.", this);
            }
        }

        /// <summary>
        /// Deuxieme passe de l'indexation : frontiere d'entree de chaque approche, puis distance a la
        /// premiere rencontre entre ses mouvements et ceux des autres approches de la meme jonction.
        ///
        /// Les deux se calculent ICI, une fois, parce qu'ils ne dependent que du trace : la jonction
        /// ne change pas de forme en cours de partie. Le cout est quadratique dans le nombre de
        /// mouvements d'UNE jonction (au plus 4 approches x 3 sorties dans le district), jamais dans
        /// le nombre de noeuds du graphe.
        /// </summary>
        private void ResolveJunctionBoundaries()
        {
            if (junctionApproaches.Count == 0)
            {
                return;
            }

            var predecessors = new List<int>[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                for (var e = 0; e < successors[i].Count; e++)
                {
                    var target = successors[i][e];
                    (predecessors[target] ??= new List<int>()).Add(i);
                }
            }

            // 1. Frontiere d'entree.
            for (var a = 0; a < junctionApproaches.Count; a++)
            {
                var approach = junctionApproaches[a];
                var entryNode = ResolveEntryNode(predecessors, approach.NodeIndex, approach.Forward);
                var entryPoint = nodes[entryNode].transform.position;
                var entryForward = entryNode == approach.NodeIndex
                    ? approach.Forward
                    : PlanarForward(nodes[entryNode].transform.rotation * Vector3.forward);
                junctionApproaches[a] = new JunctionApproachInfo(approach.JunctionKey, approach.JunctionId,
                    approach.NodeIndex, approach.Rule, approach.SignalGroup, approach.Forward, approach.ExitProbeNode,
                    entryNode, entryPoint, entryForward,
                    Vector3.Dot(Vector3.ProjectOnPlane(nodes[approach.NodeIndex].transform.position - entryPoint, Vector3.up), entryForward));
            }

            // 2. Premiere rencontre entre mouvements, A LEUR LARGEUR REELLE.
            //
            // Mesurer l'intersection stricte de deux axes centraux revient a traiter les vehicules
            // comme des points. Ils ne le sont pas : mesure du district, l'avant d'un vehicule
            // arrete a la ligne qui en decoulait se trouvait 2,34 m DANS le couloir que le trafic
            // tournant doit emprunter -- c'est le blocage rapporte en recette. La frontiere est
            // donc l'abscisse ou les deux COULOIRS se touchent, pas ou les deux axes se croisent.
            var clearance = trafficSettings != null ? trafficSettings.JunctionMovementClearance : 0f;
            for (var a = 0; a < junctionApproaches.Count; a++)
            {
                var own = junctionApproaches[a];
                var nearest = float.PositiveInfinity;
                for (var b = 0; b < junctionApproaches.Count; b++)
                {
                    if (b == a || junctionApproaches[b].JunctionKey != own.JunctionKey) continue;
                    var other = junctionApproaches[b];
                    if (other.NodeIndex == own.NodeIndex) continue;

                    foreach (var ownExit in successors[own.NodeIndex])
                    {
                        foreach (var otherExit in successors[other.NodeIndex])
                        {
                            var meeting = ResolveMovementMeeting(own, ownExit, other, otherExit, clearance);
                            if (meeting < nearest) nearest = meeting;
                        }
                    }
                }

                // La frontiere ne depasse JAMAIS le noeud de decision : une ligne d'arret posee
                // au-dela ferait attendre le vehicule dans le carrefour qu'il doit justement tenir
                // libre. Quand les couloirs ne se touchent que plus loin, le noeud fait foi.
                var nodeAlong = Vector3.Dot(
                    Vector3.ProjectOnPlane(nodes[own.NodeIndex].transform.position - own.EntryPoint, Vector3.up),
                    own.EntryForward);
                if (nodeAlong > 0.01f) nearest = Mathf.Min(nearest, nodeAlong);

                if (!float.IsFinite(nearest) || nearest <= 0f)
                {
                    // Aucun croisement mesurable : la frontiere retombe sur le noeud de decision.
                    continue;
                }

                junctionApproaches[a] = new JunctionApproachInfo(own.JunctionKey, own.JunctionId, own.NodeIndex,
                    own.Rule, own.SignalGroup, own.Forward, own.ExitProbeNode, own.EntryNode, own.EntryPoint,
                    own.EntryForward, nearest);
            }
        }

        /// <summary>
        /// Avancement, sur l'approche <paramref name="own"/>, de la premiere rencontre entre son
        /// mouvement vers <paramref name="ownExit"/> et le mouvement de <paramref name="other"/> vers
        /// <paramref name="otherExit"/>. Chaque mouvement est reduit a sa polyligne frontiere -> coin
        /// -> sortie : deux segments suffisent, la Bezier ne quitte jamais leur enveloppe.
        /// </summary>
        private float ResolveMovementMeeting(in JunctionApproachInfo own, int ownExit,
            in JunctionApproachInfo other, int otherExit, float clearance)
        {
            if (clearance <= 0.01f) return ResolveAxisCrossing(own, ownExit, other, otherExit);

            // Les deux mouvements sont echantillonnes sur la Bezier REELLEMENT parcourue -- la meme
            // que <see cref="LaneGraphRouting.SampleLaneEdge"/> emet a la conduite -- et non sur la
            // corde frontiere -> coin -> sortie, qui passe a l'exterieur du virage.
            SampleMovement(own, ownExit, movementScratchOwn);
            SampleMovement(other, otherExit, movementScratchOther);

            var squared = clearance * clearance;
            for (var i = 0; i < MovementSamples; i++)
            {
                for (var j = 0; j < MovementSamples; j++)
                {
                    if (Vector3.ProjectOnPlane(movementScratchOwn[i] - movementScratchOther[j], Vector3.up).sqrMagnitude > squared)
                        continue;

                    var along = Vector3.Dot(
                        Vector3.ProjectOnPlane(movementScratchOwn[i] - own.EntryPoint, Vector3.up), own.EntryForward);
                    return along > 0.01f ? along : float.PositiveInfinity;
                }
            }

            return float.PositiveInfinity;
        }

        /// <summary>Repli sans epaisseur : intersection stricte des axes, conserve pour un monde qui
        /// n'authore aucun degagement de mouvement.</summary>
        private float ResolveAxisCrossing(in JunctionApproachInfo own, int ownExit,
            in JunctionApproachInfo other, int otherExit)
        {
            BuildMovementPolyline(own, ownExit, out var ownStart, out var ownCorner, out var ownEnd);
            BuildMovementPolyline(other, otherExit, out var otherStart, out var otherCorner, out var otherEnd);

            var nearest = float.PositiveInfinity;
            var ownSegments = new[] { (ownStart, ownCorner), (ownCorner, ownEnd) };
            var otherSegments = new[] { (otherStart, otherCorner), (otherCorner, otherEnd) };
            foreach (var mine in ownSegments)
            {
                foreach (var theirs in otherSegments)
                {
                    if (!LaneGraphRouting.TrySegmentIntersection(mine.Item1, mine.Item2, theirs.Item1, theirs.Item2, out var point))
                    {
                        continue;
                    }

                    var along = Vector3.Dot(Vector3.ProjectOnPlane(point - own.EntryPoint, Vector3.up), own.EntryForward);
                    if (along > 0.01f && along < nearest) nearest = along;
                }
            }

            return nearest;
        }

        /// <summary>Resolution de l echantillonnage d un mouvement : 0,25 m environ sur un virage de
        /// district, soit six fois plus fin que le degagement mesure. ponytail: deux tampons
        /// reutilises, l'indexation ne tourne qu'a la construction du graphe.</summary>
        private const int MovementSamples = 41;
        private readonly Vector3[] movementScratchOwn = new Vector3[MovementSamples];
        private readonly Vector3[] movementScratchOther = new Vector3[MovementSamples];

        private void SampleMovement(in JunctionApproachInfo approach, int exitNode, Vector3[] destination)
        {
            var start = approach.EntryPoint;
            var end = nodes[exitNode].transform.position;
            var exitForward = PlanarForward(nodes[exitNode].transform.rotation * Vector3.forward);
            for (var i = 0; i < MovementSamples; i++)
            {
                destination[i] = LaneGraphRouting.ResolveJunctionTurnPoint(
                    start, approach.EntryForward, end, exitForward, i / (float)(MovementSamples - 1));
            }
        }

        private void BuildMovementPolyline(in JunctionApproachInfo approach, int exitNode,
            out Vector3 start, out Vector3 corner, out Vector3 end)
        {
            start = approach.EntryPoint;
            end = nodes[exitNode].transform.position;
            var exitForward = PlanarForward(nodes[exitNode].transform.rotation * Vector3.forward);
            corner = LaneGraphRouting.TryResolveLaneAxisCorner(start, approach.EntryForward, end, exitForward, out var resolved)
                ? resolved
                : Vector3.Lerp(start, end, 0.5f);
        }

        /// <summary>
        /// Predecesseur qui porte la frontiere d'entree : celui d'ou l'on ARRIVE, donc le mieux aligne
        /// sur le cap de l'approche. Les jointures de module posent deux noeuds a la meme place (arete
        /// de longueur nulle) : on remonte au travers, au plus quelques sauts. Sans predecesseur
        /// exploitable, le noeud de decision se sert de frontiere a lui-meme.
        /// </summary>
        private int ResolveEntryNode(IReadOnlyList<List<int>> predecessors, int nodeIndex, Vector3 forward)
        {
            var current = nodeIndex;
            for (var hop = 0; hop < 4; hop++)
            {
                var candidates = current < predecessors.Count ? predecessors[current] : null;
                if (candidates == null || candidates.Count == 0) return current;

                var best = -1;
                var bestDot = 0.5f; // Un predecesseur qui n'arrive pas dans notre axe n'est pas notre voie.
                for (var i = 0; i < candidates.Count; i++)
                {
                    var delta = Vector3.ProjectOnPlane(nodes[current].transform.position - nodes[candidates[i]].transform.position, Vector3.up);
                    if (delta.sqrMagnitude <= 0.0001f)
                    {
                        // Jointure de module : meme place, on continue de remonter par ce noeud.
                        best = candidates[i];
                        bestDot = float.PositiveInfinity;
                        break;
                    }

                    var dot = Vector3.Dot(forward, delta.normalized);
                    if (dot > bestDot) { bestDot = dot; best = candidates[i]; }
                }

                if (best < 0) return current;
                var advanced = Vector3.ProjectOnPlane(nodes[current].transform.position - nodes[best].transform.position, Vector3.up).sqrMagnitude > 0.0001f;
                current = best;
                if (advanced) return current;
            }

            return current;
        }

        private int ResolveJunctionKey(Dictionary<string, int> keys, Transform node, string junctionId)
        {
            // ponytail: le module porteur est le plus haut ancetre sous la racine du graphe. Remonter
            // quelques Transform une fois par noeud a la construction est negigeable devant un balayage
            // de hierarchie par tick, et evite d'introduire un rayon de regroupement authore de plus.
            var scope = node;
            while (scope.parent != null && scope.parent != transform)
            {
                scope = scope.parent;
            }

            var identity = scope.GetSiblingIndex() + ":" + scope.name + "|" + junctionId;
            if (keys.TryGetValue(identity, out var key))
            {
                return key;
            }

            key = keys.Count + 1;
            keys.Add(identity, key);
            return key;
        }

        /// <summary>
        /// Deux approches de la meme jonction se contredisent quand elles se disputent la traversee :
        /// deux routes prioritaires qui se croisent, ou deux approches concurrentes qui partagent le
        /// meme groupe de feux (elles seraient vertes ensemble). Signale, jamais corrige.
        /// </summary>
        private static bool ApproachesContradict(JunctionApproachInfo first, JunctionApproachInfo second)
        {
            var firstClaim = new JunctionClaim(first.JunctionKey, first.Rule, first.Forward, 0f, first.NodeIndex, 0UL, true, true, true, false);
            var secondClaim = new JunctionClaim(second.JunctionKey, second.Rule, second.Forward, 0f, second.NodeIndex, 1UL, true, true, true, false);
            if (!JunctionRules.Conflicts(firstClaim, secondClaim))
            {
                return false;
            }

            if (first.Rule == JunctionApproachRule.PriorityRoad && second.Rule == JunctionApproachRule.PriorityRoad)
            {
                return true;
            }

            return first.Rule == JunctionApproachRule.TrafficLight && second.Rule == JunctionApproachRule.TrafficLight
                && first.SignalGroup == second.SignalGroup;
        }

        /// <summary>
        /// Voie de sortie sondee d'une approche : le successeur qui continue le plus droit devant.
        /// Sans successeur, -1 -- et la place est alors indeterminee, donc traitee comme saturee.
        /// </summary>
        private int ResolveExitProbe(int nodeIndex)
        {
            var edges = successors[nodeIndex];
            if (edges.Count == 0)
            {
                return -1;
            }

            var origin = nodes[nodeIndex].transform.position;
            var forward = PlanarForward(nodes[nodeIndex].transform.rotation * Vector3.forward);
            var best = -1;
            var bestDot = float.NegativeInfinity;
            for (var i = 0; i < edges.Count; i++)
            {
                var delta = nodes[edges[i]].transform.position - origin;
                delta.y = 0f;
                if (delta.sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                var dot = Vector3.Dot(forward, delta.normalized);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = edges[i];
                }
            }

            return best >= 0 ? best : edges[0];
        }

        private static Vector3 PlanarForward(Vector3 forward)
        {
            var planar = new Vector3(forward.x, 0f, forward.z);
            return planar.sqrMagnitude > 0.0001f && float.IsFinite(planar.x) && float.IsFinite(planar.z)
                ? planar.normalized
                : Vector3.forward;
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
