---
title: 'Story 5.50 -- Correction de la geometrie des mouvements V2 et validation de conduisibilite au niveau modele'
type: 'feature'
created: '2026-09-26'
status: 'draft'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/planning-artifacts/epics.md'
  - '{project-root}/_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-25.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-28-junction-semantic-authoring-and-overlay-sign-off.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le diagnostic du 2026-09-24 montre que les mouvements V2 de `MVP_Run` ne sont pas conduisibles : entrees et sorties de giratoire jusqu'a un rayon de 0,25 m, anneau oscillant entre 5,83 et 6,19 m, virages a gauche qui tournent des la frontiere du carrefour. Aucune regle du validateur ne borne la courbure ni le repli d'enveloppe. Signer la Gate A figerait ces trajectoires dans toutes les stories suivantes.

**Approach:** Normatif : `epics.md` Story 5.50 (lignes 2172-2338) ; cette spec en est la projection d'execution et ne la resume pas. Reconstruire dans l'importeur l'anneau (cercle exact ajuste), les entrees, sorties et continuations tangentes, et les 24 virages selon la regle de frontiere de chaussee croisee. Ajouter un `DrivabilityProfile` declare, valide au niveau modele (admission R >= 4,0344 m, C1-C5, F1-F3) et exige par `RoadModelDocument.Load` (format 2). Balayer les candidats avec l'empreinte complete. Empreinter chaque decision de conflit. Publier un differentiel exhaustif en mode diff lecture seule, puis HALT pour les decisions du proprietaire avant toute regeneration definitive.

## Boundaries & Constraints

**Always:** Source V1, hash source, lignee 5.27 et identites identiques octet pour octet ; aucun `LaneNode` deplace. Un seul corridor d'anneau ; geometrie physique et largeurs appliquees de la 5.49 conservees. Referentiel de conduite = reference compilee (evaluation canonique `RoadCurve`) ; toute preuve de confinement est gonflee de delta_c = 0,05 m. Le temoin n'existe que dans le validateur. Payload canonique d'un modele non declare et fixtures 5.25/5.26 identiques octet pour octet ; `CompilerSchemaVersion` inchange. Derive des noeuds V1 mesuree par lignee, jamais contre la courbe la plus proche. Le pipeline n'ecrit, ne transfere, ne supprime ni n'approuve aucune decision. Fichiers de tests 5.25/5.26/5.27 non modifies. Execution dans l'ordre contraignant de la story (etapes 1 a 5).

**Ask First:** Toute exception a la regle de voie hors virages a droite ; repli sur une construction a sauts de courbure ; echec d'une regle de repli sur une entree ou une sortie (autre loi de largeur) ; nouvelle evidence exigeant de toucher la geometrie physique 5.49 ; tout test 5.25/5.26/5.27 qui echoue apres le changement ; tout ecart entre la spec et la story normative.

**Never:** Deplacer un `LaneNode` ; relacher une porte ou remodeler une geometrie acceptee pour faire passer un test ; porte de conduisibilite propre au pipeline ; derive mesuree contre la courbe la plus proche ; reporter une decision sur une geometrie modifiee ; changement physique (5.51) ; changement V1 ; genre de controle giratoire (5.35) ; runtime ; signature Gate A ; second corridor d'anneau ou adjacence ; decision ecrite par l'agent.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Rayon limite | element declare, R = 4,0344 m | admis, plafond = 0,25 m/s | N/A |
| Rayon refuse | R = 4,0064 m ; R <= 1,55 m | refuse ; aucun NaN produit | echec nommant element, echantillon, regle |
| Canaux incoherents | tangentes tournees de 30 deg sur positions droites ; cercle stocke kappa 0 ou 2/R ; tangentes +2 deg sur cercle exact | refuse (C5 pour le dernier) | echec nommant element, echantillon, regle |
| Repli | R 4,5 m, demi-largeur interieure 4,6 m ; pic F2 en milieu de segment | refuse | idem |
| Couture | saut 1 deg a 4 m de demi-largeur / 0,5 deg | refuse / admis | idem |
| Up variable | element declare, road-up non constant | refuse (C4) | idem |
| Non declare | modele sans profil via `RoadModelDocument.Load` | refuse | echec explicite |
| Format 1 | document format 1 lu par le chemin format 2, et inverse | refuse (format, re-serialisation, hash) | echec explicite |
| Paire modifiee | empreinte de decision != empreinte fraiche | proposition historique, Gate A fermee | revue ciblee |
| Paire nouvelle / retiree | candidat sans decision / decision sans candidat | echec dur jusqu'a decision du proprietaire | aucune ecriture par le pipeline |
| Chaine courte | corridor < demi-vehicule entre deux mouvements | paire trouvee | N/A |

