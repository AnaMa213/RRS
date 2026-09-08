---
title: 'Story 1.5 - Entree carte vide et mouvement local a pied'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
---

# Story 1.5 - Entree carte vide et mouvement local a pied

## Intent

**Problem:** Le flux Epic 1 s'arretait dans la coquille de lobby : `Start Game` affichait encore un refus, `MVP_Run` etait vide, et le profil joueur confirme n'etait pas consomme par l'entree monde.

**Approach:** `Start Game` charge maintenant `MVP_Run` seulement quand un profil joueur existe ; la scene contient une carte greybox minimale, un point de spawn, et un controller on-foot local qui instancie le prefab personnage selectionne, attache la camera, marche, sprinte et s'arrete sans Netcode ni gameplay conduite.

## Suggested Review Order

**Flux lobby vers run**

- Garde le refus visible sans profil avant tout chargement de scene.
  [`LobbyFlowController.cs:17`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L17)

- Route l'entree locale vers la scene primaire `MVP_Run`.
  [`AppSceneRouter.cs:24`](../../Assets/RoadRage/App/Services/AppSceneRouter.cs#L24)

**Spawn local**

- Orchestration runtime : profil, catalogue, prefab preview et player local.
  [`RunFlowController.cs:45`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L45)

- Collider gameplay separe du visuel greybox instancie.
  [`RunFlowController.cs:79`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L79)

- Scene cablee avec racines run, spawn, sol, route et placeholders.
  [`MVP_Run.unity:919`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L919)

**Mouvement local**

- Intent local compact, remplacable plus tard par une source reseau.
  [`OnFootMovementIntent.cs:7`](../../Assets/RoadRage/Features/OnFoot/OnFootMovementIntent.cs#L7)

- Motor CharacterController : input clavier/souris, marche, sprint, camera.
  [`LocalOnFootController.cs:77`](../../Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs#L77)

**Verification**

- Tests EditMode couvrent scene, boundaries, routeur et absence de spawn reseau.
  [`Story15EmptyMapEntryTests.cs:43`](../../Assets/RoadRage/Tests/EditMode/Story15EmptyMapEntryTests.cs#L43)

- Tests PlayMode couvrent refus sans profil, chargement, spawn et mouvement.
  [`Story15EmptyMapEntryPlayModeTests.cs:54`](../../Assets/RoadRage/Tests/PlayMode/Story15EmptyMapEntryPlayModeTests.cs#L54)

- Traces finales : EditMode 80/80 et PlayMode 32/32.
  [`story-1-5-playmode-results.txt:34`](story-1-5-playmode-results.txt#L34)
