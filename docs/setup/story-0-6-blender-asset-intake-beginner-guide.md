# Guide debutant -- Intake Blender pas a pas (complement Story 0.6)

Ce guide est un **complement** au tutoriel officiel [`story-0-6-blender-asset-intake-tutorial.md`](story-0-6-blender-asset-intake-tutorial.md). Il ne remplace rien : les statuts, VAL-025 et les regles ("Demander d'abord", "Jamais") restent definis par le tutoriel officiel. Ce document explique uniquement **ou cliquer, quoi taper, a quoi ca ressemble** pour quelqu'un qui n'a jamais ouvert Blender ni Unity.

Version cible : Blender `5.2 LTS`, Unity `6000.6.0f1` (deja confirmes Story 0.5).

---

## 0. Avant l'Etape 1 -- se reperer dans Blender

Blender fait peur au premier lancement parce que l'ecran est dense. Voici les 5 zones qui comptent pour ce tutoriel :

| Zone | Ou la trouver | A quoi elle sert ici |
| --- | --- | --- |
| **Viewport 3D** | Grande zone centrale avec le cube par defaut | La ou tu vois et manipules les objets |
| **Outliner** | Panneau en haut a droite, liste en arborescence | Renommer objets/collections, voir la hierarchie de la scene |
| **Properties (Proprietes)** | Panneau en bas a droite, colonne d'icones verticale a gauche de ce panneau | Chaque icone = un onglet different (Objet, Materiau, Scene, etc.) |
| **Header du viewport** | Bande horizontale tout en haut du viewport, menus `File / Edit / Render / Window / Help` puis `Object / Add / ...` selon le mode | Menus `Object`, `Mesh`, mode Object/Edit |
| **N-panel** | Cache par defaut ; touche `N` clavier avec la souris survolant le viewport pour l'afficher/masquer | Onglet `Item` = position/rotation/echelle/dimensions de l'objet selectionne |

**Souris et clavier de base (keymap par defaut Blender 2.8+, valable en 5.2) :**

- **Clic gauche** = selectionner un objet (clique sur l'objet dans le viewport, ou sur son nom dans l'Outliner).
- **Clic droit** = menu contextuel.
- **Molette** = zoom avant/arriere.
- **Clic molette (appuyer et glisser)** = orbiter la vue.
- **Maj (Shift) + clic molette + glisser** = deplacer la vue (pan).
- **`N`** (souris sur le viewport) = ouvrir/fermer le panneau lateral avec les valeurs Position/Rotation/Echelle.
- **`Tab`** (objet selectionne) = basculer entre Mode Objet et Mode Edition.
- **`Ctrl+S`** = sauvegarder le fichier `.blend`.

Aucune de ces bases n'est necessaire pour comprendre le reste -- reviens ici si un terme n'est pas clair plus bas.

---

## Etape 1 -- Renommage, pivot/origine, transforms, sauvegarde

