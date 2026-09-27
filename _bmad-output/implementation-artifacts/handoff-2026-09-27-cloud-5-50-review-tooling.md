# Handoff Cloud -> local -- Story 5.50, outillage de revue des paires et couverture du balayage (2026-09-27)

- **Branche Cloud :** `cloud/5-50-review-tooling`, partie de `abf748b` (tete de `systeme-traffic-ia-v2` au 2026-09-27). `systeme-traffic-ia-v2` n'a pas ete modifiee.
- **Statut :** code et tests ecrits, **ni compiles ni executes**. Le conteneur Cloud n'a ni Unity, ni .NET, ni PowerShell, ni Graphify, et AD-13 interdit toute installation. Seule la geometrie des tests du balayage a ete rejouee en Python (voir section 3).
- **Rien n'a ete decide :** aucune decision d'authoring modifiee, aucune reconfirmation lancee, aucun artefact 5.28 regenere, aucune signature Gate A.
- **Le differentiel committe `review-5-50-diff.md` (68 / 66 / 8 / 0) est perime** : il vient de l'ancien balayage. Il reste provisoire et doit etre regenere localement apres validation (section 4). Aucune paire n'est a traiter avant.

## 1. Ce qui change

| Fichier | Nature |
|---|---|
| `Features/Vehicles/Traffic/Migration/ConflictSweep.cs` | Nouveau. Balayage P1 pur : distance exacte entre rectangles orientes, borne d'intervalle, trajectoires prolongees, suivi, echecs fermes. |
| `Features/Vehicles/Traffic/Migration/PairReview.cs` | Nouveau. Differentiel structure (`PairReview.Build` / `Render`), lecture isolee du modele format 1 (`HistoricalMovementReader`), actions du proprietaire (`PairReviewActions`). |
| `Features/Vehicles/Traffic/Migration/PairReviewWindow.cs` | Nouveau. Menu `RoadRage/Traffic V2/Revue des paires 5.50`. |
| `Migration/AuthoredRoadModel.cs` | `Candidates()` et le pipeline delegent a `ConflictSweep`. `AuthoredRun` garde `CandidateModel` et `PairSweeps` meme si le rapprochement echoue. `BuildReviewDiff` rend via `PairReview`. L'ancien critere AABB est supprime. |
| `Migration/AuthoringDecisions.cs` | Format 3 : empreinte **vide admise** (= non reconfirmee) ; si elle est presente, SHA-256 hexadecimal exige. |
| `Migration/GateAReviewWindow.cs` | `Git()` passe de `private` a `internal` (identite affichee dans la confirmation). |
| `Tests/EditMode/Story550ConflictSweepTests.cs` | Nouveau, 14 tests. |
| `Tests/EditMode/Story550PairReviewTests.cs` | Nouveau, 19 tests. |

Non touches : importeur, compilateur, validateur, `RoadModelDocument`, `PairGeometryFingerprint`, table historique, reference 5.50, V1, scenes, prefabs, fichiers de tests 5.25/5.26/5.27/5.28.

## 2. Audit du balayage present en `abf748b` (faits lus dans le code)

1. **La decision etait prise sur des boites englobantes.** Le critere comparait les AABB des rectangles orientes. C'est sur, car la distance AABB minore la distance exacte, mais c'est large des qu'un vehicule est en biais : pour deux rectangles a 45 deg, les AABB se recoupent alors que les rectangles sont a plus de 0,6 m.
2. **La trajectoire n'etait pas prolongee.** Seuls les echantillons propres du mouvement etaient balayes : aucune portee au-dela des extremites, aucune couture, aucun element court traverse. C'est **contraire a P1** : un contact au-dela d'une extremite pouvait etre manque (demontre par `AShortElementIsTraversedAndItsSuccessorReached`).
3. **Le cap etait l'angle 3D des tangentes** (`Vector3.Angle`), pas le cap plan de la preuve. Ce n'est sur que sur terrain plat.
4. **Une tangente sans partie horizontale n'etait pas traitee.** L'empreinte degenerait sans echec ferme.
5. **Le suivi n'etait pas publie.** Deux mouvements consecutifs sur une voie (par exemple entree de giratoire puis sortie via un anneau court) pouvaient devenir candidats. Le champ `ConflictCandidate.Following` existe mais n'etait pas utilise.
6. **Les decisions devenaient inutilisables apres la premiere action.** Au format 3, `Parse` **exigeait** une empreinte sur chaque decision. La premiere action du proprietaire reecrit tout le fichier en format 3 alors que 75 decisions n'en ont pas encore : le fichier devenait illisible.
7. **La revue etait impossible en pratique.** Tant que les 76 decisions ne sont pas reconfirmees, `Run` echoue avant `run.Compiled`, et la fenetre Gate A n'affiche que la liste des erreurs, sans geometrie.
8. **Le rapport etait illisible pour une decision.** Chaque paire n'y etait identifiee que par des cles `GlobalObjectId`, sans carrefour, sans libelle ni raison.

