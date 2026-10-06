<!-- rrs-gate-a-binding
source-hash: b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc
importer-version: 2
compiler-schema-version: 5
pipeline-version: 3
model-id: 419bd12ec9b5fe710e8a3719692c7982
lineage-hash: f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4
decisions-hash: 98e1e4f240b74cfe05433451ccd05c60be68d43b0eaf5f0d33058d5b390119e4
model-hash: a645efb4b12e0d8d0f20bc8282f5522451490b881cffa9245cd4ee083205b32e
road-model-version: v5:6d145fd3dc3b35d3f8307aee692448bb
overlay-hash: bc049fa7a3f0fc92a85cad386a12ced4ce5652659f3b09e2a12c6f0739508df2
physical-input-hash: c777ce4ebb537362ace165a24d68f844c58a0572eac76e83dcb7bad659afd2d9
semantic-input-hash: 43e098fcd6809ecc1e8043156f7cd26f57abb73121b844cf6c8badef8b79aef4
clearance-hash: 7b7fcbf8f2458e4165ad2adb47e7222b52b12e5754baddbb8a76a277c555f14a
body-hash: dcbd5d1acec3e51bc095bbb5e97d1b269becedf09de97c5547bc8d85bb2855e5
-->
# Rapport Gate A : modele authore MVP_Run (Story 5.28)

Genere par le menu `RoadRage/Traffic V2/Compiler le modele authore`. Ne pas editer : un rapport retouche ou detache de ses entrees est rejete, jamais repare.

## Liaison

| Champ | Valeur |
|---|---|
| Hash de la source V1 extraite | `b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc` |
| Version de l'importeur / du pipeline | 2 / 3 |
| CompilerSchemaVersion | 5 |
| RoadModelId | `419bd12ec9b5fe710e8a3719692c7982` |
| Hash de la lignee | `f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-lineage.json`) |
| Hash des decisions | `98e1e4f240b74cfe05433451ccd05c60be68d43b0eaf5f0d33058d5b390119e4` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json`) |
| Hash du modele persiste | `a645efb4b12e0d8d0f20bc8282f5522451490b881cffa9245cd4ee083205b32e` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json`) |
| RoadModelVersion | `v5:6d145fd3dc3b35d3f8307aee692448bb` |
| Hash de l'overlay | `bc049fa7a3f0fc92a85cad386a12ced4ce5652659f3b09e2a12c6f0739508df2` (`_bmad-output/implementation-artifacts/overlay-5-28-mvp-run.txt`) |

## Resultat

- Erreurs dures : **0** (le modele passe `Compile` ; une seule erreur aurait bloque toute ecriture).
- Lignee inchangee : 0 identite frappee, 0 retiree (import 5.27 relance en lecture seule).
- Taches 5.27 non disposees : **0** sur 118.
- Fixtures de localisation : 6 vertes sur 6.
- Preuve physique Gate A (9 carrefours) : **verte**.

## Modele authore

| Enregistrement | Nombre |
|---|---:|
| JunctionMovement | 72 |
| JunctionControl | 40 (un par approche : 20 `Priority`, 4 `Uncontrolled`, 16 `Yield`) |
| ConflictZone | 114 (decisions acceptees seulement) |
| SignalPlan | 0 (carrefours declares non signalises) |
| LaneAdjacency | 0 (aucune adjacence authoree) |
| Ligne d'arret | 4 (controles Stop ou Yield dont la map porte l'intention ; replis : table des controles) |

## Controles par approche

Un `JunctionControl` par corridor d'approche, lie a tous les mouvements partant de cette approche. Genres authores selon l'intention reelle de la route (Story 5.35, P2) : giratoires `Yield` aux entrees et `Priority` sur l'anneau, T `Priority` sur l'axe traversant et `Yield` sur la branche, croix `Uncontrolled` (priorite a droite). `Signalized` reste refuse (5.36). Chaque mouvement a exactement un controle.

| Carrefour | Controle | Genre | Approche | Mouvements | Ligne (s_line par mouvement) |
|---|---|---|---|---:|---|
| Roundabout_NorthWest | `42f3d162d75e244896863e1711a77287` | Priority | `4ec40e5f82f7a65bed1dc3d9679f8693` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | 2 | - |
| Roundabout_NorthWest | `4332ef31bc230cb1e928d17c87c2b2a4` | Priority | `43e96165bde1e8229cf8d27082d6b798` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | 2 | - |
| Roundabout_NorthWest | `4538b544fa7443645e9f0b55dfd20c81` | Yield | `4d5c15fca8e24f15b1bdf0f0695acebf` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-981631451>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-981631451 | 1 | repli entree generique (s = 0) |
| Roundabout_NorthWest | `48d729046272166df666b735d860ff85` | Priority | `4ccc97c5021ae91587883344ee3ca299` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | 2 | - |
| Roundabout_NorthWest | `4e6af9f2c3ccc0a6b1f0ef739679ccac` | Yield | `4516fd525d5bf0205cb47b0093ab0794` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1227312198>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1227312198 | 1 | repli entree generique (s = 0) |
| Roundabout_NorthWest | `4fcbd57e6baa66a28b0732836bc2e880` | Yield | `4f3543b1218b82af65b5b8fc58457fb3` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-234956567>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2050073441738459335-234956567 | 1 | repli entree generique (s = 0) |
| Roundabout_NorthEast | `4258e4997d604bdaddd64f90730258ae` | Yield | `4ae5e1290d8b815bcb8f509e915fef89` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1335115730>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1335115730 | 1 | repli entree generique (s = 0) |
| Roundabout_NorthEast | `43fe03f1814155e96d6d1d49b9f2ffa4` | Yield | `455e4360eac83d9900ad7ea698758890` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-656642079>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-656642079 | 1 | repli entree generique (s = 0) |
| Roundabout_NorthEast | `469dd39290081f19c5f45716e14fa988` | Priority | `4c3fbf9d31b3efe44c908138001098b8` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | 2 | - |
| Roundabout_NorthEast | `4b8f1772f907da8ab8daecc656395b8b` | Yield | `4feeebdeeefd1a0f872a7e456b3a5287` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-1574439523>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2050073441738459335-1574439523 | 1 | repli entree generique (s = 0) |
| Roundabout_NorthEast | `4df5972c8018084b60d1e41cee091db6` | Priority | `432025ce90895a6728955c79e3edfb9e` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | 2 | - |
| Roundabout_NorthEast | `4e578eb71bb93361716c13717c771faa` | Priority | `4e67d7a19f6083c020b14cda38733f93` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | 2 | - |
| TJunction_West | `48171f4957d3bd8970fed52ff8b47a83` | Yield | `4f56ef8aae6bfffcbc337d463dd2fd9f` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-972045385>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-972045385 | 2 | 3.4749 m / 3.4108 m |
| TJunction_West | `4c4d0695610e00bb74583d4b6163728a` | Priority | `46b219bca5cdaf1087d30d9622fab0a5` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1516681040>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1516681040 | 2 | - |
| TJunction_West | `4df2f2bab0514361bd8aaa81b0e0049f` | Priority | `4db6ebc158e0dff4369373f117f51690` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-981631451>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-981631451 | 2 | - |
| Intersection_Center_Crossroads | `413075398038b1e09d306528753f6b9e` | Uncontrolled | `45daa67f26d456b653f543a16ea6dba5` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-531979442>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-531979442 | 3 | - |
| Intersection_Center_Crossroads | `4575f7c642af9766e86aebffaec3cfb3` | Uncontrolled | `44d95c53b9c058de55da54065eed7882` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1812162174>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1812162174 | 3 | - |
| Intersection_Center_Crossroads | `48bec7b12bfc173be79c7ff6dc888e82` | Uncontrolled | `48c833c71f68abef819cb5a36c99208f` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-972045385>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-972045385 | 3 | - |
| Intersection_Center_Crossroads | `4b0592bd1e8ddf114991c13fb4632285` | Uncontrolled | `4680aaa6ee1678ef03909266658047b6` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1461432457>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1461432457 | 3 | - |
| Roundabout_SouthEast | `44924805a30ee7ba97a11b1d9b92919e` | Yield | `4f8c2539b5152e69c8fe4d64939f96b9` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-235211969>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-235211969 | 1 | repli entree generique (s = 0) |
| Roundabout_SouthEast | `480071f81c9ce0ded1b93c6a043d7787` | Priority | `41d3971913a0bc729dacb328aaa17495` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | 2 | - |
| Roundabout_SouthEast | `481a5e58c50d4c4a68188022a32041a9` | Priority | `4e878e4befb474cfbdc785346bd6f6a4` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | 2 | - |
| Roundabout_SouthEast | `4997c37d1df9457fd9b6d871b5d9f580` | Yield | `4eb54bbcb45c5ab91993e545746f24a9` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1375911139>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1375911139 | 1 | repli entree generique (s = 0) |
| Roundabout_SouthEast | `4b9dc3fe78d50ce0ed024b340d1a5c9c` | Yield | `4403c568617df5ea8f9a2fdf743748ad` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-75338410>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2050073441738459335-75338410 | 1 | repli entree generique (s = 0) |
| Roundabout_SouthEast | `4cce28bd136319d98afaa9f310a74dbe` | Priority | `4b5540b59d5cd5141bb4bb279d10fb96` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | 2 | - |
| TJunction_North | `494595a32d7821f3022e012e6e6b6a94` | Priority | `47de8d1a8e71016a20b301b8a86852a5` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1227312198>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1227312198 | 2 | - |
| TJunction_North | `4c44e8caec8bc2c8291581b575127a8d` | Yield | `4d56a92042f124db7947938fefa7c7a7` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1461432457>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1461432457 | 2 | 3.4749 m / 3.4108 m |
| TJunction_North | `4f0d2476b2d1a607c40bf041509af48e` | Priority | `4bc86d56e68c64ed3454e51566dd43bc` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-656642079>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-656642079 | 2 | - |
| TJunction_South | `456fc7aec92eae3a031351a23bee3fa7` | Priority | `49980393f36fca422e64472c8ccc009c` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-235211969>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-235211969 | 2 | - |
| TJunction_South | `4ce3fd28fd4195f1d5cbe44f4e66fb87` | Priority | `4f4e0f3e7e8b2b6b059a8a6edab2bc90` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-539479367>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-539479367 | 2 | - |
| TJunction_South | `4fa7a31c8eaaa03cd839e636079b349e` | Yield | `42ad3d5187bfa4104bf4917b93ede39e` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1812162174>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1812162174 | 2 | 3.4108 m / 3.4749 m |
| Roundabout_SouthWest | `4125ec30a583176d1fbb06c776fa858f` | Yield | `433f39ba8eeb012212b7d5a05468b09d` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-539479367>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-539479367 | 1 | repli entree generique (s = 0) |
| Roundabout_SouthWest | `45e4596d6aa585a2517bee9c8fb834be` | Yield | `4d1d749db78465cdc4e90ee844e3d898` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-1045154302>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2050073441738459335-1045154302 | 1 | repli entree generique (s = 0) |
| Roundabout_SouthWest | `47ef2473ecfa0206cc2301cc291f7886` | Priority | `41aade042322996080b638a3a86e1088` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | 2 | - |
| Roundabout_SouthWest | `4aeddcba2f3223edb3ec477c2638bea8` | Priority | `427041528b28cda9499c4ab5aafe0d9c` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | 2 | - |
| Roundabout_SouthWest | `4cf54317710e5723de29224b473fd89f` | Yield | `40e937a99618cac3ce12d56586514283` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1516681040>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1516681040 | 1 | repli entree generique (s = 0) |
| Roundabout_SouthWest | `4d6b68fe5f6aaa73f9d436e8c46044a2` | Priority | `48e681e799268186449903ef2aa3258f` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | 2 | - |
| TJunction_East | `4370991f7408a41ba87432f030daf299` | Yield | `43cf13ae1ef6b701bbcd906cdea7df90` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-531979442>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-531979442 | 2 | 3.4749 m / 3.4108 m |
| TJunction_East | `4b5be59852dbf8c943063c130a89abbe` | Priority | `4bc735b758e931a2e6a2faa6a7cf198b` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1375911139>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1375911139 | 2 | - |
| TJunction_East | `4e342ab49e0487a48fc599a311150694` | Priority | `4970addfd3bf6360c8191bf0f3eeafb7` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1335115730>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1335115730 | 2 | - |

