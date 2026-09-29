---
title: 'Story 5.30 -- Fondation de la chaine de planification et de l epine runtime Traffic V2'
type: 'feature'
created: '2026-09-29'
status: 'done'
baseline_commit: '883a07342711657153e5da2a5bff77d1721df4fa'
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
  - La vitesse tangentielle est **signee** le long de l'avant de l'empreinte (marche arriere ou poussee < 0). Le signe est conserve ; seule une valeur non finie est refusee (`InvalidSpeed`). Elle n'est pas la vitesse de progression du plan : ni la route, ni l'horizon, ni le mouvement ne la lisent.
  - Collections copiees et exposees en lecture seule. L'ordre d'entree est sans effet. Un `TrafficId` vide ou duplique est refuse.
- **Orchestrateur.** Il lit une seule frame par decision et ne garde aucun etat.
  - Entrees : frame, `TrafficId`, `RoutePlan` precedent, sortie visee, graine de session, `lookAheadMeters`, liaison de preuve Gate A, bornes longitudinales.
  - Sortie : `PlanningDecision`. L'appelant conserve le `RoutePlan`.
- **`AgentObservation`.** `FrameId`, `TrafficId`, `RoadLocation`, vitesse tangentielle signee, empreinte. Rien d'autre.
- **Route.** `RoutePlanner.Plan` inchange, domaine `"route"`, reutilisation via `existing`.
- **`PathHorizon`.**
  - Intervalles (kind, id, s0, s1) pris dans les occurrences du `RoutePlan`, de la progression jusqu'a min(portail de sortie, s + `lookAheadMeters`). La fin est qualifiee : `ExitPortal` ou `LookAheadLimit`.
  - Echantillonnage via le `RoadCurve.Sample` de chaque element, jamais via une courbe reconstruite, lissee ou reechantillonnee. o(s) ≡ 0.
  - Publie par raccord : ecart, saut de cap et saut de courbure (s, Δκ, v* des deux cotes). Publie aussi |dκ/ds| max par intervalle.
- **Tolerances declarees** (une seule source de constantes nommees) :
  - raccord : ecart ≤ `SeamGapToleranceMeters` et cap ≤ `SeamTangentToleranceDegrees`, relus du `ValidationProfile` ;
  - raccord : |Δκ| ≤ 1e-3 m⁻¹. Seule exception : les 24 raccords tangents d'anneau signes, publies comme discontinuites. En ces points, v* = min des deux cotes ;
  - dans un element : |dκ/ds| ≤ 0,30 m⁻² ;
  - verificateur de profil : bornes d'acceleration et de freinage comparees avec `AccelerationBoundToleranceMetersPerSecondSquared` = 1e-3 m/s², et couverture de l'horizon avec `ProfileSpanToleranceMeters` = 1e-3 m (voir Design Notes, « Tolerances du verificateur »).
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
    - **Plan infaisable.** Si `MotionPlan.Issue` n'est pas `None`, aucun candidat n'est juge : le resultat est `PlanInfeasible`, jamais `None`, et `PlanIssue` porte la cause exacte du plan (`HorizonNonConforming`, `InvalidSteeringCeiling`, `GateAEvidenceMissing`, `GateAEvidenceStale` ou `NotCoveredByGateA`) avec sa distance. Cette verification precede celle du candidat.
    - Entrees : s strictement croissant, dans l'horizon ; v fini et ≥ 0. Sinon → `InvalidSpeedProfile`.
    - **Couverture.** Le candidat doit couvrir tout l'horizon : premier s ≤ `ProfileSpanToleranceMeters`, dernier s ≥ longueur − `ProfileSpanToleranceMeters`. Un candidat incomplet → `InvalidSpeedProfile` (defaut propre au candidat), a s = 0 s'il commence tard, au dernier s s'il s'arrete court.
    - Entre deux points, v² est lineaire en s. On en tire a = (v₁² − v₀²)/(2Δs), qui doit rester dans [−`MaxDeceleration`, `MaxAcceleration`] a `AccelerationBoundToleranceMetersPerSecondSquared` pres. Sinon → `AccelerationBoundExceeded` au premier s fautif.
    - Plafond : evalue sur l'union des points du profil, des echantillons de courbe et des raccords. Sur chaque sous-intervalle, max(v) aux bornes doit etre ≤ min(v*) aux bornes. Sinon → `SteeringCeilingExceeded` au premier s fautif.
    - v = 0 est admis (arret, redemarrage).
    - 0 < v < `SteeringInactiveBelowMetersPerSecond` sur une portion courbe → diagnostic `SteeringInactiveSpan`, pas un echec.
    - Fin d'horizon par `LookAheadLimit` → diagnostic `HorizonTruncated`. Rien n'est juge au-dela.
  - (3) **Faisabilite physique** (adherence, taux de braquage, ε_t mesure) : hors 5.30. Aucun resultat n'est nomme « physiquement faisable ».
