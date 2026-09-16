---
id: ANO-5.10-03
title: AI vehicles are violently launched or flipped near tunnel roundabouts
status: open
epic: 5
story: 5.10
type: bug
category: ai-traffic-physics
severity: major
priority: high
---

# ANO-5.10-03 — Des véhicules IA sont projetés ou s'envolent à proximité des ronds-points devant les tunnels

## Contexte

- **Epic :** 5 — NPC Response Foundation and Future Traffic
- **Story concernée :** 5.10 — Lane Graph, Greybox District, and Routed Traffic
- **Type :** Anomalie fonctionnelle / physique / géométrie routière
- **Sévérité initiale :** Majeure
- **Priorité initiale :** Haute
- **Scène concernée :** `MVP_Run`
- **Reproductibilité :** Fréquente dans certaines zones
- **Zone principalement concernée :** Ronds-points situés devant les tunnels / portals

La Story 5.10 a introduit le nouveau district greybox, ses routes, ses portals et le routed traffic.

Pendant les tests de recette, certains véhicules IA subissent une réaction physique anormale lorsqu'ils circulent à proximité des ronds-points situés devant les tunnels.

Une capture et une vidéo de reproduction sont fournies avec cette anomalie.

---

## Comportement observé

Lorsqu'un véhicule IA circule normalement, il peut parfois sembler entrer brutalement en collision avec quelque chose d'invisible ou avec un élément difficilement identifiable de la chaussée.

La réaction est violente :

- le véhicule est brusquement poussé vers l'avant ou vers le haut ;
- il peut fortement basculer ;
- il peut quitter complètement le sol ;
- dans certains cas, il semble véritablement être projeté dans les airs.

La vidéo fournie montre clairement un véhicule qui commence à basculer puis est soulevé jusqu'à se retrouver quasiment vertical/en l'air.

Le phénomène est observé **très majoritairement, voire presque systématiquement, autour des ronds-points situés devant les tunnels**.

La capture fournie indique en rouge une zone où le comportement a notamment été observé.

Le testeur a l'impression que le véhicule rencontre physiquement quelque chose sur sa trajectoire, mais aucun obstacle évident n'est visible à cet endroit.

Cette impression constitue une observation de gameplay et **pas encore un diagnostic technique**.

---

## Comportement attendu

Un véhicule IA circulant sur une portion valide de chaussée doit rester physiquement stable.

Il doit pouvoir :

1. entrer sur le rond-point ;
2. suivre sa trajectoire ;
3. emprunter la sortie appropriée ;
4. rejoindre ou quitter le tunnel/portal ;

sans subir d'impulsion physique anormale.

Une chaussée considérée comme praticable par le système de trafic ne doit contenir aucun élément invisible ou géométrie provoquant le soulèvement, le retournement ou la projection d'un véhicule.

---

## Étapes de reproduction

### Reproduction actuellement observée

1. Lancer une partie dans `MVP_Run`.
2. Laisser plusieurs véhicules IA circuler.
3. Observer les véhicules approchant des ronds-points devant les tunnels.
4. Attendre qu'un véhicule traverse les zones signalées sur la capture.
5. Dans certains cas, constater une réaction physique soudaine :
   - basculement ;
   - propulsion vers l'avant ;
   - soulèvement ;
   - projection dans les airs.

Le taux exact de reproduction doit être mesuré pendant l'investigation.

---

## Éléments de preuve fournis

### Capture

Une capture montre une zone située sur/à proximité d'un rond-point devant un tunnel où le phénomène est fréquemment observé.

La zone entourée en rouge indique uniquement l'emplacement approximatif associé au problème.

Elle ne constitue pas une identification de sa cause.

### Vidéo

La vidéo de reproduction montre notamment :

1. un véhicule circulant dans la zone ;
2. une modification brutale de son orientation ;
3. le véhicule qui commence à se soulever ;
4. le véhicule projeté dans les airs / positionné presque verticalement.

La vidéo confirme donc qu'il ne s'agit pas seulement d'un problème visuel de mesh : le véhicule subit une réaction physique anormale.

---

# Investigation demandée

## 1. Identifier la géométrie et les colliders présents dans la zone

Inspecter précisément la zone où le véhicule rencontre le problème.

Déterminer quels objets sont présents, notamment :

