using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.PassengerActions;
using RoadRage.Features.Players;
using RoadRage.Features.Rage;
using RoadRage.Features.Run;
using Unity.Netcode;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    public sealed class Story42PassengerActionMvpRunPlayModeTests
    {
        [UnityTest]
        public IEnumerator DevRageSandboxAcceptsAllThreePassengerSlots()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            for (var frame = 0; frame < 8; frame++) yield return null;

            var actor = GameObject.Find("PassengerActionActor");
            Assert.That(actor, Is.Not.Null);
            var state = actor.GetComponent<NetworkedPlayerState>();
            var intent = actor.GetComponent<NetworkedPassengerActionIntent>();
            var target = Object.FindAnyObjectByType<NetworkedRageState>();
            Assert.That(state.Mode.Value, Is.EqualTo(RoadRage.Shared.Domain.PlayerMode.Passenger));
            Assert.That(intent, Is.Not.Null);
            Assert.That(target, Is.Not.Null);

            var accepted = 0;
            intent.ActionValidated += (_, _, _) => accepted++;
            intent.RequestSlot(0);
            intent.RequestSlot(1);
            intent.RequestSlot(2);

            Assert.That(accepted, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator MvpRunProvidesOfflineAndNetworkPassengerActionWiring()
        {
            yield return SceneManager.LoadSceneAsync("MVP_Run", LoadSceneMode.Single);
            yield return null;

            var flow = Object.FindAnyObjectByType<RunFlowController>();
            Assert.That(flow, Is.Not.Null);
            Assert.That(Read<PassengerActionCatalog>(flow, "passengerActionCatalog"), Is.Not.Null);
            var target = Read<NetworkedRageState>(flow, "passengerActionTarget");
            Assert.That(target, Is.Not.Null);
            Assert.That(target.GetComponent<NetworkObject>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<NetworkedRunState>(), Is.Not.Null);

            var prefab = Resources.Load<GameObject>(NetworkedPlayerSpawnService.PlayerRootResourceName);
            Assert.That(prefab.GetComponent<NetworkedPassengerActionIntent>(), Is.Not.Null,
                "Le meme composant porte le chemin direct hors ligne et la Rpc client vers l'hote.");
        }

        private static T Read<T>(object target, string field) where T : class
        {
            return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target) as T;
        }
    }
}
