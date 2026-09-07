# Tutoriel Story 0.6 : Pipeline d'intake Blender et assets 3D

Ce tutoriel est le chemin manuel unique pour executer la Story 0.6. Il distille la checklist 9 etapes deja actee dans `mcp-tooling-setup.md` (nettoyage Blender -> export -> import controle -> prefab) en 5 etapes numerotees, avec les dossiers source/export/prefab exacts, la regle de gate AD-13, la regle de stabilite prefab AD-27, et un plan collider explicite pour tout asset entierement nouveau. Reprend le mesh brouillon `Blender/exports_test/scene_test.glb` (Story 0.5) comme exemple travaille, documente honnetement comme non converti. L'agent relit les preuves apres coup ; il ne fait pas le nettoyage Blender ou l'import Unity a ta place, et ne marque aucune ligne `Pass` sans preuve reelle fournie par toi.

**Principe (rappel `mcp-tooling-setup.md`) :** tout asset 3D genere par IA ou telecharge doit passer par Blender avant tout usage prefab gameplay dans Unity. Tant que la checklist n'est pas suivie de bout en bout, tout mesh reste un brouillon.

## Sources consultees

- Checklist source en 9 etapes ("Asset Generation and Intake") : `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/mcp-tooling-setup.md:218-238`. La section suivante, "Setup Order" (`:239-247`), documente un contenu different -- l'ordre d'installation des outils (Unity, MCP, Netcode, Blender, checklist intake, gameplay) -- et n'est pas une partie de la checklist de nettoyage d'asset ; ne pas la citer comme source de la checklist.
- AD-13 (Blender Intake Gate For 3D Assets) : `ARCHITECTURE-SPINE.md:152-156`.
- AD-27 (Greybox-To-Art Prefab Stability) : `ARCHITECTURE-SPINE.md:236-240`.
- Arborescence `ArtSource/`, `ArtExports/`, `Materials/`, `Prefabs/`, `ScriptableObjects/` : `ARCHITECTURE-SPINE.md:311-317`.
- Patron de structure reutilise : `docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md`.
- AC source de cette story : `_bmad-output/planning-artifacts/epics.md:389-403` (Story 0.6) -- exige explicitement que la checklist documente source, export, test d'echelle, plan collider, destination prefab et politique de remplacement ; le "Plan collider" ci-dessous et la section AD-27 couvrent ce dernier point.
- Mesh brouillon existant, cree pendant le smoke test Blender MCP Story 0.5 : `Blender/exports_test/scene_test.glb` (voir Etape 8 du tutoriel Story 0.5).

## Avant de commencer

- Utilise uniquement les statuts `Not Started`, `In Progress`, `Pass`, `Blocked`, `Not Applicable`.
- Caviarde tout identifiant sensible (chemin utilisateur, token, credential) qui apparaitrait dans une capture ou une note avec `[REDACTED_TOKEN]`. Ne colle jamais de secret dans un fichier Blender, une capture ou ce tutoriel.
- Le nettoyage Blender et l'import Unity restent des actions manuelles que tu realises toi-meme ; l'agent guide les etapes et verifie les preuves apres coup.
- **Demander d'abord** avant de : convertir reellement `scene_test.glb` (ou tout autre mesh) en prefab Unity commite dans le projet ; definir un standard de nommage ou une convention de collider au-dela de ce que ce tutoriel et AD-13/AD-27 couvrent deja.
- **Jamais :** importer un asset 3D genere ou telecharge dans Unity comme prefab gameplay sans etre passe par la checklist complete ci-dessous ; marquer `VAL-025` ou les lignes 0.6 du readiness checklist `Pass` sans preuve utilisateur reproductible ; traiter un mesh Blender comme final tant que la checklist n'a pas ete suivie de bout en bout.

## Dossiers exacts

