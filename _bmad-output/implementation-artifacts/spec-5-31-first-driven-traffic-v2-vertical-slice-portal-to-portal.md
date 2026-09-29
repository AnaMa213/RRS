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
- **Commande de repli V2 (A5).**
  - Emission : par ce seul composeur, quand il n'y a pas de commande valide (absente, hors fenetre, profil refuse, axe ou scalaire non fini).
  - Recalcul : a **chaque pas**, a partir de la vitesse longitudinale mesuree v (telemetrie) et de v_dir = `MinimumDirectionSpeed` (0,25 m/s).
  - Regles :
    - v > v_dir : gaz 0 ; frein de service = `SafeBrakingLimit` converti par la capacite de frein, borne ]0, 1] ; frein a main 0 ; volant = dernier volant fini valide, sinon 0.
    - |v| ≤ v_dir : gaz 0, `BrakeReverse` 0, **frein a main 1** (maintien). Le frein a main applique max(service, `HandbrakeTorque`) aux roues non directrices, quel que soit le sens de marche (`VehiclePhysicsBody.cs:449-466`).
    - v < −v_dir (le vehicule recule) : meme sortie, plus un diagnostic `RollingBackward`.
    - Jamais `BrakeReverse > 0` pour v ≤ v_dir, vitesses negatives comprises : `VehicleTireModel` y engagerait la marche arriere.
  - Etats terminaux, diagnostiques et publies. Le vehicule reste present.
    - `FallbackHeld` : |v| ≤ `FallbackStoppedSpeed` pendant `FallbackStoppedSteps` pas consecutifs.
    - `FallbackStopOverrun` : pas d'arret apres `FallbackOverrunFactor` × (v₀ / `SafeBrakingLimit`) secondes. Le maintien continue.
    - Constantes declarees : 0,05 m/s, 10 pas, facteur 2. Ce sont des parametres de conception, pas des resultats.
  - Une commande valide revenue reprend la main au pas suivant.
  - Un scalaire de profil non fini rend le vehicule inerte, avec un diagnostic.
  - `VehicleDriveIntent.Idle` et sa semantique V1 sont inchanges. Ce repli n'est pas le `SafetyFilter` (5.37).
- **Couverture.**
  - Reference : regle 5.30.
  - Vehicule : il faut un ε_t declare (constante unique). Tant qu'il n'est pas declare, pas de conduite hors mesure (`TrackingToleranceUndeclared`). La regle 5.30 « compte 0 » ne vaut jamais autorisation.
  - Couvert ⇔ max|o| + ε_t ≤ a_e de la preuve valide. Le verdict est publie.
  - Ni `LateralClearanceMarginMeters` ni un residu 5.51 n'entrent dans ce calcul, et la 5.31 ne regenere ni ne signe rien.
