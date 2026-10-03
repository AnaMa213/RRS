---
title: 'Story 5.34 -- Coordination de carrefour : demandes, grants, zones de conflit, sortie bloquee et traversee engagee, avec premier branchement runtime'
type: 'feature'
created: '2026-10-03'
status: 'ready-for-dev'
review_loop_iteration: 0
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
- [ ] `_bmad-output/planning-artifacts/epics.md` -- amender la Story 5.34 (index, fiche, table des revues) : grants de traversee, branchement runtime, arret avant la traversee, PlayMode au niveau story (scenarios C et D, retrait de D12), verification `Both`. Citer O1 et O6. La 5.35 garde stop/yield/priorite/priorite d'anneau et la Gate C.

*Phase 1 -- coordinateur pur (EditMode)*
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionConflictIndex.cs` (nouveau) -- incompatibilites precalculees, zone en cause par paire, chaines de traversee par modele.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs` (nouveau) -- demande, grant de traversee, statut, raison stable, instantane immuable a `EffectiveFrame`, compteurs.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs` (nouveau) -- protection des occupants, etat, anciennete, lot deterministe, sortie bornee, engagement, liberation, cycle de vie, lot fail-closed.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionDistances.cs` (nouveau, pur) -- `t_lat`, `v_ref`, `b_plan`, `D_stop`, `D_engage`, `D_request`, `a_kin`.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRequestBuilder.cs` (nouveau, pur) -- traversee, `d`, tete de file par occupation structuree, engagement, recherche de sortie bornee.
- [ ] `Assets/RoadRage/Tests/EditMode/Story534JunctionCoordinatorTests.cs` `[Core][Story534]` -- la matrice, plus :
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
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs`, `SpeedPlan.cs` -- candidat `JunctionEntry` = min(IDM, `a_kin`) et entree optionnelle, rang explicite, cause StopHold, reprise D5 ; sans entree, 5.33 au bit pres.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Blockers/Blocker.cs`, `BlockerTracker.cs` -- `JunctionGrant`, `BlockedExit`, source `JunctionCoordination`.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs` -- coordinateur, instantane par pas, resolution apres les pas, lot fail-closed, `CoordinatorMilliseconds`, diagnostics.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- lecture de l'instantane verifie, demande, entree d'arbitrage, champs de trace.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` -- `JunctionStopControlMarginMeters`.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs`, `TrafficV2WorkCounters.cs` -- partie carrefour et compteurs, par ajout.
- [ ] `Assets/RoadRage/Tests/EditMode/Story534JunctionEntryTests.cs` `[Core][Story534]` -- couvrir :
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
- [ ] `_bmad-output/implementation-artifacts/traffic-v2-5-34-explorations/scenarios-5-34.json` (nouveau) -- C et D, construits de facon deterministe par un constructeur EditMode, comme en 5.33.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story534JunctionPlayModeTests.cs` `[Story534]` :
  - scenario C ;
  - scenario D ;
  - test de saturation du collecteur : `new TrafficV2StepRunner(new TrafficV2HazardCollector(3, rayon))`, deux vehicules, `HazardCollectorSaturated` pour le seul vehicule sature. Reouverture `deferred-work.md:532`, declenchee par la modification du runner.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story533Harness.cs`, `Story533ExplorationPlayModeTests.cs`, `Story533PerformanceDiagnosticPlayModeTests.cs` -- retirer D12 et appliquer les verdicts O6 :
  - aucun grant effectif incompatible simultane ;
  - classement de chaque contact ;
  - `EnteredWithoutGrant` = 0 ;
  - p95 du pas hote et `CoordinatorMilliseconds` publies par N.
  Les records de coordination entrent dans les traces.
- [ ] `_bmad-output/implementation-artifacts/deferred-work.md`, `sprint-status.yaml` (5.34 seulement) -- solder la partie 5.34 de `:519` (la recuperation reste a la 5.39) et l'entree `:532` ; consigner les constats 5.35. Puis `graphify update .`.

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
