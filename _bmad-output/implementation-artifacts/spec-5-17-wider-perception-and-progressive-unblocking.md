---
title: "Perception elargie et deverrouillage progressif"
type: "feature"
created: "2026-09-20"
status: "in-progress"
review_loop_iteration: 0
baseline_commit: "210f48811e3f99fbb93c5d2aa75885e65edcf878"
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le conducteur IA ne detecte aujourd'hui qu'un obstacle proche devant lui a chaque pas physique. Face a une route chargee, il attend soixante secondes puis se teleporte : il ne peut ni raisonner sur plusieurs usagers, ni debloquer sa route de maniere lisible.

**Approach:** Donner au conducteur une perception locale mise a jour a cadence authorisee, couvrant plusieurs vehicules et joueurs dans un rayon et un arc authorises. Cette perception alimente le leader et expose la meme vue a la future regle d'intersection; un blocage declenche aussitot une replanification, puis une escalade visible : recul sur si necessaire, retour ou contournement routier, et trottoir seulement en dernier recours controle, sans retirer ni teleporter le vehicule.

## Boundaries & Constraints

**Always:** Conserver `NetworkedAIVehicleDriverController` host-only et `VehicleDriveIntent` comme unique sortie vers la physique. Les valeurs de rayon, arc, cadence, delai de klaxon, recul, critere de gap et recours trottoir vivent dans `DriverProfileDef`; aucune constante de conduite dans le controleur. Une requete locale `Physics.*NonAlloc` a cadence reduite est acceptable et doit eviter tout balayage global par frame. Reutiliser `LaneGraph`/`LaneGraphRouting` pour replanifier et choisir un itineraire de contournement. La sequence est obligatoire : replanifier, reculer prudemment si cela laisse passer ou permet de reprendre la route, contourner sur la chaussee, puis seulement emprunter le trottoir si les options precedentes echouent et si la manoeuvre ne risque ni pieton ni obstacle. Le spawner reste l'unique porteur de `Despawn` aux portails de sortie, et `RecoverAtWaypoint` reste reserve aux gardes physiques existantes (void/retournement/encastrement), jamais a un blocage percu.

**Ask First:** Si un depassement exige une nouvelle voie authorisee, une nouvelle RPC, un changement de schema reseau, ou la modification des contrats d'intersection de la Story 5.18, arreter pour arbitrage humain.

**Never:** Ne pas ajouter de `NavMeshAgent`, de teleportation, reinsertion, despawn ou spawn de remplacement pour resoudre un bouchon. Ne pas utiliser le trottoir sans verifications locales positives de pieton et obstacle, ni comme premiere manoeuvre. Ne pas supprimer le recovery physique de 5.14, traiter la trajectoire subie apres collision, ni livrer les priorites/interblocages d'intersection de 5.18.

## I/O & Edge-Case Matrix