## Relation de droite

Derivee de la geometrie (cap en fin de corridor d'approche, plan route), jamais authoree ; calculee pour les carrefours `Uncontrolled` seulement (priorite a droite). Fonction `right-of-way-v1`, secteurs de 90 deg, bande ambigue de 10 deg. Hash `03b75b53c644d52b5a48158c774bf4bfea8e183b70f95577a09f360d0ecf15d3` (lie a la signature Gate A). Paires ambigues : 0.

| Carrefour | Approche A | Approche B | Angle de B vu de A (deg) | Relation |
|---|---|---|---:|---|
| Intersection_Center_Crossroads | `413075398038b1e09d306528753f6b9e` | `4575f7c642af9766e86aebffaec3cfb3` | 90.0 | FromLeft |
| Intersection_Center_Crossroads | `413075398038b1e09d306528753f6b9e` | `48bec7b12bfc173be79c7ff6dc888e82` | 180.0 | Opposite |
| Intersection_Center_Crossroads | `413075398038b1e09d306528753f6b9e` | `4b0592bd1e8ddf114991c13fb4632285` | -90.0 | FromRight |
| Intersection_Center_Crossroads | `4575f7c642af9766e86aebffaec3cfb3` | `413075398038b1e09d306528753f6b9e` | -90.0 | FromRight |
| Intersection_Center_Crossroads | `4575f7c642af9766e86aebffaec3cfb3` | `48bec7b12bfc173be79c7ff6dc888e82` | 90.0 | FromLeft |
| Intersection_Center_Crossroads | `4575f7c642af9766e86aebffaec3cfb3` | `4b0592bd1e8ddf114991c13fb4632285` | 180.0 | Opposite |
| Intersection_Center_Crossroads | `48bec7b12bfc173be79c7ff6dc888e82` | `413075398038b1e09d306528753f6b9e` | 180.0 | Opposite |
| Intersection_Center_Crossroads | `48bec7b12bfc173be79c7ff6dc888e82` | `4575f7c642af9766e86aebffaec3cfb3` | -90.0 | FromRight |
| Intersection_Center_Crossroads | `48bec7b12bfc173be79c7ff6dc888e82` | `4b0592bd1e8ddf114991c13fb4632285` | 90.0 | FromLeft |
| Intersection_Center_Crossroads | `4b0592bd1e8ddf114991c13fb4632285` | `413075398038b1e09d306528753f6b9e` | 90.0 | FromLeft |
| Intersection_Center_Crossroads | `4b0592bd1e8ddf114991c13fb4632285` | `4575f7c642af9766e86aebffaec3cfb3` | 180.0 | Opposite |
| Intersection_Center_Crossroads | `4b0592bd1e8ddf114991c13fb4632285` | `48bec7b12bfc173be79c7ff6dc888e82` | -90.0 | FromRight |

## Lignes d'arret : separation (P5)

Empreinte maximale de Gate A arretee pare-chocs avant a `m_ctrl` - 0.020 m de la ligne, comparee au repli generique b = 0. Cas A : separee a b = 0, la ligne doit le rester. Cas B : contact conservatif deja present a b = 0, marge non degradee au-dela de `EnvelopeOverlapToleranceMeters` (0.050 m) et aucun nouveau recouvrement nominal. Marges en metres (negatif : contact Gate A) ; distance nominale sans gonflement.

| Mouvement | s_line | Mouvement en conflit | Cas | Marge b = 0 | Marge ligne | Delta | Nominal b = 0 | Nominal ligne | Verdict |
|---|---:|---|---|---:|---:|---:|---:|---:|---|
| TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | B | -1.192 | -1.103 | 0.089 | 1.688 | 1.616 | conforme |
| TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | A | 6.960 | 3.668 | -3.292 | 9.050 | 5.520 | conforme |
| TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | A | 2.577 | 0.009 | -2.568 | 5.050 | 1.520 | conforme |
| TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | B | -1.192 | -1.103 | 0.089 | 1.940 | 1.817 | conforme |
| TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | B | -1.192 | -1.111 | 0.081 | 1.688 | 1.590 | conforme |
| TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | A | 6.960 | 3.760 | -3.200 | 9.050 | 5.620 | conforme |
| TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | A | 2.577 | 0.091 | -2.485 | 5.050 | 1.620 | conforme |
| TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | B | -1.192 | -1.111 | 0.081 | 1.940 | 1.651 | conforme |
| TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | A | 6.960 | 3.668 | -3.292 | 9.050 | 5.520 | conforme |
| TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | B | -1.192 | -1.103 | 0.089 | 1.940 | 1.817 | conforme |
| TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | A | 2.577 | 0.009 | -2.568 | 5.050 | 1.520 | conforme |
| TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | B | -1.192 | -1.103 | 0.089 | 1.688 | 1.616 | conforme |
| TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | A | 6.960 | 3.760 | -3.200 | 9.050 | 5.620 | conforme |
| TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | B | -1.192 | -1.111 | 0.081 | 1.940 | 1.651 | conforme |
| TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | A | 2.577 | 0.091 | -2.485 | 5.050 | 1.620 | conforme |
| TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | B | -1.192 | -1.111 | 0.081 | 1.688 | 1.590 | conforme |
| TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | B | -1.192 | -1.103 | 0.089 | 1.940 | 1.817 | conforme |
| TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | B | -1.192 | -1.103 | 0.089 | 1.688 | 1.616 | conforme |
| TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | A | 6.960 | 3.668 | -3.292 | 9.050 | 5.520 | conforme |
| TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | A | 2.577 | 0.009 | -2.568 | 5.050 | 1.520 | conforme |
| TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | B | -1.192 | -1.111 | 0.081 | 1.940 | 1.651 | conforme |
| TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | B | -1.192 | -1.111 | 0.081 | 1.688 | 1.590 | conforme |
| TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | A | 6.960 | 3.760 | -3.200 | 9.050 | 5.620 | conforme |
| TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | A | 2.577 | 0.091 | -2.485 | 5.050 | 1.620 | conforme |
| TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | B | -1.192 | -1.111 | 0.081 | 1.940 | 1.651 | conforme |
| TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | A | 6.960 | 3.760 | -3.200 | 9.050 | 5.620 | conforme |
| TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | B | -1.192 | -1.111 | 0.081 | 1.688 | 1.590 | conforme |
| TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | 3.411 | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | A | 2.577 | 0.091 | -2.485 | 5.050 | 1.620 | conforme |
| TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | B | -1.192 | -1.103 | 0.089 | 1.940 | 1.817 | conforme |
| TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | A | 6.960 | 3.668 | -3.292 | 9.050 | 5.520 | conforme |
| TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | B | -1.192 | -1.103 | 0.089 | 1.688 | 1.616 | conforme |
| TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | 3.475 | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | A | 2.577 | 0.009 | -2.568 | 5.050 | 1.520 | conforme |

## Candidats de conflit et decisions

Candidat = paire de mouvements du meme carrefour, d'approches differentes, dont les enveloppes balayees se recoupent (croisement ou convergence). Rayon balaye r = demi-gabarit max 1.0300 m + marge 0.2500 m = 1.2800 m (profil versionne) ; courbes densifiees a pas r/4, recoupement a moins de 2r. Genere hors ligne sur le modele compile sans zones ; seules les decisions acceptees deviennent des `ConflictZone`, aucun consommateur n'infere de conflit.

| Carrefour | Candidats | Acceptes | Rejetes | Paires de meme approche (suivi, pas conflit) |
|---|---:|---:|---:|---:|
| Roundabout_NorthWest | 10 | 8 | 2 | 3 |
| Roundabout_NorthEast | 10 | 8 | 2 | 3 |
| TJunction_West | 12 | 10 | 2 | 3 |
| Intersection_Center_Crossroads | 48 | 42 | 6 | 12 |
| Roundabout_SouthEast | 10 | 8 | 2 | 3 |
| TJunction_North | 12 | 10 | 2 | 3 |
| TJunction_South | 12 | 10 | 2 | 3 |
| Roundabout_SouthWest | 10 | 8 | 2 | 3 |
| TJunction_East | 12 | 10 | 2 | 3 |

| Carrefour | Mouvement A | Mouvement B | Decision | Zone | Motif |
|---|---|---|---|---|---|
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `405ba721192ad202e99c9ba49b5e9fb7` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_NorthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Accepted | `4f1c595ed26310a51f9f60a0f3ead586` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthWest | Roundabout_NorthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `47114f641089fae85c0ee8305935fdaa` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthWest | Roundabout_NorthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `40b126f3a984fa21883e88b3f099319c` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `45c7253beaef1bef98463cee884919a4` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_NorthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Accepted | `470ef9a2fd6c8d61f93092fe1646ba97` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthWest | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Roundabout_NorthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Roundabout_NorthWest | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `43a738a3e1cbfb568fadc5b3548a1b8f` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `43e5a24dac93246333d9faf399592587` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `415d14c83ddadbaca56eba4406bcf2b8` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_NorthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Accepted | `458eb40d533eff27d37e8ed61c6eaabb` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_NorthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Accepted | `4cbd976887b4f64454c372023ad131a2` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `41ddaaa21dd44070074b0aa5e4581880` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Roundabout_NorthEast | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Roundabout_NorthEast | Roundabout_NorthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `4ef00a041311cff1569a33b5a476aa88` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_NorthEast | Roundabout_NorthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `44434fa7a74b378cd42ed08c42fe8692` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `40e3e192ec85c0457be62034d55155a4` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `49b398688f7efc5feae64cccd10773ac` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4ce031f1b7b682c6e75c39e87a7b4ab7` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4258af5419bba1365a3f0ad6ed3d44aa` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `40ce606892963a8943f86c2ecd963ba2` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4b49ca4b2117973d8355dbf1660471b3` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `470bc18b1824ae1e7e47ac80c41aca9c` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4b786539f4809cd080c6de032000a3bd` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4fbcc8e5f354d981b2a3580b4645f6b7` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `406690a88e36b4596ca05b5832a555a9` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_West | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4905f97ebed6b813a1b1ba7b9fc594bb` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_West | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `40fd4fd762a2c46f4f8087d3f9a597b2` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4fade05dfa8725d21db5702002aa4ca3` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4c31ad754ca8b25911fbb0f12ca04f90` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4b168164d6d552a9e382264821c7dcbe` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4aeb5f7d500459bab9a69a74b6996994` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `41143c56ca46d5c88f92b78870868c87` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4c05c15b5871449b27880b4c5c1f2e92` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `46987262dd6fe1111a00229ab6c9d8ab` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `421fede006aa2f42e5a6fd0002a6d696` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Accepted | `490e9e2ea4bef88bf4a8ee14433a429b` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4ac63be5d9fb0655462900ee2188a088` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `448d024c97cec79a916b160742727798` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4645efe76bbc8c5cda09e53a759b85b9` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `42665e8c1aa6739044cecc21fb9f79be` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4037740c6f0de354c5000388516f6185` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47c31dcb178dc8692f142c61851d9cb7` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `417f3abf3c05390e7a87ecfd98a6a481` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4f93788f6768a9c8d22bd432fff01490` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `45f53e49188f42e47576c9788d6c899b` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `454750014c7f7da3a45a39cb6189b982` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4cb4ca66e2b91ace8ba4ce3150bf3ebb` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `49c2d7c1095d014dcd2e9707187a5d9e` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `434cfd5ad42d31287fd0fc4be036f3b3` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4d99e930a4f47c2ed260aed77cac1f8d` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4987ba61c57809e24605d2648053e29e` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `49a2893231955267ed28fd25ded38ba0` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `45f30575f87cd3d65d78177ee3c66d85` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Accepted | `45a0180046cf5cfca98979d7fcd8828a` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `488452c23f729a6d1d1f9aa582c2bf8f` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `452cc0b3f8ae471194e7907f1351d3ad` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Accepted | `4b29ef3394ffa2df90a10e235b95a59c` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `44eb41658994bf0d068556234e7c7491` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4065d99bf14d8ee4c3e42099de20fa89` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `44ef4e39ca0ade7338fc8feec0f429be` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4a4ed5b2470adc5ba3350df9cc85768e` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4d643d916a884b3c1233a665b66d89bc` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4a73e9da4c0ce92c90703379e30d7b9f` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `49340dcf8a92884fb95006b2ec095381` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4c9b462c1b8587eb241406b3a9d11683` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4b4cb6b3d76549de0935338e2b66aea9` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4c3054544ab019cafa18a48c279614b0` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4a6c823b85b757d2f3505c8789aa528c` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4c20416c36051baa4edfb294cd6e169b` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `4e24e297c4d1bd9139c77a07f6177abb` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_SouthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Accepted | `48fd616bb1789845fc2043ad1f0ba7a5` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_SouthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Accepted | `433fa627c90e7aef2af3fd986d2faa8d` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_SouthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Accepted | `45168e2b5ecebf3c89b80036bbddd0af` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `45347e3ef39bcc64fdfccaf89295a4bd` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `406efc13fc2d5de520cbe34197314db3` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `4e2ebb271a55553f65fe7bc0087b7482` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Roundabout_SouthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Accepted | `4fa896603af971df28394d41512e83b8` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4b91ce1faf5f47a051923e55db19d084` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `49e18ec3cedc8169b621139413c88eab` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4782b2025438431bd3b4c6d8fedfe99b` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47d2bcbf20f128f9b2cf7d5f8dee60b9` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `436441bec58608acbc6fb187181a6bbe` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `43c7c6dff15ddd8ea442f3b4cb85a884` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4a85cf73649c4ac79f7a21112c98daa3` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4875672f99ccdb811ea74f51b86d298f` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4e90907066432b37ea55bf8b6c7967b3` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4e982b0fa19ae9f5ee3c0bc424e8ac91` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `46c0852ee863ff838d59392162782297` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4fce84fd1a162287b7b06ed08526d094` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `46a27562fc7ef73899775b45767338b1` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4651661907bde7ddf496e0b38b1a67b2` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4a44f9710a830e7f77c6952d114c4fbf` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4dc9c6211a49ca8f0980f973af7a1bac` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `46fef049de32bffeccdcc752588447a5` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4ba61b4456363d785c934bbc3dbb98aa` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_South | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4c3765f4eeddd6c6be43e3c0c8c83182` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4c147a4d6d6d6cd2fde23d045167c692` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_SouthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Accepted | `4fedf986cee6f96bf250c5b1f2439e82` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_SouthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Accepted | `4b38f8659ad8dac11a7d0a0b53fc47b5` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `450099ba966907d2e411d1b20b54ae99` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `4a4250039f40a26ff5b4051a197c149d` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `42b83e7c01b9cbacc9919a1bb7e9e68c` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `451483d7154f55215bd3214ab24e3d98` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `49b9fb8a95b566317bd90511c5724b82` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_SouthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Accepted | `4df53fca19b7ee5211d4e512b3a4da86` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Roundabout_SouthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `499ace82c622f81cf600719848bae693` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `489e099514f065e68f19cf4dbfe350b2` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47c7fee4039145473209b7b5d87472b3` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `49f9a6d925302128b2c14ee2328fabb6` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `46a75b0c04792a9ac17415da45f3c98f` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `433b2a4fbf3fe4c8557ecd7b832e4fb1` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4a435f4e82a6a473eee3d534a3378fb9` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `468af52ce1bbea22b1482f15af632390` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |
| TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `48dacd01988ac15ff1cf37499497f885` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_East | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | Rejected | - | Apres raffinement, chaque feuille possede une borne de separation strictement superieure a la tolerance. |
| TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `43bbfeff3c02c3bf890cd95139e295a3` | Le raffinement trouve un temoin : deux poses dont les rectangles orientes gonfles se recouvrent. |

## Largeurs revues

Demi-largeurs gauche et droite explicites (AD-45). La largeur revue est APPLIQUEE aux echantillons possedes (5.49) : `Uniform` ecrit la decision ; `EndpointInterpolation` (carrefours seulement) interpole chaque mouvement en s/Length entre les largeurs appliquees de ses corridors d'extremite, la decision valant plancher. Tout echantillon reste >= demi-gabarit + marge (1.2800 m) de chaque cote. Importee = amorce de l'importeur, jamais une autorite ; min-max sur les echantillons du sujet.

| Sujet | Decision g / d (m) | Application | Importee g / d (m) | Appliquee g / d (m) |
|---|---:|---|---:|---:|
| `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | 2.0000 / 2.0000 | EndpointInterpolation | 2.0000 / 2.0000 | 2.0000-4.3000 / 2.0000-4.3000 |
| `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | 2.0000 / 2.0000 | EndpointInterpolation | 2.0000 / 2.0000 | 2.0000-4.3000 / 2.0000-4.3000 |
| `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | 2.0000 / 2.0000 | EndpointInterpolation | 2.0000 / 2.0000 | 2.0000-4.3000 / 2.0000-4.3000 |
| `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | 2.0000 / 2.0000 | EndpointInterpolation | 2.0000 / 2.0000 | 2.0000-4.3000 / 2.0000-4.3000 |
| `4730190af6f78f0cd48d4bb005c9f091` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4dab8dc01f01a819f72ef4dc8a8a459d` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `49ecb9240fd46b56a68f2ba5f9b575aa` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4b5389d91d5915f3196d95d243bc789f` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4b8232a70eb5574526502f1072cb50bd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1045154302 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `443ed3bc9f5371ef4f1439781cd012a8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1574439523 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `46c85a9afac17bde2153665adab63fbc` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-234956567 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `42de5da740b31ac0176161c776f3d4a1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-75338410 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `47b9c1ab789b84066b536acecec183ab` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1227312198 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4ad222fbe3e52d2a5b6f86b332946c80` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1335115730 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4c56cbc620e585a5373ea0ea9acae384` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1375911139 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `46f7f22596a1e5204781d18a86e839ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1461432457 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `48b41f08eb1eee61a4e6281a4e963893` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1516681040 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4b08788f2b3434539d6daf47c410c6a4` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1812162174 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `410b074af81719b5992afb35a5d20cbd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-235211969 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4f545c3fada86d11df7692a0081aa1a5` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-531979442 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4261bcbef0b8a38b712c649286b082ba` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-539479367 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `488cff3f9501412fcae92bf829779984` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-656642079 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4581e2ae94d3287b91ad57e31a9fe989` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-972045385 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `431aab2ccfb1de591d13a38baa04b796` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-981631451 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `44687593778b414acca9ab06ece502b1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4a53609938eef355087249c1b74ca6b0` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4815e26dcb7e3aa355dfa72569096986` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4796d8afab7aafe3c213381ce7be51b8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4640b60be5be840f1fa778f300b64d84` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4b33ebfdfb0a4ef2c2c6cbb267a5f2be` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `4de2f43948bbec45fce0fbe76d4658ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |
| `42134703636fbc25221f6dc2ca06c793` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | 4.3000 / 4.3000 | Uniform | 2.0000 / 2.0000 | 4.3000 / 4.3000 |

## Giratoires : degagement a deux gabarits

Deux gabarits max du profil versionne (W/2 = 1.0300 m, L = 4.5000 m, marge m = 0.2500 m) cote a cote, cap tangent, au point le plus serre : R_in = r_in + m + W/2 ; c_in = sqrt((R_in + W/2)^2 + (L/2)^2) ; R_out = c_in + 2m + W/2 ; c_out = sqrt((R_out + W/2)^2 + (L/2)^2) ; residu = (r_out - m) - c_out. Preuve supplementaire : un residu positif ne reduit jamais la cible (anneau V2 4,0 / 4,0 m, ilot <= 1,75 m, pave >= 10,25 m).

V2 : centre = racine du module ; corridors d'anneau et continuations appliques ; r_in = max des bords interieurs, r_out = min des bords exterieurs. Physique : empreintes XZ des colliders ; r_in = portee de `Col_Island` ; pave = min sur 720 rayons (pas 1 cm) de la sortie de l'union des `Col_Roadway*` ; obstacles = colliders non declencheurs hors chaussee et ilot dont la hauteur recoupe [sommet de route, +2.0000 m] ; r_out = min(pave, obstacle le plus proche).

| Instance | V2 r_in (m) | V2 r_out (m) | Residu V2 (m) | Ilot (m) | Pave (m) | Obstacle le plus proche | r_out physique (m) | Residu physique (m) |
|---|---:|---:|---:|---:|---:|---|---:|---:|
| Roundabout_SouthWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495` | 1.7000 | 10.3000 | 0.0759 | 1.5000 | 10.3700 | TunnelPortal_SouthWest/Col_Wall_Left a 11.3137 m | 10.3700 | 0.3423 |
| Roundabout_NorthWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255` | 1.7000 | 10.3000 | 0.0759 | 1.5000 | 10.3700 | TunnelPortal_NorthWest/Col_Wall_Left a 11.3137 m | 10.3700 | 0.3424 |
| Roundabout_NorthEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718` | 1.7000 | 10.3000 | 0.0759 | 1.5000 | 10.3700 | TunnelPortal_NorthEast/Col_Wall_Left a 11.3137 m | 10.3700 | 0.3424 |
| Roundabout_SouthEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077` | 1.7000 | 10.3000 | 0.0759 | 1.5000 | 10.3700 | TunnelPortal_SouthEast/Col_Wall_Left a 11.3137 m | 10.3700 | 0.3423 |

## Gate A : preuve physique des 9 carrefours

Story 5.52 : pose kinematic-v1 ; gonflement de chaque preuve = marge reservee + delta_c + a_e, avec restes de grille publies dans le bloc des residus. a_e = max|o| + epsilon_t = 0.3400 m. Les 24 raccords de l'anneau sont lies au clearance-hash et au sign-off format 3.

Verdict : **vert**.

| Empreinte | Valeur |
|---|---|
| physical-input-hash (9 carrefours) | `c777ce4ebb537362ace165a24d68f844c58a0572eac76e83dcb7bad659afd2d9` |
| Entrees physiques des carrefours classiques (5.51) | `5966669fcbe9a690d1ec3b88ef32e32893e037f675fc5278a3fe346e71f2d863` |
| Entrees physiques des giratoires | `e6cc7aa52cb9d4c7e13bce46281d890c8a000d456c5726bd1692f84a589d803a` |
| semantic-input-hash (Sidewalk, 5.51) | `43e098fcd6809ecc1e8043156f7cd26f57abb73121b844cf6c8badef8b79aef4` |
| clearance-hash (bloc des residus ci-dessous) | `7b7fcbf8f2458e4165ad2adb47e7222b52b12e5754baddbb8a76a277c555f14a` |

### Residus

a_e = 0.34 m ; h = 0.05 m
pose-model = kinematic-v1 ; max|o| = 0 m ; epsilon_t = 0.34 m
marge = 0.25 m ; delta_c = 0.05 m ; h_e = 0.008 rad ; eta = 0.002 rad
reste-candidats = 0.00989820249 m ; reste-degagement = 0.0133512542 m ; parametres-hash = 2043ab22a4d847d8252ca1a2884a960c169514d1a4d95f1beb1a1712c1d19a84
raccords-signes = 24
raccord-signe = 401b55e11b401435eb1bdd8dde7caa94:entry
raccord-signe = 4030253e182e3ed1b7d2aeea7a73feb6:entry
raccord-signe = 419d893b269e14c02e84e3509d9bf193:entry
raccord-signe = 42480748339bbb6fe6fcbc99604c1aa8:exit
raccord-signe = 4357c472225591a18683e67ea2dd5f92:exit
raccord-signe = 43605e569eb08d6ffe62fa7470d59fa0:exit
raccord-signe = 4439e11d9c47c1d09aad97b8f5dd1cbe:exit
raccord-signe = 4469169721b83714f20e63d9fcfff484:exit
raccord-signe = 452ee31e83feea5ebc05406c271424a9:exit
raccord-signe = 453f130c460dc35e052c30714bec6c8e:entry
raccord-signe = 45607ec286d32b63e09ca677f22031ba:entry
raccord-signe = 45d560a7a864362a2f19600802713fac:entry
raccord-signe = 46077471fe6db9c5bfc3327df0b647af:entry
raccord-signe = 464127b42987ee35c9def93cb72dae8c:exit
raccord-signe = 470e78565e75b89add119d1f7bf3d8b3:exit
raccord-signe = 4a5a12c19e62b4f853f928a3d4fb4c96:exit
raccord-signe = 4a6aa7e11135c1ecb2cb26715ca8cab4:entry
raccord-signe = 4a772fed8c8aaeab952d011659612ea7:entry
raccord-signe = 4ac98ed2e41d83c91f0714135aa67ba7:entry
raccord-signe = 4b517add680eba2b77f4e15e9033e180:entry
raccord-signe = 4cec0461fb9fd74541d5772b2264b88f:exit
raccord-signe = 4d6ca77eae0d45e72a542a1478a2aa84:exit
raccord-signe = 4edce9aa0d470704d0479247d72ff2be:entry
raccord-signe = 4f47e1a8140c798681fef66fa633b3b5:exit

| Genre | Carrefour | Trajectoire | Surface Sidewalk | Residu physique (m) | Obstacle temoin | Physique | Residu Sidewalk (m) | Sidewalk |
|---|---|---|---|---:|---|---|---:|---|
| anneau | Roundabout_NorthEast | deux gabarits, anneau physique | - | 0.342351019 | TunnelPortal_NorthEast/Col_Wall_Left | - | - | - |
| anneau | Roundabout_NorthEast | deux gabarits, enveloppe V2 | - | 0.07594445 | - | - | - | - |
| anneau | Roundabout_NorthWest | deux gabarits, anneau physique | - | 0.342350155 | TunnelPortal_NorthWest/Col_Wall_Left | - | - | - |
| anneau | Roundabout_NorthWest | deux gabarits, enveloppe V2 | - | 0.07594487 | - | - | - | - |
| anneau | Roundabout_SouthEast | deux gabarits, anneau physique | - | 0.342349976 | TunnelPortal_SouthEast/Col_Wall_Left | - | - | - |
| anneau | Roundabout_SouthEast | deux gabarits, enveloppe V2 | - | 0.0759442151 | - | - | - | - |
| anneau | Roundabout_SouthWest | deux gabarits, anneau physique | - | 0.342349976 | TunnelPortal_SouthWest/Col_Wall_Left | - | - | - |
| anneau | Roundabout_SouthWest | deux gabarits, enveloppe V2 | - | 0.07594445 | - | - | - | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 0.357428581 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.6726527 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 0.357428581 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.47851783 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.357428581 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.301520616 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.6615837 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 4.68422 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.112792447 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 4.22707129 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.8603742 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 5.124097 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 4.62271261 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.172069013 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 0.448213577 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.684219539 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 0.448213577 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.4785182 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.448213577 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.884275854 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.448213577 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.448213577 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 4.227072 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.112794019 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.661583245 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 4.684219 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 4.62271166 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.8603747 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.172070518 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 5.124098 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 1.5183202 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.6842193 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 1.5183202 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.47851783 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 1.5183202 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.8842768 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 1.5183202 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.448209256 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 0.3015222 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.6726518 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 0.3015222 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.4785182 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.3015222 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.3015222 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 0.3015222 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.4785182 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 0.3015222 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.6726518 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.3015222 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.3015222 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 1.5183202 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.47851783 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 1.5183202 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.6842193 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 1.5183202 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.448209256 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 1.5183202 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.8842768 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 4.684219 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.661583245 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.112794019 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 4.227072 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 5.124098 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.172070518 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.8603747 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.172070518 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 4.62271166 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 0.448213577 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.4785182 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 0.448213577 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.684219539 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.448213577 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.448213577 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.448213577 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.884275854 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 4.22707129 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.112792447 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 4.68422 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.6615837 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.172069013 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 4.62271261 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 5.124097 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.8603742 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.8603742 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 0.357428581 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.47851783 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 0.357428581 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.6726527 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.357428581 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.301520616 | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | - | 3.172758 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Wall_Left | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Merge_Diagonal -> Ring_Split_West | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Merge_South -> Ring_Split_Diagonal | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Merge_West -> Ring_Split_South | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | - | 2.58207965 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Wall_Right | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | - | 5.410715 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | - | 3.17275643 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Wall_Left | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Merge_Diagonal -> Ring_Split_West | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Merge_South -> Ring_Split_Diagonal | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Merge_West -> Ring_Split_South | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | - | 2.58208346 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Wall_Right | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | - | 5.410718 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | - | 3.172758 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Wall_Left | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Merge_Diagonal -> Ring_Split_West | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Merge_South -> Ring_Split_Diagonal | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Merge_West -> Ring_Split_South | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | - | 2.58208418 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Wall_Right | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | - | 5.410715 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | - | 3.17275476 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Wall_Left | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Merge_Diagonal -> Ring_Split_West | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Merge_South -> Ring_Split_Diagonal | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Merge_West -> Ring_Split_South | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | - | 2.58208346 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Wall_Right | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | - | 5.83436537 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | - | 5.410715 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | - | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.112793922 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.22707129 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.28408241 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Right | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.2616307 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.62271166 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.172069982 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.76037097 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.4835338 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.284082234 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North | Infinity | aucun | - | 0.774812639 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 6.261681 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.6726524 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left | 6.261681 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.18480511 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE | 6.261681 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.301520377 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.68422 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.661584 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.24331331 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Right | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.112792991 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.8603743 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.622715 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.0720760152 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.2840667 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.18480511 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE | Infinity | aucun | - | 0.4482088 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW | Infinity | aucun | - | 0.884368241 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 6.07851839 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.4785182 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left | 6.07851839 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.272880584 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW | 6.07851839 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.301523954 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.112793371 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.22707129 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Left | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.261630744 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.28408241 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.62271261 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.172068015 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.76037097 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.284082234 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.4835338 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North | Infinity | aucun | - | 0.774812639 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 6.261695 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.672652066 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right | 6.261695 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.184804648 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE | 6.261695 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.3015204 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.684219 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.661583543 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Left | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.112792984 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.243308 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.8603749 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.622713 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.072075896 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.184804648 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.2840677 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE | Infinity | aucun | - | 0.448212951 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW | Infinity | aucun | - | 0.884368241 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 6.078518 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.47851783 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right | 6.078518 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.272882521 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW | 6.078518 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.301522255 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.22707129 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.112793371 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.28408241 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Right | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.261630744 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.622712 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.172068536 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North | 9.827071 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.76037097 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.4835338 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.284082234 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North | Infinity | aucun | - | 0.774812639 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 6.261695 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.672652066 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left | 6.261695 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.184804648 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE | 6.261695 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.3015201 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.661583543 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.684219 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.243308 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Right | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.112792984 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.8603741 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.62271166 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.072075896 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.2840677 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.184804648 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE | Infinity | aucun | - | 0.4482129 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW | Infinity | aucun | - | 0.884366333 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 6.078518 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.47851783 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left | 6.078518 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.272882521 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW | 6.078518 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.3015218 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.22707129 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.112793922 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Left | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.2616307 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.28408241 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.62271261 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.172069371 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North | 9.82707 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.76037097 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.284082234 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.4835338 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North | Infinity | aucun | - | 0.774812639 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 6.261681 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.6726524 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right | 6.261681 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.18480511 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE | 6.261681 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.3015206 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.661584 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.68422 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Left | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.112792991 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.24331331 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.860375762 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.622715 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North | 10.2748127 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.0720760152 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.18480511 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.2840667 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE | Infinity | aucun | - | 0.448208869 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW | Infinity | aucun | - | 0.884368241 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 6.07851839 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.4785182 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right | 6.07851839 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.272880584 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW | 6.07851839 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.3015235 | - |

### Entrees mesurees

Une ligne par entree de chaque empreinte (chemin, genre, empreinte de sa ligne canonique) : nomme ce qui a change quand une empreinte differe.

| Genre | Entree | Empreinte |
|---|---|---|
| physique | /Greybox_AIVehicle:BoxCollider | `3f771c5e5f63f729` |
| physique | MVP_Run/MVP_RageTargetVehicle_2:BoxCollider | `f6f51ac3369b11c1` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East:BoxCollider | `cf3538d8adcb98a2` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast:BoxCollider | `457a76adada661ec` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest:BoxCollider | `8db33cc29700591d` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West:BoxCollider | `b768651b4c987fb7` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_GroundPlane:BoxCollider | `6f686546f9ec624f` |
| physique | MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est:BoxCollider | `ff4f41ebf5f7dcc5` |
| physique | MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest:BoxCollider | `9a92008879a5a3d7` |
| physique | MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast:BoxCollider | `37114cb7c112d002` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Roadway:BoxCollider | `8c2b12fed85e88da` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left:BoxCollider | `978602ae93e5fd3e` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right:BoxCollider | `272023410e9e435a` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Roadway:BoxCollider | `1846088fd417d14f` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left:BoxCollider | `ce773e578c472a84` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right:BoxCollider | `1ca33b6add2b3d43` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Roadway:BoxCollider | `f8e790d36bd9d115` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left:BoxCollider | `78ebbbc2fcf2d36d` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right:BoxCollider | `458be8f0f02c0180` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Roadway:BoxCollider | `4a8a724ee30c93f9` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left:BoxCollider | `a32e9879bd2df3fd` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right:BoxCollider | `bfd723de8ba4be56` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East:BoxCollider | `4562ca0752562f32` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West:BoxCollider | `1cca88032952c587` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East:BoxCollider | `281a2fda45cfb0aa` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West:BoxCollider | `5fed5c3490f26983` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Roadway:BoxCollider | `25c6b1f0dde7ebe3` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Box_B:BoxCollider | `c4e017eb6c991efc` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Box_Diagonal:BoxCollider | `aeab177c9355986f` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE:BoxCollider | `3f087a39fc8461dc` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Box_B:BoxCollider | `e05a8bfb007129c2` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Box_Diagonal:BoxCollider | `8f1e26c88bad2d8d` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW:BoxCollider | `d8460344126da295` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `c6b37480426473f8` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `b09b94fed848dc40` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `aadbe504650a6924` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `20c9001b3794948b` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `6c0ca09d96636df5` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `5eb5bf9e52eba6ec` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Roadway:BoxCollider | `6049e581316f1330` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left:BoxCollider | `a6819073c84a8681` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Right:BoxCollider | `471fb9d5c6c65d31` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Collision/Col_Roadway:BoxCollider | `12aa4fb4469e5dc7` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left:BoxCollider | `1e323d845d9afc34` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Right:BoxCollider | `c3697ce6bcc03910` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Collision/Col_Roadway:BoxCollider | `8e14fbc359e54a09` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Left:BoxCollider | `4a1dcd097c55da36` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right:BoxCollider | `e0312c926f68b45c` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Roadway:BoxCollider | `5230c3a90707c10e` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Left:BoxCollider | `b1c8f1e278f949c2` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right:BoxCollider | `ca02e827a46a43e4` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Roadway:BoxCollider | `d0d8a7ff7498a26f` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left:BoxCollider | `bfce1b18e054b76f` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Right:BoxCollider | `2e84469f1fd102e6` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Collision/Col_Roadway:BoxCollider | `922f2afbd666f5d0` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left:BoxCollider | `d6b39f6a4ad5bced` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Right:BoxCollider | `930a56f0ec1a79df` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Collision/Col_Roadway:BoxCollider | `1d1ceb7b4928dc4c` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Left:BoxCollider | `93134bde8ee68ca9` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right:BoxCollider | `41e8175852f913c4` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Roadway:BoxCollider | `a4f216eefa1d7461` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Left:BoxCollider | `b750e2f2e0e4ae20` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right:BoxCollider | `4ece12511a68946f` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_NorthEast/Collision/Col_Island:MeshCollider | `7e0704deadbaf1c9` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_NorthEast/Collision/Col_Roadway_Disc:BoxCollider | `cb7c51388ac09952` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_NorthEast/Collision/Col_Roadway_Ring:MeshCollider | `73085376364c6ea0` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_NorthWest/Collision/Col_Island:MeshCollider | `0d82a6809648cf18` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_NorthWest/Collision/Col_Roadway_Disc:BoxCollider | `7bbf4b7fa5b869b5` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_NorthWest/Collision/Col_Roadway_Ring:MeshCollider | `5948fc9325c32efc` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_SouthEast/Collision/Col_Island:MeshCollider | `e88402828c7bb421` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_SouthEast/Collision/Col_Roadway_Disc:BoxCollider | `bf27f03c7a742ea7` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_SouthEast/Collision/Col_Roadway_Ring:MeshCollider | `742322bb3ff37909` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_SouthWest/Collision/Col_Island:MeshCollider | `e57bba589c97b778` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_SouthWest/Collision/Col_Roadway_Disc:BoxCollider | `7962f3835bd9ad66` |
| physique | MVP_Run/RunRoot/LaneGraph/Roundabout_SouthWest/Collision/Col_Roadway_Ring:MeshCollider | `697ea1f8b544a959` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Roadway:BoxCollider | `3cae6d9eb8e7f269` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `e1cbd33f18247fbd` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `a5a31b4df7c6ff91` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `284358394e196738` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `89992faabfac6659` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `1be61e05bd329d8a` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `064bd7dda42fa79e` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North:BoxCollider | `3e0cf0fc60a28f9c` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Roadway:BoxCollider | `fe8f6c8f404e956f` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `4deddd5f3c9fdd0f` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `2fb4f1faf131375a` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `7f0fe3acb9f0a792` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `8e242f7f9f7a801e` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `75b90d6c7f982c52` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `e3579d84a03d22da` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North:BoxCollider | `f13b873d5571cbcd` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Roadway:BoxCollider | `23290d1b7004652b` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `9d1d8026cbca1597` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `656c28424f486b74` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `a4f2147607898174` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `269b454b9c3f0c81` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `1653372124bb8a94` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `1568ea69d00032b1` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North:BoxCollider | `6c04468f0e715707` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Roadway:BoxCollider | `6d34ca7180864e3e` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `c4742faedc8fd244` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `7dcb1945fded959c` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `bde0449bac37059b` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `2bebd5c19303ffc5` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `e71607d270d66150` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `70e3aba8753b1cfa` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North:BoxCollider | `ad7d31ab2b910a31` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Backstop:BoxCollider | `b00711fc2b5d9aca` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Roadway:BoxCollider | `bbd778cecb233206` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Roof:BoxCollider | `da14877c33ffa271` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Sidewalk_Left:BoxCollider | `72bae1e8a3e72da9` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Sidewalk_Right:BoxCollider | `0900c2a2dcf23aad` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Wall_Left:BoxCollider | `dac78de8f52e75a8` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Wall_Right:BoxCollider | `46f913bec6c653b9` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Backstop:BoxCollider | `5ab377727e67f6b1` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Roadway:BoxCollider | `f1808efe55073d87` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Roof:BoxCollider | `25130561d3bd891a` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Sidewalk_Left:BoxCollider | `7e44eaab56ec6a71` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Sidewalk_Right:BoxCollider | `c0582f7feb8c206a` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Wall_Left:BoxCollider | `4af71006d38071cc` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Wall_Right:BoxCollider | `9c3675d4aff37a7b` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Backstop:BoxCollider | `d21a42b4c3491164` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Roadway:BoxCollider | `9deb2cdb9668880c` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Roof:BoxCollider | `20509aefcddd23ef` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Sidewalk_Left:BoxCollider | `a487141dab4c2be2` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Sidewalk_Right:BoxCollider | `4c555b775e1a665f` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Wall_Left:BoxCollider | `bacbbce3405a500a` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Wall_Right:BoxCollider | `d4e6fb6f3482d5db` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Backstop:BoxCollider | `a1cbc9ac0c87259b` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Roadway:BoxCollider | `57af42a08bf661d7` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Roof:BoxCollider | `b5fa819c3b7dd1d6` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Sidewalk_Left:BoxCollider | `47686bd4057fe3ec` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Sidewalk_Right:BoxCollider | `7e409cf39575ba35` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Wall_Left:BoxCollider | `0510ec6df82b90e3` |
| physique | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Wall_Right:BoxCollider | `fbdb39092e9c3af7` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left:BoxCollider | `978602ae93e5fd3e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right:BoxCollider | `272023410e9e435a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `ff11348b4c79dbc2` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `9ac2e2b8417dd291` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `8a8cfcb579f2a7f9` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `13edbc773fceebdb` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `4fa495b3ad2ef0ac` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `04b6757c990e95eb` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `f35373133a44f523` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `bc0e512587d0988c` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left:BoxCollider | `ce773e578c472a84` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right:BoxCollider | `1ca33b6add2b3d43` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `541cc6f9484a8035` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `b16d3c6a39a52650` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `43c6613709c677b2` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `e4d09ecbe361ebc9` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `b4e5f4f9d67840cf` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `274b24d7b198d91b` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `7d0f0fe0caff54c5` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `325a638745b0c78e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left:BoxCollider | `78ebbbc2fcf2d36d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right:BoxCollider | `458be8f0f02c0180` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `8106395ad4e5bc62` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `742e5b4b04d1c961` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `6e517d478d0c7cd6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `06aebce78e5c0742` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `7e6e00ce9abde4a6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `3fb634c5c8190ac4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `88c4dd97d4c2eb63` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `ed66b0661bd83f35` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left:BoxCollider | `a32e9879bd2df3fd` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right:BoxCollider | `bfd723de8ba4be56` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `05040af15dee5f75` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `8c6bf4d98bde27c5` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `0217bea819adf178` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `cc59c560a27a8d7c` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `1452c029ca0e0633` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `ea3a319f36974588` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `1883ac9724da2a48` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `652b60cc5b5ecd76` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Box_B:BoxCollider | `c4e017eb6c991efc` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Box_Diagonal:BoxCollider | `aeab177c9355986f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Visual_0:MeshRenderer | `4ba804ec965d8946` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Visual_1:MeshRenderer | `823ab610bfbdc2c5` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Visual_2:MeshRenderer | `a793e74d3cd83022` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE:BoxCollider | `3f087a39fc8461dc` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Box_B:BoxCollider | `e05a8bfb007129c2` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Box_Diagonal:BoxCollider | `8f1e26c88bad2d8d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Visual_0:MeshRenderer | `af3c692ea6b39fd9` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Visual_1:MeshRenderer | `5430c97ec151891a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Visual_2:MeshRenderer | `de23e850d5bed21f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW:BoxCollider | `d8460344126da295` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `c6b37480426473f8` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `b09b94fed848dc40` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `6ec8936b11aa1878` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `66b5db4123012ee0` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `cb1fe72ab228a501` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `aadbe504650a6924` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `20c9001b3794948b` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `6c0ca09d96636df5` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `678bdeedba318fed` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `5518e1a1a48bf31d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `bc96645b99568b75` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `5eb5bf9e52eba6ec` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left:BoxCollider | `a6819073c84a8681` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Right:BoxCollider | `471fb9d5c6c65d31` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `a19e02c79334cd02` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `95326d85978cd15e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `d51baf84c10419e8` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `c0ff1fea3cfbfba1` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `23c54ebc3c9184f2` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `6dc1a5dc74b3c711` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left:BoxCollider | `1e323d845d9afc34` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Right:BoxCollider | `c3697ce6bcc03910` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `d9a96388eca06e39` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `21d1f70930e7b013` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `b0a54e975677b739` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `beed4e57e85b59c3` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `5d6f80dfbd5b9cef` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `45b6c539ef53acaf` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Left:BoxCollider | `4a1dcd097c55da36` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right:BoxCollider | `e0312c926f68b45c` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `635958c8d1411956` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `763d66f635e849a0` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `d13c5004a2c7f2f0` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `b42780f23d3bbc0a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `2017ab5b8b72ba21` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `be4b67ead0a3131b` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Left:BoxCollider | `b1c8f1e278f949c2` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right:BoxCollider | `ca02e827a46a43e4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `288339788f4c1662` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `b1aba66ecad14c50` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `29b57b1465d683b1` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `139c3a8375a4f6d6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `8f4ff9d40653a59e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `6b0520f51dac86fc` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left:BoxCollider | `bfce1b18e054b76f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Right:BoxCollider | `2e84469f1fd102e6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `0b349470b272ad91` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `664e5b3059e5008a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `46fe153705ace85d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `c5d50b13e0b68e92` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `efdd60878728c14f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `87c33136e8527977` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left:BoxCollider | `d6b39f6a4ad5bced` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Right:BoxCollider | `930a56f0ec1a79df` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `e16b00f62163c020` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `2f809d1008b52b83` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `f7ba7d59ba6139f4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `9a7dcb1351948cf7` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `6102d93823abbe32` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `4663708631da476a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Left:BoxCollider | `93134bde8ee68ca9` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right:BoxCollider | `41e8175852f913c4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `66408f2e685a06f9` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `3cf0787abe80276d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `563d0864de212a2d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `cee84237ecfd48ab` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `c09016fce052ab50` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `b311d2eead6f8e22` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Left:BoxCollider | `b750e2f2e0e4ae20` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right:BoxCollider | `4ece12511a68946f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `9e7c1ffdae47dd8a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `eb2c42ac3d43352b` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `275ee32e919c41bf` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `f11d3fa66b81a5a4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `4fa211c249936e16` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `1133c56ee3407883` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `e1cbd33f18247fbd` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `a5a31b4df7c6ff91` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `df14bee466881d69` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `77459639f30983d4` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `62f53b9a73a8a276` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `284358394e196738` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `89992faabfac6659` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `1be61e05bd329d8a` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `878c66978878f5c3` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `dd2f90b9878706ca` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `64adbb3502f23c32` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `064bd7dda42fa79e` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North:BoxCollider | `3e0cf0fc60a28f9c` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Visual_Greybox_TJunction/Sidewalk_North_n2:MeshRenderer | `4a9db2abf1a1b276` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Visual_Greybox_TJunction/Sidewalk_North_n6:MeshRenderer | `3242dd8da80c5636` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Visual_Greybox_TJunction/Sidewalk_North_s2:MeshRenderer | `34575d1c79a257f8` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Visual_Greybox_TJunction/Sidewalk_North_s6:MeshRenderer | `e8733299d319bfca` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `4deddd5f3c9fdd0f` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `2fb4f1faf131375a` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `37334b5e9b00df09` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `e19033eb7e3161cc` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `ba1e2d396cf2bb65` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `7f0fe3acb9f0a792` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `8e242f7f9f7a801e` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `75b90d6c7f982c52` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `7eb1132697e23009` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `12ab5d2205ec0322` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `d52baa70c6958617` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `e3579d84a03d22da` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North:BoxCollider | `f13b873d5571cbcd` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Visual_Greybox_TJunction/Sidewalk_North_n2:MeshRenderer | `78e3317aba326118` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Visual_Greybox_TJunction/Sidewalk_North_n6:MeshRenderer | `39b3effffcec8129` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Visual_Greybox_TJunction/Sidewalk_North_s2:MeshRenderer | `5b282ac8958280fc` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Visual_Greybox_TJunction/Sidewalk_North_s6:MeshRenderer | `865fedaec04bba54` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `9d1d8026cbca1597` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `656c28424f486b74` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `855b964bebbb995e` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `dc91d5ba8e471b7e` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `15441d604243b88e` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `a4f2147607898174` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `269b454b9c3f0c81` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `1653372124bb8a94` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `d9854e6e50e152ca` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `fec0fac9de5c995d` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `c6012157afee15d9` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `1568ea69d00032b1` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North:BoxCollider | `6c04468f0e715707` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Visual_Greybox_TJunction/Sidewalk_North_n2:MeshRenderer | `9eb1a48aecc58319` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Visual_Greybox_TJunction/Sidewalk_North_n6:MeshRenderer | `4a31676e15426236` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Visual_Greybox_TJunction/Sidewalk_North_s2:MeshRenderer | `1616ca6ed06cb0de` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Visual_Greybox_TJunction/Sidewalk_North_s6:MeshRenderer | `ec2d8d7ad4528ae2` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `c4742faedc8fd244` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `7dcb1945fded959c` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `0eea2eaac5871b1b` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `86edb88b32b050dd` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `9bdc5dd6b1bae114` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `bde0449bac37059b` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `2bebd5c19303ffc5` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `e71607d270d66150` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `893a6826bdd75b7d` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `2fe5fb57e22bc4ff` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `8595a771ec902864` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `70e3aba8753b1cfa` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North:BoxCollider | `ad7d31ab2b910a31` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Visual_Greybox_TJunction/Sidewalk_North_n2:MeshRenderer | `975f8baab75a52ce` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Visual_Greybox_TJunction/Sidewalk_North_n6:MeshRenderer | `1ed4f2f0f1da6510` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Visual_Greybox_TJunction/Sidewalk_North_s2:MeshRenderer | `47cd46cfdf89c48d` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Visual_Greybox_TJunction/Sidewalk_North_s6:MeshRenderer | `accdf755af680168` |

## Disposition des taches 5.27

Chaque tache est disposee exactement une fois. Controle, Conflit et Largeur par leurs donnees typees ; les autres par une disposition explicite.

| Categorie | Sujet | Disposition | Detail |
|---|---|---|---|
| Conflit | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Conflits | 12 candidat(s) decide(s), 10 zone(s) materialisee(s) |
| Conflit | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Conflits | 12 candidat(s) decide(s), 10 zone(s) materialisee(s) |
| Conflit | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Conflits | 12 candidat(s) decide(s), 10 zone(s) materialisee(s) |
| Conflit | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Conflits | 12 candidat(s) decide(s), 10 zone(s) materialisee(s) |
| Conflit | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Conflits | 48 candidat(s) decide(s), 42 zone(s) materialisee(s) |
| Conflit | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Conflits | 10 candidat(s) decide(s), 8 zone(s) materialisee(s) |
| Conflit | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Conflits | 10 candidat(s) decide(s), 8 zone(s) materialisee(s) |
| Conflit | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Conflits | 10 candidat(s) decide(s), 8 zone(s) materialisee(s) |
| Conflit | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Conflits | 10 candidat(s) decide(s), 8 zone(s) materialisee(s) |
| Controle | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Controles | 3 controles (Priority, Yield), un par approche |
| Controle | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Controles | 3 controles (Priority, Yield), un par approche |
| Controle | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Controles | 3 controles (Priority, Yield), un par approche |
| Controle | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Controles | 3 controles (Priority, Yield), un par approche |
| Controle | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Controles | 4 controles (Uncontrolled), un par approche |
| Controle | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Controles | 6 controles (Priority, Yield), un par approche |
| Controle | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Controles | 6 controles (Priority, Yield), un par approche |
| Controle | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Controles | 6 controles (Priority, Yield), un par approche |
| Controle | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Controles | 6 controles (Priority, Yield), un par approche |
| Frontiere | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Largeur | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), EndpointInterpolation |
| Largeur | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), EndpointInterpolation |
| Largeur | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), EndpointInterpolation |
| Largeur | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), EndpointInterpolation |
| Largeur | `4730190af6f78f0cd48d4bb005c9f091` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4dab8dc01f01a819f72ef4dc8a8a459d` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `49ecb9240fd46b56a68f2ba5f9b575aa` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4b5389d91d5915f3196d95d243bc789f` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4b8232a70eb5574526502f1072cb50bd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1045154302 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `443ed3bc9f5371ef4f1439781cd012a8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1574439523 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `46c85a9afac17bde2153665adab63fbc` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-234956567 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `42de5da740b31ac0176161c776f3d4a1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-75338410 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `47b9c1ab789b84066b536acecec183ab` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1227312198 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4ad222fbe3e52d2a5b6f86b332946c80` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1335115730 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4c56cbc620e585a5373ea0ea9acae384` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1375911139 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `46f7f22596a1e5204781d18a86e839ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1461432457 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `48b41f08eb1eee61a4e6281a4e963893` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1516681040 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4b08788f2b3434539d6daf47c410c6a4` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1812162174 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `410b074af81719b5992afb35a5d20cbd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-235211969 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4f545c3fada86d11df7692a0081aa1a5` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-531979442 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4261bcbef0b8a38b712c649286b082ba` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-539479367 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `488cff3f9501412fcae92bf829779984` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-656642079 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4581e2ae94d3287b91ad57e31a9fe989` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-972045385 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `431aab2ccfb1de591d13a38baa04b796` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-981631451 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `44687593778b414acca9ab06ece502b1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4a53609938eef355087249c1b74ca6b0` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4815e26dcb7e3aa355dfa72569096986` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4796d8afab7aafe3c213381ce7be51b8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4640b60be5be840f1fa778f300b64d84` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4b33ebfdfb0a4ef2c2c6cbb267a5f2be` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `4de2f43948bbec45fce0fbe76d4658ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Largeur | `42134703636fbc25221f6dc2ca06c793` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | Largeur revue | 4.3000 / 4.3000 m (gauche / droite), Uniform |
| Ligne | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | AuthoredOnControls | Ligne authoree sur chaque controle Stop ou Yield du carrefour. |
| Ligne | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | AuthoredOnControls | Ligne authoree sur chaque controle Stop ou Yield du carrefour. |
| Ligne | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | AuthoredOnControls | Ligne authoree sur chaque controle Stop ou Yield du carrefour. |
| Ligne | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | AuthoredOnControls | Ligne authoree sur chaque controle Stop ou Yield du carrefour. |
| Ligne | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | NotRequiredForCurrentControlKind | Aucun controle Stop ni Yield : aucune ligne. |
| Ligne | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | GenericEntryFallback | Aucune ligne dans la map pour ces controles Yield ou Stop : repli explicite sur l'entree generique du mouvement (s = 0). |
| Ligne | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | GenericEntryFallback | Aucune ligne dans la map pour ces controles Yield ou Stop : repli explicite sur l'entree generique du mouvement (s = 0). |
| Ligne | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | GenericEntryFallback | Aucune ligne dans la map pour ces controles Yield ou Stop : repli explicite sur l'entree generique du mouvement (s = 0). |
| Ligne | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | GenericEntryFallback | Aucune ligne dans la map pour ces controles Yield ou Stop : repli explicite sur l'entree generique du mouvement (s = 0). |
| Portail | `49d29d3fd771ae4966aca755b3442cbc` portal:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4902253723728314459-1045154302:Exit | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Portail | `436d9a1a58c93bd19cdf0014de309f98` portal:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4902253723728314459-1574439523:Exit | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Portail | `48090a5d359625c04fb6050a2407f292` portal:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4902253723728314459-234956567:Exit | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Portail | `4dcf9641f7a714b8bc785696d1709798` portal:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4902253723728314459-75338410:Exit | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Portail | `4889b9d5d0f80bc3ce3c3514a5d6f7aa` portal:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-1045154302:Entry | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Portail | `4ce14124d3729e5b3ad1174c8653a99d` portal:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-1574439523:Entry | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Portail | `4882b42dcf37410f4f629825c30f5f99` portal:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-234956567:Entry | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Portail | `48d96a31a823c14dc3c3c55e08da6bbb` portal:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-75338410:Entry | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Section | `4730190af6f78f0cd48d4bb005c9f091` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4dab8dc01f01a819f72ef4dc8a8a459d` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `49ecb9240fd46b56a68f2ba5f9b575aa` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4b5389d91d5915f3196d95d243bc789f` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4b8232a70eb5574526502f1072cb50bd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1045154302 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `443ed3bc9f5371ef4f1439781cd012a8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1574439523 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `46c85a9afac17bde2153665adab63fbc` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-234956567 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `42de5da740b31ac0176161c776f3d4a1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-75338410 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `47b9c1ab789b84066b536acecec183ab` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1227312198 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4ad222fbe3e52d2a5b6f86b332946c80` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1335115730 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4c56cbc620e585a5373ea0ea9acae384` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1375911139 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `46f7f22596a1e5204781d18a86e839ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1461432457 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `48b41f08eb1eee61a4e6281a4e963893` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1516681040 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4b08788f2b3434539d6daf47c410c6a4` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1812162174 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `410b074af81719b5992afb35a5d20cbd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-235211969 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4f545c3fada86d11df7692a0081aa1a5` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-531979442 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4261bcbef0b8a38b712c649286b082ba` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-539479367 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `488cff3f9501412fcae92bf829779984` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-656642079 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4581e2ae94d3287b91ad57e31a9fe989` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-972045385 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `431aab2ccfb1de591d13a38baa04b796` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-981631451 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `44687593778b414acca9ab06ece502b1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4a53609938eef355087249c1b74ca6b0` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4815e26dcb7e3aa355dfa72569096986` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4796d8afab7aafe3c213381ce7be51b8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4640b60be5be840f1fa778f300b64d84` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4b33ebfdfb0a4ef2c2c6cbb267a5f2be` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `4de2f43948bbec45fce0fbe76d4658ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Section | `42134703636fbc25221f6dc2ca06c793` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | Deferred | Vitesse, classes et surface differees : voir DeferredFields, aucune valeur inventee. |
| Signal | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |
| Signal | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |
| Signal | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |
| Signal | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |
| Signal | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |
| Signal | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |
| Signal | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |
| Signal | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |
| Signal | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Unsignalized | Carrefour declare non signalise : aucun SignalPlan, aucun vert implicite. |

