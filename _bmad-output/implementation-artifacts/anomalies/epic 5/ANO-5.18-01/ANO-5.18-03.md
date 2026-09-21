---
id: ANO-5.18-03
title: Geometrie de jonction — grands arcs, arrets au milieu du carrefour, interblocage central
status: fixed
epic: 5
story: 5.18
type: bug
category: ai-traffic-geometry
severity: critical
priority: highest
reported: 2026-09-21
fixed: 2026-09-21
evidence:
  - ANO-5.18-03.mp4
---

# ANO-5.18-03 — Le carrefour est represente par son centre

## Cause de fond, commune aux trois symptomes

Une jonction est authoree comme **un seul noeud de decision pose au CENTRE de l'aire de conflit**,
relie directement aux noeuds de sortie de l'autre cote. Il n'existait ni frontiere d'entree, ni ligne
d'arret, ni connecteur de virage, ni occupation.

Tout ce que le trafic calcule a une jonction se calculait donc depuis le milieu du carrefour : ou
s'arreter, ou commencer a tourner, et qui revendique. Les trois symptomes rapportes en sont trois
lectures.

**Mesures du district `MVP_Run` (sondes `Story518JunctionGeometryProbe` et `Story518TopologyProbe`,
2026-09-21).** Carrefour central : `Col_Roadway` de 16 x 16 centre sur l'origine, chaussee
transversale large de 8 m (`z` dans [-4, 4]).

```
n=0   (-2,  8)  cap sud   approche=False  succ=[8]        <- frontiere, DEJA presente
n=8   (-2,  0)  cap sud   approche=True   succ=[7 3 5]    <- noeud de DECISION, au centre exact
n=7   (-8,  2)  cap ouest                                 <- sortie, frontiere opposee
```

Les quatre approches du carrefour central sont a **2,00 m** du centre ; les quatre frontieres
existaient deja a **8,00 m**. Le graphe portait la bonne donnee, le trafic ne s'en servait pas.

---

## 1 — Virages en grand arc et empietement sur la voie opposee (secondes 2-5)

Le connecteur partait du noeud de decision. Pour le tourne-a-droite `8 -> 7`, le coin naturel des
deux axes de voie est `(-2, 2)` : le mouvement commencait donc **2 m APRES son propre virage**. La
Bezier cubique, dont les points de controle valaient une demi-corde le long de chaque cap, poussait
alors 3,16 m plus au sud avant de crocheter vers le nord-ouest.

```
sommet de l'arc mesure : (-2,70 ; -0,96)
voie est (sens oppose) : z = -2
```

Le tourne-a-droite descendait ainsi a moins d'un metre du couloir de la voie est, qu'il n'avait
aucune raison de rencontrer. C'est le grand arc de la video, et c'est aussi ce qui mettait en conflit
des vehicules independants.

**Correction.** Le mouvement part de la **frontiere** et non du noeud de decision, et la courbe est
une Bezier **quadratique dont l'unique point de controle est le coin des deux axes de voie**
(`LaneGraphRouting.TryResolveLaneAxisCorner`). Elle est tangente aux deux voies par construction et
reste dans le triangle entree/coin/sortie.

```
avant : entree (-2, 0)  sommet (-2,70 ; -0,96)  recul 3,16 m
apres : entree (-2, 8)  coin (-2, 2)  sommet (-3,50 ; 3,50)  recul 0,00 m
```

Le sommet degage de 0,70 m le coin de trottoir `Col_Sidewalk_Corner_NW`, dont l'angle de chaussee est
a `(-4, 4)`. Aucune constante n'est introduite : le coin est un resultat de geometrie.

L'avancement sur le connecteur se lit desormais **sur la courbe**
(`ResolveJunctionTurnProgress`) et non par un rapport de distances a la sortie, qui mesurait une
corde : sur une courbe qui bombe, il retardait la visee et faisait sortir large.

---

## 2 — Arret AU MILIEU de l'intersection (feux et stops)

Il n'existait aucune cible d'arret. La regle etait : *quand le centre du vehicule passe a moins de
`arrivalRadius` (3 m) du noeud d'approche, freinage a fond.* Le noeud d'approche etant le centre du
carrefour, un feu rouge immobilisait le vehicule avec son centre quelque part dans `z` dans [-3, 3],
donc son capot en plein dans la chaussee transversale. Rien dans la chaine ne referencait l'avant du
vehicule.

**Correction, en deux temps.**

1. La **frontiere de conflit** est mesuree sur le graphe, a la construction : le premier point ou un
   mouvement de cette approche rencontre le mouvement d'une autre approche de la meme jonction
   (`LaneGraph.ResolveJunctionBoundaries`). Les 16 approches du district la lisent a **6,00 m** de
   leur frontiere d'entree.
2. La **ligne d'arret** est cette frontiere reculee de la demi-largeur du vehicule et de la marge
   authoree, et elle vise l'**AVANT** du vehicule, pas son pivot.

```
frontiere (-2, 8) -> conflit a 6,00 m -> ligne d'arret pour l'avant a 4,67 m, soit (-2 ; 3,33)
centre du vehicule a l'arret : (-2 ; 5,55)     avant : (-2 ; 3,33)
voie transversale la plus proche : z = 2, emprise jusqu'a z = 3,03
```

L'avant s'immobilise 0,30 m avant l'emprise du trafic transversal — exactement la marge authoree.

La ligne n'est pas un declencheur mais une **cible** : elle est remise au modele longitudinal deja en
place comme un leader immobile, donc le vehicule y arrive en decelerant au lieu de freiner a fond la
ou il se trouvait.

