# Sprint Change Proposal - 2026-09-30 - Plafond de braquage de la pose nominale et limite de courbe

**Statut :** APPROUVE par Kenan le 2026-09-30 (batch) et APPLIQUE : editions 4.1 a 4.11 faites dans le contrat Road World Model ; addendum 4.12 (criteres de contact) approuve et applique le meme jour ; `epics.md`, la spine d'architecture et la spec 5.31. Aucun code, aucune preuve Gate A, aucune signature, aucun ε_t ni aucune geometrie ne sont modifies.

**Sujet :** La decision proprietaire A du 2026-09-30 (diagnostic des giratoires de la 5.31) a change deux regles dans le code et dans la spec 5.31 :
- le v*(s) de conduite est le plafond de braquage de la pose nominale cinematique de la route, et non plus le plafond de regime etabli publie par le modele ;
- la limite de courbe d'adherence est appliquee des la 5.31, au lieu d'etre reportee a la 5.33.

Le contrat Road World Model §8 et les AC d'`epics.md` disent encore le contraire. La proposition aligne les textes normatifs sur la decision, sans rien changer au code, a ε_t, a Gate A ni a la geometrie.

**Mode :** batch.

**Portee :** mineure a moderee. Ajustement direct dans l'Epic 5 : aucune story ajoutee ni retiree, perimetre produit inchange. Une responsabilite passe de la 5.33 a la 5.31 (limite de courbe).

**Decision proprietaire d'origine (2026-09-30), texte de l'option retenue :** « Calculer v* par route depuis la pose nominale §8 (la meme borne que le controle NominalPoseInfeasible) ET avancer depuis la 5.33 la CurveLimit d'adherence (sinon v* monterait a ~7,9 m/s pour R = 4 m, soit 15 m/s² lateraux). Renegocie la regle A3 de la 5.31, touche le verificateur 5.30 (Ask First) et deplace une responsabilite de la 5.33. » La renegociation n'a ete consignee que dans la spec 5.31 ; cette proposition la porte dans les textes d'amont.

## 1. Probleme et preuves

### Le conflit

| Texte normatif | Ce qu'il dit | Ce que fait le code depuis la decision A |
|---|---|---|
| Contrat §8, « Steering speed ceiling » | « the model publishes v*(s) along every element. Speed planning enforces it locally » | le plan de vitesse, le verificateur de plan 5.30 et le moniteur d'execution utilisent le plafond de la pose nominale de la route |
| Contrat §8, critere de consequence de contact | *loss of control :* v > v*(s) | meme depassement du v* publie |
| `epics.md` 5.31, AC (lignes 2694 et 2696) | v(s) ≤ v*(s) ; toute vitesse observee au-dessus fait echouer le test | voir mesure ci-dessous |
| `epics.md` 5.31, Artifacts (2671) et AC (2723) | limite de courbe « reported as deferred to Story 5.33 and not applied » | la limite de courbe est appliquee |
| `epics.md` 5.33, Artifacts (2874) | la 5.33 applique la limite de courbe que la 5.31 publie comme reportee | deja appliquee |
| `epics.md` 5.50 (2224, story `done`) | « The grip-based curve speed remains 5.33's » | appliquee en 5.31 |

**Mesure.** La campagne exploratoire complete `exploratory-20260930-133627` (11 triplets, 36 604 pas) compte :
- **0 pas** au-dessus du plafond de la pose nominale ;
- environ **2 157 pas** au-dessus du v*(s) publie par le modele, avec un pic a ×11,2 sur une sortie d'anneau.

Ce second chiffre vient d'une reconstruction hors Unity (courbure interpolee a partir de s). Sur la campagne d'avant la decision A (`081217`), elle trouve 37 depassements alors que le moniteur en run en comptait 0 : elle ne vaut que comme ordre de grandeur.

### Pourquoi le plafond de regime etabli est faux pour la pose nominale

- Le correct-course du 2026-09-29 a fait de la **pose nominale cinematique** (de/ds = κ − sin(e)/a, contrat §8) la reference de mesure et de commande. Il precise que le regime etabli asin(a·κ) « n'est pas la pose nominale ».
- Le v*(κ) du compilateur (Story 5.50) est justement calcule au regime etabli : braquage δ_ss = atan(L/√(R² − a²)). Le v* de conduite est donc reste sur la seule grandeur que ce correct-course venait d'ecarter.
- Aux 24 entrees et sorties d'anneau, le pic de courbure est sature au rayon d'admission (4,03 m).
  - Le regime etabli exige 39,77° de braquage, disponible seulement a v*(κ) ≈ 0,25 m/s, soit la vitesse ou la direction se desactive (`MinimumDirectionSpeed`).
  - Or la caisse n'atteint jamais ce regime sur ces courbes courtes : la pose nominale n'exige que 32,7°, disponible jusqu'a 7,9 m/s.