## Champs differes

Aucune valeur inventee : les sections gardent 0 m/s, aucune classe et le defaut d'enum `Asphalt` jusqu'a leur reouverture.

| Champ | Reouverture |
|---|---|
| AllowedVehicleClasses | Admission d'une seconde classe de vehicule : recompilation (AD-44). |
| SpeedLimit | Story 5.33 (vitesse limite authoree). |
| Surface | Introduction d'une seconde surface roulable. |

## Fixtures de localisation (carte reelle)

Derivees structurellement, jamais par nom : corridor du portail d'entree de plus petit `RoadId`, approche de plus petit `RoadId` a au moins deux mouvements. Empreinte = gabarit max du profil.

| Fixture | Attendu | Observe | Verdict |
|---|---|---|---|
| nominal | localise sur le corridor du portail, aucun drapeau | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=None, lateral=0.0000 m, confiance=1.0000 | vert |
| frontiere | precedent retenu un quart d'hysteresis apres sa fin, OutsideEnvelope, pas de contresens | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, Ambiguous, lateral=0.0000 m, confiance=0.1667 | vert |
| deplace | reference dans l'enveloppe, empreinte debordante : meme corridor, lateral signe, OutsideEnvelope | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, lateral=1.4850 m, confiance=1.0000 | vert |
| contresens | cap oppose sur sa voie : meme corridor, WrongWay | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=WrongWay, lateral=0.0000 m, confiance=1.0000 | vert |
| carrefour ambigu | debut des mouvements divergents d'une approche : un mouvement de cette approche, Ambiguous, confiance < 1 | localise=oui, JunctionMovement `4030253e182e3ed1b7d2aeea7a73feb6`, drapeaux=Ambiguous, lateral=0.0000 m, confiance=0.0367 | vert |
| hors corridor | reference hors enveloppe dans le seuil d'acceptation : reste localisee, OutsideEnvelope, sans snap | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, lateral=3.2500 m, confiance=1.0000 | vert |

