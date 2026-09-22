---
id: ANO-5.10-03
title: AI vehicles do not react naturally to collisions and keep forcing their normal route
status: open
epic: 5
story: null
original_story: 5.10
previous_story: 5.14
reassigned: 2026-09-21
type: bug
category: ai-traffic-physics
severity: major
priority: high
---

# ANO-5.10-03 — Les véhicules IA continuent à forcer leur trajet après une collision

## Contexte

> **Réaffectée le 2026-09-21** (`planning-artifacts/sprint-change-proposal-2026-09-21.md`). La Story 5.14 a livré uniquement la séparation intention/physique ; son spec approuvé a explicitement retiré la réponse aux collisions et le retour physique vers une voie. L'anomalie reste `open`, sans story exécutable tant que les contrats Traffic V2 de réponse aux collisions et de recovery ne sont pas conçus. Les AC1–AC8 deviennent des scénarios obligatoires de l'oracle V1 et du futur gate de parité V2. Cette note supplante l'affectation 5.14 ci-dessous sans effacer son historique.

> **Réaffectée le 2026-09-18** (`planning-artifacts/sprint-change-proposal-2026-09-18.md`). Cette anomalie passe de la Story 5.10 à la **Story 5.14 — AI Drives by Intent**, dont elle constitue le cœur. Motif : sa correction est impossible sans le modèle physique à roues livré par les Stories 5.11 à 5.13 — la réaction « mordre sur un trottoir » demandée en Réaction B suppose une bordure franchissable, et les projections observées viennent du conflit entre une écriture directe de `linearVelocity` et la résolution PhysX. Ses huit critères d'acceptation restent **autoritatifs et inchangés** ; la Story 5.14 les référence sans les recopier. Statut inchangé : `open`. La Story 5.10 passe en `review`.

> **Complément de recette du 2026-09-18 — cas de la POUSSÉE, à vérifier explicitement.** La recette signale que les véhicules IA ne peuvent pas être _poussés_ : un joueur qui les percute ou les pousse ne peut ni les dévier, ni les mettre en travers, ni les sortir de la chaussée, et le véhicule repart sur sa ligne au pas de physique suivant. Le mécanisme est nommé — `NetworkedAIVehicleDriverController.ApplyMovement`, appelée à chaque `FixedUpdate` : `:846` `body.MoveRotation(rotation)` impose le lacet depuis le cap de la route, `:849` `body.linearVelocity = (forward * longitudinalSpeed * intent.Throttle) + verticalVelocity` remplace le vecteur vitesse ; second mécanisme du même genre, `RecoverAtWaypoint` (`:861`) téléporte sur le nœud de voie. **Les critères ci-dessous sont écrits autour d'une COLLISION ; la poussée continue sans choc n'y figure pas explicitement.** À exiger en plus à la livraison : une IA poussée doit être déviée et pouvoir quitter la chaussée, ne doit pas se ré-aligner en une frame, et doit revenir sur une voie valide physiquement (AC6). Entrée correspondante au registre : `implementation-artifacts/deferred-work.md`, « Les vehicules IA ne peuvent pas etre POUSSES ».

- **Epic :** 5 — Vehicle Physics, NPC Response Foundation and Routed Traffic
- **Story propriétaire :** aucune — frontière Traffic V2 collision response/recovery à décomposer après le gate d'architecture
- **Type :** Anomalie fonctionnelle / physique véhicule / réaction IA
- **Sévérité :** Majeure
- **Priorité :** Haute
- **Scène concernée :** `MVP_Run`

Cette anomalie avait initialement été identifiée parce que certains véhicules pouvaient être violemment projetés ou retournés, notamment à proximité des ronds-points devant les tunnels.

L'investigation manuelle montre cependant que le problème est plus général.

Le comportement problématique semble provenir principalement de la manière dont l'IA continue à appliquer son comportement normal de conduite après une collision ou une forte perturbation physique.

---

# Problème identifié

Lorsqu'un véhicule IA subit une collision importante, est poussé hors de sa trajectoire ou se fait percuter par un autre véhicule, il continue actuellement à essayer de suivre immédiatement son chemin normal.

