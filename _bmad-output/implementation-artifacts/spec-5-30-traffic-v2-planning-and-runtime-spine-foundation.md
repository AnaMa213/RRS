---
title: 'Story 5.30 -- Fondation de la chaine de planification et de l epine runtime Traffic V2'
type: 'feature'
created: '2026-09-29'
status: 'ready-for-dev'
review_loop_iteration: 0
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le modele signe est routable (5.29) mais aucune chaine ne relie une frame hote a une geometrie locale suivable. Sans preuve isolee de la separation route / chemin / mouvement, la 5.31 deboguerait la planification a travers la physique.

**Approach:** Construire une chaine pure cote hote : `TrafficFrame` immuable -> `AgentObservation` minimale -> `RoutePlan` (5.29) -> `PathHorizon` lu sur la reference compilee -> `MotionPlan` geometrique -> projection de decision textuelle indexee sur la frame.

Le `MotionPlan` publie les contraintes geometriques, verifie un profil de vitesse candidat et etablit la couverture de la **reference** par la preuve Gate A signee. La faisabilite physique et la couverture du **vehicule** appartiennent a la 5.31. Aucun intent, aucune physique.

## Boundaries & Constraints

**Always:**

- **Admission du modele.** Seulement via `RoadModelDocument.Load` puis `RoadModelCompiler.Compile`. L'orchestrateur refuse un `CompiledRoadModel` dont `DrivabilityProfile.Declared` est faux (`UndeclaredDrivabilityProfile`).
- **`TrafficFrame`.**
  - Contenu : `FrameId` fourni par l'appelant, `ModelId`, `Version`, et les acteurs tries par `TrafficId`.
  - Chaque acteur porte `TrafficId`, `VehicleFootprintPose` (ancre d'empreinte), vitesse, element precedent et `RoadLocation`. La localisation est calculee une fois a la construction par `RoadLocalizer.Localize`.
  - Collections copiees et exposees en lecture seule. L'ordre d'entree est sans effet. Un `TrafficId` vide ou duplique est refuse.
- **Orchestrateur.** Il lit une seule frame par decision et ne garde aucun etat.
  - Entrees : frame, `TrafficId`, `RoutePlan` precedent, sortie visee, graine de session, `lookAheadMeters`, liaison de preuve Gate A, bornes longitudinales.
  - Sortie : `PlanningDecision`. L'appelant conserve le `RoutePlan`.
- **`AgentObservation`.** `FrameId`, `TrafficId`, `RoadLocation`, vitesse tangentielle, empreinte. Rien d'autre.
- **Route.** `RoutePlanner.Plan` inchange, domaine `"route"`, reutilisation via `existing`.
- **`PathHorizon`.**
  - Intervalles (kind, id, s0, s1) pris dans les occurrences du `RoutePlan`, de la progression jusqu'a min(portail de sortie, s + `lookAheadMeters`). La fin est qualifiee : `ExitPortal` ou `LookAheadLimit`.
  - Echantillonnage via le `RoadCurve.Sample` de chaque element, jamais via une courbe reconstruite, lissee ou reechantillonnee. o(s) ≡ 0.
  - Publie par raccord : ecart, saut de cap et saut de courbure (s, Δκ, v* des deux cotes). Publie aussi |dκ/ds| max par intervalle.
- **Tolerances declarees** (une seule source de constantes nommees) :
  - raccord : ecart ≤ `SeamGapToleranceMeters` et cap ≤ `SeamTangentToleranceDegrees`, relus du `ValidationProfile` ;
  - raccord : |Δκ| ≤ 1e-3 m⁻¹. Seule exception : les 24 raccords tangents d'anneau signes, publies comme discontinuites. En ces points, v* = min des deux cotes ;
  - dans un element : |dκ/ds| ≤ 0,30 m⁻².
- **Liaison de preuve Gate A** (`GateAEvidenceBinding`). Fonction pure sur trois textes : modele, sign-off et rapport commites.
  - Elle exige : `RoadModelVersion` du sign-off = version compilee ; `ModelHash` = SHA-256 du texte du modele ; SHA-256 du bloc « Residus » du rapport (de la ligne `a_e = ` a la derniere ligne du tableau, `\n` final compris) = `ClearanceHash`.
  - a_e est lu dans la premiere ligne de ce bloc. Il doit etre fini et ≥ 0.
  - Resultats : texte ou bloc absent, ou a_e illisible → `GateAEvidenceMissing` ; version ou empreinte differente → `GateAEvidenceStale`.
  - C'est une **liaison documentaire** aux artefacts signes. Ce n'est pas une revalidation physique : celle-ci reste `GateAIsOpenedOnlyByTheOwnersBoundSignoff` (5.28, profils Geometry et Full).
- **Couverture a deux niveaux.**
  - **Reference :** couverte ⇔ liaison valide et max|o(s)| + ε_t ≤ a_e.
  - ε_t : tant qu'il n'est pas declare, il compte pour 0, conformement a `epics.md:2640`. Il est porte comme `TrackingTolerance.Undeclared`, jamais comme un zero anonyme.
  - **Vehicule :** toujours `NotEstablished` en 5.30.
  - Echec : liaison absente ou perimee → plan infaisable avec le code de liaison ; ε_t declare avec max|o| + ε_t > a_e → `NotCoveredByGateA`.
  - `LateralClearanceMarginMeters` n'entre jamais dans ce calcul.
- **`MotionPlan`, trois niveaux distincts.**
  - (1) **Contraintes geometriques publiees.**
    - κ(s), |dκ/ds| et v*(s) = `RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profil, κ)`.
    - Un v* = +∞ est represente comme `Unbounded`, jamais comme une vitesse. Il s'affiche « aucun ».
    - Discontinuites de raccord, fin d'horizon, couverture de reference.
    - Un v* NaN ou < `SteeringInactiveBelowMetersPerSecond` rend le plan geometriquement infaisable.
  - (2) **Verification d'un profil candidat** {(s, v)} fourni par l'appelant, avec des bornes explicites `MaxAcceleration` / `MaxDeceleration`. Par defaut, l'orchestrateur prend `DriverProfile.MaxAcceleration` / `SafeBrakingLimit`.
    - Entrees : s strictement croissant, dans l'horizon ; v fini et ≥ 0. Sinon → `InvalidSpeedProfile`.
    - Entre deux points, v² est lineaire en s. On en tire a = (v₁² − v₀²)/(2Δs), qui doit rester dans [−`MaxDeceleration`, `MaxAcceleration`]. Sinon → `AccelerationBoundExceeded` au premier s fautif.
    - Plafond : evalue sur l'union des points du profil, des echantillons de courbe et des raccords. Sur chaque sous-intervalle, max(v) aux bornes doit etre ≤ min(v*) aux bornes. Sinon → `SteeringCeilingExceeded` au premier s fautif.
    - v = 0 est admis (arret, redemarrage).
    - 0 < v < `SteeringInactiveBelowMetersPerSecond` sur une portion courbe → diagnostic `SteeringInactiveSpan`, pas un echec.
    - Fin d'horizon par `LookAheadLimit` → diagnostic `HorizonTruncated`. Rien n'est juge au-dela.
  - (3) **Faisabilite physique** (adherence, taux de braquage, ε_t mesure) : hors 5.30. Aucun resultat n'est nomme « physiquement faisable ».
- **`TrafficDecisionProjection`.**
  - Immuable. Contenu : `FrameId`, `RoadModelVersion`, `TrafficId`, element et s, route (outcome, raison, portail, IDs), prochain mouvement, `PathPlanId` / `MotionPlanId`, statuts de liaison et de couverture, codes stables.
  - `ToText()` deterministe et invariant de culture.
  - Produite par la seule evaluation hote. La frame n'est jamais modifiee.
- **Assembly.** La decision 5.25 (aucun nouvel assembly) est confirmee, et la preuve est consignee.

**Ask First:**
- Modifier le modele `MVP_Run`, son sign-off, son rapport, sa geometrie, `Traffic/Migration/**` ou `scripts/validation-profiles.ps1`.
- Changer une tolerance declaree ou la regle de couverture.
- Toucher un type V1 retenu.
- Ajouter un asmdef, une dependance, une `NetworkVariable` ou une RPC.

**Never:**
- Composer un `VehicleDriveIntent`, appeler `VehiclePhysicsBody` ou creer un second chemin de controle.
- Generer un profil de vitesse (planificateur 5.31).
- Utiliser `MonoBehaviour`, `Rigidbody`, `Transform` ou Netcode dans les nouveaux dossiers.
- Hors perimetre : `SafetyFilter`, cycle de vie, portails, occupation, coordination.
- Reprendre `WaypointIndex`, `arrivalRadius` ou `ResolveLookAheadPoint`.
- Re-ajuster la reference compilee.
- Convertir `LateralClearanceMarginMeters` en tolerance de suivi.
- Ecrire, regenerer ou re-signer une preuve Gate A, ou ecrire dans l'overlay (son texte est hashe).
- Modifier `AIVehicleBehaviorDebugView`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Nominal | Frame valide, liaison valide, sortie atteignable | Route, horizon conforme, reference couverte, vehicule `NotEstablished`, projection complete | N/A |
| Modele non declare | `Declared = false` | Aucune decision | `UndeclaredDrivabilityProfile` |
| Preuve absente / perimee | Texte ou bloc absent, a_e illisible / version, `ModelHash` ou `ClearanceHash` different | Horizon publie, plan infaisable | `GateAEvidenceMissing` / `GateAEvidenceStale` |
| ε_t declare | 0,01 m (a_e = 0) ; ε_t < marge de 0,25 m | Reference non couverte dans les deux cas | `NotCoveredByGateA` |
| Localisation / `NoRoute` / plan obsolete | Invalide, inaccessible, perime ou ID repete | Outcome et raison de route ; replan `StalePlan` | Aucun effet monde |
| Raccords | Corridor ↔ mouvement, anneau | Continus ; discontinuites d'anneau publiees avec v* min | Autre saut : horizon non conforme |
| Profil : plafond | Depassement entre deux points grossiers du profil ; au raccord entre les v* des deux cotes ; egal au plafond a un echantillon | Detecte ; detecte ; admis | `SteeringCeilingExceeded` + s |
| Profil : freinage | Deceleration = borne ; au-dela | Admis ; refuse au premier s fautif | `AccelerationBoundExceeded` |
| Profil : vitesses basses | v = 0 (arret, redemarrage) ; 0 < v < 0,25 en courbe | Admis ; admis avec diagnostic | `SteeringInactiveSpan` |
| Profil : v* non borne | Ligne droite a vitesse elevee | Admis, plafond « aucun » | N/A |
| Profil invalide | v < 0 ou non fini, s non croissant ou hors horizon | Refus | `InvalidSpeedProfile` |
| Ordre | Acteurs ou vehicules permutes | Frame et projections identiques | N/A |
| Entrees invalides | `TrafficId` vide/duplique, `lookAheadMeters` ≤ 0 ou non fini | Refus explicite | Code stable |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelDocument.cs:71,119` -- `Load(string)` renvoie un `RoadModelSource` et leve `FormatException` si le profil n'est pas declare. Compiler ensuite avec `RoadModelCompiler.Compile` (`RoadModelCompiler.cs:103`).
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs:63` -- `SteeringSpeedCeilingMetersPerSecond(profile, κ)` : +∞ si κ = 0 ou verrou ≤ 16°, NaN si R ≤ 1,55 m, decroissant en |κ|. L'evaluer sur `Sample(s).CurvaturePerMeter`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadCurve.cs:66,114` -- `Sample(s)` est l'evaluation canonique : position et κ lineaires, tangente normalisee. κ est signe.
- `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs:414-425,484,510` -- `ModelId`, `Version`, profils, `TryGetCorridor` et `TryGetMovement`. `.Curve` de chaque element. Portails : parcourir `Portals`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs:643,714` -- `RoadModelValidationProfile` (seams 0,05 m / 5°, marge 0,25) et `DrivabilityProfile`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs:13,24,88,141` -- `VehicleFootprint`, `VehicleFootprintPose`, `RoadLocation` et `RoadLocalizer.Localize`, qui est pur.
- `Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:32,309` -- `Plan(...)`. La reutilisation avance la progression, mais echoue sur un ID repete. `RoutePlan.cs:14,30,83`.
- `Assets/RoadRage/Features/Vehicles/DriverProfile.cs:141,165` -- `MaxAcceleration` et `SafeBrakingLimit` : bornes par defaut du verificateur.
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-signoff.json` -- lu en lecture seule : `RoadModelVersion`, `ModelHash`, `ClearanceHash`. `eol=lf` est impose par `.gitattributes:181`.
- `_bmad-output/implementation-artifacts/migration-report-5-28-mvp-run.md:313` -- bloc « Residus » hache dans `ClearanceHash`, `eol=lf` (`.gitattributes:176`). Verifie le 2026-09-29 : SHA-256 du bloc (228 lignes, `\n` final) = `ClearanceHash` signe.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs:315,1313,1397` -- production du bloc et de son hash, `TrackingAllowanceMeters = 0`. Lecture seule.
- `Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs:928` -- controle leger Core du sign-off, qui ne couvre pas `clearance-hash`. `GateAIsOpenedOnlyByTheOwnersBoundSignoff` (Geometry) fait la revalidation complete.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/GateAReviewWindow.cs:19` -- shell overlay 5.28, a ne pas etendre (`OverlayHash`).
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json` -- 44 corridors, 72 mouvements, 4 entrees et 4 sorties. Chargement : `Story529RoutePlanTests.cs:406`.
- `Assets/RoadRage/Tests/EditMode/Story526GeometryAndLocalizationTests.cs:1455` -- precedent de scan source et « aucun asmdef sous Traffic ». `Story525RoadWorldModelTests.cs:1646` : litteral scinde.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs:1390` -- `ReplayRoute`, scenario de rejeu a reprendre sans l'algorithme.
- `docs/setup/story-5-25-road-world-model-notes.md:21` -- la 5.30 y est le declencheur de reexamen de l'assembly.
- `scripts/validation-profiles.ps1:71` -- `Traffic/**` hors `Routing/` est classe Geometry, donc `Auto` -> `Full`. Ne pas modifier.

## Tasks & Acceptance

**Execution:**
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs` -- frame immuable, acteurs et builder deterministe avec localisation integree -- BC-1.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Perception/AgentObservation.cs` -- observation minimale.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/PlanningTolerances.cs`, `PathHorizon.cs`, `GateAEvidenceBinding.cs`, `MotionPlan.cs` -- tolerances, horizon, raccords, liaison documentaire, couverture a deux niveaux, contraintes publiees et verificateur de profil.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- projection et `ToText()`.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs` -- orchestrateur sans etat.
- [ ] `docs/setup/story-5-25-road-world-model-notes.md` -- section « Confirmation 5.30 » avec la direction de dependance mesuree.
- [ ] `Assets/RoadRage/Tests/EditMode/Story530PlanningSpineTests.cs` `[Category("Core")]` -- couvrir :
  - la matrice sur modeles et textes synthetiques, dont les alterations d'un octet du bloc, du modele et de la version ;
  - la permutation d'ordre et l'immuabilite de la frame ;
  - trois contrats distincts, et aucun membre entier cible/waypoint dans `PathHorizon` ou `MotionPlan` ;
  - une liaison valide sur les artefacts commites reels ;
  - un scan source et une reflexion sur les nouveaux dossiers : aucun `VehicleDriveIntent`, `VehiclePhysicsBody`, `ApplyDriveIntent`, `Rigidbody`, `MonoBehaviour`, `NetworkVariable`, `Rpc`, et aucune lecture de `LateralClearanceMarginMeters`.
- [ ] `Assets/RoadRage/Tests/EditMode/Story530PlanningReplayTests.cs` `[Category("Geometry")]` -- sur le modele reel, rejeu cinematique de chaque entree jusqu'au portail : pose placee sur l'horizon, nouvelle frame, reutilisation ou replanification. Chaque raccord et intervalle traverse respecte les tolerances. Ensemble des discontinuites d'anneau epingle par (mouvement, cote). Reference couverte a chaque pas.
- [ ] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- statut de la story. Puis `graphify update .`.

**Acceptance Criteria:**
- Given un cycle de decision hote, when la chaine s'execute, then elle lit une seule frame immuable, independante de l'ordre d'appel, et produit trois contrats distincts, aucun n'etant represente par un index de cible.
- Given chaque entree de `MVP_Run` et la liaison aux artefacts signes, when le rejeu avance jusqu'a la sortie, then chaque horizon respecte les tolerances, porte v*(s), a sa reference couverte et son vehicule `NotEstablished`.
- Given une preuve absente, alteree ou perimee, when un plan est evalue, then il est infaisable avec un code nomme et aucun artefact signe n'est ecrit.
- Given une frame decidee, when sa projection est inspectee, then le texte, indexe sur la frame, contient les lignes du contrat de debug 5.30 et les statuts de couverture, et la frame est identique avant et apres.
- Given l'implementation, when elle est revue, then aucun type ne compose ou n'applique d'intent ni ne genere de profil de vitesse, aucun asmdef n'est ajoute et la confirmation d'assembly est consignee.

## Spec Change Log

## Design Notes

**Decisions proprietaire (2026-09-29, planification).**
- Raccords : |Δκ| ≤ 1e-3 m⁻¹ avec les 24 raccords tangents d'anneau epingles comme exceptions publiees.
- Jerk : interpretation geometrique |dκ/ds| ≤ 0,30 m⁻².
- Spec gardee entiere.
- Assembly unique confirme.
- Pas d'overlay visuel.

**Mesures sur le modele signe (2026-09-29).**
- 144 raccords : ecart 0 m, cap ≤ 0,019°.
- 120 raccords ont |Δκ| ≤ 1e-7 m⁻¹.
- 24 raccords entree/sortie ↔ anneau ont |Δκ| = 1/6 m⁻¹ : raccord tangent 5.50, publie par P25 C3.
- |dκ/ds| max : 0,2845 m⁻² sur les giratoires, 0,0955 m⁻² sur les carrefours.
- v* min : 0,25 m/s sur les mouvements, 12,85 m/s sur les corridors.
- Les seuils 1e-3 et 0,30 sont des fils de declenchement calibres, pas une preuve de capacite.

**Couverture et Gate B.**
- *Fait.* La preuve signee a ete calculee sur la reference compilee avec a_e = 0 (C:416). a_e est lie a la signature via `ClearanceHash`, verifie.
- *Fait.* En 5.30 o(s) ≡ 0, donc le chemin planifie est cette reference meme, et sa couverture vaut par construction. Il ne s'agit pas d'une preuve sur le vehicule.
- *Consequence 5.31, deja au contrat signe* (C:417-419, `epics.md:2698-2699`) : tout ε_t > 0 mesure rend la trajectoire non couverte. Suivent alors la regeneration des candidats et du degagement avec l'allocation max|o| + ε_t, les decisions du proprietaire sur les paires, puis une nouvelle signature Gate A. La 5.31 n'est pas terminee sans couverture, donc Gate B passe tres probablement par une re-signature Gate A.
- *Estimation, non verifiee.* Residu signe minimal ≈ 0,1127 m (physique et Sidewalk, carrefours classiques). Si l'allocation s'ajoute au rayon d'empreinte comme δ_c, un ε_t au-dela d'environ 0,11 m rendrait un residu non positif, donc un echec geometrique. La valeur exacte depend de la maniere dont la 5.31 injectera l'allocation dans le balayage.
- La marge de 0,25 m reste reservee (C:415). Elle ne sert jamais a absorber ε_t.

**Verificateur de profil.**
- Il est sur par construction :
  - v² lineaire entre deux points donne v monotone ;
  - κ lineaire entre deux echantillons donne un v*, decroissant en |κ|, minimal a une borne ;
  - comparer max(v) aux bornes a min(v*) aux bornes ne manque donc aucun depassement.
- Il est conservateur : un profil tangent au plafond a l'interieur d'un sous-intervalle peut etre refuse.
- Le freinage anticipe est juge dans l'horizon seulement. L'obligation au-dela de l'horizon appartient au planificateur 5.31.

**Assembly.** La 5.30 lit `DriverProfile`, un type V1 retenu. Un assembly Traffic separe devrait donc referencer une feature, ce que la convention interdit, ou deplacer `DriverProfile`, ce que `epics.md:1676` exclut. L'oracle 5.24 exige V1 et V2 dans la meme portee.

**Portee runtime.** Le rapport vit dans `_bmad-output`, hors build. Le transport de la liaison au runtime est donc a trancher en 5.31. La 5.30 n'a pas de chemin runtime.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story530PlanningSpineTests"` -- expected: fixture verte, 0 erreur Console.
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story530PlanningReplayTests"` -- expected: rejeu vert sur les 4 entrees.
- `.\scripts\validate.ps1 -Profile Full` -- expected: suite EditMode complete verte, dont `GateAIsOpenedOnlyByTheOwnersBoundSignoff`. Story de contrat, donc profil Full.
