---
title: 'Story 5.50 -- Correction de la geometrie des mouvements V2 et validation de conduisibilite au niveau modele'
type: 'feature'
created: '2026-09-26'
status: 'in-progress'
baseline_commit: '780de0d64e694a3a3f6b0aa8fdf32f9776294728'
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
5. Empreinter chaque decision de conflit, publier le differentiel en mode diff lecture seule, puis appliquer la politique deterministe deleguee `5.50-AUTO-DECISIONS-v1` approuvee globalement par le proprietaire le 2026-09-27.

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
- **Decisions.** Hors delegation approuvee, le pipeline n'ecrit, ne transfere, ne supprime ni n'approuve aucune decision. `5.50-AUTO-DECISIONS-v1` autorise un run complet sur entrees epinglees : rejet seulement avec certificat exhaustif de separation, toute incertitude acceptee comme conflit conservateur, revision fraiche obligatoire sur geometrie ou volume change, manifeste d'audit complet et ecriture transactionnelle. Aucune decision n'est transferee silencieusement.
- **Table historique (P2).** Elle est une donnee Editeur de revue, isolee de `RoadModelDocument.Load`, du compilateur, du validateur et du runtime.
- **Ordre d'execution.** Fichiers de tests 5.25/5.26/5.27 non modifies. Execution dans l'ordre des etapes ci-dessous (0, 1a, 1b, 1c, plan automatise, validation, application atomique, regeneration). Aucun HALT paire par paire.

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
- Decision ecrite hors de la fonction deterministe et de la delegation `5.50-AUTO-DECISIONS-v1`.
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
| Paire modifiee | empreinte historique != empreinte fraiche (geometrie OU volume) | nouvelle revision automatisee, liee a l'ancienne et a l'empreinte fraiche ; Gate A fermee jusqu'a application validee | jamais de transfert silencieux |
| Paire nouvelle / retiree | candidat sans decision / decision sans candidat | decision automatisee / tombstone, apres preuve et validation exhaustives | aucune ecriture partielle |
| Faux candidat | toutes les combinaisons chemin/intervalle ont un certificat de separation strictement positif | `Rejected`, preuve et marge publiees | sans certificat : conflit conservateur accepte |
| Paire incertaine | preuve incomplete, limite numerique, cap degenere, rotation hors hypothese ou budget epuise | `Accepted` avec classification `ConservativeConflict` | aucun HALT paire par paire |
| Run non deterministe | deux plans sur les memes entrees different | aucune decision ecrite | corriger et relancer ; HALT seulement si defaut systemique non resolu |
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
  - `FormatVersion` 4, champs `GeometryFingerprint`, `DecisionRevisionId`, `EvidenceHash`, `DecisionRunId`, classification et versions exactes ;
  - les formats 2 et 3 sont lus comme decisions historiques non confirmees mais jamais ecrits ;
  - seul le plan automatise valide ou une action explicite du proprietaire emet le format 4.
- [ ] `Migration/GateAReviewWindow.cs` :
  - vue d'isolement d'un mouvement : marqueurs, chevrons, rayon minimal, plafond, rouge hors admission, enveloppe optionnelle, noeuds V1 ;
  - revue ciblee des paires : ancien et nouveau volume superposes, action « Reconfirmer cette paire » ;
  - texte d'overlay inchange.
- [ ] `Tests/EditMode/Story550DrivabilityTests.cs` -- toute la section « EditMode verification » de la story, et la matrice (lignes P1 comprises), avec un controle aux seules poses qui manque les trois cas P1. Tests supplementaires :
  - profil == `VehicleProfileDef_Default` ;
  - le code V1 ne reference pas Traffic V2 ;
  - menu « paires inchangees » : il ne touche jamais une paire a geometrie ou volume modifies, ni une paire absente de la table ; il echoue ferme si la table est alteree.
- [ ] `Migration/AutomatedPairDecisionPolicy.cs` et manifeste `v1-regression-5-50/automated-pair-decisions.json` :
  - classification ordonnee `Following`, `ConflictProven`, `ProvenDisjoint`, `ConservativeConflict` ;
  - certificat continu de contact ou de separation, preuves et versions canoniques par paire ;
  - nouvelle revision sur toute empreinte changee, tombstone pour toute decision retiree ;
  - double plan byte-identique, application atomique, rerun sans changement.
- [ ] Tests adversariaux : contact/tangence/separation limite, croisement entre poses, rotation, couture, chaine courte, faux positif AABB, branchement ambigu, cap degenere, rotation >= 90 deg, budget epuise, permutation d'entrees, falsification des versions/preuves/empreintes.
- [ ] Harnais de circulation Editor-only sur les 9 carrefours : mouvement seul, chaque paire acceptee, tous mouvements, saturation canonique/inversee/graines fixes et suivis ; exclusion mutuelle, ensemble compatible maximal, progres non vide, attente <= nombre de mouvements du carrefour, aucun blocage par suivi ou paire prouvee disjointe.
- [ ] `Tests/EditMode/Story528AuthoringAndGateATests.cs` -- `{"Format":1,` -> format 2 ; une decision sur paire modifiee bloque la Gate A.
- [ ] Mode diff sur `MVP_Run` -> `review-5-50-diff.md`, puis plan `5.50-AUTO-DECISIONS-v1`, validation et application transactionnelle sans HALT paire par paire. HALT uniquement pour changement de contrat, defaut systemique non resolu ou absence de politique sure et bornee.
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
- Given une decision dont la geometrie de paire ou le volume a change, when la Gate A est evaluee, then elle est montree comme proposition historique et la Gate A reste fermee jusqu'a une nouvelle revision valide. Cette revision peut etre produite par `5.50-AUTO-DECISIONS-v1` apres l'approbation globale, avec preuve et versions exactes ; Gate A reste distincte et non signee.
- Given le differentiel exhaustif, when il contient des paires nouvelles, retirees ou modifiees, then la fonction deleguee resout le lot complet sans HALT paire par paire, rejette uniquement sur preuve exhaustive de separation et accepte conservativement toute incertitude ; aucun artefact 5.28 definitif n'est regenere avant validation et application completes du plan.
- Given les artefacts regeneres, when la suite EditMode complete tourne, then :
  - les tests 5.25/5.26/5.27 passent sans modification ;
  - seul `GateAIsOpenedOnlyByTheOwnersBoundSignoff` echoue ;
  - les artefacts committes egalent un pipeline frais.

