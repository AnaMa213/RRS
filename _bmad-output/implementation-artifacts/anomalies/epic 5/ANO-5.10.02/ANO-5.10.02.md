---
id: ANO-5.10-02
title: AI vehicles can enter a circular/orbiting movement around specific road zones
status: resolved
epic: 5
story: 5.10
type: bug
category: ai-navigation
severity: major
priority: high
---

# ANO-5.10-02 — Les véhicules IA peuvent tourner en boucle autour de certaines zones du réseau routier

## Contexte

- **Epic :** 5 — NPC Response Foundation and Future Traffic
- **Story concernée :** 5.10 — Lane Graph, Greybox District, and Routed Traffic
- **Type :** Anomalie fonctionnelle / navigation IA / routing
- **Sévérité initiale :** Majeure
- **Priorité initiale :** Haute
- **Scène concernée :** `MVP_Run`
- **Reproductibilité :** Intermittente / fréquence exacte à déterminer

La Story 5.10 a introduit le nouveau district greybox, le lane graph et le système de routed traffic.

Pendant les tests de recette, certains véhicules IA peuvent adopter un comportement anormal à proximité de certaines petites zones du réseau routier : au lieu de poursuivre normalement leur trajet, ils commencent à tourner autour de la zone et semblent entrer dans une boucle de navigation.

Une capture et une vidéo de reproduction sont fournies avec cette anomalie.

---

## Comportement observé

Pendant la circulation normale du trafic, certains véhicules IA peuvent arriver à proximité de certaines zones particulières du réseau routier puis commencer à tourner autour de celles-ci.

Le véhicule ne semble alors plus progresser normalement vers sa destination.

Il peut effectuer plusieurs changements d'orientation successifs et suivre une trajectoire circulaire ou quasi circulaire autour de la zone concernée.

Plusieurs zones où ce phénomène a été observé sont indiquées en rouge sur la capture fournie.

Ces zones semblent correspondre à de petites formes/portions particulières visibles dans la représentation du réseau routier.

À ce stade, leur nature exacte n'est pas connue du testeur :

- elles peuvent appartenir au lane graph ;
- au système de navigation ;
- à la géométrie des routes/intersections ;
- à des connexions entre plusieurs segments ;
- ou à un autre système.

**Il ne faut donc pas considérer ces zones comme la cause établie du bug.**

La seule observation confirmée est qu'une corrélation spatiale existe entre certaines de ces zones et l'apparition du comportement circulaire.

---

## Comportement attendu

Un véhicule circulant normalement doit :

1. suivre une trajectoire cohérente avec son itinéraire ;
2. traverser correctement les segments et jonctions du lane graph ;
3. effectuer ses changements de direction de manière naturelle ;
4. continuer à progresser vers une sortie valide du district.

Un véhicule ne doit pas entrer dans une boucle circulaire ou quasi circulaire permanente autour d'un point, d'une connexion, d'un nœud ou d'une portion de route.

Si aucun chemin valide vers sa destination n'existe, ce cas doit être détectable comme une erreur de routing/navigation et non se manifester sous la forme d'une boucle infinie de conduite.

---

## Étapes de reproduction

La reproduction exacte doit encore être précisée pendant l'investigation.

### Reproduction actuellement observée

1. Lancer une partie dans `MVP_Run`.
2. Laisser plusieurs véhicules IA circuler dans le district de la Story 5.10.
3. Observer leur comportement à proximité des zones indiquées sur la capture fournie.
4. Attendre qu'un véhicule emprunte l'une des portions concernées.
5. Dans certains cas, constater que le véhicule cesse de progresser normalement.
6. Le véhicule commence alors à tourner autour de la zone et peut rester dans ce comportement pendant une durée anormalement longue.

---

## Éléments de preuve fournis

### Capture

La capture indique en rouge plusieurs zones du réseau près desquelles le comportement a été observé.

