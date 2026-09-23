<!-- rrs-gate-a-binding
source-hash: b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc
importer-version: 1
compiler-schema-version: 4
pipeline-version: 1
model-id: 419bd12ec9b5fe710e8a3719692c7982
lineage-hash: f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4
decisions-hash: 5b6bd7e90c245d6592034d0548ef3f683c36ee4b479579825c4e0862f36ed061
model-hash: 63f4bd8ad0c9f29f883dc806aa9ff74fba1e401ca1ba288dcd7fe7df9167afe2
road-model-version: v4:bc477eb562d7946c977c13672cab39df
overlay-hash: 25089c9d1d80b1bc189deeb27405194931932831f822fd8c12d187c9475c5cd1
body-hash: ad8d42548c813de2861d9e4a82eb1abcc43b0a2901de15bf52e9c1c68157f606
-->
# Rapport Gate A : modele authore MVP_Run (Story 5.28)

Genere par le menu `RoadRage/Traffic V2/Compiler le modele authore`. Ne pas editer : un rapport retouche ou detache de ses entrees est rejete, jamais repare.

## Liaison

| Champ | Valeur |
|---|---|
| Hash de la source V1 extraite | `b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc` |
| Version de l'importeur / du pipeline | 1 / 1 |
| CompilerSchemaVersion | 4 |
| RoadModelId | `419bd12ec9b5fe710e8a3719692c7982` |
| Hash de la lignee | `f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-lineage.json`) |
| Hash des decisions | `5b6bd7e90c245d6592034d0548ef3f683c36ee4b479579825c4e0862f36ed061` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json`) |
| Hash du modele persiste | `63f4bd8ad0c9f29f883dc806aa9ff74fba1e401ca1ba288dcd7fe7df9167afe2` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json`) |
| RoadModelVersion | `v4:bc477eb562d7946c977c13672cab39df` |
| Hash de l'overlay | `25089c9d1d80b1bc189deeb27405194931932831f822fd8c12d187c9475c5cd1` (`_bmad-output/implementation-artifacts/overlay-5-28-mvp-run.txt`) |

## Resultat

- Erreurs dures : **0** (le modele passe `Compile` ; une seule erreur aurait bloque toute ecriture).
- Lignee inchangee : 0 identite frappee, 0 retiree (import 5.27 relance en lecture seule).
- Taches 5.27 non disposees : **0** sur 118.
- Fixtures de localisation : 6 vertes sur 6.

## Modele authore

| Enregistrement | Nombre |
|---|---:|
| JunctionMovement | 72 |
| JunctionControl | 40 (un par approche, tous `Uncontrolled`) |
| ConflictZone | 76 (decisions acceptees seulement) |
| SignalPlan | 0 (carrefours declares non signalises) |
| LaneAdjacency | 0 (aucune adjacence authoree) |
| Ligne d'arret | 0 (aucune ligne sous `Uncontrolled`) |

## Controles par approche

Un `JunctionControl` par corridor d'approche, lie a tous les mouvements partant de cette approche ; seul genre admis : `Uncontrolled` (decision du proprietaire, 2026-09-23). Chaque mouvement a exactement un controle.

