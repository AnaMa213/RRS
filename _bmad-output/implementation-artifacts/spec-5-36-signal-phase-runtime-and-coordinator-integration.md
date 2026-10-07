---
title: 'Story 5.36 -- Horloge de phase des feux et evaluation Signalized dans le coordinateur'
type: 'feature'
created: '2026-10-07'
status: 'done'
baseline_commit: 'a0c24665a55e40b434fe5ae3cd93d70ed8fc4549'
review_loop_iteration: 1
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-35-authored-control-kinds-stop-yield-priority-roundabout.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le modele porte deja le plan de feux (`SignalPlan` : groupes, phases ordonnees, etats, durees ; 5.25) et `TrafficFrame` sait projeter une phase courante par mouvement (5.32). Mais aucune horloge hote ne produit cette phase, le coordinateur ignore `Signalized`, et la validation laisse passer un plan sur un carrefour non signalise ou une phase qui omet un groupe. Les feux ne sont donc pas une capacite V2 reelle.

**Approach:** Une horloge de phase hote, possedee par `TrafficV2StepRunner`, alimente `TrafficFrame` a chaque pas ; le coordinateur lit l'etat du premier mouvement de chaque traversee dans la frame du lot et le traite comme une regle de circulation. Validation sur une fixture EditMode deterministe derivee de la croix reelle de `MVP_Run`, dont les fichiers restent inchanges.

## Boundaries & Constraints

**Always:**
- Le modele ne possede que le plan. Phase courante et temps ecoule vivent dans `SignalPhaseController` (hote) et entrent par `TrafficFrame` ; le coordinateur les lit par `Resolve(frameId, reports, frame)`.
- **D1, sens du feu.** Seul `Green` autorise un grant. `Yellow` et `Red` refusent toute nouvelle demande (`SignalStop`) et revoquent un grant tenu non engage (`SignalStop`). Un grant engage (`Engaged` 5.34 : entree franchie ou d < D_stop) est conserve : on s'arrete au jaune si on le peut encore.
- **D2, fail-closed.** Mouvement `Signalized` sans etat dans la frame (plan absent de la frame, frame nulle) : refus ou revocation `SignalUnavailable`. Une absence n'est jamais un vert.
- **D3, regle et non invariant.** Le feu est une etape nommee du lot, apres la sortie : sortie -> feu -> occupant protege -> grants incompatibles -> ancien. Un vert ne leve jamais la protection d'occupant, l'exclusivite des conflits ni le controle de sortie. Un refus de feu ne reserve pas contre les plus jeunes (hors `refused`) et n'entre pas dans le briseur (hors `yielded`) : aucun grant ne nait sur rouge. Aucune exception de feu avant 5.41 ; seul `JunctionCoordinator` lit l'etat de feu pour une permission.
- **D4, validation.** Ajouts au validateur, codes stables ajoutes en fin d'enum :
  - `SignalPlanScopeInvalid` : plan sur un carrefour sans controle `Signalized`, ou membre de groupe qui n'est pas un mouvement d'un controle `Signalized` du carrefour du plan ;
  - `SignalPhaseStatesIncomplete` : une phase n'enonce pas exactement une fois chaque groupe du plan ;
  - `JunctionControlKindsMixed` etendu : `Signalized` ne se melange a aucun autre genre ;
  - lignes d'arret `Signalized` hors de cette validation, comme avant (reduction de D4 par decision proprietaire du 2026-10-07) ; leur validation geometrique arrive avec le futur format d'authoring et l'integration reelle `Signalized`.
- **D5, horloge.** Deterministe : chaque plan demarre a sa phase 0, temps cumule en `double`, avance de `Time.fixedDeltaTime` une fois par pas hote avec modele, apres le lot du coordinateur ; enchainement cyclique. Pas non fini ou ≤ 0 : `ArgumentException`. Zero plan (`MVP_Run`) : liste vide, aucun travail par vehicule.
- Raisons ajoutees par ajout (`SignalStop` = 22, `SignalUnavailable` = 23), `ToText()` invariant de culture, hote seul.

**Ask First:**
- Toute modification de `MVP_Run` (scene, `road-authoring.json`, `road-model.json`, signoff) ou de sa version de modele : HALT.
- Fixture existante rouge (`Story525`, `Story532`, `Story534`, `Story535`) apres les regles D4 : HALT avant d'adapter une assertion.

