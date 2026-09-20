---
title: "Lobby-Configurable Traffic Settings"
type: "feature"
created: "2026-09-20"
status: "in-review"
review_loop_iteration: 0
baseline_commit: "9078d7accc49f0c0d5433936ddb1c40c0019a571"
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** L'effectif du trafic vient aujourd'hui du seul `TrafficSettingsDef`, et `PortalTrafficSpawner.ResolveSessionTargetPopulation` n'a pas de valeur de session à brancher : l'hôte ne peut pas régler depuis le lobby combien de véhicules IA peuplent la ville ni combien jettent des détritus, donc il ne peut pas régler ses conditions de test sans toucher au code ou à une scène.

**Approach:** Étendre `MatchSettings` (jamais un second objet) avec deux valeurs de session bornées par des bornes authorées ajoutées à `TrafficSettingsDef`, les publier aux clients par le chemin de la Story 2.4 (données de lobby Steam, comme `difficulty`) **avant** le chargement du monde, puis les déposer dans `NetworkedRunState` comme valeurs de session hôte-owned que le spawner lit pour composer le trafic.

## Boundaries & Constraints

**Always:** Étendre `MatchSettings` et `TrafficSettingsDef` — jamais créer un second objet de réglages ni un second Def. Les bornes et défauts sont authorés dans le Def ; la valeur choisie vit dans `MatchSettings` puis dans `NetworkedRunState`. Le chemin de la Story 2.4 reste le **seul** chemin qui livre les valeurs avant le chargement du monde ; `NetworkedRunState` est l'état autoritatif de run, pas un chemin de pré-chargement alternatif. Seul l'hôte édite et publie ; un client reste en lecture seule. Le nombre de jeteurs ne dépasse jamais le total. Les écrans UI ne connaissent que des entiers et des bornes entières : jamais `MatchSettings`, jamais un Def. `MVP_Run.unity` ne doit contenir aucune chaîne `TargetPopulation`.

**Ask First:** Si satisfaire l'invariant jeteurs ≤ total exige de modifier la difficulté existante ou `LobbyRosterSnapshot` d'une manière qui casse les fakes des Stories 2.2/2.3/2.4/2.8, ou s'il faut un troisième chemin de synchronisation, arrêter et demander l'arbitrage humain.

**Never:** Aucun effectif littéral dans un contrôleur, un prefab ou une scène. Aucun système de détritus : la Story 5.20 le portera, cette story n'expose que la valeur résolue. Aucune seconde RPC, aucun second objet de settings, aucune borne inventée en repli d'un Def absent, aucune modification des prefabs Synty, aucune retéléportation ni retrait de trafic.

## I/O & Edge-Case Matrix