| Carrefour | Controle | Genre | Approche | Mouvements |
|---|---|---|---|---:|
| Roundabout_NorthWest | `42f3d162d75e244896863e1711a77287` | Uncontrolled | `4ec40e5f82f7a65bed1dc3d9679f8693` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | 2 |
| Roundabout_NorthWest | `4332ef31bc230cb1e928d17c87c2b2a4` | Uncontrolled | `43e96165bde1e8229cf8d27082d6b798` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | 2 |
| Roundabout_NorthWest | `4538b544fa7443645e9f0b55dfd20c81` | Uncontrolled | `4d5c15fca8e24f15b1bdf0f0695acebf` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-981631451>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-981631451 | 1 |
| Roundabout_NorthWest | `48d729046272166df666b735d860ff85` | Uncontrolled | `4ccc97c5021ae91587883344ee3ca299` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | 2 |
| Roundabout_NorthWest | `4e6af9f2c3ccc0a6b1f0ef739679ccac` | Uncontrolled | `4516fd525d5bf0205cb47b0093ab0794` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1227312198>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1227312198 | 1 |
| Roundabout_NorthWest | `4fcbd57e6baa66a28b0732836bc2e880` | Uncontrolled | `4f3543b1218b82af65b5b8fc58457fb3` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-234956567>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2050073441738459335-234956567 | 1 |
| Roundabout_NorthEast | `4258e4997d604bdaddd64f90730258ae` | Uncontrolled | `4ae5e1290d8b815bcb8f509e915fef89` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1335115730>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1335115730 | 1 |
| Roundabout_NorthEast | `43fe03f1814155e96d6d1d49b9f2ffa4` | Uncontrolled | `455e4360eac83d9900ad7ea698758890` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-656642079>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-656642079 | 1 |
| Roundabout_NorthEast | `469dd39290081f19c5f45716e14fa988` | Uncontrolled | `4c3fbf9d31b3efe44c908138001098b8` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | 2 |
| Roundabout_NorthEast | `4b8f1772f907da8ab8daecc656395b8b` | Uncontrolled | `4feeebdeeefd1a0f872a7e456b3a5287` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-1574439523>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2050073441738459335-1574439523 | 1 |
| Roundabout_NorthEast | `4df5972c8018084b60d1e41cee091db6` | Uncontrolled | `432025ce90895a6728955c79e3edfb9e` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | 2 |
| Roundabout_NorthEast | `4e578eb71bb93361716c13717c771faa` | Uncontrolled | `4e67d7a19f6083c020b14cda38733f93` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | 2 |
| TJunction_West | `48171f4957d3bd8970fed52ff8b47a83` | Uncontrolled | `4f56ef8aae6bfffcbc337d463dd2fd9f` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-972045385>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-972045385 | 2 |
| TJunction_West | `4c4d0695610e00bb74583d4b6163728a` | Uncontrolled | `46b219bca5cdaf1087d30d9622fab0a5` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1516681040>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1516681040 | 2 |
| TJunction_West | `4df2f2bab0514361bd8aaa81b0e0049f` | Uncontrolled | `4db6ebc158e0dff4369373f117f51690` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-981631451>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-981631451 | 2 |
| Intersection_Center_Crossroads | `413075398038b1e09d306528753f6b9e` | Uncontrolled | `45daa67f26d456b653f543a16ea6dba5` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-531979442>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-531979442 | 3 |
| Intersection_Center_Crossroads | `4575f7c642af9766e86aebffaec3cfb3` | Uncontrolled | `44d95c53b9c058de55da54065eed7882` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1812162174>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1812162174 | 3 |
| Intersection_Center_Crossroads | `48bec7b12bfc173be79c7ff6dc888e82` | Uncontrolled | `48c833c71f68abef819cb5a36c99208f` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-972045385>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-972045385 | 3 |
| Intersection_Center_Crossroads | `4b0592bd1e8ddf114991c13fb4632285` | Uncontrolled | `4680aaa6ee1678ef03909266658047b6` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1461432457>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1461432457 | 3 |
| Roundabout_SouthEast | `44924805a30ee7ba97a11b1d9b92919e` | Uncontrolled | `4f8c2539b5152e69c8fe4d64939f96b9` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-235211969>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-235211969 | 1 |
| Roundabout_SouthEast | `480071f81c9ce0ded1b93c6a043d7787` | Uncontrolled | `41d3971913a0bc729dacb328aaa17495` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | 2 |
| Roundabout_SouthEast | `481a5e58c50d4c4a68188022a32041a9` | Uncontrolled | `4e878e4befb474cfbdc785346bd6f6a4` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | 2 |
| Roundabout_SouthEast | `4997c37d1df9457fd9b6d871b5d9f580` | Uncontrolled | `4eb54bbcb45c5ab91993e545746f24a9` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1375911139>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1375911139 | 1 |
| Roundabout_SouthEast | `4b9dc3fe78d50ce0ed024b340d1a5c9c` | Uncontrolled | `4403c568617df5ea8f9a2fdf743748ad` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-75338410>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2050073441738459335-75338410 | 1 |
| Roundabout_SouthEast | `4cce28bd136319d98afaa9f310a74dbe` | Uncontrolled | `4b5540b59d5cd5141bb4bb279d10fb96` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | 2 |
| TJunction_North | `494595a32d7821f3022e012e6e6b6a94` | Uncontrolled | `47de8d1a8e71016a20b301b8a86852a5` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1227312198>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1227312198 | 2 |
| TJunction_North | `4c44e8caec8bc2c8291581b575127a8d` | Uncontrolled | `4d56a92042f124db7947938fefa7c7a7` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1461432457>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1461432457 | 2 |
| TJunction_North | `4f0d2476b2d1a607c40bf041509af48e` | Uncontrolled | `4bc86d56e68c64ed3454e51566dd43bc` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-656642079>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-656642079 | 2 |
| TJunction_South | `456fc7aec92eae3a031351a23bee3fa7` | Uncontrolled | `49980393f36fca422e64472c8ccc009c` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-235211969>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-235211969 | 2 |
| TJunction_South | `4ce3fd28fd4195f1d5cbe44f4e66fb87` | Uncontrolled | `4f4e0f3e7e8b2b6b059a8a6edab2bc90` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-539479367>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-539479367 | 2 |
| TJunction_South | `4fa7a31c8eaaa03cd839e636079b349e` | Uncontrolled | `42ad3d5187bfa4104bf4917b93ede39e` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1812162174>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1812162174 | 2 |
| Roundabout_SouthWest | `4125ec30a583176d1fbb06c776fa858f` | Uncontrolled | `433f39ba8eeb012212b7d5a05468b09d` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-539479367>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-539479367 | 1 |
| Roundabout_SouthWest | `45e4596d6aa585a2517bee9c8fb834be` | Uncontrolled | `4d1d749db78465cdc4e90ee844e3d898` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8133778351431591841-1045154302>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2050073441738459335-1045154302 | 1 |
| Roundabout_SouthWest | `47ef2473ecfa0206cc2301cc291f7886` | Uncontrolled | `41aade042322996080b638a3a86e1088` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | 2 |
| Roundabout_SouthWest | `4aeddcba2f3223edb3ec477c2638bea8` | Uncontrolled | `427041528b28cda9499c4ab5aafe0d9c` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | 2 |
| Roundabout_SouthWest | `4cf54317710e5723de29224b473fd89f` | Uncontrolled | `40e937a99618cac3ce12d56586514283` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1516681040>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1516681040 | 1 |
| Roundabout_SouthWest | `4d6b68fe5f6aaa73f9d436e8c46044a2` | Uncontrolled | `48e681e799268186449903ef2aa3258f` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | 2 |
| TJunction_East | `4370991f7408a41ba87432f030daf299` | Uncontrolled | `43cf13ae1ef6b701bbcd906cdea7df90` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-531979442>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-531979442 | 2 |
| TJunction_East | `4b5be59852dbf8c943063c130a89abbe` | Uncontrolled | `4bc735b758e931a2e6a2faa6a7cf198b` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3577401544019279690-1375911139>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-415181062651573262-1375911139 | 2 |
| TJunction_East | `4e342ab49e0487a48fc599a311150694` | Uncontrolled | `4970addfd3bf6360c8191bf0f3eeafb7` corridor:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-6244533751477241117-1335115730>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-4269744981664112333-1335115730 | 2 |

