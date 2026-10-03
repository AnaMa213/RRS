---
title: 'Story 5.33 -- Suivi de vehicule par contraintes de vitesse nommees : premier consommateur runtime de la perception et premieres explorations multi-vehicules V2 dans MVP_Run'
type: 'feature'
created: '2026-10-02'
status: 'done'
baseline_commit: 'fb8d793c7d949ea4c9e2475365f3a2b0fec5cda1'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-32-perception-boundary-occupancy-leader-and-hazards.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-31-first-driven-traffic-v2-vertical-slice-portal-to-portal.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Les faits de perception 5.32 n'ont aucun consommateur. Chaque driver V2 construit sa propre `TrafficFrame` mono-acteur, a empreinte nulle et sans danger. Le plan de vitesse ignore leader et obstacles, et la limite de route y reste reportee. Un seul vehicule V2 peut vivre (`V2SliceMaxPopulation = 1`), et aucune attente n'est decrite comme blocker. Le trafic V2 ne peut donc ni former une file, ni etre observe a plusieurs dans `MVP_Run`.

**Approach:**
- Une seule frame hote par pas physique, partagee par tous les vehicules V2, avec empreintes declarees et dangers physiques collectes.
- La perception 5.32 par vehicule, sur cette frame et son horizon.
- Trois contraintes nommees de plus : limite de route appliquee, suivi de leader (IDM 5.9) et obstacle dans le couloir balaye. Elles sont arbitrees avec le profil spatial 5.31 ; la liante est nommee et les alternatives conservees.
- Un ensemble de blockers.
- Un harness PlayMode de scenarios multi-vehicules dans `MVP_Run` : deux scenarios d'acceptation et une campagne exploratoire a 2, 4 et 8 vehicules, qui publie des constats sans les juger.

**Objectif :** des vehicules V2 reels se suivent et forment une file credible, a l'ecart authore mesure pare-chocs a pare-chocs, sans contact entre eux, dans la portee de la Gate B. Ni la coordination de carrefour (5.34/5.35) ni la Gate C ne sont revendiquees.

## Boundaries & Constraints

**Always:**

- **Prealables.** Decisions proprietaires D1 a D8 du 2026-10-02 (Design Notes). Gate A format 3 valide (`kinematic-v1`, a_e = 0,34 m), Gate B jalon 1 consignee. Modele, geometrie, scene, preuves et signature `MVP_Run` inchanges.
  - **Revue de la Story 5.52 cloturee** (statut `done`) avant toute implementation : elle touche le meme driver et les memes invariants Gate B (D8).
  - Complexite **L** (et non M) ; PlayMode au niveau story, amendement de `epics.md` consigne en phase 0 (D1).
- **Frame partagee.** Un ordonnanceur hote sans etat de decision construit une seule `TrafficFrame` par pas physique, avant tout pas de conduite.
  - `FrameId` = compteur de pas hote global.
  - Acteurs : tous les vehicules V2 lies, tries par `TrafficId`. Dangers : ceux du collecteur.
  - Chaque driver decide seulement depuis cette frame ; aucune copie par vehicule.
  - Ordre dans le `FixedUpdate` du spawner : retrait aux portails de sortie, puis frame et pas de chaque vehicule par `TrafficId` croissant, puis insertion. Un vehicule insere conduit au pas suivant.
  - Les drivers ne s'auto-cadencent plus. Chaque vehicule lie recoit exactement un pas, donc un intent, par pas physique.
  - Epoques : le compteur global porte frame, commande et composeur. La trace garde un compteur par vehicule, 1 au premier pas, sans trou ; le jalon 5.52 en depend.
  - Horizons d'intention publies : non alimentes (5.34).
- **Empreintes.** Chaque acteur V2 declare l'empreinte de son `BoxCollider` de caisse, depuis le point de reference (origine, H3 5.31). Sur le prefab actuel : avant et arriere 2,22 m, gauche et droite 1,03 m. La localisation lit la meme empreinte (contrat AD-45).
- **Collecteur de dangers** (`Lifecycle/`, hote seul, physique Unity permise).
  - Une requete `OverlapSphereNonAlloc` par vehicule V2 et par pas, rayon `HazardQueryRadiusMeters`, triggers ignores, tampon fixe.
  - Les colliders sont dedupliques par racine (`attachedRigidbody`, sinon `CharacterController`), puis classes :
    - `CharacterController` -> `WalkingPlayer` ;
    - `Rigidbody` portant `VehiclePhysicsBody` ou `NetworkedAIVehicleState` -> `Vehicle` ;
    - autre `Rigidbody` -> `Obstacle`.
  - Ecartes : collider statique (sans l'un ni l'autre), vehicule propre, toute racine qui porte un `TrafficV2VehicleDriver`, liee ou non. Ils sont comptes dans un diagnostic.
  - Un vehicule V2 present comme `TrafficActor` n'est jamais aussi un danger `Vehicle`. La deduplication se fait par racine et par identite. Un meme vehicule ne produit donc jamais a la fois un leader structure et un obstacle physique independant.
  - Boite = AABB monde du collider ; vitesse du corps ; confiance 1.
  - Identite : `RoadId` d'un domaine danger, stable pendant la session. Un id qui egale celui d'un acteur n'est pas emis et est compte (`HazardIdentityCollision`) : la frame ne leve jamais `DuplicateIdentity` de ce fait.
  - Saturation : une requete pleine (resultats = capacite) est saturee. Elle est publiee avec le vehicule qui l'a emise, jamais silencieuse. Aucune classification par motif de nom.
- **Valeurs runtime** (constantes uniques dans `TrafficV2Settings`, parametres de conception) :
  - `PerceptionLimits` : arriere 30 m, lateral 1 m, fenetre adjacente 10 m, capacite de liste 8 ;
  - tampon spatial 32 par vehicule, reutilise ;
  - collecteur : rayon 40 m, tampon 64 ;
  - population maximale d'un scenario : 8.
  - Le harness publie totaux et saturations pour les confirmer ; toute revision est Ask First.
- **Perception.** Apres le spine, `TrafficPerception.Observe(frame, id, decision.Path, limits, buffer)`. L'observation entre dans la projection.
- **Profil spatial** (`SpeedPlan`, 5.31). `RoadLimit` est applique comme plafond nomme, au meme rang que `DesiredSpeed` et `CurveLimit`.
  - Valeur d'un corridor : `SpeedLimitMetersPerSecond` compile.
  - Valeur d'un mouvement : min des valeurs authorees de ses corridors d'origine et de destination.
  - 0 = non authoree : aucun plafond, publiee `Unauthored`, jamais comme une limite appliquee.
  - Plus aucune limite reportee.
- **Arbitrage longitudinal** (`Planning/`, pur). Candidats nommes en acceleration (m/s²) :
  - le profil : (v_plan(apercu) − v)/Δt, sous le nom de sa contrainte limitante ;
  - `DesiredSpeed` : IDM route libre ;
  - `LeaderFollowing` : `DriverModel.ComputeAcceleration(profil, v, v_L, jeu)`, si le canal leader est evalue et un leader present ;
  - `Obstacle` : le minimum, sur les faits du couloir balaye (ecart lateral ≤ 0), de l'IDM au jeu `NearDistanceMeters`, avec la vitesse du danger projetee sur la tangente du point d'horizon le plus proche de la face proche, bornee a ≥ 0 ;
  - `PerceptionUnavailable` : 0, si la perception du vehicule est indisponible ou potentiellement tronquee. Raison publiee :
    - `ChannelUnavailable` : un canal a emprise n'est pas `Evaluated` ;
    - `ChannelSaturated` : un canal ou la requete spatiale de la perception rapporte `Saturated` ;
    - `HazardCollectorSaturated` : la requete du collecteur emise pour ce vehicule est saturee.
    Une perception tronquee n'est jamais lue comme complete ;
  - `SteeringCeilingUnreachable` : −`SafeBrakingLimit` (regle 5.31).
  - Cible = minimum. Egalite a 1e-6 pres departagee dans cet ordre : `SteeringCeilingUnreachable`, `PerceptionUnavailable`, `Obstacle`, `LeaderFollowing`, profil, `DesiredSpeed`.
  - Liante nommee ; alternatives rejetees conservees avec leur valeur. L'arbitrage lui-meme n'a aucun etat entre deux appels.
  - **Restriction immediate, reprise lissee (D5).** Une contrainte qui se resserre s'applique au pas meme ou la frame la montre : leader qui ralentit, jeu qui diminue, obstacle qui apparait, limite qui baisse. Aucune latence, aucun lissage ne retarde un freinage. Seule la reprise est lissee.
    - Le lissage s'applique quand la liante du pas precedent etait `LeaderFollowing`, `Obstacle` ou `PerceptionUnavailable` et que la cible augmente.
    - Acceleration appliquee = `DriverModel.SmoothAcceleration(precedente, cible, reactionTime du profil, Δt)`, plafonnee a la cible.
    - Le lissage cesse des que la cible est ≤ a l'acceleration appliquee : la cible redevient immediate.
    - Seul etat : l'acceleration appliquee precedente, portee par le driver et donnee en entree a une fonction pure.
    - Sans contrainte d'interaction, l'acceleration est celle de la 5.31 (aucun lissage) : route libre inchangee pour la Gate B.
  - Le profil spatial continue de passer `MotionPlan.VerifySpeedProfile` ; l'angle de roue 5.31 est inchange.
