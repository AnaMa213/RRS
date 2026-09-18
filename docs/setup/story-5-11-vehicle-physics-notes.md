# Story 5.11 -- Vehicle Chassis, Wheels, and Suspension

Note de livraison et de verification. Spec : `_bmad-output/implementation-artifacts/spec-5-11-vehicle-chassis-wheels-and-suspension.md`.
Decision d'architecture : AD-35 (`ARCHITECTURE-SPINE.md`), course correction du 2026-09-18.

## Ce qui est livre

| Fichier | Role |
| --- | --- |
| `Assets/RoadRage/Features/Vehicles/VehicleProfile.cs` | La donnee physique complete : masse, centre de masse, tenseur d'inertie, roues, ressort, amortisseur, longueur au repos, debattement, anti-roulis, rappel d'assiette, amortissement tangage/roulis, masque de sol, tolerance de contact de surface. Immutable a l'execution. |
| `Assets/RoadRage/Features/Vehicles/VehicleWheel.cs` | Implantation authoree d'une roue (origine du raycast, rayon, essieu, directrice/motrice). Les roues sont de la donnee : le prefab n'en gagne aucune. |
| `Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs` | Les fonctions **pures** : compression, ressort + amortisseur, anti-roulis par essieu, echantillon de telemetrie de caisse. Aucune dependance a `Rigidbody`, `Time` ou une scene. |
| `Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs` | Le Def authore : id stable en minuscules, profil, `TryValidate` qui refuse en nommant le champ fautif. Pas de catalogue, comme `DriverProfileDef`. |
| `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` | LE composant physique, porte par le prefab joueur **et** le prefab IA : applique le profil au `Rigidbody`, puis contact au sol, suspension et anti-roulis par raycasts par roue. |
| `Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset` | Profil authore par defaut, reference par les deux prefabs (meme GUID dans les deux YAML). |
| `Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs` | Telemetrie par roue (compression, contact) + caisse (vitesse, laterale, glissement, derive). Lecture seule, gardee par `Debug.isDebugBuild`. |
| `Assets/RoadRage/Tests/EditMode/Story511VehicleChassisWheelsAndSuspensionTests.cs` | 16 gardes EditMode : fonctions pures et matrice de cas limites, authoring du profil, parite des prefabs, invariants de code, geometrie de la bordure. |
| `Assets/RoadRage/Tests/PlayMode/Story511VehicleChassisWheelsAndSuspensionPlayModeTests.cs` | 2 preuves runtime : montee de la bordure authoree a basse vitesse, absence de projection a vitesse de conduite. |