- **Mesure (A2).**
  - **Hypotheses declarees.**
    - H1 : la simulation ne definit l'etat du vehicule qu'aux pas physiques (PhysX discret, `m_CollisionDetection: 0`). Entre deux pas, le mouvement est *modelise* : translation lineaire du point de reference, cap lineaire sur l'arc le plus court, |Δθ| < 90° par pas (echec ferme sinon).
    - H2 : road-up constant (C4).
    - H3 : le gabarit max contient la caisse. Le test le verifie sur le prefab V2 (collider centre sur le point de reference, dans le gabarit).
    - Aucune garantie continue n'est revendiquee au-dela de H1.
  - **Grandeur mesuree.**
    - Gabarit max (L = `MaxVehicleLengthMeters`, W = `MaxVehicleHalfWidthMeters`), centre sur le point de reference.
    - s* est la projection du point de reference reel sur la reference compilee, en distance d'horizon. La pose nominale N(s*) est la pose de reference en s*.
    - f_i = coin_i(reel) − coin_i(N). d = max_i |f_i|. Le deplacement est affine en chaque point du corps, donc son maximum sur le rectangle est atteint en un coin.
  - **Au pas k :** d_k est exact.
  - **Entre deux pas** (borne a progression appariee) :
    - σ(τ) interpole lineairement s*_k → s*_k+1, et le reel suit H1.
    - On evalue d sur une sous-grille de pas h en τ, bornes comprises, puis : sup d ≤ max_grille d + L_k·h/2 + ζ_k.
      - L_k = |Δp| + ρ|Δθ| + ℓ_k(1 + ρ·ψ'_max), ρ = √((L/2)² + W²), ℓ_k = |Δs*|.
      - ψ'_max = max, sur les segments compiles traverses, de 2·tan(α_j/2)/Δs_j (taux de rotation de la tangente normalisee-interpolee).
      - ζ_k = ecart + ρ·saut de cap de tout raccord traverse.
    - h est choisi pour que L_k·h/2 ≤ 1 mm.
    - La preuve est en Design Notes.
    - La regle `max(d_k, d_k+1) + δ/2` du lemme 5.50 est valide sous H1 mais **n'est pas utilisee** : elle consomme a elle seule le residu signe (voir Design Notes).
  - Publies aussi : ecart lateral, ecart de cap, vitesse observee face a v*(s*).
  - **Moniteur runtime.** La borne au pas est evaluee a chaque pas pour chaque vehicule V2 (diagnostic `TrackingToleranceExceeded`). La borne entre deux pas complete le calcul en post-traitement de la trace.
  - Toute vitesse > v*, ou une borne > ε_t declare, fait echouer la campagne d'acceptation.
  - **Campagne deterministe.**
    - Un constructeur EditMode (Geometry) cherche, avec un budget declare de graines, des triplets (entree, sortie, graine) dont les routes planifiees par `RoutePlanner` couvrent les 72 mouvements, les 24 raccords d'anneau et les 44 corridors.
    - Il ecrit la campagne et l'incidence element → triplets.
    - Un element jamais selectionne dans le budget est publie `NonSelectionnable`. La campagne est alors refusee, et on s'arrete pour demander au proprietaire (Ask First).
  - **Tracabilite par element** (corridor, mouvement, raccord et son cote) : passages, runs, d max (au pas et entre deux pas), max v/v*, marge a ε_t.
    - Un element non parcouru est publie `NonMesure`. Il n'est **jamais** compte comme couvert, et il rend la campagne incomplete.
    - Rapports bruts separes, exploratoire et acceptation, sous `_bmad-output/implementation-artifacts/traffic-v2-5-31-measurements/`.
  - **Separation des validations.** Les campagnes PlayMode sont `[Explicit]` + `[Category("Story531Campaign")]`, hors suite par defaut, et lancees par filtre de categorie en plus de la suite complete, jamais a sa place. La suite par defaut garde les tests courts.
  - Conditions consignees : pas physique 0,02 s, Editeur en hote, profils par defaut, commit, budget de graines.
  - **Sequence :** (1) campagne exploratoire, qui ne vaut pas acceptation ; (2) declaration de ε_t par le proprietaire ; (3) campagne d'acceptation.
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
| Repli, v > 0,25 m/s | NaN/∞, fenetre perimee, profil refuse | Frein de service borne, recalcule a chaque pas jusqu'a l'arret | Diagnostic |
| Repli, \|v\| ≤ 0,25 m/s | Idem | Frein a main, aucun `BrakeReverse`, puis `FallbackHeld` | Diagnostic |
| Repli, recul | v < −0,25 m/s | Frein a main, aucun `BrakeReverse` | `RollingBackward` |
| Repli sans arret | Arret non atteint dans la borne | Maintien continue, vehicule present | `FallbackStopOverrun` |
| Element non parcouru | Campagne sans passage sur un mouvement ou raccord | `NonMesure`, jamais couvert | Campagne incomplete |
| Element non selectionnable | Aucune graine du budget ne le route | `NonSelectionnable` | Campagne refusee, Ask First |
| Pas physique a grand cap | \|Δθ\| ≥ 90° entre deux pas | Borne non evaluable | Echec ferme |
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
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Intent/VehicleDriveIntentComposer.cs` -- composeur unique, garde, commande de repli recalculee a chaque pas selon la vitesse mesuree, etats `FallbackHeld` / `FallbackStopOverrun`.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs` -- composition, population, ε_t declare, jeton `MeasurementRun` (campagne), constantes de repli, fournisseur de preuve Editeur mis en cache.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` -- `NetworkBehaviour` hote seul : cycle, retrait, moniteur runtime de la borne au pas, trace par pas pour la borne entre deux pas, tracabilite par element, chronometres.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- lignes ajoutees.
- [ ] `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` -- branche `V2Slice`. Branche V1 intacte.
- [ ] `Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab`, `DefaultNetworkPrefabs.asset`, `MVP_Run.unity` -- en local, avec la double garde AD-6. V1 reste le defaut.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531SpeedPlanAndComposerTests.cs` `[Category("Core")]` -- couvrir :
  - la matrice ;
  - un intent par pas ;
  - le repli pour chaque axe et chaque scalaire NaN/∞, et pour une fenetre perimee ;
  - la regle de repli par pas selon v (au-dessus du seuil, dans ±0,25, en recul), sans aucun `BrakeReverse > 0` a v ≤ 0,25 ;
  - les etats `FallbackHeld` / `FallbackStopOverrun` et la reprise sur commande valide ;
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
  - la borne entre deux pas sur des trajectoires synthetiques :
    - ecart lateral constant en ligne droite → borne = ecart a 1 mm pres ;
    - rotation pure ;
    - traversee de raccord ;
    - |Δθ| ≥ 90° → echec ;
    - borne ≥ maximum dense par force brute ;
    - borne ≤ regle δ/2 sur un cas de suivi parfait a 8 m/s ;
  - H3 : collider du prefab V2 contenu dans le gabarit, centre sur le point de reference ;
  - des scans : seul le composeur construit l'intent ; seul le driver V2 appelle `ApplyDriveIntent` ; aucune ecriture Rigidbody ou `Teleport` ; jeton `MeasurementRun` absent hors `Tests/` ; prefab V2 sans type V1 ; gardes 5.7 et 5.30 verts.
- [ ] `Assets/RoadRage/Tests/EditMode/Story531DrivenReplayTests.cs` `[Category("Geometry")]` -- couvrir :
  - le rejeu cinematique de toute la chaine sur `MVP_Run`, v ≤ v* partout ;
  - le constructeur de campagne : budget de graines declare, incidence par element (72 mouvements, 24 raccords avec leur cote, 44 corridors), `NonSelectionnable` explicite, fichier de campagne ecrit.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story531V2VerticalSlicePlayModeTests.cs` (suite par defaut, tests courts) -- couvrir :
  - le refus hors mesure ;
  - le modele non declare ;
  - un run de mesure court sur une route.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story531FallbackLowSpeedPlayModeTests.cs` (suite par defaut) -- banc physique isole (patron Story512/513), `VehiclePhysicsBody` et profil du prefab V2. Repli force a chaque pas depuis :
  - 8 m/s ;
  - 0,3 m/s ;
  - 0,2 m/s ;
  - −0,5 m/s ;
  - sur sol plat et sur une pente declaree.
  Assertions :
  - vitesse longitudinale jamais < −v_dir apres un depart en marche avant ;
  - decroissance jusqu'a `FallbackHeld` ou `FallbackStopOverrun` explicite ;
  - maintien |v| ≤ 0,05 m/s pendant l'arret ;
  - aucun `BrakeReverse > 0` a v ≤ v_dir ;
  - trace brute publiee. Les seuils de resultat ne sont pas presumes.
- [ ] `Assets/RoadRage/Tests/PlayMode/Story531MeasurementCampaignPlayModeTests.cs` `[Explicit]` `[Category("Story531Campaign")]` -- campagnes exploratoire et d'acceptation (l'acceptation exige ε_t declare) ; tracabilite par element ; rapports bruts sous `traffic-v2-5-31-measurements/`.
- [ ] `_bmad-output/implementation-artifacts/deferred-work.md`, `sprint-status.yaml` -- reports 5.30 traites (cache de liaison, transport Editeur, consommation de `Unbounded`) ; raccords d'anneau renvoyes a la 5.52. Puis `graphify update .`.

