---
title: "Credible Collisions and Damage Integration"
type: "feature"
created: "2026-09-20"
status: "done"
review_loop_iteration: 0
baseline_commit: "1f65c1ead8904a3f1e753f9920d3b5ca4e821aa1"
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La couche à roues n'a aucune preuve reproductible qu'un choc véhicule-véhicule ou véhicule-décor reste proportionnel, sans projection, rotation extrême ni pénétration durable. Les seuils de dégâts `3/14 m/s` et le mode de détection effectif `ContinuousDynamic` n'ont jamais été produits par une mesure du modèle actuel.

**Approach:** Mesurer d'abord PhysX sans ajouter de solveur maison, avec un banc PlayMode commun aux chocs, au tunnelling et au coût à 30 véhicules. Conserver la réponse native si elle passe ; sinon régler uniquement la couche physique partagée, puis recalibrer le mapping de dégâts central et documenter les chiffres.

## Boundaries & Constraints

**Always:** Le host reste seul simulateur et seul auteur des dégâts ; les clients restent cinématiques et lisent `NetworkTransform`. Préserver `IsSurfaceOnlyCollision`, l'immunité dev, le mapping partagé et la garde `vehicleDamage > 0` avant tout dégât occupant. Toute décision de mode/seuil cite la mesure brute qui l'a produite.

**Ask First:** Si les deux modes tunnellent dans le domaine authoré, ou si satisfaire le banc exige un clamp de vitesse/rotation, une impulsion corrective, une nouvelle RPC ou un changement du signal `relativeVelocity.magnitude`, arrêter et demander l'arbitrage humain.

**Never:** Aucun solver de collision maison spéculatif, `WheelCollider`, écriture de pose/vitesse de conduite, récupération/téléportation comptée comme succès, instrumentation permanente ou nouvelle dépendance. Ne pas modifier les prefabs Synty.

## I/O & Edge-Case Matrix

| Scénario | Entrée / état | Sortie attendue | Gestion d'erreur |
|---|---|---|---|
| Mur / choc frontal ou décalé | 2 vitesses bornées, intent neutre puis gaz maintenus | grande vitesse produit plus de mouvement ; valeurs finies ; excursion verticale `< 0,4 m`, pic vertical `< 4 m/s`, séparation bornée | distinguer pénétration PhysX et conducteur qui pousse ; un échec reste rouge |
| Mode de détection | 30 corps, `Discrete` puis `ContinuousDynamic`, même scène et pas `0,02 s` | zéro tunnel ; médiane/p95 CPU consignés ; mode sans tunnel le moins coûteux retenu | tunnel élimine le mode ; deux modes en échec déclenchent Ask First |
| Dégâts | impact juste sous puis au-dessus du seuil mesuré | sous seuil : 0 dégât véhicule/occupant ; au-dessus : mapping `[5,15]` borné sur host | client, contact de surface ou véhicule dev : aucune mutation |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` -- `ApplyProfile:135-199` impose aujourd'hui `ContinuousDynamic`; `FixedUpdate:293-536` laisse PhysX résoudre le choc ; aides d'assiette/lacet réutilisables `:598-712`. Ne corriger ici que si le banc prouve un défaut commun.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- `OnCollisionEnter:345-369`, seuils `:81-89`, mapping pur `ComputeCollisionDamage:548-568`; conserver autorité et filtres.
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- ponts principal/secondaire `:782-871`, garde occupants positive, `IsAuthoritativeForDamage:951-960`; lecture seule attendue.
- `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` et `Greybox_AIVehicle.prefab` -- YAML `Discrete`, mais runtime `ContinuousDynamic`; utiliser leurs masse/collider/profil dans le banc, sans créer une seconde source de vérité.
- `Assets/RoadRage/Tests/PlayMode/Story513ArcadeAssistsAndUnevenGroundPlayModeTests.cs` -- patron de banc par intent et bornes de non-projection à réutiliser.
- `Assets/RoadRage/Tests/EditMode/Story35VehicleDamageHookAndTeamWipeContractStubTests.cs` et `Story510LaneGraphAndRoutedTrafficTests.cs` -- preuves existantes du mapping et de la garde occupant, à garder vertes sans affaiblissement.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- intégration obligatoire ; trafic par `PortalTrafficSpawner`, défaut 8/max 30, donc le benchmark 30 vit dans une PhysicsScene locale dédiée.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Tests/PlayMode/Story515CredibleCollisionsAndDamageIntegrationPlayModeTests.cs` -- ajouter un unique harness en PhysicsScene locale : mur, deux véhicules, vitesses/offsets, 30 corps, comparaison tunnelling + médiane/p95 `Stopwatch`; aucune dépendance.
- [x] `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` et `Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset` -- appliquer seulement le mode gagnant et, si un banc collision échoue, régler les paramètres partagés existants au minimum ; aucune nouvelle abstraction.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- remplacer les seuils `3/14 m/s` par les valeurs issues du banc, sans dupliquer ni changer le mapping `[5,15]`.
- [x] `Assets/RoadRage/Tests/EditMode/Story515CredibleCollisionsAndDamageIntegrationTests.cs` -- figer mode effectif, seuils mesurés, autorité/filtres et garde occupants ; réutiliser les contrats 3.5/5.10 plutôt que les recopier.
- [x] `docs/setup/story-5-15-credible-collisions-and-damage-notes.md` -- consigner machine, Unity, géométrie, pas/solveur, essais, tunnels, CPU, impacts/dégâts, décision et recette `MVP_Run`.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- passer `5-15-credible-collisions-and-damage-integration` à `in-progress`.
- [x] `graphify update .` -- régénérer le graphe après les changements source en préservant les modifications Graphify initiales de l'utilisateur.

**Acceptance Criteria:**
- Given les bancs reproductibles, when les chocs décor/véhicule tournent aux vitesses authorées, then la réponse est finie, ordonnée avec l'impact, sans lancement, rotation extrême ni pénétration durable, et sans téléportation.
- Given 30 véhicules, when les deux modes sont mesurés à scénario identique, then le mode retenu suit la règle écrite et ses chiffres bruts sont archivés.
- Given le nouveau profil d'impact, when le seuil est franchi, then le mapping central recalibré s'applique uniquement sur host et aucun occupant ne perd de HP lorsque le véhicule n'en perd aucun.
- Given un client réseau, when le host résout un choc, then le client lit le mouvement répliqué et ne simule ni n'applique de dégât partagé.

## Spec Change Log

## Design Notes

La réponse native est le choix par défaut : le code actuel n'ajoute aucune impulsion de collision et la Story 5.14 n'efface plus les chocs IA. Le mode runtime est déjà `ContinuousDynamic` malgré les YAML `Discrete`; le benchmark compare donc l'état effectif au candidat, puis garde une seule décision dans `VehiclePhysicsBody`.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story515CredibleCollisionsAndDamageIntegrationTests"` -- fixture ciblée verte.
- `.\scripts\validate.ps1 -TestMode EditMode` -- suite complète verte, notamment 3.5, 5.10 à 5.14.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- suite complète verte ; logs Story 5.15 contiennent mesures brutes et décision.

**Manual checks (if no CLI):**
- Dans `MVP_Run`, host puis client : choc voiture-voiture et décor lisibles, aucune projection/rotation/pénétration durable, dégâts cohérents et état client répliqué.
