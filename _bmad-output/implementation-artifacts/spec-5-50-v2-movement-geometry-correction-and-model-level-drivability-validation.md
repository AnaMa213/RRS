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

**Approach:** Normatif : `epics.md` Story 5.50 (lignes 2172-2338), plus les trois exigences du proprietaire du 2026-09-26 ci-dessous. Cette spec en est la projection d'execution et ne la resume pas.
1. Capturer localement les references avant tout code : hash dore non declare, puis empreintes historiques des 76 paires.
2. Reconstruire dans l'importeur l'anneau (cercle exact ajuste), les entrees, sorties et continuations tangentes, et les 24 virages selon la regle de frontiere de chaussee croisee.
3. Ajouter un `DrivabilityProfile` declare, valide au niveau modele (admission R >= 4,0344 m, C1-C5, F1-F3) et exige par `RoadModelDocument.Load` (format 2).
4. Balayer les candidats avec l'empreinte complete, avec une garantie conservatrice entre echantillons.
5. Empreinter chaque decision de conflit, publier le differentiel en mode diff lecture seule, puis HALT pour les decisions du proprietaire.

**Exigences du proprietaire (2026-09-26), absentes de la Story 5.50 d'`epics.md` :**
- (P1) la garantie conservatrice entre echantillons du balayage des candidats. Verifie : la Story 5.50 d'`epics.md` ne la contient pas ; seule la 5.51 porte une borne entre poses, pour le degagement vehicule-obstacle (`epics.md:2469-2470`) ;
- (P2) l'amorce des 76 decisions historiques ;
- (P3) le checkpoint de reference executable.

Report dans `epics.md` : a trancher par le proprietaire.

## Boundaries & Constraints

**Always:**
- **V1 et identites.** Source V1, hash source, lignee 5.27 et identites identiques octet pour octet ; aucun `LaneNode` deplace.
- **Anneau et 5.49.** Un seul corridor d'anneau ; geometrie physique et largeurs appliquees de la 5.49 conservees.
- **Trajectoire de reference.** Referentiel de conduite = reference compilee (evaluation canonique `RoadCurve`). Toute preuve de confinement est gonflee de delta_c = 0,05 m. Le temoin n'existe que dans le validateur.
- **Compatibilite canonique.** Payload canonique d'un modele non declare et fixtures 5.25/5.26 identiques a la reference doree capturee avant tout changement de production ; `CompilerSchemaVersion` inchange.
- **Derive des noeuds V1.** Mesuree par lignee, jamais contre la courbe la plus proche.
- **Balayage conservateur (P1).** Pour chaque paire, sur tout intervalle entre poses consecutives des deux empreintes mobiles, coutures et portee au-dela des extremites incluses, la paire est detectee des que les regions balayees reelles, gonflees de la marge du profil et de delta_c, se touchent. La garantie est demontree (Design Notes), jamais importee de la 5.51 sans preuve.
- **Decisions.** Le pipeline n'ecrit, ne transfere, ne supprime ni n'approuve aucune decision. Seules des actions explicites du proprietaire ecrivent une empreinte de reconfirmation. Aucune ne reconfirme une paire dont la geometrie d'un mouvement membre ou le volume de conflit a change.
- **Table historique (P2).** Elle est une donnee Editeur de revue, isolee de `RoadModelDocument.Load`, du compilateur, du validateur et du runtime.
- **Ordre d'execution.** Fichiers de tests 5.25/5.26/5.27 non modifies. Execution dans l'ordre des etapes ci-dessous (0, 1a, 1b, puis 1c, puis HALT).

**Ask First:**
- Toute exception a la regle de voie hors virages a droite.
- Repli sur une construction a sauts de courbure.
- Echec d'une regle de repli sur une entree ou une sortie (autre loi de largeur).
- Nouvelle evidence exigeant de toucher la geometrie physique 5.49.
- Tout test 5.25/5.26/5.27 qui echoue apres le changement.
- Hash dore qui ne correspond plus.
- Table historique incoherente avec le modele committe.
- Tout ecart entre la spec et la story normative autre que P1-P3.