- meshes de chaussée ;
- colliders de chaussée ;
- colliders de bordure ;
- éléments du rond-point ;
- éléments liés au tunnel/portal ;
- objets invisibles ;
- triggers ;
- volumes de navigation ;
- objets générés au runtime ;
- éventuels objets dupliqués.

Afficher si nécessaire les colliders/gizmos dans Unity afin de comparer leur forme avec la géométrie visible.

---

## 2. Vérifier les collisions invisibles ou incorrectes

Rechercher notamment :

- collider ne correspondant pas au mesh visible ;
- collider légèrement surélevé par rapport à la chaussée ;
- face verticale ou marche invisible ;
- collider traversant la route ;
- plusieurs colliders superposés ;
- MeshCollider généré incorrectement ;
- collider appartenant à un objet pourtant visuellement désactivé ;
- collision avec un élément du portal ;
- collider résiduel d'une ancienne géométrie ;
- objet instancié en double.

Ces éléments sont des pistes d'investigation, pas des diagnostics établis.

---

## 3. Vérifier la géométrie de la chaussée

Déterminer également si le véhicule est projeté par une irrégularité réelle de la géométrie.

Inspecter notamment :

- différences de hauteur entre deux morceaux de route ;
- raccord incorrect entre route et rond-point ;
- triangles ou vertices anormalement positionnés ;
- changement brutal de pente ;
- surfaces qui se chevauchent ;
- trous ou marches entre deux meshes ;
- raccord route/tunnel.

---

## 4. Instrumenter la collision

Lorsqu'un véhicule reproduit le bug, identifier si possible :

- l'objet avec lequel il vient d'entrer en collision ;
- le collider concerné ;
- le `contact point` ;
- la normale de collision ;
- la vitesse du véhicule juste avant l'impact ;
- sa vitesse immédiatement après ;
- son `angularVelocity` ;
- l'impulsion physique reçue.

L'objectif est d'éviter de diagnostiquer le problème uniquement à partir du résultat visuel.

Si nécessaire, ajouter temporairement un logging de développement pour les collisions anormalement violentes des véhicules IA.

---

## 5. Vérifier si le problème vient du véhicule ou de l'environnement

Déterminer si :

- tous les véhicules peuvent reproduire le problème au même endroit ;
- seuls certains véhicules/prefabs sont concernés ;
- le problème dépend de la vitesse ;
- le problème dépend de l'angle d'approche ;
- le problème se produit dans les deux sens de circulation ;
- tous les ronds-points devant les tunnels sont concernés ;
- un rond-point particulier est principalement responsable.

Cette distinction permettra de déterminer si la cause appartient principalement :

1. à la géométrie/collision de la map ;
2. au véhicule ;
3. au contrôleur de conduite ;
4. ou à une combinaison de ces éléments.

---

# Hypothèses initiales — NON CONFIRMÉES

## Hypothèse A — Collider invisible ou mal positionné

La réaction observée est compatible avec un véhicule rencontrant brutalement un collider invisible ou une face de collider qui coupe sa trajectoire.

C'est actuellement une hypothèse forte, mais elle doit être démontrée.

---

## Hypothèse B — Mauvais raccord entre meshes

Le raccord entre une route, le rond-point et/ou la zone du tunnel pourrait créer une petite marche ou une géométrie physique différente de la surface visible.

À vitesse normale, cette irrégularité pourrait agir comme une rampe ou un obstacle.

---

## Hypothèse C — Géométrie/collider dupliqué

Compte tenu des autres anomalies déjà observées sur certaines surfaces de la Story 5.10, vérifier si plusieurs objets occupent la même zone.

**Ne pas considérer pour autant ANO-5.10-01 et ANO-5.10-03 comme ayant nécessairement la même cause.**

La relation doit être vérifiée.

---

## Hypothèse D — Instabilité physique du véhicule

La géométrie pourrait être valide mais une collision mineure pourrait générer une réaction disproportionnée en raison :

- du Rigidbody ;
- des colliders du véhicule ;
- de son centre de masse ;
- du contrôle de vitesse ;
- ou de la manière dont le contrôleur applique le mouvement.

Cette piste doit principalement être étudiée si l'environnement ne révèle pas de collision anormale.

---

# Contraintes de correction

