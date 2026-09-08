using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.App.Lobby;
using RoadRage.Features.Lobby;
using RoadRage.Features.UI;
using RoadRage.Shared.Domain;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    public sealed class Story12LobbyShellTests
    {
        private const string MainMenuLobbyScenePath = "Assets/RoadRage/App/Scenes/MainMenuLobby.unity";

        private const float MinimumReadableContrast = 4.5f;

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
        public void MainMenuLobbyStatusLabelsContrastWithPanelBackgrounds()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuLobbyScenePath, OpenSceneMode.Additive);
            try
            {
                var menuScreen = FindComponentInScene<MainMenuScreen>(scene);
                Assert.That(menuScreen, Is.Not.Null, "MainMenuScreen attendu dans MainMenuLobby");

                var lobbyScreen = FindComponentInScene<LobbyShellScreen>(scene);
                Assert.That(lobbyScreen, Is.Not.Null, "LobbyShellScreen attendu dans MainMenuLobby");

                var menuPanel = (GameObject)GetPrivateField(menuScreen, "menuPanel");
                var setupPanel = (GameObject)GetPrivateField(menuScreen, "setupPanel");
                var noticePanel = (GameObject)GetPrivateField(menuScreen, "noticePanel");

                AssertReadableContrast((TMP_Text)GetPrivateField(menuScreen, "titleLabel"), PanelImage(menuPanel), "titleLabel");
                AssertReadableContrast((TMP_Text)GetPrivateField(menuScreen, "noticeText"), PanelImage(noticePanel), "noticeText");
                AssertReadableContrast((TMP_Text)GetPrivateField(lobbyScreen, "settingsSummaryLabel"), PanelImage(setupPanel), "settingsSummaryLabel");
                AssertReadableContrast((TMP_Text)GetPrivateField(lobbyScreen, "roomCodeLabel"), PanelImage(setupPanel), "roomCodeLabel");
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

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "champ introuvable : " + fieldName);
            return field.GetValue(target);
        }

        private static UnityEngine.UI.Image PanelImage(GameObject panel)
        {
            Assert.That(panel != null, Is.True, "panel doit etre cable");

            var image = panel.GetComponent<UnityEngine.UI.Image>();
            Assert.That(image != null, Is.True, panel.name + " doit porter une Image de fond");
            return image;
        }

        private static void AssertReadableContrast(TMP_Text text, UnityEngine.UI.Image background, string labelName)
        {
            Assert.That(text != null, Is.True, labelName + " doit etre cable");
            Assert.That(text.transform.IsChildOf(background.transform), Is.True, labelName + " doit rester sous " + background.name);
            Assert.That(text.color.a, Is.GreaterThanOrEqualTo(0.95f), labelName + " doit etre opaque");
            Assert.That(background.color.a, Is.GreaterThanOrEqualTo(0.95f), background.name + " doit avoir un fond opaque");

            var contrast = ContrastRatio(text.color, background.color);
            Assert.That(contrast, Is.GreaterThanOrEqualTo(MinimumReadableContrast), labelName + " manque de contraste avec " + background.name);
        }

        private static float ContrastRatio(Color foreground, Color background)
        {
            var foregroundLuminance = RelativeLuminance(foreground);
            var backgroundLuminance = RelativeLuminance(background);
            var lighter = Mathf.Max(foregroundLuminance, backgroundLuminance);
            var darker = Mathf.Min(foregroundLuminance, backgroundLuminance);

            return (lighter + 0.05f) / (darker + 0.05f);
        }

        private static float RelativeLuminance(Color color)
        {
            return 0.2126f * LinearRgb(color.r)
                + 0.7152f * LinearRgb(color.g)
                + 0.0722f * LinearRgb(color.b);
        }

        private static float LinearRgb(float channel)
        {
            return channel <= 0.03928f
                ? channel / 12.92f
                : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);
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