**Never:**
- Deplacer un `LaneNode`.
- Relacher une porte, ou remodeler une geometrie acceptee, pour faire passer un test.
- Porte de conduisibilite propre au pipeline.
- Derive mesuree contre la courbe la plus proche.
- Reporter une decision sur une geometrie ou un volume modifies.
- Reconfirmation en lot d'une paire modifiee.
- Changement physique (5.51) ; changement V1 ; genre de controle giratoire (5.35) ; runtime ; signature Gate A ; second corridor d'anneau ou adjacence.
- Decision ecrite par l'agent.
- `InternalsVisibleTo`, ou code de production modifie, avant la capture de l'etape 0.

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
| Croisement entre poses | deux mouvements droits a pas de 10 m qui se croisent a mi-intervalle, aucune paire de poses echantillonnees en recouvrement | paire detectee ; un controle aux seules poses la manque (discrimination prouvee) | N/A |
| Rotation entre poses | coin d'une empreinte qui ne touche l'autre que pendant la rotation d'un intervalle | paire detectee | N/A |
| Couture balayee | contact uniquement dans l'intervalle franchissant une couture (saut de cap <= 5 deg) | paire detectee | N/A |
| Chaine courte | corridor < demi-vehicule entre deux mouvements | paire trouvee | N/A |
| Paire modifiee | empreinte historique != empreinte fraiche (geometrie OU volume) | proposition historique, Gate A fermee ; menu « paires inchangees » ne la touche pas | revue ciblee |
| Paire nouvelle / retiree | candidat sans decision / decision sans candidat | echec dur jusqu'a decision du proprietaire | aucune ecriture par le pipeline |
| Table historique alteree | SHA-256 ou version de schema d'empreinte different | aucune paire reconfirmable, cause nommee | echec ferme |

</frozen-after-approval>

## Code Map

Racine : `Assets/RoadRage/Features/Vehicles/Traffic/`.
- `RoadModelRecords.cs:643-678,713-735` -- `RoadModelValidationProfile` : gabarit 1,03 / 4,5 / 0,25 (:646,649,652), coutures (:658,661). `RoadModelSource` : y ajouter `DrivabilityProfile{Declared,...}`. Ne PAS l'ajouter au profil de validation, serialise en bloc par JsonUtility (`RoadModelDocument.cs:211,661`).
- `RoadModelCanonicalWriter.cs:14-30,58,88-105,110-285,377` -- `ComputeFingerprint` public : empreinte 128 bits = prefixe du SHA-256 des octets canoniques. `Write` est PRIVE. Bloc declare ajoute apres Portals (:274-284), ecrit SEULEMENT si `Declared`, sans octet de presence (contrairement a `HasStopLine` :222-227).
- `RoadModelCompiler.cs:34,44-106` -- `CompilerSchemaVersion = 4` inchange. Le payload est construit localement (:69), pas expose.
- `CompiledRoadModel.cs:412` -- `Version` public : seule valeur canonique capturable avant tout changement.
- `RoadModelDocument.cs:31,72-128,134-142,636-674` -- `Format` 1 -> 2. `Load` : format :89, re-serialisation :97, hash :102. Refus du non declare apres `FromDto` (:113).
- `RoadCurve.cs:114-120,132-195,231-247,280-307` -- reference compilee : positions lineaires, tangentes normalisees-interpolees. Bords implicites `Position -/+ Right*HalfWidth`.
- `RoadCurveBuilder.cs:88-118,180-266` -- Catmull-Rom centripete, corde <= 0,025 m. Les elements construits n'y passent plus.
- `RoadGeometryValidator.cs:21,40-159,299-324` -- `UnitTolerance` 1e-3 ; `SeamDefect` reutilise pour C3/F3. Nouvelle phase apres :159, gardee par `Declared`.
- `RoadModelValidator.cs:11-95,292-294,558-561,665-668` -- codes (dernier = 33, nouveaux >= 34) ; plafonds de couture 0,05 / 5.
- `Migration/V1RoadModelImporter.cs:224,227,239-251,551-642,644-711,730-732,745-797,803-917,1252-1270` :
  - `ImporterVersion` 1 -> 2 ; delta_c :227.
  - Anneau : Catmull-Rom par les noeuds, 3 corridors `Ring_Merge -> Ring_Split` par giratoire.
  - Mouvements : un Hermite de frontiere a frontiere (`AddMovement` :803-897), re-passe par le builder (:882).
  - `Boundary` : AABB des mouvements ; NE PAS l'utiliser comme chaussee croisee.
  - Pas de structure de bras : le partenaire antiparallele est l'autre corridor de la meme `ImportedSection`, a tangente opposee (voir `CountSameDirectionPairs` :1077-1094, `_mergedAtEnd/_mergedAtStart` :288-289).
