# Story 5.15 — collisions et dégâts crédibles

## Banc reproductible

- Machine : poste de développement RRS ; Unity 6 connecté ; PhysX 3D.
- Géométrie : clones de `Greybox_PlayerCar.prefab` (masse 1200 kg, BoxCollider 2,06 × 1,42 × 4,44 m, profil `VehicleProfileDef_Default`) contre un mur Cube de 0,20 m.
- Pas : 0,02 s, 80 simulations dans une `PhysicsScene` locale ; gravité désactivée seulement pour isoler le choc horizontal.
- Vitesses : 6 m/s puis 18 m/s (vitesse avant maximale du profil), 30 corps à 18 m/s pour la comparaison de coût.

## Décision

Les deux modes ne tunnellent pas dans le domaine autorisé. `Discrete` est donc retenu : c'est le mode PhysX le moins coûteux quand le critère tunnel est à égalité. Le banc journalise médiane et p95 par pas ; la CI conserve les chiffres bruts dans ses logs.

Le choc rapide reste fini, sans pénétration du mur, avec une excursion verticale < 0,4 m et un pic vertical < 4 m/s. Aucun solver, clamp, impulsion corrective ni téléportation n'est ajouté.

Les seuils issus des paliers du banc sont 4 m/s (5 dégâts) et 12 m/s (15 dégâts). Le mapping central `[5, 15]`, l'autorité host, le filtre surface et l'immunité dev restent inchangés. La garde existante `vehicleDamage > 0` continue d'empêcher tout dégât occupant si le véhicule ne perd rien.

## Recette MVP_Run

Lancer un host puis un client dans `MVP_Run`, percuter une voiture et un décor à faible puis forte vitesse. Vérifier sur le host le mouvement et les dégâts cohérents ; côté client, vérifier que le mouvement est seulement répliqué par `NetworkTransform`, sans simulation ni mutation de dégâts partagée.
