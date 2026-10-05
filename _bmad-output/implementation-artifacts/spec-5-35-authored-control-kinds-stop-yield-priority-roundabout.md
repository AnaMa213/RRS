---
title: 'Story 5.35 -- Genres de controle authores : stop, cedez-le-passage, priorite routiere, priorite a droite et entree de giratoire, lignes d''arret, Gate C'
type: 'feature'
created: '2026-10-05'
status: 'draft'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-34-junction-coordination-core-grants-and-conflicts.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Les 40 controles de `MVP_Run` sont `Uncontrolled`. Le coordinateur 5.34 sert donc tout conflit en FIFO (anciennete, puis `TrafficId`), quel que soit le design de la route. `StopLine` n'est authoree nulle part : un vehicule refuse s'arrete jusqu'a ~7 m avant le bord du passage pieton (`TJunction_West`, constat 5.34). La Gate C n'est pas fermee. Audit du 2026-10-05 (`traffic-v2-5-35-explorations/conflict-zone-audit-20261005.md`) : 94 des 136 zones sont des conflits acceptes par prudence ; dans chaque T, toutes les paires inter-approches sont en conflit, et en giratoire une entree est en conflit avec l'entree d'un autre bras et la sortie de son propre bras. Avec le grant de traversee 5.34, un vehicule engage n'importe ou dans l'anneau bloque une entree.

**Approach:** Authorer la verite routiere de la map, sans rien inventer pour la couverture de test :
- giratoires : entrees `Yield`, anneau `Priority` ;
- T : axe traversant `Priority`, branche `Yield` ;
- croix : `Uncontrolled`, lu comme une priorite a droite.
Ajouter des `StopLine` la ou la map en porte l'intention. Le coordinateur 5.34 recoit une preseance compilee une fois par modele et une acceptation de creneau derivee de la cinematique. Garantir la compatibilite reelle des mouvements (P9) et une priorite locale d'anneau par creneau (P10). Re-signer Gate A, puis fermer la Gate C (jalon PlayMode 2, cout multi-agents).

## Boundaries & Constraints

**Always:**

- **Decisions proprietaires du 2026-10-05 (P1-P7, Design Notes).** Story reclassee M -> L, une seule story en 5 phases fermees par checkpoint (P7). `epics.md` est amende en phase 0.
- **Authoring = intention reelle de la route (P2).**
  - Giratoires : controles d'entree `Yield`, controles d'anneau (continuation + sortie) `Priority`.
  - Chaque T : approches `FromWest`/`FromEast` `Priority`, `FromSouth` `Yield`. La verification d'instance (P2) le confirme pour les 4 T.
  - Croix : 4 `Uncontrolled`.
  - Aucun `Stop` dans `MVP_Run` : les prefabs ne portent ni panneau ni marquage. `Stop` est couvert par une fixture EditMode synthetique deterministe.
  - `Signalized` reste refuse (5.36).
- **`Uncontrolled` = absence de controle authore = priorite a droite (P3).**
  - Un carrefour qui melange `Uncontrolled` et un autre genre est un echec de validation.
- **Compatibilite reelle (P9), invariant :** `Compatible traversals must not deny each other solely because they belong to the same Junction.`
  - L'incompatibilite reste la seule co-appartenance a une `ConflictZone` compilee (5.34) ; aucune regle de carrefour exclusif, aucun refus par seule appartenance au meme `JunctionId`.
  - Priorite (`Priority`, `Yield`, `Stop`, priorite a droite), creneau et briseur n'interviennent qu'entre traversees incompatibles.
  - Une zone trop conservatrice se corrige dans les donnees compilees par la voie decidee en P9, jamais par une exception runtime.
- **Priorite locale d'anneau (P10).** La presence d'un vehicule dans le giratoire n'interdit jamais l'entree par elle-meme. Une entree `Yield` ne cede qu'a un vehicule dont la trajectoire restante est incompatible avec sa traversee et dont l'ETA vers la zone de fusion rend le creneau insuffisant :
  - un vehicule qui a depasse la zone pertinente ne bloque pas (mouvement deja libere, 5.34) ;
  - un vehicule present ailleurs dans l'anneau ne bloque pas s'il ne menace pas le creneau ;
  - sans vehicule prioritaire pertinent, entree sans arret obligatoire.
  - Le verdict depend de la trajectoire incompatible et de l'ETA vers la fusion, jamais d'un booleen « giratoire occupe ».
