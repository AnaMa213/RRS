# Epic 5 Context: NPC Response Foundation and Future Traffic

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Epic 5 doit etablir la reponse NPC configurable (Rage et Fear par cible, pilotee par des donnees) puis rendre testable la premiere brique de circulation IA: trois vehicules qui suivent une route simple, changent d'etat de comportement quand leur rage evolue, et peuvent declencher un evenement Rage Road. L'epic reste un bac a sable de fondations validables isolement: il ne doit introduire ni controleur de trafic complet, ni boss, ni liste finale d'archetypes, ni comportement propre a un niveau. Il prepare l'assemblage MVP 2 (ville/autoroute, checkpoints, confrontation) sans le verrouiller et sans dependre de l'economie d'Epic 6.

## Stories

- Story 5.1: Configurable NPC Rage/Fear Foundation
- Story 5.2: Basic AI Route Following and Recovery
- Story 5.3: Rage-Driven AI Behavior States
- Story 5.4: Rage Road Event Trigger
- Story 5.5: AI Traffic Networking and Client Presentation
- Story 5.6: Epic 5 AI Traffic Playable Checkpoint

## Requirements & Constraints

- 5.1 etend uniquement la fondation Rage existante: chaque cible peut porter un etat Rage et Fear host-authoritative, avec des tendances statiques data-driven, et un effet peut influencer la rage, la peur ou les deux. Interdits explicites: controleur de trafic complet, boss, liste finale d'archetypes, comportement specifique a un niveau. La fondation doit rester testable seule dans une sandbox.
- Un vehicule IA qui change d'etat ne doit pas forcer les autres: chaque cible reste independante (pas de jauge globale, pas d'etat partage cache).
- Trafic IA minimal: trois vehicules suivant waypoints ou marqueurs de voie a des vitesses testables, capables de se recuperer ou de se reinitialiser s'ils sont bloques, retournes ou hors de la zone jouable. Le mouvement doit etre suffisamment deterministe pour des tests reseau host-authoritative, et le trafic doit pouvoir etre desactive ou isole dans une sandbox de developpement.
- Changements d'etat visibles: mouvement, labels UI/debug ou marqueurs de feedback; les etats et erreurs ne doivent pas reposer uniquement sur la couleur.
- Declenchement Rage Road: un seul evenement a la fois, avec cible identifiee et retour joueur visible; les tentatives simultanees sont arbitrees cote host par une regle documentee (premier declenchement, priorite configuree ou file d'evenements); pas de doublon pour un evenement actif. La suite (resolution, recompense) appartient a Epic 6.
- Reseau: positions, etats de comportement, labels de rage et etat de l'evenement sont synchronises depuis l'etat host-owned; un client ne peut pas forcer un changement de comportement; les clients qui rejoignent tardivement recoivent l'etat courant. La charge reseau doit rester tenable pour quatre joueurs et trois vehicules IA.
- Checkpoint final: testable dans `MVP_Run` ou `Dev_RageSandbox`, avec smoke test local et en ligne; l'evenement reste visible et stable jusqu'a resolution ou reset, et les hypotheses de tuning sont consignees pour la conception de la confrontation d'Epic 6.
- Pas de duplication de verite runtime (rage/peur, mouvement, evenement) et pas de couplage a un niveau ou a un boss dans cet epic.

## Technical Decisions

- La rage et la peur d'une cible appartiennent a un composant reseau host-owned: `NetworkedRageState` dans `RoadRage.Features.Rage` porte deja la machine d'etat de rage et le tuning ScriptableObject issus d'Epic 4. Fear et les tendances d'archetype etendent ce composant; on n'introduit pas de seconde source de verite.
- Le mouvement et l'identite de route vivent dans `NetworkedAIVehicleState` (`RoadRage.Features.Vehicles`), qui ne porte aujourd'hui qu'un index de route. Route, comportement et recuperation s'y ajoutent; le tuning et la reaction rage/peur n'y sont pas codes.
- Composition feature-sliced: `Features/Rage` reste proprietaire de la rage/peur; `Features/Vehicles` consomme cet etat via une interface ou un evenement etroit et ne le mute jamais; les declenchements Rage Road passent par Run. Aucune mutation directe entre features.
- Les donnees authored (seuils, tendances d'archetype, effets) sont des ScriptableObjects a ids stables en minuscules, enregistres au bootstrap dans le catalogue de definitions partage. Les valeurs de session vivent dans des `NetworkBehaviour`/`NetworkVariable`, jamais dans les ScriptableObjects.
- Autorite: les objets gameplay faisant autorite sont host-owned et les `NetworkVariable` gameplay sont server-write. Le host arbitre comportements IA, transitions et declenchement d'evenement. Les clients envoient des intentions typees (`Intent`) validees par le host (acteur, mode/siege, cible, disponibilite, cooldown, portee, version du payload). Les references de cible utilisent `NetworkObjectReference`, jamais des ids authored ni des index de voie comme identite runtime.
- Input handlers et UI ne mutent jamais l'etat partage; camera et input restent de la presentation locale.
- Etats: conserver la semantique de comportement deja nommee (calme, irrite, fuite, blocage, ram, declenchement de confrontation) et garder tuning, reaction et mouvement separes (AD-31), pour permettre une reponse data-driven sans casser les appels existants. Aucune hierarchie d'effets generique n'est justifiee maintenant.
- Vocabulaire a conserver: `Rage Road event` pour le cycle de crise; ne pas reintroduire de route unique, de cardinalite boss/Rage Road fixe ni de portefeuille d'equipage (superseded par les decisions courantes du spine).
- Conventions: composants d'etat prefixes `Networked`, definitions suffixees `Def`, intentions suffixees `Intent`, logs prefixes par la feature proprietaire (`[Rage]`, `[Vehicles]`). Pile figee: Unity 6 / C# 9.0, URP, Netcode for GameObjects avec transport Steam, donnees authored en ScriptableObjects.
- Scenes: `Dev_RageSandbox` existe et reste le lieu d'isolation du trafic et des etats de rage; `MVP_Run` reste la scene de validation integree greybox.

## UX & Interaction Patterns

- Le joueur doit percevoir pourquoi un vehicule IA change de comportement: label d'etat lisible (le HUD de rage ancre en haut a droite de `MVP_Run` et les vues de debug existantes servent de support) en plus du mouvement.
- Le declenchement d'un evenement Rage Road doit etre signale par un feedback visible et textuel, sans dependre de la couleur seule.
- Cet epic n'introduit aucun nouvel ecran de menu; la presentation reste en greybox et remplacable.

## Cross-Story Dependencies

- 5.1 etend la fondation Rage d'Epic 4 (Stories 4.1 a 4.4, revue passee; Epic 4 est marque termine dans le suivi) et doit rester compatible avec les appels existants, sans les reecrire.
- Ordre interne: 5.2 et 5.3 dependent de 5.1; 5.4 depend de 5.3 (au moins un vehicule capable d'atteindre un etat declencheur de confrontation); 5.5 depend de 5.2 a 5.4 et de la chaine reseau existante; 5.6 valide l'ensemble en local et en ligne.
- Amont: conduite, sieges et degats d'Epic 3, actions passager host-validees d'Epic 4.
- Aval: Epic 6 consomme l'evenement Rage Road (resolution, confrontation, recompense economique) et le tuning valide ici. Epic 5 doit livrer l'etat d'evenement et l'arbitrage de declenchement sans implementer la resolution, et sans exiger ville, autoroute, boss ni checkpoint.
