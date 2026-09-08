# Epic 2 Context: Private Online Lobby, Player Spawn & In-Game UI Foundation

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

L'Epic 2 remplace la coquille de lobby locale de l'Epic 1 par la premiere boucle reseau reelle : initialiser Steamworks avec un retour visible, permettre a un hote de creer une room privee Steam (invite native et/ou Lobby ID en repli), permettre a un joueur de rejoindre par code, faire respecter le plafond de quatre joueurs, faire apparaitre chaque joueur connecte dans le monde vide avec un `NetworkedPlayerState` autoritaire cote hote, et afficher le premier HUD utilisable (vie, sprint, statut reseau, retour joueur). C'est la fondation reseau la plus a risque du projet : l'epic se termine sur un checkpoint valide en Multiplayer Play Mode et avec un vrai test distant a deux joueurs, avant que l'Epic 3 n'ajoute la conduite.

## Stories

- Story 2.1: Online Services Bootstrap and Status Feedback
- Story 2.2: Host-Created Private Room with Join Code
- Story 2.3: Join By Code and Invite-Link Wrapper
- Story 2.4: Lobby Roster, Ready State, and Settings Sync
- Story 2.5: Networked Player Spawn in Empty World
- Story 2.6: In-Game HUD Foundation
- Story 2.7: Player Lifecycle, Disconnect, and Host-Quit Handling
- Story 2.8: Epic 2 Online Playable Checkpoint

## Requirements & Constraints

L'initialisation Steamworks (`SteamClient.Init`, etat de connexion Steam) passe par un service de bootstrap unique ; succes, echec d'initialisation, echec de connexion et etat hors-ligne/indisponible doivent tous produire un retour visible dans l'UI de lobby, jamais un echec silencieux. Aucun secret, cle ou identifiant de service n'est stocke dans le code, les scenes, les ScriptableObjects ou les fichiers commit.

La creation de room passe par un lobby Steam prive (`ISteamMatchmaking`) avec transport Networking Sockets et `MaxPlayers = 4` ; aucune ouverture de port ou connexion directe n'est requise cote hote. Le cycle de vie de la room (fermeture hote, depart joueur, expiration, nettoyage des rooms abandonnees, retour menu) doit rester visible et geree avec retour UI. Le join par code accepte un Lobby ID ou une invitation Steam ; l'entree est nettoyee/normalisee, les valeurs vides ou invalides sont rejetees avec retour visible, et la reservation de slot est validee avant la connexion pour empecher tout depassement du plafond de quatre joueurs en cas de joins concurrents. Code invalide, room pleine, session expiree et echec de service ou de Networking Sockets produisent chacun un retour UI visible distinct.

Le roster de lobby affiche les joueurs connectes (jusqu'a quatre), chaque joueur peut se marquer pret, l'hote edite le reglage de difficulte MVP, et le lancement de la partie n'est autorise que si les services sont initialises, le roster synchronise, les reglages valides, et que tous les joueurs sont prets (ou qu'une exception explicite de test solo est activee). Les clients recoivent les reglages selectionnes avant le chargement du monde.

Au chargement de `MVP_Run`, chaque joueur connecte recoit un seul objet joueur reseau avec un `NetworkedPlayerState` autoritaire cote hote ; l'input et la camera locaux n'affectent que la presentation et l'intention soumise du joueur local. Les spawns tardifs, echoues ou dupliques doivent etre geres avec un retour logge et visible.

Le HUD affiche la vie sous forme de coeurs, un statut de sprint/stamina, le nombre de joueurs, le statut reseau et une valeur d'argent placeholder ; il lit l'etat partage et emet des intentions via des interfaces approuvees, sans jamais muter directement les NetworkVariables partagees ; il doit rester lisible en solo comme a quatre joueurs.

Le cycle de vie joueur (vivant, a terre, mort, deconnecte, reconnecte) reflete `NetworkedPlayerState` cote hote ; l'etat "tous morts" est detecte et logge pour une future integration du restart complet ; en cas de quit hote ou de perte de session, les clients retournent a `MainMenuLobby` avec une erreur visible ; la migration d'hote est explicitement differee.

Le checkpoint final de l'epic doit etre verifie en Multiplayer Play Mode local et par un test distant a deux joueurs via Steamworks Networking Sockets, valider le plafond de quatre joueurs, et documenter limitations connues, etapes de test manuel et bloqueurs avant l'Epic 3.

## Technical Decisions

