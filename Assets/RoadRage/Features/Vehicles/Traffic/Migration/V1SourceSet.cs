#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    // =====================================================================================
    // Story 5.27 -- source V1 extraite, en lecture seule.
    //
    // L'adaptateur de migration est un outil editeur a sens unique (AD-38, AD-47) : il lit les
    // LaneNode de la scene, ne les modifie jamais et ne cree aucun objet de scene. Tout ce que
    // l'importeur sait de V1 passe par ce fichier, qui PROUVE son ensemble source : un LaneNode
    // hors d'une instance de module reconnue, ou une instance d'un prefab non reconnu, est un
    // echec dur plutot qu'une ligne de base silencieusement perimee.
    //
    // Identite source = GlobalObjectId (asset de scene + fileID d'instance + fileID prefab) :
    // jamais nom, hierarchie, index ni transform.
    // =====================================================================================

    /// <summary>Genre de module greybox reconnu, determine par l'identite de son prefab source.</summary>
    public enum V1ModuleKind
    {
        RoadSegment = 0,
        Crossroads = 1,
        TJunction = 2,
        Roundabout = 3,
        TunnelPortal = 4
    }

    /// <summary>Instance de module : racine d'instance de prefab portant des LaneNode.</summary>
    public sealed class V1Module
    {
        public string Key;
        public V1ModuleKind Kind;
        public string PrefabPath;

        /// <summary>Diagnostic seul (nom d'instance).</summary>
        public string Label;

        /// <summary>Rotation monde de la racine : sert a exprimer un sens en repere local prefab.</summary>
        public Quaternion Rotation;

        public readonly List<V1Node> Nodes = new List<V1Node>();

        public bool IsJunction
        {
            get { return Kind == V1ModuleKind.Crossroads || Kind == V1ModuleKind.TJunction || Kind == V1ModuleKind.Roundabout; }
        }
    }

    /// <summary>Un LaneNode V1 lu tel quel.</summary>
    public sealed class V1Node
    {
        public string Key;

        /// <summary>Diagnostic seul (nom du GameObject).</summary>
        public string Label;

        public V1Module Module;
        public LaneNodeRole Role;
        public bool ExitReusesEntry;
        public Vector3 Position;
        public Vector3 Forward;
        public Vector3 Up;
        public string SceneGuid;
        public long FileId;

        /// <summary>Aretes sortantes : authorees dans l'ordre de <c>successors</c>, puis jointure.</summary>
        public readonly List<V1Edge> Outgoing = new List<V1Edge>();

        public readonly List<V1Edge> Incoming = new List<V1Edge>();

        /// <summary>Nombre de successeurs authores (hors jointure) : fixe le sens d'un connecteur.</summary>
        public int AuthoredSuccessorCount;

        public bool IsEntryPortal
        {
            get { return Role == LaneNodeRole.PortalEntry; }
        }

        public bool IsExitPortal
        {
            get { return Role == LaneNodeRole.PortalExit || (Role == LaneNodeRole.PortalEntry && ExitReusesEntry); }
        }

        /// <summary>Connecteur de fin de voie (sans successeur authore), comme en V1.</summary>
        public bool IsOutgoingConnector
        {
            get { return Role == LaneNodeRole.Connector && AuthoredSuccessorCount == 0; }
        }

        /// <summary>Connecteur de debut de voie (au moins un successeur authore), comme en V1.</summary>
        public bool IsIncomingConnector
        {
            get { return Role == LaneNodeRole.Connector && AuthoredSuccessorCount > 0; }
        }
    }

    /// <summary>Arete V1 dirigee : successeur authore ou jointure de connecteurs.</summary>
    public sealed class V1Edge
    {
        public V1Node From;
        public V1Node To;

        /// <summary>Poids authore parallele a <c>successors</c> ; 1 pour une jointure (valeur V1).</summary>
        public float Weight;

        public bool IsConnectorJoin;

        /// <summary>Distance entre les deux connecteurs d'une jointure, en metres (0 sinon).</summary>
        public float GapMeters;

        /// <summary>Angle entre les sens des deux connecteurs d'une jointure, en degres (0 sinon).</summary>
        public float AngleDegrees;

        public string Key
        {
            get { return From.Key + ">" + To.Key; }
        }
    }

    /// <summary>Ensemble source V1 extrait, prouve et hache.</summary>
    public sealed class V1SourceSet
    {
        /// <summary>Les cinq prefabs greybox reconnus, audites le 2026-09-22.</summary>
        public static readonly IReadOnlyDictionary<string, V1ModuleKind> RecognisedPrefabs = new Dictionary<string, V1ModuleKind>
        {
            { "Assets/RoadRage/Prefabs/Greybox_RoadSegment_TwoWay.prefab", V1ModuleKind.RoadSegment },
            { "Assets/RoadRage/Prefabs/Greybox_Intersection.prefab", V1ModuleKind.Crossroads },
            { "Assets/RoadRage/Prefabs/Greybox_TJunction.prefab", V1ModuleKind.TJunction },
            { "Assets/RoadRage/Prefabs/Greybox_Roundabout.prefab", V1ModuleKind.Roundabout },
            { "Assets/RoadRage/Prefabs/Greybox_TunnelPortal.prefab", V1ModuleKind.TunnelPortal }
        };

        public readonly List<V1Module> Modules = new List<V1Module>();
        public readonly List<V1Node> Nodes = new List<V1Node>();
        public readonly List<V1Edge> Edges = new List<V1Edge>();
        public readonly List<V1Edge> Joins = new List<V1Edge>();
        public readonly List<string> Failures = new List<string>();

        public string SceneGuid;
        public float ConnectorJoinDistanceMeters;

        /// <summary>SHA-256 de la source extraite, canonisee et quantifiee (voir <see cref="ComputeSourceHash"/>).</summary>
        public string SourceHash;

        public bool IsValid
        {
            get { return Failures.Count == 0; }
        }

        /// <summary>Extrait la source de <paramref name="scene"/> avec les prefabs reconnus par defaut.</summary>
        public static V1SourceSet Extract(Scene scene)
        {
            return Extract(scene, RecognisedPrefabs);
        }

        /// <summary>
        /// Extrait et prouve la source. Ne modifie rien. Un echec est consigne dans
        /// <see cref="Failures"/> ; l'appelant ne doit rien produire tant qu'il en reste un.
        /// </summary>
        public static V1SourceSet Extract(Scene scene, IReadOnlyDictionary<string, V1ModuleKind> recognisedPrefabs)
        {
            return Extract(scene.GetRootGameObjects(), AssetDatabase.AssetPathToGUID(scene.path), recognisedPrefabs);
        }

        /// <summary>
        /// Meme extraction sur des racines explicites : l'ensemble source est exactement ce qui vit
        /// sous <paramref name="roots"/>, sans dependre de la scene qui les porte.
        /// </summary>
        public static V1SourceSet Extract(IReadOnlyList<GameObject> roots, string sceneGuid, IReadOnlyDictionary<string, V1ModuleKind> recognisedPrefabs)
        {
            var set = new V1SourceSet();
            set.SceneGuid = sceneGuid ?? string.Empty;

            var graphs = new List<LaneGraph>();
            var laneNodes = new List<LaneNode>();
            foreach (var root in roots)
            {
                graphs.AddRange(root.GetComponentsInChildren<LaneGraph>(true));
                laneNodes.AddRange(root.GetComponentsInChildren<LaneNode>(true));
            }

            LaneGraph graph = null;
            if (graphs.Count != 1)
            {
                set.Failures.Add("La scene doit porter exactement un LaneGraph, trouve : " + graphs.Count + ".");
            }
            else
            {
                graph = graphs[0];
                if (graph.TrafficSettings == null)
                {
                    set.Failures.Add("LaneGraph sans TrafficSettingsDef : seuil de jointure V1 inconnu.");
                }
                else if (!(graph.TrafficSettings.ConnectorJoinDistance > 0f))
                {
                    set.Failures.Add("Seuil de jointure V1 non positif : aucune jointure ne peut etre decouverte.");
                }
                else
                {
                    set.ConnectorJoinDistanceMeters = graph.TrafficSettings.ConnectorJoinDistance;
                }
            }

            // ------------------------------------------------ appartenance prouvee a un module reconnu
            var moduleByRoot = new Dictionary<GameObject, V1Module>();
            var nodeByComponent = new Dictionary<LaneNode, V1Node>();
            foreach (var laneNode in laneNodes)
            {
                string where = Describe(laneNode);
                if (graph != null && !laneNode.transform.IsChildOf(graph.transform))
                {
                    set.Failures.Add("LaneNode hors du LaneGraph : " + where + ".");
                    continue;
                }

                var root = PrefabUtility.GetNearestPrefabInstanceRoot(laneNode.gameObject);
                if (root == null || PrefabUtility.GetCorrespondingObjectFromSource(laneNode) == null)
                {
                    set.Failures.Add("LaneNode hors de tout module (hierarchie libre ou ajout sur instance) : " + where + ".");
                    continue;
                }

                string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(laneNode.gameObject);
                V1ModuleKind kind;
                if (!recognisedPrefabs.TryGetValue(prefabPath, out kind))
                {
                    set.Failures.Add("Module de prefab non reconnu '" + prefabPath + "' portant " + where + ".");
                    continue;
                }

                V1Module module;
                if (!moduleByRoot.TryGetValue(root, out module))
                {
                    module = new V1Module();
                    module.Key = GlobalObjectId.GetGlobalObjectIdSlow(root).ToString();
                    module.Kind = kind;
                    module.PrefabPath = prefabPath;
                    module.Label = root.name;
                    module.Rotation = root.transform.rotation;
                    moduleByRoot.Add(root, module);
                    set.Modules.Add(module);
                }

                var id = GlobalObjectId.GetGlobalObjectIdSlow(laneNode);
                var node = new V1Node();
                node.Key = id.ToString();
                node.Label = laneNode.name;
                node.Module = module;
                node.Role = laneNode.Role;
                node.ExitReusesEntry = laneNode.IsExitPortal && laneNode.Role == LaneNodeRole.PortalEntry;
                node.Position = laneNode.transform.position;
                node.Forward = laneNode.transform.forward;
                node.Up = laneNode.transform.up;
                node.SceneGuid = set.SceneGuid;
                // fileID cote scene d'un objet d'instance de prefab (regle Unity) : distinct par instance.
                node.FileId = id.targetPrefabId == 0UL
                    ? unchecked((long)id.targetObjectId)
                    : unchecked((long)((id.targetObjectId ^ id.targetPrefabId) & 0x7FFFFFFFFFFFFFFFUL));
                module.Nodes.Add(node);
                set.Nodes.Add(node);
                nodeByComponent.Add(laneNode, node);
            }

            SortByKey(set.Modules, delegate(V1Module m) { return m.Key; });
            SortByKey(set.Nodes, delegate(V1Node n) { return n.Key; });
            for (int i = 0; i < set.Modules.Count; i++)
            {
                SortByKey(set.Modules[i].Nodes, delegate(V1Node n) { return n.Key; });
            }

            for (int i = 1; i < set.Nodes.Count; i++)
            {
                if (set.Nodes[i].Key == set.Nodes[i - 1].Key)
                {
                    set.Failures.Add("Deux LaneNode partagent la cle source " + set.Nodes[i].Key + ".");
                }
            }

            // ------------------------------------------------ aretes authorees
            // Parcours par cle source triee : l'ordre d'un dictionnaire n'est pas un contrat.
            var componentOf = new Dictionary<V1Node, LaneNode>();
            foreach (var pair in nodeByComponent)
            {
                componentOf.Add(pair.Value, pair.Key);
            }

            foreach (var from in set.Nodes)
            {
                var component = componentOf[from];
                var successors = component.Successors;
                var weights = component.TurnWeights;
                if (weights.Count != successors.Count)
                {
                    set.Failures.Add("Poids non apparies aux successeurs (" + weights.Count + " pour " + successors.Count + ") : " + Describe(component) + ".");
                    continue;
                }

                for (int s = 0; s < successors.Count; s++)
                {
                    V1Node to;
                    if (successors[s] == null || !nodeByComponent.TryGetValue(successors[s], out to))
                    {
                        set.Failures.Add("Successeur " + s + " absent ou hors ensemble source : " + Describe(component) + ".");
                        continue;
                    }

                    if (to.Module != from.Module)
                    {
                        set.Failures.Add("Arete authoree entre deux modules : " + Describe(component) + " -> " + to.Label + ".");
                        continue;
                    }

                    var edge = new V1Edge();
                    edge.From = from;
                    edge.To = to;
                    edge.Weight = weights[s];
                    from.Outgoing.Add(edge);
                    to.Incoming.Add(edge);
                    from.AuthoredSuccessorCount++;
                    set.Edges.Add(edge);
                }
            }

            if (set.ConnectorJoinDistanceMeters > 0f)
            {
                DiscoverJoins(set);
            }

            SortByKey(set.Edges, delegate(V1Edge e) { return e.Key; });
            SortByKey(set.Joins, delegate(V1Edge e) { return e.Key; });
            set.SourceHash = ComputeSourceHash(set);
            return set;
        }

        /// <summary>
        /// Decouverte CANDIDATE des jointures, regle V1 reproduite (<c>LaneGraph.JoinConnectors</c>) :
        /// connecteur sortant vers le connecteur entrant le plus proche sous le seuil authore et de
        /// meme sens (<c>Dot &gt; 0</c>). Candidats parcourus par cle source ; une egalite exacte de
        /// distance, un connecteur sortant orphelin ou un entrant vise deux fois est un echec dur.
        /// </summary>
        private static void DiscoverJoins(V1SourceSet set)
        {
            float thresholdSquared = set.ConnectorJoinDistanceMeters * set.ConnectorJoinDistanceMeters;
            var claimed = new Dictionary<V1Node, V1Node>();

            for (int i = 0; i < set.Nodes.Count; i++)
            {
                var from = set.Nodes[i];
                if (!from.IsOutgoingConnector)
                {
                    continue;
                }

                V1Node best = null;
                float bestSquared = float.PositiveInfinity;
                bool tie = false;
                for (int j = 0; j < set.Nodes.Count; j++)
                {
                    var to = set.Nodes[j];
                    if (to == from || !to.IsIncomingConnector || Vector3.Dot(from.Forward, to.Forward) <= 0f)
                    {
                        continue;
                    }

                    float squared = (to.Position - from.Position).sqrMagnitude;
                    if (squared > thresholdSquared)
                    {
                        continue;
                    }

                    if (squared < bestSquared)
                    {
                        best = to;
                        bestSquared = squared;
                        tie = false;
                    }
                    else if (squared == bestSquared)
                    {
                        tie = true;
                    }
                }

                if (best == null)
                {
                    set.Failures.Add("Connecteur sortant orphelin : " + Describe(from) + ".");
                    continue;
                }

                if (tie)
                {
                    set.Failures.Add("Jointure ambigue (deux entrants a distance egale) depuis " + Describe(from) + ".");
                    continue;
                }

                V1Node other;
                if (claimed.TryGetValue(best, out other))
                {
                    set.Failures.Add("Connecteur entrant " + Describe(best) + " vise par " + Describe(other) + " et " + Describe(from) + ".");
                    continue;
                }

                claimed.Add(best, from);
                var edge = new V1Edge();
                edge.From = from;
                edge.To = best;
                edge.Weight = 1f;
                edge.IsConnectorJoin = true;
                edge.GapMeters = Mathf.Sqrt(bestSquared);
                edge.AngleDegrees = Vector3.Angle(from.Forward, best.Forward);
                from.Outgoing.Add(edge);
                best.Incoming.Add(edge);
                set.Edges.Add(edge);
                set.Joins.Add(edge);
            }

            // Un connecteur entrant jamais vise ouvrirait un corridor sans predecesseur : orphelin.
            for (int i = 0; i < set.Nodes.Count; i++)
            {
                if (set.Nodes[i].IsIncomingConnector && !claimed.ContainsKey(set.Nodes[i]))
                {
                    set.Failures.Add("Connecteur entrant orphelin : " + Describe(set.Nodes[i]) + ".");
                }
            }
        }

        /// <summary>
        /// Hash de la source V1 EXTRAITE, pas des octets de scene : un decor deplace ne perime
        /// rien, une pose de noeud si. Quantification explicite avant hash : positions en entiers
        /// de 0,1 mm, directions en 1e-6, poids en 1e-4 ; <c>-0</c> se normalise en 0.
        /// </summary>
        public static string ComputeSourceHash(V1SourceSet set)
        {
            var text = new StringBuilder();
            text.Append("rrs-v1-source/1\n");
            text.Append("join ").Append(Q(set.ConnectorJoinDistanceMeters, MetersStep)).Append('\n');
            for (int i = 0; i < set.Modules.Count; i++)
            {
                // La rotation du module fixe le datum (sens local +z) : elle fait partie de la source.
                var rotation = set.Modules[i].Rotation;
                text.Append("module ").Append(set.Modules[i].Key).Append(' ').Append(set.Modules[i].PrefabPath)
                    .Append(" q=").Append(Q(new Vector3(rotation.x, rotation.y, rotation.z), DirectionStep))
                    .Append(',').Append(Q(rotation.w, DirectionStep).ToString(CultureInfo.InvariantCulture)).Append('\n');
            }

            for (int i = 0; i < set.Nodes.Count; i++)
            {
                var node = set.Nodes[i];
                text.Append("node ").Append(node.Key)
                    .Append(" m=").Append(node.Module.Key)
                    .Append(" r=").Append((int)node.Role)
                    .Append(" x=").Append(node.ExitReusesEntry ? 1 : 0)
                    .Append(" p=").Append(Q(node.Position, MetersStep))
                    .Append(" f=").Append(Q(node.Forward, DirectionStep))
                    .Append(" u=").Append(Q(node.Up, DirectionStep));
                for (int e = 0; e < node.Outgoing.Count; e++)
                {
                    var edge = node.Outgoing[e];
                    if (!edge.IsConnectorJoin)
                    {
                        text.Append(" s=").Append(edge.To.Key).Append(':').Append(Q(edge.Weight, WeightStep));
                    }
                }

                text.Append('\n');
            }

            return Sha256Hex(text.ToString());
        }

        public const double MetersStep = 1e-4;
        public const double DirectionStep = 1e-6;
        public const double WeightStep = 1e-4;

        public static long Q(float value, double step)
        {
            return (long)Math.Round(value / step, MidpointRounding.AwayFromZero);
        }

        public static string Q(Vector3 value, double step)
        {
            return Q(value.x, step).ToString(CultureInfo.InvariantCulture) + ","
                + Q(value.y, step).ToString(CultureInfo.InvariantCulture) + ","
                + Q(value.z, step).ToString(CultureInfo.InvariantCulture);
        }

        public static string Sha256Hex(string text)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(new UTF8Encoding(false).GetBytes(text));
                var hex = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    hex.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                }

                return hex.ToString();
            }
        }

        private static void SortByKey<T>(List<T> items, Func<T, string> key)
        {
            items.Sort(delegate(T a, T b) { return string.CompareOrdinal(key(a), key(b)); });
        }

        private static string Describe(LaneNode node)
        {
            var root = PrefabUtility.GetNearestPrefabInstanceRoot(node.gameObject);
            return "'" + node.name + "'" + (root != null ? " (module '" + root.name + "')" : string.Empty);
        }

        private static string Describe(V1Node node)
        {
            return "'" + node.Label + "' (module '" + node.Module.Label + "')";
        }
    }
}
#endif
