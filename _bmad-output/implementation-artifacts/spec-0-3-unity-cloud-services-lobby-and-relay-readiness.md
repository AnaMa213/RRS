---
title: 'Story 0.3 : Steamworks, lobby et readiness Networking Sockets'
type: 'chore'
created: '2026-09-02'
status: 'in-review'
review_loop_iteration: 0
baseline_commit: '6e04205cba0d7bf27124a88b4ca278e111439037'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-0-context.md'
  - '{project-root}/docs/setup/tooling-validation-log.md'
  - '{project-root}/docs/setup/epic-0-readiness-checklist.md'
  - '{project-root}/_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md'
---

<frozen-after-approval reason="intention controlee par l'humain - ne pas modifier sauf renegociation explicite">

## Intention

**Probleme :** Story 0.2 est cloturee, mais le projet Unity n'est pas encore prouve comme connecte a Steamworks, lobby prive, Networking Sockets, invite/Lobby ID et environnement de test non-production. Sans ces preuves, les futures stories online risquent de commencer avant que les contraintes reseau, secrets et erreurs UI soient visibles.

**Approche (revisee par course-correction, voir `_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md`) :** Creer un tutoriel Story 0.3 qui guide les actions manuelles dans Unity Editor et Steamworks (SDK, AppID de test 480 "Spacewar"), puis mettre a jour les lignes de suivi pour recevoir les preuves sans jamais stocker de secret. La validation agent doit seulement relire les preuves, verifier les statuts et consigner les bloqueurs. Cette story remplace entierement l'ancienne approche Unity Cloud/Unity Gaming Services/Relay, retiree suite a la contrainte financiere solo-dev identifiee par l'utilisateur (pas de revenu recurrent, le cout ne doit pas scaler avec le nombre de joueurs) : Steamworks Networking Sockets (Steam Datagram Relay) est gratuit quel que soit le nombre de joueurs simultanes, contrairement a Unity Relay/Multiplayer Services.

## Limites & Contraintes

**Toujours :** Garder la configuration Steamworks (AppID, SDK init), l'activation du transport et les smoke notes comme actions manuelles utilisateur ; utiliser `Not Started`, `In Progress`, `Pass`, `Blocked`, `Not Applicable` ; developper et tester avec l'AppID de test officiel Valve `480` (Spacewar) avant tout compte Steamworks reel ; viser un projet/environnement de test non-production ; documenter Sessions privees creees par l'host, lobby Steam (`ISteamMatchmaking`), Networking Sockets, `MaxPlayers = 4`, invite Steam natif et Lobby ID comme wrapper UI de secours, et exigences d'erreurs visibles pour join, Networking Sockets, service, disconnect et host quit.

**Demander d'abord :** Tout service payant, changement vers matchmaking public, dedicated servers, host migration, deep links natifs OS, changement du cap joueurs, stockage de token, distribution hors-Steam pour le online, ou dependance externe non prevue necessite une approbation humaine.

**Jamais :** Ne pas creer ou modifier de compte Steamworks a la place de l'utilisateur ; ne pas coller de token, API key, credential, Lobby ID complet ou invite utilisable dans le repo ; ne pas commencer le gameplay Epic 1 ; ne pas marquer `Pass` sans preuve re-verifiable et caviardee.

## Matrice I/O & Cas Limites

| Scenario | Entree / Etat | Sortie / Comportement attendu | Gestion d'erreur |
|----------|---------------|-------------------------------|------------------|
| Configuration nominale | Projet `RRS` ouvert avec le transport Steamworks installe (Story 0.2 revisee) | Tutoriel indique comment configurer l'AppID de test 480, initialiser le SDK Steamworks, creer un lobby prive et collecter les preuves VAL-012 a VAL-015 | Les preuves sont resumees sans secret dans le log |
| Steam indisponible | Steam client absent, non connecte, ou API Steamworks ne s'initialise pas | La ligne concernee reste `Blocked` avec source consultee, option manquante et impact | Ne pas choisir un fallback payant ou architectural sans approbation |
| Secret dans une preuve | Capture ou note contient token, Lobby ID complet ou lien utilisable | La preuve est rejetee jusqu'a caviardage `[REDACTED_TOKEN]` | Ne jamais copier le secret dans le log |
| Exigence erreur UI manquante | Les cas join/Networking Sockets/service/disconnect/host quit ne sont pas notes | Le tutoriel garde Story 0.3 incomplete ou `In Progress` | Reporter clairement le manque avant Story 0.4/0.8 |

</frozen-after-approval>

## Code Map

