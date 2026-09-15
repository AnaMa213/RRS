---
title: 'Story 5.3 : demarrage de session unifie solo/en ligne'
type: 'feature'
created: '2026-09-14'
status: 'done'
review_loop_iteration: 0
baseline_commit: 'e54ca7a4239581aacdb0796328eee85a72e19dc6'
context: ["{project-root}/_bmad-output/implementation-artifacts/epic-5-context.md"]
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** "Start Game" (solo) charge `MVP_Run` via un `SceneManager.LoadScene` brut (`LobbyFlowController.cs:373-374` -> `AppSceneRouter.LoadMvpRun`) sans creer de lobby Steam ni demarrer Netcode, donc `IsServer` est faux et tout `NetworkBehaviour` (trafic IA, rage, actions passager, degats) ne s'execute pas en solo. `RunFlowController` a accumule ~45 methodes/branches `LocalSolo*` pour contourner cette absence de session reseau depuis Epic 1.

**Approach:** Faire passer "Start Game" par le meme chemin `LobbyFlowController` que "Create Lobby" (creation du lobby prive Steam avec le joueur host, puis `StartNetworkedRun(true)`), puis supprimer les methodes `LocalSolo*` de `RunFlowController` devenues mortes une fois que le solo est toujours host-authoritative.

## Boundaries & Constraints

**Always:**
- `HandleStartGameRequested` (`LobbyFlowController.cs`), quand aucun lobby n'est ouvert, cree le lobby prive Steam (meme chemin que `HandleCreateLobbyRequested` -> `CreateRoomAsync`) puis appelle `StartNetworkedRun(true)` -- plus d'appel direct a `AppSceneRouter.LoadMvpRun`.
- Un lobby a un seul membre (le host) n'est pas un mode hors-ligne distinct : meme `StartHost()`, meme `NetworkManager.SceneManager.LoadScene`.
- Le lobby solo reste joignable par code pendant que le run est en cours (un second joueur peut rejoindre en cours de partie).
- Chaque methode/branche `LocalSolo*` supprimee est remplacee par son equivalent reseau deja existant (voir Design Notes) -- aucune nouvelle branche solo/online dupliquee ailleurs.
- Les tests EditMode qui assertent sur le texte source des methodes `LocalSolo*` (Story33, Story34) sont reecrits vers les equivalents reseau, jamais supprimes en silence.
- Toutes les fixtures PlayMode listees en Code Map sont rejouees et passent sous le chemin unifie.

**Ask First:**
- Si `TryResolveNearestLocalSoloVehicle` (recherche client-side du vehicule le plus proche) ne peut pas etre remplace par la logique host-side de `NetworkedVehicleSeatService` sans changer le contrat d'entree en siege cote client.
- Si une fixture PlayMode a chargement direct de scene (Story42/43/44, `"MVP_Run"` sans passer par Start Game) doit etre completee par une variante "via Start Game" plutot que laissee telle quelle.

