# Story 5.10 - Notes de verification : graphe de voies, district greybox et trafic portail-a-portail

Date: 2026-09-16

## Ce que la story a change

- La boucle de huit waypoints (`RouteWaypoints` sur `AIRoute`) a disparu. Le trafic parcourt maintenant un **graphe de voies authore en scene** : des `LaneNode` poses sous la racine gameplay de modules greybox, collectes par une racine `LaneGraph`.
- Les trois vehicules IA poses en scene ont disparu eux aussi. L'effectif n'est plus une constante de scene : un service hote (`PortalTrafficSpawner`) fait **entrer et sortir** les vehicules **exclusivement aux portails** authores, en tendant vers une cible resolue a l'execution depuis un `TrafficSettingsDef`.
- Le parcours d'un vehicule n'est pas un itineraire : il nait d'un **tirage de virage pondere a chaque jonction** (modele `jtrrouter`), deterministe par vehicule car graine sur son `NetworkObjectId`.
- La boucle greybox `Greybox_RoadLoop_*` de la Story 3.4 est retiree de `MVP_Run` : le district route la remplace.

## Topologie du district authore

Croquis valide par l'humain, pose tel quel dans `MVP_Run` :

```
  [tunnel]                        [tunnel]
       \                              /
        O-------------T-------------O
        |             |             |
        T-------------X-------------T
        |             |             |
        O-------------T-------------O
       /                              \
  [tunnel]                        [tunnel]
```

