# Analyse : cout Traffic V2 en rond-point contre ligne droite (Story 5.33, apres D14)

Demande proprietaire du 2026-10-03 : diagnostic compare, meme population, meme instrumentation, HALT avant toute
modification. Donnees brutes : `roundabout-vs-straight-diagnostic.md` (banc `Story533RoundaboutCostDiagnosticTests`).

## Methode

- Banc EditMode sur la geometrie et le decor reels de MVP_Run, 11 routes 5.31. N = 1, 2, 4, 8 vehicules places dans une
  seule situation, au plus pres d'une ancre (7 m au moins entre centres) : 24 ancres a N = 1, 8 au-dela.
- Ligne droite : 268 emplacements hors carrefour (marge 3 m), |kappa| <= 1/150 m sur +-6 m. MVP_Run n'a aucune droite
  plus stricte a distance des carrefours. Rond-point : 1 084 emplacements dans la frontiere des 4 carrefours Roundabout.
- Pas hote rejoue dans l'ordre du runtime : preparation du driver, collecte physique, TrafficFrame, puis par vehicule
  spine (portee D14), perception sur toute la route, SpeedPlan, arbitrage et commande. Temps = meilleur de 3 executions.
- Compteurs de travail ajoutes au runtime (`TrafficV2WorkCounters`, increments seuls, aucune lecture runtime), relus par
  section.
- Hors banc : simulation physique, VehiclePhysicsBody, pauses GC et cout moteur. Ces postes viennent du diagnostic
  PlayMode du 2026-10-02.

## Resultat (ms par pas hote)

| N | ligne droite | rond-point | ecart | dont frame | dont perception |
|---|---|---|---|---|---|
| 1 | 0,75 | 1,42 | +0,67 | +0,62 | +0,08 |
| 2 | 1,40 | 3,05 | +1,65 | +1,33 | +0,27 |
| 4 | 3,90 | 7,11 | +3,21 | +2,51 | +0,76 |
| 8 | 10,33 | 17,75 | +7,42 | +4,33 | +2,56 |

Allocations : environ 55 Ko par vehicule et par pas dans les deux situations, sans difference propre au rond-point.

## La grandeur qui augmente dans le rond-point : la densite d'echantillons des courbes

Les elements d'un rond-point (mouvements d'entree, d'anneau et de sortie) portent **17,6 echantillons par metre** en
moyenne, contre **0,21 par metre** sur les corridors droits, soit 85 fois plus. Chaque operation qui parcourt les segments
d'une courbe coute donc beaucoup plus cher dans un rond-point. Trois operations le font :

1. **Occupation de la frame** (`TrafficFrame.TryOccupy`) : 132 points du perimetre de l'empreinte projetes par
   `ProjectNearest` sur l'element occupe. Les appels sont identiques dans les deux situations (132) ; les segments evalues
   apres elagage passent de 177 a 4 861 par vehicule (x27). L'empreinte de 4,4 m couvre environ 80 segments d'une courbe a
   5,7 cm, et l'amorce sous le point de reference laisse un rayon de recherche de l'ordre de la demi-longueur du vehicule :
   l'elagage ne peut pas ecarter ces segments. Cout : 0,09 ms en ligne droite, 0,62 ms en rond-point par vehicule.
   C'est le premier poste du rond-point (+0,53 ms par vehicule, +4,3 ms a N = 8).
2. **Localisation** (`RoadLocalizer.Localize`) : elle examine les 116 elements du modele a chaque appel. Les boites
   englobantes des mouvements d'un rond-point se recouvrent : 7,9 elements projetes par vehicule contre 2 (x4), puis
   5,7 candidats contre 2. Chaque projection est un `RoadCurve.Project` non elague sur une courbe dense. Chaque candidat
   transporte aussi e depuis son ancre : 302 pas RK4 contre 75 (x4, 4 sinus par pas). Cout : 0,015 ms en ligne droite,
   0,11 ms en rond-point par vehicule (x7).
3. **Projections d'obstacle de la perception** : un `RoadCurve.Project` non elague par etendue de la route pour chaque
   acteur retenu. Le rond-point ajoute environ 1,5 fois plus de projections et de segments, car la route porte plus
   d'etendues et les etendues d'anneau sont denses.

