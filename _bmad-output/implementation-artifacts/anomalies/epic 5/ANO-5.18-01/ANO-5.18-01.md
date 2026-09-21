---
id: ANO-5.18-01
title: Traffic stops on nothing — false exit saturation, false leaders, phantom obstacles, unblocking loops
status: fixed
epic: 5
story: 5.18
type: bug
category: ai-traffic-decision
severity: critical
priority: highest
reported: 2026-09-21
fixed: 2026-09-21
evidence:
  - ANO-5.18-01.mp4
  - ANO-5.18-02.mp4
---

# ANO-5.18-01 — Le trafic s'arrête sur rien

## Cause de fond, commune aux cinq symptômes

L'appartenance à une voie et l'occupation d'une voie étaient décidées par des **enveloppes de
proximité** — une sphère autour d'un nœud, une bande latérale gonflée par l'orientation de l'autre
véhicule, une classification de surface par nom de collider — là où seules **l'identité de voie** et
**l'emprise réelle** répondent.

Les cinq anomalies rapportées sont cinq expressions de cette même substitution. C'est pourquoi les
passes correctives précédentes, qui ajustaient des distances, ne pouvaient pas les faire disparaître :
elles déplaçaient le seuil d'une mesure qui ne répondait pas à la question posée.

---

## 1 — Fausse « voie de sortie saturée » (capture 1)

`ResolveApproachExitRoom` posait une **sphère de 6 m** (`JunctionExitClearanceRadius`) autour du nœud
de sortie et déclarait la sortie saturée dès qu'un `Rigidbody` ou un `CharacterController` s'y
trouvait — sans regarder la voie occupée, le sens de marche, ni la position amont/aval.

**Mesure du 2026-09-21 (`Story518ExitRoomProbe`), district `MVP_Run` :** autour de chaque nœud de
sortie, la sphère contient 4 nœuds de voie, dont **deux appartiennent à la voie de sens OPPOSÉ, à
4,00 m**. `4,00 < 6,00` : un véhicule arrivant en face est donc **toujours** compté comme occupant
notre sortie.

Deux véhicules qui se font face à un feu saturent ainsi mutuellement la sortie de l'autre. Le verdict
est réciproque et ne peut plus se défaire → arrêt définitif des deux. C'est exactement la capture 1.

Comptaient également à tort : un véhicule engagé **dans** l'intersection, un véhicule **quittant** la
voie de sortie, et tout piéton dans les 6 m, trottoir compris.

**Correction.** `TrafficPerception.OccupiesExitLane` — prédicat pur, trois conditions cumulatives :
en **aval** du nœud de sortie, empiétant sur le **couloir** de la voie (mesuré sur l'emprise, pas sur
le centre), et **ne dégageant pas**. Le réglage authoré est inchangé ; c'est la grandeur qu'il désigne
qui passe d'un rayon de proximité à une longueur utile de voie.

---

## 2 — Faux leaders et interblocage d'intersection (capture 2)

Le couloir de voie se mesurait `|écart latéral| ≤ demi-largeur + marge + extension de l'autre projetée
sur notre normale`. Pour un véhicule **perpendiculaire** — c'est-à-dire tout véhicule d'une branche
transversale — cette extension vaut sa demi-**longueur** (2,22 m) et non sa demi-largeur (1,03 m).

```
couloir gonflé = 1,03 + 0,30 + 2,22 = 3,55 m
branches concurrentes d'une même jonction, séparation mesurée = 2,83 m
```

`2,83 < 3,55` : un véhicule arrêté sur **une autre branche** du carrefour tombait dans notre couloir,
devenait notre « leader », et comme il était arrêté → `FollowingSameLane (file : leader arrêté)`.
Combiné à la saturation du § 1, cela ferme le carrefour en attente circulaire.

À noter : la géométrie **seule ne peut pas** trancher ce cas. Le nez du véhicule transversal empiète
réellement sur notre couloir (2,83 − 2,22 = 0,61 m). C'est la **priorité** qui doit l'arbitrer, pas la
poursuite en file.

**Correction.** L'appartenance à la voie se décide par **identité** : le nœud de voie courant du pair
appartient-il à notre trajectoire prévue ? En repli géométrique, le dégagement est mesuré à
l'**emprise** (`PlanarDistanceToBox` / `TryPathClearance`), et un usager mobile n'appartient à notre
file que s'il roule dans notre axe (45°). La garde latérale redondante qui vivait dans
`TrySelectLeader` est supprimée : une seule source de vérité.

---

## 3 — Joueur sur le trottoir lu comme conflit de trajectoire (ANO-5.18-01.mp4)

Tout `CharacterController` participait à la prédiction de trajectoire. Un piéton marchant sur le
trottoir vers la chaussée produisait donc un conflit prédit, alors qu'il ne l'atteindra jamais.

