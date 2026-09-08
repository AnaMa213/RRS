---
title: 'Story 2.3 - Join par code Steam avec wrapper invite-link'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
baseline_commit: '6a8e1a025402ab79ab7d53ec8ff0d2835b4e0103'
---

## Intent

**Problem:** Depuis l'Epic 1, Join By Code reste un stub qui publie toujours une notice "indisponible" : aucun join Steam reel n'est tente, et rien ne distingue code invalide, room pleine, session expiree ou echec de service.

**Approach:** Un service C# pur (`LobbyJoinService`) valide/normalise le code saisi puis rejoint un lobby Steam existant via `ISteamLobbyPlatform.JoinLobbyAsync`, implemente par `FacepunchSteamLobbyPlatform` avec la vraie API Facepunch (`Lobby.Join()` -> `RoomEnter`, verifiee par reflexion sur le DLL). Le champ de saisie (`LobbyShellScreen.joinCodeInputField`, nouveau TMP_InputField dans `MainMenuLobby.unity`) et `LobbyFlowController` cablent le tout avec un retour visible distinct par cause d'echec. Une revue a la volee a releve un vrai defaut (le meme `FacepunchSteamLobbyPlatform.currentLobby` etait ecrase silencieusement entre creation et join, permettant a un joueur d'etre hote et invite en meme temps) : corrige par des gardes explicites cote `LobbyJoinService` (refus si deja rejoint) et `LobbyFlowController` (refus croise creation/join).

## Suggested Review Order

**Coeur metier : validation et etats du join**

- Point d'entree : normalisation du code (trim, purement numerique) rejetee avant tout appel reseau ; garde anti double-clic et anti double-join (ajoutee suite a revue).
  [`LobbyJoinService.cs:50`](../../Assets/RoadRage/Features/Online/LobbyJoinService.cs#L50)

- Normalisation stricte du code de join.
  [`LobbyJoinService.cs:108`](../../Assets/RoadRage/Features/Online/LobbyJoinService.cs#L108)

**Isolation SDK pour le join de lobby**

- Seule methode du projet a appeler `Lobby.Join()` : traduit la reponse Steamworks (`RoomEnter`) en raison neutre vis-a-vis du SDK.
  [`FacepunchSteamLobbyPlatform.cs:32`](../../Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs#L32)

- Mapping `RoomEnter` -> `LobbyJoinFailureReason` (Full/DoesntExist distingues, reste regroupe en echec generique).
  [`FacepunchSteamLobbyPlatform.cs:57`](../../Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs#L57)

**Cablage App : invariant hote/invite jamais les deux (correctif de revue)**

- Refus croise : creation refusee si un lobby est deja rejoint.
  [`LobbyFlowController.cs:169`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L169)

- Refus croise : join refuse si une room hote est deja ouverte ; traduction de chaque etat de join en notice visible.
  [`LobbyFlowController.cs:192`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L192)
  [`LobbyFlowController.cs:305`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L305)

**UI : saisie du code, retour visible**

- Champ de saisie cable, code brut lu au moment du clic (jamais mute par l'ecran).
  [`LobbyShellScreen.cs:195`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L195)

- Affichage du lobby rejoint, distinct de l'affichage hote.
  [`LobbyShellScreen.cs:187`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L187)

**Tests**

- Etats de `LobbyJoinService` via une fausse plateforme de lobby (aucun client Steam requis) : code invalide, services indisponibles, room pleine, session expiree, echec generique, double-clic, double-join.
  [`Story23JoinByCodeTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story23JoinByCodeTests.cs#L1)
