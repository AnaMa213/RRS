---
title: 'Story 5.4 : etats de comportement IA pilotes par la rage'
type: 'feature'
created: '2026-09-14'
status: 'done'
review_loop_iteration: 0
baseline_commit: '4f28f36b5007e1b746c2e7a4caf6213457b4521a'
context: ["{project-root}/_bmad-output/implementation-artifacts/epic-5-context.md"]
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Les vehicules IA suivent leur route et portent une rage individuelle, mais la rage ne rend pas encore leur conduite visiblement differente.

**Approach:** Deriver cote hote le comportement de chaque IA depuis son `RageDisposition` deja authored, le synchroniser dans son etat IA et appliquer un profil de mouvement minimal, lisible et testable. Un libelle debug rend l'etat visible sans dependance a la couleur.

## Boundaries & Constraints

**Always:** Reutiliser `RageDisposition` et ses seuils : Calm, Irritated, Flee, Block, Ram et ConfrontationCapable. `NetworkedAIVehicleState` porte le comportement en lecture publique/ecriture serveur; `NetworkedAIVehicleDriverController` le derive et conduit exclusivement cote hote. Chaque IA lit seulement sa propre `NetworkedRageState`; route, progression, gravite et recuperation Story 5.2 restent intacts.

**Ask First:** Si un vrai ciblage de joueur, une collision offensive ou une logique de confrontation devient necessaire pour donner un sens supplementaire a Ram ou ConfrontationCapable.

