# Story 2.8 - Notes de checkpoint jouable en ligne Epic 2

Date: 2026-09-09

## Preuves locales

- Le gate host-side Netcode refuse les payloads invalides, les profils sans personnage, et toute connexion qui depasserait quatre joueurs reserves/connectes.
- Un invite deja dans une room ne demarre plus `StartClient()` depuis son bouton Start : il recoit une notice lui demandant d'attendre le lancement hote.
- Le roster republie un changement quand seul `CharacterId` varie, afin de rafraichir les silhouettes sans perdre l'etat pret.
- Le checkpoint automatisable verifie aussi que `MVP_Run`, `NetworkedPlayerRoot`, l'approbation de connexion, le chargement de scene Netcode, le HUD et le lifecycle restent cables.
- La non-regression contraste `MainMenuLobby` reste couverte par `Story12LobbyShellTests.MainMenuLobbyStatusLabelsContrastWithPanelBackgrounds`.

## Parcours Multiplayer Play Mode local

Objectif manuel:

1. Lancer le projet dans l'Editor avec Multiplayer Play Mode disponible.
2. Depuis `MainMenuLobby`, creer une room hote privee.
3. Publier le profil local, marquer les joueurs prets ou activer l'exception de test solo.
4. Demarrer depuis l'hote.
5. Confirmer le chargement synchronise de `MVP_Run`, le spawn de `NetworkedPlayerRoot`, le mouvement a pied, le HUD reseau, la mort/respawn locale et le retour lobby sur perte de session.

Etat 2026-09-09: execute par l'utilisateur hors de cet environnement CLI (session Editor interactive MPP). Parcours confirme fonctionnel par retour utilisateur direct ; aucune trace automatisee associee.

## Parcours distant Steam deux joueurs

Objectif manuel:

1. Deux machines connectees a Steam avec l'AppID de test configure.
2. Le host cree une room privee et communique le code.
3. Le client rejoint par code, publie son profil et passe pret.
4. Le client clique Start avant le host: aucune scene locale ne se lance, une notice demande d'attendre le host.
5. Le host clique Start: `StartHost()` demarre, le client rejoint via Steamworks Networking Sockets, `MVP_Run` charge par `NetworkManager.SceneManager`.
6. Les deux joueurs confirment spawn, mouvement, HUD, compteur joueurs, puis host quit/session lost et retour client vers `MainMenuLobby` avec notice persistante.

Etat 2026-09-09: execute par l'utilisateur hors de cet environnement CLI (deux machines/comptes Steam). Parcours confirme fonctionnel par retour utilisateur direct : creation, code, join, ready, lancement host, spawn/mouvement/HUD, host quit.

## Cap quatre joueurs

Preuve locale automatisee: `Story28Epic2OnlinePlayableCheckpointTests.ConnectionApprovalRejectsFifthReservedPlayerBeforeSpawn` couvre le refus du cinquieme slot avant creation de player object.

Objectif manuel distant: demarrer avec quatre joueurs connectes ou reserves, tenter un cinquieme join, confirmer que le host refuse la connexion et que le client recoit la raison `Connexion refusee : la session a deja quatre joueurs.`

Etat 2026-09-09: non testable dans les conditions actuelles (pas assez de comptes/machines Steam disponibles simultanement). Report accepte a une iteration ulterieure ; couverture actuelle limitee au test EditMode automatise ci-dessus.

## Host quit / session lost

Preuve locale automatisee: les tests Story 2.7 et Story 2.8 gardent le wiring `NetworkedRunSessionMonitor`, `OnTransportFailure`, retour lobby et notice persistante.

Objectif manuel distant: fermer le host pendant `MVP_Run`, puis confirmer que le client shutdown Netcode, revient a `MainMenuLobby` et voit la notice persistante de perte de session.

Etat 2026-09-09: execute par l'utilisateur hors de cet environnement CLI, dans le cadre du parcours distant Steam deux joueurs ci-dessus. Confirme fonctionnel par retour utilisateur direct.

## Limitations connues avant Epic 3

- Pas d'invitation Steam overlay ou deep link OS complet.
- Pas de reconciliation complete `SteamId` vers `clientId` au-dela du payload de connexion Netcode.
- Pas de leave-room invite dedie, migration d'hote, serveur dedie, lobby browser public, ni matchmaking public.
- Les objets gameplay restent host-owned et les `NetworkVariable` gameplay restent server-write.

## Bloqueurs

- Le cap quatre joueurs en conditions reelles (5e join refuse) reste non teste manuellement, faute de comptes/machines Steam suffisants ; reporte a une iteration ulterieure. Couverture actuelle : test EditMode automatise uniquement.
- Toutes les autres preuves manuelles distantes (parcours 2 joueurs, host quit) ont ete executees par l'utilisateur hors de cet environnement CLI et confirmees fonctionnelles par retour direct le 2026-09-09.
- Epic 2 (2.1 a 2.8) cloture `done` sur cette base le 2026-09-09.
