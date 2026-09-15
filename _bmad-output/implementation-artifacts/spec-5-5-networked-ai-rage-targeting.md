---
title: 'Story 5.5 : verrouillage de cible IA en reseau pour les actions rage/peur'
type: 'feature'
created: '2026-09-14'
status: 'done'
review_loop_iteration: 0
baseline_commit: 'afddbf05f47eed8550dd3f7bcc417063dc88b659'
context: ["{project-root}/_bmad-output/implementation-artifacts/epic-5-context.md"]
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le lock de cible rage/peur actuel est un etat global de scene (`RunFlowController.passengerActionTarget`, hors story) : une seule cible partagee par tous, jamais validee par type/portee, et `Y` est solo-only.

**Approach:** Remplacer par un lock client-local par joueur sur un vehicule IA eligible (`NetworkedAIVehicleState` + source rage/peur, spawne). `T` verrouille le plus proche, `Y` cycle deterministiquement, la camera suit via le hook existant. Sans lock, une action resout sa cible la plus proche dans sa propre portee, sans lock persistant. L'hote revalide type/spawn/portee/acteur via une resolution partagee, reutilisee par les provocations passager et par le klaxon (conducteur) qui devient lui aussi une action rage/peur host-validee au lieu de rester purement cosmetique.

## Boundaries & Constraints

**Always:** Lock = etat client-local par joueur, jamais une NetworkVariable partagee ; seule l'execution d'une action reste host-authoritative. Eligible = objet spawne portant a la fois `NetworkedAIVehicleState` et une source `IRageDispositionSource` (aujourd'hui `NetworkedRageState`) ; jamais un seul des deux. `T`/`Y` exigent un joueur assis dans un vehicule, meme garde que `TryResolveLocalSeatedVehicleCameraRig` (pas de camera hors vehicule). La resolution cible (lock si en portee, sinon plus proche eligible dans la portee propre de l'action) est extraite en un point unique reutilise par les provocations passager ET le klaxon conducteur -- pas cablee a une seule action. Remplacer entierement `passengerActionTarget`, `focusedRageTargetIndex`, la branche `Y` de `HandleRageTargetDevControls` et `HandleRageTargetCameraLockInteraction` -- ne pas les laisser cohabiter avec le nouveau lock.

**Ask First:** Canal (rage/peur/les deux) et valeurs de portee/magnitude authored a donner au klaxon comme action rage/peur -- aucune valeur n'existe aujourd'hui dans `RageTuningDef`.