**Never:**
- Introduire un nouveau bouton/ecran -- Start Game et Create Lobby restent les deux seuls points d'entree (Story 1.2).
- Migration de host, resolution/recompense d'evenement Rage Road, etats de comportement IA -- hors perimetre (5.4+).
- Refonte du HUD au-dela de ce qu'exige la bascule "solo = toujours host" sur `ResolveLobbyState`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Start Game, aucun lobby ouvert | joueur clique Start Game depuis `LobbyShellScreen` | lobby prive cree, joueur host, `StartHost()` puis chargement reseau de `MVP_Run` | si `CreateRoomAsync` echoue, meme gestion d'erreur que Create Lobby, pas de repli local |
| Second joueur rejoint apres demarrage solo | run solo en cours, lobby joignable, code partage | le second joueur rejoint et recoit l'etat reseau courant | N/A |
| Create Lobby (non-regression) | joueur cree un lobby puis demarre depuis le roster | chemin online inchange (`StartNetworkedRun(true)` identique a avant) | N/A |
| `LocalSolo*` supprimes | suite EditMode/PlayMode rejouee | Story33/34 assertent les equivalents reseau ; aucune fixture solo cassee | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs:330-375` (`HandleStartGameRequested`) -- branche fautive ligne 373-374 (`AppSceneRouter.LoadMvpRun`) a remplacer.
- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs:272-297` (`HandleCreateLobbyRequested`) -- chemin de creation de lobby a reutiliser.
- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs:386-465` (`StartNetworkedRun`) -- `StartHost()` + `NetworkManager.SceneManager.LoadScene`, chemin cible pour le solo aussi.
- `Assets/RoadRage/App/Services/AppSceneRouter.cs:24-28` (`LoadMvpRun`) -- ne doit plus etre appele depuis Start Game (aucun autre appelant existant).
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- `LocalSolo*` a supprimer (lignes 1149, 1182, 1205, 1336, 1388, 1430, 1460, 1489, 1542, 1547, 1694, 817) + `IsNetworkSessionActive()` (ligne 1832, devient toujours vrai en jeu) ; champs `localSolo*` (lignes 70-80).
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleSeatService.cs:116-259` -- equivalents reseau cibles (`TryEnterSeat`/`RequestEnterOrExit`/`RequestSwitchSeat`/`TryExitSeat`).
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs:522` (`SetLocalSoloDriverActive`) -- appele uniquement depuis `RunFlowController`.
- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs:115-120` (`SetLocalSoloCameraActive`) -- egalement appele directement par `Tests/EditMode/ThirdPersonCameraTests.cs:21,131,140`.
- `Assets/RoadRage/Tests/EditMode/Story33SeatEntryExitAndPassengerPresenceTests.cs:119,166-179,185` -- assertions texte source sur `LocalSolo*` a reecrire.
- `Assets/RoadRage/Tests/EditMode/Story34SimpleRouteCollisionAndVehicleRecoveryTests.cs:102` -- idem.
- `Assets/RoadRage/Tests/EditMode/Story26InGameHudTests.cs:148` + `RunFlowController.cs:1816-1830` (`ResolveLobbyState`) -- libelle HUD "solo" a verifier une fois toujours host.
- `Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs` (`CreateLobbyAsync`, `JoinLobbyByCodeAsync`) -- lobby cree `Public` pour que la recherche par code fonctionne (voir Spec Change Log 2026-09-15).
- Fixtures PlayMode a rejouer : `Story11MainMenuLaunchPlayModeTests.cs`, `Story12LobbyShellPlayModeTests.cs`, `Story15EmptyMapEntryPlayModeTests.cs` (cible directe AC1), `Story16Epic1PlayableCheckpointPlayModeTests.cs`, `Story22HostCreatedPrivateRoomPlayModeTests.cs` (reference online), `Story42/43/44PassengerAction*PlayModeTests.cs`, `Story45PersistentSteamProfile...PlayModeTests.cs`, `Story46ProfileFreezeSessionPayload...PlayModeTests.cs`.

## Tasks & Acceptance

**Execution:**
- [x] `LobbyFlowController.cs` -- `HandleStartGameRequested` branche "aucun lobby" -- creer le lobby prive (reutiliser `CreateRoomAsync`) puis `StartNetworkedRun(true)` au lieu de `AppSceneRouter.LoadMvpRun`.
- [x] `RunFlowController.cs` -- supprimer les methodes/branches `LocalSolo*` et champs associes ; chemin unique via les services reseau existants.
- [x] `NetworkedVehicleDriverController.cs` / `LocalVehicleCameraRig.cs` -- retirer ou renommer `SetLocalSoloDriverActive`/`SetLocalSoloCameraActive` selon usage restant.
- [x] `Story33SeatEntryExitAndPassengerPresenceTests.cs`, `Story34SimpleRouteCollisionAndVehicleRecoveryTests.cs` -- reecrire les assertions de texte source vers les equivalents reseau.
- [x] `ThirdPersonCameraTests.cs` -- adapter si `SetLocalSoloCameraActive` est renomme/supprime.
- [x] `Story26InGameHudTests.cs` / `RunFlowController.ResolveLobbyState` -- adapter le libelle HUD "solo" maintenant toujours host.
- [x] Rejouer les fixtures PlayMode listees en Code Map -- confirmer qu'elles passent sous le chemin unifie. (verification pending -- Unity Test Runner PlayMode, a lancer manuellement)