- **`TrafficDecisionProjection`.**
  - Immuable. Contenu : `FrameId`, `RoadModelVersion`, `TrafficId`, element et s, route (outcome, raison, portail, IDs), prochain mouvement, `PathPlanId` / `MotionPlanId`, statuts de liaison et de couverture, codes stables.
  - Sans plan de route (`NoRoute`, localisation invalide), la liaison, la couverture et le plafond n'ont pas ete evalues : ils sont `null` dans la projection et s'impriment « non evalue », jamais `GateAEvidenceMissing` ni « aucun ».
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
| Preuve par defaut | `default(GateAEvidenceResult)` | Jamais une preuve valide | `GateAEvidenceMissing` (valeur 0) |
| ε_t declare | 0,01 m (a_e = 0) ; ε_t < marge de 0,25 m | Reference non couverte dans les deux cas | `NotCoveredByGateA` |
| Localisation / `NoRoute` / plan obsolete | Invalide, inaccessible, perime ou ID repete | Outcome et raison de route ; replan `StalePlan` | Aucun effet monde |
| Raccords | Corridor ↔ mouvement, anneau | Continus ; discontinuites d'anneau publiees avec v* min | Autre saut : horizon non conforme |
| Profil : plafond | Depassement entre deux points grossiers du profil ; au raccord entre les v* des deux cotes ; egal au plafond a un echantillon | Detecte ; detecte ; admis | `SteeringCeilingExceeded` + s |
| Profil : freinage | Deceleration = borne ; ecart d'arrondi ≤ 1e-3 m/s² ; au-dela | Admis ; admis ; refuse au premier s fautif | `AccelerationBoundExceeded` |
| Profil sur plan infaisable | Toute cause de `MotionPlan.Issue` ≠ `None` | Refus, jamais `None`, cause exacte conservee | `PlanInfeasible` + `PlanIssue` |
| Profil incomplet | Premier s > 1e-3 m ; dernier s < longueur − 1e-3 m | Refus | `InvalidSpeedProfile` |
| Profil : vitesses basses | v = 0 (arret, redemarrage) ; 0 < v < 0,25 en courbe | Admis ; admis avec diagnostic | `SteeringInactiveSpan` |
| Profil : v* non borne | Ligne droite a vitesse elevee | Admis, plafond « aucun » | N/A |
| Profil invalide | v < 0 ou non fini, s non croissant ou hors horizon | Refus | `InvalidSpeedProfile` |
| Ordre | Acteurs ou vehicules permutes | Frame et projections identiques | N/A |
| Vitesse signee | Vitesse tangentielle < 0 (marche arriere) ; NaN, +∞ ou −∞ | Signe conserve, plan inchange ; refus | `InvalidSpeed` |
| Sans plan de route | `NoRoute`, localisation invalide | Statuts de liaison, couverture et plafond « non evalue » | N/A |
| Entrees invalides | `TrafficId` vide/duplique/inconnu, `lookAheadMeters` ≤ 0 ou non fini | Refus explicite | Code stable |

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
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs` -- frame immuable, acteurs et builder deterministe avec localisation integree -- BC-1.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Perception/AgentObservation.cs` -- observation minimale.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Planning/PlanningTolerances.cs`, `PathHorizon.cs`, `GateAEvidenceBinding.cs`, `MotionPlan.cs` -- tolerances, horizon, raccords, liaison documentaire, couverture a deux niveaux, contraintes publiees et verificateur de profil.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs` -- projection et `ToText()`.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs` -- orchestrateur sans etat.
- [x] `docs/setup/story-5-25-road-world-model-notes.md` -- section « Confirmation 5.30 » avec la direction de dependance mesuree.
- [x] `Assets/RoadRage/Tests/EditMode/Story530PlanningSpineTests.cs` `[Category("Core")]` -- couvrir :
  - la matrice sur modeles et textes synthetiques, dont les alterations d'un octet du bloc, du modele et de la version ;
  - la permutation d'ordre et l'immuabilite de la frame ;
  - trois contrats distincts, et aucun membre entier cible/waypoint dans `PathHorizon` ou `MotionPlan` ;
  - une liaison valide sur les artefacts commites reels ;
  - un scan source et une reflexion sur les nouveaux dossiers : aucun `VehicleDriveIntent`, `VehiclePhysicsBody`, `ApplyDriveIntent`, `Rigidbody`, `MonoBehaviour`, `NetworkVariable`, `Rpc`, et aucune lecture de `LateralClearanceMarginMeters`.
- [x] `Assets/RoadRage/Tests/EditMode/Story530PlanningReplayTests.cs` `[Category("Geometry")]` -- sur le modele reel, rejeu cinematique de chaque entree jusqu'au portail : pose placee sur l'horizon, nouvelle frame, reutilisation ou replanification. Chaque raccord et intervalle traverse respecte les tolerances. Ensemble des discontinuites d'anneau epingle par (mouvement, cote). Reference couverte a chaque pas.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- statut de la story. Puis `graphify update .`.

**Acceptance Criteria:**
- Given un cycle de decision hote, when la chaine s'execute, then elle lit une seule frame immuable, independante de l'ordre d'appel, et produit trois contrats distincts, aucun n'etant represente par un index de cible.
- Given chaque entree de `MVP_Run` et la liaison aux artefacts signes, when le rejeu avance jusqu'a la sortie, then chaque horizon respecte les tolerances, porte v*(s), a sa reference couverte et son vehicule `NotEstablished`.
- Given une preuve absente, alteree ou perimee, when un plan est evalue, then il est infaisable avec un code nomme et aucun artefact signe n'est ecrit.
- Given une frame decidee, when sa projection est inspectee, then le texte, indexe sur la frame, contient les lignes du contrat de debug 5.30 et les statuts de couverture, et la frame est identique avant et apres.
- Given l'implementation, when elle est revue, then aucun type ne compose ou n'applique d'intent ni ne genere de profil de vitesse, aucun asmdef n'est ajoute et la confirmation d'assembly est consignee.

## Spec Change Log

- **2026-09-29 -- revue de code (`bmad-code-review`), decisions proprietaire D1 = b, D2 = a, D3 = a.** Bloc fige renegocie par le proprietaire :
  - *D1.* La vitesse tangentielle de `TrafficFrame` / `AgentObservation` devient signee ; seule une valeur non finie est refusee.
  - *D2.* `VerifySpeedProfile` refuse un candidat incomplet (`InvalidSpeedProfile`) et ne rend jamais `None` sur un plan infaisable : nouveau code `SpeedProfileIssue.PlanInfeasible` (ajoute en fin d'enum) et champ `SpeedProfileResult.PlanIssue` qui conserve la cause exacte du `MotionPlan`. Le contrat de codes fige est ajuste par ce seul ajout ; aucun code existant ne change de sens.
  - *D3.* Tolerance nommee de 1e-3 m/s² sur les comparaisons d'acceleration et de freinage (`PlanningTolerances`), et de 1 mm sur la couverture de l'horizon.
  - *Patch.* `GateAEvidenceStatus` place `GateAEvidenceMissing` a 0. Verifie avant la reorganisation : l'enum n'est ni serialise, ni hache, ni reference par un asset ; le sign-off ne porte que `RoadModelVersion`, `ModelHash` et `ClearanceHash` (textes), et aucune empreinte signee ne depend de ces valeurs.

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

**Tolerances du verificateur (decision proprietaire, revue du 2026-09-29).**
- a = (v₁² − v₀²)/(2Δs) est calcule en double a partir de flottants : Δs est une soustraction flottante et un profil genere arrondit v en flottant. L'erreur relative ~1e-7 sur v²/(2Δs) reste sous 1e-3 m/s² tant que v²/(2Δs) < ~8 000 m/s².
- 1e-3 m/s² (0,01 % de g) est tres en dessous de tout depassement de borne physiquement distinct : un profil a 0,002 m/s² au-dessus de la borne reste refuse.
- La couverture de l'horizon tolere 1 mm, soit cinquante fois moins que le raccord tolere (0,05 m) : elle absorbe l'arrondi flottant de la longueur sans laisser une portion jugeable hors profil.
- Prouve par test : borne exacte admise, erreur d'arrondi simulee (2 ulp) admise, depassements de 2e-3, 1e-2 et 1 m/s² refuses.

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

**Execute le 2026-09-29 :** fixture Core 8/8 ; profil Full 950/950 EditMode, 0 erreur Console depuis le curseur 1275, compilation saine, aucun test exclu ou ignore, `MVP_Run` propre. `graphify update .` execute ; le graphe conserve un depassement preexistant de la limite utile (5337 noeuds avant, 5519 apres).

**Apres la revue du 2026-09-29 (decisions D1 = b, D2 = a, D3 = a et 9 patches) :** profil Full 965/965 EditMode (950 + 15 nouveaux tests Core), 0 erreur Console depuis le curseur 1815, compilation saine, aucun test exclu ou ignore, `MVP_Run` propre. Fixture Core `Story530PlanningSpineTests` 23/23. `graphify update .` execute (5 547 noeuds, depassement preexistant deja reporte). Aucune preuve, signature ni artefact Gate A modifie : `MVP_Run.road-model.json`, `MVP_Run.road-signoff.json` et `migration-report-5-28-mvp-run.md` sont inchanges.

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story530PlanningSpineTests"` -- expected: fixture verte, 0 erreur Console.
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story530PlanningReplayTests"` -- expected: rejeu vert sur les 4 entrees.
- `.\scripts\validate.ps1 -Profile Full` -- expected: suite EditMode complete verte, dont `GateAIsOpenedOnlyByTheOwnersBoundSignoff`. Story de contrat, donc profil Full.