## Spec Change Log

- **2026-09-27 -- `5.50-AUTO-DECISIONS-v1` approuve globalement par le proprietaire.** Le HALT paire par paire et l'interdiction absolue d'ecriture par l'agent sont remplaces, pour cette story seulement, par une fonction deterministe versionnee, un manifeste d'audit, des preuves par paire, une politique conservatrice et une application transactionnelle. Les 120 paires du differentiel courant seront traitees sans validation individuelle. Gate A reste distincte et ne sera pas signee automatiquement. Proposition approuvee : `planning-artifacts/sprint-change-proposal-2026-09-27.md`, SHA-256 pre-approbation `D12DE07EB0891B47D24083CD41629E53825D63FBDB43035D3578C3E9B334D2BB`.

- **2026-09-26 -- report de P1-P3 dans la story normative (sprint-change-proposal-2026-09-26.md, approuve par le proprietaire).** Les exigences P1 (balayage conservateur entre echantillons), P2 (amorce des 76 decisions historiques) et P3 (checkpoint de reference executable) figurent desormais dans la Story 5.50 d'`epics.md` (Conflict candidates, Decision reconfirmation, Execution sequence 0/1a/1b/1c, EditMode verification, Must NOT be copied, AC du balayage). La mention « absentes de la Story 5.50 d'`epics.md` » du bloc gele est donc historique ; le bloc gele n'est pas modifie. Aucun changement d'exigence.

- **2026-09-27 -- tranche Cloud `cloud/5-50-review-tooling` (non compilee ; handoff : `handoff-2026-09-27-cloud-5-50-review-tooling.md`).** Constat : le balayage de `fd11e0c` decidait sur les boites englobantes, ne prolongeait pas les trajectoires (ni portee, ni couture, ni element court : contraire a P1), mesurait le cap en 3D, ne publiait pas le suivi ; le format 3 exigeait une empreinte sur chaque decision, ce qui rendait le fichier illisible apres la premiere action du proprietaire ; la fenetre Gate A ne permettait aucune revue tant que le rapprochement echoue. Amende dans le perimetre approuve : distance exacte des rectangles orientes pour decider (les boites englobantes preselectionnent et leurs faux candidats sont publies), portee `MaxVehicleLengthMeters`/2 sur predecesseurs et successeurs par evaluation canonique, coutures en intervalles, cap plan, echec ferme sur cap degenere, rotation >= 90 deg ou portee de plus de 64 trajectoires ; empreinte vide admise au format 3 (non reconfirmee) ; revue structuree et fenetre `Revue des paires 5.50`. Interpretation a confirmer par le proprietaire : suivi = corridor de depart atteint par des corridors seuls sur au plus `MaxVehicleLengthMeters`. KEEP : aucune decision ecrite par le pipeline ; le differentiel reste provisoire jusqu'a sa regeneration locale verifiee.

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
- *Actions manuelles historiques.* Les menus de reconfirmation restent disponibles comme repli, mais le run approuve `5.50-AUTO-DECISIONS-v1` ne les appelle pas et n'exige aucune decision individuelle du proprietaire.
- *Decision automatisee.* Le run epingle toutes ses entrees, classe dans l'ordre ordinal des `PairKey`, rejette seulement sur certificat exhaustif de separation et accepte toute preuve incomplete comme `ConservativeConflict`. Chaque sortie porte une revision liee a l'empreinte fraiche et a la revision qu'elle remplace. Les suivis, faux candidats, revisions remplacees et tombstones restent dans le manifeste Editor-only.
- *Situation mesuree au moment de l'amendement.* Le differentiel contient 120 paires actives a disposer (76 modifiees, 44 nouvelles), 84 suivis ordinaires, 20 faux candidats AABB prouves par la distance exacte, 0 paire retiree et 0 echec ferme. Ces comptes sont controles mais jamais codes en dur.

## Verification

**Commands (machine locale, Editeur connecte -- indisponible dans le conteneur cloud) :**
- Etape 0 : `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550CompatibilityGolden"` -- capture ecrite, comparaison verte, `git diff --stat` limite au fichier de test.
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550DrivabilityTests"` -- vert.
- `.\scripts\validate.ps1 -TestMode EditMode` -- seul echec attendu `GateAIsOpenedOnlyByTheOwnersBoundSignoff`, 0 erreur Console.

**Manual checks:**
- Sous double garde :
  - controle visuel des 9 carrefours et des segments dont la geometrie V2 a change ;
  - vue d'isolement sur chaque paire soumise au proprietaire.
