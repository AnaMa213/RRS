# Epic 0 Context: Gate de readiness technique, outils et assets/add-ons

Note BMAD : le prefixe `# Epic 0 Context:` est conserve volontairement pour que le cache de contexte Epic reste reconnaissable par le workflow.

<!-- Compile depuis les artefacts de planning. Modifiable librement. Regenerer avec compile-epic-context si les docs de planning changent. -->

## Objectif

L'Epic 0 etablit la baseline de production de RoadRage_Simulator avant tout travail gameplay. Elle bloque l'Epic 1 tant que Unity, Blender, Unity services, les packages requis, les outils IA/MCP, la structure projet, les regles d'adoption add-on/asset et les preuves de smoke tests ne sont pas documentes et valides. Les agents peuvent guider le setup manuel, creer les artefacts de suivi, inspecter les fichiers generes, valider les preuves et emettre le go/no-go Epic 1 ; ils ne doivent pas pretendre automatiser les installations GUI externes, la creation de comptes, les approbations de services ou les acces outils non verifies.

## Stories

- Story 0.1: Checklist de readiness setup et baseline workspace locale
- Story 0.2: Installation Unity Editor, creation du projet et verrouillage des packages
- Story 0.3: Readiness Steamworks, lobby et Networking Sockets
- Story 0.4: Structure projet, scenes, namespaces et squelette runtime state
- Story 0.5: Configuration Codex, Claude, Unity MCP et Blender MCP
- Story 0.6: Blender et pipeline d'intake des assets 3D
- Story 0.7: Registre d'adoption add-on Unity, librairie UI et assets
- Story 0.8: Smoke tests Epic 0 et gate go/no-go

## Exigences & Contraintes

L'Epic 0 est obligatoire et bloque l'Epic 1 jusqu'a ce que le gate final soit marque `Pass` ou `Accepted With Known Blockers`. La checklist de setup doit distinguer les actions manuelles utilisateur des validations agent, suivre les statuts `Not Started`, `In Progress`, `Pass`, `Blocked` et `Not Applicable`, et rendre les bloqueurs visibles avant le debut du gameplay.

La fondation MVP doit rester faisable pour un developpeur solo : prouver la boucle online greybox avec des primitives avant les assets 3D polis, eviter les stacks paralleles moteur/rendu/input/reseau, et reutiliser des packages Unity, starter assets, fondations UI, scaffolds controller ou add-ons compatibles seulement apres revue. Tout asset ou add-on tiers doit etre evalue avant import selon licence, cout, compatibilite Unity, maintenance, impact dependances, impact multiplayer, disponibilite/editabilite du source et alignement avec l'architecture spine.

Le projet Unity doit etre cree sous le nom `RRS` (RoadRageSimulator), utiliser le stack Universal 3D/URP approuve, et exposer `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/` et `Assets/` pour validation. Les ecarts de versions doivent etre consignes avant l'Epic 1. La cible initiale est le build de developpement Windows PC, branche sur une configuration Steamworks non-production (AppID de test `480`/Spacewar).

