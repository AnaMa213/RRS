---
title: 'Story 5.26 -- Geometrie dirigee en abscisse curviligne et localisation de voie'
type: 'feature'
created: '2026-09-22'
status: 'done'
review_loop_iteration: 0
baseline_commit: '0909f8961ef924e1689de109fc9e9d7b541ea90b'
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le modele compile de la 5.25 porte des echantillons de courbe que personne ne sait interroger : aucun `s`, aucune projection, aucune borne, aucune localisation. La migration (5.27) ne peut rien mesurer, le routage (5.29) et la colonne (5.30) ne peuvent adresser aucune position, et les invariants geometriques reportes par la 5.25 (repere, longueur, coutures, AD-48, sens des adjacences) ne sont verifies nulle part.

**Approach:** Livrer la surface de courbe dirigee d'AD-45 (`Length`, `Sample`, `Project`, `Bounds`) commune aux corridors et mouvements, un constructeur de courbe depuis polyligne authoree, la validation geometrique a echec dur dans `Compile`, et un localisateur pur qui rend un `RoadLocation` a drapeaux composables, confiance et alternatives ordonnees. Tout est prouve en EditMode sur fixtures synthetiques.

## Boundaries & Constraints

**Always:** Repere `right = normalize(cross(up, forward))` ; lateral positif a droite, normal positif selon road-up ; cap signe dans `[-180, 180]`, positif vers la droite ; courbure signee positive quand `dT/ds` pointe a droite, nulle en ligne droite. `Project` borne `s` a `[0, Length]` sans jamais extrapoler, en 3D (jamais le seul world-up) ; egalite de distance departagee par le plus petit `s`. `Bounds` contient toute l'enveloppe de largeur. Un echec geometrique est un echec dur de `Compile` avec son propre code de motif (jamais un code groupe) ; rien n'est derive, reordonne ni repare. **`RoadModelVersion` n'est emise que par un `Compile` reussi, validation geometrique comprise : aucun autre chemin ne produit une version ou un equivalent pour un modele invalide.** Les coutures longitudinales -- `LaneConnection` comme extremites de `JunctionMovement` -- sont des invariants du modele (position et tangente ; largeurs en plus pour les mouvements). Deux profils distincts, tous deux dans la charge canonique : `RoadModelValidationProfile` (parametres statiques de validation geometrique) et `RoadLocalizationProfile` (parametres de requete : bande, hysteresis, acceptation, seuil de contresens) ; changer une valeur change la version mais jamais le schema, seul un changement de representation ou de sens incremente `CompilerSchemaVersion`. Localisation : fonction pure de (modele, pose d'empreinte, element precedent, elements de route) ; le score combine geometrie, cap, route, element precedent et connectivite explicite ; **le cap ne fait jamais preferer un element dont l'enveloppe ne contient pas la pose a un element qui la contient** ; l'ordre des `RoadId` ne departage que les egalites exactes ; `confidence` deterministe dans `[0,1]` ; `Ambiguous` decrit l'ensemble des candidats ; `localized=false` n'a aucune identite d'element. C# 9 sans `record` ni `init`.

**Ask First:** Nouvel assembly ou dependance de package ; toute modification d'un fichier V1 retenu ; relacher une tolerance provisoire (0,05 m, 5°) au-dela de la valeur du contrat.