| Scenario | Entree / etat | Sortie attendue / comportement | Gestion d'erreur |
|----------|---------------|---------------------------------|------------------|
| Perception | plusieurs voitures et joueurs dans le rayon/arc | une vue locale bornee est rafraichie a la cadence authorisee; le leader pertinent alimente le modele | buffer sature : conserver les candidats les plus proches, sans allocation ni scan global |
| Obstacle | leader immobile ou ecart nul sur la route | replanification immediate et continue; pas de phase d'attente de 60 s | route de contournement absente : tenter un recul controle, puis continuer a reevaluer |
| Escalade | blocage persistant au-dela du delai authorise | klaxon visible, recul si utile, puis deviation sur chaussee des qu'un gap est acceptable | aucun gap routier : considerer le trottoir seulement apres les autres options |
| Dernier recours | aucun recul/retour/contournement routier possible | franchissement ponctuel du trottoir pour contourner l'obstacle puis retour sur route | pieton ou obstacle detecte : refuser la manoeuvre, continuer a reevaluer sans teleportation ni retrait |
| Collision | voiture projetee hors de sa trajectoire par un choc | le chemin de recovery physique 5.14 reste disponible | ne pas classer ce cas comme manoeuvre de degagement |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- couture host-only : remplacer `TryDetectLeader` (l.867, SphereCast a chaque `FixedUpdate`) par la perception cadensee; `FixedUpdate` (l.305), `ResolveLongitudinalPedal` (l.682), `TickLaneChangeEvaluation` (l.1004) et `SubmitIntentToPhysicsLayer` (l.1092) sont les points de branchement. `RecoverAtWaypoint` (l.1116) est hors scope pour les bouchons.
- `Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs` et `Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset` -- Def auteur des seuils IDM/MOBIL et de `LaneChangeEvaluationInterval`; ajouter et valider les reglages de perception/escalade ici.
- `Assets/RoadRage/Features/Vehicles/DriverModel.cs` -- fonctions pures IDM, `TryEvaluateLaneChange` (l.103) et `IsDeliberateStop` (l.230) a reutiliser pour leader, gap et decision, sans logique doublee dans le MonoBehaviour.
- `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs` -- contrat immuable a quatre axes deja consomme par la physique; aucun nouvel intent ni RPC.
- `Assets/RoadRage/Features/Vehicles/LaneGraph.cs`, `LaneGraphRouting.cs`, `LaneNode.cs` -- graphe, successeurs et redirection vers sortie; base de replanification et de retour route, sans NavMeshAgent.
- `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` -- seule autorite de retrait au `PortalExit`; aucune modification de cette regle par la story.
- `Assets/RoadRage/Tests/EditMode/Story514AiDrivesByIntentTests.cs` et `Story510LaneGraphAndRoutedTrafficTests.cs` -- contrats a conserver : pas d'ecriture directe de mouvement hors recovery et aucun retrait hors portail.
- `Assets/RoadRage/Tests/PlayMode/Story510RoutedTrafficPlayModeTests.cs` -- precedent d'integration dans `MVP_Run`; etendre ou ajouter le banc 5.17 dans cette scene, jamais seulement `Dev_*`.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs` + `ScriptableObjects/Vehicles/DriverProfileDef_Default.asset` -- authorer/valider rayon, arc, cadence, delai de klaxon, recul, gap et dernier recours trottoir -- comportement reglable sans litteral runtime.
- [x] `Assets/RoadRage/Features/Vehicles/TrafficPerception.cs` -- ajouter le plus petit modele pur de candidats et de selection leader/arc/gap/escalade -- les decisions restent testables sans scene et reutilisables par la Story 5.18.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- rafraichir un buffer local NonAlloc a cadence authorisee, alimenter IDM et la sequence replanifier/reculer/contourner/dernier recours trottoir securise -- une seule boucle de conduite, sans recovery abusif.
- [x] `Assets/RoadRage/Tests/EditMode/Story517WiderPerceptionAndProgressiveUnblockingTests.cs` -- couvrir rayon/arc, plusieurs candidats, buffer borne, cadence, leader, klaxon, recul, gap, ordre d'escalade et veto pieton/obstacle -- preuve de la matrice et des invariants.
- [x] `Assets/RoadRage/Tests/PlayMode/Story517WiderPerceptionAndProgressiveUnblockingPlayModeTests.cs` -- verifier dans `MVP_Run` un trafic bloque qui replanifie, recule et escalade sans teleport/despawn, avec trottoir seulement si sa voie est sure -- preuve runtime de la couture physique/route.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- passer `5-17-wider-perception-and-progressive-unblocking` a `in-progress` -- suivi de sprint.
- [x] `graphify update .` -- regenerer le graphe apres les changements source -- index de code a jour (3599 noeuds, 8606 liens, 155 communautes).

**Acceptance Criteria:**
- Given un conducteur IA et plusieurs joueurs ou vehicules a proximite, when la perception authorisee est evaluee, then elle conserve les candidats dans le rayon et l'arc, choisit un leader utile au modele de conduite et reste bornee par buffer et cadence, sans balayage global par frame.
- Given un obstacle bloque une route, when le conducteur le percoit, then il replanifie sans attendre, klaxonne apres le delai authorise, recule si cela libere le passage ou permet de reprendre sa route, puis tente un contournement routier des qu'un gap authorise est disponible.
- Given recul, retour et contournement routier impossibles, when le blocage persiste, then le vehicule ne peut emprunter le trottoir qu'en dernier recours apres veto local pieton/obstacle; sinon il continue a reevaluer sans teleportation, reinsertion, retrait ni remplacement. Seul le recovery physique existant peut encore corriger un etat physique invalide.
- Given la Story 5.18 consomme la perception, when elle est developpee ensuite, then elle dispose d'une vue locale stable sans ajouter un second scan de trafic.

### Review Findings