*(Correspond a l'Etape 1 du tutoriel officiel, VAL-025 partie 1)*

### 1.1 Renommer un objet

1. Dans l'**Outliner** (panneau en haut a droite), **double-clique** sur le nom de l'objet (ex. `Cube`).
2. Le nom devient editable (fond de texte visible). Tape le nouveau nom (ex. `Prop_Barrel`), appuie sur `Entree`.
3. Fais pareil pour la **collection** qui contient l'objet (la ligne au-dessus avec une icone de dossier/boite orange, souvent nommee `Collection` par defaut).

### 1.2 Positionner le pivot/origine

Le "pivot" (Blender dit "origine") est le point autour duquel l'objet tourne et qui sert de reference de position dans Unity. Pour un objet pose au sol (ex. une caisse), on veut l'origine **a la base** de l'objet, pas au centre.

1. **Selectionne l'objet** (clic gauche dessus dans le viewport).
2. Deplace le curseur 3D (le petit repere rouge/blanc en forme de mire) a l'endroit voulu :
   - Pour le mettre au centre du monde : touche `Maj+C` (reinitialise le curseur 3D a l'origine du monde et recentre la vue).
   - Pour le placer a un point precis de l'objet (ex. sous la caisse) : `Maj + clic droit` a cet endroit dans le viewport.
3. Dans le header du viewport, ouvre le menu **`Object`** (Mode Objet uniquement) > **`Set Origin`** > choisis :
   - **`Origin to 3D Cursor`** = l'origine part exactement ou tu as place le curseur 3D (le cas le plus courant pour un prop au sol : place le curseur a la base avant).
   - **`Origin to Geometry`** = l'origine se recalcule automatiquement au centre de la geometrie de l'objet (pratique pour un objet suspendu/tenu en main, ou "centre" suffit).

### 1.3 Appliquer les transforms

"Appliquer les transforms" = figer les valeurs actuelles de position/rotation/echelle dans la geometrie elle-meme, pour repartir d'un etat propre (`Scale 1,1,1`, `Rotation 0,0,0`). Sans ca, Unity peut recevoir un objet avec une echelle bizarre du genre `0.734` au lieu de `1`.

1. Selectionne l'objet.
2. Ouvre le panneau `N` (touche `N`, souris sur le viewport) et clique l'onglet **`Item`** pour voir les valeurs actuelles `Location / Rotation / Scale`.
3. Menu header **`Object`** > **`Apply`** > **`All Transforms`** (raccourci : `Ctrl+A` puis choisir `All Transforms` dans le petit menu qui apparait a l'endroit du curseur).
4. Verifie dans le panneau `N` : `Scale` doit maintenant afficher `1.000 / 1.000 / 1.000` et `Rotation` `0° / 0° / 0°`. La `Location` peut rester non-nulle si l'objet n'est pas au centre du monde -- c'est normal.

### 1.4 Sauvegarder le fichier `.blend` au bon endroit

C'est l'action qui bloque tout le reste tant qu'elle n'est pas faite (voir "Cas limite" du tutoriel officiel).

1. Menu **`File`** (tout en haut a gauche) > **`Save As...`** (raccourci `Ctrl+Maj+S`).
2. Dans la fenetre qui s'ouvre, en haut il y a une barre de chemin (breadcrumb). Tu peux soit cliquer sur les dossiers un par un, soit **taper le chemin complet directement** dans le champ de nom de fichier en haut, par exemple :
   ```
   D:\Projets\RRS\Assets\RoadRage\ArtSource\Blender\Prop_Barrel.blend
   ```
3. Dans le champ **`File Name`** en bas, mets un nom clair et descriptif (ex. `Prop_Barrel.blend`), pas `untitled.blend`.
4. Clique **`Save As Blender File`** (bouton bleu en bas a droite de la fenetre).
5. Verifie ensuite dans l'Explorateur Windows (ou dans Blender via `File > Open` a nouveau) que le fichier existe bien a `Assets\RoadRage\ArtSource\Blender\`.

**Preuve a capturer pour VAL-025 partie 1 :** une capture d'ecran de l'Outliner montrant le nom clair de l'objet/collection, une capture du panneau `N` montrant `Scale 1,1,1` / `Rotation 0,0,0` apres application, et soit une capture de l'Explorateur Windows montrant le fichier `.blend` au bon chemin, soit le titre de la fenetre Blender (qui affiche le chemin du fichier ouvert) apres `Save As`.

---

## Etape 2 -- Echelle reelle

*(Correspond a l'Etape 2 du tutoriel officiel, VAL-025 partie 2)*

### 2.1 Verifier que Blender est en unites metriques

1. Panneau **Properties** (bas-droit), clique l'icone **Scene Properties** (icone ressemblant a un cone + sphere + lumiere, generalement 4e ou 5e icone en partant du haut de la colonne).
2. Section **`Units`** : `Unit System` doit etre sur `Metric`, `Unit Scale` sur `1.00`. C'est le reglage par defaut de Blender -- verifie juste que personne n'a change.

### 2.2 Lire les dimensions de l'objet

1. Selectionne l'objet.
2. Panneau `N` (touche `N`) > onglet `Item` > section **`Dimensions`** : affiche la taille en metres sur X/Y/Z (1 unite Blender = 1 metre par defaut).

### 2.3 Comparer a une reference

- **Si tu as une reference reelle connue** (porte standard ~2 m de haut, voiture ~1.8 m de large) : compare mentalement ou en ajoutant temporairement un objet de reference dans la scene (`Maj+A` dans le viewport > `Mesh` > `Cube`, puis ajuste son echelle a la taille connue pour comparer visuellement).
- **Si pas de reference reelle** (asset stylise) : compare a un objet greybox deja valide dans le projet Unity. Ouvre Unity, selectionne ce greybox dans la Hierarchy, regarde son `Scale`/ses dimensions dans l'Inspector (composant `Transform`, ou `Mesh Renderer > Bounds`), et compare a la dimension lue dans Blender.

### 2.4 Ajuster l'echelle si necessaire

1. Selectionne l'objet, touche `S` (scale), tape une valeur numerique (ex. `1.5`) puis `Entree` -- ou modifie directement les champs `Scale X/Y/Z` dans le panneau `N`.
2. **Important** : si tu changes l'echelle ici, retourne a l'Etape 1.3 et refais `Ctrl+A > All Transforms` pour re-figer le `Scale` a `1,1,1`.

**Preuve a capturer pour VAL-025 partie 2 :** capture du panneau `N` (section `Dimensions`) + capture ou note de la reference utilisee (mesure reelle ou capture Unity du greybox comparatif) avec les deux valeurs cote a cote.

---

## Etape 3 -- Materiaux

*(Correspond a l'Etape 3 du tutoriel officiel, VAL-025 partie 3)*

1. Selectionne l'objet.
2. Panneau **Properties** > icone **Material Properties** (icone ronde a damier rouge/blanc, generalement en bas de la colonne d'icones).
3. Tu vois une liste de "material slots". Chaque ligne = un materiau assigne a une partie du mesh.
4. **Supprimer les slots inutiles** :
   - Selectionne le slot inutile dans la liste, clique le bouton **`-`** sous la liste pour le retirer.
   - Ou : clique la petite fleche vers le bas a cote des boutons `+`/`-` > **`Remove Unused Slots`** (retire automatiquement tous les slots non utilises par la geometrie).
5. **Renommer un materiau** : double-clique sur le nom du materiau dans le champ juste sous la liste des slots (la ou c'est ecrit ex. `Material.002`), tape un nom clair (ex. `Barrel_Metal`), `Entree`.
6. **Fusionner deux materiaux redondants** (optionnel, si tu as par erreur 2 materiaux identiques) : passe en Mode Edition (`Tab`), selectionne les faces concernees (clic, ou `A` pour tout selectionner), clique sur le slot du materiau que tu veux garder dans le panneau Material Properties, puis bouton **`Assign`** (visible uniquement en Mode Edition, sous la liste des slots). Repasse en Mode Objet (`Tab`), puis refais `Remove Unused Slots`.

**Preuve a capturer pour VAL-025 partie 3 :** capture du panneau Material Properties montrant la liste finale des slots avec leurs noms.

---

## Etape 4 -- Normals, geometrie, export

*(Correspond a l'Etape 4 du tutoriel officiel, VAL-025 partie 4)*

### 4.1 Verifier/corriger les normals

Les "normals" determinent quelle face d'un polygone est visible (l'exterieur). Une normal inversee fait apparaitre une face noire ou transparente.

1. Selectionne l'objet, `Tab` pour passer en **Mode Edition**.
2. Touche `A` pour tout selectionner (tous les sommets deviennent orange).
3. Menu header **`Mesh`** (visible seulement en Mode Edition) > **`Normals`** > **`Recalculate Outside`** (raccourci : `Maj+N`).
4. Pour **voir visuellement** les normals inversees : clique la petite fleche vers le bas en haut a droite du viewport (icone deux cercles superposes, menu **`Overlays`**), section **`Geometry`**, coche **`Face Orientation`**. Les faces bien orientees apparaissent bleues, les faces inversees apparaissent rouges. Decoche apres verification (c'est juste un outil visuel temporaire).
5. `Tab` pour revenir en Mode Objet quand c'est fini.

### 4.2 Retirer la geometrie inutile

1. `Tab` pour repasser en Mode Edition.
2. Selectionne les faces/sommets a supprimer :
   - Clic simple sur une face (en mode de selection faces -- 3e icone en haut a gauche du viewport en Mode Edition, ou touche `3`).
   - `B` puis glisser un rectangle pour selectionner plusieurs elements (box select).
   - Survole une partie isolee de geometrie et appuie sur `L` pour selectionner tout l'ilot connecte.
3. **Avant de supprimer**, verifie que ce n'est pas un remplacement d'art existant qui utilise cette geometrie comme base de collider (voir "Plan collider" du tutoriel officiel -- ne s'applique pas a un asset entierement nouveau).
4. Touche `X` (ou `Suppr`/`Delete`) > choisis dans le menu **`Faces`** (ou `Vertices`/`Edges` selon le mode de selection).
5. `Tab` pour revenir en Mode Objet.

### 4.3 Exporter en FBX ou GLB

1. **Selectionne uniquement les objets a exporter** dans le viewport (clic sur le premier, `Maj+clic` pour ajouter les suivants a la selection). Verifie qu'aucun objet residuel non voulu n'est selectionne.
2. Menu **`File`** > **`Export`** > **`FBX (.fbx)`** ou **`glTF 2.0 (.glb/.gltf)`**.
3. Dans la fenetre d'export, un panneau d'options apparait sur la droite :
   - **Pour FBX** : section **`Include`** > coche **`Selected Objects`** (sinon Blender exporte TOUTE la scene, meme les objets non selectionnes).
   - **Pour glTF/GLB** : section **`Include`** > **`Limit to`** > coche **`Selected Objects`**.
4. Choisis le format de fichier : `.glb` = un seul fichier tout-en-un (recommande, plus simple) ; `.gltf` = separe en plusieurs fichiers (texture/json).
5. Choisis le dossier de destination et le nom du fichier dans la barre du haut de la fenetre, puis clique **`Export FBX`** / **`Export glTF 2.0`** (bouton en bas a droite).

**Preuve a capturer pour VAL-025 partie 4 :** capture avant/apres du viewport avec `Face Orientation` active montrant les normals correctes, capture de la fenetre d'export montrant `Selected Objects` coche, et soit une re-ouverture rapide du fichier exporte (`File > Import`) soit une capture du dossier montrant le fichier cree avec sa taille/date -- pour confirmer qu'il contient bien les objets prevus.

---

## Etape 5 -- Import Unity, test d'echelle, prefab

*(Correspond a l'Etape 5 du tutoriel officiel, VAL-025 partie 5)*

### 5.1 Importer le fichier dans Unity

1. Copie (ou exporte directement) le fichier `.fbx`/`.glb` dans le dossier `Assets/RoadRage/ArtExports/` du projet :
   - Le plus simple : ouvre l'Explorateur Windows sur `D:\Projets\RRS\Assets\RoadRage\ArtExports\` et **glisse-depose** le fichier exporte depuis Blender dedans.
   - Ou dans Unity, panneau **Project** (en bas), navigue jusqu'a `Assets > RoadRage > ArtExports`, puis glisse le fichier depuis l'Explorateur directement dans cette fenetre.
2. Unity detecte le nouveau fichier et l'importe automatiquement (courte barre de progression en bas a droite). Une icone de modele 3D apparait dans le dossier `ArtExports`.

### 5.2 Tester l'echelle en scene

1. Ouvre une scene de test (ou la scene courante).
2. **Glisse l'asset importe** depuis le panneau Project vers le panneau **Hierarchy** (en haut a gauche) ou directement dans le **Scene view** -- ca cree une instance de l'objet dans la scene.
3. Place a cote un objet de reference :
   - Soit un cube par defaut : clic droit dans la Hierarchy > **`3D Object`** > **`Cube`** (fait 1x1x1 metre par defaut, sert de "regle").
   - Soit un objet greybox deja present dans la scene.
4. Selectionne ton objet importe dans la Hierarchy, regarde le composant **`Transform`** dans l'**Inspector** (panneau a droite) : `Scale` devrait etre `1, 1, 1`. Si ce n'est pas le cas, ou si l'objet apparait couche/tourne bizarrement, c'est le "Cas limite import (echelle/axe)" du tutoriel officiel -- retourne corriger dans Blender (Etape 1.3, pivot + transforms appliques) plutot que de corriger manuellement le `Scale`/`Rotation` dans Unity.
5. Compare visuellement la taille dans la Scene view (utilise la molette/clic droit+ZQSD ou clic milieu pour naviguer la vue Unity, similaire a Blender).

### 5.3 Convertir en prefab

**Seulement si l'echelle/orientation est validee a l'etape precedente.**

1. Dans le panneau **Project**, navigue jusqu'a `Assets/RoadRage/Prefabs/`.
2. **Glisse l'objet depuis la Hierarchy** (pas depuis le fichier source dans `ArtExports`) vers ce dossier dans le panneau Project.
3. Unity cree un fichier prefab (icone bleue cube) et l'objet dans la Hierarchy devient bleu/lie au prefab -- c'est confirme visuellement par la couleur du nom dans la Hierarchy.

### 5.4 Ajouter et documenter le collider

1. Selectionne le GameObject (dans la Hierarchy ou sur le prefab).
2. Dans l'**Inspector**, bouton **`Add Component`** (en bas du panneau) > tape `Box Collider` (ou `Sphere Collider` / `Capsule Collider` selon la forme) > clique dessus pour l'ajouter.
3. Ajuste la taille/position du collider :
   - Soit directement les champs `Center`/`Size` dans l'Inspector.
   - Soit clique le bouton **`Edit Collider`** (icone petite dans le composant, ressemble a un cube avec un curseur) puis fais glisser les poignees vertes dans la Scene view.
4. **Documenter le choix** : ce tutoriel n'exige pas un champ Unity dedie -- ecris simplement dans la preuve VAL-025 (note texte accompagnant la capture) quel type de collider a ete choisi et pourquoi (ex. "BoxCollider, dimensionne a la caisse, car forme globalement cubique -- pas de mesh collider sur la geometrie de rendu detaillee").

**Preuve a capturer pour VAL-025 partie 5 :** capture du panneau Project montrant le fichier importe sous `ArtExports`, capture de la Scene view avec l'objet a cote de sa reference d'echelle, capture du panneau Project montrant le prefab cree sous `Prefabs` (ou une note explicite si tu decides de ne PAS convertir), et la note texte du choix de collider decrite ci-dessus.

---

## Pieges frequents pour un debutant

- **"Rien ne se passe quand je clique"** : verifie que la souris est bien positionnee sur le bon panneau -- beaucoup de raccourcis Blender (`N`, `Tab`, `A`, `X`) dependent de la zone survolee par la souris, pas juste de la selection.
- **Le menu `Object` ou `Mesh` n'apparait pas** : `Object` n'existe qu'en Mode Objet, `Mesh` seulement en Mode Edition -- verifie le mode actuel dans le menu deroulant tout en haut a gauche du viewport.
- **L'objet a disparu apres export/import** : verifie que l'objet etait bien selectionne (surligne en orange) avant `File > Export`, et que `Selected Objects`/`Limit to Selected` etait bien coche.
- **Unity n'affiche pas le nouveau fichier** : clique une fois dans le panneau Project puis fais un clic droit > `Refresh`, ou attends quelques secondes -- l'import automatique peut prendre un instant sur un gros fichier.
- **Le prefab ne se met pas a jour apres modif du modele** : si tu modifies a nouveau le mesh source dans Blender et re-exportes vers le meme fichier dans `ArtExports`, Unity devrait re-importer automatiquement et le prefab suivre -- si ce n'est pas le cas, clic droit sur le fichier dans Project > `Reimport`.

Pour toute question de statut/preuve/gate (VAL-025, AD-13, AD-27, "Demander d'abord"), reviens au tutoriel officiel [`story-0-6-blender-asset-intake-tutorial.md`](story-0-6-blender-asset-intake-tutorial.md) -- ce guide ne couvre que le "comment cliquer", pas les regles de gate.
