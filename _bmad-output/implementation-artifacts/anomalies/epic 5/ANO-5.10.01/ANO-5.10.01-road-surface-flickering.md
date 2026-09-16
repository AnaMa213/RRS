# ANO-5.10-01 — Clignotement de certaines surfaces du réseau routier en Play Mode

## Contexte

- **Epic :** 5 — NPC Response Foundation and Future Traffic
- **Story concernée :** 5.10 — Lane Graph, Greybox District, and Routed Traffic
- **Type :** Anomalie visuelle / géométrie de scène
- **Sévérité initiale :** Majeure visuelle
- **Impact fonctionnel :** À déterminer pendant l'analyse
- **Reproductibilité :** Observée lors du lancement d'une partie
- **Scène concernée :** `MVP_Run`

La Story 5.10 a introduit le nouveau district greybox et son réseau routier.

Pendant les tests de recette, plusieurs portions du réseau routier présentent un comportement visuel anormal.

---

## Comportement observé

Lorsque la partie est lancée, certaines surfaces de la chaussée clignotent très rapidement.

Le phénomène donne l'impression que plusieurs surfaces ou géométries différentes occupent le même emplacement et que leur affichage alterne rapidement.

Les zones concernées semblent principalement être situées autour de certaines jonctions/intersections du réseau routier.

Une capture fournie avec cette anomalie indique plusieurs zones particulièrement visibles en rouge.

Une vidéo de reproduction montre également le phénomène pendant l'exécution de la scène.

---

## Comportement attendu

Les surfaces constituant les routes et intersections doivent rester visuellement stables pendant toute l'exécution de la partie.

Il ne doit pas y avoir :

- de clignotement rapide de portions de chaussée ;
- d'alternance visible entre plusieurs surfaces ;
- de disparition/réapparition de géométrie ;
- de superposition visuellement instable de plusieurs éléments du réseau routier.

La correction ne doit pas dégrader le fonctionnement du lane graph, des intersections, du routing ou du système de trafic introduit par la Story 5.10.

---

## Étapes de reproduction

1. Ouvrir/lancer `MVP_Run`.
2. Démarrer une partie utilisant le district greybox de la Story 5.10.
3. Observer les routes et intersections, notamment les zones identifiées sur la capture fournie.
4. Laisser la simulation tourner.
5. Observer que certaines portions de chaussée clignotent rapidement.

---

## Éléments fournis

- Capture Unity indiquant en rouge plusieurs zones où le défaut est observé.
- Vidéo montrant le problème en Play Mode.

Ces éléments doivent être utilisés pendant l'investigation pour localiser les GameObjects et surfaces réellement impliqués.

---

## Hypothèse initiale — NON CONFIRMÉE

Le comportement visuel observé peut être compatible avec un problème de **z-fighting**, par exemple si deux surfaces sont coplanaires ou presque coplanaires.

Cependant, cette hypothèse ne doit pas être considérée comme le diagnostic.

D'autres causes sont possibles, notamment :

- GameObjects ou prefabs instanciés plusieurs fois ;
- coexistence d'une géométrie authored et d'une géométrie créée au runtime ;
- superposition entre meshes de route et meshes d'intersection ;
- duplication liée au système de génération/initialisation du district ;
- autre problème de rendu ou de lifecycle.

---

## Investigation demandée

Avant toute correction :

1. Reproduire l'anomalie.
2. Identifier précisément les `GameObject`, `MeshRenderer`, meshes ou prefabs correspondant aux surfaces qui clignotent.
3. Déterminer si plusieurs éléments occupent effectivement la même zone.
4. Identifier leur provenance :
   - objets déjà présents dans la scène ;
   - prefabs ;
   - génération runtime ;
   - initialisation réseau ;
   - autre mécanisme.
5. Déterminer la cause racine du clignotement.

Vérifier également si les éventuels objets superposés contiennent autre chose que du rendu :

- `Collider` ;
- composants liés au lane graph ;
- navigation ;
- triggers ;
- composants réseau ;
- ou toute autre logique pouvant influencer les véhicules.

L'objectif est de déterminer si l'anomalie est exclusivement visuelle ou si elle peut également provoquer des problèmes fonctionnels dans le système de trafic.

---

## Contraintes de correction

Ne pas appliquer un simple contournement graphique sans avoir identifié la cause racine.

Ne pas :

- masquer arbitrairement une des surfaces ;
- déplacer une surface uniquement pour supprimer visuellement le clignotement sans comprendre pourquoi elle est présente ;
- supprimer un objet pouvant appartenir au lane graph ou au système de trafic sans vérifier ses responsabilités ;
- modifier l'architecture du réseau routier sans nécessité démontrée.

La correction doit conserver le comportement attendu de la Story 5.10.

---

## Critères d'acceptation de l'anomalie

### AC1 — Stabilité visuelle

**Given** le district de la Story 5.10 est chargé  
**When** une partie est lancée dans `MVP_Run`  
**Then** les surfaces des routes et intersections restent visuellement stables  
**And** aucune des zones précédemment affectées ne présente de clignotement ou d'alternance de surfaces.

### AC2 — Cause racine identifiée

**Given** l'anomalie a été reproduite  
**When** l'investigation est terminée  
**Then** les objets ou systèmes responsables sont identifiés  
**And** la cause racine est documentée dans le compte-rendu de correction.

### AC3 — Absence de duplication fonctionnelle

**Given** les surfaces responsables ont été identifiées  
**When** leurs composants sont inspectés  
**Then** il est vérifié que la correction ne laisse pas de `Collider`, trigger, élément de navigation, composant réseau ou élément du lane graph dupliqué pouvant influencer les véhicules.

### AC4 — Non-régression Story 5.10

**Given** la correction est appliquée  
**When** les tests de la Story 5.10 sont rejoués  
**Then** le lane graph reste fonctionnel  
**And** les routes/intersections restent utilisables  
**And** le routing des véhicules reste fonctionnel  
**And** le trafic source/sink continue de fonctionner conformément à la Story 5.10.

---

## Instruction BMAD

Traiter cette anomalie comme une investigation de cause racine avant correction.

Ne pas considérer l'hypothèse de z-fighting comme une conclusion.

Utiliser le repository, la scène Unity, les prefabs concernés, les outils disponibles et les preuves fournies pour déterminer la cause réelle.

Une fois la cause identifiée, appliquer la correction minimale cohérente avec l'architecture existante, puis exécuter les tests pertinents et documenter :

1. la cause racine ;
2. les fichiers/assets/scènes modifiés ;
3. la correction appliquée ;
4. les tests réalisés ;
5. les éventuels risques ou éléments restant à surveiller.