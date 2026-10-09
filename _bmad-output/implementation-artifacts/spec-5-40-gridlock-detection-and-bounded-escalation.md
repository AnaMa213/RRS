---
title: 'Story 5.40 -- Detection d''interblocage multi-vehicules et escalade bornee'
type: 'feature'
created: '2026-10-08'
status: 'done'
baseline_commit: 'be1a6eee78d5d47bca397a8a44f1f162b2f3937a'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-39-recovery-supervisor-progress-and-physical-reattachment.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le briseur 5.35 ne couvre qu'un ensemble clos de `YieldToPriority` dans un carrefour sans titulaire ni occupant. Une attente cyclique qui traverse plusieurs carrefours, ou qui inclut un occupant, un leader ou une sortie bloquee, fige ses vehicules pour le reste du run.

**Approach:** Un `GridlockSupervisor` hote construit a chaque pas le graphe d'attente (blockers + records du coordinateur) et en extrait les cycles. Il les journalise en build de developpement et les soumet au `JunctionCoordinator`. Le coordinateur decide seul l'escalade, par paliers authores appliques dans l'ordre. Si aucun palier ne s'applique, le cycle est publie `Exhausted`, sans retrait.

## Boundaries & Constraints

**Always:**
- **G1, graphe.** Un arc A -> B existe quand un blocker de A nomme l'acteur B present dans la frame : `Leader`, `JunctionGrant` dont la cause est un acteur, `Obstacle`. Pour `BlockedExit`, l'arc va vers `Request.Exit.BoundId` quand `Bound == Occupant`. Un blocker n'existe que pendant un maintien, donc seul un vehicule tenu a des arcs. Aucun arc ne vient du temps ecoule.
- **G2, cycle.** Une composante fortement connexe de 2 membres ou plus (Tarjan) est un cycle. La detection est deterministe et independante de l'ordre des entrees. Elle est sans memoire : aucun minuteur ni seuil de duree. La sortie est triee par plus petit `TrafficId`.
- **G3, journal.** En build de developpement (`Debug.isDebugBuild`), un nouveau cycle est journalise une fois, avec ses vehicules et ses arcs. Sont aussi journalises une escalade (palier, vehicule) et le premier `Exhausted` d'un cycle. Un meme cycle n'est pas rejournalise a chaque pas.
- **G4, paliers.** `TrafficV2Settings.GridlockEscalationTiers` est la liste ordonnee authoree ; chaque palier liste est actif. Palier unique livre : `PrecedenceRelaxation`.
- **G5, escalade.** Elle s'evalue dans le lot du coordinateur, apres les regles normales et le briseur 5.35. Un cycle dont un membre a obtenu un grant neuf a ce lot n'est pas escalade. Sinon, chaque palier est essaye dans l'ordre, puis chaque membre par (anciennete, `TrafficId`). On emet au plus un grant par cycle et par lot, raison `GrantedGridlockEscalation`.
- **G6, `PrecedenceRelaxation`.** Le palier ignore seulement `YieldToPriority` et `SeniorRequestPending` dont la cause est un membre du cycle. Tout le reste tient :
  - demande valide et connue ;
  - sortie suffisante (garder-la-libre) ;
  - feu vert ;
  - arret marque devant un `Stop` ;
  - aucun conflit avec un occupant ;
  - aucun conflit avec un grant vivant, sur toute zone, `Merge` compris ;
  - priorite et reservation d'un non-membre.
- **G7, issue.** L'instantane publie une resolution par cycle soumis : membres, palier applique, vehicule servi, ou `Exhausted`. Un lot sur frame invalide n'escalade rien.

**Ask First:**
- Fixture Story534/535/536/539 rouge : HALT avant d'adapter une assertion.
- Tout palier supplementaire, tout seuil, toute modification de `MVP_Run`, du prefab V2 ou de Gate A : HALT.

