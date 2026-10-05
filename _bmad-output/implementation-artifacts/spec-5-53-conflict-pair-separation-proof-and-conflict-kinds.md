---
title: 'Story 5.53 -- Classification des paires de conflit : preuve de separation raffinee des ConservativeConflict et typage croisement / fusion'
type: 'bugfix'
created: '2026-10-05'
status: 'ready-for-dev'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '{project-root}/_bmad-output/implementation-artifacts/traffic-v2-5-35-explorations/conflict-zone-audit-20261005.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** L'audit du 2026-10-05 trouve 94 des 136 `ConflictZones` de `MVP_Run` classees `ConservativeConflict` (`continuous-contact-possible`).
- La borne continue du balayage y autorise un contact, aucun temoin ne le confirme, et la politique 5.50/5.52 s'arrete la sans raffiner.
- Consequence : dans chaque T, toutes les paires inter-approches sont en conflit, deux tout-droit opposes compris (axes a 4,0 m). En giratoire, une entree est en conflit avec l'entree d'un autre bras (7,38 m) et la sortie de son propre bras (3,99 m).
- La coordination 5.34 rend donc ces carrefours quasi exclusifs, et aucune regle 5.35 ne peut etre juste sur ce modele.

**Approach:** Une nouvelle version de la politique deterministe raffine les paires `ConservativeConflict` pour tenter une preuve de separation stricte, sur la meme geometrie gonflee et la meme rigueur. Le modele compile type chaque zone acceptee (`Crossing` strict ou `Merge` prouve) avec son abscisse de debut de contact par membre. Toute paire non prouvee reste un conflit. Prerequis de la 5.35.

## Boundaries & Constraints

**Always:**

- **Perimetre (decision proprietaire P9 du 2026-10-05)** : modele et preuve Gate A seulement. Aucun Stop/Yield/Priority, aucun controle, aucune regle runtime.
- **Exception §6 etendue.** Elle est etendue a la 5.53 (et a la 5.35, P8) par `sprint-change-proposal-2026-10-05.md`, approuvee par le proprietaire avant tout run. Toutes ses conditions s'appliquent :
  - fonction deterministe, versionnee, entrees epinglees ;
  - chaque decision porte sa `PairKey`, sa revision, son empreinte, son motif, sa preuve, `RoadModelVersion` et les versions du balayage et de la politique ;
  - manifeste complet ;
  - ecriture transactionnelle ; une seconde execution ne propose aucun changement.
- **Politique v3** (`DecisionPolicyVersion` par ajout ; v1/v2 restent lisibles), seulement pour une paire `Candidate` sans temoin :
  - Subdivision des intervalles de poses des combinaisons ou la borne autorise un contact, avec la machinerie de `ConflictSweep`, sans rien changer :
    - inflation (demi-gabarit, marge, a_e) ;
    - ensemble de poses cinematiques et grille d'offsets ;
    - formules de restes ;
    - `ProofToleranceMeters`.
  - `ProvenDisjoint` seulement si chaque feuille a une borne de separation strictement superieure a la tolerance (critere `ProvenDisjoint` existant).
  - Un temoin de recouvrement trouve en route -> `ConflictProven`.
  - Sinon, ou si le budget declare `RefinementLeafBudget` est epuise -> `ConservativeConflict` inchange, motif publie.
- **Invariance.** Les paires `ConflictProven`, `ProvenDisjoint` et `FailClosed` gardent leur classification, leur raison et leur preuve. Seules les `ConservativeConflict` peuvent changer, et seulement vers `ProvenDisjoint` ou `ConflictProven`.
- **Typage compile** (`ConflictZone.Kind`, `CompilerSchemaVersion` 5).
  - `Merge` seulement si les deux mouvements partagent le corridor de depart et que la separation stricte est prouvee (meme critere) pour toute combinaison ou l'un des deux est avant son abscisse de debut de contact. Le contact n'est alors possible que dans la convergence.
  - Sinon `Crossing`. Toute paire non prouvee, conservative ou ambigue -> `Crossing`.
  - Chaque membre porte `ContactStartSMeters`, abscisse du point de reference ou le contact devient possible, dans [0, L]. Elle sert de cible d'ETA a la 5.35.
- **Rapport et diff.** Le rapport de migration publie :
  - le diff complet paire par paire, avant / apres : classification, motif, borne de separation, feuilles, genre, `ContactStartSMeters` ;
  - les compteurs `ConservativeConflict -> ProvenDisjoint`, `-> ConflictProven`, restantes ;
  - les 42 `ConflictProven` inchangees ;
  - la table des paires de traversees compatibles par carrefour.
- **Regeneration Gate A** par le chemin 5.52 : modele, rapport, quatre preuves et diff, geometrie inchangee. La signature suit la decision du checkpoint (Design Notes).
- Les faits de modele codes en dur dans les tests 5.34 (nombre de zones, paires compatibles) sont mis a jour depuis le modele, sans toucher aux assertions de comportement.

