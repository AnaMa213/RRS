---
title: 'Story 2.1 - Bootstrap des services en ligne Steam et retour de statut'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
baseline_commit: '5c3cb6616d701d89c714311d592e9420095bab80'
---

## Intent

**Problem:** L'Epic 1 ne fait aucune tentative reelle d'initialisation Steam ; Create Lobby et Join By Code restent des placeholders "indisponible", et rien ne distingue un echec d'initialisation, une session Steam non connectee ou un environnement sans Steam.

**Approach:** Un service C# pur (`OnlineServicesBootstrapService`) resout l'appel `SteamClient.Init` derriere une abstraction testable (`ISteamPlatform`/`FacepunchSteamPlatform`) vers un des quatre etats terminaux (Online, InitializationFailed, SignInFailed, Offline), porte par le singleton persistant `RoadRageBootstrap`, declenche a l'ouverture du flux de lobby par `LobbyFlowController`, et affiche via le canal de notices existant.

## Suggested Review Order

**Etat et resolution du statut**

- Point d'entree : resolution du statut apres `Init`, quatre issues possibles y compris le cas defensif `!IsValid`.
  [`OnlineServicesBootstrapService.cs:72`](../../Assets/RoadRage/Features/Online/OnlineServicesBootstrapService.cs#L72)

- Tentative unique par session applicative : un second appel republie le statut deja resolu sans relancer `Init`.
  [`OnlineServicesBootstrapService.cs:36`](../../Assets/RoadRage/Features/Online/OnlineServicesBootstrapService.cs#L36)

- Distinction entre runtime Steam absent (`DllNotFoundException` -> Offline) et tout autre echec (-> InitializationFailed).
  [`OnlineServicesBootstrapService.cs:50`](../../Assets/RoadRage/Features/Online/OnlineServicesBootstrapService.cs#L50)

**Isolation du SDK Steam**

- Seule classe du projet a toucher `Steamworks.SteamClient` directement ; aucune logique d'etat ici.
  [`FacepunchSteamPlatform.cs`](../../Assets/RoadRage/Features/Online/FacepunchSteamPlatform.cs#L1)

- Abstraction que le service consomme, ce qui le rend testable sans client Steam installe.
  [`ISteamPlatform.cs`](../../Assets/RoadRage/Features/Online/ISteamPlatform.cs#L1)

**Cablage App : declenchement et retour visible**

- Propriete persistante construite une seule fois avec l'AppID de test 480 (Spacewar), ferme a la destruction.
  [`RoadRageBootstrap.cs:73`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L73)

- Declenchement reel : `TryInitialize()` appele a l'ouverture du flux de lobby (Awake de LobbyFlowController), pas au boot brut de l'app.
  [`LobbyFlowController.cs:47`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L47)

- Traduction de chaque etat terminal en notice avec la severite adaptee, via le canal de notices existant depuis la Story 1.1.
  [`LobbyFlowController.cs:122`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L122)

**Tests**

- Etats de resolution, idempotence de `TryInitialize`, et garde-fous architecturaux (isolation gameplay, absence de secrets en dur) via une fausse plateforme.
  [`Story21OnlineServicesBootstrapTests.cs`](../../Assets/RoadRage/Tests/EditMode/Story21OnlineServicesBootstrapTests.cs#L1)

- Verification sur le vrai cycle de vie Unity : l'ouverture du flux de lobby resout le statut et publie une notice visible.
  [`Story21OnlineServicesPlayModeTests.cs`](../../Assets/RoadRage/Tests/PlayMode/Story21OnlineServicesPlayModeTests.cs#L1)