- **Blockers** (`Blockers/`, pur). Record : `Id`, `Source`, `Kind`, `BlockingActorOrRule`, `Legitimate`, `ExpectedToClear`, `Recoverable`, `SinceFrame`.
  - Une cause externe devient blocker quand son candidat vaut ≤ 0 : elle empeche la progression a cette frame. Jamais sur la seule vitesse basse. Causes de la 5.33 :
    - le leader (candidat `LeaderFollowing`) ;
    - chaque obstacle du couloir balaye, un record par obstacle (son propre candidat IDM) ;
    - l'immobilisation de politique : v0 effectif ≤ 0,001 m/s (seuil de `DriverModel`), candidat `DesiredSpeed`.
  - Proprietes par genre : table des Design Notes.
  - `SinceFrame` = premiere frame de presence continue du meme (genre, bloqueur). Le suivi vit dans le driver, mis a jour par une fonction pure.
  - L'ensemble complet est publie. Un dominant des que l'ensemble est non vide : le blocker au plus petit candidat, egalite departagee par genre puis id. Il peut differer de la liante, par exemple quand une limite de courbe freine davantage.
- **Projection.** `TrafficDecisionProjection` et la trace s'enrichissent par ajout seul, avec `ToText()` invariant de culture :
  - faits leader et obstacles retenus, statuts et saturations ;
  - candidats, liante, acceleration visee ;
  - blockers et dominant ;
  - compteurs du collecteur.
  - Les champs 5.31 gardent leur sens. Hote seul : aucune `NetworkVariable`, RPC ni `OnValueChanged`.
- **Scenario de test** (`TrafficV2Scenario`). Jeton construit seulement sous `Tests/` (garde structurelle comme `MeasurementRun`).
  - Il porte une etiquette, une population maximale (1 a 8) et des insertions ordonnees : entree, sortie, graine, pas d'insertion au plus tot.
  - Il n'accorde aucune permission : insertion hors mesure, couverture `Covered` exigee, repli 2a actif.
  - Exclusif avec `MeasurementRun`, refus nomme. Aucun objectif intermediaire.
  - Une insertion attend un portail libre (`IsPositionClear`). Le pas reel d'insertion est publie.
  - Sans scenario, la population reste 1 ; `V2SliceMaxPopulation` reste la valeur par defaut.
- **Harness PlayMode** dans `MVP_Run`, bootstrap -> lobby -> run comme 5.31/5.52.
  - Un constructeur EditMode choisit les insertions par `RoutePlanner`, de facon deterministe.
  - Traces brutes par pas et par vehicule, resume par scenario et cout par etape, sous `_bmad-output/implementation-artifacts/traffic-v2-5-33-explorations/`.
  - Les objets ajoutes par un test (obstacle cinematique) sont crees a l'execution et detruits a la fin. La scene n'est jamais sauvegardee.
  - **Joueur hote controle.** Chaque scenario PlayMode 5.33 place le joueur hote, s'il existe, a une pose de stationnement declaree, hors de tout couloir balaye des routes du scenario et a plus du rayon du collecteur de ces routes.
    - Au debut et a chaque pas, le test verifie que le joueur n'apparait dans aucun fait leader ou obstacle d'un vehicule V2.
    - Sinon, le run est invalide : ni reussi ni constat.
  - **Perimetre d'echec.**
    - Les scenarios A et B echouent sur : contact entre vehicules, jeu hors tolerance, file qui ne se resorbe pas, et chaque critere de leur AC.
    - Dans la campagne exploratoire, les contacts et interblocages aux carrefours et giratoires ne sont pas des defauts 5.33 tant que les grants 5.34 n'existent pas. Ce sont des constats publies pour les stories suivantes.

**Ask First:**

- Toute modification du modele, de la scene, de la geometrie, de `Traffic/Migration/**`, de la preuve, du sign-off, de `RoadLocalizer` ou de `scripts/validation-profiles.ps1`. Authorer une limite de vitesse dans `MVP_Run`.
- Un type ou une signature publique 5.25–5.32 modifie autrement que par ajout optionnel. Seules exceptions approuvees ici : application de la limite de route dans `SpeedPlan` et cadence du driver.
- Les types V1 retenus : `DriverModel`, `DriverProfile*`, `VehicleDriveIntent`, `VehiclePhysicsBody`, `VehicleTireModel`, `NetworkedAIVehicle*`.
- Toute fixture 5.30–5.32 ou 5.52 modifiee, hors des assertions de limite reportee de `Story531SpeedPlanAndComposerTests`.
- Une non-regression `Story531` (EditMode ou PlayMode) ou `Story552` PlayMode rouge : HALT. Aucun seuil, test, geometrie ni preuve adapte pour la masquer.
- Une saturation du collecteur qui fait lier `PerceptionUnavailable` en route libre, avec un seul vehicule : HALT. La capacite n'est revisee que par decision proprietaire.
- Une anomalie de cout au sens de D7 : HALT plutot qu'une micro-optimisation.
- Revision des constantes runtime ; population > 8 ; commande de lobby, asmdef, package, `NetworkVariable`, RPC.
- Relacher une tolerance d'acceptation PlayMode apres mesure.

**Never:**