**Never:** Aucune reference a `com.unity.splines`. Aucun planificateur de route, occupation, leader, `Rigidbody`, `Transform` ou `MonoBehaviour` dans le code 5.26. Aucun snap, aucune avancee de route. Ne jamais copier `ResolveLookAheadPoint`, la detection d'orbite, `arrivalRadius`, le plus-proche planaire. Aucun `InternalsVisibleTo` ni empreinte contournant la validation. Aucun index spatial optimise (AD-42 / 5.46). Ne pas scinder la story.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Nominal | pose au centre d'un corridor, cap aligne | localise, bon element, `s` exact, lateral ~0, aucun drapeau, confiance 1 | N/A |
| Frontiere | pose oscillant de +/-2 cm sur la couture corridor->mouvement | au plus une transition, jamais de retour (hysteresis) | N/A |
| Deplace | vers le bord sans voisin : lateral +1,5 m puis +3 m | lateral signe exact ; `OutsideEnvelope` seulement quand l'empreinte depasse l'enveloppe ; reste localise | N/A |
| Contresens | cap 180° sur sa voie | `WrongWay`, meme element | N/A |
| Contresens a cote d'une voie opposee | cap inverse au centre d'une voie dont la voie opposee adjacente a le cap de la pose | `WrongWay` sur la voie physique ; la voie opposee n'est qu'une alternative | N/A |
| Carrefour ambigu | debut de deux mouvements divergents d'une meme approche | `Ambiguous`, alternatives ordonnees, confiance < 1 ; un element de route fourni l'emporte | N/A |
| Hors corridor | pose au-dela du seuil d'acceptation | `localized=false`, aucun element, alternatives eventuelles, modele inchange | N/A |
| Hors domaine | point avant le depart / apres la fin | `s` borne a 0 / `Length` | N/A |
| Geometrie invalide | repere non orthonorme, longueur incoherente, demi-largeur <= 0, intervalle hors domaine, couture de connexion ou de mouvement rompue, adjacence de sens oppose, desaccord `LaneSide`, ordre lateral non monotone, enveloppes chevauchantes, corridor non ancre au datum | echec dur au code dedie nommant l'id fautif | aucune sortie compilee, aucune version |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs:293` -- `RoadCurveSample` : charge que la 5.26 interroge et produit. `:640` `RoadModelValidationProfile` (contient aujourd'hui `LocalizationScoreBandMeters` et `WrongWayHeadingDegrees`, a deplacer vers le profil de localisation). `:421` `LaneAdjacency.Side`. `:408` `LaneConnection`.
- `.../Traffic/CompiledRoadModel.cs:83` -- `EffectiveLaneCorridor` et `:107` `CompiledJunctionMovement` : y exposer la courbe. `:643` `ReadOnlyCopy`.
- `.../Traffic/RoadModelCompiler.cs:38` -- `Compile`, seul emetteur ; `CompilerSchemaVersion = 2` passe a 3 (representation du profil change).
- `.../Traffic/RoadModelValidator.cs:111` -- `Validate` structurel ; codes 1-18 ; `:660` `CheckSamples`. La geometrie s'execute seulement si le structurel est vide (references resolues).
- `.../Traffic/RoadModelCanonicalWriter.cs:14,108` -- `RoadModelCanonicalPayload` et `ComputeFingerprint` publics : cible directe des tests de couverture de champ que la geometrie interdit de muter isolement.
- `Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs:81` -- fixture synthetique geometriquement incoherente ; `:342` `VersionOf` ; `:389` `MoveE1Onto` (suppose E1 d'ordre maximal) ; `:629-641`, `:1565` mutations d'ordre/datum ; `:591,755` extremites de connexion ; `:734-738` tangente/up non unitaires ; `:763` `Side`.
- `_bmad-output/implementation-artifacts/deferred-work.md:358,374` -- invariants geometriques et adjacence de sens oppose reportes a la 5.26.

## Tasks & Acceptance

**Execution:**
- [x] `.../Traffic/RoadCurve.cs` -- courbe immuable sur echantillons : `Length`, `Sample(s)`, `Project(point[, sMin, sMax])`, `Bounds(s0, s1)`, cap signe.
- [x] `.../Traffic/RoadCurveBuilder.cs` -- polyligne authoree (position, up, demi-largeurs) -> echantillons : Catmull-Rom centripete, subdivision adaptative jusqu'a la tolerance de corde, `s` cumule, tangente et courbure signee.
- [x] `.../Traffic/RoadGeometryValidator.cs` -- invariants de forme, coutures de connexion (position, tangente) et de mouvement (position, tangente, largeurs), intervalles d'adjacence et de portail, extents de boite, sens des adjacences, accord `LaneSide`, AD-48 (ancrage, monotonie, chevauchement) ; codes 19+.
- [x] `.../Traffic/RoadLocalization.cs` -- `VehicleFootprint`, pose, `RoadLocation`, candidat, drapeaux, `RoadLocalizer` (balayage lineaire des bornes par element).
- [x] `RoadModelRecords.cs`, `RoadModelCanonicalWriter.cs`, `RoadModelValidator.cs`, `RoadModelCompiler.cs`, `CompiledRoadModel.cs` -- `RoadLocalizationProfile` sur `RoadModelSource` et le modele compile, tolerances geometriques dans `RoadModelValidationProfile`, les deux ecrits dans la charge ; codes ; appel geometrique ; schema 3 ; courbe exposee.
- [x] `Story525RoadWorldModelTests.cs` -- fixture a geometrie coherente (Design Notes) ; mutations valides toujours via `Compile` ; mutations que la geometrie interdit prouvees via `RoadModelCanonicalWriter.ComputeFingerprint` sur une charge construite par le test, avec en regard l'assertion que `Compile` rejette la meme mutation ; `MoveE1Onto` et `CorridorsInSection...` adaptes.
- [x] `Assets/RoadRage/Tests/EditMode/Story526GeometryAndLocalizationTests.cs` -- chaque ligne de la matrice, matrice de signes, tolerance de corde, un cas par code geometrique, determinisme, garde source (aucun `Rigidbody`/`Transform`/`MonoBehaviour`/`Splines`/`InternalsVisibleTo`).
- [x] `docs/setup/story-5-26-geometry-localization-notes.md` -- score, deux profils et regle de schema, codes, choix de fixture.
- [x] `sprint-status.yaml` -> `in-progress` ; `graphify update .`.

**Acceptance Criteria:**
- Given un corridor ou mouvement compile, when il est echantillonne ou projete, then il rend position, tangente, road-up, courbure signee et largeurs gauche/droite a `s`, et la projection ne sort jamais de `[0, Length]`.
- Given une polyligne sur un arc de rayon 12 m, when elle est compilee, then chaque corde s'ecarte de l'arc d'au plus 0,05 m et la courbure a le signe du virage.
- Given une source geometriquement invalide, when elle est compilee, then aucune `RoadModelVersion` n'est emise, et aucun autre membre public ne rend de version pour elle.
- Given la suite 5.25 apres adaptation, when elle tourne, then chaque champ canonique reste prouve par mutation, sur `Compile` ou sur le writer public.
- Given la suite EditMode complete, when elle tourne, then elle est verte, `TrafficOracleTests` et `Story59ParameterizedDriverModelTests` sans modification.

## Design Notes

**Score a deux rangs.** Rang 0 : l'enveloppe contient le point de reference (pour l'element precedent, depassement tolere jusqu'a l'hysteresis) ; rang 1 : sinon. On trie par (rang, score, `RoadId`). Score, en metres : `|lateral| + |normal| + depassement longitudinal + 2 m x |cap|/180 - hysteresis (precedent) - hysteresis/2 (successeur/predecesseur explicite du precedent, ou element de route)`. Le cap classe donc a l'interieur d'un rang, jamais entre rangs : deux voies opposees ont des enveloppes disjointes (AD-48 valide), si bien qu'une pose a contresens reste sur sa voie physique. Acceptation : distance a l'enveloppe <= seuil. `margin` = ecart de score avec le second candidat du meme rang ; `Ambiguous` si `margin < bande` ; `confidence = clamp01(margin / bande)`, 1 sans second du meme rang, 0 si non localise.

