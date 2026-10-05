---
title: 'Story 5.53 -- addendum P12 : typing-v2, typage des quatre fusions ouest de giratoire'
type: 'bugfix'
created: '2026-10-05'
status: 'draft'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-53-conflict-pair-separation-proof-and-conflict-kinds.md'
  - '{project-root}/_bmad-output/implementation-artifacts/traffic-v2-5-53-classification/regeneration-20261005-170241.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Dans les 4 giratoires de `MVP_Run`, la paire `Ring_Split_West -> Ring_Merge_West` (continuation) x `Connector_West_In -> Ring_Merge_West` (entree) est `ConflictProven`, avec un corridor aval commun.
- Son typage v1 s'arrete au budget (65 536 feuilles) : genre `Crossing`, debuts de contact a 0.
- La meme geometrie, tournee, est typee `Merge` aux bras diagonal (28 236 feuilles) et sud (57 194).
- Consequence : l'entree ouest n'a pas de creneau de fusion en 5.35 (P12).

**Approach:** typing-v2 (decisions proprietaire du 2026-10-05). Dans la passe de typage, une feuille non prouvee dont les deux intervalles projetes sont deja contenus dans l'union de contact courante devient terminale, sans subdivision.
- Ses sous-feuilles n'auraient rien ajoute a l'union : le genre et les debuts sont exactement ceux de v1 lorsque v1 aboutit.
- Perimetre recommande (option B, a confirmer) : typing-v2 remplace v1, en une seule passe de 65 536 feuilles, pour le seul typage des paires `ConflictProven` par le balayage initial (temoin du sweep, classification independante du raffinement) qui ont un corridor aval commun. Dans `MVP_Run` : 36 paires, dont les 32 `Merge` deja types (genre et debuts identiques attendus) et les 4 paires ouest.

## Boundaries & Constraints

**Always:**