- Lignes d'arret, grants, priorites, sortie bloquee, signaux (5.34–5.36). Evitement, contournement, changement de voie (5.42). `SafetyFilter` (5.37). Interpretation de recuperation (5.39). Consommation d'`IntentPathOverlap`. Rage/Fear.
- Une regle, une observation ou une politique qui ecrit frein ou gaz. Un second composeur. Toute ecriture V2 de pose ou de vitesse.
- La vitesse basse comme proxy de blocage. Une raison d'attente unique qui ecrase l'ensemble.
- Un balayage physique prive de leader dans la boucle de conduite. `Physics.*` dans `Frame/`, `Perception/`, `Planning/` ou `Blockers/`.
- Revendiquer la Gate C, un comportement de carrefour, ou une reproductibilite physique exacte non mesuree. Compter un constat exploratoire comme une reussite.
- Retirer, teleporter ou reinserer un vehicule ailleurs qu'a un portail de sortie.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Leader plus lent | Leader V2 devant sur l'horizon | `LeaderFollowing` lie ; jeu jamais sous s0 en regime | N/A |
| Leader arrete, jeu nul ou negatif | v_L = 0, jeu ≤ 0 | Deceleration ferme, finie, bornee ; domine `DesiredSpeed` | Aucun NaN ni infini |
| Arret derriere un leader | Jeu ≥ s0/2, v ≈ 0 | Blocker `Leader` legitime, non recouvrable | N/A |
| Encastrement | Jeu < s0/2 | Blocker `Leader` illegitime, recouvrable | N/A |
| Leader lointain | Candidat leader > 0 | Aucun blocker ; liante = plus petit candidat | N/A |
| Obstacle dans le couloir | Danger fixe, ecart lateral ≤ 0 | `Obstacle` lie ; arret a s0 de la face proche ; blocker | N/A |
| Obstacle hors couloir | Ecart lateral > 0 | Publie, aucune contrainte | N/A |
| Danger qui approche | Vitesse le long de l'horizon < 0 | Traite a vitesse 0 | N/A |
| Canal indisponible | `UndeclaredFootprint` / `AgentOccupancyUnavailable` | `PerceptionUnavailable` (`ChannelUnavailable`), cible ≤ 0 | Diagnostic |
| Perception tronquee | Canal `Saturated`, ou requete du collecteur pleine pour ce vehicule | `PerceptionUnavailable` (`ChannelSaturated` / `HazardCollectorSaturated`), jamais lue comme complete | Diagnostic |
| Restriction | Leader qui freine apres une phase de reprise lissee | Nouvelle cible appliquee au meme pas, lissage abandonne | N/A |
| Reprise | Leader qui repart, liante precedente `LeaderFollowing` | Hausse lissee par le temps de reaction, jamais au-dela de la cible | N/A |
| V2 acteur et collider | Collider d'un vehicule V2 dans la requete du collecteur | Aucun danger `Vehicle` ; un seul fait (leader ou obstacle `TrafficActor`) | Diagnostic d'exclusion |
| Limite authoree | Corridor a 3 m/s (modele mute) | `RoadLimit` appliquee et liante | N/A |
| Limite absente | 0 authore (`MVP_Run`) | `Unauthored`, aucun plafond | N/A |
| Immobilisation | v0 ≤ ε | Blocker `PolicyImmobilization` legitime | N/A |
| Causes multiples | Leader et obstacle actifs | Deux records ; dominant = liante | N/A |
| Vitesse basse sans cause | Ralenti par le seul profil | Aucun blocker | N/A |
| Identite | Danger dont l'id egale un acteur | Danger non emis | `HazardIdentityCollision` |
| Saturation | Plus de candidats que la capacite | Plus proches gardes, total publie, et `PerceptionUnavailable` | Diagnostic |
| Vehicule pousse | Acteur declare non localise | Obstacle `TrafficActor` unique pour les autres ; lui : repli 5.31/5.52 | Diagnostic |
| Portail encombre | File jusqu'a l'entree | Insertion reportee, jamais dans un vehicule | N/A |
| Scenario et mesure | Deux jetons a la fois | Aucune insertion | Refus nomme |
| Sans scenario | `V2Slice` seul | Population 1, comportement 5.31/5.52 | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs:289-311` -- `FixedUpdate` auto-cadence a supprimer. `:313-446` `Step` :
  - `:323` `frameId = ++stepCounter`, a scinder en epoque globale et compteur de trace ;
  - `:330` pose sans `Footprint` ;
  - `:340-342` frame mono-acteur ;
  - `:396` `SpeedPlan.Build` ;
  - `:421` `MotionCommand.Track` ;
  - `:433-435` composeur et unique `ApplyDriveIntent` ;
  - `:504` record de trace.
  `:540-574` episodes de contact, a reutiliser pour « aucun contact entre vehicules ».
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs:79-95` -- acceleration = min(IDM route libre, suivi du profil). Point d'entree de l'arbitrage, par parametre optionnel.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/SpeedPlan.cs` :
  - `:7-24` `SpeedConstraint`, a etendre par ajout ;
  - `:27-50` `DeferredLimit*`, `:109` `DeferredLimits`, `:361-374` `DeferredLimitsOf` ;
  - `:257-265` caps par noeud : y ajouter `RoadLimit` ;
  - `LimitingConstraint`.
- `Assets/RoadRage/Features/Vehicles/DriverModel.cs` -- lecture seule :
  - `:61` `ComputeAcceleration` (ecart borne a 0,05 m, resultat dans [−20, a]) ;
  - `:167` `SmoothAcceleration` ;
  - `:230` `IsDeliberateStop` (seuil s0/2).
  Profil par defaut (`ScriptableObjects/Vehicles/DriverProfileDef_Default.asset`) : v0 8, T 1,5, s0 2, a 1,5, b 2, b_safe 4, reaction 0,3.
- `Assets/RoadRage/Features/Vehicles/Traffic/Perception/TrafficPerception.cs` :
  - `:9-31` `PerceptionLimits`, aucune valeur codee ;
  - `:38-60` `Observe` et statut de canal ;
  - les canaux a emprise exigent `horizon.Intervals[0].Id` = element occupe.
  `AgentObservation.cs:56-108` -- `VehicleGapFact.GapMeters`, `ObstacleFact` (`NearDistanceMeters`, `InSweptPath`, `Velocity` monde).
- `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs` :
  - `:7-33` `TrafficActorInput`, empreinte dans `Pose.Footprint` ;
  - `:86` constructeur, dangers optionnels, `DuplicateIdentity`.
  `TrafficFrameInputs.cs:59-72` `TrafficHazardInput`, `:143` `SpatialQueryBuffer`. `RoadModelRecords.cs:320` `RoadBoundsBox` (centre, demi-extents > 0).
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs:13-20` -- `VehicleFootprint`. `:296,568-590` : `OutsideWidthEnvelope` evalue aux coins de l'empreinte, donc au seul point de reference aujourd'hui.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` :
  - `:37` `V2SliceMaxPopulation = 1` ;
  - `:85-100` `MeasurementRun` (patron du jeton) ;
  - `:106-120` `TrafficV2Session.Request` ;
  - `:294-318` `PrepareInsertion` (frame 0 mono-acteur, inchangee).
- `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` :
  - `:154-173` `FixedUpdate` hote ;
  - `:327-360` retrait, seul chemin de despawn ;
  - `:464-570` `TickV2Slice` : plafond `:467`, entree tournante et sortie `None` hors mesure, `IsPositionClear`, `Bind`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs:199-226` -- acceleration vers pedales. Frein a main a ≤ 0,05 m/s si a < 0 : maintien d'une file a l'arret.
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:15-48` -- `TrafficDriveOutcome`, a etendre par ajout.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs:899-950` -- lecture seule. Principe V1 de classification : `Rigidbody` ou `CharacterController`, decor statique exclu.
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json` -- 4 portails d'entree, 4 de sortie, 44 corridors, 0 adjacence. Toutes les limites a 0 (28 sections, 44 corridors sans surcharge).
- Tests a garder ou adapter :
  - `Tests/EditMode/Story531SpeedPlanAndComposerTests.cs:251-292` -- les seules assertions a adapter (limite reportee, `DeferredAuthored(3)` jamais appliquee) ;
  - `Story531LifecycleAndCompositionTests.cs:143` -- le defaut 1 reste ;
  - `Tests/PlayMode/Story552MilestonePlayModeTests.cs:66-151` -- `Trace[0].Step == 1`, pas consecutifs, `OutsideWidthEnvelope` faux a chaque pas ;
  - `Story531V2VerticalSlicePlayModeTests.cs:89-176` -- patron de session V2.
- Patrons de test :
  - `Tests/EditMode/Story59ParameterizedDriverModelTests.cs:46-80,245-290,459-525` -- cas noyau V1-B01/B02/B03/B06/F01, a rejouer contre l'arbitrage ;
  - `Story532PerceptionTests.cs` -- poses, frames et modeles mutes ;
  - `Story530PlanningSpineTests.cs:253-263` -- scan des noms interdits.
- `_bmad-output/implementation-artifacts/deferred-work.md:510-514` -- branchement de la perception et harness, rattaches a la 5.33. `:508` -- dette de cout 5.31, dont la condition de reouverture est atteinte.
- `_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/campaign-5-31.json` -- routes de reference pour la disponibilite des canaux.

## Tasks & Acceptance

**Execution** -- quatre phases internes. Chaque phase se termine par un checkpoint : validation de la phase verte, sinon HALT. Aucune nouvelle story.

*Phase 0 -- prealables*
- [x] Verifier que la Story 5.52 est `done` ; sinon HALT (D8).
- [x] `_bmad-output/planning-artifacts/epics.md` -- amender la Story 5.33 (index et fiche) : complexite L, harness et PlayMode au niveau story, verification `Both` dans la table des revues. Citer la decision proprietaire du 2026-10-02 -- l'Epic ne diverge pas silencieusement de la spec.

*Phase 1 -- noyau pur (EditMode)*
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/SpeedPlan.cs` -- `RoadLimit` applique, valeurs publiees (`Applied(v)`, `Unauthored`), `DeferredLimits` desormais vide ; nouvelles valeurs `SpeedConstraint` (`RoadLimit`, `LeaderFollowing`, `Obstacle`, `PerceptionUnavailable`) par ajout -- fermer la limite reportee par la 5.31.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs` (nouveau, pur) -- candidats, minimum, ordre d'egalite, liante et rejetes, raisons de `PerceptionUnavailable`, reprise lissee (D5) -- une seule regle de suivi et d'obstacle.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs` -- `Track` accepte une decision longitudinale optionnelle ; sans elle, comportement 5.31 au bit pres -- compatibilite.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Blockers/Blocker.cs`, `BlockerTracker.cs` (nouveaux, purs) -- records, table des genres, `SinceFrame`, dominant -- ensemble de blockers pour 5.34/5.39.
- Checkpoint 1 : `Story533` EditMode (parties pures) et `Story531` EditMode verts.

*Phase 2 -- branchement runtime, un seul vehicule*
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs` (nouveau) -- requetes bornees, classification, identite, diagnostics -- dangers de la frame.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs` (nouveau) -- epoque globale, frame unique, pas ordonnes -- frame partagee.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- plus d'auto-cadence ; entree d'acteur avec empreinte ; pas sur frame fournie : perception, arbitrage, blockers, projection ; champs de trace ajoutes -- premier consommateur.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` -- constantes runtime, `TrafficV2Scenario` et sa garde, `Request` etendu par parametre optionnel, exclusivite -- scenarios rejouables.
- [x] `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` -- ordre retrait, pas, insertion ; calendrier du scenario ; population du scenario -- plusieurs vehicules V2.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- partie observation, arbitrage et blockers par ajout -- debug.
- Checkpoint 2 (D4) : `Story531` PlayMode puis `Story552` PlayMode, sans modification. Saturation du collecteur mesuree sur le jalon. Rouge ou saturation en route libre : HALT.