Il cherche donc à retrouver sa cible de navigation et à continuer sa route alors que :

- son orientation peut avoir été fortement modifiée ;
- sa vitesse peut ne plus correspondre à la situation ;
- il peut être déplacé latéralement ;
- il peut être en train de basculer ;
- un autre véhicule peut encore être en contact avec lui ;
- il peut se trouver temporairement hors de la chaussée.

Cela peut créer un conflit entre :

1. la réaction physique naturelle du `Rigidbody` à la collision ;
2. les commandes du contrôleur IA qui continuent à vouloir faire avancer le véhicule vers son trajet normal.

Dans certaines situations, cela peut contribuer à produire des réactions physiques très violentes, notamment :

- propulsion anormale ;
- véhicule projeté ;
- retournement ;
- comportement erratique après un choc ;
- véhicule continuant à pousser alors qu'il vient d'entrer en collision.

---

# Comportement observé

Lorsqu'un véhicule IA est percuté ou rencontre une collision importante :

1. la physique déplace ou fait pivoter le véhicule ;
2. le système de conduite continue immédiatement à chercher à suivre la route prévue ;
3. le véhicule continue souvent à appliquer une intention de mouvement vers sa trajectoire ;
4. la réaction produite peut devenir artificielle ou physiquement instable.

Le véhicule donne alors davantage l'impression d'un agent de navigation essayant à tout prix de rejoindre son chemin que d'un conducteur venant de subir un accident.

---

# Comportement attendu

Lorsqu'un véhicule subit une collision significative, sa priorité immédiate ne doit plus être de suivre parfaitement son trajet normal.

Il doit temporairement entrer dans un comportement de **réaction à la collision / perte de contrôle**.

Pendant cette phase :

- le suivi normal de route doit être temporairement suspendu ou fortement réduit ;
- le véhicule doit laisser la physique du choc se résoudre ;
- l'IA doit produire une réaction crédible à l'événement ;
- elle ne doit pas lutter immédiatement contre le `Rigidbody` pour rejoindre sa trajectoire.

Une fois la situation stabilisée, le véhicule doit pouvoir analyser sa position et retrouver progressivement une route valide.

---

# Réactions attendues à une collision

Les réactions ne doivent pas être parfaitement identiques pour tous les véhicules.

Lors d'une collision significative, un conducteur peut par exemple :

## Réaction A — Freinage d'urgence

Réaction la plus courante.

Le conducteur :

- pile ;
- réduit brutalement son accélération ;
- tente de stabiliser le véhicule ;
- peut s'arrêter complètement.

Il attend ensuite que la situation immédiate soit suffisamment stable avant de repartir.

---

## Réaction B — Esquive

Réaction plus rare.

Le conducteur tente d'éviter la collision ou de sortir de la situation.

Cela peut l'amener temporairement :

- à dévier fortement de sa trajectoire ;
- à quitter partiellement la chaussée ;
- à mordre sur un trottoir ou une zone normalement non utilisée par le trafic.

Cette réaction doit rester physiquement plausible.

Elle ne signifie pas que le véhicule doit systématiquement éviter tous les obstacles.

---

## Réaction C — Mauvaise réaction / perte de contrôle

Certains conducteurs peuvent mal réagir au choc.

Par exemple :

- continuer momentanément tout droit ;
- ne pas freiner suffisamment ;
- finir contre un mur ;
- heurter un autre obstacle ;
- se retrouver mal orientés ou hors de la chaussée.

Le système ne doit pas rendre tous les conducteurs artificiellement parfaits.

Une collision doit pouvoir produire une situation chaotique crédible.

---

# Variabilité des réactions

Les différentes réactions ne doivent pas nécessairement être équiprobables.

Exemple de tendance souhaitée :

- freinage / arrêt : fréquent ;
- esquive : plus rare ;
- mauvaise réaction / perte de contrôle : possible.

Les probabilités ou tendances exactes ne doivent pas être hardcodées si le système possède déjà une architecture permettant de les rendre configurables.

BMAD doit déterminer la manière la plus cohérente d'intégrer cette variabilité au modèle de conducteur existant.

---

# Détection d'une collision significative

Toutes les petites collisions ne doivent pas nécessairement interrompre la navigation.