**Defaut corrige en cours de route, trouve par le banc de flux.** Le maintien a l'arret d'un panneau
STOP etait remis a zero des que la permission etait accordee. Tant que la ligne etait le noeud
lui-meme cela ne se voyait pas ; avec une ligne a atteindre, le vehicule redemarrait, sortait du
seuil d'immobilite, perdait son maintien, et refreinait. Mesure : `AI_Vehicle_Portal_002` immobile
**28,3 s** a **0,74 m** de sa ligne, maintien plafonne a **0,30 s** sur les 1,20 s authorees. Le
maintien est desormais un **acquis** (`junctionStopSatisfied`), remis a zero seulement a
l'engagement ou a la sortie de l'approche : c'est la semantique d'un stop, on s'y arrete une fois.

---

## 3 — Interblocage du carrefour central

`TryDescribeJunctionClaim` lisait la revendication d'un pair sur **son noeud de waypoint courant**. A
l'instant ou un vehicule franchit son noeud de decision, son waypoint devient le noeud de sortie —
qui n'est pas une approche. **Il disparaissait donc de l'arbitrage a l'instant precis ou il entrait
dans le carrefour.** Le suivant rassemblait ses revendiquants, n'en trouvait aucun en conflit, et
entrait a son tour. Les deux se rencontraient au milieu.

L'arbitrage n'etait pas faux : `JunctionRules.Conflicts` teste bien les deux trajectoires
entree/sortie, donc un tourne-a-gauche face a un tout-droit est correctement concurrent. Ce qui
manquait n'etait pas une regle, c'etait la notion d'**occupant**.

**Correction.**

- Un mouvement accorde devient un **engagement** : le vehicule publie sa revendication avec
  `Committed = true` jusqu'a ce que son **arriere** ait depasse le noeud de sortie.
- `JunctionRules.Resolve` fait passer l'occupation **avant la priorite** : reprendre son mouvement a
  un vehicule engage parce qu'il vient de gauche l'immobiliserait au milieu de l'aire de conflit. Le
  verdict reste antisymetrique, et deux occupants concurrents — etat qui ne devrait pas exister —
  retombent sur l'arbitrage ordinaire.
- Un vehicule engage ne cede plus un conflit de trajectoire a un vehicule non engage de la meme
  jonction. Le freinage d'urgence, lui, n'est jamais desactive.
- L'engagement se prend **a la ligne**, jamais douze metres avant : occuper la jonction pendant tout
  le trajet d'approche la fermerait aux autres.

---

## 4 — Faux chefs de file : un vehicule qui nous fait face

```csharp
// avant
var aligned = !mobile || heading.sqrMagnitude < 0.01f
    || Mathf.Abs(Vector3.Dot(heading, pathTangent)) > SameLaneAlignmentDot;
```

La valeur absolue rendait un vehicule **qui nous fait face** aligne sur notre voie. Deux vehicules
arretes nez a nez se declaraient donc mutuellement `FollowingSameLane (leader arrete)` — et comme une
file est une attente legitime (`IsLegitimateWait`), aucun des deux ne declenchait le deblocage. Le
predicat de face-a-face ne les rattrapait pas : il exigeait une vitesse de rapprochement, et les deux
etaient immobiles.

**Correction.** Le sens est nomme (`facing`), le face-a-face se decide sur le **cap** et non sur la
vitesse, et il porte son propre motif `EmergencyBrake`, evalue **avant** celui de file. Un
face-a-face n'est donc plus une attente legitime, et l'echelle de deblocage reprend la main des deux
cotes.

---

## 5 — « Voie de sortie saturee » est desormais une LONGUEUR

Le predicat restait « un usager de la route occupe la zone de degagement ». Il est remplace par une
comparaison de longueurs :

```
libre   = distance, en aval du noeud de sortie, du bord le plus proche du premier occupant
requis  = longueur du vehicule + ecart minimal authore  (4,44 + 0,75 = 5,19 m)
sature  <=>  libre < requis
```

Un vehicule arrete a 5,50 m en aval laisse la place de degager la jonction : il ne sature plus rien.
Le diagnostic porte les quatre grandeurs (`libre`, `requis`, `utile`, `occupants`) et le nom du
premier occupant.

---

## Diagnostic

`DescribeDecision()` compose, **a la demande et pour le seul vehicule selectionne dans l'Editeur**
(`OnDrawGizmosSelected` n'est appele que pour lui), le bloc complet :

```
AI_Vehicle_Portal_002
Voie       : 47 -> 42
Jonction   : junction_district_tjunction mvt 47->42 permission=refusee revendications=1 attente=2,4s
Arret      : avant du vehicule a 0,74 m de la ligne, maintien 1,20 s
Connecteur : vers 42 avancement 0 %
Suivi      : aucun
Sortie     : libre=6,00m requis=5,19m utile=6,00m occupants=0
Decision   : JunctionYield -- jonction : maintien a l'arret
Preuve     : jonction=junction_district_tjunction mvt=47->42 permission=refusee revendications=1 arret_dans=0,74m
```

Le connecteur de virage est trace en magenta dans la vue Scene. Aucun cadre de diagnostic permanent
n'est introduit : les autres vehicules ne composent rien.

---

## Sondes de mesure

Rejouables, hors `Assets/` (aucun reimport d'asset) :

- `AgentScripts/Story518JunctionGeometryProbe.cs` — geometrie de chaque jonction, forme des arcs,
  sols environnants.
- `AgentScripts/Story518TopologyProbe.cs` — predecesseurs, successeurs, longueurs d'aretes.
- `AgentScripts/Story518BoundaryProbe.cs` — frontiere d'entree derivee, frontiere de conflit, ligne
  d'arret, coin et sommet de chaque connecteur.