- Consequence mesuree avant la decision A (`081217`) :
  - rampe a 0,23 m/s aux entrees ;
  - 7 674 pas de braquage sature et 3 660 pas de direction inactive aux entrees ;
  - 3 346 pas de direction inactive aux sorties.
- L'AC 5.31 de faisabilite (ligne 2707) exige deja que le braquage implique par la pose nominale reste dans le verrou a la vitesse planifiee, soit exactement v ≤ plafond de la pose nominale. L'epic contient donc deja les deux bornes. La decision A garde celle qui decrit la caisse reelle.

### Pourquoi la limite de courbe devient necessaire

- Sans elle, le plafond de la pose nominale autorise 7,9 m/s sur un rayon de 4,03 m, soit environ 15,5 m/s² lateraux.
- Le plafond de braquage est une limite d'autorite de direction ; il ignore l'adherence et le confort (5.50, ligne 2224).
- Le plafond de regime etabli masquait ce manque en imposant une vitesse de rampe. Des qu'il devient physiquement exact, la vitesse en courbe doit etre bornee par une contrainte laterale.

### Resultat mesure de la decision A (deja appliquee, campagnes `081217` → `133627`, memes 11 triplets)

| Critere | AVANT | APRES |
|---|---|---|
| sorties atteintes | 11/11 | 11/11 |
| elements Measured / NotMeasured | 161 / 3 | 164 / 0 |
| replanifications | 21 | 0 |
| pas en repli | 299 | 11 (un `ExitPortalReached` par run) |
| d max au pas | 0,881 m | 0,330 m |
| v min entrees / sorties d'anneau | 0,23 / 0,15 m/s | 2,68 / 2,73 m/s |
| pas satures / direction inactive (giratoires) | 11 533 / 7 006 | 0 / 0 |

- Seule regression : les tourne-a-droite serres (R = 4,21 m, d 0,19 → 0,33 m). Le proprietaire l'a acceptee (choix 1, 2026-09-30) ; la physique de direction est differee.
- Detail : `implementation-artifacts/traffic-v2-5-31-measurements/analysis-20260930-giratoires-ralentissement.md`.

### Decisions B et C : verifiees, sans conflit

