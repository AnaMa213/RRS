---
title: 'Story 5.30 -- Fondation de la chaine de planification et de l epine runtime Traffic V2'
type: 'feature'
created: '2026-09-29'
status: 'draft'
review_loop_iteration: 0
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le modele signe est routable (5.29) mais aucune chaine ne relie une frame hote a une geometrie locale suivable. Sans preuve isolee de la separation route / chemin / mouvement, la 5.31 deboguerait la planification a travers la physique.

**Approach:** Construire une chaine pure cote hote : `TrafficFrame` immuable -> `AgentObservation` minimale -> `RoutePlan` (5.29) -> `PathHorizon` continu lu sur la reference compilee -> `MotionPlan` geometrique portant v*(s), la faisabilite et la couverture Gate A -> projection de decision textuelle indexee sur la frame. Aucun intent, aucune physique : le controleur V1 reste seul a conduire.

## Boundaries & Constraints

**Always:**
- Modele admis seulement via `RoadModelDocument.Load` puis `RoadModelCompiler.Compile`. L'orchestrateur refuse tout `CompiledRoadModel` dont `DrivabilityProfile.Declared` est faux (`UndeclaredDrivabilityProfile`).
- `TrafficFrame` : `FrameId` fourni par l'appelant, `ModelId` et `Version`, acteurs tries par `TrafficId`. Chaque acteur porte son `TrafficId`, sa `VehicleFootprintPose` (ancre d'empreinte), sa vitesse, son element precedent et sa `RoadLocation`, calculee une fois a la construction par `RoadLocalizer.Localize`. Collections copiees et exposees en lecture seule. L'ordre d'entree est sans effet. Un `TrafficId` vide ou duplique est refuse.
- Une decision lit exactement une frame. L'orchestrateur est sans etat : entrees (frame, `TrafficId`, `RoutePlan` precedent, sortie visee, graine de session, `lookAheadMeters`, ε_t) -> `PlanningDecision`. L'appelant conserve le `RoutePlan`.
- `AgentObservation` : `FrameId`, `TrafficId`, `RoadLocation`, vitesse tangentielle, empreinte. Rien d'autre.
- Route : `RoutePlanner.Plan` inchange, domaine `"route"`, reutilisation via `existing`.
- `PathHorizon` : intervalles (kind, id, s0, s1) pris dans les occurrences du `RoutePlan`, de la progression jusqu'a min(portail de sortie, s + `lookAheadMeters`). Il echantillonne via le `RoadCurve.Sample` de chaque element, jamais via une courbe reconstruite, lissee ou reechantillonnee. o(s) ≡ 0. Il publie par raccord : ecart de position, saut de cap et saut de courbure (s, Δκ, v* des deux cotes). Il publie aussi |dκ/ds| max par intervalle.
- Tolerances declarees, dans une seule source de constantes nommees :
  - raccord : ecart ≤ `SeamGapToleranceMeters` et cap ≤ `SeamTangentToleranceDegrees`, relus du `ValidationProfile` ;
  - raccord : |Δκ| ≤ 1e-3 m⁻¹. Seule exception : les raccords tangents d'anneau signes, publies comme discontinuites, avec v* au raccord = min des deux cotes ;
  - dans un element : |dκ/ds| ≤ 0,30 m⁻².
