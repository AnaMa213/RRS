---
title: 'Story 2.5 - Spawn reseau des joueurs dans le monde vide'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
---

# Story 2.5 - Spawn reseau des joueurs dans le monde vide

## Intent

**Problem:** Depuis la Story 2.4, Start Game se contentait de charger localement `MVP_Run` (`SceneManager.LoadScene`), sans jamais demarrer Netcode : aucun joueur connecte ne recevait d'objet reseau, et la session en ligne restait une coquille sans etat partage.

**Approach:** `LobbyFlowController.HandleStartGameRequested` demarre desormais reellement la session Steamworks Networking Sockets quand une room hote est ouverte ou rejointe (StartHost + `NetworkManager.SceneManager.LoadScene` pour un chargement synchronise cote hote, StartClient avec le SteamId de l'hote cote invite), en publiant le profil local (nom + personnage) comme donnees de connexion NGO decodees par un callback d'approbation. Le `NetworkManager` + transport Facepunch sont construits en code, de facon paresseuse (jamais au bootstrap, uniquement juste avant StartHost/StartClient) pour ne jamais faire tourner Steamworks en arriere-plan hors session reseau. Dans `MVP_Run`, un nouveau `NetworkedPlayerSpawnService` host-only spawn un `NetworkedPlayerRoot` (host-owned, `NetworkedPlayerState` server-write) par client connecte -- balayage initial des clients deja connectes puis `OnClientConnectedCallback` pour les arrivees tardives -- avec repli sur le premier personnage du catalogue et retour logge + visible (HUD) en cas de profil manquant, personnage introuvable ou doublon de spawn. Le jeu solo local (Story 1.5, sans room) reste inchange, sans Netcode.

**Limite connue, non couverte par cette story :** aucune replication visuelle/position des avatars entre joueurs (pas de `NetworkTransform`) -- chaque client ne voit que son propre avatar local (`RunFlowController`, inchange) ; seul le `NetworkedPlayerState` host-owned est reellement reseau. Base suffisante pour les Stories 2.6 (HUD) et 2.7 (cycle de vie), qui ne lisent que cet etat. La verification multijoueur reelle (deux clients Steam distants) reste manuelle, prevue au checkpoint Story 2.8.

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

**Etat reseau et payload**

- `CharacterId` server-write ajoute a l'etat host-owned existant.
  [`NetworkedPlayerState.cs:12`](../../Assets/RoadRage/Features/Players/NetworkedPlayerState.cs#L12)

- Encodage/decodage du profil transporte par NetworkConfig.ConnectionData, sans dependance Netcode.
  [`NetworkPlayerConnectionPayload.cs:1`](../../Assets/RoadRage/Features/Players/NetworkPlayerConnectionPayload.cs#L1)

**Tests**

- Roundtrip payload et depot ClientId -> profil, sans client Steam ni Netcode.
  [`Story25NetworkedPlayerSpawnTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story25NetworkedPlayerSpawnTests.cs#L1)
