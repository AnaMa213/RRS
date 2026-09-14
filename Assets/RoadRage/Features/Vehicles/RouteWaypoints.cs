using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Geometrie de scene reutilisable pour le suivi de route IA (Story 5.2) : liste ordonnee de
    /// reperes qui boucle apres le dernier point, dans le meme esprit que le marqueur de scene
    /// VehicleRecoveryPoint (Story 3.4) -- un composant pur sans etat reseau, consomme en lecture
    /// seule par <see cref="NetworkedAIVehicleDriverController"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RouteWaypoints : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reperes ordonnes. Laisser vide pour utiliser les enfants directs de ce GameObject, dans l'ordre de la hierarchie.")]
        private Transform[] points = Array.Empty<Transform>();

        /// <summary>
        /// Reperes effectifs : le tableau serialise s'il est renseigne, sinon les enfants directs dans
        /// l'ordre de la hierarchie -- placer les waypoints sous ce GameObject suffit, aucun cablage.
        /// </summary>
        private bool UsesChildren
        {
            get { return points == null || points.Length == 0; }
        }

        public int Count
        {
            get { return UsesChildren ? transform.childCount : points.Length; }
        }

        /// <summary>Position du repere a l'index donne, boucle via <see cref="NormalizeIndex"/>.</summary>
        public Vector3 GetPosition(int index)
        {
            return GetPoint(NormalizeIndex(index, Count)).position;
        }

        private Transform GetPoint(int index)
        {
            return UsesChildren ? transform.GetChild(index) : points[index];
        }

        /// <summary>Index suivant apres arrivee, boucle apres le dernier repere.</summary>
        public int NextIndex(int index)
        {
            return NormalizeIndex(index + 1, Count);
        }

        /// <summary>
        /// Repere le plus proche de la position donnee (distance planaire) : point de depart d'un
        /// vehicule IA pose n'importe ou sur la boucle, sans index a cabler par instance.
        /// </summary>
        public int NearestIndex(Vector3 position)
        {
            var count = Count;
            var positions = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                positions[i] = GetPoint(i).position;
            }

            return NearestIndex(position, positions);
        }

        /// <summary>Predicat pur : index du repere le plus proche en distance planaire, 0 si la liste est vide.</summary>
        public static int NearestIndex(Vector3 position, IReadOnlyList<Vector3> waypointPositions)
        {
            var best = 0;
            var bestSquared = float.PositiveInfinity;
            var count = waypointPositions != null ? waypointPositions.Count : 0;

            for (var i = 0; i < count; i++)
            {
                var offset = waypointPositions[i] - position;
                offset.y = 0f;
                if (offset.sqrMagnitude < bestSquared)
                {
                    bestSquared = offset.sqrMagnitude;
                    best = i;
                }
            }

            return best;
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
    }
}