| Role | Dossier |
| --- | --- |
| Source Blender (fichiers `.blend`) | `Assets/RoadRage/ArtSource/Blender/` |
| References generees (images/concepts, pas de geometrie) | `Assets/RoadRage/ArtSource/GeneratedReferences/` |
| Export controle (FBX/GLB importes dans Unity) | `Assets/RoadRage/ArtExports/` |
| Materiaux Unity crees/ajustes a l'import (Etape 3) | `Assets/RoadRage/Materials/` |
| Prefabs convertis apres validation | `Assets/RoadRage/Prefabs/` |

Ces cinq dossiers existent deja (scaffold Story 0.4), vides a ce jour. Un dossier scratch/test hors projet (par exemple `Blender/exports_test/`, ignore par `.gitignore`) reste valable uniquement pour un export de test jetable, jamais comme source `.blend` de reference -- le fichier `.blend` source doit exister sous `Assets/RoadRage/ArtSource/Blender/` (ou un chemin documente equivalent) avant qu'un asset ne soit considere pret pour l'export controle.

## Checklist 9 etapes (source `mcp-tooling-setup.md:218-238`)

1. Renommer les objets et collections clairement.
2. Appliquer les transforms.
3. Definir une echelle reelle.
4. Reduire le nombre de material slots.
5. Verifier les normals.
6. Retirer la geometrie invisible ou excessive.
7. Exporter en `.fbx` ou `.glb`.
8. Importer dans Unity sous un dossier d'assets controle.
9. Convertir en prefab seulement apres test d'echelle en scene.

Ce tutoriel regroupe ces 9 points en 5 etapes numerotees ci-dessous (Etape 1 = points 1-2, plus le pivot/origine et la sauvegarde du fichier `.blend` source -- deux actions necessaires non listees separement dans la checklist source mais gatees par le Cas limite de l'Etape 1 ; Etape 2 = point 3 ; Etape 3 = point 4 ; Etape 4 = points 5-7 ; Etape 5 = points 8-9).

## AD-13 -- Blender Intake Gate For 3D Assets

**Regle (`ARCHITECTURE-SPINE.md:152-156`) :** tout asset 3D genere par IA ou telecharge doit etre nettoye dans Blender, sauvegarde avec son fichier source, exporte en FBX ou GLB, teste a l'echelle dans Unity, puis converti en prefab. Cette regle empeche un asset genere d'entrer en gameplay avec une echelle, des noms, des transforms, des materiaux, des normals ou une geometrie casses. Ce tutoriel est l'application concrete de cette gate -- aucune etape n'est optionnelle pour un asset destine au gameplay.

## AD-27 -- Greybox-To-Art Prefab Stability

**Regle (`ARCHITECTURE-SPINE.md:236-240`) :** l'identite du prefab gameplay, la registration `NetworkObject`, les composants gameplay, les colliders et les definition ids restent stables quand l'art s'ameliore. Les meshes et materiaux finaux remplacent des objets de rendu enfants ou des variantes de prefab apres validation multijoueur ; les colliders gameplay-critiques restent authored separement des meshes decoratifs.

En pratique : quand un mesh Blender nettoye remplace un placeholder greybox deja en jeu (deja spawn, deja reference par `NetworkObject`, deja dote de colliders et de composants gameplay), seul le mesh/materiau de rendu change. Le prefab garde le meme nom, la meme registration reseau, les memes composants gameplay, les memes colliders et les memes definition ids -- rien d'autre ne bouge.

## Plan collider

**Pour un asset entierement nouveau (pas de remplacement d'art existant) :** definir des colliders simples (box/capsule/sphere selon la forme), separes de la geometrie de rendu, dimensionnes a l'objet -- jamais un mesh collider gameplay derive de la geometrie de rendu detaillee. Documenter ce choix dans les notes du prefab (quel type de collider, pourquoi). Ce tutoriel n'impose pas de standard de nommage ou de forme fixe au-dela de ce principe general -- une convention plus precise reste "Demander d'abord", hors perimetre de cette story.

**Pour un remplacement d'art existant (mesh Blender nettoye qui remplace un placeholder greybox deja en jeu) :** appliquer uniquement AD-27 ci-dessus -- les colliders existants restent stables, le nouveau mesh ne les remplace pas.

**Note nettoyage Blender (etape geometrie/normals, voir Etape 4 point 2 ci-dessous) :** avant de supprimer une geometrie jugee invisible ou excessive, verifier qu'elle ne sert pas deja de reference/base a un collider gameplay en place (cas remplacement d'art uniquement). Cette verification evite qu'un collider fonctionnel soit supprime par erreur en le confondant avec de la geometrie decorative.

## Etape 1 -- Renommage, pivot/origine et transforms (VAL-025 partie 1)

1. Renomme clairement les objets et collections Blender (nom lisible, pas de nom par defaut type `Cube.003`).
2. Verifie et repositionne le pivot/origine de l'objet (`Object > Set Origin`) a un point utile pour Unity (base de l'objet pour un prop au sol, centre pour un objet suspendu/tenu).
3. Applique les transforms (`Object > Apply > All Transforms`) pour que l'echelle, la rotation et la position locales repartent d'un etat propre (scale `1,1,1`, rotation `0,0,0`).
4. Sauvegarde le fichier `.blend` source sous `Assets/RoadRage/ArtSource/Blender/` (ou un chemin documente equivalent).

