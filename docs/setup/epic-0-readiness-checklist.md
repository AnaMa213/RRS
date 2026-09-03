# Checklist de readiness Epic 0

Cette checklist est le chemin de setup unique pour l'Epic 0. Elle sert a installer et verifier l'environnement avant de commencer le developpement gameplay. Garde les actions manuelles separees des validations agent, ajoute une preuve avant de passer une ligne externe en `Pass`, et documente les bloqueurs avant toute decision de demarrer l'Epic 1.

## Vocabulaire de statut partage

Utilise exactement ces statuts dans cette checklist et dans les documents compagnons.

| Statut | Signification |
| --- | --- |
| `Not Started` | Aucune action ou validation confirmee n'a commence. |
| `In Progress` | Le travail a commence, mais les preuves ou la revue sont incompletes. |
| `Pass` | L'action ou la validation a reussi et la preuve est liee ou resumee. |
| `Blocked` | Le travail ne peut pas continuer tant que le bloqueur n'est pas resolu ou accepte. |
| `Not Applicable` | La ligne ne s'applique pas a ce chemin de setup ; noter pourquoi. |

## Baseline workspace actuelle

| Element | Statut | Preuve | Impact / prochaine action |
| --- | --- | --- | --- |
| Artefacts de suivi setup | `Pass` | `docs/setup/` cree pour Story 0.1 et verifie avec `Test-Path` et `rg` | Les trois fichiers cibles existent et partagent le vocabulaire de statut requis. |
| Dossiers projet Unity | `Pass` | `Assets/`, `Packages/`, `ProjectSettings/`, `Packages/manifest.json` et `Packages/packages-lock.json` existent au controle 2026-09-02 | La baseline Unity est presente ; continuer les validations via `tooling-validation-log.md`. |
| Nom du projet Unity | `Pass` | Capture Unity Hub Projects : projet `RRS` a `D:\Projets\RRS` | Nom conforme au tutoriel Story 0.2 : `RRS` (RoadRageSimulator). |
| Gate Epic 1 | `Blocked` | Epic 0 encore en cours ; Story 0.2 cloturee ; Story 0.3 preparee localement (`steam_appid.txt`, helper smoke test Steamworks, transport Facepunch verifie) mais preuves Unity/Steam manuelles non fournies ; Stories 0.4 a 0.8 encore ouvertes | Epic 1 reste bloquee tant que l'Epic 0 n'est pas marquee `Pass` ou explicitement acceptee avec bloqueurs documentes. |

Les dossiers Unity sont maintenant presents. Les validations suivantes passent seulement avec les preuves demandees dans `docs/setup/tooling-validation-log.md` ; tout mismatch de version reste bloque ou explicitement accepte avant Epic 1.

## Declencheur de re-baseline apres creation Unity

Des que le projet Universal 3D/URP `RRS` est cree manuellement, relancer une baseline avant toute autre story :

| Etape | Responsable | Statut initial | Critere de completion |
| --- | --- | --- | --- |
| Recontroler les dossiers racine Unity | Agent | `Pass` | `Assets/`, `Packages/`, `ProjectSettings/`, `Packages/manifest.json` et `Packages/packages-lock.json` existent et sont notes dans `tooling-validation-log.md`. |
| Recontroler la version Unity et le template | Utilisateur puis agent | `Pass` | Unity `6000.6.0f1` et template Universal 3D/URP confirmes par capture, `ProjectVersion.txt` et assets/settings URP. |
| Recontroler les package pins | Agent | `Pass` | Les sept packages verrouilles sont conformes, avec Unity Transport `6.6.0` approuve comme nouveau verrou pour Unity `6000.6.0f1`. |
| Mettre a jour cette checklist | Agent | `Pass` | Les lignes Story 0.2 refletent VAL-002 a VAL-011 en `Pass`. |

## Actions manuelles utilisateur

Tu realises les installations GUI, la configuration de comptes, les changements Unity Dashboard, les approbations et les smoke tests. Les agents peuvent guider les etapes et valider les preuves, mais ils ne doivent jamais declarer ces etapes reussies sans preuve.

