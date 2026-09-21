---
title: "Regles d'intersection et prevention de l'interblocage"
type: 'feature'
created: '2026-09-20'
status: 'in-progress'
review_loop_iteration: 0
baseline_commit: "210f48811e3f99fbb93c5d2aa75885e65edcf878"
context:
  - `{project-root}/docs/setup/build-workflow-rules.md`
  - `{project-root}/_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md`
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Rien n'arbitre une intersection : `LaneNode` ne porte que `role`, `successors`, `turnWeights`, `exitReusesEntry`, et le conducteur IA tire son successeur au sort en arrivant sur le noeud de decision. Deux IA qui se rencontrent n'ont donc aucune regle commune pour decider qui passe : l'etat « deux voitures arretees, aucune ne sachant qu'elle a la priorite » est structurellement possible, et la Story 5.17 ne sait le rompre qu'en escaladant vers une manoeuvre de degagement couteuse.

**Approach:** Decider la priorite **a l'authoring** (route prioritaire, priorite a droite, stop, plus des feux authorés) sur les noeuds de decision des jonctions, puis l'appliquer au runtime par un modele **pur et symetrique** : deux vehicules qui evaluent la meme situation en tirent le meme verdict, donc exactement un procede. Le controle de place sur la voie de sortie precede la priorite ; un arret interminable fait monter l'echelle de contournement des regles de 5.17 (aucun palier de retrait ni de teleportation), et un interblocage detecte est journalise en build de developpement avec les vehicules concernes.

Derive d'AC assumee : les **feux** ne figurent pas dans l'AC de l'Epic 5 (route prioritaire, priorite a droite, stop). Ajoutes sur decision humaine du 2026-09-20 ; l'AC de la Story 5.18 dans `epics.md` est complete dans la meme story.

## Boundaries & Constraints

**Always:** La priorite est lue a l'authoring, jamais recalculee par frame. L'arbitrage est une fonction **pure** dont le verdict est **invariant par echange des deux protagonistes** : c'est la garantie anti-interblocage, et elle se prouve en EditMode sans scene. La place sur la voie de sortie se controle **avant** la priorite. Les seuils de conduite (ecart accepte, maintien a l'arret, delai d'escalade) vivent dans `DriverProfileDef` ; les plans de feux dans `TrafficSettingsDef` — aucune constante de conduite dans le controleur. Le controleur reste host-only et n'emet que `VehicleDriveIntent`. La signalisation visuelle ne porte aucun etat de gameplay.

**Ask First:** Si le feu exige une NetworkVariable, une RPC ou un second objet d'etat reseau ; si les lampes ne sont pas separables sur le mesh Synty ; si l'arbitrage exige de lire chez un autre vehicule un etat non replique.

**Never:** Aucun palier de retrait, teleportation, reinsertion ou despawn ; aucun `NavMeshAgent` ; aucune priorite codée en dur par type de module ; aucune modification du trace du graphe (`successors`, `turnWeights`, positions de noeuds) ; aucune ecriture de vitesse ou de pose par le conducteur IA ; aucun collider sur une signalisation decorative.

## I/O & Edge-Case Matrix

