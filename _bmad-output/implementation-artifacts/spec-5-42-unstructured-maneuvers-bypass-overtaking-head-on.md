---
title: 'Story 5.42 -- Manoeuvres non structurees : contournement, depassement, refus et abandon face au trafic oppose'
type: 'feature'
created: '2026-10-09'
status: 'done'
baseline_commit: 'cec5ba3278fa5075bcb7bce90a9516ee7b849c01'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-41-driving-policy-and-traffic-rule-exception-protocol.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Un vehicule V2 bloque par une voiture en panne, un obstacle ou un vehicule lent attend indefiniment. Toute la pile suit une reference compilee a decalage nul, et aucune manoeuvre ne produit de demande d'exception (5.41 n'a aucun producteur).

**Approach:** Un but tactique `Maneuver` enumere les candidats lateraux que la geometrie authoree offre. Il les evalue chacun dans l'ordre : faisabilite geometrique, conflits dynamiques, politique, `SafetyFilter`. Il suit le survivant sur une reference de manoeuvre, admise par sa propre preuve d'enveloppe (decision proprietaire 1a du 2026-10-09). Gate A reste inchangee et reste la preuve de la conduite nominale.

## Boundaries & Constraints

**Always:**
- **M1, candidats.** Enumeres depuis la geometrie, dans cet ordre :
  - `CorridorOffset`, toujours offert ;
  - `AdjacentCorridor`, si une `LaneAdjacency` de meme sens couvre l'etendue ;
  - `OpposingCorridor`, s'il existe un corridor antiparallele de la meme section a enveloppe contigue ;
  - `AuthorizedSurface`, jamais offert : aucune surface n'est authorable, raison `NotOffered`.

  Chaque candidat recoit un verdict et une raison stables : `NotOffered`, `GeometryInfeasible` (cause nommee), `TrafficConflict`, `PolicyRefused`, `ExceptionPending`, `ExceptionDenied`, `SafetyRejected` ou `Selected`. Parmi les survivants, le moins couteux selon la politique est retenu, l'ordre d'enumeration departageant.
- **M2, preuve de manoeuvre.** Elle couvre continument le depart, le decalage, le passage et le retour. Le gabarit maximal est pris a la pose nominale cinematique de la reference de manoeuvre. Il est gonfle de ε_t et d'un reste entre echantillons. Il doit rester dans l'union contigue des enveloppes revues des corridors autorises de la meme section. Le chemin doit aussi :
  - ne toucher aucun `JunctionMovement` ;
  - ne jamais depasser les bouts du corridor, plus la distance d'arret a la vitesse de manoeuvre ;
  - avoir une courbure continue et faisable.

  Echec = candidat refuse, fail-closed.
- **M3, conflits et politique.** La tactique calcule la marge en temps. Pour chaque acteur predit a vitesse constante, c'est son arrivee la plus precoce dans la region balayee, moins la duree de manoeuvre. Le perimetre couvre les acteurs du corridor du candidat et de l'element amont, l'obstacle contourne excepte. Une marge negative ou un acteur deja dans la region donne `TrafficConflict`. La politique fournit seulement :
  - l'eligibilite ;
  - le cout ;
  - le creneau accepte (marge ≥ `AcceptedGapSeconds`) ;
  - le risque accepte (`T_m / (T_m + marge)` ≤ `AcceptedRisk`).
- **M4, exception.** Un candidat hors de la surface permise (`OpposingCorridor`) ne part qu'apres trois etapes :
  - `DrivingPolicy.TryPropose(OpposingCorridor)`, portee : corridor oppose, terminaison `ScopeExited`, expiration bornee ;
  - soumission au lot N par le runner ;
  - une exception effective lue a la frame de depart, apres une reevaluation complete.

  L'exception n'est exigee que tant que la reference de manoeuvre restante occupe reellement l'enveloppe du corridor oppose. Une fin `ScopeExited` causee par le retour normal et prouve vers le corridor propre est une fin reussie, jamais une cause d'`Aborted` : le but termine ensuite son retour et sa stabilisation jusqu'a `Resumed` sans exception.
