---
title: 'Story 5.53 -- Classification des paires de conflit : preuve de separation raffinee des ConservativeConflict et typage croisement / fusion'
type: 'bugfix'
created: '2026-10-05'
status: 'in-progress'
baseline_commit: 'b8e7165c639c27f86d907cb27e25c6c1faaeef7a'
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
- [x] `_bmad-output/planning-artifacts/sprint-change-proposal-2026-10-05.md` (nouveau), `docs/setup/build-workflow-rules.md` §6 -- insertion de la 5.53 avant la 5.35 ; extension de l'exception 5.50 a la 5.53 (politique v3) et a la 5.35 (P8), sous toutes ses conditions. HALT pour approbation.
- [x] `_bmad-output/planning-artifacts/epics.md`, `sprint-status.yaml` -- fiche 5.53, chemin critique 5.34 -> 5.53 -> 5.35, table des revues.

*Phase 1 -- politique et typage*
- [x] `ConflictSweep.cs`, `AutomatedPairDecisionPolicy.cs` -- raffinement borne, v3, motifs `refinement-budget` et `refined-disjoint`, invariance des autres classifications.
- [x] Schema : `RoadModelRecords.cs`, `CompiledRoadModel.cs`, `RoadModelCanonicalWriter.cs`, `RoadModelDocument.cs`, `RoadModelValidator.cs`, `RoadModelCompiler.cs`, `AuthoringDecisions.cs` -- `Kind`, `ContactStartSMeters`, version 5, validation (`Merge` exige un depart commun ; abscisses dans [0, L]).
- [x] `Tests/EditMode/Story553ConflictClassificationTests.cs` `[Geometry][Story553]` -- la matrice ; determinisme sous ordres melanges ; seconde execution sans changement ; invariance des classes non conservatives ; typage sur fixtures synthetiques.
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

