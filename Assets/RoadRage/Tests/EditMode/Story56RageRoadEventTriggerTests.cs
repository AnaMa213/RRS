using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Rage;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using TMPro;
using Unity.Netcode;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.6 : declenchement de l'evenement Rage Road. Couvre la condition authorée
    /// (ConfrontationCapable, sans seuil propre), l'arbitrage premier-arrive-gagne sur une liste deja
    /// ordonnee par NetworkObjectId, l'anti-doublon porte par l'etat, la table de transitions AD-22,
    /// le signalement de cible perdue sans reset ni retargeting, le retour textuel (HUD + prefixe
    /// [RAGE ROAD]) et le cablage de scene.
    ///
    /// Deterministe et sans Netcode, comme Story54/Story55 : les doubles sont des `new GameObject` +
    /// `AddComponent`, jamais `.Spawn()`, donc <c>IsServer</c>/<c>IsSpawned</c> ne sont pas exerçables
    /// ici. Le cablage host-only du controleur (garde d'autorite, ecriture unique) est donc verifie par
    /// assertions sur le source, idiome deja utilise par ces deux stories.
    /// </summary>
    [Category("Core")]
    public sealed class Story56RageRoadEventTriggerTests
    {
        private const string LifecycleSourcePath = "Assets/RoadRage/Features/Run/RageRoadEventLifecycle.cs";
        private const string FlowControllerSourcePath = "Assets/RoadRage/App/Run/RageRoadEventFlowController.cs";
        private const string RunStateSourcePath = "Assets/RoadRage/Features/Run/NetworkedRunState.cs";
        private const string AiVehicleStateSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs";
        private const string DebugViewSourcePath = "Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

        private static readonly RageDisposition[] AllDispositions =
        {
            RageDisposition.Calm,
            RageDisposition.Irritated,
            RageDisposition.Flee,
            RageDisposition.Block,
            RageDisposition.Ram,
            RageDisposition.ConfrontationCapable
        };

        private readonly List<Object> spawned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in spawned)
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }

            spawned.Clear();
        }

        // ---------------------------------------------------------------- Condition de declenchement

        [Test]
        public void TriggerConditionIsTheAuthoredConfrontationCapableDispositionOnly()
        {
            foreach (var disposition in AllDispositions)
            {
                var expected = disposition == RageDisposition.ConfrontationCapable;
                Assert.That(RageRoadEventLifecycle.IsTriggerConditionMet(disposition), Is.EqualTo(expected), disposition.ToString());
            }

            var source = CodeOnly(File.ReadAllText(LifecycleSourcePath));
            Assert.That(source, Does.Not.Contain("RageTuningDef"),
                "Aucun seuil propre au Rage Road : la condition reste la disposition deja derivee des paliers de RageTuningDef.");
            Assert.That(source, Does.Not.Contain("RageValue"),
                "Features/Run ne rejoue pas la rage : une seule machine, lue par IRageDispositionSource cote App/Run.");
        }

        // ---------------------------------------------------------------- Declenchement et arbitrage

        [Test]
        public void IdleEventTriggersOnTheFirstQualifiedAiOfTheOrderedCandidates()
        {
            var first = NewEligibleVehicle("FirstCapable", RageDisposition.ConfrontationCapable);
            var second = NewEligibleVehicle("SecondCapable", RageDisposition.ConfrontationCapable);

            var target = RageRoadEventFlowController.ResolveFirstTriggerCandidate(RageRoadEventState.Idle, new[] { first, second });

            Assert.That(target, Is.EqualTo(first),
                "Premier-arrive-gagne : la premiere IA de l'ordre NetworkObjectId devient l'unique cible.");
        }

        [Test]
        public void OnlyTheAiThatCrossedTheConditionBecomesTheEventTarget()
        {
            var calm = NewEligibleVehicle("Calm", RageDisposition.Calm);
            var ram = NewEligibleVehicle("Ram", RageDisposition.Ram);
            var capable = NewEligibleVehicle("Capable", RageDisposition.ConfrontationCapable);

            var target = RageRoadEventFlowController.ResolveFirstTriggerCandidate(RageRoadEventState.Idle, new[] { calm, ram, capable });

            Assert.That(target, Is.EqualTo(capable));
        }

        [Test]
        public void NoCandidateAboveTheConditionLeavesTheEventIdle()
        {
            var calm = NewEligibleVehicle("Calm", RageDisposition.Calm);
            var ram = NewEligibleVehicle("Ram", RageDisposition.Ram);

            Assert.That(RageRoadEventFlowController.ResolveFirstTriggerCandidate(RageRoadEventState.Idle, new[] { calm, ram }), Is.Null,
                "Aucune IA qualifiee : aucun evenement, et aucun message d'erreur a produire.");
            Assert.That(RageRoadEventFlowController.ResolveFirstTriggerCandidate(RageRoadEventState.Idle, new NetworkedAIVehicleState[0]), Is.Null);
            Assert.That(RageRoadEventFlowController.ResolveFirstTriggerCandidate(RageRoadEventState.Idle, null), Is.Null);
        }

        [Test]
        public void AiWithoutRageSourceNeverTriggersTheEvent()
        {
            var withoutRage = NewAiVehicleWithoutRageSource("WithoutRage");

            Assert.That(RageRoadEventFlowController.ResolveFirstTriggerCandidate(RageRoadEventState.Idle, new[] { withoutRage }), Is.Null,
                "L'eligibilite exige a la fois NetworkedAIVehicleState et une source rage/peur : sans les deux, rien ne se declenche.");
        }

        // ---------------------------------------------------------------- Anti-doublon

        [Test]
        public void AnActiveEventRefusesEveryFurtherTriggerRequest()
        {
            var capable = NewEligibleVehicle("Capable", RageDisposition.ConfrontationCapable);

            foreach (var active in new[] { RageRoadEventState.Triggered, RageRoadEventState.Confrontation })
            {
                Assert.That(RageRoadEventFlowController.ResolveFirstTriggerCandidate(active, new[] { capable }), Is.Null, active.ToString());
                Assert.That(RageRoadEventLifecycle.ResolveFirstTriggerIndex(active, new[] { RageDisposition.ConfrontationCapable }), Is.EqualTo(-1), active.ToString());
            }
        }

        [Test]
        public void TheSameAiStayingAboveTheConditionNeverCreatesASecondEvent()
        {
            var capable = NewEligibleVehicle("StaysCapable", RageDisposition.ConfrontationCapable);

            // L'anti-doublon vient de l'etat publie, pas d'un drapeau "deja declenche" par vehicule :
            // le meme candidat, reevalue, ne peut pas re-declencher tant que l'etat n'est pas revenu a Idle.
            Assert.That(RageRoadEventFlowController.ResolveFirstTriggerCandidate(RageRoadEventState.Triggered, new[] { capable }), Is.Null);

            var vehicleStateSource = File.ReadAllText(AiVehicleStateSourcePath);
            Assert.That(vehicleStateSource, Does.Not.Contain("RageRoad"),
                "Aucune seconde source de verite par vehicule : NetworkedRunState reste l'unique etat de l'evenement.");
        }

        [Test]
        public void StatesBeyondTriggeredCannotBeReTriggeredEither()
        {
            foreach (var state in new[] { RageRoadEventState.Resolved, RageRoadEventState.RewardGranted })
            {
                Assert.That(RageRoadEventLifecycle.ResolveFirstTriggerIndex(state, new[] { RageDisposition.ConfrontationCapable }), Is.EqualTo(-1),
                    state + " : rien n'est accepte tant que l'etat n'est pas Idle.");
            }
        }

        // ---------------------------------------------------------------- Table AD-22

        [Test]
        public void OnlySingleStepForwardTransitionsAreAccepted()
        {
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.Idle, RageRoadEventState.Triggered), Is.True);
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.Triggered, RageRoadEventState.Confrontation), Is.True);
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.Confrontation, RageRoadEventState.Resolved), Is.True);
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.Resolved, RageRoadEventState.RewardGranted), Is.True);
        }

        [Test]
        public void SkippedBackwardSelfAndTerminalTransitionsAreRefused()
        {
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.Idle, RageRoadEventState.Confrontation), Is.False, "Saut refuse.");
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.Triggered, RageRoadEventState.Resolved), Is.False, "Saut refuse.");
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.Triggered, RageRoadEventState.Idle), Is.False, "Retour arriere refuse.");
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.Idle, RageRoadEventState.Idle), Is.False, "Etat inchange refuse.");
            Assert.That(RageRoadEventLifecycle.CanAdvance(RageRoadEventState.RewardGranted, RageRoadEventState.RewardGranted), Is.False, "Etat terminal.");
        }

        [Test]
        public void OnlyTriggeredAndConfrontationCountAsAnActiveEvent()
        {
            Assert.That(RageRoadEventLifecycle.IsEventActive(RageRoadEventState.Triggered), Is.True);
            Assert.That(RageRoadEventLifecycle.IsEventActive(RageRoadEventState.Confrontation), Is.True);

            foreach (var inactive in new[] { RageRoadEventState.Idle, RageRoadEventState.Resolved, RageRoadEventState.RewardGranted })
            {
                Assert.That(RageRoadEventLifecycle.IsEventActive(inactive), Is.False, inactive.ToString());
            }
        }

        // ---------------------------------------------------------------- Etat partage et autorite

        [Test]
        public void RunStateOwnsTheEventStateAndItsTargetAsServerWriteNetworkVariables()
        {
            var state = NewRunState();

            Assert.That(state.RageRoadEvent.Value, Is.EqualTo(RageRoadEventState.Idle), "Defaut : aucun evenement.");
            Assert.That(state.RageRoadEvent.WritePerm, Is.EqualTo(NetworkVariableWritePermission.Server));
            Assert.That(state.RageRoadEvent.ReadPerm, Is.EqualTo(NetworkVariableReadPermission.Everyone));
            Assert.That(state.RageRoadEventTarget.WritePerm, Is.EqualTo(NetworkVariableWritePermission.Server),
                "Aucun client ne doit pouvoir changer l'etat de l'evenement.");
            Assert.That(state.RageRoadEventTarget.ReadPerm, Is.EqualTo(NetworkVariableReadPermission.Everyone),
                "Les clients, y compris les rejoignants tardifs, lisent l'etat et la cible courants.");
            Assert.That(state.RageRoadEventTarget.Value.TryGet(out _), Is.False, "Aucune cible avant declenchement.");
        }

        [Test]
        public void ClientsNeverWriteTheEventStateAndOnlyReadItFromTheRunState()
        {
            var source = File.ReadAllText(FlowControllerSourcePath);

            Assert.That(CountOccurrences(source, "RageRoadEvent.Value ="), Is.EqualTo(1),
                "Une seule ecriture d'etat : le declenchement.");
            Assert.That(CountOccurrences(source, "RageRoadEventTarget.Value ="), Is.EqualTo(1),
                "Une seule ecriture de cible : aucun retargeting.");
            Assert.That(source, Does.Contain("RageRoadEvent.Value = RageRoadEventState.Triggered"), "Epic 5 ne cable que Idle -> Triggered.");
            Assert.That(source, Does.Contain("manager.IsServer"), "Ecriture reservee au host, meme garde que le reste d'App/Run.");
            Assert.That(source, Does.Contain("FindAnyObjectByType<NetworkedRunState>()"),
                "Meme patron que RunFlowController : l'etat de run est resolu par recherche de scene.");
            Assert.That(source, Does.Not.Contain("Rpc"), "Aucune RPC : tout circule par les NetworkVariables server-write.");

            foreach (var laterState in new[] { RageRoadEventState.Confrontation, RageRoadEventState.Resolved, RageRoadEventState.RewardGranted })
            {
                Assert.That(source, Does.Not.Contain("= RageRoadEventState." + laterState), "Epic 5 ne cable que Idle -> Triggered : " + laterState);
            }

            var runStateSource = File.ReadAllText(RunStateSourcePath);
            Assert.That(runStateSource, Does.Contain("NetworkVariable<RageRoadEventState> RageRoadEvent"),
                "L'etat de l'evenement vit sur NetworkedRunState (AD-17), pas dans un champ local.");
            Assert.That(runStateSource, Does.Contain("NetworkObjectReference> RageRoadEventTarget"),
                "AD-20 : la cible est une NetworkObjectReference, jamais un id authored ni un index de voie.");
        }

        // ---------------------------------------------------------------- Cible perdue

        [Test]
        public void LostTargetIsReportedWithoutResetOrRetargeting()
        {
            Assert.That(RageRoadEventFlowController.IsEventTargetLost(RageRoadEventState.Triggered, false), Is.True,
                "Evenement actif dont la reference ne se resout plus : la cible est signalee perdue.");
            Assert.That(RageRoadEventFlowController.IsEventTargetLost(RageRoadEventState.Triggered, true), Is.False);
            Assert.That(RageRoadEventFlowController.IsEventTargetLost(RageRoadEventState.Idle, false), Is.False,
                "Sans evenement, il n'y a pas de cible perdue a signaler.");
            Assert.That(RageRoadEventFlowController.IsEventTargetLost(RageRoadEventState.Resolved, false), Is.False,
                "Hors etat actif, la presentation ne signale rien.");

            var source = File.ReadAllText(FlowControllerSourcePath);
            Assert.That(source, Does.Not.Contain("RageRoadEvent.Value = RageRoadEventState.Idle"),
                "Aucun reset automatique : l'etat publie par l'hote reste Triggered.");
            Assert.That(source, Does.Not.Contain("RageRoadEventTarget.Value = new NetworkObjectReference((NetworkObject)null)"),
                "Aucun effacement de cible : jamais de retargeting silencieux quand la cible disparait.");
        }

        // ---------------------------------------------------------------- Retour textuel

        [Test]
        public void HudRendersTheEventStateAndTheTargetNameInText()
        {
            var hud = NewObject("Story56Hud").AddComponent<RunCheckpointHudScreen>();

            hud.ShowRageRoadEventStatus(RageRoadEventState.Triggered, "Greybox_AIVehicle", false);

            var label = GetPrivateField<TMP_Text>(hud, "rageRoadEventLabel");
            Assert.That(label, Is.Not.Null, "Le libelle est cree a l'execution : aucune edition de scene requise.");
            Assert.That(label.text, Does.Contain("Rage Road"));
            Assert.That(label.text, Does.Contain("Triggered"), "L'etat est affiche en texte.");
            Assert.That(label.text, Does.Contain("Greybox_AIVehicle"), "Le nom de la cible est affiche en texte.");
        }

        [Test]
        public void HudSignalsALostTargetForAnActiveEvent()
        {
            var hud = NewObject("Story56LostTargetHud").AddComponent<RunCheckpointHudScreen>();

            hud.ShowRageRoadEventStatus(RageRoadEventState.Triggered, null, true);

            var label = GetPrivateField<TMP_Text>(hud, "rageRoadEventLabel");
            Assert.That(label.text, Does.Contain(RunCheckpointHudScreen.RageRoadEventTargetLostState),
                "La cible perdue est dite en toutes lettres, jamais par une couleur.");
            Assert.That(label.text, Does.Contain("Triggered"), "L'etat n'est ni reinitialise ni remplace par le signalement.");
        }

        [Test]
        public void HudPlaceholderClearsTheEventLine()
        {
            var hud = NewObject("Story56PlaceholderHud").AddComponent<RunCheckpointHudScreen>();

            hud.ShowRageRoadEventStatus(RageRoadEventState.Triggered, "Greybox_AIVehicle", false);
            hud.ShowAwaitingProfile();

            var label = GetPrivateField<TMP_Text>(hud, "rageRoadEventLabel");
            Assert.That(label.text, Is.EqualTo(RunCheckpointHudScreen.NoRageRoadEventState),
                "Un HUD remis a zero ne doit pas laisser un evenement fantome a l'ecran.");
        }

        [Test]
        public void AiVehicleLabelPrefixesTheEventTargetInTextOnly()
        {
            var source = CodeOnly(File.ReadAllText(DebugViewSourcePath));

            Assert.That(source, Does.Contain("SetRageRoadEventTarget"), "Le marquage vient d'un setter local, jamais d'une NetworkVariable.");
            Assert.That(source, Does.Contain("[RAGE ROAD]"), "La cible de l'evenement reste identifiable hors du cone de visee.");
            Assert.That(source, Does.Not.Contain(".color"), "Never : le retour joueur n'est jamais la couleur seule.");
            Assert.That(source, Does.Not.Contain(".Value ="), "Une vue n'ecrit jamais dans une NetworkVariable.");
            Assert.That(source, Does.Not.Contain("NetworkVariable"), "Le marqueur est purement local : rien a repliquer.");
        }

        // ---------------------------------------------------------------- Cablage de scene

        [Test]
        public void MvpRunCarriesTheEventControllerOnTheRunRoot()
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MvpRunScenePath);
            var wasOpen = alreadyOpen.IsValid();
            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);

            try
            {
                var root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == "RunRoot");
                Assert.That(root, Is.Not.Null, "RunRoot attendu dans MVP_Run");

                var controllers = root.GetComponents<RageRoadEventFlowController>();
                Assert.That(controllers.Length, Is.EqualTo(1),
                    "Un proprietaire unique et inspectable de l'evenement, sur la racine du run.");
                Assert.That(root.GetComponent<RunFlowController>(), Is.Not.Null,
                    "RunFlowController reste le hub du run sur cette meme racine.");
                Assert.That(root.GetComponentInChildren<RunCheckpointHudScreen>(true), Is.Not.Null,
                    "Le HUD de checkpoint est un enfant de la racine du run : le libelle de l'evenement vit la.");
            }
            finally
            {
                if (!wasOpen)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        // ---------------------------------------------------------------- Doubles sans Netcode

        private NetworkedRunState NewRunState()
        {
            var root = NewObject("Story56RunState");
            root.AddComponent<NetworkObject>();
            return root.AddComponent<NetworkedRunState>();
        }

        /// <summary>Candidat eligible (NetworkedAIVehicleState + NetworkedRageState), jamais spawne.</summary>
        private NetworkedAIVehicleState NewEligibleVehicle(string name, RageDisposition disposition)
        {
            var root = NewObject(name);
            root.AddComponent<NetworkObject>();
            var state = root.AddComponent<NetworkedAIVehicleState>();
            var rage = root.AddComponent<NetworkedRageState>();
            rage.Disposition.Value = disposition;
            return state;
        }

        /// <summary>NetworkedAIVehicleState sans source rage/peur : structurellement inelligible (AD-20).</summary>
        private NetworkedAIVehicleState NewAiVehicleWithoutRageSource(string name)
        {
            var root = NewObject(name);
            root.AddComponent<NetworkObject>();
            return root.AddComponent<NetworkedAIVehicleState>();
        }

        private GameObject NewObject(string name)
        {
            var value = new GameObject(name);
            spawned.Add(value);
            return value;
        }

        /// <summary>
        /// Source privee de ses commentaires de ligne et de documentation. Les gardes de ce fichier
        /// portent sur du code, pas sur de la prose : un commentaire qui NOMME la regle ("n'ecrit jamais
        /// dans une NetworkVariable") ne doit pas faire echouer la garde qui verifie cette meme regle.
        /// </summary>
        private static string CodeOnly(string source)
        {
            return string.Join("\n", source
                .Split('\n')
                .Where(line => !line.TrimStart().StartsWith("//")));
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var index = text.IndexOf(value, System.StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = text.IndexOf(value, index + value.Length, System.StringComparison.Ordinal);
            }

            return count;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            return (T)field.GetValue(target);
        }
    }
}
