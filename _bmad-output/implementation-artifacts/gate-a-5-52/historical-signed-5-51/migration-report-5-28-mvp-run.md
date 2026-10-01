<!-- rrs-gate-a-binding
source-hash: b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc
importer-version: 2
compiler-schema-version: 4
pipeline-version: 3
model-id: 419bd12ec9b5fe710e8a3719692c7982
lineage-hash: f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4
decisions-hash: 3e9c893f48b10a248fa72aa51f7f365c683e7e0a825d657861a65c4356e553fe
model-hash: 6525a2366641aeb6c28595259a2762abb4e881ff1c32b2e2f8de16e5e88c1ebb
road-model-version: v4:e8dff9e54bff1712308899158ad16a9b
overlay-hash: 20cd1d1f262f812d6fd035119a465a20dcbd4b39c18a58410f481f71d00a0abe
physical-input-hash: 5fdefa17a73c7183e701dcd94c5d20ff1b46785e1aa05911c6e3a8416a0c3595
semantic-input-hash: 9fae1d4e8423b1f7d8485ab8ff2b12dd6b334a1d49a9d40abf2f6fb6caeb6a57
clearance-hash: f397d2ca83e229e3c680d902db8344754656df4ec304d72aafcc94aa7a922d6e
body-hash: ac9116c85504699df0768ea7f132503677bffeae815f6971d124fc434b8e6200
-->
# Rapport Gate A : modele authore MVP_Run (Story 5.28)

Genere par le menu `RoadRage/Traffic V2/Compiler le modele authore`. Ne pas editer : un rapport retouche ou detache de ses entrees est rejete, jamais repare.

## Liaison

