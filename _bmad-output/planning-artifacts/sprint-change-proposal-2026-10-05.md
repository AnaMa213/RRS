# Sprint Change Proposal - 2026-10-05

**Statut :** APPROUVE par Kenan le 2026-10-05 et APPLIQUE : editions 4.1 a 4.6 ; precisions 5.1 a 5.3 approuvees avec les renforcements de la section 8. Aucun artefact Gate A n'est modifie par cette proposition, aucune signature n'est impliquee.
**Sujet :** Insertion de la Story 5.53 (preuve de separation raffinee des `ConservativeConflict` et typage `Crossing` / `Merge`) avant la 5.35. Extension de l'exception `5.50-AUTO-DECISIONS-v1` a la 5.53 (politique v3) et a la 5.35 (P8).
**Mode :** Batch. **Portee :** mineure, ajustement direct dans l'Epic 5, sans nouvel epic et sans changement de perimetre produit.
**Decisions proprietaire deja prises (2026-10-05, spec 5.35) :** P8 = a, P9 = b, P10 = a (limitee aux fusions).

## 1. Probleme et preuves

Constats par lecture du code et des donnees ; rien n'a ete execute dans Unity.

- Le manifeste `v1-regression-5-50/automated-pair-decisions.json` (run `1765b1f3…`, politique v2, balayage v2, `a_e = 0,34`, `h_e = 0,008`) classe **94** paires `ConservativeConflict` (`continuous-contact-possible`), **42** `ConflictProven`, 50 `ProvenDisjoint`, 84 suivis.
- Le motif `continuous-contact-possible` (`AutomatedPairDecisionPolicy.cs:491-497`) est rendu quand la borne d'un couple d'intervalles, `δ_I/2 + δ_J/2 + 2·gonflement` (`ConflictSweep.cs:1313`), autorise un contact sans temoin. La politique s'arrete la. `MaxSubdivisionDepth = 20` et `dyadic-a-then-b` figurent dans le manifeste et dans la preuve de chaque paire, mais **aucun code ne subdivise** : ils sont declares sans etre appliques.
- La marge negative vient du terme d'intervalle, pas de la geometrie. Exemple : tout-droit opposes de la croix, temoin en (−2 ; 0 ; 8) et (2 ; 0 ; 8), axes a 4,0 m, slack −2,276 m. Le slack median des 94 paires vaut −2,276 m, le moins negatif −0,066 m. L'intervalle de queue de corridor prolonge (portee d'un demi-gabarit, deux echantillons) porte un δ de l'ordre de 2 m.
- Consequence (audit `traffic-v2-5-35-explorations/conflict-zone-audit-20261005.md`) : aucune paire inter-approches compatible dans les T, 6 sur 54 dans la croix, une entree de giratoire en conflit avec l'entree d'un autre bras et la sortie de son propre bras. La 5.34 est quasi exclusive par carrefour et la 5.35 ne peut pas etre juste sur ce modele.

## 2. Impact

| Section | Etat | Constat |
|---|---|---|
| Epics | [!] | Story 5.53 inseree ; chemin 5.34 → 5.53 → 5.35 ; table des revues |
| Contrat Road World Model | [N/A] | Aucun contrat change. `ConflictZone.Kind` et `ContactStartSMeters` sont des ajouts de schema (v5) portes par la 5.53 |
| PRD / Spine / UX | [N/A] | Aucun changement |
| Regles de build | [!] | §6 : exception etendue a la 5.53 et a la 5.35 |
| `sprint-status.yaml` | [OK] | Entree 5-53 deja inseree le 2026-10-05 avant la 5-35 |
| Gate A | [!] | Le modele change : admission V2 en `GateAEvidenceStale` jusqu'a une signature (voir 5) |

## 3. Voie recommandee

Ajustement direct. La 5.53 corrige les donnees compilees par une preuve deterministe plus fine, sur la meme geometrie, avec le meme gonflement et la meme rigueur. Aucune exception runtime, aucun seuil assoupli. La 5.35 demarre sur des zones typees et un diff revu.

## 4. Editions normatives (formulations definitives, appliquees apres approbation)

### 4.1 `docs/setup/build-workflow-rules.md` §6 — paragraphe ajoute en fin de section

```markdown
2026-10-05 (`sprint-change-proposal-2026-10-05.md`) : la meme exception, sous toutes ses conditions,
s'applique :

- a la Story 5.53, pour la politique de decision v3 : raffinement borne des seules paires
  `ConservativeConflict` sans temoin (subdivision dyadique des intervalles, budget declare
  `RefinementLeafBudget`). `ConflictProven`, `ProvenDisjoint` et `FailClosed` gardent classification,
  raison et preuve ; une `ConservativeConflict` ne peut passer qu'a `ProvenDisjoint` (separation
  strictement superieure a la tolerance sur chaque feuille) ou a `ConflictProven` (temoin). Tout
  autre changement de classification est un HALT ;
- a la Story 5.35 (decision P8), pour la regeneration rendue necessaire par l'inclusion des
  controles dans `CandidateModel` : a geometrie inchangee, chaque paire garde sa classification, sa
  raison et sa preuve ; seuls changent les identifiants de run et de revision. Toute classification
  differente est un HALT.

Elle n'autorise ni la revue ni la signature de Gate A.
```

La derniere phrase existante du §6 (« Cette exception ne s'etend a aucune autre story que 5.52… ») est remplacee par : `Cette exception ne s'etend a aucune autre story que 5.52, 5.53 et 5.35 et n'autorise jamais la revue ni la signature de Gate A, qui restent des actes proprietaire distincts.`

### 4.2 `epics.md` — graphe (l. 1705)

`5.34 ─► 5.35 ═GATE C═` devient `5.34 ─► 5.53 ─► 5.35 ═GATE C═` (alignement des colonnes ajuste).

### 4.3 `epics.md` — chemin critique (l. 1736)

`… → 5.31 → 5.34 → 5.35 → …` devient `… → 5.31 → 5.34 → 5.53 → 5.35 → …`.

### 4.4 `epics.md` — index des stories (apres la ligne 5.34)

`| 5.53 | Conflict-pair separation proof and conflict kinds *(inserted 2026-10-05, owner decision P9; precedes 5.35)* | FOUNDATION | Road model evidence | M |`

### 4.5 `epics.md` — table des revues (apres la ligne 5.34)

`| 5.53 | EditMode | \`edge-case-hunter\`, \`verification-gap\` — dense proof logic, and the story claims a reclassification of Gate A evidence |`

### 4.6 `epics.md` — nouvelle fiche, inseree avant `### Story 5.35`

```markdown
### Story 5.53: Conflict-Pair Separation Proof and Conflict Kinds

**Type:** FOUNDATION (evidence) · **Boundary:** Road model evidence · **Complexity:** M
**Implements:** AD-46, Road World Model contract §8 (Gate A evidence lifecycle)
*Inserted 2026-10-05 (sprint-change-proposal-2026-10-05.md, owner decision P9): the 2026-10-05 audit found 94 of the 136 `MVP_Run` conflict zones accepted as `ConservativeConflict` because the decision policy stops at the continuous bound without refining it, which makes every T junction and most of the crossroads exclusive.*

As the owner,
I want every conservative conflict pair either proven disjoint, proven in conflict, or kept with a published reason, and every accepted zone typed as a crossing or a merge,
So that compatible traversals are not denied by the model and Story 5.35 can grant merge gaps only where the compiled model proves a merge.

**Prerequisites:** 5.34.

**Capability delivered:** decision policy v3 refines, under a declared leaf budget, only the `ConservativeConflict` pairs without a witness, with the existing sweep machinery (inflation, kinematic pose set, offset grid, remainders, tolerance) unchanged. The compiled model (`CompilerSchemaVersion` 5) types every `ConflictZone` as `Crossing` or `Merge` with a per-member `ContactStartSMeters`. Gate A evidence is regenerated through the Story 5.52 path with a complete pair-by-pair diff.

**Rules:**
- `ConflictProven`, `ProvenDisjoint` and `FailClosed` pairs keep classification, reason and proof; a conservative pair may only become `ProvenDisjoint` (strictly positive separation on every leaf) or `ConflictProven` (witness).
- Any unproven, conservative or ambiguous pair is `Crossing`.
- No pair is classified by hand or by LLM judgment; no runtime exception; no Stop/Yield/Priority.
- Gate A is signed only by the owner.

**Non-goals:** traffic rules and controls (5.35); coordinator changes; geometry, inflation, tolerance or profile changes.

**Unlocks:** 5.35 (merge gaps on compiled `Merge` zones only, P10).
```

## 5. Precisions a approuver avec cette proposition

**5.1 `RefinementLeafBudget` = 65 536 feuilles par paire.** Profondeur maximale : le `MaxSubdivisionDepth = 20` deja declare. Ordre : `dyadic-a-then-b`, alterne, sur l'intervalle subdivisable. Le cout par feuille n'est pas mesure ; la valeur est declaree avant tout run. Toute revision ulterieure est un Ask First de la spec 5.53.

**5.2 Lecture de « corridor de depart » pour `Merge`.** Deux mouvements de meme corridor d'entree sont des suivis (`SameApproach`), jamais une zone. La seule lecture operante est donc le **corridor de sortie commun** (`ToCorridorId`), le corridor par lequel les deux quittent le carrefour. Exemple : giratoire, `Ring_Split_South -> Ring_Merge_South` et `Connector_South_In -> Ring_Merge_South`.

**5.3 Critere `Merge` rendu discriminant.** Si `ContactStartSMeters` est le minimum des feuilles non prouvees de chaque membre, la phrase « separation prouvee pour toute combinaison ou l'un des deux est avant son abscisse de debut de contact » est vraie pour toute paire : elle ne distingue rien. La 5.53 retient donc la definition operationnelle suivante, deterministe et conservatrice :
- les feuilles non prouvees sont celles de slack ≤ tolerance apres raffinement complet de la paire, les `ConflictProven` compris (pour le typage seulement, sans toucher a leur decision) ;
- `ContactStartSMeters` d'un membre = debut, ramene dans [0, L], de la projection sur ce membre des feuilles non prouvees ;
- `Merge` si et seulement si : corridor de sortie commun, **et** sur chaque membre la projection des feuilles non prouvees est un intervalle connexe qui atteint la fin du membre (le contact, une fois possible, persiste jusqu'a la convergence). C'est le cas « fusion » de la matrice. Un contact possible en amont, separe de la convergence par une bande prouvee, rend la paire `Crossing` : c'est le cas « fusion qui croise avant ».
- Budget epuise pendant le typage, ou toute autre incertitude : `Crossing` et `ContactStartSMeters = 0`, l'abscisse la plus precoce.

## 6. Signature Gate A

La spec 5.53 recommande une signature differee et combinee a la phase 2 de la 5.35, sous ses quatre conditions (Design Notes de la spec). Cette proposition ne tranche pas ce point : il reste au checkpoint 2 de la 5.53.

## 7. Decisions demandees

1. Approuver la proposition (editions 4.1 a 4.6).
2. Approuver les precisions 5.1 (budget), 5.2 (corridor de sortie commun) et 5.3 (critere `Merge`), ou les amender.

## 8. Decisions proprietaire (2026-10-05)

1. **Proposition approuvee** telle que cadree. L'exception n'est pas generalisee au-dela de la 5.53 et de la P8 de la 5.35.
2. **`RefinementLeafBudget` = 65 536 feuilles par paire, plafond dur** ; `MaxSubdivisionDepth = 20` inchange.
   - Un maximum, pas un objectif : le raffinement s'arrete des qu'une preuve definitive est obtenue.
   - Publier pour chaque paire les feuilles reellement consommees ; publier aussi le total, la mediane, le p95, le maximum, la duree totale et, si mesurable, la memoire allouee.
   - Cout manifestement pathologique : HALT avant toute modification du budget. Budget ou profondeur modifies : Ask First.
   - Budget epuise sans preuve : resultat conservateur, jamais `ProvenDisjoint`.
3. **Corridor aval commun approuve.** Le terme ambigu « corridor de depart » est evite dans le code et les artefacts nouveaux ; le nom retenu est `CommonExitCorridor`. Un corridor d'entree partage reste un suivi et ne definit jamais un `Merge`.
4. **Critere `Merge` approuve avec renforcement.** Une paire est `Merge` seulement si toutes ces proprietes sont prouvees :
   1. les deux mouvements ont le meme `CommonExitCorridor` ;
   2. pour chaque mouvement, l'ensemble des poses ou un contact devient possible forme un seul intervalle terminal ;
   3. cet intervalle s'etend jusqu'a la fin du mouvement ;
   4. aucun autre intervalle de contact possible n'existe en amont ;
   5. il n'existe donc aucune sequence contact possible → separation → convergence finale ; un tel cas reste `Crossing`.

   Les deux `ContactStartSMeters` sont derives de la preuve et publies. Ambiguite, budget epuise ou preuve incomplete : `Crossing`, `ContactStartSMeters = 0`. Aucun `Merge` n'est attribue manuellement.
5. Les 6 fichiers 5.33 indexes sont commites a part apres inspection (`90d7a06`).

## 9. Amendement P12 (2026-10-05) -- typing-v2 des fusions ouest

Decisions proprietaire du 2026-10-05 (spec 5.35, P12 = b ; addendum `spec-5-53a-west-merge-typing-v2.md`) :

1. **typing-v2 approuve**, pour le seul typage. Une feuille non prouvee dont les deux intervalles projetes sont contenus dans l'union de contact courante est terminale. Classification, tolerances, h_e, gonflement, resolution, ordre de subdivision et critere `Merge` (point 8.4) sont inchanges.
2. **Extension de l'exception §6.** Le perimetre P8 couvre aussi l'application de typing-v2 par l'addendum 5.53, sous toutes les conditions du §6. Aucune campagne globale.
3. **Execution en addendum de la 5.53**, sans signature Gate A dediee. Le modele modifie reste en `GateAEvidenceStale` jusqu'a la signature dediee de la 5.35 ; aucune execution V2 sur un modele non signe. La 5.53 reste en `review` jusque-la.
4. **Plafond 8.2 maintenu** : 65 536 feuilles par paire. Le choix entre l'option B (v2 au lieu de v1 pour les 36 paires `ConflictProven` du balayage a corridor aval commun, resultat identique exige sur les 32 deja typees) et l'option A (seconde passe sur les 4 paires, derogation au plafond) est tranche au checkpoint d'approbation de l'addendum.