**Acceptance Criteria:**
- Given une campagne d'acceptation sous `MeasurementRun` avec ε_t declare, when elle s'execute dans `MVP_Run`, then :
  - chaque vehicule V2 s'insere a une entree et se retire a une sortie, sans evenement en route ni controleur V1 ;
  - la borne au pas et entre deux pas reste ≤ ε_t, et v ≤ v*(s) ;
  - chaque mouvement, raccord d'anneau et corridor est rapporte `Mesure` avec ses valeurs. Un seul `NonMesure` fait echouer la campagne.
- Given un repli force a chaque pas, when le banc physique part de 8, 0,3, 0,2 ou −0,5 m/s, then le vehicule n'accelere jamais en marche arriere et atteint `FallbackHeld`, ou emet `FallbackStopOverrun`, en restant present.
- Given a_e = 0 signe, when un vehicule V2 est demande hors `MeasurementRun`, then aucune insertion n'a lieu, la raison est publiee et aucune preuve n'est ecrite.
- Given une commande invalide en route, when le composeur emet, then la commande de repli est finie, recalculee a chaque pas selon la vitesse mesuree, sans `BrakeReverse` a v ≤ 0,25 m/s, et le vehicule reste present.
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
- **2026-09-29 -- revision demandee par le proprietaire** (avant approbation) :
  - La borne entre deux pas δ/2 est remplacee par une borne a progression appariee, avec hypotheses H1 a H3 et moniteur runtime.
  - Le repli devient une regle par pas selon la vitesse mesuree (frein a main a basse vitesse et en recul, etats terminaux diagnostiques), avec un banc physique a tres basse vitesse.
  - Campagne : constructeur deterministe, tracabilite par element, `NonMesure` / `NonSelectionnable` explicites, campagnes `[Explicit]` separees.
  - Controle du fichier : aucun doublon dans le Code Map ni dans les Tasks.

