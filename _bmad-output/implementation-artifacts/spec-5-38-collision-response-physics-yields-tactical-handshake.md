---
title: 'Story 5.38 -- Reponse aux collisions : la physique d''abord, poignee de main tactique'
type: 'feature'
created: '2026-10-07'
status: 'done'
baseline_commit: 'd50a5437cc1eca8a559390251541c3383afb3d56'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-37-safety-filter-narrow-veto-and-clamp-boundary.md'
  - '{project-root}/_bmad-output/implementation-artifacts/anomalies/epic 5/ANO-5.10-03/ANO-5.10-03.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Un vehicule V2 percute ou pousse continue de suivre sa route au pas suivant. Aucune couche ne detecte un choc significatif ni ne suspend la conduite nominale pendant que la physique le resout (`ANO-5.10-03` AC1-AC4, AC8).

**Approach:** L'analyse de collision publie des faits et soumet une requete versionnee. Une `TacticalDecision` par vehicule l'accepte ou la refuse avec une raison, et possede seule le but de reponse a collision jusqu'a sa fin. Ce but remplace `MotionCommand.Track` par une commande sans propulsion, qui passe ensuite par le `SafetyFilter` et le composeur comme toute commande.

## Boundaries & Constraints

**Always:**
- **C1, faits.** Les faits du pas sont l'impulsion, la vitesse normale relative, la duree de contact, le cote d'impact, le lacet, la vitesse angulaire, les roues au sol et le deplacement d de la caisse (`TrackingMeasurement.StepDisplacement`). Les contacts de dessous (`VehicleSuspensionModel.IsSurfaceContact`, filtre 5.15) sont exclus.
- **C2, significativite.** Le predicat est pur et deterministe. Un choc est significatif si v_rel ≥ 4 m/s (seuil de degat mesure par la 5.15) ou si Δv = J/m ≥ 1 m/s. Une poussee est significative si le contact dure ≥ 0,25 s et produit une instabilite.
- **C3, poignee de main.** L'analyse rend au plus une requete (`Version` croissante, `SourceFrameId`, faits) et ne pose jamais le but. `TacticalDecision.Submit` rend `Accepted` ou un refus parmi `InvalidRequest`, `StaleRequest`, `GoalAlreadyActive` et `FallbackLatched`. Le but se termine par `Resumed` ou `Cancelled(ExitPortalReached)`.
- **C4, reaction.** Le tirage est deterministe : `UnitDraw(graine de session, TrafficId, "collision", Version)`, reutilise de `RoutePlanner`. Les poids authores sont sur `DriverProfileDef.collisionReaction` : `Brake` 0,75, `Evade` 0,15, `MisReact` 0,10, avec `evadeSeconds` 0,8 et `misReactSeconds` 0,6. Des poids invalides donnent `Brake`.
- **C5, commandes sans propulsion.** Ces commandes ne demandent jamais de gaz et ne compensent jamais la trainee :
  - `Brake` : -b_max, roues droites.
  - `Evade` : volant a 0,5 × verrou, oppose au cote d'impact, avec -b confort pendant `evadeSeconds`.
  - `MisReact` : acceleration 0, angle applique au choc, pendant `misReactSeconds`.

  Apres sa phase, toute reaction passe a `Brake`. Sous la bande de service, une demande de freinage donne le frein a main.
- **C6, stabilite.** Le vehicule est stable si toutes ces conditions tiennent pendant 0,5 s : aucun contact significatif, |lacet| ≤ 0,3 rad/s, |ω| ≤ 0,5 rad/s et toutes les roues au sol. S'il est stable avec d ≤ ε_t, le but se termine par `Resumed`. S'il est stable avec d > ε_t, le but reste actif en `AwaitingRecovery`, au frein maintenu. La 5.39 prendra le relais.
- **C7, branchement.** Dans `Step`, l'ordre est le suivant : faits, analyse, `Submit`, mise a jour du but, commande (but ou `Track`), `Evaluate`, puis `Compose`.
  - Pendant le but, l'arbitrage est saute, la memoire longitudinale vaut `None` et la demande de carrefour est invalide (`WithFallback`).
  - Le verrou 2a d'ε_t est suspendu tant qu'un but est actif ou qu'un choc significatif est soumis au pas.
  - Le but et la raison sont publies sur le driver (`LastTactical`) et dans le texte de `TrafficDriveOutcome`.

