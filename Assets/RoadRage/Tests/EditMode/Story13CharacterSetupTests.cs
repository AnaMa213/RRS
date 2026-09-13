using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Players;
using RoadRage.Shared.Definitions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 1.3 : regles de validation du nom joueur, coherence du catalogue de personnages,
    /// purete du profil de session et frontieres d'assemblies.
    /// Le cablage de scene du flux manuel de creation de profil n'est plus verrouille ici depuis la
    /// Story 4.5 : ce flux a ete retire au profit du menu principal persistant.
    /// </summary>
    public sealed class Story13CharacterSetupTests
    {
        private const string CharacterCatalogAssetPath = "Assets/RoadRage/ScriptableObjects/Players/CharacterCatalog.asset";
        private const string UiAsmdefPath = "Assets/RoadRage/Features/UI/RoadRage.Features.UI.asmdef";
        private const string PlayersAsmdefPath = "Assets/RoadRage/Features/Players/RoadRage.Features.Players.asmdef";

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

        // ---------------------------------------------------------------- PlayerNameValidator

        [Test]
        public void PlayerNameValidatorRejectsEmptyName()
        {
            string normalized;
            string error;

            var accepted = PlayerNameValidator.TryNormalize(string.Empty, out normalized, out error);

            Assert.That(accepted, Is.False);
            Assert.That(error, Is.EqualTo(PlayerNameValidator.EmptyError));
            Assert.That(error, Is.Not.Empty, "un refus doit toujours nommer la regle violee");
        }

        [Test]
        public void PlayerNameValidatorRejectsWhitespaceOnlyName()
        {
            string normalized;
            string error;

            var accepted = PlayerNameValidator.TryNormalize("   ", out normalized, out error);

            Assert.That(accepted, Is.False);
            Assert.That(error, Is.EqualTo(PlayerNameValidator.EmptyError));
        }

        /// <summary>
        /// Borne basse exacte - 1. Construite depuis MinLength : la valeur en dur cacherait un
        /// changement de borne.
        /// </summary>
        [Test]
        public void PlayerNameValidatorRejectsNameJustBelowMinLength()
        {
            var justTooShort = new string('a', PlayerNameValidator.MinLength - 1);

            string normalized;
            string error;
            var accepted = PlayerNameValidator.TryNormalize(justTooShort, out normalized, out error);

            Assert.That(accepted, Is.False, "un nom de " + justTooShort.Length + " caractere(s) doit etre refuse");
            Assert.That(error, Is.EqualTo(PlayerNameValidator.TooShortError));
        }

        /// <summary>
        /// Borne basse exacte. Sans ce cas, remplacer la garde par "&lt;=" laisserait toute la suite verte :
        /// aucun autre test n'accepte un nom aussi court.
        /// </summary>
        [Test]
        public void PlayerNameValidatorAcceptsNameAtExactMinLength()
        {
            var minLengthName = new string('a', PlayerNameValidator.MinLength);

            string normalized;
            string error;
            var accepted = PlayerNameValidator.TryNormalize(minLengthName, out normalized, out error);

            Assert.That(accepted, Is.True, error);
            Assert.That(normalized, Is.EqualTo(minLengthName));
            Assert.That(normalized.Length, Is.EqualTo(PlayerNameValidator.MinLength));
        }

        [Test]
        public void PlayerNameValidatorAcceptsNameAtExactMaxLength()
        {
            var maxLengthName = new string('a', PlayerNameValidator.MaxLength);

            string normalized;
            string error;
            var accepted = PlayerNameValidator.TryNormalize(maxLengthName, out normalized, out error);

            Assert.That(accepted, Is.True, error);
            Assert.That(normalized, Is.EqualTo(maxLengthName));
            Assert.That(normalized.Length, Is.EqualTo(PlayerNameValidator.MaxLength));
        }

        /// <summary>Borne haute exacte + 1 : le premier nom refuse, et non un nom deux fois trop long.</summary>
        [Test]
        public void PlayerNameValidatorRejectsNameJustAboveMaxLength()
        {
            var justTooLong = new string('a', PlayerNameValidator.MaxLength + 1);

            string normalized;
            string error;
            var accepted = PlayerNameValidator.TryNormalize(justTooLong, out normalized, out error);

            Assert.That(accepted, Is.False, "un nom de " + justTooLong.Length + " caracteres doit etre refuse");
            Assert.That(error, Is.EqualTo(PlayerNameValidator.TooLongError));
        }

        [Test]
        public void PlayerNameValidatorRejectsTooLongName()
        {
            string normalized;
            string error;
            var fortyCharacters = new string('a', 40);

            var accepted = PlayerNameValidator.TryNormalize(fortyCharacters, out normalized, out error);

            Assert.That(accepted, Is.False);
            Assert.That(error, Is.EqualTo(PlayerNameValidator.TooLongError));
        }

        /// <summary>
        /// Les messages de borne doivent citer la borne reelle : recopier "2" et "20" en dur les
        /// laisserait mentir apres un changement de constante, sans qu'aucune comparaison aux
        /// constantes ne le voie.
        /// </summary>
        [Test]
        public void PlayerNameValidatorBoundMessagesQuoteTheActualBounds()
        {
            Assert.That(PlayerNameValidator.TooShortError, Does.Contain(PlayerNameValidator.MinLength.ToString()));
            Assert.That(PlayerNameValidator.TooLongError, Does.Contain(PlayerNameValidator.MaxLength.ToString()));
        }

        [TestCase("a<b>")]
        [TestCase("Kenan!")]
        [TestCase("Ken/an")]
        [TestCase("Ken\tan")]
        public void PlayerNameValidatorRejectsForbiddenCharacters(string raw)
        {
            string normalized;
            string error;

            var accepted = PlayerNameValidator.TryNormalize(raw, out normalized, out error);

            Assert.That(accepted, Is.False, raw);
            Assert.That(error, Is.EqualTo(PlayerNameValidator.ForbiddenCharacterError));
            Assert.That(normalized, Is.Empty, "un nom refuse ne doit produire aucune valeur normalisee");
        }

        [Test]
        public void PlayerNameValidatorTrimsBorderWhitespace()
        {
            string normalized;
            string error;

            var accepted = PlayerNameValidator.TryNormalize("  Kenan  ", out normalized, out error);

            Assert.That(accepted, Is.True, error);
            Assert.That(normalized, Is.EqualTo("Kenan"));
        }

        [TestCase("Kenan")]
        [TestCase("Kenan 2")]
        [TestCase("Ken-an_3")]
        public void PlayerNameValidatorAcceptsAllowedCharacters(string raw)
        {
            string normalized;
            string error;

            var accepted = PlayerNameValidator.TryNormalize(raw, out normalized, out error);

            Assert.That(accepted, Is.True, error);
            Assert.That(normalized, Is.EqualTo(raw));
        }

        // ---------------------------------------------------------------- Catalogue livre

        [Test]
        public void CharacterCatalogAssetExistsAndHoldsAtLeastTwoCharacters()
        {
            var catalog = LoadCatalog();

            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(2), "au moins deux personnages placeholder sont attendus");
        }

        [Test]
        public void CharacterCatalogAssetPassesItsOwnValidationGuard()
        {
            var catalog = LoadCatalog();

            string error;
            Assert.That(catalog.TryValidate(out error), Is.True, error);
        }

        [Test]
        public void CharacterCatalogIdsAreLowercaseUniqueAndNonEmpty()
        {
            var catalog = LoadCatalog();
            var seen = new HashSet<string>();

            for (var i = 0; i < catalog.Count; i++)
            {
                var character = catalog.GetAt(i);
                Assert.That(character != null, Is.True, "entree de catalogue nulle a l'index " + i);

                var id = character.Id;
                Assert.That(id.IsEmpty, Is.False, "id vide a l'index " + i);
                Assert.That(id.Value, Is.EqualTo(id.Value.ToLowerInvariant()), "id non minuscule : " + id.Value);
                Assert.That(id.Value, Is.EqualTo(id.Value.Trim()), "id borde d'espaces : '" + id.Value + "'");
                Assert.That(seen.Add(id.Value), Is.True, "id duplique : " + id.Value);
            }
        }

        [Test]
        public void CharacterCatalogResolvesCharactersByStableId()
        {
            var catalog = LoadCatalog();
            var first = catalog.GetAt(0);

            CharacterDef resolved;
            Assert.That(catalog.TryGetById(first.Id, out resolved), Is.True);
            Assert.That(resolved, Is.SameAs(first));
            Assert.That(catalog.IndexOf(first.Id), Is.EqualTo(0));
        }

        [Test]
        public void CharacterCatalogIsInertOnOutOfRangeIndexAndUnknownId()
        {
            var catalog = LoadCatalog();

            Assert.That(catalog.GetAt(-1), Is.Null);
            Assert.That(catalog.GetAt(catalog.Count), Is.Null);

            CharacterDef resolved;
            Assert.That(catalog.TryGetById(new DefinitionId("char_does_not_exist"), out resolved), Is.False);
            Assert.That(resolved, Is.Null);
        }

        [Test]
        public void CharacterDefsHavePreviewPrefabSlotReadyForStory14()
        {
            var catalog = LoadCatalog();

            for (var i = 0; i < catalog.Count; i++)
            {
                var character = catalog.GetAt(i);
                Assert.That(character.PreviewPrefab != null, Is.True,
                    "le greybox de personnage doit etre assigne depuis la Story 1.4 : " + character.RawId);
                Assert.That(character.PreviewPrefab.name, Does.StartWith("Greybox_Character_"));
            }
        }

        // ---------------------------------------------------------------- Branches de refus de TryValidate
        // Chaque branche est observee en train de retourner false sur un catalogue construit en memoire :
        // sans cela, supprimer une garde ne ferait echouer aucun test.

        [Test]
        public void CharacterCatalogValidationRejectsEmptyList()
        {
            var catalog = NewCatalog();

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False);
            Assert.That(error, Does.Contain("vide"));
        }

        [Test]
        public void CharacterCatalogValidationRejectsNullEntry()
        {
            var catalog = NewCatalog(NewDef("char_rookie"), null);

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "une entree nulle doit etre refusee");
            Assert.That(error, Does.Contain("nulle"));
            Assert.That(error, Does.Contain("1"), "le message doit situer l'entree fautive");
        }

        [TestCase("")]
        [TestCase("   ")]
        public void CharacterCatalogValidationRejectsEmptyId(string id)
        {
            var catalog = NewCatalog(NewDef(id));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un id vide doit etre refuse");
            Assert.That(error, Does.Contain("vide"));
        }

        /// <summary>
        /// Un id borde d'espaces passe IsNullOrWhiteSpace et la comparaison de casse : sans garde
        /// dediee, ces espaces voyageraient dans le DefinitionId jusqu'a la synchro reseau.
        /// </summary>
        [TestCase(" char_rookie")]
        [TestCase("char_rookie ")]
        [TestCase(" char_rookie ")]
        public void CharacterCatalogValidationRejectsPaddedId(string id)
        {
            var catalog = NewCatalog(NewDef(id));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un id borde d'espaces doit etre refuse : '" + id + "'");
            Assert.That(error, Does.Contain("espaces"));
        }

        [TestCase("Char_Rookie")]
        [TestCase("CHAR_ROOKIE")]
        public void CharacterCatalogValidationRejectsNonLowercaseId(string id)
        {
            var catalog = NewCatalog(NewDef(id));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "un id non minuscule doit etre refuse : " + id);
            Assert.That(error, Does.Contain("minuscule"));
        }

        [Test]
        public void CharacterCatalogValidationRejectsDuplicateIds()
        {
            var catalog = NewCatalog(NewDef("char_rookie"), NewDef("char_rookie"));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.False, "deux ids identiques doivent etre refuses");
            Assert.That(error, Does.Contain("duplique"));
            Assert.That(error, Does.Contain("char_rookie"));
        }

        [Test]
        public void CharacterCatalogValidationAcceptsAWellFormedInMemoryCatalog()
        {
            var catalog = NewCatalog(NewDef("char_rookie"), NewDef("char_veteran"));

            string error;
            Assert.That(catalog.TryValidate(out error), Is.True, error);
            Assert.That(error, Is.Empty);
        }

        // ---------------------------------------------------------------- Profil de session

        [Test]
        public void PlayerProfileAndStoreArePlainClassesWithoutUnityBaseTypes()
        {
            foreach (var type in new[] { typeof(PlayerProfile), typeof(PlayerProfileStore) })
            {
                Assert.That(typeof(MonoBehaviour).IsAssignableFrom(type), Is.False, type.Name + " must not be a MonoBehaviour");
                Assert.That(typeof(ScriptableObject).IsAssignableFrom(type), Is.False, type.Name + " must not be a ScriptableObject");
                Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(type), Is.False, type.Name + " must not derive from UnityEngine.Object");
            }
        }

        [Test]
        public void PlayerProfileStoreStartsEmptyThenHoldsWhatWasSet()
        {
            var store = new PlayerProfileStore();

            Assert.That(store.HasProfile, Is.False);
            Assert.That(store.Current, Is.Null);

            PlayerProfile observed = null;
            store.ProfileChanged += profile => observed = profile;

            var written = new PlayerProfile("Kenan", new DefinitionId("char_rookie"));
            store.Set(written);

            Assert.That(store.HasProfile, Is.True);
            Assert.That(store.Current, Is.SameAs(written));
            Assert.That(observed, Is.SameAs(written), "ProfileChanged doit porter le profil ecrit");
            Assert.That(store.Current.DisplayName, Is.EqualTo("Kenan"));
            Assert.That(store.Current.CharacterId.Value, Is.EqualTo("char_rookie"));
        }

        [Test]
        public void PlayerProfileStoreRefusesNullProfile()
        {
            var store = new PlayerProfileStore();

            Assert.Throws<ArgumentNullException>(() => store.Set(null));
            Assert.That(store.HasProfile, Is.False);
        }

        /// <summary>
        /// Toute mutation publique du depot doit lever ProfileChanged : un chemin muet
        /// desynchroniserait silencieusement l'entree monde de la Story 1.5.
        /// </summary>
        [Test]
        public void PlayerProfileStoreExposesNoSilentMutator()
        {
            var mutators = typeof(PlayerProfileStore)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .Select(method => method.Name)
                .ToArray();

            Assert.That(mutators, Is.EquivalentTo(new[] { "Set" }),
                "seul Set mute le depot ; tout autre mutateur doit lever ProfileChanged avant d'exister");
        }

        [Test]
        public void BootstrapExposesProfileStoreProperty()
        {
            var property = typeof(RoadRage.App.RoadRageBootstrap).GetProperty("Profiles");

            Assert.That(property, Is.Not.Null, "RoadRageBootstrap doit porter le depot de profil de session");
            Assert.That(property.PropertyType, Is.EqualTo(typeof(PlayerProfileStore)));
        }

        // ---------------------------------------------------------------- Frontieres

        [Test]
        public void UiAssemblyDoesNotReferencePlayersAssembly()
        {
            var references = ResolveAsmdefReferences(UiAsmdefPath);

            Assert.That(references, Is.Not.Empty, "les references de RoadRage.Features.UI n'ont pas pu etre lues");
            Assert.That(references, Does.Not.Contain("RoadRage.Features.Players"));
            Assert.That(references, Does.Not.Contain("RoadRage.Features.Lobby"));
        }

        [Test]
        public void PlayersAssemblyDoesNotReferenceUiAssembly()
        {
            var references = ResolveAsmdefReferences(PlayersAsmdefPath);

            Assert.That(references, Is.Not.Empty, "les references de RoadRage.Features.Players n'ont pas pu etre lues");
            Assert.That(references, Does.Not.Contain("RoadRage.Features.UI"));
            Assert.That(references, Does.Not.Contain("RoadRage.Features.Lobby"));
        }

        // ---------------------------------------------------------------- Helpers

        [Serializable]
        private sealed class AsmdefManifest
        {
            public string name = string.Empty;
            public string[] references = new string[0];
        }

        /// <summary>
        /// Lit les references d'un asmdef et resout les entrees "GUID:&lt;hash&gt;" vers le nom
        /// d'assembly cible. Une simple recherche de sous-chaine dans le JSON passerait au vert
        /// inconditionnellement des que l'option "Use GUIDs" de l'Inspector est active, alors meme que
        /// la reference interdite existe : le critere d'acceptation de decoupage serait alors sans garde.
        /// </summary>
        private static string[] ResolveAsmdefReferences(string asmdefPath)
        {
            var manifest = LoadAsmdefManifest(asmdefPath);
            Assert.That(manifest, Is.Not.Null, "asmdef introuvable : " + asmdefPath);

            var resolved = new List<string>();

            foreach (var reference in manifest.references ?? new string[0])
            {
                if (string.IsNullOrEmpty(reference))
                {
                    continue;
                }

                if (!reference.StartsWith("GUID:", StringComparison.Ordinal))
                {
                    resolved.Add(reference);
                    continue;
                }

                var guid = reference.Substring("GUID:".Length);
                var targetPath = AssetDatabase.GUIDToAssetPath(guid);
                Assert.That(targetPath, Is.Not.Empty, "reference GUID non resolue dans " + asmdefPath + " : " + reference);

                var target = LoadAsmdefManifest(targetPath);
                Assert.That(target, Is.Not.Null, "asmdef cible illisible : " + targetPath);
                resolved.Add(target.name);
            }

            return resolved.ToArray();
        }

        private static AsmdefManifest LoadAsmdefManifest(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assetPath);
            if (asset == null)
            {
                return null;
            }

            return JsonUtility.FromJson<AsmdefManifest>(asset.text);
        }

        private CharacterDef NewDef(string id)
        {
            var def = ScriptableObject.CreateInstance<CharacterDef>();
            SetPrivateField(def, "id", id);
            SetPrivateField(def, "displayName", id);
            spawned.Add(def);
            return def;
        }

        private CharacterCatalog NewCatalog(params CharacterDef[] characters)
        {
            var catalog = ScriptableObject.CreateInstance<CharacterCatalog>();
            SetPrivateField(catalog, "characters", new List<CharacterDef>(characters));
            spawned.Add(catalog);
            return catalog;
        }

        private static CharacterCatalog LoadCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CharacterCatalog>(CharacterCatalogAssetPath);
            Assert.That(catalog != null, Is.True, "catalogue introuvable : " + CharacterCatalogAssetPath);
            return catalog;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "field not found: " + fieldName);
            field.SetValue(target, value);
        }
    }
}
