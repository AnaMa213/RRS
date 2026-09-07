# Tutoriel Story 0.8 : Smoke tests finaux Epic 0 et decision go/no-go

Ce tutoriel est le chemin manuel unique pour rejouer ou completer les smoke tests Story 0.8. Il documente le harnais `NetworkManager` minimal ajoute a `Dev_LobbySmokeTest.unity`, la preuve VAL-027 maintenant `Pass` (agent + utilisateur), ce qui reste a valider plus tard pour VAL-028, et pourquoi VAL-029 a VAL-032 restent `Blocked` et differes vers des stories Epic 2 nommees. Meme discipline de preuve que les tutoriels Story 0.5/0.6 : jamais `Pass` sans preuve reelle re-verifiable.

## Sources consultees

- AC source de cette story : `_bmad-output/planning-artifacts/epics.md:423-438` (Story 0.8).
- Spec Story 0.8 : `_bmad-output/implementation-artifacts/spec-0-8-epic-0-smoke-tests-and-go-no-go-gate.md`.
- Journal de validation : `docs/setup/tooling-validation-log.md:56-62` (lignes VAL-027 a VAL-033).
- Checklist readiness : `docs/setup/epic-0-readiness-checklist.md:53,69,71-75`.
- Exigences Lobby/UI deja capturees par Story 0.3 : `docs/setup/story-0-3-unity-cloud-services-tutorial.md` etape 5.
- Stories Epic 2 referencees pour les bloqueurs differes : `_bmad-output/planning-artifacts/epics.md:569-585` (Story 2.2), `:587-604` (Story 2.3), `:606-621` (Story 2.4), `:657-673` (Story 2.7).
- Transport Steamworks deja verrouille : `Packages/com.community.netcode.transport.facepunch/Runtime/FacepunchTransport.cs` (VAL-007).
- Patron AppID test : `Assets/Editor/RoadRageSteamworksSmokeTest.cs` (VAL-012).

## Avant de commencer

- Utilise uniquement les statuts `Not Started`, `In Progress`, `Pass`, `Blocked`, `Not Applicable`.
- Caviarde tout Lobby ID, invite, token Steamworks/Relay, credential ou identifiant sensible avec `[REDACTED_TOKEN]` avant de coller une capture ou une note ici ou dans `tooling-validation-log.md`.
- Le harnais `NetworkManager` de `Dev_LobbySmokeTest.unity` ne doit jamais referencer de composant gameplay (`NetworkedRunState`, `RunCompositionRoot`, etc.) -- verifie apres toute modification avec `rg -n "NetworkedRunState|RunCompositionRoot|NetworkedPlayerState|NetworkedAIVehicleState|NetworkedRageState|NetworkedCrewEconomyState|NetworkedBossState" Assets/RoadRage/App/Scenes/Dev_LobbySmokeTest.unity` (aucune sortie attendue).
- **Demander d'abord :** avant de tenter VAL-028, confirme qu'un second joueur/machine Steam reel est disponible maintenant. Si non, la ligne reste `Blocked`/differee -- ne la tente jamais sans cette confirmation.
- **Jamais :** construire la lobby UI, la gestion de session (cap 4 joueurs, host quit, disconnect) ou toute feature gameplay depuis cette story (perimetre Epic 2) ; marquer VAL-027/VAL-028 `Pass` sans preuve live temoignee par l'agent ou l'utilisateur ; activer `Dev_LobbySmokeTest` dans les Build Settings.

## Le harnais `NetworkManager` (deja en place)

`Assets/RoadRage/App/Scenes/Dev_LobbySmokeTest.unity` contient maintenant un `GameObject` `NetworkManager` avec trois composants :

| Composant | Role | Etat par defaut |
| --- | --- | --- |
| `Unity.Netcode.NetworkManager` | Composant Netcode for GameObjects standard, `NetworkConfig` par defaut (`TickRate=30`, `ConnectionApproval=false`, liste de prefabs reseau vide) | `NetworkConfig.NetworkTransport` pointe sur le composant `UnityTransport` du meme `GameObject` |
| `Unity.Netcode.Transports.UTP.UnityTransport` | Transport local (VAL-008), utilise pour VAL-027 | Actif par defaut |
| `Netcode.Transports.Facepunch.FacepunchTransport` | Transport Steamworks (VAL-007), utilise pour VAL-028 | Present mais inactif tant que `NetworkConfig.NetworkTransport` n'y est pas reassigne manuellement |

**Aucun composant gameplay n'est attache.** La scene reste `enabled: false` dans `ProjectSettings/EditorBuildSettings.asset`, comme les autres scenes `Dev_*`.

**Bascule de transport avant chaque smoke test :** un seul transport est actif a la fois via `NetworkConfig.NetworkTransport`. Avant VAL-027, verifie qu'il pointe sur `UnityTransport`. Avant VAL-028, reassigne-le manuellement sur `FacepunchTransport` dans l'Inspector (glisser le composant `FacepunchTransport` du meme `GameObject` dans le champ `Network Transport` du composant `NetworkManager`), puis remets-le sur `UnityTransport` apres le test si tu comptes retenter VAL-027. Ne laisse jamais les deux actifs simultanement.

