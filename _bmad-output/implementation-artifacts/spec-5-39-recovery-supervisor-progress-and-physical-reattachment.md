---
title: 'Story 5.39 -- Superviseur de recuperation : progression attendue contre reelle, rattachement physique'
type: 'feature'
created: '2026-10-07'
status: 'done'
baseline_commit: 'cd46557d0e4ad9ef288a86c8fb2ad21435c5d1b3'
review_loop_iteration: 1
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-38-collision-response-physics-yields-tactical-handshake.md'
  - '{project-root}/_bmad-output/implementation-artifacts/anomalies/epic 5/ANO-5.10-03/ANO-5.10-03.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Un vehicule V2 stable mais deplace reste au frein pour toujours : `AwaitingRecovery` (5.38) ou verrou 2a (5.52). Un vehicule encastre ou cale sans cause legitime n'est jamais detecte (`ANO-5.10-03` AC5-AC7, deferred « StopHold a jeu negatif »).

**Approach:** Un `RecoverySupervisor` par vehicule detecte qu'une progression attendue n'a pas eu lieu et soumet une requete versionnee de manoeuvre. `TacticalDecision` l'accepte ou la refuse avec une raison. Elle possede la manoeuvre (realignement en marche avant, recul controle) jusqu'a sa fin. Si aucun plan ne preserve les invariants, le vehicule passe en `Faulted`, arret sur et diagnostique.

## Boundaries & Constraints

**Always:**
- **R1, causes.** Il y a trois causes d'eligibilite :
  - `Displaced` : but de collision en `AwaitingRecovery`.
  - `ToleranceLatched` : verrou 2a pose et repli terminal `Held`.
  - `ProgressDeficit` : voir R2.
- **R2, progression attendue.** Un pas attend une progression si quatre conditions tiennent :
  - le pas est nominal et commande (arbitrage present, compose hors repli, aucun but) ;
  - aucun blocker n'est legitime ;
  - la liante est `Profile` ou `DesiredSpeed`, ou bien un maintien ou un suivi dont la source est un blocker illegitime ;
  - a_route > 0, a_route etant le minimum des candidats `Profile` et `DesiredSpeed` non remplaces.

  L'episode suit deux grandeurs. E = ∫v_e, avec v_e(0) = max(0, v) et dv_e = a_route·dt, borne a [0, v_desired]. A est l'avance signee sur la reference. Toute condition fausse, un changement de reference ou A ≥ 0,5 m remet l'episode a zero. La cause est eligible si E ≥ 2 m et A < 0,5 m. Une vitesse basse seule ne suffit jamais.
- **R3, poignee de main.** Le superviseur rend au plus une requete par pas : `Version` croissante, `SourceFrameId`, cause, manoeuvre et numero de tentative. Il ne pose jamais le but. `SubmitRecovery` rend `Accepted` ou l'un de ces refus : `InvalidRequest`, `StaleRequest`, `GoalAlreadyActive`, `Unstable` (blocker C6), `NoReference` ou `RearBlocked`. Accepter depuis `AwaitingRecovery` transfere le but.
- **R4, escalade.** Les causes `Displaced` et `ToleranceLatched` commencent par `Realign`. `ProgressDeficit` commence par `Reverse`. On alterne apres chaque echec ou refus. L'historique garde chaque tentative avec sa version, sa manoeuvre, sa raison et ses frames. Une meme requete n'est jamais resoumise. Deux refus consecutifs, ou 4 tentatives dans un episode, donnent `Faulted`.
- **R5, manoeuvres.** Les deux manoeuvres passent par le `SafetyFilter` puis par le composeur.
  - `Realign` : marche avant, a = clamp((v_rec − v)/dt, −b confort, a_max) avec v_rec = 2 m/s. L'angle suit la loi de `MotionCommand.Track`, vers la pose nominale de la reference projetee.
    - Fin `Resumed` : d ≤ ε_t et stabilite C6 tenue pendant 0,5 s.
    - Echec `NoProgress` : 20 m parcourus sans `Resumed`.
  - `Reverse` : roues droites, consigne −1 m/s.
    - Fin `ManeuverCompleted` : 3 m parcourus.
    - Le composeur passe par `BrakeReverse` sous v_dir, jamais par le gaz.
    - `RearBlocked` si un acteur de la frame est dans le balayage arriere.
  - Les deux manoeuvres echouent en `Stalled` (meme registre R2 sur le deplacement plan) et sont annulees par une collision acceptee ou par la sortie.
