---
title: 'Story 5.32 -- Frontiere de perception : index d''occupation, observations de leader et de dangers'
type: 'feature'
created: '2026-10-01'
status: 'done'
baseline_commit: '5f5e776daf96b96baa6adb3f6e3c6c9377eefe10'
review_loop_iteration: 1
context:
  - '{project-root}/_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-30-traffic-v2-planning-and-runtime-spine-foundation.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La `TrafficFrame` 5.30 ne contient que des acteurs localises, et `AgentObservation` ne porte que la localisation de l'agent. Les stories 5.33 (suivi), 5.34 (sortie occupee), 5.37 et 5.40 n'ont donc aucun fait mesure sur le voisinage. Chacune serait tentee de refaire un balayage physique prive, comme le controleur V1.

**Approach:** Etendre la frame en une vue partagee, immuable et indexee : occupation structuree par element, index spatial 3D des acteurs et des dangers, signaux, fermetures, horizons d'intention publies. Ajouter une perception pure qui, pour un agent et son `PathHorizon`, produit des faits objectifs dates, bornes et qualifies. Aucun consommateur runtime en 5.32.

**Objectif :** avec plusieurs acteurs dans une frame commune, chaque agent obtient une perception correcte, deterministe, bornee et sans decision. Montrer qu'un trafic de vrais vehicules `Rigidbody` reste stable dans `MVP_Run` revient aux stories runtime, a partir de la 5.33.

## Boundaries & Constraints

**Always:**

- **Entrees de frame ajoutees**, toutes optionnelles : les appelants 5.30/5.31 compilent et se comportent sans changement.
  - Par acteur : horizon d'intention publie. `SourceFrameId` < `FrameId` ; intervalles (kind, id, s0, s1) finis, s0 ≤ s1, element connu du modele.
  - Dangers non structures : `RoadId`, genre (`WalkingPlayer`, `Pedestrian`, `Obstacle`, `Vehicle`), `RoadBoundsBox`, vitesse monde, confiance dans [0, 1]. `Vehicle` designe seulement un vehicule qui n'est pas un acteur de trafic (voiture joueur, V1).
  - Phase courante par plan de signal (plan et phase connus du modele).
  - Fermetures d'element (element connu, code de raison stable).
- **Validation.** Refus nomme (`ArgumentException`, message = code stable) pour : id vide ou duplique, valeur non finie, extent d'empreinte negative, id ou phase inconnus, horizon publie futur.
- **Identite unique.** Les `TrafficId` des acteurs et les `RoadId` des dangers partagent un seul espace d'identite. Un danger qui reprend l'id d'un acteur est refuse a la construction (`DuplicateIdentity`). Un vehicule ne peut donc apparaitre qu'une fois, comme acteur ou comme danger.
- **Acteur sans empreinte declaree.** Une empreinte est declaree si ses quatre extents sont > 0. Un acteur non declare (extents nulles du driver 5.31) reste dans la frame avec sa localisation : compatibilite 5.30/5.31 intacte. Il est exclu de l'occupation structuree et de l'index spatial. Il n'entre dans aucun fait qui exige une emprise : leader, suiveur, adjacence, obstacle, occupants de sortie. Il apparait seulement dans `UnmeasuredActors` (id, element, s de reference, sans aucune distance physique) quand il est localise sur un element que l'observation a cherche. Il garde sa place dans le recouvrement d'intentions, qui n'utilise que les horizons.
- **Occupation conservatrice.** L'intervalle [sMin, sMax] d'un acteur sur son element retenu couvre tout le rectangle de l'empreinte, pas seulement ses coins :
  - les quatre bords sont subdivises a un pas h ≤ `OccupancySampleStepMeters` (constante nommee de la frame) ; chaque echantillon est projete sur la courbe de l'element, avec le depassement aux bornes ;
  - on ajoute un reste explicite r = (h/2) / (1 − κ_max·d_max), ou κ_max = max |κ| de l'element et d_max = distance maximale d'un echantillon a la courbe + h/2 ;
  - s'applique si κ_max·d_max < 1 ; sinon l'acteur est exclu de l'occupation avec le diagnostic `OccupancyNotBounded`, jamais un intervalle sous-estime ;
  - les exclusions (`NotLocalized`, `UndeclaredFootprint`, `OccupancyNotBounded`) sont publiees par la frame, par acteur.
- **Index construits une seule fois**, a la construction de la frame :
  - occupation par element : acteurs localises, declares et bornes seulement, avec l'intervalle conservateur ci-dessus ;
  - index spatial 3D : tous les dangers, et les acteurs a empreinte declaree ;
  - etat de signal par mouvement ;
  - fermetures.