- `MotionPlan` :
  - porte κ(s) et v*(s) = `RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profil, κ)` ;
  - porte les bornes d'acceleration `DriverProfile.MaxAcceleration` / `SafeBrakingLimit` ;
  - porte max|o(s)|, ε_t (0 jusqu'a la 5.31) et le a_e signe (constante runtime 0) ;
  - couverture : couvert ⇔ max|o| + ε_t ≤ a_e, sinon `NotCoveredByGateA` ;
  - un v* NaN ou inferieur a `SteeringInactiveBelowMetersPerSecond` rend le plan infaisable ;
  - l'evaluation d'un profil de vitesse candidat {(s, v)} nomme le premier s en defaut : v > v* donne `SteeringCeilingExceeded` ; v·dv/ds hors [−`SafeBrakingLimit`, `MaxAcceleration`] donne `AccelerationBoundExceeded`.
- `TrafficDecisionProjection` :
  - immuable ;
  - contenu : `FrameId`, `RoadModelVersion`, `TrafficId`, element et s, resultat de route (outcome, raison, portail, IDs), prochain mouvement, `PathPlanId` / `MotionPlanId`, faisabilite et codes stables ;
  - `ToText()` deterministe et invariant de culture ;
  - produite par l'evaluation hote seule. La frame n'est jamais modifiee.
- La decision d'assembly de la 5.25 (aucun nouvel assembly) est confirmee, et la preuve est consignee.

**Ask First:**
- Modifier le modele `MVP_Run`, son sign-off, sa geometrie, `Traffic/Migration/**` ou `scripts/validation-profiles.ps1`.
- Changer une tolerance declaree.
- Toucher un type V1 retenu.
- Ajouter un asmdef, une dependance, une `NetworkVariable` ou une RPC.

**Never:**
- Composer un `VehicleDriveIntent`, appeler `VehiclePhysicsBody` ou creer un second chemin de controle.
- Utiliser `MonoBehaviour`, `Rigidbody`, `Transform` ou Netcode dans les nouveaux dossiers.
- Hors perimetre : profil de vitesse planifie, `SafetyFilter`, cycle de vie, portails, occupation, coordination.
- Reprendre `WaypointIndex`, `arrivalRadius` ou `ResolveLookAheadPoint`.
- Re-ajuster la reference compilee.
- Ecrire dans l'overlay Gate A (son texte est hashe dans le sign-off) ou modifier `AIVehicleBehaviorDebugView`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Nominal | Frame valide, acteur localise, sortie atteignable | Route, horizon continu, plan faisable et couvert, projection complete | N/A |
| Modele non declare | `Declared = false` | Aucune decision | `UndeclaredDrivabilityProfile` |
| Acteur non localise / `NoRoute` | Localisation invalide ou sortie inaccessible | Projection avec outcome et raison de route, sans horizon ni plan | Aucun effet monde |
| Plan obsolete | Version ou ID perime, ID repete apres boucle | `Replanned` + `StalePlan` | Aucun effet |
| Raccords | Corridor ↔ mouvement, anneau | Continus dans les tolerances. Discontinuites d'anneau publiees | Autre saut : horizon non conforme |
| Couverture | ε_t > a_e | Plan infaisable | `NotCoveredByGateA` |
| Profil candidat | v > v*(s), ou acceleration hors bornes | Infaisable, s nomme | `SteeringCeilingExceeded` / `AccelerationBoundExceeded` |
| Ordre | Acteurs ou vehicules dans un autre ordre | Frame et projections identiques | N/A |
| Entrees invalides | `TrafficId` vide/duplique, `lookAheadMeters` ≤ 0 ou non fini, valeur non finie | Refus explicite | Code stable |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelDocument.cs:71,119` -- `Load(string)` renvoie un `RoadModelSource` et leve `FormatException` si le profil n'est pas declare. Compiler ensuite avec `RoadModelCompiler.Compile` (`RoadModelCompiler.cs:103`).
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs:63` -- `SteeringSpeedCeilingMetersPerSecond(profile, κ)` : +∞ si κ = 0 ou verrou ≤ 16°, NaN si R ≤ 1,55 m. Il n'existe pas de fonction de s : l'evaluer sur `Sample(s).CurvaturePerMeter`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadCurve.cs:66,114` -- `Sample(s)` est l'evaluation canonique : position et κ lineaires, tangente normalisee. `RoadCurvePoint` est a :13, κ est signe.
- `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs:414-425,484,510` -- `ModelId`, `Version`, profils, `TryGetCorridor` et `TryGetMovement`. `.Curve` de chaque element. Pas de recherche de portail par ID : parcourir `Portals`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs:643,714` -- `RoadModelValidationProfile` (seams 0,05 m / 5°, demi-largeur 1,03, marge 0,25) et `DrivabilityProfile`.
- `Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs:13,24,88,141` -- `VehicleFootprint`, `VehicleFootprintPose`, `RoadLocation` et `RoadLocalizer.Localize(model, pose, previousElementId, routeElementIds)`, qui est pur.
- `Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:32,309` -- `Plan(...)`. La reutilisation via `existing` avance la progression, mais echoue sur un ID repete. `RoutePlan.cs:14,30,83` : occurrences [s0, s1], `ProgressOccurrenceIndex`, `RouteResult`. Pas d'accesseur de prochain mouvement.
- `Assets/RoadRage/Features/Vehicles/DriverProfile.cs:141,147,165` -- `MaxAcceleration`, `ComfortableDeceleration`, `SafeBrakingLimit`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs:1313` -- `TrackingAllowanceMeters = 0f`, Editeur seulement : a_e de la preuve signee. Lecture seule.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/GateAReviewWindow.cs:19` -- shell overlay 5.28. Ne pas l'etendre ici (`OverlayHash`, `AuthoredRoadModel.cs:310`).
- `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json` -- 44 corridors, 72 mouvements, 4 entrees et 4 sorties. Chargement de test : `Story529RoutePlanTests.cs:406`.
- `Assets/RoadRage/Tests/EditMode/Story526GeometryAndLocalizationTests.cs:1455` -- precedent de scan source pour les jetons interdits, et assertion « aucun asmdef sous Traffic ». `Story525RoadWorldModelTests.cs:1646` montre l'astuce du litteral scinde.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs:1390` -- `ReplayRoute`, precedent de rejeu cinematique. Reprendre le scenario, pas l'algorithme.
- `docs/setup/story-5-25-road-world-model-notes.md:21` -- la 5.30 y est nommee comme declencheur de reexamen de l'assembly.
- `scripts/validation-profiles.ps1:71` -- tout `Traffic/**` hors `Routing/` est classe Geometry, donc `Auto` -> `Full`. Ne pas modifier.

## Tasks & Acceptance

**Execution:**
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs` -- frame immuable, acteurs et builder deterministe avec localisation integree -- BC-1.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Perception/AgentObservation.cs` -- observation minimale d'un vehicule.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/PlanningTolerances.cs`, `PathHorizon.cs`, `MotionPlan.cs` -- tolerances et a_e comme constantes uniques, horizon, rapports de raccords, faisabilite, couverture et evaluation d'un profil candidat.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- projection et `ToText()`.
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs` -- orchestrateur sans etat, avec refus du modele non declare.
- [ ] `docs/setup/story-5-25-road-world-model-notes.md` -- section « Confirmation 5.30 » avec la direction de dependance mesuree.
- [ ] `Assets/RoadRage/Tests/EditMode/Story530PlanningSpineTests.cs` `[Category("Core")]` -- couvrir :
  - la matrice ci-dessus sur modeles synthetiques ;
  - la permutation d'ordre ;
  - l'immuabilite de la frame ;
  - trois contrats distincts, et aucun membre entier cible/waypoint dans `PathHorizon` ou `MotionPlan` ;
  - a_e runtime = `AuthoredRoadModel.TrackingAllowanceMeters` ;
  - un scan source et une reflexion sur les nouveaux dossiers : aucun `VehicleDriveIntent`, `VehiclePhysicsBody`, `ApplyDriveIntent`, `Rigidbody`, `MonoBehaviour`, `NetworkVariable`, `Rpc`.
- [ ] `Assets/RoadRage/Tests/EditMode/Story530PlanningReplayTests.cs` `[Category("Geometry")]` -- sur le modele reel :
  - rejeu cinematique de chaque entree vers les sorties : pose placee sur l'horizon, relocalisation par une nouvelle frame, reutilisation ou replanification, jusqu'au portail ;
  - toutes les tolerances sur chaque raccord et chaque intervalle traverses ;
  - ensemble des discontinuites d'anneau epingle par (mouvement, cote).
- [ ] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- statut de la story. Puis `graphify update .` apres les sources.

**Acceptance Criteria:**
- Given un cycle de decision hote, when la chaine s'execute, then elle lit une seule frame immuable dont l'identite, la version et l'ordre sont independants de l'ordre d'appel, et produit trois contrats distincts, aucun n'etant represente par un index de cible.
- Given chaque entree de `MVP_Run`, when le rejeu cinematique avance jusqu'a la sortie, then chaque horizon successif respecte les tolerances declarees, porte v*(s) et le plan reste couvert par la preuve Gate A signee.
- Given une frame decidee, when sa projection est inspectee, then le texte, indexe sur la frame, contient toutes les lignes du contrat de debug de la 5.30, et la frame est identique avant et apres.
- Given l'implementation, when elle est revue, then aucun type ne compose ou n'applique d'intent, aucun asmdef n'est ajoute et la confirmation d'assembly est consignee.

## Spec Change Log

## Design Notes

**Mesures sur le modele signe (2026-09-29).**
- Sur 144 raccords, l'ecart est de 0 m et le cap au plus 0,019°.
- 120 raccords ont |Δκ| ≤ 1e-7 m⁻¹.
- Les 24 raccords entree/sortie ↔ anneau des quatre giratoires ont |Δκ| = 1/6 m⁻¹ : le mouvement finit a κ = 0, l'anneau de 6 m commence a −1/6. La 5.50 a produit un raccord tangent, pas continu en courbure ; P25 C3 publie ces sauts et confie leur tolerance a la 5.30.
- |dκ/ds| max : 0,2845 m⁻² sur les giratoires, 0,0955 m⁻² sur les carrefours.
- v* min : 0,25 m/s sur les mouvements (exactement la regle d'admission), 12,85 m/s sur les corridors.

Le « jerk » d'un chemin sans vitesse est interprete geometriquement comme la pente |dκ/ds|. Le jerk temporel appartient a la planification de vitesse (5.31/5.33). La borne de 0,30 m⁻² est un fil de declenchement calibre sur la geometrie signee, pas une preuve de capacite du vehicule.

**Decisions proprietaire (2026-09-29, planification).**
- Raccords : tolerance stricte |Δκ| ≤ 1e-3 m⁻¹, avec les 24 raccords tangents d'anneau epingles comme exceptions publiees. Les options « tolerance unique 0,17 m⁻¹ » et « stricte sans exception » (correct-course sur la geometrie) ont ete ecartees.
- Jerk : interpretation geometrique |dκ/ds| ≤ 0,30 m⁻².
- Spec gardee entiere (~4 500 tokens) malgre la cible de 1 600.

**Couverture Gate A.** Le a_e signe n'existe qu'en constante Editeur, et aucun runtime ne lit le sign-off. On duplique donc la valeur en constante runtime, et un test d'egalite empeche la derive. Modifier `AuthoredRoadModel` toucherait l'outillage qui produit la preuve signee. Comme o(s) ≡ 0 et ε_t = 0, tout horizon de la 5.30 est couvert. Le cas non couvert est teste avec un ε_t synthetique.

**Assembly.** La 5.30 ne compose pas d'intent, mais elle lit `DriverProfile`, un type V1 retenu de `RoadRage.Features.Vehicles`. Un assembly Traffic separe devrait donc referencer une feature, ce que la convention interdit, ou deplacer `DriverProfile` vers Shared. Or `epics.md:1676` n'autorise que le deplacement de `VehicleDriveIntent`. L'isolation de test reste negative : l'oracle 5.24 exige V1 et V2 dans la meme portee. Conclusion : confirmer l'assembly unique.

**Hote seul a la 5.30.** Aucun chemin runtime n'existe encore. « Produite par l'hote, lecture seule chez les clients » se traduit ici par une projection immuable, produite par la seule evaluation hote, sans aucun transport reseau ajoute. La presentation client et l'overlay visuel viennent avec le vehicule V2 (5.31 et suivantes).

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story530PlanningSpineTests"` -- expected: fixture verte, 0 erreur Console.
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story530PlanningReplayTests"` -- expected: rejeu vert sur les 4 entrees.
- `.\scripts\validate.ps1 -Profile Full` -- expected: suite EditMode complete verte. Story de contrat, donc profil Full obligatoire.
