---
title: 'Story 5.34 -- Coordination de carrefour : demandes, grants, zones de conflit, sortie bloquee et traversee engagee, avec premier branchement runtime'
type: 'feature'
created: '2026-10-03'
status: 'done'
baseline_commit: '55c205faf04555e8d80be811b1c27bc795ca6497'
review_loop_iteration: 1
context:
  - '{project-root}/_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-33-vehicle-following-through-named-speed-constraints.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Aucun mecanisme ne decide qui peut entrer dans un carrefour. Dans `explore-8` (5.33), deux vehicules V2 sur `40ca7f10` et `4e437f94`, membres de `ConflictZones[23]` (`4258af54`, `TJunction_West`, convergence vers le corridor `40e937a9`), entrent ensemble. Ils se touchent, puis se tiennent mutuellement en StopHold a jeu negatif pendant 113 s. La regle D12 du harnais 5.33 ne fait que classer ce cas comme constat.

**Approach:** Un coordinateur hote unique, generique pour les 9 carrefours.
- Il accorde des **traversees** : la chaine des mouvements consecutifs d'un meme carrefour sur la route.
- Les demandes derivees de la frame N sont resolues en un lot deterministe, publie comme grants effectifs a N+1, apres protection des occupants reels.
- Incompatibilite = co-appartenance a une `ConflictZone` authoree. Sortie verifiee avant tout departage.
- Departage : anciennete (`RequestSinceFrame`), puis `TrafficId`.
- Un vehicule sans grant s'arrete avant la traversee, a des distances derivees de la cinematique et de la latence du pipeline.
- Scenarios PlayMode cibles, puis retrait de D12.

## Boundaries & Constraints

**Always:**

- **Decisions proprietaires du 2026-10-03** (Design Notes O1-O6) : perimetre B, epics.md amende en phase 0, `sprint-status.yaml` de la 5.33 laisse en `review`.
- **Coordinateur** (`Junction/`, pur : ni `Physics.*`, ni `UnityEngine.Object`, ni membre public nomme `Brake`, `Throttle` ou `Pedal`). Il possede seul l'etat mutable de coordination : grants anterieurs et anciennete des demandes. Il ne choisit pas de route, ne genere pas de chemin et ne commande aucun vehicule.
- **Traversee.** A partir du premier `JunctionMovement` de la route restante a ou devant le pare-chocs avant :
  - elle enchaine les mouvements consecutifs du meme `JunctionId`, avec les corridors qui les separent ;
  - elle s'arrete au premier corridor que la route quitte sans entrer dans un autre mouvement de ce carrefour (corridor de sortie de la traversee) ;
  - en croix et en T : un mouvement ; en giratoire : entree, continuations eventuelles, sortie.
  - Le grant porte sur tous les mouvements de la traversee, accordes ensemble ou refuses ensemble. Il n'y a jamais d'attente a l'interieur d'un carrefour.
- **Incompatibilite.** Deux mouvements distincts sont incompatibles si et seulement s'ils appartiennent a une meme `ConflictZone` compilee. Deux traversees sont incompatibles si un mouvement de l'une l'est avec un mouvement de l'autre.
  - `JunctionConflictIndex` est construit une seule fois par modele : mouvement -> ensemble trie des mouvements incompatibles, zone en cause par paire.
  - Aucune inference geometrique. Un mouvement est compatible avec lui-meme.
- **Lot** (frame N), dans cet ordre :
  0. **Protection des occupants**, depuis l'occupation structuree de la frame et independamment de la memoire du coordinateur. Chaque acteur V2 dont l'occupation chevauche un mouvement est un occupant protege de ce mouvement :
     - s'il a un grant, celui-ci est conserve ;
     - sinon, il recoit `Held(Restored)` si sa traversee est compatible avec tous les autres occupants et grants ;
     - sinon il est compte `IncompatibleOccupancy`, n'est jamais servi, mais bloque les mouvements incompatibles.
  1. Grants anterieurs non couverts par l'etape 0, par `TrafficId` :
     - acteur absent -> `Revoked(ActorGone)` ;
     - traversee entierement liberee -> `Released(Cleared)` ;
     - non engage et sans demande valide pour la meme traversee -> `Revoked(RequestWithdrawn)` ;
     - non engage et sortie insuffisante -> `Revoked(ExitBlocked)` ;
     - sinon `Held(Committed)` ou `Held(Pending)`.
     - Un mouvement deja libere d'une traversee engagee sort du grant (`Released(MovementCleared)`) ; les autres restent tenus.
  2. Nouvelles demandes, triees par (`RequestSinceFrame`, `TrafficId`). Pour chacune, dans cet ordre :
     - **sortie** : sinon `Denied(ExitBlocked)` ;
     - **occupant protege** incompatible : sinon `Denied(ConflictOccupied, occupant, zone)` ;
     - **grant incompatible** (tenu ou emis plus tot dans le lot) : sinon `Denied(ConflictGranted, titulaire, zone)` ;
     - **demande plus ancienne refusee pour conflit** et incompatible : sinon `Denied(SeniorRequestPending, demandeur)` ;
     - sinon `Granted`.
  3. Publication d'un instantane immuable `EffectiveFrame = N+1`. Chaque record porte : `TrafficId`, mouvements, `RequestSinceFrame`, `SourceFrame = N`, `EffectiveFrame`, `ExpiresAfterFrame = N+1`, statut, raison stable, titulaire, occupant ou zone en cause. Un grant non republie expire.
- **Frame invalide, fail-closed.** Si `new TrafficFrame` echoue au pas N :
  - le lot N n'examine aucune demande et n'emet aucun grant ;
  - il republie a l'identique les grants engages connus au dernier lot valide, `Held(CommittedCarried)`. Les grants non engages sont `Revoked(FrameUnavailable)` ; leurs titulaires sont en repli `FrameUnavailable` et peuvent s'arreter, par definition de l'engagement ;
  - au premier lot valide suivant, l'etape 0 reconstruit la protection depuis l'occupation reelle avant toute nouvelle demande.
  - Aucun mouvement incompatible avec un occupant engage ne peut donc etre servi entre-temps.