Cette annotation représente uniquement les observations du testeur.

Elle ne signifie pas que ces éléments sont nécessairement responsables du bug.

### Vidéo

Une vidéo montre un cas de véhicule adoptant ce comportement pendant une partie.

La vidéo doit être utilisée pendant l'investigation pour :

- identifier le véhicule concerné ;
- reconstruire son itinéraire ;
- déterminer le segment/lane/junction qu'il tente de rejoindre ;
- observer ses changements successifs de cible ou de direction.

---

## Investigation demandée

### 1. Identifier les éléments visibles sur la capture

Déterminer précisément à quoi correspondent les petites zones signalées en rouge.

Documenter notamment si elles correspondent à :

- un élément du lane graph ;
- un nœud ;
- une connexion entre lanes ;
- une zone de transition ;
- un élément du NavMesh ;
- une géométrie générée ;
- un waypoint ou target intermédiaire ;
- un gizmo uniquement visuel ;
- ou autre chose.

Ne pas supposer leur rôle à partir de leur apparence.

---

### 2. Reproduire le comportement avec instrumentation

Lorsqu'un véhicule entre dans la boucle, inspecter au minimum :

- son identifiant ;
- sa route courante ;
- son segment/lane courant ;
- le prochain segment/lane attendu ;
- sa destination actuelle ;
- la prochaine cible utilisée pour diriger le véhicule ;
- les décisions de routing successives ;
- les changements éventuels de cible ;
- la progression enregistrée sur la route ;
- son orientation désirée ;
- son orientation réelle.

L'objectif est de déterminer **pourquoi le véhicule continue à demander une trajectoire circulaire**.

---

### 3. Vérifier le lane graph

Inspecter particulièrement les connexions autour des zones concernées.

Rechercher notamment :

- connexions dans le mauvais sens ;
- connexions bidirectionnelles involontaires ;
- auto-connexion d'un segment ;
- cycle très court entre plusieurs nœuds ;
- plusieurs sorties contradictoires ;
- connexion vers le segment dont le véhicule vient de sortir ;
- nœuds dupliqués ;
- segments superposés ;
- orientation/tangent incorrecte ;
- target située derrière le véhicule ;
- route reconstruite à chaque frame ou trop fréquemment ;
- absence de progression vers l'étape suivante du trajet.

Cette liste constitue uniquement des pistes d'investigation.

---

### 4. Vérifier le système de steering

Déterminer si le routing est correct mais que le contrôleur de conduite n'arrive pas à atteindre correctement sa cible.

Vérifier notamment si :

- la cible de steering est trop proche du véhicule ;
- la cible se retrouve latéralement ou derrière lui ;
- le seuil permettant de considérer une cible comme atteinte est incorrect ;
- le véhicule dépasse sa cible puis essaie constamment de revenir dessus ;
- une nouvelle cible est sélectionnée alternativement de part et d'autre du véhicule ;
- le rayon de braquage du véhicule est incompatible avec la géométrie locale.

Ne pas modifier les paramètres de conduite uniquement pour masquer un problème de graph/routing.

---

### 5. Vérifier la nature déterministe du problème

Déterminer si :

- le même véhicule boucle toujours au même endroit ;
- seuls certains itinéraires déclenchent le problème ;
- certaines directions de circulation sont concernées ;
- le problème dépend du point d'entrée utilisé ;
- le problème dépend du choix aléatoire effectué à une junction ;
- plusieurs véhicules peuvent entrer simultanément dans la même boucle.

---

## Hypothèses initiales — NON CONFIRMÉES

Plusieurs catégories de causes pourraient produire ce comportement.

### Hypothèse A — Problème de lane graph / routing

Le véhicule pourrait entrer dans un cycle entre plusieurs nœuds ou connexions.

Par exemple :

`A -> B -> C -> A`

ou :

`A -> B -> A -> B`

Cette hypothèse est plausible mais non démontrée.

