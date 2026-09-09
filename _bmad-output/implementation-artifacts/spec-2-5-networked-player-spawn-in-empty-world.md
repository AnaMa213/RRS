---
title: 'Story 2.5 - Spawn reseau des joueurs dans le monde vide'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
baseline_commit: '35632df1c02b0e14d6cd9293e58d6e1bdcf9060b'
---

# Story 2.5 - Spawn reseau des joueurs dans le monde vide

## Intent

**Problem:** Depuis la Story 2.4, Start Game chargeait `MVP_Run` sans demarrer Netcode ; apres le premier correctif 2.5, les joueurs avaient un etat reseau mais pas encore de presence visible fiable, et un client pouvait rester bloque en run si l'hote quittait.

**Approach:** Story 2.5 couvre maintenant le minimum jouable de session en ligne vide : StartHost/StartClient reels, spawn host-owned d'un `NetworkedPlayerRoot` par client, avatar greybox visible pour les autres joueurs, pose locale relayee au host puis repliquee en lecture client, et retour automatique au `MainMenuLobby` avec erreur visible quand le client perd l'hote. Le jeu solo local reste inchange, sans Netcode.

## Boundaries & Constraints

**Always:** garder les NetworkObjects gameplay host-owned et les NetworkVariables server-write ; les clients ne soumettent que leur pose locale via RPC valide par le host ; la camera et l'input restent locaux ; les prefabs visuels reutilisent les `CharacterDef.PreviewPrefab` existants.

**Ask First:** remplacer les prefabs greybox, ajouter host migration, adopter un autre transport, ou transformer le mouvement on-foot local en controle reseau complet.

**Never:** ne pas synchroniser la camera ; ne pas donner l'autorite gameplay aux clients ; ne pas ajouter de conduite, de siege, d'action passager, de mort/revive complet ou de restart de run dans cette story.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Host starts online run | Room hote ouverte, roster pret, profil valide | `StartHost` demarre, `MVP_Run` charge via scene Netcode, le host spawne un `NetworkedPlayerRoot` pour chaque client connecte | Echec StartHost/SceneManager publie `NetworkStartFailedMessage` |
| Client starts joined run | Room rejointe, owner SteamId connu, profil valide | `StartClient` cible l'hote, recoit `MVP_Run`, et obtient son `NetworkedPlayerRoot` | Transport/owner manquant publie `NetworkStartFailedMessage` |
| Remote player visible | `NetworkedPlayerState` contient client id, personnage et pose | Chaque client cache son propre proxy reseau local et voit les autres joueurs avec le greybox du personnage, colliders visuels desactives | Personnage absent replie sur le premier catalogue et logge un avertissement |
| Local pose changes | Le joueur local bouge dans `MVP_Run` en session reseau | Un reporter local envoie position/yaw au proxy correspondant ; le host valide le client emetteur et replique la pose aux autres | Pose ignoree si aucun proxy ne correspond au `LocalClientId` |
| Host lost | Client pur dans `MVP_Run`, hote quitte ou session perdue | Le client shutdown Netcode, retourne a `MainMenuLobby`, et voit une notice d'erreur host perdu | Host migration explicitement differee |

## Suggested Review Order

**Demarrage reseau cote lobby**

- Point d'entree : bascule host/client/solo, ne demarre le reseau que si une room est ouverte ou rejointe.
  [`LobbyFlowController.cs:311`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L311)

- StartHost/StartClient reels, ConnectionData, SteamId cible cote invite.
  [`LobbyFlowController.cs:349`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L349)

- Decodage du profil de connexion et enregistrement host-only, toujours approuve.
  [`LobbyFlowController.cs:421`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L421)

**NetworkManager paresseux**

- Construction en code du NetworkManager + transport Facepunch, jamais au bootstrap (evite Steamworks en arriere-plan hors session).
  [`RoadRageBootstrap.cs:118`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L118)

**Spawn reseau host-only dans MVP_Run**

