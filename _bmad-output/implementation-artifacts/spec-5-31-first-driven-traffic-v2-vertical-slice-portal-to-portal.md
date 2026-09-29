---
title: 'Story 5.31 -- Premiere tranche verticale Traffic V2 conduite, de portail a portail (run de mesure)'
type: 'feature'
created: '2026-09-29'
status: 'draft'
review_loop_iteration: 0
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-29.md'
  - '_bmad-output/implementation-artifacts/spec-5-30-traffic-v2-planning-and-runtime-spine-foundation.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La chaine 5.30 produit route, horizon et plan geometrique, mais aucun vehicule ne conduit sous V2 : il n'y a ni vitesse planifiee, ni intention unique, ni cycle de vie de portail. Par ailleurs, la preuve Gate A signee (a_e = 0) ne couvre aucun vehicule physique, puisque a_e n'entre dans aucun calcul (correct-course du 2026-09-29).

**Approach:** Ajouter cote hote, derriere la chaine 5.30 :
- un plan de vitesse a contraintes nommees, verifie par le verificateur 5.30 ;
- une commande de suivi de la reference compilee ;
- le composeur unique, avec garde de finitude, fenetre de validite et freinage de repli V2 ;
- un cycle de vie de portail, choisi par composition dans le spawner existant.

La conduite n'a lieu qu'en **run de mesure explicite**. ε_t est mesure, puis declare par le proprietaire, puis verifie. Hors mesure, le refus est prouve. La Gate B n'est pas revendiquee : elle se ferme apres la Story 5.52.

## Boundaries & Constraints

**Always:**

- **Prealable.** Le correct-course du 2026-09-29 doit etre approuve avant que cette spec passe `ready-for-dev`.
- **Admission.**
  - Modele : `RoadModelDocument.Load` puis `Compile`.
  - Preuve : `GateAEvidenceBinding.Bind` sur les trois textes commites, lus seulement dans l'Editeur, avec le resultat mis en cache par modele.
  - Un build joueur, une preuve absente ou perimee, ou un modele non declare → aucun vehicule V2, avec un code nomme.
- **Composition (A4).**
  - Choix par session, `V1` (defaut de `MVP_Run`, comportement inchange) ou `V2Slice`.
  - Il est lu une fois, avant la premiere insertion, puis fige. Tout changement ulterieur est ignore avec un diagnostic.
  - En `V2Slice` : aucun vehicule IA V1, au plus un vehicule V2 vivant (`V2SliceMaxPopulation = 1`), insertions successives permises.
  - Pas de commande dans le lobby. Le vehicule V2 est un prefab distinct, sans aucun type V1.
- **Jeton `MeasurementRun`.**
  - Il n'est construit que par les tests PlayMode de cette story. Une assertion structurelle refuse toute construction hors de `Tests/`.
  - Il porte la campagne : couples entree → sortie impose et graine.
  - Sans lui, un vehicule V2 ne s'insere que si sa couverture vehicule est etablie, ce qui est impossible avec a_e = 0.