**Preuve a fournir (VAL-025 partie 1) :** capture ou note montrant le nommage clair, le pivot/origine choisi et les transforms appliquees, **et** la confirmation que le fichier `.blend` existe reellement au chemin documente (`Assets/RoadRage/ArtSource/Blender/` ou equivalent) -- pas seulement le nommage/pivot/transforms a l'ecran.

**Cas limite -- fichier `.blend` jamais sauvegarde :** si la session Blender n'a jamais ete sauvegardee en fichier `.blend` (aucun chemin disque reel), l'etape 1 n'est pas complete quel que soit l'etat visuel de la scene. Ne poursuis pas aux etapes suivantes (echelle, materiaux, export) tant que ce fichier n'existe pas reellement sur disque au chemin documente -- un export ou un import fonde sur une scene jamais sauvegardee n'a pas de source tracable.

## Etape 2 -- Echelle reelle (VAL-025 partie 2)

1. Compare les dimensions de l'objet a une reference reelle connue (par exemple une porte de 2 m, un vehicule existant de taille connue). **Repli si aucune reference reelle directe n'existe** (asset stylise, prop fictif) : compare plutot l'objet a un asset gameplay deja valide de taille connue dans le projet (par exemple un des primitives greybox deja en scene), et documente cette comparaison relative dans la preuve.
2. Ajuste l'echelle dans Blender si necessaire, puis reapplique les transforms (retour a l'Etape 1 point 3) si un changement d'echelle a ete fait apres la premiere application.

**Preuve a fournir (VAL-025 partie 2) :** capture ou note montrant la dimension mesuree (ou estimee par comparaison) de l'objet et la reference utilisee (reelle ou asset gameplay deja valide).

**Cas limite -- aucune reference d'echelle disponible :** si ni une reference reelle ni un asset gameplay deja valide de taille comparable n'existe, marque cette partie `Blocked`, documente la meilleure estimation disponible et la raison de l'absence de reference, et ne convertis pas l'asset en prefab tant que l'echelle n'est pas confirmee au moins par comparaison relative.

## Etape 3 -- Materiaux (VAL-025 partie 3)

1. Reduis le nombre de material slots au minimum necessaire (fusionne les materiaux redondants, evite un slot par face).
2. Verifie que chaque materiau restant est nomme clairement et assigne a la bonne partie du mesh.

**Preuve a fournir (VAL-025 partie 3) :** capture ou note listant les material slots restants et leur nom.

**Cas limite -- objet residuel sans materiau :** si un objet de scene sans materiau assigne (par exemple un objet cree pendant un test anterieur) est visible dans la meme scene Blender, ne l'inclus pas dans l'export tant qu'il n'a pas ete nettoye ou explicitement exclu (voir aussi Etape 4 point 3, export des objets selectionnes uniquement).