| Story | Statut | Action manuelle | Preuve a collecter | Critere de completion | Notes bloqueur |
| --- | --- | --- | --- | --- | --- |
| 0.1 Baseline readiness setup | `In Progress` | Relire ces documents de setup et confirmer qu'ils correspondent au workflow Epic 0 voulu. | Chemins des fichiers finaux et sortie des commandes de verification, plus confirmation explicite de l'utilisateur. | Les trois artefacts existent, contiennent le vocabulaire de statut partage, separent actions manuelles et validations agent, et exposent le gate Epic 1. | Artefacts crees et verifies par l'agent ; confirmation manuelle de l'utilisateur encore en attente. |
| 0.2 Unity editor et projet | `Pass` | Suivre `docs/setup/story-0-2-unity-install-tutorial.md` (etapes 1 a 3) : installer Unity Hub, installer Unity `6000.6.0f1` sur la track Unity 6 Update, puis creer un projet Universal 3D/URP nomme `RRS`. | Capture ou note montrant version editor, template projet et chemin local, telles que demandees a chaque etape du tutoriel. | Les dossiers `Assets/`, `Packages/`, `ProjectSettings/`, `Packages/manifest.json` et `Packages/packages-lock.json` existent ; VAL-002, VAL-003 et VAL-004 sont `Pass`. | Story 0.2 editor/projet cloturee. |
| 0.2 Packages verrouilles | `Pass` | Suivre `docs/setup/story-0-2-unity-install-tutorial.md` (etape 4) : installer ou confirmer via Package Manager : Universal Render Pipeline `17.6.0`, Netcode for GameObjects `2.13.2`, transport Steamworks Facepunch via git URL, Unity Transport `6.6.0`, Multiplayer Play Mode `3.0.0`, Input System `1.20.0` et Cinemachine `6.6.0`. | `Packages/manifest.json`, `Packages/packages-lock.json` et `Packages/com.community.netcode.transport.facepunch/package.json`. | Tous les packages verrouilles sont presents aux versions/commits attendus ou chaque ecart est documente et accepte avant Epic 1. | VAL-005 a VAL-011 passent ; VAL-007 passe le 2026-09-03 avec le transport Facepunch resolu et patch local du `#endregion` en trop. |
| 0.3 Steamworks et Networking Sockets | `In Progress` | Suivre `docs/setup/story-0-3-unity-cloud-services-tutorial.md` : verifier `steam_appid.txt`, ouvrir Steam, lancer dans Unity `RoadRage > Steamworks > Run AppID 480 Smoke Test`, puis confirmer creation de lobby Steam, Networking Sockets et invite/Lobby ID readiness quand le runtime existe. | Notes ou captures caviardees pour VAL-012 a VAL-015, plus exigences VAL-032 ; aucun token, Lobby ID complet, credential ou invite utilisable. | Session privee host-created, Steamworks Networking Sockets, Lobby ID, `MaxPlayers = 4`, absence de port forwarding routeur host, invite Steam/Lobby ID et exigences d'erreurs Lobby/UI visibles sont prouves, bloques ou references vers Story 2.2, Story 2.3 et Story 0.8 quand le runtime n'existe pas encore. | Preparation locale faite par l'agent : AppID 480, helper smoke test et verification statique du transport. Pas de service payant, matchmaking public, dedicated server, host migration, native deep link, changement de cap joueurs, stockage de token ou dependance externe sans approbation. |
| 0.4 Structure projet et scenes | `Not Started` | Ouvrir le projet dans Unity et approuver la creation des scenes et dossiers initiaux quand Story 0.4 commence. | Fichiers Unity montrant `Bootstrap`, `MainMenuLobby`, `MVP_Run` et la structure `Assets/RoadRage`. | Scenes seed, structure `Assets/RoadRage`, namespaces `RoadRage.App`, `RoadRage.Shared`, `RoadRage.Features.<Feature>` et squelette runtime state sont inspectables. | Le gameplay attend Epic 1 ; Story 0.4 reste du scaffolding setup. |
| 0.5 Setup IA/MCP | `Not Started` | Configurer le Unity MCP et Blender MCP selectionnes dans le client choisi. Preferer Unity Official MCP, sinon CoplayDev Unity MCP epingle a un tag. Preferer Blender Lab MCP, sinon ahujasid Blender MCP. | Notes de configuration client et preuves de smoke tests sans danger. | Les choix MCP, fallbacks, chemins client Codex/Claude et smoke tests sans danger sont documentes sans changement non relu. | Ne pas lancer plusieurs bridges sur le meme editor sauf support explicite. |
| 0.6 Blender et asset intake | `Not Started` | Installer Blender `5.2 LTS` et passer par le nettoyage Blender avant qu'un asset 3D genere ou telecharge devienne un prefab Unity. | Preuve de version Blender et export test FBX/GLB apres smoke test. | Version Blender prouvee, fallback documente si bloque, export test realise, et checklist intake Blender prete pour tout asset 3D. | Ne pas importer d'asset tiers 3D dans Unity avant revue intake. |
| 0.7 Adoption add-on et assets | `Not Started` | Lister les candidats UI, menu, movement/controller, starter assets et add-ons dans le registre avant import. | Lignes de registre completes : licence, cout, compatibilite, maintenance, dependances, multiplayer, source, fit architecture. | Chaque candidat initial a une decision controlee et aucun import n'a lieu avant une ligne complete ou un blocage documente. | Evaluer d'abord, importer ensuite, wrapper/adapter ensuite, customiser en dernier. |
| 0.8 Smoke tests finaux et go/no-go | `Not Started` | Executer les smoke tests quand les stories setup sont terminees ou explicitement bloquees. | Resultat Multiplayer Play Mode local host/client, Steam distant deux joueurs, rejet du cinquieme joueur, host quit, disconnect non-host et erreurs Lobby/UI visibles. | Le gate final est `Pass`, `Blocked` ou accepte avec bloqueurs documentes ; Epic 1 ne demarre pas avant cette decision. | Epic 1 reste bloquee tant que le gate final n'est pas `Pass` ou accepte avec bloqueurs documentes. |