Ne pas résoudre l'anomalie en :

- réduisant arbitrairement la vitesse de toutes les IA ;
- augmentant artificiellement leur masse ;
- bloquant leur rotation ;
- téléportant les véhicules après l'impact ;
- ignorant globalement les collisions ;
- modifiant les Physics Layers sans analyse de leur rôle ;
- déplaçant arbitrairement la trajectoire IA pour contourner la zone.

Ces modifications pourraient masquer le symptôme sans supprimer sa cause.

La cause physique ou géométrique doit être identifiée avant la correction.

---

# Critères d'acceptation

## AC1 — Traversée stable des zones concernées

**Given** un véhicule IA approche d'un rond-point devant un tunnel  
**When** il traverse la zone précédemment concernée  
**Then** le véhicule reste en contact normal avec la chaussée  
**And** il ne subit aucune propulsion, rotation ou élévation anormale.

---

## AC2 — Validation répétée

**Given** la correction est appliquée  
**When** plusieurs véhicules traversent successivement chaque rond-point situé devant un tunnel  
**Then** aucune projection ou collision invisible n'est reproduite sur une durée de test représentative.

Le test doit couvrir plusieurs passages et pas seulement un véhicule unique.

---

## AC3 — Différentes trajectoires

**Given** plusieurs trajectoires valides traversent le rond-point  
**When** les véhicules empruntent les différentes entrées et sorties disponibles  
**Then** toutes les trajectoires restent physiquement praticables.

---

## AC4 — Cause racine identifiée

**Given** l'anomalie a été reproduite  
**When** l'investigation est terminée  
**Then** l'objet, la géométrie, le collider ou le mécanisme responsable est identifié  
**And** la cause de l'impulsion physique est documentée.

---

## AC5 — Pas de workaround physique global

**Given** le bug est corrigé  
**Then** la stabilité des véhicules ne dépend pas d'un contournement global destiné uniquement à empêcher leur envol  
**And** les paramètres de conduite/physique généraux ne sont modifiés que si leur rôle dans la cause racine est démontré.

---

## AC6 — Non-régression Story 5.10

**Given** la correction est appliquée  
**When** la Story 5.10 est retestée  
**Then** :

- les portals continuent à fonctionner ;
- les véhicules peuvent entrer et sortir du district ;
- les différents itinéraires restent accessibles ;
- le lane graph reste fonctionnel ;
- les véhicules restent capables de traverser les ronds-points normalement ;
- le système de spawn/despawn source/sink reste fonctionnel.

---

# Corrélation avec les autres anomalies 5.10

Les anomalies suivantes concernent également la nouvelle géométrie/navigation de la Story 5.10 :

- `ANO-5.10-01` — clignotement/superposition de certaines surfaces ;
- `ANO-5.10-02` — véhicules pouvant tourner en boucle autour de certaines zones ;
- `ANO-5.10-03` — véhicules projetés physiquement près des ronds-points/tunnels.

Pendant l'investigation, vérifier si certaines de ces anomalies possèdent une cause commune.

**Ne pas les fusionner ni supposer une cause commune avant investigation.**

---

# Instruction BMAD

Traiter cette anomalie comme une investigation de physique/géométrie avant correction.

La vidéo confirme qu'un véhicule subit une réaction physique anormale, mais elle ne permet pas à elle seule d'identifier ce qu'il percute.

Commencer par reproduire le problème et inspecter les colliders/géométries réellement présents dans la zone.

Lorsque le problème se produit, identifier si possible l'objet et le collider impliqués dans la collision ainsi que le contact physique généré.

Comparer ensuite plusieurs passages afin de déterminer si le problème dépend :

- de la zone ;
- du véhicule ;
- de la trajectoire ;
- de la vitesse ;
- ou d'une combinaison de ces facteurs.

Ne modifier les paramètres généraux de physique ou de conduite qu'après avoir démontré qu'ils participent à la cause racine.

Après correction, documenter :

1. la cause racine ;
2. l'objet / collider / système impliqué ;
3. les fichiers, prefabs ou scènes modifiés ;
4. la correction appliquée ;
5. les tests de reproduction effectués ;
6. la vérification des autres ronds-points/tunnels ;
7. toute relation éventuellement identifiée avec ANO-5.10-01 ou ANO-5.10-02.