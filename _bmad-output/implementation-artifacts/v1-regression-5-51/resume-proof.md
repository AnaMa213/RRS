# Story 5.51 — audit de reprise et regression V1/V2

2026-09-28, HEAD `10fdb22453d34a018c804910e48036491e60e44f`. Baseline avant edition : `58c230a311ef61f81611ca640dd28b4c5bac28a9` dans [before-inputs.txt](before-inputs.txt). Gate A reste fermee ; aucun sign-off produit.

## Mesure de degagement

[Mesure complete](after-clearance-measure.txt) : algorithme 3, `h=0,05 m`, 52 declarations `Sidewalk`, 168 lignes couvrant les 5 jonctions, 0 echec. Minimum physique `0,112667568 m` ; minimum semantique `0,112664416 m`. Chaque ligne donne mouvement, surface, residus, position, obstacle, couture et delta. Empreintes distinctes :

- Physique : `3434683c294d514aec4e1a6f9b4fa61be674af818b0ac6ecf14850443dbfa18f`
- Semantique : `af2f5e292cc639c875b443aea7d735568341406b654a23ba80b3db89a34c09a8`

| Jonction | Lignes | Min physique (m) | Min Sidewalk (m) |
| --- | ---: | ---: | ---: |
| Intersection_Center_Crossroads | 60 | 0,112667568 | 0,112667568 |
| TJunction_East | 27 | 6,6449995 | 0,112664416 |
| TJunction_North | 27 | 6,6449995 | 0,1126647 |
| TJunction_South | 27 | 6,6449995 | 0,112664744 |
| TJunction_West | 27 | 6,6449995 | 0,112665176 |

Les reliefs publies sont `Rampe_Ouest`, `Rampe_Est` et `Relief_MarcheBasse_AvenueCenterToEast`, hauteur `0,12 m` contre garde statique IA `0,158031285 m`. Apres `open_scene` depuis le disque, [la seconde mesure](after-clearance-measure-reloaded.txt) est identique octet par octet, empreintes incluses.

## Scene, visuels et NavMesh

[Audit des 12 coins](after-corner-audit.txt) apres reouverture : chaque coin est une instance de prefab avec 3 `BoxCollider` et 3 `Chamfer_Visual_*` actifs, plus un `Chamfer_RoadFill_*` actif. Les deux parents `Col_Sidewalk_Corner_*` de `TJunction_North` restent desactives physiquement ; leurs declarations participent au gate Sidewalk. Les 36 visuels de coupe emploient `Greybox_Ground_Mat` ; les 12 remplissages emploient `Road_01`. Tous les renderers et meshes de remplacement sont presents et actifs. La concordance de surface visible/declaration passe dans la mesure complete et dans les tests adversariaux. Le jugement du rendu depuis les cameras des cinq jonctions reste a effectuer.

Le NavMesh rebake a SHA-256 `563A438F20E5BB1A56C3519BB6E99D30F41E25956C66F76A49755987064245F6`. Son GUID `9fe3a419373dd70419df65dcf5d047eb` concorde entre `.meta`, `MVP_Run.unity` et `NavMeshSurface.m_NavMeshData` rechargé (`fileID 23800000`). La scene est `isDirty=false` apres reouverture. Le diff touche la scene et le NavMesh, pas les prefabs, Synty, courbes ou donnees Road Model.

## V1/V2 et tests

[AuthoredRun frais](after-inputs.txt) : succes, 120 candidats, 72 mouvements compiles et 120 zones ; `RoadModelVersion`, `SourceHash`, `LineageHash` et `DecisionsHash` identiques a la baseline. Les blobs Git du modele, de la lignee et des decisions sont inchanges. Les 120 decisions de [after-candidates.txt](after-candidates.txt) correspondent exactement aux 120 lignes de `before-candidates.txt` ; les 120 empreintes candidates recalculees forment le meme ensemble.

`scripts/validate.ps1 -TestMode EditMode -TestFilter RoadRage.Tests.EditMode.Story551JunctionClearanceTests -TestTimeoutSec 600` : **26/26 passes**, aucune erreur Console, scene propre. `scripts/validate.ps1 -TestMode EditMode -TestTimeoutSec 600` : **910/911 passes** en 398,08 s ; [resultats](after-editmode-results.json). Seul echec : `Story528AuthoringAndGateATests.GateAIsOpenedOnlyByTheOwnersBoundSignoff`, attendu tant que Gate A reste fermee. Le premier lancement complet avec plafond par defaut de 300 s avait expire avant le verdict et produit une erreur Pipeline transitoire ; elle ne s'est pas reproduite au second lancement.

## PlayMode en nouvelle session Editor

`unity close D:\Projets\RRS --timeout 60` a ferme normalement le PID `39396` (`method=graceful`, sans `--force`). `unity open D:\Projets\RRS` a lance le PID `34584`, `ready`, avec `MVP_Run` chargee et `isDirty=false`. La Console erreur etait vide avant le run.

`scripts/validate.ps1 -TestMode PlayMode -TestTimeoutSec 600` sans filtre : **39/45 passes, 6 echecs**, aucune erreur Console. [Resultats apres edition](after-playmode-results.json) compares a la [baseline avant edition](before-playmode-results.json) par `FullName`, statut, message et trace : les 45 tests sont presents des deux cotes ; **zero changement de statut et zero changement de trace**. Les six memes tests echouent dans les deux runs : un de 5.12 (frein a main), quatre de 5.13 (contacts/relief), un de 5.7 (scene Bootstrap). Cinq messages sont identiques. Le message 5.12 contient une variation numerique de `0,00443618931` a `0,00443613529` pour le glissement arriere ; le verdict et la trace sont inchanges. Aucun nouveau delta comportemental V1 n'est mis en evidence par cette suite. Apres le run, `MVP_Run` reste `isDirty=false`.

## Ecarts ouverts

Le rendu des cinq jonctions depuis les cameras de jeu et la validation esthetique des remplissages `Road_01` n'ont pas recu d'acceptation visuelle du proprietaire. Les controles de structure/mesh/materiau et la concordance geometrique automatises couvrent les 12 angles, mais ne remplacent pas cette decision. La revue de code independante reste a faire. Gate A demeure fermee ; 5.28 et 5.51 restent `in-progress`.