- **B (cap de localisation contre l'orientation nominale).** Le contrat (« Localization result ») dit que le score utilise « heading » sans en fixer la reference. La decision est compatible. La proposition 4.5 l'ecrit dans le contrat pour qu'elle ne reste pas implicite.
- **C (progression contigue).** Conforme au contrat §4 : occurrences contigues, sans trou ni doublon. Aucune edition.

## 2. Checklist

| Point | Statut | Note |
|---|---|---|
| 1.1 a 1.3 Declencheur, probleme, preuves | [x] | Story 5.31 ; incoherence de contrat revelee en implementation (le v* de conduite est reste au regime etabli apres le correct-course du 2026-09-29) ; campagnes ci-dessus |
| 2.1 Epic 5 realisable | [x] | Oui |
| 2.2 Changements d'epic | [!] | AC et Artifacts 5.31 ; Artifacts 5.33 ; ligne de verification 5.52 ; annotations datees 5.30 et 5.50 (`done`) |
| 2.3 Autres epics | [N/A] | Aucun epic hors Epic 5 n'utilise v* ni la limite de courbe |
| 2.4 Epics obsoletes ou nouveaux | [N/A] | Aucun |
| 2.5 Ordre | [N/A] | Inchange : 5.31 → 5.52 → Gate B ; la 5.33 reste apres |
| 3.1 PRD | [N/A] | Pas de PRD separe (precedents du 2026-09-25, du 2026-09-28 et du 2026-09-29) |
| 3.2 Architecture | [!] | Contrat Road World Model : « Localization result » (score de localisation), §8 (plafond, limite de courbe, critere de contact), ligne d'admission ; note datee dans la spine |
| 3.3 UX | [N/A] | Aucune interface touchee |
| 3.4 Autres artefacts | [!] | Spec 5.31 : renvoi a cette proposition ; `deferred-work.md` deja a jour (a_lat, tourne-a-droite, dette de cout) ; `sprint-status.yaml` inchange |
| 4.1 Ajustement direct | Viable | Effort faible (textes seulement), risque faible : le code, les tests et les campagnes appliquent deja la regle |
| 4.2 Retour arriere (annuler A) | Viable, deconseille | Textes conformes a la lettre, mais la rampe a 0,23 m/s et la direction inactive reviennent ; nouvelle campagne exploratoire avant ε_t |
| 4.3 Revue MVP | [N/A] | Perimetre produit inchange |
| 4.4 Chemin retenu | [x] | Ajustement direct |

## 3. Approche recommandee

Ajustement direct des textes, dans cet ordre :
1. **Contrat §8.**
   - Deux plafonds nommes et distincts :
     - le plafond de regime etabli v*_ss(κ), publie par le modele et utilise pour l'admission ;
     - le plafond de la pose nominale v*(s), propre a la route et utilise pour la conduite.
   - La limite de courbe devient une contrainte de la 5.31.
2. **`epics.md`.** Les AC 5.31 citent le v*(s) redefini et la limite de courbe appliquee. La 5.33 garde la limite de route, le suivi et les obstacles. Les stories terminees recoivent une annotation datee.
3. **Spine.** Note datee.
4. **Reprise de la 5.31** sur les mesures existantes (`133627`) : declaration d'ε_t par le proprietaire, puis campagne d'acceptation.

**Effort :** faible. **Risque :** faible. Le changement est deja implemente, teste et mesure. Il ne touche ni Gate A (aucune preuve n'utilise v*) ni les preuves 5.52.

**Alternative ecartee :** annuler la decision A (voir 4.2 de la checklist).

## 4. Propositions detaillees

### 4.1 Contrat Road World Model, §8 - plafond de braquage et limite de courbe

**a.** Remplacer la puce « **Steering speed ceiling:** … » par :

> - **Steering speed ceilings** *(2026-09-30, sprint-change-proposal-2026-09-30.md)*. Two ceilings share the declared drivability profile's available lock δ_avail(v) and stay distinct.
>   - **Steady-state ceiling v*_ss(κ).** The model publishes it along every element: the highest speed at which δ_avail(v) covers the steady-state lock atan(L / √(R² − a²)) of the curvature κ, or none when that lock is within the high-speed lock. It serves model admission (v*_ss ≥ the minimum active steering speed, equivalently R ≥ R_adm) and evidence review. It is not a driving bound, because the steady state is not the nominal pose.
>   - **Nominal-pose ceiling v*(s).** Along a planned route, v*(s) is the highest speed at which δ_avail(v) covers |δ_N(s)|, with tan δ_N = (L/a)·tan e(s) and e the route's own offset solution (kinematic nominal pose). It is none when |δ_N| is within the high-speed lock, and 0 when |δ_N| exceeds the zero-speed lock or |e| ≥ 90°. At a seam, where e jumps, both one-sided values are evaluated and the lower one applies. Every driving-side use of v*(s) means this ceiling: speed planning, the motion-plan verifier, the runtime monitor, the published observed-speed comparison and the loss-of-control criterion.
>   - **Enforcement.** Speed planning enforces v*(s) locally along the planned trajectory, decelerating within its declared bound early enough before each tighter curve, never as a single cap equal to the lowest ceiling in the look-ahead. When it cannot be met from the current state, the plan declares that infeasibility as its binding constraint.
>   - Only a caller without a declared drivability geometry, such as an undeclared test fixture, has no offset solution. It keeps the steady-state ceiling. Runtime admission refuses undeclared models, so every driven V2 route has an offset solution.
>
> - **Curve limit** *(2026-09-30, sprint-change-proposal-2026-09-30.md; applied from Story 5.31)*. The speed plan also applies v ≤ √(a_lat / |κ(s)|), with a_lat = min(the driver's declared comfortable acceleration bound, the vehicle's lateral grip μ·g). It is a named constraint and can bind. It bounds lateral acceleration, which neither steering ceiling does. A dedicated lateral comfort parameter may replace the driver's comfortable deceleration later; that is not a change of rule.

**b.** In the consequence criteria, replace *loss of control:* « v > v*(s) » with :

> *loss of control:* v > v*(s), the route's nominal-pose ceiling, `TrackingToleranceExceeded`, or any V2 fallback command;

*Rationale:* make the chosen ceiling explicit where a campaign can fail.

### 4.2 Contrat Road World Model, ligne d'admission « declared drivability — steering admission »

AVANT : « steering speed ceiling ≥ minimum active steering speed »

APRES :

> steady-state steering speed ceiling v*_ss ≥ minimum active steering speed

*Rationale:* the admission rule is unchanged. Only its quantity is named so it is not confused with the driving ceiling.

### 4.3 Contrat Road World Model, §8 « Speed planning »

Aucune edition. La liste des contraintes nommees contient deja « curve limit » et « steering speed ceiling ».

### 4.4 Spine d'architecture - note datee

Apres la note du 2026-09-29 :

> *2026-09-30 course correction (`planning-artifacts/sprint-change-proposal-2026-09-30.md`), accepted by the owner:* the driving steering speed ceiling v*(s) is the ceiling of the route's kinematic nominal pose. The model-published steady-state ceiling v*_ss(κ) keeps serving model admission and evidence review. The grip-based curve limit is applied from Story 5.31 instead of 5.33. Localization scores heading against the candidate's nominal orientation when the vehicle's offset state is available. No Gate A proof uses either ceiling; no evidence or signature changes.

### 4.5 Contrat Road World Model, « Localization result » - score de localisation (decision B)

Apres « Candidate scoring uses geometric distance, heading, current route, previous accepted element and explicit connectivity. », ajouter :

> *2026-09-30 (sprint-change-proposal-2026-09-30.md):* When the caller supplies the vehicle's kinematic offset state, the heading term compares the body with each candidate's kinematic nominal orientation (contract §8): the offset is transported from the vehicle's own anchors along the candidate, seam jumps included. Without an anchor for a candidate, the term is the tangent heading error reduced by a declared dead band that bounds |e| on any admitted element (asin(a / R_adm) plus the seam tangent tolerance). Callers that supply no offset state keep the tangent comparison. `WrongWay` keeps its definition.

*Rationale:* write decision B into the contract. Without it, the localization could flip onto the exit branch at a ring split. The route bonus equal to the score band is profile data and needs no contract text.

### 4.6 `epics.md` - Story 5.31

**a. Artifacts** (ligne 2671)

AVANT : « free-road constraints: desired speed and steering speed ceiling applied with declared longitudinal bounds; road limit and grip curve limit named, reported as deferred to 5.33 and never applied; binding constraint named »

APRES :

> free-road constraints: desired speed, the nominal-pose steering speed ceiling and the grip-based curve limit applied with declared longitudinal bounds; road limit named, reported as deferred to 5.33 and never applied; binding constraint named

**b. AC, plafond** (ligne 2694)

AVANT : « **And** the planned speed profile respects the steering speed ceiling locally: v(s) ≤ v*(s) at every point… »

APRES :

> **And** the planned speed profile respects the steering speed ceiling locally: v(s) ≤ v*(s), the route's nominal-pose steering speed ceiling (contract §8; the model-published steady-state ceiling serves admission only), at every point…

(fin de phrase inchangee)

**c. AC, vitesse observee** (ligne 2696) : inchangee. v*(s) y prend le sens redefini en 4.1.

**d. AC, plan de vitesse** (ligne 2723)

AVANT : « **Then** it applies desired speed, the steering speed ceiling and the declared longitudinal bounds as named constraints and identifies the binding one; road limit and the grip-based curve limit are named, reported as deferred to Story 5.33 and not applied; an authored non-zero road limit… »

APRES :

> **Then** it applies desired speed, the nominal-pose steering speed ceiling, the grip-based curve limit (contract §8) and the declared longitudinal bounds as named constraints and identifies the binding one; the road limit is named, reported as deferred to Story 5.33 and not applied; an authored non-zero road limit…

(fin de phrase inchangee)

**e. Note d'insertion**, sous le titre de la story :

> *Amended 2026-09-30 (sprint-change-proposal-2026-09-30.md): the driving steering speed ceiling is the route's nominal-pose ceiling, and the grip-based curve limit is applied here instead of in 5.33 (roundabout diagnosis, owner decision A).*

### 4.7 `epics.md` - Story 5.33 (backlog)

**Artifacts** (ligne 2874)

AVANT : « …and applies the road limit and the grip-based curve limit that Story 5.31 publishes as deferred. »

APRES :

> …and applies the road limit that Story 5.31 publishes as deferred. The grip-based curve limit is applied since Story 5.31 (2026-09-30); this story keeps it among the named constraints and may replace its comfort bound by a dedicated lateral comfort parameter.

L'AC ligne 2890 reste inchangee : elle cite deja la limite de courbe et le plafond parmi les contraintes nommees.

### 4.8 `epics.md` - Story 5.52 (backlog)

**PlayMode verification** (ligne 2770)

AVANT : « …within ε_t (per-step bound and model-M inter-step bound) and v ≤ v*(s), and despawn… »

APRES :

> …within ε_t (per-step bound and model-M inter-step bound) and v ≤ v*(s), the route's nominal-pose steering speed ceiling, and despawn…

### 4.9 `epics.md` - Story 5.50 (`done`), annotation

Apres la puce ligne 2224 (« The ceiling is a kinematic steering-authority limit and ignores grip and slip. The grip-based curve speed remains 5.33's. ») :

> - *2026-09-30 (sprint-change-proposal-2026-09-30.md):* this ceiling is the steady-state ceiling v*_ss(κ). It keeps the admission rule. Driving uses the nominal-pose ceiling of each route (contract §8), and the grip-based curve limit is applied from Story 5.31.

Le texte d'origine de la story terminee n'est pas reecrit.

### 4.10 `epics.md` - Story 5.30 (`done`), annotation

Apres l'AC ligne 2641 (« the path and motion contracts carry the steering speed ceiling v*(s)… ») :

> *2026-09-30 (sprint-change-proposal-2026-09-30.md):* when the route's offset solution is known, the path points carry the nominal-pose ceiling (contract §8), and the verifier judges the plan against it. A caller without a declared drivability geometry keeps the steady-state ceiling, as delivered.

### 4.11 Spec 5.31 - renvoi

- Dans l'entree du Spec Change Log « 2026-09-30 -- diagnostic des giratoires, decisions proprietaire A / B / C », ajouter en fin :

  > Porte dans le contrat, `epics.md` et la spine par `sprint-change-proposal-2026-09-30.md`.

- Le bloc fige (lignes 56-58) porte deja la regle ; aucune autre edition.

### 4.12 Addendum - criteres de consequence des contacts (APPROUVE le 2026-09-30, applique)

**Constat** (campagne complete `exploratory-20260930-133627`, 36 587 pas) :
- 230 pas portent `OutsideEnvelope` :
  - 219 au premier pas d'un nouvel element : retention d'hysteresis 5.26, la reference depasse la fin de l'element precedent ;
  - 11 au dernier pas, au portail de sortie.
- **Aucun** pas ne deborde de l'enveloppe de largeur.
- Le taux etait le meme avant les decisions A / B / C (`093040`, `095445`).
- Le test de campagne n'evaluait les criteres que sur ±50 pas, alors que le contrat dit « jusqu'a la fin du run ». Au pied de la lettre, la regle ferait echouer tout contact : couture suivante (`OutsideEnvelope`), puis repli terminal `ExitPortalReached` (« any V2 fallback command »).
- Les 4 contacts du dos-d'ane echouaient pour cette seule raison : une couture dans les 50 pas.

**Editions du contrat** (§8, criteres de consequence ; « Localization result ») :
- *out of bounds* : d > ε_t ; reference ou empreinte hors de l'enveloppe de largeur de l'element retenu ; ou `WrongWay`. Un depassement longitudinal seul (retention a une couture, fin du portail de sortie) n'est pas une sortie de route, bien qu'il leve `OutsideEnvelope`.
- *loss of control* : tout repli V2 **autre que** le maintien terminal au portail de sortie atteint (`ExitPortalReached`).
- Definition de `OutsideEnvelope` : elle mentionne aussi le depassement longitudinal (comportement 5.26 inchange) ; le resultat expose les deux causes separement.

**Code (5.31)** :
- `RoadLocation.LongitudinalOverrunMeters` et `RoadLocation.OutsideWidthEnvelope`. Le drapeau reste leur union, bit a bit identique : aucun impact sur les fixtures 5.26 ni sur Gate A.
- Trace `outside_width`.
- Criteres evalues du premier contact a la fin du run, comme le dit le contrat. La fenetre de ±50 pas ne sert qu'a la publication.

## 5. Transmission

**Portee :** mineure a moderee. Textes seulement ; une responsabilite passe de la 5.33 a la 5.31.

| Role | Responsabilite |
|---|---|
| Developer (cette session) | Apres approbation : appliquer 4.1 a 4.11 a l'identique, puis `graphify update .` (sans effet, les artefacts sont hors perimetre du graphe). Aucun code, aucun test, aucune commande Unity. |
| Proprietaire | Approuver la proposition ; puis declarer ε_t sur la campagne `133627` ; puis approuver la campagne d'acceptation |
| Developer (5.31) | Campagne d'acceptation apres declaration d'ε_t ; aucun critere coche avant son resultat |
| Developer (5.33) | Reprendre la limite de courbe deja appliquee ; parametre de confort lateral dedie si retenu |

**Criteres de succes :**
- les textes 4.1 a 4.11 sont edites a l'identique ;
- aucun AC de la 5.31 ne contredit plus le code : v*(s) designe le plafond de la pose nominale, et la limite de courbe est appliquee ;
- aucune preuve Gate A, aucune signature, aucun ε_t et aucune geometrie ne sont modifies.

**Sprint-status :** aucun changement.
