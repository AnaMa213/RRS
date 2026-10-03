# Proposition : planification exacte a portee bornee prouvee, perception inchangee (Story 5.33, D13 bis)

Soumise a approbation proprietaire. Rien n'est change au runtime tant qu'elle n'est pas approuvee.

## 1. Mesures (banc `Story533PlanningCostBenchTests`, rapport `planning-cost-bench.md`)

1 335 poses nominales sur les 11 routes de `campaign-5-31.json`, e de la reference, comme le driver.

| Variante (horizon + plan de vitesse, par vehicule et par pas) | cout moyen | gain |
|---|---|---|
| actuel (portee 100 000 m : toute la route restante) | 3,52 ms | 1x |
| exact optimise (premier prototype, compare au bit pres : 0 difference) | 2,73 ms | 1,29x |
| exact optimise + portee bornee | 0,37 ms | 9,6x |

- Horizon actuel : 145 m et 1 766 points en moyenne (369 m et 4 385 points au plus). Repartition : echantillonnage 0,43 ms,
  points 0,21, transport de e 0,45 (RK4, 4 sinus par sous-pas : incompressible a portee egale), pentes et raccords 0,13.
- Plan de vitesse actuel : 2,02 ms, dont 1,94 ms de verification du profil (`MotionPlan.VerifySpeedProfile`).
- Occupation de la frame : 1,23 ms par vehicule ; projection elaguee exacte (0 difference sur 176 220 projections) : 0,70 ms.
- Allocations horizon + plan : 863 Ko par pas actuellement, 102 Ko a portee bornee.

**Les optimisations exactes seules sont clairement insuffisantes.** Meme au-dela du premier prototype (tableaux plats et
curseurs dans la verification), le transport de e sur toute la route reste incompressible : plancher estime de 1,2 a
1,4 ms par vehicule pour l'horizon et le plan, plus l'occupation, la perception et le reste (~0,5 ms), soit environ 2 ms
par vehicule, le double du budget de 1 ms.

## 2. Proposition

Planification (horizon de planification, transport de e, plafonds, plan de vitesse, verification) bornee a H ;
perception laissee sur toute la route restante ; optimisations exactes partout ou elles s'appliquent.

### Formule de H

H = d1 + v_ref^2 / (2 b_plan) + m

- d1 : distance du deuxieme noeud du plan de vitesse (>= 0,2 m, `MinimumKnotSpacingMeters`). La vitesse visee par la
  commande est interpolee entre les noeuds 0 et 1 (`SpeedAt(preview)`, preview <= 0,16 m).
- v_ref = max(v desiree, v courante).
- b_plan = deceleration de confort x `PlanningBoundMargin` (2 x 0,99 = 1,98 m/s2) : le plus petit freinage de la passe
  arriere (le cas inatteignable freine a SafeBrakingLimit x 0,99, plus fort).
- m = 1 m : marge numerique de la chaine arriere (arrondi de la racine).

Profil par defaut : v_ref = 8 m/s donne d1 + 17,2 m ; pire cas a 1,1 x la vitesse desiree, d1 + 20,6 m. Mesure : H moyen
18,4 m, 26,7 m au plus (d1 grand sur les lignes droites, peu echantillonnees).

### Hypotheses

