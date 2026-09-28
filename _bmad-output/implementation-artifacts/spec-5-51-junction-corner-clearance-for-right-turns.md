---
title: "Story 5.51 - Degagement physique des angles pour virages a droite"
type: "feature"
created: "2026-09-28"
status: "done"
baseline_commit: "58c230a311ef61f81611ca640dd28b4c5bac28a9"
review_loop_iteration: 0
context:
  - "{project-root}/_bmad-output/planning-artifacts/epics.md"
  - "{project-root}/_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-28.md"
---

<frozen-after-approval reason="intention du proprietaire - renegociation requise pour modifier">

## Intent

**Probleme :** Les virages a droite definitifs de la 5.50 coupent les trottoirs carres de `MVP_Run`. La Gate A exige un degagement physique demontre.

**Approche :** Couper les 12 angles par overrides ; prouver deux degagements (obstacles en volume, `Sidewalk` en plan) sur tous les mouvements ; comparer V1 avant/apres. L'ajout semantique, absent d'`epics.md`, attend l'approbation du correct-course du 2026-09-28.

## Boundaries & Constraints

**Always :** Courbes 5.50, `RoadModelVersion`, candidats, donnees/topologie V1 et lignee 5.27 inchanges. Preuves/empreintes separees : volume des obstacles dans la tranche verticale et `Sidewalk` en plan sans condition de hauteur ni d'activation du collider, y compris `TJunction_North`. Declaration et trottoir visible concordants ou echec. Gabarit/marge du profil versionne ; delta_c = 0,05 m. Baseline V1 avant edition ; EditMode complet et trafic PlayMode avant/apres dans deux sessions fraiches ; deltas soumis au proprietaire. Double garde Git/scenes.

**Ask First :** Modifier un prefab/asset partage/Synty, changer la construction ou disposer un delta V1. Premier angle : forme, semantique, visuel, sauvegarde/recharge et diff doivent tous passer avant les onze autres ; sinon arret. Aucune coupe viable : decision proprietaire.