- **Neuf jonctions.** Un carrefour en croix a quatre branches (`X`) au centre exact de la carte (0, 0, 0) ; quatre jonctions en T (`T`) a mi-cote de l'anneau, trois branches chacune (deux le long de l'anneau, une vers le centre) ; quatre ronds-points (`O`) aux coins, trois branches chacun (deux le long de l'anneau, une branche sortante en diagonale).
- **Douze avenues a double sens.** Quatre radiales (carrefour central <-> jonction en T) et huit demi-cotes d'anneau (jonction en T <-> rond-point).
- **Quatre tunnels-portails**, un par coin, au bout de la branche diagonale du rond-point. Ce sont les quatre seules entrees et les quatre seules sorties du district.

| Element                                                             | Position (monde)                       | Rotation                 |
| ------------------------------------------------------------------- | -------------------------------------- | ------------------------ |
| `Intersection_Center_Crossroads`                                    | (0, 0, 0)                              | 0 deg                    |
| `TJunction_North` / `_South` / `_East` / `_West`                    | (0, 0, +/-32) et (+/-32, 0, 0)         | 0 / 180 / 90 / 270 deg   |
| `Roundabout_NorthEast` / `_SouthEast` / `_SouthWest` / `_NorthWest` | (+/-32, 0, +/-32)                      | 0 / 90 / 180 / 270 deg   |
| `Avenue_CenterTo*` (4)                                              | (0, 0, +/-16) et (+/-16, 0, 0)         | 0 / 90 deg               |
| `Ring_*` (8)                                                        | (+/-16, 0, +/-32) et (+/-32, 0, +/-16) | 90 / 0 deg               |
| `TunnelPortal_*` (4)                                                | (+/-43,31, 0, +/-43,31)                | 45 / 135 / 225 / 315 deg |

Mesure : **204 noeuds, 4 portails d'entree, 4 portails de sortie, 0 connecteur orphelin**, graphe valide, chaque portail d'entree atteint un portail de sortie. Emprise reelle du district : **-56,04 a +56,04 m** sur les deux axes, contre une face interieure de mur de bord a +/- 58 m -- le district tient dans la carte 120 x 120 existante sans la traverser.

## Cotes de module figees -- contrat pour l'art final

Ce qui casserait le trace au passage a l'art final n'est pas le look mais la geometrie. Les valeurs
ci-dessous sont **le contrat** : un module d'art final doit les respecter au centimetre pour se
substituer a un module greybox sans retoucher le graphe. Elles decrivent ce qui est reellement pose,
pas une intention.

### Le plan de roulage -- la regle qui prime sur toutes les autres

> **LIVRE par la Story 5.11 (2026-09-18).** La premisse « ni roue ni suspension » a disparu : des
> raycasts par roue et une suspension authoree portent le vehicule (AD-35), donc une bordure authoree
> est franchissable, et sa hauteur devient la nouvelle cote de contrat. La regle en vigueur est :
>
> Une surface roulable a sa face superieure a `y = 0` exactement. Aucun collider de module ne presente
> de marche verticale au-dessus de ce plan, **sauf la bordure authoree du prototype**, dont la hauteur
> est bornee, nommee ci-dessous, et gardee par un test. Trottoirs et ilots de giratoire restent
> affleurants et distingues par leur materiau, pas par leur hauteur. Les parois de tunnel et les murs de
> bord de carte gardent leur droit de s'elever : ce sont des obstacles voulus.
>
> **La bordure livree** : 0,12 m de haut, 0,3 m d'epaisseur, 4 m de long, quatre segments sous
> `Collision/` du carrefour central `Greybox_Intersection`, nommes `Col_Curb_*`, poses sur les aretes de
> chaussee de la route est-ouest -- face interieure exactement a `|z| = 4 m`, donc **entierement dans la
> bande trottoir de 4 m, jamais dans les 8 m de chaussee**, et sans deplacer un seul `LaneNode`.
>
> Le nom `Col_Curb_*` est delibere : `SidewalksCarryTheDedicatedAreaAndStayOutOfTheVehicleBake` exige
> zero `Col_Sidewalk` sur le giratoire, et une bordure ne doit pas etre lue comme un trottoir bake.
>
> Le test `NoModuleColliderRisesAboveTheDrivingPlane` est **reecrit, pas retire** : « aucun collider de
> module ne depasse le plan de roulage au-dela de la hauteur de bordure authoree ». Sans cette garde,
> l'art pourrait reintroduire en silence les marches de 15 cm a l'origine du bug. **Tout le reste du
> tableau de cotes ci-dessous ne depend pas de la physique et reste ferme.**

**Pourquoi la regle etait si stricte, et pourquoi elle peut se relacher d'un cran.** Jusqu'a la Story
5.9 le vehicule etait pilote en ecrivant `linearVelocity`, sans roue ni suspension : une `BoxCollider`
contre une face verticale ne peut pas la gravir, PhysX resout l'interpenetration par une impulsion qui
se convertit en vitesse verticale. Les trottoirs de 15 cm de la premiere version envoyaient donc les
vehicules en l'air, en collaient d'autres, et declenchaient le hook de degats de la Story 3.5. Depuis
la Story 5.11, le contact au sol vient de raycasts par roue pilotes par des fonctions pures : la
hauteur franchissable n'est plus nulle, elle est **bornee par le debattement de suspension authore**
(0,25 m pour le profil par defaut, contre une bordure de 0,12 m), et un test EditMode tient les deux
valeurs ensemble -- monter la bordure sans couvrir le debattement serait une incoherence d'authoring,
plus une surprise de recette.

L'interdiction des trottoirs reste **entierement** portee par le graphe de voies et le masque d'aire
NavMesh. La physique n'a jamais ete le mecanisme d'interdiction et ne doit pas le devenir. Un test
EditMode (`NoModuleColliderRisesAboveTheDrivingPlane`) garde la regle.

### Communes a tout le kit

| Cote                                       | Valeur                                                                               | Consequence si elle change                                                                 |
| ------------------------------------------ | ------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------ |
| Largeur de voie                            | 4 m                                                                                  | Deplace l'axe de voie, donc tous les `LaneNode` du module.                                 |
| Deport de l'axe de voie                    | +/- 2 m depuis l'axe du module                                                       | Les connecteurs ne se rejoignent plus a la jointure.                                       |
| Largeur de chaussee (2 voies)              | 8 m                                                                                  | --                                                                                         |
| Largeur de trottoir                        | 4 m par cote                                                                         | Change l'emprise totale du module.                                                         |
| **Emprise totale (right-of-way)**          | **16 m** (8 de chaussee + 2 x 4 de trottoir)                                         | Les modules ne s'alignent plus en largeur.                                                 |
| Hauteur de trottoir                        | **0 m -- affleurant** : meme dalle de 0,2 m que la chaussee, face superieure a y = 0 | Le trottoir reste affleurant : c'est son materiau qui le distingue. Ce qui s'eleve, c'est la **bordure** du prototype, pas le trottoir. |
| **Hauteur de bordure authoree (Story 5.11)** | **0,12 m** de haut, 0,3 m d'epaisseur, segments `Col_Curb_*` sur les aretes est-ouest du carrefour central | C'est la valeur contractuelle du kit artistique. Au-dela, la roue ne monte plus (debattement de 0,25 m) ; en deca, la bordure cesse d'etre une bordure lisible. |
| Epaisseur de dalle (chaussee ET trottoir)  | 0,2 m, face superieure a **y = 0**                                                   | Les `LaneNode` sont a y = 0 : un autre niveau met le graphe sous ou sur la route.          |
| Face superieure du plan de sol de la carte | **y = -0,05** (5 cm sous le plan de roulage)                                         | Coplanaire avec les chaussees, il les fait clignoter (z-fighting) sur toute l'emprise.     |
| Sens de circulation                        | a droite : la voie "aller" est a +2 m de l'axe, vue dans le sens de marche           | Inverse toute la topologie du graphe.                                                      |
| Seuil de jointure des connecteurs          | 0,75 m                                                                               | Au-dela, deux modules voisins ne se relient plus.                                          |

### Par module

| Module                                    | Cote                                           | Valeur                                                                                             |
| ----------------------------------------- | ---------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| `Greybox_RoadSegment_TwoWay`              | Longueur                                       | **16 m**                                                                                           |
| `Greybox_Intersection` (croix 4 branches) | Emprise                                        | **16 x 16 m**                                                                                      |
| `Greybox_TJunction` (3 branches)          | Emprise                                        | **16 x 16 m**                                                                                      |
| `Greybox_Roundabout` (3 branches)         | Rayon du disque de chaussee                    | **8 m**                                                                                            |
| `Greybox_Roundabout`                      | Rayon de l'ilot central                        | 3 m                                                                                                |
| `Greybox_Roundabout`                      | Rayon de l'axe de l'anneau                     | 6 m                                                                                                |
| `Greybox_Roundabout`                      | Hauteur de l'ilot central                      | **0 m -- affleurant** (visuel remonte de 2 cm pour ne pas z-fighter avec le disque qu'il recouvre) |
| `Greybox_Roundabout`                      | Ecart angulaire max entre deux noeuds d'anneau | **37 deg** (voir « densite de l'anneau » ci-dessous)                                               |
| `Greybox_Roundabout`                      | Angles des branches                            | 2 branches a 90 deg (ouest, sud) + 1 branche diagonale a 45 deg                                    |
| `Greybox_TunnelPortal`                    | Longueur                                       | 16 m                                                                                               |
| `Greybox_TunnelPortal`                    | Gabarit libre                                  | 16 m de large x 5 m de haut                                                                        |
| `Greybox_TunnelPortal`                    | Epaisseur de paroi / plafond / fond            | 1 m                                                                                                |