1. La commande ne depend du plan que par `SpeedAt(preview)`, `Binding`, `LimitingConstraint` et la deceleration de
   planification (lecture de `MotionCommand.Track` et de l'arbitrage).
2. Toute contrainte de la passe arriere situee en d >= H atteint les noeuds 0 et 1 a au moins
   sqrt(2 b_plan (H - d1)) = sqrt(v_ref^2 + 2 b_plan m) > v_ref >= plafond de ces noeuds : la comparaison stricte de la
   passe arriere ne la retient pas, l'arret terminal en H non plus.
3. La passe avant et la vitesse courante ne depassent pas v_ref avant d1.
4. L'atteignabilite du plafond (`CeilingReachable`) ne peut basculer qu'en un noeud ou la deceleration depuis v courante
   est encore positive, donc a moins de v^2 / (2 b_plan) < H.
5. La geometrie est validee (Gate A) : aucun defaut d'horizon ni plafond invalide au-dela de H sur MVP_Run.

### Pire cas et preuve

- Chaque pas replanifie : une contrainte de planification (limite de courbe, plafond de braquage, limite de route, fin de
  route) entre dans H au moins v_ref^2 / (2 b_plan) + m avant d'etre atteinte, donc assez tot pour ralentir a b_plan.
- Preuve empirique : 7 838 commandes comparees au bit pres (6 vitesses de 0 a 1,1 x v desiree par etat) : 0 difference,
  0 plan borne refuse alors que le plan complet est accepte. A refaire dans la preuve finale sur les memes 11 routes.

### Pourquoi la perception n'est pas bornee

L'IDM reagit a un leader ou a un obstacle bien au-dela de la distance d'arret, pour le confort : derriere un leader arrete
a 8 m/s, s* = s0 + v T + v^2 / (2 sqrt(a b)) = 32,5 m. Aujourd'hui l'acceleration vaut -0,44 m/s2 a 60 m. Si le leader
n'etait percu qu'a H (18 a 27 m), l'IDM demanderait d'un coup -2,2 a -4,7 m/s2, au-dela du freinage sur. Aucun danger ne
doit donc etre exclu par H : la perception garde sa portee actuelle (toute la route restante).

Ce que la perception lit de l'horizon : la longueur (portee du leader), les intervalles (element, s, distance de debut), la
boite de toutes les positions de points (requete spatiale des obstacles) et la tangente du point le plus proche (vitesse
d'un obstacle le long du chemin). Tout cela se calcule exactement depuis les intervalles de toute la route et des caches
par element (boite des positions d'echantillons, recherche du point le plus proche), sans construire les points ni
transporter e. A prouver au bit pres par le banc etendu a des frames multi-acteurs (leaders et obstacles proches et
lointains, tires des traces A, B et explore-4).

## 3. Ce qui change reellement dans les sorties publiques

| Sortie | Change | Ne change pas |
|---|---|---|
| Horizon de planification | longueur H, fin LookAheadLimit, intervalles, points et raccords jusqu'a H seulement ; un defaut de geometrie au-dela de H est detecte plus tard | points, e et plafonds a l'interieur de H, au bit pres |
| MotionPlan | diagnostic `HorizonTruncated` leve ; plafond invalide au-dela de H detecte plus tard | issue et couverture a l'interieur de H |
| SpeedPlan | points jusqu'a H seulement ; dernier noeud lie par `HorizonTerminalStop` ; verification sur [0, H] | vitesse visee, `Binding`, `LimitingConstraint`, deceleration de planification, donc la commande |
| Perception (leader, obstacles, y compris lointains) | rien, par construction ; a prouver au bit pres | portee, faits, saturations |
| Projection et trace | longueur d'horizon et nombre de points du plan publies plus petits | liantes, contraintes nommees, commandes |
| Commande et trajectoire | rien | 5.31, 5.52 et 5.33 PlayMode a rejouer inchanges (memes nombres de pas, d max, aucune difference de contact) |

## 4. Plan d'implementation (apres approbation)

1. Occupation : projection elaguee exacte a deux niveaux de blocs (banc : 0 difference ; cible >= 5x au lieu de 1,76x).
2. Horizon de planification borne a H et construit depuis les caches par element (exact, prototype valide).
3. Perception sur toute la route depuis les intervalles et les caches par element, sans construire les points (exact).
4. Verification du profil exacte optimisee (tableaux plats, curseurs) ; tailles exactes ; projection de diagnostic
   construite a la lecture.
5. Preuves : banc EditMode (commandes et faits de perception au bit pres), 5.31, 5.52 et 5.33 PlayMode inchanges,
   `Story533Perf` contre la cible.

Cout attendu par vehicule et par pas en population pleine : horizon + plan ~0,37 ms, occupation ~0,2, perception ~0,2,
reste ~0,3, soit ~1 ms ; allocations divisees par ~8, donc environ une collection GC toutes les 8 s par vehicule au lieu
d'une par seconde.
