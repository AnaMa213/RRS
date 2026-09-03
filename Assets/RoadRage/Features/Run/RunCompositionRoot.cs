using UnityEngine;

namespace RoadRage.Features.Run
{
    /// <summary>
    /// Point d'ancrage de composition pour les racines serialisees du run.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunCompositionRoot : MonoBehaviour
    {
        [SerializeField]
        private Transform runtimeRoot;

        [SerializeField]
        private Transform spawnRoot;

        public Transform RuntimeRoot
        {
            get { return runtimeRoot; }
        }

        public Transform SpawnRoot
        {
            get { return spawnRoot; }
        }
    }
}
