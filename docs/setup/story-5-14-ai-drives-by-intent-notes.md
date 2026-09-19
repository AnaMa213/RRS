# Story 5.14 — AI Drives by Intent : notes de livraison

Trace de livraison, sur le meme modele que `story-5-11-`, `story-5-12-` et `story-5-13-…-notes.md`.
Le spec fait autorite : `_bmad-output/implementation-artifacts/spec-5-14-ai-drives-by-intent.md`.
Statut de livraison : **implementation faite, porte EditMode verte, recette humaine non executee**.
Rien dans ce document n'est un resultat de comportement.

## 1. Ce qui a change

| Fichier | Changement |
| --- | --- |
| `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` | `ApplyMovement` **supprime** ; `IntegrateLongitudinalSpeed` remplace par `ResolveLongitudinalPedal` (qui rend un `VehicleDriveIntent`) ; vitesse **relue** du `Rigidbody` ; `SubmitIntentToPhysicsLayer` ajoute sur le patron du vehicule joueur ; purge de la memoire de visee au repositionnement |
| `Assets/RoadRage/Tests/EditMode/Story514AiDrivesByIntentTests.cs` | **nouveau**, 11 tests |
| `Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs` | garde `+ verticalVelocity` **retiree** (elle protegeait l'ecriture supprimee) ; commentaires d'en-tete et de convention de lacet realignes |
| `Assets/RoadRage/Tests/EditMode/Story512TireForcesAndSteeringTests.cs` | garde piege `TheTransientStateIsNamedAndTheStoryThatLiftsItIsNotThisOne` **retiree** (decision humaine du 2026-09-19), avec un bloc de passation a sa place |
| `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` | en-tete corrige : il decrivait encore l'IA comme ecrivant la vitesse et le lacet |
| `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` | commentaire du rejeu corrige : il renvoyait a `ApplyMovement` comme a une methode vivante |
| `docs/setup/devworkflow-rollout.md` | section **P2 — harnais PlayMode** creee : le prerequis dur de l'epic etait declare et absent |
| `_bmad-output/implementation-artifacts/deferred-work.md` | cloture de `:251-252`, `:275-276`, `:299-300`, `:307-308` ; trois entrees nouvelles |
| `_bmad-output/implementation-artifacts/sprint-status.yaml` | `5-14-ai-drives-by-intent` en `in-progress` |

**Aucune ecriture de mouvement ne subsiste dans le pas de conduite de l'IA.** La seule ecriture de
vitesse qui reste du fichier est la remise a zero de `RecoverAtWaypoint`, qui n'est pas de la
conduite, et la seule du depot est la recuperation joueur 3.4
(`NetworkedVehicleDriverController.cs:295`).

## 2. La conversion acceleration -> pedale, et pourquoi il en faut une

Le modele de conduite (`DriverModel`) demande une acceleration en **m/s2** ; la couche physique attend
une **position de pedale** dans `[0, 1]`. Sans echelle, une pedale a 1 transformerait une demande de
1,5 m/s2 en 16,2 m/s2 reels : la vitesse desiree du profil serait atteinte par une poussee que la
personnalite du conducteur ne decrit pas.

- La pedale d'acceleration est donc la **fraction de la capacite disponible du vehicule** :
  `acceleration / (capacite a plein gaz x facteur de vitesse de la couche)`, bornee a `[0, 1]`. Le
  facteur de vitesse est celui que la couche physique utilise deja
  (`VehicleTireModel.ResolveDriveTorqueFactor`), donc la pedale et la couche ne peuvent pas diverger.
- La pedale de frein est la fraction de la capacite de freinage du vehicule. **Sous
  `MinimumDirectionSpeed`, l'entree de frein devient une MARCHE ARRIERE dans la couche** : le modele
  de conduite IA n'ayant aucune decision de recul, l'intent rendu est `Idle` (frein moteur authore)
  et jamais une marche arriere inventee.
- Les deux capacites sont derivees **du profil physique** que la couche lit deja (nombre de roues
  motrices, couple moteur, couple de frein, masse, rayon moyen) et calculees **paresseusement** :
  l'ordre d'execution des `Awake` entre le controleur et la couche n'est pas defini. Sans couche
  physique ou sans profil applique, rien n'est mis en cache et la conversion rend un intent neutre —
  jamais un repli numerique invente.

## 3. Ce qui est mesure — EditMode, 11 tests (`Story514AiDrivesByIntentTests`)

`TheDrivingStepWritesNoVelocityPositionOrRotation`, `TheOnlyRemainingVelocityWriteIsTheRecoveryReset`,
`TheIntentIsSubmittedToTheSamePhysicsLayerAsThePlayerVehicle`,
`TheAiPrefabSharesTheSamePhysicsLayerAndProfileAsThePlayerPrefab`,
`TheOpenLoopSpeedFieldIsGoneAndTheSpeedIsReadFromTheRigidbody`,
`TheDriverModelDecisionAndTheMissingProfileDiagnosticSurviveUnchanged`,
`EveryInertExitOfTheDrivingStepPushesTheIdleIntent`,
`TheAimPointMemoryIsPurgedWhereverTheVehicleIsRepositioned`,
`ARequestedAccelerationBecomesAPedalFractionOfTheVehicleCapacity`,
`ARequestedDecelerationBecomesABrakeFractionOfTheVehicleBrakeCapacity`,
`ASlowAiNeverEngagesTheReverseOfTheBrakeAxis`.

Porte executee le 2026-09-19 : **EditMode vert, 0 erreur Console**, `recompile_status: up_to_date`,
Editeur `6000.6.0f1` sur le port 7801, CLI `1.0.0-beta.8`.

**La garde piege de la Story 5.12 a fait son travail.**
`Story512TireForcesAndSteeringTests.TheTransientStateIsNamedAndTheStoryThatLiftsItIsNotThisOne`
exigeait que l'IA ecrive encore sa vitesse en bloc et impose son lacet, et son commentaire annoncait
que la Story 5.14 leverait cet etat intermediaire. Elle est devenue rouge des que la 5.14 a ete
livree. **Decision humaine du 2026-09-19 : elle est RETIREE**, et non rearmee sur l'autre versant --
l'invariant qu'elle portait (aucune ecriture de vitesse, de position ni de rotation depuis un chemin
de conduite) est desormais tenu par `Story514AiDrivesByIntentTests`, qui l'assere sur le pas de
conduite de `FixedUpdate` au lieu du fichier entier. Un bloc de passation, a l'emplacement de la
garde, garde la trace de ce qu'elle a fait et pourquoi elle a disparu.

## 4. Ce qui n'est PAS mesure

Aucun test PlayMode n'a ete ajoute : les deux `[UnityTest]` que l'epic prevoyait pour cette story
partent avec le perimetre ecarte (reaction au choc et recuperation physique, `ANO-5.10-03` AC1 a
AC8). Quatre comportements reposent donc **entierement** sur la recette humaine :

1. une IA **roule** ;
2. une IA **poussee** est deviee, peut etre mise en travers, peut quitter la chaussee, et ne se
   realigne pas en une frame ;
3. une IA ne **decolle plus** sur le relief de recette ;
4. les **aides de la 5.13** sont perceptibles sur le trafic IA.

Le harnais PlayMode reste non productible par l'agent (`unity cmd run_tests --mode PlayMode`
synchrone ne peut pas fonctionner, `--async_tests` a rendu 0 test). Le prerequis est desormais suivi
dans `docs/setup/devworkflow-rollout.md`, section P2.

