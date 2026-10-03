# Proposition D15 : optimisation structurelle exacte du pas Traffic V2 (Story 5.33)

Demande proprietaire du 2026-10-03, apres le diagnostic `analysis-20261003-roundabout-vs-straight.md`.

- Contrainte : aucune perte de qualite, aucun changement de comportement. Portees de perception, precision des courbes,
  footprints, geometrie signee et Gate A restent intacts.
- D14 (planification bornee) est conserve et complete, pas remplace.
- Objectif immediat : rond-point N = 8 confortablement sous 10 ms de Traffic V2 total.
- Objectif d'architecture : un cout mutualise qui prepare une flotte de 30 vehicules.

## 0. Constats qui orientent la proposition

- **L'index d'occupation par element existe deja.** `TrafficFrame` construit une fois par pas, pour chaque element, les
  occupants tries par (sMin, id). Leader, follower, adjacents et sortie ne reprojettent aucun acteur : ils lisent cet
  index en O(etendues x k).
- **Le O(N^2) vient d'un seul canal : les obstacles.** La boite de requete est l'AABB de toute la route (D14). Elle
  contient presque tous les autres acteurs. Chacun est reprojete par chaque observateur, sans elagage, sur chacune des
  13 a 16 etendues de la route.
- **La densite de preuve est traversee directement a 50 Hz.** Les mouvements de rond-point sont la geometrie signee
  Gate A : environ 230 echantillons pour 13 m, soit 17,6 par metre. `RoadCurve.Project` parcourt tous les segments de
  la courbe : 230 iterations, meme pour une plage de 5 m.
- Cette projection lineaire sert a la localisation, a la perception, a la reference de mesure (deux fois par pas et par
  vehicule) et a `MotionCommand`. L'occupation utilise `ProjectNearest` (D14), mais son amorce sous le point de
  reference laisse un rayon d'environ la demi-longueur du vehicule : il reste 37 segments par point en rond-point.
- **La localisation examine les 116 elements du modele a chaque appel**, puis projette sans elagage tous ceux dont la
  boite elargie contient le point : 8 en rond-point, 2 en ligne droite.
- `SpatialQuerySaturated` (plus de 32 entrees dans l'AABB de la route) est **fonctionnel** : il rend la perception
  indisponible pour l'arbitrage. Ce comptage doit rester exactement le meme.

## 1. Architecture actuelle et architecture proposee

| Etape | Actuelle | Proposee |
|---|---|---|
| Projection sur une courbe | parcours lineaire de tous les segments, comparaison stricte par abscisse croissante | **projection exacte acceleree** (section 2) : plage de segments par dichotomie, amorce par la boite la plus proche (ou un s connu), puis meme parcours dans le meme ordre, en sautant ce qui ne peut ni gagner ni egaler |
| Occupation (132 points) | `ProjectNearest`, amorce sous le point de reference | meme fonction, **amorce chainee** : le s du point precedent du perimetre (coherence spatiale, distance d'amorce ~ distance vraie) |
| Localisation | 116 boites testees, ~8 projections lineaires | **index spatial des elements** du modele compile (grille XZ, construite une fois). Il rend exactement les elements dont la boite elargie contient le point, puis le meme `Consider` avec la projection acceleree |
| Obstacles | AABB de la route, puis chaque entree x chaque etendue : `Project` lineaire | meme AABB (compte et saturation inchanges) ; une etendue n'est projetee que si l'entree est **proche de son element** (borne prouvee) ; projection **mutualisee dans la frame** pour les etendues completes (memo par (entree, element, plage)), donc calculee une fois par pas pour tous les observateurs |
| Reference de mesure, `MotionCommand` | `Project` lineaire sur la courbe entiere | projection acceleree sur la plage (binaire + amorce) |

Fallback 5.39 : la localisation reste globale par construction. L'index ne remplace pas une recherche « element precedent
d'abord » : il rend le meme ensemble de candidats que le balayage complet, ou que soit le vehicule (deplace, pousse ou
hors route). Il n'existe donc pas de chemin nominal qui puisse rater un candidat.

## 2. Projection exacte acceleree (coeur commun)

`RoadCurve.Project(point, sMin, sMax)` et `ProjectNearest(point, hintS)` deviennent un seul algorithme, exact au bit pres :

1. Memes bornes `sMin`/`sMax` (clamp, plage effondree).
2. Plage de segments `[i0, i1]` : exactement les segments que la boucle actuelle ne saute pas (`s1 >= sMin` et
   `s0 <= sMax`), trouvee par dichotomie.
3. Segments interieurs (`s0 >= sMin` et `s1 <= sMax`) : extremites lues dans les caches D14. Pour eux, `tMin = 0` et
   `tMax = 1` exactement, donc memes `LerpUnclamped`, memes valeurs. Segments de bord (au plus deux) : calcules par les
   memes expressions que la boucle actuelle.
4. Amorce : le segment sous `hintS` s'il est fourni ; sinon descente par la boite la plus proche (super-bloc de 64, puis
   bloc de 8, puis segment). Elle donne une distance d_amorce >= d_min.