**Ask First:**
- Fixture Story531/533/535/537 rouge apres le branchement : HALT avant d'adapter une assertion ou un seuil.
- Tout changement de seuil C2/C6 ou de poids C4 apres approbation ; toute modification de `MVP_Run`, du prefab V2 ou de Gate A : HALT.

**Never:**
- Ecriture de position, de rotation ou de vitesse, `MovePosition`, `MoveRotation`, teleportation, `RecoverAtWaypoint`, minuteur de blocage, realignement en un pas, ou aide de "stabilisation".
- Reattache a une voie, replanification, interblocage ou nettoyage (5.39 et 5.40). Logique de collision dans `Safety/`. Lecture de Rage, Fear ou ciblage.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Aucun choc | Aucun contact | Pas de requete, `Track` et intent identiques a la 5.37 | N/A |
| Frottement | v_rel = 1, Δv = 0,1, 2 pas | Non significatif | N/A |
| Bordure franchie | Contacts de dessous seuls | Exclus, non significatif | N/A |
| Choc lateral | v_rel = 6, impact a droite | `Accepted`, reaction tiree, gaz 0 | N/A |
| Poussee continue | v_rel = 1, 0,3 s, lacet 0,6 | Significatif | N/A |
| Second choc | But actif | `GoalAlreadyActive`, but conserve | N/A |
| Repli verrouille | Verrou 2a deja pose | `FallbackLatched` | Repli inchange |
| Encore instable | Contact, lacet ou roue en l'air | Pas de `Resumed` | N/A |
| Stable et deplace | d = 1 m, immobile | `AwaitingRecovery`, frein maintenu | Releve de 5.39 |
| Faits non finis | NaN | `InvalidRequest` | Aucun but |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- faits : `OnCollisionEnter/Stay/Exit` `:1043-1077` (impulsion et vitesse normale deja accumulees par episode), a etendre par accumulateurs du pas, point et cote d'impact, filtre de dessous. Verrou : `PrepareStep` `:638-640` -> `ObserveTrackingTolerance` `:941`. Branchement : `Track` `:839`, `Evaluate` `:851`, `Compose` `:864`, memoire et blockers `:879-886`, carrefour `:882`, outcome `:1018-1041`. Graine : `insertion.Seed`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs:128-238` -- `Compose(..., emergencyStop)` ; ajouter `propulsion = true`. Avec la valeur faux : gaz 0, pas de compensation de trainee, frein a main sous la bande pour a < 0.
- `Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:521` -- `UnitDraw` prive, a passer en `internal`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Safety/SafetyFilter.cs:160` -- inchange. Chemin nul admis (vehicule hors route) ; `SafetyLimits.MaxBrakingDeceleration...` = b_max.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrackingToleranceResponse.cs:24` et `TrackingMeasurement.cs:385` -- d et ε_t (`TrafficV2Settings.DeclaredTrackingTolerance`).
- `Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs:256`, `NetworkedVehicleDriverController.cs:82,385` -- seuil 5.15 de 4 m/s et filtre de dessous, reutilises sans modification.
- `Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs` et `ScriptableObjects/Vehicles/DriverProfileDef_Default.asset` -- nouveau champ serialise hors de la struct `DriverProfile` : le V1 reste inchange. Validation dans `TryValidate`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:181` -- ajouter le parametre texte `tactical`, sur le meme patron que `safety`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs:337` -- `WithFallback`.
- Patrons de test : `Tests/EditMode/Story537SafetyFilterTests.cs` (frame sur le modele committe, composeur reel, scan structurel `:376`).

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs` (nouveau) -- `CollisionFacts`, `CollisionThresholds` (C2/C6, constantes declarees), `CollisionSignificance`, `CollisionResponseRequest`, `CollisionAnalysis.Analyze` et `CollisionStability` : purs.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs` (nouveau) -- `CollisionReaction`, `CollisionReactionWeights` (serialisable), `TacticalGoal` et `TacticalReason`. `TacticalDecision` porte `Submit`, `Update`, `CommandFor` (C5) et `ToText()`, avec un seul proprietaire de l'etat du but.
- [x] `VehicleDriveIntentComposer.cs`, `RoutePlanner.cs`, `DriverProfileDef.cs` + asset, `TrafficDecisionProjection.cs` -- comme dans la Code Map.
- [x] `TrafficV2VehicleDriver.cs` -- C1, C7 et le verrou suspendu.
- [x] `Assets/RoadRage/Tests/EditMode/Story538CollisionResponseTests.cs` `[Core][Story538]` -- couvre :
  - la matrice ;
  - chaque cause C2 seule, et le determinisme ;
  - chaque refus C3, et la terminaison `Resumed` / `Cancelled` ;
  - le tirage : memes graines, memes reactions ; les trois reactions apparaissent sur 10 000 tirages ; les frequences restent a ±3 points des poids ; un poids nul n'est jamais tire ;
  - le gaz est nul pour chaque reaction et chaque phase passee au vrai composeur, sur plusieurs trainees et vitesses ; aucun couple de marche arriere n'est commande ;
  - l'esquive braque du cote oppose a l'impact ;
  - chaque cause C6 bloque `Resumed` ;
  - le scan structurel de `Collision/` et `Tactical/` (Never) ; l'analyse ne reference pas le setter du but ;
  - l'ordre des appels C7 dans la source du driver.