## VAL-027 -- Smoke test multiplayer local

### Ce que l'agent a deja verifie en direct (2026-09-07)

Dans cette meme session, l'agent a :

1. Ouvert `Dev_LobbySmokeTest.unity`, ajoute le harnais ci-dessus, verifie l'absence de composant gameplay, confirme la scene `enabled: false` dans les Build Settings, et sauvegarde.
2. Lance le Play Mode reel de l'Editor sur cette scene.
3. Execute `NetworkManager.Singleton.StartHost()` via un script Editor (`Unity_RunCommand`) : `StartHost()` retourne `true`, `IsListening=True`, `IsServer=True`, `IsHost=True`.
4. Verifie l'etat apres connexion : `IsConnectedClient=True`, `LocalClientId=0`, `ConnectedClientsIds=[0]` -- le client du host s'est reellement connecte au serveur du host via `UnityTransport`.
5. Verifie la console Unity : 0 erreur, 0 warning.
6. Nettoye : `Shutdown()` du `NetworkManager`, sortie du Play Mode, verifie que la scene revient a un etat propre (`isDirty=false`, meme nombre d'objets racine qu'avant, `git status` ne montre que le harnais ajoute).
7. Tente d'ajouter un second `NetworkManager` dans le meme processus pour simuler un deuxieme acteur : bloque a la compilation (`NetworkManager.Singleton` est en lecture seule cote script public). Confirme que Netcode for GameObjects impose un seul `NetworkManager` actif par processus -- un deuxieme acteur reel exige un deuxieme processus, exactement ce que fournit la fonctionnalite Unity **Multiplayer Play Mode**.

**Ce que cela prouve :** le harnais est reellement fonctionnel (le `NetworkManager` + `UnityTransport` de la scene demarrent, ecoutent et acceptent une connexion client reelle). **Ce que cela ne prouve pas :** une connexion entre deux acteurs distincts (host + un vrai second joueur/processus), ce que l'AC Story 0.8 et l'I/O matrix de la spec exigent litteralement ("Multiplayer Play Mode avec 1 joueur virtuel + host").

### Ce qui reste une action manuelle utilisateur

Le module Multiplayer Play Mode (`com.unity.multiplayer.playmode` `3.0.0`, VAL-009) pilote un deuxieme processus Editor (un "virtual player") depuis une fenetre GUI -- cette interaction ne peut pas etre executee par l'agent depuis cette session autonome sans risquer de perturber une session Editor live (voir aussi le cas limite ci-dessous, deja rencontre avec `Dev_VehicleSandbox` pendant cette meme session : sauvegarder une scene deja ouverte avec un etat memoire different du disque peut ecraser du contenu commit par erreur).

VAL-027 a ete complete par l'utilisateur le 2026-09-07 avec une capture Console Unity `RoadRageNetcodeSmokeTest` montrant `IsListening=True`, `IsServer=True`, `IsHost=True`, `IsConnectedClient=True`, `LocalClientId=0` et `ConnectedClientsCount=2` apres lancement host + virtual player `Player 2`. Pour rejouer ce test en regression :

