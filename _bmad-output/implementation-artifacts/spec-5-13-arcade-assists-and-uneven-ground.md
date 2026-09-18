---
title: "Arcade Assists and Uneven Ground"
type: "feature"
created: "2026-09-18"
status: "done"
review_loop_iteration: 0
baseline_commit: "8f3022e30fd941c2fe5f8f4ec78511d3724a6cb5"
context:
  - "{project-root}/docs/setup/story-5-11-vehicle-physics-notes.md"
  - "{project-root}/docs/setup/story-5-12-vehicle-physics-notes.md"
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La couche physique traverse aujourd'hui un sol irrégulier sans aucune aide et sans terme borné : l'anti-roulis est la seule force de la couche sans plafond (`VehicleSuspensionModel.ResolveAntiRollForces`), et il est proportionnel à l'écart de compression d'un essieu — exactement ce que produit une bordure franchie par une seule roue. Le seul terme d'assistance existant, `ApplyAttitudeAssist`, **retourne immédiatement dès que `groundedWheels <= 0`** : en vol, plus rien ne stabilise l'attitude. Aucune des trois aides arcade demandées n'existe, et l'autorité de conduite ne dépend pas du nombre de roues au sol. Le district, lui, **ne porte aucun relief longitudinal** : la seule dénivellation authoree est la bordure de 0,12 m du carrefour central, franchie latéralement. Sans relief sur une route rectiligne, le franchissement d'une bosse ou d'une marche n'est observable nulle part en Play Mode.

**Approach:** Ajouter au profil authoré les termes des trois aides (stabilité en lacet, contrôle de traction, récupération de tête-à-queue) et le facteur d'autorité par roues au sol, les porter par des fonctions pures dans un fichier neuf de la couche, publier le nombre de roues au sol, borner le terme non plafonné, rendre continu le point de visée de l'IA au passage de nœud, et authorer dans `MVP_Run` un **relief de recette** sur une avenue — un dos-d'âne et une marche basse — pour que ce franchissement soit réellement observable à la main.

## Boundaries & Constraints

**Always:**