- **R6, branchement.** `preserveRoute`, l'arbitrage saute, la demande de carrefour invalide et le verrou suspendu valent pour tout but actif. Un `Realign` accepte depuis le verrou 2a relache ce verrou. L'etat de recuperation est publie dans la projection.
- **R7, `Faulted`.** Le vehicule recoit le repli `Faulted` a chaque pas, sans planification. Il reste un acteur de la frame et un occupant de carrefour. Rien ne le retire ni ne le deplace.

**Ask First:**
- Fixture Story531/533/535/537/538 rouge : HALT avant d'adapter une assertion ou un seuil.
- Tout seuil R2/R4/R5 ; toute modification de `MVP_Run`, du prefab V2 ou de Gate A : HALT.

**Never:**
- Ecriture de position, rotation ou vitesse, teleportation, reinsertion, despawn en route, `RecoverAtWaypoint`, minuteur de blocage, orbite de waypoint, memoire de route auto-evitante.
- Le superviseur ne compose aucun intent, n'accorde aucun grant et ne lit ni ne modifie aucune regle de trafic ni l'etat d'un carrefour.
- Contournement local (5.42), interblocage multi-vehicules (5.40), nettoyage catastrophique.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Deplace par un choc | `AwaitingRecovery`, d = 2 m, stable | `Realign` accepte, `Resumed` a d ≤ ε_t | N/A |
| Attente legitime | Leader legitime, ou grant refuse, 100 s | Jamais eligible | N/A |
| Encastrement | `StopHold` sur leader a jeu < s0/2 | `ProgressDeficit`, `Reverse` | N/A |
| Cale sans blocker | a_route > 0, v = 0 | Eligible quand E ≥ 2 m | N/A |
| Virage lent | Liante `Profile`, roule | A ≥ 0,5 m : jamais eligible | N/A |
| Recul obstrue | Acteur derriere | `RearBlocked`, puis `Realign` | Historique |
| Manoeuvres epuisees | 4 tentatives ou 2 refus | `Faulted`, repli tenu | Vehicule present |
| Choc pendant la manoeuvre | Collision significative | But de collision accepte, tentative `CollisionPreempted` | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- `PrepareStep` `:608` (latch `:662-665`) ; `Step` `:701` : soumission collision `:735-747`, `preserveRoute` `:744`, garde planification `:751`, arbitrage `:852`, commande du but `:867`, SafetyFilter `:896`, verrou `:903`, `Compose(propulsion)` `:911`, demande carrefour `:930`, blockers `:933`, projection `:941-947`. `DriveOutcome` `:1071`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs` -- `Submit` `:143`, `Update` `:174`, `CommandFor` `:199`, raisons en fin d'enum `:87`, `AwaitingRecovery` `:84`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs` -- `CollisionPredicates.Stability` `:102`, `SuspendsToleranceLatch` `:114`, `CollisionThresholds.StableSeconds`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Blockers/Blocker.cs` -- `Legitimate`, `Recoverable`, table des genres `:99-105`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs:16,283,370` -- genres de candidats, `Superseded`, `Binding`, `Candidates`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs:110-136` -- loi d'angle a extraire en fonction pure (`Track` doit rester identique au bit pres).
- `Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs:8,129,208,265` -- `V2FallbackReason` (ajouter `Faulted = 13`), parametre `reverse` (faux par defaut, sinon comportement identique), capacite de recul par `ReverseTorque`.
- `Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs:215` -- `BrakeReverse` n'engage la marche arriere que sous v_dir.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrackingToleranceResponse.cs` -- ajouter `Release()`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrackingMeasurement.cs:79-103,203,237,314` -- `Pieces[i].Curve`, `ElementS`, `NominalHeadingErrorDegrees`, `Project` (reference du realignement).
- `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs:36,91` -- `Actors` avec empreinte (balayage arriere).
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs:43,144` -- `TrafficV2Settings` (constantes R2/R4/R5), ε_t = 0,34 m.
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:186` -- ajouter le texte `recovery` sur le patron de `tactical`.
- Patrons de test : `Tests/EditMode/Story538CollisionResponseTests.cs` (composeur reel, scans structurels), `Tests/PlayMode/Story538DriverCollisionPlayModeTests.cs` (driver reel dans `MVP_Run`, `Story533Harness`, injection a la frontiere Prepare/Step).

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs` (nouveau) -- `ProgressLedger` (R2, pur, reutilise par la tactique), `RecoveryCause`, `RecoveryManeuver`, `RecoveryRequest`, `RecoveryAttempt`, `RecoverySupervisor` (`Observe` en fin de pas, `TryRequest` au pas suivant, `Record`, escalade R4, `Faulted`, `ToText`).
- [x] `TacticalDecision.cs` -- `TacticalGoalKind.Recovery`, raisons ajoutees en fin d'enum, `SubmitRecovery`, mise a jour, commandes R5 et preemption par collision.
- [x] `MotionCommand.cs`, `VehicleDriveIntentComposer.cs`, `TrackingToleranceResponse.cs`, `TrafficV2Composition.cs`, `TrafficDecisionProjection.cs` -- voir la Code Map.
- [x] `TrafficV2VehicleDriver.cs` -- R6 et R7 : collision, puis recuperation, puis `Update`. Etat `Lifecycle`/`Fault` publie.
- [x] `Assets/RoadRage/Tests/EditMode/Story539RecoveryTests.cs` `[Core][Story539]` -- couvre :
  - la matrice ;
  - la suppression par chaque genre legitime, et un ensemble de plusieurs blockers conserve ;
  - chaque refus ;
  - l'escalade et `Faulted` ;
  - les fins `Resumed`, `Stalled`, `NoProgress` et `ManeuverCompleted` ;
  - les commandes au vrai composeur : le recul n'utilise jamais le gaz, et le parametre par defaut reste identique ;
  - les scans structurels de `Recovery/` (Never) ;
  - l'ordre des appels dans le driver.