- **2026-10-05 -- Phase 0 approuvee par le proprietaire** (`sprint-change-proposal-2026-10-05.md`, section 8). Precisions qui s'imposent a l'implementation, sans modifier le bloc fige :
  - **Vocabulaire.** « Corridor de depart » du bloc fige se lit **corridor aval commun** : le corridor dans lequel les deux mouvements convergent (`ToCorridorId` identique). Nom retenu dans le code et les rapports : `CommonExitCorridor`. Un corridor d'entree partage est un suivi (`SameApproach`), jamais un `Merge`.
  - **Critere `Merge` renforce.** Toutes ces proprietes doivent etre prouvees : meme `CommonExitCorridor` ; pour chaque membre, l'ensemble des poses ou un contact devient possible (feuilles non prouvees apres raffinement complet) forme un seul intervalle terminal ; cet intervalle atteint la fin du mouvement ; aucun autre intervalle de contact possible en amont. Une sequence contact possible -> separation -> convergence reste `Crossing`. Les deux `ContactStartSMeters` sont derives de la preuve et publies. Ambiguite, budget epuise ou preuve incomplete : `Crossing`, `ContactStartSMeters = 0`.
  - **Budget.** `RefinementLeafBudget` = 65 536 feuilles par paire, plafond dur ; `MaxSubdivisionDepth` = 20 inchange. Arret des qu'une preuve definitive est obtenue. Publier par paire les feuilles consommees, et au global total, mediane, p95, maximum, duree totale et memoire allouee si mesurable. Cout pathologique : HALT avant toute modification du budget. Budget epuise sans preuve : conservateur, jamais `ProvenDisjoint`.
  - **Implementation phase 1 (2026-10-05), ecarts a valider au checkpoint 2 :**
    - *Ordre de subdivision.* La precision 5.1 de la proposition disait « dyadic-a-then-b, alterne ». Mesure en fixture : l'alternance subdivise un intervalle deja fin (0,1 m) autant que l'intervalle large (20 m), et le cout depend de l'ordre des membres (M4 x M1 epuise le budget, M1 x M4 non), ce qui viole la ligne « ordre melange » de la matrice. Retenu : dyadique, cote au plus grand delta d'abord, A a egalite (`dyadic-larger-delta-first`, publie dans le manifeste v3). Budget et profondeur inchanges.
    - *Feuille temoin.* Le premier temoin fixe la classification `ConflictProven`. Une feuille temoin dont le terme d'intervalle (delta_A/2 + delta_B/2) depasse encore 2 x gonflement est subdivisee pour localiser le contact (typage), dans le meme budget ; sinon elle est terminale. Sans cela, une feuille de 10 m projetait tout son segment et le debut de contact tombait a 0. Le temoin utilise la tolerance fixe de `ContactWitness`, jamais la tolerance de preuve passee au raffinement.
    - *Schema 4 relisible.* `CompilerSchemaVersion` = 5 ; le document passe au format 3 (corps type). Le format 2 / schema 4 reste lisible, jamais ecrit, pour que les preuves historiques signees (`gate-a-5-52/historical-signed-5-51/`, utilisees par les tests 5.28, 5.30, 5.31, 5.52) gardent leur version liee. Consequence : le rapport de migration 5.27 (liaison `compiler-schema-version`) est regenere en phase 2, sans autre changement.
  - **Campagne `Story553Regeneration` du 2026-10-05 (diagnostic, rien n'est ecrit) -- HALT proprietaire.** Sorties : `traffic-v2-5-53-classification/diff-paires-20261005-163022.md` et `regeneration-20261005-163022.md`.
    - Verts : determinisme (deux plans identiques, run `4b271e1f...`), 42/42 `ConflictProven` inchangees, aucune autre classification changee.
    - 94 `ConservativeConflict` -> 22 `ProvenDisjoint`, 22 `ConflictProven`, 50 restees (`refinement-budget`, toutes). Voies opposees en ligne droite : resolues. Rouge : 8 zones du groupe A (giratoires, sortie d'anneau contre continuation ou sortie) restees conservatives.
    - Cout pathologique : 136 paires raffinees, mediane = p95 = max = 65 536 feuilles (total 7 493 284) ; 954 s par plan. Les 114 zones acceptees ont un typage incomplet : 0 `Merge`, debuts de contact a 0.
    - Hypothese (a confirmer) : une feuille dont la vraie distance tombe dans la bande (2 x gonflement de base, 2 x gonflement avec reste de grille], large de 2 rho h_e / 2 = 0,0198 m, n'est ni prouvable ni temoin ; elle se subdivise jusqu'a la profondeur 20. Toute frontiere de zone de contact traverse cette bande, d'ou l'epuisement systematique des paires en contact.
    - Allocation par thread non mesurable sous Mono (0 rapporte) ; tas gere 199 -> 226 Mo.
  - **Decisions proprietaire au HALT de campagne (2026-10-05) :**
    - *Arret a la resolution du modele : approuve.* Une feuille indecise dont le terme d'intervalle delta_A/2 + delta_B/2 est <= rho . h_e (derive du profil et des parametres, `ConflictSweep.RefinementResolution`, 0,0198 m aujourd'hui) devient terminale `Unresolved` (contact possible), jamais `ProvenDisjoint`. Justification : le terme rho . h_e de la borne vient de la grille des caps et aucune subdivision ne le reduit ; en dessous, la feuille est decrite a la resolution du modele. Budget, profondeur, gonflement, h_e et tolerance inchanges. Publie dans la preuve (`order=`, `resolution=`, arrets a la resolution).
    - *Typage :* une feuille `Unresolved` n'est pas un contact prouve. `Merge` seulement si la paire est `ConflictProven`, tout ce qui precede la zone terminale est prouve disjoint et la zone de contact possible est unique, continue et terminale sur les deux mouvements ; au moindre doute `Crossing`.
    - *Gate A : signature dediee a la 5.53* (la signature combinee 5.53 + 5.35 est abandonnee). Apres approbation du diff : regenerer preuves et signoff, HALT pour re-signature, puis rejouer les non-regressions 5.31, 5.52, 5.33 et 5.34 avant tout demarrage de la 5.35. La recommandation des Design Notes est donc remplacee.
    - *Ecarts phase 1 approuves :* subdivision du cote au plus grand terme d'intervalle, egalite exacte -> A (`dyadic-larger-delta-first-tie-A`, enregistre dans la preuve) ; un contact prouve fixe `ConflictProven` definitivement, la subdivision suivante ne sert qu'a localiser `ContactStartSMeters` dans les memes bornes ; format 4 lisible et verifiable, aucun artefact historique reinterprete par la v3.
  - **Rejeu v3 avec arret a la resolution (2026-10-05) -- HALT : revue proprietaire du diff.** `traffic-v2-5-53-classification/diff-paires-20261005-170241.md`, `regeneration-20261005-170241.md` ; campagne 3/3 verte, 0 erreur Console.
    - 94 `ConservativeConflict` -> 22 `ProvenDisjoint`, 72 `ConflictProven`, 0 restees ; 42/42 `ConflictProven` inchangees ; aucune autre classification changee ; deux plans identiques (run `4b0eab8e...`).
    - Typage : 82 `Crossing`, 32 `Merge`. 12 paires de giratoire au plafond (classification acquise, typage incomplet -> `Crossing`, debuts a 0).
    - Cout : feuilles mediane 34 811, p95 = max = 65 536, total 4 550 190 ; duree par paire mediane 1,93 s, p95 20,1 s, max 23,4 s ; 488 s par plan.
    - Compatibles : croix 6 -> 12 / 54, T 0 -> 2 / 12, giratoires 23 -> 25 / 33.
  - **Diff approuve par le proprietaire (2026-10-05) ; analyse ciblee des 12 paires au plafond, sans nouveau run.** Seules les 4 paires continuation ouest x entree ouest (une par giratoire) ont un corridor aval commun (`Ring_Merge_West`) et relevent de `GrantedMergeGap` ; les 8 paires entree x sortie d'un meme bras sortent par des corridors differents et ne peuvent jamais etre `Merge`. Pour les 4 paires utiles, la preuve publiee s'arrete au budget (65 536 feuilles, temoin acquis a 60-233 feuilles) sans projection de contact : ni la zone terminale ni l'absence de contact en amont ne sont etablies. Les 12 restent `Crossing`, debuts de contact a 0. Impact 5.35 publie au proprietaire (entree ouest des 4 giratoires sans creneau de fusion ; sortie au meme bras toujours bloquante).
  - **Base de revue.** `baseline_commit` reste `b8e7165`. Le commit `90d7a06` (archive des runs intermediaires 5.33/5.34) est anterieur a toute implementation 5.53 et hors perimetre de revue.

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