**Never:**
- Signal absent lu comme vert ; plan factice sur un carrefour non signalise ; phase stockee dans le modele.
- Logique adaptative ou actionnee, phases pietonnes, cout de route sensible aux feux, vert permissif (deux verts dans une zone restent interdits par 5.25).
- Format d'authoring de plan dans `AuthoringDecisions` ; PlayMode ; integration `MVP_Run`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Vert | Demande sur approche verte, rien d'incompatible | `Granted` | N/A |
| Rouge / jaune | Demande sur approche rouge ou jaune | `Denied(SignalStop)` | N/A |
| Passage au jaune | Grant tenu non engage, puis jaune | `Revoked(SignalStop)` | N/A |
| Engage au jaune | Grant engage (d < D_stop ou occupant), puis jaune | `Committed` conserve | N/A |
| Phase absente | Frame sans `SignalPhaseInput` du plan, ou `Resolve` sans frame | `Denied(SignalUnavailable)` | Fail-closed |
| Feu brule | Occupant sans grant sur approche rouge ; demande verte incompatible | Demande verte `Denied(ConflictOccupied)` | Invariant inchange |
| Rouge ancien | Demande rouge ancienne, demande verte plus jeune incompatible | Verte `Granted`, jamais `SeniorRequestPending` | N/A |
| Tout rouge | Demandes valides sur toutes les approches pendant N lots | Aucun grant, `DeadlockBreaks` = 0 | N/A |
| Verts compatibles | Deux mouvements de groupes distincts sans zone commune, verts ensemble | Plan valide, deux grants au meme lot | N/A |
| Ordre melange | Memes rapports, ordre permute | Instantane identique | N/A |
| Frame d'un autre lot | `frame.FrameId` ≠ `frameId` ou autre modele | `ArgumentException` | Diagnostic |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs:265-277,538-575` -- `JunctionControlKind.Signalized`, `SignalState`, `SignalGroup`/`SignalGroupState`/`SignalPhase`/`SignalPlan` : lus seulement.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs` -- codes `:15-128` (dernier `StopLineInvalid = 47`) ; `CheckControlKindsAndLines` `:1017-1070` (melange `:1030`, `continue` Signalized `:1057` conserve) ; `ValidatePlanStructure` `:1109`, `ValidateSignalizedCoverage` `:1191`, `ValidateConflictingGreens` (appels `:598-599`).
- `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs:190-245,355-363,492,609` -- plans compiles ; s_line projetee pour tout genre a ligne.
- `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs:93-97,264,442-466` -- `SignalPhases`, `TryGetSignalState`, `BuildSignals` (refus `UnknownSignalPlan`/`UnknownSignalPhase`/`DuplicateSignalPlan`). `Frame/TrafficFrameInputs.cs:74` `SignalPhaseInput`. Reutiliser tel quel.
- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs` -- `Resolve` `:96` ; grants en attente `:229-257` ; demandes `:282-333` (patron `StopRequired` `:294`) ; `Engaged` `:397` ; briseur `:602`. `JunctionRecords.cs:20-53` (`JunctionReason`, dernier `GrantedDeadlockBreak = 21`).
- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionConflictIndex.cs:184,214` -- `ControlKindOf`, `HasPrecedence` (aucune preseance pour `Signalized`).
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs:104-215` -- `Run` (coordinateur par modele `:113`, frame `:161`, chemin sans acteur `:141`), `Coordinate` `:198-211`. Appele par `App/Run/PortalTrafficSpawner.cs:499` en `FixedUpdate`. `Story533SharedFrameTests.cs:165` garde « aucun etat de decision » sur les champs du runner.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs:424-426` -- refus `Signalized` a l'authoring ; message « Story 5.36 » a actualiser (integration differee), refus conserve.
- Fixtures : `Tests/EditMode/Story535JunctionRulesTests.cs:29-80` (modele `MVP_Run` compile depuis `TrafficV2Settings.ModelPath`, `StopModel` synthetique, helpers `Requesting`/`Batch`/`AssertDecision`) ; `Story525RoadWorldModelTests.cs:263,1348-1398` et `Story532PerceptionTests.cs:120-152,822-835` (plans synthetiques complets, non-regression D4).
- `_bmad-output/implementation-artifacts/deferred-work.md` -- entree 5.25 « codes de signal groupes » (`:363`, laissee ouverte : aucun consommateur ne branche sur ces codes) ; marge D13 (`:547`).