Ce qui ne change pas avec la situation :
- Horizon de planification : environ 270 points et 3 intervalles, 0,07 ms. La portee bornee D14 est plus courte en
  rond-point (vitesse plus basse), ce qui compense la densite.
- Transport de e dans la spine : 0,025 ms.
- SpeedPlan : 0,26 ms, dont 0,23 ms de verification sur environ 330 noeuds.
- Arbitrage et commande : 0,04 ms.
- Collecte physique : 0,01 ms par vehicule. Le decor rend 48 colliders par requete en rond-point contre 70 en ligne
  droite, donc moins.
- Recherches de route : 0. Une construction d'horizon par pas. ConflictZone examinees et paires d'intention : 0, car aucun
  horizon d'intention n'est publie dans la frame, en runtime comme dans le banc.

## Croissance N = 1 -> 8 (cout par vehicule)

| Section | ligne droite | rond-point | Nature |
|---|---|---|---|
| frame (localisation + occupation) | 0,108 -> 0,108 | 0,73 -> 0,65 | O(N), constante par vehicule liee aux points de courbe |
| spine | 0,11 -> 0,11 | 0,11 -> 0,15 | O(N) |
| SpeedPlan | 0,28 -> 0,28 | 0,26 -> 0,30 | O(N) |
| **perception** | **0,16 -> 0,67 (x4,3)** | **0,23 -> 0,99 (x4,2)** | **O(N^2)** |
| collecte | 0,010 -> 0,015 | 0,011 -> 0,013 | O(N) |

La perception est la seule section quadratique, et elle l'est dans les deux situations. La boite de requete d'obstacles
est l'AABB de **toute la route restante** (environ 1 900 points lus par vehicule) : elle contient presque tous les autres
vehicules de la grappe. Les entrees examinees passent de 1,25 a 5,4 par vehicule. Chaque acteur qui n'occupe pas un
element de l'horizon est projete sur **chaque etendue** de la route (13 a 16), sans elagage : 4,4 projections par vehicule
a N = 1, 58 a N = 8 en ligne droite, 87 en rond-point. A N = 8, la perception est le premier poste (5,3 ms en ligne droite,
7,9 ms en rond-point).

## Classement

| Cause | Classe | Poids a N = 8 (rond-point) |
|---|---|---|
| Occupation : `ProjectNearest` x 132 points sur une courbe a 17,6 ech./m | liee au nombre de points de courbe, O(N) | ~4,3 ms |
| Localisation : 116 elements examines, ~8 projetes sans elagage, transport de e par candidat | liee au nombre d'elements (recouvrement des boites) et aux points de courbe, O(N) | ~0,85 ms |
| Perception obstacles : AABB de toute la route, chaque autre acteur projete sur chaque etendue | O(N^2), amplifiee par les points de courbe | ~7,9 ms |
| SpeedPlan (verification ~330 noeuds) | O(N), independant de la situation | ~2,4 ms |
| Operations repetees qui devraient etre mutualisees | projection d'un meme acteur sur les memes elements refaite par chaque observateur ; deux projections par pas sur la reference de mesure (preparation et composition) | voir pistes |

## Pistes (non appliquees, HALT)

1. **Occupation** : projeter un seul point (la reference, deja localisee) et borner l'intervalle d'empreinte
   analytiquement, ou elaguer avec une amorce par point du perimetre (le s du point precedent) pour un rayon de recherche
   de quelques centimetres. Exactitude a prouver au bit pres comme D14.
2. **Localisation** : index spatial des elements (deja prevu AD-42 / 5.46), projection elaguee, transport de e seulement
   pour les candidats a rang egal au meilleur.
3. **Perception** : borner la boite de requete aux etendues proches de chaque acteur candidat, ou mutualiser la projection
   d'un acteur sur un element dans la frame (l'occupation l'a deja calculee pour son propre element). C'est le seul poste
   quadratique : il porte la cible N = 8 < 10 ms.
4. **Densite des courbes** : environ 230 echantillons pour un mouvement d'environ 13 m. Un sous-echantillonnage
   changerait la geometrie signee (Gate A) : hors 5.33.
