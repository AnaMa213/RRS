---
title: 'Story 3.1 : Prefab de voiture partagee et frontiere du module Vehicules'
type: 'feature'
created: '2026-09-09'
status: 'done'
review_loop_iteration: 0
context: []
baseline_commit: 'c1f274df03e5b267d7aa1aaba56c9803d2070eb4'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le module Vehicules n'a pas encore d'objet reseau dans le monde de jeu : rien ne prouve qu'une voiture partagee peut porter une identite `NetworkObject` et une frontiere de module propre avant que conduite, sieges et degats ne s'y accrochent (Stories 3.2-3.5).

**Approach:** Doter le prefab greybox existant `Greybox_PlayerCar` (Story 1.4) d'un `NetworkObject` et d'un nouveau composant `NetworkedVehicleState` proprietaire du module `RoadRage.Features.Vehicles`, sans toucher a l'art ni au collider, puis s'assurer qu'une instance existe dans `Dev_VehicleSandbox` en plus de `MVP_Run`.

## Boundaries & Constraints

**Always:**
- Le GameObject racine de `Greybox_PlayerCar.prefab` garde son unique `BoxCollider` (m_Size 4.44x1.42x2.06) et son `GreyboxAssetSeedMetadata` (stableId `vehicle_player_shared`) inchanges ; seul l'enfant visuel `Visual_Greybox_PlayerCar` peut changer sans casser l'identite.
- `NetworkedVehicleState` vit dans `RoadRage.Features.Vehicles` (asmdef existant), etend `HostOwnedNetworkStateBehaviour`, et ne reference aucune autre feature slice (Lobby, Economy, Boss, PassengerActions) — la frontiere est deja imposee par l'asmdef, ne pas y toucher.
- La voiture est placee en scene (in-scene `NetworkObject`) dans `MVP_Run.unity` (deja present) et `Dev_VehicleSandbox.unity` (a ajouter) — pas de spawn dynamique via `Resources`.
- L'auto-enregistrement de `Greybox_PlayerCar` dans `Assets/DefaultNetworkPrefabs.asset` des l'ajout du `NetworkObject` est attendu (meme comportement deja observe pour `NetworkedPlayerRoot`) — ne pas le retirer.
- `Story14GreyboxAssetSeedTests.cs` doit etre mis a jour pour exclure uniquement `Greybox_PlayerCar` des gardes "NetworkObject-free" et "absent de DefaultNetworkPrefabs" ; les 3 autres seeds (personnages, batiment) restent inchanges.

**Ask First:** Si l'ajout du `NetworkObject` ne declenche pas automatiquement l'enregistrement dans `Assets/DefaultNetworkPrefabs.asset` (contrairement au comportement observe avec `NetworkedPlayerRoot`), demander avant de l'y ajouter manuellement.

