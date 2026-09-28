# Premier angle — TJunction_South / SE : preuves et HALT (reprise du 2026-09-28)

Reprise après l'interruption de Codex. Tous les changements non committés ont été préservés avant toute écriture : ref `refs/preserve/5-51-codex-wip` (commit `84ccac0ad4d75b8f9162226dd4253a356032fb3d`, 23 fichiers vérifiés octet par octet) et archive tar hors dépôt.

## Corrections apportées au mesureur à la reprise

- `JunctionClearance.cs` ne compilait pas (`LayerMask.Contains`, CS1929) : participation physique réécrite (matrice de couches + include/exclude des deux colliders, du côté conservatif).
- Empreinte physique : la couche 0 était codée en dur à la place de la couche du véhicule IA ; le collider IA (couche, include/exclude, boîte) fait maintenant partie des entrées physiques.
- Gate sémantique étendu aux surfaces `Sidewalk` des raccords (avenues, anneaux) situées à portée des trajectoires prolongées. Codex ne mesurait que les surfaces du module de jonction.
- Portée de recherche des obstacles et des surfaces élargie de 1 m : un obstacle placé juste hors de portée pouvait encore rendre le résidu négatif entre deux poses.
- `ConflictSweep` : les nœuds compilés reprennent leurs valeurs stockées au lieu d'être ré-échantillonnés. Les poses 5.50 sont ainsi identiques au bit près, et seuls deux champs de provenance sont ajoutés.
- Les messages d'échec donnent le chemin complet du collider.

## Gate physique

- Tous les colliders de trottoir de la jonction et des raccords sont affleurants : `y ∈ [-0,20 ; 0,00]`, au même sommet que `Col_Roadway`. Ils sont hors de la tranche véhicule (route, route + 1,44 m], donc non retenus comme obstacles.
- Obstacles retenus près des 6 mouvements : les seuls blocs urbains `Greybox_CityBlock_A_*`. Résidu minimal : **+6,6450 m** (virages à droite), +10,6450 m (virages à gauche), aucun obstacle participant pour les mouvements tout droit. Les véhicules à Rigidbody dynamique sont exclus, comme le prévoit la spec.
- La coupe ne change donc rien au gate physique de cet angle.

## Gate sémantique (`Sidewalk`, en plan, continu)

- Borne continue `min(d_a,d_b) - (|Δp| + ρΔθ)/2` : elle est conservative parce que `RoadCurve` interpole linéairement la position et de façon monotone le cap entre nœuds, et parce que les coutures sont traitées comme intervalles explicites.
- Angle SE (3 boîtes, c = 1,00 m), minimum par mouvement : droite `FromSouth → East_Out` **+0,1127 m** en (−4,25 ; −29,27), hors couture ; gauche `FromSouth → West_Out` +0,4216 m ; tout droit `FromWest → East_Out` +0,6450 m ; gauche `FromEast → South_Out` +4,3801 m.
- Raccords adjacents (`Avenue_CenterToSouth/Col_Sidewalk_Left`, `Ring_South_West/Col_Sidewalk_Left`) : ≥ +0,6450 m.
- Convergence du mouvement critique : h = 0,05 → +0,1127 ; h = 0,025 → +0,1286 ; h = 0,01 → +0,1366 ; distance aux poses seule : +0,1446 m. La borne à 0,05 m est donc bien une borne inférieure.
- Coupe minimale (polygone chanfreiné, 6 trajectoires) : c = 0,75 → −0,0217 ; **c = 0,80 → +0,0064** ; c = 0,90 → +0,0607 ; c = 1,00 → +0,1127. En dessous de 0,5 m, le minimum reste à −0,0326 m.
- Empreintes (stables avant et après rechargement) : physique `17a10f35…6022`, sémantique `26b43d1b…1cdc`.

## Forme, persistance, diff, invariants

