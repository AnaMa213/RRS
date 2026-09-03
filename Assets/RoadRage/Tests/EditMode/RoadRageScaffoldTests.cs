using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.Features.Boss;
using RoadRage.Features.Economy;
using RoadRage.Features.Players;
using RoadRage.Features.Rage;
using RoadRage.Features.Run;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    public sealed class RoadRageScaffoldTests
    {
        private static readonly string[] AllScenePaths =
        {
            "Assets/RoadRage/App/Scenes/Bootstrap.unity",
            "Assets/RoadRage/App/Scenes/MainMenuLobby.unity",
            "Assets/RoadRage/App/Scenes/MVP_Run.unity",
            "Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity",
            "Assets/RoadRage/App/Scenes/Dev_OnFootSandbox.unity",
            "Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity",
            "Assets/RoadRage/App/Scenes/Dev_LobbySmokeTest.unity"
        };

        private static readonly string[] PrimaryBuildScenePaths =
        {
            "Assets/RoadRage/App/Scenes/Bootstrap.unity",
            "Assets/RoadRage/App/Scenes/MainMenuLobby.unity",
            "Assets/RoadRage/App/Scenes/MVP_Run.unity"
        };

        private static readonly string[] RequiredFolders =
        {
            "Assets/RoadRage/App/Bootstrap",
            "Assets/RoadRage/App/Scenes",
            "Assets/RoadRage/App/Services",
            "Assets/RoadRage/Shared/Domain",
            "Assets/RoadRage/Shared/Definitions",
            "Assets/RoadRage/Shared/Networking",
            "Assets/RoadRage/Shared/Presentation",
            "Assets/RoadRage/Shared/Utilities",
            "Assets/RoadRage/Features/Lobby",
            "Assets/RoadRage/Features/Run",
            "Assets/RoadRage/Features/Players",
            "Assets/RoadRage/Features/Vehicles",
            "Assets/RoadRage/Features/Rage",
            "Assets/RoadRage/Features/PassengerActions",
            "Assets/RoadRage/Features/OnFoot",
            "Assets/RoadRage/Features/SandboxStops",
            "Assets/RoadRage/Features/Economy",
            "Assets/RoadRage/Features/Boss",
            "Assets/RoadRage/Features/UI",
            "Assets/RoadRage/ArtSource/Blender",
            "Assets/RoadRage/ArtSource/GeneratedReferences",
            "Assets/RoadRage/ArtExports",
            "Assets/RoadRage/Materials",
            "Assets/RoadRage/Prefabs",
            "Assets/RoadRage/ScriptableObjects",
            "Assets/RoadRage/Tests"
        };

        private static readonly string[] FeatureNames =
        {
            "Lobby",
            "Run",
            "Players",
            "Vehicles",
            "Rage",
            "PassengerActions",
            "OnFoot",
            "SandboxStops",
            "Economy",
            "Boss",
            "UI"
        };

        private static readonly Type[] RuntimeStateTypes =
        {
            typeof(NetworkedRunState),
            typeof(NetworkedPlayerState),
            typeof(NetworkedAIVehicleState),
            typeof(NetworkedRageState),
            typeof(NetworkedCrewEconomyState),
            typeof(NetworkedBossState)
        };

        [Test]
        public void SceneSeedsExistAndPrimaryBuildScenesAreOrdered()
        {
            foreach (var scenePath in AllScenePaths)
            {
                Assert.That(File.Exists(scenePath), Is.True, scenePath);
            }

            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            CollectionAssert.AreEqual(PrimaryBuildScenePaths, enabledScenes);
            Assert.That(enabledScenes, Does.Not.Contain("Assets/Scenes/SampleScene.unity"));
        }

        [Test]
        public void RoadRageFolderScaffoldExists()
        {
            Assert.That(Directory.Exists("Assets/RoadRage"), Is.True);

            foreach (var folder in RequiredFolders)
            {
                Assert.That(Directory.Exists(folder), Is.True, folder);
            }
        }

        [Test]
        public void AsmdefsAndNamespacesStayInsideApprovedBoundaries()
        {
            AssertAsmdef("Assets/RoadRage/App/RoadRage.App.asmdef", "RoadRage.App");
            AssertAsmdef("Assets/RoadRage/Shared/RoadRage.Shared.asmdef", "RoadRage.Shared");

            foreach (var featureName in FeatureNames)
            {
                var assemblyName = "RoadRage.Features." + featureName;
                var path = "Assets/RoadRage/Features/" + featureName + "/" + assemblyName + ".asmdef";
                var definition = AssertAsmdef(path, assemblyName);

                foreach (var reference in definition.references ?? Array.Empty<string>())
                {
                    Assert.That(reference.StartsWith("RoadRage.Features.", StringComparison.Ordinal), Is.False, assemblyName);
                }
            }

            Assert.That(File.ReadAllText("Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs"), Does.Contain("namespace RoadRage.App"));
            Assert.That(File.ReadAllText("Assets/RoadRage/Shared/Networking/IHostOwnedRuntimeState.cs"), Does.Contain("namespace RoadRage.Shared.Networking"));
            Assert.That(File.ReadAllText("Assets/RoadRage/Features/Run/NetworkedRunState.cs"), Does.Contain("namespace RoadRage.Features.Run"));
        }

        [Test]
        public void RuntimeStateShellsAreNetworkBehavioursAndServerWrite()
        {
            foreach (var runtimeStateType in RuntimeStateTypes)
            {
                Assert.That(typeof(NetworkBehaviour).IsAssignableFrom(runtimeStateType), Is.True, runtimeStateType.Name);
                Assert.That(typeof(IHostOwnedRuntimeState).IsAssignableFrom(runtimeStateType), Is.True, runtimeStateType.Name);

                var networkVariableFields = runtimeStateType
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Where(field => typeof(NetworkVariableBase).IsAssignableFrom(field.FieldType))
                    .ToArray();

                Assert.That(networkVariableFields.Length, Is.GreaterThan(0), runtimeStateType.Name);

                var gameObject = new GameObject(runtimeStateType.Name);
                try
                {
                    gameObject.AddComponent<NetworkObject>();
                    var component = gameObject.AddComponent(runtimeStateType);

                    foreach (var field in networkVariableFields)
                    {
                        var variable = (NetworkVariableBase)field.GetValue(component);
                        Assert.That(variable.WritePerm, Is.EqualTo(NetworkVariableWritePermission.Server), runtimeStateType.Name + "." + field.Name);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(gameObject);
                }
            }
        }

        [Test]
        public void RunCompositionRootDoesNotStartGameplay()
        {
            Assert.That(typeof(MonoBehaviour).IsAssignableFrom(typeof(RunCompositionRoot)), Is.True);

            var source = File.ReadAllText("Assets/RoadRage/Features/Run/RunCompositionRoot.cs");
            Assert.That(source, Does.Not.Contain("Instantiate("));
            Assert.That(source, Does.Not.Contain(".Spawn("));
            Assert.That(source, Does.Not.Contain("StartHost("));
            Assert.That(source, Does.Not.Contain("StartClient("));
        }

        [Test]
        public void ExistingSteamworksSmokeTestHelperIsPreserved()
        {
            var path = "Assets/Editor/RoadRageSteamworksSmokeTest.cs";
            Assert.That(File.Exists(path), Is.True);

            var source = File.ReadAllText(path);
            Assert.That(source, Does.Contain("namespace RoadRage.Editor"));
            Assert.That(source, Does.Contain("SteamClient.Init(TestSteamAppId, false)"));
        }

        [Test]
        public void Story04ValidationRowsRecordOnlyScaffoldEvidence()
        {
            var logLines = File.ReadAllLines("docs/setup/tooling-validation-log.md");
            var story04Lines = logLines
                .Where(line => line.Contains("VAL-016", StringComparison.Ordinal)
                    || line.Contains("VAL-017", StringComparison.Ordinal)
                    || line.Contains("VAL-018", StringComparison.Ordinal)
                    || line.Contains("VAL-019", StringComparison.Ordinal))
                .ToArray();

            foreach (var validationId in new[] { "VAL-016", "VAL-017", "VAL-018", "VAL-019" })
            {
                Assert.That(story04Lines.Any(line => line.Contains("| " + validationId + " | `Pass` |", StringComparison.Ordinal)), Is.True, validationId);
            }

            foreach (var line in story04Lines)
            {
                Assert.That(line, Does.Not.Contain("Unity Cloud Services"));
                Assert.That(line, Does.Not.Contain("Relay"));
                Assert.That(line, Does.Not.Contain("Photon"));
                Assert.That(line, Does.Not.Contain("Mirror"));
                Assert.That(line, Does.Not.Contain("dedicated server"));
            }
        }

        private static AssemblyDefinition AssertAsmdef(string path, string expectedName)
        {
            Assert.That(File.Exists(path), Is.True, path);

            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(path));
            Assert.That(definition.name, Is.EqualTo(expectedName));
            Assert.That(definition.rootNamespace, Is.EqualTo(expectedName));
            return definition;
        }

        [Serializable]
        private sealed class AssemblyDefinition
        {
            public string name;
            public string rootNamespace;
            public string[] references;
        }
    }
}