| Champ | Valeur |
|---|---|
| Hash de la source V1 extraite | `b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc` |
| Version de l'importeur / du pipeline | 2 / 3 |
| CompilerSchemaVersion | 4 |
| RoadModelId | `419bd12ec9b5fe710e8a3719692c7982` |
| Hash de la lignee | `f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-lineage.json`) |
| Hash des decisions | `3e9c893f48b10a248fa72aa51f7f365c683e7e0a825d657861a65c4356e553fe` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json`) |
| Hash du modele persiste | `6525a2366641aeb6c28595259a2762abb4e881ff1c32b2e2f8de16e5e88c1ebb` (`Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json`) |
| RoadModelVersion | `v4:e8dff9e54bff1712308899158ad16a9b` |
| Hash de l'overlay | `20cd1d1f262f812d6fd035119a465a20dcbd4b39c18a58410f481f71d00a0abe` (`_bmad-output/implementation-artifacts/overlay-5-28-mvp-run.txt`) |

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
| JunctionControl | 40 (un par approche, tous `Uncontrolled`) |
| ConflictZone | 120 (decisions acceptees seulement) |
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
| TJunction_West | 12 | 12 | 0 | 3 |
| Intersection_Center_Crossroads | 48 | 48 | 0 | 12 |
| Roundabout_SouthEast | 6 | 6 | 0 | 3 |
| TJunction_North | 12 | 12 | 0 | 3 |
| TJunction_South | 12 | 12 | 0 | 3 |
| Roundabout_SouthWest | 6 | 6 | 0 | 3 |
| TJunction_East | 12 | 12 | 0 | 3 |

| Carrefour | Mouvement A | Mouvement B | Decision | Zone | Motif |
|---|---|---|---|---|---|
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `405ba721192ad202e99c9ba49b5e9fb7` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthWest | Roundabout_NorthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `47114f641089fae85c0ee8305935fdaa` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthWest | Roundabout_NorthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `40b126f3a984fa21883e88b3f099319c` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `45c7253beaef1bef98463cee884919a4` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `43a738a3e1cbfb568fadc5b3548a1b8f` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `43e5a24dac93246333d9faf399592587` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `415d14c83ddadbaca56eba4406bcf2b8` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `41ddaaa21dd44070074b0aa5e4581880` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthEast | Roundabout_NorthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `4ef00a041311cff1569a33b5a476aa88` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthEast | Roundabout_NorthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_NorthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `44434fa7a74b378cd42ed08c42fe8692` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `40e3e192ec85c0457be62034d55155a4` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `49b398688f7efc5feae64cccd10773ac` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `465d5eec7c97132ad85c3ac363cca48e` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4ce031f1b7b682c6e75c39e87a7b4ab7` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4fdf379316413439f08ee9573e94b995` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4258af5419bba1365a3f0ad6ed3d44aa` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `40ce606892963a8943f86c2ecd963ba2` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4b49ca4b2117973d8355dbf1660471b3` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `470bc18b1824ae1e7e47ac80c41aca9c` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4b786539f4809cd080c6de032000a3bd` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4fbcc8e5f354d981b2a3580b4645f6b7` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `406690a88e36b4596ca05b5832a555a9` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4905f97ebed6b813a1b1ba7b9fc594bb` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_West | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `40fd4fd762a2c46f4f8087d3f9a597b2` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4bb1ec8d6203ad58929013286d06278b` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Accepted | `4a8d1f62800182ac0fade27255a39190` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4fade05dfa8725d21db5702002aa4ca3` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4c31ad754ca8b25911fbb0f12ca04f90` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4b168164d6d552a9e382264821c7dcbe` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4aeb5f7d500459bab9a69a74b6996994` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `41143c56ca46d5c88f92b78870868c87` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4c05c15b5871449b27880b4c5c1f2e92` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `46987262dd6fe1111a00229ab6c9d8ab` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `421fede006aa2f42e5a6fd0002a6d696` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Accepted | `490e9e2ea4bef88bf4a8ee14433a429b` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4ac63be5d9fb0655462900ee2188a088` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `448d024c97cec79a916b160742727798` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4645efe76bbc8c5cda09e53a759b85b9` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `42665e8c1aa6739044cecc21fb9f79be` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4037740c6f0de354c5000388516f6185` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47c31dcb178dc8692f142c61851d9cb7` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `417f3abf3c05390e7a87ecfd98a6a481` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4f93788f6768a9c8d22bd432fff01490` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `45f53e49188f42e47576c9788d6c899b` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `454750014c7f7da3a45a39cb6189b982` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4cb4ca66e2b91ace8ba4ce3150bf3ebb` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `49c2d7c1095d014dcd2e9707187a5d9e` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `434cfd5ad42d31287fd0fc4be036f3b3` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Accepted | `4147689d782f388b4c54ff1a77e72ca5` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4d99e930a4f47c2ed260aed77cac1f8d` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4987ba61c57809e24605d2648053e29e` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `49a2893231955267ed28fd25ded38ba0` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `45f30575f87cd3d65d78177ee3c66d85` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4505708aa91fbff05b77dbe422d6e1b5` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Accepted | `45a0180046cf5cfca98979d7fcd8828a` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `488452c23f729a6d1d1f9aa582c2bf8f` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `452cc0b3f8ae471194e7907f1351d3ad` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `415e54e20484285ef32cb5ede00b61ae` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Accepted | `4b29ef3394ffa2df90a10e235b95a59c` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `44eb41658994bf0d068556234e7c7491` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `4065d99bf14d8ee4c3e42099de20fa89` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `44ef4e39ca0ade7338fc8feec0f429be` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4a4ed5b2470adc5ba3350df9cc85768e` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4d643d916a884b3c1233a665b66d89bc` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4a73e9da4c0ce92c90703379e30d7b9f` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `49340dcf8a92884fb95006b2ec095381` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4c9b462c1b8587eb241406b3a9d11683` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4b4cb6b3d76549de0935338e2b66aea9` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Accepted | `4c3054544ab019cafa18a48c279614b0` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4a6c823b85b757d2f3505c8789aa528c` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Accepted | `4c20416c36051baa4edfb294cd6e169b` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47153008967b6178e0b67b3b0dd7a289` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `4e24e297c4d1bd9139c77a07f6177abb` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_SouthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Accepted | `48fd616bb1789845fc2043ad1f0ba7a5` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `45347e3ef39bcc64fdfccaf89295a4bd` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `406efc13fc2d5de520cbe34197314db3` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `4e2ebb271a55553f65fe7bc0087b7482` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthEast | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Roundabout_SouthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Accepted | `4fa896603af971df28394d41512e83b8` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4b91ce1faf5f47a051923e55db19d084` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `49e18ec3cedc8169b621139413c88eab` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4782b2025438431bd3b4c6d8fedfe99b` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47d2bcbf20f128f9b2cf7d5f8dee60b9` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `437c2f58c60a75b9a723a64035b160b2` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `436441bec58608acbc6fb187181a6bbe` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `43c7c6dff15ddd8ea442f3b4cb85a884` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4a85cf73649c4ac79f7a21112c98daa3` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | Accepted | `4875672f99ccdb811ea74f51b86d298f` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `40bd2f68ba2bc3ff9535b711aea9998f` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4e90907066432b37ea55bf8b6c7967b3` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4e982b0fa19ae9f5ee3c0bc424e8ac91` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `46c0852ee863ff838d59392162782297` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `4fce84fd1a162287b7b06ed08526d094` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `46a27562fc7ef73899775b45767338b1` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4651661907bde7ddf496e0b38b1a67b2` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4a44f9710a830e7f77c6952d114c4fbf` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4dc9c6211a49ca8f0980f973af7a1bac` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `46fef049de32bffeccdcc752588447a5` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `4fe75f8b969a1fadc554e6cd737aee9c` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4ba61b4456363d785c934bbc3dbb98aa` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4b09a0f12076cb983208a9d922a545b7` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | Accepted | `4c3765f4eeddd6c6be43e3c0c8c83182` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4c147a4d6d6d6cd2fde23d045167c692` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Accepted | `450099ba966907d2e411d1b20b54ae99` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `4a4250039f40a26ff5b4051a197c149d` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | Accepted | `42b83e7c01b9cbacc9919a1bb7e9e68c` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Roundabout_SouthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Accepted | `451483d7154f55215bd3214ab24e3d98` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | Accepted | `49b9fb8a95b566317bd90511c5724b82` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| Roundabout_SouthWest | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | Roundabout_SouthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Accepted | `4df53fca19b7ee5211d4e512b3a4da86` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `499ace82c622f81cf600719848bae693` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `489e099514f065e68f19cf4dbfe350b2` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `47c7fee4039145473209b7b5d87472b3` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `49f9a6d925302128b2c14ee2328fabb6` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | Accepted | `46a75b0c04792a9ac17415da45f3c98f` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `433b2a4fbf3fe4c8557ecd7b832e4fb1` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `4a435f4e82a6a473eee3d534a3378fb9` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `468af52ce1bbea22b1482f15af632390` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | Accepted | `43ebda8c3f20a8ad55ad4a9628e3edab` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `48dacd01988ac15ff1cf37499497f885` | Un temoin de poses donne un recouvrement des rectangles orientes gonfles. |
| TJunction_East | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | Accepted | `436005183adb61a537157827df3372be` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |
| TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | Accepted | `43bbfeff3c02c3bf890cd95139e295a3` | La borne continue autorise un contact sans temoin ponctuel concluant : conflit accepte par prudence. |

## Largeurs revues