- Rechargement de `MVP_Run` depuis le disque (scène non dirty) : union exacte des 3 boîtes sur une grille de 1 cm, `Sidewalk` = aire 3, ancien visuel Synty SE inactif ; mesures et empreintes identiques.
- Diff de scène : 3 overrides sur l'instance `Greybox_TJunction` (`m_IsActive` du visuel SE, `m_Size.x = 3`, `m_Center.x = 0,5`) et 8 objets ajoutés sous `Col_Sidewalk_Corner_SE`. Aucun prefab, matériau, `ProjectSettings` ou fichier Synty modifié.
- Invariants V1/V2 : SourceHash `b3064424…92fc`, LineageHash `f838ab59…e6f4`, ModelVersion `v4:e8dff9e54bff1712308899158ad16a9b`, 72 mouvements ; JSON du modèle, de la lignée, d'authoring et NavMesh identiques à `before-inputs.txt`.
- Concordance en plan déclaration ↔ visuel (rastérisation à 5 cm, tolérance de bord 3 cm, faces non verticales) : 0 cellule divergente dans les deux sens pour `TJunction_South`, `Ring_South_West`, `Avenue_CenterToSouth` et `Ring_South_East`.

## Motif du HALT : le visuel diverge dans la coupe

- Dans le triangle coupé, le seul rendu est `Greybox_GroundPlane` (sommet à **y = −0,05 m**, matériau `Greybox_Ground_Mat`). Aucune tuile d'asphalte Synty ne couvre la coupe, alors que `Col_Roadway` porte physiquement le véhicule à y = 0. On voit donc un creux gris de 5 cm là où le véhicule roule (`first-corner-cut-oblique-before-fill.png`, `first-corner-cut-top-before-fill.png`).
- Le chanfrein est une dalle grise pleine dont le sommet est à +0,058 m, alors que le trottoir Synty voisin est une dalle à 0,000–0,018 m avec une simple lèvre de bordure à +0,056 m. La dalle grise ne ressemble donc ni en relief ni en teinte aux trottoirs voisins.
- Le mesureur signale 40 appariements visuels « 1 renderer = 1 collider » impossibles sur des trottoirs non modifiés (tuiles multiples, mesh de segment unique). Ce sont de faux échecs de la méthode, contredits par la rastérisation. Il faudra les remplacer par cette rastérisation avant la phase 3.
- Les rampes `Relief_DosDane_AvenueCenterToEast/Rampe_Est|Ouest` sont des obstacles de forme non supportée : échec fermé, à traiter en phase 3 (hors premier angle).

Les onze autres angles (−0,0326 m chacun) ne sont pas modifiés.

## Validation EditMode à la reprise (`validate.ps1 -TestMode EditMode -TestTimeoutSec 900`)

Résultat brut : `ECHEC: 2/885`, puis `VALIDATION FAILED / INCOMPLETE` (log : `resume-editmode-first-corner.txt`). Compilation saine, 0 erreur Console dans la fenêtre.

- `Story528AuthoringAndGateATests.GateAIsOpenedOnlyByTheOwnersBoundSignoff` : tripwire Gate A non signée, attendu et déjà présent dans la baseline.
- `Story549RoundaboutWideningTests.TrafficV2SourcesNeverReferenceTheNavMesh` : **nouvel échec causé par la 5.51**. Le garde-fou AD-33 interdit `UnityEngine.AI` et `NavMesh` dans `Assets/RoadRage/Features/Vehicles/Traffic/**`. Or la spec place le mesureur sous `Traffic/Migration/`, et le correct-course du 2026-09-28 impose de résoudre `Sidewalk` depuis l'aire `NavMeshModifier` authorée. Il s'agit d'un conflit de contrat, à trancher par le propriétaire. Masquer les chaînes pour passer le test est exclu.

PlayMode non relancé : baseline fraîche déjà capturée (`before-playmode.txt`, 45 tests, 6 échecs), et le runner ne sert qu'une fois par session Editor.

## Reprise après les décisions A1, B1 et C du propriétaire

### B1 : architecture (AD-33 conservé)

- Nouvel adaptateur Editor `Assets/RoadRage/Features/Vehicles/SidewalkDeclarations.cs`, placé hors de `Traffic/`. C'est le seul lecteur des `NavMeshModifier` et de l'aire `Sidewalk`, résolue par son nom.
- `JunctionClearance.Measure(..., IReadOnlyList<JunctionClearanceSurface> sidewalks, h)` reçoit des colliders déclarés (actifs ou non) et une signature des champs authorés. Le fichier ne contient plus aucune mention `NavMesh` ni `UnityEngine.AI`.

