---
title: 'Bugfix - Contraste des labels MainMenuLobby'
type: 'bugfix'
created: '2026-09-08'
status: 'done'
review_loop_iteration: 0
baseline_commit: '4e88188c00ca7b66aee6648425f159d992ccd494'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-1-2-local-lobby-shell-and-match-settings-draft.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-2-2-host-created-private-room-with-join-code.md'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intent

**Problem:** Apres clic sur Create Lobby, le bouton passe bien a `Close Room`, mais `RoomCodeLabel` et `SettingsSummaryLabel` semblent absents dans `MainMenuLobby`. Le handoff precedent supposait un probleme bas niveau TMP, mais le diagnostic runtime montre que les meshes TMP, l'alpha vertex, les refs serialisees et le shader existent bien.

**Approach:** Corriger le contraste UI qui rend les labels blancs illisibles sur des panels blancs opaques, en gardant le style existant des boutons blancs avec texte noir. Ajouter un test EditMode pour verifier que les labels de statut lobby et notice ont un contraste lisible avec leur panel parent.

## Boundaries & Constraints

**Always:** Garder le flux `LobbyShellScreen` -> `LobbyFlowController` intact. Ne pas changer la creation/fermeture de room Steam, le pumping Steamworks, les refs serialisees de boutons, ni les frontieres d'assemblies existantes. La correction doit rester locale a l'apparence de `MainMenuLobby` et a un test de prevention.

**Ask First:** Tout changement de layout majeur, remplacement de TextMeshPro, changement de render pipeline, ou modification des reglages Player/URP globaux.

**Never:** Ne pas masquer le symptome avec un fallback runtime dans `LobbyFlowController`. Ne pas basculer le projet sur un autre shader/API graphique sans preuve. Ne pas enregistrer de valeurs de session dans des assets.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Ouverture lobby | Play presse depuis le menu | `SettingsSummaryLabel` est lisible sur `SetupPanel` | N/A |
| Room ouverte | Create Lobby resout vers `Open` | `RoomCodeLabel` affiche un code lisible sur `SetupPanel` et le bouton reste lisible | N/A |
| Notice visible | Une action publie une notice | `noticeText` reste lisible sur `NoticePanel` | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- `MenuPanel`, `SetupPanel` et `NoticePanel` ont des `Image` blanches opaques; les labels libres (`titleLabel`, `settingsSummaryLabel`, `roomCodeLabel`, `noticeText`) sont blancs. Les boutons gardent des labels noirs, ce qui explique pourquoi `Close Room` est visible alors que les infos de room ne le sont pas.
- `Assets/RoadRage/Features/UI/LobbyShellScreen.cs` -- `ShowSettingsSummary` et `ShowRoomCreated` mutent bien les textes; pas de changement de logique attendu.
- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` -- appelle deja `ShowRoomCreated` sur `LobbyRoomStatus.Open` et `ShowSettingsSummary` au `Awake`; pas de changement attendu.
- `ProjectSettings/ProjectSettings.asset` -- WindowsStandaloneSupport force Direct3D11 avant Direct3D12; la piste API graphique n'est pas la cause principale observee.
- `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset` -- font asset/material/shader TMP presents et supportes; le probleme est de lisibilite, pas d'asset manquant.
- `Assets/RoadRage/Tests/EditMode/Story12LobbyShellTests.cs` -- emplacement naturel pour ajouter un test de contraste scene/lobby.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- donner un fond sombre coherent aux panels qui portent des labels blancs -- rend les labels visibles sans changer les textes ni les comportements.
- [x] `Assets/RoadRage/Tests/EditMode/Story12LobbyShellTests.cs` -- ajouter un test de contraste pour `titleLabel`, `noticeText`, `settingsSummaryLabel` et `roomCodeLabel` -- empeche le retour blanc-sur-blanc.

**Acceptance Criteria:**
- Given `MainMenuLobby` est chargee, when les labels de lobby et notices sont inspectes, then leur contraste avec leur panel parent depasse le seuil lisible.
- Given le joueur ouvre le setup puis cree une room, when `RoomCodeLabel` recoit `Code : <id>`, then le texte est lisible sans changer le flux de creation de room.

## Spec Change Log

_Retroactively filed: the fix was implemented and committed (`6a8e1a0`) before this spec file was persisted to disk._

## Verification

**Commands:**
- `Unity_RunCommand` diagnostic contraste -- expected: panels sombres, labels blancs, meshes TMP actifs.
- Suite EditMode ciblee `Story12LobbyShellTests` -- expected: tous les tests verts.
- Suite PlayMode ciblee `Story22HostCreatedPrivateRoomPlayModeTests` si l'environnement Steam le permet -- expected: le bouton passe a `Close Room`, `RoomCodeLabel.text` contient le JoinCode, aucune regression logique.