</frozen-after-approval>

## Code Map

Racine : `Assets/RoadRage/Features/Vehicles/Traffic/`.
- `RoadModelRecords.cs:643-678,713-735` -- `RoadModelValidationProfile` (1,03 / 4,5 / 0,25 aux :646,649,652 ; coutures :658,661) ; `RoadModelSource` : ajouter `DrivabilityProfile{Declared,...}`. Ne PAS l'ajouter au profil de validation (serialise en bloc par JsonUtility, `RoadModelDocument.cs:211,661`).
- `RoadModelCanonicalWriter.cs:14-30,110-285,377` -- payload et `Write` ; bloc declare ajoute apres Portals (:274-284) et ecrit SEULEMENT si `Declared` (pas d'octet de presence, contrairement a `HasStopLine` :222-227).
- `RoadModelCompiler.cs:34,44-106` -- `CompilerSchemaVersion = 4` inchange ; derivation delta(v), R(delta), plafond v*(kappa).
- `RoadModelDocument.cs:31,72-128,134-142,636-674` -- `Format` 1 -> 2 ; `Load` : format :89, re-serialisation :97, hash :102 ; refus du non declare apres `FromDto` (:113).
- `RoadCurve.cs:114-120,132-195,280-307` -- `Sample`/`Project`/`Interpolate` : reference compilee. Bords implicites `Position -/+ Right*HalfWidth` (:231-247) = enveloppe de F1.
- `RoadCurveBuilder.cs:88-118,180-266` -- Catmull-Rom centripete, corde <= 0,025 m, courbure :256-264. Les elements construits n'y passent plus (echantillonnage analytique direct).
- `RoadGeometryValidator.cs:21,40-159,299-324` -- `UnitTolerance` 1e-3 ; phases forme/relations ; `SeamDefect` reutilise pour C3/F3. Nouvelle phase conduisibilite apres :159, gardee par `Declared`.
- `RoadModelValidator.cs:11-95,292-294,558-561,665-668` -- codes (dernier `TangentOpposesChord = 33`, nouveaux >= 34) ; plafonds de couture 0,05 / 5.
- `Migration/V1RoadModelImporter.cs:224,227,239-251,551-642,644-711,730-732,745-797,803-917,1252-1270` -- `ImporterVersion` 1 -> 2 ; delta_c :227 ; profil ; anneau (Catmull-Rom par les noeuds, 3 corridors `Ring_Merge -> Ring_Split` par giratoire) ; entrees/sorties/continuations et virages = un Hermite frontiere a frontiere (`AddMovement` :803-897), re-passe par `RoadCurveBuilder` (:882) ; `Describe` (seuil 30 deg) ; `Boundary` = AABB des mouvements (depend des mouvements : NE PAS l'utiliser comme chaussee croisee). Pas de structure de bras : partenaire antiparallele = autre corridor de la meme `ImportedSection` a tangente opposee (voir `CountSameDirectionPairs` :1077-1094 ; `_mergedAtEnd/_mergedAtStart` :288-289).
- `Migration/V1SourceSet.cs:47,50,251,443-480` -- `V1Module.Root`/`Rotation` ; hash source (poses et roles des noeuds seulement).
- `Migration/RoadLineage.cs:135-210` -- cles/genres/ids seulement, sans geometrie : stable si l'ensemble des cles est inchange.
- `Migration/MigrationReport.cs:112-137,176,325-365,1039-1086` -- derive 0,10 m deja par lignee (vertex/merged/graines) ; exception `JustifiedException` :356-363 ; a etendre aux connecteurs d'extremite et ancres ; menu « Migrer MVP_Run » (lignee + rapport 5.27).
- `Migration/AuthoredRoadModel.cs:150,172-281,751-798,828-927,1071-1244,1585-1768,1826-1891` -- `PipelineVersion` 2 ; `Run` ; `MatchConflicts` (bijection, echecs durs :768,788) ; `PairKey` ; `Candidates` + `SweptRadius`/`SweptOverlap` (balayage lateral seul, `ponytail:` :876-877) ; `SameApproachPairs` :1513-1530 (compte seulement) ; overlay (`BuildOverlay`/`RenderOverlay`) ; Gate A (`EvaluateGateA` :1762) ; menus et `TryWriteAll`. Mode diff = nouveau `[MenuItem]` pres de :1851.
- `Migration/AuthoringDecisions.cs:18-22,69-80,109-122,157-330,341-368,553` -- `ConflictDecision` (Id, cles A<B, Decision, Reason), `FormatVersion` 2 -> 3, parse/serialise, `Propose` (tout Accepted -- ne pas l'utiliser pour disposer).
- `Migration/GateAReviewWindow.cs:33-39,52,122-242,265-404,439-466` -- Handles seulement ; filtre `ConflictFilter.Movement` existant = point d'accroche de la vue d'isolement.
- `Migration/RoundaboutClearance.cs:116` -- seul usage du centre de giratoire (`module.Root.position`).
- `Features/Vehicles/VehicleProfileDef.cs:40-67`, `VehicleProfile.cs:131-146,400-406`, `VehicleSteeringModel.cs:39-64`, `ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset:20-64` -- braquage 40 -> 16 deg a 26 m/s, inactif si |v| < 0,25 ; empattement et point de reference DERIVES des roues z +/-1,55 (pas de champ).
- `Tests/EditMode/Story528AuthoringAndGateATests.cs:338,396-421,555-564,628-675` -- `{"Format":1,` code en dur (:338) casse au format 2 ; artefacts == pipeline frais ; `GateAIsOpenedOnlyByTheOwnersBoundSignoff` rouge par conception ; harnais `Fresh`/`RunWith`/`WithMvpRun`.
- `Tests/EditMode/Story527MigrationTests.cs:41-85,120-123,139-190,359-371` -- comptes (72 mouvements...), exceptions par libelles, rapport compare octet pour octet (passe apres regeneration). Fichier non modifiable.
- `App/Scenes/MVP_Run/MVP_Run.road-authoring.json` -- 76 decisions, toutes `Accepted`, sans empreinte.

## Tasks & Acceptance

**Execution:**
- [ ] `_bmad-output/implementation-artifacts/v1-regression-5-50/` -- AVANT tout code, Editeur connecte : `baseline-hashes.txt` (hash canonique dore d'un modele non declare -- aucun n'existe aujourd'hui --, SHA-256 de la lignee, `source-hash`, rapport 5.27, modele/overlay/rapport 5.28 committes) ; `baseline-editmode.json`.
- [ ] `RoadModelRecords.cs`, `RoadModelCompiler.cs`, `RoadModelCanonicalWriter.cs` -- `DrivabilityProfile` et derivations (Design Notes) ; bloc canonique ecrit seulement si declare.
- [ ] `RoadGeometryValidator.cs`, `RoadModelValidator.cs` -- phase conduisibilite : admission, C1-C5, F1-F3 ; codes >= 34 ; message nommant element, echantillon, regle.
- [ ] `RoadModelDocument.cs` -- format 2 ; refus du non declare ; refus croise format 1/2.
- [ ] `Migration/V1RoadModelImporter.cs` -- `ImporterVersion` 2 ; profil declare copie de `VehicleProfileDef_Default` ; anneau, entrees, sorties, continuations, virages (Design Notes) ; echantillonnage analytique (corde <= delta_c/2) ; frontieres de chaussee derivees des enveloppes de corridors.
- [ ] `Migration/MigrationReport.cs` -- derive par lignee etendue ; tables publiees par element (rayon min, plafond et distribution, cap net/cumule, sauts de couture, ecart courbure/cercle a trois points, deplacement d'ancre, distance d'exception des virages a droite).
- [ ] `Migration/AuthoringDecisions.cs` -- `FormatVersion` 3 ; `GeometryFingerprint` par decision (vide admis a la lecture = non reconfirmee).
- [ ] `Migration/AuthoredRoadModel.cs` -- balayage a empreinte complete ; paires de suivi publiees ; elements < `MaxVehicleLengthMeters` listes ; empreinte fraiche par paire ; `MatchConflicts` : empreinte differente ou vide = proposition historique, Gate A fermee ; menu mode diff (ecrit seulement `review-5-50-diff.md`) ; menu proprietaire « Reconfirmer les paires inchangees » (Design Notes) ; `PipelineVersion` 3.
- [ ] `Migration/GateAReviewWindow.cs` -- vue d'isolement d'un mouvement (marqueurs, chevrons, rayon min, plafond, rouge hors admission, enveloppe optionnelle, noeuds V1 sources), lecture seule, texte d'overlay inchange.
- [ ] `Tests/EditMode/Story550DrivabilityTests.cs` -- toute la section « EditMode verification » de la story, matrice ci-dessus incluse ; test profil == `VehicleProfileDef_Default` ; V1 ne reference pas Traffic V2.
- [ ] `Tests/EditMode/Story528AuthoringAndGateATests.cs` -- `{"Format":1,` -> format 2 ; decision sur paire modifiee bloque la Gate A.
- [ ] Mode diff sur `MVP_Run` -> `review-5-50-diff.md`. **HALT : decisions du proprietaire** (nouvelles, modifiees, retirees), chacune visible dans la vue d'isolement.
- [ ] Apres resolution complete : « Migrer MVP_Run » (rapport 5.27, `ImporterVersion` 2) puis « Compiler le modele authore » (artefacts 5.28) ; suite EditMode complete ; `sprint-status.yaml` ; note sous le HALT de `spec-5-28` ; `graphify update .`.

**Acceptance Criteria:**
- Given l'importeur corrige et la source V1 inchangee, when `MVP_Run` est importe, then hash source et lignee sont identiques octet pour octet, aucune identite n'est frappee ni retiree, et chaque noeud V1 est a 0,10 m au plus de ses elements associes par lignee (seule exception : noeud de decision lisse par un virage).
- Given `MVP_Run` corrige, when il est compile, then tous les corridors et mouvements sont admis, les noeuds d'anneau sont a 0,10 m du cercle ajuste, entrees et sorties rejoignent l'anneau tangentiellement, et chaque virage respecte la regle de voie contre une frontiere identique avant et apres la correction.
- Given une decision dont la geometrie de paire a change, when la Gate A est evaluee, then elle est montree comme proposition historique dans la revue ciblee et la Gate A reste fermee jusqu'a reconfirmation ou changement par le proprietaire.
- Given le differentiel exhaustif, when il contient des paires nouvelles, retirees ou modifiees, then le travail s'arrete et aucun artefact 5.28 definitif n'est regenere avant resolution complete.
- Given les artefacts regeneres, when la suite EditMode complete tourne, then les tests 5.25/5.26/5.27 passent sans modification, seul `GateAIsOpenedOnlyByTheOwnersBoundSignoff` echoue, et les artefacts committes egalent un pipeline frais.

## Spec Change Log

## Design Notes

**Derivations (compilateur).** L = 3,10, a = 1,55, delta(v) = lerp(40, 16, clamp01(|v|/26)) deg, R(delta) = sqrt((L/tan delta)^2 + a^2). Plafond v*(kappa) = vitesse maximale telle que delta(v) >= delta requis ; « aucun » si delta requis <= 16 deg (R >= 10,93 m). Admission : v* >= 0,25 m/s <=> R >= 4,0344 m. Garde R <= a avant toute racine ou `atan`.

**Anneau.** Cercle aux moindres carres sur les noeuds V1 d'anneau du module (verifie : rayon 6,0 m, chaque noeud <= 0,10 m) ; corridors et continuations echantillonnes analytiquement sur ce cercle. Les 3 corridors d'anneau et leurs cles restent ; seules leurs bornes glissent le long du cercle (deplacement d'ancre publie).

**Entrees/sorties.** Construction a courbure continue (clothoide ou equivalent choisi et consigne) : depart sur l'axe d'approche, raccord tangent a l'anneau, empreinte dans la voie d'approche jusqu'a l'entree du point de reference dans l'enveloppe de l'anneau. Symetrique pour les sorties. Le choix de construction est consigne dans le Spec Change Log.

**Virages.** Frontiere = bord proche de l'union des enveloppes du corridor de depart et de son partenaire antiparallele, prolongee le long de leur axe. Virage : droit dans la voie (<= 0,72 m de l'axe) jusqu'a cette frontiere, puis construction a courbure continue, puis dans la voie de depart des la sortie de la chaussee d'approche. Virage a droite : depart anticipe permis, distance publiee.

**Reconfirmation (choix a valider au checkpoint -- la story ne dit pas comment amorcer les 76 decisions sans empreinte).** Le mode diff calcule, par paire, l'empreinte ancienne a partir du modele committe avant changement et l'empreinte fraiche ; il ecrit seulement le rapport. Menu proprietaire explicite « Reconfirmer les paires inchangees » : il ecrit l'empreinte fraiche uniquement sur les decisions dont les deux empreintes sont egales. Toute autre paire se reconfirme par edition manuelle du proprietaire. Le pipeline ne l'appelle jamais.

## Verification

**Commands (machine locale, Editeur connecte -- indisponible dans le conteneur cloud) :**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550DrivabilityTests"` -- vert.
- `.\scripts\validate.ps1 -TestMode EditMode` -- seul echec attendu `GateAIsOpenedOnlyByTheOwnersBoundSignoff`, 0 erreur Console.

**Manual checks:**
- Sous double garde : controle visuel des 9 carrefours et des segments dont la geometrie V2 a change ; vue d'isolement sur chaque paire soumise au proprietaire.