**Regle d'assemblage, qui tient tout le reste :** _toute jonction place ses connecteurs de branche a
**8 m** de son centre._ C'est ce qui rend les douze avenues identiques (16 m) malgre trois formes de
jonction differentes -- entraxe de deux jonctions voisines = 8 + 16 + 8 = **32 m**. Un tunnel se pose
a 8 m (rayon du rond-point) + 8 m (demi-longueur du tunnel) = **16 m** du centre du rond-point, le
long de sa diagonale.

### Structure imposee a chaque module (AD-27)

```
<Racine gameplay>
  Visual_<Nom>/     <- uniquement des MeshRenderer. REMPLACABLE par l'art final.
  Collision/        <- uniquement des colliders (+ NavMeshModifier des surfaces non carrossables).
  Lanes/            <- uniquement des LaneNode.
```

Les `LaneNode` et les colliders ne vivent **jamais** sous `Visual_*` : remplacer l'enfant `Visual_*`
par le mesh final ne doit toucher ni le graphe ni la physique. Un test EditMode l'epingle sur les
cinq modules.

## Densite de l'anneau de giratoire -- pourquoi les vehicules mordaient l'ilot

Le vehicule va **tout droit** d'un noeud de voie au suivant : `ComputeSeekIntent` vise la position du
noeud, il ne suit pas un arc. Sur un anneau, l'ecart angulaire entre deux noeuds consecutifs fixe donc
directement la profondeur de coupe (la fleche de la corde).

La premiere version n'avait que 6 noeuds d'anneau, avec des ecarts allant jusqu'a **111 deg** :

