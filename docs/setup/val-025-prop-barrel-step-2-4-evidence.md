# VAL-025 parties 2 a 4 -- Prop_Barrel

Date : 2026-09-07

Acteur : Agent via Blender MCP, a la demande utilisateur.

Fichier source Blender : `Assets/RoadRage/ArtSource/Blender/Prop_Barrel.blend`

Export controle retenu : `Assets/RoadRage/ArtExports/Prop_Barrel.fbx`

Statut historique : parties 2, 3 et 4 completees pour `Prop_Barrel`. Au moment de cette preuve, `VAL-025` restait `In Progress` tant que la partie 5 (import controle en scene Unity, test d'echelle, prefab/collider) n'etait pas terminee ; la partie 5 est documentee ensuite dans `docs/setup/val-025-prop-barrel-step-5-evidence.md`.

## Partie 2 -- Echelle reelle

Reference utilisee : baril acier 200/210 L. Deux fiches produit donnent un diametre `584 mm` et une hauteur `883..890 mm`, donc le placeholder est arrondi a `0.6 m x 0.6 m x 0.9 m`.

Sources consultees :

- https://www.tradeindia.com/products/4-feet-22-kg-200-liter-cylindrical-heavy-duty-mild-steel-drum-8236519.html
- https://msbarrelsindia.com/product-specifications.php

Cette reference suffit pour exercer le pipeline sur un placeholder derive du cube de smoke test. Elle devra etre remplacee par la reference de l'asset final si le futur asset n'est pas un baril 200/210 L standard.

Resultat :

- Units Blender : `METRIC`, scale `1.0`.
- Dimensions avant : `2.0,2.0,2.0`.
- Dimensions apres : `0.6,0.6,0.9`.
- Bounds monde apres : min `-0.3,-0.3,0.0`, max `0.3,0.3,0.9`.
- Location : `0,0,0`.
- Rotation : `0,0,0`.
- Scale : `1,1,1`.

## Partie 3 -- Materiaux

Resultat :

- Material slots avant : `Cube_Material`.
- Material slots apres : `Prop_Barrel_Placeholder_Mat`.
- Nombre de slots final : `1`.
- Toutes les faces pointent vers le slot `0`.
- Aucun asset material Unity n'est cree sous `Assets/RoadRage/Materials/` pendant les parties 2 a 4 ; l'extraction ou remap Unity reste a traiter avec la validation d'import/prefab de la partie 5 si necessaire.

## Partie 4 -- Normals, geometrie, export

Nettoyage execute :

- Normals recalculees vers l'exterieur.
- Verification numerique des normals : dot minimum normal/centre-vers-face `0.3`, toutes les valeurs positives (`0.45, 0.3, 0.3, 0.45, 0.3, 0.3`).
- Vertices avant nettoyage : `8`.
- Edges avant nettoyage : `12`.
- Faces avant nettoyage : `6`.
- Vertices apres nettoyage : `8`.
- Edges apres nettoyage : `12`.
- Faces apres nettoyage : `6`.
- Loose vertices supprimes : `0`.
- Loose edges supprimes : `0`.
- Faces supprimees : `0`.
- Aucune geometrie de collider gameplay n'existait pour ce placeholder neuf.

Export :

- Format retenu : FBX, parce que Unity a confirme l'asset FBX comme `UnityEngine.GameObject`.
- Option d'export : objet selectionne uniquement.
- Objets selectionnes a l'export : `Prop_Barrel`.
- Resultat operateur Blender : `FINISHED`.
- Fichier exporte : `Assets/RoadRage/ArtExports/Prop_Barrel.fbx`.
- Taille FBX a la fin de la partie 4, avant correction d'axe Unity en partie 5 : `15 308` octets.
- SHA-256 FBX a la fin de la partie 4, avant correction d'axe Unity en partie 5 : `73A2EBC8D31B609AEABC742949B7D99B2E40C6499F99F51B8F6668C274E984D8`.
- Meta Unity FBX : `Assets/RoadRage/ArtExports/Prop_Barrel.fbx.meta`.
- GUID Unity FBX : `3acd84950af58ce4b96e861d6773f0a2`.
- SHA-256 `.fbx.meta` a la fin de la partie 4, avant correction d'axe Unity en partie 5 : `CE4F6455A6C7BCBA95D08931D4020F356E1F4A621529DB61FF56FBEB06144EFC`.
- Type Unity apres import metadata : `UnityEngine.GameObject`.
- Reglages importeur statique verrouilles : `importAnimation=0`, `animationType=0`, `importBlendShapes=0`, `importCameras=0`, `importLights=0`, `importPhysicalCameras=0`, `addColliders=0`.
- Console Unity apres import metadata : `0` erreur ; warnings existants limites a la collecte de signature MCP avec chemins caviardes `[REDACTED_TOKEN]`.

Resultat structure Unity apres import metadata du FBX :

```json
{
  "path": "Assets/RoadRage/ArtExports/Prop_Barrel.fbx",
  "guid": "3acd84950af58ce4b96e861d6773f0a2",
  "assetType": "UnityEngine.GameObject",
  "name": "Prop_Barrel",
  "fileName": "Prop_Barrel.fbx"
}
```

Controle contenu export :

- Le FBX contient `Prop_Barrel`.
- Le FBX contient `Prop_Barrel_Mesh`.
- Le FBX contient `Prop_Barrel_Placeholder_Mat`.
- Le FBX ne contient pas `Prop_Sphere` ni `Light`.
- La seule occurrence `Camera` detectee est la metadata FBX `DefaultCamera` / `Producer Perspective`, pas un objet de scene exporte.

Note : un export GLB temporaire a ete genere puis retire de ce lot, car Unity l'a importe comme `DefaultAsset` dans cette configuration. Le livrable controle de la partie 4 est donc le FBX. La partie 5 a ensuite remplace ce FBX par un export final corrige pour l'axe Unity ; les hashes finaux sont documentes dans `docs/setup/val-025-prop-barrel-step-5-evidence.md`.

Preuve de nettoyage GLB : `Assets/RoadRage/ArtExports/Prop_Barrel.glb` et `Assets/RoadRage/ArtExports/Prop_Barrel.glb.meta` sont absents apres nettoyage.

Preuve chemin local FBX : le FBX ne contient plus le chemin absolu local du projet, ni chemin utilisateur Windows. La metadata FBX conserve seulement `[PROJECT_ROOT]\Assets\RoadRage\ArtSource\Blender\Prop_Barrel.blend`.

## Hashes source courants

- Taille `.blend` courant apres parties 2 a 4 : `94 730` octets.
- Derniere sauvegarde `.blend` observee : 2026-09-07 14:19:12 heure locale.
- `bpy.data.is_dirty` apres sauvegarde/export : `false`.
- SHA-256 `.blend` courant apres parties 2 a 4 : `2DF8F39B8631D816DF13815F2BB8C9B7D9210C76651EC7B88800BF9791F1112A`.
- SHA-256 `.blend.meta` : `3B2E035AF8C8DE35DA7D195A9015A20E5EACE4051C4965224340349ADAF6B0A4`.

## Limites

Aucune action de partie 5 n'avait ete effectuee dans ce lot. La partie 5 a ete executee ensuite et documentee dans `docs/setup/val-025-prop-barrel-step-5-evidence.md`.
