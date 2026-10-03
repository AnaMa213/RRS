---
title: 'Story 5.34 -- Coordination de carrefour : demandes, grants, zones de conflit, sortie bloquee et traversee engagee, avec premier branchement runtime'
type: 'feature'
created: '2026-10-03'
status: 'draft'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-33-vehicle-following-through-named-speed-constraints.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Aucun mecanisme ne decide qui peut entrer dans un carrefour. Dans `explore-8` (5.33), deux vehicules V2 sur `40ca7f10` et `4e437f94`, membres de `ConflictZones[23]` (`4258af54`, `TJunction_West`, convergence vers le corridor `40e937a9`), entrent ensemble. Ils se touchent, puis se tiennent mutuellement en StopHold a jeu negatif pendant 113 s. La regle D12 du harnais 5.33 ne fait que classer ce cas comme constat.

**Approach:** Un coordinateur hote unique, generique pour les 9 carrefours.
- Les demandes derivees de la frame N sont resolues en un lot deterministe, publie comme grants effectifs a N+1.
- Incompatibilite = co-appartenance a une `ConflictZone` authoree. Sortie verifiee avant tout departage.
- Departage : anciennete de demande (`RequestSinceFrame`), puis `TrafficId`.
- Premier branchement runtime : un vehicule sans grant s'arrete avant l'entree du mouvement par une contrainte nommee.
- Scenarios PlayMode cibles, puis retrait de D12.

## Boundaries & Constraints

**Always:**

- **Decisions proprietaires du 2026-10-03** (Design Notes O1-O5) : perimetre B, epics.md amende en phase 0, `sprint-status.yaml` de la 5.33 laisse en `review`.
- **Coordinateur** (`Junction/`, pur : ni `Physics.*`, ni `UnityEngine.Object`, ni membre public nomme `Brake`, `Throttle` ou `Pedal`). Il possede seul l'etat mutable de coordination : grants anterieurs et anciennete des demandes. Il ne choisit pas de route, ne genere pas de chemin et ne commande aucun vehicule.
- **Incompatibilite.** Deux mouvements distincts sont incompatibles si et seulement s'ils appartiennent a une meme `ConflictZone` compilee. Index construit une fois depuis le modele, sans inference geometrique. Un mouvement est compatible avec lui-meme.
- **Lot** (frame N), dans cet ordre :
  1. Grants anterieurs, par `TrafficId` :
     - acteur absent de la frame -> `Revoked(ActorGone)` ;
     - mouvement libere -> `Released(Cleared)` ;
     - non engage et sans demande valide pour ce mouvement -> `Revoked(RequestWithdrawn)` ;
     - non engage et sortie insuffisante -> `Revoked(ExitBlocked)` ;
     - sinon renouvele : `Held(Committed)` s'il est engage, `Held(Pending)` sinon.
  2. Nouvelles demandes, triees par (`RequestSinceFrame`, `TrafficId`). Pour chacune, dans cet ordre :
     - **sortie** : sinon `Denied(ExitBlocked)` ;
     - **mouvement occupe** par un acteur V2 non autorise et incompatible : sinon `Denied(ConflictOccupied, occupant, zone)` ;
     - **grant incompatible** (renouvele ou emis plus tot dans le lot) : sinon `Denied(ConflictGranted, titulaire, zone)` ;
     - **demande plus ancienne refusee pour conflit** et incompatible : sinon `Denied(SeniorRequestPending, demandeur)` ;
     - sinon `Granted`.
  3. Publication d'un instantane immuable `EffectiveFrame = N+1`. Chaque record porte : `TrafficId`, mouvement, `RequestSinceFrame`, `SourceFrame = N`, `EffectiveFrame`, `ExpiresAfterFrame = N+1`, statut, raison stable, titulaire ou zone en cause. Un grant non republie expire.
