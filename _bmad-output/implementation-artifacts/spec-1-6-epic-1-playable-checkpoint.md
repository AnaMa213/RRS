---
title: 'Story 1.6 - Checkpoint jouable Epic 1'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
---

# Story 1.6 - Checkpoint jouable Epic 1

## Intent

**Problem:** L'Epic 1 devait se terminer sur une preuve jouable de bout en bout, mais le run ne rendait pas encore visibles les etats de checkpoint ni les limites exactes entre ce qui est jouable, stubbe et remplace par l'Epic 2.

**Approach:** Ajouter un HUD placeholder tres leger dans `MVP_Run`, le renseigner depuis le flux de run local, documenter la passation Epic 2, et verrouiller le parcours `Bootstrap -> MainMenuLobby -> MVP_Run` par des tests EditMode et PlayMode dedies.

## Suggested Review Order

**Checkpoint runtime**

- Le flux run renseigne le HUD sans changer l'autorite gameplay.
  [`RunFlowController.cs:30`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L30)

- Les libelles restent un affichage UI pur, sans dependance gameplay.
  [`RunCheckpointHudScreen.cs:11`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L11)

- La scene cable le HUD sous `RunRoot` et le reference depuis le flux.
  [`MVP_Run.unity:1352`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L1352)

**Preuve jouable**

- Le test PlayMode rejoue tout le parcours local jusqu'au sprint en run.
  [`Story16Epic1PlayableCheckpointPlayModeTests.cs:38`](../../Assets/RoadRage/Tests/PlayMode/Story16Epic1PlayableCheckpointPlayModeTests.cs#L38)

- Les tests EditMode verrouillent route, scene, assets, local-only et handoff.
  [`Story16Epic1PlayableCheckpointTests.cs:38`](../../Assets/RoadRage/Tests/EditMode/Story16Epic1PlayableCheckpointTests.cs#L38)

**Passation**

- Les notes separent explicitement jouable, stubs et remplacements Epic 2.
  [`story-1-6-epic-1-playable-checkpoint-notes.md:8`](../../docs/setup/story-1-6-epic-1-playable-checkpoint-notes.md#L8)

- La preuve PlayMode finale couvre 33 cas verts.
  [`story-1-6-playmode-results.txt:37`](story-1-6-playmode-results.txt#L37)

- La preuve EditMode finale couvre 86 cas verts.
  [`story-1-6-editmode-results.txt:89`](story-1-6-editmode-results.txt#L89)
