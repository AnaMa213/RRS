---
title: 'Story 1.4 - Premiers seeds greybox personnage, voiture et batiment'
type: 'feature'
created: '2026-09-08'
status: 'done'
review_loop_iteration: 0
baseline_commit: '6f3185251e9ab094a6d25ee76e6360526e3aa2b1'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-1-context.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-1-3-rough-character-creation-and-player-profile-selection.md'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intent

**Problem:** La Story 1.3 expose des `CharacterDef.previewPrefab` volontairement vides, et les stories 1.5/1.6 n'ont encore aucun prefab concret de personnage, voiture partagee ou bloc urbain a placer dans `MVP_Run`.

**Approach:** Produire un petit pack greybox interne traceable par l'intake Blender : sources `.blend`, exports FBX controles, prefabs Unity avec colliders separes et metadonnees de source/echelle/remplacement, puis brancher les `CharacterDef` existants sur les prefabs de personnage.

## Boundaries & Constraints

**Always:** Garder les assets simples, internes, gratuits et remplacables, mais reconnaissables : personnages en silhouette humanoide bipede (tete, torse, bras, jambes), voiture avec carrosserie/cabine/roues lisibles, batiment avec volume urbain, facade, porte ou fenetres ; noms, ids et dimensions stables en minuscules (`char_rookie`, `char_veteran`, `vehicle_player_shared`, `building_city_block_a`) ; root prefab a transform propre `0/0/0`, scale `1`; child visuel distinct du collider ; pas de `MeshCollider`; documenter chaque prefab dans un composant auteur testable et dans une preuve `docs/setup`; conserver la frontiere `UI`/`Players` de la 1.3.

**Ask First:** Ajouter un asset tiers, genere IA ou telecharge ; modifier une scene gameplay ; enregistrer des prefabs dans `DefaultNetworkPrefabs.asset` ; definir une convention collider plus ambitieuse que box/capsule simples ; remplacer ou renommer `char_rookie`/`char_veteran`.