### Hypothèse B — Target de conduite impossible à atteindre

Le routing pourrait être correct mais la cible utilisée par le steering pourrait provoquer une poursuite circulaire permanente.

### Hypothèse C — Connexion ou orientation incorrecte

Une connexion de lane pourrait être valide structurellement mais orientée dans la mauvaise direction.

### Hypothèse D — Géométrie / navigation

Les zones indiquées pourraient avoir une influence sur le système de navigation ou les transitions entre segments.

---

## Important

Ne pas commencer par essayer de supprimer, déplacer ou modifier les zones entourées en rouge.

Nous ne savons actuellement pas ce qu'elles représentent ni si elles sont responsables du problème.

La priorité est :

**identifier leur fonction -> reproduire le bug -> observer la décision de l'IA -> identifier la cause racine -> corriger.**

---

## Contraintes de correction

La correction ne doit pas :

- introduire de téléportation comme mécanisme normal de récupération ;
- supprimer arbitrairement un véhicule lorsqu'il boucle ;
- forcer le véhicule vers une autre position pour masquer le problème ;
- introduire une exception spécifique aux coordonnées des zones concernées ;
- hardcoder un itinéraire particulier ;
- désactiver une connexion du lane graph sans comprendre pourquoi elle produit le problème ;
- casser la capacité des véhicules à sélectionner plusieurs routes possibles.

La correction doit traiter la cause du comportement.

---

# Critères d'acceptation

## AC1 — Absence de boucle de navigation

**Given** un véhicule IA circule dans le district  
**When** il traverse les zones précédemment associées au problème  
**Then** il continue à progresser normalement sur son itinéraire  
**And** il n'entre pas dans une trajectoire circulaire persistante autour d'un point ou d'une zone.

---

## AC2 — Traversée dans plusieurs directions

**Given** une zone précédemment concernée par l'anomalie  
**When** plusieurs véhicules l'abordent depuis les différentes directions valides du lane graph  
**Then** chaque véhicule peut poursuivre vers une sortie valide de la junction  
**And** aucun trajet valide ne provoque une boucle permanente.

---

## AC3 — Routing cohérent

**Given** un véhicule possède une route valide  
**When** il atteint une junction ou une transition de lane  
**Then** son prochain segment appartient à une connexion valide du lane graph  
**And** la progression vers les segments suivants reste cohérente  
**And** le véhicule ne reboucle pas involontairement sur les segments qu'il vient de quitter.

---

## AC4 — Cause racine identifiée

**Given** le comportement circulaire a été reproduit  
**When** l'investigation est terminée  
**Then** la cause exacte est identifiée et documentée  
**And** la relation éventuelle entre les zones signalées sur la capture et le comportement est expliquée.

---

## AC5 — Pas de workaround destructif

**Given** le bug est corrigé  
**When** un véhicule rencontre la situation précédemment défaillante  
**Then** aucune téléportation, suppression ou réinitialisation arbitraire n'est nécessaire pour poursuivre la route.

---

## AC6 — Non-régression Story 5.10

**Given** la correction est appliquée  
**When** les tests de la Story 5.10 sont rejoués  
**Then** :

- les véhicules continuent à apparaître uniquement depuis les portals ;
- plusieurs itinéraires restent possibles ;
- les choix de direction aux junctions fonctionnent toujours ;
- les véhicules continuent à progresser vers une sortie ;
- les véhicules despawn uniquement aux portals ;
- le target headcount continue à être maintenu correctement.

---

## Instrumentation recommandée pour la validation

Si elle n'existe pas déjà, ajouter ou utiliser temporairement une visualisation de développement permettant d'afficher pour un véhicule sélectionné :

- current lane/edge ;
- next lane/edge ;
- current junction ;
- current steering target ;
- route ou historique récent des edges ;
- destination/exit recherchée ;
- état de progression.

Cette instrumentation doit servir au diagnostic et peut rester comme outil de debug si elle est générique et utile aux Stories 5.11/5.12.