**Never:**
- Aucun palier ne retire, teleporte, reinsere, despawn ni deplace un vehicule. Le superviseur ne decide aucun grant, ne compose aucun intent et ne lit ni ne modifie aucune regle.
- Aucun grant en conflit avec un grant vivant ou un occupant. Aucun grant cyclique, c'est-a-dire un grant dont le titulaire s'arreterait dans le carrefour, sortie insuffisante, en attendant un membre.
- On ne relache pas `keep-clear` ni un feu, on ne contourne pas (5.42), on ne cree pas d'exception de politique (5.41), on ne modifie pas le briseur 5.35.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Cycle a 3 sur 2 carrefours | A leader B, B `JunctionGrant` cause C, C `BlockedExit` occupant A | 1 cycle {A,B,C}, journalise une fois | N/A |
| Chaine ouverte, attente longue | A->B->C, C roule, `SinceFrame` ancien | Aucun cycle | N/A |
| Cycle avec occupant | Cycle de priorite + occupant membre, compatible avec A | Briseur 5.35 inactif ; un `GrantedGridlockEscalation` a A | N/A |
| Aucun membre admissible | Sortie de chaque membre insuffisante | Aucun grant ; `Exhausted` journalise | Vehicules presents |
| Menace exterieure | A cede a un non-membre qui arrive | A non servi | N/A |
| `Stop` non marque | Membre derriere un `Stop` non marque | Non servi par l'escalade | N/A |
| Progres normal | Un membre obtient un grant normal au lot | Aucune escalade | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs` -- `Resolve` `:104` (ajouter `cycles`, null = comportement identique), `ResolveBatch` `:112`, appel du briseur `:363` (escalade juste apres), `BreakDeadlocks` `:619` (patron : anciennete puis `TrafficId`, `ignoredCauses`), predicats reutilises `ExitSufficient` `:438`, `ConflictsWithOccupants` `:458`, `ConflictsWithGrants` `:473` (strict, toutes zones), `ConflictsWithSeniors` `:497` (variante ignorant les membres), `YieldsToPriority(..., ignoredCauses)` `:591`, `SignalReason` `:693`, `StopMarkedOf` `:716`, `Publish` `:821`. `ResolveUnavailableFrame` `:382` inchange.
- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs` -- `JunctionReason` `:20` (ajouter `GrantedGridlockEscalation = 24` en fin, et a `IsNewGrant` du coordinateur `:722`) ; y placer `GridlockCycle`, `GridlockEscalationTier`, `GridlockResolution` (contrats de coordination) ; `JunctionSnapshot` `:507` expose `Gridlocks` ; `JunctionExitAssessment.BoundId` `:159`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Recovery/` -- nouveau `GridlockSupervisor.cs` : `WaitsOf` (G1, pur), `Detect` (Tarjan), `Report` (G3, puits `Action<string>` injectable, `Debug.Log` par defaut sous `Debug.isDebugBuild`).
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs` -- possede le superviseur ; ordre : boucle de conduite `:185-191`, puis `Detect`, puis `Coordinate` `:198` avec les cycles, puis `Report` de l'instantane.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs:472,476` -- `Blockers`, `LastJunctionReport` (lecture seule) ; `JunctionCause` `:1037` montre le sens de `BlockingActorOrRule`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Blockers/Blocker.cs` -- genres, `BlockingActorOrRule` = `RoadId.ToString()` de l'acteur.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs:144` -- `TrafficV2Settings` : `GridlockEscalationTiers`.
- Patrons de test : `Tests/EditMode/Story535JunctionRulesTests.cs` -- `CrossingSource` `:189` (modele en memoire, zones authorables), `Requesting` `:102`, `Inside` `:121`, `AssertGrantInvariant` `:158`, permutations d'ordre `:245`.

## Tasks & Acceptance

**Execution:**
- [x] `JunctionRecords.cs` -- raison, types de cycle/palier/resolution, `JunctionSnapshot.Gridlocks` (vide par defaut).
- [x] `TrafficV2Composition.cs` -- `GridlockEscalationTiers = { PrecedenceRelaxation }`.
- [x] `JunctionCoordinator.cs` -- parametre `cycles`, `EscalateGridlocks` (G5-G7) apres `BreakDeadlocks`.
- [x] `Recovery/GridlockSupervisor.cs` (nouveau) -- G1-G3.
- [x] `TrafficV2StepRunner.cs` -- cablage dans l'ordre de la Code Map.
- [x] `Assets/RoadRage/Tests/EditMode/Story540GridlockTests.cs` `[Core][Story540]` -- couvre :
  - la matrice ;
  - le determinisme par permutation des entrees ;
  - deux cycles disjoints ;
  - `LogAssert` : un journal par cycle, aucun a la repetition ;
  - l'invariant de grants et la sortie suffisante de chaque grant d'escalade ;
  - `cycles` nul ou vide : instantane identique au lot sans escalade ;
  - les scans structurels : aucun appel de retrait, deplacement ou intent dans `GridlockSupervisor` ni dans l'escalade, et aucune valeur de palier de retrait ;
  - l'ordre des appels dans le runner.

**Acceptance Criteria:**
- Given trois vehicules ou plus en attente cyclique, when la detection tourne, then le cycle est identifie depuis le graphe des blockers et des grants, et journalise en build de developpement avec ses vehicules.
- Given un interblocage detecte, when l'escalade s'applique, then les paliers authores s'appliquent dans l'ordre. Aucun grant n'entre en conflit avec un grant vivant, aucun grant n'est cyclique, et aucun palier ne retire ni ne deplace un vehicule.
- Given une relaxation de regle demandee, when elle est evaluee, then le coordinateur la decide ; le superviseur ne fait que la soumettre.
- Given aucun cycle, when les campagnes PlayMode 5.35 tournent, then elles restent vertes et `MVP_Run` est inchange.

### Review Findings — 2026-10-08 — Independent requested review

Four configured review layers completed on `be1a6ee..09c8c59`, inspected at `b1db1de`. The owner's option 1 authorized both medium-severity verification patches; both are applied and verified. Story540 passes 21/21 EditMode, compilation is healthy, Console has zero errors in the validation window, and MVP_Run is clean. Existing approved Gate D deferrals were not reopened. [Detailed findings and validation evidence](code-review-5-40-2026-10-08.md).

- [x] [Review][Patch][Medium] R1 — Actual Red and Yellow phases now reach the coordinateur in a TrafficFrame with a submitted cycle and require SignalStop, Exhausted and no grant. A valid single-Green control proves normal progress, and the missing-state case is retained. G6. `Assets/RoadRage/Tests/EditMode/Story540GridlockTests.cs:254`.
- [x] [Review][Patch][Medium] R2 — Merge fixtures now exercise a regular live holder and a holder originally admitted through GrantedMergeGap. The ordinary gap is proved, but a member's senior reservation prevents normal progress; escalation must remain Exhausted until the external holder disappears. The invariant excludes escalation from the ordinary MergeGap exception. The multi-movement distance helper includes intermediate corridors, following Story535. G6 and AC2. `Assets/RoadRage/Tests/EditMode/Story540GridlockTests.cs:291`.

## Spec Change Log

- **2026-10-08 — Owner-authorized independent review patches (option 1).** Both findings are fixed in Story540 fixtures only; no runtime code changed. Five new parameterized cases (Red, Yellow, Green, regular Merge holder, MergeGap holder) bring Story540 to 21/21 EditMode. Final validate.ps1 window: cursor 1003, compilation healthy, zero Console errors, no skipped/inconclusive tests, MVP_Run clean. Initial fixture failures and the interrupted CLI recompilation are recorded in the review report. Graphify updated: 4,848 nodes / 11,211 edges / 183 communities. Story535 PlayMode passed 7/7 during the independent review; it was not repeated after these test-only patches. Spec is done; sprint tracking returns to review under the existing checkpoint promotion rule.

- **2026-10-08 -- Revue.**
  - **Couches actives.** `blind-hunter` (en ligne), `edge-case-hunter` et `verification-gap`. `security-review` est inactive : aucune frontiere reseau n'est deplacee.
  - **Tri.** La revue ne releve ni `intent_gap` ni `bad_spec`, donc aucune boucle.
  - **Correctifs appliques (patch) :**
    - le compteur `YieldToPriority` du lot est decremente quand l'escalade remplace un refus de ce genre (patron du briseur 5.35) ;
    - sans frame ou sans acteur, la detection est videe : plus de cycles perimes ni de journal supprime a tort ;
    - aucun vehicule tenu donne un chemin sans allocation, et le coordinateur ne cree aucune liste sans cycle soumis (cout par pas) ;
    - quatre tests ajoutes :
      - un feu rouge ou absent n'est jamais releve (`SignalUnavailable`, `Exhausted`) ;
      - un grant vivant non membre interdit seul l'escalade ;
      - un occupant sans grant interdit seul l'escalade ;
      - un cycle epuise qui disparait puis revient est rejournalise.
  - **Differe (`deferred-work.md`).** Le cablage du runner n'est prouve que par scan de source. Aucun pilote V2 n'est liable en EditMode ; la Gate D reste l'observation candidate prevue par l'epic.
  - **Rejetes :**
    - escalades successives sur des lots consecutifs : chacune est sure, et G5 borne a un grant par cycle et par lot ;
    - cycles soumis en double ou chevauchants : Tarjan rend des composantes disjointes ;
    - arcs vers les reservations de sortie : G1 approuve, une sous-detection reste conservatrice ;
    - membres nuls, arcs dupliques, resolution par defaut : entrees non produites par le code ;
    - recursion de Tarjan : la profondeur est bornee par la population (environ 30).
  - **Validation finale brute.** 0 erreur Console sur chaque fenetre, compilation saine, `MVP_Run` propre.

    | Story | EditMode | PlayMode |
    |---|---|---|
    | 5.40 | 16/16 | -- |
    | 5.39 | 22/22 | 4/4 |
    | 5.36 | 14/14 | -- |
    | 5.35 | 51/51 | 7/7 |
    | 5.34 | 48/48 | -- |

    Les PlayMode 5.35 et 5.39 ont ete relancees apres les correctifs. Graphify mis a jour : 4 848 noeuds, 11 211 liens, 183 communautes.

## Design Notes

Decisions proposees a l'approbation :
- **D1, palier unique.** Les autres paliers du registre SUMO sont exclus.
  - Ignorer garder-la-libre : le titulaire s'arreterait dans le carrefour (equilibre IDM a s0), ce qui fait un grant cyclique.
  - Ignorer un bloqueur de carrefour : conflit avec un grant vivant.
  - Contourner : 5.42.
  - Teleporter : interdit.
  - La liste reste le point d'extension de 5.41 et 5.42.
- **D2, pire cas borne.** Un cycle purement physique (sorties et leaders sans arc de preseance) n'a pas de resolution qui preserve les invariants. Il reste `Exhausted`, journalise et reevalue a chaque lot. On ne le retire pas : le nettoyage catastrophique reste differe.
- **D3, sans confirmation temporelle.** Le coordinateur reverifie chaque condition au lot, donc un cycle perime d'un pas ne produit rien.
- **Accord proprietaire pendant l'implementation (2026-10-08, option 1).** `Story536SignalTests.TheRunnerOwnsTheClockAndAdvancesItOncePerHostStep` cherchait le texte `coordinator.Resolve(FrameId, reports, frame)`. Son motif devient `coordinator.Resolve(FrameId, reports, frame, cycles)`, intention inchangee, et la methode est etiquetee `Story540`.
- **D4, placement.** Le superviseur est dans `Recovery/`, qui peut demander une escalade (BC-9). La decision est dans `Coordination` (BC-7, AD-40).

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.40 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = compte attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode Both`, puis `-Story 5.34` et `-Story 5.36` en EditMode -- expected: verts (coordinateur inchange sans cycle).
- `.\scripts\validate.ps1 -Profile Story -Story 5.39 -TestMode PlayMode` -- expected: vert (runner).
- `git status --short` -- expected: aucun fichier `MVP_Run` ni prefab modifie.