**Never:**
- Ne pas ajouter de `Rigidbody`, de logique de conduite, de gestion de sieges ou de hook de degats (Stories 3.2 a 3.5).
- Ne pas donner a `NetworkedVehicleState` de champ de gameplay pour l'instant — seulement l'identite/frontiere du module, sur laquelle les stories suivantes s'appuieront.
- Ne pas toucher aux 3 autres seeds greybox (`Greybox_Character_Rookie`, `Greybox_Character_Veteran`, `Greybox_CityBlock_A`).

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- ajouter `NetworkObject` + `NetworkedVehicleState` sur le GameObject racine (fileID `6514829079454662262`) ; `BoxCollider`, `GreyboxAssetSeedMetadata` et l'enfant `Visual_Greybox_PlayerCar` (PrefabInstance de `Assets/RoadRage/ArtExports/Greybox_PlayerCar.fbx`) restent inchanges.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs` -- patron existant a suivre (squelette `HostOwnedNetworkStateBehaviour`) pour ecrire `NetworkedVehicleState.cs` dans le meme dossier/namespace `RoadRage.Features.Vehicles`.
- `Assets/RoadRage/Shared/Networking/HostOwnedNetworkStateBehaviour.cs` -- classe de base a etendre (expose `IsHostAuthority => IsServer`).
- `Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef` -- ne reference que `RoadRage.Shared` + `Unity.Netcode.Runtime` ; ne pas y ajouter de reference vers une autre feature.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- contient deja une PrefabInstance de `Greybox_PlayerCar` (guid `33db4bab0111dcf45b577851b4f6bf82`, fileID racine `920814123571126223`) qui heritera automatiquement des composants ajoutes au prefab source ; verifier dans l'editeur apres coup.
- `Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity` -- scene sandbox existante (Camera/Light/Global Volume + un prefab d'environnement), sans voiture pour l'instant ; deja listee dans `ProjectSettings/EditorBuildSettings.asset`.
- `Assets/DefaultNetworkPrefabs.asset` -- contient deja `NetworkedPlayerRoot` (auto-enregistre a l'ajout d'un `NetworkObject`) ; s'attendre au meme comportement pour `Greybox_PlayerCar`.
- `Assets/RoadRage/Tests/EditMode/Story14GreyboxAssetSeedTests.cs` -- test `GreyboxPrefabsStayMeshColliderAndNetworkObjectFreeUntilRuntimeStoriesNeedThem` (~L125-136) et test `GreyboxSeedsAreNotRegisteredAsDefaultNetworkPrefabsYet` (~L220-236) verrouillent aujourd'hui que TOUS les seeds restent `NetworkObject`-free / hors `DefaultNetworkPrefabs.asset`. Le champ `replacementPolicy` du prefab annonce deja "jusqu'a la Story 3.1" -- exclure seulement `Greybox_PlayerCar` de ces deux assertions.
- `Assets/RoadRage/Tests/EditMode/Story31VehicleModuleBoundaryTests.cs` (nouveau) -- suivre le patron NUnit + `AssetDatabase.LoadAssetAtPath<GameObject>` de `Story14GreyboxAssetSeedTests.cs`.
- `Assets/RoadRage/Tests/EditMode/RoadRage.Tests.EditMode.asmdef` -- reference deja `RoadRage.Features.Vehicles`, aucun changement necessaire.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs` -- creer `NetworkedVehicleState : HostOwnedNetworkStateBehaviour`, `sealed`, `[DisallowMultipleComponent]`, `[RequireComponent(typeof(NetworkObject))]`, sans champ de gameplay -- composant proprietaire de la frontiere du module, ancrage pour les stories 3.2/3.3.
- [x] `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- ajouter `NetworkObject` + `NetworkedVehicleState` sur le GameObject racine -- fournit l'identite reseau requise par l'AC.
- [x] `Assets/DefaultNetworkPrefabs.asset` -- verifier l'auto-enregistrement de `Greybox_PlayerCar` declenche par l'ajout du `NetworkObject`, l'accepter sans le retirer.
- [x] `Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity` -- placer une instance du prefab `Greybox_PlayerCar`, positionnee de facon visible pres du point de vue par defaut -- satisfait l'AC "chargeable dans Dev_VehicleSandbox et MVP_Run".
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- verifier dans l'editeur que la PrefabInstance existante herite bien de `NetworkObject`/`NetworkedVehicleState` -- confirme la non-regression sur la scene deja livree.
- [x] `Assets/RoadRage/Tests/EditMode/Story14GreyboxAssetSeedTests.cs` -- exclure `Greybox_PlayerCar` des assertions "NetworkObject-free" et "absent de DefaultNetworkPrefabs", garder les 3 autres seeds inchanges.
- [x] `Assets/RoadRage/Tests/EditMode/Story31VehicleModuleBoundaryTests.cs` -- nouveau fichier : verifier `NetworkObject` + `NetworkedVehicleState` presents, exactement 1 `BoxCollider` et au moins 1 `Renderer` inchanges, presence d'une instance du prefab dans `MVP_Run.unity` et `Dev_VehicleSandbox.unity` (recherche du guid du prefab dans le texte des scenes), et que `RoadRage.Features.Vehicles.asmdef` ne reference ni Lobby, ni Economy, ni Boss, ni PassengerActions.

**Acceptance Criteria:**
- Given le prefab `Greybox_PlayerCar` modifie, when on l'inspecte (editeur ou test EditMode), then il porte un `NetworkObject`, un `NetworkedVehicleState`, exactement un `BoxCollider` inchange et l'art placeholder existant.
- Given les scenes `Dev_VehicleSandbox.unity` et `MVP_Run.unity`, when elles sont ouvertes, then chacune contient une instance du prefab `Greybox_PlayerCar`.
- Given l'assembly `RoadRage.Features.Vehicles`, when on inspecte ses references, then elle ne reference ni Lobby, ni Economy, ni Boss, ni PassengerActions.
- Given `Story14GreyboxAssetSeedTests.cs` apres modification, when la suite EditMode s'execute, then les gardes NetworkObject-free/DefaultNetworkPrefabs restent vertes pour les 3 autres seeds et refletent le changement pour `Greybox_PlayerCar`.

## Spec Change Log

## Design Notes

`NetworkedVehicleState` reste volontairement vide (aucun `NetworkVariable`) : il marque la frontiere du module et sert d'ancrage pour les stories 3.2 (conduite) et 3.3 (sieges), qui y ajouteront leurs propres champs. Meme patron minimal que `NetworkedAIVehicleState`, deja present dans le meme dossier pour l'IA (route uniquement, hors scope ici).

La voiture est placee en scene plutot que spawnee dynamiquement car il n'existe qu'une seule voiture partagee par run — different du prefab `NetworkedPlayerRoot`, instancie une fois par client connecte.

## Verification

**Commands:**
- Unity CLI live-editor `run_tests` EditMode filtre `RoadRage.Tests.EditMode.Story31` -- **non execute** : `TestRunnerApi.Execute` refuse via MCP ("User interactions are not supported for MCP tool calls") et `Unity.exe -batchmode -runTests` refuse car l'Editor interactif detient deja le verrou du projet. Verification de repli : inspection directe du prefab/scenes/asmdef compiles (0 erreur/warning console), contenu conforme a chaque assertion du fichier de test.
- Unity CLI live-editor `run_tests` EditMode filtre `RoadRage.Tests.EditMode.Story14` -- meme limitation ; verifie par inspection directe (guid `Greybox_PlayerCar` present dans `DefaultNetworkPrefabs.asset`, absent pour les 3 autres seeds).
- Confirme par l'utilisateur via `Window > General > Test Runner` (suite EditMode complete) le 2026-09-09 : tous les tests verts apres correction de la regression Story 1.6.

**Manual checks:**
- Ouvert `Dev_VehicleSandbox.unity` et `MVP_Run.unity` via l'editeur (Unity_ManageScene) : la voiture est presente dans les deux, console propre.
- Ecart trouve et corrige : l'instance placee dans `Dev_VehicleSandbox` n'avait pas herite du flag `m_InScenePlaced` (contrairement a celle de `MVP_Run`, deja presente avant la story) ; corrige via `SerializedObject` sur le `NetworkObject` en scene puis resauvegarde -- confirme present dans le fichier scene apres coup.
- Regression trouvee par l'utilisateur en executant la suite EditMode manuellement (Test Runner) : `Story16Epic1PlayableCheckpointTests.GreyboxCharactersAndWorldPropsRemainTraceableAndLocalOnly` verrouillait `Greybox_PlayerCar` comme "sans NetworkObject" (invariant Epic 1 "local-only"), non repere pendant l'investigation initiale (seul `Story14GreyboxAssetSeedTests.cs` avait ete identifie). Corrige : `AssertLocalGreyboxPrefab` prend desormais un parametre `expectNetworkObjectAbsent` ; `Greybox_PlayerCar` (false, NetworkObject attendu depuis la Story 3.1) vs `Greybox_CityBlock_A` (true, inchange). Recompilation verifiee propre (0 erreur/warning).
- Revue risk-scaled (subagent) sur le diff complet : aucun finding. Confirme que `Ownership`, la structure du composant et l'auto-enregistrement suivent exactement le patron deja etabli par `NetworkedPlayerRoot`.

## Suggested Review Order

**Identite reseau et frontiere du module**

- Composant volontairement vide : ancrage pour les stories 3.2/3.3, aucune logique de gameplay ajoutee ici.
  [`NetworkedVehicleState.cs:14`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs#L14)

- `NetworkObject` ajoute sur le GameObject racine du prefab greybox existant, sans toucher au collider ni a l'art.
  [`Greybox_PlayerCar.prefab:90`](../../Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab#L90)

- `NetworkedVehicleState` attache au meme GameObject que le `NetworkObject` (frontiere du module posee ici).
  [`Greybox_PlayerCar.prefab:117`](../../Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab#L117)

**Placement en scene et enregistrement reseau**

- Flag `m_InScenePlaced` corrige manuellement pour l'instance deja presente avant la story (ecart trouve en revue).
  [`MVP_Run.unity:1404`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L1404)

- Nouvelle instance placee dans la scene sandbox dediee, avec le meme flag correctement applique.
  [`Dev_VehicleSandbox.unity:492`](../../Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity#L492)

- Auto-enregistrement Netcode standard (meme comportement que `NetworkedPlayerRoot`), pas une inscription manuelle.
  [`DefaultNetworkPrefabs.asset:23`](../../Assets/DefaultNetworkPrefabs.asset#L23)

**Tests**

- Nouveau verrou : identite reseau, collider et art placeholder inchanges.
  [`Story31VehicleModuleBoundaryTests.cs:25`](../../Assets/RoadRage/Tests/EditMode/Story31VehicleModuleBoundaryTests.cs#L25)

- Garde Story 1.4 ajustee : seul `Greybox_PlayerCar` change de comportement attendu, les 3 autres seeds restent verrouilles.
  [`Story14GreyboxAssetSeedTests.cs:135`](../../Assets/RoadRage/Tests/EditMode/Story14GreyboxAssetSeedTests.cs#L135)

- Meme ajustement cote `DefaultNetworkPrefabs.asset`.
  [`Story14GreyboxAssetSeedTests.cs:248`](../../Assets/RoadRage/Tests/EditMode/Story14GreyboxAssetSeedTests.cs#L248)

- Regression Epic 1 corrigee apres retour utilisateur : invariant "local-only" assoupli uniquement pour la voiture.
  [`Story16Epic1PlayableCheckpointTests.cs:194`](../../Assets/RoadRage/Tests/EditMode/Story16Epic1PlayableCheckpointTests.cs#L194)