*Phase 3 -- scenarios d'acceptation et phase 4 -- campagne exploratoire* : fixtures ci-dessous. Checkpoint 3 : `Story533` PlayMode vert. Checkpoint 4 : invariants de la campagne verts, rapports publies, aucune anomalie de cout au sens de D7.
- [x] `Assets/RoadRage/Tests/EditMode/Story533LongitudinalTests.cs` `[Core][Story533]` -- couvrir :
  - la matrice ;
  - les cas noyau 5.9 V1-B01, B02 et B03 rejoues contre l'arbitrage, aux memes nombres ; V1-B06 rejoue contre la reprise lissee (D5) : convergence sans depassement, temps de reaction quasi instantane, nul ou negatif borne et fini ; aucune restriction jamais retardee ;
  - une simulation longitudinale point-masse sur un corridor `MVP_Run`, jeu mesure par la perception depuis une frame reelle : convergence vers s0 a 0,05 m pres en au plus 60 s simulees, jamais sous s0 − 0,05 m ;
  - le determinisme de l'arbitrage, l'ordre d'egalite et l'absence d'etat ;
  - les blockers (coexistence, `SinceFrame`, reinitialisation, dominant, aucune vitesse basse) ;
  - la limite de route sur modeles mutes (corridor, mouvement, 0) ;
  - `ToText()` de la projection etendue : deterministe, invariant de culture ;
  - le scan de `Planning/` et `Blockers/` : liste 5.30 plus `Physics.`, et aucun membre public declare dont le nom contient `Brake`, `Throttle` ou `Pedal`.
- [x] `Assets/RoadRage/Tests/EditMode/Story533SharedFrameTests.cs` `[Core][Story533]` -- couvrir :
  - les parties pures de l'ordonnanceur : frame unique, decisions identiques quel que soit l'ordre d'evaluation ;
  - la classification des dangers, le domaine d'identite et la deduplication : une racine V2 n'est jamais un danger, et aucun id n'est a la fois leader et obstacle dans une observation ;
  - la propagation des saturations (canal, requete spatiale, collecteur) vers `PerceptionUnavailable` ;
  - la garde du jeton de scenario ;
  - le constructeur de scenarios, deterministe ;
  - la disponibilite des canaux a emprise le long des 11 routes `campaign-5-31.json` aux poses nominales. `PerceptionUnavailable` ne doit jamais lier en route libre ; sinon HALT.
- [x] `Assets/RoadRage/Tests/EditMode/Story531SpeedPlanAndComposerTests.cs` -- ajouter `[Category("Story533")]` ; remplacer seulement les assertions de limite reportee par limite appliquee ou `Unauthored` -- l'AC 5.31 « reportee » est remplacee par la 5.33 (epics.md).
- [x] `Assets/RoadRage/Tests/PlayMode/Story533FollowingPlayModeTests.cs` `[Story533]` -- deux scenarios d'acceptation (AC ci-dessous).
- [x] `Assets/RoadRage/Tests/PlayMode/Story533ExplorationPlayModeTests.cs` `[Explicit][Category("Story533Exploration")]` -- campagne exploratoire (Design Notes) ; echec seulement sur un invariant ou une anomalie de cout D7 ; rapports bruts publies.
  - Cout par N = 2, 4, 8, publie separement : frame (collecteur compris), perception, planning et horizon (spine), plan de vitesse et arbitrage, composition, total.
- [x] `_bmad-output/implementation-artifacts/deferred-work.md`, `sprint-status.yaml` -- solder les entrees 510-514 ; consigner la reouverture de la dette 508 et les constats ; statut de la story. Puis `graphify update .`.

**Acceptance Criteria:**

- Given un pas hote avec N vehicules V2 lies, when il s'execute, then une seule frame est construite, chaque vehicule recoit exactement un pas et un intent, toutes les decisions citent le meme `FrameId`, et aucune ne lit une autre frame.
- Given un leader sur l'horizon, when le plan est produit, then `LeaderFollowing` est une contrainte nommee parmi `DesiredSpeed`, `RoadLimit`, `CurveLimit` et `SteeringCeiling` ; la liante et les rejetes sont publies ; rien n'ecrit frein ou gaz hors du composeur.
- Given les cas noyau de la Story 5.9, when ils sont rejoues contre l'arbitrage V2, then V1-B01, B02, B03 et B06 passent aux memes nombres (B06 sur la reprise lissee, D5) ; les fixtures `Story59` restent inchangees.
- Given un vehicule arrete derriere un leader, ou immobilise par sa politique, when sa progression est decrite, then l'attente est un blocker legitime avec source, attente de liberation, recouvrabilite et frame de debut ; plusieurs causes restent un ensemble, avec au plus un dominant.
- Given le scenario d'acceptation A (3 vehicules, meme route, obstacle cinematique place par le test sur un corridor droit commun, a au moins 3 × (longueur + s0) + 10 m du portail d'entree), when il s'execute, then :
  - les trois s'arretent : le premier a [s0 − 0,10 ; s0 + 0,50] m de l'obstacle, et chaque suiveur au meme intervalle de jeu percu ;
  - les blockers sont legitimes ;
  - apres retrait de l'obstacle, la file se resorbe et chacun atteint sa sortie ;
  - aucun contact de caisse entre vehicules ou avec l'obstacle, aucun `TrackingToleranceExceeded`, aucun repli hors `ExitPortalReached` ;
  - joueur hote absent de tout fait leader ou obstacle, a chaque pas ;
  - aucun retrait hors portail.
- Given le scenario d'acceptation B (2 vehicules, meme route, insertion decalee), when il s'execute, then le jeu percu du suiveur ne descend jamais sous s0 − 0,10 m ; aucun contact, aucun `TrackingToleranceExceeded`, v ≤ v* a chaque pas, et les deux atteignent leur sortie.
- Given la composition `V2Slice` sans scenario, when `Story531` et `Story552` PlayMode sont rejoues sans modification, then ils restent verts (non-regression de la Gate B).
- Given une contrainte qui se resserre (leader qui ralentit, jeu qui diminue, obstacle qui apparait, limite qui baisse), when la frame la montre, then la cible plus basse s'applique au meme pas ; seule une reprise apres contrainte d'interaction est lissee.
- Given une saturation d'un canal, de la requete spatiale ou du collecteur pour un vehicule, when il decide, then `PerceptionUnavailable` est evalue avec la raison publiee ; jamais une perception complete supposee.
- Given la campagne exploratoire, when elle s'execute, then les traces brutes, les constats et le cout par etape en fonction de N sont publies. Seul un invariant viole la fait echouer : NaN, intent manquant ou double, frame multiple par pas, retrait hors portail, population au-dela du maximum, joueur hote dans un fait V2, anomalie de cout D7. Les contacts et interblocages aux carrefours et giratoires sont des constats pour 5.34/5.35, jamais des echecs 5.33.

### Review Findings (2026-10-03)

Review scope: implementation commit `5264b12` and closure commits through `850852c`, compared with `954eb20` (Story 5.52 review closed). Four independent layers completed: blind hunter, edge cases, verification gaps, and acceptance audit. The owner-approved D9-D15 amendments were considered. No production code was changed by this review.

- [ ] [Review][Decision][R4][medium] Resolve the outstanding D13 maximum of two FixedUpdates per rendered frame. `results-20261003-structural-optimization.md:107` explicitly records one frame with three steps for explore-4; `perf-N4-20261003-115242-summary.md` confirms it. The diagnostic only asserts a nonempty run (`Assets/RoadRage/Tests/PlayMode/Story533PerformanceDiagnosticPlayModeTests.cs:323`). The measurements therefore leave this target unmet; deciding whether isolated exceptions are acceptable requires an owner amendment. Full-population costs were separately measured in the results report, so this finding does not claim they were absent.
- [x] [Review][Patch][R1][medium] Preserve StopHold for every unavailable perception, including when its source is still visible. [`Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs:480`] `LongitudinalPerception.From` retains visible facts while reporting channel or collector saturation. The `GapOpened` and `SourceDeparted` branches release a seen source without checking that reason; only the absent-source branch guards it. At rest with an opened gap, `PerceptionUnavailable` can bind at zero acceleration, producing neither brake nor handbrake and removing the blocker. This violates D11's rule that unavailable perception never releases the hold. Guard all release branches and cover visible-source saturation in the hysteresis tests.
- [x] [Review][Patch][R2][medium] Correct the population comparison's repeated normalization. [`Assets/RoadRage/Tests/PlayMode/Story533PerformanceDiagnosticPlayModeTests.cs:549`] The comparison divides every row by configured population, including metrics already divided by actual vehicle count at lines 409-410. The committed `perf-comparison-20261003-120437.md` shows 0.747 ms per vehicle becoming 0.093 ms under `/veh N=8`. Preserve already normalized metrics; derive total per-vehicle comparisons from actual vehicle counts or a declared full-population window, rather than dividing mixed-population means by the maximum. Verify publication arithmetic with known samples.
- [x] [Review][Patch][R3][medium] Reject unrecognized nonzero CLI codes before considering blank output retryable. [`scripts/validation-cli.ps1:51`] A response `{ ExitCode = 3; Text = '' }` enters the retry branch and can be followed by an accepted completed response. The helper's policy and the workflow require every nonzero code other than 6 to block immediately. Check the code first and add the blank-output/nonzero-code combination to the existing self-tests.

