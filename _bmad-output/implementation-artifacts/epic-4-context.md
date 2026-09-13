# Epic 4 Context: Passenger Chaos, Rage Sandbox & Profile Correction

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Epic 4 doit d'abord cloturer la revue des Stories 4.1 a 4.4 (rage par cible, pipeline d'actions passager validees cote host, action qui change la rage, marqueur d'incident ou d'opportunite), puis corriger le flux profil/menu/session avant d'ajouter d'autres systemes de gameplay. La selection cosmetique Rookie/Veteran faite dans le menu principal devient persistante, et le personnage choisi avant l'entree dans le lobby est celui qui apparait en session, en solo comme en multijoueur. L'epic reste un bac a sable de fondations validables isolement: il ne doit exiger ni ville, ni autoroute, ni boss, ni progression complete, ni recompense economique.

## Stories

- Story 4.1: Rage State Module and Definitions
- Story 4.2: Passenger Action Framework and Host-Validated Intent
- Story 4.3: Passenger Action One Changes Rage
- Story 4.4: Passenger Action Two Creates an Incident or Resource Opportunity
- Story 4.5: Persistent Steam Profile and Main Menu Character Selection
- Story 4.6: Profile Freeze, Session Payload, and Selected-Character Spawn

## Requirements & Constraints

- Avant toute nouvelle story, les revues finales de 4.1 a 4.4 sont un gate obligatoire, et la divergence de statut de 4.4 (review dans le suivi, done dans sa specification) doit etre arbitree. 4.1 a 4.4 restent des livraisons historiques: ne pas les reecrire pour faire comme si elles avaient toujours satisfait la nouvelle direction.
- La rage est un etat gameplay visible, suivi par cible et non par une jauge globale; au minimum une cible expose une valeur et un libelle d'etat pendant les tests.
- Les passagers disposent de trois emplacements d'actions MVP dans l'UI. Chaque action envoie une intention au host, qui valide l'acteur, le mode/siege, la cible, la disponibilite, le cooldown, la portee utile et la version du payload avant toute mutation d'etat partage.
- Chaque action produit un resultat visible sur au moins un axe: rage, incident, ressource/opportunite de faible valeur ou effet d'aide. Cooldown, cible invalide, siege indisponible et joueur deconnecte donnent un retour visible.
- Le test de la rage doit etre repetable sur plusieurs cibles: HUD de rage ancre en haut a droite dans `MVP_Run` pour la cible focalisee, au moins deux vehicules drivable supplementaires configures comme rage targets, et un controle dev qui fait apparaitre un vehicule normal ou rage-target a tout moment, avec verification de clearance pour qu'il soit immediatement drivable et jamais incruste dans le decor.
- Les actions absurdes restent des jouets, des declencheurs ou des gains faibles; les recompenses significatives et l'economie n'appartiennent pas a cet epic.
- Profil: au premier lancement avec Steam disponible, un profil local persistant est cree automatiquement depuis l'identite Steam avec Rookie par defaut. Aucun champ de nom, aucune confirmation, aucun ecran de gestion de profil. Si Steam est indisponible (override humain du 2026-09-13, prioritaire sur la direction initiale qui excluait tout repli), le menu reste utilisable avec une identite en memoire non persistee (Rookie par defaut), l'erreur Steam reste visible et aucun fichier profil n'est ecrit. Rookie/Veteran sont purement cosmetiques, la selection survit au redemarrage, le menu principal est la seule surface de selection, et le choix est gele des l'entree dans le lobby.
- Le deroulement de session reste lancable et verifiable apres l'epic (FR27), mais le checkpoint de chaos passager de l'ancienne roadmap est deprecie au profit de la correction profil/session.
- Les noms finaux, le ton, les animations, les sons, les visuels et les limites de classification/contenu restent remplacables tant que les limites de ton ne sont pas tranchees.
- L'epic reste testable sans trafic IA d'Epic 5 ni confrontation/economie d'Epic 6. Tests attendus: solo, host/client, persistance apres relance et immuabilite du choix dans le lobby.

## Technical Decisions