- **Immuabilite et ordre.** Collections copiees, triees de facon deterministe (s, puis `RoadId`), exposees en lecture seule. Aucun membre public modifiable. L'ordre des entrees est sans effet.
- **Requete spatiale bornee.** L'appelant fournit un tampon de capacite fixe. La requete n'alloue pas, ecrit les candidats dans un ordre deterministe, rapporte le total et `Saturated` si le total depasse la capacite. Jamais de troncature silencieuse.
- **Perception** (`Perception/`) : fonction pure de (frame, `TrafficId`, `PathHorizon`, `PerceptionLimits`, tampon). Elle produit :
  - **Leader** : premier occupant devant, sur les intervalles de l'horizon, dans l'ordre de la route. Jeu = distance d'arc entre le sMax conservateur de l'agent et le sMin conservateur du leader, negatif en cas de chevauchement. Plus la vitesse du leader.
  - **Suiveur** : occupant le plus proche derriere, sur l'element courant puis sur ses predecesseurs directs (corridors predecesseurs, mouvements entrants, corridor d'origine d'un mouvement), dans `RearRangeMeters`.
  - **Occupation adjacente** : par `LaneAdjacency` du corridor courant, occupants du voisin dans la fenetre [sMin − W, sMax + W] transportee lineairement. Cote et ecart longitudinal signe.
  - **Obstacles, joueurs et pietons** (index spatial) : dangers, et acteurs declares absents de l'occupation des elements de l'horizon, dont la boite rejoint le couloir balaye de l'horizon elargi de `LateralRangeMeters`. Faits : distance d'arc jusqu'a la face proche ; ecart lateral au flanc de l'agent (≤ 0 = dans le couloir balaye) ; ecart vertical dans le seuil d'acceptation de la localisation.
  - **`IntentPathOverlap`** : l'horizon de l'agent et l'horizon publie d'un autre acteur utilisent des portions de route qui peuvent se recouvrir dans l'espace, sur un meme element (s qui se recouvrent) ou sur deux mouvements d'une meme `ConflictZone`. C'est un fait spatial. Il ne predit ni collision, ni conflit temporel, ni presence future synchronisee : les horizons publies ne portent aucune fenetre temporelle. Jamais au-dela de l'horizon declare de chacun.
  - **Occupation de la sortie visee** : pour le prochain mouvement de l'horizon, longueur libre du corridor de sortie depuis son debut jusqu'a l'arriere du premier occupant, nombre d'occupants, premier occupant.
- **Metadonnees de chaque fait** (`ObservationMetadata`) : horodatage (frame de mesure, ou frame source pour un horizon publie) ; source (`StructuredOccupancy`, `SpatialQuery`, `PublishedHorizon`) ; portee effectivement cherchee ; confiance (min des confiances de localisation des deux acteurs, ou confiance declaree du danger).
- **Listes bornees** par `ListCapacity` : au-dela, les plus proches (distance, puis `RoadId`) sont gardes et le canal porte total et `Saturated`.
- **Statut par canal.** Leader, suiveur, adjacence et obstacles exigent l'empreinte et l'occupation de l'agent. Si elles manquent, le canal porte un statut explicite (`UndeclaredFootprint`, `AgentOccupancyUnavailable`) et aucun fait. La sortie visee et `IntentPathOverlap` ne dependent que de l'horizon et restent evalues. Jamais d'exception, jamais une liste vide qui se ferait passer pour « rien percu ».
- `AgentObservation` garde ses champs et son constructeur 5.30. `Perceived` distingue l'observation minimale d'une observation percue vide. `ToText()` deterministe, invariant de culture.

**Ask First:**

- Brancher la perception dans `PlanningSpine`, `TrafficV2VehicleDriver` ou `TrafficDecisionProjection`.
- Modifier un type ou une signature publique 5.25–5.31 autrement que par ajout optionnel.
- Modifier le modele `MVP_Run`, sa preuve, `Traffic/Migration/**`, `RoadLocalizer` ou `scripts/validation-profiles.ps1`.
- Ajouter un asmdef, une `NetworkVariable`, une RPC ou un acces physique Unity.

**Never:**