| Scenario | Entree / etat | Sortie attendue | Gestion d'erreur |
|---|---|---|---|
| Deux approches concurrentes | A et B revendiquent la meme jonction | ordre total et symetrique : exactement un `Proceed`, l'autre `Wait` | revendications egales (meme distance, meme voie) : departage par `NetworkObjectId`, jamais d'egalite |
| Priorite a droite | jonction sans route prioritaire authoree | l'approche situee a droite de l'autre passe d'abord | sens indeterminable : retomber sur l'ordre total, jamais bloquer les deux |
| Stop | approche `Stop` | arret complet, maintien authore, puis franchissement si ecart accepte | ecart jamais accepte : `Wait`, puis escalade 5.17 |
| Voie de sortie saturee | file arretee au-dela de la jonction | `Wait` **avant toute** consideration de priorite | place indeterminee : traiter comme saturee, jamais comme libre |
| Feu | phase courante n'autorise pas l'approche | `Wait` ; l'approche autorisee franchit sans arret | plan absent ou `junctionId` inconnu : aucune regle de feu, comportement authore precedent |
| Interblocage | tous les revendiquants en `Wait` au-dela du delai authore | palier `IntersectionBreach` (entrer malgre la regle de degagement) puis echelle 5.17 ; journal dev avec les `NetworkObjectId` | aucun palier possible : `Wait` avec journalisation bornee, jamais de retrait |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/LaneNode.cs` -- 338 l., 4 champs serialises (`role`, `successors`, `turnWeights`, `exitReusesEntry`). Porte aujourd'hui **aucune** notion de priorite : c'est ici qu'arrive l'authoring de l'approche, et le fichier a un precedent de convention de connecteur a respecter (doc XML, `IsOutgoingConnector`).
- `Assets/RoadRage/Features/Vehicles/LaneGraph.cs` -- surface runtime unique du graphe (`GetSuccessors`, `GetTurnWeights`, `GetNodePosition/Rotation`, `IsExitPortal`, `NearestExitNodeIndex`, `TryGetRoadPosition`, `TrafficSettings`). `EnsureBuilt` (l.283) est le seul point de collecte : c'est la ou indexer les approches de jonction, une fois, sans balayage par tick.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- `FixedUpdate` (l.307) : ordre physique → `TickLocalTraffic` (l.330, pris en charge = `return`) → arrivee au waypoint → `ResolveNextNode` (l.391, tirage pondere). **Le point d'insertion des regles est le declencheur d'avancee** (l.328-337) : tant que la regle dit `Wait`, aucun `ResolveNextNode` n'a lieu. `RecoverAtWaypoint` (l.944) reste reserve aux gardes physiques.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs` -- perception cadencee de 5.17 (`RefreshPerception` l.65, buffer `NonAlloc` de 64, tri borne), `UpdateFrontalConflict` (l.162) = **le motif a reprendre** pour l'arbitrage symetrique (`TrafficPerception.MustYield`, verdict enonce des deux cotes puis verrouille jusqu'a separation) ; `TickLocalTraffic` (l.186) porte l'echelle `FollowRoute → Replan → Reverse → RoadDetour → SidewalkDetour` (`maneuverActive`, `blockedElapsedSeconds`, `profile.HornDelay`, `retryAfter`).
- `Assets/RoadRage/Features/Vehicles/TrafficPerception.cs` -- pur, reutilisable : `IsInArc`, `TrySelectLeader`, `PredictOrientedCollision`, `MustYield` (ordre total sur `(correct, distance de voie, id)`), `ClosestPointOnSegment`. C'est l'outil de la place sur la voie de sortie, pas un second scan.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs` -- `WaypointIndex` replique (Everyone / Server) : **c'est la source lue a distance** pour savoir sur quelle approche se trouve un autre vehicule, sans nouveau champ reseau.
- `Assets/RoadRage/Features/Vehicles/DriverProfile.cs` + `DriverProfileDef.cs` + `ScriptableObjects/Vehicles/DriverProfileDef_Default.asset` -- `DriverProfile` est une struct a constructeur positionnel long (5.17 a ajoute ses 17 champs en parametres par defaut) ; les seuils d'intersection suivent le meme chemin, sans casser les 11+ appelants des fixtures anterieures.
- `Assets/RoadRage/Features/Vehicles/TrafficSettingsDef.cs` -- racine d'authoring du trafic, deja referencee par `LaneGraph` (champ `trafficSettings`), avec `TryValidate` (l.148) et `OnValidate` (l.236) : c'est le porteur des plans de feux, pas un nouveau catalogue.
- `Assets/RoadRage/Prefabs/Greybox_Intersection.prefab` (2775 l.) et `Greybox_TJunction.prefab` (2323 l.) -- 12 et 9 `LaneNode` ; les noeuds de decision sont `Junction_From{North,South,East,West}` (successeurs `Connector_*_Out`, poids 30/50/20) et `Junction_From{West,South,East}` (poids 40/60, 50/50, 60/40). Enfants existants sous `Visual_*` : `Road_*`, `Crossing_*` (meshes Synty par GUID, **sans collider**), `Sidewalk_*` — le motif a suivre pour la signalisation.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` -- gardes de scene a ne pas casser : `TheMapCentreCarriesAFourBranchCrossroadsWithAuthoredTurnRatios` (l.404 : 4 noeuds a 3 successeurs, exactement 1 tout droit / 1 gauche / 1 droite, tout droit de poids maximal), `RoundaboutsAreGeometryOnlyWithAuthoredExitRatios` (l.320), `NoModuleColliderRisesAboveTheDrivingPlane` (l.881), `MvpRunDistrictIsAConnectedPortalToPortalLaneGraph` (l.270). Helpers `WithMvpRun` (l.1696) et `ResolveGraph` (l.1687).
- `Assets/RoadRage/Tests/EditMode/Story514AiDrivesByIntentTests.cs`, `Story517WiderPerceptionAndProgressiveUnblockingTests.cs`, `RoadRageScaffoldTests.cs` -- contrats a garder verts **sans affaiblissement** : aucun `RecoverAtWaypoint` sur un blocage percu, aucun retrait hors portail, aucun `NetworkVariable` en ecriture non-serveur.
- `AgentScripts/` -- motif d'authoring hors `Assets/` (donc sans import ni rechargement de domaine) : ouverture additive de scene (`Story513RecipeRelief.cs:45`), `SerializedObject` + `ApplyModifiedPropertiesWithoutUndo` (`Story516LobbyTrafficRowsAuthoring.cs:83`), `Undo.RecordObject`, controle bloquant **avant** `EditorSceneManager.MarkSceneDirty` + `SaveScene` (`Story516LobbyTrafficRowsAuthoring.cs:110-117`).

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/JunctionRules.cs` -- ajouter le modele pur : enum d'approche, revendication, plan de feux (`ResolvePhase(plan, temps)`), `Resolve` d'arbitrage et detection d'interblocage -- la garantie anti-interblocage doit etre prouvable sans scene ni reseau.
- [x] `Assets/RoadRage/Features/Vehicles/LaneNode.cs` -- ajouter l'approche authoree (regle + `junctionId` stable) -- la priorite est une donnee lue a l'authoring, pas un calcul par frame.
- [x] `Assets/RoadRage/Features/Vehicles/LaneGraph.cs` -- indexer les approches et leur jonction a la construction (`EnsureBuilt`) et les exposer en lecture -- aucune recherche de hierarchie par tick ; signaler une jonction dont les approches se contredisent.
- [x] `Assets/RoadRage/Features/Vehicles/TrafficSettingsDef.cs` + `ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset` -- authorer les plans de feux (id de jonction, phases, durees, garde de vert minimal) et les valider dans `TryValidate` -- les feux sont de la donnee de monde, pas un reglage de conducteur.
- [x] `Assets/RoadRage/Features/Vehicles/DriverProfile.cs` + `DriverProfileDef.cs` + `DriverProfileDef_Default.asset` -- authorer ecart accepte, maintien a l'arret et delai d'escalade -- aucun seuil de conduite code en dur, aucun appelant anterieur casse.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs` + `.cs` -- construire la revendication a l'approche, la confronter aux autres (`WaypointIndex` replique + perception), faire passer le controle de place avant la priorite, tenir l'arret, et inserer le palier `IntersectionBreach` en tete de l'echelle 5.17 -- le declencheur d'avancee de noeud reste le seul point de blocage.
- [x] `Assets/RoadRage/Features/Vehicles/TrafficSignalLampView.cs` -- presentation seule : teinter les lampes selon la phase resolue -- les deux pairs calculent la meme phase, donc aucun etat reseau ajoute.
- [x] `AgentScripts/Story518SignalAuthoring.cs` -- poser sur les prefabs de module la signalisation Synty (`SM_Prop_TrafficLight_01`, `SM_Prop_Sign_Stop_01`) sous `Visual_*` et renseigner les approches -- les meshes Synty sont regeneres a chaque reimport : aucune edition manuelle de prefab.
- [x] `Assets/RoadRage/Tests/EditMode/Story518IntersectionRulesAndDeadlockPreventionTests.cs` -- couvrir la matrice, l'invariance par echange des protagonistes, et l'authoring des prefabs -- preuve du modele et de la donnee.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story518IntersectionRulesAndDeadlockPreventionPlayModeTests.cs` -- dans `MVP_Run`, deux IA a la meme jonction : les deux s'arretent, exactement une franchit, aucune ne reste figee -- preuve runtime. **Fichier ecrit, verdict NON OBTENU** : la voie synchrone est refusee par le CLI, la voie asynchrone reste `running` indefiniment (`StartHost()` -> `ArgumentException: Invalid Socket` sur le transport Facepunch). Aucun resultat n'est un succes (AD-8).
- [x] `_bmad-output/planning-artifacts/epics.md` -- completer l'AC de la Story 5.18 (feux authorés) avec la date et le motif de la derive -- BMAD reste la source de verite.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- passer `5-18-intersection-rules-and-deadlock-prevention` a `in-progress` -- suivi de sprint.
- [x] `graphify update .` -- regenerer le graphe apres les changements source, **apres verification du perimetre** (`graphify-out/manifest.json` est cumulatif : un chemin hors `Assets/RoadRage/` deja present doit etre elague avant, sinon le graphe gonfle). Fait : `AgentScripts/` ajoute a `.graphifyignore`, 8 entrees hors perimetre elaguees, rebuild a 3 697 noeuds / 8 871 liens / 153 communautes, god-nodes tous RoadRage.

**Acceptance Criteria:**
- Given des intersections authorées, when la priorite est lue, then chaque approche porte sa regle (route prioritaire, priorite a droite, stop, feu) decidee a l'authoring, et aucune regle n'est recalculee par frame.
- Given deux vehicules qui revendiquent la meme jonction, when chacun evalue la situation, then leurs deux verdicts sont symetriques et un seul obtient `Proceed` : aucun etat ou les deux attendent l'autre, quelle que soit l'approche.
- Given une approche qui n'a pas de place sur sa voie de sortie, when elle evalue l'entree, then ce controle precede l'application de la priorite et le vehicule attend.
- Given une approche `Stop`, when le vehicule l'aborde, then il s'arrete completement, y demeure le temps authore, puis ne franchit qu'avec un ecart accepte.
- Given un feu authoré, when la phase courante n'autorise pas l'approche, then elle attend ; l'approche autorisee franchit sans arret, et le plan absent laisse le comportement authore precedent.
- Given un blocage qui persiste malgre les regles, when le delai authore est depasse, then les paliers authorés et actifs s'appliquent dans l'ordre (entrer dans l'intersection normalement tenue libre, puis les paliers 5.17), aucun palier de retrait ou de teleportation n'existe, et l'interblocage detecte est journalise en build de developpement avec les vehicules concernes.
- Given les prefabs de carrefour et de jonction en T, when ils sont relus, then la signalisation Synty y est un enfant visuel sans collider, et les gardes de scene de la Story 5.10 restent vertes sans modification.

### Review Findings

- [x] [Review][Patch] Brancher le suivi de trajectoire continue et la vitesse limite de courbe dans le controleur runtime ; les helpers actuels sont uniquement appeles par les tests [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs:393`] -- **Fait.** La visee vient de `ResolvePathLookAheadPoint` sur la trajectoire de conduite, bornee par la moitie du rayon de la voie (`ResolveBoundedLookAhead`), et la vitesse desiree est plafonnee deux fois : par la courbure du TRACE (`ResolvePathCurveSpeedLimit`) et par la courbure REELLEMENT COMMANDEE par la poursuite (`ResolveCommandedCurveSpeedLimit`). Le cas particulier `junctionTurnExitNode` de la conduite a disparu : un virage est une arete comme une autre.
- [x] [Review][Patch] Generer une meme trajectoire echantillonnee pour le suivi, la perception et les connecteurs de jonction au lieu de cordes de noeuds divergentes [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs:493`] -- **Fait.** `LaneGraphRouting.SampleLaneEdge` est la brique unique ; `BuildPathFromLaneGraph` l'utilise pour la conduite comme pour la perception, et `TryEmitJunctionMovement` emet le mouvement de jonction depuis la FRONTIERE indexee sur le graphe, c'est-a-dire le mouvement que `CommitJunctionTraversal` revendique deja.
- [ ] [Review][Patch] Representer un corridor de voie mesurable et verifier le gabarit du vehicule, pas seulement des points centraux dans un triangle [`Assets/RoadRage/Features/Vehicles/LaneGraph.cs:319`]
- [x] [Review][Patch] Evaluer les conflits de jonction sur les mouvements courbes et leur largeur de securite, pas uniquement sur deux cordes sans epaisseur [`Assets/RoadRage/Features/Vehicles/JunctionRules.cs:397`] -- **Fait.** `LaneGraph.ResolveMovementMeeting` echantillonne desormais la Bezier REELLEMENT parcourue des deux mouvements et rend la premiere abscisse ou leurs COULOIRS se touchent, au degagement authore `TrafficSettingsDef.JunctionMovementClearance` (2,36 m = 2 x 1,03 + 0,30). Mesure du district : l'avant d'un vehicule arrete a sa ligne mordait de **2,34 m** dans le couloir de virage, **0,00 m** apres correction, sur les 16 approches. Voir ANO-5.18-09.
- [x] [Review][Patch] Ne jamais relacher une cession de priorite sur un simple delai tant que le conflit demeure actif [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs:1028`] -- **Fait.** `TickYieldRelease` ne relache plus sur `JunctionEscalationDelay` : le seul motif est un CYCLE d'attente (`TryBreakWaitCycle`), rompu par le meme ordre total que le reste de l'arbitrage (plus petit `NetworkObjectId` du cycle). Hors cycle l'attente a un debouche et court librement, ce qui distingue une saturation reelle d'un interblocage.
- [ ] [Review][Patch] Dedoublonner les revendiquants et garantir un ensemble symetrique malgre des profils de conducteur differents [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs:1746`] -- **Moitie faite.** Le dedoublonnage par `NetworkObjectId` est en place (un vehicule porte plusieurs colliders et entrait donc plusieurs fois dans l'ensemble arbitre). RESTE : le rayon d'approche vient du profil de l'OBSERVATEUR (`profile.JunctionApproachRadius`), donc deux conducteurs aux profils differents peuvent construire des ensembles differents. Le rayon doit devenir une donnee de MONDE (jonction / `TrafficSettingsDef`), pas de conducteur.
- [ ] [Review][Patch] Garantir que la sortie arbitree est exactement celle empruntee et que son occupation est projetee sur l'axe authore [`Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.Traffic.cs:1645`]
- [x] [Review][Patch] Transformer les mesures runtime de virage, courbe et giratoire en assertions sur trajectoire observee, erreur laterale et gabarit [`Assets/RoadRage/Tests/PlayMode/Story518RuntimeEvidencePlayModeTests.cs:162`] -- **Fait, et deplace en EditMode.** `Story518LaneContainmentTests` simule la loi de guidage de production (meme visee, meme loi de braquage, memes assets) sur la geometrie REELLE de `MVP_Run` et assert l'erreur laterale contre un budget MESURE (0,97 m = demi-chaussee 2,00 m moins demi-gabarit 1,03 m), sur les 24 mouvements tournants et les 4 anneaux. Le banc tourne en 0,8 s et ne depend pas du Play Mode, dont le lanceur ne supporte qu'une execution par session d'editeur.

## Spec Change Log

### 2026-09-21 -- recette ANO-5.18-10 (un seul modele de recuperation)

**Quatre symptomes, deux causes.** La recette rapportait une cession inexplicable, des vehicules
arretes trop avant dans le carrefour, un vehicule tournant prisonnier et un enchevetrement apres
contact. L'analyse ne trouve pas quatre defauts : elle en trouve deux, et le second est structurel.

**Cause structurelle : l'echelle de recuperation avait un seul point d'entree, et ce n'etait pas
le bon.** Elle n'etait atteignable que par le predicat de FILE (`blocked`), qui exige un leader
percu, arrete, dans notre couloir. Quatre horloges distinctes coexistaient -- file bloquee
(`blockedElapsedSeconds`), attente de jonction (`junctionWaitSeconds`), cession
(`yieldElapsedSeconds`), file immobile (`queueElapsedSeconds`) -- avec trois echappatoires
differentes, et le face-a-face n'en avait **aucune**. Les trois blocages rapportes n'entraient donc
jamais dans l'echelle : ils restaient arretes indefiniment, chacun sous un nom different mais avec
le meme resultat.

**Correction.** Un seul declencheur : l'ABSENCE DE PROGRES le long de la route
(`TickRouteProgress`). La distance au waypoint qui diminue est le seul signe qui ne se laisse pas
tromper -- une roue qui patine, un vehicule qui pivote sur place ou qui recule en boucle ont tous
une vitesse non nulle sans avancer d'un metre. Un seul escalier, `JunctionRules.ResolveRecoveryStage`,
pur et mesurable, aux seuils DEJA AUTHORES (klaxon 2 s, puis un delai d'escalade de 12 s par palier ;
aucune constante de temps nouvelle) :

| palier | seuil | action |
|---|---|---|
| 0 | < 2 s | conduite nominale, regles ordinaires |
| 1 | 2 s | replanification puis contournement sur la chaussee |
| 2 | 14 s | recul pour rouvrir un espace de manoeuvre |
| 3 | 26 s | manoeuvre elargie, espace voisin et trottoir |
| 4 | 38 s | dernier recours : un vehicule IA **a l'arret** cesse d'etre infranchissable |

Chaque palier reste valide par `ValidatePath` : aucun n'invente de place qui n'existe pas. Au
dernier recours seul le franchissement d'un vehicule IA immobile est tolere -- le decor, les
pietons et les joueurs restent infranchissables a TOUS les paliers (`IsStalledPeerVehicle`).

**Face-a-face.** Le vehicule qui manoeuvre est celui que la conformite de voie designe comme MAL
PLACE (sens de circulation lu sur le graphe, puis distance a l'axe, puis identifiant) :
`MustYieldHeadOn`. Le vehicule correctement place ne quitte sa voie qu'au palier 3, quand l'autre a
manifestement echoue a se degager.

**Feu rouge.** Une attente BORNEE ne fait pas tourner l'horloge : le plan de feux garantit sa fin.
L'etat (`heldByBoundedWait`) remonte la file d'un cran par pas, comme `IsLegitimateWait`, sinon
seule la tete serait protegee. Sans cette garde, un vehicule correctement arrete a un feu partirait
en contournement au bout du delai de klaxon.

Preuves : `Story518UnifiedRecoveryTests` -- monotonie et bornes de l'escalier, authoring degenere,
entree par le palier et non par le predicat de file, politesse du face-a-face, gel de l'horloge sur
attente bornee, et limites du dernier recours.


### 2026-09-21 -- recette ANO-5.18-11 (une cession qui ne pouvait plus se defaire)

**Symptome.** "Un vehicule entre en cession du point de conflit sans raison lisible" :
`YieldTrajectory ... pair=AI_Vehicle_Portal_040 arrivee=0,00s vs +Infini`, le pair voisin etant
`FollowingSameLane (file : leader arrete) voie=53/54 ... attente=30,7s`.

**Cause.** Les noeuds 53 et 54 sont sur un anneau de giratoire. La priorite a l'anneau
(ANO-5.18-06) etait evaluee AVANT tout le reste, donc elle accordait la priorite a un vehicule
**immobilise** sur l'anneau -- pour toujours. L'ordre d'arrivee, lui, traitait deja correctement
une arrivee infinie (`YieldsAtConflict` la ramene au maximum) : c'est la regle d'anneau qui le
court-circuitait. Un leader arrete influencait donc bien la decision, comme la recette le soupconnait.

**Correction.** Une arrivee infinie ne dit pas "il passe en premier", elle dit "il est a l'arret".
Un pair qui n'arrivera jamais ne peut pas etre prioritaire : la finitude des deux arrivees est
constatee AVANT la priorite a l'anneau, et la regle reste antisymetrique (celui qui roule passe,
celui qui est arrete cede). Ne plus ceder ne fait pas rouler dedans -- le suivi de file, la ligne
de cession (ANO-5.18-08) et le freinage d'urgence restent en dessous. Cela transforme une attente
SANS ISSUE en un blocage, c'est-a-dire en quelque chose dont l'echelle de recuperation sait sortir.

**Diagnostic.** Chaque verdict pose desormais son MOTIF (`conflictVerdictReason`), visible dans la
trace : "pair a l'arret : il n'atteindra jamais le point", "anneau N : nous entrons", "ordre
d'arrivee", "arrivees egales : plus petit identifiant". Un verdict sans motif ne se verifie pas en
recette -- c'est precisement ce qui rendait cette cession inexplicable. La trace complete porte
aussi le palier de recuperation et la duree sans progres.

Preuves : `Story518UnifiedRecoveryTests`.


### 2026-09-21 -- recette ANO-5.18-09 (la ligne d'arret traitait les vehicules comme des points)

**Symptome.** "Les vehicules attendent trop pres de l'intersection, voire partiellement dedans",
et le vehicule qui tourne s'en trouve prisonnier.

**Cause, mesuree.** `JunctionApproachInfo.ConflictEntryDistance` etait l'abscisse de la premiere
INTERSECTION STRICTE de deux axes centraux -- deux polylignes **sans epaisseur**. Un vehicule
arrete a la ligne qui en decoule a pourtant 2,06 m de large. Sonde `Story518StopLineProbe` sur les
16 approches du district, avant correction :

| approche | frontiere axes | ligne | frontiere couloirs | intrusion de l'avant |
|---|---|---|---|---|
| intersection (x4, feux) | 6,00 m | 4,67 m | 3,66 m | **2,34 m** |
| T-junction, branches Stop (x4) | 6,00 m | 4,67 m | 3,66 m | **2,34 m** |
| T-junction, route prioritaire (x4) | 6,00 m | 4,67 m | 4,35 m | 1,65 m |
| T-junction, route prioritaire (x4) | 6,00 m | 4,67 m | 6,40 m | -0,40 m |

**Correction.** La frontiere est desormais la premiere abscisse ou les deux COULOIRS se touchent :
les deux mouvements sont echantillonnes sur la Bezier reellement parcourue (41 points, ~0,25 m de
resolution) et compares au degagement authore `TrafficSettingsDef.JunctionMovementClearance`
(2,36 m = deux demi-largeurs de 1,03 m plus la marge de 0,30 m). La frontiere est en outre bornee
par le noeud de decision : une ligne posee au-dela ferait attendre le vehicule DANS le carrefour
qu'il doit justement tenir libre.

**Resultat mesure : intrusion maximale 2,34 m -> 0,00 m sur les 16 approches.** L'avant d'un
vehicule arrete reste hors du couloir de virage, ce qui rend au trafic tournant la place que la
recette lui reclamait. Aucun decalage global aveugle n'est introduit : la valeur se deduit de la
geometrie de la jonction et du gabarit authore, et elle varie d'une approche a l'autre (2,33 m,
3,02 m et 5,07 m selon la branche).

Note : la valeur decouverte au passage est que l'ancienne regle ne trouvait aucun croisement
exploitable sur la plupart des approches et retombait systematiquement sur le noeud de decision.

Preuves : `Story518UnifiedRecoveryTests` -- intrusion nulle sur les 16 approches, aucune ligne
posee au-dela de son noeud de decision, et au moins huit approches dont la frontiere recule
reellement (garde de non-regression du correctif lui-meme). Ferme la Review Finding #4.


### 2026-09-21 -- recette ANO-5.18-08 (une cession n'avait pas de ligne)

**Symptome rapporte.** "Les voitures cedent leur point de conflit trop tot au milieu des troncons
de route et pas juste devant l'intersection." La proposition qui accompagnait le rapport --
reduire le champ de perception -- a ete MESUREE puis ECARTEE, parce qu'elle aggrave le defaut.

**Cause.** Une cession de trajectoire posait `ResolveStopIntent`, c'est-a-dire le frein a fond, A
LA POSITION COURANTE, des que le conflit entrait dans l'horizon de prediction. Avec le profil
authore (perception 20 m, prediction 3,0 s, vitesse desiree 8,00 m/s), cela veut dire un arret
complet jusqu'a une vingtaine de metres en amont du point de conflit, n'importe ou sur le troncon.
Les jonctions AUTHOREES ne connaissaient pas ce defaut : `TickJunctionRules` pose une ligne d'arret
et laisse l'IDM freiner dessus. Les quatre giratoires du district ne portent AUCUNE approche
authoree (ANO-5.18-06), donc aucune ligne -- ce qui explique qu'ils concentraient le symptome.

**Correction.** Le point de rencontre des deux couloirs, deja calcule par
`TrafficPerception.FindPathConflict`, est retenu et devient la ligne de cession, reculee de la
demi-largeur du vehicule et de la marge authoree : exactement la construction de la ligne d'arret
d'une approche authoree, appliquee a une geometrie que le graphe ne decrit pas. Tant que la ligne
est devant, le ralentissement passe par le meme canal que la jonction -- un leader immobile
virtuel a cette distance, `PlannedStopGap` -- donc par l'IDM, donc progressif ; le vehicule garde
aussi sa direction pendant l'approche au lieu de braquer a zero comme le faisait l'arret ferme.
L'arret ferme redevient le comportement des que la ligne est atteinte. Un vehicule qui cede ne
s'engage jamais dans une jonction (`yieldingTrajectory`), sans quoi le waypoint avancerait dans
l'aire de conflit pendant qu'il freine encore.

**Mesure du freinage** (profil authore, 8,00 m/s, `Story518YieldBrakingProbe`) :

| conflit vu a | decelaration de pointe | a-coup | arret |
|---|---|---|---|
| 25 m | -1,93 m/s2 (confort authore : 2,00) | 7,5 m/s3 | 0,82 m avant la ligne |
| 15 m | -4,74 m/s2 | 21,0 m/s3 | 0,82 m avant la ligne |
| 8 m | -12,15 m/s2 | 64,5 m/s3 | 0,82 m avant la ligne |

Ancien comportement, dans les trois cas : frein a fond immediat, arret sur place, soit 25 / 15 /
8 m AVANT le point de conflit.

**Pourquoi la perception n'est pas reduite.** Le tableau est la refutation : freiner au confort
authore depuis 8,00 m/s demande environ 25 m de course. La portee de perception vaut deja 20 m.
La reduire raccourcirait la course disponible et FORCERAIT le freinage dur -- l'inverse de la
demande. Ce qui manquait n'etait pas une portee plus courte, c'etait la cible d'arret.

Preuves : `Story518YieldStopLineTests` -- position de la ligne, decroissance monotone de l'ecart en
approche, bascule continue vers l'arret ferme, refus d'un point deja depasse, sens conservateur de
la projection sur la corde en courbe, et cablage de la contrainte combinee `PlannedStopGap`.


### 2026-09-21 -- recette ANO-5.18-07 (face-a-face contre un vehicule a l'arret)

**Symptome rapporte.** Une voiture arrivee pendant la manoeuvre de virage d'un prioritaire s'arrete
a son stop EN TRAVERS de sa trajectoire. Le prioritaire se declare en face-a-face et freine ; le
second ne peut plus avancer, sa permission de jonction etant refusee. Plus rien ne bouge. Demande :
que le prioritaire contourne legerement.

**Cause -- deux verrous, et il fallait les deux.**

1. Le predicat de FILE lisait l'attente reguliere de l'autre (`JunctionYield`) comme une file
   legitime. Or une file se resout quand sa tete AVANCE ; celle-la, en avancant, entre dans nous.
   ANO-5.18-03 avait corrige le NOM de la decision, pas ce predicat.
2. La garde de face-a-face excluait l'echelle de deblocage pour celui qui ne cede pas. C'est juste
   tant que l'autre ROULE -- deux vehicules qui se rapprochent ne se contournent pas, ils
   s'arretent -- et faux des qu'il est a l'arret : ce n'est plus un face-a-face, c'est un obstacle.

**Correction.** `queueing` exclut desormais le face-a-face, et la garde s'ouvre quand le pair est
immobile (`headOnStationary`, au seuil d'immobilite deja authore). L'echelle de la Story 5.17
reprend alors la main et `ValidatePath` continue de refuser une echappatoire sans place sure :
l'arret reste le repli, aucun contournement n'est invente sur du vide.

Preuves : `Story518StalledHeadOnTests`.


### 2026-09-21 -- recette ANO-5.18-09 (troncon entre les tunnels et les giratoires : mesure)

**Proposition evaluee, puis ecartee sur mesure.** "Creer un troncon de route entre le rond-point et
les tunnels, afin de laisser les voitures qui spawn prendre les decisions."

Mesure (`Story518PortalRunwayProbe`, `Story518PortalChainProbe`) :

- le troncon EXISTE deja : 12,00 m d'une seule arete droite entre le portail et le noeud d'entree,
  puis 2,26 m jusqu'au premier noeud d'anneau, soit 14,26 m au total, pour les quatre portails ;
- il ne peut pas etre allonge vers l'exterieur : l'emprise du graphe est x et z dans
  [-47,6 ; 47,6] et les quatre portails sont poses EXACTEMENT sur ce bord -- (44,7;47,6),
  (47,6;-44,7), (-44,7;-47,6), (-47,6;44,7). L'allonger demanderait d'agrandir le district ;
- les quatre anneaux ont 11 noeuds, un rayon de 6,00 m, centres a (+-31,9 ; +-31,9).

Le troncon est-il SUFFISANT ? Un vehicule apparait a 0 m/s et accelere a 1,50 m/s2 authores : sur
12 m il atteint 6,00 m/s, et s'arreter depuis 6,00 m/s au freinage de confort (2,00 m/s2) demande
9,0 m. La place est donc la. Ce qui manquait n'etait pas la route, c'etait la LIGNE a laquelle
s'arreter : les giratoires ne portent aucune approche authoree, donc la cession y freinait sur
place, parfois a la bouche du tunnel -- ce qui bloque aussi le portail pour l'apparition suivante.
ANO-5.18-08 la fournit.


### 2026-09-21 -- recette ANO-5.18-06 (priorite a l'anneau dans les giratoires)

**La regle n'existait pas.** La sonde le montre sans ambiguite : les quatre giratoires du district
portent ZERO regle d'approche, et leurs douze entrees non plus. La priorite a l'anneau etait
seulement ESPEREE de l'ordre d'arrivee au point de conflit -- le commentaire de
`UpdateTrajectoryConflict` l'affirmait : "un vehicule deja engage sur un anneau atteint le point de
conflit avant celui qui approche l'entree, donc il passe -- la priorite a l'anneau sans regle
dediee". Le pari est faux : un vehicule lance vers l'entree peut atteindre ce point AVANT celui qui
fait le tour, et l'ordre d'arrivee lui donne alors le passage. Un giratoire n'est pas une course.

Aggravant : le repli du reste du reseau est la PRIORITE A DROITE, et elle est inversee dans un
giratoire. En circulation a droite l'anneau tourne dans le sens antihoraire, donc celui qui circule
arrive par la GAUCHE de celui qui entre : appliquer le repli donnerait la priorite a l'entrant.

**Correction.** L'anneau est identifie par la seule TOPOLOGIE du graphe, une fois a la construction
(`LaneGraph.BuildRingIndex`) : un cycle ferme parcouru en prenant partout le successeur le mieux
aligne sur l'axe authore du noeud. Aucun nom, aucun prefab, aucune constante de scene -- un
giratoire plus grand ou a cinq branches se detecte de lui-meme. La regle est ensuite une fonction
pure et ANTISYMETRIQUE par construction, `JunctionRules.CompareRingPrecedence`, consultee AVANT
l'ordre d'arrivee dans `ResolveConflictYield`.

Etre ENGAGE sur l'anneau suppose deux pieds dessus -- le noeud quitte et le noeud vise. Le seul
noeud vise ne suffit pas : un vehicule qui vient d'atteindre le noeud d'entree vise deja l'anneau
alors qu'il est encore sur la ligne de cession, et se declarerait prioritaire au moment precis ou
il doit ceder.

Preuves : `Story518RoundaboutPriorityTests` -- antisymetrie sur toutes les paires d'anneaux, les
quatre giratoires reconnus sans authoring, et les douze entrees verifiees une a une comme cedant a
leur anneau, l'anneau se sachant prioritaire en retour.


### 2026-09-21 -- recette ANO-5.18-05 (giratoires : blocages residuels et lenteur)

Deux defauts rapportes en recette, deux causes distinctes, toutes deux mesurees.

**1. Blocage residuel en giratoire -- le cap d'un pair etait lu sur son NEZ.** Le releve montrait
`EmergencyBrake (face-a-face dans la voie), arrivee=1,11s vs +Infini`. L'arrivee infinie dit que le
pair est immobile ; or un vehicule a l'arret garde le nez ou son dernier virage l'a laisse. Dans un
anneau, ou la voie tourne en permanence, ce nez fige peut pointer a plus de 135 deg de notre
tangente, ce qui suffisait a declarer un FACE-A-FACE. Et un face-a-face ne s'arbitre pas -- les deux
vehicules s'arretent, definitivement -- la rupture de cycle d'attente ne s'y appliquant pas
puisqu'elle ne couvre que les cessions de priorite. Le cap d'un pair IA se lit desormais sur SA
VOIE (`ResolveNodeForward(peer.CurrentLaneNode)`), donnee authoree que les deux pairs lisent a
l'identique. Un vehicule reellement a contresens porte un noeud de voie oppose et reste detecte.

**2. Lenteur et a-coups en giratoire -- deux defauts de mesure cumules.**

- Le rayon de courbure etait lu par le cercle passant par TROIS POINTS VOISINS. La trajectoire
  echantillonne les courbes au metre et les droites tous les quatre metres : a chaque raccord les
  trois points etaient espaces de 4, 1 et 1 m, et le cercle qui les traverse annoncait un rayon tres
  serre sur une portion pourtant droite. Freinage sec, puis reprise au segment suivant. Le rayon se
  lit maintenant comme un VIRAGE -- angle cumule par unite de longueur d'arc
  (`ResolveRadiusOverArc`) -- ce qui est exact sur un arc et insensible a la densite
  d'echantillonnage. Sur l'anneau : 8,64 m lus avant correction, 5,82 m apres, pour 6,00 m reels.
- Un plafond de vitesse fonde sur la courbure COMMANDEE a ete ajoute puis RETIRE. Il gagnait 24 %
  d'ecart sur un virage isole, mais il prenait l'angle de visee TRANSITOIRE pour une demande de
  courbure permanente et fermait une boucle : plus lent -> visee plus courte -> angle plus grand ->
  plafond plus bas. Mesure : 1,00 m/s sur tout l'anneau pour une vitesse desiree de 8,0.

**3. La visee etait trop COURTE, et sa borne haute la raccourcissait encore.** Contre l'intuition,
la mesure est nette sur l'anneau : bridee a la moitie du rayon, l'ecart vaut 1,27 m ; libre, il
tombe a 0,22 m -- a vitesse pleine et sans a-coup dans les deux cas. Une visee courte ne suit pas
mieux une courbe, elle la poursuit en zigzag. La borne haute est supprimee, le plancher reste
l'empattement (sous lui une poursuite pure oscille), et `lookAheadSeconds` passe de 0,6 a 1,0 s sur
`Greybox_AIVehicle`.

**Etat mesure apres correction** -- anneau : ecart 0,22 m, vitesse 8,00 m/s constante, a-coup
0,0 m/s^2. Virage large : 0,79 m. Virage serre : 1,35 m, plancher PHYSIQUE documente et borne par
`TheTightestTurnsStayAtTheirMeasuredPhysicalFloor`. Trois pistes mesurees et ecartees pour le
descendre : conge circulaire a rayon constant (meme ecart), balayage de visee (0,2 a 1,2 s, sans
effet), reserve de braquage (sans effet en virage, immobilise les giratoires). Le descendre releve
de l'AUTHORING -- carrefour plus large ou vehicule plus court -- pas du code.



## Design Notes

L'invariance par echange est le coeur du mecanisme : chaque vehicule n'observe pas un « tour de passage » mais une fonction symetrique de l'etat partage, donc les deux parties enoncent le meme gagnant sans se parler. C'est le motif deja en place pour les conflits frontaux (`TrafficPerception.MustYield` + `UpdateFrontalConflict`, verrouille jusqu'a separation) ; l'intersection l'etend a n revendiquants en ajoutant la regle authorée comme premier critere, et l'ordre total `(distance, voie, NetworkObjectId)` comme dernier.

Le plan de feux est une **fonction du temps partage**, pas un etat replique : les deux pairs tirent la meme phase de la meme horloge d'une donnee identique, ce qui evite toute NetworkVariable et tout RPC.

`IntersectionBreach` s'insere **en tete** de l'echelle existante parce que c'est le palier le moins couteux et le plus lisible : il ne fait qu'assouplir une regle que le vehicule respectait, la ou `Reverse`, `RoadDetour` et `SidewalkDetour` engagent une manoeuvre.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story518IntersectionRulesAndDeadlockPreventionTests"` -- attendu : fixture verte, aucune garde 5.10/5.14/5.17 affaiblie.
- `.\scripts\validate.ps1 -TestMode EditMode` -- attendu : suite complete verte (le filtrage fonctionne en EditMode).
- `.\scripts\validate.ps1 -TestMode PlayMode` -- attendu : suite runtime complete incluant le banc 5.18 dans `MVP_Run`. **Le filtrage ne fonctionne pas en PlayMode** (mesure du 2026-09-18) : passer la suite entiere, sans filtre.

**Manual checks (if no CLI):**
- Dans `MVP_Run`, placer deux IA sur deux approches de la meme jonction et observer : les deux ralentissent, une seule franchit, l'autre repart derriere elle. Verifier aussi l'ordre visuel des lampes et la presence des panneaux stop sans collider.
