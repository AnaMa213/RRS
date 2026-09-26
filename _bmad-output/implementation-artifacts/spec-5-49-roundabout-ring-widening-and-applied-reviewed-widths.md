---
title: 'Story 5.49 -- Elargissement des anneaux de giratoire et largeurs revues appliquees'
type: 'feature'
created: '2026-09-23'
status: 'done'
baseline_commit: '9ab80d6005f03e24703c1e4f512f36861a4c9c3d'
review_loop_iteration: 0
context:
  - '_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-23.md'
  - '_bmad-output/implementation-artifacts/spec-5-28-junction-semantic-authoring-and-overlay-sign-off.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Les 4 giratoires de `MVP_Run` ne contiennent pas deux vehicules cote a cote (anneau V2 4,0 m, residu a deux gabarits -1,78 m ; anneau physique 5,0 m, -0,88 m), et le pipeline 5.28 refuse toute largeur revue differente de l'amorce de l'importeur : la Gate A ne peut pas signer les giratoires.

**Approach:** Elargir physiquement le prefab `Greybox_Roundabout` (ilot reduit, disque de chaussee) et degager par overrides d'instance les trottoirs des 12 modules adjacents ; faire APPLIQUER la largeur revue par le pipeline 5.28 (anneau 4,0 / 4,0 m, mouvements interpoles), publier imported/applied et le residu a deux gabarits V2 + physique par giratoire ; prouver par regression V1 avant/apres que tout delta comportemental est soumis au proprietaire ; regenerer les artefacts 5.28 pour la nouvelle revue.

## Boundaries & Constraints

**Always:** Donnees et topologie de trafic V1 (LaneNode, successeurs, poids, connecteurs, portails) intactes : hash source V1 et lignee 5.27 identiques octet pour octet (empreintes pre-changement : `source-hash` `b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc`, SHA-256 de `MVP_Run.road-lineage.json` `f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4`), zero identite frappee ou retiree. Un corridor logique d'anneau. Invariant d'axe renegocie par le proprietaire (2026-09-23, revue de spec) : les noeuds V1 d'anneau restent au rayon 6,0 m ; l'axe importe/compile (representation cordes + Hermite, mesure ~5,83-6,19 m du centre) est la geometrie de reference acceptee et reste INCHANGE ; la 5.49 ne change que l'enveloppe de largeur. Cible contraignante : anneau V2 `HalfWidthLeft` = `HalfWidthRight` = 4,0 m sur tout echantillon d'anneau ; ilot physique <= 1,75 m ; rayon exterieur physique degage/pave >= 10,25 m, mesure sur les colliders. Residu a deux gabarits strictement positif sur l'enveloppe V2 appliquee ET sur l'anneau physique, entrees tirees du seul profil de validation versionne (jamais de nouvelle constante), publie par instance ; il ne permet jamais de reduire la cible. Overrides d'instance autorises sur les 8 `Ring_*` et les 4 `TunnelPortal_*` adjacents, prefabs partages intacts. Baseline de regression V1 capturee AVANT toute modification de prefab ou de scene ; tout delta rapporte au proprietaire. Double garde d'etat avant/apres toute operation Editeur ; sauvegarde de scene seulement si l'etat dirty est explique. Ecriture fail-closed des artefacts ; tests 5.25/5.26/5.27 inchanges.

