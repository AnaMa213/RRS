---
title: 'Story 5.37 -- SafetyFilter : veto etroit et frontiere de bornage'
type: 'feature'
created: '2026-10-07'
status: 'done'
baseline_commit: 'e922d455b97a2a777c7be8db4722c176a2181c17'
review_loop_iteration: 1
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-36-signal-phase-runtime-and-coordinator-integration.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le pipeline V2 va de `MotionCommand.Track` directement au composeur. Aucune frontiere ne rejette une commande invalide pour une raison objective, et rien ne protege d'une collision imminente quand la planification freine trop peu. Les stories 5.38 et 5.39 seraient tentees d'y loger de la tactique.

**Approach:** Ajouter un `SafetyFilter` pur et sans etat entre la commande de suivi et le composeur. Il rend un verdict (`Pass`, `Clamp`, `EmergencyStop` ou `Reject`) et exactement une raison. Il lit seulement la commande, l'acteur, la frame, les faits de proximite et l'`AuthorizedContact` porte par le plan.

## Boundaries & Constraints

**Always:**
- **S1, branchement.** Le filtre est appele dans `TrafficV2VehicleDriver.Step`, apres `Track` et avant `Compose`. `Pass` transmet la commande inchangee et `Clamp` la commande bornee. `EmergencyStop` transmet la meme commande avec l'acceleration `-b_max`. `Reject` ne transmet aucune commande et passe la raison `V2FallbackReason.SafetyRejected` (= 12, ajoutee en fin d'enum) au repli. Le filtre n'ecrit ni intent ni pedale.
- **S2, raisons.** L'enum `SafetyReason` est stable, avec des valeurs ajoutees en fin d'enum. Une seule raison par verdict, dans cet ordre de priorite :
  - `InvalidActorState` : vitesse ou pose non finie, ou acteur absent de la frame. Verdict `Reject`.
  - `NonFiniteOutput` : acceleration, angle ou courbure de reference non fini. Verdict `Reject`.
  - `StalePlan` : pas physique hors de la fenetre de validite, ou `SourceFrameId` posterieur a la frame. Verdict `Reject`.
  - `LocalPlanInvalidated` : un element du chemin est ferme dans la frame evaluee (`IsClosed`). Verdict `Reject`.
  - `PhysicallyInvalidPath` : |courbure de reference| > 1/`AdmissionRadiusMeters` × (1 + 1e-3). Verdict `Reject`.
  - `ImminentUnintendedCollision` : voir S3. Verdict `EmergencyStop`.
  - `PhysicallyInvalidIntent` : angle au-dela de `LowSpeedLockDegrees`, ou acceleration hors de [-b_max, a_max]. Verdict `Clamp`.
  - `None` : verdict `Pass`.

  `b_max` et `a_max` sont les capacites physiques de frein et de moteur du composeur.