Elle ne doit pas devenir une dépendance fonctionnelle du système.

---

# Instruction BMAD

Traiter cette anomalie comme un problème de navigation/routing nécessitant une analyse de cause racine.

Ne pas partir du principe que les zones entourées en rouge sont responsables du comportement : elles indiquent uniquement les endroits où le testeur a observé le phénomène.

Commencer par déterminer ce que ces éléments représentent dans l'implémentation de la Story 5.10.

Reproduire ensuite le problème en observant l'état interne du véhicule concerné et son chemin à travers le lane graph.

Déterminer si la cause appartient principalement :

1. au graph/routing ;
2. à la sélection de la prochaine cible ;
3. au steering/conduite ;
4. à la géométrie/navigation ;
5. ou à l'interaction de plusieurs de ces systèmes.

Avant d'effectuer un changement architectural important, expliquer la cause identifiée et vérifier que la correction reste compatible avec les futures Stories 5.11 et 5.12.

Après correction, documenter :

1. la cause racine ;
2. la signification des zones signalées sur la capture ;
3. les fichiers/assets/scènes modifiés ;
4. la correction appliquée ;
5. les tests exécutés ;
6. les éventuels risques restant à surveiller.

---

# Résolution — 2026-09-16

## 1. Cause racine

**Catégorie : sélection de cible / steering.** Ni le lane graph ni le routing ne sont en cause.

`NetworkedAIVehicleDriverController` est un contrôleur de **poursuite pure** : il vise le nœud
courant, borne son braquage à `steerDegreesPerSecond` (90 °/s sur `Greybox_AIVehicle`) et roule à la
vitesse désirée du profil (8 m/s sur `DriverProfileDef_Default`). Son rayon de braquage minimal est
donc **R = v / ω = 8 / 1,571 = 5,09 m**, alors que le franchissement de nœud était validé
**uniquement** par `HasArrivedAtWaypoint`, c'est-à-dire une distance planaire ≤ `arrivalRadius`
(3 m).

Ces deux valeurs sont incompatibles : **un nœud qui tombe à l'intérieur du cercle de braquage du
véhicule ne peut jamais être atteint.** Le véhicule tourne autour de lui à un rayon ≈ R = 5,09 m,
n'entre jamais dans le rayon d'arrivée de 3 m, donc `ResolveNextNode` n'est jamais appelé, donc
`WaypointIndex` ne change plus jamais. L'orbite est permanente et parfaitement stable.

État mesuré sur les véhicules en orbite (identique sur tous les cas) :

| Grandeur | Valeur observée |
|---|---|
| Distance à la cible | 3,9 – 6,9 m (toujours entre `arrivalRadius` et 2R) |
| Angle vers la cible | ±91° à ±109° — cible **par le travers** |
| Nœud courant / suivant | **figés**, aucune progression |
| `traversedEdges` | figé |
| Recalcul de route | **aucun** — la route n'est jamais recalculée pendant le phénomène |

Réponses aux questions de diagnostic du handoff : la route logique est correcte et acyclique (1) à
(4) ; la progression sur la lane n'avance pas (5) ; le steering target est **fixe** (6, 9) et ne
change ni n'oscille (10, 11) ; la distance à la cible ne diminue pas, elle se stabilise à R (7) ; la
cible finit **latéralement** au véhicule (8) ; aucun recalcul de route ni de prochain edge (12, 13) ;
l'orientation désirée est cohérente avec le routing (14) ; **le véhicule ne possède pas le rayon de
braquage nécessaire (15)** — c'est le seul point qui échoue.

## 2. Ce que représentent réellement les zones signalées

Ce sont les **quatre giratoires** du district : `Roundabout_NorthEast`, `Roundabout_SouthEast`,
`Roundabout_SouthWest`, `Roundabout_NorthWest`, instances de `Greybox_Roundabout.prefab`, chacune
portant un anneau de 11 `LaneNode` (`Ring_Split_*`, `Ring_Arc_*`, `Ring_Merge_*`) de **rayon 6,00 m**.