- `Migration/V1SourceSet.cs:47,50,251,443-480` -- `V1Module.Root`/`Rotation` ; hash source (poses et roles des noeuds).
- `Migration/RoadLineage.cs:135-210` -- cles, genres et ids, sans geometrie.
- `Migration/MigrationReport.cs:112-137,176,325-365,1039-1086` -- derive 0,10 m par lignee ; `JustifiedException` :356-363 ; menu « Migrer MVP_Run ».
- `Migration/AuthoredRoadModel.cs:1,150,172-281,751-798,828-927,1071-1244,1513-1530,1585-1768,1826-1891` :
  - Editeur seulement (`#if UNITY_EDITOR`) ; `PipelineVersion` 2 ; `Run`.
  - `MatchConflicts` : echecs durs :768,788.
  - `PairKey` :795 ; `SweptRadius` = demi-largeur + marge par vehicule (:866-869).
  - `SweptOverlap` : balayage lateral seul, `ponytail:` :876-877.
  - `SameApproachPairs` : compte seulement.
  - Overlay, Gate A (:1762), menus, `TryWriteAll`.
- `Migration/AuthoringDecisions.cs:69-80,122,157-330,553` -- `ConflictDecision`, `FormatVersion` 2 -> 3, parse/serialise.
- `Migration/GateAReviewWindow.cs:33-39,52,122-242,265-404` -- Handles seulement ; filtre `ConflictFilter.Movement` = accroche de la vue d'isolement.
- `Features/Vehicles/VehicleProfileDef.cs:40-67`, `VehicleProfile.cs:131-146,400-406`, `VehicleSteeringModel.cs:39-64`, `ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset:20-64` :
  - braquage 40 -> 16 deg a 26 m/s, inactif si |v| < 0,25 ;
  - empattement et point de reference DERIVES des roues z +/-1,55 (pas de champ).
- `Tests/EditMode/Story525RoadWorldModelTests.cs:88`, `Story526GeometryAndLocalizationTests.cs:69` -- fixtures `BuildModel()` privees : lues par reflexion depuis l'assembly de tests, sans modifier ces fichiers.
- `Tests/EditMode/Story526GeometryAndLocalizationTests.cs:1458-1472` -- interdit `InternalsVisibleTo`.
- `Tests/EditMode/Story528AuthoringAndGateATests.cs:338,396-421,555-564,628-675` -- `{"Format":1,` en dur (:338) ; artefacts == pipeline frais ; harnais.
- `Tests/EditMode/Story527MigrationTests.cs:41-85,120-123,139-190,359-371` -- comptes, exceptions par libelles, rapport octet pour octet. Non modifiable.
- `App/Scenes/MVP_Run/MVP_Run.road-authoring.json` -- 76 decisions `Accepted`, sans empreinte.

## Tasks & Acceptance

**Execution:**