| Scenario        | Entrée / état                                         | Sortie attendue                                 | Gestion d'erreur                                         |
| --------------- | ----------------------------------------------------- | ----------------------------------------------- | -------------------------------------------------------- |
| Butée           | clic `+` alors que la valeur est à la borne effective | valeur inchangée, bouton `+` inactif            | butée structurelle, aucune notice                        |
| Jeteurs > total | l'hôte baisse le total sous le nombre de jeteurs      | total inchangé                                  | notice de refus distincte et visible, valeur non publiée |
| Def absent      | `trafficSettings` non assigné dans `MainMenuLobby`    | valeurs non éditables                           | avertissement unique, aucune borne inventée              |
| Sans session    | run démarré hors lobby (Dev, `MVP_Run` seule)         | valeur de run jamais écrite (`-1`)              | repli sur le défaut authoré du Def                       |
| Client en lobby | l'hôte a publié puis lance le run                     | client reçoit les deux valeurs avant chargement | l'hôte ne se réapplique jamais sa propre lecture         |
| Deux runs       | valeur changée entre deux runs                        | second run utilise la nouvelle valeur           | aucun rechargement d'asset ni recompilation              |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/TrafficSettingsDef.cs` -- Def auteur du trafic : `defaultTargetPopulation`/`min`/`max` + `ClampTargetPopulation` + `TryValidate:79`. Ajouter les bornes/défaut jeteurs ici ; asset `ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset`.
- `Assets/RoadRage/Features/Lobby/MatchSettings.cs` -- classe pure, `Difficulty` + emplacement réservé. Y ajouter les deux valeurs de session ; rester non-MonoBehaviour/non-ScriptableObject (garde `Story12LobbyShellTests:42`).
- `Assets/RoadRage/Features/Online/ISteamLobbyPlatform.cs` -- interface sans SDK ; les méthodes optionnelles ont un corps par défaut (`SetLobbyJoinCode`, `JoinLobbyByCodeAsync`). `LobbyRosterSnapshot` (l.110+) : ctor à paramètres optionnels, `Empty`, afin que les fakes 2.2/2.3/2.4/2.8 compilent sans modification.
- `Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs` -- clés de données de lobby `difficulty`/`runLaunchRequested`/`joinCode` ; écrire/lire les deux nouvelles clés ici, jamais ailleurs.
- `Assets/RoadRage/Features/Online/LobbyRosterService.cs` -- `PublishDifficulty:119` (no-op hors room hôte ouverte) : même forme pour la publication. **Piège** : `SnapshotsEqual:196` ne compare que `HasLobby`/`OwnerId`/`Difficulty`/`RunLaunchRequested`/membres — sans y ajouter les deux valeurs, un changement de trafic ne lève jamais `RosterChanged` et le client ne reçoit rien. `HandleRoomStatusChanged` ne vide `Current` que hors room ouverte : le service persistant de `RoadRageBootstrap` porte donc encore l'instantané pendant toute la run.
- `Assets/RoadRage/Features/UI/LobbyRosterScreen.cs` -- écran post-room (Story 2.4) ; modèle `ShowSettingsSummary`/`SetDifficultyEditable`/`CycleDifficulty`+event. Objet porteur : `/Canvas/LobbyPanel` de `MainMenuLobby.unity`.
- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` -- couture : `Settings:110`, `HandleDifficultyChanged:641`, relecture client `:749-760`, `PublishDifficulty` sur room Open `:856`, notices `PublishUnavailable:953`. C'est ici que vivent bornes, refus et publication.
- `Assets/RoadRage/Features/Run/NetworkedRunState.cs` -- état de run hôte-owned in-scene (`HostOwnedNetworkStateBehaviour`, `NetworkVariable` server-write). `SessionSeed` y est déclaré mais jamais écrit : ne pas s'en servir comme précédent de transport.
- `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` -- `ResolveSessionTargetPopulation:150` est le point de branchement unique documenté pour la Story 5.16 ; `FixedUpdate:73` résout `deficit = ResolveSessionTargetPopulation(settings) - liveVehicles.Count`. Garder exactement une définition et un appel.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` -- `NoTrafficPopulationLivesOutsideTheAuthoredDef:712` fige aujourd'hui `settings.ClampTargetPopulation(settings.DefaultTargetPopulation)` et `MVP_Run.unity` sans `TargetPopulation` : l'assertion doit être réécrite pour la nouvelle forme unique, jamais supprimée ni affaiblie.
- `Assets/RoadRage/Tests/EditMode/Story24LobbyRosterTests.cs` -- patron `FakeSteamLobbyPlatform`/`SetDifficultyCallCount` à imiter pour la publication.
- `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- authoring des nouveaux contrôles sous `/Canvas/LobbyPanel` + assignation du Def sur `LobbyFlowController`. `/Canvas/NoticePanel/NoticeText` est un panneau frère : la notice de refus y reste visible quel que soit le panneau actif.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- `NetworkedRunState` et `PortalTrafficSpawner` y vivent déjà ; aucune retouche de scène attendue.
- Assemblies : `Features.Lobby`/`Features.Online`/`Features.UI`/`Features.Vehicles` ne référencent que `RoadRage.Shared` ; `RoadRage.App` référence déjà tout. Aucun asmdef à modifier.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Vehicles/TrafficSettingsDef.cs` + `ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset` -- ajouter jeteurs défaut `2`, bornes `0`-`8`, `ClampLitterThrowers` et les invariants de `TryValidate` -- bornes authorées, aucune borne en code.
- [x] `Assets/RoadRage/Features/Lobby/MatchSettings.cs` -- deux valeurs de session entières -- une seule source de session, mappable vers l'état réseau.
- [x] `Assets/RoadRage/Features/Online/ISteamLobbyPlatform.cs` + `FacepunchSteamLobbyPlatform.cs` -- `SetLobbyTrafficSettings` (corps par défaut vide) et deux champs `int` dans `LobbyRosterSnapshot` (ctor à paramètres optionnels, `-1` = non publié) lus/écrits comme `difficulty` -- un seul chemin de livraison.
- [x] `Assets/RoadRage/Features/Online/LobbyRosterService.cs` -- `PublishTrafficSettings` sur le modèle de `PublishDifficulty`, et comparaison des deux valeurs dans `SnapshotsEqual` -- sans quoi un changement ne se propage jamais.
- [x] `Assets/RoadRage/Features/UI/LobbyRosterScreen.cs` -- deux lignes valeur+borne, boutons `-`/`+`, deux events de pas entier, un setter d'affichage des valeurs et un setter d'éditabilité ; bouton inactif à la borne -- l'écran n'invente ni borne ni refus.
- [x] `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` -- `[SerializeField] TrafficSettingsDef`, amorçage de `Settings` en `Awake`, validation bornes + invariant jeteurs ≤ total avec notice de refus distincte, publication sur room Open et à chaque acceptation, repli du client sur l'instantané reçu -- décision et retour visible centralisés ici.
- [x] `Assets/RoadRage/Features/Run/NetworkedRunState.cs` -- `NetworkVariable<int> AiVehicleTargetCount` et `LitterThrowerCount`, server-write `Everyone`, `-1` = non résolu ; ces deux noms évitent la chaîne interdite dans `MVP_Run.unity` -- valeurs de session hôte-owned, seul état autoritatif de la run.
- [x] `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` -- publier une fois les valeurs résolues depuis l'instantané de lobby persistant, puis résoudre l'effectif depuis l'état de run avec repli sur le défaut du Def, dans `ResolveSessionTargetPopulation` seul -- un point de branchement, aucun littéral.
- [x] `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- authorer les deux lignes sous `/Canvas/LobbyPanel`, assigner les champs sérialisés de l'écran et le Def sur `LobbyFlowController`, sauver -- sans objet authoré, rien n'est éditable.
- [x] `Assets/RoadRage/Tests/EditMode/Story516LobbyConfigurableTrafficSettingsTests.cs` -- couvrir la matrice I/O, la publication conditionnelle, le piège `SnapshotsEqual`, les bornes du Def et les gardes de source -- preuve des invariants.
- [x] `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` -- réécrire `NoTrafficPopulationLivesOutsideTheAuthoredDef` vers la nouvelle forme de résolution unique en conservant son intention -- contrat 5.10 mis à jour, pas neutralisé.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- passer `5-16-lobby-configurable-traffic-settings` à `in-progress` -- suivi de sprint.
- [x] `graphify update .` -- régénérer le graphe après les changements source.

**Acceptance Criteria:**

- Given `MatchSettings` porte déjà un emplacement réservé aux paramètres extensibles, when les réglages de trafic sont ajoutés, then ils étendent `MatchSettings` sans créer de second objet, et défauts et bornes vivent dans `TrafficSettingsDef`.
- Given l'hôte ouvre les réglages de partie, when il ajuste le nombre de véhicules IA puis le nombre de jeteurs, then chaque valeur reste dans ses bornes authorées, la borne atteinte désactive le bouton correspondant, une baisse du total sous le nombre de jeteurs est refusée avec un retour visible, et seul l'hôte peut éditer.
- Given un client est connecté au lobby, when l'hôte lance la run, then le client dispose des deux valeurs avant le chargement du monde, par le chemin de la Story 2.4 seule.
- Given la run démarre, when l'hôte compose le trafic, then l'effectif cible et le nombre de jeteurs viennent des réglages résolus, aucun contrôleur, prefab ou scène ne porte d'effectif, et changer les valeurs entre deux runs prend effet sans recompilation ni édition de scène.

## Spec Change Log

## Design Notes

`-1` est le seul sentinel, et il est partagé par les trois porteurs (instantané de lobby, `NetworkedRunState`) : il signifie « aucune session ne l'a résolu ». `0` est une valeur légale du Def, donc il ne peut pas porter ce sens. Sans session, le spawner retombe sur `DefaultTargetPopulation` : c'est le repli déjà documenté, qui garde verts les bancs PlayMode existants des Stories 5.7/5.9/5.10.

L'instantané de lobby est relu au démarrage de la run, et non un troisième porteur : `LobbyRosterService` vit sur `RoadRageBootstrap`, survit au changement de scène, et `HandleRoomStatusChanged` ne vide `Current` que quand la room se ferme. La valeur de session est donc publiée par l'hôte, transportée par le chemin 2.4, et déposée dans `NetworkedRunState` — un seul chemin, deux étages.

L'UI ne reçoit que des entiers et des bornes **effectives** : la borne haute des jeteurs passée à l'écran est `min(borne authorée, total courant)`, ce qui rend l'invariant visible par la butée sans que l'écran connaisse l'invariant.

Le sentinel `-1` vit une seule fois, dans `RoadRage.Shared.Domain.SessionTrafficValue.Unresolved` : ses trois porteurs (`LobbyRosterSnapshot` dans `Features.Online`, `MatchSettings` dans `Features.Lobby`, `NetworkedRunState` dans `Features.Run`) sont dans trois assemblies qui ne se référencent pas entre elles, et une constante recopiée trois fois serait trois fois fausse le jour où elle change. `Shared` est la seule surface qu'ils partagent tous les trois.

## Verification

**Commands:**

- `unity cmd run_tests --mode EditMode --filter "RoadRage.Tests.EditMode.Story516LobbyConfigurableTrafficSettingsTests" --filter_type testName` -- exécuté : **28/28 verte** (22 avant l'ajout des six preuves réclamées par la revue).
- `.\scripts\validate.ps1 -TestMode EditMode` -- exécuté : **OK, 645/645**. Le gate Console était propre à cet instant ; il ne l'est plus (voir la limite ci-dessous), donc ce verdict est daté.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- exécuté **avant** les corrections hors périmètre : 45 tests, 7 en échec. Il s'arrête désormais **avant** les tests sur `ECHEC: 6 erreur(s) en Console (AD-7)` : six `error CS0103` historiques produites par un état intermédiaire de cette story et corrigées depuis (`recompile_status` : `completed, failed: false`). `clear_console` répond `{cleared: true}` et les rejoue quand même. La voie directe `run_tests --async_tests true` revient avec `total: 0` (panne de harnais déjà consignée). **Le verdict PlayMode n'a donc pas été obtenu** : voir `deferred-work.md`, condition de clôture « faire tourner la suite PlayMode dans le Test Runner ».
- `git status --short` -- exécuté : seuls les fichiers de la story, la sortie Graphify et les scripts de travail.

**Hors périmètre, sur demande explicite de l'humain (2026-09-20)** : la suite EditMode n'était pas verte avant cette story. Mesure avant corrections, avant les six preuves de revue : **638 tests, 2 en échec** ; après corrections : **645 tests, 0 en échec**. Les deux dérives de gardes de source ont été corrigées sans toucher au code produit, et leur intention a été conservée :

- `Story510...NothingRemovesATrafficVehicleAnywhereButAnExitPortal` interdisait la sous-chaîne `Despawn(`, que `OnNetworkDespawn()` (`NetworkedAIVehicleDriverController.cs:299,302`) contient déjà — le test avortait donc **avant** ses assertions sur le spawner, qui n'étaient jamais évaluées. La garde compte désormais `Despawn(` moins `OnNetworkDespawn(`.
- `Story513...TheWheelSpinIsStillIntegratedWhileNoWheelTouchesTheGround` cherchait `ResolveTireForces(` dans `VehiclePhysicsBody.FixedUpdate` ; le corps appelle désormais `VehicleTireModel.IntegrateDrivenContact`, qui résout les forces de pneu par sous-pas. La garde vise ce point d'entrée, l'invariant (« en l'air, la roue continue de tourner ») est inchangé.
- L'angle mort créé par la première (les invariants de retrait du spawner n'étaient plus évalués) est fermé côté 5.16 par `AddingTheSessionBranchOpensNoSecondRemovalPathInTheSpawner`, sur le fichier réellement modifié par cette story.
- Les **sept échecs PlayMode** des bancs 5.12 et 5.13 ont été diagnostiqués et corrigés (isolation de scène de banc, aucune ligne de code produit touchée). Voir `deferred-work.md` pour le diagnostic mesuré, le banc 5.11 laissé volontairement tel quel, et l'absence de verdict machine.

**Manual checks (if no CLI):**

- Dans `MVP_Run`, hôte puis client : l'effectif observé correspond à la valeur réglée dans le lobby, deux runs avec des valeurs différentes donnent deux populations différentes, et le refus « jeteurs > total » affiche bien une notice dans le panneau de notices.
- Limite assumée et à consigner telle quelle : la vérification retenue est **EditMode seulement**, donc la couture `LobbyFlowController` (retour visible, éditabilité, ordre avant chargement) est couverte par des gardes de source et par la recette manuelle, pas par un test PlayMode. Si la recette manuelle échoue, le classement de vérification doit être rouvert avant de bricoler le code.
- La Console de l'Editeur porte déjà des erreurs antérieures à cette story (compilation PlayMode de la Story 5.15, séquences `seq 79-786`) plus trois erreurs `CS0103` produites par un état intermédiaire de cette story et corrigées depuis. `scripts/validate.ps1` gate sur `console --level error` **sans `--since`** : il échoue donc avant d'exécuter le moindre test, et aucune relance ne peut le rendre vert dans cette session d'Editeur. Sortie brute relevée telle quelle : `ECHEC: 13 erreur(s) en Console (AD-7). VALIDATION FAILED / INCOMPLETE`. La preuve de test a donc été obtenue par `unity cmd run_tests` directement, et non en contournant le gate autrement.

### Review Findings

- [ ] [Review][Patch] [High] Gate traffic spawning until lobby settings resolve [Assets/RoadRage/App/Run/PortalTrafficSpawner.cs:127-130]
- [ ] [Review][Patch] [Medium] Restore host settings from the persistent lobby snapshot after returning from a run [Assets/RoadRage/App/Lobby/LobbyFlowController.cs:125-128, 900-928]
- [ ] [Review][Patch] [Medium] Validate the traffic pair before writing it to run state [Assets/RoadRage/App/Run/PortalTrafficSpawner.cs:214-219]
- [ ] [Review][Patch] [Low] Validate received settings before displaying them to clients [Assets/RoadRage/App/Lobby/LobbyFlowController.cs:912-928]
- [ ] [Review][Patch] [Low] Disable traffic controls when the TrafficSettingsDef is missing [Assets/RoadRage/App/Lobby/LobbyFlowController.cs:777-791]
- [ ] [Review][Patch] [Medium] Test the lobby-to-run-state handoff and its pending-snapshot retry [Assets/RoadRage/Tests/EditMode/Story516LobbyConfigurableTrafficSettingsTests.cs:387-406]
- [ ] [Review][Patch] [Medium] Test the Steam traffic metadata write/read round trip [Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs:137-152, 186-195]
- [ ] [Review][Patch] [Low] Restrict scene-authoring lookups to MainMenuLobby [AgentScripts/Story516LobbyTrafficRowsAuthoring.cs:51-58, 85-99]
- [ ] [Review][Patch] [Low] Stop scene authoring when expected Button or TMP_Text components are missing [AgentScripts/Story516LobbyTrafficRowsAuthoring.cs:75-84, 115-125]
- [ ] [Review][Patch] [Low] Tear down isolated PlayMode scenes after each test [Assets/RoadRage/Tests/PlayMode/Story512TireForcesAndSteeringPlayModeTests.cs:73-101; Assets/RoadRage/Tests/PlayMode/Story513ArcadeAssistsAndUnevenGroundPlayModeTests.cs:90-128]