---
title: 'Story 5.31 -- Premiere tranche verticale Traffic V2 conduite, de portail a portail'
type: 'feature'
created: '2026-09-29'
status: 'draft'
review_loop_iteration: 0
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '_bmad-output/implementation-artifacts/spec-5-30-traffic-v2-planning-and-runtime-spine-foundation.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La chaine 5.30 produit route, horizon et plan geometrique, mais aucun vehicule ne conduit sous V2. Il manque la vitesse planifiee, l'intention unique et le cycle de vie de portail. La Gate B exige un vehicule IA conduit par V2 de bout en bout dans `MVP_Run`, sans casser V1.

**Approach:** Ajouter cote hote, derriere la chaine 5.30 :
- un plan de vitesse a contraintes nommees, verifie par le verificateur 5.30 ;
- une commande de suivi de la reference compilee ;
- le composeur unique d'intention, avec garde de finitude et fenetre de validite ;
- un cycle de vie V2 lie aux portails, choisi par composition dans le spawner existant.

ε_t est declare puis mesure en local. Hors runs de mesure, aucun vehicule ne conduit une trajectoire que la preuve Gate A signee ne couvre pas. La regeneration Gate A avec allocation et la re-signature sont hors de cette story (A1).

## Boundaries & Constraints

**Always:**

- **Admission.**
  - Modele : `RoadModelDocument.Load` puis `Compile`.
  - Preuve : `GateAEvidenceBinding.Bind` sur les trois textes commites, lus seulement dans l'Editeur (`UNITY_EDITOR`), avec le resultat mis en cache par modele.
  - Un build joueur, un texte absent ou une preuve perimee → aucun vehicule V2 ne conduit (`GateAEvidenceMissing` / `GateAEvidenceStale`). Un modele non declare → aucun vehicule V2 (`UndeclaredDrivabilityProfile`).
- **Composition (A4).**
  - Choix par session, `V1` (defaut de `MVP_Run`) ou `V2Slice`.
  - Il est lu une fois, avant la premiere insertion, puis fige. Tout changement ulterieur est ignore avec un diagnostic.
  - En `V2Slice` : aucun vehicule V1, au plus un vehicule V2 vivant (`V2SliceMaxPopulation = 1`), reinsertion permise apres une sortie.
  - La branche V1 du spawner garde un comportement identique.
  - Le vehicule V2 est un prefab distinct, sans controleur ni etat V1.
