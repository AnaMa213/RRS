# Epic 1 Context: Playable Game Shell, Main Menu & Empty World Entry

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

L'Epic 1 produit la premiere version reellement lancable du jeu : le joueur ouvre l'application, arrive sur un menu principal simple, appuie sur Play, traverse une coquille de lobby locale avec des reglages de partie brouillon, cree ou choisit un personnage rudimentaire, puis entre dans une carte vide ou il peut se deplacer avec des assets placeholder. Tout reste local et hors ligne : aucun service Steamworks, aucune session reseau, aucun gameplay de conduite, de rage ou d'economie. L'enjeu est de fixer tot le squelette de flux (Bootstrap -> MainMenuLobby -> MVP_Run), les frontieres de modules et les emplacements UI que l'Epic 2 remplacera par du vrai multijoueur, sans avoir a reecrire la structure. L'Epic 0 est cloturee en `Accepted With Known Blockers` (2026-09-07) : l'Epic 1 demarre sous ce statut, avec obligation de ne pas masquer les bloqueurs reseau documentes.

## Stories

- Story 1.1: Bootstrap and Main Menu Launch
- Story 1.2: Local Lobby Shell and Match Settings Draft
- Story 1.3: Rough Character Creation and Player Profile Selection
- Story 1.4: First Rough Character, Car, and Building Asset Seeds
- Story 1.5: Empty Map Entry and Local On-Foot Movement
- Story 1.6: Epic 1 Playable Checkpoint

## Requirements & Constraints

Le flux de lancement doit demarrer depuis `Bootstrap` et router vers `MainMenuLobby`, avec une commande Play, une commande Quit pour les builds, et un emplacement visible reserve aux erreurs de service ou de reseau. Appuyer sur Play doit mener a l'ecran suivant sans exiger de services en ligne.

La coquille de lobby locale expose Create Lobby, un placeholder Join By Code, Start Game et Back. Elle porte des reglages de partie brouillon (difficulte plus parametres extensibles) stockes dans un objet de donnees local, concu pour etre synchronisable par l'epic reseau plus tard. Toute action en ligne indisponible doit afficher un retour visible explicite plutot que d'echouer en silence.

La creation/selection de personnage fournit un champ de nom joueur et au moins un modele placeholder. Le personnage retenu utilise un identifiant stable exploitable plus tard pour la synchronisation reseau ; les noms vides ou invalides recoivent un retour UI visible ; le profil selectionne est transmis au flux d'entree dans le monde.

Les assets de cette epic sont des placeholders greybox : un personnage, une voiture partagee, au moins un batiment ou bloc urbain simple. Chaque prefab documente sa source, un controle d'echelle, un plan de collider et une politique de remplacement. Les scripts gameplay ne dependent ni des mesh finaux ni des rigs d'animation finaux. Tout asset telecharge ou genere par IA passe par l'intake Blender et est enregistre dans le registre avant usage.

L'entree monde charge `MVP_Run` avec une carte vide (plan de sol ou rue greybox) et le personnage selectionne ; le joueur peut se deplacer, regarder, sprinter et s'arreter.

Contraintes transverses : faisabilite solo, une seule pile Unity/URP/input/network, pas de reinvention de fondations Unity deja resolues, et gate d'adoption obligatoire avant tout import tiers. L'epic se termine sur un checkpoint jouable : lancement -> menu -> setup -> monde vide sans erreur console bloquante, avec des placeholders d'etat (lobby, joueur, futures valeurs HUD) et des notes listant ce qui est jouable, ce qui est stub, et ce que l'Epic 2 remplacera.

## Technical Decisions

Le code se range en tranches de fonctionnalites : `RoadRage.App` compose scenes et services, `RoadRage.Shared` ne contient que des primitives, ids, wrappers reseau et interfaces etroites, et chaque feature vit sous `RoadRage.Features.<Feature>`. Pour cette epic les tranches concernees sont `Lobby`, `Run`, `Players`, `OnFoot` et `UI`. Aucune reference directe feature-a-feature : la coordination passe par l'orchestration Run, des interfaces/evenements dans Shared, ou l'etat reseau.

Bootstrap possede les services persistants et la duree de vie du `NetworkManager`, meme si l'Epic 1 ne l'utilise pas encore. On garde le seed a trois scenes (`Bootstrap`, `MainMenuLobby`, `MVP_Run`) ; les scenes `Dev_*` servent d'ateliers isoles. Le chargement additif et le streaming de monde sont differes.