Le système doit distinguer autant que possible :

- contact léger ;
- petit frottement ;
- collision réelle ;
- impact suffisamment important pour déclencher une réaction.

La manière de déterminer ce seuil doit être analysée pendant l'implémentation.

Elle peut notamment dépendre de données physiques telles que :

- vitesse relative ;
- impulsion de collision ;
- changement brutal de vitesse ;
- rotation générée ;
- direction de l'impact.

Ne pas introduire un seuil arbitraire sans vérifier son comportement en jeu.

---

# État Collision / Recovery

Le système devrait conceptuellement distinguer au minimum deux phases.

## Phase 1 — Collision Response

Immédiatement après un impact significatif :

- la conduite normale n'est plus prioritaire ;
- l'IA réagit au choc ;
- le contrôleur ne doit pas forcer immédiatement le véhicule à reprendre son ancienne trajectoire ;
- la physique doit pouvoir se résoudre correctement.

## Phase 2 — Recovery

Lorsque :

- les collisions immédiates ont cessé ;
- la vitesse et la rotation du véhicule sont redevenues suffisamment stables ;
- la situation autour du véhicule permet de recommencer à conduire ;

l'IA peut chercher à retrouver le réseau routier.

Elle doit alors :

1. déterminer où elle se trouve ;
2. identifier une manière valide de rejoindre une lane ;
3. se réaligner progressivement ;
4. reprendre son itinéraire ou recalculer une route valide.

Le véhicule ne doit pas nécessairement chercher à revenir exactement au point où il a quitté sa trajectoire.

---

# Retour sur la route

Un véhicule déplacé hors de son parcours doit pouvoir revenir naturellement dans le trafic.

Le système de récupération ne doit pas utiliser :

- téléportation ;
- repositionnement instantané ;
- despawn/re-spawn ;
- snap brutal sur une lane.

Le véhicule doit physiquement rejoindre une portion valide du réseau.

Si nécessaire, son itinéraire peut être recalculé depuis la lane valide qu'il réussit à rejoindre.

---

# Relation avec le modèle de conduite

Cette réaction à la collision doit être compatible avec le modèle de conducteur introduit par la Story 5.9.

À terme, les paramètres du conducteur, Rage/Fear ou certains archétypes pourront éventuellement influencer sa réaction.

Par exemple :

- conducteur prudent → freinage plus probable ;
- conducteur agressif → freinage tardif ou poursuite du mouvement ;
- conducteur paniqué → esquive plus forte.

Cependant, l'objectif de cette anomalie n'est pas de construire maintenant toute la personnalisation émotionnelle future.

La priorité est d'établir un système de réaction aux collisions générique et crédible sur lequel les futures stories pourront s'appuyer.

---

# Investigation technique demandée

Avant correction, analyser précisément le fonctionnement actuel du contrôleur lors d'une collision.

Vérifier notamment :

- comment l'accélération est appliquée au `Rigidbody` ;
- si la vitesse est directement imposée ;
- si le steering continue à fonctionner pendant une collision ;
- si le véhicule continue à poursuivre sa target alors qu'il est en perte de contrôle ;
- si les forces/velocities calculées par la physique sont écrasées ou combattues par le contrôleur ;
- comment est déterminée la prochaine cible après déplacement du véhicule ;
- si le contrôleur possède déjà une notion de perte de contrôle ou recovery.

Identifier la cause exacte des projections physiques précédemment observées et déterminer dans quelle mesure elles proviennent du conflit entre conduite normale et physique.

---

# Contraintes

Ne pas résoudre le problème en :

- augmentant simplement la masse du véhicule ;
- bloquant artificiellement ses rotations ;
- réduisant globalement la vitesse des IA ;
- désactivant les collisions ;
- téléportant les véhicules après un choc ;
- forçant immédiatement le véhicule sur la lane la plus proche ;
- empêchant les véhicules de quitter physiquement la route.

Ces solutions masqueraient le problème au lieu de construire un comportement de collision exploitable pour le gameplay futur.

---

# Critères d'acceptation

## AC1 — Interruption du comportement normal après un choc

