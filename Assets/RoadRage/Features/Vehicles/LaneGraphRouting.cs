using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.10 : toutes les decisions de parcours du trafic, en fonctions pures statiques --
    /// testables en EditMode sans scene ni Netcode. Le <see cref="LaneGraph"/> fournit la donnee,
    /// <see cref="NetworkedAIVehicleDriverController"/> applique le resultat ; aucune decision de
    /// parcours ne vit dans un MonoBehaviour.
    ///
    /// Modele jtrrouter : le parcours nait d'un tirage de virage pondere a chaque jonction, jamais
    /// d'un itineraire pre-calcule par vehicule -- donc aucun A*, aucune matrice origine-destination.
    ///
    /// <c>NormalizeIndex</c> et <c>NearestIndex</c> viennent de <c>RouteWaypoints</c> (Story 5.2),
    /// supprime avec la boucle ; leur comportement est inchange.
    /// </summary>
    public static class LaneGraphRouting
    {
        /// <summary>
        /// COIN d'un mouvement de jonction : le point ou l'axe de la voie d'entree rencontre l'axe de
        /// la voie de sortie. C'est le sommet reel du virage -- celui qu'un conducteur contourne.
        ///
        /// Le coin n'est rendu que s'il est DEVANT l'entree et DERRIERE la sortie. Un coin situe en
        /// arriere de l'entree signale que le mouvement commence deja au-dela de son propre virage :
        /// c'etait exactement le cas avant ANO-5.18-03, ou le mouvement partait du noeud de decision
        /// pose au CENTRE du carrefour, 2 m apres le coin. Aucune courbe ne rattrape cela, et la
        /// fonction refuse donc plutot que d'en inventer une.
        /// </summary>
        public static bool TryResolveLaneAxisCorner(Vector3 entry, Vector3 entryForward, Vector3 exit, Vector3 exitForward, out Vector3 corner)
        {
            corner = Vector3.zero;
            var entryDirection = Vector3.ProjectOnPlane(entryForward, Vector3.up);
            var exitDirection = Vector3.ProjectOnPlane(exitForward, Vector3.up);
            if (entryDirection.sqrMagnitude <= 0.0001f || exitDirection.sqrMagnitude <= 0.0001f) return false;
            entryDirection.Normalize();
            exitDirection.Normalize();

            var denominator = Cross(entryDirection, exitDirection);
            if (Mathf.Abs(denominator) < 0.01f) return false; // Axes paralleles : pas de coin, c est un tout droit.

            var offset = Vector3.ProjectOnPlane(exit - entry, Vector3.up);
            var alongEntry = Cross(offset, exitDirection) / denominator;
            var alongExit = Cross(offset, entryDirection) / denominator;
            if (!float.IsFinite(alongEntry) || !float.IsFinite(alongExit)) return false;

            // Le coin doit etre devant nous ET en amont de la sortie, sinon le mouvement est mal pose.
            if (alongEntry <= 0.01f || alongExit >= -0.01f) return false;

            corner = entry + entryDirection * alongEntry;
            corner.y = Mathf.Lerp(entry.y, exit.y, 0.5f);
            return true;
        }

        /// <summary>
        /// Point du connecteur de virage a l'avancement donne. Quand le COIN des deux axes de voie
        /// existe, la courbe est une Bezier QUADRATIQUE dont il est l'unique point de controle : elle
        /// part tangente a la voie d'entree, arrive tangente a la voie de sortie, et reste du bon cote
        /// du coin -- c'est la trajectoire d'un virage ordinaire.
        ///
        /// Sans coin exploitable (axes paralleles, ou mouvement mal pose), la fonction retombe sur
        /// l'arc cubique d'origine, qui reste tangent aux deux caps. Un tout droit y donne une droite.
        /// </summary>
        public static Vector3 ResolveJunctionTurnPoint(Vector3 entry, Vector3 entryForward, Vector3 exit, Vector3 exitForward, float progress)
        {
            progress = Mathf.Clamp01(progress);
            var chord = Vector3.ProjectOnPlane(exit - entry, Vector3.up).magnitude;
            entryForward = Vector3.ProjectOnPlane(entryForward, Vector3.up).normalized;
            exitForward = Vector3.ProjectOnPlane(exitForward, Vector3.up).normalized;
            if (chord <= 0.0001f || entryForward.sqrMagnitude <= 0.0001f || exitForward.sqrMagnitude <= 0.0001f)
                return Vector3.Lerp(entry, exit, progress);

            if (TryResolveLaneAxisCorner(entry, entryForward, exit, exitForward, out var corner))
            {
                var remainder = 1f - progress;
                return remainder * remainder * entry + 2f * remainder * progress * corner + progress * progress * exit;
            }

            var firstControl = entry + entryForward * (chord * 0.5f);
            var secondControl = exit - exitForward * (chord * 0.5f);
            var inverse = 1f - progress;
            return inverse * inverse * inverse * entry
                + 3f * inverse * inverse * progress * firstControl
                + 3f * inverse * progress * progress * secondControl
                + progress * progress * progress * exit;
        }

        /// <summary>
        /// Longueur approchee du connecteur : la polyligne frontiere -> coin -> sortie. Elle sert a
        /// convertir une duree de visee en avancement, donc une sous-estimation ferait viser trop
        /// loin et couper le virage.
        /// </summary>
        public static float ResolveJunctionTurnLength(Vector3 entry, Vector3 entryForward, Vector3 exit, Vector3 exitForward)
        {
            if (!TryResolveLaneAxisCorner(entry, entryForward, exit, exitForward, out var corner))
            {
                return Vector3.ProjectOnPlane(exit - entry, Vector3.up).magnitude;
            }

            return Vector3.ProjectOnPlane(corner - entry, Vector3.up).magnitude
                + Vector3.ProjectOnPlane(exit - corner, Vector3.up).magnitude;
        }

        /// <summary>
        /// Avancement du vehicule SUR le connecteur, par echantillonnage : le parametre du point le
        /// plus proche de lui. Il remplace le rapport de distances a la sortie, qui mesurait une corde
        /// et non une course -- sur une courbe qui bombe, la distance a la sortie decroit plus
        /// lentement que l avancement reel, donc la visee retardait et le vehicule sortait large.
        /// </summary>
        public static float ResolveJunctionTurnProgress(Vector3 entry, Vector3 entryForward, Vector3 exit,
            Vector3 exitForward, Vector3 position, int samples = 12)
        {
            samples = Mathf.Clamp(samples, 2, 64);
            var best = 0f;
            var bestSquared = float.PositiveInfinity;
            for (var i = 0; i <= samples; i++)
            {
                var t = i / (float)samples;
                var point = ResolveJunctionTurnPoint(entry, entryForward, exit, exitForward, t);
                var squared = Vector3.ProjectOnPlane(point - position, Vector3.up).sqrMagnitude;
                if (squared >= bestSquared) continue;
                bestSquared = squared;
                best = t;
            }

            return best;
        }

        /// <summary>
        /// Echantillonne UNE arete de voie SUR SA COURBE et ajoute les points obtenus a
        /// <paramref name="destination"/> apres <paramref name="count"/> ; rend le nouveau compte. Le
        /// point de depart n'est jamais reemis, celui d'arrivee l'est toujours.
        ///
        /// C'est la brique unique de la Story 5.18 : la conduite, la perception et l'arbitrage de
        /// jonction doivent lire LA MEME trajectoire. Tant que la trajectoire prevue reliait les noeuds
        /// en LIGNE DROITE, ces trois lectures divergeaient de la course reelle :
        ///
        /// - sur l'anneau de 6,00 m des giratoires, la corde entre deux noeuds passe 0,31 m a
        ///   l'interieur de l'arc, et la visee extrapolee au-dela du noeud sortait 1,68 m a l'exterieur ;
        /// - a un carrefour, la corde est une diagonale en travers de l'intersection, ce qui a produit
        ///   des faces-a-faces declares entre deux vehicules a angle droit (ANO-5.18-04).
        ///
        /// Une arete DROITE n'a rien a echantillonner : sa corde est sa courbe, et elle ne consomme
        /// donc qu'un seul point de la trajectoire.
        /// </summary>
        public static int SampleLaneEdge(Vector3 from, Vector3 fromForward, Vector3 to, Vector3 toForward,
            float step, Vector3[] destination, int count, float straightStep = 4f)
        {
            if (destination == null || count < 0 || count >= destination.Length) return Mathf.Max(0, count);
            var chord = Vector3.ProjectOnPlane(to - from, Vector3.up).magnitude;
            if (chord <= 0.0001f) return count;

            var length = ResolveJunctionTurnLength(from, fromForward, to, toForward);
            var straight = !float.IsFinite(length) || length <= chord + 0.02f;

            // Une DROITE se subdivise aussi, plus grossierement. Sa forme n'en a pas besoin, mais les
            // lectures qui balayent la trajectoire sur une FENETRE de distance, si : tant qu'une
            // ligne droite de 20 m ne comptait qu'un seul segment, la fenetre s'y epuisait d'un coup
            // et ne voyait jamais la courbe suivante. Le plafond de vitesse arrivait donc trop tard,
            // c'est-a-dire jamais.
            var span = straight ? chord : length;
            var resolution = Mathf.Max(0.25f, straight ? straightStep : step);
            var samples = Mathf.Min(
                Mathf.Max(1, Mathf.CeilToInt(span / resolution)),
                destination.Length - count);

            for (var i = 1; i <= samples; i++)
            {
                var progress = i / (float)samples;
                destination[count++] = straight
                    ? Vector3.Lerp(from, to, progress)
                    : ResolveJunctionTurnPoint(from, fromForward, to, toForward, progress);
            }

            return count;
        }

        /// <summary>
        /// Vitesse plafond imposee par la COURBURE de la trajectoire deja construite, sur la fenetre
        /// que le vehicule va reellement parcourir. Elle rend l'infini quand la trajectoire est droite.
        ///
        /// Sans elle, le vehicule vise une courbe que son braquage ne peut pas tenir : mesure
        /// ANO-5.18-04, le connecteur de virage serre du district demande 4,25 m quand le braquage
        /// disponible a 8,0 m/s n'en permet que 4,85 m. Le vehicule sortait 1,84 m a cote de sa propre
        /// trajectoire, pour un budget d'ecart de 0,97 m avant l'axe median.
        /// </summary>
        public static float ResolvePathCurveSpeedLimit(Vector3[] path, int count, Vector3 position,
            float window, float wheelbase, float maxSteerAngleDegrees, float highSpeedSteerAngleDegrees,
            float fullReductionSpeed)
        {
            return VehicleSteeringModel.ResolveCurveSpeedLimit(
                ResolvePathMinimumRadius(path, count, position, window), wheelbase,
                maxSteerAngleDegrees, highSpeedSteerAngleDegrees, fullReductionSpeed);
        }

        /// <summary>
        /// Rayon de courbure le PLUS SERRE de la trajectoire sur la fenetre a venir, depuis le point le
        /// plus proche du vehicule. Infini sur une trajectoire droite.
        ///
        /// Deux decisions en dependent, et c'est la meme grandeur : la vitesse tenable, et la longueur
        /// de visee. Une poursuite pure coupe l'interieur d'une courbe d'environ L^2/(8R) ; borner la
        /// visee par le rayon ramene donc cette coupe a R/8, soit 0,53 m sur le virage de 4,24 m du
        /// district et 0,75 m sur l'anneau de 6,00 m -- sous le budget mesure de 0,97 m. Aucune
        /// constante de reglage : la borne EST la geometrie de la voie.
        /// </summary>
        public static float ResolvePathMinimumRadius(Vector3[] path, int count, Vector3 position, float window)
        {
            if (path == null || count < 3) return float.PositiveInfinity;
            count = Mathf.Min(count, path.Length);
            if (!float.IsFinite(window) || window <= 0f) return float.PositiveInfinity;

            var start = 1;
            var bestSquared = float.PositiveInfinity;
            for (var i = 1; i + 1 < count; i++)
            {
                var squared = Vector3.ProjectOnPlane(path[i] - position, Vector3.up).sqrMagnitude;
                if (squared >= bestSquared) continue;
                bestSquared = squared;
                start = i;
            }

            var travelled = 0f;
            var tightest = float.PositiveInfinity;
            for (var i = start; i + 1 < count; i++)
            {
                var radius = ResolveRadiusOverArc(path, count, i, CurvatureArc);
                if (radius < tightest) tightest = radius;

                travelled += Vector3.ProjectOnPlane(path[i + 1] - path[i], Vector3.up).magnitude;
                if (travelled >= window) break;
            }

            return tightest;
        }

        /// <summary>
        /// Longueur d'arc sur laquelle la courbure se mesure. Elle doit couvrir plusieurs segments
        /// pour que la lecture ne depende pas de la densite d'echantillonnage.
        /// </summary>
        private const float CurvatureArc = 2f;

        /// <summary>
        /// Rayon de courbure lu comme un VIRAGE : angle parcouru par unite de longueur d'arc, soit
        /// <c>R = arc / angle</c>. Exact sur un arc de cercle, et surtout INSENSIBLE a la densite
        /// d'echantillonnage -- ce que le cercle passant par trois points voisins n'est pas.
        ///
        /// C'est ce qui rendait le plafond de vitesse nerveux : la trajectoire echantillonne les
        /// courbes au metre et les lignes droites tous les quatre metres, donc a chaque raccord les
        /// trois points voisins etaient espaces de 4, 1 et 1 m. Le cercle qui les traverse y annonce
        /// un rayon tres serre sur une portion pourtant droite, le vehicule freinait sec, puis
        /// repartait au segment suivant : l'a-coup rapporte en recette dans les giratoires.
        /// </summary>
        public static float ResolveRadiusOverArc(Vector3[] path, int count, int index, float arc)
        {
            if (path == null || index < 0) return float.PositiveInfinity;
            count = Mathf.Min(count, path.Length);
            if (index + 1 >= count) return float.PositiveInfinity;
            arc = Mathf.Max(0.5f, arc);

            var from = Vector3.ProjectOnPlane(path[index + 1] - path[index], Vector3.up);
            if (from.sqrMagnitude <= 0.000001f) return float.PositiveInfinity;
            from.Normalize();

            // L'angle et la longueur doivent porter sur LA MEME portion : on cumule le virage segment
            // par segment, et la longueur des segments dont on a compte le virage. Rapporter un
            // virage d'un segment a la longueur de deux en doublait le rayon lu -- 8,6 m annonces sur
            // l'anneau de 6,0 m des giratoires.
            var travelled = 0f;
            var turn = 0f;
            var previous = from;
            for (var i = index + 1; i + 1 < count && travelled < arc; i++)
            {
                var leg = Vector3.ProjectOnPlane(path[i + 1] - path[i], Vector3.up);
                var length = leg.magnitude;
                if (length <= 0.0001f) continue;
                var direction = leg / length;
                turn += Vector3.Angle(previous, direction) * Mathf.Deg2Rad;
                travelled += length;
                previous = direction;
            }

            if (travelled <= 0.0001f || turn <= 0.0001f) return float.PositiveInfinity;
            return travelled / turn;
        }

        /// <summary>
        /// Croisement planaire de deux segments, avec le POINT. C est la brique du calcul de la
        /// frontiere de conflit d une jonction : le premier endroit ou notre mouvement rencontre celui
        /// d une autre approche. Les segments colineaires rendent faux -- une fusion de voies n est pas
        /// un croisement, et elle releve de la place en sortie.
        /// </summary>
        public static bool TrySegmentIntersection(Vector3 firstStart, Vector3 firstEnd,
            Vector3 secondStart, Vector3 secondEnd, out Vector3 point)
        {
            point = Vector3.zero;
            var first = Vector3.ProjectOnPlane(firstEnd - firstStart, Vector3.up);
            var second = Vector3.ProjectOnPlane(secondEnd - secondStart, Vector3.up);
            var offset = Vector3.ProjectOnPlane(secondStart - firstStart, Vector3.up);
            var denominator = Cross(first, second);
            if (Mathf.Abs(denominator) < 0.0001f) return false;

            var firstT = Cross(offset, second) / denominator;
            var secondT = Cross(offset, first) / denominator;
            if (!float.IsFinite(firstT) || !float.IsFinite(secondT)) return false;

            // Tolerance sur les EXTREMITES. Une polyligne de mouvement rencontre tres souvent l'autre
            // exactement a un sommet -- un tourne-a-gauche qui rejoint la voie qu'un autre parcourt
            // tout droit, par exemple. Les coordonnees venant d'instances de prefab composees, le
            // parametre y vaut 1,0000004 plutot que 1, et le croisement etait alors manque : mesure du
            // 2026-09-21, l'approche 28 du district lisait sa frontiere de conflit a 10,00 m au lieu
            // de 6,00. La tolerance vaut 2 cm sur un segment de 10 m.
            const float endpointTolerance = 0.002f;
            if (firstT < -endpointTolerance || firstT > 1f + endpointTolerance
                || secondT < -endpointTolerance || secondT > 1f + endpointTolerance) return false;

            point = firstStart + first * Mathf.Clamp01(firstT);
            return true;
        }

        private static float Cross(Vector3 first, Vector3 second)
        {
            return first.x * second.z - first.z * second.x;
        }

        /// <summary>Predicat pur : ramene tout index (negatif inclus) dans [0, count) en bouclant.</summary>
        public static int NormalizeIndex(int index, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            var wrapped = index % count;
            return wrapped < 0 ? wrapped + count : wrapped;
        }

        /// <summary>Predicat pur : index du repere le plus proche en distance planaire, 0 si la liste est vide.</summary>
        public static int NearestIndex(Vector3 position, IReadOnlyList<Vector3> positions)
        {
            var best = 0;
            var bestSquared = float.PositiveInfinity;
            var count = positions != null ? positions.Count : 0;

            for (var i = 0; i < count; i++)
            {
                var offset = positions[i] - position;
                offset.y = 0f;
                if (offset.sqrMagnitude < bestSquared)
                {
                    bestSquared = offset.sqrMagnitude;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>
        /// Tirage deterministe dans [0, 1[ : la graine vient du <c>NetworkObjectId</c> du vehicule
        /// (stable, distinct par instance, attribue par l'hote) et le pas du nombre d'aretes deja
        /// parcourues. Jamais <c>UnityEngine.Random</c> : deux executions de la meme session
        /// produisent le meme parcours, et deux vehicules inseres au meme portail divergent.
        /// Melangeur splitmix64, choisi pour disperser des graines consecutives (1, 2, 3...) des le
        /// premier pas -- un simple produit lineaire les laisserait correlees.
        /// </summary>
        public static float DeterministicUnitSample(ulong seed, int step)
        {
            unchecked
            {
                var hash = seed * 0x9E3779B97F4A7C15UL;
                hash += (ulong)(uint)step * 0xBF58476D1CE4E5B9UL;
                hash ^= hash >> 30;
                hash *= 0xBF58476D1CE4E5B9UL;
                hash ^= hash >> 27;
                hash *= 0x94D049BB133111EBUL;
                hash ^= hash >> 31;

                // 53 bits sur 2^53 : la valeur reste strictement sous 1 apres conversion en float.
                return (float)((hash >> 11) * (1.0 / 9007199254740992.0));
            }
        }

        /// <summary>
        /// Tirage de virage pondere a une jonction, tous les successeurs eligibles. Rend la valeur
        /// d'un element de <paramref name="successors"/> (un index de noeud du graphe), ou -1 si la
        /// liste est vide. Poids absents, non finis ou de somme nulle : premier successeur retenu et
        /// <paramref name="weightsInvalid"/> leve, pour que l'appelant avertisse une fois -- jamais
        /// de division par zero.
        /// </summary>
        public static int SelectWeightedSuccessor(
            IReadOnlyList<int> successors,
            IReadOnlyList<float> weights,
            ulong seed,
            int step,
            out bool weightsInvalid)
        {
            return SelectWeightedSuccessor(successors, weights, null, seed, step, out weightsInvalid);
        }

        /// <summary>
        /// Tirage de virage pondere a une jonction, restreint a un sous-ensemble ELIGIBLE fourni par
        /// l'appelant (correctif post-livraison 2026-09-16). <paramref name="eligibleCandidates"/> est
        /// parallele a <paramref name="successors"/> ; <c>null</c> ou plus court que la liste veut
        /// dire "eligible" -- une restriction absente ne restreint rien. Le sous-ensemble vient donc
        /// entierement de l'appelant, la ponderation reste celle des ratios authores sur le noeud, et
        /// l'absence de candidat eligible rend -1 pour que l'appelant reoriente.
        ///
        /// Fonction toujours pure et toujours statique : elle ne sait rien de la notion de noeud
        /// parcouru, elle sait seulement qu'un candidat donne est eligible ou non. C'est ce qui la
        /// garde testable en EditMode sans scene, sans Netcode, et sans le driver.
        ///
        /// L'ordre de parcours et le tirage restent inchanges quand tous les candidats sont
        /// eligibles : la suite est identique a celle de la surcharge sans restriction, ce qui
        /// preserve les ratios authores et le determinisme par graine.
        /// </summary>
        public static int SelectWeightedSuccessor(
            IReadOnlyList<int> successors,
            IReadOnlyList<float> weights,
            IReadOnlyList<bool> eligibleCandidates,
            ulong seed,
            int step,
            out bool weightsInvalid)
        {
            weightsInvalid = false;

            var count = successors != null ? successors.Count : 0;
            if (count <= 0)
            {
                return -1;
            }

            var eligibleCount = 0;
            var total = 0f;
            var firstEligible = -1;
            var lastEligible = -1;
            for (var i = 0; i < count; i++)
            {
                if (!IsEligible(eligibleCandidates, i))
                {
                    continue;
                }

                eligibleCount++;
                total += ResolveWeight(weights, i);
                lastEligible = i;
                if (firstEligible < 0)
                {
                    firstEligible = i;
                }
            }

            if (eligibleCount <= 0)
            {
                // Aucun candidat eligible : c'est a l'appelant de reorienter. Aucun successeur
                // ineligible n'est jamais rendu ici.
                return -1;
            }

            if (eligibleCount == 1)
            {
                return successors[firstEligible];
            }

            if (total <= 0f)
            {
                weightsInvalid = true;
                return successors[firstEligible];
            }

            var roll = DeterministicUnitSample(seed, step) * total;
            var cursor = 0f;
            for (var i = 0; i < count; i++)
            {
                if (!IsEligible(eligibleCandidates, i))
                {
                    continue;
                }

                cursor += ResolveWeight(weights, i);
                if (roll < cursor)
                {
                    return successors[i];
                }
            }

            return successors[lastEligible];
        }

        private static bool IsEligible(IReadOnlyList<bool> eligibleCandidates, int index)
        {
            return eligibleCandidates == null || index >= eligibleCandidates.Count || eligibleCandidates[index];
        }

        private static float ResolveWeight(IReadOnlyList<float> weights, int index)
        {
            if (weights == null || index >= weights.Count)
            {
                return 0f;
            }

            var weight = weights[index];
            return float.IsFinite(weight) && weight > 0f ? weight : 0f;
        }

        /// <summary>
        /// Garde-fou d'errance (<c>--max-edges-factor</c> de jtrrouter, defaut 2) : un parcours qui a
        /// traverse plus de <paramref name="edgeBudgetFactor"/> fois le nombre de noeuds du graphe
        /// n'aboutira vraisemblablement pas par le tirage seul et doit etre reoriente vers la sortie
        /// la plus proche. Facteur non fini ou nul : aucun budget, le parcours n'est jamais reoriente
        /// pour cette raison (la reorientation n'est pas un retrait, donc jamais urgente).
        /// </summary>
        public static bool IsEdgeBudgetExceeded(int traversedEdges, int graphNodeCount, float edgeBudgetFactor)
        {
            if (graphNodeCount <= 0 || !float.IsFinite(edgeBudgetFactor) || edgeBudgetFactor <= 0f)
            {
                return false;
            }

            return traversedEdges > Mathf.CeilToInt(edgeBudgetFactor * graphNodeCount);
        }

        /// <summary>
        /// Premier pas d'un plus court chemin dirige vers n'importe quelle sortie accessible.
        /// Rend -1 quand aucune sortie n'est atteignable : l'appelant reste alors sur place au lieu
        /// de sauter vers un portail seulement proche dans l'espace.
        /// </summary>
        public static int FindNextTowardReachableExit(
            int current,
            int nodeCount,
            Func<int, IReadOnlyList<int>> getSuccessors,
            Func<int, bool> isExit)
        {
            if (current < 0 || current >= nodeCount || getSuccessors == null || isExit == null)
            {
                return -1;
            }

            if (isExit(current))
            {
                return current;
            }

            var parent = new int[nodeCount];
            for (var i = 0; i < parent.Length; i++) parent[i] = -2;
            var queue = new int[nodeCount];
            var head = 0;
            var tail = 0;
            parent[current] = -1;
            queue[tail++] = current;

            var exit = -1;
            while (head < tail && exit < 0)
            {
                var node = queue[head++];
                var successors = getSuccessors(node);
                for (var i = 0; successors != null && i < successors.Count; i++)
                {
                    var candidate = successors[i];
                    if (candidate < 0 || candidate >= nodeCount || parent[candidate] != -2) continue;
                    parent[candidate] = node;
                    if (isExit(candidate))
                    {
                        exit = candidate;
                        break;
                    }

                    queue[tail++] = candidate;
                }
            }

            if (exit < 0) return -1;
            while (parent[exit] != current) exit = parent[exit];
            return exit;
        }

        /// <summary>
        /// Predicat pur (ANO-5.10-02) : un repere DEJA DEPASSE et situe a l'interieur du cercle de
        /// braquage du vehicule ne sera jamais rattrape par la poursuite pure -- le vehicule tourne
        /// autour de lui indefiniment sans jamais entrer dans son rayon d'arrivee. C'est exactement
        /// l'orbite observee : la cible se stabilise par le travers (+/-90 degres), a une distance
        /// egale au rayon de braquage, et le franchissement de noeud ne se produit plus jamais.
        ///
        /// Le rayon vient de la vitesse courante et de la vitesse de lacet maximale (R = v / w) :
        /// aucun seuil supplementaire a authorer, et le predicat se reduit tout seul la ou il doit --
        /// a basse vitesse le vehicule braque assez court pour revenir sur son repere, et un point
        /// interieur au cercle etant a au plus 2R, un repere lointain n'est jamais concerne (un
        /// vehicule retourne a l'autre bout du district revient le chercher, comme aujourd'hui).
        ///
        /// La garde "deja depasse" est ce qui separe l'orbite d'une approche serree normale : sur un
        /// giratoire, le noeud suivant reste DEVANT le vehicule, donc ce predicat ne s'y declenche
        /// pas et le trace du giratoire reste suivi noeud par noeud.
        /// </summary>
        public static bool HasPassedUnreachableWaypoint(
            Vector3 position,
            Vector3 forward,
            Vector3 waypointPosition,
            float speed,
            float maxYawDegreesPerSecond)
        {
            var toWaypoint = waypointPosition - position;
            toWaypoint.y = 0f;

            var flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude <= 0.0001f || toWaypoint.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            flatForward.Normalize();

            // Repere encore devant : la poursuite normale s'en occupe, rien a court-circuiter.
            if (Vector3.Dot(flatForward, toWaypoint) > 0f)
            {
                return false;
            }

            if (!float.IsFinite(speed) || !float.IsFinite(maxYawDegreesPerSecond)
                || speed <= 0f || maxYawDegreesPerSecond <= 0f)
            {
                return false;
            }

            // Centre du virage du cote du repere : c'est celui que le vehicule va decrire, puisqu'il
            // braque vers sa cible. Un repere a l'interieur de ce cercle est hors d'atteinte.
            var turnRadius = speed / (maxYawDegreesPerSecond * Mathf.Deg2Rad);
            var right = Vector3.Cross(Vector3.up, flatForward);
            var side = Vector3.Dot(right, toWaypoint) >= 0f ? 1f : -1f;
            var center = right * (side * turnRadius);

            return (toWaypoint - center).sqrMagnitude < turnRadius * turnRadius;
        }

        /// <summary>
        /// Predicat pur (Story 5.12, rendu CONTINU par la Story 5.13) : point de visee anticipe du suivi
        /// de trajectoire. Viser la POSITION d'un noeud fait basculer la cible d'un noeud au suivant
        /// d'un coup -- et comme le basculement se produit a <c>arrivalRadius</c> du noeud, le vehicule
        /// se met a viser en diagonale un point situe de l'autre cote de la jonction : il coupe
        /// l'interieur du virage, d'une profondeur exactement bornee par ce rayon.
        ///
        /// Ici la cible IDEALE se deplace CONTINUMENT : tant que le noeud vise est plus loin que la
        /// distance de visee, la cible reste le noeud (viser un point de la meme droite ne change pas le
        /// cap) ; des que le vehicule en est plus pres, la cible glisse au-dela du noeud, le long de son
        /// SENS DE CIRCULATION authore. A la distance de visee exacte, les deux formules se rejoignent :
        /// aucune discontinuite dans la loi elle-meme.
        ///
        /// **Le saut qui restait, et d'ou il venait.** La loi ci-dessus est continue tant que le NOEUD
        /// vise ne change pas -- et il change, par construction, quand le vehicule entre dans
        /// <c>arrivalRadius</c>. A ce moment la cible ideale saute de la branche sortante du noeud
        /// quitte a celle du noeud suivant : jusqu'a deux fois la distance de visee (4,8 m a 8 m/s avec
        /// la duree livree), donc un echelon de consigne de direction d'un pas de physique a l'autre.
        ///
        /// <paramref name="previousAimPoint"/> et <paramref name="maximumStepDistance"/> referment cet
        /// echelon : la cible rendue part du point precedent et avance VERS la cible ideale d'au plus un
        /// pas. La continuite est alors une propriete de la fonction, pas une esperance -- la position
        /// rendue ne peut jamais s'ecarter de plus d'un pas de la precedente, quelle que soit la
        /// discontinuite de la cible ideale.
        ///
        /// Le rappel est INERTE a un pas maximal nul ou non fini : la cible rendue est alors exactement
        /// la cible ideale, c'est-a-dire la loi de la Story 5.12. C'est le patron de desactivation deja
        /// en place dans la couche (valeur nulle = terme inerte), et c'est ce qui permet de mesurer les
        /// deux lois sur les memes trajectoires.
        ///
        /// Le sens de circulation vient du noeud lui-meme (<see cref="LaneGraph.GetNodeRotation"/>),
        /// pas d'un successeur tire : le tirage de virage n'est fait qu'a l'arrivee, et deviner
        /// maintenant lequel sera tire reviendrait a decider deux fois.
        ///
        /// Aucune mutation : c'est une lecture de geometrie, et la memoire de la cible appartient a
        /// l'appelant.
        /// </summary>
        public static Vector3 ResolveLookAheadPoint(
            Vector3 position,
            Vector3 waypointPosition,
            Vector3 waypointForward,
            float lookAheadDistance,
            Vector3 previousAimPoint,
            float maximumStepDistance)
        {
            var ideal = ResolveIdealLookAheadPoint(position, waypointPosition, waypointForward, lookAheadDistance);

            // Un point IDEAL non fini ne peut venir que d'une ENTREE non finie : les operations de
            // cette fonction sont bornees. Le repli rend la position du noeud (l'entree brute) plutot
            // qu'une valeur inventee, et il evite qu'un rappel en fabrique une seconde a partir d'elle.
            if (!float.IsFinite(ideal.x) || !float.IsFinite(ideal.y) || !float.IsFinite(ideal.z))
            {
                ideal = waypointPosition;
            }

            return RecallAimPoint(previousAimPoint, ideal, maximumStepDistance);
        }

        /// <summary>
        /// Point de visee SUR la trajectoire : projection planaire du vehicule sur la polyligne, puis
        /// avance de <paramref name="distance"/> le long d'elle.
        ///
        /// Il remplace <see cref="ResolveIdealLookAheadPoint"/>, qui prolongeait le noeud vise EN LIGNE
        /// DROITE le long de sa tangente. Sur une voie courbe cette droite quitte la courbe par
        /// l'exterieur : mesure ANO-5.18-04 sur l'anneau de 6,00 m du giratoire nord-est, le point vise
        /// se trouvait 1,68 m HORS de l'anneau au passage de chaque noeud, pour une demi-voie de 2,00 m.
        /// Le vehicule visait donc la bordure, et l'anneau se parcourait par l'exterieur.
        ///
        /// La polyligne peut dater du dernier tour de perception : c'est pourquoi la fonction se
        /// REPROJETTE au lieu de compter depuis son origine. Elle rend aussi une visee utilisable quand
        /// le vehicule s'est ecarte de sa voie, ce qui est precisement le moment ou elle compte.
        /// </summary>
        /// <summary>
        /// Tangente de la polyligne a la projection de <paramref name="position"/>, ou
        /// <see cref="Vector3.zero"/> si elle n'est pas exploitable.
        ///
        /// C'est la seule reference de sens utilisable EN CONTINU. L'axe routier le plus proche
        /// (<c>LaneGraph.TryGetRoadPosition</c>) rend l'arete la plus proche quelle que soit sa
        /// direction : au milieu d'un carrefour, c'est souvent une arete transversale ou opposee,
        /// et un vehicule parfaitement correct s'y declare a contresens. La tangente de SA PROPRE
        /// trajectoire, elle, est la direction que le vehicule est cense suivre a cet instant --
        /// y compris a l'interieur d'un connecteur de virage.
        /// </summary>
        public static Vector3 ResolvePathTangent(Vector3[] path, int count, Vector3 position)
        {
            if (path == null || count < 2) return Vector3.zero;
            count = Mathf.Min(count, path.Length);

            var bestSegment = -1;
            var bestSquared = float.PositiveInfinity;
            for (var i = 0; i + 1 < count; i++)
            {
                var segment = Vector3.ProjectOnPlane(path[i + 1] - path[i], Vector3.up);
                var length = segment.magnitude;
                if (length <= 0.0001f) continue;
                var fraction = Mathf.Clamp01(
                    Vector3.Dot(Vector3.ProjectOnPlane(position - path[i], Vector3.up), segment) / (length * length));
                var squared = Vector3.ProjectOnPlane(path[i] + segment * fraction - position, Vector3.up).sqrMagnitude;
                if (squared >= bestSquared) continue;
                bestSquared = squared;
                bestSegment = i;
            }

            if (bestSegment < 0) return Vector3.zero;
            return Vector3.ProjectOnPlane(path[bestSegment + 1] - path[bestSegment], Vector3.up).normalized;
        }

        /// <summary>
        /// Abscisse curviligne d'un point sur la polyligne : longueur d'arc depuis son origine
        /// jusqu'a la projection de <paramref name="position"/>. Rend -1 si la polyligne est
        /// inexploitable.
        /// </summary>
        public static float ResolvePathArcLength(Vector3[] path, int count, Vector3 position)
        {
            if (path == null || count < 2) return -1f;
            count = Mathf.Min(count, path.Length);

            var bestSegment = -1;
            var bestFraction = 0f;
            var bestSquared = float.PositiveInfinity;
            for (var i = 0; i + 1 < count; i++)
            {
                var segment = Vector3.ProjectOnPlane(path[i + 1] - path[i], Vector3.up);
                var length = segment.magnitude;
                if (length <= 0.0001f) continue;
                var fraction = Mathf.Clamp01(
                    Vector3.Dot(Vector3.ProjectOnPlane(position - path[i], Vector3.up), segment) / (length * length));
                var squared = Vector3.ProjectOnPlane(path[i] + segment * fraction - position, Vector3.up).sqrMagnitude;
                if (squared >= bestSquared) continue;
                bestSquared = squared;
                bestSegment = i;
                bestFraction = fraction;
            }

            if (bestSegment < 0) return -1f;

            var arc = 0f;
            for (var i = 0; i < bestSegment; i++)
            {
                arc += Vector3.ProjectOnPlane(path[i + 1] - path[i], Vector3.up).magnitude;
            }

            return arc + Vector3.ProjectOnPlane(path[bestSegment + 1] - path[bestSegment], Vector3.up).magnitude * bestFraction;
        }

        /// <summary>
        /// Distance RESTANTE LE LONG DE LA TRAJECTOIRE entre <paramref name="position"/> et
        /// <paramref name="target"/>, les deux projetes sur la polyligne. Rend -1 quand la mesure
        /// n'est pas exploitable (polyligne trop courte, ou cible deja derriere).
        ///
        /// C'est la mesure de progres que la distance euclidienne au waypoint ne sait pas donner.
        /// En ligne droite les deux coincident ; en courbe, en giratoire et pendant une manoeuvre,
        /// la distance a vol d'oiseau peut stagner ou CROITRE alors que le vehicule avance
        /// normalement sur son arc -- l'horloge de blocage se declenchait alors sur un vehicule qui
        /// roulait, et inversement ne se declenchait pas sur un vehicule qui tournait sur place a
        /// distance constante de son waypoint.
        /// </summary>
        public static float ResolvePathRemainingDistance(Vector3[] path, int count, Vector3 position, Vector3 target)
        {
            var here = ResolvePathArcLength(path, count, position);
            if (here < 0f) return -1f;
            var there = ResolvePathArcLength(path, count, target);
            if (there < 0f) return -1f;
            var remaining = there - here;
            return remaining > 0f ? remaining : -1f;
        }

        public static Vector3 ResolvePathLookAheadPoint(Vector3[] path, int count, Vector3 position, float distance)
        {
            if (path == null || count < 2) return position;
            count = Mathf.Min(count, path.Length);
            if (!float.IsFinite(distance) || distance < 0f) distance = 0f;

            // 1. Projection : le segment le plus proche, et ou sur lui.
            var bestSegment = 0;
            var bestFraction = 0f;
            var bestSquared = float.PositiveInfinity;
            for (var i = 0; i + 1 < count; i++)
            {
                var segment = Vector3.ProjectOnPlane(path[i + 1] - path[i], Vector3.up);
                var length = segment.magnitude;
                if (length <= 0.0001f) continue;
                var fraction = Mathf.Clamp01(
                    Vector3.Dot(Vector3.ProjectOnPlane(position - path[i], Vector3.up), segment) / (length * length));
                var squared = Vector3.ProjectOnPlane(path[i] + segment * fraction - position, Vector3.up).sqrMagnitude;
                if (squared >= bestSquared) continue;
                bestSquared = squared;
                bestSegment = i;
                bestFraction = fraction;
            }

            // 2. Avance le long de la polyligne depuis cette projection.
            var from = Vector3.Lerp(path[bestSegment], path[bestSegment + 1], bestFraction);
            var remaining = distance;
            for (var i = bestSegment; i + 1 < count; i++)
            {
                var to = path[i + 1];
                var leg = Vector3.ProjectOnPlane(to - from, Vector3.up).magnitude;
                if (leg >= remaining)
                {
                    return leg <= 0.0001f ? to : Vector3.Lerp(from, to, remaining / leg);
                }

                remaining -= leg;
                from = to;
            }

            // 3. Trajectoire plus courte que la visee : on prolonge sa derniere tangente, faute de mieux.
            var tail = Vector3.ProjectOnPlane(path[count - 1] - path[Mathf.Max(0, count - 2)], Vector3.up);
            return tail.sqrMagnitude <= 0.0001f ? path[count - 1] : path[count - 1] + tail.normalized * remaining;
        }

        /// <summary>
        /// Rayon de courbure de la polyligne au point <paramref name="index"/> : le cercle passant par
        /// ses deux voisins. Trois points alignes rendent l'infini, ce qui est bien le rayon d'une
        /// ligne droite.
        /// </summary>
        public static float ResolvePathRadius(Vector3[] path, int count, int index)
        {
            if (path == null || index <= 0 || index + 1 >= Mathf.Min(count, path.Length)) return float.PositiveInfinity;
            var a = Vector3.ProjectOnPlane(path[index - 1], Vector3.up);
            var b = Vector3.ProjectOnPlane(path[index], Vector3.up);
            var c = Vector3.ProjectOnPlane(path[index + 1], Vector3.up);
            var ab = b - a;
            var bc = c - b;
            var ca = a - c;
            var area = Mathf.Abs(ab.x * ca.z - ca.x * ab.z) * 0.5f;
            if (area <= 0.000001f) return float.PositiveInfinity;
            return ab.magnitude * bc.magnitude * ca.magnitude / (4f * area);
        }

        /// <summary>Point de visee IDEAL d'un noeud donne, sans memoire : la loi de la Story 5.12.</summary>
        private static Vector3 ResolveIdealLookAheadPoint(
            Vector3 position,
            Vector3 waypointPosition,
            Vector3 waypointForward,
            float lookAheadDistance)
        {
            var toWaypoint = waypointPosition - position;
            toWaypoint.y = 0f;

            if (!float.IsFinite(lookAheadDistance) || lookAheadDistance <= 0f)
            {
                return waypointPosition;
            }

            var distance = toWaypoint.magnitude;
            if (!float.IsFinite(distance) || distance >= lookAheadDistance)
            {
                return waypointPosition;
            }

            var flatForward = new Vector3(waypointForward.x, 0f, waypointForward.z);
            if (!float.IsFinite(flatForward.x) || !float.IsFinite(flatForward.z) || flatForward.sqrMagnitude <= 0.0001f)
            {
                return waypointPosition;
            }

            flatForward.Normalize();
            return waypointPosition + (flatForward * (lookAheadDistance - distance));
        }

        /// <summary>
        /// Rappel BORNE du point de visee vers sa cible ideale : c'est la partie de la fonction qui rend
        /// la visee continue au passage de noeud. Le point rendu se deplace de la position precedente
        /// vers la cible ideale, d'au plus <paramref name="maximumStepDistance"/> -- donc jamais d'un
        /// saut, quelle que soit la discontinuite de la cible.
        ///
        /// Un pas maximal nul, negatif ou non fini rend le rappel INERTE (la cible ideale telle quelle),
        /// et une memoire non finie est ignoree de la meme facon : dans les deux cas la fonction rend une
        /// valeur utilisable plutot qu'un point invente ou un NaN.
        /// </summary>
        private static Vector3 RecallAimPoint(Vector3 previousAimPoint, Vector3 idealAimPoint, float maximumStepDistance)
        {
            if (!float.IsFinite(maximumStepDistance) || maximumStepDistance <= 0f)
            {
                return idealAimPoint;
            }

            if (!float.IsFinite(previousAimPoint.x) || !float.IsFinite(previousAimPoint.y) || !float.IsFinite(previousAimPoint.z))
            {
                return idealAimPoint;
            }

            var offset = idealAimPoint - previousAimPoint;
            var distance = offset.magnitude;

            if (!float.IsFinite(distance) || distance <= maximumStepDistance)
            {
                return idealAimPoint;
            }

            return previousAimPoint + (offset * (maximumStepDistance / distance));
        }

        /// <summary>
        /// Descente gloutonne vers une cible : parmi les successeurs, celui dont la position est la
        /// plus proche de <paramref name="target"/>. C'est la reorientation "vers la sortie la plus
        /// proche" -- un pas a la fois, sans A* ni itineraire pre-calcule. Rend la valeur d'un element
        /// de <paramref name="successors"/>, ou -1 si la liste est vide.
        /// </summary>
        public static int SelectSuccessorTowardTarget(
            IReadOnlyList<int> successors,
            IReadOnlyList<Vector3> successorPositions,
            Vector3 target)
        {
            var count = successors != null ? successors.Count : 0;
            if (count <= 0)
            {
                return -1;
            }

            var best = successors[0];
            var bestSquared = float.PositiveInfinity;

            for (var i = 0; i < count; i++)
            {
                if (successorPositions == null || i >= successorPositions.Count)
                {
                    break;
                }

                var offset = successorPositions[i] - target;
                offset.y = 0f;
                if (offset.sqrMagnitude < bestSquared)
                {
                    bestSquared = offset.sqrMagnitude;
                    best = successors[i];
                }
            }

            return best;
        }
    }
}