|                                                  | avant                     | apres       |
| ------------------------------------------------ | ------------------------- | ----------- |
| Ecart angulaire max                              | 111 deg                   | 37 deg      |
| Longueur de corde                                | 9,89 m                    | 3,81 m      |
| Distance de la trajectoire au centre             | 3,40 m                    | **5,69 m**  |
| Bord interieur du vehicule (demi-largeur 1,03 m) | 2,37 m                    | **4,66 m**  |
| Rayon de l'ilot                                  | 3,00 m                    | 3,00 m      |
| **Marge**                                        | **-0,63 m (dans l'ilot)** | **+1,66 m** |

Cinq noeuds d'arc intermediaires (`Ring_Arc_*`) ont ete ajoutes par giratoire pour ramener tout ecart
sous 37 deg. Ce n'est pas une limite de braquage : a 8 m/s et 90 deg/s, le rayon minimal atteignable
est de 5,1 m, largement compatible avec l'anneau de 6 m. C'etait une **densite d'authoring**
insuffisante, corrigee dans le module -- donc dans le perimetre de cette story.

Un test EditMode verifie desormais, arete par arete, que la corde laisse le vehicule hors de l'ilot.

**Ce qui reste ouvert et n'appartient PAS a cette story :** le suivi de trajectoire reste une
poursuite de point a point. Un vehicule coupe toujours legerement a l'interieur de chaque virage, et
la profondeur de coupe est bornee par la distance au noeud vise (donc par `arrivalRadius`, 3 m). Sur
la geometrie authoree ici la marge est confortable ; un trace plus serre demanderait un vrai suivi de
courbe (poursuite pure, point de visee anticipe), ce qui est une mecanique de conduite, pas
d'authoring -- a tracer pour la Story 5.18 ou 5.17 (ex-5.11 / ex-5.12).

## Authoring du graphe

- **Roles de noeud** : `Normal`, `PortalEntry`, `PortalExit`, `Connector`. Un `PortalEntry` peut porter
  le drapeau "sortie reutilisant l'entree" et accepte alors aussi les vehicules sortants.
- **Convention de connecteur** -- c'est elle qui rend la jointure automatique non ambigue :
  un connecteur **sans** successeur authore est une **sortie de module** (fin de voie) ;
  un connecteur **avec** au moins un successeur est une **entree de module** (debut de voie).
  `LaneGraph` relie une sortie a l'entree voisine la plus proche sous le seuil authore
  (0,75 m) **et de meme sens de circulation** -- jamais l'inverse. Poser deux modules bout a bout
  suffit : aucun cablage manuel entre instances de prefab.
- **Ratios de virage authores** :
  - Carrefour en croix : quatre noeuds de decision (un par sens d'arrivee), trois successeurs chacun,
    droite / tout droit / gauche = **30 / 50 / 20** (valeurs par defaut de `jtrrouter`). Pas de demi-tour.
  - Jonction en T : trois noeuds de decision, deux successeurs chacun (40/60, 60/40, 50/50).
  - Rond-point : trois points de decision sur l'anneau, "sortir ici" / "continuer" = **60 / 40**.
- **Sens giratoire** : circulation a droite, donc l'anneau tourne dans le sens trigonometrique. Chaque
  branche porte deux noeuds d'anneau -- on **sort** juste avant la branche, on **entre** juste apres --
  si bien qu'un vehicule qui vient d'entrer ne peut pas ressortir immediatement par sa propre branche.
- **Noeuds d'arc** : les `Ring_Arc_*` d'un giratoire ne decident rien (un seul successeur). Ils
  existent uniquement pour que la corde parcourue reste proche de l'anneau.
- **Gizmo** : selectionner la racine `LaneGraph` affiche les aretes avec leur sens (pointes de fleche),
  les portails d'entree en vert, les portails de sortie en magenta, les connecteurs en jaune, et les
  connecteurs orphelins dans un cube rouge dresse.
- **Index de noeud = `NetworkedAIVehicleState.WaypointIndex`**. L'ordre de collecte est l'ordre de
  hierarchie, identique sur tous les pairs puisque la scene l'est : aucune NetworkVariable ajoutee.

## Authoring NavMesh

Le NavMesh de cette story est de la **donnee authoree**, pas un fournisseur de chemin actif :
aucun `NavMeshAgent` n'existe (AD-33). L'inviolabilite reelle des trottoirs vient du graphe -- un
vehicule ne circule que sur des aretes authorees.

| Element                               | Valeur                                                                                                                      | Ou                                     |
| ------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- | -------------------------------------- |
| Aire NavMesh `Sidewalk`               | index **3** (premier slot libre apres les 3 aires par defaut)                                                               | `ProjectSettings/NavMeshAreas.asset`   |
| Type d'agent `Vehicle`                | rayon 1,2 m, hauteur 2 m, pente 45 deg, marche 0,4 m                                                                        | `ProjectSettings/NavMeshAreas.asset`   |
| **Masque d'aire de l'agent vehicule** | `NavMesh.AllAreas & ~(1 << 3)` -- toutes les aires SAUF `Sidewalk`                                                          | decision par defaut de la story        |
| Trottoirs bordes de chaussee          | `NavMeshModifier` : `overrideArea = true`, `area = Sidewalk`, **`ignoreFromBuild = true`**                                  | `Collision/Col_Sidewalk_*` des modules |
| Ilot central de rond-point            | `NavMeshModifier` : `overrideArea = true`, `area = Sidewalk`, **`ignoreFromBuild = false`**                                 | `Collision/Col_Island`                 |
| Surface du district                   | `NavMeshSurface` sur la racine `LaneGraph` : agent `Vehicle`, `collectObjects = Children`, `useGeometry = PhysicsColliders` | `MVP_Run`                              |
| Donnee bakee                          | `Assets/RoadRage/App/Scenes/MVP_Run/NavMesh-LaneGraph-Vehicle.asset`                                                        | --                                     |

**Deux mecanismes, parce que la geometrie n'est pas la meme.** Un trottoir est _a cote_ de la
chaussee : l'exclure du bake (`ignoreFromBuild`) ne laisse aucune surface navigable derriere lui.
L'ilot central d'un rond-point est _par-dessus_ la chaussee : l'exclure du bake laisserait le disque
navigable dessous, donc c'est ici l'aire dediee qui fait le travail -- la surface existe, mais elle
porte l'aire `Sidewalk`, que le masque de l'agent vehicule ne coche pas.

Les Stories 5.17 (contournement d'obstacle, ex-5.12) et 5.21 (poursuite en rage, ex-5.14) ouvriront cette exclusion
**explicitement** plutot que de la decouvrir.

Sondages apres bake (avec le masque vehicule) :

| Point                            | Surface bakee | Accessible au masque vehicule |
| -------------------------------- | ------------- | ----------------------------- |
| Chaussee du carrefour central    | oui           | **oui**                       |
| Chaussee d'une avenue radiale    | oui           | **oui**                       |
| Anneau d'un rond-point           | oui           | **oui**                       |
| Chaussee d'une jonction en T     | oui           | **oui**                       |
| Trottoir d'une avenue            | **non**       | **non**                       |
| Trottoir d'un demi-cote d'anneau | **non**       | **non**                       |
| Ilot central d'un rond-point     | oui           | **non**                       |

Revalides apres l'aplatissement des trottoirs et de l'ilot : rendre ces surfaces affleurantes n'a pas
regresse leur exclusion, qui tient a `ignoreFromBuild` et a l'aire dediee, pas a leur hauteur.

## Reglages de trafic authores

Asset : `Assets/RoadRage/ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset` (id `traffic_default`).

| Parametre                                     | Valeur | Provenance                                                                                                                                                                                                            |
| --------------------------------------------- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `defaultTargetPopulation`                     | 8      | Jugement : dense sans saturer un district de cette taille. Point de reglage unique.                                                                                                                                   |
| `minTargetPopulation` / `maxTargetPopulation` | 0 / 30 | Borne haute alignee sur la cible ~30 vehicules des Stories 5.16 / 5.22 (ex-5.17).                                                                                                                                               |
| `edgeBudgetFactor`                            | 2      | `--max-edges-factor` de `jtrrouter`, valeur par defaut. Sur 204 noeuds, budget = 408 aretes ; les parcours tires observes vont de 20 a 109 aretes, donc le garde-fou ne se declenche jamais en fonctionnement normal. |
| `portalClearanceRadius`                       | 12 m   | Jugement : au-dela d'une longueur de vehicule, sous la profondeur du tunnel.                                                                                                                                          |
| `connectorJoinDistance`                       | 0,75 m | Jugement : large devant l'imprecision de pose, tres etroit devant les 4 m qui separent deux voies a une meme jointure.                                                                                                |

**AD-32 : c'est le seul endroit ou un effectif de trafic est ecrit.** Aucun controleur, aucun prefab,
aucune scene n'en porte. `PortalTrafficSpawner.ResolveSessionTargetPopulation` est le point de
branchement **unique et nomme** ou la Story 5.16 injectera la valeur de session.

## AD-34 : le retrait interdit, et ce qu'il change

Tous les systemes de trafic etudies s'echappent d'une situation impossible par une disparition.
L'interdire oblige le spawner a n'avoir **qu'une seule porte de sortie** :

- Le despawn est declenche par l'arrivee a un noeud `PortalExit`, signalee par
  `NetworkedAIVehicleDriverController.HasReachedExitPortal`. Jamais par un compteur, une distance au
  joueur, un echec de trajet, un capot retourne ou un hors-zone.
- Un budget d'aretes depasse **reoriente** vers le portail de sortie le plus proche (descente
  gloutonne, un pas a la fois) au lieu de conclure.
- Un cul-de-sac reoriente de meme, avec un avertissement.
- Un graphe **sans aucun portail de sortie** rend le vehicule inerte sur place, avec un avertissement
  emis une fois -- il n'est toujours pas retire.
- Une insertion refusee parce que le portail est encombre n'est jamais abandonnee : le deficit
  (cible moins effectif vivant) **est** la file d'attente, bornee par la cible, et le pas suivant
  retente.
- Le palier de teleportation de recuperation sur place de la Story 5.2 est **conserve** : il remet le
  vehicule en jeu, il ne le retire pas.

## Correctifs post-livraison -- retours terrain du 2026-09-16

Trois retours de jeu de l'humain. Les mesures ci-dessous sont prises sur les valeurs authorees du
depot (prefabs, scene, constantes), pas sur une impression de jeu.

### 1. La dalle d'intersection n'est pas au niveau du sol -- **accepte, non corrige**

Mesure : les faces superieures des chaussees, trottoirs et ilots des modules sont a `y = 0`, et le
plan de sol de la carte a `y = -0,05` (cube 120 x 120, `m_LocalScale.y = 0,1` a `y = -0,1`). Toutes
les instances du district sont posees a `y = 0` : il n'existe donc **aucune marche entre deux dalles
du district**, et une seule marche dans tout le decor -- la levre de **5 cm** entre une dalle et le
plan de sol, hors du district. C'est elle qui bump, partout ou l'on quitte ou rejoint une dalle,
carrefours carres compris : le retour est reel, son origine n'est pas celle qu'on croyait.

Decision humaine : **aucun collider n'est deplace**, la levre reste, le bump est **accepte**. Ce qui
change est sa consequence (retour 2), pas sa hauteur. L'ecart est consigne dans « Ecarts assumes ».

### 2. Degats trop faciles -- **corrige par la regle, pas par la geometrie**

Deux mecanismes distincts, tous deux dans la consommation du contact.

- **Defaut franc.** `RunFlowController.ApplyNetworkedCollisionDamage` appliquait
  `NetworkedPlayerLifecycleService.PlayerCollisionDamage` aux occupants de siege **avant** de tester
  `vehicleDamage`, alors que sa jumelle `ApplySecondaryVehicleCollisionDamage` portait deja la garde
  correcte. Toute entree de collision -- y compris sous le seuil de 3 m/s, ou la voiture ne perd
  aucun PV -- retirait des PV au conducteur et a chaque passager. Corrige : la garde
  `vehicleDamage > 0` precede desormais l'application aux occupants.
- **Discrimination impossible par la vitesse.** A vitesse de conduite, monter sur une dalle
  affleurante produit une vitesse de fermeture horizontale comparable a celle d'un choc contre un
  mur. Ce qui separe les deux n'est pas la vitesse mais **la hauteur du point de contact** :
  `NetworkedVehicleDriverController.OnCollisionEnter` n'emet plus l'evenement de collision quand
  **tous** les points de contact touchent le vehicule a hauteur de son dessous
  (`IsSurfaceOnlyCollision`, predicat pur `IsSurfaceContact`). Une mesure, aucun seuil de vitesse
  supplementaire -- qui laisserait le bump blesser des qu'on roule vite.

Mesure du vehicule, qui fixe la tolerance : la boite de collision des deux prefabs de voiture
(`Greybox_AIVehicle`, `Greybox_PlayerCar`) fait `2,06 x 1,42 x 4,44 m` pour un centre a `y = 0,73`,
donc son **dessous est a 2 cm au-dessus du plan de roulage**. Avec la tolerance authoree de **0,1 m**,
la bande classee « contact de surface » va de `+0,02` a `+0,12 m` au-dessus de la chaussee : la levre
de 5 cm y tombe, et la face d'un mur (paroi de tunnel haute de 5 m, mur de bord de carte, immeuble),
qui touche le vehicule bien plus haut, n'y tombe jamais. Une entree sans aucun point de contact n'est
pas classee de surface : dans le doute, le comportement d'origine est conserve.

### 3. Le trafic IA tournait en boucle -- **corrige par une marche auto-evitante**

Deux causes, une seule regle.

- **Giratoires.** Les points de decision d'un anneau portent « sortir ici » / « continuer » =
  **60 / 40** : un vehicule qui « continue » a ses trois points de decision parcourt donc un tour
  complet avec une probabilite de `0,4^3 = 6,4 %`, et les tours partiels sont frequents. Le budget
  d'aretes (408) est trop large pour s'en apercevoir, par construction.
- **Carrefours carres.** Croix centrale (droite 30 %) et jonctions en T (droite 40 a 60 %) laissent
  s'enchainer des virages a droite qui font le tour d'un ilot : le vehicule repasse par la meme
  jonction et se lit comme un vehicule qui tourne en rond.

Regle retenue : **un vehicule IA ne parcourt jamais deux fois le meme noeud de voie**. Le noeud
d'insertion puis chaque noeud atteint sont marques dans une memoire dimensionnee sur `NodeCount`
(204) ; le tirage de virage ne porte que sur les successeurs **non parcourus**
(`LaneGraphRouting.SelectWeightedSuccessor` avec sous-ensemble eligible, toujours pure et statique) ;
quand plus aucun n'est eligible, la reorientation gloutonne existante vers la sortie la plus proche
reprend la main -- c'est l'echappatoire, bornee par le budget d'aretes. La memoire est remise a zero a
l'insertion et a la recuperation sur place, **jamais ailleurs**.

Consequences de forme, assumees :

- Sur un anneau, le « continuer » qui ramenerait au noeud d'entree n'est plus eligible : **le tour
  complet devient impossible** et la sortie est forcee au plus tard a la derniere branche rencontree
  avant bouclage. C'est exactement la demande humaine -- continuer reste permis, boucler ne l'est pas.
- Le parcours devient une **marche auto-evitate**, bornee par `NodeCount` plutot que par le budget
  d'aretes. Les quatre tunnels etant les seules feuilles du graphe, un vehicule trouve toujours une
  sortie simple a rejoindre ; un vehicule piege par une impasse complete est repris par la
  reorientation gloutonne, puis, en dernier recours, par le palier de recuperation sur place de la
  Story 5.2. Il n'est jamais retire (AD-34).
- Les ratios authores et le determinisme par graine sont **inchanges** : quand tous les candidats
  sont eligibles, la suite tiree est celle de la surcharge sans restriction.

### 4. Rayon d'arrivee : mesure, et decision de perimetre

Mesure : `HasArrivedAtWaypoint` tranche sur **3 m**. Deux noeuds d'anneau de giratoire sont espaces
d'au plus **3,81 m** (corde a 37 deg sur un rayon d'axe de 6 m), et deux noeuds de decision d'une
croix ou d'un T sont a **2,83 m** l'un de l'autre (voisins en diagonale, a 2 m du centre) ou 4 m
(opposes). Le rayon d'arrivee **depasse donc l'espacement des noeuds de jonction** : une decision
peut etre prise avant que le vehicule soit au noeud.

Ce rayon n'est **pas borne ici**, et c'est une decision de perimetre, pas un oubli : le symptome de
bouclage est explique par les poids et supprime par la regle de non-bouclage, alors que borner le
rayon toucherait le suivi de trajectoire -- une poursuite de point a point, mecanique de conduite
desormais portee par la Story 5.12 (Tire Forces and Steering), qui remplace la poursuite point-a-point par un point de visee anticipe. Il ne sera rouvert que si
l'observation Play Mode montre encore un bouclage.

### Verification du correctif

- **EditMode** (`Story510LaneGraphAndRoutedTrafficTests`) : predicat de contact de surface sous et
  au-dessus de la tolerance, entree dont tous les contacts sont des contacts de surface (l'evenement
  n'est pas leve), garde des degats d'occupants, tirage eligible (jamais un successeur parcouru ;
  aucun eligible rend -1 ; ratios authores et determinisme inchanges quand tous les candidats sont
  eligibles), et **marche d'anneau** : le dernier « continuer » qui bouclerait est ineligible, la
  sortie est forcee a une branche.
- **PlayMode** (`Story510RoutedTrafficPlayModeTests`) : sur la fenetre d'observation dans `MVP_Run`
  en hote, la suite de `WaypointIndex` echantillonnee par vehicule ne contient **aucun noeud revu**,
  et aucun vehicule ne reste dans l'emprise d'un giratoire au-dela du budget d'un tour.
- **Manuel** : `MVP_Run` en Play Mode hote pendant plusieurs minutes -- aucun vehicule ne tourne en
  rond dans un giratoire ni autour du carrefour central ou d'une jonction en T ; monter sur le
  trottoir ou franchir la levre de 5 cm ne retire aucun PV. Le bump de 5 cm reste perceptible : c'est
  l'ecart accepte ci-dessus.

## Le district est un terrain de test, explicitement PAS le Level 1 de MVP 2

Le district authore dans `MVP_Run` existe pour exercer le trafic route : avenues, carrefours,
ronds-points, tunnels-portails. Il ne porte **aucun** contenu de niveau -- aucun commerce, equipement,
boss, acces vehicule ni recuperation de dechets, et aucun contrat run/niveau/checkpoint (AD-30). Sa
geometrie sera jetee ou remaniee quand le Level 1 de MVP 2 sera concu ; ce qui survivra, c'est le
**kit de modules et leurs cotes**, plus le code (graphe, tirage, budget, insertion), qui se reutilise
d'une ville a l'autre.

## Ecarts assumes

- **Ronds-points : geometrie seulement.** Une petite boucle de noeuds a trois branches, avec des
  ratios de sortie authores. Aucune regle de cession du passage, aucune priorite aux engages : c'est
  de l'arbitrage d'intersection, donc Story 5.18 (ex-5.11). Un test EditMode verifie qu'aucun mot de
  circulation (`Yield`, `Priority`, `GiveWay`) n'a fuite dans le driver ou le spawner.
- **Aucun arbitrage de jonction du tout.** Le carrefour central, les jonctions en T et les
  ronds-points se traversent sans priorite, sans controle d'occupation de la voie de sortie et sans
  echelle anti-blocage. Ce sont des points de collision assumes jusqu'a la Story 5.18 (ex-5.11).
- **Jointure par proximite O(n^2).** Balayage de tous les connecteurs contre tous, a la construction
  du graphe. Trivial a l'echelle du district (204 noeuds). Marque `ponytail:` dans
  `LaneGraph.JoinConnectors`, avec le chemin de sortie : indexer par cellule au-dela de quelques
  centaines de noeuds.
- **Une seule voie par sens.** Les modules n'authorent pas de voies paralleles. Le predicat MOBIL de
  la Story 5.9 et son minuteur restent donc sans liste de candidats : le changement de voie attend
  une story qui authore des voies multiples. Rien n'a ete ajoute ici pour le simuler.
- **Demi-tour possible dans un rond-point.** Un vehicule qui fait un tour complet de l'anneau peut
  ressortir par sa branche d'arrivee. C'est le comportement reel d'un giratoire, et le budget
  d'aretes borne les boucles pathologiques.
- **Blocs urbains deplaces.** Les quatre `Greybox_CityBlock_A_*` etaient a (+/-20, +/-20), soit
  exactement sur l'emprise de l'anneau. Ils sont maintenant a (+/-16, +/-16), dans les quatre
  quadrants interieurs -- ce qui est aussi leur place naturelle.
- **Reperes joueur revalides, pas deplaces a l'aveugle.** `PlayerSpawn` (10 ; 0,05 ; -18),
  `Greybox_PlayerCar_Parked` (10 ; 0,07 ; -22) et `VehicleRecoveryPoint` (9 ; 0 ; -18) ont ete
  verifies contre la geometrie finale : aucun n'est a l'interieur d'un module, et le plus proche
  noeud de voie est a 7,3 m -- donc hors chaussee, en bordure de l'avenue Sud, et loin de tout tunnel.
- **Tests d'autres stories reexprimes.** `Story15`, `Story16` et `Story34` epinglaient
  `Greybox_RoadLoop_*` par son nom. Ce que ces stories exigent -- une route greybox praticable a
  l'entree du run, un circuit borne et decore avec son repere de recuperation -- reste verifie, sur
  la geometrie du district. `Story57` et `Story59` epinglaient un effectif de 3 vehicules poses en
  scene ; ils verifient maintenant la cible authoree et le spawn runtime.
- **Le NavMesh vehicule n'a aucun consommateur runtime** avant la Story 5.17 (ex-5.12). Marque `ponytail:` dans
  la spec ; un test EditMode epingle l'exclusion des trottoirs pour qu'elle ne regresse pas en
  silence.
- **Levre de 5 cm au bord du district.** Le plan de sol passant 5 cm sous le plan de roulage, le
  pourtour du district presente une marche de 5 cm. Elle tombe sur le bord exterieur du trottoir,
  exactement ou une bordure se trouverait, et elle est trois fois plus basse que la marche de 15 cm
  qui envoyait les vehicules en l'air. Le trafic IA ne la rencontre jamais (il ne quitte pas les
  aretes authorees) ; seul le joueur qui sort de la chaussee la franchit. **Correctif du 2026-09-16 :**
  cette levre est **acceptee telle quelle** -- aucun collider n'est deplace -- et c'est desormais son
  effet qui change : un contact qui ne touche le vehicule qu'a hauteur de son dessous ne produit plus
  aucun degat (voir « Correctifs post-livraison » ci-dessus). Le bump reste.
- **Ilot de giratoire : visuel et collider divergent de 2 cm.** Le collider est affleurant (y = 0,
  comme tout le reste), le visuel est remonte de 2 cm pour ne pas z-fighter avec le disque de
  chaussee qu'il recouvre exactement. Un vehicule qui roulerait dessus s'y enfoncerait visuellement
  de 2 cm -- il n'y roule jamais, le graphe ne l'y mene pas.
- **Recouvrement cosmetique aux tunnels.** Aucun : le disque du rond-point s'arrete exactement au
  rayon ou commence le tunnel, y compris sur la diagonale. C'est la raison pour laquelle le
  rond-point est un disque (MeshCollider cylindrique) et non une dalle carree.

## Verification

- **EditMode**, filtre `Story510LaneGraphAndRoutedTrafficTests` : couvre la matrice d'E/S de la spec
  (tirage pondere et son determinisme, poids absents ou de somme nulle, successeur unique, aucun
  successeur, budget d'aretes, reorientation vers la sortie la plus proche, jointure de connecteurs
  sous et au-dela du seuil, jointure a contresens refusee, portail sortie-reutilisant-l-entree,
  graphe sans sortie, graphe sans `Def`, portail encombre mis en file, cul-de-sac), plus les gardes
  d'authoring (noeuds et colliders hors `Visual_*`, trottoirs hors du bake, ilot en aire dediee, aire
  `Sidewalk`, type d'agent `Vehicle`, absence de `NavMeshAgent`, composition du district, carrefour
  central et ses ratios, ronds-points sans regle de circulation) et de source (aucun effectif
  litteral, un seul chemin de despawn et il est conditionne au portail de sortie, `RouteWaypoints`
  disparu).
- **EditMode**, suite complete : les Stories 1.5, 1.6, 3.4, 5.2, 5.7 et 5.9 ont ete reexprimees, pas
  desactivees.
- **PlayMode**, filtre `Story510RoutedTrafficPlayModeTests` : dans `MVP_Run` en hote, sur une fenetre
  d'observation, tout vehicule apparu l'est a un portail d'entree, tout vehicule disparu l'etait a un
  portail de sortie, l'effectif atteint la cible authoree sans la depasser, la place liberee par une
  sortie est reprise, et les vehicules en jeu ont emprunte des parcours differents.
- **Manuel, vue Scene** : racine `LaneGraph` selectionnee, le gizmo doit montrer un graphe connexe,
  le sens des aretes lisible, aucun connecteur orphelin en alerte rouge.
- **Manuel, fenetre Navigation** : le type d'agent `Vehicle` existe, l'aire `Sidewalk` existe, et la
  surface du district ne porte aucune surface navigable sur les trottoirs.
