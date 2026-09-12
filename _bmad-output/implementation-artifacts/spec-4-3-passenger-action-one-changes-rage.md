---
title: "Story 4.3 : L'action passager un change la rage"
type: 'feature'
created: '2026-09-12'
status: 'in-review'
review_loop_iteration: 0
context: []
baseline_commit: '6c529fccebe6160bf54097fdb326c502b1395c48'
story_key: '4-3-passenger-action-one-changes-rage'
---

<frozen-after-approval reason="human-owned intent -- do not modify unless human renegotiates">

## Intent

**Problem:** Le framework passager valide deja les trois slots mais ne produit encore aucun effet de gameplay; le slot 1 doit maintenant prouver le contrat en augmentant la rage d'une cible visible dans `MVP_Run`.

**Approach:** Brancher un effet de rage minimal sur l'evenement `ActionValidated` existant pour le slot 0, afficher la cible focalisee dans un HUD rage en haut a droite, et enrichir `MVP_Run` avec plusieurs rage targets, un spawn dev verifie et un focus/cycle camera local.

## Boundaries & Constraints

**Always:** Reutiliser `NetworkedPassengerActionIntent.ActionValidated` apres validation hote; appliquer la rage seulement cote hote ou chemin solo autoritaire; garder la validation/cooldown/sequence dans Story 4.2; utiliser `NetworkedRageState.ApplyRageDelta` et le tuning `RageTuningDef`; HUD et camera lisent/observent seulement l'etat partage; les spawn dev verifient la clearance avant placement.

**Ask First:** Ajouter un champ `rageDelta` a `PassengerActionDef`, creer un framework generique d'effets, modifier le contrat de validation passager, ou transformer les rage targets en IA Epic 5.

**Never:** Appliquer incident, ressource, crew-help, recompense, Rage Road, comportement IA, economie ou dismount NPC; laisser un client muter directement `NetworkedRageState`; reutiliser `futureHudLabel` comme seul affichage rage permanent; rendre la camera focus synchronisee comme gameplay state.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Slot 0 valide | Passager assis, cible rage focalisee, cooldown libre | `ActionValidated` applique un delta de rage une fois, le HUD rage montre valeur/disposition changees | N/A |
| Slot 0 refuse | Validation 4.2 refuse acteur, siege, cooldown, sequence, cible ou portee | Aucune rage n'est appliquee, verdict visible existant conserve | Pas de mutation de `RageValue` |
| Cible focalisee change | Plusieurs `NetworkedRageState` existent dans `MVP_Run` | Le cycle local change la cible observee et la prochaine intention cible cette rage target | Message visible si aucune cible |
| Spawn dev | Joueur demande vehicule normal ou rage-target | Vehicule place sur un point libre, drivable immediatement, avec rage state si demande | Refus visible si aucune position libre |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs` -- point d'extension existant: `ActionValidated` est emis uniquement apres `PassengerActionValidation.Validate`; ne pas dupliquer la validation.
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- composition `MVP_Run`: lie l'intent passager, connait `checkpointHud`, gere les entrees solo `E`/`Shift+E`, resout les vehicules et peut porter le focus rage/spawn dev.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- HUD lecture seule; ajouter un label/methode rage dedie plutot que saturer `futureHudLabel`.
- `Assets/RoadRage/Features/Rage/NetworkedRageState.cs` et `RageTuningDef.cs` -- API de mutation autoritaire `ApplyRageDelta(float, RageTuningDef)` et resolution des dispositions.
- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` -- camera conducteur locale; ne pas la reutiliser pour le focus passager, ajouter une observation locale depuis Run si necessaire.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- contient `PassengerActionTarget`, `RunFlowController`, `RunCheckpointHudScreen`; ajouter deux vehicules drivable rage-targets et cabler les nouveaux champs.
- `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity` -- harnais Story 4.1/4.2 pour verifier slot 0 sans dependance Epic 5.
- `Assets/RoadRage/Tests/EditMode/Story42PassengerActionFrameworkTests.cs` -- precedent pour accepted/cooldown/replay; ajouter un test 4.3 qui prouve l'effet unique.
- `Assets/RoadRage/Tests/PlayMode/Story42PassengerActionMvpRunPlayModeTests.cs` -- precedent scene `MVP_Run`; ajouter une suite 4.3 pour HUD, cibles multiples, spawn dev/focus.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/App/Run/RunFlowController.cs` -- ajouter un delta constant Story 4.3 pour le slot 0, s'abonner une seule fois a `ActionValidated`, appliquer `target.ApplyRageDelta`, maintenir la cible rage focalisee, et fournir les controles dev de cycle/spawn avec feedback.
- [x] `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- ajouter un affichage rage dedie avec texte stable pour nom de cible, valeur et disposition; le garder separe des verdicts/vehicules.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- cabler le HUD rage, le tuning, au moins trois rage targets total dont deux vehicules drivable supplementaires, et le prefab/point de spawn dev.
- [x] `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity` -- brancher l'effet slot 0 si le sandbox utilise le meme chemin d'intent, sans casser les diagnostics 4.1/4.2.
- [x] `Assets/RoadRage/Tests/EditMode/Story43PassengerActionOneChangesRageTests.cs` -- couvrir effet slot 0 accepte, rejet sans effet, application unique malgre cooldown/replay, et selection de cible.
- [x] `Assets/RoadRage/Tests/PlayMode/Story43PassengerActionOneMvpRunPlayModeTests.cs` -- verifier `MVP_Run` avec HUD rage top-right, cibles multiples, spawn dev safe et focus/cycle local.

