# Audit de cohesion — RunFlowController.cs

Item P1 du backlog `docs/setup/devworkflow-rollout.md` : *"Audit de cohesion `RunFlowController.cs` (1522 lignes, 82 methodes, 21% du churn du repo)"*.

Perimetre : lecture seule, aucune modification de code, aucun `graphify update`. Conforme a AD-13 (devworkflow spine) : produire une preuve, pas justifier un refactor decide d'avance.

Date : 2026-09-17. Fichier audite : `Assets/RoadRage/App/Run/RunFlowController.cs` (1522 lignes confirmees).

---

## 1. Responsabilites reelles

Lecture integrale du fichier. Les ~82 methodes (77 `private`/`static private` + methodes publiques) se regroupent en 9 clusters fonctionnels, chacun correspondant a un flux precis du run (spawn, degats, siege, HUD, cible de rage). Aucune methode n'est isolee ou orpheline d'un cluster.

| # | Groupe | Methodes principales | ~Lignes |
|---|---|---|---|
| 1 | Cycle Unity (orchestration) | `Start`, `Update`, `OnDestroy` | ~110 (appels vers les autres groupes) |
| 2 | Spawn joueur local / profil | `TrySpawnSelectedProfile`, `BindHudRuntimeState`, `DisableVisualColliders`, `AttachNetworkPoseReporter`, `AttachLocalVoidRespawnController`, `ResolveSpawnPoint`, `PublishWarning`, `ResolveLobbyState` | ~150 |
| 3 | Actions passager (host-validated intent) | `EnsurePassengerActionBinding`, `UnsubscribeFromPassengerActionIntent`, `EnsureAuthoritativePassengerActionBindings`, `HandlePassengerActionValidated`, `RefreshPassengerActionIncidentHud` | ~110 |
| 4 | Cible de rage / verrou T-Y + spawn dev vehicules | `TrySpawnDevVehicle`, `HandleRageTargetDevControls`, `HandleRageTargetLockControls`, `RevalidateRageTargetLock`, `ApplyRageTargetLock`, `ApplyRageTargetLockMarker`, `ResolveLockRageState`, `RefreshFocusedRageHud`, `EnsureMinimumMvpRageTargets`, `CountDriveableRageVehicles`, `TryFindDevVehicleSpawnPose`, `FindRageTargets`, `CreateDevVehicle` | ~350 |
| 5 | Pont HUD reseau (compteur joueurs) | `EnsureNetworkHudBridge`, `HandleNetworkPlayerCountChanged`, `ResolveConnectedPlayerCount` | ~40 |
| 6 | Degats / collision vehicule | `EnsureVehicleEventBridge`, `UnsubscribeFromVehicleEvents`, `EnsureSecondaryVehicleDamageBridges`, `ApplySecondaryVehicleCollisionDamage`, `HandleVehicleCollided`, `ApplyCollisionConsequencesIfAuthoritative`, `ApplyNetworkedCollisionDamage`, `ShowVehicleInoperableMessageIfNeeded`, `SubscribeToVehicleState`, `UnsubscribeFromVehicleState`, `HandleVehicleHpChanged`, `HandleVehicleDamageFlagChanged`, `RefreshVehicleDamageHud`, `ResolveObservedVehicleState`, `IsAuthoritativeForDamage`, `HandleVehicleRecovered` | ~350 |
| 7 | Etat joueur reseau -> HUD (HP/stamina/argent/lifecycle/mode/siege) | `ResolveLocalNetworkedPlayerStateIfNeeded`, `ResolveLocalNetworkedPlayerState`, `Subscribe/UnsubscribeFromLocalOnFootController`, `HandleLocalStaminaChanged`, `Subscribe/UnsubscribeFromLocalNetworkedPlayerState`, `RefreshHudFromLocalNetworkedPlayerState`, `HandleHpChanged`, `HandleReviveDeadlineChanged`, `RefreshLocalReviveCountdown`, `ResolveNetworkTime`, `HandleStaminaChanged`, `HandleMoneyChanged`, `HandleModeChanged`, `HandleSeatIndexChanged`, `RefreshLocalSeatMode`, `HandleLifecycleChanged`, `SynchronizeLocalSeatedPose` | ~450 |
| 8 | Klaxon / camera siege vehicule | `HandleVehicleHornInteraction`, `EnsureVehicleHonkBridges`, `HandleHonkTargetResolved`, `TryResolveLocalSeatedVehicle`, `SuppressVehicleCamerasForLocalSeatExit`, `RestoreOnFootCamera`, `ShowVehicleSeatMessage` | ~150 |
| 9 | Utilitaires statiques | `IsNetworkSessionActive`, `IsDriverSeat`, `IsVehicleSeatMode`, `SetLocalPlayerBodyActive`, `EnsureNetworkSessionMonitor` | ~60 |