Modifies : `NetworkedVehicleDriverController` (contournement de surface retire, reglages de chassis migres, annulation roulis/tangage retiree, `ConfigureArcadeBody` supprime), `NetworkedAIVehicleDriverController` (commentaire d'axe vertical), `RoadRageNetcodeSmokeTestAutoStart` (montage de la vue), les deux prefabs vehicules, `Greybox_Intersection.prefab` (bordure), `Story510LaneGraphAndRoutedTrafficTests`, `story-5-10-lane-graph-district-notes.md`.

## Le contrat de bordure

- **Hauteur authoree : 0,12 m**, epaisseur 0,3 m, longueur 4 m, quatre segments `Col_Curb_*` sous `Collision/` du carrefour central, face interieure exactement a `|z| = 4 m`.
- Entierement dans la bande trottoir de 4 m, jamais dans les 8 m de chaussee. **Aucun `LaneNode` deplace, ajoute ni supprime.**
- Degagement de voie mesure : `4 - 2 - (2,06 / 2) = 0,97 m` par cote, conformement aux cotes figees.
- Le nom `Col_Curb_*` evite `Col_Sidewalk_*`, que `SidewalksCarryTheDedicatedAreaAndStayOutOfTheVehicleBake` exige a zero sur le giratoire.
- **La contrepartie visuelle est un livrable du kit artistique, pas de cette story** : la bordure est de la geometrie de collision. L'AC ne demande que les trois controles physiques, et les surfaces roulables du district sont encore des tuiles Synty affleurantes. Une tuile de bordure devra etre posee a **0,12 m** et la garde du test mise a jour avec elle.

## Mesures de verification

| Verification | Resultat |
| --- | --- |
| `.\scripts\validate.ps1 -TestMode EditMode` | **537/537 verts**, 0 erreur Console. 523 tests avant la story, +16 (nouvelle fixture) -2 (tests de surface retires). |
| `.\scripts\validate.ps1 -TestMode PlayMode` (16:3x) | **34/35**, dont les deux tests Story 5.11 **verts**. L'unique echec est `Story59ParameterizedDriverModelPlayModeTests.AiVehiclesDriveAlongTheirRouteUnderTheAuthoredDriverProfile`, qui attend `MVP_Run` et trouve `MainMenuLobby` -- il echoue **avant** toute conduite, sur le chargement de scene, et n'a pas de lien avec la couche physique. |
| `.\scripts\validate.ps1 -TestMode PlayMode` (16:39, puis 16:41) | **Aucun test execute**. Le harnais PlayMode n'execute plus rien, en synchrone comme en asynchrone. AD-8 fait correctement echouer ferme, mais la preuve runtime n'est pas reproductible a la demande. |

**Conclusion de verification** : la preuve EditMode est complete et verte. La preuve runtime a ete obtenue une fois (34/35, les deux tests 5.11 verts) et **n'est pas reproductible dans l'etat actuel du harnais PlayMode**. Les valeurs mesurees (`[Story511] bordure ... excursion verticale ...`) sont emises par `Debug.Log` dans le test : elles sont a relever au prochain run PlayMode vert et a consigner ici. En l'etat, la story ne peut pas pretendre a une porte PlayMode verte.

## Deux constats a ne pas relire comme des regressions

1. **La garde de hauteur de la Story 5.10 ne gardait rien.** `Collider.bounds` sur un prefab **non instancie** renvoie une boite de taille nulle : `bounds.max.y` y vaut la position du transform, soit `0`. La comparaison `<= 0.0001` etait donc vraie par construction, pour tout collider. Mesure sur `Greybox_Intersection.prefab` : `boundsMin == boundsMax == (6.000, 0.060, 4.150)` pour un collider de `size = (4.000, 0.120, 0.300)`. La version reecrite mesure sur une **instance** (`PrefabUtility.InstantiatePrefab`), ce qui est la seule facon d'obtenir une `bounds` exploitable. Le meme piege a ete evite dans la nouvelle fixture.
2. **Le harnais PlayMode est plus abime que documente.** `build-workflow-rules.md` et le registre decrivent « rouge 3/33, non filtrable ». Mesure du 2026-09-18 : la suite etait **verte 33/33** avant cette story, puis un run a rendu 34/35, puis deux runs ont rendu **zero test**. Le CLI explique le mode d'echec : `run_tests --mode PlayMode` synchrone ne peut pas fonctionner (« entering play mode triggers a domain reload that drops the request »), et il faut `--async_tests` -- mais le chemin asynchrone a lui aussi rendu zero test ici. `scripts/validate.ps1` reste fail-closed, donc rien n'est masque ; c'est la **preuve** qui manque, pas la garde. C'est le prerequis bloquant deja enregistre pour la Story 5.14, et il est plus large que prevu : il ne s'agit pas d'un filtre a reparer mais de l'execution PlayMode elle-meme.

## Etat intermediaire assume (5.11 -> 5.12)

`VehiclePhysicsBody` propriete **l'axe vertical** (gravite, ressort, amortisseur, anti-roulis). Les deux controleurs continuent d'ecrire la vitesse longitudinale et laterale en bloc, et **relisent-reecrivent** la composante verticale a l'identique -- un read-modify-write, jamais une decision de mouvement. Le commentaire d'en-tete de `VehiclePhysicsBody` porte la date et la story qui leve cet etat (5.12, Tire Forces and Steering).

Consequence de forme : le collider de caisse (dessous a 0,158 m au-dessus du sol au repos) **passe au-dessus** d'une bordure de 0,12 m. Ce n'est pas un raccourci, c'est la geometrie authoree : c'est la roue qui suit la marche, et le test PlayMode mesure precisement cela (hauteur du point de contact).

## Ce qui n'est pas fait

- Aucune tuile visuelle de bordure (art, voir ci-dessus).
- `RecoverAtWaypoint` (teleportation de recuperation) est **intact** : sa suppression appartient a la Story 5.14 (`ANO-5.10-03` AC6).
- Le remplacement des ecritures `linearVelocity` par des efforts aux roues appartient a la Story 5.12.
- Les roues visuelles et le reglage de pneu realiste restent differes (AD-14, AD-7 inchange).

## Trois anomalies de recette et leur cause racine (2026-09-18)

### 1 et 3 -- un vehicule touche reste couche ; un vehicule gare se penche et bouge tout seul

**Cause principale : l'anti-roulis etait applique A L'ENVERS.** Le cote le plus comprime est celui ou la caisse s'affaisse : c'est lui qu'il faut CHARGER. L'appelant chargeait le cote soulage, donc l'anti-roulis ENTRETENAIT l'inclinaison au lieu de la corriger. Un vehicule bouscule partait sur le flanc et y restait.

La fonction pure etait pourtant juste -- sa documentation decrivait l'application correcte. C'est l'appelant qui ne la respectait pas, et le test EditMode n'attachait la convention qu'a la fonction pure, jamais a son application. **Correctif structurel** : `ResolveAntiRollForce` (scalaire signe, lisible dans les deux sens) devient `ResolveAntiRollForces(out leftForce, out rightForce)`, et une garde de forme verifie que l'appelant ne reinterprete pas le sens. Un scalaire signe se lit dans les deux sens ; une paire non.

**Seconde cause, meme symptome** : rien ne redressait une caisse hors contact. Au-dela d'une vingtaine de degres, les ancrages de roue montent au-dessus de la longueur au repos, les rayons ne touchent plus, la suspension cesse d'exister -- et le seuil de retournement de l'IA (produit scalaire 0,35, environ 70 degres) est bien trop haut pour rattraper une caisse penchee de 30. Deux parametres authores ajoutes au profil : `attitudeLevellingRate` (couple de rappel proportionnel a l'inclinaison, nul a l'aplomb) et `attitudeDamping` (amortissement tangage et roulis ; **le lacet n'est jamais amorti**, il porte la direction).

