---
title: 'Story 5.51 - Degagement physique des angles pour virages a droite'
type: 'feature'
created: '2026-09-28'
status: 'in-progress'
baseline_commit: '58c230a311ef61f81611ca640dd28b4c5bac28a9'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/planning-artifacts/epics.md'
  - '{project-root}/_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-28.md'
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
- [ ] `Assets/RoadRage/Features/Vehicles/Traffic/Migration/JunctionClearance.cs` - references prolongees de longueur/2 + marge + delta_c, poses canoniques (echantillons/coutures), empreinte gonflee. Physique : colliders non-trigger sans Rigidbody dynamique entre route et sommet IA, sauf relief routier franchissable (addendum 2026-09-28). Semantique : surfaces `Sidewalk` (aire par nom) meme a collider desactive, appariees aux visuels. Deux residus continus `min(d_a,d_b) - (|Delta p| + rho*Delta theta)/2` strictement positifs ; publier par coin/mouvement minimum, position, surface/obstacle, couture, h et deltas. Empreintes distinctes des entrees physiques et semantiques (aire, forme/activation, mesh et transform visuels), invalidation sur changement pertinent.
- [ ] `Assets/RoadRage/App/Scenes/MVP_Run.unity` et `MVP_Run/NavMesh-LaneGraph-Vehicle.asset` - dimensionner les 12 coupes minimales, ajuster curbs et visuels, rebaker NavMesh ; prefabs et Synty intacts.
- [ ] `Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs` - coin carre negatif puis coupe positive sur les deux gates ; trottoir plat, coins desactives du T Nord, route non `Sidewalk`, contact entre poses/coutures, tangentes degenerees, declaration sans visuel, invalidation des deux empreintes ; adversariaux, hauteur IA, invariants V1/V2, residus partout.
- [ ] `_bmad-output/implementation-artifacts/v1-regression-5-51/` - apres edition dans une autre session Editor : memes suites, comparaison par test/trace et disposition proprietaire des deltas ; documenter faisabilite, overrides, empreinte et residus. Puis `sprint-status.yaml` et `graphify update .`.

**Acceptance Criteria :**

- Given le premier angle modifie, when la scene est sauvee/rechargee, then forme physique, region `Sidewalk`, visuel et diff de scene concordent ; sinon aucun autre angle n'est deploye.
- Given les cinq jonctions, when tous les mouvements et raccords sont balayes, then les residus physique et semantique sont chacun strictement positifs et publies avec leurs empreintes separees, y compris pour `TJunction_North`.
- Given la coupe, when V1 et V2 sont recalcules, then source, lignee, identites, version et candidats restent identiques ; chaque delta comportemental V1 est soumis au proprietaire.

## Spec Change Log

- 2026-09-28 (addendum proprietaire) : A1 rejete au rendu (teinte `Greybox_Road_Mat`), A2 valide et applique. Relief routier franchissable = surface roulable pour le gate physique, selon le critere explicite (a) sans chevauchement `Sidewalk`, (b) repose sur la chaussee, (c) hauteur <= garde au sol statique du vehicule IA (0,158 m) ; boites de toute orientation projetees par l'enveloppe de leurs 8 sommets. Amendements appliques a `epics.md` (5.51), au correct-course (section 6), au spine et au contrat Road World Model ; version du mesureur 2. L'intention gelee est inchangee.
- 2026-09-28 (reprise apres HALT du premier angle) : Kenan a tranche sans changer l'intention. A1 : combler le triangle coupe avec une surface `Greybox_Road_Mat` sans collider ; A2 (mesh triangulaire propre au projet) si A1 n'est pas coherent ; A3 refuse. B1 : AD-33 conserve, la lecture `NavMeshModifier`/`Sidewalk` passe dans un adaptateur Editor hors `Features/Vehicles/Traffic/` (Code Map mis a jour). C : coupe de 1,00 m conservee au premier angle. Appariement visuel remplace par une concordance de surface visible conservative ; les rampes hors forme supportee sont mesurees, jamais ignorees.
- 2026-09-28 : Kenan a approuve la spec et le correct-course semantique. L'amendement figure desormais dans `epics.md`, le contrat Road World Model et l'architecture ; la mention d'attente dans le bloc gele reste l'historique de la revue.

## Verification

**Commands :** `./scripts/validate.ps1 -TestMode EditMode` (seul tripwire Gate A rouge permis) ; `./scripts/validate.ps1 -TestMode PlayMode` avant/apres en sessions fraiches. Controle visuel des cinq jonctions sous double garde.