- `docs/setup/story-0-3-unity-cloud-services-tutorial.md` -- Tutoriel manuel reecrit pour Steamworks, AppID de test 480, lobby prive, Networking Sockets, `MaxPlayers = 4`, invite Steam/Lobby ID et preuves sans secret.
- `steam_appid.txt` -- Fichier AppID Steam local pour les tests Editor/build avec l'AppID public Valve `480` (Spacewar).
- `Assets/Editor/RoadRageSteamworksSmokeTest.cs` -- Helper editor-only expose via `RoadRage > Steamworks > Run AppID 480 Smoke Test`, appelle `SteamClient.Init(480, false)` puis `SteamNetworkingUtils.InitRelayNetworkAccess()` sans logger d'identifiant sensible.
- `docs/setup/tooling-validation-log.md` -- Lignes `VAL-012` a `VAL-015` retargetees sur Steamworks a pointer vers le tutoriel ; `VAL-032` peut recevoir une note d'exigence d'erreurs UI visibles, sans pretendre aux smoke tests finaux.
- `docs/setup/epic-0-readiness-checklist.md` -- Ligne action manuelle et validation agent Story 0.3 synchronisee avec le tutoriel Steamworks et les preuves attendues.
- `Packages/manifest.json` et `Packages/packages-lock.json` -- Preuves read-only que le transport Steamworks (Story 0.2 revisee) est present avant la configuration Steamworks.
- `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Statut Story 0.3 a maintenir pendant l'execution.
- `_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md` -- Justification et detail complet du remplacement Unity Multiplayer Services/Relay par Steamworks.

## Taches & Acceptation

**Execution :**
- [x] `docs/setup/story-0-3-unity-cloud-services-tutorial.md` -- Creer le tutoriel lineaire Story 0.3 avec etapes manuelles, preuves attendues, caviardage des secrets, et arret explicite avant Story 0.4 (version initiale Unity Cloud).
- [x] `docs/setup/tooling-validation-log.md` -- Mettre a jour `VAL-012` a `VAL-015` pour pointer vers le tutoriel et preciser les preuves attendues, sans passer `Pass` tant que l'utilisateur n'a pas fourni de preuve.
- [x] `docs/setup/epic-0-readiness-checklist.md` -- Synchroniser les lignes Story 0.3 pour distinguer action manuelle utilisateur et validation agent.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Garder Story 0.3 en `in-progress` pendant la creation du tutoriel, puis `review` quand le travail agent-executable est pret a relire.
- [x] Course-correction : remplacer le contenu Unity Cloud/Unity Gaming Services/Relay par Steamworks/AppID 480/Networking Sockets dans le tutoriel, cette spec, et les lignes VAL-012 a VAL-015.
- [x] `steam_appid.txt` -- Ajouter le fichier AppID local `480` a la racine du projet Unity pour preparer le smoke test Steamworks.
- [x] `Assets/Editor/RoadRageSteamworksSmokeTest.cs` -- Ajouter un menu Unity editor-only qui lance l'initialisation Steamworks AppID 480 et l'acces Steam Relay sans demarrer le futur lobby gameplay.
- [x] `docs/setup/tooling-validation-log.md` et `docs/setup/epic-0-readiness-checklist.md` -- Marquer `VAL-007` en `Pass`, passer `VAL-012` a `VAL-015` en `In Progress`, et documenter precisement les preuves utilisateur restantes.

**Criteres d'acceptation :**
- Given Story 0.2 est `done` (avec le transport Steamworks installe apres revision), when le tutoriel Story 0.3 est lu, then il guide la configuration AppID de test Steamworks et la creation de lobby prive/Networking Sockets sans demander a l'agent d'effectuer les actions externes.
- Given une preuve contient un Lobby ID complet, token, credential ou invite utilisable, when elle est reportee dans le log, then le tutoriel impose le caviardage avec `[REDACTED_TOKEN]`.
- Given `VAL-012` a `VAL-015` sont inspectees apres creation du tutoriel, when aucune preuve utilisateur reelle n'a encore ete fournie, then elles restent `Not Started` ou `In Progress`, jamais `Pass`.
- Given les exigences lobby sont collectees, when Story 0.3 avance, then les cas create, join, leave, host close, expiration, abandoned room cleanup, return-to-menu et erreurs visibles sont notes pour Epic 2.
- Given un compte Steamworks reel et le fee unique ne sont pas encore payes, when le tutoriel est suivi, then le developpement et les tests restent possibles via l'AppID 480 sans bloquer Story 0.3.

## Spec Change Log

- 2026-09-02 : Spec initiale creee apres cloture Story 0.2 et promotion de Story 0.3 en `ready-for-dev` (version Unity Cloud/Unity Gaming Services/Relay).
- 2026-09-02 : Implementation agent cree le tutoriel Story 0.3, synchronise les lignes de preuve et passe le suivi sprint en `review`.
- 2026-09-02 : Revue BMAD appliquee : distinction `review` documentaire vs preuves Unity manuelles, exigences join-code/VAL-032 precisees, et remediation secrets documentee.
- 2026-09-03 : Course-correction (`_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md`) -- remplacement complet du premisse Story 0.3 : Unity Cloud/Unity Gaming Services/Multiplayer Services Sessions/Relay retires, remplaces par Steamworks (AppID de test 480, lobby prive `ISteamMatchmaking`, Networking Sockets/Steam Datagram Relay, invite Steam natif ou Lobby ID). Declenche par une contrainte financiere solo-dev (pas de revenu recurrent) identifiee par l'utilisateur et confirmee apres recherche de precedents (Lethal Company, How to Fish, Meccha Chameleon utilisent tous un modele equivalent gratuit). Remplacement propre : aucune preuve VAL-012 a VAL-015 n'existait encore sur l'ancien chemin, donc aucun rollback necessaire.
- 2026-09-03 : Passage d'implementation locale demande par l'utilisateur. Ajoute `steam_appid.txt` avec `480`, un helper Unity Editor `RoadRageSteamworksSmokeTest` exposant le menu `RoadRage > Steamworks > Run AppID 480 Smoke Test`, et une synchronisation des documents de suivi. `VAL-007` passe `Pass` sur preuve locale du transport Facepunch resolu ; `VAL-012` a `VAL-015` restent `In Progress` car l'utilisateur doit encore lancer Unity avec Steam ouvert et fournir les preuves caviardees.

## Notes De Design

Story 0.3 doit rester une readiness story : elle peut produire un tutoriel, des pointeurs de preuve et des notes de validation, mais elle ne doit pas implementer le futur lobby UI gameplay d'Epic 2. Les preuves de smoke tests finaux restent couvertes par Story 0.8, sauf si une preuve fournie ici est utile comme note preparatoire. Le compte Steamworks reel et son fee unique (~100$) restent un prerequis avant toute sortie publique, pas un blocage pour le developpement (l'AppID 480 couvre ce besoin en attendant).

## Verification

**Commandes :**
- `Test-Path docs/setup/story-0-3-unity-cloud-services-tutorial.md` -- attendu : `True` apres implementation.
- `Test-Path steam_appid.txt` -- attendu : `True`.
- `$appid = (Get-Content -LiteralPath 'steam_appid.txt' -Raw).Trim(); if ($appid -ne '480') { throw "steam_appid.txt doit contenir 480" }` -- attendu : aucune erreur.
- `Test-Path Assets/Editor/RoadRageSteamworksSmokeTest.cs` -- attendu : `True`.
- `$smoke = Get-Content -LiteralPath 'Assets/Editor/RoadRageSteamworksSmokeTest.cs' -Raw; foreach ($needle in 'SteamClient.Init(TestSteamAppId, false)','SteamNetworkingUtils.InitRelayNetworkAccess','RoadRage/Steamworks/Run AppID 480 Smoke Test') { if ($smoke -notmatch [regex]::Escape($needle)) { throw "helper smoke test manque $needle" } }` -- attendu : aucune erreur.
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; if ($log -notmatch "\|\s*VAL-007\s*\|\s*``Pass``") { throw 'VAL-007 doit etre Pass apres installation du transport Facepunch' }` -- attendu : aucune erreur.
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; foreach ($id in 'VAL-012','VAL-013','VAL-014','VAL-015') { if ($log -notmatch "\|\s*$id\s*\|") { throw "$id manquant" } }` -- attendu : aucune erreur.
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; foreach ($id in 'VAL-012','VAL-013','VAL-014','VAL-015') { if ($log -notmatch "\|\s*$id\s*\|\s*``(Not Started|In Progress)``") { throw "$id ne doit pas etre Pass sans preuve" } }` -- attendu : aucune erreur.
- `$yaml = Get-Content -LiteralPath '_bmad-output/implementation-artifacts/sprint-status.yaml' -Raw; if ($yaml -notmatch '(?m)^  0-3-unity-cloud-services-lobby-and-relay-readiness: review$') { throw 'sprint-status story 0.3 doit etre review' }` -- attendu : aucune erreur.
- `$t = Get-Content -LiteralPath 'docs/setup/story-0-3-unity-cloud-services-tutorial.md' -Raw; foreach ($needle in '480','Spacewar','SteamClient.Init','ISteamMatchmaking','Networking Sockets','[REDACTED_TOKEN]','MaxPlayers = 4','Arret obligatoire avant Story 0.4','Story 2.2','Story 2.3','VAL-029','room pleine','session expiree') { if ($t -notmatch [regex]::Escape($needle)) { throw "tutoriel manque $needle" } }` -- attendu : aucune erreur.

**Tentative non comptabilisee :**
- Unity batchmode a ete lance pour verifier la compilation du helper, mais Unity a quitte pendant la phase licence avec `Access token is unavailable` avant compilation verifiable. Le log local a ete supprime car il contenait des identifiants machine/licence ; la validation Unity reste donc a faire manuellement via l'Editor.