- **Preseance entre deux traversees incompatibles d'un meme carrefour.** Elle se decide sur les controles de leur premier mouvement (le controle d'approche), dans cet ordre :
  1. `Priority` passe avant `Yield` et `Stop` ;
  2. deux `Uncontrolled` : l'approche venant de la droite passe d'abord ;
  3. sinon aucune preseance : departage generique 5.34. Frontiere documentee avec 5.41 (tourne-a-gauche face au tout-droit oppose, etc.).
  - Une traversee qui commence par un mouvement d'anneau (`Priority`) passe donc avant les entrees. La circulation engagee est deja protegee par les grants et l'occupation 5.34.
- **Relation « vient de la droite » derivee (P4).**
  - Fonction pure, deterministe et versionnee (`RightOfWayFunctionVersion`). Entree : le cap en fin de corridor d'approche, dans le plan route (`right = cross(up, tangent)`, AD-45).
  - Pour deux approches A et B : B vient de la droite de A si l'angle entre `t_B` et la gauche de A est ≤ 45° ; B est opposee si l'angle entre `t_B` et `-t_A` est ≤ 45°.
  - Toute paire a moins de `RightOfWayAmbiguityDegrees` (10°, declaree) d'une frontiere de secteur est `Ambiguous`, donc echec de validation.
  - La relation est calculee une fois par modele dans l'index, puis publiee et revue dans le rapport de migration. Son hash est lie a la signature Gate A et l'admission le controle : divergence -> `GateAEvidenceStale`.
- **`StopLine` (P5).**
  - Authoree sur un controle `Stop`/`Yield` seulement si la map en porte l'intention. Dans `MVP_Run` : bord interieur (cote carrefour) du passage pieton de la branche d'un T.
  - Giratoires et croix n'en ont pas : repli explicite sur l'entree generique du mouvement (`s = 0`), dispose dans le rapport.
  - Le compilateur projette le segment en une abscisse `s_line` sur chaque mouvement controle. Un croisement absent, multiple ou hors de la plage du mouvement fait echouer la validation.
  - Une ligne n'est authoree que si l'empreinte maximale de Gate A (a_e incluse), arretee pare-chocs avant a `m_ctrl` de la ligne, garde une separation strictement positive avec tout mouvement en conflit.
- **Frontiere de controle.** b = `s_line` sur le premier mouvement de la traversee, sinon 0.
  - `d` (pare-chocs avant -> b), la cible et la fenetre d'arret de `JunctionEntry` (O13/O14 inchanges : arret a `m_ctrl` avant b), l'engagement (« entree franchie »), la tete de file et `D_request`/`D_engage`/`D_stop` visent b.
  - **Occupation et protection** : un acteur n'occupe le premier mouvement que si son pare-chocs avant a franchi max(0, b). Le troncon avant la ligne n'est ni occupation, ni engagement. La liberation (arriere au-dela) est inchangee.
  - `s0` n'est jamais un retrait a la ligne. La distance de freinage decide quand ralentir, jamais ou s'arreter.
- **Acceptation de creneau (P6)**, fonctions pures bornees en forme close.
  - Un demandeur R est refuse `Denied(YieldToPriority, P, zone)` si un acteur P dont la traversee approchee (la derniere de ses approches, valide ou `TooFar`, hors engagee) est incompatible avec celle de R et a preseance sur elle verifie `ETA_P < t_gap_R`.
  - `t_gap_R = t_clear_R + t_lat + m_gap`.
    - `t_clear_R` : temps de R, depuis (d_R, v_R), pour parcourir d_R + la longueur de traversee jusqu'a la fin de son dernier mouvement en conflit avec celle de P + L. Acceleration `a` du profil, vitesse plafonnee par mouvement a min(v0, √(a_lat/κmax)), precalcule par mouvement.
    - `ETA_P` : temps minimal de P jusqu'au debut de son premier mouvement en conflit. Acceleration `a` jusqu'a v0, sans autre plafond (borne conservative).
    - `m_gap` = `JunctionGapMarginSeconds` = 1,0 s, seule valeur declaree, avant mesure.
  - Un P refuse `ExitBlocked` au lot courant ne compte pas. Les paires de traversees pertinentes et leurs longueurs sont precalculees par modele.
