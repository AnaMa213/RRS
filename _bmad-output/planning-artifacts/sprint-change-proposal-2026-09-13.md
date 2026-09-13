# Proposition de correction de trajectoire — 2026-09-13

## 1. Résumé du problème

La Story 4.4 a révélé que le MVP actuel est orienté vers une boucle intégrée trop tôt (route unique, portefeuille d'équipe, Rage Road et boss), alors que la phase en cours doit d'abord être un bac à sable de fondations validables isolément. Le flux profil actuel exige une création manuelle et ne survit pas au redémarrage, ce qui contredit la nouvelle exigence : identité Steam, profil automatique, Rookie par défaut et sélection Rookie/Veteran directement dans le menu principal.

La demande est un pivot de produit et de roadmap, pas une demande de code. Les Epics 1–3 et les Stories 4.1–4.4 restent des livraisons historiques ; elles ne doivent pas être réécrites pour faire semblant d'avoir toujours satisfait les nouvelles exigences.

**Classification : majeure.** Replanification produit/architecture nécessaire avant toute nouvelle story ; aucun rollback de code recommandé.

## 2. État actuel constaté

### Artefacts et statut

- Les documents canonique (`SPEC`, `mvp-scope`, `gameplay-model`, `module-composition`) définissent encore une boucle MVP intégrée : voiture partagée, trois véhicules IA, Rage Road, portefeuille commun, amélioration, boss et reprise depuis le début.
- `epics.md` aligne les Epics 1–7 sur cette boucle. Epic 0–3 sont marqués terminés ; Epic 4 est en cours.
- `sprint-status.yaml` marque 4.1, 4.2, 4.3 **et 4.4** en `review`. Les fichiers de spécification, le commit `7991d57` et les tests 4.4 montrent néanmoins que 4.4 a été implémentée. Cela contredit l'énoncé initial selon lequel seules 4.1–4.3 sont sans revue finale : le statut réel doit être arbitré, sans le modifier avant approbation.
- Aucun document UX dédié n'existe : les exigences UX sont dispersées dans le SPEC, l'architecture et les stories.

### Implémentation et reconnaissance Graphify

Graphify (graphe existant : 7 885 nœuds) confirme les points d'attache suivants, vérifiés ensuite dans le code :

1. `RoadRageBootstrap` possède le `PlayerProfileStore` persistant **en mémoire seulement**.
2. `PlayerProfileFlowController` ouvre `CharacterSetupScreen`, exige un champ de nom, valide ce nom et confirme le personnage. `CharacterSetupScreen` n'est qu'une vignette UI 2D, sans aperçu 3D.
3. `LobbyFlowController` republie le profil dans le roster et l'encode (`displayName`, `characterId`) dans `NetworkPlayerConnectionPayload` avant `StartHost`/`StartClient`.
4. `NetworkedPlayerSpawnService` lit ce payload via `NetworkPlayerRegistry`, résout le `CharacterId` dans `CharacterCatalog`, puis l'écrit dans `NetworkedPlayerState`.
5. `NetworkedPlayerPresentation` instancie déjà le prefab visuel correspondant au `CharacterId`. Cette chaîne est précisément l'adaptateur à conserver pour la sélection Rookie/Veteran.
6. `NetworkedRageState` est une brique host-authoritative réutilisable avec tuning ScriptableObject. `NetworkedAIVehicleState` ne porte encore qu'un index de route ; il n'y a ni Fear, ni archétype de conducteur, ni trafic IA réel.
7. `NetworkedVehicleState` possède déjà des dommages Wheel/Engine/Brake. C'est une fondation à adapter ; ce n'est pas une simulation de pièces achetables complète.
8. Deux sources d'argent divergent déjà : `NetworkedPlayerState.Money` est un placeholder HUD et `NetworkedCrewEconomyState.CrewWallet` est le contrat planifié. Aucune ne convient à la nouvelle économie individuelle.

## 3. Conflits déterminants

| Domaine | État actuel | Nouvelle direction | Décision |
| --- | --- | --- | --- |
| Profil | Nom manuel, confirmation, session mémoire | Création automatique, nom Steam, persistance | REFACTOR ciblé |
| Personnage | Catalogue/id et visuel réseau existent | Rookie par défaut, Veteran visuel seulement, menu principal | KEEP + ADAPT |
| Menu | Play ouvre un placeholder/setup ; pas d'aperçu 3D | sélection et aperçu inspectable dans le menu | REFACTOR ciblé |
| Lobby | Peut republier le profil même après entrée | profil figé dans le lobby | KEEP + ADAPT |
| Spawn | payload puis `CharacterId` puis présentation déjà cohérents | spawn avec modèle sélectionné | KEEP |
| Steam | utilisé pour services/lobbies ; identité non exposée au profil | identité Steam obligatoire | KEEP + ADAPT |
| Rage | rage configurable par cible | Rage + Fear configurable par archétype | KEEP + ADAPT |
| Économie | crew wallet planifié et placeholder par joueur | portefeuille individuel | REFACTOR avant économie réelle |
| Run | une boucle MVP complète et redémarrage au début | sandbox maintenant ; runs/levels/checkpoints plus tard | REFACTOR de roadmap et contrats |
| Boss/ville/autoroute | endpoint générique et route unique planifiés | cible MVP 2, détails provisoires | DEPRECATE dans le MVP actuel ; conserver comme direction |

## 4. Matrice de préservation

| Zone | Epic/Story | Statut | Décision | Motif / changement requis |
| --- | --- | --- | --- | --- |
| Bootstrap, scènes, UI notices | 1.1 | implémenté | KEEP | Point d'entrée et erreurs visibles restent valides. |
| Lobby shell et paramètres | 1.2 | implémenté | KEEP + ADAPT | Conserver Create/Join/Start ; retirer le détour profil et ne pas exposer la sélection dans le lobby. |
| Création de profil et nom libre | 1.3 | implémenté | REFACTOR | Supprimer le flux de création/validation manuel ; remplacer par profil Steam persisté. Story historique inchangée. |
| Rookie/Veteran greybox et catalogue | 1.3/1.4 | implémenté | KEEP + ADAPT | Réutiliser ids/prefabs ; Rookie par défaut ; aucun attribut gameplay. |
| Entrée monde locale | 1.5/1.6 | implémenté | KEEP + ADAPT | Lire le profil persistant, non l'écran de setup. |
| Services Steam, room, join | 2.1–2.3 | implémenté | KEEP | Steam est maintenant aussi la source d'identité, pas seulement le transport/lobby. |
| Roster/ready | 2.4 | implémenté | KEEP + ADAPT | Roster peut afficher profil choisi, mais bloque toute modification après entrée. |
| Payload, spawn et présentation | 2.5 | implémenté | KEEP | Préserver le payload et `CharacterId`; remplacer uniquement la source du profil. |
| HUD/lifecycle | 2.6–2.7 | implémenté | KEEP + ADAPT | Clarifier que `Money` est un placeholder à retirer avant économie. |
| Conduite, sièges, dégâts | 3.1–3.6 | implémenté | KEEP + ADAPT | Les dommages à composants sont une base utile ; ne pas en déduire le système de réparations maintenant. |
| Rage et tuning | 4.1 | review | KEEP + ADAPT | Étendre ultérieurement vers une réponse Rage/Fear data-driven, sans casser les appels actuels. |
| Validation actions passager | 4.2 | review | KEEP | Pipeline intent/host/cooldown réutilisable. |
| Action Rage et outils dev | 4.3 | review | KEEP + ADAPT | Garde sa valeur de sandbox ; supprimer plus tard les hypothèses de « passager uniquement » si elles limitent les futures interactions. |
| Marqueur incident | 4.4 | review dans le statut, `done` dans sa spec | KEEP + ADAPT | Marqueur minimal réutilisable ; ne pas le promouvoir en économie/loot. |
| Action 4.5, checkpoint 4.6 | backlog | DEPRECATE | Ils servent l'ancienne boucle à trois actions ; les remplacer par des stories correctives approuvées. |
| Crew wallet et upgrade commun | 6.5–6.8 | backlog | REMOVE du futur MVP 1 | Contradit l'économie individuelle. Aucune implémentation à supprimer aujourd'hui. |
| Boss/Rage Road/boucle intégrée | 5–7 backlog | DEPRECATE comme MVP 1 | Reporter au MVP 2, sous contrats plus génériques. |
| Stockage profil Steam, menu aperçu, Fear, wallet individuel, contrat run/level | absent | NEW | Nouvelles fondations ou décisions nécessaires. |

## 5. Séparation de roadmap

### MVP 1 — Foundation Sandbox (nouveau sens du MVP courant)

Objectif : prouver indépendamment les briques réutilisables dans les scènes de développement et les petits parcours existants. Une story peut utiliser des mocks et des greyboxes ; elle ne doit pas exiger la ville, l'autoroute, une progression complète ou le boss.

- profil Steam persistant, sélection visuelle et spawn cohérent ;
- lobby/session/runtimes séparés ;
- véhicules, NPC, interactions, objets, inventaire et wallet individuel, lorsqu'ils sont introduits ;
- Rage/Fear et effets de provocation/intimidation configurables ;
- identifiant de véhicule et traçabilité source de litter ;
- dommages véhicule avec extension raisonnable vers pièces ;
- tests solo et réseau ciblés, puis seulement des checkpoints de fondation.

### MVP 2 — Advanced Gameplay Assembly

Objectif : composer les briques validées en run roguelite : niveaux ville/autoroute, progression, boss, transitions, équilibrage et checkpoints inter-niveaux. Les ressources de run survivent seulement selon les règles du run ; les cosmétiques/profil persistent entre les runs. Une mort d'équipe revient au checkpoint inter-niveaux et réinitialise les ressources temporaires. Les règles exactes d'actif obligatoire à la reprise restent ouvertes.

Les idées « vieille dame », camion, véhicule précis du boss, récompenses $1/$10/$20, phases, liste finale des pièces et tuning sont **provisoires** : elles ne deviennent pas des abstractions ou contrats génériques maintenant.

## 6. Impact des artefacts

| Artefact | Impact | Action proposée |
| --- | --- | --- |
| `SPEC.md` | significatif | Remplacer le succès MVP intégré par le sandbox MVP 1 ; ajouter les catégories Locked / Target / Provisional. |
| `mvp-scope.md` | significatif | Distinguer MVP 1 et MVP 2 ; retirer la cardinalité imposée de boss/route/Rage Road du MVP 1. |
| `gameplay-model.md` | significatif | Documenter niveaux, ressources de run, checkpoints et économie individuelle, en séparant exigences verrouillées et direction. |
| `module-composition.md` | significatif | Remplacer crew wallet par wallet individuel et ajouter les limites profile/identity/lobby/runtime. |
| `ARCHITECTURE-SPINE.md` | significatif | Réviser AD-5, 6, 8, 15, 16, 17, 22, 24, 26 ; conserver host authority, scènes/sandboxes et data ScriptableObject. |
| UX | significatif | Créer un contrat UX léger : menu avec aperçu 3D, sélection pré-lobby, lobby séparé et non éditable. |
| `epics.md` | significatif | Garder les stories terminées ; ajouter un epic correctif avant toute suite d'Epic 4/5 ; superséder les stories backlog incompatibles. |
| `sprint-status.yaml` | significatif mais post-approbation | Conserver les statuts historiques, marquer 4.5/4.6 superseded et ajouter les stories correctives en backlog. |
| `AGENTS.md` / project-context | mineur | Mettre à jour les pointeurs MVP et invariants après adoption. |
| Code/tests/scènes | aucun maintenant | Aucun changement autorisé à cette étape. |

## 7. Impact Epic et stories

- **Epic 1 :** reste valide sauf Story 1.3. Son résultat est préservé, mais sa création manuelle devient une dette corrective, non une story rouverte.
- **Epic 2 :** reste fortement valide. Le chainage profile → payload → spawn est le mécanisme à conserver. Ajouter un verrou d'immuabilité de sélection dans le lobby.
- **Epic 3 :** reste valide comme sandbox véhicule. Les trois indicateurs de dommage sont une amorce acceptable pour de futures pièces ; ne pas construire réparations/vol de pièces maintenant.
- **Epic 4 :** 4.1–4.4 attendent la revue finale. Ne pas poursuivre 4.5/4.6 dans leur forme actuelle. 4.4 doit être considérée revue/à revoir selon décision sur la divergence de statut.
- **Epics 5–7 :** la logique traffic/Rage/Fear, économie, bosses et intégration reste directionnelle, mais les stories actuelles sont séquencées pour l'ancienne boucle. Elles doivent être remplacées/replanifiées, pas simplement renommées.

## 8. Stories correctives minimales proposées

| ID proposé | Type | Maintenant ? | Dépendances | Critères d'acceptation de haut niveau |
| --- | --- | --- | --- | --- |
| CC-1 Profil Steam et sélection menu | Fondation | oui | 1.1–1.4, Steam 2.1 | Au premier lancement, profil créé depuis Steam ; Rookie par défaut ; aucun nom libre ni écran profil ; Rookie/Veteran sélectionnables et sauvegardés depuis le menu ; aperçu 3D inspectable ; modèles purement visuels. |
| CC-2 Frontière lobby/session/spawn du profil | Fondation | oui | CC-1, 2.2–2.5 | La sélection est gelée dès l'entrée lobby ; payload/spawn utilisent le choix sauvegardé ; lobby/session/runtime restent trois états distincts ; solo et multijoueur spawnent le même choix ; tests de persistance et de non-modification lobby. |
| CC-3 Contrat économie individuelle | Fondation | avant toute récompense/achat | 2.5, 2.6 | Une source d'autorité par joueur remplace la direction crew wallet ; HUD et transactions n'utilisent plus `NetworkedPlayerState.Money` comme placeholder ; aucun partage ni progression persistante ajoutés. |
| CC-4 Réponse NPC Rage/Fear configurable | Fondation | avant trafic/comportements | revue 4.1–4.4 | Conserver Rage ; ajouter seulement le minimum pour Fear, seuils et profils d'archétype data-driven ; actions décrivent leur effet ; aucune liste d'archétypes, IA complète ou boss. |
| CC-5 Contrat de run/niveau/checkpoint | Design + fondation légère | avant MVP 2, pas avant | CC-3/CC-4 | Définir frontières run/profile et checkpoint inter-niveau ; documenter autorité et reset ; aucune ville, autoroute, boss ni checkpoint jouable imposé. |

**Gate préalable :** réaliser les revues de code 4.1–4.4 et consigner le statut réel de 4.4 avant de modifier les stories ou de commencer CC-4.

## 9. Recommandations d'architecture — strict minimum

1. Conserver un profil local persistant distinct des `NetworkBehaviour` : identité Steam + sélection cosmétique seulement. Le runtime reste dans `NetworkedPlayerState` et les objets spawnés.
2. Créer une petite frontière Steam identity (lecture du display name/SteamId) ; pas de fallback offline, de compte maison ni de backend.
3. Garder `CharacterId` stable et la chaîne payload/spawn/presentation existante. Le menu devient la seule écriture de sélection avant lobby.
4. Pour Rage/Fear, garder les états host-authoritative et les définitions statiques data-driven. Le véhicule IA ne doit pas contenir directement les règles de tuning ; aucune hiérarchie d'effets générique n'est justifiée.
5. Introduire un wallet par `ClientId`/joueur au moment où l'économie devient réelle. Les récompenses/achats sont host-authoritative ; objets et inventaire sont également par joueur.
6. Conserver les composants Wheel/Engine/Brake, mais présenter les pièces remplaçables comme extension future ; ne pas multiplier aujourd'hui les classes de composants.
7. Si litter est introduit, son instance garde une référence Netcode-safe vers le véhicule source/son identité, pas une recherche globale fragile. Cette story est future.
8. Le futur `Run` orchestre les niveaux et checkpoints ; le profil ne stocke jamais monnaie temporaire, items de run, vie, siège ou état de lobby.

## 10. Risques et garde-fous

- **Régression du flux réseau :** ne pas réécrire payload/spawn ; adapter uniquement la source du profil.
- **Profil confondu avec runtime :** interdire l'écriture de l'état de jeu dans le stockage persistant.
- **Steam couplé au gameplay :** Steam fournit l'identité et le transport, pas les statistiques ou l'économie.
- **Sélection mutable dans le lobby :** geler le choix avant publication roster et connexion.
- **Rage/Fear codés dans un contrôleur de véhicule :** garder tuning et réaction séparés des mouvements.
- **Portefeuille partagé réintroduit :** bloquer `CrewWallet` avant toute story économie ; migrer explicitement HUD et transactions.
- **Ville/boss codés dans des systèmes génériques :** MVP 2 uniquement, avec déclencheurs/rewards simples et configurables.
- **Sur-ingénierie :** aucune sauvegarde de run, architecture d'items universelle, fallback Steam ou machine à états de niveaux maintenant.
- **Autorité floue :** persistant local = profil ; lobby/session = plateforme/connexion ; runtime synchronisé = hôte ; présentation/menu = local.

## 11. Chemin recommandé

1. Revoir 4.1–4.4 et résoudre la divergence documentaire de 4.4.
2. Approuver cette correction puis mettre à jour SPEC, scope, architecture, UX et epics en une passe cohérente.
3. Exécuter CC-1 puis CC-2, avec un test de redémarrage et un test host/client.
4. Replanifier Epic 4/5/6 autour de CC-3 et CC-4 ; garder CC-5 en préparation de MVP 2.
5. Ne commencer l'assemblage ville/autoroute/boss/checkpoint qu'après validation des briques sandbox.

## 12. Checklist Correct Course

- [x] 1.1–1.3 Déclencheur, problème et preuves identifiés (Story 4.4, divergences d'artefacts et code inspecté).
- [x] 2.1–2.5 Impact Epic analysé et ordre réévalué.
- [x] 3.1–3.4 SPEC, architecture, UX, artefacts secondaires analysés ; aucun UX contract distinct trouvé.
- [x] 4.1–4.4 Options évaluées : ajustement direct **non suffisant** ; rollback **non viable** ; revue MVP **nécessaire** ; approche retenue = hybride (replanification + refactors minimaux).
- [x] 5.1–5.5 Proposition, impacts, action plan et handoff établis.
- [x] 6.1–6.2 Proposition relue.
- [x] 6.3 Approbation explicite reçue le 2026-09-13.
- [x] 6.4 `sprint-status.yaml` validé après mise à jour.
- [x] 6.5 Handoff : artefacts PM/Architect appliqués ; le développement commence après les revues finales 4.1–4.4.

## Handoff après approbation

**Appliqué après approbation :** package SPEC, spine d'architecture, contrat UX, `epics.md`, statut de revue Story 4.4 et `sprint-status.yaml`. La validation du suivi de sprint est passée.

**Prochain handoff :** Developer effectue les revues 4.1–4.4, puis commence 4.5 suivi de 4.6. Critères de succès : aucun mécanisme de profil manuel, sélection persistante pré-lobby, spawn cohérent, économie individuelle planifiée et futures fondations sans verrouillage du gameplay MVP 2.