Demi-largeurs gauche et droite explicites (AD-45). La largeur revue est APPLIQUEE aux echantillons possedes (5.49) : `Uniform` ecrit la decision ; `EndpointInterpolation` (carrefours seulement) interpole chaque mouvement en s/Length entre les largeurs appliquees de ses corridors d'extremite, la decision valant plancher. Tout echantillon reste >= demi-gabarit + marge (1.2800 m) de chaque cote. Importee = amorce de l'importeur, jamais une autorite ; min-max sur les echantillons du sujet.

| Sujet | Decision g / d (m) | Application | Importee g / d (m) | Appliquee g / d (m) |
|---|---:|---|---:|---:|
| `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | 2.0000 / 2.0000 | Uniform | 2.0000 / 2.0000 | 2.0000 / 2.0000 |
| `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | 2.0000 / 2.0000 | EndpointInterpolation | 2.0000 / 2.0000 | 2.0000-4.0000 / 2.0000-4.0000 |
| `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | 2.0000 / 2.0000 | EndpointInterpolation | 2.0000 / 2.0000 | 2.0000-4.0000 / 2.0000-4.0000 |
| `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | 2.0000 / 2.0000 | EndpointInterpolation | 2.0000 / 2.0000 | 2.0000-4.0000 / 2.0000-4.0000 |
| `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | 2.0000 / 2.0000 | EndpointInterpolation | 2.0000 / 2.0000 | 2.0000-4.0000 / 2.0000-4.0000 |
| `4730190af6f78f0cd48d4bb005c9f091` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `4dab8dc01f01a819f72ef4dc8a8a459d` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `49ecb9240fd46b56a68f2ba5f9b575aa` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `4b5389d91d5915f3196d95d243bc789f` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
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
| `44687593778b414acca9ab06ece502b1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `4a53609938eef355087249c1b74ca6b0` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `4815e26dcb7e3aa355dfa72569096986` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `4796d8afab7aafe3c213381ce7be51b8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `4640b60be5be840f1fa778f300b64d84` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `4b33ebfdfb0a4ef2c2c6cbb267a5f2be` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `4de2f43948bbec45fce0fbe76d4658ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |
| `42134703636fbc25221f6dc2ca06c793` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | 4.0000 / 4.0000 | Uniform | 2.0000 / 2.0000 | 4.0000 / 4.0000 |

## Giratoires : degagement a deux gabarits

Deux gabarits max du profil versionne (W/2 = 1.0300 m, L = 4.5000 m, marge m = 0.2500 m) cote a cote, cap tangent, au point le plus serre : R_in = r_in + m + W/2 ; c_in = sqrt((R_in + W/2)^2 + (L/2)^2) ; R_out = c_in + 2m + W/2 ; c_out = sqrt((R_out + W/2)^2 + (L/2)^2) ; residu = (r_out - m) - c_out. Preuve supplementaire : un residu positif ne reduit jamais la cible (anneau V2 4,0 / 4,0 m, ilot <= 1,75 m, pave >= 10,25 m).

V2 : centre = racine du module ; corridors d'anneau et continuations appliques ; r_in = max des bords interieurs, r_out = min des bords exterieurs. Physique : empreintes XZ des colliders ; r_in = portee de `Col_Island` ; pave = min sur 720 rayons (pas 1 cm) de la sortie de l'union des `Col_Roadway*` ; obstacles = colliders non declencheurs hors chaussee et ilot dont la hauteur recoupe [sommet de route, +2.0000 m] ; r_out = min(pave, obstacle le plus proche).

| Instance | V2 r_in (m) | V2 r_out (m) | Residu V2 (m) | Ilot (m) | Pave (m) | Obstacle le plus proche | r_out physique (m) | Residu physique (m) |
|---|---:|---:|---:|---:|---:|---|---:|---:|
| Roundabout_SouthWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495` | 2.0000 | 10.0000 | 1.9945 | 1.5000 | 10.3700 | TunnelPortal_SouthWest/Col_Wall_Left a 11.3137 m | 10.3700 | 2.7818 |
| Roundabout_NorthWest `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255` | 2.0000 | 10.0000 | 1.9945 | 1.5000 | 10.3700 | TunnelPortal_NorthWest/Col_Wall_Left a 11.3137 m | 10.3700 | 2.7818 |
| Roundabout_NorthEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718` | 2.0000 | 10.0000 | 1.9945 | 1.5000 | 10.3700 | TunnelPortal_NorthEast/Col_Wall_Left a 11.3137 m | 10.3700 | 2.7818 |
| Roundabout_SouthEast `GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077` | 2.0000 | 10.0000 | 1.9945 | 1.5000 | 10.3700 | TunnelPortal_SouthEast/Col_Wall_Left a 11.3137 m | 10.3700 | 2.7818 |

## Gate A : preuve physique des 9 carrefours

Correct-course du 2026-09-25, precise le 2026-09-28. Balayage conservateur de la Story 5.51 (algorithme 5 ; giratoires : balayage v1) sur les references compilees 5.50 : empreinte = gabarit max du profil versionne + marge, gonfle de delta_c = 0.0500 m ; poses canoniques a pas h et coutures explicites ; residu = min(d_a, d_b) - delta/2, strictement positif. Carrefours classiques : chaque mouvement prolonge de L/2 + marge + delta_c, gate physique (obstacles dans la tranche du vehicule IA, relief routier franchissable excepte) et gate Sidewalk en plan (declarations, actives ou non). Giratoires : chaque mouvement prolonge de meme et chaque corridor d'anneau entier, gate physique, plus les residus d'anneau a deux gabarits ci-dessus. Allocation de suivi laterale a_e = 0.0000 m. Hors `RoadModelVersion`, hash source et lignee.

Verdict : **vert**.