## Suggested Review Order

**Decision d'escalade (coordinateur, seule autorite)**

- Point d'entree : cycles soumis, progres normal, paliers dans l'ordre, au plus un grant.
  [`JunctionCoordinator.cs:678`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L678)

- G6 : seules la preseance et l'anciennete des membres sont ignorees ; le reste tient.
  [`JunctionCoordinator.cs:746`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L746)

- Branchement dans le lot, apres le briseur 5.35, avant la publication.
  [`JunctionCoordinator.cs:368`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L368)

- Refus remplace : compteur YieldToPriority corrige, records de refus retires.
  [`JunctionCoordinator.cs:727`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L727)

- Reservation d'ancien filtree par les membres du cycle.
  [`JunctionCoordinator.cs:510`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L510)

**Detection (superviseur, soumet sans decider)**

- G1 : arcs vers des acteurs presents ; sortie bloquee vers son occupant.
  [`GridlockSupervisor.cs:39`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/GridlockSupervisor.cs#L39)

- G2 : Tarjan deterministe, composantes de 2 membres ou plus, aucun minuteur.
  [`GridlockSupervisor.cs:62`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/GridlockSupervisor.cs#L62)

- G3 : journal d'un nouveau cycle ; chemin sans allocation hors attente.
  [`GridlockSupervisor.cs:99`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/GridlockSupervisor.cs#L99)

- Escalade journalisee, Exhausted une fois par cycle.
  [`GridlockSupervisor.cs:122`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/GridlockSupervisor.cs#L122)

**Cablage hote**

- Apres les pas de conduite, cycles de N soumis au lot de N.
  [`TrafficV2StepRunner.cs:207`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs#L207)

- Arcs construits seulement si un vehicule est tenu ; frame refusee : aucun cycle.
  [`TrafficV2StepRunner.cs:215`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs#L215)

**Contrats et donnees authorees**

- Paliers authores, ordonnes, tous actifs ; aucun palier de retrait.
  [`TrafficV2Composition.cs:169`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs#L169)

- Palier, cycle, resolution et instantane publie.
  [`JunctionRecords.cs:588`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs#L588)

- Nouvelle raison de grant, en fin d'enum.
  [`JunctionRecords.cs:59`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs#L59)

**Preuves**

- Cycle tenu par un occupant : un seul grant, au seul membre admissible.
  [`Story540GridlockTests.cs:160`](../../Assets/RoadRage/Tests/EditMode/Story540GridlockTests.cs#L160)

- Cycle a trois sur deux carrefours, depuis les arcs reels.
  [`Story540GridlockTests.cs:64`](../../Assets/RoadRage/Tests/EditMode/Story540GridlockTests.cs#L64)

- Gardes isolees : feu, grant vivant seul, occupant seul.
  [`Story540GridlockTests.cs:229`](../../Assets/RoadRage/Tests/EditMode/Story540GridlockTests.cs#L229)

- Exhausted, aucun retrait, rejournal apres retour.
  [`Story540GridlockTests.cs:195`](../../Assets/RoadRage/Tests/EditMode/Story540GridlockTests.cs#L195)

- Scans structurels : aucun retrait, deplacement ou intent ; une seule emission.
  [`Story540GridlockTests.cs:331`](../../Assets/RoadRage/Tests/EditMode/Story540GridlockTests.cs#L331)

- Test 5.36 adapte (option 1) : le lot recoit aussi les cycles.
  [`Story536SignalTests.cs:378`](../../Assets/RoadRage/Tests/EditMode/Story536SignalTests.cs#L378)
