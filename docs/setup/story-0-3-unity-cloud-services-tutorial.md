# Tutoriel Story 0.3 : Steamworks, lobby et readiness Networking Sockets

Ce tutoriel est le chemin manuel pour executer la Story 0.3. Il couvre la configuration Steamworks du projet `RRS` via l'AppID de test officiel Valve `480` (Spacewar), l'initialisation du SDK Steamworks, la readiness lobby prive (`ISteamMatchmaking`), Networking Sockets (Steam Datagram Relay), `MaxPlayers = 4`, invite Steam/Lobby ID et exigences d'erreurs visibles. L'agent relit les preuves apres coup ; il ne cree pas de compte, n'active pas de service payant, ne stocke pas de secret et ne marque aucune ligne `Pass` sans preuve reelle.

**Contexte du changement :** cette story remplace l'approche initiale (Unity Cloud/Unity Gaming Services/Multiplayer Services Sessions/Relay) suite a une course-correction documentee dans `_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md`. Raison : Unity Relay/Multiplayer Services facture a l'usage au-dela d'un palier gratuit, ce qui est incompatible avec un jeu sans revenu recurrent. Steamworks Networking Sockets (Steam Datagram Relay) est gratuit quel que soit le nombre de joueurs simultanes -- seul un fee unique (~100$, non recurrent) est du a Valve avant toute sortie publique, pas avant ou pendant le developpement.

## Sources Steamworks consultees

