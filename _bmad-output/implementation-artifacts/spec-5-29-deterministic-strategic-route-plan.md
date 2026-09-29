---
title: 'Story 5.29 -- RoutePlan strategique deterministe'
type: 'feature'
created: '2026-09-28'
status: 'done'
baseline_commit: 'd3e8945d33dcd8d54c2ce7f5cbfccc468543d95e'
review_loop_iteration: 0
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le modele routier signe possede des corridors, mouvements et portails stables, mais aucun plan ne relie une localisation a une sortie. Le routage V1 tire un virage a chaque carrefour et peut rediriger vers une sortie choisie par proximite geometrique.

**Approach:** Produire un `RoutePlan` strategique complet sur la topologie compilee, avec identites ordonnees, sortie, cout, raison et version du modele. Les preferences de virage influencent les couts et la variation reproductible ; une destination inaccessible ou un plan devenu invalide donnent un resultat explicite.

## Boundaries & Constraints

**Always:** Parcourir les `LaneConnection` et `JunctionMovement` diriges, et ne retenir que des continuations menant reellement a l'objectif. Le cout global d'une route complete combine distance dirigee restante (corridors et mouvements, depuis le `s` courant jusqu'au `s` du portail) et cout de preference issu d'un tirage deterministe ; publier ces composantes et le cout total finis. La preference n'est pas un choix glouton local : a continuations de meme cout de distance, la probabilite de selection suit les poids authores, et augmenter un poids ne reduit pas sa preference a cout aval fixe. Deriver l'alea de la graine de session, de l'identite trafic stable et d'un domaine/compteur de decision, jamais de `NetworkObjectId` seul. L'ordre des collections ne change pas le resultat. Verifier identites et version avant reutilisation ; calcul et replan sont bornes par les etats topologiques, pas par le budget d'aretes V1.

Sur chaque visite de corridor ou mouvement, le `s` ne decroit jamais : transition au bout de l'element seulement, puis `s=0` sur le suivant. Un portail est atteignable sur une visite seulement si son `s` est strictement devant le `s` d'entree ; l'egalite ne vaut pas franchissement. Une sortie derriere le depart exige une boucle topologique complete et une nouvelle visite du corridor, identifiable par une occurrence repetee dans le plan. Une boucle fermee sans sortie termine par `NoRoute` ; une repetition d'ID ou de zone geometrique n'est pas a elle seule une erreur.

Poids negatifs/non finis : validation refusee. Parmi les choix qui atteignent l'objectif, un poids nul ne participe pas au tirage si un poids positif existe ; il demeure une route admissible si les choix positifs ne menent pas a l'objectif. Si tous les choix admissibles valent zero, tirage uniforme deterministe et diagnostic `ZeroWeightFallback`, sans rendre la destination inaccessible.

Le resultat separe `Outcome` (`Planned`, `Replanned`, `NoRoute`, `InvalidInput`), `Reason` (`Requested`, `StalePlan`, `InvalidStart`, `StaleLocalization`, `DestinationUnavailable`, `DestinationUnreachable`) et diagnostics dont `ZeroWeightFallback` ; le repli peut ainsi accompagner une route reussie. Un plan existant avec version ou ID obsolete est replannifie depuis la localisation actuelle, jamais reutilise silencieusement.

**Ask First:** Changer la topologie ou les poids authores de `MVP_Run`, la semantique des portails ou le contrat de version du modele ; ajouter une dependance ou une assembly.

**Never:** Produire chemin local, trajectoire, vitesse, volant, grant ou effet sur le cycle de vie d'un vehicule ; utiliser NavMesh, proximite euclidienne, `WaypointIndex`, memoire auto-evitante V1, budget d'aretes V1 ou redirection gloutonne vers la sortie la plus proche.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Route normale | Localisation valide, sortie accessible | Occurrences continues d'IDs corridor/mouvement avec intervalles `s`, portail, couts et version | N/A |
| Replan | Plan anterieur stale, ID disparu ou demande explicite | Nouveau plan valide avec `Replanned` et raison `StalePlan`/`Requested`, ou echec nomme | Aucun effet vehicule |
| Entree invalide | Localisation absente ou version du modele differente | `InvalidStart`/`StaleLocalization` | Aucun plan suppose valide |
| Destination indisponible | Sortie absente ou fermee | `NoRoute` + `DestinationUnavailable` | Aucun despawn/teleport/reinsertion |
| Destination inaccessible | Sortie presente mais sans route dirigee | `NoRoute` + `DestinationUnreachable`, calcul termine | Aucun effet vehicule |
| Portail sur corridor courant | `s` devant, egal ou derriere le depart | Direct si strictement devant ; sinon boucle topologique ou `NoRoute` | Aucun saut arriere |
| Cycle legal / ferme | Giratoire avec sortie / cycle sans sortie | Nouvelle visite permise / `NoRoute` borne | Pas de rejet par position seule |
| Poids invalides ou nuls | Negatif/non fini ; zero melange aux positifs ; tous admissibles a zero | Validation refusee ; zero exclu du tirage positif ; repli uniforme `ZeroWeightFallback` | Pas de biais silencieux |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs:253,500,580` -- `Version`, corridors, connexions, mouvements, portails et recherches ; `GetSuccessorCorridors` ne couvre que les connexions.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs:39,409,461,563` -- `RoadId`, liens diriges, poids de mouvement et portails.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs:88` -- `RoadLocation` porte element, `s`, `ModelId` et `ModelVersion` ; aucun deplacement physique.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs:407` -- poids actuellement controles pour finitude seulement ; ajouter le refus des negatifs.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/MigrationReport.cs:493` -- parcours de reachability source, utile comme precedent ; pas un planner runtime.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelDocument.cs:71` -- charge le modele `MVP_Run.road-model.json` sans ouvrir la scene.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs:75` -- matrices de ratios et graines V1, scenarios a reprendre sans copier l'algorithme.
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json` -- modele reel : 44 corridors, 72 mouvements, 0 connexion, 4 entrees et 4 sorties ; test indispensable du parcours par mouvements.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlan.cs` et `RoutePlanner.cs` -- contrats immuables des occurrences dirigees et intervalles `s`, resultat/codes stables, couts decomposes, selection globale bornee et replan explicite.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs` -- refuser les poids negatifs ; garder un repli uniforme diagnostique si tous les choix faisables valent zero.
- [x] `Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs` -- matrice ci-dessus : ratio 30/50/20 sur trois continuations synthetiques faisables de meme cout aval (pas sur toute la carte), preference monotone, branche fortement ponderee en impasse, permutations d'ordre, graines/identites/domaines ; portail devant/egal/derriere avec et sans retour, cycle ferme, zeros mixtes/tous zeros, version/ID stale. Sur le modele signe `MVP_Run`, verifier chaque entree vers les quatre sorties par mouvements (aucune `LaneConnection`), continuite et bornes de chaque occurrence, IDs/version et absence d'effet de cycle de vie.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- suivre le statut de la story ; `graphify update .` apres les sources.

**Acceptance Criteria:**
- Given le modele compile et une localisation courante, when le planner recoit un objectif de sortie, then le plan ordonne des IDs existants relie le depart au portail par la seule topologie, porte raison/cout/version et reste distinct d'un chemin ou d'une commande de conduite.
- Given trois continuations faisables de cout aval egal ponderees 30/50/20, when des identites/graines variees sont planifiees, then les frequences reproduisent ces ratios et la meme entree de decision donne le meme plan ; un choix prefere menant a une impasse est exclu avant le tirage.
- Given un portail derriere le `s` courant, when le planner cherche la sortie, then il n'y parvient qu'apres une vraie boucle dirigee et une nouvelle visite du corridor, ou renvoie `NoRoute` en temps borne ; aucun saut arriere ni rejet automatique d'une zone revisitee.
- Given des poids nuls parmi les chemins admissibles, when un calcul est demande, then les positifs gouvernent le tirage s'ils existent ; un ensemble admissible tout nul utilise un repli uniforme signale, sans cacher une destination atteignable.
- Given une sortie inaccessible ou un plan obsolete, when un calcul ou replan est demande, then le resultat nomme sa cause et termine sans modifier le monde.

## Spec Change Log

## Review Findings

- Corrige : une boucle positive ne masque plus une sortie accessible par une continuation de poids nul.
- Corrige : un portail au-dela de la longueur de son corridor est `DestinationUnavailable`.
- Ajoute aux tests : fermeture de la sortie planifiee, distance totale egale a la somme des occurrences, boucle positive avec seule sortie de poids nul.

## Design Notes

La topologie signee `MVP_Run` contient 44 corridors, 72 mouvements, zero connexion, quatre entrees a `s=0` et quatre sorties a `s≈9,5` ; chacune des entrees atteint les quatre sorties par 4–7 transitions. Ses autres embranchements portent aussi 60/40 et 50/50 : le ratio 30/50/20 se mesure sur un graphe controle a cout aval egal, pas sur la distribution de toutes les routes reelles. L'objectif peut etre une sortie precise ou « toute sortie » ; le resultat nomme toujours le portail retenu. La representation des visites distingue le depart partiel d'un retour ulterieur sur le meme element, afin de permettre un tour de giratoire sans revisite indefinie du meme etat dirige.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story529RoutePlanTests"` -- fixture verte, zero erreur Console.
- `.\scripts\validate.ps1 -TestMode EditMode` -- suite complete verte sur le modele reel.

**Resultats (2026-09-29) :** fixture 5.29 `9/9` ; suite complete `937/937`, 0 ignore, 0 inconclusif, `scriptCompilationFailed=false`, 0 erreur Console depuis le curseur 51, scene `MVP_Run` propre. `graphify update .` execute apres le dernier changement source ; graphe 5 326 noeuds, vue agregee au-dela du seuil utile 5 000. Les suppressions paralleles de `Assets/Readme.asset` et `Assets/TutorialInfo/` ont ete laissees intactes.

## Suggested Review Order

**Selection et cout global**

- Point d'entree : valide les identites, la localisation et les sorties disponibles.
  [`RoutePlanner.cs:32`](../../Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs#L32)

- Ne tire que parmi les continuations qui atteignent une sortie sans cycle inutile.
  [`RoutePlanner.cs:109`](../../Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs#L109)

- Combine distance dirigee et preference deterministe sur la route complete.
  [`RoutePlanner.cs:140`](../../Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs#L140)

**Position et reutilisation**

- Traite le `s` initial et autorise une nouvelle visite apres un tour dirige.
  [`RoutePlanner.cs:178`](../../Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs#L178)

- Refuse la reutilisation si identites, progression ou continuite sont devenues invalides.
  [`RoutePlanner.cs:309`](../../Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs#L309)

**Contrats et preuves**

- Expose occurrences, progression, couts et codes de resultat sans effet de conduite.
  [`RoutePlan.cs:6`](../../Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlan.cs#L6)

- Refuse les poids de mouvement negatifs dans le validateur du modele.
  [`RoadModelValidator.cs:407`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs#L407)

- Prouve ratios, cycles, poids nuls, replan et 16 routes du modele reel.
  [`Story529RoutePlanTests.cs:11`](../../Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs#L11)

- Conserve le graphe limite au code RoadRage lors de sa mise a jour.
  [`.graphifyignore:58`](../../.graphifyignore#L58)