- **`Yield`** : aucun arret obligatoire. Grant si creneau suffisant, sinon arret a la frontiere par `JunctionEntry`.
- **`Stop`**, deux conditions distinctes et ordonnees :
  1. arret marque : vitesse tangentielle ≤ `StopHaltSpeedMetersPerSecond` (0,05 m/s, declaree), pare-chocs dans la fenetre d'arret de b ;
  2. ensuite seulement, acceptation de creneau.
  - Avant 1 : `Denied(StopRequired)`. L'etat « arret marque » vit dans le coordinateur, avec la duree de vie de l'anciennete O2 : jamais de minuterie.
- **Ordre du lot, par demande.** sortie -> `StopRequired` -> occupant protege -> grant incompatible -> `YieldToPriority` -> reservation de l'ancien -> `Granted`.
  - La sortie est toujours evaluee avant toute priorite.
  - Une reservation d'ancien ne bloque jamais un plus jeune qui a preseance sur elle.
  - Un grant non engage n'est pas revoque pour priorite.
- **Briseur d'interblocage (P3), jamais a la place de la regle.** A la fin du lot, par carrefour, il agit seulement si toutes ces conditions sont reunies :
  - aucun grant tenu ou emis ;
  - aucun occupant protege ;
  - un ensemble W non vide de demandes valides refusees seulement `YieldToPriority` ;
  - chaque cause d'un membre de W est elle-meme dans W (aucune progression possible).
  - Il accorde alors au plus un grant : le plus ancien membre de W (`RequestSinceFrame`, puis `TrafficId`) qui passe sortie et conflits, raison `GrantedDeadlockBreak`. Compteur publie.
- **Performance (D13).**
  - Preseance, relation de droite, longueurs de degagement et paires de traversees : precalculees une fois par modele.
  - Par lot, seuls les acteurs du meme carrefour sont examines (seau construit une fois). Aucun balayage des 40 controles, 72 mouvements ou 136 zones par vehicule et par pas.
  - Compteurs dans `TrafficV2WorkCounters`. p95 du pas hote Traffic V2 < 10 ms a N = 8.
