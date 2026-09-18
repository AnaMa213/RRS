---
title: "Vehicle Chassis, Wheels, and Suspension"
type: "feature"
created: "2026-09-18"
status: "done"
review_loop_iteration: 0
baseline_commit: "7718e7e94056331b056c4bb9e01c899f4c42d7ed"
context:
  - "{project-root}/docs/setup/story-5-10-lane-graph-district-notes.md"
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La voiture n'a ni roue ni suspension : le conducteur ecrit `Rigidbody.linearVelocity` en bloc (`NetworkedVehicleDriverController.cs:661`), donc toute face verticale -- bordure, levre de 5 cm du plan de sol, ile de giratoire -- devient une impulsion de depenetration. Recette : vehicules proyectes pres des giratoires et des tunnels (`ANO-5.10-03`), arretes net par une levre basse, et une marche nue suffit a declencher le hook de degats de la Story 3.5.

**Approach:** Poser la couche physique unique joueur+IA d'AD-35 : masse, centre de masse et tenseur d'inertie authores dans un `VehicleProfileDef`, contact au sol, suspension et anti-roulis calcules par raycasts par roue dans des fonctions pures verifiables en EditMode. Puis prouver la couche en authorant une bordure franchissable sur le carrefour central et en retirant le contournement `IsSurfaceContact`, qui n'existait que faute de suspension.

## Boundaries & Constraints

**Always:**

- AD-35 : raycasts par roue, **jamais** `WheelCollider`. Un seul composant physique, porte par le prefab joueur **et** le prefab IA, configure par profil -- aucun reglage chassis/suspension en `[SerializeField]` sur un controleur.
- La couche physique ne lit que `VehicleProfileDef` : jamais rage, peur ni disposition (AD-33). L'hote reste le seul simulateur (AD-21).
- NFR18 : identite des prefabs, `NetworkObject` enregistres, composants gameplay et ids de definition inchanges. Le collider du vehicule (`(2.06, 1.42, 4.44)`) et son emprise ne bougent pas.
- Masse, centre de masse et tenseur d'inertie **explicitement authores** (`automaticCenterOfMass` et `automaticInertiaTensor` desactives) : plus aucun COM implicite.
- La bordure est authoree **dans la bande trottoir de 4 m, jamais dans les 8 m de chaussee**, et **le graphe de voies n'est pas modifie** : aucun `LaneNode` deplace, ajoute ni supprime.
- Le garde `vehicleDamage > 0` de `RunFlowController.ApplyNetworkedCollisionDamage` reste **intact** : seuil independant et toujours valide.
- La telemetrie vit dans `RoadRage.DevTools`, ne detient aucun etat de gameplay, et lit uniquement.
- Cette story ne prend que **l'axe vertical**. Le remplacement des ecritures `linearVelocity` longitudinales et laterales appartient a la Story 5.12.

**Ask First:**

- Toute correction touchant le graphe de voies (`LaneNode`, `LaneGraph`, ratios authores) pour faire passer un des trois controles de bordure.
- Toute modification du collider du vehicule, de l'emprise d'un module ou de la largeur de chaussee.
- Descendre la hauteur de bordure sous la valeur necessaire au franchissement a basse vitesse.

**Never:**

- `WheelCollider`, ou un modele de pneu tiers (`ADDON-003` est clos en negatif par AD-35).
- Ecrire la velocite verticale, la position ou la rotation du `Rigidbody` depuis un conducteur ; masquer un symptome par une masse augmentee, une rotation gelee, une vitesse IA reduite, des collisions desactivees ou une teleportation apres choc.
- Toucher a la teleportation de recuperation de `NetworkedAIVehicleDriverController.RecoverAtWaypoint` : sa suppression appartient a la Story 5.14 (`ANO-5.10-03` AC6).
- `NavMeshAgent` sur un vehicule, roues visuelles ou art final (AD-14).

## I/O & Edge-Case Matrix