**Never :** Deplacer un `LaneNode`, modifier V2 ou les giratoires, reduire la marge, simuler un chanfrein par simple redimensionnement de `BoxCollider`, modifier/deriver un mesh Synty, signer Gate A ou assimiler hash V1 stable a comportement stable.

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Scenes/MVP_Run.unity:2039` - overrides `TJunction_North` desactivent deux colliders de coin, pas les visuels ; `MVP_Run/NavMesh-LaneGraph-Vehicle.asset` - rebake.
- `Assets/RoadRage/Prefabs/Greybox_Intersection.prefab:1600`, `Greybox_TJunction.prefab:169` - coins/curbs et visuels ; `Greybox_AIVehicle.prefab:65` - sommet a 1,44 m. Prefabs en lecture seule.
- `ProjectSettings/NavMeshAreas.asset:10` - `Sidewalk` = aire 3 ; les 12 coins declares et leurs visuels ont des bornes XZ egales dans l'Editor (2026-09-28), dont deux colliders desactives au T Nord.
- `Assets/RoadRage/Features/Vehicles/Traffic/Migration/ConflictSweep.cs:435,818` - trajets, coutures, delta ; portee a etendre. `RoadCurve.cs:280` - poses canoniques ; `RoundaboutClearance.cs:314` - projection convexe reutilisable, filtre par nom impropre ; `AuthoredRoadModel.cs:74` - rapport.
- `Assets/RoadRage/Features/Vehicles/SidewalkDeclarations.cs` - adaptateur Editor hors `Traffic/` (AD-33, garde `Story549...TrafficV2SourcesNeverReferenceTheNavMesh`) : seul lecteur des `NavMeshModifier` et de l'aire `Sidewalk` ; livre des `JunctionClearanceSurface` neutres. `JunctionClearance.cs` ne reference jamais le NavMesh.
- `MVP_Run.unity` - `TJunction_South/Visual_Greybox_TJunction/Chamfer_RoadFill_SE` : remplissage visuel sans collider du triangle coupe (A2), mesh projet `App/Scenes/MVP_Run/ChamferRoadFill_TJunction_South_SE.asset`, materiau Synty `Road_01` reference tel quel, UV d'une tuile d'asphalte virtuelle alignee sur `Road_n2_s2`.
- `Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs` - regle du relief routier franchissable (cas purs, borne derivee du prefab IA, classement reel dans `MVP_Run`) et premier angle.

## Tasks & Acceptance

**Execution :**

- [x] `_bmad-output/implementation-artifacts/v1-regression-5-51/` - avant edition : commit, hashes source/lignee, version/candidats, EditMode complet et PlayMode trafic.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` - essayer un angle : union exacte de trois boites et visuel gris par overrides ; sauver/recharger et controler ensemble forme physique, region `Sidewalk`, surface visible et diff de scene. Arret sur tout echec avant les onze autres.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs` - references prolongees de longueur/2 + marge + delta_c, poses canoniques (echantillons/coutures), empreinte gonflee. Physique : colliders non-trigger sans Rigidbody dynamique entre route et sommet IA, sauf relief routier franchissable (addendum 2026-09-28). Semantique : surfaces `Sidewalk` (aire par nom) meme a collider desactive, appariees aux visuels. Deux residus continus `min(d_a,d_b) - (|Delta p| + rho*Delta theta)/2` strictement positifs ; publier par coin/mouvement minimum, position, surface/obstacle, couture, h et deltas. Empreintes distinctes des entrees physiques et semantiques (aire, forme/activation, mesh et transform visuels), invalidation sur changement pertinent.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` et `MVP_Run/NavMesh-LaneGraph-Vehicle.asset` - dimensionner les 12 coupes minimales, ajuster curbs et visuels, rebaker NavMesh ; prefabs et Synty intacts.
- [x] `Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs` - coin carre negatif puis coupe positive sur les deux gates ; trottoir plat, coins desactives du T Nord, route non `Sidewalk`, contact entre poses/coutures, tangentes degenerees, declaration sans visuel, invalidation des deux empreintes ; adversariaux, hauteur IA, invariants V1/V2, residus partout.
- [x] `_bmad-output/implementation-artifacts/v1-regression-5-51/` - apres edition dans une autre session Editor : memes suites, comparaison par test/trace ; documenter faisabilite, overrides, empreinte et residus. `graphify update .` execute.
- [ ] Revue de code independante, acceptation visuelle des cinq jonctions et decision proprietaire sur la faible variation numerique 5.12 dans un test PlayMode preexistant ; Gate A reste fermee.

**Acceptance Criteria :**

- Given le premier angle modifie, when la scene est sauvee/rechargee, then forme physique, region `Sidewalk`, visuel et diff de scene concordent ; sinon aucun autre angle n'est deploye.
- Given les cinq jonctions, when tous les mouvements et raccords sont balayes, then les residus physique et semantique sont chacun strictement positifs et publies avec leurs empreintes separees, y compris pour `TJunction_North`.
- Given la coupe, when V1 et V2 sont recalcules, then source, lignee, identites, version et candidats restent identiques ; chaque delta comportemental V1 est soumis au proprietaire.

## Spec Change Log