- Balayage initial (hote + clients deja connectes) puis arrivees tardives.
  [`NetworkedPlayerSpawnService.cs:50`](../../Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs#L50)

- Resolution profil/personnage avec repli et retour visible+logge sur doublon/echec.
  [`NetworkedPlayerSpawnService.cs:83`](../../Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs#L83)

- Spawn host-owned explicite (`Spawn()` sans clientId) : jamais `SpawnAsPlayerObject`.
  [`NetworkedPlayerSpawnService.cs:134`](../../Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs#L134)

**Presence visible et pose minimale**

- `NetworkedPlayerState` doit porter `ClientId`, `CharacterId`, `WorldPosition` et `YawDegrees`, tous ecrits par le serveur.
  [`NetworkedPlayerState.cs`](../../Assets/RoadRage/Features/Players/NetworkedPlayerState.cs)

- Un composant de presentation sur `NetworkedPlayerRoot` instancie le prefab greybox du personnage pour les joueurs distants et applique la pose repliquee.
  [`NetworkedPlayerPresentation.cs`](../../Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs)

- Le joueur local conserve son controller/camera local-only, mais ajoute un reporter de pose uniquement en session reseau.
  [`RunFlowController.cs`](../../Assets/RoadRage/App/Run/RunFlowController.cs)

**Perte hote**

- Un moniteur de session dans `MVP_Run` ecoute les deconnexions Netcode et renvoie les clients purs vers `MainMenuLobby` si le serveur disparait.
  [`NetworkedRunSessionMonitor.cs`](../../Assets/RoadRage/App/Run/NetworkedRunSessionMonitor.cs)

- `MainMenuFlowController` rejoue la derniere notice persistante a l'ouverture pour que l'erreur de perte hote reste visible apres chargement de scene.
  [`MainMenuFlowController.cs`](../../Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs)

**Etat reseau et payload**

- `CharacterId` server-write ajoute a l'etat host-owned existant.
  [`NetworkedPlayerState.cs:12`](../../Assets/RoadRage/Features/Players/NetworkedPlayerState.cs#L12)

- Encodage/decodage du profil transporte par NetworkConfig.ConnectionData, sans dependance Netcode.
  [`NetworkPlayerConnectionPayload.cs:1`](../../Assets/RoadRage/Features/Players/NetworkPlayerConnectionPayload.cs#L1)

**Tests**

- Roundtrip payload et depot ClientId -> profil, sans client Steam ni Netcode.
  [`Story25NetworkedPlayerSpawnTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story25NetworkedPlayerSpawnTests.cs#L1)

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Players/NetworkedPlayerState.cs` -- ajouter l'identite client et la pose minimale server-write -- permettre presentation et validation RPC.
- [x] `Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs` -- creer la presentation reseau distante et l'endpoint RPC de pose -- rendre les joueurs visibles sans donner l'autorite gameplay au client.
- [x] `Assets/RoadRage/App/Run/NetworkedLocalPlayerPoseReporter.cs` et `RunFlowController.cs` -- brancher la pose du joueur local vers son proxy reseau -- synchroniser position/yaw avec les autres clients.
- [x] `Assets/RoadRage/App/Run/NetworkedRunSessionMonitor.cs` et `MainMenuFlowController.cs` -- detecter la perte hote et rejouer l'erreur visible au retour lobby -- respecter NFR12 dans la Story 2.5.
- [x] `Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab` -- ajouter la presentation reseau avec le `CharacterCatalog` assigne -- garantir le rendu runtime sur host et clients.
- [x] `Assets/RoadRage/Tests/EditMode/Story25NetworkedPlayerSpawnTests.cs` -- couvrir prefab visible, pose, RPC non-owner et host quit -- verrouiller les regressions constatees.

**Acceptance Criteria:**
- Given une session reseau dans `MVP_Run`, when deux joueurs sont connectes, then chaque machine voit le greybox de l'autre joueur et ne voit pas un doublon de son propre proxy reseau.
- Given le joueur local bouge, when son reporter trouve le proxy `ClientId == LocalClientId`, then seule cette pose est acceptee par le host et repliquee aux autres.
- Given un client pur perd le serveur, when Netcode notifie la deconnexion de `ServerClientId` ou du client local, then le client shutdown le reseau, revient a `MainMenuLobby`, et affiche une erreur visible.
- Given une partie solo locale, when `MVP_Run` charge sans `NetworkManager` en ecoute, then le spawn local Story 1.5 reste inchange et aucun reporter/monitor reseau actif ne casse le run.

## Spec Change Log

- 2026-09-09 -- Recalibrage demande par Kenan : la limite "pas de visuel/position et pas de host quit" sort des limitations connues et entre dans la Story 2.5, tout en gardant host authority, input/camera local-only et host migration hors scope.

## Verification

**Commands:**
- Tests EditMode Story 2.5 via Unity MCP -- attendu : compilation OK, tests cibles verts, console sans erreurs Netcode/Facepunch.
- `git diff --check` -- attendu : aucune erreur whitespace.