## Design Notes

**Couverture de la reference ≠ erreur de suivi.**
- *Faits.* La preuve couvre la reference compilee, gonflee de marge + δ_c. a_e = 0 est lie par `ClearanceHash`, mais n'est qu'un texte (`AuthoredRoadModel.cs:1313,1397`). La marge est reservee (C:415).
- Un ε_t > 0 n'est donc couvert par rien tant que la 5.52 n'a pas fait entrer l'allocation dans chaque preuve et obtenu ta re-signature.
- *Estimation non verifiee :* residu minimal ≈ 0,1127 m. Si l'ε_t mesure en approche, la 5.52 echouera sur la geometrie, pas sur la signature.

**Pourquoi aux coins.** Un ecart de cap deplace les coins d'environ (L/2)·|sin Δψ| a ecart lateral nul. Seule une borne sur chaque point du gabarit rend valide le gonflement de Minkowski de la preuve. Pour un point b du corps, f_b = (p − n) + (R(θ) − R(ψ))·b est affine en b, donc |f_b| est convexe en b et maximal en un coin du rectangle.

**Borne entre deux pas -- preuve.**
- *Hypotheses :* H1 a H3 (Boundaries). La preuve Gate A couvre toute pose nominale continue en s, puisque le balayage 5.51 inclut son propre terme d'intervalle. Il suffit donc de borner la distance de chaque point reel a *une* pose nominale.
- *Construction :*
  - on apparie τ ↦ N(σ(τ)), avec σ lineaire de s*_k a s*_k+1 ;
  - sous H1, un point reel du corps se deplace a au plus |Δp| + ρ|Δθ| par unite de τ ;
  - le point nominal correspondant se deplace a au plus ℓ_k(|dn/ds| + ρ|dψ/ds|), avec |dn/ds| ≤ 1 (positions compilees lineaires en s, corde ≤ arc) et |dψ/ds| ≤ 2·tan(α_j/2)/Δs_j sur chaque segment (tangente normalisee-interpolee, maximum au milieu du segment) ;
  - chaque |f_i| est donc L_k-lipschitzienne en τ, et le max sur les coins aussi.