- **Anciennete (O2).** `RequestSinceFrame` = premiere frame d'une suite ininterrompue de demandes valides du meme vehicule pour la meme traversee (meme premier mouvement). Elle n'est remise a zero que par une frame valide sans demande ou par un changement de traversee. Une frame invalide n'interrompt pas l'anciennete. `TrafficId` ne sert qu'au departage a anciennete egale.
- **Distances, derivees** (fonctions pures, aucune constante calibree a l'oeil). Pour un vehicule de vitesse v et de profil (v0, T, s0, a, b), au pas physique Δt :
  - latence de decision `t_lat = Δt_lot + Δt` ; un lot par pas physique, donc `Δt_lot = Δt` et `t_lat = 2Δt`. Elle couvre le lot N -> N+1 et un pas d'actuation : la frame N decrit l'etat apres la physique N−1, et l'intent issu d'un refus lu a N+1 agit pendant la physique N+1 ;
  - vitesse de reference conservative `v_ref = v + a·t_lat` (acceleration maximale du profil pendant la latence) ;
  - deceleration de planification `b_plan = 0,99·b` (convention D14) ;
  - marge de controle longitudinal `m_ctrl` = `JunctionStopControlMarginMeters`. C'est la seule valeur declaree : elle couvre la montee du frein et l'ecart de suivi longitudinal, absents du modele point-masse. 0,5 m, declaree avant mesure ;
  - **arret garanti** `D_stop(v) = v_ref·t_lat + v_ref²/(2·b_plan) + m_ctrl` ;
  - **engagement de la contrainte** `D_engage(v) = max(D_stop(v), s*(v_ref)) + v_ref·t_lat`, avec `s*(v) = s0 + v·T + v²/(2·√(a·b))` ;
  - **demande** `D_request(v) = D_engage(v) + v_ref·t_lat`.
  - Pour d'autres profils ou un autre Δt, elles se recalculent ; rien n'est code en metres.
- **Demande valide** (une au plus par vehicule et par frame, fonction pure de la frame et de l'horizon de la decision) :
  - premiere traversee de la route restante a ou devant le pare-chocs avant ;
  - vehicule localise, hors repli ;
  - `d` (pare-chocs avant -> entree du premier mouvement) ≤ `D_request(v)` ;
  - **tete de file**, definie depuis l'occupation structuree de la frame : aucune occupation d'un autre acteur V2 (`GetOccupants`, intervalle [`SMin`, `SMax`]) ne coupe la route du demandeur entre son pare-chocs avant et l'entree. C'est vrai quel que soit le prochain mouvement de cet acteur : un leader qui masque l'entree empeche toujours la demande.
  - Ni un blocker, ni le signe d'une acceleration, ni la vitesse n'entrent dans la definition.
- **Engagement.** Un vehicule est engage pour sa traversee s'il tient un grant effectif et que, au choix :
  - son pare-chocs avant a franchi l'entree du premier mouvement ;
  - `d < D_stop(v)`.
  Un grant engage n'est jamais revoque ; il ne sort que par `Released` ou `ActorGone`. Un refus ou une revocation n'est donc possible que si `d ≥ D_stop(v)`.
- **Liberation (O4).** Un mouvement d'une traversee est libere quand l'occupation de son titulaire ne le chevauche plus : localise au-dela, avec `SMin ≥ 0` sur l'element suivant. Si le titulaire n'est pas localise, quand les coins de son empreinte ne sont plus dans la `Boundary` du carrefour (`Released(ClearedUnlocalized)`). La traversee est liberee quand tous ses mouvements le sont. Un titulaire en repli qui occupe encore un mouvement garde son grant (etape 0). Aucun grant ne survit sans demande valide ou sans occupant reel.
- **Sortie** (avant tout departage), avec une borne semantique.
  - La recherche part de la fin du dernier mouvement de la traversee et suit la route du demandeur, corridor par corridor.
  - Elle s'arrete au premier de ces evenements :
    1. longueur libre cumulee ≥ `L + s0` -> suffisante (`L` : longueur de l'empreinte declaree, 4,44 m ; s0 du profil) ;
    2. `SMin` du premier occupant V2 -> longueur libre = distance cumulee jusqu'a lui ;
    3. entree d'un mouvement d'un autre carrefour -> longueur libre = distance cumulee : la recherche ne traverse jamais un carrefour futur (`ExitSearchBound`) ;
    4. abscisse effective du portail de sortie de la route -> suffisante, car le portail retire les vehicules.
  - Les corridors internes a la traversee, dont les corridors d'anneau de 0,87 m et 5,58 m, ne sont jamais la sortie.
  - Les grants deja emis vers le meme corridor de sortie, dont le titulaire n'y est pas encore, retranchent chacun `L + s0`.
  - Les dangers non V2 ne comptent pas.
- **Branchement runtime.**
  - `TrafficV2StepRunner` possede le coordinateur. Par pas hote : frame N -> pas des vehicules avec l'instantane `EffectiveFrame == N` -> resolution des demandes de N -> instantane N+1.
  - Un instantane dont `EffectiveFrame ≠ N` vaut « aucun grant ». Aucun vehicule ne lit un grant non publie.
  - Le coordinateur tourne aussi en production (population 1).
- **Contrainte `JunctionEntry`** (arbitrage 5.33, par ajout).
  - Active si la traversee n'a pas de grant effectif, que le vehicule n'y est pas entre et que `d ≤ D_engage(v)`.
  - Candidat = min(IDM vers un obstacle fixe virtuel a l'entree, `a_kin`), avec `a_kin = −v²/(2·max(d − m_ctrl, ε))`. L'IDM donne le confort et un equilibre a s0 ; `a_kin` rend la garantie d'arret structurelle.
  - Rang d'egalite entre `StopHold` et `Obstacle` ; ordre relatif 5.33 inchange.
  - Cause de StopHold D11, liberee quand le grant devient effectif. Une perception indisponible ne libere jamais.
  - Reprise lissee D5 comme `LeaderFollowing` et `Obstacle`.
  - Sans contrainte active, l'arbitrage est celui de la 5.33 au bit pres.
- **Blockers** (par ajout) :
  - `JunctionGrant` : refus pour conflit, titulaire ou occupant en cause ;
  - `BlockedExit` : refus pour sortie insuffisante ;
  - source `JunctionCoordination`, legitimes, liberation attendue, non recouvrables. Regle 5.33 : seulement comme cause reelle d'immobilisation.
- **Performance (D13).**
  - Index, chaines de traversee et voisinage de sortie sont precalcules par modele. Aucun parcours des 72 mouvements ou des 136 zones par vehicule et par pas : le cout par lot ne depend que des demandes, des grants tenus et des occupants du carrefour concerne. Compteurs dans `TrafficV2WorkCounters`.
  - Cout du coordinateur publie a part (`CoordinatorMilliseconds`) a N = 2, 4 et 8.
  - Budget : pas hote Traffic V2 a N = 8 de p95 < 10 ms (cible D13), et au plus 2 FixedUpdate par frame sur explore-4 a N = 4.
- **Verdicts de campagne (O6, frontiere 5.34 / 5.35).** Chaque contact V2-V2 est classe a son premier pas depuis les elements des deux vehicules :
  - **Echec 5.34** : deux mouvements distincts d'une meme `ConflictZone`, giratoires compris (16 fusions entree / continuation y sont authorees) ; ou, a n'importe quel pas, deux grants incompatibles effectifs, ou `EnteredWithoutGrant > 0`.
  - **Constat 5.35, publie et non bloquant, sans contact** : l'ordre de service a une fusion d'anneau (entree servie avant une continuation, ou l'inverse) differe de « l'anneau d'abord ». En 5.34, cet ordre est le FIFO generique, jamais une priorite d'anneau.
  - **Tout autre contact, et tout verrou 2a nominal** : echec, comme en 5.33. Aucune exemption de fenetre : D12 n'a pas de successeur.
- **Projection et trace**, par ajout, `ToText()` invariant de culture :
  - demande : traversee, `d`, `D_stop`, `D_engage`, `D_request`, anciennete, tete de file, engagement ;
  - statut, raison, titulaire, occupant ou zone ;
  - compteurs du lot, dont `EnteredWithoutGrant` et `IncompatibleOccupancy`.
  - Hote seul : aucune `NetworkVariable`, RPC ni `OnValueChanged`.
- **Controles de `MVP_Run` inchanges** : 40 `Uncontrolled`.

**Ask First:**

- Revision de `JunctionStopControlMarginMeters`, de `t_lat`, de `b_plan` ou de la borne de sortie. Toute hysteresis, temporisation ou expiration de grant au-dela d'un lot.
- Toute modification de semantique 5.33 de l'arbitrage, du StopHold ou des blockers autre que l'ajout de `JunctionEntry`, `JunctionGrant` et `BlockedExit`.
- Le modele, la scene, la geometrie, les controles, `Traffic/Migration/**`, Gate A ; tout type public 5.25–5.33 modifie autrement que par ajout optionnel.
- Non-regression rouge (`Story531`, `Story552`, `Story533` A et B) : HALT, sans adapter de seuil ni de test.
- Budget D13 depasse (p95 a N = 8, ou FixedUpdate par frame) : HALT et arbitrage proprietaire, jamais de micro-optimisation silencieuse.
- Precondition du scenario C inatteignable sans retoucher les contraintes : HALT, ne rien regler pour la provoquer.
- `EnteredWithoutGrant > 0`, `IncompatibleOccupancy > 0`, ou un contact ou verrou de la categorie « tout autre » : HALT et analyse. Une relation de carrefour absente du modele se tranche par decision proprietaire, jamais par une exemption de test.
- Faire compter les dangers non V2 dans la sortie ; population > 8 ; `NetworkVariable`, RPC, asmdef, package.

**Never:**

- Stop, cedez-le-passage, priorite routiere, priorite d'anneau (y compris un ordre « anneau d'abord » code en dur), ligne d'arret, signaux (5.35, 5.36). Gate C. Recuperation, y compris d'un StopHold a jeu negatif (5.39). Escalade d'interblocage (5.40). Exceptions de regle (5.41).
- Une negociation entre deux vehicules comme autorite. Une inference d'incompatibilite ou de priorite depuis la geometrie, un nom, un cycle ou l'ordre de mise a jour.
- Un grant partiel d'une traversee. Une attente a l'interieur d'un carrefour imposee par le coordinateur.
- Un coordinateur qui ecrit frein ou gaz, ou un second composeur. Agir sur un grant dans la frame qui l'emet. Une liberation par minuterie. Un nouveau grant emis depuis une frame invalide.
- Une exemption de campagne qui succede a D12.
- Retirer, teleporter ou reinserer un vehicule ailleurs qu'a un portail de sortie.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Deux demandes incompatibles | Lot N, ZC23 | Un `Granted` effectif a N+1, l'autre `Denied(ConflictGranted)` | N/A |
| Ordre melange | Memes demandes et acteurs permutes | Instantane identique | N/A |
| Anciennete egale | Meme `RequestSinceFrame` | `TrafficId` le plus petit servi | N/A |
| Plus ancien a id fort | Demandeur id 8 plus ancien qu'id 1 | Id 8 servi d'abord | N/A |
| Flux de jeunes compatibles avec le titulaire mais pas avec l'ancien | Ancien refuse pour conflit | Jeunes `SeniorRequestPending` ; ancien servi a la liberation | N/A |
| Mouvements compatibles | Pas de zone commune, meme approche, ou meme mouvement | Grants simultanes | N/A |
| Sortie et conflit | Sortie pleine et zone tenue | `Denied(ExitBlocked)`, evalue d'abord | N/A |
| Entree de giratoire sur l'anneau de 0,87 m | `43605e56` (`Roundabout_SouthWest`) -> anneau 0,87 m -> mouvement suivant | Traversee de plusieurs mouvements, sortie mesuree apres le dernier, jamais sur 0,87 m | N/A |
| Sortie qui atteint un autre carrefour | Modele mute : corridor de sortie < `L + s0` avant le mouvement suivant | Recherche arretee, `Denied(ExitBlocked)` avec `ExitSearchBound` | Diagnostic |
| Traversee partiellement incompatible | Continuation en conflit avec un grant tenu | Traversee entiere refusee | N/A |
| Demande interrompue | Une frame valide sans demande | Anciennete remise a la reprise ; grant non engage `RequestWithdrawn` | N/A |
| Refus au dernier instant permis | Revocation ou refus a `d = D_stop(v)`, latence `t_lat` | Pare-chocs avant jamais au-dela de l'entree | N/A |
| Engage | `d < D_stop(v)` ou entre | `Held(Committed)` malgre sortie ou conflit nouveaux | N/A |
| Repli dans le mouvement | Verrou 2a, occupe un mouvement | Grant tenu ; libere des que l'empreinte quitte la frontiere | N/A |
| Frame refusee puis valide | Occupant engage sur M1, demande incompatible sur M2 | Lot invalide : M1 republie, aucune demande examinee ; lot valide : M1 protege d'abord, M2 refuse | Compteur `FrameFailures` |
| Memoire perdue | Coordinateur neuf, occupant reel sur M1 | `Held(Restored)` ou blocage ; M2 incompatible refuse | N/A |
| Despawn ou disparition | Acteur absent de la frame | `Revoked(ActorGone)` | N/A |
| Entre sans grant | Occupe un mouvement, aucun grant | Protege a l'etape 0, conflits refuses | Compteur `EnteredWithoutGrant` |
| Leader qui masque l'entree | Occupation d'un autre V2 entre le demandeur et l'entree, autre prochain mouvement | Aucune demande ; `JunctionEntry` peut s'appliquer | N/A |
| Instantane decale | `EffectiveFrame ≠ N` | Lu comme aucun grant | Diagnostic |
| Carrefour libre, vehicule seul | Demande a `D_request` | Grant avant `D_engage` ; arbitrage 5.33 au bit pres | N/A |
| Mouvement inconnu | Id absent du modele | `Denied(InvalidRequest)` | Diagnostic |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs` -- lecture seule :
  - `:113-142` `CompiledJunctionMovement` (`FromCorridorId`, `ToCorridorId`, `Curve`, `LengthMeters`) ;
  - `:166-180` `CompiledConflictZone` (`MemberMovementIds`, `Volume`) ;
  - `:462` `ConflictZones`, `:510` `TryGetMovement`, `:570` `GetConflictZonesInJunction`.
  - Aucun index mouvement -> zones : le coordinateur construit le sien, une fois par modele. `Junction.Boundary` (`RoadModelRecords.cs:440-454`) sert a la liberation non localisee.
- `App/Scenes/MVP_Run/MVP_Run.road-model.json` -- 9 carrefours (1 croix, 4 T, 4 giratoires), 72 mouvements, 40 controles `Uncontrolled`, 136 zones a 2 membres, 0 `LaneConnection`.
  - Topologie verifiee le 2026-10-03 : en croix et en T (36 mouvements), aucun mouvement n'est suivi d'un mouvement du meme carrefour. En giratoire, les 24 mouvements d'entree et de continuation (`Ring_Split -> Ring_Merge`) le sont tous ; les 12 sorties, non.
  - Zones de giratoire : 16 entree/continuation (fusions), 12 entree/sortie, 4 continuation/sortie, 4 sortie/sortie, 4 entree/entree.
  - Corridors de sortie de traversee : 13,5-16 m en croix et T, 9,5-13,5 m en giratoire. Corridors internes d'anneau : 0,87 et 5,58 m.
  - `ConflictZones[23]` = `4258af5419bba1365a3f0ad6ed3d44aa` (`40ca7f10…` FromEast tout droit, `4e437f94…` FromSouth gauche, depart commun `40e937a9…`).
  - Cas 0,87 m : `43605e56` (`Roundabout_SouthWest`, entree Ouest) -> `Ring_Merge_West -> Ring_Split_South` (0,87 m) -> `419d893b` (sortie Sud) ou `42e993cf` (continuation).
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs:90-167` -- `Run` : frame, puis pas par `TrafficId` (`:156-161`), echec de frame `:144-148`. Y inserer l'instantane d'entree, la resolution apres les pas et `CoordinatorMilliseconds` dans `TrafficV2StepCost`.
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
- `Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs:566-594` -- canal `Exit` : premier mouvement de l'horizon, corridor de depart seul. Insuffisant en giratoire, d'ou la traversee et la recherche bornee.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs:46-74` -- `TrafficV2Settings` (constantes).
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:46,223` -- `TrafficLongitudinalOutcome`, `WithLongitudinal` : patron d'ajout. `TrafficV2WorkCounters.cs` : compteurs.
- `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs:173,499` -- `FixedUpdate` et `v2Runner.Step` : ordre retrait -> pas -> insertion inchange.
- `Assets/RoadRage/Tests/PlayMode/Story533Harness.cs:315-363` -- D12 : `PreJunctionConflict`, `JunctionContactWindowSteps`, `JunctionConflictBefore`.
  - `Story533ExplorationPlayModeTests.cs:150-160,265-266` : exemption et assertion.
  - `Story533PerformanceDiagnosticPlayModeTests.cs:31` : `[Category("Story533Perf")]`, explore-4 tronque a N = 1..4. Il publie moyenne, mediane, p95 et maximum du pas hote, et les FixedUpdate par frame, sans assertion de budget. `:78` : arret sur D12.
- `_bmad-output/implementation-artifacts/traffic-v2-5-33-explorations/scenarios-5-33.json` -- `explore-8` :
  - insertion #1 via `40ca7f10`, insertion #8 via `4e437f94` ;
  - obstacle `sortie` sur `4db6ebc1…` (approche de `40ca7f10`), pas 0-1500.
  - `campaign-final-20261003.md` constat 3 : contact au pas 1846, StopHold mutuel de 1866 a 7499. `results-20261003-structural-optimization.md` : couts N = 2..8 de reference.
- `_bmad-output/implementation-artifacts/deferred-work.md:519-520` (interblocage, partie 5.34) et `:532` (saturation du collecteur non prouvee en PlayMode, reouverte par toute modification du runner).
- Patrons de test :
  - `Tests/EditMode/Story533SharedFrameTests.cs` (frames et modeles `MVP_Run`) ;
  - `Story533LongitudinalTests.cs` (simulation point-masse ; scan `Planning`/`Blockers` `:1337-1360`, a etendre a `Junction`) ;
  - `Tests/PlayMode/Story533FollowingPlayModeTests.cs` (session, obstacle cinematique, joueur stationne).

## Tasks & Acceptance

**Execution** -- quatre phases, chacune close par un checkpoint vert, sinon HALT.

*Phase 0 -- prealables*
- [x] `_bmad-output/planning-artifacts/epics.md` -- amender la Story 5.34 (index, fiche, table des revues) : grants de traversee, branchement runtime, arret avant la traversee, PlayMode au niveau story (scenarios C et D, retrait de D12), verification `Both`. Citer O1 et O6. La 5.35 garde stop/yield/priorite/priorite d'anneau et la Gate C.

*Phase 1 -- coordinateur pur (EditMode)*
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionConflictIndex.cs` (nouveau) -- incompatibilites precalculees, zone en cause par paire, chaines de traversee par modele.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs` (nouveau) -- demande, grant de traversee, statut, raison stable, instantane immuable a `EffectiveFrame`, compteurs.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs` (nouveau) -- protection des occupants, etat, anciennete, lot deterministe, sortie bornee, engagement, liberation, cycle de vie, lot fail-closed.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionDistances.cs` (nouveau, pur) -- `t_lat`, `v_ref`, `b_plan`, `D_stop`, `D_engage`, `D_request`, `a_kin`.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRequestBuilder.cs` (nouveau, pur) -- traversee, `d`, tete de file par occupation structuree, engagement, recherche de sortie bornee.
- [x] `Assets/RoadRage/Tests/EditMode/Story534JunctionCoordinatorTests.cs` `[Core][Story534]` -- la matrice, plus :
  - sur chaque paire de traversees de la croix et des 4 T de `MVP_Run` : incompatibles jamais tenues ensemble, compatibles jamais refusees pour conflit ;
  - fait de modele : chaines de longueur 1 en croix et en T, ≥ 2 depuis toute entree de giratoire ;
  - cas 0,87 m sur `43605e56` ;
  - borne `ExitSearchBound` sur modele mute ;
  - matrice de determinisme a ordres melanges (demandes et acteurs) ;
  - `RequestSinceFrame` conserve sur demande continue et a travers une frame invalide, remis sur interruption valide ou changement de traversee ;
  - equite : simulation multi-lots ou des vehicules a faible id arrivent en continu sur une traversee incompatible : un demandeur continu a id fort est servi en un nombre borne de lots apres la liberation de ses titulaires ; aucun faible id servi avant un plus ancien incompatible ;
  - cycle de vie : sortie, despawn, disparition, demande annulee avant engagement, repli dans un mouvement ;
  - **frame refusee** : engage -> frame refusee -> frame valide. Aucun mouvement incompatible servi dans les trois instantanes. Meme preuve avec un coordinateur neuf au lot valide (protection par occupation seule) ;
  - invariant par lot : chaque grant a un acteur present, demandeur ou occupant ;
  - performance structurelle : index construit une fois par modele (compteur) ; travail par lot independant du nombre de mouvements et de zones du modele.
- Checkpoint 1 : `Story534` EditMode vert.

*Phase 2 -- branchement runtime*
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs`, `SpeedPlan.cs` -- candidat `JunctionEntry` = min(IDM, `a_kin`) et entree optionnelle, rang explicite, cause StopHold, reprise D5 ; sans entree, 5.33 au bit pres.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Blockers/Blocker.cs`, `BlockerTracker.cs` -- `JunctionGrant`, `BlockedExit`, source `JunctionCoordination`.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs` -- coordinateur, instantane par pas, resolution apres les pas, lot fail-closed, `CoordinatorMilliseconds`, diagnostics.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- lecture de l'instantane verifie, demande, entree d'arbitrage, champs de trace.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` -- `JunctionStopControlMarginMeters`.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs`, `TrafficV2WorkCounters.cs` -- partie carrefour et compteurs, par ajout.
- [x] `Assets/RoadRage/Tests/EditMode/Story534JunctionEntryTests.cs` `[Core][Story534]` -- couvrir :
  - demande valide et non-demande, dont le leader qui masque l'entree avec un autre prochain mouvement ;
  - engagement ;
  - distances : valeurs du profil par defaut a 0, 4 et 8 m/s publiees ; monotonie en v ; recalcul pour un autre Δt ;
  - **pire cas** : simulation point-masse avec le modele de latence (mesure a N, publication a N+1, actuation a N+1, acceleration jusqu'a `a` avant reaction, deceleration realisee bornee a `b_plan`). Pour v de 0 a v0 par pas de 0,5 m/s, refus ou revocation a `d = D_stop(v)`, puis refus a la premiere frame ou `d ≤ D_engage(v)` sans grant : pare-chocs avant jamais au-dela de l'entree, distance minimale publiee ;
  - `JunctionEntry` : equilibre a s0, fini a `d ≤ 0`, rang d'egalite, StopHold et liberation au grant, reprise lissee ;
  - carrefour libre : aucun pas ou `JunctionEntry` lie sur les 11 routes `campaign-5-31.json` pour un vehicule seul, sinon HALT ;
  - instantane decale ignore ;
  - blockers ;
  - `ToText()` ;
  - scan de `Junction/` : liste 5.30, plus `Physics.` et les noms de pedale.
- Checkpoint 2 (non-regression et cout) :
  - `Story531` et `Story533` EditMode ;
  - puis `Story531`, `Story552` et `Story533` PlayMode, sans modification de leurs assertions ;
  - puis `Story533Perf` : au plus 2 FixedUpdate par frame a N = 4, cout du coordinateur publie.
  - Rouge ou budget depasse : HALT.

*Phase 3 -- scenarios cibles, phase 4 -- retrait de D12 et campagne*
- [x] `_bmad-output/implementation-artifacts/traffic-v2-5-34-explorations/scenarios-5-34.json` (nouveau) -- C et D, construits de facon deterministe par un constructeur EditMode, comme en 5.33.
- [x] `Assets/RoadRage/Tests/PlayMode/Story534JunctionPlayModeTests.cs` `[Story534]` :
  - scenario C ;
  - scenario D ;
  - test de saturation du collecteur : `new TrafficV2StepRunner(new TrafficV2HazardCollector(3, rayon))`, deux vehicules, `HazardCollectorSaturated` pour le seul vehicule sature. Reouverture `deferred-work.md:532`, declenchee par la modification du runner.
- [x] `Assets/RoadRage/Tests/PlayMode/Story533Harness.cs`, `Story533ExplorationPlayModeTests.cs`, `Story533PerformanceDiagnosticPlayModeTests.cs` -- retirer D12 et appliquer les verdicts O6 :
  - aucun grant effectif incompatible simultane ;
  - classement de chaque contact ;
  - `EnteredWithoutGrant` = 0 ;
  - p95 du pas hote et `CoordinatorMilliseconds` publies par N.
  Les records de coordination entrent dans les traces.
- [x] `_bmad-output/implementation-artifacts/deferred-work.md`, `sprint-status.yaml` (5.34 seulement) -- solder la partie 5.34 de `:519` (la recuperation reste a la 5.39) et l'entree `:532` ; consigner les constats 5.35. Puis `graphify update .`.

### Review Findings — 2026-10-05

Review of `55c205f..812cdae`: all four patch findings fixed, R1 integration proof completed in MVP_Run, required functional regressions and explicit performance/exploration campaigns passed. Story promoted to **done** after official validation. Full rationale, failed attempts, owner arbitration and final evidence: `code-review-5-34-2026-10-05.md`. Approval changes O8–O14 and existing owner-approved deferrals remain authoritative.

- [x] [Review][Patch][high] Scenario C must prove fresh arbitration in the shared request batch, rather than accept an earlier grant as `Held(Pending)` [Assets/RoadRage/Tests/PlayMode/Story534JunctionPlayModeTests.cs:277]. The recorded run grants at 1495 and selects meeting batch 1496. Assert `Granted` and the absence of a prior grant on either target traversal; regenerate the evidence. Preserve the approved constraints; if the required precondition cannot be obtained within them, HALT as specified.
- [x] [Review][Patch][high] Match the complete pending traversal and its exit before retaining a grant or suppressing its replacement request [Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs:207]. The first movement alone does not establish coverage after a replan; `CoveredByOwnGrant` at line 435 can suppress a request that the driver's `Covers` rejects. Add a regression for a changed continuation/exit with the same first movement, without revoking committed grants.
- [x] [Review][Patch][medium] Preserve O8 engagement when a committed roundabout traversal advances to a continuation [Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRequestBuilder.cs:94]. Engagement is recomputed from distance to the next movement, so a slow vehicle with `Held(Committed)` can request its already granted suffix instead of the next traversal. Add a real-frame regression on an internal ring corridor; downstream stopping failure has not been demonstrated.
- [x] [Review][Patch][medium] Include fallback occupancy-report construction in the published coordinator timing and allocation scope [Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs:198]. `Coordinate` starts its measurements only afterward (line 207), contrary to the metric's documented scope. The existing deferred PlayMode coverage for actors without a driving step is not reopened by this review.

**Initial verification attempt — blocked:** `.\scripts\validate.ps1 -Profile Story -Story 5.34 -TestMode Both` exited 1 before test execution. Compilation-state query returned no response or CLI exit code 6 (AD-8). Tests executed: **0**; result: **VALIDATION FAILED / INCOMPLETE**. Recompilation reported `up_to_date`, Console errors since cursor 158: 0; these do not substitute for the failed official gate. Raw output: `code-review-5-34-2026-10-05-validation.txt`. No retry, alternate test execution, wrapper or probe was used. Remaining required regression/performance/exploration validations were not executed in this review.

**Patch execution — 2026-10-05, owner choice 1:** R2-R4 are applied. R1 now requires a fresh `Granted` and rejects any target grant before the first shared request batch. The test removes the south obstacle one physics step after the east obstacle; model, scene, routes, physics tuning and acceptance thresholds are unchanged. The latest official run (`code-review-5-34-2026-10-05-patches-validation-5.txt`, exit 1) reports **48/48 EditMode passed, 2/3 PlayMode passed**, compilation healthy and 0 Console errors since cursor 350.

**Previous gate — blocked by C.3 (resolved by the run below):** C now proves fresh arbitration at source batch 1496: east `Granted`, south `Denied(ConflictGranted)` with holder and zone 23. East is `Released(Cleared)` at batch 1757; south first satisfies the stopped-and-held condition at frame 1760, at d = 0.34797 m, with `BlockedExit:40e937a99618cac3ce12d56586514283` (0.51603 m free versus 6.44 m required). The unchanged C.3 assertion requires `JunctionGrant`. Journal: `traffic-v2-5-34-explorations/scenario-C-20261005-102124-junction.txt`; full chronology and raw failure: `code-review-5-34-2026-10-05.md`. No acceptance relaxation or further staging/tuning was performed after this failed checkpoint. Required Story533/Story531/Story552 regressions and explicit performance/exploration runs remain unexecuted. Status remains `in-progress` until the C montage/criterion is resolved and all required gates pass.

**C montage revised, C.3 unchanged — 2026-10-05, owner choice 1:** Only C's insertion order is reversed, with routes, obstacles, distances and D unchanged. South has the smaller TrafficId and wins the fresh tie at batch 1495; east is `Denied(ConflictGranted)` with holder and zone 23, then stops at frame 1759 at d = 0.348 m under `JunctionGrant` before south's `Released(Cleared)` at batch 1838. East is served at the first sufficient exit batch 1892, enters at 2006 and exits normally. All C assertions pass, with no prior target grant, contacts, incompatible grants/occupancy, entry without grant or tracking-tolerance failure. Evidence: `traffic-v2-5-34-explorations/scenario-C-20261005-104718-summary.md` and its junction journal. Official validation (`code-review-5-34-2026-10-05-montage-C-validation.txt`) exited **0**: **48/48 EditMode, 3/3 PlayMode passed**, no skipped/inconclusive tests, healthy compilation and 0 Console errors since cursor 489. Required regression and explicit campaign validations are now running; story remains `in-progress` pending them.

**First D13 checkpoint — failed, subsequently resolved:** Story533 (69/69 EditMode, 12/12 PlayMode), Story531 (50/50 EditMode, 13/13 PlayMode) and Story552 (4/4 PlayMode) passed with no skipped/inconclusive tests, healthy compilation and 0 Console errors. `Story533Perf` executed all five explicit tests and failed `DiagnoseFourVehicles`: **one frame with 4 FixedUpdate**, exceeding the unchanged maximum of 2. Official output: `code-review-5-34-2026-10-05-performance.txt`, exit **1**, **4/5 passed**, `VALIDATION FAILED / INCOMPLETE`; 0 Console errors since cursor 845. The N4 summary (`traffic-v2-5-33-explorations/perf-N4-20261005-110140-summary.md`) locates the overrun at frame 1806, host steps 932–935 (N3 at those steps), preceded by a 62.935 ms frame with 61.485 ms outside PlayerLoop, Traffic V2 0 and GC 0. The gate intentionally includes the complete N4 run window, transitions and partial boundaries; this frame is not exempted. N8 passes at p95 **9.573 ms**, coordinator p95 **0.056 ms**. Cause of the preceding external/Editor-render stall is not established. No retry, optimization, threshold change or exclusion was made. `Story533Exploration` was not launched after the D13 failure. All four original patch findings are fixed, but status remains **in-progress** under the frozen Ask First rule: budget exceeded means HALT and owner arbitration.

**Authorized N4 replay — 2026-10-05, owner choice 1: failed again.** `.\scripts\validate.ps1 -TestMode PlayMode -TestFilter 'RoadRage.Tests.PlayMode.Story533PerformanceDiagnosticPlayModeTests.DiagnoseFourVehicles' -TestFilterType testName -IncludeExplicit` ran exactly one test and exited **1**: **0/1 passed**, `VALIDATION FAILED / INCOMPLETE`, healthy compilation, 0 Console errors since cursor 953. Raw output: `code-review-5-34-2026-10-05-performance-N4-replay.txt`. The unchanged-threshold replay summary (`traffic-v2-5-33-explorations/perf-N4-20261005-112127-summary.md`) reports **two frames with 3 FixedUpdate**, preceded by 59.081 ms and 53.315 ms frames. The first preceding frame includes Traffic V2 **28.840 ms** and GC **5.850 ms**; the second reports Traffic V2 **2.383 ms**, GC **0**. Maximum allowed remains **2**; the gate covers the full window, including those N3-population steps. N4 full-population p95 is **3.088 ms**, coordinator p95 **0.035 ms**. This repeated failure is not dismissed as an isolated external/Editor stall; the cause needs diagnosis. No third run, optimization, threshold change, frame exclusion or exploration campaign was performed. Status remains **in-progress**, pending owner arbitration of the D13 diagnostic/correction scope.

### Review validation closeout — 2026-10-05

**Owner-authorized D13 diagnosis/correction — 2026-10-05, choice 1, thresholds unchanged:** the enriched existing performance fixture identified a 59.305 ms `SceneView.Repaint` contribution to one failed N4 run. A Game-tab comparison removed later overruns but still failed at the initial partial boundary (three physical steps); both failed official outputs and summaries are retained in the review report. Recorder discovery/start was occurring after the existing warmup, immediately before measurement. It now occurs on the first driven step, before five warmup steps and completion of the initialization render frame. All physical steps after measurement begins, both partial boundaries and all population transitions remain counted; no D13 assertion or scenario changed. Summaries publish initialization time and host epochs. Story533 PlayMode revalidation passed **12/12**, exit **0**, 0 Console errors since cursor 1367 (`code-review-5-34-2026-10-05-D13-publication-validation.txt`). The corrected full `Story533Perf` campaign passed **5/5**, exit **0**, 0 Console errors since cursor 1471 (`code-review-5-34-2026-10-05-performance-corrected.txt`): N4 maximum **2 FixedUpdate/frame**, full N4 host p95 **2.962 ms**, coordinator p95 **0.035 ms**; full N8 host p95 **9.479 ms < 10**, coordinator p95 **0.054 ms**, 926 full-population steps. Initialization at host step 2 and measurement start at step 7 preserve the same 3,862-step N4 window. Measurements are in the Editor with Game activated; external Editor stalls are not claimed to be universally eliminated. Graphify update exited **0** (test-only change, no runtime topology change). Required explicit exploration is in progress, so status remains **in-progress** until its official gate passes.

**Final required gate — passed:** `Story533Exploration` executed all **5** selected explicit tests, exit **0**, zero skipped/inconclusive, healthy compilation, **0 Console errors since cursor 1567**, no modified scene. Raw output: `code-review-5-34-2026-10-05-exploration.txt`. Nominal N2, its replay, N4 and N8 reach all exit portals, with zero V2-V2 contacts, incompatible grants, entries without grant, incompatible occupancy and nominal tracking latches. `explore-8-20261005-122334-summary.md` reports **8/8 exits**, **926** full-population steps, full-population host p95 **9.568 ms < 10**, coordinator p95 **0.044 ms**. The pushed scenario (`explore-poussee-20261005-122353-summary.md`) records its expected pre-5.39 tracking fallback, with coordination invariants green; recovery is not claimed. Together with Story534 **48/48 EditMode + 3/3 PlayMode**, Story533 **69/69 + 12/12**, Story531 **50/50 + 13/13**, Story552 **4/4 PlayMode**, the post-fixture Story533 **12/12 PlayMode** recheck and corrected performance **5/5**, all required scoped gates are green. No full epic suite, scene/model/physics/Gate A/threshold change, verification wrapper, additional review loop or new deferral was introduced. The frozen contract is unchanged. Story and sprint entry **5-34-junction-coordination-core-grants-and-conflicts** are **done**; final Editor state is clean `MainMenuLobby`.

**Acceptance Criteria:**

- Given le scenario C (reproduction de la rencontre `explore-8` : deux vehicules sur les routes des insertions #1 et #8, obstacle `sortie`), when il s'execute, then les records publies montrent, dans l'ordre :
  1. deux demandes actives au meme lot sur `40ca7f10` et `4e437f94` (precondition ; absente = run invalide, jamais reussi) ;
  2. un seul `Granted`, au plus ancien (ou au plus petit `TrafficId` a egalite) ;
  3. l'autre `Denied(ConflictGranted)` citant le titulaire et `4258af54`, arrete avant l'entree (pare-chocs avant jamais au-dela, distance minimale publiee), blocker `JunctionGrant` legitime ;
  4. `Released(Cleared)` du premier ;
  5. `Granted` du second au meme lot ou au suivant ;
  6. son entree puis sa sortie.
  Et a aucun pas deux grants incompatibles effectifs, aucun contact entre vehicules, aucun `TrackingToleranceExceeded`, aucun repli hors `ExitPortalReached`, `EnteredWithoutGrant` = 0, les deux portails de sortie atteints.
- Given le scenario D (un premier vehicule passe par `40ca7f10`, puis est tenu sur `40e937a9` par un obstacle cinematique place par le test, entierement hors du mouvement, en laissant une longueur libre < L + s0), when un second demande `4e437f94`, then :
  - il est refuse `ExitBlocked`, raison de sortie evaluee avant tout conflit ;
  - il s'arrete avant l'entree avec un blocker `BlockedExit`, et n'entre jamais tant que la sortie est insuffisante ;
  - apres retrait de l'obstacle, il est servi et les deux sortent, sans contact ni `TrackingToleranceExceeded`.
- Given `Story531`, `Story552`, `Story533` A et B rejoues sans modification de leurs assertions, when ils s'executent, then ils restent verts.
- Given `Story533Perf` et la campagne a N = 2, 4 et 8, when ils s'executent, then le cout du coordinateur est publie a part pour chaque N, le pas hote Traffic V2 a N = 8 a un p95 < 10 ms, et explore-4 a N = 4 compte au plus 2 FixedUpdate par frame.
- Given la campagne `Story533Exploration` sans D12, when elle s'execute, then chaque contact est classe selon O6. Tout echec 5.34, tout contact « autre » et tout verrou 2a nominal font echouer la campagne. Les constats 5.35 sont publies sans effet sur le verdict, de meme que la sequence ZC23 si elle est contestee et l'absence de StopHold mutuel a jeu negatif.
- Given toute fixture `Story534`, when la suite tourne, then chaque grant, refus, revocation et liberation porte une raison stable assertable.

## Spec Change Log

- **2026-10-03 -- implementation, avant le constructeur de demandes, decision proprietaire O8 (traversee demandee).**
  - Constat mesure sur `MVP_Run`. Corridors de sortie : 13,5 m (T), 16 m (croix), 9,5 a 13,5 m (giratoires). D'une entree de traversee a l'entree de la suivante : 23,4 a 32,0 m.
  - Vitesses d'entree relevees dans les traces 5.33 (`perf-N8-20261003-175604`, `acceptance-B-20261003-174756`) : 6,3 a 6,55 m/s sur les T tout droit, 5,75 m/s en tourne-a-gauche, 4,4 a 4,9 m/s en entree de giratoire.
  - Formules du bloc fige : `D_engage` 24,5 m et `D_request` 24,8 m a 6,5 m/s ; 33,2 m et 33,5 m a 8 m/s.
  - Lecture litterale (« premiere traversee a ou devant le pare-chocs avant ») : la traversee suivante B n'est demandable qu'une fois le pare-chocs sorti de A, a `d_B` ≤ 13,5 m < `D_engage`. `JunctionEntry` lierait a chaque enchainement et le controle « carrefour libre » echouerait (HALT du bloc fige).
  - **Decision O8 :** la demande porte sur la premiere traversee de la route restante a ou devant le pare-chocs avant **pour laquelle le vehicule ne tient pas de grant engage**. B devient demandable des que A est engage (`d_A < D_stop(v)` ou entree franchie), donc a `d_B` ≥ `D_stop` + 23,4 m, soit au moins 7 m de marge meme a v0 = 8 m/s.
  - Le coordinateur recoit, a cote de la demande, les faits d'engagement des traversees tenues (`d`, `D_stop`, entree franchie). La regle d'engagement reste celle du bloc fige ; elle est evaluee par le coordinateur depuis ces faits.
  - La contrainte `JunctionEntry` porte sur la meme traversee que la demande. Une demande reste au plus une par vehicule et par frame ; le reste du bloc fige est inchange.
  - Alternatives ecartees : la lecture litterale (echec certain du controle) et « premiere traversee dont l'entree est devant le pare-chocs » (marge de 3,9 a 4,7 m aux vitesses mesurees, echec a 8 m/s sur la croix tout droit : 32 m < 33,5 m).
  - Consequence sur l'invariant par lot : un grant engage dont le titulaire approche sans etre entre n'est plus porte par sa demande (elle est passee a la traversee suivante) mais par ses faits d'engagement, rapportes par le constructeur. L'invariant se lit donc : chaque grant a un acteur present, demandeur, occupant ou engage sur sa traversee.
- **2026-10-03 -- implementation, arbitrage de l'entree, decision proprietaire O9 (attente dans la fenetre de maintien).**
  - Mesure sur le modele point-masse du banc 5.33 (IDM, `a_kin`, StopHold D11, roue libre sous 0,41 m/s), depart a 4, 6 et 8 m/s sans grant : le vehicule entre en StopHold a d ≤ s0 + Δhold = 2,5 m et s'arrete a d ≈ 2,42 m de l'entree.
  - A l'arret, les formules du bloc fige donnent `D_engage` = 2,093 m et `D_request` = 2,096 m : la demande deviendrait invalide (`TooFar`), l'anciennete O2 serait perdue a chaque arret et le vehicule ne serait jamais servi. Liberer le maintien quand `JunctionEntry` devient inactive reintroduirait le rampement sous 0,5 m/s supprime par D11.
  - **Decision O9 :** un vehicule dans la fenetre de maintien reste demandeur et contraint. Demande valide si d ≤ max(`D_request(v)`, s0 + Δhold) ; `JunctionEntry` active si d ≤ max(`D_engage(v)`, s0 + Δhold). Le maintien a l'entree ne se libere qu'au grant effectif (ou a un changement de traversee). Les formules derivees, `D_stop` et l'engagement sont inchanges ; le plancher n'agit que sous ~0,3 m/s.
  - Alternatives ecartees : fenetre de maintien reduite a `D_request(0)` − s0 ≈ 0,1 m (rampement de 2,5 a 2,1 m sous 0,5 m/s, risque de derive laterale mesure en 5.33) ; pas de StopHold a l'entree (contraire au bloc fige, rampement indefini).
- **2026-10-03 -- implementation, controle « carrefour libre », decision proprietaire O10 (distance de demande sous acceleration).**
  - Mesure par `Story534JunctionEntryTests.AVehicleAloneNeverMeetsAnActiveEntryConstraintOnTheElevenCampaignRoutes` : vehicule seul, 11 routes de `campaign-5-31.json`, pipeline reel du driver en point-masse (frame, spine, perception, plan de vitesse, rapport, arbitrage, lot), depart a l'arret au portail.
  - Avec la formule du bloc fige (`D_request` = `D_engage(v)` + `v_ref·t_lat`) : 19 pas isoles sur environ 31 000, tous en acceleration (v de 1,8 a 4,4 m/s), ou `JunctionEntry` lie a −0,30 / −0,69 m/s² pendant une frame. Exemple a l'insertion : d = 5,822 m, fenetre [`D_engage` 5,840 ; `D_request` 5,914] sautee. Cause : l'ecart `v_ref·t_lat` ne couvre pas la croissance de `D_engage` en un pas a l'acceleration a.
  - **Decision O10 :** `D_request(v)` = `D_engage(v_ref)` + `v_ref·t_lat`, soit `D_engage` evalue a la vitesse atteignable pendant la latence. Condition suffisante : `D_request(v)` ≥ `D_engage(v + a·Δt)` + (v + a·Δt)·Δt, vraie des que l'acceleration appliquee reste ≤ a (garanti par l'arbitrage : route libre et profil bornes par a). Mesure avec O10 : 0 pas actif sur les 11 routes. `D_stop`, `D_engage`, l'engagement et le plancher O9 sont inchanges ; `D_request` passe de 33,49 a 33,86 m a 8 m/s.
  - Sans rapport avec O10, la meme mesure a revele un defaut d'implementation corrige : une route de mesure qui repasse par un carrefour (objectif intermediaire) confondait ses deux passages dans la position des mouvements. Les positions sont desormais bornees de la traversee la plus recente derriere le vehicule a la traversee demandee, et un mouvement repete n'est « derriere » que si toutes ses occurrences le sont.
- **2026-10-03 -- checkpoint 2, non-regression `Story533` A, decision proprietaire O11 (file tenue a l'entree d'un carrefour).**
  - Resultats : `Story531` 50/50 EditMode et 13/13 PlayMode, `Story552` 4/4, `Story533` 69/69 EditMode et 10/11 PlayMode. Seul le scenario A echoue (`acceptance-A-20261003-211821`).
  - Constat : l'obstacle de A est a 0,5 m dans le corridor de sortie `47de8d1a` (13,5 m) d'un giratoire. En 5.33, la file de trois se formait dans l'anneau. En 5.34, les vehicules 1 et 2 recoivent la traversee (l'obstacle non V2 n'est pas compte dans la recherche de sortie, conforme au bloc fige). Le vehicule 3 est refuse `ExitBlocked` (13,5 − 2 × 6,44 m reserves < 6,44 m) et tenu a 2,497 m de l'entree `452ee31e`, blocker `BlockedExit` legitime, a 16,15 m de son leader. Apres retrait : liberation `GrantEffective`, 3/3 sorties, 0 contact, aucun `TrackingToleranceExceeded`, aucun rampement. Seule l'assertion « chaque suiveur a [s0 − 0,10 ; s0 + 0,50] m de son leader » echoue.
  - Aucun corridor de `MVP_Run` ne depasse 16 m : une file de trois (≈ 21 m) recouvre toujours un carrefour.
  - **Decision O11 :** le controle de file de A est amende. Un suiveur arrete avant l'entree d'un carrefour par un blocker de coordination legitime (`JunctionGrant` ou `BlockedExit`), a 0 < d ≤ s0 + Δhold de cette entree, occupe une position de file valide. Le premier vehicule et les suiveurs hors carrefour gardent l'intervalle [s0 − 0,10 ; s0 + 0,50] m. Les autres assertions de A et toutes celles de B sont inchangees. Cette decision amende, pour A seulement, le critere « `Story533` A et B rejoues sans modification de leurs assertions ».
- **2026-10-03 -- scenario C, decision proprietaire O12 (service apres une sortie commune).**
  - Mesure (`scenario-C-20261003-212947`) : `40ca7f10` et `4e437f94` debouchent toutes deux sur `40e937a9`, le corridor de sortie du scenario D. Le titulaire est `Released(Cleared)` au lot 1838 alors que son arriere occupe encore le debut de cette sortie (0 m libre). Le second est donc refuse `ExitBlocked` (borne `Occupant`), par la regle de sortie du bloc fige. Il est servi au lot 1892, le premier ou sa sortie libre atteint L + s0 = 6,44 m (6,434 m au lot 1891, 6,557 m au lot 1892), soit 54 lots apres le `Cleared`. Les etapes 1 a 4 de la sequence C sont vertes.
  - Le critere C.5 (« `Granted` du second au meme lot ou au suivant ») et la regle de sortie sont incompatibles dans cette geometrie.
  - **Decision O12 :** le second est servi au premier lot ou sa sortie devient suffisante (≥ L + s0), ou au lot suivant. Entre le `Released(Cleared)` et ce lot, seuls des refus `ExitBlocked(Occupant)` sont permis, sans nouveau refus pour conflit. Les criteres C.1 a C.4 et C.6 sont inchanges. La trace de carrefour publie desormais la longueur exigee (`junction_exit_required_m`).
  - Correction de test sans decision : la fixture de saturation du collecteur fixait une capacite de 3, or la caisse d'un vehicule compte elle-meme 3 colliders. Les deux requetes saturaient donc a tout rayon. La capacite est desormais le compte mesure du vehicule sature, au plus petit rayon ou il depasse strictement celui du vehicule epargne.
- **2026-10-03 -- revue (blind-hunter, edge-case-hunter, verification-gap), patchs appliques.**
  - Etape 1, grant non engage : il est desormais aussi revoque `Revoked(ConflictOccupied, occupant, zone)` quand un occupant protege incompatible est present (vehicule entre sans grant). Lecture unique de l'etape 0 (« bloque les mouvements incompatibles ») ; un grant engage reste tenu. Test : `APendingGrantDoesNotSurviveAnIncompatibleVehicleThatEnteredWithoutGrant`.
  - Tests ajoutes : invalidation « hors repli » (`AReportInFallbackIsAnInvalidRequestAndWithdrawsItsPendingGrant`) et porte p95 D13 a N = 8 (`D13RejectsAnN8HostStepP95OfTenMillisecondsOrMore`).
  - Campagne : le p95 D13 ne compte plus que les pas en population pleine, comme `Story533Perf` ; le journal de coordination signale sa troncature.
  - Differes (deferred-work) : vehicule non localise et coordination (5.39) ; rapport d'occupation du runner pour un vehicule sans pas.
- **2026-10-03 -- apres revue, defaut signale par le proprietaire (videos C et D), decision proprietaire O13 (point d'arret sans grant).**
  - Perimetre : position d'attente et frontiere de maintien seulement. La priorite (qui recoit le grant) est inchangee et reste aux stories de regles de circulation.
  - Constat mesure (`scenario-C-20261003-223818`, `scenario-D-20261003-224427`) : le vehicule refuse s'arrete a d = 2,448 m du debut de son mouvement, d etant mesure du pare-chocs avant (`SMax` de l'empreinte) a `s = 0` du premier mouvement de la traversee. La frontiere de carrefour (AABB) deborde de 2 m avant ce point. Decomposition : l'IDM du bloc fige vise un obstacle virtuel a l'entree, donc un equilibre a s0 = 2 m ; le maintien D11 se declenche a d ≤ s0 + Δhold = 2,5 m. Second defaut : `a_kin` (≤ 0, nul a l'arret) entre toujours dans le minimum. Le candidat ne peut donc jamais accelerer : en D, `a_kin` lie a −0,44 m/s² des d = 7,6 m, et un vehicule arrete entre la fenetre et `D_engage` y reste fige.
  - **Decision O13, qui amende la ligne « Candidat = min(IDM…, a_kin)… equilibre a s0 » du bloc fige :** l'entree est une ligne, pas un vehicule. Le candidat est l'IDM vers un obstacle virtuel place a s0 − `m_ctrl` au-dela de l'entree, d'ou un equilibre du pare-chocs avant a `m_ctrl` de l'entree. `a_kin`, qui vise le meme point, n'entre dans le minimum qu'a d ≤ `D_stop(v)`, ou il assure la garantie d'arret en cas de refus tardif. La fenetre de maintien d'une entree devient `m_ctrl` + Δhold = 1,0 m, et le plancher O9 suit. Bande d'arret declaree : [`m_ctrl` − 0,02 ; `m_ctrl` + Δhold] = [0,48 ; 1,0] m. Aucune nouvelle constante : `m_ctrl` et Δhold sont inchanges.
  - Mesures sur le banc point-masse (`o13-stop-band.txt`, `o13-worst-case.txt`, `o13-resume.txt`), v0 de 0 a 8 m/s, refus de `D_stop` a 40 m : arret a 0,946..0,955 m pour tout refus a d ≥ (`D_stop` + `D_engage`)/2, a 0,50..0,58 m pour un refus a `D_stop` exactement ; minimum global 0,501 m, aucun franchissement. Apres le grant : liberation `GrantEffective`, entree franchie en 1,38 s (depart libre 1,12 s), sans nouveau maintien.
  - Geometrie (`o13-clearance.txt`) : sur les 48 entrees de traversee de `MVP_Run`, l'empreinte arretee a d = `m_ctrl` reste a au moins 1,98 m de tout mouvement en conflit.
  - Tests ajoutes : `ARefusedVehicleStopsInTheDeclaredBandBeforeTheEntryWhateverItsSpeedAndTheRefusalDistance`, `AVehicleHeldBeforeTheEntryLeavesNormallyOnceItsGrantIsEffective`, `TheFootprintHeldAtTheControlMarginStaysClearOfEveryConflictingMovementOfMvpRun`. Les tests d'equilibre et d'egalite sont adaptes.
  - PlayMode prepare mais non execute (validation par le proprietaire) : C et D assertent la bande d'arret, puis une reprise sans maintien ni `JunctionEntry` jusqu'a l'entree, en au plus 3 s apres le premier pas a grant effectif. La condition O11 de `Story533` A suit la nouvelle fenetre (d ≤ `m_ctrl` + Δhold).
  - Validation du proprietaire (`scenario-C-20261003-231258`) : arret a d = 0,949 m, entree 2,48 s apres le grant. La position reste en retrait du bord interieur des passages pietons, de ~3 m au bras nord et de ~7 m au bras est de TJunction_West, parce que le debut des mouvements (s = 0, import V1) ne coincide pas avec ce bord. Aucune donnee du modele ne porte cette limite. **Decision proprietaire :** pas de `StopLine` authoree en 5.34 ; la limite visuelle releve de la 5.35 (controles authores). Mesures et pistes ecartees consignees dans deferred-work.
- **2026-10-03 -- decision proprietaire O14 (arret au plus pres de l'entree, ~0,25 m).**
  - Constat : avec O13, l'arret a ~0,95 m vient de `m_ctrl` (0,5 m), puis d'environ 0,45 m d'approche asymptotique de l'IDM, que le maintien D11 coupe des d ≤ `m_ctrl` + Δhold. Baisser `m_ctrl` seul aurait donne ~0,70 m.
  - **Decision O14 :**
    - `m_ctrl` passe de 0,5 a 0,25 m (revision proprietaire de la seule valeur declaree ; D_stop baisse de 0,25 m a toute vitesse, D_engage et D_request sont inchanges au-dela de ~1 m/s).
    - L'IDM de `JunctionEntry` vise la ligne d'entree (obstacle virtuel a s0 au-dela).
    - La fin de l'approche est conduite par `a_kin` (deceleration constante qui s'annule a `m_ctrl`, retenue a d ≤ `D_stop`) : l'arret se fait en temps fini, sans rampement.
    - La fenetre de maintien d'une entree devient `D_stop` a la vitesse d'entree du maintien (0,5 m/s), soit 0,352 m : une valeur derivee, sans nouvelle constante. Le plancher O9 suit.
    - Bande d'arret declaree : [`m_ctrl` − 0,02 ; fenetre] = [0,23 ; 0,352] m.
  - Mesures du banc (`o13-*.txt`, regeneres) : arret a 0,341..0,348 m pour tout refus a d ≥ (`D_stop` + `D_engage`)/2, de v0 = 0 a 8 m/s ; 0,25..0,33 m pour un refus a `D_stop` exactement. Minimum global 0,251 m, aucun franchissement. Reprise : entree franchie 0,9 s apres le grant (0,68 s pour un depart libre). Degagement geometrique a d = `m_ctrl` : 1,95 m au moins sur les 48 entrees.
  - L'exemple calcule de la spec passe a `D_stop` = 16,95 m a 8 m/s (17,2 m avant). Les assertions PlayMode (C, D, condition O11 de `Story533` A) suivent la nouvelle fenetre ; elles sont preparees mais non executees (validation par le proprietaire).

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
- **O6 (revue de la spec, 2026-10-03)** :
  - frame invalide fail-closed, avec protection des occupants avant toute demande ;
  - distances derivees de la cinematique et de la latence, plus un test au pire cas ;
  - tete de file definie par l'occupation structuree ;
  - recherche de sortie a borne semantique, avec le cas 0,87 m ;
  - `Story533Perf` au checkpoint, index precalcule, cout du coordinateur par N, budget D13 ;
  - frontiere 5.34 / 5.35 explicite dans les verdicts.
  Approuves sans reserve : reservation de l'ancien, tete de file, sortie le long de la route, demande anticipee, test de saturation du collecteur.

**O7 -- Traversee plutot que mouvement (decision proprietaire du 2026-10-03, grant par traversee valide).** Dans `MVP_Run`, les 24 mouvements d'entree et de continuation de giratoire debouchent sur un corridor d'anneau de 0,87 ou 5,58 m suivi d'un autre mouvement du meme carrefour.
- Avec un grant par mouvement, deux issues seulement :
  - une recherche de sortie bornee au prochain mouvement ne trouverait jamais `L + s0` = 6,44 m : refus permanent ;
  - le vehicule attendrait a l'interieur de l'anneau son grant suivant : risque d'attente circulaire.
- Le grant de traversee, tout ou rien, supprime les deux : jamais d'attente dans un carrefour, sortie mesuree apres le dernier mouvement.
- Consequence assumee : un vehicule engage dans l'anneau tient deja ses continuations. Une entree incompatible attend donc un vehicule deja engage. C'est l'effet generique de la traversee engagee, pas une regle « l'anneau d'abord ».
- Le debit d'anneau baisse (reservation sur toute la traversee), a mesurer. La 5.35 pourra affiner.

**Reservation de l'ancien.** Sans elle, le FIFO seul ne garantit pas O2. Avec elle :
- l'ensemble des grants incompatibles avec la plus ancienne demande refusee pour conflit ne peut que decroitre ;
- elle est servie des que ses titulaires liberent, ce qui est borne hors interblocage (5.40).
Une demande refusee pour sortie ne reserve rien : elle gelerait le carrefour pour une cause exterieure.

**Distances : exemple calcule, non une constante.** Profil par defaut (v0 8, T 1,5, s0 2, a 1,5, b 2), Δt = 0,02 s, `m_ctrl` = 0,5 m, a v = 8 m/s :
- `t_lat` = 0,04 s ; `v_ref` = 8,06 m/s ; `b_plan` = 1,98 m/s² ;
- `D_stop` = 0,32 + 16,40 + 0,5 ≈ 17,2 m ;
- `s*(v_ref)` ≈ 32,8 m, donc `D_engage` ≈ 33,2 m ;
- `D_request` ≈ 33,5 m.
A ces vitesses, `s*` domine : la contrainte s'applique tot et doucement (IDM), et `a_kin` ne lie qu'en cas de refus tardif. Une demande emise a `D_request` precede `D_engage` d'au moins un lot : sur carrefour libre, le grant est effectif avant que la contrainte ne s'applique.

**Tete de file.** Une definition par « meme prochain mouvement » laisserait demander un suiveur masque par un leader qui tourne ailleurs. La definition retenue (aucune occupation V2 entre le pare-chocs et l'entree) l'inclut et ferme ce cas.
- Limite assumee : un obstacle non V2 (pieton, objet) n'interrompt pas la demande. Un titulaire non engage tenu derriere lui garde son grant et bloque les traversees incompatibles tant que l'obstacle reste. Constat publie ; traitement 5.39 / 5.40.

**Budget D13.** D13 fixait « N = 8 sous 10 ms par pas hote » sans preciser la statistique. La lecture proprietaire du 2026-10-03 en fait le p95.
- `Story533Perf` ne couvre que N = 1..4 et n'assertait aucun budget. Le p95 a N = 8 se lit donc sur la campagne (explore-8), dont le harnais publie desormais le p95 du pas hote.

**Hypotheses non verifiees.**
- Dans les campagnes, la sequence ZC23 de `explore-8` peut ne pas se reproduire : les attentes aux autres carrefours decalent les arrivees, et le PlayMode multi-vehicule n'est pas deterministe au bit pres. C'est la raison d'etre du scenario dedie C.
- `m_ctrl` = 0,5 m n'est pas mesure : les scenarios C et D publient la distance minimale a l'entree pendant un refus.
- Le cout des grants de traversee en giratoire n'est pas mesure.

## Verification

**Commands** (Domain Reload actif : les runs PlayMode s'enchainent sans redemarrer l'Editeur) :
- `.\scripts\validate.ps1 -Profile Story -Story 5.34 -TestMode Both` -- expected: `VALIDATION STORY`, compte execute = compte attendu, C et D verts, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.33 -TestMode Both`, puis `-Story 5.31 -TestMode Both`, puis `-Story 5.52 -TestMode PlayMode` -- expected: verts, assertions inchangees.
- `.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story533Perf -TestFilterType category -IncludeExplicit` -- expected: au plus 2 FixedUpdate par frame a N = 4, cout du coordinateur publie.
- `.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story533Exploration -TestFilterType category -IncludeExplicit` -- expected: verdicts O6 verts, p95 a N = 8 < 10 ms, rapports ecrits.

## Suggested Review Order

**Coordinateur : lot, grants et conflits**

- Point d'entree : un lot par pas, etapes 0 (occupants), 1 (grants anterieurs), 2 (demandes).
  [`JunctionCoordinator.cs:74`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L74)

- Etape 0 : les occupants reels bloquent les mouvements incompatibles, sans memoire.
  [`JunctionCoordinator.cs:118`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L118)

- Patch de revue : grant non engage revoque face a un occupant entre sans grant.
  [`JunctionCoordinator.cs:219`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L219)

- Etape 2 : demandes par (anciennete, TrafficId), sortie evaluee avant tout conflit.
  [`JunctionCoordinator.cs:234`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L234)

- Engagement : entree franchie ou d < D_stop ; un grant engage reste tenu.
  [`JunctionCoordinator.cs:336`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L336)

- Sortie suffisante : L + s0, moins les grants deja emis vers le meme corridor.
  [`JunctionCoordinator.cs:350`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L350)

- Liberation : arriere au-dela du mouvement ; boundary seulement sans localisation.
  [`JunctionCoordinator.cs:321`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L321)

- Paires de conflit compilees une fois depuis les ConflictZones du modele.
  [`JunctionConflictIndex.cs:141`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionConflictIndex.cs#L141)


**Demande d'un vehicule : rapport, distances, sortie**

- Rapport par acteur : occupation, approches (O8), demande unique et motif de rejet.
  [`JunctionRequestBuilder.cs:42`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRequestBuilder.cs#L42)

- O8 : une traversee engagee est tenue, la demande passe a la suivante.
  [`JunctionRequestBuilder.cs:96`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRequestBuilder.cs#L96)

- Tete de file : aucun occupant V2 entre le pare-chocs et l'entree.
  [`JunctionRequestBuilder.cs:225`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRequestBuilder.cs#L225)

- Recherche de sortie bornee : premier occupant, mouvement suivant, portail ou fin de route.
  [`JunctionRequestBuilder.cs:258`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRequestBuilder.cs#L258)

- Distances derivees D_stop, D_engage, D_request (O10) et fenetre de maintien (O14).
  [`JunctionDistances.cs:78`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionDistances.cs#L78)


**Arbitrage : contrainte JunctionEntry et point d'arret (O13, O14)**

- Candidat : IDM vers la ligne, a_kin seulement en deca de D_stop ; arret a m_ctrl.
  [`LongitudinalArbitration.cs:679`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs#L679)

- Fenetre de maintien d'une entree : D_stop a la vitesse d'entree du maintien.
  [`LongitudinalArbitration.cs:692`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs#L692)

- Contrainte active sans grant effectif, avant l'entree, a d <= D_engage.
  [`LongitudinalArbitration.cs:491`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs#L491)

- Maintien a l'entree libere seulement au grant effectif ; perception indisponible ne libere pas.
  [`LongitudinalArbitration.cs:520`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs#L520)

- m_ctrl, seule valeur declaree : 0,25 m (decision O14).
  [`TrafficV2Composition.cs:120`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs#L120)


**Branchement runtime**

- Le runner resout un lot par pas ; frame refusee : seuls les grants engages survivent.
  [`TrafficV2StepRunner.cs:211`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs#L211)

- Le driver construit son rapport et l'entree d'arbitrage sur la route a jour.
  [`TrafficV2VehicleDriver.cs:738`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L738)

- Blockers JunctionGrant et BlockedExit, nommes depuis le record de la traversee.
  [`BlockerTracker.cs:40`](../../Assets/RoadRage/Features/Vehicles/Traffic/Blockers/BlockerTracker.cs#L40)


**Tests**

- Bande d'arret declaree, de v0 = 0 a 8 m/s, quelle que soit la distance du refus.
  [`Story534JunctionEntryTests.cs:462`](../../Assets/RoadRage/Tests/EditMode/Story534JunctionEntryTests.cs#L462)

- Reprise apres grant, comparee a un depart libre.
  [`Story534JunctionEntryTests.cs:514`](../../Assets/RoadRage/Tests/EditMode/Story534JunctionEntryTests.cs#L514)

- Empreinte arretee hors de tout mouvement en conflit sur les 48 entrees de MVP_Run.
  [`Story534JunctionEntryTests.cs:550`](../../Assets/RoadRage/Tests/EditMode/Story534JunctionEntryTests.cs#L550)

- Pire cas avec latence : jamais au-dela de l'entree.
  [`Story534JunctionEntryTests.cs:247`](../../Assets/RoadRage/Tests/EditMode/Story534JunctionEntryTests.cs#L247)

- Regles du coordinateur sur frames reelles.
  [`Story534JunctionCoordinatorTests.cs:221`](../../Assets/RoadRage/Tests/EditMode/Story534JunctionCoordinatorTests.cs#L221)

- PlayMode C et D, avec l'assertion O14 d'arret et de reprise.
  [`Story534JunctionPlayModeTests.cs:167`](../../Assets/RoadRage/Tests/PlayMode/Story534JunctionPlayModeTests.cs#L167)

- Amendement O11 du controle de file de Story533 A.
  [`Story533FollowingPlayModeTests.cs:149`](../../Assets/RoadRage/Tests/PlayMode/Story533FollowingPlayModeTests.cs#L149)