Validation executed during review:

```powershell
.\scripts\validate.ps1 -Profile Story -Story 5.33 -TestMode Both
```

Raw result excerpt:

```text
VALIDATION STORY (Story533)
Tests EditMode           66/66 passes
Tests PlayMode           2/2 passes
Erreurs Console          0 depuis le curseur 2358
Etat de compilation      sain (scriptCompilationFailed=false)
Scenes ouvertes          aucune scene modifiee
OK
```

The targeted tests do not cover R1-R3. Full suites, the explicit exploratory campaign, and the performance diagnostics were not rerun in this review. The validation generated acceptance A/B traces and summaries with stamps `20261003-162416` and `20261003-162453`. Story status is `in-progress` until the review items are resolved.

### Review patch resolution (2026-10-03)

R1-R3 were applied at the owner's request:

- R1: both visible-source release branches now require available perception. Three new EditMode cases cover all unavailable reasons, leader and obstacle holds, both release conditions, blocker continuity, physical handbrake composition, and release after perception becomes available again.
- R2: comparison costs are normalized by the actual vehicle count of each measured step before averaging. Already normalized metrics retain their values. Global frame recorders have no per-vehicle estimate and publish `-`. Three non-explicit `Story533` PlayMode tests exercise the real comparison text with mixed populations, the 0.747/0.099 regression values, missing data, and invariant culture. The historical performance comparisons remain records of the original runs; their old `/veh` columns must not be used as corrected evidence. No new performance budget or D13 exception is inferred from these arithmetic tests.
- R3: unknown nonzero CLI exit codes are rejected before the blank-output retry rule. The existing helper self-test now includes silent failures, failure after transient silence, and silent code 6, checking retry and wait counts. `validate.ps1` executes those simulations before contacting Unity, so verification stays on the project-approved path.

Validation: `validate.ps1 -Profile Story -Story 5.33 -TestMode Both` passed 69/69 EditMode and 5/5 PlayMode tests; the CLI self-test passed 13/13 cases. Console errors since cursor 2448: 0. `MVP_Run` stayed clean. The raw recap is stored in `traffic-v2-5-33-explorations/review-fixes-20261003-validation.txt`; acceptance A/B reports are stamped `20261003-164149` and `20261003-164226`.

D4 non-regressions were also executed through `validate.ps1`: Story 5.31 Both passed 50/50 EditMode and 13/13 PlayMode tests (Console cursor 2562); Story 5.52 PlayMode passed 4/4 tests (cursor 2634). Both had 0 Console errors, healthy compilation, no skipped or inconclusive tests, and no dirty scene. The 5.31 fixture regenerated `traffic-v2-5-31-measurements/playmode-short-run-steps.tsv` as expected. The explicit exploration and performance campaigns and the full suites were not rerun for these patches.

`graphify update .` completed (7,379 nodes, 20,641 edges). It still reports a partial AST extraction for the unchanged `Story531DrivenReplayTests.cs` at line 112; Unity compilation and the executable tests are the validation authority. R4 remains open and the story remains `in-progress`.

### Reprise R4 autorisee (2026-10-03)

Le proprietaire demande de traiter R4 puis de fermer la story en `done` une fois corrigee. Le scope restant est R4 ; les phases initiales et R1-R3 sont deja executees. Les modifications presentes dans l'arbre sont celles de cette revue et de ses validations, autorisees dans la conversation. Ne pas refaire l'implementation initiale.

Investigation initiale : la trace locale `perf-N4-20261003-115242-frames.tsv` contient une frame 1576 a 81,669 ms, `PlayerLoop` 6,569 ms, `TrafficV2.Step` 2,271 ms, `GC.Collect` 0 ; la frame suivante 1577 compte trois pas. La trace vehicule montre trois vehicules aux pas 765-769, avant l'insertion du quatrieme. Ceci explique le contexte du pic, mais n'autorise pas a exclure cette frame du maximum D13. La sonde publie aussi des intervalles inverses tels que `765-764` : son ordre par rapport au spawner doit etre verifie avant de conclure sur l'alignement des mesures.

- [x] `Assets/RoadRage/Tests/PlayMode/Story533PerformanceDiagnosticPlayModeTests.cs` : investiguer et corriger le comptage/alignement des frames et toute cause applicative demontree du depassement. Rendre les cibles D13 bloquantes dans la campagne `Story533Perf`, avec mesures de population pleine non vides et maximum de deux pas sur l'ensemble de la fenetre explore-4/N4. Publier les depassements et leur contexte avant toute assertion. Conserver les seuils, les constantes de temps Unity, la physique, le scenario, la geometrie et la Gate A ; aucune exclusion opportuniste d'une pause Editeur ou d'une population transitoire.
- [x] `Assets/RoadRage/Tests/PlayMode/Story533PerformancePublicationTests.cs` : couvrir les regressions de mesure et la porte D13 dans la categorie `Story533` si la correction en introduit une logique testable.
- [x] Verifier par `scripts/validate.ps1` : Story 5.33 Both puis campagne explicite `Story533Perf` et campagne exploratoire requise par le spec ; conserver les sorties officielles, y compris un echec. Les non-regressions D4 deja vertes restent valides si aucun code runtime n'est modifie ; sinon les rejouer. Pas de suite complete de fin d'epic. Apres source changee, `graphify update .`.
- [x] Clore R4 avec les preuves exactes dans un rapport et dans cette spec. Si le seuil reste depasse sans correction autorisee, consigner la cause et laisser la story ouverte ; aucune exception D13 n'est approuvee. La cloture `done` demandee reste conditionnee a une preuve verte, puis a la revue de la correction.

### Correction et preuves R4 (2026-10-03)

Rapport : `traffic-v2-5-33-explorations/review-R4-20261003.md`. Delta source limite aux deux fixtures PlayMode ci-dessus ; aucun changement runtime, scene, geometrie, preuve, scenario, physique, constante Unity ou seuil. Compteur physique independant conserve ; epoques capturees aux bornes Update et verifiees contre ce compteur, sans hypothese d'ordre du spawner. Duree moteur et recorders attaches a leur frame ; bords partiels publies. La porte D13 exige des mesures non vides en population pleine et publie tous les depassements avant d'asserter.

Campagne `Story533Perf` officielle : 5/5 passes, 0 erreur Console depuis 2847, compilation saine, `MVP_Run` propre. Couts en population pleine N1/N2/N3/N4/N8 : 0.871/0.793/0.776/0.770/0.736 ms par vehicule. N4 : 1970 pas en population pleine, 3.081 ms/pas hote ; maximum physique **2** sur les **6942 frames de toute la fenetre**, transitions incluses. N8 : 920 pas en population pleine, **5.890 ms/pas hote**. Alignement physique/epoque verifie pour tous les runs. Aucune exclusion ni exception D13.

Le maximum N3 est **3**, conserve dans `perf-N3-20261003-171753-summary.md` : frame 3079, epoques 1608-1610, apres une frame de 41.028 ms (`PlayerLoop` 39.685 ms, Traffic V2 2.310 ms, physique 0.063 ms, GC 0). Le poste restant du PlayerLoop n'est pas attribue a une cause precise ; la cible de deux pas porte sur N4. Les depassements historiques ne sont ni effaces ni declares faux par la correction d'alignement. Une future execution N4 qui depasserait deux pas echouera.

Validation finale `Story533 Both` : **69/69 EditMode et 8/8 PlayMode**, 0 erreur Console depuis 2984, compilation saine, scene propre ; aucun edit source concurrent. Cette preuve remplace le premier run vert (curseur 2721) pendant lequel une petite correction de diagnostic avait encore ete ecrite durant EditMode. Les campagnes finales et cette validation n'ont aucun edit source concurrent. D4 demeure valide, puisque R4 ne change aucun runtime.

Campagne `Story533Exploration` officielle : **5/5 passes**, 0 erreur Console depuis 3071, compilation saine, scene propre. 2/2 sorties et rejeu a 0.001 m d'ecart maximal ; file de quatre resorbee, 4/4 sorties ; N8 : 5/8 sorties, trois vehicules bloques apres contact de carrefour, constats pre-5.34/5.35 conserves ; poussee : contamination au pas 705 puis arret du scenario au pas 855. Ces constats ne sont pas des preuves d'acceptation ni une Gate C.

