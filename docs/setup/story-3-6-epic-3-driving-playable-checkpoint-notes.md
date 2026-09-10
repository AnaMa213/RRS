# Story 3.6 - Notes de checkpoint jouable de conduite Epic 3

Date: 2026-09-10

## Preuves executees

- Revue de cloture retroactive des stories 3.1-3.5 menee avant cette story (commits `ff1b9da`, `f37f7c7`) : 3.1 et 3.4 etaient deja `done` (sprint-status juste perime) ; 3.2 n'a produit aucun defaut vivant ; 3.3 a revele et corrige un vrai bug (le chemin solo hors-ligne laissait un joueur mort monter en voiture, contrairement au chemin reseau) et differe un point mineur (proximite d'entree de siege fondee sur une position joueur non validee cote host, dette pre-existante depuis la Story 2.5, cf. `deferred-work.md`) ; 3.5 avait deja ete validee `done` sur demande explicite.
- La meme revue a mis en lumiere et corrige 3 regressions EditMode pre-existantes, invisibles jusqu'ici car chaque story ne relancait que son propre filtre de test : les bornes de rendu documentees de `Greybox_PlayerCar` (Story 1.4) n'avaient jamais suivi la rotation -90 degres du visuel operee par la Story 3.2, et la hierarchie de scene attendue par les Stories 1.5/1.6 (`Greybox_RoadStrip`) n'avait pas suivi le remplacement par la boucle a 4 segments de la Story 3.4.
- `Story36Epic3DrivingPlayableCheckpointTests` (nouveau) compose ces invariants en un seul gate : wiring `RunFlowController`/HUD dans `MVP_Run`, instance du vehicule partage presente en scene, ensemble complet des composants Epic 3 (sieges, recuperation, resurrection, degats/VFX) presents ensemble sur les prefabs joueur et vehicule, et frontiere asmdef `RoadRage.Features.Vehicles` toujours etanche.
- Suite EditMode complete relancee apres ajout des 4 tests de checkpoint ci-dessus : **258/258 tests verts**, 0 echec (executee via Unity MCP `run_tests`/`test_status`). Le 254/254 obtenu pendant la revue de 3.1-3.5 (avant que ces 4 tests n'existent) est mentionne plus haut comme preuve de cette revue anterieure, pas comme preuve du present checkpoint.
- Parcours reel partiel en Play Mode (`MVP_Run`, via Unity MCP `editor_play`/`Unity_RunCommand`) : un profil joueur valide est pose programmatiquement (`RoadRageBootstrap.Profiles.Set`), puis `RunFlowController.TrySpawnSelectedProfile` (l'API publique reellement appelee par le jeu) fait apparaitre le personnage greybox avec sa camera a pied suivant le joueur -- confirme visuellement par capture d'ecran. Voir "Limitations" ci-dessous pour ce qui n'a pas pu etre automatise au-dela de ce point.

## Greybox et placeholders

- Personnage, voiture et batiments restent des primitives greybox (aucun art final) ; le vehicule partage un seul mesh sans roues separees (le "vacillement roue" en degats est simule par une oscillation du corps, pas une roue dediee).
- HUD en jeu reste un affichage texte minimal (TMP_Text), sans mise en page finale.
- La carte `MVP_Run` est une boucle a 4 segments avec limites et quelques decors (blocs urbains, barils) -- pas un environnement final.
- Aucun trafic IA, aucune action passager, aucune economie, aucun boss, aucune Rage Road : tout cela reste hors scope jusqu'a l'Epic 4+.

## Reste pour l'Epic 4

- Le module Vehicules et le contrat de sieges (siege conducteur/passagers) sont la fondation directe requise par l'Epic 4 (les actions passager exigent un siege passager occupe).
- Le contrat de degats/team-wipe (Story 3.5) reste un stub : la condition all-dead est detectable, mais le redemarrage reel de run est stube jusqu'a l'Epic 7.
- La porte de proximite d'entree de siege se fie a une position joueur non validee cote host (voir `deferred-work.md`) -- sans impact en coop non-adversarial aujourd'hui, a revisiter si un contexte competitif ou un cheat public apparait.

## Limitations

- Le parcours golden-path complet (marche -> entree -> conduite -> sortie -> recuperation) n'a pas pu etre automatise de bout en bout dans cet environnement CLI : les evenements clavier injectes depuis l'exterieur via `UnityEngine.InputSystem` n'atterrissent pas de maniere fiable dans la fenetre de detection "cette frame" de `RunFlowController.Update()`, l'Editeur ne semblant avancer ses frames de simulation qu'au rythme des appels MCP plutot qu'en temps reel (reflection est par ailleurs bloquee par le bac a sable de l'outil, ce qui empeche d'appeler directement les methodes privees du chemin solo).
- Chaque mecanique individuelle (revendication/conduite 3.2, entree/sortie/camera 3.3, collision/recuperation par `R` 3.4, degats/resurrection/VFX 3.5) a deja ete verifiee manuellement en Play Mode reel par l'agent d'implementation de sa propre story (voir les sections Verification des specs `spec-3-2-*` a `spec-3-5-*`) ; ce checkpoint n'a pas trouve de raison de douter que l'enchainement complet fonctionne, mais n'en a pas la preuve automatisee de bout en bout.
- Verification hote/client Multiplayer Play Mode (etat vehicule partage visible d'un second client) : non executee dans cet environnement CLI, memes contraintes que le point ci-dessus plus l'absence d'un second processus/compte pour rejoindre localement.
- Build Windows dev : non genere dans cette session.

## Bloqueurs

Aucun bloqueur bureaucratique connu. Recommandation avant de considerer l'Epic 3 entierement livre en conditions reelles : une passe interactive manuelle de 2-3 minutes (clavier reel) dans `MVP_Run` couvrant marche -> `E` entree conducteur -> conduite -> `E` sortie -> collision volontaire + `R` recuperation, et si possible une session Multiplayer Play Mode host/client locale pour confirmer l'etat partage. Aucun de ces points n'a revele de doute pendant la revue de code ; il s'agit d'une confirmation de bon sens, pas d'un correctif en attente.