**Correction.** La participation se décide sur la **surface foulée** (`IsOnCarriageway`, rayon vers le
bas + classification de surface déjà en place), pas sur une distance. Et le conflit joueur nommé
(`PlayerImmediateConflict`) exige désormais **trois** conditions simultanées : sur la chaussée, dans
notre voie, et à un écart d'**emprise à emprise** ≤ `MinimumGap + SafetyMargin` (0,75 + 0,30 m
authorés — pas une constante nouvelle). La réponse est un **contournement** via l'échelle de manœuvre
de la Story 5.17, l'arrêt n'étant que le repli si aucune échappatoire n'est validée.

---

## 4 — Obstacle immobile fantôme, boucle infinie (ANO-5.18-02.mp4)

**Objets nommés par la sonde `Story518PhantomObstacleProbe` (2026-09-21)**, retenus « obstacle
immobile dans la voie » avec un dégagement de **0,00 m** :

| Objet | Parent | Hauteur mesurée |
|---|---|---|
| `Rampe_Ouest` | `Relief_DosDane_AvenueCenterToEast` | 0,12 m |
| `Rampe_Est` | `Relief_DosDane_AvenueCenterToEast` | 0,12 m |
| `Relief_MarcheBasse_AvenueCenterToEast` | `GreyboxMap` | 0,12 m |

C'est le **relief roulant de la Story 5.13** — un dos d'âne et une marche basse que le véhicule est
explicitement censé franchir (le banc PlayMode 5.13 le lui demande). Les trois mesurent **0,12 m**,
sous la hauteur franchissable authorée de **0,15 m** (`MaxCurbHeight`).

Ils étaient lus comme des murs parce que `IsLowSurface` gardait la règle de franchissement derrière
une reconnaissance par **nom** de collider (`Col_Roadway`, `Col_Sidewalk_`, `Col_Curb_`), qu'aucun de
ces trois objets ne porte. D'où la boucle : obstacle → manœuvre de contournement → retour sur la voie
→ même obstacle, indéfiniment.

**Correction.** La règle de franchissement s'applique à **toute** surface statique assez basse, quel
que soit son nom. C'est une **suppression** de garde, aucun seuil n'est ajouté.

---

## 5 — Boucle file → déblocage

`blocked` se déduisait de « leader arrêté + je suis lent ». Toute file légitime (feu rouge, priorité,
sortie occupée) devenait donc un blocage au bout du délai de klaxon authoré (2 s), déclenchait une
manœuvre d'évitement, ramenait le véhicule derrière le même leader, et recommençait.

**Correction.** La file légitime est un état nommé (`TrafficQueue`), reconnu en lisant la **cause**
chez le leader lui-même (`IsLegitimateWait`) et non sa vitesse. Un saut suffit : chaque véhicule
interroge son propre leader, donc l'état se propage le long de la file à chaque pas. `StaticObstacle`,
`EmergencyBrake`, `DeadlockRecovery`, `UnblockingManeuver` restent des blocages — l'échelle de la
Story 5.17 conserve tous ses cas d'usage.

---

## Diagnostic

Le motif seul ne se vérifiait pas : « obstacle immobile » sans le **nom** de l'objet est invérifiable
en recette, et c'est ce qui a laissé passer les passes correctives précédentes.

`TrafficDecisionReason` distingue désormais `TrafficQueue`, `ExitSaturated` et
`PlayerImmediateConflict` des états voisins, et `DecisionDetail` nomme le sujet et les grandeurs :

```
FollowingSameLane        leader=AI_Vehicle_03 voie=42/41 ecart=2,4m vLeader=0,0
ExitSaturated            jonction=junction_district_intersection sortie=19 occupant=AI_Vehicle_07 revendications=2
StaticObstacle           objet=Col_Wall_Left ecart=1,2m source=perception/voie=88
PlayerImmediateConflict  joueur=Player_01 ecart=0,84m relation=devant/voie
```

Composé à la cadence de perception (5 Hz), affiché par le libellé monde déjà en place. Aucun cadre de
diagnostic permanent n'est introduit.

---

## Sondes de mesure

Rejouables, hors `Assets/` (aucun réimport d'asset) :

- `AgentScripts/Story518ExitRoomProbe.cs` — contenu réel de la sphère de dégagement de sortie.
- `AgentScripts/Story518PhantomObstacleProbe.cs` — objets statiques retenus dans le couloir de voie.
- `AgentScripts/Story518ReliefProbe.cs` — géométrie exacte du relief incriminé.
- `AgentScripts/Story518TrafficDiagnosisProbe.cs` — entraxe des voies, anneau de giratoire, gabarit.
