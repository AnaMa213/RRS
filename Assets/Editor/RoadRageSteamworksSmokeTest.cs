#if UNITY_EDITOR
using System;
using Steamworks;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Editor
{
    public static class RoadRageSteamworksSmokeTest
    {
        private const uint TestSteamAppId = 480;

        [MenuItem("RoadRage/Steamworks/Run AppID 480 Smoke Test")]
        public static void Run()
        {
            var initializedForSmokeTest = false;

            try
            {
                if (!SteamClient.IsValid)
                {
                    SteamClient.Init(TestSteamAppId, false);
                    initializedForSmokeTest = true;
                }

                SteamNetworkingUtils.InitRelayNetworkAccess();
                Debug.Log("[RoadRageSteamworksSmokeTest] SteamClient.Init succeeded for AppID 480 (Spacewar); Steam account and lobby identifiers intentionally omitted.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[RoadRageSteamworksSmokeTest] SteamClient.Init failed for AppID 480. Ensure the Steam client is installed, running, and signed in. {exception.GetType().Name}: {exception.Message}");
            }
            finally
            {
                if (initializedForSmokeTest && SteamClient.IsValid)
                {
                    SteamClient.Shutdown();
                    Debug.Log("[RoadRageSteamworksSmokeTest] Steamworks smoke-test client shutdown complete.");
                }
            }
        }
    }
}
#endif
