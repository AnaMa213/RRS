using System.Collections;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.PassengerActions;
using TMPro;
using Unity.Netcode;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoadRage.Tests.PlayMode
{
    [Category("Story44")]
    public sealed class Story44PassengerActionTwoMvpRunPlayModeTests
    {
        /// <summary>
        /// Dev_RageSandbox demarre son propre hote (RageSandboxAutoStart.StartHost()) des qu'aucun
        /// NetworkManager n'ecoute deja. NetworkManager gere sa propre survie (DontDestroyOnLoad)
        /// independamment de la scene : sans arret explicite ici, cet hote reste actif et pollue les
        /// fixtures suivantes qui verifient desormais l'etat reseau (Story 5.3, meme risque que
        /// Story15EmptyMapEntryPlayModeTests).
        /// </summary>
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                if (manager.IsListening)
                {
                    manager.Shutdown();
                }

                Object.Destroy(manager.gameObject);
            }

            yield return null;
        }

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