**Ask First:**

- Toute modification d'inflation, de a_e, d'ε_t, de marge, de restes, de tolerance, de profil ou de geometrie. Une revision de `RefinementLeafBudget`.
- Une paire `ConflictProven`, `ProvenDisjoint` ou `FailClosed` qui changerait ; une paire qui passerait de disjointe a conflit.
- Une paire de voies opposees en ligne droite, ou une paire du groupe A de l'audit, qui reste `ConservativeConflict` : publier le motif et HALT avant la 5.35, jamais d'exception.
- Tout run avant approbation de la proposition de changement.

**Never:**

- Classer une paire a la main ou par jugement LLM. Ajouter une exception runtime. Toucher au coordinateur, aux controles, a Stop/Yield/Priority (5.35).
- Signer Gate A. Lancer un test PlayMode.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Voies opposees droites | Axes paralleles a 4,0 m (fixture) | `ProvenDisjoint`, aucune zone | N/A |
| Croisement reel | Axes qui se croisent | `ConflictProven`, `Crossing` | N/A |
| Fusion | Meme depart, pas de croisement en amont | `Merge`, `ContactStartSMeters` par membre | N/A |
| Fusion qui croise avant | Meme depart, contact possible en amont de la convergence | `Crossing` | N/A |
| Cas limite | Separation exactement a la tolerance | Reste `ConservativeConflict` | Motif publie |
| Budget epuise | Feuilles > `RefinementLeafBudget` | Reste `ConservativeConflict` | Motif `refinement-budget` |
| Rejeu | Seconde execution, entrees identiques | Aucun changement propose | N/A |
| Ordre melange | Paires et poses permutees | Decisions identiques | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs`
  - `:47-62` `PairRelation` ; `:77-88` `EnvelopeSelected`, `ExactSlackMeters`, `EnvelopeSlackMeters` ;
  - `:701-752` `Evaluate` (a_e, `HypothesisFailure`, `Candidate` si `ExactProven`) ; `:760+` `EvaluatePaths` ; `:800-835` bornes et restes ;
  - `:227` `AlgorithmVersion = 1`. Le raffinement se greffe ici, sans modifier les bornes existantes.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AutomatedPairDecisionPolicy.cs`
  - `:29-34` versions, tolerance, profondeur ;
  - `:60-127` plan et manifeste ; `:446-560` `Classify` (`:483` temoin, `:491-497` `continuous-contact-possible`, `:499-515` `ProvenDisjoint`) ;
  - `:564-572` `ContactWitness` ; `:665-673` `SameInputs` (version du candidat).
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs`
  - `:191-197` chemins (decisions, modele, signoff, rapport, diff) ;
  - `:259-278` `CandidateModel` ; `:2312-2318`, `:2377-2404` plan et proposition ;
  - `:1550-1720` rapport ; `:2081-2125` signoff.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs:99,252,363,690` -- `ConflictDecision`, ajout du genre et des `ContactStartSMeters`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs` (`ConflictZone`), `CompiledRoadModel.cs:166-180`, `RoadModelCanonicalWriter.cs`, `RoadModelDocument.cs`, `RoadModelValidator.cs`, `RoadModelCompiler.cs:34` (`CompilerSchemaVersion = 4`) -- genre et abscisses par ajout, dans la charge canonique.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/GateAEvidenceRegeneration.cs:60,208,327` -- regeneration et diff (chemin 5.52). Precedent : `_bmad-output/implementation-artifacts/gate-a-5-52/`.
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json` -- 136 `Conflicts` acceptes : 94 `ConservativeConflict`, 42 `ConflictProven`, tous en politique v2.
- `_bmad-output/implementation-artifacts/traffic-v2-5-35-explorations/conflict-zone-audit-20261005.md` -- groupes A (16), B (54), C (24), P (42), liste des zones.
- `Assets/RoadRage/Tests/EditMode/Story534JunctionCoordinatorTests.cs` -- faits de modele `MVP_Run` (zones, paires de T) a deriver du modele.

## Tasks & Acceptance

**Execution:**

*Phase 0*
- [ ] `_bmad-output/planning-artifacts/sprint-change-proposal-2026-10-05.md` (nouveau), `docs/setup/build-workflow-rules.md` §6 -- insertion de la 5.53 avant la 5.35 ; extension de l'exception 5.50 a la 5.53 (politique v3) et a la 5.35 (P8), sous toutes ses conditions. HALT pour approbation.
- [ ] `_bmad-output/planning-artifacts/epics.md`, `sprint-status.yaml` -- fiche 5.53, chemin critique 5.34 -> 5.53 -> 5.35, table des revues.

*Phase 1 -- politique et typage*
- [ ] `ConflictSweep.cs`, `AutomatedPairDecisionPolicy.cs` -- raffinement borne, v3, motifs `refinement-budget` et `refined-disjoint`, invariance des autres classifications.
- [ ] Schema : `RoadModelRecords.cs`, `CompiledRoadModel.cs`, `RoadModelCanonicalWriter.cs`, `RoadModelDocument.cs`, `RoadModelValidator.cs`, `RoadModelCompiler.cs`, `AuthoringDecisions.cs` -- `Kind`, `ContactStartSMeters`, version 5, validation (`Merge` exige un depart commun ; abscisses dans [0, L]).
- [ ] `Tests/EditMode/Story553ConflictClassificationTests.cs` `[Geometry][Story553]` -- la matrice ; determinisme sous ordres melanges ; seconde execution sans changement ; invariance des classes non conservatives ; typage sur fixtures synthetiques.
- Checkpoint 1 : `Story553` EditMode vert.

*Phase 2 -- regeneration et diff*
- [ ] Run v3 sur `MVP_Run` par le chemin 5.52. Puis `traffic-v2-5-53-classification/diff-paires-*.md` et `regeneration-*.md` (nouveaux) : diff complet, compteurs, table des compatibles. Mettre a jour l'audit.
- [ ] `Story534JunctionCoordinatorTests.cs` -- faits de modele derives du modele.
- [ ] `Tests/EditMode/Story553MvpRunClassificationTests.cs` `[Geometry][Story553]` -- 42 `ConflictProven` inchangees ; paires du groupe B en ligne droite et du groupe A resolues ou publiees ; table des compatibles egale au rapport.
- Checkpoint 2 : `Story553` et `Story534` EditMode verts. **HALT : revue proprietaire du diff paire par paire, puis decision de signature.**
- [ ] `deferred-work.md`, `sprint-status.yaml` (5.53 seulement), `graphify update .`.

**Acceptance Criteria:**

- Given les 94 `ConservativeConflict` de `MVP_Run`, when la politique v3 s'execute, then chacune est soit prouvee disjointe (separation strictement positive sur chaque feuille), soit `ConflictProven` (temoin), soit conservee avec son motif. Le nombre de passages a `ProvenDisjoint` est publie.
- Given les 42 `ConflictProven`, when la v3 s'execute, then aucune ne change de classification, de raison ou de preuve.
- Given un second run sur les memes entrees, when il s'execute, then aucun changement n'est propose.
- Given le modele compile, when il est valide, then chaque zone porte un genre : `Merge` uniquement sur preuve de fusion a depart commun, sinon `Crossing`, avec des `ContactStartSMeters` valides.
- Given la regeneration, when elle s'acheve, then le diff complet paire par paire, les compteurs et la table des traversees compatibles par carrefour sont publies, sans aucune classification manuelle.

## Spec Change Log

## Design Notes

**Signature Gate A : proposition pour le checkpoint 2 (demande du proprietaire du 2026-10-05).**

*Faits :*
- Le modele change, donc l'admission V2 passe en `GateAEvidenceStale` : aucun vehicule V2 ne roule dans `MVP_Run` jusqu'a une signature (fail-closed).
- Les phases 3 et 4 de la 5.35 (regles et runtime) suivent sa phase 2 (signature). Juste apres une signature combinee, le runtime est donc encore celui de la 5.34.
- Le coordinateur 5.34 ne lit ni genres, ni lignes, ni `Kind`.

*Option recommandee, signature differee et combinee a la phase 2 de la 5.35.* Les preuves ne sont pas affaiblies, a quatre conditions :
1. Le proprietaire revoit le diff 5.53 a son checkpoint 2.
2. La 5.53 reste en `review` jusqu'a la signature combinee.
3. Juste apres cette signature, avant toute phase 3 de la 5.35, le proprietaire rejoue les PlayMode 5.31, 5.52, 5.33 et 5.34 (C, D, campagne). C'est la non-regression runtime de la 5.53, attribuable sans melange.
4. Rouge -> HALT impute a la 5.53.

*Cout :* la 5.53 n'a aucune preuve runtime avant la 5.35, et un defaut de zone n'apparait qu'au rejeu combine.

*Alternative :* une signature dediee a la 5.53, attribution immediate, un acte proprietaire de plus. Si le proprietaire refuse la combinaison : HALT et signature dediee.

**Pourquoi typer ici.** La 5.35 n'accorde `GrantedMergeGap` qu'a une paire reconnue comme fusion par le modele compile (P10). Le typage est un resultat de la meme preuve geometrique. Le produire en runtime serait une inference de geometrie interdite.

**Hypotheses non verifiees.** Le nombre de paires que le raffinement prouvera disjointes est inconnu : la subdivision reduit les restes, mais une paire de virages voisins peut rester sous la tolerance. Le cout du raffinement par paire n'est pas mesure, d'ou le budget.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.53 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.34 -TestMode EditMode` -- expected: vert, comportements inchanges.

**Manual checks (proprietaire):**
- Revue du diff paire par paire au checkpoint 2, puis decision de signature (combinee ou dediee).
