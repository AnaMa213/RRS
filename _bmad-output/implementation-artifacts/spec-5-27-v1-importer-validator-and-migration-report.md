---
title: 'Story 5.27 -- Importeur V1, validateur semantique et rapport de migration mesure'
type: 'feature'
created: '2026-09-23'
status: 'done'
baseline_commit: '867fb9d48c5cc11ef4e9d8b623466e573bcd8879'
review_loop_iteration: 0
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le Road World Model (5.25/5.26) n'a jamais vu la vraie carte : aucun `MVP_Run` migre, aucune disposition des 204 noeuds V1, et les tolerances geometriques restent des hypotheses non mesurees.

**Approach:** Un adaptateur editeur a sens unique lit le district V1, prouve son ensemble source, produit un `RoadModelSource` candidat a identites stables (lignee persistee), mesure la geometrie et genere un rapport machine lie a sa source. Le modele ne passe pas encore `Compile` (controles absents) et le rapport le dit.

## Boundaries & Constraints

**Always:** Code sous `Features/Vehicles/Traffic/Migration/`, entierement `#if UNITY_EDITOR`, dans l'assembly existante. Lecture seule de V1 (aucun fichier V1 modifie, aucune ecriture de scene). Chaque noeud, arete, poids, jointure de connecteur et role de portail recoit une disposition typee (merge ou rejet avec motif). **Cle de lignee** : deterministe, construite uniquement depuis des `GlobalObjectId` de source et une semantique source (role, sens d'arete), selon la table des Design Notes ; jamais nom, hierarchie, ordre de liste, index, transform, `LateralOrder` ni `RoadId` genere ; deux entites de meme cle = echec dur. Re-import sur source inchangee : zero identite frappee ; entite disparue tombstonee et disposee, jamais recyclee. Rapport lie au hash de la source V1 extraite, a `ImporterVersion`, `CompilerSchemaVersion`, `ModelId`, au hash de lignee et au hash de son corps ; champ `RoadModelVersion` = « absente -- Compile refuse : N erreurs par code ». **Serialisation deterministe** (tri ordinal par cle, culture invariante, decimales fixes, LF, aucun horodatage) : deux imports de la meme source produisent des octets identiques. **Ecriture fail-closed** : tout est calcule en memoire, rien n'est ecrit en cas d'echec, et un couple lignee/rapport desaccorde est toujours rejete par `Verify`. L'importeur ne code aucun effectif : 25 modules / 204 noeuds / 28 sections / 44 corridors / 9 carrefours / 72 mouvements / 8 portails sont des assertions de l'instantane `MVP_Run` lie (tests et rapport). Seuils du contrat publies tels quels (seul 0,75 m herite de V1) ; mesures en count/min/p50/p95/max ; classement mesure / seuil / deviation a corriger / exception justifiee. Seule exception approuvee : lissage des noeuds de decision V1 par les mouvements tournants, comptee avec sa deviation max. Semantique absente (largeurs revues, adjacence, controles, lignes, conflits, signaux) = taches d'authoring explicites.

**Ask First:** Nouvelle assembly ou dependance ; toute modification de V1 ou de `MVP_Run` ; relacher un seuil ; toute autre categorie d'exception justifiee ; toute modification de 5.26 au-dela de la surcharge de mesure de `RoadCurveBuilder`.