**Acceptance Criteria:**
- Given un passager assis et une rage target focalisee, when il declenche le premier slot MVP, then l'hote valide l'action et la rage de cette cible augmente ou change d'etat.
- Given le HUD `MVP_Run`, when une cible rage est focalisee, then un element ancre en haut a droite affiche clairement son etat.
- Given `MVP_Run`, when la scene demarre, then au moins deux vehicules drivable supplementaires sont aussi configures comme rage targets.
- Given le controle dev de spawn, when un vehicule normal ou rage-target est demande, then il apparait en position libre et drivable, jamais encastre.
- Given plusieurs rage targets, when le passager cycle/focalise une cible, then la camera locale montre la cible choisie et l'action suivante affecte cette cible.

## Spec Change Log

## Design Notes

Delta constant dans `RunFlowController` pour cette story: c'est moins joli qu'un champ `rageDelta` par definition, mais c'est le plus petit contrat qui prouve le slot 0. Ajouter le champ authored quand 4.4/4.5 auront besoin de tunings d'effets distincts.

## Verification

**Commands:**
- `unity command --project-path D:\Projets\RRS recompile --focus false` puis `recompile_status` -- expected: compilation terminee sans erreur.
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.Story43PassengerActionOneChangesRageTests --filter_type testName --async_tests true` -- expected: tous verts.
- `unity command --project-path D:\Projets\RRS run_tests --mode PlayMode --filter RoadRage.Tests.PlayMode.Story43PassengerActionOneMvpRunPlayModeTests --filter_type testName --async_tests true` -- expected: tous verts.
- `git diff --check` -- expected: aucune erreur.

**Manual checks (if no CLI):**
- Depuis `MVP_Run`, entrer comme passager, cycler les rage targets, declencher le slot 1, verifier la hausse de rage dans le HUD top-right, puis spawner un vehicule normal et un rage-target sans encastrement.

## Suggested Review Order

**Effet rage**

- Point d'entree : l'effet slot 0 reste derriere ActionValidated.
  [`RunFlowController.cs:343`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L343)

- L'intent accepte le changement local de cible sans revalider le contrat.
  [`NetworkedPassengerActionIntent.cs:72`](../../Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs#L72)

**Focus et spawn**

- Le cycle resout l'index courant a chaque demande locale.
  [`RunFlowController.cs:355`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L355)

- Les spawns dev refusent les placements occupes avant instanciation.
  [`RunFlowController.cs:484`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L484)

- MVP_Run reutilise le prefab voiture et le tuning rage existants.
  [`MVP_Run.unity:3779`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L3779)

**HUD et sandbox**

- Le HUD garde un champ rage separe des verdicts et degats.
  [`RunCheckpointHudScreen.cs:199`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L199)

- Dev_RageSandbox applique le meme effet slot 0 au harnais.
  [`RageSandboxAutoStart.cs:133`](../../Assets/RoadRage/DevTools/RageSandboxAutoStart.cs#L133)

**Verification**

- EditMode couvre accepte, rejete, cooldown/replay et selection.
  [`Story43PassengerActionOneChangesRageTests.cs:31`](../../Assets/RoadRage/Tests/EditMode/Story43PassengerActionOneChangesRageTests.cs#L31)

- PlayMode couvre HUD, cibles multiples, spawn et focus MVP_Run.
  [`Story43PassengerActionOneMvpRunPlayModeTests.cs:17`](../../Assets/RoadRage/Tests/PlayMode/Story43PassengerActionOneMvpRunPlayModeTests.cs#L17)
