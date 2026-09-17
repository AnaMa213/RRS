# Story 1.4 -- Preuve des seeds greybox Epic 1

Date : 2026-09-08

Acteur : Agent via Blender background et Unity MCP.

Statut : quatre seeds greybox internes crees, importes, prefabriques et verifies pour la Story 1.4. Aucun asset tiers, genere IA ou telecharge n'a ete utilise.

## Source et exports

Source Blender unique : `Assets/RoadRage/ArtSource/Blender/Epic1_GreyboxAssetSeeds.blend`

Generation : l'outil Blender CLI direct a d'abord echoue car `blender` n'etait pas dans le PATH de la session MCP. Le contournement retenu a lance un Blender background isole avec `bpy.app.binary_path` depuis l'instance connectee (`D:\Program Files\Blender\blender.exe`) et `--factory-startup`, sans nettoyer ni sauvegarder la scene interactive.

Blender : `5.2.1 LTS`, units `METRIC`, scale `1.0`, source sauvegardee avec `bpy.data.is_dirty=false`.

Exports controles :

| Seed | Stable id | Export FBX | Taille Blender | Materiaux | Lisibilite |
| --- | --- | --- | --- | --- | --- |
| `Greybox_Character_Rookie` | `char_rookie` | `Assets/RoadRage/ArtExports/Greybox_Character_Rookie.fbx` | `1.065 x 0.370 x 1.820 m` | 3 | Humanoide bipede : tete, torse, bras, jambes, pieds |
| `Greybox_Character_Veteran` | `char_veteran` | `Assets/RoadRage/ArtExports/Greybox_Character_Veteran.fbx` | `1.065 x 0.377 x 1.820 m` | 4 | Humanoide bipede avec visor |
| `Greybox_PlayerCar` | `vehicle_player_shared` | `Assets/RoadRage/ArtExports/Greybox_PlayerCar.fbx` | `4.440 x 2.060 x 1.420 m` | 3 | Voiture : carrosserie, cabine, capot, coffre, pare-chocs, quatre roues |
| `Greybox_CityBlock_A` | `building_city_block_a` | `Assets/RoadRage/ArtExports/Greybox_CityBlock_A.fbx` | `6.875 x 4.250 x 8.360 m` | 4 | Immeuble : volume urbain, facade, porte, fenetres, toit |

Export FBX : selection des objets du seed uniquement, `object_types={'EMPTY','MESH'}`, `axis_forward='Y'`, `axis_up='Z'`, `use_metadata=false`.

## Import Unity et prefabs

Importeur final pour les quatre FBX : `globalScale=1`, `useFileScale=false`, `bakeAxisConversion=true`, `addCollider=false`, `importAnimation=false`, `animationType=None`, `importBlendShapes=false`, `importCameras=false`, `importLights=false`.

Prefabs crees :

| Prefab | Stable id | Mesure Unity | Renderer count | Collider |
| --- | --- | --- | --- | --- |
| `Assets/RoadRage/Prefabs/Greybox_Character_Rookie.prefab` | `char_rookie` | `size=(1.065,1.820,0.370)`, `min=(-0.533,0.000,-0.140)`, `max=(0.533,1.820,0.230)` | 8 | `CapsuleCollider` root |
| `Assets/RoadRage/Prefabs/Greybox_Character_Veteran.prefab` | `char_veteran` | `size=(1.065,1.820,0.378)`, `min=(-0.533,0.000,-0.148)`, `max=(0.533,1.820,0.230)` | 9 | `CapsuleCollider` root |
| `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` | `vehicle_player_shared` | `size=(4.440,1.420,2.060)`, `min=(-2.220,0.020,-1.030)`, `max=(2.220,1.440,1.030)` | 10 | `BoxCollider` root |
| `Assets/RoadRage/Prefabs/Greybox_CityBlock_A.prefab` | `building_city_block_a` | `size=(6.875,8.360,4.250)`, `min=(-3.750,0.000,-2.125)`, `max=(3.125,8.360,2.125)` | 16 | `BoxCollider` root |

Chaque prefab a :

- un root gameplay a position `0,0,0`, rotation `0,0,0`, scale `1,1,1`;
- un child visuel `Visual_<PrefabName>` lie au FBX controle ;
- exactement un collider root simple ;
- aucun MeshCollider (`MeshCollider` absent sur tous les prefabs) ;
- aucun `NetworkObject` ;
- un composant `GreyboxAssetSeedMetadata` avec source, export, scale check, collider plan, replacement policy et visual readability.

## CharacterDef

`Assets/RoadRage/ScriptableObjects/Players/CharacterDef_Rookie.asset` garde `id: char_rookie` et reference `Assets/RoadRage/Prefabs/Greybox_Character_Rookie.prefab` dans `previewPrefab`.

`Assets/RoadRage/ScriptableObjects/Players/CharacterDef_Veteran.asset` garde `id: char_veteran` et reference `Assets/RoadRage/Prefabs/Greybox_Character_Veteran.prefab` dans `previewPrefab`.

Les ids, noms et teintes de la Story 1.3 ne sont pas renommes.

## Politique de remplacement

Les meshes restent greybox. Quand l'art final arrive, remplacer uniquement le child visuel ou une variante de rendu apres intake. Le root prefab, le stable id, les references `CharacterDef`, les colliders et les composants gameplay futurs restent stables conformement a AD-27.

La voiture n'est pas enregistree dans `DefaultNetworkPrefabs.asset` en Story 1.4 : elle devient un seed partage local, et la registration reseau attendra une story avec spawn reseau reel.