## Suggested Review Order

**Chaine de decision**

- Une evaluation pure relie observation, route, horizon, mouvement et projection.
  [PlanningSpine.cs:61](../../Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs#L61)

- La frame trie et copie les acteurs avant une localisation unique.
  [TrafficFrame.cs:43](../../Assets/RoadRage/Features/Vehicles/Traffic/Frame/TrafficFrame.cs#L43)

**Geometrie et couverture**

- L'horizon suit exclusivement les occurrences et courbes compilees; seuls les raccords signes derogent.
  [PathHorizon.cs:74](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/PathHorizon.cs#L74)

- La preuve documentaire lie version et empreintes des trois textes signes.
  [GateAEvidenceBinding.cs:33](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/GateAEvidenceBinding.cs#L33)

- Le plan distingue contraintes geometriques, couverture de reference et couverture vehicule.
  [MotionPlan.cs:58](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs#L58)

- Le verificateur controle un profil fourni sans produire de vitesses.
  [MotionPlan.cs:107](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs#L107)

**Lecture et preuves**

- La projection fournit une sortie textuelle stable indexee sur la frame.
  [TrafficDecisionProjection.cs:10](../../Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs#L10)

- Les tests Core couvrent les contrats, les erreurs et la liaison Gate A.
  [Story530PlanningSpineTests.cs:50](../../Assets/RoadRage/Tests/EditMode/Story530PlanningSpineTests.cs#L50)

- Le rejeu Geometry traverse quatre entrees et epingle les 24 exceptions d'anneau.
  [Story530PlanningReplayTests.cs:41](../../Assets/RoadRage/Tests/EditMode/Story530PlanningReplayTests.cs#L41)

- La note explique le maintien de l'assembly existant.
  [story-5-25-road-world-model-notes.md:27](../../docs/setup/story-5-25-road-world-model-notes.md#L27)

### Review Findings

Revue du 2026-09-29 (`bmad-code-review`, mode full, diff `883a073..7d0b727` hors `graphify-out/` et `.meta`). Couches : Blind Hunter, Edge Case Hunter, Verification Gap, Acceptance Auditor. Aucun ecart bloquant aux cinq AC ; les constats portaient sur la fermeture par defaut, la lisibilite du diagnostic et des trous de preuve.

Decisions proprietaire du 2026-09-29 : **D1 = b, D2 = a, D3 = a**, avec autorisation d'appliquer les 9 patches. Resolution ci-dessous, constat par constat ; les tests nommes sont dans `Story530PlanningSpineTests` (Core) sauf mention contraire.

- [x] [Review][Decision] `TrafficFrame` refuse toute vitesse tangentielle negative (`InvalidSpeed`) [TrafficFrame.cs:60] — **Resolu (D1 = b).** La vitesse est signee le long de l'avant de l'empreinte ; seules NaN et ±∞ sont refusees. `AgentObservation` conserve le signe et documente que ce n'est pas la vitesse de progression. Verifie par recherche : `TangentialSpeedMetersPerSecond` n'est lue que par `TrafficActor` et `AgentObservation` ; ni la route, ni `PathHorizon`, ni `MotionPlan`, ni la projection ne la lisent. Test : `SignedTangentialSpeedIsKeptAndNeverReadAsPlanProgress` (signe conserve a -3 et +3, decision et projection identiques, refus de NaN / ±∞).
- [x] [Review][Decision] `VerifySpeedProfile` ignore `Issue` / `GeometricallyFeasible` et n'exige pas que le candidat couvre l'horizon [MotionPlan.cs:102] — **Resolu (D2 = a).** Plan infaisable -> `SpeedProfileIssue.PlanInfeasible` (ajoute en fin d'enum) avec `SpeedProfileResult.PlanIssue` = cause exacte du plan et sa distance ; candidat incomplet -> `InvalidSpeedProfile` (premier s > 1 mm, ou dernier s < longueur - 1 mm). Contrat de codes ajuste dans le bloc fige et le Spec Change Log. Tests : `InfeasiblePlanNeverAdmitsACandidateAndKeepsItsCause` (preuve absente / perimee, non couverte, plafond invalide, horizon non conforme), `CandidateMustCoverTheWholeHorizon` (debut tardif, arret court, tolerance de 1 mm, horizon tronque `HorizonTruncated`, horizon complet jusqu'au portail), `SteeringCeilingIsCheckedBetweenCoarsePointsAndAtRingSeams` (raccord). Les profils de `CandidateProfileChecksBoundsAndDoesNotGenerateSpeeds` ont ete completes pour couvrir l'horizon.
- [x] [Review][Decision] Comparaison exacte des bornes d'acceleration, sans tolerance [MotionPlan.cs:124] — **Resolu (D3 = a).** `PlanningTolerances.AccelerationBoundToleranceMetersPerSecondSquared` = 1e-3 m/s², justifiee dans les Design Notes (« Tolerances du verificateur ») et appliquee aux deux sens. Test : `AccelerationToleranceAdmitsRoundingAndRefusesRealOvershoot` (borne exacte admise ; erreur d'arrondi simulee a +2 ulp, mesuree entre 0 et la tolerance, admise en acceleration et en freinage ; depassements de 2e-3, 1e-2 et 1 m/s² refuses au dernier point).
- [x] [Review][Patch] `GateAEvidenceStatus.Valid` vaut 0 [GateAEvidenceBinding.cs:9] — **Resolu.** `GateAEvidenceMissing` est la valeur 0 ; verifie avant : l'enum n'est ni serialise, ni hache, ni reference par un asset, un JSON ou un rapport signe (recherche hors `.cs` : aucune occurrence). Test : `DefaultEvidenceFailsClosed` (`default` -> `Missing`, `Valid` faux, `MotionPlan` infaisable avec `GateAEvidenceMissing`).
- [x] [Review][Patch] La projection affiche `Gate A GateAEvidenceMissing` sans plan de route [TrafficDecisionProjection.cs:48] — **Resolu.** `EvidenceStatus` et `ReferenceCoverage` sont nullables ; `ToText()` imprime « non evalue » pour la liaison, la couverture et le plafond quand il n'y a pas de plan. Test : `ProjectionSaysNotEvaluatedWhenNoRouteExistsAndReportsEvaluatedStages`.
- [x] [Review][Patch] Aucun test ne produit les codes de non-conformite d'horizon [Story530PlanningSpineTests.cs] — **Resolu, avec une limite mesuree.** `HorizonDefectsAreNamedAndReachTheDecision` produit `SeamCurvature` de bout en bout (carrefour non signe : chemin, mouvement `HorizonNonConforming`, distance, projection) et `InfeasiblePlanNeverAdmitsACandidateAndKeepsItsCause` verifie la propagation jusqu'au verificateur. `GeometryDefectsAreRefusedByTheValidatorBeforeAnyHorizonExists` etablit que ecart de position, saut de courbure et pente de courbure incoherents avec la geometrie sont refuses a la compilation (`RoadModelCompilationException`, regles C1 / C5). `MissingElement`, `SeamGap`, `SeamTangent` et `CurvatureSlope` restent des secondes lignes de defense inatteignables avec un modele compile valide (`RoutePlan` n'a pas de constructeur public, `RoutePlan` porte la version-empreinte du modele, le validateur applique les memes tolerances de raccord) ; ils ne sont donc pas exerces isolement, et cette limite est consignee plutot que contournee par un `InternalsVisibleTo` que la 5.26 interdit.
- [x] [Review][Patch] Lignes « Profil » de la matrice sans test [Story530PlanningSpineTests.cs] — **Resolu.** `SteeringCeilingIsCheckedBetweenCoarsePointsAndAtRingSeams` : depassement entre deux points grossiers du profil (extremites au-dessus du plafond, defaut strictement interieur), egal au plafond a l'echantillon du raccord d'anneau (admis), au-dessus du v* du cote fini (refuse a s = raccord), `MinimumCeiling` = min des deux cotes. `CandidateMustCoverTheWholeHorizon` : ligne droite a 50 m/s, plafond « aucun », admis. Deceleration = borne : `AccelerationToleranceAdmitsRoundingAndRefusesRealOvershoot` et `CandidateProfileChecksBoundsAndDoesNotGenerateSpeeds`.
- [x] [Review][Patch] Le rejeu n'etablit pas que l'horizon atteint la sortie [Story530PlanningReplayTests.cs:41] — **Resolu.** A chaque pas : reutilisation (`Planned` / `Requested`, meme sortie, memes occurrences, progression sur l'occurrence courante), `Path.End == ExitPortal`, intervalles = occurrences restantes, dernier intervalle = occurrence de sortie, longueur = somme des longueurs restantes (0,01 m) ; chaque raccord verifie `RoundaboutDiscontinuity` ⇔ |Δκ| > 1e-3, |Δκ| = 1/6 a 1e-3 pres, v* = min des deux cotes, et l'ensemble observe (mouvement, cote) est inclus dans les 24 epingles et non vide. Regard court : `TruncatedAndCompleteHorizonsAreQualified` (`LookAheadLimit`, longueur 10 m, `HorizonTruncated`, puis `ExitPortal` sans diagnostic).
- [x] [Review][Patch] Refus d'entree et recherche d'acteur non testes [Story530PlanningSpineTests.cs] — **Resolu.** `ActorLookupAndInputRefusalsAreStable` (trois acteurs, tous retrouves dont le dernier ; absents avant, apres et vide ; `UnknownTrafficId`, `EmptyTrafficId`, frame nulle, `lookAheadMeters` = 0, < 0, NaN, ±∞ pour `Evaluate` et `PathHorizon.Build`), `PlanFromAnotherModelVersionIsStaleAtRouteAndHorizon` (`StalePlan` par version de modele, replan et `Build`), `SignedTangentialSpeedIsKeptAndNeverReadAsPlanProgress` (`InvalidSpeed`), `ProjectionDoesNotDependOnActorInputOrder` (trois acteurs, six permutations, chacun des trois acteurs evalues).
- [x] [Review][Patch] Repli `DriverProfile`, tolerance de suivi invalide et contenu de projection peu assertes [Story530PlanningSpineTests.cs] — **Resolu.** `DriverProfileSuppliesTheDefaultLongitudinalBounds` (bornes = profil, acceleration refusee au-dessus de 1,7, freinage a 2,5 admis sous 2,9, profil par defaut -> bornes invalides -> `InvalidSpeedProfile`), `InvalidTrackingToleranceIsNotCoveredEvenWithValidEvidence` (NaN, -1, +∞ avec preuve valide), `NextMovementStartsFromTheReusedPlanProgress` (progression > 0, prochain mouvement != premier mouvement), et contenu de projection (ids de plan, texte du plafond, liste d'IDs, sortie) dans `ProjectionSaysNotEvaluatedWhenNoRouteExistsAndReportsEvaluatedStages`.
- [x] [Review][Patch] Le scan de symboles interdits et la reflexion ignorent `PlanningSpine.cs` [Story530PlanningSpineTests.cs] — **Resolu.** `NewPlanningSurfaceContainsNoControlOrTargetIndex` scanne aussi `Traffic/PlanningSpine.cs` (8 fichiers au total, garde de nombre) et etend la reflexion a `PlanningRequest`, `PlanningDecision`, `TrafficDecisionProjection`, `TrafficFrame`, `TrafficActor`, `AgentObservation`.
- [x] [Review][Patch] La « Confirmation 5.30 » affirme l'assembly « mesure » sans mesure [docs/setup/story-5-25-road-world-model-notes.md:27] — **Resolu.** Mesure du 2026-09-29 consignee (1 fichier reference `DriverProfile`, 0 `VehicleDriveIntent`, 0 `.asmdef` sous `Traffic/`, assembly `RoadRage.Features.Vehicles`) ; ligne vide parasite retiree.
- [x] [Review][Defer] Les 24 raccords d'anneau signes sont des identifiants `MVP_Run` codes en dur dans `PathHorizon` (et dupliques dans le rejeu) [PathHorizon.cs:77] — deferred, decision proprietaire d'epingler ; a deplacer vers la donnee signee a la re-signature Gate A (5.31)
- [x] [Review][Defer] La liaison Gate A hache le modele et parse le JSON a chaque `Evaluate`, et la projection est construite a chaque decision [PlanningSpine.cs:83] — deferred, la 5.30 n'a pas de chemin runtime ; cache par modele au transport runtime (5.31)
- [x] [Review][Defer] Analyse du rapport Gate A fragile mais fermee : ancres litterales, unite de a_e non validee (le hash du bloc garde), CRLF n'importe ou -> `Stale` [GateAEvidenceBinding.cs:47] — deferred, durcir avec le transport runtime (5.31)
- [x] [Review][Defer] `PathPoint` conserve +∞ dans un `float` (`Unbounded` n'est qu'un booleen derive) alors que la spec veut « jamais comme une vitesse » [PathHorizon.cs:16] — deferred, forme d'API a fixer par le premier consommateur (5.31)