| Empreinte | Valeur |
|---|---|
| physical-input-hash (9 carrefours) | `5fdefa17a73c7183e701dcd94c5d20ff1b46785e1aa05911c6e3a8416a0c3595` |
| Entrees physiques des carrefours classiques (5.51) | `2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7` |
| Entrees physiques des giratoires | `12e1082c901dac20deaa8209a9f1b0028030d3d19a3bc3f417b059084775d834` |
| semantic-input-hash (Sidewalk, 5.51) | `9fae1d4e8423b1f7d8485ab8ff2b12dd6b334a1d49a9d40abf2f6fb6caeb6a57` |
| clearance-hash (bloc des residus ci-dessous) | `f397d2ca83e229e3c680d902db8344754656df4ec304d72aafcc94aa7a922d6e` |

### Residus

a_e = 0 m ; h = 0.05 m

| Genre | Carrefour | Trajectoire | Surface Sidewalk | Residu physique (m) | Obstacle temoin | Physique | Residu Sidewalk (m) | Sidewalk |
|---|---|---|---|---:|---|---|---:|---|
| anneau | Roundabout_NorthEast | deux gabarits, anneau physique | - | 2.78177547 | TunnelPortal_NorthEast/Col_Wall_Left | - | - | - |
| anneau | Roundabout_NorthEast | deux gabarits, enveloppe V2 | - | 1.99448538 | - | - | - | - |
| anneau | Roundabout_NorthWest | deux gabarits, anneau physique | - | 2.78177452 | TunnelPortal_NorthWest/Col_Wall_Left | - | - | - |
| anneau | Roundabout_NorthWest | deux gabarits, enveloppe V2 | - | 1.99448538 | - | - | - | - |
| anneau | Roundabout_SouthEast | deux gabarits, anneau physique | - | 2.78177452 | TunnelPortal_SouthEast/Col_Wall_Left | - | - | - |
| anneau | Roundabout_SouthEast | deux gabarits, enveloppe V2 | - | 1.99448538 | - | - | - | - |
| anneau | Roundabout_SouthWest | deux gabarits, anneau physique | - | 2.78177452 | TunnelPortal_SouthWest/Col_Wall_Left | - | - | - |
| anneau | Roundabout_SouthWest | deux gabarits, enveloppe V2 | - | 1.99448538 | - | - | - | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 0.112667568 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.645480752 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 0.112667568 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_North_Out (droite) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.112667568 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.112667568 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.497870833 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 4.64548063 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.4978702 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 4.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.42158252 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 5.09333038 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 4.38012 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.421583533 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 0.6450009 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.645480752 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 0.6450009 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.644999862 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.6450009 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.645479441 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.6450009 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.6450009 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 4.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.497870564 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.497870237 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 4.64548063 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 4.38011837 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.4215835 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.421584219 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_East_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 5.093331 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 1.65437508 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.645480633 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 1.65437508 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 1.65437508 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.645480633 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_South_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 1.65437508 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 0.112669289 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.645480633 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 0.112669289 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.644999862 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromNorth -> Connector_West_Out (droite) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.112669289 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.112669289 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 0.112669289 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.644999862 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 0.112669289 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.645480633 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.112669289 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.112669289 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 1.65437508 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 1.65437508 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.645480633 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 1.65437508 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_North_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 1.65437508 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East | - | 0.645480633 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 4.64548063 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.497870237 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.497870564 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 4.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 5.093331 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.421584219 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 0.4215835 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.421584219 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West | - | 4.38011837 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 0.6450009 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.644999862 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 0.6450009 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.645480752 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.6450009 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.6450009 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.6450009 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East | - | 0.645479441 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 4.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.4978702 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 4.64548063 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.497870833 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.421583533 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 4.38012 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 5.09333038 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_North_Out (gauche) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.42158252 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.42158252 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 0.112667568 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.645 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 0.112667568 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.645480752 | - |
| degagement | Intersection_Center_Crossroads | Intersection_Center_Crossroads: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW | 0.112667568 | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West | - | 0.112667568 | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | - | 3.527526 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Wall_Left | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Merge_Diagonal -> Ring_Split_West | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Merge_South -> Ring_Split_Diagonal | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Merge_West -> Ring_Split_South | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | - | 3.52752686 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthEast/Collision/Col_Wall_Right | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthEast | Roundabout_NorthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | - | 3.52753043 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Wall_Left | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Merge_Diagonal -> Ring_Split_West | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Merge_South -> Ring_Split_Diagonal | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Merge_West -> Ring_Split_South | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | - | 3.52752972 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_NorthWest/Collision/Col_Wall_Right | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_NorthWest | Roundabout_NorthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | - | 3.527526 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Wall_Left | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Connector_South_In -> Ring_Merge_South (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Connector_West_In -> Ring_Merge_West (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Merge_Diagonal -> Ring_Split_West | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Merge_South -> Ring_Split_Diagonal | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Merge_West -> Ring_Split_South | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | - | 3.527526 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthEast/Collision/Col_Wall_Right | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthEast | Roundabout_SouthEast: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | - | 3.527522 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Wall_Left | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Connector_South_In -> Ring_Merge_South (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Connector_West_In -> Ring_Merge_West (entree d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Merge_Diagonal -> Ring_Split_West | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Merge_South -> Ring_Split_Diagonal | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Merge_West -> Ring_Split_South | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_Diagonal -> Connector_Diagonal_Out (sortie d'anneau) | - | 3.52753043 | MVP_Run/RunRoot/LaneGraph/TunnelPortal_SouthWest/Collision/Col_Wall_Right | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Connector_South_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Connector_West_Out (sortie d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | Roundabout_SouthWest | Roundabout_SouthWest: Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | - | Infinity | aucun | - | - | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.4978705 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.6449995 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.64547873 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Right | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.4978698 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.380113 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.4215849 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.4215858 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.6449981 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.6454787 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North | Infinity | aucun | - | 0.6449981 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.6454803 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.645 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.112664416 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.64548 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.497870833 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.644998 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Right | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.4978698 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.421581119 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.380118 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.421585947 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.645480633 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.645 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE | Infinity | aucun | - | 0.6450003 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW | Infinity | aucun | - | 0.645480633 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.6449994 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.645480633 | - |
| degagement | TJunction_East | TJunction_East: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.112665921 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.497870266 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.6449995 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Left | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.4978699 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.64547873 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 4.3801136 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.4215829 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.4215858 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.6454787 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.6449981 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North | Infinity | aucun | - | 0.6449981 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.645480156 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.645 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East | - | 0.112665057 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.64548 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.497870356 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Left | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.49786976 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.644999 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.421582937 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.380113 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.421585768 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.645 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.6454817 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE | Infinity | aucun | - | 0.645 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW | Infinity | aucun | - | 0.645480633 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.6449995 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.6454817 | - |
| degagement | TJunction_North | TJunction_North: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.1126647 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.6449995 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.497870266 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.64547873 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Right | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.4978699 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.3801136 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.421583384 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.4215858 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.6449981 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.6454787 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North | Infinity | aucun | - | 0.6449981 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.645480156 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.645 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.112664744 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.497870356 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.64548 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.644999 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Right | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.49786976 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.421582371 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 4.3801136 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.421585768 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.6454817 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.645 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE | Infinity | aucun | - | 0.645 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW | Infinity | aucun | - | 0.645480633 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.6449995 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.6454817 | - |
| degagement | TJunction_South | TJunction_South: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast | - | 0.112664908 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.6449995 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.4978705 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Left | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.4978698 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.64547873 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 4.3801136 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.421583116 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_South_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North | 10.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.4215858 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.6454787 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Left | Infinity | aucun | - | 0.6449981 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromEast -> Connector_West_Out (tout droit) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North | Infinity | aucun | - | 0.6449981 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.6454803 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.645 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_East_Out (droite) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE | 6.645 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West | - | 0.112665191 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.497870833 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.64548 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Left | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.4978698 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.644998 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.4215829 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 4.380117 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromSouth -> Connector_West_Out (gauche) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North | 10.6449986 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.421585947 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.645 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right | Infinity | aucun | - | 0.645480633 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE | Infinity | aucun | - | 0.645 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_East_Out (tout droit) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW | Infinity | aucun | - | 0.645480931 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.6449994 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.645480633 | - |
| degagement | TJunction_West | TJunction_West: Junction_FromWest -> Connector_South_Out (droite) | RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW | 6.6449995 | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest | - | 0.112665176 | - |

### Entrees mesurees

Une ligne par entree de chaque empreinte (chemin, genre, empreinte de sa ligne canonique) : nomme ce qui a change quand une empreinte differe.

| Genre | Entree | Empreinte |
|---|---|---|
| physique | /Greybox_AIVehicle:BoxCollider | `3f771c5e5f63f729` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_East:BoxCollider | `cf3538d8adcb98a2` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthEast:BoxCollider | `457a76adada661ec` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_SouthWest:BoxCollider | `8db33cc29700591d` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_CityBlock_A_West:BoxCollider | `b768651b4c987fb7` |
| physique | MVP_Run/RunRoot/GreyboxMap/Greybox_GroundPlane:BoxCollider | `6f686546f9ec624f` |
| physique | MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est:BoxCollider | `ff4f41ebf5f7dcc5` |
| physique | MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest:BoxCollider | `9a92008879a5a3d7` |
| physique | MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast:BoxCollider | `37114cb7c112d002` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Roadway:BoxCollider | `8c2b12fed85e88da` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left:BoxCollider | `3230f46b058e3160` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right:BoxCollider | `ec9881b289311ba9` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Roadway:BoxCollider | `1846088fd417d14f` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left:BoxCollider | `b78ec2beb09edf8e` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right:BoxCollider | `607ce444e9dd0ec2` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Roadway:BoxCollider | `f8e790d36bd9d115` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left:BoxCollider | `c094be411d1e99a4` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right:BoxCollider | `6ec6cb4d8933ea01` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Roadway:BoxCollider | `4a8a724ee30c93f9` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left:BoxCollider | `738a4c33707f1551` |
| physique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right:BoxCollider | `87b40e71f6cdc6a8` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_East:BoxCollider | `353107b5a59c1f1e` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_North_West:BoxCollider | `04573ccc5f79a9bd` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_East:BoxCollider | `2b3b1eed75326657` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Curb_South_West:BoxCollider | `fc1cb514c52aba1f` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Roadway:BoxCollider | `25c6b1f0dde7ebe3` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Box_B:BoxCollider | `a45dfc34f1225006` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Box_Diagonal:BoxCollider | `4a68c54e54e2d6f5` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE:BoxCollider | `5fef44153eb53bc9` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Box_B:BoxCollider | `72c7b7a6d2375558` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Box_Diagonal:BoxCollider | `eb0f23161703f0bc` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW:BoxCollider | `83c3ea25ad813caa` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `346661f3cc35978b` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `d7ee8e7bdffeef21` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `88013fe23f6a9542` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `cf9db23a92004d19` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `070ccada9e847b3a` |
| physique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `16332e1c801a4673` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Roadway:BoxCollider | `6049e581316f1330` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left:BoxCollider | `a6819073c84a8681` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Right:BoxCollider | `49ea32d8c412fe5f` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Collision/Col_Roadway:BoxCollider | `12aa4fb4469e5dc7` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Left:BoxCollider | `1e323d845d9afc34` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_East_South/Collision/Col_Sidewalk_Right:BoxCollider | `c3697ce6bcc03910` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Collision/Col_Roadway:BoxCollider | `8e14fbc359e54a09` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Left:BoxCollider | `4a1dcd097c55da36` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_East/Collision/Col_Sidewalk_Right:BoxCollider | `e0312c926f68b45c` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Roadway:BoxCollider | `5230c3a90707c10e` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Left:BoxCollider | `f95235610db46fe4` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right:BoxCollider | `ca02e827a46a43e4` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Roadway:BoxCollider | `d0d8a7ff7498a26f` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left:BoxCollider | `bfce1b18e054b76f` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Right:BoxCollider | `a22795de9b2927e9` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Collision/Col_Roadway:BoxCollider | `922f2afbd666f5d0` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Left:BoxCollider | `d6b39f6a4ad5bced` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_South_West/Collision/Col_Sidewalk_Right:BoxCollider | `930a56f0ec1a79df` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Collision/Col_Roadway:BoxCollider | `1d1ceb7b4928dc4c` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Left:BoxCollider | `93134bde8ee68ca9` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_North/Collision/Col_Sidewalk_Right:BoxCollider | `41e8175852f913c4` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Roadway:BoxCollider | `a4f216eefa1d7461` |
| physique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Left:BoxCollider | `517b7f0ddac71e31` |
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
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `779bca713b5568b4` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `4c664968f511abe7` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `54fde63684efc16e` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `576467eb4042b04f` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `fb2cb9d3063c714f` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `b78da4fbac2d081a` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North:BoxCollider | `e96df02edfe6b537` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Roadway:BoxCollider | `fe8f6c8f404e956f` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `bedced664315ed2e` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `905080b4b1d8bfe9` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `80598a2a5ec0861a` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `7e5b9bdbff8da704` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `c743b777fd4b45e2` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `25c877ee6cc1c0b4` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North:BoxCollider | `25e3b05146556b77` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Roadway:BoxCollider | `23290d1b7004652b` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `64131410a9f66318` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `0c71b331d682bfc7` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `3dadd22280536dc4` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `ec7fbea11e0f7319` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `c27e44aaf4c3c419` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `0dffd08a33ddac30` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North:BoxCollider | `c0a01f207cb0c014` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Roadway:BoxCollider | `6d34ca7180864e3e` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `0afe015d45daa617` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `a7e87b5378d8f1ec` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `5481a6ff92443eb8` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `5f05df2d4a0ffd7b` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `46b674d463cf4633` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `91a192a1a8c0302d` |
| physique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North:BoxCollider | `b79aa6dbe207201a` |
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
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Left:BoxCollider | `3230f46b058e3160` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Collision/Col_Sidewalk_Right:BoxCollider | `ec9881b289311ba9` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `eaa8837b85bead34` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `4e7261f25b40c4b6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `c32f5114379a84d3` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `921b12bde787f0b6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `190eab46863c35e6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `b7204d65281da3af` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `fa6384d51363831f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToEast/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `fcd45bc6481ca644` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Left:BoxCollider | `b78ec2beb09edf8e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Collision/Col_Sidewalk_Right:BoxCollider | `607ce444e9dd0ec2` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `7d44fa8e9170d409` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `a65f3b40cca416ac` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `8d07183040b1dc6b` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `6f201c57a87d1753` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `51cedf20b1742782` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `fd9453105fdb1af8` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `482a6683d5b9624c` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToNorth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `3056560540b58a51` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Left:BoxCollider | `c094be411d1e99a4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Collision/Col_Sidewalk_Right:BoxCollider | `6ec6cb4d8933ea01` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `f2d839850f66e00f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `a3089846f77dd94e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `8aa1747eb74d9419` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `c1c414e107047e5c` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `1b14ff279c1145e7` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `bedce1a3b1724fff` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `a6fd7641b10b2b2a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToSouth/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `c2cf59b9f2ab8b6d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Left:BoxCollider | `738a4c33707f1551` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Collision/Col_Sidewalk_Right:BoxCollider | `87b40e71f6cdc6a8` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `844b6402d14b6e91` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `b8826ebd86228d4e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `8f88ef5e49c9d408` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `1a86cd83ec72d964` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `04e6be561f45bf94` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `65c9fe6f3e931132` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `9ac89d15498a77eb` |
| semantique | MVP_Run/RunRoot/LaneGraph/Avenue_CenterToWest/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `73e71faca92aeddf` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Box_B:BoxCollider | `a45dfc34f1225006` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Box_Diagonal:BoxCollider | `4a68c54e54e2d6f5` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Visual_0:MeshRenderer | `7fcfb0c3c2079f29` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Visual_1:MeshRenderer | `409d9c0975a77ea6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE/Chamfer_Visual_2:MeshRenderer | `f9b27c9f6f8f95b6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NE:BoxCollider | `5fef44153eb53bc9` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Box_B:BoxCollider | `72c7b7a6d2375558` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Box_Diagonal:BoxCollider | `eb0f23161703f0bc` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Visual_0:MeshRenderer | `b5174a1d3bb7d86e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Visual_1:MeshRenderer | `e73ee917c9a1fb31` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW/Chamfer_Visual_2:MeshRenderer | `f154d8a8fe0e7e01` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_NW:BoxCollider | `83c3ea25ad813caa` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `346661f3cc35978b` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `d7ee8e7bdffeef21` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `373b28e4add4cf62` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `8c449decacb89f2a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `8dd45aa3e7182915` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `88013fe23f6a9542` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `cf9db23a92004d19` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `070ccada9e847b3a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `b9b9554b98898292` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `19424e4bc3f04948` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `b71350cb8a91c468` |
| semantique | MVP_Run/RunRoot/LaneGraph/Intersection_Center_Crossroads/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `16332e1c801a4673` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Left:BoxCollider | `a6819073c84a8681` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Collision/Col_Sidewalk_Right:BoxCollider | `49ea32d8c412fe5f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `a19e02c79334cd02` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `95326d85978cd15e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `d51baf84c10419e8` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `b0abcaf854ec1613` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `78c5b08facde92bd` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_East_North/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `7b3a3dbf883e7c26` |
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
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Left:BoxCollider | `f95235610db46fe4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Collision/Col_Sidewalk_Right:BoxCollider | `ca02e827a46a43e4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `b69ff775f86e551d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `78b05404ad674594` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `58993a1ba346a936` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `139c3a8375a4f6d6` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `8f4ff9d40653a59e` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_North_West/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `6b0520f51dac86fc` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Left:BoxCollider | `bfce1b18e054b76f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Collision/Col_Sidewalk_Right:BoxCollider | `a22795de9b2927e9` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `0b349470b272ad91` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `664e5b3059e5008a` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s6:MeshRenderer | `46fe153705ace85d` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `5d44b3336f122732` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `45fa6f5576281707` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_South_East/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s6:MeshRenderer | `f4efa7cd527a50d7` |
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
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Left:BoxCollider | `517b7f0ddac71e31` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Collision/Col_Sidewalk_Right:BoxCollider | `4ece12511a68946f` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n2:MeshRenderer | `fc7df5a7ed35f4a4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_n6:MeshRenderer | `748e255bb4c82852` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_L_s2:MeshRenderer | `1454db1b76802be3` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n2:MeshRenderer | `f11d3fa66b81a5a4` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_n6:MeshRenderer | `4fa211c249936e16` |
| semantique | MVP_Run/RunRoot/LaneGraph/Ring_West_South/Visual_Greybox_RoadSegment_TwoWay/Walk_R_s2:MeshRenderer | `1133c56ee3407883` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `779bca713b5568b4` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `4c664968f511abe7` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `6d8665ac436694c2` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `fbd3d60f6115cc43` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `d0fcf9c38ae8ee8d` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `54fde63684efc16e` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `576467eb4042b04f` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `fb2cb9d3063c714f` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `10e163559e1c359f` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `7d625b4026ecb582` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `305e61b2b31c0430` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `b78da4fbac2d081a` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Collision/Col_Sidewalk_North:BoxCollider | `e96df02edfe6b537` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Visual_Greybox_TJunction/Sidewalk_North_n2:MeshRenderer | `4d821f2d463f3e07` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Visual_Greybox_TJunction/Sidewalk_North_n6:MeshRenderer | `e38a15cd1e2d19c8` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Visual_Greybox_TJunction/Sidewalk_North_s2:MeshRenderer | `1d85331ac7a57e07` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_East/Visual_Greybox_TJunction/Sidewalk_North_s6:MeshRenderer | `399e400acaf884e8` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `bedced664315ed2e` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `905080b4b1d8bfe9` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `492801dc13d1783e` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `d1c82c8f60a86df9` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `83418f00123b8fdf` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `80598a2a5ec0861a` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `7e5b9bdbff8da704` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `c743b777fd4b45e2` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `97d7e1950b1a39b7` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `102b936a2ecf6e66` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `50119408f0b27844` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `25c877ee6cc1c0b4` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Collision/Col_Sidewalk_North:BoxCollider | `25e3b05146556b77` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Visual_Greybox_TJunction/Sidewalk_North_n2:MeshRenderer | `6e4eb33f85a7c5cc` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Visual_Greybox_TJunction/Sidewalk_North_n6:MeshRenderer | `f592ee37be78db67` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Visual_Greybox_TJunction/Sidewalk_North_s2:MeshRenderer | `e76ac0841b259aad` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_North/Visual_Greybox_TJunction/Sidewalk_North_s6:MeshRenderer | `081bdea85dc28c56` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `64131410a9f66318` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `0c71b331d682bfc7` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `38ddc180475091be` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `6d8168ceb0f48dd6` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `76372846bda3dcbe` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `3dadd22280536dc4` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `ec7fbea11e0f7319` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `c27e44aaf4c3c419` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `aa30adf7c0441561` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `838afabe1362f9c8` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `e3bb0e7f9aac3fb0` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `0dffd08a33ddac30` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Collision/Col_Sidewalk_North:BoxCollider | `c0a01f207cb0c014` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Visual_Greybox_TJunction/Sidewalk_North_n2:MeshRenderer | `d740e06d45feb774` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Visual_Greybox_TJunction/Sidewalk_North_n6:MeshRenderer | `fc1c59f43d4032f7` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Visual_Greybox_TJunction/Sidewalk_North_s2:MeshRenderer | `f52665eb3c3895fb` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_South/Visual_Greybox_TJunction/Sidewalk_North_s6:MeshRenderer | `a3e357cae6ed6360` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_B:BoxCollider | `0afe015d45daa617` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Box_Diagonal:BoxCollider | `a7e87b5378d8f1ec` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_0:MeshRenderer | `2d5bcf1ddec24794` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_1:MeshRenderer | `307b07012c3ce173` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE/Chamfer_Visual_2:MeshRenderer | `0934032ff2c05b29` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SE:BoxCollider | `5481a6ff92443eb8` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_B:BoxCollider | `5f05df2d4a0ffd7b` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Box_Diagonal:BoxCollider | `46b674d463cf4633` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_0:MeshRenderer | `72f99c8e83ea0296` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_1:MeshRenderer | `d2379a603ec67ba1` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW/Chamfer_Visual_2:MeshRenderer | `36c623ba5e7303ef` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_Corner_SW:BoxCollider | `91a192a1a8c0302d` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Collision/Col_Sidewalk_North:BoxCollider | `b79aa6dbe207201a` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Visual_Greybox_TJunction/Sidewalk_North_n2:MeshRenderer | `83feec3ab2f480b2` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Visual_Greybox_TJunction/Sidewalk_North_n6:MeshRenderer | `b44598fd85318448` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Visual_Greybox_TJunction/Sidewalk_North_s2:MeshRenderer | `6dff32b567d9eb5d` |
| semantique | MVP_Run/RunRoot/LaneGraph/TJunction_West/Visual_Greybox_TJunction/Sidewalk_North_s6:MeshRenderer | `166856ee31ec1e18` |

## Disposition des taches 5.27

Chaque tache est disposee exactement une fois. Controle, Conflit et Largeur par leurs donnees typees ; les autres par une disposition explicite.

| Categorie | Sujet | Disposition | Detail |
|---|---|---|---|
| Conflit | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Conflits | 12 candidat(s) decide(s), 12 zone(s) materialisee(s) |
| Conflit | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Conflits | 12 candidat(s) decide(s), 12 zone(s) materialisee(s) |
| Conflit | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Conflits | 12 candidat(s) decide(s), 12 zone(s) materialisee(s) |
| Conflit | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Conflits | 12 candidat(s) decide(s), 12 zone(s) materialisee(s) |
| Conflit | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Conflits | 48 candidat(s) decide(s), 48 zone(s) materialisee(s) |
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
| Largeur | `4e5a1a75c3a9e48af02ad41483e25491` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1056351953 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `442bd8af1793e34f2d407ec98f9e6581` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-1186247037 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4b095728e42083ceb90543412e198092` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-215267690 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4993ac8c2a6dc3f17d03a16cbb916fac` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-2174360367984508665-525617468 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `490b6106a4522c5dfec0040c66cd82b9` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3237468531753948436-2089303282 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), Uniform |
| Largeur | `4c8d26eb6c05c178fc8444e842e07d8c` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1515478495 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), EndpointInterpolation |
| Largeur | `41a63c5603c904390698494de09c299d` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-1873927255 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), EndpointInterpolation |
| Largeur | `4309f6e91e6d597ef92349b8ede4fc89` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-663126718 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), EndpointInterpolation |
| Largeur | `4933ae7e9cbb42b7278015dfd5e3c3b0` junction:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7654237338994223078-764670077 | Largeur revue | 2.0000 / 2.0000 m (gauche / droite), EndpointInterpolation |
| Largeur | `4730190af6f78f0cd48d4bb005c9f091` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1515478495 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `4dab8dc01f01a819f72ef4dc8a8a459d` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-1873927255 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `49ecb9240fd46b56a68f2ba5f9b575aa` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-663126718 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `4b5389d91d5915f3196d95d243bc789f` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-1277576483161264561-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8536729323650507684-764670077 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
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
| Largeur | `44687593778b414acca9ab06ece502b1` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1515478495 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `4a53609938eef355087249c1b74ca6b0` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-1873927255 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `4815e26dcb7e3aa355dfa72569096986` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-663126718 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `4796d8afab7aafe3c213381ce7be51b8` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-7715433215887918611-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-5068725685921314059-764670077 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `4640b60be5be840f1fa778f300b64d84` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1515478495>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1515478495 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `4b33ebfdfb0a4ef2c2c6cbb267a5f2be` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-1873927255>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-1873927255 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `4de2f43948bbec45fce0fbe76d4658ad` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-663126718>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-663126718 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
| Largeur | `42134703636fbc25221f6dc2ca06c793` section:GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-8651751632837739532-764670077>GlobalObjectId_V1-2-ee081d5cbff641bb9dd499b4f13a8d62-3857765549630400084-764670077 | Largeur revue | 4.0000 / 4.0000 m (gauche / droite), Uniform |
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
| frontiere | precedent retenu un quart d'hysteresis apres sa fin, OutsideEnvelope, pas de contresens | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, Ambiguous, lateral=0.0000 m, confiance=0.1667 | vert |
| deplace | reference dans l'enveloppe, empreinte debordante : meme corridor, lateral signe, OutsideEnvelope | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, lateral=1.4850 m, confiance=1.0000 | vert |
| contresens | cap oppose sur sa voie : meme corridor, WrongWay | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=WrongWay, lateral=0.0000 m, confiance=1.0000 | vert |
| carrefour ambigu | debut des mouvements divergents d'une approche : un mouvement de cette approche, Ambiguous, confiance < 1 | localise=oui, JunctionMovement `4030253e182e3ed1b7d2aeea7a73feb6`, drapeaux=Ambiguous, lateral=0.0000 m, confiance=0.0367 | vert |
| hors corridor | reference hors enveloppe dans le seuil d'acceptation : reste localisee, OutsideEnvelope, sans snap | localise=oui, LaneCorridor `4f3543b1218b82af65b5b8fc58457fb3`, drapeaux=OutsideEnvelope, lateral=3.2500 m, confiance=1.0000 | vert |

## Overlay et Gate A

Overlay canonique : `_bmad-output/implementation-artifacts/overlay-5-28-mvp-run.txt` (25 instances de module, hash `20cd1d1f262f812d6fd035119a465a20dcbd4b39c18a58410f481f71d00a0abe`), produit par la meme fonction que le dessin de la fenetre `RoadRage/Traffic V2/Revue Gate A`.

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