- Steamworks API Overview : https://partner.steamgames.com/doc/sdk/api
- Steamworks API Example Application (SpaceWar, AppID 480) : https://partner.steamgames.com/doc/sdk/api/example
- Steam Networking (vue d'ensemble) : https://partner.steamgames.com/doc/features/multiplayer/networking
- Steam Datagram Relay : https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay
- ISteamNetworkingSockets Interface : https://partner.steamgames.com/doc/api/ISteamnetworkingSockets
- Transport communautaire Facepunch pour Netcode for GameObjects : https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/Transports/com.community.netcode.transport.facepunch
- Transport communautaire Steam Networking Sockets pour Netcode for GameObjects : https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/Transports/com.community.netcode.transport.steamnetworkingsockets

## Avant de commencer

- Confirme que Story 0.2 (revisee) est terminee : `VAL-002` a `VAL-011` doivent etre `Pass` dans `docs/setup/tooling-validation-log.md`, et `Packages/manifest.json` plus `Packages/packages-lock.json` doivent montrer Netcode for GameObjects `2.13.2`, Unity Transport `6.6.0`, Multiplayer Play Mode `3.0.0`, et le transport Steamworks (`com.community.netcode.transport.facepunch` ou `.steamnetworkingsockets`) installe via git URL.
- Utilise uniquement les statuts `Not Started`, `In Progress`, `Pass`, `Blocked`, `Not Applicable`.
- Travaille avec l'AppID de test officiel Valve `480` (Spacewar) pour tout le developpement et les tests -- aucun compte Steamworks reel ni fee unique ne sont necessaires a ce stade.
- Caviarde tout token, API key, credential, Lobby ID complet et invite utilisable avec `[REDACTED_TOKEN]`.
- Reference les preuves avec un chemin, une capture caviardee ou une note re-verifiable. Si un secret arrive dans le repo ou l'historique Git, arrete la validation, purge la preuve, fais tourner/revoque le secret concerne, puis documente la remediation sans recopier le secret.
- Si Steamworks demande un service payant, matchmaking public, dedicated server, host migration, native deep link OS, changement de cap joueurs, stockage de token, distribution hors-Steam pour le online, ou dependance externe non approuvee, arrete-toi et demande une approbation humaine.

**Preparation locale deja faite par l'agent (2026-09-03) :**

- `steam_appid.txt` existe a la racine du projet Unity et contient `480`.
- `Assets/Editor/RoadRageSteamworksSmokeTest.cs` ajoute le menu Unity `RoadRage > Steamworks > Run AppID 480 Smoke Test`.
- Le helper appelle `SteamClient.Init(480, false)` puis `SteamNetworkingUtils.InitRelayNetworkAccess()` sans logger de Steam ID, Lobby ID, invite ou credential.
- Cette preparation ne prouve pas encore que Steam fonctionne sur ta machine : VAL-012 reste `In Progress` tant que tu n'as pas lance le menu dans Unity avec Steam ouvert et conserve un log Console caviarde.

## Etape 1 -- Configurer l'AppID de test Steamworks (480 / Spacewar)

1. A la racine du projet Unity `RRS`, verifie que `steam_appid.txt` existe et contient une seule ligne : `480`. Si tu testes un build Windows separe plus tard, copie aussi ce fichier a la racine du dossier du build.
2. Assure-toi que le client Steam est installe et connecte avec un compte Steam valide (ton compte de developpement) avant de lancer le projet depuis Unity Editor.
3. Ouvre le projet `RRS` dans Unity Editor et confirme que le transport Steamworks (installe en Story 0.2 revisee) est present dans `Packages/manifest.json`.
4. Dans Unity Editor, utilise le helper deja present : menu `RoadRage > Steamworks > Run AppID 480 Smoke Test`. Il initialise Steamworks uniquement pour le smoke test puis ferme le client initialise par le test ; ne cable pas encore le flux `Bootstrap`/`MainMenuLobby` complet (ca reste Story 0.4/Epic 2).
5. Lance le menu avec Steam ouvert et confirme dans la Console Unity que `SteamClient.Init` reussit sans exception.

**Preuve a fournir (VAL-012 -- Configuration AppID test Steamworks) :** capture ou note montrant `steam_appid.txt` avec `480`, le menu `RoadRage > Steamworks > Run AppID 480 Smoke Test`, et un log Console confirmant `SteamClient.Init` reussi. Caviarde tout identifiant de compte Steam sensible. Tant que cette preuve n'existe pas, VAL-012 reste `In Progress`.

**Cas limite -- Steam indisponible :** si le client Steam n'est pas installe, pas connecte, ou si `SteamClient.Init` echoue de maniere repetee, marque VAL-012 `Blocked`, note l'erreur exacte rencontree et l'impact.

## Etape 2 -- Confirmer le transport Networking Sockets

1. Verifie que le transport Steamworks choisi en Story 0.2 revisee (`com.community.netcode.transport.facepunch` ou `com.community.netcode.transport.steamnetworkingsockets`) est present dans `Packages/manifest.json` et `Packages/packages-lock.json`. Ne marque pas encore "transport assigne au NetworkManager" tant que le `NetworkManager` reel n'existe pas.
2. Confirme dans la documentation ou le code du transport choisi que Networking Sockets (Steam Datagram Relay) est bien le chemin reseau utilise -- pas de connexion directe IP uniquement. Pour le transport Facepunch installe ici, `Library/PackageCache/com.community.netcode.transport.facepunch@27d3e825ecdd/Runtime/FacepunchTransport.cs` appelle `SteamNetworkingUtils.InitRelayNetworkAccess()`, `SteamNetworkingSockets.ConnectRelay(...)` et `SteamNetworkingSockets.CreateRelaySocket(...)`.
3. N'active pas public matchmaking, dedicated servers, host migration ou un palier payant sans approbation humaine explicite.

**Preuve a fournir (VAL-013 -- Lobby Steam et Networking Sockets) :** captures ou notes montrant le transport installe, la reference a Networking Sockets/Steam Datagram Relay dans sa documentation ou son code, puis plus tard le transport assigne au `NetworkManager` quand il existe. Si possible, ajoute un test de creation de lobby minimal (`ISteamMatchmaking.CreateLobby` ou l'appel equivalent du transport) reussi en environnement AppID 480. Caviarde tout identifiant ou Lobby ID utilisable. Tant que cette preuve runtime n'existe pas, VAL-013 reste `In Progress`.

**Cas limite -- service indisponible :** si Networking Sockets ou la creation de lobby echoue de maniere repetee, marque VAL-013 `Blocked`, note la documentation consultee, l'option manquante et l'impact sur les stories online.

## Etape 3 -- Capturer les exigences session privee, Networking Sockets et Lobby ID

Story 0.3 enregistre la readiness et les exigences ; elle n'implemente pas encore le lobby UI gameplay. Le futur flux host doit creer un lobby prive via `ISteamMatchmaking`, utiliser Networking Sockets sans port forwarding routeur cote host et exposer un Lobby ID aux clients invites.

Note explicitement ces exigences :

- L'host cree un lobby prive (`ISteamMatchmaking.CreateLobby` avec visibilite `Private` ou equivalent).
- Networking Sockets est le chemin reseau attendu ; aucun port forwarding routeur host n'est requis.
- Le Lobby ID est considere sensible dans les docs/prompts et doit etre caviarde.
- Le futur champ Lobby ID doit trim les espaces, normaliser le format attendu, refuser champ vide et caracteres invalides, et afficher des erreurs visibles pour code invalide, room pleine ou session expiree.
- `MaxPlayers = 4`, host inclus.
- Une tentative de cinquieme joueur doit etre refusee avec une erreur UI visible.

**Preuve a fournir (VAL-013 et VAL-014) :** notes de smoke test, captures de settings ou notes d'implementation caviardees qui prouvent ou bloquent le lobby prive host-created, Networking Sockets, le flow Lobby ID, l'absence de port forwarding et `MaxPlayers = 4`. Remplace le Lobby ID complet par `[REDACTED_TOKEN]`. Si aucun flux host runnable n'existe encore, garde la ligne concernee `In Progress` et indique que Story 2.2 doit prouver le host-created private room flow, Story 2.3 le join-by-Lobby-ID/invite flow, et Story 0.8/VAL-029 le rejet effectif du cinquieme joueur.

**Cas limite -- secret dans une preuve :** si une note ou capture contient un Lobby ID complet, token, credential, API key ou invite utilisable, rejette la preuve jusqu'au caviardage. Ne copie jamais le secret original dans `tooling-validation-log.md`.

## Etape 4 -- Definir l'invite Steam et le Lobby ID comme chemins de secours

L'invitation principale passe par l'overlay Steam natif (invite ami depuis le menu Steam). Le Lobby ID reste un chemin de secours partageable manuellement, sans devenir un native deep link OS, un handler de compte ou un systeme de matchmaking separe dans Story 0.3.

Documente ce futur comportement :

- L'UI d'invitation peut ouvrir l'overlay Steam natif pour inviter un ami directement.
- En secours, l'UI peut copier, afficher ou partager un Lobby ID qui porte la meme intention que l'invite Steam.
- Le Lobby ID partage ne doit pas etre expose en clair dans les docs ou prompts.
- Format autorise dans les docs : nom de session lisible, environnement de test AppID 480, placeholder `[REDACTED_TOKEN]` pour le Lobby ID et optionnellement un display name non sensible.
- Format interdit : credential, token, detail Steamworks, invite/Lobby ID complet utilisable ou protocole OS/deep link natif.
- Test futur attendu : le Lobby ID permet de retrouver le meme lobby caviarde pour le flux Story 2.3, sans handler natif ni matchmaking public.
- Les deep links natifs OS restent differes jusqu'a approbation et story dediee.

**Preuve a fournir (VAL-015 -- Invite Steam / Lobby ID) :** note caviardee, capture design ou note de setting montrant que l'invitation utilise l'overlay Steam natif ou le Lobby ID comme secours. Remplace tout identifiant utilisable par `[REDACTED_TOKEN]`. Tant que cette preuve n'existe pas, VAL-015 reste `Not Started` ou `In Progress`.

## Etape 5 -- Capturer les exigences d'erreurs Lobby/UI visibles

Note ces exigences maintenant pour que l'Epic 2 ne les decouvre pas trop tard. La future UI devra montrer un feedback visible pour :

- Creation de room privee : succes et echec.
- Join par Lobby ID ou invite : succes, champ vide, espaces a trim, format normalise, caracteres invalides, code invalide, room pleine, session expiree et echec service.
- Leave room par joueur non-host.
- Host close ou host quit.
- Expiration de session.
- Nettoyage de room abandonnee.
- Retour au menu apres service failure, host quit, disconnect ou session loss.
- Networking Sockets failure.
- Steam indisponible ou authentication failure.
- Cinquieme joueur refuse quand `MaxPlayers = 4`.

Ces notes alimentent VAL-032 et Epic 2. Elles ne remplacent pas les preuves finales de smoke tests Story 0.8.

## Etape 6 -- Mettre a jour les documents de suivi

Apres chaque vraie action manuelle ou chaque bloqueur, mets a jour `docs/setup/tooling-validation-log.md` :

| ID | Statut avant preuve reelle | Preuve attendue |
| --- | --- | --- |
| VAL-012 | `Not Started` ou `In Progress` | Configuration AppID 480 et `SteamClient.Init` reussi, caviardes. |
| VAL-013 | `Not Started` ou `In Progress` | Transport et Networking Sockets actifs maintenant ; lobby prive host-created et Lobby-ID flow seront prouves par Story 2.2, Story 2.3 et Story 0.8 si aucun flux runnable n'existe encore. |
| VAL-014 | `Not Started` ou `In Progress` | `MaxPlayers = 4`, host inclus, exigence de cinquieme joueur refuse maintenant ; preuve effective via VAL-029/Story 0.8. |
| VAL-015 | `Not Started` ou `In Progress` | Invite Steam natif ou Lobby ID de secours, sans claim de native deep link OS. |
| VAL-032 | `In Progress` | Exigences d'erreurs Lobby/UI capturees pour create, join, leave, host close, expiration, abandoned cleanup, return-to-menu, Networking Sockets, service, disconnect et host quit. |

Apres tout changement de `VAL-012` a `VAL-015`, mets aussi a jour `docs/setup/epic-0-readiness-checklist.md` et `_bmad-output/implementation-artifacts/sprint-status.yaml`.

Une ligne passe en `Pass` seulement si date, acteur, commande/chemin UI, chemin/resume de preuve caviardee, validateur et resultat sont tous presents et re-verifiables.

## Avant sortie publique (hors scope Story 0.3)

Un compte Steamworks reel et son fee unique (~100$, non recurrent, pas lie au nombre de joueurs) seront necessaires avant toute sortie publique du jeu, pour obtenir un AppID definitif remplacant `480`. Ce n'est pas un blocage pour le developpement ou pour cette story -- c'est un futur item a traiter separement, note ici pour ne pas etre oublie.

## Arret obligatoire avant Story 0.4

Arrete-toi ici apres preparation des preuves et notes Story 0.3. Ne commence pas Story 0.4 (structure projet, scenes, namespaces, runtime state), le lobby UI gameplay, matchmaking, host migration, dedicated servers, native deep links, distribution hors-Steam ou Epic 1 depuis cette story.