## Candidats de conflit et decisions

Candidat = paire de mouvements du meme carrefour, d'approches differentes, dont les enveloppes balayees se recoupent (croisement ou convergence). Rayon balaye r = demi-gabarit max 1.0300 m + marge 0.2500 m = 1.2800 m (profil versionne) ; courbes densifiees a pas r/4, recoupement a moins de 2r. Genere hors ligne sur le modele compile sans zones ; seules les decisions acceptees deviennent des `ConflictZone`, aucun consommateur n'infere de conflit.

| Carrefour | Candidats | Acceptes | Rejetes | Paires de meme approche (suivi, pas conflit) |
|---|---:|---:|---:|---:|
| Roundabout_NorthWest | 6 | 6 | 0 | 3 |
| Roundabout_NorthEast | 6 | 6 | 0 | 3 |
| TJunction_West | 6 | 6 | 0 | 3 |
| Intersection_Center_Crossroads | 28 | 28 | 0 | 12 |
| Roundabout_SouthEast | 6 | 6 | 0 | 3 |
| TJunction_North | 6 | 6 | 0 | 3 |
| TJunction_South | 6 | 6 | 0 | 3 |
| Roundabout_SouthWest | 6 | 6 | 0 | 3 |
| TJunction_East | 6 | 6 | 0 | 3 |