- Le projet reste feature-sliced: la rage vit dans `Features/Rage`, les actions passager dans `Features/PassengerActions`, le profil/menu cote App; la composition passe par Run, Shared ou des interfaces/evenements etroits, jamais par une mutation directe entre features.
- La rage d'une cible appartient a un composant reseau host-owned de type `NetworkedRageState`. Les valeurs de session vivent dans des `NetworkBehaviour`/`NetworkVariable`, jamais dans les ScriptableObjects.
- Les seuils de rage, les definitions d'actions et les autres donnees authored sont des ScriptableObjects a ids stables, globaux et en minuscules, enregistres dans le catalogue de definitions partage au bootstrap.
- Les objets gameplay faisant autorite sont host-owned et les `NetworkVariable` gameplay sont server-write par defaut; le host arbitre rage, sante, sieges et resultats.
- Les actions client sont des intentions typees (ServerRPC) validees par le host. Les references de cible utilisent `NetworkObjectReference` ou l'equivalent Netcode-sur; jamais des ids authored ni des index de voie comme identite runtime.
- Input handlers et scripts UI ne mutent jamais l'etat partage directement; ils collectent l'entree locale et soumettent une intention. Camera et input restent de la presentation locale et ne sont pas synchronises.
- Continuite a preserver: le payload de connexion, la resolution host, `CharacterId`, le spawn et la presentation existants. Seule la source du profil change (menu principal persistant au lieu de l'ecran de creation manuel); ne pas reecrire la chaine reseau.
- Le profil persistant ne contient que l'identite Steam et un choix cosmetique. Ni monnaie, ni objet, ni vie, ni siege, ni etat de lobby ou de session. Le runtime reste dans `NetworkedPlayerState` et les objets spawnes; l'etat de session ne s'ecrit jamais dans le stockage persistant.
- Le lobby peut afficher le personnage choisi mais ne modifie ni ne republie la selection: elle est gelee avant l'entree.
- Economie: `NetworkedCrewEconomyState` est supplante avant tout travail economique reel, et un portefeuille individuel par joueur (host-authoritative) est la direction retenue. `NetworkedPlayerState.Money` reste un placeholder a retirer plus tard; aucune recompense, achat ou inventaire n'est construit dans cet epic.
- Ne pas transformer les dommages vehicule existants (Wheel/Engine/Brake) en systeme de pieces ou de reparation dans cet epic.
- Garder le tuning et la reaction de rage separes du controleur de vehicule, de facon a permettre plus tard une reponse Rage/Fear data-driven sans casser les appels actuels. Aucune hierarchie d'effets generique n'est justifiee maintenant.
- Conventions a conserver: composants d'etat prefixes `Networked`, definitions suffixees `Def`, intentions suffixees `Intent`, logs diagnostiques prefixes par la feature proprietaire (`[Rage]`, `[PassengerActions]`).

## UX & Interaction Patterns

- Le HUD ou la dev UI rend lisibles l'etat de rage courant et les actions passager disponibles; le HUD de rage de `MVP_Run` est ancre en haut a droite et suit la cible focalisee.
- La camera passager peut focaliser une rage target et cycler entre les cibles disponibles pour observer laquelle change; ce focus reste de la presentation locale, sans autorite gameplay.
- Chaque action donne un retour visible apres usage, meme en greybox; les etats indisponible, cooldown et cible invalide sont visibles pour le passager.
- Menu principal: un apercu 3D inspectable (rotation libre, presentation locale), deux controles directs et libelles Rookie/Veteran avec le choix courant visible, et les actions Start Game / Create Lobby / Join Lobby. Pas de champ de nom, pas de page profil, pas d'affichage de statistiques, de progression ni de personnage verrouille.
- Lobby: identite du personnage selectionne affichee en lecture seule, sans controle de changement.
- L'identite du personnage est toujours presentee avec du texte en plus du modele, et les etats/erreurs ne reposent pas seulement sur la couleur.
- Les erreurs Steam/session restent explicites et visibles; aucun flux d'identite de repli n'est propose.
- L'UI gameplay lit l'etat partage et n'emet que des intentions; elle ne devient pas proprietaire de la rage, des cooldowns ou des resultats.

## Cross-Story Dependencies

- Story 4.1 pose la rage et ses definitions; Story 4.2 en depend pour fournir le framework d'actions et le pipeline d'intention host-validee. Stories 4.3 et 4.4 dependent du framework 4.2 et avancent comme actions separees.
- Stories 4.1 a 4.4 dependent des sieges et de la conduite partages d'Epic 3, sans attendre le trafic IA d'Epic 5 ni l'economie d'Epic 6.
- Story 4.5 depend du flux Steam et de la chaine menu de Epic 1/2; Story 4.6 depend de 4.5 et reutilise le payload, la resolution host et le spawn existants (Story 2.5). Le gel de selection complete la frontiere lobby/session/runtime.
- L'ordre de travail de la roadmap corrective est: gate de revue 4.1-4.4, puis 4.5, puis 4.6, avant toute autre suite de gameplay.
- Les epics ulterieurs consomment les contrats produits ici: rage configurable par cible et reactions data-driven (Story 5.1), et portefeuille individuel pour l'economie. L'epic prepare donc des fondations sans verrouiller l'assemblage MVP 2 (niveaux, boss, checkpoints).
