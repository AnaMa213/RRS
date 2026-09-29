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

- Decision (code review 2026-09-29) : la reutilisation accepte un plan a visites repetees ; l'occurrence courante est choisie par progression acquise, intervalle `s` et non-regression stricte ; `StalePlan` est reserve a une vraie invalidation (recul sans visite porteuse), plus jamais a un plan simplement cyclique.
- Decision (code review 2026-09-29) : `RouteRequest` immuable remplace la signature positionnelle ; `RouteSeed` / `DecisionCounter`, types distincts sans conversion implicite, interdisent l'echange des deux `ulong`.
- Documentation (code review 2026-09-29) : `epic-5-context.md` attribue par commentaire de provenance (sources normatives nommees) ; faits normatifs et marqueurs historiques restaures ; formulations de 5.47 harmonisees sur `epics.md`.

## Review Findings

- Corrige : une boucle positive ne masque plus une sortie accessible par une continuation de poids nul.
- Corrige : un portail au-dela de la longueur de son corridor est `DestinationUnavailable`.
- Ajoute aux tests : fermeture de la sortie planifiee, distance totale egale a la somme des occurrences, boucle positive avec seule sortie de poids nul.

### Review Findings

Revue adversariale du 2026-09-29 (lentilles : blind-hunter, edge-case-hunter, verification-gap, acceptance-auditor ; base `d3e8945`, diff `review-5-29-diff.patch`, 12 fichiers +1133/-13). 4 decisions, 20 patches, 1 defer, 7 constats ecartes comme bruit. Tous les constats decision/patch sont resolus et appliques le 2026-09-29 (decisions du proprietaire : D1:2, D2:1, D3:2, D4:1) ; chaque constat porte sa resolution ci-dessous.