- [x] `Assets/RoadRage/Tests/PlayMode/Story539RecoveryPlayModeTests.cs` `[Story539]` -- dans `MVP_Run`, un vehicule V2 reel est pousse lateralement par une impulsion physique ; le fait de contact du pas est injecte (extraction Gate D). Il doit :
  - passer en `AwaitingRecovery` ;
  - reussir un `Realign` jusqu'a `Resumed` ;
  - n'avoir a aucun pas un saut de position superieur a |v|·dt + 1 cm ;
  - atteindre son portail de sortie.
- [x] `_bmad-output/implementation-artifacts/recipe-5-39-reattachment-gate-d.md` -- recette Gate D : vehicule sorti de la chaussee par un choc reel, rattachement sans teleportation, sortie de route non empechee.

**Acceptance Criteria:**
- Given un plan et un grant qui predisent une progression, when elle n'a pas lieu sans blocker legitime, then la recuperation devient eligible. Un blocker legitime la supprime, et l'ensemble de blockers est conserve.
- Given un vehicule deplace hors de la chaussee par un choc, when la recuperation se termine, then il rejoint sa reference sans teleportation et reprend une route valide jusqu'a sa sortie.
- Given aucune manoeuvre ne preserve les invariants, when l'escalade s'acheve, then le vehicule est `Faulted`, arrete et diagnostique. Il n'est ni retire ni deplace.
- Given aucune collision ni aucun deplacement, when les campagnes PlayMode 5.33 et 5.35 tournent, then elles restent vertes et `MVP_Run` est inchange.

### Review Findings — 2026-10-07 — Independent requested review