**Never:** Aucune version emise hors `Compile` ; aucun controle, conflit, priorite ou signal invente (`Uncontrolled` n'est pas un repli) ; aucune adjacence de sens oppose ; aucune topologie tiree d'un nom ; aucune jointure par proximite hors decouverte candidate V1 (0,75 m, `Dot > 0`) ; aucune inference de cycle ; `NearestNodeIndex`, `Rebuild()`, `GetSiblingIndex`, `NetworkObjectId` interdits ; pas de runtime, pas de sync bidirectionnelle ; aucun asset V2 persiste autre que la lignee ; aucune estimation presentee comme preuve d'un seuil.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Instantane nominal | `MVP_Run` lie | effectifs de l'instantane, 0 non dispose, 12 mvts carrefour, 6 par T, 3 entrees/3 sorties/3 continuations par giratoire, chaque entree atteint au moins une sortie | N/A |
| Re-import | lignee issue d'un premier import | memes IDs, zero frappe, octets de lignee et de rapport identiques | N/A |
| Entite disparue | lignee avec une cle absente de la source | ID tombstone, dispose « retire » | N/A |
| Entite nouvelle | entree de lignee retiree | nouvel ID, dispose « nouveau » | N/A |
| Collision de cle | deux entites produisant la meme cle | echec dur nommant les deux sources | rien n'est ecrit |
| Source non reconnue | prefab inconnu ou `LaneNode` hors module | echec dur nommant l'objet | rien n'est ecrit |
| Rapport perime | binding (dont hash de lignee) different ou corps edite | rejete avec motif | jamais repare |
| Mesure hors seuil | ex. derive > 0,10 m | deviation a corriger listee, seuil inchange | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/LaneNode.cs:41` / `LaneGraph.cs:22` -- source V1 (role, successors, turnWeights, exitReusesEntry) ; regle de jointure `LaneGraph.cs:288` a reproduire comme decouverte candidate.
- `Assets/RoadRage/Prefabs/Greybox_{RoadSegment_TwoWay,Intersection,TJunction,Roundabout,TunnelPortal}.prefab` -- 5 modules reconnus (6/12/9/17/4 noeuds) ; connecteurs a z=+/-8 local, voies a x=+/-2.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- 25 instances sous `RunRoot` (grille 16 m) ; aucune surcharge de `LaneNode`.
- `.../Traffic/RoadModelRecords.cs:362,461,563,593,709` -- `LaneCorridor`, `JunctionMovement`, `Portal`, `ImportManifestEntry`/`SourceTrace`, `RoadModelSource` ; `RoadId` non serialisable Unity (hex via `ToString`/`TryParse`).
- `.../Traffic/RoadCurveBuilder.cs:27,184` -- `Build` ; critere 5.26 = distance spline-corde sondee a u=1/4,1/2,3/4, bornee a tolerance/2 (`Subdivide`). `RoadCurve.cs:75,123` -- courbe et `Project`.
- `.../Traffic/RoadModelValidator.cs:11` -- codes ; code 9 bloquera `Compile` ; `RoadGeometryValidator.Validate` (interne) appelable pour lister la geometrie.
- `Assets/RoadRage/ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset:24` -- `connectorJoinDistance: 0.75`.
- `Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs:281,1697` -- garde V1 « chaque entree atteint une sortie » ; motif `WithMvpRun`.
- `NetworkedAIVehicleDriverController.cs:628` -- V1 vise une seule sortie par vehicule (repli sur la plus proche) : aucune exigence toutes-entrees-vers-toutes-sorties.
- `Tests/EditMode/Story525RoadWorldModelTests.cs:1652` -- garde des motifs V1 sur tout `Traffic/` ; `Story526...Tests.cs:119` -- valeurs de profil a reprendre.

## Tasks & Acceptance

**Execution:**
- [x] `.../Traffic/RoadCurveBuilder.cs` -- surcharge `Build(..., out float maxChordDeviationMeters)` rendant le max du critere de subdivision existant sur les cordes emises ; `Build` inchange et delegue au meme chemin ; test : echantillons identiques champ a champ entre les deux surcharges.
- [x] `.../Traffic/Migration/V1SourceSet.cs` -- extraire modules, noeuds (cle, pose, role, arcs, poids), jointures candidates avec ecart et angle ; echec dur sur prefab inconnu, noeud hors module, connecteur orphelin ; hash canonique de la source extraite.
- [x] `.../Traffic/Migration/RoadLineage.cs` -- lignee JSON triee (ModelId, cle -> id/genre, tombstones) ; frappe, preservation, retrait ; collision = echec dur.
- [x] `.../Traffic/Migration/V1RoadModelImporter.cs` -- source -> `RoadModelSource` + dispositions typees + taches d'authoring (Design Notes).
- [x] `.../Traffic/Migration/MigrationReport.cs` -- mesures, portee, validation (structurelle + geometrique, sans version), rapport Markdown deterministe, `Verify`, menu `RoadRage/Traffic V2/Migrer MVP_Run` (Design Notes, ecriture).
- [x] `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-lineage.json` et `_bmad-output/implementation-artifacts/migration-report-5-27-mvp-run.md` -- generes par le menu, committes.
- [x] `Assets/RoadRage/Tests/EditMode/Story527MigrationTests.cs` -- chaque ligne de la matrice (echecs durs sur scene temporaire) ; determinisme octet a octet ; rapport committe verifie contre un import frais ; surcharge de mesure egale au critere sur l'arc de 12 m de la 5.26.
- [x] `deferred-work.md` -- fixtures de localisation sur carte reelle (modele compile, 5.28) ; jointure segment-segment non exercee. `sprint-status.yaml` -> `in-progress` ; `graphify update .`.

**Acceptance Criteria:**
- Given le rapport genere, when on le lit, then il publie les distributions mesurees d'ecart et d'angle de connecteur, couture, corde (critere 5.26), derive de noeud et de portail, a cote du seuil et du classement de chaque valeur.
- Given le modele candidat, when la portee est calculee sur la topologie V2 (corridors, connexions, mouvements, parcours trie par `RoadId`), then chaque portail d'entree atteint au moins un portail de sortie (contrat AD-47 et garde V1) ; la matrice complete entrees x sorties est publiee sans etre un gate.
- Given l'import, when il se termine, then chaque semantique absente est une tache d'authoring et le rapport dit que le modele ne passe pas la validation, avec les erreurs par code.
- Given le rapport committe, when la suite tourne, then il est accepte contre un import frais et rejete des qu'un element du binding ou son corps change.
- Given la suite EditMode complete, when elle tourne, then elle est verte, sans modification des tests V1, 5.25 et 5.26.

## Design Notes

**Cles de lignee.** `G(x)` = `GlobalObjectId` de `x` ; module = racine d'instance de prefab ; arete = (noeud source, noeud cible) en sens de circulation.

| Entite | Cle |
|---|---|
| Section de module, Junction | `section:G(module)`, `junction:G(module)` |
| Section d'anneau | `section:G(Merge)>G(Split)` (trois par giratoire : la cle de module collisionnerait) |
| Corridor de module | `corridor:G(premier noeud propre)>G(dernier noeud propre)` (premier = sans predecesseur dans le module ; connecteurs fusionnes exclus) |
| Corridor d'anneau | `corridor:G(Merge)>G(Split)` par la meme regle |
| JunctionMovement | `movement:G(source)>G(cible)` de l'arete V1 qu'il dispose |
| LaneConnection | `connection:G(connecteur sortant)>G(connecteur entrant)` |
| Portal | `portal:G(noeud):Entry` ou `:Exit` |

**Decoupage.** Un corridor par voie de module a deux voies, une section par module. Connecteurs de carrefour fusionnes a l'extremite du corridor voisin (derive = ecart). Jointure non-carrefour/non-carrefour -> `LaneConnection` ; carrefour/carrefour -> echec dur. Un `Junction` par instance. Chaque arete V1 d'un noeud de decision -> un mouvement (poids = `RoutePreferenceWeight`, plus grand = prefere). Giratoire : 3 corridors d'anneau (sections a un corridor), 9 mouvements (entree `In->Merge`, sortie `Split->Out` 60, continuation `Split->Merge` 40). Poids 1 d'un choix unique = « trivial ».

**Geometrie.** Corridors par `RoadCurveBuilder` a travers leurs noeuds ; up = `up` du noeud. Mouvement = Hermite cubique dense entre pose de fin d'approche et pose de depart ; le noeud de decision est dispose « graine de controle/route ». Largeur candidate = moitie de l'ecart des voies appariees ; anneau et mouvements amorces depuis l'approche. Datum = voie de sens local prefab +z ; ordre = signe du decalage projete. Corde = critere 5.26 via la surcharge ; la moitie de budget spline-contre-intention n'est pas mesuree et le rapport le dit.

**Ecriture.** Hash source sur la source V1 extraite et canonisee (un decor deplace ne perime rien). Quantification numerique explicite avant hash et serialisation : positions et distances en entiers de 0,1 mm, angles et directions en 1e-6, poids en 1e-4, `-0` normalise ; le rapport affiche les mesures a precision fixe (m : 4 decimales, degres : 3). Le menu calcule lignee et rapport en memoire, ecrit deux `.tmp`, puis remplace la lignee, puis le rapport. `ponytail:` deux fichiers ne sont pas atomiques ensemble : un arret entre les deux remplacements laisse un couple que `Verify` rejette (hash de lignee), et relancer le menu le repare.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story527MigrationTests"` -- vert.
- `.\scripts\validate.ps1 -TestMode EditMode` -- suite complete verte, 0 erreur Console.

## Suggested Review Order

**Le passage unique et sa liaison**

- Point d'entree : extraction, import, validation, mesures, portee, rapport lie ; rien n'est ecrit ici.
  [`MigrationReport.cs:127`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/MigrationReport.cs#L127)

- Validation sans version hors Compile, jamais comptee deux fois.
  [`MigrationReport.cs:182`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/MigrationReport.cs#L182)

- Verify : rapport retouche, detache ou perime rejete, jamais repare.
  [`MigrationReport.cs:911`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/MigrationReport.cs#L911)

- Ecriture fail-closed : un passage en echec n'ecrit rien.
  [`MigrationReport.cs:1012`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/MigrationReport.cs#L1012)

- Menu garde : Play Mode, scene sale ou dechargee, lignee absente ou vide.
  [`MigrationReport.cs:959`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/MigrationReport.cs#L959)

**Ensemble source prouve**

- Racines explicites ; LaneNode hors module ou prefab inconnu = echec dur.
  [`V1SourceSet.cs:190`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1SourceSet.cs#L190)

- Jointures candidates regle V1, egalite et orphelins en echec dur.
  [`V1SourceSet.cs:362`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1SourceSet.cs#L362)

- Hash de la source extraite quantifiee, rotation de module comprise.
  [`V1SourceSet.cs:452`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1SourceSet.cs#L452)

**Decoupage et dispositions**

- Voies de module : datum par sens local +z, ordre lateral mesure.
  [`V1RoadModelImporter.cs:366`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs#L366)

- Jointures : fusion a l'extremite, LaneConnection, ou echec dur.
  [`V1RoadModelImporter.cs:487`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs#L487)

- Carrefours : anneaux, graines de decision, mouvements par arete V1.
  [`V1RoadModelImporter.cs:551`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs#L551)

- Mouvement Hermite dense, pas en longueur et en angle (coutures < 0,02 deg).
  [`V1RoadModelImporter.cs:803`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs#L803)

- Poids : polarite definie ou echec dur ; chaque poids vise un mouvement nomme.
  [`V1RoadModelImporter.cs:975`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs#L975)

- Completude : tout noeud et toute arete disposes, sinon echec dur.
  [`V1RoadModelImporter.cs:1027`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs#L1027)

- Assemblage du RoadModelSource candidat, aucun controle ni conflit invente.
  [`V1RoadModelImporter.cs:1107`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs#L1107)

**Identites stables**

- Resolution : preservee, frappee, ou tombstonee sans recyclage.
  [`RoadLineage.cs:154`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/RoadLineage.cs#L154)

- Lecture : fichier absent = premier import, vide ou incoherent = echec dur.
  [`RoadLineage.cs:73`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/RoadLineage.cs#L73)

- Collision de cle de lignee : echec dur nommant les deux sources.
  [`V1RoadModelImporter.cs:206`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1RoadModelImporter.cs#L206)

**Mesures et portee**

- Mesures et classement ; exception de lissage bornee a l'axe d'approche.
  [`MigrationReport.cs:324`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/MigrationReport.cs#L324)

- Portee sur topologie V2, sortie strictement en avant.
  [`MigrationReport.cs:389`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/MigrationReport.cs#L389)

- Surcharge 5.26 : max mesure du critere de subdivision, construction inchangee.
  [`RoadCurveBuilder.cs:228`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadCurveBuilder.cs#L228)

**Peripheriques**

- Instantane lie : effectifs, dispositions, aucune boucle de corridor.
  [`Story527MigrationTests.cs:30`](../../Assets/RoadRage/Tests/EditMode/Story527MigrationTests.cs#L30)

- Rapport et lignee committes egaux a un import frais.
  [`Story527MigrationTests.cs:332`](../../Assets/RoadRage/Tests/EditMode/Story527MigrationTests.cs#L332)

- Rapport genere : liaison, mesures, taches d'authoring, dispositions.
  [`migration-report-5-27-mvp-run.md:1`](migration-report-5-27-mvp-run.md#L1)

- Artefacts haches epingles en LF.
  [`.gitattributes`](../../.gitattributes)