- [x] [Review][Patch] (ex-Decision D1) `epic-5-context.md` : mettre la mise a jour documentaire en face de son vrai perimetre — resolu : commentaire de provenance date en tete du document, nommant les sources normatives (`epics.md`, AD-42/AD-48, clarification Gate A du 2026-09-28) ; aucune reecriture de l'historique Git ; le contenu legitime est conserve.
- [x] [Review][Patch] (ex-Decision D2) `epic-5-context.md` : restaurer les faits normatifs, harmoniser 5.47 — resolu : selection du personnage avant lobby, preuves et reserves Gate A (cinq carrefours conventionnels, separation sous hauteur vehicule, coins nord desactives, resultats par coin/mouvement, empreinte d'entree canonique, sequence cinq-portes de la 5.51, « ne signe pas Gate A »), frontieres d'autorite V1/NavMesh et marqueurs `(historical)` restaures ; la clarification de relief roulable du 2026-09-28 est conservee ; les deux formulations de 5.47 sont harmonisees sur `epics.md` (5.46 decide, 5.47 conditionnel non specifie, saute si budget satisfait).
- [x] [Review][Patch] (ex-Decision D3) Reutilisation des plans a visites repetees sans `StalePlan` abusif — resolu : `TryReuse` choisit l'occurrence par progression acquise, intervalle `s` et non-regression stricte a progression egale ; un retour de giratoire reutilise le plan (`Planned`/`Requested`), un recul reel sans visite porteuse reste `Replanned`/`StalePlan` ; tests ajoutes (`ReuseDistinguishesRepeatedVisitsOnALoop`).
- [x] [Review][Patch] (ex-Decision D4) Contrat de requete immuable a types distincts — resolu : `RouteRequest` remplace la signature positionnelle ; `RouteSeed` et `DecisionCounter`, types distincts sans conversion implicite, interdisent l'echange silencieux des deux `ulong` ; appels et tests migres ; aucune dependance ou assembly ajoutee.

- [x] [Review][Patch] Garde d'entree du constructeur `RoutePlan` (occurrences vide dereferencee) [Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlan.cs:55] — resolu : `ArgumentException` explicite.
- [x] [Review][Patch] Supprimer la condition morte `match < 0` de `RoutePlanner.TryReuse` (le test precedent la couvre) [Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:328] — resolu : supprimee par la reecriture de `TryReuse` (D3).
- [x] [Review][Patch] Supprimer le champ mort `Edge.From` (renseigne, jamais relu) [Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:12] — resolu : champ supprime, `Edge.Feasible` ajoute au passage.
- [x] [Review][Patch] Corriger le commentaire Bellman-Ford : couts non negatifs, pas strictement positifs [Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:140] — resolu : « couts non negatifs ».
- [x] [Review][Patch] Eviter le calcul double de `CanReachWithout` et le balayage lineaire de `IsClosed` [Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:117] — resolu : faisabilite calculee une fois par arete ; portails fermes indexes en `HashSet`.
- [x] [Review][Patch] Documenter l'egalite float exacte comme contrat de reutilisation (valeurs stockees re-emises telles quelles) [Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:353] — resolu : contrat documente dans `TryReuse` et la doc de `Advance`.
- [x] [Review][Patch] Documenter l'API publique : types, parametres de `Plan`, semantique destination vide / `existing` / `closedPortalIds`, fautes d'appel partageant `InvalidStart` [Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlan.cs:6] — resolu : docs XML ajoutees (enums, requete, resultat, semantiques).
- [x] [Review][Patch] Rendre falsifiables les assertions `Is.Not.EqualTo(Id(62))` (portail absent des modeles mort-ne) [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:208] — resolu : plan non nul + sortie attendue reelle (60 ou 61).
- [x] [Review][Patch] Isoler domaine et compteur dans `domainChangesDecision` [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:196] — resolu : domaine seul et compteur seul testes separement, les trois sensibilites assertees.
- [x] [Review][Patch] Renforcer les preuves AC2 : permutation et poids augmente compares sur le plan complet, ratios sur plusieurs identites, reproductibilite du plan entier [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:177] — resolu : equivalence de plan complet, ratios par identite (80 et 81), reproductibilite integrale, poids augmente compare en nombre de choix et en cout.
- [x] [Review][Patch] Observer `RouteOutcome.InvalidInput` et la convention `Plan` nul, avec un cas `s` hors bornes [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:265] — resolu : assertees sur version stale, element inconnu, non localise, `s` hors bornes.
- [x] [Review][Patch] Verrouiller le modele signe : 4 entrees, 4 sorties et 16 paires planifiees (test sinon vacue) [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:404] — resolu : comptes verrouilles.
- [x] [Review][Patch] Couvrir la selection : plus proche portail parmi plusieurs en avant, victoire de la continuation au cout aval moindre [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:247] — resolu : test `NearestAheadPortalWinsAndDistanceBeatsPreference` (portail le plus proche ; branche longue perdante sur 200 graines).
- [x] [Review][Patch] Verifier `PreferenceCost` > 0 quand un tirage a lieu et la decomposition du cout total [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:423] — resolu : > 0 et < 40 asserte sur 2 x 4000 graines.
- [x] [Review][Patch] Verifier l'absence de `ZeroWeightFallback` sur une route ponderee normale [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:156] — resolu : `Diagnostics == None` asserte.
- [x] [Review][Patch] Asserter `RouteReason.Requested` sur les chemins nominaux (plan frais, reutilisation, replan explicite) [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:280] — resolu : asserte sur les trois chemins.
- [x] [Review][Patch] Exercer la clause de destination de `TryReuse` (sortie differente du plan en cache) [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:275] — resolu : test `ReuseRefusesADifferentDestination`.
- [x] [Review][Patch] Verrouiller le refus des poids negatifs (code `NumericValueOutOfRange`, champ) et le cas non fini [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:243] — resolu : code et mouvement fautif verifies ; cas non fini couvert (`NonFiniteNumericValue`).
- [x] [Review][Patch] Ajouter un en-tete date/motive aux exclusions `.graphifyignore` (convention du fichier) [.graphifyignore:58] — resolu : troisieme passage date (2026-09-29).
- [x] [Review][Patch] Materialiser la preuve d'absence d'effet de cycle de vie (non-mutation du modele) [Assets/RoadRage/Tests/EditMode/Story529RoutePlanTests.cs:404] — resolu : non-mutation du modele et re-plan equivalent assertees.

- [x] [Review][Defer] Graphe Graphify au-dela du seuil utile, etat non reconcilie avec la documentation [graphify-out/GRAPH_REPORT.md] — deferred, pre-existing

## Design Notes

La topologie signee `MVP_Run` contient 44 corridors, 72 mouvements, zero connexion, quatre entrees a `s=0` et quatre sorties a `s≈9,5` ; chacune des entrees atteint les quatre sorties par 4–7 transitions. Ses autres embranchements portent aussi 60/40 et 50/50 : le ratio 30/50/20 se mesure sur un graphe controle a cout aval egal, pas sur la distribution de toutes les routes reelles. L'objectif peut etre une sortie precise ou « toute sortie » ; le resultat nomme toujours le portail retenu. La representation des visites distingue le depart partiel d'un retour ulterieur sur le meme element, afin de permettre un tour de giratoire sans revisite indefinie du meme etat dirige.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story529RoutePlanTests"` -- fixture verte, zero erreur Console.
- `.\scripts\validate.ps1 -TestMode EditMode` -- suite complete verte sur le modele reel.

**Resultats (2026-09-29) :** fixture 5.29 `9/9` ; suite complete `937/937`, 0 ignore, 0 inconclusif, `scriptCompilationFailed=false`, 0 erreur Console depuis le curseur 51, scene `MVP_Run` propre. `graphify update .` execute apres le dernier changement source ; graphe 5 326 noeuds, vue agregee au-dela du seuil utile 5 000. Les suppressions paralleles de `Assets/Readme.asset` et `Assets/TutorialInfo/` ont ete laissees intactes.

**Resultats corrections de revue (2026-09-29) :** fixture 5.29 `12/12` (`validate.ps1 -TestMode EditMode -TestFilter` : 0 erreur Console depuis le curseur 397, `scriptCompilationFailed=false`, scene `MVP_Run` propre, 0 ignore, 0 inconclusif) ; suite complete `-Profile Full` verte : `940/940`, 0 ignore, 0 inconclusif, aucune exclusion (profil Full : suite complete), 0 erreur Console depuis le curseur 429, `scriptCompilationFailed=false`, scene `MVP_Run` propre. `graphify update .` re-execute apres corrections : 5 337 noeuds / 14 278 liens, vue agregee au-dela du seuil utile (constat differe inchange, voir `deferred-work.md`).

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