Ce sont donc bien des éléments du lane graph — mais **elles ne sont pas la cause**. Elles sont
l'endroit où la poursuite pure roule au plus près de sa limite physique :

- rayon de l'anneau **6,00 m** contre un rayon de braquage minimal de **5,09 m** — 0,9 m de marge ;
- espacement des nœuds d'anneau **3,43 m** contre un rayon d'arrivée de **3,00 m** — 0,43 m de marge.

La moindre perturbation (contact avec un autre véhicule ou avec le joueur) suffit alors à faire
passer la cible à l'intérieur du cercle de braquage. Le même défaut existe partout dans le district ;
les giratoires sont simplement le seul endroit où il se déclenche en pratique. Les quatre nœuds de
blocage identifiés sont tous des nœuds d'anneau : `Ring_Merge_Diagonal`, `Ring_Split_West`,
`Ring_Arc_Diagonal_1`, `Ring_Arc_South_1`.

## 3. Comment le bug a été reproduit

Reproduction déterministe hors Play Mode, par **rejeu cinématique** du contrôleur sur le graphe réel
de `MVP_Run` (mêmes équations : `HasArrivedAtWaypoint`, `ComputeSeekIntent`, `ApplyMovement`,
`ResolveNextNode`, `Time.fixedDeltaTime` = 0,02 s), exécuté dans l'Éditeur via Unity MCP.

- Départ parfaitement aligné sur un portail : **0 orbite sur 48 trajets** — le bug ne se voit pas.
- Avec **une** perturbation unique en cours de route (décalage latéral + changement de cap, ce que
  produit un contact) : **56 orbites sur 320 trajets (18 %)**, puis **996 sur 2160 (46 %)** sur un
  balayage plus sévère.

Cela explique le caractère intermittent rapporté : le trafic nominal ne déclenche rien, seuls les
véhicules bousculés tombent dedans.

## 4. Données ayant permis d'identifier la cause

1. **Audit statique du graphe** : 204 nœuds, 232 arêtes, **0 self-loop**, **0 connecteur orphelin**,
   4 portails d'entrée, 4 de sortie, `TryValidate` vert. → hypothèse A (cycle de graphe) écartée.
2. **Simulation du routing logique seul** : **160/160 parcours atteignent un portail de sortie**,
   depuis les 4 portails et 40 graines. → le routing ne boucle pas.
3. **Mesure géométrique** : R = 5,09 m contre anneau 6,00 m et `arrivalRadius` 3,00 m.
4. **Signature de l'état bloqué** : cible figée par le travers à une distance ≈ R (tableau ci-dessus).

## 5. Fichiers modifiés

| Fichier | Nature |
|---|---|
| `Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs` | Nouveau prédicat pur `HasPassedUnreachableWaypoint` |
| `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` | Le prédicat complète la condition de franchissement de nœud |
| `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` | 6 gardes de non-régression |

Aucune scène, aucun prefab, aucun `Def`, aucun tracé n'a été touché.

## 6. Correction appliquée

Un nœud est désormais franchi de **deux** façons complémentaires : l'arrivée nominale (inchangée), ou
le **dépassement irrattrapable** — le nœud est déjà derrière le véhicule **et** tombe dans son cercle
de braquage.

```csharp
if (HasArrivedAtWaypoint(transform.position, waypointPosition, arrivalRadius)
    || LaneGraphRouting.HasPassedUnreachableWaypoint(
        transform.position, transform.forward, waypointPosition, currentSpeed, steerDegreesPerSecond))
```

Propriétés du prédicat :

- **Auto-calibré** : le rayon vient de la vitesse courante et de la vitesse de lacet maximale
  (R = v / ω). Aucun nouveau seuil à authorer, aucun réglage de conduite modifié.