Four configured review layers completed on `cd46557..a52ca8c`, inspected at `9cd2fdf`. The owner's option 1 authorized all four medium-severity patches on 2026-10-08; they are now applied and verified. Story539 passes 22/22 EditMode and 4/4 PlayMode, with healthy compilation, zero Console errors and clean `MVP_Run`. Existing approved deferrals were not reopened. [Detailed findings, dispositions, reproductions and validation evidence](code-review-5-39-2026-10-07.md).

- [x] [Review][Patch][Medium] R1 — Rear sweep now compares the rear rectangle with conservative bounds of all four actor footprint corners projected into its frame. Rotated and extended footprints block even when their reference point is ahead; disjoint actors remain clear. Reproduced before the patch and verified by the new EditMode geometry cases. `Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs:329`.
- [x] [Review][Patch][Medium] R2 — Exhaustion is checked before the no-cause return in the next request phase, so the Faulted fallback is applied on that same step. Legitimate waits cannot hide a last failed or rejected attempt; last-attempt `Resumed` and exit success are preserved. Regression cases cover `Stalled`, `NoProgress`, a nonconsecutive fourth rejection and a successful fourth realignment. `Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs:232`.
- [x] [Review][Patch][Medium] R3 — Faulted blocks both new exit detection and the driver's portal-completion property, including a previously acquired exit flag. The live MVP_Run fixture injects an actually localized exit pose and exercises the production spawner's removal consumer; the Faulted vehicle remains spawned and listed. `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs:496`.
- [x] [Review][Patch][Medium] R4 — Existing forward-speed reverse cases now require positive brake input and positive service torque through the actual tire model, with coast torque disabled for that assertion. No composer behavior changed. `Assets/RoadRage/Tests/EditMode/Story539RecoveryTests.cs:451`.

## Spec Change Log

- **2026-10-08 — Cloture demandee par le proprietaire.** Apres la correction et la validation des quatre findings, synchronisation de la 5.39 en `done` dans `sprint-status.yaml` ; la spec etait deja `done`. La validation finale reste 22/22 EditMode et 4/4 PlayMode, compilation saine et zero erreur Console. Cette cloture de story ne remplace pas les suites completes de fin d'epic ni les recettes et limites deja assignees a la Gate D.

- **2026-10-08 — Owner-authorized review patches (option 1).** All four independent review findings fixed. New fixtures reproduced four EditMode failures and the Faulted portal PlayMode failure before source corrections; a successful fourth realignment remained green. Final `validate.ps1 -Profile Story -Story 5.39 -TestMode Both`: 22/22 EditMode, 4/4 PlayMode, no skipped/inconclusive tests, healthy compilation, zero Console errors since cursor 101, `MVP_Run` clean. Initial infrastructure interruption and failing reproduction output are retained in the review report. Graphify updated: 4,803 nodes / 11,071 edges / 197 communities. Thresholds, scene/prefab authoring and Gate A unchanged. Sprint entry remains `review` under the checkpoint-only promotion rule; full suites and actual contact/calibration recipes remain assigned to their original gates.

- **2026-10-07 -- Revue.** Couches actives : blind-hunter (en ligne), edge-case-hunter et verification-gap. security-review est inactive : aucune frontiere reseau n'est deplacee. La revue ne releve ni intent_gap ni bad_spec. Correctifs autorises par le proprietaire :
  1. Le verrou 2a est relache par toute manoeuvre acceptee, et plus seulement par un `Realign` (extension de D3). Sans cela, apres un realignement echoue, le verrou se reposait et le recul accepte ensuite recevait une commande nulle.
  2. Une preuve au niveau du pilote (`Story539RecoveryPlayModeTests`, pas injectes, patron 5.38) couvre :
     - une attente legitime de 60 s derriere un leader a s0, sans aucune requete ;
     - un vehicule cale qui devient `ProgressDeficit` et recule par le pilote (gaz nul, `BrakeReverse`, `ManeuverCompleted`) ;
     - les tentatives epuisees qui donnent `Faulted` : repli `Faulted` a chaque pas, sans planification, sans requete meme sous un choc, vehicule present.
  - **Differes** (`deferred-work.md`) : balayage arriere verifie seulement a l'acceptation ; realignement pres d'un carrefour sans grant (R6).
  - **Rejetes :**
    - entrees deja validees par les profils (acceleration, couple de recul, deceleration confort) ;
    - « deux refus donnent Faulted » et realignement sans mouvement dans l'enveloppe, tous deux conformes a R4/R5 ;
    - balayage aveugle aux dangers non acteurs, limite D5 approuvee ;
    - parcours 3D non signe, effet negligeable ;
    - equivalence tautologique de la loi d'angle : `Track` reste couvert par `Story531` 50/50.
  - **Validation brute finale :**
    - Story539 : 17/17 EditMode et 4/4 PlayMode ;
    - avant les correctifs de revue : Story538 16/16 EditMode et 1/1 PlayMode, Story537 22/22, Story531 50/50, Story552 4/4, Story535 7/7, Story533 12/12 ;
    - 0 erreur Console sur chaque fenetre, compilation saine, `MVP_Run` propre.