*Etape 0 -- reference, en local avec Unity, AVANT tout changement C# de production (P3) :*
- [ ] Garde. Arbre propre, noter `git rev-parse HEAD` ; double garde d'etat Editeur.
- [ ] `Tests/EditMode/Story550CompatibilityGolden.cs` (seul fichier C# autorise a ce stade) :
  - `GoldenUndeclaredSource()` : modele non declare deterministe (ModelId litteral ; echantillons litteraux couvrant section, corridor droit, corridor en arc, jonction, mouvement, portail ; aucun aleatoire, aucune horloge).
  - `[Explicit] CaptureGolden` : compile ce modele et les fixtures `BuildModel()` 5.25 et 5.26 (par reflexion), puis ecrit `CompiledRoadModel.Version` de chacun dans `_bmad-output/implementation-artifacts/v1-regression-5-50/golden-undeclared.txt`.
  - `UndeclaredModelsMatchTheGolden` : recompile et compare a ce fichier.
- [ ] Verifier que `git diff --stat` = ce fichier de test et son `.meta`. Lancer `validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550CompatibilityGolden"` : capture ecrite, test de comparaison vert.
- [ ] `v1-regression-5-50/baseline-hashes.txt` : SHA-256 de la lignee, `source-hash`, rapport 5.27, et `MVP_Run.road-model.json`, overlay, rapport 5.28 et authoring committes.
- [ ] Copie octet pour octet de `MVP_Run.road-model.json` (format 1) : `baseline-road-model-format1.json`.
- [ ] `baseline-editmode.json`.
- [ ] Commit « reference 5.50 ».

*Etape 1a -- acces aux octets canoniques, sans changement de comportement :*
- [ ] `RoadModelCanonicalWriter.cs` -- accesseur public en lecture seule des octets canoniques d'un modele compile.
- [ ] `CaptureGolden` complete : ecrit `golden-undeclared.bin` pour chaque modele.
- [ ] Le test exige que le prefixe du SHA-256 de ces octets egale la `Version` capturee a l'etape 0 ; sinon HALT.
- [ ] Suite EditMode inchangee. Commit.

*Etape 1b -- empreintes historiques, AVANT tout changement d'importeur, de format ou de balayage (P2) :*
- [ ] `Migration/PairGeometryFingerprint.cs` -- fonction pure, versionnee (`FingerprintSchemaVersion` 1). SHA-256 de :
  - la version de schema ;
  - les cles de paire ;
  - les octets canoniques des echantillons des deux mouvements ;
  - le volume de zone quantifie.
- [ ] Menu Editeur « Capturer les empreintes historiques 5.50 ». Il execute le pipeline ACTUEL inchange sur `MVP_Run` et refuse si la `Version` fraiche differe du `ModelVersion` de `baseline-road-model-format1.json`. Il ecrit `v1-regression-5-50/historical-pair-fingerprints.json`, avec pour chaque decision :
  - les cles et la `Decision` ;
  - le hash de chaque mouvement ;
  - le volume ;
  - l'empreinte.
  
  Il ecrit aussi la version de schema et le SHA-256 du modele source ; le fichier porte son propre SHA-256 dans `baseline-hashes.txt`.
- [ ] Test : la table couvre les 76 decisions exactement, et aucune source hors `Migration/` ne la reference.
- [ ] Commit.

*Etape 1c -- implementation :*
- [ ] `RoadModelRecords.cs`, `RoadModelCompiler.cs`, `RoadModelCanonicalWriter.cs` -- `DrivabilityProfile` et derivations ; bloc canonique ecrit seulement si declare.
- [ ] `RoadGeometryValidator.cs`, `RoadModelValidator.cs` -- phase de conduisibilite :
  - admission, C1-C5, F1-F3 ;
  - codes >= 34 ;
  - message nommant l'element, l'echantillon et la regle.
- [ ] `RoadModelDocument.cs` -- format 2 ; refus du non declare ; refus croise format 1 et format 2.
- [ ] `Migration/V1RoadModelImporter.cs` :
  - `ImporterVersion` 2 ;
  - profil declare copie de `VehicleProfileDef_Default` ;
  - anneau, entrees, sorties, continuations et virages (Design Notes) ;
  - echantillonnage analytique (corde <= delta_c/2) ;
  - frontieres de chaussee derivees des enveloppes de corridors.
- [ ] `Migration/MigrationReport.cs` -- derive par lignee etendue aux connecteurs d'extremite et aux ancres. Tables publiees par element :
  - rayon minimal ;
  - plafond de vitesse et sa distribution ;
  - cap net et cumule ;
  - sauts de couture ;
  - ecart entre courbure et cercle a trois points ;
  - deplacement d'ancre ;
  - distance d'exception des virages a droite.
- [ ] `Migration/AuthoredRoadModel.cs` :
  - balayage conservateur (Design Notes) ;
  - paires de suivi publiees ;
  - elements < `MaxVehicleLengthMeters` listes ;
  - empreinte fraiche par paire (meme fonction que 1b) ;
  - `MatchConflicts` : une empreinte absente ou differente fait une proposition historique, et la Gate A reste fermee ;
  - menu mode diff (ecrit seulement `review-5-50-diff.md`, qui confronte la table historique aux empreintes fraiches) ;
  - `PipelineVersion` 3.
- [ ] `Migration/AuthoringDecisions.cs` :
  - `FormatVersion` 3, champ `GeometryFingerprint` ;
  - le format 2 est lu (toute decision y est non reconfirmee) mais jamais ecrit ;
  - seules les actions du proprietaire (Design Notes) emettent le format 3.
- [ ] `Migration/GateAReviewWindow.cs` :
  - vue d'isolement d'un mouvement : marqueurs, chevrons, rayon minimal, plafond, rouge hors admission, enveloppe optionnelle, noeuds V1 ;
  - revue ciblee des paires : ancien et nouveau volume superposes, action « Reconfirmer cette paire » ;
  - texte d'overlay inchange.
- [ ] `Tests/EditMode/Story550DrivabilityTests.cs` -- toute la section « EditMode verification » de la story, et la matrice (lignes P1 comprises), avec un controle aux seules poses qui manque les trois cas P1. Tests supplementaires :
  - profil == `VehicleProfileDef_Default` ;
  - le code V1 ne reference pas Traffic V2 ;
  - menu « paires inchangees » : il ne touche jamais une paire a geometrie ou volume modifies, ni une paire absente de la table ; il echoue ferme si la table est alteree.
- [ ] `Tests/EditMode/Story528AuthoringAndGateATests.cs` -- `{"Format":1,` -> format 2 ; une decision sur paire modifiee bloque la Gate A.
- [ ] Mode diff sur `MVP_Run` -> `review-5-50-diff.md`. **HALT : decisions du proprietaire** (paires nouvelles, modifiees, retirees), chacune visible dans la vue d'isolement.
- [ ] Apres resolution complete :
  - « Migrer MVP_Run » (rapport 5.27, `ImporterVersion` 2) ;
  - « Compiler le modele authore » (artefacts 5.28) ;
  - suite EditMode complete ;
  - `sprint-status.yaml` ;
  - note sous le HALT de `spec-5-28` ;
  - `graphify update .`.

**Acceptance Criteria:**
- Given l'importeur corrige et la source V1 inchangee, when `MVP_Run` est importe, then :
  - hash source et lignee sont identiques octet pour octet ;
  - aucune identite n'est frappee ni retiree ;
  - chaque noeud V1 est a 0,10 m au plus de ses elements associes par lignee (seule exception : noeud de decision lisse par un virage).
- Given `MVP_Run` corrige, when il est compile, then :
  - tous ses elements sont admis ;
  - les noeuds d'anneau sont a 0,10 m du cercle ajuste ;
  - entrees et sorties rejoignent l'anneau tangentiellement ;
  - chaque virage respecte la regle de voie contre une frontiere identique avant et apres la correction.
- Given les modeles non declares de l'etape 0, when ils sont compiles apres le changement, then leurs octets canoniques egalent `golden-undeclared.bin` et leur `Version` egale la capture pre-changement.
- Given une decision dont la geometrie de paire ou le volume a change, when la Gate A est evaluee, then elle est montree comme proposition historique et la Gate A reste fermee jusqu'a reconfirmation ou changement par le proprietaire.
- Given le differentiel exhaustif, when il contient des paires nouvelles, retirees ou modifiees, then le travail s'arrete et aucun artefact 5.28 definitif n'est regenere avant resolution complete.
- Given les artefacts regeneres, when la suite EditMode complete tourne, then :
  - les tests 5.25/5.26/5.27 passent sans modification ;
  - seul `GateAIsOpenedOnlyByTheOwnersBoundSignoff` echoue ;
  - les artefacts committes egalent un pipeline frais.

## Spec Change Log

## Design Notes

**Derivations.**
- Constantes : L = 3,10, a = 1,55.
- Braquage disponible : delta(v) = lerp(40, 16, clamp01(|v|/26)) deg.
- Rayon : R(delta) = sqrt((L/tan delta)^2 + a^2).
- Plafond v*(kappa) : « aucun » si delta requis <= 16 deg (R >= 10,93 m).
- Admission : v* >= 0,25 m/s, soit R >= 4,0344 m.
- Garde R <= a avant toute racine.

**Anneau, entrees et sorties, virages.**
- *Anneau.* Cercle aux moindres carres sur les noeuds V1 d'anneau (6,0 m, chaque noeud <= 0,10 m). Les 3 corridors d'anneau et leurs cles restent ; leurs bornes glissent le long du cercle (deplacement publie).
- *Entrees et sorties.* Construction a courbure continue, choisie et consignee dans le Spec Change Log : depart sur l'axe d'approche, raccord tangent, empreinte dans la voie d'approche jusqu'a l'entree dans l'enveloppe de l'anneau. Symetrique pour les sorties.
- *Virages.* La frontiere est le bord proche de l'union des enveloppes du corridor de depart et de son partenaire antiparallele. Le virage reste droit dans la voie (<= 0,72 m de l'axe) jusqu'a elle, puis suit une construction a courbure continue, puis reste dans la voie de depart. Le depart anticipe n'est permis qu'aux virages a droite, avec sa distance publiee.