## Validations agent

Les agents valident les fichiers generes, lockfiles packages, settings et preuves que tu fournis. Ils ne creent pas de comptes, n'approuvent pas de services, n'ajoutent pas de secrets, n'installent pas d'outils GUI et n'inventent pas de succes de smoke test.

| Story | Statut | Validation agent | Preuve requise | Critere de completion | Notes resultat |
| --- | --- | --- | --- | --- | --- |
| 0.1 Baseline readiness setup | `Pass` | Confirmer que les trois artefacts setup existent et contiennent `Not Started`, `In Progress`, `Pass`, `Blocked` et `Not Applicable`. | Sorties `Test-Path` et `rg`. | Toutes les commandes de verification Story 0.1 passent et les prompts temporaires de review sont absents. | Validation des artefacts setup OK. |
| 0.2 Unity editor et projet | `Pass` | Controler `Assets/`, `Packages/manifest.json`, `Packages/packages-lock.json` et `ProjectSettings/` apres creation manuelle, en suivant les criteres de preuve de `docs/setup/story-0-2-unity-install-tutorial.md` (etapes 1 a 3). | Inspection filesystem ; preuves fournies pour VAL-002 a VAL-004 dans `tooling-validation-log.md`. | VAL-002, VAL-003 et VAL-004 sont `Pass`. | Projet, editor, Hub et licence locale valides. |
| 0.2 Packages verrouilles | `Pass` | Verifier les entrees exactes et les versions resolues du stack approuve, en suivant les criteres de preuve de `docs/setup/story-0-2-unity-install-tutorial.md` (etape 4). | `manifest.json` et `packages-lock.json` ; preuves fournies pour VAL-005 a VAL-011 dans `tooling-validation-log.md`. | Manifest et lockfile confirment les versions approuvees. | VAL-005 a VAL-011 passent ; Transport `6.6.0` est le verrou approuve. |
| 0.3 Steamworks services | `In Progress` | Relire les preuves caviardees produites depuis `docs/setup/story-0-3-unity-cloud-services-tutorial.md` : configuration AppID test 480, menu smoke test Unity, transport Steamworks, Networking Sockets, session privee host-created, Lobby ID path, `MaxPlayers = 4`, rejet du cinquieme joueur, absence de port forwarding routeur host, invite Steam/Lobby ID et VAL-032. | Capture caviardee, export settings ou notes de smoke test ; Lobby ID complets, details Steamworks, credentials, API keys, service tokens et invites utilisables remplaces par `[REDACTED_TOKEN]`. | Aucune preuve ne contient de secret ; chaque flux reseau attendu a une preuve reproductible, un bloqueur ou une story future nommee, et les exigences d'erreurs UI visibles sont capturees pour Epic 2. | Preparation locale agent terminee ; les agents valident maintenant les preuves utilisateur pour `SteamClient.Init`, lobby/runtime et invite. Les smoke tests locaux/distants finaux restent Story 0.8. |
| 0.4 Structure et squelette runtime | `Not Started` | Verifier dossiers `Assets/RoadRage`, scenes, namespaces, asmdefs et noms de squelettes host-owned runtime state. | Fichiers Unity et liste de scenes. | La structure respecte le seed et les limites d'assemblies sans commencer Epic 1. | Preserver `RoadRage.App`, `RoadRage.Shared` et `RoadRage.Features.<Feature>`. |
| 0.5 Configuration MCP | `Not Started` | Confirmer ordre de preference Unity/Blender MCP, fallback epingle, configuration client, smoke tests sans danger et actions interdites. | Notes de smoke tests Unity et Blender. | Les MCP ne changent pas versions, services payants, dedicated servers, secrets, scenes ou assets sans revue. | Les MCP sont des assistants controles, pas un autopilot. |
| 0.6 Asset intake | `Not Started` | Confirmer source Blender, export FBX/GLB, test echelle, revue materiaux, plan collider et stabilite prefab avant adoption Unity. | Checklist intake et fichiers asset. | Tout asset 3D a une source Blender, export controle, test d'echelle et plan prefab stable avant import gameplay. | Garder identite prefab gameplay, registration `NetworkObject`, composants, colliders et definition ids stables. |
| 0.7 Registre adoption | `Not Started` | Evaluer chaque candidat selon licence, cout, compatibilite Unity, maintenance, dependances, impact multiplayer, source/editabilite, droits redistribution, cout rollback et alignement architecture. | Entrees de registre completes. | Aucune decision `Adopt` sans champs obligatoires complets ni cout de rollback compris. | Un asset achete ou importe ne peut pas devenir une boite noire pour l'etat gameplay central. |
| 0.8 Decision gate | `Not Started` | Emettre la decision finale Epic 0 : `Pass`, `Blocked` ou accepte avec bloqueurs documentes. | Log de validation complet et liste de bloqueurs. | Local host/client, Steam distant, cap quatre joueurs avec rejet cinquieme, host quit, disconnect non-host et erreurs Lobby/UI sont passes ou bloques explicitement. | Epic 1 ne commence qu'apres satisfaction du gate. |

## Gate go/no-go Epic 1

Epic 1 est bloquee tant que l'Epic 0 n'est pas marquee `Pass` ou explicitement acceptee avec bloqueurs documentes. Si elle est acceptee avec bloqueurs, la liste doit nommer la validation non resolue, l'impact, le responsable, le contournement et la raison exacte pour laquelle le gameplay peut commencer sans masquer le risque.

Ne commence pas le gameplay Epic 1 tant que le setup Unity, les pins packages, Steamworks/Networking Sockets, la securite MCP, l'asset intake Blender, la revue d'adoption add-on ou les smoke tests requis sont encore non documentes.