- [x] [Review][Patch] Remplacer la redirection gloutonne et le saut direct vers un portail par une route dirigee prouvee vers n'importe quelle sortie accessible [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs:587`] -- **Fait.** `ResolveRedirectToNearestExit` interroge `LaneGraphRouting.FindNextTowardReachableExit` et ne retombe sur l'immobilite inerte que si AUCUNE route ne rejoint un portail. La garde de la Story 5.10 `ADeadEndRedirectsTowardTheNearestExitInsteadOfEndingTheRoute` exigeait l'ancien contrat (`return exitIndex`, viser le portail le plus proche meme inatteignable) : elle a ete realignee sur le nouveau, en conservant ce qui comptait -- cas traite explicitement, jamais de retrait, defaut audible une fois.
- [x] [Review][Patch] Fonder la replanification sur l'accessibilite et un echec de route observe, pas sur la seule proximite geometrique ou l'immobilite [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs:737`] -- **Fait.** `TryReplanTowardsExit` exige un bloqueur REELLEMENT percu (`blockingCollider != null`) -- la seule immobilite d'un voisin ne suffit plus -- et sa cible vient de `LaneGraphRouting.FindNextTowardReachableExit`, une route dirigee prouvee vers un portail de sortie ATTEIGNABLE, jamais du noeud geometriquement le plus proche. Le waypoint courant est conserve jusqu'a sa vraie arrivee, et la cible doit passer `ValidatePath`. ANO-5.18-10 ajoute le declencheur manquant : l'absence de progres le long de la route (`TickRouteProgress`), c'est-a-dire un ECHEC DE ROUTE OBSERVE et non une vitesse nulle.
- [x] [Review][Patch] Identifier le vehicule a contresens depuis le segment de voie et lui attribuer seul la recuperation du face-a-face [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs:363`] -- **Fait.** `ResolveLaneConformance` lit le SEGMENT DE VOIE (`LaneGraph.TryGetRoadPosition`) : sens de circulation, puis distance a l'axe, puis `NetworkObjectId` -- `TrafficPerception.MustYield`. La designation existait ; ce qui manquait etait l'ATTRIBUTION de la recuperation, ajoutee par ANO-5.18-10 : dans un face-a-face, seul le vehicule mal place entre dans l'echelle des le palier 1. Le vehicule correctement place n'en sort qu'au palier 3, quand l'autre a manifestement echoue a se degager.
- [x] [Review][Patch] Conserver l'historique des manoeuvres echouees et n'accepter une recuperation qu'apres retour sur une voie et un sens valides [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs:694`] -- **Fait.** `failedManeuverMask` interdit de rejouer un palier echoue pendant l'episode, et `ResetUnblockingEpisode` n'efface l'episode qu'apres `HasRecoveredRoute` -- a moins de `demi-largeur + marge` de l'axe ET dans le bon sens. Une manoeuvre qui se termine hors voie ou a contresens est ANNULEE, pas validee. ANO-5.18-10 y ajoute la remise a zero de l'horloge de blocage, sans quoi un episode resolu laissait le vehicule a son dernier palier.
- [x] [Review][Patch] Corriger l'ordre d'escalade en replanification, recul utile, detour routier, puis trottoir et refuser les boucles de meme manoeuvre [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs:814`] -- **Fait, avec un ordre RE-SPECIFIE par la recette.** `JunctionRules.ResolveRecoveryStage` et `TryEscalateRecovery` posent un escalier unique : replanification puis evitement local (palier 1), recul (2), trottoir et espace voisin (3), dernier recours (4). **Ecart assume avec le libelle de cette finding** : elle demandait *replanification, recul, detour routier, trottoir*. La recette ANO-5.18 a re-specifie l'escalade comme *trafic normal -> evitement local -> recul/repositionnement -> manoeuvre elargie -> trottoir*, donc l'evitement local passe AVANT le recul. L'instruction de recette fait foi ; un contournement sur la chaussee est moins couteux et moins perturbant qu'un recul. Les boucles sont refusees par `failedManeuverMask` et bornees par `RetryCooldown`.
- [ ] [Review][Patch] Ajouter des regressions comportementales pour route bloquee, sortie alternative, demi-tour sur/refuse, file legitime, face-a-face et echecs repetes -- **Partiellement fait.** Couverts par `Story518StalledHeadOnTests` (face-a-face contre un pair arrete, file legitime exclue du face-a-face) et `Story518UnifiedRecoveryTests` (escalier complet, echecs repetes via les paliers, politesse du face-a-face, gel sur attente bornee). RESTE : route bloquee de bout en bout, sortie alternative reellement empruntee, et demi-tour sur/refuse -- ces trois la demandent un banc de flux, donc le Play Mode. [`Assets/RoadRage/Tests/PlayMode/Story517WiderPerceptionAndProgressiveUnblockingPlayModeTests.cs:43`]
- [x] [Review][Patch] Faire echouer fermee la verification d'occupation d'un portail lorsque son buffer est sature [`Assets/RoadRage/App/Run/PortalTrafficSpawner.cs:289`] -- **Fait** (verifie, pas modifie). `PortalTrafficSpawner.IsPortalClear` rend `false` des que `found >= clearanceHits.Length` : un tampon plein signifie environnement inconnu, jamais portail libre. La garde etait deja en place et commentee ; seule la case restait a cocher.

## Spec Change Log

### Reprise : immobilisation aux portails et croisement sur route a double sens

L'utilisateur signale que les IA ne quittent plus les tunnels et demande de ne pas confondre un
croisement normal sur deux voies avec une collision frontale. Diagnostic en lecture dans MVP_Run :
aux quatre portails 188/192/196/200, le mur `Col_Wall_Left` mesure reellement 1 x 5 x 16 m et est
tourne de 45 degres; son AABB monde mesure environ 12 x 5 x 12 m. L'ancienne prediction donnait
TTC=0 au spawn alors que la distance au mur etait 6 m. Le meme defaut sur les voitures diagonales
pouvait creer un faux danger entre voies distinctes.

Correction : geometrie orientee des BoxCollider conservee dans le snapshot; projection dans le
repere de conduite pour le leader; intersection continue des boites orientees pour la prediction
et les acteurs mobiles dans une manoeuvre. Les murs ne sont pas ignores et un vehicule qui
empiete dans la voie reste un danger. Enveloppe conservative maintenue pour les autres formes.
Tests prepares : quatre orientations du vrai prefab tunnel, croisement de vrais prefabs IA a
4 m d'ecart et empietement dangereux, invariance par rotation; le faux test PlayMode qui gelait
la vitesse est remplace par une sortie de tunnel mesuree avec la vraie physique dans MVP_Run.

Le checkpoint humain reste actif : aucun test ni Play Mode lance par l'agent. La recompilation
des scripts est permise pour charger le correctif (la restriction aux lectures de compilation
notee precedemment etait une interpretation trop large du checkpoint, pas une demande utilisateur).
Les scenarios de manoeuvre complets du document initial restent a valider; ce correctif du depart
ne constitue pas une validation de toute la Story 5.17.

### Reprise du 2026-09-20 apres deux essais manuels en echec

Le document utilisateur `C:/Users/Kenan/.codex/attachments/61060d6d-3bff-4dd9-b2bf-aae0c9277ec9/pasted-text.txt` est la demande courante, a lire integralement. Il autorise une refonte ciblee de la decision locale : anticipation des trajectoires relatives, priorite entre deux IA selon le sens reel des voies, manoeuvres engagees et retour au trajet. Ce complement prime les descriptions de l'ancienne implementation. Aucune nouvelle RPC, voie ou regle complete d'intersection n'est requise. Le checkpoint demande par l'utilisateur reste obligatoire : preparer les tests mais NE PAS lancer de tests, Play Mode, validate.ps1, ni simulation; controles de compilation/Console en lecture autorises. Ne pas annoncer une validation comportementale sans execution. Ne pas committer. Graphify sera actualise apres les changements (ce n'est pas un test).

Diagnostic source confirme : `TrySelectLeader` exclut les obstacles sans Rigidbody; l'ancien arc confond voie voisine et couloir de collision; les clearances scalaires ignorent l'obstacle central et ne testent pas l'enveloppe du vehicule; le point de detour se deplace avec le vehicule a chaque tick; le minuteur global tient lieu de manoeuvre; `TryReplanTowardsExit` saute un waypoint sans trajet local valide; aucune prediction ni arbitration n'existe. Le test PlayMode force la vitesse a zero et deplace l'obstacle avec l'IA, donc ne prouve aucun mouvement. Le profil rage immobilisant coupe le calcul avant la perception : conserver sa semantique voulue mais permettre une reaction de securite a un danger imminent. Verifier aussi le vrai contrat frein/recul du modele de pneus, pas uniquement le signe d'un intent.

L'Editeur est connecte, hors Play Mode, scenes MainMenuLobby et MVP_Run chargees et propres. Aucun changement scene/prefab n'est autorise a l'aveugle. Les profils, colliders et graphe du vrai prefab doivent etre inspectes; si besoin, utiliser seulement le CLI deja adopte avec garde Git/scenes. Les changements 5.17 actuels sont ceux a corriger, les conserver hors parties remplacees. Le commit 5.16 existe deja.

#### Correction attendue et preuves a preparer

- [ ] Tracer completement la boucle host-only, le graphe et ses directions, les colliders statiques/joueur/IA/pieton et la consommation physique de l'intent; corriger la cause partagee, sans modifier inutilement la physique.
- [ ] Remplacer la clearance fictive par une verification volumique bornee des trajectoires/poses tenant compte du gabarit, du braquage, du sol, des obstacles centraux, des pietons et du trafic mobile. Saturation = arret prudent, jamais espace libre suppose. Differencier positivement chaussee et trottoir, sans assimiler un espace inconnu a un trottoir praticable. Les surfaces greybox existent (Col_Roadway, Col_Sidewalk_*); preferer le contrat du graphe/geometrie authorisee, documenter toute convention transitoire.
- [ ] Perception locale cadensee incluant obstacles statiques et trajectoires relatives. Risque de collision avec temps/position de rapprochement pour face-a-face, croisement et rattrapage; freinage d'urgence independant du lissage IDM, sans marche arriere involontaire. Pas de balayage global de scene par tick.
- [ ] Manoeuvre stable avec cibles fixes dans le monde, progres mesure, fin/abandon/reessai borne et cooldown. Recul utile et lent avant detour si manque de place; degagement lateral puis depassement puis retour au trajet conserve. Ne jamais ecraser un obstacle central en accelerant vers le cote declare libre. Revalider la securite pendant l'engagement. Si toutes options impossibles, arret et reevaluation; supprimer le teleport de simple lenteur, garder uniquement les gardes physiques invalides.
- [ ] Arbitrage deterministe reciproque entre deux IA frontales : sens/position sur segments du LaneGraph d'abord, identifiant reseau stable comme departage. Le vehicule correct attend pendant que l'autre degage, sans miroir ni inversion de priorite a chaque pas. Plusieurs vehicules/pietons doivent pouvoir opposer leur veto.
- [ ] Reglages de prediction/manoeuvre necessaires authorables avec valeurs valides, conserves par la modulation du profil; aucun nouveau package, schema reseau ou systeme generaliste.
- [ ] Remplacer les tests 5.17 insuffisants par checks utiles de geometrie/prediction/priorite/stabilite/saturation et bancs PlayMode qui laissent fonctionner la vraie physique, mesurent recul/deplacement/degagement/retour et collisions, dans MVP_Run. Couvrir les onze scenarios du document utilisateur, avec infrastructure existante; un test seulement d'intent ne suffit pas. Ne pas rendre les erreurs silencieuses via LogAssert.ignoreFailingMessages global. Mettre a jour les tests de contrat existants si le refactor change une forme syntaxique, jamais affaiblir un invariant.
- [ ] Exposer une telemetrie/gizmo compact de l'etat, cible et motif de refus utile au prochain essai manuel; aucune dependance du comportement a l'outil debug.

Les anciennes cases cochees au-dessus ne prouvent pas les comportements : cette reprise et sa recette restent en attente jusqu'a execution humaine. Retour attendu de l'implementation : fichiers touches, causes corrigees, controles reellement effectues, limites et commandes exactes a executer par l'utilisateur.

## Design Notes

La reduction de cadence avec buffer `NonAlloc` est le plus petit mecanisme qui satisfait la borne de cout sans introduire un index spatial global. La perception de clearance ponctuelle de `RecoverAtWaypoint` est distincte : elle protege un joueur pendant une correction physique et ne devient pas une seconde perception de conduite. Le trottoir cesse d'etre une interdiction absolue mais reste une manoeuvre de dernier rang : son veto local doit couvrir pietons et obstacles, et le vehicule doit revenir sur le reseau routier des que possible.

## Verification

Dernier checkpoint : recompilation explicite terminee, `failed:false`, `errors:[]`;
nouvelle API de geometrie orientee chargee dans l'Editeur. Diagnostic sur les colliders existants
de MVP_Run : les huit murs lateraux des quatre tunnels ne declenchent plus la fausse collision
au spawn. Scenes MainMenuLobby et MVP_Run propres. Aucun test ni Play Mode execute; sortie
physique du tunnel, croisement en double sens et scenarios de manoeuvre a valider par l'utilisateur.

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story517WiderPerceptionAndProgressiveUnblockingTests"` -- a executer par l'utilisateur.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- suite runtime complete, incluant le banc `MVP_Run` 5.17, a executer par l'utilisateur.

**Manual checks (if no CLI):**
- Dans `MVP_Run`, bloquer une IA avec une autre voiture et observer replanification immediate, klaxon, recul utile puis deviation routiere; ne permettre le trottoir qu'en dernier recours sans pieton/obstacle, et verifier l'absence de retrait ou teleportation.