- **M5, but.** Pendant `Maneuver`, la reference de manoeuvre sert a tout : suivi, verrou ε_t (mesure, non suspendu), perception et `SafetyFilter`.
  - Un registre de progression constate un cale (`Stalled`).
  - Chaque pas reevalue les conflits. Deux causes declenchent l'abandon :
    - une marge restante negative ;
    - une exception disparue avant terme (`Expired`, `FrameUnavailable`, etc.) alors que la reference restante exige encore le corridor oppose.

    Le retour derriere l'obstacle se fait s'il est prouvable et finit avant l'obstacle (`Aborted`). Sinon la manoeuvre continue (`Committed`), avec le `SafetyFilter` toujours actif.
  - Fin `Resumed` seulement de retour sur la reference nominale, stable 0,5 s dans ε_t.
  - Une collision significative preempte le but (`CollisionPreempted`).
- **M6, faits.** Une voiture en panne (`TrafficActor`), la voiture du joueur (`Vehicle`), un obstacle (`Obstacle`) et un pieton (`Pedestrian`/`WalkingPlayer`) restent des faits distincts, evalues par le meme appel.

**Ask First:**
- Une fixture Story531 a Story541 rouge : HALT avant d'adapter une assertion.
- Toute modification de `MVP_Run`, du modele routier, du prefab V2, de Gate A ou de `ViolableTrafficRules` : HALT.
- Toute modification de la logique du `SafetyFilter`, ou toute valeur differente des valeurs D1 a D4 : HALT.

**Never:**
- Aucune regle « bloque ⇒ corridor oppose », ni aucun ordre de preference code en dur.
- Aucune autre surface qu'un corridor ; aucun `JunctionMovement` dans un chemin de manoeuvre.
- Aucun appel a `DriverModel.TryEvaluateLaneChange`, aucun MOBIL, aucun changement de voie de route ; aucune copie de la cadence V1.
- `DrivingPolicy` ne planifie ni ne detecte rien. Le `SafetyFilter` ne lit aucune exception. Aucune teleportation.
- La reconnaissance generale du contresens et du face-a-face est differee (`deferred-work.md`), comme l'integration du contournement dans `MVP_Run` (decision B).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Corridor large | Fixture : corridor de 9 m, obstacle centre de 1 m (decalage requis 2,37 m ≤ 3,13 m disponibles) | `CorridorOffset` `Selected`, aucune demande | N/A |
| Corridor etroit | Fixture : 4 m, voiture de 2,06 m | `CorridorOffset` `GeometryInfeasible` ; `OpposingCorridor` demande l'exception | N/A |
| Politique refuse | Surfaces sans `OpposingCorridor` | `PolicyRefused`, aucune demande | Attente legitime |
| Exception refusee | Autorite : `Denied` | `ExceptionDenied`, aucun depart | Nouvel essai apres D4 |
| Trafic en face | Marge < creneau ou risque | Refuse, aucune demande | Attente |
| Arrivee en face pendant le depart | Marge restante < 0, retour prouvable | `Aborted`, retour derriere l'obstacle | N/A |
| Idem apres le point de non-retour | Retour non prouvable | `Committed`, le `SafetyFilter` reste actif | N/A |
| Vehicule lent | Leader a 3 m/s, v_m a 8 m/s | Passage, retour, `Resumed` sans cale | `Stalled` si cale |
| Retour normal | `ScopeExited` apres reentree prouvee dans le corridor propre | Exception `Ended` (succes), but jusqu'a `Resumed` sans exception | N/A |
| Exception perdue en route | `Expired` ou `FrameUnavailable`, corridor oppose encore requis | `Aborted` si retour prouvable, sinon `Committed` | `SafetyFilter` actif |
| Fin de corridor | Retour au-dela de L moins l'arret | `GeometryInfeasible CorridorTooShort` | N/A |
| `MVP_Run` | Obstacle de gabarit voiture, avenue de 16 m | Tous candidats refuses, attente, aucune demande | Mesure enregistree |

</frozen-after-approval>

## Code Map