- **Inchanges** : classification (le verdict `ConflictProven` n'est pas rouvert : pour ces paires il vient du balayage, pas du raffinement), `RefinementLeafBudget` = 65 536 par paire, plafond dur (proposition 8.2), `MaxSubdivisionDepth` = 20, `ProofToleranceMeters`, h_e, gonflement, a_e, resolution, ordre `dyadic-larger-delta-first-tie-A`, critere `Merge` (`ZoneTyping.Of` : union unique, terminale sur les deux membres).
- **Perimetre (option B)** : une seule passe de typage par paire, v2 au lieu de v1, pour les 36 paires ci-dessus. Les 100 autres decisions sont inchangees, au bit pres. Les 32 fusions deja typees changent de revision (chaine `typing-v2`), avec un genre et des debuts identiques exiges, sinon HALT. Les paires classees par le raffinement (72) gardent v1 : un elagage avant leur premier temoin pourrait changer leur classification.
- **Integration a la politique v3** : la condition et la passe v2 vivent dans `TypeZone` de `AutomatedPairDecisionPolicy`. Tout plan ulterieur, dont la regeneration P8 de la 5.35, les reproduit donc de facon deterministe. La version de typage est publiee par paire (`typing-v2|…`) ; `TypingMatches` et `ValidatePlan` l'acceptent seulement si la condition est remplie.
- **Issue** : v2 complete et union terminale sur les deux membres -> `Merge` avec ses `ContactStartSMeters`. Sinon -> `Crossing` a 0 inchange, et HALT.
- **Ecriture** : par le menu transactionnel de la 5.53, sur les 36 decisions du perimetre seulement ; la seconde execution ne propose aucun changement ; diff publie paire par paire, en separant les 32 identiques des 4 ouest.
- **Gate A** : aucune signature dediee (decision proprietaire du 2026-10-05). Le modele modifie met l'admission V2 en `GateAEvidenceStale` jusqu'a la signature dediee de la 5.35 (phase 2). Aucune execution V2 sur ce modele non signe.
- **Exception §6** : extension P8 de `sprint-change-proposal-2026-10-05.md` a cette application (amendement du 2026-10-05).

**Ask First:**

- Une des 4 paires reste incomplete ou non terminale en v2 : HALT, aucune autre methode sans decision.
- Une difference de genre ou de debut entre v1 et v2 sur une paire de controle : HALT, l'equivalence est rompue.
- Une modification de la politique hors de la passe de typage, ou d'une decision hors des 36.

**Never:**

- Campagne globale, hausse du budget par passe, changement de tolerance, h_e, gonflement ou critere de conflit, reouverture du verdict `ConflictProven`, `Merge` ecrit a la main, transfert par symetrie entre bras.
- Signer Gate A. Toucher au runtime, aux controles ou a la 5.35.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Fusion ouest | v1 incomplete, `ConflictProven`, aval commun | v2 complete -> `Merge`, debuts publies | Sinon `Crossing` a 0, HALT |
| Fusions deja typees | 32 `Merge` (T, croix, giratoires), typage v1 complet | v2 : meme genre, memes debuts ; nouvelle revision | Ecart -> HALT |
| Classee par raffinement | Une des 72 paires `ConservativeConflict -> ConflictProven` | v1 inchange, decision inchangee | N/A |
| Pas d'aval commun | Entree x sortie d'un meme bras | `Crossing` par definition, aucune passe v2 | N/A |
| Rejeu | Seconde execution | Aucun changement propose | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweepRefinement.cs`
  - `:177-305` `Refine` : boucle DFS `:249-285`, projection `:592-604`, fusion d'intervalles `:606-625` ;
  - `:451-515` `EvaluateLeaf` (prouve par `AabbDistance`, temoin, `WitnessSplit`, arret a la resolution) ;
  - `:91-143` `ZoneTyping` (`Of`, `Terminal`, `Canonical` « typing-v1 »).
  - Ajouter un mode typage v2 : test de contenance contre l'union courante avant `TrySplit`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AutomatedPairDecisionPolicy.cs`
  - `:45` `RefinementLeafBudget` ; `:1015-1023` `Refine` ;
  - `:1047-1058` `TypeZone` : condition et passe v2 ;
  - `:917-926` `TypingMatches` (prefixe `typing-v1` seul aujourd'hui) ; `ValidatePlan`.
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json` (4 decisions), `MVP_Run.road-model.json` (`ConflictZoneTypes`).
- Paires : `traffic-v2-5-53-classification/regeneration-20261005-170241.md:32,34,36,38` (plafond) et detail `:121,124,127,130`.
- Tests : `Assets/RoadRage/Tests/EditMode/Story553ConflictClassificationTests.cs`, `Story553MvpRunClassificationTests.cs`, `Story553RegenerationTests.cs`.

## Tasks & Acceptance

**Execution:**
- [ ] `_bmad-output/planning-artifacts/sprint-change-proposal-2026-10-05.md` -- verifier l'amendement P12 approuve.
- [ ] `ConflictSweepRefinement.cs` -- mode typage v2 (contenance dans l'union courante -> terminale), `typing-v2` dans `Canonical`, compteurs par etat (prouvees, temoins, resolution, terminales par contenance).
- [ ] `AutomatedPairDecisionPolicy.cs` -- typage v2 dans `TypeZone` pour les paires `ConflictProven` par le balayage a corridor aval commun ; `TypingMatches`/`ValidatePlan` lies a la condition.
- [ ] `Tests/EditMode/Story553TypingV2Tests.cs` `[Geometry][Story553]` -- matrice ; equivalence v1/v2 sur fixtures synthetiques ; diagnostic publie (feuilles par etat) pour les 36 paires du perimetre ; determinisme.
- Checkpoint 1 : `Story553` EditMode vert ; equivalence sur les 32 fusions deja typees ; issue des 4 paires connue. **HALT : revue proprietaire.**
- [ ] Application transactionnelle (36 decisions), regeneration du rapport 5.28 et du diff, mise a jour de `Story553MvpRunClassificationTests` (genres et debuts derives des artefacts).
- [ ] `traffic-v2-5-53-classification/typing-v2-*.md` (nouveau) -- diff des 36 paires (32 identiques, 4 ouest), feuilles par etat, cout.
- [ ] `spec-5-53-…md` (Spec Change Log), `deferred-work.md`. `sprint-status` : 5.53 reste en `review` jusqu'a la signature 5.35.

**Acceptance Criteria:**

- Given les 4 paires de fusion ouest, when la politique v3 avec typing-v2 s'execute, then chacune est soit `Merge` avec des `ContactStartSMeters` derives de la preuve, soit `Crossing` a 0 avec HALT. Leur verdict `ConflictProven` est inchange.
- Given les 32 fusions deja typees, when typing-v2 les type, then le genre et les debuts sont identiques a ceux publies en v1 ; seule la chaine de typage change.
- Given les 100 autres decisions, when le plan s'execute, then aucune ne change.
- Given un second plan, when il s'execute, then aucun changement n'est propose.
- Given le modele applique, when l'admission V2 est evaluee, then elle reste en `GateAEvidenceStale` jusqu'a la signature 5.35.

## Spec Change Log

## Design Notes

**Pourquoi l'equivalence est exacte.** L'union de contact est l'union des projections des feuilles terminales non prouvees. Prenons une feuille contenue dans l'union courante :
- en v1, ses sous-feuilles sont prouvees (non projetees) ou projetees a l'interieur de cette feuille, donc de l'union ;
- en v2, elle est projetee entiere, toujours a l'interieur de l'union.
Dans les deux cas l'union ne change pas, et l'etat de la suite du parcours (meme ordre) est identique. Seul le temoin enregistre peut differer, et seulement si une contenance a lieu avant le premier temoin : il n'affecte ni la classification acquise ni le typage.

**Plafond par paire (proposition 8.2) : options a trancher au checkpoint.**
- **B (recommandee)** : v2 remplace v1 pour le typage des 36 paires `ConflictProven` du balayage a corridor aval commun. Une passe, 65 536 feuilles au plus par paire. Le plafond dur est respecte, mais 32 revisions de plus sont ecrites, a resultat identique exige.
- **A** : passe v2 apres une passe v1 incomplete, sur les 4 paires seulement. Seulement 4 revisions, mais jusqu'a 2 x 65 536 feuilles par paire : une derogation au plafond 8.2.

**Hypotheses non verifiees.**
- Que v2 aboutisse sous 65 536 feuilles sur les 4 paires. L'explication du cout (boites alignees sur le monde, subdivision interne) n'est pas mesuree : le checkpoint 1 la publie avant toute ecriture.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.53 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = attendu, 0 erreur Console.