- **S'éteint de lui-même à basse vitesse** : à 1 m/s, R = 0,64 m, le véhicule peut revenir sur son
  repère, le prédicat ne se déclenche pas.
- **Ne porte jamais loin** : un point intérieur au cercle est à au plus 2R (≈ 10 m à 8 m/s). Un
  véhicule retourné à l'autre bout du district revient chercher son repère, comme avant.
- **Ne coupe pas les virages** : la garde « déjà dépassé » (produit scalaire ≤ 0) fait que le nœud
  suivant d'un anneau de giratoire, qui reste **devant**, n'est jamais court-circuité.

Aucun des interdits n'est utilisé : ni téléportation, ni suppression de véhicule, ni reset
silencieux, ni coordonnée en dur, ni itinéraire figé, ni connexion désactivée. Les routes
alternatives, le tirage pondéré aux jonctions et le déterminisme par graine sont intacts.

## 7. Tests exécutés

| Test | Résultat |
|---|---|
| Audit statique du lane graph de `MVP_Run` | 0 self-loop, 0 orphelin, `TryValidate` vert |
| Routing logique, 4 portails × 40 graines | 160/160 atteignent une sortie |
| Rejeu cinématique nominal, 48 trajets | 48/48 atteignent une sortie |
| Balayage perturbé 320 trajets, A/B | **56 orbites → 0** |
| Balayage perturbé sévère 2160 trajets, A/B | **996 orbites → 0** |
| Profil de déclenchement sur 100 trajets nominaux | 3,8 % des franchissements, toujours à ≥ 90° (jamais en avance) |
| Non-régression des trajets nominaux | Portail-à-portail conservé sur 100/100 |
| 6 nouvelles gardes EditMode | Vertes (évaluées directement dans l'Éditeur) |

## 8. Risques et points à surveiller

- **Les itinéraires nominaux changent** (44 % des trajets comparés). Conséquence attendue et
  acceptée : `SelectWeightedSuccessor` est graine par `traversedEdges`, donc consommer un nœud un
  instant plus tôt décale le tirage. Les trajets restent valides, pondérés, déterministes et
  portail-à-portail — mais un trajet donné n'est plus identique à celui d'avant le correctif.
- **La marge géométrique reste faible** : anneau 6,00 m contre R 5,09 m. Le correctif garantit la
  progression, pas une trajectoire élégante — un véhicule bousculé rejoint sa voie en élargissant.
  Si la Story 5.11/5.12 augmente `desiredSpeed` au-delà de ~9,4 m/s, R dépasse le rayon de l'anneau
  et les giratoires deviendront intraçables ; il faudra alors ralentir en courbure ou élargir les
  anneaux.
- **92 arêtes de longueur nulle** (jointures de connecteurs, deux nœuds superposés). Sans effet sur
  cette anomalie, mais `ComputeSeekIntent` rend `Idle` sur ces nœuds : le véhicule s'immobilise
  ~2 frames physiques à chaque jointure de module. Cosmétique, hors périmètre de cette ANO.
- **ANO-5.10-03** (réaction à la collision) reste ouverte. Ce correctif empêche un véhicule bousculé
  de rester en orbite, mais ne lui donne pas de réaction naturelle à un choc — c'est le périmètre de
  l'autre anomalie.

## 9. Outils de debug ajoutés

Aucun composant de debug runtime n'a été ajouté : le diagnostic a été mené par rejeu cinématique hors
Play Mode, plus fiable et reproductible qu'une instrumentation en scène. Ce rejeu est **conservé dans
le projet** sous forme de garde de non-régression
(`PerturbedVehiclesAlwaysReachAnExitPortalInTheAuthoredDistrict`), directement réutilisable par les
Stories 5.11 et 5.12 pour vérifier qu'un changement de profil de conduite ou de tracé ne réintroduit
pas d'orbite.