5. Parcours par abscisse croissante, comparaison stricte : un super-bloc, un bloc ou un segment dont la boite est a plus
   de d_amorce + 1 mm est saute.

**Invariant.** Un segment saute est strictement plus loin que le minimum, avec une marge tres superieure a l'arrondi.
Le premier segment qui atteint le minimum (la plus petite abscisse a egalite) est donc le meme. `bestS`, `Sample` et les
decalages sont calcules par les memes expressions. Preuve D14 etendue aux plages et a l'amorce par boite. `hintS` ne
change jamais le resultat, seulement le cout.

## 3. Calculs mutualises

- Par modele compile, une fois : grille des elements pour la localisation ; blocs et super-blocs par courbe (deja D14).
- Par frame, une fois pour tous les observateurs :
  - occupation par element (existe) ;
  - pour chaque entree spatiale (acteur ou danger), la liste des elements dont la boite, elargie du rayon d'influence de
    l'observateur, rejoint sa boite. Elle est calculee paresseusement par rayon ; il n'y a qu'un rayon en pratique, meme
    footprint et memes limites pour tous les V2 ;
  - projection d'une entree sur une plage d'element (memo).
- Ce qui reste par observateur : l'enumeration de l'AABB de route (tests de boites seulement, tri en x deja en place),
  necessaire pour garder `Total` et `SpatialQuerySaturated` identiques. Ensuite, des lectures O(1) et les projections des
  seules paires proches, dont l'etendue partielle propre a l'observateur.

## 4. Complexite (S : segments d'une courbe, ~230 en rond-point ; E = 116 elements ; N vehicules ; k voisins proches)

| Operation | Avant | Apres |
|---|---|---|
| `Project` sur plage | O(S) | O(S/64 + 8 + 8 + segments a portee), en pratique quelques dizaines d'operations |
| Occupation par vehicule | 132 x O(segments dans ~2 m) | 132 x O(1) amorti (amorce chainee) |
| Localisation par vehicule | O(E) + ~8 x O(S) | O(cellule) + ~8 x projection acceleree |
| Obstacles, total du pas | O(N x N x etendues x S) | O(N x k_AABB) tests de boite + O(paires proches) projections memoisees |
| Construction de la frame | O(N x 132 x S_local) | O(N x 132) + O((N + dangers) x k) pour l'index de proximite |

Le terme residuel O(N x k_AABB) n'est qu'un test de boites (quelques ns) conserve pour l'exactitude du compte de
saturation. Pour 30 vehicules, il represente environ 900 tests par pas.

## 5. Caches et invalidation

