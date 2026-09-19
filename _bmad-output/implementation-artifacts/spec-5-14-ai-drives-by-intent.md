---
title: "AI Drives by Intent"
type: "feature"
created: "2026-09-19"
status: "in-progress"
review_loop_iteration: 0
baseline_commit: "06369e3f8ce36c6b3af1d1f1b68e66881e403626"
context:
  - "{project-root}/docs/setup/devworkflow-rollout.md"
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** `NetworkedAIVehicleDriverController.ApplyMovement` (`:892-917`) impose le mouvement de l'IA en écrivant directement le `Rigidbody` à chaque `FixedUpdate` : `body.MoveRotation(rotation)` fixe le lacet sur le cap de la route, et `body.linearVelocity = (forward * longitudinalSpeed * intent.Throttle) + verticalVelocity` **remplace** le vecteur vitesse — une grandeur que le contrôleur **entretient lui-même en boucle ouverte** (`IntegrateLongitudinalSpeed:606-635`, champs `currentSpeed:137` et `appliedAcceleration:138`). Trois conséquences sont mesurées, pas supposées. (1) Une IA **ne peut pas être poussée** : ce que la physique a gagné pendant le contact est effacé au pas de physique suivant et le véhicule repart sur sa ligne (`deferred-work.md:251-252`, vérification due à la 5.12 puis à la 5.14, faite en 5.12 et confirmée non corrigée par `:275-276`). (2) Sur le relief de recette de la Story 5.13, une IA **décolle et poursuit son tracé en l'air** : la composante verticale est relue **puis réécrite telle quelle**, donc une vitesse verticale donnée par un contact n'est jamais consommée — `m_UseGravity: 1` est bien actif sur les deux prefabs, c'est l'écriture qui neutralise la gravité (`:307-308`). (3) Les **aides arcade et la réduction d'autorité de la Story 5.13 n'ont aucun effet sur une caisse d'IA**, faute de chemin d'intent (`:299-300`). Ces trois symptômes — un longitudinal, un vertical, un de couple — ont une seule cause : l'IA écrit son mouvement au lieu de le produire.

**Approach:** L'IA cesse de conduire en écrivant. Elle émet un `VehicleDriveIntent` (direction, accélérateur, frein, frein à main) et le soumet à la **même** couche physique que le joueur (`VehiclePhysicsBody.ApplyDriveIntent:237`), et sa vitesse cesse d'être une entrée entretenue par le contrôleur pour devenir une **lecture** du `Rigidbody`. Le suivi de route, le tirage de virage pondéré, la mémoire auto-évitante, la détection de leader et le point de visée des Stories 5.10/5.12/5.13 restent **inchangés** : seule change la manière de traduire une décision déjà prise en mouvement du véhicule.

**Dettes reprises (demande humaine du 2026-09-19) — trois entrées fermées, une consignée, une écartée :**