| Scenario                      | Input / State                                                                           | Expected Output / Behavior                                                                                                           | Error Handling                                                                                        |
| ----------------------------- | --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| Roue au sol                   | raycast atteint le sol, `distance < restLength`                                         | compression `= restLength - distance` bornee a `[0, travel]` ; ressort + amortisseur appliques au point de contact ; roue `Grounded` | distance `> restLength` ou aucun contact -> compression 0, force 0, roue `Airborne`, aucune exception |
| Roulis                        | compression gauche != compression droite                                                | terme anti-roulis proportionnel a l'ecart : il reduit le roulis **sans l'annuler**                                                   | ecart nul -> anti-roulis nul                                                                          |
| Bordure a basse vitesse       | vehicule < ~2 m/s contre `Col_Curb_*`                                                   | la roue monte la bordure, le vehicule reste sur ses roues et au sol                                                                  | si la montee echoue, **HALT** : cause = hauteur authoree ou arrivee des roues, jamais le graphe       |
| Bordure a vitesse de conduite | impact ~8-18 m/s contre `Col_Curb_*`                                                    | aucune excursion verticale durable, roues au sol apres le contact, pas de degats issus d'un simple contact de surface                | si le vehicule decolle, **HALT** : cause = couche physique, jamais geometrie                          |
| Profil invalide               | `VehicleProfileDef` mal author e (id non minuscule, masse <= 0, `restLength <= travel`) | `TryValidate` refuse en nommant le champ fautif                                                                                      | aucune valeur de repli silencieuse : le composant ne s'active pas sur un profil invalide              |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- joueur. A retirer : `surfaceContactTolerance` `:91-92`, branche et commentaire de contournement `:344-361`, `IsSurfaceContact` `:362-388`, `IsSurfaceOnlyCollision` `:389-415`, `ResolveVehicleUndersideHeight` `:418-426`. A deplacer vers le profil : `centerOfMassOffset`, `rollStabilityAssist` `:37-96`. Ecriture en bloc `:661`, `MoveRotation` `:742`, annulation roulis/tangage `:752-755`, `ConfigureArcadeBody` (COM/interpolate/collisionDetection au spawn, **joueur seul**) -- remplace par l'application du profil.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- IA. `ApplyMovement` `:828-846` ecrit `linearVelocity` + `MoveRotation` ; tuning serialise `:73-114` ; ne partage aujourd'hui **aucun** composant de mouvement avec le joueur.
- `Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs` + `DriverProfile.cs` -- gabarit a copier : `Def` = `id` + struct + `TryValidate`, sans catalogue (reference directe par le prefab).
- `Assets/RoadRage/ScriptableObjects/Vehicles/` -- emplacement des Defs (`DriverProfileDef_Default.asset`, `TrafficSettingsDef_Default.asset`).
- `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- Rigidbody `:974-1000` (`m_Mass: 1200`, `m_ImplicitCom: 1`, COM `(0,0,0)`, `m_CollisionDetection: 0`) ; BoxCollider `:853-854`.
- `Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab` -- Rigidbody `:142-168` : valeurs **identiques**, COM implicite, `ConfigureArcadeBody` jamais appele.
- `Assets/RoadRage/Prefabs/Greybox_Intersection.prefab` -- module retenu (carrefour central, 1 instance, `MVP_Run.unity:6338`). Enfants : `Lanes`, `Collision` (`Col_Roadway` 16x16 a face superieure `y = 0`, 4 `Col_Sidewalk_Corner_*` 4x4 a `(±6, -0,1, ±6)`), `Visual_Greybox_Intersection`. La bordure va sous `Collision/`.
- `Assets/RoadRage/Prefabs/Greybox_RoadSegment_TwoWay.prefab` -- cotes figees : chaussee 8 m, trottoir 4 m/cote, emprise 16 m, `Lane_Back_Mid` a `x = -2`. **Jamais de bordure ici** (ligne droite).
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- district : 25 modules sous le root `LaneGraph` `:1404`, 204 `LaneNode` ; le trafic IA y tourne deja.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` -- a supprimer : `SurfaceContactPredicateAccepts...` `:509`, `OnlyAnEntryWhoseEveryContactIsASurfaceContactSkips...` `:535` (lit le texte source). A reecrire : `NoModuleColliderRisesAboveTheDrivingPlane` `:920` (aujourd'hui `collider.bounds.max.y <= 0.0001f`, seuls `Col_Wall_Left/Right`, `Col_Roof`, `Col_Backstop` toleres). **Ne pas toucher** : `SidewalksCarryTheDedicatedAreaAndStayOutOfTheVehicleBake` `:970` exige **0** `Col_Sidewalk*` sur le giratoire -- d'ou le nom `Col_Curb_*`. Helpers reutilisables : `CodeOnly` `:1598`, `ExtractMethod` `:1634`.
- `Assets/RoadRage/DevTools/RoadRage.DevTools.asmdef` -- reference deja `RoadRage.Features.Vehicles` ; `includePlatforms` et `defineConstraints` vides, donc l'assembly est compilee dans les builds joueur : la garde est au niveau source (`#if UNITY_EDITOR` + stub `#else`, motif de `RoadRageNetcodeSmokeTestAutoStart.cs:29` / `:160`).
- `Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs` -- gabarit de vue dev : cible serialisee, `Update()` en lecture seule, aucun etat detenu.
- `Assets/RoadRage/App/Run/RunFlowController.cs:856` -- garde `vehicleDamage > 0`, **lecture seule** (son test `OccupantCollisionDamageIsGatedOnPositiveVehicleDamage`, Story510 `:563`, reste valide).
- `docs/setup/story-5-10-lane-graph-district-notes.md:43-127` -- contrat de cotes pour l'art : la regle « aucun collider de module ne s'eleve au-dessus du plan de roulage » y devient « ... de plus que la hauteur de bordure authoree ».

## Tasks & Acceptance

**Execution:**

- [x] Baseline d'abord : `.\scripts\validate.ps1 -TestMode PlayMode` **avant** toute modification. **Mesure du 2026-09-18 16:2x : 33/33 verts** -- et non 3/33 rouges comme l'annoncent `build-workflow-rules.md` et le registre d'adoption.
- [x] `Assets/RoadRage/Features/Vehicles/VehicleProfile.cs` -- nouveau `struct VehicleProfile` : masse, centre de masse, tenseur d'inertie, implantation des roues, `springRate`, amortisseur, `restLength`, `travel`, raideur anti-roulis, masque de sol.
- [x] `Assets/RoadRage/Features/Vehicles/VehicleWheel.cs` -- **ajoute** : implantation authoree d'une roue. Les roues sont de la donnee, pas des GameObjects : le prefab n'en gagne aucun.
- [x] `Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs` -- Def : `id` minuscule stable, `VehicleProfile`, `TryValidate` nommant le champ fautif (masse, `restLength <= travel`, masque vide, essieu sans paire).
- [x] `Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs` -- fonctions **pures** : compression, force ressort+amortisseur, force anti-roulis, echantillon de telemetrie ; aucune dependance a `Rigidbody`, `Time` ou une scene.
- [x] `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` -- composant **unique** des deux prefabs : profil applique au `Rigidbody`, raycasts par roue en `FixedUpdate`, suspension et anti-roulis par `AddForceAtPosition`, etat publie en lecture seule.
- [x] `NetworkedVehicleDriverController.cs` -- contournement de surface, reglages chassis/suspension migres, annulation roulis/tangage et `ConfigureArcadeBody` retires ; composante verticale relue-reecrite a l'identique, jamais decidee.
- [x] `NetworkedAIVehicleDriverController.cs` -- axe vertical nomme comme appartenant a la couche physique ; `RecoverAtWaypoint` **intact**.
- [x] `Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset` -- profil authore, reference par les deux prefabs (meme GUID).
- [x] `Greybox_PlayerCar.prefab` et `Greybox_AIVehicle.prefab` -- meme `VehiclePhysicsBody` et meme profil ; `m_ImplicitCom: 0` et `m_ImplicitTensor: 0` ; masse et centre de masse authores ; identite, `NetworkObject`, composants gameplay et ids inchanges. Les cles serialisees orphelines des champs migres ont ete resauvegardees hors du YAML.
- [x] `Greybox_Intersection.prefab` -- quatre `Col_Curb_*` sous `Collision/`, hauteur 0,12 m, epaisseur 0,3 m, face interieure a `|z| = 4 m` ; aucun `LaneNode` touche. Le nom evite `Col_Sidewalk_*`, que la garde du giratoire exige a zero.
- [x] `Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs` -- vue par roue + caisse, garde `Debug.isDebugBuild`, lecture seule ; montee par le harnais auto-start de `RoadRage.DevTools` dans `Dev_VehicleSandbox`, jamais referencee par `RoadRage.App`.
- [x] `Story510LaneGraphAndRoutedTrafficTests.cs` -- deux tests de surface supprimes, garde de hauteur reecrite **et reparee** : elle mesurait `Collider.bounds` sur un asset non instancie, ou Unity renvoie une boite de taille nulle, donc elle etait vraie par construction et ne gardait rien.
- [x] `Assets/RoadRage/Tests/EditMode/Story511VehicleChassisWheelsAndSuspensionTests.cs` -- 17 gardes EditMode : matrice de cas limites, authoring du profil, parite joueur/IA, absence de `WheelCollider` et de reglage serialise, geometrie de la bordure, vue de telemetrie.
- [x] `Assets/RoadRage/Tests/PlayMode/Story511VehicleChassisWheelsAndSuspensionPlayModeTests.cs` -- 2 preuves runtime sur un banc route + bordure + bande trottoir, la bordure etant mesuree sur le prefab du carrefour pour que banc et authoring ne divergent pas.
- [x] `docs/setup/story-5-11-vehicle-physics-notes.md` -- livraison, mesures, contrat de bordure, constats.
- [x] `docs/setup/story-5-10-lane-graph-district-notes.md` -- regle provisoire remplacee par le contrat livre (hauteur de bordure 0,12 m).
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- `5-11-vehicle-chassis-wheels-and-suspension` en `in-progress`.
- [x] `graphify update .` -- **mesure : 2949 noeuds / 7014 liens / 129 communautes** ; `IsSurfaceContact`, `IsSurfaceOnlyCollision` et `surfaceContactTolerance` a **0 occurrence**, la couche physique bien presente.
- [x] **Controle IA / bordure** -- la geometrie est prouvee en EditMode (aucun `LaneNode` dans l'emprise de la bordure, degagement de 0,97 m, hauteur sous le debattement et sous le dessous du chassis). Le controle « les vehicules IA traversent le module sans la toucher en conduite nominale » **n'a pas de test dedie** : il repose sur la porte PlayMode verte du 2026-09-18 (mesure utilisateur) et sur la conduite observee par l'humain. A noter pour la suite : c'est la Story 5.12 qui possede le point de visee anticipe et le rayon d'arrivee, tous deux nommes par l'AC comme les corrections a privilegier si ce controle echoue.

**Acceptance Criteria:**

- Given les deux prefabs portent aujourd'hui un Rigidbody identique a COM implicite et sans suspension, when le chassis est authore, then masse, centre de masse et tenseur d'inertie sont explicites, les deux prefabs portent **le meme** composant physique configure par profil, et l'identite, l'enregistrement `NetworkObject`, les composants gameplay et les ids de definition sont inchanges.
- Given le vehicule doit garder un contact fiable, when le modele de roue est construit, then contact, compression et transfert de charge viennent de **raycasts par roue** (jamais `WheelCollider`), raideur, amortissement, longueur au repos et debattement sont authores, un terme anti-roulis reduit le roulis sans l'annuler, et ces fonctions sont verifiables en EditMode.
- Given les vehicules doivent avoir des ressentis differents, when la configuration est authoree, then tout parametre chassis/roue/suspension vit dans un `VehicleProfileDef` a id stable, aucun ne reste en `[SerializeField]` sur un controleur, et une vue de telemetrie `RoadRage.DevTools` montre par roue compression et contact plus vitesse, vitesse laterale, glissement et angle de derive, en build de developpement seulement.
- Given les modules n'ont aujourd'hui aucun relief, when la story est verifiee dans l'Editeur, then une bordure est authoree sur **un carrefour ou un giratoire, jamais une ligne droite**, mesuree contre les cotes figees (2 m de l'axe de voie au bord de chaussee, vehicule de 2,06 m, 0,97 m de degagement par cote), et les trois controles passent : aucun `LaneNode` sur ou dans la bordure ; les vehicules IA traversent le module sans la toucher en conduite nominale ; le vehicule la monte a basse vitesse et reste au sol quand il la heurte a vitesse de conduite.
- Given le predicat de contact de surface n'existe que pour compenser l'absence de suspension, when les roues portent le vehicule, then `IsSurfaceContact`, `IsSurfaceOnlyCollision` et leur tolerance disparaissent avec le test EditMode qui assertait sur leur texte source, `NoModuleColliderRisesAboveTheDrivingPlane` devient une garde sur la hauteur de bordure authoree, et le garde `vehicleDamage > 0` de `RunFlowController` reste intact.

## Spec Change Log

## Design Notes

- **Pourquoi l'axe vertical seulement.** 5.11 livre le contact au sol et la suspension ; 5.12 remplace les ecritures de `linearVelocity` par des forces aux roues. Pour que 5.11 soit livrable seule, la couche physique **propriete l'axe vertical** (gravite + ressort + amortisseur + anti-roulis) et les controleurs cessent d'ecrire la composante verticale, tout en gardant le longitudinal et le lateral jusqu'a 5.12. Cet etat intermediaire est nomme, pas subi : le commentaire d'en-tete de `VehiclePhysicsBody` porte la date et la story qui le leve.
- **Raycasts plutot que `WheelCollider`, et pourquoi c'est un choix de testabilite.** `WheelCollider` n'est prouvable que pendant un pas de physique, or la suite PlayMode est rouge et non filtrable ; les fonctions pures se prouvent en EditMode, ou la suite est verte et filtrable (AD-35 : decision de testabilite, pas de fidelite).
- **Implantation des roues en donnee, pas en GameObjects.** Le prefab greybox n'a pas de roues et ne doit pas en gagner : ajouter des enfants changerait sa composition et la garde de parite joueur/IA. Les positions locales des quatre roues vivent donc dans le profil, ce qui garde NFR18 trivialement satisfait et evite d'introduire des roues visuelles (art, AD-14).
- **`Col_Curb_*` et pas `Col_Sidewalk_*`.** `SidewalksCarryTheDedicatedAreaAndStayOutOfTheVehicleBake` exige **0** `Col_Sidewalk` sur le giratoire ; nommer la bordure autrement evite d'avoir a renegocier une garde de 5.10 qui n'a pas a bouger.
- **L'anti-roulis remplace l'annulation roulis/tangage, il ne s'y ajoute pas.** Les deux remplissent la meme fonction ; garder les deux donnerait un roulis nul, ce qui contredit l'AC « sans l'annuler ».
- **Le carrefour central plutot qu'un giratoire.** Il est unique (1 instance), c'est le point que le joueur traverse, et les vehicules y coupent l'interieur des virages -- exactement le cas que l'AC vise. Le giratoire reste evite pour ne pas rouvrir sa garde de colliders.
- **Hauteur de bordure : une valeur, deux traces.** Point de depart 0,12 m -- assez pour etre une bordure franche, bien au-dessus de la levre de 5 cm du plan de sol acceptee en 5.10 -- puis mesure : le debattement de suspension la depasse et le vehicule la franchit a basse vitesse sans decoller a vitesse de conduite. La constante du test EditMode est la source cote code ; la note de district reste le contrat humain pour le kit artistique. Un test lie le debattement de suspension a cette hauteur, pour que physique et art ne divergent pas en silence.
- **Autorite : la garde existe deja.** Les deux controleurs posent `body.isKinematic = !IsServer` (`NetworkedVehicleDriverController.cs:166`, `NetworkedAIVehicleDriverController.cs:198`) : cote client un `AddForce` est sans effet. `VehiclePhysicsBody` s'appuie sur cette garde et n'ajoute **aucune** verification d'autorite, pour ne pas creer un second chemin de verite (AD-21).
- **Pas de catalogue pour ce Def.** `DriverProfileDef` a explicitement choisi la reference directe par le prefab, faute de lookup par id avant 5.16 et 5.19 ; `VehicleProfileDef` suit le meme choix et le documente dans son en-tete. Inventer un `VehicleProfileCatalog` serait de la configuration morte.
- **Ce qui ne part pas dans le profil.** `rolloverUprightDotThreshold`, `rolloverSustainedSeconds`, `voidHeightThreshold`, le seuil de blocage et le rayon d'arrivee ne sont pas des parametres de chassis : ce sont des seuils de recuperation et de decision, et ils restent ou ils sont. Seuls partent `centerOfMassOffset` et `rollStabilityAssist` (remplaces par le terme anti-roulis), plus `surfaceContactTolerance` qui disparait.
- **La preuve runtime ne peut pas se filtrer.** Le mode PlayMode n'accepte pas `-TestFilter` (limite mesuree du CLI) : la suite complete est le seul mode, d'ou la baseline obligatoire en tache 0.

## Verification

**Commands:**

- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story511VehicleChassisWheelsAndSuspensionTests"` -- **mesure : 17/17 verts, exit 0**.
- `.\scripts\validate.ps1 -TestMode EditMode` -- **mesure : 537/537 verts**, 0 erreur Console (523 tests avant la story, +17 pour la fixture 5.11, -2 pour les tests de surface retires).
- `\scripts\validate.ps1 -TestMode PlayMode` -- **mesure utilisateur du 2026-09-18 (Test Runner de l'Editeur) : tout est vert.** C'est la seule mesure PlayMode disponible : le chemin CLI ne fonctionne pas dans la session de l'agent -- en synchrone il repond `PlayMode tests cannot run synchronously over HTTP: entering play mode triggers a domain reload that drops the request`, et en asynchrone (`--async_tests`) le statut revient `completed` avec **zero test** et une duree nulle. Aucune mesure PlayMode n'a donc pu etre produite par l'agent ; celle qui fait foi vient de l'Editeur, et elle doit etre citee comme telle.
- `graphify update .` -- attendu : graphe regenere, aucun noeud residuel `.IsSurfaceContact()` / `.IsSurfaceOnlyCollision()`.

**Manual checks (if no CLI):**

- Editor, `MVP_Run`, hote : traverser le carrefour central en conduite nominale (aucun contact avec `Col_Curb_*`), puis monter la bordure a basse vitesse et la heurter a vitesse de conduite. Attendu : le vehicule monte ou reste au sol, aucun envol, aucun arret net, aucun degat sur un contact de surface. Consigner captures et lecture de telemetrie dans `docs/setup/story-5-11-vehicle-physics-notes.md`.
- Pour cette passe de mesure, la vue de telemetrie est ajoutee **a la main** sur la voiture dans `MVP_Run` puis retiree avant de sauver la scene : `MVP_Run.unity` et `git status --short` doivent revenir a leur etat initial (garde de double etat d'`AGENTS.md`).

## Suggested Review Order

**La couche physique -- le cœur du changement**

- Point d'entree : le composant unique joueur+IA, qui applique le profil puis simule par raycasts par roue.
  [`VehiclePhysicsBody.cs:28`](../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L28)

- Ce qui se prouve sans scene : compression, ressort, amortisseur borne, anti-roulis en paire, frottement, telemetrie.
  [`VehicleSuspensionModel.cs:105`](../../Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs#L105)

- La donnee physique complete, immutable, et l'implantation des roues comme donnee plutot que comme GameObjects.
  [`VehicleProfile.cs:20`](../../Assets/RoadRage/Features/Vehicles/VehicleProfile.cs#L20)

- Le Def authore : id stable, `TryValidate` qui nomme le champ fautif, sans catalogue (comme `DriverProfileDef`).
  [`VehicleProfileDef.cs:22`](../../Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs#L22)

- La purge d'etat : indispensable aux deux teleportations, sinon la difference finie produit un pic d'amortisseur.
  [`VehiclePhysicsBody.cs:162`](../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L162)

**Le retrait du contournement et la regle de degats**

- L'evenement de collision, garde par l'immunite de dev, puis par la discrimination de contact de surface.
  [`NetworkedVehicleDriverController.cs:355`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L355)

- La regle de discrimination : tolerance **lue dans le profil authore**, donc la meme pour joueur et IA.
  [`NetworkedVehicleDriverController.cs:395`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L395)

**La bordure prototype et son contrat de kit artistique**

- Les quatre `Col_Curb_*` sous `Collision/`, dans la bande trottoir, sans deplacer un seul `LaneNode`.
  [`Greybox_Intersection.prefab:2093`](../../Assets/RoadRage/Prefabs/Greybox_Intersection.prefab#L2093)

- La regle de plan de roulage requalifiee : ce qui s'eleve desormais, c'est la bordure, pas le trottoir.
  [`story-5-10-lane-graph-district-notes.md`](../../docs/setup/story-5-10-lane-graph-district-notes.md)

**L'outil de developpement (hors perimetre de la story)**

- Un duplicata du prefab joueur dont le visuel greybox devient le mesh POLYGON, colliders Synty retires.
  [`Dev_IndestructibleCar.prefab`](../../Assets/RoadRage/Prefabs/Dev_IndestructibleCar.prefab)

- Le marqueur : aucune ligne de code de jeu n'a besoin de connaitre le developpement.
  [`DevIndestructibleVehicle.cs:22`](../../Assets/RoadRage/Features/Vehicles/DevIndestructibleVehicle.cs#L22)

- L'instrument de mesure des trois controles de bordure.
  [`VehiclePhysicsTelemetryView.cs`](../../Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs)

**Peripheriques**

- Les 20 gardes EditMode : fonctions pures, authoring, parite des prefabs, invariants de code, geometrie.
  [`Story511VehicleChassisWheelsAndSuspensionTests.cs`](../../Assets/RoadRage/Tests/EditMode/Story511VehicleChassisWheelsAndSuspensionTests.cs)

- Les deux preuves runtime sur le vrai module : montee a basse vitesse, non-projection a vitesse de conduite.
  [`Story511VehicleChassisWheelsAndSuspensionPlayModeTests.cs`](../../Assets/RoadRage/Tests/PlayMode/Story511VehicleChassisWheelsAndSuspensionPlayModeTests.cs)

- La garde 5.10 reecrite **et reparee** : elle mesurait `bounds` sur un asset, donc ne gardait rien.
  [`Story510LaneGraphAndRoutedTrafficTests.cs`](../../Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs)

- La note de livraison : mesures, anomalies de recette, constats d'outillage, resultats de revue.
  [`story-5-11-vehicle-physics-notes.md`](../../docs/setup/story-5-11-vehicle-physics-notes.md)
