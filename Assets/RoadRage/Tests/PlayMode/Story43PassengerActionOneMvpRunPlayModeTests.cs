using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Rage;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
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

            var rageLabel = GameObject.Find("RageStatusLabel")?.GetComponent<TextMeshProUGUI>();
            Assert.That(rageLabel, Is.Not.Null);
            Assert.That(rageLabel.rectTransform.anchorMin, Is.EqualTo(new Vector2(1f, 1f)));
            Assert.That(rageLabel.text, Does.Contain("Rage"));
            Assert.That(rageLabel.text, Does.Contain("Peur"));

            var targets = Object.FindObjectsByType<NetworkedRageState>(FindObjectsInactive.Exclude);
            Assert.That(targets.Length, Is.GreaterThanOrEqualTo(3));

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

        [UnityTest]
        public IEnumerator DevSpawnCreatesClearDrivableVehiclesAndFocusCycleChangesTarget()
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

            var focusedBefore = Read<NetworkedRageState>(flow, "passengerActionTarget");
            flow.CycleFocusedRageTarget();
            Assert.That(Read<NetworkedRageState>(flow, "passengerActionTarget"), Is.Not.EqualTo(focusedBefore));
        }

        [UnityTest]
        public IEnumerator MvpRunAppliesFearReactionToTheFocusedRageTarget()
        {
            yield return SceneManager.LoadSceneAsync("MVP_Run", LoadSceneMode.Single);
            yield return null;

            var flow = Object.FindAnyObjectByType<RunFlowController>();
            var target = Read<NetworkedRageState>(flow, "passengerActionTarget");
            var rageBefore = target.RageValue.Value;
            var fearBefore = target.FearValue.Value;

            Assert.That(flow.TryApplyFocusedRageReaction(ReactionChannel.Fear), Is.True);
            Assert.That(target.RageValue.Value, Is.EqualTo(rageBefore));
            Assert.That(target.FearValue.Value, Is.EqualTo(fearBefore + 25f));
            Assert.That(GameObject.Find("RageStatusLabel").GetComponent<TextMeshProUGUI>().text, Does.Contain("Peur 25"));
        }

        private static T Read<T>(object target, string field) where T : class
        {
            return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target) as T;
        }
    }
}