- **Cycle de vie.**
  - Insertion a un portail d'entree libre : point de reference au s du portail, cap tangent, vitesse nulle. Elle n'a lieu que si la premiere decision est conduisible.
  - Identite `RoadId(high, low)` derivee de (graine de session, compteur d'insertion). `RouteSeed` vient de `NetworkedRunState.SessionSeed`, qui n'est jamais ecrit aujourd'hui (0).
  - Retrait seulement quand le point de reference est localise sur le corridor du portail de sortie, avec s ≥ `Portal.SMeters`.
  - Tout echec en route : le vehicule reste present, recoit le freinage de repli, et un diagnostic est publie. Jamais de retrait, teleportation ou realignement force.
- **Cycle de decision, a chaque pas physique (cadence 1).**
  - Chaine : `TrafficFrame` (`FrameId` = compteur de pas, acteurs V2 seulement) → `PlanningSpine.Evaluate` → `SpeedPlan` → `MotionCommand` → composeur → un seul `ApplyDriveIntent` par pas.
  - La commande porte `SourceFrameId` et une validite [p, p] (`PlanValiditySteps = 1`).
  - Les epoques de decision et physique sont publiees separement.
- **Plan de vitesse (A3).**
  - Contraintes appliquees : `DesiredSpeed` (`DriverProfile.DesiredSpeed`), `SteeringCeiling` v*(s) (min des deux cotes au raccord, `Unbounded` jamais lu comme une vitesse), bornes longitudinales.
  - Contraintes nommees mais **non appliquees** : `RoadLimit` et `CurveLimit` (adherence), a l'etat `DeferredUnauthored`, ou `DeferredAuthored(valeur)` si une valeur non nulle est authoree. Une valeur authoree n'est jamais presentee comme une absence de limite.
  - Construction :
    - passe arriere a `ComfortableDeceleration`, passe avant a `MaxAcceleration` ;
    - la contrainte liante est nommee en chaque point, les alternatives rejetees sont conservees ;
    - l'horizon couvre toute la route restante. S'il est tronque (`LookAheadLimit`), la vitesse terminale vaut 0.
  - Tout profil passe `MotionPlan.VerifySpeedProfile` avec les memes bornes. Un echec → freinage de repli et diagnostic.
  - Si v* est inatteignable depuis l'etat courant, la contrainte liante est `SteeringCeilingUnreachable`, avec freinage a `SafeBrakingLimit`.
  - L'acceleration visee vaut min(`DriverModel.ComputeAcceleration(profil, v, 0, NoLeaderGap)`, suivi du plan). L'enveloppe IDM 5.9 est inchangee.
- **Commande de suivi.**
  - Acceleration visee plus angle de roue vise.
  - L'angle combine une anticipation de κ au point de reference, par la geometrie `DrivabilityProfile`, et une correction de l'ecart lateral et de cap.
  - Elle lit uniquement la reference compilee.
- **Composeur unique** (`Intent/`).
  - A chaque pas, il produit exactement un `VehicleDriveIntent` et trois scalaires d'autorite (`maxForwardSpeed`, `steerRateDegreesPerSecond`, `brakeTorque`, issus du `VehicleProfile`), tous finis.
- **Freinage de repli V2 (A5).**
  - Il est emis par ce seul composeur quand il n'y a pas de commande valide : absente, hors fenetre, profil refuse, axe ou scalaire non fini. Il vaut un seul pas et est reevalue au pas suivant.
  - Contenu :
    - au-dessus de `MinimumDirectionSpeed` : gaz 0, volant 0, frein de service egal a la deceleration `SafeBrakingLimit` convertie par la capacite de frein, borne [0, 1], frein a main 0 ;
    - a `MinimumDirectionSpeed` ou en dessous : aucune entree de frein, puisque `BrakeReverse` y engagerait la marche arriere. Le frein moteur `coastTorque` retient le vehicule.
  - Un scalaire de profil non fini rend le vehicule inerte, avec un diagnostic.
  - `VehicleDriveIntent.Idle` et sa semantique V1 sont inchanges. Ce repli n'est pas le `SafetyFilter` (5.37).
- **Couverture.**
  - Reference : regle 5.30.
  - Vehicule : il faut un ε_t declare (constante unique). Tant qu'il n'est pas declare, pas de conduite hors mesure (`TrackingToleranceUndeclared`). La regle 5.30 « compte 0 » ne vaut jamais autorisation.
  - Couvert ⇔ max|o| + ε_t ≤ a_e de la preuve valide. Le verdict est publie.
  - Ni `LateralClearanceMarginMeters` ni un residu 5.51 n'entrent dans ce calcul, et la 5.31 ne regenere ni ne signe rien.
- **Mesure (A2).**
  - Gabarit max : `MaxVehicleHalfWidthMeters` × `MaxVehicleLengthMeters`, centre sur le point de reference (1,55 m devant l'essieu arriere, comme la preuve 5.50/5.51). s* est la projection du point de reference reel sur la reference compilee de l'element courant ; la pose nominale est la pose de reference en s*.
  - Grandeurs :
    - d_k = max sur les 4 coins de |coin reel − coin nominal| au pas k ;
    - entre deux pas : d ≤ max(d_k, d_k+1) + δ_k/2, avec δ_k = |Δp| + ρ|Δθ| et ρ = √((L/2)² + W²), sous l'hypothese |Δθ| < 90° par pas (echec ferme sinon) ;
    - publies aussi : ecart lateral, ecart de cap, vitesse observee face a v*(s*).
  - Toute vitesse > v*, ou un d borne > ε_t declare, fait echouer le run d'acceptation.
  - Campagne deterministe : un ensemble de couples entree → sortie et de graines qui couvre **chaque mouvement et chaque raccord d'anneau au moins une fois**. Le taux de couverture est publie ; un mouvement non parcouru rend la campagne incomplete.
  - Conditions consignees : pas physique 0,02 s, Editeur en hote, profils par defaut, commit.
  - Sequence : (1) run exploratoire, qui ne vaut pas acceptation ; (2) declaration de ε_t par le proprietaire ; (3) runs d'acceptation.
- **Projection.** Etendre `TrafficDecisionProjection` avec :
  - les contraintes (appliquees et reportees) et la liante ;
  - les epoques et `SourceFrameId` ;
  - l'intention finale en quatre flottants, plus l'indicateur de repli et la raison, sans nommer le type (garde 5.30) ;
  - la couverture vehicule et l'etiquette de mesure.
  - Hote seul : aucune `NetworkVariable`, RPC ni `OnValueChanged` ajoutes ; les clients recoivent le mouvement par `NetworkTransform` seul.
- **Cout de base.** Temps par etape (frame, localisation, route, plan, composition) par vehicule, publies dans la sortie PlayMode, a titre d'observation seulement.

**Ask First:**
- La valeur de ε_t (etape 2 du protocole).
- Tout artefact ou outillage Gate A, et la geometrie.
- Types V1 retenus : `NetworkedAIVehicleDriverController`, `NetworkedAIVehicleState`, `VehiclePhysicsBody`, `VehicleDriveIntent`, `VehicleTireModel`, `DriverModel`.
- Gardes et tests 5.7 / 5.30, `validation-profiles.ps1`.
- `NetworkVariable`, RPC, asmdef ou package.
- Une commande dans le lobby.

**Never:**
- Revendiquer la Gate B.
- Regenerer ou signer une preuve Gate A.
- Allouer la marge ou un residu a ε_t.
- Un second composeur. Toute ecriture de commande hors composeur.
- Toute ecriture V2 de position, rotation, vitesse, `MoveRotation` ou `Teleport`.
- Emettre `BrakeReverse > 0` a `MinimumDirectionSpeed` ou en dessous.
- Hors perimetre : `SafetyFilter`, suivi de leader, evitement, grants, Rage/Fear, recuperation, gridlock, LOD.
- Reprendre `RecoverAtWaypoint`, `WaypointIndex`, `arrivalRadius`, la boucle ouverte ou le rappel de point vise.
- Re-ajuster la reference.
- Presenter un resultat physique ou reseau sans execution Unity.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Campagne de mesure | `V2Slice`, `MeasurementRun`, ε_t declare | Insertions successives, conduite, retrait aux sorties ; d, v, v* et couverture publies | d > ε_t, v > v*, campagne incomplete : echec |
| Run exploratoire | `MeasurementRun`, ε_t non declare | Mesures publiees, aucune acceptation | N/A |
| Hors mesure | `V2Slice`, a_e = 0 | Aucune insertion | `NotCoveredByGateA` / `TrackingToleranceUndeclared` |
| Jeton hors tests | Construction dans le code de production | Echec d'assertion structurelle | N/A |
| Preuve / modele | Build joueur, preuve perimee, modele non declare | Aucune insertion | Code nomme |
| Commande invalide, v > seuil | NaN/∞, fenetre perimee, profil refuse | Repli : frein de service borne, un pas | Diagnostic |
| Commande invalide, v ≤ 0,25 m/s | Idem | Aucune entree de frein (frein moteur), jamais de marche arriere | Diagnostic |
| Plafond | Courbe plus serree, v* inatteignable | Freinage anticipe ; sinon `SteeringCeilingUnreachable` et frein a `SafeBrakingLimit` | Refus du verificateur → repli |
| Limites reportees | 0 authore ; valeur non nulle synthetique | `DeferredUnauthored` ; `DeferredAuthored(v)`, jamais appliquee | N/A |
| Echec en route | `NoRoute`, horizon non conforme | Vehicule present, repli | Aucun retrait |
| Profil conducteur absent | `DriverProfileDef` nul | Inerte | Diagnostic unique, sans repli code |
| Composition | V1 par defaut ; V2 change apres insertion | V1 inchange ; choix fige | Diagnostic |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs:12,27,43,61` -- `PlanningRequest`, `Evaluate`, `PlanningDecision`. Exceptions pour une requete invalide, codes souples sinon.
- `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs:14,51,78` -- `TrafficActorInput`, tri, localisation unique, `TryGetActor`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/PathHorizon.cs:11,29,50,77,108` -- `PathPoint` (v* en +∞ float, `Unbounded`), `Points`, `PathSeam.MinimumCeilingMetersPerSecond`, `SignedRingSeams`. Pas d'API d'echantillonnage.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs:16,24,37,71,107` -- `TrackingTolerance`, `LongitudinalBounds`, `SpeedProfilePoint`, couverture, `VerifySpeedProfile`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/GateAEvidenceBinding.cs:33` -- `Bind`. a_e est lu dans le bloc hache.
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:33,76` -- a etendre sur place.
- `Assets/RoadRage/Tests/EditMode/Story530PlanningSpineTests.cs:253-263` -- `Frame/`, `Perception/`, `Planning/` et `Debug/` ne nomment jamais `VehicleDriveIntent`, `VehiclePhysicsBody`, `ApplyDriveIntent`.
- `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs:231-249,295,762` -- l'intent persiste d'un pas a l'autre, sans garde de finitude ni garde hote (l'appelant pose `isKinematic = !IsServer`). `TrySampleTelemetry`.
- `Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs` (`ResolveWheelDriveTorque`, `ResolveWheelBrakeTorque`) -- `BrakeReverse` freine au-dessus de `minimumDirectionSpeed` (0,25) et recule en dessous. Sans entree, `coastTorque` s'applique.
- `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs:16-24` -- `Idle` = (0,0,0,0), inchange.
- `Assets/RoadRage/Features/Vehicles/VehicleSteeringModel.cs:39` -- angle vise = f(`Steer`, vitesse, verrou 40°→16°).
- `Assets/RoadRage/Features/Vehicles/DriverModel.cs:19,61` -- `NoLeaderGap`, `ComputeAcceleration`. Par defaut : v0 = 8, a = 1,5, b = 2, `SafeBrakingLimit` = 4.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs:251,307,724-798,1092-1109` -- precedents V1 a lire seulement : kinematique client, `IsServer`, pedale et capacite, soumission.
- `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs:36,40,90,171,256-357` -- population et portails. Le controleur V1 y est code en dur a :269 et :335.
- `Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab`, `DefaultNetworkPrefabs.asset:28` -- modele des composants du prefab V2.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs:63,487` -- limite de vitesse reportee a la 5.33.
- `Assets/RoadRage/Tests/PlayMode/Story510RoutedTrafficPlayModeTests.cs:104-307,384-406` -- gabarit bootstrap → lobby → `MVP_Run`.
- `Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs:47,91-139,158-172,225-282`, `Story59ParameterizedDriverModelTests.cs:46-67`, `Story514AiDrivesByIntentTests.cs:55,104-147` -- gardes a garder verts.
- `_bmad-output/implementation-artifacts/v1-regression-5-51/after-playmode-results.json` -- reference PlayMode : 45 tests, 39 reussis, 6 echecs connus.

## Tasks & Acceptance

**Execution:**
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/SpeedPlan.cs` -- contraintes appliquees et reportees, passes, liante, verification 5.30.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs` -- acceleration et angle vises, `SourceFrameId`, validite.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs` -- composeur unique, garde, repli V2.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` -- composition, population, ε_t declare, jeton `MeasurementRun` (campagne), fournisseur de preuve Editeur mis en cache.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- `NetworkBehaviour` hote seul : cycle, retrait, enregistreur de mesure (regle d'intervalle), chronometres.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- lignes ajoutees.
- [ ] `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` -- branche `V2Slice`. Branche V1 intacte.
- [ ] `Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab`, `DefaultNetworkPrefabs.asset`, `MVP_Run.unity` -- en local, avec la double garde AD-6. V1 reste le defaut.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531SpeedPlanAndComposerTests.cs` `[Category("Core")]` -- couvrir :
  - la matrice ;
  - un intent par pas ;
  - le repli pour chaque axe et chaque scalaire NaN/∞, et pour une fenetre perimee ;
  - l'absence de `BrakeReverse > 0` a v ≤ 0,25 ;
  - la validite d'un pas ;
  - `Idle` V1 inchange ;
  - la verification 5.30 sur chaque profil ;
  - les etats `DeferredUnauthored` / `DeferredAuthored` ;
  - `SteeringCeilingUnreachable` ;
  - les raccords d'anneau ;
  - l'enveloppe IDM.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531LifecycleAndCompositionTests.cs` `[Category("Core")]` -- couvrir :
  - les refus (couverture, ε_t, preuve, modele) ;
  - identite et graine deterministes ;
  - retrait uniquement a s ≥ portail ;
  - la regle d'intervalle de mesure sur des trajectoires synthetiques ;
  - des scans : seul le composeur construit l'intent ; seul le driver V2 appelle `ApplyDriveIntent` ; aucune ecriture Rigidbody ou `Teleport` ; jeton `MeasurementRun` absent hors `Tests/` ; prefab V2 sans type V1 ; gardes 5.7 et 5.30 verts.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531DrivenReplayTests.cs` `[Category("Geometry")]` -- couvrir :
  - le rejeu cinematique de toute la chaine sur `MVP_Run`, v ≤ v* partout ;
  - l'ensemble de campagne couvrant les 72 mouvements et les 24 raccords d'anneau.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story531V2VerticalSlicePlayModeTests.cs` -- couvrir :
  - la campagne exploratoire (non acceptante) ;
  - la campagne d'acceptation, qui exige ε_t declare ;
  - le refus hors mesure ;
  - le modele non declare.
  Sortie brute consignee.
- [ ] `_bmad-output/implementation-artifacts/deferred-work.md`, `sprint-status.yaml` -- reports 5.30 traites (cache de liaison, transport Editeur, consommation de `Unbounded`) ; raccords d'anneau renvoyes a la 5.52. Puis `graphify update .`.

**Acceptance Criteria:**
- Given une campagne d'acceptation sous `MeasurementRun` avec ε_t declare, when elle s'execute dans `MVP_Run`, then chaque vehicule V2 s'insere a une entree et se retire a une sortie, sans evenement en route ni controleur V1, et chaque pas publie d ≤ ε_t (regle d'intervalle comprise) et v ≤ v*(s). Chaque mouvement et chaque raccord d'anneau est parcouru.
- Given a_e = 0 signe, when un vehicule V2 est demande hors `MeasurementRun`, then aucune insertion n'a lieu, la raison est publiee et aucune preuve n'est ecrite.
- Given une commande invalide en route, when le composeur emet, then un freinage de repli fini et borne vaut un seul pas, jamais une marche arriere, et le vehicule reste present.
- Given la composition `V1` par defaut, when la suite PlayMode complete s'execute, then les resultats V1 egalent la reference (6 echecs connus, aucun nouveau).
- Given une decision, when sa projection est inspectee, then elle distingue contraintes appliquees et reportees et porte la liante, les epoques, l'intent final, la couverture et l'etiquette de mesure, sans nouveau chemin de synchronisation client.

## Spec Change Log

- **2026-09-29 -- arbitrages proprietaire A1 = a, A2 = a, A3 = a, A4 = a, A5 = b** (planification, avant approbation) :
  - La Gate B est retiree de la story ; integration de ε_t et re-signature en 5.52.
  - ε_t sur le deplacement du gabarit, avec regle d'intervalle.
  - Limites de route et d'adherence reportees et identifiables.
  - Composition par session, jeton reserve aux tests.
  - Freinage de repli V2 borne, valable un pas, sans marche arriere.
  - Correction d'un constat anterieur : `Idle` applique `coastTorque`, ce n'est pas une roue libre pure.

## Design Notes

**Couverture de la reference ≠ erreur de suivi.**
- *Faits.* La preuve couvre la reference compilee, gonflee de marge + δ_c. a_e = 0 est lie par `ClearanceHash`, mais n'est qu'un texte (`AuthoredRoadModel.cs:1313,1397`). La marge est reservee (C:415).
- Un ε_t > 0 n'est donc couvert par rien tant que la 5.52 n'a pas fait entrer l'allocation dans chaque preuve et obtenu ta re-signature.
- *Estimation non verifiee :* residu minimal ≈ 0,1127 m. Si l'ε_t mesure en approche, la 5.52 echouera sur la geometrie, pas sur la signature.

**Pourquoi aux coins.** Un ecart de cap deplace les coins d'environ (L/2)·|sin Δψ| a ecart lateral nul. Seule une borne sur chaque point du gabarit rend valide le gonflement de Minkowski de la preuve. La regle d'intervalle reprend le lemme de balayage 5.50 (δ/2) : entre deux pas, la position est supposee lineaire et le cap monotone.

**Etudie dans le code (Cloud, 2026-09-29) :**
- API finales 5.29 / 5.30 ;
- persistance de l'intent et absence de garde de finitude ;
- semantique frein / marche arriere / `coastTorque` ;
- limites reportees ;
- `SessionSeed` jamais ecrit ;
- couplage V1 du spawner ;
- gardes 5.7 et 5.30 ;
- 6 echecs PlayMode connus.

**A mesurer en local, rien n'a ete execute ici :**
- ε_t ;
- d, v face a v* ;
- comportement aux 24 raccords d'anneau (saut de κ, taux de braquage de 300°/s) ;
- efficacite reelle du repli ;
- temps par etape ;
- non-regression V1 ;
- duree de la campagne. *Estimation :* plusieurs minutes de PlayMode pour couvrir 72 mouvements.

**Choix de conception (contestables).**
- `ComfortableDeceleration` pour planifier, `SafeBrakingLimit` pour l'infaisabilite et le repli.
- Cadence et validite d'un pas.
- Preuve lue dans l'Editeur seulement.
- Composition figee avant la premiere insertion.

## Verification

Rien n'a ete execute dans le Cloud (pas d'Unity, de PowerShell ni de Test Runner).

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story531SpeedPlanAndComposerTests"` -- expected: vert, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Full` -- expected: EditMode complet vert.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- expected: suite complete. Tests 5.31 verts (l'acceptation exige ε_t declare), 6 echecs connus identiques, aucun nouveau. Sortie brute et mesures consignees.
