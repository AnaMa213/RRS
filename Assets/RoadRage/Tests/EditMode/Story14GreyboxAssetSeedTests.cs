using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Players;
using RoadRage.Shared.Authoring;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 1.4 : verrouille les seeds greybox comme assets reconnaissables, tracables et
    /// remplacables, sans les promouvoir en gameplay reseau ni en scene.
    /// </summary>
    [Category("Core")]
    public sealed class Story14GreyboxAssetSeedTests
    {
        private const string SourceBlendPath = "Assets/RoadRage/ArtSource/Blender/Epic1_GreyboxAssetSeeds.blend";
        private const string EvidencePath = "docs/setup/story-1-4-greybox-asset-seeds-evidence.md";
        private const string AddonRegisterPath = "docs/setup/addon-adoption-register.md";
        private const string DefaultNetworkPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
        private const string CharacterCatalogAssetPath = "Assets/RoadRage/ScriptableObjects/Players/CharacterCatalog.asset";
        private const string VehicleStableId = "vehicle_player_shared";

        /// <summary>
        /// Story 5.10 : ce seed a ete repris comme habillage decoratif du district (trottoirs, places
        /// et tour de repere), en dehors du perimetre du lane graph -- voir
        /// docs/setup/story-1-4-greybox-asset-seeds-evidence.md. Son gabarit et ses parties
        /// reconnaissables ne sont donc plus ceux d'un batiment simple Story 1.4, et sa base descend
        /// legitimement sous le sol (semelle de trottoir), contrairement aux trois autres seeds.
        /// </summary>
        private const string CityBlockStableId = "building_city_block_a";

        private static readonly SeedExpectation[] Seeds =
        {
            new SeedExpectation(
                "Assets/RoadRage/Prefabs/Greybox_Character_Rookie.prefab",
                "Assets/RoadRage/ArtExports/Greybox_Character_Rookie.fbx",
                "char_rookie",
                typeof(CapsuleCollider),
                new Vector3(1.05f, 1.8f, 0.35f),
                new Vector3(1.08f, 1.84f, 0.4f),
                "Head",
                "Torso",
                "LeftArm",
                "RightArm",
                "LeftLeg",
                "RightLeg",
                "LeftFoot",
                "RightFoot"),
            new SeedExpectation(
                "Assets/RoadRage/Prefabs/Greybox_Character_Veteran.prefab",
                "Assets/RoadRage/ArtExports/Greybox_Character_Veteran.fbx",
                "char_veteran",
                typeof(CapsuleCollider),
                new Vector3(1.05f, 1.8f, 0.35f),
                new Vector3(1.08f, 1.84f, 0.41f),
                "Head",
                "Torso",
                "LeftArm",
                "RightArm",
                "LeftLeg",
                "RightLeg",
                "VeteranVisor"),
            new SeedExpectation(
                "Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab",
                "Assets/RoadRage/ArtExports/Greybox_PlayerCar.fbx",
                "vehicle_player_shared",
                typeof(BoxCollider),
                // Story 3.2 : le visuel a ete tourne de -90 degres pour aligner le nez sur +Z
                // (nouvel avant gameplay) ; largeur/longueur sont donc desormais portees par
                // x/z inverses par rapport a l'orientation d'origine de la Story 1.4.
                new Vector3(2.0f, 1.4f, 4.4f),
                new Vector3(2.1f, 1.45f, 4.5f),
                "Body",
                "Cabin",
                "Hood",
                "Trunk",
                "Wheel_FrontLeft",
                "Wheel_FrontRight",
                "Wheel_RearLeft",
                "Wheel_RearRight"),
            // Story 5.10 : repris comme ilot de district decoratif (trottoirs + places autour d'une
            // tour de repere), instancie 4 fois dans MVP_Run -- plus le batiment simple de Story 1.4.
            // Gabarit et parties reconnaissables mesures sur l'etat actuel du prefab (voir preuve).
            new SeedExpectation(
                "Assets/RoadRage/Prefabs/Greybox_CityBlock_A.prefab",
                "Assets/RoadRage/ArtExports/Greybox_CityBlock_A.fbx",
                CityBlockStableId,
                typeof(BoxCollider),
                new Vector3(15.9f, 24.1f, 15.9f),
                new Vector3(16.1f, 24.3f, 16.1f),
                "Tower",
                "Sidewalk",
                "Plaza")
        };

        [Test]
        public void GreyboxSeedSourceAndExportsExistUnderTheIntakeFolders()
        {
            Assert.That(File.Exists(SourceBlendPath), Is.True, SourceBlendPath);

            foreach (var seed in Seeds)
            {
                Assert.That(File.Exists(seed.ExportPath), Is.True, seed.ExportPath);
                Assert.That(seed.ExportPath, Does.StartWith("Assets/RoadRage/ArtExports/"));
                Assert.That(seed.ExportPath, Does.EndWith(".fbx"));
            }
        }

        [Test]
        public void GreyboxPrefabsHaveMetadataRendererColliderAndCleanRootScale()
        {
            foreach (var seed in Seeds)
            {
                var prefab = LoadPrefab(seed.PrefabPath);
                Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one), seed.PrefabPath);

                var metadata = prefab.GetComponent<GreyboxAssetSeedMetadata>();
                Assert.That(metadata != null, Is.True, seed.PrefabPath + " doit porter GreyboxAssetSeedMetadata");
                Assert.That(metadata.StableId, Is.EqualTo(seed.StableId));
                Assert.That(metadata.SourceAssetPath, Is.EqualTo(SourceBlendPath));
                Assert.That(metadata.ExportAssetPath, Is.EqualTo(seed.ExportPath));
                Assert.That(metadata.ScaleCheck, Is.Not.Empty);
                Assert.That(metadata.ColliderPlan, Does.Contain("Collider"));
                Assert.That(metadata.ReplacementPolicy, Is.Not.Empty);
                Assert.That(metadata.VisualReadability, Is.Not.Empty);

                string error;
                Assert.That(metadata.TryValidate(out error), Is.True, error);

                Assert.That(prefab.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0), seed.PrefabPath);
                Assert.That(prefab.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(1), seed.PrefabPath);
                Assert.That(prefab.GetComponent(seed.ColliderType), Is.Not.Null, seed.PrefabPath + " collider root attendu");
            }
        }

        [Test]
        public void GreyboxPrefabsStayMeshColliderAndNetworkObjectFreeUntilRuntimeStoriesNeedThem()
        {
            foreach (var seed in Seeds)
            {
                var prefab = LoadPrefab(seed.PrefabPath);

                Assert.That(prefab.GetComponentsInChildren<MeshCollider>(true), Is.Empty, seed.PrefabPath);

                if (seed.StableId == VehicleStableId)
                {
                    // Story 3.1 : Greybox_PlayerCar porte desormais un NetworkObject (frontiere du
                    // module Vehicules) ; les 3 autres seeds restent NetworkObject-free.
                    Assert.That(prefab.GetComponentsInChildren<NetworkObject>(true), Is.Not.Empty,
                        "Story 3.1 dote Greybox_PlayerCar d'un NetworkObject : " + seed.PrefabPath);
                }
                else
                {
                    Assert.That(prefab.GetComponentsInChildren<NetworkObject>(true), Is.Empty,
                        "Story 1.4 livre des seeds locaux ; la registration reseau attend un spawn reel : " + seed.PrefabPath);
                }
            }
        }

        [Test]
        public void GreyboxPrefabsHaveRecognizableObjectParts()
        {
            foreach (var seed in Seeds)
            {
                var prefab = LoadPrefab(seed.PrefabPath);
                var names = string.Join("|", prefab.GetComponentsInChildren<Transform>(true).Select(t => t.name));

                foreach (var requiredNamePart in seed.RequiredNameParts)
                {
                    Assert.That(names, Does.Contain(requiredNamePart), seed.PrefabPath + " manque " + requiredNamePart);
                }
            }
        }

        [Test]
        public void GreyboxPrefabBoundsMatchDocumentedRoughScale()
        {
            foreach (var seed in Seeds)
            {
                var prefab = LoadPrefab(seed.PrefabPath);
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                Assert.That(instance != null, Is.True, seed.PrefabPath + " doit etre instanciable");

                try
                {
                    var bounds = CalculateRendererBounds(instance);

                    Assert.That(bounds.size.x, Is.InRange(seed.MinBounds.x, seed.MaxBounds.x), seed.PrefabPath + " largeur/longueur");
                    Assert.That(bounds.size.y, Is.InRange(seed.MinBounds.y, seed.MaxBounds.y), seed.PrefabPath + " hauteur");
                    Assert.That(bounds.size.z, Is.InRange(seed.MinBounds.z, seed.MaxBounds.z), seed.PrefabPath + " profondeur");
                    // Le trottoir du district (Story 5.10) porte une semelle qui descend sous le sol
                    // pour ne jamais laisser de vide visible au raccord avec le terrain -- les trois
                    // autres seeds restent des batiments simples poses exactement au sol.
                    var minRenderedY = seed.StableId == CityBlockStableId ? -0.19f : -0.01f;
                    Assert.That(bounds.min.y, Is.GreaterThanOrEqualTo(minRenderedY), seed.PrefabPath + " base sous le sol");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }

        [Test]
        public void CharacterDefsReferenceTheMatchingGreyboxPreviewPrefabs()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CharacterCatalog>(CharacterCatalogAssetPath);
            Assert.That(catalog != null, Is.True, "catalogue introuvable");

            for (var i = 0; i < catalog.Count; i++)
            {
                var character = catalog.GetAt(i);
                Assert.That(character != null, Is.True, "entree catalogue nulle " + i);
                Assert.That(character.PreviewPrefab != null, Is.True, character.RawId + " doit avoir un previewPrefab Story 1.4");

                var metadata = character.PreviewPrefab.GetComponent<GreyboxAssetSeedMetadata>();
                Assert.That(metadata != null, Is.True, "previewPrefab sans metadata : " + character.RawId);
                Assert.That(metadata.StableId, Is.EqualTo(character.RawId));
            }
        }

        [Test]
        public void Story14EvidenceAndAdoptionRegisterDocumentThePack()
        {
            Assert.That(File.Exists(EvidencePath), Is.True, EvidencePath);
            var evidence = File.ReadAllText(EvidencePath);

            foreach (var seed in Seeds)
            {
                Assert.That(evidence, Does.Contain(seed.PrefabPath), seed.PrefabPath);
                Assert.That(evidence, Does.Contain(seed.ExportPath), seed.ExportPath);
                Assert.That(evidence, Does.Contain(seed.StableId), seed.StableId);
            }

            Assert.That(evidence, Does.Contain(SourceBlendPath));
            Assert.That(evidence, Does.Contain("CapsuleCollider"));
            Assert.That(evidence, Does.Contain("BoxCollider"));
            Assert.That(evidence, Does.Contain("aucun MeshCollider"));

            var register = File.ReadAllText(AddonRegisterPath);
            Assert.That(register, Does.Contain("Epic 1 greybox asset seeds"));
            Assert.That(register, Does.Contain("ADDON-007"));
            Assert.That(register, Does.Contain("`Adopt`"));
        }

        [Test]
        public void GreyboxSeedsAreNotRegisteredAsDefaultNetworkPrefabsYet()
        {
            // Depuis la Story 2.5, DefaultNetworkPrefabs.asset contient legitimement le prefab
            // NetworkedPlayerRoot (auto-enregistre par Netcode a l'ajout du NetworkObject) : la
            // liste n'est plus vide par construction. Depuis la Story 3.1, Greybox_PlayerCar y
            // est lui aussi auto-enregistre (NetworkObject ajoute sur son GameObject racine).
            // Cette garde reste utile pour verifier que les 3 autres seeds greybox (assets
            // visuels, Story 1.4) ne s'y retrouvent jamais par accident.
            var networkPrefabText = File.ReadAllText(DefaultNetworkPrefabsPath);

            foreach (var seed in Seeds)
            {
                var guid = AssetDatabase.AssetPathToGUID(seed.PrefabPath);
                Assert.That(guid, Is.Not.Empty, seed.PrefabPath);

                if (seed.StableId == VehicleStableId)
                {
                    Assert.That(networkPrefabText, Does.Contain(guid),
                        "Story 3.1 enregistre Greybox_PlayerCar dans DefaultNetworkPrefabs.asset : " + seed.PrefabPath);
                }
                else
                {
                    Assert.That(networkPrefabText, Does.Not.Contain(guid),
                        "registration reseau prematuree dans DefaultNetworkPrefabs.asset : " + seed.PrefabPath);
                }
            }
        }

        [Test]
        public void GreyboxMetadataRejectsIncompleteAuthoringData()
        {
            var holder = new GameObject("InvalidGreyboxSeedMetadata");
            try
            {
                var metadata = holder.AddComponent<GreyboxAssetSeedMetadata>();

                string error;
                Assert.That(metadata.TryValidate(out error), Is.False);
                Assert.That(error, Is.Not.Empty);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        private static GameObject LoadPrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab != null, Is.True, "prefab introuvable : " + prefabPath);
            return prefab;
        }

        private static Bounds CalculateRendererBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            Assert.That(renderers.Length, Is.GreaterThan(0), root.name);

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private sealed class SeedExpectation
        {
            public SeedExpectation(string prefabPath, string exportPath, string stableId, Type colliderType, Vector3 minBounds, Vector3 maxBounds, params string[] requiredNameParts)
            {
                PrefabPath = prefabPath;
                ExportPath = exportPath;
                StableId = stableId;
                ColliderType = colliderType;
                MinBounds = minBounds;
                MaxBounds = maxBounds;
                RequiredNameParts = requiredNameParts;
            }

            public readonly string PrefabPath;
            public readonly string ExportPath;
            public readonly string StableId;
            public readonly Type ColliderType;
            public readonly Vector3 MinBounds;
            public readonly Vector3 MaxBounds;
            public readonly string[] RequiredNameParts;
        }
    }
}