**Fixture 5.25 coherente.** Bandes paralleles, z in [0,20], ecart de 5 m, demi-largeurs 2 m : A1 (datum, x=0), A1b (5), A2 (10) vers +z ; D1 (15, z 20->10) puis E1 (15, z 10->0) en continuation ; D2 (20) sur toute la longueur vers -z. Ordres : A1 0, A1b 1, A2 2, D1 3, E1 4, D2 5. D1 et E1 ne se recouvrent sur le datum qu'en un point, donc ils ne sont pas compares entre eux. `MoveE1Onto` repasse D2 a 4. Mouvements : demi-tours analytiques a z=20 (A1->D1 r=7,5 ; A1->D2 r=10 ; A2->D1 r=2,5 ; A2->D2 r=5). Le portail de sortie est place en fin de E1 (s=10).

**Pourquoi la geometrie dans `Compile`.** Le contrat exige une continuite « validee par le compilateur » et AD-48 un echec de validation. Un second gate optionnel laisserait un modele invalide versionne et consommable.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story526GeometryAndLocalizationTests"` -- vert.
- `.\scripts\validate.ps1 -TestMode EditMode` -- suite complete verte, 0 erreur Console.

## Suggested Review Order

**Le gate unique : geometrie dans `Compile`**

- Seul emetteur de version ; la geometrie passe avant, donc un modele invalide n'a jamais de version.
  [`RoadModelCompiler.cs:42`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs#L42)

- La geometrie ne tourne que si le structurel est vide : references resolues garanties.
  [`RoadModelValidator.cs:511`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs#L511)

- Schema 3 : un seul increment, pour la representation des deux profils.
  [`RoadModelCompiler.cs:32`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs#L32)

**Invariants geometriques (codes 19+)**

- Forme : repere orthonorme, tangente dans le sens de la corde, up non retourne, longueur.
  [`RoadGeometryValidator.cs:164`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadGeometryValidator.cs#L164)

- Coutures longitudinales : connexions (position, tangente) et mouvements (plus largeurs).
  [`RoadGeometryValidator.cs:100`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadGeometryValidator.cs#L100)

- Mesure de couture partagee, seuils lus dans le profil de validation.
  [`RoadGeometryValidator.cs:282`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadGeometryValidator.cs#L282)

- AD-48 : ancrage, monotonie stricte et chevauchement projetes sur le datum.
  [`RoadGeometryValidator.cs:419`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadGeometryValidator.cs#L419)

- Adjacence : meme sens exige, `Side` confronte a la geometrie et a `LateralOrder`.
  [`RoadGeometryValidator.cs:311`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadGeometryValidator.cs#L311)

**Surface de courbe (AD-45)**

- Projection 3D bornee a `[0, Length]`, egalite departagee par le plus petit `s`.
  [`RoadCurve.cs:132`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadCurve.cs#L132)

- Echantillonnage : repere reorthonormalise, courbure et largeurs interpolees.
  [`RoadCurve.cs:114`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadCurve.cs#L114)

- Cap signe autour de road-up, positif vers la droite.
  [`RoadCurve.cs:31`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadCurve.cs#L31)

- Constructeur : Catmull-Rom centripete, subdivision bornee, echec plutot que corde hors tolerance.
  [`RoadCurveBuilder.cs:27`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadCurveBuilder.cs#L27)

**Localisation**

- Point d'entree pur : validation d'entree, rangs, tri, confiance.
  [`RoadLocalization.cs:130`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs#L130)

- Rang d'enveloppe : le cap ne classe qu'a l'interieur d'un rang (contresens reste sur sa voie).
  [`RoadLocalization.cs:357`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs#L357)

- Confiance deterministe depuis la marge de score, bornee a `[0,1]`.
  [`RoadLocalization.cs:232`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs#L232)

- `OutsideEnvelope` : empreinte laterale plus depassement longitudinal de la reference.
  [`RoadLocalization.cs:427`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadLocalization.cs#L427)

**Donnees et charge canonique**

- Deux profils distincts : validation statique et parametres de requete.
  [`RoadModelRecords.cs:679`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs#L679)

- Le profil de localisation entre dans la charge : la valeur change la version, pas le schema.
  [`RoadModelCanonicalWriter.cs:126`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCanonicalWriter.cs#L126)

- Courbe exposee sur les vues compilees de corridor et de mouvement.
  [`CompiledRoadModel.cs:105`](../../Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs#L105)

**Tests**

- Aucune version hors `Compile` : garde par reflexion sur tout membre public.
  [`Story526GeometryAndLocalizationTests.cs:846`](../../Assets/RoadRage/Tests/EditMode/Story526GeometryAndLocalizationTests.cs#L846)

- Regression contresens a cote de la voie opposee (correction 4 du proprietaire).
  [`Story526GeometryAndLocalizationTests.cs:970`](../../Assets/RoadRage/Tests/EditMode/Story526GeometryAndLocalizationTests.cs#L970)

- Tolerance de corde et signe de courbure sur l'arc de 12 m.
  [`Story526GeometryAndLocalizationTests.cs:539`](../../Assets/RoadRage/Tests/EditMode/Story526GeometryAndLocalizationTests.cs#L539)

- Fixture 5.25 reecrite en geometrie coherente.
  [`Story525RoadWorldModelTests.cs:88`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L88)

- Couverture des champs interdits par la geometrie : writer public, jamais une version.
  [`Story525RoadWorldModelTests.cs:463`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L463)

**Peripheriques**

- Score, profils, codes et choix de fixture.
  [`story-5-26-geometry-localization-notes.md:1`](../../docs/setup/story-5-26-geometry-localization-notes.md#L1)