1. Ouvre `Dev_LobbySmokeTest.unity` dans Unity (`File > Open Scene`).
2. Verifie que `NetworkConfig.NetworkTransport` du `NetworkManager` pointe bien sur `UnityTransport` (voir section precedente).
3. Ouvre `Window > Multiplayer Play Mode`.
4. Ajoute un virtual player (`+ Add a Player Tag` ou equivalent selon la version), active-le.
5. Lance le Play Mode sur l'Editor principal (Main Editor) **et** sur le virtual player.
6. Dans l'Editor principal, appelle `NetworkManager.Singleton.StartHost()` (bouton de test, menu, ou Console via un script temporaire) ; dans le virtual player, appelle `NetworkManager.Singleton.StartClient()` avec l'adresse `127.0.0.1`.
7. Confirme visuellement (Inspector du `NetworkManager` sur chaque instance, ou logs Console) que le host affiche deux clients connectes (`ConnectedClientsIds.Count == 2`) et que le virtual player affiche `IsConnectedClient = true`.
8. Capture ou note ce resultat (nombres de clients connectes, absence d'erreur), caviarde tout identifiant sensible, puis fournis-la a l'agent si VAL-027 doit etre re-verifiee.

**Cas limite -- connexion echoue :** si le host ou le virtual player n'affiche pas de connexion reussie, VAL-027 reste `Blocked`, avec le message d'erreur console consigne dans `tooling-validation-log.md`.

## VAL-028 -- Smoke test Steam distant (deux joueurs)

**Demander d'abord, sans exception :** cette section ne doit etre suivie que si un second joueur ou une seconde machine Steam reelle est confirme disponible maintenant. Sinon, VAL-028 reste `Blocked`/differe -- ne la tente jamais sans cette confirmation prealable. Pendant cette session agent autonome, aucune confirmation de ce type n'a ete recueillie ; VAL-028 n'a donc pas ete tentee.

Si un second joueur reel est disponible :

1. Sur le `NetworkManager` de `Dev_LobbySmokeTest.unity`, reassigne `NetworkConfig.NetworkTransport` sur `FacepunchTransport`.
2. Assure-toi que Steam est ouvert et connecte sur les deux machines, `steam_appid.txt` (`480`) present (VAL-012).
3. Sur l'hote : `StartHost()`. Sur le client distant : `StartClient()` avec l'ID Steam de l'hote renseigne dans `FacepunchTransport.targetSteamId` (jamais colle en clair dans une note commitee -- caviarder `[REDACTED_TOKEN]`).
4. Confirme la connexion sur les deux machines (`IsConnectedClient=true` cote client, `ConnectedClientsIds.Count==2` cote host).
5. Caviarde toute preuve (Lobby ID, Steam ID) avant de la fournir a l'agent pour mise a jour de VAL-028.

**Cas limite -- aucun second joueur disponible :** VAL-028 reste `Blocked`/differee, jamais `Pass`.

## VAL-029 a VAL-032 -- Differes vers Epic 2

Ces quatre lignes exigent une UI lobby/session runtime (roster, ready state, gestion de cap, erreurs visibles) qui n'existe pas encore dans le projet -- c'est explicitement hors perimetre de Story 0.8 ("Jamais : construire la lobby UI, la gestion de session... perimetre Epic 2"). Chacune reste `Blocked` avec une story Epic 2 nommee :

| Ligne | Exigence | Story Epic 2 de reference |
| --- | --- | --- |
| VAL-029 | Rejet du cinquieme joueur (cap `MaxPlayers = 4`) | Story 2.3 -- Join By Code and Invite-Link Wrapper (`epics.md:587-604`, "concurrent joins cannot exceed the four-player cap") |
| VAL-030 | Gestion host quit (retour `MainMenuLobby` avec erreur visible) | Story 2.7 -- Player Lifecycle, Disconnect, and Host-Quit Handling (`epics.md:657-673`) |
| VAL-031 | Disconnect joueur non-host (retrait propre, pas de host quit ni team wipe invalide) | Story 2.7 -- Player Lifecycle, Disconnect, and Host-Quit Handling (`epics.md:657-673`) |
| VAL-032 | Erreurs Lobby/UI visibles (create, join, Networking Sockets, disconnect, service, host quit, room full, expiration) | Story 2.2 -- Host-Created Private Room (`epics.md:569-585`), Story 2.3 (`epics.md:587-604`), Story 2.4 -- Lobby Roster, Ready State, and Settings Sync (`epics.md:606-621`), Story 2.7 (`epics.md:657-673`) |

Ces references restent coherentes avec la note deja posee par Story 0.3 sur VAL-032 ("les preuves de smoke tests restent a fournir plus tard").

## VAL-033 -- Decision finale Epic 0

Avec VAL-027 `Pass`, VAL-028 `Blocked`/differee (aucun second joueur confirme) et VAL-029 a VAL-032 `Blocked` (differes vers Epic 2), le gate final n'est pas un `Pass` plein. La spec Story 0.8 place explicitement le choix entre `Blocked` et `Accepted With Known Blockers` dans la section "Demander d'abord" -- **cette decision reste humaine**, pas automatisee par l'agent.

Le 2026-09-07, l'utilisateur a confirme explicitement que VAL-027 peut etre cloturee, que VAL-028 sera validee plus tard, et que l'Epic 0 peut etre cloturee avec des bloqueurs connus. VAL-033 et le gate Epic 1 (`docs/setup/epic-0-readiness-checklist.md`, section "Gate go/no-go Epic 1") sont donc synchronises sur la decision finale `Accepted With Known Blockers`.

## Rappel de la regle de caviardage

Reutilise la regle `[REDACTED_TOKEN]` des Stories 0.3/0.5/0.6 pour tout Lobby ID, Steam ID, invite ou identifiant sensible visible dans une capture, un log ou une note liee aux smoke tests VAL-027/VAL-028. Ne colle jamais de secret original dans ce tutoriel ni dans `tooling-validation-log.md`.

## Mettre a jour les documents de suivi

Apres chaque vraie action manuelle ou chaque bloqueur, mets a jour dans la meme passe coherente :

- `docs/setup/tooling-validation-log.md` -- VAL-027 a VAL-033 (lignes principales et notes datees dans "Notes de validation"), jamais `Pass` sans preuve reproductible.
- `docs/setup/epic-0-readiness-checklist.md` -- lignes Story 0.8 (`:53`, `:69`) et section "Gate go/no-go Epic 1" (`:71-75`), synchronisees avec le statut reel de VAL-027 a VAL-033.
- `_bmad-output/implementation-artifacts/sprint-status.yaml` -- garder Story 0.8 et `epic-0` en `done` une fois la decision finale documentee.

## Arret obligatoire

Ne construis pas la lobby UI, la gestion de session ou toute feature gameplay depuis cette story (perimetre Epic 2). Le gameplay Epic 1 peut commencer uniquement parce que le gate go/no-go est maintenant `Accepted With Known Blockers` avec confirmation humaine documentee ; ne masque pas les validations restantes.