## Design Notes

Decisions proposees a l'approbation :
- D1 : seuils declares R2/R4/R5, calibres par la recette Gate D.
- D2 : une manoeuvre de recuperation roule hors de la couverture Gate A, a vitesse bornee, sous le `SafetyFilter`, sans revendication de preuve. La 5.52 a renvoye explicitement a la 5.39 la recuperation apres sortie de couverture.
- D3 : un `Realign` accepte relache le verrou 2a.
- D4 : pas de requete de replanification dediee. Le planificateur replanifie au premier pas nominal (`RouteOutcome.Replanned`), qui est compte et publie.
- D5 : le recul se fait roues droites. Le balayage arriere ne voit que les acteurs de la frame. Limite connue : un obstacle statique arriere reste invisible ; un contact a 1 m/s reste sous le seuil de choc.
- D6 : `Faulted` est definitif jusqu'a la politique de nettoyage catastrophique, toujours differee.

Accords proprietaires pendant l'implementation (2026-10-07), tests d'autres stories adaptes a R6/D3 et etiquetes `Story539` au niveau de la methode :
- `Story538CollisionResponseTests.TheDriverRunsAnalysisSubmitUpdateCommandSafetyThenCompose` : `collisionGoal` devient `tacticalGoal` pour l'arbitrage saute et la demande de carrefour invalide (R6).
- `Story538DriverCollisionPlayModeTests` : gaz nul exige seulement pendant le but de collision ; `AwaitingRecovery` atteint puis transfert a `Recovery`/`Realign`, route et reference figees pendant les deux buts. La pose injectee etant figee, le banc injecte aussi le parcours du pas (2 m/s x dt) pendant le realignement, a la meme frontiere Prepare/Step ; sans lui, R5 le declare a juste titre `Stalled`.
- `Story552MilestonePlayModeTests` (renomme `AnActualToleranceExceedanceLatchesUntilHeldThenRecoveryRejoinsAndExits`) : verrou 2a reel jusqu'a `Held`, puis `Realign` accepte (cause `ToleranceLatched`), verrou relache, aucune teleportation, sortie normale au portail (D3).

Pendant un but, l'arbitrage est saute : le blocker set est donc vide pour les causes `Displaced` et `ToleranceLatched`. Le deplacement est la cause, et le `SafetyFilter` protege la manoeuvre.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.39 -TestMode Both` -- expected: `VALIDATION STORY`, compte execute = compte attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.38 -TestMode Both`, puis `-Story 5.37` et `-Story 5.31` en EditMode -- expected: verts (tactique, composeur).
- `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode PlayMode`, puis `-Story 5.33` -- expected: verts (non-regression nominale).
- `git status --short` -- expected: aucun fichier `MVP_Run` ni prefab modifie.

## Suggested Review Order

**Detection et escalade (superviseur)**

