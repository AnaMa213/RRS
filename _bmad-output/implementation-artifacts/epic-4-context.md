# Epic 4 Context: Passenger Chaos Actions & Rage Module

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

L'Epic 4 donne un vrai role aux passagers pendant la conduite: ils disposent de trois actions de chaos declenchees depuis l'UI, transmises comme intentions client, validees cote host, puis appliquees sous forme d'effet visible sur la rage d'une cible, un incident, une opportunite de faible valeur ou une aide d'equipage. Cet epic etablit aussi le contrat de rage independante qui sera consomme plus tard par le trafic IA, les evenements Rage Road et la boucle complete du MVP.

## Stories

- Story 4.1: Rage State Module and Definitions
- Story 4.2: Passenger Action Framework and Host-Validated Intent
- Story 4.3: Passenger Action One Changes Rage
- Story 4.4: Passenger Action Two Creates an Incident or Resource Opportunity
- Story 4.5: Passenger Action Three Provides Crew Help
- Story 4.6: Epic 4 Passenger Chaos Playable Checkpoint

## Requirements & Constraints

- La rage doit exister comme etat gameplay visible, suivi par cible et non comme jauge globale; au minimum une cible doit pouvoir afficher une valeur et un libelle d'etat de rage pendant les tests.
- Les passagers ont trois emplacements d'actions MVP accessibles via l'UI en jeu.
- Chaque action passager doit envoyer une intention au host avant toute mutation d'etat partage; le host valide l'acteur, le siege/mode joueur, la cible, la disponibilite, le cooldown, la portee utile et la version du payload avant d'appliquer l'effet.
- Les cas de cooldown, cible invalide, siege indisponible et joueur deconnecte doivent produire un retour visible.
- Les trois actions couvrent trois usages MVP: augmenter/changer la rage, creer un incident ou une opportunite de faible valeur, et fournir une aide d'equipage.
- Chaque action doit produire un resultat visible sur au moins un axe: rage, incident, ressource/opportunite mineure ou effet d'aide.
- `MVP_Run` doit permettre de tester la rage contre plusieurs cibles: au moins deux vehicules drivable supplementaires configures comme rage targets, plus un controle dev pour faire apparaitre un vehicule normal ou rage-target avec verification de clearance.
- Les actions absurdes restent des jouets, declencheurs ou gains faibles; les recompenses significatives restent reservees aux confrontations Rage Road des epics ulterieurs.
- Les noms finaux, le ton, les animations, les sons, les visuels et les limites de classification/contenu restent remplacables tant que les limites de ton ne sont pas tranchees.
- L'epic doit rester testable sans trafic IA reel d'Epic 5 ni resolution de confrontation d'Epic 6.
- Le checkpoint final doit laisser le jeu lancable et verifiable en local Multiplayer Play Mode avec au moins deux joueurs, dont un conducteur et un passager.

## Technical Decisions

- Le projet reste feature-sliced: le code de rage vit dans `Features/Rage`, les actions passager dans `Features/PassengerActions`, et la composition passe par Run, Shared ou des interfaces/evenements etroits plutot que par mutation directe entre features.
- La rage d'un vehicule ennemi appartient a un composant reseau host-owned de type `NetworkedRageState`; les valeurs de session vivent dans des `NetworkBehaviour`/`NetworkVariable`, jamais dans les ScriptableObjects.
- Les definitions statiques de rage, seuils et actions passager sont des ScriptableObjects avec ids stables, globaux et en minuscules, enregistres dans le catalogue de definitions partage au bootstrap.
- Les payloads reseau envoient des ids de definition et des references Netcode-sures vers les cibles, pas des copies de donnees, des index de voie ou des ids authored utilises comme identite runtime.
- Les objets gameplay faisant autorite sont host-owned et les `NetworkVariable` gameplay sont server-write par defaut.
- Les input handlers et scripts UI ne mutent jamais l'etat partage directement; ils collectent l'entree locale, construisent une intention typee et la soumettent au host.
- Camera et input restent de la presentation locale et ne sont pas synchronises comme etat gameplay; le focus/cycle de camera entre rage targets sert a observer quelle cible change, pas a porter une autorite gameplay.
- Les conventions de nommage a conserver: composants d'etat reseau prefixes `Networked`, definitions suffixees `Def`, intentions client suffixees `Intent`, logs diagnostics prefixes par la feature proprietaire comme `[Rage]` ou `[PassengerActions]`.

## UX & Interaction Patterns

- Le HUD ou la dev UI doit rendre lisibles l'etat de rage courant et les trois actions passager disponibles.
- Dans `MVP_Run`, le HUD de rage est ancre en haut a droite et montre l'etat de la rage target actuellement focalisee.
- Le passager peut focaliser une rage target et cycler entre les rage targets disponibles pour voir quelle cible est affectee.
- Chaque action doit donner un retour visible apres usage, meme en greybox: changement de rage, marqueur d'incident, opportunite mineure ou feedback d'aide.
- Les etats indisponible, cooldown et cible invalide doivent etre visibles pour le passager.
- L'UI gameplay lit l'etat partage et envoie seulement des intentions; elle ne devient pas proprietaire de la rage, des cooldowns ou des resultats.

## Cross-Story Dependencies

- Story 4.1 pose la rage et ses definitions; Story 4.2 depend de cette base pour fournir le framework d'actions et le pipeline d'intention host-validee.
- Stories 4.3, 4.4 et 4.5 dependent du framework de Story 4.2 et peuvent ensuite avancer comme trois actions MVP separees.
- Story 4.6 depend de l'ensemble des stories precedentes et verifie l'experience passager/conducteur en contexte jouable.
- L'epic depend des sieges et de la conduite partages d'Epic 3, mais ne doit pas attendre le trafic IA d'Epic 5 ni la confrontation/economie d'Epic 6.
- Les epics 5 et 6 consommeront les contrats produits ici: rage par cible, intentions passager validees par le host, effets visibles et donnees statiques stables.