- **Raisons stables, `ToText()` invariant de culture, hote seul** : aucune `NetworkVariable`, RPC ni `OnValueChanged`.
- **Gate A (P1).**
  - Geometrie, seuils, ε_t, a_e, marges et preuves inchanges.
  - Le pipeline existant regenere le modele, le rapport de migration et les preuves sur la nouvelle version, puis HALT pour la re-signature par le proprietaire sur un nouveau record (l'ancien passe en historique).
  - Aucun test runtime contre `MVP_Run` n'est attendu vert avant cette signature.
- **PlayMode** : fixtures preparees et modifiees par l'agent, executees uniquement par le proprietaire. L'agent lance EditMode et controles statiques.

**Ask First:**

- **BLOQUANT avant `ready-for-dev` -- P9 (zones trop conservatrices).** L'audit identifie 16 paires (groupe A, axes >= 7 m) et 54 paires (groupe B, axes ~4 m, voies voisines) classees `ConservativeConflict` par `AutomatedPairDecisionPolicy.cs:491-497`, qui n'essaie jamais de prouver la separation d'une paire `Candidate`. Cause dans les donnees compilees (preuve Gate A), pas dans le coordinateur : HALT, voie de correction a decider par le proprietaire.
- **BLOQUANT avant `ready-for-dev` -- P10 (fusion d'anneau et invariant 5.34).** Le grant de traversee (O7) reserve toute la traversee ; « deux traversees incompatibles ne tiennent jamais de grants simultanes » (epics 5.34). Une entree servie devant un vehicule d'anneau engage mais lointain exige d'amender cet invariant : decision proprietaire.

- **P8 (tranchee le 2026-10-05, option a)** : `CandidateModel` inclut les controles, donc sa version change et `AutomatedPairDecisionPolicy.SameInputs` echoue. L'exception `5.50-AUTO-DECISIONS-v1` est etendue a la 5.35 par une proposition de changement de sprint redigee en phase 0 et approuvee par le proprietaire avant tout nouveau run, sous toutes ses conditions (build rules §6). Preuve exigee : a geometrie inchangee, chaque paire garde sa classification, sa raison et sa preuve ; seuls changent les identifiants de run et de revision. Toute classification differente : HALT. L'extension n'autorise ni la revue ni la signature de Gate A.
- Un verdict de preuve Gate A, une decision de paire ou un ensemble de raccords signes qui change.
- Un T dont l'instance ne correspond pas au prefab (axe traversant douteux), une relation `Ambiguous`, une `StopLine` sans separation positive, ou un `Stop` que la map semblerait justifier : remonter les elements, ne rien authorer.
- Revision de `m_gap`, `StopHaltSpeedMetersPerSecond`, `RightOfWayAmbiguityDegrees`, `m_ctrl` ou des formules ; toute hysteresis ou minuterie.
- Non-regression rouge (`Story531`, `Story552`, `Story533` A/B, `Story534` C/D) : HALT. Une assertion n'est modifiee que si la preseance authoree change legitimement l'issue, et par decision proprietaire (patron O11/O12).
- Budget D13 depasse, interblocage durable en campagne, `EnteredWithoutGrant > 0`, contact entre mouvements en conflit : HALT.
- Phase qui revele une decision architecturale independante, ou spec a scinder : HALT avant de scinder.

**Never:**

- Authorer un controle ou une ligne pour la couverture de code. Inferer une regle depuis un nom, un cycle ou un nombre de noeuds. Recalculer une preseance par vehicule et par pas. Recopier la relation de droite dans `road-authoring.json`.
- Un stop par minuterie. `s0` comme retrait a la ligne. Un briseur qui agit alors qu'une progression est possible.
- Signaux (5.36), escalade d'interblocage (5.40), exceptions de regle et Code de la route au-dela de ces regles (5.41), changement de voie, recuperation (5.39), optimisation de cout (5.46, AD-42).
- Modifier geometrie, trottoirs, seuils de preuve ou ε_t. Signer Gate A. Lancer un test PlayMode.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Priorite routiere | T : `Yield` (ancien) et `Priority` incompatibles, ETA_P < t_gap | `Priority` servi, `Yield` `Denied(YieldToPriority)` | N/A |
| Creneau suffisant | ETA_P ≥ t_gap_R | `Yield` servi sans arret | N/A |
| Priorite a droite | Croix, deux approches perpendiculaires incompatibles | L'approche de droite passe d'abord, ordres melanges identiques | N/A |
| Approches opposees | Croix, tourne-a-gauche face au tout-droit | Aucune preseance, departage 5.34 | N/A |
| Cycle de 4 | 4 demandes arretees, chacune cedant a sa droite | Un seul `GrantedDeadlockBreak` au plus ancien | Compteur |
| Pas de cycle | 3 demandes, cause d'un membre hors de W | Aucun briseur | N/A |
| Stop sans arret | `Stop`, v > 0,05 m/s dans la fenetre | `Denied(StopRequired)`, arret a b | N/A |
| Stop marque | Arret marque, creneau suffisant | Grant ; aucun temps d'attente minimal | N/A |
| Sortie et priorite | Sortie insuffisante et priorite | `ExitBlocked` evalue d'abord | N/A |
| P bloque en sortie | P `ExitBlocked` au lot | P ignore pour le creneau de R | N/A |
| Reservation | Ancien `Yield` en attente, jeune `Priority` | Le jeune n'est pas `SeniorRequestPending` | N/A |
| Ligne dans le mouvement | b = 6,4 m (`TJunction_West`, branche) | Arret pare-chocs a [`m_ctrl` − 0,02 ; fenetre] de b ; non occupant, non engage avant b | N/A |
| Sans ligne | Entree de giratoire | b = 0, comportement 5.34 | Disposition au rapport |
| Rotation | Carrefour synthetique tourne de 0 a 345° par 15°, et miroir | Relation invariante en rotation, gauche/droite echangees en miroir | `Ambiguous` dans la bande |
| Traversees compatibles | Meme carrefour, aucune `ConflictZone` commune (ex. tout-droit et virage qui ne se croisent pas, opposes compatibles) | Grants simultanes, aucun refus, aucune priorite evaluee | N/A |
| Anneau, avant la sortie precedente | Vehicule d'anneau dont la traversee sort a la sortie precedente | Ne bloque pas l'entree | N/A |
| Anneau, avant la sortie precedente | Meme position, traversee qui continue vers la fusion | Bloque seulement si ETA < t_gap | N/A |
| Anneau, entre sortie precedente et entree | Continuation vers la fusion | Bloque si ETA < t_gap, sinon entree servie | N/A |
| Anneau, proche de la fusion | Occupant ou ETA court | L'entrant attend | N/A |
| Anneau, apres l'entree | Fusion depassee, mouvement libere | Ne bloque pas | N/A |
| Anneau, secteur oppose | ETA vers la fusion ≥ t_gap | Entree servie | N/A |
| Plusieurs vehicules d'anneau | Aucun ne menace le creneau | Entree servie sans arret | N/A |
| Melange interdit | `Uncontrolled` + `Yield` dans un carrefour | Validation en echec | Diagnostic |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs`
  - `:69-76` `ControlDecision` (Id, `ApproachKey`, `Kind`), sans ligne ;
  - `:150` `FormatVersion = 4`, a porter a 5 avec une `StopLine` optionnelle par controle ;
  - `:204-227` lecture, `:340-346` ecriture ;
  - `:160-180` dispositions : `Ligne` -> `NotRequiredForCurrentControlKind`, 9 taches « reouverture 5.35 ».
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs`
  - `:259-278` `ApplyDecisions` puis `CandidateModel` compile **avec controles** (P8) ;
  - `:389-425` construction des `JunctionControl` ; `:400-405` refuse tout genre sauf `Uncontrolled` (« relevent de la Story 5.35 ») ;
  - `:1561-1580`, `:1805`, `:1861` rapport (« tous `Uncontrolled` », lignes) ;
  - `:2081-2125` signoff (format 3).
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AutomatedPairDecisionPolicy.cs:60-95,665-673` -- `SameInputs` compare `ModelVersion` du candidat (P8).
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/GateAEvidenceRegeneration.cs:60,208,327` -- `Regenerate`, `RenderReport`, `RenderDiff` (chemin 5.52).
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs:1131-1132` -- taches `Controle` et `Ligne`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs:265-272,489-501` -- `JunctionControlKind`, `JunctionControl` (`HasStopLine`, `StopLine`).
  - Deja dans la charge canonique (`RoadModelCanonicalWriter.cs:255-265`), le document (`RoadModelDocument.cs:311,486`) et le validateur (`RoadModelValidator.cs:425-428`, finitude seule ; `:488`).
  - `CompiledRoadModel.cs:151-161` : ajouter `s_line` par mouvement controle, compile.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/GateAEvidenceBinding.cs:57-122` -- `ClearanceHash` ne lie que le bloc « Residus ». Ajouter le hash de relation de droite au format de signoff et a la verification. `Lifecycle/TrafficV2Composition.cs:325-345` : `Admit`.
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json` -- 40 `Controls` (`ApproachKey`, `Kind`), 9 dispositions `Ligne`. `MVP_Run.road-model.json` (`v4:46e91057…`), `.road-signoff.json`, `.road-signoff-history.json`.
- Topologie des controles (verifiee le 2026-10-05) :
  - croix : 4 controles de 3 mouvements ;
  - T : 3 controles de 2 mouvements (`FromWest`, `FromEast`, `FromSouth`) ;
  - giratoires : 3 d'entree (`Connector_*_In`, 1 mouvement) et 3 d'anneau (`Ring_Split_*`, continuation + sortie).
  - Scenario C : `40ca7f10` = `FromEast` (axe, `Priority`), `4e437f94` = `FromSouth` (branche, `Yield`).
- `Assets/RoadRage/Prefabs/Greybox_TJunction.prefab`, `Greybox_Intersection.prefab`, `Greybox_Roundabout.prefab` -- aucun panneau ni marquage. T : `Sidewalk_North_*` continu (axe O-E), tuiles `Crossing_*` de 4 m. Giratoire : aucun passage pieton. Ligne de `TJunction_West`, branche : x = −30 (constat 5.34).
- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/`
  - `JunctionConflictIndex.cs:16-180` (cache par modele, `IncompatibleWith`, `TryGetConflict`) : y precalculer preseance, relation de droite, paires de traversees et longueurs ;
  - `JunctionCoordinator.cs:74` (lot), `:118` (etape 0), `:234-275` (etape 2, ordre a etendre), `:336` (engagement), `:350` (sortie) ;
  - `JunctionRequestBuilder.cs:42-143`. `:82-110` : approches et `d = start[k0] - front`, a viser vers b ; la derniere approche est presente meme `TooFar`. `:117-133` : statuts d'occupation, a decaler a b ; `:225` : tete de file ;
  - `JunctionRecords.cs:20-45` (`JunctionReason`, a etendre par ajout), `:163-177` (`JunctionApproach`) ;
  - `JunctionDistances.cs:78-120` (`D_stop`, `D_engage`, fenetre O14).
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs:128-150,454-520` -- `JunctionEntryInput` (distance a b, inchange sinon).
- Tests :
  - `Tests/EditMode/Story534JunctionCoordinatorTests.cs`, `Story534JunctionEntryTests.cs:550` (degagement a `m_ctrl`), `:801` (vehicule seul, 11 routes) ;
  - `Tests/PlayMode/Story534JunctionPlayModeTests.cs:229,347` (C, D), `Story533Harness.cs`, `Story533ExplorationPlayModeTests.cs`, `Story533PerformanceDiagnosticPlayModeTests.cs` ;
  - `traffic-v2-5-34-explorations/scenarios-5-34.json`.
- `_bmad-output/implementation-artifacts/deferred-work.md` -- entrees 5.34 « StopLine » et « Constats 5.35 » (a solder) ; marge D13, flotte de 30, dette route+horizon 5.31, determinisme PlayMode (rouvertes par la Gate C).

## Tasks & Acceptance

**Execution** -- cinq phases, chacune fermee par un checkpoint vert, sinon HALT.

*Phase 0*
- [ ] `_bmad-output/planning-artifacts/epics.md` -- amender la 5.35 (index L, fiche, revues) : P1-P8, `Stop` en fixture synthetique.
- [ ] `_bmad-output/planning-artifacts/sprint-change-proposal-2026-10-05.md` (nouveau), `docs/setup/build-workflow-rules.md` §6 -- extension P8 de l'exception 5.50 a la 5.35, sous toutes ses conditions. Approbation proprietaire avant la phase 2.

*Phase 1 -- authoring, schema, compilation*
- [ ] `AuthoringDecisions.cs` -- format 5, `StopLine` optionnelle par controle, nouvelle disposition `Ligne` « repli entree generique ».
- [ ] `AuthoredRoadModel.cs` -- genres `Priority`/`Yield`/`Stop`/`Uncontrolled` admis, `Signalized` refuse ; melange `Uncontrolled` refuse ; lignes reportees ; rapport mis a jour (genres, lignes, replis, relation de droite et son hash).
- [ ] `RoadModelValidator.cs`, `CompiledRoadModel.cs` -- projection `s_line` par mouvement, croisement unique.
- [ ] `Junction/RightOfWay.cs` (nouveau, pur) -- relation versionnee, secteurs, bande `Ambiguous`.
- [ ] `MVP_Run.road-authoring.json` -- genres P2, lignes des 4 branches de T, dispositions. Avant d'ecrire : verification d'instance des 4 T et separation positive de chaque ligne, publiees.
- [ ] `Tests/EditMode/Story535ControlAuthoringTests.cs` `[Core][Story535]` -- format 5, refus (melange, `Signalized`, ligne sans croisement), projection, relation sur rotations et miroir, `Ambiguous`, fait de modele P2, separation des lignes.
- Checkpoint 1 : `Story535` EditMode vert ; extension P8 approuvee.

*Phase 2 -- Gate A*
- [ ] Regenerer modele, rapport, preuves et diff sur la nouvelle version, par le chemin 5.52, avec le run de paires autorise par P8. Verdicts, classifications de paires et raccords identiques, sinon HALT.
- [ ] `GateAEvidenceBinding.cs`, signoff de `AuthoredRoadModel.cs`, `TrafficV2Composition.cs` -- hash de relation de droite lie et verifie a l'admission.
- Checkpoint 2 : **HALT, re-signature proprietaire.**

*Phase 3 -- regles pures*
- [ ] `JunctionConflictIndex.cs` -- precalculs par modele, compteur de construction.
- [ ] `JunctionPriority.cs` (nouveau, pur) -- `t_clear`, `ETA`, `t_gap`, preseance de traversees.
- [ ] `JunctionCoordinator.cs`, `JunctionRecords.cs` -- ordre du lot, `StopRequired`, `YieldToPriority`, reservation filtree, arret marque, briseur, compteurs et raisons par ajout.
- [ ] `Tests/EditMode/Story535JunctionRulesTests.cs` `[Core][Story535]` -- la matrice, plus :
  - fixture synthetique `Stop` ;
  - permutations priorite routiere et priorite a droite ;
  - arrivees simultanees sous ordres melanges ;
  - cas limites de `t_gap`/`ETA` (v = 0, plafond, distances nulles) ;
  - aucun service avant creneau ;
  - travail par lot independant de la taille du modele ;
  - scan `Junction/` (liste 5.34).
- Checkpoint 3 : `Story535` et `Story534` EditMode verts.

*Phase 4 -- branchement runtime*
- [ ] `JunctionRequestBuilder.cs`, `JunctionDistances.cs`, `TrafficV2VehicleDriver.cs`, `Debug/TrafficDecisionProjection.cs`, `TrafficV2WorkCounters.cs` -- b, occupation decalee, champs de trace (genre, b, t_gap, ETA, cause, arret marque).
- [ ] `Tests/EditMode/Story535StopLineTests.cs` `[Core][Story535]` -- cas `TJunction_West` : arret dans la bande de la ligne et non ~7 m avant, non occupant avant b, reprise ; vehicule seul sur les 11 routes, aucun `JunctionEntry` lie ; pire cas 5.34 vers b.
- Checkpoint 4 : `Story535`, `Story534`, `Story533` et `Story531` EditMode verts.

*Phase 5 -- PlayMode prepare, Gate C*
- [ ] `traffic-v2-5-35-explorations/scenarios-5-35.json` et `Tests/PlayMode/Story535JunctionPlayModeTests.cs` `[Story535]` -- trois scenarios :
  - E : rencontre ZC23, la branche cede, arret a la ligne, puis service ;
  - F : priorite a droite a la croix ;
  - G : entree de giratoire face a la circulation.
- [ ] Campagne Gate C `[Explicit]` (`Story535GateC`) a N = 2, 4 et 8 : croix, ≥ 1 T, ≥ 1 giratoire ; cout par frontiere publie (frame, coordinateur, priorite, pas vehicule) ; adaptation des verdicts `Story533Exploration`.
- [ ] `deferred-work.md`, `sprint-status.yaml` (5.35 seulement) -- solder « StopLine » et « Constats 5.35 », reevaluer les dettes rouvertes avec mesure, puis `graphify update .`.

**Acceptance Criteria:**

- Given les scenarios E, F et G executes par le proprietaire, when ils tournent :
  - E : `Denied(YieldToPriority)` cite le vehicule d'axe ; la branche s'arrete dans la bande de sa `StopLine` ; puis service et deux sorties ;
  - F : l'approche de droite passe d'abord ;
  - G : l'entree attend la liberation de l'anneau.
  - Dans les trois : aucun contact, aucun `TrackingToleranceExceeded`, `EnteredWithoutGrant` = 0.
- Given la campagne Gate C a N = 2, 4 et 8, when elle tourne, then :
  - tous les vehicules atteignent un portail de sortie, sans interblocage durable ni retrait, teleportation ou reinsertion ;
  - aucune entree sans place en sortie ;
  - p95 du pas hote < 10 ms a N = 8 ;
  - briseurs et cout par frontiere publies, observationnels.
- Given `Story531`, `Story552`, `Story533` A/B et `Story534` C/D rejoues apres re-signature, when ils tournent, then ils restent verts, sauf decision proprietaire consignee.
- Given deux traversees d'un meme carrefour sans `ConflictZone` commune (croix, T et giratoires de `MVP_Run` apres P9, et fixture synthetique : tout-droit et virage qui ne se croisent pas, mouvements opposes compatibles), when elles demandent au meme lot, then elles recoivent des grants simultanes ; aucun refus, aucune preseance ni creneau n'est evalue entre elles ; une recherche exhaustive sur toutes les paires compatibles de `MVP_Run` le prouve.
- Given une entree `Yield` de giratoire et un vehicule d'anneau place tour a tour avant la sortie precedente, entre la sortie precedente et l'entree, proche de la fusion, apres l'entree et dans le secteur oppose, when l'entrant demande, then le verdict depend seulement de l'incompatibilite de la trajectoire restante et de l'ETA vers la zone de fusion (matrice), jamais de l'occupation globale de l'anneau. Plusieurs vehicules d'anneau qui ne menacent pas le creneau ne bloquent pas, et sans vehicule prioritaire pertinent l'entree se fait sans arret. Couverture EditMode sur `Roundabout_SouthWest` reel et PlayMode (scenario G etendu).
- Given toute fixture `Story535`, when la suite tourne, then chaque grant, refus, revocation et liberation porte une raison stable.

## Spec Change Log

## Design Notes

**Decisions proprietaires du 2026-10-05.**
- **P1** Gate A re-signee par le proprietaire apres regeneration a geometrie inchangee.
- **P2** Authoring = verite de la route.
  - Prefabs : aucun panneau ni marquage. T : trottoir nord continu, d'ou l'axe O-E traversant et la branche Sud.
  - Pas de `Stop` dans `MVP_Run` ; rien dans le decor ne le justifie a `TJunction_West`.
- **P3** `Uncontrolled` = priorite a droite ; le briseur n'agit que si aucune progression n'est possible.
- **P4** Relation derivee de la geometrie et compilee, jamais authoree. Conflits de meme rang : 5.34, frontiere 5.41.
- **P5** `StopLine` authoree si l'intention est claire, sinon repli explicite. Ligne de controle distincte de l'occupation.
- **P6** Creneau = degagement + `t_lat` + marge declaree. Stop = arret marque, puis creneau ; Yield sans arret obligatoire.
- **P7** L, cinq phases, PlayMode execute par le proprietaire.
- **Ajout proprietaire du 2026-10-05 (avant implementation) :** compatibilite reelle des mouvements et priorite locale d'anneau par creneau ; ni l'une ni l'autre n'est reportee a la 5.41.
- **P8** Option a : exception `5.50-AUTO-DECISIONS-v1` etendue a la 5.35 par proposition de changement ; classifications identiques exigees a geometrie inchangee.

**Preseance par controle d'approche, pas par paire de mouvements.** Une traversee de giratoire contient une entree (`Yield`) et des continuations (`Priority`). Comparer paire par paire rendrait deux traversees d'entrees differentes mutuellement prioritaires (contradiction). Le controle d'approche est la regle reelle : on cede la ou l'on entre. La priorite de l'anneau s'exerce sur les vehicules deja engages, que la 5.34 protege deja.

**Pourquoi lier la relation de droite a la signature.** Elle n'entre pas dans `RoadModelVersion` : elle est derivee, comme l'index. Sans lien, une modification de la fonction changerait le comportement sous une signature inchangee.

**Exemple (profil par defaut, Δt = 0,02 s), a titre indicatif.** Branche arretee (v = 0), traversee de ~12 m jusqu'a la fin du dernier mouvement en conflit, L = 4,44 m, a = 1,5 m/s² : t_clear ≈ √(2·16,4/1,5) ≈ 4,7 s, donc t_gap ≈ 4,7 + 0,04 + 1,0 ≈ 5,7 s. Un vehicule d'axe a 8 m/s cede le creneau au-dela d'environ 45 m.

**Hypotheses non verifiees.**
- P8 : effet reel de `SameInputs` sur la regeneration (refus ou nouveau run), a mesurer en phase 1 ; l'identite des classifications a geometrie inchangee est attendue, pas prouvee.
- Le plafond √(a_lat/κmax) suppose a_lat disponible par profil ; sinon HALT.
- Le debit de la croix sous priorite a droite et la frequence du briseur ne sont pas mesures.
- Le cout par vehicule du creneau n'est pas mesure, avec une marge D13 de 0,7 a 1,7 ms.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.34 -TestMode EditMode`, puis `5.33` et `5.31` -- expected: verts.

**Manual checks (proprietaire):**
- Re-signature Gate A sur le nouveau record (phase 2).
- `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode PlayMode` puis `-TestFilter Story535GateC -TestFilterType category -IncludeExplicit` -- E/F/G et Gate C verts, rapports ecrits ; rejeux 5.31, 5.52, 5.33, 5.34 verts.