- **Cycle de vie.**
  - Insertion a un portail d'entree libre : point de reference au s du portail, cap tangent, vitesse nulle. Elle n'a lieu que si la premiere decision est conduisible (route planifiee, horizon conforme, plan geometriquement faisable, couverture autorisee).
  - Identite trafic `RoadId(high, low)` derivee de (graine de session, compteur d'insertion). `RouteSeed` vient de `NetworkedRunState.SessionSeed`, qui n'est jamais ecrit aujourd'hui : il vaut 0 et reste deterministe.
  - Retrait seulement quand le point de reference est localise sur le corridor du portail de sortie, avec s ≥ `Portal.SMeters`.
  - Tout echec en route laisse le vehicule en place, en `Idle`, avec un diagnostic. Jamais de retrait, teleportation ou reinsertion en route.
- **Cycle de decision, a chaque pas physique (cadence 1).**
  - Chaine : `TrafficFrame` (`FrameId` = compteur de pas physique, acteurs V2 seulement) → `PlanningSpine.Evaluate` → `SpeedPlan` → `MotionCommand` → composeur → un seul `ApplyDriveIntent`.
  - La commande porte `SourceFrameId` et une validite [p, p] en pas physiques (`PlanValiditySteps = 1`).
  - L'epoque de decision (`FrameId`) et l'epoque physique (pas) sont publiees separement.
- **Plan de vitesse (route libre).**
  - Contraintes nommees par point :
    - `DesiredSpeed` (`DriverProfile.DesiredSpeed`) ;
    - `RoadLimit` : `Deferred` (champ reporte a 5.33, `AuthoringDecisions.cs:487`), A3 ;
    - `CurveLimit` : `Deferred` (adherence 5.33, C:190), A3 ;
    - `SteeringCeiling` v*(s) : au raccord, le min des deux cotes ; `Unbounded` n'est jamais lu comme une vitesse.
  - Construction :
    - passe arriere a la borne `ComfortableDeceleration`, passe avant a `MaxAcceleration` ;
    - la contrainte liante est nommee en chaque point, et les alternatives rejetees sont conservees ;
    - l'horizon couvre toute la route restante. S'il est tronque (`LookAheadLimit`), la vitesse terminale vaut 0.
  - Tout profil genere passe `MotionPlan.VerifySpeedProfile` avec les memes bornes. Un echec → rien n'est applique, `Idle` et diagnostic.
  - Si v* est inatteignable depuis l'etat courant, la contrainte liante est `SteeringCeilingUnreachable`, avec freinage a `SafeBrakingLimit` et publication.
  - L'acceleration visee vaut min(`DriverModel.ComputeAcceleration(profil, v, 0, NoLeaderGap)`, acceleration de suivi du plan). L'enveloppe IDM 5.9 est inchangee.
- **Commande de suivi.**
  - Acceleration visee plus angle de roue vise.
  - L'angle combine une anticipation de κ au point de reference, par la geometrie du profil de conduisibilite, et une correction d'ecart lateral et de cap issus de la localisation.
  - Elle lit uniquement la reference compilee.
- **Composeur unique** (`Intent/`).
  - A chaque pas, il produit exactement un `VehicleDriveIntent` et trois scalaires d'autorite finis (`maxForwardSpeed`, `steerRateDegreesPerSecond`, `brakeTorque`, issus du `VehicleProfile`).
  - Un axe ou un scalaire non fini, ou une commande absente ou hors fenetre → `VehicleDriveIntent.Idle` avec des scalaires neutres finis (A5) et un diagnostic.
  - `ApplyDriveIntent` persiste d'un pas a l'autre : le cycle de vie resoumet a chaque pas, y compris `Idle` a la desactivation.
- **Couverture a deux niveaux.**
  - Reference : regle 5.30.
  - Vehicule :
    - exige un ε_t declare (constante unique). Tant qu'il n'est pas declare, pas de conduite (`TrackingToleranceUndeclared`). La regle 5.30 « compte 0 » ne vaut jamais autorisation de conduite ;
    - couvert ⇔ max|o| + ε_t ≤ a_e. Avec a_e = 0 et ε_t > 0, `NotCoveredByGateA` → aucune insertion ;
    - seule exception : une autorisation `MeasurementRun` construite uniquement par les tests PlayMode. Chaque decision prise sous elle est etiquetee dans la projection.
  - `LateralClearanceMarginMeters` n'entre jamais dans ce calcul.
- **Mesure (A2).** A chaque pas, publier :
  - le point de reference, s*, l'ecart lateral et l'ecart de cap ;
  - d = max, sur les 4 coins du gabarit max (`MaxVehicleHalfWidthMeters`, `MaxVehicleLengthMeters`, centre sur le point de reference, comme en 5.50), de |coin reel − coin nominal(s*)| ;
  - la vitesse observee face a v*(s*).
  - Un d max > ε_t, ou toute vitesse > v*, fait echouer le run.
- **Projection.** Etendre `TrafficDecisionProjection` avec :
  - les contraintes et la liante, les epoques, `SourceFrameId` ;
  - l'intention finale en quatre flottants, plus `Idle` et la raison de garde, sans nommer le type (garde 5.30) ;
  - la couverture vehicule et l'etiquette de mesure.
  - Hote seul : aucune `NetworkVariable`, RPC ni `OnValueChanged` ajoutes. Les clients recoivent le mouvement par `NetworkTransform` seul.
- **Cout de base.** Temps par etape (frame, localisation, route, plan, composition) par vehicule, publies dans la sortie PlayMode, a titre d'observation seulement.

**Ask First:**
- Valeur de ε_t (decision proprietaire apres le run exploratoire).
- Tout artefact ou outillage Gate A (modele, sign-off, rapport, `Traffic/Migration/**`), et la geometrie.
- Types V1 retenus (`NetworkedAIVehicleDriverController`, `NetworkedAIVehicleState`, `VehiclePhysicsBody`, `VehicleDriveIntent`, `DriverModel`).
- Gardes et tests 5.7 / 5.30, `scripts/validation-profiles.ps1`.
- Ajouter `NetworkVariable`, RPC, asmdef ou package.
- Exposer la composition dans le lobby.

**Never:**
- Un second composeur. Perception, routage, planification ou cycle de vie qui ecrivent une commande.
- Toute ecriture V2 de position, rotation, vitesse, `MoveRotation` ou `Teleport`.
- Hors perimetre : `SafetyFilter` (5.37), suivi de leader, evitement, grants, Rage/Fear, recuperation, gridlock, LOD.
- Reprendre `RecoverAtWaypoint`, `WaypointIndex`, `arrivalRadius`, l'integration en boucle ouverte ou le rappel de point vise.
- Re-ajuster la reference compilee.
- Utiliser la marge comme tolerance de suivi.
- Regenerer ou re-signer la Gate A.
- Presenter un resultat physique ou reseau sans execution Unity.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Run de mesure nominal | `V2Slice`, ε_t declare, `MeasurementRun` | Insertion a une entree, conduite, retrait a la sortie. Mesures d et v publiees | d > ε_t ou v > v* : echec |
| Hors mesure | `V2Slice`, ε_t > 0, a_e = 0 | Aucune insertion | `NotCoveredByGateA` |
| ε_t non declare | `V2Slice` | Aucune insertion | `TrackingToleranceUndeclared` |
| Preuve / modele | Build joueur, preuve perimee, modele non declare | Aucune insertion | Code nomme |
| Commande invalide | Axe ou scalaire NaN/∞, `SourceFrameId` perime, pas de plan | `Idle` avec scalaires finis | Diagnostic de garde |
| Plafond | Courbe plus serree devant, v* inatteignable | Freinage anticipe a la borne ; sinon liante `SteeringCeilingUnreachable` et freinage a `SafeBrakingLimit` | Profil refuse par le verificateur → `Idle` |
| Raccord d'anneau | Saut de v* | v ≤ min des deux cotes | Idem |
| Echec en route | `NoRoute`, horizon non conforme | Vehicule en place, `Idle` | Aucun retrait |
| Profil conducteur absent | `DriverProfileDef` nul | Vehicule inerte | Diagnostic unique, sans repli code |
| Composition | V1 par defaut ; V2 change apres insertion | V1 inchange ; choix fige | Diagnostic |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs:12,27,43,61` -- `PlanningRequest` (textes de preuve, `TrackingTolerance`, `LongitudinalBounds`, profil candidat), `Evaluate`, `PlanningDecision`. Exceptions pour une trame ou une requete invalide, codes souples sinon.
- `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs:14,51,78` -- `TrafficActorInput`, construction triee, localisation unique, `TryGetActor`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/PathHorizon.cs:11,29,50,77,108` -- `PathPoint` (v* en +∞ float, `Unbounded`), `PathInterval.Points`, `PathSeam.MinimumCeilingMetersPerSecond`, `SignedRingSeams` (code en dur, differe). Aucune API d'echantillonnage : iterer `Points`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs:16,24,37,71,107` -- `TrackingTolerance`, `LongitudinalBounds`, `SpeedProfilePoint`, couverture, `VerifySpeedProfile`. Les helpers prives `SpeedAt` / `CeilingAt` sont a :186-219.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/GateAEvidenceBinding.cs:33` -- `Bind` sur les trois textes. a_e est lu dans le bloc hache.
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:33,76` -- constructeur interne et `ToText`. Aucun point d'extension : modifier sur place.
- `Assets/RoadRage/Tests/EditMode/Story530PlanningSpineTests.cs:253-263` -- garde : `Frame/`, `Perception/`, `Planning/` et `Debug/` ne nomment jamais `VehicleDriveIntent`, `VehiclePhysicsBody`, `ApplyDriveIntent`.
- `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs:231-249,295` -- `ApplyDriveIntent(intent, maxForwardSpeed, steerRate, brakeTorque)` ne fait que memoriser les valeurs, qui persistent jusqu'au prochain appel. Aucune garde de finitude. Pas de garde hote : c'est l'appelant qui pose `isKinematic = !IsServer`. Telemetrie : `TrySampleTelemetry` :762.
- `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs:16-24` -- `Idle` = (0,0,0,0), qui roule en roue libre sans frein. Les bornes laissent passer NaN.
- `Assets/RoadRage/Features/Vehicles/VehicleSteeringModel.cs:39` -- angle vise = f(`Steer`, vitesse, verrou 40°→16°). Doit etre coherent avec `DrivabilityProfile`.
- `Assets/RoadRage/Features/Vehicles/DriverModel.cs:19,61` -- `NoLeaderGap`, `ComputeAcceleration`. Profil par defaut : v0 = 8, a = 1,5, b = 2, `SafeBrakingLimit` = 4.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs:251,307,724-798,1092-1109` -- precedents V1 a lire seulement : kinematique client, garde `IsServer`, conversion pedale et capacite, soumission. Ne pas le modifier.
- `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs:36,40,90,171,256-357` -- population, insertion, retrait. Le controleur V1 y est code en dur a :269 et :335.
- `Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab` -- modele des composants (NetworkObject, NetworkTransform serveur avec interpolation, Rigidbody 1200 kg, `VehiclePhysicsBody` + `VehicleProfileDef_Default`). Il est enregistre dans `DefaultNetworkPrefabs.asset:28`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs:63,487` -- limite de vitesse reportee a la 5.33, d'ou toutes les limites a 0 dans le modele.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs:234-236` et `JunctionClearance.cs:110,115` -- gonflement = marge + δ_c. a_e n'y entre pas (voir Design Notes).
- `Assets/RoadRage/Tests/PlayMode/Story510RoutedTrafficPlayModeTests.cs:104-307,384-406` -- gabarit du jalon (bootstrap → lobby → `MVP_Run`, `Inconclusive`, premiere apparition et disparition).
- `Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs:47,91-139,158-172,225-282` -- NetworkVariables IA figees, aucun `OnValueChanged`, cablage du spawner.
- `Assets/RoadRage/Tests/EditMode/Story59ParameterizedDriverModelTests.cs:46-67` et `Story514AiDrivesByIntentTests.cs:55,104-147` -- enveloppe IDM, meme couche physique et meme profil.
- `_bmad-output/implementation-artifacts/v1-regression-5-51/after-playmode-results.json` -- reference PlayMode (45 tests, 39 reussis, 6 echecs connus).

## Tasks & Acceptance

**Execution:**
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/SpeedPlan.cs` -- contraintes nommees, passes arriere et avant, liante et rejetees, verification 5.30.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs` -- acceleration et angle vises, `SourceFrameId`, validite, sans nommer les types physiques.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs` -- composeur unique, garde de finitude et de fenetre, `Idle`.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` -- composition, population, ε_t declare, jeton `MeasurementRun`, fournisseur de preuve Editeur mis en cache.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- `NetworkBehaviour` hote seul (kinematique cote client) : cycle par pas, retrait, enregistreur de mesure, chronometres.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- lignes ajoutees.
- [ ] `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` -- branche `V2Slice`. Branche V1 intacte.
- [ ] `Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab`, `DefaultNetworkPrefabs.asset`, `MVP_Run.unity` -- prefab V2 reference par le spawner, composition V1 par defaut. En local, avec la double garde AD-6.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531SpeedPlanAndComposerTests.cs` `[Category("Core")]` -- couvrir :
  - la matrice ;
  - un intent par pas ;
  - `Idle` sur chaque axe et chaque scalaire NaN/∞ et sur une fenetre perimee ;
  - la verification 5.30 sur chaque profil genere ;
  - la contrainte liante et les rejetees ;
  - `SteeringCeilingUnreachable` ;
  - les raccords d'anneau ;
  - l'enveloppe IDM.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531LifecycleAndCompositionTests.cs` `[Category("Core")]` -- couvrir :
  - les refus (couverture, ε_t non declare, preuve, modele) ;
  - identite et graine deterministes ;
  - retrait uniquement a s ≥ portail ;
  - scans : seul le composeur construit l'intent ; seul le driver V2 appelle `ApplyDriveIntent` ; aucune ecriture Rigidbody ou `Teleport` en V2 ; aucun jeton `MeasurementRun` hors tests ; prefab V2 sans type V1 ; gardes 5.7 et 5.30 intacts.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531DrivenReplayTests.cs` `[Category("Geometry")]` -- rejeu cinematique de toute la chaine (plan de vitesse et composeur compris) de chaque entree vers une sortie de `MVP_Run`, v ≤ v* partout.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story531V2VerticalSlicePlayModeTests.cs` -- trois tests :
  - jalon 1 sous `MeasurementRun`, sortie brute et mesures publiees ;
  - refus hors mesure ;
  - modele non declare.
- [ ] `_bmad-output/implementation-artifacts/deferred-work.md`, `sprint-status.yaml` -- statut, resolution des reports 5.30 traites. Puis `graphify update .`.

**Acceptance Criteria:**
- Given `V2Slice`, ε_t declare et `MeasurementRun`, when le jalon PlayMode s'execute dans `MVP_Run`, then un seul vehicule V2 s'insere a une entree, conduit sa route et se retire a une sortie, sans apparition, disparition ni teleportation en route, sans controleur V1, avec d ≤ ε_t et v ≤ v*(s) a chaque pas publie.
- Given la composition `V1` par defaut, when la suite PlayMode complete s'execute, then les resultats V1 sont identiques a la reference (6 echecs connus, aucun nouveau).
- Given a_e = 0 signe, when un vehicule V2 est demande hors `MeasurementRun`, then aucune insertion n'a lieu et la raison est publiee.
- Given un pas physique, when la chaine s'execute, then exactement un intent fini (ou `Idle`) est soumis, et aucun autre composant n'ecrit de commande ni de mouvement.
- Given une decision, when sa projection est inspectee, then elle porte les contraintes, la liante, les epoques, l'intent final, la couverture vehicule et l'etiquette de mesure. Les clients n'ont aucun chemin de synchronisation supplementaire.

## Spec Change Log

## Design Notes

**Couverture de la reference ≠ erreur de suivi.**
- *Faits verifies.*
  - La preuve signee couvre la reference compilee, gonflee de marge + δ_c, avec a_e = 0 lie par `ClearanceHash`.
  - a_e n'entre dans aucun calcul de degagement : il n'apparait que dans le texte du rapport (`AuthoredRoadModel.cs:1313,1397`).
  - La marge de 0,25 m est reservee (C:415).
- *Consequence.* Un vehicule physique a ε_t > 0 n'est pas couvert. Le contrat (C:417-419, E:2698-2700) exige alors :
  - d'ajouter l'allocation au gonflement de l'outillage Gate A ;
  - de regenerer candidats et degagements (physique et Sidewalk) ;
  - la decision du proprietaire sur chaque paire ;
  - une re-signature, l'ancien enregistrement etant conserve.
  Rien de cela n'existe dans le code. La 5.31, telle qu'`epics.md` l'ecrit, ne peut donc pas atteindre la Gate B sans ce chantier (A1).
- *Estimation non verifiee.* Residu signe minimal ≈ 0,1127 m. Un ε_t proche ou au-dela rendrait probablement un residu non positif, donc un echec geometrique et non un simple re-signe.

**Pourquoi mesurer d aux coins et non l'ecart lateral seul.** La preuve balaie un gabarit rigide centre sur le point de reference. Un ecart de cap Δψ deplace les coins d'environ (L/2)·|sin Δψ| meme quand l'ecart lateral est nul. Seul un ε_t qui borne le deplacement de chaque point du gabarit rend le gonflement de Minkowski valide.

**Etudie dans le code (Cloud, 2026-09-29) :**
- API 5.29 / 5.30 finales ;
- semantique de persistance de `ApplyDriveIntent` ;
- absence de garde de finitude ;
- `Idle` = roue libre ;
- limites de vitesse reportees ;
- `SessionSeed` jamais ecrit ;
- couplage V1 du spawner ;
- gardes 5.7 et 5.30 ;
- 6 echecs PlayMode connus.

**A mesurer en local, sans resultat invente ici :**
- ε_t observe ;
- d et v face a v* ;
- comportement aux 24 raccords d'anneau (saut de κ, taux de braquage de 300°/s) ;
- temps par etape ;
- non-regression PlayMode V1.

**Protocole de mesure local :**
1. Run exploratoire sous `MeasurementRun`, qui ne vaut pas acceptation.
2. Le proprietaire declare ε_t, avec la valeur observee et la justification (Ask First).
3. Runs d'acceptation du jalon 1.
4. Verdict de couverture publie. Avec a_e = 0 : non couvert → Gate B ouverte, en attente de la story de re-signature (A1).

**Choix de conception (contestables).**
- Borne de planification `ComfortableDeceleration`, borne d'infaisabilite `SafeBrakingLimit`.
- Cadence et validite d'un pas.
- Preuve lue dans l'Editeur seulement, le build restant ferme.
- Composition figee avant la premiere insertion.
- Report 5.30 des raccords d'anneau codes en dur maintenu jusqu'a la re-signature.

## Verification

Aucune de ces commandes n'a ete executee dans le Cloud (pas d'Unity, de PowerShell ni de Test Runner).

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story531SpeedPlanAndComposerTests"` -- expected: vert, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Full` -- expected: EditMode complet vert.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- expected: suite complete. Les tests 5.31 sont verts ; les 6 echecs connus sont identiques, aucun nouveau. La sortie brute est consignee.
