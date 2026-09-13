---
title: "Correctif : frontiere UI et test d'orbite camera"
type: 'bugfix'
created: '2026-09-13'
status: 'done'
baseline_commit: '00dfc03d438d44ccb33f9429243992990088c259'
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Deux tests EditMode echouent pour des raisons distinctes mais issues des corrections Epic 4 : le HUD UI depend directement de la feature PassengerActions, contraire a la frontiere feature-sliced, et la fixture d'orbite Cinemachine injecte la souris par un etat global qui ne produit pas un delta deterministe.

**Approach:** Garder le HUD lecture seule mais faire porter l'adaptation de l'incident par `App/Run`, puis injecter directement le controle `Mouse.delta` dans le test de camera. Aucun comportement gameplay, prefab ni configuration Cinemachine ne doit changer.

## Boundaries & Constraints

**Always:** `Features.UI` ne reference aucune autre `RoadRage.Features.*`; `RunFlowController` reste la composition App autorisee a lire l'etat incident partage et a pousser du texte UI; le HUD ne mute jamais l'incident. Le test camera continue de verifier l'entree native Cinemachine sans facteur frame-time.

**Ask First:** Ajouter une interface, un evenement partage ou une nouvelle dependance d'asmdef.

**Never:** Deplacer l'etat d'incident hors de PassengerActions, modifier le prefab `Greybox_PlayerCar`, changer les gains/liaisons Cinemachine, ou assouplir les assertions de test pour masquer les erreurs.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Incident absent | aucun `NetworkedPassengerActionIncidentState` | HUD affiche aucun incident | aucun throw ni recherche dans UI |
| Incident actif | compteur partage positif | HUD affiche l'incident actif avec son compteur | lecture seule client/host |
| Delta souris | `Mouse.delta=(100,20)` injecte dans la fixture | axes Cinemachine changent de `+12/-2.4` | test echoue si le delta n'est pas lu |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- depend actuellement de `NetworkedPassengerActionIncidentState`; doit devenir un renderer agnostique d'un compteur.
- `Assets/RoadRage/Features/UI/RoadRage.Features.UI.asmdef` -- reference PassengerActions, violation detectee par `RoadRageScaffoldTests`.
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- point de composition qui peut lire l'etat incident et actualiser le HUD chaque frame.
- `Assets/RoadRage/Features/PassengerActions/NetworkedPassengerActionIncidentState.cs` -- etat partage existant, proprietaire de `ActivationCount`; ne pas modifier.
- `Assets/RoadRage/Tests/EditMode/ThirdPersonCameraTests.cs` -- fixture qui doit utiliser `QueueDeltaStateEvent` pour le binding `<Pointer>/delta`.
- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` et `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- configuration deja correcte, lecture seule.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` et `RoadRage.Features.UI.asmdef` -- retirer la dependance PassengerActions et exposer une methode de rendu du compteur.
- [x] `Assets/RoadRage/App/Run/RunFlowController.cs` -- relayer l'etat incident partage vers ce rendu depuis la composition App.
- [x] `Assets/RoadRage/Tests/EditMode/ThirdPersonCameraTests.cs` -- injecter le controle delta de la souris de facon deterministe.

**Acceptance Criteria:**
- Given l'asmdef UI, when le test de scaffold inspecte ses references, then aucune reference `RoadRage.Features.*` n'est presente.
- Given un incident absent ou actif, when `MVP_Run` actualise son HUD, then le texte est correct sans que UI reference PassengerActions ni mute l'etat partage.
- Given le test d'orbite, when il injecte le delta de souris, then les axes changent de `+12/-2.4` et le test est vert.

## Verification

**Commands:**
- Lancer manuellement `RoadRage.Tests.EditMode.RoadRageScaffoldTests.AsmdefsAndNamespacesStayInsideApprovedBoundaries` -- attendu : vert.
- Lancer manuellement `RoadRage.Tests.EditMode.ThirdPersonCameraTests.MouseActionDrivesNativeOrbitWithoutFrameScaling` -- attendu : vert.
- Lancer manuellement `RoadRage.Tests.PlayMode.Story44PassengerActionTwoMvpRunPlayModeTests` -- attendu : HUD d'incident toujours visible.

## Scope extension (approved 2026-09-13)

L'utilisateur a ajoute les trois echecs PlayMode au correctif. Les assertions du menu et du checkpoint
doivent verifier le comportement public apres la refonte (etat menu explicite, deplacement relatif a la
camera). Le Rage Sandbox doit aussi enregistrer ses objets reseau in-scene si un hote residuel ecoute deja.