- Point d'entree : cause du pas, fin d'une manoeuvre, fin d'episode sur progression.
  [`RecoverySupervisor.cs:191`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs#L191)

- R2 : progression attendue par le plan, l'ensemble entier des blockers est lu.
  [`RecoverySupervisor.cs:287`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs#L287)

- Registre E contre A : rien ne s'accumule hors attente, ce n'est pas un minuteur.
  [`RecoverySupervisor.cs:51`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs#L51)

- R4 : premiere manoeuvre selon la cause, alternance, budget, puis Faulted.
  [`RecoverySupervisor.cs:230`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs#L230)

- Un refus met a jour l'historique ; deux refus consecutifs donnent Faulted.
  [`RecoverySupervisor.cs:245`](../../Assets/RoadRage/Features/Vehicles/Traffic/Recovery/RecoverySupervisor.cs#L245)

**Poignee de main et manoeuvres (tactique)**

- R3 : acceptation ou refus motive ; transfert depuis AwaitingRecovery.
  [`TacticalDecision.cs:251`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L251)

- R5 : Resumed dans epsilon_t, NoProgress, ManeuverCompleted ou Stalled.
  [`TacticalDecision.cs:379`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L379)

- Commandes : realignement vers 2 m/s, recul roues droites vers -1 m/s.
  [`TacticalDecision.cs:322`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L322)

- Un choc significatif annule la manoeuvre : la physique d'abord.
  [`TacticalDecision.cs:226`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L226)

**Branchement du pilote**

- La recuperation soumet apres la collision, jamais au pas d'un choc accepte.
  [`TrafficV2VehicleDriver.cs:775`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L775)

- D3 etendu : toute manoeuvre acceptee relache le verrou 2a.
  [`TrafficV2VehicleDriver.cs:786`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L786)

- Le realignement reutilise la loi de suivi vers la pose nominale projetee.
  [`TrafficV2VehicleDriver.cs:927`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L927)

- R7 : Faulted, avec repli tenu, sans planification ni deplacement.
  [`TrafficV2VehicleDriver.cs:971`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L971)

- Le superviseur lit les faits du pas compose, sa requete part au pas suivant.
  [`TrafficV2VehicleDriver.cs:1005`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L1005)

- Parcours plan du pas, mesure de la manoeuvre.
  [`TrafficV2VehicleDriver.cs:679`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L679)

**Frontieres partagees**

- Recul : BrakeReverse sous v_dir, frein au-dessus, jamais de gaz ; defaut inchange.
  [`VehicleDriveIntentComposer.cs:240`](../../Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs#L240)

- La loi d'angle extraite de Track, arithmetique identique.
  [`MotionCommand.cs:123`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs#L123)

- Seuils declares D1, a calibrer a la Gate D.
  [`TrafficV2Composition.cs:149`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs#L149)

- Liberation du verrou 2a.
  [`TrackingToleranceResponse.cs:36`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrackingToleranceResponse.cs#L36)

**Preuves**

- Poussee physique reelle, Displaced puis Realign, sans teleportation, jusqu'a la sortie.
  [`Story539RecoveryPlayModeTests.cs:46`](../../Assets/RoadRage/Tests/PlayMode/Story539RecoveryPlayModeTests.cs#L46)

- Pilote reel : attente legitime, recul sur deficit, Faulted.
  [`Story539RecoveryPlayModeTests.cs:127`](../../Assets/RoadRage/Tests/PlayMode/Story539RecoveryPlayModeTests.cs#L127)

- Verrou 2a reel jusqu'a Held, puis recuperation et sortie (test 5.52 adapte).
  [`Story552MilestonePlayModeTests.cs:129`](../../Assets/RoadRage/Tests/PlayMode/Story552MilestonePlayModeTests.cs#L129)

- Matrice de suppression par chaque blocker legitime.
  [`Story539RecoveryTests.cs:100`](../../Assets/RoadRage/Tests/EditMode/Story539RecoveryTests.cs#L100)

- Commandes passees au vrai composeur.
  [`Story539RecoveryTests.cs:362`](../../Assets/RoadRage/Tests/EditMode/Story539RecoveryTests.cs#L362)

- Scans structurels : aucune ecriture du corps, aucun retrait ni grant.
  [`Story539RecoveryTests.cs:469`](../../Assets/RoadRage/Tests/EditMode/Story539RecoveryTests.cs#L469)

- Recette Gate D, a executer.
  [`recipe-5-39-reattachment-gate-d.md`](recipe-5-39-reattachment-gate-d.md)
