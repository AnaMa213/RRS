using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Lobby;
using RoadRage.Features.Lobby;
using RoadRage.Features.UI;
using RoadRage.Shared.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    public sealed class Story12LobbyShellTests
    {
        private const string MainMenuLobbyScenePath = "Assets/RoadRage/App/Scenes/MainMenuLobby.unity";

        [Test]
        public void MatchSettingsDefaultsToNormalDifficulty()
        {
            var settings = new MatchSettings();

            Assert.That(settings.Difficulty, Is.EqualTo(Difficulty.Normal));
        }

        [Test]
        public void MatchSettingsDifficultyIsMutable()
        {
            var settings = new MatchSettings();

            settings.Difficulty = Difficulty.Hard;

            Assert.That(settings.Difficulty, Is.EqualTo(Difficulty.Hard));
        }

        [Test]
        public void MatchSettingsIsPlainClassWithoutUnityBaseTypes()
        {
            var type = typeof(MatchSettings);

            Assert.That(typeof(MonoBehaviour).IsAssignableFrom(type), Is.False, "MatchSettings must not be a MonoBehaviour");
            Assert.That(typeof(ScriptableObject).IsAssignableFrom(type), Is.False, "MatchSettings must not be a ScriptableObject");
        }

        [Test]
        public void MainMenuLobbySceneContainsWiredLobbyComponents()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuLobbyScenePath, OpenSceneMode.Additive);
            try
            {
                var screen = FindComponentInScene<LobbyShellScreen>(scene);
                Assert.That(screen, Is.Not.Null, "LobbyShellScreen expected in MainMenuLobby scene");
                AssertSerializedObjectFieldsNonNull(screen);

                var flowController = FindComponentInScene<LobbyFlowController>(scene);
                Assert.That(flowController, Is.Not.Null, "LobbyFlowController expected in MainMenuLobby scene");
                AssertSerializedObjectFieldsNonNull(flowController);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void UiFeatureSourceNeverCallsSceneManagementOrMutatesMatchSettingsDirectly()
        {
            var uiSourceFiles = Directory.GetFiles(AssetsPath("RoadRage", "Features", "UI"), "*.cs", SearchOption.AllDirectories);
            Assert.That(uiSourceFiles.Length, Is.GreaterThan(0));

            foreach (var file in uiSourceFiles)
            {
                var source = File.ReadAllText(file);
                Assert.That(source, Does.Not.Contain("SceneManager.LoadScene"), file);
                Assert.That(source, Does.Not.Contain("Application.Quit"), file);
                Assert.That(source, Does.Not.Contain("MatchSettings"), file + " must not reference the Lobby feature's data object directly");
            }
        }

        [Test]
        public void UiAssemblyDoesNotReferenceLobbyAssembly()
        {
            var uiAsmdefPath = Path.Combine(AssetsPath("RoadRage", "Features", "UI"), "RoadRage.Features.UI.asmdef");
            Assert.That(File.Exists(uiAsmdefPath), Is.True, uiAsmdefPath);

            var source = File.ReadAllText(uiAsmdefPath);

            Assert.That(source, Does.Not.Contain("RoadRage.Features.Lobby"));
        }

        /// <summary>
        /// Construit un chemin absolu sous Assets/. Les chemins relatifs supposeraient que le repertoire
        /// courant est la racine du projet, ce qui n'est pas garanti (notamment sous la commande de repli
        /// -batchmode -runTests lancee depuis un autre repertoire, ou l'on obtiendrait une
        /// DirectoryNotFoundException au lieu d'un echec de test lisible).
        /// </summary>
        private static string AssetsPath(params string[] segments)
        {
            var path = Application.dataPath;

            foreach (var segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return path;
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                var component = rootObject.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static void AssertSerializedObjectFieldsNonNull(Component component)
        {
            var fields = component.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                .Where(field => typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType));

            foreach (var field in fields)
            {
                var value = field.GetValue(component) as UnityEngine.Object;

                // Comparaison via l'operateur == surcharge d'UnityEngine.Object (et non NUnit Is.Not.Null,
                // qui compare des references et laisserait passer un faux-null Unity : objet detruit ou
                // reference manquante, qui se comporte pourtant comme null a l'execution).
                Assert.That(value != null, Is.True, component.GetType().Name + "." + field.Name);
            }
        }
    }
}
