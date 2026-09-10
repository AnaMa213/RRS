using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.DevTools
{
    /// <summary>
    /// Story 4.1 : auto-start editeur simplifie pour Dev_RageSandbox, sur le modele de
    /// RoadRageNetcodeSmokeTestAutoStart (Dev_VehicleSandbox) mais sans harness de siege --
    /// demarre l'hote si aucun NetworkManager n'ecoute deja, pour que la scene soit jouable
    /// directement en Play Mode. Editor-only behavior.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RageSandboxAutoStart : MonoBehaviour
    {
        private const string RageSandboxSceneName = "Dev_RageSandbox";

#if UNITY_EDITOR
        private IEnumerator Start()
        {
            yield return null;

            if (!string.Equals(SceneManager.GetActiveScene().name, RageSandboxSceneName, StringComparison.Ordinal))
            {
                yield break;
            }

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[RageSandboxAutoStart] No NetworkManager.Singleton found.");
                yield break;
            }

            var manager = NetworkManager.Singleton;
            if (manager.IsListening)
            {
                Debug.Log("[RageSandboxAutoStart] NetworkManager already listening; auto-start skipped.");
                yield break;
            }

            var started = manager.StartHost();
            Debug.Log($"[RageSandboxAutoStart] StartHost() returned {started}.");
        }
#else
        private void Start()
        {
            enabled = false;
        }
#endif
    }
}
