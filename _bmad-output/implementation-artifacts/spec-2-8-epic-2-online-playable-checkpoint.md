---
title: 'Story 2.8 - Checkpoint jouable en ligne de l Epic 2'
type: 'feature'
created: '2026-09-09'
status: 'done'
review_loop_iteration: 0
baseline_commit: 'c9a467abab2ce21f73d42df4d430b393ac5e8a33'
context: []
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intent

**Problem:** L Epic 2 dispose de slices 2.1 a 2.7, mais le checkpoint final n est pas encore prouve : il manque un gate integre qui valide creation room, join, roster, lancement, spawn, mouvement, HUD, cycle de vie, cap quatre joueurs et erreurs visibles avant de demarrer l Epic 3. La revue pre-2.8 a aussi trouve deux failles qui empechent de cloturer proprement l epic : l approval Netcode accepte tout, donc le cap quatre joueurs n est pas garanti cote host, et un client rejoint peut lancer `StartClient` depuis le roster sans signal de lancement hote.

**Approach:** Transformer la Story 2.8 en revue/correction de cloture : durcir le passage lobby -> reseau, ajouter les tests de non-regression manquants, produire des notes de checkpoint avec preuves locales et tests manuels distants, puis synchroniser les statuts 2.1-2.8 uniquement quand le gate est passe.

## Boundaries & Constraints

**Always:** Le lobby Steam prive et `MaxPlayers = 4` restent la source d entree utilisateur, mais le host Netcode doit aussi refuser les connexions invalides ou excedentaires. Les joueurs ne demarrent pas une scene client de leur cote : le host lance la session et le chargement synchronise de `MVP_Run`. Les `NetworkObject` gameplay restent host-owned, les `NetworkVariable` gameplay server-write, le HUD lecture seule, et les notices passent par `RoadRageBootstrap.Notices`.

**Ask First:** Si la correction exige un vrai systeme d invitation Steam overlay, un deep link OS, une reconciliation SteamId -> `clientId` complete, un flux leave-room invite, une migration d hote, ou une refonte UI/rendering globale.