- *Conclusion :* sur une grille de pas h, sup ≤ max aux noeuds + L_k·h/2. Un raccord traverse ajoute son saut, ζ_k. La borne est exacte aux pas et rigoureuse entre les pas **sous H1 seulement**. Le vrai mouvement PhysX n'est pas defini entre deux pas, puisque la detection de collision est discrete.
- *Pourquoi pas δ/2 (lemme 5.50) :* ce lemme borne la distance a une pose *reelle* d'extremite. Il reste valide pour le vehicule physique sous H1, mais il facture toute la translation le long de la route.
  - A 8 m/s sur l'anneau de 6 m, avec un pas de 0,02 s : |Δp| = 0,16 m, Δθ = 0,0267 rad, ρ = 2,4746 m, donc δ/2 = 0,113 m.
  - C'est l'integralite du residu signe minimal (0,1127 m). La borne appariee retire ce terme. A suivi parfait, les termes de second ordre sont de l'ordre du millimetre (calcul d'ordre de grandeur, pas une mesure).
- *Garantie empirique :* ε_t declare est verifie sur la campagne, pas prouve pour des conditions non parcourues. D'ou le moniteur runtime a chaque pas.
- *H3, verifie statiquement :*
  - essieux a z = ±1,55 (`VehicleProfileDef_Default`), donc point de reference = origine ;
  - `BoxCollider` 2,06 × 4,44 m centre a l'origine (`Greybox_AIVehicle.prefab`), contenu dans le gabarit 2,06 × 4,5 m, sans aucune marge en largeur ;
  - a reverifier sur le prefab V2.

**Repli.** `BrakeReverse` sous v_dir = marche arriere (`VehicleTireModel.ResolveWheelDriveTorque`). Le frein a main agit sur les roues non directrices, quel que soit le sens (`VehiclePhysicsBody.cs:449-466`). L'echec PlayMode connu `Story512...TheHandbrakeLocksTheRearWheelsAndBreaksTheirGrip` porte sur la perte d'adherence en vitesse, pas sur le maintien. L'efficacite du maintien reste a mesurer sur le banc.

**Campagne, faits verifies sur le JSON signe.**
- 72/72 mouvements sont sur un chemin entree → sortie, aucun poids nul.
- Les 12 mouvements de poids 1 sont des entrees d'anneau sans alternative.
- *Inconnu :* que `RoutePlanner` selectionne chaque mouvement dans le budget de graines. Le cout global (distance + preference) peut ecarter des detours ; le constructeur l'etablit en local.

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
- selectionnabilite de chaque element par le planificateur ;
- maintien du frein a main (plat et pente) ;
- rapport des tests `[Explicit]` par Unity en PlayMode ;
- duree de la campagne. *Estimation :* plusieurs minutes de PlayMode.

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
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story531DrivenReplayTests"` -- expected: rejeu vert, campagne construite, aucun `NonSelectionnable`.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- expected: suite par defaut complete. Tests 5.31 courts et banc de repli verts, 6 echecs connus identiques, aucun nouveau.
- `.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story531Campaign -TestFilterType category` -- expected: en plus de la suite, jamais a sa place. Campagne exploratoire, puis acceptation apres declaration de ε_t. Aucun `NonMesure`, rapports bruts consignes.