- **Anciennete (O2).** `RequestSinceFrame` = premiere frame d'une suite ininterrompue de demandes valides du meme vehicule pour le meme mouvement. Elle n'est remise a zero que par une frame sans demande ou un changement de mouvement. `TrafficId` ne sert qu'au departage a anciennete egale.
- **Demande valide** (une au plus par vehicule et par frame, construite par une fonction pure depuis la frame et l'horizon de la decision) :
  - premier `JunctionMovement` de la route restante a ou devant le pare-chocs avant ;
  - vehicule localise, hors repli ;
  - distance pare-chocs avant -> entree du mouvement `d` ≤ `s*(v) + JunctionEngageMarginMeters + JunctionRequestLeadMeters`, avec `s*(v) = s0 + v·T + v²/(2·√(a·b))` (profil) ;
  - perception disponible, et aucun leader ni obstacle du couloir balaye avant l'entree (tete de file).
- **Engagement.** Un vehicule est engage pour son mouvement s'il tient un grant effectif et que, au choix :
  - son pare-chocs avant a franchi l'entree du mouvement ;
  - il ne peut plus s'arreter confortablement : `d < v²/(2·b)`.
  Un grant engage n'est jamais revoque. Il n'est retire que par `Released` ou `ActorGone`.
- **Liberation (O4).** Un grant est libere quand :
  - le titulaire est localise hors du mouvement et son occupation ne le chevauche plus : sur le corridor de depart, `SMin ≥ 0` ;
  - ou, s'il n'est pas localise, quand les coins de son empreinte ne sont plus dans la `Boundary` du carrefour (`Released(ClearedUnlocalized)`).
  Un titulaire en repli qui occupe encore le mouvement garde son grant. Aucun grant ne survit sans demande valide ou sans occupant reel.
- **Sortie** (avant tout departage). Longueur libre = distance le long de la route du demandeur, depuis la fin du mouvement, jusqu'au `SMin` du premier occupant V2 sur les elements suivants. Elle est bornee a `L + s0`, ou `L` est la longueur de l'empreinte declaree.
  - Les grants deja emis vers le meme corridor de depart, dont le titulaire n'y est pas encore, retranchent chacun `L + s0`.
  - Les corridors d'anneau courts (0,87 m) sont donc traverses, pas consideres comme pleins.
  - Les dangers non V2 ne comptent pas.
- **Branchement runtime.**
  - `TrafficV2StepRunner` possede le coordinateur. Par pas hote : frame N -> pas des vehicules avec l'instantane `EffectiveFrame == N` -> resolution des demandes de N -> instantane N+1.
  - Un instantane dont `EffectiveFrame ≠ N` vaut « aucun grant ». Aucun vehicule ne lit un grant non publie.
  - Echec de la frame : seuls les grants engages sont republies ; les autres sont `Revoked(FrameUnavailable)`.
  - Le coordinateur tourne aussi en production (population 1).
- **Contrainte `JunctionEntry`** (arbitrage 5.33, par ajout).
  - Active si le prochain mouvement n'a pas de grant effectif, que le vehicule n'est pas entre et que `d ≤ s*(v) + JunctionEngageMarginMeters`.
  - Candidat : IDM vers un obstacle fixe virtuel a l'entree. Equilibre : pare-chocs avant a s0 de l'entree.
  - Rang d'egalite entre `StopHold` et `Obstacle` ; ordre relatif 5.33 inchange.
  - Cause de StopHold D11, liberee quand le grant devient effectif. Une perception indisponible ne libere jamais.
  - Reprise lissee D5 comme `LeaderFollowing` et `Obstacle`.
  - Sans contrainte active, l'arbitrage est celui de la 5.33 au bit pres.
- **Blockers** (par ajout) :
  - `JunctionGrant` : refus pour conflit, titulaire ou occupant en cause ;
  - `BlockedExit` : refus pour sortie insuffisante ;
  - source `JunctionCoordination`, legitimes, liberation attendue, non recouvrables. Regle 5.33 : seulement comme cause reelle d'immobilisation.
- **Valeurs** (constantes dans `TrafficV2Settings`, declarees avant mesure) : `JunctionEngageMarginMeters` = 2 m ; `JunctionRequestLeadMeters` = 10 m.
- **Projection et trace**, par ajout, `ToText()` invariant de culture :
  - demande : mouvement, `d`, anciennete, tete de file, engagement ;
  - statut, raison, titulaire ou zone ;
  - compteurs du lot, dont `EnteredWithoutGrant`.
  - Hote seul : aucune `NetworkVariable`, RPC ni `OnValueChanged`.
- **Controles de `MVP_Run` inchanges** : 40 `Uncontrolled`.

**Ask First:**

- Revision de `JunctionEngageMarginMeters` ou `JunctionRequestLeadMeters`. Toute hysteresis, temporisation ou expiration de grant au-dela d'un lot.
- Toute modification de semantique 5.33 de l'arbitrage, du StopHold ou des blockers autre que l'ajout de `JunctionEntry`, `JunctionGrant` et `BlockedExit`.
- Le modele, la scene, la geometrie, les controles, `Traffic/Migration/**`, Gate A ; tout type public 5.25–5.33 modifie autrement que par ajout optionnel.
- Non-regression rouge (`Story531`, `Story552`, `Story533` A et B) : HALT, sans adapter de seuil ni de test.
- Precondition du scenario C inatteignable sans retoucher les contraintes : HALT, ne rien regler pour la provoquer.
- `EnteredWithoutGrant > 0`, ou un verrou 2a dans une campagne nominale une fois D12 retiree : HALT et analyse.
- Faire compter les dangers non V2 dans la sortie ; population > 8 ; `NetworkVariable`, RPC, asmdef, package.

**Never:**

- Stop, cedez-le-passage, priorite routiere, priorite d'anneau, ligne d'arret, signaux (5.35, 5.36). Gate C. Recuperation, y compris d'un StopHold a jeu negatif (5.39). Escalade d'interblocage (5.40). Exceptions de regle (5.41).
- Une negociation entre deux vehicules comme autorite. Une inference d'incompatibilite ou de priorite depuis la geometrie, un nom, un cycle ou l'ordre de mise a jour.
- Un coordinateur qui ecrit frein ou gaz, ou un second composeur. Agir sur un grant dans la frame qui l'emet. Une liberation par minuterie.
- Retirer, teleporter ou reinserer un vehicule ailleurs qu'a un portail de sortie.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Deux demandes incompatibles | Lot N, ZC23 | Un `Granted` effectif a N+1, l'autre `Denied(ConflictGranted)` | N/A |
| Ordre melange | Memes demandes permutees | Instantane identique | N/A |
| Anciennete egale | Meme `RequestSinceFrame` | `TrafficId` le plus petit servi | N/A |
| Plus ancien a id fort | Demandeur id 8 plus ancien qu'id 1 | Id 8 servi d'abord | N/A |
| Flux de jeunes compatibles avec le titulaire mais pas avec l'ancien | Ancien refuse pour conflit | Jeunes `SeniorRequestPending` ; ancien servi a la liberation | N/A |
| Mouvements compatibles | Pas de zone commune, meme approche, ou meme mouvement | Grants simultanes | N/A |
| Sortie et conflit | Sortie pleine et zone tenue | `Denied(ExitBlocked)`, evalue d'abord | N/A |
| Sortie courte d'anneau | Corridor 0,87 m vide puis suivant | Longueur cumulee sur la route, grant possible | N/A |
| Demande interrompue | Une frame sans demande | Anciennete remise a la reprise ; grant non engage `RequestWithdrawn` | N/A |
| Revocation avant engagement | Sortie qui se remplit, `d ≥ v²/2b` | `Revoked(ExitBlocked)`, arret avant l'entree | N/A |
| Engage | `d < v²/2b` ou entre | `Held(Committed)` malgre sortie ou conflit nouveaux | N/A |
| Repli dans le mouvement | Verrou 2a, occupe le mouvement | Grant tenu ; libere des que l'empreinte quitte la frontiere | N/A |
| Despawn ou disparition | Acteur absent de la frame | `Revoked(ActorGone)` au lot suivant | N/A |
| Entre sans grant | Localise sur le mouvement, aucun grant | Conflits refuses `ConflictOccupied` | Compteur `EnteredWithoutGrant` |
| Pas tete de file | Leader ou obstacle avant l'entree | Aucune demande ; `JunctionEntry` peut s'appliquer | N/A |
| Frame refusee | `new TrafficFrame` echoue | Engages republies, autres `FrameUnavailable` | Diagnostic |
| Instantane decale | `EffectiveFrame ≠ N` | Lu comme aucun grant | Diagnostic |
| Carrefour libre, vehicule seul | Demande a l'avance | Grant avant engagement ; arbitrage 5.33 au bit pres | N/A |
| Demande valide sans ConflictZone | Mouvement sans conflit | `Granted` si sortie suffisante | N/A |
| Mouvement inconnu | Id absent du modele | `Denied(InvalidRequest)` | Diagnostic |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs` -- lecture seule :
  - `:113-142` `CompiledJunctionMovement` (`FromCorridorId`, `ToCorridorId`, `Curve`, `LengthMeters`) ;
  - `:166-180` `CompiledConflictZone` (`MemberMovementIds`, `Volume`) ;
  - `:462` `ConflictZones`, `:510` `TryGetMovement`, `:570` `GetConflictZonesInJunction`.
  - Aucun index mouvement -> zones : le coordinateur construit le sien. `Junction.Boundary` (`RoadModelRecords.cs:440-454`) sert a la liberation non localisee.
- `App/Scenes/MVP_Run/MVP_Run.road-model.json` -- 9 carrefours (1 croix, 4 T, 4 giratoires), 72 mouvements, 40 controles `Uncontrolled`, 136 zones a 2 membres.
  - 100 croisements, 36 convergences (meme corridor de depart), 0 divergence.
  - `ConflictZones[23]` = `4258af5419bba1365a3f0ad6ed3d44aa` (`40ca7f10…` FromEast tout droit, `4e437f94…` FromSouth gauche, depart commun `40e937a9…`).
  - Corridors de depart : 13,5-16 m en croix et T ; 0,87-13,5 m en giratoire.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs:90-167` -- `Run` : frame, puis pas par `TrafficId` (`:156-161`), echec de frame `:144-148`. Y inserer l'instantane d'entree et la resolution apres les pas.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` :
  - `:572` `Step(frameId, frame, hazardQuery, collector, share)` : ajouter l'instantane ;
  - `:610-616` `PlanningSpine.Evaluate` ;
  - `:648-654` perception sur `decision.PerceptionPath` ;
  - `:683-689` `LongitudinalPerception.From` / `FrontDistanceMeters` puis `LongitudinalArbitration.Decide` : y joindre la demande et l'entree ;
  - `:742` `BlockerTracker.Update` ;
  - `:747-752` projection et trace ;
  - `:862-885` episodes de contact.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs` :
  - `:13-25` `LongitudinalCandidateKind`, dont l'ordre des valeurs vaut ordre d'egalite : passer a un rang explicite sans renumeroter ;
  - `:60-101` StopHold ;
  - `:132-215` `LongitudinalObstacle`, `LongitudinalPerception.From`, `FrontDistanceMeters` (distance d'horizon du pare-chocs avant) ;
  - `:411` `Decide`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/SpeedPlan.cs:7` -- `SpeedConstraint`, valeur `JunctionEntry` par ajout.
- `Assets/RoadRage/Features/Vehicles/Traffic/Blockers/Blocker.cs:7-20`, `BlockerTracker.cs:27` -- genres et sources, par ajout.
- `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs` -- lecture seule :
  - `:187` `TryGetActor` ;
  - `:202` `GetOccupants` (tries par `SMin`, `TrafficId`) ;
  - `:208` `TryGetOccupancy` (`ElementOccupant` : element, `SMin`, `SMax`).
- `Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs:566-594` -- canal `Exit` : premier mouvement de l'horizon, corridor de depart seul. Insuffisant en giratoire, d'ou la longueur cumulee le long de la route.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs:46-74` -- `TrafficV2Settings` (constantes).
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:46,223` -- `TrafficLongitudinalOutcome`, `WithLongitudinal` : patron d'ajout.
- `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs:173,499` -- `FixedUpdate` et `v2Runner.Step` : ordre retrait -> pas -> insertion inchange.
- `Assets/RoadRage/Tests/PlayMode/Story533Harness.cs:315-363` -- D12 : `PreJunctionConflict`, `JunctionContactWindowSteps`, `JunctionConflictBefore`.
  - `Story533ExplorationPlayModeTests.cs:150-160,265-266` : exemption et assertion.
  - `Story533PerformanceDiagnosticPlayModeTests.cs:78` : arret sur D12.
- `_bmad-output/implementation-artifacts/traffic-v2-5-33-explorations/scenarios-5-33.json` -- `explore-8` :
  - insertion #1 via `40ca7f10`, insertion #8 via `4e437f94` ;
  - obstacle `sortie` sur `4db6ebc1…` (approche de `40ca7f10`), pas 0-1500.
  - `campaign-final-20261003.md` constat 3 : contact au pas 1846, StopHold mutuel de 1866 a 7499.
- `_bmad-output/implementation-artifacts/deferred-work.md:519-520` (interblocage, partie 5.34) et `:532` (saturation du collecteur non prouvee en PlayMode, reouverte par toute modification du runner).
- Patrons de test :
  - `Tests/EditMode/Story533SharedFrameTests.cs` (frames et modeles `MVP_Run`) ;
  - `Story533LongitudinalTests.cs:1337-1360` (scan `Planning`/`Blockers` : y ajouter `Junction`) ;
  - `Tests/PlayMode/Story533FollowingPlayModeTests.cs` (session, obstacle cinematique, joueur stationne).

## Tasks & Acceptance

**Execution** -- quatre phases, chacune close par un checkpoint vert, sinon HALT.

*Phase 0 -- prealables*
- [ ] `_bmad-output/planning-artifacts/epics.md` -- amender la Story 5.34 (index, fiche, table des revues) : branchement runtime, contrainte d'arret avant le mouvement, PlayMode au niveau story (scenarios C et D, retrait de D12), verification `Both`. Citer O1. La 5.35 garde stop/yield/priorite/anneau et la Gate C.

*Phase 1 -- coordinateur pur (EditMode)*
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionConflictIndex.cs` (nouveau) -- incompatibilites depuis les `ConflictZone`, zone en cause par paire.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs` (nouveau) -- demande, grant, statut, raison stable, instantane immuable a `EffectiveFrame`, compteurs.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs` (nouveau) -- etat, anciennete, lot deterministe, sortie, engagement, liberation et cycle de vie.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRequestBuilder.cs` (nouveau, pur) -- demande valide, `d`, tete de file, engagement, elements de sortie le long de la route.
- [ ] `Assets/RoadRage/Tests/EditMode/Story534JunctionCoordinatorTests.cs` `[Core][Story534]` -- la matrice, plus :
  - sur chaque paire de mouvements de la croix et des 4 T de `MVP_Run` : incompatibles jamais tenus ensemble, compatibles jamais refuses pour conflit ;
  - matrice de determinisme a ordres melanges (demandes et acteurs) ;
  - `RequestSinceFrame` conserve sur demande continue, remis sur interruption ou changement de mouvement ;
  - equite : simulation multi-lots ou des vehicules a faible id arrivent en continu sur un mouvement incompatible : un demandeur continu a id fort est servi en un nombre borne de lots apres la liberation de ses titulaires ; aucun faible id servi avant un plus ancien incompatible ;
  - cycle de vie : sortie, despawn, disparition, demande annulee avant engagement, repli dans le mouvement ;
  - invariant par lot : chaque grant a un acteur present, demandeur ou occupant.
- Checkpoint 1 : `Story534` EditMode vert.

*Phase 2 -- branchement runtime*
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs`, `SpeedPlan.cs` -- candidat `JunctionEntry` et entree optionnelle, rang explicite, cause StopHold, reprise D5 ; sans entree, 5.33 au bit pres.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Blockers/Blocker.cs`, `BlockerTracker.cs` -- `JunctionGrant`, `BlockedExit`, source `JunctionCoordination`.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs` -- coordinateur, instantane par pas, resolution apres les pas, echec de frame, diagnostics.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- lecture de l'instantane verifie, demande, entree d'arbitrage, champs de trace.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` -- deux constantes.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- partie carrefour par ajout.
- [ ] `Assets/RoadRage/Tests/EditMode/Story534JunctionEntryTests.cs` `[Core][Story534]` -- couvrir :
  - demande valide et non-demande ;
  - engagement ;
  - `JunctionEntry` : equilibre a s0, fini a `d ≤ 0`, rang d'egalite, StopHold et liberation au grant, reprise lissee ;
  - carrefour libre : aucun pas ou `JunctionEntry` lie sur les 11 routes `campaign-5-31.json` pour un vehicule seul, sinon HALT ;
  - instantane decale ignore ;
  - blockers ;
  - `ToText()` ;
  - scan de `Junction/` : liste 5.30, plus `Physics.` et les noms de pedale.
- Checkpoint 2 (non-regression) :
  - `Story531` et `Story533` EditMode ;
  - puis `Story531`, `Story552` et `Story533` PlayMode, sans modification de leurs assertions.

*Phase 3 -- scenarios cibles, phase 4 -- retrait de D12*
- [ ] `_bmad-output/implementation-artifacts/traffic-v2-5-34-explorations/scenarios-5-34.json` (nouveau) -- C et D, construits de facon deterministe par un constructeur EditMode, comme en 5.33.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story534JunctionPlayModeTests.cs` `[Story534]` :
  - scenario C ;
  - scenario D ;
  - test de saturation du collecteur : `new TrafficV2StepRunner(new TrafficV2HazardCollector(3, rayon))`, deux vehicules, `HazardCollectorSaturated` pour le seul vehicule sature. Reouverture `deferred-work.md:532`, declenchee par la modification du runner.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story533Harness.cs`, `Story533ExplorationPlayModeTests.cs`, `Story533PerformanceDiagnosticPlayModeTests.cs` -- retirer D12. Ajouter les invariants par pas :
  - aucun grant effectif incompatible simultane ;
  - aucun contact V2-V2 sur deux mouvements d'une meme zone ;
  - `EnteredWithoutGrant` = 0.
  Les records de coordination entrent dans les traces.
- [ ] `_bmad-output/implementation-artifacts/deferred-work.md`, `sprint-status.yaml` (5.34 seulement) -- solder la partie 5.34 de `:519` (la recuperation reste a la 5.39) et l'entree `:532` ; consigner les constats. Puis `graphify update .`.

**Acceptance Criteria:**

- Given le scenario C (reproduction de la rencontre `explore-8` : deux vehicules sur les routes des insertions #1 et #8, obstacle `sortie`), when il s'execute, then les records publies montrent, dans l'ordre :
  1. deux demandes actives au meme lot sur `40ca7f10` et `4e437f94` (precondition ; absente = run invalide, jamais reussi) ;
  2. un seul `Granted`, au plus ancien (ou au plus petit `TrafficId` a egalite) ;
  3. l'autre `Denied(ConflictGranted)` citant le titulaire et `4258af54`, arrete avant l'entree (pare-chocs avant jamais au-dela), blocker `JunctionGrant` legitime ;
  4. `Released(Cleared)` du premier ;
  5. `Granted` du second au meme lot ou au suivant ;
  6. son entree puis sa sortie.
  Et a aucun pas deux grants incompatibles effectifs, aucun contact entre vehicules, aucun `TrackingToleranceExceeded`, aucun repli hors `ExitPortalReached`, `EnteredWithoutGrant` = 0, les deux portails de sortie atteints.
- Given le scenario D (un premier vehicule passe par `40ca7f10`, puis est tenu sur `40e937a9` par un obstacle cinematique place par le test, entierement hors du mouvement, en laissant une longueur libre < L + s0), when un second demande `4e437f94`, then :
  - il est refuse `ExitBlocked`, raison de sortie evaluee avant tout conflit ;
  - il s'arrete avant l'entree avec un blocker `BlockedExit`, et n'entre jamais tant que la sortie est insuffisante ;
  - apres retrait de l'obstacle, il est servi et les deux sortent, sans contact ni `TrackingToleranceExceeded`.
- Given `Story531`, `Story552`, `Story533` A et B rejoues sans modification de leurs assertions, when ils s'executent, then ils restent verts.
- Given la campagne `Story533Exploration` sans D12, when elle s'execute, then tout verrou 2a nominal est un echec et les invariants de coordination sont verts. Les constats sont publies, dont la sequence ZC23 si elle est contestee et l'absence de StopHold mutuel a jeu negatif.
- Given toute fixture `Story534`, when la suite tourne, then chaque grant, refus, revocation et liberation porte une raison stable assertable.

## Spec Change Log

## Design Notes

**Decisions proprietaires du 2026-10-03.**
- **O1 Perimetre B** : fondation et premier branchement runtime ; epics.md amende. Inclus :
  - coordination generique des mouvements incompatibles ;
  - demandes et grants ;
  - arret avant le mouvement sans grant ;
  - maintien et liberation ;
  - scenarios PlayMode cibles, dont ZC23 ;
  - non-regression 5.31, 5.52 et 5.33.
  Exclu : stop, cedez-le-passage, priorite routiere, priorite d'anneau, signaux, Gate C, recuperation 5.39.
- **O2 Departage** : plus ancienne demande, puis `TrafficId`, avec une anciennete stable sur demande continue. Les tests prouvent qu'un demandeur continu est servi et qu'un faible id ne monopolise pas le passage.
- **O3** : la 5.33 reste en `review` dans `sprint-status.yaml` ; sa promotion releve du checkpoint d'Epic.
- **O4** : les 9 carrefours, giratoires compris, passent par le coordinateur generique. Pas de retrait d'un grant engage. Un vehicule en repli garde son grant tant qu'il occupe le mouvement. Conditions de liberation explicites ; aucun grant sans occupant valide.
- **O5** : le scenario C demontre la sequence complete, pas seulement l'absence de collision.

**Choix de conception proposes (contestables au checkpoint).**
- *Reservation de l'ancien.* Sans elle, le FIFO seul ne garantit pas O2. Un jeune compatible avec le titulaire, mais pas avec l'ancien, passerait et repousserait l'ancien indefiniment. Avec elle :
  - l'ensemble des grants incompatibles avec la plus ancienne demande refusee pour conflit ne peut que decroitre ;
  - elle est servie des que ses titulaires liberent, ce qui est borne hors interblocage (5.40).
  Une demande refusee pour sortie ne reserve rien : elle gelerait le carrefour pour une cause exterieure.
- *Tete de file.* Seul le premier vehicule d'une approche demande. Sinon, un suiveur coince derriere son leader tiendrait un grant et bloquerait les mouvements incompatibles.
- *Distances.* Demande a `s*(v) + 2 + 10` m, engagement de la contrainte a `s*(v) + 2` m. A 8 m/s, avec le profil par defaut : `s*` = 32,5 m, demande a 44,5 m, contrainte a 34,5 m. A cette distance, l'IDM freine a environ −1,33 m/s², sous le −2 m/s² de confort. Le grant arrive donc avant que la contrainte ne morde quand le carrefour est libre. Valeurs a confirmer par la mesure.
- *Expiration a un lot.* Un grant vaut pour `N+1` et doit etre republie. Aucune minuterie : sa survie depend a chaque lot d'une demande valide ou d'un occupant reel.
- *Occupation sans grant.* Un acteur V2 present sur un mouvement bloque les mouvements incompatibles sans recevoir de grant. Lui en donner un creerait deux grants incompatibles (invariant 8).

**Limite connue, hors perimetre.** En giratoire, l'anneau est fait de corridors : un vehicule qui circule n'est pas un mouvement et n'entre dans aucune `ConflictZone`. Ceder a l'anneau releve de la 5.35. Les contacts d'anneau restent des constats.

**Hypotheses non verifiees.** Dans les campagnes, la sequence ZC23 de `explore-8` peut ne pas se reproduire : les attentes aux autres carrefours decalent les arrivees, et le PlayMode multi-vehicule n'est pas deterministe au bit pres (`deferred-work.md`). C'est la raison d'etre du scenario dedie C.

## Verification

**Commands** (Domain Reload actif : les runs PlayMode s'enchainent sans redemarrer l'Editeur) :
- `.\scripts\validate.ps1 -Profile Story -Story 5.34 -TestMode Both` -- expected: `VALIDATION STORY`, compte execute = compte attendu, C et D verts, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.33 -TestMode Both`, puis `-Story 5.31 -TestMode Both`, puis `-Story 5.52 -TestMode PlayMode` -- expected: verts, assertions inchangees.
- `.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story533Exploration -TestFilterType category -IncludeExplicit` -- expected: invariants verts sans D12, rapports ecrits.
