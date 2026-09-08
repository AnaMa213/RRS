---
title: 'Story 2.4 - Roster de lobby, etat pret et synchronisation des reglages'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
baseline_commit: '4f26f2187dd8b1cf1d1ee539e804a81a441d3387'
---

## Intent

**Problem:** Depuis les Stories 2.2/2.3, une room Steam privee peut etre creee et rejointe, mais rien n'affiche les joueurs connectes, ne permet de se marquer pret, ni ne synchronise la difficulte choisie par l'hote avant le lancement de la partie : Start Game reste un simple check de profil, sans lien avec l'etat reel de la room.

**Approach:** Un nouveau service C# pur (`LobbyRosterService`) interroge par polling (`Tick`, comme `OnlineServicesBootstrapService`) les donnees de lobby et de membre Steam via trois nouvelles methodes de `ISteamLobbyPlatform` implementees par `FacepunchSteamLobbyPlatform` (`Lobby.GetData`/`SetData` pour la difficulte partagee, `Lobby.GetMemberData`/`SetMemberData` pour l'etat pret par joueur). `LobbyFlowController` compose roster/pret/difficulte/services en un gate qui refuse Start Game avec une cause visible distincte tant que l'hote n'a pas un roster synchronise et tous les joueurs prets (ou une exception de test solo locale) ; un joueur ayant rejoint par code recoit en continu la difficulte de l'hote avant tout chargement du monde. `LobbyShellScreen` expose un bouton pret, un bouton d'exception de test solo, et un affichage du roster, tous nouveaux. Une revue a la volee a releve un vrai defaut (le libelle du bouton pret restait perime apres une fermeture/reouverture de room ou un join, alors que l'etat pret reel est silencieusement remis a `false` par `LobbyRosterService`) : corrige en resynchronisant `ShowReadyState(false)` depuis `LobbyFlowController` sur les memes transitions.

## Suggested Review Order

**Gate de lancement cote hote**

- Point d'entree : refuse Start Game avec une cause distincte (services, roster non synchronise, joueurs non prets) uniquement quand une room hote est ouverte ; solo local et joueur invite restent inchanges.
  [`LobbyFlowController.cs:249`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L249)

**Service de roster (polling, sans callback Steamworks statique)**

- `Tick()` interroge la plateforme uniquement hors repos (room ouverte ou lobby rejoint) et ne republie un evenement que si l'instantane a change.
  [`LobbyRosterService.cs:91`](../../Assets/RoadRage/Features/Online/LobbyRosterService.cs#L91)

- `AllMembersReady`/`IsSynchronized` : roster vide ou non synchronise compte comme non pret.
  [`LobbyRosterService.cs:57`](../../Assets/RoadRage/Features/Online/LobbyRosterService.cs#L57)

- `SetLocalReady`/`PublishDifficulty` : aucun appel plateforme hors lobby actif ou hors room hote ouverte.
  [`LobbyRosterService.cs:106`](../../Assets/RoadRage/Features/Online/LobbyRosterService.cs#L106)

**Isolation SDK pour le roster et les donnees de lobby**

- Seule classe a lire/ecrire les donnees de lobby et de membre Steam (signatures verifiees par reflexion sur le DLL Facepunch : `GetMemberData(Friend, string)`, `SetMemberData` local uniquement).
  [`FacepunchSteamLobbyPlatform.cs:72`](../../Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs#L72)

- Nouveaux membres d'interface et structs neutres vis-a-vis du SDK.
  [`ISteamLobbyPlatform.cs:21`](../../Assets/RoadRage/Features/Online/ISteamLobbyPlatform.cs#L21)

**Cablage App : difficulte hote/invite et resynchronisation de l'etat pret (correctif de revue)**

- Publication de la difficulte a l'ouverture de room et a chaque edition hote.
  [`LobbyFlowController.cs:286`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L286)

- Reception continue de la difficulte hote par le joueur invite, jamais appliquee cote hote.
  [`LobbyFlowController.cs:320`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L320)

- Resynchronisation du libelle pret sur Open/Joined (correctif de revue).
  [`LobbyFlowController.cs:386`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L386)
  [`LobbyFlowController.cs:438`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L438)

**UI : bouton pret, exception de test solo, affichage du roster**

- Nouveaux hooks d'ecran, purs relais d'intention comme le reste de LobbyShellScreen.
  [`LobbyShellScreen.cs:74`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L74)
  [`LobbyShellScreen.cs:253`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L253)
  [`LobbyShellScreen.cs:289`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L289)

**Bootstrap**

- Construction et pompage de `LobbyRosterService`, meme instance de plateforme que LobbyRoom/LobbyJoin.
  [`RoadRageBootstrap.cs:97`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L97)

**Tests**

- Etats de `LobbyRosterService` via une fausse plateforme de lobby : polling conditionnel, AllMembersReady, reset a l'ouverture/fermeture de room, publication conditionnelle de la difficulte.
  [`Story24LobbyRosterTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story24LobbyRosterTests.cs#L1)
