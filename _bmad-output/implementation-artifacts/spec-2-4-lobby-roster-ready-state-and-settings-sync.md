---
title: 'Story 2.4 - Roster de lobby, etat pret et synchronisation des reglages'
type: 'feature'
created: '2026-09-08'
status: 'done'
route: 'one-shot'
baseline_commit: '4f26f2187dd8b1cf1d1ee539e804a81a441d3387'
---

## Intent

**Problem:** Depuis les Stories 2.2/2.3, une room Steam privee peut etre creee et rejointe, mais rien n'affiche les joueurs connectes ni leur personnage, ne permet de se marquer pret, ni ne synchronise la difficulte choisie par l'hote avant le lancement de la partie : Start Game reste un simple check de profil, sans lien avec l'etat reel de la room.

**Approach:** Un nouveau service C# pur (`LobbyRosterService`) interroge par polling (`Tick`, comme `OnlineServicesBootstrapService`) les donnees de lobby et de membre Steam via quatre nouvelles methodes de `ISteamLobbyPlatform` implementees par `FacepunchSteamLobbyPlatform` (`Lobby.GetData`/`SetData` pour la difficulte partagee, `Lobby.GetMemberData`/`SetMemberData` pour l'etat pret et le profil -- nom + personnage choisi -- par joueur). `LobbyFlowController` compose roster/pret/difficulte/services en un gate qui refuse Start Game avec une cause visible distincte tant que l'hote n'a pas un roster synchronise et tous les joueurs prets (ou une exception de test solo locale) ; un joueur ayant rejoint par code recoit en continu la difficulte de l'hote avant tout chargement du monde.

Un premier jet avait case le roster sous forme de texte brut dans le panneau de setup existant : retour humain -- il fallait un vrai ecran de lobby distinct montrant la silhouette teintee (Story 1.3, `CharacterDef.PreviewTint`) et le nom de chaque joueur, pas une liste texte. Reconstruit en consequence : `LobbyShellScreen` redevient l'ecran pre-room (creation/join/personnage/jeu solo local, inchange depuis la Story 1.2/2.3) ; un nouvel ecran `LobbyRosterScreen` (panneau `LobbyPanel` dedie, jusqu'a 4 `LobbyPlayerSlotView`) prend le relais des qu'une room est ouverte ou rejointe -- roster avec personnage+nom+pret, bouton pret, difficulte (hote), exception de test solo desormais explicitement etiquetee, Start Game, fermeture de room. `LobbyFlowController` bascule les deux ecrans sur Open/Closed/Joined et publie le profil local (nom + id de personnage) au meme rythme que la difficulte. Une revue a la volee (avant ce retour humain) avait deja releve et corrige un defaut reel : le libelle du bouton pret restait perime apres une fermeture/reouverture de room ou un join, alors que l'etat pret reel est silencieusement remis a `false` par `LobbyRosterService` -- corrige en resynchronisant `ShowReadyState(false)` sur les memes transitions, preserve dans la reconstruction.

**Limite connue, non couverte par cette story :** aucun flux de sortie de room cote invite n'existe (deja hors scope depuis la Story 2.3) ; le bouton Back de `LobbyShellScreen` devient inatteignable une fois dans `LobbyRosterScreen`. Seul l'hote peut revenir en arriere, via "Fermer la room".

## Suggested Review Order

**Gate de lancement cote hote**

- Point d'entree : refuse Start Game avec une cause distincte (services, roster non synchronise, joueurs non prets) uniquement quand une room hote est ouverte ; solo local et joueur invite restent inchanges.
  [`LobbyFlowController.cs:311`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L311)

**Bascule d'ecran pre-room / post-room**

- Sur Open : masque LobbyShellScreen, affiche LobbyRosterScreen avec le code, publie difficulte et profil local.
  [`LobbyFlowController.cs:519`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L519)

- Sur Joined : meme bascule cote invite, difficulte en lecture seule, bouton de fermeture masque.
  [`LobbyFlowController.cs:587`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L587)

- Resolution du roster affiche : teinte du personnage via le catalogue (silhouette neutre si inconnu), nom deja resolu par la plateforme.
  [`LobbyFlowController.cs:461`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L461)

**Service de roster (polling, sans callback Steamworks statique)**

- `Tick()` interroge la plateforme uniquement hors repos (room ouverte ou lobby rejoint) et ne republie un evenement que si l'instantane a change.
  [`LobbyRosterService.cs:91`](../../Assets/RoadRage/Features/Online/LobbyRosterService.cs#L91)

- `AllMembersReady`/`IsSynchronized` : roster vide ou non synchronise compte comme non pret.
  [`LobbyRosterService.cs:57`](../../Assets/RoadRage/Features/Online/LobbyRosterService.cs#L57)

- `PublishLocalProfile` : nom + personnage publies pour hote et invite, jamais hors lobby actif.
  [`LobbyFlowController.cs:413`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L413)

**Isolation SDK pour le roster et les donnees de lobby**

- Lecture du roster : nom RoadRage publie avec repli sur le nom Steam, id de personnage brut (signatures verifiees par reflexion sur le DLL Facepunch : `GetMemberData(Friend, string)`).
  [`FacepunchSteamLobbyPlatform.cs:76`](../../Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs#L76)

- Publication du profil local (`SetMemberData`, portee locale uniquement).
  [`FacepunchSteamLobbyPlatform.cs:109`](../../Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs#L109)

**UI : nouvel ecran de lobby dedie (silhouette + nom + pret)**

- Ecran post-room : roster de 4 slots, pret, difficulte hote, exception de test solo etiquetee, Start Game, fermeture de room.
  [`LobbyRosterScreen.cs:17`](../../Assets/RoadRage/Features/UI/LobbyRosterScreen.cs#L17)

- Affichage d'un slot : silhouette teintee + nom + etat pret, ou emplacement libre.
  [`LobbyRosterScreen.cs:209`](../../Assets/RoadRage/Features/UI/LobbyRosterScreen.cs#L209)

**Tests**

- Etats de `LobbyRosterService` via une fausse plateforme de lobby : polling conditionnel, AllMembersReady, reset a l'ouverture/fermeture de room, publication conditionnelle de la difficulte et du profil.
  [`Story24LobbyRosterTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story24LobbyRosterTests.cs#L1)