| Carrefour | Mouvement A | Mouvement B | Decision | Zone | Motif |
|---|---|---|---|---|---|
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `405ba721192ad202e99c9ba49b5e9fb7` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthWest | Roundabout_NorthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `47114f641089fae85c0ee8305935fdaa` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthWest | Roundabout_NorthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `40b126f3a984fa21883e88b3f099319c` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `45c7253beaef1bef98463cee884919a4` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `43a738a3e1cbfb568fadc5b3548a1b8f` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `43e5a24dac93246333d9faf399592587` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `415d14c83ddadbaca56eba4406bcf2b8` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `41ddaaa21dd44070074b0aa5e4581880` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthEast | Roundabout_NorthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `4ef00a041311cff1569a33b5a476aa88` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthEast | Roundabout_NorthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `44434fa7a74b378cd42ed08c42fe8692` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `40e3e192ec85c0457be62034d55155a4` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `49b398688f7efc5feae64cccd10773ac` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4258af5419bba1365a3f0ad6ed3d44aa` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `40ce606892963a8943f86c2ecd963ba2` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `470bc18b1824ae1e7e47ac80c41aca9c` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4b786539f4809cd080c6de032000a3bd` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4fbcc8e5f354d981b2a3580b4645f6b7` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `406690a88e36b4596ca05b5832a555a9` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4fade05dfa8725d21db5702002aa4ca3` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4c31ad754ca8b25911fbb0f12ca04f90` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4b168164d6d552a9e382264821c7dcbe` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4aeb5f7d500459bab9a69a74b6996994` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `41143c56ca46d5c88f92b78870868c87` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `46987262dd6fe1111a00229ab6c9d8ab` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `421fede006aa2f42e5a6fd0002a6d696` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Accepted | `490e9e2ea4bef88bf4a8ee14433a429b` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4ac63be5d9fb0655462900ee2188a088` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4645efe76bbc8c5cda09e53a759b85b9` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `42665e8c1aa6739044cecc21fb9f79be` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47c31dcb178dc8692f142c61851d9cb7` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4cb4ca66e2b91ace8ba4ce3150bf3ebb` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `49c2d7c1095d014dcd2e9707187a5d9e` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4d99e930a4f47c2ed260aed77cac1f8d` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4987ba61c57809e24605d2648053e29e` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `49a2893231955267ed28fd25ded38ba0` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `45f30575f87cd3d65d78177ee3c66d85` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Accepted | `45a0180046cf5cfca98979d7fcd8828a` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `488452c23f729a6d1d1f9aa582c2bf8f` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `452cc0b3f8ae471194e7907f1351d3ad` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4065d99bf14d8ee4c3e42099de20fa89` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `44ef4e39ca0ade7338fc8feec0f429be` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `49340dcf8a92884fb95006b2ec095381` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4c9b462c1b8587eb241406b3a9d11683` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4b4cb6b3d76549de0935338e2b66aea9` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4a6c823b85b757d2f3505c8789aa528c` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4c20416c36051baa4edfb294cd6e169b` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `4e24e297c4d1bd9139c77a07f6177abb` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_SouthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Accepted | `48fd616bb1789845fc2043ad1f0ba7a5` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `45347e3ef39bcc64fdfccaf89295a4bd` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `406efc13fc2d5de520cbe34197314db3` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `4e2ebb271a55553f65fe7bc0087b7482` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Roundabout_SouthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Accepted | `4fa896603af971df28394d41512e83b8` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4b91ce1faf5f47a051923e55db19d084` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4782b2025438431bd3b4c6d8fedfe99b` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47d2bcbf20f128f9b2cf7d5f8dee60b9` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4875672f99ccdb811ea74f51b86d298f` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4e90907066432b37ea55bf8b6c7967b3` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4e982b0fa19ae9f5ee3c0bc424e8ac91` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `46c0852ee863ff838d59392162782297` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4fce84fd1a162287b7b06ed08526d094` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4651661907bde7ddf496e0b38b1a67b2` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4a44f9710a830e7f77c6952d114c4fbf` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `46fef049de32bffeccdcc752588447a5` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4c147a4d6d6d6cd2fde23d045167c692` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `450099ba966907d2e411d1b20b54ae99` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `4a4250039f40a26ff5b4051a197c149d` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `42b83e7c01b9cbacc9919a1bb7e9e68c` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `451483d7154f55215bd3214ab24e3d98` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `49b9fb8a95b566317bd90511c5724b82` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_SouthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Accepted | `4df53fca19b7ee5211d4e512b3a4da86` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47c7fee4039145473209b7b5d87472b3` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `433b2a4fbf3fe4c8557ecd7b832e4fb1` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4a435f4e82a6a473eee3d534a3378fb9` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `468af52ce1bbea22b1482f15af632390` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `48dacd01988ac15ff1cf37499497f885` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |
| TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `43bbfeff3c02c3bf890cd95139e295a3` | Enveloppes balayees d'approches differentes en recoupement (croisement ou convergence) : zone materialisee. |

