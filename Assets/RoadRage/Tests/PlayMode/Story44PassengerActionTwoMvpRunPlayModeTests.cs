using System.Collections;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.PassengerActions;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    [Category("Story44")]
    public sealed class Story44PassengerActionTwoMvpRunPlayModeTests
    {
        [UnityTest]
        public IEnumerator MvpRunProvidesSharedIncidentStateAndReadOnlyHud()
        {
            yield return SceneManager.LoadSceneAsync("MVP_Run", LoadSceneMode.Single);
            yield return null;

            Assert.That(Object.FindAnyObjectByType<NetworkedPassengerActionIncidentState>(), Is.Not.Null);

            var label = GameObject.Find("PassengerActionIncidentLabel")?.GetComponent<TextMeshProUGUI>();
            Assert.That(label, Is.Not.Null);
            Assert.That(label.text, Does.Contain("Incident"));
        }

        [UnityTest]
        public IEnumerator RageSandboxShowsTheSharedIncidentMarker()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            for (var frame = 0; frame < 8; frame++) yield return null;

            var intent = Object.FindAnyObjectByType<NetworkedPassengerActionIntent>();
            var incident = Object.FindAnyObjectByType<NetworkedPassengerActionIncidentState>();
            var view = Object.FindAnyObjectByType<PassengerActionDebugView>();
            Assert.That(intent, Is.Not.Null);
            Assert.That(incident, Is.Not.Null);
            Assert.That(view, Is.Not.Null);

            intent.RequestSlot(1);

            Assert.That(incident.ActivationCount.Value, Is.EqualTo(1));
            Assert.That(view.IncidentText, Is.EqualTo("Incident : actif (1)"));
        }
    }
}