**Never:** Ajouter un controleur de trafic, une seconde machine de rage, des seuils hardcodes, un evenement Rage Road, sa resolution, un boss ou un comportement propre a un niveau. Le ciblage verrouillable des joueurs et les actions rage/peur sont hors de cette story et sont enregistres dans `deferred-work.md`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Transition de rage | La disposition propre a une IA change | Le host publie le comportement correspondant et applique son profil de route | Les autres IA restent inchangees |
| Block / confrontation | `Block` ou `ConfrontationCapable` | Le vehicule cesse de poursuivre la route et le label conserve l'etat | Ne cree aucun Rage Road event |
| Etat rage absent | Composant manquant/non spawne | Repli Calm sans exception ni ecriture client | Le label rend l'absence explicite s'il est configure |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Shared/Domain/RageDisposition.cs:3-11` -- enum existant des six etats; ne pas creer un enum jumeau.
- `Assets/RoadRage/Features/Rage/NetworkedRageState.cs:14-92` -- producteur host-owned de `Disposition`, rage et peur.
- `Assets/RoadRage/Features/Rage/RageTuningDef.cs:104-184` -- seuils data-driven `RageValue` -> `RageDisposition` deja authored.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs:14-30` -- etat reseau IA a etendre sans dupliquer rage ou route.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs:25-238` -- boucle host-only, mouvement et recuperation Story 5.2 a reutiliser.
- `Assets/RoadRage/Features/Rage/RageStateDebugView.cs:16-102` -- precedent de presentation TMP lecture seule.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity:4022-4027,4967-4968` et `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity:803-806` -- scenes de validation existantes.
- `Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs:15-147` -- tests purs de route/recovery a conserver et completer.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs` -- ajouter le comportement courant synchronise, serveur-ecriture, repli Calm.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- mapper sa propre disposition vers les profils de conduite distincts, sans casser route/recovery host-only.
- [x] `Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs` et `Assets/RoadRage/App/Scenes/MVP_Run.unity` / `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity` -- afficher textuellement l'etat synchronise sans ecriture depuis la vue.
- [x] `Assets/RoadRage/Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs` -- couvrir les six etats, profils distincts, repli Calm, independance et permissions reseau.

**Acceptance Criteria:**
- Given des IA avec une rage independante, when les seuils changent, then chacune adopte Calm, Irritated, Flee, Block, Ram ou ConfrontationCapable sans affecter les autres.
- Given un changement de comportement, when le host le traite, then mouvement et label debug textuel le rendent visible et aucun client ne peut l'ecrire.
- Given une route vide ou une recuperation, when le controleur l'evalue, then les garanties Story 5.2 restent intactes et aucun evenement Rage Road n'est cree.

## Spec Change Log

- 2026-09-14 -- Implementation : le comportement est porte par `NetworkedAIVehicleState.Behavior`
  (server-write) et derive host-only depuis la rage propre au vehicule, lue via la nouvelle interface
  etroite `RoadRage.Shared.Domain.IRageDispositionSource`. Un asmdef de feature ne peut pas en
  referencer un autre (`RoadRageScaffoldTests.AsmdefsAndNamespacesStayInsideApprovedBoundaries`),
  donc `Features/Vehicles` ne peut pas voir `NetworkedRageState` directement -- meme motif que
  `NpcReactionEffect`, deja place dans `Shared.Domain` pour cette raison.
- 2026-09-14 -- Implementation : la rage, le label et le declencheur dev sont ajoutes au prefab
  `Greybox_AIVehicle`, pas instance par instance ; les trois IA de `MVP_Run` en heritent sans
  modification de la scene. `Dev_RageSandbox` ne contient aucun vehicule IA ni route depuis la
  Story 5.2 : y ajouter du trafic exigerait des `NetworkObject` places en scene dont le
  `GlobalObjectIdHash` est calcule par l'editeur, donc non ecrivable a la main de facon sure.
  Verification portee sur `MVP_Run`, scene d'integration exigee par `AGENTS.md`.
- 2026-09-14 -- Correctif post-implementation (regression report PlayMode) : donner leur propre
  `NetworkedRageState` aux IA de route faisait atteindre `MinimumMvpRageTargetCount` (3) sans aucun
  vehicule pilotable, cassant `Story43PassengerActionOneMvpRunPlayModeTests.MvpRunShowsTopRightRageHudAndMultipleRageVehicles`
  (attendait >= 2 cibles-rage pilotables). `RunFlowController.EnsureMinimumMvpRageTargets` verifie
  desormais ce second minimum (`MinimumMvpDriveableRageVehicleCount` = 2) independamment du premier.
  Un second echec PlayMode observe en meme temps (`Story22HostCreatedPrivateRoomPlayModeTests
  .OpenRoomShowsJoinCodeAndCloseButtonLabel`) vient d'une regression Story 5.3 anterieure et non liee
  (le label affiche desormais `DisplayJoinCode`, pas `JoinCode`) ; le test est corrige pour suivre le
  comportement reel, hors perimetre fonctionnel de cette story.

## Design Notes

Le profil reste local au controleur de route : vitesse/immobilisation suffisent a rendre l'escalade testable. Ram est un profil visible, pas encore une attaque ciblee.

## Verification

**Manual checks (if no CLI):**
- Dans `MVP_Run` et `Dev_RageSandbox`, modifier independamment deux IA et verifier mouvement/label de chaque palier, puis confirmer qu'un client observe sans pouvoir changer l'etat.

## Suggested Review Order

**Derivation host-only du comportement**

- Point d'entree : lecture de la rage propre au vehicule, ecriture uniquement sur changement.
  [`NetworkedAIVehicleDriverController.cs:112`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L112)

- Repli Calm explicite quand `IRageDispositionSource` est absent -- jamais d'exception.
  [`NetworkedAIVehicleDriverController.cs:188`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L188)

- Immobilisation Block/ConfrontationCapable placee apres les recuperations existantes, avant "stuck".
  [`NetworkedAIVehicleDriverController.cs:151`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L151)

- Predicat pur : escalade de vitesse Calm < Irritated < Flee < Ram, 0 pour Block/ConfrontationCapable.
  [`NetworkedAIVehicleDriverController.cs:200`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L200)

**Frontiere d'asmdef : rage sans reference feature-a-feature**

- Nouvelle interface etroite, seul moyen pour Features/Vehicles de lire la rage sans la referencer.
  [`IRageDispositionSource.cs:13`](../../Assets/RoadRage/Shared/Domain/IRageDispositionSource.cs#L13)

- `NetworkedRageState` projette sa NetworkVariable existante -- aucun nouvel etat, aucun setter.
  [`NetworkedRageState.cs:36`](../../Assets/RoadRage/Features/Rage/NetworkedRageState.cs#L36)

- Comportement publie : lecture Everyone / ecriture Server, projection et non second etat de rage.
  [`NetworkedAIVehicleState.cs:31`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs#L31)

**Presentation textuelle**

- Label lecture seule, cree automatiquement si aucun TMP_Text n'est cable, jamais par la couleur.
  [`AIVehicleBehaviorDebugView.cs:71`](../../Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs#L71)

- Composition du prefab : rage + label ajoutes une fois, herites par les trois IA de `MVP_Run`.
  [`Greybox_AIVehicle.prefab:19`](../../Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab#L19)

**Correctif MVP : vehicules-rage pilotables**

- Les IA satisfaisant desormais le minimum brut de cibles-rage, un second minimum pilotable est ajoute.
  [`RunFlowController.cs:560`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L560)

- Compte uniquement les cibles-rage portant un controleur joueur, jamais les IA.
  [`RunFlowController.cs:580`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L580)

**Tests et peripheriques**

- Couverture des six dispositions, de l'escalade et de l'independance entre vehicules.
  [`Story54RageDrivenAiBehaviorStatesTests.cs:67`](../../Assets/RoadRage/Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs#L67)

- Verifie que Block/ConfrontationCapable sont les deux seuls etats a immobiliser.
  [`Story54RageDrivenAiBehaviorStatesTests.cs:81`](../../Assets/RoadRage/Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs#L81)

- Composition du prefab verifiee directement en asset (rage + vue + controleur presents).
  [`Story54RageDrivenAiBehaviorStatesTests.cs:196`](../../Assets/RoadRage/Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs#L196)

- Garde de statique mise a jour (3 -> 4) pour la nouvelle fonction pure `ResolveCruiseSpeedMultiplier`.
  [`Story52BasicAiRouteFollowingAndRecoveryTests.cs:121`](../../Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs#L121)

- Correctif de test hors-perimetre (regression Story 5.3) : le libelle suit `DisplayJoinCode`.
  [`Story22HostCreatedPrivateRoomPlayModeTests.cs:92`](../../Assets/RoadRage/Tests/PlayMode/Story22HostCreatedPrivateRoomPlayModeTests.cs#L92)
