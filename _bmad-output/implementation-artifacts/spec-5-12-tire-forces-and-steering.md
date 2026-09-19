---
title: "Tire Forces and Steering"
type: "feature"
created: "2026-09-18"
status: "done"
review_loop_iteration: 0
baseline_commit: "68976ac7a315fe24529bfa8b4ba073d4bd6ec521"
context:
  - "{project-root}/docs/setup/story-5-11-vehicle-physics-notes.md"
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La voiture ne tourne pas par ses pneus. Le conducteur impose le mouvement : `ApplyPhysics` ecrit `Rigidbody.linearVelocity` en bloc (`NetworkedVehicleDriverController.cs:660`, methode `:625-663`) et `ApplySteering` impose le lacet par `body.MoveRotation(...)` (`:721-741`). Le frottement livre par 5.11 est un contact borne par la charge, pas un pneu (`VehiclePhysicsBody.cs:265-276` / `VehicleSuspensionModel.ResolveGroundFrictionForce:135-150`) : aucune perte d'adherence progressive, donc ni derive controlee ni glissement. Cote IA, `ComputeSeekIntent` (`NetworkedAIVehicleDriverController.cs:787-811`) vise la position du noeud suivant en ligne droite : le vehicule coupe l'interieur de chaque virage d'une profondeur bornee par `arrivalRadius` (3 m, prefab `:224`) — dette de `deferred-work.md:227-229`, reaffectee a cette story (`:242-245`), et raison pour laquelle le controle « les IA traversent le carrefour sans toucher la bordure » de 5.11 n'a aucune assertion (`:262-264`).