**Troisieme cause, meme symptome** : la vitesse de compression etait derivee par difference finie **a travers une perte de contact**. Au premier appui apres un decollement, la compression passe de 0 a sa valeur en un pas -- plusieurs metres par seconde -- et l'amortisseur injecte un pic de force a chaque reprise : la caisse tremble sans fin et ne se pose jamais. La derive est desormais nulle quand la roue ne touchait pas la frame precedente.

### 2 -- les vehicules des joueurs prennent des degats en montant un trottoir

**Cause** : la discrimination de contact de surface avait ete retiree, sur la premisse que la suspension la rendait inutile. La premisse est fausse : le dessous du collider est a 0,158 m au repos, mais un plongeon de caisse (freinage, appui) le fait descendre sous les 0,12 m de la bordure authoree, et le contact produit alors un choc a plus de 3 m/s.

**Correctif** : la regle est retablie, mais **adossee au profil authore** (`Profile.SurfaceContactTolerance`, 0,15 m : elle couvre la bordure de 0,12 m sans jamais couvrir la face d'un mur), et le predicat pur vit dans `VehicleSuspensionModel.IsSurfaceContact` ou il se prouve en EditMode. Un contact dont TOUS les points ne touchent que le dessous du vehicule ne produit aucun degat ; un mur, une autre voiture ou un choc par l'arriere touchent la caisse bien plus haut et gardent leurs degats de la Story 3.5. Le garde `vehicleDamage > 0` de `RunFlowController` reste intact.

**Ecart assume, a valider par l'humain** : l'AC de la story demandait la suppression du predicat, sur un raisonnement que la recette a demontre faux. La demande produit -- etre blesse par un mur, une voiture ou un choc, jamais par un relief franchi -- l'emporte ; la regle revient sous une autre forme, authoree, partagee, et prouvee par un test de discrimination.

## Mesures apres correctifs

| Verification | Resultat |
| --- | --- |
| `unity cmd run_tests --mode EditMode` | **542/542 verts, 0 echec.** 537 avant correctifs, +5 gardes (paire anti-roulis, application non reinterpretable, rappel d'assiette, amortissement qui epargne le lacet, discrimination surface/obstacle). |
| `recompile_status` | `completed`, `failed: false`, `errors: []`, y compris apres recompilation forcee. |
| `.\scripts\validate.ps1 -TestMode EditMode` | **rouge sur la porte Console, pas sur le code** -- voir le constat ci-dessous. |
| `.\scripts\validate.ps1 -TestMode PlayMode` | inchange : zero test execute. |

### Constat d'outillage : la porte Console est empoisonnable par l'historique

Les erreurs restantes sont celles d'une compilation **ratee** (un guillemet echappe ecrit par erreur dans le fixture), corrigee depuis. Elles continuent d'apparaitre alors que le fichier sur disque ne contient plus aucun antislash, que `recompile` repond `up_to_date`, qu'une recompilation forcee repond `failed: false, errors: []`, que `clear_console` repond `cleared: true`, et meme apres suppression de `Temp/pipeline_console_log.json`.

Elles reviennent avec **le meme numero de sequence et le meme horodatage** pendant que le curseur de lecture avance : `unity cmd console` rejoue un historique au lieu de rendre l'etat courant, et `validate.ps1` l'appelle **sans `--since`**. Consequence : toute erreur de compilation passee rend la porte rouge pour le reste de la session d'Editeur, meme corrigee. C'est un defaut de l'outillage de verification, pas un etat du code, et il rejoint le constat deja fait sur l'execution PlayMode. A traiter avec le prerequis bloquant de la Story 5.14.

## Deuxieme passe de recette (2026-09-18) : decollage et glace

### Le vehicule encaisse violemment decollait et continuait de monter

**Cause** : la force d'amortisseur vient d'une difference finie -- `damper x derivee de compression` -- et **rien ne la bornait**. Sur un choc violent, cette derivee atteint plusieurs dizaines de metres par seconde et la force depassait plusieurs fois le poids du vehicule ; comme elle est appliquee a chaque frame ou la roue touche, chaque reprise de contact re-injectait plus d'energie que la gravite n'en retire : le vehicule montait au lieu de retomber.

**Correctif** : la force d'une roue est plafonnee a **quatre fois la charge statique qu'elle porte**. La borne est **derivee du vehicule** (`masse x gravite / nombre de roues`, multipliee par un facteur documente dans le modele pur) et non un nombre absolu : elle suit la masse sans reglage. Mesure : a pleine course le ressort seul ne fait que 2,7 fois la charge statique, donc le plafond ne change rien au repos -- il n'agit que dans le choc, qui est exactement le probleme.

### Le vehicule a l'arret glissait comme sur de la glace

**Cause** : la couche physique **n'avait aucun frottement**. Un vehicule pose sur quatre rayons n'a ni resistance laterale ni resistance au roulement ; la seule chose qui freinait un glissement lateral etait le `lateralGrip` du controleur, qui ne s'applique **que si un conducteur est assis** (`ApplyPhysics` retourne tot quand le siege est libre). Un choc de flanc sur une voiture garee l'emportait donc sans aucune opposition.

**Correctif** : frottement de contact applique par roue portante, borne par la charge que cette roue porte -- un pneu ne transmet pas plus que ce que le sol lui rend -- avec une attenuation sous 0,5 m/s pour que le vehicule s'immobilise au lieu de brouter. Deux coefficients authores dans le profil : `lateralFrictionCoefficient` (1,2, choix arcade assume) et `rollingResistanceCoefficient` (0,03, volontairement marginal pour ne pas lutter contre la conduite). **C'est un contact, pas un modele de pneu** : la Story 5.12 le remplace par un glissement progressif pilote par le slip.

Consequence a surveiller : le frottement lateral **s'ajoute** au `lateralGrip` du controleur quand un conducteur est assis (36 + 1,2 g). Le comportement en virage peut donc se raffermir legerement. Si c'est trop, c'est un coefficient author e, pas une reprise de code.

| Verification | Resultat |
| --- | --- |
| `unity cmd run_tests --mode EditMode` | **544/544 verts, 0 echec** (542 avant cette passe, +2 gardes : plafond de force de suspension, frottement de contact borne). |
| Test Runner de l'Editeur, mode PlayMode | **tout est vert** (mesure utilisateur du 2026-09-18). C'est la seule mesure PlayMode de cette story. |
| `unity cmd run_tests --mode PlayMode` (agent) | **impossible** : en synchrone le CLI repond que le rechargement de domaine perd la requete, et en asynchrone (`--async_tests`) le statut revient `completed` avec zero test et une duree nulle. La porte PlayMode ne peut donc pas etre produite par l'agent ; elle vient de l'Editeur et doit etre citee comme telle. |
| `.\scripts\validate.ps1 -TestMode EditMode` | toujours rouge sur la porte Console historique, pas sur le code. |

**Ce que la porte PlayMode verte change** : les deux tests `Story511…PlayModeTests` (montee de la bordure a basse vitesse, absence de projection a vitesse de conduite) sont desormais prouves sur du vrai pas de physique, et les trois anomalies de recette -- vehicule couche, degats de trottoir, glissade de flanc -- ont ete corrigees puis rapportees comme telles par l'humain. **Ce qu'elle ne couvre pas** : le controle « les vehicules IA traversent le carrefour sans toucher la bordure en conduite nominale » n'a **pas de test dedie** ; il repose sur la geometrie EditMode et sur l'observation. Le point de visee anticipe et le rayon d'arrivee qui le corrigeraient appartiennent a la Story 5.12.

### Vehicule de developpement indestructible : livre, eprouve seulement a l'oeil

Demande de recette du 2026-09-18 : un vehicule jouable indestructible, distingue par un visuel POLYGON.

**J'avais d'abord refuse sur une interdiction mal lue, et c'etait une erreur.** `AGENTS.md` interdit d'utiliser un **prefab** Synty comme racine jouable ou reseau -- pas d'utiliser leurs **meshes**. AD-27 prescrit l'inverse : les meshes finaux remplacent les enfants visuels sous un root projet.

Ce qui est livre : `Assets/RoadRage/Prefabs/Dev_IndestructibleCar.prefab`, duplicata du prefab joueur (donc memes composants, meme `VehicleProfileDef`, meme identite reseau) dont l'enfant `Visual_Greybox_PlayerCar` est remplace par `SM_Veh_Car_Sedan_01` du pack POLYGON City -- echelle 0,9, **colliders Synty retires** (MeshCollider + 4 SphereCollider de roue) pour qu'ils ne se cumulent pas avec le collider gameplay du projet. Il porte `DevIndestructibleVehicle`, marqueur de developpement qui ne detient aucun etat et ne se replique pas.

L'immunite est obtenue **en un seul point** : `NetworkedVehicleDriverController.OnCollisionEnter` ne leve aucun evenement pour ce vehicule, or c'est cet evenement qui alimente tout le chemin de degats -- vehicule puis occupants. Aucune couche de degats n'a donc a connaitre le developpement.

Usage : la touche **V** hors session reseau (`RunFlowController.devVehiclePrefab`, rebranche sur ce prefab dans `MVP_Run` ; **Shift+V** pour la variante cible de rage). **Correction de revue du 2026-09-18** : la premiere redaction de cette note affirmait que ce vehicule n'etait « monte ni par le lobby ni par la composition de run ». C'est faux depuis la passe de recette de l'humain, qui a place deux instances de ce prefab **directement dans `MVP_Run`** (`MVP_RageTargetVehicle_1/2`) et l'a enregistre dans `Assets/DefaultNetworkPrefabs.asset`. C'est un choix de developpement assume, pas une anomalie -- mais la note doit decrire l'etat reel, pas l'etat souhaite. Consequence a garder en tete : ces instances de scene ne sont couvertes par aucune garde de non-regression, et `DefaultNetworkPrefabs.asset` est consomme par `LobbyFlowController.defaultNetworkPrefabs`.

Ce qui reste a eprouver : l'echelle et le placement du mesh Synty demandent une confirmation visuelle, et la conduite de ce vehicule n'a pas ete observee par l'agent en Play Mode.

## Revue (etape 4) : trois couches, neuf correctifs, trois reports

Couches executees : `blind-hunter` (toujours), `edge-case-hunter` (changement non trivial : physique, collisions, prefabs, scene) et `verification-gap` (comportement observable). `security-review` **non declenchee** : aucun RPC introduit, aucun usage de `SenderClientId` modifie, aucune autorite deplacee, aucune permission de `NetworkVariable` elargie, aucune donnee client atteignant un chemin autoritaire. L'enregistrement du prefab de dev dans `DefaultNetworkPrefabs.asset` n'est pas une frontiere de confiance.

Aucun `intent_gap`, aucun `bad_spec` : **pas de boucle de re-derivation**. Neuf correctifs appliques directement, dont deux corrigent des affirmations de la story elle-meme :

- **Le banc PlayMode mesurait la bordure sur un prefab NON INSTANCIE.** `Collider.bounds` y degenere en la position du transform, donc la lecture rendait 0,06 m -- la MOITIE de la bordure authoree. Le banc rejouait une marche de 6 cm en croyant en rejouer une de 12, et la fixture EditMode comme cette note affirmaient le contraire. Corrige : mesure sur `PrefabUtility.InstantiatePrefab`, plus une assertion qui lie la hauteur du banc a la constante du contrat.
- **Le profil authore ne stockait pas ses derniers reglages.** Les cinq cles ajoutees apres la creation de l'asset (assiette, amortissement, frottements, tolerance) n'etaient **pas serialisees** : les valeurs appliquees venaient des initialiseurs C# du Def. L'asset presente comme « point de reglage unique » ne portait donc que les reglages d'origine. Corrige par resauvegarde forcee : les cinq cles sont desormais dans l'asset.
- **Un corps dynamique pouvait servir de sol** : le masque est la couche par defaut, qui porte les autres vehicules et les personnages. Une roue posee sur le toit d'une autre voiture la prenait pour le sol et poussait contre elle. Garde ajoutee : un corps non cinematique n'est jamais un sol.
- **Un `BindProfile` refuse laissait l'ancien profil en place** (`HasProfile` vrai, `Profile` perime). L'etat est desormais purge sur chaque chemin d'echec.
- **Pic d'amortisseur a chaque teleportation** : la vitesse de compression est une difference finie entre deux frames, donc un deplacement discontinu en produit une absurdite. `ResetSuspensionState()` est expose et appele par les deux chemins de recuperation (joueur et IA).
- **Immunite de dev accordee a un marqueur desactive** : le marqueur doit etre actif pour immuniser.
- **Boucle de banc sans borne d'iterations** dans le test de traversee : un vehicule coince aurait fige la suite au lieu d'echouer lisiblement.
- **Garde de dessous en coordonnees melangees** (hauteur locale + `bounds` monde) : ramenee en local.
- **Commentaire perime** citant `ApplyStabilityAssist`, methode supprimee par la story.

Trois findings reportes au registre (details et demonstration dans `deferred-work.md`) : le rayon de roue aveugle quand son origine est dans le sol, le sens applique de l'anti-roulis garde par une assertion textuelle seulement, et le controle IA / bordure sans assertion.

Un finding ecarte : la re-serialisation complete de `Greybox_Intersection.prefab` (1755 insertions / 1539 suppressions pour quatre colliders ajoutes). La revue a verifie que les trois fileID cibles par les surcharges de `MVP_Run.unity` existent toujours dans les deux versions, et n'etablit aucune consequence. C'est du bruit de diff a surveiller, pas un defaut.

Etat apres correctifs : **544/544 en EditMode**. La porte PlayMode reste celle de l'Editeur, mesure utilisateur.