La readiness online doit valider des lobbies prives crees par l'host via `ISteamMatchmaking`, un flow invite Steam natif/Lobby ID, Networking Sockets (Steam Datagram Relay), `MaxPlayers = 4`, aucune ouverture de port routeur cote host, et des invites comme wrappers UI autour du Lobby ID sauf deep links natifs verifies plus tard. Les echecs de join, Networking Sockets, service, disconnect et host quit doivent avoir des exigences d'erreurs Lobby/UI visibles capturees pour implementation ulterieure. (Remplace l'approche initiale Unity Cloud/Unity Gaming Services/Relay -- voir `_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md`.)

Les secrets, API keys, tokens et credentials de service ne doivent jamais etre stockes dans les prompts, scripts, scenes, ScriptableObjects ou fichiers commites. Les identifiants Steam generes par le projet peuvent apparaitre dans les fichiers de configuration locaux, mais les captures, logs et prompts doivent caviarder tout identifiant compte/Lobby ID sensible sauf revue explicite. Les preuves finales de readiness doivent couvrir le smoke test local host/client Multiplayer Play Mode, le smoke test distant deux joueurs via Steamworks Networking Sockets, la validation du cap quatre joueurs, la gestion host quit, et les preuves ou notes d'implementation d'erreurs Lobby/UI visibles.

## Decisions Techniques

Le stack verrouille est Unity `6000.6.0f1` sur la release track Unity 6 Update, C# 9.0 tel que supporte par Unity, Universal 3D/URP, Netcode for GameObjects `2.13.2`, un transport Steamworks communautaire (`com.community.netcode.transport.facepunch` ou `.steamnetworkingsockets`, commit/tag epingle a l'installation), Unity Transport `6.6.0`, Universal Render Pipeline `17.6.0`, Multiplayer Play Mode `3.0.0`, Input System `1.20.0`, Cinemachine `6.6.0`, Blender `5.2 LTS`, et FBX ou GLB pour l'interchange 3D.

Le projet suit l'approche Feature-Sliced Host-Authoritative Unity. `Assets/RoadRage` est organise autour de `App`, `Shared`, `Features`, dossiers source/export art, materiaux, prefabs, ScriptableObjects et tests. Les namespaces et limites d'assemblies sont `RoadRage.App`, `RoadRage.Shared` et `RoadRage.Features.<Feature>`. Les scenes seed sont `Bootstrap`, `MainMenuLobby`, `MVP_Run`, plus les sandboxes de developpement vehicle, on-foot, rage et lobby smoke test.

La verite runtime appartient a l'host. Les clients soumettent des intentions joueur ; l'host valide et mute l'etat gameplay partage. Les `NetworkObject` gameplay-authoritative sont owned par l'host et les `NetworkVariable` gameplay sont server-write par defaut. Les types d'etat canonique a preserver sont `NetworkedRunState`, `NetworkedPlayerState`, `NetworkedAIVehicleState`, `NetworkedRageState`, `NetworkedCrewEconomyState` et `NetworkedBossState`.

Les donnees statiques authoring utilisent des ScriptableObjects avec ids globaux stables ; les valeurs de session runtime vivent dans des `NetworkBehaviour` et `NetworkVariable` host-owned. Le setup initial doit preserver les futurs modules Vehicle, OnFoot, PassengerActions, Rage, Economy, Lobby/Network, Run, Boss, SandboxStops et UI.

Tout asset 3D genere par IA ou telecharge doit passer par un nettoyage Blender avant utilisation prefab Unity. Le chemin d'intake doit sauvegarder la source Blender, exporter en FBX ou GLB, importer dans Unity, creer le prefab apres test d'echelle, et garder l'identite prefab gameplay, l'enregistrement `NetworkObject`, les composants gameplay, les colliders et les definition ids stables quand l'art est remplace.

Unity MCP doit privilegier Unity Official MCP si disponible ; sinon utiliser CoplayDev Unity MCP epingle a un tag de release. Blender MCP doit privilegier Blender Lab MCP si stable ; sinon utiliser le fallback ahujasid Blender MCP. Les MCP sont des assistants controles : toute modification de scenes, prefabs, scripts, packages ou assets doit etre revue dans Unity/Blender et commitee par petites etapes. Les MCP ne doivent pas ajouter silencieusement des services payants, changer les versions de packages, convertir vers dedicated servers, stocker des secrets ou contourner l'asset intake.

## UX & Patterns D'interaction

Le travail UX de l'Epic 0 concerne surtout le setup : checklists et logs de validation doivent rendre faciles a scanner les statuts de readiness, actions manuelles, validations agent, bloqueurs et preuves. Le registre d'adoption add-on doit enregistrer les premieres decisions pour les fondations menu/UI et mouvement/controller avant l'Epic 1, meme si la decision est d'utiliser les packages Unity built-in.

Les exigences lobby a capturer pour la suite : creer une room privee, afficher une invite Steam/Lobby ID, rejoindre via l'overlay Steam ou par Lobby ID, traiter le Lobby ID comme wrapper de secours, et afficher des erreurs visibles pour join, Networking Sockets, disconnect, service et host quit. Les scripts UI/input restent des couches presentation/intention et ne mutent pas directement l'etat gameplay partage.

## Dependances Cross-Story

Story 0.1 cree les documents de setup que toutes les stories Epic 0 suivantes mettront a jour. Story 0.2 depend de la checklist et de la creation manuelle du projet Unity avant validation des packages. Story 0.3 depend du projet Unity et du transport Steamworks avant validation de l'AppID de test, Networking Sockets, Lobby ID et exigences d'erreurs service.

Story 0.4 depend de la baseline projet/packages, puis etablit scenes, dossiers, namespaces, boundaries d'assemblies et suivi du squelette runtime state pour tous les epics gameplay. Story 0.5 depend des decisions tooling et consigne l'usage sur Codex/Claude/MCP plus les smoke tests Unity et Blender MCP sans danger. Story 0.6 depend de l'installation Blender et definit le pipeline asset intake que le futur travail art doit suivre.

Story 0.7 doit enregistrer les decisions initiales de fondation UI/menu et movement/controller avant le debut Epic 1. Story 0.8 depend de toutes les stories setup precedentes terminees ou explicitement bloquees, puis consigne la decision finale `Pass`, `Blocked` ou `Accepted With Known Blockers` qui controle le demarrage de l'Epic 1.