**Constat cle** : le menu d'echappement (Story 5.8) et le flow de lobby (Stories 1.x-2.x) ont deja ete extraits dans leurs propres controleurs (`RunEscapeMenuFlowController.cs`, `LobbyFlowController.cs`). `RunFlowController` ne porte *aucune* logique d'escape menu malgre ce que la formulation initiale du backlog laissait supposer — c'est un signe de discipline d'extraction deja exercee par l'equipe, pas d'un fichier qui accumule tout sans discrimination.

Tous les groupes convergent vers une seule responsabilite de plus haut niveau : **etre le point d'integration runtime du joueur local pendant un run** — spawn, etat reseau -> HUD, interactions vehicule, cible de rage. C'est un role de "composition root / adapter" assumé par Story (chaque Story y ajoute son pont), pas neuf responsabilites metier tangled.

## 2. Dependances entrantes / sortantes

`graphify query "who depends on RunFlowController"` (BFS depth=2, 481 noeuds trouves, tronque a 40 par le budget token) confirme un rayon d'impact large mais attendu pour un controleur d'integration : `NetworkedVehicleState`, `NetworkedVehicleDriverController`, `NetworkedAIVehicleState`, `LocalOnFootController`, `NetworkedPlayerState`, `RageTuningDef`, `NetworkedPassengerActionIntent`, `CharacterCatalog`, `PassengerActionCatalog`, `AIVehicleBehaviorDebugView`, `PassengerActionDebugView`, `RunCheckpointHudScreen`, `RunCompositionRoot`, plus ~20 fichiers de tests (Story12, Story15, Story16, Story26, Story42, Story43, Story46, Story55...).

Contre-verification manuelle (`using` + types de champs + appels de methode dans le fichier) : les dependances sortantes reelles sont `RoadRage.Features.{OnFoot, PassengerActions, Players, Rage, Run, UI, Vehicles}`, `RoadRage.Shared.{Domain, Input, Presentation}`, `Unity.Netcode`, `UnityEngine`, `UnityEngine.InputSystem` — coherent avec le graphe. Aucune hallucination detectee sur cette requete : tous les noeuds retournes correspondent a des symboles reels du fichier.

Dependances entrantes reelles (`rg "\bRunFlowController\b" Assets/RoadRage -g "*.cs"`, 33 fichiers) : 11 fichiers de production (voir §4) + 22 fichiers de tests. Aucun fichier de production ne depend de *methodes internes* de `RunFlowController` — les references externes portent sur le type lui-meme (recherche de composant, wiring de scene) ou sur ses membres publics (`ActiveLocalPlayer`, `TrySpawnSelectedProfile`, `TrySpawnDevVehicle`).

## 3. Analyse du churn

**Verification du chiffre "21% du churn du repo" : CONFIRME, avec la methodologie "commits touchant le fichier / commits totaux du repo".**

```
Commits totaux du repo (git log --format=%H) : 108
Commits touchant RunFlowController.cs (--follow) : 23
23 / 108 = 21,3 %  ← correspond exactement au chiffre du backlog
```

Autre methodologie testee (lignes changees, `git log --numstat`), pour contexte — plus favorable a RunFlowController :

```
RunFlowController.cs, add+del cumules sur 23 commits : 2 770 lignes
Tous les .cs du repo, add+del cumules              : 48 000 lignes
2 770 / 48 000 = 5,8 %
```

