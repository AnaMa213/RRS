---
title: 'Story 2.2 - Creation de room privee hote avec code de join'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
baseline_commit: 'aa303eba62b34b8cac42bae1effb1970901df87f'
---

## Intent

**Problem:** Depuis l'Epic 1, Create Lobby reste un stub qui publie toujours une notice "indisponible" : aucun lobby Steam reel n'est cree, et rien ne distingue services indisponibles, echec de creation, room ouverte ou room fermee.

**Approach:** Un service C# pur (`LobbyRoomService`) cree un lobby Steam prive de 4 joueurs max via une abstraction testable (`ISteamLobbyPlatform`/`FacepunchSteamLobbyPlatform`), porte par le singleton persistant `RoadRageBootstrap`, declenche par le meme bouton Create Lobby / Close Room dans `LobbyFlowController`, avec code de join et etat visibles dans `LobbyShellScreen`. En verifiant le comportement en conditions reelles (session Steam active), un bug bloquant preexistant a ete decouvert et corrige : rien ne pompait `SteamClient.RunCallbacks()`, ce qui bloquait indefiniment toute operation Steamworks asynchrone.

## Suggested Review Order

**Etat et transitions de la room**

- Point d'entree : creation avec garde anti-double-creation et verification des services en ligne avant tout appel SDK.
  [`LobbyRoomService.cs:48`](../../Assets/RoadRage/Features/Online/LobbyRoomService.cs#L48)

- Fermeture explicite par l'hote, sans effet si aucune room n'est ouverte.
  [`LobbyRoomService.cs:85`](../../Assets/RoadRage/Features/Online/LobbyRoomService.cs#L85)

**Pompage des callbacks Steamworks (bug bloquant decouvert et corrige en conditions reelles)**

- `SteamClient.Init` est appele sans thread de callbacks automatique : `Tick()` pompe les callbacks a chaque frame, uniquement une fois Online.
  [`OnlineServicesBootstrapService.cs:77`](../../Assets/RoadRage/Features/Online/OnlineServicesBootstrapService.cs#L77)

- Isolation SDK : seule methode du projet a appeler `SteamClient.RunCallbacks()` directement.
  [`FacepunchSteamPlatform.cs:32`](../../Assets/RoadRage/Features/Online/FacepunchSteamPlatform.cs#L32)

- Declenchement reel : le proprietaire persistant pompe a chaque Update Unity, sans quoi toute creation de lobby restait bloquee sur `Creating`.
  [`RoadRageBootstrap.cs:91`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L91)

**Isolation SDK pour la creation de lobby**

- Seule classe du projet a toucher `Steamworks.SteamMatchmaking` directement ; lobby prive par defaut du SDK, Networking Sockets, aucune ouverture de port cote hote.
  [`FacepunchSteamLobbyPlatform.cs:18`](../../Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs#L18)

- Abstraction que `LobbyRoomService` consomme, testable sans client Steam installe.
  [`ISteamLobbyPlatform.cs:10`](../../Assets/RoadRage/Features/Online/ISteamLobbyPlatform.cs#L10)

**Cablage App : meme bouton pour creer/fermer, retour visible**

- Un seul bouton bascule creation/fermeture selon l'etat courant de la room.
  [`LobbyFlowController.cs:127`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L127)

- Traduction de chaque etat de room en libelle d'ecran et notice visible.
  [`LobbyFlowController.cs:200`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L200)

**UI : code affiche, libelle du bouton**

- Room ouverte : code de join affiche, libelle du bouton passe a "Close Room".
  [`LobbyShellScreen.cs:144`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L144)

- Room fermee : etat initial et etat de repli sur echec.
  [`LobbyShellScreen.cs:161`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L161)

**Tests**

- Etats de `LobbyRoomService` via une fausse plateforme de lobby (aucun client Steam requis) : succes, services indisponibles, echec de creation, double-clic, fermeture.
  [`Story22HostCreatedPrivateRoomTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story22HostCreatedPrivateRoomTests.cs#L1)

- Verification sur le vrai cycle de vie Unity, tolerante a l'environnement Steam de la machine : le clic doit toujours atteindre un etat terminal, jamais rester bloque sur Creating.
  [`Story22HostCreatedPrivateRoomPlayModeTests.cs:1`](../../Assets/RoadRage/Tests/PlayMode/Story22HostCreatedPrivateRoomPlayModeTests.cs#L1)
