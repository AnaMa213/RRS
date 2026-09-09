using RoadRage.Features.Players;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.App.Run
{
    /// <summary>
    /// Pont local-only entre le joueur controle par l'input Story 1.5 et son proxy reseau Story 2.5.
    /// Le composant ne possede aucune verite partagee : il soumet seulement position/yaw au proxy dont
    /// le ClientId correspond au client local.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedLocalPlayerPoseReporter : MonoBehaviour
    {
        private const float MinimumReportInterval = 0.02f;

        [SerializeField]
        private float reportInterval = 0.08f;

        private NetworkedPlayerPresentation target;
        private float nextReportTime;

        private void Update()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || !manager.IsClient)
            {
                return;
            }

            if (!Represents(target, manager.LocalClientId))
            {
                target = ResolveTarget(manager.LocalClientId);
            }

            if (target == null || Time.unscaledTime < nextReportTime)
            {
                return;
            }

            nextReportTime = Time.unscaledTime + Mathf.Max(MinimumReportInterval, reportInterval);
            target.SubmitLocalPose(transform.position, transform.eulerAngles.y);
        }

        private static NetworkedPlayerPresentation ResolveTarget(ulong localClientId)
        {
            var candidates = FindObjectsByType<NetworkedPlayerPresentation>(FindObjectsInactive.Exclude);
            for (var i = 0; i < candidates.Length; i++)
            {
                if (Represents(candidates[i], localClientId))
                {
                    return candidates[i];
                }
            }

            return null;
        }

        private static bool Represents(NetworkedPlayerPresentation candidate, ulong localClientId)
        {
            return candidate != null && candidate.IsSpawned && candidate.RepresentsClient(localClientId);
        }
    }
}