**Never:** Aucun controller de mouvement, aucun spawn dans `MVP_Run`, aucun rig/animation, aucune logique conduite, aucun package, aucun service reseau ; ne pas compenser une echelle/axe casse par une scale manuelle sur prefab au lieu de corriger l'import/export.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Pack nominal | Projet sans prefabs Story 1.4 | Prefabs `Greybox_Character_Rookie`, `Greybox_Character_Veteran`, `Greybox_PlayerCar`, `Greybox_CityBlock_A` crees sous `Assets/RoadRage/Prefabs/` avec silhouette reconnaissable, renderer, collider simple et metadata complete | N/A |
| Catalogue personnage | `CharacterCatalog.asset` contient `char_rookie` et `char_veteran` | Chaque `CharacterDef.previewPrefab` reference le prefab greybox portant le meme stable id | Test EditMode echoue si une reference est nulle ou mismatchee |
| Intake incomplet | Source/export/prefab ou preuve manquante pour un seed | La story reste non verifiable ; aucun statut `done` | Documenter le blocage plutot que simuler une preuve |
| Collider fragile | Prefab avec `MeshCollider`, collider absent ou collider derive du mesh | Refus par test : collision gameplay doit rester authored separement | Corriger le prefab root |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Players/CharacterDef.cs:29` -- slot `previewPrefab` laisse vide en 1.3 ; Story 1.4 le remplit sans faire connaitre `Players` a `UI`.
- `Assets/RoadRage/Features/Players/CharacterCatalog.cs:85` -- garde ids minuscules/non vides/uniques a conserver ; les ids servent de stable ids pour les prefabs personnage.
- `Assets/RoadRage/ScriptableObjects/Players/CharacterDef_Rookie.asset`, `CharacterDef_Veteran.asset` -- assets a patcher via Unity/SerializedObject, pas a la main dans YAML si l'Editor est disponible.
- `Assets/RoadRage/Prefabs/Prop_Barrel.prefab` et `docs/setup/val-025-prop-barrel-step-5-evidence.md:54` -- patron : root gameplay, child visuel FBX, `BoxCollider` separe, note source/echelle/collider, aucun `NetworkObject` pour prop statique.
- `docs/setup/story-0-6-blender-asset-intake-tutorial.md:30` -- dossiers obligatoires `ArtSource/Blender`, `ArtExports`, `Materials`, `Prefabs`; `:52`/`:58` rappellent AD-13/AD-27.
- `docs/setup/addon-adoption-register.md:40` -- ligne ADDON-005 comme precedent d'un placeholder interne adopte et prouve.
- `Assets/DefaultNetworkPrefabs.asset` -- liste vide ; lecture seule pour cette story sauf approbation explicite.
- `Assets/RoadRage/Tests/EditMode/Story13CharacterSetupTests.cs:268` -- test a remplacer : le slot n'est plus vide apres Story 1.4.
- `Assets/RoadRage/Tests/EditMode/Story13CharacterSetupTests.cs:577` -- helper asmdef robuste a reutiliser pour verifier les frontieres.
- `Assets/RoadRage/Tests/PlayMode/Story13CharacterSetupPlayModeTests.cs:116` -- profil survivant au changement de scene ; la Story 1.5 consommera ces prefabs, pas encore ici.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Shared/Authoring/GreyboxAssetSeedMetadata.cs` -- creer un `MonoBehaviour` auteur avec `StableId`, `SourceAssetPath`, `ExportAssetPath`, `ScaleCheck`, `ColliderPlan`, `ReplacementPolicy` + `TryValidate`.
- [x] `Assets/RoadRage/ArtSource/Blender/Epic1_GreyboxAssetSeeds.blend` et `Assets/RoadRage/ArtExports/Greybox_*.fbx` -- creer/exporter les quatre meshes greybox internes reconnaissables (humanoides, voiture, batiment), avec transforms appliquees, dimensions reelles documentees et material slots minimaux.
- [x] `Assets/RoadRage/Prefabs/Greybox_Character_Rookie.prefab`, `Greybox_Character_Veteran.prefab`, `Greybox_PlayerCar.prefab`, `Greybox_CityBlock_A.prefab` -- creer les prefabs root propres avec child visuel FBX, collider root simple, metadata complete, aucun `MeshCollider`; personnage avec capsule/box simple, voiture avec box conservatrice, building avec box statique.
- [x] `Assets/RoadRage/ScriptableObjects/Players/CharacterDef_Rookie.asset` et `CharacterDef_Veteran.asset` -- assigner `previewPrefab` aux prefabs correspondants sans changer ids, noms ou teintes.
- [x] `docs/setup/story-1-4-greybox-asset-seeds-evidence.md` et `docs/setup/addon-adoption-register.md` -- documenter source/export/prefab, echelle, collider, remplacement, hashes ou mesures Unity ; ajouter une ligne ADDON pour le pack interne.
- [x] `Assets/RoadRage/Tests/EditMode/Story14GreyboxAssetSeedTests.cs` + ajustement du test Story 1.3 obsolet -- couvrir metadata, chemins, colliders, bounds approximatifs, absence de `MeshCollider`, refs catalogue et absence de registration reseau prematuree.

**Acceptance Criteria:**
- Given le projet est inspecte apres import, when les prefabs Story 1.4 sont charges, then ils ont tous un renderer visible, un collider simple separe, une metadata valide et une scale root `1,1,1`.
- Given un humain inspecte les prefabs sans art final, when il voit leur silhouette, then il distingue clairement un humanoide, une voiture et un batiment plutot que des primitives generiques interchangeables.
- Given le catalogue personnage est charge, when chaque `CharacterDef` est resolu, then son `PreviewPrefab` est non nul et porte un stable id identique a `CharacterDef.RawId`.
- Given les assets de reseau sont inspectes, when `DefaultNetworkPrefabs.asset` est lu, then Story 1.4 n'a ajoute aucune registration `NetworkObject` sans spawn reseau reel.

## Spec Change Log

## Design Notes

Le composant metadata evite une documentation uniquement externe qui diverge du prefab. Il reste auteur/validation, pas gameplay : la future 1.5 peut instancier le prefab sans lire ces notes. Les meshes restent greybox, mais leur lecture visuelle doit deja soutenir les tests de flux : un cube seul ne suffit pas pour un personnage, une voiture ou un immeuble.

## Verification