**Ask First:** Modifier les prefabs partages `Greybox_RoadSegment_TwoWay` ou `Greybox_TunnelPortal` (les overrides d'instance dans `MVP_Run` sont la voie retenue) ; toucher un `LaneNode` ou une donnee de trafic V1 ; relacher une valeur de la cible ; changer de conception physique si le disque de chaussee echoue a la mesure ; toute largeur de carrefour en croix ou en T differente de l'actuelle.

**Never:** Second corridor ou `LaneAdjacency` sur l'anneau ; elargir en deplacant des noeuds ; genre de controle giratoire (5.35) ; signature Gate A (reste en 5.28) ; amorce de l'importeur traitee comme autorite ; delta comportemental V1 accepte parce que le hash source est inchange.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Largeur revue != amorce | section d'anneau 4,0 / 4,0, amorce 2,0 / 2,0 | ecrite sur chaque echantillon possede ; rapport importee / appliquee | N/A |
| Asymetrique | section 3,5 / 4,0 | appliquee exactement telle qu'authoree | N/A |
| Mouvement d'entree/sortie | carrefour en `EndpointInterpolation` | g/d interpoles en `s/Length` entre largeurs appliquees des corridors d'extremite (2,0 -> 4,0) | N/A |
| Sous le gabarit | une demi-largeur possedee ou interpolee < `MaxVehicleHalfWidthMeters + LateralClearanceMarginMeters` | echec dur nommant sujet et echantillon | rien n'est ecrit |
| Plancher de carrefour viole | echantillon interpole < g/d de la decision `EndpointInterpolation` | echec dur | rien n'est ecrit |
| Mode interdit | `EndpointInterpolation` sur une section | echec dur | rien n'est ecrit |
| Portail sur sujet elargi | largeur appliquee != importee sur un corridor portant un portail | echec dur (enveloppe de portail derivee de l'import) | rien n'est ecrit |
| Collider non supporte | collider hors `Box`/`MeshCollider` convexe, dans la tranche de sonde, a portee du disque pave | echec dur nommant le collider | rien n'est ecrit |

</frozen-after-approval>

*Renegociation approuvee le 2026-09-25 (invariant d'axe d'anneau), a realiser par la Story 5.50 -- voir Spec Change Log. Le bloc gele ci-dessus reste l'etat livre par la 5.49.*

## Code Map

- `.../Traffic/Migration/AuthoredRoadModel.cs:259-389` -- `ApplyDecisions` : bloc « largeurs revues » a remplacer (compare -> applique) ; `Copy` (:570) est SUPERFICIEL (tableaux et `Samples` partages avec `import.Source`) : cloner avant ecriture. `PipelineVersion` (:128) 1 -> 2. `RenderBody` (:1032), section « Largeurs revues » (:1135) ; nouvelle section giratoires apres elle.
- `.../Traffic/Migration/AuthoringDecisions.cs:70,105,222,278,409,541` -- `WidthDecision`, `FormatVersion`, parse/serialise, `UniformWidth` (amorce), `WidthRecord` ; `TryParseDeclaredEnum` pour le nouvel enum.
- `.../Traffic/Migration/V1RoadModelImporter.cs:98-160,644-711,803-865,1232` -- `ImportedCurve{IsRing,Module,Role,From,To}`, `ImportRing`, interpolation d'origine, enveloppe de portail (min g/d a l'import). Lecture seule.
- `.../Traffic/Migration/V1SourceSet.cs:37,243-247` -- `V1Module` : ajouter `Root` (Transform, hors hash) renseigne dans `Extract`.
- `.../Traffic/RoadModelRecords.cs:295,362,461,643` -- `RoadCurveSample{SMeters,HalfWidth*}`, `LaneCorridor`/`JunctionMovement.Samples`, profil (1,03 / 4,5 / 0,25).
- `Tests/EditMode/Story528AuthoringAndGateATests.cs:215,578-610` -- test `ADivergentWidth...` a remplacer ; harnais `Fresh`/`RunWith`/`WithMvpRun`.
- `Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs:320-400,945-975` -- contraintes V1 sur le prefab : `Col_Island` lu par `localScale.x * 0.5`, 0 `m_IgnoreFromBuild` et 0 `Col_Sidewalk` dans le giratoire, `NavMeshModifier` seulement en aire Sidewalk.
- `Assets/RoadRage/Prefabs/Greybox_Roundabout.prefab` -- `Col_Roadway_Disc` box 16x16, `Col_Island` cylindre convexe echelle 6 (r 3,0, sommet affleurant y 0, `NavMeshModifier`), visuels Synty (16 tuiles route, 4 tuiles herbe 0,42).
- Mesure Editeur 2026-09-23 (centre de chaque giratoire) : `Col_Sidewalk_*` des 8 `Ring_*` et des 4 `TunnelPortal_*` a 8,94 m, 16 m de long, laterale 4-8 m ; `Col_Wall_*` du portail a 11,31 m ; `Col_Roof` a y 5-6 ; sol `Greybox_GroundPlane` sommet -0,05.
- `scripts/validate.ps1` -- n'affiche que les echecs : capturer `unity cmd test_status --format json` apres chaque passage.

## Tasks & Acceptance

**Execution:**
- [x] `_bmad-output/implementation-artifacts/v1-regression-5-49/` -- AVANT tout changement, session Editeur fraiche : `validate.ps1 -TestMode EditMode` puis `-TestMode PlayMode`, chacun suivi de `test_status` -> `baseline-editmode.json`, `baseline-playmode.json` ; `baseline-hashes.txt` : SHA-256 de `MVP_Run.road-lineage.json`, `source-hash` fraichement extrait, SHA-256 du rapport 5.27.
- [x] `Assets/RoadRage/Prefabs/Greybox_Roundabout.prefab` -- `Col_Island` echelle 3,0 (r 1,5) ; `Collision/Col_Roadway_Ring` : `MeshCollider` convexe cylindre integre, echelle (21, 0,1, 21), sommet y 0, sans `NavMeshModifier` ; visuels : tuiles herbe ramenees dans r 1,5, disque de chaussee visuel 1 cm sous y 0 (materiau route existant, aucun nouvel asset). Noeuds intacts.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- overrides d'instance sur les 8 `Ring_*` et les 4 `TunnelPortal_*` adjacents : `Col_Sidewalk_Left/Right` raccourcis de 4 m cote giratoire (taille z 16 -> 12, centre decale), tuiles `Walk_*` des 4 premiers metres desactivees ; rebake du `NavMeshSurface` (donnee authoree, AD-33). Sous double garde. Apres rebake, `git diff --stat` limite a `MVP_Run.unity` (overrides + reference de bake), `MVP_Run/NavMesh-LaneGraph-Vehicle.asset` et au prefab giratoire ; tout autre fichier = STOP.
- [x] `.../Traffic/Migration/AuthoringDecisions.cs` -- enum `WidthApplication {Uniform, EndpointInterpolation}` porte par `WidthDecision.Application` (par nom, parse strict) ; `FormatVersion` 2 ; amorce en `Uniform`.
- [x] `.../Traffic/Migration/V1SourceSet.cs` -- `V1Module.Root`.
- [x] `.../Traffic/Migration/AuthoredRoadModel.cs` -- application des largeurs (Design Notes), toutes les lignes de la matrice, imported/applied par sujet ; appel de la mesure giratoire ; rapport ; `PipelineVersion` 2.
- [x] `.../Traffic/Migration/RoundaboutClearance.cs` -- residu a deux gabarits (formule) ; enveloppe V2 ; mesure physique par empreinte 2D (Design Notes).
- [x] `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json` -- format 2 ; 12 sections d'anneau 4,0 / 4,0 `Uniform` ; 4 carrefours giratoires `EndpointInterpolation` plancher 2,0 / 2,0 ; le reste `Uniform` inchange ; `Conflicts`/`Controls`/`Dispositions` inchanges. Puis menu `Compiler le modele authore` : modele, overlay, rapport 5.28 regeneres.
- [x] `Tests/EditMode/Story528AuthoringAndGateATests.cs` -- remplacer `ADivergentWidth...` par : applique (dont asymetrique), sous le gabarit refuse, mode interdit, plancher, portail elargi ; garder « Largeur non revue ».
- [x] `Tests/EditMode/Story549RoundaboutWideningTests.cs` -- formule sur les exemples dores ; sur `MVP_Run` : `source-hash` frais et SHA-256 de la lignee committee egaux aux empreintes pre-changement (constantes du test), lignee re-serialisee par la migration 5.27 egale octet pour octet au fichier committe, sans frappe ni retrait ; aucune source Traffic V2 (`Features/Vehicles/Traffic`) ne reference `NavMesh`/`UnityEngine.AI` ; par instance : echantillons d'anneau 4,0 / 4,0, positions/tangentes d'axe egales a l'import, ilot <= 1,75, pave >= 10,25, aucun collider non-chaussee a moins de 10,25, residus V2 et physique > 0 ; rapport contenant importee / appliquee.
- [x] `v1-regression-5-49/` -- apres changement : EditMode (meme session) ; HALT redemarrage Editeur ; PlayMode ; `comparison.md` : empreintes avant/apres (lignee, source, rapport 5.27), statut par test avant/apres, tests ajoutes/retires, chaque delta a disposer par le proprietaire. HALT si delta.
- [x] `sprint-status.yaml` (5.49 -> `in-progress` puis `review`) ; `spec-5-28` : note sous le HALT (5.49 livree, artefacts regeneres, nouvelle `RoadModelVersion`) ; `graphify update .`.

**Acceptance Criteria:**
- Given les 4 instances, when elles sont mesurees, then l'anneau compile porte 4,0 / 4,0 m sur tout echantillon d'anneau et de continuation, son axe est identique a l'import (noeuds V1 au rayon 6,0 m, bande ~5,83-6,19 m inchangee), l'ilot <= 1,75 m et le rayon pave >= 10,25 m sans collider trottoir/bordure en deca.
- Given deux gabarits max du profil, when ils sont places cote a cote au point le plus serre, then le residu est > 0 sur l'enveloppe V2 et sur l'anneau physique, publie par instance.
- Given le changement physique, when la source V1 est re-extraite, then le `source-hash` et le SHA-256 des octets de la lignee egalent les empreintes pre-changement et aucune identite n'est frappee ni retiree.
- Given les suites V1 avant/apres, when elles sont comparees, then chaque delta est liste pour le proprietaire et aucun n'est accepte d'office.
- Given les artefacts regeneres, when la suite EditMode complete tourne, then seul `GateAIsOpenedOnlyByTheOwnersBoundSignoff` echoue (sign-off absent, 5.28) et les artefacts committes egalent un pipeline frais.

## Spec Change Log

- **2026-09-25 -- renegociation approuvee par le proprietaire (sprint-change-proposal-2026-09-25.md), a realiser par la Story 5.50 (axe) et par la Story 5.28 a sa reprise (preuve physique des giratoires).**
  Invariant gele renegocie (Always) : « l'axe importe/compile (representation cordes + Hermite, mesure ~5,83-6,19 m du centre) est la geometrie de reference acceptee et reste INCHANGE ». Remplacement approuve : l'axe de l'anneau deviendra le cercle exact ajuste sur les noeuds V1 d'anneau (rayon 6,0 m), chaque noeud associe par lignee a 0,10 m au plus du cercle ; les noeuds ne bougent pas.
  Conditionnel (matrice, `EndpointInterpolation` 2,0 -> 4,0 m sur les entrees et sorties) : conserve, sauf echec des regles de repli de la 5.50 sur une entree ou une sortie ; une autre loi de largeur exigera alors une decision du proprietaire.
  Preuve physique : les mesures 5.49 (residus a deux gabarits, rayons d'ilot et de chaussee) ont ete faites sur l'ancienne geometrie V2. `RoundaboutClearance` sera etendu par la 5.28 a sa reprise pour les recalculer sur les trajectoires finales 5.50 (balayage conservateur de l'empreinte, transitions vers les corridors adjacents incluses, residu d'anneau recalcule), et c'est cette preuve recalculee que liera la Gate A. Un residu non positif arretera le travail pour une decision du proprietaire.
  Inchange : geometrie physique et largeurs appliquees de la 5.49, cible contraignante (4,0 / 4,0 m, ilot <= 1,75 m, chaussee >= 10,25 m), hash source et lignee, un seul corridor d'anneau.
  Design Notes « L'axe ne change pas, donc les candidats de conflit non plus » : vrai pour la 5.49 seule ; avec la 5.50, les candidats changeront et passeront par le differentiel exhaustif et la reconfirmation du proprietaire.

## Design Notes

**Application.** Sections d'abord : chaque echantillon de chaque corridor recoit g/d de la decision (`Uniform` seul admis). Puis carrefours : `Uniform` ecrit g/d sur chaque echantillon de mouvement ; `EndpointInterpolation` ecrit `lerp(fin appliquee de From, debut applique de To, s/Length)`, g/d de la decision valant plancher. Ensuite gabarit sur tout echantillon. L'axe ne change pas, donc les candidats de conflit non plus : la bijection candidats/decisions existante le prouve.

**Frontiere de carrefour (revue 5.49).** Un carrefour dont une largeur appliquee differe de l'import recalcule `Junction.Boundary` sur ses echantillons appliques, avec la regle de l'importeur (position +/- max(g, d) sur les trois axes) ; les carrefours non elargis gardent leur frontiere importee a l'identique. Un carrefour `Uniform` dont la largeur differe de ses corridors d'extremite est refuse par le compilateur (`MovementSeamBroken`).

**Residu.** `R_in = r_in + m + W/2 ; c_in = sqrt((R_in + W/2)^2 + (L/2)^2) ; R_out = c_in + 2m + W/2 ; c_out = sqrt((R_out + W/2)^2 + (L/2)^2) ; residu = (r_out - m) - c_out`. Exemples dores (W/2 1,03, L 4,5, m 0,25) : (4, 8) -> -1,78 ; (3, 8) -> -0,88 ; (2, 10) -> +1,99. V2 : centre = `Root` du module ; anneau = corridors `IsRing` + continuations ; par echantillon, bord interieur/exterieur = le plus proche/lointain du centre ; `r_in` = max des interieurs, `r_out` = min des exterieurs (estime ~2,19 / ~9,83, residu ~+1,67 ; MESURE le 2026-09-23 : 2,55 / 9,83, residu +1,35 -- les bords des continuations Hermite ne sont pas radiaux ; physique : ilot 1,50, pave 10,37, obstacle le plus proche `Col_Wall_Left` du portail a 11,31, residu +2,78). Physique : empreinte XZ (enveloppe convexe des coins de `BoxCollider` ou des sommets de `MeshCollider` convexe) ; `r_in` = portee max de `Col_Island` du module ; pave = min sur 720 rayons (0,5 deg, pas 1 cm) de la sortie de l'union des `Col_Roadway*` ; obstacles = colliders non declencheurs hors `Col_Roadway*`/ilot dont la hauteur recoupe [sommet de route, +2 m] ; `r_out` = min(pave, obstacle le plus proche). `ponytail:` classement par nom de collider et tranche de 2 m, outil de mesure seulement ; a remplacer par une couche physique si un decor vient s'y ajouter.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story549RoundaboutWideningTests"` -- vert.
- `.\scripts\validate.ps1 -TestMode EditMode` -- seul echec attendu `GateAIsOpenedOnlyByTheOwnersBoundSignoff`, 0 erreur Console.
- `.\scripts\validate.ps1 -TestMode PlayMode` (session fraiche, avant ET apres) -- comparaison dans `comparison.md`.

**Manual checks:**
- Controle visuel des 4 giratoires et de la branche diagonale (portail tunnel) dans l'Editeur, sous double garde ; overlay regenere pret pour la nouvelle revue 5.28.

## Suggested Review Order

**Largeurs revues appliquees (point d'entree)**

- Largeurs appliquees sur copies : sections `Uniform`, puis carrefours.
  [`AuthoredRoadModel.cs:491`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L491)
- Entrees/sorties interpolees en s/Length entre largeurs appliquees ; decision = plancher.
  [`AuthoredRoadModel.cs:565`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L565)
- Frontiere recalculee seulement pour un carrefour reellement elargi (constat de revue).
  [`AuthoredRoadModel.cs:601`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L601)
- Gabarit par courbe, echec nommant sujet, courbe et echantillon.
  [`AuthoredRoadModel.cs:633`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L633)
- Portail sur sujet elargi refuse : son enveloppe derive de l'import.
  [`AuthoredRoadModel.cs:622`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L622)

**Format des decisions**

- Regle d'application explicite, portee par la decision.
  [`AuthoringDecisions.cs:46`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs#L46)
- Parse strict par nom ; format 2.
  [`AuthoringDecisions.cs:243`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs#L243)

**Mesure des giratoires et residu a deux gabarits**

- Mesure appelee apres compilation 2, avant toute ecriture.
  [`AuthoredRoadModel.cs:248`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L248)
- Formule du residu, entrees du seul profil versionne.
  [`RoundaboutClearance.cs:57`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/RoundaboutClearance.cs#L57)
- Enveloppe V2 : pire bord interieur et exterieur, enveloppe vide refusee.
  [`RoundaboutClearance.cs:114`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/RoundaboutClearance.cs#L114)
- Anneau physique par empreintes 2D ; bornes synchronisees en EditMode.
  [`RoundaboutClearance.cs:167`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/RoundaboutClearance.cs#L167)
- Racine d'instance hors hash source.
  [`V1SourceSet.cs:50`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/V1SourceSet.cs#L50)
- Tableau publie par instance dans le rapport Gate A.
  [`AuthoredRoadModel.cs:1385`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L1385)

**Geometrie physique**

- Disque de chaussee convexe r 10,5 ; ilot r 1,5 ; noeuds intacts.
  [`Greybox_Roundabout.prefab:2960`](../../Assets/RoadRage/Prefabs/Greybox_Roundabout.prefab#L2960)
- Overrides d'instance : 24 trottoirs raccourcis, 24 tuiles masquees.
  [`MVP_Run.unity`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity)

**Peripheriques**

- Cible, axe inchange, interpolation et frontieres sur la vraie carte.
  [`Story549RoundaboutWideningTests.cs:76`](../../Assets/RoadRage/Tests/EditMode/Story549RoundaboutWideningTests.cs#L76)
- Lignee et hash source identiques aux empreintes pre-changement.
  [`Story549RoundaboutWideningTests.cs:37`](../../Assets/RoadRage/Tests/EditMode/Story549RoundaboutWideningTests.cs#L37)
- Collider non supporte : echec dur a portee, ignore au-dela.
  [`Story549RoundaboutWideningTests.cs:197`](../../Assets/RoadRage/Tests/EditMode/Story549RoundaboutWideningTests.cs#L197)
- Application, asymetrie et refus de marche de largeur de carrefour.
  [`Story528AuthoringAndGateATests.cs:215`](../../Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs#L215)
- Gabarit, mode interdit, plancher, portail elargi.
  [`Story528AuthoringAndGateATests.cs:250`](../../Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs#L250)
- Regression V1 avant/apres : aucun delta.
  [`comparison.md`](v1-regression-5-49/comparison.md)