Les sessions en ligne sont des lobbies Steam prives crees par l'hote (`ISteamMatchmaking`, `MaxPlayers = 4`), avec Steamworks Networking Sockets (Steam Datagram Relay) comme chemin reseau unique, gratuit quel que soit le nombre de joueurs simultanes. Le join se fait via invitation Steam native et/ou Lobby ID partage comme code, tous deux de simples wrappers UI autour du meme join de lobby Steam, jamais un deep link OS natif. Le matchmaking public, le lobby browser, les serveurs dedies et la migration d'hote restent hors MVP.

Le hote reste autoritaire pour l'etat de jeu partage : les clients envoient de l'intention, le hote valide et mute l'etat. Les NetworkObjects a autorite gameplay sont possedes par l'hote et les NetworkVariables gameplay sont server-write par defaut ; les actions joueur deviennent des intents `ServerRPC` types, valides par le hote (acteur, phase de run, mode/siege joueur, cooldown, reference de cible, portee, version de payload) avant toute mutation.

`NetworkedPlayerState` porte `PlayerMode` (Driver/Passenger/OnFootStop/OnFootRageRoad/Spectating) et `PlayerLifecycle` (Alive/Downed/Dead) ; en Epic 2 seuls Spectating/OnFoot generique et le cycle Alive/Downed/Dead sont pertinents, les autres modes restent reserves aux epics ulterieures. Bootstrap possede les services persistants, `SteamClient.Init`, l'etat de connexion Steam et la duree de vie du `NetworkManager` ; `MainMenuLobby` cree ou rejoint le lobby Steam prive puis l'hote demarre le reseau avant un chargement synchronise vers `MVP_Run` ; `RunCompositionRoot` resout les racines de scene serialisees et spawn/enregistre les NetworkObjects gameplay possedes par l'hote.

Camera et input restent des preoccupations de presentation strictement locales (rig Cinemachine local par joueur, Input System vers une couche PlayerIntent), jamais synchronisees ni autorisees a muter l'etat partage directement.

Cible de validation : builds de developpement Windows PC avec l'AppID de test Steamworks (480/Spacewar), un smoke test local Multiplayer Play Mode hote/client, un smoke test distant a deux joueurs via Networking Sockets, une validation du plafond a quatre joueurs, la gestion du quit hote, et des erreurs Lobby/UI visibles pour join, Networking Sockets, deconnexion et service.

Pile verrouillee pertinente : Netcode for GameObjects 2.13.2, transport Steamworks communautaire (Facepunch ou SteamNetworkingSockets, licence/commit a confirmer a l'installation), Unity Transport 6.6.0, Multiplayer Play Mode 3.0.0.

## UX & Interaction Patterns

L'UI de lobby doit permettre de creer une room privee, voir/partager un code de join, et rejoindre par code, avec une invitation traitee comme un wrapper autour de ce meme code tant qu'aucun deep link natif n'est verifie. Une zone d'erreurs visibles couvre systematiquement echec de join, echec Networking Sockets, deconnexion, echec de service et quit hote — c'est l'emplacement deja reserve en Epic 1 qui se branche ici pour de vrai. Le HUD gameplay de cette epic se limite a vie/sprint/nombre de joueurs/statut reseau/argent placeholder ; l'affichage complet de l'etat de run, de l'argent, de la rage et des actions passager reste pour les epics suivantes. Toute UI lit l'etat partage et emet des intentions, sans jamais muter l'etat gameplay directement.

## Cross-Story Dependencies

Sequence interne : Story 2.1 (bootstrap services) conditionne toutes les autres. Story 2.2 (creation de room) depend de 2.1. Story 2.3 (join par code) depend d'une room existante creee en 2.2. Story 2.4 (roster/ready/settings) depend de 2.2 et 2.3 pour peupler le lobby. Story 2.5 (spawn reseau) depend d'une session de lobby valide demarree via 2.4. Story 2.6 (HUD) et Story 2.7 (cycle de vie/deconnexion/quit hote) dependent toutes deux des joueurs spawnes en 2.5. Story 2.8 (checkpoint) depend de l'ensemble des stories precedentes.

Dependances externes : l'Epic 2 remplace la coquille de lobby locale et l'entree monde locale de l'Epic 1 par leurs equivalents reseau, sans redessiner les emplacements UI deja prevus. Les validations reseau non resolues au gate de l'Epic 0 (smoke test distant, plafond quatre joueurs, quit hote, erreurs Lobby/UI) sont couvertes ici. En aval, l'Epic 3 reutilise directement le spawn joueur reseau et la fondation HUD pour y brancher la conduite du vehicule partage.