Le chiffre du backlog (21%) mesure donc la **frequence de commits touchant le fichier**, pas le volume de lignes. Les deux mesures sont coherentes entre elles (un gros fichier d'integration est touche souvent, meme si chaque touche est petite) — aucune contradiction, juste deux angles differents. Le chiffre "21%" est donc confirme et sourcable.

**Clusters de co-changement** (`git log -p` sur les 23 commits, historique de story par story) : chaque commit ajoute ou modifie un groupe de methodes appartenant a un seul cluster du §1, jamais un melange arbitraire :

- `feat(story-4-3)` : ajoute intégralement le groupe 4 (cible de rage T/Y) : `HandleRageTargetLockControls`, `RevalidateRageTargetLock`, `ApplyRageTargetLock`, `ApplyRageTargetLockMarker`, `ResolveLockRageState` ensemble.
- `feat(story-3-5)` : ajoute integralement le groupe 6 (degats vehicule) : `HandleVehicleCollided`, `ApplyCollisionConsequencesIfAuthoritative`, `ApplyNetworkedCollisionDamage`, `SubscribeToVehicleState`, `UnsubscribeFromVehicleState`, `HandleVehicleHpChanged`, `RefreshVehicleDamageHud`, `IsAuthoritativeForDamage` ensemble.
- `feat(story-4-2)` : ajoute integralement le groupe 3 (actions passager) : `UnsubscribeFromPassengerActionIntent`, `HandlePassengerActionValidated`, `EnsurePassengerActionBinding` ensemble.
- `feat(story-2-7)` (implicite, groupe 7) : `SubscribeToLocalOnFootController`, `HandleLocalStaminaChanged`, `Subscribe/UnsubscribeFromLocalNetworkedPlayerState`, `HandleHpChanged`, `HandleReviveDeadlineChanged` ajoutes ensemble.
- Les commits `fix(...)` (2026-09-10, 2026-09-12, 2026-09-16) touchent 1 a 3 methodes ciblees dans un seul groupe (ex. `ApplyNetworkedCollisionDamage` seul pour le correctif du 09-16), jamais un fix a cheval sur plusieurs groupes non lies.

**Conclusion churn** : le churn eleve vient du fait que *chaque nouvelle Story de gameplay solo/multi ajoute son pont d'integration ici* (c'est le role assume du fichier), pas d'un couplage accidentel entre responsabilites sans rapport. Les co-changements respectent les frontieres de groupes identifiees en §1.

## 4. Tests associes (sur 33 fichiers referencant RunFlowController)

**11 fichiers de production** (hors le fichier lui-meme) :
`DevVehicleSpawner.cs`, `LocalVoidRespawnController.cs`, `NetworkedPlayerLifecycleService.cs`, `NetworkedPlayerSpawnService.cs`, `NetworkedVehicleSeatIntent.cs`, `PortalTrafficSpawner.cs`, `RageRoadEventFlowController.cs`, `RunEscapeMenuFlowController.cs`, `LocalOnFootController.cs`, `NetworkedPlayerState.cs`, `NetworkedVehicleDriverController.cs`.

**22 fichiers de tests**, mappage par Story :

| Fichier | Story / sujet |
|---|---|
| Story15EmptyMapEntryTests / PlayModeTests | 1.5 — entree carte vide, spawn joueur solo |
| Story16Epic1PlayableCheckpointTests / PlayModeTests | 1.6 — checkpoint jouable Epic 1 |
| Story25NetworkedPlayerSpawnTests | 2.5 — spawn joueur reseau |
| Story26InGameHudTests | 2.6 — HUD en jeu (compteur joueurs) |
| Story27PlayerLifecycleTests | 2.7 — cycle de vie joueur (mort/revive) |
| Story28Epic2OnlinePlayableCheckpointTests | 2.8 — checkpoint jouable en ligne |
| Story33SeatEntryExitAndPassengerPresenceTests | 3.3 — entree/sortie siege |
| Story34SimpleRouteCollisionAndVehicleRecoveryTests | 3.4 — collision/recuperation vehicule |
| Story35VehicleDamageHookAndTeamWipeContractStubTests | 3.5 — degats vehicule |
| Story36Epic3DrivingPlayableCheckpointTests | 3.6 — checkpoint conduite |
| Story42PassengerActionMvpRunPlayModeTests | 4.2 — framework action passager |
| Story43PassengerActionOneChangesRageTests / MvpRunPlayModeTests | 4.3 — action passager -> rage |
| Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnTests / PlayModeTests | 4.6 — gel selection personnage |
| Story510LaneGraphAndRoutedTrafficTests | 5.10 — graphe de voies / trafic |
| Story55NetworkedAiRageTargetingTests | 5.5 — verrou de cible IA |
| Story56RageRoadEventTriggerTests | 5.6 — evenement route rage |
| Story58EscapeMenuTests / PlayModeTests | 5.8 — menu d'echappement (teste l'interaction, meme si la logique vit dans `RunEscapeMenuFlowController`) |