### Concordance visuelle (remplace l'appariement « un renderer = un collider »)

- **Grille :** pas de 5 cm par module. Chaque cellule prend la surface visible la plus haute parmi les faces non verticales des renderers actifs du module.
- **Aspect « trottoir » :** couple mesh + matériaux dont au moins un porteur, quelque part dans la scène, a ≥ 3/4 de son empreinte dans une zone déclarée.
- **Échecs :** cellule déclarée sans trottoir visible, trottoir visible non déclaré, ou égalité de hauteur entre un trottoir et autre chose.
- **Tolérance :** une cellule d'érosion (5 cm).
- **Périmètre :** modules dont une surface entre dans la preuve. Giratoires et portails, hors de portée, ne sont pas jugés ici.
- **Faux échecs :** les 40 appariements tuilés erronés ont disparu (avenues, anneaux, `Col_Sidewalk_North` des T).
- **Cas adversariaux, en mémoire et restaurés, scène non dirty :**
  - ancien coin Synty SE réactivé → « 120 cellules trottoir visible non déclaré » ;
  - dalle `Chamfer_Visual_0` masquée → « 4405 cellules déclaré sans trottoir visible » ;
  - l'empreinte sémantique change dans les deux cas.

### Rampes `Relief_DosDane_AvenueCenterToEast`

- **Forme :** deux `BoxCollider` inclinés de ±5,7°, sommet à 0,12 m, qui couvrent toute l'avenue (x 9,0..11,4, z −4..4). Ils sont dans le périmètre des trajectoires prolongées du carrefour central.
- **Mesure :** l'empreinte plane est désormais exacte pour toute boîte orientée (enveloppe des 8 sommets). Les rampes sont donc mesurées, et non ignorées.
- **Résultat :** gate physique **négatif** (−0,0245 à −0,025 m) pour les mouvements qui entrent par l'Est ou en sortent. Un dos d'âne franchissable échoue au gate « volume entre route et sommet IA ».
- **Statut :** à trancher par le propriétaire avant la phase 3. L'échec est hors premier angle, mais bloquant pour le carrefour central.

### A1 : remplissage de la coupe

- **Objet :** `TJunction_South/Visual_Greybox_TJunction/Chamfer_RoadFill_SE`, carré de 1 × 1 m à l'angle, épais de 2 cm, sommet à y = 0,000, `Greybox_Road_Mat`, **aucun collider**.
- **Emprise visible :** la moitié qui correspond au triangle coupé. L'autre moitié est enfermée sous la dalle du chanfrein (sommet +0,058 m) et ne masque aucune surface de trottoir.
- **Diff de scène :** par rapport à la préservation, uniquement +98 lignes (un GameObject : Transform, MeshFilter, MeshRenderer).
- **NavMesh :** le bake collecte les colliders (`m_UseGeometry: 1`), le remplissage n'y entre donc pas.
- **Sauvegarde et rechargement :**
  - forme exacte (grille de 1 cm), `Sidewalk` = aire 3 ;
  - mesures inchangées : SE +0,1127 m en sémantique, +6,645 m en physique ;
  - aucune discordance visuelle dans le périmètre.
- **Chaussée de `TJunction_South` hors trottoir (63 750 cellules) :**
  - **0 trou**, **0 surface concurrente à moins de 2 mm** (pas de z-fighting) ;
  - le remplissage est visible sur 190 cellules ;
  - la colonne x = 8,03 signalée hors niveau est un artefact d'arrondi du script de contrôle, hors chaussée.
- **Rendu** (`first-corner-*-a1-fill.png`) : la géométrie est cohérente (affleurante, bords nets, sans débord). En revanche, `Greybox_Road_Mat` (`_BaseColor` 0,12 / 0,13 / 0,14) est un gris-bleu uni nettement plus sombre que l'asphalte texturé Synty. Pixel mesuré : 0,30 contre 0,39 en vue de dessus. En vue oblique, le triangle se lit presque comme une ombre. Cet écart vient du matériau imposé et ne peut pas être corrigé sans modifier un asset partagé.

### Empreintes

- Physique : `2d45db1a…4a59`.
- Sémantique : `2f20787c…bbb4`.