## Tasks & Acceptance

**Execution:**
- [x] `RoadModelValidator.cs` -- regles et codes D4.
- [x] `Signals/SignalPhaseController.cs` (nouveau, `Features/Vehicles/Traffic/Signals/`) -- horloge D5 : construit sur un modele compile, `Current` trie par `PlanId`, `Advance(seconds)`.
- [x] `JunctionRecords.cs`, `JunctionCoordinator.cs` -- raisons D1/D2, parametre `TrafficFrame frame = null` de `Resolve`, etape feu sur demandes et grants en attente, garde de frame.
- [x] `TrafficV2StepRunner.cs` -- controleur par modele (comme le coordinateur), phases passees a `TrafficFrame`, frame passee au lot, avance apres le lot sur les deux chemins ; propriete `Signals`.
- [x] `AuthoredRoadModel.cs` -- message du refus `Signalized`.
- [x] `Tests/EditMode/Story536SignalTests.cs` `[Core][Story536]` -- fixture : la croix de `MVP_Run` (4 controles `Uncontrolled`) passee en `Signalized`, un groupe par approche, phases tout-rouge 2 s puis vert 10 s / jaune 3 s par approche ; variante a groupes par mouvement pour les verts compatibles. Couvre la matrice, chaque refus D4, l'horloge (transitions exactes a dt = 0,02, cycle, determinisme, pas invalide), le runner (avance par pas, zero plan sur `MVP_Run`), le scan « seul le coordinateur lit `TryGetSignalState` » et `MVP_Run` sans plan ni `Signalized`.
- [x] `deferred-work.md` -- entree 5.36 : integration `Signalized` dans `MVP_Run` differee (reouverture : carrefour signalise authore comme exigence produit, avec format d'authoring de plan) ; vert permissif hors modele ; note D13 « aucun travail par vehicule ajoute sur `MVP_Run` ».

**Acceptance Criteria:**
- Given la fixture signalisee, when l'horloge avance, then la phase courante entre par `TrafficFrame`, les grants suivent l'etat de phase par le mecanisme commun du coordinateur et deux membres d'une zone incompatible ne sont jamais verts ensemble.
- Given une source avec un plan sur un carrefour non signalise, une phase sans etat d'un groupe, un `Signalized` melange ou sans plan unique, when elle est validee, then la compilation echoue avec le code dedie.
- Given la 5.36 terminee, when on compare `MVP_Run`, then scene, authoring, modele et signoff sont inchanges, `Story535`, `Story534`, `Story532` et `Story525` restent verts, et la capacite est consignee implementee et integration differee avec sa condition de reouverture.

### Review Findings — 2026-10-07

Review target: `a0c2466..9b024cd`, checked on `da77340`. No confirmed defect in the approved signal runtime; one unresolved verification action. [Review evidence and raw validation diagnostics](code-review-5-35-5-36-2026-10-07.md). Story536 13/13, Story535 42/42 EditMode + 6/6 PlayMode, Story534 48/48 and Story532 36/36 are green with zero Console errors. Story525 was not executed: the specified category selects zero tests.

- [x] [Review][Patch][Medium] R9 — Repair the specified targeted 5.25 non-regression route and execute it through validate.ps1. The fixture has Core only; the two adapted methods carry Story536. `-Profile Story -Story 5.25 -TestMode EditMode` returns `ECHEC: Aucun test EditMode pour la categorie Story525 (AD-8).` This leaves the final non-regression AC unverified; do not substitute a full suite or custom wrapper. [`Story525RoadWorldModelTests.cs:16`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L16).

## Spec Change Log

- **2026-10-07 — Correctif R9 appliqué à la demande du propriétaire.** La fixture 5.25 porte désormais Story525 ; sa garde sur les collections réciproques parcourt le schéma source et ses records imbriqués, sans compter les index runtime du même namespace. La liste des appartenances permises et les assertions de propriété restent identiques. `validate.ps1 -Profile Story -Story 5.25 -TestMode EditMode` : 46/46 verts ; Story536 : 14/14 verts, dont cette garde catégorisée Story536. Non-régressions : Story535 49/49 EditMode + 6/6 PlayMode, Story534 48/48 et Story532 36/36. Zéro erreur Console sur chaque fenêtre. La capacité signalisée reste implémentée et son intégration différée ; aucun PlayMode signalisé ni changement MVP_Run. Spec `done`, sprint `review` selon la règle locale de checkpoint d'epic. [Résultats et diagnostics](code-review-5-35-5-36-2026-10-07.md#application-des-correctifs).

- **2026-10-07 — Requested code review of 5.35 and 5.36.** Four configured layers completed; no confirmed signal-runtime defect after checking D1-D5 and approved deferrals. R9 records a demonstrated verification failure, not a failing signal test. Story and sprint tracking return to `in-progress` pending the missing acceptance proof. Source and MVP_Run scene/authoring/model/signoff are unchanged. No assertions were adapted, no PlayMode signal integration introduced and no full suite run. Raw Story525 diagnostic and successful targeted results are in the linked review report.

- **2026-10-07 -- Reduction de D4 (decision proprietaire, option 1).** Declencheur : le run `Fast` (Core) rendait 23 echecs `StopLineInvalid` dans `Story525RoadWorldModelTests` (20) et `Story550` (3), car le modele synthetique de la 5.25, reutilise par la 5.50, porte sur ses controles `Signalized` une ligne `(-4,0,19) -> (4,0,19)` qui ne coupe pas leurs mouvements (« Ask First » fixture rouge). Amende : la sous-regle « ligne `Signalized` validee comme `Stop`/`Yield` » est retiree de D4 ; le validateur ignore de nouveau les lignes `Signalized`. Evite : modifier les fixtures 5.25/5.50 et le golden `Story550CompatibilityGolden` pour une regle sans consommateur reel. KEEP : toutes les autres regles D4 (`SignalPlanScopeInvalid`, `SignalPhaseStatesIncomplete`, melange `Signalized` refuse). La validation des lignes de feu est consignee dans `deferred-work.md` avec l'integration `Signalized`.
- **2026-10-07 -- Adaptation de deux tests 5.25 aux regles D4 (decision proprietaire, option 1).** Declencheur : apres la reduction ci-dessus, `Fast` rendait 8 echecs, dont 2 lies a D4. `Story525.EveryBehaviorAffectingChangeMovesTheVersion` passait C1/C2 en `Stop` en gardant le plan (`SignalPlanScopeInvalid`) ; `Story525.EachMovementReadsItsKindFromItsSingleAuthoritativeControlBinding` melangeait `Signalized` et `Yield` (`JunctionControlKindsMixed`, et deja rouge depuis la 5.35 par `StopLineInvalid` de la ligne `Yield` synthetique). Amende : la version compare `Stop` et `Yield` sans ligne ni plan (seul le genre differe) ; le genre par liaison est verifie avec C1 `Priority` et C2 `Yield`, sans ligne ni plan. Donnees de fixture et golden inchanges ; les deux tests portent `[Category("Story536")]` ; helper `PhaseForSingleGroup` devenu inutile retire. KEEP : D4 entiere (refus du plan factice et du melange). Six autres echecs `Core` (`Story510`, `Story511`, `Story525.NoSourceRecordOwnsAnAuthorableReciprocalCollection`, `Story550CompatibilityGolden` attendant `v4:`, `Story550.TheDocumentRefuses…`, `Story550.HistoricalTable…`) ne touchent ni aux feux ni aux codes modifies ; non verifies contre la base.
- **2026-10-07 -- Revue (blind-hunter en ligne, edge-case-hunter, verification-gap ; security-review inactive, aucune frontiere reseau).** Patchs : horloge bornee a un tour de cycle avant la boucle (duree minuscule mais positive : boucle hote sans fin) ; test du branchement du runner par l'ordre des appels dans la source (aucun vehicule V2 liable en EditMode, patron `Story533SharedFrameTests`) : frame avec phases, lot avec frame, avance apres le lot ; cas `SignalPlanScopeInvalid` d'un membre `Signalized` d'un autre carrefour signalise ; commentaire XML deplace dans le coordinateur. Differes (`deferred-work.md`) : phase plus courte que le pas hote jamais publiee ; feu lu sur le seul premier mouvement d'une traversee multi-mouvements. Rejete : lignes `Signalized` non validees (decision proprietaire ci-dessus). `Story536` 13/13 EditMode, 0 erreur Console, `MVP_Run` propre.

## Design Notes

Le jaune n'a pas besoin d'une regle propre : « s'arreter si on le peut » est exactement la frontiere d'engagement 5.34. La surete ne depend ni de la duree du jaune ni d'un tout-rouge : un vehicule engage garde son grant, donc tout vert incompatible reste refuse (`ConflictGranted`) jusqu'a sa liberation. Le feu se lit sur le premier mouvement (controle d'approche, comme la preseance 5.35) ; le melange etant refuse, tout mouvement d'un carrefour signalise est `Signalized`.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.36 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode EditMode`, puis `5.34`, `5.32`, `5.25` -- expected: verts (non-regression D3/D4).
- `git status --short` -- expected: aucun fichier `App/Scenes/MVP_Run*` modifie.

## Suggested Review Order

**Regle de feu dans le coordinateur**

- Point d'entree : la frame du lot devient la seule source de l'etat des feux.
  [`JunctionCoordinator.cs:104`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L104)

- Seul Green autorise ; absence d'etat = refus, jamais un vert.
  [`JunctionCoordinator.cs:697`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L697)

- Nouvelle demande : feu apres la sortie, hors reservation d'ancien et hors briseur.
  [`JunctionCoordinator.cs:318`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L318)

- Grant non engage revoque au jaune ; un engage est conserve plus haut.
  [`JunctionCoordinator.cs:261`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L261)

- Deux raisons stables ajoutees en fin d'enum.
  [`JunctionRecords.cs:55`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs#L55)

**Horloge hote et branchement**

- Phase courante cumulee en double, bornee a un tour de cycle.
  [`SignalPhaseController.cs:43`](../../Assets/RoadRage/Features/Vehicles/Traffic/Signals/SignalPhaseController.cs#L43)

- Le runner possede une horloge par modele, comme le coordinateur.
  [`TrafficV2StepRunner.cs:120`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs#L120)

- La frame N porte la phase de l'horloge.
  [`TrafficV2StepRunner.cs:168`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs#L168)

- L'horloge avance d'un pas fixe apres le lot.
  [`TrafficV2StepRunner.cs:223`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs#L223)

**Validation du plan (D4)**

- Aucun plan factice ; membres limites aux mouvements Signalized du carrefour.
  [`RoadModelValidator.cs:1289`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs#L1289)

- Chaque phase enonce exactement un etat par groupe.
  [`RoadModelValidator.cs:1223`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs#L1223)

- Signalized ne se melange a aucun autre genre.
  [`RoadModelValidator.cs:1057`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs#L1057)

- Lignes de feu hors validation, par decision proprietaire (integration differee).
  [`RoadModelValidator.cs:1076`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs#L1076)

**Preuves**

- Jaune : revocation du non engage, conservation de l'engage, vert incompatible refuse.
  [`Story536SignalTests.cs:404`](../../Assets/RoadRage/Tests/EditMode/Story536SignalTests.cs#L404)

- Vert sans derogation aux invariants ; rouge sans reservation.
  [`Story536SignalTests.cs:451`](../../Assets/RoadRage/Tests/EditMode/Story536SignalTests.cs#L451)

- Cycle complet : transitions exactes, jamais deux verts incompatibles.
  [`Story536SignalTests.cs:308`](../../Assets/RoadRage/Tests/EditMode/Story536SignalTests.cs#L308)

- Refus D4, dont le membre d'un autre carrefour signalise.
  [`Story536SignalTests.cs:250`](../../Assets/RoadRage/Tests/EditMode/Story536SignalTests.cs#L250)

- Branchement du runner par ordre des appels.
  [`Story536SignalTests.cs:357`](../../Assets/RoadRage/Tests/EditMode/Story536SignalTests.cs#L357)

- Tests 5.25 adaptes : genre compare sans ligne ni plan.
  [`Story525RoadWorldModelTests.cs:752`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L752)