- `Traffic/Tactical/TacticalDecision.cs:83` (`TacticalGoalKind`, ajout `Maneuver = 3`), `:95` (`TacticalReason`, ajouts en fin), `:251` (patron `SubmitRecovery`), `:281` (`Update`), `:317` (`CommandFor`), `ProgressLedger` (registre R2, reutilise).
- `Traffic/Planning/` (nouveau `ManeuverPath.cs`) : decalage o(s) C² sur la courbe du corridor ; echantillons `RoadCurveSample` → `TrackPiece` (`Lifecycle/TrackingMeasurement.cs:84`) et `ReferenceTrack` (`:150`). Le suivi `MotionCommand.TrackingWheelAngleDegrees` (`Planning/MotionCommand.cs:148`), `StepDisplacement` (`:385`), `NominalHeadingErrorDegrees` et `TangentRateMax` sont reutilises tels quels.
- `RoadCurve.cs:254` (`Project`), `RoadCurvePoint.HalfWidthLeft/RightMeters` (largeurs revues), `CompiledRoadModel.GetCorridorsInSection` `:589`, `Adjacencies` `:467`, `ValidationProfile.EnvelopeOverlapToleranceMeters`.
- `Traffic/Tactical/` (nouveau `ManeuverEvaluation.cs`) : M1 a M3, de facon pure.
- `Traffic/Perception/TrafficPerception.cs:43` (`Observe`), `:351` (`TryObstacle`) : sur un chemin de manoeuvre, projection sur sa courbe, aucun canal structure, tout acteur devient fait d'obstacle (`:323` ecarte sinon les acteurs de l'horizon).
- `Traffic/Lifecycle/TrafficV2VehicleDriver.cs` :
  - `:659` politique ;
  - `:697` verrou (`CollisionPredicates.SuspendsToleranceLatch`, `Collision/CollisionResponse.cs:114`, qui ne suspend plus pour `Maneuver`) ;
  - `:925` commande du but ;
  - `:965` `SafetyFilter` sur le chemin de manoeuvre ;
  - `:973` exceptions ;
  - expose les demandes du pas.
- `Lifecycle/TrafficV2StepRunner.cs:251` : `coordinator.Resolve(..., ruleExceptions)` avec les demandes des pilotes du pas N.
- `Lifecycle/TrafficV2Composition.cs:144-180` : reglages D1 a D4.
- `Debug/TrafficDecisionProjection.cs` : ligne `Maneuver` (candidats et raisons), patron `WithPolicy`.
- Lecture seule : `Safety/SafetyFilter.cs:160`, `Policy/DrivingPolicy.cs:307` (`TryPropose`), `Junction/TrafficRuleAuthority.cs:127` (fin `ScopeExited`).
- Tests : `Tests/EditMode/Story532PerceptionTests.cs:163-216` (modele synthetique : sections, ordre lateral, adjacence), `Story541DrivingPolicyTests.cs` (lots du coordinateur), `Tests/PlayMode/Story533Harness.cs:260` (`CreateObstacle`) et `Story539RecoveryPlayModeTests.cs` (patron `MVP_Run`).

## Tasks & Acceptance

**Execution:**
- [x] `Traffic/Planning/ManeuverPath.cs` : chemin, vitesse de manoeuvre, preuve M2 et ses causes.
- [x] `Traffic/Tactical/ManeuverEvaluation.cs` : declenchement D1, M1, M3 et selection.
- [x] `TacticalDecision.cs` : but M5 (phases depart, passage, retour, abandon) et sa commande (IDM `DriverModel.ComputeAcceleration` sur le fait le plus proche du chemin).
- [x] `TrafficPerception.cs` : observation sur un chemin de manoeuvre.
- [x] `TrafficV2VehicleDriver.cs`, `TrafficV2StepRunner.cs`, `CollisionResponse.cs`, `TrafficV2Composition.cs`, `TrafficDecisionProjection.cs` : branchement M4 et M5.
- [x] `Tests/EditMode/Story542ManeuverTests.cs`, `[Core][Story542]`. Il couvre :
  - la matrice ;
  - M6 ;
  - la preuve continue et la courbure ;
  - un banc bicyclette (memoire « EditMode guidance bench ») qui prouve la progression sans cale ;
  - le cycle demande → lot N → effective a N+1 ;
  - la mesure `MVP_Run` sur le modele committe ;
  - un scan structurel (aucune branche « bloque ⇒ oppose », aucun `TryEvaluateLaneChange`, `DrivingPolicy.cs` inchange).