Toutes deux sont stables avant et après remplissage et rechargement. L'empreinte sémantique est insensible au remplissage de chaussée, qui n'est ni déclaré ni d'aspect trottoir.

### Libre circulation hors chaussée

Aucun collider n'est ajouté. Les colliders de trottoir restent affleurants (sommet 0,0), comme avant. La preuve ne porte que sur les trajectoires compilées et ne crée aucun confinement.

### Validation EditMode après B1 et A1 (`gate-editmode-a1-b1.txt`)

Résultat brut : `ECHEC: 1/885`, puis `VALIDATION FAILED / INCOMPLETE`. Le seul échec est `Story528…GateAIsOpenedOnlyByTheOwnersBoundSignoff` (Gate A non signée, identique à la baseline). Le garde-fou AD-33 `Story549…TrafficV2SourcesNeverReferenceTheNavMesh` repasse au vert. Compilation saine, 0 erreur Console dans la fenêtre.

## Addendum propriétaire : A2 et relief routier franchissable (mesureur v2)

### A2 : remplissage texturé du triangle coupé

- **Mesh :** asset projet `Assets/RoadRage/App/Scenes/MVP_Run/ChamferRoadFill_TJunction_South_SE.asset`, 3 sommets, 1 triangle, YAML texte de 3,8 Ko. C'est le triangle exact de la coupe, à y = 0 : (−4 ; −28), (−4 ; −27), (−5 ; −28), en repère du coin (−2 ; 2), (−2 ; 1), (−1 ; 2). Rien sous la dalle, aucun débord.
- **UV :** ceux d'une tuile d'asphalte virtuelle occupant le carré d'angle, de même orientation et même densité que la tuile Synty `Road_n2_s2` (du/dx = 0,2417, dv/dz = 0,2408). Résultat : [0,7405 ; 0,9822] × [0,0235 ; 0,2643], dans la plage utilisée par les tuiles, sans repli.
- **Rendu :** matériau `Road_01` (Synty) référencé tel quel ; ombres, sondes, calque et drapeaux statiques copiés de `Road_n2_s2`. **Aucun collider.** Aucun asset Synty ni prefab modifié.
- **Scène :** l'objet A1 est retiré. Le delta total par rapport à la préservation reste +98 lignes : un GameObject avec Transform, MeshFilter et MeshRenderer.
- **Après sauvegarde et rechargement :**
  - forme exacte (grille de 1 cm), `Sidewalk` = aire 3 ;
  - chaussée : 0 trou, 0 z-fighting, remplissage visible sur 190 cellules ;
  - pixel du triangle = pixel de la chaussée voisine, (0,388 ; 0,357 ; 0,349) ;
  - raccord invisible en vue de dessus et en vue oblique (`first-corner-*-a2-fill.png`).

### Règle du relief routier franchissable

Un volume dans la tranche du véhicule est une surface roulable si et seulement si les trois conditions suivantes sont réunies. Sinon, c'est un obstacle.

- **(a)** Il ne chevauche aucune surface `Sidewalk` déclarée ; le contact à moins de 1 cm est admis.
- **(b)** Ses sommets, rentrés de 5 cm, reposent tous sur une surface à hauteur de route non `Sidewalk` ; à égalité de hauteur, c'est le trottoir qui l'emporte.
- **(c)** Sa hauteur ne dépasse pas la garde au sol statique du véhicule IA, soit 0,45 − 1200 × 9,81 / (4 × 32 000) − 0,22 + (0,73 − 0,71) = **0,158 m**, contre 0,12 m de bordure authorée franchie par les bancs 5.11 et 5.13.

Les boîtes de toute orientation sont projetées par l'enveloppe de leurs 8 sommets, ce qui est exact. Les autres formes non supportées restent un échec ferme.