Meme sans reseau actif, on n'introduit aucun raccourci contraire au modele hote-autoritaire : les scripts d'input et d'UI ne mutent jamais l'etat gameplay partage directement ; ils produisent des intentions. Les squelettes d'etat runtime (`NetworkedRunState`, `NetworkedPlayerState`, ...) existent deja et resteront la source de verite cote hote ; les reglages de partie et le profil joueur de l'Epic 1 doivent etre modelisables vers ces conteneurs sans duplication de verite.

Les donnees statiques auteur (personnages, definitions futures) prennent la forme de ScriptableObjects avec des ids globalement uniques en minuscules, sous `ScriptableObjects/<Feature>`, enregistres dans un catalogue partage au bootstrap. Les valeurs de session runtime ne vivent jamais dans des assets ScriptableObject.

Conventions : dossiers de features en PascalCase, composants d'etat reseau prefixes `Networked`, DTO d'intention suffixes `Intent`, definitions ScriptableObject suffixes `Def`, logs de diagnostic prefixes par feature (`[Lobby]`, `[Run]`). C# 9.0 supporte par Unity ; eviter les types record/init-only pour les donnees serialisees Unity.

Les fondations retenues au gate d'adoption de l'Epic 0 sont des packages Unity built-in deja pinnes : UGUI pour le menu/UI, Input System plus Cinemachine pour l'input et la camera, avec un controller on-foot ecrit en interne dans `RoadRage.Features.OnFoot`. Aucun starter controller ni package tiers n'est adopte ; en adopter un exige une nouvelle ligne de registre validee avant import. La camera est locale, jamais synchronisee.

Stabilite prefab greybox-vers-art : identite du prefab, enregistrement `NetworkObject`, composants gameplay, colliders et ids de definition restent stables quand l'art est remplace ; les colliders gameplay sont authored separement des mesh decoratifs, et l'art final remplace des enfants de rendu ou des variantes de prefab.

Cible de validation : builds de developpement Windows PC. Le cardinal du MVP est plafonne (une route, une voiture joueur, quatre joueurs max, trois vehicules IA, trois actions passager, un arret sandbox, une amelioration, un boss) : l'Epic 1 ne doit pas anticiper au-dela.

## UX & Interaction Patterns

Il n'existe pas de contrat UX dedie ; les exigences UX proviennent du SPEC et de l'architecture. L'UI de menu, lobby, creation de personnage et HUD peut s'appuyer sur les fondations UI Unity retenues. L'UI de lobby doit deja prevoir la place de la creation de room privee, de l'affichage d'un code de session et du join par code, meme si ces actions sont des placeholders en Epic 1. Une zone d'erreurs visibles doit exister des maintenant pour les echecs de join, de sockets reseau, de deconnexion, de service et de quit hote, afin que l'Epic 2 branche le comportement reel sans redessiner l'ecran. L'UI gameplay grandira ensuite vers l'etat de run, l'argent, la rage, les actions et les issues victoire/echec. Toute UI lit l'etat partage et emet des intentions ; elle n'ecrit jamais l'etat gameplay.

## Cross-Story Dependencies

L'Epic 1 est sequentielle : Story 1.1 pose la route Bootstrap -> MainMenuLobby dont depend Story 1.2 ; la coquille de lobby et son objet de reglages alimentent Story 1.3 ; le profil joueur produit par Story 1.3 est consomme par l'entree monde de Story 1.5. Story 1.4 depend du pipeline d'intake Blender et du registre d'adoption livres en Epic 0, et fournit les prefabs placeholder utilises par Story 1.5 et testes en Story 1.6. Story 1.6 depend de toutes les precedentes.

Dependances externes : le gate Epic 0 (`Accepted With Known Blockers`) autorise le demarrage, mais les validations reseau non resolues (lobby Steam runtime, smoke test distant deux joueurs, cap quatre joueurs, quit hote, erreurs Lobby/UI visibles) restent des bloqueurs ouverts reportes vers l'Epic 2 et ne doivent pas etre consideres comme couverts par l'Epic 1.

Vers l'aval : l'Epic 2 remplace la coquille de lobby locale par une room privee Steam reelle avec invite Steam natif et Lobby ID en repli, transforme les reglages locaux en etat synchronise, remplace l'entree monde locale par un spawn joueur reseau et transforme les placeholders d'etat en HUD. L'Epic 3 reutilise le prefab de voiture partagee cree ici, et l'Epic 6 reprend le mouvement on-foot pour la confrontation et les arrets sandbox : les frontieres de modules posees en Epic 1 doivent rester valides pour ces reprises.