| Cache | Portee | Invalidation |
|---|---|---|
| Blocs, super-blocs et segments par courbe (D14) | `RoadCurve`, immuable | aucune : un nouveau modele construit de nouvelles courbes |
| Grille des elements (localisation) | `CompiledRoadModel`, paresseuse, une fois | aucune : le modele compile est immuable ; une autre version est un autre objet |
| Proximite entree -> elements, memo de projections | `TrafficFrame`, paresseux | durent une frame, soit un pas hote ; la frame suivante repart a vide. Fonctions pures des entrees de la frame, donc le meme resultat quel que soit l'observateur qui les remplit |

## 6. Memoire supplementaire

- Grille des elements : cellules de 8 m sur l'emprise de MVP_Run, quelques milliers d'indices (< 50 Ko), une fois par
  modele.
- Par frame :
  - proximite : (N + dangers) x ~4 indices ;
  - memo : au plus une projection par paire proche (environ 80 octets).
- Pour 30 vehicules et 20 dangers, de l'ordre de 20 Ko par pas, a mettre en regard des ~55 Ko alloues par vehicule et par
  pas aujourd'hui.
- Aucune memoire par courbe au-dela de D14.

## 7. Invariants d'exactitude (prouves en EditMode avant chaque mesure)

- **E1.** `Project(point, sMin, sMax)` est egal au bit pres a l'algorithme lineaire de reference (copie dans le test) :
  s, lateral, normal, distance, depassement, repere. Corpus : projections de localisation, de perception, de reference de
  mesure et de `MotionCommand` relevees sur les 11 routes et les grappes du banc, plus des plages aleatoires
  deterministes.
- **E2.** `ProjectNearest(point, h)` est egal a `Project(point)` quel que soit h (preuve D14 conservee).
- **E3.** Localisation : l'index rend exactement l'ensemble des elements dont la boite elargie contient le point
  (compare au balayage complet sur le corpus). Le tri (rang, score, id) est total, donc `RoadLocation` est identique
  (compare champ a champ).
- **E4.** Obstacles : meme `Total` et meme saturation (meme AABB et meme index) ; une etendue n'est ecartee que si la
  borne prouve qu'aucun fait n'est possible ; memes faits au bit pres. Compare a une copie de reference du canal actuel
  sur toutes les grappes du banc, N = 1 a 8, dans les deux situations.
- **E5.** Rien ne change dans la geometrie, les echantillons, les artefacts Gate A, les footprints, les portees, les
  limites ou les tolerances.

## 8. Risques pour 5.31, 5.32 et 5.52

| Story | Point de contact | Risque | Garde |
|---|---|---|---|
| 5.31 | `ReferenceTrack.Project` (mesure de d), `MotionCommand.Track` | une projection differente changerait d ou la commande | E1 au bit pres ; rejeu Story531 PlayMode (d max 0,1993 m, memes indicateurs) |
| 5.32 | localisation de la frame, perception | un candidat ou un fait different | E3, E4 ; tests Story532 |
| 5.52 | `RoadGeometryValidator` et la regeneration Gate A utilisent `RoadCurve.Project` | un ecart changerait des preuves signees | E1 ; tests Story552 EditMode (empreintes et preuves) ; Story552 PlayMode Gate B (936 pas, d max 0,1993 m). Aucune donnee signee n'est regeneree |

## 9. Plan par etapes mesurables

Apres chaque etape : preuves d'exactitude, tests 5.33, puis banc `Story533RoundaboutBench` (ligne droite et rond-point,
N = 1, 2, 4, 8).

1. **Projection acceleree** (`RoadCurve`) et **amorce chainee de l'occupation**.
2. **Index spatial de localisation** (`CompiledRoadModel`, `RoadLocalizer`).
3. **Obstacles** : proximite par element et memo de projections dans la frame, rejet prouve par etendue.
4. Si la cible n'est pas atteinte : verification du profil par curseurs au lieu de recherches dichotomiques par noeud
   (exacte), et transport de e de la localisation restreint aux candidats utiles (a prouver).

En fin de passe : rejeu Story531, Story552 et Story533 PlayMode inchanges, puis Story533Perf contre la cible.