**Classement mesuré dans `MVP_Run` :**
- **Reliefs roulables :** `Rampe_Ouest`, `Rampe_Est` et `Relief_MarcheBasse_AvenueCenterToEast` (0,120 m chacun).
- **Obstacles :** `Col_Curb_*` du carrefour (chevauchent les trottoirs d'angle), blocs d'immeubles (hauteur).
- **Mouvements tout droit et à gauche du carrefour central :** redeviennent positifs.
- **Échecs restants (19) :** tous portés par les onze angles non coupés.

**Amendements normatifs appliqués :**
- `epics.md` 5.51 : formes supportées, clause Obstacles, addendum, obligations de test ;
- correct-course, section 6 ;
- spine et contrat Road World Model : une phrase chacun ;
- spec : Code Map et journal des changements.

**Tests :** `Story551JunctionClearanceTests`, 8/8 via `validate.ps1 -TestFilter`.

### État des preuves du premier angle (mesureur v2, après rechargement)

- **Physique :** +6,645 m.
- **Sémantique :** +0,1127 m, borne continue.
- **Empreintes :** physique `e097bf51…87d1` ; sémantique `41116…01ef`.

### Gate du premier angle : conclusion

| Critère | Résultat |
| --- | --- |
| Rendu visuel | A2 continu avec l'asphalte : pixel identique, 0 trou, 0 z-fighting ; captures `*-a2-fill.png` |
| Compilation | saine, 0 erreur Console dans la fenêtre |
| Tests EditMode | 1/893 en échec, uniquement le tripwire Gate A non signée, identique à la baseline (`gate-editmode-a2-relief.txt`). Verdict du script : `VALIDATION FAILED / INCOMPLETE` |
| Preuve physique | +6,645 m (mesureur v2) |
| Preuve sémantique | +0,1127 m, borne continue (coupe de 1,00 m) |
| Sauvegarde et rechargement | forme, mesures, empreintes et rendu identiques après rechargement |
| Invariants | SourceHash, LineageHash, `ModelVersion`, 72 mouvements ; JSON V1 et NavMesh identiques à `before-inputs.txt` ; aucun asset Synty ni prefab modifié |

**Le gate du premier angle passe.** Les onze autres angles sont intacts.

### Décision d'extension

L'extension est décidée, en deux vagues.

**Vague 1 : les 7 angles de T restants.** Même prefab, pas de bordure, donc la construction validée se reproduit à l'identique : 3 boîtes, dalle, triangle A2 et c = 1,00 m. Aux deux angles de `TJunction_North`, les boîtes ajoutées héritent de l'état désactivé du collider d'origine. Le monde physique de ce T reste ainsi celui de la baseline, et l'angle n'y existe que comme surface sémantique.

**Vague 2 : les 4 angles du carrefour central.** Ils portent des bordures `Col_Curb_*`, qui sont des obstacles physiques réels, et le raccord bordure/chanfrein est une construction nouvelle. On refait donc d'abord un mini-gate complet sur un seul angle du carrefour, avant les trois autres.

**Ensuite :**
- rebake NavMesh, puis vérification de son hash ;
- mesures complètes ;
- tests adversariaux de la spec ;
- V1 avant/après en sessions Editor fraîches : le redémarrage manuel est requis pour PlayMode ;
- arbitrage propriétaire de tout delta V1.

## Vague 1 : les sept angles en T restants (2026-09-28)

**Préalables vérifiés :**
- Boîte inclinée : l'enveloppe des 8 sommets couvre le volume entier à toute hauteur, et la tranche verticale est jugée sur la boîte englobante monde. Test : porte-à-faux d'une boîte inclinée de 60°.
- Mesh non convexe : refusé (test).
- Condition (b) du relief : démontrée sur une grille de 10 cm couvrant toute l'empreinte rentrée. Tests : trou de chaussée, trottoir, sol nu.
- Textes normatifs alignés.

**Méthode :**
- Un script générique reproduit la construction validée ; il a été vérifié à l'identique sur `TJunction_South/SE`.
- Coupe c = 1,00 m, sommet coupé déduit du centre de jonction.
- Colliders : même état d'activation que l'original.
- Dalles calées sur le visuel Synty d'origine.
- Triangle A2 avec ajustement UV affine vérifié sur la tuile d'asphalte la plus proche.
- Chaque angle suit la même séquence, avec arrêt au premier échec :
  - garde de scène ;
  - construction, sauvegarde et rechargement ;
  - forme sur grille de 1 cm ;
  - concordance collider/visuel ;
  - rendu (pixel triangle = pixel chaussée) ;
  - chaussée (trous, z-fighting) ;
  - mesure des deux preuves ;
  - garde Git.

| Angle | Colliders | Tuile UV | Physique min | Sémantique min | Rendu | Chaussée |
| --- | --- | --- | --- | --- | --- | --- |
| TJunction_South/SE (1er angle) | actifs | Road_n2_s2 | +6,6450 | +0,1127 | écart 0,000 | 0 trou, 0 z-fight |
| TJunction_South/SW | actifs | Road_s2_s2 | +6,6450 | +0,1127 | écart 0,000 | 0 trou, 0 z-fight |
| TJunction_North/SE | **désactivés, conservés** | Road_n2_s2 | +6,6450 | +0,1127 | écart 0,000 (ombre d'immeuble sur la dalle) | 0 trou, 0 z-fight |
| TJunction_North/SW | **désactivés, conservés** | Road_s2_s2 | +6,6450 | +0,1127 | écart 0,000 | 0 trou, 0 z-fight |
| TJunction_East/SE | actifs | Road_n2_s2 | +6,6450 | +0,1127 | écart 0,000 | 0 trou, 0 z-fight |
| TJunction_East/SW | actifs | Road_s2_s2 | +6,6450 | +0,1127 | écart 0,000 | 0 trou, 0 z-fight |
| TJunction_West/SE | actifs | Road_n2_s2 | +6,6450 | +0,1127 | écart 0,000 | 0 trou, 0 z-fight |
| TJunction_West/SW | actifs | Road_s2_s2 | +6,6450 | +0,1127 | écart 0,000 (ombre d'immeuble sur la dalle) | 0 trou, 0 z-fight |

**Toutes les lignes des 4 T sont positives :** 27 lignes par jonction, dont 7 surfaces de raccord ; minimum P = +6,645 m, S = +0,1127 m. Les 12 échecs restants sont tous sur `Intersection_Center_Crossroads`, qui n'est pas touché.

**Écart visuel connu (inchangé depuis le premier angle validé) :** la dalle du chanfrein est un bloc gris uni de 0,24 m (sommet +0,058 m), plus sombre et plus plein que le trottoir Synty clair à simple lèvre de bordure. C'est le visuel gris candidat de la spec.

**Invariants :**
- SourceHash `b3064424…92fc`, LineageHash `f838ab59…e6f4`, `ModelVersion` `v4:e8dff9e54bff1712308899158ad16a9b`, 72 mouvements.
- JSON V1 et NavMesh identiques à `before-inputs.txt`.
- Prefabs, matériaux, `ProjectSettings`, `Packages` et Synty : intacts.

**Diff de scène :** voir `wave1/scene-diff-by-document.txt`. 188 documents ajoutés, 0 retiré, 4 instances de T modifiées (6 propriétés chacune). Captures dans `wave1/`.

**EditMode :** `ECHEC: 1/897`, uniquement le tripwire Gate A, puis `VALIDATION FAILED / INCOMPLETE` (`gate-editmode-wave1.txt`). `Story551JunctionClearanceTests` : 12/12, dont la non-régression des 4 T.

**Baseline PlayMode :** conservée telle quelle (`before-playmode.txt`, 45 tests, 6 échecs 5.12/5.13/5.7). Pas encore de passe « après » : elle viendra en session fraîche, après les 12 angles.

## Critère (b) du relief : inclusion géométrique exacte (mesureur v3, 2026-09-28)

La grille de 10 cm ne pouvait pas exclure un trou ou un chevauchement situé entre deux points. `RoadSupported` soustrait désormais l'empreinte du relief de l'union des supports (BoxCollider actifs, non-trigger, non dynamiques, à dessus horizontal à hauteur de route, non `Sidewalk`), chaque support dilaté de 1 mm pour joindre les dalles jointives ; tout reste d'aire non nulle refuse le relief. Tout chevauchement d'un collider `Sidewalk`, actif ou non, refuse aussi. Seuil de hauteur (0,158 m) et conditions (a)/(c) inchangés.

- Supports réels mesurés : `Col_Roadway` de l'avenue (boîte plate, dessus à 0) sous `Rampe_Est`, `Rampe_Ouest` et `Relief_MarcheBasse_AvenueCenterToEast` ; les trois restent franchissables.
- Tests ajoutés : fente de 6 cm entre deux points de l'ancienne grille (refusée), dalles jointives (acceptées), couverture pure (moitiés jointives, fente de 4 mm, coin manquant, support englobant, aucun support).
- Lignes des 4 T : identiques à la vague 1 sous v3.
- Commit `7fe9301`.

## Mini-gate du carrefour central : angle SE (2026-09-28)

**Raccord aux bordures.** `Col_Curb_South_East` (4 × 0,12 × 0,30 m, x ∈ [4 ; 8], z ∈ [−4,3 ; −4]) longe l'arête de l'angle face à l'avenue Est et traverse le triangle coupé. Son extrémité est reculée de 1,00 m par le même type d'override que le collider d'angle (`m_Size.x` = 3, `m_Center.x` = 0,5) : x ∈ [5 ; 8], soit exactement le sommet du chanfrein. Nom, activation, solidité et hauteur de 0,12 m sont inchangés ; la bordure reste un obstacle physique (jamais un relief : elle chevauche le trottoir) et est entièrement contenue dans le trottoir chanfreiné (contrôle sur grille de 1 cm : 0 point dans le triangle coupé, 0 hors du trottoir). Aucune bordure n'est ajoutée le long de la diagonale.

**Construction :** identique aux angles en T (boîte source réduite, `Chamfer_Box_B`, `Chamfer_Box_Diagonal`, trois visuels gris, Synty masqué, triangle A2 `ChamferRoadFill_Intersection_Center_Crossroads_SE.asset` calé sur `Road_n2_s2`).

**Résultats (après sauvegarde et rechargement) :**
- Forme exacte, visuel = collider, trois autres angles et trois autres bordures intacts.
- Triangle : écart de pixel 0,000 avec la chaussée (0,024 avant, trottoir Synty) ; 0 trou, 0 z-fight (190 cellules du remplissage).
- Virage à droite `Junction_FromSouth -> Connector_East_Out` : P = +0,1127 m (témoin `Col_Curb_South_East`, (4,25 ; −2,73)), S = +0,1127 m ; avant −0,0326 / −0,0326.
- Toutes les lignes passant par l'angle SE ou sa bordure restent positives et inchangées (min P +0,4216 hors virage à droite).
- Carrefour : 51 lignes positives hors des trois virages non coupés ; les 9 échecs restants sont exclusivement ceux des angles NE, NW et SW.
- Aucun écart de concordance visuelle `Sidewalk` sur le carrefour.

**Diff de scène (`crossroads-se/scene-diff-by-document.txt`) :** 24 documents ajoutés, 0 retiré, 1 instance modifiée (`Intersection_Center_Crossroads`), 5 propriétés : `Sidewalk_Corner_SE.m_IsActive` = 0, `Col_Sidewalk_Corner_SE` et `Col_Curb_South_East` `m_Size.x` = 3, `m_Center.x` = 0,5.

**Invariants :** SourceHash `b3064424…92fc`, LineageHash `f838ab59…e6f4`, `ModelVersion` `v4:e8dff9e54bff1712308899158ad16a9b`, 72 mouvements ; NavMesh inchangé ; prefabs, Synty et `ProjectSettings` intacts. Les tests qui identifient les bordures par leur nom (5.10 rejeu autour du carrefour, 5.11 hauteur de bordure sur instance de prefab) passent.

**Écarts visuels ouverts (non acceptés) :**
- Dalle grise unie (sommet +0,058/+0,060 m), plus sombre que le trottoir Synty : point ouvert de livraison, comme aux T.
- Préexistant, inchangé en nature : le collider de bordure (0,12 m) n'a pas de visuel propre ; il dépassait déjà de 6,4 cm la lèvre Synty (0,056 m) et dépasse de 6 cm la dalle grise sur ses 3 m restants. Aucune barrière nouvelle n'est créée.

**EditMode (`crossroads-se/gate-editmode-crossroads-se.txt`, `-TestTimeoutSec 900`) :** `ECHEC: 1/899`, uniquement le tripwire Gate A, puis `VALIDATION FAILED / INCOMPLETE`. Un premier lancement au délai par défaut (300 s) a expiré sans résultat. `Story551JunctionClearanceTests` : 14/14.

Captures : `crossroads-se/corner-SE-{top,oblique}-{before,after}.png`. Les angles NE, NW et SW attendent l'accord du propriétaire.