- [x] `Tests/PlayMode/Story542BypassRefusalPlayModeTests.cs`, `[Story542]` : dans `MVP_Run`, un obstacle de gabarit voiture sur le corridor d'avenue de la route. Le vehicule s'arrete derriere ; chaque candidat est publie refuse ; aucune demande, aucun but, aucun verrou, aucune recuperation, aucun contact. Une fois l'obstacle retire, le vehicule repart et sort.

**Acceptance Criteria:**
- Given une fixture large ou une adjacence de meme sens, when le meme obstacle est rencontre, then le nouveau candidat gagne par la meme evaluation, sans changement de logique.
- Given un depart par le corridor oppose, when il est execute, then une exception acceptee et effective le precede, et le chemin reste prouve dans l'enveloppe autorisee.
- Given `MVP_Run`, when la mesure s'execute, then le candidat gagnant observe (aucun) est enregistre par la story, jamais code en dur.
- Given aucun blocage, when les fixtures Story531 a Story541 tournent, then le comportement est identique.

- **2026-10-09 -- Revue.**
  - **Couches actives.** `blind-hunter` (en ligne), `edge-case-hunter` et `verification-gap`. `security-review` inactive : aucune frontiere reseau deplacee. Aucun `intent_gap` ni `bad_spec`.
  - **Correctifs appliques (patch) :**
    - une fin `ScopeExited` publiee apres le retour normal n'est plus lue comme une exception perdue (M4) ;
    - une exception deja effective n'autorise un depart que si elle couvre la sortie de l'enveloppe opposee ;
    - la region de conflit est reduite au reste du chemin pendant la supervision ;
    - un danger est teste sur son emprise le long du corridor ;
    - une construction de chemin hors domaine devient `InvalidInput`, jamais une exception du pas ;
    - aucune evaluation hors corridor de la route ou a contresens ; corridor introuvable en supervision : engagement (fail-closed) ;
    - corridor oppose le plus proche lateralement ; adjacent sans courbe ecarte ;
    - supervision et declencheur D1 extraits en fonctions pures (`ManeuverEvaluation.Supervise`, `ManeuverTrigger.Observe`) et testes, avec l'IDM le long du chemin, les fins `CollisionPreempted`, `ExitPortalReached`, `Stalled`, les causes `NoClosingSpeed`, `TooClose`, et la ligne `Maneuver` de la projection (PlayMode).
  - **Differe (`deferred-work.md`).** Boucle pilote -> runner -> autorite et supervision dans un pilote lie (prolonge le report P6 de la 5.41) ; `Stalled` sur le corridor oppose.
  - **Rejetes :** IDM qui borne a 0 la vitesse le long du chemin d'un vehicule en face (convention 5.33, story contresens differee) ; arret apres un retour d'abandon (il finit derriere la cause) ; exception detenue sans usage (autorite seule, bornee).
  - **Validation finale brute.** 0 erreur Console sur chaque fenetre, compilation saine, `MVP_Run` propre.

    | Story | EditMode | PlayMode |
    |---|---|---|
    | 5.42 | 25/25 | 1/1 |
    | 5.41 | 35/35 | -- |
    | 5.40 | 21/21 | -- |
    | 5.39 | 22/22 | 4/4 |
    | 5.38 | 16/16 | 1/1 |
    | 5.36 | 14/14 | -- |

    5.35 (51/51, 7/7) et 5.37 (22/22) ont ete rejouees avant les correctifs de revue, qui ne touchent pas leurs chemins.

## Design Notes

Decisions approuvees par le proprietaire le 2026-10-09 (D1 a D4) :
- **D1, declenchement.**
  - Un blocker dominant `Leader` ou `Obstacle` legitime, tenu au moins 2 s.
  - Vitesse de la cause au plus 0,5 × vitesse desiree.
  - Ce declenchement ouvre une evaluation, jamais un candidat.