- [x] `_bmad-output/implementation-artifacts/recipe-5-38-push-and-impact-gate-d.md` -- recette ecrite, executee a la Gate D. Un joueur percute une IA, puis une IA en percute une autre. Le vehicule heurte est devie, peut se mettre en travers, ne se realigne pas en un pas et ne recoit aucune propulsion artificielle.

**Acceptance Criteria:**
- Given une IA en conduite normale, when elle subit une collision significative, then un but temporaire prend la priorite sur le suivi de route, sans commande de propulsion.
- Given des collisions comparables, when les reactions sont tirees, then `Brake`, `Evade` et `MisReact` sont possibles selon les poids authores, avec un tirage deterministe pour une meme graine.
- Given un vehicule en contact significatif, en rotation, deplace ou instable, when il est mis a jour, then il ne reprend pas la conduite nominale. L'analyse soumet une requete ; seule la tactique accepte ou refuse avec une raison et possede le but.
- Given aucune collision significative, when les campagnes PlayMode 5.33 et 5.35 tournent sur `MVP_Run`, then elles restent vertes et `MVP_Run` est inchange.

## Spec Change Log

- **2026-10-07 -- Revue.** Couches actives : blind-hunter (en ligne), edge-case-hunter et verification-gap. security-review est inactive : aucune frontiere reseau n'est deplacee. La revue ne releve ni intent_gap ni bad_spec.
  - **Correctifs (patch) :**
    1. La stabilite se compte seulement en freinage. Une reaction est donc toujours suivie d'au moins 0,5 s de freinage ; avant le correctif, `Resumed` pouvait tomber au pas de fin de phase.
    2. Le verrou 2a est aussi suspendu tant qu'un contact dure (`SuspendsToleranceLatch`). Une poussee lente ne peut plus le poser avant d'atteindre 0,25 s de contact.
    3. L'accumulateur de contacts est pur (`StepContactAccumulator`) et teste. Le cote d'impact est la moyenne des points de la paire la plus rapide, avec une zone morte de 0,25 m : un choc frontal ou arriere donne un cote inconnu. Les contacts sont oublies tant que le vehicule est inerte.
    4. Une esquive sans cote connu passe directement au freinage.
    5. Un refus `GoalAlreadyActive` ne masque plus la raison publiee du but.
  - **Rejetes :**
    - minuteur de but, interdit par la story ; c'est le domaine de la 5.39 ;
    - borne haute des durees authorees ;
    - validation du profil hors V2 : l'asset par defaut est valide ;
    - texte `Tactical` du driver, controle par la recette Gate D ;
    - `Pass` avec perception nulle : la surcharge par `EmergencyStop` est voulue.
  - **Reste a la Gate D :** l'extraction des contacts sur une vraie caisse (filtre de dessous, signe) n'est executee qu'en PlayMode, par la recette.
  - **Validation brute finale :**
    - EditMode : Story538 14/14, Story537 22/22, Story531 50/50 ;
    - PlayMode : Story552 4/4 (verrou 2a reel), Story535 7/7, Story533 12/12 ;
    - 0 erreur Console sur chaque fenetre, compilation saine, `MVP_Run` propre.

## Design Notes

Decisions proposees a l'approbation : D1 seuils C2/C6 declares, calibres par la recette Gate D ; D2 poids C4 ; D3 verrou 2a suspendu (C7) ; D4 `AwaitingRecovery` (C6) ; D5 carrefour invalide pendant le but ; D6 l'entree differee « memoire longitudinale apres veto » reste differee (aucun veto nominal observe). Elle ne se rouvre qu'a la Gate D.