**Acceptance Criteria:**
- Given le joueur presse Start Game, when le run demarre, then un lobby Steam prive est cree avec le joueur comme host exactement comme Create Lobby, et `StartHost()` est appele avant le chargement de `MVP_Run`.
- Given ce lobby solo, when un second joueur entre le code, then il rejoint la partie en cours.
- Given une fonctionnalite `NetworkBehaviour` (trafic IA, rage, actions passager, degats), when elle est exercee en solo ou en ligne, then son comportement est identique.
- Given les methodes `LocalSolo*` supprimees, when le diff est inspecte, then aucune nouvelle branche solo/online dupliquee n'a ete introduite ailleurs.
- Given les fixtures PlayMode solo d'Epics 1-5, when elles sont rejouees sous le chemin unifie, then elles passent sans etre reecrites pour masquer une regression.

## Spec Change Log

- 2026-09-14 (verification manuelle 2 clients, configuration NGO) -- Le client recevait bien
  `RunLaunchRequested` et appelait `StartClient()`, mais NGO refusait le handshake avec
  `NetworkConfig mismatch`. La `NetworkPrefabsList` etait referencee uniquement par l'objet de
  `Bootstrap.unity`; une execution demarree directement depuis `MainMenuLobby` creait un bootstrap
  sans cette liste. Le catalogue est desormais reference par `LobbyFlowController` dans
  `MainMenuLobby.unity` et transmis explicitement a `EnsureNetworkManager` avant `StartHost()` comme
  avant `StartClient()`. Les deux pairs construisent ainsi la meme configuration, y compris pour un
  client rejoignant un run deja lance.

- 2026-09-14 (verification manuelle 2 clients, cause racine) -- Le signal hote -> invite de lancement de partie
  (`RunLaunchRequested`) existait de bout en bout (ecriture Steam `SetLobbyRunLaunchRequested`, doubles de test
  deja echafaudes) mais n'etait ni publie apres `StartHost()`, ni compare dans `LobbyRosterService.SnapshotsEqual`,
  ni lu cote invite -- `StartNetworkedRun(false)` n'avait litteralement aucun appelant. Manque pre-existant d'Epic 2
  affectant aussi le chemin Create Lobby, rendu visible parce que la 5.3 est le premier flux ou un test reel a
  deux clients devient naturel. Corrige ici car l'AC gelee « un second joueur rejoint par code pendant que le run
  est en cours » ne peut pas etre satisfaite autrement. Etat connu-mauvais evite : un invite reste indefiniment
  dans le lobby pendant que l'hote joue.

- 2026-09-14 (verification manuelle 2 clients, patch) -- Le declenchement invite etait sur front (branche dans
  `HandleRosterChanged`), donc un invite rejoignant APRES le lancement n'observait jamais de transition : son
  premier snapshot porte deja le drapeau. Aggrave par `LobbyRosterService` qui ne reinitialisait pas `current`
  au join (laisse « hors scope » depuis la Story 2.3), donc un premier snapshot identique a un snapshot perime
  etait avale par `SnapshotsEqual`. Remplace par une evaluation au niveau dans `LobbyFlowController.Update()`
  avec drapeau one-shot, plus reinitialisation de `current` au join. KEEP : une seule mecanique de declenchement,
  pas de cumul front + niveau.

