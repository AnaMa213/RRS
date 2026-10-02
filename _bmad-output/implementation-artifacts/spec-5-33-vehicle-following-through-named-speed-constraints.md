---
title: 'Story 5.33 -- Suivi de vehicule par contraintes de vitesse nommees : premier consommateur runtime de la perception et premieres explorations multi-vehicules V2 dans MVP_Run'
type: 'feature'
created: '2026-10-02'
status: 'draft'
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
- [ ] Verifier que la Story 5.52 est `done` ; sinon HALT (D8).
- [ ] `_bmad-output/planning-artifacts/epics.md` -- amender la Story 5.33 (index et fiche) : complexite L, harness et PlayMode au niveau story, verification `Both` dans la table des revues. Citer la decision proprietaire du 2026-10-02 -- l'Epic ne diverge pas silencieusement de la spec.

*Phase 1 -- noyau pur (EditMode)*
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/SpeedPlan.cs` -- `RoadLimit` applique, valeurs publiees (`Applied(v)`, `Unauthored`), `DeferredLimits` desormais vide ; nouvelles valeurs `SpeedConstraint` (`RoadLimit`, `LeaderFollowing`, `Obstacle`, `PerceptionUnavailable`) par ajout -- fermer la limite reportee par la 5.31.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/LongitudinalArbitration.cs` (nouveau, pur) -- candidats, minimum, ordre d'egalite, liante et rejetes, raisons de `PerceptionUnavailable`, reprise lissee (D5) -- une seule regle de suivi et d'obstacle.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs` -- `Track` accepte une decision longitudinale optionnelle ; sans elle, comportement 5.31 au bit pres -- compatibilite.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Blockers/Blocker.cs`, `BlockerTracker.cs` (nouveaux, purs) -- records, table des genres, `SinceFrame`, dominant -- ensemble de blockers pour 5.34/5.39.
- Checkpoint 1 : `Story533` EditMode (parties pures) et `Story531` EditMode verts.

*Phase 2 -- branchement runtime, un seul vehicule*
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2HazardCollector.cs` (nouveau) -- requetes bornees, classification, identite, diagnostics -- dangers de la frame.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2StepRunner.cs` (nouveau) -- epoque globale, frame unique, pas ordonnes -- frame partagee.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- plus d'auto-cadence ; entree d'acteur avec empreinte ; pas sur frame fournie : perception, arbitrage, blockers, projection ; champs de trace ajoutes -- premier consommateur.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` -- constantes runtime, `TrafficV2Scenario` et sa garde, `Request` etendu par parametre optionnel, exclusivite -- scenarios rejouables.
- [ ] `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` -- ordre retrait, pas, insertion ; calendrier du scenario ; population du scenario -- plusieurs vehicules V2.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- partie observation, arbitrage et blockers par ajout -- debug.
- Checkpoint 2 (D4) : `Story531` PlayMode puis `Story552` PlayMode, sans modification. Saturation du collecteur mesuree sur le jalon. Rouge ou saturation en route libre : HALT.

*Phase 3 -- scenarios d'acceptation et phase 4 -- campagne exploratoire* : fixtures ci-dessous. Checkpoint 3 : `Story533` PlayMode vert. Checkpoint 4 : invariants de la campagne verts, rapports publies, aucune anomalie de cout au sens de D7.
- [ ] `Assets/RoadRage/Tests/EditMode/Story533LongitudinalTests.cs` `[Core][Story533]` -- couvrir :
  - la matrice ;
  - les cas noyau 5.9 V1-B01, B02 et B03 rejoues contre l'arbitrage, aux memes nombres ; V1-B06 rejoue contre la reprise lissee (D5) : convergence sans depassement, temps de reaction quasi instantane, nul ou negatif borne et fini ; aucune restriction jamais retardee ;
  - une simulation longitudinale point-masse sur un corridor `MVP_Run`, jeu mesure par la perception depuis une frame reelle : convergence vers s0 a 0,05 m pres en au plus 60 s simulees, jamais sous s0 − 0,05 m ;
  - le determinisme de l'arbitrage, l'ordre d'egalite et l'absence d'etat ;
  - les blockers (coexistence, `SinceFrame`, reinitialisation, dominant, aucune vitesse basse) ;
  - la limite de route sur modeles mutes (corridor, mouvement, 0) ;
  - `ToText()` de la projection etendue : deterministe, invariant de culture ;
  - le scan de `Planning/` et `Blockers/` : liste 5.30 plus `Physics.`, et aucun membre public declare dont le nom contient `Brake`, `Throttle` ou `Pedal`.
- [ ] `Assets/RoadRage/Tests/EditMode/Story533SharedFrameTests.cs` `[Core][Story533]` -- couvrir :
  - les parties pures de l'ordonnanceur : frame unique, decisions identiques quel que soit l'ordre d'evaluation ;
  - la classification des dangers, le domaine d'identite et la deduplication : une racine V2 n'est jamais un danger, et aucun id n'est a la fois leader et obstacle dans une observation ;
  - la propagation des saturations (canal, requete spatiale, collecteur) vers `PerceptionUnavailable` ;
  - la garde du jeton de scenario ;
  - le constructeur de scenarios, deterministe ;
  - la disponibilite des canaux a emprise le long des 11 routes `campaign-5-31.json` aux poses nominales. `PerceptionUnavailable` ne doit jamais lier en route libre ; sinon HALT.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531SpeedPlanAndComposerTests.cs` -- ajouter `[Category("Story533")]` ; remplacer seulement les assertions de limite reportee par limite appliquee ou `Unauthored` -- l'AC 5.31 « reportee » est remplacee par la 5.33 (epics.md).
- [ ] `Assets/RoadRage/Tests/PlayMode/Story533FollowingPlayModeTests.cs` `[Story533]` -- deux scenarios d'acceptation (AC ci-dessous).
- [ ] `Assets/RoadRage/Tests/PlayMode/Story533ExplorationPlayModeTests.cs` `[Explicit][Category("Story533Exploration")]` -- campagne exploratoire (Design Notes) ; echec seulement sur un invariant ou une anomalie de cout D7 ; rapports bruts publies.
  - Cout par N = 2, 4, 8, publie separement : frame (collecteur compris), perception, planning et horizon (spine), plan de vitesse et arbitrage, composition, total.
- [ ] `_bmad-output/implementation-artifacts/deferred-work.md`, `sprint-status.yaml` -- solder les entrees 510-514 ; consigner la reouverture de la dette 508 et les constats ; statut de la story. Puis `graphify update .`.

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

## Spec Change Log

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

**Commands** (l'Editeur doit etre redemarre avant chaque execution PlayMode : le runner ne sert qu'une fois par session) :

- `.\scripts\validate.ps1 -Profile Story -Story 5.33 -TestMode EditMode` -- expected: `VALIDATION STORY`, compte execute = compte attendu (fixtures `Story533`, dont 5.31 adaptee), 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.33 -TestMode PlayMode` -- expected: scenarios A et B verts, 0 erreur Console, `MVP_Run` propre.
- `.\scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode EditMode` -- expected: vert. Non-regression ciblee de `SpeedPlan` et `MotionCommand` (D4).
- `.\scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode PlayMode`, puis `-Story 5.52 -TestMode PlayMode` -- expected: verts sans modification (D4, checkpoint 2).
- `.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story533Exploration -TestFilterType category -IncludeExplicit` -- expected: invariants verts, rapports ecrits ; les constats sont publies, pas juges.