## Hashes

| Fichier | SHA-256 |
| --- | --- |
| `Assets/RoadRage/ArtSource/Blender/Epic1_GreyboxAssetSeeds.blend` | `238A18F493423B19EE7A223956E1D7B7E14339F1004EC14316992512A4CB2937` |
| `Assets/RoadRage/ArtExports/Greybox_Character_Rookie.fbx` | `67B91C5C0208AF308C6E2EA922C4556C00739F106DF36A2D3B8D2536188F0C16` |
| `Assets/RoadRage/ArtExports/Greybox_Character_Veteran.fbx` | `F7DE34373770BCA19C5FF8FB36EC4F1D76C2866408B59CE04AD8A38227CBD415` |
| `Assets/RoadRage/ArtExports/Greybox_PlayerCar.fbx` | `8E171A4C0D887F42BD8DBF1A4C7D42DBB45DA8C288E62CADFA3593FCF79DA980` |
| `Assets/RoadRage/ArtExports/Greybox_CityBlock_A.fbx` | `5657D65CC2CCBFD5F191F0E5AB6F27B26CC4C4CD37BDC4BC97E6B99CA5EA0BD5` |
| `Assets/RoadRage/Prefabs/Greybox_Character_Rookie.prefab` | `95F97F552EFCCFA890AF8C067CF2A8AC5993D0BD7B5836DD34B209CC7A80D5ED` |
| `Assets/RoadRage/Prefabs/Greybox_Character_Veteran.prefab` | `D71E6126DC09E39B3B028A6F118BA3CE1FBA69FA8333578505D9FEA51FB02C92` |
| `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` | `77EC2BA0396EECA17735AB19FA7B01BA795A06B66CBF0FEF5EF47D44A4627347` |
| `Assets/RoadRage/Prefabs/Greybox_CityBlock_A.prefab` | `CBAC3856BF8804213E8EAFB7409BE70C1D7DB297A3507663D29AEEC8D38506A6` (mis a jour 2026-09-16, repurposing decoratif Story 5.10 ci-dessus ; ancien hash `93511888E569388BE1FD073EA564A1D6F0986168C28F4E9E95E0857CE3B0286A`) |

## Mise a jour -- repurposing decoratif Story 5.10 (2026-09-16)

`Assets/RoadRage/Prefabs/Greybox_CityBlock_A.prefab` a ete repris comme habillage decoratif du
district de la Story 5.10 : il porte desormais un ilot complet (trottoirs, places et une tour de
repere) instancie 4 fois dans `MVP_Run` (`Greybox_CityBlock_A_East/_SouthEast/_West/_SouthWest`),
en plus du batiment simple d'origine.

Les nouvelles parties visuelles sont des props **Synty POLYGON - City Pack** (`SM_Env_Sidewalk_*`,
`SM_Bld_OfficeSquare_01`) instancies en enfants visuels sous `Visual_Greybox_CityBlock_A` --
conforme a la regle d'usage des packs Synty (AGENTS.md, AD-13/AD-27) : decor uniquement, aucun
`NetworkObject`, aucun `MeshCollider`, le root gameplay et son unique `BoxCollider` restent
inchanges. Le FBX source de Story 1.4 (`Greybox_CityBlock_A.fbx`) n'a pas ete retouche ; seul le
prefab a grandi.

Consequence sur le contrat scelle de Story 1.4 : les parties reconnaissables `Door`/`Window`/
`RoofCap` du petit batiment ne sont plus garanties sur ce seed precis (les trois autres seeds --
personnages, voiture -- restent inchanges). Le test `GreyboxPrefabsHaveRecognizableObjectParts` et
`GreyboxPrefabBoundsMatchDocumentedRoughScale` ont ete mis a jour en consequence
(`Assets/RoadRage/Tests/EditMode/Story14GreyboxAssetSeedTests.cs`).

Nouvelle mesure Unity (`CalculateRendererBounds`, meme methode que le test) :

| Prefab | Stable id | Mesure Unity | Renderer count | Collider |
| --- | --- | --- | --- | --- |
| `Assets/RoadRage/Prefabs/Greybox_CityBlock_A.prefab` | `building_city_block_a` | `size=(16.000,24.184,16.000)`, `min=(-8.000,-0.184,-8.000)`, `max=(8.000,24.000,8.000)` | 17 | `BoxCollider` root (inchange) |

La base descend a `y = -0.184` (semelle de trottoir enterree pour ne jamais laisser de vide visible
au raccord avec le terrain) : c'est desormais le seul des quatre seeds dont la base n'est pas
exactement au sol.

## Verification

Commandes executees :

- `Unity_RunCommand` titre `Story 1.4 refresh generated assets and scripts` : compilation/import reussis.
- `Unity_RunCommand` titre `Story 1.4 create greybox prefabs and assign CharacterDefs retry` : quatre prefabs crees et deux `CharacterDef` assignes.
- `Unity_RunCommand` titre `Story 1.4 verify greybox prefabs measured retry` : metadata valide, renderers presents, un collider par prefab, aucun `MeshCollider`, aucun `NetworkObject`, parties reconnaissables presentes.
- `Unity_RunCommand` titre `Story 1.4 measured prefab bounds text` : mesures Unity listees ci-dessus.
- `Unity_ReadConsole` Types `Error` apres import : zero erreur.

Limite : aucune scene gameplay n'a ete modifiee et aucun test multijoueur n'a ete execute dans cette story.
