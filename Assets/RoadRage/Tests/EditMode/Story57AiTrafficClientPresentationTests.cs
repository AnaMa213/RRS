using System;
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
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.7 : contrat de presentation client du trafic IA. Les Stories 5.2 a 5.6 ont pose chaque
    /// morceau de l'etat replique (position par NetworkTransform, WaypointIndex, Behavior, rage,
    /// Rage Road) ; cette fixture n'en reconstruit rien et verrouille seulement ce que l'AC « les
    /// clients voient » exige et que rien ne gardait : la permission de LECTURE des NetworkVariables
    /// (RoadRageScaffoldTests ne teste que l'ecriture), l'absence de toute ecriture ou RPC dans les
    /// vues, le rafraichissement de presentation hors garde d'autorite, la composition des trois
    /// vehicules de MVP_Run, et l'absence de NetworkVariable IA sans consommateur.
    ///
    /// Deterministe et sans Netcode, comme Story54/55/56 : les doubles sont des `new GameObject` +
    /// `AddComponent`, jamais `.Spawn()`, donc `IsServer`/`IsSpawned` ne sont pas exercables ici. Le
    /// cablage host-only est donc verifie par assertions sur le source, idiome deja accepte dans ce
    /// depot (voir `CodeOnly` : les gardes portent sur du code, jamais sur de la prose).
    /// </summary>
    public sealed class Story57AiTrafficClientPresentationTests
    {
        private const string AiVehicleStateSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs";
        private const string AiDriverSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";
        private const string AiDebugViewSourcePath = "Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs";
        private const string RageDebugViewSourcePath = "Assets/RoadRage/Features/Rage/RageStateDebugView.cs";
        private const string HudSourcePath = "Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs";
        private const string FlowControllerSourcePath = "Assets/RoadRage/App/Run/RageRoadEventFlowController.cs";
        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

        /// <summary>Les deux seules NetworkVariables de l'etat IA, chacune avec son consommateur reel.</summary>
        private static readonly string[] ReplicatedAiStateFields = { "WaypointIndex", "Behavior" };

        /// <summary>Sources autorisees a lire l'etat IA replique (et donc a le rendre).</summary>
        private static readonly string[] AiStateConsumerSourcePaths = { AiDriverSourcePath, AiDebugViewSourcePath };

        /// <summary>Vues qui n'ont aucune raison d'ecrire quoi que ce soit sur le reseau.</summary>
        private static readonly string[] ClientViewSourcePaths = { AiDebugViewSourcePath, RageDebugViewSourcePath, HudSourcePath };

        /// <summary>Tout le chemin de presentation IA : polling par frame, jamais d'abonnement de rattrapage.</summary>
        private static readonly string[] PresentationSourcePaths =
        {
            AiDriverSourcePath,
            AiDebugViewSourcePath,
            RageDebugViewSourcePath,
            HudSourcePath,
            FlowControllerSourcePath
        };

        private static readonly Type[] ReplicatedStateTypes =
        {
            typeof(NetworkedAIVehicleState),
            typeof(NetworkedRageState),
            typeof(NetworkedRunState)
        };

        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in spawned)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            spawned.Clear();
        }

        // ---------------------------------------------------------------- Permission de lecture

        [Test]
        public void EveryReplicatedStateNetworkVariableIsEveryoneReadAndServerWrite()
        {
            foreach (var stateType in ReplicatedStateTypes)
            {
                var root = NewObject("Story57" + stateType.Name);
                root.AddComponent<NetworkObject>();
                var component = root.AddComponent(stateType);

                var networkVariableFields = stateType
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Where(field => typeof(NetworkVariableBase).IsAssignableFrom(field.FieldType))
                    .ToArray();

                Assert.That(networkVariableFields.Length, Is.GreaterThan(0), stateType.Name);

                foreach (var field in networkVariableFields)
                {
                    var variable = (NetworkVariableBase)field.GetValue(component);
                    var label = stateType.Name + "." + field.Name;

                    Assert.That(variable.ReadPerm, Is.EqualTo(NetworkVariableReadPermission.Everyone),
                        label + " doit rester lisible par tous : une lecture privee rendrait le client aveugle sans faire echouer RoadRageScaffoldTests.");
                    Assert.That(variable.WritePerm, Is.EqualTo(NetworkVariableWritePermission.Server),
                        label + " reste server-write : un client ne force jamais le comportement d'une IA.");
                }
            }
        }

        // ---------------------------------------------------------------- Les vues ne font que lire

        [Test]
        public void ClientViewsOnlyReadReplicatedState()
        {
            foreach (var path in ClientViewSourcePaths)
            {
                var source = CodeOnly(File.ReadAllText(path));

                Assert.That(source, Does.Not.Contain(".Value ="), path + " : une vue n'ecrit jamais dans une NetworkVariable.");
                Assert.That(source, Does.Not.Contain(".Value="), path + " : variante sans espace de la meme ecriture.");
                Assert.That(source, Does.Not.Contain("Rpc"), path + " : aucune vue n'envoie de RPC.");
            }

            Assert.That(CodeOnly(File.ReadAllText(AiDebugViewSourcePath)), Does.Contain("Behavior.Value"),
                "Le libelle IA rend l'etat replique, pas une copie locale.");
            Assert.That(CodeOnly(File.ReadAllText(RageDebugViewSourcePath)), Does.Contain("RageValue.Value"),
                "Le libelle de rage rend l'etat replique.");
            Assert.That(CodeOnly(File.ReadAllText(HudSourcePath)), Does.Not.Contain("NetworkVariable"),
                "Le HUD ne connait aucune NetworkVariable : il recoit des valeurs deja lues par l'orchestration (Features/UI ne reference pas Netcode).");
        }

        [Test]
        public void AiBehaviorViewRendersAnExplicitStateWhenNoAiIsPresent()
        {
            var view = NewObject("Story57NoAiView").AddComponent<AIVehicleBehaviorDebugView>();

            Assert.That(view.GetComponent<NetworkedAIVehicleState>(), Is.Null,
                "Scene sans trafic (Dev_RageSandbox) : la vue ne trouve aucune cible.");

            var text = InvokePrivate<string>(view, "ComposeText");

            Assert.That(text, Is.EqualTo("IA : etat absent"),
                "Aucune cible : la vue dit l'absence en texte, sans NullReferenceException et sans Calm silencieux qui masquerait un vehicule mal compose.");
        }

        // ---------------------------------------------------------------- Arrivant tardif

        [Test]
        public void ClientPresentationPollsWithoutASecondSyncPath()
        {
            foreach (var path in PresentationSourcePaths)
            {
                Assert.That(CodeOnly(File.ReadAllText(path)), Does.Not.Contain("OnValueChanged"),
                    path + " : le rafraichissement reste du polling par frame (un arrivant tardif se remplit au premier tick) ; un abonnement dedie serait une seconde voie a maintenir.");
            }

            Assert.That(CodeOnly(File.ReadAllText(AiDebugViewSourcePath)), Does.Contain("private void LateUpdate()"),
                "Le libelle IA relit l'etat a chaque frame.");
            Assert.That(CodeOnly(File.ReadAllText(RageDebugViewSourcePath)), Does.Contain("private void Update()"),
                "Le libelle de rage relit l'etat a chaque frame.");
            Assert.That(CodeOnly(File.ReadAllText(FlowControllerSourcePath)), Does.Contain("private void Update()"),
                "L'orchestration Rage Road relit l'etat a chaque frame, sur tous les pairs.");
        }

        // ---------------------------------------------------------------- Presentation sur tous les pairs

        [Test]
        public void EventPresentationRunsOnEveryPeerOutsideTheAuthorityGuard()
        {
            var source = CodeOnly(File.ReadAllText(FlowControllerSourcePath));
            var update = ExtractMethod(source, "private void Update()");

            var authorityDepth = BraceDepthOf(update, "if (IsAuthoritative()");
            var decisionDepth = BraceDepthOf(update, "EvaluateHostTrigger(state);");
            var presentationDepth = BraceDepthOf(update, "RefreshEventPresentation(state);");

            Assert.That(authorityDepth, Is.GreaterThanOrEqualTo(0), "garde d'autorite introuvable dans Update()");
            Assert.That(decisionDepth, Is.EqualTo(authorityDepth + 1),
                "La decision (evaluation du declenchement) reste, elle, host-only.");
            Assert.That(presentationDepth, Is.EqualTo(authorityDepth),
                "RefreshEventPresentation est appelee au meme niveau que le bloc d'autorite, jamais dedans : la presentation tourne sur tous les pairs.");
        }

        // ---------------------------------------------------------------- Cout mort

        [Test]
        public void EveryAiStateNetworkVariableHasAConsumer()
        {
            Assert.That(CodeOnly(File.ReadAllText(AiVehicleStateSourcePath)), Does.Not.Contain("RouteIndex"),
                "RouteIndex est retiree : repliquee sans jamais etre ecrite ni lue, elle payait un cout de spawn pour une seconde source de verite de route.");

            var fields = typeof(NetworkedAIVehicleState)
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(field => typeof(NetworkVariableBase).IsAssignableFrom(field.FieldType))
                .Select(field => field.Name)
                .ToArray();

            CollectionAssert.AreEquivalent(ReplicatedAiStateFields, fields,
                "Aucune NetworkVariable IA sans consommateur : RouteIndex (repliquee sans jamais etre ecrite ni lue) est retiree en Story 5.7 et ne doit pas revenir.");

            var consumerSources = AiStateConsumerSourcePaths
                .Select(path => CodeOnly(File.ReadAllText(path)))
                .ToArray();

            foreach (var field in fields)
            {
                var token = "." + field + ".Value";
                Assert.That(consumerSources.Any(source => source.Contains(token)), Is.True,
                    token + " doit etre lu ou ecrit par le mouvement ou l'affichage : une NetworkVariable sans consommateur est du cout de replication mort.");
            }
        }

        // ---------------------------------------------------------------- Cablage de scene

        [Test]
        public void MvpRunAiTrafficIsSpawnedAtRuntimeByThePortalSpawnerFromAFullyComposedPrefab()
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MvpRunScenePath);
            var wasOpen = alreadyOpen.IsValid();
            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(MvpRunScenePath, OpenSceneMode.Additive);

            try
            {
                var root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == "RunRoot");
                Assert.That(root, Is.Not.Null, "RunRoot attendu dans MVP_Run");

                var traffic = root.transform.Find("AITraffic");
                Assert.That(traffic, Is.Not.Null, "AITraffic attendu sous RunRoot");

                // Story 5.10 : le trafic n'est plus pose en scene. Il entre et sort par les portails du
                // graphe de voies, insere a l'execution par l'hote. Ce que la Story 5.7 verrouille --
                // la composition d'un vehicule de trafic, et donc ce que le client voit -- se verifie
                // desormais sur le prefab que le spawner instancie, pas sur trois instances figees.
                var spawner = traffic.GetComponent<PortalTrafficSpawner>();
                Assert.That(spawner, Is.Not.Null, "PortalTrafficSpawner attendu sur AITraffic : c'est lui qui fait exister le trafic cote client.");

                var serialized = new SerializedObject(spawner);
                Assert.That(serialized.FindProperty("laneGraph").objectReferenceValue, Is.Not.Null,
                    "Le spawner doit connaitre le graphe : sans lui aucun portail, donc aucun vehicule chez personne.");

                var prefab = serialized.FindProperty("vehiclePrefab").objectReferenceValue as GameObject;
                Assert.That(prefab, Is.Not.Null, "Le spawner doit referencer le prefab reseau du vehicule de trafic.");

                Assert.That(prefab.GetComponent<NetworkObject>(), Is.Not.Null,
                    prefab.name + " doit porter un NetworkObject : sans lui l'hote ne peut pas le spawner et le client ne le voit jamais.");
                Assert.That(prefab.GetComponent<NetworkedAIVehicleState>(), Is.Not.Null,
                    prefab.name + " doit porter l'etat IA replique (WaypointIndex, Behavior).");
                Assert.That(prefab.GetComponent<NetworkedRageState>(), Is.Not.Null,
                    prefab.name + " doit porter l'etat de rage replique (source de Behavior et du Rage Road).");
                Assert.That(prefab.GetComponent<NetworkTransform>(), Is.Not.Null,
                    prefab.name + " doit repliquer sa position par le NetworkTransform existant -- pas par une RPC de mouvement.");
                Assert.That(prefab.GetComponent<AIVehicleBehaviorDebugView>(), Is.Not.Null,
                    prefab.name + " doit rendre son comportement en texte, jamais par une couleur seule.");

                Assert.That(root.GetComponentsInChildren<NetworkedAIVehicleState>(true).Length, Is.EqualTo(0),
                    "Plus aucun vehicule de trafic pose en scene : l'effectif est resolu a l'execution, il n'est plus une constante de scene.");

                var sceneText = File.ReadAllText(MvpRunScenePath);
                Assert.That(sceneText, Does.Not.Contain("value: AI_Vehicle_0"),
                    "Les trois instances in-scene de la Story 5.2 sont retirees.");

                var prefabGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
                Assert.That(File.ReadAllText("Assets/DefaultNetworkPrefabs.asset"), Does.Contain(prefabGuid),
                    "Le prefab de trafic doit rester enregistre dans DefaultNetworkPrefabs : c'est ce qui autorise son spawn runtime chez les clients.");
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

        private GameObject NewObject(string name)
        {
            var value = new GameObject(name);
            spawned.Add(value);
            return value;
        }

        /// <summary>
        /// Source privee de ses commentaires de ligne et de documentation. Les gardes de ce fichier
        /// portent sur du code, pas sur de la prose : un commentaire qui NOMME la regle ("une vue
        /// n'ecrit jamais dans une NetworkVariable") ne doit pas faire echouer la garde qui verifie
        /// cette meme regle (patron `CodeOnly` de Story56).
        /// </summary>
        private static string CodeOnly(string source)
        {
            return string.Join("\n", source
                .Split('\n')
                .Where(line => !line.TrimStart().StartsWith("//")));
        }

        private static string ExtractMethod(string source, string signature)
        {
            var start = source.IndexOf(signature, System.StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "signature introuvable : " + signature);

            var next = source.IndexOf("private void ", start + signature.Length, System.StringComparison.Ordinal);
            return next < 0 ? source.Substring(start) : source.Substring(start, next - start);
        }

        /// <summary>
        /// Profondeur d'accolades, relative au debut du fragment, a laquelle apparait une instruction.
        /// Sert a prouver qu'une instruction est au meme niveau qu'un `if` et non a l'interieur de son
        /// bloc -- sans dependre de l'indentation exacte du fichier.
        /// </summary>
        private static int BraceDepthOf(string fragment, string statement)
        {
            var depth = 0;
            foreach (var rawLine in fragment.Split('\n'))
            {
                var line = rawLine.Trim();

                if (line.StartsWith("}"))
                {
                    depth--;
                }

                if (line.Contains(statement))
                {
                    return depth;
                }

                if (line.StartsWith("{"))
                {
                    depth++;
                }
            }

            return -1;
        }

        private static T InvokePrivate<T>(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "method not found: " + methodName);
            return (T)method.Invoke(target, null);
        }
    }
}
