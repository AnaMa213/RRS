using System.Collections.Generic;
using RoadRage.Shared.Domain;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.5 : resolution de cible IA rage/peur, point unique reutilise par les provocations
    /// passager (<see cref="RoadRage.App.Run.NetworkedPassengerActionIntent"/>) ET le klaxon
    /// conducteur (<see cref="NetworkedVehicleDriverController"/>) -- jamais cablee a une seule
    /// action (Boundaries). Eligible = objet portant a la fois <see cref="NetworkedAIVehicleState"/>
    /// et une source <see cref="IRageDispositionSource"/>, jamais un seul des deux.
    ///
    /// <see cref="ResolveNearestEligible"/>/<see cref="ResolveActionTarget"/>/
    /// <see cref="ResolveNextInCycle"/>/<see cref="ResolveTarget"/> restent des fonctions pures sur
    /// une liste de candidats fournie par l'appelant. Les chemins host passent requireSpawned=true ;
    /// les appels EditMode peuvent laisser sa valeur par defaut pour exercer la geometrie sans
    /// demarrer une session Netcode.
    /// </summary>
    public static class AiRageTargetResolution
    {
        public static bool IsStructurallyEligible(NetworkedAIVehicleState candidate)
        {
            return candidate != null && candidate.GetComponent<IRageDispositionSource>() != null;
        }

        private static bool IsEligible(NetworkedAIVehicleState candidate, bool requireSpawned)
        {
            return IsStructurallyEligible(candidate) && (!requireSpawned || candidate.IsSpawned);
        }

        /// <summary>Balayage de scene reel : eligible ET spawne, trie par NetworkObjectId (identique host/client) pour un cycle deterministe.</summary>
        public static NetworkedAIVehicleState[] FindEligibleCandidates()
        {
            var all = Object.FindObjectsByType<NetworkedAIVehicleState>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var eligible = new List<NetworkedAIVehicleState>(all.Length);
            for (var i = 0; i < all.Length; i++)
            {
                if (IsEligible(all[i], true))
                {
                    eligible.Add(all[i]);
                }
            }

            eligible.Sort((left, right) => left.NetworkObjectId.CompareTo(right.NetworkObjectId));
            return eligible.ToArray();
        }

        /// <summary>Plus proche eligible dans maxRange, ou null si aucun (jamais de repli hors portee).</summary>
        public static NetworkedAIVehicleState ResolveNearestEligible(
            Vector3 fromPosition,
            IReadOnlyList<NetworkedAIVehicleState> candidates,
            float maxRange,
            bool requireSpawned = false)
        {
            if (candidates == null || float.IsNaN(maxRange) || maxRange < 0f)
            {
                return null;
            }

            NetworkedAIVehicleState nearest = null;
            var nearestSqrDistance = float.PositiveInfinity;
            var maxSqrRange = maxRange * maxRange;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!IsEligible(candidate, requireSpawned))
                {
                    continue;
                }

                var sqrDistance = (candidate.transform.position - fromPosition).sqrMagnitude;
                if (sqrDistance > maxSqrRange || sqrDistance >= nearestSqrDistance)
                {
                    continue;
                }

                nearestSqrDistance = sqrDistance;
                nearest = candidate;
            }

            return nearest;
        }

        /// <summary>
        /// Lock actif (non nul) : reste la cible s'il est toujours eligible et en portee, sinon null --
        /// jamais de repli sur une autre cible (Boundaries "Toujours"). Sans lock (null) : plus proche
        /// eligible dans maxRange.
        /// </summary>
        public static NetworkedAIVehicleState ResolveActionTarget(
            Vector3 fromPosition,
            NetworkedAIVehicleState currentLock,
            IReadOnlyList<NetworkedAIVehicleState> candidates,
            float maxRange,
            bool requireSpawned = false)
        {
            if (currentLock != null)
            {
                return !float.IsNaN(maxRange)
                    && maxRange >= 0f
                    && IsEligible(currentLock, requireSpawned)
                    && (currentLock.transform.position - fromPosition).sqrMagnitude <= maxRange * maxRange
                    ? currentLock
                    : null;
            }

            return ResolveNearestEligible(fromPosition, candidates, maxRange, requireSpawned);
        }

        /// <summary>
        /// Point unique de resolution host-side reutilise par les provocations passager et le klaxon
        /// (Boundaries). lockSpecified=false : plus proche eligible dans maxRange, sans creer de lock
        /// persistant. lockSpecified=true : le lock doit lui-meme resoudre a un candidat eligible en
        /// portee ; lockedCandidate nul (reference invalide/usurpee ou hors-type envoyee par un
        /// client) ou hors-portee refuse l'action -- jamais de repli sur une autre cible.
        /// </summary>
        public static NetworkedAIVehicleState ResolveTarget(
            Vector3 fromPosition,
            bool lockSpecified,
            NetworkedAIVehicleState lockedCandidate,
            IReadOnlyList<NetworkedAIVehicleState> candidates,
            float maxRange,
            bool requireSpawned = false)
        {
            if (!lockSpecified)
            {
                return ResolveNearestEligible(fromPosition, candidates, maxRange, requireSpawned);
            }

            return lockedCandidate != null
                ? ResolveActionTarget(fromPosition, lockedCandidate, candidates, maxRange, requireSpawned)
                : null;
        }

        /// <summary>
        /// Cycle deterministe sur une liste deja triee (cf. <see cref="FindEligibleCandidates"/>) :
        /// stable (retourne le lock courant) si la liste est vide ou ne contient qu'un candidat.
        /// </summary>
        public static NetworkedAIVehicleState ResolveNextInCycle(NetworkedAIVehicleState currentLock, IReadOnlyList<NetworkedAIVehicleState> sortedCandidates)
        {
            if (sortedCandidates.Count == 0)
            {
                return currentLock;
            }

            if (currentLock == null)
            {
                return sortedCandidates[0];
            }

            for (var i = 0; i < sortedCandidates.Count; i++)
            {
                if (sortedCandidates[i] == currentLock)
                {
                    return sortedCandidates[(i + 1) % sortedCandidates.Count];
                }
            }

            return sortedCandidates[0];
        }
    }
}