## Largeurs revues

Demi-largeurs gauche et droite explicites (AD-45) ; tous les echantillons possedes valent ces demi-largeurs au pas canonique (0.0010 m) pres.

| Sujet | Gauche (m) | Droite (m) |
|---|---:|---:|
| `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | 2.0000 | 2.0000 |
| `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | 2.0000 | 2.0000 |
| `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | 2.0000 | 2.0000 |
| `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | 2.0000 | 2.0000 |
| `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | 2.0000 | 2.0000 |
| `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | 2.0000 | 2.0000 |
| `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | 2.0000 | 2.0000 |
| `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | 2.0000 | 2.0000 |
| `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | 2.0000 | 2.0000 |
| `4730190af6f78f0cd48d4bb005c9f091` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | 2.0000 | 2.0000 |
| `4dab8dc01f01a819f72ef4dc8a8a459d` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | 2.0000 | 2.0000 |
| `49ecb9240fd46b56a68f2ba5f9b575aa` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | 2.0000 | 2.0000 |
| `4b5389d91d5915f3196d95d243bc789f` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | 2.0000 | 2.0000 |
| `4b8232a70eb5574526502f1072cb50bd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1045154302 | 2.0000 | 2.0000 |
| `443ed3bc9f5371ef4f1439781cd012a8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1574439523 | 2.0000 | 2.0000 |
| `46c85a9afac17bde2153665adab63fbc` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-234956567 | 2.0000 | 2.0000 |
| `42de5da740b31ac0176161c776f3d4a1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-75338410 | 2.0000 | 2.0000 |
| `47b9c1ab789b84066b536acecec183ab` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1227312198 | 2.0000 | 2.0000 |
| `4ad222fbe3e52d2a5b6f86b332946c80` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1335115730 | 2.0000 | 2.0000 |
| `4c56cbc620e585a5373ea0ea9acae384` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1375911139 | 2.0000 | 2.0000 |
| `46f7f22596a1e5204781d18a86e839ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1461432457 | 2.0000 | 2.0000 |
| `48b41f08eb1eee61a4e6281a4e963893` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1516681040 | 2.0000 | 2.0000 |
| `4b08788f2b3434539d6daf47c410c6a4` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1812162174 | 2.0000 | 2.0000 |
| `410b074af81719b5992afb35a5d20cbd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-235211969 | 2.0000 | 2.0000 |
| `4f545c3fada86d11df7692a0081aa1a5` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-531979442 | 2.0000 | 2.0000 |
| `4261bcbef0b8a38b712c649286b082ba` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-539479367 | 2.0000 | 2.0000 |
| `488cff3f9501412fcae92bf829779984` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-656642079 | 2.0000 | 2.0000 |
| `4581e2ae94d3287b91ad57e31a9fe989` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-972045385 | 2.0000 | 2.0000 |
| `431aab2ccfb1de591d13a38baa04b796` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-981631451 | 2.0000 | 2.0000 |
| `44687593778b414acca9ab06ece502b1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | 2.0000 | 2.0000 |
| `4a53609938eef355087249c1b74ca6b0` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | 2.0000 | 2.0000 |
| `4815e26dcb7e3aa355dfa72569096986` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | 2.0000 | 2.0000 |
| `4796d8afab7aafe3c213381ce7be51b8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | 2.0000 | 2.0000 |
| `4640b60be5be840f1fa778f300b64d84` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | 2.0000 | 2.0000 |
| `4b33ebfdfb0a4ef2c2c6cbb267a5f2be` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | 2.0000 | 2.0000 |
| `4de2f43948bbec45fce0fbe76d4658ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | 2.0000 | 2.0000 |
| `42134703636fbc25221f6dc2ca06c793` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | 2.0000 | 2.0000 |

## Disposition des taches 5.27

Chaque tache est disposee exactement une fois. Controle, Conflit et Largeur par leurs donnees typees ; les autres par une disposition explicite.

| Categorie | Sujet | Disposition | Detail |
|---|---|---|---|
| Conflit | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Conflits | 6 candidat(s) decide(s), 6 zone(s) materialisee(s) |
| Conflit | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Conflits | 6 candidat(s) decide(s), 6 zone(s) materialisee(s) |
| Conflit | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Conflits | 6 candidat(s) decide(s), 6 zone(s) materialisee(s) |
| Conflit | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Conflits | 6 candidat(s) decide(s), 6 zone(s) materialisee(s) |
| Conflit | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Conflits | 28 candidat(s) decide(s), 28 zone(s) materialisee(s) |
| Conflit | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Conflits | 6 candidat(s) decide(s), 6 zone(s) materialisee(s) |
| Conflit | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Conflits | 6 candidat(s) decide(s), 6 zone(s) materialisee(s) |
| Conflit | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Conflits | 6 candidat(s) decide(s), 6 zone(s) materialisee(s) |
| Conflit | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Conflits | 6 candidat(s) decide(s), 6 zone(s) materialisee(s) |
| Controle | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Controles | 3 controles Uncontrolled, un par approche |
| Controle | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Controles | 3 controles Uncontrolled, un par approche |
| Controle | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Controles | 3 controles Uncontrolled, un par approche |
| Controle | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Controles | 3 controles Uncontrolled, un par approche |
| Controle | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Controles | 4 controles Uncontrolled, un par approche |
| Controle | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Controles | 6 controles Uncontrolled, un par approche |
| Controle | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Controles | 6 controles Uncontrolled, un par approche |
| Controle | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Controles | 6 controles Uncontrolled, un par approche |
| Controle | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Controles | 6 controles Uncontrolled, un par approche |
| Frontiere | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Frontiere | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Reviewed | Revue a l'overlay Gate A (sign-off du proprietaire). |
| Largeur | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4730190af6f78f0cd48d4bb005c9f091` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4dab8dc01f01a819f72ef4dc8a8a459d` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `49ecb9240fd46b56a68f2ba5f9b575aa` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4b5389d91d5915f3196d95d243bc789f` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4b8232a70eb5574526502f1072cb50bd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1045154302 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `443ed3bc9f5371ef4f1439781cd012a8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-1574439523 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `46c85a9afac17bde2153665adab63fbc` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-234956567 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `42de5da740b31ac0176161c776f3d4a1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1470702659269911219-75338410 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `47b9c1ab789b84066b536acecec183ab` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1227312198 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4ad222fbe3e52d2a5b6f86b332946c80` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1335115730 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4c56cbc620e585a5373ea0ea9acae384` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1375911139 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `46f7f22596a1e5204781d18a86e839ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1461432457 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `48b41f08eb1eee61a4e6281a4e963893` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1516681040 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4b08788f2b3434539d6daf47c410c6a4` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-1812162174 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `410b074af81719b5992afb35a5d20cbd` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-235211969 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4f545c3fada86d11df7692a0081aa1a5` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-531979442 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4261bcbef0b8a38b712c649286b082ba` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-539479367 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `488cff3f9501412fcae92bf829779984` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-656642079 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4581e2ae94d3287b91ad57e31a9fe989` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-972045385 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `431aab2ccfb1de591d13a38baa04b796` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7270336988349436968-981631451 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `44687593778b414acca9ab06ece502b1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4a53609938eef355087249c1b74ca6b0` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4815e26dcb7e3aa355dfa72569096986` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4796d8afab7aafe3c213381ce7be51b8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4640b60be5be840f1fa778f300b64d84` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4b33ebfdfb0a4ef2c2c6cbb267a5f2be` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `4de2f43948bbec45fce0fbe76d4658ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Largeur | `42134703636fbc25221f6dc2ca06c793` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite) |
| Ligne | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
| Ligne | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
| Ligne | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
| Ligne | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
| Ligne | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
| Ligne | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
| Ligne | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
| Ligne | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
| Ligne | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | NotRequiredForCurrentControlKind | Aucune ligne sous Uncontrolled ; reouverture : Story 5.35, des qu'un controle passe a Stop, Yield ou Priority. |
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
| frontiere | precedent retenu un quart d'hysteresis apres sa fin, OutsideEnvelope, pas de contresens | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, Ambiguous, lateral=0.0000 m, confiance=0.3394 | vert |
| deplace | reference dans l'enveloppe, empreinte debordante : meme corridor, lateral signe, OutsideEnvelope | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, lateral=1.4850 m, confiance=1.0000 | vert |
| contresens | cap oppose sur sa voie : meme corridor, WrongWay | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=WrongWay, lateral=0.0000 m, confiance=1.0000 | vert |
| carrefour ambigu | debut des mouvements divergents d'une approche : un mouvement de cette approche, Ambiguous, confiance < 1 | localise=oui, JunctionMovement `4e0c96d3fe6dbf797b6b539c16fe42ac`, drapeaux=Ambiguous, lateral=0.0007 m, confiance=0.3371 | vert |
| hors corridor | reference hors enveloppe dans le seuil d'acceptation : reste localisee, OutsideEnvelope, sans snap | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, lateral=3.2500 m, confiance=1.0000 | vert |

## Overlay et Gate A

Overlay canonique : `_bmad-output/implementation-artifacts/overlay-5-28-mvp-run.txt` (25 instances de module, hash `25089c9d1d80b1bc189deeb27405194931932831f822fd8c12d187c9475c5cd1`), produit par la meme fonction que le dessin de la fenetre `RoadRage/Traffic V2/Revue Gate A`.

La Gate A n'est ouverte que par `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-signoff.json`, ecrit par le proprietaire depuis cette fenetre apres revue des 25 instances, et lie aux hashes source, lignee, decisions, compilateur, modele, version et overlay d'un pipeline frais. Un sign-off absent ou perime garde la Gate A fermee, jamais repare.

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