**Given** un véhicule IA suit normalement son itinéraire  
**When** il subit une collision significative  
**Then** il ne continue pas immédiatement à forcer son suivi normal de route  
**And** une phase temporaire de réaction à la collision prend la priorité.

---

## AC2 — Réaction physique stable

**Given** un véhicule vient d'être percuté  
**When** la collision est résolue par la physique  
**Then** le contrôleur IA ne génère pas de forces ou commandes conduisant à une projection physique artificielle  
**And** le mouvement résultant reste cohérent avec le choc subi.

---

## AC3 — Variabilité des réactions

**Given** plusieurs véhicules subissent des collisions significatives dans des conditions comparables  
**When** leur réaction est déterminée  
**Then** ils peuvent produire plusieurs comportements crédibles  
**Including** freinage/arrêt comme réaction courante  
**And** esquive comme réaction moins fréquente  
**And** mauvaise réaction ou poursuite incontrôlée comme possibilité.

Les distributions exactes doivent rester configurables si cela correspond à l'architecture existante.

---

## AC4 — Pas de reprise prématurée

**Given** un véhicule est encore :

- en contact important avec un autre objet ;
- en rotation importante ;
- déplacé hors de sa trajectoire ;
- ou physiquement instable ;

**When** son contrôleur est mis à jour  
**Then** il ne doit pas considérer que la situation est déjà terminée et reprendre immédiatement son comportement nominal.

---

## AC5 — Recovery

**Given** la collision est terminée et le véhicule est suffisamment stable  
**When** une route valide peut être retrouvée  
**Then** le véhicule passe progressivement en mode récupération  
**And** cherche un moyen physiquement crédible de rejoindre le réseau routier.

---

## AC6 — Retour au trafic

**Given** un véhicule a été déplacé hors de sa route par une collision  
**When** il termine sa phase de récupération  
**Then** il rejoint une lane valide sans téléportation  
**And** reprend ensuite un itinéraire valide vers sa destination.

---

## AC7 — Possibilité de quitter temporairement la route

**Given** une collision ou une réaction d'esquive déplace le véhicule hors de la chaussée  
**When** cette trajectoire est physiquement possible  
**Then** le système ne doit pas artificiellement empêcher le véhicule de quitter temporairement la route  
**And** le recovery doit ensuite être capable de le ramener vers une zone de circulation valide.

---

## AC8 — Non-régression conduite normale

**Given** aucune collision significative ne se produit  
**When** les véhicules circulent normalement  
**Then** le comportement de route/routing de la Story 5.10 continue de fonctionner comme auparavant.

---

# Relation avec Traffic V2

Cette correction doit être conçue avec les contrats Traffic V2 de perception, décision tactique, planification de mouvement, SafetyFilter, politique conducteur et Recovery Supervisor. Les anciennes Stories 5.17–5.23 sont superseded et ne sont plus des propriétaires exécutables.

En particulier, la future Rage/Fear doit pouvoir moduler la réaction à une collision sans nécessiter de remplacer complètement ce système.

---

# Instruction BMAD

Cette anomalie n'est plus à traiter principalement comme un problème de géométrie du rond-point.

Les observations précédentes autour des tunnels ont permis de révéler un problème plus général dans la gestion physique des véhicules après une collision.

Analyser en priorité l'interaction entre :

- `Rigidbody` ;
- système de steering ;
- modèle de conduite ;
- suivi du lane graph ;
- récupération après déplacement physique.

Identifier ce qui provoque actuellement la poursuite immédiate du trajet après un impact.

Proposer ensuite un comportement générique de :

**Normal Driving → Collision Response → Recovery → Normal Driving**

sans créer une architecture parallèle au modèle de conduite existant.

La réaction à une collision doit laisser temporairement la priorité à la situation physique plutôt qu'au suivi strict de l'itinéraire.

Une fois la situation stabilisée, l'IA doit être capable de retrouver naturellement une route valide et de reprendre sa circulation.

Avant implémentation, séparer explicitement la réponse immédiate au choc, la stabilisation physique et la récupération de progression. Safety, collision response et recovery peuvent devenir des stories exécutables distinctes après le gate d'architecture ; aucune ne doit dupliquer un second système de conduite.
