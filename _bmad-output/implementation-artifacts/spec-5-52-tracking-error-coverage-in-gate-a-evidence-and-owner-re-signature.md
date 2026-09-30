---
title: 'Story 5.52 -- Couverture de l''erreur de suivi et pose nominale cinematique dans la preuve Gate A, re-signature proprietaire'
type: 'feature'
created: '2026-09-30'
status: 'in-progress'
baseline_commit: '5845dc54f70818097b302935e1f4669df7b38414'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
  - '{project-root}/_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-29.md'
  - '{project-root}/_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-29-kinematic-pose.md'
  - '{project-root}/_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-27.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-31-first-driven-traffic-v2-vertical-slice-portal-to-portal.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La preuve Gate A signee (format 2, 2026-09-28) est calculee a pose tangente avec a_e = 0 : a_e n'est qu'un texte, et aucune trajectoire physique n'est couverte. Le 2026-09-30, le proprietaire a declare ε_t = 0,34 m (campagne `acceptance-20260930-153614`). Or le residu signe minimal aux angles des carrefours classiques est de 0,1127 m : l'echec geometrique est attendu.

**Approach:** Decisions proprietaire du 2026-09-30 : 1a (une seule spec avec point d'arret), 2a (repli V2 jusqu'a l'arret maintenu), 3 (sous-agents reserves aux couches de revue). Ordre de travail :
- **Phase A.** a_e = max|o| + ε_t entre dans le gonflement de chaque preuve et dans les empreintes versionnees. Un diagnostic intermediaire est publie sur `MVP_Run`, a pose tangente ; il ne vaut pas preuve.
- **Phase B.** Pose nominale cinematique complete (contrat §8). Les quatre familles de preuves sont regenerees : candidats `ConflictSweep`, degagement physique et `Sidewalk` `JunctionClearance`, balayage des giratoires, residus d'anneau `RoundaboutClearance`. Sont publies les residus reels par mouvement, coin et surface, et le diff des candidats.
- **Point d'arret.** Un residu final ≤ 0, un intervalle non ferme ou une pose infaisable arrete la story en `in-progress`. Le deficit chiffre et localise est presente au proprietaire, pour un correct-course geometrique separe.
- **Phase C** (residus tous positifs). Reevaluation deleguee des paires, nouvel enregistrement signe avec historique, re-signature par le proprietaire, rejeu du jalon PlayMode 1, puis Gate B.

## Boundaries & Constraints

**Always:**
- a_e = max|o(s)| + ε_t, avec max|o| = `PathHorizon.MaximumAbsoluteOffsetMeters` (0) et ε_t = `TrafficV2Settings.DeclaredTrackingTolerance` (0,34 m). a_e s'ajoute a la marge reservee et a δ_c ; il ne les remplace jamais.
- Construction conservee pour chaque preuve : rectangles orientes discrets du gabarit max ; gonflement en ecart de distance (`ConflictSweep`) ou integre au rectangle (`JunctionClearance`, avec son ρ gonfle) ; regle δ/2 entre deux poses ; echec ferme a 90°.
- Pose cinematique (contrat §8) :
  - convention de la mesure 5.31 : cap = tangente tournee de −e autour de road-up ;
  - intervalles d'entree par element sur le graphe de transitions (`SweepGraph.FromModel` = transitions du `RoutePlanner`), amorces aux portails d'entree (`Portal.SMeters`), sauts de tangente aux raccords, cycles compris ;
  - intervalles acceptes seulement par le controle d'inductivite ;
  - union sur chaque decalage, grille h_e sur l'enveloppe des deux demi-segments adjacents ;
  - restes δ/2 (Δθ_max = Δs·max|sin e|/a) et ρ·h_e/2 declares, publies par preuve et ajoutes au gonflement.
- Faisabilite braquage et taux verifiee sur tout l'ensemble de poses.
- Parametres declares, publies et haches dans la preuve : a_e, version du modele de pose, h_e, tolerance numerique η, budget d'iterations, entrees de faisabilite lues sur le prefab V2.
- Les parametres `Legacy` (pose tangente, a_e = 0) conservent le comportement historique de la preuve. La reproduction au bit pres exige les memes entrees geometriques historiques ; `Legacy` ne restaure ni la scene, ni les prefabs, ni l'authoring anciens. Si ces entrees ne sont plus disponibles, les artefacts signes historiques sont conserves octet pour octet et lies par leurs hashes ; leurs empreintes deviennent explicitement perimees face a la geometrie actuelle.
- Aucun artefact signe n'est ecrit avant la phase C ; la regeneration ecrit seulement sous `_bmad-output/implementation-artifacts/gate-a-5-52/`.
- Reevaluation des paires :
  - toute paire dont l'enveloppe change devient une proposition historique, reevaluee par `5.50-AUTO-DECISIONS-v1` sous toutes ses conditions (entrees epinglees, moteur committe, double plan identique, harnais des neuf carrefours, ecriture transactionnelle, second passage sans changement) ;
  - nouvelles paires et orphelines traitees explicitement.
- Reponse 2a, hors run de mesure : apres `TrackingToleranceExceeded`, le composeur recoit a chaque pas le refus `TrackingToleranceExceeded` et freine en repli jusqu'a l'arret maintenu, maintien compris.
  - Le vehicule reste present et physiquement libre : aucune contrainte `Rigidbody`, aucun changement d'`isKinematic`.
  - Le diagnostic est publie.
  - Les runs de mesure gardent leur comportement 5.31.
- Double garde AD-6 pour toute operation Unity MCP.

**Ask First:** (HALT proprietaire)
- tout residu final ≤ 0, `HeadingOffsetBoundNotClosed` ou `NominalPoseInfeasible` : arret de la story en `in-progress` ;
- une decision de conflit hors des conditions de `5.50-AUTO-DECISIONS-v1` ;
- tout changement de geometrie ;
- la re-signature Gate A ;
- un commit Git.

**Never:**
- Modifier la marge, δ_c, un seuil, h_e, η ou un reste pour obtenir un verdict ; convertir la marge ou un residu en allocation ; changer ε_t (une nouvelle valeur relance la story).
- Signer, ou ecrire un sign-off ou un artefact signe a la main.
- Classer une paire par jugement ; reutiliser une decision sur une enveloppe changee.
- Clore la Gate B sur une preuve a pose tangente ; presenter la borne comme continue au-dela du modele M.
- Reponse 2a : snap sur la route, teleportation, despawn, contrainte `Rigidbody`, confinement du modele routier ; recuperation ou replanification (5.39).
- Sous-agent generaliste ; script de verification ecrit par l'agent (tout passe par `validate.ps1`).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Allocation synthetique | a_e = x, trajectoire droite, obstacle lateral | Gonflement = marge + δ_c + x ; residu decale de x, ecart de conflit de 2x | N/A |
| Empreinte v2 | Un parametre change (marge, δ_c, a_e, h_e, η, version de pose, intervalle d'entree) | Empreinte differente | N/A |
| Parametres `Legacy` | Pose tangente, a_e = 0 ; entrees historiques ou geometrie actuelle distincte | Reproduction bit a bit avec les memes entrees historiques ; sinon artefacts signes intacts, candidats reconstruits et empreintes historiques non reconfirmees | Aucun ancien choix reutilise ; aucun nouveau binding signe |
| Diagnostic intermediaire | a_e = 0,34 m, pose tangente, `MVP_Run` | Rapport publie, etiquete « diagnostic » | Jamais lie ni signe |
| Fermeture impossible | Budget epuise ou \|e\| ≥ 90° | Aucune preuve | `HeadingOffsetBoundNotClosed` |
| Element inatteignable | Ni portail ni predecesseur | Signale, sans intervalle | Poses balayees a e = 0 (sur-ensemble) |
| Pose infaisable | v*_N(e) sous la vitesse de direction active, ou taux au-dela | Jamais couvert | `NominalPoseInfeasible`, HALT |
| Residu final ≤ 0 | Preuve cinematique avec a_e | Deficit localise publie (carrefour, mouvement, surface ou obstacle, position, s, couture, intervalle e, restes) | HALT, story `in-progress` |
| Double execution | Memes entrees | Rapports identiques octet pour octet | Echec sinon |
| Preuve a pose tangente | Sign-off signe actuel | `PoseModelMismatch`, jamais couvert | N/A |
| Tolerance depassee, hors mesure | d > ε_t a un pas | Repli a chaque pas, puis `FallbackHeld` ; une poussee externe deplace le vehicule | Diagnostic publie |
| Tolerance depassee, en mesure | Campagne 5.31 | Comportement 5.31 inchange | Campagne jugee |
| Preuve perimee | Empreinte ou hash different | Gate A fermee | Raison nommee |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs`
  - `:213-243` : `Inflation` = marge + δ_c ; `HalfLength`, `Rho` (non gonfle).
  - `:440` : `Paths`, prolongation de L/2.
  - `:587-630` : poses tangentes.
  - `:635-791` : `Evaluate` / `EvaluatePaths` (borne δ_I/2 + δ_J/2 + 2·gonflement, prefiltre AABB, temoin, volume).
  - `:823-853` : echec ferme a 90°, `Delta`.
  - `:167-192` : `SweepGraph.FromModel`.
- `Migration/JunctionClearance.cs`
  - `:108-116` : gonflement integre aux dimensions.
  - `:119` : `Subdivide` (h = 0,05 m, coutures explicites).
  - `:210-253` : `MeasurePath`, residu min(d0, d1) − δ/2.
  - `:268-484` : `Measure` (carrefours classiques ou giratoires).
  - `:462,471,482` : empreintes ; `:85,88` : versions.
- `Migration/RoundaboutClearance.cs:11-16,71-81` : formule fermee a deux gabarits, cap tangent, marge m par cote ; `:64` : `Sweep` ; `:88` : `Measure`.
- `Migration/PairGeometryFingerprint.cs`
  - `:16,90-125` : schema v1, sans parametre de gonflement.
  - `:360-428` : la table historique 5.50 exige le schema v1 ; le conserver.
- `Migration/AuthoredRoadModel.cs`
  - `:202-321` : `Run` ; `PairSweeps` et `CandidateModel` sont poses avant `MatchConflicts` (`:258-265`).
  - `:791-842` : une empreinte perimee echoue, « Proposition historique non reconfirmee ».
  - `:1312` : `TrackingAllowanceMeters` = 0, texte seul.
  - `:1321-1406` : `MeasureEvidence`, `RenderClearance` (premiere ligne « a_e = … ; h = … », hachee).
  - `:1945-2126` : sign-off format 2, `VerifySignoff`, `EvaluateGateA`.
- `Migration/AutomatedPairDecisionPolicy.cs`
  - `:29-35` : versions et approbation.
  - `:38-140` : `CreatePlan`.
  - `:393-523` : `Classify` ; `ContactWitness` lit `ConflictSweep.Inflation`.
- `Migration/PairReview.cs:18-34` : `PairDecisionState` (Missing, Stale, Confirmed, Orphan), vocabulaire du diff ; `:519` : `ProofOf`.
- `Migration/GateAReviewWindow.cs:166-188` : `Sign` ecrase le sign-off, sans historique.
- `Planning/GateAEvidenceBinding.cs:17,43-82` : lit « a_e = » dans le bloc `### Residus` ; toujours `TangentAligned`.
- `Planning/MotionPlan.cs:116-129` : `EvaluateVehicleCoverage` (`PoseModelMismatch` sauf si `Kinematic`).
- `Planning/PathHorizon.cs`
  - `:83-97,236-247` : 24 raccords d'anneau codes en dur.
  - `:120` : `NominalSteeringCeilingMetersPerSecond`.
- `Traffic/RoadCurve.cs:255-307` : `AdvanceKinematicOffset` (RK4, pas ≤ 0,1 m, κ lineaire), `SignedTangentJumpRadians`.
- `Lifecycle/TrackingMeasurement.cs:150-266` : solution de route (RK4 propre, e interpole lineairement), `Nominal`, `BodyRateMax`.
- `Lifecycle/TrafficV2Composition.cs:52,63-65,225-239` : ε_t, chemins des trois textes, `EvaluateInsertion`.
- `Lifecycle/TrafficV2VehicleDriver.cs`
  - `:274` : `isKinematic = !IsServer`.
  - `:385-434` : refus vers le composeur.
  - `:436-491` : `Monitor` (compte et journal de `TrackingToleranceExceeded`, formules de faisabilite).
- `Intent/VehicleDriveIntentComposer.cs:8-24,116` : `V2FallbackReason`, `V2FallbackTerminal`, `Compose`.
- `Routing/RoutePlanner.cs:187-198` : transitions = connexions + mouvements.
- `RoadModelRecords.cs:714-722` : `DrivabilityProfile` (L, a, braquages, vitesse inactive ; ni taux ni vitesse).
- `Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab:174,188` : `vehicleProfile` (taux de braquage, μ) et `driverProfile` (`DesiredSpeed`).
- Artefacts signes : `App/Scenes/MVP_Run/MVP_Run.road-signoff.json` (format 2), `MVP_Run.road-authoring.json` (decisions, empreintes v1), `_bmad-output/implementation-artifacts/migration-report-5-28-mvp-run.md` (bloc haché), `v1-regression-5-50/automated-pair-decisions.json`.
- Tests existants :
  - `Tests/EditMode/Story550*`, `Story551JunctionClearanceTests.cs`, `Story528AuthoringAndGateATests.cs` (tripwire Gate A) ;
  - `Story530PlanningReplayTests.cs:139` (reflexion sur `SignedRingSeams`) ;
  - `Story531LifecycleAndCompositionTests.cs` (refus) ;
  - `Tests/PlayMode/Story531FallbackLowSpeedPlayModeTests.cs` et `Story531V2VerticalSlicePlayModeTests.cs` (patrons de banc et de jalon).

## Tasks & Acceptance

**Execution:**

*Phase A -- allocation, empreintes, reponse 2a*
- [ ] `Traffic/Migration/GateAEvidenceParameters.cs` (nouveau) -- valeur immuable portant les parametres du « Always », avec `Legacy`, `Declared` et un texte canonique hache -- une seule source des parametres.
- [ ] `ConflictSweep.cs`, `JunctionClearance.cs`, `RoundaboutClearance.cs`, `AutomatedPairDecisionPolicy.cs` (`ContactWitness`) -- recoivent les parametres ; gonflement = marge + δ_c + a_e ; `Legacy` inchange au bit pres, et versions d'algorithme propres a chaque modele de pose -- a_e entre dans chaque preuve.
- [ ] `PairGeometryFingerprint.cs` -- schema v2 : marge, δ_c, a_e, version du modele de pose, h_e, η et intervalles d'entree des deux mouvements ; v1 conserve pour `Legacy` et pour la table 5.50 -- toute enveloppe changee change d'empreinte.
- [ ] `Traffic/Migration/GateAEvidenceRegeneration.cs` (nouveau) -- `Run` jusqu'a `CandidateModel` et `PairSweeps` sous des parametres donnes ; preuves de degagement sur `CandidateModel` (meme geometrie, zones sans effet) ; diff des candidats contre les decisions signees (`PairDecisionState`, changements de relation) ; rapport canonique trie ; aucune ecriture signee.
- [ ] `VehicleDriveIntentComposer.cs` (raison `TrackingToleranceExceeded = 10`), `TrafficV2VehicleDriver.cs` -- verrou 2a hors mesure ; projection et journal ; runs de mesure inchanges.
- [ ] `Tests/EditMode/Story552EvidenceTests.cs` `[Core][Story552]` -- gonflement exact par famille, sensibilite de l'empreinte v2, identite `Legacy` (empreintes v1, residus), analyse du bloc par la liaison, regle du verrou, scans (aucune contrainte ni ecriture de pose dans le chemin V2).
- [ ] `Tests/PlayMode/Story552ToleranceResponsePlayModeTests.cs` `[Story552]` -- banc sur le patron 5.31 avec le profil V2 : verrou depuis 8 m/s, puis `FallbackHeld` ; ensuite une impulsion horizontale externe deplace le vehicule. Constraints `None`, `isKinematic` faux, vehicule present, pose finie.
- [ ] `Tests/EditMode/Story552RegenerationTests.cs` `[Explicit][Geometry][Category("Story552Regeneration")]`, cas A -- diagnostic a_e + pose tangente vers `gate-a-5-52/diagnostic-ae-tangente.md`.

*Phase B -- pose cinematique et regeneration*
- [ ] `Traffic/Migration/KinematicOffsetBounds.cs` (nouveau) -- intervalles I_X, amorces de portail, images par `AdvanceKinematicOffset`, sauts, elargissement de η, controle d'inductivite, budget, `HeadingOffsetBoundNotClosed`, elements inatteignables, enveloppes par demi-segment, max|sin e|.
- [ ] `ConflictSweep.cs`, `JunctionClearance.cs` -- chemin cinematique :
  - grille de caps par echantillon ;
  - δ calcule sur la rotation de caisse ;
  - reste ρ·h_e/2 ;
  - prefiltre sur l'AABB de l'union ;
  - temoins et volume issus des poses de grille ;
  - residu publie avec restes et intervalle e.
- [ ] `RoundaboutClearance.cs` -- formule fermee a gabarit tourne du pire |e| des corridors d'anneau et des continuations du module ; marge m + a_e par cote ; reste 0 publie.
- [ ] `Traffic/Planning/NominalPoseFeasibility.cs` (nouveau, pur) -- braquage et taux (Design Notes), partage avec `TrafficV2VehicleDriver.Monitor` (resultats inchanges) et la preuve.
- [ ] `Tests/EditMode/Story552KinematicPoseSetTests.cs` `[Geometry][Story552]` -- sur droites, arcs, S, sauts de raccord et cycles :
  - enumeration exhaustive bornee contenue dans les intervalles ;
  - cas non fermable explicite ;
  - union ≥ maximum d'echantillonnage dense ;
  - anneau tourne egal a la force brute ;
  - cas de faisabilite.
  
  Sur `MVP_Run` : fermeture ; solutions des routes de la campagne 5.31 contenues a 0,1 m pres.
- [ ] `Story552RegenerationTests.cs`, cas B -- regeneration cinematique vers `gate-a-5-52/regeneration-cinematique.md` et `gate-a-5-52/diff-candidats.md` ; deux executions identiques ; test de verdict listant chaque deficit. **HALT** si un residu ≤ 0, une borne non fermee ou une pose infaisable.

*Phase C -- seulement si tous les residus sont > 0*
- [ ] `AuthoredRoadModel.cs` -- pipeline par defaut sur `Declared` ; bloc `### Residus` avec a_e, `pose-model = kinematic-v1`, restes, parametres et liste signee des 24 raccords.
- [ ] `App/Scenes/MVP_Run/MVP_Run.road-authoring.json`, `v1-regression-5-50/automated-pair-decisions.json` -- decisions produites par `AutomatedPairDecisionPolicy` sous la procedure §1.5 de l'amendement 2026-09-27. Le manifeste enregistre le balayage v2, l'empreinte v2 et les parametres. Le commit du moteur exige ton accord (Ask First).
- [ ] `MVP_Run.road-model.json`, overlay et `migration-report-5-28-mvp-run.md` -- regeneres par les menus existants, jamais edites a la main ; diff et empreintes publies.
- [ ] Sign-off -- `SignoffFormat` 3, portant a_e, ε_t, max|o|, versions et hash des parametres.
  - `GateAReviewWindow.Sign` ecrit l'enregistrement courant et ajoute l'ancien, verbatim, dans `MVP_Run.road-signoff-history.json`, dans une seule ecriture transactionnelle.
  - `GateAEvidenceBinding` lit le format 3 : `Kinematic`, a_e, raccords signes.
  - `PathHorizon` lit les raccords signes ; la liste codee en dur est supprimee.
  - Adapter `Story530PlanningReplayTests`.
- [ ] Re-signature par le proprietaire (HALT).
- [ ] Fixtures liees a la preuve legacy : 5.28, 5.51 et refus 5.31, mis a jour. Soldes dans `deferred-work.md`.
- [ ] `Tests/PlayMode/Story552MilestonePlayModeTests.cs` `[Story552]` -- jalon 1 hors mesure : portail a portail, d ≤ ε_t au pas et sous le modele M, v ≤ v*, aucun contact hors chaussee ; Gate B consignee.

**Acceptance Criteria:**
- Given une allocation synthetique, when chaque famille est calculee, then son gonflement vaut exactement marge + δ_c + a_e plus les restes publies. Given les memes entrees geometriques historiques, when `Legacy` est execute, then la preuve signee est reproduite au bit pres. Given ces entrees absentes et la geometrie actuelle differente, when `Legacy` est execute, then les hashes des artefacts historiques et la coherence interne de leur binding restent verifies, les anciennes empreintes sont explicitement perimees/non reconfirmees et aucune decision historique n'est reutilisee.
- Given ε_t declare, when le diagnostic intermediaire s'execute, then son rapport est publie, etiquete diagnostic, et n'entre dans aucune liaison ni couverture.
- Given `MVP_Run`, when l'ensemble de poses est construit, then les intervalles passent le controle d'inductivite, contiennent les solutions des routes 5.31, et la faisabilite est verifiee partout ; sinon echec explicite nomme.
- Given la regeneration cinematique, when elle est publiee, then chaque residu (mouvement, coin, surface), ses restes, la version de pose et le diff des candidats sont publies, et deux executions sont identiques.
- Given un residu final ≤ 0, when le verdict est evalue, then la story s'arrete en `in-progress` avec le deficit localise, et rien n'est modifie pour le faire passer.
- Given un depassement hors mesure, when les pas suivants s'executent, then le repli V2 s'applique jusqu'a l'arret maintenu, le vehicule reste present et poussable, et le diagnostic est publie.
- Given des residus tous positifs, when les paires sont reevaluees, then aucune decision n'est reutilisee sur une enveloppe changee, et un second passage ne propose aucun changement.
- Given la preuve regeneree, when la Gate A est re-signee, then seul le proprietaire signe un enregistrement format 3 ; l'ancien reste dans l'historique ; la liaison lit a_e = 0,34 m et `Kinematic`.
- Given la nouvelle signature, when le jalon 1 est rejoue hors mesure, then les vehicules V2 roulent de portail a portail dans la portee de preuve, et la Gate B se ferme.

## Spec Change Log

- 2026-09-30 : accord proprietaire pour conditionner la reproduction bit a bit de `Legacy` aux memes entrees geometriques historiques. Sur `MVP_Run` corrige, verifier les SHA-256 des artefacts signes conserves et le refus explicite des empreintes historiques ; aucun changement de preuve, de signature ou de geometrie.

## Design Notes

**Pourquoi des parametres versionnes et une regeneration separee.**
- Basculer le pipeline des maintenant rendrait `MatchConflicts` et les fixtures 5.27, 5.28, 5.50 et 5.51 rouges pendant tout l'arret geometrique.
- A la place, `Legacy` garde les parametres historiques et `GateAEvidenceRegeneration` publie sans rien signer. Apres le correct-course geometrique, l'etat signe reste verifiable comme historique, mais ses fingerprints ne sont pas reconfirmes sur la scene actuelle. La bascule a lieu en phase C.

**Intervalles.**
- Iteration de Kleene sur `SweepGraph`, avec les images de bornes par la meme integration que le runtime.
- Elargissement de η = 2·10⁻³ rad. η couvre l'ecart entre les integrations RK4 et l'interpolation lineaire de `ReferenceTrack`, soit h²/8·max|e''| ≈ 10⁻³ rad.
- Budget de 256 passes, puis controle d'inductivite strict.
- Enveloppe sur un demi-segment : bornes aux noeuds RK4, plus (|κ|max + 1/a)·h/2.

**Grille.**
- h_e = 0,008 rad, soit ρ·h_e/2 ≈ 9,9 mm pour la preuve de conflit et ≈ 13,4 mm pour le degagement (ρ gonfle). Parametre declare avant toute mesure.

**Faisabilite (§8, « a la vitesse planifiee » pour un ensemble independant de la route).**
- v_max(s, e) = min(`DesiredSpeed` V2, v*_N(e), √(μ·g/|κ(s)|)) : les contraintes appliquees par `SpeedPlan`, avec μ·g ≥ a_lat.
- Braquage faisable ⇔ v*_N(e) ≥ `SteeringInactiveBelowMetersPerSecond`. |δ_N| est monotone en |e|, donc le controle aux bornes est exact.
- Taux : v·|dδ/ds|, avec dδ/ds = f(e)(κ − sin e/a) et f = (L/a)/(cos²e + (L/a)² sin²e).
  - Evalue sur la grille, plus un reste v·(h_e/2)·G, avec G = f_max|(L/a)² − 1|/min(1, (L/a)²)²·(|κ| + 1/a) + f_max/a.
  - Les sauts aux raccords, bornes par la tolerance d'admission, ne sont pas des taux : c'est la regle du moniteur 5.31.

**Anneau tourne.**
- Point interieur = distance du centre au rectangle tourne (bord ou coin). Coin exterieur = √(R² + W² + (L/2)² + 2R(W·cos e + (L/2)|sin e|)).
- Les deux sont croissants en |e| pour e < 65° : le pire cas est en max|e|, exact et sans grille.

**Point d'arret attendu.**
- 0,1127 − 0,34 ≈ −0,227 m sur le diagnostic, avant l'effet du cap. Le rapport final nomme chaque ligne ≤ 0 pour le correct-course geometrique.

**Ecart de mesure (2026-09-30, implementation).** La distance de `JunctionClearance.Distance` vaut 0 des qu'il y a recouvrement : un residu negatif plafonnait a -(delta/2 + reste) (-0,047 m) quelle que soit la penetration. Hors parametres `Legacy`, les preuves de degagement utilisent `SignedDistance` (moins la profondeur de penetration, SAT) : un deficit est chiffre. Les cas separes sont identiques ; la preuve signee n'est pas touchee.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.52 -TestMode EditMode` -- attendu : compte execute = compte attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter Story552Regeneration -TestFilterType category -IncludeExplicit` -- attendu : rapports ecrits sous `gate-a-5-52/`, determinisme vert ; le verdict rouge est la preuve du HALT. Si ce mode n'est pas supporte : STOP et signaler.
- `.\scripts\validate.ps1 -Profile Story -Story 5.52 -TestMode PlayMode` -- attendu : tests `Story552` verts. Redemarrer l'Editeur si le runner PlayMode a deja servi dans la session.

## Point d'arret du 2026-09-30 (decision 1a)

- EditMode `Story552` : 20/20, 0 erreur Console. Regeneration `[Explicit]` `Story552Regeneration` : A (diagnostic) et B (regeneration cinematique complete, deterministe) verts ; C (verdict) rouge : **74 deficits**. Rapports : `gate-a-5-52/regeneration-cinematique.md`, `gate-a-5-52/diff-candidats.md`, `gate-a-5-52/diagnostic-ae-tangente.md`.
- Diagnostic a_e seul (pose tangente, ne vaut pas preuve) : 20 deficits, minimum -0,229 m (= 0,1127 - 0,34).
- Regeneration cinematique : bornes d'ecart fermees en 23 iterations, 0 element inatteignable, aucune pose infaisable (marges min 4,43 m/s et 37 deg/s). Minima : physique -0,513 m, Sidewalk -0,513 m, anneau V2 -0,521 m (|e| max 15,24 deg ; anneau physique +0,342 m).
- Deficits : virages a droite des 5 carrefours classiques (angles de trottoir et bordures, jusqu'a -0,513 m), virages a gauche vers les trottoirs de sortie (-0,261 a -0,428 m), enveloppe V2 des 4 anneaux (-0,521 m). Candidats : 16 nouvelles paires, 120 changees, 0 confirmee, 0 orpheline.
- Story arretee `in-progress` : correct-course geometrique separe du proprietaire requis. Aucune marge, seuil, reste ni geometrie modifie. Restent : test PlayMode 2a (redemarrage Editeur), phase C, revues.