**Commands:**
- `Unity_ReadConsole` (Types `Error`) apres refresh -- attendu : zero erreur de compilation/import.
- `Unity_RunCommand` TestRunnerApi EditMode assembly `RoadRage.Tests.EditMode` -- attendu : suite verte avec `Story14GreyboxAssetSeedTests`.
- `Unity_RunCommand` verification prefab mesuree -- attendu : bounds coherents, root scale `1`, collider present, `MeshCollider` absent, metadata valide.
- `git status --short` -- attendu : seulement spec, docs, sources/exports/prefabs/metas, metadata component, CharacterDef assets et tests de cette story.

**Results 2026-09-08:**
- `Unity_RunCommand` refresh assets/scripts -- pass, console sans erreur apres refresh.
- `Unity_RunCommand` verification prefab mesuree -- pass : root scale `1`, metadata valide, un collider root simple, aucun `MeshCollider`, aucun `NetworkObject`, pieces nommees humanoide/voiture/batiment presentes.
- `Unity_RunCommand` TestRunnerApi EditMode assembly `RoadRage.Tests.EditMode` -- pass : `CASES=74 PASSED=74 FAILED=0 OTHER=0 API_PASS=74 API_FAIL=0 API_SKIP=0 STATE=Passed`.
- `Unity_ReadConsole` (Types `Error`) -- pass : `0` erreur.

**Matrix audit:**
- Pack nominal -- couvert par `GreyboxPrefabsHaveMetadataRendererColliderAndCleanRootScale`, `GreyboxPrefabsHaveRecognizableObjectParts`, `GreyboxPrefabBoundsMatchDocumentedRoughScale` ; tous passes dans le run EditMode.
- Catalogue personnage -- couvert par `CharacterDefsReferenceTheMatchingGreyboxPreviewPrefabs` ; passe dans le run EditMode.
- Intake incomplet -- couvert par `GreyboxSeedSourceAndExportsExistUnderTheIntakeFolders`, `Story14EvidenceAndAdoptionRegisterDocumentThePack`, `GreyboxMetadataRejectsIncompleteAuthoringData` ; tous passes dans le run EditMode.
- Collider fragile -- couvert par `GreyboxPrefabsStayMeshColliderAndNetworkObjectFreeUntilRuntimeStoriesNeedThem` et `GreyboxPrefabsHaveMetadataRendererColliderAndCleanRootScale` ; tous passes dans le run EditMode.

## Suggested Review Order

**Traceabilite**

- Le composant auteur fixe identite, source, echelle, collider et remplacement.
  [`GreyboxAssetSeedMetadata.cs:11`](../../Assets/RoadRage/Shared/Authoring/GreyboxAssetSeedMetadata.cs#L11)

- La validation refuse ids, chemins et notes incomplets avant qu'ils divergent.
  [`GreyboxAssetSeedMetadata.cs:70`](../../Assets/RoadRage/Shared/Authoring/GreyboxAssetSeedMetadata.cs#L70)

- La preuve consigne origine interne, mesures, colliders et politique de remplacement.
  [`story-1-4-greybox-asset-seeds-evidence.md:7`](../../docs/setup/story-1-4-greybox-asset-seeds-evidence.md#L7)

**Prefabs**

- Le prefab rookie montre le patron root, collider, metadata, child FBX.
  [`Greybox_Character_Rookie.prefab:15`](../../Assets/RoadRage/Prefabs/Greybox_Character_Rookie.prefab#L15)

- Les stable ids non-personnage couvrent voiture partagee et batiment.
  [`Greybox_PlayerCar.prefab:70`](../../Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab#L70)

- Les CharacterDef gardent leur identite et recoivent les previews.
  [`CharacterDef_Rookie.asset:18`](../../Assets/RoadRage/ScriptableObjects/Players/CharacterDef_Rookie.asset#L18)

- Le registre adoption declare le pack interne et ses contraintes.
  [`addon-adoption-register.md:42`](../../docs/setup/addon-adoption-register.md#L42)

**Verification**

- Les tests Story 1.4 couvrent prefabs, metadata, bounds, catalogue et reseau.
  [`Story14GreyboxAssetSeedTests.cs:86`](../../Assets/RoadRage/Tests/EditMode/Story14GreyboxAssetSeedTests.cs#L86)

- Le test Story 1.3 accepte maintenant les previews livrees.
  [`Story13CharacterSetupTests.cs:268`](../../Assets/RoadRage/Tests/EditMode/Story13CharacterSetupTests.cs#L268)
