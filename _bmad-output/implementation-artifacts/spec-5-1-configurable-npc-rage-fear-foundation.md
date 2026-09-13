---
title: "Story 5.1 : fondation configurable Rage/Fear des PNJ"
type: "feature"
created: "2026-09-13"
status: "done"
review_loop_iteration: 0
baseline_commit: "f9a39e6fe2105d9dbb5b1315be58b83204894635"
context:
  ["{project-root}/_bmad-output/implementation-artifacts/epic-5-context.md"]
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La fondation Rage de l'Epic 4 n'a qu'une reaction (`RageValue` + `Disposition`) et aucune peur: les PNJ ne peuvent pas reagir selon leur temperament, qui serait donc code en dur a chaque evolution.

**Approach:** Etendre le composant host-owned `NetworkedRageState` (peur + application d'effet unique), porter les tendances authored dans `RageTuningDef`, et introduire un effet configurable visant la rage, la peur ou les deux.

## Boundaries & Constraints

**Always:**

- `NetworkedRageState` reste l'unique source de verite et gagne `FearValue` a cote de `RageValue`/`Disposition`; NetworkVariables serveur-ecriture, lecture `Everyone`.
- Les tendances vivent dans `RageTuningDef`, jamais mutees a l'execution; les valeurs de session vivent dans les NetworkVariables.
- `TryValidate` reste le contrat des donnees authored (messages sans accents nommant le champ fautif); `RageDisposition` (0-5) fige.
- Testable seule: suite EditMode sans Netcode + controle visuel dans `Dev_RageSandbox`.

**Ask First:**

- Toute modification du comportement des appels existants (`ApplyRageDelta` du slot 0 via `RunFlowController` et son homologue `RageSandboxAutoStart`); cette story n'en prevoit aucune.

**Never:**

- Changer la signature de `ApplyRageDelta(float, RageTuningDef)` ou les noms des champs serialises `id`, `maxRageValue`, `thresholds`.
- Ajouter trafic, boss, archetypes finaux, comportement lie a un niveau, catalogue de definitions partage ou composant reseau de peur separe.
- Referencer une autre `RoadRage.Features.*` depuis `RoadRage.Features.Rage` (garde `RoadRageScaffoldTests`).
- Lever une exception depuis les methodes d'etat: tuning absent ou effet vide = no-op silencieux.

## I/O & Edge-Case Matrix

| Scenario               | Input / State                                                       | Expected Behavior                                                              | Error Handling                               |
| ---------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------ | -------------------------------------------- |
| Canal rage             | `NpcReactionEffect(Rage, 25)`, sensibilite 1, `RageValue` 90        | `RageValue` 100, `Disposition` recalculee, `FearValue` inchangee               | N/A                                          |
| Canal peur             | `NpcReactionEffect(Fear, 25)`, `FearValue` 90, max 100              | `FearValue` 100, rage et `Disposition` inchangees                              | N/A                                          |
| Canaux les deux        | `NpcReactionEffect(Both, 25)`                                       | les deux bougent, `Disposition` recalculee une fois                            | N/A                                          |
| Independance           | deux cibles, effet sur la premiere                                  | seule la premiere change                                                       | N/A                                          |
| Sensibilites invalides | `RageSensitivity` = 0 (canal vise) / -1 / NaN; `maxFearValue` = NaN | 0 = canal inerte; -1 et NaN refusent le tuning (`maxFearValue` 0 reste valide) | `TryValidate` faux, message nommant le champ |
| Entree inerte          | `ReactionChannel.None`, magnitude nulle, ou `tuning` absent         | aucun changement                                                               | no-op silencieux                             |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Shared/Domain/NpcReactionEffect.cs` -- **NOUVEAU**, en `Shared` car `Features.Vehicles` ne peut referencer `Features.Rage`: `ReactionChannel` (`[Flags]` None/Rage/Fear/Both) + `NpcReactionEffect` (`[Serializable]`: `channel`, `magnitude`, ctor, accesseurs, `AffectsRage`/`AffectsFear`, `TryValidate`).
- `Assets/RoadRage/Features/Rage/NetworkedRageState.cs:16-24` -- NetworkVariables a imiter; `:32` `ApplyRageDelta` intacte; ajouter `FearValue`, `ApplyFearDelta`, `ApplyReactionEffect`.
- `Assets/RoadRage/Features/Rage/RageTuningDef.cs:46-57` -- champs prives + accesseurs; `:78` `TryValidate` a etendre; `:116` `ResolveDisposition` reutilise. `RageTuningCatalog.cs:141` en herite: rien a y changer.
- `Assets/RoadRage/Features/Rage/RageStateDebugView.cs:30-48` -- `Render()` par polling; `:50-77` `[ContextMenu]` editor-only, gabarit du declencheur dev.
- `Assets/RoadRage/Tests/EditMode/Story41RageStateModuleAndDefinitionsTests.cs:287-325` -- harness a reprendre (`CreateInstance` + `SetPrivateField` par reflexion, `NewRageState` non spawn).
- `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity:803` -- `RageStateDebugView` deja cable: le nouveau champ dev prend son defaut C#, donc aucune edition de scene ni d'asset.
- Lecture seule: `Features/Vehicles/NetworkedAIVehicleState.cs`, `App/Run/RunFlowController.cs:395-407`, `DevTools/RageSandboxAutoStart.cs:299-303`.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Shared/Domain/NpcReactionEffect.cs` -- creer `ReactionChannel` + `NpcReactionEffect` + `TryValidate` -- contrat du canal d'effet, referencable hors de la feature Rage.
- [x] `Assets/RoadRage/Features/Rage/RageTuningDef.cs` -- ajouter `maxFearValue` (100), `rageSensitivity`, `fearSensitivity` (1), accesseurs, regles de `TryValidate` -- tendances authored sans renommer l'existant.
- [x] `Assets/RoadRage/Features/Rage/NetworkedRageState.cs` -- ajouter `FearValue`, `ApplyFearDelta`, `ApplyReactionEffect` -- etat et application host-authoritative.
- [x] `Assets/RoadRage/Features/Rage/RageStateDebugView.cs` -- afficher la peur + `[ContextMenu]` d'effet (`testReactionChannel` defaut `Both`) -- verification isolee du sandbox.
- [x] `Assets/RoadRage/Tests/EditMode/Story51NpcRageFearFoundationTests.cs` -- fixture couvrant la matrice I/O et le tuning livre -- preuve deterministe sans Netcode.

**Acceptance Criteria:**

- Given la fondation Story 4 close, when une cible recoit un effet, then elle porte rage et peur host-authoritative et chaque cible reste independante.
- Given un effet configure, when il vise la rage, la peur ou les deux, then seuls les canaux vises changent, `Disposition` n'etant recalculee que si la rage change.
- Given un tuning invalide, when `TryValidate` s'execute, then il est refuse avec un message nommant le champ et `RageTuningCatalog.TryValidate` refuse un catalogue qui le contient.
- Given le checkpoint joue en hote, when le declencheur dev vise la peur seule, then le label montre les deux valeurs sans que la rage bouge; et given le diff complet, then aucun controleur de trafic, boss ou archetype final n'apparait.

## Spec Change Log

## Design Notes

Tendances = **multiplicateurs de sensibilite** sur la magnitude de l'effet, pas des deltas absolus: un seul asset suffit donc par temperament, sans catalogue d'archetypes. Le **canal explicite** evite qu'un delta nul rende "n'influence pas" indistinguable de "influence de zero". `ApplyReactionEffect` est nouvelle, donc `ApplyRageDelta` reste le chemin du slot 0 de la Story 4.3 et les tests 4.1/4.3 comme le sandbox restent verts; le recalcul passe par `ResolveDisposition`, unique predicat existant.

## Verification

**Commands:**

- Aucune commande CLI: verification par le Test Runner Unity, a lancer par l'humain (checkpoint de verification du workflow).

**Manual checks (if no CLI):**

- `RoadRage.Tests.EditMode` filtre sur `Story51NpcRageFearFoundationTests`, puis suite complete: attendu vert, sans nouvelle fixture rouge.
- Apres import Unity, verifier que les `.cs.meta` des 2 nouveaux fichiers existent (sans eux git ne suit pas l'asset).
- `Dev_RageSandbox` en Play Mode hote: label avec rage et peur; un effet sur le canal peur seul bouge la peur, pas la rage.

## Suggested Review Order

**Le contrat partage du canal d'effet**

- Canal explicite plutot qu'un couple de deltas signes: "n'influence pas" reste distinguable de "influence de zero".
  [`NpcReactionEffect.cs:11`](../../Assets/RoadRage/Shared/Domain/NpcReactionEffect.cs#L11)

- Effet serialisable porte par la session, jamais un asset; place en Shared pour ne pas franchir la garde d'asmdef.
  [`NpcReactionEffect.cs:26`](../../Assets/RoadRage/Shared/Domain/NpcReactionEffect.cs#L26)

**L'etat host-authoritative et l'application de l'effet**

- Point d'entree du comportement: magnitude modulee par les sensibilites, puis repartie sur les canaux vises.
  [`NetworkedRageState.cs:76`](../../Assets/RoadRage/Features/Rage/NetworkedRageState.cs#L76)

- Peur ajoutee au meme composant: une seule source de verite, ecriture serveur, lecture everyone.
  [`NetworkedRageState.cs:26`](../../Assets/RoadRage/Features/Rage/NetworkedRageState.cs#L26)

- Chemin direct de la peur, sans toucher la rage ni la disposition: les deux canaux restent independants.
  [`NetworkedRageState.cs:54`](../../Assets/RoadRage/Features/Rage/NetworkedRageState.cs#L54)

**Les tendances authored**

- Bornes et sensibilites authored: un asset par temperament, sans catalogue d'archetypes.
  [`RageTuningDef.cs:56`](../../Assets/RoadRage/Features/Rage/RageTuningDef.cs#L56)

- Zero accepte comme canal inerte, negatif et non fini refuses: c'est la regle qui rend la sensibilite configurable.
  [`RageTuningDef.cs:123`](../../Assets/RoadRage/Features/Rage/RageTuningDef.cs#L123)

**La surface de verification du sandbox**

- Label a deux valeurs et declencheur dev par canal: permet de prouver a l'oeil qu'un canal laisse l'autre intact.
  [`RageStateDebugView.cs:81`](../../Assets/RoadRage/Features/Rage/RageStateDebugView.cs#L81)

- Canal du declencheur dev, `Both` par defaut; aucune edition de scene n'est requise grace au defaut C#.
  [`RageStateDebugView.cs:33`](../../Assets/RoadRage/Features/Rage/RageStateDebugView.cs#L33)

**Les preuves**

- Canal rage: clamp haut, disposition recalculee une fois, peur intacte.
  [`Story51NpcRageFearFoundationTests.cs:103`](../../Assets/RoadRage/Tests/EditMode/Story51NpcRageFearFoundationTests.cs#L103)

- Canal peur: la disposition est laissee volontairement incoherente pour prouver l'absence de recalcul.
  [`Story51NpcRageFearFoundationTests.cs:124`](../../Assets/RoadRage/Tests/EditMode/Story51NpcRageFearFoundationTests.cs#L124)

- Independance des cibles: l'effet sur l'une ne touche pas l'autre.
  [`Story51NpcRageFearFoundationTests.cs:251`](../../Assets/RoadRage/Tests/EditMode/Story51NpcRageFearFoundationTests.cs#L251)

- Bornes basses et peur clampee a `MaxFearValue`.
  [`Story51NpcRageFearFoundationTests.cs:271`](../../Assets/RoadRage/Tests/EditMode/Story51NpcRageFearFoundationTests.cs#L271)