- AD-35 / AD-33 : la couche physique ne lit que `VehicleProfileDef` — jamais rage, peur, disposition, ni aléa. Les fonctions pures n'ont aucune dépendance à `Time`, à un `Rigidbody` ou à une scène.
- Chaque aide est **désactivable par sa propre valeur authoree** (valeur nulle = terme inerte), sur le patron déjà en place `rate <= 0 → Vector3.zero` (`VehicleSuspensionModel.cs:188-192`). Aucune aide ne remet l'entrée conducteur à zéro : la neutralisation reste le fait de l'entrée.
- Le réglage du sol irrégulier se fait contre **0,12 m**, la hauteur de bordure réellement authoree (`Greybox_Intersection.prefab:2053-2724`, sommet `y = 0,12` ; `story-5-11-vehicle-physics-notes.md:24`). Jamais contre une valeur supposée.
- Le **relief de recette** authoré dans `MVP_Run` — dos-d'âne et marche basse sur une avenue — tient dans **0,12 m** au-dessus du plan de roulage, **ne déplace aucun `LaneNode`**, et n'est ajouté à **aucun prefab de module** : c'est un objet de scène, instrument de la recette, pas un élément du district. Les 0,15 m de marche de la première version des modules avaient été retirés pour cause de projection : ne pas remonter au-dessus de 0,12 m.
- **Aucun terme de la couche ne dépasse le budget de charge porté.** Un terme non borné est un défaut, pas un réglage.
- Les tests EditMode de la Story 5.9 restent verts **sans modification** (règle d'epic) : un échec signale une fuite de la physique dans la couche de décision, à corriger avant de continuer.
- NFR18 : ids de prefabs, `NetworkObject` enregistrés, composants gameplay, colliders et ids de définition inchangés.

**Ask First:**

- Toute retouche de la géométrie d'un **prefab de module**, de la hauteur de bordure ou du graphe de voies. Le relief de recette authoré dans `MVP_Run` par cette story est **le seul ajout de géométrie autorisé**, et il reste un objet de scène.
- Toute modification du **sens**, de la **paire** ou du **contrat** de l'anti-roulis et de ses tests 5.11. Le borner est attendu ; l'inverser ne l'est pas — le sens a déjà été livré à l'envers une fois.
- Toute modification d'une fixture 5.9 ou 5.2. Écart accepté le 2026-09-18 pour la **seule** continuité du point de visée (cf. Design Notes).
- Tout `WheelCollider`, toute surface roulable supplémentaire, toute nouvelle valeur de profil non listée dans les tâches.

**Never:**

- Masquer un symptôme par une vitesse gelée, des collisions désactivées, une masse augmentée ou un seuil binaire d'adhérence.
- Écrire `linearVelocity`, la position ou la rotation du `Rigidbody` depuis un chemin de conduite. Le seul `linearVelocity =` restant du contrôleur joueur est la récupération 3.4 (`NetworkedVehicleDriverController.cs:295`).
- Supprimer l'écriture de vitesse ou le `MoveRotation` de l'IA, `RecoverAtWaypoint`, ou l'intégration de vitesse en boucle ouverte : périmètre de la Story 5.14.
- Nommer quoi que ce soit `rollStabilityAssist` dans un contrôleur (`Story511…Tests.cs:549-560` l'interdit, commentaires compris).
- Ajouter une méthode `static` au contrôleur IA (`Story52…Tests.cs:138` en compte exactement 3).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
| --- | --- | --- | --- |
| Adhérence saturée | `Throttle = 1`, glissement longitudinal au-delà du pic authore | le terme de contrôle de traction atténue le couple moteur de cette roue, sans l'annuler ; l'atténuation est nulle quand le glissement est sous le pic | valeur authoree nulle : terme inerte, comportement d'avant cette story |
| Virage rapide, lacet libre | vitesse de conduite, `Steer` maintenu | le terme de stabilité en lacet amortit la composante de lacet **proportionnellement à son authoree**, sans jamais neutraliser le cap demande par le conducteur | terme authore nul : lacet totalement rendu à la géométrie du pneu |
| Tête-à-queue | véhicule en rotation, conducteur braquant à contre | couple de récupération borné qui ramène le cap vers l'entrée conducteur ; il ne remplace jamais l'entrée et ne se déclenche pas sous le seuil de dérive authore | dérive nulle ou entrée nulle : aucun couple |
| Relief authore de 0,12 m | le véhicule franchit la bordure du carrefour latéralement, puis le dos-d'âne et la marche basse de l'avenue longitudinalement, à vitesse de conduite | la roue monte ou la caisse passe ; aucun terme ne dépasse le budget de charge ; le véhicule n'est ni projeté en l'air ni arrêté contre la lèvre | relief sous le seuil authore, ou véhicule à l'arrêt : comportement nominal inchangé |
| Roues en l'air | `groundedWheels = 0` | autorité de conduite et de direction réduite à proportion (donc nulle) ; l'attitude reste stable **sans** que la rotation soit écrite ou figée ; la rotation de roue continue d'être intégrée | tous les termes d'aide restent finis, aucune division par zéro |
| Autorité dégradée | 1 à 3 roues au sol sur 4 | autorité réduite dans la proportion du nombre de roues au sol, multipliée par la réduction de dégâts 3.5 déjà existante | `wheelCount <= 0` : facteur neutre, jamais NaN |
| Profil incomplet | aide authoree non finie ou négative | `TryValidate` refuse en nommant le champ fautif | aucune valeur de repli silencieuse : le composant ne s'active pas |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` -- la couche unique joueur+IA. `FixedUpdate:249-455` ; `groundedWheels` local `:264`, incrémenté `:359-363`, consommé `:453` seulement → à publier. Couple moteur d'un pas `:288-296`, frein `:298-304`, adhérence `:307`. Boucle par roue `:309-448` : raycast vertical unique `:313-319`, `grounded`/`compression` `:321-323`, charge normale `:346-357`, ajout de la force de suspension `:366`, angle de roue par roue `:370-372`, couple/frein par roue `:378-382`, pneu `:388-426`, spin intégré même en l'air `:433-440`, échantillon `:442-448`. Queue de pas `:451-453` : `ApplyAntiRoll` puis `ApplyAttitudeAssist`. `UpdateSteeringState:465-486` (cible `:466-479`, `MoveTowards` `:480-486`). **`ApplyAttitudeAssist:498-521`** — `return` immédiat si `groundedWheels <= 0` `:500`, couple `:507-515`. `ApplyAntiRoll:526-563` — paire appliquée aux deux points de contact `:553`/`:558`. API publique : `HasProfile:71`, `WheelCount:74`, `Profile:80`, `BindProfile:97`, `ApplyProfile:109`, `ApplyDriveIntent:204`, `ResetSuspensionState:225`, `TryGetWheelState:567`, `TryGetTireSample:585`, `CurrentSteerAngleDegrees:598`, `TrySampleTelemetry:608`. Champs privés `:50-68`. En-tête `:21-32` : l'IA écrit encore sa vitesse, 5.14 lève ceci ; aucun contrôle d'autorité ici (AD-21).
- `Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs` -- fonctions pures de référence, motif à suivre. `IsWheelGrounded:78-80` (**inégalité stricte** : à `restLength` exact la roue est en l'air), `ResolveCompression:88-95`, `ResolveSuspensionForce:107-123` (plafond `staticLoad × 4`, `MaxSuspensionForceMultiple:39`), **`ResolveAntiRollForces:139-147` — seul terme sans plafond ni seuil**, `ResolveLevellingTorque:183+`, `ResolveAttitudeDampingTorque` (projette sur `forward`/`right` : **le lacet n'est jamais touché**), `IsSurfaceContact:229` (dégâts uniquement), `WheelState`/`TelemetrySample` `:235-330`.
- `Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs` -- fonctions pures : `ResolveSlipRatio:55`, `ResolveSlipAngleDegrees:75`, `ResolveGripFraction:99`, `ResolveTireForces:141` (budget unique `adherence × charge`), `ResolveLowSpeedRamp:181`, `ResolveDriveTorqueFactor:196`, `ResolveWheelDriveTorque:215`, `ResolveWheelBrakeTorque:260`, `IntegrateWheelAngularVelocity:291`, `SampleTire:327`, `TireSample:349` (`Grounded`, `NormalLoad`, `GripUsage`). C'est ici que le glissement est déjà mesuré : le contrôle de traction s'y branche.
- `Assets/RoadRage/Features/Vehicles/VehicleSteeringModel.cs` -- `ResolveSteerAngleDegrees:39`, `ResolveSteerRateDegreesPerSecond:74`, `ResolveWheelSteerAngleDegrees:94` (consomme `IsSteering`), `MoveSteerAngleDegrees:110`.
- `Assets/RoadRage/Features/Vehicles/VehicleProfile.cs` + `VehicleProfileDef.cs` + `Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset` -- **31 champs, deux copies identiques** (`VehicleProfileDef.cs:30-72` vs asset `:17-69`) : les garder alignées. Aucun champ d'aide n'existe. `TryValidate:98-403` (aucune règle sur une aide, puisqu'aucune n'existe) ; helpers `:445-463` ; `OnValidate:468-474` avertit seulement. Champs d'ancrage : `LateralFrictionCoefficient:77`, `AntiRollRate:62`, `AttitudeLevellingRate:67`, `AttitudeDamping:72`, `TirePeakSlip*:161-171`.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- joueur. `ResolveDriveAuthority` statique **pure** `:592-608` (réduction 3.5, déjà prouvée en EditMode), appelée par `SubmitIntentToPhysicsLayer:665-680` → `physicsBody.ApplyDriveIntent(intent, maxForwardSpeed, steerRateDegreesPerSecond, brakeTorque)` `:679`. `ResolveSteerDirectionMultiplier` (contrat 3.2) et les trois résolveurs de dégâts 3.5 sont à préserver. `body.linearVelocity = Vector3.zero` `:295` = récupération 3.4, **l'unique occurrence autorisée**.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- IA. `ApplyMovement:847-884` écrit `linearVelocity` `:861`/`:870` et `MoveRotation` `:867` : **ne pas y toucher** (5.14). Câblage de la visée `:357-369` (distance = vitesse × `lookAheadSeconds`, `ResolveLookAheadPoint` `:362-367`, `ComputeSeekIntent` `:369`). `ComputeSeekIntent:787-820` (pure, publique). `ResolveNextNode:372-…` (bascule de nœud, appelée `:347-352`). **Aucune méthode `static` supplémentaire** : `Story52…:138` en compte 3.
- `Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs` -- `ResolveLookAheadPoint:293-336` : la cible reste le nœud tant que la distance de visée ne l'atteint pas, puis glisse au-delà le long du sens de circulation du nœud. La cible est prolongée jusqu'à `vitesse × durée` (jusqu'à 4,8 m à 8 m/s), **puis bascule d'un coup sur le nœud suivant** : c'est le saut à corriger ici, au même endroit que la loi existante. `HasPassedUnreachableWaypoint:234`, `SelectSuccessorTowardTarget:280`.
- `Assets/RoadRage/Features/Vehicles/VehicleWheel.cs` -- `IsSteering`, `IsDriven`, `axleIndex`, `radius` : 4 roues, avant directrice, **quatre roues motrices** (`Story512…:685` le garde sur les deux copies du profil).
- `Assets/RoadRage/Tests/EditMode/Story513ArcadeAssistsAndUnevenGroundTests.cs` -- **à créer**. Conventions : classe `public sealed`, `[Test]` seul, namespace `RoadRage.Tests.EditMode`, doc XML en français, helpers locaux `CodeWithoutComments`, `Occurrences`, `ExtractMethodBody`, `WithInstantiated`, `AssertRefused`. Asmdef `RoadRage.Tests.EditMode.asmdef` référence déjà `RoadRage.Features.Vehicles`.
- `Assets/RoadRage/Tests/EditMode/Story511VehicleChassisWheelsAndSuspensionTests.cs` -- gardes à préserver et à étendre : `WheelCollider` interdit sur **8 fichiers listés `:70-79`** (`:531`) — le fichier neuf doit y être ajouté ; `RollStabilityAssistIsGone` `:545-569` ; `ResolveGroundFrictionForce` interdit `:156`/`:160` ; profil `:322-353` ; `TheVehicleUndersideAndTheSuspensionTravelClearTheAuthoredCurb:697` (`Travel > 0,12`) ; `CurbHeight = 0.12f` `:34` ; dégagement `:664` (0,97 m) ; télémétrie `:727-746`.
- `Assets/RoadRage/Tests/EditMode/Story512TireForcesAndSteeringTests.cs` -- `TheDamageAuthorityIsProvenRatherThanSearched:615` appelle `ResolveDriveAuthority(profile, …)` : **ne pas changer cette signature**. `TheAuthoredProfileKeepsAnArcadeEnvelope:714` (enveloppe à respecter). `Occurrences("linearVelocity =") == 1` `:458` ; `MoveRotation` absent `:455`/`:462` ; `lateralGrip` absent `:466` ; `Occurrences("SubmitIntentToPhysicsLayer(VehicleDriveIntent.Idle)") == 2` `:648` (l'AC « aucune aide ne remet l'entrée à zéro ») ; interdits sur `VehiclePhysicsBody.cs` `:476-492` (dont `Random`, `NetworkedRageState`, `body.isKinematic = !IsServer`).
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` -- `NoModuleColliderRisesAboveTheDrivingPlane:865` : tout collider doit avoir `max.y <= 0,0001` **sauf** un nom commençant par `Col_Curb`, plafonné à `CurbHeight` → toute géométrie de banc authorée doit respecter ces deux règles ou vivre dans une fixture, pas dans le district. `SidewalksCarryTheDedicatedAreaAndStayOutOfTheVehicleBake:929`. Rejeu de trajectoire : appeler les fonctions pures réelles `:1197-1279`.
- `Assets/RoadRage/Tests/PlayMode/Story511VehicleChassisWheelsAndSuspensionPlayModeTests.cs` -- banc de référence : `DriveAt` **écrit `linearVelocity` en bloc** `:247-251` (volontaire, mesure la 5.11 — ne pas convertir). Seuils = bornes de non-régression : `MaxVerticalExcursionAtDrivingSpeed = 0.4f` `:69`, `MaxVerticalSpeedAtDrivingSpeed = 4f` `:72`. Ce banc mesure déjà montée vs projection : c'est l'instrument de l'AC2.
- `Assets/RoadRage/Tests/PlayMode/Story512TireForcesAndSteeringPlayModeTests.cs` -- passe par `physics.ApplyDriveIntent` (`:197-200`) : **écrit, jamais exécuté**. Modèle du banc à livrer.
- `Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs` -- instrument de mesure en jeu, garde `Debug.isDebugBuild` `:57-64` ; `ComposeText:88-137` ; lit `TryGetWheelState`/`TryGetTireSample`, **ne recalcule rien**. Monté dans `Dev_VehicleSandbox.unity`, pas dans `MVP_Run`.
- `_bmad-output/implementation-artifacts/deferred-work.md` -- clôture `:278-280` (saut du point de visée, décision du 2026-09-18). **Le fichier est abîmé** : `:262`/`:270` `source*spec` au lieu de `source_spec`, `:264`/`:272` `evidence:` en colonne 0, `Col_Curb*\*` en `:263`/`:271`. Réparer ces 6 lignes fait partie des tâches.
- `Assets/RoadRage/Prefabs/Greybox_Intersection.prefab` -- la bordure authoree : 4 `Col_Curb_*` sous `Collision` `:2012`, taille `(4, 0,12, 0,3)`, centre `y = 0,06`, face intérieure à `|z| = 4` m. **Les deux prefabs véhicule partagent le même profil** (guid `c0d6f3b1e92b5f045b0aabf44ad005e1`).
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- la scène d'intégration, et le lieu de la recette. `GreyboxMap` (14 enfants) porte le district : `Intersection_Center_Crossroads` au centre, les **quatre avenues rectilignes** `Avenue_CenterToEast` / `_North` / `_South` / `_West` (instances de `Greybox_RoadSegment_TwoWay`, guid `b48fc52f2967574458e6c9cc3a10d61f`), les anneaux `Ring_*`, les `Roundabout_*`, les `TJunction_*`, les `TunnelPortal_*` et les `Greybox_CityBlock_A_*`. L'objet `LaneGraph` porte le graphe. **Le relief de recette se pose ici, en objet de scène sous `GreyboxMap`** — jamais dans un prefab de module, sinon il apparaîtrait sur toutes les instances, y compris celles des routes IA, et heurterait `Story510…:865`.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Vehicles/VehicleProfile.cs` + `VehicleProfileDef.cs` + `VehicleProfileDef_Default.asset` -- ajouter les termes des trois aides et le facteur d'autorité par roues au sol, étendre `TryValidate` (finis, bornes, zéro = inerte) et poser les valeurs par défaut -- les deux copies de données restent alignées, sinon `TheDefaultProfileAssetIsAuthoredAndValid` et `TheDrivetrainIsAllWheelDriveOnBothCopiesOfTheProfile` divergent.
- [x] `Assets/RoadRage/Features/Vehicles/VehicleArcadeAssist.cs` -- nouveau : fonctions pures (couple de stabilité en lacet borné, atténuation de couple moteur par glissement, couple de récupération borné, facteur `ResolveGroundedAuthorityFactor(groundedWheels, wheelCount)`) -- c'est le modèle à prouver en EditMode, sur le patron de `VehicleTireModel`.
- [x] `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` -- publier le nombre de roues au sol, appliquer le facteur d'autorité au couple moteur et à l'angle de roue, appliquer les aides au niveau caisse et par roue, et étendre `ApplyAttitudeAssist` pour que l'attitude tienne quand aucune roue ne touche **sans écrire la rotation** -- la couche reste la seule autorité (AD-21, AD-35).
- [x] `Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs` -- borner l'anti-roulis contre la charge portée, sans changer son sens ni sa paire -- c'est le seul terme non plafonné de la couche et le suspect direct de la projection par une bordure.
- [x] `Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs` -- rendre `ResolveLookAheadPoint` continu au passage de nœud par une mémoire de la cible précédente et une vitesse de rappel authorée, sans muter la géométrie -- clôture la dette `deferred-work.md:278-280`, prise par cette story le 2026-09-18.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- porter la cible précédente et la passer au résolveur au point d'appel existant, **sans nouvelle méthode `static`** -- le saut se corrige dans la fonction pure, pas par un second chemin.
- [x] `Assets/RoadRage/Features/Vehicles/DriverProfile.cs` + `DriverProfileDef.cs` + l'asset de profil conducteur -- ajouter le paramètre de rappel de cible -- donnée authoree dans un `Def`, jamais une constante de contrôleur (`Story59…:429`).
- [x] `Assets/RoadRage/Tests/EditMode/Story513ArcadeAssistsAndUnevenGroundTests.cs` -- nouveau : matrice de cas limites, désactivation individuelle par valeur authoree, bornes de chaque terme, facteur d'autorité par roues au sol, continuité du point de visée au passage de nœud, présence et hauteur du relief de recette dans `MVP_Run` avec aucun `LaneNode` dans son emprise, absence de `WheelCollider` et d'écriture de mouvement -- les preuves EditMode de la story.
- [x] `Assets/RoadRage/Tests/EditMode/Story511VehicleChassisWheelsAndSuspensionTests.cs` + `Story512TireForcesAndSteeringTests.cs` -- ajouter le fichier neuf à la liste des 8 gardés, étendre les gardes de profil aux nouveaux champs, préserver l'enveloppe arcade et le contrat de `ResolveDriveAuthority` -- ne rien relâcher des gardes existantes.
- [x] `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` -- rejouer la continuité du point de visée avec les fonctions pures réelles, sur les nœuds du carrefour -- c'est l'instrument EditMode de la dette reprise.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- authorer le relief de recette sur `Avenue_CenterToEast` : un **dos-d'âne** à crête authoree **0,12 m** (profil en pente douce, pleine largeur de chaussée) et une **marche basse** de 0,12 m juste à côté, en objets de scène sous `GreyboxMap`, sans toucher un prefab de module ni un `LaneNode` -- sans relief longitudinal, le franchissement d'une bosse et d'une marche n'est observable nulle part en Play Mode, et c'est la recette humaine qui tient l'AC2. Opération sous la double garde d'état d'`AGENTS.md` (scenes + Git) ; sauvegarder la scène seulement après vérification.
- [x] `Assets/RoadRage/Tests/PlayMode/Story513ArcadeAssistsAndUnevenGroundPlayModeTests.cs` -- nouveau : franchissement de la bordure authoree de 0,12 m, franchissement du relief de recette, roue délestée, autorité réduite -- **banc à exécuter par l'humain**, l'agent ne peut pas produire la porte PlayMode.
- [x] `Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs` -- afficher le nombre de roues au sol et le facteur d'autorité appliqué, en lecture seule de l'état publié -- instrument de la recette humaine, sans seconde vérité.
- [x] `_bmad-output/implementation-artifacts/deferred-work.md` -- clôturer `:278-280`, enregistrer les dettes nouvelles, et **réparer les 6 lignes abîmées** (`:262-264`, `:270-272`) -- le formateur markdown y a cassé la clé `source_spec`, converti `Col_Curb_*` en `Col_Curb*\*` et aplati deux `evidence`.
- [x] `docs/setup/story-5-13-arcade-assists-and-uneven-ground-notes.md` -- nouveau : valeurs authorees retenues, mesures, constats et procédure de recette -- trace de livraison, comme les notes 5.11 et 5.12.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- `5-13-arcade-assists-and-uneven-ground` en `in-progress`.
- [x] `graphify update .` -- regrapher, et comparer la taille à la mesure 5.12 (3 089 nœuds / 7 352 liens).

**Acceptance Criteria:**

- Given la conduite arcade doit pardonner plus que la simulation, when les aides sont ajoutées, then la stabilité en lacet, le contrôle de traction et la récupération de tête-à-queue sont des termes authorés individuellement réglables, chacun lit **uniquement `VehicleProfileDef`** et jamais la rage, la peur ou une disposition de conducteur, et chacun est désactivable par sa seule valeur authoree sans qu'aucune aide ne remette silencieusement l'entrée conducteur à zéro.
- Given le district porte maintenant une vraie bordure (Story 5.11), when le véhicule rencontre un sol irrégulier, then bordures, petites marches, bosses et géométrie irrégulière sont franchies sans que le véhicule soit projeté en l'air ni coincé contre la lèvre, et le réglage est fait contre la hauteur de bordure authoree en 5.11 (**0,12 m**), jamais contre une valeur supposée. Le relief longitudinal qui rend ce franchissement observable est **authoré dans `MVP_Run`** — un dos-d'âne et une marche basse de 0,12 m sur une avenue — en objets de scène, sans déplacer un `LaneNode` ni modifier un prefab de module.
- Given des roues peuvent quitter le sol, when une ou plusieurs perdent le contact, then l'autorité de conduite et de direction est réduite en proportion du nombre de roues au sol, un véhicule en vol garde une attitude stable sans que sa rotation soit artificiellement gelée ni écrite, et le nombre de roues au sol et son effet sur l'autorité sont **vérifiables en EditMode**.
- Given la dette `deferred-work.md:278-280` a été confiée à cette story le 2026-09-18, when le point de visée est rendu continu au passage de nœud, then la continuité est prouvée en EditMode sur les trajectoires rejouées du district, et l'entrée de dette est close avec sa mesure.
- Given la porte PlayMode est rouge (3/33) et non filtrable, when la story est livrée, then l'absolu des critères 2 et 3 est porté par une **mesure humaine en Play Mode consignée dans la note de livraison**, et cette mesure est une **condition de clôture** : sans elle la story ne passe pas en revue.

## Spec Change Log

## Design Notes

- **Pourquoi l'IA est inerte ici, et pourquoi ce n'est pas un oubli.** Les deux prefabs véhicule partagent le même profil authoré (guid `c0d6f3b1e92b5f045b0aabf44ad005e1`), donc aucune aide ne peut être différenciée par le profil. Mais jusqu'à la Story 5.14, `NetworkedAIVehicleDriverController.ApplyMovement` **remplace** la vélocité (`:861`, `:870`) et **impose** le lacet (`:867`) à chaque pas : un couple d'aide appliqué à une caisse d'IA est donc écrasé, pas nocif. Aucun verrou par véhicule n'est ajouté (ce serait un second chemin de vérité, AD-35) ; l'IA devient réellement concernée en 5.14, par construction. Le seul risque à surveiller est celui du **premier pas** entre l'application de l'aide et l'écrasement de la rotation : la recette doit vérifier que le trafic IA ne régresse pas en `MVP_Run`.
- **L'anti-roulis est le suspect n°1 de la projection, et c'est un fait de code.** `ResolveAntiRollForces` est le seul terme sans plafond ni seuil, il est proportionnel à l'écart de compression d'un essieu, et une bordure franchie par une seule roue produit exactement cet écart. Sa magnitude maximale atteignable (`antiRollRate × travel` = 20 000 × 0,25 = 5 000 N) dépasse la charge statique d'une roue (≈ 2 943 N) : il peut localement annuler ou doubler la charge d'un coin. Le borner **contre le budget de charge** est le remède attendu ; changer son sens ou sa paire ne l'est pas.
- **Le « coincé contre la lèvre » a une cause distincte, et elle est géométrique.** La détection tient en **un rayon vertical unique** par roue (`VehiclePhysicsBody.cs:313-319`), dont l'origine est l'ancrage de roue. Si l'ancrage passe au-delà de la lèvre horizontalement, le rayon manque la marche : la roue est déclarée en l'air, la suspension n'existe plus, alors que le collider du châssis bute sur la face verticale. C'est une **inférence de géométrie**, pas une mesure — la recette doit la confirmer ou l'infirmer avant qu'un mécanisme soit ajouté.
- **La mesure humaine est une condition de clôture (décision du 2026-09-18).** Le précédent de 5.11 et 5.12 était de livrer avec la mesure due, jamais exécutée : quatre bancs PlayMode attendent encore (`deferred-work.md:290-292`), et la revue de 5.12 a démontré qu'une couche physique inerte à l'exécution laisse les 573 tests EditMode verts. Rendre la mesure bloquante est ce qui empêche cette story de reproduire l'écart.
- **Le saut de visée est une décision de conduite, pas un bug de géométrie** (`deferred-work.md:278-280`). La cible est prolongée jusqu'à `vitesse × durée` (4,8 m à 8 m/s) puis bascule d'un coup sur le nœud suivant. La 5.12 ne l'a pas corrigé parce que la mesure montrait un nombre de pas dans l'emprise de la bordure **identique** avec et sans visée anticipée (104) : le saut n'était pas la cause du défaut observé. Le remède est donc un lissage de cible — filtrage ou vitesse de rappel authoree —, ce qui en fait une fonction pure et une valeur de `Def`, pas une retouche du graphe.
- **Pourquoi un relief de recette, et pourquoi deux formes (demande du 2026-09-18).** Le district n'a de dénivellation que la bordure du carrefour, franchie **latéralement** : c'est le cas d'une roue seule qui se comprime, pas celui d'une caisse qui passe sur une bosse. L'AC2 nomme pourtant trois cas distincts — bordures, **petites marches**, **bosses** — et la clause « ni coincé contre la lèvre » ne se teste que **longitudinalement**. Le dos-d'âne couvre la bosse ; la marche basse couvre la lèvre, et c'est elle qui confirmera ou infirmera l'inférence géométrique ci-dessus. Les deux sont posés en **objets de scène** et non dans `Greybox_RoadSegment_TwoWay.prefab` : un prefab se retrouverait sur les quatorze instances du district, dont celles des routes IA, et ferait échouer `Story510…:865` sans rien apporter à la recette.
- **Ce qui n'est pas converti.** Le banc PlayMode 5.11 (`DriveAt:247-251`) écrit `linearVelocity` volontairement : il mesure la suspension, pas la conduite. Le convertir mêlerait deux preuves. La 5.13 ajoute son propre banc, qui passe par la couche d'intent.
- **Écart accepté, tracé.** Reprendre la dette `:278-280` oblige à toucher le contrôleur IA, que trois gardes rendent coûteux (`Story52…:138` sur les `static`, cinq tests de `Story59` qui lisent son source). L'intention est de **ne rien modifier des fixtures 5.9 et 5.2** en logeant la correction dans la fonction pure ; toute modification devenue nécessaire est un écart explicite à consigner dans le Spec Change Log, jamais un ajustement silencieux.

## Verification

**Commands:**

- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story513ArcadeAssistsAndUnevenGroundTests"` -- attendu : fixture verte, exit 0.
- `.\scripts\validate.ps1 -TestMode EditMode` -- attendu : les 573 verts de la baseline 5.12 **plus** ceux de cette story, 0 erreur Console. Si la porte Console reste rouge à cause d'entrées historiques rejouées sans `--since`, la mesure de repli est `unity cmd run_tests --mode EditMode` + `test_status`, et le fait doit être rapporté tel quel.
- `.\scripts\validate.ps1 -TestMode PlayMode` -- attendu : **non productible par l'agent** (harnais rouge 3/33, non filtrable). Ne jamais conclure d'un résultat vide.
- `graphify update .` -- attendu : graphe régénéré, comparaison avec 3 089 nœuds / 7 352 liens (5.12).

**Manual checks (if no CLI):**

- **Blocage de clôture — recette humaine, Éditeur, `MVP_Run`, hôte.** À exécuter après livraison du banc ; sans résultat brut rapporté, la story ne passe pas en revue.
  - Franchir la bordure authoree du carrefour central (`Col_Curb_*`, 0,12 m) à vitesse de conduite, roue par roue et de biais. Attendu : le véhicule monte, aucun décollage, aucune immobilisation contre la lèvre. Consigner l'excursion verticale et la vitesse verticale maximales, à comparer aux bornes de non-régression du banc 5.11 (0,4 m / 4 m/s).
  - Franchir le relief de recette de `Avenue_CenterToEast` dans les deux sens : **dos-d'âne** puis **marche basse**, à basse vitesse et à vitesse de conduite. Attendu : la caisse passe, aucun décollage, aucune immobilisation contre la face. C'est le seul endroit du district où le franchissement longitudinal est observable.
  - Si la marche basse ne reproduit **pas** d'immobilisation, le dire : cela infirme l'inférence géométrique sur le rayon de roue, et aucun mécanisme ne doit être ajouté pour un symptôme non reproduit.
  - Délester une roue (bordure franchie par un seul côté) et vérifier que la caisse se redresse au lieu de se coucher : c'est la démonstration qui manquait à `deferred-work.md:258-260`.
  - Saut ou perte des quatre contacts : attendu une attitude stable, **sans rotation figée** — le véhicule tourne encore sur lui-même s'il était en rotation.
  - En virage et en appui, vérifier que chaque aide est perceptible puis, valeur authoree remise à zéro une par une, qu'elle disparaît : la désactivation individuelle se voit.
  - Trafic IA du carrefour en conduite nominale : attendu aucune régression visible par rapport à l'état de la Story 5.12.
    **Réserve connue, mesurée le 2026-09-18 :** un véhicule IA qui franchit le relief de recette **décolle et poursuit son trajet en l'air**. Ce n'est pas un défaut du relief ni des aides — c'est le chemin de mouvement en boucle ouverte de l'IA (`NetworkedAIVehicleDriverController.ApplyMovement`), qui réimpose le vecteur vitesse à chaque pas — et c'est la Story **5.14** qui le possède. Conséquence pour cette recette : **cette ligne ne se juge pas sur l'avenue**. L'observer **sur le carrefour central**, où aucune IA ne touche le relief d'aucune sorte ; et **ne pas** en conclure à une régression des aides. Constat complet : `deferred-work.md` et section 7 de la note de livraison.
- Lire le nombre de roues au sol et le facteur d'autorité dans la vue de télémétrie et consigner les valeurs.
- Garde de double état d'`AGENTS.md` : `MVP_Run.unity` et `git status --short` doivent revenir à leur état initial après la recette.

## Suggested Review Order

**Le modèle pur des aides — le cœur du changement**

- L'autorité par roues au sol est une proportion, jamais un réglage : quatre lectures, une seule formule.
  [`VehicleArcadeAssist.cs:62`](../../../Assets/RoadRage/Features/Vehicles/VehicleArcadeAssist.cs#L62)

- Le budget de lacet est dérivé du véhicule (adhérence × charge × demi-voie), jamais un nombre absolu.
  [`VehicleArcadeAssist.cs:95`](../../../Assets/RoadRage/Features/Vehicles/VehicleArcadeAssist.cs#L95)

- Stabilité en lacet : exactement nulle à l'arrêt de rotation, donc incapable de tenir un cap contre le conducteur.
  [`VehicleArcadeAssist.cs:130`](../../../Assets/RoadRage/Features/Vehicles/VehicleArcadeAssist.cs#L130)

- Contrôle de traction : un facteur dans ]0, 1[, plafonné sous 1 pour ne jamais annuler le couple moteur.
  [`VehicleArcadeAssist.cs:166`](../../../Assets/RoadRage/Features/Vehicles/VehicleArcadeAssist.cs#L166)

- Récupération de tête-à-queue : bornée par le budget, dirigée par l'entrée, inerte sous le seuil de dérive.
  [`VehicleArcadeAssist.cs:209`](../../../Assets/RoadRage/Features/Vehicles/VehicleArcadeAssist.cs#L209)

**La couche physique qui consomme ces sorties**

- L'autorité appliquée vient d'une source unique publiée : compte et facteur ne peuvent plus diverger.
  [`VehiclePhysicsBody.cs:332`](../../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L332)

- Le couple moteur est réduit par l'autorité — retirer cette ligne faisait disparaître l'AC3 sans rougir.
  [`VehiclePhysicsBody.cs:351`](../../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L351)

- Le couple par roue est atténué par le contrôle de traction, sur le glissement du pas précédent.
  [`VehiclePhysicsBody.cs:460`](../../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L460)

- Le budget de lacet suit le contact : sans roue au sol, les aides de lacet deviennent inertes.
  [`VehiclePhysicsBody.cs:372`](../../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L372)

- Compte et facteur publiés ensemble, depuis une seule mesure — c'est l'instrument de la recette.
  [`VehiclePhysicsBody.cs:538`](../../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L538)

- Les deux aides de lacet, bornées individuellement puis en somme : aucun chemin ne dépasse le budget.
  [`VehiclePhysicsBody.cs:689`](../../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L689)

- L'amortissement d'assiette n'est plus conditionné au contact : c'est lui qui tient une caisse en vol.
  [`VehiclePhysicsBody.cs:605`](../../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L605)

- L'anti-roulis, seul terme qui n'avait aucun plafond, est borné par la charge portée.
  [`VehicleSuspensionModel.cs:148`](../../../Assets/RoadRage/Features/Vehicles/VehicleSuspensionModel.cs#L148)

- L'état publié est purgé là où le véhicule est reposé : aucune autorité périmée après une téléportation.
  [`VehiclePhysicsBody.cs:258`](../../../Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs#L258)

**La donnée authorée**

- Quatre termes d'aide, chacun désactivable par sa seule valeur nulle.
  [`VehicleProfile.cs:176`](../../../Assets/RoadRage/Features/Vehicles/VehicleProfile.cs#L176)

- La demi-voie moyenne : l'échelle à laquelle une force latérale produit un couple de lacet.
  [`VehicleProfile.cs:527`](../../../Assets/RoadRage/Features/Vehicles/VehicleProfile.cs#L527)

- Les valeurs par défaut, puis les quatre refus de validation (dont l'atténuation strictement sous 1).
  [`VehicleProfileDef.cs:79`](../../../Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs#L79)
  [`VehicleProfileDef.cs:412`](../../../Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs#L412)

**La continuité du point de visée — la dette reprise**

- La loi idéale de la 5.12, inchangée, et le rappel borné qui la rend continue au passage de nœud.
  [`LaneGraphRouting.cs:311`](../../../Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs#L311)

- La fonction de rappel : la cible rendue ne peut plus s'écarter d'un pas, quelle que soit la discontinuité.
  [`LaneGraphRouting.cs:373`](../../../Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs#L373)

- La mémoire de cible vit chez l'appelant ; la fonction pure reste sans état.
  [`NetworkedAIVehicleDriverController.cs:384`](../../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L384)

- Et elle est effacée partout où l'IA repose le véhicule — sinon le saut supprimé réapparaissait, invisible.
  [`NetworkedAIVehicleDriverController.cs:482`](../../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L482)

- Dernière barrière avant l'écriture de vitesse : un point non fini n'atteint jamais `linearVelocity`.
  [`NetworkedAIVehicleDriverController.cs:842`](../../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L842)

- La vitesse de rappel, authorée dans le profil conducteur, et son passage par la modulation de disposition.
  [`DriverProfile.cs:84`](../../../Assets/RoadRage/Features/Vehicles/DriverProfile.cs#L84)
  [`DriverModel.cs:159`](../../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L159)

**Le relief de recette dans la scène d'intégration**

- Les deux objets de scène, sous `GreyboxMap`, sur l'avenue : le dos-d'âne puis la marche basse.
  [`MVP_Run.unity:2704`](../../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L2704)
  [`MVP_Run.unity:587`](../../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L587)

**Périphériques**

- L'instrument des contrôles humains : roues au sol et autorité appliquée, en lecture seule.
  [`VehiclePhysicsTelemetryView.cs:18`](../../../Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs#L18)

- Les gardes de la story : autorité, relief, et la consommation des sorties pures.
  [`Story513ArcadeAssistsAndUnevenGroundTests.cs:102`](../../../Assets/RoadRage/Tests/EditMode/Story513ArcadeAssistsAndUnevenGroundTests.cs#L102)
  [`Story513ArcadeAssistsAndUnevenGroundTests.cs:644`](../../../Assets/RoadRage/Tests/EditMode/Story513ArcadeAssistsAndUnevenGroundTests.cs#L644)
  [`Story513ArcadeAssistsAndUnevenGroundTests.cs:985`](../../../Assets/RoadRage/Tests/EditMode/Story513ArcadeAssistsAndUnevenGroundTests.cs#L985)

- La continuité mesurée sur le rejeu de district, avec les fonctions pures réelles.
  [`Story510LaneGraphAndRoutedTrafficTests.cs:1277`](../../../Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs#L1277)