- **D2, geometrie.**
  - Transition o(s) = d·(6u⁵ − 15u⁴ + 10u³) : courbure continue, |o''| ≤ 5,774·d/L_t².
  - L_t est le plus petit tel que |κ| tienne dans `ManeuverLateralAccelerationMetersPerSecondSquared` = 2,0 m/s² a v_m, et dans le braquage du profil a v_m.
  - Degagement longitudinal de 1,0 m, lateral de 0,5 m vis-a-vis de l'obstacle.
  - Pas de preuve de 0,1 m ; reste entre echantillons de (Δ/2)·(1 + ρ·ψ'_max).
- **D3, vitesse.**
  - v_m = min(vitesse desiree, plafonds de courbure).
  - Pour un depassement, la marge de vitesse v_m − v_cause doit valoir au moins 2 m/s, sinon `GeometryInfeasible NoClosingSpeed`.
  - La longueur de passage est etiree par v_m / (v_m − v_cause).
- **D4, exception** (amendee par la decision 4A du 2026-10-09).
  - Expiration a N + ⌈(t(LastOtherDistance) + 2 s)/Δt⌉ : temps jusqu'au dernier point de la reference qui occupe encore l'enveloppe du corridor oppose, plus 2 s. Jamais raccourcie pour faire partir une manoeuvre.
  - Au plus `MaxRuleExceptionFrames` = 1000 pas (20 s a 50 Hz, porte de 500 a 1000), sinon `PolicyRefused ExceptionWindow`, fail-closed.
  - Nouvel essai apres un refus : 2 s.

**Amendements et constats pendant l'implementation (2026-10-09).**
- **Decision 4A.** Avec D4 tel quel et 500 pas, aucun contournement oppose n'etait autorise (T_m mesure 13,2 s depuis l'arret a 6 m/s). Le plafond et le comptage ont ete amendes (D4 ci-dessus) ; specs 5.41 et 5.42 mises a jour, bornes 1000/1001 testees.
- **Lecture de D1.** La cause liante est lue sur la decision longitudinale du pas (leader suivi, obstacle, ou leur maintien a l'arret) ; le blocker dominant, s'il existe, doit etre legitime. Un vehicule lent qui ne fait pas s'arreter le suiveur n'a aucun blocker, mais reste une cause liante. Aucune valeur de D1 n'est changee.
- **Constat geometrique.** Un vehicule deja arrete a s0 = 2 m derriere une voiture en panne n'a qu'environ 1 m de depart : `TooClose`, attente legitime. L'evaluation utile a lieu pendant l'approche.
- **Marge du decalage intra-corridor.** Le decalage requis ajoute un pas de preuve (0,1 m) au gonflement : 2,47 m dans la fixture large (2,37 m + reste de preuve), toujours dans les 3,13 m disponibles.
- **Fixtures.** Le temoin C1 du validateur derive d'environ 2e-4 m par metre (integration flottante par pas de 1 cm) : les corridors synthetiques restent sous ~200 m (140 m, 180 m pour le depassement), conducteur de fixture a 6 m/s, 8 m/s pour le vehicule lent (matrice).
- **Mesure `MVP_Run` (decision B).** Gagnant observe : aucun. Sur chaque corridor d'au moins 11,7 m des sections a double sens (`Ring_*` 13,5 m, `Avenue_*` 16 m), chaque candidat est `NotOffered` ou `GeometryInfeasible` (aucune selection). Le PlayMode le confirme sur `Ring_North_West` (scenario 5.33 B) : attente, aucune demande, aucun but, aucun verrou, aucune recuperation, aucun contact, puis sortie apres retrait de l'obstacle.
- **Scans figes 5.36, 5.38, 5.39, 5.40.** Leurs sous-chaines exactes (appel du verrou, `ObserveTrackingTolerance(step, displacement, ...)`, `Coordinate(...)`, `Resolve(..., cycles)`) sont preservees par le code ; aucune assertion existante n'a ete modifiee.

**Portee de l'exception sur le corridor oppose.** Si le vehicule s'y localise, la sortie termine l'exception (`ScopeExited`). Sinon l'expiration la borne. Une portee sur le corridor propre, au contraire, la terminerait en pleine manoeuvre des la premiere relocalisation.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.42 -TestMode Both` : expected `VALIDATION STORY`, compte execute egal au compte attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.41 -TestMode EditMode`, puis `-Story 5.39 -TestMode PlayMode` : expected verts.
- `git status --short` : expected aucun fichier `MVP_Run`, modele ou prefab modifie.

## Suggested Review Order

**Evaluation des candidats (M1-M3)**

- Point d'entree : enumeration, puis selection par cout seul, ordre ensuite.
  [`ManeuverEvaluation.cs:145`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L145)
- Ordre impose par candidat : geometrie, conflits, politique, exception, SafetyFilter.
  [`ManeuverEvaluation.cs:161`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L161)
- Decalage vise, vitesse plafonnee par la longueur, maintien etire, corridor trop court.
  [`ManeuverEvaluation.cs:256`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L256)
- Marge en temps : arrivee la plus precoce, region reduite au reste du chemin.
  [`ManeuverEvaluation.cs:345`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L345)
- Exception effective et couvrant la sortie de l'enveloppe opposee.
  [`ManeuverEvaluation.cs:242`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L242)
- Fenetre 4A jusqu'a LastOtherDistance + 2 s, plafond 1000 pas fail-closed.
  [`ManeuverEvaluation.cs:226`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L226)

**Reference de manoeuvre et preuve (M2)**

- Preuve continue : gabarit cinematique gonfle dans l'union contigue, aucun mouvement de carrefour.
  [`ManeuverPath.cs:368`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/ManeuverPath.cs#L368)
- Decalage C2 echantillonne en TrackPiece : suivi et epsilon_t reutilises tels quels.
  [`ManeuverPath.cs:238`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/ManeuverPath.cs#L238)
- Courbure admise : acceleration laterale D2, braquage, rayon d'admission.
  [`ManeuverPath.cs:321`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/ManeuverPath.cs#L321)

**But Maneuver et supervision (M4-M5)**

- Decision pure : conflit ou exception perdue -> abandon prouve, sinon engagement ; ScopeExited = succes.
  [`ManeuverEvaluation.cs:531`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L531)
- Retour d'abandon prouve, fini derriere la cause.
  [`ManeuverEvaluation.cs:583`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L583)
- Phases, fin Resumed sur la reference nominale, Stalled par le registre R2.
  [`TacticalDecision.cs:509`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L509)
- Commande : loi de suivi sur la reference de manoeuvre, IDM sur le couloir balaye.
  [`TacticalDecision.cs:545`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L545)
- Declencheur D1 : meme cause lente et legitime tenue 2 s.
  [`ManeuverEvaluation.cs:638`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/ManeuverEvaluation.cs#L638)

**Branchement pilote, runner, perception**

- Verrou epsilon_t mesure contre la reference de manoeuvre, appel D3 5.38 preserve.
  [`TrafficV2VehicleDriver.cs:717`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L717)
- Pendant le but : supervision, proximite et SafetyFilter le long du chemin.
  [`TrafficV2VehicleDriver.cs:964`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L964)
- Evaluation, demande au lot N, depart a N+1, delai apres refus.
  [`TrafficV2VehicleDriver.cs:1103`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L1103)
- Supervision du pilote et suivi de la fin ScopeExited publiee.
  [`TrafficV2VehicleDriver.cs:1201`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L1201)
- Demandes du pas N soumises au meme lot que les grants.
  [`TrafficV2StepRunner.cs:262`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs#L262)
- Faits d'obstacle le long de la courbe du chemin, aucun canal structure.
  [`TrafficPerception.cs:82`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L82)
- Reglages D1-D4 et plafond 1000 pas (decision 4A).
  [`TrafficV2Composition.cs:184`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs#L184)

**Preuves**

- Banc bicyclette : contournement et depassement lent sans cale, dans epsilon_t, Resumed.
  [`Story542ManeuverTests.cs:373`](../../Assets/RoadRage/Tests/EditMode/Story542ManeuverTests.cs#L373)
- Supervision : abandon avant, engagement apres le point de non-retour.
  [`Story542ManeuverTests.cs:498`](../../Assets/RoadRage/Tests/EditMode/Story542ManeuverTests.cs#L498)
- Bornes 1000/1001 a l'autorite et dans la fenetre de manoeuvre.
  [`Story542ManeuverTests.cs:264`](../../Assets/RoadRage/Tests/EditMode/Story542ManeuverTests.cs#L264)
- Mesure MVP_Run : aucun candidat ne survit.
  [`Story542ManeuverTests.cs:435`](../../Assets/RoadRage/Tests/EditMode/Story542ManeuverTests.cs#L435)
- PlayMode MVP_Run : refus fail-closed, attente, puis sortie.
  [`Story542BypassRefusalPlayModeTests.cs:43`](../../Assets/RoadRage/Tests/PlayMode/Story542BypassRefusalPlayModeTests.cs#L43)