- **S3, collision imminente.** Pour chaque danger, c'est-a-dire le leader et les obstacles du couloir balaye de `LongitudinalPerception`, on calcule l'ecart g et la vitesse d'approche c = v − v_danger. Le danger est imminent si c > 0 et g ≤ c·τ + c²/(2·b_max), avec τ = fenetre de validite × dt. Un contact deja etabli sans approche (c ≤ 0) n'est pas imminent ; il releve de la 5.39.
- **S4, `AuthorizedContact`.** Contrat de donnees : `TargetId`, `MaxClosingSpeedMetersPerSecond` (scope : vitesse d'approche maximale au contact, finie et ≥ 0) et `ExpiresAtStep`. Il est porte par le plan, sous forme d'argument nullable du filtre ; aucun producteur avant la 5.44. Un scope non fini ou negatif rend l'autorisation sans effet. L'autorisation peut seulement retirer un danger de l'evaluation `ImminentUnintendedCollision`, et seulement si ce danger remplit trois conditions :
  - son identifiant est egal a `TargetId` ;
  - le pas courant est ≤ `ExpiresAtStep` ;
  - c ≤ `MaxClosingSpeedMetersPerSecond`.

  Elle ne neutralise jamais une autre raison ni un autre verdict. Les autres dangers restent evalues. `WalkingPlayer` et `Pedestrian` ne sont jamais exemptes. Le filtre ne lit aucun etat Rage, Fear ou ciblage.
- **S5, epoques.** Le resultat publie `SourceFrameId` (plan), `PhysicsStep` et `NearFieldStep`. Un echantillon de proximite plus recent, s'il est fourni, remplace la perception source **pour S3 seulement**. Il ne modifie jamais la commande autrement que par un veto.
- Le verdict est publie sur le driver (`LastSafety`) et dans le texte du `TrafficDriveOutcome`. Il reste cote hote uniquement.

**Ask First:**
- Fixture Story531/533/534/535 rouge (EditMode ou PlayMode) apres le branchement : HALT avant d'adapter une assertion ou un seuil.
- Toute modification de `MVP_Run` (scene, authoring, modele, signoff) : HALT.

**Never:**
- Choix de route, classement de manoeuvre, negociation de carrefour, jugement de regle de circulation, choix de cible, escalade de recuperation, evitement ou contournement dans `Safety/`.
- Lecture de `RoutePlan`, `JunctionSnapshot`, `JunctionCoordinator`, `LongitudinalArbitration.Decide`, `BlockerTracker` ou d'un type Rage/Fear depuis `Safety/`.
- Producteur d'`AuthorizedContact` (5.44) ; collision response (5.38) ; changement de `VehicleDriveIntent.Idle` ou du repli V2.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Route libre | Commande finie, valide, sans danger | `Pass/None`, commande identique | N/A |
| Angle excessif | angle = lock + 5° | `Clamp/PhysicallyInvalidIntent`, angle = lock | N/A |
| Leader freine fort | c = 10, g = 5, b_max = 8 | `EmergencyStop/ImminentUnintendedCollision`, a = −b_max | N/A |
| Contact autorise | Danger = cible, non expire, c ≤ scope | `Pass/None` | N/A |
| Autorisation expiree ou hors scope | Pas > expiry, ou c > scope | `EmergencyStop` | N/A |
| Pieton cible | `TargetId` = pieton | `EmergencyStop` | Jamais exempte |
| Autorisation et autre raison | Cible autorisee, angle excessif | `Clamp/PhysicallyInvalidIntent` | L'autorisation n'agit que sur S3 |
| Scope invalide | `MaxClosingSpeedMetersPerSecond` NaN ou < 0 | `EmergencyStop` | Autorisation sans effet |
| Plusieurs causes | NaN + danger imminent | `Reject/NonFiniteOutput` | Priorite S2 |
| Echantillon recent | Source sans danger, echantillon imminent | `EmergencyStop`, `NearFieldStep` publie | N/A |
| Contact etabli | g < 0, c ≤ 0 | `Pass` | Releve de 5.39 |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs:800-836` -- insertion entre `MotionCommand.Track` (`:818`) et `composer.Compose` (`:833`). La perception `perceived` est construite en `:789`, dans le bloc d'arbitrage ; elle doit etre hissee hors de ce bloc pour etre transmise au filtre. Chemin et spans : `decision.Path`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs:8-28,251-265` -- `V2FallbackReason` (ajouter `SafetyRejected = 12`). Rendre `ResolveCapacities` `internal static` pour obtenir b_max et a_max. Le composeur garde ses propres gardes (AD-39).
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs:11-54` -- champs lus : `SourceFrameId`, fenetre, `IsFinite`, `ReferenceCurvaturePerMeter`. Constructeur reutilise pour `Clamp` et `EmergencyStop`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs:159-251` -- `LongitudinalPerception` (leader et obstacles du couloir, `Kind`, `Id`) : conteneur de faits reutilise tel quel, sans appel a `Decide`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs:57-69` -- `AdmissionRadiusMeters`. `Frame/TrafficFrame.cs:187,269` -- `TryGetActor`, `IsClosed`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:160-188` -- `TrafficDriveOutcome` : ajouter le texte du verdict.
- `Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef` -- ne reference pas `RoadRage.Features.Rage` : garantie structurelle deja en place.
- Patrons de test : `Tests/EditMode/Story530PlanningSpineTests.cs:244-265` (scan de sources interdites), `Story536SignalTests.cs:357-380` (ordre des appels dans la source du runner).
- `Tests/EditMode/TrafficOracle/TrafficTraceRecord.cs:74` -- `SafetyResult` reserve a la V2 : non branche ici.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs` (nouveau) -- `SafetyVerdict`, `SafetyReason`, `AuthorizedContact`, `SafetyLimits`, `SafetyResult` et `SafetyFilter.Evaluate` statique et pur ; `ToText()` invariant de culture.
- [x] `VehicleDriveIntentComposer.cs` -- `SafetyRejected` ; capacites accessibles.
- [x] `TrafficV2VehicleDriver.cs` -- branchement S1, `LastSafety`, verdict dans l'outcome.
- [x] `TrafficDecisionProjection.cs` -- champ texte `Safety` de `TrafficDriveOutcome`.
- [x] `Tests/EditMode/Story537SafetyFilterTests.cs` `[Core][Story537]` -- couvre :
  - un test par raison, chacune declenchee seule par sa propre cause ;
  - la matrice ;
  - la priorite S2 ;
  - S4 : identite, scope, expiry, pieton et joueur a pied ;
  - S5 : epoques distinctes, l'echantillon recent n'agit que par veto ;
  - un scan structurel des tokens interdits sous `Safety/` (Never) ;
  - l'ordre des appels `Track` → `Evaluate` → `Compose` dans la source du driver.

**Acceptance Criteria:**
- Given une commande proposee, when le filtre l'evalue, then il rend un verdict et exactement une raison parmi les seuls motifs S2.
- Given un plan portant un `AuthorizedContact`, when le contact avec la cible est imminent, then la classification vient du plan seul, et tout autre danger reste involontaire, joueurs et pietons compris.
- Given `Safety/`, when il est inspecte, then il ne contient aucune logique de route, de manoeuvre, de carrefour, de regle, de cible ou de recuperation.
- Given le branchement, when les campagnes PlayMode 5.33 et 5.35 tournent sur `MVP_Run`, then elles restent vertes et `MVP_Run` est inchange.

### Review Findings — 2026-10-07

Review target: `e922d45..a313d50`, checked on `4450afd`. Four independent configured review layers completed. [Review report and validation evidence](code-review-5-37-2026-10-07.md).

- [x] [Review][Patch][Medium] R1 — Render the safety verdict, objective reason and epochs in `TrafficDecisionProjection.ToText()`. The driver supplies `Drive.Safety`, but the textual projection never reads it. Fixed; all four verdicts, epochs and culture-invariant output covered in Story537.
- [x] [Review][Patch][High] R2 — Preserve emergency braking through composition: normal damping compensation reduces the requested maximum braking, and normal translation coasts for `0.05 < speed <= ServiceBandMetersPerSecond`. Fixed with an explicit emergency translation in the composer and verdict passed from the driver; normal-command, fallback and Idle semantics retained. Story537 covers nonzero damping, full brake/handbrake around the service band and no reverse drive torque.
- [x] [Review][Defer] R3 — Longitudinal memory after a safety veto remains the previously approved deferral in `deferred-work.md`; runtime driver veto coverage remains assigned to 5.38 by the existing spec change log.

## Spec Change Log

- **2026-10-07 — Requested independent code review and closeout.** Four configured layers completed; R1/R2 fixed and covered by two additional Story537 tests. The diagnostic text publishes every safety verdict and its reason/epochs. The composer receives the emergency verdict explicitly and applies full service brake, or handbrake below its service band, without damping reducing the braking authority. Safety's frozen command contract, normal translation, fallback and Idle are preserved. Post-fix validation through `validate.ps1`: Story537 EditMode 22/22, Story531 EditMode 50/50, Story535 PlayMode 7/7, Story533 PlayMode 12/12; zero Console errors on every validation window, compilation healthy, MVP_Run unchanged and clean in memory. Approved longitudinal-memory and runtime driver-veto deferrals remain with 5.38. Graphify updated. Spec and sprint entry set to `done` under the owner's explicit request to close 5.37. [Review dispositions and raw results](code-review-5-37-2026-10-07.md).

- **2026-10-07 -- Revue (blind-hunter en ligne, edge-case-hunter, verification-gap ; security-review inactive, aucune frontiere reseau).** Aucun intent_gap ni bad_spec. Patchs : (1) aucun verdict evalue ni publie quand le verrou de tolerance retire la commande ; (2) echantillon recent ignore s'il est sans perception, non posterieur a la frame ou au-dela du pas courant ; (3) bornes calculees avant le composeur : profil degenere (capacite nulle) -> avertissement unique et vehicule inerte, au lieu d'une exception suivie de bornes nulles ; (4) `SafetyResult.Refusal`, et tests : bornes de `SafetyLimits.For` egales aux capacites reelles du composeur et a la drivabilite declaree, verdicts passes au vrai composeur (Reject -> repli `SafetyRejected`, EmergencyStop -> frein plein), source du driver renforcee. Differe : memoire longitudinale apres un veto (`deferred-work.md`). Rejetes : perception indisponible lue comme absence de fait (l'arbitrage la traite, Safety n'absorbe pas la routine), danger non fini, marche arriere (V2 ne recule pas), libelle `PhysicsStep` (une frame par pas hote). Reste : le driver lui-meme n'est exerce qu'en PlayMode nominal (verdict Pass) ; ses vetos le seront par la 5.38. Validation brute : Story537 EditMode 20/20, Story531 EditMode 50/50, Story535 PlayMode 7/7, Story533 PlayMode 12/12, 0 erreur Console, `MVP_Run` propre.

## Design Notes

Le seuil S3 fait de la planification la seule responsable du freinage courant. L'IDM 5.33 freine bien avant que g atteigne la distance d'arret a b_max augmentee de la latence. Le filtre n'agit donc qu'au dernier moment, quand seul le frein maximal peut encore aider. Il ne choisit jamais une autre trajectoire. Un `EmergencyStop` reste une commande, traduite par le composeur en frein de service plein, puis en frein a main sous la bande de service.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.37 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = compte attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode PlayMode`, puis `-Story 5.33 -TestMode PlayMode` -- expected: verts (non-regression du branchement).
- `.\scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode EditMode` -- expected: vert (composeur et repli).
- `git status --short` -- expected: aucun fichier `MVP_Run` modifie.

## Suggested Review Order

**Frontiere de surete**

- Point d'entree : une raison unique par verdict, dans l'ordre de priorite S2.
  [`SafetyFilter.cs:160`](../../Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs#L160)

- Echantillon recent accepte seulement s'il est posterieur a la frame et au plus au pas courant.
  [`SafetyFilter.cs:165`](../../Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs#L165)

- Seuil S3 : approche positive dans l'enveloppe d'arret a b_max plus la latence.
  [`SafetyFilter.cs:225`](../../Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs#L225)

- Exemption limitee a la cible, a l'expiration et au scope ; pietons et joueurs a pied exclus.
  [`SafetyFilter.cs:54`](../../Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs#L54)

- Raisons stables, ordre de priorite porte par l'enum.
  [`SafetyFilter.cs:17`](../../Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs#L17)

**Branchement dans le driver**

- Filtre entre Track et Compose, ignore si le verrou de tolerance retire la commande.
  [`TrafficV2VehicleDriver.cs:851`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L851)

- Bornes physiques calculees avant le composeur ; profil degenere : vehicule inerte.
  [`TrafficV2VehicleDriver.cs:618`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L618)

- Bornes reprises des capacites memes du composeur et de la drivabilite declaree.
  [`SafetyFilter.cs:96`](../../Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs#L96)

- Rejet traduit en repli V2, raison ajoutee en fin d'enum.
  [`VehicleDriveIntentComposer.cs:29`](../../Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs#L29)

- Verdict publie dans le resultat de conduite, hote seul.
  [`TrafficV2VehicleDriver.cs:1039`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L1039)

**Preuves**

- Priorite : plusieurs causes, une seule raison.
  [`Story537SafetyFilterTests.cs:183`](../../Assets/RoadRage/Tests/EditMode/Story537SafetyFilterTests.cs#L183)

- Contact autorise : n'agit que sur S3, jamais sur une autre raison.
  [`Story537SafetyFilterTests.cs:237`](../../Assets/RoadRage/Tests/EditMode/Story537SafetyFilterTests.cs#L237)

- Echantillon recent : veto seulement, echantillons invalides ignores.
  [`Story537SafetyFilterTests.cs:262`](../../Assets/RoadRage/Tests/EditMode/Story537SafetyFilterTests.cs#L262)

- Verdicts passes au vrai composeur, bornes reelles du vehicule.
  [`Story537SafetyFilterTests.cs:318`](../../Assets/RoadRage/Tests/EditMode/Story537SafetyFilterTests.cs#L318)

- Absence structurelle de logique tactique et d'etat.
  [`Story537SafetyFilterTests.cs:344`](../../Assets/RoadRage/Tests/EditMode/Story537SafetyFilterTests.cs#L344)