Sorties officielles conservees : `r4-story-validation-20261003.txt`, `r4-perf-validation-20261003.txt`, `r4-exploration-validation-20261003.txt`, dans le dossier du rapport. Suites completes non executees (fin d'epic uniquement). La revue R4 et la cloture finale sont encore a effectuer par le workflow parent.

`graphify update .` termine : 7388 noeuds, 20667 liens, 226 communautes ; avertissement AST partiel preexistant sur `Story531DrivenReplayTests.cs:112` inchange. Perimetre conserve, seules les fins de ligne des quatre JSON/rapports generes normalisees en LF. Dernier controle des scenes : `MVP_Run` chargee, active, `isDirty=false`.

### Revue finale du delta R4

Trois couches executees en parallele sur le seul delta R4 sauvegarde avant correction : risk-scaled, edge-case et verification-gap. Le changement concerne le cycle de vie et la publication d'une mesure ; aucune frontiere reseau n'est deplacee. Le diff initial de la story et R1-R3 ne sont pas empiles avec une nouvelle revue (AD-10).

Triage : quatre corrections locales `patch`, sans changement de spec ni exception D13 : duree provisoire du dernier intervalle en attente ; denominateur de recorder incluant des pas non mesures ; comparaison N8 non ecrite si l'assertion D13 echoue ; absence de test de l'assemblage reel et du flush terminal. Les assertions et les campagnes deja vertes restent consignees ; la preuve finale du diagnostic sera mise a jour apres ces correctifs.

### Preuve finale post-revue et cloture (2026-10-03)

Source figee a 17:45 (fixtures PlayMode de diagnostic et de publication) ; aucun edit source pendant les validations suivantes. `validate.ps1 -Profile Story -Story 5.33 -TestMode Both` : **69/69 EditMode, 11/11 PlayMode**, 0 erreur Console depuis 3223, compilation saine, scenes propres (`r4-review-patch-story-validation-20261003.txt`).

Campagne `Story533Perf` rejouee sur ce diagnostic stabilise (`r4-review-patch-perf-validation-20261003.txt`, campagne `20261003-174909`) : **5/5 passes**, 0 erreur Console depuis 3320, compilation saine, scenes propres. Tous les resumes : fin normale, alignement physique/epoque verifie, verdict vert.

| N | Pas en population pleine | ms/pas hote | ms/vehicule (cible <= 1) | Maximum physique/frame |
|---|---:|---:|---:|---:|
| 1 | 2832 | 0.876 | 0.876 | 2 |
| 2 | 2554 | 1.624 | 0.812 | 4 |
| 3 | 2267 | 2.345 | 0.782 | 2 |
| 4 | 1970 | 3.103 | 0.776 | **2** (18 498 frames, aucun bord ni transition exclu) |
| 8 | 920 | 5.868 (cible < 10) | 0.734 | 2 |

La cible D13 de deux pas porte sur explore-4/N4 : atteinte. Le maximum N2 de 4 est publie, non exclu : frame 5437, pas 2791-2794, frame precedente de 69.13 ms avec Traffic V2 a 1.035 ms et GC 0, donc un rattrapage apres une pause du moteur sans lien avec le cout applicatif ; il ne concerne pas la cible N4. Le N3 historique a 3 (run 171753) reste consigne. Aucune exception D13 n'est approuvee ni invoquee. Les campagnes exploratoire (5/5) et les non-regressions D4 restent applicables : le delta apres elles ne touche que le diagnostic et ses tests. Aucune affirmation de reproductibilite generale ni de Gate C. Suites completes : fin d'epic. Story cloturee `done` sur demande du proprietaire.

## Spec Change Log

- **2026-10-02 -- implementation, mesure EditMode avant le checkpoint 2, decision proprietaire D9 (capacite du collecteur).** Le risque connu des Design Notes s'est realise. Mesure par la fixture `Story533SharedFrameTests.TheStaticDecorOfMvpRunLeavesRoomInTheCollectorBufferForAFullScenario` : le long des 11 routes de `campaign-5-31.json`, pas de 2 m, rayon 40 m.
  - 809 requetes saturees sur 1 538 avec la capacite 64.
  - Tampon non borne : maximum 98 colliders, dont 97 statiques, pres du carrefour central ; mediane 65, p90 90.
  - A rayon reduit : 58 a 25 m, 75 a 30 m, 86 a 35 m.
  - Avec 64, un vehicule seul aurait lie `PerceptionUnavailable` (`HazardCollectorSaturated`) sur plus de la moitie du reseau : HALT prevu.
  - **Decision D9 : `HazardQueryCapacity` = 256, rayon 40 m inchange.** La valeur « tampon 64 » du bloc fige est remplacee par cette decision. La garde EditMode reste : maximum statique + 8 vehicules < capacite.
- **2026-10-02 -- checkpoint 2, rejeu Story552 PlayMode, decision proprietaire D10 (budget d'entree de la fixture 5.52).** Deux runs sur Editeur redemarre, meme echec : `AnActualToleranceExceedanceHoldsTheDriverAcrossPushesAndItsExit`, premier test du run, reste sur `MainMenuLobby`.
  - Cause lue en Console : le premier `CreateLobbyAsync` Steam, juste apres l'initialisation du client par `Bootstrap`, ne repond pas en 300 frames (~2,8 s). L'appel suivant repond en 0,25 s. Aucun code trafic n'a tourne.
  - Les 3 autres tests 552 sont verts : jalon Gate B 936 pas, d max 0,1993 m, borne M max 0,2003 m, v/v* max 0,344, 0 contact ; reponse 2a arret au pas 95.
  - **Decision D10 : `Story552MilestonePlayModeTests.EnterMvpRun` attend 30 s en temps reel au lieu de 300 frames.** Aucune assertion de mesure n'est modifiee ; D4 reste entier pour les seuils. Le harnais 5.33 suit la meme regle. La fixture 5.31 (`Story531V2VerticalSlicePlayModeTests`, 300 frames, verte ce jour) est laissee intacte et consignee dans `deferred-work.md`.
- **2026-10-02 -- checkpoint 3, scenario A rouge, decision proprietaire D11 (maintien a l'arret) et politique pre-5.39.** Run `acceptance-A-20261002-182834` : 1 sortie sur 3, vehicules 2 et 3 en `TrackingToleranceExceeded` (frames 1109 et 1468), d = 0,3407 et 0,340 m.
  - Cause diagnostiquee sur la trace : l'IDM n'arrete jamais franchement derriere une cause arretee. A l'arret, jeu > s0, son candidat est > 0 : aucun blocker, et le vehicule rampe vers s0 (0,35 a 0,84 m a 0,02-0,5 m/s, braquage jusqu'a -1). Sous 0,5 m/s, `VehicleTireModel.ResolveLowSpeedRamp` attenue l'adherence laterale : dans la courbe en S de la file, d derive jusqu'a epsilon_t et le verrou 2a (5.52) coupe perception et arbitrage pour toujours. Aucun leader fantome, jeu fige, blocker memorise ni PerceptionUnavailable.
  - epsilon_t reste 0,34 m : l'allocation Gate A n'est pas modifiee pour masquer ce comportement (a_e dimensionne aussi les degagements de carrefour).
  - **D11 -- StopHold.** L'IDM conduit l'approche. Quasi arrete (v <= 0,5 m/s = `VehicleTireModel.SlipReferenceSpeed`) avec une liante `LeaderFollowing` ou `Obstacle` dont le jeu vaut au plus s0 + Delta_hold, le vehicule passe en `StopHold` : candidat nomme par sa cause et sa source, acceleration min(candidat de la source, -b), arret au frein a main a 0 m/s, etat porte par `LongitudinalMemory`. Liberation par hysteresis : source disparue d'une perception disponible (une perception indisponible ne libere jamais), jeu >= s0 + Delta_release, ou source repartie (vitesse >= 0,5 m/s et jeu > s0 + Delta_hold) ; un frisson du leader ne libere pas. La reprise lissee D5 part de 0, non de -b. Le maintien amende l'arbitrage du bloc fige ; 5.31 au bit pres sans interaction reste vrai.
  - **Calibration (banc EditMode, `stophold-calibration.md`)** avec la roue libre mesuree de la bande de service (2 m/s2 sous 0,41 m/s) : IDM seul, 0,39-0,40 m de rampement ; Delta_hold 0,25 m, 0,15 m ; **Delta_hold 0,5 m, 0 m**, jeu maintenu 2,447-2,452 m, liberation 0,34 s apres le depart du leader, aucune re-entree. Source a 0,05 m/s pendant 60 s : 4, 2 et 1 liberations pour Delta_release 1 ; 1,5 ; 2 m. **Retenu : Delta_hold 0,5 m, Delta_release 2 m** (`TrafficV2Settings.StopHold`).
  - **Semantique des blockers revisee :** un blocker est une cause reelle d'immobilisation, la source d'un StopHold et les autres faits qui le tiendraient seuls (jeu <= s0 + Delta_hold), ou l'immobilisation de politique. Un candidat <= 0 en roulant n'en est plus un (le vehicule 3 portait `Leader:02` a 3,72 m/s, frame 782). Remplace la regle « candidat <= 0 » du bloc fige.
  - **Rejeu D11 (`acceptance-A-20261002-214649`) et critere de rampement precise (decision proprietaire du 2026-10-02).** Aucun `TrackingToleranceExceeded`, d max 0,1995 / 0,1937 / 0,1874 m, 3 sorties sur 3, StopHold a 2,497-2,499 m puis liberations en cascade (SourceGone fr 1073, SourceDeparted fr 1107 et 1175). Le critere « acceleration > 0 sous 0,5 m/s derriere une source arretee » comptait 0,095 m de fin d'approche du vehicule 2 (19 pas a 0,25 m/s, sans arret, d +0,009 m). Rampement redefini comme ce mouvement APRES un arret (v < 0,05 m/s) derriere une cause d'interaction, jusqu'a une liberation explicite ; seuil 0,05 m inchange ; fin d'approche publiee a part. Applique aux traces : run fautif 0,31 / 0,23 / 0,44 m (echec), rejeu D11 0 / 0 / 0 m.
  - **Campagne du 2026-10-02 22:00 et decision proprietaire D12 (verrou pre-5.34).** explore-2, rejeu (ecart 0 m sur 3 017 pas-vehicule), explore-4 (file de 4 en StopHold, liberation en cascade, 0 rampement apres arret) et explore-poussee (contamination au pas 705, arret au pas 855) verts. explore-8 rouge : au pas 1846 le vehicule 1, libere du maintien derriere l'obstacle « sortie », traverse le carrefour a 6,9 m/s sur le mouvement `40ca7f10` pendant que le vehicule 8 arrive sur `4e437f94` ; les deux sont membres de la zone de conflit `ConflictZones[23]` (controles distincts) et aucun ne voit l'autre dans son couloir avant le chevauchement ; impulsion 1 230 N.s, d 0,004 -> 0,345 m en 10 pas, verrou 2a au pas 1857. Cout N = 8 : 4,723 ms (1,44 x N = 2), dans D7. **D12 :** dans les campagnes nominales, un verrou est un constat pre-5.34 (fenetre fonctionnelle close, D7 juge sur la fenetre propre) seulement si le vehicule verrouille a eu, au plus 50 pas avant, un contact de caisse avec un autre vehicule V2 et si, a ce pas, les deux etaient sur deux mouvements distincts d'une meme zone de conflit. Tout autre verrou nominal reste un echec ; A et B restent stricts. Regle a retirer par la 5.34.
  - **Diagnostic de performance et decision proprietaire D13 (2026-10-02).** Fixture `Story533Perf` (explore-4 tronque a N = 1..4, conditions d'acceptation) : pas Traffic V2 moyen 4,36 / 7,42 / 10,06 / 13,19 ms, marginal ~3 ms par vehicule ; en population pleine a N = 4, 19,4 a 21,3 ms par pas hote, au-dela du pas fixe de 20 ms, d'ou jusqu'a 17 FixedUpdate par frame et des frames de 350 a 427 ms (gel de la video). Postes : occupation de la frame (`TryOccupy` projette ~130 points sur toute la courbe de l'element), SpeedPlan et horizon reconstruits sur toute la route restante a chaque pas (dette 508), perception ; GC gen0 ~1 collection par seconde et par vehicule. Ni quadratique, ni harnais, ni traitement periodique. Analyse : `traffic-v2-5-33-explorations/analysis-20261002-performance-diagnostic.md`. Le seuil relatif D7 passe mais le budget absolu est depasse : dette 508 bloquante. **D13 : optimiser dans la 5.33, avant cloture et avant la campagne complete** -- occupation fenetree, horizon et plan de vitesse incrementaux sans changement de comportement (preuve au bit pres en EditMode, 5.31/5.52/5.33 PlayMode inchanges), puis allocations. Cible : <= 1 ms par vehicule en population pleine, N = 8 sous 10 ms par pas hote, au plus 2 FixedUpdate par frame sur explore-4 a N = 4, mesures par `Story533Perf`.
  - **Banc de planification et decision proprietaire D14 (2026-10-02).** Un horizon incremental exact est impossible (e de depart change a chaque pas, transport non lineaire). Banc EditMode `Story533PlanningCostBenchTests` (1 335 poses, 11 routes) : horizon + plan 3,52 ms par vehicule et par pas ; exact optimise 2,73 ms (1,29x, 0 difference au bit pres) ; exact + portee bornee 0,37 ms (9,6x, 7 838 commandes identiques, 0 refus) ; verification du profil 1,94 ms sur 2,02 ; occupation 1,23 -> 0,70 ms par projection elaguee exacte (0 difference sur 176 220). **D14 : planification bornee a H = d1 + v_ref^2 / (2 b_plan) + 1 m** (d1 : premier noeud du plan au-dela de la previsualisation, v_ref = max(v desiree, v courante), b_plan = deceleration de confort x 0,99) ; **perception laissee sur toute la route restante** (un leader percu seulement a H imposerait -2,2 a -4,7 m/s2), servie par des caches par element sans construire les points ; optimisations exactes (occupation, horizon, verification). Changements publics : horizon et plan publies jusqu'a H, `HorizonTruncated`, `HorizonTerminalStop` au dernier noeud, defaut de geometrie au-dela de H detecte plus tard ; commande, liantes nommees et perception inchangees, a prouver au bit pres (EditMode) puis 5.31/5.52/5.33 PlayMode inchanges et `Story533Perf` contre la cible. Proposition : `traffic-v2-5-33-explorations/proposal-20261002-bounded-planning-horizon.md`.
  - **Diagnostic rond-point contre ligne droite et decision proprietaire D15 (2026-10-03).**
    - Constat : la croissance quadratique venait du seul canal obstacles de la perception. La densite de la geometrie
      signee des mouvements de giratoire (17,6 echantillons/m) etait parcourue a 50 Hz par toutes les projections lineaires
      de `RoadCurve`, et la localisation balayait les 116 elements.
    - Optimisation structurelle exacte, sans aucune perte de qualite (`proposal-20261003-structural-optimization.md`) :
      - projection acceleree au bit pres : plage par dichotomie, amorce, elagage par blocs ; projection compacte et amorce
        chainee pour l'occupation ;
      - perception d'obstacles : rejet prouve par etendue (angle corde / tangente), projections memoisees dans la frame,
        index des acteurs non mesures ;
      - verification du profil par curseurs ;
      - index spatial de localisation par modele.
    - Geometrie signee, Gate A, portees, footprints et D14 inchanges.
    - Preuves EditMode : 0 difference sur 55 680 projections, 876 observations d'obstacles (4 402 faits, 465 saturees) et
      2 918 points de localisation.
    - Banc (`results-20261003-structural-optimization.md`) : rond-point N = 8 de 17,75 a 5,68 ms, ligne droite N = 8 de
      10,33 a 3,79 ms ; pas O(N), environ 0,7 ms par vehicule en giratoire.
    - Compteurs de travail diagnostiques `TrafficV2WorkCounters` ajoutes au runtime (increments seuls).
  - **Politique pre-5.39 :** scenarios nominaux (A, B, campagnes 2/4/8), aucune poussee et tout `TrackingToleranceExceeded` est un echec immediat ; poussee deplacee d'explore-2 vers le scenario exploratoire separe `explore-poussee`, dont le premier verrou marque le point de contamination, clot la fenetre d'observation fonctionnelle et arrete le scenario. Le verrou 2a n'est ni rendu recuperable ni complete d'une reprise hors route : responsabilite de 5.39.

## Design Notes

**Decisions proprietaires du 2026-10-02 ([K] retenu).**
- **D1 Perimetre.** La tranche verticale reste complete, en une seule story, reclassee de M a L, avec harness et PlayMode au niveau story. C'est un amendement de `epics.md` (« none at story level »), consigne en phase 0 et non laisse en divergence silencieuse. Execution en phases internes avec checkpoints, sans nouvelle story.
- **D2 Population.** `V2SliceMaxPopulation` de production reste 1. Seul le jeton de scenario, reserve aux tests, la leve localement jusqu'a 8.
- **D3 Mode.** Comportement de production : Gate A/B actives, repli 2a actif, aucun objectif intermediaire (reserve a la mesure par le contrat §4).
- **D4 Empreinte et Gate B.** Empreinte reelle declaree partout. Le controle `OutsideWidthEnvelope` aux quatre coins devient effectif pour la premiere fois. Exception approuvee a la regle « tests de la story seulement » : `Story531` EditMode et PlayMode, puis `Story552` PlayMode, sont rejoues. Rouge : HALT, sans adapter de seuil.
- **D5 Reaction.** Restriction immediate, reprise lissee (Boundaries). Ni latence globale, ni lissage du freinage. V1-B06 : le noyau `SmoothAcceleration` est reutilise tel quel pour la reprise ; ses cas 5.9 passent contre la reprise de l'arbitrage.
  - La latence de mesure d'un pas reste. La frame N est lue apres la physique N−1, l'intent est applique au pas N, comme en 5.31. Elle n'est pas ajoutee par la 5.33.
- **D6 Limite de route.** Appliquee par le code et testee sur modeles mutes. `MVP_Run` reste non authore (0) ; ni `RoadModelVersion` ni Gate A rouverts.
- **D7 Cout.** Mesure a 2, 4 et 8 vehicules, sans optimisation en 5.33. Postes publies separement. L'optimisation reste obligatoire avant la Gate C ou toute montee significative de population.
  - Anomalie architecturale (HALT), seuils declares avant mesure :
    - cout total par vehicule a N = 2 au-dessus de 2 × la pire moyenne 5.31 (route+horizon 2,283 ms + plan de vitesse 2,66 ms) ;
    - ou cout par vehicule a N = 8 au-dessus de 2 × celui a N = 2 (croissance non lineaire).
- **D8 Ordre.** Aucune implementation avant la cloture de la revue 5.52.

**Pourquoi un arbitrage en acceleration.** Le profil 5.31 est spatial, v(s), et verifie. Un leader mobile n'est pas une borne en s. L'IDM 5.9 est la contrainte de suivi eprouvee : equilibre a l'arret = s0 exactement. On garde donc le profil pour les contraintes de route et on arbitre au niveau de l'acceleration visee, la ou la 5.31 prenait deja min(IDM, profil). Pas de module Tactical en 5.33 : le seul but implicite est « suivre la route ». Les stories 5.38 et 5.42 pourront selectionner au-dessus.

Exemple (profil par defaut) : v = 8 m/s, leader arrete a 30 m. s* = 2 + 12 + 64/(2·√3) = 32,48 m ; a = 1,5·(0 − (32,48/30)²) = −1,76 m/s². A l'arret et a 2 m : a = 0.

**Blockers, table des genres** (contestable, documentee pour 5.39) :

| Genre | Legitime | Liberation attendue | Recouvrable |
|---|---|---|---|
| `Leader` | jeu ≥ s0/2 (sinon encastrement) | oui | non si legitime, oui sinon |
| `Obstacle` mobile (joueur, vehicule, acteur) | oui | oui | non |
| `Obstacle` fixe | oui | non | oui (5.42) |
| `PolicyImmobilization` | oui | non | non |

**Canal indisponible ou tronque.** Une regle sans etat et conservatrice : pas d'acceleration. Elle couvre aussi toute saturation, puisqu'un tampon plein peut cacher le fait le plus proche du collecteur.

Traiter l'absence comme « aucun leader » accelererait vers un vehicule non vu. Un repli complet arreterait sur un simple raccord. Le test EditMode sur les routes 5.31 prouve qu'elle ne lie pas en route libre.

Risque connu : les colliders statiques du decor comptent dans le tampon du collecteur avant d'etre ecartes. Si 64 ne suffit pas dans `MVP_Run`, un vehicule seul ne pourrait plus accelerer. Le checkpoint 2 le mesure ; HALT, et capacite revisee par le proprietaire seulement.

**Explorations (campagne `[Explicit]`).** Configurations de 2, 4 et 8 vehicules. Observations publiees, jamais jugees :
- suivi ;
- formation et resorption de files ;
- vehicule arrete devant ;
- sortie occupee ;
- croisements au carrefour en croix ;
- entrees et sorties d'un giratoire ;
- vehicule pousse par une impulsion de test ;
- stabilite de la frame ;
- saturations ;
- cout CPU selon N.
Attendu, et hors perimetre : sans grants (5.34), deux vehicules qui se croisent ne se voient que comme obstacles. Les contacts et interblocages aux carrefours et giratoires sont des constats publies pour 5.34/5.35, jamais des defauts 5.33.

**Tolerances.** [s0 − 0,10 ; s0 + 0,50] m (PlayMode) et 0,05 m (EditMode) sont des parametres declares avant toute mesure, pas des resultats. Le jeu percu est conservateur (occupation 5.32), donc ≤ jeu physique.

**Reproductibilite.** Le constructeur rejoue les memes entrees : insertions, graines, pas. Les decisions sont deterministes pour une frame donnee (EditMode). La trajectoire physique entre deux executions est **mesuree** (ecart maximal publie), pas supposee identique.

**Joueur hote.** Les scenarios 5.33 le stationnent hors des routes et le verifient a chaque pas. Le jalon 5.52 rejoue sans modification ne le controle pas : s'il echoue parce que le joueur se trouve sur la route, HALT, sans modifier le test.

**Estimations non verifiees.** Cout 5.31 ≈ 2 a 5 ms par vehicule et par pas. A 8 vehicules, plus de 20 ms par pas : simulation plus lente que le temps reel, physique a pas fixe inchangee. A mesurer.

## Verification

**Commands** (Domain Reload actif depuis le 2026-10-03 : les runs PlayMode s'enchainent sans redemarrer l'Editeur, voir docs/setup/build-workflow-rules.md 3.3) :

- `.\scripts\validate.ps1 -Profile Story -Story 5.33 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = compte attendu (fixtures `Story533`, dont 5.31 adaptee), 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.33 -TestMode PlayMode` -- expected: scenarios A et B verts, 0 erreur Console, `MVP_Run` propre.
- `.\scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode EditMode` -- expected: vert. Non-regression ciblee de `SpeedPlan` et `MotionCommand` (D4).
- `.\scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode PlayMode`, puis `-Story 5.52 -TestMode PlayMode` -- expected: verts sans modification (D4, checkpoint 2).
- `.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story533Exploration -TestFilterType category -IncludeExplicit` -- expected: invariants verts, rapports ecrits ; les constats sont publies, pas juges.

## Suggested Review Order

**Frame partagee et ordonnancement**

- Point d'entree : une frame par pas hote, collecteur, puis un pas par vehicule trie par TrafficId.
  [`TrafficV2StepRunner.cs:90`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs#L90)

- Ordre hote : retrait aux portails, pas des vehicules, puis insertion ; calendrier du scenario.
  [`PortalTrafficSpawner.cs:173`](../../Assets/RoadRage/App/Run/PortalTrafficSpawner.cs#L173)

- Le driver ne s'auto-cadence plus : pas sur la frame fournie, perception, arbitrage, blockers.
  [`TrafficV2VehicleDriver.cs:572`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L572)

**Dangers physiques**

- Classification par racine : une racine V2 n'est jamais un danger, decor statique ecarte.
  [`TrafficV2HazardCollector.cs:101`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs#L101)

- Requetes bornees, saturation publiee par vehicule, identites de session.
  [`TrafficV2HazardCollector.cs:127`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs#L127)

**Arbitrage longitudinal et maintien a l'arret**

- Candidats nommes, minimum, ordre d'egalite, reprise lissee D5 ; fonction pure.
  [`LongitudinalArbitration.cs:411`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs#L411)

- StopHold D11 : liberation par hysteresis, perception indisponible ne libere jamais.
  [`LongitudinalArbitration.cs:465`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs#L465)

- Branchement perception -> arbitrage dans le pas du driver.
  [`TrafficV2VehicleDriver.cs:682`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L682)

- Commande : decision longitudinale optionnelle, 5.31 au bit pres sans elle.
  [`MotionCommand.cs:87`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs#L87)

- Limite de route appliquee comme plafond nomme ; 0 reste Unauthored.
  [`SpeedPlan.cs:329`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/SpeedPlan.cs#L329)

**Blockers**

- Ensemble par pas, SinceFrame continu ; correctif de revue pour une source de maintien non vue.
  [`BlockerTracker.cs:36`](../../Assets/RoadRage/Features/Vehicles/Traffic/Blockers/BlockerTracker.cs#L36)

**Performance D14/D15 (exacte, sans changement de comportement)**

- Planification bornee a H ; perception sur toute la route restante.
  [`TrafficV2VehicleDriver.cs:614`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L614)

- Chemin de route cache par element, sans construire les points.
  [`RoutePath.cs:42`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/RoutePath.cs#L42)

- Coeur de projection exact : plage, amorce, elagage par blocs.
  [`RoadCurve.cs:291`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadCurve.cs#L291)

- Index spatial de localisation par modele.
  [`RoadLocalization.cs:316`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs#L316)

**Composition, scenario et debug**

- Jeton de scenario reserve aux tests, exclusif avec la mesure ; production a 1.
  [`TrafficV2Composition.cs:218`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs#L218)

- Constantes runtime, capacite du collecteur D9.
  [`TrafficV2Composition.cs:74`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs#L74)

- Projection enrichie par ajout : observation, arbitrage, blockers.
  [`TrafficDecisionProjection.cs:223`](../../Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs#L223)

**Tests**

- Frame unique, decisions independantes de l'ordre d'evaluation.
  [`Story533SharedFrameTests.cs:136`](../../Assets/RoadRage/Tests/EditMode/Story533SharedFrameTests.cs#L136)

- Maintien, hysteresis et blockers sous perception aveugle.
  [`Story533LongitudinalTests.cs:1054`](../../Assets/RoadRage/Tests/EditMode/Story533LongitudinalTests.cs#L1054)

- Scenario d'acceptation A dans MVP_Run.
  [`Story533FollowingPlayModeTests.cs:60`](../../Assets/RoadRage/Tests/PlayMode/Story533FollowingPlayModeTests.cs#L60)

- Invariants par pas : un intent, une frame, epoque de decision.
  [`Story533Harness.cs:377`](../../Assets/RoadRage/Tests/PlayMode/Story533Harness.cs#L377)
