using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Rage;
using RoadRage.Features.Vehicles;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    public sealed class Story43PassengerActionOneMvpRunPlayModeTests
    {
        [UnityTest]
        public IEnumerator MvpRunShowsTopRightRageHudAndMultipleRageVehicles()
        {
            yield return SceneManager.LoadSceneAsync("MVP_Run", LoadSceneMode.Single);
            yield return null;

            var flow = Object.FindAnyObjectByType<RunFlowController>();
            Assert.That(flow, Is.Not.Null);

            var targets = Object.FindObjectsByType<NetworkedRageState>(FindObjectsInactive.Exclude);
            Assert.That(targets.Length, Is.GreaterThanOrEqualTo(3));

            // Story 5.5 : la HUD n'affiche plus de cible par defaut (aucun lock global auto-seede) ;
            // on verrouille explicitement une cible ici pour verifier le format du libelle, comme un
            // joueur le ferait avec T (RunFlowController.HandleRageTargetLockControls).
            var lockCandidate = targets[0].GetComponent<NetworkedAIVehicleState>();
            if (lockCandidate == null)
            {
                lockCandidate = targets[0].gameObject.AddComponent<NetworkedAIVehicleState>();
            }

            SetPrivateField(flow, "rageTargetLock", lockCandidate);
            InvokePrivate(flow, "RefreshFocusedRageHud");

            var rageLabel = GameObject.Find("RageStatusLabel")?.GetComponent<TextMeshProUGUI>();
            Assert.That(rageLabel, Is.Not.Null);
            Assert.That(rageLabel.rectTransform.anchorMin, Is.EqualTo(new Vector2(1f, 1f)));
            Assert.That(rageLabel.text, Does.Contain("Rage"));
            Assert.That(rageLabel.text, Does.Contain("Peur"));

            var rageVehicles = 0;
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i].GetComponent<NetworkedVehicleDriverController>() != null)
                {
                    rageVehicles++;
                }
            }

            Assert.That(rageVehicles, Is.GreaterThanOrEqualTo(2));
        }

        /// <summary>
        /// Story 5.5 : le cycle de focus rage global (C) et la reaction de test Y solo-only sont
        /// remplaces par le lock rage/peur par joueur (T/Y, RunFlowController.HandleRageTargetLockControls,
        /// Story55NetworkedAiRageTargetingTests). Ce test ne couvre plus que le spawn dev, inchange.
        /// </summary>
        [UnityTest]
        public IEnumerator DevSpawnCreatesClearDrivableVehicles()
        {
            yield return SceneManager.LoadSceneAsync("MVP_Run", LoadSceneMode.Single);
            yield return null;

            var flow = Object.FindAnyObjectByType<RunFlowController>();
            Assert.That(flow, Is.Not.Null);

            var beforeVehicles = Object.FindObjectsByType<NetworkedVehicleState>(FindObjectsInactive.Exclude).Length;
            var beforeTargets = Object.FindObjectsByType<NetworkedRageState>(FindObjectsInactive.Exclude).Length;

            Assert.That(flow.TrySpawnDevVehicle(false), Is.True);
            Assert.That(flow.TrySpawnDevVehicle(true), Is.True);

            var afterVehicles = Object.FindObjectsByType<NetworkedVehicleState>(FindObjectsInactive.Exclude);
            var afterTargets = Object.FindObjectsByType<NetworkedRageState>(FindObjectsInactive.Exclude);
            Assert.That(afterVehicles.Length, Is.EqualTo(beforeVehicles + 2));
            Assert.That(afterTargets.Length, Is.EqualTo(beforeTargets + 1));
            Assert.That(GameObject.Find("DevVehicle").GetComponent<NetworkedVehicleDriverController>(), Is.Not.Null);
            Assert.That(GameObject.Find("DevRageTargetVehicle").GetComponent<NetworkedVehicleDriverController>(), Is.Not.Null);
        }

        private static void SetPrivateField(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private static void InvokePrivate(object target, string method)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
        }
    }
}