1. `deferred-work.md:251-252` et `:275-276` — **les IA ne peuvent pas être poussées.** Fermée par le mécanisme : la poussée n'est plus effacée au pas suivant, le véhicule est dévié, peut quitter la chaussée et ne se réaligne pas en une frame. Mesurée à la recette.
2. `deferred-work.md:307-308` — **une IA qui franchit le relief de recette décolle**, symptôme vertical de la même cause. Fermée : la gravité redevient effective sur une caisse d'IA.
3. `deferred-work.md:299-300` — **les aides arcade n'ont aucun effet sur l'IA.** Fermée par construction, et **c'est cette story qui est observée** sur l'effet des aides sur le trafic, pas la 5.13.
4. **Harnais PlayMode rouge (3/33) et non filtrable** — prérequis **dur** déclaré par l'epic comme du suivi d'outillage. La ligne est **absente** de `docs/setup/devworkflow-rollout.md` (vérifié le 2026-09-19) : elle est créée par cette story.
5. **Écarté de cette story par découpe du 2026-09-19 :** la **réaction à une collision significative et la récupération physique** vers une voie valide (`ANO-5.10-03` AC1 à AC8, AC3 de l'epic). Une entrée de registre dédiée a été appendue le même jour, avec la raison de l'ordre : la réaction au choc n'a rien à suspendre ni à reprendre tant que la conduite écrase la physique. Les **surfaces roulantes** (`:240`) restent, elles, reportées avec leur condition de réouverture — cette story est la première à laisser un véhicule quitter la chaussée, elle ne lève pas le report.

## Boundaries & Constraints

**Always:**

- **Une seule couche physique** (AD-35). Le contrôleur IA n'écrit plus jamais `linearVelocity`, la position ni la rotation du `Rigidbody`. La seule écriture de vitesse qui reste dans le dépôt est la récupération joueur 3.4 (`NetworkedVehicleDriverController.cs:295`).
- **Cette story ne change aucune décision de conduite.** Elle change le chemin par lequel une décision existante atteint les roues. La décision longitudinale reste `DriverModel` : les appels gardés par `Story59…` (`ResolveEffectiveProfile`, `ComputeAcceleration`, `SmoothAcceleration`, `ShouldEvaluateLaneChange`, `ResolveNoisyDesiredSpeed`) restent présents dans le contrôleur IA, et `currentSpeed` devient une **lecture** du `Rigidbody` (projection planaire), jamais une intégration.
- **AD-33 : le chemin ne lit que le profil conducteur** — jamais la rage, la peur ou une disposition de conducteur au-delà de `ResolveBehavior`, qui reste tel quel.
- Le **point de visée et sa continuité** (5.12, 5.13) sont conservés à l'identique, y compris le rappel borné. La mémoire de visée (`previousAimPoint:180`, `hasAimPoint:181`) est purgée partout où le véhicule est **reposé** : `RecoverAtWaypoint` reste en place dans cette story, donc il doit purger la mémoire de visée comme il purge déjà la mémoire de parcours.
- **L'AC8 de l'anomalie est un absolu en non-régression** : sans perturbation, parcours, tirage de virage pondéré, portails et mémoire auto-évitante de la Story 5.10 se comportent comme avant, au comportement près de « la vitesse vient de la physique ».
- **NFR18** : ids de prefabs, `NetworkObject` enregistrés, composants gameplay, colliders et ids de définition inchangés. Les trois IA de `MVP_Run` sont **placées en scène** : aucun spawn runtime n'est introduit.

**Ask First:**

- Toute modification d'une **fixture de la Story 5.9** — l'epic les déclare vertes **sans modification**, et un échec y signale une fuite de la physique dans la couche de décision.
- Toute modification d'une **fixture de la Story 5.2**. **Une seule** est concernée : la garde de la composante verticale (`Does.Contain("+ verticalVelocity")`), qui protège exactement l'écriture que cette story supprime — elle doit être retirée **parce que** sa raison d'être disparaît, avec l'écart consigné au Spec Change Log. `DriverControllerHasNoSharedStaticStateBeyondItsPureFunctions:138` et `DriverControllerAppliesRecoveryWithoutRpcOrAdvancingWaypointIndex` restent **valides sans modification** : cette story n'ajoute aucune méthode `static` et ne touche pas `RecoverAtWaypoint`.
- Toute **nouvelle `NetworkVariable`**, RPC ou champ répliqué : aucune n'est nécessaire, et l'état de conduite doit rester **hôte seul** comme aujourd'hui (`NetworkedAIVehicleState` ne porte que `WaypointIndex` et `Behavior`).
- Toute modification de la **sémantique de `WaypointIndex`**, du contrat `NetworkedAIVehicleState`, d'un prefab de véhicule ou d'un collider.
- Toute modification de `RecoverAtWaypoint` **au-delà** de la purge de la mémoire de visée : sa suppression par téléportation appartient au travail écarté, et la toucher ici serait le faire à moitié.

**Never:**

- Masquer un symptôme : augmenter la masse, figer les rotations, réduire globalement la vitesse des IA, désactiver les collisions, téléporter, snapper sur la voie la plus proche, ou empêcher un véhicule de quitter la route. Ce sont les contraintes explicites de `ANO-5.10-03`, et ce sont exactement les solutions qui laissent la cause en place.
- Ajouter un **garde-fou vertical de compensation** dans la couche de conduite : la Story 5.13 l'a explicitement écarté, précisément parce que ce serait ce travail exécuté au mauvais endroit.
- Ajouter un **second chemin d'intent** : l'IA passe par `ApplyDriveIntent`, jamais par un chemin parallèle ou un jumeau local.
- Ajouter un `WheelCollider` (AD-35).
- Livrer tout ou partie de la **réaction au choc** ou de la **récupération physique** : périmètre écarté le 2026-09-19, entrée dédiée au registre.
- Inventer une **table de surfaces roulantes** : le report `deferred-work.md:240` garde sa condition de réouverture.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
| --- | --- | --- | --- |
| Conduite nominale | IA sur une voie, aucune perturbation, profil et graphe présents | le véhicule **avance réellement** : les efforts viennent des roues, la vitesse croît vers la vitesse désirée du profil, le parcours progresse | profil ou graphe absent : comportement d'arrêt authoré, inchangé |
| Vitesse lue, jamais entretenue | `FixedUpdate` successif sans aucun contact | la vitesse est **relue** du `Rigidbody` à chaque pas ; aucune grandeur de vitesse n'est conservée par le contrôleur entre deux pas | vitesse non finie : l'intent reste fini, le véhicule ne disparaît pas |
| Poussée sans choc | joueur poussant une IA lente ou arrêtée | le véhicule est **dévié** par le contact, peut être mis en travers, peut quitter la chaussée, et ne se réaligne **pas** en une frame | aucune force de rappel n'est ajoutée pour « tenir la file » : la poussée n'est pas compensée |
| Relief de recette | IA franchissant le dos-d'âne puis la marche basse de 0,12 m de `Avenue_CenterToEast` | le véhicule suit le relief **comme le joueur** : la gravité le ramène, il ne décolle pas, il ne poursuit pas son tracé en l'air | perte des quatre contacts : la reprise vient de la couche physique, jamais d'une écriture de rotation |
| Aides et autorité | IA avec une à trois roues au sol, ou les quatre en l'air | les aides de la 5.13 et la réduction d'autorité s'appliquent à l'IA **comme au joueur** — même couche, même profil | quatre roues en l'air : autorité nulle, conduite inerte, aucune exception levée |
| Choc subi | IA percutée par le joueur ou par un autre véhicule | la physique du choc se résout sans être **combattue** : la vitesse et le cap ne sont pas réimposés au pas suivant | **la réaction conduite après le choc n'est pas livrée ici** : elle appartient au périmètre écarté, et ce trou est connu, pas découvert |
| Repositionnement | le garde-fou hôte (vide, retournement soutenu, blocage prolongé) déclenche `RecoverAtWaypoint` | comportement inchangé — ce chemin n'est pas dans le périmètre — mais la **mémoire de visée est purgée** avec la mémoire de parcours | sans cette purge, le véhicule reposé viserait l'ancien point pendant un pas |
| Profil incomplet | profil conducteur ou profil physique absent, mal câblé | avertissement **unique**, véhicule inerte, aucune conduite | aucun repli codé en dur : `AMissingProfileLeavesTheVehicleInertWithASingleWarningAndNoHardCodedFallback` reste vrai |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- **le fichier de la story**. À **supprimer** : `ApplyMovement:892-917` (les deux écritures `body.MoveRotation` / `body.linearVelocity` et le read-modify-write de la composante verticale) et `IntegrateLongitudinalSpeed:606-635` (il écrit `appliedAcceleration:138` et `currentSpeed:137`). À **remplacer** : la lecture de la vitesse par `body.linearVelocity` projeté sur le plan, puis la construction d'un `VehicleDriveIntent` depuis la décision existante, puis `physicsBody.ApplyDriveIntent(intent, maxSpeed, steerRate, brakeTorque)`. À **conserver tel quel** : `FixedUpdate:249-407` dans sa structure (garde `!IsServer`, `ResolveBehavior:595`, garde-fou de vide, garde-fou de retournement `rolloverElapsedSeconds:135`, détection de leader `TryDetectLeader:685`, arrêt voulu `DriverModel.IsDeliberateStop`, détection de blocage `IsStuck`, franchissement de nœud `HasArrivedAtWaypoint`/`HasPassedUnreachableWaypoint`, `ResolveNextNode:409`, `ResetRouteMemoryAt:470`, `MarkNodeTraversed:504`, `ResolveRedirectToNearestExit:554`, `ResolveScanDirection:637`, `TickLaneChangeEvaluation:822`), ainsi que toute la visée `:357-369` (`previousAimPoint:180`, `hasAimPoint:181`). `ComputeSeekIntent:842-877` reste **pure et publique** : elle produit déjà un `VehicleDriveIntent`, c'est la brique dont le nouveau chemin se sert. À **ajouter** : la purge de `previousAimPoint`/`hasAimPoint` dans `RecoverAtWaypoint:923-956`, à côté de `ResetRouteMemoryAt`. **Aucune méthode `static` supplémentaire** (`Story52…:138` en compte 3 : `ComputeSeekIntent`, `HasArrivedAtWaypoint`, `IsStuck`).
- `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` -- la couche unique, **déjà prête, à ne pas modifier**. Point d'entrée : `ApplyDriveIntent(intent, maxForwardSpeed, steerRateDegreesPerSecond, brakeTorque):237-252` — l'intent est **stocké** et consommé au `FixedUpdate` suivant, donc l'ordre d'exécution des scripts n'a aucune importance. Leviers lus du profil et réduits par les dégâts 3.5 côté joueur : `MaxForwardSpeed`, `SteerRateDegreesPerSecond`, `BrakeTorque`. Utile à la recette : `GroundedWheelCount:93`, `GroundedAuthorityFactor` (`:196`, `:538`).
- `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs` -- le contrat à émettre : `readonly struct:14`, constructeur `(throttle, steer, brakeReverse, handbrake):18` avec **clamp au constructeur**, `IsIdle`, `Idle`. Les quatre axes existent déjà : la conversion n'en invente aucun.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- le **patron à copier sans le modifier**. `SubmitIntentToPhysicsLayer:662-680` : refus explicite et **averti une seule fois** si `physicsBody == null || !physicsBody.HasProfile`, puis soumission. `ResolveDriveAuthority:592-608` (statique **pure**, réductions de dégâts 3.5) — c'est l'autorité du **joueur** ; l'IA n'en a pas aujourd'hui et n'en reçoit pas ici. `body.linearVelocity = Vector3.zero:295` = récupération 3.4, **la seule écriture de vitesse autorisée du dépôt**.
- `Assets/RoadRage/Features/Vehicles/DriverModel.cs` -- la décision à préserver intacte : `ComputeAcceleration:61`, `ResolveEffectiveProfile:137`, `SmoothAcceleration:167`, `ShouldEvaluateLaneChange:188`, `ResolveNoisyDesiredSpeed:201`, `IsDeliberateStop:230`. **Aucune modification attendue** : si un changement y devient nécessaire, c'est le signal que la décision a été déplacée, et c'est un écart à consigner.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs` -- **2 `NetworkVariable` serveur-écriture** : `WaypointIndex:26`, `Behavior:31`. Aucune n'est ajoutée, aucune n'est modifiée.
- `Assets/RoadRage/Features/Vehicles/DriverProfile.cs` + `DriverProfileDef.cs` + `Assets/RoadRage/ScriptableObjects/Drivers/DriverProfileDef_Default.asset` -- **aucun champ ajouté par cette story** : la conversion n'a besoin d'aucune donnée authorée nouvelle. Ils ne sont cités que parce que la garde `TheControllerHoldsNoDriveConstantsAndReadsEveryParameterFromTheDef` exige que les paramètres continuent de venir du `Def`.
- `Assets/RoadRage/Tests/EditMode/Story514AiDrivesByIntentTests.cs` -- **à créer**. Conventions du dépôt : classe `public sealed`, `[Test]` seul, namespace `RoadRage.Tests.EditMode`, doc XML en français, helpers locaux (`CodeWithoutComments`, `Occurrences`, `ExtractMethodBody`, `WithInstantiated`). L'asmdef `RoadRage.Tests.EditMode.asmdef` référence déjà `RoadRage.Features.Vehicles`.
- `Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs` -- **une garde à amender, sous Ask First** : celle qui exige `Does.Contain("+ verticalVelocity")` et protège l'écriture verticale supprimée. Les tests de `ComputeSeekIntent` `:29-66` raisonnent aujourd'hui sur la loi de lacet de `ApplyMovement` (`Quaternion.AngleAxis(yaw, Vector3.up)`) : ils doivent être **réécrits sur l'intent**, pas supprimés, sinon la poursuite perd sa preuve de cap. `DriverControllerHasNoSharedStaticStateBeyondItsPureFunctions:128` (`Occurrences(source, "static ") == 3` `:138`) et `DriverControllerAppliesRecoveryWithoutRpcOrAdvancingWaypointIndex` restent verts sans modification.
- `Assets/RoadRage/Tests/EditMode/Story59ParameterizedDriverModelTests.cs` -- **à ne pas modifier**. Gardes qui contraignent directement cette story : `DriverControllerSourcePath:20` ; `TheControllerHoldsNoDriveConstantsAndReadsEveryParameterFromTheDef` (exige les appels `DriverModel.*` **et** `ResolveEffectiveProfile(driverProfile.Profile, behavior)` dans le source du contrôleur) ; `AMissingProfileLeavesTheVehicleInertWithASingleWarningAndNoHardCodedFallback` (`if (driverProfile == null)`, `warnedMissingDriverProfile`, et **aucun** `new DriverProfile(`) ; `TheAiVehiclePrefabCarriesTheDefaultDriverProfile:400` (profil **sur le prefab**, hérité par les trois instances de `MVP_Run`) ; helper `SetPrivateField:647`.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` -- `ReplayRoute` (rejeu cinématique avec les fonctions pures **réelles**) : c'est l'instrument de non-régression de conduite nominale. À exécuter, à ne pas modifier.
- `Assets/RoadRage/Tests/PlayMode/` -- **aucun fichier nouveau attendu** : les deux `[UnityTest]` que l'epic prévoyait (AC2 de l'anomalie, puis AC6 avec AC7) partent avec le périmètre écarté. `Story511…PlayModeTests.cs:247-251` écrit `linearVelocity` **volontairement** (il mesure la suspension) : ne pas le convertir.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- la scène d'intégration et le lieu de la recette : les **3 IA en-scene** sous `RunRoot/AITraffic` (in-scene placed, `NetworkTransform` serveur, aucune `.Spawn()` runtime) et les **deux voitures pilotables** `MVP_RageTargetVehicle_1`/`_2` (instances de `Dev_IndestructibleCar`) y cohabitent. Le relief de recette de la Story 5.13 est sur `Avenue_CenterToEast`.
- `_bmad-output/implementation-artifacts/deferred-work.md` -- **clôtures à écrire** : `:251-252` et `:275-276` (poussée), `:307-308` (décollage vertical), `:299-300` (aides inertes sur l'IA), chacune avec sa mesure. **Laisser ouvertes** : `:240` (surfaces roulantes), l'entrée du harnais PlayMode, les preuves runtime non exécutées de 5.11/5.12/5.13, et le diagnostic inachevé des deux `MVP_RageTargetVehicle_*` non conduisibles. L'entrée de découpe du 2026-09-19 est **déjà appendue** : ne pas la dupliquer. Le fichier est **abîmé par le formateur markdown** par endroits (`source*spec`, `Col_Curb*\*`) : vérifier avant d'appliquer, ne pas aggraver.
- `docs/setup/devworkflow-rollout.md` -- **lacune à combler**. `epics.md` (4e bloc d'AC de la Story 5.14) et `sprint-status.yaml` déclarent le correctif du harnais PlayMode « tracked as tooling in `docs/setup/devworkflow-rollout.md` », or **aucune entrée de ce fichier ne le mentionne** (vérifié le 2026-09-19 : les seules occurrences de « PlayMode » y sont le paramètre `-TestMode` et une ligne de préalable EditMode). Le prérequis est donc déclaré mais non suivi. Ce fichier porte le suivi de l'outillage pur, **hors cérémonie BMAD** (décision de Kenan, 2026-09-17) : c'est là que la ligne doit exister, pas dans les tâches de code.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- remplacer l'écriture par l'intent : supprimer `ApplyMovement` et l'intégration en boucle ouverte, **lire** la vitesse planaire sur le `Rigidbody`, construire un `VehicleDriveIntent` depuis la décision existante (poursuite par `ComputeSeekIntent`, longitudinale par `DriverModel`), et le soumettre à `physicsBody.ApplyDriveIntent(...)` sur le patron de `NetworkedVehicleDriverController.SubmitIntentToPhysicsLayer`, diagnostic d'absence de profil compris -- c'est le cœur de la story, et c'est ce qui rend le véhicule poussable, soumis au relief et sensible aux aides.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- purger `previousAimPoint`/`hasAimPoint` dans `RecoverAtWaypoint`, à côté de `ResetRouteMemoryAt` -- le véhicule reposé vise sinon l'ancien point pendant un pas, et la continuité de visée de la 5.13 repose exactement sur cette mémoire.
- [x] `Assets/RoadRage/Tests/EditMode/Story514AiDrivesByIntentTests.cs` -- **nouveau** : la matrice de cas limites, l'**absence d'écriture de mouvement** dans le fichier IA (hors commentaires) et la **présence de la lecture de vitesse** au lieu de l'intégration, la présence des appels `DriverModel.*` et de `ResolveBehavior`, l'absence de champ de vitesse en boucle ouverte, la purge de la mémoire de visée au repositionnement, et la non-régression du profil. C'est la preuve EditMode de la story.
- [x] `Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs` -- **sous Ask First** : retirer la garde du `+ verticalVelocity`, qui protège l'écriture supprimée, et **réécrire** les tests de `ComputeSeekIntent` sur l'intent là où ils s'appuyaient sur la loi de lacet de `ApplyMovement`. Aucune autre assertion ne bouge, et l'écart est consigné au Spec Change Log.
- [x] `docs/setup/devworkflow-rollout.md` -- **créer la ligne manquante du prérequis dur** (harnais PlayMode rouge 3/33, non filtrable) avec son statut et son déclencheur de réévaluation, en respectant la nature du fichier : outillage pur, hors cérémonie BMAD. Sans cette ligne, l'AC4 de l'epic déclare un suivi qui n'existe pas.
- [x] `_bmad-output/implementation-artifacts/deferred-work.md` -- **clôturer** `:251-252`/`:275-276`, `:307-308` et `:299-300` avec leur mesure, en nettoyant les lignes abîmées rencontrées au passage ; **laisser ouvertes** `:240` et l'entrée du harnais PlayMode, et y consigner ce que cette story laisse non prouvé. Ne pas dupliquer l'entrée de découpe du 2026-09-19.
- [x] `docs/setup/story-5-14-ai-drives-by-intent-notes.md` -- **nouveau** : valeurs et correspondances retenues, mesures, constats et procédure de recette (l'IA roule, la poussée, le relief, les aides sur le trafic) -- trace de livraison, comme les notes 5.11, 5.12 et 5.13.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- `5-14-ai-drives-by-intent` en `in-progress`.
- [ ] `graphify update .` -- regrapher, et comparer à la mesure de la Story 5.13. Le rebuild du crochet de commit est non déterministe : ne pas confondre son churn avec une modification de périmètre. **NON EXÉCUTÉ, et volontairement** : la mesure du dépôt est qu'un `graphify update .` nu re-extrait les nœuds de documentation et gonfle le graphe (2 846 -> 4 034 nœuds mesuré le 2026-09-18) ; c'est le **crochet de commit** qui rebuild, et il le fera au premier commit de cette story. Task laissée ouverte tant que le commit n'a pas eu lieu.

**Acceptance Criteria:**

- Given le contrôleur IA intègre sa vitesse en boucle ouverte et écrit `Rigidbody.linearVelocity` et la rotation directement, when l'IA passe à l'intent, then elle produit un `VehicleDriveIntent` (direction, accélérateur, frein, frein à main) et **n'écrit plus jamais** la vitesse, la position ni la rotation du `Rigidbody`, le champ de vitesse en boucle ouverte est supprimé et la vitesse est relue du `Rigidbody`, et véhicule joueur et véhicules IA tournent sur **le même** composant physique (AD-35).
- Given le modèle de conduite paramétré de la Story 5.9 doit survivre intact, when la suite EditMode tourne, then les fixtures `DriverModel`, IDM et MOBIL de la Story 5.9 passent **vertes sans modification** ; un échec signale une fuite de la physique dans la couche de décision et est corrigé avant que la story continue.
- Given `deferred-work.md:251-252` et `:275-276` exigent qu'une IA poussée soit déviée au lieu de repartir sur sa ligne, when cette story est livrée, then la poussée n'est **plus effacée** au pas de physique suivant — le véhicule est dévié, peut être mis en travers et peut quitter la chaussée — et **ne se réaligne pas en une frame**. Ce qui reste après le choc (la réaction conduite) appartient au périmètre écarté et est **connu**, pas découvert.
- Given `deferred-work.md:307-308` constate qu'une IA qui franchit le relief de recette décolle et poursuit son tracé en l'air, when l'IA passe par la couche physique, then la gravité redevient **effective** sur une caisse d'IA : au franchissement du dos-d'âne et de la marche basse de 0,12 m, le véhicule touche le sol, ne décolle pas et ne poursuit pas son tracé en l'air.
- Given `deferred-work.md:299-300` constate que les aides arcade et la réduction d'autorité n'ont **aucun** effet sur l'IA, when l'IA passe par la couche physique, then les aides de la Story 5.13 et la réduction d'autorité par roues au sol s'appliquent au trafic IA **par construction** — même couche, même profil — et c'est **cette story** qui est observée sur l'effet des aides sur le trafic, à la recette.
- Given la suite PlayMode est rouge (3/33) et non filtrable, and un des fixtures rouges est `Story57AiTrafficClientPresentationPlayModeTests` — le domaine de cette story, when cette story est ordonnancée, then le correctif du harnais PlayMode est un **prérequis dur** consigné dans `docs/setup/devworkflow-rollout.md`, et **aucune absence de résultat n'est lue comme un succès**.
- Given `deferred-work.md:240` reporte les surfaces roulantes différenciées avec une condition de réouverture, when cette story devient la première à laisser un véhicule quitter la chaussée, then le report reste un report : aucune table de surfaces n'est inventée et la condition de réouverture enregistrée est conservée telle quelle.
- Given aucune perturbation ne se produit, when le trafic circule normalement, then le routage et le comportement de la Story 5.10 sont **inchangés** (AC8 de l'anomalie, ici en non-régression), le point de visée et sa continuité de la 5.13 compris.

## Spec Change Log

**2026-09-19 — Garde de la Story 5.12 non anticipee au planning.** Declencheur : la premiere
execution EditMode complete est sortie rouge sur
`Story512TireForcesAndSteeringTests.TheTransientStateIsNamedAndTheStoryThatLiftsItIsNotThisOne`, qui
exige que le controleur IA ecrive encore sa vitesse en bloc et impose son lacet -- et dont le
commentaire annonce que la Story 5.14 levera cet etat intermediaire. Le Code Map de ce spec nommait
les fixtures 5.2 et 5.9 a amender, pas celle de 5.12 : c'est une **lacune de planification**, pas une
surprise de l'implementation. Amendement : decision humaine du 2026-09-19, la garde est **retiree**
et non rearmee sur l'autre versant, un bloc de passation prend sa place, et l'invariant (aucune
ecriture de vitesse, position ou rotation depuis un chemin de conduite) est tenu par
`Story514AiDrivesByIntentTests`, qui l'assere sur le pas de conduite de `FixedUpdate` au lieu du
fichier entier. Etat connu-evite : deux copies de la meme assertion dans deux fixtures de stories
differentes, qui divergeraient au premier changement. KEEP : **dater et nommer un etat intermediaire
est ce qui a rendu visible, sans lire le code, que la 5.14 n'etait pas livree** -- la pratique est
bonne et survit a la garde qui l'exprimait.

**2026-09-19 — Deux commentaires decrivaient un code disparu.** Declencheur : releve pendant
l'implementation, puis arbitre par l'humain qui a demande la correction immediate plutot que le
renvoi au registre. Amendement : l'en-tete de `VehiclePhysicsBody.cs` (il annoncait encore l'IA
ecrivant `linearVelocity` et `MoveRotation`) et le doc de `Story510…ReplayRoute` (il renvoyait a
`ApplyMovement` comme a une methode vivante). Les deux fichiers etaient declares « a ne pas
modifier » par ce spec : le spec designait les **mecanismes** a ne pas toucher, pas les phrases qui
les decrivent devenues fausses. Etat connu-evite : un prochain lecteur -- ou un prochain agent --
prenant ces commentaires pour l'etat du code. KEEP : `ReplayRoute` reste le rejeu d'une cinematique
**typique**, volontairement recopiee ; il ne doit pas etre rattache a une methode vivante.

## Design Notes

- **Pourquoi la poussée ne se corrige pas par une force.** La poussée n'est pas une force à contrebalancer : c'est une **écriture à supprimer**. Tant que `linearVelocity` est réassigné à chaque pas, ce que la physique a gagné pendant le contact est effacé au pas suivant — un « couple de résistance » ajouté dans la couche de conduite ne ferait que rendre l'effacement plus coûteux à écrire. Corollaire symétrique, et c'est lui qui fixe l'ordre des stories : l'écriture ne peut **pas** être supprimée avant que les efforts viennent des roues, sinon l'IA n'a plus aucune source de force longitudinale et reste figée. C'est pourquoi les Stories 5.11 à 5.13 devaient précéder celle-ci.
- **Pourquoi le décollage vertical est le même défaut, pas un second.** `ApplyMovement` relit la composante verticale puis la **réécrit** dans le vecteur reconstruit : une vitesse verticale donnée par un contact est réinjectée au lieu d'être consommée. La gravité est active (`m_UseGravity: 1` sur les deux prefabs) et n'est pas la variable. Le fait que la composante **horizontale** soit forcée à `forward * longitudinalSpeed` achève l'explication : la vitesse est une **entrée** au lieu d'être une **conséquence**, donc l'IA ne peut ni ralentir sur une bosse ni être ralentie par elle. Le garde de la Story 5.2 sur `+ verticalVelocity` protège aujourd'hui ce read-modify-write : il est retiré **parce que** sa raison d'être disparaît, et non parce qu'il gêne.
- **Ce qui reste volontairement en place, et pourquoi ce n'est pas un oubli.** `RecoverAtWaypoint` continue de téléporter (vide, retournement soutenu, blocage prolongé hors de vue d'un joueur), et **aucune réaction au choc** n'est ajoutée. Ce n'est pas une moitié de travail : `ApplyMovement` réécrit le vecteur vitesse à chaque pas, donc il n'y a **rien à suspendre ni à reprendre** tant que ce chemin existe. Supprimer d'abord l'écriture et traiter la réaction ensuite est ce qui rend la seconde possible ; l'inverse produirait une machine de réaction dont chaque effet serait écrasé au pas suivant, exactement le mode d'échec que ce dépôt a déjà payé quatre fois.
- **Les gardes de texte sont le piège mesuré de ce dépôt.** Elles scannent le fichier entier, commentaires compris : une garde qui interdit `linearVelocity` dans le contrôleur IA rougira sur un commentaire qui explique sa disparition, et une garde qui interdit `MoveRotation` rougira sur la phrase qui dit qu'il a été retiré. La Story 5.12 a payé quatre faux rouges de cette façon. Retirer les lignes `//` avant d'asserter, et nommer des jetons précis (un jeton comme `"Rage"` matche `RoadRage`, et `"linearVelocity"` matche une **lecture** légitime).
- **Le risque principal de cette story n'est pas l'écriture, c'est l'immobilité.** Une conversion d'intent peut compiler, passer toutes les gardes EditMode et produire un véhicule qui **ne bouge plus** : les tests EditMode ne prouvent pas qu'une voiture avance, et l'epic n'exige plus de test PlayMode pour les AC de cette story depuis la découpe. La recette humaine est donc la **mesure de clôture** : la première chose à observer est qu'une IA **roule**.
- **Ce risque est accru par une dette ouverte qu'il faut connaître maintenant.** `deferred-work.md` porte un diagnostic **inachevé** (Story 5.13) : les deux `MVP_RageTargetVehicle_*` de `MVP_Run` ne répondent pas à l'accélérateur — l'intention est bien lue et bien livrée à `VehiclePhysicsBody` (`Throttle = 1`), mais la mesure s'arrête avant la rotation de roue, les échantillons de pneu et l'angle de roue, et trois causes restent possibles (couple non intégré, charge portée nulle, blocage géométrique). Ce sont **des instances d'un autre prefab** que les trois IA, donc ce n'est pas un blocage démontré — mais c'est la même couche, et c'est la raison pour laquelle la recette de cette story commence par « une IA roule-t-elle ? » et non par les cas fins.
- **Ce que cette story ne prouve pas.** Aucun test PlayMode automatisé ne la couvre : les deux `[UnityTest]` que l'epic prévoyait partent avec le périmètre écarté. Les comportements de poussée, de relief et d'effet des aides sont des comportements **runtime** mesurés par la recette humaine, consignés dans la note de livraison ; sans résultat brut rapporté, la story ne passe pas en revue — même régime que les Stories 5.11 à 5.13, avec cette différence que la mesure est ici la **seule** preuve de ces trois AC.

## Verification

**Commands:**

- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story514AiDrivesByIntentTests"` -- attendu : fixture verte, exit 0.
- `.\scripts\validate.ps1 -TestMode EditMode` -- attendu : les 601 verts de la baseline 5.13, dont les fixtures **5.9 inchangées**, plus ceux de cette story ; 0 erreur Console. Si la porte Console reste rouge sur des entrées historiques rejouées sans `--since`, la mesure de repli est `unity cmd run_tests --mode EditMode` + `test_status`, et le fait est rapporté tel quel.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- attendu : **non productible par l'agent** (harnais rouge, non filtrable). Ne jamais conclure d'un résultat vide. Cette story n'ajoute aucun test PlayMode, donc **aucun gate PlayMode ne peut la valider ni l'invalider** : le dire, ne pas le contourner.
- `graphify update .` -- attendu : graphe régénéré, comparaison avec la mesure de la Story 5.13.

**Mesures du 2026-09-19 (rapportees brutes, y compris ce qui ne va pas) :**

- `unity cmd run_tests --mode EditMode` -> **613 tests, 613 passes, 0 echec**, fixture
  `Story514AiDrivesByIntentTests` presente avec ses 11 tests. Mesure **reproduite deux fois**.
- `unity cmd test_status` -> `total 614, passed 613, failed 1`, en citant
  `Story512…TheTransientStateIsNamedAndTheStoryThatLiftsItIsNotThisOne`, **un test qui n'existe plus
  dans le fichier**. Divergence mesuree entre les deux commandes : `test_status` a rendu un resume
  fige d'une execution anterieure et **ne doit pas etre lu comme le verdict courant** tant que ce
  point n'est pas instruit.
- `\.\scripts\validate.ps1 -TestMode EditMode` -> **rouge sur la porte Console, lue sans `--since`** :
  65 entrees d'erreur rejouees (`[Netcode] ServerSpawnSceneObjectsOnStartSweep` puis 64
  `NullReferenceException` dans `NetworkObject.OnNetworkBehaviourDestroyed`). Attribution mesuree :
  `unity cmd console --level error --since 712` -> **0 entree nouvelle** apres l'execution complete de
  la suite, donc **ces erreurs ne sont pas reproduites par la suite EditMode**. Horodatage 16:44-16:45
  local, Editeur hors Play Mode au releve. Leur origine n'est **pas etablie** : c'est un constat, pas
  une explication.
- `\.\scripts\validate.ps1 -TestMode EditMode -TestFilter "…Story514AiDrivesByIntentTests"` -> **aucun
  resultat de test**, parce que le script s'arrete a la porte Console et n'execute donc pas la suite.
  Le filtre n'a jamais servi ici : le verdict vient de `run_tests`.

**Audit de la matrice I/O (exigence de l'etape d'implementation) :** les lignes « conduite nominale »,
« vitesse lue jamais entretenue », « repositionnement » et « profil incomplet » sont couvertes par des
tests EditMode qui ont tourne et sont verts. Les lignes **« poussee sans choc », « relief de recette »
et « aides et autorite » n'ont AUCUN test qui les couvre** : ce sont des comportements d'execution, et
l'epic a volontairement plafonne la porte PlayMode de cette story a zero test depuis la decoupe du
2026-09-19. Elles sont couvertes par la **recette humaine** declaree ci-dessus, et la ligne « choc
subi » est explicitement hors perimetre. Cet ecart est **signale, pas masque** : il est la raison pour
laquelle la recette est une condition de cloture.

**Manual checks (if no CLI):**

- **Blocage de clôture — recette humaine, Éditeur, `MVP_Run`, hôte.** À exécuter après livraison ; sans résultat brut rapporté, la story ne passe pas en revue.
  - **L'IA roule (à faire en premier).** Observer le trafic du carrefour en conduite nominale : les trois IA doivent **avancer**, atteindre leur vitesse de croisière, franchir les nœuds et boucler leur parcours. Une IA immobile est un échec de la story, pas un défaut d'ambiance — vérifier alors `GroundedWheelCount` et le facteur d'autorité dans la vue de télémétrie sur le véhicule IA, et rapporter la mesure telle quelle.
  - **Poussée (dette `:251-252`/`:275-276`).** Percuter une IA avec la voiture joueur, puis la pousser franchement. Attendu : le véhicule est **dévié**, peut être mis en travers, peut sortir de la chaussée, et **ne se réaligne pas en une frame**. Recommencer par poussée continue sans choc net : même attente.
  - **Relief (dette `:307-308`).** Faire franchir le dos-d'âne puis la marche basse de `Avenue_CenterToEast` à une IA. Attendu : elle suit le relief **comme le joueur**, ne décolle pas, ne poursuit pas son tracé en l'air. C'est la mesure qui clôt la dette.
  - **Aides sur le trafic IA (dette `:299-300`).** En virage et en appui, vérifier qu'une aide de la 5.13 est perceptible sur une IA, puis remettre sa valeur authorée à zéro et vérifier qu'elle disparaît.
  - **Non-régression de conduite (AC8).** Trafic nominal du carrefour et des giratoires : tirage de virage, portails, point de visée, aucune IA à l'arrêt, aucune IA coupant l'intérieur d'un giratoire plus qu'avant. Toute régression de nature différente (ordre des fixtures, etc.) est rapportée **telle quelle**, sans être attribuée à cette story.
  - **Ce qui n'est pas jugé ici :** la réaction après un choc significatif. Elle n'est pas livrée, son absence est attendue, et il ne faut pas la lire comme une régression.
- Garde de double état d'`AGENTS.md` : `MVP_Run.unity` et `git status --short` doivent revenir à leur état initial après la recette.