**Balayage conservateur (P1) -- preuve.**
*Hypotheses.* Plan horizontal (road-up constant, C4). Empreinte rectangulaire centree sur le point de reference ; rho = sqrt((L_max/2)^2 + W^2), avec L_max = `MaxVehicleLengthMeters` et W = `MaxVehicleHalfWidthMeters`. Sur l'intervalle [i, i+1] d'une reference compilee :
- la position est lineaire : |p(s) - p_i| + |p(s) - p_{i+1}| = |dp| ;
- le cap de la tangente normalisee-interpolee est monotone entre theta_i et theta_{i+1} tant que |dtheta| < 90 deg (sinon echec ferme de l'intervalle) : |theta(s) - theta_i| + |theta(s) - theta_{i+1}| = |dtheta|.

*Lemme.* Pour un point du corps b (|b| <= rho), |R(t)b - R(t')b| = 2|b|sin(|t-t'|/2) <= rho|t-t'|. La distance au pose i est donc bornee par D_i(s) = |p(s) - p_i| + rho|theta(s) - theta_i|. Comme D_i + D_{i+1} = |dp| + rho|dtheta| = delta, le min(D_i, D_{i+1}) <= delta/2. L'empreinte en s est donc incluse dans F_i ⊕ B(delta/2) ou dans F_{i+1} ⊕ B(delta/2).

*Regle.* Une paire est candidate s'il existe un intervalle I de A et un intervalle J de B tels que, pour une paire d'extremites (a, b) :
dist(F_a, F_b) <= delta_I/2 + delta_J/2 + 2(m + delta_c)
- m est la marge du profil, appliquee par vehicule comme aujourd'hui (:866-869).
- La distance rectangle-rectangle est exacte (polygones convexes) ou sous-estimee, jamais surestimee.
- Les intervalles couvrent la chaine dirigee connectee sur toute la portee (`MaxVehicleLengthMeters`/2 au-dela des extremites, elements courts traverses).
- Une couture compte comme un intervalle (dp = ecart, dtheta = saut de cap) : elle couvre toute transition lineaire en position et monotone en cap entre les deux poses d'extremite.
- Volume de zone = AABB des F ⊕ B(delta/2 + m + delta_c) des intervalles contributeurs.

La borne coincide avec celle de la 5.51, mais elle est demontree ici pour deux empreintes mobiles, pas importee.

**Amorce des 76 decisions (P2).**
- *Pourquoi 1b.* Apres l'etape 1c, un document format 1 est refuse par `Load`. L'ancienne geometrie ne reste donc accessible que par la table de 1b, calculee sur le modele committe avant tout changement.
- *Comment 1b est prouvee.* Par le refus en cas d'ecart de `ModelVersion` et par le SHA-256 du modele source.
- *Isolation.* La table n'est lue que par le mode diff et les actions du proprietaire, toutes Editeur (`#if UNITY_EDITOR`). Elle ne produit jamais de `CompiledRoadModel`.
- *Menu « Reconfirmer les paires inchangees ».* Explicite, proprietaire seul, jamais appele par le pipeline. Il ecrit l'empreinte fraiche uniquement quand empreinte historique == empreinte fraiche. Toute autre paire passe par « Reconfirmer cette paire », apres affichage des geometries et volumes ancien et nouveau, ou par edition manuelle.
- *Estimation, non mesuree.* Le balayage P1 change le volume de toutes les paires, qui ajoute la longueur et les intervalles. Ce menu reconfirmera donc probablement **aucune** des 76 paires, et chaque paire passera par une decision individuelle du proprietaire.

## Verification

**Commands (machine locale, Editeur connecte -- indisponible dans le conteneur cloud) :**
- Etape 0 : `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550CompatibilityGolden"` -- capture ecrite, comparaison verte, `git diff --stat` limite au fichier de test.
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550DrivabilityTests"` -- vert.
- `.\scripts\validate.ps1 -TestMode EditMode` -- seul echec attendu `GateAIsOpenedOnlyByTheOwnersBoundSignoff`, 0 erreur Console.

**Manual checks:**
- Sous double garde :
  - controle visuel des 9 carrefours et des segments dont la geometrie V2 a change ;
  - vue d'isolement sur chaque paire soumise au proprietaire.
