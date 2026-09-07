# VAL-025 partie 1 -- Prop_Barrel

Date : 2026-09-07

Acteur : Agent via Blender MCP, a la demande utilisateur. Cette preuve est une execution assistee MCP de la partie 1 seulement ; elle ne suffisait pas, seule, au passage global de `VAL-025` a `Pass`.

Fichier source Blender : `Assets/RoadRage/ArtSource/Blender/Prop_Barrel.blend`

Statut historique : partie 1 completee pour `Prop_Barrel`. Au moment de cette preuve, `VAL-025` restait `In Progress` tant que les parties 2 a 5 n'etaient pas terminees ; les preuves suivantes documentent ensuite les parties 2 a 5.

Provenance : placeholder derive du cube de smoke test Blender MCP Story 0.5. Le nom `Prop_Barrel` sert a exercer la checklist ; ce n'est pas encore un asset barrel final.

## Action MCP

Outil : `mcp__blender.execute_blender_code`

Scene inspectee avant modification :

- Fichier Blender ouvert : aucun chemin sauvegarde (`bpy.data.filepath` vide).
- Scene en unites metriques (`METRIC`, scale `1.0`).
- Mesh selectionne unique : `Prop_Barrel`.
- Collection initiale : `Collection`.
- Mesh datablock initial : `Cube`.
- Transform initial : location `0,0,0`, rotation `0,0,0`, scale `1,1,1`.
- Dimensions initiales : `2,2,2`.
- Bounds Z initiaux : `-1` a `1`, donc pivot/origine au centre du cube.

Operations executees :

- Objet conserve sous le nom clair `Prop_Barrel`.
- Mesh datablock renomme `Prop_Barrel_Mesh`.
- Collection contenant l'objet renommee `Prop_Barrel_Source`.
- Rotation et echelle appliquees au mesh.
- Origine placee a la base du prop : bounds monde finaux Z `0` a `2`.
- Location finale conservee a `0,0,0`.
- Rotation finale `0,0,0`.
- Scale finale `1,1,1`.
- Fichier sauvegarde sous `Assets/RoadRage/ArtSource/Blender/Prop_Barrel.blend`.

Preuve apres sauvegarde interactive :

- Chemin normalise repo : `Assets/RoadRage/ArtSource/Blender/Prop_Barrel.blend`
- `bpy.data.is_dirty` apres seconde sauvegarde : `false`
- Fichier present sur disque : oui
- Taille du fichier : `95 658` octets
- SHA-256 a la fin de la partie 1, avant execution des parties 2 a 4 : `9A5B45E16457DDEAE0FC11DD84B5D4CF36759A92C7E57B014017740932C42F24`
- Meta Unity generee par import cible de la source `.blend` : `Assets/RoadRage/ArtSource/Blender/Prop_Barrel.blend.meta`
- GUID Unity source : `41919fd127a8e5342a391465fef78685`
- Objet actif/selectionne : `Prop_Barrel`
- Mesh datablock : `Prop_Barrel_Mesh`
- Collection : `Prop_Barrel_Source`
- Dimensions : `2,2,2`
- Bounds monde : min `-1,-1,0`, max `1,1,2`
- Materiau existant conserve : `Cube_Material`

Resultat structure MCP apres sauvegarde :

```json
{
  "blend_filepath": "Assets/RoadRage/ArtSource/Blender/Prop_Barrel.blend",
  "is_dirty_after_resave": false,
  "file_exists": true,
  "file_size_bytes": 95658,
  "object": {
    "name": "Prop_Barrel",
    "data_name": "Prop_Barrel_Mesh",
    "collections": ["Prop_Barrel_Source"],
    "location": [0.0, 0.0, 0.0],
    "rotation_euler_deg": [0.0, 0.0, 0.0],
    "scale": [1.0, 1.0, 1.0],
    "dimensions": [2.0, 2.0, 2.0],
    "world_bounds_min": [-1.0, -1.0, 0.0],
    "world_bounds_max": [1.0, 1.0, 2.0],
    "material_slots": ["Cube_Material"]
  },
  "unit_system": "METRIC",
  "unit_scale": 1.0
}
```

## Limites

La verification Blender en background n'a pas pu etre executee depuis le terminal car l'executable `blender` n'est pas dans le `PATH` de cette session. La preuve ci-dessus vient du Blender interactif connecte au MCP et d'une verification filesystem locale du fichier sauvegarde.

Aucune action des parties 2 a 5 n'a ete effectuee ici : pas de validation d'echelle par reference, pas de reduction materiaux, pas de recalcul normals, pas de nettoyage geometrie, pas d'export FBX/GLB, pas d'import Unity controle sous `ArtExports`, pas de prefab.

Le fichier `.blend` vit sous `Assets/`, donc Unity lui a cree une metadata de source et le voit comme asset importable. Cette importation ciblee sert uniquement a stabiliser le GUID du fichier source ; elle ne remplace pas l'export controle FBX/GLB ni le test d'echelle Unity de l'etape 5.

Note de suivi : apres cette preuve, les parties 2 a 4 ont modifie le fichier source. Le hash courant du `.blend` est documente dans `docs/setup/val-025-prop-barrel-step-2-4-evidence.md`.
