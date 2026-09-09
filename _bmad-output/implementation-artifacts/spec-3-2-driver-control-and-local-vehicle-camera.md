---
title: 'Story 3.2 : Controle conducteur et camera vehicule locale'
type: 'feature'
created: '2026-09-09'
status: 'in-review'
review_loop_iteration: 0
context: []
baseline_commit: 'b99d9669e13b0c3a20d70c19dca376d97d8022bd'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La voiture partagee (Story 3.1) porte une identite reseau mais ne bouge pas et n'a ni conducteur ni camera : rien ne prouve qu'un input local peut piloter un Rigidbody simule par le host, avec une camera locale jamais synchronisee.

**Approach:** Ajouter au module Vehicules une revendication de conducteur host-arbitree (`NetworkedVehicleState.DriverClientId`), un controleur d'input/physique arcade host-simule (`Rigidbody` + `NetworkTransform` serveur-autoritaire), et un rig de camera Cinemachine strictement local, actif uniquement pour le conducteur local.

## Boundaries & Constraints

**Always:**
- `NetworkedVehicleDriverController` et `LocalVehicleCameraRig` vivent dans `RoadRage.Features.Vehicles` et ne referencent aucune autre feature slice (meme frontiere qu'en 3.1, verrouillee par `RoadRage.Features.Vehicles.asmdef`) — la revendication de conducteur reste un `ulong DriverClientId` propre au module Vehicules, sans toucher `NetworkedPlayerState`/`PlayerMode`/`SeatIndex` (occupation formelle des sieges = Story 3.3).
- Le `Rigidbody` de la voiture n'est simule que sur le host (`IsServer`) ; sur les clients il reste `isKinematic = true` et la position/rotation arrive via `NetworkTransform` (autorite serveur, comportement par defaut NGO) — pas de sync maison en `NetworkVariable<Vector3>`.
- `DriverClientId` n'est ecrit que par le host, apres validation que le siege est libre (sentinel `ulong.MaxValue`) ou deja tenu par le demandeur ; toute revendication alors qu'un autre client est deja conducteur est rejetee sans modifier l'etat, avec un log serveur visible.
- La `CinemachineCamera` enfant du prefab n'est jamais un `NetworkBehaviour` ; son activation est pilotee localement, uniquement quand `DriverClientId.Value == NetworkManager.Singleton.LocalClientId`, desactivee sinon.
- L'entree/sortie formelle des sieges (multi-passagers, sortie vers position sure) reste hors scope — la revendication de 3.2 est une simple touche de claim/release sans proximite au vehicule requise (`Dev_VehicleSandbox` n'a pas de personnage).

**Ask First:** Si l'ajout d'un `CinemachineBrain` sur la Main Camera de `MVP_Run`/`Dev_VehicleSandbox` perturbe visiblement la camera on-foot existante (`LocalOnFootController`, Camera simple reparentee) lors du test manuel, HALT et demander avant de continuer.