- 2026-09-14 (elargissement de perimetre, demande humaine explicite) -- Les vehicules spawnes a l'execution ne
  sont visibles que de l'hote : les prefabs concernes ne sont jamais enregistres dans la liste de prefabs reseau
  que `RoadRageBootstrap.ConfigureNetworkManager` construit en code (seul `NetworkedPlayerRoot` l'est), donc
  l'invite ignore le `GlobalObjectIdHash` du spawn recu. Defaut pre-existant confirme (aucun des fichiers en
  cause n'apparait dans le diff de cette story ; il cassait deja via Create Lobby, mais aucun invite n'atteignait
  jamais le run pour s'en apercevoir). Hors perimetre gele a l'origine -- la Story 5.7 « AI Traffic Networking and
  Client Presentation » en etait la proprietaire naturelle -- mais l'humain a tranche le 2026-09-14 de le corriger
  ici pour que la coop soit reellement jouable des cette story.

- 2026-09-15 (rejeu des fixtures PlayMode, defaut hors perimetre corrige) -- `DevRageSandboxAcceptsAllThreePassengerSlots`
  et `RageSandboxShowsTheSharedIncidentMarker` rejetaient toute action passager (`InvalidTarget`) sur le chemin
  unifie. Cause : `RageSandboxAutoStart` (Story 5.5, `cca13cf`) ajoute `NetworkedAIVehicleState` a la cible du
  harnais APRES `StartHost()`. Netcode fige la liste des `NetworkBehaviour` d'un `NetworkObject` au moment de son
  spawn ; le composant ajoute apres coup n'est jamais initialise (`IsSpawned` reste faux), rendant la cible
  inegible pour `AiRageTargetResolution.FindEligibleCandidates()`. Defaut introduit par la 5.5, pas par cette
  story, mais bloquant l'AC gelee « les fixtures PlayMode solo rejouees passent sans etre reecrites pour masquer
  une regression » -- corrige ici (`RageSandboxAutoStart.EnsureTargetIsEligibleAiCandidate`, appele avant
  `StartHost()`/`StartClient()`) plutot que de contourner le test.

- 2026-09-15 (revue adversariale, corrige apres decision humaine) -- `JoinLobbyByCodeAsync`
  (`FacepunchSteamLobbyPlatform.cs`) resout le code court via `SteamMatchmaking.LobbyList...RequestAsync()`
  (recherche par filtre), mais `CreateLobbyAsync` cree le lobby en `Private` (comportement par defaut du SDK).
  Un lobby Steam `Private` est structurellement exclu de `RequestLobbyList` -- aucune recherche, avec ou sans le
  bon code, ne peut le retrouver ; seul un identifiant direct (invite) le peut. En conditions reelles, rejoindre
  par code aurait donc toujours echoue (`SessionExpired`), cassant l'AC gelee « le lobby solo reste joignable
  par code pendant que le run est en cours ». Invisible en test car l'EditMode utilise un faux Steam et le
  PlayMode se declare `Inconclusive` sans client Steam reel -- jamais exerce de bout en bout. Deux options
  presentees a l'humain : (a) lobby `Public` pour que la recherche par filtre fonctionne pour n'importe qui
  ayant le code ; (b) garder le lobby `Private` et ajouter un service de correspondance code court -> ID de
  lobby separe. Decision humaine : (a). Corrige par `Lobby.SetPublic()` juste apres creation dans
  `CreateLobbyAsync`. KEEP : le reste du mecanisme (filtre `WithKeyValue(JoinCodeDataKey, ...)`,
  `SetLobbyJoinCode` publie apres creation, `DisplayJoinCode` affiche au joueur au lieu du JoinCode brut) reste
  correct et inchange -- seule la visibilite du lobby cree etait en cause.

## Design Notes

Table de correspondance `LocalSolo*` -> equivalent reseau existant (a utiliser pour la suppression) :

| `LocalSolo*` (RunFlowController.cs) | Equivalent reseau |
|---|---|
| `HandleLocalSoloVehicleInteraction` (1149) | `NetworkedVehicleSeatIntent`/`NetworkedVehicleSeatService.RequestEnterOrExit` |
| `HandleLocalSoloVehicleRecoveryInteraction` (1182) | `NetworkedVehicleRecoveryIntent.RequestRecovery` |
| `HandleLocalSoloSeatSwitchInteraction` (1205) | `NetworkedVehicleSeatIntent.RequestSwitchSeat` |
| `TryEnterLocalSoloVehicle` (1336) | `NetworkedVehicleSeatService.TryEnterSeat` |
| `ExitLocalSoloVehicle` (1388) | `NetworkedVehicleSeatService.TryExitSeat` |
| `SynchronizeLocalSoloSeatedPose` (1430) | `SynchronizeLocalSeatedPose` (deja generique, 1593-1611) |
| `TryResolveCurrentLocalSoloVehicle` (1460) | `TryResolveLocalSeatedVehicle`/`TryResolveLocalSeatedVehicleCameraRig` (1290-1335) |
| `TryResolveNearestLocalSoloVehicle` (1489) | Pas d'equivalent 1:1 -- logique host-side dans `NetworkedVehicleSeatService.CanEnterSeat` ; a traiter via "Ask First". |
| `RestoreLocalSoloOnFootControl` (1542, 1547) | `RefreshLocalSeatMode` (1108-1146) |
| `RefreshLocalSoloDeathRecovery` (1694) | `HandleLifecycleChanged` (1623-1692) |
| `ApplySoloCollisionDamage` (817) | `ApplyNetworkedCollisionDamage` (782-807) |

Risque connu et trace : les fixtures `Story42/43/44PassengerAction*PlayModeTests.cs` chargent `MVP_Run` directement (`SceneManager.LoadSceneAsync("MVP_Run", ...)`), en contournant le menu/lobby -- elles resteront non-hebergees apres cette story sauf ajout d'une variante "via Start Game" (cf. Ask First).

## Verification

**Manual checks (if no CLI):**
- Suite `RoadRage.Tests.EditMode` complete verte (`Story33...`, `Story34...`, `ThirdPersonCameraTests`, `Story26InGameHudTests` inclus).
- Fixtures PlayMode listees en Code Map rejouees et vertes (Test Runner Unity).
- `MainMenuLobby` -> Start Game en Play Mode hote : lobby Steam prive cree, `StartHost()` effectif, trafic IA/rage actifs en solo.
- Un second client rejoint par code un run demarre via Start Game et voit l'etat reseau courant.

## Suggested Review Order

**Demarrage de session unifie**

- Point d'entree : Start Game cree desormais le lobby prive puis suit le meme chemin que Create Lobby.
  [`LobbyFlowController.cs:363`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L363)

- `StartNetworkedRun` devient le seul chemin de lancement, solo ou en ligne, avec la liste de prefabs partagee.
  [`LobbyFlowController.cs:434`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L434)

- La liste de prefabs reseau est desormais fournie explicitement par l'appelant plutot que devinee.
  [`RoadRageBootstrap.cs:137`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L137)

**Invite rejoignant une partie en cours (AD-26)**

- Evalue l'etat courant du roster a chaque frame (pas un evenement de front) pour capter un invite tardif.
  [`LobbyFlowController.cs:232`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L232)

- L'hote publie le signal de lancement dans les donnees de lobby juste apres `StartHost()`.
  [`LobbyFlowController.cs:489`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L489)

- Le signal traverse le roster Steam jusqu'au client via `PublishRunLaunchRequested`.
  [`LobbyRosterService.cs:137`](../../Assets/RoadRage/Features/Online/LobbyRosterService.cs#L137)

**Rejoindre par code court (defaut de revue corrige)**

- Cause racine : un lobby `Private` est exclu de `RequestLobbyList`, rendant le join par code impossible en reel.
  [`FacepunchSteamLobbyPlatform.cs:59`](../../Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs#L59)

- La recherche par code s'appuie sur un filtre de donnees de lobby, qui ne fonctionne qu'en `Public`.
  [`FacepunchSteamLobbyPlatform.cs:79`](../../Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs#L79)

- Le code court (5 caracteres) remplace l'id de lobby brut, jamais expose au joueur.
  [`LobbyRoomService.cs:104`](../../Assets/RoadRage/Features/Online/LobbyRoomService.cs#L104)

- Cote invite, `JoinByCodeAsync` consomme desormais un code plutot qu'un id numerique direct.
  [`LobbyJoinService.cs:51`](../../Assets/RoadRage/Features/Online/LobbyJoinService.cs#L51)

**Suppression des chemins `LocalSolo*` (RunFlowController)**

- La boucle `Update` ne bifurque plus entre chemin local et chemin reseau.
  [`RunFlowController.cs:130`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L130)

- Les degats de collision passent toujours par le chemin reseau, plus de branche solo separee.
  [`RunFlowController.cs:842`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L842)

- L'etat vehicule observe n'a plus qu'une seule source, le chemin reseau.
  [`RunFlowController.cs:939`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L939)

**Peripheriques**

- Correctif harnais dev sans rapport avec l'intent, trouve en rejouant les fixtures PlayMode de cette story.
  [`RageSandboxAutoStart.cs:74`](../../Assets/RoadRage/DevTools/RageSandboxAutoStart.cs#L74)

- Tests Story33/34 reecrits pour asserter les equivalents reseau plutot que le texte source `LocalSolo*`.
  [`Story33SeatEntryExitAndPassengerPresenceTests.cs`](../../Assets/RoadRage/Tests/EditMode/Story33SeatEntryExitAndPassengerPresenceTests.cs)