**Never:** Controleur de trafic, boss, resolution de confrontation, modification de `NetworkedAIVehicleDriverController`/comportements 5.4 hors de l'effet klaxon ajoute ici. Repli sur une autre cible quand le lock est hors portee. Mutation rage/peur cote client sans revalidation hote. Forcer les regles de siege passager (`PassengerActionValidation`) sur le klaxon, qui reste conducteur-only.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Verrouillage initial | `T`, aucun lock, IA eligibles presentes | La plus proche est verrouillee pour ce joueur, la camera locale la suit | Aucune IA eligible : aucun changement, message affiche |
| Cycle | `Y`, un lock existe, plusieurs IA eligibles | Passage deterministe a la suivante | Une seule IA eligible : lock et camera restent stables |
| Action sans lock | Action declenchee, pas de lock actif | Cible la plus proche dans la portee propre de l'action, sans creer de lock persistant | Aucune IA eligible dans la portee : l'action n'affecte personne |
| Lock hors de portee | Action declenchee, lock actif mais distance > portee de l'action | Aucune mutation | Jamais de repli sur une autre cible |
| Validation hote | Client envoie une reference de cible | L'hote revalide type (IA + source rage/peur), spawn, portee, acteur avant mutation | Cible invalide/non spawnee/hors portee : verdict refuse, aucune mutation |
| Klaxon | Conducteur declenche le klaxon, lock ou cible la plus proche dans sa portee | La cible unique resolue recoit l'effet rage/peur authored, host-valide | Aucune IA eligible en portee : aucun effet, evenement `VehicleHonked` presentation seul conserve |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Run/RunFlowController.cs:56,84,296-353,397-546,436-466,1130-1163` -- etat global et handlers `T`/`Y` actuels (dont `Y` solo-only de `HandleRageTargetDevControls`) a remplacer par un lock par joueur.
- `Assets/RoadRage/Features/PassengerActions/PassengerActionIntent.cs:8-43` -- forme d'intent reseau existante (struct + `NetworkObjectReference`), patron a suivre sans le dupliquer.
- `Assets/RoadRage/Features/PassengerActions/PassengerActionValidation.cs:73-101` -- garde host-side (`TargetValid`/`TargetOutOfRange`) a etendre pour verifier le type IA-eligible.
- `Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs:80-189` -- `RequestSlot`/`SubmitIntentRpc`/`ApplyAuthoritative` : la resolution de cible s'insere avant `Validate`, sans dupliquer ce squelette.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs:19-35` et `Assets/RoadRage/Features/Rage/NetworkedRageState.cs:14-39` -- co-presence + `IsSpawned` definit l'eligibilite ; aucun flag existant a reutiliser.
- `Assets/RoadRage/Shared/Domain/IRageDispositionSource.cs` -- `App/Run` reference deja les deux features, donc l'eligibilite se verifie sans nouvelle interface.
- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs:148-217` -- `SetRageTargetLookOverride`/`ApplyLookAt`, reutilisable tel quel pour suivre la cible en continu. Correctif feel 2026-09-15 : viser la cible en dur sortait la vue de la route des qu'elle passait sur le cote ou derriere ; la visee passe par un relais borne a un cone de conduite (`ResolveRageTargetLookPoint`).
- `Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs:18-50` -- precedent de label texte lecture-seule (5.4) pour rendre le lock visible sans couleur.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs:364-384` -- `RequestHonk`/`HonkRpc`, aujourd'hui "purement cosmetique" : doit appeler la resolution de cible partagee puis appliquer un effet rage/peur host-authoritatif.
- `Assets/RoadRage/Features/Rage/RageTuningDef.cs:104-184` -- seuils authored existants ; y ajouter portee/magnitude/canal du klaxon selon le meme patron.
- `Assets/RoadRage/Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs` -- convention de nommage pour `Story55NetworkedAiRageTargetingTests.cs`.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/App/Run/RunFlowController.cs` -- retirer l'etat/handlers globaux listes ci-dessus ; ajouter un lock client-local par joueur (`T` = plus proche eligible + camera, `Y` = cycle deterministique) via `LocalVehicleCameraRig.SetRageTargetLookOverride` et la garde `TryResolveLocalSeatedVehicleCameraRig`.
- [x] Meme fichier -- ajouter une resolution "plus proche eligible dans une portee donnee", reutilisable par toute action, appelee quand aucun lock n'est actif.
- [x] `Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs` -- dans `ApplyAuthoritative`, exiger `NetworkedAIVehicleState` sur la cible resolue avant `targetValid` -- verification de type portee par l'hote.
- [x] `Assets/RoadRage/Features/Rage/RageTuningDef.cs` -- ajouter portee/magnitude/canal authores pour le klaxon.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- `RequestHonk`/`HonkRpc` resolvent la cible partagee (lock ou plus proche dans la portee klaxon) et appliquent l'effet rage/peur host-authoritatif avec la meme revalidation type/spawn/portee/acteur ; conserve `VehicleHonked` pour la presentation.
- [x] `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` (correctif feel 2026-09-15) -- soft lock : suivi exact de la cible dans un cone de +/- 40 deg autour de l'axe de conduite, maintien au bord du cone (du cote de la cible) au-dela, hysteresis de cote pour une cible pile derriere. Angle expose en champ serialise (knob de feel).
- [x] `Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs` + `RunFlowController.ApplyRageTargetLockMarker` (correctif feel 2026-09-15) -- la cible hors cone reste identifiable via le prefixe texte `[CIBLE]` sur le libelle monde deja porte par chaque IA ; marquage strictement local, jamais replique, donc host et client marquent chacun leur propre lock.
- [x] `Assets/RoadRage/Tests/EditMode/ThirdPersonCameraTests.cs` (correctif feel 2026-09-15) -- cone de visee (dans/hors cone, distance conservee, cote indicatif), hysteresis arriere, et LookAt du siege pointe sur le relais et non sur la cible.
- [x] `Assets/RoadRage/Tests/EditMode/Story55NetworkedAiRageTargetingTests.cs` -- couvrir verrouillage du plus proche, cycle deterministe stable sans alternative, absence de repli hors portee, resolution sans lock bornee a la portee, rejet hote d'une cible non-IA/non spawnee, locks independants entre deux acteurs, effet klaxon host-valide sur la cible unique resolue.

**Acceptance Criteria:**
- Given des vehicules IA eligibles presents, when un joueur assis appuie sur `T`, then le plus proche est verrouille pour lui seul et sa camera locale le suit, sans affecter le lock d'un autre joueur.
- Given un lock actif, when `Y` est presse, then le lock passe a l'IA eligible suivante de facon deterministe, ou reste stable si aucune autre n'existe.
- Given un lock hors de la portee autorisee d'une action, when cette action est declenchee, then aucune mutation n'a lieu et aucune cible de repli n'est choisie.
- Given aucun lock actif, when une action rage/peur est declenchee, then seule l'IA eligible la plus proche dans la portee de cette action est affectee, sans creer de lock persistant.
- Given une cible envoyee par un client, when l'hote la traite, then il revalide type IA-eligible, etat spawne, portee et acteur avant toute mutation de rage/peur.
- Given une cible verrouillee qui passe sur le cote ou derriere le joueur, when le joueur continue de conduire, then la vue reste bornee au cone de conduite (jamais braquee hors route), penche du cote de la cible et ne bat pas gauche/droite quand la cible traverse l'axe arriere ; la cible reste identifiable par son libelle `[CIBLE]` et le HUD rage.
- Given un conducteur qui klaxonne avec un lock ou une IA eligible a portee, when le klaxon est declenche, then l'effet rage/peur authored s'applique uniquement a cette cible unique, host-valide comme les autres actions.

## Verification

**Commands:**
- Aucune commande CLI : verification par le Test Runner Unity (checkpoint humain).

**Manual checks (if no CLI):**
- `RoadRage.Tests.EditMode` filtre `Story55NetworkedAiRageTargetingTests`, puis suite complete verte.
- `MVP_Run` en Play Mode, host + client : chacun verrouille une IA differente avec `T`, cycle avec `Y` sans affecter l'autre joueur, une provocation hors-portee n'affecte personne, et un klaxon conducteur avec/sans lock affecte uniquement la cible unique resolue.

## Suggested Review Order

**Resolution de cible partagee (coeur de la story)**

- Point d'entree unique reutilise par provocations passager et klaxon : lock si eligible/en portee, sinon plus proche, jamais de repli implicite.
  [`AiRageTargetResolution.cs:118`](../../Assets/RoadRage/Features/Vehicles/AiRageTargetResolution.cs#L118)

**Lock client-local par joueur et sa camera**

- `T`/`Y` : verrouille/cycle sur l'IA eligible la plus proche, garde par siege identique a l'ancien hook camera.
  [`RunFlowController.cs:452`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L452)

- Correctif code review : auto-efface le lock quand sa cible est detruite/despawnee, sinon les actions restaient refusees sans retour au joueur.
  [`RunFlowController.cs:531`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L531)

- Applique le lock au HUD, au marqueur IA et a la camera en un seul point d'ecriture.
  [`RunFlowController.cs:556`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L556)

**Validation hote (type/spawn/portee/acteur)**

- Exige `NetworkedAIVehicleState` sur la cible resolue avant `targetValid`, meme chemin pour toute cible envoyee par un client.
  [`NetworkedPassengerActionIntent.cs:176`](../../Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs#L176)

**Klaxon comme action rage/peur host-validee**

- Le klaxon perd son statut purement cosmetique : resout la cible partagee puis applique l'effet host-authoritatif.
  [`NetworkedVehicleDriverController.cs:404`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L404)

- Revalidation type/spawn/portee/acteur identique aux autres actions, avant toute mutation.
  [`NetworkedVehicleDriverController.cs:444`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L444)

- Portee/magnitude/canal authores du klaxon, absents avant cette story.
  [`RageTuningDef.cs:123`](../../Assets/RoadRage/Features/Rage/RageTuningDef.cs#L123)

**Camera : soft lock conique (correctif feel demande en session)**

- Vise un relais borne au cone de conduite plutot que la cible en dur, pour ne jamais sortir la vue de la route.
  [`LocalVehicleCameraRig.cs:194`](../../Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs#L194)

- Marqueur `[CIBLE]` local (jamais replique) sur le libelle IA existant, pour identifier la cible quand elle sort du cone.
  [`AIVehicleBehaviorDebugView.cs:45`](../../Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs#L45)

**Peripheriques (tests, harness dev)**

- Couverture de la resolution, de l'isolation par joueur, du klaxon et de l'auto-clear du lock stale.
  [`Story55NetworkedAiRageTargetingTests.cs:26`](../../Assets/RoadRage/Tests/EditMode/Story55NetworkedAiRageTargetingTests.cs#L26)

- Couverture du cone de visee et de l'hysteresis arriere.
  [`ThirdPersonCameraTests.cs:14`](../../Assets/RoadRage/Tests/EditMode/ThirdPersonCameraTests.cs#L14)

- Complete la composition de la cible harnais (`NetworkedAIVehicleState`) pour rester eligible sous les nouvelles regles.
  [`RageSandboxAutoStart.cs:108`](../../Assets/RoadRage/DevTools/RageSandboxAutoStart.cs#L108)