**Never:** Ne pas ajouter de matchmaking public, lobby browser, serveur dedie, port forwarding, host migration, conduite/vehicule Epic 3, restart complet apres all-dead, ou etat gameplay client-authoritative. Ne pas marquer 2.1-2.7 `done` tant que 2.8 n a pas ses preuves et notes de checkpoint.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Lancement hote valide | Room ouverte, services online, roster synchronise, tous prets ou exception solo test | `StartHost()` demarre, `MVP_Run` charge via `NetworkManager.SceneManager`, joueurs spawnent et voient le HUD | Echec Netcode publie `NetworkStartFailedMessage` |
| Client rejoint avant lancement hote | Client dans un lobby rejoint clique Start sans signal hote | Aucun `StartClient()` premature ; un retour visible explique d attendre le lancement hote | Pas de chargement local de `MVP_Run` |
| Connexion invalide | Payload Netcode absent/corrompu ou profil non enregistre | ConnectionApproval refuse la connexion et ne cree pas de player object | Notice/log clair, aucun spawn par profil par defaut silencieux |
| Cap quatre joueurs | Quatre joueurs deja connectes ou reserves, cinquieme tentative | Host refuse le 5e client avant spawn ; aucun cinquieme `NetworkedPlayerState` | Cause visible/documentee dans les notes de test |
| Roster personnage change | Un membre publie seulement un nouveau `CharacterId` | `LobbyRosterService` detecte le changement et l UI roster met a jour la silhouette | Pas d etat pret perdu |
| Session perdue en run | Host quit ou `OnTransportFailure` dans `MVP_Run` | Clients shutdown Netcode, retournent a `MainMenuLobby`, notice visible persistante | Pas de migration hote |
| Checkpoint manuel distant | Deux machines/comptes Steam testent via Networking Sockets | Creation, code, join, ready, start, spawn, mouvement, HUD et deconnexion sont notes | Si non executable, raison et bloqueur explicites |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` (L317-L356) -- gate de Start Game : host deja gate par services/roster/ready, mais client rejoint lance aujourd hui `StartNetworkedRun(false)` sans signal hote.
- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` (L372-L447) -- demarrage Netcode et transport Facepunch ; client cible `lobbyRoster.Current.OwnerId`.
- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` (L455-L468) -- `HandleConnectionApproval` decode le payload mais approuve toujours ; point a durcir pour payload, capacite et reservation host-side.
- `Assets/RoadRage/Features/Online/LobbyRosterService.cs` (L185-L208) -- `SnapshotsEqual` ignore `CharacterId`, ce qui peut laisser les portraits de roster perimes.
- `Assets/RoadRage/Features/Online/ISteamLobbyPlatform.cs` (L74-L116) -- `LobbyMemberSnapshot` porte `SteamId`, `DisplayName`, `CharacterId`, `Ready`; source de comparaison roster.
- `Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs` (L120-L170) -- cree/configure le `NetworkManager`, transport Facepunch et prefab `NetworkedPlayerRoot`.
- `Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs` (L100-L180) -- spawn host-owned et fallback profil ; doit rester sans creation de 5e joueur.
- `Assets/RoadRage/App/Run/NetworkedRunSessionMonitor.cs` (L32-L84) -- retour lobby sur host lost/transport failure en run, deja etendu en 2.7.
- `Assets/RoadRage/Features/UI/LobbyRosterScreen.cs` -- ecran roster dedie : ready, start, difficulte, room code, slots quatre joueurs.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- HUD en jeu lecture seule : coeurs, stamina, player count, statut reseau, argent, overlay mort.
- `Assets/RoadRage/Tests/EditMode/Story21OnlineServicesBootstrapTests.cs` a `Story27PlayerLifecycleTests.cs` -- 110 tests EditMode deja presents, a rejouer et etendre par `Story28Epic2OnlinePlayableCheckpointTests.cs`.
- `_bmad-output/implementation-artifacts/spec-fix-mainmenulobby-label-constrast.md` -- l ancien handoff TMP a ete remplace par un bugfix contraste `done`; garder seulement une verification de non-regression lisibilite.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` -- durcir `HandleConnectionApproval` avec une regle testable qui refuse payload invalide, profil absent et cinquieme connexion, puis renseigne `response.Reason`/logs -- garantir le cap et eviter le spawn par defaut silencieux.
- [x] `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` -- empecher le client rejoint de demarrer `StartClient()` sans signal explicite de lancement hote ; le bouton client doit afficher un retour d attente ou rester inerte documente -- eviter les joins prematures avant scene host.
- [x] `Assets/RoadRage/Features/Online/LobbyRosterService.cs` -- inclure `CharacterId` dans `SnapshotsEqual` -- assurer que le roster reflete les changements de personnage.
- [x] `Assets/RoadRage/Tests/EditMode/Story28Epic2OnlinePlayableCheckpointTests.cs` -- ajouter des tests de gate integre pour approval/cap, start client premature, comparaison roster par personnage, wiring des scenes/prefabs/HUD/lifecycle, et non-regression du contraste MainMenuLobby -- donner une preuve automatisable au checkpoint.
- [x] `docs/setup/story-2-8-epic-2-online-playable-checkpoint-notes.md` -- documenter parcours MPP local, parcours distant Steam deux joueurs, cap quatre joueurs, host quit/session lost, limitations connues et bloqueurs avant Epic 3 -- rendre la cloture reviewable.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` et specs 2.1-2.7 si necessaire -- apres verification 2.8, synchroniser 2.1-2.8 vers `done` et `epic-2` vers `done` seulement si les preuves passent ou que les bloqueurs sont explicitement acceptes. Applique le 2026-09-09 : preuves manuelles distantes executees et confirmees par retour direct de l'utilisateur ; cap quatre joueurs en conditions reelles accepte comme limitation reportee.

**Acceptance Criteria:**
- Given les stories 2.1-2.7 existent, when les tests de checkpoint 2.8 tournent, then la chaine online critique est couverte par tests EditMode/PlayMode locaux et les resultats sont enregistres sous `_bmad-output/implementation-artifacts/story-2-8-*results.*`.
- Given quatre joueurs sont deja connectes ou reserves, when un cinquieme client tente la connexion, then le host refuse avant spawn et aucun cinquieme etat joueur n existe.
- Given un client a rejoint le lobby, when il clique Start avant le lancement host, then il ne demarre pas de session client prematuree et recoit un feedback visible.
- Given le checkpoint est cloture, when on lit les notes 2.8, then elles distinguent preuves executees, tests distants non executes, limitations acceptees et vrais bloqueurs avant Epic 3.

## Spec Change Log

- 2026-09-09 -- Implementation: ajout du gate d'approbation host-side, blocage du Start client premature, detection roster par `CharacterId`, tests checkpoint Story 2.8, notes de checkpoint, et correction de deux assertions EditMode Story 2.7 trop fragiles pour le runner live-editor.

## Design Notes

2.8 n est pas une nouvelle feature de gameplay : c est le verrou de sortie de l Epic 2. Les correctifs acceptes appartiennent ici parce qu ils conditionnent directement l AC "four-player cap verified" et le parcours "host start -> clients receive run", donc les deferer rendrait le checkpoint faux.

## Verification

**Commands:**
- `git diff --check` -- passed 2026-09-09, aucune erreur whitespace.
- Unity CLI live-editor `run_tests` EditMode filtre `RoadRage.Tests.EditMode.Story2` -- passed 2026-09-09, 118/118, resultat enregistre dans `story-2-8-editmode-results.txt`.
- Unity CLI live-editor `run_tests` PlayMode filtre `RoadRage.Tests.PlayMode.Story2` -- passed 2026-09-09, 5/5, resultat enregistre dans `story-2-8-playmode-results.txt`.
- Unity batch classique avec `-runTests` -- non utilise pour les resultats finaux : le projet etait deja ouvert dans l'Editor, donc le runner batch refusait d'ouvrir une seconde instance. Les resultats `.txt` proviennent du runner de l'Editor connecte ; aucun `.xml` n'a ete produit par cette voie.

**Manual checks:**
- Multiplayer Play Mode local : execute par l'utilisateur hors de cet environnement CLI, confirme fonctionnel par retour direct.
- Test distant Steamworks Networking Sockets deux joueurs : room privee, code, join, ready, lancement host, spawn/mouvement/HUD, host quit -- execute par l'utilisateur hors de cet environnement CLI, confirme fonctionnel par retour direct.
- Cap quatre joueurs en conditions reelles : non teste, faute de comptes/machines Steam suffisants ; accepte comme limitation reportee a une iteration ulterieure. Couverture actuelle : test EditMode automatise uniquement.
- Details dans `docs/setup/story-2-8-epic-2-online-playable-checkpoint-notes.md`.
