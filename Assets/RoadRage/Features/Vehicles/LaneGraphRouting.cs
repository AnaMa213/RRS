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