L'ecart d est la mesure meme d'ε_t : reprendre seulement a d ≤ ε_t remet la caisse dans l'enveloppe couverte par la Gate A. Aucun nouveau seuil de deplacement n'est donc introduit.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.38 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = compte attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.37 -TestMode EditMode`, puis la meme commande avec `-Story 5.31` -- expected: verts (composeur).
- `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode PlayMode`, puis la meme commande avec `-Story 5.33` -- expected: verts (AC8).
- `git status --short` -- expected: aucun fichier `MVP_Run` ni prefab modifie.

## Suggested Review Order

**Handshake and goal ownership**

- Entry point: accept or refuse with a reason; the tactical layer alone draws the reaction and owns the goal.
  [`TacticalDecision.cs:143`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L143)

- End of the goal: stability counted only while braking; `Resumed` requires d <= epsilon_t, otherwise `AwaitingRecovery`.
  [`TacticalDecision.cs:174`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L174)

- Commands without propulsion: Brake, Evade away from the impact, MisReact coasting.
  [`TacticalDecision.cs:198`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L198)

- Authored distribution; invalid weights fall back to Brake.
  [`TacticalDecision.cs:63`](../../Assets/RoadRage/Features/Vehicles/Traffic/Tactical/TacticalDecision.cs#L63)

**Collision analysis**

- Pure significance predicate: relative speed, delta-v, sustained push with instability.
  [`CollisionResponse.cs:89`](../../Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs#L89)

- First physical cause blocking resumption.
  [`CollisionResponse.cs:102`](../../Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs#L102)

- Per-step contacts: side from mean contact points, dead zone, contact streak.
  [`CollisionResponse.cs:133`](../../Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs#L133)

- D3: the 2a latch waits for contact to end or the goal to finish.
  [`CollisionResponse.cs:114`](../../Assets/RoadRage/Features/Vehicles/Traffic/Collision/CollisionResponse.cs#L114)

**Driver wiring**

- Analysis, submission and goal update on the step's facts.
  [`TrafficV2VehicleDriver.cs:814`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L814)

- The goal replaces route following; the SafetyFilter still evaluates it.
  [`TrafficV2VehicleDriver.cs:865`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L865)

- Arbitration skipped during the goal: longitudinal memory and blockers become empty.
  [`TrafficV2VehicleDriver.cs:847`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L847)

- Composition without propulsion; junction request invalidated (D5).
  [`TrafficV2VehicleDriver.cs:907`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L907)

- 2a latch suspended through the shared predicate.
  [`TrafficV2VehicleDriver.cs:661`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L661)

- Contacts filtered by the 5.15 rule (underside excluded), one pair per call.
  [`TrafficV2VehicleDriver.cs:1117`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L1117)

**Shared boundaries**

- `propulsion` false: no throttle, no drag compensation, handbrake below the band.
  [`VehicleDriveIntentComposer.cs:130`](../../Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs#L130)

- Route draw reused, `collision` domain.
  [`RoutePlanner.cs:522`](../../Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs#L522)

- Authored weights, outside the V1 struct.
  [`DriverProfileDef.cs:45`](../../Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs#L45)

**Evidence**

- No propulsion and no reverse for every reaction, through the real composer.
  [`Story538CollisionResponseTests.cs:268`](../../Assets/RoadRage/Tests/EditMode/Story538CollisionResponseTests.cs#L268)

- Deterministic draw, frequencies within ±3 points of the weights.
  [`Story538CollisionResponseTests.cs:216`](../../Assets/RoadRage/Tests/EditMode/Story538CollisionResponseTests.cs#L216)

- Each instability blocks resumption; a displaced vehicle awaits recovery.
  [`Story538CollisionResponseTests.cs:177`](../../Assets/RoadRage/Tests/EditMode/Story538CollisionResponseTests.cs#L177)

- Contact accumulator and latch suspension.
  [`Story538CollisionResponseTests.cs:352`](../../Assets/RoadRage/Tests/EditMode/Story538CollisionResponseTests.cs#L352)

- Absence of any body write, and driver call order.
  [`Story538CollisionResponseTests.cs:425`](../../Assets/RoadRage/Tests/EditMode/Story538CollisionResponseTests.cs#L425)

- Gate D recipe (push and impact), to be executed.
  [`recipe-5-38-push-and-impact-gate-d.md`](recipe-5-38-push-and-impact-gate-d.md)
