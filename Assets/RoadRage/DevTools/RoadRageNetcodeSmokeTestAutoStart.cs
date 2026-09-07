using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.DevTools
{
    /// <summary>
    /// Story 0.8 (VAL-027/VAL-028) auto-start helper for the Dev_LobbySmokeTest NetworkManager
    /// harness. Multiplayer Play Mode "Additional Editor Instance" windows (e.g. "Player 2")
    /// show no menu bar and do not receive the Main Editor's keyboard shortcuts, so this uses
    /// the Play Mode Scenario player tag to decide the role automatically on Play: the
    /// untagged Editor player starts the host, a player tagged "Client" starts the client.
    /// Editor-only behavior. The assembly is runtime-compatible so Unity can resolve this
    /// MonoBehaviour when it is attached to a scene object.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoadRageNetcodeSmokeTestAutoStart : MonoBehaviour
    {
        private const string ClientTag = "Client";

#if UNITY_EDITOR
        private IEnumerator Start()
        {
            yield return null;

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[RoadRageNetcodeSmokeTestAutoStart] No NetworkManager.Singleton found.");
                yield break;
            }

            var manager = NetworkManager.Singleton;
            if (manager.IsListening)
            {
                Debug.Log("[RoadRageNetcodeSmokeTestAutoStart] NetworkManager already listening; auto-start skipped.");
                yield break;
            }

            var tags = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags;
            var tagList = tags == null ? string.Empty : string.Join(",", tags);
            var hasClientTag = tags != null && tags.Any(tag => string.Equals(tag, ClientTag, StringComparison.OrdinalIgnoreCase));
            var isVirtualProject = Application.dataPath.Replace('\\', '/').Contains("/Library/VP/", StringComparison.OrdinalIgnoreCase);
            var shouldStartClient = hasClientTag || isVirtualProject;

            if (shouldStartClient)
            {
                yield return new WaitForSecondsRealtime(0.5f);

                var started = manager.StartClient();
                Debug.Log($"[RoadRageNetcodeSmokeTestAutoStart] Tags='{tagList}' VirtualProject={isVirtualProject} -> StartClient() returned {started}.");
            }
            else
            {
                var started = manager.StartHost();
                Debug.Log($"[RoadRageNetcodeSmokeTestAutoStart] Tags='{tagList}' VirtualProject={isVirtualProject} -> StartHost() returned {started}.");
            }
        }
#else
        private void Start()
        {
            enabled = false;
        }
#endif
    }
}
