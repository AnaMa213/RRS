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