Chaque groupe de responsabilite du §1 a au moins un test EditMode ou PlayMode dedie. Bonne couverture story-par-story ; pas de zone du fichier identifiee comme non testee.

## 5. Comparaison avec LobbyFlowController et NetworkedAIVehicleDriverController

| | RunFlowController | LobbyFlowController | NetworkedAIVehicleDriverController |
|---|---|---|---|
| Lignes | 1522 | 970 | 918 |
| Methodes (approx.) | ~82 | 28 | 26 |
| Responsabilite | Integration runtime du joueur local pendant un run (9 sous-groupes) | Flow de lobby : creation/join, approbation connexion, roster, difficulte, statut salle | Pilotage physique/IA d'un vehicule (route, evitement, recuperation) |
| Cohesion observee | Multi-groupe mais chaque groupe = 1 pont Story, pas de melange | Un seul flux lineaire (lobby -> demarrage run) | Un seul flux (FixedUpdate -> intent -> mouvement -> recuperation) |

`LobbyFlowController` (970 lignes / 28 methodes) et `NetworkedAIVehicleDriverController` (918 lignes / 26 methodes) sont deja des fichiers volumineux pour ce codebase — pas de petits controleurs de 200 lignes. `RunFlowController` est ~1,6x plus gros en lignes et ~3x plus gros en nombre de methodes, mais c'est parce qu'il agrege ce que Lobby (1 flux) et AI Driver (1 flux) traitent chacun separement : spawn, etat reseau, degats, actions passager, cible de rage, siege/camera, HUD. Sept flux paralleles rassembles dans un seul MonoBehaviour, c'est normal pour un "run flow controller" qui doit reagir a un `NetworkedPlayerState` par joueur — ce n'est pas un outlier pathologique par rapport au style du reste du repo, c'est le plus gros exemplaire d'un pattern deja gros ailleurs.

## 6. Verdict — risque de refactor

**Pas de probleme de cohesion reel au sens "responsabilites non liees tangled".** Les preuves :

1. Chaque groupe du §1 correspond a un flux gameplay distinct mais tous rattaches au meme role : "reagir aux evenements d'un joueur local pendant le run et les propager au HUD/vehicule". Pas de responsabilite etrangere (ex. rien sur le lobby, rien sur l'IA de trafic aside from le pont degats generique).
2. Les co-changements (§3) suivent les frontieres de groupes : une Story ajoute un cluster complet, elle ne modifie pas trois clusters non lies a la fois.
3. Le fichier a deja subi une extraction disciplinee par le passe (escape menu -> `RunEscapeMenuFlowController`, event route rage -> `RageRoadEventFlowController`, spawn/lifecycle -> des services dedies `NetworkedPlayerSpawnService`/`NetworkedPlayerLifecycleService`). L'equipe casse deja le fichier quand une Story le justifie.
4. Le chiffre de churn (21%) s'explique par la frequence des touches, pas par un volume anormal de lignes changees (5,8% du churn .cs du repo) — coherent avec un role de point d'integration, pas avec un fichier fragile qu'on repare sans cesse.
5. Couverture de test complete, groupe par groupe (§4) — un refactor casserait potentiellement plus de tests qu'il n'en corrigerait de bugs latents.

**Recommandation : ne pas refactorer maintenant.** Conforme a AD-13 (devworkflow spine) : pas de besoin mesure, pas de defaut de correction repete sur un meme point, pas de bug attribuable a la taille du fichier.

**Condition de declenchement differee** (a surveiller, pas a agir maintenant) : si l'un de ces signaux apparait, rouvrir l'audit avec un objectif concret d'extraction (probablement le groupe 6 "degats/collision vehicule" ou le groupe 7 "etat joueur -> HUD", les deux plus gros et les plus autonomes en dependances) :
   - Un meme correctif (`fix(...)`) doit toucher 2 groupes non lies du §1 dans le meme commit (signe de couplage reel, pas encore observe sur les 23 commits examines).
   - Le fichier depasse ~2000 lignes (signe qu'un 10e flux gameplay a ete ajoute sans extraction).
   - Un develop signale une collision merge repetee sur ce fichier (plusieurs Stories en parallele modifiant la meme zone).

Aucune de ces conditions n'est remplie a ce jour (2026-09-17).