- Un fait qui decide : priorite, droit de passage, leader *a suivre*, exception de regle, pedale, contrainte de vitesse.
- Une reaction multi-vehicule : suivre, ceder, freiner, contourner, donner la priorite ou changer de voie. Elle appartient a la 5.33 et aux stories tactiques suivantes.
- Une distance, une occupation ou un jeu calcule avec une empreinte nulle ; un intervalle d'occupation tire des seuls coins.
- Dans `Frame/` ou `Perception/` : `Physics.*`, `Rigidbody`, `MonoBehaviour`, `Transform`, Netcode, `VehicleDriveIntent`.
- Balayage de sphere ou rayon dans l'axe avant ; jeu centre a centre ou depuis l'origine de reference seule.
- Prediction au-dela de l'horizon declare ; evaluation de changement de voie.
- Copie profonde de la frame par vehicule ; etat conserve entre deux appels de perception.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Leader en ligne droite | Deux acteurs sur un corridor, empreintes declarees | Jeu = (s_L − arriere_L) − (s_A + avant_A), different du centre a centre | N/A |
| Leader apres un virage | Leader sur l'element suivant, hors de l'axe avant de l'agent d'au moins la somme des demi-largeurs | Trouve le long de l'horizon, jeu d'arc pare-chocs a pare-chocs | N/A |
| Aucun leader | Aucun occupant devant dans l'horizon | Pas de leader ; portee = longueur de l'horizon | N/A |
| Joueur a pied | `WalkingPlayer` dans le couloir balaye ; un autre a cote de la chaussee | Les deux rapportes ; ecart lateral ≤ 0 pour le premier seulement | N/A |
| Vehicule non localise | Acteur declare non localise, pose sur le chemin | Absent de l'occupation, present une seule fois comme obstacle | N/A |
| Courbe serree | Empreinte sur un mouvement `MVP_Run` de rayon minimal proche de R_adm | [sMin, sMax] contient la projection d'une grille dense de l'interieur du rectangle | N/A |
| Courbure non bornee | κ_max·d_max ≥ 1 | Acteur hors occupation, diagnostic | `OccupancyNotBounded` |
| Acteur sans empreinte | Autre acteur a extents nulles, localise devant l'agent | Present dans la frame, localise ; ni leader ni occupant ; liste dans `UnmeasuredActors` sans distance | N/A |
| Agent sans empreinte | Extents nulles pour l'agent observe | Canaux a emprise sans faits ; sortie et recouvrement evalues | `UndeclaredFootprint` |
| Identite dupliquee | Danger avec le `RoadId` d'un acteur | Aucune frame | `DuplicateIdentity` |
| Saturation | Plus de candidats que la capacite | Plus proches gardes ; total et `Saturated` | Diagnostic |
| Ordre | Entrees permutees | Frame et `ToText()` identiques | N/A |
| Entree invalide | Horizon futur, id inconnu, doublon, NaN | Aucune frame | Code nomme |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs` -- frame 5.30 a etendre : tri par `RoadId`, localisation unique a la construction, `TryGetActor` par dichotomie. Appelants `Lifecycle/TrafficV2Composition.cs:307` et `Lifecycle/TrafficV2VehicleDriver.cs:340` (pose sans `Footprint`, extents nulles) : doivent compiler inchanges.
- `Assets/RoadRage/Features/Vehicles/Traffic/Perception/AgentObservation.cs` -- struct minimale 5.30, construite par `PlanningSpine.cs:89`. A etendre sans casser ce constructeur.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs:13-31,86-118` -- `VehicleFootprint` (repere chassis : x droite, z avant ; extents depuis l'origine de reference), `VehicleFootprintPose`, `RoadLocation` (`Localized`, `ElementId`, `SMeters`, `Confidence`, drapeaux). `RoadLocalizer.Localize` reste l'unique localisation.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadCurve.cs:39-56,123,132` -- `Project(point, sMin, sMax)` : s borne + `LongitudinalOverrunMeters` non signe (borne de depart ou de fin selon s). Base de la projection des coins et des dangers.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/PathHorizon.cs:36-52,80-88` -- `PathInterval` (kind, id, s0, s1, distances de depart et de fin, points) : axe d'arc de la perception. L'horizon part de la progression de l'agent.
- `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs:432-470,510,560,570,580-590` -- `.Curve` des corridors et mouvements, `Adjacencies`, `ConflictZones` (`MemberMovementIds`), `SignalPlans`, `GetSuccessorCorridors` / `GetPredecessorCorridors`, `TryGetMovement` (`FromCorridorId`, `ToCorridorId`).
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs:236,320,422-436,520-545,686-706` -- `LaneSide`, `RoadBoundsBox` (reutilise pour les dangers), `LaneAdjacency` (fenetres From/To), `SignalGroupState` / `SignalState`, `RoadLocalizationProfile.AcceptanceDistanceMeters` (seuil vertical « autre etage »).
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs:44-50` -- `RadiusMeters` : rayon d'admission R_adm (4,03 m sur `MVP_Run`, entrees et sorties d'anneau saturees a ce rayon, 5.31). Source du cas de courbe serree.
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- modele de `ToText()` deterministe et invariant de culture.
- `Assets/RoadRage/Tests/EditMode/Story530PlanningSpineTests.cs:28-41,245-264,812-860` -- chargement du modele historique signe, `Pose(curve, s)`, `EntryFrame`, `Mutate(modelText, ...)`. Son scan de `Frame/` et `Perception/` doit rester vert : aucun membre nomme `waypoint` ou `target…index`.
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json` -- 44 corridors, 72 mouvements, 136 zones de conflit, **0 adjacence, 0 plan de signal** : les cas adjacence et signal ajoutent ces records via `Mutate`.
- `Assets/RoadRage/Tests/EditMode/Story59ParameterizedDriverModelTests.cs:524-583` -- corpus V1 (V1-B03/B04/B05) : intentions et nombres a reprendre (demi-longueur greybox 2,22 m, s0 = 2 m, tampons 32 et 48), pas le mecanisme.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrameInputs.cs` -- entrees (horizon publie, danger, phase de signal, fermeture) et leur validation -- garde la frame lisible.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs` -- entrees optionnelles, occupation par element, index spatial 3D, signaux par mouvement, fermetures, requete spatiale bornee -- vue partagee unique.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Perception/AgentObservation.cs` -- faits, metadonnees, canaux et saturation, `Perceived`, `ToText()`.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs` -- `PerceptionLimits` (portees, fenetre, capacites ; refus si non fini, ≤ 0 ou capacite < 1) et `Observe`.
- [x] `Assets/RoadRage/Tests/EditMode/Story532PerceptionTests.cs` `[Category("Core")]` `[Category("Story532")]` -- couvrir :
  - la matrice ;
  - l'immuabilite : aucun setter public, collections en lecture seule, frame identique avant et apres N perceptions ;
  - l'ordre d'entree melange ;
  - la vue partagee : aucun champ d'observation de type `TrafficFrame`, `TrafficActor` ou collection de ceux-ci ;
  - des faits sans decision : aucun membre public nomme `RightOfWay`, `Priority`, `Grant`, `Yield`, `Pedal`, `Brake`, `Throttle`, `Steer`, `Allowed` ;
  - un scan de `Frame/` et `Perception/` : liste interdite 5.30, plus `Physics.` et `SphereCast` ;
  - adjacence, signal, fermeture et `IntentPathOverlap` (meme element, zone de conflit) sur modeles mutes ;
  - une frame sans nouvelles entrees : acteurs et localisation identiques a la 5.30, nouveaux index vides ;
  - l'occupation conservatrice sur courbe serree : contenance contre une grille dense de l'interieur du rectangle, reste r > 0 et fini, `OccupancyNotBounded` quand κ_max·d_max ≥ 1 ;
  - des scenarios **multi-acteurs** dans une seule frame :
    - agent, leader et suiveur sur le meme corridor ;
    - plusieurs vehicules devant, sur des elements successifs de l'horizon (le leader est le plus proche, les autres ne masquent rien) ;
    - plusieurs occupants du corridor de sortie (longueur libre jusqu'au premier, nombre exact) ;
    - un acteur sur une voie adjacente ;
    - un vehicule non localise present une seule fois comme obstacle, et aucune requete d'obstacles ne rend deux fois le meme id ;
    - une saturation de tampon avec plusieurs candidats ;
    - les observations successives de plusieurs agents depuis la meme frame : chacune egale a son observation calculee seule, dans n'importe quel ordre d'agents, frame inchangee (aucun etat qui fuit).
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- statut de la story. Puis `graphify update .`.

**Acceptance Criteria:**

- Given un pas hote, when la frame est construite, then c'est une vue immuable unique, ordonnee et indexee, qui contient acteurs, empreintes, vitesses, localisation, occupation, signaux, fermetures et horizons publies, et aucun consommateur ne peut l'ecrire.
- Given un agent et une frame, when la perception s'execute, then elle rend leader, suiveur, occupation adjacente, obstacles, joueurs et pietons, `IntentPathOverlap` et occupation de sortie, chacun date, borne, source et qualifie ; aucun ne dit qui passe, quelle regle plier ni quelle commande appliquer.
- Given un leader juste apres un virage, when la perception s'execute, then il est trouve le long de l'horizon et son jeu est mesure pare-chocs a pare-chocs depuis les occupations conservatrices, jamais sous-estimees.
- Given plusieurs acteurs dans une frame commune, when plusieurs agents sont observes successivement, then chaque observation est correcte, deterministe, bornee, sans decision, et independante des autres.
- Given un tampon borne, when il y a plus de candidats que sa capacite, then la saturation est rapportee explicitement.
- Given les appelants 5.30 et 5.31, when le projet compile, then ils compilent sans modification.

## Spec Change Log

## Design Notes

**Pourquoi deux sources.** L'occupation structuree ignore un vehicule non localise (pousse hors de la chaussee) ou un pieton. L'index spatial ignore l'ordre d'arc d'une voie courbe. Le cas « vehicule non localise » et le cas « leader apres un virage » prouvent chacun ce que l'autre source manquerait.

**Jeu sur l'arc.** jeu = D(arriere_L) − D(avant_A), ou D est la distance d'horizon a l'abscisse projetee. En courbe, c'est l'arc du chemin et non la corde : la grandeur que l'IDM 5.33 consomme. Exemple droit : s_A = 10, avant_A = 2,22 ; s_L = 20, arriere_L = 2,22 ; jeu = 5,56 m (centre a centre : 10 m).

**Occupation conservatrice, pourquoi elle tient.** Hors des extremites de l'element, s(p) est lisse tant que κ·d < 1, et |∇s| ≤ 1/(1 − |κ|·|d|). Son gradient ne s'annule jamais : ses extrema sur le rectangle sont donc sur le bord. Tout point du bord est a moins de h/2 d'un echantillon, d'ou le reste r. Sur un arc pur, les coins suffiraient ; sur une clothoide ou a une variation de courbure, pas forcement. C'est ce que la subdivision couvre. La projection bornee prolonge s le long de la tangente terminale, ce qui reste 1-lipschitzien.

**Index spatial.** Tableau trie par x minimal, recherche dichotomique, puis filtre 3D des boites. `ponytail:` O(n) sur une bande x dense ; grille si la 5.46 mesure un cout.

**Hors 5.32 : premier consommateur runtime = 5.33.** Ordre verifie dans `epics.md` : 5.32 → 5.33 sur la branche de suivi. Le jeu de contraintes nommees de la 5.33 contient « leader following » et « obstacle » : c'est le premier consommateur du leader et des dangers. La 5.34 lit l'occupation de sortie plus tard, sur l'autre branche. La 5.33 porte donc :
- la frame partagee unique par pas ;
- le collecteur hote des dangers physiques, sous le meme contrat de tampon borne, de saturation et d'identite unique ;
- les valeurs runtime de `PerceptionLimits` (la 5.32 n'en code aucune) ;
- le harness multi-vehicule deterministe (`deferred-work.md`).

Deux reserves pour la specification de la 5.33. `epics.md` ne lui donne aucun PlayMode au niveau story : le premier PlayMode multi-vehicule de l'epic est le jalon 2 de la 5.35, avec la mesure de cout multi-agents de la Gate C, qui reutilisera ce harness. Et plusieurs vehicules V2 simultanes levent `V2SliceMaxPopulation = 1`, fige par la 5.31 : ce sera une decision proprietaire.

## Verification

**Commands:**

- `.\scripts\validate.ps1 -Profile Story -Story 5.32 -TestMode EditMode` -- expected: recapitulatif `VALIDATION STORY`, fixture `Story532` verte, 0 erreur Console, compilation saine (appelants 5.30/5.31 compris).

## Review Findings

Revue du 2026-10-02 (`blind-hunter`, `edge-case-hunter`, `verification-gap` ; `security-review` inactive : aucune frontiere reseau). Aucun `intent_gap` ni `bad_spec`. Correctifs appliques, puis `Story532` 27/27 EditMode, 0 erreur Console, `MVP_Run` propre :

- [x] [Review][Patch] Leader a cheval sur la fin de l'horizon, ou a la meme abscisse que l'agent, invisible dans tous les canaux -- premier intervalle juge sur sMin, egalite departagee par id (leader et suiveur complementaires).
- [x] [Review][Patch] Confiance hors regle de la spec : obstacle-acteur = min des deux localisations (0 si non localise), sortie = min avec l'agent.
- [x] [Review][Patch] Vitesse d'un acteur-obstacle nulle -- avant normalise x vitesse tangentielle.
- [x] [Review][Patch] Portee du suiveur annoncee a `RearRangeMeters` meme quand un seul element en amont est cherche -- portee effective publiee.
- [x] [Review][Patch] Boite de requete sans l'arriere de l'agent -- coins de l'empreinte inclus.
- [x] [Review][Patch] Recouvrement de zone de conflit hors de la portion declaree d'un horizon tronque -- plage de la zone sur chaque mouvement, restreinte a [s0, s1].
- [x] [Review][Patch] Genre de danger non defini accepte -- `InvalidHazard`, correspondance explicite des genres.
- [x] [Review][Patch] Tests ajoutes : `AgentOccupancyUnavailable`, suiveur sur predecesseur et portee effective, debord avant le debut d'element (jeu et longueur libre negative), filtres d'exclusion (fenetre adjacente, autre etage, derriere l'agent, intervalle disjoint), genres de dangers, confiance et vitesse d'un acteur pousse.
- [x] [Review][Defer] Projection globale sur un element replie -- `deferred-work.md` ; non atteignable sur `MVP_Run`.
- Risque accepte (regle de build) : les fixtures 5.30/5.31, dont le scan `NewPlanningSurfaceContainsNoControlOrTargetIndex`, ne tournent qu'en fin d'epic ; `FactsStateNoDecision` et `FrameAndPerceptionSourcesUseNoPhysicsNorControlPath` reprennent leurs interdits sur `Frame/` et `Perception/`.

### Revue independante du commit 5cc4f4b (2026-10-02)

Perimetre : `5f5e776..5cc4f4b`, `Assets/RoadRage/` et artefacts BMAD, hors sorties Graphify generees. 12 fichiers, 2 591 lignes ajoutees / 15 retirees. Les quatre couches `blind-hunter`, `edge-case-hunter`, `verification-gap` et `acceptance-auditor` ont ete executees ; la derniere apres les trois premieres, avec reutilisation d'un agent. Aucun correctif source applique. Les contre-exemples ci-dessous sont issus de la lecture et du calcul geometrique, pas de nouvelles fixtures executees. Verdict : corrections requises ; 6 patches, aucune decision de specification requise.

- [x] [Review][Patch][R1][high] La saturation spatiale peut supprimer l'obstacle le plus proche [Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs:293]. `QuerySpatial` remplit le tampon dans l'ordre x minimal puis id ; `SpatialQueryBuffer.Add` abandonne les entrees suivantes. La perception ne classe par distance que ce prefixe. Sur NB2, agent a s=2, deux dangers de meme x et memes extents, danger de petit id a s=25 et danger de grand id a s=8, capacite spatiale 2 : l'agent et le danger lointain remplissent le tampon, le danger proche disparait. La saturation est annoncee mais la contrainte "les plus proches sont gardes" n'est pas respectee ; le total du canal ne compte que les faits du prefixe. Le test de saturation actuel n'asserte les identites proches que lorsque la requete spatiale ne sature pas. Correction : filtrer et classer les candidats eligibles avant leur perte, tout en conservant tampon fixe, total, determinisme et requete sans allocation. Sources : les quatre couches.

- [x] [Review][Patch][R2][high] Le choix du centre le plus proche masque une boite qui touche une autre portion du chemin [Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs:329]. `TryObstacle` choisit un seul intervalle par distance du centre, puis applique les filtres d'emprise sans essayer les autres intervalles. Sur la route synthetique NB2 -> MR -> EB, agent NB2 s=5, danger centre=(25,0,20), extents=(1,1,20) : NB2 est le plus proche du centre (19 m, contre 20 m pour EB), puis l'ecart lateral 16,97 m provoque le rejet. Pourtant la boite x=[24,26], z=[0,40] touche la ligne centrale de EB a z=40. C'est un faux negatif meme sans saturation. Correction : evaluer l'intersection et les filtres de la boite sur chaque intervalle, puis choisir le fait eligible pertinent. Ajouter ce cas a `Story532`. Sources : edge-case-hunter, acceptance-auditor.

- [x] [Review][Patch][R3][high] Le recouvrement de zone ignore les enveloppes et les intersections entre echantillons [Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs:452]. `ZoneSpan` exige qu'un point echantillonne de chaque ligne centrale entre dans le volume. Dans le modele synthetique, zone centre=(7.25,0,34), extents=(0.1,2,0.1), membres MS/MR : les deux enveloppes de demi-largeur 2 m touchent la zone, mais ni MS (x=6) ni MR (x environ 6,79..6,88 sur cette tranche z) n'y fait entrer sa ligne centrale. Un horizon complet sur chaque mouvement ne produit aucun fait. De plus, une intersection de ligne centrale dans s=[0.04,0.06] peut tomber entre les echantillons a 0 et 0,1 m. Le contrat de contexte definit les mouvements par leurs enveloppes balayees ; le commentaire `ponytail:` n'autorise pas leur remplacement par les lignes centrales. Correction : intersection continue ou conservatrice des enveloppes avec la zone, restreinte aux portions declarees, avec fixtures d'enveloppe seule et de zone mince. Sources : blind-hunter, edge-case-hunter, acceptance-auditor.

- [x] [Review][Patch][R4][medium] Le d_max de l'occupation differe de la formule figee [Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs:265]. La spec impose la distance maximale des echantillons a la courbe plus h/2 ; le code utilise seulement `abs(LateralOffsetMeters)`, sans composante normale ni depassement longitudinal aux bornes. `RoadProjection.DistanceMeters` porte la distance 3D demandee. Cela change le reste et peut accepter une occupation que le seuil specifie exclut, notamment pour une empreinte depassant fortement une extremite de mouvement courbe. C'est un ecart de contrat confirme, pas a lui seul une preuve de sous-contenance du rectangle. Correction : utiliser la distance prescrite et tester les composantes omises, sans modifier `RoadLocalizer`. Sources : blind-hunter, acceptance-auditor.

- [x] [Review][Patch][R5][medium] La requete large applique la tolerance normale uniquement sur l'axe y monde [Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs:289]. Le filtre fin utilise `point.Up`, mais l'AABB n'ajoute `AcceptanceDistanceMeters` que sur y. Sur un corridor droit de tangente z, road-up=(sin(80 deg),cos(80 deg),0), agent de demi-largeur 1,03 m et `LateralRangeMeters=0.1`, ses coins et l'elargissement portent la borne x a environ 2,14 m de la reference. Un petit danger de demi-extents 0,01 m, a 2,4 m selon road-up et 5 m devant, a x environ 2,36 m : il est elimine par la requete alors que son ecart normal est inferieur au seuil 2,5 m et son ecart lateral est eligible. Le validateur autorise un road-up constant dont le produit scalaire avec le haut monde est positif ; ce cas ne concerne pas la carte plane actuelle. Correction : une borne large couvrant aussi la tolerance normale selon les reperes de route, avec test sur modele incline valide. Source : blind-hunter ; contre-exemple ajuste par le triage pour respecter le validateur (road-up strictement ascendant).

- [x] [Review][Patch][R6][medium] Des fenetres d'adjacence recouvrantes dupliquent les occupants [Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs:250]. Chaque record ajoute les occupants sans deduplication. Deux adjacences d'ids differents, memes corridors et cote, fenetres recouvrantes, sont valides : les validateurs controlent leurs ids et leur geometrie individuellement, sans interdire ce recouvrement. Avec deux voisins et `ListCapacity=2`, les deux occurrences du voisin le plus proche peuvent prendre les deux places, cacher le second et gonfler total/saturation. Correction : produire un fait par identite occupant/corridor/cote avant le bornage, avec fixture de fenetres recouvrantes. Source : blind-hunter.

**Triage.** Les doublons de couches ont ete fusionnes. Neuf autres propositions ne deviennent pas des actions de cette revue : distance d'obstacle sur un arc ideal sans temoin epingle sur la courbe compilee ; provenance d'horizon non specifiee et absence de consommateur runtime ; bornes s publiees que le contrat n'interdit pas ; overflow aux tailles proches de `float.MaxValue` sans cas d'usage atteint ; messages de pose invalides herites de la 5.26 ; metadonnees des diagnostics `UnmeasuredActors`, dont le contrat donne explicitement les champs ; enrichissement de `ToText()` et de ses assertions sans fuite d'etat constatee ; test supplementaire d'une copie d'horizon deja presente ; assertion supplementaire de confiance de sortie dont le calcul actuel suit deja la spec. La projection sur element replie reste la dette deja enregistree, sans nouvelle entree de defer. Les tests de non-regression des six correctifs retenus restent a ajouter.

**Verification executee par cette revue.** Commande autorisee : `.\scripts\validate.ps1 -Profile Story -Story 5.32 -TestMode EditMode`. Extraits bruts du recapitulatif (code de sortie 0) :

```text
  VALIDATION STORY (Story532)
  Profil                   Story (Story532)
  Tests executes           27 au total (27 en EditMode)
  Suites completes         non executees ; reservees a la fin d'epic
  Unity CLI                1.0.0-beta.8
  Editeur                  port 7800, Unity 6000.6.0f1, ready
  Curseur Console          81 (0 erreur(s) anterieure(s) ignoree(s))
  Recompilation            up_to_date
  Erreurs Console          0 depuis le curseur 81
  Etat de compilation      sain (scriptCompilationFailed=false)
  Avertissements hors Synty 3 (informatif, hors gate)
  Selection                27 test(s) attendus (categorie Story532) ; 1042 non executes par selection ; 2 test(s) [Explicit] hors suite par defaut
  Tests EditMode           27/27 passes
  Ignores EditMode         0 skipped, 0 inconclusive (reels, dans l'execution)
  Non executes (selection) 1042 test(s) de la suite EditMode, hors profil Story
  Scenes ouvertes          aucune scene modifiee
  Arbre de travail         4 entree(s) modifiee(s) -- voir git status
OK
VALIDATION STORY (Story532) : tests de la story seulement. Suites completes non executees ; elles relevent de la fin d'epic. Cette sortie ne vaut pas validation complete.
```

Scene ouverte avant/apres : `MainMenuLobby`, `isDirty=false`. Les quatre modifications preexistantes concernent uniquement `graphify-out/`. Cette revue n'a pas sauvegarde de scene, ni modifie de source, modele, preuve ou signature. Au terme de la revue initiale, le statut `in-progress` refletait les correctifs encore ouverts.

### Resolution des six patches (2026-10-02)

Choix proprietaire : **1, appliquer les six correctifs**, sans confirmation par constat. R1 a R6 sont resolus ; aucun nouvel outil, package, asmdef, acces physique ou branchement runtime.

- **R1** : pagination optionnelle de `QuerySpatial`, toujours sans allocation et avec un tampon fixe. La perception parcourt toutes les pages, compte les faits eligibles exactement et ne conserve que les `ListCapacity` plus proches (distance puis id). La saturation spatiale reste explicite. `SaturatedSpatialPagesKeepTheNearestEligibleFactsAndTheirExactTotal` : trois capacites spatiales (1, 2, 4), trois capacites de liste (1, 2, 8), ordre id/distance inverse, candidats hors etage, comparaison a la requete non saturee et reutilisation du tampon. Cout connu O(n * pages) documente par `ponytail:` ; curseur d'index si mesure de besoin en 5.46.
- **R2** : filtres d'emprise sur chaque intervalle avant selection du fait eligible le plus proche. Le corps doit rencontrer l'intervalle effectivement cherche ; seul le premier intervalle inclut l'arriere de l'agent. `AnObstacleBoxTouchingTheOutgoingPathSurvivesAnIneligibleNearestCenterProjection` : boite du contre-exemple trouvee sur EB, distance et appartenance au couloir verifiees, absence avec horizon court.
- **R3** : parcours des segments compiles, reutilisation de `RoadCurve.Bounds` pour couvrir les bords tournants et les largeurs variables, puis intersection continue segment/boite elargie. Les plages restent dans les portions declarees. Les bornes AABB sont conservatives : elles peuvent ajouter un recouvrement potentiel, jamais le supprimer ; cette limite et l'option d'intersection polygonale sont explicites dans le commentaire `ponytail:`. `ConflictOverlapCoversEnvelopeOnlyAndBetweenSampleIntersectionsWithoutExtendingHorizons` : cas enveloppe seule et zone mince, puis horizons tronques de chacun des deux agents. Pas de prediction temporelle.
- **R4** : `d_max` utilise `RoadProjection.DistanceMeters`. `OccupancyRemainderIncludesNormalDistanceAndRejectsAnUnboundedEndpointOverhang` : le reste augmente pour un decalage normal, et une longue empreinte qui depasse l'extremite d'un mouvement est exclue avec `OccupancyNotBounded`.
- **R5** : la borne large couvre la tolerance normale sur les trois axes monde ; le filtre fin garde le repere road-up. `SpatialQueryIncludesTheNormalAllowanceOnABankedRoad` : modele compile incline a 80 degres, horizon droit court, petit danger a 2,4 m selon road-up, ecart normal et appartenance au couloir verifies.
- **R6** : un seul fait par occupant/corridor/cote, avec conservation de l'ecart le plus proche avant bornage. `OverlappingAdjacencyWindowsDoNotDuplicateOccupantsOrConsumeTheirCapacity` : deux fenetres recouvrantes, deux voisins, capacite 2, ids exacts, total 2 et absence de saturation.

**Validation executee apres correctifs**, exclusivement par `.\scripts\validate.ps1 -Profile Story -Story 5.32 -TestMode EditMode` ; code de sortie 0, 9 cas ajoutes aux 27 existants. Extraits bruts :

```text
  VALIDATION STORY (Story532)
  Profil                   Story (Story532)
  Tests executes           36 au total (36 en EditMode)
  Suites completes         non executees ; reservees a la fin d'epic
  Unity CLI                1.0.0-beta.8
  Editeur                  port 7801, Unity 6000.6.0f1, ready
  Curseur Console          161 (0 erreur(s) anterieure(s) ignoree(s))
  Recompilation            completed
  Erreurs Console          0 depuis le curseur 161
  Etat de compilation      sain (scriptCompilationFailed=false)
  Avertissements hors Synty 29 (informatif, hors gate)
  Selection                36 test(s) attendus (categorie Story532) ; 1042 non executes par selection ; 2 test(s) [Explicit] hors suite par defaut
  Tests EditMode           36/36 passes
  Ignores EditMode         0 skipped, 0 inconclusive (reels, dans l'execution)
  Non executes (selection) 1042 test(s) de la suite EditMode, hors profil Story
  Scenes ouvertes          aucune scene modifiee
  Arbre de travail         10 entree(s) modifiee(s) -- voir git status
OK
VALIDATION STORY (Story532) : tests de la story seulement. Suites completes non executees ; elles relevent de la fin d'epic. Cette sortie ne vaut pas validation complete.
```

Les warnings projet restent visibles dans la sortie de validation (assembly SandboxStops vide, API obsoletes et avertissement UAC0005 hors fichiers modifies) ; aucun n'est masque par le correctif. `MainMenuLobby` reste propre. Modele, scene, preuve et signature `MVP_Run` inchanges. Les suites completes restent reservees a la fin d'epic selon la decision proprietaire.

`graphify update .` execute avec succes : 6 624 noeuds, 18 221 liens, 216 communautes. Depassement preexistant de 5 000 noeuds conserve sans modifier le perimetre. Avertissement d'extraction AST partielle sur `Story531DrivenReplayTests.cs` non modifie (ligne 112) ; la compilation Unity est saine. Les noms de certaines communautes ont ete recalcules par leur hub ; aucune nouvelle passe LLM de labelisation demandee. Statuts de la story et du sprint : `done`.

## Suggested Review Order

**Perception : une fonction pure sur la frame partagee**

- Point d'entree : statut par canal, aucun fait sans occupation de l'agent.
  [`TrafficPerception.cs:42`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L42)
- Leader le long de l'horizon, jeu d'arc entre occupations conservatrices.
  [`TrafficPerception.cs:114`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L114)
- Egalite d'abscisse departagee par id : leader et suiveur complementaires.
  [`TrafficPerception.cs:145`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L145)
- Suiveur sur un element en amont, portee effectivement cherchee publiee.
  [`TrafficPerception.cs:150`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L150)
- Obstacles par index spatial, hors acteurs deja structures sur l'horizon.
  [`TrafficPerception.cs:260`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L260)
- Couloir balaye, filtres vertical et arriere, confiance selon la source.
  [`TrafficPerception.cs:313`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L313)
- Recouvrement d'intention spatial, jamais au-dela de l'horizon declare.
  [`TrafficPerception.cs:372`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L372)
- Sortie visee : longueur libre jusqu'a l'arriere du premier occupant.
  [`TrafficPerception.cs:464`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs#L464)

**Frame : vue immuable et index construits une fois**

- Occupation conservatrice : bords subdivises, reste explicite, exclusion si non bornee.
  [`TrafficFrame.cs:238`](../../Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs#L238)
- Construction : validation nommee, exclusions, occupation et index spatial.
  [`TrafficFrame.cs:87`](../../Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs#L87)
- Requete spatiale bornee, sans allocation, saturation jamais silencieuse.
  [`TrafficFrame.cs:204`](../../Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs#L204)
- Identite unique acteur/danger et genres valides.
  [`TrafficFrame.cs:333`](../../Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs#L333)
- Tampon a capacite fixe possede par l'appelant.
  [`TrafficFrameInputs.cs:143`](../../Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrameInputs.cs#L143)

**Faits publies**

- Observation : champs 5.30 conserves, canaux bornes, `Perceived`.
  [`AgentObservation.cs:187`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/AgentObservation.cs#L187)
- Texte deterministe invariant de culture, preuve de determinisme.
  [`AgentObservation.cs:235`](../../Assets/RoadRage/Features/Vehicles/Traffic/Perception/AgentObservation.cs#L235)

**Preuves**

- Contenance du rectangle sur la courbe la plus serree de `MVP_Run`.
  [`Story532PerceptionTests.cs:453`](../../Assets/RoadRage/Tests/EditMode/Story532PerceptionTests.cs#L453)
- Leader apres le virage, manque par un balayage dans l'axe avant.
  [`Story532PerceptionTests.cs:426`](../../Assets/RoadRage/Tests/EditMode/Story532PerceptionTests.cs#L426)
- Plusieurs agents depuis une meme frame : aucun etat qui fuit.
  [`Story532PerceptionTests.cs:934`](../../Assets/RoadRage/Tests/EditMode/Story532PerceptionTests.cs#L934)
- Acteur pousse non localise : obstacle unique, jamais aussi danger.
  [`Story532PerceptionTests.cs:685`](../../Assets/RoadRage/Tests/EditMode/Story532PerceptionTests.cs#L685)