## Etape 4 -- Normals, geometrie et export (VAL-025 partie 4)

1. Verifie les normals (`Mesh > Normals > Recalculate Outside` en Edit Mode, ou equivalent) ; corrige toute face inversee visible en shading.
2. Retire la geometrie invisible ou excessive (faces internes non vues, doublons de vertices, detail hors champ de camera). **Avant suppression**, verifie que la geometrie visee ne sert pas deja de reference/base a un collider gameplay en place -- cette verification ne s'applique qu'a un remplacement d'art existant, ou le mesh source sert deja de reference a un collider deja en place (regle AD-27). Elle ne s'applique jamais a un asset entierement nouveau : pour un asset neuf, le Plan collider ci-dessus interdit deja de deriver un collider gameplay de la geometrie de rendu, donc aucune geometrie de rendu n'est jamais "la base d'un futur collider" dans ce cas -- les deux sections ne se contredisent pas, elles couvrent deux cas distincts (remplacement vs asset neuf).
3. Exporte en `.fbx` ou `.glb`. **Limite l'export aux objets prevus** en utilisant l'option de selection de l'exporteur choisi (`Selected Objects Only` en FBX, `Include > Limit to > Selected Objects` en glTF/GLB) pour eviter qu'un objet residuel de la scene (comme un objet de test laisse par une session anterieure) se retrouve dans le fichier exporte.

**Preuve a fournir (VAL-025 partie 4) :** capture ou note montrant les normals corrigees, la geometrie retiree (avec confirmation qu'aucun collider gameplay n'a ete perdu par erreur), et le fichier exporte avec la liste des objets qu'il contient reellement.

**Cas limite -- export :** si le fichier exporte contient un objet non prevu, reexporte apres avoir corrige la selection avant de continuer a l'Etape 5 -- ne corrige pas l'objet apres coup dans Unity.

**Cas limite -- normals/geometrie :** si une face inversee ou un doublon de vertex persiste apres correction visible en shading Unity a l'Etape 5, reviens corriger la source Blender plutot que de compenser dans Unity (materiau/shader), pour garder le fichier source et l'export coherents.

## Etape 5 -- Import controle, test d'echelle et prefab (VAL-025 partie 5)