## Overlay et Gate A

Overlay canonique : `_bmad-output/implementation-artifacts/overlay-5-28-mvp-run.txt` (25 instances de module, hash `bc049fa7a3f0fc92a85cad386a12ced4ce5652659f3b09e2a12c6f0739508df2`), produit par la meme fonction que le dessin de la fenetre `RoadRage/Traffic V2/Revue Gate A`.

La Gate A n'est ouverte que par `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-signoff.json`, ecrit par le proprietaire depuis cette fenetre apres revue des 25 instances, et lie aux hashes source, lignee, decisions, compilateur, modele, version et overlay d'un pipeline frais, ainsi qu'aux empreintes physique et Sidewalk et aux residus de la preuve physique (comparaison exacte). Un sign-off absent ou perime, une empreinte ou un residu different, un residu non positif ou une decision non reconfirmee garde la Gate A fermee, jamais repare.

| Instance | Genre |
|---|---|
| TunnelPortal_SouthWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1045154302` | TunnelPortal |
| TunnelPortal_NorthEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1574439523` | TunnelPortal |
| TunnelPortal_NorthWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-234956567` | TunnelPortal |
| TunnelPortal_SouthEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-75338410` | TunnelPortal |
| TJunction_East `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953` | TJunction |
| TJunction_West `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037` | TJunction |
| TJunction_South `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690` | TJunction |
| TJunction_North `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468` | TJunction |
| Intersection_Center_Crossroads `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282` | Crossroads |
| Ring_North_West `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1227312198` | RoadSegment |
| Ring_East_North `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1335115730` | RoadSegment |
| Ring_East_South `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1375911139` | RoadSegment |
| Avenue_CenterToNorth `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1461432457` | RoadSegment |
| Ring_West_South `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1516681040` | RoadSegment |
| Avenue_CenterToSouth `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1812162174` | RoadSegment |
| Ring_South_East `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-235211969` | RoadSegment |
| Avenue_CenterToEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-531979442` | RoadSegment |
| Ring_South_West `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-539479367` | RoadSegment |
| Ring_North_East `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-656642079` | RoadSegment |
| Avenue_CenterToWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-972045385` | RoadSegment |
| Ring_West_North `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-981631451` | RoadSegment |
| Roundabout_SouthWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495` | Roundabout |
| Roundabout_NorthWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255` | Roundabout |
| Roundabout_NorthEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718` | Roundabout |
| Roundabout_SouthEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077` | Roundabout |