**Never:**
- Ne pas toucher `NetworkedPlayerState`, `PlayerMode` ou `SeatIndex` (Story 3.3).
- Ne pas ajouter de degats, de recuperation apres retournement, ni de route/collision avancee (Stories 3.4/3.5).
- Ne pas modifier `Story31VehicleModuleBoundaryTests.cs` sauf si un test existant casse reellement.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Claim libre | Client A revendique, `DriverClientId == sentinel` | Host assigne `DriverClientId = A`, camera locale de A s'active | N/A |
| Claim duplique | Client B revendique alors que `DriverClientId == A` | Etat inchange | Log serveur visible, aucune camera activee pour B |
| Reclaim idempotent | A revendique alors qu'il est deja conducteur | Aucun changement d'etat | N/A |
| Conduite host | Host conducteur, avance/freine/tourne/recule | Rigidbody host repond directement, transform replique aux clients | N/A |
| Conduite client distant | Client distant conducteur envoie l'intent via Rpc | Host valide `SenderClientId == DriverClientId` avant d'appliquer au Rigidbody | Rpc ignoree si sender != DriverClientId |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs` -- ajouter `NetworkVariable<ulong> DriverClientId` (sentinel `ulong.MaxValue`) + `IsDriver(ulong clientId)`.
- `Assets/RoadRage/Features/Players/NetworkedPlayerState.cs:25-43` -- patron de declaration `NetworkVariable` a suivre (lecture seule, ne pas modifier).
- `Assets/RoadRage/App/Run/NetworkedPlayerLifecycleIntent.cs` et `Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs:94-135` -- patron `Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)` valide contre `rpcParams.Receive.SenderClientId`, a suivre pour claim/release et soumission d'intent de conduite.
- `Assets/RoadRage/Features/OnFoot/OnFootMovementIntent.cs` et `Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs:130-163` -- patron `readonly struct` d'intent + lecture `Keyboard.current`, a suivre pour `VehicleDriveIntent`.
- `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs` (nouveau) -- struct Throttle/Steer/BrakeReverse, meme patron que `OnFootMovementIntent`.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` (nouveau) -- `NetworkBehaviour`, `[RequireComponent(typeof(NetworkedVehicleState), typeof(Rigidbody))]` ; claim/release touche, lecture input local, soumission d'intent au host, `FixedUpdate` host-only appliquant une conduite arcade par vitesse cible, grip lateral, freinage fort, steering inverse en marche arriere, `isKinematic = !IsServer` pose dans `OnNetworkSpawn`.
- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` (nouveau) -- `MonoBehaviour` non-networked, reference `NetworkedVehicleState` + `CinemachineCamera` enfant, bascule l'activation locale selon `DriverClientId` vs `NetworkManager.Singleton.LocalClientId`.
- `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- ajouter `Rigidbody`, `NetworkTransform` (`Unity.Netcode.Components`, defaut serveur-autoritaire), `NetworkedVehicleDriverController`, `LocalVehicleCameraRig` + enfant `CinemachineCamera` (inactif par defaut, Follow/LookAt = racine) ; correction follow-up : le root gameplay avance en +Z, le visuel FBX dont le nez etait en +X est tourne a -90 degres, et le `BoxCollider` est longitudinal sur Z pour eviter la conduite par la tranche.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` et `Dev_VehicleSandbox.unity` -- ajouter `CinemachineBrain` sur la Main Camera existante ; `Dev_VehicleSandbox.unity` -- ajouter un plan/sol simple (aucune surface actuelle, confirme par inspection) pour permettre le test de conduite.
- `Assets/RoadRage/Tests/EditMode/Story31VehicleModuleBoundaryTests.cs` -- patron de test a suivre (`AssetDatabase.LoadAssetAtPath`, verification asmdef via `JsonUtility`) pour le nouveau fichier de test.
- `Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity` -- ne contenait aucun `NetworkManager` (seule `Dev_LobbySmokeTest.unity` en a un) : trou decouvert en verification manuelle (aucun personnage, aucune session reseau active). Corrige en y ajoutant un `NetworkManager` local (meme patron que `Dev_LobbySmokeTest` : `UnityTransport` loopback 127.0.0.1:7777, `NetworkPrefabsLists` = `Assets/DefaultNetworkPrefabs.asset`, composant `RoadRageNetcodeSmokeTestAutoStart` pour host auto-start) -- rend la scene reellement testable en solo, sans Steam ni personnage.
- `Assets/RoadRage/Prefabs/Greybox_PlayerCar_LowFriction.physicMaterial` (nouveau) -- assigne au `BoxCollider` du prefab. Le materiau physique par defaut (friction ~0.6) sur une caisse de 1200 kg opposait une friction statique (~7000 N) superieure a l'ancien `motorForce` (4200 N) : la voiture restait totalement figee au sol, `AddForce` ne parvenait jamais a vaincre le frottement. Le nouveau controleur conserve ce materiau bas frottement pour que le grip lateral soit gere par code.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs` -- ajouter `DriverClientId` -- fondation de la revendication de conducteur
- [x] `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs` -- nouveau struct -- capture l'intention locale de conduite
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- claim/release + input + physique host -- coeur de l'AC conduite
- [x] `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` -- bascule camera locale -- AC camera non synchronisee
- [x] `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- `Rigidbody` + `NetworkTransform` + nouveaux composants + `CinemachineCamera` enfant -- cablage
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` / `Dev_VehicleSandbox.unity` -- `CinemachineBrain` + sol dans le sandbox -- rend la conduite testable
- [x] `Assets/RoadRage/Tests/EditMode/Story32DriverControlAndLocalCameraTests.cs` (nouveau) -- verrouille composants, sentinel, camera inactive par defaut, frontiere asmdef
- [x] `Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity` -- ajout d'un `NetworkManager` local auto-host -- corrige un trou de testabilite decouvert en verification (aucune session reseau possible sans lui)
- [x] `Assets/RoadRage/Prefabs/Greybox_PlayerCar_LowFriction.physicMaterial` + assignation au `BoxCollider` -- corrige un bug bloquant decouvert en verification live (friction par defaut > motorForce, voiture totalement figee)
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- remplacer la force brute par un modele arcade a vitesse cible, freinage fort, grip lateral code et steering reverse-aware
- [x] `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- corriger l'axe avant gameplay : root en +Z, visuel tourne depuis son nez +X, collider longitudinal sur Z
- [x] `Assets/RoadRage/Tests/EditMode/Story31VehicleModuleBoundaryTests.cs` / `Story32DriverControlAndLocalCameraTests.cs` -- verrouiller l'axe long +Z, le bumper avant en +Z et l'inversion de direction en recul

**Acceptance Criteria:**
- Given le prefab `Greybox_PlayerCar` avec `NetworkedVehicleState`, when un client revendique le controle conducteur, then le host assigne `DriverClientId` et l'input local de ce client pilote le `Rigidbody` host-simule.
- Given un `DriverClientId` deja assigne, when un autre client tente de revendiquer, then la tentative est rejetee sans changer l'etat et loguee cote host.
- Given un client devient conducteur local, when sa `CinemachineCamera` enfant s'active, then aucune donnee de camera n'est synchronisee sur le reseau (`LocalVehicleCameraRig` n'est pas un `NetworkBehaviour`).
- Given `RoadRage.Features.Vehicles.asmdef`, when inspecte apres la story, then il ne reference toujours aucune autre feature slice.

## Spec Change Log

## Design Notes

Sentinel `ulong.MaxValue` pour `DriverClientId` (aucun conducteur) -- meme esprit que `SeatIndex = -1` dans `NetworkedPlayerState`, mais aucun champ `SeatIndex` n'est ajoute ici : la Story 3.3 branchera l'etat joueur formel.

`NetworkTransform` (`Unity.Netcode.Components`) choisi plutot qu'une sync maison en `NetworkVariable<Vector3>`/`<float>` (patron `NetworkedPlayerPresentation`) car le mouvement est un `Rigidbody` physique complet (position + rotation, y compris retournement possible, cf. Story 3.4), pas une pose `CharacterController` simple -- `NetworkTransform` gere nativement l'interpolation cote client sans code de sync a maintenir.

Modele arcade volontairement simple, sans `WheelCollider` -- coherent avec la decision technique de l'epic. La premiere version `AddForce` + torque donnait un ressenti de camion et masquait le bug d'axe du prefab ; la correction remplace cela par une vitesse longitudinale ciblee, un grip lateral artificiel, un freinage immediatement lisible, une assistance anti-roulis, et une direction inversee uniquement quand la voiture recule. `Cinemachine` est introduit ici en premiere utilisation reelle dans le projet (mandate par `ARCHITECTURE-SPINE.md:138` : "chaque joueur a un rig camera Cinemachine local-only") ; la camera on-foot existante (Camera simple reparentee, Story 1.5) reste une dette anterieure non retouchee par cette story.

## Verification

**Commands:**
- Contrairement a l'hypothese initiale de l'agent d'implementation (memes limitations supposees qu'en 3.1), `TestRunnerApi.Execute` fonctionne en fait via Unity MCP (`Unity_RunCommand`) en routant les callbacks vers `Debug.Log` plutot que le retour synchrone. Suite EditMode complete executee deux fois en direct dans l'orchestration (avant et apres le correctif de friction ci-dessous) : **215/215 tests verts** a chaque fois, dont les 7 `Story32DriverControlAndLocalCameraTests` -- zero regression Epic 1/2/3.1.
- 2026-09-09 follow-up conduite arcade : tests EditMode cibles `Story31VehicleModuleBoundaryTests` + `Story32DriverControlAndLocalCameraTests` via `Unity_RunCommand`/`TestRunnerApi` : **12/12 tests verts**, 0 echec, 0 skip. Smoke check Unity sans reflexion : collider `(2.06, 1.42, 4.44)`, bumper avant en local `z=2.16`, signes steering forward/reverse `1/-1/-1`.

**Manual checks (executees en direct via Unity MCP, Play Mode reel, host local dans `Dev_VehicleSandbox` apres ajout du `NetworkManager` -- voir Code Map) :**
- Claim conducteur par l'utilisateur (touche F reelle) : confirme -- `DriverClientId` assigne, camera vehicule Cinemachine activee visuellement, camera on-foot non perturbee (leve le point "Ask First").
- Conduite : signalee bloquee par l'utilisateur (voiture totalement figee malgre le claim) -- diagnostique en direct (composant de test temporaire en `FixedUpdate`, force continue + inspection `Rigidbody.IsSleeping()`) : la friction par defaut du `BoxCollider` (~0.6) opposait une friction statique (~7000 N sur 1200 kg) superieure a `motorForce` (4200 N), la voiture ne bougeait jamais. Corrige (`Greybox_PlayerCar_LowFriction.physicMaterial`, voir Code Map) et reconfirme en direct : acceleration normale des la premiere frame avec la valeur de production.
- Camera strictement locale (non synchronisee) et rejet de revendication en double (2e client) : non re-testes manuellement apres le correctif de friction (logique de claim/camera inchangee par ce fix) -- couverts par relecture de code et tests structurels ; a confirmer par l'utilisateur avec Multiplayer Play Mode s'il le souhaite.