## 5. Procedure de recette (Editeur, `MVP_Run`, hote)

A executer dans cet ordre, en rapportant les valeurs brutes.

1. **Une IA roule-t-elle ?** — a faire en premier. Trafic du carrefour en conduite nominale : les
   trois IA avancent, atteignent leur vitesse de croisiere, franchissent les nœuds, bouclent leur
   parcours. Une IA immobile est un **echec de la story**, pas un defaut d'ambiance : lire alors
   `GroundedWheelCount` et le facteur d'autorite dans la vue de telemetrie sur un vehicule IA.
   C'est le risque principal, et il est nomme : la couche physique consomme le dernier intent recu,
   donc un chemin qui ne soumet plus rien laisse courir le dernier intent.
2. **Poussee** — percuter une IA, puis la pousser franchement ; recommencer par poussee continue
   sans choc net. Attendu : deviation, mise en travers possible, sortie de chaussee possible, aucun
   realignement en une frame.
3. **Relief** — faire franchir a une IA le dos-d'ane puis la marche basse de 0,12 m de
   `Avenue_CenterToEast`. Attendu : elle suit le relief comme le joueur, ne decolle pas, ne poursuit
   pas son trace en l'air.
4. **Aides sur le trafic** — en virage et en appui, verifier qu'une aide de la 5.13 est perceptible
   sur une IA, puis remettre sa valeur authoree a zero et verifier qu'elle disparait.
5. **Non-regression de conduite** — trafic nominal du carrefour et des giratoires : tirage de
   virage, portails, point de visee, aucune IA a l'arret, aucune IA coupant l'interieur d'un
   giratoire plus qu'avant. Toute regression de nature differente est rapportee telle quelle, sans
   etre attribuee a cette story.
6. **Ce qui n'est pas juge ici** : la reaction apres un choc significatif. Elle n'est pas livree,
   son absence est attendue, et il ne faut pas la lire comme une regression.

Garde de double etat d'`AGENTS.md` : `MVP_Run.unity` et `git status --short` doivent revenir a leur
etat initial apres la recette.

## 6. Risques ouverts et decisions en attente

- **Dette des deux `MVP_RageTargetVehicle_*`** (diagnostic inacheve, Story 5.13) : ces instances ne
  repondent pas a l'accelerateur alors que l'intent est bien livre a `VehiclePhysicsBody`. Ce sont
  des instances d'un **autre prefab** que les trois IA, donc ce n'est pas un blocage demontre — mais
  c'est la meme couche, et c'est la raison pour laquelle la recette commence par « une IA roule ? ».
- **Commentaires perimes corriges (2026-09-19)** dans les deux fichiers que le spec declarait « a ne
  pas modifier » : `VehiclePhysicsBody.cs` (en-tete) et `Story510LaneGraphAndRoutedTrafficTests.cs`
  (commentaire du rejeu). Aucun test ne s'y accrochait ; la correction etait necessaire parce qu'un
  commentaire qui decrit un code disparu est un mensonge pour le prochain lecteur.
- **`ANO-5.10-03` reste sans story proprietaire** : son fichier porte toujours `story: 5.14`, alors
  que la 5.14 reduite ne le porte plus. `status: open` inchange. La reaffectation releve d'une
  correction de parcours, pas d'une retouche pendant un cycle de build.
