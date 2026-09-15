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
        /// <summary>
        /// Dev_RageSandbox demarre son propre hote (RageSandboxAutoStart.StartHost()) des qu'aucun
        /// NetworkManager n'ecoute deja. NetworkManager gere sa propre survie (DontDestroyOnLoad)
        /// independamment de la scene : sans arret explicite ici, cet hote reste actif et pollue les
        /// fixtures suivantes qui verifient desormais l'etat reseau (Story 5.3, meme risque que
        /// Story15EmptyMapEntryPlayModeTests) -- par exemple "Start Game ignore : session reseau deja
        /// demarree" dans une fixture lancee juste apres celle-ci.
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
        public IEnumerator DevRageSandboxAcceptsAllThreePassengerSlots()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            for (var frame = 0; frame < 8; frame++) yield return null;

            var intent = Object.FindAnyObjectByType<NetworkedPassengerActionIntent>();
            Assert.That(intent, Is.Not.Null, "le harness doit creer une intention passager, quel que soit son clientId.");
            var state = intent.GetComponent<NetworkedPlayerState>();
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

        /// <summary>
        /// Story 5.5 : la cible d'action n'est plus un champ global auto-seede (passengerActionTarget,
        /// retire) mais un lock client-local par joueur, absent tant que T/Y n'a pas ete presse -- ce
        /// test ne verifie donc plus qu'une cible par defaut existe au chargement de la scene.
        /// </summary>
        [UnityTest]
        public IEnumerator MvpRunProvidesOfflineAndNetworkPassengerActionWiring()
        {
            yield return SceneManager.LoadSceneAsync("MVP_Run", LoadSceneMode.Single);
            yield return null;

            var flow = Object.FindAnyObjectByType<RunFlowController>();
            Assert.That(flow, Is.Not.Null);
            Assert.That(Read<PassengerActionCatalog>(flow, "passengerActionCatalog"), Is.Not.Null);
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