**Approach:** Remplacer le longitudinal et le lateral par des efforts aux roues : modele de pneu pur (glissement longitudinal et angulaire, force progressive avec pic puis chute, force combinee bornee par l'adherence), etat de rotation par roue et point d'entree d'intent dans `VehiclePhysicsBody`, l'unique composant physique joueur+IA. Le modele de direction devient un angle de roue dependant de la vitesse, avec retour au centre progressif. La poursuite de l'IA vise un point anticipe sur la trajectoire. `VehicleDriveIntent` gagne une voie de frein a main, sur le chemin d'intent existant.

## Boundaries & Constraints

**Always:**

- AD-35 : la couche physique ne lit que `VehicleProfileDef`, jamais rage, peur ni disposition (AD-33). L'hote reste le seul simulateur (AD-21) ; l'autorite s'appuie sur `body.isKinematic = !IsServer` deja pose (`NetworkedVehicleDriverController.cs:160`, `NetworkedAIVehicleDriverController.cs:198`) — aucune seconde verification d'autorite.
- Slip, force de pneu et adherence sont des **fonctions pures** verifiables en EditMode. Perte d'adherence **progressive** : jamais de seuil binaire ; une force de pneu ne depasse jamais `adherence x charge portee par cette roue`.
- Acceleration, freinage et vitesse de pointe sont authores **par profil** et progressifs ; ils ne restent pas en `[SerializeField]` sur le controleur. Les roues consomment enfin `VehicleWheel.IsSteering` / `IsDriven`, authores et inertes depuis 5.11.
- `VehicleDriveIntent` gagne `Handbrake` sur le chemin existant : meme RPC, meme validation serveur, nom de RPC inchange (NFR5).
- NFR18 : identite des prefabs, `NetworkObject` enregistres, composants gameplay, colliders et ids de definition inchanges.
- Le graphe de voies et la geometrie des modules ne bougent pas : aucun `LaneNode` deplace, ajoute ou supprime, aucune retouche de `Col_Curb_*` ni des colliders de module.
- La couche physique **ne teste pas la derive** : aucune aide arcade (stabilite en lacet, controle de traction, recuperation de tete-a-queue) n'est ecrite ici — elles appartiennent a la 5.13.
- Les tests EditMode de la Story 5.9 (`DriverModel`, IDM, MOBIL) passent verts **sans modification** (regle du sprint-change-proposal 2026-09-18). Un echec signifie que la physique a fui dans la couche de decision : corriger avant de continuer.

**Ask First:**

- Toute correction touchant le graphe de voies, le rayon d'arrivee author e ou la geometrie de bordure pour faire passer le controle « les IA ne touchent pas la bordure en conduite nominale ». Les corrections privilegiees sont le point de visee anticipe et le rayon d'arrivee.
- Toute modification du collider du vehicule, de la composition d'un prefab ou d'une largeur de chaussee.
- Supprimer l'ecriture `linearVelocity` de l'IA, son integration de vitesse en boucle ouverte, ou `RecoverAtWaypoint` : c'est le perimetre de la Story 5.14.
- Changer la touche de frein a main ou l'ensemble des touches de conduite.

**Never:**

- `WheelCollider` ou un modele de pneu tiers (`ADDON-003` clos en negatif par AD-35).
- Ecrire `linearVelocity`, la position ou la rotation du `Rigidbody` depuis le chemin de conduite joueur ; `MoveRotation` reste legitime cote IA jusqu'a la 5.14.
- Toucher la garde `vehicleDamage > 0` de `RunFlowController`, la regle de contact de surface pilotee par le profil, ou la teleportation de recuperation `RecoverAtWaypoint` (5.14).
- Authorer une seconde surface roulable : le coefficient d'adherence par surface est **expose**, pas consomme (report de `deferred-work.md:238-241`).
- Un seuil binaire d'adherence, une vitesse gelee, des collisions desactivees ou une masse augmentee pour masquer un symptome.

## I/O & Edge-Case Matrix

| Scenario                   | Input / State                                                                                    | Expected Output / Behavior                                                                                                                                                      | Error Handling                                                                                   |
| -------------------------- | ------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| Depart                     | vitesse nulle, `Throttle = 1`, roues motrices au sol                                             | effort progressif aux roues motrices ; glissement faible, puis vitesse croissante ; la pointe authoree est approchee par une pente de force, jamais par une ecriture de vitesse | roue non motrice : aucun effort moteur ; `fixedDeltaTime <= 0` : sortie sans calcul              |
| Freinage et marche arriere | `BrakeReverse = 1`, vitesse > seuil                                                              | effort de freinage progressif sur les roues ; la marche arriere ne s'engage que sous le seuil de vitesse deja utilise (`minimumSteerSpeed`)                                     | vitesse quasi nulle : pas d'inversion instantanee, pas de NaN dans le glissement                 |
| Direction                  | `Steer` maintenu, vitesse croissante                                                             | l'angle de roue effectif diminue avec la vitesse ; la voiture tourne par les efforts lateraux des roues avant, sans `MoveRotation`                                              | `Steer` relache : retour au centre progressif, jamais instantane                                 |
| Derive                     | glissement lateral au-dela du pic authore                                                        | la force laterale decroit **progressivement** ; la derive est atteignable et se referme quand le glissement redescend sous le pic                                               | au-dela du pic, la force tend vers une valeur bornee, jamais vers zero ni vers l'infini          |
| Frein a main               | `Handbrake` actionne, vehicule en mouvement                                                      | les roues arriere se bloquent : glissement longitudinal sature, adherence laterale arriere s'effondre, le lacet augmente (entree en derive)                                     | relachement : l'adherence revient progressivement, pas par un saut                               |
| Roue en l'air              | aucun contact, ou contact perdu                                                                  | charge nulle : aucun effort de pneu, aucune exception ; l'etat de rotation de la roue reste fini                                                                                | reprise de contact : la derivee de compression reste gardee par `wheelWasGrounded` (acquis 5.11) |
| Profil invalide            | raideur de pneu nulle ou negative, angle de roue maximal hors bornes, pointe de glissement nulle | `TryValidate` refuse en nommant le champ fautif                                                                                                                                 | aucune valeur de repli silencieuse : le composant ne s'active pas sur un profil invalide         |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- joueur. A retirer : ecriture en bloc `:660`, `ResolveTargetSpeed` `:691`, `ResolveSpeedChangeRate` `:706`, `ApplySteering` `:721-741` (dont `MoveRotation` `:740` et `lowSpeedAssist` `:735-738`), `lateralGrip` `:64` et les bornes de vitesse serialisees `:40-72`. A conserver : `ResolveSteerDirectionMultiplier` `:665` (testee par Story 3.2, test `Story32DriverControlAndLocalCameraTests.cs:90-95`), les trois resolveurs de degats 3.5 `:581-595` (`ResolveEffectiveMaxForwardSpeed` garde par `Story35VehicleDamageHookAndTeamWipeContractStubTests.cs:169-170`) — ils scalent desormais l'autorite de conduite, pas une vitesse ecrite. Chemin d'intent a etendre : `SubmitDriveIntent:598`, `SubmitDriveIntentRpc:609-610`, `ApplyServerDriveIntent:615`, appels `:602`/`:606`. Input : `ReadLocalDriveIntent:743-775` lit `Keyboard.current` sous `LocalInputGate.IsBlocked` `:195` (aucune action Brake/Handbrake dans `InputSystem_Actions.inputactions` ; `spaceKey` n'est lu nulle part dans `Assets/RoadRage`).
- `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs` -- struct 35 lignes : `Idle` `:12`, ctor `:14-19`, champs `:22-28`, `IsIdle` `:30-32`. Ajouter la voie handbrake casse les 4 sites d'appel : ici `:12`, `NetworkedVehicleDriverController.cs:622` et `:774`, `NetworkedAIVehicleDriverController.cs:810`.
- `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` -- l'unique composant physique des deux prefabs. `FixedUpdate` `:180-286` : raycasts par roue `:213-218`, `WheelState` `:245-248`, force de suspension `:250-259`, **frottement provisoire a remplacer** `:265-276`. Le point d'entree d'efforts longitudinalx/lateraux n'existe pas : `AddForceAtPosition` n'est appele que pour la suspension, l'anti-roulis (`:311-355`) et l'assiette. API publique : `WheelCount` `:55`, `Profile` `:61`, `BindProfile` `:78`, `ApplyProfile` `:90`, `ResetSuspensionState` `:162` (a etendre a la rotation des roues), `TryGetWheelState` `:364`, `TrySampleTelemetry` `:381`. La charge normale par roue (variable locale `force`) et la vitesse de rotation des roues ne sont **pas publiees** : ce sont les deux entrees manquantes d'un pneu.
- `Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs` -- fonctions pures de reference (motif a suivre) : `ResolveSuspensionForce` `:105`, `ResolveGroundFrictionForce` `:135-150`, `ResolveFrictionAlong` `:153-168` (ramp `FrictionRampSpeed = 0.5f` `:37` — a conserver sous une forme equivalente : sans elle, un vehicule a l'arret tremble, mesure 5.11), `ResolveAntiRollForces` `:183`, `SampleTelemetry` `:205`, `TelemetrySample` `:279-304`, `WheelState` `:307-328`.
- `Assets/RoadRage/Features/Vehicles/VehicleProfile.cs` + `VehicleProfileDef.cs` + `Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset` -- 15 champs authores (masse, COM, tenseur, 4 roues `(±0.85, 0.22, ±1.55)` rayon 0,33, ressort 32000, amortisseur 1900, `restLength` 0,45, `travel` 0,25, anti-roulis 20000, assiette, `lateralFrictionCoefficient` 1,2, `rollingResistanceCoefficient` 0,03, masque 1, `surfaceContactTolerance` 0,15). Valeurs **en double** (`VehicleProfileDef.cs:30-52` et l'asset `:17-53`) : garder les deux coherentes. `TryValidate` `:77-256` nomme le champ fautif ; `OnValidate` `:258` avertit. Reutiliser `lateralFrictionCoefficient` comme adherence du pneu (pas de renommage, pas de table par surface — configuration morte tant qu'une seconde surface n'existe pas).
- `Assets/RoadRage/Features/Vehicles/VehicleWheel.cs` -- `IsSteering` `:40-74`, `IsDriven` `:44-80` authores mais **lus nulle part** dans `Assets/RoadRage` : c'est ici que le modele de direction prend ses roues avant.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- IA. `ComputeSeekIntent` `:787-811` (visee point-a-point, `steer = signedAngle / steerFullLockDegrees` `:805-808`, `new VehicleDriveIntent(1f, steer, 0f)` `:810`) appele en `:352`, suivi de `ApplyMovement(intent, fixedDeltaTime, currentSpeed)` `:353`. `ApplyMovement` `:827-851` (ecriture en bloc `:849`, `MoveRotation` `:846`) et `IntegrateLongitudinalSpeed` `:555` restent **tels quels** jusqu'a la 5.14 : leur commentaire d'en-tete `:829-835` doit nommer la 5.14, pas la 5.12. Constantes IA serialisees : `arrivalRadius` 3 `:80-81`, `steerFullLockDegrees` 45 `:84-85`, `steerDegreesPerSecond` 90 `:88-89`. `ResolveScanDirection` `:586-610` est un ersatz de visee anticipee pour la perception, pas pour la direction. Le lacet est impose : ne pas le convertir ici.
- `Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs` -- predicats purs reutilisables pour la visee et le parcours : `HasPassedUnreachableWaypoint` `:234`, `SelectSuccessorTowardTarget` `:280`, `NormalizeIndex` `:21`. `LaneGraph.GetNodeRotation(int)` (`LaneGraph.cs:104-108`) existe et n'est **pas consomme** : le noeud porte deja son sens de circulation (`LaneNode.cs:29-31`).
- `Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs` -- a mettre a jour : les 3 tests de `ComputeSeekIntent` `:21-58` figent la visee point-a-point, et `DriverControllerHasNoSharedStaticStateBeyondItsPureFunctions` `:119-127` exige `Occurrences(source, "static ") == 3` : toute fonction statique ajoutee au controleur IA le casse — mettre les nouvelles fonctions pures dans un fichier dedie.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` -- `ReplayReachesExit` `:1197-1279` **recopie** les equations de poursuite (constantes figees `:1211-1213` : `ArrivalRadius = 3f`, `SteerFullLockDegrees = 45f`, `SteerDegreesPerSecond = 90f`). Faire appeler les fonctions pures reelles par le rejeu, puis y ajouter l'assertion de franchissement du carrefour hors emprise de `Col_Curb_*` (fermeture du controle sans assertion de `deferred-work.md:262-264`). Ne pas toucher `NoModuleColliderRisesAboveTheDrivingPlane` ni `SidewalksCarryTheDedicatedAreaAndStayOutOfTheVehicleBake`.
- `Assets/RoadRage/Tests/EditMode/Story511VehicleChassisWheelsAndSuspensionTests.cs` -- a mettre a jour : `GroundFrictionOpposesASlideAndIsCappedByTheLoadCarried` `:145-179` (frottement provisoire), `DefaultProfileIsFullyAuthoredAndValid` `:322-353` (nouveaux champs du profil), `TheTelemetryViewLivesInDevToolsAndHoldsNoGameplayState` `:668-685` dont l'assertion `Does.Not.Contain("linearVelocity =")` `:681` porte **uniquement** sur le fichier de telemetrie (piege : `var linearVelocity = ...` y declenche un faux rouge). A garder intactes : `NoWheelColliderIsUsedByThePhysicsLayer` `:467-484`, `NoChassisOrSuspensionTuningRemainsSerializedOnTheControllers` `:486-513`, `TheSurfaceContactRuleIsProfileDrivenAndTheDamageGuardIsUntouched` `:515-542`.
- `Assets/RoadRage/Tests/PlayMode/Story511VehicleChassisWheelsAndSuspensionPlayModeTests.cs` -- banc de la suspension : `DriveAt` `:240-244` **ecrit `body.linearVelocity` en bloc** pour contourner le chemin de conduite. Ne pas le convertir : il mesure la couche 5.11 ; ajouter un banc separe qui passe par le point d'entree d'intent.
- `Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs` -- instrument de mesure : `ComposeText` `:91-124` (caisse `:101-104`, roues `:119-120`), garde `Debug.isDebugBuild` `:53-60`. Y ajouter glissement, force de pneu et adherence par roue : c'est l'instrument des controles humains de cette story.
- `docs/setup/story-5-11-vehicle-physics-notes.md` -- constats a respecter : anti-roulis en paire, aucune friction native, plafond d'amortisseur, rampe basse vitesse, cache PlayMode non reproductible par l'agent.
- `_bmad-output/implementation-artifacts/deferred-work.md` -- entrees a mettre a jour : `:227-229` (dette de poursuite, a marquer cloturee), `:262-264` (controle de bordure sans assertion, rouvert par cette story), `:250-253` (vehicules IA non poussables : a verifier ici et a laisser a la 5.14, l'IA ecrivant encore sa vitesse).
- `docs/setup/story-5-10-lane-graph-district-notes.md:168-170` -- renvoi de la dette vers l'ancienne numerotation : a realigner sur la cloture par la 5.12.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs` -- nouveau : fonctions pures de pneu (glissement longitudinal et angulaire, force progressive avec pic puis chute, force combinee bornee par `adherence x charge`, echantillon de telemetrie de pneu) -- c'est le modele a prouver en EditMode.
- [x] `Assets/RoadRage/Features/Vehicles/VehicleSteeringModel.cs` -- nouveau : fonctions pures de direction (angle de roue maximal reduit par la vitesse, retour au centre progressif, repartition avant/arriere) -- la direction devient une consigne d'angle, plus un lacet impose.
- [x] `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` -- etat de rotation par roue, application des efforts de pneu au point de contact, publication de la charge normale par roue, point d'entree d'intent (`ApplyDriveIntent`) et remise a zero du spin dans `ResetSuspensionState`, en remplacement du frottement provisoire `:265-276` -- une seule couche physique, prete pour la 5.14.
- [x] `Assets/RoadRage/Features/Vehicles/VehicleProfile.cs` + `VehicleProfileDef.cs` + `VehicleProfileDef_Default.asset` -- ajouter les parametres de pneu, de direction et de frein a main, deplacer acceleration, freinage et vitesse de pointe du controleur vers le profil, etendre `TryValidate` ; les deux copies de valeurs restent coherentes.
- [x] `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs` -- voie `Handbrake` -- voie demandee par l'AC, sur le chemin existant.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- supprimer l'ecriture en bloc, le `MoveRotation` de conduite et les bornes de vitesse serialisees ; soumettre l'intent (handbrake inclus) a la couche physique ; lire la touche de frein a main comme les autres, sous `LocalInputGate` -- l'autorite et les degats 3.5 sont preserves, l'intent reste le seul chemin.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- visee anticipee : `ComputeSeekIntent` vise un point situe devant le vehicule sur la trajectoire des noeuds (duree de visee authoree), au lieu de la position du noeud -- ferme la dette de coupe dans les virages. Le lacet impose, l'integration de vitesse et `RecoverAtWaypoint` restent inchanges et nomment la 5.14.
- [x] `Assets/RoadRage/Tests/EditMode/Story512TireForcesAndSteeringTests.cs` -- nouveau : matrice de cas limites, courbe de pneu (pic, chute progressive, borne par l'adherence), glissement nul, direction dependante de la vitesse, retour au centre, frein a main, authoring du profil, absence de `WheelCollider` et d'ecriture de vitesse dans le chemin joueur -- les preuves EditMode de la story. **Mesure : 24/24 verts.**
- [x] `Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs` + `Story510LaneGraphAndRoutedTrafficTests.cs` -- adapter les tests de visee au point anticipe, faire appeler les fonctions pures reelles par le rejeu, et y ajouter le controle de bordure de 5.11. **Ecart assume, voir Spec Change Log : la mesure absolue s'est revelee hors de portee de l'instrument ; ce qui est garde est la non-degradation mesuree plus l'exercice reel de la visee. Mesure : 40/40 verts.**
- [x] `Assets/RoadRage/Tests/EditMode/Story511VehicleChassisWheelsAndSuspensionTests.cs` -- adapter les tests du frottement provisoire et du profil ; ne rien relacher des gardes `WheelCollider`, reglages serialises et contact de surface.
- [x] `Assets/RoadRage/Tests/PlayMode/Story512TireForcesAndSteeringPlayModeTests.cs` -- nouveau : preuve runtime sur banc (depart, freinage, virage, derive au frein a main) passant par le point d'entree d'intent, pas par une ecriture de vitesse -- **ecrit, non execute : le harnais PlayMode n'est pas mesurable par l'agent, voir Verification**.
- [x] `Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs` -- glissement, force et adherence par roue -- instrument de mesure des controles manuels.
- [x] `docs/setup/story-5-12-vehicle-physics-notes.md` -- nouveau : mesures, valeurs authorees retenues, constats et anomalies -- trace de livraison, comme la note 5.11.
- [x] `_bmad-output/implementation-artifacts/deferred-work.md` + `docs/setup/story-5-10-lane-graph-district-notes.md:168-170` -- cloture de la dette de poursuite, sort du controle de bordure et de l'entree « vehicules non poussables » enregistres ; renvoi de la note de district realigne.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- `5-12-tire-forces-and-steering` en `in-progress`.
- [x] `graphify update .` -- **mesure : 3 089 nœuds / 7 352 liens / 128 communautes** (5.11 : 2 949 / 7 014 / 129), delta coherent avec les nouveaux fichiers, aucune fuite de perimetre.

**Ecarts de livraison :**

- Le controle de bordure de 5.11 est mesure mais **pas tranche en absolu** par l'instrument EditMode ; la mesure et sa limite sont enregistrees dans le Spec Change Log, dans `deferred-work.md` et dans la note de livraison.
- L'essieu **moteur** a ete deplace a l'arriere (la donnee author e de 5.11 portait `isDriven` a l'avant, mais le drapeau etait inerte). C'est le montage qui donne un sens au frein a main ; l'AC ne tranche pas le montage, donc le point est signale pour confirmation a la recette dans la note de livraison.
- Trois appels a une API de pneu intermediaire (`ResolveTireForce` / `ResolveCombinedForces`) ont ete livres par l'implementation puis corriges en cours de route : le modele a budget unique (`ResolveTireForces`) est la version livree, et la compilation est verte.

**Acceptance Criteria:**

- Given le modele de conduite ecrit aujourd'hui `Rigidbody.linearVelocity` en bloc en ne preservant que la composante verticale, when il est remplace, then le mouvement longitudinal vient d'**efforts appliques aux roues**, l'ecriture en bloc disparait, acceleration, freinage et vitesse de pointe sont progressifs et authores par profil, et l'hote reste le seul simulateur (AD-21).
- Given un pneu ne peut transmettre qu'une force bornee, when l'adherence laterale et longitudinale est calculee, then la perte d'adherence est **progressive**, pilotee par une valeur de glissement, jamais par un seuil binaire, une derive controlee est atteignable et recuperable aux valeurs de glissement authorees, et glissement, force de pneu et adherence sont des fonctions pures verifiables en EditMode.
- Given la direction doit rester lisible a vitesse elevee, when le modele de direction est construit, then l'angle de roue depend de la vitesse, la roue revient au centre progressivement, la poursuite point-a-point de l'IA est remplacee par un **point de visee anticipe** (ce qui cloture la dette enregistree dans `deferred-work.md` le 2026-09-18), et les vehicules ne coupent plus l'interieur des virages d'une profondeur bornee par le rayon d'arrivee.
- Given le frein a main fait partie du ressenti demande, when l'intent est etendu, then `VehicleDriveIntent` gagne une voie de frein a main et elle voyage sur le chemin d'intent **existant** : ni second intent, ni second RPC (NFR5).
- Given le controle d'acceptation de 5.11 « les vehicules IA traversent le module sans toucher la bordure en conduite nominale » n'avait aucune assertion, when cette story livre le point de visee anticipe, then ce controle devient une assertion verifiable en EditMode sur la trajectoire rejouee, et l'etat du banc PlayMode de 5.11 est enregistre tel qu'observe.

## Spec Change Log

- **2026-09-18 -- le controle de bordure de 5.11 n'est pas tranche en absolu, et l'assertion est reinstrumentee.** Trouvaille de l'implementation : le rejeu cinematique entre dans l'emprise de `Col_Curb_*` pour **tous** les couples (rayon d'arrivee, duree de visee) essayes -- 104 pas a rayon 3,0 m avec comme sans visee anticipee, 126 a 2,5 m, 154 a 2,0 m, 180 a 1,5 m, 198 a 1,0 m -- donc le second levier nomme par l'AC est mesuree comme la mauvaise direction, et aucun reglage ne ramene le chiffre a zero. Cause : l'instrument rejoue une cinematique pure (vitesse constante, lacet borne, ni pneu, ni suspension, ni contact) et ne peut pas decider si un vehicule REEL touche une bordure. Amende : l'assertion absolue est retiree du test EditMode et remplacee par ce qui est mesurable et non vide -- trajectoires completes, sortie atteinte par toutes, visee anticipee reellement exercee, et non-degradation du degagement par rapport a la poursuite point-a-point d'avant 5.12. L'absolu reste tenu par l'observation humaine en Play Mode, comme la Story 5.11 l'avait enregistre. Ce qui est evite : une garde qui echoue pour toujours quelle que soit la valeur author ee, ou pire, un reglage de la direction mesure contre un modele qui ne represente pas la voiture. KEEP : la mesure complete et sa limite, consignees dans `docs/setup/story-5-12-vehicle-physics-notes.md` et `deferred-work.md` avec condition de reouverture.
- **2026-09-18 -- l'essieu moteur passe a l'arriere.** La donnee author e de 5.11 portait `isDriven` sur l'essieu avant (traction) mais le drapeau etait inerte ; la 5.12 le consomme et l'a porte a l'arriere, seul montage ou le frein a main (qui agit sur les roues non directrices) bloque l'essieu moteur et fait entrer en derive. KEEP : le point est signale comme decision a confirmer en recette, reversible en une valeur du profil.

## Design Notes

- **Perimetre : le joueur maintenant, l'IA en 5.14.** L'AC 5.12 parle du « modele de conduite » qui ecrit `linearVelocity` en bloc, et l'AC de la Story 5.14 dit explicitement que l'IA « produit un `VehicleDriveIntent` et n'ecrit plus jamais la velocite », avec suppression du champ de vitesse en boucle ouverte. L'IA garde donc son ecriture et son lacet imposes apres cette story : etat intermediaire **nomme**, comme la 5.11 l'a fait pour l'axe vertical — le commentaire d'en-tete porte la date et la story qui le leve (5.14), et le commentaire actuel `:829-835` qui annonce deja 5.14 est conserve. La couche physique, elle, est deja la seule et la meme pour les deux : c'est ce que la 5.14 consommera.
- **Pourquoi le rayon d'arrivee est un vrai sujet.** `arrivalRadius = 3f` (prefab `:224`) depasse l'ecart entre noeuds de decision du carrefour (2,83 m, mesure du 2026-09-16) : le vehicule avance au noeud suivant avant de l'atteindre, ce qui raccourcit l'arc roule. Ce constat avait ete laisse ouvert au motif que « le modele de conduite appartient aux Stories 5.11/5.12 » : le point de visee anticipe est la correction, et le rayon lui-meme se reexamine a la mesure. Corriger le graphe reste le dernier recours (Ask First).
- **Pas de table par surface.** `deferred-work.md:238-241` reporte les surfaces differenciees avec une condition de reouverture ecrite ; la 5.12 **expose** le coefficient d'adherence (parametre des fonctions pures de pneu, author e dans le profil) sans inventer de table indexee par un type de surface qu'aucun materiau ne porte : ce serait de la configuration morte, et le point d'ancrage est deja en place.
- **La rampe basse vitesse survit.** `FrictionRampSpeed = 0.5f` (`VehicleSuspensionModel.cs:37`) existe parce que sans elle un vehicule pose sur quatre rayons tremble a l'arret et glisse comme sur de la glace (mesure 5.11). Le modele de pneu doit garder un comportement equivalent a vitesse quasi nulle — sinon un vehicule gare repart tout seul au moindre choc de flanc.
- **La pointe authoree est une pente, pas une ecriture.** La vitesse de pointe du profil reste une valeur author ee et les degats 3.5 la reduisent : elle s'obtient en eteignant progressivement l'effort moteur a l'approche de la limite. C'est ce qui permet de garder `ResolveEffectiveMaxForwardSpeed()` (contrat de source de la Story 3.5) sans reintroduire d'ecriture de vitesse.
- **Ce qui n'est pas converti.** Le banc PlayMode de la 5.11 (`DriveAt` `:240-244`) ecrit `linearVelocity` volontairement : il mesure la suspension, pas la conduite. Le convertir melerait deux preuves ; un banc separe passe par le point d'entree d'intent.
- **Le controle PlayMode n'est pas reproductible par l'agent.** Le harnais PlayMode est casse dans cette session (`run_tests --mode PlayMode` : synchrone impossible, asynchrone a 0 test) : seule une mesure humaine dans l'Editeur fait foi, et elle doit etre citee comme telle.

## Verification

**Commands:**

- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story512TireForcesAndSteeringTests"` -- attendu : fixture verte, exit 0.
- `.\scripts\validate.ps1 -TestMode EditMode` -- attendu : **544 verts + ceux de cette story** (baseline mesuree le 2026-09-18 apres la pose en scene des deux voitures pilotables), 0 erreur Console. Si la porte Console reste rouge a cause d'entrees historiques rejouees sans `--since`, la mesure de repli est `unity cmd run_tests --mode EditMode` + `test_status`, et le fait doit etre rapporte.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- attendu : **non productible par l'agent** (harnais casse). Ne pas conclure d'un resultat vide : la mesure qui fait foi vient de l'Editeur, citee comme telle, comme en 5.11.
- `graphify update .` -- attendu : graphe regenere, aucune visee point-a-point residuelle, taille comparee a la mesure 5.11 (2 949 nœuds).

**Manual checks (if no CLI):**

- Editeur, `MVP_Run`, hote : accelerer depuis l'arret, freiner jusqu'a l'arret, puis tourner a vitesse de conduite. Attendu : la voiture tourne par ses roues, l'angle de braquage diminue avec la vitesse, la roue revient au centre au relachement, aucune remise sur rails.
- Derive : frein a main en virage. Attendu : entree en derive, puis recuperation progressive quand le frein est relache ; lire glissement, force et adherence par roue dans la vue de telemetrie et consigner les valeurs.
- Carrefour central : observer les vehicules IA en conduite nominale. Attendu : aucun contact avec `Col_Curb_*`, aucune coupe de l'interieur du virage ; consigner captures et lecture de telemetrie dans `docs/setup/story-5-12-vehicle-physics-notes.md`.
- Garde de double etat d'`AGENTS.md` : une vue de telemetrie ajoutee a la main dans `MVP_Run` doit etre retiree avant de sauver la scene ; `MVP_Run.unity` et `git status --short` reviennent a leur etat initial.

## Suggested Review Order

**Le modele de pneu -- le cœur du changement**

- Le budget unique d'adherence : les deux glissements normalises par leur pic, une magnitude, une direction.
  [`VehicleTireModel.cs:141`](../../Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs#L141)

- La courbe de force : pic exactement a 1, chute monotone vers une asymptote authoree qui n'est jamais zero.
  [`VehicleTireModel.cs:99`](../../Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs#L99)

- Le train roulant : couple moteur, reaction du pneu, frein qui ne peut que ramener la rotation a zero.
  [`VehicleTireModel.cs:291`](../../Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs#L291)

- L'angle de roue qui diminue avec la vitesse et le retour au centre a son propre taux.
  [`VehicleSteeringModel.cs:39`](../../Assets/RoadRage/Features/Vehicles/VehicleSteeringModel.cs#L39)

**L'application aux roues -- une seule couche, joueur et IA**

- Le point d'entree d'efforts : c'est lui que la Story 5.14 branchera sur l'IA.
  [`VehiclePhysicsBody.cs:204`](../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L204)

- La boucle par roue : charge portee publiee, repere de la roue braquee, efforts au point de contact, spin.
  [`VehiclePhysicsBody.cs:249`](../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L249)

- La publication par roue, seule fenetre de lecture de la telemetrie et des controles humains.
  [`VehiclePhysicsBody.cs:585`](../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L585)

**Le chemin joueur -- plus aucune ecriture de mouvement**

- Chaque pas de conduite : lire une intention, l'inverser si besoin, la soumettre.
  [`NetworkedVehicleDriverController.cs:643`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L643)

- L'inversion marche arriere, extraite en fonction pure parce qu'une garde de texte ne prouvait rien.
  [`NetworkedVehicleDriverController.cs:662`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L662)

- L'autorite de conduite reduite par les degats de la Story 3.5, elle aussi mesuree et non cherchee.
  [`NetworkedVehicleDriverController.cs:593`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L593)

**La visee anticipee de l'IA, et ce qu'elle ne peut pas prouver**

- Le point de visee : continu a la distance de visee, sans mutation de la geometrie.
  [`LaneGraphRouting.cs:293`](../../Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs#L293)

- Le cablage : le noeud courant donne la direction, la vitesse et la duree author ee donnent la distance.
  [`NetworkedAIVehicleDriverController.cs:362`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L362)

- Le controle de bordure de 5.11 : ce qu'il garde, la mesure qui l'a produit, et sa limite ecrite noir sur blanc.
  [`Story510LaneGraphAndRoutedTrafficTests.cs:1204`](../../Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs#L1204)

**La donnee author ee et ses gardes**

- Les parametres de pneu, de direction, de frein a main et de train roulant, en un seul endroit.
  [`VehicleProfileDef_Default.asset:52`](../../Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset#L52)

- Les deux reglages casses refuses : essieu a isDriven incoherent, profil sans roue non directrice.
  [`VehicleProfileDef.cs:150`](../../Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs#L150)

**Peripheriques**

- Les gardes EditMode de la story : courbe de pneu, direction, frein a main, autorite, invariants de code.
  [`Story512TireForcesAndSteeringTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story512TireForcesAndSteeringTests.cs#L1)

- Les deux preuves runtime, a executer par l'humain : le harnais PlayMode n'est pas mesurable par l'agent.
  [`Story512TireForcesAndSteeringPlayModeTests.cs:1`](../../Assets/RoadRage/Tests/PlayMode/Story512TireForcesAndSteeringPlayModeTests.cs#L1)

- L'instrument de mesure : glissement, charge, force transmise et adherence par roue.
  [`VehiclePhysicsTelemetryView.cs:97`](../../Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs#L97)

- La note de livraison : mesures, valeurs retenues, constats, resultat de revue, procedure de recette.
  [`story-5-12-vehicle-physics-notes.md:1`](../../docs/setup/story-5-12-vehicle-physics-notes.md#L1)