1. Importe le fichier `.fbx`/`.glb` dans Unity sous `Assets/RoadRage/ArtExports/`.
2. Place l'objet importe dans une scene de test (ou la scene courante) a cote d'un objet de taille connue (une primitive greybox deja en jeu, ou l'unite `1 m` de la grille Unity) pour verifier l'echelle et l'orientation visuellement.
3. Convertis l'objet en prefab sous `Assets/RoadRage/Prefabs/` seulement apres validation de l'echelle -- jamais avant.

**Preuve a fournir (VAL-025 partie 5) :** capture ou note montrant l'import sous `Assets/RoadRage/ArtExports/`, la comparaison d'echelle en scene, la conversion en prefab sous `Assets/RoadRage/Prefabs/` (ou la decision explicite de ne pas convertir si l'echelle/l'orientation n'est pas encore satisfaisante), **et** le choix de collider applique documente dans les notes du prefab conformement au "Plan collider" ci-dessus (formes utilisees et pourquoi, ou confirmation que les colliders existants restent inchanges pour un remplacement d'art).

**Cas limite -- import (echelle/axe) :** un decalage d'echelle ou d'orientation (axe vertical invers ou objet couche) peut apparaitre a l'import, que le format choisi soit FBX ou GLB -- ce n'est pas un probleme propre a FBX : un export GLB peut presenter le meme risque via des conventions d'axe glTF differentes de celles de Blender. Quel que soit le format, corrige a la source dans Blender (Etape 1, transforms appliquees et pivot/origine) puis reexporte, plutot que de compenser uniquement par une echelle/rotation manuelle sur l'objet importe dans Unity.

## Exemple travaille -- `scene_test.glb`

`Blender/exports_test/scene_test.glb` est le mesh brouillon cree pendant le smoke test Blender MCP de la Story 0.5 : un objet `Cube` avec le materiau `Cube_Material` assigne, plus un `Prop_Sphere` residuel sans materiau issu d'un test Blender MCP anterieur dans la meme session (voir VAL-024 dans `tooling-validation-log.md`).

En appliquant la checklist ci-dessus a cet exemple :

- **Etape 1 (rename/pivot/transforms) :** `Cube` et `Prop_Sphere` ont des noms utilisables mais pas descriptifs du gameplay ; a renommer avant tout usage reel. **Le fichier `.blend` source de cette session n'a jamais ete sauvegarde** -- la session Blender qui a cree `Cube` et `Prop_Sphere` est restee ouverte sans `Ctrl+S` depuis sa creation (2026-09-07, voir VAL-024). Il n'existe donc aucun fichier `.blend` a `Assets/RoadRage/ArtSource/Blender/` ni ailleurs pour cet exemple. Consequence directe du Cas limite Etape 1 : cet exemple **ne peut pas** poursuivre au-dela de l'Etape 1 tant qu'un fichier `.blend` reel n'est pas sauvegarde. Le dossier scratch/test `Blender/exports_test/` (ignore par `.gitignore`) ne documente que l'existence d'un export `.glb` de test jetable, jamais une source `.blend` -- cette reference scratch/test ne compense pas l'absence de fichier source.
- **Etapes 2 a 5 :** non applicables tant que l'Etape 1 n'est pas satisfaite. `Prop_Sphere` illustre par ailleurs concretement le Cas limite Etape 3/Etape 4 point 3 : un objet residuel sans materiau qui doit etre exclu (`Selected Objects Only`) ou nettoye avant tout export de production futur reutilisant cette scene.

`scene_test.glb` reste donc un **brouillon non converti** : aucune conversion en prefab Unity n'est faite ni prevue depuis ce tutoriel. Convertir reellement ce mesh (ou tout autre) en prefab commite reste "Demander d'abord".

## Rappel de la regle de caviardage

Reutilise la regle de caviardage `[REDACTED_TOKEN]` des Stories 0.3/0.5 pour tout chemin utilisateur sensible, token ou identifiant de compte visible dans une capture, un nom de fichier ou une note liee a l'intake Blender. Ne colle jamais de secret original dans ce tutoriel ni dans `tooling-validation-log.md`.

## Mettre a jour les documents de suivi

Apres chaque vraie action manuelle ou chaque bloqueur, mets a jour dans la meme passe coherente :

- `docs/setup/tooling-validation-log.md` -- `VAL-025` (ligne principale et note datee dans "Notes de validation"), statut `Not Started`, `In Progress` ou `Blocked` selon la preuve reellement fournie, jamais `Pass` sans preuve utilisateur reproductible pour les 5 parties ci-dessus.
- `docs/setup/epic-0-readiness-checklist.md` -- lignes Story 0.6 (action manuelle et validation agent), synchronisees avec le statut reel de `VAL-025` et de ce tutoriel, sans jamais employer un temps accompli ("preuve fournie") sur une ligne non `Pass`.
- `_bmad-output/implementation-artifacts/sprint-status.yaml` -- avancer Story 0.6 uniquement quand le tutoriel et la synchro ci-dessus sont prets.

Une ligne `VAL-025` passe en `Pass` seulement si, pour chacune des 5 parties, date, acteur, commande/chemin UI, chemin/resume de preuve caviardee, validateur et resultat sont tous presents et re-verifiables.

## Arret obligatoire avant Story 0.7

Arrete-toi ici apres preparation du tutoriel et synchro des documents de suivi. Ne convertis pas reellement `scene_test.glb` (ou tout autre mesh) en prefab Unity commite depuis cette story. Ne commence pas Story 0.7 (registre d'adoption add-on), Story 0.8 (smoke tests finaux), ni le gameplay Epic 1 depuis cette story.