## 3. Balayage corrige -- ce qui est prouve, et ce qui reste une interpretation

**Regle, inchangee par rapport a la spec :** une paire est candidate si, pour un intervalle I de A, un intervalle J de B et deux poses d'extremite a et b, `dist(F_a, F_b) <= delta_I/2 + delta_J/2 + 2(marge + delta_c)`, avec `delta = |dp| + rho.|dtheta|` calcule dans le plan horizontal.

**Dans le perimetre approuve :**
- **Distance exacte.** La distance rectangle-rectangle est calculee exactement (SAT, puis sommets contre aretes). La spec admet « exacte ou sous-estimee ». Les AABB servent a preselectionner, et la marge qu'elles donnent est publiee.
- **Faux candidats publies.** Une paire retenue par les seules AABB et ecartee par la distance exacte est publiee (`EnvelopeOnly`, section « ecartees par la distance exacte ») et n'est pas candidate.
- **Portee.** Le point de reference est prolonge de `MaxVehicleLengthMeters / 2` au-dela de chaque extremite, sur les predecesseurs et les successeurs (corridors, connexions, mouvements d'autres carrefours) :
  - chaque coupe est une evaluation canonique `RoadCurve.Sample` ;
  - un element plus court que la portee restante est traverse ;
  - chaque embranchement produit une trajectoire ;
  - deux poses consecutives issues d'elements differents forment l'intervalle de couture.
- **Echecs fermes.** Ils rendent la paire candidate, avec une raison publiee, dans trois cas : tangente sans partie horizontale, rotation de 90 deg ou plus entre deux poses, plus de 64 trajectoires prolongees pour un mouvement.

**Interpretations a confirmer par le proprietaire :**
- **(I-1) Definition du suivi.** B suit A si le corridor de depart de B est atteint depuis le corridor d'arrivee de A **par des corridors seulement** (connexions, sans autre mouvement), la longueur cumulee avant lui ne depassant pas `MaxVehicleLengthMeters`. C'est ma lecture de « one movement directly follows another along a single-lane path ». Une telle paire est publiee comme suivi et n'est jamais candidate.
- **(I-2) Effet sur les comptes.** La distance exacte et le suivi publie retirent des candidats, alors que la portee en ajoute. *Estimation, non mesuree :* le nombre de paires « nouvelles » devrait baisser. Seule la regeneration locale le dira.

**Verification faite dans le Cloud (Python, pas C#).** J'ai porte la geometrie en Python et rejoue les cas des tests :
- translation, rotation, couture et element court : chaque cas echappe au controle aux seules poses et est trouve par le balayage ;
- sur 250 tirages aleatoires deterministes : 105 contacts reels, aucun manque ;
- la distance AABB ne depasse jamais la distance exacte, et la distance exacte ne surestime jamais (ecart maximal de 0,3 mm avec une recherche dense).

Ce n'est pas une execution du C#.

## 4. A faire en local (Editeur connecte, double garde d'etat)

1. **Recuperer la branche sans ecraser le travail local :**
   ```powershell
   git fetch origin cloud/5-50-review-tooling
   git switch cloud/5-50-review-tooling
   ```
   Ou fusionner dans `systeme-traffic-ia-v2`, a ta decision, apres relecture.
2. **Garde :** `git status --short` puis `unity cmd list_open_scenes`. Aucune scene `isDirty=true` inexpliquee.
3. **Compilation** (premiere chose qui peut echouer, car rien n'a ete compile) :
   ```powershell
   .\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550ConflictSweepTests"
   ```
   Attendu : compilation sans erreur, 14 tests verts :
   - `ATranslationCrossingBetweenTwoPosesTenMetresApartIsFound`, `ACornerTouchingOnlyDuringAnIntervalRotationIsFound`
   - `AContactOnlyWithinASeamIntervalIsFound`, `AShortElementIsTraversedAndItsSuccessorReached`
   - `EveryEndPoseCombinationIsExamined`, `APairRetainedOnlyByBoundingBoxesIsPublishedButNotACandidate`
   - `TheExactRectangleDistanceMatchesADenseBoundarySearch`
   - `ARotationOfNinetyDegreesOrMoreFailsClosed`, `ATangentWithoutHorizontalPartFailsClosed`, `AnOverBranchedReachFailsClosed`
   - `FollowingIsDetectedAlongCorridorsWithinOneVehicleLength`, `FollowingNeverCrossesAMovement`
   - `HeadAndTailCutsAreCanonicalEvaluations`, `TheSweepNeverMissesADenseContact`

   En cas d'erreur de compilation, la rapporter telle quelle ; rien n'est a considerer comme valide.
4. **Revue des paires :**
   ```powershell
   .\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550PairReviewTests"
   ```
   Attendu : 19 tests verts. Ils ouvrent `MVP_Run` en additif sans jamais le sauvegarder, et les actions du proprietaire n'y operent que sur des copies en memoire.
   - `TheHistoricalReaderReadsOnlyTheFormatOneBaseline` attend 72 mouvements dans le baseline, tous libelles et d'au moins deux echantillons : verifie dans le fichier committe.
5. **Non-regression 5.50 :**
   ```powershell
   .\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550CompatibilityGolden"
   .\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story550DrivabilityTests"
   ```
   Attendu : vert. Compilateur, writer et document ne sont pas touches.
6. **Suite EditMode complete :** `.\scripts\validate.ps1 -TestMode EditMode`. Rapporter la sortie brute.
   - **Rouges attendus, deja rouges en `abf748b` :** les tests 5.28 qui exigent `run.Succeeded` restent rouges tant que les decisions ne sont pas reconfirmees. Ce sont notamment `OnlyAcceptedCandidatesBecomeZonesAndEveryZoneComesFromACandidate`, `CandidatesComeFromTheVersionedProfileAndAreInvariantToResamplingAndPairOrder`, `AFreshProposalParsesAndCompilesWithTheSameCandidateSet`, `TheCommittedArtifactsEqualAFreshPipeline` et `GateAIsOpenedOnlyByTheOwnersBoundSignoff`.
   - **Comparer avec une execution sur `abf748b`** (ou `baseline-editmode.json`) : tout **nouveau** rouge est a me signaler.
7. **Regenerer le differentiel** avec le menu `RoadRage/Traffic V2/Generer le differentiel 5.50`. Il n'ecrit que `review-5-50-diff.md`. Controler :
   - l'en-tete : comptes modifiees / nouvelles / retirees / inchangees, ecartees par la distance exacte, suivi, echecs fermes, elements courts ;
   - que chaque paire porte **carrefour : mouvement A x mouvement B**, sa raison et son etat de decision ;
   - que `git diff --stat` ne touche que ce fichier.

   Le differentiel ne devient la base de ton HALT qu'apres cette verification.
8. **Fenetre, verification manuelle** (menu `RoadRage/Traffic V2/Revue des paires 5.50`, puis « Charger le differentiel frais ») :
   - **Chargement et en-tete.** La fenetre refuse si `MVP_Run` n'est pas ouvert ou s'il est modifie. Les comptes de l'en-tete sont egaux a ceux du rapport regenere.
   - **Filtres.** Les filtres par statut, carrefour et texte fonctionnent. Selectionner une paire la cadre dans la Scene view.
   - **Vue d'isolement :**
     - A en orange, B en bleu, avec depart vert, arrivee rouge et chevrons tous les 2 m ;
     - anciennes trajectoires en pointille ;
     - segments sous le rayon d'admission en rouge epais ;
     - volume ancien en gris, volume nouveau en rouge ;
     - empreintes temoins, enveloppes (bouton) et noeuds V1 sources en magenta.
   - **Detail.** Raison, empreintes, volumes, changements A / B / volume, relation fraiche, marges exacte et englobante, conduisibilite (rayon minimal et plafond).
   - **Actions :**
     - « Accepter » et « Rejeter » restent grises sans motif ;
     - « Reconfirmer cette paire » n'apparait que sur une decision existante non reconfirmee ;
     - « Disposer la decision orpheline » n'apparait que sur une paire retiree ;
     - le bouton « Reconfirmer les paires inchangees (N) » est grise si N = 0.
   - **Tester les dialogues avec « Annuler » seulement.** Toute confirmation ecrit `MVP_Run.road-authoring.json`.
9. **Apres fusion :** `graphify update .`, a faire en local puisque Graphify est absent du conteneur.

## 5. Decisions reservees au proprietaire (non prises)

- **(I-1)** Definition du suivi (section 3).
- **(D-1)** Le test 5.28 `CandidatesComeFromTheVersionedProfileAndAreInvariantToResamplingAndPairOrder` verifie encore le balayage **lateral** de la 5.28 (remplace par la 5.50) et **l'invariance au reechantillonnage**. Une borne conservatrice par intervalle n'est pas invariante au reechantillonnage : des echantillons plus denses resserrent la borne. Je ne l'ai pas modifie, car retirer une propriete approuvee ne peut se faire sans toi. Trois voies :
  - l'adapter : symetrie de l'ordre des paires et paire lointaine sans contact, sur `ConflictSweep` ;
  - le retirer, en consignant la raison ;
  - exiger un autre critere.
- **(D-2)** Fusionner ou non `cloud/5-50-review-tooling` dans `systeme-traffic-ia-v2`.
- Le traitement individuel des paires attend le differentiel regenere et la fenetre verifiee (points 7 et 8).