- 2026-09-28 (revue de la vague 1) : Kenan a valide les huit angles en T et demande une preuve complete du critere (b) du relief. La grille de 10 cm est remplacee par une inclusion geometrique exacte (soustraction convexe de l'union des supports, joint de 1 mm) ; seuil de hauteur et conditions (a)/(c) inchanges ; version du mesureur 3. `epics.md` et le correct-course alignes. Mini-gate autorise sur un seul angle du carrefour central.
- 2026-09-28 (addendum proprietaire) : A1 rejete au rendu (teinte `Greybox_Road_Mat`), A2 valide et applique. Relief routier franchissable = surface roulable pour le gate physique, selon le critere explicite (a) sans chevauchement `Sidewalk`, (b) repose sur la chaussee, (c) hauteur <= garde au sol statique du vehicule IA (0,158 m) ; boites de toute orientation projetees par l'enveloppe de leurs 8 sommets. Amendements appliques a `epics.md` (5.51), au correct-course (section 6), au spine et au contrat Road World Model ; version du mesureur 2. L'intention gelee est inchangee.
- 2026-09-28 (reprise apres HALT du premier angle) : Kenan a tranche sans changer l'intention. A1 : combler le triangle coupe avec une surface `Greybox_Road_Mat` sans collider ; A2 (mesh triangulaire propre au projet) si A1 n'est pas coherent ; A3 refuse. B1 : AD-33 conserve, la lecture `NavMeshModifier`/`Sidewalk` passe dans un adaptateur Editor hors `Features/Vehicles/Traffic/` (Code Map mis a jour). C : coupe de 1,00 m conservee au premier angle. Appariement visuel remplace par une concordance de surface visible conservative ; les rampes hors forme supportee sont mesurees, jamais ignorees.
- 2026-09-28 : Kenan a approuve la spec et le correct-course semantique. L'amendement figure desormais dans `epics.md`, le contrat Road World Model et l'architecture ; la mention d'attente dans le bloc gele reste l'historique de la revue.
- 2026-09-28 (reprise et cloture de revue) : les constats de la revue independante sont corriges et completes par l'agent -- filtrage par scene des DEUX requetes physiques globales (`RoadSupported` et `RoadTop`), canaux UV 0-7 dans l'empreinte semantique et exclus de l'empreinte physique des meshes de collision, role de trottoir unique et symetrique (`SidewalkRole`, egalite de hauteur comprise), ensemble vide physique publie explicitement sans dependre d'un minimum present, garde d'ouverture/fermeture de `Bootstrap`, test d'integration de la porte visuelle pour un visuel nomme hors zone declaree, `AlgorithmVersion` 5. Les douze decoupes, les trajectoires 5.50, les donnees V1 et la lignee 5.27 sont inchanges.
- 2026-09-28 (acceptation proprietaire) : livraison acceptee (commit `2a9f4e2`) ; la story passe `done` dans la spec et le sprint. Les deux points visuels restent soumis a l'arbitrage ulterieur du proprietaire. Gate A reste fermee ; 5.28 reste `in-progress`.

## Verification

**Commands :** `./scripts/validate.ps1 -TestMode EditMode` (seul tripwire Gate A rouge permis) ; `./scripts/validate.ps1 -TestMode PlayMode` avant/apres en sessions fraiches. Controle visuel des cinq jonctions sous double garde.

**Etat 2026-09-28 (cloture de reprise, revue corrigee) :** mesure `algorithme 5` : 168 lignes, 0 echec, minima physique et semantique 0,112667568 m, 28 residus +inf publies explicitement, empreintes `2e1677f8...` / `9fae1d4e...`, rejouee **identique octet par octet apres rechargement de la scene** (`v1-regression-5-51/final-proof.txt`, SHA256 `782F60D0...`). V1/V2 identiques aux entrees gelees (blobs `d6f5fe71` / `d796696d` / `3b873515`, version `v4:e8dff9e5...`) ; NavMesh `563A438F...`, GUID `9fe3a419...` concordant meta / scene / fileID 23800000. Fixture ciblee **33/33** ; epreuve par mutation des huit correctifs : **8/33 rouges exactement attendus** ; EditMode complet **918 tests, 1 echec** = `Story528AuthoringAndGateATests.GateAIsOpenedOnlyByTheOwnersBoundSignoff` (Gate A fermee, seul tripwire permis) ; PlayMode session fraiche **39/45**, memes six echecs que la baseline, zero changement de statut ou de trace, une seule variation numerique de message (5.12 : `0,00443618931` -> `0,00443613529`). Captures brutes : `v1-regression-5-51/preserved-2026-09-28/reprise-*.txt` + `reprise-*-results.json`. Restent ouverts pour arbitrage proprietaire : le rendu visuel des cinq jonctions, les dalles de trottoir plus sombres que les pieces Synty et les portions de bordure physique sans visuel ; Gate A reste fermee ; 5.28 reste `in-progress` ; la 5.51 est acceptee et passee `done` par le proprietaire le 2026-09-28.

## Suggested Review Order

**Mesureur -- filtrage par scene et empreintes**

- Entree de la preuve : deux portes independantes, un residu publie par ligne.
  [`JunctionClearance.cs:247`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L247)

- Un relief ne repose que sur la chaussee de SA scene (OverlapBox filtre).
  [`JunctionClearance.cs:790`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L790)

- La surface roulable sous une pose suit la meme regle (RaycastAll filtre).
  [`JunctionClearance.cs:871`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L871)

- Empreintes : geometrie toujours, UV visuels 0-7 en semantique seulement.
  [`JunctionClearance.cs:979`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L979)

- UV hors collision : un changement purement visuel n'invalide plus le physique.
  [`JunctionClearance.cs:1014`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L1014)

- Etiquette de revision : le saut 4->5 documente les deux changements de preuve.
  [`JunctionClearance.cs:79`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L79)

**Porte visuelle -- declarations contre surfaces vues**

- Grille de concordance : la boucle couvre desormais les bords du domaine.
  [`JunctionClearance.cs:459`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L459)

- Role de trottoir unique : candidat, titulaire et egalite de hauteur symetriques.
  [`JunctionClearance.cs:616`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L616)

- Nom authoring reconnu : un nouveau couple mesh/materiau ne passe plus inapercu.
  [`JunctionClearance.cs:621`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L621)

- Deux cellules adjacentes de 5 cm font echec ; une cellule isolee reste toleree.
  [`JunctionClearance.cs:627`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs#L627)

**Tests adversariaux**

- Un support superpose mais etranger ne porte jamais le relief.
  [`Story551JunctionClearanceTests.cs:147`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L147)

- Le meme controle pour la surface roulable des poses (RaycastAll).
  [`Story551JunctionClearanceTests.cs:166`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L166)

- Exhaustivite : le jeu mesure egale l'import V1 ; 12 virages a temoin fini.
  [`Story551JunctionClearanceTests.cs:520`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L520)

- La regle de nom est liee a la porte : visuel nomme hors zone => echec, sinon silence.
  [`Story551JunctionClearanceTests.cs:575`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L575)

- UV : le canal principal et un canal secondaire modifient l'empreinte semantique.
  [`Story551JunctionClearanceTests.cs:727`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L727)

- UV d'un mesh de collision sans effet sur l'empreinte physique.
  [`Story551JunctionClearanceTests.cs:760`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L760)

- Bootstrap : ouverte par le test seulement si absente, fermee seulement par lui.
  [`Story551JunctionClearanceTests.cs:823`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L823)

- Portes vertes de reference : quatre T puis les cinq jonctions d'un bloc.
  [`Story551JunctionClearanceTests.cs:456`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L456)

- Recapitulatif global : `Failures` vide et cinq jonctions couvertes.
  [`Story551JunctionClearanceTests.cs:511`](../../Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs#L511)

**Preuves et artefacts**

- Preuve finale algorithme 5 : 168 lignes, 0 echec, rejouee a l'identique.
  [`final-proof.txt`](v1-regression-5-51/final-proof.txt)

- Generateur rejouable (hors tests) des preuves et de l'audit des angles.
  [`Story551FinalProof.cs:1`](../../AgentScripts/Story551FinalProof.cs#L1)

- Scene livree : 12 angles coupes, remplissages `Road_01`, curbs recules.
  [`ChamferRoadFill_TJunction_South_SE.asset`](../../Assets/RoadRage/App/Scenes/MVP_Run/ChamferRoadFill_TJunction_South_SE.asset)

- NavMesh rebake, meme GUID qu'avant la coupe.
  [`NavMesh-LaneGraph-Vehicle.asset`](../../Assets/RoadRage/App/Scenes/MVP_Run/NavMesh-LaneGraph-Vehicle.asset)
